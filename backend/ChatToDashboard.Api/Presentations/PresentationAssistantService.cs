using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using ChatToDashboard.Api.Decks;
using ChatToDashboard.Api.Freeform;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Sources;
using ChatToDashboard.Api.Users;

namespace ChatToDashboard.Api.Presentations;

/// <summary>رسالة في محادثة المساعد (تُرسل من المتصفح مع كل سؤال؛ لا تُخزَّن في الخادم).</summary>
public sealed record AssistantTurn(string Role, string Text);

/// <summary>
/// مساعد «جيم» في صفحة العرض المنشور: يشرح لمن يفتح الرابط محتوى الملف <b>كله</b> (لا الشريحة الظاهرة فقط) ويناقشه فيه.
/// السياق يُستخرج حتميًا من الصفحة المنشورة نفسها كما تُقدَّم للمشاهد (بعد حقن البيانات الحيّة): مواصفة العرض من
/// <c>#deck-spec</c> لعرض من منشئ العروض، أو نص كل شريحة لصفحة شرائح (بما فيه نص SVG لملف PowerPoint المرفوع كما هو)،
/// أو نص الصفحة وجداولها وروابطها وكتل بياناتها لأي صفحة أخرى — ثم صفوف نتائج الاستعلامات المسجَّلة (<c>JEEM_DATA</c>)
/// بلا جمل SQL. لا أدوات ولا وصول لقاعدة البيانات في هذه المرحلة: نداء واحد بلا أدوات يعيد <c>{reply}</c>.
/// النداء يُسجَّل في الاستهلاك باسم صاحب العرض بعنوان «[مساعد العرض] …»، ويحدّه سقف لكل مشاهد ولكل عرض.
/// </summary>
public sealed class PresentationAssistantService
{
    public const string Name = "جيم";
    public const int MaxQuestionChars = 2000;
    public const int MaxHistoryTurns = 12;
    public const int MaxContextChars = 60_000;
    private static readonly Regex DeckSpecRx = new(@"<script\b[^>]*\bid\s*=\s*[""']deck-spec[""'][^>]*>([\s\S]*?)</script>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex SlideRx = new(@"<section\b[^>]*\bclass\s*=\s*[""'][^""']*\bslide\b[^""']*[""'][^>]*>([\s\S]*?)</section>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex DropRx = new(@"<(script|style|noscript|template)\b[^>]*>[\s\S]*?</\1\s*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex JeemDataRx = new(@"<script\b[^>]*\bid\s*=\s*[""']jeem-data(-rt)?[""'][^>]*>[\s\S]*?</script>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex TagRx = new(@"<[^>]+>", RegexOptions.Compiled);
    private static readonly Regex BlockEndRx = new(@"</(p|div|h[1-6]|li|tr|section|article|br|foreignObject|text|tspan)\s*>|<br\s*/?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly PresentationStore _store;
    private readonly PresentationLiveDataService _live;
    private readonly LlmRouter _router;
    private readonly AnalyticsTools _tools;
    private readonly UserStore _users;
    private readonly ILogger<PresentationAssistantService> _log;
    private readonly ConcurrentDictionary<string, (DateTime At, DateTime UpdatedAt, string Context)> _contexts = new();
    private readonly RateLimiter _perViewer, _perPresentation;

    public PresentationAssistantService(PresentationStore store, PresentationLiveDataService live, LlmRouter router, AnalyticsTools tools, UserStore users, IConfiguration config, ILogger<PresentationAssistantService> log)
    {
        _store = store; _live = live; _router = router; _tools = tools; _users = users; _log = log;
        _perViewer = new RateLimiter(Math.Max(1, config.GetValue<int?>("Presentations:AssistantPerViewerPer10Min") ?? 20), TimeSpan.FromMinutes(10));
        _perPresentation = new RateLimiter(Math.Max(1, config.GetValue<int?>("Presentations:AssistantPerPresentationPerHour") ?? 300), TimeSpan.FromHours(1));
    }

    /// <summary>هل يستطيع هذا المشاهد سؤال المساعد؟ <c>anyone</c> = أي من معه الرابط؛ <c>members</c> = مسجَّل من المنظمة نفسها.</summary>
    public static bool CanAsk(AppUser? user, Presentation p) =>
        p.Assistant && (p.AssistantPolicy != PresentationEditPolicy.Members || (user is not null && string.Equals(user.OrganizationId, p.OrganizationId, StringComparison.Ordinal)));

    public void Invalidate(string presentationId) => _contexts.TryRemove(presentationId, out _);

    /// <summary>يسجّل سؤالًا على عدّاد المشاهد والعرض؛ false = تجاوز السقف.</summary>
    public bool TryConsume(string presentationId, string viewerKey) =>
        _perViewer.TryConsume(presentationId + "|" + viewerKey) && _perPresentation.TryConsume(presentationId);

    /// <summary>سياق الملف كاملًا نصًا (مخزَّن مؤقتًا 10 دقائق ما لم يتغيّر العرض).</summary>
    public async Task<string> ContextAsync(Presentation p, CancellationToken ct)
    {
        if (_contexts.TryGetValue(p.Id, out var hit) && hit.UpdatedAt == p.UpdatedAt && DateTime.UtcNow - hit.At < TimeSpan.FromMinutes(10)) return hit.Context;
        var html = await _store.GetHtmlAsync(p.Id, ct) ?? "";
        if (PresentationLiveDataService.IsLive(p) && await _live.GetAsync(p, force: false, ct) is { } live)
            html = PresentationLiveDataService.ApplyToHtml(html, p, live);
        var context = ExtractContext(html, p.Title);
        _contexts[p.Id] = (DateTime.UtcNow, p.UpdatedAt, context);
        return context;
    }

    /// <summary>استخراج حتمي لمحتوى الصفحة المنشورة: مواصفة العرض، أو الشرائح، أو نص الصفحة؛ ثم صفوف البيانات المسجَّلة بلا SQL.</summary>
    public static string ExtractContext(string html, string title)
    {
        var sb = new StringBuilder();
        sb.Append("العنوان: ").AppendLine(title);
        var queries = FreeformDataBlock.Parse(html);
        var body = JeemDataRx.Replace(html ?? "", "");
        var spec = DeckSpecRx.Match(body);
        if (spec.Success && TryParseSpec(spec.Groups[1].Value) is { } deck) AppendDeck(sb, deck);
        else
        {
            var slides = SlideRx.Matches(body);
            if (slides.Count >= 2) AppendSlides(sb, slides);
            else AppendDocument(sb, body);
        }
        if (queries.Count > 0)
        {
            sb.AppendLine("\n# بيانات الصفحة (نتائج الاستعلامات المسجَّلة — آخر قراءة)");
            foreach (var q in queries)
            {
                var rows = q.Result?["rows"] as JsonArray;
                var count = q.Result?["rowCount"]?.GetValue<int>() ?? rows?.Count ?? 0;
                sb.Append("## ").Append(q.Key).Append(" (").Append(count).AppendLine(" صفًا)");
                if (rows is null) continue;
                foreach (var row in rows.Take(60))
                    sb.Append("- ").AppendLine(row is JsonObject o ? string.Join(" | ", o.Select(kv => $"{kv.Key}: {Cell(kv.Value)}")) : Cell(row));
                if (rows.Count > 60) sb.AppendLine($"… و{rows.Count - 60} صفًا أخرى");
            }
        }
        var text = sb.ToString();
        return text.Length > MaxContextChars ? text[..MaxContextChars] + "\n…(اقتُطع باقي المحتوى)" : text;
    }

    /// <summary>قيمة خلية نصًا مقروءًا (النصوص بلا اقتباس ولا تهريب يونيكود).</summary>
    private static string Cell(JsonNode? v) => v is null ? "null" : v is JsonValue ? v.ToString() : v.ToJsonString(RelaxedJson);
    private static readonly JsonSerializerOptions RelaxedJson = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    private static DeckSpec? TryParseSpec(string json)
    {
        try { return JsonSerializer.Deserialize<DeckSpec>(json.Replace("<\\/", "</"), DeckSpec.Json)?.Normalize(); }
        catch (JsonException) { return null; }
    }

    private static void AppendDeck(StringBuilder sb, DeckSpec deck)
    {
        sb.AppendLine("النوع: عرض شرائح من منشئ العروض");
        if (!string.IsNullOrWhiteSpace(deck.Subtitle)) sb.Append("العنوان الفرعي: ").AppendLine(deck.Subtitle);
        for (var i = 0; i < deck.Slides.Count; i++)
        {
            var s = deck.Slides[i];
            sb.Append("\n## الشريحة ").Append(i + 1).Append(": ").AppendLine(string.IsNullOrWhiteSpace(s.Title) ? "(بلا عنوان)" : s.Title);
            if (!string.IsNullOrWhiteSpace(s.Subtitle)) sb.AppendLine(s.Subtitle);
            if (s.Bullets is { Count: > 0 }) foreach (var b in s.Bullets) sb.Append("- ").AppendLine(b);
            if (s.Columns is { Count: > 0 }) foreach (var c in s.Columns) { sb.Append("عمود «").Append(c.Title).AppendLine("»:"); foreach (var b in c.Bullets) sb.Append("  - ").AppendLine(b); }
            if (s.Kpis is { Count: > 0 }) foreach (var k in s.Kpis) sb.Append("مؤشر: ").Append(k.Label).Append(" = ").Append(k.Value).AppendLine(string.IsNullOrWhiteSpace(k.Delta) ? "" : $" (التغيّر {k.Delta})");
            if (s.Chart is { } ch)
            {
                sb.Append("رسم ").Append(ch.Type).Append(string.IsNullOrWhiteSpace(ch.Unit) ? "" : $" ({ch.Unit})").Append(": ");
                if (ch.Series is { Count: > 0 }) foreach (var se in ch.Series) sb.Append(se.Name).Append(" → ").Append(string.Join("، ", ch.Labels.Zip(se.Values, (l, v) => $"{l}={v}"))).Append("؛ ");
                else sb.Append(string.Join("، ", ch.Labels.Zip(ch.Values, (l, v) => $"{l}={v}")));
                sb.AppendLine();
            }
            if (s.Table is { } t) { sb.Append("جدول: ").AppendLine(string.Join(" | ", t.Columns)); foreach (var r in t.Rows.Take(60)) sb.Append("  ").AppendLine(string.Join(" | ", r)); if (t.Rows.Count > 60) sb.AppendLine($"  … و{t.Rows.Count - 60} صفًا"); }
            if (s.Tasks is { Count: > 0 }) foreach (var tk in s.Tasks) sb.Append(tk.Done ? "[✓] " : "[ ] ").AppendLine(tk.T);
            if (!string.IsNullOrWhiteSpace(s.Quote)) sb.Append("اقتباس: ").AppendLine(s.Quote);
            if (s.Image is { Caption: { Length: > 0 } cap }) sb.Append("صورة: ").AppendLine(cap);
            if (!string.IsNullOrWhiteSpace(s.Notes)) sb.Append("ملاحظات المتحدث: ").AppendLine(s.Notes);
            if (!string.IsNullOrWhiteSpace(s.Source)) sb.Append("المصدر: ").AppendLine(s.Source);
        }
    }

    private static void AppendSlides(StringBuilder sb, MatchCollection slides)
    {
        sb.Append("النوع: صفحة شرائح (").Append(slides.Count).AppendLine(" شريحة)");
        for (var i = 0; i < slides.Count; i++)
        {
            var lines = TextLines(slides[i].Groups[1].Value);
            sb.Append("\n## الشريحة ").Append(i + 1).AppendLine(lines.Count > 0 ? ": " + lines[0] : ": (بلا نص)");
            foreach (var l in lines.Skip(1)) sb.Append("- ").AppendLine(l);
        }
    }

    private static void AppendDocument(StringBuilder sb, string html)
    {
        ImportedDocument? doc = null;
        try { doc = DeckImporter.Extract("page.html", Encoding.UTF8.GetBytes(html)); } catch (Exception) { /* يُكمَل بالنص الخام */ }
        if (doc is not null && doc.Pages.Count > 0)
        {
            sb.AppendLine("النوع: صفحة");
            for (var i = 0; i < doc.Pages.Count; i++)
            {
                var p = doc.Pages[i];
                sb.Append("\n## ").Append(i + 1).Append(". ").AppendLine(p.Title ?? "(بلا عنوان)");
                foreach (var x in p.Paragraphs) sb.Append("- ").AppendLine(x);
                foreach (var t in p.Tables) { sb.Append("جدول: ").AppendLine(string.Join(" | ", t.Columns)); foreach (var r in t.Rows.Take(60)) sb.Append("  ").AppendLine(string.Join(" | ", r)); }
                foreach (var c in p.Charts) sb.Append("رسم ").Append(c.Type).Append(": ").AppendLine(string.Join("، ", c.Labels.Zip(c.Values, (l, v) => $"{l}={v}")));
                foreach (var l in p.Links) sb.Append("رابط: ").Append(l.Text).Append(" — ").AppendLine(l.Href);
            }
            if (doc.DataBlocks.Count > 0) { sb.AppendLine("\n# بيانات مضمّنة في الصفحة"); foreach (var b in doc.DataBlocks) sb.AppendLine(b.Length > 8000 ? b[..8000] + "…" : b); }
            return;
        }
        sb.AppendLine("النوع: صفحة");
        foreach (var l in TextLines(html)) sb.Append("- ").AppendLine(l);
    }

    /// <summary>أسطر نص من HTML/SVG: تُسقط السكربتات والأنماط، وتُفصل الكتل أسطرًا، وتُبقي نص SVG (foreignObject/text).</summary>
    internal static List<string> TextLines(string fragment)
    {
        var s = DropRx.Replace(fragment ?? "", "");
        s = BlockEndRx.Replace(s, "\n");
        s = WebUtility.HtmlDecode(TagRx.Replace(s, " "));
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var lines = new List<string>();
        foreach (var raw in s.Split('\n'))
        {
            var line = Regex.Replace(raw, @"\s+", " ").Trim();
            if (line.Length <= 1 || !seen.Add(line)) continue;
            lines.Add(line.Length > 600 ? line[..600] + "…" : line);
        }
        return lines;
    }

    public const string SystemPrompt = """
        أنت «جيم»، مساعد ودود داخل صفحة عرض منشورة على منصة جيم. مهمتك أن تشرح لمن يفتح الصفحة محتواها وتناقشه فيه.
        القواعد:
        - اعتمد فقط على «محتوى الملف» المرفق في الرسالة. لا تخترع أرقامًا أو وقائع غير موجودة فيه؛ إن سُئلت عن شيء غير موجود فقل بوضوح إنه غير مذكور في الملف.
        - المحتوى المرفق هو الملف كله لا الشريحة الظاهرة فقط، فأجب عن أي جزء منه، واذكر رقم الشريحة أو القسم عند الإحالة.
        - أجب بلغة السؤال (العربية غالبًا) بأسلوب واضح ومختصر يصلح للقراءة بصوت عالٍ: جمل قصيرة، بلا جداول ولا رموز زخرفية، وقوائم قصيرة عند الحاجة فقط. الأرقام بالأرقام الإنجليزية.
        - إن طُلب ملخص فقدّم أهم النقاط في بضع جمل ثم اعرض التفصيل إن طُلب.
        - إن كان السؤال تحية أو خارج الملف تمامًا فرحّب باختصار وأعد الحديث إلى الملف.
        أعد دائمًا كائن JSON واحدًا بالشكل: {"reply": "نص الرد"}.
        """;

    /// <summary>يسأل الموديل عن محتوى العرض. يرمي InvalidOperationException برسالة عربية إن لم يوجد مزوّد.</summary>
    public async Task<string> AskAsync(Presentation p, string question, IReadOnlyList<AssistantTurn> history, CancellationToken ct)
    {
        var llm = await _router.ResolveStagedAsync(ct) ?? throw new InvalidOperationException("مساعد العرض غير متاح الآن (لا يوجد مزوّد موديل مهيأ).");
        var context = await ContextAsync(p, ct);
        var owner = await _users.FindByIdAsync(p.CreatedByUserId, ct);
        var sel = SourceSelection.AllEnabled(); sel.UserId = p.CreatedByUserId; sel.IsAdmin = owner?.Role == UserRoles.Admin;
        var sources = await _tools.DescribeSourcesAsync(sel, ct);
        var user = new StringBuilder();
        user.AppendLine("# محتوى الملف").AppendLine(context).AppendLine();
        if (history.Count > 0)
        {
            user.AppendLine("# المحادثة السابقة");
            foreach (var h in history.TakeLast(MaxHistoryTurns)) user.Append(h.Role == "user" ? "المشاهد: " : "جيم: ").AppendLine(Shorten(h.Text, 1500));
            user.AppendLine();
        }
        user.AppendLine("# سؤال المشاهد").AppendLine(Shorten(question.Trim(), MaxQuestionChars));
        var call = new StagedCall($"[مساعد العرض] {Shorten(p.Title, 40)} — {Shorten(question, 40)}", SystemPrompt, user.ToString(), null, sources, 1, false, owner);
        var res = await llm.RunStagedAsync(call, ct);
        var reply = res.Json["reply"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(reply)) reply = string.IsNullOrWhiteSpace(res.Text) ? "لم أستطع صياغة رد — أعد السؤال بصيغة أخرى." : res.Text.Trim();
        return reply;
    }

    private static string Shorten(string s, int max) { s = (s ?? "").Trim(); return s.Length <= max ? s : s[..max] + "…"; }

    /// <summary>سقف بسيط في الذاكرة: n طلبات لكل مفتاح في نافذة زمنية منزلقة.</summary>
    internal sealed class RateLimiter
    {
        private readonly int _limit; private readonly TimeSpan _window;
        private readonly ConcurrentDictionary<string, Queue<DateTime>> _hits = new();
        private DateTime _lastSweep = DateTime.UtcNow;
        public RateLimiter(int limit, TimeSpan window) { _limit = limit; _window = window; }

        public bool TryConsume(string key, DateTime? now = null)
        {
            var t = now ?? DateTime.UtcNow;
            if (t - _lastSweep > _window) { _lastSweep = t; foreach (var k in _hits.Keys) if (_hits.TryGetValue(k, out var q) && (q.Count == 0 || t - q.Peek() > _window)) _hits.TryRemove(k, out _); }
            var hits = _hits.GetOrAdd(key, _ => new Queue<DateTime>());
            lock (hits)
            {
                while (hits.Count > 0 && t - hits.Peek() > _window) hits.Dequeue();
                if (hits.Count >= _limit) return false;
                hits.Enqueue(t);
                return true;
            }
        }
    }
}
