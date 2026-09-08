using System.Security.Claims;
using ChatToDashboard.Api.History;
using ChatToDashboard.Api.Share;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>
/// Publishes a frozen snapshot of a dashboard — summary/widgets exactly as they were the
/// moment the link was created, never re-executed and never permission-checked again (Part
/// 3 of the design: independent of the Active-dashboard Owner/Editor/Viewer model in
/// HistoryController). "Who created it" is the signed-in account; GET-by-id is the one
/// deliberate exception to "everything requires login" — the whole point of a share link
/// is that the person opening it doesn't need an account.
/// </summary>
[ApiController]
[Route("api/share")]
public class ShareController : ControllerBase
{
    private readonly ShareStore _store;
    private readonly UserStore _users;
    private readonly HistoryStore _history;

    public ShareController(ShareStore store, UserStore users, HistoryStore history)
    {
        _store = store;
        _users = users;
        _history = history;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShareRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest(new { error = "question is required." });

        // A Draft has no Owner and is visible only to its creator — a share link off one
        // would be an orphaned, unmanageable copy of someone's private work-in-progress.
        // Only a dashboard that's been promoted to Active (see HistoryController.Activate)
        // may be shared.
        if (string.IsNullOrWhiteSpace(request.HistoryId))
            return BadRequest(new { error = "يجب تفعيل اللوحة أولاً قبل مشاركتها." });
        var dashboard = await _history.GetByIdAsync(request.HistoryId, ct);
        if (dashboard is null || !dashboard.IsActive)
            return BadRequest(new { error = "يجب تفعيل اللوحة أولاً قبل مشاركتها." });

        var entry = new SharedDashboard
        {
            CreatedByUserId = UserId,
            Question = request.Question,
            Summary = request.Summary,
            WidgetsJson = request.Widgets.ValueKind == System.Text.Json.JsonValueKind.Undefined
                ? "[]"
                : request.Widgets.GetRawText(),
            FiltersJson = request.Filters.ValueKind == System.Text.Json.JsonValueKind.Undefined
                ? "[]"
                : request.Filters.GetRawText(),
            ActiveFiltersJson = request.ActiveFilters.ValueKind == System.Text.Json.JsonValueKind.Undefined
                ? "{}"
                : request.ActiveFilters.GetRawText(),
            ExpiresAt = request.ExpiresAt,
        };

        var saved = await _store.SaveAsync(entry, ct);
        return Ok(saved);
    }

    /// <summary>
    /// Public: anyone with the id can view the shared dashboard — no login needed. Once
    /// expired or manually revoked, this stops serving the snapshot and instead returns a
    /// clear "no longer active" message naming the creator, rather than a generic 404 —
    /// the link itself still "exists", it's just inactive.
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(string id, CancellationToken ct)
    {
        var entry = await _store.GetAsync(id, ct);
        if (entry is null) return NotFound(new { error = "الرابط غير موجود أو تم حذفه." });

        if (entry.RevokedAt is not null || (entry.ExpiresAt is not null && entry.ExpiresAt < DateTime.UtcNow))
        {
            var creator = await _users.FindByIdAsync(entry.CreatedByUserId, ct);
            var creatorName = creator?.DisplayName ?? creator?.Username ?? "صاحب الرابط";
            return StatusCode(StatusCodes.Status410Gone, new
            {
                inactive = true,
                reason = entry.RevokedAt is not null
                    ? $"تم إلغاء هذا الرابط من قِبل {creatorName}."
                    : "انتهت صلاحية هذا الرابط.",
                creatorName,
            });
        }

        await _store.IncrementViewCountAsync(id, ct);
        return Ok(entry);
    }

    /// <summary>The current user's own published links, for management — includes each
    /// one's expiry/revoked state and view count.</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await _store.ListAsync(UserId, ct));

    /// <summary>Manually disables a link before it expires — the creator only.</summary>
    [HttpPost("{id}/revoke")]
    public async Task<IActionResult> Revoke(string id, CancellationToken ct)
    {
        var revoked = await _store.RevokeAsync(UserId, id, ct);
        return revoked ? NoContent() : NotFound(new { error = "غير موجود" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var deleted = await _store.DeleteAsync(UserId, id, ct);
        return deleted ? NoContent() : NotFound(new { error = "غير موجود" });
    }
}
