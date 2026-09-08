using System.Text.Json;
using ChatToDashboard.Api.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;

namespace ChatToDashboard.Api.Users;

/// <summary>Accounts and their per-source permissions.</summary>
public class UserStore
{
    private readonly DataStore _db;
    private readonly ILogger<UserStore> _logger;

    public UserStore(DataStore db, ILogger<UserStore> logger)
    {
        _db = db;
        _logger = logger;
    }

    private string Table => _db.Provider == DbProvider.Sqlite ? "\"AppUsers\"" : "[staging].[AppUsers]";

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);

        var text = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {Table} (
                 "Id" TEXT PRIMARY KEY, "Username" TEXT UNIQUE, "DisplayName" TEXT,
                 "PasswordHash" TEXT, "AuthMethod" TEXT, "Role" TEXT, "IsActive" INTEGER,
                 "AllowAllSystems" INTEGER, "AllowedSystemsJson" TEXT,
                 "AllowAllFiles" INTEGER, "AllowedFilesJson" TEXT, "CreatedAt" TEXT)
               """
            : $"""
               IF OBJECT_ID('staging.AppUsers') IS NULL
               CREATE TABLE {Table} (
                 [Id] NVARCHAR(64) PRIMARY KEY, [Username] NVARCHAR(200) UNIQUE, [DisplayName] NVARCHAR(200),
                 [PasswordHash] NVARCHAR(400), [AuthMethod] NVARCHAR(40), [Role] NVARCHAR(40), [IsActive] BIT,
                 [AllowAllSystems] BIT, [AllowedSystemsJson] NVARCHAR(MAX),
                 [AllowAllFiles] BIT, [AllowedFilesJson] NVARCHAR(MAX), [CreatedAt] DATETIME2)
               """;

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = text;
            await command.ExecuteNonQueryAsync(ct);
        }

        // Migration for a table created before per-file permissions replaced per-category
        // ones (AllowAllCategories/AllowedCategoriesJson stay in place, unused, per this
        // codebase's own no-drop-columns convention — see HistoryStore.EnsureSchemaAsync).
        foreach (var (column, sqliteType, sqlServerType) in new[]
                 {
                     ("AllowAllFiles", "INTEGER", "BIT"),
                     ("AllowedFilesJson", "TEXT", "NVARCHAR(MAX)"),
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

        // Backfill: a row from before this migration has AllowAllFiles = NULL. Copy the old
        // AllowAllCategories value across rather than defaulting everyone to unrestricted —
        // an account that was already narrowed to specific categories should come out of
        // the migration seeing nothing (fail closed) rather than suddenly seeing every file,
        // until an admin re-grants the right ones under the new per-file model.
        await using (var backfill = connection.CreateCommand())
        {
            backfill.CommandText = $"UPDATE {Table} SET AllowAllFiles = AllowAllCategories WHERE AllowAllFiles IS NULL";
            try { await backfill.ExecuteNonQueryAsync(ct); }
            // AllowAllCategories column absent on a brand-new DB (never created at all) —
            // nothing to backfill.
            catch (SqliteException ex) when (ex.Message.Contains("no such column", StringComparison.OrdinalIgnoreCase))
            {
            }
            catch (SqlException ex) when (ex.Message.Contains("Invalid column name", StringComparison.OrdinalIgnoreCase))
            {
            }
        }
    }

    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {Table}");
    }

    public async Task<IReadOnlyList<AppUser>> ListAsync(CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<AppUser>($"SELECT * FROM {Table} ORDER BY Username");
        return rows.ToList();
    }

    public async Task<AppUser?> FindByIdAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<AppUser>($"SELECT * FROM {Table} WHERE Id = @id", new { id });
    }

    public async Task<AppUser?> FindByUsernameAsync(string username, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<AppUser>(
            $"SELECT * FROM {Table} WHERE LOWER(Username) = LOWER(@username)", new { username });
    }

    /// <summary>Active users whose username or display name contains <paramref name="query"/>
    /// — capped small and narrow (never the full roster) so a non-Admin can search for
    /// someone to grant a permission/role to without the Admin-only GET /api/users listing.</summary>
    public async Task<IReadOnlyList<AppUser>> SearchAsync(string query, int limit, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var like = $"%{query}%";
        var top = _db.Provider == DbProvider.Sqlite ? "" : $"TOP {limit} ";
        var tail = _db.Provider == DbProvider.Sqlite ? $" LIMIT {limit}" : "";
        var rows = await connection.QueryAsync<AppUser>(
            $"SELECT {top}* FROM {Table} WHERE IsActive = 1 AND " +
            "(LOWER(Username) LIKE LOWER(@like) OR LOWER(DisplayName) LIKE LOWER(@like)) " +
            $"ORDER BY Username{tail}",
            new { like, limit });
        return rows.ToList();
    }

    public async Task<AppUser> CreateAsync(AppUser user, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        user.Id = Guid.NewGuid().ToString("N");
        user.CreatedAt = DateTime.UtcNow;

        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"INSERT INTO {Table} (Id, Username, DisplayName, PasswordHash, AuthMethod, Role, IsActive, " +
            "AllowAllSystems, AllowedSystemsJson, AllowAllFiles, AllowedFilesJson, CreatedAt) " +
            "VALUES (@Id, @Username, @DisplayName, @PasswordHash, @AuthMethod, @Role, @IsActive, " +
            "@AllowAllSystems, @AllowedSystemsJson, @AllowAllFiles, @AllowedFilesJson, @CreatedAt)",
            user);
        _logger.LogInformation("User {Username} created ({AuthMethod}, role {Role})", user.Username, user.AuthMethod, user.Role);
        return user;
    }

    public async Task UpdateAsync(AppUser user, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"UPDATE {Table} SET DisplayName = @DisplayName, PasswordHash = @PasswordHash, " +
            "AuthMethod = @AuthMethod, Role = @Role, IsActive = @IsActive, AllowAllSystems = @AllowAllSystems, " +
            "AllowedSystemsJson = @AllowedSystemsJson, AllowAllFiles = @AllowAllFiles, " +
            "AllowedFilesJson = @AllowedFilesJson WHERE Id = @Id",
            user);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"DELETE FROM {Table} WHERE Id = @id", new { id });
    }

    public static UserInfo ToInfo(AppUser user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        DisplayName = user.DisplayName,
        AuthMethod = user.AuthMethod,
        Role = user.Role,
        IsActive = user.IsActive,
        AllowAllSystems = user.AllowAllSystems,
        AllowedSystems = Deserialize(user.AllowedSystemsJson),
        AllowAllFiles = user.AllowAllFiles,
        AllowedFiles = Deserialize(user.AllowedFilesJson),
        CreatedAt = user.CreatedAt,
    };

    private static List<string> Deserialize(string? json) =>
        string.IsNullOrWhiteSpace(json) ? new() : JsonSerializer.Deserialize<List<string>>(json) ?? new();
}
