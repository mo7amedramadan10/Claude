using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ChatToDashboard.Api.Freeform;

/// <summary>
/// تحقق حتمي من بلوكات البيانات (<c>data-jeem-chart</c> و<c>data-jeem-value</c>) في HTML أعاده الموديل: كل بلوك يجب أن يشير إلى نتيجة
/// استعلام مسجَّلة في الصفحة، وأن تكون أعمدته موجودة في صفوفها، ونوعه من الأنواع المدعومة. الخطأ يُعاد للموديل ليصلحه (محاولة ثانية) أو
/// يُذكر للمستخدم — فلا تصل إلى الصفحة رسمة «مربوطة» بشيء غير موجود دون أن يعرف أحد.
/// </summary>
public static class FreeformBlockValidator
{
    private static readonly Regex ChartAttr = new(@"\bdata-jeem-chart\s*=\s*(?:'(?<v>[^']*)'|""(?<v>[^""]*)"")", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ValueAttr = new(@"\bdata-jeem-value\s*=\s*(?:'(?<v>[^']*)'|""(?<v>[^""]*)"")", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex Scripts = new(@"<script\b[^>]*>[\s\S]*?</script\s*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly HashSet<string> ChartTypes = new(StringComparer.OrdinalIgnoreCase) { "bar", "line", "pie", "donut", "funnel", "treemap", "scatter" };

    /// <summary>عدد البلوكات في HTML (رسوم + قيم).</summary>
    public static int CountBlocks(string? html) => string.IsNullOrEmpty(html) ? 0 : ChartAttr.Matches(Scripts.Replace(html, "")).Count + ValueAttr.Matches(Scripts.Replace(html, "")).Count;

    /// <summary>مشاكل البلوكات بالعربية (قائمة فارغة = سليمة). نتيجة بلا صفوف لا يُتحقق من أعمدتها (لا يمكن معرفتها).</summary>
    public static List<string> Validate(string? html, IReadOnlyList<FreeformQuery> queries)
    {
        var problems = new List<string>();
        if (string.IsNullOrEmpty(html)) return problems;
        var body = Scripts.Replace(html, "");
        var byKey = queries.GroupBy(q => q.Key).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        foreach (Match m in ChartAttr.Matches(body))
        {
            JsonObject? spec;
            try { spec = JsonNode.Parse(WebUtility.HtmlDecode(m.Groups["v"].Value)) as JsonObject; }
            catch (JsonException) { spec = null; }
            if (spec is null) { problems.Add("بلوك رسم بمواصفة JSON غير صالحة."); continue; }
            var q = Str(spec["q"]); var title = Str(spec["title"]); var name = string.IsNullOrWhiteSpace(title) ? "رسم" : $"الرسم «{title}»";
            var type = Str(spec["type"]) ?? "bar";
            if (!ChartTypes.Contains(type)) problems.Add($"{name}: النوع «{type}» غير مدعوم (المتاح: {string.Join(" | ", ChartTypes)}).");
            if (!Resolve(q, byKey, name, problems, out var cols)) continue;
            foreach (var col in Columns(spec["x"]).Concat(Columns(spec["y"])))
                if (cols is not null && !cols.Contains(col)) problems.Add($"{name}: العمود «{col}» غير موجود في نتيجة {q} (الأعمدة: {string.Join("، ", cols)}).");
        }
        foreach (Match m in ValueAttr.Matches(body))
        {
            var reference = WebUtility.HtmlDecode(m.Groups["v"].Value).Trim();
            var dot = reference.IndexOf('.');
            var q = dot > 0 ? reference[..dot] : reference; var col = dot > 0 ? reference[(dot + 1)..] : null;
            var name = $"القيمة «{reference}»";
            if (!Resolve(q, byKey, name, problems, out var cols)) continue;
            if (string.IsNullOrWhiteSpace(col)) problems.Add($"{name}: اكتبها بالشكل qN.<العمود>.");
            else if (cols is not null && !cols.Contains(col)) problems.Add($"{name}: العمود «{col}» غير موجود في نتيجة {q} (الأعمدة: {string.Join("، ", cols)}).");
        }
        return problems.Distinct().ToList();
    }

    private static bool Resolve(string? q, Dictionary<string, FreeformQuery> byKey, string name, List<string> problems, out HashSet<string>? cols)
    {
        cols = null;
        if (string.IsNullOrWhiteSpace(q)) { problems.Add($"{name}: بلا مفتاح استعلام \"q\"."); return false; }
        if (!byKey.TryGetValue(q, out var query)) { problems.Add($"{name}: المفتاح {q} لا يقابل أي نتيجة query_data مسجَّلة — استعلم أولًا واستخدم المفتاح المذكور في ذيل النتيجة."); return false; }
        cols = ColumnsOf(query);
        return true;
    }

    /// <summary>أعمدة نتيجة استعلام من صفّها الأول (بلا حساسية لحالة الأحرف كما يقرؤها الراسم)، أو null بلا صفوف.</summary>
    public static HashSet<string>? ColumnsOf(FreeformQuery q)
    {
        if (q.Result?["rows"] is not JsonArray rows || rows.Count == 0 || rows[0] is not JsonObject first) return null;
        return new HashSet<string>(first.Select(p => p.Key), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>وصف النتائج المسجَّلة للموديل: المفتاح والأعمدة وعدد الصفوف — ليعيد استخدامها في البلوكات بلا استعلام جديد.</summary>
    public static string Describe(IReadOnlyList<FreeformQuery> queries)
    {
        if (queries.Count == 0) return "";
        var sb = new StringBuilder("# نتائج بيانات مسجَّلة في الصفحة (استخدمها في البلوكات مباشرة أو استعلم بـ query_data لغيرها)\n");
        foreach (var q in queries.Take(30))
        {
            var cols = ColumnsOf(q); var n = q.Result?["rowCount"]?.GetValue<int>() ?? (q.Result?["rows"] as JsonArray)?.Count ?? 0;
            sb.AppendLine($"- {q.Key}: {n} صف — الأعمدة: {(cols is null ? "(بلا صفوف)" : string.Join("، ", cols))}");
        }
        return sb.AppendLine().ToString();
    }

    private static string? Str(JsonNode? n) => n is JsonValue v && v.TryGetValue<string>(out var s) ? s : null;
    private static IEnumerable<string> Columns(JsonNode? n) => n switch
    {
        JsonArray a => a.Select(Str).Where(s => !string.IsNullOrWhiteSpace(s))!,
        JsonValue v when v.TryGetValue<string>(out var s) && !string.IsNullOrWhiteSpace(s) => new[] { s },
        _ => Array.Empty<string>()
    };
}
