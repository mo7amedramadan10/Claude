using static ChatToDashboard.Api.Interactive.InteractiveRowHelpers;

namespace ChatToDashboard.Api.Interactive;

/// <summary>
/// §4.22's real data provider for crm-analysis ("تحليل مسار المبيعات") — the simplest of the
/// batch in data-contract terms, matching crm-analysis.js's own doc comment: every filter
/// dimension the engine cross-filters on (month/week/industry/city/org size/agent/stage) is
/// derived client-side from data.leads itself, so this provider only needs to hand over one
/// real row per lead. Pipeline stage order comes from an optional pipeline_stages table when
/// the project has one, or falls back to each stage name's first appearance in the leads data
/// (still a real order, not a fabricated one) — and is always returned explicitly rather than
/// leaving it to the engine's own generic 5-stage fallback, which would show the wrong stage
/// names for a project whose real pipeline doesn't match that default.
/// </summary>
public class CrmAnalysisDataProvider : IInteractiveDataProvider
{
    public string TemplateId => "crm-analysis";

    private readonly InteractiveContractMatcher _matcher;

    public CrmAnalysisDataProvider(InteractiveContractMatcher matcher) => _matcher = matcher;

    private static readonly IReadOnlyList<InteractiveTableContract> Contract = new List<InteractiveTableContract>
    {
        new("leads", "العملاء المحتملون في مسار المبيعات، صف واحد لكل عميل محتمل", new List<InteractiveColumnContract>
        {
            new("industry", "قطاع العميل المحتمل", Required: false),
            new("city", "مدينة العميل المحتمل", Required: false),
            new("org_size", "حجم منشأة العميل المحتمل", Required: false),
            new("agent", "مندوب المبيعات المسؤول", Required: false),
            new("created_at", "تاريخ إنشاء العميل المحتمل"),
            new("closed_at", "تاريخ إغلاق الصفقة، إن أُغلقت", Required: false),
            new("stage", "اسم المرحلة الحالية في مسار المبيعات"),
            new("value", "قيمة الصفقة"),
            new("churned", "هل فُقد هذا العميل بعد الإغلاق (صحيح/خطأ)", Required: false),
        }),
        new("pipeline_stages", "ترتيب مراحل مسار المبيعات الحقيقي", new List<InteractiveColumnContract>
        {
            new("name", "اسم المرحلة"),
            new("order", "ترتيب المرحلة في المسار"),
        }, Required: false),
    };

    public async Task<InteractiveDataResult> BuildAsync(string projectId, CancellationToken ct)
    {
        var missing = new[] { "العملاء المحتملون (leads)" };
        var matched = await _matcher.MatchAsync(projectId, Contract, ct);
        if (matched is null) return InteractiveDataResult.Unlinked(missing);

        var leadRows = await _matcher.QueryAsync(BuildSelect(matched.Tables["leads"], "industry", "city", "org_size", "agent", "created_at", "closed_at", "stage", "value", "churned"), matched.Context, ct);
        if (leadRows.Count == 0) return InteractiveDataResult.Unlinked(missing);

        var parsed = leadRows
            .Select(r => (Industry: Str(r, "industry"), City: Str(r, "city"), OrgSize: Str(r, "org_size"), Agent: Str(r, "agent"),
                CreatedAt: ParseDate(r, "created_at"), ClosedAt: ParseDate(r, "closed_at"), Stage: Str(r, "stage"),
                Value: Num(r, "value"), Churned: Bool(r, "churned")))
            .Where(r => r.CreatedAt is not null && r.Stage is not null)
            .Select(r => (r.Industry, r.City, r.OrgSize, r.Agent, CreatedAt: r.CreatedAt!.Value, r.ClosedAt, Stage: r.Stage!, r.Value, r.Churned))
            .ToList();
        if (parsed.Count == 0) return InteractiveDataResult.Unlinked(missing);

        List<string> stages;
        if (matched.Tables.TryGetValue("pipeline_stages", out var stagesTable))
        {
            var rows = await _matcher.QueryAsync(BuildSelect(stagesTable, "name", "order"), matched.Context, ct);
            stages = rows
                .Select(r => (Name: Str(r, "name"), Order: Num(r, "order")))
                .Where(r => r.Name is not null)
                .OrderBy(r => r.Order)
                .Select(r => r.Name!)
                .ToList();
        }
        else
        {
            stages = new List<string>();
        }
        if (stages.Count == 0)
        {
            // No explicit stage table — order stages by each name's first appearance in the
            // real lead data, which is still a real order (chronologically first-seen), not a
            // guess, and always matches the project's actual stage vocabulary.
            stages = parsed.OrderBy(l => l.CreatedAt).Select(l => l.Stage).Distinct().ToList();
        }
        var stageIndex = stages.Select((s, i) => (s, i)).ToDictionary(x => x.s, x => x.i, StringComparer.Ordinal);

        var leads = parsed.Select(l => new
        {
            createdAt = l.CreatedAt.ToString("yyyy-MM-dd"),
            industry = l.Industry ?? "",
            city = l.City ?? "",
            orgSize = l.OrgSize ?? "",
            agent = l.Agent ?? "",
            stageIndex = stageIndex.TryGetValue(l.Stage, out var idx) ? idx : 0,
            value = l.Value,
            closedAt = l.ClosedAt?.ToString("yyyy-MM-dd"),
            churned = l.Churned,
        }).ToList();

        return InteractiveDataResult.Ready(new { stages, leads });
    }
}
