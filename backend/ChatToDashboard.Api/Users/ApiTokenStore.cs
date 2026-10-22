using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Users;

/// <summary>مفتاح وصول شخصي (لربط Claude Desktop وأي أداة خارجية بالمنصة) — يُعرض للمستخدم مرة واحدة
/// عند إنشائه، ولا يُخزَّن منه إلا تجزئة SHA-256 وبادئته للعرض. يرث صلاحيات صاحبه كاملة.</summary>
public class ApiToken
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("userId")] public string UserId { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonIgnore] public string TokenHash { get; set; } = "";
    /// <summary>أول 12 حرفًا من المفتاح (مثل <c>jeem_a1b2c3d4</c>) للتعرّف عليه في القائمة.</summary>
    [JsonPropertyName("prefix")] public string Prefix { get; set; } = "";
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }
    [JsonPropertyName("lastUsedAt")] public DateTime? LastUsedAt { get; set; }
    [JsonPropertyName("revokedAt")] public DateTime? RevokedAt { get; set; }
}

public class ApiTokenStore
{
    public const string TokenPrefix = "jeem_";
    private readonly DataStore _db;
    private readonly SemaphoreSlim _schemaGate = new(1, 1);
    private volatile bool _schemaReady;

    public ApiTokenStore(DataStore db) => _db = db;

    private string Table => _db.Provider == DbProvider.Sqlite ? "\"ApiTokens\"" : "[staging].[ApiTokens]";

    private async Task EnsureSchemaOnceAsync(CancellationToken ct)
    {
        if (_schemaReady) return;
        await _schemaGate.WaitAsync(ct);
        try { if (!_schemaReady) { await EnsureSchemaAsync(ct); _schemaReady = true; } }
        finally { _schemaGate.Release(); }
    }

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);
        var text = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {Table} (
                 "Id" TEXT PRIMARY KEY, "UserId" TEXT NOT NULL, "Name" TEXT NOT NULL, "TokenHash" TEXT NOT NULL UNIQUE,
                 "Prefix" TEXT NOT NULL, "CreatedAt" TEXT NOT NULL, "LastUsedAt" TEXT, "RevokedAt" TEXT)
               """
            : $"""
               IF OBJECT_ID('staging.ApiTokens') IS NULL
               CREATE TABLE {Table} (
                 [Id] NVARCHAR(32) PRIMARY KEY, [UserId] NVARCHAR(200) NOT NULL, [Name] NVARCHAR(200) NOT NULL, [TokenHash] NVARCHAR(64) NOT NULL UNIQUE,
                 [Prefix] NVARCHAR(20) NOT NULL, [CreatedAt] DATETIME2 NOT NULL, [LastUsedAt] DATETIME2, [RevokedAt] DATETIME2)
               """;
        await using var command = connection.CreateCommand();
        command.CommandText = text;
        await command.ExecuteNonQueryAsync(ct);
    }

    /// <summary>ينشئ مفتاحًا جديدًا ويعيد نصه الكامل (المرة الوحيدة التي يتوفر فيها) مع سجله.</summary>
    public async Task<(string Token, ApiToken Record)> CreateAsync(string userId, string name, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        var token = TokenPrefix + Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();
        var record = new ApiToken
        {
            Id = Convert.ToHexString(RandomNumberGenerator.GetBytes(8)).ToLowerInvariant(),
            UserId = userId, Name = name.Trim(), TokenHash = Hash(token), Prefix = token[..12], CreatedAt = DateTime.UtcNow,
        };
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"INSERT INTO {Table} (Id, UserId, Name, TokenHash, Prefix, CreatedAt) VALUES (@Id, @UserId, @Name, @TokenHash, @Prefix, @CreatedAt)", record);
        return (token, record);
    }

    /// <summary>يتحقق من مفتاح وارد ويعيد معرّف صاحبه (أو null إن كان غير موجود أو مُلغى)، ويحدّث آخر استخدام.</summary>
    public async Task<string?> ResolveUserIdAsync(string token, CancellationToken ct = default)
    {
        if (!token.StartsWith(TokenPrefix, StringComparison.Ordinal)) return null;
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var hash = Hash(token);
        var row = await connection.QuerySingleOrDefaultAsync<ApiToken>(
            $"SELECT Id, UserId, Name, Prefix, CreatedAt, LastUsedAt, RevokedAt FROM {Table} WHERE TokenHash = @hash", new { hash });
        if (row is null || row.RevokedAt is not null) return null;
        // آخر استخدام يُحدَّث بحبيبة دقيقة حتى لا يكتب كل طلب.
        if (row.LastUsedAt is null || DateTime.UtcNow - row.LastUsedAt > TimeSpan.FromMinutes(1))
            await connection.ExecuteAsync($"UPDATE {Table} SET LastUsedAt = @now WHERE Id = @id", new { id = row.Id, now = DateTime.UtcNow });
        return row.UserId;
    }

    public async Task<IReadOnlyList<ApiToken>> ListAsync(string userId, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<ApiToken>(
            $"SELECT Id, UserId, Name, Prefix, CreatedAt, LastUsedAt, RevokedAt FROM {Table} WHERE UserId = @userId ORDER BY CreatedAt DESC", new { userId });
        return rows.ToList();
    }

    public async Task<bool> RevokeAsync(string userId, string id, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.ExecuteAsync($"UPDATE {Table} SET RevokedAt = @now WHERE Id = @id AND UserId = @userId AND RevokedAt IS NULL",
            new { id, userId, now = DateTime.UtcNow }) > 0;
    }

    private static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
}
