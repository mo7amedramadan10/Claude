using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ChatToDashboard.Api.Data;
using Dapper;
using DocumentFormat.OpenXml.Packaging;

namespace ChatToDashboard.Api.Decks;

/// <summary>عنصر محجوز في تخطيط (نسب من عرض/ارتفاع الشريحة) — تُستخدم لاحقًا لوضع محتوى الشريحة في مكانه الصحيح.</summary>
public sealed class TemplatePlaceholder
{
    [JsonPropertyName("type")] public string Type { get; set; } = "body";
    [JsonPropertyName("idx")] public int? Idx { get; set; }
    [JsonPropertyName("x")] public double X { get; set; }
    [JsonPropertyName("y")] public double Y { get; set; }
    [JsonPropertyName("w")] public double W { get; set; }
    [JsonPropertyName("h")] public double H { get; set; }
    [JsonPropertyName("prompt")] public string? Prompt { get; set; }
    /// <summary>الدور المستنتج: title | subtitle | body | pic | other.</summary>
    [JsonPropertyName("role")] public string Role { get; set; } = "other";
    [JsonPropertyName("size")] public double? Size { get; set; }
    [JsonPropertyName("color")] public string? Color { get; set; }
    [JsonPropertyName("align")] public string? Align { get; set; }
    [JsonPropertyName("font")] public string? Font { get; set; }
    [JsonPropertyName("bold")] public bool? Bold { get; set; }
}

/// <summary>الربط الافتراضي بين أنواع شرائح العرض وتخطيطات القالب (فهارس في Layouts) — يحسبه التحليل ويعدّله المستخدم.</summary>
public sealed class TemplateDefaults
{
    [JsonPropertyName("title")] public int? Title { get; set; }
    [JsonPropertyName("section")] public int? Section { get; set; }
    [JsonPropertyName("content")] public int? Content { get; set; }
    [JsonPropertyName("end")] public int? End { get; set; }
}

/// <summary>صفحة من القالب: تخطيط (<c>layout</c>) أو شريحة مثال (<c>slide</c>) مع ما يلزم لاختيارها وعرضها.</summary>
public sealed class TemplatePage
{
    [JsonPropertyName("kind")] public string Kind { get; set; } = "layout";
    [JsonPropertyName("index")] public int Index { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("master")] public int Master { get; set; }
    [JsonPropertyName("layout")] public int? Layout { get; set; }
    [JsonPropertyName("placeholders")] public List<TemplatePlaceholder> Placeholders { get; set; } = new();
    [JsonPropertyName("usedBySlides")] public int UsedBySlides { get; set; }
    /// <summary>تخطيط افتراضي من PowerPoint (اسم إنجليزي قياسي) لا تستخدمه أي شريحة مثال — يُخفى افتراضيًا.</summary>
    [JsonPropertyName("generic")] public bool Generic { get; set; }
    [JsonPropertyName("unsupported")] public List<string> Unsupported { get; set; } = new();
    [JsonPropertyName("texts")] public int Texts { get; set; }
    [JsonPropertyName("charts")] public int Charts { get; set; }
    [JsonPropertyName("svgUrl")] public string SvgUrl { get; set; } = "";
    /// <summary>للتخطيطات: نسخة بلا الأماكن المحجوزة تُستخدم خلفيةً لشرائح المنصة.</summary>
    [JsonPropertyName("bgUrl")] public string? BgUrl { get; set; }
    [JsonIgnore] public string Svg { get; set; } = "";
    [JsonIgnore] public string? BgSvg { get; set; }
}

public sealed class TemplateInfo
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("fileName")] public string FileName { get; set; } = "";
    [JsonPropertyName("sizeBytes")] public long SizeBytes { get; set; }
    [JsonPropertyName("uploadedAt")] public DateTime UploadedAt { get; set; }
    [JsonPropertyName("width")] public double Width { get; set; }
    [JsonPropertyName("height")] public double Height { get; set; }
    [JsonPropertyName("themeColors")] public Dictionary<string, string> ThemeColors { get; set; } = new();
    [JsonPropertyName("fonts")] public List<string> Fonts { get; set; } = new();
    [JsonPropertyName("logo")] public string? Logo { get; set; }
    [JsonPropertyName("defaults")] public TemplateDefaults Defaults { get; set; } = new();
    [JsonPropertyName("layouts")] public List<TemplatePage> Layouts { get; set; } = new();
    [JsonPropertyName("slides")] public List<TemplatePage> Slides { get; set; } = new();
    [JsonPropertyName("warnings")] public List<string> Warnings { get; set; } = new();
}

/// <summary>قالب PowerPoint للشركة لكل مشروع: الملف كاملًا في <c>dbo.DeckBrandTemplates</c> وصفحاته المرسومة SVG في
/// <c>dbo.DeckBrandTemplatePages</c> (تُرسم مرة عند الرفع؛ الصور تُحال إلى مسار وسائط يُقرأ من الملف عند الطلب).</summary>
public class DeckBrandTemplateStore
{
    public const int MaxBytes = 64 * 1024 * 1024;
    private static readonly string[] GenericNames = { "Title Slide", "Title and Content", "Section Header", "Two Content", "Comparison", "Title Only", "Blank", "Content with Caption", "Picture with Caption", "Title and Vertical Text", "Vertical Title and Text" };
    private readonly DataStore _db;
    private readonly SemaphoreSlim _schemaGate = new(1, 1);
    private volatile bool _schemaReady;
    private readonly Dictionary<string, (byte[] Bytes, DateTime At)> _bytesCache = new();
    private readonly object _cacheLock = new();
    public DeckBrandTemplateStore(DataStore db) => _db = db;
    private const string Table = "[dbo].[DeckBrandTemplates]";
    private const string Pages = "[dbo].[DeckBrandTemplatePages]";
    private static string Key(string? projectId) => string.IsNullOrEmpty(projectId) ? "-" : projectId;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    private async Task EnsureSchemaAsync(CancellationToken ct)
    {
        if (_schemaReady) return;
        await _schemaGate.WaitAsync(ct);
        try
        {
            if (_schemaReady) return;
            await using var connection = await _db.OpenConnectionAsync(ct);
            await _db.CreateContainerIfMissingAsync(connection, ct);
            await using var command = connection.CreateCommand();
            command.CommandText = $"""
                IF OBJECT_ID('dbo.DeckBrandTemplates') IS NULL
                CREATE TABLE {Table} (
                  [ProjectKey] NVARCHAR(64) PRIMARY KEY, [Id] NVARCHAR(32) NOT NULL, [FileName] NVARCHAR(260) NOT NULL, [SizeBytes] BIGINT NOT NULL,
                  [Bytes] VARBINARY(MAX) NOT NULL, [InfoJson] NVARCHAR(MAX) NOT NULL, [UploadedBy] NVARCHAR(200), [UploadedAt] DATETIME2 NOT NULL);
                IF OBJECT_ID('dbo.DeckBrandTemplatePages') IS NULL
                CREATE TABLE {Pages} (
                  [TemplateId] NVARCHAR(32) NOT NULL, [Kind] NVARCHAR(10) NOT NULL, [Idx] INT NOT NULL, [Svg] NVARCHAR(MAX) NOT NULL,
                  CONSTRAINT PK_DeckBrandTemplatePages PRIMARY KEY ([TemplateId], [Kind], [Idx]))
                """;
            await command.ExecuteNonQueryAsync(ct);
            _schemaReady = true;
        }
        finally { _schemaGate.Release(); }
    }

    /// <summary>يحلّل الملف ويرسم كل صفحاته (بلا حفظ) — يُستخدم عند الرفع وفي الاختبارات.</summary>
    public static (TemplateInfo Info, List<TemplatePage> Pages) Analyze(byte[] bytes, string fileName, string templateId)
    {
        using var r = new PptxSvgRenderer(bytes, ip => $"/api/decks/brand/template/{templateId}/media/{Path.GetFileName(ip.Uri.ToString())}");
        var info = new TemplateInfo { Id = templateId, FileName = Path.GetFileName(fileName), SizeBytes = bytes.Length, UploadedAt = DateTime.UtcNow, Width = r.Width, Height = r.Height };
        var pages = new List<TemplatePage>();
        var usage = new int[r.Layouts.Count];
        foreach (var s in r.Slides) { var li = r.LayoutIndexOfSlide(s); if (li >= 0) usage[li]++; }
        // الثيم من أول أساس
        if (r.Masters.Count > 0 && r.Masters[0].ThemePart is { } tp)
        {
            using var st = tp.GetStream(); var theme = XDocument.Load(st).Root!; XNamespace A = "http://schemas.openxmlformats.org/drawingml/2006/main";
            foreach (var e in theme.Descendants(A + "clrScheme").FirstOrDefault()?.Elements() ?? Enumerable.Empty<XElement>())
            { var c = e.Element(A + "srgbClr")?.Attribute("val")?.Value ?? e.Element(A + "sysClr")?.Attribute("lastClr")?.Value; if (c is not null) info.ThemeColors[e.Name.LocalName] = "#" + c; }
        }
        var fontCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        void CountFonts(OpenXmlPart part)
        {
            using var s = part.GetStream(); var x = XDocument.Load(s).Root!;
            foreach (var tf in x.Descendants().Where(e => e.Name.LocalName is "latin" or "cs").Select(e => e.Attribute("typeface")?.Value).Where(v => !string.IsNullOrEmpty(v) && !v!.StartsWith("+")))
                fontCounts[tf!] = fontCounts.GetValueOrDefault(tf!) + 1;
        }
        for (var i = 0; i < r.Layouts.Count; i++)
        {
            var l = r.Layouts[i]; var name = PptxSvgRenderer.LayoutName(l); var masterIdx = r.Masters.ToList().IndexOf(r.MasterOf(l));
            var svg = r.RenderLayout(i);
            var page = new TemplatePage { Kind = "layout", Index = i, Name = name, Master = masterIdx, UsedBySlides = usage[i], Svg = svg, Unsupported = r.Unsupported.Distinct().ToList(), SvgUrl = $"/api/decks/brand/template/{templateId}/pages/layout/{i}.svg", BgUrl = $"/api/decks/brand/template/{templateId}/pages/bg/{i}.svg" };
            page.BgSvg = r.RenderLayoutBackground(i);
            page.Generic = usage[i] == 0 && GenericNames.Any(g => string.Equals(g, name, StringComparison.OrdinalIgnoreCase)) && (r.Masters.Count > 1 && masterIdx > 0 || r.Layouts.Count > 20);
            page.Placeholders = Placeholders(l, r.Width, r.Height);
            CountFonts(l); pages.Add(page);
        }
        for (var i = 0; i < r.Slides.Count; i++)
        {
            var s = r.Slides[i]; var svg = r.RenderSlide(i);
            using var st = s.GetStream(); var x = XDocument.Load(st).Root!;
            var texts = x.Descendants().Where(e => e.Name.LocalName == "t").Select(e => e.Value.Trim()).Where(t => t.Length > 0).ToList();
            var title = texts.FirstOrDefault(t => t.Length is > 2 and < 80) ?? "";
            var charts = x.Descendants().Count(e => e.Name.LocalName == "graphicFrame");
            var li = r.LayoutIndexOfSlide(s);
            pages.Add(new TemplatePage { Kind = "slide", Index = i, Name = title.Length > 0 ? title : $"صفحة {i + 1}", Master = li >= 0 ? pages[li].Master : 0, Layout = li, Texts = texts.Count, Charts = charts, Svg = svg, Unsupported = r.Unsupported.Distinct().ToList(), SvgUrl = $"/api/decks/brand/template/{templateId}/pages/slide/{i}.svg" });
            CountFonts(s);
        }
        info.Fonts = fontCounts.OrderByDescending(k => k.Value).Select(k => k.Key).Take(8).ToList();
        info.Defaults = GuessDefaults(pages.Where(p => p.Kind == "layout").ToList());
        info.Layouts = pages.Where(p => p.Kind == "layout").ToList(); info.Slides = pages.Where(p => p.Kind == "slide").ToList();
        var unsupported = pages.SelectMany(p => p.Unsupported).Distinct().ToList();
        if (unsupported.Count > 0) info.Warnings.Add($"{unsupported.Count} صورة بصيغة EMF/WMF لا يعرضها المتصفح (تظهر في PowerPoint المُصدَّر فقط): {string.Join("، ", unsupported.Take(5))}");
        var generic = info.Layouts.Count(l => l.Generic);
        if (generic > 0) info.Warnings.Add($"{generic} تخطيطًا افتراضيًا من PowerPoint لا تستخدمه أي صفحة مثال أُخفي من القائمة.");
        return (info, pages);
    }

    /// <summary>اختيار التخطيطات الافتراضية بالاسم ثم بالبنية (غلاف/عنوان، فاصل، محتوى بأكبر مساحة، خاتمة).</summary>
    public static TemplateDefaults GuessDefaults(List<TemplatePage> layouts)
    {
        var real = layouts.Where(l => !l.Generic).ToList(); if (real.Count == 0) real = layouts;
        int? Pick(string rx, Func<TemplatePage, bool>? structural = null)
        {
            var byName = real.Where(l => Regex.IsMatch(l.Name, rx, RegexOptions.IgnoreCase)).OrderByDescending(l => l.UsedBySlides).FirstOrDefault();
            if (byName is not null) return byName.Index;
            return structural is null ? null : real.Where(structural).OrderByDescending(l => l.UsedBySlides).FirstOrDefault()?.Index;
        }
        var d = new TemplateDefaults
        {
            Title = Pick(@"^(cover|title slide|غلاف|عنوان)", l => l.Placeholders.Any(p => p.Type == "ctrTitle")),
            Section = Pick(@"separator|section|فاصل|قسم", l => l.Placeholders.Count(p => p.Role is "title" or "subtitle") >= 1 && !l.Placeholders.Any(p => p.Role == "body")),
            Content = Pick(@"general|inner|content|محتوى|داخلي", l => l.Placeholders.Any(p => p.Role == "body")),
            End = Pick(@"end|closing|thank|شكر|خاتمة"),
        };
        d.Content ??= real.OrderByDescending(l => l.Placeholders.Where(p => p.Role == "body").Sum(p => p.W * p.H)).FirstOrDefault()?.Index;
        d.Title ??= real.FirstOrDefault()?.Index;
        return d;
    }

    private static List<TemplatePlaceholder> Placeholders(SlideLayoutPart l, double W, double H)
    {
        using var st = l.GetStream(); var x = XDocument.Load(st).Root!;
        XNamespace P = "http://schemas.openxmlformats.org/presentationml/2006/main", A = "http://schemas.openxmlformats.org/drawingml/2006/main";
        var list = new List<TemplatePlaceholder>();
        foreach (var sp in x.Descendants(P + "sp"))
        {
            var ph = sp.Element(P + "nvSpPr")?.Element(P + "nvPr")?.Element(P + "ph"); if (ph is null) continue;
            var type = ph.Attribute("type")?.Value ?? "body"; if (type is "dt" or "ftr" or "sldNum") continue;
            var off = sp.Element(P + "spPr")?.Element(A + "xfrm")?.Element(A + "off"); var ext = sp.Element(P + "spPr")?.Element(A + "xfrm")?.Element(A + "ext");
            if (off is null || ext is null) continue;
            double Em(XElement e, string a) => double.Parse(e.Attribute(a)?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture) / PptxSvgRenderer.Emu;
            var prompt = string.Concat(sp.Descendants(A + "t").Select(t => t.Value)).Trim();
            var lvl1 = sp.Element(P + "txBody")?.Element(A + "lstStyle")?.Element(A + "lvl1pPr"); var rpr = lvl1?.Element(A + "defRPr");
            var tp = new TemplatePlaceholder
            {
                Type = type, Idx = int.TryParse(ph.Attribute("idx")?.Value, out var ix) ? ix : null,
                X = Math.Round(Em(off, "x") / W, 4), Y = Math.Round(Em(off, "y") / H, 4), W = Math.Round(Em(ext, "cx") / W, 4), H = Math.Round(Em(ext, "cy") / H, 4),
                Prompt = prompt.Length > 0 ? prompt : null, Align = lvl1?.Attribute("algn")?.Value,
                Size = rpr?.Attribute("sz")?.Value is { } sz ? double.Parse(sz, System.Globalization.CultureInfo.InvariantCulture) / 100 : null,
                Color = rpr?.Element(A + "solidFill")?.Element(A + "srgbClr")?.Attribute("val")?.Value is { } c ? "#" + c : null,
                Font = rpr?.Element(A + "latin")?.Attribute("typeface")?.Value, Bold = rpr?.Attribute("b")?.Value is { } b ? b == "1" : null,
            };
            list.Add(tp);
        }
        // الأدوار: عنوان = ctrTitle/title أو أعلى محجوز نصي قصير بمطالبة «عنوان»؛ فرعي = subTitle أو مطالبة «فرعي»؛ محتوى = أكبر مساحة متبقية
        var textPh = list.Where(p => p.Type is "body" or "title" or "ctrTitle" or "subTitle" or "obj").OrderBy(p => p.Y).ToList();
        var title = list.FirstOrDefault(p => p.Type is "ctrTitle" or "title")
                    ?? textPh.FirstOrDefault(p => p.Prompt is { } pr && (Regex.IsMatch(pr, @"عنوان|title", RegexOptions.IgnoreCase)) && !Regex.IsMatch(pr, @"فرعي|sub", RegexOptions.IgnoreCase))
                    ?? textPh.FirstOrDefault(p => p.H < 0.2);
        if (title is not null) title.Role = "title";
        var sub = list.FirstOrDefault(p => p.Type == "subTitle") ?? textPh.FirstOrDefault(p => p.Role == "other" && p.Prompt is { } pr && Regex.IsMatch(pr, @"فرعي|sub", RegexOptions.IgnoreCase));
        if (sub is not null && sub != title) sub.Role = "subtitle";
        var body = textPh.Where(p => p.Role == "other").OrderByDescending(p => p.W * p.H).FirstOrDefault();
        if (body is not null && body.W * body.H > 0.05) body.Role = "body";
        foreach (var p in list.Where(p => p.Type == "pic")) p.Role = "pic";
        return list;
    }

    public async Task<TemplateInfo> SaveAsync(string? projectId, string? userId, string fileName, byte[] bytes, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        var id = Guid.NewGuid().ToString("N")[..16];
        var (info, pages) = await Task.Run(() => Analyze(bytes, fileName, id), ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var old = await connection.QuerySingleOrDefaultAsync<string>($"SELECT Id FROM {Table} WHERE ProjectKey = @k", new { k = Key(projectId) });
        await using (var tx = await connection.BeginTransactionAsync(ct))
        {
            if (old is not null) { await connection.ExecuteAsync($"DELETE FROM {Pages} WHERE TemplateId = @old", new { old }, tx); await connection.ExecuteAsync($"DELETE FROM {Table} WHERE ProjectKey = @k", new { k = Key(projectId) }, tx); }
            await connection.ExecuteAsync($"INSERT INTO {Table} (ProjectKey, Id, FileName, SizeBytes, Bytes, InfoJson, UploadedBy, UploadedAt) VALUES (@k, @id, @fileName, @size, @bytes, @json, @userId, @at)",
                new { k = Key(projectId), id, fileName = info.FileName, size = info.SizeBytes, bytes, json = JsonSerializer.Serialize(info, Json), userId, at = info.UploadedAt }, tx);
            foreach (var p in pages)
            {
                await connection.ExecuteAsync($"INSERT INTO {Pages} (TemplateId, Kind, Idx, Svg) VALUES (@id, @kind, @idx, @svg)", new { id, kind = p.Kind, idx = p.Index, svg = p.Svg }, tx);
                if (p.BgSvg is not null) await connection.ExecuteAsync($"INSERT INTO {Pages} (TemplateId, Kind, Idx, Svg) VALUES (@id, 'bg', @idx, @svg)", new { id, idx = p.Index, svg = p.BgSvg }, tx);
            }
            await tx.CommitAsync(ct);
        }
        lock (_cacheLock) { if (old is not null) _bytesCache.Remove(old); _bytesCache[id] = (bytes, DateTime.UtcNow); }
        return info;
    }

    public async Task<TemplateInfo?> GetInfoAsync(string? projectId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var json = await connection.QuerySingleOrDefaultAsync<string>($"SELECT InfoJson FROM {Table} WHERE ProjectKey = @k", new { k = Key(projectId) });
        return json is null ? null : JsonSerializer.Deserialize<TemplateInfo>(json, Json);
    }

    public async Task<string?> GetPageSvgAsync(string templateId, string kind, int idx, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<string>($"SELECT Svg FROM {Pages} WHERE TemplateId = @templateId AND Kind = @kind AND Idx = @idx", new { templateId, kind, idx });
    }

    /// <summary>بايتات الملف بمعرّف القالب (مع ذاكرة قصيرة؛ الملف قد يكون عشرات الميغابايت).</summary>
    public async Task<byte[]?> GetBytesAsync(string templateId, CancellationToken ct = default)
    {
        lock (_cacheLock) { if (_bytesCache.TryGetValue(templateId, out var c) && DateTime.UtcNow - c.At < TimeSpan.FromMinutes(15)) return c.Bytes; }
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var bytes = await connection.QuerySingleOrDefaultAsync<byte[]>($"SELECT Bytes FROM {Table} WHERE Id = @templateId", new { templateId });
        if (bytes is not null) lock (_cacheLock) { foreach (var k in _bytesCache.Where(kv => DateTime.UtcNow - kv.Value.At > TimeSpan.FromMinutes(15)).Select(kv => kv.Key).ToList()) _bytesCache.Remove(k); _bytesCache[templateId] = (bytes, DateTime.UtcNow); }
        return bytes;
    }

    /// <summary>صورة من وسائط الملف باسمها (image12.png…) — للمتصفح والصفحة المنشورة.</summary>
    public static (byte[] Bytes, string ContentType)? Media(byte[] pptx, string name)
    {
        if (!Regex.IsMatch(name, @"^[\w.-]{1,80}$")) return null;
        using var doc = PresentationDocument.Open(new MemoryStream(pptx), false);
        foreach (var ip in AllImageParts(doc))
            if (string.Equals(Path.GetFileName(ip.Uri.ToString()), name, StringComparison.OrdinalIgnoreCase))
            { using var s = ip.GetStream(); using var ms = new MemoryStream(); s.CopyTo(ms); return (ms.ToArray(), ip.ContentType); }
        return null;
    }
    private static IEnumerable<ImagePart> AllImageParts(OpenXmlPackage pkg)
    {
        var seen = new HashSet<string>();
        IEnumerable<ImagePart> Walk(OpenXmlPartContainer c)
        {
            foreach (var rel in c.Parts)
            {
                var uri = rel.OpenXmlPart.Uri.ToString(); if (!seen.Add(uri)) continue;
                if (rel.OpenXmlPart is ImagePart ip) yield return ip;
                foreach (var x in Walk(rel.OpenXmlPart)) yield return x;
            }
        }
        return Walk(pkg);
    }

    public async Task<TemplateInfo?> SetDefaultsAsync(string? projectId, TemplateDefaults defaults, CancellationToken ct = default)
    {
        var info = await GetInfoAsync(projectId, ct); if (info is null) return null;
        int? Valid(int? i) => i is { } v && v >= 0 && v < info.Layouts.Count ? v : null;
        info.Defaults = new TemplateDefaults { Title = Valid(defaults.Title), Section = Valid(defaults.Section), Content = Valid(defaults.Content), End = Valid(defaults.End) };
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {Table} SET InfoJson = @json WHERE ProjectKey = @k", new { json = JsonSerializer.Serialize(info, Json), k = Key(projectId) });
        return info;
    }

    public async Task DeleteAsync(string? projectId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var id = await connection.QuerySingleOrDefaultAsync<string>($"SELECT Id FROM {Table} WHERE ProjectKey = @k", new { k = Key(projectId) });
        if (id is null) return;
        await connection.ExecuteAsync($"DELETE FROM {Pages} WHERE TemplateId = @id; DELETE FROM {Table} WHERE ProjectKey = @k", new { id, k = Key(projectId) });
        lock (_cacheLock) _bytesCache.Remove(id);
    }
}
