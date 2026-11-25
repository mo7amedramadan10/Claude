using System.Security.Claims;
using ChatToDashboard.Api.Entities;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>أنواع الكيانات في المنظمة (مشروع، قسم، إدارة…) — القراءة لكل مستخدم في المنظمة، والإضافة والتعديل والحذف للمسؤول.</summary>
[ApiController]
[Route("api/entity-types")]
[Authorize]
public class EntityTypesController : ControllerBase
{
    private readonly EntityTypeStore _types;
    private readonly EntityStore _entities;
    public EntityTypesController(EntityTypeStore types, EntityStore projects) { _types = types; _entities = projects; }

    private string? OrganizationId => User.FindFirstValue("OrganizationId") is { Length: > 0 } id ? id : null;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        if (OrganizationId is not { } org) return NotFound(new { error = "الحساب غير مرتبط بأي منظمة." });
        var types = await _types.ListAsync(org, ct);
        var counts = new Dictionary<string, int>();
        foreach (var t in types) counts[t.Id] = await _entities.CountByTypeAsync(org, t.Id, ct);
        return Ok(new { types, icons = EntityTypeStore.Icons, usage = counts });
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Create([FromBody] EntityTypeRequest request, CancellationToken ct)
    {
        if (OrganizationId is not { } org) return NotFound(new { error = "الحساب غير مرتبط بأي منظمة." });
        var existing = await _types.ListAsync(org, ct);
        if (existing.Count >= EntityTypeStore.MaxPerOrganization) return BadRequest(new { error = $"الحد الأقصى {EntityTypeStore.MaxPerOrganization} نوعًا." });
        var t = new EntityType { Icon = "layers", IsDefault = request.IsDefault == true };
        if (EntityTypeStore.Validate(request, t, creating: true) is { } err) return BadRequest(new { error = err });
        if (existing.Any(x => x.Singular == t.Singular)) return Conflict(new { error = $"النوع «{t.Singular}» موجود بالفعل." });
        return Ok(await _types.CreateAsync(org, t, ct));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Update(string id, [FromBody] EntityTypeRequest request, CancellationToken ct)
    {
        if (OrganizationId is not { } org) return NotFound(new { error = "الحساب غير مرتبط بأي منظمة." });
        var existing = await _types.ListAsync(org, ct);
        var t = existing.FirstOrDefault(x => x.Id == id);
        if (t is null) return NotFound(new { error = "النوع غير موجود." });
        if (EntityTypeStore.Validate(request, t, creating: false) is { } err) return BadRequest(new { error = err });
        if (existing.Any(x => x.Id != id && x.Singular == t.Singular)) return Conflict(new { error = $"النوع «{t.Singular}» موجود بالفعل." });
        // الافتراضي يُنقل لنوع آخر بجعل ذلك النوع افتراضيًا، لا بإلغائه (لا بد من افتراضي واحد دائمًا)
        if (request.IsDefault == true) t.IsDefault = true;
        await _types.UpdateAsync(t, ct);
        return Ok(t);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        if (OrganizationId is not { } org) return NotFound(new { error = "الحساب غير مرتبط بأي منظمة." });
        var t = (await _types.ListAsync(org, ct)).FirstOrDefault(x => x.Id == id);
        if (t is null) return NotFound(new { error = "النوع غير موجود." });
        if (t.IsDefault) return Conflict(new { error = "لا يُحذف النوع الافتراضي — اجعل نوعًا آخر افتراضيًا أولًا." });
        var used = await _entities.CountByTypeAsync(org, id, ct);
        if (used > 0) return Conflict(new { error = $"النوع مستخدَم في {used} — غيّر نوعها أولًا." });
        await _types.DeleteAsync(org, id, ct);
        return NoContent();
    }
}
