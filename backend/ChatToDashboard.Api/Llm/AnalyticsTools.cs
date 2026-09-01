using System.Data.Common;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using ChatToDashboard.Api.Data;
using ChatToDashboard.Api.Models;
using Dapper;

namespace ChatToDashboard.Api.Llm;

/// <summary>A tool in provider-neutral form; each LLM client maps it to its own wire format.</summary>
public record ToolSpec(string Name, string Description, JsonObject InputSchema);

/// <summary>
/// Everything about the dashboard agent that does not depend on which LLM provider is used:
/// the tool catalogue, the tool implementations, read-only SQL validation, the system prompt,
/// and parsing/validating the final dashboard JSON.
/// </summary>
public class AnalyticsTools
{
    public const int MaxRowsReturned = 500;

    private static readonly Regex ForbiddenSqlKeywords = new(
        @"\b(INSERT|UPDATE|DELETE|DROP|ALTER|CREATE|TRUNCATE|MERGE|EXEC|EXECUTE|GRANT|REVOKE|BACKUP|RESTORE|USE|KILL|SHUTDOWN|PRAGMA|ATTACH|DETACH|VACUUM|REINDEX|REPLACE)\b|(\bsp_\w+)|(\bxp_\w+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly DataFolderLoader _loader;
    private readonly DataStore _db;
    private readonly DocumentSearchService _documents;
    private readonly ILogger<AnalyticsTools> _logger;

    public AnalyticsTools(
        DataFolderLoader loader,
        DataStore db,
        DocumentSearchService documents,
        ILogger<AnalyticsTools> logger)
    {
        _loader = loader;
        _db = db;
        _documents = documents;
        _logger = logger;
    }

    public IReadOnlyList<ToolSpec> BuildTools()
    {
        var rowCap = _db.Provider == DbProvider.Sqlite ? "LIMIT 500" : "TOP 500";

        var tools = new List<ToolSpec>
        {
            new(
                "list_files",
                "Lists the available data tables with their column names and data types. " +
                "Call this first to see what data exists.",
                new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject(),
                    ["required"] = new JsonArray(),
                }),
            new(
                "query_data",
                $"Runs a read-only SELECT query ({_db.DialectName} dialect) against the loaded data tables " +
                $"and returns the rows as JSON. Only SELECT statements are allowed; results are capped at " +
                $"{MaxRowsReturned} rows, so always use {rowCap} (or less). Reference tables as {_db.TableNamingHint}.",
                new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["sql"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = $"A single SELECT statement ({_db.DialectName} dialect).",
                        },
                    },
                    ["required"] = new JsonArray { "sql" },
                }),
        };

        if (_documents.Enabled)
        {
            tools.Add(new ToolSpec(
                "search_documents",
                "Searches the unstructured documents (PDF, DOCX) in the data folder and returns the " +
                "most relevant text passages for the given query.",
                new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["query"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "Natural-language search query.",
                        },
                    },
                    ["required"] = new JsonArray { "query" },
                }));
        }

        return tools;
    }

    // $$ delimiters: {{expr}} interpolates, single braces stay literal for the JSON schema below.
    public string BuildSystemPrompt() =>
        $$"""
        You are an analytics assistant that answers questions about data stored in a database and
        responds with a dashboard specification in JSON.

        WORKFLOW
        1. Call list_files to see the available tables and columns (unless you already know them from
           earlier in this conversation).
        2. Call query_data with SELECT queries to get the numbers you need. Prefer aggregated
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

    public async Task<(string Result, bool IsError)> ExecuteToolAsync(
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
            // Feed SQL errors back so the model can correct its query.
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
    public static (DashboardSpec? Dashboard, string? Error) TryParseDashboard(string text)
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
    public static string? ValidateReadOnlySql(string sql)
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
