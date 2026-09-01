using System.Data.Common;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using ChatToDashboard.Api.Data;
using ChatToDashboard.Api.Models;
using ChatToDashboard.Api.Repository;
using ChatToDashboard.Api.Sources;
using Dapper;
using Microsoft.Extensions.Options;

namespace ChatToDashboard.Api.Llm;

/// <summary>A tool in provider-neutral form; each LLM client maps it to its own wire format.</summary>
public record ToolSpec(string Name, string Description, JsonObject InputSchema);

/// <summary>
/// Everything about the dashboard agent that does not depend on which LLM provider is used:
/// the tool catalogue, the tool implementations, read-only SQL validation, the system prompt,
/// and parsing/validating the final dashboard JSON.
/// </summary>
public class AnalyticsTools
{
    public const int MaxRowsReturned = 500;

    private static readonly Regex ForbiddenSqlKeywords = new(
        @"\b(INSERT|UPDATE|DELETE|DROP|ALTER|CREATE|TRUNCATE|MERGE|EXEC|EXECUTE|GRANT|REVOKE|BACKUP|RESTORE|USE|KILL|SHUTDOWN|PRAGMA|ATTACH|DETACH|VACUUM|REINDEX|REPLACE)\b|(\bsp_\w+)|(\bxp_\w+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly DataFolderLoader _loader;
    private readonly DataStore _db;
    private readonly DocumentSearchService _documents;
    private readonly RepositoryStore _repository;
    private readonly SourceOptions _sources;
    private readonly ILogger<AnalyticsTools> _logger;

    public AnalyticsTools(
        DataFolderLoader loader,
        DataStore db,
        DocumentSearchService documents,
        RepositoryStore repository,
        IOptions<SourceOptions> sources,
        ILogger<AnalyticsTools> logger)
    {
        _loader = loader;
        _db = db;
        _documents = documents;
        _repository = repository;
        _sources = sources.Value;
        _logger = logger;
    }

    /// <summary>
    /// Which sources are on/off for this question, resolved against the configured systems
    /// and the categories that actually exist in the repository.
    /// </summary>
    public record SourceContext(
        IReadOnlyList<string> EnabledSystems,
        IReadOnlyList<string> DisabledSystems,
        IReadOnlyList<string> UnconnectedSystems,
        IReadOnlyList<string> EnabledCategories,
        IReadOnlyList<string> DisabledCategories,
        IReadOnlyDictionary<string, string> TableCategories,
        bool HasDocuments);

    public async Task<SourceContext> DescribeSourcesAsync(
        SourceSelection selection, CancellationToken ct = default)
    {
        var enabledSystems = new List<string>();
        var disabledSystems = new List<string>();
        var unconnected = new List<string>();
        foreach (var system in _sources.Systems)
        {
            if (!selection.AllowsSystem(system.Id)) { disabledSystems.Add(system.Name); continue; }
            enabledSystems.Add(system.Name);
            if (!system.IsConnected) unconnected.Add(system.Name);
        }

        var files = await _repository.ListAsync(ct);
        var enabledCategories = new List<string>();
        var disabledCategories = new List<string>();
        foreach (var category in files.Select(f => f.Category).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (selection.AllowsCategory(category)) enabledCategories.Add(category);
            else disabledCategories.Add(category);
        }

        // Table -> category, so a query against a switched-off category can be refused by name.
        var tableCategories = files
            .Where(f => !string.IsNullOrWhiteSpace(f.TableName))
            .GroupBy(f => f.TableName!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Category, StringComparer.OrdinalIgnoreCase);

        var hasDocuments = files.Any(f => f.Kind == "pdf" && selection.AllowsCategory(f.Category));

        return new SourceContext(
            enabledSystems, disabledSystems, unconnected,
            enabledCategories, disabledCategories, tableCategories, hasDocuments);
    }

    public IReadOnlyList<ToolSpec> BuildTools(SourceContext context)
    {
        var rowCap = _db.Provider == DbProvider.Sqlite ? "LIMIT 500" : "TOP 500";

        var tools = new List<ToolSpec>
        {
            new(
                "list_files",
                "Lists the available data tables with their column names and data types. " +
                "Call this first to see what data exists.",
                new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject(),
                    ["required"] = new JsonArray(),
                }),
            new(
                "query_data",
                $"Runs a read-only SELECT query ({_db.DialectName} dialect) against the loaded data tables " +
                $"and returns the rows as JSON. Only SELECT statements are allowed; results are capped at " +
                $"{MaxRowsReturned} rows, so always use {rowCap} (or less). Reference tables as {_db.TableNamingHint}.",
                new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["sql"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = $"A single SELECT statement ({_db.DialectName} dialect).",
                        },
                    },
                    ["required"] = new JsonArray { "sql" },
                }),
        };

        if (_documents.Enabled || context.HasDocuments)
        {
            tools.Add(new ToolSpec(
                "search_documents",
                "Searches the text of PDF/DOCX documents (uploaded repository files and the data " +
                "folder) and returns the most relevant passages, with the file name and category.",
                new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["query"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "Natural-language search query.",
                        },
                    },
                    ["required"] = new JsonArray { "query" },
                }));
        }

        return tools;
    }

    private static string Bullets(IReadOnlyList<string> items) =>
        items.Count == 0 ? "(لا يوجد)" : string.Join("، ", items);

    // $$ delimiters: {{expr}} interpolates, single braces stay literal for the JSON schema below.
    public string BuildSystemPrompt(SourceContext context) =>
        $$"""
        أنت مساعد تحليلات يجيب على الأسئلة عن بيانات المؤسسة ويرد بمواصفات لوحة معلومات بصيغة JSON.
        اكتب كل النصوص الظاهرة للمستخدم (summary و title و source) باللغة العربية.

        خطوات العمل
        1. نادِ list_files لمعرفة الجداول والأعمدة المتاحة (إلا لو كنت عارفها من نفس المحادثة).
        2. نادِ query_data باستعلامات SELECT للحصول على الأرقام. فضّل الاستعلامات المجمّعة
           (GROUP BY) اللي بترجع بيانات جاهزة للرسم على جلب صفوف خام.
        3. لما تجمع البيانات، رد بالـ JSON النهائي فقط.

        المصادر المفعّلة حاليًا
        - أنظمة مفعّلة: {{Bullets(context.EnabledSystems)}}
        - أنظمة مقفولة: {{Bullets(context.DisabledSystems)}}
        - أنظمة مفعّلة لكن لسه غير مربوطة بقاعدة بيانات (مفيش داتا منها): {{Bullets(context.UnconnectedSystems)}}
        - تصنيفات مستودع الملفات المفعّلة: {{Bullets(context.EnabledCategories)}}
        - تصنيفات مستودع الملفات المقفولة: {{Bullets(context.DisabledCategories)}}

        قواعد المصادر — مهمة جدًا
        - لو السؤال محتاج مصدر مقفول، ما تحاولش تخمّن ولا تجاوب من مصدر تاني. رد باعتذار مهذب
          واذكر اسم المصدر أو التصنيف المقفول بالظبط وقول للمستخدم يفعّله من قائمة "المصادر" فوق.
        - لو المصدر مفعّل لكنه غير مربوط بعد، قول إن النظام ده لسه مش موصّل وإن مفيش بيانات منه.
        - في الحالتين دول رجّع JSON صحيح فيه summary يشرح المطلوب، و widgets تبقى مصفوفة فاضية [].
        - ما تخترعش أرقام أبدًا. كل رقم لازم يكون جاي من نتيجة query_data أو search_documents فعلية.

        {{_db.DialectPrompt}}

        صيغة الرد النهائي
        رسالتك الأخيرة لازم تكون كائن JSON واحد فقط — من غير أسوار كود ولا أي كلام قبله أو بعده —
        مطابق للمخطط ده:
        {
          "summary": "إجابة من سطر أو اثنين على السؤال",
          "widgets": [
            {
              "type": "kpi | bar | line | pie | table",
              "title": "عنوان العنصر",
              "data": [ ... ],
              "xKey": "اختياري، لـ bar/line: اسم حقل التصنيف",
              "yKey": "اختياري، لـ bar/line: اسم الحقل الرقمي",
              "source": "جملتان بالعربي: الأولى مصدر البيانات، والثانية طريقة الحساب."
            }
          ]
        }

        حقل source إجباري في كل عنصر، ولازم يكون جملتين بالظبط تفصل بينهما نقطة ومسافة:
        - الجملة الأولى: منين جت البيانات — اسم الجدول أو الملف والتصنيف أو النظام.
        - الجملة الثانية: إزاي اتحسبت — العملية الفعلية اللي عملتها.
        مثال: "من جدول staging_sample_sales في مستودع الملفات (تصنيف المبيعات). تم تجميع
        SUM(Revenue) وتقسيمها حسب Region مع ترتيب تنازلي."
        الكلام ده لازم يكون مطابق للأدوات اللي ناديتها فعلًا — ممنوع تأليف مصدر أو طريقة حساب.

        اصطلاحات بيانات العناصر
        - kpi: data عبارة عن [{"label": "...", "value": <رقم أو نص>}] (عنصر واحد).
        - bar/line: data مصفوفة كائنات؛ حدد xKey لحقل التصنيف/الزمن وyKey للحقل الرقمي
          (مثال [{"month": "2024-01", "revenue": 1234.5}] مع xKey "month" وyKey "revenue").
        - pie: data عبارة عن [{"label": "...", "value": <رقم>}, ...] (٦ شرائح كحد أقصى، والباقي في "أخرى").
        - table: data مصفوفة صفوف؛ المفاتيح بتبقى عناوين الأعمدة.
        خلي الأرقام أرقام JSON مش نصوص.

        اختيار العناصر
        - للسؤال العادي: اختر ٢ إلى ٤ عناصر تجاوب عليه، وابدأ بـ kpi لو فيه رقم واحد رئيسي.
        - لو المستخدم طلب "تقرير شامل" أو "تقرير كامل" أو نظرة عامة على الأداء: ابنِ لوحة غنية
          من ٥ إلى ٨ عناصر تغطي كل المصادر المفعّلة — عدة مؤشرات kpi، ورسمين أو تلاتة مختلفين
          (bar وline وpie)، وجدول تفصيلي في الآخر.
        """;

    public async Task<(string Result, bool IsError)> ExecuteToolAsync(
        string toolName, JsonObject input, SourceContext context, CancellationToken ct)
    {
        try
        {
            switch (toolName)
            {
                case "list_files":
                {
                    var schema = await _loader.GetSchemaAsync(ct);
                    // Hide tables belonging to a switched-off repository category, and label
                    // the rest so the model can attribute each number to a real source.
                    var visible = schema
                        .Where(t => !context.TableCategories.TryGetValue(t.Table, out var category)
                                    || context.EnabledCategories.Contains(category, StringComparer.OrdinalIgnoreCase))
                        .Select(t => new
                        {
                            table = t.Table,
                            category = context.TableCategories.TryGetValue(t.Table, out var c) ? c : null,
                            columns = t.Columns,
                        });
                    return (JsonSerializer.Serialize(new
                    {
                        tables = visible,
                        disabledCategories = context.DisabledCategories,
                        disabledSystems = context.DisabledSystems,
                    }), false);
                }
                case "query_data":
                {
                    var sql = input["sql"]?.GetValue<string>();
                    if (string.IsNullOrWhiteSpace(sql))
                        return ("Error: 'sql' input is required.", true);

                    var blocked = context.TableCategories
                        .Where(kv => !context.EnabledCategories.Contains(kv.Value, StringComparer.OrdinalIgnoreCase))
                        .FirstOrDefault(kv => sql.Contains(kv.Key, StringComparison.OrdinalIgnoreCase)
                            || sql.Contains(kv.Key.Split('.').Last(), StringComparison.OrdinalIgnoreCase));
                    if (blocked.Key is not null)
                        return ($"الاستعلام مرفوض: الجدول {blocked.Key} تابع لتصنيف \"{blocked.Value}\" " +
                                "وهو غير مفعّل حاليًا. اطلب من المستخدم تفعيله من قائمة المصادر.", true);

                    return await ExecuteQueryAsync(sql, ct);
                }
                case "search_documents":
                {
                    var query = input["query"]?.GetValue<string>() ?? string.Empty;
                    var folderHits = _documents.Enabled
                        ? _documents.Search(query).Select(h => new
                        {
                            file = h.SourceFile, category = (string?)null, score = h.Score, text = h.Text,
                        })
                        : Enumerable.Empty<object>().Select(_ => new
                        {
                            file = string.Empty, category = (string?)null, score = 0d, text = string.Empty,
                        });

                    var documents = await _repository.GetTextDocumentsAsync(ct);
                    var repositoryHits = documents
                        .Where(d => context.EnabledCategories.Contains(d.Category, StringComparer.OrdinalIgnoreCase))
                        .Select(d => new
                        {
                            file = d.Name,
                            category = (string?)d.Category,
                            score = ScoreText(query, d.Text),
                            text = Excerpt(query, d.Text),
                        })
                        .Where(h => h.score > 0);

                    var hits = folderHits.Concat(repositoryHits)
                        .OrderByDescending(h => h.score).Take(5).ToList();
                    return (JsonSerializer.Serialize(hits), false);
                }
                default:
                    return ($"Error: unknown tool '{toolName}'.", true);
            }
        }
        catch (DbException ex)
        {
            // Feed SQL errors back so the model can correct its query.
            _logger.LogWarning(ex, "SQL error executing tool {Tool}", toolName);
            return ($"{_db.DialectName} error: {ex.Message}", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing tool {Tool}", toolName);
            return ($"Error: {ex.Message}", true);
        }
    }

    private static HashSet<string> Terms(string text) =>
        Regex.Matches(text.ToLowerInvariant(), @"[\p{L}\p{Nd}]{2,}").Select(m => m.Value).ToHashSet();

    /// <summary>Share of the query's terms that appear in the document.</summary>
    private static double ScoreText(string query, string text)
    {
        var queryTerms = Terms(query);
        if (queryTerms.Count == 0) return 0;
        var textTerms = Terms(text);
        return (double)queryTerms.Count(textTerms.Contains) / queryTerms.Count;
    }

    /// <summary>A window of the document around the first matching term.</summary>
    private static string Excerpt(string query, string text, int window = 600)
    {
        var term = Terms(query).FirstOrDefault(t => text.Contains(t, StringComparison.OrdinalIgnoreCase));
        var index = term is null ? 0 : Math.Max(0, text.IndexOf(term, StringComparison.OrdinalIgnoreCase) - window / 4);
        return text.Substring(index, Math.Min(window, text.Length - index));
    }

    private async Task<(string Result, bool IsError)> ExecuteQueryAsync(string sql, CancellationToken ct)
    {
        var validationError = ValidateReadOnlySql(sql);
        if (validationError is not null)
            return ($"Query rejected: {validationError}", true);

        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = new List<Dictionary<string, object?>>();

        await using var reader = await connection.ExecuteReaderAsync(
            new CommandDefinition(sql, commandTimeout: 30, cancellationToken: ct));
        while (rows.Count < MaxRowsReturned && await reader.ReadAsync(ct))
        {
            var row = new Dictionary<string, object?>();
            for (var i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            rows.Add(row);
        }

        return (JsonSerializer.Serialize(new { rowCount = rows.Count, rows }), false);
    }

    /// <summary>
    /// Parses and validates the final dashboard JSON. Tolerates a markdown code fence
    /// or stray prose around the object, but the JSON itself must match the schema.
    /// </summary>
    public static (DashboardSpec? Dashboard, string? Error) TryParseDashboard(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (null, "the response contained no text.");

        var candidate = text.Trim();
        var fence = Regex.Match(candidate, @"```(?:json)?\s*(\{.*\})\s*```", RegexOptions.Singleline);
        if (fence.Success)
            candidate = fence.Groups[1].Value;
        else
        {
            var start = candidate.IndexOf('{');
            var end = candidate.LastIndexOf('}');
            if (start < 0 || end <= start)
                return (null, "no JSON object found in the response.");
            candidate = candidate[start..(end + 1)];
        }

        DashboardSpec? spec;
        try
        {
            spec = JsonSerializer.Deserialize<DashboardSpec>(candidate);
        }
        catch (JsonException ex)
        {
            return (null, $"JSON deserialization failed: {ex.Message}");
        }

        if (spec is null)
            return (null, "JSON deserialized to null.");

        var validationErrors = spec.Validate();
        if (validationErrors.Count > 0)
            return (null, string.Join(" ", validationErrors));

        return (spec, null);
    }

    /// <summary>Returns an error message if the SQL is not a single read-only SELECT, else null.</summary>
    public static string? ValidateReadOnlySql(string sql)
    {
        // Strip comments so keywords can't hide in or behind them.
        var stripped = Regex.Replace(sql, @"--[^\n]*|/\*.*?\*/", " ", RegexOptions.Singleline).Trim();

        if (stripped.Length == 0)
            return "empty statement.";

        if (!Regex.IsMatch(stripped, @"^(SELECT|WITH)\b", RegexOptions.IgnoreCase))
            return "only SELECT statements (optionally starting with a WITH clause) are allowed.";

        // Reject multiple statements: a semicolon may only appear at the very end.
        var withoutTrailingSemicolons = stripped.TrimEnd(';', ' ', '\t', '\r', '\n');
        if (withoutTrailingSemicolons.Contains(';'))
            return "multiple statements are not allowed.";

        var forbidden = ForbiddenSqlKeywords.Match(stripped);
        if (forbidden.Success)
            return $"forbidden keyword '{forbidden.Value.ToUpperInvariant()}' — the query must be read-only.";

        return null;
    }
}
