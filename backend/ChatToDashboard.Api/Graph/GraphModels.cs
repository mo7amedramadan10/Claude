namespace ChatToDashboard.Api.Graph;

/// <summary>
/// One node in a project's network graph (see GraphStore). DataJson carries whatever extra
/// numeric metrics the network's own domain needs (e.g. policy-network's complaint count /
/// compliance % / sentiment) — kept as a flexible JSON blob, same convention as
/// AnalystResult.VerificationJson elsewhere in this app, since each network "domain" (policy,
/// procurement, client-supplier — see the interactive-dashboards handoff's phase table) defines
/// its own metric shape and this store has no business knowing it ahead of time.
/// </summary>
public class GraphNode
{
    public string Id { get; set; } = "";
    public string ProjectId { get; set; } = "";
    public string NetworkKey { get; set; } = "";
    public string NodeKey { get; set; } = "";
    public string Type { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int? Year { get; set; }
    public string? DataJson { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GraphEdge
{
    public string Id { get; set; } = "";
    public string ProjectId { get; set; } = "";
    public string NetworkKey { get; set; } = "";
    public string FromKey { get; set; } = "";
    public string ToKey { get; set; } = "";
    public string RelType { get; set; } = "";
    public int? Year { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Known network keys this store seeds demo content for on first access per project —
/// see GraphStore.SeedIfEmptyAsync. Phase 3 of the handoff adds Procurement/ClientSupplier.</summary>
public static class GraphNetworkKeys
{
    public const string Policy = "policy";
}
