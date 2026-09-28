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
    /// model answers with a list of labeled blocks (data-grounded vs. general knowledge)
    /// instead of building widgets. A real multi-turn conversation: <paramref
    /// name="recentTurns"/>/<paramref name="priorSummary"/>/<paramref name="ledger"/> frame
    /// this question as a continuation the same way <see cref="GenerateDashboardAsync"/>'s own
    /// currentDashboard does for a dashboard follow-up (see AnalyticsTools.
    /// ComposeInquiryUserMessage) — all three empty/null means a brand-new conversation.
    /// </summary>
    /// <param name="recentTurns">The conversation's own tail, replayed verbatim (including
    /// each turn's block labels) so wording stays consistent turn to turn.</param>
    /// <param name="priorSummary">The model's own rolling summary (from the previous turn's
    /// InquiryResponse.Summary) of everything before <paramref name="recentTurns"/>.</param>
    /// <param name="ledger">Every "data" block produced anywhere in this conversation so far,
    /// regardless of age — never summarized away, since it's the only place an old turn's real
    /// figures survive once that turn itself has scrolled out of <paramref name="recentTurns"/>.
    /// Already masked (see InquiryAccessService) before this is called, so a block the caller
    /// can no longer access never reaches the model at all.</param>
    Task<InquiryResponse> GenerateInquiryAsync(
        string question,
        IReadOnlyList<ConversationTurn>? recentTurns = null,
        string? priorSummary = null,
        IReadOnlyList<InquiryBlock>? ledger = null,
        SourceSelection? sources = null,
        AppUser? requestingUser = null,
        string? lang = null,
        CancellationToken ct = default);

    /// <summary>
    /// "🔄 حوّله لداشبورد" — reshapes one already-answered Inquiries block's own data into a
    /// dashboard, via a single non-tool-calling call (no new query_data/list_files/...).
    /// </summary>
    /// <param name="currentDashboard">
    /// Same contract as <see cref="GenerateDashboardAsync"/>'s parameter of the same name:
    /// present means add this block's data to it (every existing widget copied forward,
    /// unchanged, plus the new one); absent means either nothing is on screen yet, or the
    /// user chose "استبدال" on the replace-or-add prompt — either way this just builds a
    /// fresh dashboard containing this block's data alone.
    /// </param>
    Task<DashboardSpec> GenerateDashboardFromInquiryAsync(
        InquiryBlock block,
        DashboardStateInput? currentDashboard = null,
        SourceSelection? sources = null,
        AppUser? requestingUser = null,
        string? lang = null,
        CancellationToken ct = default);
}
