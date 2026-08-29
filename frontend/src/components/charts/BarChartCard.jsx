import {
  ResponsiveContainer, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip,
} from 'recharts';
import Card, { SERIES_COLORS, GRID_COLOR, AXIS_COLOR, inferKeys } from './Card.jsx';

export default function BarChartCard({ widget }) {
  const { rows, xKey, yKey } = inferKeys(widget);

  return (
    <Card title={widget.title}>
      <div className="h-64">
        <ResponsiveContainer width="100%" height="100%">
          <BarChart data={rows} margin={{ top: 8, right: 8, bottom: 0, left: 0 }} barCategoryGap="25%">
            <CartesianGrid vertical={false} stroke={GRID_COLOR} />
            <XAxis
              dataKey={xKey}
              tick={{ fill: AXIS_COLOR, fontSize: 12 }}
              tickLine={false}
              axisLine={{ stroke: GRID_COLOR }}
            />
            <YAxis
              tick={{ fill: AXIS_COLOR, fontSize: 12 }}
              tickLine={false}
              axisLine={false}
              width={56}
            />
            <Tooltip
              cursor={{ fill: 'rgba(0,0,0,0.04)' }}
              contentStyle={{ borderRadius: 12, border: `1px solid ${GRID_COLOR}`, fontSize: 13 }}
            />
            <Bar dataKey={yKey} fill={SERIES_COLORS[0]} radius={[4, 4, 0, 0]} maxBarSize={48} />
          </BarChart>
        </ResponsiveContainer>
      </div>
    </Card>
  );
}
