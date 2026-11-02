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
    private readonly DataStore _db;
    private readonly SystemApiLoader _systems;
    private readonly ILogger<DataController> _logger;

    public DataController(
        DataStore db,
        SystemApiLoader systems,
        ILogger<DataController> logger)
    {
        _db = db;
        _systems = systems;
        _logger = logger;
    }

    /// <summary>يعيد تحميل كل الأنظمة الخارجية المفعَّلة على الطلب (آلية مجلد البيانات أُزيلت).</summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        try
        {
            var systems = await _systems.LoadAllAsync(ct);
            return Ok(new { systems });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Data refresh failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>Returns the tables currently loaded in the shared schema — an admin-only
    /// operational view, deliberately not scoped to any one project (deployment-wide reference
    /// data such as external systems' tables, not any project's own).</summary>
    [HttpGet("tables")]
    public async Task<IActionResult> Tables(CancellationToken ct)
    {
        var schema = await _db.GetSchemaAsync(null, ct);
        return Ok(schema);
    }
}
