using System.Security.Cryptography;
using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Presentations;

/// <summary>تخزين العروض التقديمية (HTML كامل في قاعدة البيانات) — انظر <see cref="Presentation"/>.</summary>
public class PresentationStore
{
    private readonly DataStore _db;
    private readonly SemaphoreSlim _schemaGate = new(1, 1);
    private volatile bool _schemaReady;

    public PresentationStore(DataStore db) => _db = db;

    private string Table => _db.Provider == DbProvider.Sqlite ? "\"Presentations\"" : "[staging].[Presentations]";

    private const string Columns =
        "Id, OrganizationId, CreatedByUserId, CreatedByName, Title, Slug, FileName, SizeBytes, CreatedAt, UpdatedAt, ExpiresAt, RevokedAt, COALESCE(ViewCount, 0) AS ViewCount";

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
                 "Id" TEXT PRIMARY KEY, "OrganizationId" TEXT, "CreatedByUserId" TEXT NOT NULL, "CreatedByName" TEXT,
                 "Title" TEXT NOT NULL, "Slug" TEXT, "FileName" TEXT NOT NULL, "Html" TEXT NOT NULL, "SizeBytes" INTEGER NOT NULL,
                 "CreatedAt" TEXT NOT NULL, "UpdatedAt" TEXT NOT NULL, "ExpiresAt" TEXT, "RevokedAt" TEXT, "ViewCount" INTEGER NOT NULL DEFAULT 0)
               """
            : $"""
               IF OBJECT_ID('staging.Presentations') IS NULL
               CREATE TABLE {Table} (
                 [Id] NVARCHAR(32) PRIMARY KEY, [OrganizationId] NVARCHAR(64), [CreatedByUserId] NVARCHAR(200) NOT NULL, [CreatedByName] NVARCHAR(200),
                 [Title] NVARCHAR(300) NOT NULL, [Slug] NVARCHAR(100), [FileName] NVARCHAR(300) NOT NULL, [Html] NVARCHAR(MAX) NOT NULL, [SizeBytes] BIGINT NOT NULL,
                 [CreatedAt] DATETIME2 NOT NULL, [UpdatedAt] DATETIME2 NOT NULL, [ExpiresAt] DATETIME2, [RevokedAt] DATETIME2, [ViewCount] INT NOT NULL DEFAULT 0)
               """;
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = text;
            await command.ExecuteNonQueryAsync(ct);
        }
        // ترحيل: عمود الاسم في الرابط (Slug) لجداول أُنشئت قبل وجوده — فريد داخل المنظمة (يُفرض في الكود).
        try
        {
            await using var alter = connection.CreateCommand();
            alter.CommandText = _db.Provider == DbProvider.Sqlite
                ? $"ALTER TABLE {Table} ADD COLUMN \"Slug\" TEXT"
                : $"ALTER TABLE {Table} ADD [Slug] NVARCHAR(100)";
            await alter.ExecuteNonQueryAsync(ct);
        }
        catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.Message.Contains("duplicate column", StringComparison.OrdinalIgnoreCase)) { }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2705) { }
    }

    /// <summary>هل الاسم مستخدم داخل المنظمة (باستثناء عرض معيّن عند إعادة التسمية)؟</summary>
    public async Task<bool> SlugTakenAsync(string? organizationId, string slug, string? exceptId = null, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var sql = organizationId is null
            ? $"SELECT COUNT(*) FROM {Table} WHERE OrganizationId IS NULL AND LOWER(Slug) = LOWER(@slug) AND (@exceptId IS NULL OR Id <> @exceptId)"
            : $"SELECT COUNT(*) FROM {Table} WHERE OrganizationId = @organizationId AND LOWER(Slug) = LOWER(@slug) AND (@exceptId IS NULL OR Id <> @exceptId)";
        return await connection.ExecuteScalarAsync<int>(sql, new { organizationId, slug, exceptId }) > 0;
    }

    public async Task<Presentation?> GetBySlugAsync(string organizationId, string slug, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<Presentation>(
            $"SELECT {Columns} FROM {Table} WHERE OrganizationId = @organizationId AND LOWER(Slug) = LOWER(@slug)", new { organizationId, slug });
    }

    public async Task SetSlugAsync(string id, string slug, CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {Table} SET Slug = @slug, UpdatedAt = @now WHERE Id = @id", new { id, slug, now = DateTime.UtcNow });
    }

    public async Task<Presentation> SaveAsync(Presentation p, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        p.Id = NewId();
        p.CreatedAt = p.UpdatedAt = DateTime.UtcNow;
        p.ViewCount = 0;
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"INSERT INTO {Table} (Id, OrganizationId, CreatedByUserId, CreatedByName, Title, Slug, FileName, Html, SizeBytes, CreatedAt, UpdatedAt, ExpiresAt, RevokedAt, ViewCount) " +
            "VALUES (@Id, @OrganizationId, @CreatedByUserId, @CreatedByName, @Title, @Slug, @FileName, @Html, @SizeBytes, @CreatedAt, @UpdatedAt, @ExpiresAt, @RevokedAt, 0)", p);
        return p;
    }

    /// <summary>البيانات الوصفية بلا المحتوى.</summary>
    public async Task<Presentation?> GetAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<Presentation>($"SELECT {Columns} FROM {Table} WHERE Id = @id", new { id });
    }

    public async Task<string?> GetHtmlAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.ExecuteScalarAsync<string?>($"SELECT Html FROM {Table} WHERE Id = @id", new { id });
    }

    public async Task<IReadOnlyList<Presentation>> ListAsync(string? organizationId, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = organizationId is null
            ? await connection.QueryAsync<Presentation>($"SELECT {Columns} FROM {Table} WHERE OrganizationId IS NULL ORDER BY CreatedAt DESC")
            : await connection.QueryAsync<Presentation>($"SELECT {Columns} FROM {Table} WHERE OrganizationId = @organizationId ORDER BY CreatedAt DESC", new { organizationId });
        return rows.ToList();
    }

    /// <summary>استبدال المحتوى (نسخة أحدث على نفس الرابط) مع تحديث الاسم والحجم والعنوان إن أُعطي.</summary>
    public async Task ReplaceAsync(string id, string html, string fileName, string? title, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"UPDATE {Table} SET Html = @html, FileName = @fileName, SizeBytes = @size, UpdatedAt = @now, Title = COALESCE(@title, Title) WHERE Id = @id",
            new { id, html, fileName, size = (long)System.Text.Encoding.UTF8.GetByteCount(html), now = DateTime.UtcNow, title });
    }

    public async Task SetExpiryAsync(string id, DateTime? expiresAt, CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {Table} SET ExpiresAt = @expiresAt, RevokedAt = NULL, UpdatedAt = @now WHERE Id = @id", new { id, expiresAt, now = DateTime.UtcNow });
    }

    public async Task RevokeAsync(string id, CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {Table} SET RevokedAt = @now WHERE Id = @id AND RevokedAt IS NULL", new { id, now = DateTime.UtcNow });
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"DELETE FROM {Table} WHERE Id = @id", new { id });
    }

    public async Task IncrementViewCountAsync(string id, CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {Table} SET ViewCount = COALESCE(ViewCount, 0) + 1 WHERE Id = @id", new { id });
    }

    private static string NewId()
    {
        Span<byte> bytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
