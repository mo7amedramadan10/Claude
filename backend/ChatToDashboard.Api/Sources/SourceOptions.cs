namespace ChatToDashboard.Api.Sources;

/// <summary>How a system's data is pulled in, when it has an HTTP endpoint.</summary>
public class SystemApi
{
    /// <summary>Full URL of the endpoint returning the records. Empty means no API for this system.</summary>
    public string? Url { get; set; }

    public string Method { get; set; } = "GET";

    /// <summary>Extra request headers — e.g. "Authorization": "Bearer ..." if it ever needs one.</summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>JSON body for POST endpoints; ignored for GET.</summary>
    public string? Body { get; set; }

    /// <summary>
    /// Dotted path to the array inside the response, e.g. "result.items". Leave empty to
    /// auto-detect (result.items, result, items, data, or a top-level array).
    /// </summary>
    public string? ResultPath { get; set; }

    /// <summary>Safety cap on how many records are loaded.</summary>
    public int MaxRecords { get; set; } = 20000;

    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Accept an untrusted TLS certificate. Only for an internal server with a self-signed
    /// certificate — it disables certificate validation for this endpoint's requests.
    /// </summary>
    public bool AllowInvalidCertificate { get; set; }
}

/// <summary>
/// A back-office system that can be toggled as a data source. A system becomes "connected"
/// once it has an API endpoint (or a connection string / explicit tables); until then it is
/// listed but reported as not wired up, so the model never invents data for it.
/// </summary>
public class SystemSource
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>Set once the system is connected to a database; empty means not wired up.</summary>
    public string? ConnectionString { get; set; }

    /// <summary>Pull the system's records from an HTTP endpoint.</summary>
    public SystemApi? Api { get; set; }

    /// <summary>Optional: the staging tables that belong to this system, once it has data.</summary>
    public List<string> Tables { get; set; } = new();

    public bool HasApi => !string.IsNullOrWhiteSpace(Api?.Url);

    public bool IsConnected =>
        !string.IsNullOrWhiteSpace(ConnectionString) || HasApi || Tables.Count > 0;
}

public class SourceOptions
{
    public const string SectionName = "Sources";

    public List<SystemSource> Systems { get; set; } = new();
}
