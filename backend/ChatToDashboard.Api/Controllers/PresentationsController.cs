using System.Text;
using ChatToDashboard.Api.Presentations;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>
/// العروض التقديمية (صفحات HTML مرفوعة) على مستوى المنظمة — انظر <see cref="Presentation"/>.
/// المسار العام <c>/p/{id}</c> يعرض صفحة مشاهدة (wwwroot/presentation.html) تضع المحتوى داخل iframe
/// بسمة <c>sandbox</c> بلا <c>allow-same-origin</c>: فالصفحة المرفوعة تعمل بأصل معتم لا يقرأ كوكي
/// النظام ولا ينادي واجهاته بصلاحيات المشاهد. مسار المحتوى نفسه يرسل ترويسة CSP sandbox احتياطًا
/// لمن يفتحه مباشرة.
/// </summary>
[ApiController]
[Route("api/presentations")]
public class PresentationsController : ControllerBase
{
    /// <summary>حد حجم الصفحة: 15 ميغابايت تكفي لعرض فيه صور مضمّنة (data URI).</summary>
    public const long MaxBytes = 15L * 1024 * 1024;
    private const string SandboxCsp = "sandbox allow-scripts allow-popups allow-forms allow-modals allow-downloads";

    private readonly PresentationStore _store;
    private readonly PermissionsService _permissions;

    public PresentationsController(PresentationStore store, PermissionsService permissions)
    {
        _store = store;
        _permissions = permissions;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var list = await _store.ListAsync(user.OrganizationId, ct);
        return Ok(list.Select(p => new { p.Id, p.Title, p.FileName, p.SizeBytes, p.CreatedByUserId, p.CreatedByName, p.CreatedAt, p.UpdatedAt, p.ExpiresAt, p.RevokedAt, p.ViewCount, p.IsActive, canManage = CanManage(user, p) }));
    }

    /// <summary>رفع عرض جديد: ملف .html/.htm واحد + عنوان + تاريخ انتهاء اختياري.</summary>
    [HttpPost]
    [RequestSizeLimit(MaxBytes + 1024 * 1024)]
    public async Task<IActionResult> Create([FromForm] IFormFile? file, [FromForm] string? title, [FromForm] DateTime? expiresAt, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var (html, error) = await ReadHtmlAsync(file, ct);
        if (error is not null) return BadRequest(new { error });
        var presentation = new Presentation
        {
            OrganizationId = user.OrganizationId, CreatedByUserId = user.Id, CreatedByName = user.DisplayName ?? user.Username,
            Title = string.IsNullOrWhiteSpace(title) ? Path.GetFileNameWithoutExtension(file!.FileName) : title.Trim(),
            FileName = Path.GetFileName(file!.FileName), Html = html!, SizeBytes = Encoding.UTF8.GetByteCount(html!),
            ExpiresAt = expiresAt?.ToUniversalTime(),
        };
        await _store.SaveAsync(presentation, ct);
        return Ok(new { presentation.Id, presentation.Title, url = $"/p/{presentation.Id}" });
    }

    /// <summary>استبدال المحتوى بنسخة أحدث على نفس الرابط (صاحبه أو المسؤول).</summary>
    [HttpPut("{id}/file")]
    [RequestSizeLimit(MaxBytes + 1024 * 1024)]
    public async Task<IActionResult> Replace(string id, [FromForm] IFormFile? file, [FromForm] string? title, CancellationToken ct)
    {
        var (user, p, fail) = await ManagedAsync(id, ct);
        if (fail is not null) return fail;
        var (html, error) = await ReadHtmlAsync(file, ct);
        if (error is not null) return BadRequest(new { error });
        await _store.ReplaceAsync(id, html!, Path.GetFileName(file!.FileName), string.IsNullOrWhiteSpace(title) ? null : title.Trim(), ct);
        return NoContent();
    }

    public class ExpiryRequest { public DateTime? ExpiresAt { get; set; } }

    /// <summary>تمديد/تغيير الصلاحية أو إعادة تفعيل رابط مُلغى.</summary>
    [HttpPut("{id}/expiry")]
    public async Task<IActionResult> SetExpiry(string id, [FromBody] ExpiryRequest request, CancellationToken ct)
    {
        var (_, _, fail) = await ManagedAsync(id, ct);
        if (fail is not null) return fail;
        await _store.SetExpiryAsync(id, request.ExpiresAt?.ToUniversalTime(), ct);
        return NoContent();
    }

    [HttpPost("{id}/revoke")]
    public async Task<IActionResult> Revoke(string id, CancellationToken ct)
    {
        var (_, _, fail) = await ManagedAsync(id, ct);
        if (fail is not null) return fail;
        await _store.RevokeAsync(id, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var (_, _, fail) = await ManagedAsync(id, ct);
        if (fail is not null) return fail;
        await _store.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>بيانات العرض لصفحة المشاهدة العامة (العنوان وصاحبه وحالته) — بلا تسجيل دخول.</summary>
    [HttpGet("{id}/meta")]
    [AllowAnonymous]
    public async Task<IActionResult> Meta(string id, CancellationToken ct)
    {
        var p = await _store.GetAsync(id, ct);
        if (p is null) return NotFound(new { error = "الرابط غير موجود أو تم حذفه." });
        if (!p.IsActive)
            return StatusCode(StatusCodes.Status410Gone, new
            {
                inactive = true,
                reason = p.RevokedAt is not null ? $"تم إلغاء هذا الرابط من قِبل {p.CreatedByName ?? "صاحب الرابط"}." : "انتهت صلاحية هذا الرابط.",
                creatorName = p.CreatedByName,
            });
        return Ok(new { p.Id, p.Title, p.CreatedByName, p.UpdatedAt });
    }

    /// <summary>محتوى الصفحة كما رُفع — يُحمَّل داخل iframe معزول في صفحة المشاهدة. يزيد عدّاد المشاهدات.</summary>
    [HttpGet("{id}/content")]
    [AllowAnonymous]
    public async Task<IActionResult> Content(string id, CancellationToken ct)
    {
        var p = await _store.GetAsync(id, ct);
        if (p is null || !p.IsActive) return NotFound();
        var html = await _store.GetHtmlAsync(id, ct) ?? "";
        await _store.IncrementViewCountAsync(id, ct);
        Response.Headers["Content-Security-Policy"] = SandboxCsp;
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers["Referrer-Policy"] = "no-referrer";
        Response.Headers["Cache-Control"] = "no-store";
        return Content(html, "text/html; charset=utf-8");
    }

    private static bool CanManage(AppUser user, Presentation p) =>
        user.Role == UserRoles.Admin || string.Equals(p.CreatedByUserId, user.Id, StringComparison.Ordinal);

    private async Task<(AppUser? User, Presentation? Presentation, IActionResult? Fail)> ManagedAsync(string id, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return (null, null, Unauthorized());
        var p = await _store.GetAsync(id, ct);
        if (p is null || !string.Equals(p.OrganizationId, user.OrganizationId, StringComparison.Ordinal))
            return (user, null, NotFound(new { error = "العرض غير موجود." }));
        if (!CanManage(user, p)) return (user, p, StatusCode(403, new { error = "يمكن لصاحب العرض أو المسؤول فقط تعديله." }));
        return (user, p, null);
    }

    private static async Task<(string? Html, string? Error)> ReadHtmlAsync(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0) return (null, "اختر ملف HTML أولًا.");
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".html" or ".htm")) return (null, "الملف يجب أن يكون صفحة HTML واحدة (.html).");
        if (file.Length > MaxBytes) return (null, $"حجم الملف يتجاوز الحد ({MaxBytes / 1024 / 1024} ميغابايت). ضمّن الصور بحجم أصغر أو اربطها من رابط خارجي.");
        using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var html = await reader.ReadToEndAsync(ct);
        if (string.IsNullOrWhiteSpace(html)) return (null, "الملف فارغ.");
        return (html, null);
    }
}
