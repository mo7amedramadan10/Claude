/* بيانات تجريبية ثابتة لمعاينة "الإدارة المالية" في معرض النماذج فقط — تكرار أرقام
   النموذج الأولي نفسها (التدفق النقدي والأرباح/الخسائر عبر window.DK.rng بنفس البذور
   والمعاملات) مُعاد تبويبها إلى شكل data.cashFlow/profitLoss/regions المطلوب لـ
   window.JEEM_FINANCE_MOUNT بدل التوليد المباشر داخل المحرك. */
(function () {
  function cashSeries(key, n, scale) {
    const r = window.DK.rng(key), points = [];
    for (let i = 0; i < n; i++) points.push({ in: Math.round((40000 + r() * 50000) * scale), out: Math.round((25000 + r() * 45000) * scale) });
    return { opening: 120000 * scale, points };
  }
  function plSeries(key, n, scale) {
    const r = window.DK.rng(key); let a = 120, b = 95; const revenue = [], expenses = [];
    for (let i = 0; i < n; i++) { a += 5 + r() * 18 - (i === 5 ? 20 : 0); b += 3 + r() * 13; revenue.push(Math.round(a * scale * 1000)); expenses.push(Math.round(b * scale * 1000)); }
    return { revenue, expenses };
  }

  window.JEEM_FINANCE_DEMO = {
    id: 'finance-ops', name: 'الإدارة المالية',
    kpis: {
      rev: { value: 45672000, deltaPct: 5.0 }, exp: { value: 28903000, deltaPct: -3.2 }, pro: { value: 34215000, deltaPct: 18.0 },
    },
    cashFlow: { d: cashSeries('cfd', 31, 1), w: cashSeries('cfw', 12, 6), m: cashSeries('cfm', 12, 26) },
    profitLoss: { h: plSeries('plh', 13, 1), d: plSeries('pld', 30, 24), w: plSeries('plw', 12, 160), m: plSeries('plm', 12, 700) },
    regions: [
      { name: 'أمريكا الشمالية', gridCol: 1, gridRow: 1, gridSpan: 2, inventoryValue: 8.4e6, deltaPct: 4.1, activeItems: Math.round(8.4 * 37) },
      { name: 'أوروبا', gridCol: 3, gridRow: 1, gridSpan: 1, inventoryValue: 12.6e6, deltaPct: 6.8, activeItems: Math.round(12.6 * 37) },
      { name: 'آسيا', gridCol: 4, gridRow: 1, gridSpan: 2, inventoryValue: 15.2e6, deltaPct: 9.3, activeItems: Math.round(15.2 * 37) },
      { name: 'الشرق الأوسط', gridCol: 4, gridRow: 2, gridSpan: 1, inventoryValue: 9.7e6, deltaPct: 12.4, activeItems: Math.round(9.7 * 37) },
      { name: 'أفريقيا', gridCol: 3, gridRow: 2, gridSpan: 1, inventoryValue: 4.1e6, deltaPct: -2.6, activeItems: Math.round(4.1 * 37) },
      { name: 'أمريكا الجنوبية', gridCol: 2, gridRow: 3, gridSpan: 1, inventoryValue: 3.3e6, deltaPct: 1.8, activeItems: Math.round(3.3 * 37) },
      { name: 'أوقيانوسيا', gridCol: 5, gridRow: 3, gridSpan: 1, inventoryValue: 2.2e6, deltaPct: -0.9, activeItems: Math.round(2.2 * 37) },
    ],
    automationInsights: [
      { title: 'تغطية الأتمتة', desc: 'نسبة العمليات المؤتمتة بالكامل', pct: 72 },
      { title: 'مسار الأعمال – أ', desc: 'تنبيه عند انخفاض الهامش', pct: 54 },
      { title: 'مطابقة الفواتير', desc: 'مطابقة آلية مع أوامر الشراء', pct: 88 },
    ],
    automationExceptions: [
      { title: 'تغطية الأتمتة', desc: 'استثناءات قيد المعالجة', pct: 31 },
      { title: 'مسار الأعمال – أ', desc: 'تنبيه عند تجاوز الحد', pct: 47 },
      { title: 'مدفوعات متأخرة', desc: 'تصعيد بعد 15 يومًا', pct: 19 },
    ],
    operationsFlowEfficiency: [96, 72, 64, 81, 88],
  };
})();
