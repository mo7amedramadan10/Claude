using System.Security.Claims;
using ChatToDashboard.Api.Integrations;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>
/// External System Integration — admin-only settings CRUD, visual-identity/identity-transport
/// setup helpers, the Publish action, and the two self-contained HTML deliverables. Every write
/// here is an explicit analyst action; nothing in this controller is ever called automatically
/// from editing a dashboard (see PublishService's own remarks).
/// </summary>
[ApiController]
[Route("api/integrations")]
[Authorize(Roles = UserRoles.Admin)]
public class IntegrationsController : ControllerBase
{
    private const long MaxUploadBytes = 15 * 1024 * 1024;

    private readonly IntegrationStore _integrations;
    private readonly VisualIdentityService _visualIdentity;
    private readonly IIntegrationSetupAssistant _assistant;
    private readonly PublishService _publish;
    private readonly IntegrationDeliverables _deliverables;
    private readonly PermissionsService _permissions;

    public IntegrationsController(
        IntegrationStore integrations, VisualIdentityService visualIdentity, IIntegrationSetupAssistant assistant,
        PublishService publish, IntegrationDeliverables deliverables, PermissionsService permissions)
    {
        _integrations = integrations;
        _visualIdentity = visualIdentity;
        _assistant = assistant;
        _publish = publish;
        _deliverables = deliverables;
        _permissions = permissions;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // ---------- CRUD ----------

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok((await _integrations.ListAsync(ct)).Select(ToSummary));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIntegrationRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "اسم التكامل مطلوب." });
        var integration = await _integrations.CreateAsync(request.Name.Trim(), UserId, ct);
        return Ok(ToDetail(integration));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id, CancellationToken ct)
    {
        var integration = await _integrations.GetByIdAsync(id, ct);
        if (integration is null) return NotFound();
        return Ok(ToDetail(integration));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        await _integrations.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPut("{id}/apis")]
    public async Task<IActionResult> UpdateApis(string id, [FromBody] UpdateIntegrationApisRequest request, CancellationToken ct)
    {
        if (await _integrations.GetByIdAsync(id, ct) is null) return NotFound();
        await _integrations.UpdateApisAsync(id, request, ct);
        return NoContent();
    }

    /// <summary>The client's own database shape — captured once so Publish can retarget each
    /// widget's query to their real schema (see PublishService). Never the connection string
    /// itself, which stays local to the generated connector service.</summary>
    [HttpPut("{id}/client-schema")]
    public async Task<IActionResult> UpdateClientSchema(string id, [FromBody] UpdateClientSchemaRequest request, CancellationToken ct)
    {
        if (await _integrations.GetByIdAsync(id, ct) is null) return NotFound();
        if (!ClientDbProviders.All.Contains(request.ClientDbProvider, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { error = "نوع قاعدة بيانات غير مدعوم — لازم يكون SqlServer أو Sqlite." });

        await _integrations.UpdateClientSchemaAsync(id, request.ClientDbProvider, request.ClientSchemaDescription?.Trim() ?? "", ct);
        return NoContent();
    }

    // ---------- visual identity (Part B) ----------

    /// <summary>Path 1 (manual) — also the single "save" endpoint every other path (2, 3a, 3b
    /// vision) funnels into once the analyst has reviewed a proposed token set, possibly
    /// editing it first. Suggest/extract endpoints below never write to the integration
    /// themselves — they only return a proposal.</summary>
    [HttpPut("{id}/visual-identity")]
    public async Task<IActionResult> SetVisualIdentity(string id, [FromBody] ManualVisualIdentityRequest request, CancellationToken ct)
    {
        if (await _integrations.GetByIdAsync(id, ct) is null) return NotFound();
        var (valid, error) = VisualIdentityService.ValidateManual(request.AccentColor, request.SecondaryColor, request.FontFamily);
        if (!valid) return BadRequest(new { error });
        await _integrations.UpdateVisualIdentityAsync(id, request.AccentColor.Trim(), request.SecondaryColor.Trim(), request.FontFamily.Trim(), ct);
        return NoContent();
    }

    /// <summary>Path 2 — AI-assisted suggestion. Returns a proposal only; the analyst applies
    /// it via SetVisualIdentity above (optionally after editing it).</summary>
    [HttpPost("{id}/visual-identity/suggest")]
    public async Task<IActionResult> SuggestVisualIdentity(string id, [FromBody] SuggestVisualIdentityRequest request, CancellationToken ct)
    {
        if (await _integrations.GetByIdAsync(id, ct) is null) return NotFound();
        if (string.IsNullOrWhiteSpace(request.Description) && string.IsNullOrWhiteSpace(request.SeedColor))
            return BadRequest(new { error = "أدخل وصفًا أو لونًا واحدًا على الأقل." });

        var user = await _permissions.GetCurrentUserAsync(User, ct);
        var result = await _visualIdentity.SuggestAsync(request.Description, request.SeedColor, user, ct);
        if (result is null) return UnprocessableEntity(new { error = "تعذّر توليد اقتراح — جرّب وصفًا مختلفًا أو أدخل القيم يدويًا." });
        return Ok(result);
    }

    /// <summary>Path 3a — deterministic color-palette extraction from an uploaded image. No AI
    /// model involved. Returns a proposal only.</summary>
    [HttpPost("{id}/visual-identity/extract-palette")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<IActionResult> ExtractPalette(string id, IFormFile file, CancellationToken ct)
    {
        if (await _integrations.GetByIdAsync(id, ct) is null) return NotFound();
        var bytes = await ReadUploadAsync(file, ct);
        if (bytes is null) return BadRequest(new { error = "ملف غير صالح." });

        VisualIdentityResult? result;
        try
        {
            result = _visualIdentity.ExtractPaletteFromImage(bytes);
        }
        catch (Exception)
        {
            return BadRequest(new { error = "تعذّر قراءة هذه الصورة — تأكد إنها PNG أو JPEG صالحة." });
        }
        if (result is null) return UnprocessableEntity(new { error = "لم يتم العثور على ألوان قابلة للاستخراج في هذه الصورة." });
        return Ok(result);
    }

    /// <summary>Path 3, text-only half — locates an explicit font-name mention already written
    /// in an uploaded PDF's text. Returns a proposal only ({ fontFamily } with the analyst's
    /// current accent/secondary left for them to fill or keep).</summary>
    [HttpPost("{id}/visual-identity/extract-pdf-font")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<IActionResult> ExtractPdfFont(string id, IFormFile file, CancellationToken ct)
    {
        if (await _integrations.GetByIdAsync(id, ct) is null) return NotFound();
        var bytes = await ReadUploadAsync(file, ct);
        if (bytes is null) return BadRequest(new { error = "ملف غير صالح." });

        var user = await _permissions.GetCurrentUserAsync(User, ct);
        var font = await _visualIdentity.ExtractFontFromPdfAsync(bytes, user, ct);
        if (font is null) return UnprocessableEntity(new { error = "لم يتم العثور على اسم خط مذكور صراحة في نص الملف." });
        return Ok(new { fontFamily = font });
    }

    /// <summary>Path 3, vision half — "genuine visual/image understanding" (e.g. a logo with no
    /// text). Refuses with a clear reason (never sends the image) unless
    /// LlmSettingsStore.VisualIdentitySupportsImage is explicitly set — see
    /// VisualIdentityService.ExtractFromImageAsync.</summary>
    [HttpPost("{id}/visual-identity/extract-image")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<IActionResult> ExtractFromImage(string id, IFormFile file, CancellationToken ct)
    {
        if (await _integrations.GetByIdAsync(id, ct) is null) return NotFound();
        var dataUrl = await ReadUploadAsDataUrlAsync(file, ct);
        if (dataUrl is null) return BadRequest(new { error = "ملف غير صالح — ارفع صورة (PNG/JPEG)." });

        var user = await _permissions.GetCurrentUserAsync(User, ct);
        var (result, refusal) = await _visualIdentity.ExtractFromImageAsync(dataUrl, user, ct);
        if (refusal is not null) return UnprocessableEntity(new { error = refusal });
        return Ok(result);
    }

    // ---------- identity transport (Part C) ----------

    /// <summary>Matches a free-text description to one of the three fixed mechanisms — never
    /// writes anything; the analyst reviews the match (and its plain-language Summary) before
    /// calling SetIdentityTransportPending below.</summary>
    [HttpPost("{id}/identity-transport/match")]
    public async Task<IActionResult> MatchIdentityTransport(string id, [FromBody] MatchIdentityTransportRequest request, CancellationToken ct)
    {
        if (await _integrations.GetByIdAsync(id, ct) is null) return NotFound();
        if (string.IsNullOrWhiteSpace(request.Description))
            return BadRequest(new { error = "الوصف مطلوب." });

        var user = await _permissions.GetCurrentUserAsync(User, ct);
        var match = await _assistant.MatchIdentityTransportAsync(request.Description, user, ct);
        return Ok(match);
    }

    /// <summary>Sets (or replaces) the mechanism+parameter pending confirmation — from either
    /// the NL-match above or the analyst typing it directly. Always leaves IdentityConfirmed
    /// false; only ConfirmIdentityTransport below can set it (Part C: "require explicit
    /// confirmation" as its own separate step).</summary>
    [HttpPut("{id}/identity-transport/pending")]
    public async Task<IActionResult> SetIdentityTransportPending(string id, [FromBody] ConfirmIdentityTransportRequest request, CancellationToken ct)
    {
        if (await _integrations.GetByIdAsync(id, ct) is null) return NotFound();
        if (!IdentityTransportMechanisms.All.Contains(request.Mechanism, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { error = "آلية غير معروفة — لازم تكون query أو header أو cookie." });
        if (string.IsNullOrWhiteSpace(request.ParameterName))
            return BadRequest(new { error = "اسم المعامل/الرأس/الكوكي مطلوب." });

        await _integrations.SetIdentityTransportPendingAsync(id, request.Mechanism.ToLowerInvariant(), request.ParameterName.Trim(), ct);
        return NoContent();
    }

    [HttpPost("{id}/identity-transport/confirm")]
    public async Task<IActionResult> ConfirmIdentityTransport(string id, CancellationToken ct)
    {
        var integration = await _integrations.GetByIdAsync(id, ct);
        if (integration is null) return NotFound();
        if (integration.IdentityMechanism is null || integration.IdentityParameterName is null)
            return BadRequest(new { error = "حدّد الآلية واسم المعامل أولاً قبل التأكيد." });

        var user = await _permissions.GetCurrentUserAsync(User, ct);
        await _integrations.ConfirmIdentityTransportAsync(id, user?.Username ?? UserId, ct);
        return NoContent();
    }

    // ---------- publish (Part A) ----------

    [HttpGet("{id}/existing-slot")]
    public async Task<IActionResult> ExistingSlot(string id, [FromQuery] string localHistoryId, CancellationToken ct)
    {
        var slot = await _publish.FindExistingSlotAsync(id, localHistoryId, ct);
        return Ok(slot is null
            ? new { exists = false }
            : new
            {
                exists = true,
                slotId = slot.Id,
                externalDashboardId = slot.ExternalDashboardId,
                title = slot.Title,
                lastPublishedAt = slot.LastPublishedAt,
            });
    }

    [HttpPost("{id}/publish")]
    public async Task<IActionResult> Publish(string id, [FromBody] PublishRequest request, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var result = await _publish.PublishAsync(id, request.DashboardId, request.SlotId, user, ct);
        if (!result.Success) return UnprocessableEntity(new { error = result.Error });
        return Ok(new { slotId = result.Slot!.Id, externalDashboardId = result.Slot.ExternalDashboardId });
    }

    [HttpGet("{id}/dashboards")]
    public async Task<IActionResult> ListPublishedDashboards(string id, CancellationToken ct) =>
        Ok(await _integrations.ListSlotsAsync(id, ct));

    [HttpGet("{id}/publish-log")]
    public async Task<IActionResult> PublishLog(string id, CancellationToken ct) =>
        Ok(await _integrations.ListPublishLogAsync(id, ct: ct));

    // ---------- deliverables (Part D / Part E) ----------

    [HttpGet("{id}/deliverables/viewer")]
    public async Task<IActionResult> DownloadViewer(string id, CancellationToken ct)
    {
        var integration = await _integrations.GetByIdAsync(id, ct);
        if (integration is null) return NotFound();
        var html = _deliverables.BuildViewerHtml(integration);
        return File(System.Text.Encoding.UTF8.GetBytes(html), "text/html", $"{Slugify(integration.Name)}-dashboard-viewer.html");
    }

    [HttpGet("{id}/deliverables/admin")]
    public async Task<IActionResult> DownloadAdmin(string id, CancellationToken ct)
    {
        var integration = await _integrations.GetByIdAsync(id, ct);
        if (integration is null) return NotFound();
        var html = _deliverables.BuildPermissionsAdminHtml(integration);
        return File(System.Text.Encoding.UTF8.GetBytes(html), "text/html", $"{Slugify(integration.Name)}-permissions-admin.html");
    }

    /// <summary>Deliverable 3 — the connector microservice source, zipped, for the client to
    /// build and deploy themselves. See IntegrationDeliverables.BuildConnectorZip.</summary>
    [HttpGet("{id}/deliverables/connector")]
    public async Task<IActionResult> DownloadConnector(string id, CancellationToken ct)
    {
        var integration = await _integrations.GetByIdAsync(id, ct);
        if (integration is null) return NotFound();
        var zip = _deliverables.BuildConnectorZip(integration);
        return File(zip, "application/zip", $"{Slugify(integration.Name)}-connector.zip");
    }

    // ---------- helpers ----------

    private static object ToSummary(ExternalIntegration i) => new
    {
        i.Id,
        i.Name,
        writeApiConfigured = !string.IsNullOrWhiteSpace(i.WriteApiUrl),
        readApiConfigured = !string.IsNullOrWhiteSpace(i.ReadApiBaseUrl),
        identityConfirmed = i.IdentityConfirmed,
        visualIdentityConfigured = !string.IsNullOrWhiteSpace(i.AccentColor),
        clientSchemaConfigured = !string.IsNullOrWhiteSpace(i.ClientSchemaDescription),
        i.CreatedAt,
        i.UpdatedAt,
    };

    /// <summary>Never echoes stored auth credentials — only whether one is set, same
    /// convention as LlmSettingsController's IsConfigured booleans.</summary>
    private static object ToDetail(ExternalIntegration i) => new
    {
        i.Id,
        i.Name,
        writeApiUrl = i.WriteApiUrl,
        writeApiAuthHeader = i.WriteApiAuthHeader,
        writeApiAuthConfigured = !string.IsNullOrWhiteSpace(i.WriteApiAuthValue),
        readApiBaseUrl = i.ReadApiBaseUrl,
        directoryApiUrl = i.DirectoryApiUrl,
        permissionsApiUrl = i.PermissionsApiUrl,
        permissionsApiAuthHeader = i.PermissionsApiAuthHeader,
        permissionsApiAuthConfigured = !string.IsNullOrWhiteSpace(i.PermissionsApiAuthValue),
        identityMechanism = i.IdentityMechanism,
        identityParameterName = i.IdentityParameterName,
        identityConfirmed = i.IdentityConfirmed,
        identityConfirmedAt = i.IdentityConfirmedAt,
        identityConfirmedBy = i.IdentityConfirmedBy,
        accentColor = i.AccentColor,
        secondaryColor = i.SecondaryColor,
        fontFamily = i.FontFamily,
        clientDbProvider = i.ClientDbProvider,
        clientSchemaDescription = i.ClientSchemaDescription,
        i.CreatedBy,
        i.CreatedAt,
        i.UpdatedAt,
    };

    private static async Task<byte[]?> ReadUploadAsync(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0 || file.Length > MaxUploadBytes) return null;
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        return ms.ToArray();
    }

    private static async Task<string?> ReadUploadAsDataUrlAsync(IFormFile? file, CancellationToken ct)
    {
        var bytes = await ReadUploadAsync(file, ct);
        if (bytes is null) return null;
        var mediaType = file!.ContentType is { Length: > 0 } ct2 ? ct2 : "image/png";
        return $"data:{mediaType};base64,{Convert.ToBase64String(bytes)}";
    }

    private static string Slugify(string name)
    {
        var cleaned = new string(name.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_').ToArray());
        return cleaned.Length > 0 ? cleaned : "integration";
    }
}
