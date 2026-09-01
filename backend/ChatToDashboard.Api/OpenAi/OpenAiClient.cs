using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Models;

namespace ChatToDashboard.Api.OpenAi;

/// <summary>
/// Calls the OpenAI Chat Completions API over plain HTTP and drives the same tool-calling
/// loop as the Claude client: the model requests list_files / query_data / search_documents,
/// we execute each call and append a "tool" message, until it returns the dashboard JSON.
/// </summary>
public class OpenAiClient : IDashboardGenerator
{
    private const int MaxToolIterations = 15;
    private const int MaxJsonRepairAttempts = 3;

    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly AnalyticsTools _tools;
    private readonly ILogger<OpenAiClient> _logger;

    public OpenAiClient(
        HttpClient http,
        IConfiguration configuration,
        AnalyticsTools tools,
        ILogger<OpenAiClient> logger)
    {
        _http = http;
        // Trimmed: a stray space or newline pasted with the key makes the API reject it as invalid.
        _apiKey = configuration["OpenAI:ApiKey"]?.Trim() is { Length: > 0 } key
            ? key
            : throw new InvalidOperationException(
                "OpenAI API key is not configured. " +
                "Set it with: dotnet user-secrets set \"OpenAI:ApiKey\" \"<key>\"");
        _model = configuration["OpenAI:Model"] ?? "gpt-4o";
        _tools = tools;
        _logger = logger;
    }

    public async Task<DashboardSpec> GenerateDashboardAsync(
        string question, IReadOnlyList<ChatTurn>? history = null, CancellationToken ct = default)
    {
        var messages = new JsonArray
        {
            new JsonObject { ["role"] = "system", ["content"] = _tools.BuildSystemPrompt() },
        };

        foreach (var turn in history ?? Array.Empty<ChatTurn>())
        {
            if (string.IsNullOrWhiteSpace(turn.Text)) continue;
            if (turn.Role != "user" && turn.Role != "assistant") continue;
            messages.Add(new JsonObject { ["role"] = turn.Role, ["content"] = turn.Text.Trim() });
        }

        messages.Add(new JsonObject { ["role"] = "user", ["content"] = question });

        var tools = BuildToolsJson();
        var jsonRepairAttempts = 0;

        for (var iteration = 0; iteration < MaxToolIterations; iteration++)
        {
            ct.ThrowIfCancellationRequested();
            var response = await CallChatCompletionsAsync(messages, tools, ct);

            var choice = response["choices"]?.AsArray().FirstOrDefault()?.AsObject()
                ?? throw new InvalidOperationException("OpenAI API response had no choices.");
            var message = choice["message"]!.AsObject();
            var finishReason = choice["finish_reason"]?.GetValue<string>();

            // Echo the assistant turn back verbatim on the next request.
            messages.Add(message.DeepClone());

            var toolCalls = message["tool_calls"]?.AsArray();
            if (toolCalls is { Count: > 0 })
            {
                foreach (var call in toolCalls)
                {
                    var callObject = call!.AsObject();
                    var callId = callObject["id"]!.GetValue<string>();
                    var function = callObject["function"]!.AsObject();
                    var toolName = function["name"]!.GetValue<string>();

                    // Arguments arrive as a JSON string; a model can emit malformed JSON here.
                    JsonObject arguments;
                    try
                    {
                        var raw = function["arguments"]?.GetValue<string>();
                        arguments = string.IsNullOrWhiteSpace(raw)
                            ? new JsonObject()
                            : JsonNode.Parse(raw)?.AsObject() ?? new JsonObject();
                    }
                    catch (JsonException ex)
                    {
                        messages.Add(ToolResultMessage(callId, $"Error: arguments were not valid JSON ({ex.Message})."));
                        continue;
                    }

                    var (result, isError) = await _tools.ExecuteToolAsync(toolName, arguments, ct);
                    messages.Add(ToolResultMessage(callId, result, isError));
                }
                continue;
            }

            if (finishReason == "length")
            {
                jsonRepairAttempts++;
                if (jsonRepairAttempts >= MaxJsonRepairAttempts)
                    throw new InvalidOperationException(
                        "The model's response was repeatedly truncated. Increase OpenAI:MaxTokens.");
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
            if (dashboard is not null) return dashboard;

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

    private static JsonObject ToolResultMessage(string callId, string content, bool isError = false) =>
        new()
        {
            ["role"] = "tool",
            ["tool_call_id"] = callId,
            // OpenAI has no is_error flag on tool messages; label failures so the model notices.
            ["content"] = isError ? $"ERROR: {content}" : content,
        };

    private JsonArray BuildToolsJson()
    {
        var tools = new JsonArray();
        foreach (var tool in _tools.BuildTools())
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

    private async Task<JsonObject> CallChatCompletionsAsync(
        JsonArray messages, JsonArray tools, CancellationToken ct)
    {
        var body = new JsonObject
        {
            ["model"] = _model,
            ["messages"] = messages.DeepClone(),
            ["tools"] = tools.DeepClone(),
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions")
        {
            Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");

        using var response = await _http.SendAsync(request, ct);
        var responseText = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"OpenAI API returned {(int)response.StatusCode}: {responseText}");

        return JsonNode.Parse(responseText)?.AsObject()
            ?? throw new InvalidOperationException("OpenAI API returned an empty response body.");
    }
}
