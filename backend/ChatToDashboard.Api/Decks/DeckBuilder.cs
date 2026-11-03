using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ChatToDashboard.Api.Build;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Semantics;
using ChatToDashboard.Api.Users;

namespace ChatToDashboard.Api.Decks;

/// <summary>طلب توليد/تعديل عرض بالموديل.</summary>
public sealed record DeckBuildRequest(
    /// <summary>ما طلبه المستخدم في المحادثة («اعمل عرض نتائج الربع…» أو «ضيف شريحة عن…»).</summary>
    string Message,
    /// <summary>المواصفة الحالية عند التعديل (null = عرض جديد).</summary>
    DeckSpec? Current,
    /// <summary>نموذج مختار (شرائحه تحدد الهيكل) — اختياري.</summary>
    DeckTemplate? Template,
    AnalyticsTools.SourceContext Context,
    AppUser? RequestingUser);

public sealed record DeckBuildResult(DeckSpec Spec, string Reply, int DataSlidesBuilt, int DataSlidesTotal);

/// <summary>
/// منشئ العروض بالموديل على مرحلتين (كأسلوب البناء المرحلي للوحات): (1) نداء تخطيط واحد بلا أدوات يعيد
/// المواصفة كاملة مع تعليم الشرائح التي تحتاج بيانات (needsData + intent + sql مقترح)؛ (2) نداء لكل شريحة بيانات
/// بالتوازي بأداة query_data فقط يعيد أرقام الشريحة (رسم/جدول/مؤشرات/نقاط) ومصدرها. عند التعديل تُمرَّر المواصفة
/// الحالية ويُطلب إرجاعها كاملة بعد التغيير مع الإبقاء على الشرائح غير المعنية (بمعرّفاتها) كما هي.
/// </summary>
public class DeckBuilder
{
    private const int SlideMaxIterations = 5;
    private readonly LlmRouter _router;
    private readonly DataProfileService _profiles;
    private readonly AnalyticsTools _tools;
    private readonly ColumnSemanticsService _semantics;
    private readonly int _parallelism;
    private readonly ILogger<DeckBuilder> _logger;

    public DeckBuilder(LlmRouter router, DataProfileService profiles, AnalyticsTools tools, ColumnSemanticsService semantics, IConfiguration configuration, ILogger<DeckBuilder> logger)
    {
        _router = router; _profiles = profiles; _tools = tools; _semantics = semantics; _logger = logger;
        _parallelism = Math.Clamp(configuration.GetValue("Build:Parallelism", 4), 1, 8);
    }

    public async Task<DeckBuildResult> BuildAsync(DeckBuildRequest request, CancellationToken ct)
    {
        var llm = await _router.ResolveStagedAsync(ct) ?? throw new InvalidOperationException("لا يوجد مزوّد يدعم البناء المرحلي.");
        var profile = await _profiles.BuildAsync(request.Context, ct);
        var hasData = profile.Tables.Count > 0;
        string profileText = "";
        if (hasData)
        {
            await _semantics.EnsureAnalyzedAsync(request.Context.ProjectId, profile.Tables.Select(t => t.Table), TimeSpan.FromSeconds(30), ct);
            profileText = profile.ToPromptText(semantics: await _semantics.GetPromptNotesAsync(request.Context.ProjectId, ct));
        }

        // ---- المرحلة 1: التخطيط (المواصفة كاملة) ----
        var system = new StringBuilder()
            .AppendLine("أنت مصمم عروض تقديمية تنفيذية داخل منصة «جيم». تنتج مواصفة عرض JSON فقط (لا HTML)، بالعربية الفصحى، موجزة ومناسبة للإدارة.")
            .AppendLine("التخطيطات المتاحة لكل شريحة: title (عنوان العرض)، section (فاصل قسم)، bullets (3–6 نقاط قصيرة)، two-column (عمودان بعنوان ونقاط)، kpis (2–4 مؤشرات: label/value/delta)، chart (رسم: type bar|line|pie مع labels/values)، table (جدول حتى 10 صفوف)، quote (اقتباس/رسالة واحدة)، tasks (قائمة مهام قابلة للتعديل من الجمهور بعد النشر).")
            .AppendLine("القاعدة الذهبية: لا تخترع أرقامًا. أي شريحة تعتمد على بيانات المشروع تُعلَّم needsData:true مع intent (ما المطلوب تحديدًا) وsql مقترح (SELECT واحد بأسماء الجداول والأعمدة حرفيًا كما في ملف التعريف) وتترك قيمها فارغة — ستُملأ لاحقًا بنداء منفصل ينفّذ الاستعلام. الشرائح النصية (أهداف، توصيات، نطاق) تكتبها أنت مباشرة.")
            .AppendLine("لو لا توجد بيانات في المشروع، لا تعلّم أي شريحة needsData واكتب عرضًا نصيًا واضحًا بناءً على طلب المستخدم.")
            .AppendLine("أبقِ العرض بين 5 و12 شريحة ما لم يطلب المستخدم غير ذلك. أول شريحة دائمًا title.")
            .AppendLine(hasData ? "\n# ملف تعريف بيانات المشروع\n" + profileText : "\n# لا توجد بيانات مرفوعة في المشروع.")
            .ToString();

        var user = new StringBuilder();
        if (request.Template is { } t)
        {
            user.AppendLine($"# النموذج المختار: {t.Name} — {t.Description}").AppendLine("هيكل الشرائح (التزم به وبترتيبه، وعدّل العناوين لتناسب الطلب):");
            var i = 0; foreach (var s in t.Slides) user.AppendLine($"{++i}) [{s.Layout}] {s.Title}{(s.NeedsData ? " — بيانات: " + s.Intent : "")}{(s.Hint is not null && s.Layout == "chart" ? " — نوع الرسم: " + s.Hint : "")}");
            user.AppendLine();
        }
        if (request.Current is { } cur)
        {
            user.AppendLine("# المواصفة الحالية للعرض (JSON) — عدّلها بحسب الطلب وأعدها كاملة؛ أبقِ الشرائح غير المعنية بمعرّفاتها وبياناتها كما هي حرفيًا")
                .AppendLine(JsonSerializer.Serialize(cur, DeckSpec.Json)).AppendLine();
        }
        user.AppendLine("# طلب المستخدم").AppendLine(request.Message.Trim()).AppendLine().AppendLine("""
            # صيغة الرد — كائن JSON واحد فقط بلا أي نص خارجه
            {
              "reply": "جملة أو جملتان للمستخدم بالعربية تصفان ما فعلته",
              "spec": {
                "title": "عنوان العرض", "subtitle": "اختياري",
                "slides": [
                  {"id": "اختياري (أبقِ المعرّف عند التعديل)", "layout": "title|section|bullets|two-column|kpis|chart|table|quote|tasks", "title": "...",
                   "subtitle": "اختياري", "bullets": ["..."], "columns": [{"title": "...", "bullets": ["..."]}], "kpis": [{"label": "...", "value": "...", "delta": "اختياري"}],
                   "chart": {"type": "bar|line|pie", "labels": [], "values": []}, "table": {"columns": [], "rows": [[]]}, "quote": "...", "notes": "ملاحظات المتحدث (اختياري)",
                   "needsData": true, "intent": "ما المطلوب من البيانات", "sql": "SELECT ..."}
                ]
              }
            }
            """);

        var planCall = new StagedCall($"[عرض] {Shorten(request.Message, 40)} — تخطيط", system, user.ToString(), null, request.Context, 2, false, request.RequestingUser);
        var plan = await llm.RunStagedAsync(planCall, ct);
        var reply = plan.Json["reply"]?.GetValue<string>() ?? "تم إعداد العرض.";
        var spec = (plan.Json["spec"]?.Deserialize<DeckSpec>(DeckSpec.Json) ?? new DeckSpec { Title = "عرض جديد" }).Normalize();
        if (spec.Slides.Count == 0) spec.Slides.Add(new DeckSlide { Id = DeckSpec.NewId(), Layout = "title", Title = spec.Title });

        // استعلامات مقترحة من مرحلة التخطيط (تُقرأ من الـ JSON الخام لأن sql ليس حقلًا في المواصفة)
        var suggestedSql = new Dictionary<string, string>(StringComparer.Ordinal);
        if (plan.Json["spec"]?["slides"] is JsonArray planned)
            foreach (var (node, idx) in planned.OfType<JsonObject>().Select((n, i) => (n, i)))
                if (node["sql"]?.GetValue<string>() is { Length: > 0 } sql && idx < spec.Slides.Count) suggestedSql[spec.Slides[idx].Id] = sql;

        // ---- المرحلة 2: تعبئة شرائح البيانات بالتوازي ----
        var dataSlides = hasData ? spec.Slides.Where(s => s.NeedsData == true).ToList() : new List<DeckSlide>();
        if (!hasData) foreach (var s in spec.Slides.Where(s => s.NeedsData == true)) { s.NeedsData = null; s.MissingReason = "لا توجد بيانات مرفوعة في المشروع."; }
        var built = 0;
        using var gate = new SemaphoreSlim(_parallelism);
        await Task.WhenAll(dataSlides.Select(async slide =>
        {
            await gate.WaitAsync(ct);
            try { if (await FillSlideAsync(llm, slide, suggestedSql.GetValueOrDefault(slide.Id), profileText, request, ct)) Interlocked.Increment(ref built); }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex) { _logger.LogWarning(ex, "فشل بناء شريحة {Title}", slide.Title); slide.MissingReason = "تعذّر بناء الشريحة: " + ex.Message; }
            finally { gate.Release(); }
        }));
        foreach (var s in spec.Slides) { s.NeedsData = null; s.Intent = null; }
        return new DeckBuildResult(spec.Normalize(), reply, built, dataSlides.Count);
    }

    /// <summary>نداء شريحة واحدة بأداة query_data: ينفّذ الاستعلام ويعيد محتوى الشريحة بالشكل المطلوب لتخطيطها.</summary>
    private async Task<bool> FillSlideAsync(IStagedLlm llm, DeckSlide slide, string? suggestedSql, string profileText, DeckBuildRequest request, CancellationToken ct)
    {
        var layout = slide.Layout ?? "bullets";
        var shape = layout switch
        {
            "kpis" => "\"kpis\": [{\"label\": \"...\", \"value\": \"رقم منسّق مع الوحدة\", \"delta\": \"اختياري مثل +12%\"}]  (2–4 مؤشرات)",
            "chart" => "\"chart\": {\"type\": \"" + (slide.Chart?.Type ?? "bar") + "\", \"labels\": [\"...\"], \"values\": [أرقام], \"unit\": \"اختياري\"}  (حتى 12 فئة؛ للخط الزمني رتّب زمنيًا)",
            "table" => "\"table\": {\"columns\": [\"...\"], \"rows\": [[\"...\"]]}  (حتى 10 صفوف، الأرقام منسّقة)",
            _ => "\"bullets\": [\"...\"]  (3–6 نقاط بأرقام حقيقية من النتيجة)",
        };
        var system = "أنت تبني شريحة واحدة في عرض تقديمي من بيانات المشروع الحقيقية. نفّذ الاستعلام بأداة query_data أولًا (عدّله إن فشل)، ثم أعد JSON واحدًا فقط بالشكل المطلوب. لا تخترع أرقامًا؛ إن تعذّر فعلًا أعد {\"noData\": true, \"missingReason\": \"...\"}.\n\n# ملف تعريف البيانات\n" + profileText;
        var user = new StringBuilder()
            .AppendLine($"# الشريحة: «{slide.Title}» — التخطيط: {layout}")
            .AppendLine($"المطلوب: {slide.Intent ?? slide.Title}");
        if (suggestedSql is not null) user.AppendLine().AppendLine("# استعلام مقترح من التخطيط (نفّذه أولًا)").AppendLine(suggestedSql);
        user.AppendLine().AppendLine("# صيغة الرد").AppendLine("{" + shape + ", \"source\": \"من أي جدول وكيف حُسب (جملة)\", \"query\": {\"table\": \"...\", \"sql\": \"...\"}, \"notes\": \"ملاحظة للمتحدث (اختياري)\"}");
        var tools = _tools.BuildTools(request.Context).Where(t => t.Name == "query_data").ToList();
        var call = new StagedCall($"[عرض] شريحة — {slide.Title}", system, user.ToString(), tools, request.Context, SlideMaxIterations, true, request.RequestingUser);
        var result = await llm.RunStagedAsync(call, ct);
        var j = result.Json;
        if (j["noData"]?.GetValue<bool>() == true) { slide.MissingReason = j["missingReason"]?.GetValue<string>() ?? "لا تكفي البيانات لهذه الشريحة."; return false; }
        try
        {
            switch (layout)
            {
                case "kpis": slide.Kpis = j["kpis"]?.Deserialize<List<DeckKpi>>(DeckSpec.Json); break;
                case "chart": slide.Chart = j["chart"]?.Deserialize<DeckChart>(DeckSpec.Json) ?? slide.Chart; break;
                case "table": slide.Table = j["table"]?.Deserialize<DeckTable>(DeckSpec.Json); break;
                default: slide.Bullets = j["bullets"]?.Deserialize<List<string>>(DeckSpec.Json); break;
            }
        }
        catch (JsonException) { slide.MissingReason = "رد الموديل لهذه الشريحة لم يكن بالشكل المطلوب."; return false; }
        var filled = layout switch { "kpis" => slide.Kpis is { Count: > 0 }, "chart" => slide.Chart is { Values.Count: > 0 }, "table" => slide.Table is { Rows.Count: > 0 }, _ => slide.Bullets is { Count: > 0 } };
        if (!filled) { slide.MissingReason = "لم تُرجع البيانات نتيجة لهذه الشريحة."; return false; }
        slide.Source = j["source"]?.GetValue<string>();
        slide.Query = j["query"]?.Deserialize<DeckQuery>(DeckSpec.Json);
        slide.Notes ??= j["notes"]?.GetValue<string>();
        slide.MissingReason = null;
        return true;
    }

    private static string Shorten(string s, int n) => s.Length > n ? s[..n] + "…" : s;
}
