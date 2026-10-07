using System.Text.Json;
using System.Text.RegularExpressions;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Organizations;
using ChatToDashboard.Api.Projects;
using ChatToDashboard.Api.SchemaLibrary;
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
    private readonly SchemaMatchingService _schemaMatching;

    public TemplatePromptService(
        TemplateStore templates, OrganizationStore organizations, ProjectStore projects, AnalyticsTools analytics,
        SchemaMatchingService schemaMatching)
    {
        _templates = templates;
        _organizations = organizations;
        _projects = projects;
        _analytics = analytics;
        _schemaMatching = schemaMatching;
    }

    /// <summary>Resolves the template into its final prompt, or null with <paramref
    /// name="error"/> set when the kind/key is unknown, deleted, or not currently published.
    /// <paramref name="RequiredWidgets"/> is non-null only for a Dashboard template with a
    /// declared widget list — see ChatController.Post, which reconciles the model's actual
    /// response against it so a dropped widget becomes an explicit "no data" placeholder
    /// instead of silently disappearing.</summary>
    public async Task<(string? Prompt, string? Error, IReadOnlyList<BuiltinDashboardWidgetSpec>? RequiredWidgets)> ResolveAsync(
        TemplateRef templateRef, AppUser user, SourceSelection effectiveSources, CancellationToken ct = default)
    {
        // Fetched once here (rather than separately inside BuildCommonVariablesAsync, as before
        // this feature) so EnabledIntegrations is available for schema matching below too,
        // without describing sources against the DB/connectors twice per request.
        var sourceContext = await _analytics.DescribeSourcesAsync(effectiveSources, ct);

        string body;
        IReadOnlyList<BuiltinDashboardWidgetSpec>? requiredWidgets = null;
        switch (templateRef.Kind)
        {
            case TemplateKinds.Widget:
            {
                var resolved = await ResolveWidgetAsync(templateRef.Key, ct);
                if (resolved.Error is not null) return (null, resolved.Error, null);
                body = resolved.Prompt!;
                break;
            }
            case TemplateKinds.Dashboard:
            {
                var resolved = await ResolveDashboardAsync(templateRef.Key, ct);
                if (resolved.Error is not null) return (null, resolved.Error, null);
                body = resolved.Prompt!;
                requiredWidgets = resolved.Widgets;
                break;
            }
            case TemplateKinds.Kpi:
            {
                var resolved = await ResolveKpiAsync(templateRef, ct);
                if (resolved.Error is not null) return (null, resolved.Error, null);
                body = resolved.Prompt!;
                // A single KPI build is treated as a one-item required-widget list too — same
                // "must appear, real or an honest noData placeholder" guarantee BuildRequiredWidgetsBlock
                // already gives a dashboard template, extended here to the KPI-library "add" path for
                // the first time, plus whatever Table/Sql draft this row carries feeds schema matching
                // below exactly like a dashboard widget's own.
                if (resolved.Widget is not null) requiredWidgets = new[] { resolved.Widget };
                break;
            }
            default:
                return (null, "نوع النموذج غير معروف.", null);
        }

        var vars = await BuildCommonVariablesAsync(user, sourceContext, ct);
        body = Substitute(body, vars);

        var rules = await _templates.GetAsync(TemplateKinds.Rules, "general", ct);
        if (!string.IsNullOrWhiteSpace(rules?.PromptText))
            body = $"{Substitute(rules.PromptText, vars)}\n\n{body}";

        // Appended last (after the general rules block) so it reads as the final, most
        // specific instruction for this particular request — a template's own declared
        // widgets are a hard requirement this build must account for, not a stylistic
        // preference like the rest of the prompt above it.
        if (requiredWidgets is { Count: > 0 })
        {
            body = $"{body}\n\n{BuildRequiredWidgetsBlock(requiredWidgets)}";

            // The field/table alias library's deterministic pre-match (see SchemaMatchingService)
            // — only ever adds a hint on top of the instruction above, never replaces it: an
            // unmatched or wrongly-matched widget still falls back to the model exploring the
            // real schema itself exactly as before this feature existed.
            if (sourceContext.EnabledIntegrations.Count > 0)
            {
                var matches = await _schemaMatching.MatchAsync(requiredWidgets, sourceContext.EnabledIntegrations, ct);
                if (SchemaMatchingService.BuildPromptHintBlock(matches) is { } hint)
                    body = $"{body}\n\n{hint}";
            }
        }

        return (body, null, requiredWidgets);
    }

    /// <summary>The "every declared widget must appear, real or explicitly flagged" rule a
    /// template-originated dashboard build gets on top of the normal system prompt — see
    /// <see cref="ResolveAsync"/>. Kept separate from the per-template PromptText (which stays
    /// focused on what the dashboard is *about*) since this applies identically to every
    /// dashboard template regardless of who wrote its prompt.</summary>
    private static string BuildRequiredWidgetsBlock(IReadOnlyList<BuiltinDashboardWidgetSpec> widgets)
    {
        // Page is purely a display/tab grouping the frontend derives on its own by matching the
        // model's literal titles back against this same list (see ChatController.Post) — the
        // model never needs to echo it. It's shown here only so a multi-page template's widgets
        // keep whatever "theme" context their page name used to carry when it was still baked
        // into the title string as a prefix (e.g. "المحفظة — ..."), now that the title itself is
        // the bare original text.
        var hasPages = widgets.Any(w => w.Page is not null);
        string list;
        if (hasPages)
        {
            var n = 0;
            list = string.Join("\n\n", widgets
                .GroupBy(w => w.Page ?? "")
                .Select(g => $"صفحة «{g.Key}»:\n" + string.Join("\n",
                    g.Select(w => $"{++n}) {w.Title} (نوع مقترح: {w.Type})"))));
        }
        else
        {
            list = string.Join("\n", widgets.Select((w, i) => $"{i + 1}) {w.Title} (نوع مقترح: {w.Type})"));
        }
        var pagesNote = hasPages
            ? "\nمقسّمة هنا إلى صفحات/تبويبات لتوضيح سياق كل عنصر فقط — ده تبويب عرض في الواجهة، ومالوش تأثير على عنوان العنصر نفسه أو على شكل الرد.\n"
            : "";
        return $"""
            عناصر هذه اللوحة المطلوبة — إلزامي
            القائمة دي كل عناصر اللوحة المطلوبة، بنفس العدد والعناوين الحرفية بالضبط:{pagesNote}
            {list}
            لازم يظهر كل عنصر من دول في ردك، بنفس العنوان الحرفي، من غير أي حذف أو دمج أو
            إعادة صياغة — حتى لو بعضهم صعب تجيب له بيانات حقيقية. لكل عنصر، حالتين بس:
            - لقيت مصدر بيانات حقيقي مطابق فعليًا (بعد ما تنادي list_files/query_data وتتأكد):
              ابنيه عادي زي أي عنصر، بكل قواعد الدقة والمصدر المعتادة.
              - مفيش مصدر بيانات مطابق متاح لك دلوقتي (المصدر غير مفعّل، أو غير مربوط، أو
              مفيش جدول/عمود يطابق المطلوب أصلًا): أرجعه بنفس العنوان الحرفي بالظبط، وحط
              "noData": true، و"missingReason" بجملة عربية قصيرة وصادقة توضح السبب (مثلاً
              "يحتاج نظام المبيعات، وهو غير مفعّل حاليًا" أو "لا يوجد عمود يحدد هذا المقياس في
              المصادر المتاحة") — واترك "data" مصفوفة فاضية وممنوع تحط "source". النوع
              المقترح جنب كل عنصر فوق مجرد توجيه؛ لو شكل تاني من المخطط التسعة (kpi/bar/
              line/pie/table/progress-table/trend-matrix/status-bar/radial-gauge/linear-gauge)
              أنسب للبيانات اللي لقيتها فعلًا، استخدمه بدل المقترح.
            ممنوع تحذف أي عنصر من القائمة من ردك النهائي لأي سبب — حتى لو كل عناصر اللوحة
            طلعوا noData، كلهم لازم يظهروا. ده استثناء صريح من قاعدة "widgets فاضية لو
            المصدر مقفول/غير مربوط" العادية فوق: هنا بالذات، كل عنصر بيترجم noData فردي
            بعنوانه، مش مصفوفة widgets فاضية بالكامل.
            """;
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

    private async Task<(string? Prompt, string? Error, IReadOnlyList<BuiltinDashboardWidgetSpec>? Widgets)> ResolveDashboardAsync(
        string key, CancellationToken ct)
    {
        var builtin = BuiltinTemplates.FindDashboard(key);
        var over = await _templates.GetAsync(TemplateKinds.Dashboard, key, ct);
        if (builtin is null && over is null) return (null, "هذا النموذج غير موجود.", null);
        if (builtin is null && over is { IsCustom: false }) return (null, "هذا النموذج غير موجود.", null);

        var status = over?.Status ?? TemplateStatuses.Published;
        if (status != TemplateStatuses.Published) return (null, "هذا النموذج غير متاح للمستخدمين حاليًا.", null);

        var prompt = over?.PromptText ?? builtin?.Prompt;
        if (string.IsNullOrWhiteSpace(prompt)) return (null, "هذا النموذج غير مكتمل الإعداد.", null);

        // The override's own WidgetsJson if the platform-owner edited it, else the built-in's —
        // the one authoritative widget list for this item either way. Computed unconditionally
        // (not just when the prompt references {{widgets}}) since ResolveAsync's caller also
        // needs it for the required-widgets instruction block and later reconciliation, even
        // for the original 8 built-ins, whose hand-written prompt text already enumerates its
        // widgets inline and never uses {{widgets}} itself.
        var specs = over?.WidgetsJson is { Length: > 0 } json
            ? JsonSerializer.Deserialize<List<BuiltinDashboardWidgetSpec>>(json, JsonOptions) ?? new()
            : builtin?.Widgets.ToList() ?? new();

        if (prompt.Contains("{{widgets}}"))
        {
            var list = string.Join("\n", specs.Select((w, i) =>
                $"{i + 1}) {w.Title}" + (string.IsNullOrWhiteSpace(w.Prompt) ? "" : $" — {w.Prompt}")));
            prompt = prompt.Replace("{{widgets}}", list);
        }
        return (prompt, null, specs);
    }

    private async Task<(string? Prompt, string? Error, BuiltinDashboardWidgetSpec? Widget)> ResolveKpiAsync(
        TemplateRef templateRef, CancellationToken ct)
    {
        if (!int.TryParse(templateRef.Key, out var rowIndex) || rowIndex < 0)
            return (null, "مؤشر غير صالح.", null);
        var mtype = templateRef.KpiMeasureType ?? -1;
        if (mtype < 0 || mtype > 5) return (null, "نوع قياس غير صالح.", null);
        var name = (templateRef.KpiName ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 200) return (null, "اسم المؤشر غير صالح.", null);

        // Category-off is authoritative and independent of the row's own status (see
        // TemplateKinds.KpiCategory) — checked against the index the client sent, never the
        // free-text category name, so nothing short of the real category id can bypass it.
        if (templateRef.KpiCategoryIndex is { } catIndex)
        {
            var disabledCats = await _templates.GetDisabledKpiCategoriesAsync(ct);
            if (disabledCats.Contains(catIndex)) return (null, "هذا التصنيف موقوف حاليًا.", null);
        }

        var rowOver = await _templates.GetAsync(TemplateKinds.Kpi, rowIndex.ToString(), ct);
        if (rowOver is { Status: not TemplateStatuses.Published }) return (null, "هذا المؤشر غير متاح حاليًا.", null);

        var mtypeOver = await _templates.GetAsync(TemplateKinds.KpiMeasureType, mtype.ToString(), ct);
        if (mtypeOver is { Status: not TemplateStatuses.Published }) return (null, "هذا نوع من المؤشرات غير متاح حاليًا.", null);

        var template = rowOver?.PromptText ?? mtypeOver?.PromptText ?? BuiltinTemplates.KpiMeasureTypePrompts[mtype];

        var vars = new Dictionary<string, string>
        {
            ["kpi_name"] = name,
            ["kpi_name_en"] = Truncate(templateRef.KpiNameEn, 200),
            ["category"] = Truncate(templateRef.KpiCategory, 120),
            ["measure_type"] = Truncate(templateRef.KpiMeasureTypeLabel, 60),
        };

        // Same "both blank means fall back to kpi-library.js's own draft" convention as the
        // admin editor (see TemplatesController.AdminSaveKpiRow) — an admin override on this
        // row always wins, even a deliberately-blanked one.
        var over = rowOver?.WidgetsJson is { Length: > 0 } json
            ? JsonSerializer.Deserialize<KpiQueryOverride>(json, JsonOptions)
            : null;
        var table = over?.Table ?? templateRef.KpiTable;
        var sql = over?.Sql ?? templateRef.KpiSql;
        var widget = string.IsNullOrWhiteSpace(table)
            ? null
            : new BuiltinDashboardWidgetSpec("kpi", name, Table: table, Sql: sql);

        return (Substitute(template, vars), null, widget);
    }

    private async Task<Dictionary<string, string>> BuildCommonVariablesAsync(
        AppUser user, AnalyticsTools.SourceContext ctx, CancellationToken ct)
    {
        var vars = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(user.OrganizationId))
        {
            var org = await _organizations.FindByIdAsync(user.OrganizationId, ct);
            vars["org_name"] = org?.Name ?? "";
        }

        if (!string.IsNullOrWhiteSpace(ctx.ProjectId))
        {
            var project = await _projects.FindByIdAsync(ctx.ProjectId, ct);
            vars["project_name"] = project?.Name ?? "";
        }

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
