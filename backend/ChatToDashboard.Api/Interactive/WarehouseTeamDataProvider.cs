using System.Text.Json;

namespace ChatToDashboard.Api.Interactive;

/// <summary>
/// §4.16's real data provider for warehouse-team ("نظرة على فريق المستودع") — matches the
/// project's real tables against staff/tasks(log)/shifts and, when staff+tasks are found,
/// builds the exact shape warehouse-team.js's JEEM_WAREHOUSE_MOUNT expects (data.staff/
/// data.off/data.feed/data.shift — see that file's own reads). "tasks" here is each staff
/// member's activity-log entries (what populates both their card's history and the global
/// feed), not a separate fixed task-catalog table. "shifts" (next shift-change time) is
/// optional — the engine already has a sane fallback when it's absent.
/// </summary>
public class WarehouseTeamDataProvider : IInteractiveDataProvider
{
    public string TemplateId => "warehouse-team";

    private readonly InteractiveContractMatcher _matcher;

    public WarehouseTeamDataProvider(InteractiveContractMatcher matcher) => _matcher = matcher;

    private static readonly IReadOnlyList<InteractiveTableContract> Contract = new List<InteractiveTableContract>
    {
        new("staff", "موظفو المستودع، في المناوبة أو خارجها", new List<InteractiveColumnContract>
        {
            new("id", "معرّف الموظف"),
            new("name", "اسم الموظف"),
            new("role", "دور الموظف (مثل: مشغّل رافعة، مسؤول استلام)", Required: false),
            new("zone", "منطقة العمل الحالية للموظف", Required: false),
            new("on_shift", "هل الموظف في المناوبة الحالية الآن (صحيح/خطأ)"),
            new("current_task", "وصف المهمة الحالية التي ينفّذها الموظف", Required: false),
            new("current_order", "رقم/كود الطلب الحالي الذي يعمل عليه الموظف", Required: false),
            new("tasks_done", "عدد المهام المنجزة في هذه المناوبة", Required: false),
            new("tasks_total", "إجمالي عدد المهام المطلوبة في هذه المناوبة", Required: false),
            new("efficiency_pct", "نسبة كفاءة الموظف المئوية", Required: false),
            new("shift_return_at", "موعد عودة الموظف للمناوبة القادمة (لموظف خارج المناوبة)", Required: false),
        }),
        new("tasks", "سجل أحداث/أنشطة الموظفين (كل صف = حدث واحد لموظف في وقت معيّن)", new List<InteractiveColumnContract>
        {
            new("staff_id", "معرّف الموظف صاحب هذا الحدث (يطابق مفتاح جدول الموظفين)"),
            new("description", "وصف الحدث/النشاط"),
            new("logged_at", "تاريخ ووقت حدوث هذا الحدث"),
        }),
        new("shifts", "معلومات جدول المناوبات (موعد تغيير المناوبة القادم)", new List<InteractiveColumnContract>
        {
            new("next_change_at", "تاريخ ووقت تغيير المناوبة القادم"),
        }, Required: false),
    };

    public async Task<InteractiveDataResult> BuildAsync(string projectId, CancellationToken ct)
    {
        var missing = new[] { "بيانات الموظفين (staff)", "المهام (tasks)", "المناوبات (shifts)" };
        var matched = await _matcher.MatchAsync(projectId, Contract, ct);
        if (matched is null) return InteractiveDataResult.Unlinked(missing);

        var staffTable = matched.Tables["staff"];
        var tasksTable = matched.Tables["tasks"];

        var staffSql = BuildSelect(staffTable, "id", "name", "role", "zone", "on_shift", "current_task", "current_order", "tasks_done", "tasks_total", "efficiency_pct", "shift_return_at");
        var tasksSql = BuildSelect(tasksTable, "staff_id", "description", "logged_at");

        var staffRows = await _matcher.QueryAsync(staffSql, matched.Context, ct);
        var taskRows = await _matcher.QueryAsync(tasksSql, matched.Context, ct);
        if (staffRows.Count == 0) return InteractiveDataResult.Unlinked(missing);

        var logsByStaff = taskRows
            .Select(r => new { staffId = Str(r, "staff_id"), desc = Str(r, "description") ?? "", at = DateOf(r, "logged_at") })
            .Where(x => x.staffId is not null)
            .OrderByDescending(x => x.at ?? DateTime.MinValue)
            .ToList();

        var staff = staffRows.Select(r => new
        {
            id = Str(r, "id"),
            name = Str(r, "name") ?? "",
            role = Str(r, "role") ?? "",
            zone = Str(r, "zone") ?? "",
            onShift = Bool(r, "on_shift"),
            task = Str(r, "current_task") ?? "",
            ord = Str(r, "current_order") ?? "",
            done = (int)Num(r, "tasks_done"),
            tot = (int)Num(r, "tasks_total"),
            eff = (int)Num(r, "efficiency_pct"),
            ret = TimeOf(r, "shift_return_at"),
        }).ToList();

        var onShiftStaff = staff.Where(s => s.onShift).Select(s => new
        {
            id = s.id, name = s.name, role = s.role, zone = s.zone, on = true, eff = s.eff, done = s.done, tot = s.tot, task = s.task, ord = s.ord,
            log = logsByStaff.Where(l => l.staffId == s.id).Take(6).Select(l => new[] { l.at.HasValue ? l.at.Value.ToString("HH:mm") : "", HtmlEsc(l.desc) }).ToList(),
        }).ToList();

        var off = staff.Where(s => !s.onShift).Select(s => new
        {
            id = s.id, name = s.name, role = s.role, zone = s.zone, done = s.done, eff = s.eff, ret = s.ret,
        }).ToList();

        var feed = logsByStaff.Take(40).Select(l => new
        {
            w = l.staffId,
            html = HtmlEsc(l.desc),
            kind = "info",
            t = l.at.HasValue ? l.at.Value.ToString("HH:mm") : "",
            unread = true,
        }).ToList();

        object? shift = null;
        if (matched.Tables.TryGetValue("shifts", out var shiftsTable))
        {
            var shiftSql = BuildSelect(shiftsTable, "next_change_at");
            var shiftRows = await _matcher.QueryAsync(shiftSql, matched.Context, ct);
            var at = shiftRows.FirstOrDefault() is { } row ? DateOf(row, "next_change_at") : null;
            if (at.HasValue) shift = new { nextAt = at.Value.ToString("o") };
        }

        return InteractiveDataResult.Ready(new { staff = onShiftStaff, off, feed, shift });
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

    private static string? TimeOf(Dictionary<string, JsonElement> row, string col) =>
        DateOf(row, col)?.ToString("HH:mm");

    /// <summary>feed[].html/staff log entries are rendered as raw innerHTML by
    /// warehouse-team.js (matching its own demo data's pre-formatted bold/chip markup) — real,
    /// uploaded text must never reach that unescaped, so every real description is HTML-escaped
    /// first. It loses the demo's bold-name styling, but renders as safe plain text instead of
    /// risking arbitrary markup/script from project data.</summary>
    private static string HtmlEsc(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
}
