using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ChatToDashboard.Api.Build;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Sources;

namespace ChatToDashboard.Api.Semantics;

/// <summary>
/// يفهم جدولًا مرفوعًا مرة واحدة (المرحلة أ من طبقة المعنى): يبني ملف تعريف الجدول (الأعمدة
/// والقيم والعينة)، ثم تخمين حتمي من أسماء الأعمدة (<see cref="ConceptCatalog.GuessFromName"/>)،
/// ثم نداء واحد للموديل بلا أدوات يربط كل عمود بمفهوم من القاموس مع ثقة وخريطة قيم (حالة
/// «مفتوحة» = open). النتيجة تُحفظ في <see cref="ColumnSemanticsStore"/> ولا تلمس ما أكّده
/// المستخدم. يعمل في الخلفية بعد الرفع فلا يؤخّر الحفظ، ويُستدعى يدويًا من واجهة المصادر.
/// </summary>
public class ColumnSemanticsService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly ColumnSemanticsStore _store;
    private readonly DataProfileService _profiles;
    private readonly AnalyticsTools _tools;
    private readonly LlmRouter _router;
    private readonly ConceptPriorStore _priors;
    private readonly ILogger<ColumnSemanticsService> _logger;

    public ColumnSemanticsService(
        ColumnSemanticsStore store, DataProfileService profiles, AnalyticsTools tools, LlmRouter router,
        ConceptPriorStore priors, ILogger<ColumnSemanticsService> logger)
    {
        _priors = priors;
        _store = store;
        _profiles = profiles;
        _tools = tools;
        _router = router;
        _logger = logger;
    }

    /// <summary>يشغّل التحليل بلا انتظار (بعد الرفع) ويسجّل أي فشل بدل أن يُسقط الطلب.</summary>
    public void AnalyzeInBackground(string? projectId, string? tableName)
    {
        if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(tableName)) return;
        _ = RunAnalysisAsync(projectId, tableName);
    }

    /// <summary>الجداول قيد التحليل الآن (مفتاح المشروع|الجدول) — تمنع تشغيل التحليل نفسه مرتين من الرفع
    /// وقائمة المعاني وبناء اللوحة في وقت واحد.</summary>
    private readonly HashSet<string> _inFlight = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>حد التوازي لنداءات الموديل في التحليل الخلفي على مستوى التطبيق.</summary>
    private readonly SemaphoreSlim _gate = new(3);

    /// <summary>يشغّل تحليل جدول واحد في مهمة خلفية مع منع التكرار وحد التوازي، ويعيد مهمته
    /// (أو null إن كان الجدول نفسه قيد التحليل بالفعل).</summary>
    private Task? RunAnalysisAsync(string projectId, string tableName)
    {
        var key = projectId + "|" + tableName;
        lock (_inFlight) { if (!_inFlight.Add(key)) return null; }
        return Task.Run(async () =>
        {
            await _gate.WaitAsync();
            try { await AnalyzeTableAsync(projectId, tableName, CancellationToken.None); }
            catch (Exception ex) { _logger.LogWarning(ex, "فشل تحليل معاني الجدول {Table}", tableName); }
            finally { _gate.Release(); lock (_inFlight) { _inFlight.Remove(key); } }
        });
    }

    /// <summary>الجداول التي لم يُنتج لها الموديل أو المستخدم أي صف بعد (لم تُحلَّل، أو تخمينات أسماء فقط
    /// لأن نداء الموديل فشل) — تشمل جداول الأنظمة الخارجية التي لا تمر بمسار الرفع.</summary>
    public async Task<IReadOnlyList<string>> UnanalyzedTablesAsync(string? projectId, IEnumerable<string> tables, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(projectId)) return Array.Empty<string>();
        var analyzed = (await _store.ListForProjectAsync(projectId, ct))
            .Where(r => r.Source is SemanticSources.Ai or SemanticSources.User)
            .Select(r => r.TableName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return tables.Where(t => !analyzed.Contains(t)).ToList();
    }

    /// <summary>يضمن تحليل كل الجداول المعطاة قبل بناء لوحة: يشغّل التحليل لغير المحلَّلة (بتوازٍ محدود)
    /// وينتظر حتى <paramref name="budget"/>؛ ما لا يكتمل في المهلة يستمر في الخلفية ويُفيد البناء التالي.
    /// جدول قيد التحليل بالفعل (من رفع أو نداء سابق) لا يُعاد تشغيله. يعيد عدد ما اكتمل داخل المهلة.</summary>
    public async Task<int> EnsureAnalyzedAsync(string? projectId, IEnumerable<string> tables, TimeSpan budget, CancellationToken ct = default)
    {
        var pending = await UnanalyzedTablesAsync(projectId, tables, ct);
        if (pending.Count == 0) return 0;
        var tasks = pending.Select(t => RunAnalysisAsync(projectId!, t)).Where(t => t is not null).Select(t => t!).ToList();
        if (tasks.Count == 0) return 0;
        var all = Task.WhenAll(tasks);
        var finished = await Task.WhenAny(all, Task.Delay(budget, ct)) == all;
        if (!finished && budget > TimeSpan.Zero) _logger.LogInformation("تحليل المعاني لم يكتمل خلال المهلة لـ{Count} جدول — يستمر في الخلفية", tasks.Count);
        return tasks.Count(t => t.IsCompleted);
    }

    /// <summary>يحلّل الجدول ويحفظ النتيجة، ويعيد الصفوف المحفوظة. القاعدة الحتمية وحدها عندما
    /// لا يتوفر مزوّد يدعم النداء المرحلي أو يفشل النداء.</summary>
    public async Task<IReadOnlyList<ColumnSemantic>> AnalyzeTableAsync(string projectId, string tableName, CancellationToken ct = default)
    {
        var profile = await _profiles.ProfileOneAsync(tableName, projectId, ct);
        if (profile is null) throw new InvalidOperationException($"الجدول {tableName} غير موجود.");

        var rows = new List<ColumnSemantic>();
        var priors = await _priors.LookupManyAsync(profile.Columns.Select(c => c.Name), ct);
        foreach (var c in profile.Columns)
        {
            // التعلّم العابر للعملاء أولًا (اسم مُوحَّد سبق تأكيده)، ثم المرادفات.
            (string Concept, double Confidence)? prior = priors.TryGetValue(c.Name, out var p) ? p : null;
            var guess = ConceptCatalog.GuessFromName(c.Name);
            var concept = prior?.Concept ?? guess?.Concept.Id;
            var confidence = prior?.Confidence ?? guess?.Confidence ?? 0.2;
            rows.Add(new ColumnSemantic
            {
                ProjectId = projectId, TableName = tableName, ColumnName = c.Name,
                Concept = concept, Role = ConceptCatalog.Find(concept)?.Role ?? RoleFromKind(c.Kind),
                Confidence = confidence, Source = SemanticSources.Rule,
                Note = prior is not null ? "اسم سبق ربطه بهذا المفهوم في ملفات أخرى." : guess is null ? null : "تخمين من اسم العمود.",
            });
        }

        try
        {
            var llm = await _router.ResolveStagedAsync(ct);
            if (llm is not null) await RefineWithModelAsync(llm, projectId, profile, rows, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "تعذّر تحليل معاني {Table} بالموديل — اعتُمد التخمين الحتمي", tableName);
        }

        await _store.UpsertAnalyzedAsync(rows, ct);
        // نتيجة موديل واثقة تُسجَّل كسابقة (الاسم المُوحَّد فقط، لا بيانات).
        foreach (var r in rows.Where(r => r.Source == SemanticSources.Ai && r.Concept is not null && r.Confidence >= 0.9))
            await _priors.RecordAsync(r.ColumnName, r.Concept!, byUser: false, ct);
        return await _store.ListForTableAsync(projectId, tableName, ct);
    }

    /// <summary>تأكيد المستخدم يُحفظ للمشروع ويُسجَّل سابقةً أقوى من أي نتيجة موديل.</summary>
    public async Task SetByUserAsync(string projectId, string tableName, string columnName, string? concept, string role,
        string? unit, string? valueMapJson, CancellationToken ct = default)
    {
        await _store.SetByUserAsync(projectId, tableName, columnName, concept, role, unit, valueMapJson, ct);
        if (concept is not null) await _priors.RecordAsync(columnName, concept, byUser: true, ct);
    }

    private async Task RefineWithModelAsync(IStagedLlm llm, string projectId, ProfiledTable profile, List<ColumnSemantic> rows, CancellationToken ct)
    {
        var system = """
            أنت محلل بيانات تربط أعمدة جدول مرفوع بمفاهيم تحليلية من قاموس ثابت. لكل عمود اختر المفهوم
            الأقرب من القاموس أدناه (أو null إن لم يطابق أي مفهوم — عمود مساعد أو غامض)، ودوره، ووحدته إن
            كانت مالية أو زمنية، وثقتك من 0 إلى 1، وسببًا قصيرًا بالعربية. استدل من اسم العمود **وقيمه
            وعينة الصفوف معًا** لا من الاسم وحده: عمود Status قيمه «مفتوحة/مكسوبة/خاسرة» هو deal_status
            ولو كان اسمه مبهمًا. عند مفهوم له قيم قياسية، اربط كل قيمة من قيم العميل بقيمة قياسية في
            valueMap (قيمة العميل → القياسية)؛ اترك ما لا يطابق.
            قواعد: لا تخترع مفهومًا خارج القاموس؛ معرّفات السجلات record_id؛ الأعمدة النصية الحرة name.
            الرد كائن JSON واحد: {"columns": [{"column": "اسم العمود حرفيًا", "concept": "id أو null", "role": "measure|dimension|time|identifier|text|flag", "unit": "اختياري", "confidence": 0.0, "valueMap": {"قيمة": "قياسية"} أو null, "note": "..."}]}

            # القاموس
            """ + ConceptCatalog.ToPromptText();

        var user = new StringBuilder().AppendLine("# الجدول").AppendLine(new DataProfile(new[] { profile }).ToPromptText(9000))
            .AppendLine().AppendLine("# التخمين الأولي من الأسماء (راجعه وصحّحه)")
            .AppendLine(string.Join("\n", rows.Select(r => $"- {r.ColumnName}: {r.Concept ?? "null"} ({r.Confidence:0.0})")))
            .ToString();

        var selection = new SourceSelection { SystemsUnset = true, FilesUnset = true, ProjectId = projectId, IsAdmin = true };
        var context = await _tools.DescribeSourcesAsync(selection, ct);
        var call = new StagedCall($"[معاني] {profile.SourceName ?? profile.Table}", system, user, null, context, 2, false, null);
        var result = await llm.RunStagedAsync(call, ct);

        var byName = rows.ToDictionary(r => r.ColumnName, StringComparer.OrdinalIgnoreCase);
        foreach (var node in result.Json["columns"] as JsonArray ?? new JsonArray())
        {
            if (node is not JsonObject o) continue;
            var column = Str(o, "column");
            if (column is null || !byName.TryGetValue(column, out var row)) continue;
            var concept = Str(o, "concept");
            if (concept is not null && !ConceptCatalog.IsKnown(concept)) concept = null; // لا نثق بمفهوم خارج القاموس
            row.Concept = concept;
            var role = Str(o, "role");
            row.Role = role is ConceptRoles.Measure or ConceptRoles.Dimension or ConceptRoles.Time or ConceptRoles.Identifier or ConceptRoles.Text or ConceptRoles.Flag
                ? role : ConceptCatalog.Find(concept)?.Role ?? row.Role;
            row.Unit = Str(o, "unit");
            row.Confidence = o["confidence"] is JsonValue cv && cv.TryGetValue<double>(out var conf) ? Math.Clamp(conf, 0, 1) : row.Confidence;
            row.ValueMapJson = o["valueMap"] is JsonObject vm && vm.Count > 0 ? vm.ToJsonString() : null;
            row.Note = Str(o, "note");
            row.Source = SemanticSources.Ai;
        }
    }

    /// <summary>ملاحظات المعنى لكل عمود في المشروع، لتُلحَق بملف تعريف البيانات في نداءات البناء:
    /// «الإيراد (revenue)» أو مع خريطة القيم «حالة الصفقة (deal_status): مفتوحة=open، مكسوبة=won».</summary>
    public async Task<IReadOnlyDictionary<(string Table, string Column), string>> GetPromptNotesAsync(string? projectId, CancellationToken ct = default)
    {
        var notes = new Dictionary<(string, string), string>();
        if (string.IsNullOrWhiteSpace(projectId)) return notes;
        foreach (var s in await _store.ListForProjectAsync(projectId, ct))
        {
            var concept = ConceptCatalog.Find(s.Concept);
            if (concept is null || s.Confidence < 0.4) continue;
            var text = $"{concept.Label} ({concept.Id})";
            if (s.ValueMapJson is { Length: > 2 })
            {
                try
                {
                    var map = JsonSerializer.Deserialize<Dictionary<string, string>>(s.ValueMapJson, JsonOptions);
                    if (map is { Count: > 0 }) text += ": " + string.Join("، ", map.Select(kv => $"{kv.Key}={kv.Value}"));
                }
                catch (JsonException) { }
            }
            notes[(s.TableName, s.ColumnName)] = text;
        }
        return notes;
    }

    private static string RoleFromKind(string kind) => kind switch
    {
        "number" => ConceptRoles.Measure, "date" => ConceptRoles.Time, _ => ConceptRoles.Dimension,
    };

    private static string? Str(JsonObject o, string key) =>
        o[key] is JsonValue v && v.TryGetValue<string>(out var s) && !string.IsNullOrWhiteSpace(s) ? s.Trim() : null;
}
