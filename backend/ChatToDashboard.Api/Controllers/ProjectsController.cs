using System.Security.Claims;
using ChatToDashboard.Api.Organizations;
using ChatToDashboard.Api.Projects;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>The signed-in user's own organization and its projects — the org-admin-facing
/// half of the SaaS hierarchy (see OrganizationsController for the platform-wide,
/// every-organization view). An org Admin always sees/manages every project in their org;
/// a plain "User" account sees only the project(s) ProjectStore.ProjectRoles names them on
/// (see List's remarks) — only an org Admin can create a project or change that table.</summary>
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
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private bool IsAdmin => User.IsInRole(UserRoles.Admin);

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var organizationId = OrganizationId;
        if (organizationId is null) return NotFound(new { error = "الحساب غير مرتبط بأي منظمة." });

        var org = await _organizations.FindByIdAsync(organizationId, ct);
        if (org is null) return NotFound(new { error = "المنظمة غير موجودة." });

        // Admin (org-wide access by construction, same as every other admin-gated endpoint
        // here) sees every project; a plain User only the one(s) they hold a role on — see
        // ProjectRoles' own remarks. A User with no project role at all (the common case
        // before this table existed) sees nothing here, same as before this narrowing: the
        // sidebar's "current project" widget still resolves via HistoryController's own
        // fallback-to-first-for-org, so their single-project workspace keeps working exactly
        // as before — this endpoint only drives the org-admin "المشاريع" management screen.
        var projects = IsAdmin
            ? await _projects.ListByOrganizationAsync(organizationId, ct)
            : await _projects.ListForUserAsync(UserId, organizationId, ct);
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
    [Authorize(Roles = UserRoles.Admin)]
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

    /// <summary>The Owner/Editor/Viewer assignments on one project — for the management UI.
    /// Org Admin only (a project has no non-Admin "owner" the way a dashboard does).</summary>
    [HttpGet("{id}/roles")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> GetRoles(string id, CancellationToken ct)
    {
        var project = await _projects.FindByIdAsync(id, ct);
        if (project is null || project.OrganizationId != OrganizationId) return NotFound(new { error = "المشروع غير موجود." });

        var roles = await _projects.ListRolesAsync(id, ct);
        var roleUsers = new List<object>();
        foreach (var r in roles)
        {
            var u = await _users.FindByIdAsync(r.UserId, ct);
            roleUsers.Add(new { userId = r.UserId, role = r.Role, displayName = u?.DisplayName ?? u?.Username ?? r.UserId, username = u?.Username });
        }
        return Ok(new { roles = roleUsers });
    }

    /// <summary>Grants (or changes) a named user's role on the project. No eligibility check
    /// on the target — same as DashboardRoles, their own source permissions are what's
    /// checked when they actually query data, not at assignment time.</summary>
    [HttpPut("{id}/roles/{targetUserId}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> SetRole(string id, string targetUserId, [FromBody] SetProjectRoleRequest request, CancellationToken ct)
    {
        var project = await _projects.FindByIdAsync(id, ct);
        if (project is null || project.OrganizationId != OrganizationId) return NotFound(new { error = "المشروع غير موجود." });
        if (request.Role != ProjectRoles.Owner && request.Role != ProjectRoles.Editor && request.Role != ProjectRoles.Viewer)
            return BadRequest(new { error = "الدور غير صحيح." });

        var target = await _users.FindByIdAsync(targetUserId, ct);
        if (target is null || target.OrganizationId != OrganizationId) return NotFound(new { error = "المستخدم غير موجود." });

        await _projects.SetRoleAsync(id, targetUserId, request.Role, ct);
        return NoContent();
    }

    [HttpDelete("{id}/roles/{targetUserId}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> RemoveRole(string id, string targetUserId, CancellationToken ct)
    {
        var project = await _projects.FindByIdAsync(id, ct);
        if (project is null || project.OrganizationId != OrganizationId) return NotFound(new { error = "المشروع غير موجود." });

        await _projects.RemoveRoleAsync(id, targetUserId, ct);
        return NoContent();
    }
}
