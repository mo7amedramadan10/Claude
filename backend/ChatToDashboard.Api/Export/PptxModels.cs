using System.Text.Json.Serialization;

namespace ChatToDashboard.Api.Export;

/// <summary>
/// One slide's worth of widget data, sent by the browser exactly as it already has it:
/// KPI value/label and table rows straight from the widget's `data`, chart types as a
/// PNG snapshot of the SVG already on screen (the browser draws it to a canvas and
/// exports that — no server-side charting).
/// </summary>
public class PptxWidgetInput
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    /// <summary>PNG as a data URL ("data:image/png;base64,...") or raw base64.</summary>
    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("columns")]
    public List<string>? Columns { get; set; }

    [JsonPropertyName("rows")]
    public List<List<string>>? Rows { get; set; }
}

public class PptxExportRequest
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = "";

    [JsonPropertyName("widgets")]
    public List<PptxWidgetInput> Widgets { get; set; } = new();
}
