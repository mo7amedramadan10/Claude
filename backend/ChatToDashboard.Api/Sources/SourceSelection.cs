using System.Text.Json.Serialization;

namespace ChatToDashboard.Api.Sources;

/// <summary>
/// Which sources the user has switched on for this question. Sent by the UI with every
/// chat request; the agent is told about it so it can decline and name what to enable
/// instead of answering from a source the user turned off.
/// </summary>
public class SourceSelection
{
    [JsonPropertyName("systems")]
    public List<string> Systems { get; set; } = new();

    [JsonPropertyName("categories")]
    public List<string> Categories { get; set; } = new();

    /// <summary>
    /// True when the UI did not send a selection at all (older client, direct API call):
    /// everything stays enabled rather than silently answering with nothing.
    /// </summary>
    [JsonIgnore]
    public bool IsUnset { get; set; }

    public static SourceSelection AllEnabled() => new() { IsUnset = true };

    public bool AllowsSystem(string id) => IsUnset || Systems.Contains(id, StringComparer.OrdinalIgnoreCase);

    public bool AllowsCategory(string category) =>
        IsUnset || Categories.Contains(category, StringComparer.OrdinalIgnoreCase);
}
