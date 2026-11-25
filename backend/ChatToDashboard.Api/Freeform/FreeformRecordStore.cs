using System.Text.Json;
using System.Text.Json.Serialization;
using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Freeform;

/// <summary>سجل واحد أدخلته صفحة تصميم حر عبر <c>jeem.records(name)</c>: صف في مجموعة (الطلبات، الردود…) بحقوله JSON ومن أدخله ومتى.</summary>
public sealed class FreeformRecord
{
    public long Id { get; set; }
    public string DesignId { get; set; } = "";
    public string Collection { get; set; } = "";
    public string Json { get; set; } = "{}";
    public string? CreatedByUserId { get; set; }
    public string? CreatedByName { get; set; }
    public string? AnonId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedByName { get; set; }
}

/// <summary>ملخص مجموعة سجلات في تصميم.</summary>
public sealed class FreeformRecordCollection
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("count")] public int Count { get; set; }
    [JsonPropertyName("lastAt")] public DateTime? LastAt { get; set; }
}

/// <summary>من يُدخل البيانات في صفحة التصميم المنشورة: members (أعضاء المنظمة المسجَّلون — الافتراضي) أو public (أي من يفتح الرابط،
/// بحد للإدخالات). داخل المنصة (المحرر) يُدخل من يملك الوصول إلى التصميم دائمًا.</summary>
public static class FreeformRecordPolicy
{
    public const string Members = "members";
    public const string Public = "public";
    public static bool IsValid(string? p) => p is Members or Public;
    public static string Normalize(string? p) => p == Public ? Public : Members;
}

/// <summary>
/// قواعد الوصول إلى السجلات (نقية، مختبَرة). «العضو» = مسجَّل من منظمة التصميم. «المدير» = من يملك تعديل التصميم (صاحبه، المسؤول،
/// أو من شورك معه بالتعديل). الإدخال: داخل المنصة أي من يرى التصميم؛ في الرابط العام بحسب السياسة. القراءة: العضو يرى كل السجلات،
/// وغيره (زائر سياسة عامة) يرى ما أدخله هو فقط. التعديل والحذف: صاحب السجل أو المدير.
/// </summary>
public static class FreeformRecordAccess
{
    public sealed record Viewer(string? UserId, string? OrganizationId, string? AnonId, bool Manager, bool InPlatform);

    public static bool IsMember(Viewer v, FreeformDesign d) =>
        v.UserId is not null && (v.InPlatform || string.Equals(v.OrganizationId, d.OrganizationId, StringComparison.Ordinal));

    /// <summary>null = مسموح؛ وإلا (الحالة، الرسالة).</summary>
    public static (int Status, string Error)? CanAdd(Viewer v, FreeformDesign d)
    {
        if (v.InPlatform || v.Manager || IsMember(v, d)) return null;
        if (FreeformRecordPolicy.Normalize(d.RecordsPolicy) == FreeformRecordPolicy.Public) return null;
        return v.UserId is null ? (401, "سجّل الدخول بحساب منظمتك لإدخال البيانات في هذه الصفحة.") : (403, "إدخال البيانات في هذه الصفحة لأعضاء المنظمة فقط.");
    }

    /// <summary>هل يرى كل السجلات؟ (وإلا ما أدخله هو فقط.)</summary>
    public static bool SeesAll(Viewer v, FreeformDesign d) => v.Manager || IsMember(v, d);

    public static bool Owns(Viewer v, FreeformRecord r) =>
        (v.UserId is not null && string.Equals(r.CreatedByUserId, v.UserId, StringComparison.OrdinalIgnoreCase))
        || (v.UserId is null && r.CreatedByUserId is null && v.AnonId is not null && string.Equals(r.AnonId, v.AnonId, StringComparison.Ordinal));

    public static bool CanChange(Viewer v, FreeformRecord r) => v.Manager || Owns(v, r);

    /// <summary>معرّف الزائر المجهول: نص عشوائي يولّده متصفحه (16–64 حرفًا سداسيًا/شرطة)؛ غير ذلك يُهمل.</summary>
    public static string? NormalizeAnon(string? anon) =>
        anon is { Length: >= 16 and <= 64 } && anon.All(c => char.IsAsciiHexDigit(c) || c == '-') ? anon.ToLowerInvariant() : null;
}

/// <summary>
/// جدول <c>dbo.FreeformRecords</c>: سجلات الإدخال لكل تصميم حر ومجموعة. الإضافة صف جديد فلا تعارض بين مُدخِلين متزامنين
/// (بخلاف <c>jeem.store</c> الذي يكتب قيمة واحدة لكل مفتاح). مستقل عن النشر: المحرر والرابط المنشور يقرآن ويكتبان البيانات نفسها.
/// </summary>
public class FreeformRecordStore
{
    public const int MaxCollections = 20;
    public const int MaxRecordsPerDesign = 50_000;
    public const int MaxRecordBytes = 32 * 1024;
    public const int MaxFields = 100;
    private const string T = "[dbo].[FreeformRecords]";
    private readonly DataStore _db;
    private readonly SemaphoreSlim _schemaGate = new(1, 1);
    private volatile bool _schemaReady;
    public FreeformRecordStore(DataStore db) => _db = db;

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
                IF OBJECT_ID('dbo.FreeformRecords') IS NULL
                CREATE TABLE {T} (
                  [Id] BIGINT IDENTITY PRIMARY KEY, [DesignId] NVARCHAR(32) NOT NULL, [Collection] NVARCHAR(100) NOT NULL, [Json] NVARCHAR(MAX) NOT NULL,
                  [CreatedByUserId] NVARCHAR(200) NULL, [CreatedByName] NVARCHAR(200) NULL, [AnonId] NVARCHAR(64) NULL,
                  [CreatedAt] DATETIME2 NOT NULL, [UpdatedAt] DATETIME2 NOT NULL, [UpdatedByName] NVARCHAR(200) NULL);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FreeformRecords_Design') CREATE INDEX IX_FreeformRecords_Design ON {T} ([DesignId], [Collection], [Id]);
                """);
            _schemaReady = true;
        }
        finally { _schemaGate.Release(); }
    }

    /// <summary>اسم مجموعة صالح: حروف بأي لغة وأرقام وشرطة وشرطة سفلية ونقطة، 1–60 حرفًا.</summary>
    public static bool IsValidCollection(string? name) =>
        name is { Length: >= 1 and <= 60 } && name.All(c => char.IsLetterOrDigit(c) || c is '-' or '_' or '.');

    /// <summary>يتحقق من قيمة السجل: كائن JSON بحقول ≤ <see cref="MaxFields"/> وأسماء ≤ 100 حرف وحجم ≤ <see cref="MaxRecordBytes"/>. يعيد null أو رسالة الخطأ.</summary>
    public static string? Validate(JsonElement data)
    {
        if (data.ValueKind != JsonValueKind.Object) return "السجل يجب أن يكون كائنًا بحقول، مثل {\"الاسم\": \"…\", \"العدد\": 3}.";
        var n = 0;
        foreach (var p in data.EnumerateObject())
        {
            if (++n > MaxFields) return $"عدد الحقول يتجاوز الحد ({MaxFields}).";
            if (p.Name.Length is 0 or > 100) return "اسم حقل فارغ أو أطول من 100 حرف.";
        }
        if (System.Text.Encoding.UTF8.GetByteCount(data.GetRawText()) > MaxRecordBytes) return $"حجم السجل يتجاوز الحد ({MaxRecordBytes / 1024} كيلوبايت).";
        return null;
    }

    /// <summary>دمج حقول التعديل فوق السجل (الحقل بقيمة null يُحذف).</summary>
    public static string Merge(string currentJson, JsonElement patch)
    {
        var node = System.Text.Json.Nodes.JsonNode.Parse(currentJson) as System.Text.Json.Nodes.JsonObject ?? new System.Text.Json.Nodes.JsonObject();
        foreach (var p in patch.EnumerateObject())
        {
            if (p.Value.ValueKind == JsonValueKind.Null) node.Remove(p.Name);
            else node[p.Name] = System.Text.Json.Nodes.JsonNode.Parse(p.Value.GetRawText());
        }
        return node.ToJsonString();
    }

    public async Task<IReadOnlyList<FreeformRecordCollection>> CollectionsAsync(string designId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var c = await _db.OpenConnectionAsync(ct);
        return (await c.QueryAsync<FreeformRecordCollection>(
            $"SELECT [Collection] AS Name, COUNT(*) AS [Count], MAX([CreatedAt]) AS LastAt FROM {T} WHERE DesignId = @designId GROUP BY [Collection] ORDER BY [Collection]",
            new { designId })).ToList();
    }

    /// <summary>صفحة سجلات بالترتيب التصاعدي (بعد <paramref name="afterId"/>) أو التنازلي (قبل <paramref name="beforeId"/>)؛ <paramref name="ownerUserId"/>/
    /// <paramref name="ownerAnon"/> يقصرانها على سجلات صاحبها. يعيد limit+1 ليُعرف إن كان هناك المزيد.</summary>
    public async Task<(IReadOnlyList<FreeformRecord> Items, int Total)> ListAsync(string designId, string collection, int limit, bool desc, long? cursor,
        bool onlyOwn, string? ownerUserId, string? ownerAnon, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var c = await _db.OpenConnectionAsync(ct);
        var own = !onlyOwn ? "" : ownerUserId is not null ? " AND CreatedByUserId = @ownerUserId" : ownerAnon is not null ? " AND CreatedByUserId IS NULL AND AnonId = @ownerAnon" : " AND 1 = 0";
        var cur = cursor is null ? "" : desc ? " AND Id < @cursor" : " AND Id > @cursor";
        var args = new { designId, collection, ownerUserId, ownerAnon, cursor, take = limit + 1 };
        var items = (await c.QueryAsync<FreeformRecord>(
            $"SELECT TOP (@take) * FROM {T} WHERE DesignId = @designId AND [Collection] = @collection{own}{cur} ORDER BY Id {(desc ? "DESC" : "ASC")}", args)).ToList();
        var total = await c.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {T} WHERE DesignId = @designId AND [Collection] = @collection{own}", args);
        return (items, total);
    }

    public async Task<FreeformRecord?> GetAsync(string designId, string collection, long id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var c = await _db.OpenConnectionAsync(ct);
        return await c.QuerySingleOrDefaultAsync<FreeformRecord>($"SELECT * FROM {T} WHERE Id = @id AND DesignId = @designId AND [Collection] = @collection", new { id, designId, collection });
    }

    /// <summary>إضافة سجل؛ يرفض بعد حد المجموعات أو السجلات للتصميم. يعيد (السجل، الخطأ).</summary>
    public async Task<(FreeformRecord? Record, string? Error)> AddAsync(string designId, string collection, string json, string? userId, string? userName, string? anonId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var c = await _db.OpenConnectionAsync(ct);
        var stats = await c.QuerySingleAsync<(int Total, int Collections, int InThis)>(
            $"SELECT COUNT(*) AS Total, COUNT(DISTINCT [Collection]) AS Collections, SUM(CASE WHEN [Collection] = @collection THEN 1 ELSE 0 END) AS InThis FROM {T} WHERE DesignId = @designId",
            new { designId, collection });
        if (stats.Total >= MaxRecordsPerDesign) return (null, $"بلغت سجلات هذا التصميم الحد ({MaxRecordsPerDesign:N0}).");
        if (stats.InThis == 0 && stats.Collections >= MaxCollections) return (null, $"عدد المجموعات يتجاوز الحد ({MaxCollections}).");
        var now = DateTime.UtcNow;
        var r = new FreeformRecord { DesignId = designId, Collection = collection, Json = json, CreatedByUserId = userId, CreatedByName = userName, AnonId = anonId, CreatedAt = now, UpdatedAt = now };
        r.Id = await c.ExecuteScalarAsync<long>(
            $"INSERT INTO {T} (DesignId, [Collection], [Json], CreatedByUserId, CreatedByName, AnonId, CreatedAt, UpdatedAt) OUTPUT INSERTED.Id VALUES (@DesignId, @Collection, @Json, @CreatedByUserId, @CreatedByName, @AnonId, @CreatedAt, @UpdatedAt)", r);
        return (r, null);
    }

    public async Task UpdateAsync(FreeformRecord r, string json, string? byName, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var c = await _db.OpenConnectionAsync(ct);
        r.Json = json; r.UpdatedAt = DateTime.UtcNow; r.UpdatedByName = byName;
        await c.ExecuteAsync($"UPDATE {T} SET [Json] = @Json, UpdatedAt = @UpdatedAt, UpdatedByName = @UpdatedByName WHERE Id = @Id", r);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var c = await _db.OpenConnectionAsync(ct);
        await c.ExecuteAsync($"DELETE FROM {T} WHERE Id = @id", new { id });
    }

    public async Task DeleteDesignAsync(string designId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var c = await _db.OpenConnectionAsync(ct);
        await c.ExecuteAsync($"DELETE FROM {T} WHERE DesignId = @designId", new { designId });
    }

    /// <summary>كل سجلات مجموعة (للتصدير ولبناء الجدول لاحقًا) بالترتيب التصاعدي.</summary>
    public async Task<IReadOnlyList<FreeformRecord>> AllAsync(string designId, string collection, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var c = await _db.OpenConnectionAsync(ct);
        return (await c.QueryAsync<FreeformRecord>($"SELECT * FROM {T} WHERE DesignId = @designId AND [Collection] = @collection ORDER BY Id", new { designId, collection })).ToList();
    }
}
