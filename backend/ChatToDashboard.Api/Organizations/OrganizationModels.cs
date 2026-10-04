using System.Text.Json.Serialization;

namespace ChatToDashboard.Api.Organizations;

/// <summary>
/// A subscribing company on the platform (SaaS tenant). Owns one or more <see
/// cref="Projects.Project"/>s and, through them, its own isolated dashboards/sources/files —
/// see Projects.ProjectModels for how a project ties back here. Isolation for now is logical
/// (every tenant-scoped row carries an OrganizationId/ProjectId column in the one shared
/// database), not a separate physical database per organization.
/// </summary>
public class Organization
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

/// <summary>What the client sees — the raw row plus counts that only make sense aggregated.</summary>
public class OrganizationInfo
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("slug")] public string Slug { get; set; } = "";
    [JsonPropertyName("isActive")] public bool IsActive { get; set; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }
    [JsonPropertyName("projectCount")] public int ProjectCount { get; set; }
    [JsonPropertyName("userCount")] public int UserCount { get; set; }
}

public class OrganizationRequest
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
}

/// <summary>Body of POST /api/organizations/{id}/admins — the platform owner assigning (or
/// adding another) Admin-role account to someone else's organization. Always Local auth and
/// Role=Admin; there's no case for creating an Active-Directory or non-admin account here —
/// that's the org's own Admin's job via UsersController once one exists.</summary>
public class CreateOrgAdminRequest
{
    [JsonPropertyName("username")] public string Username { get; set; } = "";
    [JsonPropertyName("displayName")] public string DisplayName { get; set; } = "";
    [JsonPropertyName("password")] public string Password { get; set; } = "";
}
