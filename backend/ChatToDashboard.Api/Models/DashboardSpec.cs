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

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("widgets")]
    public List<DashboardWidget> Widgets { get; set; } = new();

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
        return errors;
    }
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
