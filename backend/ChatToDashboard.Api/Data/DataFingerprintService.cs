using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Dapper;

namespace ChatToDashboard.Api.Data;

/// <summary>
/// بصمة بيانات الجداول التي يعتمد عليها تصميم/عرض: لكل جدول عدد الصفوف + <c>CHECKSUM_AGG(BINARY_CHECKSUM(*))</c>
/// (يكتشف الإضافة والحذف والتعديل)، تُدمج في تجزئة قصيرة واحدة. تُقارن عند الفتح بالبصمة المحفوظة لإظهار
/// «تغيّرت بيانات المصدر منذ آخر تحديث». الجداول تُستخرج من SQL الاستعلامات (FROM/JOIN) وتُقبل فقط إن وُجدت في الكتالوج.
/// ذاكرة مؤقتة قصيرة لكل جدول حتى لا يُمسح الجدول عند كل فتح متتابع.
/// </summary>
public class DataFingerprintService
{
    private static readonly Regex TableRx = new(@"\b(?:FROM|JOIN)\s+(?<t>(?:\[[^\]]+\]|[\w؀-ۿ]+)(?:\s*\.\s*(?:\[[^\]]+\]|[\w؀-ۿ]+))?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private readonly DataStore _db;
    private readonly Dictionary<string, (DateTime At, string Value)> _cache = new();
    private readonly object _gate = new();
    public static readonly TimeSpan CacheFor = TimeSpan.FromSeconds(45);
    public DataFingerprintService(DataStore db) => _db = db;

    /// <summary>أسماء الجداول (schema.table) المذكورة بعد FROM/JOIN، بلا أقواس وبحروف صغيرة.</summary>
    public static IReadOnlyList<string> ExtractTables(IEnumerable<string> sqls)
    {
        var set = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var sql in sqls)
            foreach (Match m in TableRx.Matches(sql ?? ""))
            {
                var raw = m.Groups["t"].Value.Replace("[", "").Replace("]", "");
                var parts = raw.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0 || parts.Length > 2) continue;
                set.Add(parts.Length == 2 ? $"{parts[0]}.{parts[1]}" : $"dbo.{parts[0]}");
            }
        return set.ToList();
    }

    /// <summary>بصمة مدمجة للجداول المذكورة في الاستعلامات ضمن كتالوج المشروع؛ null إن لم يُطابق أي جدول.</summary>
    public async Task<string?> ComputeAsync(IEnumerable<string> sqls, string? projectId, CancellationToken ct = default)
    {
        var wanted = ExtractTables(sqls); if (wanted.Count == 0) return null;
        var catalog = await _db.GetSchemaAsync(projectId, ct);
        var known = new Dictionary<string, (string Schema, string Name)>(StringComparer.OrdinalIgnoreCase);
        foreach (var t in catalog)
        {
            var (schema, name) = SplitName(t.Table);
            known[$"{schema}.{name}"] = (schema, name);
            if (!known.ContainsKey($"dbo.{name}")) known[$"dbo.{name}"] = (schema, name);
        }
        var parts = new List<string>();
        foreach (var w in wanted)
        {
            if (!known.TryGetValue(w, out var tbl)) continue;
            parts.Add($"{tbl.Schema}.{tbl.Name}={await TableFingerprintAsync(tbl.Schema, tbl.Name, ct)}");
        }
        if (parts.Count == 0) return null;
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(";", parts))))[..24];
    }

    private static (string Schema, string Name) SplitName(string full)
    {
        var parts = full.Replace("[", "").Replace("]", "").Split('.', 2);
        return parts.Length == 2 ? (parts[0], parts[1]) : ("dbo", parts[0]);
    }

    private async Task<string> TableFingerprintAsync(string schema, string name, CancellationToken ct)
    {
        var key = $"{schema}.{name}";
        lock (_gate) { if (_cache.TryGetValue(key, out var c) && DateTime.UtcNow - c.At < CacheFor) return c.Value; }
        string value;
        await using var connection = await _db.OpenConnectionAsync(ct);
        var q = $"[{schema.Replace("]", "]]")}].[{name.Replace("]", "]]")}]";
        try
        {
            var row = await connection.QuerySingleAsync<(long C, long? H)>($"SELECT COUNT_BIG(*) AS C, CHECKSUM_AGG(BINARY_CHECKSUM(*)) AS H FROM {q}", commandTimeout: 30);
            value = $"{row.C}:{row.H}";
        }
        catch (Exception) // أنواع لا يقبلها BINARY_CHECKSUM (xml/text…) أو جدول اختفى — العدد وحده
        {
            try { value = (await connection.ExecuteScalarAsync<long>($"SELECT COUNT_BIG(*) FROM {q}", commandTimeout: 30)).ToString(); }
            catch (Exception) { value = "missing"; }
        }
        lock (_gate) _cache[key] = (DateTime.UtcNow, value);
        return value;
    }
}
