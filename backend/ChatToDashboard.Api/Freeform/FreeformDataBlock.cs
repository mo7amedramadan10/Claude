using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ChatToDashboard.Api.Freeform;

/// <summary>استعلام مسجَّل لتصميم حر: مفتاحه في الصفحة (<c>q1</c>…) وجملته ونتيجته الأخيرة.</summary>
public sealed record FreeformQuery(string Key, string Sql, JsonNode? Result);

/// <summary>
/// كتلة بيانات الصفحة في «تصميم حر»: <c>&lt;script type="application/json" id="jeem-data"&gt;</c> تحمل نتائج الاستعلامات
/// بمفاتيحها، و<c>&lt;script id="jeem-data-rt"&gt;</c> يعرّف <c>window.JEEM_DATA</c> منها قبل أي سكربت في الصفحة.
/// الصفحة تقرأ أرقامها من <c>JEEM_DATA.q1.rows</c>، فالتحديث = إعادة تنفيذ الاستعلامات واستبدال الكتلة بلا موديل.
/// </summary>
public static class FreeformDataBlock
{
    public const string DataId = "jeem-data", RuntimeId = "jeem-data-rt";
    private static readonly Regex DataRx = new(@"<script\b[^>]*\bid\s*=\s*[""']jeem-data[""'][^>]*>(?<json>[\s\S]*?)</script>\s*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RtRx = new(@"<script\b[^>]*\bid\s*=\s*[""']jeem-data-rt[""'][^>]*>[\s\S]*?</script>\s*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex KeyRx = new(@"^q(\d+)$", RegexOptions.Compiled);
    private static readonly JsonSerializerOptions JsonOpts = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    /// <summary>يقرأ الاستعلامات المحفوظة في كتلة صفحة قائمة (أو قائمة فارغة).</summary>
    public static List<FreeformQuery> Parse(string? html)
    {
        var list = new List<FreeformQuery>();
        if (string.IsNullOrEmpty(html)) return list;
        var m = DataRx.Match(html); if (!m.Success) return list;
        try
        {
            var root = JsonNode.Parse(m.Groups["json"].Value)?.AsObject();
            if (root?["queries"] is JsonObject qs)
                foreach (var (k, v) in qs)
                    if (v is JsonObject o && o["sql"]?.GetValue<string>() is { } sql) list.Add(new FreeformQuery(k, sql, o["result"]?.DeepClone()));
        }
        catch (JsonException) { }
        return list.OrderBy(q => KeyIndex(q.Key)).ToList();
    }

    public static int KeyIndex(string key) { var m = KeyRx.Match(key); return m.Success ? int.Parse(m.Groups[1].Value) : int.MaxValue; }
    public static string NextKey(IEnumerable<FreeformQuery> existing) => "q" + (existing.Select(q => KeyIndex(q.Key)).Where(i => i != int.MaxValue).DefaultIfEmpty(0).Max() + 1);

    /// <summary>الكتلة النصية الكاملة (البيانات + سكربت التشغيل).</summary>
    public static string Render(IReadOnlyList<FreeformQuery> queries, DateTime refreshedAt)
    {
        var qs = new JsonObject();
        foreach (var q in queries) qs[q.Key] = new JsonObject { ["sql"] = q.Sql, ["result"] = q.Result?.DeepClone() };
        var root = new JsonObject { ["refreshedAt"] = refreshedAt.ToString("O"), ["queries"] = qs };
        var json = root.ToJsonString(JsonOpts).Replace("</", "<\\/");
        return $"<script type=\"application/json\" id=\"{DataId}\">{json}</script>\n<script id=\"{RuntimeId}\">(function(){{var b=JSON.parse(document.getElementById('{DataId}').textContent);var d={{}};for(var k in b.queries){{var q=b.queries[k];var r=q.result||{{}};d[k]={{sql:q.sql,rows:r.rows||[],rowCount:r.rowCount||0}}}}window.JEEM_DATA=d;window.JEEM_DATA_META={{refreshedAt:b.refreshedAt}}}})()</script>\n";
    }

    /// <summary>يستبدل الكتلة القائمة أو يُدرجها أول <c>&lt;head&gt;</c> (وإلا أول الوثيقة) لتسبق كل سكربتات الصفحة.</summary>
    public static string Inject(string html, IReadOnlyList<FreeformQuery> queries, DateTime refreshedAt)
    {
        html = RtRx.Replace(DataRx.Replace(html ?? "", ""), "");
        if (queries.Count == 0) return html;
        var block = Render(queries, refreshedAt);
        var head = Regex.Match(html, @"<head\b[^>]*>", RegexOptions.IgnoreCase);
        if (head.Success) return html.Insert(head.Index + head.Length, "\n" + block);
        var htmlTag = Regex.Match(html, @"<html\b[^>]*>", RegexOptions.IgnoreCase);
        if (htmlTag.Success) return html.Insert(htmlTag.Index + htmlTag.Length, "\n<head>" + block + "</head>");
        return block + html;
    }

    public static bool HasBlock(string? html) => !string.IsNullOrEmpty(html) && DataRx.IsMatch(html);

    /// <summary>يضمن تضمين الجسر <c>/js/jeem-page.js</c> (قبل <c>&lt;/body&gt;</c>) لصفحة ببيانات حتى تحدّث أرقامها وهي مفتوحة عبر <c>jeem.data</c>.</summary>
    public static string EnsurePageBridge(string html)
    {
        if (string.IsNullOrEmpty(html) || Regex.IsMatch(html, @"jeem-page\.js", RegexOptions.IgnoreCase)) return html;
        const string tag = "<script src=\"/js/jeem-page.js\"></script>";
        return Regex.IsMatch(html, @"</body>", RegexOptions.IgnoreCase) ? Regex.Replace(html, @"</body>", tag + "</body>", RegexOptions.IgnoreCase) : html + tag;
    }

    /// <summary>لصفحات «شرائح»: يضمن وجود سكربت شريط المصغّرات <c>/js/jeem-slides.js</c> قبل <c>&lt;/body&gt;</c> (عند النشر).</summary>
    public static string EnsureSlidesKit(string html)
    {
        if (string.IsNullOrEmpty(html) || Regex.IsMatch(html, @"jeem-slides\.js", RegexOptions.IgnoreCase)) return html;
        const string tag = "<script src=\"/js/jeem-slides.js\"></script>";
        return Regex.IsMatch(html, @"</body>", RegexOptions.IgnoreCase) ? Regex.Replace(html, @"</body>", tag + "</body>", RegexOptions.IgnoreCase) : html + tag;
    }
}
