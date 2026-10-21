/* بيانات تجريبية ثابتة لمعاينة "مخاطر الموردين والتسليم" في معرض النماذج فقط — نفس أرقام
   النموذج الأولي بالضبط (بما فيها سلاسل الاتجاه المصغّرة المحسوبة بنفس دالة DK.rng
   المحجوبة بالبذرة)، بشكل data المطلوب لـ window.JEEM_RISK_MOUNT. */
(function () {
  const G = '#5E8C6A', R = '#D64545', A = '#E09A2B';
  const regions = ['دول البلطيق', 'شرق أوروبا', 'دول الشمال', 'جنوب أوروبا', 'غرب أوروبا'].map(name => ({ name }));

  window.JEEM_RISK_DEMO = {
    id: 'supplier-risk', name: 'مخاطر الموردين والتسليم',
    years: [2024, 2025],
    regions,
    regionMetrics: {
      2025: [[9.8, 95.4, 4.6], [-5.4, 82.0, 17.4], [-16.3, 88.5, 11.5], [-5.6, 95.2, 4.8], [-0.5, 96.0, 4.0]],
      2024: [[6.1, 94.8, 5.2], [2.2, 90.4, 9.6], [-3.1, 92.2, 7.8], [1.8, 95.6, 4.4], [3.4, 96.5, 3.5]],
    },
    kpis: {
      2025: { otifPct: 91.5, otifChangePts: -2.07, revenue: 298000, revenueChangePct: -2.9, grossProfit: 103000, grossProfitChangePct: -2.3 },
      2024: { otifPct: 93.6, otifChangePts: 0.84, revenue: 306900, revenueChangePct: 4.1, grossProfit: 105400, grossProfitChangePct: 3.2 },
    },
    kpiSparklines: {
      otif: { 2025: [65.41, 67.81, 64.7, 59.71, 53.17, 54.23, 54.26, 51.23, 51.71, 48.5, 51.11, 60.13], 2024: [52.99, 58.5, 62.49, 69.47, 66, 58.98, 59.67, 52.13, 43.29, 47.8, 42.94, 43.76] },
      revenue: { 2025: [62.46, 64.33, 65.27, 65.58, 63.02, 55.69, 43.14, 52.19, 51.43, 44.48, 53.88, 55.3], 2024: [53.16, 58.76, 67.95, 67.29, 64.32, 66.5, 60.03, 54.27, 43.91, 51.7, 50.7, 50.96] },
      grossProfit: { 2025: [69.13, 66.91, 65.07, 62.91, 57.58, 47.49, 47.26, 45.83, 48.39, 53.68, 56.08, 53.85], 2024: [54.89, 60.77, 61.63, 65.8, 60.78, 64.96, 55, 50.72, 44.99, 50.75, 48.61, 46.28] },
    },
    profitImpact: { 2025: [0.4, -4.5, -0.4, -0.9, 3.0], 2024: [0.6, 0.7, 0.3, 0.4, 1.2] },
    quarterlyProfit: { 2025: [26, 22, 26.6, 27], 2024: [25.1, 26.2, 26.4, 27.7] },
    suppliers: [
      { name: 'لوجستيات المجمّدات الشرقية', revenueK: 27, riskScore: 15.2, region: 1, action: 'تأخر التسليم', actionPlan: 'مراجعة تعافٍ أسبوعية' },
      { name: 'أطعمة القيمة بالجملة', revenueK: 56, riskScore: 11.5, region: 1, action: 'تعرض إيراد مرتفع', actionPlan: 'خطة تحسين التسليم' },
      { name: 'مجموعة ألبان الشمال', revenueK: 34, riskScore: 7.8, region: 2, action: 'جودة', actionPlan: 'مراقبة جودة مستمرة' },
      { name: 'مشروبات ممتازة', revenueK: 24, riskScore: 5.2, region: 3, action: '' },
      { name: 'أطعمة البحر المتوسط', revenueK: 37, riskScore: 4.2, region: 3, action: '' },
      { name: 'منتجات البلطيق الطازجة', revenueK: 53, riskScore: 4.0, region: 0, action: '' },
      { name: 'الوجبات العالمية', revenueK: 39, riskScore: 2.6, region: 4, action: '' },
    ],
    supplierKpis: {
      2025: { lateDeliveryPct: 8.5, highRiskCount: 2, revenueAtRiskSar: 83120 },
      2024: { lateDeliveryPct: 6.1, highRiskCount: 1, revenueAtRiskSar: 51400 },
    },
    monthlyOtif: {
      2025: [94.3, 95.6, 91.6, 92.8, 83.0, 92.0, 89.9, 89.5, 90.6, 93.4, 94.4, 90.7],
      2024: [95.1, 95.8, 94.2, 93.9, 92.6, 93.8, 94.4, 92.1, 93.3, 95.0, 95.6, 94.1],
    },
    categoryRisk: {
      2025: [
        { name: 'مجمّد', latePct: 13.2, storageRisk: 14.5, revenueExposed: 46, color: R },
        { name: 'محيط', latePct: 8.2, storageRisk: 7.9, revenueExposed: 22, color: A },
        { name: 'مبرد', latePct: 7.0, storageRisk: 5.6, revenueExposed: 18, color: G },
        { name: 'جاف', latePct: 5.6, storageRisk: 3.1, revenueExposed: 9, color: G },
      ],
      2024: [
        { name: 'مجمّد', latePct: 10.56, storageRisk: 12.33, revenueExposed: 46, color: R },
        { name: 'محيط', latePct: 6.56, storageRisk: 6.72, revenueExposed: 22, color: A },
        { name: 'مبرد', latePct: 5.6, storageRisk: 4.76, revenueExposed: 18, color: G },
        { name: 'جاف', latePct: 4.48, storageRisk: 2.64, revenueExposed: 9, color: G },
      ],
    },
    inventory: {
      2025: { categories: [
        { name: 'المجمّدات', stockoutPct: 6.8, wastePct: 3.9, coverageDays: 9 },
        { name: 'الألبان', stockoutPct: 4.9, wastePct: 5.6, coverageDays: 6 },
        { name: 'المخبوزات', stockoutPct: 3.6, wastePct: 7.2, coverageDays: 3 },
        { name: 'المشروبات', stockoutPct: 2.1, wastePct: 1.1, coverageDays: 21 },
        { name: 'الوجبات الخفيفة', stockoutPct: 1.4, wastePct: 0.8, coverageDays: 28 },
      ] },
      2024: { categories: [
        { name: 'المجمّدات', stockoutPct: 5.2, wastePct: 3.1, coverageDays: 9 },
        { name: 'الألبان', stockoutPct: 4.1, wastePct: 4.8, coverageDays: 6 },
        { name: 'المخبوزات', stockoutPct: 3.2, wastePct: 6.5, coverageDays: 3 },
        { name: 'المشروبات', stockoutPct: 2.4, wastePct: 1.3, coverageDays: 21 },
        { name: 'الوجبات الخفيفة', stockoutPct: 1.6, wastePct: 0.9, coverageDays: 28 },
      ] },
    },
  };
})();
