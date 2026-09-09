using System.Text.Json.Serialization;
using ChatToDashboard.Api.Users;
using ChatToDashboard.Api.Widgets;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>Body of POST /api/widgets/forecast.</summary>
public class ForecastWidgetRequest
{
    [JsonPropertyName("labels")] public List<string> Labels { get; set; } = new();
    [JsonPropertyName("values")] public List<double> Values { get; set; } = new();
    [JsonPropertyName("periods")] public int Periods { get; set; } = 3;
    [JsonPropertyName("seasonLength")] public int? SeasonLength { get; set; }
}

public class ForecastWidgetResponse
{
    [JsonPropertyName("method")] public string Method { get; set; } = "";
    [JsonPropertyName("r2")] public double R2 { get; set; }
    [JsonPropertyName("labels")] public List<string> Labels { get; set; } = new();
    [JsonPropertyName("values")] public List<double> Values { get; set; } = new();
    [JsonPropertyName("lower")] public List<double> Lower { get; set; } = new();
    [JsonPropertyName("upper")] public List<double> Upper { get; set; } = new();
    [JsonPropertyName("note")] public string? Note { get; set; }
}

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

    /// <summary>
    /// The "🔮 توقّع الأشهر الجاية" button: a real statistical forecast (see ForecastService)
    /// computed directly on whatever data a chart already has client-side — works identically
    /// for a wizard-built widget or a chat-authored one, since neither the original SQL nor an
    /// LLM call is needed, just the numbers already on screen.
    /// </summary>
    [HttpPost("forecast")]
    public async Task<IActionResult> Forecast([FromBody] ForecastWidgetRequest request, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();

        if (request.Values is null || request.Values.Count < 2)
            return BadRequest(new { error = "محتاج نقطتين بيانات على الأقل عشان نقدر نتوقع." });

        var periods = Math.Clamp(request.Periods <= 0 ? 3 : request.Periods, 1, 12);
        ForecastOutcome outcome;
        try
        {
            outcome = ForecastService.Forecast(request.Values, periods, request.SeasonLength);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        var labels = Enumerable.Range(1, periods).Select(h => $"توقّع +{h}").ToList();
        return Ok(new ForecastWidgetResponse
        {
            Method = outcome.Method,
            R2 = outcome.RSquared,
            Note = outcome.Note,
            Labels = labels,
            Values = outcome.Points.Select(p => Math.Round(p.Value, 2)).ToList(),
            Lower = outcome.Points.Select(p => Math.Round(p.Lower, 2)).ToList(),
            Upper = outcome.Points.Select(p => Math.Round(p.Upper, 2)).ToList(),
        });
    }

    /// <summary>
    /// The dashboard-filter path for a chat-authored widget (one with a stored `query` — see
    /// DashboardWidget.Query — rather than the wizard's full structured query). Re-runs that
    /// widget's own SQL with the active filter(s) spliced in server-side; see
    /// WidgetQueryService.ExecuteSqlFilterAsync for why this is safe despite taking SQL text
    /// from the client, and for the cases it can't handle (the widget is simply left
    /// "غير متأثر بالفلتر" rather than risking a wrong result).
    /// </summary>
    [HttpPost("sql-filter")]
    public async Task<IActionResult> SqlFilter([FromBody] SqlFilterRequest request, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var effective = PermissionsService.GetEffectiveSelection(user, request.Sources);
        try
        {
            return Ok(await _service.ExecuteSqlFilterAsync(request.Table, request.Sql, request.Filters, effective, ct));
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
