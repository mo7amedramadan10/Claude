using ChatToDashboard.Api.Models;
using ChatToDashboard.Api.Sources;

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
    Task<DashboardSpec> GenerateDashboardAsync(
        string question,
        DashboardStateInput? currentDashboard = null,
        SourceSelection? sources = null,
        string? imageDataUrl = null,
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
        CancellationToken ct = default);

    /// <summary>
    /// "🔄 حوّله لداشبورد" — reshapes one already-answered Inquiries response's own data into a
    /// normal dashboard, via a single non-tool-calling call (no new query_data/list_files/...).
    /// </summary>
    Task<DashboardSpec> GenerateDashboardFromInquiryAsync(
        InquiryResponse inquiry,
        SourceSelection? sources = null,
        CancellationToken ct = default);
}
