/* بيانات تجريبية ثابتة لمعاينة "إدارة أعمال المقاولات" في معرض النماذج فقط — نفس أرقام
   وأسماء النموذج الأولي بالضبط، بشكل data المطلوب لـ window.JEEM_CONSTRUCTION_MOUNT.
   مواعيد اليوم بتاريخ اليوم الفعلي الحقيقي (لا تاريخ ثابت)، فهذا الملف صالح لأي يوم تحميل. */
(function () {
  const C = { pur: '#6A4FD8', grn: '#8DBF2E', org: '#E8794A' };
  const todayISO = (() => { const d = new Date(); return d.toISOString().slice(0, 10); })();
  const lastUpdatedAt = (() => { const d = new Date(); d.setDate(d.getDate() - 4); return d.toISOString(); })();

  window.JEEM_CONSTRUCTION_DEMO = {
    id: 'construction-crm', name: 'إدارة أعمال المقاولات',
    lastUpdatedAt,
    requests: {
      totalActivityPct: { 7: 64, 30: 71 },
      categories: [
        { name: 'جديدة', color: C.pur, icon: 'plus', pct: 32, count7: 24, count30: 86 },
        { name: 'مكتملة', color: C.grn, icon: 'check', pct: 41, count7: 15, count30: 54 },
        { name: 'متأخرة', color: C.org, icon: 'clock', pct: 24, count7: 5, count30: 18 },
      ],
    },
    quotes: {
      7: [
        { name: 'معتمدة', count: 16, valueK: 17, color: C.org },
        { name: 'مطلوبة', count: 9, valueK: 11, color: C.grn },
        { name: 'مسودة', count: 11, valueK: 20, color: C.pur },
      ],
      30: [
        { name: 'معتمدة', count: 58, valueK: 61, color: C.org },
        { name: 'مطلوبة', count: 32, valueK: 40, color: C.grn },
        { name: 'مسودة', count: 40, valueK: 72, color: C.pur },
      ],
    },
    jobs: [
      { name: 'تتطلب إجراء', count: 57, valueK: 76, color: C.grn },
      { name: 'بانتظار الفوترة', count: 128, valueK: 257, color: C.pur },
      { name: 'نشطة', count: 35, valueK: 34, color: C.org },
    ],
    appointments: [
      { name: 'محمد الجهني', code: 'C-001', date: todayISO, time: '9:00 ص', location: 'اجتماع عبر الإنترنت', status: 'ok', valueK: 4.2 },
      { name: 'نوال الغامدي', code: 'C-014', date: todayISO, time: '10:30 ص', location: 'موقع العميل – الملقا', status: 'go', valueK: 3.1 },
      { name: 'عادل البقمي', code: 'C-022', date: todayISO, time: '12:00 م', location: 'المكتب الرئيسي', status: 'act', valueK: 2.6 },
      { name: 'ريم العمري', code: 'C-031', date: todayISO, time: '1:30 م', location: 'موقع المشروع – النرجس', status: 'go', valueK: 5.4 },
      { name: 'تركي الدوسري', code: 'C-040', date: todayISO, time: '3:00 م', location: 'اجتماع عبر الإنترنت', status: 'act', valueK: 1.5 },
      { name: 'ليان القحطاني', code: 'C-047', date: todayISO, time: '4:30 م', location: 'موقع العميل – حطين', status: 'ok', valueK: 3.9 },
    ],
    payoutAccounts: (() => {
      const days = ['السبت', 'الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة'];
      return [
        { name: 'البنك الأهلي', inTransit: 4291.56, instantAvailable: 1992.12, targetDaily: 1500, targetPctOfGoal: 60, weeklyTransfers: { labels: days, vals: (() => { const r = window.DK.rng('pay0'); return days.map(() => Math.round(400 + r() * 1400)); })() } },
        { name: 'مصرف الراجحي', inTransit: 6120.4, instantAvailable: 845.9, targetDaily: 1500, targetPctOfGoal: 48, weeklyTransfers: { labels: days, vals: (() => { const r = window.DK.rng('pay1'); return days.map(() => Math.round(400 + r() * 1400)); })() } },
      ];
    })(),
  };
})();
