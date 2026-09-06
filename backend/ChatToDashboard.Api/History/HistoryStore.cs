using ChatToDashboard.Api.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;

namespace ChatToDashboard.Api.History;

/// <summary>
/// Persists generated dashboards so a user can reopen one later without calling the
/// model again. History is per-user (see <see cref="Controllers.HistoryController"/> for
/// how the user id is derived — the app has no login yet) and capped at the latest
/// <see cref="MaxPerUser"/> entries; the cap is enforced inline on every insert.
/// </summary>
public class HistoryStore
{
    private const int MaxPerUser = 60;

    private readonly DataStore _db;
    private readonly ILogger<HistoryStore> _logger;

    public HistoryStore(DataStore db, ILogger<HistoryStore> logger)
    {
        _db = db;
        _logger = logger;
    }

    private string Table => _db.Provider == DbProvider.Sqlite
        ? "\"DashboardHistory\""
        : "[staging].[DashboardHistory]";

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);

        var text = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {Table} (
                 "Id" TEXT PRIMARY KEY, "UserId" TEXT, "Question" TEXT, "QueryDescription" TEXT,
                 "Summary" TEXT, "WidgetsJson" TEXT, "FiltersJson" TEXT, "ActiveFiltersJson" TEXT,
                 "CreatedAt" TEXT)
               """
            : $"""
               IF OBJECT_ID('staging.DashboardHistory') IS NULL
               CREATE TABLE {Table} (
                 [Id] NVARCHAR(64) PRIMARY KEY, [UserId] NVARCHAR(200), [Question] NVARCHAR(MAX),
                 [QueryDescription] NVARCHAR(MAX), [Summary] NVARCHAR(MAX), [WidgetsJson] NVARCHAR(MAX),
                 [FiltersJson] NVARCHAR(MAX), [ActiveFiltersJson] NVARCHAR(MAX), [CreatedAt] DATETIME2)
               """;

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = text;
            await command.ExecuteNonQueryAsync(ct);
        }

        // Migration for a table created before FiltersJson/ActiveFiltersJson existed —
        // SQLite has no "ADD COLUMN IF NOT EXISTS", so the duplicate-column failure is just
        // swallowed (same pattern as LlmSettingsStore.EnsureSchemaAsync).
        foreach (var (column, sqliteType, sqlServerType) in new[]
                 {
                     ("FiltersJson", "TEXT", "NVARCHAR(MAX)"),
                     ("ActiveFiltersJson", "TEXT", "NVARCHAR(MAX)"),
                 })
        {
            try
            {
                await using var alter = connection.CreateCommand();
                alter.CommandText = _db.Provider == DbProvider.Sqlite
                    ? $"ALTER TABLE {Table} ADD COLUMN \"{column}\" {sqliteType}"
                    : $"ALTER TABLE {Table} ADD [{column}] {sqlServerType}";
                await alter.ExecuteNonQueryAsync(ct);
            }
            catch (SqliteException ex) when (ex.Message.Contains("duplicate column", StringComparison.OrdinalIgnoreCase))
            {
            }
            catch (SqlException ex) when (ex.Message.Contains("already", StringComparison.OrdinalIgnoreCase))
            {
            }
        }
    }

    /// <summary>Saves a new entry, then trims the user's history down to <see cref="MaxPerUser"/>.</summary>
    public async Task<DashboardHistoryEntry> SaveAsync(DashboardHistoryEntry entry, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        entry.Id = string.IsNullOrWhiteSpace(entry.Id) ? Guid.NewGuid().ToString("N") : entry.Id;
        entry.CreatedAt = entry.CreatedAt == default ? DateTime.UtcNow : entry.CreatedAt;

        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"INSERT INTO {Table} (Id, UserId, Question, QueryDescription, Summary, WidgetsJson, " +
            "FiltersJson, ActiveFiltersJson, CreatedAt) " +
            "VALUES (@Id, @UserId, @Question, @QueryDescription, @Summary, @WidgetsJson, " +
            "@FiltersJson, @ActiveFiltersJson, @CreatedAt)",
            entry);

        // Retention: keep only the latest MaxPerUser rows for this user. Simplest inline
        // approach — no background job — delete whatever falls outside that window.
        var staleIds = (await connection.QueryAsync<string>(
                $"SELECT Id FROM {Table} WHERE UserId = @UserId ORDER BY CreatedAt DESC",
                new { entry.UserId }))
            .Skip(MaxPerUser)
            .ToList();
        if (staleIds.Count > 0)
            await connection.ExecuteAsync($"DELETE FROM {Table} WHERE Id IN @Ids", new { Ids = staleIds });

        return entry;
    }

    /// <summary>The user's entries, newest first, capped at <paramref name="limit"/>.</summary>
    public async Task<IReadOnlyList<DashboardHistoryEntry>> ListAsync(
        string userId, int limit = MaxPerUser, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var top = _db.Provider == DbProvider.Sqlite ? "" : $"TOP {limit} ";
        var tail = _db.Provider == DbProvider.Sqlite ? $" LIMIT {limit}" : "";
        // COALESCE covers rows saved before FiltersJson/ActiveFiltersJson existed — the
        // migration in EnsureSchemaAsync adds the columns as NULL on old rows, not "[]"/"{}".
        var rows = await connection.QueryAsync<DashboardHistoryEntry>(
            $"SELECT {top}Id, UserId, Question, QueryDescription, Summary, WidgetsJson, " +
            "COALESCE(FiltersJson, '[]') AS FiltersJson, COALESCE(ActiveFiltersJson, '{}') AS ActiveFiltersJson, " +
            $"CreatedAt FROM {Table} WHERE UserId = @userId ORDER BY CreatedAt DESC{tail}",
            new { userId });
        return rows.ToList();
    }

    /// <summary>
    /// Overwrites an existing entry's content in place — only if it belongs to
    /// <paramref name="userId"/>. Used by dashboard-editor autosave, so editing a saved
    /// dashboard updates the same history row instead of piling up a new one per change.
    /// </summary>
    public async Task<bool> UpdateAsync(
        string userId, string id, string summary, string widgetsJson,
        string filtersJson, string activeFiltersJson, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var affected = await connection.ExecuteAsync(
            $"UPDATE {Table} SET Summary = @summary, QueryDescription = @summary, WidgetsJson = @widgetsJson, " +
            "FiltersJson = @filtersJson, ActiveFiltersJson = @activeFiltersJson " +
            "WHERE Id = @id AND UserId = @userId",
            new { id, userId, summary, widgetsJson, filtersJson, activeFiltersJson });
        return affected > 0;
    }

    /// <summary>Deletes one entry — only if it belongs to <paramref name="userId"/>.</summary>
    public async Task<bool> DeleteAsync(string userId, string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var affected = await connection.ExecuteAsync(
            $"DELETE FROM {Table} WHERE Id = @id AND UserId = @userId", new { id, userId });
        return affected > 0;
    }

    public async Task ClearAsync(string userId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"DELETE FROM {Table} WHERE UserId = @userId", new { userId });
    }
}
