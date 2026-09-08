using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatToDashboard.Api.History;

/// <summary>
/// One saved dashboard: the question that produced it and everything needed to redraw
/// it — the widgets exactly as the model returned them — without asking the model again.
/// </summary>
public class DashboardHistoryEntry
{
    public string Id { get; set; } = "";
    public string UserId { get; set; } = "";
    public string Question { get; set; } = "";

    /// <summary>
    /// A short "what was queried" line. The dashboard schema has no separate field for
    /// this (only <see cref="Summary"/>), so it is set to the same text — reusing it
    /// keeps this feature from touching the model's response schema or prompt.
    /// </summary>
    public string QueryDescription { get; set; } = "";
    public string Summary { get; set; } = "";
    public string WidgetsJson { get; set; } = "[]";

    /// <summary>The dashboard's filter definitions (see DashboardSpec.Filters) — restored
    /// on reopen so the filter controls themselves still exist, not just the widgets.</summary>
    public string FiltersJson { get; set; } = "[]";

    /// <summary>Which filter values were actually selected (frontend's state.activeFilters:
    /// filterId -> values) — restored on reopen so the controls show the same selection the
    /// dashboard was showing when saved, matching the already-filtered widget data.</summary>
    public string ActiveFiltersJson { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }

    /// <summary>Draft (false, default) — a frozen snapshot, unchanged behavior. Active (true)
    /// — the user explicitly promoted it: it gets an Owner/roles and re-executes its widgets
    /// live on every open instead of showing the frozen <see cref="WidgetsJson"/>.</summary>
    public bool IsActive { get; set; }

    /// <summary>Only meaningful when <see cref="IsActive"/>. Whoever's data permission the
    /// dashboard's live query executes under — starts as the activating user, changeable via
    /// owner transfer. Null for a Draft.</summary>
    public string? OwnerId { get; set; }
}

/// <summary>One row of <c>DashboardRoles</c>: a named user's Editor/Viewer role on one Active
/// dashboard. The Owner is not stored here — it's <see cref="DashboardHistoryEntry.OwnerId"/>.</summary>
public class DashboardRoleEntry
{
    public string DashboardId { get; set; } = "";
    public string UserId { get; set; } = "";

    /// <summary>"editor" or "viewer".</summary>
    public string Role { get; set; } = "";
}

public static class DashboardRoles
{
    public const string Editor = "editor";
    public const string Viewer = "viewer";
}

/// <summary>Body of PUT /api/history/{id}/roles/{userId}.</summary>
public class SetDashboardRoleRequest
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "";
}

/// <summary>Body of POST /api/history/{id}/transfer-owner.</summary>
public class TransferOwnerRequest
{
    [JsonPropertyName("newOwnerId")]
    public string NewOwnerId { get; set; } = "";
}

/// <summary>Body of POST /api/history/{id}/widgets/{index}/refresh — same shape as the
/// share-scoped equivalent (index-only, never client-supplied table/sql).</summary>
public class RefreshDashboardWidgetRequest
{
    [JsonPropertyName("filters")]
    public List<Widgets.FilterCondition>? Filters { get; set; }
}

/// <summary>Body of POST /api/history — the frontend sends the widgets array verbatim.</summary>
public class SaveHistoryRequest
{
    [JsonPropertyName("question")]
    public string Question { get; set; } = "";

    [JsonPropertyName("queryDescription")]
    public string? QueryDescription { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = "";

    [JsonPropertyName("widgets")]
    public JsonElement Widgets { get; set; }

    [JsonPropertyName("filters")]
    public JsonElement Filters { get; set; }

    [JsonPropertyName("activeFilters")]
    public JsonElement ActiveFilters { get; set; }
}

/// <summary>Body of PUT /api/history/{id}/rename — renames the title/description only,
/// leaving widgets/filters untouched (unlike the full-content autosave below).</summary>
public class RenameHistoryRequest
{
    [JsonPropertyName("question")]
    public string Question { get; set; } = "";

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = "";
}

/// <summary>Body of PUT /api/history/{id} — dashboard-editor autosave.</summary>
public class UpdateHistoryRequest
{
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = "";

    [JsonPropertyName("widgets")]
    public JsonElement Widgets { get; set; }

    [JsonPropertyName("filters")]
    public JsonElement Filters { get; set; }

    [JsonPropertyName("activeFilters")]
    public JsonElement ActiveFilters { get; set; }
}
