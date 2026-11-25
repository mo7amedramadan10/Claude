using System.Text.Json;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using ChatToDashboard.Api.Build;
using ChatToDashboard.Api.Decks;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Semantics;
using ChatToDashboard.Api.Users;

namespace ChatToDashboard.Api.Freeform;

public sealed record FreeformResult(string Reply, string? Html, bool UsedData, IReadOnlyList<FreeformQuery>? Queries = null);
/// <summary>نتيجة تعديل جزء واحد: <c>Fragment</c> null = لا تعديل (سؤال توضيح أو رفض بعد محاولتين).</summary>
public sealed record FreeformFragmentResult(string Reply, string? Fragment, bool UsedData, IReadOnlyList<FreeformQuery> Queries, string? Rejected);

/// <summary>يكتب صفحة HTML كاملة من طلب حر (كما تفعل artifacts) داخل المنصة: الموديل يرى ملف تعريف بيانات المشروع ويستطيع
/// الاستعلام بـ <c>query_data</c> لتضمين أرقام حقيقية، ويلتزم بهوية المشروع (ألوان/خطوط) وجسر <c>jeem-page.js</c> للأجزاء القابلة
/// للتعديل. التعديل يمرّر HTML الحالي ويطلب نسخة كاملة محدّثة. الرد JSON: <c>{"reply": "...", "html": "..."}</c>.</summary>
public class FreeformBuilder
{
    private const int MaxIterations = 6;
    private readonly LlmRouter _router;
    private readonly DataProfileService _profiles;
    private readonly AnalyticsTools _tools;
    private readonly ColumnSemanticsService _semantics;
    private readonly ILogger<FreeformBuilder> _logger;

    public FreeformBuilder(LlmRouter router, DataProfileService profiles, AnalyticsTools tools, ColumnSemanticsService semantics, ILogger<FreeformBuilder> logger)
    { _router = router; _profiles = profiles; _tools = tools; _semantics = semantics; _logger = logger; }

    public async Task<FreeformResult> GenerateAsync(string message, string? currentHtml, IReadOnlyList<FreeformMessage> history, AnalyticsTools.SourceContext context, DeckBrand? brand, AppUser? user, CancellationToken ct, string kind = "design", FreeformTemplate? template = null, string? lang = null, IReadOnlyList<FreeformAttachment>? attachments = null, IReadOnlyList<FreeformQuery>? seedQueries = null)
    {
        var llm = await _router.ResolveStagedAsync(ct) ?? throw new InvalidOperationException("لا يوجد مزوّد موديل مهيأ — اضبط مفاتيح الموديلات من الإعدادات.");
        var profile = await _profiles.BuildAsync(context, ct);
        var hasData = profile.Tables.Count > 0;
        var profileText = "";
        if (hasData)
        {
            await _semantics.EnsureAnalyzedAsync(context.ProjectId, profile.Tables.Select(t => t.Table), TimeSpan.FromSeconds(20), ct);
            profileText = profile.ToPromptText(semantics: await _semantics.GetPromptNotesAsync(context.ProjectId, ct));
        }
        var system = BuildSystemPrompt(brand, hasData, profileText, kind, template, LanguageOf(lang, currentHtml));
        var userMsg = new StringBuilder();
        if (history.Count > 0)
        {
            userMsg.AppendLine("# المحادثة السابقة (للسياق)");
            foreach (var h in history.TakeLast(8)) userMsg.AppendLine($"{(h.Role == "user" ? "المستخدم" : "أنت")}: {Shorten(h.Text, 600)}");
            userMsg.AppendLine();
        }
        if (!string.IsNullOrWhiteSpace(currentHtml))
        {
            userMsg.AppendLine("# الصفحة الحالية (HTML كاملة) — عدّلها بحسب الطلب وأعدها كاملة");
            userMsg.AppendLine(currentHtml.Length > 180_000 ? currentHtml[..180_000] + "\n<!-- …اقتُطع -->" : currentHtml);
            userMsg.AppendLine();
        }
        userMsg.Append(FreeformAttachmentText.Build(attachments));
        userMsg.AppendLine("# الطلب").AppendLine(message.Trim());
        var tools = hasData ? _tools.BuildTools(context).Where(t => t.Name == "query_data").ToList() : null;
        var call = new StagedCall($"[تصميم حر/{KindLabel(kind)}] {Shorten(message, 40)}", system, userMsg.ToString(), tools, context, MaxIterations, false, user);
        // تسجيل كل استعلام ينفّذه الموديل مع نتيجته (مراقب محيطي على حلقة الأدوات)، وإلحاق مفتاحه بالرد ليقرأه من JEEM_DATA
        var queries = FreeformDataBlock.Parse(currentHtml); if (seedQueries is { Count: > 0 }) queries = queries.Concat(seedQueries).ToList(); var recorded = new List<FreeformQuery>(queries); var gate = new object();
        AnalyticsTools.ToolObserver.Value = (name, input, result, isError) =>
        {
            if (name != "query_data" || isError) return result;
            var sql = input["sql"]?.GetValue<string>(); if (string.IsNullOrWhiteSpace(sql)) return result;
            JsonNode? parsed; try { parsed = JsonNode.Parse(result); } catch (JsonException) { return result; }
            string key; lock (gate) { key = FreeformDataBlock.NextKey(recorded); recorded.Add(new FreeformQuery(key, sql.Trim(), parsed)); }
            return result + $"\n\n[مفتاح هذه النتيجة في الصفحة: {key} — للرسوم والأرقام استخدمه في بلوك data-jeem-chart/data-jeem-value (\"q\":\"{key}\")، أو اقرأ JEEM_DATA.{key}.rows في جافاسكربت الصفحة؛ لا تكتب الأرقام حرفيًا]";
        };
        StagedResult res; string? html; List<string> motionProblems = new();
        try
        {
            res = await llm.RunStagedAsync(call, ct);
            html = ExtractHtml(res.Json, res.Text);
            // «حركة»: فحص العقد بعد التوليد، ومحاولة ثانية واحدة بالأسباب؛ يُعتمد الأقل مشاكل
            if (kind == "motion" && html is not null && (motionProblems = FreeformMotionCheck.Validate(html)).Count > 0)
            {
                var retry = call with { Label = call.Label + " — تصحيح", UserMessage = userMsg.ToString() + FreeformMotionCheck.RetryMessage(html, motionProblems) };
                var res2 = await llm.RunStagedAsync(retry, ct);
                var html2 = ExtractHtml(res2.Json, res2.Text);
                var p2 = FreeformMotionCheck.Validate(html2);
                if (html2 is not null && p2.Count < motionProblems.Count) { res = res2; html = html2; motionProblems = p2; }
            }
        }
        finally { AnalyticsTools.ToolObserver.Value = null; }
        var reply = res.Json["reply"]?.GetValue<string>() ?? (html is null ? "لم أستطع إنتاج صفحة من هذا الطلب — وضّح أكثر." : "جاهزة.");
        if (motionProblems.Count > 0) reply = (reply + "\n⚠️ الحركة قد لا تعمل كما ينبغي: " + string.Join(" ", motionProblems.Take(2)) + " — اطلب «صحّح المشاهد» أو أعد المحاولة.").Trim();
        if (html is not null && recorded.Count > 0) html = FreeformDataBlock.Inject(html, recorded, DateTime.UtcNow);
        var blockProblems = html is null ? new List<string>() : FreeformBlockValidator.Validate(html, recorded.Count > 0 ? recorded : FreeformDataBlock.Parse(html));
        if (blockProblems.Count > 0) reply = (reply + "\n⚠️ بلوكات بيانات تحتاج مراجعة (ستظهر فارغة بسببها): " + string.Join(" ", blockProblems.Take(3))).Trim();
        if (html is not null && html.Length > FreeformStore.MaxHtmlBytes) html = html[..FreeformStore.MaxHtmlBytes];
        return new FreeformResult(reply, html, (res.Json["usedData"]?.GetValue<bool>() ?? false) || recorded.Count > queries.Count, recorded);
    }

    /// <summary>قيود وضع تعديل جزء: تُلحق بنظام الموديل بعد قواعد الصفحة وتغلبها (لا إعادة للصفحة كاملة).</summary>
    internal static string FragmentRules(string scopeLabel) => $$"""

        # وضع تعديل جزء واحد — قيود صارمة تغلب أي تعليمات سابقة
        - نطاق هذا الطلب: {{scopeLabel}} فقط. ستتلقى هذا الجزء وحده، فأعده هو وحده معدَّلًا في الحقل "fragment" — لا الصفحة كاملة ولا أي جزء آخر.
        - حافظ على القالب: لا تضف <style> ولا <script> ولا <link>، ولا تغيّر الوسم الجذر ولا أصنافه، ولا تضف سمات style تغيّر الألوان أو الخطوط أو التخطيط أو المقاسات.
        - استخدم فقط الأصناف (classes) الموجودة في هذا الجزء أو في أنماط الصفحة المرفقة؛ لا تخترع أصنافًا جديدة.
        - المسموح: تغيير النصوص والأرقام والصور، وإضافة أو حذف بنود من النوع الموجود نفسه (بطاقة، سطر جدول، نقطة) بنسخ بنيتها كما هي.
        - إضافة رسمة أو رقم من البيانات (أو استبدال رسمة ثابتة بها) مسموحة فقط ببلوك data-jeem-chart / data-jeem-value كما في قسم «بلوكات البيانات»
          لا بـ SVG يدوي، ويجوز على البلوك سمة style للمقاس والموضع فقط (width/height/position/top/left/inset-inline-start). "q" يجب أن يكون
          مفتاح نتيجة مسجَّلة (المذكورة أدناه أو الناتجة من query_data في هذا الطلب) وأعمدته من صفوفها حرفيًا — يُتحقق من ذلك آليًا.
        - حافظ على لغة الجزء واتجاهه، ولا تغيّر الصور أو الشعار إلا بطلب صريح.
        - لا تخترع أرقامًا: الرقم من البيانات (query_data) أو من كلام المستخدم، وإلا ضع «—» واذكر ذلك في الرد.
        - إن كان الطلب غامضًا (أي رقم؟ أي بطاقة؟) أو لا يخص هذا الجزء، فلا تعدّل شيئًا: اجعل "fragment" فارغًا واسأل في "reply".
        - "reply": سطر أو سطران بالعربية تذكر فيهما ما غيّرته تحديدًا (مثل: غيّرت العنوان وأرقام البطاقتين 2 و3).
        أعد JSON فقط بالشكل: {"reply": "...", "fragment": "<…الجزء المعدَّل كاملًا…>", "usedData": false}
        """;

    /// <summary>تعديل جزء واحد من الصفحة (شريحة أو عنصر): الموديل يرى الجزء وأنماط الصفحة للقراءة فقط ويعيد الجزء وحده، ويُتحقق منه
    /// (<see cref="FreeformScope.CheckFragment"/>) مع محاولة ثانية واحدة بسبب الرفض. بقية الصفحة لا تصل إلى الموديل أصلًا.</summary>
    public async Task<FreeformFragmentResult> GenerateFragmentAsync(string message, string fullHtml, string fragment, string scopeLabel, IReadOnlyList<FreeformMessage> history, AnalyticsTools.SourceContext context, DeckBrand? brand, AppUser? user, CancellationToken ct, string kind = "design", FreeformTemplate? template = null, IReadOnlyList<FreeformAttachment>? attachments = null)
    {
        var llm = await _router.ResolveStagedAsync(ct) ?? throw new InvalidOperationException("لا يوجد مزوّد موديل مهيأ — اضبط مفاتيح الموديلات من الإعدادات.");
        var profile = await _profiles.BuildAsync(context, ct);
        var hasData = profile.Tables.Count > 0; var profileText = "";
        if (hasData)
        {
            await _semantics.EnsureAnalyzedAsync(context.ProjectId, profile.Tables.Select(t => t.Table), TimeSpan.FromSeconds(20), ct);
            profileText = profile.ToPromptText(semantics: await _semantics.GetPromptNotesAsync(context.ProjectId, ct));
        }
        var system = BuildSystemPrompt(brand, hasData, profileText, kind, template, LanguageOf(null, fullHtml)) + FragmentRules(scopeLabel);
        var known = FreeformScope.KnownClasses(fullHtml);
        var baseMsg = new StringBuilder();
        if (history.Count > 0)
        {
            baseMsg.AppendLine("# المحادثة السابقة (للسياق)");
            foreach (var h in history.TakeLast(6)) baseMsg.AppendLine($"{(h.Role == "user" ? "المستخدم" : "أنت")}: {Shorten(h.Text, 400)}");
            baseMsg.AppendLine();
        }
        var css = FreeformScope.PageCss(fullHtml);
        if (css.Length > 0) baseMsg.AppendLine("# أنماط الصفحة (للقراءة فقط — لا تعدّلها ولا تكرّرها)").AppendLine(css).AppendLine();
        if (hasData) baseMsg.Append(FreeformBlockValidator.Describe(FreeformDataBlock.Parse(fullHtml)));
        var charts = FreeformScope.ChartElements(fragment);
        if (charts.Count > 0) baseMsg.AppendLine("# الرسوم الموجودة في هذا الجزء (يرسمها سكربت القالب — للاستبدال اتبع «استبدال رسمة موجودة في القالب»)")
            .AppendLine(string.Join("\n", charts.Select(c => "- " + c))).AppendLine();
        baseMsg.AppendLine($"# الجزء المطلوب تعديله ({scopeLabel})").AppendLine(fragment.Length > 120_000 ? fragment[..120_000] : fragment).AppendLine();
        baseMsg.Append(FreeformAttachmentText.Build(attachments));
        baseMsg.AppendLine("# الطلب").AppendLine(message.Trim());
        var tools = hasData ? _tools.BuildTools(context).Where(t => t.Name == "query_data").ToList() : null;
        var existing = FreeformDataBlock.Parse(fullHtml); var recorded = new List<FreeformQuery>(existing); var gate = new object();
        AnalyticsTools.ToolObserver.Value = (name, input, result, isError) =>
        {
            if (name != "query_data" || isError) return result;
            var sql = input["sql"]?.GetValue<string>(); if (string.IsNullOrWhiteSpace(sql)) return result;
            JsonNode? parsed; try { parsed = JsonNode.Parse(result); } catch (JsonException) { return result; }
            string key; lock (gate) { key = FreeformDataBlock.NextKey(recorded); recorded.Add(new FreeformQuery(key, sql.Trim(), parsed)); }
            return result + $"\n\n[مفتاح هذه النتيجة في الصفحة: {key} — استخدمه في بلوك data-jeem-chart/data-jeem-value (\"q\":\"{key}\")]";
        };
        try
        {
            string? feedback = null; string reply = ""; var usedData = false;
            for (var attempt = 0; attempt < 2; attempt++)
            {
                var msg = baseMsg.ToString() + (feedback is null ? "" : $"\n# رُفض ردك السابق\n{feedback}\nأعد الجزء نفسه ملتزمًا بالقيود.\n");
                var res = await llm.RunStagedAsync(new StagedCall($"[تصميم حر/{scopeLabel}] {Shorten(message, 40)}", system, msg, tools, context, MaxIterations, false, user), ct);
                reply = res.Json["reply"]?.GetValue<string>() ?? "";
                usedData |= res.Json["usedData"]?.GetValue<bool>() ?? false;
                var raw = res.Json["fragment"]?.GetValue<string>() ?? res.Json["html"]?.GetValue<string>();
                var (frag, error) = FreeformScope.CheckFragment(fragment, raw, known);
                if (error is null && frag is not null)
                {
                    // بلوكات البيانات: المفتاح والأعمدة يجب أن تطابق نتيجة مسجَّلة؛ محاولة ثانية بالسبب، ثم يُقبل الجزء مع تنبيه
                    var probs = FreeformBlockValidator.Validate(frag, recorded);
                    if (probs.Count > 0 && attempt == 0) { feedback = "بلوكات بيانات غير صالحة:\n- " + string.Join("\n- ", probs); continue; }
                    if (probs.Count > 0) reply = (reply + "\n⚠️ تنبيه: " + string.Join(" ", probs.Take(3))).Trim();
                }
                if (error is null) return new FreeformFragmentResult(string.IsNullOrWhiteSpace(reply) ? (frag is null ? "لم أعدّل شيئًا — وضّح المطلوب." : "عدّلت الجزء المطلوب.") : reply, frag, usedData || recorded.Count > existing.Count, recorded, null);
                feedback = error;
            }
            return new FreeformFragmentResult("لم أستطع التعديل دون تغيير تصميم القالب، فلم أغيّر شيئًا. السبب: " + feedback, null, false, recorded, feedback);
        }
        finally { AnalyticsTools.ToolObserver.Value = null; }
    }

    /// <summary>HTML من رد الموديل: حقل <c>html</c> في JSON، وإلا أول وثيقة <c>&lt;!doctype&gt;/&lt;html&gt;</c> في النص.</summary>
    public static string? ExtractHtml(JsonObject json, string text)
    {
        var h = json["html"]?.GetValue<string>();
        if (!string.IsNullOrWhiteSpace(h) && Regex.IsMatch(h, @"<(!doctype|html|body|div|section|main)\b", RegexOptions.IgnoreCase)) return EnsureDocument(h.Trim());
        var m = Regex.Match(text ?? "", @"<!doctype\s+html[\s\S]*</html\s*>|<html[\s\S]*</html\s*>", RegexOptions.IgnoreCase);
        return m.Success ? EnsureDocument(m.Value) : null;
    }
    private static string EnsureDocument(string h) => Regex.IsMatch(h, @"^\s*<!doctype", RegexOptions.IgnoreCase) || Regex.IsMatch(h, @"^\s*<html", RegexOptions.IgnoreCase) ? h
        : "<!doctype html>\n<html lang=\"ar\" dir=\"rtl\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"></head><body>\n" + h + "\n</body></html>";

    public static string KindLabel(string kind) => kind switch { "doc" => "مستند", "slides" => "شرائح", "dashboard" => "لوحة", "motion" => "حركة", _ => "تصميم" };

    /// <summary>تعليمات كل نوع مخرج (كما تختلف Docs/Slides/Design/Dashboard/Motion في Claude): البنية والتفاعل والطباعة.</summary>
    public static string KindInstructions(string kind) => kind switch
    {
        "doc" => """
            # النوع: مستند (Docs)
            - صفحة بشكل ورقة: حاوية بيضاء بعرض أقصى 820px في المنتصف بظل خفيف، هوامش 48px، خط 16px وسطر 1.8، عناوين مرقّمة عند الحاجة، جداول بحدود خفيفة، قوائم.
            - ابدأ بترويسة (العنوان، الجهة، التاريخ) وعند الحاجة جدول محتويات بروابط داخلية، واختم بتذييل.
            - `@media print { @page { size: A4; margin: 18mm } }` بلا ظلال ولا خلفيات، وفواصل صفحات مناسبة (`break-inside: avoid` للجداول).
            - لا عناصر تفاعلية إلا طيّ الأقسام الطويلة إن لزم. المحتوى هو الأساس: اكتب نصًا كاملًا لا عناوين فارغة.
            """,
        "slides" => """
            # النوع: شرائح (Slides)
            - عرض تقديمي داخل صفحة واحدة: كل شريحة `<section class="slide">` بنسبة 16:9 تملأ الشاشة، تُعرض واحدة في كل مرة.
            - تنقّل بالأسهم/المسافة والنقر، وأزرار «السابق/التالي» ورقم الشريحة وشريط تقدّم، ودعم اللمس (سحب) وزر ملء الشاشة.
            - 8–15 شريحة: غلاف، أقسام، محتوى بنقاط قليلة وخط كبير (العنوان ≥ 40px، النص ≥ 22px)، خاتمة. استخدم SVG للرسوم والأيقونات.
            - `@media print` تطبع كل شريحة في صفحة أفقية مستقلة.
            - عرّف دالة عامة `window.goToSlide(i)` (الفهرس من 0) تنتقل إلى الشريحة وتحدّث حالتها، وضمّن قبل `</body>`:
              `<script src="/js/jeem-slides.js"></script>` — تضيف المنصة بها شريط مصغّرات الشرائح أسفل الصفحة للتنقّل (لا تبنِ شريطًا بنفسك).
              الشريط ثابت أسفل الصفحة ويعلن ارتفاعه في `--jeem-strip-h` على `<html>`؛ احجز له مساحة: `body{padding-bottom:var(--jeem-strip-h,0px)}` أو اطرحه من ارتفاع الشريحة.
            """,
        "dashboard" => """
            # النوع: لوحة (Dashboard)
            - لوحة مؤشرات: صف بطاقات مؤشرات (القيمة الكبيرة + التسمية + التغيّر بسهم ولون مع نص)، ثم شبكة رسوم (أعمدة/خط/دائري) مرسومة بـ SVG مضمّن بكود صغير تكتبه أنت، وجدول تفصيلي، وفلاتر بسيطة (قائمة/أزرار) تعيد الرسم من البيانات المضمّنة في JS.
            - البيانات: إن توفرت أداة query_data فاستعلم عن الأرقام الحقيقية وضعها في مصفوفة JS واحدة في أعلى السكربت، واذكر المصدر تحت كل رسم. بلا بيانات اكتب قيمًا توضيحية مع شارة «بيانات توضيحية» واضحة.
            - ألوان الحالة ثابتة: أخضر للصعود الجيد، أحمر للهبوط، رمادي للثبات، ولا تعتمد على اللون وحده (أيقونة + نص). لا رسوم بمحورين. شبكة متجاوبة (4 → 2 → 1 أعمدة).
            """,
        "motion" => """
            # النوع: حركة (Motion)
            - عرض متحرك قصير (15–40 ثانية) من 3 إلى 6 مشاهد يحكي فكرة/عملية/أرقامًا. **محرّك التشغيل تقدّمه المنصة** (`/js/jeem-motion.js` يُضاف تلقائيًا):
              هو الذي يُظهر مشهدًا واحدًا فقط في كل لحظة ويشغّل الحركات ويضيف أزرار التشغيل/الإيقاف والإعادة وشريط الزمن والتكرار ويتعامل مع تقليل
              الحركة. فلا تكتب محرّكًا ولا مؤقّتات ولا أزرار تحكم ولا keyframes ولا JavaScript للحركة، ولا تضمّن jeem-motion.js بنفسك.
            - البنية الإلزامية:
              `<div data-jeem-motion data-loop="true"> <section class="jm-scene" data-slide data-duration="6" data-title="المشكلة"> … </section> … </div>`
              المسرح بنسبة 16:9 يملأ العرض (اضبط مقاسه وخلفيته بالـ CSS كما تشاء)، وكل مشهد `section.jm-scene` ابن مباشر له بمحتوى كامل مستقل
              (المحرّك يضعه فوق بعضه بموضع مطلق ويُظهر واحدًا فقط؛ لا تضع للمشاهد position/opacity/visibility/display بنفسك).
              `data-duration` بالثواني (4–8 عادة)، و`data-title` اسم قصير للمشهد.
              `data-loop="true"` تعود الحركة من البداية كلما انتهت؛ إن طلب المستخدم ألا تتكرر (تتوقف عند آخر مشهد) اكتب `data-loop="false"`.
            - الحركة بسمات فقط على العناصر داخل المشهد: `data-in="fade|up|down|start|end|zoom|pop|draw|count"` و`data-delay` (ثوانٍ من بداية المشهد،
              اختياري؛ بدونه تدخل العناصر بالتتابع) و`data-dur` (اختياري). `draw` لخطوط/أسهم SVG تُرسم، و`count` لرقم يُعدّ من الصفر: اكتب داخله القيمة
              النهائية كما تُعرض («1,250 ر.س»، «70٪»). لا تحرّك عنصرًا بغير هذه السمات.
            - المشهد مساحة ثابتة: نص قليل كبير واضح (عنوان قصير + سطر أو سطران أو 3 بنود على الأكثر) ورسم/أيقونات تشرحه، بلا تمرير داخله، وكل
              المحتوى داخل حدود المسرح على الجوال أيضًا (وحدات مرنة/clamp). لا صوت ولا مكتبات.
            """,
        _ => """
            # النوع: تصميم (Design)
            - صفحة أو نموذج أولي بصري حرّ: واجهة، صفحة هبوط، بطاقات، نموذج، رسم توضيحي… بحرية كاملة في التخطيط والتفاعل.
            - اهتم بالتفاصيل: تسلسل بصري واضح، مساحات متوازنة، حالات تحويم/تركيز، وتجاوب كامل.
            """,
    };

    /// <summary>لغة الصفحة: المطلوبة صراحة للتصميم الجديد، وإلا المستنتجة من صفحة التصميم القائم (`lang="en"`/`dir="ltr"`)، وإلا العربية.</summary>
    public static string LanguageOf(string? requested, string? currentHtml)
    {
        if (requested == "en" || requested == "ar") return requested;
        if (!string.IsNullOrEmpty(currentHtml))
        {
            var head = currentHtml.Length > 600 ? currentHtml[..600] : currentHtml;
            if (System.Text.RegularExpressions.Regex.IsMatch(head, "<html[^>]*\\blang=[\"']?en", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                || System.Text.RegularExpressions.Regex.IsMatch(head, "<html[^>]*\\bdir=[\"']?ltr", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) return "en";
        }
        return "ar";
    }

    public static string BuildSystemPrompt(DeckBrand? brand, bool hasData, string profileText, string kind = "design", FreeformTemplate? template = null, string? lang = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("""
            أنت مصمم ومطوّر واجهات داخل منصة «جيم». تكتب صفحة ويب واحدة كاملة ومستقلة (HTML + CSS + JS مضمّنة) من طلب المستخدم بالعربية الفصحى،
            كما تفعل مع artifacts: عرض تقديمي بتمرير، لوحة، صفحة تفاعلية، تقرير… بتصميم عصري نظيف، متجاوب (يعمل على 390px بلا تمرير أفقي)،
            RTL افتراضيًا (`<html lang="ar" dir="rtl">`)، بألوان متناسقة وخطوط عربية واضحة، ودعم `prefers-reduced-motion`، ووصولية أساسية.
            القواعد:
            - لا مكتبات خارجية إلا Google Fonts؛ كل شيء مضمّن. لا تضع روابط لصور خارجية؛ استخدم SVG مضمّنًا أو أشكال CSS.
            - لا تخترع أرقامًا أو حقائق. إن طُلبت أرقام من بيانات المشروع فاستخدم أداة query_data (إن وُجدت) وضمّن النتائج في الصفحة، وإلا اكتب قيمًا توضيحية مع تنبيه «بيانات توضيحية».
            - الأجزاء التي يريدها المستخدم قابلة للتعديل بعد النشر (مهام، قوائم، ملاحظات): ضمّن `<script src="/js/jeem-page.js"></script>` واحفظ بياناتها عبر
              `jeem.store.get(key)` / `jeem.store.set(key, value)` (تعمل داخل المنصة وبـ localStorage خارجها)، ولا تعتمد على إعادة نشر الصفحة.
            - صفحة **تجمع إدخالات من أشخاص كثيرين** (نموذج طلب، استبيان، تسجيل حضور، سجل متابعة، شكوى، حجز): احفظ كل إرسال سجلًا مستقلًا بـ
              `const R = jeem.records('اسم المجموعة'); await R.add({حقل: قيمة, …})` لا بـ jeem.store (عشرات المُدخلين يتعارضون على قيمة واحدة).
              اسم المجموعة عربي قصير بلا مسافات زائدة (الطلبات، الردود، الحضور)، وأسماء الحقول **ثابتة وواضحة بالعربية** والقيم الرقمية أرقامًا لا نصوصًا
              (التقييم: 4 لا "4 نجوم") والاختيارات من قائمة ثابتة — لأن السجلات تصير جدولًا في «المصادر» تُبنى عليه لوحة متابعة، وكل حقل عمود فيه.
              العرض: `R.list({order:'desc', limit:50})` → `{items:[{id, data, createdAt, createdBy, mine, canChange}], total, onlyOwn}`، و`R.watch(items => …)` للتحديث،
              و`R.update(id, {حقل})`/`R.remove(id)` لما `canChange` له. تعامل مع `e.code`: `sign_in_required` («سجّل الدخول بحساب منظمتك لإرسال النموذج»)،
              `rate_limited`، `forbidden`. لا تعرض بيانات الآخرين الشخصية حين `onlyOwn`. اعرض رسالة نجاح واضحة بعد الإرسال وامسح النموذج.
            - عند التعديل أعد الصفحة **كاملة** بعد التعديل (لا مقتطفًا).
            أعد JSON فقط: {"reply": "جملة أو اثنتان بالعربية عمّا صممته وما يمكن تعديله", "html": "<!doctype html>…", "usedData": true|false}
            """);
        sb.AppendLine(KindInstructions(kind));
        if (lang == "en") sb.AppendLine("""
            # لغة الصفحة: الإنجليزية
            - اكتب كل نصوص الصفحة بالإنجليزية (العناوين، الفقرات، التسميات، الأزرار، نصوص الرسوم) و`<html lang="en" dir="ltr">` بتخطيط LTR وخط لاتيني واضح؛ لا تترك نصًا عربيًا في الصفحة.
            - حقل `reply` في JSON يبقى بالعربية (رسالة للمستخدم داخل المنصة).
            """);
        if (template is not null) sb.AppendLine(FreeformTemplates.BriefFor(template, lang));
        if (brand is not null && !brand.IsEmpty)
        {
            sb.AppendLine("# هوية المشروع (التزم بها)");
            if (brand.Name is not null) sb.AppendLine($"- الجهة: {brand.Name}");
            if (brand.Primary is not null) sb.AppendLine($"- اللون الأساسي: {brand.Primary}");
            if (brand.Secondary is not null) sb.AppendLine($"- اللون الثانوي: {brand.Secondary}");
            if (brand.Accent is not null) sb.AppendLine($"- لون الإبراز: {brand.Accent}");
            if (brand.Font is not null) sb.AppendLine($"- الخط: '{brand.Font}' (ثم 'IBM Plex Sans Arabic', Tahoma, sans-serif)");
            if (brand.FontFaces is { Count: > 0 } faces) { sb.AppendLine("- ضمّن قواعد @font-face التالية كما هي في <style>:"); foreach (var f in faces) sb.AppendLine("  " + f.FontFaceCss()); }
            if (brand.Footer is not null) sb.AppendLine($"- تذييل الصفحة: {brand.Footer}");
            if (brand.Logo is not null) sb.AppendLine("- شعار الجهة متاح في المنصة؛ ضع عنصر <img data-jeem-logo alt=\"الشعار\"> حيث يناسب (ستملؤه المنصة) ولا تضمّن بياناته.");
        }
        if (hasData)
        {
            sb.AppendLine().AppendLine("# بيانات المشروع المتاحة عبر query_data").AppendLine(profileText);
            sb.AppendLine(SourcesNote);
            sb.AppendLine("""
                # عقد البيانات الحيّة (إلزامي عند استخدام query_data)
                - كل نتيجة من query_data تُحفظ تلقائيًا في الصفحة تحت مفتاح يُذكر في ذيل النتيجة (مثل JEEM_DATA.q1)، ويعرّفه المتصفح
                  قبل تشغيل سكربتاتك: `window.JEEM_DATA.q1 = {sql, rowCount, rows: [ {عمود: قيمة, …}, … ]}`.
                - لا تكتب أرقام النتائج حرفيًا في HTML. اكتب جافاسكربت في الصفحة يقرأ `JEEM_DATA.qN.rows` ويحسب ويملأ المؤشرات والجداول
                  ويرسم الرسوم عند التحميل (`DOMContentLoaded`). هكذا يستطيع المستخدم تحديث البيانات لاحقًا بلا إعادة بناء.
                - لا تُدرج بنفسك أي وسم `<script id="jeem-data">` أو `jeem-data-rt`؛ المنصة تحقنهما. عند التعديل أبقِهما كما هما إن وُجدا.
                - اجعل كل الرسم في دالة واحدة `render()` تقرأ من `JEEM_DATA` وتعيد بناء المؤشرات والجداول والرسوم من الصفر، واستدعِها عند
                  `DOMContentLoaded` وأيضًا في `document.addEventListener('jeem:data', render)`: الصفحة المنشورة تتلقى بيانات حيّة من المنصة
                  وهي مفتوحة (تحدّث `JEEM_DATA` ثم تبث هذا الحدث) فتتحدّث الأرقام بلا إعادة تحميل. ضمّن `<script src="/js/jeem-page.js"></script>` قبل سكربتاتك.
                - أرقام لا تأتي من الاستعلامات (توضيحية) تبقى بشارة «توضيحية» كما سبق.
                # بلوكات البيانات القياسية (الطريقة المفضّلة للرسوم والأرقام من البيانات)
                - لا ترسم الرسوم البيانية بنفسك (لا SVG ولا Canvas يدويًا). ضع بلوك رسم تملؤه المنصة بمحرّكها وبألوان القالب:
                  `<div data-jeem-chart='{"q":"q3","type":"bar","x":"<عمود التسميات>","y":"<عمود القيم>","title":"عنوان الرسم"}' style="height:280px"></div>`
                  الأنواع: bar | line | pie | donut | funnel | treemap | scatter | table (جدول بأعمدة النتيجة أو "cols")؛ و"y" مصفوفة أعمدة لسلاسل متعددة (bar/line، "stacked": true)،
                  و"fmt": number | compact | currency | percent، و"unit" اختياري. "q" مفتاح نتيجة query_data وأسماء الأعمدة كما في صفوفها.
                - لرقم مفرد من البيانات داخل النص أو بطاقة مؤشر: `<span data-jeem-value="q2.<العمود>" data-fmt="compact">—</span>`
                  (اختياري: data-agg="sum|avg|min|max|count" على كل الصفوف، أو data-row="0" لصف بعينه).
                - البلوكات تتحدّث وحدها مع تحديث البيانات وفي الصفحة المنشورة، فلا تحتاج render() خاصة بها. اجعل للبلوك ارتفاعًا مناسبًا داخل التصميم.
                """);
            sb.AppendLine(ReplaceChartRule);
        }
        else sb.AppendLine().AppendLine(NoDataNote);
        return sb.ToString();
    }

    /// <summary>«المصدر» في كلام المستخدم = الجداول المختارة للتصميم (لا مرفق في الرسالة) — بدونها طلب الموديل من المستخدم «إرسال الملف».</summary>
    internal const string SourcesNote = """
        # مصادر البيانات المختارة
        - الجداول أعلاه هي مصادر البيانات التي اختارها المستخدم لهذا التصميم (من «🗄️ المصادر»). حين يقول «المصدر» أو «ملف المصدر» أو «الملف المرفق»
          أو «البيانات» أو «الإكسل» فهو يقصدها، وهي متاحة لك الآن عبر query_data. لا تطلب منه إرسال الملف أو لصق محتواه — استعلم منها مباشرة.
        """;

    internal const string NoDataNote = """
        # لا توجد مصادر بيانات متاحة لهذا التصميم
        - إن طلب المستخدم رسومًا أو أرقامًا من «المصدر» أو «الملف»، فلا تخترعها: أخبره أن المصادر المختارة لهذا التصميم فارغة أو غير متاحة لحسابه،
          واقترح اختيارها من «🗄️ المصادر» أسفل المحادثة أو رفع الملف من «الملفات»، ولا تعدّل الصفحة.
        """;

    /// <summary>استبدال رسمة قالب يرسمها سكربت الصفحة: إخفاء الأصل لا حذفه (السكربت يبحث عنه بالمعرّف) وبلوك بالأصناف نفسها بعده.</summary>
    internal const string ReplaceChartRule = """
        # استبدال رسمة موجودة في القالب ببيانات المستخدم
        - رسوم القوالب غالبًا <svg>/<canvas> فارغة في المصدر يرسمها سكربت الصفحة عند التحميل (من كتل مثل chart-data). لا تحذف عنصرها ولا تعدّل السكربت
          ولا كتلة chart-data. أضف إلى العنصر الأصلي السمة data-jeem-replaced (تخفيه المنصة)، وضع بعده مباشرة بلوك
          <div class="<أصناف الأصل نفسها>" data-jeem-chart='{…}'></div> ليأخذ موضعه ومقاسه من أنماط القالب، بمواصفة من نتيجة query_data.
        - أرقام البطاقات المجاورة للرسمة (مؤشرات) تُربط بـ data-jeem-value على العنصر نفسه بدل كتابة الرقم.
        """;

    private static string Shorten(string s, int n) => s.Length > n ? s[..n] + "…" : s;
}
