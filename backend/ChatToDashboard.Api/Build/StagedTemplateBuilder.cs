using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ChatToDashboard.Api.Data;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Models;
using ChatToDashboard.Api.SchemaLibrary;
using ChatToDashboard.Api.Semantics;
using ChatToDashboard.Api.Templates;
using ChatToDashboard.Api.Users;
using Dapper;

namespace ChatToDashboard.Api.Build;

/// <summary>مدخلات بناء قالب لوحة بالمسار المرحلي — يجمعها ChatController من TemplatePromptService.</summary>
public sealed record StagedBuildRequest(
    string TemplateName,
    /// <summary>نص القالب نفسه بعد استبدال المتغيرات (ما يصف «عمّ اللوحة»)، بلا كتلة العناصر الإلزامية.</summary>
    string TemplateBody,
    IReadOnlyList<BuiltinDashboardWidgetSpec> Widgets,
    /// <summary>«القواعد العامة» التي يحررها مالك المنصة (قد تكون فارغة).</summary>
    string? GeneralRules,
    /// <summary>تطابقات مكتبة الحقول والجداول (تلميحات فقط).</summary>
    IReadOnlyDictionary<string, SchemaMatchingService.MatchedWidget> Hints,
    AnalyticsTools.SourceContext Context,
    string? Lang,
    AppUser? RequestingUser);

/// <summary>
/// البناء المرحلي لقالب لوحة (بديل حلقة الأدوات الواحدة لمسار القوالب):
///   ١) ملف تعريف البيانات بلا موديل (DataProfileService).
///   ٢) نداء تخطيط واحد بلا أدوات: لكل عنصر إما استعلام SQL أو «غير ممكن» وسببه، مع السياق
///      المشترك (الجداول والتعريفات والفترة) والفلاتر المقترحة.
///   ٣) نداء لكل عنصر ممكن، بالتوازي، بأداة query_data فقط وسقف دورات صغير، يعيد العنصر كاملًا.
///   ٤) نداء تجميع صغير بلا أدوات يكتب الملخص والسرد؛ والفلاتر تُتحقَّق قيمها من قاعدة البيانات
///      مباشرة لا من الموديل.
/// العناصر غير الممكنة لا تُرسل للموديل أصلًا وتظهر كـ noData بسبب واضح. فشل عنصر واحد لا يسقط
/// اللوحة. ChatController يُجري بعد ذلك نفس التسوية والتعلّم اللذين يجريهما للمسار القديم.
/// </summary>
public class StagedTemplateBuilder
{
    private const int WidgetMaxIterations = 5;
    private const int MaxFilterOptions = 30;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private static readonly JsonElement EmptyArray = JsonDocument.Parse("[]").RootElement;

    private readonly LlmRouter _router;
    private readonly DataProfileService _profiles;
    private readonly AnalyticsTools _tools;
    private readonly DataStore _db;
    private readonly ColumnSemanticsService _semantics;
    private readonly WidgetFeasibilityService _feasibility;
    private readonly int _parallelism;
    private readonly ILogger<StagedTemplateBuilder> _logger;

    public StagedTemplateBuilder(
        LlmRouter router, DataProfileService profiles, AnalyticsTools tools, DataStore db,
        ColumnSemanticsService semantics, WidgetFeasibilityService feasibility, IConfiguration configuration,
        ILogger<StagedTemplateBuilder> logger)
    {
        _semantics = semantics;
        _feasibility = feasibility;
        _router = router;
        _profiles = profiles;
        _tools = tools;
        _db = db;
        _parallelism = Math.Clamp(configuration.GetValue("Build:Parallelism", 4), 1, 8);
        _logger = logger;
    }

    /// <summary>null عندما لا يدعم المزوّد الحالي البناء المرحلي — فيعود المتحكم للمسار القديم.</summary>
    public async Task<DashboardSpec?> BuildAsync(StagedBuildRequest request, CancellationToken ct = default)
    {
        var llm = await _router.ResolveStagedAsync(ct);
        if (llm is null) return null;

        var profile = await _profiles.BuildAsync(request.Context, ct);
        if (profile.Tables.Count == 0)
        {
            // لا جداول مرئية أصلًا: لا معنى لنداء التخطيط — كل العناصر بانتظار مصدر بسبب واضح.
            const string reason = "لا توجد مصادر بيانات مفعّلة في المشروع — ارفع ملفًا أو فعّل نظامًا من «المصادر».";
            var empty = request.Widgets.Select(w => NoDataWidget(w, reason)).ToList();
            var text = $"لا توجد مصادر بيانات مفعّلة لبناء لوحة «{request.TemplateName}» — ارفع ملفًا أو فعّل نظامًا من «المصادر» ثم أعد البناء.";
            return new DashboardSpec { Summary = text, Narration = text, Widgets = empty };
        }
        // طبقة المعنى: ملاحظات المفاهيم المؤكدة/المرجّحة لكل عمود تُلحق بملف التعريف في كل نداء.
        var semantics = await _semantics.GetPromptNotesAsync(request.Context.ProjectId, ct);
        var profileText = profile.ToPromptText(semantics: semantics);

        // الجدوى الحتمية (المرحلة ج من طبقة المعنى): عنصر يحتاج مفهومًا صلبًا غير موجود في
        // بيانات المشروع لا يُرسل للموديل أصلًا — سببه معروف قبل أي توكن.
        var feasibility = await _feasibility.EvaluateAsync(request.Context.ProjectId, request.Widgets, ct);
        var available = await _feasibility.AvailableConceptsAsync(request.Context.ProjectId, ct);
        var candidates = request.Widgets.Where(w => !feasibility.TryGetValue(w.Title, out var f) || f.Feasible).ToList();
        var availableText = available.Count == 0 ? null : string.Join("، ", available.Select(kv =>
            $"{ConceptCatalog.Find(kv.Key)?.Label ?? kv.Key} ({kv.Key}) ← {string.Join("/", kv.Value.Select(c => c.Column).Distinct().Take(3))}"));

        var plan = candidates.Count == 0
            ? new BuildPlan("", new Dictionary<string, PlannedWidget>(StringComparer.OrdinalIgnoreCase), Array.Empty<PlannedFilter>())
            : await PlanAsync(llm, request with { Widgets = candidates }, profileText, availableText, ct);

        var results = new DashboardWidget[request.Widgets.Count];
        using var gate = new SemaphoreSlim(_parallelism);
        var tasks = request.Widgets.Select(async (spec, i) =>
        {
            if (feasibility.TryGetValue(spec.Title, out var fz) && !fz.Feasible)
            {
                results[i] = NoDataWidget(spec, $"يحتاج «{string.Join("» و«", fz.Missing)}» وهو غير موجود في بياناتك — أضف عمودًا له أو أكّد معنى عمود موجود من «المصادر».");
                return;
            }
            var planned = plan.Widgets.TryGetValue(spec.Title, out var p) ? p : null;
            if (planned is null || !planned.Feasible)
            {
                results[i] = NoDataWidget(spec, planned?.Reason ?? "لا يوجد في المصادر المتاحة جدول أو عمود يطابق هذا العنصر.");
                return;
            }
            await gate.WaitAsync(ct);
            try { results[i] = await BuildWidgetAsync(llm, request, profile, profileText, plan, spec, planned, ct); }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "فشل بناء العنصر «{Title}» في قالب «{Template}»", spec.Title, request.TemplateName);
                results[i] = NoDataWidget(spec, "تعذّر بناء هذا العنصر من البيانات المتاحة — جرّب تحديد مصدر له يدويًا.");
            }
            finally { gate.Release(); }
        });
        await Task.WhenAll(tasks);

        var widgets = results.ToList();
        var filters = await VerifyFiltersAsync(plan.Filters, profile, ct);
        var (summary, narration) = await AssembleAsync(llm, request, widgets, ct);
        return new DashboardSpec { Summary = summary, Narration = narration, Widgets = widgets, Filters = filters };
    }

    // ---------- ١) التخطيط ----------

    private sealed record PlannedWidget(string Title, bool Feasible, string? Reason, string? Type, string? Sql, string? Note);
    private sealed record PlannedFilter(string Label, string Field, string Table, string Type);
    private sealed record BuildPlan(string SharedContext, IReadOnlyDictionary<string, PlannedWidget> Widgets, IReadOnlyList<PlannedFilter> Filters);

    private async Task<BuildPlan> PlanAsync(IStagedLlm llm, StagedBuildRequest request, string profileText, string? availableText, CancellationToken ct)
    {
        var system = new StringBuilder()
            .AppendLine(LanguageLine(request.Lang))
            .AppendLine("""
                أنت محلل بيانات مؤسسي تخطط بناء لوحة متابعة من قالب جاهز على بيانات حقيقية موصوفة أدناه.
                مهمتك التخطيط فقط، لا تنفيذ استعلامات. لكل عنصر مطلوب قرّر: هل يمكن بناؤه من هذه الجداول؟
                قواعد ملزمة:
                - لا تفترض وجود جدول أو عمود غير مذكور في ملف تعريف البيانات. الأسماء تُكتب كما هي حرفيًا.
                - أي قيمة نصية في شرط WHERE أو CASE تُؤخذ فقط من «القيم» المذكورة للعمود في ملف التعريف.
                - عنصر «ممكن» = استعلام SELECT واحد (مجمّع بـ SUM/COUNT/AVG/GROUP BY عند الحاجة) يرجع الشكل المناسب لنوعه.
                - عنصر يحتاج رقمًا خارجيًا غير موجود (مستهدف، سقف، سعر) = «غير ممكن» بسبب صريح، لا معادلة مخترعة.
                - يجوز التقريب الصريح: إن أمكن بناء مقياس قريب من المطلوب من الأعمدة المتاحة، اجعله ممكنًا واكتب في note ما الفرق بالضبط.
                - عرّف المقاييس المشتركة مرة واحدة في sharedContext (مثلًا: الإيراد = SUM(Revenue)، الفترة الافتراضية = آخر ١٢ شهرًا من البيانات) ليبقى كل العناصر متسقة.
                - الفلاتر: اقترح ٢ إلى ٤ أعمدة فئوية أو زمنية ذات معنى تجاري، موجودة فعلًا في جدول محدد. لا تذكر قيمها؛ ستُجلب من قاعدة البيانات.
                """)
            .AppendLine(_db.DialectPrompt(request.Context.ProjectId))
            .AppendLine()
            .AppendLine("# ملف تعريف البيانات المتاحة")
            .AppendLine(profileText)
            .AppendLine(availableText is null ? "" : $"\n# المفاهيم المتاحة في هذه البيانات (مؤكدة أو مرجّحة)\n{availableText}")
            .ToString();

        var user = new StringBuilder()
            .AppendLine("# القالب")
            .AppendLine(request.TemplateBody.Trim())
            .AppendLine()
            .AppendLine("# العناصر المطلوبة (العنوان ثابت ولا يُغيَّر)");
        var n = 0;
        foreach (var w in request.Widgets)
        {
            user.Append(++n).Append(") «").Append(w.Title).Append("» — النوع المقترح: ").Append(WidgetShapeCatalog.NormalizeType(w.Type));
            if (w.Page is not null) user.Append(" — الصفحة: ").Append(w.Page);
            if (!string.IsNullOrWhiteSpace(w.Prompt)) user.Append(" — ").Append(w.Prompt);
            if (request.Hints.TryGetValue(w.Title, out var h))
            {
                user.Append(" — تلميح من مكتبة الحقول: الجدول ").Append(h.RealTable);
                if (h.RealValueColumn is not null) user.Append("، عمود القيمة ").Append(h.RealValueColumn);
                if (h.RealGroupColumn is not null) user.Append("، عمود التجميع ").Append(h.RealGroupColumn);
            }
            user.AppendLine();
        }
        if (!string.IsNullOrWhiteSpace(request.GeneralRules))
            user.AppendLine().AppendLine("# قواعد عامة من مالك المنصة").AppendLine(request.GeneralRules.Trim());
        user.AppendLine().AppendLine("""
            # صيغة الرد — كائن JSON واحد فقط بلا أي نص خارجه
            {
              "sharedContext": "تعريفات المقاييس والجداول المستخدمة والفترة الافتراضية، في فقرة قصيرة",
              "filters": [{"label": "اسم الفلتر بالعربية", "field": "اسم العمود حرفيًا", "table": "اسم الجدول حرفيًا", "type": "single_select" | "multi_select" | "date_range" | "numeric_range"}],
              "widgets": [{"title": "العنوان حرفيًا كما ورد", "feasible": true | false, "reason": "سبب عدم الإمكان بالعربية (عند false)", "type": "النوع الذي ستُبنى به", "sql": "استعلام SELECT واحد (عند true)", "note": "وصف أي تقريب (اختياري)"}]
            }
            أدرج كل العناصر المطلوبة بنفس العدد والعناوين الحرفية.
            """);

        var call = new StagedCall($"[قالب] {request.TemplateName} — تخطيط", system, user.ToString(), null, request.Context, 2, false, request.RequestingUser);
        var result = await llm.RunStagedAsync(call, ct);

        var planned = new Dictionary<string, PlannedWidget>(StringComparer.OrdinalIgnoreCase);
        foreach (var node in result.Json["widgets"] as JsonArray ?? new JsonArray())
        {
            if (node is not JsonObject o) continue;
            var title = Str(o, "title");
            if (title is null) continue;
            var feasible = o["feasible"] is JsonValue fv && fv.TryGetValue<bool>(out var f) && f;
            var sql = Str(o, "sql");
            planned[title] = new PlannedWidget(title, feasible && !string.IsNullOrWhiteSpace(sql), Str(o, "reason"), Str(o, "type"), sql, Str(o, "note"));
        }
        var filters = new List<PlannedFilter>();
        foreach (var node in result.Json["filters"] as JsonArray ?? new JsonArray())
        {
            if (node is not JsonObject o) continue;
            var label = Str(o, "label"); var field = Str(o, "field"); var table = Str(o, "table"); var type = Str(o, "type");
            if (label is null || field is null || table is null || type is null) continue;
            filters.Add(new PlannedFilter(label, field, table, type));
        }
        return new BuildPlan(Str(result.Json, "sharedContext") ?? "", planned, filters);
    }

    // ---------- ٢) عنصر واحد ----------

    private async Task<DashboardWidget> BuildWidgetAsync(
        IStagedLlm llm, StagedBuildRequest request, DataProfile profile, string profileText, BuildPlan plan,
        BuiltinDashboardWidgetSpec spec, PlannedWidget planned, CancellationToken ct)
    {
        var type = WidgetShapeCatalog.IsKnown(planned.Type) ? planned.Type!.ToLowerInvariant() : WidgetShapeCatalog.NormalizeType(spec.Type);

        // الجزء الثابت أولًا (قواعد + ملف التعريف + السياق المشترك) ليستفيد كل نداء من تخزين البادئة.
        var system = new StringBuilder()
            .AppendLine(LanguageLine(request.Lang))
            .AppendLine("""
                أنت محلل بيانات مؤسسي تبني عنصرًا واحدًا من لوحة متابعة على بيانات حقيقية.
                قواعد ملزمة:
                - كل رقم في ردك يجب أن يأتي من نتيجة query_data فعلية في هذه المحادثة. لا تخمين ولا تقريب ولا حساب ذهني.
                - نفّذ الاستعلام المقترح أولًا؛ إن أخطأ فاقرأ الخطأ وصحّحه. لا تزد عن أربعة استعلامات.
                - أي قيمة نصية في WHERE/CASE تُؤخذ فقط من القيم المذكورة في ملف تعريف البيانات.
                - الأرقام في data أرقام JSON لا نصوص. ١٥ صفًا كحد أقصى (جمّع أو Top N).
                - اكتب النصوص الظاهرة (التسميات، source) بالعربية الفصحى المختصرة.
                - source: جملتان بالضبط — من أين جاءت البيانات (اسم الجدول والأعمدة الحقيقية)، ثم كيف حُسبت.
                - query: إن بُني العنصر من استعلام واحد، ضع {"table": "اسم الجدول الحقيقي", "sql": "نص الاستعلام حرفيًا كما نُفّذ"}؛ وإلا احذف الحقل.
                - إن تبيّن بعد الاستعلام أن البيانات لا تكفي فعلًا، أعد {"noData": true, "missingReason": "سبب قصير"} بدل اختلاق قيم.
                """)
            .AppendLine(_db.DialectPrompt(request.Context.ProjectId))
            .AppendLine()
            .AppendLine("# ملف تعريف البيانات المتاحة")
            .AppendLine(profileText)
            .AppendLine()
            .AppendLine("# السياق المشترك للوحة")
            .AppendLine(string.IsNullOrWhiteSpace(plan.SharedContext) ? "(لا يوجد)" : plan.SharedContext)
            .ToString();

        var user = new StringBuilder()
            .Append("# العنصر المطلوب\nالعنوان (ثابت): «").Append(spec.Title).AppendLine("»")
            .Append("النوع: ").AppendLine(type);
        if (!string.IsNullOrWhiteSpace(spec.Prompt)) user.Append("الوصف: ").AppendLine(spec.Prompt);
        if (!string.IsNullOrWhiteSpace(planned.Note)) user.Append("ملاحظة التخطيط: ").AppendLine(planned.Note);
        user.AppendLine().AppendLine("# الاستعلام المقترح من مرحلة التخطيط (نفّذه بـ query_data أولًا)")
            .AppendLine(planned.Sql)
            .AppendLine().AppendLine("# شكل data المطلوب لهذا النوع")
            .AppendLine(WidgetShapeCatalog.Describe(type))
            .AppendLine().AppendLine("""
                # صيغة الرد النهائي — كائن JSON واحد فقط بلا أي نص خارجه
                {"type": "...", "title": "...", "data": ..., "xKey": "اختياري", "yKey": "اختياري", "options": {اختياري}, "source": "...", "query": {"table": "...", "sql": "..."}, "dataIssues": ["اختياري"]}
                أو عند تعذّر البناء فعلًا: {"type": "...", "title": "...", "noData": true, "missingReason": "..."}
                """);

        var tools = _tools.BuildTools(request.Context).Where(t => t.Name == "query_data").ToList();
        var call = new StagedCall($"[قالب] {request.TemplateName} — {spec.Title}", system, user.ToString(), tools, request.Context,
            WidgetMaxIterations, true, request.RequestingUser);
        var result = await llm.RunStagedAsync(call, ct);

        DashboardWidget? widget;
        try { widget = JsonSerializer.Deserialize<DashboardWidget>(result.Text, JsonOptions); }
        catch (JsonException) { widget = null; }
        if (widget is null) return NoDataWidget(spec, "رد الموديل لهذا العنصر لم يكن بالشكل المطلوب.");

        widget.Title = spec.Title; // العنوان ثابت: التسوية والصفحات تعتمد عليه حرفيًا
        if (!WidgetShapeCatalog.IsKnown(widget.Type)) widget.Type = type;
        if (widget.NoData)
        {
            widget.MissingReason ??= planned.Reason ?? "لا تكفي البيانات المتاحة لبناء هذا العنصر.";
            widget.Data = EmptyArray;
            return widget;
        }
        if (widget.Data.ValueKind == JsonValueKind.Undefined)
            widget.Data = WidgetShapeCatalog.ObjectShaped.Contains(widget.Type) ? JsonDocument.Parse("{}").RootElement : EmptyArray;
        if (widget.Query is not null && (string.IsNullOrWhiteSpace(widget.Query.Sql) || profile.Find(widget.Query.Table) is null))
            widget.Query = null; // استعلام لا يشير لجدول حقيقي لا يصلح لإعادة التنفيذ بالفلاتر
        if (string.IsNullOrWhiteSpace(widget.Source))
            widget.Source = widget.Query is not null
                ? $"من الجدول {widget.Query.Table}. حُسب بالاستعلام: {widget.Query.Sql}."
                : "من الجداول المتاحة في المشروع. حُسب باستعلام تجميعي.";

        // نفس التحقق الذي يمر به رد المسار القديم، على هذا العنصر وحده.
        var probe = new DashboardSpec { Summary = "-", Narration = "-", Widgets = new List<DashboardWidget> { widget } };
        var errors = probe.Validate();
        if (errors.Count > 0)
        {
            _logger.LogInformation("عنصر «{Title}» غير صالح: {Errors}", spec.Title, string.Join(" | ", errors));
            return NoDataWidget(spec, "رد الموديل لهذا العنصر لم يطابق شكل البيانات المطلوب.");
        }
        return widget;
    }

    // ---------- ٣) الفلاتر: التحقق من قاعدة البيانات مباشرة ----------

    private async Task<List<DashboardFilter>> VerifyFiltersAsync(IReadOnlyList<PlannedFilter> planned, DataProfile profile, CancellationToken ct)
    {
        var filters = new List<DashboardFilter>();
        foreach (var f in planned.Take(4))
        {
            var table = profile.Find(f.Table);
            var column = table?.Columns.FirstOrDefault(c => string.Equals(c.Name, f.Field, StringComparison.OrdinalIgnoreCase));
            if (table is null || column is null) continue;
            var type = f.Type.ToLowerInvariant();
            var options = new List<FilterOption>();
            if (type is "single_select" or "multi_select")
            {
                if (column.Kind != "text") continue;
                try
                {
                    await using var connection = await _db.OpenConnectionAsync(ct);
                    var col = _db.Provider == DbProvider.Sqlite ? $"\"{column.Name.Replace("\"", "\"\"")}\"" : $"[{column.Name.Replace("]", "]]")}]";
                    var qt = _db.QualifiedTable(table.Table);
                    var sql = _db.Provider == DbProvider.Sqlite
                        ? $"SELECT DISTINCT {col} AS v FROM {qt} WHERE {col} IS NOT NULL AND {col} <> '' ORDER BY 1 LIMIT {MaxFilterOptions + 1}"
                        : $"SELECT DISTINCT TOP {MaxFilterOptions + 1} {col} AS v FROM {qt} WHERE {col} IS NOT NULL AND CAST({col} AS NVARCHAR(MAX)) <> '' ORDER BY 1";
                    var values = (await connection.QueryAsync<string>(new CommandDefinition(sql, commandTimeout: 20, cancellationToken: ct))).ToList();
                    if (values.Count == 0 || values.Count > MaxFilterOptions) continue; // عمود عالي التنوع لا يصلح فلترًا
                    options = values.Select(v => new FilterOption { Label = v, Value = v }).ToList();
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogDebug(ex, "تعذّر جلب قيم الفلتر {Field}", f.Field);
                    continue;
                }
            }
            else if (type == "date_range" ? column.Kind != "date" : type == "numeric_range" ? column.Kind != "number" : true)
                continue;

            filters.Add(new DashboardFilter
            {
                Id = $"f{filters.Count + 1}", Label = f.Label, Field = column.Name, Table = table.Table, Type = type, Options = options,
            });
        }
        return filters;
    }

    // ---------- ٤) التجميع ----------

    private async Task<(string Summary, string Narration)> AssembleAsync(
        IStagedLlm llm, StagedBuildRequest request, IReadOnlyList<DashboardWidget> widgets, CancellationToken ct)
    {
        var built = widgets.Where(w => !w.NoData).ToList();
        var missing = widgets.Where(w => w.NoData).ToList();
        var fallbackSummary = built.Count == 0
            ? $"تعذّر بناء عناصر لوحة «{request.TemplateName}» من المصادر المتاحة حاليًا — راجع أسباب كل عنصر وحدّد له مصدرًا."
            : $"بُنيت {built.Count} من {widgets.Count} عناصر لوحة «{request.TemplateName}» من بيانات المشروع" +
              (missing.Count > 0 ? $"، و{missing.Count} عنصرًا بانتظار مصدر بيانات مناسب." : ".");
        if (built.Count == 0) return (fallbackSummary, fallbackSummary);

        var digest = new StringBuilder();
        foreach (var w in built)
        {
            var data = w.Data.GetRawText();
            digest.Append("- «").Append(w.Title).Append("» (").Append(w.Type).Append("): ")
                .AppendLine(data.Length > 400 ? data[..400] + "…" : data);
        }
        if (missing.Count > 0)
            digest.Append("عناصر بلا بيانات: ").AppendLine(string.Join("، ", missing.Select(w => $"«{w.Title}»")));

        var system = LanguageLine(request.Lang) + """

            أنت تكتب ملخص لوحة متابعة وسردها من عناصر مبنية فعلًا. لا تضف أي رقم غير موجود في العناصر أدناه.
            - summary: جملة إلى ثلاث جمل مختصرة تلخّص أهم ما تُظهره اللوحة، وتذكر عدد العناصر التي تنتظر مصدر بيانات إن وُجدت.
            - narration: شرح سردي بالعربية الفصحى من ثلاث إلى ست جمل، يُقرأ بصوت مسموع، بلا تفاصيل تقنية ولا أسماء أعمدة.
            الرد كائن JSON واحد: {"summary": "...", "narration": "..."}
            """;
        try
        {
            var call = new StagedCall($"[قالب] {request.TemplateName} — تجميع", system,
                $"# اللوحة: {request.TemplateName}\n{request.TemplateBody.Trim()}\n\n# العناصر المبنية\n{digest}",
                null, request.Context, 2, false, request.RequestingUser);
            var result = await llm.RunStagedAsync(call, ct);
            var summary = Str(result.Json, "summary");
            var narration = Str(result.Json, "narration");
            if (!string.IsNullOrWhiteSpace(summary) && !string.IsNullOrWhiteSpace(narration)) return (summary, narration);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "فشل نداء التجميع لقالب «{Template}» — استُخدم ملخص تلقائي", request.TemplateName);
        }
        return (fallbackSummary, fallbackSummary);
    }

    // ---------- مساعدات ----------

    private static DashboardWidget NoDataWidget(BuiltinDashboardWidgetSpec spec, string reason) => new()
    {
        Type = WidgetShapeCatalog.NormalizeType(spec.Type), Title = spec.Title, Page = spec.Page,
        NoData = true, MissingReason = reason, Data = EmptyArray,
    };

    private static string LanguageLine(string? lang) => lang == "en"
        ? "LANGUAGE: write every user-facing text (labels, source, summary, narration) in clear professional English. Instructions below are in Arabic; follow them exactly."
        : "";

    private static string? Str(JsonObject o, string key) =>
        o[key] is JsonValue v && v.TryGetValue<string>(out var s) && !string.IsNullOrWhiteSpace(s) ? s.Trim() : null;
}
