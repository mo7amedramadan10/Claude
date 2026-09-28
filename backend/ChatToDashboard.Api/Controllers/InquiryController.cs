using ChatToDashboard.Api.Inquiry;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Models;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>
/// "الاستفسارات" — a conversational, text-only sibling of /api/chat: same tool-use flow
/// (list_files/query_data/forecast_data/search_documents, same source gating via
/// PermissionsService), but the model answers with labeled blocks (data-grounded vs. general
/// knowledge) instead of building a dashboard, and every conversation is saved server-side
/// (see ConversationStore) so the user can pick it back up later. Never touches the
/// dashboard-history feature directly — only "🔄 حوّله لداشبورد" ever bridges the two, and
/// only on explicit click (see Convert below).
/// </summary>
[ApiController]
[Route("api/inquiry")]
public class InquiryController : ControllerBase
{
    /// <summary>How many of a conversation's own most recent turns are replayed to the model
    /// verbatim on every ask — everything older than this is covered only by the model's own
    /// rolling summary (see AnalyticsTools.ComposeInquiryUserMessage) plus the findings
    /// ledger, which is never trimmed regardless of age.</summary>
    private const int RecentTurnsVerbatimCount = 6;

    private readonly IDashboardGenerator _generator;
    private readonly ConversationStore _conversations;
    private readonly InquiryAccessService _access;
    private readonly PermissionsService _permissions;
    private readonly ILogger<InquiryController> _logger;

    public InquiryController(
        IDashboardGenerator generator, ConversationStore conversations, InquiryAccessService access,
        PermissionsService permissions, ILogger<InquiryController> logger)
    {
        _generator = generator;
        _conversations = conversations;
        _access = access;
        _permissions = permissions;
        _logger = logger;
    }

    /// <summary>Asks a question inside a saved conversation — creating one implicitly on its
    /// first message ("محادثة جديدة" on the frontend is just omitting conversationId).</summary>
    [HttpPost]
    public async Task<ActionResult<InquiryAskApiResponse>> Post([FromBody] InquiryAskRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new InquiryAskApiResponse { Error = "message is required." });

        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        // Narrowed server-side against this user's own permissions — same rule as /api/chat.
        var effectiveSources = PermissionsService.GetEffectiveSelection(user, request.Sources);

        InquiryConversationEntry? existing = null;
        var maskedTurns = new List<ConversationTurn>();
        if (!string.IsNullOrWhiteSpace(request.ConversationId))
        {
            existing = await _conversations.GetByIdAsync(user.Id, request.ConversationId, ct);
            if (existing is null)
                return NotFound(new InquiryAskApiResponse { Error = "المحادثة غير موجودة." });

            // Continuing a saved conversation always runs under the user's *current*
            // permissions — never whatever they had when it started (see InquiryAccessService).
            var rawTurns = ConversationStore.DeserializeTurns(existing.TranscriptJson);
            maskedTurns = await _access.MaskInaccessibleBlocksAsync(user, rawTurns, effectiveSources, ct);
        }

        var recentTurns = maskedTurns.TakeLast(RecentTurnsVerbatimCount).ToList();
        // Never summarized away, and never re-derived from the *raw* transcript — a block
        // this user can no longer reach must not leak into the model's own context either.
        var ledger = maskedTurns
            .Where(t => t.Blocks is not null)
            .SelectMany(t => t.Blocks!)
            .Where(b => !b.Masked && b.Kind == InquiryBlockKinds.Data)
            .ToList();

        InquiryResponse answer;
        try
        {
            answer = await _generator.GenerateInquiryAsync(
                request.Message.Trim(), recentTurns, existing?.Summary, ledger, effectiveSources, user, request.Lang, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Inquiry request failed");
            return StatusCode(500, new InquiryAskApiResponse { Error = ex.Message });
        }

        var now = DateTime.UtcNow;
        var userTurn = new ConversationTurn { Role = ConversationRoles.User, Text = request.Message.Trim(), CreatedAt = now };
        var botTurn = new ConversationTurn { Role = ConversationRoles.Bot, Blocks = answer.Blocks, FollowUps = answer.FollowUps, CreatedAt = now };

        if (existing is null)
        {
            var title = MakeTitle(request.Message);
            var created = await _conversations.CreateAsync(user.Id, title, new[] { userTurn, botTurn }, answer.Summary, ct);
            return Ok(new InquiryAskApiResponse { ConversationId = created.Id, Title = created.Title, Turn = botTurn });
        }

        await _conversations.AppendTurnsAsync(user.Id, existing.Id, new[] { userTurn, botTurn }, answer.Summary, ct);
        return Ok(new InquiryAskApiResponse { ConversationId = existing.Id, Turn = botTurn });
    }

    /// <summary>The saved-conversations list — newest first.</summary>
    [HttpGet("conversations")]
    public async Task<ActionResult<List<ConversationSummaryDto>>> ListConversations(CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();

        var entries = await _conversations.ListAsync(user.Id, ct: ct);
        return Ok(entries.Select(e => new ConversationSummaryDto { Id = e.Id, Title = e.Title, UpdatedAt = e.UpdatedAt }).ToList());
    }

    /// <summary>Reopens one saved conversation — every "data" block whose source this user can
    /// no longer access is masked before this ever leaves the server (see
    /// InquiryAccessService); masked blocks cannot be converted (see Convert below).</summary>
    [HttpGet("conversations/{id}")]
    public async Task<ActionResult<ConversationDetailDto>> GetConversation(string id, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();

        var entry = await _conversations.GetByIdAsync(user.Id, id, ct);
        if (entry is null) return NotFound();

        var rawTurns = ConversationStore.DeserializeTurns(entry.TranscriptJson);
        var masked = await _access.MaskInaccessibleBlocksAsync(user, rawTurns, null, ct);
        return Ok(new ConversationDetailDto { Id = entry.Id, Title = entry.Title, Turns = masked });
    }

    [HttpDelete("conversations/{id}")]
    public async Task<IActionResult> DeleteConversation(string id, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();

        var deleted = await _conversations.DeleteAsync(user.Id, id, ct);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>"🔄 حوّله لداشبورد" — re-reads the target block from storage (never trusts the
    /// client's own copy) and re-checks the requesting user's current permission on it before
    /// converting, so a masked block can never be converted regardless of what the client
    /// sends.</summary>
    [HttpPost("convert")]
    public async Task<ActionResult<ChatResponse>> Convert([FromBody] ConvertInquiryBlockRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ConversationId))
            return BadRequest(new ChatResponse { Error = "conversationId is required." });

        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var effectiveSources = PermissionsService.GetEffectiveSelection(user, request.Sources);

        var entry = await _conversations.GetByIdAsync(user.Id, request.ConversationId, ct);
        if (entry is null) return NotFound(new ChatResponse { Error = "المحادثة غير موجودة." });

        var turns = ConversationStore.DeserializeTurns(entry.TranscriptJson);
        var blocks = request.TurnIndex >= 0 && request.TurnIndex < turns.Count ? turns[request.TurnIndex].Blocks : null;
        var block = blocks is not null && request.BlockIndex >= 0 && request.BlockIndex < blocks.Count
            ? blocks[request.BlockIndex] : null;
        if (block is null)
            return BadRequest(new ChatResponse { Error = "عنصر غير موجود." });
        if (block.Kind != InquiryBlockKinds.Data)
            return BadRequest(new ChatResponse { Error = "هذا الجزء من خارج البيانات — لا يحمل بيانات مؤسسية يمكن تحويلها." });

        var deniedReason = await _access.CheckBlockAccessAsync(user, block, effectiveSources, ct);
        if (deniedReason is not null)
            return BadRequest(new ChatResponse { Error = "تعذّر التحويل: " + deniedReason });

        try
        {
            var dashboard = await _generator.GenerateDashboardFromInquiryAsync(
                block, request.CurrentDashboard, effectiveSources, user, request.Lang, ct);
            return Ok(new ChatResponse { Dashboard = dashboard });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Inquiry-to-dashboard conversion failed");
            return StatusCode(500, new ChatResponse { Error = ex.Message });
        }
    }

    /// <summary>A short lightweight title straight from the first question — no separate model
    /// call (the spec explicitly allows this fallback), trimmed at a word boundary.</summary>
    private static string MakeTitle(string firstMessage)
    {
        const int maxLen = 60;
        var trimmed = firstMessage.Trim();
        if (trimmed.Length <= maxLen) return trimmed;
        var cut = trimmed[..maxLen];
        var lastSpace = cut.LastIndexOf(' ');
        if (lastSpace > 20) cut = cut[..lastSpace];
        return cut + "…";
    }
}
