using static ChatToDashboard.Api.Interactive.InteractiveRowHelpers;

namespace ChatToDashboard.Api.Interactive;

/// <summary>
/// §4.22's real data provider for crm-leads ("العملاء المحتملون والطلبات الواردة"). Lead
/// sources (the bubble pack) are derived by grouping real requests by their own source field —
/// not a separate table a project would maintain redundantly. The four pipeline stages are
/// fixed engine vocabulary (see crm-leads.js's own doc comment); a request's real stage is
/// matched against that fixed list by name, or read as a 0-3 index directly. The three
/// headline KPIs (average deal value, cancellation rate, conversion rate) can't be derived
/// from the pipeline rows alone — there is no "lost" stage to compute a cancellation rate
/// from, and a meaningful trend needs real historical snapshots, not an average over today's
/// open pipeline — so they come from a dedicated reporting table (lead_kpi_history) a real CRM
/// analytics layer would already maintain, pre-formatted the same way the prototype's own demo
/// values are (a literal "ر.س142.56" string, not a number the frontend formats itself).
/// Interaction-hour activity is optional: a project with no interaction log still gets the
/// rest of the dashboard, with the heatmap showing the engine's own "no data yet" state.
/// </summary>
public class CrmLeadsDataProvider : IInteractiveDataProvider
{
    public string TemplateId => "crm-leads";

    private readonly InteractiveContractMatcher _matcher;

    public CrmLeadsDataProvider(InteractiveContractMatcher matcher) => _matcher = matcher;

    private static readonly string[] Stages = { "تفاوض", "عرض مُرسل", "قيد المراجعة", "مكسوبة" };
    private static readonly HashSet<string> ValidTags = new(StringComparer.Ordinal) { "new", "pri", "ret" };
    private static readonly string[] PaletteColors = { "#DFC6AE", "#C9D6EA", "#C4DCC9", "#B9DCD3", "#EBCACA", "#D8C9E8" };
    private static readonly int[] BusinessHours = { 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
    private static readonly string[] HourLabels = { "8ص", "9ص", "10ص", "11ص", "12م", "1م", "2م", "3م", "4م", "5م" };

    private static readonly IReadOnlyList<InteractiveTableContract> Contract = new List<InteractiveTableContract>
    {
        new("lead_requests", "طلبات العملاء المحتملين في مسار المبيعات", new List<InteractiveColumnContract>
        {
            new("company", "اسم الشركة/العميل"),
            new("description", "وصف مختصر للطلب"),
            new("contact_person", "اسم جهة الاتصال"),
            new("contact_role", "المسمى الوظيفي لجهة الاتصال", Required: false),
            new("source", "مصدر هذا العميل المحتمل"),
            new("tag", "تصنيف: new (جديد) أو pri (أولوية) أو ret (عائد)", Required: false),
            new("stage", "مرحلة الطلب: اسم المرحلة أو رقمها (0 إلى 3)"),
            new("next_action_at", "تاريخ الإجراء القادم المرتبط بهذا الطلب"),
            new("attachments_count", "عدد المرفقات", Required: false),
            new("comments_count", "عدد التعليقات", Required: false),
        }),
        new("lead_kpi_history", "لقطات دورية لمؤشرات الأداء (متوسط قيمة الطلب، معدل الإلغاء، معدل التحويل)", new List<InteractiveColumnContract>
        {
            new("period_days", "مدى النافذة الزمينة لهذه اللقطة بالأيام (7 أو 30 أو 90)"),
            new("snapshot_date", "تاريخ اللقطة"),
            new("avg_deal_value", "متوسط قيمة الطلب في هذه اللقطة"),
            new("cancellation_rate_pct", "معدل الإلغاء المئوي في هذه اللقطة"),
            new("conversion_rate_pct", "معدل التحويل المئوي في هذه اللقطة"),
        }),
        new("lead_interactions", "سجل تفاعلات العملاء المحتملين بالطابع الزمني", new List<InteractiveColumnContract>
        {
            new("interaction_at", "وقت التفاعل"),
        }, Required: false),
    };

    public async Task<InteractiveDataResult> BuildAsync(string projectId, CancellationToken ct)
    {
        var missing = new[] { "طلبات العملاء المحتملين (lead_requests)", "مؤشرات الأداء الدورية (lead_kpi_history)" };
        var matched = await _matcher.MatchAsync(projectId, Contract, ct);
        if (matched is null) return InteractiveDataResult.Unlinked(missing);

        var reqRows = await _matcher.QueryAsync(BuildSelect(matched.Tables["lead_requests"], "company", "description", "contact_person", "contact_role", "source", "tag", "stage", "next_action_at", "attachments_count", "comments_count"), matched.Context, ct);
        var kpiRows = await _matcher.QueryAsync(BuildSelect(matched.Tables["lead_kpi_history"], "period_days", "snapshot_date", "avg_deal_value", "cancellation_rate_pct", "conversion_rate_pct"), matched.Context, ct);
        if (reqRows.Count == 0 || kpiRows.Count == 0) return InteractiveDataResult.Unlinked(missing);

        var parsed = reqRows
            .Select(r => (Company: Str(r, "company"), Source: Str(r, "source"), Stage: Str(r, "stage"), Tag: Str(r, "tag"),
                Description: Str(r, "description") ?? "", Person: Str(r, "contact_person") ?? "", Role: Str(r, "contact_role") ?? "",
                Next: DateStr(r, "next_action_at"), Attachments: (int)Num(r, "attachments_count"), Comments: (int)Num(r, "comments_count")))
            .Where(r => r.Company is not null && r.Source is not null)
            .ToList();
        if (parsed.Count == 0) return InteractiveDataResult.Unlinked(missing);

        var sourceNames = parsed.Select(r => r.Source!).Distinct().ToList();
        var indexOfSource = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i < sourceNames.Count; i++) indexOfSource[sourceNames[i]] = i;
        var total = parsed.Count;
        var sources = sourceNames.Select((name, i) =>
        {
            var count = parsed.Count(r => r.Source == name);
            return new { name, pct = (int)Math.Round(count / (double)total * 100), color = PaletteColors[i % PaletteColors.Length], count };
        }).ToList();

        var requests = parsed.Select(r => new
        {
            co = r.Company,
            d = r.Description,
            p = r.Person,
            role = r.Role,
            tag = ValidTags.Contains(r.Tag ?? "") ? r.Tag : "new",
            st = ResolveStage(r.Stage),
            src = indexOfSource[r.Source!],
            dt = r.Next,
            at = r.Attachments,
            cm = r.Comments,
        }).ToList();

        var kpiParsed = kpiRows
            .Select(r => (Period: (int)Num(r, "period_days"), At: ParseDate(r, "snapshot_date"), Value: Num(r, "avg_deal_value"), Cancel: Num(r, "cancellation_rate_pct"), Conv: Num(r, "conversion_rate_pct")))
            .Where(r => r.At is not null && r.Period > 0)
            .Select(r => (r.Period, At: r.At!.Value, r.Value, r.Cancel, r.Conv))
            .ToList();
        if (kpiParsed.Count == 0) return InteractiveDataResult.Unlinked(missing);
        var periods = kpiParsed.Select(r => r.Period).Distinct().OrderBy(p => p).ToList();
        var kpisOut = new Dictionary<string, object>();
        foreach (var period in periods)
        {
            var series = kpiParsed.Where(r => r.Period == period).OrderBy(r => r.At).ToList();
            kpisOut[period.ToString()] = new[]
            {
                BuildKpi("متوسط قيمة الطلب", series.Select(s => s.Value).ToList(), v => $"ر.س{v:F2}", good: 1),
                BuildKpi("معدل الإلغاء", series.Select(s => s.Cancel).ToList(), v => $"{v:F1}%", good: -1),
                BuildKpi("معدل التحويل", series.Select(s => s.Conv).ToList(), v => $"{v:F1}%", good: 1),
            };
        }

        object activityByPeriod = new Dictionary<string, object>();
        if (matched.Tables.TryGetValue("lead_interactions", out var interTable))
        {
            var rows = await _matcher.QueryAsync(BuildSelect(interTable, "interaction_at"), matched.Context, ct);
            var times = rows.Select(r => ParseDate(r, "interaction_at")).Where(t => t is not null).Select(t => t!.Value).ToList();
            if (times.Count > 0) activityByPeriod = BuildActivity(times, periods);
        }

        return InteractiveDataResult.Ready(new { periods, sources, requests, kpis = kpisOut, activityByPeriod, activityHours = HourLabels });
    }

    private static int ResolveStage(string? raw)
    {
        if (int.TryParse(raw, out var idx) && idx is >= 0 and < 4) return idx;
        var found = Array.IndexOf(Stages, raw);
        return found >= 0 ? found : 0;
    }

    private static object BuildKpi(string name, List<double> series, Func<double, string> format, int good)
    {
        if (series.Count == 0) return new { name, value = format(0), deltaPct = 0, good, spark = (List<double>?)null };
        var last = series[^1];
        var first = series[0];
        var deltaPct = first != 0 ? Math.Round((last - first) / Math.Abs(first) * 100, 1) : 0;
        return new { name, value = format(last), deltaPct, good, spark = series.Count >= 2 ? series : null };
    }

    private static object BuildActivity(List<DateTime> times, List<int> periods)
    {
        var latestDay = times.Max().Date;
        var byPeriod = new Dictionary<string, List<List<int>>>();
        foreach (var period in periods)
        {
            var windowStart = latestDay.AddDays(-(period - 1));
            var matrix = BusinessHours.Select(_ => Enumerable.Repeat(0, period).ToList()).ToList();
            foreach (var t in times)
            {
                var day = t.Date;
                if (day < windowStart || day > latestDay) continue;
                var hourIdx = Array.IndexOf(BusinessHours, t.Hour);
                if (hourIdx < 0) continue;
                var dayIdx = (day - windowStart).Days;
                matrix[hourIdx][dayIdx]++;
            }
            byPeriod[period.ToString()] = matrix;
        }
        return byPeriod;
    }
}
