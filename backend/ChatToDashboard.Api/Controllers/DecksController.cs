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
    private readonly ProjectStore _projects;
    private readonly DeckBuilder _builder;
    private readonly DeckImporter _importer;
    private readonly PresentationStore _presentations;
    private readonly OrganizationStore _organizations;
    private readonly PermissionsService _permissions;
    private readonly AnalyticsTools _tools;
    private readonly WidgetFeasibilityService _feasibility;
    private readonly IWebHostEnvironment _env;

    public DecksController(DeckStore decks, DeckBrandStore brands, DeckBuilder builder, PresentationStore presentations, OrganizationStore organizations,
        PermissionsService permissions, AnalyticsTools tools, WidgetFeasibilityService feasibility, IWebHostEnvironment env, ProjectStore projects, DeckImporter importer)
    {
        _decks = decks; _brands = brands; _projects = projects; _importer = importer; _builder = builder; _presentations = presentations; _organizations = organizations;
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
        var b = await _brands.GetAsync(cur.Value.Selection.ProjectId, ct);
        return Ok(new { brand = b, fonts = DeckBrand.Fonts, maxLogoBytes = DeckBrand.MaxLogoBytes, canEdit = await CanEditBrandAsync(cur.Value.User, cur.Value.Selection.ProjectId, ct) });
    }

    [HttpPut("brand")]
    public async Task<IActionResult> SetBrand([FromBody] DeckBrand brand, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        if (!await CanEditBrandAsync(cur.Value.User, cur.Value.Selection.ProjectId, ct)) return StatusCode(403, new { error = "تعديل هوية المشروع للمسؤول أو مالك المشروع فقط." });
        if (brand.Validate() is { } err) return BadRequest(new { error = err });
        if (brand.IsEmpty) await _brands.DeleteAsync(cur.Value.Selection.ProjectId, ct);
        else await _brands.SetAsync(cur.Value.Selection.ProjectId, cur.Value.User.OrganizationId, cur.Value.User.Id, brand, ct);
        return Ok(new { brand = await _brands.GetAsync(cur.Value.Selection.ProjectId, ct) });
    }

    [HttpDelete("brand")]
    public async Task<IActionResult> DeleteBrand(CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        if (!await CanEditBrandAsync(cur.Value.User, cur.Value.Selection.ProjectId, ct)) return StatusCode(403, new { error = "تعديل هوية المشروع للمسؤول أو مالك المشروع فقط." });
        await _brands.DeleteAsync(cur.Value.Selection.ProjectId, ct);
        return NoContent();
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
        return Ok(new { deck = d, spec = JsonSerializer.Deserialize<DeckSpec>(d.SpecJson, DeckSpec.Json), publishedUrl = await PublishedUrlAsync(d, ct) });
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
        return Ok(new { deck, spec = result.Spec, reply = result.Reply, dataSlidesBuilt = result.DataSlidesBuilt, dataSlidesTotal = result.DataSlidesTotal });
    }

    /// <summary>استيراد ملف PowerPoint/PDF/HTML موجود كعرض قابل للتعديل: استخراج حتمي ← شرائح ← (اختياريًا) تحسين الترتيب بالموديل.</summary>
    [HttpPost("import")]
    [RequestSizeLimit(DeckImporter.MaxBytes + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = DeckImporter.MaxBytes + 1024 * 1024)]
    public async Task<IActionResult> Import([FromForm] IFormFile? file, [FromForm] bool refine, [FromForm] string? title, CancellationToken ct)
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
        if (doc.Pages.Count == 0) return BadRequest(new { error = "لم يُعثر على محتوى نصي في الملف.", warnings = doc.Warnings });
        var spec = DeckImporter.ToSpec(doc);
        if (!string.IsNullOrWhiteSpace(title)) spec.Title = title.Trim();
        var warnings = new List<string>(doc.Warnings);
        if (refine)
        {
            var context = await _tools.DescribeSourcesAsync(selection, ct);
            var (refined, note) = await _importer.RefineAsync(spec, context, user, ct);
            spec = refined; if (note is not null) warnings.Add(note);
        }
        var deck = await _decks.CreateAsync(selection.ProjectId, user.OrganizationId, user.Id, spec, null, ct);
        return Ok(new { deck, spec, kind = doc.Kind, pagesRead = doc.Pages.Count, warnings });
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
        spec.Brand = await _brands.GetAsync(selection.ProjectId, ct);
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
