/* ==========================================================
   جيم — بيانات تجريبية لشبكة العملاء وطلبات الشراء (نظام المشتريات)
   كل الأسماء والأرقام للعرض فقط ولا تمثل بيانات فعلية.
   الشهر (y): 1 = يناير 2026 … 9 = سبتمبر 2026.
   ========================================================== */
(function () {
  const ST = { new: 'جديد', tech: 'قيد الدراسة الفنية', appr: 'بانتظار الاعتماد', rfq: 'مطروح للعروض', award: 'مُرسى', done: 'مُسلَّم', stuck: 'متعثر' };
  const STONE = { new: 'info', tech: 'info', appr: 'warn', rfq: 'info', award: 'ok', done: 'ok', stuck: 'bad' };
  const MONTHS = ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو', 'يوليو', 'أغسطس', 'سبتمبر'];

  const clients = [
    { id: 'C1', n: 'تجمع الرياض الصحي الأول', d: 'أكبر العملاء طلبًا: أجهزة طبية ومستهلكات وخدمات صيانة لمستشفيات التجمع.', s: -12, sla: 71 },
    { id: 'C2', n: 'أمانة منطقة الرياض', d: 'طلبات صيانة وتشغيل ومعدات ميدانية وخدمات نظافة.', s: 8, sla: 79 },
    { id: 'C3', n: 'جامعة الملك سعود', d: 'طلبات تقنية معلومات وأثاث ومختبرات بحثية.', s: 24, sla: 86 },
    { id: 'C4', n: 'الهيئة الملكية لمدينة الرياض', d: 'طلبات خدمات استشارية ودراسات ومشاريع كبرى.', s: 31, sla: 90 }
  ];
  const cats = [
    { id: 'G1', n: 'الأجهزة الطبية' }, { id: 'G2', n: 'تقنية المعلومات' }, { id: 'G3', n: 'الصيانة والتشغيل' },
    { id: 'G4', n: 'الأثاث والتجهيزات' }, { id: 'G5', n: 'الخدمات الاستشارية' }, { id: 'G6', n: 'المستهلكات الطبية' }
  ];
  const suppliers = [
    { id: 'S1', n: 'شركة الرعاية المتقدمة للتجهيزات الطبية', g: ['G1', 'G6'], s: 6, sla: 74 },
    { id: 'S2', n: 'مؤسسة الحلول الطبية', g: ['G1', 'G6'], s: 18, sla: 88 },
    { id: 'S3', n: 'شركة التقنية الرقمية', g: ['G2'], s: 27, sla: 92 },
    { id: 'S4', n: 'مجموعة الأنظمة الذكية', g: ['G2'], s: 11, sla: 81 },
    { id: 'S5', n: 'شركة البناء الحديث للصيانة', g: ['G3'], s: -9, sla: 63 },
    { id: 'S6', n: 'مؤسسة الخليج للتشغيل', g: ['G3'], s: 4, sla: 77 },
    { id: 'S7', n: 'مصنع الأثاث الوطني', g: ['G4'], s: 15, sla: 85 },
    { id: 'S8', n: 'بيت الخبرة للاستشارات', g: ['G5'], s: 33, sla: 94 },
    { id: 'S9', n: 'شركة المستهلكات الوطنية', g: ['G6'], s: 2, sla: 69 }
  ];
  /* الطلبات: [معرف، العنوان، العميل، الفئة، القيمة ر.س، الحالة، عمر الطلب بالأيام، الشهر، الموردون المقدمون، المورد المُرسى عليه] */
  const R = [
    ['R01', 'أجهزة تنفس صناعي للعناية المركزة', 'C1', 'G1', 4850000, 'stuck', 96, 3, ['S1', 'S2'], null],
    ['R02', 'مستهلكات غسيل الكلى لسنة', 'C1', 'G6', 2140000, 'award', 41, 5, ['S2', 'S9'], 'S9'],
    ['R03', 'صيانة أجهزة الأشعة المقطعية', 'C1', 'G3', 960000, 'appr', 58, 6, ['S5', 'S6'], null],
    ['R04', 'أسرّة مرضى كهربائية', 'C1', 'G4', 1320000, 'done', 72, 2, ['S7'], 'S7'],
    ['R05', 'شاشات مراقبة المرضى', 'C1', 'G1', 3100000, 'tech', 37, 7, ['S1', 'S2'], null],
    ['R06', 'قفازات وكمامات طبية', 'C1', 'G6', 680000, 'stuck', 64, 4, ['S9'], null],
    ['R07', 'صيانة الحدائق والإنارة', 'C2', 'G3', 2750000, 'award', 48, 4, ['S5', 'S6'], 'S6'],
    ['R08', 'معدات رش ومكافحة', 'C2', 'G3', 540000, 'rfq', 19, 8, ['S6'], null],
    ['R09', 'أنظمة كاميرات المراقبة البلدية', 'C2', 'G2', 1880000, 'stuck', 83, 3, ['S4'], null],
    ['R10', 'أثاث مراكز الخدمة', 'C2', 'G4', 430000, 'done', 51, 1, ['S7'], 'S7'],
    ['R11', 'أجهزة حاسب محمولة لأعضاء هيئة التدريس', 'C3', 'G2', 2460000, 'award', 33, 5, ['S3', 'S4'], 'S3'],
    ['R12', 'تجهيز مختبر الذكاء الاصطناعي', 'C3', 'G2', 3900000, 'appr', 44, 6, ['S3', 'S4'], null],
    ['R13', 'أثاث قاعات المحاضرات', 'C3', 'G4', 1150000, 'rfq', 12, 8, ['S7'], null],
    ['R14', 'خوادم مركز البيانات', 'C3', 'G2', 5200000, 'tech', 21, 9, ['S3', 'S4'], null],
    ['R15', 'دراسة النقل الحضري الشامل', 'C4', 'G5', 6800000, 'award', 39, 2, ['S8'], 'S8'],
    ['R16', 'استشارات إدارة المشاريع', 'C4', 'G5', 2950000, 'done', 62, 1, ['S8'], 'S8'],
    ['R17', 'منصة إدارة الأصول الرقمية', 'C4', 'G2', 4100000, 'new', 6, 9, [], null],
    ['R18', 'تشغيل وصيانة المرافق', 'C4', 'G3', 3600000, 'appr', 55, 7, ['S5', 'S6'], null]
  ];
  /* التعثرات والشكاوى: [معرف، العنوان، الوصف، الشهر، الطلبات المتأثرة، الجهة المسببة] */
  const K = [
    ['K1', 'تأخر الاعتماد المالي', 'طلبات تجاوزت 30 يومًا بانتظار اعتماد الميزانية.', 4, ['R03', 'R12', 'R18'], 'C1'],
    ['K2', 'مواصفات غير مكتملة من العميل', 'إعادة الطلب للعميل لاستكمال المواصفات الفنية أكثر من مرة.', 3, ['R01', 'R09'], 'C1'],
    ['K3', 'عرض وحيد وضعف المنافسة', 'منافسة تلقت عرضًا واحدًا فقط مما يستدعي إعادة الطرح.', 4, ['R06', 'R09', 'R13'], null],
    ['K4', 'تأخر التوريد عن الموعد', 'المورد تجاوز موعد التسليم في أمر الشراء.', 6, ['R02', 'R07'], ['S9', 'S6']],
    ['K5', 'رفض فني للعينات', 'العينات المقدمة لم تطابق المواصفات.', 5, ['R01', 'R06'], 'S1'],
    ['K6', 'تجاوز الميزانية المعتمدة', 'أقل العروض أعلى من التكلفة التقديرية بأكثر من 15%.', 7, ['R12', 'R14'], 'C3'],
    ['K7', 'شكوى العميل من بطء التحديثات', 'العميل لا يعرف حالة طلبه ويطلب تحديثًا أسبوعيًا.', 6, ['R01', 'R05', 'R03'], 'C1'],
    ['K8', 'تجاوز مدة الدراسة الفنية', 'الدراسة الفنية تجاوزت مدة مستوى الخدمة (14 يومًا).', 8, ['R05', 'R14'], null]
  ];

  const nodes = [], edges = [];
  clients.forEach(c => nodes.push({ id: c.id, t: 'client', n: c.n, d: c.d, y: 1, s: c.s, sla: c.sla }));
  cats.forEach(c => nodes.push({ id: c.id, t: 'category', n: c.n, d: 'فئة مشتريات يُصنَّف تحتها الطلب.', y: 1 }));
  suppliers.forEach(s => { nodes.push({ id: s.id, t: 'supplier', n: s.n, d: 'مورد مؤهل في فئات: ' + s.g.map(g => cats.find(c => c.id === g).n).join('، ') + '.', y: 1, s: s.s, sla: s.sla });  });
  R.forEach(([id, n, c, g, v, st, age, y, offers, win], i) => {
    nodes.push({ id, t: 'request', n, code: 'PR-' + (2040 + i), d: `طلب ${['مستعجل', 'اعتيادي'][i % 3 ? 1 : 0]} من «${clients.find(x => x.id === c).n}» بقيمة تقديرية ${v.toLocaleString('en-US')} ر.س.`, y, v, st, age });
    edges.push({ a: c, b: id, r: 'req' }, { a: id, b: g, r: 'cat' });
    offers.forEach(s => edges.push({ a: s, b: id, r: 'offer', y: Math.min(9, y + 1) }));
    if (win) { const po = 'P' + id.slice(1); nodes.push({ id: po, t: 'po', n: 'أمر شراء PO-' + (5100 + i), d: `صادر لطلب «${n}» مع «${suppliers.find(x => x.id === win).n}».`, y: Math.min(9, y + 1), v: Math.round(v * (.86 + (i % 5) * .03)), done: st === 'done' ? 100 : 30 + (i * 13) % 60 }); edges.push({ a: id, b: po, r: 'award', y: Math.min(9, y + 1) }, { a: po, b: win, r: 'with', y: Math.min(9, y + 1) }); }
  });
  K.forEach(([id, n, d, y, reqs, by]) => { nodes.push({ id, t: 'issue', n, d, y }); reqs.forEach(r => edges.push({ a: id, b: r, r: 'hits', y })); [].concat(by || []).forEach(b => edges.push({ a: id, b, r: 'cause', y })); });
  /* اتفاقية إطارية للمستهلكات مع التجمع الصحي */
  nodes.push({ id: 'PF1', t: 'po', n: 'اتفاقية إطارية للمستهلكات الطبية', d: 'اتفاقية سنوية بأسعار ثابتة يطلب منها التجمع حسب الحاجة.', y: 1, v: 3800000, done: 64 });
  edges.push({ a: 'C1', b: 'PF1', r: 'frame' }, { a: 'PF1', b: 'S2', r: 'with' }, { a: 'PF1', b: 'G6', r: 'cat' });

  const reqsOf = (n, u) => n.t === 'request' ? [n] : [...n.nb].map(x => u.byId[x]).filter(o => o.t === 'request' && u.vis(o));
  const sumV = L => L.reduce((a, r) => a + r.v, 0);
  const sar = v => v >= 1e6 ? (v / 1e6).toFixed(1) + ' م' : Math.round(v / 1e3) + ' ألف';
  const hits = (r, u) => [...r.nb].some(x => u.byId[x].t === 'issue' && u.vis(u.byId[x]));

  window.JEEM_NETWORK = {
    types: {
      client:   { n: 'عميل',              pl: 'العملاء',              col: '#127DBD', shape: 'circle',  r: 22 },
      supplier: { n: 'مورد',              pl: 'الموردون',             col: '#00BAB3', shape: 'circle',  r: 11 },
      request:  { n: 'طلب شراء',          pl: 'طلبات الشراء',         col: '#6E62B0', shape: 'square',  r: 10 },
      po:       { n: 'أمر شراء أو اتفاقية', pl: 'أوامر الشراء',        col: '#28B6E6', shape: 'hex',     r: 10 },
      issue:    { n: 'تعثر أو شكوى',      pl: 'التعثرات والشكاوى',    col: '#D9822B', shape: 'diamond', r: 10, minor: true },
      category: { n: 'فئة مشتريات',       pl: 'فئات المشتريات',       col: '#8A94A0', shape: 'circle',  r: 7, outline: true, minor: true }
    },
    rels: { req: 'قدّم طلب', cat: 'ضمن فئة', offer: 'قدّم عرضًا لـ', award: 'صدر عنه', with: 'مع المورد', frame: 'اتفاقية إطارية', hits: 'يعطّل', cause: 'بسبب', serves: 'مؤهل في فئة' },
    nodes, edges,
    asks: [
      { q: 'أي العملاء لديهم أكثر طلبات متعثرة؟', ids: ['C1', 'R01', 'R02', 'R03', 'R05', 'R06', 'K1', 'K2', 'K4', 'K5', 'K7'], a: 'تجمع الرياض الصحي الأول لديه 5 طلبات متعثرة أو متأثرة بتعثر بقيمة 11.7 م ر.س، أقدمها أجهزة التنفس الصناعي (96 يومًا) بسبب مواصفات غير مكتملة ورفض فني للعينات.' },
      { q: 'ما الطلبات العالقة بانتظار الاعتماد المالي؟', ids: ['K1', 'R03', 'R12', 'R18', 'C1', 'C3', 'C4'], a: '3 طلبات بقيمة 8.5 م ر.س بانتظار الاعتماد منذ 44 إلى 58 يومًا، أكبرها تجهيز مختبر الذكاء الاصطناعي لجامعة الملك سعود.' },
      { q: 'أي الموردين يتأخرون في التوريد؟', ids: ['K4', 'S9', 'S6', 'R02', 'R07', 'P02', 'P07', 'C1', 'C2'], a: 'أمرا شراء تأخرا في التوريد: مستهلكات غسيل الكلى مع شركة المستهلكات الوطنية (التزام 69%)، وصيانة الحدائق مع مؤسسة الخليج للتشغيل (77%).' },
      { q: 'طلبات جامعة الملك سعود وحالتها', ids: ['C3', 'R11', 'R12', 'R13', 'R14', 'P11', 'S3', 'S4', 'S7', 'K6'], a: 'للجامعة 4 طلبات بقيمة 12.7 م ر.س: طلب مُرسى، وطلب بانتظار الاعتماد، وطلب مطروح، وطلب قيد الدراسة الفنية. طلبا المختبر والخوادم مهددان بتجاوز الميزانية.' }
    ],
    cfg: {
      hub: 'client', issue: 'issue',
      hubs: clients.map(c => ({ id: c.id, label: c.n.replace('الهيئة الملكية لمدينة الرياض', 'الهيئة الملكية').replace('تجمع الرياض الصحي الأول', 'التجمع الصحي الأول') })), hubsAll: 'كل العملاء',
      hubPos: { C1: [-380, -200], C2: [380, -200], C3: [-380, 230], C4: [380, 230] }, anchor: .06,
      springs: { req: 120, cat: 150, offer: 130, award: 70, with: 110, hits: 95, cause: 170, serves: 150, frame: 90 },
      springK: { req: .09, cat: .012, offer: .02, cause: .015 },
      dashed: ['hits', 'cause'], far: ['serves'],
      radius: n => n.t === 'request' ? Math.min(7, n.v / 9e5) : n.t === 'supplier' ? Math.min(5, n.deg * .6) : 0,
      time: { min: 1, max: 9, ticks: MONTHS.map(m => m.slice(0, 3)), out: m => MONTHS[m - 1], play: 'تشغيل تدفق الطلبات شهرًا بشهر' },
      since: n => n.t === 'request' ? `${n.code} · ${MONTHS[n.y - 1]}` : n.t === 'issue' ? 'رُصد في ' + MONTHS[n.y - 1] : 'منذ يناير',
      scopeAll: 'كل العملاء والطلبات',
      /* نطاق المؤشرات عند تحديد عقدة: العقدة وجيرانها، ولعميل: طلباته فقط وما يرتبط بها مباشرة */
      scope: (n, u) => { const s = new Set([n.id, ...n.nb]); if (n.t === 'client') [...n.nb].map(x => u.byId[x]).filter(o => o.t === 'request').forEach(r => r.nb.forEach(x => { if (u.byId[x].t !== 'client') s.add(x); })); return s; },
      tip: (n, u) => {
        if (n.t === 'request') return `${ST[n.st]} · ${sar(n.v)} ر.س · ${n.age} يومًا`;
        if (n.t === 'po') return `${sar(n.v)} ر.س · تسليم ${n.done}%`;
        if (n.t === 'issue') return `${[...n.nb].filter(x => u.byId[x].t === 'request').length} طلبات متأثرة`;
        const L = reqsOf(n, u); return `${L.length} طلبات · ${sar(sumV(L))} ر.س${n.sla ? ` · الالتزام بالمدة ${n.sla}%` : ''}`;
      },
      rev: { req: 'مقدَّم من', cat: 'طلبات الفئة', offer: 'العروض المقدمة', award: 'صادر عن الطلب', with: 'أوامر الشراء', frame: 'عميل الاتفاقية', hits: 'التعثرات', cause: 'تعثرات مرتبطة به', serves: 'الموردون المؤهلون' },
      kpis: (L, u, sel) => {
        const R = L.filter(n => n.t === 'request'), open = R.filter(r => r.st !== 'done'), stuck = R.filter(r => r.st === 'stuck' || hits(r, u));
        const cl = L.filter(n => n.t === 'client'), sla = cl.length ? Math.round(cl.reduce((a, n) => a + n.sla, 0) / cl.length) : (sel && sel.sla) || 0;
        const age = open.length ? Math.round(open.reduce((a, r) => a + r.age, 0) / open.length) : 0;
        const sat = cl.length ? Math.round(cl.reduce((a, n) => a + n.s, 0) / cl.length) : 0;
        return [
          { t: 'kpi', ti: 'الطلبات المفتوحة', v: String(open.length), d: '+3', g: false, n: `من ${R.length} طلبًا`, ic: 'file' },
          { t: 'kpi', ti: 'قيمة الطلبات المفتوحة', v: sar(sumV(open)), u: 'ر.س', d: '+12.6%', g: true, ic: 'wallet' },
          { t: 'kpi', ti: 'الطلبات المتعثرة', v: String(stuck.length), d: '+2', g: false, n: `${sar(sumV(stuck))} ر.س معرضة للتأخير`, ic: 'bell', hl: 1 },
          { t: 'kpi', ti: 'متوسط عمر الطلب المفتوح', v: String(age), u: 'يومًا', d: '-4', g: true, ic: 'clock' },
          { t: 'kpi', ti: 'الالتزام بمدة الخدمة', v: (sla || 81) + '%', d: '+2.4 نقطة', g: true, n: 'المستهدف 85%', ic: 'target' },
          { t: 'kpi', ti: 'صافي رضا العملاء', v: (sat > 0 ? '+' : '') + sat, d: sat >= 0 ? '+5' : '-7', g: sat >= 0, n: 'من −100 إلى +100', ic: 'star' }
        ];
      },
      issueTitle: 'التعثرات الأكثر تأثيرًا (بالقيمة)', issueVal: (n, u) => [...n.nb].map(x => u.byId[x]).filter(o => o.t === 'request').reduce((a, r) => a + r.v, 0),
      issueFmt: (n, u) => sar([...n.nb].map(x => u.byId[x]).filter(o => o.t === 'request').reduce((a, r) => a + r.v, 0)),
      stats: (n, u) => {
        if (n.t === 'request') return [['الحالة', ST[n.st]], ['القيمة', sar(n.v)], ['العمر', n.age + ' يومًا']];
        if (n.t === 'po') return [['القيمة', sar(n.v)], ['التسليم', n.done + '%']];
        if (n.t === 'issue') { const L = [...n.nb].map(x => u.byId[x]).filter(o => o.t === 'request'); return [['طلبات متأثرة', L.length], ['القيمة', sar(sumV(L))]]; }
        const L = reqsOf(n, u); return [['الطلبات', L.length], ['القيمة', sar(sumV(L))]].concat(n.sla ? [['الالتزام', n.sla + '%']] : []);
      },
      sent: { title: 'صافي الرضا والتقييم', neg: 'غير راضٍ', pos: 'راضٍ' },
      linkVal: o => o.t === 'request' ? ST[o.st] : o.t === 'po' ? sar(o.v) : '',
      widgets: (n, u) => {
        const L = n ? (n.t === 'request' ? [n] : reqsOf(n, u)) : u.N.filter(o => o.t === 'request' && u.vis(o));
        const nm = n ? n.n : 'كل العملاء', tot = Math.max(1, L.length);
        const byCat = {}; L.forEach(r => { const g = [...r.nb].map(x => u.byId[x]).find(o => o.t === 'category'); if (g) byCat[g.n] = (byCat[g.n] || 0) + r.v; });
        const cats = Object.entries(byCat).sort((a, b) => b[1] - a[1]).map(([k]) => k);
        const rows = [...L].sort((a, b) => b.age - a.age).slice(0, 8);
        return [
          { t: 'funnel', ti: 'مراحل الطلبات', su: nm, x: ['مقدَّمة', 'اجتازت الدراسة الفنية', 'معتمدة ماليًا', 'مُرساة', 'مُسلَّمة'], b: tot, int: 1, sp: 4 },
          { t: 'line', ti: 'الطلبات الجديدة شهريًا', su: nm, x: MONTHS, b: Math.max(1, tot / 6), ly: 1, sp: 8 },
          { t: 'hbars', ti: 'قيمة الطلبات حسب الفئة', su: nm, x: cats.length ? cats : ['لا توجد طلبات'], b: Math.max(4e5, sumV(L) / Math.max(1, cats.length) * 1.4), k: 'sar', sp: 5 },
          { t: 'line', ti: 'الالتزام بمدة الخدمة شهريًا', su: 'المستهدف 85%', x: MONTHS, b: n && n.sla ? n.sla : 80, k: 'pct', vol: .04, tr: .006, tg: 85, sp: 7 },
          rows.length > 1
            ? { t: 'table', ti: 'الطلبات الأقدم عمرًا', su: nm, rows: rows.map(r => r.n), c: [['الطلب', 'l'], ['الرقم', 'id'], ['الحالة', 'st'], ['القيمة (ر.س)', 'sar'], ['العمر (يوم)', 'n'], ['التعثرات', 'n']], vals: rows.map(r => [null, r.code, ST[r.st], sar(r.v), r.age, [...r.nb].filter(x => u.byId[x].t === 'issue').length || '—']), sts: [...new Set(rows.map(r => r.st))].map(s => [ST[s], STONE[s]]), n: rows.length, sp: 12 }
            : { t: 'donut', ti: 'توزيع وقت الطلب على المراحل', su: nm, x: ['الدراسة الفنية', 'الاعتماد المالي', 'الطرح والعروض', 'الترسية والتعاقد'], sp: 12 }
        ];
      },
      cols: [['الحالة', n => n.st ? ST[n.st] : '—'], ['القيمة (ر.س)', n => n.v ? sar(n.v) : '—'], ['العمر', n => n.age != null ? n.age + ' يومًا' : '—'], ['الالتزام بالمدة', n => n.sla ? n.sla + '%' : '—'], ['الشهر', n => MONTHS[n.y - 1]]],
      kw: [['متعثر', 'issue'], ['تعثر', 'issue'], ['تأخ', 'issue'], ['شكو', 'issue'], ['مورد', 'supplier'], ['طلب', 'request'], ['أمر شراء', 'po'], ['عقد', 'po'], ['فئ', 'category'], ['عميل', 'client'], ['عملاء', 'client']],
      askNode: n => n.t === 'request' ? `لماذا تأخر طلب «${n.n}» وما الخطوة التالية؟` : `ما وضع طلبات «${n.n}» وما أبرز التعثرات؟`,
      answer: (L, u) => {
        const R = L.filter(n => n.t === 'request'), st = R.filter(r => r.st === 'stuck' || hits(r, u));
        return R.length ? `وجدت ${R.length} طلبات مرتبطة بقيمة ${sar(sumV(R))} ر.س، منها ${st.length} متعثرة أو متأخرة${st[0] ? `، أقدمها «${[...st].sort((a, b) => b.age - a.age)[0].n}»` : ''}.` : `وجدت ${L.length} عقدة مرتبطة بسؤالك، ظاهرة الآن على الشبكة.`;
      }
    }
  };
})();
