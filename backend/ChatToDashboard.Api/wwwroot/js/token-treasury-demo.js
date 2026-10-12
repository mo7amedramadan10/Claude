/* مكتبة بيانات تجريبية ثابتة — معاينة المعرض فقط (openTreasuryGalDemo في app.js). لا صلة لها
   بمسار بناء اللوحات الحقيقي. الشكل مطابق لما تتوقعه token-treasury.js's JEEM_TREASURY_MOUNT. */
(function () {
  let sd = 5; const rnd = () => (sd = (sd * 16807) % 2147483647) / 2147483647;

  const assets = [
    { symbol: 'ETH', name: 'إيثريوم', value: 611.85, cls: 'eth', decimals: 2 },
    { symbol: 'SAR', name: 'ريال رقمي', value: 546013875 / 100, cls: 'sar', decimals: 0 },
    { symbol: 'USDT', name: 'دولار مستقر', value: 411406.08, cls: 'usdt', decimals: 2 },
  ];
  const side = [
    { label: 'القيمة السوقية', value: 2.34e6, icon: 'chart' },
    { label: 'القيمة المقفلة', value: 17.01e6, icon: 'shield' },
    { label: 'عدد الحائزين', value: 3400, icon: 'users', integer: true },
    { label: 'حجم 24 ساعة', value: 314.74e3, icon: 'history' },
  ];
  const participation = { 2022: [1800, 2600, 4400, 2500], 2023: [3600, 6900, 9400, 13100], 2024: [12000, 14800, 16300, 19600] };
  const revenueSeries = {
    w: { v: [21000, 28000, 52000, 66000, 71000, 90292, 74000, 95000, 88000, 128000], l: ['السبت', 'الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة'] },
    m: { v: [210, 260, 240, 330, 360, 410, 395, 470, 520, 610].map(v => v * 1000), l: ['أ1', 'أ2', 'أ3', 'أ4'] },
    y: { v: [1.2, 1.5, 1.4, 2.1, 2.6, 2.4, 3.1, 3.6, 3.4, 4.2, 4.8, 5.3].map(v => v * 1e6), l: ['يناير', 'أبريل', 'يوليو', 'أكتوبر'] },
  };
  const n = 120, tvlVals = []; let x = 1.2;
  for (let i = 0; i < n; i++) { x = Math.max(.4, x * (1 + (rnd() - .48) * .12)); tvlVals.push(x * 1e6); }
  const tvlHistory = tvlVals.map((value, i) => {
    const d = new Date(2023, 1, 21); d.setDate(d.getDate() - (n - 1 - i) * 9);
    return { date: d.toLocaleDateString('ar-SA-u-ca-gregory-nu-latn', { year: 'numeric', month: 'short', day: 'numeric' }), value };
  });

  window.JEEM_TREASURY_DEMO = {
    id: 'token-treasury', name: 'خزينة الأصول الرقمية',
    currency: { rate: 3.75, symbol: 'ر.س' },
    totalAssets: 1618801.89,
    assets, side, participation, revenueSeries,
    revenue: { w: [2079, 0.1285], m: [9140, 0.5612], y: [104220, 6.7140] },
    supply: { circulating: 675152272, burned: 1252873 },
    tvlHistory, tvlTicks: ['2020', '2021', '2022', '2023'],
  };
})();
