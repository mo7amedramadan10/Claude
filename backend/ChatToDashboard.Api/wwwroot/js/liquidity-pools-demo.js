/* مكتبة بيانات تجريبية ثابتة — معاينة المعرض فقط (openPoolsGalDemo في app.js). لا صلة لها
   بمسار بناء اللوحات الحقيقي. الشكل مطابق لما تتوقعه liquidity-pools.js's JEEM_POOLS_MOUNT. */
(function () {
  let sd = 11; const rnd = () => (sd = (sd * 16807) % 2147483647) / 2147483647;
  const TOK = { USDC: 210, SUI: 190, USDT: 160, CETUS: 170, WETH: 230, WBTC: 30, DEEP: 270, NAVX: 300 };
  const POOLS = [
    ['USDC', 'SUI', .25, 'رائجة', true, 10684011.54, 10766872.37, 26917.18, 161.67, [3.62, 3.48, 3.80]],
    ['CETUS', 'SUI', .25, 'رائجة', true, 8420330.10, 6210448.90, 15526.12, 128.40, [0.071, 0.064, 0.082]],
    ['USDT', 'USDC', .01, 'مستقرة', false, 22451900.75, 18904221.30, 1890.42, 6.12, [1.0003, 0.9995, 1.0008]],
    ['WETH', 'USDC', .05, 'مستقرة', true, 6790110.33, 4120774.62, 2060.39, 24.85, [3420, 3180, 3650]],
    ['SUI', 'USDT', .25, 'رائجة', true, 5318420.05, 3904118.71, 9760.30, 88.20, [3.61, 3.70, 3.95]],
    ['WBTC', 'USDC', .05, 'مستقرة', false, 4402119.80, 2210945.12, 1105.47, 18.40, [64200, 61000, 66800]],
    ['DEEP', 'SUI', 1, 'جديدة', true, 1290442.67, 1840221.04, 18402.21, 312.75, [0.043, 0.047, 0.058]],
    ['NAVX', 'SUI', 1, 'جديدة', true, 880130.42, 640122.88, 6401.23, 204.10, [0.112, 0.095, 0.124]],
    ['USDC', 'USDT', .01, 'مستقرة', false, 310220.11, 82014.55, 8.20, 2.05, [1.0001, 0.9992, 1.0010]],
  ].map(([a, b, fee, label, inc, liq, vol, fees, apr, rg], i) => ({
    id: 'p' + i, a, b, fee, label, inc, liq, vol, fees, apr, price: rg[0], lo: rg[1], hi: rg[2],
  }));
  const positions = [{ pool: 'p0', amt: 12450, lo: 3.45, hi: 3.78, fees: 184.32 }, { pool: 'p4', amt: 4200, lo: 3.75, hi: 4.10, fees: 21.07 }];
  const PER = {
    day: { n: 36, unit: 'ساعة', vol: 24783752516.42, tvl: 153897416.49, cum: 7783752516.42, lab: i => `${String((i * 40 / 60 | 0) % 24).padStart(2, '0')}:${String(i * 40 % 60).padStart(2, '0')}` },
    week: { n: 36, unit: 'يوم', vol: 168204118902.10, tvl: 151204990.12, cum: 7783752516.42, lab: i => `يوم ${i + 1}` },
    month: { n: 36, unit: 'يوم', vol: 702118402557.80, tvl: 148990411.73, cum: 7783752516.42, lab: i => `${(i % 30) + 1} سبتمبر` },
  };
  const series = {}, labels = {}, periods = {};
  Object.keys(PER).forEach(k => {
    series[k] = Array.from({ length: PER[k].n }, (_, i) => Math.round((.25 + rnd() * .75) * (k === 'day' ? 1 : k === 'week' ? 6.8 : 28) * 9e8 * (1 + Math.sin(i / 4) * .25)));
    labels[k] = Array.from({ length: PER[k].n }, (_, i) => PER[k].lab(i));
    periods[k] = { tvl: PER[k].tvl, cum: PER[k].cum, vol: PER[k].vol };
  });

  window.JEEM_POOLS_DEMO = {
    id: 'liquidity-pools', name: 'مجمّعات السيولة المركّزة',
    tokenHues: TOK, incentiveToken: 'CETUS', pools: POOLS, positions, periods, series, labels,
  };
})();
