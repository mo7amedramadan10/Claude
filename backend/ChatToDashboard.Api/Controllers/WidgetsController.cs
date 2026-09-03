using ChatToDashboard.Api.Users;
using ChatToDashboard.Api.Widgets;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>
/// The deterministic "Add Widget" / edit-widget path: a structured query (metric,
/// aggregation, dimension, time range — chosen through the wizard UI, no SQL involved)
/// executed directly against the data, without calling the LLM. See
/// <see cref="WidgetQueryService"/> for why this exists and how it stays safe.
/// </summary>
[ApiController]
[Route("api/widgets")]
public class WidgetsController : ControllerBase
{
    private readonly WidgetQueryService _service;
    private readonly PermissionsService _permissions;

    public WidgetsController(WidgetQueryService service, PermissionsService permissions)
    {
        _service = service;
        _permissions = permissions;
    }

    [HttpPost("fields")]
    public async Task<IActionResult> Fields([FromBody] WidgetFieldsRequest request, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var effective = PermissionsService.GetEffectiveSelection(user, request.Sources);
        return Ok(await _service.GetAvailableFieldsAsync(effective, ct));
    }

    [HttpPost("query")]
    public async Task<IActionResult> Query([FromBody] WidgetQueryEnvelope body, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var effective = PermissionsService.GetEffectiveSelection(user, body.Sources);
        try
        {
            return Ok(await _service.ExecuteAsync(body.Query, effective, ct));
        }
        catch (WidgetQueryValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Real DISTINCT values for one column — the only legitimate filter-option source.</summary>
    [HttpPost("filter-values")]
    public async Task<IActionResult> FilterValues([FromBody] FilterValuesRequest request, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var effective = PermissionsService.GetEffectiveSelection(user, request.Sources);
        try
        {
            return Ok(await _service.GetFilterValuesAsync(request.Table, request.Field, effective, ct));
        }
        catch (WidgetQueryValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
