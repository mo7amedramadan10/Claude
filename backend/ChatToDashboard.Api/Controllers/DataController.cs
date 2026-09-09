using ChatToDashboard.Api.Data;
using ChatToDashboard.Api.Sources;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

// Bulk data reload is an operational action, not something every user needs — admin only.
[ApiController]
[Route("api/data")]
[Authorize(Roles = UserRoles.Admin)]
public class DataController : ControllerBase
{
    private readonly DataFolderLoader _loader;
    private readonly DocumentSearchService _documents;
    private readonly SystemApiLoader _systems;
    private readonly ILogger<DataController> _logger;

    public DataController(
        DataFolderLoader loader,
        DocumentSearchService documents,
        SystemApiLoader systems,
        ILogger<DataController> logger)
    {
        _loader = loader;
        _documents = documents;
        _systems = systems;
        _logger = logger;
    }

    /// <summary>Re-scans the data folder and reloads every staging table on demand.</summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        try
        {
            var loaded = await _loader.LoadAllAsync(ct);
            _documents.Reindex(_loader.DataFolderPath);
            var systems = await _systems.LoadAllAsync(ct);
            return Ok(new
            {
                tables = loaded,
                systems,
                ragChunks = _documents.Enabled ? _documents.IndexedChunkCount : (int?)null,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Data refresh failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>Returns the tables currently loaded in the staging schema.</summary>
    [HttpGet("tables")]
    public async Task<IActionResult> Tables(CancellationToken ct)
    {
        var schema = await _loader.GetSchemaAsync(ct);
        return Ok(schema);
    }
}
