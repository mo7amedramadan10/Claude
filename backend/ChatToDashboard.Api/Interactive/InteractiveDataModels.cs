namespace ChatToDashboard.Api.Interactive;

/// <summary>
/// One template's real-data provider — INTERACTIVE-HANDOFF.md §1.3's dataEndpoint
/// ("/api/projects/{projectId}/interactive/{templateId}/data") implemented per TEMPLATE,
/// not per engine: several templates can share the same RenderKind/engine (e.g.
/// policy-network, procurement-network and client-supplier-network are all "network") while
/// each needs its own real sources and column mapping. Registered via DI (see Program.cs)
/// and dispatched by Controllers/InteractiveController.cs; the frontend already knows which
/// engine to mount from the saved dashboard's own RenderKind (see
/// DashboardHistoryEntry.RenderKind) — this endpoint only ever returns that engine's data.
/// </summary>
public interface IInteractiveDataProvider
{
    string TemplateId { get; }

    /// <summary>Builds the engine's real payload for this project, or an unlinked result
    /// when the project hasn't connected the sources this engine needs yet — never
    /// fabricated numbers, same rule as every other data path in this app.</summary>
    Task<InteractiveDataResult> BuildAsync(string projectId, CancellationToken ct);
}

/// <summary>
/// <paramref name="Linked"/> false means "show the empty state, not an error" (§3's rule:
/// a template with no linked source renders an explicit empty state + "Link a source"
/// button) — <see cref="MissingSources"/> names what would need connecting, for that
/// button's own copy. <paramref name="Data"/> is only meaningful when Linked is true, and
/// is whatever shape the engine's own mount function (network-graph.js/workflow-graph.js/
/// marketplace-graph.js, or a §4.16+ one) expects — this layer never interprets it.
/// </summary>
public record InteractiveDataResult(bool Linked, object? Data, IReadOnlyList<string>? MissingSources = null)
{
    public static InteractiveDataResult Unlinked(IReadOnlyList<string> missingSources) =>
        new(false, null, missingSources);

    public static InteractiveDataResult Ready(object data) => new(true, data);
}
