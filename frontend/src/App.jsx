import { useState } from 'react';
import ChatPanel from './components/ChatPanel.jsx';
import DashboardRenderer from './components/DashboardRenderer.jsx';

export default function App() {
  const [messages, setMessages] = useState([]);
  const [dashboard, setDashboard] = useState(null);
  const [loading, setLoading] = useState(false);

  async function ask(question) {
    setMessages((prev) => [...prev, { role: 'user', text: question }]);
    setLoading(true);
    try {
      const res = await fetch('/api/chat', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ message: question }),
      });
      const payload = await res.json().catch(() => null);
      if (!res.ok || !payload?.dashboard) {
        const error = payload?.error || `Request failed (${res.status})`;
        setMessages((prev) => [...prev, { role: 'assistant', text: error, isError: true }]);
        return;
      }
      setDashboard(payload.dashboard);
      setMessages((prev) => [...prev, { role: 'assistant', text: payload.dashboard.summary }]);
    } catch (err) {
      setMessages((prev) => [
        ...prev,
        { role: 'assistant', text: `Could not reach the backend: ${err.message}`, isError: true },
      ]);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="flex h-screen">
      <aside className="flex w-[35%] min-w-[320px] flex-col border-r border-stone-200 bg-white">
        <ChatPanel messages={messages} loading={loading} onSend={ask} />
      </aside>
      <main className="w-[65%] flex-1 overflow-y-auto">
        <DashboardRenderer dashboard={dashboard} loading={loading} />
      </main>
    </div>
  );
}
