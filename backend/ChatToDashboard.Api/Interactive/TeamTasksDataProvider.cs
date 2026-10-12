using System.Text.Json;

namespace ChatToDashboard.Api.Interactive;

/// <summary>
/// §4.20's real data provider for team-tasks ("إدارة مهام الفريق") — matches the project's
/// real tables against the §4.20 contract (tasks/time_entries/calendar_items/meetings/
/// reminders/activity_daily/project_time_share) and builds the shape team-tasks.js's
/// JEEM_TEAMTASKS_MOUNT expects. Unlike crm-pipeline/warehouse-team this needs real
/// aggregation, not a plain column-mapped SELECT: time spent per task is SUM(stopped_at -
/// started_at) over its completed time_entries, and the "active/running" task is whichever
/// one has an open entry (stopped_at IS NULL) — both computed in C# after a plain fetch,
/// portable across SQLite/SQL Server without dialect-specific date-diff SQL. Meetings/
/// reminders are filtered to today's real date (the engine has no date picker of its own —
/// it only ever shows "today's" items), and calendar_items are filtered to the current real
/// week (Sun-Thu) and converted from a real date to the engine's 0-4 day-of-week index.
/// </summary>
public class TeamTasksDataProvider : IInteractiveDataProvider
{
    public string TemplateId => "team-tasks";

    private readonly InteractiveContractMatcher _matcher;

    public TeamTasksDataProvider(InteractiveContractMatcher matcher) => _matcher = matcher;

    private static readonly IReadOnlyList<InteractiveTableContract> Contract = new List<InteractiveTableContract>
    {
        new("tasks", "مهام الفريق القابلة لتتبّع الوقت عليها", new List<InteractiveColumnContract>
        {
            new("id", "معرّف المهمة"),
            new("title", "عنوان المهمة"),
            new("project_id", "اسم أو معرّف المشروع التابعة له المهمة", Required: false),
            new("daily_limit_sec", "الحد اليومي المستهدف لهذه المهمة بالثواني", Required: false),
        }),
        new("time_entries", "سجلات تشغيل/إيقاف مؤقت كل مهمة (كل صف = فترة تشغيل واحدة)", new List<InteractiveColumnContract>
        {
            new("task_id", "معرّف المهمة المرتبطة بهذه الفترة"),
            new("started_at", "وقت بدء التشغيل"),
            new("stopped_at", "وقت الإيقاف (فارغ/null لو لسه شغّال)", Required: false),
        }),
        new("calendar_items", "عناصر تقويم الأسبوع (مهام مجدولة بوقت ومدة محددين)", new List<InteractiveColumnContract>
        {
            new("id", "معرّف العنصر"),
            new("title", "عنوان العنصر"),
            new("description", "وصف العنصر", Required: false),
            new("day", "التاريخ الفعلي لهذا العنصر"),
            new("start_min", "دقيقة بداية العنصر من منتصف الليل (0-1439)"),
            new("duration_min", "مدة العنصر بالدقائق"),
            new("priority", "أولوية العنصر: high أو med أو low أو done"),
            new("assignees", "أسماء المسؤولين عن هذا العنصر، مفصولة بفاصلة", Required: false),
            new("comments_count", "عدد التعليقات على هذا العنصر", Required: false),
            new("task_id", "معرّف المهمة المرتبطة (لو هذا العنصر مرتبط بمؤقت مهمة)", Required: false),
        }),
        new("meetings", "اجتماعات اليوم", new List<InteractiveColumnContract>
        {
            new("id", "معرّف الاجتماع"),
            new("title", "عنوان الاجتماع"),
            new("start_at", "وقت بدء الاجتماع"),
            new("kind", "نوع الاجتماع (مثل: video أو users)", Required: false),
        }, Required: false),
        new("reminders", "تذكيرات اليوم", new List<InteractiveColumnContract>
        {
            new("id", "معرّف التذكير"),
            new("title", "عنوان التذكير"),
            new("at", "وقت التذكير"),
            new("priority", "أولوية التذكير: high أو low", Required: false),
            new("done", "هل أُنجز التذكير (صحيح/خطأ)", Required: false),
        }, Required: false),
        new("activity_daily", "نسبة النشاط اليومي للفريق خلال الأسبوع الحالي", new List<InteractiveColumnContract>
        {
            new("date", "التاريخ"),
            new("pct", "نسبة النشاط المئوية"),
        }, Required: false),
        new("project_time_share", "نسبة توزيع الوقت على كل مشروع", new List<InteractiveColumnContract>
        {
            new("project_id", "اسم أو معرّف المشروع"),
            new("pct", "النسبة المئوية من إجمالي الوقت"),
        }, Required: false),
    };

    public async Task<InteractiveDataResult> BuildAsync(string projectId, CancellationToken ct)
    {
        var missing = new[] { "المهام (tasks)", "سجلات الوقت (time_entries)", "عناصر التقويم (calendar_items)", "الاجتماعات (meetings)", "التذكيرات (reminders)", "النشاط اليومي (activity_daily)", "توزيع الوقت على المشاريع (project_time_share)" };
        var matched = await _matcher.MatchAsync(projectId, Contract, ct);
        if (matched is null) return InteractiveDataResult.Unlinked(missing);

        var tasksTable = matched.Tables["tasks"];
        var entriesTable = matched.Tables["time_entries"];
        var calTable = matched.Tables["calendar_items"];

        var taskRows = await _matcher.QueryAsync(BuildSelect(tasksTable, "id", "title", "project_id", "daily_limit_sec"), matched.Context, ct);
        var entryRows = await _matcher.QueryAsync(BuildSelect(entriesTable, "task_id", "started_at", "stopped_at"), matched.Context, ct);
        var calRows = await _matcher.QueryAsync(BuildSelect(calTable, "id", "title", "description", "day", "start_min", "duration_min", "priority", "assignees", "comments_count", "task_id"), matched.Context, ct);
        if (taskRows.Count == 0) return InteractiveDataResult.Unlinked(missing);

        var entries = entryRows.Select(r => new { taskId = Str(r, "task_id"), started = DateOf(r, "started_at"), stopped = DateOf(r, "stopped_at") }).ToList();
        var openEntry = entries.Where(e => e.stopped is null && e.started.HasValue).OrderByDescending(e => e.started).FirstOrDefault();

        var tasks = taskRows.Select(r =>
        {
            var id = Str(r, "id");
            var spentSec = entries.Where(e => e.taskId == id && e.started.HasValue && e.stopped.HasValue)
                .Sum(e => Math.Max(0, (e.stopped!.Value - e.started!.Value).TotalSeconds));
            return new
            {
                id,
                title = Str(r, "title") ?? "",
                project = Str(r, "project_id") ?? "",
                spentSec = (long)spentSec,
                dailyLimitSec = (long)Num(r, "daily_limit_sec", 3600),
                favorite = false,
            };
        }).ToList();

        var now0 = DateTime.Now;
        var weekStart = now0.Date.AddDays(-(int)now0.DayOfWeek);

        var calendarItems = calRows
            .Select(r => new { r, date = DateOf(r, "day") })
            .Where(x => x.date.HasValue)
            .Select(x => new { x.r, dayIndex = (x.date!.Value.Date - weekStart).Days })
            .Where(x => x.dayIndex is >= 0 and <= 4)
            .Select(x => new
            {
                id = Str(x.r, "id"),
                day = x.dayIndex,
                startMin = (int)Num(x.r, "start_min"),
                durationMin = (int)Num(x.r, "duration_min", 60),
                priority = Str(x.r, "priority") ?? "low",
                title = Str(x.r, "title") ?? "",
                description = Str(x.r, "description") ?? "",
                assignees = (Str(x.r, "assignees") ?? "").Split(new[] { ',', '،', ';', '؛' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
                commentsCount = (int)Num(x.r, "comments_count"),
                taskId = Str(x.r, "task_id"),
            }).ToList();

        var meetings = new List<object>();
        var reminders = new List<object>();
        if (matched.Tables.TryGetValue("meetings", out var meetingsTable))
        {
            var rows = await _matcher.QueryAsync(BuildSelect(meetingsTable, "id", "title", "start_at", "kind"), matched.Context, ct);
            meetings = rows.Select(r => new { r, at = DateOf(r, "start_at") }).Where(x => x.at.HasValue && x.at.Value.Date == now0.Date)
                .Select(x => (object)new { id = Str(x.r, "id"), startMin = x.at!.Value.Hour * 60 + x.at.Value.Minute, title = Str(x.r, "title") ?? "", kind = Str(x.r, "kind") ?? "video" }).ToList();
        }
        if (matched.Tables.TryGetValue("reminders", out var remindersTable))
        {
            var rows = await _matcher.QueryAsync(BuildSelect(remindersTable, "id", "title", "at", "priority", "done"), matched.Context, ct);
            reminders = rows.Select(r => new { r, at = DateOf(r, "at") }).Where(x => x.at.HasValue && x.at.Value.Date == now0.Date)
                .Select(x => (object)new { id = Str(x.r, "id"), atMin = x.at!.Value.Hour * 60 + x.at.Value.Minute, title = Str(x.r, "title") ?? "", priority = Str(x.r, "priority") ?? "low", done = Bool(x.r, "done") }).ToList();
        }

        var activityDaily = new List<object>();
        if (matched.Tables.TryGetValue("activity_daily", out var actTable))
        {
            var rows = await _matcher.QueryAsync(BuildSelect(actTable, "date", "pct"), matched.Context, ct);
            activityDaily = rows.Select(r => new { r, date = DateOf(r, "date") }).Where(x => x.date.HasValue)
                .Select(x => new { dayIndex = (x.date!.Value.Date - weekStart).Days, pct = Num(x.r, "pct") })
                .Where(x => x.dayIndex is >= 0 and <= 4)
                .Select(x => (object)new { day = x.dayIndex, pct = (int)x.pct }).ToList();
        }

        var projectTimeShare = new List<object>();
        if (matched.Tables.TryGetValue("project_time_share", out var shareTable))
        {
            var rows = await _matcher.QueryAsync(BuildSelect(shareTable, "project_id", "pct"), matched.Context, ct);
            var palette = new[] { "#127DBD", "#00BAB3", "#6E62B0", "#D9822B", "#28B6E6", "#C2569B", "#7CB342" };
            projectTimeShare = rows.Select((r, i) => (object)new { name = Str(r, "project_id") ?? "", pct = (int)Num(r, "pct"), color = palette[i % palette.Length] }).ToList();
        }

        return InteractiveDataResult.Ready(new
        {
            tasks,
            activeTaskId = openEntry?.taskId,
            running = openEntry is not null,
            calendarItems,
            meetings,
            reminders,
            activityDaily,
            projectTimeShare,
        });
    }

    private static string BuildSelect(MatchedInteractiveTable table, params string[] logicalCols)
    {
        var pairs = logicalCols.Select(key => (Key: key, Real: table.Column(key))).Where(p => p.Real is not null).ToList();
        return pairs.Count == 0
            ? $"SELECT * FROM {table.RealTable}"
            : $"SELECT {string.Join(", ", pairs.Select(p => $"{p.Real} AS \"{p.Key}\""))} FROM {table.RealTable}";
    }

    private static string? Str(Dictionary<string, JsonElement> row, string col) =>
        row.TryGetValue(col, out var v) && v.ValueKind is JsonValueKind.String ? v.GetString()
        : row.TryGetValue(col, out v) && v.ValueKind is JsonValueKind.Number ? v.ToString()
        : null;

    private static double Num(Dictionary<string, JsonElement> row, string col, double fallback = 0)
    {
        if (!row.TryGetValue(col, out var v)) return fallback;
        return v.ValueKind switch
        {
            JsonValueKind.Number => v.GetDouble(),
            JsonValueKind.String when double.TryParse(v.GetString(), out var d) => d,
            _ => fallback,
        };
    }

    private static bool Bool(Dictionary<string, JsonElement> row, string col)
    {
        if (!row.TryGetValue(col, out var v)) return false;
        return v.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => v.GetDouble() != 0,
            JsonValueKind.String => v.GetString() is { } s && (s.Equals("true", StringComparison.OrdinalIgnoreCase) || s == "1"),
            _ => false,
        };
    }

    private static DateTime? DateOf(Dictionary<string, JsonElement> row, string col)
    {
        var s = Str(row, col);
        return !string.IsNullOrWhiteSpace(s) && DateTime.TryParse(s, out var dt) ? dt : null;
    }
}
