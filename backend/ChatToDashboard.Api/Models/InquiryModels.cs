using System.Text.Json;
using System.Text.Json.Serialization;
using ChatToDashboard.Api.Sources;

namespace ChatToDashboard.Api.Models;

/// <summary>Body of POST /api/inquiry — "الاستفسارات" mode: same tool-use flow as the chat-to-
/// dashboard endpoint, but the model answers in plain text instead of building widgets. Never
/// carries a currentDashboard — Inquiries never continues or is continued by the dashboard
/// currently on screen (see the Inquiries sub-tab's isolation rule).</summary>
public class InquiryRequest
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("sources")]
    public SourceSelection? Sources { get; set; }
}

/// <summary>
/// The structured JSON an Inquiries-mode answer must be — deserialized and validated the same
/// way as <see cref="DashboardSpec"/>, just a much smaller contract: a direct textual answer,
/// its two-sentence provenance (same convention as a widget's <see
/// cref="DashboardWidget.Source"/>), and the real data point(s) behind it — kept around so
/// "🔄 حوّله لداشبورد" can hand them to a follow-up call without re-querying anything.
/// </summary>
public class InquiryResponse
{
    [JsonPropertyName("answer")]
    public string Answer { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public JsonElement Data { get; set; }

    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(Answer))
            errors.Add("\"answer\" is required and must be a non-empty string.");
        if (string.IsNullOrWhiteSpace(Source))
            errors.Add("\"source\" is required: two sentences — where the data came from, then how it was calculated.");
        return errors;
    }
}

public class InquiryApiResponse
{
    [JsonPropertyName("answer")]
    public InquiryResponse? Answer { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

/// <summary>Body of POST /api/inquiry/convert — "➕ أضف إلى لوحة المتابعة": the exact answer/
/// source/data of one already-given Inquiries answer, restructured into a dashboard via a
/// single non-tool-calling call (see IDashboardGenerator.GenerateDashboardFromInquiryAsync).</summary>
public class ConvertInquiryRequest
{
    [JsonPropertyName("answer")]
    public string Answer { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public JsonElement Data { get; set; }

    /// <summary>
    /// The dashboard currently on screen — same shape and same "add to it, don't replace"
    /// contract as <see cref="ChatRequest.CurrentDashboard"/>. Null when there's nothing to
    /// add to yet (first conversion of a session, or right after "🆕 ابدأ لوحة جديدة"), in
    /// which case this behaves exactly like the old "🔄 حوّله لداشبورد": a fresh dashboard
    /// containing just this answer's data.
    /// </summary>
    [JsonPropertyName("currentDashboard")]
    public DashboardStateInput? CurrentDashboard { get; set; }

    [JsonPropertyName("sources")]
    public SourceSelection? Sources { get; set; }
}
