using ChatToDashboard.Api.Repository;
using ChatToDashboard.Api.Sources;
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

    public SourcesController(IOptions<SourceOptions> options, RepositoryStore store, SystemApiLoader systems)
    {
        _options = options.Value;
        _store = store;
        _systems = systems;
    }

    /// <summary>
    /// The source list the header dropdown is built from: the configured systems, plus the
    /// categories that actually exist in the file repository right now.
    /// </summary>
    /// <summary>Re-fetches one system's records from its endpoint, on demand.</summary>
    [HttpPost("{id}/refresh")]
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

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var categories = await _store.ListCategoriesAsync(ct);
        return Ok(new
        {
            systems = _options.Systems.Select(s =>
            {
                var status = _systems.StatusFor(s.Id);
                return new
                {
                    id = s.Id,
                    name = s.Name,
                    connected = s.IsConnected,
                    // Only a system with an endpoint can be refreshed from the UI.
                    refreshable = s.HasApi,
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
