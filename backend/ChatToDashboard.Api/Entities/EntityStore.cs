using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Entities;

/// <summary>Projects, one level under Organizations — see Entity's remarks. Same shared-DB
/// Dapper pattern as every other store here.</summary>
public class EntityStore
{
    private readonly DataStore _db;

    public EntityStore(DataStore db) => _db = db;

    private string Table => "[dbo].[Entities]";
    private string RolesTable => "[dbo].[EntityRoles]";

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);

        var text = $"""
               IF OBJECT_ID('dbo.Entities') IS NULL
               CREATE TABLE {Table} (
                 [Id] NVARCHAR(64) PRIMARY KEY, [OrganizationId] NVARCHAR(64), [Name] NVARCHAR(200),
                 [Slug] NVARCHAR(200), [CreatedAt] DATETIME2)
               """;

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = text;
            await command.ExecuteNonQueryAsync(ct);
        }

        // ترحيل: لا عمود هنا مضمون الوجود بافتراض — جدول Projects المشترك هذا ظهر عمليًا
        // بشكل مختلف تمامًا عن هذا الكود (حتى Id/Name/CreatedAt غابا في الإنتاج رغم كونهما
        // من صلب التعريف الأول)، فكل عمود يُضاف دفاعيًا كعمود عادي (بلا PRIMARY KEY).
        await AddColumnIfMissingAsync(connection, "Id", sqlServer: "NVARCHAR(64)", ct);
        await AddColumnIfMissingAsync(connection, "OrganizationId", sqlServer: "NVARCHAR(64)", ct);
        await AddColumnIfMissingAsync(connection, "Name", sqlServer: "NVARCHAR(200)", ct);
        await AddColumnIfMissingAsync(connection, "Slug", sqlServer: "NVARCHAR(200)", ct);
        await AddColumnIfMissingAsync(connection, "CreatedAt", sqlServer: "DATETIME2", ct);
        await AddColumnIfMissingAsync(connection, "TypeId", sqlServer: "NVARCHAR(64)", ct);

        // Owner/Editor/Viewer roles on a project — mirrors DashboardRoles' shape (see its
        // own remarks), except Owner lives here too since a Entity has no OwnerId column.
        var rolesText = $"""
               IF OBJECT_ID('dbo.EntityRoles') IS NULL
               CREATE TABLE {RolesTable} (
                 [EntityId] NVARCHAR(64) NOT NULL, [UserId] NVARCHAR(200) NOT NULL, [Role] NVARCHAR(20) NOT NULL,
                 PRIMARY KEY ([EntityId], [UserId]))
               """;
        await using (var rolesCommand = connection.CreateCommand())
        {
            rolesCommand.CommandText = rolesText;
            await rolesCommand.ExecuteNonQueryAsync(ct);
        }
    }

    private async Task AddColumnIfMissingAsync(System.Data.Common.DbConnection connection, string column, string sqlServer, CancellationToken ct)
    {
        try
        {
            await using var alter = connection.CreateCommand();
            alter.CommandText = $"ALTER TABLE {Table} ADD [{column}] {sqlServer}";
            await alter.ExecuteNonQueryAsync(ct);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2705) { }
    }

    public async Task<int> CountForOrganizationAsync(string organizationId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM {Table} WHERE OrganizationId = @organizationId", new { organizationId });
    }

    public async Task<IReadOnlyList<Entity>> ListByOrganizationAsync(string organizationId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<Entity>(
            $"SELECT * FROM {Table} WHERE OrganizationId = @organizationId ORDER BY CreatedAt", new { organizationId });
        return rows.ToList();
    }

    public async Task<Entity?> FindByIdAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<Entity>($"SELECT * FROM {Table} WHERE Id = @id", new { id });
    }

    /// <summary>The organization's first (default) project — what the existing single-project
    /// chat/dashboard workspace runs against until it's made project-switchable.</summary>
    public async Task<Entity?> FirstForOrganizationAsync(string organizationId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<Entity>(
            $"SELECT TOP 1 * FROM {Table} WHERE OrganizationId = @organizationId ORDER BY CreatedAt", new { organizationId });
    }

    /// <summary>
    /// Resolves "the caller's current project" the one consistent way every place that scopes
    /// data by project needs it — originally HistoryController's own CurrentEntityIdAsync,
    /// pulled out here so RepositoryStore/IntegrationStore scoping (via
    /// PermissionsService.GetEffectiveSelectionAsync/SourceSelection.EntityId) and every
    /// controller that touches files, integrations or dashboards resolve it identically. An
    /// Admin always gets their org's first (default) project — unchanged legacy behavior, since
    /// an Admin manages the whole org, not one project. A plain User gets the first project
    /// EntityRoles names them on, falling back to the org's first project for an account with
    /// no explicit role yet (every pre-existing account before EntityRoles existed).
    /// </summary>
    public async Task<string?> ResolveCurrentEntityIdAsync(
        string? organizationId, string userId, bool isAdmin, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(organizationId)) return null;
        if (!isAdmin)
        {
            var owned = await ListForUserAsync(userId, organizationId, ct);
            if (owned.Count > 0) return owned[0].Id;
        }
        var project = await FirstForOrganizationAsync(organizationId, ct);
        return project?.Id;
    }

    public async Task<Entity> CreateAsync(string organizationId, string name, CancellationToken ct = default, string? typeId = null)
    {
        await EnsureSchemaAsync(ct);
        var project = new Entity
        {
            Id = Guid.NewGuid().ToString("N"),
            OrganizationId = organizationId,
            Name = name,
            Slug = await UniqueSlugAsync(organizationId, name, ct),
            CreatedAt = DateTime.UtcNow,
            TypeId = typeId,
        };

        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"INSERT INTO {Table} (Id, OrganizationId, Name, Slug, CreatedAt, TypeId) " +
            "VALUES (@Id, @OrganizationId, @Name, @Slug, @CreatedAt, @TypeId)",
            project);
        return project;
    }

    /// <summary>تعديل اسم الكيان و/أو نوعه (الاسم المختصر للرابط لا يتغيّر حتى لا تنكسر الروابط القائمة).</summary>
    public async Task UpdateAsync(string id, string name, string? typeId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {Table} SET Name = @name, TypeId = @typeId WHERE Id = @id", new { id, name, typeId });
    }

    /// <summary>عدد كيانات المنظمة بهذا النوع صراحة — حذف نوع مستخدَم مرفوض.</summary>
    public async Task<int> CountByTypeAsync(string organizationId, string typeId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {Table} WHERE OrganizationId = @organizationId AND TypeId = @typeId", new { organizationId, typeId });
    }

    public async Task<IReadOnlyList<EntityRoleEntry>> ListRolesAsync(string entityId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<EntityRoleEntry>(
            $"SELECT EntityId, UserId, Role FROM {RolesTable} WHERE EntityId = @entityId", new { entityId });
        return rows.ToList();
    }

    /// <summary>Every project (within <paramref name="organizationId"/>) this user holds any
    /// role on — what narrows a plain "User" account's project list/current-project down from
    /// "every project in the org" (the Admin/platform-owner view) to just their own.</summary>
    public async Task<IReadOnlyList<Entity>> ListForUserAsync(string userId, string organizationId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<Entity>(
            $"SELECT p.* FROM {Table} p JOIN {RolesTable} r ON r.EntityId = p.Id " +
            "WHERE r.UserId = @userId AND p.OrganizationId = @organizationId ORDER BY p.CreatedAt",
            new { userId, organizationId });
        return rows.ToList();
    }

    public async Task SetRoleAsync(string entityId, string userId, string role, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"DELETE FROM {RolesTable} WHERE EntityId = @entityId AND UserId = @userId", new { entityId, userId });
        await connection.ExecuteAsync(
            $"INSERT INTO {RolesTable} (EntityId, UserId, Role) VALUES (@entityId, @userId, @role)",
            new { entityId, userId, role });
    }

    public async Task RemoveRoleAsync(string entityId, string userId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"DELETE FROM {RolesTable} WHERE EntityId = @entityId AND UserId = @userId", new { entityId, userId });
    }

    private async Task<string> UniqueSlugAsync(string organizationId, string name, CancellationToken ct)
    {
        var baseSlug = Slugify(name);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var slug = baseSlug;
        var suffix = 1;
        while (await connection.ExecuteScalarAsync<int>(
                   $"SELECT COUNT(*) FROM {Table} WHERE OrganizationId = @organizationId AND Slug = @slug",
                   new { organizationId, slug }) > 0)
            slug = $"{baseSlug}-{++suffix}";
        return slug;
    }

    private static string Slugify(string name)
    {
        var cleaned = new string(name.Trim().ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray());
        while (cleaned.Contains("--")) cleaned = cleaned.Replace("--", "-");
        cleaned = cleaned.Trim('-');
        return cleaned.Length > 0 ? cleaned : "project";
    }
}
