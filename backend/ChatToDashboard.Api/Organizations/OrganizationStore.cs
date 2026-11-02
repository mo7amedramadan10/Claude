using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Organizations;

/// <summary>Platform-level tenants. Same shared-DB/Dapper pattern as every other store here
/// (see Users.UserStore) — a new table in the one database, not a separate database per
/// tenant (see the class remarks on Organization for why).</summary>
public class OrganizationStore
{
    private readonly DataStore _db;

    public OrganizationStore(DataStore db) => _db = db;

    private string Table => "[dbo].[Organizations]";

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);

        var text = $"""
               IF OBJECT_ID('dbo.Organizations') IS NULL
               CREATE TABLE {Table} (
                 [Id] NVARCHAR(64) PRIMARY KEY, [Name] NVARCHAR(200), [Slug] NVARCHAR(200) UNIQUE,
                 [IsActive] BIT, [CreatedAt] DATETIME2)
               """;

        await using var command = connection.CreateCommand();
        command.CommandText = text;
        await command.ExecuteNonQueryAsync(ct);

        // ترحيل: لا عمود هنا مضمون الوجود بافتراض — الدليل الحي أن جدول Organizations
        // المشترك هذا سبق وجوده بشكل مختلف تمامًا عن هذا الكود (حتى Id/Name/CreatedAt
        // ظهرا كأعمدة غير موجودة في الإنتاج رغم كونهما من صلب التعريف الأول) فكل عمود
        // يُضاف دفاعيًا كعمود عادي (بلا PRIMARY KEY ولا UNIQUE، وتفرّد Slug محسوم في الكود عبر UniqueSlugAsync).
        await AddColumnIfMissingAsync(connection, "Id", sqlServer: "NVARCHAR(64)", ct);
        await AddColumnIfMissingAsync(connection, "Name", sqlServer: "NVARCHAR(200)", ct);
        await AddColumnIfMissingAsync(connection, "Slug", sqlServer: "NVARCHAR(200)", ct);
        await AddColumnIfMissingAsync(connection, "IsActive", sqlServer: "BIT", ct);
        await AddColumnIfMissingAsync(connection, "CreatedAt", sqlServer: "DATETIME2", ct);
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

    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {Table}");
    }

    /// <summary>The platform's very first organization — the one existing data/users are
    /// backfilled into on first run (see Program.cs). Only meaningful when exactly that
    /// seeding scenario applies; for everything else use <see cref="ListAsync"/>.</summary>
    public async Task<Organization?> FirstAsync(CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<Organization>(
            $"SELECT TOP 1 * FROM {Table} ORDER BY CreatedAt");
    }

    public async Task<IReadOnlyList<Organization>> ListAsync(CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<Organization>($"SELECT * FROM {Table} ORDER BY CreatedAt");
        return rows.ToList();
    }

    public async Task<Organization?> FindByIdAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<Organization>($"SELECT * FROM {Table} WHERE Id = @id", new { id });
    }

    /// <summary>Resolves the organization a /o/{slug} link names — the public, anonymous-safe
    /// half of login (see AuthController.OrganizationBySlug/Login): the slug and name are
    /// treated as public (an organization's own identity, shown on its login screen before
    /// any credential is entered), unlike everything else in this store.</summary>
    public async Task<Organization?> FindBySlugAsync(string slug, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<Organization>(
            $"SELECT * FROM {Table} WHERE LOWER(Slug) = LOWER(@slug)", new { slug });
    }

    public async Task<Organization> CreateAsync(string name, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        var org = new Organization
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = name,
            Slug = await UniqueSlugAsync(name, ct),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"INSERT INTO {Table} (Id, Name, Slug, IsActive, CreatedAt) VALUES (@Id, @Name, @Slug, @IsActive, @CreatedAt)",
            org);
        return org;
    }

    public async Task SetActiveAsync(string id, bool isActive, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {Table} SET IsActive = @isActive WHERE Id = @id", new { id, isActive });
    }

    private async Task<string> UniqueSlugAsync(string name, CancellationToken ct)
    {
        var baseSlug = Slugify(name);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var slug = baseSlug;
        var suffix = 1;
        while (await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {Table} WHERE Slug = @slug", new { slug }) > 0)
            slug = $"{baseSlug}-{++suffix}";
        return slug;
    }

    private static string Slugify(string name)
    {
        var cleaned = new string(name.Trim().ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray());
        while (cleaned.Contains("--")) cleaned = cleaned.Replace("--", "-");
        cleaned = cleaned.Trim('-');
        return cleaned.Length > 0 ? cleaned : "org";
    }
}
