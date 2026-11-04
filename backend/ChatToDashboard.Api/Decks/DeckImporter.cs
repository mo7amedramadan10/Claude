using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Users;
using DocumentFormat.OpenXml.Packaging;
using UglyToad.PdfPig;
using A = DocumentFormat.OpenXml.Drawing;
using C = DocumentFormat.OpenXml.Drawing.Charts;
using P = DocumentFormat.OpenXml.Presentation;

namespace ChatToDashboard.Api.Decks;

/// <summary>صفحة/شريحة مستخرجة من ملف خارجي قبل تحويلها إلى شرائح العرض.</summary>
public sealed class ImportedPage
{
    public string? Title { get; set; }
    public List<string> Paragraphs { get; } = new();
    public List<DeckTable> Tables { get; } = new();
    public List<DeckChart> Charts { get; } = new();
    public List<string> Images { get; } = new();
    /// <summary>روابط خارجية (نص ← عنوان) تُجمع في شريحة روابط بدل أن تتناثر كنقاط.</summary>
    public List<(string Text, string Href)> Links { get; } = new();
    public string? Notes { get; set; }
    public bool IsEmpty => Title is null && Paragraphs.Count == 0 && Tables.Count == 0 && Charts.Count == 0 && Images.Count == 0 && Links.Count == 0;
}

/// <summary>الوثيقة المستخرجة. <see cref="DataBlocks"/>: كتل JSON مضمّنة في الصفحة ونصوص حرفية من سكربتاتها (بلا تنفيذ أي كود) —
/// تُستخدم فقط مدخلًا لوضع «فهم المحتوى بالذكاء الاصطناعي» لأن صفحات التطبيقات التفاعلية تحمل محتواها الحقيقي في بياناتها لا في وسومها.</summary>
public sealed record ImportedDocument(string Kind, string Title, List<ImportedPage> Pages, List<string> Warnings)
{
    public List<string> DataBlocks { get; } = new();
}

/// <summary>استيراد ملف موجود (PowerPoint أو PDF أو صفحة HTML) إلى مواصفة عرض قابلة للتعديل:
/// استخراج حتمي بلا موديل (العناوين، الفقرات/النقاط، الجداول، الصور، ورسوم PowerPoint البيانية)، ثم تحويل كل صفحة إلى شريحة
/// أو أكثر بتخطيط مناسب، واختياريًا نداء موديل واحد بلا أدوات يعيد ترتيب المحتوى في تخطيطات أفضل (مؤشرات من الأرقام، نقاط موجزة).</summary>
public class DeckImporter
{
    public const int MaxBytes = 25 * 1024 * 1024;
    private const int MaxImageBytes = 600 * 1024;
    private const int MaxImagesPerDeck = 20;
    private readonly LlmRouter _router;
    private readonly ILogger<DeckImporter> _logger;
    public DeckImporter(LlmRouter router, ILogger<DeckImporter> logger) { _router = router; _logger = logger; }

    public static string? KindOf(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch { ".pptx" => "pptx", ".pdf" => "pdf", ".html" or ".htm" => "html", _ => null };
    }

    // ================= الاستخراج =================
    public static ImportedDocument Extract(string fileName, byte[] bytes)
    {
        var kind = KindOf(fileName) ?? throw new InvalidOperationException("الصيغة غير مدعومة — ارفع ملف .pptx أو .pdf أو .html.");
        var title = Path.GetFileNameWithoutExtension(fileName).Replace('_', ' ').Replace('-', ' ').Trim();
        var warnings = new List<string>();
        var dataBlocks = new List<string>();
        var pages = kind switch { "pptx" => ExtractPptx(bytes, warnings), "pdf" => ExtractPdf(bytes, warnings), _ => ExtractHtml(bytes, warnings, dataBlocks, ref title) };
        pages = pages.Where(p => !p.IsEmpty).ToList();
        if (pages.Count == 0 && dataBlocks.Count == 0) warnings.Add("لم يُعثر على محتوى نصي قابل للاستخراج (قد يكون الملف صورًا ممسوحة).");
        var doc = new ImportedDocument(kind, title, pages, warnings);
        doc.DataBlocks.AddRange(dataBlocks);
        return doc;
    }

    private static List<ImportedPage> ExtractPptx(byte[] bytes, List<string> warnings)
    {
        var pages = new List<ImportedPage>();
        using var ms = new MemoryStream(bytes);
        using var doc = PresentationDocument.Open(ms, false);
        var pres = doc.PresentationPart ?? throw new InvalidOperationException("ملف PowerPoint غير صالح.");
        var ids = pres.Presentation.SlideIdList?.Elements<P.SlideId>() ?? Enumerable.Empty<P.SlideId>();
        var imagesLeft = MaxImagesPerDeck;
        foreach (var sid in ids)
        {
            if (pres.GetPartById(sid.RelationshipId!) is not SlidePart slide) continue;
            var page = new ImportedPage();
            var tree = slide.Slide?.CommonSlideData?.ShapeTree; if (tree is null) continue;
            foreach (var el in tree.ChildElements)
            {
                switch (el)
                {
                    case P.Shape sh:
                    {
                        var ph = sh.NonVisualShapeProperties?.ApplicationNonVisualDrawingProperties?.GetFirstChild<P.PlaceholderShape>();
                        var phType = ph?.Type?.Value; var isTitle = phType == P.PlaceholderValues.Title || phType == P.PlaceholderValues.CenteredTitle;
                        var paras = sh.TextBody?.Elements<A.Paragraph>().SelectMany(ParagraphLines).Where(t => t.Length > 0).ToList() ?? new();
                        if (paras.Count == 0) break;
                        if (isTitle && page.Title is null) { page.Title = string.Join(" ", paras); break; }
                        if (phType == P.PlaceholderValues.SlideNumber || phType == P.PlaceholderValues.Footer || phType == P.PlaceholderValues.DateAndTime) break;
                        page.Paragraphs.AddRange(paras);
                        break;
                    }
                    case P.GraphicFrame gf:
                    {
                        if (gf.Graphic?.GraphicData?.GetFirstChild<A.Table>() is { } tbl) { var t = TableOf(tbl); if (t.Rows.Count > 0) page.Tables.Add(t); }
                        else if (gf.Graphic?.GraphicData?.GetFirstChild<C.ChartReference>() is { } cref && cref.Id?.Value is { } rid && slide.GetPartById(rid) is ChartPart cp)
                        {
                            var ch = ChartOf(cp); if (ch is not null) page.Charts.Add(ch); else warnings.Add("رسم بياني لم يمكن قراءة بياناته فتُرك.");
                        }
                        break;
                    }
                    case P.Picture pic when imagesLeft > 0:
                    {
                        var rid = pic.BlipFill?.Blip?.Embed?.Value; if (rid is null) break;
                        if (slide.GetPartById(rid) is ImagePart ip && ImageDataUrl(ip) is { } url) { page.Images.Add(url); imagesLeft--; }
                        break;
                    }
                }
            }
            if (page.Title is null && page.Paragraphs.Count > 0 && page.Paragraphs[0].Length <= 80) { page.Title = page.Paragraphs[0]; page.Paragraphs.RemoveAt(0); }
            page.Notes = slide.NotesSlidePart?.NotesSlide?.InnerText is { Length: > 0 } n ? Clean(n) : null;
            pages.Add(page);
        }
        if (imagesLeft == 0) warnings.Add($"اكتُفي بأول {MaxImagesPerDeck} صورة.");
        return pages;
    }

    private static string ParagraphText(A.Paragraph p) => Clean(string.Join(" ", ParagraphLines(p)));

    /// <summary>فقرة PowerPoint قد تحمل عدة أسطر بفواصل أسطر (كما يكتبها بنّاؤنا)؛ كل سطر نقطة مستقلة بلا رمز التنقيط الأمامي.</summary>
    private static IEnumerable<string> ParagraphLines(A.Paragraph p)
    {
        var sb = new StringBuilder();
        foreach (var e in p.ChildElements) { if (e is A.Run r) sb.Append(r.Text?.Text); else if (e is A.Break) sb.Append('\n'); else if (e is A.Field f) sb.Append(f.Text?.Text); }
        foreach (var line in sb.ToString().Split('\n'))
        {
            var t = Regex.Replace(Clean(line), @"^([•\-–●▪○■]|\d+[\.\)])\s+", "");
            if (t.Length > 0) yield return t;
        }
    }

    private static DeckTable TableOf(A.Table tbl)
    {
        var rows = tbl.Elements<A.TableRow>().Select(r => r.Elements<A.TableCell>().Select(c => Clean(string.Join(" ", c.TextBody?.Elements<A.Paragraph>().Select(ParagraphText) ?? Enumerable.Empty<string>()))).ToList()).Where(r => r.Any(x => x.Length > 0)).ToList();
        if (rows.Count == 0) return new DeckTable();
        return new DeckTable { Columns = rows[0], Rows = rows.Skip(1).Take(40).ToList() };
    }

    /// <summary>أول سلسلة من رسم PowerPoint (أعمدة/خط/دائري) → تسميات وقيم.</summary>
    private static DeckChart? ChartOf(ChartPart cp)
    {
        var plot = cp.ChartSpace?.GetFirstChild<C.Chart>()?.PlotArea; if (plot is null) return null;
        string type = "bar"; DocumentFormat.OpenXml.OpenXmlElement? series = null;
        foreach (var child in plot.ChildElements)
        {
            switch (child)
            {
                case C.BarChart b: series = b.Elements<C.BarChartSeries>().FirstOrDefault(); type = "bar"; break;
                case C.LineChart l: series = l.Elements<C.LineChartSeries>().FirstOrDefault(); type = "line"; break;
                case C.PieChart p: series = p.Elements<C.PieChartSeries>().FirstOrDefault(); type = "pie"; break;
                case C.DoughnutChart d: series = d.Elements<C.PieChartSeries>().FirstOrDefault(); type = "pie"; break;
                case C.AreaChart a: series = a.Elements<C.AreaChartSeries>().FirstOrDefault(); type = "line"; break;
            }
            if (series is not null) break;
        }
        if (series is null) return null;
        var labels = series.GetFirstChild<C.CategoryAxisData>()?.Descendants<C.StringPoint>().Select(p => p.NumericValue?.Text ?? "").ToList()
                     ?? series.GetFirstChild<C.CategoryAxisData>()?.Descendants<C.NumericPoint>().Select(p => p.NumericValue?.Text ?? "").ToList() ?? new();
        if (labels.Count == 0) labels = series.GetFirstChild<C.CategoryAxisData>()?.Descendants<C.NumericPoint>().Select(p => p.NumericValue?.Text ?? "").ToList() ?? new();
        var values = series.GetFirstChild<C.Values>()?.Descendants<C.NumericPoint>().Select(p => double.TryParse(p.NumericValue?.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0).ToList() ?? new();
        if (values.Count == 0) return null;
        if (labels.Count < values.Count) labels.AddRange(Enumerable.Range(labels.Count + 1, values.Count - labels.Count).Select(i => i.ToString()));
        return new DeckChart { Type = type, Labels = labels.Take(values.Count).ToList(), Values = values };
    }

    private static string? ImageDataUrl(ImagePart ip)
    {
        var ct = ip.ContentType; if (ct is not ("image/png" or "image/jpeg" or "image/gif" or "image/webp")) return null;
        using var s = ip.GetStream(); using var ms = new MemoryStream(); s.CopyTo(ms);
        if (ms.Length > MaxImageBytes || ms.Length == 0) return null;
        return $"data:{ct};base64,{Convert.ToBase64String(ms.ToArray())}";
    }

    private static List<ImportedPage> ExtractPdf(byte[] bytes, List<string> warnings)
    {
        var pages = new List<ImportedPage>();
        using var pdf = PdfDocument.Open(bytes);
        foreach (var p in pdf.GetPages())
        {
            var lines = new List<string>(); var cur = new StringBuilder(); double? lastY = null; double? lastH = null;
            foreach (var w in p.GetWords())
            {
                var y = w.BoundingBox.Bottom; var h = w.BoundingBox.Height;
                if (lastY is not null && Math.Abs(y - lastY.Value) > Math.Max(2, (lastH ?? h) * 0.6)) { lines.Add(cur.ToString()); cur.Clear(); }
                if (cur.Length > 0) cur.Append(' ');
                cur.Append(w.Text); lastY = y; lastH = h;
            }
            if (cur.Length > 0) lines.Add(cur.ToString());
            lines = lines.Select(Clean).Where(l => l.Length > 0 && !Regex.IsMatch(l, @"^\d{1,3}$")).ToList();
            if (lines.Count == 0) continue;
            var page = new ImportedPage();
            if (lines[0].Length <= 90) { page.Title = lines[0]; lines.RemoveAt(0); }
            // دمج الأسطر إلى فقرات: السطر الذي ينتهي بعلامة ترقيم أو يبدأ بنقطة يبقى مستقلًا، وإلا يُلحق بما قبله
            foreach (var l in lines)
            {
                var bullet = Regex.IsMatch(l, @"^([•\-–●▪○■]|\d+[\.\)])\s");
                if (!bullet && page.Paragraphs.Count > 0 && !Regex.IsMatch(page.Paragraphs[^1], @"[\.:؟!؛]$") && page.Paragraphs[^1].Length < 160)
                    page.Paragraphs[^1] = page.Paragraphs[^1] + " " + l;
                else page.Paragraphs.Add(Regex.Replace(l, @"^([•\-–●▪○■]|\d+[\.\)])\s+", ""));
            }
            pages.Add(page);
        }
        return pages;
    }

    private static readonly Regex TagRx = new(@"<(/?)(h[1-6]|p|li|table|tr|td|th|img|br|div|section|article|aside|header|footer|main|nav|button|a|span|label|input|select|option|textarea|form|ul|ol|blockquote|pre|figure|figcaption|summary|details)\b([^>]*?)(/?)>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly HashSet<string> SkipTags = new(StringComparer.OrdinalIgnoreCase) { "nav", "button", "input", "select", "option", "textarea", "form" };
    private static readonly HashSet<string> VoidTags = new(StringComparer.OrdinalIgnoreCase) { "img", "br", "input" };
    private const int MaxDataChars = 60_000;
    /// <summary>رسائل وتلميحات واجهة شائعة لا تُعد محتوى (في النص المرئي وفي نصوص السكربت).</summary>
    private static readonly Regex UiTextRx = new(@"^(اضغط|انقر|جارٍ|جاري|يرجى|اسحب|اختر|لديك تعديلات|تم الحفظ|تم التنزيل|تعذّر|تعذر|حفظ |تراجع$|إلغاء$|عرض فقط|العودة إلى|التالي|السابق)", RegexOptions.Compiled);

    /// <summary>قراءة خفيفة لصفحة HTML بلا مكتبة: العناوين تبدأ شرائح، والفقرات/عناصر القوائم نقاط، والجداول جداول، والصور (data URL) صور،
    /// والروابط تُجمع على حدة. عناصر الواجهة (أزرار، تنقّل، حقول إدخال، <c>aria-hidden</c>، <c>role=tab</c>) تُتجاهل بكاملها لأنها ليست محتوى.
    /// كتل <c>&lt;script type="application/json"&gt;</c> والنصوص الحرفية في السكربتات تُحفظ في <paramref name="dataBlocks"/> (بلا تنفيذ) لوضع الفهم بالموديل.</summary>
    private static List<ImportedPage> ExtractHtml(byte[] bytes, List<string> warnings, List<string> dataBlocks, ref string title)
    {
        var html = Encoding.UTF8.GetString(bytes);
        html = Regex.Replace(html, @"<!--.*?-->", "", RegexOptions.Singleline);
        CollectDataBlocks(html, dataBlocks);
        html = Regex.Replace(html, @"<(script|style|svg|noscript|template)\b[^>]*>.*?</\1\s*>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        var t = Regex.Match(html, @"<title[^>]*>(.*?)</title>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (t.Success && Clean(WebUtility.HtmlDecode(Regex.Replace(t.Groups[1].Value, "<[^>]+>", ""))) is { Length: > 0 } tt) title = tt;
        var body = Regex.Match(html, @"<body[^>]*>(.*)</body>", RegexOptions.Singleline | RegexOptions.IgnoreCase) is { Success: true } bm ? bm.Groups[1].Value : html;

        var pages = new List<ImportedPage>(); var page = new ImportedPage();
        List<List<string>>? table = null; List<string>? row = null; var cellBuf = new StringBuilder(); var textBuf = new StringBuilder(); var images = 0;
        int pos = 0; string? open = null;
        // تجاهل شجرة عنصر واجهة: اسم الوسم المتجاهَل وعمق تداخله حتى يُغلق
        string? skipTag = null; var skipDepth = 0;
        string? linkHref = null; var linkBuf = new StringBuilder();
        void FlushText()
        {
            var s = Clean(WebUtility.HtmlDecode(Regex.Replace(textBuf.ToString(), "<[^>]+>", " "))); textBuf.Clear();
            if (s.Length > 0) page.Paragraphs.Add(s.Length > 400 ? s[..400] + "…" : s);
        }
        foreach (Match m in TagRx.Matches(body))
        {
            var between = body[pos..m.Index]; pos = m.Index + m.Length;
            var closing = m.Groups[1].Value == "/"; var tag = m.Groups[2].Value.ToLowerInvariant(); var attrs = m.Groups[3].Value; var selfClosed = m.Groups[4].Value == "/" || VoidTags.Contains(tag);
            if (skipTag is not null)
            {
                if (tag == skipTag && !selfClosed) { if (closing) { if (--skipDepth == 0) skipTag = null; } else skipDepth++; }
                continue;
            }
            if (linkHref is not null) { if (linkBuf.Length > 0 && between.Length > 0) linkBuf.Append(' '); linkBuf.Append(between); }
            else if (table is not null && row is not null) { if (cellBuf.Length > 0 && between.Length > 0) cellBuf.Append(' '); cellBuf.Append(between); }
            else if (open is not null) textBuf.Append(between);
            if (!closing && !selfClosed && (SkipTags.Contains(tag) || Regex.IsMatch(attrs, @"aria-hidden\s*=\s*[""']?true", RegexOptions.IgnoreCase) || Regex.IsMatch(attrs, @"role\s*=\s*[""']?(tab|tablist|button|presentation)\b", RegexOptions.IgnoreCase)))
            { skipTag = tag; skipDepth = 1; continue; }
            if (tag == "a")
            {
                if (!closing) { var href = Regex.Match(attrs, @"href\s*=\s*[""']([^""']+)", RegexOptions.IgnoreCase).Groups[1].Value; if (href.StartsWith("http", StringComparison.OrdinalIgnoreCase)) { linkHref = href; linkBuf.Clear(); } }
                else if (linkHref is not null) { var text = Clean(WebUtility.HtmlDecode(Regex.Replace(linkBuf.ToString(), "<[^>]+>", " "))); page.Links.Add((text.Length > 0 ? text : linkHref, linkHref)); linkHref = null; }
                continue;
            }
            if (linkHref is not null) continue;
            switch (tag)
            {
                case "h1": case "h2": case "h3": case "h4": case "h5": case "h6":
                    if (!closing) { FlushText(); if (!page.IsEmpty) { pages.Add(page); page = new ImportedPage(); } open = tag; }
                    else { var s = Clean(WebUtility.HtmlDecode(Regex.Replace(textBuf.ToString(), "<[^>]+>", " "))); textBuf.Clear(); open = null; if (s.Length > 0) { if (page.Title is null) page.Title = s; else page.Paragraphs.Add(s); } }
                    break;
                case "p": case "li": case "div": case "section": case "article": case "aside": case "header": case "footer": case "main": case "blockquote": case "pre": case "figure": case "figcaption": case "summary": case "details": case "label": case "br":
                    if (table is not null) break;
                    if (!closing && tag != "br") { FlushText(); open = tag; } else { FlushText(); if (tag != "br") open = null; }
                    break;
                case "table": if (!closing) { FlushText(); table = new(); } else { if (table is { Count: > 0 }) page.Tables.Add(new DeckTable { Columns = table[0], Rows = table.Skip(1).Take(40).ToList() }); table = null; row = null; } break;
                case "tr": if (table is null) break; if (!closing) row = new(); else { if (row is { Count: > 0 }) table.Add(row); row = null; } break;
                case "td": case "th": if (row is null) break; if (!closing) cellBuf.Clear(); else row.Add(Clean(WebUtility.HtmlDecode(Regex.Replace(cellBuf.ToString(), "<[^>]+>", " ")))); break;
                case "img":
                    var src = Regex.Match(attrs, @"src\s*=\s*[""']([^""']+)", RegexOptions.IgnoreCase).Groups[1].Value;
                    if (src.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase) && src.Length < MaxImageBytes * 4 / 3 && images < MaxImagesPerDeck) { page.Images.Add(src); images++; }
                    else if (src.Length > 0 && !src.StartsWith("data:")) warnings.Add("صورة خارجية تُركت (الروابط الخارجية لا تُضمَّن): " + (src.Length > 60 ? src[..60] + "…" : src));
                    break;
            }
        }
        FlushText(); if (!page.IsEmpty) pages.Add(page);
        // تلميحات الواجهة القصيرة الشائعة («اضغط على…», «جارٍ التحميل…») ليست محتوى
        foreach (var pg in pages) pg.Paragraphs.RemoveAll(x => UiTextRx.IsMatch(x) || x.Length <= 1);
        return pages;
    }

    /// <summary>كتل JSON المضمّنة كما هي، ثم النصوص الحرفية (بين علامات اقتباس) في السكربتات المضمّنة التي تبدو كلامًا لا كودًا.</summary>
    private static void CollectDataBlocks(string html, List<string> dataBlocks)
    {
        var total = 0;
        foreach (Match m in Regex.Matches(html, @"<script\b[^>]*type\s*=\s*[""']application/(?:ld\+)?json[""'][^>]*>(.*?)</script\s*>", RegexOptions.Singleline | RegexOptions.IgnoreCase))
        {
            var j = m.Groups[1].Value.Trim(); if (j.Length == 0) continue;
            if (total + j.Length > MaxDataChars) j = j[..Math.Max(0, MaxDataChars - total)];
            if (j.Length == 0) break;
            dataBlocks.Add("JSON:\n" + j); total += j.Length;
        }
        var strings = new List<string>(); var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match m in Regex.Matches(html, @"<script\b(?![^>]*\bsrc\s*=)(?![^>]*type\s*=\s*[""']application/)[^>]*>(.*?)</script\s*>", RegexOptions.Singleline | RegexOptions.IgnoreCase))
        {
            foreach (Match lit in Regex.Matches(m.Groups[1].Value, @"""((?:[^""\\\n]|\\.){3,})""|'((?:[^'\\\n]|\\.){3,})'|`((?:[^`\\]|\\.){3,})`"))
            {
                var v = lit.Groups[1].Success ? lit.Groups[1].Value : lit.Groups[2].Success ? lit.Groups[2].Value : lit.Groups[3].Value;
                v = Clean(Regex.Replace(v, @"\$\{[^}]*\}|<[^>]+>", " "));
                // عربي = كلام غالبًا؛ لاتيني فقط = كلام إن حمل حرفًا كبيرًا أو 4 كلمات فأكثر (يستبعد أسماء الأصناف مثل «node dim»)
                if (v.Length < 3 || Regex.IsMatch(v, @"[{};=]|^[#.][\w-]+$|^(application|text|image)/")) continue;
                if (!Regex.IsMatch(v, @"[\u0600-\u06FF]") && !(Regex.IsMatch(v, @"[A-Z]") && Regex.IsMatch(v, @"[A-Za-z]{2,}\s+[A-Za-z]{2,}")) && Regex.Matches(v, @"[A-Za-z]{2,}").Count < 4) continue;
                if (UiTextRx.IsMatch(v)) continue;
                if (seen.Add(v)) strings.Add(v);
            }
        }
        if (strings.Count > 0)
        {
            var sb = new StringBuilder("نصوص من بيانات الصفحة:\n");
            foreach (var v in strings) { if (total + sb.Length + v.Length > MaxDataChars) break; sb.Append("- ").Append(v).Append('\n'); }
            dataBlocks.Add(sb.ToString());
        }
    }

    private static string Clean(string s) => Regex.Replace(s ?? "", @"\s+", " ").Trim();

    // ================= التحويل إلى شرائح =================
    public static DeckSpec ToSpec(ImportedDocument doc)
    {
        var spec = new DeckSpec { Title = doc.Title.Length > 0 ? doc.Title : "عرض مستورد" };
        var first = doc.Pages.FirstOrDefault();
        var startAt = 0;
        if (first is not null && first.Tables.Count == 0 && first.Charts.Count == 0 && first.Paragraphs.Count <= 2 && first.Title is not null && first.Images.Count == 0)
        {
            spec.Title = first.Title; spec.Slides.Add(new DeckSlide { Id = DeckSpec.NewId(), Layout = "title", Title = first.Title, Subtitle = first.Paragraphs.FirstOrDefault(), Notes = first.Notes }); startAt = 1;
        }
        else spec.Slides.Add(new DeckSlide { Id = DeckSpec.NewId(), Layout = "title", Title = spec.Title });

        foreach (var p in doc.Pages.Skip(startAt))
        {
            var title = p.Title ?? (p.Paragraphs.Count > 0 ? Shorten(p.Paragraphs[0], 60) : "شريحة");
            var bullets = p.Paragraphs.Where(x => x.Length > 0).ToList();
            var placedText = false;
            if (bullets.Count > 0)
            {
                // أرقام في نقاط قصيرة («الإيراد: 4.2M») → مؤشرات
                var kpis = bullets.Select(b => Regex.Match(b, @"^(?<l>[^:：]{2,40})[:：]\s*(?<v>[\d\.,%٪+\-−]+[A-Za-z؀-ۿ\.\s]{0,12})$")).ToList();
                if (bullets.Count is >= 2 and <= 6 && kpis.All(m => m.Success))
                    spec.Slides.Add(new DeckSlide { Id = DeckSpec.NewId(), Layout = "kpis", Title = title, Kpis = kpis.Select(m => new DeckKpi { Label = m.Groups["l"].Value.Trim(), Value = m.Groups["v"].Value.Trim() }).ToList(), Notes = p.Notes });
                else
                    foreach (var chunk in Chunk(bullets, 8).Select((c, i) => (c, i)))
                        spec.Slides.Add(new DeckSlide { Id = DeckSpec.NewId(), Layout = "bullets", Title = chunk.i == 0 ? title : $"{title} (تابع)", Bullets = chunk.c.Select(b => b.Length > 220 ? b[..220] + "…" : b).ToList(), Notes = chunk.i == 0 ? p.Notes : null });
                placedText = true;
            }
            foreach (var t in p.Tables.Where(t => t.Columns.Count > 0))
                spec.Slides.Add(new DeckSlide { Id = DeckSpec.NewId(), Layout = "table", Title = title, Table = new DeckTable { Columns = t.Columns, Rows = t.Rows.Select(r => r.Take(t.Columns.Count).ToList()).ToList() } });
            foreach (var c in p.Charts) spec.Slides.Add(new DeckSlide { Id = DeckSpec.NewId(), Layout = "chart", Title = title, Chart = c });
            foreach (var img in p.Images) spec.Slides.Add(new DeckSlide { Id = DeckSpec.NewId(), Layout = "image", Title = title, Image = new DeckImage { Src = img } });
            if (p.Links.Count > 0)
                spec.Slides.Add(new DeckSlide { Id = DeckSpec.NewId(), Layout = "bullets", Title = p.Title ?? "روابط", Bullets = p.Links.Take(12).Select(l => l.Text == l.Href ? l.Href : $"{l.Text} — {l.Href}").ToList() });
            if (!placedText && p.Tables.Count == 0 && p.Charts.Count == 0 && p.Images.Count == 0 && p.Links.Count == 0 && p.Title is not null)
                spec.Slides.Add(new DeckSlide { Id = DeckSpec.NewId(), Layout = "section", Title = p.Title, Notes = p.Notes });
            if (spec.Slides.Count >= DeckSpec.MaxSlides) break;
        }
        return spec.Normalize();
    }

    private static IEnumerable<List<string>> Chunk(List<string> items, int size) { for (var i = 0; i < items.Count; i += size) yield return items.Skip(i).Take(size).ToList(); }
    private static string Shorten(string s, int n) => s.Length <= n ? s : s[..n] + "…";

// ================= فهم المحتوى بالموديل (لصفحات التطبيقات التفاعلية خاصة) =================
    /// <summary>المدخل النصي للموديل: مخطط الصفحات (عنوان، نقاط، جداول، روابط، صور بمعرّفات) ثم كتل بيانات الصفحة. مقيَّد بالطول.</summary>
    public static string BuildUnderstandInput(ImportedDocument doc)
    {
        var sb = new StringBuilder();
        sb.Append("العنوان: ").AppendLine(doc.Title);
        for (var i = 0; i < doc.Pages.Count; i++)
        {
            var p = doc.Pages[i];
            sb.Append("\n## ").Append(i + 1).Append(". ").AppendLine(p.Title ?? "(بلا عنوان)");
            foreach (var x in p.Paragraphs) sb.Append("- ").AppendLine(x);
            foreach (var t in p.Tables) { sb.Append("جدول: ").AppendLine(string.Join(" | ", t.Columns)); foreach (var r in t.Rows.Take(20)) sb.Append("  ").AppendLine(string.Join(" | ", r)); }
            foreach (var c in p.Charts) sb.Append("رسم ").Append(c.Type).Append(": ").AppendLine(string.Join(", ", c.Labels.Zip(c.Values, (l, v) => $"{l}={v}")));
            for (var k = 0; k < p.Images.Count; k++) sb.Append("صورة: image:").Append(i).Append('-').AppendLine(k.ToString());
            foreach (var l in p.Links) sb.Append("رابط: ").Append(l.Text).Append(" — ").AppendLine(l.Href);
            if (p.Notes is not null) sb.Append("ملاحظات: ").AppendLine(p.Notes);
        }
        if (doc.DataBlocks.Count > 0) { sb.AppendLine("\n# بيانات مضمّنة في الصفحة (محتواها الحقيقي غالبًا هنا):"); foreach (var b in doc.DataBlocks) sb.AppendLine(b); }
        var text = sb.ToString();
        return text.Length > MaxDataChars + 20_000 ? text[..(MaxDataChars + 20_000)] + "\n…(اقتُطع)" : text;
    }

    public const string UnderstandSystemPrompt = """
        أنت مصمم عروض تقديمية داخل منصة «جيم». تتلقى محتوى مستخرجًا آليًا من ملف أو صفحة ويب (مخطط الأقسام، ثم بيانات مضمّنة في الصفحة قد تحمل
        المحتوى الحقيقي: مصفوفات نصوص، JSON مهام، أدوار أطراف…). ابنِ منه عرضًا تقديميًا كاملًا يعبّر عن المحتوى نفسه بترتيب منطقي:
        - اقرأ البيانات المضمّنة بعناية: قوائم المسؤوليات والآليات والمخرجات، الأطراف وأدوارهم، المهام وحالتها… واجعلها شرائح حقيقية.
        - التخطيطات: title, section, bullets, two-column (columns:[{title,bullets}]), kpis (kpis:[{label,value,delta}]), chart, table (table:{columns,rows}),
          image (image:{src:"image:…"} بنفس المعرّف الوارد)، quote، tasks (tasks:[{t,done}] + editable:{key:"tasks-<n>",kind:"tasks"}) لقوائم المهام.
        - تجاهل نصوص الواجهة (أزرار، تلميحات «اضغط على…»، رسائل الحفظ). الروابط تُجمع في شريحة «روابط» بنقاط «الاسم — الرابط».
        - لا تخترع أرقامًا أو حقائق؛ لا تحذف محتوى جوهريًا؛ 5–20 شريحة؛ عناوين موجزة بالعربية الفصحى؛ نقاط لا تتجاوز 8 في الشريحة.
        أعد JSON فقط بالشكل: {"spec": {"title": "...", "subtitle": "...", "slides": [ ... ]}}
        """;

    /// <summary>نداء واحد بلا أدوات يبني المواصفة كاملة من المحتوى المستخرج؛ عند الفشل تعود المواصفة الحتمية مع ملاحظة.</summary>
    public async Task<(DeckSpec Spec, string? Note)> UnderstandAsync(ImportedDocument doc, DeckSpec fallback, AnalyticsTools.SourceContext context, AppUser? user, CancellationToken ct)
    {
        var llm = await _router.ResolveStagedAsync(ct);
        if (llm is null) return (fallback, "لا يوجد مزوّد موديل مهيأ — أُبقي الترتيب الحتمي.");
        try
        {
            var call = new StagedCall("[عرض] استيراد — فهم المحتوى", UnderstandSystemPrompt, BuildUnderstandInput(doc), null, context, 1, false, user);
            var res = await llm.RunStagedAsync(call, ct);
            var spec = ApplyUnderstood(res.Json, doc);
            return spec is null ? (fallback, "لم يُعد الموديل مواصفة صالحة — أُبقي الترتيب الحتمي.") : (spec, null);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { _logger.LogWarning(ex, "فشل فهم المحتوى المستورد بالموديل"); return (fallback, "تعذّر الفهم بالموديل: " + ex.Message); }
    }

    /// <summary>تفسير رد الموديل: مواصفة مطبَّعة، إعادة الصور الحقيقية مكان معرّفاتها، وإسقاط صور بمعرّف مجهول.</summary>
    public static DeckSpec? ApplyUnderstood(JsonObject json, ImportedDocument doc)
    {
        var spec = json["spec"]?.Deserialize<DeckSpec>(DeckSpec.Json);
        if (spec is null || spec.Slides.Count == 0) return null;
        if (string.IsNullOrWhiteSpace(spec.Title)) spec.Title = doc.Title;
        foreach (var s in spec.Slides)
        {
            if (s.Image is not null)
            {
                var m = Regex.Match(s.Image.Src ?? "", @"^image:(\d+)-(\d+)$");
                if (m.Success && int.Parse(m.Groups[1].Value) < doc.Pages.Count && int.Parse(m.Groups[2].Value) < doc.Pages[int.Parse(m.Groups[1].Value)].Images.Count)
                    s.Image.Src = doc.Pages[int.Parse(m.Groups[1].Value)].Images[int.Parse(m.Groups[2].Value)];
                else if (!(s.Image.Src ?? "").StartsWith("data:image/")) s.Image = null;
            }
            if (s.Layout == "tasks" && s.Editable is null) s.Editable = new DeckEditable { Key = "tasks-" + (string.IsNullOrEmpty(s.Id) ? DeckSpec.NewId() : s.Id), Kind = "tasks" };
        }
        return spec.Normalize();
    }

    // ================= تحسين اختياري بالموديل =================
    /// <summary>نداء واحد بلا أدوات: يعيد ترتيب المواصفة الحتمية في تخطيطات أفضل وعناوين أوضح دون اختلاق أرقام. عند الفشل تعود المواصفة كما هي.</summary>
    public async Task<(DeckSpec Spec, string? Note)> RefineAsync(DeckSpec spec, AnalyticsTools.SourceContext context, AppUser? user, CancellationToken ct)
    {
        var llm = await _router.ResolveStagedAsync(ct);
        if (llm is null) return (spec, "لا يوجد مزوّد موديل مهيأ — أُبقي الترتيب الحتمي.");
        var light = JsonSerializer.Serialize(StripImages(spec), DeckSpec.Json);
        var system = """
            أنت محرر عروض تقديمية داخل منصة «جيم». تتلقى مواصفة عرض JSON مستخرجة آليًا من ملف، وتعيد نفس المواصفة محسَّنة:
            - اجعل العناوين واضحة وموجزة بالعربية الفصحى، واختصر النقاط الطويلة دون حذف معلومة.
            - حوّل النقاط التي هي أرقام مع تسمياتها إلى شريحة kpis، وادمج الشرائح شديدة القصر، وقسّم المكتظة.
            - لا تخترع أرقامًا أو حقائق غير موجودة، ولا تغيّر بيانات الجداول والرسوم، ولا تحذف شريحة فيها جدول أو رسم أو صورة (أبقِ حقل image كما وصلك إن وُجد).
            - التخطيطات المسموحة: title, section, bullets, two-column, kpis, chart, table, image, quote.
            أعد JSON فقط بالشكل: {"spec": { ...المواصفة... }}
            """;
        try
        {
            var call = new StagedCall("[عرض] استيراد — تحسين الترتيب", system, "المواصفة:\n" + light, null, context, 1, false, user);
            var res = await llm.RunStagedAsync(call, ct);
            var refined = res.Json["spec"]?.Deserialize<DeckSpec>(DeckSpec.Json);
            if (refined is null || refined.Slides.Count == 0) return (spec, "لم يُعد الموديل مواصفة صالحة — أُبقي الترتيب الحتمي.");
            RestoreImages(spec, refined);
            if (string.IsNullOrWhiteSpace(refined.Title)) refined.Title = spec.Title;
            return (refined.Normalize(), null);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { _logger.LogWarning(ex, "فشل تحسين العرض المستورد بالموديل"); return (spec, "تعذّر التحسين بالموديل: " + ex.Message); }
    }

    /// <summary>الصور تُستبدل بمعرّفات قصيرة قبل إرسالها للموديل (data URL كبيرة) وتُعاد بعده.</summary>
    private static DeckSpec StripImages(DeckSpec spec)
    {
        var copy = JsonSerializer.Deserialize<DeckSpec>(JsonSerializer.Serialize(spec, DeckSpec.Json), DeckSpec.Json)!;
        foreach (var s in copy.Slides) if (s.Image is not null) s.Image.Src = "image:" + s.Id;
        return copy;
    }
    private static void RestoreImages(DeckSpec original, DeckSpec refined)
    {
        var map = original.Slides.Where(s => s.Image?.Src is { Length: > 0 }).ToDictionary(s => "image:" + s.Id, s => s.Image!.Src);
        foreach (var s in refined.Slides) if (s.Image is not null && map.TryGetValue(s.Image.Src, out var src)) s.Image.Src = src;
        foreach (var s in refined.Slides.Where(s => s.Image is not null && s.Image.Src.StartsWith("image:")).ToList()) s.Image = null;
    }
}
