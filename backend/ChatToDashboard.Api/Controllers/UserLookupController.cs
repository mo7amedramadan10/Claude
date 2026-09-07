using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>
/// A narrow, any-authenticated-user lookup — exact username only, never a listing — so a
/// non-Admin Active-dashboard Owner can grant an Editor/Viewer role or transfer ownership
/// to someone by typing their username, without needing the Admin-only GET /api/users
/// (UsersController) to enumerate everyone first.
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
}
