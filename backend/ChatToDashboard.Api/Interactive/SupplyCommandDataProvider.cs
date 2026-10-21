using static ChatToDashboard.Api.Interactive.InteractiveRowHelpers;

namespace ChatToDashboard.Api.Interactive;

/// <summary>
/// §4.22's real data provider for supply-command ("مركز قيادة سلسلة الإمداد" — 3 pages).
/// Everything in this dashboard — the KPIs, the 24-month drill-down trend, OTIF, the per-
/// channel/warehouse/priority/supplier breakdowns, the loss matrix, the efficiency waterfall
/// and the margin comparison — is derived from one real order-level ledger (ممثل supply_orders)
/// rather than several separately-maintained pre-aggregated tables that could drift out of
/// sync with each other. Always reports exactly two years ([latest-1, latest]), with a year
/// that has no real orders simply rolling up to zero months, so the engine's own fixed two-year
/// drill UI always has real (if sometimes empty) data behind it instead of silently falling
/// back to its placeholder '2024'/'2025' labels. "Revenue at risk" (the risk page) is revenue
/// tied to problem orders (late or stockout) per dimension; "profit risk by channel" (the
/// profit page) is specifically the gross-profit impact of late orders per channel — two
/// different, both real, framings of the same ledger, not duplicates of each other.
/// </summary>
public class SupplyCommandDataProvider : IInteractiveDataProvider
{
    public string TemplateId => "supply-command";

    private readonly InteractiveContractMatcher _matcher;

    public SupplyCommandDataProvider(InteractiveContractMatcher matcher) => _matcher = matcher;

    private static readonly string[] OrderTypes = { "reg", "promo" };

    private static readonly IReadOnlyList<InteractiveTableContract> Contract = new List<InteractiveTableContract>
    {
        new("supply_orders", "سجل طلبات سلسلة الإمداد، صف واحد لكل طلب", new List<InteractiveColumnContract>
        {
            new("order_date", "تاريخ الطلب"),
            new("order_type", "نوع الطلب: reg (اعتيادي) أو promo (ترويجي)"),
            new("channel", "قناة البيع"),
            new("warehouse", "المستودع المنفِّذ"),
            new("category", "فئة المنتج"),
            new("customer_priority", "أولوية العميل"),
            new("revenue", "إيراد الطلب"),
            new("gross_profit", "مجمل ربح الطلب"),
            new("on_time", "هل وصل الطلب في الوقت المحدد (صحيح/خطأ)"),
            new("supplier", "المورد المرتبط بهذا الطلب", Required: false),
            new("cost_of_goods", "تكلفة البضاعة لهذا الطلب", Required: false),
            new("stockout", "هل تأثر الطلب بنفاد مخزون (صحيح/خطأ)", Required: false),
            new("returned", "هل أُعيد هذا الطلب (صحيح/خطأ)", Required: false),
            new("late_penalty", "غرامة التأخير المالية لهذا الطلب، إن وُجدت", Required: false),
            new("waste_cost", "تكلفة الهدر المخصَّصة لهذا الطلب، إن وُجدت", Required: false),
            new("ship_days", "عدد أيام الشحن الفعلية لهذا الطلب", Required: false),
        }),
    };

    internal record Order(
        DateTime Date, string OrderType, string Channel, string Warehouse, string Category, string Priority,
        string? Supplier, double Revenue, double GrossProfit, double Cogs, bool OnTime, bool Stockout, bool Returned,
        double LatePenalty, double WasteCost, double ShipDays);

    public async Task<InteractiveDataResult> BuildAsync(string projectId, CancellationToken ct)
    {
        var missing = new[] { "طلبات سلسلة الإمداد (supply_orders)" };
        var matched = await _matcher.MatchAsync(projectId, Contract, ct);
        if (matched is null) return InteractiveDataResult.Unlinked(missing);

        var rows = await _matcher.QueryAsync(BuildSelect(matched.Tables["supply_orders"],
            "order_date", "order_type", "channel", "warehouse", "category", "customer_priority", "revenue", "gross_profit",
            "on_time", "supplier", "cost_of_goods", "stockout", "returned", "late_penalty", "waste_cost", "ship_days"), matched.Context, ct);
        if (rows.Count == 0) return InteractiveDataResult.Unlinked(missing);

        var orders = rows
            .Select(r => (Date: ParseDate(r, "order_date"), Type: Str(r, "order_type"), Channel: Str(r, "channel"), Warehouse: Str(r, "warehouse"),
                Category: Str(r, "category"), Priority: Str(r, "customer_priority"), Revenue: Num(r, "revenue"), GrossProfit: Num(r, "gross_profit"),
                OnTime: Bool(r, "on_time"), Supplier: Str(r, "supplier"), Cogs: Num(r, "cost_of_goods"), Stockout: Bool(r, "stockout"),
                Returned: Bool(r, "returned"), LatePenalty: Num(r, "late_penalty"), WasteCost: Num(r, "waste_cost"), ShipDays: Num(r, "ship_days")))
            .Where(r => r.Date is not null && r.Type is "reg" or "promo" && r.Channel is not null && r.Warehouse is not null && r.Category is not null && r.Priority is not null)
            .Select(r => new Order(r.Date!.Value.Date, r.Type!, r.Channel!, r.Warehouse!, r.Category!, r.Priority!, r.Supplier, r.Revenue, r.GrossProfit, r.Cogs, r.OnTime, r.Stockout, r.Returned, r.LatePenalty, r.WasteCost, r.ShipDays))
            .ToList();
        if (orders.Count == 0) return InteractiveDataResult.Unlinked(missing);

        var channels = orders.Select(o => o.Channel).Distinct().ToList();
        var warehouses = orders.Select(o => o.Warehouse).Distinct().ToList();
        var categories = orders.Select(o => o.Category).Distinct().ToList();
        var priorities = orders.Select(o => o.Priority).Distinct().ToList();
        var suppliers = orders.Where(o => o.Supplier is not null).Select(o => o.Supplier!).Distinct().ToList();

        var maxYear = orders.Max(o => o.Date.Year);
        var years = new[] { maxYear - 1, maxYear };
        var monthStart = new DateTime(years[0], 1, 1);

        var kpis = new Dictionary<string, object>();
        var trend = new Dictionary<string, object>();
        var otif = new Dictionary<string, object>();
        var lateByCategory = new Dictionary<string, object>();
        var channelRevenue = new Dictionary<string, object>();
        var channelGrossProfit = new Dictionary<string, object>();
        var otifByPriority = new Dictionary<string, object>();
        var otifByWarehouse = new Dictionary<string, object>();
        var revenueAtRiskCh = new Dictionary<string, object>();
        var revenueAtRiskWh = new Dictionary<string, object>();
        var revenueAtRiskCat = new Dictionary<string, object>();
        var revenueAtRiskSup = new Dictionary<string, object>();
        var lossMatrixPct = new Dictionary<string, object>();
        var efficiencyWaterfallVals = new Dictionary<string, object>();
        var marginByDeliveryPct = new Dictionary<string, object>();
        var profitRiskByChannel = new Dictionary<string, object>();

        foreach (var ord in OrderTypes)
        {
            var all = orders.Where(o => o.OrderType == ord).ToList();
            var thisYear = all.Where(o => o.Date.Year == maxYear).ToList();
            var prevYear = all.Where(o => o.Date.Year == maxYear - 1).ToList();
            double OtifPct(List<Order> set) => set.Count > 0 ? set.Count(o => o.OnTime) / (double)set.Count * 100 : 0;

            var revenue = thisYear.Sum(o => o.Revenue);
            var revenuePrevYear = prevYear.Sum(o => o.Revenue);
            var grossProfit = thisYear.Sum(o => o.GrossProfit);
            var otifPct = OtifPct(thisYear);
            var stockoutCount = thisYear.Count(o => o.Stockout);

            kpis[ord] = new
            {
                revenue,
                revenuePrevYear,
                revenueDeltaPct = PctChange(revenue, revenuePrevYear),
                grossProfit,
                grossProfitMarginPct = revenue > 0 ? grossProfit / revenue * 100 : 0,
                grossProfitDeltaPct = PctChange(grossProfit, prevYear.Sum(o => o.GrossProfit)),
                otifPct,
                lateOrders = thisYear.Count(o => !o.OnTime),
                avgShipDays = thisYear.Count > 0 ? thisYear.Average(o => o.ShipDays) : 0,
                stockoutPct = thisYear.Count > 0 ? stockoutCount / (double)thisYear.Count * 100 : 0,
                stockoutOrdersAffected = stockoutCount,
            };

            trend[ord] = new
            {
                rev = new { months = MonthlySum(all, monthStart, o => o.Revenue) },
                gp = new { months = MonthlySum(all, monthStart, o => o.GrossProfit) },
                otif = new { months = MonthlyOtifPct(all, monthStart) },
                late = new { months = MonthlyCount(all, monthStart, o => !o.OnTime) },
                so = new { months = MonthlyRatePct(all, monthStart, o => o.Stockout) },
            };

            otif[ord] = new { onTime = thisYear.Count(o => o.OnTime), late = thisYear.Count(o => !o.OnTime) };
            lateByCategory[ord] = categories.Select(c => thisYear.Count(o => o.Category == c && !o.OnTime)).ToList();
            channelRevenue[ord] = channels.Select(c => thisYear.Where(o => o.Channel == c).Sum(o => o.Revenue)).ToList();
            channelGrossProfit[ord] = channels.Select(c => thisYear.Where(o => o.Channel == c).Sum(o => o.GrossProfit)).ToList();
            otifByPriority[ord] = priorities.Select(p => OtifPct(thisYear.Where(o => o.Priority == p).ToList())).ToList();
            otifByWarehouse[ord] = warehouses.Select(w => OtifPct(thisYear.Where(o => o.Warehouse == w).ToList())).ToList();

            double AtRiskRevenue(IEnumerable<Order> set) => set.Where(o => !o.OnTime || o.Stockout).Sum(o => o.Revenue);
            revenueAtRiskCh[ord] = channels.Select(c => AtRiskRevenue(thisYear.Where(o => o.Channel == c))).ToList();
            revenueAtRiskWh[ord] = warehouses.Select(w => AtRiskRevenue(thisYear.Where(o => o.Warehouse == w))).ToList();
            revenueAtRiskCat[ord] = categories.Select(c => AtRiskRevenue(thisYear.Where(o => o.Category == c))).ToList();
            revenueAtRiskSup[ord] = suppliers.Select(s => AtRiskRevenue(thisYear.Where(o => o.Supplier == s))).ToList();

            lossMatrixPct[ord] = channels.Select(c => categories.Select(cat =>
            {
                var bucket = thisYear.Where(o => o.Channel == c && o.Category == cat).ToList();
                var total = bucket.Sum(o => o.Revenue);
                var lost = bucket.Where(o => !o.OnTime || o.Stockout || o.Returned).Sum(o => o.Revenue);
                return total > 0 ? Math.Round(lost / total * 100, 1) : 0;
            }).ToList()).ToList();

            var cogs = thisYear.Sum(o => o.Cogs);
            var adjustment = grossProfit - (revenue - cogs);
            var stockoutLoss = -thisYear.Where(o => o.Stockout).Sum(o => o.Revenue);
            var returnsLoss = -thisYear.Where(o => o.Returned).Sum(o => o.Revenue);
            var latePenalty = -thisYear.Sum(o => o.LatePenalty);
            var wasteCost = -thisYear.Sum(o => o.WasteCost);
            var net = revenue + adjustment + stockoutLoss + returnsLoss + latePenalty + wasteCost - cogs;
            efficiencyWaterfallVals[ord] = new[] { revenue, adjustment, stockoutLoss, returnsLoss, latePenalty, wasteCost, -cogs, net };

            var onTimeSet = thisYear.Where(o => o.OnTime).ToList();
            var failedSet = thisYear.Where(o => !o.OnTime).ToList();
            double Margin(List<Order> set) => set.Sum(o => o.Revenue) > 0 ? set.Sum(o => o.GrossProfit) / set.Sum(o => o.Revenue) * 100 : 0;
            marginByDeliveryPct[ord] = new { onTime = Margin(onTimeSet), failed = Margin(failedSet) };

            profitRiskByChannel[ord] = channels.Select(c => -thisYear.Where(o => o.Channel == c && !o.OnTime).Sum(o => o.GrossProfit)).ToList();
        }

        return InteractiveDataResult.Ready(new
        {
            years = years.Select(y => y.ToString()).ToList(),
            channels,
            warehouses,
            categories,
            suppliers,
            customerPriorities = priorities,
            kpis,
            trend,
            otif,
            lateByCategory,
            channelRevenue,
            channelGrossProfit,
            otifByPriority,
            otifByWarehouse,
            revenueAtRisk = new { ch = revenueAtRiskCh, wh = revenueAtRiskWh, cat = revenueAtRiskCat, sup = revenueAtRiskSup },
            lossMatrixPct,
            efficiencyWaterfallVals,
            marginByDeliveryPct,
            profitRiskByChannel,
        });
    }

    private static double PctChange(double now, double before) => before != 0 ? Math.Round((now - before) / before * 100, 1) : 0;

    internal static List<double> MonthlySum(List<Order> orders, DateTime monthStart, Func<Order, double> selector)
    {
        var result = new List<double>();
        for (var i = 0; i < 24; i++)
        {
            var month = monthStart.AddMonths(i);
            result.Add(orders.Where(o => o.Date.Year == month.Year && o.Date.Month == month.Month).Sum(selector));
        }
        return result;
    }

    private static List<double> MonthlyCount(List<Order> orders, DateTime monthStart, Func<Order, bool> predicate)
    {
        var result = new List<double>();
        for (var i = 0; i < 24; i++)
        {
            var month = monthStart.AddMonths(i);
            result.Add(orders.Count(o => o.Date.Year == month.Year && o.Date.Month == month.Month && predicate(o)));
        }
        return result;
    }

    internal static List<double> MonthlyOtifPct(List<Order> orders, DateTime monthStart)
    {
        var result = new List<double>();
        for (var i = 0; i < 24; i++)
        {
            var month = monthStart.AddMonths(i);
            var bucket = orders.Where(o => o.Date.Year == month.Year && o.Date.Month == month.Month).ToList();
            result.Add(bucket.Count > 0 ? Math.Round(bucket.Count(o => o.OnTime) / (double)bucket.Count * 100, 1) : 0);
        }
        return result;
    }

    private static List<double> MonthlyRatePct(List<Order> orders, DateTime monthStart, Func<Order, bool> predicate)
    {
        var result = new List<double>();
        for (var i = 0; i < 24; i++)
        {
            var month = monthStart.AddMonths(i);
            var bucket = orders.Where(o => o.Date.Year == month.Year && o.Date.Month == month.Month).ToList();
            result.Add(bucket.Count > 0 ? Math.Round(bucket.Count(predicate) / (double)bucket.Count * 100, 2) : 0);
        }
        return result;
    }
}
