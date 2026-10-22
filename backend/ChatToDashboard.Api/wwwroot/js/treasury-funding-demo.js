/* بيانات تجريبية ثابتة لمعاينة "التمويل والسيولة النقدية" في معرض النماذج فقط — نفس
   أرقام ومنطق توليد النموذج الأولي بالضبط (دالة DK.rng المحجوبة بالبذرة)، بشكل data
   المطلوب لـ window.JEEM_FUNDING_MOUNT. */
(function () {
  const NAVY = '#1F4E9C', GOLD = '#E3A21A';
  const MON = ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو', 'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر'];

  function maturitiesFor(yr) {
    const r = window.DK.rng('mat' + yr);
    const debt = MON.map(() => Math.round(260 + r() * 70) * 1e6), cds = MON.map(() => Math.round(220 + r() * 50) * 1e6);
    const cap = MON.map((_, i) => Math.max(debt[i], cds[i]) + (60 + Math.round(r() * 40)) * 1e6);
    debt[4] += 120e6; cds[4] += 90e6;
    return { months: MON, debt, cds, cap };
  }

  const years = [2025, 2026];
  const byYear = {}; years.forEach(y => { byYear[y] = maturitiesFor(y); });

  const WH_DEF = [['أسعار الأسواق (سايبور 3 أشهر)', GOLD, 2.8], ['سلف البنك المركزي', NAVY, 1.9], ['شهادات إيداع الوسطاء', '#28B6E6', 1.4]];
  const n = 26, whLabels = Array.from({ length: n }, (_, i) => MON[Math.floor(i / 26 * 7)].slice(0, 3) + ' ' + (1 + (i * 7) % 28));
  const whSeries = WH_DEF.map(([name, color, base], k) => { const r = window.DK.rng('wh' + k); let v = base; return { name, color, vals: whLabels.map(() => (v = Math.max(.3, v + (r() - .5) * .35)) && +v.toFixed(2)) }; });

  const cashDefs = [['الرصيد الافتتاحي', 950000, 12, NAVY], ['الرصيد الحالي', 1523125, 12, '#28B6E6'], ['حركة جديدة', 102120, 13, '#E8794A']];
  const cashPosition = cashDefs.map(([label, value, deltaPct, color], i) => {
    const r = window.DK.rng('c' + i);
    const spark = Array.from({ length: 5 }, (_, j) => 30 + j * 14 + r() * 10);
    return { label, value, deltaPct, color, spark };
  });

  const banks = [
    { name: 'البنك الأهلي', value: 412300, color: window.DK.PAL[0] },
    { name: 'مصرف الراجحي', value: 298400, color: '#28B6E6' },
    { name: 'بنك الرياض', value: 176800, color: '#9DB4CC' },
    { name: 'البنك السعودي الفرنسي', value: 121500, color: '#1F4E9C' },
    { name: 'بنك البلاد', value: 64263, color: GOLD },
    { name: 'بنك الجزيرة', value: 52000, color: '#D7DDE5' },
  ];

  const hrs = Array.from({ length: 12 }, (_, i) => `${8 + i}:00`);
  const rIntra = window.DK.rng('intra');
  const today = hrs.map((_, i) => Math.round((80 + Math.sin(i / 2) * 60 + rIntra() * 60) * 1e6));
  const yesterday = today.map(v => Math.round(v * (1.05 + rIntra() * .4)));

  window.JEEM_FUNDING_DEMO = {
    id: 'treasury-funding', name: 'التمويل والسيولة النقدية',
    tick: [
      { name: 'سريع', value: 989859145, deltaPct: 12 },
      { name: 'تسوية الأوراق المالية', value: 990102001, deltaPct: 15 },
      { name: 'نظام المقاصة', value: 117859145, deltaPct: 12 },
      { name: 'سداد', value: 989859145, deltaPct: 12 },
      { name: 'الشيكات', value: 989859145, deltaPct: 12 },
    ],
    maturities: { years, byYear, rates: { 2026: { debtPct: 5.41, cdsPct: 5.13 }, 2025: { debtPct: 5.12, cdsPct: 4.96 } } },
    financingRates: { labels: whLabels, series: whSeries },
    cashPosition,
    banks,
    intraday: { hours: hrs, today, yesterday },
  };
})();
