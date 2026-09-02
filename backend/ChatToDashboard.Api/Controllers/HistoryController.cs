using ChatToDashboard.Api.History;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>
/// Saved dashboards ("السجل"). The app has no login, so "the current user" is a random
/// id the frontend generates once and keeps in localStorage, sent as the X-User-Id
/// header — good enough to keep one browser's history separate from another's without
/// standing up real authentication. A request with no header falls back to a shared
/// "local" bucket, so the API still works from curl/Postman.
/// </summary>
[ApiController]
[Route("api/history")]
public class HistoryController : ControllerBase
{
    private readonly HistoryStore _store;

    public HistoryController(HistoryStore store) => _store = store;

    private string UserId =>
        Request.Headers.TryGetValue("X-User-Id", out var v) && !string.IsNullOrWhiteSpace(v)
            ? v.ToString().Trim()
            : "local";

    /// <summary>Saves a generated dashboard. Called right after the chat flow renders one.</summary>
    [HttpPost]
    public async Task<IActionResult> Save([FromBody] SaveHistoryRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest(new { error = "question is required." });

        var entry = new DashboardHistoryEntry
        {
            UserId = UserId,
            Question = request.Question,
            // No separate field exists on the model's response for this — see HistoryModels.cs.
            QueryDescription = string.IsNullOrWhiteSpace(request.QueryDescription)
                ? request.Summary
                : request.QueryDescription,
            Summary = request.Summary,
            WidgetsJson = request.Widgets.ValueKind == System.Text.Json.JsonValueKind.Undefined
                ? "[]"
                : request.Widgets.GetRawText(),
        };

        var saved = await _store.SaveAsync(entry, ct);
        return Ok(saved);
    }

    /// <summary>The current user's saved dashboards, newest first.</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await _store.ListAsync(UserId, ct: ct));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var deleted = await _store.DeleteAsync(UserId, id, ct);
        return deleted ? NoContent() : NotFound(new { error = "غير موجود" });
    }

    [HttpDelete]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        await _store.ClearAsync(UserId, ct);
        return NoContent();
    }
}
