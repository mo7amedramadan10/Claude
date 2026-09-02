using System.Text.Json.Serialization;

namespace ChatToDashboard.Api.Users;

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string User = "User";
}

public static class AuthMethods
{
    public const string Local = "Local";
    public const string ActiveDirectory = "ActiveDirectory";
}

/// <summary>
/// An account. Local accounts carry a password hash; Active Directory accounts carry
/// none — their credentials are verified against the configured directory on every
/// login instead. Per-source access (<see cref="AllowAllSystems"/>/<see cref="AllowedSystemsJson"/>
/// and the category equivalents) reuses the exact shape the chat request already sends as
/// <c>Sources.SourceSelection</c>, so it can be intersected with what the client asks for
/// without a separate model. Admins always have full access regardless of these flags.
/// </summary>
public class AppUser
{
    public string Id { get; set; } = "";
    public string Username { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string AuthMethod { get; set; } = AuthMethods.Local;
    public string Role { get; set; } = UserRoles.User;
    public bool IsActive { get; set; } = true;
    public bool AllowAllSystems { get; set; } = true;
    public string AllowedSystemsJson { get; set; } = "[]";
    public bool AllowAllCategories { get; set; } = true;
    public string AllowedCategoriesJson { get; set; } = "[]";
    public DateTime CreatedAt { get; set; }
}

/// <summary>What the client sees — never the password hash.</summary>
public class UserInfo
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("username")] public string Username { get; set; } = "";
    [JsonPropertyName("displayName")] public string DisplayName { get; set; } = "";
    [JsonPropertyName("authMethod")] public string AuthMethod { get; set; } = "";
    [JsonPropertyName("role")] public string Role { get; set; } = "";
    [JsonPropertyName("isActive")] public bool IsActive { get; set; }
    [JsonPropertyName("allowAllSystems")] public bool AllowAllSystems { get; set; }
    [JsonPropertyName("allowedSystems")] public List<string> AllowedSystems { get; set; } = new();
    [JsonPropertyName("allowAllCategories")] public bool AllowAllCategories { get; set; }
    [JsonPropertyName("allowedCategories")] public List<string> AllowedCategories { get; set; } = new();
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }
}

public class LoginRequest
{
    [JsonPropertyName("username")] public string Username { get; set; } = "";
    [JsonPropertyName("password")] public string Password { get; set; } = "";
}

/// <summary>Body for both creating and updating a user (PUT leaves Password empty to keep it unchanged).</summary>
public class UserRequest
{
    [JsonPropertyName("username")] public string Username { get; set; } = "";
    [JsonPropertyName("displayName")] public string DisplayName { get; set; } = "";
    [JsonPropertyName("password")] public string? Password { get; set; }
    [JsonPropertyName("authMethod")] public string AuthMethod { get; set; } = AuthMethods.Local;
    [JsonPropertyName("role")] public string Role { get; set; } = UserRoles.User;
    [JsonPropertyName("isActive")] public bool IsActive { get; set; } = true;
    [JsonPropertyName("allowAllSystems")] public bool AllowAllSystems { get; set; } = true;
    [JsonPropertyName("allowedSystems")] public List<string> AllowedSystems { get; set; } = new();
    [JsonPropertyName("allowAllCategories")] public bool AllowAllCategories { get; set; } = true;
    [JsonPropertyName("allowedCategories")] public List<string> AllowedCategories { get; set; } = new();
}
