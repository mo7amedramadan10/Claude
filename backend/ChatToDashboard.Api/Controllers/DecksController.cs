using ChatToDashboard.Api.Data;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;
using ChatToDashboard.Api.Projects;
using System.Text;
using System.Text.Json;
using ChatToDashboard.Api.Decks;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Organizations;
using ChatToDashboard.Api.Presentations;
using ChatToDashboard.Api.Semantics;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

public class CreateDeckRequest { public string? TemplateKey { get; set; } public string? Title { get; set; } }
public class UpdateDeckRequest { public DeckSpec? Spec { get; set; } }
public class GenerateDeckRequest
{
    public string Message { get; set; } = "";
    /// <summary>null = إنشاء عرض جديد من الرسالة (وبالنموذج إن أُعطي)؛ وإلا تعديل العرض المحفوظ.</summary>
    public string? DeckId { get; set; }
    public string? TemplateKey { get; set; }
    /// <summary>المواصفة الحالية من الواجهة (قد تحمل تعديلات يدوية غير محفوظة بعد).</summary>
    public DeckSpec? Spec { get; set; }
}
public class PublishDeckRequest { public string? Slug { get; set; } public string? Title { get; set; } }

/// <summary>
/// «منشئ العروض» (شاشة على مستوى المشروع تحت «المحلل الذكي»): عروض JSON تُبنى بالمحادثة أو من نموذج أو يدويًا،
/// وتُنشر كصفحة مستقلة في «العروض التقديمية» (مستوى المنظمة، بلا تغيير فيها) على الرابط المقروء.
/// </summary>
[ApiController]
[Route("api/decks")]
public class DecksController : ControllerBase
{
    private readonly DeckStore _decks;
    private readonly DeckBrandStore _brands;
    private readonly DeckBrandFontStore _fonts;
    private readonly DeckBrandTemplateStore _templates;
    private readonly ProjectStore _projects;
    private readonly DeckBuilder _builder;
    private readonly DeckImporter _importer;
    private readonly DataFingerprintService _fingerprints;
    private readonly PresentationStore _presentations;
    private readonly OrganizationStore _organizations;
    private readonly PermissionsService _permissions;
    private readonly AnalyticsTools _tools;
    private readonly WidgetFeasibilityService _feasibility;
    private readonly IWebHostEnvironment _env;

    public DecksController(DeckStore decks, DeckBrandStore brands, DeckBuilder builder, PresentationStore presentations, OrganizationStore organizations,
        PermissionsService permissions, AnalyticsTools tools, WidgetFeasibilityService feasibility, IWebHostEnvironment env, ProjectStore projects, DeckImporter importer, DeckBrandFontStore fonts, DeckBrandTemplateStore templates, DataFingerprintService fingerprints)
    {
        _decks = decks; _brands = brands; _projects = projects; _importer = importer; _fingerprints = fingerprints; _fonts = fonts; _templates = templates; _builder = builder; _presentations = presentations; _organizations = organizations;
        _permissions = permissions; _tools = tools; _feasibility = feasibility; _env = env;
    }

    private async Task<(AppUser User, Sources.SourceSelection Selection)?> CurrentAsync(CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return null;
        return (user, await _permissions.GetEffectiveSelectionAsync(user, null, ct));
    }

    // ---- هوية المشروع (تُعمَّم على كل النماذج والمعاينات والنشر والتصدير) ----
    [HttpGet("brand")]
    public async Task<IActionResult> GetBrand(CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var b = await BrandWithFontsAsync(cur.Value.Selection.ProjectId, ct);
        return Ok(new { brand = b, fonts = DeckBrand.Fonts, uploadedFonts = b?.FontFaces ?? await _fonts.ListAsync(cur.Value.Selection.ProjectId, ct), maxLogoBytes = DeckBrand.MaxLogoBytes, maxFontBytes = DeckBrandFontStore.MaxBytes, canEdit = await CanEditBrandAsync(cur.Value.User, cur.Value.Selection.ProjectId, ct) });
    }

    [HttpPut("brand")]
    public async Task<IActionResult> SetBrand([FromBody] DeckBrand brand, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        if (!await CanEditBrandAsync(cur.Value.User, cur.Value.Selection.ProjectId, ct)) return StatusCode(403, new { error = "تعديل هوية المشروع للمسؤول أو مالك المشروع فقط." });
        var uploaded = await _fonts.ListAsync(cur.Value.Selection.ProjectId, ct);
        if (brand.Validate(uploaded.Select(f => f.Family)) is { } err) return BadRequest(new { error = err });
        if (brand.IsEmpty) await _brands.DeleteAsync(cur.Value.Selection.ProjectId, ct);
        else await _brands.SetAsync(cur.Value.Selection.ProjectId, cur.Value.User.OrganizationId, cur.Value.User.Id, brand, ct);
        return Ok(new { brand = await BrandWithFontsAsync(cur.Value.Selection.ProjectId, ct) });
    }

    [HttpDelete("brand")]
    public async Task<IActionResult> DeleteBrand(CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        if (!await CanEditBrandAsync(cur.Value.User, cur.Value.Selection.ProjectId, ct)) return StatusCode(403, new { error = "تعديل هوية المشروع للمسؤول أو مالك المشروع فقط." });
        await _brands.DeleteAsync(cur.Value.Selection.ProjectId, ct);
        return NoContent();
    }

    /// <summary>الهوية مع خطوطها المرفوعة (@font-face) — null بلا هوية؛ إن وُجدت خطوط بلا هوية تُعاد هوية فارغة تحملها.</summary>
    private async Task<DeckBrand?> BrandWithFontsAsync(string? projectId, CancellationToken ct)
    {
        var b = await _brands.GetAsync(projectId, ct);
        var fonts = await _fonts.ListAsync(projectId, ct);
        if (b is null && fonts.Count == 0) return null;
        b ??= new DeckBrand();
        b.FontFaces = fonts;
        return b;
    }

    // ---- خطوط الشركة (ttf/otf/woff/woff2) ----
    [HttpPost("brand/fonts")]
    [RequestSizeLimit(DeckBrandFontStore.MaxBytes * 4 + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = DeckBrandFontStore.MaxBytes * 4 + 1024 * 1024)]
    public async Task<IActionResult> UploadFonts([FromForm] List<IFormFile> files, [FromForm] string? family, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        if (!await CanEditBrandAsync(cur.Value.User, cur.Value.Selection.ProjectId, ct)) return StatusCode(403, new { error = "تعديل هوية المشروع للمسؤول أو مالك المشروع فقط." });
        if (files is null || files.Count == 0) return BadRequest(new { error = "اختر ملف خط واحدًا على الأقل (.ttf أو .otf أو .woff أو .woff2)." });
        var existing = await _fonts.ListAsync(cur.Value.Selection.ProjectId, ct);
        var added = new List<DeckBrandFont>(); var errors = new List<string>();
        foreach (var file in files)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!DeckBrandFont.Formats.ContainsKey(ext)) { errors.Add($"{file.FileName}: الصيغة غير مدعومة."); continue; }
            if (file.Length == 0 || file.Length > DeckBrandFontStore.MaxBytes) { errors.Add($"{file.FileName}: الحجم يتجاوز {DeckBrandFontStore.MaxBytes / 1024 / 1024} ميغابايت."); continue; }
            if (existing.Count + added.Count >= DeckBrandFontStore.MaxPerProject) { errors.Add($"{file.FileName}: الحد {DeckBrandFontStore.MaxPerProject} ملفًا للمشروع."); continue; }
            byte[] bytes; using (var ms = new MemoryStream()) { await file.CopyToAsync(ms, ct); bytes = ms.ToArray(); }
            var names = DeckBrandFontStore.ReadSfntNames(bytes);
            if (ext is ".ttf" or ".otf" && names is null) { errors.Add($"{file.FileName}: ليس ملف خط صالحًا."); continue; }
            var fam = !string.IsNullOrWhiteSpace(family) ? family.Trim() : names?.Family ?? Regex.Replace(Path.GetFileNameWithoutExtension(file.FileName), @"[-_](regular|bold|light|medium|italic|semibold|black|thin)+$", "", RegexOptions.IgnoreCase).Replace('-', ' ').Replace('_', ' ');
            if (fam.Length > 120) fam = fam[..120];
            var (weight, style) = DeckBrandFontStore.WeightStyleOf(names?.Subfamily ?? Path.GetFileNameWithoutExtension(file.FileName));
            added.Add(await _fonts.AddAsync(cur.Value.Selection.ProjectId, cur.Value.User.Id, file.FileName, bytes, fam, weight, style, ct));
        }
        if (added.Count == 0) return BadRequest(new { error = string.Join(" ", errors) });
        return Ok(new { added, errors, fonts = await _fonts.ListAsync(cur.Value.Selection.ProjectId, ct), brand = await BrandWithFontsAsync(cur.Value.Selection.ProjectId, ct) });
    }

    [HttpDelete("brand/fonts/{id}")]
    public async Task<IActionResult> DeleteFont(string id, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        if (!await CanEditBrandAsync(cur.Value.User, cur.Value.Selection.ProjectId, ct)) return StatusCode(403, new { error = "تعديل هوية المشروع للمسؤول أو مالك المشروع فقط." });
        if (!await _fonts.DeleteAsync(cur.Value.Selection.ProjectId, id, ct)) return NotFound(new { error = "ملف الخط غير موجود." });
        // لو كانت الهوية تختار عائلة لم يعد لها ملفات تُعاد إلى الافتراضي
        var b = await _brands.GetAsync(cur.Value.Selection.ProjectId, ct);
        var remaining = await _fonts.ListAsync(cur.Value.Selection.ProjectId, ct);
        if (b?.Font is { } f && !DeckBrand.Fonts.Contains(f) && !remaining.Any(x => string.Equals(x.Family, f, StringComparison.OrdinalIgnoreCase)))
        { b.Font = null; b.Validate(); if (b.IsEmpty) await _brands.DeleteAsync(cur.Value.Selection.ProjectId, ct); else await _brands.SetAsync(cur.Value.Selection.ProjectId, cur.Value.User.OrganizationId, cur.Value.User.Id, b, ct); }
        return Ok(new { fonts = remaining, brand = await BrandWithFontsAsync(cur.Value.Selection.ProjectId, ct) });
    }

    /// <summary>ملف الخط للمتصفح والصفحة المنشورة (داخل iframe بأصل معتم بلا كوكي) — بلا دخول؛ المعرّف عشوائي والخط ليس سرًّا.</summary>
    [HttpGet("brand/fonts/{id}/file")]
    [AllowAnonymous]
    [ResponseCache(Duration = 86400 * 30, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> FontFile(string id, CancellationToken ct)
    {
        if (!Regex.IsMatch(id, "^[0-9a-f]{16}$")) return NotFound();
        var f = await _fonts.GetFileAsync(id, ct);
        if (f is null) return NotFound();
        Response.Headers["Access-Control-Allow-Origin"] = "*";
        return File(f.Value.Bytes, f.Value.ContentType, f.Value.FileName);
    }

    // ---- قالب الشركة (PowerPoint) لكل مشروع ----
    [HttpGet("brand/template")]
    public async Task<IActionResult> GetTemplate([FromQuery] bool all, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var info = await _templates.GetInfoAsync(cur.Value.Selection.ProjectId, ct);
        if (info is not null && !all) info.Layouts = info.Layouts.Where(l => !l.Generic).ToList();
        return Ok(new { template = info, maxBytes = DeckBrandTemplateStore.MaxBytes, canEdit = await CanEditBrandAsync(cur.Value.User, cur.Value.Selection.ProjectId, ct) });
    }

    [HttpPost("brand/template")]
    [RequestSizeLimit(DeckBrandTemplateStore.MaxBytes + 2 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = DeckBrandTemplateStore.MaxBytes + 2 * 1024 * 1024)]
    public async Task<IActionResult> UploadTemplate([FromForm] IFormFile? file, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        if (!await CanEditBrandAsync(cur.Value.User, cur.Value.Selection.ProjectId, ct)) return StatusCode(403, new { error = "رفع قالب الشركة للمسؤول أو مالك المشروع فقط." });
        if (file is null || file.Length == 0) return BadRequest(new { error = "اختر ملف قالب PowerPoint (.pptx أو .potx)." });
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".pptx" or ".potx")) return BadRequest(new { error = "الصيغة غير مدعومة — المدعوم .pptx و.potx." });
        if (file.Length > DeckBrandTemplateStore.MaxBytes) return BadRequest(new { error = $"حجم القالب يتجاوز {DeckBrandTemplateStore.MaxBytes / 1024 / 1024} ميغابايت." });
        byte[] bytes; using (var ms = new MemoryStream()) { await file.CopyToAsync(ms, ct); bytes = ms.ToArray(); }
        TemplateInfo info;
        try { info = await _templates.SaveAsync(cur.Value.Selection.ProjectId, cur.Value.User.Id, file.FileName, bytes, ct); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { return BadRequest(new { error = "تعذّرت قراءة القالب: " + ex.Message }); }
        info.Layouts = info.Layouts.Where(l => !l.Generic).ToList();
        return Ok(new { template = info });
    }

    [HttpPut("brand/template/defaults")]
    public async Task<IActionResult> SetTemplateDefaults([FromBody] TemplateDefaults defaults, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        if (!await CanEditBrandAsync(cur.Value.User, cur.Value.Selection.ProjectId, ct)) return StatusCode(403, new { error = "تعديل قالب الشركة للمسؤول أو مالك المشروع فقط." });
        var info = await _templates.SetDefaultsAsync(cur.Value.Selection.ProjectId, defaults, ct);
        if (info is null) return NotFound(new { error = "لا قالب مرفوعًا." });
        info.Layouts = info.Layouts.Where(l => !l.Generic).ToList();
        return Ok(new { template = info });
    }

    [HttpDelete("brand/template")]
    public async Task<IActionResult> DeleteTemplate(CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        if (!await CanEditBrandAsync(cur.Value.User, cur.Value.Selection.ProjectId, ct)) return StatusCode(403, new { error = "حذف قالب الشركة للمسؤول أو مالك المشروع فقط." });
        await _templates.DeleteAsync(cur.Value.Selection.ProjectId, ct);
        return NoContent();
    }

    /// <summary>صفحة مرسومة SVG — بلا دخول (الصفحة المنشورة لاحقًا داخل iframe بلا كوكي؛ المعرّف عشوائي).</summary>
    [HttpGet("brand/template/{tid}/pages/{kind}/{idx:int}.svg")]
    [AllowAnonymous]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> TemplatePage(string tid, string kind, int idx, CancellationToken ct)
    {
        if (!Regex.IsMatch(tid, "^[0-9a-f]{16}$") || kind is not ("layout" or "slide" or "bg")) return NotFound();
        var svg = await _templates.GetPageSvgAsync(tid, kind, idx, ct);
        if (svg is null) return NotFound();
        return Content(svg, "image/svg+xml; charset=utf-8");
    }

    [HttpGet("brand/template/{tid}/media/{name}")]
    [AllowAnonymous]
    [ResponseCache(Duration = 86400 * 30, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> TemplateMedia(string tid, string name, CancellationToken ct)
    {
        if (!Regex.IsMatch(tid, "^[0-9a-f]{16}$")) return NotFound();
        var bytes = await _templates.GetBytesAsync(tid, ct); if (bytes is null) return NotFound();
        var m = DeckBrandTemplateStore.Media(bytes, name); if (m is null) return NotFound();
        return File(m.Value.Bytes, m.Value.ContentType);
    }

    /// <summary>المسؤول أو من له دور مالك/محرّر في المشروع الحالي (المشاهد لا يغيّر الهوية). الحساب بلا دور صريح يُعامل كمشاهد.</summary>
    private async Task<bool> CanEditBrandAsync(AppUser user, string? projectId, CancellationToken ct)
    {
        if (user.Role == UserRoles.Admin || user.IsPlatformOwner) return true;
        if (projectId is null) return false;
        var role = (await _projects.ListRolesAsync(projectId, ct)).FirstOrDefault(r => string.Equals(r.UserId, user.Id, StringComparison.Ordinal))?.Role;
        return role is "owner" or "editor";
    }

    /// <summary>نماذج العروض مع جدواها لبيانات المشروع الحالي (بنفس قواعد عناوين عناصر اللوحات).</summary>
    [HttpGet("templates")]
    public async Task<IActionResult> Templates(CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var available = (await _feasibility.AvailableConceptsAsync(cur.Value.Selection.ProjectId, ct)).Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var analyzed = available.Count > 0;
        return Ok(DeckTemplates.All.Select(t =>
        {
            var dataSlides = t.Slides.Where(s => s.NeedsData).ToList();
            int? buildable = null; var missing = new List<string>();
            if (analyzed && dataSlides.Count > 0)
            {
                buildable = 0;
                foreach (var s in dataSlides)
                {
                    var f = WidgetRequirementResolver.Evaluate(WidgetRequirementResolver.Resolve(s.Title, s.Intent), available);
                    if (f.Feasible) buildable = buildable + 1; else missing.AddRange(f.Missing);
                }
            }
            return new
            {
                key = t.Key, name = t.Name, category = t.Category, description = t.Description, icon = t.Icon,
                slides = t.Slides.Select(s => new { s.Layout, s.Title, s.NeedsData, s.Hint }), total = t.Slides.Count, dataSlides = dataSlides.Count, buildable,
                topMissing = missing.GroupBy(m => m).OrderByDescending(g => g.Count()).Take(3).Select(g => g.Key),
            };
        }));
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        return Ok(await _decks.ListAsync(cur.Value.Selection.ProjectId, ct));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var d = await _decks.GetAsync(id, cur.Value.Selection.ProjectId, ct);
        if (d is null) return NotFound(new { error = "العرض غير موجود." });
        var spec0 = JsonSerializer.Deserialize<DeckSpec>(d.SpecJson, DeckSpec.Json);
        bool? dataChanged = null;
        if (d.DataFingerprint is not null && spec0 is not null)
        {
            var fp = await _fingerprints.ComputeAsync(spec0.Slides.Select(x => x.Query?.Sql).Where(x => !string.IsNullOrWhiteSpace(x))!, cur.Value.Selection.ProjectId, ct);
            dataChanged = fp is not null && fp != d.DataFingerprint;
        }
        return Ok(new { deck = d, spec = spec0, publishedUrl = await PublishedUrlAsync(d, ct), dataChanged });
    }

    /// <summary>عرض جديد من نموذج (هيكل بلا بيانات) أو فارغ.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDeckRequest request, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var template = DeckTemplates.Find(request.TemplateKey) ?? DeckTemplates.Find("blank")!;
        var spec = DeckTemplates.ToSpec(template, request.Title);
        var d = await _decks.CreateAsync(cur.Value.Selection.ProjectId, cur.Value.User.OrganizationId, cur.Value.User.Id, spec, template.Key, ct);
        return Ok(new { deck = d, spec });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateDeckRequest request, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var d = await _decks.GetAsync(id, cur.Value.Selection.ProjectId, ct);
        if (d is null) return NotFound(new { error = "العرض غير موجود." });
        if (request.Spec is null) return BadRequest(new { error = "المواصفة مطلوبة." });
        var spec = request.Spec.Normalize();
        await _decks.UpdateSpecAsync(id, spec, ct);
        return Ok(new { spec });
    }

    public class DeckAutoRefreshRequest { public bool On { get; set; } }
    [HttpPut("{id}/auto-refresh")]
    public async Task<IActionResult> SetAutoRefresh(string id, [FromBody] DeckAutoRefreshRequest request, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var d = await _decks.GetAsync(id, cur.Value.Selection.ProjectId, ct); if (d is null) return NotFound(new { error = "العرض غير موجود." });
        await _decks.SetAutoRefreshAsync(id, request.On, ct); return Ok(new { autoRefresh = request.On });
    }

    /// <summary>تحديث بيانات شرائح العرض بلا موديل: إعادة تنفيذ <c>query.sql</c> لكل شريحة وإعادة صياغتها بقواعد ثابتة (DeckDataRefresher).</summary>
    [HttpPost("{id}/refresh")]
    public async Task<IActionResult> RefreshData(string id, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var (user, selection) = cur.Value;
        var d = await _decks.GetAsync(id, selection.ProjectId, ct); if (d is null) return NotFound(new { error = "العرض غير موجود." });
        var spec = JsonSerializer.Deserialize<DeckSpec>(d.SpecJson, DeckSpec.Json)!.Normalize(); var withSql = spec.Slides.Where(s => !string.IsNullOrWhiteSpace(s.Query?.Sql)).ToList();
        if (withSql.Count == 0) return BadRequest(new { error = "لا شرائح بيانات باستعلام محفوظ في هذا العرض.", noQueries = true });
        var context = await _tools.DescribeSourcesAsync(selection, ct);
        var updated = 0; var failed = new List<object>();
        foreach (var slide in withSql)
        {
            var (res, isError) = await _tools.RunReadOnlyQueryAsync(slide.Query!.Sql!, context, ct);
            if (isError) { failed.Add(new { slide = slide.Title, error = res }); continue; }
            System.Text.Json.Nodes.JsonArray? rows = null; try { rows = System.Text.Json.Nodes.JsonNode.Parse(res)?["rows"] as System.Text.Json.Nodes.JsonArray; } catch (System.Text.Json.JsonException) { }
            if (rows is null || !DeckDataRefresher.Apply(slide, rows)) { failed.Add(new { slide = slide.Title, error = "نتيجة فارغة أو لا تناسب تخطيط الشريحة" }); continue; }
            slide.MissingReason = null; updated++;
        }
        var refreshedAt = DateTime.UtcNow;
        if (updated > 0) { await _decks.UpdateSpecAsync(id, spec.Normalize(), ct); await _decks.SetDataStateAsync(id, await _fingerprints.ComputeAsync(withSql.Select(x => x.Query!.Sql!), selection.ProjectId, ct), refreshedAt, ct); }
        return Ok(new { spec, updated, total = withSql.Count, failed, refreshedAt });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var d = await _decks.GetAsync(id, cur.Value.Selection.ProjectId, ct);
        if (d is null) return NotFound(new { error = "العرض غير موجود." });
        await _decks.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>توليد أو تعديل بالمحادثة: يعيد المواصفة الجديدة (محفوظة في العرض) وردًّا نصيًا.</summary>
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateDeckRequest request, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        if (string.IsNullOrWhiteSpace(request.Message)) return BadRequest(new { error = "اكتب ما تريد في العرض." });
        var (user, selection) = cur.Value;
        DeckRecord? deck = null; DeckSpec? current = null;
        if (!string.IsNullOrWhiteSpace(request.DeckId))
        {
            deck = await _decks.GetAsync(request.DeckId, selection.ProjectId, ct);
            if (deck is null) return NotFound(new { error = "العرض غير موجود." });
            current = request.Spec?.Normalize() ?? JsonSerializer.Deserialize<DeckSpec>(deck.SpecJson, DeckSpec.Json);
        }
        var context = await _tools.DescribeSourcesAsync(selection, ct);
        var template = DeckTemplates.Find(request.TemplateKey ?? deck?.TemplateKey);
        DeckBuildResult result;
        try { result = await _builder.BuildAsync(new DeckBuildRequest(request.Message.Trim(), current, deck is null ? template : null, context, user), ct); }
        catch (InvalidOperationException ex) { return StatusCode(503, new { error = ex.Message }); }
        if (deck is null) deck = await _decks.CreateAsync(selection.ProjectId, user.OrganizationId, user.Id, result.Spec, template?.Key, ct);
        else await _decks.UpdateSpecAsync(deck.Id, result.Spec, ct);
        var sqls = result.Spec.Slides.Select(x => x.Query?.Sql).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!).ToList();
        if (sqls.Count > 0) { var at = DateTime.UtcNow; var fp = await _fingerprints.ComputeAsync(sqls, selection.ProjectId, ct); await _decks.SetDataStateAsync(deck.Id, fp, at, ct); deck.DataFingerprint = fp; deck.DataRefreshedAt = at; }
        return Ok(new { deck, spec = result.Spec, reply = result.Reply, dataSlidesBuilt = result.DataSlidesBuilt, dataSlidesTotal = result.DataSlidesTotal });
    }

    /// <summary>استيراد ملف PowerPoint/PDF/HTML موجود كعرض قابل للتعديل: استخراج حتمي ← شرائح ← (اختياريًا) تحسين الترتيب بالموديل.</summary>
    [HttpPost("import")]
    [RequestSizeLimit(DeckImporter.MaxBytes + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = DeckImporter.MaxBytes + 1024 * 1024)]
    public async Task<IActionResult> Import([FromForm] IFormFile? file, [FromForm] bool refine, [FromForm] string? mode, [FromForm] string? title, CancellationToken ct)
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
        mode = (mode ?? (refine ? "refine" : "none")).Trim().ToLowerInvariant();
        if (doc.Pages.Count == 0 && !(mode == "understand" && doc.DataBlocks.Count > 0)) return BadRequest(new { error = "لم يُعثر على محتوى نصي في الملف.", warnings = doc.Warnings });
        var spec = DeckImporter.ToSpec(doc);
        if (!string.IsNullOrWhiteSpace(title)) spec.Title = title.Trim();
        var warnings = new List<string>(doc.Warnings);
        if (mode is "refine" or "understand")
        {
            var context = await _tools.DescribeSourcesAsync(selection, ct);
            var (result, note) = mode == "understand" ? await _importer.UnderstandAsync(doc, spec, context, user, ct) : await _importer.RefineAsync(spec, context, user, ct);
            spec = result; if (note is not null) warnings.Add(note);
            if (!string.IsNullOrWhiteSpace(title)) spec.Title = title.Trim();
        }
        var deck = await _decks.CreateAsync(selection.ProjectId, user.OrganizationId, user.Id, spec, null, ct);
        return Ok(new { deck, spec, kind = doc.Kind, pagesRead = doc.Pages.Count, dataBlocks = doc.DataBlocks.Count, mode, warnings });
    }

    /// <summary>ينشر العرض كصفحة HTML مستقلة في «العروض التقديمية» (أو يحدّث المنشور سابقًا على نفس الرابط).</summary>
    [HttpPost("{id}/publish")]
    public async Task<IActionResult> Publish(string id, [FromBody] PublishDeckRequest request, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var (user, selection) = cur.Value;
        var d = await _decks.GetAsync(id, selection.ProjectId, ct);
        if (d is null) return NotFound(new { error = "العرض غير موجود." });
        var spec = JsonSerializer.Deserialize<DeckSpec>(d.SpecJson, DeckSpec.Json)!.Normalize();
        spec.Brand = await BrandWithFontsAsync(selection.ProjectId, ct);
        if (spec.TemplateMode != "off" && await _templates.GetInfoAsync(selection.ProjectId, ct) is { } tinfo) { tinfo.Layouts = tinfo.Layouts.Where(l => !l.Generic).ToList(); spec.Template = tinfo; }
        var runtimePath = Path.Combine(_env.WebRootPath, "js", "jeem-deck.js");
        if (!System.IO.File.Exists(runtimePath)) return StatusCode(500, new { error = "محرك العرض غير موجود على الخادم (js/jeem-deck.js)." });
        var runtime = await System.IO.File.ReadAllTextAsync(runtimePath, ct);
        var html = DeckHtmlExporter.Build(spec, runtime, includeBridge: spec.Slides.Any(s => s.Editable is not null));
        var title = string.IsNullOrWhiteSpace(request.Title) ? spec.Title : request.Title.Trim();
        var orgSlug = user.OrganizationId is null ? null : (await _organizations.FindByIdAsync(user.OrganizationId, ct))?.Slug;

        Presentation? existing = d.PresentationId is null ? null : await _presentations.GetAsync(d.PresentationId, ct);
        if (existing is not null && string.Equals(existing.OrganizationId, user.OrganizationId, StringComparison.Ordinal))
        {
            await _presentations.ReplaceAsync(existing.Id, html, Slugify(title) + ".html", title, ct);
            return Ok(new { presentationId = existing.Id, url = existing.PublicPath(orgSlug), updated = true });
        }
        var slug = PresentationSlug.Normalize(string.IsNullOrWhiteSpace(request.Slug) ? title : request.Slug);
        if (slug.Length < 2) slug = "deck";
        var baseSlug = slug; var n = 1;
        while (await _presentations.SlugTakenAsync(user.OrganizationId, slug, null, ct)) slug = $"{baseSlug}-{++n}";
        var p = new Presentation
        {
            OrganizationId = user.OrganizationId, CreatedByUserId = user.Id, CreatedByName = user.DisplayName ?? user.Username,
            Title = title, Slug = slug, FileName = Slugify(title) + ".html", Html = html, SizeBytes = Encoding.UTF8.GetByteCount(html),
        };
        await _presentations.SaveAsync(p, ct);
        await _decks.SetPresentationAsync(d.Id, p.Id, ct);
        return Ok(new { presentationId = p.Id, url = p.PublicPath(orgSlug), updated = false });
    }

    /// <summary>تصدير PPTX شريحةً بشريحة (الرسوم تصل كصور PNG من المتصفح).</summary>
    [HttpPost("export/pptx")]
    public IActionResult ExportPptx([FromBody] DeckPptxRequest request)
    {
        if (request.Slides.Count == 0) return BadRequest(new { error = "لا توجد شرائح." });
        var bytes = DeckPptxBuilder.Build(request);
        return File(bytes, "application/vnd.openxmlformats-officedocument.presentationml.presentation", (string.IsNullOrWhiteSpace(request.Title) ? "deck" : Slugify(request.Title)) + ".pptx");
    }

    private async Task<string?> PublishedUrlAsync(DeckRecord d, CancellationToken ct)
    {
        if (d.PresentationId is null) return null;
        var p = await _presentations.GetAsync(d.PresentationId, ct);
        if (p is null) return null;
        var orgSlug = p.OrganizationId is null ? null : (await _organizations.FindByIdAsync(p.OrganizationId, ct))?.Slug;
        return p.PublicPath(orgSlug);
    }

    private static string Slugify(string s) { var x = PresentationSlug.Normalize(s); return x.Length == 0 ? "deck" : x; }
}
