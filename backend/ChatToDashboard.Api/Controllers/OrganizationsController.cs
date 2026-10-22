using ChatToDashboard.Api.Organizations;
using ChatToDashboard.Api.Projects;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>Platform-level view across every organization (SaaS tenant) — "us, the company
/// that owns the system" per the product's own framing. Platform-owner only (see
/// AppUser.IsPlatformOwner); an organization's own admin manages just their org/projects
/// through ProjectsController instead.</summary>
[ApiController]
[Route("api/organizations")]
[Authorize(Policy = "PlatformOwner")]
public class OrganizationsController : ControllerBase
{
    private readonly OrganizationStore _organizations;
    private readonly ProjectStore _projects;
    private readonly UserStore _users;

    public OrganizationsController(OrganizationStore organizations, ProjectStore projects, UserStore users)
    {
        _organizations = organizations;
        _projects = projects;
        _users = users;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var orgs = await _organizations.ListAsync(ct);
        var result = new List<OrganizationInfo>();
        foreach (var org in orgs)
        {
            result.Add(new OrganizationInfo
            {
                Id = org.Id,
                Name = org.Name,
                Slug = org.Slug,
                IsActive = org.IsActive,
                CreatedAt = org.CreatedAt,
                ProjectCount = await _projects.CountForOrganizationAsync(org.Id, ct),
                UserCount = await _users.CountByOrganizationAsync(org.Id, ct),
            });
        }
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id, CancellationToken ct)
    {
        var org = await _organizations.FindByIdAsync(id, ct);
        if (org is null) return NotFound(new { error = "المنظمة غير موجودة." });

        return Ok(new
        {
            organization = new OrganizationInfo
            {
                Id = org.Id,
                Name = org.Name,
                Slug = org.Slug,
                IsActive = org.IsActive,
                CreatedAt = org.CreatedAt,
                ProjectCount = await _projects.CountForOrganizationAsync(org.Id, ct),
                UserCount = await _users.CountByOrganizationAsync(org.Id, ct),
            },
            projects = (await _projects.ListByOrganizationAsync(org.Id, ct))
                .Select(p => new ProjectInfo { Id = p.Id, OrganizationId = p.OrganizationId, Name = p.Name, Slug = p.Slug, CreatedAt = p.CreatedAt }),
            users = (await _users.ListByOrganizationAsync(org.Id, ct)).Select(UserStore.ToInfo),
        });
    }

    /// <summary>Creates the organization and its first project in one step — a subscribing
    /// company always starts with at least one project (see the SaaS hierarchy remarks on
    /// Organization/Project), there's no useful state of "organization with zero projects".</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OrganizationRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "اسم المنظمة مطلوب." });

        var org = await _organizations.CreateAsync(request.Name.Trim(), ct);
        var project = await _projects.CreateAsync(org.Id, "المشروع الرئيسي", ct);

        return Ok(new
        {
            organization = new OrganizationInfo { Id = org.Id, Name = org.Name, Slug = org.Slug, IsActive = org.IsActive, CreatedAt = org.CreatedAt, ProjectCount = 1, UserCount = 0 },
            project = new ProjectInfo { Id = project.Id, OrganizationId = project.OrganizationId, Name = project.Name, Slug = project.Slug, CreatedAt = project.CreatedAt },
        });
    }

    /// <summary>Assigns (creates) an Admin account for someone else's organization — the
    /// platform owner's half of "build an organization and put an admin on it"; the org's
    /// own Admin then creates everyone else through UsersController, which only ever works
    /// within the caller's own organization.</summary>
    [HttpPost("{id}/admins")]
    public async Task<IActionResult> CreateAdmin(string id, [FromBody] CreateOrgAdminRequest request, CancellationToken ct)
    {
        var org = await _organizations.FindByIdAsync(id, ct);
        if (org is null) return NotFound(new { error = "المنظمة غير موجودة." });
        if (string.IsNullOrWhiteSpace(request.Username)) return BadRequest(new { error = "اسم المستخدم مطلوب." });
        if (string.IsNullOrWhiteSpace(request.Password)) return BadRequest(new { error = "كلمة المرور مطلوبة." });
        if (await _users.FindByUsernameInOrganizationAsync(org.Id, request.Username.Trim(), ct) is not null)
            return Conflict(new { error = "اسم المستخدم ده موجود بالفعل في هذه المنظمة." });

        var admin = new AppUser
        {
            Username = request.Username.Trim(),
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? request.Username.Trim() : request.DisplayName.Trim(),
            AuthMethod = AuthMethods.Local,
            Role = UserRoles.Admin,
            IsActive = true,
            PasswordHash = PasswordHasher.Hash(request.Password),
            AllowAllSystems = true,
            AllowAllFiles = true,
            OrganizationId = org.Id,
            IsPlatformOwner = false,
        };
        var created = await _users.CreateAsync(admin, ct);
        return Ok(UserStore.ToInfo(created));
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(string id, CancellationToken ct) => await SetActive(id, true, ct);

    [HttpPost("{id}/suspend")]
    public async Task<IActionResult> Suspend(string id, CancellationToken ct) => await SetActive(id, false, ct);

    private async Task<IActionResult> SetActive(string id, bool isActive, CancellationToken ct)
    {
        var org = await _organizations.FindByIdAsync(id, ct);
        if (org is null) return NotFound(new { error = "المنظمة غير موجودة." });
        await _organizations.SetActiveAsync(id, isActive, ct);
        return NoContent();
    }
}
