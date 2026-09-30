using System.Text.Json.Serialization;

namespace ChatToDashboard.Api.Integrations;

public static class IdentityTransportMechanisms
{
    public const string Query = "query";
    public const string Header = "header";
    public const string Cookie = "cookie";
    public static readonly IReadOnlyList<string> All = new[] { Query, Header, Cookie };
}

/// <summary>The two database engines the generated connector service (Deliverable 3) knows how
/// to run a live query against — the same pair this app's own DataStore already supports, so
/// the exact same driver packages (Microsoft.Data.SqlClient / Microsoft.Data.Sqlite) cover it.
/// A client on a different engine isn't supported yet — surfaced as a clear "غير مدعوم" choice
/// rather than silently accepting a value the connector can't act on.</summary>
public static class ClientDbProviders
{
    public const string SqlServer = "SqlServer";
    public const string Sqlite = "Sqlite";
    public static readonly IReadOnlyList<string> All = new[] { SqlServer, Sqlite };
}

/// <summary>Data-level (row) permissions — an optional second layer beneath the whole-dashboard
/// everyone/restricted check (Part E). A table with this exact fixed name and shape (UserId,
/// FilterKey, FilterValue — see IntegrationConnector's own DataPermissions class, duplicated
/// there since it has no reference back to this codebase), if the client creates and populates
/// one, is auto-detected from the discovered schema — never configured here, never something an
/// analyst names or designs. The client alone decides what goes in it.</summary>
public static class DataPermissions
{
    public const string TableName = "ChatToDashboard_DataPermissions";
}

/// <summary>
/// One external client system's whole integration setup — everything Part A/B/C describe: the
/// connector service's base URL (its route shape is fixed and known — see IntegrationConnector's
/// Program.cs — so every endpoint Publish/viewer.html/admin.html need is derived from this one
/// value, never separately configured), how the current user's identity is forwarded (Part C,
/// requires <see cref="IdentityConfirmed"/> before use), and the visual-identity token set (Part
/// B) applied to the viewer page. Credentials are stored as given (this is an admin-only internal
/// settings surface, same posture as appsettings-configured Sources:Systems:Api:Headers) but
/// never echoed back by the read endpoints — only an IsConfigured-style boolean, same convention
/// as LlmSettingsController.
/// </summary>
public class ExternalIntegration
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";

    // The deployed connector's base URL (e.g. "http://connector.client-network:5000") and the
    // one shared key protecting its backend-only endpoints (/publish, /schema — never
    // /dashboards, /permissions or /directory, which the client's own browser calls directly and
    // so can never safely carry a shared secret; see IntegrationConnector's Program.cs).
    public string? ConnectorBaseUrl { get; set; }
    public string? ConnectorAuthHeader { get; set; }
    public string? ConnectorAuthValue { get; set; }

    // Part A/D/E — every endpoint the connector exposes, derived from ConnectorBaseUrl rather
    // than configured separately, since we control both ends and its route shape never varies.
    public string? PublishUrl => CombineUrl(ConnectorBaseUrl, "publish");
    public string? DashboardsUrl => CombineUrl(ConnectorBaseUrl, "dashboards");
    public string? DirectoryUrl => CombineUrl(ConnectorBaseUrl, "directory");
    public string? PermissionsUrl => CombineUrl(ConnectorBaseUrl, "permissions");
    public string? SchemaUrl => CombineUrl(ConnectorBaseUrl, "schema");
    public string? PermissionFilterKeysUrl => CombineUrl(ConnectorBaseUrl, "permission-filter-keys");
    public string? QueryUrl => CombineUrl(ConnectorBaseUrl, "query");

    private static string? CombineUrl(string? baseUrl, string path) =>
        string.IsNullOrWhiteSpace(baseUrl) ? null : baseUrl.TrimEnd('/') + "/" + path;

    // Part C — fixed mechanism set; see IdentityTransportMechanisms. Never active (viewer.html
    // is generated with NO identity-forwarding code) until IdentityConfirmed is explicitly set —
    // see IntegrationsController's confirm endpoint.
    public string? IdentityMechanism { get; set; }
    public string? IdentityParameterName { get; set; }
    public bool IdentityConfirmed { get; set; }
    public DateTime? IdentityConfirmedAt { get; set; }
    public string? IdentityConfirmedBy { get; set; }

    // Part B — the fixed three-token visual identity set (never open-ended CSS).
    public string? AccentColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? FontFamily { get; set; }

    // The client's OWN database — tables/columns/types, discovered once from the connector's own
    // GET /schema (see IntegrationsController.DiscoverClientSchema), so Publish can retarget
    // each widget's query to run directly against their real schema instead of ours (see
    // AnalyticsTools.RetargetSqlSystemPrompt and PublishService). The connection string itself is
    // never given to us — it stays local to the connector's own config, entirely outside our
    // reach.
    public string? ClientDbProvider { get; set; }
    public string? ClientSchemaDescription { get; set; }

    // Set automatically by ClientSchemaDiscoveryService whenever the discovered schema contains
    // a table named DataPermissions.TableName — never set by hand. Gates the publish-modal's
    // per-widget data-level-filtering UI: it only ever appears when this is true.
    public bool DataPermissionsAvailable { get; set; }

    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>One dashboard already published under one integration — the external dashboardId we
/// assigned it, so re-publishing the same Active dashboard updates this same slot instead of
/// creating a duplicate (see PublishService).</summary>
public class PublishedDashboardSlot
{
    public string Id { get; set; } = "";
    public string IntegrationId { get; set; } = "";
    public string ExternalDashboardId { get; set; } = "";
    public string LocalHistoryId { get; set; } = "";
    public string Title { get; set; } = "";
    public DateTime FirstPublishedAt { get; set; }
    public DateTime LastPublishedAt { get; set; }
    public string LastPublishedBy { get; set; } = "";
}

/// <summary>One widget's data-level-filtering mark, per (integration, dashboard) — set once by
/// the analyst in the publish modal (Part E, data-level extension), reused on every future
/// publish of that same dashboard to that same integration. Identifies the widget by its position
/// in the dashboard's own WidgetsJson array, since widgets have no separate id of their own.
/// Never applies unless <see cref="ExternalIntegration.DataPermissionsAvailable"/> is true.</summary>
public class WidgetDataFilter
{
    public string IntegrationId { get; set; } = "";
    public string LocalHistoryId { get; set; } = "";
    public int WidgetIndex { get; set; }
    public string FilterKey { get; set; } = "";
}

/// <summary>One publish attempt, success or failure — "log every publish (who, when, which
/// dashboard, which integration) — same transparency principle used elsewhere in the app" (see
/// UsageTracker for the established precedent this mirrors).</summary>
public class IntegrationPublishLogEntry
{
    public string Id { get; set; } = "";
    public string IntegrationId { get; set; } = "";
    public string ExternalDashboardId { get; set; } = "";
    public string LocalHistoryId { get; set; } = "";
    public string DashboardTitle { get; set; } = "";
    public string UserId { get; set; } = "";
    public bool Success { get; set; }
    public string? Error { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ---------- request/response DTOs ----------

public class CreateIntegrationRequest
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
}

public class UpdateIntegrationApisRequest
{
    [JsonPropertyName("connectorBaseUrl")] public string? ConnectorBaseUrl { get; set; }
    [JsonPropertyName("connectorAuthHeader")] public string? ConnectorAuthHeader { get; set; }
    [JsonPropertyName("connectorAuthValue")] public string? ConnectorAuthValue { get; set; }
}

public class WidgetDataFilterEntry
{
    [JsonPropertyName("widgetIndex")] public int WidgetIndex { get; set; }
    [JsonPropertyName("filterKey")] public string FilterKey { get; set; } = "";
}

public class SetWidgetDataFiltersRequest
{
    /// <summary>The complete set of marked widgets for this (integration, dashboard) pair —
    /// replaces whatever was there before; an empty list clears all marks.</summary>
    [JsonPropertyName("filters")] public List<WidgetDataFilterEntry> Filters { get; set; } = new();
}

/// <summary>One widget's retargeted-and-cached publish preview (see PublishService.PrepareAsync)
/// — <see cref="Columns"/> are the real output columns of <see cref="Sql"/>, parsed from the
/// query text itself, so the analyst picks a data-level filter column from what the query
/// actually returns rather than typing/guessing a name.</summary>
public class PublishPreviewWidget
{
    [JsonPropertyName("index")] public int Index { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("sql")] public string? Sql { get; set; }
    [JsonPropertyName("columns")] public List<string> Columns { get; set; } = new();
    [JsonPropertyName("filterKey")] public string? FilterKey { get; set; }
}

public class PublishPrepareResult
{
    [JsonPropertyName("previewId")] public string PreviewId { get; set; } = "";
    [JsonPropertyName("widgetsNeedingColumn")] public List<PublishPreviewWidget> WidgetsNeedingColumn { get; set; } = new();
}

public class ConfirmColumnChoice
{
    [JsonPropertyName("widgetIndex")] public int WidgetIndex { get; set; }
    [JsonPropertyName("column")] public string Column { get; set; } = "";
}

public class ConfirmPublishRequest
{
    [JsonPropertyName("previewId")] public string PreviewId { get; set; } = "";
    [JsonPropertyName("slotId")] public string? SlotId { get; set; }
    [JsonPropertyName("columnChoices")] public List<ConfirmColumnChoice> ColumnChoices { get; set; } = new();
}

/// <summary>Shape of the connector's own GET /schema response (see IntegrationConnector's
/// SchemaInfo/SchemaTable/SchemaColumn) — structure only, deserialized case-insensitively since
/// it's produced by a separate project's default ASP.NET Core JSON casing.</summary>
public class ConnectorSchemaResponse
{
    public string Provider { get; set; } = "";
    public List<ConnectorSchemaTable> Tables { get; set; } = new();
}

public class ConnectorSchemaTable
{
    public string Name { get; set; } = "";
    public List<ConnectorSchemaColumn> Columns { get; set; } = new();
}

public class ConnectorSchemaColumn
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
}

public class ManualVisualIdentityRequest
{
    [JsonPropertyName("accentColor")] public string AccentColor { get; set; } = "";
    [JsonPropertyName("secondaryColor")] public string SecondaryColor { get; set; } = "";
    [JsonPropertyName("fontFamily")] public string FontFamily { get; set; } = "";
}

public class SuggestVisualIdentityRequest
{
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("seedColor")] public string? SeedColor { get; set; }
}

public class MatchIdentityTransportRequest
{
    [JsonPropertyName("description")] public string Description { get; set; } = "";
}

public class ConfirmIdentityTransportRequest
{
    [JsonPropertyName("mechanism")] public string Mechanism { get; set; } = "";
    [JsonPropertyName("parameterName")] public string ParameterName { get; set; } = "";
}

public class PublishRequest
{
    /// <summary>Which HistoryStore Active dashboard to publish.</summary>
    [JsonPropertyName("dashboardId")] public string DashboardId { get; set; } = "";

    /// <summary>Null/absent means "create a new slot"; set (an existing PublishedDashboardSlot.Id)
    /// means "update that slot" — see PublishService's create-or-update decision.</summary>
    [JsonPropertyName("slotId")] public string? SlotId { get; set; }
}
