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
        // Error 2705: "Column names in each table must be unique" — SQL Server's actual
        // wording for a duplicate ADD COLUMN never contains "already" (see UserStore's
        // identical migration loop for the full explanation).
        catch (SqlException ex) when (ex.Number == 2705)
        {
        }

        // Migration for a table created before document reading got its own provider —
        // null/empty means disabled (PDF uploads keep using PdfPig's plain-text extraction
        // only), never inherited from the dashboard-building Provider above.
        try
        {
            await using var alter = connection.CreateCommand();
            alter.CommandText = _db.Provider == DbProvider.Sqlite
                ? $"ALTER TABLE {Table} ADD COLUMN \"DocumentReaderProvider\" TEXT"
                : $"ALTER TABLE {Table} ADD [DocumentReaderProvider] NVARCHAR(50)";
            await alter.ExecuteNonQueryAsync(ct);
        }
        catch (SqliteException ex) when (ex.Message.Contains("duplicate column", StringComparison.OrdinalIgnoreCase))
        {
        }
        catch (SqlException ex) when (ex.Number == 2705)
        {
        }
    }

    /// <summary>Current override, if any has ever been saved — every field null otherwise.
    /// DocumentReaderProvider null/empty means "disabled" (PdfPig only), not "same as
    /// Provider" — it never falls back to the dashboard-building provider.</summary>
    public async Task<(string? Provider, string? OllamaModel, string? OpenAiModel, string? DocumentReaderProvider)> GetAsync(CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var row = await connection.QuerySingleOrDefaultAsync<SettingsRow>(
            $"SELECT Provider, OllamaModel, OpenAiModel, DocumentReaderProvider FROM {Table} WHERE Id = 1");
        return (row?.Provider, row?.OllamaModel, row?.OpenAiModel, row?.DocumentReaderProvider);
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

    /// <summary>
    /// Sets (or clears, with null/"") the document-reading provider only — independent of
    /// SetAsync above, since switching who builds dashboards shouldn't touch this, and vice
    /// versa. A row must already exist (from the app's own startup or an earlier SetAsync);
    /// on a genuinely fresh DB this is a no-op, matching the "disabled by default" contract.
    /// </summary>
    public async Task SetDocumentReaderAsync(string? provider, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var sql = _db.Provider == DbProvider.Sqlite
            ? $"""
               INSERT INTO {Table} (Id, DocumentReaderProvider) VALUES (1, @provider)
               ON CONFLICT(Id) DO UPDATE SET DocumentReaderProvider = @provider
               """
            : $"""
               MERGE {Table} AS t USING (SELECT 1 AS Id) AS s ON t.Id = s.Id
               WHEN MATCHED THEN UPDATE SET DocumentReaderProvider = @provider
               WHEN NOT MATCHED THEN INSERT (Id, DocumentReaderProvider) VALUES (1, @provider);
               """;
        await connection.ExecuteAsync(sql, new { provider = string.IsNullOrWhiteSpace(provider) ? null : provider });
    }

    private class SettingsRow
    {
        public string? Provider { get; set; }
        public string? OllamaModel { get; set; }
        public string? OpenAiModel { get; set; }
        public string? DocumentReaderProvider { get; set; }
    }
}
