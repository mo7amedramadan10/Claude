using System.Security.Claims;
using System.Text.Json;
using ChatToDashboard.Api.Projects;
using ChatToDashboard.Api.Sources;

namespace ChatToDashboard.Api.Users;

/// <summary>
/// Turns "what the client asked for" into "what this user is actually allowed to see" —
/// the server-side half of source gating. <see cref="Sources.SourceSelection"/> already
/// tells the agent which systems/categories are on; this just narrows the client's
/// request down to the signed-in user's own permissions before it ever reaches
/// <c>AnalyticsTools</c>, so a user can never widen their own access by editing the
/// request body. Admins are never restricted.
/// </summary>
public class PermissionsService
{
    private readonly UserStore _users;
    private readonly ProjectStore _projects;

    public PermissionsService(UserStore users, ProjectStore projects)
    {
        _users = users;
        _projects = projects;
    }

    public static string? UserId(ClaimsPrincipal principal) => principal.FindFirstValue(ClaimTypes.NameIdentifier);

    public async Task<AppUser?> GetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var id = UserId(principal);
        return id is null ? null : await _users.FindByIdAsync(id, ct);
    }

    /// <summary>Intersects the client's requested selection with what <paramref name="user"/>
    /// may access, and stamps the user's current project (see
    /// ProjectStore.ResolveCurrentProjectIdAsync) so AnalyticsTools.DescribeSourcesAsync can
    /// scope repository files and integrations to it — an instance method (not the static
    /// helper this used to be) purely because resolving the project needs a DB round trip.</summary>
    public async Task<SourceSelection> GetEffectiveSelectionAsync(
        AppUser user, SourceSelection? requested, CancellationToken ct = default)
    {
        var isAdmin = user.Role == UserRoles.Admin;
        var projectId = await _projects.ResolveCurrentProjectIdAsync(user.OrganizationId, user.Id, isAdmin, ct);

        if (isAdmin)
        {
            var admin = requested ?? SourceSelection.AllEnabled();
            admin.UserId = user.Id;
            admin.IsAdmin = true;
            admin.ProjectId = projectId;
            return admin;
        }

        var req = requested ?? SourceSelection.AllEnabled();
        var (systemsUnset, systems) = Narrow(req.SystemsUnset, req.Systems, user.AllowAllSystems, Deserialize(user.AllowedSystemsJson));
        var (filesUnset, files) = Narrow(req.FilesUnset, req.Files, user.AllowAllFiles, Deserialize(user.AllowedFilesJson));

        return new SourceSelection
        {
            SystemsUnset = systemsUnset, Systems = systems,
            FilesUnset = filesUnset, Files = files,
            UserId = user.Id, IsAdmin = false, ProjectId = projectId,
        };
    }

    /// <summary>
    /// Combines one dimension (systems, or categories) of the client's request with the
    /// user's own permission for it. Unset on both sides is the only way the result stays
    /// unset (truly unrestricted); an explicit list on either side narrows to it, and two
    /// explicit lists narrow to their intersection.
    /// </summary>
    private static (bool Unset, List<string> List) Narrow(
        bool requestedUnset, List<string> requestedList, bool userUnset, List<string> userList)
    {
        if (requestedUnset && userUnset) return (true, new List<string>());
        if (requestedUnset) return (false, userList);
        if (userUnset) return (false, requestedList);
        return (false, requestedList.Where(s => userList.Contains(s, StringComparer.OrdinalIgnoreCase)).ToList());
    }

    private static List<string> Deserialize(string? json) =>
        string.IsNullOrWhiteSpace(json) ? new() : JsonSerializer.Deserialize<List<string>>(json) ?? new();
}
