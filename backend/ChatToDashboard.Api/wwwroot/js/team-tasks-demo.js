/* بيانات تجريبية ثابتة لمعاينة "إدارة مهام الفريق" في معرض النماذج فقط —
   نفس أرقام وأسماء النموذج الأولي بالضبط، بشكل data المطلوب لـ window.JEEM_TEAMTASKS_MOUNT.
   اليوم ويوم الأسبوع الحاليان حقيقيان (يُحسبان في المحرك نفسه من Date() الفعلي)، فهذا
   الملف لا يحتوي على تاريخ ثابت — فقط أرقام نشاط/تقويم تجريبية صالحة لأي أسبوع. */
(function () {
  const PPL = ['سارة', 'فهد', 'نورة', 'خالد', 'ريم', 'ماجد'];
  const who = idx => idx.map(i => PPL[i]);
  const RAW_CAL = [
    [0, 9 * 60, 120, 'done', 'مراجعة الموجز', 'مراجعة أهداف المشروع ونطاقه', [0, 1], 18],
    [0, 13 * 60, 150, 'med', 'إنشاء لوحة الإلهام', 'جمع مراجع بصرية للهوية', [2], 4],
    [1, 9 * 60, 210, 'done', 'تصميم الخطوط والتخطيط', 'اختيار الخطوط وعناصر التخطيط', [3, 4], 9],
    [1, 14 * 60, 90, 'low', 'مراجعة مع العميل', 'عرض المسودة الأولى', [0], 2],
    [2, 9 * 60 + 30, 150, 'med', 'اختيار لوحة الألوان', 'إنشاء نظام ألوان متناسق', [1, 5], 3, 't1'],
    [2, 13 * 60, 120, 'high', 'نماذج الواجهات', 'تحويل الإطارات إلى واجهات نهائية', [2, 3], 7],
    [3, 9 * 60, 180, 'high', 'تصميم واجهة المستخدم', 'واجهات جذابة ومتسقة بصريًا', [0, 1, 2, 3], 34],
    [3, 13 * 60, 150, 'low', 'إنشاء نموذج أولي', 'نموذج تفاعلي لعرض التصميم', [4], 5],
    [4, 9 * 60, 90, 'low', 'اختبار المستخدمين', 'جلسات مع 5 مستخدمين', [5], 1],
    [4, 10 * 60 + 45, 150, 'high', 'عرض على العميل', 'عرض المشروع وجمع الملاحظات', [0, 2, 4], 12],
  ];
  const calendarItems = RAW_CAL.map(([d, s, len, pr, n, desc, w, cm, task], i) => ({
    id: 'e' + i, day: d, startMin: s, durationMin: len, priority: pr, title: n, description: desc,
    assignees: who(w), commentsCount: cm, taskId: task || null,
  }));
  const PRI_CYCLE = ['bad', 'mid', 'ok', 'done'];
  const monthActivity = Array.from({ length: 31 }, (_, idx) => {
    const d = idx + 1, count = ((d * 7) % 5) + 1;
    return { day: d, count, priorities: Array.from({ length: count }, (_, j) => PRI_CYCLE[(d + j) % 4]) };
  });

  window.JEEM_TEAMTASKS_DEMO = {
    id: 'team-tasks', name: 'لوحة الفريق',
    tasks: [
      { id: 't1', title: 'اختيار لوحة الألوان', project: 'تطبيق الألعاب', spentSec: 4 * 3600 + 7 * 60 + 11, dailyLimitSec: 6 * 3600, favorite: true },
      { id: 't2', title: 'تصميم صفحة الهبوط', project: 'تطبيق الضبط الموسيقي', spentSec: 3600 + 12 * 60, dailyLimitSec: 4 * 3600, favorite: false },
      { id: 't3', title: 'تحليل المنافسين وظيفيًا', project: 'منصة طبيب', spentSec: 25 * 60, dailyLimitSec: 3 * 3600, favorite: false },
    ],
    activeTaskId: 't1', running: true,
    calendarItems,
    meetings: [
      { id: 'm0', startMin: 10 * 60 + 45, title: 'عرض المشروع وجمع الملاحظات', kind: 'video' },
      { id: 'm1', startMin: 13 * 60, title: 'اجتماع مع فريق تجربة المستخدم', kind: 'video' },
      { id: 'm2', startMin: 15 * 60, title: 'تهيئة العضو الجديد', kind: 'users' },
    ],
    reminders: [
      { id: 'r0', atMin: 9 * 60 + 30, title: 'مراجعة نتائج الاختبار', priority: 'low', done: true },
      { id: 'r1', atMin: 10 * 60 + 45, title: 'عرض على العميل', priority: 'high', done: false },
      { id: 'r2', atMin: 16 * 60 + 15, title: 'إضافة رابط المشروع للعميل', priority: 'high', done: false },
      { id: 'r3', atMin: 17 * 60, title: 'تحديث خطة الأسبوع القادم', priority: 'low', done: false },
    ],
    activityDaily: [
      { day: 0, pct: 92 }, { day: 1, pct: 41 }, { day: 2, pct: 70 }, { day: 3, pct: 55 }, { day: 4, pct: 48 },
    ],
    projectTimeShare: [
      { name: 'تطبيق الألعاب', pct: 44, color: '#127DBD' },
      { name: 'متجر ماغنوم', pct: 24, color: '#00BAB3' },
      { name: 'منصة طبيب', pct: 18, color: '#6E62B0' },
      { name: 'منتصف الليل', pct: 14, color: '#D9822B' },
    ],
    monthActivity,
  };
})();
