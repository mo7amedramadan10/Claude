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
                     // SaaS hierarchy (Platform → Organization → Project) — see AppUser's remarks.
                     ("OrganizationId", "TEXT", "NVARCHAR(64)"),
                     ("IsPlatformOwner", "INTEGER", "BIT"),
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
            // Error 2705: "Column names in each table must be unique" — the wording SQL
            // Server actually uses for a duplicate ADD COLUMN never contains "already", so
            // matching on the message (as this used to) never caught it; the column was
            // already present (e.g. a fresh DB whose CREATE TABLE included it directly) and
            // this ALTER was a no-op migration step that should be silently skipped.
            catch (SqlException ex) when (ex.Number == 2705)
            {
            }
        }

        // IsPlatformOwner is non-nullable on AppUser — a row from before this column existed
        // reads back as NULL otherwise, which Dapper can't cast to bool.
        await using (var platformOwnerBackfill = connection.CreateCommand())
        {
            platformOwnerBackfill.CommandText = $"UPDATE {Table} SET IsPlatformOwner = 0 WHERE IsPlatformOwner IS NULL";
            await platformOwnerBackfill.ExecuteNonQueryAsync(ct);
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

        // Migration for a row saved before file access moved to be per-file only (see
        // UsersController.Create/Update, which now always force AllowAllFiles=true on any
        // write) — an existing row can still carry AllowAllFiles=0 from before that change,
        // which would keep blocking a user from even their own uploads (AllowsFile stayed
        // false for every file, including ones they created themselves) until they happened
        // to be edited again. Runs unconditionally, not just WHERE NULL like the migration
        // above, since the value to fix here is an explicit 0, not a missing column.
        await using (var filesBackfill = connection.CreateCommand())
        {
            filesBackfill.CommandText =
                $"UPDATE {Table} SET AllowAllFiles = 1, AllowedFilesJson = '[]' WHERE AllowAllFiles = 0 OR AllowAllFiles IS NULL";
            await filesBackfill.ExecuteNonQueryAsync(ct);
        }

        await RelaxUsernameUniquenessAsync(connection, ct);
    }

    /// <summary>
    /// Username used to be globally UNIQUE at the database level (see the CREATE TABLE above,
    /// which still shows the original column — that clause is now dead on any table this
    /// migration has already run against). That's wrong for a multi-tenant deployment: two
    /// different organizations each wanting an "admin" account could never coexist. Username
    /// now only needs to be unique *within* an organization (see
    /// FindByUsernameInOrganizationAsync and AuthController's /o/{slug}-scoped login), so this
    /// replaces the single-column constraint with a composite one on (OrganizationId,
    /// Username).
    ///
    /// SQLite has no ALTER TABLE ... DROP CONSTRAINT, so the only way to actually remove a
    /// column-level UNIQUE is the standard SQLite pattern: rebuild the table without it, copy
    /// every row across, swap it in. Guarded by inspecting sqlite_master's stored CREATE TABLE
    /// text so this only ever runs once per database, not on every startup. SQL Server's
    /// inline UNIQUE became an auto-named constraint, discoverable (but not guessable) via
    /// sys.key_constraints — dropped by that discovered name, then replaced by a filtered
    /// unique index (filtered so legacy rows with OrganizationId still NULL, pre-backfill,
    /// never collide with each other under the new index).
    /// </summary>
    private async Task RelaxUsernameUniquenessAsync(System.Data.Common.DbConnection connection, CancellationToken ct)
    {
        if (_db.Provider == DbProvider.Sqlite)
        {
            var createSql = await connection.ExecuteScalarAsync<string?>(
                "SELECT sql FROM sqlite_master WHERE type = 'table' AND name = 'AppUsers'");
            if (createSql is null || !createSql.Contains("\"Username\" TEXT UNIQUE", StringComparison.Ordinal))
                return; // already rebuilt, or a brand-new table that never had the old clause

            await using var transaction = await connection.BeginTransactionAsync(ct);
            try
            {
                await using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = (SqliteTransaction)transaction;
                    cmd.CommandText = """
                        CREATE TABLE "AppUsers_rebuild" (
                          "Id" TEXT PRIMARY KEY, "Username" TEXT, "DisplayName" TEXT,
                          "PasswordHash" TEXT, "AuthMethod" TEXT, "Role" TEXT, "IsActive" INTEGER,
                          "AllowAllSystems" INTEGER, "AllowedSystemsJson" TEXT,
                          "AllowAllFiles" INTEGER, "AllowedFilesJson" TEXT, "CreatedAt" TEXT,
                          "OrganizationId" TEXT, "IsPlatformOwner" INTEGER)
                        """;
                    await cmd.ExecuteNonQueryAsync(ct);
                }
                await using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = (SqliteTransaction)transaction;
                    cmd.CommandText = """
                        INSERT INTO "AppUsers_rebuild"
                          (Id, Username, DisplayName, PasswordHash, AuthMethod, Role, IsActive,
                           AllowAllSystems, AllowedSystemsJson, AllowAllFiles, AllowedFilesJson, CreatedAt,
                           OrganizationId, IsPlatformOwner)
                        SELECT Id, Username, DisplayName, PasswordHash, AuthMethod, Role, IsActive,
                               AllowAllSystems, AllowedSystemsJson, AllowAllFiles, AllowedFilesJson, CreatedAt,
                               OrganizationId, IsPlatformOwner
                        FROM "AppUsers"
                        """;
                    await cmd.ExecuteNonQueryAsync(ct);
                }
                await using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = (SqliteTransaction)transaction;
                    cmd.CommandText = "DROP TABLE \"AppUsers\"";
                    await cmd.ExecuteNonQueryAsync(ct);
                }
                await using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = (SqliteTransaction)transaction;
                    cmd.CommandText = "ALTER TABLE \"AppUsers_rebuild\" RENAME TO \"AppUsers\"";
                    await cmd.ExecuteNonQueryAsync(ct);
                }
                await using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = (SqliteTransaction)transaction;
                    // SQLite treats each NULL as distinct under a UNIQUE index (unlike SQL
                    // Server), so pre-backfill rows with OrganizationId still NULL never
                    // collide with each other here — no WHERE filter needed.
                    cmd.CommandText = "CREATE UNIQUE INDEX \"idx_appusers_org_username\" ON \"AppUsers\" (\"OrganizationId\", \"Username\")";
                    await cmd.ExecuteNonQueryAsync(ct);
                }
                await transaction.CommitAsync(ct);
                _logger.LogInformation("Migrated AppUsers.Username from a globally-unique to an organization-scoped unique constraint.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Failed to relax AppUsers.Username's uniqueness constraint — usernames remain globally unique until this is retried.");
            }
        }
        else
        {
            try
            {
                await using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = """
                        DECLARE @constraintName NVARCHAR(200);
                        SELECT @constraintName = kc.name
                        FROM sys.key_constraints kc
                        JOIN sys.index_columns ic ON ic.object_id = kc.parent_object_id AND ic.index_id = kc.unique_index_id
                        JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
                        WHERE kc.parent_object_id = OBJECT_ID('staging.AppUsers') AND c.name = 'Username' AND kc.type = 'UQ';
                        IF @constraintName IS NOT NULL
                          EXEC('ALTER TABLE staging.AppUsers DROP CONSTRAINT [' + @constraintName + ']');
                        """;
                    await cmd.ExecuteNonQueryAsync(ct);
                }
                await using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = """
                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_appusers_org_username' AND object_id = OBJECT_ID('staging.AppUsers'))
                          CREATE UNIQUE INDEX idx_appusers_org_username ON staging.AppUsers (OrganizationId, Username) WHERE OrganizationId IS NOT NULL;
                        """;
                    await cmd.ExecuteNonQueryAsync(ct);
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Failed to relax AppUsers.Username's uniqueness constraint — usernames remain globally unique until this is retried.");
            }
        }
    }

    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {Table}");
    }

    public async Task<int> CountByOrganizationAsync(string organizationId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM {Table} WHERE OrganizationId = @organizationId", new { organizationId });
    }

    public async Task<IReadOnlyList<AppUser>> ListByOrganizationAsync(string organizationId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<AppUser>(
            $"SELECT * FROM {Table} WHERE OrganizationId = @organizationId ORDER BY Username", new { organizationId });
        return rows.ToList();
    }

    /// <summary>Attaches every account that predates the SaaS hierarchy (OrganizationId still
    /// NULL/empty) to <paramref name="organizationId"/> — run once at startup (see Program.cs)
    /// so an existing install's users/data land in the organization created to hold them.
    /// Any such account that's an Admin also becomes the platform owner: before this
    /// migration they administered the whole (single-tenant) system, so post-upgrade they
    /// should still be able to see/manage every organization, not just this default one.</summary>
    public async Task BackfillMissingOrganizationAsync(string organizationId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"UPDATE {Table} SET OrganizationId = @organizationId, " +
            "IsPlatformOwner = CASE WHEN Role = 'Admin' THEN 1 ELSE IsPlatformOwner END " +
            "WHERE OrganizationId IS NULL OR OrganizationId = ''",
            new { organizationId });
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

    /// <summary>Global, cross-organization lookup — safe only where there is genuinely no
    /// organization context yet (Program.cs's own fixed-name seed/demo accounts) or where
    /// ambiguity across organizations is acceptable. Username is no longer unique
    /// system-wide (see RelaxUsernameUniquenessAsync), so this can return any one of several
    /// same-named accounts in different organizations — every other caller (login, in-org
    /// uniqueness checks, the user directory) uses FindByUsernameInOrganizationAsync instead.</summary>
    public async Task<AppUser?> FindByUsernameAsync(string username, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<AppUser>(
            $"SELECT * FROM {Table} WHERE LOWER(Username) = LOWER(@username)", new { username });
    }

    /// <summary>The one real lookup login/account-creation use — scoped to a single
    /// organization, so "admin" in one organization never matches "admin" in another.</summary>
    public async Task<AppUser?> FindByUsernameInOrganizationAsync(
        string organizationId, string username, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<AppUser>(
            $"SELECT * FROM {Table} WHERE OrganizationId = @organizationId AND LOWER(Username) = LOWER(@username)",
            new { organizationId, username });
    }

    /// <summary>Active users, within <paramref name="organizationId"/> only, whose username or
    /// display name contains <paramref name="query"/> — capped small and narrow (never the
    /// full roster) so a non-Admin can search for someone to grant a permission/role to
    /// without the Admin-only GET /api/users listing, and without reaching into another
    /// organization's accounts now that usernames aren't globally unique.</summary>
    public async Task<IReadOnlyList<AppUser>> SearchAsync(
        string organizationId, string query, int limit, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var like = $"%{query}%";
        var top = _db.Provider == DbProvider.Sqlite ? "" : $"TOP {limit} ";
        var tail = _db.Provider == DbProvider.Sqlite ? $" LIMIT {limit}" : "";
        var rows = await connection.QueryAsync<AppUser>(
            $"SELECT {top}* FROM {Table} WHERE IsActive = 1 AND OrganizationId = @organizationId AND " +
            "(LOWER(Username) LIKE LOWER(@like) OR LOWER(DisplayName) LIKE LOWER(@like)) " +
            $"ORDER BY Username{tail}",
            new { organizationId, like, limit });
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
            "AllowAllSystems, AllowedSystemsJson, AllowAllFiles, AllowedFilesJson, CreatedAt, " +
            "OrganizationId, IsPlatformOwner) " +
            "VALUES (@Id, @Username, @DisplayName, @PasswordHash, @AuthMethod, @Role, @IsActive, " +
            "@AllowAllSystems, @AllowedSystemsJson, @AllowAllFiles, @AllowedFilesJson, @CreatedAt, " +
            "@OrganizationId, @IsPlatformOwner)",
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
        OrganizationId = user.OrganizationId,
        IsPlatformOwner = user.IsPlatformOwner,
    };

    private static List<string> Deserialize(string? json) =>
        string.IsNullOrWhiteSpace(json) ? new() : JsonSerializer.Deserialize<List<string>>(json) ?? new();
}
