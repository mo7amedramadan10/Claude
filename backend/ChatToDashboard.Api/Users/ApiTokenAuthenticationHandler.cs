using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace ChatToDashboard.Api.Users;

/// <summary>
/// مصادقة <c>Authorization: Bearer jeem_…</c> بمفتاح وصول شخصي (<see cref="ApiTokenStore"/>): تبني نفس
/// مطالبات دخول الكوكي (انظر AuthController) فتعمل كل المتحكمات بلا تغيير وبصلاحيات صاحب المفتاح نفسها.
/// المخطط الافتراضي سياسة تحوّل الطلب هنا عند وجود الترويسة، وإلى الكوكي في غير ذلك (Program.cs).
/// </summary>
public class ApiTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string Scheme = "ApiToken";
    private readonly ApiTokenStore _tokens;
    private readonly UserStore _users;

    public ApiTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
        ApiTokenStore tokens, UserStore users) : base(options, logger, encoder)
    {
        _tokens = tokens;
        _users = users;
    }

    /// <summary>هل يحمل الطلب مفتاح وصول شخصيًا؟ (تُستخدم في سياسة اختيار المخطط).</summary>
    public static bool HasToken(HttpRequest request) =>
        request.Headers.Authorization.ToString() is { } h && h.StartsWith("Bearer " + ApiTokenStore.TokenPrefix, StringComparison.Ordinal);

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!HasToken(Request)) return AuthenticateResult.NoResult();
        var token = Request.Headers.Authorization.ToString()["Bearer ".Length..].Trim();
        var userId = await _tokens.ResolveUserIdAsync(token, Context.RequestAborted);
        if (userId is null) return AuthenticateResult.Fail("مفتاح الوصول غير صالح أو مُلغى.");
        var user = await _users.FindByIdAsync(userId, Context.RequestAborted);
        if (user is null || !user.IsActive) return AuthenticateResult.Fail("حساب المفتاح غير موجود أو موقوف.");

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("DisplayName", user.DisplayName),
            new Claim("OrganizationId", user.OrganizationId ?? ""),
            new Claim("IsPlatformOwner", user.IsPlatformOwner ? "true" : "false"),
            new Claim("AuthVia", Scheme),
        }, Scheme);
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        return Task.CompletedTask;
    }
}
