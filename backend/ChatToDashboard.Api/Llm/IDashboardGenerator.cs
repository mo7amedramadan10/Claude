using ChatToDashboard.Api.Models;
using ChatToDashboard.Api.Sources;
using ChatToDashboard.Api.Users;

namespace ChatToDashboard.Api.Llm;

/// <summary>
/// Turns a natural-language question into a validated dashboard spec, using whichever
/// LLM provider is configured.
/// </summary>
public interface IDashboardGenerator
{
    /// <param name="currentDashboard">
    /// The dashboard currently on screen (last summary + full widgets), present when this
    /// question should continue/refine it and absent for a fresh start — see
    /// <see cref="Models.ChatRequest.CurrentDashboard"/> and AnalyticsTools.ComposeUserMessage.
    /// </param>
    /// <param name="imageDataUrl">
    /// An optional reference image ("data:&lt;mime&gt;;base64,...") — a dashboard screenshot
    /// or mockup to recreate with real data, attached to this question only.
    /// </param>
    /// <param name="requestingUser">Who asked — recorded on the usage log entry only (see
    /// UsageTracker.Begin); never affects what the model is allowed to see, which is already
    /// narrowed server-side into <paramref name="sources"/> before this is called.</param>
    /// <param name="lang">The frontend's UI language ("en" or null/"ar") — see
    /// AnalyticsTools.LanguageOverrideBlock. Anything other than "en" produces Arabic content,
    /// unchanged from before this parameter existed.</param>
    /// <param name="modelOverride">
    /// Set only by LlmRouter, when it has routed this specific call (because it carries an
    /// image) to a provider other than the one this client would otherwise use on its own —
    /// see LlmSettingsStore.ImageReaderProvider. Replaces the client's own configured/saved
    /// sub-model for this call only; null (every other caller) leaves that resolution exactly
    /// as it was before this parameter existed. Anthropic has no sub-model to override, so
    /// ClaudeClient ignores this.
    /// </param>
    Task<DashboardSpec> GenerateDashboardAsync(
        string question,
        DashboardStateInput? currentDashboard = null,
        SourceSelection? sources = null,
        string? imageDataUrl = null,
        AppUser? requestingUser = null,
        string? lang = null,
        string? modelOverride = null,
        CancellationToken ct = default);

    /// <summary>
    /// "الاستفسارات" mode — the exact same tool-use flow (list_files/query_data/forecast_data/
    /// search_documents, same source gating) as <see cref="GenerateDashboardAsync"/>, but the
    /// model answers in plain text instead of building widgets. Never continuation-aware —
    /// Inquiries is isolated from whatever dashboard is currently on screen.
    /// </summary>
    Task<InquiryResponse> GenerateInquiryAsync(
        string question,
        SourceSelection? sources = null,
        AppUser? requestingUser = null,
        string? lang = null,
        CancellationToken ct = default);

    /// <summary>
    /// "➕ أضف إلى لوحة المتابعة" — reshapes one already-answered Inquiries response's own data
    /// into a dashboard, via a single non-tool-calling call (no new query_data/list_files/...).
    /// </summary>
    /// <param name="currentDashboard">
    /// Same contract as <see cref="GenerateDashboardAsync"/>'s parameter of the same name:
    /// present means add this answer's data to it (every existing widget copied forward,
    /// unchanged, plus the new one); absent means there's nothing to add to yet, so this just
    /// builds a fresh dashboard containing this answer's data alone.
    /// </param>
    Task<DashboardSpec> GenerateDashboardFromInquiryAsync(
        InquiryResponse inquiry,
        DashboardStateInput? currentDashboard = null,
        SourceSelection? sources = null,
        AppUser? requestingUser = null,
        string? lang = null,
        CancellationToken ct = default);
}
