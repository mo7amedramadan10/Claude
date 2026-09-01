namespace ChatToDashboard.Api.Sources;

/// <summary>
/// A back-office system that can be toggled as a data source. Each system will get its own
/// connection string when it is wired up; until then it is listed but reported as
/// "not connected yet" so the model never invents data for it.
/// </summary>
public class SystemSource
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>Set once the system is connected; empty means not wired up yet.</summary>
    public string? ConnectionString { get; set; }

    /// <summary>Optional: the staging tables that belong to this system, once it has data.</summary>
    public List<string> Tables { get; set; } = new();

    public bool IsConnected => !string.IsNullOrWhiteSpace(ConnectionString) || Tables.Count > 0;
}

public class SourceOptions
{
    public const string SectionName = "Sources";

    public List<SystemSource> Systems { get; set; } = new();
}
