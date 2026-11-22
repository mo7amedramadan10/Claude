using ChatToDashboard.Api.Data;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text;
using System.Text.RegularExpressions;
using ChatToDashboard.Api.Decks;
using ChatToDashboard.Api.Freeform;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Organizations;
using ChatToDashboard.Api.Presentations;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

public class FreeformGenerateRequest { public string Message { get; set; } = ""; public string? DesignId { get; set; } public string? Html { get; set; } public string? Kind { get; set; } public string? TemplateKey { get; set; } /// <summary>لغة الصفحة للتصميم الجديد: ar (افتراضي) أو en؛ لتصميم قائم تُستنتج من صفحته.</summary>
    public string? Lang { get; set; } /// <summary>المصادر المختارة (systems/files)؛ null = لا تغيير (المحفوظة مع التصميم أو الكل).</summary>
    public Sources.SourceSelection? Sources { get; set; } }
public class FreeformSaveRequest { public string? Html { get; set; } public string? Title { get; set; } public string? Note { get; set; } public bool? AutoRefresh { get; set; } public Sources.SourceSelection? Sources { get; set; } public bool? ClearSources { get; set; } }
public class FreeformPublishRequest { public string? Slug { get; set; } public string? Title { get; set; } }
public class FreeformUsePageRequest { public string? Title { get; set; } public Sources.SourceSelection? Sources { get; set; } /// <summary>لغة الصفحة: ar (افتراضي) أو en.</summary>
    public string? Lang { get; set; } }
public class FreeformConvertRequest { public string Kind { get; set; } = ""; public string? Note { get; set; } public Sources.SourceSelection? Sources { get; set; } }
/// <summary>تحويل لوحة محادثة (مواصفتها كاملة بعناصرها وبياناتها) إلى تصميم حر من نوع معيّن — «لوحات المتابعة» داخل تصميم حر.</summary>
public class FreeformFromDashboardRequest { public string Kind { get; set; } = "dashboard"; public string? Title { get; set; } public System.Text.Json.JsonElement Dashboard { get; set; } public string? Note { get; set; } public Sources.SourceSelection? Sources { get; set; } }

/// <summary>«تصميم حر»: صفحات HTML كاملة يكتبها الموديل من المحادثة (كـ artifacts) لكل مشروع — إصدارات، تعديل يدوي، نشر في «العروض التقديمية».</summary>
[ApiController]
[Route("api/freeform")]
[Authorize]
public class FreeformController : ControllerBase
{
    private readonly FreeformStore _store;
    private readonly FreeformBuilder _builder;
    private readonly DeckBrandStore _brands;
    private readonly DeckBrandFontStore _fonts;
    private readonly PresentationStore _presentations;
    private readonly OrganizationStore _organizations;
    private readonly PermissionsService _permissions;
    private readonly AnalyticsTools _tools;
    private readonly FreeformTemplateStore _templates;
    private readonly DataFingerprintService _fingerprints;
    private readonly PresentationLiveDataService _live;

    private readonly FreeformDashboardTemplates _dashTemplates;
    public FreeformController(IWebHostEnvironment env, FreeformStore store, FreeformBuilder builder, DeckBrandStore brands, DeckBrandFontStore fonts, PresentationStore presentations, OrganizationStore organizations, PermissionsService permissions, AnalyticsTools tools, FreeformTemplateStore templates, DataFingerprintService fingerprints, PresentationLiveDataService live, FreeformDashboardTemplates dashTemplates)
    { _env = env; _store = store; _builder = builder; _brands = brands; _fonts = fonts; _presentations = presentations; _organizations = organizations; _permissions = permissions; _tools = tools; _templates = templates; _fingerprints = fingerprints; _live = live; _dashTemplates = dashTemplates; }

    /// <summary>المستخدم الحالي واختياره الفعلي للمصادر: <paramref name="requested"/> (من الطلب أو المحفوظ مع التصميم) مقيَّد بصلاحياته؛
    /// null = كل ما يحق له.</summary>
    private async Task<(AppUser User, Sources.SourceSelection Selection)?> CurrentAsync(CancellationToken ct, Sources.SourceSelection? requested = null)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return null;
        return (user, await _permissions.GetEffectiveSelectionAsync(user, requested, ct));
    }

    /// <summary>اختيار المصادر كما يُحفظ مع التصميم: null حين لم يُرسل شيء أو حين اختار المستخدم كل المصادر.</summary>
    internal static string? SourcesJsonOf(Sources.SourceSelection? s)
    {
        if (s is null) return null;
        if (s.SystemsUnset && s.FilesUnset) return null;
        return System.Text.Json.JsonSerializer.Serialize(new Sources.SourceSelection { Systems = s.Systems ?? new(), Files = s.Files ?? new() });
    }
    private static Sources.SourceSelection? StoredSources(FreeformDesign? d) => d?.SourcesSelection;

    private async Task<DeckBrand?> BrandAsync(string? projectId, CancellationToken ct)
    {
        var b = await _brands.GetAsync(projectId, ct); var fonts = await _fonts.ListAsync(projectId, ct);
        if (b is null && fonts.Count == 0) return null;
        b ??= new DeckBrand(); b.FontFaces = fonts; return b;
    }

    /// <summary>يملأ `<img data-jeem-logo>` بشعار الهوية عند العرض/النشر (المخزَّن يبقى بلا بيانات الشعار).</summary>
    internal static string ApplyBrandAssets(string html, DeckBrand? brand)
    {
        if (string.IsNullOrEmpty(html)) return html;
        if (brand?.Logo is { } logo) html = Regex.Replace(html, @"<img\b([^>]*)\bdata-jeem-logo\b([^>]*)>", m => Regex.IsMatch(m.Value, @"\bsrc\s*=") ? Regex.Replace(m.Value, @"\bsrc\s*=\s*(""[^""]*""|'[^']*')", $"src=\"{logo}\"") : $"<img{m.Groups[1].Value} data-jeem-logo{m.Groups[2].Value} src=\"{logo}\">", RegexOptions.IgnoreCase);
        else html = Regex.Replace(html, @"<img\b[^>]*\bdata-jeem-logo\b[^>]*>", "", RegexOptions.IgnoreCase);
        return html;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var list = await _store.ListAsync(cur.Value.Selection.ProjectId, ct);
        // الرابط الدائم لكل تصميم منشور (عرض واحد لكل تصميم؛ إعادة النشر تحدّثه على الرابط نفسه) — تحلّه القائمة مرة واحدة
        var presentations = await _presentations.ListAsync(cur.Value.User.OrganizationId, ct);
        var byId = presentations.ToDictionary(p => p.Id, p => p);
        var orgSlug = cur.Value.User.OrganizationId is null ? null : (await _organizations.FindByIdAsync(cur.Value.User.OrganizationId, ct))?.Slug;
        return Ok(list.Select(d => new
        {
            id = d.Id, projectId = d.ProjectId, organizationId = d.OrganizationId, createdByUserId = d.CreatedByUserId, title = d.Title, kind = d.Kind, templateKey = d.TemplateKey,
            version = d.Version, presentationId = d.PresentationId, queryCount = d.QueryCount, dataRefreshedAt = d.DataRefreshedAt, autoRefresh = d.AutoRefresh, sizeBytes = d.SizeBytes, createdAt = d.CreatedAt, updatedAt = d.UpdatedAt,
            publishedUrl = d.PresentationId is not null && byId.TryGetValue(d.PresentationId, out var pres) ? pres.PublicPath(orgSlug) : null,
            publishedActive = d.PresentationId is not null && byId.TryGetValue(d.PresentationId, out var pa) && pa.IsActive,
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var d = await _store.GetAsync(id, cur.Value.Selection.ProjectId, ct);
        if (d is null) return NotFound(new { error = "التصميم غير موجود." });
        var brand = await BrandAsync(cur.Value.Selection.ProjectId, ct);
        // تغيّر بيانات المصدر منذ آخر تحديث؟ (بصمة الجداول المذكورة في الاستعلامات المسجَّلة مقابل المحفوظة)
        bool? dataChanged = null;
        if (d.QueryCount > 0 && d.DataFingerprint is not null)
        {
            var qs = await _store.QueriesAsync(id, ct);
            var fp = await _fingerprints.ComputeAsync(qs.Select(q => q.Sql), cur.Value.Selection.ProjectId, ct);
            dataChanged = fp is not null && fp != d.DataFingerprint;
        }
        return Ok(new { design = d, html = ApplyBrandAssets(d.Html, brand), versions = await _store.VersionsAsync(id, ct), messages = await _store.MessagesAsync(id, 60, ct), publishedUrl = await PublishedUrlAsync(d, ct), dataChanged });
    }

    [HttpGet("{id}/versions/{version:int}")]
    public async Task<IActionResult> GetVersion(string id, int version, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var d = await _store.GetAsync(id, cur.Value.Selection.ProjectId, ct); if (d is null) return NotFound();
        var html = await _store.VersionHtmlAsync(id, version, ct); if (html is null) return NotFound(new { error = "الإصدار غير موجود." });
        return Ok(new { version, html = ApplyBrandAssets(html, await BrandAsync(cur.Value.Selection.ProjectId, ct)) });
    }

    /// <summary>كتالوج نماذج تصميم حر (اختياريًا لنوع واحد <c>?kind=doc</c>).</summary>
    [HttpGet("templates")]
    public async Task<IActionResult> Templates([FromQuery] string? kind, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var custom = await _templates.ListAsync(cur.Value.Selection.ProjectId, ct);
        // نماذج اللوحات من كتالوج المحادثة تُعرض تحت نوع «لوحة» بعد الثابتة (مع احترام تعديلات/إخفاء مالك المنصّة)
        var dashboards = string.IsNullOrWhiteSpace(kind) || kind == "dashboard" ? await _dashTemplates.ListAsync(ct) : Array.Empty<FreeformTemplate>();
        var all = custom.Concat(FreeformTemplates.All).Concat(dashboards);
        return Ok(string.IsNullOrWhiteSpace(kind) ? all.ToList() : all.Where(t => t.Kind == kind).ToList());
    }

    /// <summary>رفع ملف موجود (PowerPoint/PDF/HTML) وتحويله إلى نموذج مخصص للمشروع من النوع المطلوب — قراءة حتمية بلا موديل.</summary>
    [HttpPost("templates/import")]
    [RequestSizeLimit(DeckImporter.MaxBytes + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = DeckImporter.MaxBytes + 1024 * 1024)]
    public async Task<IActionResult> ImportTemplate([FromForm] IFormFile? file, [FromForm] string? kind, [FromForm] string? name, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var (user, selection) = cur.Value;
        if (file is null || file.Length == 0) return BadRequest(new { error = "اختر ملفًا بصيغة .pptx أو .pdf أو .html." });
        if (DeckImporter.KindOf(file.FileName) is null) return BadRequest(new { error = "الصيغة غير مدعومة — المدعوم: PowerPoint (.pptx) وPDF وصفحة HTML." });
        if (file.Length > DeckImporter.MaxBytes) return BadRequest(new { error = $"حجم الملف يتجاوز {DeckImporter.MaxBytes / 1024 / 1024} ميغابايت." });
        byte[] bytes; using (var ms = new MemoryStream()) { await file.CopyToAsync(ms, ct); bytes = ms.ToArray(); }
        ImportedDocument doc;
        try { doc = DeckImporter.Extract(file.FileName, bytes); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { return BadRequest(new { error = "تعذّرت قراءة الملف: " + ex.Message }); }
        if (doc.Pages.All(p => p.IsEmpty) && doc.DataBlocks.Count == 0) return BadRequest(new { error = "لم يُعثر على محتوى نصي في الملف.", warnings = doc.Warnings });
        var t = await _templates.CreateAsync(selection.ProjectId, user.Id, FreeformTemplateStore.FromDocument(doc, kind ?? "design", name, file.FileName), ct);
        return Ok(new { template = t, pagesRead = doc.Pages.Count(p => !p.IsEmpty), warnings = doc.Warnings });
    }

    /// <summary>رفع ملف ليصير تصميمًا <b>كما هو بلا أي تعديل</b> (لا موديل، لا نموذج): صفحة HTML تُحفظ حرفيًا، وملف PowerPoint يُرسم
    /// شريحةً شريحة SVG بالمحرّك الحتمي (<see cref="PptxPageBuilder"/>) في صفحة شرائح واحدة بالصور مضمّنة، ويُفرض نوع «شرائح».
    /// ثم يُفتح في المحرر ليُعدَّل نصيًا أو بالمحادثة أو يُنشر.</summary>
    /// <summary>نتيجة قراءة ملف مرفوع «كما هو»: صفحة HTML حرفيًا أو ملف PowerPoint محوَّلًا إلى صفحة شرائح بلا موديل.</summary>
    private sealed record PageFile(string Html, string Name, string? Kind, int Slides, List<string> Notes, bool IsHtml);

    /// <summary>يقرأ ملف HTML/PPTX المرفوع ويعيد الصفحة الجاهزة، أو رسالة خطأ للمستخدم (مشترك بين إنشاء تصميم جديد والاستيراد إلى تصميم قائم).</summary>
    private static async Task<(PageFile? page, string? error)> ReadPageFileAsync(IFormFile? file, string? title, CancellationToken ct)
    {
        if (file is null || file.Length == 0) return (null, "اختر ملفًا بصيغة .html أو .pptx.");
        var isHtml = Regex.IsMatch(file.FileName, @"\.html?$", RegexOptions.IgnoreCase);
        var isPptx = Regex.IsMatch(file.FileName, @"\.pptx$", RegexOptions.IgnoreCase);
        if (!isHtml && !isPptx) return (null, "هذا الخيار لصفحات HTML وملفات PowerPoint (.pptx) فقط. لملفات PDF استخدم «رفع من ملف موجود».");
        if (file.Length > DeckImporter.MaxBytes) return (null, $"حجم الملف يتجاوز {DeckImporter.MaxBytes / 1024 / 1024} ميغابايت.");
        string html; string? name = string.IsNullOrWhiteSpace(title) ? null : title.Trim(); var notes = new List<string>(); int slides = 0; string? kind = null;
        if (isHtml)
        {
            using var reader = new StreamReader(file.OpenReadStream(), new UTF8Encoding(false), detectEncodingFromByteOrderMarks: true);
            html = await reader.ReadToEndAsync(ct);
            if (string.IsNullOrWhiteSpace(html)) return (null, "الملف فارغ.");
            name ??= TitleFromHtml(html) ?? Path.GetFileNameWithoutExtension(file.FileName);
        }
        else
        {
            byte[] bytes; using (var ms = new MemoryStream()) { await file.CopyToAsync(ms, ct); bytes = ms.ToArray(); }
            name ??= Path.GetFileNameWithoutExtension(file.FileName);
            PptxPageBuilder.Result built;
            try { built = PptxPageBuilder.Build(bytes, name); }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex) { return (null, "تعذّرت قراءة ملف PowerPoint: " + ex.Message); }
            html = built.Html; slides = built.SlideCount; kind = "slides";
            if (built.Unsupported.Count > 0) notes.Add($"{built.Unsupported.Count} صورة بصيغة EMF/WMF غير مدعومة تظهر بإطار «غير مدعومة»");
        }
        if (Encoding.UTF8.GetByteCount(html) > FreeformStore.MaxHtmlBytes) return (null, $"الصفحة الناتجة تتجاوز {FreeformStore.MaxHtmlBytes / 1024 / 1024} ميغابايت (صورها مضمّنة).");
        if (name.Length > 120) name = name[..120];
        return (new PageFile(html, name, kind, slides, notes, isHtml), null);
    }

    /// <summary>رفع صفحة HTML كما هي، أو ملف PowerPoint يُحوَّل حتميًا إلى صفحة شرائح — تصميم جديد بلا موديل.</summary>
    [HttpPost("import-page")]
    [RequestSizeLimit(DeckImporter.MaxBytes + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = DeckImporter.MaxBytes + 1024 * 1024)]
    public async Task<IActionResult> ImportPage([FromForm] IFormFile? file, [FromForm] string? kind, [FromForm] string? title, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var (user, selection) = cur.Value;
        var (page, error) = await ReadPageFileAsync(file, title, ct);
        if (page is null) return BadRequest(new { error });
        var design = await _store.CreateAsync(selection.ProjectId, user.OrganizationId, user.Id, page.Name, page.Html, $"رفع الملف «{file!.FileName}» كما هو", page.Kind ?? kind, null, ct);
        await _store.AddMessageAsync(design.Id, "user", $"رفع الملف «{file.FileName}» كما هو بلا تعديل.", null, ct);
        var reply = page.IsHtml
            ? "وُضعت الصفحة كما رُفعت حرفيًا (بلا أي تعديل من الموديل). يمكنك الآن تعديل نصها من مساحة العمل، أو طلب تعديلات بالمحادثة، أو نشرها برابط."
            : $"حُوِّل العرض إلى صفحة شرائح كما هو: {page.Slides} شريحة مرسومة بمواضعها وخلفياتها وصورها الأصلية بلا موديل{(page.Notes.Count > 0 ? " — " + string.Join("؛ ", page.Notes) : "")}. يمكنك تعديل النصوص من مساحة العمل أو نشره برابط.";
        await _store.AddMessageAsync(design.Id, "assistant", reply, 1, ct);
        var brand = await BrandAsync(selection.ProjectId, ct);
        return Ok(new { design, html = ApplyBrandAssets(design.Html, brand), versions = await _store.VersionsAsync(design.Id, ct), messages = await _store.MessagesAsync(design.Id, 60, ct), slides = page.Slides, notes = page.Notes });
    }

    /// <summary>
    /// استيراد ملف (HTML كما هو، أو PowerPoint محوَّلًا إلى صفحة شرائح) إلى تصميم قائم: إصدار جديد على المعرّف نفسه مع بقاء العنوان
    /// والرسائل والمصادر ورابط النشر (الصفحة المنشورة تتغير عند «تحديث النشر» فقط). ملف PowerPoint يحوّل نوع التصميم إلى شرائح.
    /// </summary>
    [HttpPost("{id}/import-page")]
    [RequestSizeLimit(DeckImporter.MaxBytes + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = DeckImporter.MaxBytes + 1024 * 1024)]
    public async Task<IActionResult> ImportPageInto(string id, [FromForm] IFormFile? file, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var selection = cur.Value.Selection;
        var d = await _store.GetAsync(id, selection.ProjectId, ct); if (d is null) return NotFound(new { error = "التصميم غير موجود." });
        var (page, error) = await ReadPageFileAsync(file, null, ct);
        if (page is null) return BadRequest(new { error });
        var kindChanged = page.Kind is not null && page.Kind != d.Kind;
        if (kindChanged) await _store.SetKindAsync(id, page.Kind!, ct);
        var v = await _store.AddVersionAsync(id, page.Html, $"استيراد الملف «{file!.FileName}» كما هو", null, ct);
        await _store.AddMessageAsync(id, "user", $"استيراد الملف «{file.FileName}» إلى هذا التصميم كما هو.", null, ct);
        var reply = page.IsHtml
            ? $"استُبدلت صفحة التصميم بالملف المرفوع حرفيًا (الإصدار {v})؛ الإصدارات السابقة محفوظة ويمكن استعادتها من قائمة الإصدارات."
            : $"استُبدلت صفحة التصميم بالعرض المرفوع كصفحة شرائح: {page.Slides} شريحة مرسومة كما هي بلا موديل (الإصدار {v}){(page.Notes.Count > 0 ? " — " + string.Join("؛ ", page.Notes) : "")}.";
        if (d.PresentationId is not null) reply += " الرابط المنشور يعرض النسخة المنشورة السابقة حتى تضغط «تحديث النشر».";
        await _store.AddMessageAsync(id, "assistant", reply, v, ct);
        var design = await _store.GetAsync(id, selection.ProjectId, ct) ?? d;
        var brand = await BrandAsync(selection.ProjectId, ct);
        return Ok(new { design, html = ApplyBrandAssets(design.Html, brand), version = v, versions = await _store.VersionsAsync(id, ct), messages = await _store.MessagesAsync(id, 60, ct), slides = page.Slides, notes = page.Notes, kindChanged, published = d.PresentationId is not null });
    }

    /// <summary>عنوان الصفحة من <c>&lt;title&gt;</c> أو أول <c>&lt;h1&gt;</c> (بلا وسوم)، أو null.</summary>
    internal static string? TitleFromHtml(string html)
    {
        foreach (var rx in new[] { @"<title\b[^>]*>([\s\S]*?)</title>", @"<h1\b[^>]*>([\s\S]*?)</h1>" })
        {
            var m = Regex.Match(html, rx, RegexOptions.IgnoreCase);
            if (!m.Success) continue;
            var t = System.Net.WebUtility.HtmlDecode(Regex.Replace(Regex.Replace(m.Groups[1].Value, "<[^>]+>", ""), @"\s+", " ")).Trim();
            if (t.Length > 0) return t;
        }
        return null;
    }

    [HttpDelete("templates/{key}")]
    public async Task<IActionResult> DeleteTemplate(string key, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var t = await _templates.GetAsync(key, cur.Value.Selection.ProjectId, ct); if (t is null) return NotFound();
        if (t.CreatedByUserId != cur.Value.User.Id && cur.Value.User.Role != "Admin") return Forbid();
        await _templates.DeleteAsync(key, cur.Value.Selection.ProjectId, ct); return NoContent();
    }

    private readonly IWebHostEnvironment _env;

    /// <summary>ملف HTML لنموذج صفحة جاهزة من wwwroot (null إن لم يكن للنموذج صفحة أو الملف مفقود).</summary>
    private async Task<string?> TemplatePageHtmlAsync(FreeformTemplate t, CancellationToken ct, string? lang = null)
    {
        // الإنجليزية إن طُلبت ووُجد ملفها، وإلا العربية
        var path = lang == "en" && !string.IsNullOrWhiteSpace(t.PageEn) ? t.PageEn : t.Page;
        if (string.IsNullOrWhiteSpace(path) || path.Contains("..")) return null;
        var file = _env.WebRootFileProvider.GetFileInfo(path);
        if (!file.Exists || file.IsDirectory) return null;
        using var reader = new StreamReader(file.CreateReadStream(), Encoding.UTF8);
        return await reader.ReadToEndAsync(ct);
    }

    /// <summary>صفحة نموذج جاهز كما هي (لمعاينة المعرض).</summary>
    [HttpGet("templates/{key}/page")]
    public async Task<IActionResult> TemplatePage(string key, [FromQuery] string? lang, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var t = FreeformTemplates.Find(key); if (t is null) return NotFound();
        var html = await TemplatePageHtmlAsync(t, ct, lang); if (html is null) return NotFound(new { error = "هذا النموذج بلا صفحة جاهزة." });
        return Content(html, "text/html; charset=utf-8");
    }

    /// <summary>«استخدم هذا النموذج» لنموذج صفحة جاهزة: تصميم جديد من ملف الصفحة فورًا بلا موديل، بنوع النموذج ومفتاحه، ثم يُعدَّل نصيًا أو بالمحادثة.</summary>
    [HttpPost("templates/{key}/use")]
    public async Task<IActionResult> UseTemplatePage(string key, [FromBody] FreeformUsePageRequest? request, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var (user, selection) = cur.Value;
        var t = FreeformTemplates.Find(key); if (t is null) return NotFound();
        var en = request?.Lang == "en";
        var html = await TemplatePageHtmlAsync(t, ct, request?.Lang); if (html is null) return BadRequest(new { error = "هذا النموذج بلا صفحة جاهزة." });
        var title = string.IsNullOrWhiteSpace(request?.Title) ? (en && t.NameEn is not null ? t.NameEn : t.Name).Split('—')[0].Trim() : request!.Title!.Trim();
        if (title.Length > 120) title = title[..120];
        var design = await _store.CreateAsync(selection.ProjectId, user.OrganizationId, user.Id, title, html, $"بدء من النموذج الجاهز «{t.Name}»", t.Kind, t.Key, ct, SourcesJsonOf(request?.Sources));
        await _store.AddMessageAsync(design.Id, "user", $"ابدأ من النموذج الجاهز «{t.Name}».", null, ct);
        await _store.AddMessageAsync(design.Id, "assistant", $"فُتح النموذج «{t.Name}» بشرائحه الـ{t.Sections.Count} كما هو (بلا موديل). عدّل النصوص مباشرة من «✏️ تعديل النص»، أو اكتب هنا ما تريد تغييره (مثل: «املأ الشريحة الخامسة بأرقام المبيعات الفعلية»).", 1, ct);
        var brand = await BrandAsync(selection.ProjectId, ct);
        return Ok(new { design, html = ApplyBrandAssets(design.Html, brand), versions = await _store.VersionsAsync(design.Id, ct), messages = await _store.MessagesAsync(design.Id, 60, ct) });
    }

    private async Task<FreeformTemplate?> ResolveTemplateAsync(string? key, string? projectId, CancellationToken ct)
        => string.IsNullOrWhiteSpace(key) ? null : FreeformTemplates.Find(key) ?? await _dashTemplates.FindAsync(key, ct) ?? await _templates.GetAsync(key, projectId, ct);

    /// <summary>تحويل تصميم إلى نوع آخر (مستند ← شرائح…): تصميم جديد من النوع المطلوب بالمحتوى نفسه، والأصل يبقى كما هو.</summary>
    [HttpPost("{id}/convert")]
    public async Task<IActionResult> Convert(string id, [FromBody] FreeformConvertRequest request, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var (user, selection) = cur.Value;
        var src = await _store.GetAsync(id, selection.ProjectId, ct); if (src is null) return NotFound(new { error = "التصميم غير موجود." });
        if (!FreeformStore.Kinds.Contains(request.Kind ?? "")) return BadRequest(new { error = "نوع غير معروف." });
        if (request.Kind == src.Kind) return BadRequest(new { error = "التصميم من هذا النوع بالفعل." });
        if (string.IsNullOrWhiteSpace(src.Html)) return BadRequest(new { error = "التصميم بلا صفحة بعد." });
        var wantedSrc = request.Sources ?? StoredSources(src);
        if (wantedSrc is not null) selection = (await CurrentAsync(ct, wantedSrc))!.Value.Selection;
        var fromLabel = FreeformBuilder.KindLabel(src.Kind); var toLabel = FreeformBuilder.KindLabel(request.Kind);
        var message = $"حوّل الصفحة الحالية (وهي {fromLabel}) إلى {toLabel} كاملة: احتفظ بكل المحتوى والنصوص والأرقام والجداول والهوية البصرية نفسها، وأعد تنظيمه وفق بنية «{toLabel}» الموضحة في التعليمات."
            + (string.IsNullOrWhiteSpace(request.Note) ? "" : $"\nملاحظات المستخدم: {request.Note.Trim()}");
        var context = await _tools.DescribeSourcesAsync(selection, ct);
        var brand = await BrandAsync(selection.ProjectId, ct);
        FreeformResult result;
        try { result = await _builder.GenerateAsync(message, src.Html, Array.Empty<FreeformMessage>(), context, brand, user, ct, request.Kind!); }
        catch (InvalidOperationException ex) { return StatusCode(503, new { error = ex.Message }); }
        if (string.IsNullOrWhiteSpace(result.Html)) return StatusCode(502, new { error = "لم يُنتج الموديل صفحة من التحويل — جرّب ثانية أو أضف ملاحظة." });
        var design = await _store.CreateAsync(selection.ProjectId, user.OrganizationId, user.Id, $"{src.Title} ({toLabel})", result.Html, message, request.Kind, null, ct, SourcesJsonOf(wantedSrc));
        await _store.AddMessageAsync(design.Id, "user", $"تحويل «{src.Title}» من {fromLabel} إلى {toLabel}", null, ct);
        await _store.AddMessageAsync(design.Id, "assistant", result.Reply, design.Version, ct);
        return Ok(new { design, html = ApplyBrandAssets(design.Html, brand), reply = result.Reply, sourceId = src.Id });
    }

    /// <summary>
    /// تحويل لوحة متابعة (لوحة محادثة) إلى تصميم حر: نداء بناء واحد يتلقّى مواصفة اللوحة كاملة (الملخص، العناصر بعناوينها
    /// وأنواعها وبياناتها وجمل SQL) ويُنتج صفحة HTML مستقلة من النوع المطلوب؛ اللوحة الأصلية تبقى في السجل كما هي.
    /// </summary>
    [HttpPost("from-dashboard")]
    public async Task<IActionResult> FromDashboard([FromBody] FreeformFromDashboardRequest request, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var (user, selection) = cur.Value;
        var kind = FreeformStore.NormalizeKind(request.Kind);
        if (request.Sources is not null) selection = (await CurrentAsync(ct, request.Sources))!.Value.Selection;
        if (request.Dashboard.ValueKind != System.Text.Json.JsonValueKind.Object) return BadRequest(new { error = "مواصفة اللوحة مطلوبة." });
        var spec = DashboardSpecText(request.Dashboard, out var sqls);
        if (string.IsNullOrWhiteSpace(spec)) return BadRequest(new { error = "اللوحة بلا عناصر." });
        var toLabel = FreeformBuilder.KindLabel(kind);
        var title = string.IsNullOrWhiteSpace(request.Title) ? "لوحة متابعة" : request.Title.Trim();
        var message = $"حوّل لوحة المتابعة التالية إلى {toLabel} كاملة بصفحة HTML مستقلة: احتفظ بكل العناصر بعناوينها وأنواعها وأرقامها كما هي، وبالترتيب نفسه، وبهوية المشروع."
            + (sqls.Count > 0 ? "\nكل عنصر له جملة SQL: نفّذها بأداة query_data لتقرأ أحدث الصفوف وتُسجَّل في JEEM_DATA، واقرأ الأرقام منها في جافاسكربت الصفحة لا حرفيًا." : "")
            + (string.IsNullOrWhiteSpace(request.Note) ? "" : $"\nملاحظات المستخدم: {request.Note.Trim()}")
            + "\n\n# مواصفة اللوحة\n" + spec;
        var context = await _tools.DescribeSourcesAsync(selection, ct);
        var brand = await BrandAsync(selection.ProjectId, ct);
        FreeformResult result;
        try { result = await _builder.GenerateAsync(message, null, Array.Empty<FreeformMessage>(), context, brand, user, ct, kind); }
        catch (InvalidOperationException ex) { return StatusCode(503, new { error = ex.Message }); }
        if (string.IsNullOrWhiteSpace(result.Html)) return StatusCode(502, new { error = "لم يُنتج الموديل صفحة من التحويل — جرّب ثانية أو أضف ملاحظة." });
        var design = await _store.CreateAsync(selection.ProjectId, user.OrganizationId, user.Id, $"{title} ({toLabel})", result.Html, message, kind, null, ct, SourcesJsonOf(request.Sources));
        if (result.Queries is { Count: > 0 } qs) { var fp = await _fingerprints.ComputeAsync(qs.Select(q => q.Sql), selection.ProjectId, ct); await _store.SetQueriesAsync(design.Id, qs, DateTime.UtcNow, fp, ct); }
        await _store.AddMessageAsync(design.Id, "user", $"تحويل لوحة المتابعة «{title}» إلى {toLabel}", null, ct);
        await _store.AddMessageAsync(design.Id, "assistant", result.Reply, design.Version, ct);
        return Ok(new { design, html = ApplyBrandAssets(design.Html, brand), reply = result.Reply });
    }

    /// <summary>نص موجز لمواصفة اللوحة يُرسل للموديل: الملخص ثم كل عنصر (العنوان، النوع، الحجم، SQL، والبيانات مقتطعة).</summary>
    internal static string DashboardSpecText(System.Text.Json.JsonElement d, out List<string> sqls)
    {
        sqls = new List<string>(); var sb = new StringBuilder();
        if (d.TryGetProperty("name", out var n) && n.ValueKind == System.Text.Json.JsonValueKind.String) sb.AppendLine($"العنوان: {n.GetString()}");
        if (d.TryGetProperty("summary", out var sm) && sm.ValueKind == System.Text.Json.JsonValueKind.String) sb.AppendLine($"الملخص: {sm.GetString()}");
        if (!d.TryGetProperty("widgets", out var ws) || ws.ValueKind != System.Text.Json.JsonValueKind.Array || ws.GetArrayLength() == 0) return "";
        var i = 0;
        foreach (var w in ws.EnumerateArray())
        {
            i++;
            var title = w.TryGetProperty("title", out var t) ? t.GetString() : null;
            var type = w.TryGetProperty("type", out var ty) ? ty.GetString() : null;
            var size = w.TryGetProperty("layout", out var l) && l.ValueKind == System.Text.Json.JsonValueKind.Object && l.TryGetProperty("size", out var sz) ? sz.GetString() : null;
            sb.AppendLine($"## العنصر {i}: {title} (النوع: {type}{(size is null ? "" : $"، الحجم: {size}")})");
            if (w.TryGetProperty("page", out var pg) && pg.ValueKind == System.Text.Json.JsonValueKind.String) sb.AppendLine($"الصفحة/التبويب: {pg.GetString()}");
            if (w.TryGetProperty("query", out var q) && q.ValueKind == System.Text.Json.JsonValueKind.Object && q.TryGetProperty("sql", out var sq) && sq.ValueKind == System.Text.Json.JsonValueKind.String && !string.IsNullOrWhiteSpace(sq.GetString()))
            { sqls.Add(sq.GetString()!); sb.AppendLine($"SQL: {sq.GetString()}"); }
            if (w.TryGetProperty("data", out var data) && data.ValueKind != System.Text.Json.JsonValueKind.Undefined && data.ValueKind != System.Text.Json.JsonValueKind.Null)
            { var raw = data.GetRawText(); sb.AppendLine($"البيانات: {(raw.Length > 2500 ? raw[..2500] + "…(اقتُطع)" : raw)}"); }
            if (w.TryGetProperty("noData", out var nd) && nd.ValueKind == System.Text.Json.JsonValueKind.True) sb.AppendLine("(بلا بيانات — اعرضه كعنصر فارغ بعنوانه)");
        }
        return sb.Length > 60_000 ? sb.ToString(0, 60_000) + "\n…(اقتُطع)" : sb.ToString();
    }

    /// <summary>توليد أو تعديل بالمحادثة. بلا <c>designId</c> يُنشأ تصميم جديد بعنوان من الطلب.</summary>
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] FreeformGenerateRequest request, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        if (string.IsNullOrWhiteSpace(request.Message)) return BadRequest(new { error = "اكتب ما تريد تصميمه." });
        var (user, selection) = cur.Value;
        FreeformDesign? design = null; IReadOnlyList<FreeformMessage> history = Array.Empty<FreeformMessage>();
        if (!string.IsNullOrWhiteSpace(request.DesignId))
        {
            design = await _store.GetAsync(request.DesignId, selection.ProjectId, ct);
            if (design is null) return NotFound(new { error = "التصميم غير موجود." });
            history = await _store.MessagesAsync(design.Id, 16, ct);
        }
        // المصادر: المرسلة في الطلب تغلب، وإلا المحفوظة مع التصميم، وإلا كل ما يحق للمستخدم — مقيَّدة بصلاحياته دائمًا.
        var wanted = request.Sources ?? StoredSources(design);
        if (wanted is not null) selection = (await CurrentAsync(ct, wanted))!.Value.Selection;
        var sourcesJson = SourcesJsonOf(request.Sources);
        var currentHtml = !string.IsNullOrWhiteSpace(request.Html) ? request.Html : design?.Html;
        var context = await _tools.DescribeSourcesAsync(selection, ct);
        var brand = await BrandAsync(selection.ProjectId, ct);
        FreeformResult result;
        var kind = FreeformStore.NormalizeKind(design?.Kind ?? request.Kind);
        // النموذج: المحفوظ مع التصميم (يُبقي البنية عبر التعديلات) وإلا المختار في الطلب الأول
        var template = await ResolveTemplateAsync(design?.TemplateKey ?? request.TemplateKey, selection.ProjectId, ct);
        if (template is not null && template.Kind != kind) template = null;
        try { result = await _builder.GenerateAsync(request.Message, currentHtml, history, context, brand, user, ct, kind, template, design is null ? request.Lang : null); }
        catch (InvalidOperationException ex) { return StatusCode(503, new { error = ex.Message }); }
        int? version = null;
        if (design is null)
        {
            var title = TitleFrom(request.Message);
            design = await _store.CreateAsync(selection.ProjectId, user.OrganizationId, user.Id, title, result.Html ?? "", request.Message, kind, template?.Key, ct, sourcesJson);
            version = design.Version > 0 ? design.Version : null;
        }
        else
        {
            if (result.Html is not null) { version = await _store.AddVersionAsync(design.Id, result.Html, request.Message, null, ct); design.Version = version.Value; design.Html = result.Html; }
            if (request.Sources is not null && sourcesJson != design.SourcesJson) { await _store.SetSourcesAsync(design.Id, sourcesJson, ct); design.SourcesJson = sourcesJson; }
        }
        if (result.Html is not null && result.Queries is { Count: > 0 } qs) { var now = DateTime.UtcNow; var fp = await _fingerprints.ComputeAsync(qs.Select(q => q.Sql), selection.ProjectId, ct); await _store.SetQueriesAsync(design.Id, qs, now, fp, ct); design.QueryCount = qs.Count; design.DataRefreshedAt = now; design.DataFingerprint = fp; }
        await _store.AddMessageAsync(design.Id, "user", request.Message, null, ct);
        await _store.AddMessageAsync(design.Id, "assistant", result.Reply, version, ct);
        return Ok(new { design, html = ApplyBrandAssets(design.Html, brand), reply = result.Reply, version, usedData = result.UsedData, versions = await _store.VersionsAsync(design.Id, ct) });
    }

    /// <summary>تحديث بيانات الصفحة بلا موديل: إعادة تنفيذ الاستعلامات المسجَّلة واستبدال كتلة <c>jeem-data</c> كإصدار جديد.</summary>
    [HttpPost("{id}/refresh")]
    public async Task<IActionResult> Refresh(string id, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var (user, selection) = cur.Value;
        var d = await _store.GetAsync(id, selection.ProjectId, ct); if (d is null) return NotFound(new { error = "التصميم غير موجود." });
        if (StoredSources(d) is { } storedSel) selection = (await CurrentAsync(ct, storedSel))!.Value.Selection; // المصادر المختارة للتصميم
        var stored = await _store.QueriesAsync(id, ct);
        if (stored.Count == 0) stored = FreeformDataBlock.Parse(d.Html); // تصميم قديم فيه كتلة بلا تسجيل في الجدول
        if (stored.Count == 0) return BadRequest(new { error = "هذا التصميم بلا استعلامات مسجَّلة (بُني قبل عقد البيانات أو بلا بيانات). استخدم «إعادة البناء بالبيانات الحالية» بالموديل.", noQueries = true });
        var context = await _tools.DescribeSourcesAsync(selection, ct);
        var fresh = new List<FreeformQuery>(); var failed = new List<object>();
        foreach (var q in stored)
        {
            var (res, isError) = await _tools.RunReadOnlyQueryAsync(q.Sql, context, ct);
            if (isError) { failed.Add(new { key = q.Key, error = res }); fresh.Add(q with { Result = FreeformDataBlock.Parse(d.Html).FirstOrDefault(x => x.Key == q.Key)?.Result }); continue; }
            JsonNode? parsed; try { parsed = JsonNode.Parse(res); } catch (JsonException) { parsed = null; }
            fresh.Add(q with { Result = parsed });
        }
        if (failed.Count == stored.Count) return StatusCode(502, new { error = "فشلت كل الاستعلامات — ربما تغيّرت أعمدة البيانات. راجع المصادر أو أعد البناء بالموديل.", failed });
        var now = DateTime.UtcNow;
        var html = FreeformDataBlock.Inject(d.Html, fresh, now);
        var version = await _store.AddVersionAsync(id, html, $"تحديث البيانات ({fresh.Count - failed.Count} من {stored.Count} استعلامًا)", null, ct);
        await _store.SetQueriesAsync(id, fresh, now, await _fingerprints.ComputeAsync(fresh.Select(q => q.Sql), selection.ProjectId, ct), ct);
        if (d.PresentationId is not null) _live.Invalidate(d.PresentationId);
        await _store.AddMessageAsync(id, "assistant", failed.Count == 0 ? $"حُدِّثت البيانات من {stored.Count} استعلامًا." : $"حُدِّثت البيانات من {stored.Count - failed.Count} استعلامًا وفشل {failed.Count}.", version, ct);
        var brand = await BrandAsync(selection.ProjectId, ct);
        return Ok(new { version, html = ApplyBrandAssets(html, brand), refreshedAt = now, ok = stored.Count - failed.Count, failed, versions = await _store.VersionsAsync(id, ct) });
    }

    /// <summary>حفظ تعديل يدوي للكود (إصدار جديد) و/أو تغيير العنوان.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Save(string id, [FromBody] FreeformSaveRequest request, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var d = await _store.GetAsync(id, cur.Value.Selection.ProjectId, ct); if (d is null) return NotFound(new { error = "التصميم غير موجود." });
        int? version = null;
        if (request.AutoRefresh is { } ar) await _store.SetAutoRefreshAsync(id, ar, ct);
        // تغيير المصادر المختارة للتصميم من الواجهة (null مع ClearSources = العودة إلى كل المصادر)
        if (request.Sources is not null || request.ClearSources == true) { var sj = request.ClearSources == true ? null : SourcesJsonOf(request.Sources); await _store.SetSourcesAsync(id, sj, ct); d.SourcesJson = sj; }
        if (!string.IsNullOrWhiteSpace(request.Html))
        {
            if (Encoding.UTF8.GetByteCount(request.Html) > FreeformStore.MaxHtmlBytes) return BadRequest(new { error = "الصفحة تتجاوز 4 ميغابايت." });
            if (!string.Equals(request.Html, d.Html, StringComparison.Ordinal)) { version = await _store.AddVersionAsync(id, request.Html, request.Note ?? "تعديل يدوي للكود", request.Title?.Trim(), ct); await _store.AddMessageAsync(id, "assistant", "حُفظ تعديل يدوي للكود.", version, ct); }
        }
        if (version is null && !string.IsNullOrWhiteSpace(request.Title)) await _store.SetTitleAsync(id, request.Title.Trim(), ct);
        return Ok(new { version, versions = await _store.VersionsAsync(id, ct) });
    }

    [HttpPost("{id}/restore/{version:int}")]
    public async Task<IActionResult> Restore(string id, int version, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var d = await _store.GetAsync(id, cur.Value.Selection.ProjectId, ct); if (d is null) return NotFound(new { error = "التصميم غير موجود." });
        var html = await _store.VersionHtmlAsync(id, version, ct); if (html is null) return NotFound(new { error = "الإصدار غير موجود." });
        var v = await _store.AddVersionAsync(id, html, $"استعادة الإصدار {version}", null, ct);
        await _store.AddMessageAsync(id, "assistant", $"استُعيد الإصدار {version} كإصدار {v}.", v, ct);
        return Ok(new { version = v, html = ApplyBrandAssets(html, await BrandAsync(cur.Value.Selection.ProjectId, ct)), versions = await _store.VersionsAsync(id, ct) });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var d = await _store.GetAsync(id, cur.Value.Selection.ProjectId, ct); if (d is null) return NotFound(new { error = "التصميم غير موجود." });
        if (d.CreatedByUserId != cur.Value.User.Id && cur.Value.User.Role != UserRoles.Admin) return StatusCode(403, new { error = "الحذف لصاحب التصميم أو المسؤول." });
        await _store.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>النشر في «العروض التقديمية» (أو تحديث المنشور سابقًا على نفس الرابط) بالصفحة كما هي مع شعار الهوية.</summary>
    [HttpPost("{id}/publish")]
    public async Task<IActionResult> Publish(string id, [FromBody] FreeformPublishRequest request, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var (user, selection) = cur.Value;
        var d = await _store.GetAsync(id, selection.ProjectId, ct); if (d is null) return NotFound(new { error = "التصميم غير موجود." });
        if (string.IsNullOrWhiteSpace(d.Html)) return BadRequest(new { error = "لا توجد صفحة لنشرها بعد." });
        var html = ApplyBrandAssets(d.Html, await BrandAsync(selection.ProjectId, ct));
        if (d.Kind == "slides") html = FreeformDataBlock.EnsureSlidesKit(html);
        // صفحة ببيانات: تُربط بمصدرها فتُقدَّم بآخر الأرقام عند كل فتح (بيانات حيّة) ويضمَن فيها الجسر لتحديثها وهي مفتوحة.
        var hasData = d.QueryCount > 0 || FreeformDataBlock.HasBlock(d.Html);
        if (hasData) html = FreeformDataBlock.EnsurePageBridge(html);
        var title = string.IsNullOrWhiteSpace(request.Title) ? d.Title : request.Title.Trim();
        var orgSlug = user.OrganizationId is null ? null : (await _organizations.FindByIdAsync(user.OrganizationId, ct))?.Slug;
        Presentation? existing = d.PresentationId is null ? null : await _presentations.GetAsync(d.PresentationId, ct);
        if (existing is not null && string.Equals(existing.OrganizationId, user.OrganizationId, StringComparison.Ordinal))
        {
            await _presentations.ReplaceAsync(existing.Id, html, Slugify(title) + ".html", title, ct);
            await _presentations.SetSourceAsync(existing.Id, PresentationLiveDataService.KindFreeform, d.Id, hasData && (existing.SourceKind is null || existing.LiveData), ct);
            _live.Invalidate(existing.Id);
            return Ok(new { presentationId = existing.Id, url = existing.PublicPath(orgSlug), updated = true, liveData = hasData && (existing.SourceKind is null || existing.LiveData) });
        }
        var slug = PresentationSlug.Normalize(string.IsNullOrWhiteSpace(request.Slug) ? title : request.Slug);
        if (slug.Length < 2) slug = "design";
        var baseSlug = slug; var n = 1;
        while (await _presentations.SlugTakenAsync(user.OrganizationId, slug, null, ct)) slug = $"{baseSlug}-{++n}";
        var p = new Presentation
        {
            OrganizationId = user.OrganizationId, CreatedByUserId = user.Id, CreatedByName = user.DisplayName ?? user.Username,
            Title = title, Slug = slug, FileName = Slugify(title) + ".html", Html = html, SizeBytes = Encoding.UTF8.GetByteCount(html),
        };
        await _presentations.SaveAsync(p, ct);
        await _presentations.SetSourceAsync(p.Id, PresentationLiveDataService.KindFreeform, d.Id, hasData, ct);
        await _store.SetPresentationAsync(d.Id, p.Id, ct);
        return Ok(new { presentationId = p.Id, url = p.PublicPath(orgSlug), updated = false, liveData = hasData });
    }

    private async Task<string?> PublishedUrlAsync(FreeformDesign d, CancellationToken ct)
    {
        if (d.PresentationId is null) return null;
        var p = await _presentations.GetAsync(d.PresentationId, ct); if (p is null) return null;
        var orgSlug = p.OrganizationId is null ? null : (await _organizations.FindByIdAsync(p.OrganizationId, ct))?.Slug;
        return p.PublicPath(orgSlug);
    }

    internal static string TitleFrom(string message)
    {
        var t = Regex.Replace(message.Trim(), @"\s+", " ");
        t = Regex.Replace(t, @"^(اعمل|اعملي|صمّم|صمم|ابني|ابنِ|أنشئ|انشئ|اكتب|عايز|عاوز|محتاج|اريد|أريد)\s+(لي\s+|لى\s+)?", "");
        t = Regex.Replace(t, @"^(صفحة|عرض|لوحة|تصميم)\s+", "");
        if (t.Length > 60) t = t[..60].TrimEnd() + "…";
        return t.Length > 0 ? t : "تصميم جديد";
    }
    private static string Slugify(string s) { var x = PresentationSlug.Normalize(s); return x.Length == 0 ? "design" : x; }
}
