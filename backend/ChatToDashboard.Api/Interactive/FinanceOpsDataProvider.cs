using static ChatToDashboard.Api.Interactive.InteractiveRowHelpers;

namespace ChatToDashboard.Api.Interactive;

/// <summary>
/// §4.22's real data provider for finance-ops ("الإدارة المالية"). The cash-flow chart (daily/
/// weekly/monthly) and the headline rev/exp/pro KPIs both come from the SAME real ledgers
/// (cash_transactions for cash movements, profit_loss_entries for accrual revenue/expenses)
/// rather than a separately-maintained KPI figure that could disagree with the chart next to
/// it: the KPIs are just the most recent complete period's totals from the same series the
/// chart already aggregates. "Opening" balance for any window is the real running balance
/// (cumulative in−out) as of the moment that window starts, computed from the full available
/// transaction history rather than assumed to be zero. Inventory regions, automation insights/
/// exceptions and the operations-flow efficiency map onto their own small tables directly — see
/// finance-ops.js's own doc comment for why the flow diagram's 5 fixed nodes stay fixed in the
/// engine while only their efficiency numbers come from real data.
/// </summary>
public class FinanceOpsDataProvider : IInteractiveDataProvider
{
    public string TemplateId => "finance-ops";

    private readonly InteractiveContractMatcher _matcher;

    public FinanceOpsDataProvider(InteractiveContractMatcher matcher) => _matcher = matcher;

    private static readonly string[] FlowNodeNames = { "بيانات العملاء", "تغطية الأتمتة", "ضبط الاستثناءات", "تحسين الأداء", "رضا العملاء" };

    private static readonly IReadOnlyList<InteractiveTableContract> Contract = new List<InteractiveTableContract>
    {
        new("cash_transactions", "حركات النقد الفعلية (تدفق داخل/خارج)، صف لكل حركة", new List<InteractiveColumnContract>
        {
            new("transaction_date", "تاريخ الحركة"),
            new("amount_in", "قيمة النقد الداخل في هذه الحركة", Required: false),
            new("amount_out", "قيمة النقد الخارج في هذه الحركة", Required: false),
        }),
        new("profit_loss_entries", "قيود الإيراد والمصروفات المحاسبية، صف لكل قيد", new List<InteractiveColumnContract>
        {
            new("entry_at", "وقت القيد"),
            new("revenue", "قيمة الإيراد في هذا القيد", Required: false),
            new("expenses", "قيمة المصروفات في هذا القيد", Required: false),
        }),
        new("inventory_regions", "مخزون الشركة موزَّعًا على المناطق", new List<InteractiveColumnContract>
        {
            new("name", "اسم المنطقة"),
            new("grid_col", "عمود موضع المنطقة في الشبكة", Required: false),
            new("grid_row", "صف موضع المنطقة في الشبكة", Required: false),
            new("grid_span", "عدد الأعمدة التي تمتد عليها المنطقة", Required: false),
            new("inventory_value", "قيمة المخزون في هذه المنطقة"),
            new("delta_pct", "نسبة تغيّر قيمة المخزون", Required: false),
            new("active_items", "عدد الأصناف النشطة في هذه المنطقة", Required: false),
        }),
        new("automation_insights", "رؤى أتمتة العمليات المالية", new List<InteractiveColumnContract>
        {
            new("title", "عنوان الرؤية"), new("desc", "وصف الرؤية"), new("pct", "نسبة الاكتمال أو التغطية المئوية"),
        }, Required: false),
        new("automation_exceptions", "استثناءات أتمتة العمليات المالية", new List<InteractiveColumnContract>
        {
            new("title", "عنوان الاستثناء"), new("desc", "وصف الاستثناء"), new("pct", "نسبة الاكتمال أو المعالجة المئوية"),
        }, Required: false),
        new("operations_flow_efficiency", "كفاءة كل مرحلة في مخطط العمليات", new List<InteractiveColumnContract>
        {
            new("node_name", "اسم المرحلة (يطابق أحد أسماء المراحل الخمس الثابتة في اللوحة)"),
            new("efficiency_pct", "نسبة كفاءة هذه المرحلة"),
        }, Required: false),
    };

    public async Task<InteractiveDataResult> BuildAsync(string projectId, CancellationToken ct)
    {
        var missing = new[] { "حركات النقد (cash_transactions)", "قيود الإيراد والمصروفات (profit_loss_entries)", "مناطق المخزون (inventory_regions)" };
        var matched = await _matcher.MatchAsync(projectId, Contract, ct);
        if (matched is null) return InteractiveDataResult.Unlinked(missing);

        var cashRows = await _matcher.QueryAsync(BuildSelect(matched.Tables["cash_transactions"], "transaction_date", "amount_in", "amount_out"), matched.Context, ct);
        var plRows = await _matcher.QueryAsync(BuildSelect(matched.Tables["profit_loss_entries"], "entry_at", "revenue", "expenses"), matched.Context, ct);
        var regionRows = await _matcher.QueryAsync(BuildSelect(matched.Tables["inventory_regions"], "name", "grid_col", "grid_row", "grid_span", "inventory_value", "delta_pct", "active_items"), matched.Context, ct);
        if (cashRows.Count == 0 || plRows.Count == 0 || regionRows.Count == 0) return InteractiveDataResult.Unlinked(missing);

        var cash = cashRows
            .Select(r => (At: ParseDate(r, "transaction_date"), In: Num(r, "amount_in"), Out: Num(r, "amount_out")))
            .Where(r => r.At is not null)
            .Select(r => (At: r.At!.Value.Date, r.In, r.Out))
            .ToList();
        var pl = plRows
            .Select(r => (At: ParseDate(r, "entry_at"), Revenue: Num(r, "revenue"), Expenses: Num(r, "expenses")))
            .Where(r => r.At is not null)
            .Select(r => (At: r.At!.Value, r.Revenue, r.Expenses))
            .ToList();
        if (cash.Count == 0 || pl.Count == 0) return InteractiveDataResult.Unlinked(missing);

        var cashFlow = new
        {
            d = BuildCashWindow(cash, DailyBuckets(cash.Select(c => c.At).Max(), 31, cash.Select(c => c.At).Min()), i => (i + 1).ToString()),
            w = BuildCashWindow(cash, WeeklyBuckets(cash.Select(c => c.At).Max(), 12), i => "أ" + (i + 1)),
            m = BuildCashWindow(cash, MonthlyBuckets(cash.Select(c => c.At).Max(), 12), i => ArabicMonthsFull[i]),
        };

        var profitLoss = new
        {
            h = BuildPlWindow(pl, HourlyBucketsToday(pl.Select(p => p.At).Max())),
            d = BuildPlWindow(pl, DailyBuckets(pl.Select(p => p.At).Max(), 30, pl.Select(p => p.At).Min())),
            w = BuildPlWindow(pl, WeeklyBuckets(pl.Select(p => p.At).Max(), 12)),
            m = BuildPlWindow(pl, MonthlyBuckets(pl.Select(p => p.At).Max(), 12)),
        };

        var latestPlMonth = new DateTime(pl.Max(p => p.At).Year, pl.Max(p => p.At).Month, 1);
        var prevPlMonth = latestPlMonth.AddMonths(-1);
        double RevIn(DateTime from, DateTime toExclusive) => pl.Where(p => p.At >= from && p.At < toExclusive).Sum(p => p.Revenue);
        double ExpIn(DateTime from, DateTime toExclusive) => pl.Where(p => p.At >= from && p.At < toExclusive).Sum(p => p.Expenses);
        var thisRev = RevIn(latestPlMonth, latestPlMonth.AddMonths(1));
        var prevRev = RevIn(prevPlMonth, latestPlMonth);
        var thisExp = ExpIn(latestPlMonth, latestPlMonth.AddMonths(1));
        var prevExp = ExpIn(prevPlMonth, latestPlMonth);
        var kpis = new
        {
            rev = new { value = thisRev, deltaPct = PctChange(thisRev, prevRev) },
            exp = new { value = thisExp, deltaPct = PctChange(thisExp, prevExp) },
            pro = new { value = thisRev - thisExp, deltaPct = PctChange(thisRev - thisExp, prevRev - prevExp) },
        };

        var regions = regionRows.Select(r => new
        {
            name = Str(r, "name") ?? "",
            gridCol = (int)Num(r, "grid_col"),
            gridRow = (int)Num(r, "grid_row"),
            gridSpan = Math.Max(1, (int)Num(r, "grid_span")),
            inventoryValue = Num(r, "inventory_value"),
            deltaPct = Num(r, "delta_pct"),
            activeItems = (int)Num(r, "active_items"),
        }).ToList();

        List<object> insights = new(), exceptions = new();
        if (matched.Tables.TryGetValue("automation_insights", out var insTable))
        {
            var rows = await _matcher.QueryAsync(BuildSelect(insTable, "title", "desc", "pct"), matched.Context, ct);
            insights = rows.Where(r => Str(r, "title") is not null).Select(r => (object)new { title = Str(r, "title"), desc = Str(r, "desc") ?? "", pct = (int)Num(r, "pct") }).ToList();
        }
        if (matched.Tables.TryGetValue("automation_exceptions", out var excTable))
        {
            var rows = await _matcher.QueryAsync(BuildSelect(excTable, "title", "desc", "pct"), matched.Context, ct);
            exceptions = rows.Where(r => Str(r, "title") is not null).Select(r => (object)new { title = Str(r, "title"), desc = Str(r, "desc") ?? "", pct = (int)Num(r, "pct") }).ToList();
        }

        var efficiency = FlowNodeNames.Select(_ => 0.0).ToList();
        if (matched.Tables.TryGetValue("operations_flow_efficiency", out var effTable))
        {
            var rows = await _matcher.QueryAsync(BuildSelect(effTable, "node_name", "efficiency_pct"), matched.Context, ct);
            var byName = rows.Select(r => (Name: Str(r, "node_name"), Pct: Num(r, "efficiency_pct"))).Where(r => r.Name is not null).ToDictionary(r => r.Name!, r => r.Pct, StringComparer.Ordinal);
            efficiency = FlowNodeNames.Select(n => byName.TryGetValue(n, out var v) ? v : 0.0).ToList();
        }

        return InteractiveDataResult.Ready(new
        {
            kpis,
            cashFlow,
            profitLoss,
            regions,
            automationInsights = insights,
            automationExceptions = exceptions,
            operationsFlowEfficiency = efficiency,
        });
    }

    private static readonly string[] ArabicMonthsFull =
    {
        "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
        "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر",
    };

    private static double PctChange(double now, double before) => before != 0 ? Math.Round((now - before) / Math.Abs(before) * 100, 1) : 0;

    /// <summary>A bucket is [Start, EndExclusive) — used identically for cash and P&amp;L so
    /// day/week/month grouping logic is written once and shared by both ledgers.</summary>
    internal readonly record struct Bucket(DateTime Start, DateTime EndExclusive);

    private static List<Bucket> DailyBuckets(DateTime latest, int count, DateTime earliestAvailable)
    {
        var start = latest.Date.AddDays(-(count - 1));
        if (start < earliestAvailable.Date) start = earliestAvailable.Date;
        var days = (latest.Date - start).Days + 1;
        return Enumerable.Range(0, days).Select(i => new Bucket(start.AddDays(i), start.AddDays(i + 1))).ToList();
    }

    internal static List<Bucket> WeeklyBuckets(DateTime latest, int count)
    {
        var end = latest.Date.AddDays(1);
        var buckets = new List<Bucket>();
        for (var i = 0; i < count; i++) { var s = end.AddDays(-7 * (i + 1)); buckets.Add(new Bucket(s, end.AddDays(-7 * i))); }
        buckets.Reverse();
        return buckets;
    }

    internal static List<Bucket> MonthlyBuckets(DateTime latest, int count)
    {
        var latestMonth = new DateTime(latest.Year, latest.Month, 1);
        var buckets = new List<Bucket>();
        for (var i = 0; i < count; i++) { var s = latestMonth.AddMonths(-i); buckets.Add(new Bucket(s, s.AddMonths(1))); }
        buckets.Reverse();
        return buckets;
    }

    /// <summary>13 half-hour slots from 10:00 to 16:30 on the latest day data is available
    /// for — matching the engine's own fixed business-hours convention (plLabel's `10 +
    /// floor(i/2)`), not a free-form bucket count.</summary>
    private static List<Bucket> HourlyBucketsToday(DateTime latest)
    {
        var day = latest.Date;
        var start = day.AddHours(10);
        return Enumerable.Range(0, 13).Select(i => new Bucket(start.AddMinutes(30 * i), start.AddMinutes(30 * (i + 1)))).ToList();
    }

    internal static object BuildCashWindow(List<(DateTime At, double In, double Out)> cash, List<Bucket> buckets, Func<int, string> label)
    {
        if (buckets.Count == 0) return new { opening = 0, points = Array.Empty<object>() };
        var opening = cash.Where(c => c.At < buckets[0].Start).Sum(c => c.In - c.Out);
        var points = buckets.Select(b => (object)new
        {
            @in = cash.Where(c => c.At >= b.Start && c.At < b.EndExclusive).Sum(c => c.In),
            @out = cash.Where(c => c.At >= b.Start && c.At < b.EndExclusive).Sum(c => c.Out),
        }).ToList();
        return new { opening, points };
    }

    private static object BuildPlWindow(List<(DateTime At, double Revenue, double Expenses)> pl, List<Bucket> buckets)
    {
        var revenue = buckets.Select(b => pl.Where(p => p.At >= b.Start && p.At < b.EndExclusive).Sum(p => p.Revenue)).ToList();
        var expenses = buckets.Select(b => pl.Where(p => p.At >= b.Start && p.At < b.EndExclusive).Sum(p => p.Expenses)).ToList();
        return new { revenue, expenses };
    }
}
