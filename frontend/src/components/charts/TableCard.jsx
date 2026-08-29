import Card from './Card.jsx';

function formatCell(value) {
  if (value === null || value === undefined) return '—';
  if (typeof value === 'number') return value.toLocaleString(undefined, { maximumFractionDigits: 2 });
  return String(value);
}

export default function TableCard({ widget }) {
  const rows = Array.isArray(widget.data) ? widget.data : [];
  const columns = rows.length > 0 ? Object.keys(rows[0]) : [];

  return (
    <Card title={widget.title}>
      <div className="max-h-80 overflow-auto rounded-xl border border-stone-200">
        <table className="w-full text-sm">
          <thead className="sticky top-0 bg-stone-50 text-left text-stone-600">
            <tr>
              {columns.map((column) => (
                <th key={column} className="whitespace-nowrap px-4 py-2.5 font-medium">
                  {column}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-stone-100 text-stone-800">
            {rows.map((row, i) => (
              <tr key={i} className="hover:bg-stone-50">
                {columns.map((column) => (
                  <td
                    key={column}
                    className={`whitespace-nowrap px-4 py-2 ${typeof row[column] === 'number' ? 'text-right tabular-nums' : ''}`}
                  >
                    {formatCell(row[column])}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </Card>
  );
}
