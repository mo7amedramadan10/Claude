using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Presentations;

/// <summary>قيمة واحدة في مخزن بيانات العرض: مفتاح ← JSON برقم إصدار للتعارض.</summary>
public sealed record PresentationDataEntry(string Key, string Json, long Version, DateTime UpdatedAt, string? UpdatedBy);

/// <summary>
/// «بيانات العرض»: مخزن صغير لكل عرض منشور (مفتاح ← JSON) تقرأه الصفحة المرفوعة وتكتبه عبر الجسر
/// <c>jeem-page.js</c>، فتبقى الصفحة ثابتة وبياناتها القابلة للتعديل (مهام، قرارات، جداول…) في القاعدة.
/// رقم الإصدار يمنع أن يمسح تعديلٌ تعديلًا متزامنًا: الكتابة بإصدار قديم تُرفض (409).
/// </summary>
public class PresentationDataStore
{
    public const int MaxKeys = 50;
    public const int MaxJsonBytes = 256 * 1024;
    private readonly DataStore _db;
    private readonly SemaphoreSlim _schemaGate = new(1, 1);
    private volatile bool _schemaReady;

    public PresentationDataStore(DataStore db) => _db = db;

    private string Table => "[dbo].[PresentationData]";

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
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            IF OBJECT_ID('dbo.PresentationData') IS NULL
            CREATE TABLE {Table} (
              [PresentationId] NVARCHAR(32) NOT NULL, [Key] NVARCHAR(100) NOT NULL, [Json] NVARCHAR(MAX) NOT NULL,
              [Version] BIGINT NOT NULL, [UpdatedAt] DATETIME2 NOT NULL, [UpdatedBy] NVARCHAR(200),
              CONSTRAINT PK_PresentationData PRIMARY KEY ([PresentationId], [Key]))
            """;
        await command.ExecuteNonQueryAsync(ct);
    }

    /// <summary>مفتاح صالح: حروف وأرقام وشرطات ونقاط، 1–100 حرفًا.</summary>
    public static bool IsValidKey(string? key) =>
        key is { Length: >= 1 and <= 100 } && key.All(c => char.IsLetterOrDigit(c) || c is '-' or '_' or '.');

    public async Task<PresentationDataEntry?> GetAsync(string presentationId, string key, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<PresentationDataEntry>(
            $"SELECT [Key], [Json], [Version], [UpdatedAt], [UpdatedBy] FROM {Table} WHERE PresentationId = @presentationId AND [Key] = @key",
            new { presentationId, key });
    }

    public async Task<IReadOnlyList<(string Key, long Version, DateTime UpdatedAt, string? UpdatedBy, int Bytes)>> ListAsync(string presentationId, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<(string Key, long Version, DateTime UpdatedAt, string? UpdatedBy, int Bytes)>(
            $"SELECT [Key], [Version], [UpdatedAt], [UpdatedBy], DATALENGTH([Json]) / 2 AS Bytes FROM {Table} WHERE PresentationId = @presentationId ORDER BY [Key]",
            new { presentationId });
        return rows.ToList();
    }

    /// <summary>كتابة بإصدار متوقع: null = بلا تحقق (إنشاء أو استبدال)، وإلا يجب أن يساوي الإصدار الحالي
    /// (0 لمفتاح غير موجود). يعيد الإصدار الجديد، أو null عند التعارض.</summary>
    public async Task<(bool Ok, long Version, string? Error)> SetAsync(string presentationId, string key, string json, long? expectedVersion, string? updatedBy, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var current = await connection.QuerySingleOrDefaultAsync<long?>(
            $"SELECT [Version] FROM {Table} WHERE PresentationId = @presentationId AND [Key] = @key", new { presentationId, key });
        if (expectedVersion is not null && (current ?? 0) != expectedVersion)
            return (false, current ?? 0, "تعارض: عُدِّلت هذه البيانات من مكان آخر — أعد التحميل ثم كرّر التعديل.");
        if (current is null)
        {
            var count = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {Table} WHERE PresentationId = @presentationId", new { presentationId });
            if (count >= MaxKeys) return (false, 0, $"تجاوز عدد المفاتيح الحد ({MaxKeys}).");
        }
        var next = (current ?? 0) + 1;
        var now = DateTime.UtcNow;
        var affected = current is null
            ? await connection.ExecuteAsync($"INSERT INTO {Table} (PresentationId, [Key], [Json], [Version], [UpdatedAt], [UpdatedBy]) VALUES (@presentationId, @key, @json, @next, @now, @updatedBy)",
                new { presentationId, key, json, next, now, updatedBy })
            : await connection.ExecuteAsync($"UPDATE {Table} SET [Json] = @json, [Version] = @next, [UpdatedAt] = @now, [UpdatedBy] = @updatedBy WHERE PresentationId = @presentationId AND [Key] = @key AND [Version] = @current",
                new { presentationId, key, json, next, now, updatedBy, current });
        return affected == 1 ? (true, next, null) : (false, current ?? 0, "تعارض: عُدِّلت هذه البيانات من مكان آخر — أعد التحميل ثم كرّر التعديل.");
    }

    public async Task DeleteAsync(string presentationId, string key, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"DELETE FROM {Table} WHERE PresentationId = @presentationId AND [Key] = @key", new { presentationId, key });
    }

    public async Task DeleteAllAsync(string presentationId, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"DELETE FROM {Table} WHERE PresentationId = @presentationId", new { presentationId });
    }
}
