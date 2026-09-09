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

    /// <summary>The dashboard's filter definitions (see DashboardSpec.Filters) — without
    /// these the shared view has no way to show which filter was applied when the link was
    /// created, even though the widget data itself is already the filtered snapshot.</summary>
    public string FiltersJson { get; set; } = "[]";

    /// <summary>Which filter values were selected at share time (frontend's
    /// state.activeFilters: filterId -> values) — shown read-only in the shared view; a
    /// snapshot is frozen exactly as it was when created, never re-executed (Part 3).</summary>
    public string ActiveFiltersJson { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }

    /// <summary>Null means "never expires" — the creator picks this themselves at creation
    /// time (a specific date or a duration resolved to one client-side); there is no
    /// system-enforced default.</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Null while active; set the moment the creator manually revokes the link.</summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>A simple open/view count, incremented on every successful (non-expired,
    /// non-revoked) anonymous GET — surfaced to the creator as a basic usage signal.</summary>
    public int ViewCount { get; set; }
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

    [JsonPropertyName("filters")]
    public JsonElement Filters { get; set; }

    [JsonPropertyName("activeFilters")]
    public JsonElement ActiveFilters { get; set; }

    /// <summary>Null (the default from the JSON binder when omitted) means "never expires".
    /// Sent by the frontend as an absolute ISO-8601 instant — a duration preset ("7 أيام")
    /// or a specific picked date are both resolved to one client-side before this is sent,
    /// so the server never needs to know which the creator chose.</summary>
    [JsonPropertyName("expiresAt")]
    public DateTime? ExpiresAt { get; set; }

    /// <summary>The Active dashboard this snapshot is taken from — required so the server can
    /// verify it really is Active (a Draft has no Owner, so a share link off one would be an
    /// orphaned, unmanageable copy of someone's private work-in-progress). See
    /// ShareController.Create.</summary>
    [JsonPropertyName("historyId")]
    public string? HistoryId { get; set; }
}
