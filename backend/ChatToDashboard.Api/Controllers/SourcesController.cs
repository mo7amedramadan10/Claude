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
    /// user is actually permitted to see (admins see everything) — so a system or file
    /// someone has no access to doesn't even appear as an option. Each repository file is
    /// its own independent source here, at the same level as a system — not grouped by
    /// category.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var allowed = PermissionsService.GetEffectiveSelection(user, null);
        var isAdmin = user.Role == UserRoles.Admin;

        var repoFiles = await _store.ListAsync(ct);
        // allowed.AllowsFile is now always true (file access is never set per-user anymore —
        // see UsersController), so the actual gate for a non-Admin is the same creator/
        // explicitly-granted rule RepositoryController.Files applies to the file list itself.
        var files = repoFiles.Where(f => allowed.AllowsFile(f.Id) && (isAdmin
                || string.IsNullOrWhiteSpace(f.CreatedByUserId)
                || string.Equals(f.CreatedByUserId, user.Id, StringComparison.OrdinalIgnoreCase)
                || f.PermittedUserIds.Contains(user.Id, StringComparer.OrdinalIgnoreCase)))
            .Select(f => new { id = f.Id, name = f.DisplayName, category = f.Category })
            .ToList();
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
            files,
            tableLabels = BuildTableLabels(repoFiles, allowed),
        });
    }

    /// <summary>
    /// Staging-table name -> a friendly Arabic label ("اسم النظام" for a system-backed table,
    /// "اسم الملف (تصنيف الملف)" for a file-repository one) — lets the dashboard header show
    /// which real-world sources a set of widgets depends on (see index.html's
    /// computeDashboardSources) without the frontend ever needing to know a raw table name.
    /// Filtered to the same files the rest of this response already allows, for the same
    /// reason: a file the user has no access to shouldn't even be named to them.
    /// </summary>
    private IReadOnlyDictionary<string, string> BuildTableLabels(
        IReadOnlyList<RepositoryFile> repoFiles, SourceSelection allowed)
    {
        var labels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var system in _options.Systems)
        {
            var table = system.HasApi ? _systems.TableFor(system) : null;
            if (table is not null) labels[table] = system.Name;
        }
        foreach (var f in repoFiles)
        {
            if (string.IsNullOrWhiteSpace(f.TableName) || !allowed.AllowsFile(f.Id)) continue;
            labels[f.TableName!] = $"{f.DisplayName} (تصنيف {f.Category})";
        }
        return labels;
    }
}
