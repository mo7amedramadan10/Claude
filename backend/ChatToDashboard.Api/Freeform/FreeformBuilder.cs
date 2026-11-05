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

    public async Task<FreeformResult> GenerateAsync(string message, string? currentHtml, IReadOnlyList<FreeformMessage> history, AnalyticsTools.SourceContext context, DeckBrand? brand, AppUser? user, CancellationToken ct, string kind = "design", FreeformTemplate? template = null)
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
        var system = BuildSystemPrompt(brand, hasData, profileText, kind, template);
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
        userMsg.AppendLine("# الطلب").AppendLine(message.Trim());
        var tools = hasData ? _tools.BuildTools(context).Where(t => t.Name == "query_data").ToList() : null;
        var call = new StagedCall($"[تصميم حر/{KindLabel(kind)}] {Shorten(message, 40)}", system, userMsg.ToString(), tools, context, MaxIterations, false, user);
        // تسجيل كل استعلام ينفّذه الموديل مع نتيجته (مراقب محيطي على حلقة الأدوات)، وإلحاق مفتاحه بالرد ليقرأه من JEEM_DATA
        var queries = FreeformDataBlock.Parse(currentHtml); var recorded = new List<FreeformQuery>(queries); var gate = new object();
        AnalyticsTools.ToolObserver.Value = (name, input, result, isError) =>
        {
            if (name != "query_data" || isError) return result;
            var sql = input["sql"]?.GetValue<string>(); if (string.IsNullOrWhiteSpace(sql)) return result;
            JsonNode? parsed; try { parsed = JsonNode.Parse(result); } catch (JsonException) { return result; }
            string key; lock (gate) { key = FreeformDataBlock.NextKey(recorded); recorded.Add(new FreeformQuery(key, sql.Trim(), parsed)); }
            return result + $"\n\n[مفتاح هذه النتيجة في الصفحة: JEEM_DATA.{key} — اقرأ الصفوف من JEEM_DATA.{key}.rows في جافاسكربت الصفحة ولا تكتب الأرقام حرفيًا]";
        };
        StagedResult res;
        try { res = await llm.RunStagedAsync(call, ct); }
        finally { AnalyticsTools.ToolObserver.Value = null; }
        var html = ExtractHtml(res.Json, res.Text);
        var reply = res.Json["reply"]?.GetValue<string>() ?? (html is null ? "لم أستطع إنتاج صفحة من هذا الطلب — وضّح أكثر." : "جاهزة.");
        if (html is not null && recorded.Count > 0) html = FreeformDataBlock.Inject(html, recorded, DateTime.UtcNow);
        if (html is not null && html.Length > FreeformStore.MaxHtmlBytes) html = html[..FreeformStore.MaxHtmlBytes];
        return new FreeformResult(reply, html, (res.Json["usedData"]?.GetValue<bool>() ?? false) || recorded.Count > queries.Count, recorded);
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
            """,
        "dashboard" => """
            # النوع: لوحة (Dashboard)
            - لوحة مؤشرات: صف بطاقات مؤشرات (القيمة الكبيرة + التسمية + التغيّر بسهم ولون مع نص)، ثم شبكة رسوم (أعمدة/خط/دائري) مرسومة بـ SVG مضمّن بكود صغير تكتبه أنت، وجدول تفصيلي، وفلاتر بسيطة (قائمة/أزرار) تعيد الرسم من البيانات المضمّنة في JS.
            - البيانات: إن توفرت أداة query_data فاستعلم عن الأرقام الحقيقية وضعها في مصفوفة JS واحدة في أعلى السكربت، واذكر المصدر تحت كل رسم. بلا بيانات اكتب قيمًا توضيحية مع شارة «بيانات توضيحية» واضحة.
            - ألوان الحالة ثابتة: أخضر للصعود الجيد، أحمر للهبوط، رمادي للثبات، ولا تعتمد على اللون وحده (أيقونة + نص). لا رسوم بمحورين. شبكة متجاوبة (4 → 2 → 1 أعمدة).
            """,
        "motion" => """
            # النوع: حركة (Motion)
            - مشهد متحرك قصير (10–30 ثانية) بـ SVG + CSS animations أو JS (requestAnimationFrame) يشرح فكرة/عملية/أرقامًا: عناصر تظهر بترتيب، أسهم تتحرك، أرقام تُعدّ تصاعديًا.
            - عناصر تحكم ثابتة أسفل المشهد: تشغيل/إيقاف، إعادة، شريط زمن، وخيار تكرار. المشهد بنسبة 16:9 يملأ العرض.
            - احترم `prefers-reduced-motion`: عند تفعيله اعرض الحالة النهائية ثابتة. لا صوت. لا اعتماد على مكتبات.
            """,
        _ => """
            # النوع: تصميم (Design)
            - صفحة أو نموذج أولي بصري حرّ: واجهة، صفحة هبوط، بطاقات، نموذج، رسم توضيحي… بحرية كاملة في التخطيط والتفاعل.
            - اهتم بالتفاصيل: تسلسل بصري واضح، مساحات متوازنة، حالات تحويم/تركيز، وتجاوب كامل.
            """,
    };

    public static string BuildSystemPrompt(DeckBrand? brand, bool hasData, string profileText, string kind = "design", FreeformTemplate? template = null)
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
            - عند التعديل أعد الصفحة **كاملة** بعد التعديل (لا مقتطفًا).
            أعد JSON فقط: {"reply": "جملة أو اثنتان بالعربية عمّا صممته وما يمكن تعديله", "html": "<!doctype html>…", "usedData": true|false}
            """);
        sb.AppendLine(KindInstructions(kind));
        if (template is not null) sb.AppendLine(FreeformTemplates.BriefFor(template));
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
                """);
        }
        return sb.ToString();
    }

    private static string Shorten(string s, int n) => s.Length > n ? s[..n] + "…" : s;
}
