using static ChatToDashboard.Api.Interactive.InteractiveRowHelpers;

namespace ChatToDashboard.Api.Interactive;

/// <summary>
/// §4.22's real data provider for treasury-funding ("التمويل والسيولة النقدية"). Most of this
/// dashboard maps onto flat tables directly (payment systems, per-year debt maturities, a named
/// financing-rate time series, banks). "الرصيد الافتتاحي/الحالي/حركة جديدة" (opening/current/
/// net-movement) is NOT a free-form project-defined list despite looking like one in the
/// prototype — it is a fixed three-row structural breakdown every treasury reports the same
/// way, so it is derived here from one real cash_balance_history ledger (latest balance =
/// current, balance at this calendar month's start = opening, their difference = net movement;
/// each row's own delta% compares against the same figure a month earlier) rather than being a
/// table a project would supply three independently-typed rows for. Each sparkline is its own
/// row's last 5 history points, min-max normalized to a 20–100% height range — the dashboard
/// renders spark heights as raw percentages (`height:${h}%`), not currency, so real history
/// has to be rescaled the same way the prototype's already-normalized demo numbers were.
/// Intraday today-vs-yesterday activity is the one optional piece: a project with no
/// transaction-level ledger still gets every other section, with intraday showing the
/// engine's own "no data yet" state instead of blocking the whole dashboard.
/// </summary>
public class TreasuryFundingDataProvider : IInteractiveDataProvider
{
    public string TemplateId => "treasury-funding";

    private readonly InteractiveContractMatcher _matcher;

    public TreasuryFundingDataProvider(InteractiveContractMatcher matcher) => _matcher = matcher;

    private static readonly string[] PaletteColors = { "#127DBD", "#D9822B", "#00A89F", "#6E62B0", "#7CB342", "#C2569B" };
    private static readonly string[] CashRowColors = { "#1F4E9C", "#28B6E6", "#E8794A" };
    private static readonly string[] ArabicMonths =
    {
        "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
        "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر",
    };
    private static readonly int[] BusinessHours = Enumerable.Range(8, 12).ToArray(); // 8:00-19:00

    private static readonly IReadOnlyList<InteractiveTableContract> Contract = new List<InteractiveTableContract>
    {
        new("payment_systems", "لقطة أنظمة المدفوعات الحالية (سريع، سداد، تسوية الأوراق المالية، ...)", new List<InteractiveColumnContract>
        {
            new("name", "اسم نظام المدفوعات"),
            new("value", "القيمة الحالية لهذا النظام"),
            new("delta_pct", "نسبة التغير"),
        }),
        new("debt_maturities", "استحقاقات الدَّين وشهادات إيداع الوسطاء شهريًا لكل سنة", new List<InteractiveColumnContract>
        {
            new("year", "السنة"),
            new("month", "رقم الشهر (1 إلى 12)"),
            new("debt_amount", "مبلغ استحقاق الدَّين لهذا الشهر"),
            new("cds_amount", "مبلغ استحقاق شهادات إيداع الوسطاء لهذا الشهر"),
            new("cap_amount", "سقف الاستحقاق المستهدف لهذا الشهر", Required: false),
            new("debt_rate_pct", "سعر تمويل الدَّين الحالي لهذه السنة", Required: false),
            new("cds_rate_pct", "سعر شهادات إيداع الوسطاء الحالي لهذه السنة", Required: false),
        }),
        new("financing_rate_observations", "سجل زمني لأسعار التمويل المختلفة (سايبور، سلف البنك المركزي، ...)", new List<InteractiveColumnContract>
        {
            new("observed_at", "تاريخ الرصد"),
            new("series_name", "اسم سعر التمويل"),
            new("rate_pct", "قيمة السعر بالنسبة المئوية"),
        }),
        new("cash_balance_history", "سجل الرصيد النقدي الكلي عبر الزمن (صف لكل رصد)", new List<InteractiveColumnContract>
        {
            new("recorded_at", "وقت هذا الرصد"),
            new("balance", "الرصيد النقدي الكلي في هذه اللحظة"),
        }),
        new("banks", "أرصدة الحسابات البنكية", new List<InteractiveColumnContract>
        {
            new("name", "اسم البنك"),
            new("value", "الرصيد في هذا البنك"),
            new("color", "لون مخصّص للبنك (قيمة HEX)", Required: false),
        }),
        new("intraday_transactions", "حركات نقدية بالطابع الزمني، لمقارنة نشاط اليوم بالأمس", new List<InteractiveColumnContract>
        {
            new("occurred_at", "وقت الحركة"),
            new("amount", "قيمة الحركة"),
        }, Required: false),
    };

    public async Task<InteractiveDataResult> BuildAsync(string projectId, CancellationToken ct)
    {
        var missing = new[]
        {
            "أنظمة المدفوعات (payment_systems)", "استحقاقات الدَّين (debt_maturities)",
            "أسعار التمويل (financing_rate_observations)", "سجل الرصيد النقدي (cash_balance_history)", "البنوك (banks)",
        };
        var matched = await _matcher.MatchAsync(projectId, Contract, ct);
        if (matched is null) return InteractiveDataResult.Unlinked(missing);

        var paymentRows = await _matcher.QueryAsync(BuildSelect(matched.Tables["payment_systems"], "name", "value", "delta_pct"), matched.Context, ct);
        var maturityRows = await _matcher.QueryAsync(BuildSelect(matched.Tables["debt_maturities"], "year", "month", "debt_amount", "cds_amount", "cap_amount", "debt_rate_pct", "cds_rate_pct"), matched.Context, ct);
        var rateRows = await _matcher.QueryAsync(BuildSelect(matched.Tables["financing_rate_observations"], "observed_at", "series_name", "rate_pct"), matched.Context, ct);
        var balanceRows = await _matcher.QueryAsync(BuildSelect(matched.Tables["cash_balance_history"], "recorded_at", "balance"), matched.Context, ct);
        var bankRows = await _matcher.QueryAsync(BuildSelect(matched.Tables["banks"], "name", "value", "color"), matched.Context, ct);
        if (paymentRows.Count == 0 || maturityRows.Count == 0 || rateRows.Count == 0 || balanceRows.Count == 0 || bankRows.Count == 0)
            return InteractiveDataResult.Unlinked(missing);

        var tick = paymentRows.Select(r => new { name = Str(r, "name") ?? "", value = Num(r, "value"), deltaPct = Num(r, "delta_pct") }).ToList();

        var maturities = BuildMaturities(maturityRows);
        if (maturities is null) return InteractiveDataResult.Unlinked(missing);

        var financingRates = BuildFinancingRates(rateRows);
        if (financingRates is null) return InteractiveDataResult.Unlinked(missing);

        var balances = balanceRows
            .Select(r => (At: ParseDate(r, "recorded_at"), Balance: Num(r, "balance")))
            .Where(r => r.At is not null)
            .Select(r => (At: r.At!.Value, r.Balance))
            .OrderBy(r => r.At)
            .ToList();
        if (balances.Count == 0) return InteractiveDataResult.Unlinked(missing);
        var cashPosition = BuildCashPosition(balances);

        var colorIdx = 0;
        var banks = bankRows.Select(r => new
        {
            name = Str(r, "name") ?? "",
            value = Num(r, "value"),
            color = Str(r, "color") ?? PaletteColors[colorIdx++ % PaletteColors.Length],
        }).ToList();

        object intraday = new { hours = Array.Empty<string>(), today = Array.Empty<double>(), yesterday = Array.Empty<double>() };
        if (matched.Tables.TryGetValue("intraday_transactions", out var intraTable))
        {
            var rows = await _matcher.QueryAsync(BuildSelect(intraTable, "occurred_at", "amount"), matched.Context, ct);
            var txns = rows
                .Select(r => (At: ParseDate(r, "occurred_at"), Amount: Num(r, "amount")))
                .Where(r => r.At is not null)
                .Select(r => (At: r.At!.Value, r.Amount))
                .ToList();
            if (txns.Count > 0) intraday = BuildIntraday(txns);
        }

        return InteractiveDataResult.Ready(new { tick, maturities, financingRates, cashPosition, banks, intraday });
    }

    private static object? BuildMaturities(List<Dictionary<string, System.Text.Json.JsonElement>> rows)
    {
        var byYear = rows
            .Select(r => (Year: (int)Num(r, "year"), Month: (int)Num(r, "month"), Debt: Num(r, "debt_amount"), Cds: Num(r, "cds_amount"), Cap: Num(r, "cap_amount"), DebtRate: Num(r, "debt_rate_pct"), CdsRate: Num(r, "cds_rate_pct")))
            .Where(r => r.Year > 0 && r.Month is >= 1 and <= 12)
            .GroupBy(r => r.Year)
            .ToList();
        if (byYear.Count == 0) return null;

        var years = byYear.Select(g => g.Key).OrderBy(y => y).ToList();
        var byYearOut = new Dictionary<string, object>();
        var ratesOut = new Dictionary<string, object>();
        foreach (var g in byYear)
        {
            var byMonth = g.ToDictionary(r => r.Month);
            var months = new List<string>();
            var debt = new List<double>();
            var cds = new List<double>();
            var cap = new List<double>();
            for (var m = 1; m <= 12; m++)
            {
                months.Add(ArabicMonths[m - 1]);
                if (byMonth.TryGetValue(m, out var r)) { debt.Add(r.Debt); cds.Add(r.Cds); cap.Add(r.Cap > 0 ? r.Cap : Math.Max(r.Debt, r.Cds)); }
                else { debt.Add(0); cds.Add(0); cap.Add(0); }
            }
            byYearOut[g.Key.ToString()] = new { months, debt, cds, cap };
            var rated = g.FirstOrDefault(r => r.DebtRate > 0 || r.CdsRate > 0);
            ratesOut[g.Key.ToString()] = new { debtPct = rated.DebtRate, cdsPct = rated.CdsRate };
        }
        return new { years, byYear = byYearOut, rates = ratesOut };
    }

    private static object? BuildFinancingRates(List<Dictionary<string, System.Text.Json.JsonElement>> rows)
    {
        var parsed = rows
            .Select(r => (At: ParseDate(r, "observed_at"), Series: Str(r, "series_name"), Rate: Num(r, "rate_pct")))
            .Where(r => r.At is not null && r.Series is not null)
            .Select(r => (At: r.At!.Value.Date, Series: r.Series!, r.Rate))
            .ToList();
        if (parsed.Count == 0) return null;

        var labelDates = parsed.Select(r => r.At).Distinct().OrderBy(d => d).ToList();
        var labels = labelDates.Select(d => $"{ArabicMonths[d.Month - 1][..3]} {d.Day}").ToList();
        var seriesNames = parsed.Select(r => r.Series).Distinct().ToList();
        var byDate = parsed.GroupBy(r => (r.At, r.Series)).ToDictionary(g => g.Key, g => g.Last().Rate);

        var colorIdx = 0;
        var series = seriesNames.Select(name => new
        {
            name,
            color = PaletteColors[colorIdx++ % PaletteColors.Length],
            vals = labelDates.Select(d => byDate.TryGetValue((d, name), out var v) ? (double?)v : null).ToList(),
        }).ToList();

        return new { labels, series };
    }

    /// <summary>Opening = balance as of this calendar month's start (the closest record at or
    /// before it); current = the latest record overall; net movement = their difference. Each
    /// row's own delta% compares that same figure to a month earlier, so "opening" moves
    /// against last month's opening, "current" against last month's closing balance, and "net
    /// movement" against last month's own net movement — never against an unrelated figure.</summary>
    internal static List<object> BuildCashPosition(List<(DateTime At, double Balance)> balances)
    {
        var latest = balances[^1];
        var thisMonthStart = new DateTime(latest.At.Year, latest.At.Month, 1);
        var prevMonthStart = thisMonthStart.AddMonths(-1);

        double BalanceAtOrBefore(DateTime cutoff) =>
            balances.Where(b => b.At <= cutoff).Select(b => b.Balance).DefaultIfEmpty(balances[0].Balance).Last();
        double BalanceAtOrAfter(DateTime cutoff) =>
            balances.Where(b => b.At >= cutoff).Select(b => b.Balance).DefaultIfEmpty(latest.Balance).First();

        var thisOpening = BalanceAtOrAfter(thisMonthStart);
        var thisCurrent = latest.Balance;
        var thisNew = thisCurrent - thisOpening;

        var prevOpening = BalanceAtOrAfter(prevMonthStart);
        var prevClosing = BalanceAtOrBefore(thisMonthStart.AddDays(-1));
        var prevNew = prevClosing - prevOpening;

        double Pct(double now, double before) => before != 0 ? (now - before) / Math.Abs(before) * 100 : 0;

        List<double> SparkFor(DateTime from)
        {
            var window = balances.Where(b => b.At >= from.AddMonths(-1)).Select(b => b.Balance).ToList();
            var tail = window.Count > 5 ? window.Skip(window.Count - 5).ToList() : window;
            if (tail.Count == 0) return new();
            var min = tail.Min(); var max = tail.Max();
            return tail.Select(v => max > min ? 20 + (v - min) / (max - min) * 80 : 60.0).ToList();
        }

        return new List<object>
        {
            new { label = "الرصيد الافتتاحي", value = thisOpening, deltaPct = Pct(thisOpening, prevOpening), color = CashRowColors[0], spark = SparkFor(thisMonthStart) },
            new { label = "الرصيد الحالي", value = thisCurrent, deltaPct = Pct(thisCurrent, prevClosing), color = CashRowColors[1], spark = SparkFor(thisMonthStart) },
            new { label = "حركة جديدة", value = thisNew, deltaPct = Pct(thisNew, prevNew), color = CashRowColors[2], spark = SparkFor(thisMonthStart) },
        };
    }

    private static object BuildIntraday(List<(DateTime At, double Amount)> txns)
    {
        var latestDay = txns.Max(t => t.At).Date;
        var yesterday = latestDay.AddDays(-1);
        var hours = BusinessHours.Select(h => $"{h}:00").ToList();
        double SumHour(DateTime day, int hour) => txns.Where(t => t.At.Date == day && t.At.Hour == hour).Sum(t => t.Amount);
        var today = BusinessHours.Select(h => SumHour(latestDay, h)).ToList();
        var yesterdayVals = BusinessHours.Select(h => SumHour(yesterday, h)).ToList();
        return new { hours, today, yesterday = yesterdayVals };
    }
}
