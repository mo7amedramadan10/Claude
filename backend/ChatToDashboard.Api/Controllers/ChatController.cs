using ChatToDashboard.Api.History;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Models;
using ChatToDashboard.Api.Templates;
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
    private readonly TemplatePromptService _templatePrompts;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IDashboardGenerator generator, PermissionsService permissions, HistoryStore history,
        TemplatePromptService templatePrompts, ILogger<ChatController> logger)
    {
        _generator = generator;
        _permissions = permissions;
        _history = history;
        _templatePrompts = templatePrompts;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Post([FromBody] ChatRequest request, CancellationToken ct)
    {
        if (request.Template is null && string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new ChatResponse { Error = "message is required." });
        if (request.Image is { Length: > 0 } image &&
            (!image.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase) || image.Length > MaxImageDataUrlLength))
            return BadRequest(new ChatResponse { Error = "image must be a data:image/... URL under 12MB." });

        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        // Narrowed server-side against this user's own permissions — the client's
        // requested selection can only ever shrink, never widen, what it's allowed to see.
        var effectiveSources = await _permissions.GetEffectiveSelectionAsync(user, request.Sources, ct);

        // A template-originated question (chat-widget/dashboard-gallery/KPI-library "add"):
        // the browser sent only a TemplateRef, never prompt text, so the actual instruction
        // sent to the model is assembled here — request.Message is never used as the prompt
        // in this branch, only as a display label the frontend already showed in the bubble.
        string prompt;
        IReadOnlyList<BuiltinDashboardWidgetSpec>? requiredWidgets = null;
        if (request.Template is not null)
        {
            var (resolved, error, templateWidgets) = await _templatePrompts.ResolveAsync(request.Template, user, effectiveSources, ct);
            if (resolved is null) return BadRequest(new ChatResponse { Error = error ?? "تعذّر تحميل هذا النموذج." });
            prompt = resolved;
            requiredWidgets = templateWidgets;
        }
        else
        {
            prompt = request.Message.Trim();
        }

        try
        {
            var dashboard = await _generator.GenerateDashboardAsync(
                prompt, request.CurrentDashboard, effectiveSources, request.Image, user, request.Lang, ct: ct);

            // Defense in depth alongside TemplatePromptService's own required-widgets
            // instruction block: the model is told every declared template widget must appear
            // (real or an explicit noData placeholder), but nothing guarantees it actually
            // did — so reconcile here too. A required title missing from the response
            // entirely (not even as a noData placeholder) gets one appended rather than
            // silently vanishing from the dashboard the user asked for by its known shape.
            if (requiredWidgets is { Count: > 0 })
            {
                // Grouped (not a simple "is this title present at all" HashSet) because a
                // multi-page template can legitimately require the SAME literal title twice —
                // e.g. "استهلاك الميزانية" appears once under "المحفظة" and again under
                // "الميزانية" in the project-portfolio template — so one matching widget in the
                // response must only satisfy one occurrence, not both. Matched positionally
                // within each title group (response order vs. the template's own declared
                // order), which also doubles as the only place a widget's Page is ever set: the
                // model is never asked to echo a page/tab field itself, so the real tab bar
                // (app.js's getDashboardPages) relies entirely on this match against the
                // template's own declared, page-tagged widget list.
                var requiredByTitle = requiredWidgets
                    .GroupBy(w => w.Title, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);
                var responseByTitle = dashboard.Widgets
                    .GroupBy(w => w.Title, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

                foreach (var (title, reqGroup) in requiredByTitle)
                {
                    var respGroup = responseByTitle.TryGetValue(title, out var r) ? r : new List<DashboardWidget>();
                    var matched = Math.Min(reqGroup.Count, respGroup.Count);
                    for (var i = 0; i < matched; i++)
                        if (reqGroup[i].Page is not null) respGroup[i].Page = reqGroup[i].Page;
                    for (var i = matched; i < reqGroup.Count; i++)
                    {
                        var req = reqGroup[i];
                        dashboard.Widgets.Add(new DashboardWidget
                        {
                            Type = req.Type,
                            Title = req.Title,
                            Page = req.Page,
                            NoData = true,
                            MissingReason = "لم يتم تضمين هذا العنصر في رد النموذج — جرّب تحديد مصدر بيانات له يدويًا.",
                        });
                    }
                }
            }

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
