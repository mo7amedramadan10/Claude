using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Models;
using ChatToDashboard.Api.Sources;
using ChatToDashboard.Api.Usage;

namespace ChatToDashboard.Api.Claude;

/// <summary>
/// Calls the Anthropic Messages API over plain HTTP and drives the tool-use loop:
/// Claude asks for list_files / query_data / search_documents, we execute the tool
/// and send back tool_result blocks, until Claude returns the final dashboard JSON.
/// </summary>
public class ClaudeClient : IDashboardGenerator
{
    private const int MaxToolIterations = 15;
    private const int MaxJsonRepairAttempts = 3;

    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly int _maxTokens;
    private readonly AnalyticsTools _tools;
    private readonly UsageTracker _usage;
    private readonly ILogger<ClaudeClient> _logger;

    public ClaudeClient(
        HttpClient http,
        IConfiguration configuration,
        AnalyticsTools tools,
        UsageTracker usage,
        ILogger<ClaudeClient> logger)
    {
        _http = http;
        // Trimmed: a stray space or newline pasted with the key makes the API reject it as invalid.
        _apiKey = configuration["Anthropic:ApiKey"]?.Trim() is { Length: > 0 } key
            ? key
            : throw new InvalidOperationException(
                "Anthropic API key is not configured. " +
                "Set it with: dotnet user-secrets set \"Anthropic:ApiKey\" \"<key>\"");
        _model = configuration["Anthropic:Model"] ?? "claude-sonnet-5";
        _maxTokens = configuration.GetValue("Anthropic:MaxTokens", 16000);
        _tools = tools;
        _usage = usage;
        _logger = logger;
    }

    public async Task<DashboardSpec> GenerateDashboardAsync(
        string question,
        IReadOnlyList<ChatTurn>? history = null,
        SourceSelection? sources = null,
        CancellationToken ct = default)
    {
        var messages = new JsonArray();

        // Replay prior Q&A summaries (text only) so follow-up questions have context.
        // Tool calls from earlier turns are not replayed; Claude re-queries as needed.
        void AppendTextTurn(string role, string text)
        {
            // The Messages API requires alternating roles; merge consecutive same-role turns.
            if (messages.Count > 0 && messages[^1]!["role"]!.GetValue<string>() == role)
            {
                var previous = messages[^1]!.AsObject();
                previous["content"] = previous["content"]!.GetValue<string>() + "\n\n" + text;
                return;
            }
            messages.Add(new JsonObject { ["role"] = role, ["content"] = text });
        }

        foreach (var turn in history ?? Array.Empty<ChatTurn>())
        {
            if (string.IsNullOrWhiteSpace(turn.Text)) continue;
            if (turn.Role != "user" && turn.Role != "assistant") continue;
            AppendTextTurn(turn.Role, turn.Text.Trim());
        }

        AppendTextTurn("user", question);

        // A conversation must start with a user turn; drop a leading assistant turn.
        while (messages.Count > 0 && messages[0]!["role"]!.GetValue<string>() == "assistant")
            messages.RemoveAt(0);

        var context = await _tools.DescribeSourcesAsync(sources ?? SourceSelection.AllEnabled(), ct);
        var tools = BuildToolsJson(context);
        var jsonRepairAttempts = 0;

        var trace = _usage.Begin("Anthropic", _model, question, DescribeSources(context));
        trace.SetSystemPrompt(_tools.BuildSystemPrompt(context));
        try
        {
        for (var iteration = 0; iteration < MaxToolIterations; iteration++)
        {
            ct.ThrowIfCancellationRequested();
            var response = await CallMessagesApiAsync(messages, tools, context, trace, ct);

            var stopReason = response["stop_reason"]?.GetValue<string>();
            var content = response["content"]?.AsArray()
                ?? throw new InvalidOperationException("Anthropic API response had no content array.");

            // Echo the assistant turn back verbatim on the next request.
            messages.Add(new JsonObject
            {
                ["role"] = "assistant",
                ["content"] = content.DeepClone(),
            });

            if (stopReason == "tool_use")
            {
                var toolResults = new JsonArray();
                foreach (var block in content)
                {
                    if (block?["type"]?.GetValue<string>() != "tool_use") continue;
                    var toolUseId = block["id"]!.GetValue<string>();
                    var toolName = block["name"]!.GetValue<string>();
                    var input = block["input"]?.AsObject() ?? new JsonObject();

                    var toolClock = System.Diagnostics.Stopwatch.StartNew();
                    var (result, isError) = await _tools.ExecuteToolAsync(toolName, input, context, ct);
                    trace.RecordToolCall(toolName, input.ToJsonString(), result, isError, toolClock.ElapsedMilliseconds);
                    var resultBlock = new JsonObject
                    {
                        ["type"] = "tool_result",
                        ["tool_use_id"] = toolUseId,
                        ["content"] = result,
                    };
                    if (isError) resultBlock["is_error"] = true;
                    toolResults.Add(resultBlock);
                }

                messages.Add(new JsonObject { ["role"] = "user", ["content"] = toolResults });
                continue;
            }

            if (stopReason == "max_tokens")
            {
                jsonRepairAttempts++;
                if (jsonRepairAttempts >= MaxJsonRepairAttempts)
                    throw new InvalidOperationException(
                        "Claude's response was repeatedly truncated (max_tokens). Increase Anthropic:MaxTokens.");
                messages.Add(new JsonObject
                {
                    ["role"] = "user",
                    ["content"] =
                        "Your response was cut off because it exceeded the token limit. Respond again with the " +
                        "complete dashboard JSON only, using fewer/smaller widgets (aggregate the data further).",
                });
                continue;
            }

            // Final (non-tool) turn: expect the dashboard JSON.
            var text = string.Concat(content
                .Where(b => b?["type"]?.GetValue<string>() == "text")
                .Select(b => b!["text"]!.GetValue<string>()));

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
                    $"Claude did not return valid dashboard JSON after {MaxJsonRepairAttempts} attempts. Last error: {parseError}");

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
            $"Tool-use loop did not converge within {MaxToolIterations} iterations.");
        }
        catch (Exception ex)
        {
            await trace.CompleteAsync(false, null, ex.Message, CancellationToken.None);
            throw;
        }
    }

    /// <summary>A short, readable note of which sources were on for this question.</summary>
    private static string DescribeSources(AnalyticsTools.SourceContext context) =>
        $"أنظمة: {(context.EnabledSystems.Count == 0 ? "(لا يوجد)" : string.Join("، ", context.EnabledSystems))} | " +
        $"تصنيفات: {(context.EnabledCategories.Count == 0 ? "(لا يوجد)" : string.Join("، ", context.EnabledCategories))}";

    private JsonArray BuildToolsJson(AnalyticsTools.SourceContext context)
    {
        var tools = new JsonArray();
        foreach (var tool in _tools.BuildTools(context))
        {
            tools.Add(new JsonObject
            {
                ["name"] = tool.Name,
                ["description"] = tool.Description,
                ["input_schema"] = tool.InputSchema.DeepClone(),
            });
        }
        return tools;
    }

    private async Task<JsonObject> CallMessagesApiAsync(
        JsonArray messages, JsonArray tools, AnalyticsTools.SourceContext context,
        UsageTrace trace, CancellationToken ct)
    {
        // Prompt caching: one breakpoint after the stable prefix (tools + system), and one
        // on the last message so each tool-loop iteration reuses the growing conversation
        // prefix instead of re-processing it.
        var body = new JsonObject
        {
            ["model"] = _model,
            ["max_tokens"] = _maxTokens,
            ["system"] = new JsonArray
            {
                new JsonObject
                {
                    ["type"] = "text",
                    ["text"] = _tools.BuildSystemPrompt(context),
                    ["cache_control"] = new JsonObject { ["type"] = "ephemeral" },
                },
            },
            ["tools"] = tools.DeepClone(),
            ["messages"] = messages.DeepClone(),
        };
        MarkLastMessageCacheable(body["messages"]!.AsArray());

        var requestBody = body.ToJsonString();
        var clock = System.Diagnostics.Stopwatch.StartNew();
        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/messages")
        {
            Content = new StringContent(requestBody, Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("x-api-key", _apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");

        using var response = await _http.SendAsync(request, ct);
        var responseText = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Anthropic API returned {(int)response.StatusCode}: {responseText}");

        var parsed = JsonNode.Parse(responseText)?.AsObject()
            ?? throw new InvalidOperationException("Anthropic API returned an empty response body.");
        trace.RecordTurn(requestBody, responseText, parsed, clock.ElapsedMilliseconds);
        return parsed;
    }

    /// <summary>
    /// Puts a cache_control breakpoint on the last content block of the last message
    /// (normalizing string content to a block array first).
    /// </summary>
    private static void MarkLastMessageCacheable(JsonArray messages)
    {
        if (messages.Count == 0) return;
        var last = messages[^1]!.AsObject();
        var content = last["content"]!;

        if (content.GetValueKind() == JsonValueKind.String)
        {
            last["content"] = new JsonArray(new JsonObject
            {
                ["type"] = "text",
                ["text"] = content.GetValue<string>(),
                ["cache_control"] = new JsonObject { ["type"] = "ephemeral" },
            });
            return;
        }

        var blocks = content.AsArray();
        if (blocks.Count > 0)
            blocks[^1]!.AsObject()["cache_control"] = new JsonObject { ["type"] = "ephemeral" };
    }
}
