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
}
