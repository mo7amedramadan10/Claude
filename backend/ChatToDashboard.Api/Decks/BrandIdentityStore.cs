using System.Text.Json;
using System.Text.Json.Serialization;
using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Decks;

/// <summary>هوية بصرية محفوظة باسم في مكتبة هويات المشروع (بجانب الهوية الافتراضية في <see cref="DeckBrandStore"/>):
/// تُختار لتصميم بعينه في «تصميم حر» فيُعاد تلوينه وخطّه وشعاره بحسبها.</summary>
public class BrandIdentity
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("brand")] public DeckBrand Brand { get; set; } = new();
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updatedAt")] public DateTime UpdatedAt { get; set; }
}

/// <summary>جدول <c>dbo.BrandIdentities</c>: هويات مسمّاة لكل مشروع، الهوية كاملة JSON بشكل <see cref="DeckBrand"/> (بلا الخطوط المرفوعة التي تبقى للمشروع).</summary>
public class BrandIdentityStore
{
    public const int MaxPerEntity = 30;
    private readonly DataStore _db;
    private readonly SemaphoreSlim _schemaGate = new(1, 1);
    private volatile bool _schemaReady;
    public BrandIdentityStore(DataStore db) => _db = db;
    private const string Table = "[dbo].[BrandIdentities]";

    private async Task EnsureSchemaAsync(CancellationToken ct)
    {
        if (_schemaReady) return;
        await _schemaGate.WaitAsync(ct);
        try
        {
            if (_schemaReady) return;
            await using var connection = await _db.OpenConnectionAsync(ct);
            await _db.CreateContainerIfMissingAsync(connection, ct);
            await connection.ExecuteAsync($"""
                IF OBJECT_ID('dbo.BrandIdentities') IS NULL
                CREATE TABLE {Table} (
                  [Id] NVARCHAR(32) PRIMARY KEY, [EntityKey] NVARCHAR(64) NOT NULL, [OrganizationId] NVARCHAR(64), [Name] NVARCHAR(120) NOT NULL,
                  [BrandJson] NVARCHAR(MAX) NOT NULL, [CreatedBy] NVARCHAR(200), [CreatedAt] DATETIME2 NOT NULL, [UpdatedAt] DATETIME2 NOT NULL);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BrandIdentities_Project') CREATE INDEX IX_BrandIdentities_Project ON {Table} ([EntityKey]);
                """);
            _schemaReady = true;
        }
        finally { _schemaGate.Release(); }
    }

    private static string Key(string? entityId) => string.IsNullOrEmpty(entityId) ? "-" : entityId;
    private static readonly JsonSerializerOptions Json = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    private static BrandIdentity Map(dynamic r)
    {
        DeckBrand brand;
        try { brand = JsonSerializer.Deserialize<DeckBrand>((string)r.BrandJson) ?? new DeckBrand(); } catch (JsonException) { brand = new DeckBrand(); }
        return new BrandIdentity { Id = r.Id, Name = r.Name, Brand = brand, CreatedAt = DateTime.SpecifyKind((DateTime)r.CreatedAt, DateTimeKind.Utc), UpdatedAt = DateTime.SpecifyKind((DateTime)r.UpdatedAt, DateTimeKind.Utc) };
    }

    public async Task<List<BrandIdentity>> ListAsync(string? entityId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync($"SELECT Id, Name, BrandJson, CreatedAt, UpdatedAt FROM {Table} WHERE EntityKey = @k ORDER BY CreatedAt", new { k = Key(entityId) });
        return rows.Select(r => (BrandIdentity)Map(r)).ToList();
    }

    public async Task<BrandIdentity?> GetAsync(string? entityId, string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var r = await connection.QuerySingleOrDefaultAsync($"SELECT Id, Name, BrandJson, CreatedAt, UpdatedAt FROM {Table} WHERE EntityKey = @k AND Id = @id", new { k = Key(entityId), id });
        return r is null ? null : Map(r);
    }

    public async Task<int> CountAsync(string? entityId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {Table} WHERE EntityKey = @k", new { k = Key(entityId) });
    }

    public async Task<BrandIdentity> AddAsync(string? entityId, string? organizationId, string userId, string name, DeckBrand brand, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        var now = DateTime.UtcNow; brand.FontFaces = null; brand.UpdatedAt = null;
        var it = new BrandIdentity { Id = Guid.NewGuid().ToString("N")[..16], Name = name, Brand = brand, CreatedAt = now, UpdatedAt = now };
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"INSERT INTO {Table} (Id, EntityKey, OrganizationId, Name, BrandJson, CreatedBy, CreatedAt, UpdatedAt) VALUES (@Id, @k, @organizationId, @Name, @json, @userId, @now, @now)",
            new { it.Id, k = Key(entityId), organizationId, it.Name, json = JsonSerializer.Serialize(brand, Json), userId, now });
        return it;
    }

    public async Task<bool> UpdateAsync(string? entityId, string id, string name, DeckBrand brand, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        brand.FontFaces = null; brand.UpdatedAt = null;
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.ExecuteAsync($"UPDATE {Table} SET Name = @name, BrandJson = @json, UpdatedAt = @now WHERE EntityKey = @k AND Id = @id",
            new { k = Key(entityId), id, name, json = JsonSerializer.Serialize(brand, Json), now = DateTime.UtcNow }) > 0;
    }

    public async Task<bool> DeleteAsync(string? entityId, string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.ExecuteAsync($"DELETE FROM {Table} WHERE EntityKey = @k AND Id = @id", new { k = Key(entityId), id }) > 0;
    }
}
