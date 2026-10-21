// ---------- live-push client (INTERACTIVE-HANDOFF.md §7's "hub /hubs/live") ----------
// Talks to the plain WebSocket endpoint in Program.cs ("/ws/live/{channel}") — see
// Live/LiveChannelHub.cs for why this is a hand-rolled client instead of the official
// @microsoft/signalr browser bundle (not available to vendor in this environment).
// Auto-reconnects with backoff; the caller never has to think about connection state —
// just subscribe() and handle messages, same as a SignalR hub connection would feel.
(function () {
  function subscribe(channel, onMessage, onStatus) {
    let socket = null;
    let closedByCaller = false;
    let attempt = 0;

    function connect() {
      if (closedByCaller) return;
      const proto = location.protocol === 'https:' ? 'wss:' : 'ws:';
      socket = new WebSocket(`${proto}//${location.host}/ws/live/${encodeURIComponent(channel)}`);
      socket.addEventListener('open', () => { attempt = 0; onStatus && onStatus('open'); });
      socket.addEventListener('message', e => {
        try { onMessage(JSON.parse(e.data)); } catch { /* malformed frame — ignore it */ }
      });
      socket.addEventListener('close', () => {
        onStatus && onStatus('closed');
        if (closedByCaller) return;
        // Capped exponential backoff — a dashboard left open overnight shouldn't hammer the
        // server every second once a connection has failed repeatedly.
        const delay = Math.min(30000, 1000 * Math.pow(1.6, attempt++));
        setTimeout(connect, delay);
      });
      socket.addEventListener('error', () => { try { socket.close(); } catch {} });
    }
    connect();

    return {
      close() { closedByCaller = true; try { socket && socket.close(); } catch {} },
    };
  }

  window.JeemLive = { subscribe };
})();
