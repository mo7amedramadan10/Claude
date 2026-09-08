using System.Data.Common;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using ChatToDashboard.Api.Data;
using ChatToDashboard.Api.Models;
using ChatToDashboard.Api.Repository;
using ChatToDashboard.Api.Sources;
using ChatToDashboard.Api.Widgets;
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

    // A recurring mistake (seen with Ollama's smaller models, but not exclusive to it): writing
    // 'column_name' with single quotes — a SQL *string literal* — instead of "column_name"
    // with double quotes (SQLite's identifier syntax), in GROUP BY and/or in a SELECT-list
    // alias. A query still runs and "succeeds" either way, but every row evaluates the literal
    // identically, so GROUP BY 'x' silently collapses into a single group instead of one per
    // real value, and SELECT 'x' AS label shows the literal text "x" instead of each row's
    // actual value. Without a hint the model has no signal anything is wrong beyond "the
    // numbers look off", and — observed live — can spend a dozen tool calls guessing at
    // WHERE-clause exclusions that can never fix a target that was never a column reference to
    // begin with. Flagging it in the tool result itself lets the model self-correct in one turn.
    // Captures either the single-quoted literal (group 1) or the double-quoted column (group 2)
    // GROUP BY resolves to, so the SELECT-list check below knows which name to look for.
    private static readonly Regex GroupByTarget = new(
        @"GROUP\s+BY\s+(?:'([^']*)'|""([^""]*)"")", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Every tool result below is serialized here, then embedded as a plain string value inside
    // a *second* JSON document (the provider request body — see ClaudeClient/OpenAiClient).
    // The default encoder hex-escapes non-ASCII text into a six-character ASCII sequence
    // ("\uXXXX" as literal text, not a real code point yet) — so the second, legitimate
    // serialization pass escapes THOSE literal backslashes too, and the model ends up reading
    // undecoded "\uXXXX\uXXXX..." text instead of the Arabic word it names. Never double-
    // escaping here (this relaxed encoder leaves non-ASCII as real UTF-8) is what lets an
    // Arabic table/column name — or any other tool result text — reach the model as itself.
    private static readonly JsonSerializerOptions ToolResultJsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>Stand-in for a widget's omitted "data" (see TryParseDashboard) — a real,
    /// serializable JsonElement, never the default Undefined one JsonSerializer can't write.</summary>
    private static readonly JsonElement EmptyArrayElement = JsonDocument.Parse("[]").RootElement;

    private readonly DataFolderLoader _loader;
    private readonly DataStore _db;
    private readonly DocumentSearchService _documents;
    private readonly RepositoryStore _repository;
    private readonly SystemApiLoader _systemLoader;
    private readonly SourceOptions _sources;
    private readonly ILogger<AnalyticsTools> _logger;

    public AnalyticsTools(
        DataFolderLoader loader,
        DataStore db,
        DocumentSearchService documents,
        RepositoryStore repository,
        SystemApiLoader systemLoader,
        IOptions<SourceOptions> sources,
        ILogger<AnalyticsTools> logger)
    {
        _loader = loader;
        _db = db;
        _documents = documents;
        _repository = repository;
        _systemLoader = systemLoader;
        _sources = sources.Value;
        _logger = logger;
    }

    /// <summary>
    /// Which sources are on/off for this question, resolved against the configured systems
    /// and every file actually in the repository — each file is its own independent
    /// source, at the same level as a system, not grouped by category.
    /// </summary>
    public record SourceContext(
        IReadOnlyList<string> EnabledSystems,
        IReadOnlyList<string> DisabledSystems,
        IReadOnlyList<string> UnconnectedSystems,
        // Display names, for the "المصادر المفعّلة حاليًا" prompt bullets.
        IReadOnlyList<string> EnabledFiles,
        IReadOnlyList<string> DisabledFiles,
        // Repository file ids currently enabled — every file with an Id present here has
        // passed the on/off toggle (independent of RestrictedFileTables below).
        IReadOnlySet<string> EnabledFileIds,
        // Table -> owning file's display name, for every repository file that has a table
        // (all of them, enabled or not) — used to label list_files' "tables" entries and to
        // name the file in a refusal message.
        IReadOnlyDictionary<string, string> TableFiles,
        IReadOnlyDictionary<string, string> TableSystems,
        IReadOnlyDictionary<string, string> DisabledSystemTables,
        // Table -> owning file's display name, DISABLED files only — the query-blocking
        // gate CheckSourcePermission scans, mirroring DisabledSystemTables.
        IReadOnlyDictionary<string, string> DisabledFileTables,
        // Table -> the file's display name, for a repository file the current user is
        // neither the creator of nor explicitly granted onto — every file with a recorded
        // creator is auto-restricted to it (see RepositoryFile.PermittedUserIds), so this
        // covers essentially all of them for a non-Admin except their own uploads and
        // whatever's been explicitly shared with them. An Admin never appears here. Separate
        // from DisabledFileTables since the reason is per-user identity, not the on/off
        // toggle every file also carries.
        IReadOnlyDictionary<string, string> RestrictedFileTables,
        bool HasDocuments);

    public async Task<SourceContext> DescribeSourcesAsync(
        SourceSelection selection, CancellationToken ct = default)
    {
        var enabledSystems = new List<string>();
        var disabledSystems = new List<string>();
        var unconnected = new List<string>();
        var systemTables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var disabledSystemTables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var system in _sources.Systems)
        {
            // A system with an endpoint owns the staging table its records were loaded into.
            var table = system.HasApi ? _systemLoader.TableFor(system) : null;
            if (table is not null) systemTables[table] = system.Name;

            if (!selection.AllowsSystem(system.Id))
            {
                disabledSystems.Add(system.Name);
                if (table is not null) disabledSystemTables[table] = system.Name;
                continue;
            }
            enabledSystems.Add(system.Name);
            if (!system.IsConnected) unconnected.Add(system.Name);
        }

        var files = await _repository.ListAsync(ct);
        var enabledFiles = new List<string>();
        var disabledFiles = new List<string>();
        var enabledFileIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var tableFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var disabledFileTables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in files)
        {
            if (!string.IsNullOrWhiteSpace(f.TableName)) tableFiles[f.TableName!] = f.DisplayName;

            if (selection.AllowsFile(f.Id))
            {
                enabledFiles.Add(f.DisplayName);
                enabledFileIds.Add(f.Id);
            }
            else
            {
                disabledFiles.Add(f.DisplayName);
                if (!string.IsNullOrWhiteSpace(f.TableName)) disabledFileTables[f.TableName!] = f.DisplayName;
            }
        }

        // Every file is automatically restricted to its own creator — not opt-in, and not
        // removable (see RepositoryFile.PermittedUserIds) — with PermittedUserIds as the
        // *additional* names its creator (or an Admin) has explicitly granted on top of
        // that. So unlike a category/system toggle, this check applies to every file with a
        // table, not just ones someone has configured a list on.
        var restrictedFileTables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!selection.IsAdmin)
        {
            // A file uploaded before CreatedByUserId existed has no recorded creator — there's
            // no one it could be auto-restricted "to", so it stays open (old behavior) rather
            // than becoming inaccessible to everyone but Admins with no way to grant it back.
            foreach (var f in files.Where(f => !string.IsNullOrWhiteSpace(f.TableName) && !string.IsNullOrWhiteSpace(f.CreatedByUserId)))
            {
                var allowed = selection.UserId is not null
                    && (string.Equals(selection.UserId, f.CreatedByUserId, StringComparison.OrdinalIgnoreCase)
                        || f.PermittedUserIds.Contains(selection.UserId, StringComparer.OrdinalIgnoreCase));
                if (!allowed) restrictedFileTables[f.TableName!] = f.DisplayName;
            }
        }

        var hasDocuments = files.Any(f => f.Kind == "pdf" && selection.AllowsFile(f.Id));

        return new SourceContext(
            enabledSystems, disabledSystems, unconnected,
            enabledFiles, disabledFiles, enabledFileIds, tableFiles,
            systemTables, disabledSystemTables, disabledFileTables, restrictedFileTables, hasDocuments);
    }

    public IReadOnlyList<ToolSpec> BuildTools(SourceContext context)
    {
        var rowCap = _db.Provider == DbProvider.Sqlite ? "LIMIT 500" : "TOP 500";

        var tools = new List<ToolSpec>
        {
            new(
                "list_files",
                "Lists what data exists: the queryable tables with their columns and types, and the " +
                "files saved in the file repository (name, category, type, row or page count, upload " +
                "date). Call this first. Note PDF files have no queryable table — their metadata is " +
                "listed here and their text is searched with search_documents.",
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
            new(
                "forecast_data",
                $"Runs a read-only SELECT query ({_db.DialectName} dialect) that returns exactly two columns " +
                "— a chronological period label, then a numeric value, ordered oldest-to-newest — and computes " +
                "a REAL statistical forecast (ordinary least-squares linear regression, with an additive " +
                "seasonal adjustment when the series covers at least two full seasonal cycles) for the next " +
                "periodsAhead periods, including a ~95% prediction interval. Use this — never your own " +
                "estimate — whenever the user asks to forecast/predict/project a future value. The forecasted " +
                "numbers this returns are the only ones you may put in a widget's \"forecast\" field; never " +
                "invent a projected number yourself, and never put a forecasted value in \"data\" as if it " +
                "were an observed one.",
                new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["sql"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = $"A single SELECT statement ({_db.DialectName} dialect) " +
                                "returning exactly two columns — period label, then numeric value — ordered " +
                                "chronologically ascending (oldest first).",
                        },
                        ["periodsAhead"] = new JsonObject
                        {
                            ["type"] = "integer",
                            ["description"] = "How many future periods to forecast (1-12).",
                        },
                        ["seasonLength"] = new JsonObject
                        {
                            ["type"] = "integer",
                            ["description"] = "Optional: periods per seasonal cycle (e.g. 12 for monthly data " +
                                "with yearly seasonality) — only worth setting when the query returned at " +
                                "least two full cycles of history. Omit otherwise.",
                        },
                    },
                    ["required"] = new JsonArray { "sql", "periodsAhead" },
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

    /// <summary>
    /// Frames the dashboard currently on screen (if any) as explicit context ahead of the
    /// user's new question, so a follow-up like "اعرضلي بس القطاع الرقمي" can refine or extend
    /// it. This is the *only* continuation signal the model gets — whether <paramref
    /// name="currentDashboard"/> is present or null is decided entirely by the frontend (the
    /// "🆕 ابدأ لوحة جديدة" button, or the first question of a session), never guessed from the
    /// question's wording. See BuildSystemPrompt's "حالة اللوحة الحالية" section for the
    /// matching instructions on how to use this.
    /// </summary>
    public static string ComposeUserMessage(string question, DashboardStateInput? currentDashboard)
    {
        if (currentDashboard is null || currentDashboard.Widgets.Count == 0) return question;

        // Same relaxed encoder as every tool result (see ToolResultJsonOptions above) — this
        // widgets JSON can carry Arabic titles/sources just like a query result does, and the
        // default encoder's double-escaping bug applies here exactly the same way.
        var widgetsJson = JsonSerializer.Serialize(currentDashboard.Widgets, ToolResultJsonOptions);
        return $$"""
            اللوحة المعروضة حاليًا للمستخدم:
            الملخص السابق: {{currentDashboard.Summary}}
            العناصر الحالية (JSON كامل، شامل حقل source لكل عنصر):
            {{widgetsJson}}

            سؤال المستخدم الجديد:
            {{question}}
            """;
    }

    /// <summary>
    /// "🔄 حوّله لداشبورد" — frames an already-answered Inquiries response as a pure
    /// restructuring task: the real data is handed over verbatim, no tool call is offered
    /// (see IDashboardGenerator.GenerateDashboardFromInquiryAsync), and the model's only job
    /// is to shape it into the normal widgets JSON — never to re-derive or second-guess the
    /// numbers themselves.
    /// </summary>
    public static string ComposeConversionUserMessage(InquiryResponse inquiry)
    {
        var dataJson = JsonSerializer.Serialize(inquiry.Data, ToolResultJsonOptions);
        return $$"""
            مهمة إعادة هيكلة فقط — من غير أي نداء أداة (list_files أو query_data أو غيرها):
            الأرقام دي جاية فعليًا من استعلام سابق نُفِّذ بالفعل، ومهمتك دلوقتي إنك تحوّلها لعنصر
            أو أكتر (widgets) بالشكل المناسب حسب طبيعتها (kpi/bar/line/pie/table أو أحد الأنواع
            الجديدة progress-table/trend-matrix/status-bar لو مناسب فعلًا)، من غير ما تخترع أي
            رقم إضافي أو تحسب حاجة مختلفة عن اللي جاي تحت.

            الإجابة النصية الأصلية (استرشد بيها لصياغة summary وnarration، من غير نسخ حرفي بالضرورة):
            {{inquiry.Answer}}

            مصدر البيانات وطريقة حسابها (استخدمه كما هو في حقل source):
            {{inquiry.Source}}

            البيانات الفعلية اللي استندت عليها الإجابة — استخدمها زي ما هي بالضبط:
            {{dataJson}}

            رجّع كائن JSON واحد فقط مطابق لمخطط اللوحة المعتاد (summary وnarration وwidgets)،
            من غير أي نص خارج الـJSON ولا markdown fence.
            """;
    }

    // $$ delimiters: {{expr}} interpolates, single braces stay literal for the JSON schema below.
    public string BuildSystemPrompt(SourceContext context) =>
        $$"""
        أنت "محلّل بيانات مؤسسي" (Enterprise Analytics Agent) بتجاوب على أسئلة عن بيانات
        المؤسسة وترجع مواصفات لوحة معلومات بصيغة JSON. اكتب كل النصوص الظاهرة للمستخدم
        (summary وtitle وsource وnarration) باللغة العربية، بأسلوب مهني ومباشر وبدون حشو.

        الشرح السردي (narration) — حقل منفصل تمامًا عن summary وعن source
        كل رد لازم يحتوي حقل "narration" بالإضافة لـ summary: لو summary هو جملة أو
        جملتين مختصرتين للعرض السريع، فـ narration نص سردي أغنى وأطول (حوالي ٤ إلى ٨
        جمل حسب عدد العناصر) مخصّص للقراءة أو للاستماع (زر 🔊 بيقرأ narration بالظبط، مش
        summary ولا source). قواعده صارمة ولازم تتبع بالحرف:
        - فصحى فقط، من غير أي عامية أو خلط لهجات — حتى لو صياغة السؤال نفسه عامية. دي
          قاعدة منفصلة عن "اكتب بالعربي" العامة فوق: النموذج بميل افتراضيًا لعربي مبسّط
          أو شبه عامي لو ما تنبّهش لده صراحة.
        - ممنوع منعًا باتًا ذكر أي تفصيلة تقنية أو "مطبخ" البيانات: اسم ملف، اسم جدول،
          اسم تصنيف، كلمة "قاعدة بيانات" أو "استعلام" أو "SQL"، اسم عمود، أو أي وصف
          لإزاي اتجابت الإجابة. كل ده مكانه حصرًا في source بتاع كل عنصر، ومش بيتأثر
          بالحقل ده أبدًا. narration بيتكلم عن معنى البيانات، مش مصدرها.
        - استعرض العناصر (widgets) الموجودة بترتيبها، وخصّص لكل عنصر جملة أو جملتين
          مميّزتين له — نوّع في تركيب الجملة وبداية كل جملة بين عنصر وتاني، ما تكررش
          نفس القالب ("كذا... وكذلك كذا...").
        - إجباري: لازم يظهر عنوان كل عنصر (title) بالحرف الواحد جوه الجملة أو الجمل
          الخاصة بيه — نفس الصياغة بالظبط من غير أي اختصار أو إعادة صياغة أو استبدال
          بمرادف، حتى لو الجملة كانت هتبقى أنسيَب بمرادف أو وصف عام. مش تفصيلة تجميلية:
          الواجهة بتزامن القراءة الصوتية مع إضاءة العنصر نفسه على الشاشة لحظة ما اسمه
          بيتقال، وده بيعتمد كليًا على وجود العنوان الحرفي جوه الجملة — غيابه أو تغييره
          بيكسر هذا التزامن تمامًا حتى لو النص نفسه سليم لغويًا.
        - اربط بين العناصر بروابط سردية طبيعية ("أما فيما يخص..."، "وعلى صعيد آخر..."،
          "كما يُظهر...") بدل ما توصف كل عنصر لوحده بمعزل عن الباقي — الهدف فقرة واحدة
          متماسكة مش قائمة نقاط.
        - الطول: ٤ إلى ٨ جمل تقريبًا حسب عدد العناصر — كفاية عشان يبقى فيه مضمون حقيقي،
          وقصير كفاية عشان يتقرأ أو يتسمع بسرعة. ممنوع تحشو الكلام عشان توصل لطول معيّن.
        - لو فيه حاجة لافتة فعلًا في الأرقام (ارتفاع حاد، تفاوت واضح، تركّز في فئة معينة)
          يصح تنوّه عنها بأسلوب وصفي — بس من غير ما تخترع سبب أو رقم مش موجود أصلًا في
          بيانات العنصر اللي رجعته الأداة.

        ترتيب الأولويات عند أي تعارض بين مطلبين
        1) الدقة والصدق مع البيانات الفعلية — فوق أي حاجة تانية.
        2) الالتزام الصارم بصلاحيات المستخدم على المصادر (قسم "قواعد المصادر" تحت).
        3) وضوح الإجابة واكتمالها بالنسبة للسؤال المطروح فعليًا.
        4) عدد عناصر اللوحة وتنوّعها — الشكل والألوان والتنسيق مش مسؤوليتك أصلًا،
           الواجهة بتتكفل بيها تلقائيًا حسب نوع كل عنصر (type).

        القاعدة الذهبية: ممنوع تختلق رقمًا أو تخمّنه أو "تقرّبه" من غير ما يكون جاي فعليًا
        من نتيجة أداة ناديتها (list_files أو query_data أو search_documents). لو مش
        متاح عندك الرقم، قول كده صراحة في summary بدل ما تخترعه أو تسكت عن غيابه.

        استنتاج المنطق التجاري والمعادلات
        مش لازم المستخدم يحدد كل معادلة بنفسه — لو السؤال يقتضي حساب مؤشر مركّب (نسبة،
        متوسط، معدل نمو) من أعمدة موجودة فعلًا، احسبه بنفسك حسب مستوى الوضوح:
        - مستوى ١ (واضح من كلام المستخدم): نفّذ المعادلة زي ما هي من غير تردد.
        - مستوى ٢ (استنتاج منطقي من الأعمدة المتاحة): لو فيه معادلة شائعة ومنطقية ممكن
          تُشتق من الأعمدة الموجودة (مثلاً "نسبة المنصرف من قيمة العقد" = المنصرف ÷ قيمة
          العقد × ١٠٠، من عمودي DisbursedAmount وContractValue)، نفّذها واذكر المعادلة
          بالظبط في source.
        - مستوى ٣ (غموض جوهري بيأثر على النتيجة): لو فيه أكتر من تعريف منطقي للمقياس
          والفرق بينهم ممكن يغيّر النتيجة بشكل جوهري، اختَر التفسير الأقرب واذكر افتراضك
          بوضوح في summary — ما تعرضش رقم مستنتج على إنه تعريف مؤسسي رسمي إلا لو
          التعريف موجود فعليًا في مستند أو مصدر رجعت له.
        اذكر أي معادلة مستنتجة بوضوح في source (الجملة التانية: طريقة الحساب) — مش بس
        "SUM" أو "AVG"، لازم تفصيل المعادلة الفعلية لو كانت مركّبة.

        الفلاتر (filters) — اقتراح فلاتر مفيدة على مستوى اللوحة
        لكل لوحة فيها أبعاد (dimensions) منطقية للاستكشاف — زي الحالة، الإدارة، المنطقة،
        النوع، الأولوية، السنة — قيّم إمكانية اقتراح فلتر عليها في حقل filters بالرد النهائي:
        - اختَر بس أبعاد ذات معنى تجاري حقيقي (المستخدم فعلًا محتاج يفلتر بيها). تجنّب
          الأعمدة التقنية زي الـ ID أو الـ GUID أو أي عمود عالي التنوّع (قيم كتير جدًا ومالهاش
          معنى كفلتر، زي رقم صف فريد لكل سجل).
        - "options" في كل فلتر لازم تكون قيم حقيقية جبتها فعليًا باستعلام SELECT DISTINCT
          على العمود ده — ممنوع تختلق قيمة زي "قيد المراجعة" لو مش موجودة فعليًا في البيانات.
          لو محتاج تجيب القيم المميزة، نادِ query_data باستعلام DISTINCT منفصل قبل ما ترجع الرد.
        - كل فلتر لازم يحدد "table" بالاسم الفعلي للجدول اللي جاي منه العمود (نفس اسم الجدول
          اللي استخدمته في query_data)، لأن ده اللي بيحدد أنهي عناصر تانية في اللوحة ممكن
          تتأثر بيه لاحقًا.
        - "type": استخدم single_select لو فيه قيمة واحدة تتختار، multi_select لو منطقي
          تختار أكتر من قيمة (زي الحالة)، date_range للفترات الزمنية، numeric_range
          لمقاييس رقمية (زي الميزانية). لـ date_range وnumeric_range سيب options فاضية —
          الواجهة بتعرض حقلين إدخال بدالها.
        - لو مفيش أبعاد مناسبة فعلًا، سيب filters مصفوفة فاضية [] — ممنوع تضيف فلتر شكلي
          من غير قيمة حقيقية وراه.
        - الفلاتر دي اقتراح بس؛ تفعيلها الفعلي بيحصل في الواجهة بعد كده من غير ما تحتاج
          تتدخل إنت تاني — لكن ده يشتغل بس مع العناصر اللي حاطط لها حقل "query" (تحت)،
          فأي عنصر من نفس الجدول اللي حددته "table" في الفلتر لازم يحمل query.table مطابق
          عشان الفلتر يقدر يأثّر فيه فعليًا، وإلا هيظهر للمستخدم كعنصر "غير متأثر بالفلتر".

        التنبؤ (forecast) — لما المستخدم يطلب توقع مستقبلي صريح
        لو السؤال فيه طلب توقع/تنبؤ/إسقاط لقيمة مستقبلية (زي "توقع مبيعات الشهر الجاي" أو
        "إيه المتوقع للربع القادم؟") — ممنوع تمامًا إنك تحسب أو تخمّن الرقم المتوقع بنفسك مهما
        كان بسيطًا. نادِ أداة forecast_data باستعلام SELECT يرجّع عمودين بالظبط (تسمية الفترة،
        ثم القيمة الرقمية) مرتبة زمنيًا تصاعديًا، وحدد periodsAhead (عدد الفترات المطلوب
        توقعها). لو عندك سببين منطقيين إن فيه موسمية سنوية والبيانات بتغطي سنتين كاملتين على
        الأقل بنفس التجميع (شهري مثلًا)، حدد seasonLength (زي ١٢ للبيانات الشهرية).
        - ضع نتيجة forecast_data زي ما هي بالظبط في حقل "forecast" بالعنصر (labels/values/
          lower/upper/method/note) — ممنوع تُدرج أي رقم من الـ forecast جوه "data" العادية،
          وممنوع تُعدّل أو "تظبط" الأرقام اللي رجعتها الأداة.
        - اذكر في summary إن في توقعًا إحصائيًا مبنيًا على البيانات التاريخية (مش رقمًا مؤكدًا)،
          وانقل ملاحظة "note" لو موجودة (زي تحذير قلة البيانات) بمعناها في الكلام.
        - لو المستخدم سأل سؤالًا عاديًا مش فيه طلب توقع صريح، ممنوع تستخدم forecast_data من
          نفسك أو تضيف حقل forecast من غير ما يُطلب — التوقع اختياري بحت.

        منهج داخلي قبل كل رد (ما يظهرش للمستخدم، بس اتبعه فعليًا في كل مرة)
        افهم طبيعة السؤال (جديد؟ تعديل على اللي فات؟ عن المستودع نفسه؟) ← خطّط الاستعلام
        اللي هتحتاجه (إيه الجدول، إيه التجميع، إيه الفلتر الزمني) ← نفّذ الأداة المناسبة
        ← تحقق من معقولية النتيجة (قسم "فحص النتيجة" تحت) ← اكتب الرد النهائي بصيغة JSON
        فقط. لا تشارك خطوات التفكير دي مع المستخدم أبدًا — بس النتيجة النهائية.

        خطوات العمل
        1. نادِ list_files لمعرفة الجداول والأعمدة المتاحة، وكمان قائمة الملفات المحفوظة في
           مستودع الملفات (الاسم والتصنيف والنوع وعدد الصفوف أو الصفحات وتاريخ الرفع).
           لو السؤال عن المستودع نفسه — كام ملف، إيه التصنيفات، أحدث ملف — الإجابة موجودة
           في نتيجة list_files مباشرة من غير ما تحتاج query_data.
        2. نادِ query_data باستعلامات SELECT للحصول على الأرقام. فضّل الاستعلامات المجمّعة
           (GROUP BY) اللي بترجع بيانات جاهزة للرسم على جلب صفوف خام. لو رجعت صفوف فاضية
           أو NULL في مكان متوقع فيه رقم، صرّح بده في summary — ما تعتبروش صفر تلقائيًا
           ولا تتجاهله.
        3. قبل الرد، تأكد إن كل رقم هتحطه فعلًا موجود في نتيجة أداة ناديتها (مش نتيجة
           حسبتها إنت من دماغك من غير استعلام حقيقي). بعدين رد بالـ JSON النهائي فقط —
           من غير أي شرح أو مقدمة برّاه.

        قاعدة "ما تغيّرش القصد من غير ما تقول"
        لو طلب المستخدم غامض أو ينفهم بأكتر من طريقة، اختَر التفسير الأقرب لسياق المحادثة
        والبيانات المتاحة فعليًا، واذكر في summary إيه الافتراض اللي اخترته بجملة واضحة
        (مثلًا: "افترضت إن المقصود بـ'الشهر ده' هو آخر شهر متاح فعليًا في البيانات وهو
        ٢٠٢٤-٠٦"). ممنوع تغيّر نوع اللوحة أو نطاقها الزمني أو مصادرها عن اللي طلبه المستخدم
        من غير ما توضح ده صراحة.

        حالة اللوحة الحالية (متابعة أم بداية جديدة؟ — قرار الواجهة، مش تخمينك)
        القرار ما بين "متابعة لنفس اللوحة" و"بداية جديدة من الصفر" بيوصلك جاهز من الواجهة —
        مش حاجة تستنتجها إنت من صياغة السؤال. العلامة الوحيدة المعتمدة: هل رسالة المستخدم دي
        مسبوقة بقسم عنوانه "اللوحة المعروضة حاليًا للمستخدم" (فيه summary وwidgets كاملة من
        آخر رد فعلي) ولا لأ.
        - لو موجود: الجلسة في وضع "متابعة" — عامل السؤال الجديد كتحسين أو امتداد لهذه اللوحة
          بالذات (تضييق/فلترة نفس البيانات، تعديل فترة زمنية، إضافة عنصر جديد بجانبها، ...)،
          إلا لو كان واضحًا تمامًا (مش مجرد صياغة غامضة) إن السؤال عن موضوع مختلف كليًا لا
          علاقة له باللي معروض — في الحالة دي بس تجاهل اللوحة القديمة وابنِ لوحة جديدة تجاوب
          على السؤال الفعلي، ووضّح في summary إنك بنيت لوحة جديدة لاختلاف الموضوع.
          - إجراء إجباري بالترتيب ده بالظبط، من غير تخطي أي خطوة:
            ١) انسخ كل عنصر موجود في "العناصر الحالية" (الحرفية اللي وصلتك في الرسالة، بكل
               حقوله بالكامل: type وtitle وdata وxKey وyKey وsource وforecast وquery لو
               موجود) زي ما هو — ده نقطة البداية الإجبارية لمصفوفة widgets في ردك، قبل أي
               تعديل. حقل query تحديدًا مهم يفضل موجود من غير تغيير عشان الفلاتر تفضل شغالة
               على العنصر ده حتى بعد المتابعة.
            ٢) عدّل بس العنصر أو العناصر اللي طلب المستخدم تعديلها فعليًا (استبدل بياناتها
               بنتيجة استعلام جديد لو لزم الأمر)، أو احذف بس اللي طلب حذفه صراحة، أو أضف
               عنصرًا جديدًا لو طلب إضافة — فوق النسخة اللي عملتها في الخطوة ١، مش بدلًا
               منها.
            ٣) أي عنصر مالوش علاقة بطلب المستخدم يفضل زي ما هو بالحرف من غير أي تغيير.
          - خطأ شائع وممنوع تمامًا: الرجوع بمصفوفة widgets فيها بس العنصر الجديد أو المعدّل
            وترك باقي العناصر القديمة تختفي من غير ما يُطلب حذفها. مثال: لو المستخدم قال
            "زوّد رسم لكذا" وكان معاك عنصر واحد قبل كده، ردك النهائي لازم يحتوي على عنصرين
            (العنصر القديم + الجديد)، مش عنصر واحد بس. راجع عدد عناصر ردك النهائي مقابل عدد
            "العناصر الحالية" قبل ما تبعت — لو قل من غير ما يُطلب حذف صريح، ده خطأ صححه فورًا.
          - لسه لازم تنادي الأدوات (list_files/query_data/forecast_data) من جديد لأي رقم
            جديد محتاجه — بيانات الرد اللي فات مش نتيجة أداة متاحة لك، بس خليها مرجعك لفهم
            "الأساس" اللي مبني عليه السؤال (نفس الجدول والتجميع) مش استعلام مختلف تمامًا،
            إلا لو التعديل نفسه بيقتضي كده.
          - لو طلب المستخدم "غيّر اللون" أو "كبّر الخط" أو أي تعديل شكلي بحت: وضّح في summary
            إن شكل اللوحة (الألوان والأحجام) بيتحدد تلقائيًا حسب نوع العنصر ومش قابل للتغيير
            من هنا، وركّز بدل كده على أي تعديل حقيقي في المحتوى لو موجود ضمن نفس الرسالة.
        - لو مفيش قسم "اللوحة المعروضة حاليًا للمستخدم" مرفق أصلًا (أول سؤال في الجلسة، أو
          المستخدم ضغط زر "🆕 ابدأ لوحة جديدة"): ابنِ اللوحة من الصفر من غير أي افتراض عن رد
          سابق — الواجهة هي اللي بتضمن إرسال الحالة الحالية بس لما يكون فعلاً متابعة، فمفيش
          داعي تتأكد من ده بنفسك.

        لو المستخدم أرفق صورة لداشبورد أو تقرير (سكرين شوت من نظام تاني، أو رسم توضيحي، أو Mockup)
        - اعتبرها "قالب شكلي" فقط: عدد العناصر، نوع كل رسم (kpi/bar/line/pie/table)، وعنوانه تقريبًا.
        - ممنوع تقرأ أي رقم من الصورة نفسها أو تخمّنه. كل رقم في ردك لازم يكون حقيقي جاي من
          query_data أو list_files فعلًا، بنفس القواعد بالظبط كأي سؤال عادي.
        - ابنِ نفس عدد ونوع العناصر اللي في الصورة قدر الإمكان، وحاول تطابق نفس الترتيب والتجميع
          العام، باستخدام أقرب بيانات حقيقية متاحة لكل عنصر.
        - لو عنصر في الصورة مفيش له بيانات حقيقية مناسبة، استبدله بعنصر تاني من بيانات فعلية
          واذكر ده بوضوح في summary بدل ما تسكت عنه.
        - اذكر في summary إن اللوحة اتبنت استرشادًا بالصورة المرفقة، وإن الألوان والتنسيق في
          الصورة الأصلية اتجوهت عمدًا لأن شكل اللوحة عندك بيتحدد تلقائيًا مش من الصورة.

        المصادر المفعّلة حاليًا
        - أنظمة مفعّلة: {{Bullets(context.EnabledSystems)}}
        - أنظمة مقفولة (لا يحق للمستخدم الوصول لها): {{Bullets(context.DisabledSystems)}}
        - أنظمة مفعّلة لكن لسه غير مربوطة بقاعدة بيانات (مفيش داتا منها بعد): {{Bullets(context.UnconnectedSystems)}}
        - ملفات مستودع الملفات المفعّلة: {{Bullets(context.EnabledFiles)}}
        - ملفات مستودع الملفات المقفولة (لا يحق للمستخدم الوصول لها): {{Bullets(context.DisabledFiles)}}

        قواعد المصادر — مهمة جدًا، وهي حدود صلاحيات حقيقية مش مجرد اقتراح
        هناك ثلاث حالات مختلفة لازم تفرّق بينها في ردك، ولكل واحدة صياغة مختلفة:
        - "غير مصرّح" (مصدر أو ملف في قائمة "مقفولة" فوق): ما تحاولش تخمّن ولا تجاوب من
          مصدر تاني بديل. رد باعتذار مهذب، اذكر اسم المصدر أو الملف المقفول بالظبط بالاسم،
          وقول للمستخدم إنه يحتاج يطلب تفعيله (من الإدمن أو من قائمة "المصادر" لو عنده صلاحية).
        - "غير مربوط بعد" (نظام في قائمة "مفعّلة لكن غير مربوطة" فوق): المستخدم عنده صلاحية
          الوصول له، لكن مفيش بيانات منه لسه لأنه لسه مش موصّل فعليًا. قول ده بوضوح — الفرق
          واضح عن "غير مصرّح" لأن هنا المشكلة تقنية/تشغيلية مش صلاحيات.
        - "لا توجد بيانات" (مصدر مفعّل ومربوط لكن الاستعلام رجع صفوف فاضية فعليًا): وضّح إن
          البيانات مفحوصة فعلًا لكن مفيش نتائج تطابق الشرط المطلوب (مثلاً فترة زمنية مفيش
          فيها سجلات).
        - في الحالتين الأولى والتانية رجّع JSON صحيح فيه summary وnarration يشرحوا الحالة
          بوضوح (narration بنفس أسلوبه السردي المعتاد، من غير أي تفاصيل تقنية برضه)، وwidgets
          تبقى مصفوفة فاضية []. في الحالة التالتة ممكن ترجع widget فاضي أو kpi بقيمة صفر
          مع توضيح صريح في summary إن النتيجة صفر فعليًا مش خطأ.
        - ما تخترعش أرقام أبدًا في أي حالة من التلاتة. كل رقم لازم يكون جاي من نتيجة
          list_files أو query_data أو search_documents فعلية.
        - ملفات الـ PDF مالهاش جداول: بياناتها الوصفية بترجع في list_files، ومحتواها بيتبحث فيه
          بـ search_documents. لو السؤال عن عدد الملفات أو أسمائها، استخدم list_files.

        {{_db.DialectPrompt}}

        قواعد إضافية على الاستعلام
        - لو مش متأكد من اسم عمود أو جدول بالظبط، ارجع لنتيجة list_files تاني بدل ما تخمّن
          الاسم أو تفترضه.
        - لو الاستعلام رجع خطأ SQL، اقرأ رسالة الخطأ وصحح سبب الخطأ فعليًا (اسم عمود غلط،
          جدول مش موجود، خطأ صياغة) وأعد المحاولة — بدل ما تدّي إجابة عامة أو تتجاهل الخطأ.
          لو استمر الخطأ بعد محاولتين متتاليتين، وضّح المشكلة صراحة في summary بدل ما تدخل
          في حلقة إعادة محاولات بلا نهاية.
        - افتكر إن التاريخ في القاعدة غالبًا مخزّن كنص، فاستخدم دوال التاريخ المناسبة لنوع
          القاعدة (مذكورة فوق) وما تفترضش صيغة معينة من غير ما تتأكد منها.

        فحص النتيجة قبل كتابة الرد (Result Validation)
        قبل ما تكتب الـ JSON النهائي، تأكد إن:
        - عدد الصفوف اللي رجعت منطقي مقارنة بالسؤال (مثلًا لو سألت عن "كل الشهور" ورجع صف
          واحد بس، راجع الاستعلام قبل ما ترد).
        - مفيش قيم NULL أو فاضية في مكان محوري (زي label أو value) من غير ما توضحها في summary.
        - الأرقام في نطاق منطقي (مفيش نسبة تتخطى ١٠٠٪ لمقياس نسبي، ولا قيمة سالبة لمقياس
          المفروض يكون موجب دايمًا) — لو لقيت حاجة غريبة، اذكرها بوضوح في summary بدل ما
          تسكت عنها أو "تصلحها" من دماغك.

        دلالات الوقت (Time Semantics)
        - لما المستخدم يقول "الشهر ده" أو "السنة دي" أو "آخر ٣٠ يوم"، اربطها بأحدث تاريخ
          موجود فعليًا في البيانات (مش بالضرورة تاريخ اليوم، لو البيانات متأخرة عن الآن)،
          واذكر في summary الفترة الزمنية اللي فعليًا استخدمتها بالأرقام (مثلًا "بيانات
          شهر ٢٠٢٤-٠٦" بدل "الشهر الحالي" بشكل مبهم).
        - النظام حاليًا ما بيوفّرش إعداد لسنة مالية مختلفة عن السنة الميلادية. لو المستخدم
          طلب "السنة المالية" أو "الربع المالي"، استخدم السنة/الربع الميلادي العادي ووضّح
          صراحة في summary إنك استخدمت التقويم الميلادي لعدم وجود إعداد سنة مالية مختلفة.

        صيغة الرد النهائي
        رسالتك الأخيرة لازم تكون كائن JSON واحد فقط — من غير أسوار كود ولا أي كلام قبله أو بعده —
        مطابق للمخطط ده (نفس المخطط دايمًا، بدون أي حقول إضافية):
        {
          "summary": "إجابة من سطر أو اثنين على السؤال، تتضمن أي افتراض اتخذته أو حالة استثنائية",
          "narration": "شرح سردي أغنى بالفصحى، ٤ إلى ٨ جمل، يستعرض كل عنصر بالترتيب — شوف قسم (الشرح السردي) فوق",
          "widgets": [
            {
              "type": "kpi | bar | line | pie | table | progress-table | trend-matrix | status-bar",
              "title": "عنوان العنصر",
              "data": [ ... ],
              "xKey": "اختياري، لـ bar/line: اسم حقل التصنيف",
              "yKey": "اختياري، لـ bar/line أحادي السلسلة: اسم الحقل الرقمي",
              "series": "اختياري، لـ bar/line متعدد السلاسل بدل yKey — شوف قسم (أشكال إضافية) تحت",
              "stacked": "اختياري، bar فقط مع series — true للتكديس، false أو غايب للتجاور",
              "centerLabel": "اختياري، pie فقط — شوف قسم (أشكال إضافية) تحت",
              "comparison": "اختياري على أي نوع — تخطيط مقارنة، شوف قسم (تخطيط المقارنة) تحت",
              "source": "جملتان بالعربي: الأولى مصدر البيانات، والثانية طريقة الحساب.",
              "forecast": "اختياري، بس فقط لو المستخدم طلب توقع فعليًا — شوف قسم (التنبؤ) تحت",
              "query": "اختياري لكن مهم لتفعيل الفلاتر — شوف قسم (حقل query) تحت"
            }
          ],
          "filters": [
            {
              "id": "معرّف قصير مميّز، مثلاً filter_status",
              "label": "اسم الفلتر بالعربي، مثلاً الحالة",
              "field": "اسم العمود الفعلي في الجدول",
              "table": "اسم الجدول الفعلي اللي جاي منه العمود",
              "type": "single_select | multi_select | date_range | numeric_range",
              "options": [ { "label": "متأخر", "value": "متأخر" } ],
              "appliesTo": "dashboard"
            }
          ]
        }
        حقل filters اختياري تمامًا — سيبه [] لو مفيش فلاتر مفيدة فعلًا (قسم "الفلاتر" فوق).

        حقل type إجباري ولازم يكون بالظبط واحد من القيم التمانية: kpi أو bar أو line أو pie
        أو table أو progress-table أو trend-matrix أو status-bar — أي قيمة تانية أو نوع
        جديد مش موجود في القايمة دي هيترفض تلقائيًا من الواجهة ويتعرض كجدول احتياطي بدل ما
        يختفي. لو مش متأكد أنهي نوع الأنسب لبيانات معيّنة، استخدم table لأنه بيقبل أي شكل
        بيانات جدولية بأمان.

        حقل source إجباري في كل عنصر، ولازم يكون جملتين بالظبط تفصل بينهما نقطة ومسافة:
        - الجملة الأولى: منين جت البيانات — اسم الجدول أو الملف والتصنيف أو النظام.
        - الجملة الثانية: إزاي اتحسبت — العملية الفعلية اللي عملتها.
        مثال: "من جدول staging_sample_sales في مستودع الملفات (تصنيف المبيعات). تم تجميع
        SUM(Revenue) وتقسيمها حسب Region مع ترتيب تنازلي."
        الكلام ده لازم يكون مطابق للأدوات اللي ناديتها فعلًا — ممنوع تأليف مصدر أو طريقة حساب.

        حقل query — بيخلي فلاتر اللوحة تقدر تأثّر فعليًا على العنصر
        لو بنيت عنصر (bar/line/pie/kpi) من نتيجة نداء query_data واحد بالظبط (مش مجمّع من
        أكتر من نداء)، حط حقل "query" فيه:
        { "table": "اسم الجدول الفعلي اللي جاي منه العمود (نفس الاسم اللي استخدمته في SQL)",
          "sql": "نفس جملة SELECT اللي فعلًا نفّذتها بالظبط، حرفيًا، من غير أي تعديل" }
        - "sql" لازم يكون النص الحرفي اللي بعته لـ query_data، مش نسخة معدّلة أو مبسّطة —
          الواجهة بتضيف شرط WHERE فوقه لاحقًا لما المستخدم يختار فلتر، فأي اختلاف عن
          الاستعلام الحقيقي هيدّي نتيجة غلط.
        - سيب query غايبة (من غير الحقل خالص) لو العنصر مبني من أكتر من نداء query_data
          مجمّعين، أو من list_files/search_documents بس، أو كان forecast — في الحالات دي
          مفيش استعلام واحد واضح يترجعله الفلتر، والعنصر هيظهر ببساطة "غير متأثر بالفلتر"
          وده أفضل من query غلط بيرجّع نتيجة غير صحيحة.
        - ده منفصل تمامًا عن source: source نص عربي للعرض، وquery بيانات تقنية خام للتنفيذ
          الآلي فقط — الاثنين لازم يوصفوا نفس الاستعلام لكن بصيغتين مختلفتين تمامًا.

        اصطلاحات بيانات العناصر
        - kpi: data عبارة عن [{"label": "...", "value": <رقم أو نص>}] (عنصر واحد).
        - bar/line: data مصفوفة كائنات؛ حدد xKey لحقل التصنيف/الزمن وyKey للحقل الرقمي
          (مثال [{"month": "2024-01", "revenue": 1234.5}] مع xKey "month" وyKey "revenue").
        - pie: data عبارة عن [{"label": "...", "value": <رقم>}, ...] (٦ شرائح كحد أقصى، والباقي في "أخرى").
        - table: data مصفوفة صفوف؛ المفاتيح بتبقى عناوين الأعمدة.
        - forecast (bar/line بس): كائن اختياري { "labels": [...], "values": [...], "lower": [...],
          "upper": [...], "method": "...", "note": "...", "r2": <رقم أو null> } — القيم دي لازم
          تكون بالظبط ناتج forecast_data (قسم "التنبؤ" تحت)، وممنوع تتكرر جوه data العادية.
        خلي الأرقام أرقام JSON مش نصوص. كل عنصر data لازم يكون ١٥ صف كحد أقصى تقريبًا —
        لو النتيجة الخام أكبر من كده، جمّعها أو رتّبها ("Top N") قبل ما تحطها في data بدل
        ما ترسل كل الصفوف الخام.

        توسعة bar/line: عدة سلاسل على نفس الرسم
        بدل yKey واحد، حط حقل "series": مصفوفة {"key": "اسم حقل رقمي موجود في كل عنصر data",
        "label": "اسم السلسلة بالعربي"} — مثال: data مثل [{"quarter":"Q1","baseline":538,
        "actual":399}] مع xKey "quarter" وseries [{"key":"baseline","label":"الأساسي"},
        {"key":"actual","label":"الفعلي"}]. استخدمها لما السؤال فيه مقارنة صريحة بين أكتر
        من قيمة لكل تصنيف (مخطط مقابل فعلي، خطة مقابل تنفيذ، عدة سيناريوهات). لـ bar فقط:
        حط "stacked": true لعرض السلاسل متكدسة فوق بعض داخل عمود واحد لكل تصنيف (مفيد لما
        السلاسل أجزاء من نفس الإجمالي، زي حالات المشاريع)، أو سيب stacked غايب/false لعرضها
        متجاورة (مفيد للمقارنة المباشرة، زي مخطط مقابل فعلي). الألوان بتتحدد تلقائيًا من
        نفس نظام ألوان التطبيق — ما تحطش حقل لون لأي سلسلة. لو مفيش مقارنة فعلية مطلوبة،
        استمر تستخدم yKey العادي بسلسلة واحدة زي أي وقت فات.

        توسعة pie: تسمية مركز الدائرة
        حقل "centerLabel" اختياري — نص عربي قصير (كلمة أو كلمتين) بيوصف بالظبط إيه اللي
        الرقم في نص الدائرة بيمثله، مثلاً "مبادرة" لو الشرائح بتمثل عدد مبادرات، أو "مشروع"
        لعدد مشاريع. لو مش موجود، الواجهة بتحاول تخمنه من العنوان تلقائيًا — لكن الأدق دايمًا
        إنك تحطه إنت صراحة لو عارف بالظبط إيه اللي بيتحسب.

        progress-table (نوع جديد كليًا) — جدول بشريط تقدّم مدمج
        استخدمه لما السؤال عن نسبة تنفيذ/استهلاك مقابل مستهدف أو ميزانية، مقسّم على فئات
        (مثلاً "نسبة تنفيذ العقود لكل جهة" أو "استهلاك الميزانية حسب القسم"). data: مصفوفة
        صفوف [{"label": "اسم الصف (اسم الجهة/القسم)", "total": <رقم الإجمالي أو المستهدف>,
        "segments": [{"value": <رقم>, "label": "وصف الجزء", "color": "accent | danger |
        amber | muted"}, ...]}]. الشريط بيتحسب نسبيًا لـ total الخاص بكل صف (مش لازم مجموع
        segments يساوي total بالظبط). حقل color اختياري ولازم يكون بالظبط واحدة من القيم
        الأربع دي — أي قيمة تانية أو غيابه هيترجم "muted" تلقائيًا (رمادي محايد)؛ استخدم
        "accent" للمُنفَّذ/الإيجابي، "danger" للمتجاوز/السلبي، "amber" للتحذير، "muted"
        للمتبقي/غير المستخدم.

        trend-matrix (نوع جديد كليًا) — جدول مؤشرات عبر فترات مع اتجاه تلقائي
        استخدمه لما السؤال عن إزاي اتغير مؤشر أو أكتر من فترة زمنية للتانية (مثلاً "قارن
        أداء المؤشرات بين الربع الأول والتاني"). data: مصفوفة [{"metric": "اسم المؤشر",
        "periods": [{"label": "Q1 2025", "value": <رقم>}, {"label": "Q2 2025", "value":
        <رقم>}, ...]}] — كل الصفوف لازم تشترك في نفس عدد وترتيب الفترات (نفس labels).
        عمود الاتجاه (سهم لأعلى/لأسفل/ثابت + الفرق العددي + رسم صغير) بيتحسب بالكامل في
        الواجهة من قيم آخر فترتين — ما تحطش حقل اتجاه أو لون أو رسم بنفسك، وما تحسبش الفرق
        إنت وتضيفه كنص.

        status-bar (نوع جديد كليًا) — رقم إجمالي + شارات حالة
        استخدمه لما السؤال عن إجمالي مقسّم حسب درجة تأخر أو خطورة (مثلاً "كام مشروع متأخر
        جدًا مقابل على المسار"). data: كائن واحد (مش مصفوفة) {"total": <رقم>, "label":
        "وصف الإجمالي", "statuses": [{"label": "متأخر جدًا", "count": <رقم>, "severity":
        "severe | warning | ontrack"}, ...]}. severity لازم تكون بالظبط واحدة من القيم
        التلاتة دي (severe=أحمر/خطر، warning=أصفر/تحذير، ontrack=أخضر/سليم). لو عندك أكتر
        من تصنيف إجمالي مختلف (مثلاً فئتين منفصلتين من المشاريع)، ابعت عنصر status-bar
        منفصل كامل لكل واحدة — ما تحطش أكتر من صف جوه data نفسها.

        تخطيط المقارنة (comparison) — خيار عرض على أي نوع موجود، مش نوع سادس
        استخدمه لما السؤال فيه مقارنة صريحة بين فترتين أو سيناريوهين لنفس التوزيع أو نفس
        المؤشر (مثلاً "قارن حالة المبادرات بين الربع الأول والتاني" أو "قارن الإيرادات بين
        يناير وفبراير"). حط حقل "comparison" على مستوى العنصر بدل حقل data المباشر:
        { "left": { "title": "الربع الأول", "data": [...] }, "right": { "title": "الربع
        الثاني", "data": [...] } } — كل جانب فيه نفس الحقول اللي النوع (type) المكتوب في
        نفس العنصر محتاجها عادةً (data، وxKey/series لو bar/line، إلخ). النوع بيتطبق على
        الجانبين الاتنين، وكل جانب بيتعرض ككارد كامل ومستقل بجانب التاني مع سهم رابط بينهم.
        لو حطيت comparison، سيب حقل data في المستوى الأعلى فاضي أو غايب — البيانات كلها
        جوه left/right.

        اختيار العناصر (محتوى اللوحة — الشكل والتنسيق مش مسؤوليتك، بيتحدد تلقائيًا في الواجهة)
        - للسؤال العادي: اختر ٢ إلى ٤ عناصر تجاوب عليه، وابدأ بـ kpi لو فيه رقم واحد رئيسي.
        - لو المستخدم طلب "تقرير شامل" أو "تقرير كامل" أو نظرة عامة على الأداء: ابنِ لوحة غنية
          من ٥ إلى ٨ عناصر تغطي كل المصادر المفعّلة — عدة مؤشرات kpi، ورسمين أو تلاتة مختلفين
          (bar وline وpie)، وجدول تفصيلي في الآخر.
        - رتّب العناصر بترتيب منطقي: kpi الملخّصة أولًا، بعدين الرسوم البيانية، وأخيرًا أي
          جدول تفصيلي.

        قائمة تحقق أخيرة قبل إرسال الرد (راجعها ذهنيًا في كل مرة)
        - كل رقم في الرد جاي من نتيجة أداة فعلية؟ (مفيش رقم مختلق).
        - حقل source في كل عنصر مطابق للأداة اللي ناديتها فعلًا؟
        - لو فيه افتراض اتخذته (فترة زمنية، تفسير غامض)، مذكور بوضوح في summary؟
        - الـ JSON مطابق للمخطط بالظبط (نفس الحقول، من غير حقول زيادة، وtype من القيم
          التمانية المسموحة فقط)؟
        - لو استخدمت series أو progress-table أو trend-matrix أو status-bar، كل حقل لون/
          severity فيها واحد بالظبط من القيم المسموحة (accent/danger/amber/muted أو
          severe/warning/ontrack) — مش لون hex ولا كلمة تانية مخترعة؟ ولو حطيت comparison،
          سيبت data في المستوى الأعلى فاضية وحطيت كل حاجة جوه left/right؟
        - لو المصدر مقفول أو غير مربوط، اتبعت "قواعد المصادر" بالظبط (widgets فاضية + توضيح)؟
        - لو فيه filters، كل "options" فيها جاي من استعلام DISTINCT حقيقي، وكل "table"
          مطابق لاسم الجدول اللي فعلًا استخدمته؟
        - لو حطيت حقل "forecast"، هل هو ناتج نداء forecast_data فعلي بالظبط (مش رقم حسبته
          إنت)، وهل المستخدم طلب توقعًا صراحة أصلًا؟
        - لو كان معاك قسم "اللوحة المعروضة حاليًا للمستخدم"، هل فعلاً عاملت السؤال كمتابعة
          لها (احتفظت بالعناصر غير المتأثرة وsource بتاعها) بدل ما تبني لوحة جديدة بلا داعٍ؟
          هل عدد عناصر ردك النهائي ≥ عدد "العناصر الحالية" (إلا لو المستخدم طلب حذف صريح)؟
          لو ردك فيه عنصر واحد بس رغم إن "العناصر الحالية" كانت أكتر من كده، ده على الأغلب
          خطأ — راجعه قبل ما تبعت.
        - narration بالفصحى فقط، وخالٍ تمامًا من أي ذكر لجدول أو ملف أو تصنيف أو "استعلام"
          أو "قاعدة بيانات" أو أي تفصيلة تقنية (كل ده مكانه source بس)، ومستعرض العناصر
          بترتيبها بروابط سردية طبيعية مش قائمة نقاط؟ وهل عنوان كل عنصر (title) ظاهر
          بالحرف الواحد جوه الجملة الخاصة بيه من غير أي تغيير؟

        الدقة أهم من الاكتمال، والاكتمال أهم من الجمال. لوحة بعنصرين صحيحين ومصدرهما واضح
        أفضل بكثير من لوحة بثمن عناصر بعضها مختلق أو غير موثّق.
        """;

    /// <summary>
    /// "الاستفسارات" mode — same tool catalogue (BuildTools) and the exact same source-
    /// permission rules as BuildSystemPrompt, but the final answer is a short, direct text
    /// answer instead of a dashboard: no widgets, no narration, no filters, no chart-type
    /// vocabulary at all. Kept as its own separate prompt (not a parameterized branch of
    /// BuildSystemPrompt) so the carefully-tuned dashboard prompt is never touched by this.
    /// </summary>
    public string BuildInquirySystemPrompt(SourceContext context) =>
        $$"""
        أنت "محلّل بيانات مؤسسي" (Enterprise Analytics Agent) بترد على استفسارات عن بيانات
        المؤسسة بإجابة نصية مباشرة وواضحة — من غير ما تبني لوحة معلومات أو أي عناصر رسوم
        بيانية أو تُرجع حقل widgets أصلًا. اكتب كل النصوص الظاهرة للمستخدم بالعربية الفصحى،
        بأسلوب مهني ومباشر وبدون حشو.

        القاعدة الذهبية: ممنوع تختلق رقمًا أو تخمّنه أو "تقرّبه" من غير ما يكون جاي فعليًا
        من نتيجة أداة ناديتها (list_files أو query_data أو forecast_data أو
        search_documents). لو مش متاح عندك الرقم، قول كده صراحة في إجابتك بدل ما تخترعه
        أو تسكت عن غيابه.

        خطوات العمل
        1. نادِ list_files لمعرفة الجداول والأعمدة المتاحة والملفات المحفوظة في المستودع.
        2. نادِ query_data باستعلام SELECT واحد مباشر (مجمّع لو محتاج) يرجّع بالظبط الرقم
           أو الأرقام المطلوبة للإجابة على سؤال المستخدم — من غير ما تجيب صفوف خام زيادة
           عن الحاجة.
        3. لو السؤال فيه طلب توقع/تنبؤ صريح لفترة مستقبلية، نادِ forecast_data بدل ما تحسب
           أو تخمّن الرقم بنفسك — بنفس القواعد المعتادة (استعلام برجّع فترة وقيمة رقمية
           بس، مرتب زمنيًا تصاعديًا).
        4. قبل الرد، تأكد إن كل رقم هتذكره فعلاً موجود في نتيجة أداة ناديتها.

        {{SourcesSummaryBullets(context)}}

        قواعد المصادر — مهمة جدًا، وهي حدود صلاحيات حقيقية مش مجرد اقتراح (نفس القواعد
        المعتمدة في بناء اللوحات بالظبط)
        هناك ثلاث حالات مختلفة لازم تفرّق بينها في ردك:
        - "غير مصرّح" (مصدر أو ملف في قائمة "مقفولة" فوق): ما تحاولش تخمّن ولا تجاوب من
          مصدر تاني بديل. رد باعتذار مهذب، اذكر اسم المصدر أو الملف المقفول بالظبط بالاسم،
          وقول للمستخدم إنه يحتاج يطلب تفعيله (من الإدمن أو من قائمة "المصادر" لو عنده صلاحية).
        - "غير مربوط بعد" (نظام في قائمة "مفعّلة لكن غير مربوطة" فوق): المستخدم عنده صلاحية
          الوصول له، لكن مفيش بيانات منه لسه لأنه لسه مش موصّل فعليًا. قول ده بوضوح.
        - "لا توجد بيانات" (مصدر مفعّل ومربوط لكن الاستعلام رجع صفوف فاضية فعليًا): وضّح إن
          البيانات مفحوصة فعلًا لكن مفيش نتائج تطابق الشرط المطلوب.
        - في الحالات التلاتة، لسه لازم ترجع كائن JSON صحيح بنفس الصيغة تحت — answer يشرح
          الحالة بوضوح (وليه مفيش رقم)، وsource يوضّح المصدر المقصود، وdata فاضية ({}).

        صيغة الرد النهائي — إجباري، JSON فقط من غير أي نص خارجه أو markdown fence
        {
          "answer": "الإجابة الكاملة بالعربية الفصحى — جملة أو فقرة قصيرة (لا تتعدى ٣-٤
            جمل)، تجاوب على سؤال المستخدم مباشرة بالأرقام الفعلية، من غير أي تفاصيل تقنية
            (اسم جدول أو ملف أو تصنيف أو كلمة \"استعلام\" أو \"قاعدة بيانات\") — كل ده مكانه
            source بس.",
          "source": "جملتان بالظبط، بنفس تنسيق حقل source في عناصر اللوحة العادية: الجملة
            الأولى منين جت البيانات (اسم الجدول أو الملف والتصنيف أو النظام)، والتانية إزاي
            اتحسبت.",
          "data": "القيمة أو الصفوف الفعلية اللي استندت عليها الإجابة، بنفس شكلها من نتيجة
            الأداة (رقم واحد، أو مصفوفة صفوف، أو كائن) — لازم تكون نفس الأرقام بالظبط اللي
            ذكرتها في answer، من غير أي فرق، عشان ممكن تتحول لاحقًا لعنصر لوحة معلومات."
        }
        """;

    /// <summary>Shared between BuildSystemPrompt and BuildInquirySystemPrompt — kept as its
    /// own small helper (not extracted from BuildSystemPrompt's inline text) so neither
    /// prompt's existing, tuned wording is touched by the other.</summary>
    private static string SourcesSummaryBullets(SourceContext context) => $"""
        المصادر المفعّلة حاليًا
        - أنظمة مفعّلة: {Bullets(context.EnabledSystems)}
        - أنظمة مقفولة (لا يحق للمستخدم الوصول لها): {Bullets(context.DisabledSystems)}
        - أنظمة مفعّلة لكن لسه غير مربوطة بقاعدة بيانات (مفيش داتا منها بعد): {Bullets(context.UnconnectedSystems)}
        - ملفات مستودع الملفات المفعّلة: {Bullets(context.EnabledFiles)}
        - ملفات مستودع الملفات المقفولة (لا يحق للمستخدم الوصول لها): {Bullets(context.DisabledFiles)}
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
                    // Hide tables belonging to a switched-off repository file, and label
                    // the rest so the model can attribute each number to a real source.
                    var visible = schema
                        .Where(t => !context.DisabledFileTables.ContainsKey(t.Table))
                        .Where(t => !context.DisabledSystemTables.ContainsKey(t.Table))
                        .Where(t => !context.RestrictedFileTables.ContainsKey(t.Table))
                        .Select(t => new
                        {
                            table = t.Table,
                            file = context.TableFiles.TryGetValue(t.Table, out var fn) ? fn : null,
                            system = context.TableSystems.TryGetValue(t.Table, out var sys) ? sys : null,
                            columns = t.Columns,
                        });
                    // The repository's own catalogue: a PDF contributes no table, so without
                    // this the model has no way to know the file exists at all.
                    var files = await _repository.ListAsync(ct);
                    var visibleFiles = files
                        .Where(f => context.EnabledFileIds.Contains(f.Id))
                        .Where(f => f.TableName is null || !context.RestrictedFileTables.ContainsKey(f.TableName))
                        .Select(f => new
                        {
                            name = f.DisplayName,
                            category = f.Category,
                            kind = f.Kind,
                            rows = f.Kind == "pdf" ? (int?)null : f.RowCount,
                            columns = f.Kind == "pdf" ? (int?)null : f.ColumnCount,
                            pages = f.Kind == "pdf" ? f.PageCount : (int?)null,
                            uploadedAt = f.UploadedAt.ToString("yyyy-MM-dd"),
                            queryableTable = f.TableName,
                        })
                        .ToList();

                    return (JsonSerializer.Serialize(new
                    {
                        tables = visible,
                        repositoryFiles = visibleFiles,
                        repositoryFileCount = visibleFiles.Count,
                        disabledFiles = context.DisabledFiles,
                        disabledSystems = context.DisabledSystems,
                    }, ToolResultJsonOptions), false);
                }
                case "query_data":
                {
                    var sql = input["sql"]?.GetValue<string>();
                    if (string.IsNullOrWhiteSpace(sql))
                        return ("Error: 'sql' input is required.", true);

                    var permissionError = CheckSourcePermission(sql, context);
                    if (permissionError is not null) return (permissionError, true);

                    return await ExecuteQueryAsync(sql, ct);
                }
                case "forecast_data":
                {
                    var sql = input["sql"]?.GetValue<string>();
                    if (string.IsNullOrWhiteSpace(sql))
                        return ("Error: 'sql' input is required.", true);
                    if (input["periodsAhead"] is not JsonValue periodsNode || !periodsNode.TryGetValue<int>(out var periodsAhead))
                        return ("Error: 'periodsAhead' input is required and must be an integer.", true);
                    periodsAhead = Math.Clamp(periodsAhead, 1, 12);
                    int? seasonLength = input["seasonLength"] is JsonValue seasonNode && seasonNode.TryGetValue<int>(out var sl) ? sl : null;

                    var permissionError = CheckSourcePermission(sql, context);
                    if (permissionError is not null) return (permissionError, true);

                    return await ExecuteForecastAsync(sql, periodsAhead, seasonLength, ct);
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
                        .Where(d => context.EnabledFileIds.Contains(d.Id))
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
                    return (JsonSerializer.Serialize(hits, ToolResultJsonOptions), false);
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

    /// <summary>
    /// Null if the query is allowed; otherwise the Arabic refusal message to return to the
    /// model. Internal (not private) so <see cref="Widgets.WidgetQueryService"/> can apply the
    /// exact same table/category gate to a widget's stored SQL before re-executing it with a
    /// filter spliced in — the same permission surface as the LLM's own query_data tool.
    /// </summary>
    internal static string? CheckSourcePermission(string sql, SourceContext context)
    {
        var blockedSystem = context.DisabledSystemTables
            .FirstOrDefault(kv => sql.Contains(kv.Key, StringComparison.OrdinalIgnoreCase)
                || sql.Contains(kv.Key.Split('.').Last(), StringComparison.OrdinalIgnoreCase));
        if (blockedSystem.Key is not null)
            return $"الاستعلام مرفوض: الجدول {blockedSystem.Key} تابع لـ\"{blockedSystem.Value}\" " +
                   "وهو غير مفعّل حاليًا. اطلب من المستخدم تفعيله من قائمة المصادر.";

        var blockedFile = context.DisabledFileTables
            .FirstOrDefault(kv => sql.Contains(kv.Key, StringComparison.OrdinalIgnoreCase)
                || sql.Contains(kv.Key.Split('.').Last(), StringComparison.OrdinalIgnoreCase));
        if (blockedFile.Key is not null)
            return $"الاستعلام مرفوض: الجدول {blockedFile.Key} تابع لملف \"{blockedFile.Value}\" " +
                   "وهو غير مفعّل حاليًا. اطلب من المستخدم تفعيله من قائمة المصادر.";

        var restrictedFile = context.RestrictedFileTables
            .FirstOrDefault(kv => sql.Contains(kv.Key, StringComparison.OrdinalIgnoreCase)
                || sql.Contains(kv.Key.Split('.').Last(), StringComparison.OrdinalIgnoreCase));
        if (restrictedFile.Key is not null)
            return $"الاستعلام مرفوض: الجدول {restrictedFile.Key} تابع لملف \"{restrictedFile.Value}\" " +
                   "وصلاحية استخدامه كمصدر بيانات غير ممنوحة لهذا المستخدم.";

        return null;
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

        var warning = DetectQuotedIdentifierMistake(sql);
        if (warning is not null)
            return (JsonSerializer.Serialize(new { rowCount = rows.Count, rows, warning }, ToolResultJsonOptions), false);

        return (JsonSerializer.Serialize(new { rowCount = rows.Count, rows }, ToolResultJsonOptions), false);
    }

    /// <summary>
    /// See the GroupByTarget comment above: catches both directions of the single/double-quote
    /// mix-up around whatever name GROUP BY resolves to — GROUP BY 'x' itself, and/or a
    /// SELECT-list alias that still literal-quotes the same name GROUP BY already treats (or
    /// should treat) as the real column.
    /// </summary>
    private static string? DetectQuotedIdentifierMistake(string sql)
    {
        var groupBy = GroupByTarget.Match(sql);
        if (!groupBy.Success) return null;

        var selectAliasesLiteral = (string name) =>
            Regex.IsMatch(sql, $@"'{Regex.Escape(name)}'\s+AS\s+\w+", RegexOptions.IgnoreCase);

        if (groupBy.Groups[1].Success)
        {
            var literal = groupBy.Groups[1].Value;
            return selectAliasesLiteral(literal)
                ? $"Both the SELECT list and GROUP BY use '{literal}' in single quotes, which SQL " +
                    "treats as a fixed string literal — not a column reference — so every row shows " +
                    "the same label text and groups into one bucket. Use double quotes for the column " +
                    $"in both places: SELECT \"{literal}\" AS ..., GROUP BY \"{literal}\"."
                : $"GROUP BY '{literal}' uses single quotes, which SQL treats as a fixed string " +
                    "literal — not a column reference — so every row groups into the same single " +
                    $"bucket. If you meant to group by a column, use double quotes instead: GROUP BY \"{literal}\".";
        }

        var column = groupBy.Groups[2].Value;
        return selectAliasesLiteral(column)
            ? $"GROUP BY \"{column}\" correctly groups by the column, but the SELECT list aliases the " +
                $"fixed string '{column}' instead of the column's actual value — every row will show " +
                $"the literal text \"{column}\" as the label instead of the real grouped value. Use " +
                $"double quotes there too: SELECT \"{column}\" AS ..."
            : null;
    }

    /// <summary>
    /// Runs the model's two-column time-series query, then hands the value column to
    /// ForecastService for a real statistical forecast — see the forecast_data tool description
    /// in BuildTools() for the exact contract the model must follow.
    /// </summary>
    private async Task<(string Result, bool IsError)> ExecuteForecastAsync(
        string sql, int periodsAhead, int? seasonLength, CancellationToken ct)
    {
        var validationError = ValidateReadOnlySql(sql);
        if (validationError is not null)
            return ($"Query rejected: {validationError}", true);

        await using var connection = await _db.OpenConnectionAsync(ct);
        var labels = new List<string>();
        var values = new List<double>();

        await using var reader = await connection.ExecuteReaderAsync(
            new CommandDefinition(sql, commandTimeout: 30, cancellationToken: ct));
        if (reader.FieldCount < 2)
            return ("Error: the query must return exactly two columns — a period label, then a numeric value.", true);
        while (values.Count < MaxRowsReturned && await reader.ReadAsync(ct))
        {
            if (reader.IsDBNull(1)) continue; // skip a period with no value rather than failing the whole fit
            if (!TryToDouble(reader.GetValue(1), out var value)) continue;
            labels.Add(reader.IsDBNull(0) ? "" : reader.GetValue(0)?.ToString() ?? "");
            values.Add(value);
        }

        if (values.Count < 2)
            return ("Error: need at least 2 historical rows with a non-null numeric value to forecast.", true);

        ForecastOutcome outcome;
        try
        {
            outcome = ForecastService.Forecast(values, periodsAhead, seasonLength);
        }
        catch (InvalidOperationException ex)
        {
            return ($"Error: {ex.Message}", true);
        }

        return (JsonSerializer.Serialize(new
        {
            historical = new { labels, values },
            forecast = new
            {
                method = outcome.Method,
                r2 = outcome.RSquared,
                note = outcome.Note,
                values = outcome.Points.Select(p => Math.Round(p.Value, 2)).ToList(),
                lower = outcome.Points.Select(p => Math.Round(p.Lower, 2)).ToList(),
                upper = outcome.Points.Select(p => Math.Round(p.Upper, 2)).ToList(),
            },
        }, ToolResultJsonOptions), false);
    }

    private static bool TryToDouble(object raw, out double value)
    {
        switch (raw)
        {
            case double d: value = d; return true;
            case float f: value = f; return true;
            case decimal m: value = (double)m; return true;
            case int i: value = i; return true;
            case long l: value = l; return true;
            case short s: value = s; return true;
            default: return double.TryParse(raw?.ToString(), out value);
        }
    }

    /// <summary>
    /// Pulls the single JSON object out of a model's final-turn text — tolerates a markdown
    /// code fence or stray prose around it. Shared by every "parse the model's final answer"
    /// path (dashboard, Inquiries); each does its own typed deserialization/validation after.
    /// </summary>
    private static (string? Candidate, string? Error) ExtractJsonCandidate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (null, "the response contained no text.");

        var candidate = text.Trim();
        var fence = Regex.Match(candidate, @"```(?:json)?\s*(\{.*\})\s*```", RegexOptions.Singleline);
        if (fence.Success)
            return (fence.Groups[1].Value, null);

        var start = candidate.IndexOf('{');
        var end = candidate.LastIndexOf('}');
        if (start < 0 || end <= start)
            return (null, "no JSON object found in the response.");
        return (candidate[start..(end + 1)], null);
    }

    /// <summary>
    /// Parses and validates the final dashboard JSON. Tolerates a markdown code fence
    /// or stray prose around the object, but the JSON itself must match the schema.
    /// </summary>
    public static (DashboardSpec? Dashboard, string? Error) TryParseDashboard(string text)
    {
        var (candidate, extractError) = ExtractJsonCandidate(text);
        if (candidate is null) return (null, extractError);

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

        // A comparison-carrying widget legitimately omits "data" entirely (see
        // BuildSystemPrompt), which deserializes to a JsonElement with ValueKind Undefined —
        // Validate() above already allows that, but JsonSerializer.Serialize throws when
        // asked to write an Undefined element back out for the API response. Normalize it to
        // an empty array (the same value an explicit "data": [] would produce) so the
        // response can actually be serialized.
        foreach (var w in spec.Widgets)
            if (w.Data.ValueKind == JsonValueKind.Undefined) w.Data = EmptyArrayElement;

        return (spec, null);
    }

    /// <summary>
    /// Parses and validates an Inquiries-mode final answer JSON (see InquiryModels.cs and
    /// BuildInquirySystemPrompt) — same tolerant extraction as <see cref="TryParseDashboard"/>,
    /// just a much smaller contract.
    /// </summary>
    public static (InquiryResponse? Inquiry, string? Error) TryParseInquiry(string text)
    {
        var (candidate, extractError) = ExtractJsonCandidate(text);
        if (candidate is null) return (null, extractError);

        InquiryResponse? spec;
        try
        {
            spec = JsonSerializer.Deserialize<InquiryResponse>(candidate);
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

        // Same Undefined-JsonElement serialization hazard as a dashboard widget's "data" —
        // see TryParseDashboard. Here "data" is required by Validate() to be present at all
        // only in spirit (the model is told to always include the real values), but nothing
        // enforces it structurally, so guard the same way regardless.
        if (spec.Data.ValueKind == JsonValueKind.Undefined) spec.Data = EmptyArrayElement;

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
