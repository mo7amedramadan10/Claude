import KpiCard from './charts/KpiCard.jsx';
import BarChartCard from './charts/BarChartCard.jsx';
import LineChartCard from './charts/LineChartCard.jsx';
import PieChartCard from './charts/PieChartCard.jsx';
import TableCard from './charts/TableCard.jsx';

const WIDGETS = {
  kpi: KpiCard,
  bar: BarChartCard,
  line: LineChartCard,
  pie: PieChartCard,
  table: TableCard,
};

function Skeleton() {
  return (
    <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
      {[0, 1, 2, 3].map((i) => (
        <div key={i} className="animate-pulse rounded-2xl border border-stone-200 bg-white p-5 shadow-sm">
          <div className="mb-4 h-4 w-1/3 rounded bg-stone-200" />
          <div className="h-48 rounded-xl bg-stone-100" />
        </div>
      ))}
    </div>
  );
}

export default function DashboardRenderer({ dashboard, loading }) {
  // Keep the previous dashboard visible while a new one is loading; only show
  // the skeleton when there is nothing to show yet.
  if (!dashboard) {
    return (
      <div className="p-6">
        {loading ? (
          <Skeleton />
        ) : (
          <div className="flex h-[80vh] items-center justify-center">
            <div className="max-w-md text-center">
              <div className="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-blue-50 text-2xl">
                📊
              </div>
              <h2 className="text-lg font-semibold text-stone-800">Your dashboard will appear here</h2>
              <p className="mt-1 text-sm text-stone-500">
                Ask a question in the chat and Claude will query your data and build charts for the answer.
              </p>
            </div>
          </div>
        )}
      </div>
    );
  }

  return (
    <div className={`p-6 transition-opacity ${loading ? 'opacity-60' : 'opacity-100'}`}>
      <p className="mb-5 rounded-2xl border border-stone-200 bg-white px-5 py-4 text-sm text-stone-700 shadow-sm">
        {dashboard.summary}
      </p>
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        {dashboard.widgets.map((widget, i) => {
          const Widget = WIDGETS[widget.type?.toLowerCase()];
          if (!Widget) return null;
          const span = widget.type === 'table' || widget.type === 'line' ? 'lg:col-span-2' : '';
          return (
            <div key={i} className={span}>
              <Widget widget={widget} />
            </div>
          );
        })}
      </div>
    </div>
  );
}
