/* بيانات تجريبية ثابتة لمعاينة "استوديو المشاريع" في معرض النماذج فقط — نفس أرقام
   وأسماء النموذج الأولي بالضبط، بشكل data المطلوب لـ window.JEEM_STUDIO_MOUNT. التواريخ
   تُحسب دومًا بالنسبة لليوم الفعلي الحالي (لا تاريخ ثابت)، فهذا الملف صالح لأي يوم تحميل. */
(function () {
  const iso = offset => { const d = new Date(); d.setHours(0, 0, 0, 0); d.setDate(d.getDate() + offset); return d.toISOString().slice(0, 10); };

  const team = [
    { name: 'محمد رضا', role: 'مصمم واجهات' }, { name: 'مهدي غلام', role: 'مصمم تجربة' },
    { name: 'نسيم مقصود', role: 'مصممة جرافيك' }, { name: 'زهراء باقر', role: 'مديرة منتج' },
    { name: 'مصطفى طاهر', role: 'مصمم واجهات' },
  ];
  const RAW = [
    ['موقع مجوهرات PLC', 0, 5, 'prog', [0, 1], '#E8A13A'],
    ['مشروع NFT – صفحة المنتجات', 2, 7, 'prog', [2, 3, 0], '#2F6BFF'],
    ['صفحة إطلاق NFT', -1, 1, 'done', [4], '#2F6BFF'],
    ['مجموعة NFT', 1, 3, 'up', [1], '#2F6BFF'],
    ['تطبيق TDI – التهيئة', -1, 2, 'prog', [3, 2], '#E8A13A'],
    ['موقع عيادة أسنان – صفحة الهبوط', 4, 8, 'up', [4, 1], '#E8A13A'],
    ['هوية مقهى بُن', 5, 9, 'done', [2], '#7CB342'],
  ];
  const projects = RAW.map(([name, s, e, status, who, color]) => ({ name, start: iso(s), end: iso(e), status, who, color }));

  window.JEEM_STUDIO_DEMO = {
    id: 'project-studio', name: 'استوديو المشاريع',
    team,
    statusTotals: { up: 4, done: 27, prog: 12, all: 43 },
    projects,
    todayTasks: [
      { title: 'تصميم واجهة البطل للموقع', who: [1], done: false },
      { title: 'حزمة رسوم NFT', who: [2, 0], done: false },
      { title: 'مفاهيم ثلاثية الأبعاد لأبرون', who: [3, 4], done: false },
    ],
    messages: [
      { type: 'msg', dir: 'in', kind: 'voice', dur: '0:21', time: '10:12 ص' },
      { type: 'day', label: 'أمس' },
      { type: 'msg', dir: 'out', kind: 'text', text: 'تم رفع الإصدار الجديد من الواجهة 👌', time: '10:14 ص' },
      { type: 'msg', dir: 'in', kind: 'img', time: '10:31 ص' },
      { type: 'msg', dir: 'in', kind: 'text', text: 'شاهدوا التصميم المحدث 🔥', time: '10:33 ص' },
    ],
  };
})();
