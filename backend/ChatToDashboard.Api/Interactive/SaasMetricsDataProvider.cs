using System.Text.Json;
using static ChatToDashboard.Api.Interactive.InteractiveRowHelpers;

namespace ChatToDashboard.Api.Interactive;

/// <summary>
/// §4.22's real data provider for saas-metrics ("إيرادات الاشتراكات والاحتفاظ") — everything
/// saas-metrics.js's JEEM_SAAS_MOUNT needs (MRR waterfall, weekly net-new, cohort retention,
/// plan mix, churn rate) is derived from ONE logical table: a weekly per-account MRR ledger
/// (account_id, week_start, mrr, plan) — the shape billing-analytics tools (ChartMogul,
/// Baremetrics, ProfitWell) already export. Deriving every figure from this single ledger,
/// rather than asking for several separately-aggregated tables that could disagree with each
/// other, is what keeps the waterfall (opening+new+expansion+contraction+churn=closing) and
/// the net-new weekly bars always arithmetically consistent with the plan mix and the churn
/// rate — they are all just different slices of the same account-week values. ltv_cac_ratio/
/// payback_months are the one genuinely external pair (depend on marketing/sales spend, not
/// derivable from MRR history) — a tiny optional single-row table. Per-plan NRR falls back to
/// the overall NRR when fewer than 2 accounts are active in a plan at the churn window's
/// start (too small a sample for its own ratio to mean anything) — never a fabricated number,
/// just the closest meaningful figure available. The four insight sentences are computed from
/// real aggregates each time (no fixed narrative), mirroring supplier-risk/supply-command's
/// precedent of generalizing "always the same story" prototype text into real conditions.
/// </summary>
public class SaasMetricsDataProvider : IInteractiveDataProvider
{
    public string TemplateId => "saas-metrics";

    private readonly InteractiveContractMatcher _matcher;

    public SaasMetricsDataProvider(InteractiveContractMatcher matcher) => _matcher = matcher;

    private static readonly int[] Windows = { 13, 26 };
    private const int ChurnWindowWeeks = 4; // ~one month at weekly cadence, for logo churn % and per-plan NRR/churn

    private static readonly IReadOnlyList<InteractiveTableContract> Contract = new List<InteractiveTableContract>
    {
        new("account_mrr_snapshots", "لقطة أسبوعية للدخل الشهري المتكرر (MRR) لكل حساب اشتراك — صف لكل حساب في كل أسبوع كان نشطًا فيه (غياب الحساب في أسبوع يعني توقّف اشتراكه)", new List<InteractiveColumnContract>
        {
            new("account_id", "معرّف حساب الاشتراك"),
            new("week_start", "بداية الأسبوع لهذه اللقطة"),
            new("mrr", "قيمة الدخل الشهري المتكرر لهذا الحساب في هذا الأسبوع"),
            new("plan", "اسم باقة الاشتراك لهذا الحساب في هذا الأسبوع", Required: false),
        }),
        new("kpi_summary", "مؤشرا تكلفة اكتساب العميل والاسترداد (صف واحد، خارج بيانات الاشتراكات نفسها)", new List<InteractiveColumnContract>
        {
            new("ltv_cac_ratio", "نسبة القيمة الدائمة للعميل إلى تكلفة اكتسابه", Required: false),
            new("payback_months", "عدد الأشهر اللازمة لاسترداد تكلفة اكتساب العميل", Required: false),
        }, Required: false),
    };

    private static readonly string[] ArabicMonths =
    {
        "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
        "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر",
    };

    internal record Snapshot(string AccountId, DateTime Week, double Mrr, string? Plan);
    private record MrrWindow(double Opening, double New, double Expansion, double Contraction, double Churn);
    private record NetNewWindow(List<string> Labels, List<double> Vals);
    private record PlanMix(string Name, int Accounts, double Revenue, double Arpa, double ChurnPct, double Nrr);
    internal record CohortData(List<string> Months, List<string> Ages, List<List<int?>> Logo, List<List<int?>> Rev);

    public async Task<InteractiveDataResult> BuildAsync(string projectId, CancellationToken ct)
    {
        var missing = new[] { "لقطات الدخل الشهري المتكرر الأسبوعية (account_mrr_snapshots)" };
        var matched = await _matcher.MatchAsync(projectId, Contract, ct);
        if (matched is null) return InteractiveDataResult.Unlinked(missing);

        var snapTable = matched.Tables["account_mrr_snapshots"];
        var snapRows = await _matcher.QueryAsync(BuildSelect(snapTable, "account_id", "week_start", "mrr", "plan"), matched.Context, ct);

        var snapshots = snapRows
            .Select(r => (Id: Str(r, "account_id"), Week: ParseDate(r, "week_start"), Mrr: Num(r, "mrr"), Plan: Str(r, "plan")))
            .Where(r => r.Id is not null && r.Week is not null)
            .Select(r => new Snapshot(r.Id!, r.Week!.Value.Date, r.Mrr, r.Plan))
            .ToList();
        if (snapshots.Count == 0) return InteractiveDataResult.Unlinked(missing);

        var weeks = snapshots.Select(s => s.Week).Distinct().OrderBy(w => w).ToList();
        if (weeks.Count < 2) return InteractiveDataResult.Unlinked(missing);
        var weekIndex = weeks.Select((w, i) => (w, i)).ToDictionary(x => x.w, x => x.i);

        var perAccount = new Dictionary<string, Dictionary<int, double>>(StringComparer.Ordinal);
        var latestPlanByAccount = new Dictionary<string, (DateTime Week, string Plan)>(StringComparer.Ordinal);
        foreach (var s in snapshots)
        {
            if (!perAccount.TryGetValue(s.AccountId, out var byWeek)) perAccount[s.AccountId] = byWeek = new();
            byWeek[weekIndex[s.Week]] = s.Mrr;
            if (!string.IsNullOrWhiteSpace(s.Plan) &&
                (!latestPlanByAccount.TryGetValue(s.AccountId, out var cur) || s.Week >= cur.Week))
                latestPlanByAccount[s.AccountId] = (s.Week, s.Plan!);
        }
        var accountIds = perAccount.Keys.ToList();
        double ValueAt(string id, int weekIdx) => perAccount.TryGetValue(id, out var m) && m.TryGetValue(weekIdx, out var v) ? v : 0;

        // One pass over every weekly transition, classified into the same four movement
        // buckets the engine's waterfall shows — reused for both the 13/26-week windows and
        // the per-week net-new bars, so a window's total always equals the sum of its weeks.
        var transitions = new List<(int WeekIdx, double New, double Expansion, double Contraction, double Churn)>();
        for (var t = 1; t < weeks.Count; t++)
        {
            double n = 0, x = 0, c = 0, ch = 0;
            foreach (var id in accountIds)
            {
                var prev = ValueAt(id, t - 1);
                var curr = ValueAt(id, t);
                if (prev <= 0 && curr > 0) n += curr;
                else if (prev > 0 && curr <= 0) ch += -prev;
                else if (prev > 0 && curr > prev) x += curr - prev;
                else if (prev > 0 && curr > 0 && curr < prev) c += curr - prev;
            }
            transitions.Add((t, n, x, c, ch));
        }

        var mrrOut = new Dictionary<string, MrrWindow>();
        var netNewOut = new Dictionary<string, NetNewWindow>();
        foreach (var window in Windows)
        {
            var startIdx = Math.Max(0, weeks.Count - 1 - window);
            var opening = accountIds.Sum(id => ValueAt(id, startIdx));
            var inWindow = transitions.Where(tr => tr.WeekIdx > startIdx).ToList();
            mrrOut[window.ToString()] = new MrrWindow(
                opening, inWindow.Sum(tr => tr.New), inWindow.Sum(tr => tr.Expansion),
                inWindow.Sum(tr => tr.Contraction), inWindow.Sum(tr => tr.Churn));
            netNewOut[window.ToString()] = new NetNewWindow(
                inWindow.Select(tr => FormatWeekLabel(weeks[tr.WeekIdx])).ToList(),
                inWindow.Select(tr => tr.New + tr.Expansion + tr.Contraction + tr.Churn).ToList());
        }

        var latestIdx = weeks.Count - 1;
        var activeNow = accountIds.Where(id => ValueAt(id, latestIdx) > 0).ToList();
        var totalMrrNow = activeNow.Sum(id => ValueAt(id, latestIdx));
        var arpa = activeNow.Count > 0 ? totalMrrNow / activeNow.Count : 0;

        var churnStartIdx = Math.Max(0, latestIdx - ChurnWindowWeeks);
        var activeAtChurnStart = accountIds.Where(id => ValueAt(id, churnStartIdx) > 0).ToList();
        var churnedInWindow = activeAtChurnStart.Count(id => ValueAt(id, latestIdx) <= 0);
        var churnRatePct = activeAtChurnStart.Count > 0 ? churnedInWindow / (double)activeAtChurnStart.Count * 100 : 0;
        var startSumAll = activeAtChurnStart.Sum(id => ValueAt(id, churnStartIdx));
        var endSumAll = activeAtChurnStart.Sum(id => ValueAt(id, latestIdx));
        var overallNrr = startSumAll > 0 ? endSumAll / startSumAll * 100 : 0;

        string PlanOf(string id) => latestPlanByAccount.TryGetValue(id, out var p) ? p.Plan : "غير محدد";
        var plans = activeNow.GroupBy(PlanOf).Select(g =>
        {
            var revenue = g.Sum(id => ValueAt(id, latestIdx));
            var activeAtStart = g.Where(id => ValueAt(id, churnStartIdx) > 0).ToList();
            var churnedPlan = activeAtStart.Count(id => ValueAt(id, latestIdx) <= 0);
            var planChurnPct = activeAtStart.Count > 0 ? churnedPlan / (double)activeAtStart.Count * 100 : 0;
            var startSum = activeAtStart.Sum(id => ValueAt(id, churnStartIdx));
            var endSum = activeAtStart.Sum(id => ValueAt(id, latestIdx));
            // Falls back to the site-wide NRR when too few accounts in this plan were active
            // at the churn window's start for the plan's own ratio to be meaningful.
            var planNrr = activeAtStart.Count >= 2 && startSum > 0 ? endSum / startSum * 100 : overallNrr;
            var count = g.Count();
            return new PlanMix(g.Key, count, revenue, count > 0 ? revenue / count : 0, planChurnPct, planNrr);
        }).OrderByDescending(p => p.Revenue).ToList();

        double? ltvCac = null, paybackMonths = null;
        if (matched.Tables.TryGetValue("kpi_summary", out var kpiTable))
        {
            var kpiRows = await _matcher.QueryAsync(BuildSelect(kpiTable, "ltv_cac_ratio", "payback_months"), matched.Context, ct);
            if (kpiRows.Count > 0)
            {
                ltvCac = Num(kpiRows[0], "ltv_cac_ratio");
                paybackMonths = Num(kpiRows[0], "payback_months");
            }
        }

        var cohorts = BuildCohorts(snapshots, weeks);
        var insights = BuildInsights(mrrOut, churnRatePct, netNewOut, plans, cohorts);

        return InteractiveDataResult.Ready(new
        {
            ranges = Windows,
            mrr = mrrOut.ToDictionary(kv => kv.Key, kv => new { opening = kv.Value.Opening, @new = kv.Value.New, expansion = kv.Value.Expansion, contraction = kv.Value.Contraction, churn = kv.Value.Churn }),
            churnRatePct,
            arpa,
            ltvCac,
            paybackMonths,
            netNew = netNewOut.ToDictionary(kv => kv.Key, kv => new { labels = kv.Value.Labels, vals = kv.Value.Vals }),
            cohorts = new { months = cohorts.Months, ages = cohorts.Ages, logo = cohorts.Logo, rev = cohorts.Rev },
            plans = plans.Select(p => new { name = p.Name, accounts = p.Accounts, revenue = p.Revenue, arpa = p.Arpa, churnPct = p.ChurnPct, nrr = p.Nrr }),
            insights,
        });
    }

    private static string FormatWeekLabel(DateTime week) => $"{week.Day} {ArabicMonths[week.Month - 1]}";

    /// <summary>Cohort = the calendar month of an account's earliest nonzero snapshot. Age in
    /// months = months elapsed since that cohort month. "Logo" retention = % of the cohort's
    /// accounts with a nonzero snapshot anywhere in that age's calendar month; "revenue"
    /// retention = that age month's share of the cohort's own age-0 total. Null (not a 0) for
    /// an age the data hasn't reached yet, so the engine's own heatmap leaves it blank rather
    /// than drawing a fabricated 0% — same distinction the demo data's own nulls encode.</summary>
    internal static CohortData BuildCohorts(List<Snapshot> snapshots, List<DateTime> weeks)
    {
        DateTime MonthOf(DateTime w) => new(w.Year, w.Month, 1);
        var cohortMonthByAccount = snapshots
            .Where(s => s.Mrr > 0)
            .GroupBy(s => s.AccountId)
            .ToDictionary(g => g.Key, g => MonthOf(g.Min(s => s.Week)));
        if (cohortMonthByAccount.Count == 0) return new CohortData(new(), new(), new(), new());

        var latestMonth = MonthOf(weeks[^1]);
        var cohortMonths = cohortMonthByAccount.Values.Distinct().OrderBy(m => m).ToList();
        var recentCohorts = cohortMonths.Count > 9 ? cohortMonths.Skip(cohortMonths.Count - 9).ToList() : cohortMonths;
        const int maxAge = 12;
        var ages = Enumerable.Range(0, maxAge + 1).Select(a => $"ش{a}").ToList();

        // Account's value for a given calendar month = average of its snapshots that fall in it.
        var byAccountMonth = snapshots.GroupBy(s => (s.AccountId, Month: MonthOf(s.Week)))
            .ToDictionary(g => g.Key, g => g.Average(s => s.Mrr));
        double ValueInMonth(string accountId, DateTime month) =>
            byAccountMonth.TryGetValue((accountId, month), out var v) ? v : 0;

        var monthLabels = new List<string>();
        var logo = new List<List<int?>>();
        var rev = new List<List<int?>>();
        foreach (var cohort in recentCohorts)
        {
            monthLabels.Add($"{ArabicMonths[cohort.Month - 1]} {cohort.Year % 100:D2}");
            var members = cohortMonthByAccount.Where(kv => kv.Value == cohort).Select(kv => kv.Key).ToList();
            var age0Revenue = members.Sum(id => ValueInMonth(id, cohort));
            var logoRow = new List<int?>();
            var revRow = new List<int?>();
            for (var age = 0; age <= maxAge; age++)
            {
                var ageMonth = cohort.AddMonths(age);
                if (ageMonth > latestMonth) { logoRow.Add(null); revRow.Add(null); continue; }
                var activeCount = members.Count(id => ValueInMonth(id, ageMonth) > 0);
                var ageRevenue = members.Sum(id => ValueInMonth(id, ageMonth));
                logoRow.Add(members.Count > 0 ? (int)Math.Round(activeCount / (double)members.Count * 100) : 0);
                revRow.Add(age0Revenue > 0 ? (int)Math.Round(ageRevenue / age0Revenue * 100) : 0);
            }
            logo.Add(logoRow);
            rev.Add(revRow);
        }

        return new CohortData(monthLabels, ages, logo, rev);
    }

    private static List<object> BuildInsights(
        Dictionary<string, MrrWindow> mrrOut, double churnRatePct, Dictionary<string, NetNewWindow> netNewOut,
        List<PlanMix> plans, CohortData cohorts)
    {
        var insights = new List<object>();

        var mrr13 = mrrOut["13"];
        var nrr13 = mrr13.Opening > 0 ? (mrr13.Opening + mrr13.Expansion + mrr13.Contraction + mrr13.Churn) / mrr13.Opening * 100 : 0;
        insights.Add(new
        {
            anchor = "smWfC",
            title = nrr13 >= 100 ? "الدخل ينمو رغم إلغاء بعض الحسابات" : "الدخل تحت ضغط من التسرب",
            text = $"صافي الاحتفاظ قرابة {nrr13:F1}% مع إلغاء حسابات {churnRatePct:F2}% شهريًا — " +
                   (nrr13 >= 100 ? "الحسابات الباقية تنمو بما يكفي لتعويض المغادرة." : "نمو الحسابات الباقية لا يعوّض المغادرة بعد."),
        });

        if (plans.Count > 0)
        {
            var accountsTotal = plans.Sum(p => p.Accounts);
            var revenueTotal = plans.Sum(p => p.Revenue);
            var topByAccounts = plans.OrderByDescending(p => p.Accounts).First();
            var topByRevenue = plans.OrderByDescending(p => p.Revenue).First();
            if (accountsTotal > 0 && revenueTotal > 0)
            {
                var accShare = (double)topByAccounts.Accounts / accountsTotal * 100;
                var accPlanRevShare = topByAccounts.Revenue / revenueTotal * 100;
                insights.Add(new
                {
                    anchor = "smPlanC",
                    title = $"{topByAccounts.Name} {accShare:F0}% من العملاء و{accPlanRevShare:F1}% من الإيراد",
                    text = topByRevenue.Name == topByAccounts.Name
                        ? $"نفس الباقة الأعلى عددًا وإيرادًا — {topByAccounts.Name} تقود الاثنين معًا."
                        : $"{topByRevenue.Name} {topByRevenue.Revenue / revenueTotal * 100:F1}% من الإيراد رغم كونها أقل عددًا من {topByAccounts.Name}.",
                });
            }
        }

        var netNew13 = netNewOut["13"];
        var worstIdx = -1;
        for (var i = 0; i < netNew13.Vals.Count; i++)
            if (netNew13.Vals[i] < 0 && (worstIdx < 0 || netNew13.Vals[i] < netNew13.Vals[worstIdx])) worstIdx = i;
        insights.Add(new
        {
            anchor = "smNetC",
            title = worstIdx >= 0 ? "أسبوع سالب واحد على الأقل في آخر 13 أسبوعًا" : "لا أسابيع سالبة في آخر 13 أسبوعًا",
            text = worstIdx >= 0
                ? $"أدنى أسبوع كان {netNew13.Labels[worstIdx]} بصافي {netNew13.Vals[worstIdx]:N0} ر.س."
                : "صافي الدخل الجديد موجب في كل أسبوع من آخر 13 أسبوعًا.",
        });

        if (cohorts.Months.Count >= 2)
        {
            var oldest = cohorts.Logo[0];
            var newest = cohorts.Logo[^1];
            var compareAge = Math.Min(oldest.Count, newest.Count) > 1 ? 1 : 0;
            if (oldest[compareAge] is int oldVal && newest[compareAge] is int newVal)
            {
                insights.Add(new
                {
                    anchor = "smCohC",
                    title = newVal >= oldVal ? "الاحتفاظ يتحسن من دفعة لأخرى" : "الاحتفاظ يتراجع من دفعة لأخرى",
                    text = $"دفعة {cohorts.Months[^1]} عند الشهر {compareAge} احتفظت بـ{newVal}% من حساباتها، مقابل {oldVal}% لدفعة {cohorts.Months[0]} في نفس العمر.",
                });
            }
        }

        return insights;
    }
}
