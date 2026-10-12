using System.Text.Json;
using ChatToDashboard.Api.Llm;

namespace ChatToDashboard.Api.Interactive;

/// <summary>
/// §4.19's real data provider for crm-pipeline ("إدارة العملاء — مراحل البيع") — matches the
/// project's real tables against this template's contract (customers/stages/
/// new_customers_daily/pipeline_stats, per INTERACTIVE-HANDOFF.md §4.19) and, when every
/// required table is found, builds the exact JSON shape crm-pipeline.js's JEEM_CRM_MOUNT
/// expects (see that file's own `data.stages`/`data.members`/`data.customers`/
/// `data.newCustomersDaily`/`data.pipelineStats` reads). currentUserName/currentUserRole/
/// today are deliberately left unset — BuildAsync has no per-viewer context (this payload is
/// the same for every viewer of the project, not re-built per request-user), so the engine's
/// own generic "أنا" fallback applies instead of guessing an identity.
/// </summary>
public class CrmPipelineDataProvider : IInteractiveDataProvider
{
    public string TemplateId => "crm-pipeline";

    private readonly InteractiveContractMatcher _matcher;

    public CrmPipelineDataProvider(InteractiveContractMatcher matcher) => _matcher = matcher;

    private static readonly IReadOnlyList<InteractiveTableContract> Contract = new List<InteractiveTableContract>
    {
        new("stages", "مراحل البيع (عمود المفتاح، اسم المرحلة، ترتيبها، وهل هي مرحلة الفوز/الصفقة المغلقة)", new List<InteractiveColumnContract>
        {
            new("key", "معرّف قصير فريد للمرحلة"),
            new("name", "الاسم المعروض للمرحلة"),
            new("order", "ترتيب المرحلة ضمن التدفق", Required: false),
            new("is_won", "هل هذه مرحلة إغلاق الصفقة بنجاح (صحيح/خطأ)", Required: false),
        }),
        new("members", "أعضاء فريق المبيعات المسؤولون عن العملاء", new List<InteractiveColumnContract>
        {
            new("id", "معرّف العضو"),
            new("name", "اسم العضو"),
            new("role", "المسمى الوظيفي للعضو", Required: false),
        }),
        new("customers", "العملاء/الصفقات في قمع المبيعات", new List<InteractiveColumnContract>
        {
            new("id", "معرّف العميل"),
            new("name", "اسم العميل"),
            new("description", "وصف مختصر لنشاط العميل", Required: false),
            new("stage", "مفتاح المرحلة الحالية لهذا العميل (يطابق مفتاح جدول المراحل)"),
            new("due_date", "تاريخ الاستحقاق القادم لهذا العميل", Required: false),
            new("owner_id", "معرّف عضو الفريق المسؤول عن هذا العميل", Required: false),
            new("address", "عنوان العميل", Required: false),
            new("email", "البريد الإلكتروني للعميل", Required: false),
            new("comments_count", "عدد التعليقات على هذا العميل", Required: false),
            new("attachments_count", "عدد المرفقات لهذا العميل", Required: false),
        }),
        new("new_customers_daily", "عدد العملاء الجدد يوميًا، آخر 6 أيام تقريبًا", new List<InteractiveColumnContract>
        {
            new("date", "تاريخ اليوم"),
            new("total", "إجمالي العملاء الجدد في هذا اليوم"),
            new("referrals", "عدد العملاء الجدد عبر الإحالة في هذا اليوم", Required: false),
        }),
        new("pipeline_stats", "إحصاءات ملخّصة لقمع المبيعات بالكامل (صف واحد)", new List<InteractiveColumnContract>
        {
            new("success_rate", "نسبة الصفقات الناجحة المئوية"),
            new("tasks_in_progress", "عدد المهام قيد التنفيذ المرتبطة بالعملاء"),
            new("prepayments", "إجمالي الدفعات المقدّمة من العملاء"),
        }),
    };

    public async Task<InteractiveDataResult> BuildAsync(string projectId, CancellationToken ct)
    {
        var missing = new[] { "العملاء (customers)", "مراحل البيع (stages)", "العملاء الجدد يوميًا (new_customers_daily)", "إحصاءات الصفقات (pipeline_stats)" };
        var matched = await _matcher.MatchAsync(projectId, Contract, ct);
        if (matched is null) return InteractiveDataResult.Unlinked(missing);

        var stagesTable = matched.Tables["stages"];
        var membersTable = matched.Tables["members"];
        var customersTable = matched.Tables["customers"];
        var dailyTable = matched.Tables["new_customers_daily"];
        var statsTable = matched.Tables["pipeline_stats"];

        var stagesSql = BuildSelect(stagesTable, "key", "name", "order", "is_won");
        var membersSql = BuildSelect(membersTable, "id", "name", "role");
        var customersSql = BuildSelect(customersTable, "id", "name", "description", "stage", "due_date", "owner_id", "address", "email", "comments_count", "attachments_count");
        var dailySql = BuildSelect(dailyTable, "date", "total", "referrals");
        var statsSql = BuildSelect(statsTable, "success_rate", "tasks_in_progress", "prepayments");

        var stagesRows = await _matcher.QueryAsync(stagesSql, matched.Context, ct);
        var membersRows = await _matcher.QueryAsync(membersSql, matched.Context, ct);
        var customersRows = await _matcher.QueryAsync(customersSql, matched.Context, ct);
        var dailyRows = await _matcher.QueryAsync(dailySql, matched.Context, ct);
        var statsRows = await _matcher.QueryAsync(statsSql, matched.Context, ct);

        if (stagesRows.Count == 0 || customersRows.Count == 0) return InteractiveDataResult.Unlinked(missing);

        var stages = stagesRows.Select(r => new
        {
            key = Str(r, "key"),
            name = Str(r, "name"),
            isWon = Bool(r, "is_won"),
        }).ToList();

        var members = membersRows.Select(r => new
        {
            id = Str(r, "id"),
            name = Str(r, "name"),
            role = Str(r, "role") ?? "",
        }).ToList();

        var customers = customersRows.Select(r => new
        {
            id = Str(r, "id"),
            name = Str(r, "name") ?? "",
            description = Str(r, "description") ?? "",
            stage = Str(r, "stage") ?? "",
            due = DateStr(r, "due_date"),
            ownerId = Str(r, "owner_id"),
            address = Str(r, "address") ?? "",
            email = Str(r, "email") ?? "",
            commentsCount = Num(r, "comments_count"),
            attachmentsCount = Num(r, "attachments_count"),
        }).ToList();

        var newCustomersDaily = dailyRows.Select(r => new
        {
            label = DateStr(r, "date") ?? Str(r, "date") ?? "",
            total = Num(r, "total"),
            referrals = Num(r, "referrals"),
        }).ToList();

        var first = statsRows.FirstOrDefault();
        var pipelineStats = new
        {
            successRate = first is null ? 0 : Num(first, "success_rate"),
            tasksInProgress = first is null ? 0 : (int)Num(first, "tasks_in_progress"),
            prepayments = first is null ? 0 : Num(first, "prepayments"),
        };

        return InteractiveDataResult.Ready(new { stages, members, customers, newCustomersDaily, pipelineStats });
    }

    /// <summary>Aliases each matched real column back to its logical key (SELECT real_col AS
    /// "logicalKey", ...) so the row dictionaries QueryAsync parses are keyed by the names
    /// Str/Num/Bool/DateStr below actually read — without this, the query still runs and
    /// returns the right row count, but every field reads back null because the JSON keys are
    /// the real column names, not the logical ones.</summary>
    private static string BuildSelect(MatchedInteractiveTable table, params string[] logicalCols)
    {
        var pairs = logicalCols
            .Select(key => (Key: key, Real: table.Column(key)))
            .Where(p => p.Real is not null)
            .ToList();
        return pairs.Count == 0
            ? $"SELECT * FROM {table.RealTable}"
            : $"SELECT {string.Join(", ", pairs.Select(p => $"{p.Real} AS \"{p.Key}\""))} FROM {table.RealTable}";
    }

    private static string? Str(Dictionary<string, JsonElement> row, string col) =>
        row.TryGetValue(col, out var v) && v.ValueKind is JsonValueKind.String ? v.GetString()
        : row.TryGetValue(col, out v) && v.ValueKind is JsonValueKind.Number ? v.ToString()
        : null;

    private static string? DateStr(Dictionary<string, JsonElement> row, string col)
    {
        var s = Str(row, col);
        if (string.IsNullOrWhiteSpace(s)) return null;
        return DateTime.TryParse(s, out var dt) ? dt.ToString("yyyy-MM-dd") : s;
    }

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
}
