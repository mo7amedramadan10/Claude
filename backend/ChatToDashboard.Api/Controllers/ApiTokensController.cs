using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

public class CreateApiTokenRequest { public string? Name { get; set; } }

/// <summary>مفاتيح الوصول الشخصية للمستخدم الحالي (انظر <see cref="ApiTokenStore"/>). لا تُدار بمفتاح
/// وصول آخر: الإنشاء والإلغاء من جلسة الكوكي فقط حتى لا يولّد مفتاح مسرَّب مفاتيح جديدة.</summary>
[ApiController]
[Route("api/tokens")]
public class ApiTokensController : ControllerBase
{
    private readonly ApiTokenStore _tokens;
    private readonly PermissionsService _permissions;

    public ApiTokensController(ApiTokenStore tokens, PermissionsService permissions)
    {
        _tokens = tokens;
        _permissions = permissions;
    }

    private bool ViaToken => User.HasClaim("AuthVia", ApiTokenAuthenticationHandler.Scheme);

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        return Ok(await _tokens.ListAsync(user.Id, ct));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateApiTokenRequest request, CancellationToken ct)
    {
        if (ViaToken) return StatusCode(403, new { error = "لا يمكن إنشاء مفتاح وصول بمفتاح وصول — سجّل الدخول من المتصفح." });
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var name = string.IsNullOrWhiteSpace(request.Name) ? "Claude Desktop" : request.Name.Trim();
        if (name.Length > 100) return BadRequest(new { error = "الاسم طويل جدًا." });
        var (token, record) = await _tokens.CreateAsync(user.Id, name, ct);
        // المفتاح الكامل يُرجَع هنا فقط ولا يمكن استرجاعه لاحقًا.
        return Ok(new { token, record.Id, record.Name, record.Prefix, record.CreatedAt });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Revoke(string id, CancellationToken ct)
    {
        if (ViaToken) return StatusCode(403, new { error = "إلغاء المفاتيح من المتصفح فقط." });
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        return await _tokens.RevokeAsync(user.Id, id, ct) ? NoContent() : NotFound(new { error = "المفتاح غير موجود." });
    }

    /// <summary>من أنا؟ — لاختبار المفتاح من الجسر (يعمل بالكوكي أو بالمفتاح).</summary>
    [HttpGet("whoami")]
    public async Task<IActionResult> WhoAmI(CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        return Ok(new { user.Id, user.Username, user.DisplayName, user.Role, user.OrganizationId, viaToken = ViaToken });
    }
}
