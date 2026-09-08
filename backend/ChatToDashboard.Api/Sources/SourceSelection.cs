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

    /// <summary>
    /// Repository file ids currently enabled as data sources — each file is its own
    /// independent source, at the same level as a system, not grouped by category.
    /// </summary>
    [JsonPropertyName("files")]
    public List<string> Files { get; set; } = new();

    /// <summary>
    /// True when the UI did not send a systems selection at all (older client, direct API
    /// call, or a permission set with no system restriction): every system stays enabled
    /// rather than silently answering with nothing. Independent from
    /// <see cref="FilesUnset"/> because a user's permissions can restrict one
    /// dimension without restricting the other.
    /// </summary>
    [JsonIgnore]
    public bool SystemsUnset { get; set; }

    /// <summary>Same as <see cref="SystemsUnset"/>, for files.</summary>
    [JsonIgnore]
    public bool FilesUnset { get; set; }

    /// <summary>
    /// The account this selection is being resolved for — stamped by
    /// PermissionsService.GetEffectiveSelection, never sent by the client. Lets
    /// AnalyticsTools.DescribeSourcesAsync check a repository file's per-file named-user
    /// permission list (see RepositoryFile.PermittedUserIds) without widening every call
    /// site that already threads a SourceSelection through (ChatController, WidgetsController,
    /// ShareController's share-scoped refresh) to also carry a separate user id.
    /// </summary>
    [JsonIgnore]
    public string? UserId { get; set; }

    /// <summary>An Admin bypasses per-file permission lists entirely, same as they already
    /// bypass category/system narrowing.</summary>
    [JsonIgnore]
    public bool IsAdmin { get; set; }

    public static SourceSelection AllEnabled() => new() { SystemsUnset = true, FilesUnset = true };

    public bool AllowsSystem(string id) => SystemsUnset || Systems.Contains(id, StringComparer.OrdinalIgnoreCase);

    public bool AllowsFile(string fileId) => FilesUnset || Files.Contains(fileId, StringComparer.OrdinalIgnoreCase);
}
