using System.Security.Cryptography;
using ChatToDashboard.Api.Data;
using Dapper;

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
                 "Summary" TEXT, "WidgetsJson" TEXT, "CreatedAt" TEXT)
               """
            : $"""
               IF OBJECT_ID('staging.SharedDashboard') IS NULL
               CREATE TABLE {Table} (
                 [Id] NVARCHAR(32) PRIMARY KEY, [CreatedByUserId] NVARCHAR(200), [Question] NVARCHAR(MAX),
                 [Summary] NVARCHAR(MAX), [WidgetsJson] NVARCHAR(MAX), [CreatedAt] DATETIME2)
               """;

        await using var command = connection.CreateCommand();
        command.CommandText = text;
        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task<SharedDashboard> SaveAsync(SharedDashboard entry, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        entry.Id = NewShareId();
        entry.CreatedAt = DateTime.UtcNow;

        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"INSERT INTO {Table} (Id, CreatedByUserId, Question, Summary, WidgetsJson, CreatedAt) " +
            "VALUES (@Id, @CreatedByUserId, @Question, @Summary, @WidgetsJson, @CreatedAt)",
            entry);
        return entry;
    }

    /// <summary>Reads a share by id — deliberately no owner check, this is the public view.</summary>
    public async Task<SharedDashboard?> GetAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<SharedDashboard>(
            $"SELECT Id, CreatedByUserId, Question, Summary, WidgetsJson, CreatedAt FROM {Table} WHERE Id = @id",
            new { id });
    }

    public async Task<IReadOnlyList<SharedDashboard>> ListAsync(string userId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<SharedDashboard>(
            $"SELECT Id, CreatedByUserId, Question, Summary, WidgetsJson, CreatedAt FROM {Table} " +
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

    /// <summary>16 hex characters (64 bits) — unguessable enough for a casual share link.</summary>
    private static string NewShareId()
    {
        Span<byte> bytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
