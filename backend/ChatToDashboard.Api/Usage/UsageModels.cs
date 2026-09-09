using System.Text.Json.Serialization;

namespace ChatToDashboard.Api.Usage;

/// <summary>One tool call made during a request, with its full input and result.</summary>
public class UsageToolCall
{
    [JsonPropertyName("turn")] public int Turn { get; set; }
    [JsonPropertyName("tool")] public string Tool { get; set; } = string.Empty;
    [JsonPropertyName("input")] public string Input { get; set; } = string.Empty;
    [JsonPropertyName("result")] public string Result { get; set; } = string.Empty;
    [JsonPropertyName("isError")] public bool IsError { get; set; }
    [JsonPropertyName("durationMs")] public long DurationMs { get; set; }
}

/// <summary>One round-trip to the model inside the tool-use loop.</summary>
public class UsageTurn
{
    [JsonPropertyName("turn")] public int Turn { get; set; }
    [JsonPropertyName("inputTokens")] public int InputTokens { get; set; }
    [JsonPropertyName("outputTokens")] public int OutputTokens { get; set; }
    [JsonPropertyName("cacheReadTokens")] public int CacheReadTokens { get; set; }
    [JsonPropertyName("cacheWriteTokens")] public int CacheWriteTokens { get; set; }
    [JsonPropertyName("stopReason")] public string? StopReason { get; set; }
    [JsonPropertyName("durationMs")] public long DurationMs { get; set; }

    /// <summary>The exact JSON body sent to the provider for this turn.</summary>
    [JsonPropertyName("requestBody")] public string? RequestBody { get; set; }

    /// <summary>The exact JSON body returned by the provider.</summary>
    [JsonPropertyName("responseBody")] public string? ResponseBody { get; set; }
}

/// <summary>Everything about one question: what was sent, what came back, and what it cost.</summary>
public class UsageRecord
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("timestamp")] public DateTime Timestamp { get; set; }
    [JsonPropertyName("provider")] public string Provider { get; set; } = string.Empty;
    [JsonPropertyName("model")] public string Model { get; set; } = string.Empty;
    [JsonPropertyName("question")] public string Question { get; set; } = string.Empty;

    [JsonPropertyName("enabledSources")] public string EnabledSources { get; set; } = string.Empty;
    [JsonPropertyName("turnCount")] public int TurnCount { get; set; }
    [JsonPropertyName("toolCallCount")] public int ToolCallCount { get; set; }

    [JsonPropertyName("inputTokens")] public int InputTokens { get; set; }
    [JsonPropertyName("outputTokens")] public int OutputTokens { get; set; }
    [JsonPropertyName("cacheReadTokens")] public int CacheReadTokens { get; set; }
    [JsonPropertyName("cacheWriteTokens")] public int CacheWriteTokens { get; set; }
    [JsonPropertyName("totalTokens")] public int TotalTokens { get; set; }
    [JsonPropertyName("estimatedCost")] public decimal EstimatedCost { get; set; }

    [JsonPropertyName("durationMs")] public long DurationMs { get; set; }
    [JsonPropertyName("success")] public bool Success { get; set; }
    [JsonPropertyName("error")] public string? Error { get; set; }

    /// <summary>The system prompt sent with the request (identical on every turn).</summary>
    [JsonPropertyName("systemPrompt")] public string? SystemPrompt { get; set; }

    [JsonPropertyName("turns")] public List<UsageTurn> Turns { get; set; } = new();
    [JsonPropertyName("toolCalls")] public List<UsageToolCall> ToolCalls { get; set; } = new();

    /// <summary>The final dashboard JSON (or the model's last text) returned to the UI.</summary>
    [JsonPropertyName("finalResponse")] public string? FinalResponse { get; set; }
}

/// <summary>Aggregates shown at the top of the usage page.</summary>
public class UsageSummary
{
    [JsonPropertyName("requests")] public int Requests { get; set; }
    [JsonPropertyName("failedRequests")] public int FailedRequests { get; set; }
    [JsonPropertyName("inputTokens")] public long InputTokens { get; set; }
    [JsonPropertyName("outputTokens")] public long OutputTokens { get; set; }
    [JsonPropertyName("cacheReadTokens")] public long CacheReadTokens { get; set; }
    [JsonPropertyName("totalTokens")] public long TotalTokens { get; set; }
    [JsonPropertyName("estimatedCost")] public decimal EstimatedCost { get; set; }
    [JsonPropertyName("avgTokensPerRequest")] public int AvgTokensPerRequest { get; set; }
    [JsonPropertyName("avgDurationMs")] public long AvgDurationMs { get; set; }
    [JsonPropertyName("byModel")] public List<ModelUsage> ByModel { get; set; } = new();
    [JsonPropertyName("byDay")] public List<DayUsage> ByDay { get; set; } = new();
}

public class ModelUsage
{
    [JsonPropertyName("model")] public string Model { get; set; } = string.Empty;
    [JsonPropertyName("requests")] public int Requests { get; set; }
    [JsonPropertyName("totalTokens")] public long TotalTokens { get; set; }
    [JsonPropertyName("estimatedCost")] public decimal EstimatedCost { get; set; }
}

public class DayUsage
{
    [JsonPropertyName("day")] public string Day { get; set; } = string.Empty;
    [JsonPropertyName("requests")] public int Requests { get; set; }
    [JsonPropertyName("totalTokens")] public long TotalTokens { get; set; }
    [JsonPropertyName("estimatedCost")] public decimal EstimatedCost { get; set; }
}
