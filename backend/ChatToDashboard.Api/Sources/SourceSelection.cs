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
    /// True when the UI did not send a systems selection at all (older client, direct API
    /// call, or a permission set with no system restriction): every system stays enabled
    /// rather than silently answering with nothing. Independent from
    /// <see cref="CategoriesUnset"/> because a user's permissions can restrict one
    /// dimension without restricting the other.
    /// </summary>
    [JsonIgnore]
    public bool SystemsUnset { get; set; }

    /// <summary>Same as <see cref="SystemsUnset"/>, for categories.</summary>
    [JsonIgnore]
    public bool CategoriesUnset { get; set; }

    public static SourceSelection AllEnabled() => new() { SystemsUnset = true, CategoriesUnset = true };

    public bool AllowsSystem(string id) => SystemsUnset || Systems.Contains(id, StringComparer.OrdinalIgnoreCase);

    public bool AllowsCategory(string category) =>
        CategoriesUnset || Categories.Contains(category, StringComparer.OrdinalIgnoreCase);
}
