using ChatToDashboard.Api.Models;
using ChatToDashboard.Api.Sources;

namespace ChatToDashboard.Api.Llm;

/// <summary>
/// Turns a natural-language question into a validated dashboard spec, using whichever
/// LLM provider is configured.
/// </summary>
public interface IDashboardGenerator
{
    /// <param name="imageDataUrl">
    /// An optional reference image ("data:&lt;mime&gt;;base64,...") — a dashboard screenshot
    /// or mockup to recreate with real data, attached to this question only.
    /// </param>
    Task<DashboardSpec> GenerateDashboardAsync(
        string question,
        IReadOnlyList<ChatTurn>? history = null,
        SourceSelection? sources = null,
        string? imageDataUrl = null,
        CancellationToken ct = default);
}
