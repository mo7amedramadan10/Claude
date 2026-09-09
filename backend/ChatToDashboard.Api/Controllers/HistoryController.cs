using System.Security.Claims;
using System.Text.Json;
using ChatToDashboard.Api.History;
using ChatToDashboard.Api.Sources;
using ChatToDashboard.Api.Users;
using ChatToDashboard.Api.Widgets;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>
/// Saved dashboards ("السجل") — Drafts stay private per account (unchanged); an Active
/// dashboard is additionally visible to its Owner and any named Editor/Viewer (Part 2).
/// </summary>
[ApiController]
[Route("api/history")]
public class HistoryController : ControllerBase
{
    private readonly HistoryStore _store;
    private readonly DashboardAccessService _access;
    private readonly UserStore _users;
    private readonly WidgetQueryService _widgets;

    public HistoryController(HistoryStore store, DashboardAccessService access, UserStore users, WidgetQueryService widgets)
    {
        _store = store;
        _access = access;
        _users = users;
        _widgets = widgets;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private bool IsAdmin => User.IsInRole(Users.UserRoles.Admin);

    /// <summary>Saves a generated dashboard. Called right after the chat flow renders one.
    /// Always creates a Draft — promoting to Active is a separate, explicit step.</summary>
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
            FiltersJson = request.Filters.ValueKind == System.Text.Json.JsonValueKind.Undefined
                ? "[]"
                : request.Filters.GetRawText(),
            ActiveFiltersJson = request.ActiveFilters.ValueKind == System.Text.Json.JsonValueKind.Undefined
                ? "{}"
                : request.ActiveFilters.GetRawText(),
        };

        var saved = await _store.SaveAsync(entry, ct);
        return Ok(await EnrichAsync(saved, ct));
    }

    /// <summary>The current user's Drafts plus every Active dashboard they own or hold a
    /// role on, newest first — each annotated with the caller's role and live disabled
    /// state (see <see cref="EnrichAsync"/>).</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var entries = await _store.ListAsync(UserId, ct: ct);
        var roles = await _store.GetMyRolesAsync(UserId, entries.Select(e => e.Id).ToList(), ct);
        var results = new List<object>();
        foreach (var entry in entries)
        {
            // Ownership always wins over a (possibly stale, e.g. left over from before an
            // ownership transfer) Editor/Viewer row for the same user.
            var myRole = entry.IsActive && entry.OwnerId == UserId ? "owner" : roles.GetValueOrDefault(entry.Id);
            results.Add(await EnrichAsync(entry, ct, myRole));
        }
        return Ok(results);
    }

    /// <summary>
    /// Dashboard-editor autosave: overwrites an existing entry's widgets/summary in place
    /// instead of creating a new history row for every edit. A Draft is still creator-only;
    /// an Active dashboard also accepts its Owner or an Editor (their own data permission,
    /// not any standing grant, is what actually gates the query they can build — see
    /// DashboardAccessService).
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateHistoryRequest request, CancellationToken ct)
    {
        var entry = await _store.GetByIdAsync(id, ct);
        if (entry is null) return NotFound(new { error = "غير موجود" });

        var role = await ResolveRoleAsync(entry, ct);
        var canEdit = entry.IsActive
            ? role is "owner" or DashboardRoles.Editor || IsAdmin
            : entry.UserId == UserId;
        if (!canEdit) return NotFound(new { error = "غير موجود" });

        var widgetsJson = request.Widgets.ValueKind == JsonValueKind.Undefined ? "[]" : request.Widgets.GetRawText();

        // Only the Owner (or an Admin standing in for them) may change which data sources
        // an Active dashboard depends on — its live query always executes under the
        // Owner's own permission, so an Editor introducing a table the Owner can't access
        // would break the dashboard for everyone the moment it re-runs. Editors stay free
        // to add/remove/rework widgets built on tables the dashboard already uses.
        if (entry.IsActive && role == DashboardRoles.Editor && !IsAdmin)
        {
            var introduced = WidgetTableExtractor.ExtractTables(widgetsJson)
                .Except(WidgetTableExtractor.ExtractTables(entry.WidgetsJson), StringComparer.OrdinalIgnoreCase).ToList();
            if (introduced.Count > 0)
                return BadRequest(new
                {
                    error = $"تغيير مصادر البيانات صلاحية تخص مالك اللوحة فقط — لا يمكنك كمحرِّر إضافة عنصر يعتمد على مصدر بيانات جديد ({string.Join("، ", introduced)}).",
                });
        }

        var filtersJson = request.Filters.ValueKind == JsonValueKind.Undefined ? "[]" : request.Filters.GetRawText();
        var activeFiltersJson = request.ActiveFilters.ValueKind == JsonValueKind.Undefined ? "{}" : request.ActiveFilters.GetRawText();
        await _store.UpdateContentAsync(id, request.Summary, widgetsJson, filtersJson, activeFiltersJson, ct);
        return NoContent();
    }

    /// <summary>Renames a dashboard's title/description only — same permission as
    /// <see cref="Update"/> (a Draft's creator; an Active dashboard's Owner/Editor/Admin) but
    /// never touches widgets/filters, so it's safe to call from a plain rename UI without
    /// re-sending the whole dashboard content.</summary>
    [HttpPut("{id}/rename")]
    public async Task<IActionResult> Rename(string id, [FromBody] RenameHistoryRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest(new { error = "الاسم مطلوب." });

        var entry = await _store.GetByIdAsync(id, ct);
        if (entry is null) return NotFound(new { error = "غير موجود" });

        var role = await ResolveRoleAsync(entry, ct);
        var canEdit = entry.IsActive
            ? role is "owner" or DashboardRoles.Editor || IsAdmin
            : entry.UserId == UserId;
        if (!canEdit) return NotFound(new { error = "غير موجود" });

        await _store.RenameAsync(id, request.Question, request.Summary, ct);
        var updated = await _store.GetByIdAsync(id, ct);
        return Ok(await EnrichAsync(updated!, ct, role));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var entry = await _store.GetByIdAsync(id, ct);
        if (entry is null) return NotFound(new { error = "غير موجود" });

        if (!entry.IsActive)
        {
            var deleted = await _store.DeleteAsync(UserId, id, ct);
            return deleted ? NoContent() : NotFound(new { error = "غير موجود" });
        }

        // Deleting an Active dashboard is Owner/Admin-only — Editors may change its content
        // but not remove it out from under everyone with a role on it.
        if (entry.OwnerId != UserId && !IsAdmin) return NotFound(new { error = "غير موجود" });
        await _store.DeleteUnfilteredAsync(id, ct);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        await _store.ClearAsync(UserId, ct);
        return NoContent();
    }

    /// <summary>Promotes a Draft the caller owns to Active — they become its Owner, which
    /// requires them to hold valid permission on every source the dashboard's widgets
    /// depend on (same check as a fresh question).</summary>
    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(string id, CancellationToken ct)
    {
        var entry = await _store.GetByIdAsync(id, ct);
        if (entry is null || entry.UserId != UserId) return NotFound(new { error = "غير موجود" });
        if (entry.IsActive) return Ok(await EnrichAsync(entry, ct, "owner"));

        var reason = await _access.CheckEligibilityAsync(UserId, entry.WidgetsJson, ct);
        if (reason is not null) return BadRequest(new { error = reason });

        await _store.ActivateAsync(id, UserId, ct);
        var updated = await _store.GetByIdAsync(id, ct);
        return Ok(await EnrichAsync(updated!, ct, "owner"));
    }

    /// <summary>The Owner's/Editors'/Viewers' assignments on one Active dashboard — for the
    /// management UI. Owner-only (or Admin).</summary>
    [HttpGet("{id}/roles")]
    public async Task<IActionResult> GetRoles(string id, CancellationToken ct)
    {
        var entry = await _store.GetByIdAsync(id, ct);
        if (entry is null || !entry.IsActive) return NotFound(new { error = "غير موجود" });
        if (entry.OwnerId != UserId && !IsAdmin) return Forbid();

        var roles = await _store.ListRolesAsync(id, ct);
        // Resolved here (not left to the frontend) so a non-Admin Owner — who can't call
        // GET /api/users to build their own id->name map — still sees real names instead
        // of raw ids for the handful of people already on this dashboard.
        var owner = string.IsNullOrWhiteSpace(entry.OwnerId) ? null : await _users.FindByIdAsync(entry.OwnerId, ct);
        var roleUsers = new List<object>();
        foreach (var r in roles)
        {
            var u = await _users.FindByIdAsync(r.UserId, ct);
            roleUsers.Add(new { userId = r.UserId, role = r.Role, displayName = u?.DisplayName ?? u?.Username ?? r.UserId, username = u?.Username });
        }
        return Ok(new
        {
            ownerId = entry.OwnerId,
            ownerName = owner?.DisplayName ?? owner?.Username,
            roles = roleUsers,
        });
    }

    /// <summary>Grants (or changes) a named user's Editor/Viewer role. Owner or Admin only —
    /// no eligibility check on the target: per the design, an Editor/Viewer's own
    /// pre-existing permission is what's checked at query time, not at assignment time.</summary>
    [HttpPut("{id}/roles/{targetUserId}")]
    public async Task<IActionResult> SetRole(string id, string targetUserId, [FromBody] SetDashboardRoleRequest request, CancellationToken ct)
    {
        var entry = await _store.GetByIdAsync(id, ct);
        if (entry is null || !entry.IsActive) return NotFound(new { error = "غير موجود" });
        if (entry.OwnerId != UserId && !IsAdmin) return Forbid();
        if (request.Role != DashboardRoles.Editor && request.Role != DashboardRoles.Viewer)
            return BadRequest(new { error = "الدور غير صحيح." });
        if (targetUserId == entry.OwnerId)
            return BadRequest(new { error = "صاحب اللوحة لديه الصلاحية الكاملة بالفعل." });

        await _store.SetRoleAsync(id, targetUserId, request.Role, ct);
        return NoContent();
    }

    [HttpDelete("{id}/roles/{targetUserId}")]
    public async Task<IActionResult> RemoveRole(string id, string targetUserId, CancellationToken ct)
    {
        var entry = await _store.GetByIdAsync(id, ct);
        if (entry is null || !entry.IsActive) return NotFound(new { error = "غير موجود" });
        if (entry.OwnerId != UserId && !IsAdmin) return Forbid();

        await _store.RemoveRoleAsync(id, targetUserId, ct);
        return NoContent();
    }

    /// <summary>
    /// Owner transfer — initiated by the current Owner, or by a System Admin standing in for
    /// them. No exceptions to the eligibility rule, Admin included: the candidate must
    /// already hold valid permission on every source the dashboard depends on.
    /// </summary>
    [HttpPost("{id}/transfer-owner")]
    public async Task<IActionResult> TransferOwner(string id, [FromBody] TransferOwnerRequest request, CancellationToken ct)
    {
        var entry = await _store.GetByIdAsync(id, ct);
        if (entry is null || !entry.IsActive) return NotFound(new { error = "غير موجود" });
        if (entry.OwnerId != UserId && !IsAdmin) return Forbid();
        if (string.IsNullOrWhiteSpace(request.NewOwnerId))
            return BadRequest(new { error = "حدد المالك الجديد." });

        var reason = await _access.CheckEligibilityAsync(request.NewOwnerId, entry.WidgetsJson, ct);
        if (reason is not null)
            return BadRequest(new { error = $"لا يمكن نقل الملكية: {reason}" });

        await _store.TransferOwnerAsync(id, request.NewOwnerId, ct);
        // The new Owner's standing is now implicit and full — drop any leftover
        // Editor/Viewer row for them so they don't show up as both.
        await _store.RemoveRoleAsync(id, request.NewOwnerId, ct);
        var updated = await _store.GetByIdAsync(id, ct);
        // Not necessarily "owner" for the response's myRole — the caller (Owner or Admin)
        // may have just transferred ownership away from themselves, in which case their own
        // standing is whatever role (if any) they're left with, resolved fresh below.
        return Ok(await EnrichAsync(updated!, ct));
    }

    /// <summary>
    /// Re-runs one widget already published on this Active dashboard, with a filter
    /// selection — same "index-only, never client-supplied table/sql" shape as
    /// ShareController.RefreshWidget. Always executes under the Owner's live data
    /// permission (re-checked here, not a cached snapshot), regardless of which of the
    /// Owner/Editor/Viewer roles the caller holds — a Viewer sees the same numbers the
    /// Owner would.
    /// </summary>
    [HttpPost("{id}/widgets/{index:int}/refresh")]
    public async Task<IActionResult> RefreshWidget(
        string id, int index, [FromBody] RefreshDashboardWidgetRequest request, CancellationToken ct)
    {
        var entry = await _store.GetByIdAsync(id, ct);
        if (entry is null || !entry.IsActive) return NotFound(new { error = "غير موجود" });

        var role = await ResolveRoleAsync(entry, ct);
        if (role is null && !IsAdmin) return NotFound(new { error = "غير موجود" });

        if (string.IsNullOrWhiteSpace(entry.OwnerId))
            return Conflict(new { disabled = true, reason = "هذه اللوحة ليس لها مالك — لا يمكن تحديثها." });

        var owner = await _users.FindByIdAsync(entry.OwnerId, ct);
        if (owner is null)
            return Conflict(new { disabled = true, reason = "تعذّر العثور على مالك اللوحة." });

        var ineligible = await _access.CheckEligibilityAsync(owner, entry.WidgetsJson, ct);
        if (ineligible is not null)
            return Conflict(new
            {
                disabled = true,
                reason = "هذه اللوحة لم يعد لمالكها صلاحية وصول صالحة للبيانات ولا يمكن تحديثها — " +
                         "قم بنقل الملكية إلى شخص يملك صلاحية الوصول.",
            });

        var selection = PermissionsService.GetEffectiveSelection(owner, SourceSelection.AllEnabled());

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

    /// <summary>"owner" | "editor" | "viewer" | null for a Draft or an Active dashboard the
    /// caller has no standing on. Admins are never blocked but are not implicitly a role
    /// holder either (kept separate — see IsAdmin checks at each call site).</summary>
    private Task<string?> ResolveRoleAsync(DashboardHistoryEntry entry, CancellationToken ct) =>
        _store.ResolveRoleAsync(UserId, entry, ct);

    /// <summary>Wraps a raw entry with the caller-facing fields the frontend needs: the
    /// caller's role, and — for an Active dashboard — the live disabled state from
    /// re-validating the Owner's permission on every open (not just at save/transfer time).</summary>
    private async Task<object> EnrichAsync(DashboardHistoryEntry entry, CancellationToken ct, string? myRole = null)
    {
        bool disabled = false;
        string? disabledReason = null;
        string? ownerName = null;
        if (entry.IsActive)
        {
            myRole ??= await ResolveRoleAsync(entry, ct);
            if (string.IsNullOrWhiteSpace(entry.OwnerId))
            {
                disabled = true;
                disabledReason = "هذه اللوحة ليس لها مالك — لا يمكن تحديثها.";
            }
            else
            {
                var owner = await _users.FindByIdAsync(entry.OwnerId, ct);
                ownerName = owner?.DisplayName ?? owner?.Username;
                var reason = owner is null
                    ? "تعذّر العثور على مالك اللوحة."
                    : await _access.CheckEligibilityAsync(owner, entry.WidgetsJson, ct);
                if (reason is not null)
                {
                    disabled = true;
                    disabledReason = "هذه اللوحة لم يعد لمالكها صلاحية وصول صالحة للبيانات ولا يمكن تحديثها — " +
                                      "قم بنقل الملكية إلى شخص يملك صلاحية الوصول.";
                }
            }
        }

        return new
        {
            entry.Id, entry.UserId, entry.Question, entry.QueryDescription, entry.Summary,
            entry.WidgetsJson, entry.FiltersJson, entry.ActiveFiltersJson, entry.CreatedAt,
            entry.IsActive, entry.OwnerId, ownerName,
            myRole = entry.IsActive ? myRole : "owner",
            disabled, disabledReason,
        };
    }
}
