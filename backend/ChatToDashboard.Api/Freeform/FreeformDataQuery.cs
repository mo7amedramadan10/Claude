using ChatToDashboard.Api.Build;

namespace ChatToDashboard.Api.Freeform;

/// <summary>طلب «📊 رسمة من بياناتي»: جدول + مقياس (تجميع على عمود) + تقسيم اختياري (عمود فئة أو تاريخ بحبيبة)، أو عنصر من لوحة محفوظة.</summary>
public class FreeformDataQueryRequest
{
    public string? Table { get; set; }
    /// <summary>sum | avg | min | max | count | count_distinct.</summary>
    public string? Agg { get; set; }
    public string? Measure { get; set; }
    public string? Dimension { get; set; }
    /// <summary>للتاريخ: day | month | quarter | year (افتراضيًا month).</summary>
    public string? Grain { get; set; }
    public int? Limit { get; set; }
    /// <summary>عنصر من لوحة محفوظة بدل البناء من الأعمدة.</summary>
    public string? HistoryId { get; set; }
    /// <summary>عنصر بُني بـ data/build (معرّف في ذاكرة الخادم).</summary>
    public string? BuildId { get; set; }
    public int? WidgetIndex { get; set; }
    /// <summary>true = معاينة فقط بلا تسجيل الاستعلام في التصميم.</summary>
    public bool Preview { get; set; }
    /// <summary>مفاتيح كتلة البيانات في الصفحة الحية الآن (قد تحمل إضافات لم تُحفظ بعد) حتى لا يتكرر المفتاح.</summary>
    public List<string>? Keys { get; set; }
}

/// <summary>بناء SQL حتمي آمن من اختيارات المستخدم (بلا موديل): الجدول والأعمدة يجب أن تكون في ملف تعريف بيانات المستخدم المرئية له،
/// والمعرّفات تُقتبس دائمًا، والتجميع والحبيبة من قوائم ثابتة.</summary>
public static class FreeformDataQuery
{
    public static readonly string[] Aggs = { "sum", "avg", "min", "max", "count", "count_distinct" };
    public static readonly string[] Grains = { "day", "month", "quarter", "year" };

    public sealed record Built(string Sql, string? DimensionAlias, string MeasureAlias, bool Temporal);

    internal static string Quote(string id) => $"[{id.Replace("]", "]]")}]";
    internal static string QuoteTable(string table) => string.Join(".", table.Split('.').Select(Quote));

    public static (Built? Query, string? Error) Build(DataProfile profile, FreeformDataQueryRequest r)
    {
        var t = string.IsNullOrWhiteSpace(r.Table) ? null : profile.Find(r.Table.Trim());
        if (t is null) return (null, "اختر جدولًا من بياناتك المتاحة.");
        ProfiledColumn? Col(string? name) => string.IsNullOrWhiteSpace(name) ? null : t.Columns.FirstOrDefault(c => string.Equals(c.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
        var agg = (r.Agg ?? "sum").Trim().ToLowerInvariant();
        if (!Aggs.Contains(agg)) return (null, "نوع التجميع غير معروف.");
        var m = Col(r.Measure);
        if (agg != "count" && m is null) return (null, "اختر عمود المقياس.");
        if (!string.IsNullOrWhiteSpace(r.Measure) && m is null) return (null, $"العمود «{r.Measure}» غير موجود في الجدول.");
        var d = Col(r.Dimension);
        if (!string.IsNullOrWhiteSpace(r.Dimension) && d is null) return (null, $"العمود «{r.Dimension}» غير موجود في الجدول.");

        string mExpr = agg switch
        {
            "count" => m is null ? "COUNT(*)" : $"COUNT({Quote(m.Name)})",
            "count_distinct" => $"COUNT(DISTINCT {Quote(m!.Name)})",
            _ => $"{agg.ToUpperInvariant()}(TRY_CONVERT(float, {Quote(m!.Name)}))",
        };
        var mAlias = agg switch
        {
            "sum" => $"إجمالي {m!.Name}", "avg" => $"متوسط {m!.Name}", "min" => $"أدنى {m!.Name}", "max" => $"أعلى {m!.Name}",
            "count_distinct" => $"عدد {m!.Name} المميزة", _ => m is null ? "عدد السجلات" : $"عدد {m.Name}",
        };
        var from = QuoteTable(t.Table);
        if (d is null) return (new Built($"SELECT {mExpr} AS {Quote(mAlias)} FROM {from}", null, mAlias, false), null);

        var temporal = d.Kind == "date";
        string dExpr;
        if (temporal)
        {
            var g = Grains.Contains((r.Grain ?? "").ToLowerInvariant()) ? r.Grain!.ToLowerInvariant() : "month";
            var dv = $"TRY_CONVERT(date, {Quote(d.Name)})";
            dExpr = g switch
            {
                "day" => $"CONVERT(char(10), {dv}, 126)",
                "year" => $"CAST(YEAR({dv}) AS char(4))",
                "quarter" => $"CONCAT(YEAR({dv}), '-Q', DATEPART(quarter, {dv}))",
                _ => $"CONVERT(char(7), {dv}, 126)",
            };
        }
        else dExpr = Quote(d.Name);
        var limit = Math.Clamp(r.Limit ?? (temporal ? 24 : 12), 1, 60);
        var notNull = temporal ? $"TRY_CONVERT(date, {Quote(d.Name)}) IS NOT NULL" : $"{Quote(d.Name)} IS NOT NULL";
        var order = temporal ? $"{dExpr} DESC" : "2 DESC";
        var sql = $"SELECT TOP ({limit}) {dExpr} AS {Quote(d.Name)}, {mExpr} AS {Quote(mAlias)} FROM {from} WHERE {notNull} GROUP BY {dExpr} ORDER BY {order}";
        // الزمني: آخر N فترات ثم بترتيب تصاعدي للعرض
        if (temporal) sql = $"SELECT * FROM ({sql}) AS t ORDER BY 1 ASC";
        return (new Built(sql, d.Name, mAlias, temporal), null);
    }
}
