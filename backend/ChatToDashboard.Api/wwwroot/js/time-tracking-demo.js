/* بيانات تجريبية لمعاينة "تتبع الوقت للفرق" في معرض النماذج فقط — تُولَّد لأيام
   الشهر الحالي الفعلي (بدل تاريخ ثابت يصبح قديمًا)، بنفس شكل data المطلوب لـ
   window.JEEM_TIMETRACKING_MOUNT (مشاريع/أشخاص/سجلات وقت/إجازات/حضور حقيقية الشكل). */
(function () {
  const now = new Date(), Y = now.getFullYear(), M = now.getMonth(), dim = new Date(Y, M + 1, 0).getDate();
  const projects = [
    { id: 'pj0', name: 'بوابة الخدمات', color: '#127DBD', billableRate: 78 },
    { id: 'pj1', name: 'تطبيق الجوال', color: '#00BAB3', billableRate: 63 },
    { id: 'pj2', name: 'منصة البيانات', color: '#6E62B0', billableRate: 71 },
    { id: 'pj3', name: 'الهوية البصرية', color: '#D9822B', billableRate: 58 },
    { id: 'pj4', name: 'نظام الموارد', color: '#28B6E6', billableRate: 66 },
    { id: 'pj5', name: 'التسويق الرقمي', color: '#C2569B', billableRate: 48 },
    { id: 'pj6', name: 'الدعم الفني', color: '#7CB342', billableRate: 0 },
    { id: 'pj7', name: 'التدريب', color: '#E5B33B', billableRate: 0 },
    { id: 'pj8', name: 'أعمال داخلية', color: '#9AA5B1', billableRate: 0 },
  ];
  const people = [
    { id: 'u0', name: 'سارة القحطاني', role: 'مصممة واجهات' },
    { id: 'u1', name: 'خالد العتيبي', role: 'مطور خلفيات' },
    { id: 'u2', name: 'نورة الشمري', role: 'مديرة مشروع' },
    { id: 'u3', name: 'فهد الدوسري', role: 'مطور واجهات' },
    { id: 'u4', name: 'ريم الحربي', role: 'محللة بيانات' },
    { id: 'u5', name: 'ماجد الغامدي', role: 'مختبر جودة' },
  ];
  const presence = [
    { userId: 'u0', status: 'online' }, { userId: 'u1', status: 'online' }, { userId: 'u2', status: 'leave' },
    { userId: 'u3', status: 'offline' }, { userId: 'u4', status: 'online' }, { userId: 'u5', status: 'remote' },
  ];
  const clampLen = (start, len) => Math.min(len, dim - start + 1);
  const pad2 = n => String(n).padStart(2, '0');
  const iso = (d) => `${Y}-${pad2(M + 1)}-${pad2(d)}`;
  const leaveDefs = [
    { p: 2, type: 'annual', start: Math.min(10, dim), len: clampLen(Math.min(10, dim), 3) },
    { p: 4, type: 'sick', start: Math.min(Math.max(1, dim - 10), dim), len: clampLen(Math.min(Math.max(1, dim - 10), dim), 2) },
  ];
  const leaves = leaveDefs.map(l => ({ userId: people[l.p].id, type: l.type, startDate: iso(l.start), endDate: iso(l.start + l.len - 1) }));
  const onLeaveDay = (p, d) => leaveDefs.some(l => l.p === p && l.type !== 'remote' && d >= l.start && d <= l.start + l.len - 1);

  const timeEntries = [];
  let eid = 0;
  for (let d = 1; d <= dim; d++) {
    const wd = new Date(Y, M, d).getDay();
    if (wd === 5 || wd === 6) continue; // عطلة نهاية الأسبوع
    for (let p = 0; p < people.length; p++) {
      if (onLeaveDay(p, d)) continue;
      const j1 = (p + d) % projects.length, hours1 = 3 + ((p * 3 + d) % 4) * 0.75;
      timeEntries.push({ id: 'te' + (eid++), userId: people[p].id, projectId: projects[j1].id, startedAt: `${iso(d)}T09:${pad2((p * 7) % 60)}:00`, durationMin: Math.round(hours1 * 60), billable: projects[j1].billableRate > 0 });
      if (d % 3 === 0) {
        const j2 = (p + d + 3) % projects.length, hours2 = 1 + ((p + d) % 3) * 0.5;
        timeEntries.push({ id: 'te' + (eid++), userId: people[p].id, projectId: projects[j2].id, startedAt: `${iso(d)}T14:${pad2((p * 11) % 60)}:00`, durationMin: Math.round(hours2 * 60), billable: projects[j2].billableRate > 0 });
      }
    }
  }

  window.JEEM_TIMETRACKING_DEMO = { id: 'time-tracking', name: 'تتبع الوقت', projects, people, timeEntries, leaves, presence };
})();
