/* بيانات تجريبية ثابتة لمعاينة "تحليل مسار المبيعات" في معرض النماذج فقط — تولّد 3000 عميل
   محتمل اصطناعي بنفس منطق وتوزيعات النموذج الأولي (دالة DK.rng المحجوبة بالبذرة)، لكن بشكل
   data.leads (صف واحد لكل عميل محتمل بأسماء حقول حقيقية) المطلوب لـ window.JEEM_ANALYSIS_MOUNT
   بدل مصفوفة مُفهرَسة داخلية. التواريخ تغطي آخر 5 أشهر فعلية بالنسبة لليوم الحالي — لا تاريخ
   ثابت — فهذا الملف صالح لأي يوم تحميل. */
(function () {
  const r = window.DK.rng('crm-analysis');
  const IND = ['النقل والخدمات اللوجستية', 'البنوك والتمويل', 'تقنية المعلومات', 'الجهات الحكومية', 'الخدمات المهنية', 'التعليم', 'الزراعة والتعدين', 'التجزئة والتوزيع', 'الاتصالات', 'التصنيع', 'الإعلام والترفيه', 'الإنشاءات والهندسة'];
  const SIZE = ['متوسطة (201–500)', 'صغيرة (11–200)', 'متناهية (1–10)', 'كبرى (1001+)', 'كبيرة (501–1000)'];
  const AG = ['ليلى السبيعي', 'جاسم المطيري', 'مشعل الحربي', 'كوثر العنزي', 'عمر الزهراني', 'هند الشهري', 'سعد القرني', 'دانة الرشيد'];
  const CITY = ['الرياض', 'جدة', 'الدمام', 'المدينة المنورة', 'أبها', 'تبوك', 'القصيم', 'حائل'];
  const STG = ['عملاء محتملون', 'مؤهلون', 'عرض مقدّم', 'تفاوض', 'صفقة مغلقة'];
  const pick = w => { let t = r() * w.reduce((a, b) => a + b, 0); for (let i = 0; i < w.length; i++) if ((t -= w[i]) < 0) return i; return w.length - 1; };

  const now = new Date(); now.setHours(0, 0, 0, 0);
  const monthStart = new Date(now.getFullYear(), now.getMonth() - 4, 1); // آخر 5 أشهر بما فيها الشهر الحالي

  const leads = Array.from({ length: 3000 }, () => {
    const m = pick([1.2, 1.1, 1.3, .9, .8]), d = Math.floor(r() * 28);
    let st = pick([5, 2.2, 1.3, .9, 1.1]);
    if (m === 4 && st === 4 && r() < .35) st = 3; // تذبذب طبيعي في توزيع الاحتمال، لا تلاعب يدوي بشهر بعينه
    const created = new Date(monthStart.getFullYear(), monthStart.getMonth() + m, 1 + d);
    const value = Math.round(1200 + r() * 5200 + (st === 4 ? r() * 2000 : 0));
    const days = Math.round(20 + r() * 80);
    const churned = st === 4 && r() < .5 && m !== 4;
    const closed = new Date(created); closed.setDate(closed.getDate() + days);
    return {
      createdAt: created.toISOString().slice(0, 10),
      industry: IND[pick([6, 5, 4.5, 3.5, 3, 2.2, 2, 2, 2, 1.8, 1.4, 1.2])],
      city: CITY[pick([5, 3.5, 2.5, 1.5, 1, .8, 1.2, .7])],
      orgSize: SIZE[pick([5, 4.4, 3, 1, .8])],
      agent: AG[pick([3.2, 2.2, 2.1, 1.3, 1.3, 1, 1, .8])],
      stageIndex: st,
      value,
      closedAt: st === STG.length - 1 ? closed.toISOString().slice(0, 10) : null,
      churned: st === STG.length - 1 ? churned : false,
    };
  });

  window.JEEM_ANALYSIS_DEMO = { id: 'crm-analysis', name: 'تحليل مسار المبيعات', stages: STG, leads };
})();
