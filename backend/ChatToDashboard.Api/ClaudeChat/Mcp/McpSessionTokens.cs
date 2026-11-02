using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace ChatToDashboard.Api.ClaudeChat.Mcp;

/// <summary>سياق جلسة أدوات MCP لرسالة واحدة: المستخدم ومشروعه الحالي وعدّاد الاستعلامات.</summary>
public sealed class McpSession
{
    public required string Token { get; init; }
    public required string UserId { get; init; }
    public string? ProjectId { get; init; }
    public DateTime ExpiresAt { get; init; }
    private int _queries;
    public int Queries => _queries;
    public int CountQuery() => Interlocked.Increment(ref _queries);
}

/// <summary>
/// مفاتيح جلسات مؤقتة في الذاكرة فقط (لا تُكتب في القاعدة): مع كل رسالة في «محادثة Claude» يُولَّد مفتاح
/// عشوائي مربوط بـ(المستخدم، المشروع) بعمر قصير، يُمرَّر إلى Claude Code CLI في ترويسة
/// <c>Authorization: Bearer</c> لخادم MCP الداخلي، ويُلغى عند انتهاء الرسالة. أي نداء بلا مفتاح صالح يُرفض.
/// </summary>
public class McpSessionTokens
{
    public const string Prefix = "mcp_";
    public static readonly TimeSpan DefaultLifetime = TimeSpan.FromMinutes(10);
    private readonly ConcurrentDictionary<string, McpSession> _sessions = new(StringComparer.Ordinal);
    private readonly TimeProvider _clock;

    public McpSessionTokens(TimeProvider? clock = null) => _clock = clock ?? TimeProvider.System;

    public McpSession Issue(string userId, string? projectId, TimeSpan? lifetime = null)
    {
        Prune();
        var token = Prefix + Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();
        var session = new McpSession
        {
            Token = token, UserId = userId, ProjectId = projectId,
            ExpiresAt = _clock.GetUtcNow().UtcDateTime + (lifetime ?? DefaultLifetime),
        };
        _sessions[token] = session;
        return session;
    }

    /// <summary>يعيد الجلسة لمفتاح صالح غير منتهٍ، وإلا null.</summary>
    public McpSession? Resolve(string? token)
    {
        if (string.IsNullOrWhiteSpace(token) || !token.StartsWith(Prefix, StringComparison.Ordinal)) return null;
        if (!_sessions.TryGetValue(token, out var session)) return null;
        if (session.ExpiresAt <= _clock.GetUtcNow().UtcDateTime) { _sessions.TryRemove(token, out _); return null; }
        return session;
    }

    public void Revoke(string token) => _sessions.TryRemove(token, out _);

    public int ActiveCount => _sessions.Count;

    private void Prune()
    {
        var now = _clock.GetUtcNow().UtcDateTime;
        foreach (var (key, s) in _sessions)
            if (s.ExpiresAt <= now) _sessions.TryRemove(key, out _);
    }
}
