using System.Text.Json;
using System.Text.Json.Serialization;
using ChatToDashboard.Api.Widgets;

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
    /// state.activeFilters: filterId -> values) — the shared view opens with the same
    /// selection, and the recipient can change it from there (see POST
    /// /api/share/{id}/widgets/{index}/refresh).</summary>
    public string ActiveFiltersJson { get; set; } = "{}";
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

    [JsonPropertyName("filters")]
    public JsonElement Filters { get; set; }

    [JsonPropertyName("activeFilters")]
    public JsonElement ActiveFilters { get; set; }
}

/// <summary>
/// Body of POST /api/share/{id}/widgets/{index}/refresh — re-runs one widget already
/// published in this share with a (possibly different) filter selection, or with none at
/// all for a plain refresh. Deliberately carries no table/sql/query of its own: the widget
/// to run comes from the share's own stored WidgetsJson at <c>index</c>, never from the
/// (anonymous, unauthenticated) caller — see ShareController for why that boundary matters.
/// </summary>
public class RefreshShareWidgetRequest
{
    [JsonPropertyName("filters")]
    public List<FilterCondition>? Filters { get; set; }
}
