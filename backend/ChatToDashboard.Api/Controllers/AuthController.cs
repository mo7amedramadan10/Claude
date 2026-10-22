using System.Security.Claims;
using ChatToDashboard.Api.Auth;
using ChatToDashboard.Api.Organizations;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserStore _users;
    private readonly OrganizationStore _organizations;
    private readonly LdapAuthenticator _ldap;
    private readonly ILogger<AuthController> _logger;

    public AuthController(UserStore users, OrganizationStore organizations, LdapAuthenticator ldap, ILogger<AuthController> logger)
    {
        _users = users;
        _organizations = organizations;
        _ldap = ldap;
        _logger = logger;
    }

    /// <summary>The organization a /o/{slug} link names — public (name + slug only), so the
    /// login screen can greet the right organization by name before any credential is
    /// entered. See OrganizationStore.FindBySlugAsync's remarks on why this is safe to expose
    /// anonymously.</summary>
    [HttpGet("org/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> OrganizationBySlug(string slug, CancellationToken ct)
    {
        var org = await _organizations.FindBySlugAsync(slug, ct);
        if (org is null || !org.IsActive) return NotFound(new { error = "المنظمة غير موجودة." });
        return Ok(new { id = org.Id, name = org.Name, slug = org.Slug });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { error = "اسم المستخدم وكلمة المرور مطلوبان." });
        if (string.IsNullOrWhiteSpace(request.OrgSlug))
            return BadRequest(new { error = "رابط المنظمة مطلوب — استخدم رابط تسجيل الدخول الخاص بمنظمتك." });

        var org = await _organizations.FindBySlugAsync(request.OrgSlug.Trim(), ct);
        if (org is null || !org.IsActive)
            return Unauthorized(new { error = "اسم المستخدم أو كلمة المرور غير صحيحة." });

        var user = await _users.FindByUsernameInOrganizationAsync(org.Id, request.Username.Trim(), ct);
        if (user is null || !user.IsActive)
            return Unauthorized(new { error = "اسم المستخدم أو كلمة المرور غير صحيحة." });

        bool ok;
        if (user.AuthMethod == AuthMethods.ActiveDirectory)
        {
            var (success, error) = await _ldap.AuthenticateAsync(user.Username, request.Password, ct);
            ok = success;
            if (!success) _logger.LogInformation("AD login failed for {Username}: {Error}", user.Username, error);
        }
        else
        {
            ok = PasswordHasher.Verify(request.Password, user.PasswordHash);
        }

        if (!ok)
            return Unauthorized(new { error = "اسم المستخدم أو كلمة المرور غير صحيحة." });

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("DisplayName", user.DisplayName),
            new Claim("OrganizationId", user.OrganizationId ?? ""),
            new Claim("IsPlatformOwner", user.IsPlatformOwner ? "true" : "false"),
        }, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) });

        _logger.LogInformation("{Username} signed in ({AuthMethod})", user.Username, user.AuthMethod);
        return Ok(UserStore.ToInfo(user));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    /// <summary>The signed-in user's current info — re-read from the database (not the
    /// cookie) so a role/permission change or deactivation by an admin takes effect on
    /// the very next request instead of waiting for the cookie to expire.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = id is null ? null : await _users.FindByIdAsync(id, ct);
        if (user is null || !user.IsActive)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Unauthorized();
        }
        return Ok(UserStore.ToInfo(user));
    }
}
