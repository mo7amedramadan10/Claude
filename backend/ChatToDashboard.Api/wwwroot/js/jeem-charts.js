/* جيم — رسم عناصر اللوحة (ApexCharts)
   في التطبيق الفعلي: البيانات تأتي من JSON اللوحة اللي يولّدها الـ AI (widgets[]).
   هنا البيانات تجريبية للعرض فقط. */
(function () {
  if (!window.ApexCharts) return;
  const css = getComputedStyle(document.documentElement);
  const v = n => css.getPropertyValue(n).trim();
  const C = { c1: v('--chart-1'), c2: v('--chart-2'), c3: v('--chart-3'), c4: v('--chart-4'), grid: v('--chart-grid'), ink2: v('--ink-2'), ink3: v('--ink-3') };
  const font = v('--font');
  const fmt = n => n.toLocaleString('en-US', { maximumFractionDigits: 1 });

  const base = {
    chart: { fontFamily: font, toolbar: { show: false }, zoom: { enabled: false }, parentHeightOffset: 0,
             animations: { enabled: true, easing: 'easeinout', speed: 700 } },
    grid: { borderColor: C.grid, strokeDashArray: 0, padding: { left: 8, right: 8 } },
    dataLabels: { enabled: false },
    legend: { show: false },
    tooltip: { theme: 'light', style: { fontFamily: font, fontSize: '12px' } },
    states: { hover: { filter: { type: 'darken', value: 0.92 } } }
  };
  const axisLabel = { style: { colors: C.ink3, fontSize: '12px', fontFamily: font } };
  const mk = (sel, opts) => { const el = document.querySelector(sel); if (el) new ApexCharts(el, Object.assign({}, base, opts)).render(); };

  /* ---- مؤشرات مصغّرة (Sparklines) ---- */
  const spark = (sel, data, color) => mk(sel, {
    chart: Object.assign({}, base.chart, { type: 'area', height: 56, sparkline: { enabled: true } }),
    series: [{ name: 'القيمة', data }],
    stroke: { width: 2, curve: 'smooth' },
    fill: { type: 'gradient', gradient: { shadeIntensity: 0, opacityFrom: .22, opacityTo: 0, stops: [0, 100] } },
    colors: [color],
    tooltip: { enabled: false }
  });
  spark('#sp-sales',  [4.6, 4.9, 5.2, 5.0, 5.6, 5.9, 6.1, 6.4, 6.9], C.c1);
  spark('#sp-orders', [1310, 1360, 1420, 1390, 1500, 1550, 1580, 1630, 1680], C.c1);
  spark('#sp-aov',    [3.85, 3.83, 3.84, 3.80, 3.79, 3.78, 3.76, 3.75, 3.74], C.c1);

  /* ---- المبيعات الشهرية مقابل المستهدف ---- */
  const months = ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو', 'يوليو', 'أغسطس', 'سبتمبر'];
  mk('#ch-trend', {
    chart: Object.assign({}, base.chart, { type: 'area', height: 290 }),
    series: [
      { name: 'المبيعات الفعلية', data: [4.6, 4.9, 5.2, 5.0, 5.6, 5.9, 6.1, 6.4, 6.9] },
      { name: 'المستهدف', data: [5.0, 5.2, 5.4, 5.6, 5.8, 6.0, 6.2, 6.4, 6.6] }
    ],
    colors: [C.c1, '#8B9299'],
    stroke: { width: [2.5, 2], curve: 'smooth', dashArray: [0, 5] },
    fill: { type: ['gradient', 'solid'], opacity: [1, 0], gradient: { shadeIntensity: 0, opacityFrom: .18, opacityTo: 0, stops: [0, 100] } },
    markers: { size: 0, hover: { size: 6 }, strokeColors: '#fff', strokeWidth: 2 },
    xaxis: { categories: months, labels: axisLabel, axisBorder: { show: false }, axisTicks: { show: false }, crosshairs: { stroke: { color: '#C5CBD1', dashArray: 3 } } },
    yaxis: { labels: Object.assign({ formatter: n => fmt(n) + 'م' }, axisLabel), min: 4, max: 7.2, tickAmount: 4 },
    tooltip: Object.assign({}, base.tooltip, { shared: true, y: { formatter: n => fmt(n) + ' مليون ر.س' } }),
    annotations: { points: [{ x: 'سبتمبر', y: 6.9, marker: { size: 6, fillColor: C.c4, strokeColor: '#fff', strokeWidth: 3 },
      label: { text: 'تجاوز المستهدف', offsetY: -6, borderWidth: 0, style: { background: C.c1, color: '#fff', fontFamily: font, fontSize: '11px', padding: { left: 8, right: 8, top: 3, bottom: 4 } } } }] }
  });

  /* المبيعات حسب المنطقة: مرسومة HTML/CSS مباشرة (أوضح في الاتجاه من اليمين لليسار) — راجع .hbars في app.css */

  /* ---- توزيع المبيعات حسب القطاع ---- */
  mk('#ch-sector', {
    chart: Object.assign({}, base.chart, { type: 'donut', height: 210 }),
    series: [21.3, 14.2, 10.1, 5.0],
    labels: ['التجزئة', 'الصناعي', 'الحكومي', 'الضيافة'],
    colors: [C.c1, C.c2, C.c3, C.c4],
    stroke: { width: 2, colors: ['#fff'] },
    plotOptions: { pie: { expandOnClick: false, donut: { size: '72%', labels: { show: true,
      name: { fontFamily: font, fontSize: '12px', color: C.ink3, offsetY: 18 },
      value: { fontFamily: font, fontSize: '24px', fontWeight: 700, color: v('--ink'), offsetY: -14, formatter: n => fmt(+n) + 'م' },
      total: { show: true, label: 'الإجمالي', fontFamily: font, color: C.ink3, formatter: () => '50.6م' } } } } },
    tooltip: Object.assign({}, base.tooltip, { y: { formatter: n => fmt(n) + ' مليون ر.س' } })
  });
})();
