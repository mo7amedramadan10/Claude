namespace ChatToDashboard.Api.Auth;

/// <summary>
/// Active Directory / LDAP connection settings. Left blank in the shipped config —
/// fill in via user-secrets or environment variables, same as every other credential
/// in this app (see README). Authentication binds directly as the signing-in user
/// (no service account required for that); an optional bind account is only for
/// future directory lookups, not used by login itself.
/// </summary>
public class LdapOptions
{
    public const string SectionName = "ActiveDirectory";

    public bool Enabled { get; set; }

    /// <summary>Domain controller hostname or IP, e.g. "dc01.company.local".</summary>
    public string Host { get; set; } = "";

    public int Port { get; set; } = 389;

    /// <summary>Use LDAPS (typically port 636). Requires a certificate the server trusts.</summary>
    public bool UseSsl { get; set; }

    /// <summary>
    /// DNS domain used to build the UPN for bind ("username@domain") when the entered
    /// username has no "@" already, e.g. "company.local".
    /// </summary>
    public string Domain { get; set; } = "";
}
