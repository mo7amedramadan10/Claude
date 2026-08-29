import {
  ResponsiveContainer, LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip,
} from 'recharts';
import Card, { SERIES_COLORS, GRID_COLOR, AXIS_COLOR, inferKeys } from './Card.jsx';

export default function LineChartCard({ widget }) {
  const { rows, xKey, yKey } = inferKeys(widget);

  return (
    <Card title={widget.title}>
      <div className="h-64">
        <ResponsiveContainer width="100%" height="100%">
          <LineChart data={rows} margin={{ top: 8, right: 8, bottom: 0, left: 0 }}>
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
              contentStyle={{ borderRadius: 12, border: `1px solid ${GRID_COLOR}`, fontSize: 13 }}
            />
            <Line
              type="monotone"
              dataKey={yKey}
              stroke={SERIES_COLORS[0]}
              strokeWidth={2}
              dot={false}
              activeDot={{ r: 4 }}
            />
          </LineChart>
        </ResponsiveContainer>
      </div>
    </Card>
  );
}
