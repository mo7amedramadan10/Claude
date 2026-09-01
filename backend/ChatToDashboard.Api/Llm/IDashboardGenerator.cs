using ChatToDashboard.Api.Models;

namespace ChatToDashboard.Api.Llm;

/// <summary>
/// Turns a natural-language question into a validated dashboard spec, using whichever
/// LLM provider is configured.
/// </summary>
public interface IDashboardGenerator
{
    Task<DashboardSpec> GenerateDashboardAsync(
        string question, IReadOnlyList<ChatTurn>? history = null, CancellationToken ct = default);
}
