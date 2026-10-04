using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Projects;

/// <summary>Projects, one level under Organizations — see Project's remarks. Same shared-DB
/// Dapper pattern as every other store here.</summary>
public class ProjectStore
{
    private readonly DataStore _db;

    public ProjectStore(DataStore db) => _db = db;

    private string Table => _db.Provider == DbProvider.Sqlite ? "\"Projects\"" : "[staging].[Projects]";
    private string RolesTable => _db.Provider == DbProvider.Sqlite ? "\"ProjectRoles\"" : "[staging].[ProjectRoles]";

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);

        var text = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {Table} (
                 "Id" TEXT PRIMARY KEY, "OrganizationId" TEXT, "Name" TEXT, "Slug" TEXT, "CreatedAt" TEXT)
               """
            : $"""
               IF OBJECT_ID('staging.Projects') IS NULL
               CREATE TABLE {Table} (
                 [Id] NVARCHAR(64) PRIMARY KEY, [OrganizationId] NVARCHAR(64), [Name] NVARCHAR(200),
                 [Slug] NVARCHAR(200), [CreatedAt] DATETIME2)
               """;

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = text;
            await command.ExecuteNonQueryAsync(ct);
        }

        // Owner/Editor/Viewer roles on a project — mirrors DashboardRoles' shape (see its
        // own remarks), except Owner lives here too since a Project has no OwnerId column.
        var rolesText = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {RolesTable} (
                 "ProjectId" TEXT NOT NULL, "UserId" TEXT NOT NULL, "Role" TEXT NOT NULL,
                 PRIMARY KEY ("ProjectId", "UserId"))
               """
            : $"""
               IF OBJECT_ID('staging.ProjectRoles') IS NULL
               CREATE TABLE {RolesTable} (
                 [ProjectId] NVARCHAR(64) NOT NULL, [UserId] NVARCHAR(200) NOT NULL, [Role] NVARCHAR(20) NOT NULL,
                 PRIMARY KEY ([ProjectId], [UserId]))
               """;
        await using (var rolesCommand = connection.CreateCommand())
        {
            rolesCommand.CommandText = rolesText;
            await rolesCommand.ExecuteNonQueryAsync(ct);
        }
    }

    public async Task<int> CountForOrganizationAsync(string organizationId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM {Table} WHERE OrganizationId = @organizationId", new { organizationId });
    }

    public async Task<IReadOnlyList<Project>> ListByOrganizationAsync(string organizationId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<Project>(
            $"SELECT * FROM {Table} WHERE OrganizationId = @organizationId ORDER BY CreatedAt", new { organizationId });
        return rows.ToList();
    }

    public async Task<Project?> FindByIdAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<Project>($"SELECT * FROM {Table} WHERE Id = @id", new { id });
    }

    /// <summary>The organization's first (default) project — what the existing single-project
    /// chat/dashboard workspace runs against until it's made project-switchable.</summary>
    public async Task<Project?> FirstForOrganizationAsync(string organizationId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var top = _db.Provider == DbProvider.Sqlite ? "" : "TOP 1 ";
        var tail = _db.Provider == DbProvider.Sqlite ? " LIMIT 1" : "";
        return await connection.QuerySingleOrDefaultAsync<Project>(
            $"SELECT {top}* FROM {Table} WHERE OrganizationId = @organizationId ORDER BY CreatedAt{tail}", new { organizationId });
    }

    public async Task<Project> CreateAsync(string organizationId, string name, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        var project = new Project
        {
            Id = Guid.NewGuid().ToString("N"),
            OrganizationId = organizationId,
            Name = name,
            Slug = await UniqueSlugAsync(organizationId, name, ct),
            CreatedAt = DateTime.UtcNow,
        };

        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"INSERT INTO {Table} (Id, OrganizationId, Name, Slug, CreatedAt) " +
            "VALUES (@Id, @OrganizationId, @Name, @Slug, @CreatedAt)",
            project);
        return project;
    }

    public async Task<IReadOnlyList<ProjectRoleEntry>> ListRolesAsync(string projectId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<ProjectRoleEntry>(
            $"SELECT ProjectId, UserId, Role FROM {RolesTable} WHERE ProjectId = @projectId", new { projectId });
        return rows.ToList();
    }

    /// <summary>Every project (within <paramref name="organizationId"/>) this user holds any
    /// role on — what narrows a plain "User" account's project list/current-project down from
    /// "every project in the org" (the Admin/platform-owner view) to just their own.</summary>
    public async Task<IReadOnlyList<Project>> ListForUserAsync(string userId, string organizationId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<Project>(
            $"SELECT p.* FROM {Table} p JOIN {RolesTable} r ON r.ProjectId = p.Id " +
            "WHERE r.UserId = @userId AND p.OrganizationId = @organizationId ORDER BY p.CreatedAt",
            new { userId, organizationId });
        return rows.ToList();
    }

    public async Task SetRoleAsync(string projectId, string userId, string role, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"DELETE FROM {RolesTable} WHERE ProjectId = @projectId AND UserId = @userId", new { projectId, userId });
        await connection.ExecuteAsync(
            $"INSERT INTO {RolesTable} (ProjectId, UserId, Role) VALUES (@projectId, @userId, @role)",
            new { projectId, userId, role });
    }

    public async Task RemoveRoleAsync(string projectId, string userId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"DELETE FROM {RolesTable} WHERE ProjectId = @projectId AND UserId = @userId", new { projectId, userId });
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
