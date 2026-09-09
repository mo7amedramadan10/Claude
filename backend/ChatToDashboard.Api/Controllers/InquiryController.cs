using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Models;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>
/// "الاستفسارات" — a text-only sibling of /api/chat: same tool-use flow (list_files/
/// query_data/forecast_data/search_documents, same source gating via PermissionsService),
/// but the model answers in plain text instead of building a dashboard. Never touches the
/// dashboard-history feature — an Inquiries answer is cheap to re-ask and isn't saved (see
/// ChatController for the analogous dashboard-building endpoint, which is unchanged).
/// </summary>
[ApiController]
[Route("api/inquiry")]
public class InquiryController : ControllerBase
{
    private readonly IDashboardGenerator _generator;
    private readonly PermissionsService _permissions;
    private readonly ILogger<InquiryController> _logger;

    public InquiryController(IDashboardGenerator generator, PermissionsService permissions, ILogger<InquiryController> logger)
    {
        _generator = generator;
        _permissions = permissions;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<InquiryApiResponse>> Post([FromBody] InquiryRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new InquiryApiResponse { Error = "message is required." });

        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        // Narrowed server-side against this user's own permissions — same rule as /api/chat.
        var effectiveSources = PermissionsService.GetEffectiveSelection(user, request.Sources);

        try
        {
            var answer = await _generator.GenerateInquiryAsync(request.Message.Trim(), effectiveSources, user, ct);
            return Ok(new InquiryApiResponse { Answer = answer });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Inquiry request failed");
            return StatusCode(500, new InquiryApiResponse { Error = ex.Message });
        }
    }

    /// <summary>"➕ أضف إلى لوحة المتابعة" — reshapes one already-answered Inquiries response
    /// into a dashboard via a single non-tool-calling call (never re-queries anything), adding
    /// it to request.CurrentDashboard when one is given rather than replacing it.</summary>
    [HttpPost("convert")]
    public async Task<ActionResult<ChatResponse>> Convert([FromBody] ConvertInquiryRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Answer) || string.IsNullOrWhiteSpace(request.Source))
            return BadRequest(new ChatResponse { Error = "answer and source are required." });

        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var effectiveSources = PermissionsService.GetEffectiveSelection(user, request.Sources);

        var inquiry = new InquiryResponse { Answer = request.Answer, Source = request.Source, Data = request.Data };

        try
        {
            var dashboard = await _generator.GenerateDashboardFromInquiryAsync(inquiry, request.CurrentDashboard, effectiveSources, user, ct);
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
}
