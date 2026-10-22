using System.Globalization;
using System.Text.Json;

namespace ChatToDashboard.Api.Interactive;

/// <summary>
/// §4.17's real data provider for liquidity-pools ("مجمّعات السيولة المركّزة") — matches the
/// project's real tables against a logical contract of pools/activity/positions and builds the
/// shape liquidity-pools.js's JEEM_POOLS_MOUNT expects (pools[], positions[], and the
/// periods/series/labels day·week·month breakdown of the activity history). The reference
/// doc's "in production: actual time" rule is honoured relative to the data's own latest
/// timestamp (not the server clock) — a volume history is only ever "recent" relative to when
/// it was actually recorded, unlike team-tasks/time-tracking's own-day dashboards which anchor
/// on the real calendar date.
/// </summary>
public class LiquidityPoolsDataProvider : IInteractiveDataProvider
{
    public string TemplateId => "liquidity-pools";

    private readonly InteractiveContractMatcher _matcher;

    public LiquidityPoolsDataProvider(InteractiveContractMatcher matcher) => _matcher = matcher;

    private static readonly IReadOnlyList<InteractiveTableContract> Contract = new List<InteractiveTableContract>
    {
        new("pools", "مجمّعات السيولة المتاحة", new List<InteractiveColumnContract>
        {
            new("id", "معرّف المجمّع"),
            new("token_a", "رمز الأصل الأول في الزوج"),
            new("token_b", "رمز الأصل الثاني في الزوج"),
            new("fee_pct", "نسبة رسوم التداول للمجمّع"),
            new("category", "تصنيف المجمّع: رائجة أو مستقرة أو جديدة", Required: false),
            new("has_incentive", "هل للمجمّع مكافآت تعدين إضافية (صحيح/خطأ)", Required: false),
            new("price", "السعر الحالي للأصل الثاني مقابل الأول"),
            new("price_lo", "الحد الأدنى لنطاق السعر المعروض للمجمّع", Required: false),
            new("price_hi", "الحد الأعلى لنطاق السعر المعروض للمجمّع", Required: false),
            new("liquidity_usd", "إجمالي السيولة في المجمّع بالدولار"),
            new("volume_24h_usd", "حجم التداول خلال آخر 24 ساعة بالدولار"),
            new("fees_24h_usd", "الرسوم المحصّلة خلال آخر 24 ساعة بالدولار", Required: false),
            new("apr_pct", "العائد السنوي التقديري للمجمّع", Required: false),
        }),
        new("activity", "سجل حجم التداول وقيمة السيولة الكلية عبر الزمن لكل المجمّعات", new List<InteractiveColumnContract>
        {
            new("recorded_at", "وقت هذا السجل"),
            new("volume_usd", "حجم التداول المسجَّل في هذه اللحظة بالدولار"),
            new("tvl_usd", "القيمة الكلية المقفلة وقت هذا السجل بالدولار", Required: false),
        }),
        new("positions", "مراكز السيولة المضافة من المستخدمين إلى المجمّعات", new List<InteractiveColumnContract>
        {
            new("pool_id", "معرّف المجمّع الذي أُضيف إليه المركز"),
            new("amount_usd", "قيمة المركز بالدولار"),
            new("price_lo", "الحد الأدنى لنطاق سعر هذا المركز"),
            new("price_hi", "الحد الأعلى لنطاق سعر هذا المركز"),
            new("fees_earned_usd", "الرسوم المكتسبة لهذا المركز حتى الآن", Required: false),
        }, Required: false),
    };

    private static readonly string[] ArabicMonths =
    {
        "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
        "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر",
    };

    public async Task<InteractiveDataResult> BuildAsync(string projectId, CancellationToken ct)
    {
        var missing = new[] { "المجمّعات (pools)", "سجل النشاط (activity)", "مراكز السيولة (positions)" };
        var matched = await _matcher.MatchAsync(projectId, Contract, ct);
        if (matched is null) return InteractiveDataResult.Unlinked(missing);

        var poolsTable = matched.Tables["pools"];
        var activityTable = matched.Tables["activity"];

        var poolRows = await _matcher.QueryAsync(
            BuildSelect(poolsTable, "id", "token_a", "token_b", "fee_pct", "category", "has_incentive", "price", "price_lo", "price_hi", "liquidity_usd", "volume_24h_usd", "fees_24h_usd", "apr_pct"),
            matched.Context, ct);
        var activityRows = await _matcher.QueryAsync(
            BuildSelect(activityTable, "recorded_at", "volume_usd", "tvl_usd"),
            matched.Context, ct);
        if (poolRows.Count == 0 || activityRows.Count == 0) return InteractiveDataResult.Unlinked(missing);

        var pools = poolRows.Select(r =>
        {
            var price = Num(r, "price");
            var lo = r.ContainsKey("price_lo") ? Num(r, "price_lo") : price * 0.95;
            var hi = r.ContainsKey("price_hi") ? Num(r, "price_hi") : price * 1.05;
            return new
            {
                id = Str(r, "id") ?? "",
                a = Str(r, "token_a") ?? "",
                b = Str(r, "token_b") ?? "",
                fee = Num(r, "fee_pct"),
                label = Str(r, "category") ?? "",
                inc = Bool(r, "has_incentive", false),
                liq = Num(r, "liquidity_usd"),
                vol = Num(r, "volume_24h_usd"),
                fees = Num(r, "fees_24h_usd"),
                apr = Num(r, "apr_pct"),
                price,
                lo,
                hi,
            };
        }).ToList();

        var tokenHues = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in pools)
        {
            tokenHues.TryAdd(p.a, TokenHue(p.a));
            tokenHues.TryAdd(p.b, TokenHue(p.b));
        }

        var positions = new List<object>();
        if (matched.Tables.TryGetValue("positions", out var positionsTable))
        {
            var rows = await _matcher.QueryAsync(BuildSelect(positionsTable, "pool_id", "amount_usd", "price_lo", "price_hi", "fees_earned_usd"), matched.Context, ct);
            positions = rows.Select(r => (object)new
            {
                pool = Str(r, "pool_id") ?? "",
                amt = Num(r, "amount_usd"),
                lo = Num(r, "price_lo"),
                hi = Num(r, "price_hi"),
                fees = Num(r, "fees_earned_usd"),
            }).ToList();
        }

        var activity = activityRows
            .Select(r => (Time: ParseDate(r, "recorded_at"), Volume: Num(r, "volume_usd"), Tvl: r.ContainsKey("tvl_usd") ? Num(r, "tvl_usd") : (double?)null))
            .Where(a => a.Time is not null)
            .OrderBy(a => a.Time)
            .ToList();
        if (activity.Count == 0) return InteractiveDataResult.Unlinked(missing);

        var anchor = activity[^1].Time!.Value;
        var cum = activity.Sum(a => a.Volume);
        var fallbackTvl = pools.Sum(p => p.liq);
        var latestTvl = activity.LastOrDefault(a => a.Tvl is not null).Tvl ?? fallbackTvl;

        var periods = new Dictionary<string, object>();
        var series = new Dictionary<string, List<double>>();
        var labels = new Dictionary<string, List<string>>();

        BuildBucketedPeriod("day", anchor.AddHours(-24), anchor, TimeSpan.FromHours(1), 24,
            t => t.ToString("HH:00"), activity, periods, series, labels, cum, latestTvl);
        BuildBucketedPeriod("week", anchor.AddDays(-7), anchor, TimeSpan.FromDays(1), 7,
            t => $"{t.Day} {ArabicMonths[t.Month - 1]}", activity, periods, series, labels, cum, latestTvl);
        BuildBucketedPeriod("month", anchor.AddDays(-30), anchor, TimeSpan.FromDays(1), 30,
            t => $"{t.Day} {ArabicMonths[t.Month - 1]}", activity, periods, series, labels, cum, latestTvl);

        return InteractiveDataResult.Ready(new { tokenHues, pools, positions, periods, series, labels });
    }

    private static void BuildBucketedPeriod(
        string key, DateTime start, DateTime end, TimeSpan bucketSize, int bucketCount, Func<DateTime, string> labelOf,
        List<(DateTime? Time, double Volume, double? Tvl)> activity,
        Dictionary<string, object> periods, Dictionary<string, List<double>> series, Dictionary<string, List<string>> labels,
        double cum, double latestTvl)
    {
        var buckets = new double[bucketCount];
        var bucketLabels = new string[bucketCount];
        for (var i = 0; i < bucketCount; i++)
        {
            var bucketStart = start + TimeSpan.FromTicks(bucketSize.Ticks * i);
            bucketLabels[i] = labelOf(bucketStart);
        }
        foreach (var a in activity)
        {
            var t = a.Time!.Value;
            if (t < start || t > end) continue;
            var idx = (int)((t - start).Ticks / bucketSize.Ticks);
            if (idx < 0) idx = 0;
            if (idx >= bucketCount) idx = bucketCount - 1;
            buckets[idx] += a.Volume;
        }

        var vol = buckets.Sum();
        periods[key] = new { tvl = latestTvl, cum, vol };
        series[key] = buckets.ToList();
        labels[key] = bucketLabels.ToList();
    }

    private static int TokenHue(string token)
    {
        unchecked
        {
            var hash = 0;
            foreach (var c in token) hash = hash * 31 + c;
            return ((hash % 360) + 360) % 360;
        }
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

    private static bool Bool(Dictionary<string, JsonElement> row, string col, bool defaultValue)
    {
        if (!row.TryGetValue(col, out var v)) return defaultValue;
        return v.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => v.GetDouble() != 0,
            JsonValueKind.String => v.GetString() is { } s && (s.Equals("true", StringComparison.OrdinalIgnoreCase) || s == "1" || s.Equals("نعم")),
            _ => defaultValue,
        };
    }

    private static DateTime? ParseDate(Dictionary<string, JsonElement> row, string col)
    {
        var s = Str(row, col);
        if (string.IsNullOrWhiteSpace(s)) return null;
        return DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ? dt : null;
    }
}
