using System.Text.Json;
using System.Text.RegularExpressions;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Organizations;
using ChatToDashboard.Api.Projects;
using ChatToDashboard.Api.Sources;
using ChatToDashboard.Api.Users;

namespace ChatToDashboard.Api.Templates;

/// <summary>
/// Turns a <see cref="TemplateRef"/> (kind + key, plus a KPI row's own display fields) into the
/// final prompt text actually sent to the AI — the one place that assembly happens. This is the
/// server-side half of the platform-owner's hard requirement: a template-originated chat request
/// carries only a <see cref="TemplateRef"/>, never prompt text, so nothing the browser sends can
/// change what instruction the model receives for a given template (see ChatController.Post,
/// which calls this instead of trusting ChatRequest.Message whenever Template is set).
/// </summary>
public class TemplatePromptService
{
    private static readonly Regex VariablePattern = new(@"\{\{\s*(\w+)\s*\}\}", RegexOptions.Compiled);

    /// <summary>Used for every manual (outside the MVC pipeline) (de)serialization of
    /// WidgetsJson/SourcesJson in this file and TemplatesController, so a value written with
    /// one casing still deserializes cleanly regardless of which JsonSerializerOptions wrote it.</summary>
    public static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly TemplateStore _templates;
    private readonly OrganizationStore _organizations;
    private readonly ProjectStore _projects;
    private readonly AnalyticsTools _analytics;

    public TemplatePromptService(
        TemplateStore templates, OrganizationStore organizations, ProjectStore projects, AnalyticsTools analytics)
    {
        _templates = templates;
        _organizations = organizations;
        _projects = projects;
        _analytics = analytics;
    }

    /// <summary>Resolves the template into its final prompt, or null with <paramref
    /// name="error"/> set when the kind/key is unknown, deleted, or not currently published.</summary>
    public async Task<(string? Prompt, string? Error)> ResolveAsync(
        TemplateRef templateRef, AppUser user, SourceSelection effectiveSources, CancellationToken ct = default)
    {
        string body;
        switch (templateRef.Kind)
        {
            case TemplateKinds.Widget:
            {
                var resolved = await ResolveWidgetAsync(templateRef.Key, ct);
                if (resolved.Error is not null) return (null, resolved.Error);
                body = resolved.Prompt!;
                break;
            }
            case TemplateKinds.Dashboard:
            {
                var resolved = await ResolveDashboardAsync(templateRef.Key, ct);
                if (resolved.Error is not null) return (null, resolved.Error);
                body = resolved.Prompt!;
                break;
            }
            case TemplateKinds.Kpi:
            {
                var resolved = await ResolveKpiAsync(templateRef, ct);
                if (resolved.Error is not null) return (null, resolved.Error);
                body = resolved.Prompt!;
                break;
            }
            default:
                return (null, "نوع النموذج غير معروف.");
        }

        var vars = await BuildCommonVariablesAsync(user, effectiveSources, ct);
        body = Substitute(body, vars);

        var rules = await _templates.GetAsync(TemplateKinds.Rules, "general", ct);
        if (!string.IsNullOrWhiteSpace(rules?.PromptText))
            body = $"{Substitute(rules.PromptText, vars)}\n\n{body}";

        return (body, null);
    }

    private async Task<(string? Prompt, string? Error)> ResolveWidgetAsync(string key, CancellationToken ct)
    {
        var builtin = BuiltinTemplates.FindWidget(key);
        var over = await _templates.GetAsync(TemplateKinds.Widget, key, ct);
        if (builtin is null && over is null) return (null, "هذا النموذج غير موجود.");
        if (builtin is null && over is { IsCustom: false }) return (null, "هذا النموذج غير موجود.");

        var status = over?.Status ?? TemplateStatuses.Published;
        if (status != TemplateStatuses.Published) return (null, "هذا النموذج غير متاح للمستخدمين حاليًا.");

        var prompt = over?.PromptText ?? builtin?.Prompt;
        if (string.IsNullOrWhiteSpace(prompt)) return (null, "هذا النموذج غير مكتمل الإعداد.");
        return (prompt, null);
    }

    private async Task<(string? Prompt, string? Error)> ResolveDashboardAsync(string key, CancellationToken ct)
    {
        var builtin = BuiltinTemplates.FindDashboard(key);
        var over = await _templates.GetAsync(TemplateKinds.Dashboard, key, ct);
        if (builtin is null && over is null) return (null, "هذا النموذج غير موجود.");
        if (builtin is null && over is { IsCustom: false }) return (null, "هذا النموذج غير موجود.");

        var status = over?.Status ?? TemplateStatuses.Published;
        if (status != TemplateStatuses.Published) return (null, "هذا النموذج غير متاح للمستخدمين حاليًا.");

        var prompt = over?.PromptText ?? builtin?.Prompt;
        if (string.IsNullOrWhiteSpace(prompt)) return (null, "هذا النموذج غير مكتمل الإعداد.");

        // {{widgets}} only matters when the stored prompt actually references it (every
        // built-in's own hand-written sentence already enumerates its 10 widgets inline and
        // never does) — generated from whichever widget list is authoritative for this item:
        // the override's own WidgetsJson if the platform-owner edited it, else the built-in's.
        if (prompt.Contains("{{widgets}}"))
        {
            var specs = over?.WidgetsJson is { Length: > 0 } json
                ? JsonSerializer.Deserialize<List<BuiltinDashboardWidgetSpec>>(json, JsonOptions) ?? new()
                : builtin?.Widgets.ToList() ?? new();
            var list = string.Join("\n", specs.Select((w, i) => $"{i + 1}) {w.Title}"));
            prompt = prompt.Replace("{{widgets}}", list);
        }
        return (prompt, null);
    }

    private async Task<(string? Prompt, string? Error)> ResolveKpiAsync(TemplateRef templateRef, CancellationToken ct)
    {
        if (!int.TryParse(templateRef.Key, out var rowIndex) || rowIndex < 0)
            return (null, "مؤشر غير صالح.");
        var mtype = templateRef.KpiMeasureType ?? -1;
        if (mtype < 0 || mtype > 5) return (null, "نوع قياس غير صالح.");
        var name = (templateRef.KpiName ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 200) return (null, "اسم المؤشر غير صالح.");

        var rowOver = await _templates.GetAsync(TemplateKinds.Kpi, rowIndex.ToString(), ct);
        if (rowOver is { Status: not TemplateStatuses.Published }) return (null, "هذا المؤشر غير متاح حاليًا.");

        var mtypeOver = await _templates.GetAsync(TemplateKinds.KpiMeasureType, mtype.ToString(), ct);
        if (mtypeOver is { Status: not TemplateStatuses.Published }) return (null, "هذا نوع من المؤشرات غير متاح حاليًا.");

        var template = rowOver?.PromptText ?? mtypeOver?.PromptText ?? BuiltinTemplates.KpiMeasureTypePrompts[mtype];

        var vars = new Dictionary<string, string>
        {
            ["kpi_name"] = name,
            ["kpi_name_en"] = Truncate(templateRef.KpiNameEn, 200),
            ["category"] = Truncate(templateRef.KpiCategory, 120),
            ["measure_type"] = Truncate(templateRef.KpiMeasureTypeLabel, 60),
        };
        return (Substitute(template, vars), null);
    }

    private async Task<Dictionary<string, string>> BuildCommonVariablesAsync(
        AppUser user, SourceSelection effectiveSources, CancellationToken ct)
    {
        var vars = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(user.OrganizationId))
        {
            var org = await _organizations.FindByIdAsync(user.OrganizationId, ct);
            vars["org_name"] = org?.Name ?? "";
        }

        if (!string.IsNullOrWhiteSpace(effectiveSources.ProjectId))
        {
            var project = await _projects.FindByIdAsync(effectiveSources.ProjectId, ct);
            vars["project_name"] = project?.Name ?? "";
        }

        var ctx = await _analytics.DescribeSourcesAsync(effectiveSources, ct);
        var sourceNames = ctx.EnabledSystems
            .Concat(ctx.EnabledFiles)
            .Concat(ctx.EnabledIntegrations.Select(i => i.Name))
            .ToList();
        vars["sources"] = sourceNames.Count > 0 ? string.Join("، ", sourceNames) : "غير محددة";

        return vars;
    }

    private static string Substitute(string text, IReadOnlyDictionary<string, string> vars) =>
        VariablePattern.Replace(text, m => vars.TryGetValue(m.Groups[1].Value, out var v) ? v : m.Value);

    private static string Truncate(string? value, int max)
    {
        value = (value ?? "").Trim();
        return value.Length > max ? value[..max] : value;
    }

    /// <summary>The variable keys known to the substitution engine, with which kinds each
    /// applies to — drives the admin editor's insertion chips and its unknown-variable
    /// validation (see TemplatesController's save endpoints).</summary>
    public static readonly IReadOnlyDictionary<string, string[]> KnownVariables = new Dictionary<string, string[]>
    {
        ["project_name"] = new[] { TemplateKinds.Widget, TemplateKinds.Dashboard, TemplateKinds.Kpi, TemplateKinds.KpiMeasureType, TemplateKinds.Rules },
        ["org_name"] = new[] { TemplateKinds.Widget, TemplateKinds.Dashboard, TemplateKinds.Kpi, TemplateKinds.KpiMeasureType, TemplateKinds.Rules },
        ["sources"] = new[] { TemplateKinds.Widget, TemplateKinds.Dashboard, TemplateKinds.Kpi, TemplateKinds.KpiMeasureType, TemplateKinds.Rules },
        ["widgets"] = new[] { TemplateKinds.Dashboard },
        ["kpi_name"] = new[] { TemplateKinds.Kpi, TemplateKinds.KpiMeasureType },
        ["kpi_name_en"] = new[] { TemplateKinds.Kpi, TemplateKinds.KpiMeasureType },
        ["category"] = new[] { TemplateKinds.Kpi, TemplateKinds.KpiMeasureType },
        ["measure_type"] = new[] { TemplateKinds.Kpi, TemplateKinds.KpiMeasureType },
    };

    /// <summary>Every <c>{{...}}</c> placeholder in <paramref name="text"/> that isn't a known
    /// variable for <paramref name="kind"/> — the admin editor must block save while this is
    /// non-empty (see IMPLEMENTATION.md's validation rule, ported server-side too since this is
    /// the authoritative check: an unknown variable would otherwise reach end users verbatim,
    /// e.g. a literal "{{typo}}" in a generated dashboard request).</summary>
    public static IReadOnlyList<string> FindUnknownVariables(string text, string kind) =>
        VariablePattern.Matches(text)
            .Select(m => m.Groups[1].Value)
            .Distinct()
            .Where(name => !KnownVariables.TryGetValue(name, out var kinds) || !kinds.Contains(kind))
            .ToList();
}
