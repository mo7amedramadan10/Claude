import { useEffect, useRef, useState } from 'react';

export default function ChatPanel({ messages, loading, onSend }) {
  const [draft, setDraft] = useState('');
  const bottomRef = useRef(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, loading]);

  function submit(e) {
    e.preventDefault();
    const question = draft.trim();
    if (!question || loading) return;
    setDraft('');
    onSend(question);
  }

  return (
    <div className="flex h-full flex-col">
      <header className="border-b border-stone-200 px-5 py-4">
        <h1 className="text-lg font-semibold text-stone-900">Chat to Dashboard</h1>
        <p className="text-sm text-stone-500">Ask a question about your data</p>
      </header>

      <div className="flex-1 space-y-3 overflow-y-auto px-5 py-4">
        {messages.length === 0 && (
          <div className="rounded-2xl bg-stone-50 p-4 text-sm text-stone-500">
            Try: <span className="italic">“What is total revenue by region?”</span> or{' '}
            <span className="italic">“Show monthly sales trends by category.”</span>
          </div>
        )}
        {messages.map((message, i) => (
          <div key={i} className={message.role === 'user' ? 'flex justify-end' : 'flex justify-start'}>
            <div
              className={
                message.role === 'user'
                  ? 'max-w-[85%] rounded-2xl rounded-br-md bg-blue-600 px-4 py-2.5 text-sm text-white shadow-sm'
                  : message.isError
                    ? 'max-w-[85%] rounded-2xl rounded-bl-md border border-red-200 bg-red-50 px-4 py-2.5 text-sm text-red-700 shadow-sm'
                    : 'max-w-[85%] rounded-2xl rounded-bl-md bg-stone-100 px-4 py-2.5 text-sm text-stone-800 shadow-sm'
              }
            >
              {message.text}
            </div>
          </div>
        ))}
        {loading && (
          <div className="flex justify-start">
            <div className="rounded-2xl rounded-bl-md bg-stone-100 px-4 py-3 shadow-sm">
              <span className="flex gap-1">
                <span className="h-2 w-2 animate-bounce rounded-full bg-stone-400 [animation-delay:0ms]" />
                <span className="h-2 w-2 animate-bounce rounded-full bg-stone-400 [animation-delay:150ms]" />
                <span className="h-2 w-2 animate-bounce rounded-full bg-stone-400 [animation-delay:300ms]" />
              </span>
            </div>
          </div>
        )}
        <div ref={bottomRef} />
      </div>

      <form onSubmit={submit} className="border-t border-stone-200 p-4">
        <div className="flex gap-2">
          <input
            value={draft}
            onChange={(e) => setDraft(e.target.value)}
            placeholder="Ask about your data…"
            className="flex-1 rounded-xl border border-stone-300 px-4 py-2.5 text-sm outline-none transition focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
          />
          <button
            type="submit"
            disabled={loading || !draft.trim()}
            className="rounded-xl bg-blue-600 px-4 py-2.5 text-sm font-medium text-white shadow-sm transition hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-40"
          >
            Send
          </button>
        </div>
      </form>
    </div>
  );
}
