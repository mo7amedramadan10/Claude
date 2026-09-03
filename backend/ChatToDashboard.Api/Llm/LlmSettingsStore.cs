using ChatToDashboard.Api.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;

namespace ChatToDashboard.Api.Llm;

/// <summary>
/// The runtime-changeable settings this app has: which LLM provider answers questions, and
/// which model for each of the providers that support picking one (Ollama, OpenAI). A single
/// row, persisted so a switch made from the UI survives a restart — unlike the Llm:Provider
/// config value, which only sets the *default* the first time this row doesn't exist yet.
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

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = text;
            await command.ExecuteNonQueryAsync(ct);
        }

        // Migration for a table created before OpenAiModel existed — SQLite has no
        // "ADD COLUMN IF NOT EXISTS", so the duplicate-column failure is just swallowed.
        try
        {
            await using var alter = connection.CreateCommand();
            alter.CommandText = _db.Provider == DbProvider.Sqlite
                ? $"ALTER TABLE {Table} ADD COLUMN \"OpenAiModel\" TEXT"
                : $"ALTER TABLE {Table} ADD [OpenAiModel] NVARCHAR(200)";
            await alter.ExecuteNonQueryAsync(ct);
        }
        catch (SqliteException ex) when (ex.Message.Contains("duplicate column", StringComparison.OrdinalIgnoreCase))
        {
        }
        catch (SqlException ex) when (ex.Message.Contains("already", StringComparison.OrdinalIgnoreCase))
        {
        }
    }

    /// <summary>Current override, if any has ever been saved — every field null otherwise.</summary>
    public async Task<(string? Provider, string? OllamaModel, string? OpenAiModel)> GetAsync(CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var row = await connection.QuerySingleOrDefaultAsync<SettingsRow>(
            $"SELECT Provider, OllamaModel, OpenAiModel FROM {Table} WHERE Id = 1");
        return (row?.Provider, row?.OllamaModel, row?.OpenAiModel);
    }

    public async Task SetAsync(string provider, string? ollamaModel, string? openAiModel, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);

        // Only the field relevant to the provider being saved is touched — switching to
        // OpenAI, say, must not wipe out a previously chosen Ollama model for next time.
        var sql = _db.Provider == DbProvider.Sqlite
            ? $"""
               INSERT INTO {Table} (Id, Provider, OllamaModel, OpenAiModel) VALUES (1, @provider, @ollamaModel, @openAiModel)
               ON CONFLICT(Id) DO UPDATE SET
                 Provider = @provider,
                 OllamaModel = COALESCE(@ollamaModel, OllamaModel),
                 OpenAiModel = COALESCE(@openAiModel, OpenAiModel)
               """
            : $"""
               MERGE {Table} AS t USING (SELECT 1 AS Id) AS s ON t.Id = s.Id
               WHEN MATCHED THEN UPDATE SET
                 Provider = @provider,
                 OllamaModel = COALESCE(@ollamaModel, t.OllamaModel),
                 OpenAiModel = COALESCE(@openAiModel, t.OpenAiModel)
               WHEN NOT MATCHED THEN INSERT (Id, Provider, OllamaModel, OpenAiModel) VALUES (1, @provider, @ollamaModel, @openAiModel);
               """;

        await connection.ExecuteAsync(sql, new { provider, ollamaModel, openAiModel });
    }

    private class SettingsRow
    {
        public string? Provider { get; set; }
        public string? OllamaModel { get; set; }
        public string? OpenAiModel { get; set; }
    }
}
