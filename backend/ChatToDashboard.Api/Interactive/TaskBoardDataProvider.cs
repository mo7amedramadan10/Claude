using static ChatToDashboard.Api.Interactive.InteractiveRowHelpers;

namespace ChatToDashboard.Api.Interactive;

/// <summary>
/// §4.22's real data provider for task-board ("لوحة مهام المشاريع"). The four Kanban columns
/// (لم تبدأ/قيد التنفيذ/قيد المراجعة/مكتملة) and the three priority levels are fixed
/// vocabulary in the engine itself, not project data — see task-board.js's own doc comment —
/// so this provider only validates a task's status/priority against that fixed set, falling
/// back to todo/med for an unrecognized value rather than dropping the task. People/projects
/// are referenced by their position in data.people (the `who` index convention — see
/// ProjectStudioDataProvider's own doc comment for why assignee ids are resolved to array
/// indices here rather than carried through as real ids).
/// </summary>
public class TaskBoardDataProvider : IInteractiveDataProvider
{
    public string TemplateId => "task-board";

    private readonly InteractiveContractMatcher _matcher;

    public TaskBoardDataProvider(InteractiveContractMatcher matcher) => _matcher = matcher;

    private static readonly string[] PaletteColors = { "#127DBD", "#D9822B", "#00A89F", "#6E62B0", "#7CB342", "#C2569B" };
    private static readonly HashSet<string> ValidStatuses = new(StringComparer.Ordinal) { "todo", "doing", "review", "done" };
    private static readonly HashSet<string> ValidPriorities = new(StringComparer.Ordinal) { "high", "med", "low" };

    private static readonly IReadOnlyList<InteractiveTableContract> Contract = new List<InteractiveTableContract>
    {
        new("board_team", "أعضاء فريق اللوحة", new List<InteractiveColumnContract>
        {
            new("id", "معرّف العضو"),
            new("name", "اسم العضو"),
        }),
        new("board_projects", "المشاريع المصنَّفة لها المهام", new List<InteractiveColumnContract>
        {
            new("id", "معرّف المشروع"),
            new("name", "اسم المشروع"),
            new("color", "لون مخصّص للمشروع (قيمة HEX)", Required: false),
        }),
        new("board_tasks", "مهام اللوحة", new List<InteractiveColumnContract>
        {
            new("title", "عنوان المهمة"),
            new("category", "تصنيف المهمة", Required: false),
            new("priority", "الأولوية: high أو med أو low", Required: false),
            new("status", "الحالة: todo أو doing أو review أو done"),
            new("progress_pct", "نسبة الإنجاز المئوية", Required: false),
            new("project_id", "معرّف المشروع المرتبط بهذه المهمة"),
            new("due_date", "تاريخ الاستحقاق"),
            new("assignee_ids", "معرّفات الأعضاء المكلَّفين بهذه المهمة، مفصولة بفواصل", Required: false),
        }),
    };

    public async Task<InteractiveDataResult> BuildAsync(string projectId, CancellationToken ct)
    {
        var missing = new[] { "فريق اللوحة (board_team)", "المشاريع (board_projects)", "المهام (board_tasks)" };
        var matched = await _matcher.MatchAsync(projectId, Contract, ct);
        if (matched is null) return InteractiveDataResult.Unlinked(missing);

        var teamRows = await _matcher.QueryAsync(BuildSelect(matched.Tables["board_team"], "id", "name"), matched.Context, ct);
        var projRows = await _matcher.QueryAsync(BuildSelect(matched.Tables["board_projects"], "id", "name", "color"), matched.Context, ct);
        var taskRows = await _matcher.QueryAsync(BuildSelect(matched.Tables["board_tasks"], "title", "category", "priority", "status", "progress_pct", "project_id", "due_date", "assignee_ids"), matched.Context, ct);
        if (teamRows.Count == 0 || projRows.Count == 0 || taskRows.Count == 0) return InteractiveDataResult.Unlinked(missing);

        var memberIds = teamRows.Select(r => Str(r, "id")).ToList();
        var indexOfMember = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i < memberIds.Count; i++) if (memberIds[i] is not null) indexOfMember[memberIds[i]!] = i;
        List<int> ResolveAssignees(string? raw) => string.IsNullOrWhiteSpace(raw)
            ? new()
            : raw.Split(new[] { ',', '|', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(id => indexOfMember.TryGetValue(id, out var idx) ? idx : -1).Where(idx => idx >= 0).ToList();

        var people = teamRows.Select(r => new { name = Str(r, "name") ?? "" }).ToList();

        var projectIds = projRows.Select(r => Str(r, "id")).ToList();
        var indexOfProject = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i < projectIds.Count; i++) if (projectIds[i] is not null) indexOfProject[projectIds[i]!] = i;
        var colorIdx = 0;
        var projects = projRows.Select(r => new { name = Str(r, "name") ?? "", color = Str(r, "color") ?? PaletteColors[colorIdx++ % PaletteColors.Length] }).ToList();

        var tasks = taskRows
            .Select(r => (
                Title: Str(r, "title"), Category: Str(r, "category") ?? "عام",
                Priority: ValidPriorities.Contains(Str(r, "priority") ?? "") ? Str(r, "priority")! : "med",
                Status: ValidStatuses.Contains(Str(r, "status") ?? "") ? Str(r, "status")! : "todo",
                ProgressPct: r.ContainsKey("progress_pct") ? (int?)Num(r, "progress_pct") : null,
                ProjectId: Str(r, "project_id"), Due: DateStr(r, "due_date"), Assignees: Str(r, "assignee_ids")))
            .Where(t => t.Title is not null && t.Due is not null)
            .Select(t =>
            {
                var pj = t.ProjectId is not null && indexOfProject.TryGetValue(t.ProjectId, out var pidx) ? pidx : 0;
                var pg = t.ProgressPct ?? DefaultProgress(t.Status);
                return (object)new
                {
                    t = t.Title,
                    cat = t.Category,
                    pri = t.Priority,
                    st = t.Status,
                    pg,
                    pj,
                    d = t.Due,
                    who = ResolveAssignees(t.Assignees),
                };
            })
            .ToList();
        if (tasks.Count == 0) return InteractiveDataResult.Unlinked(missing);

        return InteractiveDataResult.Ready(new { people, projects, tasks });
    }

    private static int DefaultProgress(string status) => status switch
    {
        "done" => 100,
        "review" => 80,
        "doing" => 30,
        _ => 0,
    };
}
