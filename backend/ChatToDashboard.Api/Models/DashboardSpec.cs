using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatToDashboard.Api.Models;

/// <summary>
/// The structured dashboard JSON Claude must return. Deserialized and validated
/// before anything reaches the frontend.
/// </summary>
public class DashboardSpec
{
    private static readonly HashSet<string> AllowedWidgetTypes =
        new(StringComparer.OrdinalIgnoreCase) { "kpi", "bar", "line", "pie", "table" };
    private static readonly HashSet<string> AllowedFilterTypes =
        new(StringComparer.OrdinalIgnoreCase) { "single_select", "multi_select", "date_range", "numeric_range" };

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("widgets")]
    public List<DashboardWidget> Widgets { get; set; } = new();

    /// <summary>
    /// Optional dashboard-level filters the agent identified as useful, each backed by
    /// values it actually queried (never invented). Absent/empty is valid — not every
    /// dashboard needs one, and older saved dashboards have no such field at all.
    /// </summary>
    [JsonPropertyName("filters")]
    public List<DashboardFilter> Filters { get; set; } = new();

    /// <summary>Returns a list of validation problems; empty means the spec is valid.</summary>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(Summary))
            errors.Add("\"summary\" is required and must be a non-empty string.");
        // An empty array is valid: it is how the agent answers when the question needs a
        // source the user has switched off, and the summary explains what to enable.
        if (Widgets is null)
            errors.Add("\"widgets\" must be an array.");
        else
        {
            for (var i = 0; i < Widgets.Count; i++)
            {
                var w = Widgets[i];
                if (w is null) { errors.Add($"widgets[{i}] is null."); continue; }
                if (string.IsNullOrWhiteSpace(w.Type) || !AllowedWidgetTypes.Contains(w.Type))
                    errors.Add($"widgets[{i}].type must be one of: kpi, bar, line, pie, table (got \"{w.Type}\").");
                if (string.IsNullOrWhiteSpace(w.Title))
                    errors.Add($"widgets[{i}].title is required.");
                if (w.Data.ValueKind != JsonValueKind.Array)
                    errors.Add($"widgets[{i}].data must be a JSON array.");
                if (string.IsNullOrWhiteSpace(w.Source))
                    errors.Add($"widgets[{i}].source is required: two sentences — where the data came " +
                               "from, then how it was calculated.");
            }
        }

        if (Filters is not null)
        {
            for (var i = 0; i < Filters.Count; i++)
            {
                var f = Filters[i];
                if (f is null) { errors.Add($"filters[{i}] is null."); continue; }
                if (string.IsNullOrWhiteSpace(f.Field))
                    errors.Add($"filters[{i}].field is required.");
                if (string.IsNullOrWhiteSpace(f.Table))
                    errors.Add($"filters[{i}].table is required.");
                if (string.IsNullOrWhiteSpace(f.Type) || !AllowedFilterTypes.Contains(f.Type))
                    errors.Add($"filters[{i}].type must be one of: single_select, multi_select, date_range, numeric_range (got \"{f.Type}\").");
                var needsOptions = string.Equals(f.Type, "single_select", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(f.Type, "multi_select", StringComparison.OrdinalIgnoreCase);
                if (needsOptions && (f.Options is null || f.Options.Count == 0))
                    errors.Add($"filters[{i}].options must be a non-empty array for type \"{f.Type}\" " +
                               "(values actually queried, never invented).");
            }
        }
        return errors;
    }
}

public class DashboardFilter
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    /// <summary>The real column this filter narrows — validated against the schema before use.</summary>
    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;

    /// <summary>The real table <see cref="Field"/> belongs to — which widgets it can affect.</summary>
    [JsonPropertyName("table")]
    public string? Table { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = "single_select"; // single_select | multi_select | date_range | numeric_range

    /// <summary>Selectable values — must come from an actual DISTINCT query, never invented.</summary>
    [JsonPropertyName("options")]
    public List<FilterOption> Options { get; set; } = new();

    [JsonPropertyName("appliesTo")]
    public string AppliesTo { get; set; } = "dashboard";
}

public class FilterOption
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

public class DashboardWidget
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public JsonElement Data { get; set; }

    [JsonPropertyName("xKey")]
    public string? XKey { get; set; }

    [JsonPropertyName("yKey")]
    public string? YKey { get; set; }

    /// <summary>
    /// Provenance shown behind the widget's ⓘ button: exactly two sentences — where the
    /// data came from, then how it was calculated.
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }
}
