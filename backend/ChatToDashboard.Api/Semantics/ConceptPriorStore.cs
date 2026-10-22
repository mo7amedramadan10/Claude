using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Semantics;

/// <summary>
/// التعلّم العابر للعملاء: «اسم عمود مُوحَّد → مفهوم» بعدّادين (تأكيدات مستخدمين، ونتائج موديل
/// عالية الثقة). لا يُخزَّن أي شيء من بيانات العميل ولا اسم مشروعه — الاسم المُوحَّد للعمود فقط
/// (انظر <see cref="ConceptCatalog.Normalize"/>). يُستخدم لتحسين التخمين الأولي قبل نداء الموديل
/// في المشاريع التالية.
/// </summary>
public class ConceptPriorStore
{
    private readonly DataStore _db;
    private readonly SemaphoreSlim _schemaGate = new(1, 1);
    private volatile bool _schemaReady;

    public ConceptPriorStore(DataStore db) => _db = db;

    /// <summary>ينشئ الجدول مرة واحدة لكل عملية (لا DDL في كل بحث).</summary>
    private async Task EnsureSchemaOnceAsync(CancellationToken ct)
    {
        if (_schemaReady) return;
        await _schemaGate.WaitAsync(ct);
        try { if (!_schemaReady) { await EnsureSchemaAsync(ct); _schemaReady = true; } }
        finally { _schemaGate.Release(); }
    }

    private string Table => _db.Provider == DbProvider.Sqlite ? "\"ConceptPriors\"" : "[staging].[ConceptPriors]";

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);
        var text = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {Table} (
                 "NormalizedName" TEXT NOT NULL, "Concept" TEXT NOT NULL, "Hits" INTEGER NOT NULL, "UserHits" INTEGER NOT NULL,
                 "UpdatedAt" TEXT NOT NULL, PRIMARY KEY ("NormalizedName", "Concept"))
               """
            : $"""
               IF OBJECT_ID('staging.ConceptPriors') IS NULL
               CREATE TABLE {Table} (
                 [NormalizedName] NVARCHAR(200) NOT NULL, [Concept] NVARCHAR(64) NOT NULL, [Hits] INT NOT NULL, [UserHits] INT NOT NULL,
                 [UpdatedAt] DATETIME2 NOT NULL, PRIMARY KEY ([NormalizedName], [Concept]))
               """;
        await using var command = connection.CreateCommand();
        command.CommandText = text;
        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task RecordAsync(string columnName, string concept, bool byUser, CancellationToken ct = default)
    {
        var name = ConceptCatalog.Normalize(columnName);
        if (name.Length == 0 || !ConceptCatalog.IsKnown(concept)) return;
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var now = DateTime.UtcNow;
        var args = new { h = byUser ? 0 : 1, u = byUser ? 1 : 0, now, name, concept };
        var update = $"UPDATE {Table} SET Hits = Hits + @h, UserHits = UserHits + @u, UpdatedAt = @now WHERE NormalizedName = @name AND Concept = @concept";
        if (await connection.ExecuteAsync(update, args) > 0) return;
        try
        {
            await connection.ExecuteAsync(
                $"INSERT INTO {Table} (NormalizedName, Concept, Hits, UserHits, UpdatedAt) VALUES (@name, @concept, @h, @u, @now)", args);
        }
        catch (Exception)
        {
            // سباق: كاتب آخر أدرج المفتاح نفسه بين التحديث والإدراج — نعيد التحديث بدل الفشل.
            await connection.ExecuteAsync(update, args);
        }
    }

    /// <summary>بحث جماعي لأعمدة جدول كامل في استعلام واحد (بدل N استعلامات).</summary>
    public async Task<IReadOnlyDictionary<string, (string Concept, double Confidence)>> LookupManyAsync(IEnumerable<string> columnNames, CancellationToken ct = default)
    {
        var result = new Dictionary<string, (string, double)>(StringComparer.OrdinalIgnoreCase);
        var byNormalized = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var c in columnNames)
        {
            var n = ConceptCatalog.Normalize(c);
            if (n.Length == 0) continue;
            (byNormalized.TryGetValue(n, out var l) ? l : byNormalized[n] = new()).Add(c);
        }
        if (byNormalized.Count == 0) return result;
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<(string NormalizedName, string Concept, int Hits, int UserHits)>(
            $"SELECT NormalizedName, Concept, Hits, UserHits FROM {Table} WHERE NormalizedName IN @names", new { names = byNormalized.Keys.ToArray() });
        foreach (var g in rows.GroupBy(r => r.NormalizedName))
        {
            var best = g.OrderByDescending(r => r.UserHits * 10 + r.Hits).First();
            (string, double)? hit = best.UserHits >= 1 ? (best.Concept, 0.9) : best.Hits >= 3 ? (best.Concept, 0.85) : null;
            if (hit is null) continue;
            foreach (var original in byNormalized[g.Key]) result[original] = hit.Value;
        }
        return result;
    }

    /// <summary>أقوى مفهوم سابق لاسم عمود: تأكيد مستخدم واحد يكفي، وإلا ثلاث نتائج موديل متفقة.
    /// null عندما لا يوجد ما يكفي.</summary>
    public async Task<(string Concept, double Confidence)?> LookupAsync(string columnName, CancellationToken ct = default)
    {
        var name = ConceptCatalog.Normalize(columnName);
        if (name.Length == 0) return null;
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = (await connection.QueryAsync<(string Concept, int Hits, int UserHits)>(
            $"SELECT Concept, Hits, UserHits FROM {Table} WHERE NormalizedName = @name", new { name })).ToList();
        var best = rows.OrderByDescending(r => r.UserHits * 10 + r.Hits).FirstOrDefault();
        if (best.Concept is null) return null;
        if (best.UserHits >= 1) return (best.Concept, 0.9);
        if (best.Hits >= 3) return (best.Concept, 0.85);
        return null;
    }
}
