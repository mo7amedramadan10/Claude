using ChatToDashboard.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

[ApiController]
[Route("api/data")]
public class DataController : ControllerBase
{
    private readonly DataFolderLoader _loader;
    private readonly DocumentSearchService _documents;
    private readonly ILogger<DataController> _logger;

    public DataController(DataFolderLoader loader, DocumentSearchService documents, ILogger<DataController> logger)
    {
        _loader = loader;
        _documents = documents;
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
            return Ok(new { tables = loaded, ragChunks = _documents.Enabled ? _documents.IndexedChunkCount : (int?)null });
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
