using System.Text.Json.Serialization;

namespace ChatToDashboard.Api.Templates;

/// <summary>The kinds of AI-facing templates the platform-owner's "مكتبة النماذج" screen
/// edits — see TemplateStore remarks for how all four share one table.</summary>
public static class TemplateKinds
{
    public const string Widget = "widget";
    public const string Dashboard = "dashboard";
    /// <summary>One of the 6 measure-type prompt templates (percentage/time/money/score/
    /// count/other) behind the KPI library's 1,899 rows — see BuiltinTemplates.KpiMeasureTypePrompts.</summary>
    public const string KpiMeasureType = "kpi_mtype";
    /// <summary>A single KPI-library row's own override, keyed by its stable index into
    /// kpi-library.js's `rows` array (that file is static/baked, so the index is a stable id).</summary>
    public const string Kpi = "kpi";
    /// <summary>Singleton (Key is always "general") — free text prepended to every
    /// template-originated prompt, same as every other kind's override row.</summary>
    public const string Rules = "rules";
}

public static class TemplateStatuses
{
    public const string Draft = "draft";
    public const string Published = "published";
    public const string Stopped = "stopped";
}

/// <summary>One row of the shared PromptTemplateOverrides table — present only where the
/// platform-owner has actually touched a built-in (an override) or added a new item (a
/// custom one); absent means "use the built-in default, published". See TemplateStore.</summary>
public class TemplateOverride
{
    public string Id { get; set; } = "";
    public string Kind { get; set; } = "";
    public string Key { get; set; } = "";
    public bool IsCustom { get; set; }
    public string Status { get; set; } = TemplateStatuses.Published;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Category { get; set; }
    /// <summary>JSON string[] — widget/dashboard's "تعتمد على" source tags (display only).</summary>
    public string? SourcesJson { get; set; }
    /// <summary>JSON array of {type,title} — a dashboard template's 10 sub-widgets.</summary>
    public string? WidgetsJson { get; set; }
    public string? PromptText { get; set; }
    public int Version { get; set; }
    public string? UpdatedByUserId { get; set; }
    public string? UpdatedByName { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>What a ChatRequest sends for a template-originated question — never the prompt
/// text itself (see ChatController.Post / TemplatePromptService), only enough to look the
/// template up server-side plus, for a KPI row, the handful of already-public display fields
/// (name/category/measure-type label) kpi-library.js shows in the UI anyway, which the server
/// has no copy of since that 1,899-row catalog stays a static client asset (see kpi-library.js's
/// own remarks) — these are substituted as literal data inside the server's own fixed template
/// sentence, never treated as instructions themselves.</summary>
public class TemplateRef
{
    [JsonPropertyName("kind")] public string Kind { get; set; } = "";
    [JsonPropertyName("key")] public string Key { get; set; } = "";
    [JsonPropertyName("kpiName")] public string? KpiName { get; set; }
    [JsonPropertyName("kpiNameEn")] public string? KpiNameEn { get; set; }
    [JsonPropertyName("kpiCategory")] public string? KpiCategory { get; set; }
    [JsonPropertyName("kpiMeasureType")] public int? KpiMeasureType { get; set; }
    [JsonPropertyName("kpiMeasureTypeLabel")] public string? KpiMeasureTypeLabel { get; set; }
}
