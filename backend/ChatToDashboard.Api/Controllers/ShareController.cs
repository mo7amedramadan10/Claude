using System.Security.Claims;
using ChatToDashboard.Api.History;
using ChatToDashboard.Api.Presentations;
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
    private readonly PresentationAssistantService _assistant;

    public ShareController(ShareStore store, UserStore users, HistoryStore history, PresentationAssistantService assistant)
    {
        _store = store;
        _users = users;
        _history = history;
        _assistant = assistant;
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
        var by = await _users.FindByIdAsync(entry.CreatedByUserId, ct);
        return Ok(new
        {
            entry.Id, entry.Question, entry.Summary, entry.WidgetsJson, entry.FiltersJson, entry.ActiveFiltersJson, entry.CreatedAt, entry.ExpiresAt,
            creatorName = by?.DisplayName ?? by?.Username, // لرأس صفحة المشاركة («من فلان») كصفحة العرض المنشور
        });
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

    private static bool IsLive(SharedDashboard e) => e.RevokedAt is null && (e.ExpiresAt is null || e.ExpiresAt >= DateTime.UtcNow);

    /// <summary>مساعد «جيم» على رابط لوحة متابعة مشارَكة — متاح لأي من يفتح الرابط (اللقطة عامة أصلًا)، بلا تسجيل دخول.</summary>
    [HttpGet("{id}/assistant")]
    [AllowAnonymous]
    public async Task<IActionResult> AssistantInfo(string id, CancellationToken ct)
    {
        var entry = await _store.GetAsync(id, ct);
        if (entry is null || !IsLive(entry)) return NotFound(new { error = "الرابط غير موجود أو غير نشط." });
        Response.Headers["Cache-Control"] = "no-store";
        return Ok(new { name = PresentationAssistantService.Name, enabled = true, allowed = true, signedIn = User.Identity?.IsAuthenticated == true, policy = "anyone", reason = (string?)null });
    }

    /// <summary>سؤال المساعد عن محتوى اللوحة المشارَكة (الملخص والعناصر وبياناتها). 429 عند تجاوز السقف.</summary>
    [HttpPost("{id}/assistant/ask")]
    [AllowAnonymous]
    public async Task<IActionResult> AssistantAsk(string id, [FromBody] PresentationsController.AssistantAskRequest request, CancellationToken ct)
    {
        var entry = await _store.GetAsync(id, ct);
        if (entry is null || !IsLive(entry)) return NotFound(new { error = "الرابط غير موجود أو غير نشط." });
        var question = (request.Question ?? "").Trim();
        if (question.Length == 0) return BadRequest(new { error = "اكتب سؤالًا." });
        if (question.Length > PresentationAssistantService.MaxQuestionChars) return BadRequest(new { error = $"السؤال أطول من الحد ({PresentationAssistantService.MaxQuestionChars} حرفًا)." });
        var viewerKey = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? HttpContext.Connection.RemoteIpAddress?.ToString() ?? "anon";
        if (!_assistant.TryConsume("share:" + entry.Id, viewerKey)) return StatusCode(429, new { error = "أسئلة كثيرة في وقت قصير — انتظر قليلًا ثم أعد المحاولة." });
        var history = (request.History ?? new()).Where(h => !string.IsNullOrWhiteSpace(h.Text)).TakeLast(PresentationAssistantService.MaxHistoryTurns)
            .Select(h => new AssistantTurn(h.Role == "user" ? "user" : "assistant", h.Text!.Trim())).ToList();
        try
        {
            var context = PresentationAssistantService.DashboardContext(entry.Question, entry.Summary, entry.WidgetsJson);
            var reply = await _assistant.AskWithContextAsync(entry.Question, context, entry.CreatedByUserId, question, history, ct);
            return Ok(new { reply });
        }
        catch (InvalidOperationException ex) { return StatusCode(503, new { error = ex.Message }); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return StatusCode(502, new { error = "تعذّر الحصول على رد من المساعد الآن — حاول مرة أخرى.", detail = ex.Message });
        }
    }
}
