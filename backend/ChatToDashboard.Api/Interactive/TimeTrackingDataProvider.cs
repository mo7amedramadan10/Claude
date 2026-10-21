using System.Text.Json;

namespace ChatToDashboard.Api.Interactive;

/// <summary>
/// §4.21's real data provider for time-tracking ("تتبع الوقت للفرق") — matches the project's
/// real tables against the §4.21 contract (time_entries/projects/leaves/presence) and builds
/// the shape time-tracking.js's JEEM_TIMETRACKING_MOUNT expects. The engine does its own
/// period aggregation (day/week/month) client-side from raw time_entries, so this provider is
/// mostly plain column-mapped SELECTs — no server-side aggregation needed, unlike team-tasks.
/// §4.21's own contract has no dedicated "people" table (every other table only carries a
/// bare user_id) — "people" here is matched as an optional extra table; when a project has no
/// such directory, each person's display name falls back to their raw user_id rather than
/// fabricating one.
/// </summary>
public class TimeTrackingDataProvider : IInteractiveDataProvider
{
    public string TemplateId => "time-tracking";

    private readonly InteractiveContractMatcher _matcher;

    public TimeTrackingDataProvider(InteractiveContractMatcher matcher) => _matcher = matcher;

    private static readonly IReadOnlyList<InteractiveTableContract> Contract = new List<InteractiveTableContract>
    {
        new("time_entries", "سجلات الوقت المسجّل لكل شخص على كل مشروع", new List<InteractiveColumnContract>
        {
            new("user_id", "معرّف الشخص"),
            new("project_id", "معرّف أو اسم المشروع"),
            new("started_at", "وقت بداية هذه الفترة المسجّلة"),
            new("duration_min", "مدة هذه الفترة بالدقائق"),
            new("billable", "هل هذه الفترة قابلة للفوترة (صحيح/خطأ)", Required: false),
        }),
        new("projects", "المشاريع", new List<InteractiveColumnContract>
        {
            new("id", "معرّف المشروع"),
            new("name", "اسم المشروع"),
            new("color", "لون تمييز المشروع (كود hex)", Required: false),
            new("billable_rate", "نسبة الفوترة المئوية الافتراضية لهذا المشروع", Required: false),
        }),
        new("leaves", "إجازات الفريق", new List<InteractiveColumnContract>
        {
            new("user_id", "معرّف الشخص صاحب الإجازة"),
            new("type", "نوع الإجازة: annual أو sick أو remote"),
            new("start_date", "تاريخ بداية الإجازة"),
            new("end_date", "تاريخ نهاية الإجازة"),
        }, Required: false),
        new("presence", "حالة حضور كل شخص الآن", new List<InteractiveColumnContract>
        {
            new("user_id", "معرّف الشخص"),
            new("status", "الحالة: online أو offline أو leave أو remote"),
        }, Required: false),
        new("people", "دليل الأشخاص بأسمائهم ووظائفهم (قد لا يكون موجودًا كجدول مستقل)", new List<InteractiveColumnContract>
        {
            new("id", "معرّف الشخص"),
            new("name", "اسم الشخص"),
            new("role", "المسمى الوظيفي", Required: false),
        }, Required: false),
    };

    public async Task<InteractiveDataResult> BuildAsync(string projectId, CancellationToken ct)
    {
        var missing = new[] { "سجلات الوقت (time_entries)", "المشاريع (projects)", "الإجازات (leaves)", "الحضور (presence)" };
        var matched = await _matcher.MatchAsync(projectId, Contract, ct);
        if (matched is null) return InteractiveDataResult.Unlinked(missing);

        var entriesTable = matched.Tables["time_entries"];
        var projectsTable = matched.Tables["projects"];

        var entryRows = await _matcher.QueryAsync(BuildSelect(entriesTable, "user_id", "project_id", "started_at", "duration_min", "billable"), matched.Context, ct);
        var projectRows = await _matcher.QueryAsync(BuildSelect(projectsTable, "id", "name", "color", "billable_rate"), matched.Context, ct);
        if (entryRows.Count == 0 || projectRows.Count == 0) return InteractiveDataResult.Unlinked(missing);

        var palette = new[] { "#127DBD", "#00BAB3", "#6E62B0", "#D9822B", "#28B6E6", "#C2569B", "#7CB342", "#E5B33B", "#9AA5B1" };
        var projects = projectRows.Select((r, i) => new
        {
            id = Str(r, "id"),
            name = Str(r, "name") ?? "",
            color = Str(r, "color") is { Length: > 0 } c ? c : palette[i % palette.Length],
            billableRate = (int)Num(r, "billable_rate"),
        }).ToList();

        var timeEntries = entryRows.Select((r, i) => new
        {
            id = "te" + i,
            userId = Str(r, "user_id"),
            projectId = Str(r, "project_id"),
            startedAt = Str(r, "started_at"),
            durationMin = (int)Num(r, "duration_min"),
            billable = Bool(r, "billable"),
        }).ToList();

        var userIds = timeEntries.Select(e => e.userId).Where(x => x is not null).Distinct().ToList();

        var people = new List<(string? Id, string Name, string Role)>();
        if (matched.Tables.TryGetValue("people", out var peopleTable))
        {
            var rows = await _matcher.QueryAsync(BuildSelect(peopleTable, "id", "name", "role"), matched.Context, ct);
            people = rows.Select(r => (Str(r, "id"), Str(r, "name") ?? Str(r, "id") ?? "", Str(r, "role") ?? "")).ToList();
        }
        var knownIds = people.Select(p => p.Id).ToHashSet();
        foreach (var uid in userIds)
        {
            if (uid is not null && !knownIds.Contains(uid))
                people.Add((uid, uid, ""));
        }
        var peopleOut = people.Select(p => (object)new { id = p.Id, name = p.Name, role = p.Role }).ToList();

        var leaves = new List<object>();
        if (matched.Tables.TryGetValue("leaves", out var leavesTable))
        {
            var rows = await _matcher.QueryAsync(BuildSelect(leavesTable, "user_id", "type", "start_date", "end_date"), matched.Context, ct);
            leaves = rows.Select(r => (object)new { userId = Str(r, "user_id"), type = Str(r, "type") ?? "annual", startDate = DateStr(r, "start_date"), endDate = DateStr(r, "end_date") }).ToList();
        }

        var presence = new List<object>();
        if (matched.Tables.TryGetValue("presence", out var presenceTable))
        {
            var rows = await _matcher.QueryAsync(BuildSelect(presenceTable, "user_id", "status"), matched.Context, ct);
            presence = rows.Select(r => (object)new { userId = Str(r, "user_id"), status = Str(r, "status") ?? "offline" }).ToList();
        }

        return InteractiveDataResult.Ready(new { projects, people = peopleOut, timeEntries, leaves, presence });
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

    private static double Num(Dictionary<string, JsonElement> row, string col)
    {
        if (!row.TryGetValue(col, out var v)) return 0;
        return v.ValueKind switch
        {
            JsonValueKind.Number => v.GetDouble(),
            JsonValueKind.String when double.TryParse(v.GetString(), out var d) => d,
            _ => 0,
        };
    }

    private static bool Bool(Dictionary<string, JsonElement> row, string col)
    {
        if (!row.TryGetValue(col, out var v)) return true; // billable defaults true when unknown — matches "not excluded" being the safer display default.
        return v.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => v.GetDouble() != 0,
            JsonValueKind.String => v.GetString() is { } s && (s.Equals("true", StringComparison.OrdinalIgnoreCase) || s == "1"),
            _ => true,
        };
    }

    private static string? DateStr(Dictionary<string, JsonElement> row, string col)
    {
        var s = Str(row, col);
        if (string.IsNullOrWhiteSpace(s)) return null;
        return DateTime.TryParse(s, out var dt) ? dt.ToString("yyyy-MM-dd") : s;
    }
}
