using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Llm;

/// <summary>
/// The one runtime-changeable setting this app has: which LLM provider (and, for Ollama,
/// which model) currently answers questions. A single row, persisted so a switch made from
/// the UI survives a restart — unlike the Llm:Provider config value, which only sets the
/// *default* the first time this row doesn't exist yet.
/// </summary>
public class LlmSettingsStore
{
    private readonly DataStore _db;

    public LlmSettingsStore(DataStore db) => _db = db;

    private string Table => _db.Provider == DbProvider.Sqlite
        ? "\"LlmSettings\""
        : "[staging].[LlmSettings]";

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);

        var text = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {Table} (
                 "Id" INTEGER PRIMARY KEY CHECK ("Id" = 1), "Provider" TEXT, "OllamaModel" TEXT)
               """
            : $"""
               IF OBJECT_ID('staging.LlmSettings') IS NULL
               CREATE TABLE {Table} (
                 [Id] INT PRIMARY KEY CHECK ([Id] = 1), [Provider] NVARCHAR(50), [OllamaModel] NVARCHAR(200))
               """;

        await using var command = connection.CreateCommand();
        command.CommandText = text;
        await command.ExecuteNonQueryAsync(ct);
    }

    /// <summary>Current override, if any has ever been saved — both fields null otherwise.</summary>
    public async Task<(string? Provider, string? OllamaModel)> GetAsync(CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var row = await connection.QuerySingleOrDefaultAsync<SettingsRow>(
            $"SELECT Provider, OllamaModel FROM {Table} WHERE Id = 1");
        return (row?.Provider, row?.OllamaModel);
    }

    public async Task SetAsync(string provider, string? ollamaModel, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);

        var sql = _db.Provider == DbProvider.Sqlite
            ? $"""
               INSERT INTO {Table} (Id, Provider, OllamaModel) VALUES (1, @provider, @model)
               ON CONFLICT(Id) DO UPDATE SET Provider = @provider, OllamaModel = @model
               """
            : $"""
               MERGE {Table} AS t USING (SELECT 1 AS Id) AS s ON t.Id = s.Id
               WHEN MATCHED THEN UPDATE SET Provider = @provider, OllamaModel = @model
               WHEN NOT MATCHED THEN INSERT (Id, Provider, OllamaModel) VALUES (1, @provider, @model);
               """;

        await connection.ExecuteAsync(sql, new { provider, model = ollamaModel });
    }

    private class SettingsRow
    {
        public string? Provider { get; set; }
        public string? OllamaModel { get; set; }
    }
}
