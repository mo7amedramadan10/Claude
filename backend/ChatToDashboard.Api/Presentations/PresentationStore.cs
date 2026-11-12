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

    private string Table => "[dbo].[Presentations]";

    private const string Columns =
        "Id, OrganizationId, CreatedByUserId, CreatedByName, Title, Slug, FileName, SizeBytes, CreatedAt, UpdatedAt, ExpiresAt, RevokedAt, COALESCE(ViewCount, 0) AS ViewCount, COALESCE(EditPolicy, 'anyone') AS EditPolicy, SourceKind, SourceId, COALESCE(LiveData, 0) AS LiveData, COALESCE(Assistant, 1) AS Assistant, COALESCE(AssistantPolicy, 'anyone') AS AssistantPolicy";

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
        var text = $"""
               IF OBJECT_ID('dbo.Presentations') IS NULL
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
        // ترحيل: لا عمود هنا مضمون الوجود بافتراض — جداول هذه القاعدة المشتركة ظهرت عمليًا
        // بأشكال مختلفة تمامًا عن هذا الكود حتى في أعمدة من صلب التعريف الأول (انظر
        // Organizations/ProjectStore)، فكل عمود يُضاف دفاعيًا — بلا PRIMARY KEY/UNIQUE/
        // NOT NULL حتى لو كان معرَّفًا هكذا في CREATE (قيمة افتراضية لكل عمود NOT NULL تكفي
        // للإضافة بأمان على صفوف موجودة مسبقًا).
        await AddColumnIfMissingAsync(connection, "Id", sqlServer: "NVARCHAR(32)", ct);
        await AddColumnIfMissingAsync(connection, "OrganizationId", sqlServer: "NVARCHAR(64)", ct);
        await AddColumnIfMissingAsync(connection, "CreatedByUserId", sqlServer: "NVARCHAR(200) NOT NULL DEFAULT ''", ct);
        await AddColumnIfMissingAsync(connection, "CreatedByName", sqlServer: "NVARCHAR(200)", ct);
        await AddColumnIfMissingAsync(connection, "Title", sqlServer: "NVARCHAR(300) NOT NULL DEFAULT ''", ct);
        await AddColumnIfMissingAsync(connection, "Slug", sqlServer: "NVARCHAR(100)", ct);
        await AddColumnIfMissingAsync(connection, "FileName", sqlServer: "NVARCHAR(300) NOT NULL DEFAULT ''", ct);
        await AddColumnIfMissingAsync(connection, "Html", sqlServer: "NVARCHAR(MAX) NOT NULL DEFAULT ''", ct);
        await AddColumnIfMissingAsync(connection, "SizeBytes", sqlServer: "BIGINT NOT NULL DEFAULT 0", ct);
        await AddColumnIfMissingAsync(connection, "CreatedAt", sqlServer: "DATETIME2 NOT NULL DEFAULT '1970-01-01'", ct);
        await AddColumnIfMissingAsync(connection, "UpdatedAt", sqlServer: "DATETIME2 NOT NULL DEFAULT '1970-01-01'", ct);
        await AddColumnIfMissingAsync(connection, "ExpiresAt", sqlServer: "DATETIME2", ct);
        await AddColumnIfMissingAsync(connection, "RevokedAt", sqlServer: "DATETIME2", ct);
        await AddColumnIfMissingAsync(connection, "ViewCount", sqlServer: "INT NOT NULL DEFAULT 0", ct);
        await AddColumnIfMissingAsync(connection, "EditPolicy", sqlServer: "NVARCHAR(16)", ct);
        await AddColumnIfMissingAsync(connection, "SourceKind", sqlServer: "NVARCHAR(16)", ct);
        await AddColumnIfMissingAsync(connection, "SourceId", sqlServer: "NVARCHAR(64)", ct);
        await AddColumnIfMissingAsync(connection, "LiveData", sqlServer: "BIT NOT NULL DEFAULT 0", ct);
        await AddColumnIfMissingAsync(connection, "Assistant", sqlServer: "BIT NOT NULL DEFAULT 1", ct);
        await AddColumnIfMissingAsync(connection, "AssistantPolicy", sqlServer: "NVARCHAR(16)", ct);
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

    /// <summary>ربط العرض بمصدره داخل المنصة (تصميم حر / عرض) وحالة البيانات الحيّة.</summary>
    public async Task SetSourceAsync(string id, string? sourceKind, string? sourceId, bool liveData, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {Table} SET SourceKind = @sourceKind, SourceId = @sourceId, LiveData = @liveData WHERE Id = @id", new { id, sourceKind, sourceId, liveData });
    }

    public async Task SetLiveDataAsync(string id, bool on, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {Table} SET LiveData = @on, UpdatedAt = @now WHERE Id = @id", new { id, on, now = DateTime.UtcNow });
    }

    /// <summary>إعدادات مساعد «جيم» للعرض: مفعَّل؟ ومن يستطيع سؤاله.</summary>
    public async Task SetAssistantAsync(string id, bool enabled, string policy, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {Table} SET Assistant = @enabled, AssistantPolicy = @policy WHERE Id = @id", new { id, enabled, policy });
    }

    public async Task SetEditPolicyAsync(string id, string policy, CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {Table} SET EditPolicy = @policy, UpdatedAt = @now WHERE Id = @id", new { id, policy, now = DateTime.UtcNow });
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
