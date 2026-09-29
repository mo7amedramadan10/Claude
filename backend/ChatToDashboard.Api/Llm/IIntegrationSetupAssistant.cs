using ChatToDashboard.Api.Users;

namespace ChatToDashboard.Api.Llm;

/// <summary>
/// A small, fixed token set — accent color, secondary color, font family — never open-ended
/// CSS. Every visual-identity path (manual entry, AI suggestion, deterministic image
/// extraction, PDF font extraction, vision extraction) produces exactly this shape, so
/// Integrations/VisualIdentityService's contrast validation and IntegrationsController's
/// storage logic never need to branch on which path produced it.
/// </summary>
public record VisualIdentitySuggestion(string AccentColor, string SecondaryColor, string FontFamily, string? Explanation);

/// <summary>
/// Result of matching an analyst's free-text description of their client's setup to one of
/// Part C's three fixed identity-transport mechanisms. The model's only role is picking one of
/// "query" | "header" | "cookie" and filling in the variable part (the parameter/header/cookie
/// name) — never writing new transport logic; see Integrations/IdentityTransportAssistant for
/// where the actual (pre-written, tested) code paths for each mechanism live. Matched is false
/// when the description doesn't clearly map to one of the three — the caller must then ask the
/// analyst to clarify rather than guessing an approximation.
/// </summary>
public record IdentityTransportMatch(bool Matched, string? Mechanism, string? ParameterName, string Summary);

/// <summary>
/// Lightweight, tool-free text suggestions that support setting up an External Integration —
/// same routing contract as <see cref="ITableNamingAssistant"/> (through LlmRouter's normal
/// dashboard-building provider, never its own independent setting): these are one-off
/// suggestion calls an analyst explicitly triggers from a button, not a recurring per-request
/// choice worth its own admin setting. Implementations never throw on a model/parsing failure —
/// they return null (or IdentityTransportMatch.Matched = false) so the analyst always keeps the
/// manual-entry fallback rather than the whole settings screen erroring out.
/// </summary>
public interface IIntegrationSetupAssistant
{
    /// <param name="description">A short free-text description of the client's brand (e.g.
    /// "أزرق داكن وذهبي، خط رسمي"), or null/empty if only <paramref name="seedColor"/> is given.</param>
    /// <param name="seedColor">A single hex color the analyst already knows, or null/empty if
    /// only <paramref name="description"/> is given. At least one of the two must be non-empty.</param>
    Task<VisualIdentitySuggestion?> SuggestVisualIdentityAsync(
        string? description, string? seedColor, AppUser? requestingUser = null, CancellationToken ct = default);

    /// <param name="extractedPdfText">Plain text already extracted from an uploaded brand-guide
    /// PDF via PdfPig (see VisualIdentityService) — this call only ever reads text, never the
    /// PDF's pages as images, so it needs no vision capability.</param>
    /// <returns>The font name found, or null if the text contains no explicit font mention.</returns>
    Task<string?> SuggestFontFromPdfTextAsync(
        string extractedPdfText, AppUser? requestingUser = null, CancellationToken ct = default);

    /// <param name="description">The analyst's free-text description of how their client's
    /// system identifies the current user (e.g. "بيبعتوا X-User-Id في الهيدر").</param>
    Task<IdentityTransportMatch> MatchIdentityTransportAsync(
        string description, AppUser? requestingUser = null, CancellationToken ct = default);

    /// <summary>"Build the dashboard directly on the client's own tables" — a faithful SQL-to-
    /// SQL translation of one widget's query from this app's own schema to the external
    /// client's real one (see ExternalIntegration.ClientSchemaDescription), so Publish can ship
    /// a query the client's generated connector service can run verbatim, live, on every
    /// viewer.html open — never this app's own internal SQL, which is meaningless (and usually
    /// unreachable) outside this app's own database.</summary>
    /// <param name="originalSql">This widget's own internal query, if it has one (see
    /// DashboardWidget.Query) — given purely as an intent reference, never executed as-is.</param>
    /// <returns>The retargeted query, or null if no honest equivalent exists against the given
    /// schema (never a guess) — PublishService leaves such a widget's Sql unset in that case.</returns>
    Task<string?> RetargetSqlAsync(
        string widgetTitle, string? originalSql, string clientDbProvider, string clientSchemaDescription,
        AppUser? requestingUser = null, CancellationToken ct = default);
}

/// <summary>
/// Genuine visual/image understanding (a logo image with no accompanying text) — deliberately
/// its own interface with its own independent, explicitly-flagged setting (see
/// LlmSettingsStore.GetVisualIdentityReaderAsync), exactly like <see cref="IDocumentTextExtractor"/>
/// is kept separate from <see cref="IDashboardGenerator"/>: which model (if any) may receive an
/// image for this task is a deliberate, capability-gated admin choice, never inferred or
/// silently routed to whatever provider happens to build dashboards today.
/// </summary>
public interface IVisualIdentityImageExtractor
{
    Task<VisualIdentitySuggestion?> ExtractVisualIdentityFromImageAsync(
        string imageDataUrl, AppUser? requestingUser = null, CancellationToken ct = default);
}
