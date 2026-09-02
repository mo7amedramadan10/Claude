using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatToDashboard.Api.Share;

/// <summary>
/// A dashboard snapshot published under a random link so it can be opened by anyone who
/// has the link — no login exists in this app, so "share" means "publish a read-only
/// copy under an unguessable id", the same trust model as most lightweight share links.
/// </summary>
public class SharedDashboard
{
    public string Id { get; set; } = "";
    public string CreatedByUserId { get; set; } = "";
    public string Question { get; set; } = "";
    public string Summary { get; set; } = "";
    public string WidgetsJson { get; set; } = "[]";
    public DateTime CreatedAt { get; set; }
}

/// <summary>Body of POST /api/share — the frontend sends the widgets array verbatim.</summary>
public class CreateShareRequest
{
    [JsonPropertyName("question")]
    public string Question { get; set; } = "";

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = "";

    [JsonPropertyName("widgets")]
    public JsonElement Widgets { get; set; }
}
