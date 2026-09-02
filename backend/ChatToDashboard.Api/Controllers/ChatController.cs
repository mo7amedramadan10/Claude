using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Models;
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
    private readonly ILogger<ChatController> _logger;

    public ChatController(IDashboardGenerator generator, ILogger<ChatController> logger)
    {
        _generator = generator;
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

        try
        {
            var dashboard = await _generator.GenerateDashboardAsync(
                request.Message.Trim(), request.History, request.Sources, request.Image, ct);
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
