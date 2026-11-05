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

public class FreeformGenerateRequest { public string Message { get; set; } = ""; public string? DesignId { get; set; } public string? Html { get; set; } public string? Kind { get; set; } public string? TemplateKey { get; set; } }
public class FreeformSaveRequest { public string? Html { get; set; } public string? Title { get; set; } public string? Note { get; set; } public bool? AutoRefresh { get; set; } }
public class FreeformPublishRequest { public string? Slug { get; set; } public string? Title { get; set; } }
public class FreeformConvertRequest { public string Kind { get; set; } = ""; public string? Note { get; set; } }

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

    public FreeformController(FreeformStore store, FreeformBuilder builder, DeckBrandStore brands, DeckBrandFontStore fonts, PresentationStore presentations, OrganizationStore organizations, PermissionsService permissions, AnalyticsTools tools, FreeformTemplateStore templates, DataFingerprintService fingerprints, PresentationLiveDataService live)
    { _store = store; _builder = builder; _brands = brands; _fonts = fonts; _presentations = presentations; _organizations = organizations; _permissions = permissions; _tools = tools; _templates = templates; _fingerprints = fingerprints; _live = live; }

    private async Task<(AppUser User, Sources.SourceSelection Selection)?> CurrentAsync(CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return null;
        return (user, await _permissions.GetEffectiveSelectionAsync(user, null, ct));
    }

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
        return Ok(await _store.ListAsync(cur.Value.Selection.ProjectId, ct));
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
        var all = custom.Concat(FreeformTemplates.All);
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

    /// <summary>رفع صفحة HTML لتصير تصميمًا <b>كما هي بلا أي تعديل</b> (لا موديل، لا نموذج): الملف يُحفظ حرفيًا كإصدار 1 ويُفتح في
    /// المحرر ليُعدَّل نصيًا أو بالمحادثة أو يُنشر. الحجم بحد صفحة التصميم (4MB).</summary>
    [HttpPost("import-page")]
    [RequestSizeLimit(FreeformStore.MaxHtmlBytes + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = FreeformStore.MaxHtmlBytes + 1024 * 1024)]
    public async Task<IActionResult> ImportPage([FromForm] IFormFile? file, [FromForm] string? kind, [FromForm] string? title, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var (user, selection) = cur.Value;
        if (file is null || file.Length == 0) return BadRequest(new { error = "اختر ملف صفحة بصيغة .html." });
        if (!Regex.IsMatch(file.FileName, @"\.html?$", RegexOptions.IgnoreCase)) return BadRequest(new { error = "هذا الخيار لصفحات HTML فقط (.html أو .htm). لملفات PowerPoint وPDF استخدم «رفع من ملف موجود»." });
        if (file.Length > FreeformStore.MaxHtmlBytes) return BadRequest(new { error = $"حجم الصفحة يتجاوز {FreeformStore.MaxHtmlBytes / 1024 / 1024} ميغابايت." });
        string html;
        using (var reader = new StreamReader(file.OpenReadStream(), new UTF8Encoding(false), detectEncodingFromByteOrderMarks: true)) html = await reader.ReadToEndAsync(ct);
        if (string.IsNullOrWhiteSpace(html)) return BadRequest(new { error = "الملف فارغ." });
        var name = string.IsNullOrWhiteSpace(title) ? TitleFromHtml(html) ?? Path.GetFileNameWithoutExtension(file.FileName) : title.Trim();
        if (name.Length > 120) name = name[..120];
        var design = await _store.CreateAsync(selection.ProjectId, user.OrganizationId, user.Id, name, html, $"رفع الصفحة «{file.FileName}» كما هي", kind, null, ct);
        await _store.AddMessageAsync(design.Id, "user", $"رفع الصفحة «{file.FileName}» كما هي بلا تعديل.", null, ct);
        await _store.AddMessageAsync(design.Id, "assistant", "وُضعت الصفحة كما رُفعت حرفيًا (بلا أي تعديل من الموديل). يمكنك الآن تعديل نصها من مساحة العمل، أو طلب تعديلات بالمحادثة، أو نشرها.", 1, ct);
        var brand = await BrandAsync(selection.ProjectId, ct);
        return Ok(new { design, html = ApplyBrandAssets(design.Html, brand), versions = await _store.VersionsAsync(design.Id, ct), messages = await _store.MessagesAsync(design.Id, 60, ct) });
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

    private async Task<FreeformTemplate?> ResolveTemplateAsync(string? key, string? projectId, CancellationToken ct)
        => string.IsNullOrWhiteSpace(key) ? null : FreeformTemplates.Find(key) ?? await _templates.GetAsync(key, projectId, ct);

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
        var fromLabel = FreeformBuilder.KindLabel(src.Kind); var toLabel = FreeformBuilder.KindLabel(request.Kind);
        var message = $"حوّل الصفحة الحالية (وهي {fromLabel}) إلى {toLabel} كاملة: احتفظ بكل المحتوى والنصوص والأرقام والجداول والهوية البصرية نفسها، وأعد تنظيمه وفق بنية «{toLabel}» الموضحة في التعليمات."
            + (string.IsNullOrWhiteSpace(request.Note) ? "" : $"\nملاحظات المستخدم: {request.Note.Trim()}");
        var context = await _tools.DescribeSourcesAsync(selection, ct);
        var brand = await BrandAsync(selection.ProjectId, ct);
        FreeformResult result;
        try { result = await _builder.GenerateAsync(message, src.Html, Array.Empty<FreeformMessage>(), context, brand, user, ct, request.Kind!); }
        catch (InvalidOperationException ex) { return StatusCode(503, new { error = ex.Message }); }
        if (string.IsNullOrWhiteSpace(result.Html)) return StatusCode(502, new { error = "لم يُنتج الموديل صفحة من التحويل — جرّب ثانية أو أضف ملاحظة." });
        var design = await _store.CreateAsync(selection.ProjectId, user.OrganizationId, user.Id, $"{src.Title} ({toLabel})", result.Html, message, request.Kind, null, ct);
        await _store.AddMessageAsync(design.Id, "user", $"تحويل «{src.Title}» من {fromLabel} إلى {toLabel}", null, ct);
        await _store.AddMessageAsync(design.Id, "assistant", result.Reply, design.Version, ct);
        return Ok(new { design, html = ApplyBrandAssets(design.Html, brand), reply = result.Reply, sourceId = src.Id });
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
        var currentHtml = !string.IsNullOrWhiteSpace(request.Html) ? request.Html : design?.Html;
        var context = await _tools.DescribeSourcesAsync(selection, ct);
        var brand = await BrandAsync(selection.ProjectId, ct);
        FreeformResult result;
        var kind = FreeformStore.NormalizeKind(design?.Kind ?? request.Kind);
        // النموذج: المحفوظ مع التصميم (يُبقي البنية عبر التعديلات) وإلا المختار في الطلب الأول
        var template = await ResolveTemplateAsync(design?.TemplateKey ?? request.TemplateKey, selection.ProjectId, ct);
        if (template is not null && template.Kind != kind) template = null;
        try { result = await _builder.GenerateAsync(request.Message, currentHtml, history, context, brand, user, ct, kind, template); }
        catch (InvalidOperationException ex) { return StatusCode(503, new { error = ex.Message }); }
        int? version = null;
        if (design is null)
        {
            var title = TitleFrom(request.Message);
            design = await _store.CreateAsync(selection.ProjectId, user.OrganizationId, user.Id, title, result.Html ?? "", request.Message, kind, template?.Key, ct);
            version = design.Version > 0 ? design.Version : null;
        }
        else if (result.Html is not null) { version = await _store.AddVersionAsync(design.Id, result.Html, request.Message, null, ct); design.Version = version.Value; design.Html = result.Html; }
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
