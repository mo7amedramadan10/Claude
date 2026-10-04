using System.Text.Json.Serialization;

namespace ChatToDashboard.Api.Projects;

/// <summary>
/// One workspace under an <see cref="Organizations.Organization"/> — its own chat/dashboards,
/// sources and file repo (per the SaaS hierarchy: Platform → Organization → Project). An
/// organization can hold several; today only the organization's default project is actually
/// wired into the existing chat/dashboard/sources/files screens (see ProjectsController
/// remarks) — creating a second project records it, but switching the live workspace to it is
/// a follow-up.
/// </summary>
public class Project
{
    public string Id { get; set; } = "";
    public string OrganizationId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class ProjectInfo
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("organizationId")] public string OrganizationId { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("slug")] public string Slug { get; set; } = "";
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }
}

public class ProjectRequest
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
}

/// <summary>One row of <c>ProjectRoles</c>: a named user's role on one project — unlike
/// DashboardRoles (where the Owner lives on the dashboard row itself), a project has no
/// single natural "creator" owner, so Owner/Editor/Viewer all live here alike. An org
/// Admin/platform owner always sees every project in their org regardless of this table —
/// it only ever narrows a plain "User" account down to the project(s) they're named on.</summary>
public class ProjectRoleEntry
{
    public string ProjectId { get; set; } = "";
    public string UserId { get; set; } = "";

    /// <summary>"owner", "editor" or "viewer".</summary>
    public string Role { get; set; } = "";
}

public static class ProjectRoles
{
    public const string Owner = "owner";
    public const string Editor = "editor";
    public const string Viewer = "viewer";
}

/// <summary>Body of PUT /api/projects/{id}/roles/{userId}.</summary>
public class SetProjectRoleRequest
{
    [JsonPropertyName("role")] public string Role { get; set; } = "";
}
