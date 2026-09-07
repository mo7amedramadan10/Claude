using System.Linq;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>
/// A narrow, any-authenticated-user user directory — exact-username lookup, and a small
/// capped partial-match search — never the full roster, so a non-Admin (an Active
/// dashboard's Owner, or a file's creator) can find someone to grant access to without the
/// Admin-only GET /api/users (UsersController) enumerating everyone first.
/// </summary>
[ApiController]
[Route("api/users/lookup")]
[Authorize]
public class UserLookupController : ControllerBase
{
    private readonly UserStore _users;

    public UserLookupController(UserStore users) => _users = users;

    [HttpGet("{username}")]
    public async Task<IActionResult> Get(string username, CancellationToken ct)
    {
        var user = await _users.FindByUsernameAsync(username.Trim(), ct);
        if (user is null || !user.IsActive) return NotFound(new { error = "المستخدم غير موجود." });
        return Ok(new { id = user.Id, username = user.Username, displayName = user.DisplayName });
    }

    /// <summary>Partial-match search (username or display name) — up to 8 results, for a
    /// searchable picker (file permissions, dashboard roles/transfer) that a non-Admin can
    /// use without the Admin-only GET /api/users.</summary>
    [HttpGet("~/api/users/search")]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2) return Ok(Array.Empty<object>());
        var users = await _users.SearchAsync(q.Trim(), 8, ct);
        return Ok(users.Select(u => new { id = u.Id, username = u.Username, displayName = u.DisplayName }));
    }
}
