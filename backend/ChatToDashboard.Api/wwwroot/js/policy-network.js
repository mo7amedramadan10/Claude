/* ==========================================================
   جيم — إعداد لوحة «شبكة الأنظمة واللوائح» (مجال خاص فوق محرك الشبكات العام)
   لا بيانات هنا — nodes/edges تُجلب من GET /api/graph/policy (انظر network-graph.js).
   كل ما هنا تصنيف وعرض ومنطق تقديم خاص بهذا المجال فقط: أنواع العقد، العلاقات، المؤشرات،
   الرسوم، الأعمدة، الكلمات المفتاحية، واقتراحات «اسأل جيم».
   ========================================================== */
window.JEEM_NETWORK_CONFIG = {
  networkKey: 'policy',
  types: {
    law:       { n: 'نظام',           pl: 'الأنظمة',             col: '#127DBD', shape: 'circle',  r: 22 },
    entity:    { n: 'جهة',            pl: 'الجهات',              col: '#00BAB3', shape: 'circle',  r: 12 },
    reg:       { n: 'لائحة أو قرار',  pl: 'اللوائح والقرارات',   col: '#6E62B0', shape: 'square',  r: 11 },
    platform:  { n: 'منصة إلكترونية', pl: 'المنصات',             col: '#28B6E6', shape: 'hex',     r: 11 },
    complaint: { n: 'شكاوى وتظلمات',  pl: 'الشكاوى',             col: '#D9822B', shape: 'diamond', r: 10, minor: true },
    chapter:   { n: 'محور في النظام', pl: 'محاور الأنظمة',       col: '#8A94A0', shape: 'circle',  r: 7, outline: true, minor: true }
  },
  rels: {
    exec: 'لائحة تنفيذية لـ', amend: 'يُعدِّل', has: 'يتضمن المحور', super: 'تُشرف على تطبيق', apply: 'مُلزمة بتطبيق',
    via: 'يُنفَّذ عبر', under: 'شكاوى بموجب', against: 'شكاوى ضد', cross: 'يتقاطع مع', benefit: 'مستفيد من', report: 'تتلقى البلاغات من'
  },
  /* إعدادات المجال: كل النصوص والمؤشرات الخاصة بالأنظمة واللوائح */
  cfg: {
    hub: 'law', issue: 'complaint',
    hubs: [{ id: 'L1', label: 'المنافسات والمشتريات' }, { id: 'L2', label: 'حماية البيانات الشخصية' }, { id: 'L3', label: 'التخصيص' }], hubsAll: 'كل الأنظمة',
    hubPos: { L1: [0, -20], L2: [-340, 160], L3: [330, 170] },
    springs: { has: 78, cross: 320, under: 110, against: 110 }, dashed: ['under', 'against'], far: ['cross'],
    radius: n => n.t === 'complaint' ? Math.min(8, n.c / 45) : n.t === 'entity' ? Math.min(6, n.deg * 1.2) : 0,
    time: { min: 2019, max: 2026, ticks: [2019, 2020, 2021, 2022, 2023, 2024, 2025, 2026], out: y => y, play: 'تشغيل تطور الشبكة عبر السنوات' },
    since: n => 'منذ ' + n.y,
    scopeAll: 'كل الأنظمة المعروضة',
    tip: (n, u) => n.t === 'complaint' ? `${u.nf(n.c)} شكوى` : `${n.deg} ارتباطات · ${u.nf(n.c)} شكوى${n.cm ? ` · امتثال ${n.cm}%` : ''}`,
    rev: { has: 'محور ضمن', exec: 'لائحته التنفيذية', super: 'تُشرف عليه', apply: 'مُلزَم به', via: 'منصة لـ', under: 'شكاوى مرتبطة', against: 'شكاوى ضده', amend: 'عُدِّل بواسطة', benefit: 'المستفيدون', report: 'جهة البلاغات' },
    kpis: (L, u) => {
      const cnt = t => L.filter(n => n.t === t).length, ents = L.filter(n => n.t === 'entity'), comp = L.filter(n => n.t === 'complaint');
      const cm = ents.length ? Math.round(ents.reduce((a, n) => a + n.cm, 0) / ents.length) : 0;
      const sent = L.length ? Math.round(L.reduce((a, n) => a + n.s * Math.max(1, n.c), 0) / L.reduce((a, n) => a + Math.max(1, n.c), 0)) : 0;
      // d: '' on every KPI here — JR's kpi() fills in a RANDOM trend percentage when d is
      // omitted (dash-render.js's own demo-filler behavior); none of these numbers have a real
      // period-over-period comparison yet, so the trend line must be explicitly suppressed
      // rather than silently replaced with fabricated-looking data.
      return [
        { t: 'kpi', ti: 'الأنظمة واللوائح', v: String(cnt('law') + cnt('reg')), d: '', ic: 'file' },
        { t: 'kpi', ti: 'الجهات المرتبطة', v: String(ents.length), d: '', n: `${cnt('platform')} منصات إلكترونية`, ic: 'users' },
        { t: 'kpi', ti: 'الشكاوى (12 شهرًا)', v: u.nf(comp.reduce((a, n) => a + n.c, 0)), d: '', n: `${comp.length} مجموعات شكاوى`, ic: 'bell', hl: 1 },
        { t: 'kpi', ti: 'متوسط امتثال الجهات', v: cm + '%', d: '', ic: 'shield' },
        { t: 'kpi', ti: 'صافي الرأي العام', v: (sent > 0 ? '+' : '') + sent, d: '', n: 'من −100 إلى +100', ic: 'chat' }
      ];
    },
    issueTitle: 'أعلى مجموعات الشكاوى', issueVal: n => n.c, issueFmt: (n, u) => u.nf(n.c),
    stats: (n, u) => [['الشكاوى', u.nf(n.c)]].concat(n.t !== 'complaint' ? [['الامتثال', n.cm + '%']] : []),
    sent: { title: 'الرأي العام', neg: 'سلبي', pos: 'إيجابي' },
    linkVal: (o, u) => o.t === 'complaint' ? u.nf(o.c) : '',
    widgets: (n, u) => {
      const nm = n ? n.n : 'كل الأنظمة', base = n ? n.c : 2900;
      const ents = (n ? [...n.nb].map(x => u.byId[x]).filter(o => o.t === 'entity') : u.N.filter(o => o.t === 'entity')).filter(u.vis).sort((a, b) => b.c - a.c).slice(0, 6).map(o => o.n);
      return [
        { t: 'line', ti: 'الشكاوى الشهرية', su: nm, x: 'M12', b: Math.max(4, base / 12), ly: 1, sp: 6 },
        n && n.t === 'complaint'
          ? { t: 'donut', ti: 'حالة الشكاوى', su: nm, x: ['قيد المعالجة', 'مغلقة بالحل', 'محالة للجنة', 'مرفوضة'], tot: u.nf(n.c), sp: 6 }
          : { t: 'line', ti: 'نسبة الامتثال الشهرية', su: nm, x: 'M12', b: n ? n.cm || 80 : 81, k: 'pct', vol: .03, tr: .004, tg: 85, sp: 6 },
        { t: 'hbars', ti: 'الشكاوى حسب الجهة', su: ents.length ? 'الجهات المرتبطة مباشرة' : '', x: ents.length ? ents : ['لا توجد جهات مرتبطة'], b: Math.max(10, base / 3), sp: 6 },
        { t: 'donut', ti: 'مصادر الرأي العام', su: nm, x: ['منصات التواصل', 'منصات الشكاوى', 'الأخبار', 'الاستبيانات'], sp: 6 }
      ];
    },
    cols: [['الشكاوى', (n, u) => u.nf(n.c)], ['الامتثال', n => n.t === 'complaint' ? '—' : n.cm + '%'], ['الرأي العام', n => (n.s > 0 ? '+' : '') + n.s], ['السنة', n => n.y]],
    kw: [['شكاو', 'complaint'], ['شكوى', 'complaint'], ['تظلم', 'complaint'], ['جه', 'entity'], ['لائح', 'reg'], ['قرار', 'reg'], ['منص', 'platform']],
    askNode: n => `ما أبرز المخاطر المرتبطة بـ«${n.n}»؟`,
    answer: (L, u) => { const c = L.filter(n => n.t === 'complaint').sort((a, b) => b.c - a.c); return c.length ? `وجدت ${L.length} عقدة مرتبطة، أبرزها «${c[0].n}» بـ${u.nf(c[0].c)} شكوى${c[1] ? ` ثم «${c[1].n}» (${u.nf(c[1].c)})` : ''}.` : `وجدت ${L.length} عقدة مرتبطة بسؤالك، ظاهرة الآن على الشبكة.`; }
  },
  /* اقتراحات «اسأل جيم» — تطابق عقد البذرة المزروعة افتراضيًا لكل مشروع جديد (GraphStore.PolicySeed).
     إن بُنيت شبكة المشروع لاحقًا من بيانات حقيقية بمعرّفات مختلفة فهذه الاقتراحات لن تجد تطابقًا
     تامًا في G.asks، فيسقط «اسأل جيم» تلقائيًا لمسار البحث الحر بالكلمات المفتاحية (كود network-graph.js: ask()). */
  asks: [
    { q: 'أين تتركز الشكاوى في نظام المنافسات؟', ids: ['L1', 'C3', 'C4', 'C5', 'C1', 'K1', 'K2', 'K3', 'K4'], a: 'تتركز 62% من شكاوى النظام في محوري الترسية والتعاقد، وأعلاها تظلمات الترسية (318) وتأخر المستحقات (274).' },
    { q: 'ما الجهات المُلزمة بالأنظمة الثلاثة؟', ids: ['L1', 'L2', 'L3', 'E8'], a: 'وزارة الصحة هي الجهة الوحيدة المرتبطة بالأنظمة الثلاثة معًا، ولديها 3 مجموعات شكاوى مفتوحة.' },
    { q: 'ما الذي تغيّر منذ 2024؟', ids: ['R4', 'R5', 'R1', 'C1', 'C6', 'E2'], a: 'صدر قراران: ضوابط التعاقد والشراء (2024) وعدّلت التأهيل والكراسات، وتعديل مدد التظلم (2025).' },
    { q: 'مخاطر حماية البيانات في القطاع الصحي', ids: ['L2', 'C8', 'C7', 'K6', 'K7', 'E14', 'E8', 'E6'], a: 'مقدمو الخدمات الصحية الخاصة الأدنى امتثالًا (61%) ومرتبطون بمجموعتي شكاوى: تسرب البيانات والرسائل دون موافقة.' }
  ]
};
