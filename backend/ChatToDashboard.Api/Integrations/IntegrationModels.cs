using System.Text.Json.Serialization;

namespace ChatToDashboard.Api.Integrations;

public static class IdentityTransportMechanisms
{
    public const string Query = "query";
    public const string Header = "header";
    public const string Cookie = "cookie";
    public static readonly IReadOnlyList<string> All = new[] { Query, Header, Cookie };
}

/// <summary>
/// One external client system's whole integration setup — everything Part A/B/C describe:
/// where we publish to (write API), what the client's own read/directory APIs are (baked into
/// the two delivered files, never called by us directly — see IntegrationDeliverables), how the
/// current user's identity is forwarded (Part C, requires <see cref="IdentityConfirmed"/> before
/// use), and the visual-identity token set (Part B) applied to the viewer page. Credentials are
/// stored as given (this is an admin-only internal settings surface, same posture as
/// appsettings-configured Sources:Systems:Api:Headers) but never echoed back by the read
/// endpoints — only an IsConfigured-style boolean, same convention as LlmSettingsController.
/// </summary>
public class ExternalIntegration
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";

    // Part A — where a Publish action sends a dashboard's JSON.
    public string? WriteApiUrl { get; set; }
    public string? WriteApiAuthHeader { get; set; }
    public string? WriteApiAuthValue { get; set; }

    // Part D — the client's own read API base URL, baked into viewer.html at generation time.
    // Never called from our backend; only the browser running viewer.html on the client's own
    // domain calls it.
    public string? ReadApiBaseUrl { get; set; }

    // Part E, optional — a type-ahead search API for permissions-admin.html.
    public string? DirectoryApiUrl { get; set; }

    // Part E — where permissions-admin.html writes each dashboard's everyone/restricted
    // decision. Not specified as a separate concept in the client-facing requirements doc
    // (which only describes "a simple table on your side" read by the Read API) — kept as its
    // own configurable endpoint since the doc explicitly leaves the write mechanism to
    // "whatever write mechanism the client's API exposes for it".
    public string? PermissionsApiUrl { get; set; }
    public string? PermissionsApiAuthHeader { get; set; }
    public string? PermissionsApiAuthValue { get; set; }

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
    [JsonPropertyName("writeApiUrl")] public string? WriteApiUrl { get; set; }
    [JsonPropertyName("writeApiAuthHeader")] public string? WriteApiAuthHeader { get; set; }
    [JsonPropertyName("writeApiAuthValue")] public string? WriteApiAuthValue { get; set; }
    [JsonPropertyName("readApiBaseUrl")] public string? ReadApiBaseUrl { get; set; }
    [JsonPropertyName("directoryApiUrl")] public string? DirectoryApiUrl { get; set; }
    [JsonPropertyName("permissionsApiUrl")] public string? PermissionsApiUrl { get; set; }
    [JsonPropertyName("permissionsApiAuthHeader")] public string? PermissionsApiAuthHeader { get; set; }
    [JsonPropertyName("permissionsApiAuthValue")] public string? PermissionsApiAuthValue { get; set; }
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
