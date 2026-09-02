using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;

namespace ChatToDashboard.Api.Auth;

/// <summary>
/// Verifies a username/password against Active Directory by attempting an LDAP bind as
/// that user — the standard way to check AD credentials without needing a privileged
/// service account. A successful bind is proof of a valid password; nothing else about
/// the directory is read here.
/// </summary>
public class LdapAuthenticator
{
    private readonly LdapOptions _options;
    private readonly ILogger<LdapAuthenticator> _logger;

    public LdapAuthenticator(IOptions<LdapOptions> options, ILogger<LdapAuthenticator> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public bool Enabled => _options.Enabled && !string.IsNullOrWhiteSpace(_options.Host);

    public async Task<(bool Success, string? Error)> AuthenticateAsync(
        string username, string password, CancellationToken ct)
    {
        if (!Enabled)
            return (false, "تسجيل الدخول عبر Active Directory غير مفعّل على هذا السيرفر.");
        if (string.IsNullOrWhiteSpace(password))
            return (false, "كلمة المرور مطلوبة.");

        try
        {
            using var connection = new LdapConnection { SecureSocketLayer = _options.UseSsl };
            await connection.ConnectAsync(_options.Host, _options.Port);

            var principal = username.Contains('@')
                ? username
                : $"{username}@{_options.Domain}";
            await connection.BindAsync(principal, password);

            return connection.Bound
                ? (true, null)
                : (false, "بيانات الدخول غير صحيحة.");
        }
        catch (LdapException ex)
        {
            _logger.LogWarning(ex, "LDAP authentication failed for {Username}", username);
            return (false, "بيانات الدخول غير صحيحة أو تعذّر الوصول لخادم Active Directory.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LDAP connection error authenticating {Username}", username);
            return (false, "تعذّر الاتصال بخادم Active Directory.");
        }
    }
}
