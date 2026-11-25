using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ChatToDashboard.Api.Freeform;

/// <summary>
/// تحويل لوحة متابعة إلى تصميم حر (شرائح/مستند/تصميم/لوحة) بمخططات حيّة: كل عنصر له استعلام يُنفَّذ على الخادم ويُسجَّل نتيجةً في
/// <c>JEEM_DATA</c>، ويُعطى الموديل وسم بلوك جاهزًا (<c>data-jeem-chart</c> أو <c>data-jeem-value</c>) يضعه كما هو — كما يفعل
/// «📊 رسمة من بياناتي ← من لوحة محفوظة ← كما في اللوحة» تمامًا، فتُرسم الرسمة بمحرّك المنصة من البيانات الحقيقية وتتحدّث مع
/// «تحديث البيانات» والرابط المنشور الحيّ، بدل أن يرسم الموديل SVG ثابتًا. ما ينساه الموديل يُضاف آليًا (<see cref="EnsureBlocks"/>).
/// </summary>
public static class FreeformDashboardBlocks
{
    public sealed record Block(string Key, string Title, string Markup);

    static string? Str(JsonElement w, string p) => w.ValueKind == JsonValueKind.Object && w.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    /// <summary>مواصفة البلوك من شكل العنصر في اللوحة وأعمدة نتيجته (منقولة من <c>ffdmAsIs</c> في الواجهة).</summary>
    public static string Markup(JsonElement widget, string key, JsonArray rows)
    {
        var t = (Str(widget, "type") ?? "").ToLowerInvariant();
        var title = Str(widget, "title") ?? "";
        var cols = rows.FirstOrDefault() is JsonObject first ? first.Select(kv => kv.Key).ToList() : new List<string>();
        bool IsNum(string c) => rows.Any(r => r is JsonObject o && o[c] is JsonValue v && v.TryGetValue<double>(out _));
        bool Has(string? c) => c is not null && cols.Contains(c);
        var xKey = Str(widget, "xKey"); var yKey = Str(widget, "yKey");
        var x = Has(xKey) ? xKey! : cols.FirstOrDefault(c => !IsNum(c)) ?? cols.FirstOrDefault() ?? "";
        var series = widget.ValueKind == JsonValueKind.Object && widget.TryGetProperty("series", out var s) && s.ValueKind == JsonValueKind.Array
            ? s.EnumerateArray().Where(e => Has(Str(e, "key"))).ToList() : new List<JsonElement>();
        var cfg = new JsonObject();
        JsonNode y = series.Count > 1 ? new JsonArray(series.Select(e => (JsonNode?)JsonValue.Create(Str(e, "key"))).ToArray())
            : JsonValue.Create(series.Count == 1 ? Str(series[0], "key") : Has(yKey) ? yKey : cols.FirstOrDefault(c => IsNum(c) && c != x) ?? cols.LastOrDefault() ?? "")!;
        var yName = y is JsonValue yv ? yv.GetValue<string>() : null;
        if (t.Contains("kpi") || t.Contains("stat") || t.Contains("metric") || t == "number")
        {
            var col = Has(yKey) ? yKey! : cols.FirstOrDefault(IsNum) ?? yName ?? "";
            return $"<div class=\"jeem-kpi-block\"><div class=\"jeem-kpi-value\" style=\"font-size:40px;font-weight:800;line-height:1.1\"><span data-jeem-value=\"{WebUtility.HtmlEncode(key + "." + col)}\" data-fmt=\"number\"></span></div><div class=\"jeem-kpi-label\">{WebUtility.HtmlEncode(title)}</div></div>";
        }
        string type = t.Contains("table") ? "table"
            : new[] { ("bar", "bar"), ("column", "bar"), ("hbar", "bar"), ("line", "line"), ("area", "line"), ("forecast", "line"), ("pie", "pie"), ("donut", "donut"),
                      ("funnel", "funnel"), ("treemap", "treemap"), ("scatter", "scatter"), ("bubble", "scatter") }.FirstOrDefault(m => t == m.Item1 || t.StartsWith(m.Item1)).Item2
              ?? (t.Contains("radial") || t.Contains("gauge") ? "donut" : t.Contains("stage") ? "funnel" : "bar");
        // بلا عنوان داخل البلوك: عنوان العنصر يكتبه الموديل عنوانًا للبطاقة/الشريحة (وإلا تكرّر مرتين)
        cfg["q"] = key; cfg["type"] = type; cfg["x"] = x; cfg["y"] = y; cfg["fmt"] = "compact";
        var labels = new JsonObject();
        foreach (var e in series) if (Str(e, "label") is { Length: > 0 } l) labels[Str(e, "key")!] = l;
        if (labels.Count > 0) cfg["labels"] = labels;
        if (widget.ValueKind == JsonValueKind.Object && widget.TryGetProperty("stacked", out var st) && st.ValueKind == JsonValueKind.True) cfg["stacked"] = true;
        if (Str(widget, "centerLabel") is { Length: > 0 } cl) cfg["centerLabel"] = cl;
        var json = cfg.ToJsonString(new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        var h = type == "table" ? 360 : 320;
        return $"<div data-jeem-chart='{json.Replace("&", "&amp;").Replace("'", "&#39;")}' style=\"height:{h}px\"></div>";
    }

    /// <summary>هل وُضع بلوك المفتاح في الصفحة؟</summary>
    public static bool Present(string html, string key) =>
        html.Contains($"\"q\":\"{key}\"", StringComparison.Ordinal) || html.Contains($"&quot;q&quot;:&quot;{key}&quot;", StringComparison.Ordinal)
        || html.Contains($"data-jeem-value=\"{key}.", StringComparison.Ordinal);

    /// <summary>يضيف البلوكات التي لم يضعها الموديل: لصفحة شرائح شريحةٌ لكل بلوك بعد آخر شريحة، ولغيرها قسمٌ قبل آخر &lt;/body&gt;. يعيد العدد.</summary>
    public static (string Html, int Added) EnsureBlocks(string html, IReadOnlyList<Block> blocks)
    {
        var missing = blocks.Where(b => !Present(html, b.Key)).ToList();
        if (missing.Count == 0) return (html, 0);
        var dir = System.Text.RegularExpressions.Regex.IsMatch(html, "<html[^>]*\\bdir=[\"']?ltr", System.Text.RegularExpressions.RegexOptions.IgnoreCase) ? "ltr" : "rtl";
        var slides = FreeformScope.FindSlides(html);
        var sb = new StringBuilder();
        foreach (var b in missing)
        {
            var body = $"<div style=\"padding:40px 56px;box-sizing:border-box;width:100%;height:100%;display:flex;flex-direction:column;gap:18px\" dir=\"{dir}\"><h2 style=\"margin:0\">{WebUtility.HtmlEncode(b.Title)}</h2><div style=\"flex:1;min-height:0\">{b.Markup}</div></div>";
            sb.Append(slides.Count >= 2 ? $"<section class=\"slide\" data-jeem-added=\"chart\">{body}</section>" : $"<section data-jeem-added=\"chart\" style=\"margin:24px auto;max-width:1100px\">{body}</section>");
        }
        if (slides.Count >= 2) { var last = slides[^1]; return (html.Insert(last.End, sb.ToString()), missing.Count); }
        return (FreeformDataBlock.InsertBeforeBodyEnd(html, sb.ToString()), missing.Count);
    }
}
