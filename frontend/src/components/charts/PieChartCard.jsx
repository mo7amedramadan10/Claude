import { ResponsiveContainer, PieChart, Pie, Cell, Tooltip, Legend } from 'recharts';
import Card, { SERIES_COLORS, GRID_COLOR } from './Card.jsx';

export default function PieChartCard({ widget }) {
  const rows = (Array.isArray(widget.data) ? widget.data : []).map((row) => ({
    label: row.label ?? Object.values(row).find((v) => typeof v === 'string') ?? '—',
    value: row.value ?? Object.values(row).find((v) => typeof v === 'number') ?? 0,
  }));

  return (
    <Card title={widget.title}>
      <div className="h-64">
        <ResponsiveContainer width="100%" height="100%">
          <PieChart>
            <Pie
              data={rows}
              dataKey="value"
              nameKey="label"
              innerRadius="55%"
              outerRadius="80%"
              paddingAngle={2}
              stroke="#ffffff"
              strokeWidth={2}
            >
              {rows.map((_, i) => (
                <Cell key={i} fill={SERIES_COLORS[i % SERIES_COLORS.length]} />
              ))}
            </Pie>
            <Tooltip
              contentStyle={{ borderRadius: 12, border: `1px solid ${GRID_COLOR}`, fontSize: 13 }}
            />
            <Legend
              iconType="circle"
              iconSize={8}
              wrapperStyle={{ fontSize: 13 }}
              formatter={(value) => <span style={{ color: '#52514e' }}>{value}</span>}
            />
          </PieChart>
        </ResponsiveContainer>
      </div>
    </Card>
  );
}
