using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Data.Common;
using ChatToDashboard.Api.Data;
using ChatToDashboard.Api.Models;
using Dapper;

namespace ChatToDashboard.Api.Claude;

/// <summary>
/// Calls the Anthropic Messages API over plain HTTP and drives the tool-use loop:
/// Claude asks for list_files / query_data / search_documents, we execute the tool
/// and send back tool_result blocks, until Claude returns the final dashboard JSON.
/// </summary>
public class ClaudeClient
{
    private const int MaxToolIterations = 15;
    private const int MaxJsonRepairAttempts = 3;
    private const int MaxRowsReturned = 500;

    private static readonly Regex ForbiddenSqlKeywords = new(
        @"\b(INSERT|UPDATE|DELETE|DROP|ALTER|CREATE|TRUNCATE|MERGE|EXEC|EXECUTE|GRANT|REVOKE|BACKUP|RESTORE|USE|KILL|SHUTDOWN|PRAGMA|ATTACH|DETACH|VACUUM|REINDEX|REPLACE)\b|(\bsp_\w+)|(\bxp_\w+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly int _maxTokens;
    private readonly DataFolderLoader _loader;
    private readonly DataStore _db;
    private readonly DocumentSearchService _documents;
    private readonly ILogger<ClaudeClient> _logger;

    public ClaudeClient(
        HttpClient http,
        IConfiguration configuration,
        DataFolderLoader loader,
        DataStore db,
        DocumentSearchService documents,
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
        _loader = loader;
        _db = db;
        _documents = documents;
        _logger = logger;
    }

    public async Task<DashboardSpec> GenerateDashboardAsync(
        string question, IReadOnlyList<ChatTurn>? history = null, CancellationToken ct = default)
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

        var tools = ToolDefinitions.Build(_db, _documents.Enabled);
        var jsonRepairAttempts = 0;

        for (var iteration = 0; iteration < MaxToolIterations; iteration++)
        {
            ct.ThrowIfCancellationRequested();
            var response = await CallMessagesApiAsync(messages, tools, ct);

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

                    var (result, isError) = await ExecuteToolAsync(toolName, input, ct);
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

            var (dashboard, parseError) = TryParseDashboard(text);
            if (dashboard is not null) return dashboard;

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

    private async Task<JsonObject> CallMessagesApiAsync(JsonArray messages, JsonArray tools, CancellationToken ct)
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
                    ["text"] = BuildSystemPrompt(),
                    ["cache_control"] = new JsonObject { ["type"] = "ephemeral" },
                },
            },
            ["tools"] = tools.DeepClone(),
            ["messages"] = messages.DeepClone(),
        };
        MarkLastMessageCacheable(body["messages"]!.AsArray());

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/messages")
        {
            Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("x-api-key", _apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");

        using var response = await _http.SendAsync(request, ct);
        var responseText = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Anthropic API returned {(int)response.StatusCode}: {responseText}");

        return JsonNode.Parse(responseText)?.AsObject()
            ?? throw new InvalidOperationException("Anthropic API returned an empty response body.");
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

    // $$ delimiters: {{expr}} interpolates, single braces stay literal for the JSON schema below.
    private string BuildSystemPrompt() =>
        $$"""
        You are an analytics assistant that answers questions about data stored in SQL Server and
        responds with a dashboard specification in JSON.

        WORKFLOW
        1. Call list_files to see the available tables and columns (unless you already know them from
           earlier in this conversation).
        2. Call query_data with T-SQL SELECT queries to get the numbers you need. Prefer aggregated
           queries (GROUP BY) that return chart-ready data over fetching raw rows.
        3. When you have the data, respond with the final dashboard JSON and nothing else.

        {{_db.DialectPrompt}}

        FINAL RESPONSE FORMAT
        Your final message must be ONLY a single JSON object — no markdown code fences, no prose
        before or after — matching exactly this schema:
        {
          "summary": "1-2 sentence answer to the question",
          "widgets": [
            {
              "type": "kpi | bar | line | pie | table",
              "title": "string",
              "data": [ ... ],
              "xKey": "optional string, for bar/line: name of the label field",
              "yKey": "optional string, for bar/line: name of the value field"
            }
          ]
        }
        Widget data conventions:
        - kpi: data is [{"label": "...", "value": <number or string>}] (one entry).
        - bar/line: data is an array of objects; set xKey to the category/time field name and yKey to
          the numeric field name (e.g. [{"month": "2024-01", "revenue": 1234.5}], xKey "month", yKey "revenue").
        - pie: data is [{"label": "...", "value": <number>}, ...] (keep to at most 6 slices; group the rest into "Other").
        - table: data is an array of row objects; keys become column headers.
        Choose 2-4 widgets that best answer the question; lead with a kpi when a single number is the
        headline. Keep numbers as JSON numbers, not strings.
        """;

    private async Task<(string Result, bool IsError)> ExecuteToolAsync(
        string toolName, JsonObject input, CancellationToken ct)
    {
        try
        {
            switch (toolName)
            {
                case "list_files":
                {
                    var schema = await _loader.GetSchemaAsync(ct);
                    return (JsonSerializer.Serialize(schema), false);
                }
                case "query_data":
                {
                    var sql = input["sql"]?.GetValue<string>();
                    if (string.IsNullOrWhiteSpace(sql))
                        return ("Error: 'sql' input is required.", true);
                    return await ExecuteQueryAsync(sql, ct);
                }
                case "search_documents":
                {
                    var query = input["query"]?.GetValue<string>() ?? string.Empty;
                    var hits = _documents.Search(query);
                    return (JsonSerializer.Serialize(hits), false);
                }
                default:
                    return ($"Error: unknown tool '{toolName}'.", true);
            }
        }
        catch (DbException ex)
        {
            // Feed SQL errors back so Claude can correct its query.
            _logger.LogWarning(ex, "SQL error executing tool {Tool}", toolName);
            return ($"{_db.DialectName} error: {ex.Message}", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing tool {Tool}", toolName);
            return ($"Error: {ex.Message}", true);
        }
    }

    private async Task<(string Result, bool IsError)> ExecuteQueryAsync(string sql, CancellationToken ct)
    {
        var validationError = ValidateReadOnlySql(sql);
        if (validationError is not null)
            return ($"Query rejected: {validationError}", true);

        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = new List<Dictionary<string, object?>>();

        await using var reader = await connection.ExecuteReaderAsync(
            new CommandDefinition(sql, commandTimeout: 30, cancellationToken: ct));
        while (rows.Count < MaxRowsReturned && await reader.ReadAsync(ct))
        {
            var row = new Dictionary<string, object?>();
            for (var i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            rows.Add(row);
        }

        return (JsonSerializer.Serialize(new { rowCount = rows.Count, rows }), false);
    }

    /// <summary>
    /// Parses and validates the final dashboard JSON. Tolerates a markdown code fence
    /// or stray prose around the object, but the JSON itself must match the schema.
    /// </summary>
    private static (DashboardSpec? Dashboard, string? Error) TryParseDashboard(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (null, "the response contained no text.");

        var candidate = text.Trim();
        var fence = Regex.Match(candidate, @"```(?:json)?\s*(\{.*\})\s*```", RegexOptions.Singleline);
        if (fence.Success)
            candidate = fence.Groups[1].Value;
        else
        {
            var start = candidate.IndexOf('{');
            var end = candidate.LastIndexOf('}');
            if (start < 0 || end <= start)
                return (null, "no JSON object found in the response.");
            candidate = candidate[start..(end + 1)];
        }

        DashboardSpec? spec;
        try
        {
            spec = JsonSerializer.Deserialize<DashboardSpec>(candidate);
        }
        catch (JsonException ex)
        {
            return (null, $"JSON deserialization failed: {ex.Message}");
        }

        if (spec is null)
            return (null, "JSON deserialized to null.");

        var validationErrors = spec.Validate();
        if (validationErrors.Count > 0)
            return (null, string.Join(" ", validationErrors));

        return (spec, null);
    }

    /// <summary>Returns an error message if the SQL is not a single read-only SELECT, else null.</summary>
    internal static string? ValidateReadOnlySql(string sql)
    {
        // Strip comments so keywords can't hide in or behind them.
        var stripped = Regex.Replace(sql, @"--[^\n]*|/\*.*?\*/", " ", RegexOptions.Singleline).Trim();

        if (stripped.Length == 0)
            return "empty statement.";

        if (!Regex.IsMatch(stripped, @"^(SELECT|WITH)\b", RegexOptions.IgnoreCase))
            return "only SELECT statements (optionally starting with a WITH clause) are allowed.";

        // Reject multiple statements: a semicolon may only appear at the very end.
        var withoutTrailingSemicolons = stripped.TrimEnd(';', ' ', '\t', '\r', '\n');
        if (withoutTrailingSemicolons.Contains(';'))
            return "multiple statements are not allowed.";

        var forbidden = ForbiddenSqlKeywords.Match(stripped);
        if (forbidden.Success)
            return $"forbidden keyword '{forbidden.Value.ToUpperInvariant()}' — the query must be read-only.";

        return null;
    }
}
