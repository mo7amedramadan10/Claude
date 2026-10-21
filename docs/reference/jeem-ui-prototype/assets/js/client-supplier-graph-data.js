/* ==========================================================
   جيم — شبكة العملاء والموردين عبر المنصة (تُبنى من بيانات منصة المشتريات نفسها)
   المصدر: window.JEEM_MARKET (marketplace-data.js) حتى تتطابق الأرقام بين اللوحتين.
   العقد:
     platform  المنصة (في المركز)       client  عميل (يمين)       supplier  مورد (يسار)
     deal      مشروع: علاقة عميل بمورد باسم مشروع (عدد طلباته وحالاتها)     type  نوع طلب     issue  تأخير لدى مورد
   ========================================================== */
(function () {
  const M = window.JEEM_MARKET; if (!M) return;
  const T = M.today, MONTHS = M.months.concat(['أكتوبر']);
  const mon = d => Math.min(9, Math.floor(d / 30.4) + 1);
  const stat = r => { if (r.cancelAt != null) return T < r.cancelAt ? 'wait' : 'canc'; const m = r.created + r.offerDays; if (T < m) return 'wait'; const e = T - m; return e >= r.actual ? 'done' : e > r.sla ? 'late' : 'prog'; };
  const R = M.requests.map(r => Object.assign({ st: stat(r) }, r));
  const SN = Object.fromEntries(Object.entries(M.status).map(([k, v]) => [k, v.n]));
  const agg = L => { const st = {}; L.forEach(r => st[r.st] = (st[r.st] || 0) + 1); const fin = L.filter(r => r.st === 'done'); return { cnt: L.length, late: st.late || 0, val: L.reduce((a, r) => a + r.value, 0), st, onTime: fin.length ? Math.round(fin.filter(r => r.actual <= r.sla).length / fin.length * 100) : null }; };
  const net = a => a.onTime == null ? 0 : Math.round((a.onTime - 50) * 2 * (1 - a.late / Math.max(1, a.cnt)));
  const sar = v => v >= 1e6 ? (v / 1e6).toFixed(1) + ' م' : Math.round(v / 1e3) + ' ألف';
  const pl = (n, few, many, one, two) => n === 1 && one ? one : n === 2 && two ? two : `${n} ${n >= 3 && n <= 10 ? few : many}`;

  const nodes = [], edges = [];
  nodes.push(Object.assign({ id: 'P', t: 'platform', n: 'منصة المشتريات', d: 'تستقبل طلبات العملاء وتطرحها على الموردين المؤهلين وتتابع تنفيذها.', y: 1 }, agg(R.filter(r => r.created <= T))));
  M.clients.forEach(c => { const L = R.filter(r => r.client === c.id), a = agg(L); nodes.push(Object.assign({ id: c.id, t: 'client', n: c.n, d: `يطلب عبر المنصة ${pl(a.cnt, 'طلبات', 'طلبًا')} من ${new Set(L.filter(r => r.supplier).map(r => r.supplier)).size} موردين.`, y: 1, s: net(a), nSup: new Set(L.filter(r => r.supplier).map(r => r.supplier)).size }, a)); });
  M.suppliers.forEach(v => { const L = R.filter(r => r.supplier === v.id && r.st !== 'wait'), a = agg(L); nodes.push(Object.assign({ id: v.id, t: 'supplier', n: v.n, d: `مؤهل في: ${v.types.map(t => M.types.find(x => x.id === t).n).join('، ')}.`, y: 1, s: net(a), nCl: new Set(L.map(r => r.client)).size }, a)); });
  M.types.forEach(t => { const a = agg(R.filter(r => r.type === t.id)); nodes.push(Object.assign({ id: t.id, t: 'type', n: t.n, d: 'نوع طلب تطرحه المنصة على الموردين المؤهلين فيه.', y: 1 }, a)); edges.push({ a: 'P', b: t.id, r: 'offers' }); });
  /* المشاريع: كل عميل ↔ مورد بينهما طلبات مرساة = مشروع يحمل اسمًا حسب نوع الطلبات الغالب والعميل */
  const STEM = {
    T1: ['تجهيز المعدات الرئيسية', 'تحديث الأجهزة', 'توريد معدات التشغيل'],
    T2: ['الإمداد السنوي بالمستهلكات', 'مخزون المستلزمات الحرجة', 'توريد المستلزمات الدورية'],
    T3: ['الصيانة والتشغيل الشامل', 'صيانة المرافق', 'تشغيل المحطات والمنشآت'],
    T4: ['التحول الرقمي', 'البنية التحتية التقنية', 'تحديث الأنظمة'],
    T5: ['الدراسات والاستشارات', 'مكتب إدارة المشاريع', 'التطوير المؤسسي']
  };
  const SITE = { C1: 'مستشفيات التجمع', C2: 'الأمانة', C3: 'الجامعة', C4: 'الهيئة الملكية', C5: 'شبكات المياه', C6: 'المستشفى التخصصي' };
  const usedStem = {};
  const projectName = (c, L) => { const tc = {}; L.forEach(r => tc[r.type] = (tc[r.type] || 0) + 1); const t = Object.entries(tc).sort((a, b) => b[1] - a[1])[0][0], k = c + t, i = usedStem[k] = (usedStem[k] ?? -1) + 1; return { name: `مشروع ${STEM[t][i % 3]} · ${SITE[c]}`, main: t }; };
  const pairs = {};
  R.filter(r => r.supplier && r.st !== 'wait').forEach(r => (pairs[r.client + '|' + r.supplier] = pairs[r.client + '|' + r.supplier] || []).push(r));
  Object.entries(pairs).forEach(([k, L], i) => {
    const [c, v] = k.split('|'), a = agg(L), id = 'D' + (i + 1), C = M.clients.find(x => x.id === c), V = M.suppliers.find(x => x.id === v), P = projectName(c, L);
    nodes.push(Object.assign({ id, t: 'deal', n: P.name, client: C.short, supplier: V.short, d: `العميل: ${C.n} · المورد المنفّذ: ${V.n}. يضم ${pl(a.cnt, 'طلبات', 'طلبًا', 'طلبًا واحدًا', 'طلبين')} مرساة بقيمة ${sar(a.val)} ر.س.`, y: mon(Math.min(...L.map(r => r.created))), s: net(a), types: [...new Set(L.map(r => r.type))], reqs: L.map(r => r.id) }, a));
    edges.push({ a: c, b: id, r: 'asks' }, { a: id, b: v, r: 'serves' });
    [...new Set(L.map(r => r.type))].forEach(t => edges.push({ a: id, b: t, r: 'kind' }));
  });
  /* التأخير: مجموعة لكل مورد لديه طلبات متأخرة الآن */
  M.suppliers.forEach((v, i) => {
    const L = R.filter(r => r.supplier === v.id && r.st === 'late'); if (!L.length) return;
    const id = 'K' + (i + 1), first = Math.min(...L.map(r => r.created + r.offerDays + r.sla));
    nodes.push({ id, t: 'issue', n: `تأخير لدى ${v.short}`, d: `${pl(L.length, 'طلبات', 'طلبًا', 'طلب واحد', 'طلبان')} تجاوزت مدة التنفيذ ولم تكتمل بعد.`, y: mon(first), cnt: L.length, late: L.length, val: L.reduce((a, r) => a + r.value, 0), st: { late: L.length }, reqs: L.map(r => r.id) });
    edges.push({ a: id, b: v.id, r: 'cause', y: mon(first) });
    Object.entries(pairs).forEach(([k, P], j) => { if (k.endsWith('|' + v.id) && P.some(r => r.st === 'late')) edges.push({ a: id, b: 'D' + (j + 1), r: 'hits', y: mon(first) }); });
  });

  const cIdx = Object.fromEntries(M.clients.map((c, i) => [c.id, i])), vIdx = Object.fromEntries(M.suppliers.map((v, i) => [v.id, i]));
  const arc = (i, n, side) => { const a = (i - (n - 1) / 2) / Math.max(1, n - 1) * 1.9; return [side * (560 * Math.cos(a * .5)), 360 * Math.sin(a * .9)]; };
  const ST_LIST = s => Object.entries(s || {}).filter(([, v]) => v).map(([k, v]) => `${SN[k]} ${v}`).join(' · ');

  window.JEEM_NETWORK = {
    types: {
      client:   { n: 'عميل',             pl: 'العملاء',            col: '#127DBD', shape: 'circle',  r: 18 },
      supplier: { n: 'مورد',             pl: 'الموردون',           col: '#00BAB3', shape: 'circle',  r: 15, major: true },
      deal:     { n: 'مشروع',            pl: 'المشاريع',           col: '#6E62B0', shape: 'square',  r: 7, minor: true },
      type:     { n: 'نوع طلب',          pl: 'أنواع الطلبات',      col: '#28B6E6', shape: 'hex',     r: 12, major: true },
      issue:    { n: 'تأخير',            pl: 'التأخيرات',          col: '#D9822B', shape: 'diamond', r: 9, minor: true },
      platform: { n: 'المنصة',           pl: 'المنصة',             col: '#8A94A0', shape: 'circle',  r: 26, outline: true, major: true }
    },
    rels: { offers: 'تطرح طلبات', asks: 'يملك', serves: 'ينفّذه', kind: 'يتضمن', cause: 'لدى المورد', hits: 'يعطّل' },
    nodes, edges,
    asks: [],
    cfg: {
      hub: 'client', issue: 'issue',
      hubs: M.clients.map(c => ({ id: c.id, label: c.short })), hubsAll: 'كل العملاء',
      anchor: .12,
      anchorFor: n => n.t === 'client' ? arc(cIdx[n.id], M.clients.length, 1) : n.t === 'supplier' ? arc(vIdx[n.id], M.suppliers.length, -1) : n.t === 'platform' ? [0, 0] : null,
      springs: { offers: 90, asks: 150, serves: 150, kind: 200, cause: 90, hits: 160 },
      springK: { offers: .08, asks: .06, serves: .06, kind: .006, cause: .03, hits: .01 },
      dashed: ['cause', 'hits'], far: ['kind'],
      radius: n => n.t === 'deal' ? Math.min(9, n.cnt * 1.2) : n.t === 'supplier' || n.t === 'client' ? Math.min(10, n.cnt / 4) : n.t === 'issue' ? Math.min(6, n.cnt * 1.5) : n.t === 'type' ? Math.min(6, n.cnt / 9) : 0,
      time: { min: 1, max: 9, ticks: M.months.map(m => m.slice(0, 3)), out: m => MONTHS[m - 1], play: 'تشغيل نمو المشاريع شهرًا بشهر' },
      since: n => n.t === 'deal' ? 'بدأ بأول طلب في ' + MONTHS[n.y - 1] : n.t === 'issue' ? 'بدأ في ' + MONTHS[n.y - 1] : 'منذ يناير',
      scopeAll: 'كل العملاء والموردين',
      /* عند تحديد عميل أو مورد: تتنوّر مشاريعه والطرف المقابل في كل مشروع (بدون أنواع الطلبات حتى لا تتشتت) */
      focus: (n, u) => { const s = new Set([n.id, ...n.nb]); if (n.t === 'client' || n.t === 'supplier') [...n.nb].forEach(x => { const d = u.byId[x]; if (d.t === 'deal') d.nb.forEach(y => { const t = u.byId[y].t; if (t === 'client' || t === 'supplier' || t === 'issue') s.add(y); }); }); if (n.t === 'deal') [...n.nb].forEach(x => s.add(x)); return s; },
      focusEdge: (e, n) => n.t === 'client' || n.t === 'supplier' ? e.r !== 'kind' && e.r !== 'offers' : true,
      scope: (n, u) => { const s = new Set([n.id, ...n.nb]); if (n.t === 'client' || n.t === 'supplier') [...n.nb].forEach(x => { if (u.byId[x].t === 'deal') u.byId[x].nb.forEach(y => { const t = u.byId[y].t; if (t !== 'type') s.add(y); }); }); return s; },
      tip: n => n.t === 'platform' ? `${n.cnt} طلبًا عبر المنصة` : n.t === 'type' ? `${n.cnt} طلبًا · ${ST_LIST(n.st)}` : n.t === 'issue' ? `${n.cnt} متأخرة` : `${pl(n.cnt, 'طلبات', 'طلبًا', 'طلب واحد', 'طلبان')} · ${sar(n.val)} ر.س${n.late ? ` · ${n.late} متأخر` : ''}${n.t === 'supplier' ? ` · ${n.nCl} عملاء` : n.t === 'client' ? ` · ${n.nSup} موردين` : n.t === 'deal' ? ` · ${n.client} ← ${n.supplier}` : ''}`,
      rev: { offers: 'تطرحه المنصة', asks: 'العميل صاحب المشروع', serves: 'المشاريع التي ينفّذها', kind: 'المشاريع من هذا النوع', cause: 'التأخيرات', hits: 'التأخيرات المؤثرة' },
      kpis: (L, u, sel) => {
        const D = L.filter(n => n.t === 'deal'), sum = k => D.reduce((a, n) => a + (n[k] || 0), 0), stc = k => D.reduce((a, n) => a + ((n.st || {})[k] || 0), 0);
        const fin = stc('done'), on = D.reduce((a, n) => a + (n.onTime != null ? Math.round(n.onTime / 100 * ((n.st || {}).done || 0)) : 0), 0);
        return [
          { t: 'kpi', ti: 'العملاء', v: String(L.filter(n => n.t === 'client').length), d: '', n: 'يطلبون عبر المنصة', ic: 'users' },
          { t: 'kpi', ti: 'الموردون', v: String(L.filter(n => n.t === 'supplier').length), d: '', n: 'يلبّون الطلبات', ic: 'briefcase' },
          { t: 'kpi', ti: 'المشاريع', v: String(D.length), d: '+5', g: true, n: 'بين عملاء وموردين', ic: 'share' },
          { t: 'kpi', ti: 'الطلبات المرساة', v: String(sum('cnt')), d: '+14%', g: true, n: sar(sum('val')) + ' ر.س', ic: 'file' },
          { t: 'kpi', ti: 'متأخرة الآن', v: String(sum('late')), d: '+2', g: false, n: `${stc('prog')} قيد التنفيذ`, ic: 'bell', hl: 1 },
          { t: 'kpi', ti: 'مكتملة في الموعد', v: (fin ? Math.round(on / fin * 100) : 0) + '%', d: '+3 نقاط', g: true, n: `من ${fin} مكتملة`, ic: 'check' }
        ];
      },
      issueTitle: 'التأخيرات حسب المورد', issueVal: n => n.cnt, issueFmt: n => String(n.cnt),
      stats: n => n.t === 'platform' ? [['الطلبات', n.cnt], ['متأخرة', n.late]] : n.t === 'type' ? [['الطلبات', n.cnt], ['متأخرة', n.late]] : n.t === 'issue' ? [['متأخرة', n.cnt], ['القيمة', sar(n.val)]]
        : [['الطلبات', n.cnt], ['القيمة', sar(n.val)], ['في الموعد', n.onTime == null ? '—' : n.onTime + '%']].concat(n.t === 'supplier' ? [['العملاء', n.nCl]] : n.t === 'client' ? [['الموردون', n.nSup]] : []),
      sent: { title: 'صافي الالتزام بالمواعيد', neg: 'متأخر', pos: 'ملتزم' },
      linkVal: o => o.t === 'deal' ? pl(o.cnt, 'طلبات', 'طلبًا', 'طلب', 'طلبان') : o.t === 'issue' ? o.cnt + ' متأخر' : '',
      widgets: (n, u) => {
        const vis = u.N.filter(u.vis), D = (n ? [...n.nb].map(x => u.byId[x]).filter(o => o.t === 'deal') : vis.filter(o => o.t === 'deal')).filter(u.vis);
        const cl = M.clients.filter(c => D.some(d => d.nb.has(c.id))), sv = M.suppliers.filter(v => D.some(d => d.nb.has(v.id)));
        const cell = (c, v) => { const d = D.find(x => x.nb.has(c.id) && x.nb.has(v.id)); return d ? d.cnt : 0; };
        const stTot = k => D.reduce((a, d) => a + ((d.st || {})[k] || 0), 0), stKeys = ['prog', 'late', 'done', 'canc'].filter(k => stTot(k));
        const top = [...D].sort((a, b) => b.cnt - a.cnt).slice(0, 8);
        return [
          cl.length && sv.length ? { t: 'heat', ti: 'مصفوفة العملاء × الموردين', su: 'عدد الطلبات المرساة بين كل عميل ومورد', rows: cl.map(c => c.short), cols: sv.map(v => v.short), vals: cl.map(c => sv.map(v => cell(c, v))), num: 1, sp: 8 } : { t: 'note', ti: 'مصفوفة العملاء × الموردين', tx: ['لا توجد علاقات ضمن النطاق.'], sp: 8 },
          { t: 'hbars', ti: 'الطلبات المرساة حسب الحالة', su: n ? n.n : 'كل المشاريع', x: stKeys.length ? stKeys.map(k => SN[k]) : ['لا يوجد'], vals: stKeys.length ? stKeys.map(stTot) : [0], ns: 1, sp: 4 },
          top.length ? { t: 'table', ti: 'أكبر المشاريع', su: 'مرتبة بعدد الطلبات', rows: top.map(d => d.n), c: [['المشروع', 'l'], ['العميل', 'l'], ['المورد', 'l'], ['الطلبات', 'n'], ['القيمة (ر.س)', 'n'], ['متأخرة', 'n'], ['في الموعد', 'n'], ['الأنواع', 'l']], vals: top.map(d => [null, d.client, d.supplier, d.cnt, sar(d.val), d.late || '—', d.onTime == null ? '—' : d.onTime + '%', d.types.map(t => M.types.find(x => x.id === t).n).join('، ')]), n: top.length, sp: 12 } : null
        ].filter(Boolean);
      },
      cols: [['الطلبات', n => n.cnt ?? '—'], ['متأخرة', n => n.late || '—'], ['القيمة (ر.س)', n => n.val ? sar(n.val) : '—'], ['في الموعد', n => n.onTime == null ? '—' : n.onTime + '%']],
      kw: [['تاخ', 'issue'], ['تأخ', 'issue'], ['مورد', 'supplier'], ['عميل', 'client'], ['عملاء', 'client'], ['مشروع', 'deal'], ['مشاريع', 'deal'], ['علاق', 'deal'], ['نوع', 'type']],
      askNode: n => n.t === 'supplier' ? `من عملاء «${n.n}» وما وضع طلباتهم؟` : n.t === 'client' ? `من موردو «${n.n}» ومن الأكثر تأخيرًا؟` : `ما وضع «${n.n}»؟`,
      answer: L => { const D = L.filter(n => n.t === 'deal'), late = D.reduce((a, d) => a + (d.late || 0), 0), cnt = D.reduce((a, d) => a + d.cnt, 0), worst = [...D].sort((a, b) => (b.late || 0) - (a.late || 0))[0];
        return D.length ? `وجدت ${pl(D.length, 'مشاريع', 'مشروعًا', 'مشروعًا واحدًا', 'مشروعين')} بين عملاء وموردين تضم ${pl(cnt, 'طلبات', 'طلبًا')}، منها ${late} متأخرة${worst && worst.late ? `، أكثرها «${worst.n}» (${worst.late})` : ''}.` : `وجدت ${L.length} عقدة مرتبطة بسؤالك، ظاهرة الآن على الشبكة.`; }
    }
  };

  /* أسئلة جاهزة محسوبة من البيانات */
  const deals = nodes.filter(n => n.t === 'deal'), sups = nodes.filter(n => n.t === 'supplier'), cls = nodes.filter(n => n.t === 'client'), iss = nodes.filter(n => n.t === 'issue');
  const reach = [...sups].sort((a, b) => b.nCl - a.nCl)[0], big = [...deals].sort((a, b) => b.cnt - a.cnt)[0], wi = [...iss].sort((a, b) => b.cnt - a.cnt)[0];
  const lateDeals = deals.filter(d => d.late), lateCl = {}; lateDeals.forEach(d => edges.filter(e => e.b === d.id && e.r === 'asks').forEach(e => lateCl[e.a] = (lateCl[e.a] || 0) + d.late));
  const wc = Object.entries(lateCl).sort((a, b) => b[1] - a[1])[0];
  const supsOf = c => edges.filter(e => e.a === c && e.r === 'asks').map(e => edges.find(x => x.a === e.b && x.r === 'serves').b);
  window.JEEM_NETWORK.asks = [
    { q: 'أي الموردين يخدمون أكبر عدد من العملاء؟', ids: [reach.id, ...edges.filter(e => e.b === reach.id && e.r === 'serves').map(e => e.a), ...edges.filter(e => e.r === 'asks' && edges.some(x => x.a === e.b && x.b === reach.id)).map(e => e.a)], a: `«${reach.n}» يخدم ${reach.nCl} عملاء بـ${pl(reach.cnt, 'طلبات', 'طلبًا')}، والتزامه بالمواعيد ${reach.onTime ?? '—'}%.` },
    { q: 'أين تتركز الطلبات المتأخرة؟', ids: [...iss.map(n => n.id), ...lateDeals.map(d => d.id), ...iss.map(n => edges.find(e => e.a === n.id && e.r === 'cause').b)], a: wi ? `${pl(iss.reduce((a, n) => a + n.cnt, 0), 'طلبات', 'طلبًا')} متأخرة الآن لدى ${pl(iss.length, 'موردين', 'موردًا', 'مورد واحد', 'موردين')}، أكثرها «${wi.n}» (${wi.cnt}). والعميل الأكثر تأثرًا «${cls.find(c => c.id === wc[0]).n}» (${wc[1]}).` : 'لا توجد طلبات متأخرة.' },
    { q: 'ما أكبر مشروع بين عميل ومورد؟', ids: [big.id, ...edges.filter(e => e.a === big.id || e.b === big.id).map(e => e.a === big.id ? e.b : e.a)], a: `الأكبر هو «${big.n}» بين «${big.client}» و«${big.supplier}»: ${pl(big.cnt, 'طلبات', 'طلبًا')} بقيمة ${sar(big.val)} ر.س، وفي الموعد ${big.onTime ?? '—'}%.` },
    { q: 'أي العملاء يعتمدون على مورد واحد أكثر من غيره؟', ids: (() => { const c = [...cls].sort((a, b) => Math.max(...deals.filter(d => edges.some(e => e.a === b.id && e.b === d.id)).map(d => d.cnt / b.cnt)) - Math.max(...deals.filter(d => edges.some(e => e.a === a.id && e.b === d.id)).map(d => d.cnt / a.cnt)))[0]; return [c.id, ...supsOf(c.id), ...edges.filter(e => e.a === c.id).map(e => e.b)]; })(),
      a: (() => { let best = null; cls.forEach(c => deals.filter(d => edges.some(e => e.a === c.id && e.b === d.id)).forEach(d => { const r = d.cnt / c.cnt; if (!best || r > best.r) best = { c, d, r }; })); return `«${best.c.n}» يعتمد على مورد واحد («${best.d.supplier}» في ${best.d.n}) في ${Math.round(best.r * 100)}% من طلباته المرساة، وهذا تركّز يستحق تنويع الموردين.`; })() }
  ];
})();
