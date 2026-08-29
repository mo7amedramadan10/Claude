using System.Text.Json.Serialization;

namespace ChatToDashboard.Api.Models;

public class ChatRequest
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

public class ChatResponse
{
    [JsonPropertyName("dashboard")]
    public DashboardSpec? Dashboard { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}
