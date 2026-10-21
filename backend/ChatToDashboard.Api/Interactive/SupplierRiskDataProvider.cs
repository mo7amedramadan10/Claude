using static ChatToDashboard.Api.Interactive.InteractiveRowHelpers;

namespace ChatToDashboard.Api.Interactive;

/// <summary>
/// §4.22's real data provider for supplier-risk ("مخاطر الموردين والتسليم" — 3 pages). Most of
/// the "overview" page (regional order-change/OTIF/late%, the KPI cards and their monthly
/// sparklines, the quarterly profit trend, and the per-region profit-impact waterfall) is
/// derived from one real order/delivery ledger — including profitImpact itself: each region's
/// contribution-to-YoY-profit-change is (thisYearRegionProfit − lastYearRegionProfit) ÷
/// lastYearTOTALProfit × 100, which is exactly how the prototype's own numbers already summed
/// to its overall grossProfitChangePct (verified: -4.5+0.4-0.4-0.9+3.0 ≈ -2.4 ≈ its own
/// kpis.2025.grossProfitChangePct of -2.3) — not a coincidence, a real identity. Supplier risk
/// scores and category storage-risk are NOT derivable from order counts (they are the kind of
/// proprietary scoring a real supply-chain risk system computes and stores), so those two come
/// from their own small tables rather than being guessed from a formula. Category scatter
/// colors are computed from real thresholds here (red ≥10% late or ≥10 storage risk, amber ≥8%
/// or ≥7) instead of requiring a separately-maintained color per category that could drift out
/// of sync with the numbers next to it. The suppliers list and supplierKpis.highRiskCount/
/// revenueAtRiskSar are a current snapshot (not year-keyed) — same as the prototype's own
/// non-year-keyed `data.suppliers` array — while lateDeliveryPct still varies by year since the
/// ledger genuinely supports that.
/// </summary>
public class SupplierRiskDataProvider : IInteractiveDataProvider
{
    public string TemplateId => "supplier-risk";

    private readonly InteractiveContractMatcher _matcher;

    public SupplierRiskDataProvider(InteractiveContractMatcher matcher) => _matcher = matcher;

    private const string Green = "#5E8C6A", Red = "#D64545", Amber = "#E09A2B";

    private static readonly IReadOnlyList<InteractiveTableContract> Contract = new List<InteractiveTableContract>
    {
        new("deliveries", "سجل الطلبات/التسليمات، كل صف بمنطقة مورده وإيراده وربحه وهل وصل في الوقت", new List<InteractiveColumnContract>
        {
            new("order_date", "تاريخ الطلب"),
            new("region", "منطقة المورد"),
            new("revenue", "إيراد هذا الطلب"),
            new("gross_profit", "مجمل ربح هذا الطلب"),
            new("on_time", "هل وصل هذا الطلب في الوقت المحدد (صحيح/خطأ)"),
        }),
        new("suppliers", "لقطة الموردين الحالية: إيرادهم ودرجة مخاطرهم", new List<InteractiveColumnContract>
        {
            new("name", "اسم المورد"),
            new("region", "منطقة المورد"),
            new("revenue_k", "متوسط إيراد المورد بالآلاف"),
            new("risk_score", "درجة مخاطر المورد"),
            new("action", "إجراء ذو أولوية مطلوب لهذا المورد، إن وُجد", Required: false),
            new("action_plan", "خطة هذا الإجراء", Required: false),
        }),
        new("category_risk_annual", "مخاطر التسليم والتخزين حسب الفئة لكل سنة", new List<InteractiveColumnContract>
        {
            new("year", "السنة"),
            new("category", "اسم الفئة"),
            new("late_pct", "نسبة التأخر لهذه الفئة"),
            new("storage_risk", "درجة مخاطر التخزين لهذه الفئة"),
            new("revenue_exposed", "الإيراد المعرّض لهذه الفئة بالآلاف"),
        }),
        new("inventory_categories_annual", "مخاطر المخزون حسب الفئة لكل سنة", new List<InteractiveColumnContract>
        {
            new("year", "السنة"),
            new("category", "اسم الفئة"),
            new("stockout_pct", "معدل نفاد المخزون لهذه الفئة"),
            new("waste_pct", "معدل الهدر لهذه الفئة"),
            new("coverage_days", "أيام تغطية المخزون لهذه الفئة"),
        }),
    };

    public async Task<InteractiveDataResult> BuildAsync(string projectId, CancellationToken ct)
    {
        var missing = new[]
        {
            "سجل التسليمات (deliveries)", "الموردون (suppliers)",
            "مخاطر الفئات السنوية (category_risk_annual)", "مخاطر المخزون السنوية (inventory_categories_annual)",
        };
        var matched = await _matcher.MatchAsync(projectId, Contract, ct);
        if (matched is null) return InteractiveDataResult.Unlinked(missing);

        var delivRows = await _matcher.QueryAsync(BuildSelect(matched.Tables["deliveries"], "order_date", "region", "revenue", "gross_profit", "on_time"), matched.Context, ct);
        var supplierRows = await _matcher.QueryAsync(BuildSelect(matched.Tables["suppliers"], "name", "region", "revenue_k", "risk_score", "action", "action_plan"), matched.Context, ct);
        var catRows = await _matcher.QueryAsync(BuildSelect(matched.Tables["category_risk_annual"], "year", "category", "late_pct", "storage_risk", "revenue_exposed"), matched.Context, ct);
        var invRows = await _matcher.QueryAsync(BuildSelect(matched.Tables["inventory_categories_annual"], "year", "category", "stockout_pct", "waste_pct", "coverage_days"), matched.Context, ct);
        if (delivRows.Count == 0 || supplierRows.Count == 0 || catRows.Count == 0 || invRows.Count == 0)
            return InteractiveDataResult.Unlinked(missing);

        var deliveries = delivRows
            .Select(r => (At: ParseDate(r, "order_date"), Region: Str(r, "region"), Revenue: Num(r, "revenue"), GrossProfit: Num(r, "gross_profit"), OnTime: Bool(r, "on_time")))
            .Where(r => r.At is not null && r.Region is not null)
            .Select(r => (At: r.At!.Value, Region: r.Region!, r.Revenue, r.GrossProfit, r.OnTime))
            .ToList();
        if (deliveries.Count == 0) return InteractiveDataResult.Unlinked(missing);

        var regions = deliveries.Select(d => d.Region).Distinct().ToList();
        var years = deliveries.Select(d => d.At.Year).Distinct().OrderBy(y => y).ToList();

        var regionMetrics = new Dictionary<string, object>();
        var kpis = new Dictionary<string, object>();
        var kpiSparklines = new Dictionary<string, Dictionary<string, List<double>>> { ["otif"] = new(), ["revenue"] = new(), ["grossProfit"] = new() };
        var profitImpact = new Dictionary<string, object>();
        var quarterlyProfit = new Dictionary<string, object>();
        var monthlyOtif = new Dictionary<string, object>();

        foreach (var year in years)
        {
            var thisYear = deliveries.Where(d => d.At.Year == year).ToList();
            var lastYear = deliveries.Where(d => d.At.Year == year - 1).ToList();

            regionMetrics[year.ToString()] = regions.Select(region =>
            {
                var thisR = thisYear.Where(d => d.Region == region).ToList();
                var lastR = lastYear.Where(d => d.Region == region).ToList();
                var orderChangePct = lastR.Count > 0 ? Math.Round((thisR.Count - lastR.Count) / (double)lastR.Count * 100) : (thisR.Count > 0 ? 100 : 0);
                var otifPct = thisR.Count > 0 ? Math.Round(thisR.Count(d => d.OnTime) / (double)thisR.Count * 100, 1) : 0;
                var latePct = Math.Round(100 - otifPct, 1);
                return new[] { orderChangePct, otifPct, latePct };
            }).ToList();

            var thisRevenue = thisYear.Sum(d => d.Revenue);
            var lastRevenue = lastYear.Sum(d => d.Revenue);
            var thisGp = thisYear.Sum(d => d.GrossProfit);
            var lastGp = lastYear.Sum(d => d.GrossProfit);
            var thisOtif = thisYear.Count > 0 ? thisYear.Count(d => d.OnTime) / (double)thisYear.Count * 100 : 0;
            var lastOtif = lastYear.Count > 0 ? lastYear.Count(d => d.OnTime) / (double)lastYear.Count * 100 : 0;

            kpis[year.ToString()] = new
            {
                otifPct = thisOtif,
                otifChangePts = thisOtif - lastOtif,
                revenue = thisRevenue,
                revenueChangePct = PctChange(thisRevenue, lastRevenue),
                grossProfit = thisGp,
                grossProfitChangePct = PctChange(thisGp, lastGp),
            };

            var months = MonthlySeries(thisYear);
            kpiSparklines["otif"][year.ToString()] = months.Otif;
            kpiSparklines["revenue"][year.ToString()] = months.Revenue;
            kpiSparklines["grossProfit"][year.ToString()] = months.GrossProfit;
            monthlyOtif[year.ToString()] = months.Otif;

            profitImpact[year.ToString()] = regions.Select(region =>
            {
                var thisRGp = thisYear.Where(d => d.Region == region).Sum(d => d.GrossProfit);
                var lastRGp = lastYear.Where(d => d.Region == region).Sum(d => d.GrossProfit);
                return lastGp != 0 ? Math.Round((thisRGp - lastRGp) / lastGp * 100, 1) : 0;
            }).ToList();

            quarterlyProfit[year.ToString()] = Enumerable.Range(1, 4)
                .Select(q => thisYear.Where(d => (d.At.Month - 1) / 3 + 1 == q).Sum(d => d.GrossProfit) / 1000)
                .ToList();
        }

        var supplierParsed = supplierRows
            .Select(r => (Name: Str(r, "name"), Region: Str(r, "region"), RevenueK: Num(r, "revenue_k"), RiskScore: Num(r, "risk_score"), Action: Str(r, "action"), ActionPlan: Str(r, "action_plan")))
            .Where(r => r.Name is not null && r.Region is not null)
            .ToList();
        var regionIndex = regions.Select((r, i) => (r, i)).ToDictionary(x => x.r, x => x.i);
        var suppliers = supplierParsed.Select(s => new
        {
            name = s.Name,
            revenueK = s.RevenueK,
            riskScore = s.RiskScore,
            region = regionIndex.TryGetValue(s.Region!, out var ri) ? ri : 0,
            action = s.Action ?? "",
            actionPlan = s.ActionPlan ?? "",
        }).ToList();

        var highRiskCount = supplierParsed.Count(s => s.RiskScore > 10);
        var revenueAtRiskSar = supplierParsed.Where(s => s.RiskScore > 7).Sum(s => s.RevenueK) * 1000;
        var supplierKpis = years.ToDictionary(y => y.ToString(), y =>
        {
            var thisYear = deliveries.Where(d => d.At.Year == y).ToList();
            var otif = thisYear.Count > 0 ? thisYear.Count(d => d.OnTime) / (double)thisYear.Count * 100 : 0;
            return (object)new { lateDeliveryPct = Math.Round(100 - otif, 1), highRiskCount, revenueAtRiskSar };
        });

        var categoryRisk = catRows
            .Select(r => (Year: (int)Num(r, "year"), Category: Str(r, "category"), Late: Num(r, "late_pct"), Storage: Num(r, "storage_risk"), Exposed: Num(r, "revenue_exposed")))
            .Where(r => r.Category is not null)
            .GroupBy(r => r.Year)
            .ToDictionary(g => g.Key.ToString(), g => (object)g.Select(r => new
            {
                name = r.Category,
                latePct = r.Late,
                storageRisk = r.Storage,
                revenueExposed = r.Exposed,
                color = r.Late >= 10 || r.Storage >= 10 ? Red : r.Late >= 8 || r.Storage >= 7 ? Amber : Green,
            }).ToList());

        var inventory = invRows
            .Select(r => (Year: (int)Num(r, "year"), Category: Str(r, "category"), Stockout: Num(r, "stockout_pct"), Waste: Num(r, "waste_pct"), Coverage: Num(r, "coverage_days")))
            .Where(r => r.Category is not null)
            .GroupBy(r => r.Year)
            .ToDictionary(g => g.Key.ToString(), g => (object)new
            {
                categories = g.Select(r => new { name = r.Category, stockoutPct = r.Stockout, wastePct = r.Waste, coverageDays = r.Coverage }).ToList(),
            });

        return InteractiveDataResult.Ready(new
        {
            years,
            regions = regions.Select(r => new { name = r }).ToList(),
            regionMetrics,
            kpis,
            kpiSparklines,
            profitImpact,
            quarterlyProfit,
            suppliers,
            supplierKpis,
            monthlyOtif,
            categoryRisk,
            inventory,
        });
    }

    private static double PctChange(double now, double before) => before != 0 ? Math.Round((now - before) / before * 100, 1) : 0;

    private static (List<double> Otif, List<double> Revenue, List<double> GrossProfit) MonthlySeries(List<(DateTime At, string Region, double Revenue, double GrossProfit, bool OnTime)> rows)
    {
        var otif = new List<double>();
        var revenue = new List<double>();
        var grossProfit = new List<double>();
        for (var m = 1; m <= 12; m++)
        {
            var inMonth = rows.Where(r => r.At.Month == m).ToList();
            otif.Add(inMonth.Count > 0 ? Math.Round(inMonth.Count(r => r.OnTime) / (double)inMonth.Count * 100, 1) : 0);
            revenue.Add(inMonth.Sum(r => r.Revenue));
            grossProfit.Add(inMonth.Sum(r => r.GrossProfit));
        }
        return (otif, revenue, grossProfit);
    }
}
