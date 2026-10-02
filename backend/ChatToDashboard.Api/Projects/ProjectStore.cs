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

        await using var command = connection.CreateCommand();
        command.CommandText = text;
        await command.ExecuteNonQueryAsync(ct);
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
