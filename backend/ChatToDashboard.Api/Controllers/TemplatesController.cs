using System.Text.Json;
using System.Text.Json.Serialization;
using ChatToDashboard.Api.Templates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>Body of PUT api/templates/admin/{kind}/{key} and POST api/templates/admin/{kind}
/// — every field the platform-owner's editor modal can set. Widget/dashboard items use
/// Title/Description/Icon/Category/Sources(+Widgets for dashboards)/PromptText/Status; KPI
/// measure-type and per-row items use only PromptText/Status (their display fields live in
/// the static kpi-library.js, not here — see TemplateRef's remarks).</summary>
public class SaveTemplateRequest
{
    [JsonPropertyName("title")] public string? Title { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("icon")] public string? Icon { get; set; }
    [JsonPropertyName("category")] public string? Category { get; set; }
    [JsonPropertyName("sources")] public List<string>? Sources { get; set; }
    [JsonPropertyName("widgets")] public List<BuiltinDashboardWidgetSpec>? Widgets { get; set; }
    [JsonPropertyName("promptText")] public string? PromptText { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; } = TemplateStatuses.Published;
}

/// <summary>Platform-owner CRUD over every AI-facing template (see TemplateStore/
/// TemplatePromptService) plus the read-only, promptText-free catalog every signed-in user's
/// chat/gallery/KPI-library screens fetch instead of the old hardcoded TEMPLATES/DASH_GALLERY
/// arrays. The split is the point: <see cref="Catalog"/> is the only endpoint an ordinary user
/// can reach, and it never serializes PromptText — the actual instruction text is resolved
/// later, server-side only, from a templateId (see ChatController.Post/TemplatePromptService).</summary>
[ApiController]
[Route("api/templates")]
[Authorize]
public class TemplatesController : ControllerBase
{
    private readonly TemplateStore _templates;

    public TemplatesController(TemplateStore templates) => _templates = templates;

    // ---------- Public (any signed-in user): display data only, never PromptText ----------

    [HttpGet("catalog")]
    public async Task<IActionResult> Catalog(CancellationToken ct)
    {
        var widgetOverrides = (await _templates.ListByKindAsync(TemplateKinds.Widget, ct))
            .ToDictionary(o => o.Key);
        var dashboardOverrides = (await _templates.ListByKindAsync(TemplateKinds.Dashboard, ct))
            .ToDictionary(o => o.Key);

        var widgets = new List<object>();
        foreach (var b in BuiltinTemplates.Widgets)
        {
            var o = widgetOverrides.GetValueOrDefault(b.Id);
            if ((o?.Status ?? TemplateStatuses.Published) != TemplateStatuses.Published) continue;
            widgets.Add(new
            {
                id = b.Id, cat = o?.Category ?? b.Category, icon = o?.Icon ?? b.Icon,
                title = o?.Title ?? b.Title, desc = o?.Description ?? b.Description,
            });
        }
        foreach (var o in widgetOverrides.Values.Where(o => o.IsCustom))
        {
            if (o.Status != TemplateStatuses.Published) continue;
            widgets.Add(new
            {
                id = o.Key, cat = o.Category ?? "kpi", icon = o.Icon ?? "spark",
                title = o.Title ?? "", desc = o.Description ?? "",
            });
        }

        var dashboards = new List<object>();
        foreach (var b in BuiltinTemplates.Dashboards)
        {
            var o = dashboardOverrides.GetValueOrDefault(b.Id);
            if ((o?.Status ?? TemplateStatuses.Published) != TemplateStatuses.Published) continue;
            dashboards.Add(new
            {
                id = b.Id, layout = b.Layout, cat = o?.Category ?? b.Category,
                name = o?.Title ?? b.Name, popular = b.Popular, desc = o?.Description ?? b.Description,
                sources = DeserializeSources(o?.SourcesJson) ?? b.Sources.ToList(),
                widgets = DeserializeWidgets(o?.WidgetsJson) ?? b.Widgets.Select(w => (object)new { type = w.Type, title = w.Title }).ToList(),
            });
        }
        foreach (var o in dashboardOverrides.Values.Where(o => o.IsCustom))
        {
            if (o.Status != TemplateStatuses.Published) continue;
            dashboards.Add(new
            {
                id = o.Key, layout = "a", cat = o.Category ?? "sales",
                name = o.Title ?? "", popular = false, desc = o.Description ?? "",
                sources = DeserializeSources(o.SourcesJson) ?? new List<string>(),
                widgets = DeserializeWidgets(o.WidgetsJson) ?? Enumerable.Empty<object>(),
            });
        }

        var kpiStatuses = await _templates.GetKpiRowStatusesAsync(ct);
        var kpiOverrides = kpiStatuses
            .Where(kv => kv.Value != TemplateStatuses.Published)
            .ToDictionary(kv => kv.Key.ToString(), kv => kv.Value);

        return Ok(new { widgets, dashboards, kpiOverrides });
    }

    // ---------- Admin: widget / dashboard templates ----------

    [HttpGet("admin/{kind}")]
    [Authorize(Policy = "PlatformOwner")]
    public async Task<IActionResult> AdminList(string kind, CancellationToken ct)
    {
        if (kind is not (TemplateKinds.Widget or TemplateKinds.Dashboard)) return NotFound();
        var overrides = (await _templates.ListByKindAsync(kind, ct)).ToDictionary(o => o.Key);
        var items = new List<object>();

        if (kind == TemplateKinds.Widget)
        {
            foreach (var b in BuiltinTemplates.Widgets)
                items.Add(ToAdminItem(kind, b.Id, false, overrides.GetValueOrDefault(b.Id),
                    b.Title, b.Description, b.Icon, b.Category, null, null, b.Prompt));
        }
        else
        {
            foreach (var b in BuiltinTemplates.Dashboards)
                items.Add(ToAdminItem(kind, b.Id, false, overrides.GetValueOrDefault(b.Id),
                    b.Name, b.Description, null, b.Category, b.Sources.ToList(),
                    b.Widgets.Select(w => new { type = w.Type, title = w.Title }), b.Prompt));
        }
        foreach (var o in overrides.Values.Where(o => o.IsCustom))
            items.Add(ToAdminItem(kind, o.Key, true, o, o.Title ?? "", o.Description ?? "", o.Icon,
                o.Category, DeserializeSources(o.SourcesJson), DeserializeWidgets(o.WidgetsJson), o.PromptText ?? ""));

        return Ok(items);
    }

    [HttpPut("admin/{kind}/{key}")]
    [Authorize(Policy = "PlatformOwner")]
    public async Task<IActionResult> AdminSave(string kind, string key, [FromBody] SaveTemplateRequest body, CancellationToken ct)
    {
        if (kind is not (TemplateKinds.Widget or TemplateKinds.Dashboard)) return NotFound();
        var builtin = kind == TemplateKinds.Widget
            ? (object?)BuiltinTemplates.FindWidget(key) : BuiltinTemplates.FindDashboard(key);
        var existing = await _templates.GetAsync(kind, key, ct);
        if (builtin is null && existing is null) return NotFound(new { error = "هذا النموذج غير موجود." });

        var error = Validate(body, kind);
        if (error is not null) return BadRequest(new { error });

        var row = existing ?? new TemplateOverride { Kind = kind, Key = key, IsCustom = false };
        row.Title = body.Title; row.Description = body.Description; row.Icon = body.Icon; row.Category = body.Category;
        row.SourcesJson = body.Sources is null ? null : JsonSerializer.Serialize(body.Sources, TemplatePromptService.JsonOptions);
        row.WidgetsJson = body.Widgets is null ? null : JsonSerializer.Serialize(body.Widgets, TemplatePromptService.JsonOptions);
        row.PromptText = body.PromptText;
        row.Status = body.Status;
        row.UpdatedByUserId = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        row.UpdatedByName = HttpContext.User.Identity?.Name;
        var saved = await _templates.UpsertAsync(row, ct);
        return Ok(new { version = saved.Version });
    }

    [HttpPost("admin/{kind}")]
    [Authorize(Policy = "PlatformOwner")]
    public async Task<IActionResult> AdminCreate(string kind, [FromBody] SaveTemplateRequest body, CancellationToken ct)
    {
        if (kind is not (TemplateKinds.Widget or TemplateKinds.Dashboard)) return NotFound();
        var error = Validate(body, kind);
        if (error is not null) return BadRequest(new { error });

        var key = $"custom-{Guid.NewGuid():N}";
        var row = new TemplateOverride
        {
            Kind = kind, Key = key, IsCustom = true,
            Title = body.Title, Description = body.Description, Icon = body.Icon, Category = body.Category,
            SourcesJson = body.Sources is null ? null : JsonSerializer.Serialize(body.Sources, TemplatePromptService.JsonOptions),
            WidgetsJson = body.Widgets is null ? null : JsonSerializer.Serialize(body.Widgets, TemplatePromptService.JsonOptions),
            PromptText = body.PromptText,
            Status = body.Status,
            UpdatedByUserId = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            UpdatedByName = HttpContext.User.Identity?.Name,
        };
        await _templates.UpsertAsync(row, ct);
        return Ok(new { id = key });
    }

    [HttpPost("admin/{kind}/{key}/revert")]
    [Authorize(Policy = "PlatformOwner")]
    public async Task<IActionResult> AdminRevert(string kind, string key, CancellationToken ct)
    {
        var existing = await _templates.GetAsync(kind, key, ct);
        if (existing is { IsCustom: true }) return BadRequest(new { error = "عنصر مُضاف بالكامل — استخدم الحذف بدلًا من الرجوع للأصل." });
        await _templates.DeleteAsync(kind, key, ct);
        return Ok(new { ok = true });
    }

    [HttpDelete("admin/{kind}/{key}")]
    [Authorize(Policy = "PlatformOwner")]
    public async Task<IActionResult> AdminDelete(string kind, string key, CancellationToken ct)
    {
        var existing = await _templates.GetAsync(kind, key, ct);
        if (existing is null || !existing.IsCustom)
            return BadRequest(new { error = "لا يمكن حذف نموذج أساسي — استخدم «رجوع للنص الأصلي»." });
        await _templates.DeleteAsync(kind, key, ct);
        return Ok(new { ok = true });
    }

    // ---------- Admin: general rules (singleton) ----------

    [HttpGet("admin/rules")]
    [Authorize(Policy = "PlatformOwner")]
    public async Task<IActionResult> AdminGetRules(CancellationToken ct)
    {
        var row = await _templates.GetAsync(TemplateKinds.Rules, "general", ct);
        return Ok(new { promptText = row?.PromptText ?? "", version = row?.Version ?? 0, updatedByName = row?.UpdatedByName, updatedAt = row?.UpdatedAt });
    }

    [HttpPut("admin/rules")]
    [Authorize(Policy = "PlatformOwner")]
    public async Task<IActionResult> AdminSaveRules([FromBody] SaveTemplateRequest body, CancellationToken ct)
    {
        var unknown = TemplatePromptService.FindUnknownVariables(body.PromptText ?? "", TemplateKinds.Rules);
        if (unknown.Count > 0) return BadRequest(new { error = $"متغيرات غير معروفة: {string.Join(", ", unknown)}" });

        var existing = await _templates.GetAsync(TemplateKinds.Rules, "general", ct);
        var row = existing ?? new TemplateOverride { Kind = TemplateKinds.Rules, Key = "general", IsCustom = false };
        row.PromptText = body.PromptText; row.Status = TemplateStatuses.Published;
        row.UpdatedByUserId = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        row.UpdatedByName = HttpContext.User.Identity?.Name;
        var saved = await _templates.UpsertAsync(row, ct);
        return Ok(new { version = saved.Version });
    }

    // ---------- Admin: KPI library (6 measure-type templates + per-row overrides) ----------

    [HttpGet("admin/kpi-mtypes")]
    [Authorize(Policy = "PlatformOwner")]
    public async Task<IActionResult> AdminListKpiMtypes(CancellationToken ct)
    {
        var overrides = (await _templates.ListByKindAsync(TemplateKinds.KpiMeasureType, ct)).ToDictionary(o => o.Key);
        var items = Enumerable.Range(0, BuiltinTemplates.KpiMeasureTypePrompts.Count).Select(i =>
        {
            var o = overrides.GetValueOrDefault(i.ToString());
            return new
            {
                mtype = i, isOverridden = o is not null, status = o?.Status ?? TemplateStatuses.Published,
                promptText = o?.PromptText ?? BuiltinTemplates.KpiMeasureTypePrompts[i],
                version = o?.Version ?? 0, updatedByName = o?.UpdatedByName, updatedAt = o?.UpdatedAt,
            };
        });
        return Ok(items);
    }

    [HttpPut("admin/kpi-mtypes/{mtype}")]
    [Authorize(Policy = "PlatformOwner")]
    public async Task<IActionResult> AdminSaveKpiMtype(int mtype, [FromBody] SaveTemplateRequest body, CancellationToken ct)
    {
        if (mtype < 0 || mtype >= BuiltinTemplates.KpiMeasureTypePrompts.Count) return NotFound();
        var unknown = TemplatePromptService.FindUnknownVariables(body.PromptText ?? "", TemplateKinds.KpiMeasureType);
        if (unknown.Count > 0) return BadRequest(new { error = $"متغيرات غير معروفة: {string.Join(", ", unknown)}" });

        var existing = await _templates.GetAsync(TemplateKinds.KpiMeasureType, mtype.ToString(), ct);
        var row = existing ?? new TemplateOverride { Kind = TemplateKinds.KpiMeasureType, Key = mtype.ToString(), IsCustom = false };
        row.PromptText = body.PromptText; row.Status = body.Status;
        row.UpdatedByUserId = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        row.UpdatedByName = HttpContext.User.Identity?.Name;
        var saved = await _templates.UpsertAsync(row, ct);
        return Ok(new { version = saved.Version });
    }

    [HttpPost("admin/kpi-mtypes/{mtype}/revert")]
    [Authorize(Policy = "PlatformOwner")]
    public async Task<IActionResult> AdminRevertKpiMtype(int mtype, CancellationToken ct)
    {
        await _templates.DeleteAsync(TemplateKinds.KpiMeasureType, mtype.ToString(), ct);
        return Ok(new { ok = true });
    }

    /// <summary>Sparse — only rows the platform-owner has actually touched. The admin KPI tab
    /// pairs this with the client's own static kpi-library.js (name/category/measure-type —
    /// see TemplateRef's remarks on why that catalog never moved server-side) to show/edit
    /// individual rows without duplicating all 1,899 of them here.</summary>
    [HttpGet("admin/kpi-overrides")]
    [Authorize(Policy = "PlatformOwner")]
    public async Task<IActionResult> AdminListKpiOverrides(CancellationToken ct)
    {
        var rows = await _templates.ListByKindAsync(TemplateKinds.Kpi, ct);
        return Ok(rows.Select(o => new
        {
            index = int.Parse(o.Key), status = o.Status, promptText = o.PromptText,
            version = o.Version, updatedByName = o.UpdatedByName, updatedAt = o.UpdatedAt,
        }));
    }

    [HttpPut("admin/kpi/{index}")]
    [Authorize(Policy = "PlatformOwner")]
    public async Task<IActionResult> AdminSaveKpiRow(int index, [FromBody] SaveTemplateRequest body, CancellationToken ct)
    {
        if (index < 0) return BadRequest(new { error = "مؤشر غير صالح." });
        if (!string.IsNullOrWhiteSpace(body.PromptText))
        {
            var unknown = TemplatePromptService.FindUnknownVariables(body.PromptText, TemplateKinds.Kpi);
            if (unknown.Count > 0) return BadRequest(new { error = $"متغيرات غير معروفة: {string.Join(", ", unknown)}" });
        }

        var existing = await _templates.GetAsync(TemplateKinds.Kpi, index.ToString(), ct);
        var row = existing ?? new TemplateOverride { Kind = TemplateKinds.Kpi, Key = index.ToString(), IsCustom = false };
        // Empty promptText means "use the measure-type's own template" — only Status is then overridden.
        row.PromptText = string.IsNullOrWhiteSpace(body.PromptText) ? null : body.PromptText;
        row.Status = body.Status;
        row.UpdatedByUserId = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        row.UpdatedByName = HttpContext.User.Identity?.Name;
        var saved = await _templates.UpsertAsync(row, ct);
        return Ok(new { version = saved.Version });
    }

    [HttpPost("admin/kpi/{index}/revert")]
    [Authorize(Policy = "PlatformOwner")]
    public async Task<IActionResult> AdminRevertKpiRow(int index, CancellationToken ct)
    {
        await _templates.DeleteAsync(TemplateKinds.Kpi, index.ToString(), ct);
        return Ok(new { ok = true });
    }

    // ---------- helpers ----------

    private static object ToAdminItem(
        string kind, string key, bool isCustom, TemplateOverride? over,
        string title, string description, string? icon, string? category,
        List<string>? sources, object? widgets, string prompt) => new
    {
        kind, key, isCustom, isOverridden = over is not null,
        status = over?.Status ?? TemplateStatuses.Published,
        title = over?.Title ?? title, description = over?.Description ?? description,
        icon = over?.Icon ?? icon, category = over?.Category ?? category,
        sources = DeserializeSources(over?.SourcesJson) ?? sources ?? new List<string>(),
        widgets = DeserializeWidgets(over?.WidgetsJson) ?? widgets,
        promptText = over?.PromptText ?? prompt,
        version = over?.Version ?? 0, updatedByName = over?.UpdatedByName, updatedAt = over?.UpdatedAt,
    };

    private static string? Validate(SaveTemplateRequest body, string kind)
    {
        if (string.IsNullOrWhiteSpace(body.Title)) return "العنوان مطلوب.";
        if (string.IsNullOrWhiteSpace(body.PromptText)) return "النص المرسل لجيم مطلوب.";
        if (body.Status is not (TemplateStatuses.Draft or TemplateStatuses.Published or TemplateStatuses.Stopped))
            return "حالة غير معروفة.";
        var unknown = TemplatePromptService.FindUnknownVariables(body.PromptText, kind);
        if (unknown.Count > 0) return $"متغيرات غير معروفة: {string.Join(", ", unknown)}";
        return null;
    }

    private static List<string>? DeserializeSources(string? json) =>
        json is { Length: > 0 } ? JsonSerializer.Deserialize<List<string>>(json, TemplatePromptService.JsonOptions) : null;

    private static List<object>? DeserializeWidgets(string? json) =>
        json is { Length: > 0 }
            ? JsonSerializer.Deserialize<List<BuiltinDashboardWidgetSpec>>(json, TemplatePromptService.JsonOptions)
                ?.Select(w => (object)new { type = w.Type, title = w.Title }).ToList()
            : null;
}
