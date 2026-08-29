using System.Text.Json.Serialization;

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
