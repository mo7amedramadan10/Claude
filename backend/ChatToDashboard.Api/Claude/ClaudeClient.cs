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
        DashboardStateInput? currentDashboard = null,
        SourceSelection? sources = null,
        string? imageDataUrl = null,
        CancellationToken ct = default)
    {
        // The dashboard currently on screen (when this is a continuation, not a fresh start —
        // see AnalyticsTools.ComposeUserMessage) is framed as part of this single user turn,
        // so there is no separate multi-turn history to replay; a fresh request starts with
        // exactly this one turn.
        var userText = AnalyticsTools.ComposeUserMessage(question, currentDashboard);
        var messages = new JsonArray { new JsonObject { ["role"] = "user", ["content"] = userText } };

        // A reference image, if attached, rides along on this question's turn only.
        if (TryParseDataUrl(imageDataUrl, out var mediaType, out var base64Data))
        {
            var last = messages[^1]!.AsObject();
            var text = last["content"]!.GetValue<string>();
            last["content"] = new JsonArray(
                new JsonObject
                {
                    ["type"] = "image",
                    ["source"] = new JsonObject { ["type"] = "base64", ["media_type"] = mediaType, ["data"] = base64Data },
                },
                new JsonObject { ["type"] = "text", ["text"] = text });
        }

        var context = await _tools.DescribeSourcesAsync(sources ?? SourceSelection.AllEnabled(), ct);
        var systemPrompt = _tools.BuildSystemPrompt(context);
        var tools = BuildToolsJson(context);

        var trace = _usage.Begin("Anthropic", _model, question, DescribeSources(context));
        trace.SetSystemPrompt(systemPrompt);
        return await RunLoopAsync(
            messages, systemPrompt, tools, context, trace,
            AnalyticsTools.TryParseDashboard, "dashboard", ct);
    }

    public async Task<InquiryResponse> GenerateInquiryAsync(
        string question, SourceSelection? sources = null, CancellationToken ct = default)
    {
        // Inquiries is never continuation-aware — isolated from whatever dashboard is on
        // screen (see the sub-tab isolation rule) — so this is always a single fresh turn.
        var messages = new JsonArray { new JsonObject { ["role"] = "user", ["content"] = question } };

        var context = await _tools.DescribeSourcesAsync(sources ?? SourceSelection.AllEnabled(), ct);
        var systemPrompt = _tools.BuildInquirySystemPrompt(context);
        var tools = BuildToolsJson(context);

        var trace = _usage.Begin("Anthropic", _model, question, DescribeSources(context));
        trace.SetSystemPrompt(systemPrompt);
        return await RunLoopAsync(
            messages, systemPrompt, tools, context, trace,
            AnalyticsTools.TryParseInquiry, "inquiry", ct);
    }

    public async Task<DashboardSpec> GenerateDashboardFromInquiryAsync(
        InquiryResponse inquiry, SourceSelection? sources = null, CancellationToken ct = default)
    {
        // "🔄 حوّله لداشبورد": a pure restructuring call — the data is already real and
        // already fetched, so no tools are offered at all (tools: null below), guaranteeing
        // no new query_data/list_files call can happen here.
        var userText = AnalyticsTools.ComposeConversionUserMessage(inquiry);
        var messages = new JsonArray { new JsonObject { ["role"] = "user", ["content"] = userText } };

        var context = await _tools.DescribeSourcesAsync(sources ?? SourceSelection.AllEnabled(), ct);
        var systemPrompt = _tools.BuildSystemPrompt(context);

        var trace = _usage.Begin("Anthropic", _model, "🔄 تحويل استفسار إلى لوحة: " + inquiry.Answer, DescribeSources(context));
        trace.SetSystemPrompt(systemPrompt);
        return await RunLoopAsync(
            messages, systemPrompt, null, context, trace,
            AnalyticsTools.TryParseDashboard, "dashboard", ct);
    }

    /// <summary>
    /// The tool-use loop shared by every mode above: call the API, execute any requested
    /// tool and loop, or parse+validate the final text via <paramref name="tryParse"/> —
    /// retrying (a "your JSON was invalid, try again" turn) up to <see
    /// cref="MaxJsonRepairAttempts"/> times. <paramref name="tools"/> null/empty means no
    /// tool is ever offered, so the very first turn is already necessarily final (used by
    /// the tool-free conversion call).
    /// </summary>
    private async Task<T> RunLoopAsync<T>(
        JsonArray messages, string systemPrompt, JsonArray? tools, AnalyticsTools.SourceContext context,
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
                var response = await CallMessagesApiAsync(
                    messages, systemPrompt, forceFinalAnswer ? null : tools, trace, ct);

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
                            "complete JSON only, using fewer/smaller widgets (aggregate the data further).",
                    });
                    continue;
                }

                // Final (non-tool) turn: expect the JSON this mode's caller asked for.
                var text = string.Concat(content
                    .Where(b => b?["type"]?.GetValue<string>() == "text")
                    .Select(b => b!["text"]!.GetValue<string>()));

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
                        $"Claude did not return valid {kindLabel} JSON after {MaxJsonRepairAttempts} attempts. Last error: {parseError}");

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
                $"Tool-use loop did not converge within {MaxToolIterations} iterations.");
        }
        catch (Exception ex)
        {
            await trace.CompleteAsync(false, null, ex.Message, CancellationToken.None);
            throw;
        }
    }

    /// <summary>Splits a "data:&lt;mime&gt;;base64,&lt;data&gt;" URL into its two parts.</summary>
    private static bool TryParseDataUrl(string? dataUrl, out string mediaType, out string base64Data)
    {
        mediaType = ""; base64Data = "";
        if (string.IsNullOrWhiteSpace(dataUrl)) return false;
        var match = System.Text.RegularExpressions.Regex.Match(dataUrl, @"^data:([\w/.+-]+);base64,(.+)$", System.Text.RegularExpressions.RegexOptions.Singleline);
        if (!match.Success) return false;
        mediaType = match.Groups[1].Value;
        base64Data = match.Groups[2].Value;
        return true;
    }

    /// <summary>A short, readable note of which sources were on for this question.</summary>
    private static string DescribeSources(AnalyticsTools.SourceContext context) =>
        $"أنظمة: {(context.EnabledSystems.Count == 0 ? "(لا يوجد)" : string.Join("، ", context.EnabledSystems))} | " +
        $"ملفات: {(context.EnabledFiles.Count == 0 ? "(لا يوجد)" : string.Join("، ", context.EnabledFiles))}";

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
        JsonArray messages, string systemPrompt, JsonArray? tools, UsageTrace trace, CancellationToken ct)
    {
        // Prompt caching: one breakpoint after the stable prefix (tools + system), and one
        // on the last message so each tool-loop iteration reuses the growing conversation
        // prefix instead of re-processing it. Forcing a tools-less final answer (tools null)
        // changes that prefix and so loses the cache hit for this one call only — an accepted
        // cost, since it only happens on the last few iterations of an already-degenerate loop.
        var body = new JsonObject
        {
            ["model"] = _model,
            ["max_tokens"] = _maxTokens,
            ["system"] = new JsonArray
            {
                new JsonObject
                {
                    ["type"] = "text",
                    ["text"] = systemPrompt,
                    ["cache_control"] = new JsonObject { ["type"] = "ephemeral" },
                },
            },
            ["messages"] = messages.DeepClone(),
        };
        if (tools is { Count: > 0 })
            body["tools"] = tools.DeepClone();
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
