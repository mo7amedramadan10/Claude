using System.Text.Json.Serialization;
using ChatToDashboard.Api.Sources;

namespace ChatToDashboard.Api.Models;

public class ChatRequest
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The dashboard currently shown to the user — its last summary and full widgets array
    /// (each widget's "source" included), sent only when this question should continue or
    /// refine it. Null for the first question of a session, or right after the "🆕 ابدأ لوحة
    /// جديدة" button is clicked — the frontend alone decides which, by including or omitting
    /// this field; the model never has to infer "new topic vs. follow-up" from wording alone.
    /// </summary>
    [JsonPropertyName("currentDashboard")]
    public DashboardStateInput? CurrentDashboard { get; set; }

    /// <summary>Which sources the user has enabled; omitted means "everything".</summary>
    [JsonPropertyName("sources")]
    public SourceSelection? Sources { get; set; }

    /// <summary>
    /// Optional reference image (a data URL, e.g. "data:image/jpeg;base64,...") — a screenshot
    /// or mockup of a dashboard the model should analyze and rebuild using real data.
    /// </summary>
    [JsonPropertyName("image")]
    public string? Image { get; set; }
}

/// <summary>The dashboard state a continuation question is framed against — see <see cref="ChatRequest.CurrentDashboard"/>.</summary>
public class DashboardStateInput
{
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("widgets")]
    public List<DashboardWidget> Widgets { get; set; } = new();
}

public class ChatResponse
{
    [JsonPropertyName("dashboard")]
    public DashboardSpec? Dashboard { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}
