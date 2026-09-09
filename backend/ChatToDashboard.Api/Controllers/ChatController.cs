using ChatToDashboard.Api.History;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Models;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    // The frontend already downsizes images before sending (see index.html); this is a
    // generous backstop against an oversized payload landing straight in the LLM request.
    private const int MaxImageDataUrlLength = 12 * 1024 * 1024;

    private readonly IDashboardGenerator _generator;
    private readonly PermissionsService _permissions;
    private readonly HistoryStore _history;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IDashboardGenerator generator, PermissionsService permissions, HistoryStore history, ILogger<ChatController> logger)
    {
        _generator = generator;
        _permissions = permissions;
        _history = history;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Post([FromBody] ChatRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new ChatResponse { Error = "message is required." });
        if (request.Image is { Length: > 0 } image &&
            (!image.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase) || image.Length > MaxImageDataUrlLength))
            return BadRequest(new ChatResponse { Error = "image must be a data:image/... URL under 12MB." });

        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        // Narrowed server-side against this user's own permissions — the client's
        // requested selection can only ever shrink, never widen, what it's allowed to see.
        var effectiveSources = PermissionsService.GetEffectiveSelection(user, request.Sources);

        try
        {
            var dashboard = await _generator.GenerateDashboardAsync(
                request.Message.Trim(), request.CurrentDashboard, effectiveSources, request.Image, user, ct);

            // Mirrors HistoryController.Update's Owner-only new-data-source guard, but on the
            // chat-continuation path: without this, an Editor could reach the same outcome —
            // a widget depending on a table the dashboard's Owner never used — just by asking
            // for it in the chat box instead of the wizard, since a chat answer would otherwise
            // land as a disconnected fresh Draft the frontend silently forks into (see
            // index.html's ask()), never touching Update()'s check at all. Checked against the
            // dashboard's own stored widgets, never the client-supplied currentDashboard, so
            // this can't be bypassed by echoing back a doctored "current" state.
            if (!string.IsNullOrWhiteSpace(request.HistoryId))
            {
                var entry = await _history.GetByIdAsync(request.HistoryId, ct);
                if (entry is not null && entry.IsActive)
                {
                    var role = await _history.ResolveRoleAsync(user.Id, entry, ct);
                    if (role == DashboardRoles.Editor && user.Role != UserRoles.Admin)
                    {
                        var newTables = dashboard.Widgets
                            .Where(w => w.Query is not null && !string.IsNullOrWhiteSpace(w.Query.Table))
                            .Select(w => w.Query!.Table);
                        var existingTables = WidgetTableExtractor.ExtractTables(entry.WidgetsJson);
                        var introduced = newTables
                            .Where(t => !existingTables.Contains(t))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();
                        if (introduced.Count > 0)
                            return BadRequest(new ChatResponse
                            {
                                Error = $"تغيير مصادر البيانات صلاحية تخص مالك اللوحة فقط — لا يمكنك كمحرِّر " +
                                    $"إضافة عنصر يعتمد على مصدر بيانات جديد ({string.Join("، ", introduced)}).",
                            });
                    }
                }
            }

            return Ok(new ChatResponse { Dashboard = dashboard });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chat request failed");
            return StatusCode(500, new ChatResponse { Error = ex.Message });
        }
    }
}
