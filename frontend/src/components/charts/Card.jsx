export const SERIES_COLORS = ['#2a78d6', '#eb6834', '#1baf7a', '#eda100', '#e87ba4', '#008300'];

export const GRID_COLOR = '#e1e0d9';
export const AXIS_COLOR = '#898781';

/** Infers x/y keys for bar/line widgets when the backend omits them. */
export function inferKeys(widget) {
  const rows = Array.isArray(widget.data) ? widget.data : [];
  const first = rows[0] ?? {};
  const keys = Object.keys(first);
  const xKey = widget.xKey || keys.find((k) => typeof first[k] !== 'number') || keys[0];
  const yKey = widget.yKey || keys.find((k) => k !== xKey && typeof first[k] === 'number') || keys[1];
  return { rows, xKey, yKey };
}

export default function Card({ title, children }) {
  return (
    <div className="h-full rounded-2xl border border-stone-200 bg-white p-5 shadow-sm">
      <h3 className="mb-3 text-sm font-medium text-stone-600">{title}</h3>
      {children}
    </div>
  );
}
