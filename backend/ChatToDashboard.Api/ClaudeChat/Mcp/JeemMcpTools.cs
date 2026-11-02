using System.ComponentModel;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using ModelContextProtocol.Server;

namespace ChatToDashboard.Api.ClaudeChat.Mcp;

/// <summary>
/// أدوات القراءة الثلاث لمحادثة Claude عبر MCP: <c>list_tables</c> و<c>describe_table</c> و<c>run_sql</c>.
/// كل أداة تعمل في نطاق جلسة الرسالة الحالية (<see cref="McpSession"/>: المستخدم ومشروعه) ولا ترى غير
/// جداول هذا المشروع والأنظمة المفعَّلة. النتائج JSON مضغوط ليقرأه الموديل، والأخطاء نص يبدأ بـ«خطأ:».
/// </summary>
[McpServerToolType]
public sealed class JeemMcpTools
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    private readonly IHttpContextAccessor _http;
    private readonly ProjectCatalog _catalog;
    private readonly ILogger<JeemMcpTools> _logger;

    public JeemMcpTools(IHttpContextAccessor http, ProjectCatalog catalog, ILogger<JeemMcpTools> logger)
    {
        _http = http;
        _catalog = catalog;
        _logger = logger;
    }

    private McpSession Session => _http.HttpContext.CurrentSession()
        ?? throw new InvalidOperationException("لا توجد جلسة MCP صالحة لهذا الطلب.");

    [McpServerTool(Name = "list_tables", ReadOnly = true, Idempotent = true),
     Description("جداول مشروع المستخدم الحالي (الملفات المرفوعة) وجداول الأنظمة الخارجية المفعَّلة، بأسمائها كما تُكتب في SQL وعدد صفوفها. ابدأ بها دائمًا قبل أي استعلام.")]
    public async Task<string> ListTables(CancellationToken ct)
    {
        var session = Session;
        if (!Budget(session, out var stop)) return stop!;
        var tables = await _catalog.ListAsync(session.ProjectId, ct);
        if (tables.Count == 0)
            return "لا توجد جداول في مشروعك الحالي: ارفع ملفًا من «المصادر» أو فعّل نظامًا خارجيًا.";
        return JsonSerializer.Serialize(new
        {
            project = session.ProjectId,
            tables = tables.Select(t => new { name = t.Name, kind = t.Kind, source = t.Source, rows = t.Rows }),
            hint = "اكتب اسم الجدول في SQL كما هو في name (بمخططه)، مثل: SELECT TOP (10) * FROM " + tables[0].Name,
        }, Json);
    }

    [McpServerTool(Name = "describe_table", ReadOnly = true, Idempotent = true),
     Description("أعمدة جدول واحد وأنواعها، ومعنى كل عمود إن كان معروفًا (مثل revenue أو deal_status)، وعينة 5 صفوف. يقبل اسمًا من نتيجة list_tables فقط.")]
    public async Task<string> DescribeTable(
        [Description("اسم الجدول كما ظهر في list_tables (مثل proj_ab12.Sales)")] string table, CancellationToken ct)
    {
        var session = Session;
        if (!Budget(session, out var stop)) return stop!;
        var allowed = await _catalog.ListAsync(session.ProjectId, ct);
        var match = ProjectCatalog.Match(allowed, table);
        if (match is null) return $"خطأ: الجدول «{table}» ليس من جداول مشروعك. المتاح: {string.Join("، ", allowed.Select(t => t.Name))}";
        try
        {
            var (columns, sampleCols, sample) = await _catalog.DescribeAsync(match, session.ProjectId, 5, ct);
            return JsonSerializer.Serialize(new
            {
                table = match.Name, kind = match.Kind, source = match.Source, rows = match.Rows,
                columns = columns.Select(c => new { name = c.Name, type = c.Type, nullable = c.Nullable, concept = c.Concept, meaning = c.ConceptLabel, note = c.Note }),
                sample = new { columns = sampleCols, rows = sample },
            }, Json);
        }
        catch (SqlException ex) { return "خطأ من قاعدة البيانات: " + ex.Message; }
    }

    [McpServerTool(Name = "run_sql", ReadOnly = true, Idempotent = true),
     Description("ينفّذ استعلام T-SQL للقراءة فقط (SELECT أو WITH، جملة واحدة) على جداول مشروع المستخدم الحالي ويعيد {columns, rows, truncated}. أقصى عدد صفوف 500 (يُفرض TOP تلقائيًا). ممنوع أي كتابة أو جداول خارج المشروع.")]
    public async Task<string> RunSql(
        [Description("استعلام SELECT واحد بأسماء الجداول كما في list_tables")] string sql, CancellationToken ct)
    {
        var session = Session;
        if (!Budget(session, out var stop)) return stop!;
        var allowed = await _catalog.ListAsync(session.ProjectId, ct);
        var validation = ReadOnlySqlValidator.Validate(sql, allowed.Select(t => t.Name).ToList(), _catalog.MaxRows);
        if (!validation.Ok) return "خطأ: " + validation.Error;
        try
        {
            var (columns, rows, truncated) = await _catalog.RunAsync(validation.Sql, ct);
            return JsonSerializer.Serialize(new { columns, rows, rowCount = rows.Count, truncated, maxRows = _catalog.MaxRows }, Json);
        }
        catch (SqlException ex)
        {
            _logger.LogInformation("run_sql رُفض من SQL Server: {Message}", ex.Message);
            return "خطأ من قاعدة البيانات: " + ex.Message + " — راجع الأعمدة بـ describe_table.";
        }
    }

    /// <summary>سقف الاستعلامات لكل رسالة: بعده يُطلب من الموديل التوقف والإجابة بما لديه.</summary>
    private bool Budget(McpSession session, out string? stop)
    {
        var n = session.CountQuery();
        var max = _catalog.MaxQueriesPerMessage;
        stop = n > max ? $"تجاوزت الحد الأقصى للاستعلامات في هذه الرسالة ({max}). توقف عن الاستعلام وأجب بما لديك الآن." : null;
        return stop is null;
    }
}
