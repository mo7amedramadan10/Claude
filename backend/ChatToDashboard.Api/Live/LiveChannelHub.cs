using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace ChatToDashboard.Api.Live;

/// <summary>
/// Minimal self-hosted live-push broker for INTERACTIVE-HANDOFF.md §7's "hub /hubs/live"
/// concept — plain ASP.NET Core WebSockets (built into the shared framework, no extra
/// package) rather than SignalR's own hub abstraction, since this deployment vendors every
/// client-side library itself instead of pulling from a public CDN (see wwwroot/lib's other
/// entries) and the official @microsoft/signalr browser bundle isn't available to vendor
/// here. One broadcast group per caller-defined "channel" string (convention:
/// "{projectId}:{key}", e.g. "proj123:treasury-funding") — see Program.cs's "/ws/live/
/// {channel}" endpoint for the connection handshake and per-project access check, and
/// wwwroot/js/jeem-live.js for the matching browser client.
/// </summary>
public class LiveChannelHub
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<WebSocket, byte>> _channels = new();

    /// <summary>Owns one accepted socket until it closes or the request is cancelled —
    /// registers it in <paramref name="channel"/>'s group first so a broadcast can never
    /// race ahead of a socket that's about to join, then blocks reading (and discarding) any
    /// inbound frames purely to detect the client closing, since this channel is push-only
    /// from the server.</summary>
    public async Task RunAsync(string channel, WebSocket socket, CancellationToken ct)
    {
        var group = _channels.GetOrAdd(channel, _ => new());
        group[socket] = 0;
        try
        {
            var buffer = new byte[4096];
            while (socket.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                var result = await socket.ReceiveAsync(buffer, ct);
                if (result.MessageType == WebSocketMessageType.Close) break;
            }
        }
        catch (OperationCanceledException) { }
        catch (WebSocketException) { }
        finally
        {
            group.TryRemove(socket, out _);
            if (group.IsEmpty) _channels.TryRemove(channel, out _);
            if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                try { await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None); }
                catch { /* already gone */ }
            }
        }
    }

    /// <summary>Pushes a JSON payload to every socket currently subscribed to
    /// <paramref name="channel"/>. The caller decides the channel naming and payload shape
    /// (e.g. treasury-funding's price-tick JSON) — this broker only moves bytes. A no-op
    /// (not an error) when nobody is currently listening.</summary>
    public async Task BroadcastAsync(string channel, string json, CancellationToken ct = default)
    {
        if (!_channels.TryGetValue(channel, out var group) || group.IsEmpty) return;
        var bytes = Encoding.UTF8.GetBytes(json);
        foreach (var socket in group.Keys)
        {
            if (socket.State != WebSocketState.Open) continue;
            try { await socket.SendAsync(bytes, WebSocketMessageType.Text, true, ct); }
            catch { /* a dead socket is reaped by its own RunAsync loop, not here */ }
        }
    }

    public int SubscriberCount(string channel) => _channels.TryGetValue(channel, out var g) ? g.Count : 0;
}
