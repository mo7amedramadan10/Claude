import Card from './Card.jsx';

function formatValue(value) {
  if (typeof value !== 'number') return String(value ?? '—');
  if (Math.abs(value) >= 1_000_000) return `${(value / 1_000_000).toFixed(1)}M`;
  if (Math.abs(value) >= 10_000) return `${(value / 1_000).toFixed(1)}K`;
  return value.toLocaleString(undefined, { maximumFractionDigits: 2 });
}

export default function KpiCard({ widget }) {
  const entry = Array.isArray(widget.data) ? widget.data[0] : null;
  const value = entry?.value ?? Object.values(entry ?? {}).find((v) => typeof v === 'number');
  const label = entry?.label;

  return (
    <Card title={widget.title}>
      <div className="flex h-40 flex-col items-start justify-center">
        <span className="text-5xl font-semibold tracking-tight text-stone-900">{formatValue(value)}</span>
        {label && <span className="mt-2 text-sm text-stone-500">{label}</span>}
      </div>
    </Card>
  );
}
