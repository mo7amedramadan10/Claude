using ChatToDashboard.Api.Claude;
using ChatToDashboard.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly ClaudeClient _claude;
    private readonly ILogger<ChatController> _logger;

    public ChatController(ClaudeClient claude, ILogger<ChatController> logger)
    {
        _claude = claude;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Post([FromBody] ChatRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new ChatResponse { Error = "message is required." });

        try
        {
            var dashboard = await _claude.GenerateDashboardAsync(request.Message.Trim(), request.History, ct);
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
