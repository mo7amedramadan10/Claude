using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using ChatToDashboard.Api.Build;
using ChatToDashboard.Api.Decks;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Semantics;
using ChatToDashboard.Api.Users;

namespace ChatToDashboard.Api.Freeform;

public sealed record FreeformResult(string Reply, string? Html, bool UsedData);

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

    public async Task<FreeformResult> GenerateAsync(string message, string? currentHtml, IReadOnlyList<FreeformMessage> history, AnalyticsTools.SourceContext context, DeckBrand? brand, AppUser? user, CancellationToken ct)
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
        var system = BuildSystemPrompt(brand, hasData, profileText);
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
        var call = new StagedCall($"[تصميم حر] {Shorten(message, 40)}", system, userMsg.ToString(), tools, context, MaxIterations, false, user);
        var res = await llm.RunStagedAsync(call, ct);
        var html = ExtractHtml(res.Json, res.Text);
        var reply = res.Json["reply"]?.GetValue<string>() ?? (html is null ? "لم أستطع إنتاج صفحة من هذا الطلب — وضّح أكثر." : "جاهزة.");
        if (html is not null && html.Length > FreeformStore.MaxHtmlBytes) html = html[..FreeformStore.MaxHtmlBytes];
        return new FreeformResult(reply, html, res.Json["usedData"]?.GetValue<bool>() ?? false);
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

    public static string BuildSystemPrompt(DeckBrand? brand, bool hasData, string profileText)
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
        if (hasData) sb.AppendLine().AppendLine("# بيانات المشروع المتاحة عبر query_data").AppendLine(profileText);
        return sb.ToString();
    }

    private static string Shorten(string s, int n) => s.Length > n ? s[..n] + "…" : s;
}
