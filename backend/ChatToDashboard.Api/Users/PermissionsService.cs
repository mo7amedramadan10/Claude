using System.Security.Claims;
using System.Text.Json;
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

    public PermissionsService(UserStore users) => _users = users;

    public static string? UserId(ClaimsPrincipal principal) => principal.FindFirstValue(ClaimTypes.NameIdentifier);

    public async Task<AppUser?> GetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var id = UserId(principal);
        return id is null ? null : await _users.FindByIdAsync(id, ct);
    }

    /// <summary>Intersects the client's requested selection with what <paramref name="user"/> may access.</summary>
    public static SourceSelection GetEffectiveSelection(AppUser user, SourceSelection? requested)
    {
        if (user.Role == UserRoles.Admin) return requested ?? SourceSelection.AllEnabled();

        var req = requested ?? SourceSelection.AllEnabled();
        var (systemsUnset, systems) = Narrow(req.SystemsUnset, req.Systems, user.AllowAllSystems, Deserialize(user.AllowedSystemsJson));
        var (categoriesUnset, categories) = Narrow(req.CategoriesUnset, req.Categories, user.AllowAllCategories, Deserialize(user.AllowedCategoriesJson));

        return new SourceSelection
        {
            SystemsUnset = systemsUnset, Systems = systems,
            CategoriesUnset = categoriesUnset, Categories = categories,
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
