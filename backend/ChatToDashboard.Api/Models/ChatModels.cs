using System.Text.Json.Serialization;
using ChatToDashboard.Api.Sources;

namespace ChatToDashboard.Api.Models;

public class ChatRequest
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Prior conversation turns (oldest first), so follow-up questions like
    /// "now break that down by month" have context. Optional.
    /// </summary>
    [JsonPropertyName("history")]
    public List<ChatTurn> History { get; set; } = new();

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

public class ChatTurn
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty; // "user" or "assistant"

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class ChatResponse
{
    [JsonPropertyName("dashboard")]
    public DashboardSpec? Dashboard { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}
