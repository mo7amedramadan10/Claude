using System.Security.Cryptography;
using ChatToDashboard.Api.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;

namespace ChatToDashboard.Api.Share;

/// <summary>
/// Persists published dashboard snapshots. Anyone with a share's id can read it
/// (<see cref="GetAsync"/> takes no owner) — only listing "my shares" and deleting one
/// are scoped to the creator's browser id, same pattern as <c>HistoryStore</c>.
/// </summary>
public class ShareStore
{
    private readonly DataStore _db;

    public ShareStore(DataStore db) => _db = db;

    private string Table => _db.Provider == DbProvider.Sqlite
        ? "\"SharedDashboard\""
        : "[staging].[SharedDashboard]";

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);

        var text = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {Table} (
                 "Id" TEXT PRIMARY KEY, "CreatedByUserId" TEXT, "Question" TEXT,
                 "Summary" TEXT, "WidgetsJson" TEXT, "FiltersJson" TEXT, "ActiveFiltersJson" TEXT,
                 "CreatedAt" TEXT)
               """
            : $"""
               IF OBJECT_ID('staging.SharedDashboard') IS NULL
               CREATE TABLE {Table} (
                 [Id] NVARCHAR(32) PRIMARY KEY, [CreatedByUserId] NVARCHAR(200), [Question] NVARCHAR(MAX),
                 [Summary] NVARCHAR(MAX), [WidgetsJson] NVARCHAR(MAX), [FiltersJson] NVARCHAR(MAX),
                 [ActiveFiltersJson] NVARCHAR(MAX), [CreatedAt] DATETIME2)
               """;

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = text;
            await command.ExecuteNonQueryAsync(ct);
        }

        // Migration for a table created before FiltersJson/ActiveFiltersJson/the Part 3
        // columns existed — SQLite has no "ADD COLUMN IF NOT EXISTS", so the duplicate-column
        // failure is just swallowed (same pattern as HistoryStore.EnsureSchemaAsync).
        foreach (var (column, sqliteType, sqlServerType) in new[]
                 {
                     ("FiltersJson", "TEXT", "NVARCHAR(MAX)"),
                     ("ActiveFiltersJson", "TEXT", "NVARCHAR(MAX)"),
                     ("ExpiresAt", "TEXT", "DATETIME2"),
                     ("RevokedAt", "TEXT", "DATETIME2"),
                     ("ViewCount", "INTEGER", "INT"),
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
            // Error 2705: "Column names in each table must be unique" — SQL Server's actual
            // wording for a duplicate ADD COLUMN never contains "already" (see UserStore's
            // identical migration loop for the full explanation).
            catch (SqlException ex) when (ex.Number == 2705)
            {
            }
        }
    }

    public async Task<SharedDashboard> SaveAsync(SharedDashboard entry, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        entry.Id = NewShareId();
        entry.CreatedAt = DateTime.UtcNow;
        entry.ViewCount = 0;

        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"INSERT INTO {Table} (Id, CreatedByUserId, Question, Summary, WidgetsJson, " +
            "FiltersJson, ActiveFiltersJson, CreatedAt, ExpiresAt, RevokedAt, ViewCount) " +
            "VALUES (@Id, @CreatedByUserId, @Question, @Summary, @WidgetsJson, " +
            "@FiltersJson, @ActiveFiltersJson, @CreatedAt, @ExpiresAt, @RevokedAt, @ViewCount)",
            entry);
        return entry;
    }

    /// <summary>Reads a share by id — deliberately no owner check, this is the public view.
    /// Includes an expired/revoked entry too — the caller decides what "inactive" means and
    /// how to respond; this is just the raw row.</summary>
    public async Task<SharedDashboard?> GetAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        // COALESCE covers rows saved before FiltersJson/ActiveFiltersJson/ViewCount existed —
        // the migration in EnsureSchemaAsync adds the columns as NULL on old rows, not "[]"/0.
        return await connection.QuerySingleOrDefaultAsync<SharedDashboard>(
            "SELECT Id, CreatedByUserId, Question, Summary, WidgetsJson, " +
            "COALESCE(FiltersJson, '[]') AS FiltersJson, COALESCE(ActiveFiltersJson, '{}') AS ActiveFiltersJson, " +
            "CreatedAt, ExpiresAt, RevokedAt, COALESCE(ViewCount, 0) AS ViewCount " +
            $"FROM {Table} WHERE Id = @id",
            new { id });
    }

    /// <summary>Bumps the view count by one — called only for a genuinely active (not
    /// expired/revoked) view, so the count reflects real opens of live content.</summary>
    public async Task IncrementViewCountAsync(string id, CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"UPDATE {Table} SET ViewCount = COALESCE(ViewCount, 0) + 1 WHERE Id = @id", new { id });
    }

    public async Task<IReadOnlyList<SharedDashboard>> ListAsync(string userId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<SharedDashboard>(
            "SELECT Id, CreatedByUserId, Question, Summary, WidgetsJson, CreatedAt, " +
            $"ExpiresAt, RevokedAt, COALESCE(ViewCount, 0) AS ViewCount FROM {Table} " +
            "WHERE CreatedByUserId = @userId ORDER BY CreatedAt DESC",
            new { userId });
        return rows.ToList();
    }

    public async Task<bool> DeleteAsync(string userId, string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var affected = await connection.ExecuteAsync(
            $"DELETE FROM {Table} WHERE Id = @id AND CreatedByUserId = @userId", new { id, userId });
        return affected > 0;
    }

    /// <summary>Manually revokes a share — creator-only, idempotent (revoking an already
    /// revoked link just keeps its original RevokedAt).</summary>
    public async Task<bool> RevokeAsync(string userId, string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var affected = await connection.ExecuteAsync(
            $"UPDATE {Table} SET RevokedAt = COALESCE(RevokedAt, @now) " +
            "WHERE Id = @id AND CreatedByUserId = @userId",
            new { id, userId, now = DateTime.UtcNow });
        return affected > 0;
    }

    /// <summary>16 hex characters (64 bits) — unguessable enough for a casual share link.</summary>
    private static string NewShareId()
    {
        Span<byte> bytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
