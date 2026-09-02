using System.Text.Json;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>Account management — creating/editing users and their per-source permissions. Admin only.</summary>
[ApiController]
[Route("api/users")]
[Authorize(Roles = UserRoles.Admin)]
public class UsersController : ControllerBase
{
    private readonly UserStore _users;

    public UsersController(UserStore users) => _users = users;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok((await _users.ListAsync(ct)).Select(UserStore.ToInfo));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserRequest request, CancellationToken ct)
    {
        var error = ValidateRequest(request, isCreate: true);
        if (error is not null) return BadRequest(new { error });

        if (await _users.FindByUsernameAsync(request.Username.Trim(), ct) is not null)
            return Conflict(new { error = "اسم المستخدم ده موجود بالفعل." });

        var user = new AppUser
        {
            Username = request.Username.Trim(),
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? request.Username.Trim() : request.DisplayName.Trim(),
            AuthMethod = request.AuthMethod,
            Role = request.Role,
            IsActive = request.IsActive,
            PasswordHash = request.AuthMethod == AuthMethods.Local ? PasswordHasher.Hash(request.Password!) : "",
            AllowAllSystems = request.AllowAllSystems,
            AllowedSystemsJson = JsonSerializer.Serialize(request.AllowedSystems),
            AllowAllCategories = request.AllowAllCategories,
            AllowedCategoriesJson = JsonSerializer.Serialize(request.AllowedCategories),
        };
        var created = await _users.CreateAsync(user, ct);
        return Ok(UserStore.ToInfo(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UserRequest request, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(id, ct);
        if (user is null) return NotFound(new { error = "المستخدم غير موجود." });

        var error = ValidateRequest(request, isCreate: false);
        if (error is not null) return BadRequest(new { error });

        // Never allow the last remaining admin to demote or deactivate themselves by
        // accident — that would lock everyone out of user management.
        if (user.Role == UserRoles.Admin && (request.Role != UserRoles.Admin || !request.IsActive))
        {
            var otherActiveAdmins = (await _users.ListAsync(ct))
                .Any(u => u.Id != user.Id && u.Role == UserRoles.Admin && u.IsActive);
            if (!otherActiveAdmins)
                return BadRequest(new { error = "لازم يفضل مسؤول واحد نشط على الأقل." });
        }

        user.DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? user.Username : request.DisplayName.Trim();
        user.AuthMethod = request.AuthMethod;
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.AllowAllSystems = request.AllowAllSystems;
        user.AllowedSystemsJson = JsonSerializer.Serialize(request.AllowedSystems);
        user.AllowAllCategories = request.AllowAllCategories;
        user.AllowedCategoriesJson = JsonSerializer.Serialize(request.AllowedCategories);
        if (request.AuthMethod == AuthMethods.Local && !string.IsNullOrWhiteSpace(request.Password))
            user.PasswordHash = PasswordHasher.Hash(request.Password);
        else if (request.AuthMethod == AuthMethods.ActiveDirectory)
            user.PasswordHash = "";

        await _users.UpdateAsync(user, ct);
        return Ok(UserStore.ToInfo(user));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(id, ct);
        if (user is null) return NotFound(new { error = "المستخدم غير موجود." });
        if (user.Role == UserRoles.Admin)
        {
            var otherActiveAdmins = (await _users.ListAsync(ct)).Any(u => u.Id != id && u.Role == UserRoles.Admin && u.IsActive);
            if (!otherActiveAdmins) return BadRequest(new { error = "لازم يفضل مسؤول واحد نشط على الأقل." });
        }
        await _users.DeleteAsync(id, ct);
        return NoContent();
    }

    private static string? ValidateRequest(UserRequest request, bool isCreate)
    {
        if (string.IsNullOrWhiteSpace(request.Username)) return "اسم المستخدم مطلوب.";
        if (request.AuthMethod is not (AuthMethods.Local or AuthMethods.ActiveDirectory)) return "طريقة الدخول غير معروفة.";
        if (request.Role is not (UserRoles.Admin or UserRoles.User)) return "الدور غير معروف.";
        if (request.AuthMethod == AuthMethods.Local && isCreate && string.IsNullOrWhiteSpace(request.Password))
            return "كلمة المرور مطلوبة للحسابات المحلية.";
        return null;
    }
}
