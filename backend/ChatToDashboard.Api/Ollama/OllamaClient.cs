using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Models;
using ChatToDashboard.Api.Sources;
using ChatToDashboard.Api.Usage;

namespace ChatToDashboard.Api.Ollama;

/// <summary>
/// Calls an internal Ollama deployment (via a company API gateway that proxies Ollama's
/// own wire format unchanged, per its Connection Guide) and drives the same tool-calling
/// loop as the Claude/OpenAI clients: the model requests list_files / query_data /
/// search_documents, we execute each call and append a "tool" message, until it returns
/// the dashboard JSON.
///
/// Differences from Ollama's native /api/chat that this client accounts for:
/// - The response is one top-level object, not wrapped in a "choices" array.
/// - message.tool_calls[].function.arguments arrives as a JSON object already —
///   never a stringified JSON blob the way OpenAI sends it.
/// - Token counts are top-level prompt_eval_count/eval_count, not a nested "usage" object
///   (see UsageTrace.RecordTurn, which already falls back to the response's top level).
/// - "stream" is NOT omittable — Ollama defaults to streaming, so every request pins it false.
/// </summary>
public class OllamaClient : IDashboardGenerator
{
    private const int MaxToolIterations = 15;
    private const int MaxJsonRepairAttempts = 3;

    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _defaultModel;
    private readonly LlmSettingsStore _settings;
    private readonly AnalyticsTools _tools;
    private readonly UsageTracker _usage;
    private readonly ILogger<OllamaClient> _logger;

    public OllamaClient(
        HttpClient http,
        IConfiguration configuration,
        LlmSettingsStore settings,
        AnalyticsTools tools,
        UsageTracker usage,
        ILogger<OllamaClient> logger)
    {
        _http = http;
        // Trimmed: a stray space or newline pasted with the token makes the gateway reject it.
        _apiKey = configuration["Ollama:ApiKey"]?.Trim() is { Length: > 0 } key
            ? key
            : throw new InvalidOperationException(
                "Ollama API token is not configured. " +
                "Set it with: dotnet user-secrets set \"Ollama:ApiKey\" \"<token>\" " +
                "(create one under \"My API Tokens\" in the gateway's own UI).");
        _defaultModel = configuration["Ollama:Model"] ?? "qwen3:14b";
        _settings = settings;
        _tools = tools;
        _usage = usage;
        _logger = logger;
    }

    public async Task<DashboardSpec> GenerateDashboardAsync(
        string question,
        IReadOnlyList<ChatTurn>? history = null,
        SourceSelection? sources = null,
        string? imageDataUrl = null,
        CancellationToken ct = default)
    {
        // Screenshot-to-dashboard needs a vision-capable model; the gateway's models list
        // (all plain chat/instruct models) doesn't advertise that, so it's not offered here —
        // the model would otherwise silently ignore the image.
        if (!string.IsNullOrWhiteSpace(imageDataUrl))
            throw new InvalidOperationException(
                "الموديل الداخلي الحالي لا يدعم تحليل الصور. بدّل مؤقتًا إلى Claude أو GPT من إعدادات الموديل لهذا الطلب.");

        var model = (await _settings.GetAsync(ct)).OllamaModel is { Length: > 0 } saved ? saved : _defaultModel;
        var context = await _tools.DescribeSourcesAsync(sources ?? SourceSelection.AllEnabled(), ct);
        var messages = new JsonArray
        {
            new JsonObject { ["role"] = "system", ["content"] = _tools.BuildSystemPrompt(context) },
        };

        foreach (var turn in history ?? Array.Empty<ChatTurn>())
        {
            if (string.IsNullOrWhiteSpace(turn.Text)) continue;
            if (turn.Role != "user" && turn.Role != "assistant") continue;
            messages.Add(new JsonObject { ["role"] = turn.Role, ["content"] = turn.Text.Trim() });
        }
        messages.Add(new JsonObject { ["role"] = "user", ["content"] = question });

        var tools = BuildToolsJson(context);
        var jsonRepairAttempts = 0;

        var trace = _usage.Begin("Ollama", model, question, DescribeSources(context));
        trace.SetSystemPrompt(_tools.BuildSystemPrompt(context));
        try
        {
        for (var iteration = 0; iteration < MaxToolIterations; iteration++)
        {
            ct.ThrowIfCancellationRequested();
            var response = await CallChatAsync(model, messages, tools, trace, ct);

            var message = response["message"]?.AsObject()
                ?? throw new InvalidOperationException("Ollama gateway response had no 'message'.");
            var doneReason = response["done_reason"]?.GetValue<string>();

            // Echo the assistant turn back verbatim on the next request.
            messages.Add(message.DeepClone());

            var toolCalls = message["tool_calls"]?.AsArray();
            if (toolCalls is { Count: > 0 })
            {
                foreach (var call in toolCalls)
                {
                    var function = call?["function"]?.AsObject();
                    if (function is null) continue;
                    var toolName = function["name"]?.GetValue<string>() ?? "";

                    // Ollama sends arguments as a JSON object already (unlike OpenAI's
                    // stringified JSON) — but read defensively in case a proxy/version differs.
                    var argumentsNode = function["arguments"];
                    JsonObject arguments = argumentsNode switch
                    {
                        JsonObject obj => obj,
                        JsonValue val when val.TryGetValue<string>(out var raw) && !string.IsNullOrWhiteSpace(raw)
                            => TryParseArguments(raw),
                        _ => new JsonObject(),
                    };

                    var toolClock = System.Diagnostics.Stopwatch.StartNew();
                    var (result, isError) = await _tools.ExecuteToolAsync(toolName, arguments, context, ct);
                    trace.RecordToolCall(toolName, arguments.ToJsonString(), result, isError, toolClock.ElapsedMilliseconds);
                    messages.Add(ToolResultMessage(toolName, result, isError));
                }
                continue;
            }

            if (doneReason == "length")
            {
                jsonRepairAttempts++;
                if (jsonRepairAttempts >= MaxJsonRepairAttempts)
                    throw new InvalidOperationException(
                        "The model's response was repeatedly truncated. Try a model with a larger context window.");
                messages.Add(new JsonObject
                {
                    ["role"] = "user",
                    ["content"] =
                        "Your response was cut off because it exceeded the token limit. Respond again with the " +
                        "complete dashboard JSON only, using fewer/smaller widgets (aggregate the data further).",
                });
                continue;
            }

            var text = message["content"]?.GetValue<string>() ?? string.Empty;
            var (dashboard, parseError) = AnalyticsTools.TryParseDashboard(text);
            if (dashboard is not null)
            {
                await trace.CompleteAsync(true, text, null, ct);
                return dashboard;
            }

            jsonRepairAttempts++;
            _logger.LogWarning("Dashboard JSON invalid (attempt {Attempt}): {Error}", jsonRepairAttempts, parseError);
            if (jsonRepairAttempts >= MaxJsonRepairAttempts)
                throw new InvalidOperationException(
                    $"The model did not return valid dashboard JSON after {MaxJsonRepairAttempts} attempts. Last error: {parseError}");

            messages.Add(new JsonObject
            {
                ["role"] = "user",
                ["content"] =
                    $"Your previous response was not valid dashboard JSON. Error: {parseError}\n" +
                    "Respond again with ONLY a single JSON object matching the required schema — " +
                    "no markdown fences, no explanation, no text outside the JSON.",
            });
        }

        throw new InvalidOperationException(
            $"Tool-calling loop did not converge within {MaxToolIterations} iterations.");
        }
        catch (Exception ex)
        {
            await trace.CompleteAsync(false, null, ex.Message, CancellationToken.None);
            throw;
        }
    }

    private static JsonObject TryParseArguments(string raw)
    {
        try { return JsonNode.Parse(raw)?.AsObject() ?? new JsonObject(); }
        catch (JsonException) { return new JsonObject(); }
    }

    /// <summary>A short, readable note of which sources were on for this question.</summary>
    private static string DescribeSources(AnalyticsTools.SourceContext context) =>
        $"أنظمة: {(context.EnabledSystems.Count == 0 ? "(لا يوجد)" : string.Join("، ", context.EnabledSystems))} | " +
        $"تصنيفات: {(context.EnabledCategories.Count == 0 ? "(لا يوجد)" : string.Join("، ", context.EnabledCategories))}";

    private static JsonObject ToolResultMessage(string toolName, string content, bool isError = false) =>
        new()
        {
            ["role"] = "tool",
            ["tool_name"] = toolName,
            ["content"] = isError ? $"ERROR: {content}" : content,
        };

    private JsonArray BuildToolsJson(AnalyticsTools.SourceContext context)
    {
        var tools = new JsonArray();
        foreach (var tool in _tools.BuildTools(context))
        {
            tools.Add(new JsonObject
            {
                ["type"] = "function",
                ["function"] = new JsonObject
                {
                    ["name"] = tool.Name,
                    ["description"] = tool.Description,
                    ["parameters"] = tool.InputSchema.DeepClone(),
                },
            });
        }
        return tools;
    }

    private async Task<JsonObject> CallChatAsync(
        string model, JsonArray messages, JsonArray tools, UsageTrace trace, CancellationToken ct)
    {
        var body = new JsonObject
        {
            ["model"] = model,
            ["messages"] = messages.DeepClone(),
            ["tools"] = tools.DeepClone(),
            ["stream"] = false, // Ollama streams by default; the tool-loop needs one complete reply.
        };

        var requestBody = body.ToJsonString();
        var clock = System.Diagnostics.Stopwatch.StartNew();
        using var request = new HttpRequestMessage(HttpMethod.Post, "chat")
        {
            Content = new StringContent(requestBody, Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");

        using var response = await _http.SendAsync(request, ct);
        var responseText = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Ollama gateway returned {(int)response.StatusCode}: {responseText}");

        var parsed = JsonNode.Parse(responseText)?.AsObject()
            ?? throw new InvalidOperationException("Ollama gateway returned an empty response body.");
        trace.RecordTurn(requestBody, responseText, parsed, clock.ElapsedMilliseconds);
        return parsed;
    }
}
