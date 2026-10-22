using static ChatToDashboard.Api.Interactive.InteractiveRowHelpers;

namespace ChatToDashboard.Api.Interactive;

/// <summary>
/// §4.22's real data provider for construction-crm ("إدارة أعمال المقاولات"). Request
/// categories are whatever categories the project's own service_requests rows actually use
/// (no fixed Saudi-specific list in the engine) — icon/color are assigned by rotation since
/// they are presentation only, never inferable from a category's real name. "إجمالي النشاط%"
/// (the big headline number) is the real period-over-period growth in request volume (this
/// 7/30-day window vs the one before it), not an arbitrary number. Quote funnel ordering uses
/// an optional stage_order column when the source has one, falling back to first-appearance
/// order in the query (still a real order, just whatever the source naturally returns, same
/// honesty standard as every other "no explicit order given" fallback in this batch).
/// targetPctOfGoal (the weekly-transfer progress chip) is computed from the real weekly
/// transfers against the real daily target rather than requiring a separately-maintained
/// percentage that could drift out of sync with the numbers right next to it.
/// </summary>
public class ConstructionCrmDataProvider : IInteractiveDataProvider
{
    public string TemplateId => "construction-crm";

    private readonly InteractiveContractMatcher _matcher;

    public ConstructionCrmDataProvider(InteractiveContractMatcher matcher) => _matcher = matcher;

    private static readonly int[] Windows = { 7, 30 };
    private static readonly string[] PaletteColors = { "#127DBD", "#D9822B", "#00A89F", "#6E62B0", "#7CB342", "#C2569B" };
    private static readonly string[] CategoryIcons = { "briefcase", "file", "edit", "shield", "clock", "database" };
    private static readonly HashSet<string> ValidAppointmentStatuses = new(StringComparer.Ordinal) { "ok", "act", "done" };

    private static readonly IReadOnlyList<InteractiveTableContract> Contract = new List<InteractiveTableContract>
    {
        new("service_requests", "طلبات الخدمة الواردة، كل طلب بفئته وتاريخه", new List<InteractiveColumnContract>
        {
            new("category", "فئة الطلب"),
            new("created_at", "تاريخ ووقت إنشاء الطلب"),
        }),
        new("quotes", "عروض الأسعار المُنشأة، كل عرض بمرحلته وقيمته", new List<InteractiveColumnContract>
        {
            new("stage", "اسم مرحلة العرض"),
            new("created_at", "تاريخ إنشاء العرض"),
            new("value", "قيمة العرض"),
            new("stage_order", "ترتيب المرحلة في مسار العروض", Required: false),
        }),
        new("jobs", "لقطة الأعمال الحالية حسب الفئة (صف لكل فئة)", new List<InteractiveColumnContract>
        {
            new("name", "اسم فئة العمل"),
            new("count", "عدد الأعمال في هذه الفئة"),
            new("value", "القيمة الإجمالية لهذه الفئة"),
        }),
        new("appointments", "مواعيد العملاء القادمة والجارية والمكتملة", new List<InteractiveColumnContract>
        {
            new("client_name", "اسم العميل"),
            new("scheduled_at", "تاريخ ووقت الموعد"),
            new("location", "موقع الموعد"),
            new("status", "حالة الموعد: ok (مؤكد) أو act (جارٍ الآن) أو done (مكتمل)"),
            new("value", "القيمة المرتبطة بهذا الموعد"),
            new("code", "رمز مرجعي للموعد", Required: false),
        }),
        new("payout_accounts", "حسابات تحويل الأرصدة البنكية", new List<InteractiveColumnContract>
        {
            new("name", "اسم الحساب/البنك"),
            new("in_transit", "المبلغ في الطريق إلى البنك"),
            new("instant_available", "المبلغ المتاح للتحويل الفوري"),
            new("target_daily", "المستهدف اليومي للتحويلات"),
        }),
        new("payout_weekly_transfers", "التحويلات اليومية لآخر أسبوع لكل حساب", new List<InteractiveColumnContract>
        {
            new("account_name", "اسم الحساب (يطابق اسمه في جدول الحسابات)"),
            new("transfer_date", "تاريخ التحويل"),
            new("amount", "قيمة التحويل في هذا اليوم"),
        }, Required: false),
    };

    public async Task<InteractiveDataResult> BuildAsync(string projectId, CancellationToken ct)
    {
        var missing = new[]
        {
            "طلبات الخدمة (service_requests)", "عروض الأسعار (quotes)", "الأعمال (jobs)",
            "المواعيد (appointments)", "حسابات التحويل (payout_accounts)",
        };
        var matched = await _matcher.MatchAsync(projectId, Contract, ct);
        if (matched is null) return InteractiveDataResult.Unlinked(missing);

        var reqRows = await _matcher.QueryAsync(BuildSelect(matched.Tables["service_requests"], "category", "created_at"), matched.Context, ct);
        var quoteRows = await _matcher.QueryAsync(BuildSelect(matched.Tables["quotes"], "stage", "created_at", "value", "stage_order"), matched.Context, ct);
        var jobRows = await _matcher.QueryAsync(BuildSelect(matched.Tables["jobs"], "name", "count", "value"), matched.Context, ct);
        var apptRows = await _matcher.QueryAsync(BuildSelect(matched.Tables["appointments"], "client_name", "scheduled_at", "location", "status", "value", "code"), matched.Context, ct);
        var acctRows = await _matcher.QueryAsync(BuildSelect(matched.Tables["payout_accounts"], "name", "in_transit", "instant_available", "target_daily"), matched.Context, ct);
        if (reqRows.Count == 0 || quoteRows.Count == 0 || jobRows.Count == 0 || apptRows.Count == 0 || acctRows.Count == 0)
            return InteractiveDataResult.Unlinked(missing);

        var requests = reqRows
            .Select(r => (Category: Str(r, "category"), At: ParseDate(r, "created_at")))
            .Where(r => r.Category is not null && r.At is not null)
            .Select(r => (Category: r.Category!, At: r.At!.Value))
            .ToList();
        if (requests.Count == 0) return InteractiveDataResult.Unlinked(missing);
        var now = requests.Max(r => r.At);
        var requestsOut = BuildRequests(requests, now);

        var quotes = quoteRows
            .Select(r => (Stage: Str(r, "stage"), At: ParseDate(r, "created_at"), Value: Num(r, "value"), Order: Num(r, "stage_order")))
            .Where(r => r.Stage is not null && r.At is not null)
            .Select(r => (Stage: r.Stage!, At: r.At!.Value, r.Value, r.Order))
            .ToList();
        if (quotes.Count == 0) return InteractiveDataResult.Unlinked(missing);
        var quotesOut = BuildQuotes(quotes, now);

        var colorIdx = 0;
        var jobs = jobRows.Select(r => new
        {
            name = Str(r, "name") ?? "",
            count = (int)Num(r, "count"),
            valueK = Num(r, "value") / 1000,
            color = PaletteColors[colorIdx++ % PaletteColors.Length],
        }).ToList();

        var apptParsed = apptRows
            .Select(r => (Name: Str(r, "client_name"), At: ParseDate(r, "scheduled_at"), Location: Str(r, "location"), Status: Str(r, "status"), Value: Num(r, "value"), Code: Str(r, "code")))
            .Where(r => r.Name is not null && r.At is not null)
            .Select(r => (r.Name, At: r.At!.Value, r.Location, r.Status, r.Value, r.Code))
            .ToList();
        if (apptParsed.Count == 0) return InteractiveDataResult.Unlinked(missing);
        var appointments = apptParsed.Select(r => (object)new
        {
            name = r.Name,
            code = r.Code ?? "",
            date = r.At.ToString("yyyy-MM-dd"),
            time = FormatTime(r.At),
            location = r.Location ?? "",
            status = ValidAppointmentStatuses.Contains(r.Status ?? "") ? r.Status : "ok",
            valueK = r.Value / 1000,
        }).ToList();

        var weeklyByAccount = new Dictionary<string, (List<string> Labels, List<double> Vals, double Total)>(StringComparer.Ordinal);
        if (matched.Tables.TryGetValue("payout_weekly_transfers", out var weeklyTable))
        {
            var rows = await _matcher.QueryAsync(BuildSelect(weeklyTable, "account_name", "transfer_date", "amount"), matched.Context, ct);
            var parsed = rows
                .Select(r => (Account: Str(r, "account_name"), At: ParseDate(r, "transfer_date"), Amount: Num(r, "amount")))
                .Where(r => r.Account is not null && r.At is not null)
                .Select(r => (Account: r.Account!, At: r.At!.Value.Date, r.Amount))
                .ToList();
            foreach (var g in parsed.GroupBy(r => r.Account))
            {
                var days = g.OrderBy(r => r.At).Select(r => r.At).Distinct().ToList();
                var last7 = days.Count > 7 ? days.Skip(days.Count - 7).ToList() : days;
                var labels = last7.Select(d => ArabicWeekdaysShort[(int)d.DayOfWeek]).ToList();
                var vals = last7.Select(d => g.Where(r => r.At == d).Sum(r => r.Amount)).ToList();
                weeklyByAccount[g.Key] = (labels, vals, vals.Sum());
            }
        }

        var payoutAccounts = acctRows.Select(r =>
        {
            var name = Str(r, "name") ?? "";
            var targetDaily = Num(r, "target_daily");
            var weekly = weeklyByAccount.TryGetValue(name, out var w) ? w : (new List<string>(), new List<double>(), 0);
            var targetPctOfGoal = targetDaily > 0 && weekly.Item2.Count > 0 ? weekly.Item3 / (targetDaily * weekly.Item2.Count) * 100 : 0;
            return new
            {
                name,
                inTransit = Num(r, "in_transit"),
                instantAvailable = Num(r, "instant_available"),
                targetDaily,
                targetPctOfGoal,
                weeklyTransfers = new { labels = weekly.Item1, vals = weekly.Item2 },
            };
        }).ToList();

        var lastUpdatedAt = new[] { requests.Max(r => r.At), quotes.Max(q => q.At), apptParsed.Max(a => a.At) }.Max();

        return InteractiveDataResult.Ready(new
        {
            payoutAccounts = payoutAccounts,
            lastUpdatedAt = lastUpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
            requests = requestsOut,
            quotes = quotesOut,
            jobs,
            appointments,
        });
    }

    private static readonly string[] ArabicWeekdaysShort = { "أحد", "اثن", "ثلا", "أرب", "خمي", "جمع", "سبت" };

    private static string FormatTime(DateTime at)
    {
        var hour12 = at.Hour % 12 == 0 ? 12 : at.Hour % 12;
        return $"{hour12}:{at.Minute:D2} {(at.Hour < 12 ? "ص" : "م")}";
    }

    private static object BuildRequests(List<(string Category, DateTime At)> requests, DateTime now)
    {
        var categories = requests.Select(r => r.Category).Distinct().ToList();
        var totalActivityPct = new Dictionary<string, object>();
        var colorIdx = 0;
        var iconIdx = 0;
        var categoriesOut = categories.Select(cat => new
        {
            name = cat,
            icon = CategoryIcons[iconIdx++ % CategoryIcons.Length],
            color = PaletteColors[colorIdx++ % PaletteColors.Length],
            pct7 = PctShare(requests, cat, now, 7),
            pct30 = PctShare(requests, cat, now, 30),
            count7 = requests.Count(r => r.Category == cat && r.At > now.AddDays(-7)),
            count30 = requests.Count(r => r.Category == cat && r.At > now.AddDays(-30)),
        }).ToList();

        foreach (var window in Windows)
        {
            var thisCount = requests.Count(r => r.At > now.AddDays(-window));
            var prevCount = requests.Count(r => r.At > now.AddDays(-window * 2) && r.At <= now.AddDays(-window));
            totalActivityPct[window.ToString()] = prevCount > 0 ? Math.Round((thisCount - prevCount) / (double)prevCount * 100) : (thisCount > 0 ? 100 : 0);
        }

        // "pct" (the segmented-bar share) uses the 7-day window's share, matching the engine's
        // own default S.per === 7 starting state; the 30-day counts are still exact (count30).
        var finalCategories = categoriesOut.Select(c => new
        {
            c.name, c.icon, c.color, pct = c.pct7, c.count7, c.count30,
        }).ToList();

        return new { totalActivityPct, categories = finalCategories };
    }

    private static int PctShare(List<(string Category, DateTime At)> requests, string category, DateTime now, int window)
    {
        var inWindow = requests.Where(r => r.At > now.AddDays(-window)).ToList();
        if (inWindow.Count == 0) return 0;
        return (int)Math.Round(inWindow.Count(r => r.Category == category) / (double)inWindow.Count * 100);
    }

    private static object BuildQuotes(List<(string Stage, DateTime At, double Value, double Order)> quotes, DateTime now)
    {
        var out7 = BuildQuoteWindow(quotes, now, 7);
        var out30 = BuildQuoteWindow(quotes, now, 30);
        return new Dictionary<string, object> { ["7"] = out7, ["30"] = out30 };
    }

    private static List<object> BuildQuoteWindow(List<(string Stage, DateTime At, double Value, double Order)> quotes, DateTime now, int window)
    {
        var inWindow = quotes.Where(q => q.At > now.AddDays(-window)).ToList();
        var stages = inWindow.GroupBy(q => q.Stage)
            .Select(g => (Name: g.Key, Order: g.Max(q => q.Order), FirstSeen: g.Min(q => q.At), Count: g.Count(), ValueK: g.Sum(q => q.Value) / 1000))
            .OrderBy(s => s.Order > 0 ? s.Order : int.MaxValue).ThenBy(s => s.FirstSeen)
            .ToList();
        var colorIdx = 0;
        return stages.Select(s => (object)new { name = s.Name, color = PaletteColors[colorIdx++ % PaletteColors.Length], count = s.Count, valueK = s.ValueK }).ToList();
    }
}
