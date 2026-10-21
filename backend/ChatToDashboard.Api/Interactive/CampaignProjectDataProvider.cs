using static ChatToDashboard.Api.Interactive.InteractiveRowHelpers;

namespace ChatToDashboard.Api.Interactive;

/// <summary>
/// §4.22's real data provider for campaign-project ("تفاصيل مشروع حملة") — matches the
/// project's real tables and builds the shape campaign-project.js's JEEM_CAMPAIGN_MOUNT
/// expects. The 7-day "توقع" (forecast) tail on the spend/conversion charts was pure random
/// noise in the prototype (the same DK.rng walk just kept running past "today"); a real
/// provider can't know the future, so it replaces that with an actual (if simple, clearly
/// labelled "forecast") method: each series continues flat at the trailing-7-day average of
/// its own real daily values — a transparent, honest projection, not a fabricated one.
/// budget.total/roas/convRatePct are genuinely external ad-platform metrics (allocated budget,
/// return-on-spend, click-through conversion rate) that aren't derivable from daily spend/
/// conversions alone, so they come from one small campaign_info row rather than being guessed.
/// activity-log "color" has no real meaning in the prototype either (arbitrary indices for
/// visual variety) — kept that way here too, via a stable hash of each entry's own text rather
/// than a fabricated semantic.
/// </summary>
public class CampaignProjectDataProvider : IInteractiveDataProvider
{
    public string TemplateId => "campaign-project";

    private readonly InteractiveContractMatcher _matcher;

    public CampaignProjectDataProvider(InteractiveContractMatcher matcher) => _matcher = matcher;

    private static readonly int[] Windows = { 15, 30 };
    private const int ForecastDays = 7;
    private const int ForecastAvgWindow = 7;

    private static readonly IReadOnlyList<InteractiveTableContract> Contract = new List<InteractiveTableContract>
    {
        new("campaign_info", "بيانات الحملة العامة (صف واحد)", new List<InteractiveColumnContract>
        {
            new("name", "اسم الحملة"),
            new("archived", "هل الحملة مؤرشفة (صحيح/خطأ)", Required: false),
            new("due_date", "تاريخ استحقاق الحملة"),
            new("roas", "العائد على الإنفاق الإعلاني (ROAS)"),
            new("budget_total", "الميزانية الكلية المخصَّصة للحملة"),
            new("conv_rate_pct", "نسبة التحويل المئوية للحملة"),
            new("member_names", "أسماء الأعضاء المكلَّفين بالحملة، مفصولة بفواصل", Required: false),
        }),
        new("campaign_daily_performance", "الإنفاق والتحويلات اليومية الفعلية للحملة", new List<InteractiveColumnContract>
        {
            new("date", "التاريخ"),
            new("spend", "الإنفاق الفعلي في هذا اليوم"),
            new("conversions", "عدد التحويلات الفعلية في هذا اليوم"),
        }),
        new("campaign_tasks", "مهام الحملة وحالتها", new List<InteractiveColumnContract>
        {
            new("title", "عنوان المهمة"),
            new("status", "حالة المهمة: done أو prog أو on أو pend"),
            new("assignee", "اسم المسؤول عن المهمة", Required: false),
            new("due_date", "تاريخ استحقاق المهمة", Required: false),
        }, Required: false),
        new("campaign_files", "ملفات ومرفقات الحملة", new List<InteractiveColumnContract>
        {
            new("name", "اسم الملف"),
            new("size_bytes", "حجم الملف بالبايت", Required: false),
            new("uploaded_by", "اسم من رفع الملف", Required: false),
            new("uploaded_at", "تاريخ الرفع", Required: false),
        }, Required: false),
        new("campaign_activity_log", "سجل نشاط الحملة", new List<InteractiveColumnContract>
        {
            new("occurred_at", "وقت الحدث"),
            new("text", "وصف الحدث"),
            new("icon", "اسم أيقونة الحدث (اختياري)", Required: false),
        }, Required: false),
    };

    private static readonly string[] ArabicMonths =
    {
        "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
        "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر",
    };
    private static readonly HashSet<string> KnownIcons = new(StringComparer.Ordinal)
    {
        "bell", "briefcase", "calendar", "cart", "chart", "chat", "check", "chev-down", "chev-left", "clock",
        "dashboard", "database", "down", "download", "drag", "edit", "expand", "file", "filter", "grid", "hash",
        "headset", "history", "image", "info", "layers", "link", "list", "logout", "menu", "mic", "minus", "more",
        "pause", "percent", "pin", "play", "plus", "publish", "search", "send", "server", "settings", "share",
        "sheet", "shield", "sidebar", "spark", "star", "target", "trash", "up", "users", "wallet", "x",
    };
    private static readonly HashSet<string> ValidTaskStatuses = new(StringComparer.Ordinal) { "done", "prog", "on", "pend" };

    internal record PerformanceSeries(List<string> Labels, List<double?> SpendActual, List<double?> SpendForecast, List<double?> ConvActual, List<double?> ConvForecast);

    public async Task<InteractiveDataResult> BuildAsync(string projectId, CancellationToken ct)
    {
        var missing = new[] { "بيانات الحملة (campaign_info)", "الأداء اليومي (campaign_daily_performance)" };
        var matched = await _matcher.MatchAsync(projectId, Contract, ct);
        if (matched is null) return InteractiveDataResult.Unlinked(missing);

        var infoTable = matched.Tables["campaign_info"];
        var perfTable = matched.Tables["campaign_daily_performance"];
        var infoRows = await _matcher.QueryAsync(BuildSelect(infoTable, "name", "archived", "due_date", "roas", "budget_total", "conv_rate_pct", "member_names"), matched.Context, ct);
        var perfRows = await _matcher.QueryAsync(BuildSelect(perfTable, "date", "spend", "conversions"), matched.Context, ct);
        if (infoRows.Count == 0 || perfRows.Count == 0) return InteractiveDataResult.Unlinked(missing);

        var info = infoRows[0];
        var dueDate = ParseDate(info, "due_date");
        var today = DateTime.Now.Date;
        var daysRemaining = dueDate is null ? 0 : Math.Max(0, (dueDate.Value.Date - today).Days);

        var daily = perfRows
            .Select(r => (Date: ParseDate(r, "date"), Spend: Num(r, "spend"), Conversions: Num(r, "conversions")))
            .Where(r => r.Date is not null)
            .Select(r => (Date: r.Date!.Value.Date, r.Spend, r.Conversions))
            .OrderBy(r => r.Date)
            .ToList();
        if (daily.Count == 0) return InteractiveDataResult.Unlinked(missing);

        var performance = new Dictionary<string, object>();
        var budgetByWindow = new Dictionary<string, object>();
        foreach (var window in Windows)
        {
            var slice = daily.Count > window ? daily.Skip(daily.Count - window).ToList() : daily;
            var series = BuildPerformanceSeries(slice);
            performance[window.ToString()] = new
            {
                labels = series.Labels, spendActual = series.SpendActual, spendForecast = series.SpendForecast,
                convActual = series.ConvActual, convForecast = series.ConvForecast,
            };
            budgetByWindow[window.ToString()] = new { spent = slice.Sum(d => d.Spend), conversions = slice.Sum(d => d.Conversions) };
        }

        List<object>? members = null;
        var memberNames = Str(info, "member_names");
        if (!string.IsNullOrWhiteSpace(memberNames))
            members = memberNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(n => (object)new { name = n }).ToList();

        var budget = new Dictionary<string, object>(budgetByWindow)
        {
            ["total"] = Num(info, "budget_total"),
            ["convRatePct"] = Num(info, "conv_rate_pct"),
        };

        List<object> tasks = new();
        if (matched.Tables.TryGetValue("campaign_tasks", out var tasksTable))
        {
            var rows = await _matcher.QueryAsync(BuildSelect(tasksTable, "title", "status", "assignee", "due_date"), matched.Context, ct);
            tasks = rows.Where(r => Str(r, "title") is not null).Select(r => (object)new
            {
                title = Str(r, "title"),
                status = ValidTaskStatuses.Contains(Str(r, "status") ?? "") ? Str(r, "status") : "pend",
                assignee = Str(r, "assignee") ?? "",
                due = DateStr2(r, "due_date"),
            }).ToList();
        }

        List<object> files = new();
        if (matched.Tables.TryGetValue("campaign_files", out var filesTable))
        {
            var rows = await _matcher.QueryAsync(BuildSelect(filesTable, "name", "size_bytes", "uploaded_by", "uploaded_at"), matched.Context, ct);
            files = rows.Where(r => Str(r, "name") is not null).Select(r => (object)new
            {
                name = Str(r, "name"),
                sizeLabel = FormatSize(Num(r, "size_bytes")),
                by = Str(r, "uploaded_by") ?? "",
                date = DateStr2(r, "uploaded_at"),
            }).ToList();
        }

        List<object> feed = new();
        if (matched.Tables.TryGetValue("campaign_activity_log", out var logTable))
        {
            var rows = await _matcher.QueryAsync(BuildSelect(logTable, "occurred_at", "text", "icon"), matched.Context, ct);
            feed = rows
                .Select(r => (At: ParseDate(r, "occurred_at"), Text: Str(r, "text"), Icon: Str(r, "icon")))
                .Where(r => r.At is not null && r.Text is not null)
                .OrderByDescending(r => r.At)
                .Take(6)
                .Select(r => (object)new
                {
                    icon = r.Icon is not null && KnownIcons.Contains(r.Icon) ? r.Icon : "info",
                    text = r.Text,
                    when = RelativeTime(r.At!.Value),
                    color = Math.Abs(r.Text!.GetHashCode()) % 6,
                })
                .ToList();
        }

        return InteractiveDataResult.Ready(new
        {
            name = Str(info, "name") ?? "الحملة",
            archived = Bool(info, "archived"),
            dueDate = dueDate?.ToString("yyyy-MM-dd"),
            roas = Num(info, "roas"),
            ranges = Windows,
            budget,
            members,
            daysRemaining,
            performance,
            feed,
            tasks,
            files,
        });
    }

    /// <summary>The window's real actual values, plus a 7-day forecast tail that continues
    /// flat at the trailing-average of the real values — a transparent projection method, not
    /// a guess dressed up as data. The forecast series' first point duplicates the last actual
    /// point so the dashed line visually connects to the solid one, matching the engine's own
    /// expectation (see campaign-project.js's `cut()`/`band` handling).</summary>
    internal static PerformanceSeries BuildPerformanceSeries(List<(DateTime Date, double Spend, double Conversions)> actual)
    {
        var n = actual.Count;
        var labels = new List<string>();
        foreach (var d in actual) labels.Add($"{d.Date.Day} {ArabicMonths[d.Date.Month - 1]}");
        var lastDate = actual[^1].Date;
        for (var i = 1; i <= ForecastDays; i++) { var d = lastDate.AddDays(i); labels.Add($"{d.Day} {ArabicMonths[d.Month - 1]}"); }

        var tail = actual.Count > ForecastAvgWindow ? actual.Skip(actual.Count - ForecastAvgWindow).ToList() : actual;
        var avgSpend = tail.Average(d => d.Spend);
        var avgConv = tail.Average(d => d.Conversions);

        var spendActual = new List<double?>();
        var spendForecast = new List<double?>();
        var convActual = new List<double?>();
        var convForecast = new List<double?>();
        for (var i = 0; i < n; i++) { spendActual.Add(actual[i].Spend); convActual.Add(actual[i].Conversions); spendForecast.Add(null); convForecast.Add(null); }
        spendForecast[n - 1] = actual[n - 1].Spend; // connect the dashed line to the last solid point
        convForecast[n - 1] = actual[n - 1].Conversions;
        for (var i = 0; i < ForecastDays; i++)
        {
            spendActual.Add(null); convActual.Add(null);
            spendForecast.Add(Math.Round(avgSpend));
            convForecast.Add(Math.Round(avgConv, 1));
        }

        return new PerformanceSeries(labels, spendActual, spendForecast, convActual, convForecast);
    }

    private static string? DateStr2(Dictionary<string, System.Text.Json.JsonElement> row, string col)
    {
        var dt = ParseDate(row, col);
        return dt is null ? "—" : $"{dt.Value.Day:D2} {ArabicMonths[dt.Value.Month - 1]}";
    }

    private static string FormatSize(double bytes) =>
        bytes > 1e6 ? $"{bytes / 1e6:F1} م.ب" : $"{Math.Round(bytes / 1e3)} ك.ب";

    private static string RelativeTime(DateTime at)
    {
        var span = DateTime.Now - at;
        if (span.TotalMinutes < 1) return "الآن";
        if (span.TotalMinutes < 60) return $"قبل {(int)span.TotalMinutes} دقيقة";
        if (span.TotalHours < 2) return "قبل ساعة";
        if (span.TotalHours < 24) return $"قبل {(int)span.TotalHours} ساعات";
        if (span.TotalDays < 2) return "قبل يوم";
        if (span.TotalDays < 7) return $"قبل {(int)span.TotalDays} أيام";
        return at.ToString("d MMMM", new System.Globalization.CultureInfo("ar"));
    }
}
