using System.Security.Claims;
using ChatToDashboard.Api.Organizations;
using ChatToDashboard.Api.Entities;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>The signed-in user's own organization and its projects — the org-admin-facing
/// half of the SaaS hierarchy (see OrganizationsController for the platform-wide,
/// every-organization view). An org Admin always sees/manages every project in their org;
/// a plain "User" account sees only the project(s) EntityStore.EntityRoles names them on
/// (see List's remarks) — only an org Admin can create a project or change that table.</summary>
[ApiController]
[Route("api/entities")]
// المسار القديم يبقى مؤقتًا لأي عميل لم يُحدَّث بعد (الواجهة الحالية تستخدم /api/entities)
[Route("api/projects")]
public class EntitiesController : ControllerBase
{
    private readonly EntityStore _entities;
    private readonly OrganizationStore _organizations;
    private readonly UserStore _users;
    private readonly EntityTypeStore _types;

    public EntitiesController(EntityStore projects, OrganizationStore organizations, UserStore users, EntityTypeStore types)
    {
        _types = types;
        _entities = projects;
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
        // EntityRoles' own remarks. A User with no role at all sees no entity (currentId null):
        // EntityGate then refuses every entity-scoped API and the UI shows «لم تُضف إلى أي كيان».
        var projects = IsAdmin
            ? await _entities.ListByOrganizationAsync(organizationId, ct)
            : await _entities.ListForUserAsync(UserId, organizationId, ct);
        // أنواع الكيانات (مشروع/قسم/إدارة…) ونوع كل كيان الفعلي — الواجهة تعرض الأسماء بها
        var types = await _types.ListAsync(organizationId, ct);
        return Ok(new
        {
            organization = new { id = org.Id, name = org.Name, slug = org.Slug },
            types,
            entities = projects.Select(p => Info(p, types)),
            // مساحة العمل الحالية للمستخدم (اختياره المحفوظ، وإلا الكيان الافتراضي)
            currentId = await _entities.ResolveCurrentEntityIdAsync(organizationId, UserId, IsAdmin, ct),
        });
    }

    /// <summary>تبديل مساحة العمل: يحفظ الكيان المختار للمستخدم، فتعمل عليه كل الشاشات (المحادثة واللوحات والتصاميم والملفات…).</summary>
    [HttpPut("current")]
    public async Task<IActionResult> SetCurrent([FromBody] EntityCurrentRequest request, CancellationToken ct)
    {
        var organizationId = OrganizationId;
        if (organizationId is null) return NotFound(new { error = "الحساب غير مرتبط بأي منظمة." });
        if (string.IsNullOrWhiteSpace(request.EntityId)) return BadRequest(new { error = "اختر كيانًا." });
        if (!await _entities.CanWorkInAsync(request.EntityId, organizationId, UserId, IsAdmin, ct))
            return StatusCode(403, new { error = "ليس لك دور في هذا الكيان — اطلب من مسؤول المنظمة إضافتك إليه." });
        await _entities.SetCurrentChoiceAsync(UserId, request.EntityId, ct);
        return Ok(new { currentId = request.EntityId });
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Create([FromBody] EntityRequest request, CancellationToken ct)
    {
        var organizationId = OrganizationId;
        if (organizationId is null) return NotFound(new { error = "الحساب غير مرتبط بأي منظمة." });
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { error = "الاسم مطلوب." });
        var types = await _types.ListAsync(organizationId, ct);
        if (request.TypeId is not null && types.All(t => t.Id != request.TypeId)) return BadRequest(new { error = "النوع غير موجود." });

        var project = await _entities.CreateAsync(organizationId, request.Name.Trim(), ct, request.TypeId);
        return Ok(Info(project, types));
    }

    /// <summary>تعديل اسم الكيان ونوعه (المسؤول فقط).</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Update(string id, [FromBody] EntityRequest request, CancellationToken ct)
    {
        var project = await _entities.FindByIdAsync(id, ct);
        if (project is null || project.OrganizationId != OrganizationId) return NotFound(new { error = "غير موجود." });
        var name = string.IsNullOrWhiteSpace(request.Name) ? project.Name : request.Name.Trim();
        if (name.Length > 200) return BadRequest(new { error = "الاسم أطول من 200 حرف." });
        var types = await _types.ListAsync(project.OrganizationId, ct);
        var typeId = request.TypeId ?? project.TypeId;
        if (typeId is not null && types.All(t => t.Id != typeId)) return BadRequest(new { error = "النوع غير موجود." });
        await _entities.UpdateAsync(id, name, typeId, ct);
        project.Name = name; project.TypeId = typeId;
        return Ok(Info(project, types));
    }

    private static EntityInfo Info(Entity p, IReadOnlyList<EntityType> types) => new()
    {
        Id = p.Id, OrganizationId = p.OrganizationId, Name = p.Name, Slug = p.Slug, CreatedAt = p.CreatedAt,
        TypeId = EntityTypeStore.Resolve(types, p.TypeId)?.Id,
    };

    /// <summary>The Owner/Editor/Viewer assignments on one project — for the management UI.
    /// Org Admin only (a project has no non-Admin "owner" the way a dashboard does).</summary>
    [HttpGet("{id}/roles")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> GetRoles(string id, CancellationToken ct)
    {
        var project = await _entities.FindByIdAsync(id, ct);
        if (project is null || project.OrganizationId != OrganizationId) return NotFound(new { error = "المشروع غير موجود." });

        var roles = await _entities.ListRolesAsync(id, ct);
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
    public async Task<IActionResult> SetRole(string id, string targetUserId, [FromBody] SetEntityRoleRequest request, CancellationToken ct)
    {
        var project = await _entities.FindByIdAsync(id, ct);
        if (project is null || project.OrganizationId != OrganizationId) return NotFound(new { error = "المشروع غير موجود." });
        if (request.Role != EntityRoles.Owner && request.Role != EntityRoles.Editor && request.Role != EntityRoles.Viewer)
            return BadRequest(new { error = "الدور غير صحيح." });

        var target = await _users.FindByIdAsync(targetUserId, ct);
        if (target is null || target.OrganizationId != OrganizationId) return NotFound(new { error = "المستخدم غير موجود." });

        await _entities.SetRoleAsync(id, targetUserId, request.Role, ct);
        return NoContent();
    }

    /// <summary>صلاحيات مستخدم واحد على كل كيانات المنظمة — لتبويب «صلاحية الكيانات» في نافذة المستخدم.</summary>
    [HttpGet("user-roles/{targetUserId}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> GetUserRoles(string targetUserId, CancellationToken ct)
    {
        if (OrganizationId is not { } orgId) return NotFound();
        var target = await _users.FindByIdAsync(targetUserId, ct);
        if (target is null || target.OrganizationId != orgId) return NotFound(new { error = "المستخدم غير موجود." });
        var entities = await _entities.ListByOrganizationAsync(orgId, ct);
        var types = await _types.ListAsync(orgId, ct);
        var result = new List<object>();
        foreach (var e in entities)
        {
            var role = (await _entities.ListRolesAsync(e.Id, ct)).FirstOrDefault(r => r.UserId == targetUserId)?.Role;
            result.Add(new { id = e.Id, name = e.Name, typeId = EntityTypeStore.Resolve(types, e.TypeId)?.Id, role });
        }
        return Ok(new { entities = result });
    }

    /// <summary>يستبدل كل أدوار المستخدم على كيانات منظمته بالقائمة المرسلة (الكيان الغائب منها = بلا دور).</summary>
    [HttpPut("user-roles/{targetUserId}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> SetUserRoles(string targetUserId, [FromBody] SetUserEntityRolesRequest request, CancellationToken ct)
    {
        if (OrganizationId is not { } orgId) return NotFound();
        var target = await _users.FindByIdAsync(targetUserId, ct);
        if (target is null || target.OrganizationId != orgId) return NotFound(new { error = "المستخدم غير موجود." });
        var wanted = new Dictionary<string, string>();
        foreach (var r in request.Roles ?? new())
        {
            if (string.IsNullOrWhiteSpace(r.EntityId)) continue;
            if (r.Role != EntityRoles.Owner && r.Role != EntityRoles.Editor && r.Role != EntityRoles.Viewer)
                return BadRequest(new { error = "الدور غير صحيح." });
            wanted[r.EntityId] = r.Role;
        }
        var entities = await _entities.ListByOrganizationAsync(orgId, ct);
        if (wanted.Keys.Any(k => entities.All(e => e.Id != k))) return BadRequest(new { error = "كيان غير موجود في المنظمة." });
        foreach (var e in entities)
        {
            if (wanted.TryGetValue(e.Id, out var role)) await _entities.SetRoleAsync(e.Id, targetUserId, role, ct);
            else await _entities.RemoveRoleAsync(e.Id, targetUserId, ct);
        }
        return NoContent();
    }

    [HttpDelete("{id}/roles/{targetUserId}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> RemoveRole(string id, string targetUserId, CancellationToken ct)
    {
        var project = await _entities.FindByIdAsync(id, ct);
        if (project is null || project.OrganizationId != OrganizationId) return NotFound(new { error = "المشروع غير موجود." });

        await _entities.RemoveRoleAsync(id, targetUserId, ct);
        return NoContent();
    }
}

/// <summary>طلب تبديل الكيان الحالي.</summary>
public class EntityCurrentRequest { public string EntityId { get; set; } = ""; }

/// <summary>طلب ضبط أدوار مستخدم على كيانات منظمته.</summary>
public class SetUserEntityRolesRequest { public List<UserEntityRole> Roles { get; set; } = new(); }
public class UserEntityRole { public string EntityId { get; set; } = ""; public string Role { get; set; } = ""; }
