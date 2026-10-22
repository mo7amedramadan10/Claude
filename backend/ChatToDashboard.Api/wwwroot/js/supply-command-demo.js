/* بيانات تجريبية ثابتة لمعاينة "مركز قيادة سلسلة الإمداد" في معرض النماذج فقط — تكرار
   أرقام النموذج الأولي نفسها (مُعاد تبويبها حسب نوع الطلب reg/promo بدل عامل تحجيم f()،
   ومُفصَّلة شهريًا بدل أرقام سنوية/ربعية جاهزة) لتطابق window.JEEM_COMMAND_MOUNT. */
(function () {
  const CH = ['التجزئة', 'المطاعم والفنادق', 'الموزعون', 'التجارة الإلكترونية'];
  const WH = ['المستودع الغربي', 'المستودع الشرقي', 'مستودع الشمال'];
  const CAT = ['المخبوزات', 'المشروبات', 'الألبان', 'المجمّدات', 'الوجبات الخفيفة'];
  const SUP = ['أطعمة القيمة', 'لوجستيات المجمّدات', 'ألبان الشمال', 'منتجات البلطيق', 'مشروبات ممتازة'];
  const PRI = ['استراتيجيون', 'منخفضة', 'عاديون'];
  const YEARS = ['2024', '2025'];

  const kpis = {
    reg: { revenue: 528080, revenuePrevYear: 267960, revenueDeltaPct: 97.1, grossProfit: 185310, grossProfitMarginPct: 35.09, grossProfitDeltaPct: 0.1, otifPct: 92.53, otifTargetPct: 95, lateOrders: 142, avgShipDays: 7.51, stockoutPct: 3.26, stockoutOrdersAffected: 62 },
    promo: { revenue: 179547, revenuePrevYear: 91106, revenueDeltaPct: 97.1, grossProfit: 63005, grossProfitMarginPct: 35.09, grossProfitDeltaPct: 0.1, otifPct: 89.1, otifTargetPct: 95, lateOrders: 48, avgShipDays: 7.51, stockoutPct: 4.4, stockoutOrdersAffected: 21 },
  };

  function monthly(r, target, rate) {
    const out = [];
    for (let yi = 0; yi < 2; yi++) {
      if (rate) {
        const base = yi === 0 ? target - 1.5 : target;
        for (let m = 0; m < 12; m++) out.push(+(base * (0.97 + r() * 0.06)).toFixed(2));
      } else {
        const yearSum = yi === 0 ? target * 0.51 : target;
        const w = Array.from({ length: 12 }, () => 0.7 + r() * 0.6), ws = w.reduce((a, b) => a + b, 0);
        w.forEach(x => out.push(Math.round(yearSum * x / ws)));
      }
    }
    return out;
  }
  function trendFor(ord) {
    const k = kpis[ord];
    return {
      rev: { months: monthly(window.DK.rng('sc-rev-' + ord), k.revenue, false) },
      gp: { months: monthly(window.DK.rng('sc-gp-' + ord), k.grossProfit, false) },
      otif: { months: monthly(window.DK.rng('sc-otif-' + ord), k.otifPct, true) },
      late: { months: monthly(window.DK.rng('sc-late-' + ord), k.lateOrders, false) },
      so: { months: monthly(window.DK.rng('sc-so-' + ord), k.stockoutPct, true) },
    };
  }

  window.JEEM_COMMAND_DEMO = {
    id: 'supply-command', name: 'مركز قيادة سلسلة الإمداد',
    years: YEARS, channels: CH, warehouses: WH, categories: CAT, suppliers: SUP, customerPriorities: PRI,
    kpis,
    trend: { reg: trendFor('reg'), promo: trendFor('promo') },
    otif: { reg: { onTime: 1759, late: 142 }, promo: { onTime: 590, late: 72 } },
    lateByCategory: { reg: [38, 31, 27, 29, 17], promo: [13, 11, 9, 10, 6] },
    channelRevenue: { reg: [212000, 140000, 105000, 71000], promo: [72080, 47600, 35700, 24140] },
    channelGrossProfit: { reg: [70000, 51000, 40000, 24000], promo: [23800, 17340, 13600, 8160] },
    otifByPriority: { reg: [94.1, 92.6, 91.8], promo: [91.1, 89.6, 88.8] },
    otifByWarehouse: { reg: [95.4, 82.6, 88.5], promo: [92.9, 80.1, 86.0] },
    revenueAtRisk: {
      ch: { reg: [168000, 120000, 101000, 74000], promo: [57120, 40800, 34340, 25160] },
      wh: { reg: [96000, 210000, 141000], promo: [32640, 71400, 47940] },
      cat: { reg: [52000, 88000, 74000, 131000, 41000], promo: [17680, 29920, 25160, 44540, 13940] },
      sup: { reg: [148000, 121000, 77000, 46000, 32000], promo: [50320, 41140, 26180, 15640, 10880] },
    },
    lossMatrixPct: {
      reg: [[3.1, 5.2, 4.4, 8.9, 2.0], [2.4, 3.9, 6.1, 7.2, 1.7], [1.8, 4.6, 3.2, 9.8, 2.6], [1.2, 2.2, 2.9, 5.1, 1.4]],
      promo: [[4.0, 6.8, 5.7, 11.6, 2.6], [3.1, 5.1, 7.9, 9.4, 2.2], [2.3, 6.0, 4.2, 12.7, 3.4], [1.6, 2.9, 3.8, 6.6, 1.8]],
    },
    efficiencyWaterfallVals: {
      reg: [528080, 167500, -15210, -937, -863, -796, -342770, 335004],
      promo: [179547, 56950, -5171, -319, -293, -271, -116542, 113901],
    },
    marginByDeliveryPct: { reg: { onTime: 35.4, failed: 12.1 }, promo: { onTime: 35.4, failed: 12.1 } },
    profitRiskByChannel: { reg: [42000, 31000, 18000, 9000], promo: [14280, 10540, 6120, 3060] },
  };
})();
