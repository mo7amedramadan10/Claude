using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ChatToDashboard.Api.Decks;

/// <summary>
/// تحديث بيانات شرائح العرض **حتميًا بلا موديل**: لكل شريحة تحمل <c>query.sql</c> تُعاد الصفوف وتُصاغ في شكل الشريحة
/// بقواعد ثابتة (أول عمود نصي = التسميات، أول عمود رقمي = القيم؛ صف واحد = مؤشر لكل عمود). التنسيق بفاصل آلاف إنجليزي.
/// </summary>
public static class DeckDataRefresher
{
    public static string Fmt(object? v) => v switch
    {
        null => "—",
        JsonValue jv when jv.TryGetValue<double>(out var d) => d % 1 == 0 && Math.Abs(d) < 1e15 ? ((long)d).ToString("N0", CultureInfo.InvariantCulture) : d.ToString("N2", CultureInfo.InvariantCulture),
        JsonValue jv when jv.TryGetValue<string>(out var s) => DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt) && s.Length >= 10 && s.Contains('-') ? dt.ToString("yyyy-MM-dd") : s,
        JsonValue jv => jv.ToJsonString(),
        _ => v.ToString() ?? "",
    };
    private static bool IsNum(JsonNode? n) => n is JsonValue v && v.TryGetValue<double>(out _);
    private static double Num(JsonNode? n) => n is JsonValue v && v.TryGetValue<double>(out var d) ? d : 0;

    /// <summary>يطبّق صفوف نتيجة (مصفوفة كائنات) على الشريحة بحسب تخطيطها. يعيد false إن لم يكن للشريحة ما يُحدَّث.</summary>
    public static bool Apply(DeckSlide slide, JsonArray rows)
    {
        if (rows.Count == 0) return false;
        var first = rows[0]!.AsObject(); var cols = first.Select(kv => kv.Key).ToList();
        var numCols = cols.Where(c => rows.All(r => r?[c] is null || IsNum(r![c]))).Where(c => rows.Any(r => r?[c] is not null)).ToList();
        var textCols = cols.Except(numCols).ToList();
        var labelCol = textCols.FirstOrDefault() ?? cols[0]; var valueCol = numCols.FirstOrDefault(c => c != labelCol) ?? numCols.FirstOrDefault();
        switch (slide.Layout)
        {
            case "kpis":
                slide.Kpis = rows.Count == 1
                    ? cols.Take(6).Select(c => new DeckKpi { Label = c, Value = Fmt(first[c]) }).ToList()
                    : rows.Take(6).Select(r => new DeckKpi { Label = Fmt(r![labelCol]), Value = valueCol is null ? "—" : Fmt(r[valueCol]) }).ToList();
                return true;
            case "chart":
                if (valueCol is null) return false;
                slide.Chart ??= new DeckChart();
                slide.Chart.Labels = rows.Take(12).Select(r => Fmt(r![labelCol])).ToList();
                slide.Chart.Values = rows.Take(12).Select(r => Num(r![valueCol])).ToList();
                slide.Chart.Series = null;
                return true;
            case "table":
                slide.Table = new DeckTable { Columns = cols, Rows = rows.Take(40).Select(r => cols.Select(c => Fmt(r![c])).ToList()).ToList() };
                return true;
            default:
                slide.Bullets = rows.Count == 1
                    ? cols.Take(6).Select(c => $"{c}: {Fmt(first[c])}").ToList()
                    : rows.Take(6).Select(r => valueCol is null ? Fmt(r![labelCol]) : $"{Fmt(r![labelCol])}: {Fmt(r[valueCol])}").ToList();
                return true;
        }
    }
}
