using System.Security.Claims;
using System.Text.Json;
using ChatToDashboard.Api.Share;
using ChatToDashboard.Api.Sources;
using ChatToDashboard.Api.Users;
using ChatToDashboard.Api.Widgets;
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
    private readonly WidgetQueryService _widgets;
    private readonly UserStore _users;

    public ShareController(ShareStore store, WidgetQueryService widgets, UserStore users)
    {
        _store = store;
        _widgets = widgets;
        _users = users;
    }

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
            FiltersJson = request.Filters.ValueKind == System.Text.Json.JsonValueKind.Undefined
                ? "[]"
                : request.Filters.GetRawText(),
            ActiveFiltersJson = request.ActiveFilters.ValueKind == System.Text.Json.JsonValueKind.Undefined
                ? "{}"
                : request.ActiveFilters.GetRawText(),
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

    /// <summary>
    /// Public: re-runs one widget already published in this share, with a filter selection
    /// the (anonymous, unauthenticated) recipient chose — the "🔄 تحديث" button and the
    /// filter bar in the shared view both go through here. Deliberately takes only an index
    /// into this share's own stored widgets plus a filter list — never a table/sql/query
    /// from the caller — so an anonymous visitor can only ever re-run a query this exact
    /// share already published (and whose unfiltered numbers are already public via GET
    /// above), never redirect it at some other table. The query then runs under the
    /// *sharer's* own data permissions (re-checked live, not a snapshot from share time) —
    /// the same "runs as its owner" model most embedded/shared BI views use, and it narrows
    /// automatically if that account's access is later reduced.
    /// </summary>
    [HttpPost("{id}/widgets/{index:int}/refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshWidget(
        string id, int index, [FromBody] RefreshShareWidgetRequest request, CancellationToken ct)
    {
        var entry = await _store.GetAsync(id, ct);
        if (entry is null) return NotFound(new { error = "الرابط غير موجود أو تم حذفه." });

        var sharer = await _users.FindByIdAsync(entry.CreatedByUserId, ct);
        if (sharer is null) return BadRequest(new { error = "تعذّر التحقق من صلاحيات صاحب الرابط." });
        var selection = PermissionsService.GetEffectiveSelection(sharer, SourceSelection.AllEnabled());

        JsonElement widget;
        try
        {
            var widgets = JsonDocument.Parse(entry.WidgetsJson).RootElement;
            if (index < 0 || index >= widgets.GetArrayLength())
                return BadRequest(new { error = "عنصر غير موجود." });
            widget = widgets[index];
        }
        catch (JsonException)
        {
            return BadRequest(new { error = "بيانات اللوحة تالفة." });
        }

        if (!widget.TryGetProperty("query", out var query) || query.ValueKind != JsonValueKind.Object)
            return BadRequest(new { error = "هذا العنصر غير قابل بالتحديث." });

        try
        {
            if (query.TryGetProperty("sql", out var sqlProp) && sqlProp.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(sqlProp.GetString()))
            {
                var table = query.TryGetProperty("table", out var t) ? t.GetString() ?? "" : "";
                var result = await _widgets.ExecuteSqlFilterAsync(
                    table, sqlProp.GetString()!, request.Filters, selection, ct);
                return Ok(new { data = result.Data });
            }

            if (query.TryGetProperty("table", out var tableProp) && tableProp.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(tableProp.GetString()))
            {
                var wizardRequest = query.Deserialize<WidgetQueryRequest>()
                    ?? throw new WidgetQueryValidationException("تعذّرت قراءة استعلام هذا العنصر.");
                wizardRequest.Filters = request.Filters;
                var result = await _widgets.ExecuteAsync(wizardRequest, selection, ct);
                return Ok(result);
            }
        }
        catch (WidgetQueryValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        return BadRequest(new { error = "هذا العنصر غير قابل بالتحديث." });
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
