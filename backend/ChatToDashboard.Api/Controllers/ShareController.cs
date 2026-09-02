using System.Security.Claims;
using ChatToDashboard.Api.Share;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>
/// Publishes a dashboard under a link anyone can open, read-only, without the chat app
/// around it. "Who created it" is now the signed-in account; GET-by-id is the one
/// deliberate exception to "everything requires login" — the whole point of a share link
/// is that the person opening it doesn't need an account.
/// </summary>
[ApiController]
[Route("api/share")]
public class ShareController : ControllerBase
{
    private readonly ShareStore _store;

    public ShareController(ShareStore store) => _store = store;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShareRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest(new { error = "question is required." });

        var entry = new SharedDashboard
        {
            CreatedByUserId = UserId,
            Question = request.Question,
            Summary = request.Summary,
            WidgetsJson = request.Widgets.ValueKind == System.Text.Json.JsonValueKind.Undefined
                ? "[]"
                : request.Widgets.GetRawText(),
        };

        var saved = await _store.SaveAsync(entry, ct);
        return Ok(saved);
    }

    /// <summary>Public: anyone with the id can view the shared dashboard — no login needed.</summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(string id, CancellationToken ct)
    {
        var entry = await _store.GetAsync(id, ct);
        return entry is null ? NotFound(new { error = "الرابط غير موجود أو تم حذفه." }) : Ok(entry);
    }

    /// <summary>The current user's own published links, for management.</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await _store.ListAsync(UserId, ct));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var deleted = await _store.DeleteAsync(UserId, id, ct);
        return deleted ? NoContent() : NotFound(new { error = "غير موجود" });
    }
}
