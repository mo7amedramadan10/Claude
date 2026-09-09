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
public class OllamaClient : IDashboardGenerator, IDocumentTextExtractor
{
    private const int MaxToolIterations = 15;
    private const int MaxJsonRepairAttempts = 3;
    // A weaker/local model can keep calling tools indefinitely instead of ever concluding —
    // it never gets a "you're out of budget" signal otherwise, since nothing about the
    // conversation itself changes turn to turn. From this iteration on, no tool definitions
    // are sent at all (see the loop below), so the model *cannot* call one and must answer in
    // plain text — reusing the existing JSON-repair retries below as a safety net if that
    // first forced answer isn't valid JSON. Leaves enough headroom for MaxJsonRepairAttempts
    // retries to still fit before MaxToolIterations is reached.
    private const int ForceFinalAnswerAtIteration = MaxToolIterations - MaxJsonRepairAttempts - 1;

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
        DashboardStateInput? currentDashboard = null,
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
        var systemPrompt = _tools.BuildSystemPrompt(context);
        var messages = new JsonArray
        {
            new JsonObject { ["role"] = "system", ["content"] = systemPrompt },
        };

        // The dashboard currently on screen (when this is a continuation, not a fresh start —
        // see AnalyticsTools.ComposeUserMessage) is framed as part of this single user turn;
        // there is no separate multi-turn history to replay.
        messages.Add(new JsonObject { ["role"] = "user", ["content"] = AnalyticsTools.ComposeUserMessage(question, currentDashboard) });

        var tools = BuildToolsJson(context);
        var trace = _usage.Begin("Ollama", model, question, DescribeSources(context));
        trace.SetSystemPrompt(systemPrompt);
        return await RunLoopAsync(
            model, messages, tools, context, trace,
            AnalyticsTools.TryParseDashboard, "dashboard", ct);
    }

    public async Task<InquiryResponse> GenerateInquiryAsync(
        string question, SourceSelection? sources = null, CancellationToken ct = default)
    {
        var model = (await _settings.GetAsync(ct)).OllamaModel is { Length: > 0 } saved ? saved : _defaultModel;
        var context = await _tools.DescribeSourcesAsync(sources ?? SourceSelection.AllEnabled(), ct);
        var systemPrompt = _tools.BuildInquirySystemPrompt(context);
        // Inquiries is never continuation-aware — isolated from whatever dashboard is on
        // screen (see the sub-tab isolation rule) — so this is always a single fresh turn.
        var messages = new JsonArray
        {
            new JsonObject { ["role"] = "system", ["content"] = systemPrompt },
            new JsonObject { ["role"] = "user", ["content"] = question },
        };

        var tools = BuildToolsJson(context);
        var trace = _usage.Begin("Ollama", model, question, DescribeSources(context));
        trace.SetSystemPrompt(systemPrompt);
        return await RunLoopAsync(
            model, messages, tools, context, trace,
            AnalyticsTools.TryParseInquiry, "inquiry", ct);
    }

    public async Task<DashboardSpec> GenerateDashboardFromInquiryAsync(
        InquiryResponse inquiry, DashboardStateInput? currentDashboard = null, SourceSelection? sources = null, CancellationToken ct = default)
    {
        // "➕ أضف إلى لوحة المتابعة": a pure restructuring call — the data is already real and
        // already fetched, so no tools are offered at all (tools: null below), guaranteeing
        // no new query_data/list_files call can happen here.
        var model = (await _settings.GetAsync(ct)).OllamaModel is { Length: > 0 } saved ? saved : _defaultModel;
        var context = await _tools.DescribeSourcesAsync(sources ?? SourceSelection.AllEnabled(), ct);
        var systemPrompt = _tools.BuildSystemPrompt(context);
        var messages = new JsonArray
        {
            new JsonObject { ["role"] = "system", ["content"] = systemPrompt },
            new JsonObject { ["role"] = "user", ["content"] = AnalyticsTools.ComposeConversionUserMessage(inquiry, currentDashboard) },
        };

        var trace = _usage.Begin("Ollama", model, "🔄 تحويل استفسار إلى لوحة: " + inquiry.Answer, DescribeSources(context));
        trace.SetSystemPrompt(systemPrompt);
        return await RunLoopAsync(
            model, messages, null, context, trace,
            AnalyticsTools.TryParseDashboard, "dashboard", ct);
    }

    /// <summary>See ClaudeClient.ExtractDocumentTextAsync — but the internal gateway's models
    /// are all plain chat/instruct models with no vision support (see the same rejection in
    /// GenerateDashboardAsync above), so this can never actually read a page image. Throws
    /// immediately rather than sending a request the model would just ignore the images on;
    /// DocumentReaderRouter's caller (the repository upload flow) catches this and falls back
    /// to PdfPig's plain-text extraction, same as any other extraction failure.</summary>
    public Task<string> ExtractDocumentTextAsync(
        string fileName, IReadOnlyList<string> pageImageDataUrls, CancellationToken ct = default) =>
        throw new InvalidOperationException(
            "الموديل الداخلي الحالي لا يدعم تحليل الصور، فمش هيقدر يقرأ صفحات المستند. " +
            "بدّل موديل قراءة المستندات لـ Claude أو GPT من الإعدادات.");

    private static JsonObject TryParseArguments(string raw)
    {
        try { return JsonNode.Parse(raw)?.AsObject() ?? new JsonObject(); }
        catch (JsonException) { return new JsonObject(); }
    }

    /// <summary>A short, readable note of which sources were on for this question.</summary>
    private static string DescribeSources(AnalyticsTools.SourceContext context) =>
        $"أنظمة: {(context.EnabledSystems.Count == 0 ? "(لا يوجد)" : string.Join("، ", context.EnabledSystems))} | " +
        $"ملفات: {(context.EnabledFiles.Count == 0 ? "(لا يوجد)" : string.Join("، ", context.EnabledFiles))}";

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

    /// <summary>
    /// The tool-calling loop shared by every mode above: call the API, execute any requested
    /// tool and loop, or parse+validate the final text via <paramref name="tryParse"/> —
    /// retrying (a "your JSON was invalid, try again" turn) up to <see
    /// cref="MaxJsonRepairAttempts"/> times. <paramref name="tools"/> null/empty means no
    /// tool is ever offered, so the very first turn is already necessarily final (used by
    /// the tool-free conversion call).
    /// </summary>
    private async Task<T> RunLoopAsync<T>(
        string model, JsonArray messages, JsonArray? tools, AnalyticsTools.SourceContext context,
        UsageTrace trace, Func<string, (T? Result, string? Error)> tryParse, string kindLabel, CancellationToken ct)
        where T : class
    {
        var jsonRepairAttempts = 0;
        var forcedFinalAnswerNoticeSent = false;
        try
        {
            for (var iteration = 0; iteration < MaxToolIterations; iteration++)
            {
                ct.ThrowIfCancellationRequested();
                var forceFinalAnswer = tools is not { Count: > 0 } || iteration >= ForceFinalAnswerAtIteration;
                if (forceFinalAnswer && tools is { Count: > 0 } && !forcedFinalAnswerNoticeSent)
                {
                    forcedFinalAnswerNoticeSent = true;
                    messages.Add(new JsonObject
                    {
                        ["role"] = "user",
                        ["content"] =
                            "لقد استدعيت عددًا كافيًا من الأدوات بالفعل. لا تنادِ أي أداة أخرى — " +
                            "استخدم فقط النتائج التي جمعتها حتى الآن، وأجب فورًا بكائن JSON النهائي " +
                            "مطابقًا للمخطط المطلوب، من غير أي نداء أدوات إضافي.",
                    });
                }
                var response = await CallChatAsync(model, messages, forceFinalAnswer ? null : tools, trace, ct);

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
                            "complete JSON only, using fewer/smaller widgets (aggregate the data further).",
                    });
                    continue;
                }

                var text = message["content"]?.GetValue<string>() ?? string.Empty;
                var (parsed, parseError) = tryParse(text);
                if (parsed is not null)
                {
                    await trace.CompleteAsync(true, text, null, ct);
                    return parsed;
                }

                jsonRepairAttempts++;
                _logger.LogWarning("{Kind} JSON invalid (attempt {Attempt}): {Error}", kindLabel, jsonRepairAttempts, parseError);
                if (jsonRepairAttempts >= MaxJsonRepairAttempts)
                    throw new InvalidOperationException(
                        $"The model did not return valid {kindLabel} JSON after {MaxJsonRepairAttempts} attempts. Last error: {parseError}");

                messages.Add(new JsonObject
                {
                    ["role"] = "user",
                    ["content"] =
                        $"Your previous response was not valid JSON. Error: {parseError}\n" +
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

    private async Task<JsonObject> CallChatAsync(
        string model, JsonArray messages, JsonArray? tools, UsageTrace trace, CancellationToken ct)
    {
        var body = new JsonObject
        {
            ["model"] = model,
            ["messages"] = messages.DeepClone(),
            ["stream"] = false, // Ollama streams by default; the tool-loop needs one complete reply.
        };
        // Omit "tools" entirely (rather than send "[]") when forcing a tools-less final
        // answer — matches the other clients and avoids relying on how this gateway treats
        // an empty array specifically.
        if (tools is { Count: > 0 })
            body["tools"] = tools.DeepClone();

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
