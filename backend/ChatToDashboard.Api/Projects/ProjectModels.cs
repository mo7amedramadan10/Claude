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
