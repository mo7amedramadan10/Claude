/* بيانات تجريبية ثابتة لمعاينة "تفاصيل مشروع حملة" في معرض النماذج فقط — نفس أرقام
   وأسماء النموذج الأولي بالضبط، بشكل data المطلوب لـ window.JEEM_CAMPAIGN_MOUNT. سلسلة
   الأداء تُحسب بنفس دالة DK.rng العشوائية المحجوبة بالبذرة، لكن بالنسبة لليوم الفعلي
   الحالي بدل تاريخ ثابت — فهذا الملف لا يحتوي على تاريخ ثابت. */
(function () {
  function perfFor(n) {
    const f = 7, r = window.DK.rng('cp' + n);
    const today = new Date(); today.setHours(0, 0, 0, 0);
    const start = new Date(today); start.setDate(start.getDate() - (n - 1));
    const labels = Array.from({ length: n + f }, (_, i) => { const d = new Date(start); d.setDate(d.getDate() + i); return d.toLocaleDateString('ar-SA-u-ca-gregory-nu-latn', { day: 'numeric', month: 'short' }); });
    let a = 180, b = 2; const sp = [], cv = [];
    for (let i = 0; i < n + f; i++) {
      a += 6 + r() * 14 - (i === Math.floor(n * .55) ? 60 : 0);
      b += .3 + r() * .5 - (i === Math.floor(n * .55) ? 2 : 0);
      sp.push(Math.round(a)); cv.push(+b.toFixed(1));
    }
    const cut = (arr, fc) => arr.map((v, i) => fc ? (i >= n - 1 ? v : null) : (i < n ? v : null));
    return { labels, spendActual: cut(sp), spendForecast: cut(sp, 1), convActual: cut(cv), convForecast: cut(cv, 1) };
  }

  window.JEEM_CAMPAIGN_DEMO = {
    id: 'campaign-project', name: 'حملة الربيع الترويجية',
    archived: false,
    dueDate: (() => { const d = new Date(); d.setDate(d.getDate() + 16); return d.toISOString().slice(0, 10); })(),
    roas: 4.5,
    ranges: [15, 30],
    budget: {
      total: 12000, convRatePct: 4.5,
      15: { spent: 4800, conversions: 84 },
      30: { spent: 9100, conversions: 151 },
    },
    members: [{ name: 'سارة' }, { name: 'علي' }, { name: 'منى' }, { name: 'خالد' }],
    daysRemaining: 12,
    performance: { 15: perfFor(15), 30: perfFor(30) },
    feed: [
      { icon: 'wallet', text: 'سارة حدّثت الميزانية من 5,000 إلى 6,000 ر.س', when: 'قبل ساعتين', color: 0 },
      { icon: 'image', text: 'علي رفع 5 تصاميم إعلانية جديدة', when: 'قبل 3 ساعات', color: 3 },
      { icon: 'up', text: 'تنبيه أداء: ارتفاع نسبة النقر', when: 'قبل يوم', color: 1 },
      { icon: 'spark', text: 'اقتراح ذكي: زيادة الإنفاق على حملة التخفيضات', when: 'قبل يومين', color: 5 },
      { icon: 'check', text: 'خالد أكمل اختبار A/B لصفحة الهبوط', when: 'قبل 3 أيام', color: 4 },
    ],
    tasks: [
      { title: 'إعداد التصاميم الإعلانية', status: 'done', assignee: 'علي', due: '04 نوفمبر' },
      { title: 'إطلاق إعلانات ميتا', status: 'prog', assignee: 'سارة', due: '08 نوفمبر' },
      { title: 'متابعة نسبة النقر يوميًا', status: 'on', assignee: 'منى', due: '12 نوفمبر' },
      { title: 'إعداد التقرير النهائي', status: 'pend', assignee: 'خالد', due: '16 نوفمبر' },
    ],
    files: [
      { name: 'تصاميم_الربيع_v3.pdf', sizeLabel: '2.4 م.ب', by: 'علي', date: '10 فبراير' },
      { name: 'نص_الإعلان_النهائي.docx', sizeLabel: '156 ك.ب', by: 'منى', date: '08 فبراير' },
      { name: 'الميزانية.xlsx', sizeLabel: '88 ك.ب', by: 'سارة', date: '05 فبراير' },
      { name: 'الجدول_الزمني.pptx', sizeLabel: '1.2 م.ب', by: 'خالد', date: '12 فبراير' },
      { name: 'ملاحظات_العميل.docx', sizeLabel: '234 ك.ب', by: 'منى', date: '11 فبراير' },
    ],
  };
})();
