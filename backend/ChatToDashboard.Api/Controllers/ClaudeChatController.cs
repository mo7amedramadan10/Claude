using System.Text;
using System.Text.Json;
using ChatToDashboard.Api.ClaudeChat;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

public class ClaudeChatSendRequest { public string Message { get; set; } = ""; }
public class ClaudeChatRenameRequest { public string Title { get; set; } = ""; }

/// <summary>محادثة Claude (CLI على الخادم) — انظر <see cref="ClaudeCliService"/>. الوصول بالأدوار في
/// <c>ClaudeCli:AllowedRoles</c>؛ كل مستخدم يرى محادثاته هو فقط.</summary>
[ApiController]
[Route("api/claude-chat")]
public class ClaudeChatController : ControllerBase
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly ClaudeCliService _cli;
    private readonly ClaudeChatStore _store;
    private readonly PermissionsService _permissions;

    public ClaudeChatController(ClaudeCliService cli, ClaudeChatStore store, PermissionsService permissions)
    {
        _cli = cli;
        _store = store;
        _permissions = permissions;
    }

    private async Task<(AppUser? User, IActionResult? Fail)> AuthorizedAsync(CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return (null, Unauthorized());
        if (!_cli.Enabled) return (user, StatusCode(403, new { error = "محادثة Claude معطَّلة من إعدادات الخادم (ClaudeCli:Enabled)." }));
        if (!_cli.AllowedRoles.Contains(user.Role, StringComparer.OrdinalIgnoreCase))
            return (user, StatusCode(403, new { error = "هذه الميزة متاحة للمسؤولين فقط." }));
        return (user, null);
    }

    /// <summary>حالة الميزة لهذا المستخدم: مفعّلة؟ مسموح له؟ والأمر متاح على الخادم؟</summary>
    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var allowed = _cli.Enabled && _cli.AllowedRoles.Contains(user.Role, StringComparer.OrdinalIgnoreCase);
        if (!allowed) return Ok(new { enabled = _cli.Enabled, allowed = false, available = false, detail = (string?)null });
        var (available, detail) = await _cli.ProbeAsync(ct);
        return Ok(new { enabled = true, allowed = true, available, detail, model = _cli.Model, workingDirectory = _cli.WorkingDirectory });
    }

    [HttpGet("chats")]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var (user, fail) = await AuthorizedAsync(ct);
        if (fail is not null) return fail;
        return Ok(await _store.ListAsync(user!.Id, ct));
    }

    [HttpGet("chats/{id}/messages")]
    public async Task<IActionResult> Messages(string id, CancellationToken ct)
    {
        var (user, fail) = await AuthorizedAsync(ct);
        if (fail is not null) return fail;
        var chat = await _store.GetAsync(id, user!.Id, ct);
        if (chat is null) return NotFound(new { error = "المحادثة غير موجودة." });
        return Ok(new { chat, messages = await _store.MessagesAsync(id, ct) });
    }

    [HttpPut("chats/{id}")]
    public async Task<IActionResult> Rename(string id, [FromBody] ClaudeChatRenameRequest request, CancellationToken ct)
    {
        var (user, fail) = await AuthorizedAsync(ct);
        if (fail is not null) return fail;
        if (await _store.GetAsync(id, user!.Id, ct) is null) return NotFound(new { error = "المحادثة غير موجودة." });
        if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest(new { error = "العنوان مطلوب." });
        await _store.RenameAsync(id, request.Title.Trim()[..Math.Min(120, request.Title.Trim().Length)], ct);
        return NoContent();
    }

    [HttpDelete("chats/{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var (user, fail) = await AuthorizedAsync(ct);
        if (fail is not null) return fail;
        await _store.DeleteAsync(id, user!.Id, ct);
        return NoContent();
    }

    /// <summary>إرسال رسالة وبث الرد بـ Server-Sent Events: أحداث <c>init</c> و<c>delta</c> و<c>done</c> و<c>error</c>.
    /// <c>id</c> = "new" ينشئ محادثة جديدة (معرّف جلسة CLI جديد) ويعيده في حدث <c>chat</c> أولًا.</summary>
    [HttpPost("chats/{id}/send")]
    public async Task Send(string id, [FromBody] ClaudeChatSendRequest request, CancellationToken ct)
    {
        var (user, fail) = await AuthorizedAsync(ct);
        if (fail is not null) { Response.StatusCode = fail is ObjectResult o && o.StatusCode is int s ? s : 401; return; }
        var message = request.Message?.Trim() ?? "";
        if (message.Length == 0) { Response.StatusCode = 400; return; }

        ClaudeChatSummary? chat;
        var isNew = id == "new";
        if (isNew)
        {
            var title = message.Length > 60 ? message[..60] + "…" : message;
            chat = await _store.CreateAsync(Guid.NewGuid().ToString(), user!.Id, title, ct);
        }
        else
        {
            chat = await _store.GetAsync(id, user!.Id, ct);
            if (chat is null) { Response.StatusCode = 404; return; }
        }

        Response.ContentType = "text/event-stream; charset=utf-8";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["X-Accel-Buffering"] = "no";
        await WriteAsync("chat", new { chat.Id, chat.Title }, ct);

        await _store.AddMessageAsync(chat.Id, "user", message, ct: ct);
        await foreach (var ev in _cli.SendAsync(chat.Id, resume: !isNew, message, ct))
        {
            switch (ev.Kind)
            {
                case "init": await WriteAsync("init", new { model = ev.Model }, ct); break;
                case "delta": await WriteAsync("delta", new { text = ev.Text }, ct); break;
                case "error":
                    await WriteAsync("error", new { message = ev.Text }, ct);
                    return;
                case "done":
                    var saved = await _store.AddMessageAsync(chat.Id, "assistant", ev.Text ?? "", ev.CostUsd, ev.DurationMs, CancellationToken.None);
                    await WriteAsync("done", new { message = saved, model = ev.Model }, ct);
                    return;
            }
        }
    }

    private async Task WriteAsync(string eventName, object payload, CancellationToken ct)
    {
        var data = JsonSerializer.Serialize(payload, Json);
        await Response.WriteAsync($"event: {eventName}\ndata: {data}\n\n", Encoding.UTF8, ct);
        await Response.Body.FlushAsync(ct);
    }
}
