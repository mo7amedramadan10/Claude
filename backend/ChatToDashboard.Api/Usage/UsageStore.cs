using System.Text.Json;
using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Usage;

/// <summary>
/// Persists one row per question asked: the prompts sent, the tool calls made, the
/// model's reply, token counts and estimated cost.
/// </summary>
public class UsageStore
{
    private readonly DataStore _db;
    private readonly CostCalculator _cost;
    private readonly ILogger<UsageStore> _logger;

    public UsageStore(DataStore db, CostCalculator cost, ILogger<UsageStore> logger)
    {
        _db = db;
        _cost = cost;
        _logger = logger;
    }

    private string Table => _db.Provider == DbProvider.Sqlite ? "\"usage_Log\"" : "[staging].[usage_Log]";

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);

        var text = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {Table} (
                 "Id" TEXT PRIMARY KEY, "Timestamp" TEXT, "Provider" TEXT, "Model" TEXT,
                 "Question" TEXT, "EnabledSources" TEXT, "TurnCount" INTEGER, "ToolCallCount" INTEGER,
                 "InputTokens" INTEGER, "OutputTokens" INTEGER, "CacheReadTokens" INTEGER,
                 "CacheWriteTokens" INTEGER, "TotalTokens" INTEGER, "EstimatedCost" REAL,
                 "DurationMs" INTEGER, "Success" INTEGER, "Error" TEXT,
                 "SystemPrompt" TEXT, "TurnsJson" TEXT, "ToolCallsJson" TEXT, "FinalResponse" TEXT)
               """
            : $"""
               IF OBJECT_ID('staging.usage_Log') IS NULL
               CREATE TABLE {Table} (
                 [Id] NVARCHAR(64) PRIMARY KEY, [Timestamp] DATETIME2, [Provider] NVARCHAR(40), [Model] NVARCHAR(120),
                 [Question] NVARCHAR(MAX), [EnabledSources] NVARCHAR(MAX), [TurnCount] INT, [ToolCallCount] INT,
                 [InputTokens] INT, [OutputTokens] INT, [CacheReadTokens] INT,
                 [CacheWriteTokens] INT, [TotalTokens] INT, [EstimatedCost] DECIMAL(18,6),
                 [DurationMs] BIGINT, [Success] BIT, [Error] NVARCHAR(MAX),
                 [SystemPrompt] NVARCHAR(MAX), [TurnsJson] NVARCHAR(MAX), [ToolCallsJson] NVARCHAR(MAX),
                 [FinalResponse] NVARCHAR(MAX))
               """;

        await using var command = connection.CreateCommand();
        command.CommandText = text;
        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task SaveAsync(UsageRecord record, CancellationToken ct = default)
    {
        try
        {
            await EnsureSchemaAsync(ct);
            await using var connection = await _db.OpenConnectionAsync(ct);
            await connection.ExecuteAsync(
                $"INSERT INTO {Table} (Id, Timestamp, Provider, Model, Question, EnabledSources, TurnCount, " +
                "ToolCallCount, InputTokens, OutputTokens, CacheReadTokens, CacheWriteTokens, TotalTokens, " +
                "EstimatedCost, DurationMs, Success, Error, SystemPrompt, TurnsJson, ToolCallsJson, FinalResponse) " +
                "VALUES (@Id, @Timestamp, @Provider, @Model, @Question, @EnabledSources, @TurnCount, " +
                "@ToolCallCount, @InputTokens, @OutputTokens, @CacheReadTokens, @CacheWriteTokens, @TotalTokens, " +
                "@EstimatedCost, @DurationMs, @Success, @Error, @SystemPrompt, @TurnsJson, @ToolCallsJson, @FinalResponse)",
                new
                {
                    record.Id, record.Timestamp, record.Provider, record.Model, record.Question,
                    record.EnabledSources, record.TurnCount, record.ToolCallCount, record.InputTokens,
                    record.OutputTokens, record.CacheReadTokens, record.CacheWriteTokens, record.TotalTokens,
                    record.EstimatedCost, record.DurationMs, record.Success, record.Error, record.SystemPrompt,
                    TurnsJson = JsonSerializer.Serialize(record.Turns),
                    ToolCallsJson = JsonSerializer.Serialize(record.ToolCalls),
                    record.FinalResponse,
                });
        }
        catch (Exception ex)
        {
            // Never let observability break the thing it observes.
            _logger.LogError(ex, "Failed to record usage for request {Id}", record.Id);
        }
    }

    /// <summary>Recent requests without the heavy prompt/response payloads.</summary>
    public async Task<IReadOnlyList<UsageRecord>> ListAsync(int limit = 100, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var top = _db.Provider == DbProvider.Sqlite ? "" : $"TOP {limit} ";
        var tail = _db.Provider == DbProvider.Sqlite ? $" LIMIT {limit}" : "";
        var rows = await connection.QueryAsync<UsageRecord>(
            $"SELECT {top}Id, Timestamp, Provider, Model, Question, EnabledSources, TurnCount, ToolCallCount, " +
            "InputTokens, OutputTokens, CacheReadTokens, CacheWriteTokens, TotalTokens, EstimatedCost, " +
            $"DurationMs, Success, Error FROM {Table} ORDER BY Timestamp DESC{tail}");

        // Priced at read time so an edited price table applies to the whole history.
        var records = rows.ToList();
        foreach (var record in records) record.EstimatedCost = _cost.Estimate(record);
        return records;
    }

    /// <summary>One request in full, including every prompt and tool result.</summary>
    public async Task<UsageRecord?> GetAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var row = await connection.QuerySingleOrDefaultAsync<dynamic>(
            $"SELECT * FROM {Table} WHERE Id = @id", new { id });
        if (row is null) return null;

        var dict = (IDictionary<string, object?>)row;
        string? Text(string key) => dict.TryGetValue(key, out var v) ? v?.ToString() : null;
        int Int(string key) => dict.TryGetValue(key, out var v) && v is not null ? Convert.ToInt32(v) : 0;

        var record = new UsageRecord
        {
            Id = Text("Id") ?? id,
            Timestamp = DateTime.TryParse(Text("Timestamp"), out var ts) ? ts : default,
            Provider = Text("Provider") ?? "",
            Model = Text("Model") ?? "",
            Question = Text("Question") ?? "",
            EnabledSources = Text("EnabledSources") ?? "",
            TurnCount = Int("TurnCount"),
            ToolCallCount = Int("ToolCallCount"),
            InputTokens = Int("InputTokens"),
            OutputTokens = Int("OutputTokens"),
            CacheReadTokens = Int("CacheReadTokens"),
            CacheWriteTokens = Int("CacheWriteTokens"),
            TotalTokens = Int("TotalTokens"),
            EstimatedCost = dict["EstimatedCost"] is null ? 0 : Convert.ToDecimal(dict["EstimatedCost"]),
            DurationMs = dict["DurationMs"] is null ? 0 : Convert.ToInt64(dict["DurationMs"]),
            Success = dict["Success"] is not null && Convert.ToBoolean(dict["Success"]),
            Error = Text("Error"),
            SystemPrompt = Text("SystemPrompt"),
            FinalResponse = Text("FinalResponse"),
            Turns = Deserialize<List<UsageTurn>>(Text("TurnsJson")) ?? new(),
            ToolCalls = Deserialize<List<UsageToolCall>>(Text("ToolCallsJson")) ?? new(),
        };
        record.EstimatedCost = _cost.Estimate(record);
        return record;
    }

    private static T? Deserialize<T>(string? json) =>
        string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json);

    public async Task<UsageSummary> SummarizeAsync(CancellationToken ct = default)
    {
        var records = await ListAsync(5000, ct);
        var summary = new UsageSummary
        {
            Requests = records.Count,
            FailedRequests = records.Count(r => !r.Success),
            InputTokens = records.Sum(r => (long)r.InputTokens),
            OutputTokens = records.Sum(r => (long)r.OutputTokens),
            CacheReadTokens = records.Sum(r => (long)r.CacheReadTokens),
            TotalTokens = records.Sum(r => (long)r.TotalTokens),
            EstimatedCost = records.Sum(r => r.EstimatedCost),
            AvgTokensPerRequest = records.Count == 0 ? 0 : (int)(records.Sum(r => (long)r.TotalTokens) / records.Count),
            AvgDurationMs = records.Count == 0 ? 0 : records.Sum(r => r.DurationMs) / records.Count,
            ByModel = records.GroupBy(r => r.Model).Select(g => new ModelUsage
            {
                Model = g.Key,
                Requests = g.Count(),
                TotalTokens = g.Sum(r => (long)r.TotalTokens),
                EstimatedCost = g.Sum(r => r.EstimatedCost),
            }).OrderByDescending(m => m.TotalTokens).ToList(),
            ByDay = records.GroupBy(r => r.Timestamp.ToString("yyyy-MM-dd")).Select(g => new DayUsage
            {
                Day = g.Key,
                Requests = g.Count(),
                TotalTokens = g.Sum(r => (long)r.TotalTokens),
                EstimatedCost = g.Sum(r => r.EstimatedCost),
            }).OrderBy(d => d.Day).TakeLast(30).ToList(),
        };
        return summary;
    }

    public async Task ClearAsync(CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"DELETE FROM {Table}");
    }
}
