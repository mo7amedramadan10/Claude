using ChatToDashboard.Api.Usage;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

// Prompts, tool results and cost data for every user's questions — admin only.
[ApiController]
[Route("api/usage")]
[Authorize(Roles = UserRoles.Admin)]
public class UsageController : ControllerBase
{
    private readonly UsageStore _store;

    public UsageController(UsageStore store) => _store = store;

    /// <summary>Totals plus the most recent requests (without the heavy payloads).</summary>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int limit = 100, CancellationToken ct = default)
    {
        var summary = await _store.SummarizeAsync(ct);
        var records = await _store.ListAsync(Math.Clamp(limit, 1, 500), ct);
        return Ok(new { summary, records });
    }

    /// <summary>One request in full: system prompt, every turn's request/response, and tool results.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOne(string id, CancellationToken ct)
    {
        var record = await _store.GetAsync(id, ct);
        return record is null ? NotFound(new { error = "غير موجود" }) : Ok(record);
    }

    [HttpDelete]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        await _store.ClearAsync(ct);
        return NoContent();
    }
}
