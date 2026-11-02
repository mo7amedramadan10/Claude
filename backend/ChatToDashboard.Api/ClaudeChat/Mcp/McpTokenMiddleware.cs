using Microsoft.Extensions.Primitives;

namespace ChatToDashboard.Api.ClaudeChat.Mcp;

/// <summary>بوابة خادم MCP الداخلي: يقبل فقط طلبًا يحمل <c>Authorization: Bearer mcp_…</c> لمفتاح جلسة صالح
/// (<see cref="McpSessionTokens"/>)، ويضع الجلسة في <c>HttpContext.Items</c> لتقرأها الأدوات. غير ذلك 401.</summary>
public static class McpTokenMiddleware
{
    public const string Path = "/api/claude-chat/mcp";
    public const string ItemKey = "jeem.mcp.session";

    public static IApplicationBuilder UseMcpSessionGate(this IApplicationBuilder app) =>
        app.UseWhen(ctx => ctx.Request.Path.StartsWithSegments(Path), branch => branch.Use(async (ctx, next) =>
        {
            var tokens = ctx.RequestServices.GetRequiredService<McpSessionTokens>();
            var header = ctx.Request.Headers.Authorization is { Count: > 0 } h ? h.ToString() : "";
            var token = header.StartsWith("Bearer ", StringComparison.Ordinal) ? header["Bearer ".Length..].Trim() : null;
            var session = tokens.Resolve(token);
            if (session is null)
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                ctx.Response.ContentType = "application/json; charset=utf-8";
                await ctx.Response.WriteAsync("{\"error\":\"مفتاح جلسة MCP مفقود أو منتهٍ.\"}");
                return;
            }
            ctx.Items[ItemKey] = session;
            await next();
        }));

    /// <summary>الجلسة الحالية من سياق الطلب (داخل أداة MCP).</summary>
    public static McpSession? CurrentSession(this HttpContext? ctx) =>
        ctx?.Items.TryGetValue(ItemKey, out var s) == true ? s as McpSession : null;
}
