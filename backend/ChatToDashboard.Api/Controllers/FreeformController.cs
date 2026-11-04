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

public class FreeformGenerateRequest { public string Message { get; set; } = ""; public string? DesignId { get; set; } public string? Html { get; set; } public string? Kind { get; set; } }
public class FreeformSaveRequest { public string? Html { get; set; } public string? Title { get; set; } public string? Note { get; set; } }
public class FreeformPublishRequest { public string? Slug { get; set; } public string? Title { get; set; } }

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

    public FreeformController(FreeformStore store, FreeformBuilder builder, DeckBrandStore brands, DeckBrandFontStore fonts, PresentationStore presentations, OrganizationStore organizations, PermissionsService permissions, AnalyticsTools tools)
    { _store = store; _builder = builder; _brands = brands; _fonts = fonts; _presentations = presentations; _organizations = organizations; _permissions = permissions; _tools = tools; }

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
        return Ok(new { design = d, html = ApplyBrandAssets(d.Html, brand), versions = await _store.VersionsAsync(id, ct), messages = await _store.MessagesAsync(id, 60, ct), publishedUrl = await PublishedUrlAsync(d, ct) });
    }

    [HttpGet("{id}/versions/{version:int}")]
    public async Task<IActionResult> GetVersion(string id, int version, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var d = await _store.GetAsync(id, cur.Value.Selection.ProjectId, ct); if (d is null) return NotFound();
        var html = await _store.VersionHtmlAsync(id, version, ct); if (html is null) return NotFound(new { error = "الإصدار غير موجود." });
        return Ok(new { version, html = ApplyBrandAssets(html, await BrandAsync(cur.Value.Selection.ProjectId, ct)) });
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
        try { result = await _builder.GenerateAsync(request.Message, currentHtml, history, context, brand, user, ct, kind); }
        catch (InvalidOperationException ex) { return StatusCode(503, new { error = ex.Message }); }
        int? version = null;
        if (design is null)
        {
            var title = TitleFrom(request.Message);
            design = await _store.CreateAsync(selection.ProjectId, user.OrganizationId, user.Id, title, result.Html ?? "", request.Message, kind, ct);
            version = design.Version > 0 ? design.Version : null;
        }
        else if (result.Html is not null) { version = await _store.AddVersionAsync(design.Id, result.Html, request.Message, null, ct); design.Version = version.Value; design.Html = result.Html; }
        await _store.AddMessageAsync(design.Id, "user", request.Message, null, ct);
        await _store.AddMessageAsync(design.Id, "assistant", result.Reply, version, ct);
        return Ok(new { design, html = ApplyBrandAssets(design.Html, brand), reply = result.Reply, version, usedData = result.UsedData, versions = await _store.VersionsAsync(design.Id, ct) });
    }

    /// <summary>حفظ تعديل يدوي للكود (إصدار جديد) و/أو تغيير العنوان.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Save(string id, [FromBody] FreeformSaveRequest request, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var d = await _store.GetAsync(id, cur.Value.Selection.ProjectId, ct); if (d is null) return NotFound(new { error = "التصميم غير موجود." });
        int? version = null;
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
        var title = string.IsNullOrWhiteSpace(request.Title) ? d.Title : request.Title.Trim();
        var orgSlug = user.OrganizationId is null ? null : (await _organizations.FindByIdAsync(user.OrganizationId, ct))?.Slug;
        Presentation? existing = d.PresentationId is null ? null : await _presentations.GetAsync(d.PresentationId, ct);
        if (existing is not null && string.Equals(existing.OrganizationId, user.OrganizationId, StringComparison.Ordinal))
        {
            await _presentations.ReplaceAsync(existing.Id, html, Slugify(title) + ".html", title, ct);
            return Ok(new { presentationId = existing.Id, url = existing.PublicPath(orgSlug), updated = true });
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
        await _store.SetPresentationAsync(d.Id, p.Id, ct);
        return Ok(new { presentationId = p.Id, url = p.PublicPath(orgSlug), updated = false });
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
