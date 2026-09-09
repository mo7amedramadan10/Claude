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

    private string RolesTable => _db.Provider == DbProvider.Sqlite
        ? "\"DashboardRoles\""
        : "[staging].[DashboardRoles]";

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
                     ("IsActive", "INTEGER", "BIT"),
                     ("OwnerId", "TEXT", "NVARCHAR(200)"),
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

        // Editor/Viewer roles on Active dashboards — mirrors repo_FilePermissions' shape.
        // The Owner is not in here; it's DashboardHistory.OwnerId.
        var rolesText = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {RolesTable} (
                 "DashboardId" TEXT NOT NULL, "UserId" TEXT NOT NULL, "Role" TEXT NOT NULL,
                 PRIMARY KEY ("DashboardId", "UserId"))
               """
            : $"""
               IF OBJECT_ID('staging.DashboardRoles') IS NULL
               CREATE TABLE {RolesTable} (
                 [DashboardId] NVARCHAR(64) NOT NULL, [UserId] NVARCHAR(200) NOT NULL, [Role] NVARCHAR(20) NOT NULL,
                 PRIMARY KEY ([DashboardId], [UserId]))
               """;
        await using (var rolesCommand = connection.CreateCommand())
        {
            rolesCommand.CommandText = rolesText;
            await rolesCommand.ExecuteNonQueryAsync(ct);
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

    /// <summary>
    /// The user's own Draft entries, plus every Active dashboard they own or hold an
    /// Editor/Viewer role on — newest first, capped at <paramref name="limit"/>. A Draft
    /// stays exactly as private as before this feature; only Active dashboards are ever
    /// visible to someone other than their creator.
    /// </summary>
    public async Task<IReadOnlyList<DashboardHistoryEntry>> ListAsync(
        string userId, int limit = MaxPerUser, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var top = _db.Provider == DbProvider.Sqlite ? "" : $"TOP {limit} ";
        var tail = _db.Provider == DbProvider.Sqlite ? $" LIMIT {limit}" : "";
        // COALESCE covers rows saved before FiltersJson/ActiveFiltersJson/IsActive existed —
        // the migration in EnsureSchemaAsync adds the columns as NULL on old rows.
        var rows = await connection.QueryAsync<DashboardHistoryEntry>(
            $"SELECT {top}t.Id, t.UserId, t.Question, t.QueryDescription, t.Summary, t.WidgetsJson, " +
            "COALESCE(t.FiltersJson, '[]') AS FiltersJson, COALESCE(t.ActiveFiltersJson, '{}') AS ActiveFiltersJson, " +
            "COALESCE(t.IsActive, 0) AS IsActive, t.OwnerId, t.CreatedAt " +
            $"FROM {Table} t WHERE (COALESCE(t.IsActive, 0) = 0 AND t.UserId = @userId) " +
            $"OR (COALESCE(t.IsActive, 0) = 1 AND (t.OwnerId = @userId " +
            $"OR EXISTS (SELECT 1 FROM {RolesTable} r WHERE r.DashboardId = t.Id AND r.UserId = @userId))) " +
            $"ORDER BY t.CreatedAt DESC{tail}",
            new { userId });
        return rows.ToList();
    }

    /// <summary>Fetches one entry regardless of who owns it — callers must check the
    /// requester's role themselves (see DashboardAccessService).</summary>
    public async Task<DashboardHistoryEntry?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<DashboardHistoryEntry>(
            "SELECT Id, UserId, Question, QueryDescription, Summary, WidgetsJson, " +
            "COALESCE(FiltersJson, '[]') AS FiltersJson, COALESCE(ActiveFiltersJson, '{}') AS ActiveFiltersJson, " +
            $"COALESCE(IsActive, 0) AS IsActive, OwnerId, CreatedAt FROM {Table} WHERE Id = @id",
            new { id });
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

    /// <summary>Same as <see cref="UpdateAsync"/> but without the creator-only filter — for an
    /// Active dashboard's Owner or an Editor, whose access was already checked by the caller
    /// via DashboardAccessService (they may not be the original creator after an ownership
    /// transfer).</summary>
    public async Task UpdateContentAsync(
        string id, string summary, string widgetsJson,
        string filtersJson, string activeFiltersJson, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"UPDATE {Table} SET Summary = @summary, QueryDescription = @summary, WidgetsJson = @widgetsJson, " +
            "FiltersJson = @filtersJson, ActiveFiltersJson = @activeFiltersJson " +
            "WHERE Id = @id",
            new { id, summary, widgetsJson, filtersJson, activeFiltersJson });
    }

    /// <summary>Renames a dashboard's title and description — a lightweight edit distinct from
    /// <see cref="UpdateContentAsync"/>'s full content autosave, so it never touches
    /// widgets/filters. Permission is checked by the caller (see HistoryController.Rename).</summary>
    public async Task RenameAsync(string id, string question, string summary, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"UPDATE {Table} SET Question = @question, Summary = @summary, QueryDescription = @summary WHERE Id = @id",
            new { id, question, summary });
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

    /// <summary>Deletes one entry unconditionally — the caller (an Active dashboard's Owner,
    /// or an Admin) already had their access checked via DashboardAccessService.</summary>
    public async Task DeleteUnfilteredAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"DELETE FROM {Table} WHERE Id = @id", new { id });
        await connection.ExecuteAsync($"DELETE FROM {RolesTable} WHERE DashboardId = @id", new { id });
    }

    public async Task ClearAsync(string userId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"DELETE FROM {Table} WHERE UserId = @userId", new { userId });
    }

    /// <summary>Promotes a Draft to Active, stamping <paramref name="ownerId"/> as its Owner.
    /// The caller must already have checked the promoting user's data permission.</summary>
    public async Task ActivateAsync(string id, string ownerId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"UPDATE {Table} SET IsActive = 1, OwnerId = @ownerId WHERE Id = @id",
            new { id, ownerId });
    }

    /// <summary>Reassigns the Owner of an Active dashboard. The caller must already have
    /// checked the new owner's eligibility (valid permission on every dependency).</summary>
    public async Task TransferOwnerAsync(string id, string newOwnerId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"UPDATE {Table} SET OwnerId = @newOwnerId WHERE Id = @id", new { id, newOwnerId });
    }

    /// <summary>All Editor/Viewer role rows for one dashboard.</summary>
    public async Task<IReadOnlyList<DashboardRoleEntry>> ListRolesAsync(string dashboardId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<DashboardRoleEntry>(
            $"SELECT DashboardId, UserId, Role FROM {RolesTable} WHERE DashboardId = @dashboardId",
            new { dashboardId });
        return rows.ToList();
    }

    /// <summary>This user's Editor/Viewer role (if any) on each of the given dashboards.
    /// Ownership is not covered here — check DashboardHistoryEntry.OwnerId separately.</summary>
    public async Task<IReadOnlyDictionary<string, string>> GetMyRolesAsync(
        string userId, IReadOnlyCollection<string> dashboardIds, CancellationToken ct = default)
    {
        if (dashboardIds.Count == 0) return new Dictionary<string, string>();
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<DashboardRoleEntry>(
            $"SELECT DashboardId, UserId, Role FROM {RolesTable} WHERE UserId = @userId AND DashboardId IN @dashboardIds",
            new { userId, dashboardIds });
        return rows.ToDictionary(r => r.DashboardId, r => r.Role);
    }

    /// <summary>"owner" | "editor" | "viewer" | null for a Draft or an Active dashboard the
    /// given user has no standing on. Shared by HistoryController (the wizard/autosave path's
    /// Owner-only new-data-source guard) and ChatController (the identical guard on the
    /// chat-continuation path — see ChatController.Post) so both ask the exact same
    /// question the exact same way.</summary>
    public async Task<string?> ResolveRoleAsync(string userId, DashboardHistoryEntry entry, CancellationToken ct = default)
    {
        if (!entry.IsActive) return entry.UserId == userId ? "owner" : null;
        if (entry.OwnerId == userId) return "owner";
        var roles = await GetMyRolesAsync(userId, new[] { entry.Id }, ct);
        return roles.GetValueOrDefault(entry.Id);
    }

    public async Task SetRoleAsync(string dashboardId, string userId, string role, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"DELETE FROM {RolesTable} WHERE DashboardId = @dashboardId AND UserId = @userId",
            new { dashboardId, userId });
        await connection.ExecuteAsync(
            $"INSERT INTO {RolesTable} (DashboardId, UserId, Role) VALUES (@dashboardId, @userId, @role)",
            new { dashboardId, userId, role });
    }

    public async Task RemoveRoleAsync(string dashboardId, string userId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"DELETE FROM {RolesTable} WHERE DashboardId = @dashboardId AND UserId = @userId",
            new { dashboardId, userId });
    }
}
