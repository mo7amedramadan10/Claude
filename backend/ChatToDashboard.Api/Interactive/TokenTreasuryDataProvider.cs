using System.Globalization;
using System.Text.Json;

namespace ChatToDashboard.Api.Interactive;

/// <summary>
/// §4.18's real data provider for token-treasury ("خزينة الأصول الرقمية") — matches the
/// project's real tables against a logical contract of assets/stats/revenue_history/
/// tvl_history/participation and builds the shape token-treasury.js's JEEM_TREASURY_MOUNT
/// expects. §4.18 itself says the compound-interest simulator stays a pure client-side
/// calculation regardless of data source — nothing here touches it. The revenue card
/// (data.revenue, a single profit+yield pair per window) and the revenue chart
/// (data.revenueSeries, a day/month/year bucketed history) are two independent shapes in the
/// engine's own data contract, so they're built from two independent aggregations of the same
/// revenue_history rows rather than reusing one another's numbers — same distinction the demo
/// data itself keeps. currency/tvlTicks are left for the engine's own built-in defaults when
/// no real equivalent exists (a display/axis convenience, not a fact that needs a source).
/// </summary>
public class TokenTreasuryDataProvider : IInteractiveDataProvider
{
    public string TemplateId => "token-treasury";

    private readonly InteractiveContractMatcher _matcher;

    public TokenTreasuryDataProvider(InteractiveContractMatcher matcher) => _matcher = matcher;

    private static readonly IReadOnlyList<InteractiveTableContract> Contract = new List<InteractiveTableContract>
    {
        new("assets", "الأصول الرقمية المحتفَظ بها في الخزينة", new List<InteractiveColumnContract>
        {
            new("id", "معرّف الأصل", Required: false),
            new("symbol", "رمز الأصل (مثل ETH أو USDT)"),
            new("name", "الاسم المعروض للأصل", Required: false),
            new("balance_usd", "قيمة رصيد هذا الأصل بالدولار"),
        }),
        new("stats", "لقطة مؤشرات الخزينة الحالية (صف واحد)", new List<InteractiveColumnContract>
        {
            new("market_cap_usd", "القيمة السوقية الحالية بالدولار", Required: false),
            new("tvl_usd", "القيمة الكلية المقفلة الحالية بالدولار", Required: false),
            new("holders_count", "عدد حائزي الرمز", Required: false),
            new("volume_24h_usd", "حجم التداول خلال آخر 24 ساعة بالدولار", Required: false),
            new("circulating_supply", "المعروض المتداول من الرمز", Required: false),
            new("burned_supply", "إجمالي الكمية المحروقة من الرمز حتى الآن", Required: false),
        }),
        new("revenue_history", "سجل إيراد الخزينة عبر الزمن", new List<InteractiveColumnContract>
        {
            new("recorded_at", "وقت هذا السجل"),
            new("revenue_usd", "الإيراد المسجَّل في هذه اللحظة بالدولار"),
        }),
        new("tvl_history", "سجل القيمة الكلية المقفلة عبر الزمن", new List<InteractiveColumnContract>
        {
            new("recorded_at", "وقت هذا السجل"),
            new("tvl_usd", "القيمة المقفلة وقت هذا السجل بالدولار"),
        }, Required: false),
        new("participation", "عدد المستخدمين النشطين لكل ربع سنة", new List<InteractiveColumnContract>
        {
            new("year", "السنة"),
            new("quarter", "الربع (1 إلى 4)"),
            new("users_count", "عدد المستخدمين النشطين في هذا الربع"),
        }, Required: false),
    };

    private static readonly string[] ArabicMonths =
    {
        "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
        "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر",
    };
    private static readonly string[] ArabicWeekdays =
    {
        "الأحد", "الإثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة", "السبت",
    };

    public async Task<InteractiveDataResult> BuildAsync(string projectId, CancellationToken ct)
    {
        var missing = new[] { "الأصول (assets)", "مؤشرات الخزينة (stats)", "سجل الإيراد (revenue_history)", "سجل القيمة المقفلة (tvl_history)", "مشاركة المستخدمين (participation)" };
        var matched = await _matcher.MatchAsync(projectId, Contract, ct);
        if (matched is null) return InteractiveDataResult.Unlinked(missing);

        var assetsTable = matched.Tables["assets"];
        var statsTable = matched.Tables["stats"];
        var revenueTable = matched.Tables["revenue_history"];

        var assetRows = await _matcher.QueryAsync(BuildSelect(assetsTable, "id", "symbol", "name", "balance_usd"), matched.Context, ct);
        var statsRows = await _matcher.QueryAsync(BuildSelect(statsTable, "market_cap_usd", "tvl_usd", "holders_count", "volume_24h_usd", "circulating_supply", "burned_supply"), matched.Context, ct);
        var revenueRows = await _matcher.QueryAsync(BuildSelect(revenueTable, "recorded_at", "revenue_usd"), matched.Context, ct);
        if (assetRows.Count == 0 || statsRows.Count == 0 || revenueRows.Count == 0) return InteractiveDataResult.Unlinked(missing);

        var assets = assetRows.Select(r => new
        {
            symbol = Str(r, "symbol") ?? "",
            name = Str(r, "name") ?? Str(r, "symbol") ?? "",
            value = Num(r, "balance_usd"),
            cls = (Str(r, "symbol") ?? "").ToLowerInvariant(),
            decimals = 2,
        }).ToList();
        var totalAssets = assets.Sum(a => a.value);

        var stats = statsRows[0];
        var side = new object[]
        {
            new { label = "القيمة السوقية", value = Num(stats, "market_cap_usd"), icon = "chart" },
            new { label = "القيمة المقفلة", value = Num(stats, "tvl_usd"), icon = "shield" },
            new { label = "عدد الحائزين", value = Num(stats, "holders_count"), icon = "users", integer = true },
            new { label = "حجم 24 ساعة", value = Num(stats, "volume_24h_usd"), icon = "history" },
        };
        var supply = new { circulating = Num(stats, "circulating_supply"), burned = Num(stats, "burned_supply") };

        var revenue = revenueRows
            .Select(r => (Time: ParseDate(r, "recorded_at"), Revenue: Num(r, "revenue_usd")))
            .Where(r => r.Time is not null)
            .Select(r => (Time: r.Time!.Value, r.Revenue))
            .OrderBy(r => r.Time)
            .ToList();
        if (revenue.Count == 0) return InteractiveDataResult.Unlinked(missing);
        var anchor = revenue[^1].Time;

        double ProfitSince(int days) => revenue.Where(r => r.Time > anchor.AddDays(-days)).Sum(r => r.Revenue);
        object RevenueWindow(int days)
        {
            var profit = ProfitSince(days);
            var yieldPct = totalAssets > 0 ? profit / totalAssets * (365.0 / days) * 100 : 0;
            return new[] { profit, yieldPct };
        }
        var revenueOut = new Dictionary<string, object>
        {
            ["w"] = RevenueWindow(7),
            ["m"] = RevenueWindow(30),
            ["y"] = RevenueWindow(365),
        };

        var (weekV, weekL) = BuildDaily(revenue, anchor, 7);
        var (monthV, monthL) = BuildMonthly(revenue, anchor, 12);
        var (yearV, yearL) = BuildYearly(revenue, anchor, 10);
        var revenueSeries = new Dictionary<string, object>
        {
            ["w"] = new { v = weekV, l = weekL },
            ["m"] = new { v = monthV, l = monthL },
            ["y"] = new { v = yearV, l = yearL },
        };

        List<object> tvlHistory = new();
        List<string> tvlTicks = new();
        if (matched.Tables.TryGetValue("tvl_history", out var tvlTable))
        {
            var tvlRows = await _matcher.QueryAsync(BuildSelect(tvlTable, "recorded_at", "tvl_usd"), matched.Context, ct);
            var tvl = tvlRows
                .Select(r => (Time: ParseDate(r, "recorded_at"), Value: Num(r, "tvl_usd")))
                .Where(r => r.Time is not null)
                .Select(r => (Time: r.Time!.Value, r.Value))
                .OrderBy(r => r.Time)
                .ToList();
            tvlHistory = tvl.Select(r => (object)new { date = r.Time.ToString("yyyy-MM-dd"), value = r.Value }).ToList();
            tvlTicks = tvl.Select(r => r.Time.Year.ToString()).Distinct().Take(6).ToList();
        }

        var participation = new Dictionary<string, int[]>();
        if (matched.Tables.TryGetValue("participation", out var partTable))
        {
            var rows = await _matcher.QueryAsync(BuildSelect(partTable, "year", "quarter", "users_count"), matched.Context, ct);
            foreach (var r in rows)
            {
                var year = Str(r, "year") ?? "";
                var quarter = (int)Num(r, "quarter");
                if (string.IsNullOrEmpty(year) || quarter < 1 || quarter > 4) continue;
                if (!participation.TryGetValue(year, out var arr)) participation[year] = arr = new int[4];
                arr[quarter - 1] = (int)Num(r, "users_count");
            }
        }
        var participationOut = participation.OrderBy(kv => kv.Key, StringComparer.Ordinal).ToDictionary(kv => kv.Key, kv => kv.Value);

        return InteractiveDataResult.Ready(new
        {
            totalAssets,
            assets,
            side,
            participation = participationOut,
            revenueSeries,
            revenue = revenueOut,
            supply,
            tvlHistory,
            tvlTicks,
        });
    }

    private static (List<double> V, List<string> L) BuildDaily(List<(DateTime Time, double Revenue)> rows, DateTime anchor, int days)
    {
        var start = anchor.Date.AddDays(-(days - 1));
        var buckets = new double[days];
        var labels = new string[days];
        for (var i = 0; i < days; i++) labels[i] = ArabicWeekdays[(int)start.AddDays(i).DayOfWeek];
        foreach (var (time, rev) in rows)
        {
            var day = time.Date;
            if (day < start || day > anchor.Date) continue;
            var idx = (day - start).Days;
            if (idx >= 0 && idx < days) buckets[idx] += rev;
        }
        return (buckets.ToList(), labels.ToList());
    }

    private static (List<double> V, List<string> L) BuildMonthly(List<(DateTime Time, double Revenue)> rows, DateTime anchor, int months)
    {
        var start = new DateTime(anchor.Year, anchor.Month, 1).AddMonths(-(months - 1));
        var buckets = new double[months];
        var labels = new string[months];
        for (var i = 0; i < months; i++) labels[i] = ArabicMonths[start.AddMonths(i).Month - 1];
        foreach (var (time, rev) in rows)
        {
            var m0 = new DateTime(time.Year, time.Month, 1);
            if (m0 < start) continue;
            var idx = (m0.Year - start.Year) * 12 + (m0.Month - start.Month);
            if (idx >= 0 && idx < months) buckets[idx] += rev;
        }
        return (buckets.ToList(), labels.ToList());
    }

    private static (List<double> V, List<string> L) BuildYearly(List<(DateTime Time, double Revenue)> rows, DateTime anchor, int years)
    {
        var startYear = anchor.Year - (years - 1);
        var buckets = new double[years];
        var labels = new string[years];
        for (var i = 0; i < years; i++) labels[i] = (startYear + i).ToString();
        foreach (var (time, rev) in rows)
        {
            var idx = time.Year - startYear;
            if (idx >= 0 && idx < years) buckets[idx] += rev;
        }
        return (buckets.ToList(), labels.ToList());
    }

    private static string BuildSelect(MatchedInteractiveTable table, params string[] logicalCols)
    {
        var pairs = logicalCols.Select(key => (Key: key, Real: table.Column(key))).Where(p => p.Real is not null).ToList();
        return pairs.Count == 0
            ? $"SELECT * FROM {table.RealTable}"
            : $"SELECT {string.Join(", ", pairs.Select(p => $"{p.Real} AS \"{p.Key}\""))} FROM {table.RealTable}";
    }

    private static string? Str(Dictionary<string, JsonElement> row, string col) =>
        row.TryGetValue(col, out var v) && v.ValueKind is JsonValueKind.String ? v.GetString()
        : row.TryGetValue(col, out v) && v.ValueKind is JsonValueKind.Number ? v.ToString()
        : null;

    private static double Num(Dictionary<string, JsonElement> row, string col)
    {
        if (!row.TryGetValue(col, out var v)) return 0;
        return v.ValueKind switch
        {
            JsonValueKind.Number => v.GetDouble(),
            JsonValueKind.String when double.TryParse(v.GetString(), out var d) => d,
            _ => 0,
        };
    }

    private static DateTime? ParseDate(Dictionary<string, JsonElement> row, string col)
    {
        var s = Str(row, col);
        if (string.IsNullOrWhiteSpace(s)) return null;
        return DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ? dt : null;
    }
}
