using static ChatToDashboard.Api.Interactive.InteractiveRowHelpers;

namespace ChatToDashboard.Api.Interactive;

/// <summary>
/// §4.22's real data provider for project-studio ("استوديو المشاريع") — matches the project's
/// real tables against team members/projects/today's tasks/studio chat and builds the shape
/// project-studio.js's JEEM_STUDIO_MOUNT expects. data.team/data.projects/data.todayTasks all
/// reference team members by their position in data.team (the engine's own convention —
/// `TEAM[w]`, `data-who="${i}"`), so every assignee id found in the real tables is resolved to
/// that array index here, once, rather than carrying real ids through to the frontend. Messages
/// are an OPTIONAL table: a channel with no chat history yet is still a fully linked, useful
/// dashboard (status/tasks/Gantt/team), same "degrade gracefully, don't block linking over one
/// missing piece" rule token-treasury's tvl_history/participation already established. Like
/// crm-pipeline's own doc comment notes, this payload has no per-viewer identity — a message's
/// "dir" (out/in) is a real `direction` column naming which side of the channel sent it, not
/// something resolved per request-user.
/// </summary>
public class ProjectStudioDataProvider : IInteractiveDataProvider
{
    public string TemplateId => "project-studio";

    private readonly InteractiveContractMatcher _matcher;

    public ProjectStudioDataProvider(InteractiveContractMatcher matcher) => _matcher = matcher;

    private static readonly IReadOnlyList<InteractiveTableContract> Contract = new List<InteractiveTableContract>
    {
        new("project_studio_team", "أعضاء فريق الاستوديو", new List<InteractiveColumnContract>
        {
            new("id", "معرّف العضو"),
            new("name", "اسم العضو"),
            new("role", "المسمى الوظيفي للعضو", Required: false),
        }),
        new("projects", "مشاريع الاستوديو القادمة والجارية والمكتملة، بجدولها الزمني وفريق كل مشروع", new List<InteractiveColumnContract>
        {
            new("name", "اسم المشروع"),
            new("status", "حالة المشروع: up (قادم) أو prog (قيد التنفيذ) أو done (مكتمل)"),
            new("start_date", "تاريخ بداية المشروع"),
            new("end_date", "تاريخ نهاية المشروع"),
            new("assignee_ids", "معرّفات أعضاء الفريق المكلَّفين بهذا المشروع، مفصولة بفواصل", Required: false),
            new("color", "لون مخصّص لعرض المشروع في الجدول الزمني (قيمة HEX)", Required: false),
        }),
        new("today_tasks", "مهام مكلَّف بها الفريق، لكل منها تاريخ استحقاق", new List<InteractiveColumnContract>
        {
            new("title", "عنوان المهمة"),
            new("due_date", "تاريخ استحقاق المهمة"),
            new("assignee_ids", "معرّفات أعضاء الفريق المكلَّفين بهذه المهمة، مفصولة بفواصل", Required: false),
            new("done", "هل أُنجزت المهمة (صحيح/خطأ)", Required: false),
        }, Required: false),
        new("studio_messages", "سجل محادثة فريق الاستوديو", new List<InteractiveColumnContract>
        {
            new("sent_at", "وقت إرسال الرسالة"),
            new("direction", "اتجاه الرسالة: out (من صاحب القناة) أو in (من الفريق)"),
            new("kind", "نوع الرسالة: text أو voice أو img", Required: false),
            new("text", "نص الرسالة (لرسائل النص)", Required: false),
            new("duration_label", "مدة الرسالة الصوتية المعروضة (لرسائل الصوت)", Required: false),
        }, Required: false),
    };

    public async Task<InteractiveDataResult> BuildAsync(string projectId, CancellationToken ct)
    {
        var missing = new[] { "فريق الاستوديو (project_studio_team)", "المشاريع (projects)" };
        var matched = await _matcher.MatchAsync(projectId, Contract, ct);
        if (matched is null) return InteractiveDataResult.Unlinked(missing);

        var teamTable = matched.Tables["project_studio_team"];
        var projectsTable = matched.Tables["projects"];

        var teamRows = await _matcher.QueryAsync(BuildSelect(teamTable, "id", "name", "role"), matched.Context, ct);
        var projectRows = await _matcher.QueryAsync(BuildSelect(projectsTable, "name", "status", "start_date", "end_date", "assignee_ids", "color"), matched.Context, ct);
        if (teamRows.Count == 0 || projectRows.Count == 0) return InteractiveDataResult.Unlinked(missing);

        var memberIds = teamRows.Select(r => Str(r, "id")).ToList();
        var indexOfMember = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i < memberIds.Count; i++) if (memberIds[i] is not null) indexOfMember[memberIds[i]!] = i;
        List<int> ResolveAssignees(string? raw) => string.IsNullOrWhiteSpace(raw)
            ? new()
            : raw.Split(new[] { ',', '|', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(id => indexOfMember.TryGetValue(id, out var idx) ? idx : -1)
                .Where(idx => idx >= 0)
                .ToList();

        var team = teamRows.Select(r => new { name = Str(r, "name") ?? "", role = Str(r, "role") ?? "" }).ToList();

        var validStatuses = new HashSet<string> { "up", "prog", "done" };
        var projects = projectRows
            .Select(r => new
            {
                name = Str(r, "name") ?? "",
                status = validStatuses.Contains(Str(r, "status") ?? "") ? Str(r, "status")! : "up",
                start = DateStr(r, "start_date"),
                end = DateStr(r, "end_date"),
                who = ResolveAssignees(Str(r, "assignee_ids")),
                color = Str(r, "color"),
            })
            .Where(p => p.start is not null && p.end is not null)
            .ToList();

        var statusTotals = new
        {
            up = projects.Count(p => p.status == "up"),
            prog = projects.Count(p => p.status == "prog"),
            done = projects.Count(p => p.status == "done"),
            all = projects.Count,
        };

        List<object> todayTasks = new();
        if (matched.Tables.TryGetValue("today_tasks", out var tasksTable))
        {
            var taskRows = await _matcher.QueryAsync(BuildSelect(tasksTable, "title", "due_date", "assignee_ids", "done"), matched.Context, ct);
            var today = DateTime.Now.Date;
            todayTasks = taskRows
                .Select(r => (Title: Str(r, "title"), Due: ParseDate(r, "due_date"), Assignees: Str(r, "assignee_ids"), Done: Bool(r, "done")))
                .Where(t => t.Title is not null && t.Due?.Date == today)
                .Select(t => (object)new { title = t.Title, who = ResolveAssignees(t.Assignees), done = t.Done })
                .ToList();
        }

        List<object> messages = new();
        if (matched.Tables.TryGetValue("studio_messages", out var msgTable))
        {
            var msgRows = await _matcher.QueryAsync(BuildSelect(msgTable, "sent_at", "direction", "kind", "text", "duration_label"), matched.Context, ct);
            var parsed = msgRows
                .Select(r => (At: ParseDate(r, "sent_at"), Dir: Str(r, "direction"), Kind: Str(r, "kind") ?? "text", Text: Str(r, "text"), Dur: Str(r, "duration_label")))
                .Where(m => m.At is not null && (m.Dir == "out" || m.Dir == "in"))
                .OrderBy(m => m.At)
                .ToList();
            messages = BuildMessageFeed(parsed!);
        }

        return InteractiveDataResult.Ready(new { team, projects, statusTotals, todayTasks, messages });
    }

    private static readonly string[] ArabicMonths =
    {
        "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
        "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر",
    };

    /// <summary>Inserts a "اليوم"/"أمس"/date day-separator before the first message of every
    /// calendar day — a pure display grouping the engine's own chat markup expects
    /// (`{type:'day', label}` entries interleaved with `{type:'msg', ...}` ones), computed
    /// here rather than required of the source table, the same way every other provider in
    /// this batch keeps pure formatting conventions out of its logical data contract.</summary>
    internal static List<object> BuildMessageFeed(List<(DateTime? At, string? Dir, string Kind, string? Text, string? Dur)> rows)
    {
        var today = DateTime.Now.Date;
        var feed = new List<object>();
        DateTime? lastDay = null;
        foreach (var m in rows)
        {
            var day = m.At!.Value.Date;
            if (lastDay != day)
            {
                var label = day == today ? "اليوم" : day == today.AddDays(-1) ? "أمس" : $"{day.Day} {ArabicMonths[day.Month - 1]}";
                feed.Add(new { type = "day", label });
                lastDay = day;
            }
            var at = m.At!.Value;
            var hour12 = at.Hour % 12 == 0 ? 12 : at.Hour % 12;
            var time = $"{hour12}:{at.Minute:D2} {(at.Hour < 12 ? "ص" : "م")}";
            feed.Add(new { type = "msg", dir = m.Dir, kind = m.Kind, text = m.Text, dur = m.Dur, time });
        }
        return feed;
    }
}
