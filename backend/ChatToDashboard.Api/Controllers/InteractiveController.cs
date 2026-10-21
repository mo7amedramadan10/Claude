using System.Security.Claims;
using ChatToDashboard.Api.Interactive;
using ChatToDashboard.Api.Projects;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>
/// INTERACTIVE-HANDOFF.md §1.3/§7's dataEndpoint — the generic real-data path every
/// "interactive" (RenderKind) dashboard uses, old and new alike: GET .../data returns
/// either the engine's real payload (whatever shape its own mount function expects) or an
/// explicit "not linked yet" result the frontend renders as the standard empty state. See
/// Interactive/InteractiveDataModels.cs for the provider contract.
///
/// The doc's own route shape is "/api/projects/{projectId}/interactive/{templateId}/data",
/// but this app has no frontend concept of an explicit, navigable projectId for the
/// signed-in user's own workspace — every other project-scoped endpoint (see
/// HistoryController.CurrentProjectIdAsync) resolves "the caller's current project" purely
/// server-side from their claims, the same way this controller does. Matching that existing
/// convention (dropping {projectId} from the URL) takes precedence over the doc's literal
/// path here, per this task's own "reference doc wins on behavior, but adapt to how the
/// real app already works where the two genuinely conflict" rule.
/// </summary>
[ApiController]
[Route("api/interactive")]
public class InteractiveController : ControllerBase
{
    private readonly InteractiveDataRegistry _registry;
    private readonly ProjectStore _projects;

    public InteractiveController(InteractiveDataRegistry registry, ProjectStore projects)
    {
        _registry = registry;
        _projects = projects;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private bool IsAdmin => User.IsInRole(UserRoles.Admin);

    private Task<string?> CurrentProjectIdAsync(CancellationToken ct) =>
        _projects.ResolveCurrentProjectIdAsync(User.FindFirstValue("OrganizationId"), UserId, IsAdmin, ct);

    [HttpGet("{templateId}/data")]
    public async Task<IActionResult> GetData(string templateId, CancellationToken ct)
    {
        var provider = _registry.Find(templateId);
        if (provider is null) return NotFound(new { error = $"لا يوجد نموذج تفاعلي مسجّل باسم \"{templateId}\"." });

        var projectId = await CurrentProjectIdAsync(ct);
        if (projectId is null) return BadRequest(new { error = "لا يوجد مشروع حالي لهذا المستخدم." });

        var result = await provider.BuildAsync(projectId, ct);
        return Ok(new
        {
            linked = result.Linked,
            data = result.Linked ? result.Data : null,
            missingSources = result.MissingSources ?? Array.Empty<string>(),
        });
    }
}
