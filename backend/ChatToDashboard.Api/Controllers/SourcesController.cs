using ChatToDashboard.Api.Repository;
using ChatToDashboard.Api.Sources;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ChatToDashboard.Api.Controllers;

[ApiController]
[Route("api/sources")]
public class SourcesController : ControllerBase
{
    private readonly SourceOptions _options;
    private readonly RepositoryStore _store;
    private readonly SystemApiLoader _systems;
    private readonly PermissionsService _permissions;

    public SourcesController(
        IOptions<SourceOptions> options, RepositoryStore store, SystemApiLoader systems, PermissionsService permissions)
    {
        _options = options.Value;
        _store = store;
        _systems = systems;
        _permissions = permissions;
    }

    /// <summary>Re-fetches one system's records from its endpoint, on demand — an operational action, admin only.</summary>
    [HttpPost("{id}/refresh")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Refresh(string id, CancellationToken ct)
    {
        var system = _systems.Find(id);
        if (system is null) return NotFound(new { error = "نظام غير معروف." });
        if (!system.HasApi)
            return BadRequest(new { error = $"\"{system.Name}\" غير مربوط بـ endpoint، فمفيش بيانات تُجلب." });

        var result = await _systems.LoadOneAsync(id, ct);
        return result?.Error is null
            ? Ok(new { system = system.Name, records = result?.Records ?? 0 })
            : StatusCode(502, new { system = system.Name, error = result.Error });
    }

    /// <summary>
    /// The source list the header dropdown is built from. Filtered to what the signed-in
    /// user is actually permitted to see (admins see everything) — so a system or
    /// category someone has no access to doesn't even appear as an option.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var allowed = PermissionsService.GetEffectiveSelection(user, null);

        var categories = (await _store.ListCategoriesAsync(ct)).Where(allowed.AllowsCategory).ToList();
        return Ok(new
        {
            systems = _options.Systems.Where(s => allowed.AllowsSystem(s.Id)).Select(s =>
            {
                var status = _systems.StatusFor(s.Id);
                return new
                {
                    id = s.Id,
                    name = s.Name,
                    connected = s.IsConnected,
                    // Only a system with an endpoint can be refreshed from the UI, and only an admin may.
                    refreshable = s.HasApi && user.Role == UserRoles.Admin,
                    lastRefreshed = status.LastRefreshed,
                    records = status.Records,
                    error = status.Error,
                    refreshing = status.Refreshing,
                };
            }),
            categories,
        });
    }
}
