using System.Text.Json;
using ChatToDashboard.Api.Graph;
using ChatToDashboard.Api.Projects;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>
/// Serves network-graph data (nodes/edges) for the "لوحات تفاعلية" network dashboards —
/// see GraphStore's own remarks for the seeding/golden-rule story. Only nodes/edges come from
/// here, matching the reference policy-graph-data.js's own comment ("الهيكل المتوقع من الـ API:
/// nodes, edges"); node-type taxonomy (colors/shapes), relation labels, and all the
/// domain-specific presentation logic (KPI/widget/column builders) stay in the frontend's own
/// per-network config file (wwwroot/js/policy-network.js) — that's config/behavior, not data.
/// </summary>
[ApiController]
[Route("api/graph")]
public class GraphController : ControllerBase
{
    private readonly GraphStore _store;
    private readonly ProjectStore _projects;
    private readonly PermissionsService _permissions;

    public GraphController(GraphStore store, ProjectStore projects, PermissionsService permissions)
    {
        _store = store;
        _projects = projects;
        _permissions = permissions;
    }

    [HttpGet("{networkKey}")]
    public async Task<ActionResult<object>> GetNetwork(string networkKey, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();

        var isAdmin = user.Role == UserRoles.Admin;
        var projectId = await _projects.ResolveCurrentProjectIdAsync(user.OrganizationId, user.Id, isAdmin, ct);
        if (projectId is null) return NotFound();

        var (nodes, edges) = await _store.GetNetworkAsync(projectId, networkKey, ct);
        if (nodes.Count == 0) return NotFound();

        return Ok(new
        {
            nodes = nodes.Select(n =>
            {
                var extra = string.IsNullOrEmpty(n.DataJson)
                    ? new Dictionary<string, object?>()
                    : JsonSerializer.Deserialize<Dictionary<string, object?>>(n.DataJson) ?? new();
                var node = new Dictionary<string, object?>
                {
                    ["id"] = n.NodeKey, ["t"] = n.Type, ["n"] = n.Name, ["d"] = n.Description, ["y"] = n.Year,
                };
                foreach (var (key, value) in extra) node[key] = value;
                return node;
            }),
            edges = edges.Select(e => new { a = e.FromKey, b = e.ToKey, r = e.RelType, y = e.Year }),
        });
    }
}
