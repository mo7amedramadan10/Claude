using System.Security.Claims;
using ChatToDashboard.Api.Organizations;
using ChatToDashboard.Api.Projects;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>The signed-in user's own organization and its projects — the org-admin-facing
/// half of the SaaS hierarchy (see OrganizationsController for the platform-wide,
/// every-organization view). Any signed-in user can list; only an org Admin can create.</summary>
[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly ProjectStore _projects;
    private readonly OrganizationStore _organizations;
    private readonly UserStore _users;

    public ProjectsController(ProjectStore projects, OrganizationStore organizations, UserStore users)
    {
        _projects = projects;
        _organizations = organizations;
        _users = users;
    }

    private string? OrganizationId => User.FindFirstValue("OrganizationId") is { Length: > 0 } id ? id : null;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var organizationId = OrganizationId;
        if (organizationId is null) return NotFound(new { error = "الحساب غير مرتبط بأي منظمة." });

        var org = await _organizations.FindByIdAsync(organizationId, ct);
        if (org is null) return NotFound(new { error = "المنظمة غير موجودة." });

        var projects = await _projects.ListByOrganizationAsync(organizationId, ct);
        return Ok(new
        {
            organization = new { id = org.Id, name = org.Name, slug = org.Slug },
            projects = projects.Select(p => new ProjectInfo
            {
                Id = p.Id, OrganizationId = p.OrganizationId, Name = p.Name, Slug = p.Slug, CreatedAt = p.CreatedAt,
            }),
        });
    }

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Create([FromBody] ProjectRequest request, CancellationToken ct)
    {
        var organizationId = OrganizationId;
        if (organizationId is null) return NotFound(new { error = "الحساب غير مرتبط بأي منظمة." });
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { error = "اسم المشروع مطلوب." });

        var project = await _projects.CreateAsync(organizationId, request.Name.Trim(), ct);
        return Ok(new ProjectInfo
        {
            Id = project.Id, OrganizationId = project.OrganizationId, Name = project.Name, Slug = project.Slug, CreatedAt = project.CreatedAt,
        });
    }
}
