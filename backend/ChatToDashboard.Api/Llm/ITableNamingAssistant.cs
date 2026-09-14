using ChatToDashboard.Api.Users;

namespace ChatToDashboard.Api.Llm;

/// <summary>
/// Suggests a short, clean SQL table identifier for a newly uploaded repository file, so the
/// model building dashboards later has a name short enough to type back correctly instead of
/// mentally "simplifying" a long filename-derived one (e.g. writing "staging.opportunities"
/// instead of the real "staging.repo_Opportunities_Tracker_V_3_0__1__976323") — see
/// RepositoryStore.SaveAsync, the only caller, and AnalyticsTools' "Invalid object name"
/// error-hint fix that handles the same underlying failure from the other direction.
///
/// Deliberately routed through LlmRouter's normal (dashboard-building) provider resolution
/// rather than its own independent setting the way IDocumentTextExtractor is — this is a single
/// lightweight suggestion call, not a recurring per-file-type choice worth its own admin
/// setting. A failure here (any exception, or the model returning nothing usable) must never
/// block a file upload, so implementations return null rather than throwing; the caller falls
/// back to the existing filename-sanitization behavior in that case.
/// </summary>
public interface ITableNamingAssistant
{
    /// <param name="displayName">The file's user-entered display name.</param>
    /// <param name="description">The file's user-entered description, if any.</param>
    /// <param name="columnNames">The parsed file's column headers, in order.</param>
    Task<string?> SuggestTableNameAsync(
        string displayName, string? description, IReadOnlyList<string> columnNames,
        AppUser? requestingUser = null, CancellationToken ct = default);
}
