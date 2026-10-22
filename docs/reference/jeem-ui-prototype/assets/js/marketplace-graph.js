/* ==========================================================
   جيم — منصة المشتريات: من يطلب ماذا ومِن مَن (لوحة تفاعلية)
   مخطط تدفق (Sankey) من اليمين لليسار:
     العملاء (يطلبون) ← أنواع الطلبات ← المنصة (تجمع الطرفين وتطرح الطلب) ← الموردون (يلبّون)
   - عرض كل شريط = عدد الطلبات، ولونه = نوع الطلب (لون ثابت لكل نوع).
   - الطلبات التي لم تُرسَ بعد تبقى داخل المنصة «بانتظار العروض»، والملغاة قبل الترسية «ملغى».
   - ضغطة على عميل/نوع/مورد/شريط تُنوّر ما يخصه عبر كل الأعمدة، وتحدّث المؤشرات واللوحة والرسوم.
   - تصفية حسب الحالة، وشريط زمني يومي يُظهر نمو العلاقات.
   البيانات: window.JEEM_MARKET (assets/js/marketplace-data.js)
   ========================================================== */
(function () {
  const M = window.JEEM_MARKET, JR = window.JR, root = document.getElementById('mkRoot');
  if (!M || !root) return;
  const $ = (s, el = root) => el.querySelector(s), $$ = (s, el = root) => [...el.querySelectorAll(s)];
  const esc = s => String(s).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
  const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
  const nf = (v, d = 0) => Number(v).toLocaleString('en-US', { maximumFractionDigits: d, minimumFractionDigits: d });
  const sar = v => v >= 1e6 ? nf(v / 1e6, 1) + ' م' : nf(v / 1e3) + ' ألف';
  const pl = (n, few, many, one, two) => n === 1 && one ? one : n === 2 && two ? two : `${nf(n)} ${n >= 3 && n <= 10 ? few : many}`;
  const NS = 'http://www.w3.org/2000/svg', mk = (t, a = {}) => { const e = document.createElementNS(NS, t); for (const k in a) e.setAttribute(k, a[k]); return e; };
  const reduceMotion = matchMedia('(prefers-reduced-motion: reduce)').matches;
  const CL = Object.fromEntries(M.clients.map(c => [c.id, c])), TY = Object.fromEntries(M.types.map(t => [t.id, t])), SU = Object.fromEntries(M.suppliers.map(v => [v.id, v]));
  const SINK = { WAIT: { id: 'WAIT', n: 'بانتظار عروض الموردين', short: 'بانتظار العروض' }, CANC: { id: 'CANC', n: 'ملغى قبل الترسية', short: 'ملغى' } };
  const ST = M.status, STK = Object.keys(ST);
  const dayLabel = d => { const m = Math.min(8, Math.floor(d / 30.4)); return `${Math.round(d - m * 30.4) + 1} ${M.months[m]}`; };

  /* ---------- الحالة ---------- */
  const S = { t: M.today, sel: null, on: new Set(STK), view: 'flow', hl: null };
  /* حالة الطلب في يوم t */
  function stat(r, t) {
    if (t < r.created) return null;
    if (r.cancelAt != null) return t < r.cancelAt ? 'wait' : 'canc';
    const m = r.created + r.offerDays; if (t < m) return 'wait';
    const e = t - m; return e >= r.actual ? 'done' : e > r.sla ? 'late' : 'prog';
  }
  const target = (r, st) => st === 'wait' ? 'WAIT' : st === 'canc' ? 'CANC' : r.supplier;
  const live = () => M.requests.map(r => { const st = stat(r, S.t); return st ? { r, st, to: target(r, st) } : null; }).filter(x => x && S.on.has(x.st));
  const selFn = () => {
    if (S.hl) return x => S.hl.has(x.r.id);
    if (!S.sel) return null;
    const { k, id } = S.sel;
    if (k === 'client') return x => x.r.client === id;
    if (k === 'type') return x => x.r.type === id;
    if (k === 'sup') return x => x.to === id;
    if (k === 'l1') { const [a, b] = id.split('>'); return x => x.r.client === a && x.r.type === b; }
    if (k === 'l2') { const [a, b] = id.split('>'); return x => x.r.type === a && x.to === b; }
    return null;
  };
  const nameOf = id => (CL[id] || TY[id] || SU[id] || SINK[id]).n;
  const shortOf = id => { const o = CL[id] || TY[id] || SU[id] || SINK[id]; return o.short || o.n; };

  /* ---------- الهيكل ---------- */
  const svg = $('#mkSvg'); const VW = 1200, VH = 720, TOP = 128, BOT = 700;
  svg.setAttribute('viewBox', `0 0 ${VW} ${VH}`);
  const COL = { client: { x0: 1012, x1: 1192 }, type: { x0: 712, x1: 862 }, sup: { x0: 12, x1: 192 }, sink: { x0: 412, x1: 572 } };
  const PLAT = { x0: 392, x1: 592 };
  svg.innerHTML = `<defs><linearGradient id="mkPlat" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#EAF6FB"/><stop offset="1" stop-color="#F4FAFD"/></linearGradient></defs>
    <rect class="mk-plat" x="${PLAT.x0}" y="16" width="${PLAT.x1 - PLAT.x0}" height="${VH - 24}" rx="20"/>
    <g class="mk-plat-head" transform="translate(${(PLAT.x0 + PLAT.x1) / 2},40)"><text class="t">منصة المشتريات</text><text class="s" y="18">تجمع الطلبات وتطرحها على الموردين</text></g>
    <text class="mk-colh" x="${COL.client.x1}" y="108">العملاء · يطلبون</text>
    <text class="mk-colh" x="${COL.type.x1}" y="108">أنواع الطلبات</text>
    <text class="mk-colh" x="${COL.sup.x1}" y="108">الموردون · يلبّون</text>`;
  const gRib = mk('g', { class: 'mk-ribs' }), gHi = mk('g', { class: 'mk-his' }), gNodes = mk('g', { class: 'mk-nodes' }), gStats = mk('g', { class: 'mk-pstats', transform: `translate(${(PLAT.x0 + PLAT.x1) / 2},92)` });
  svg.append(gStats, gRib, gHi, gNodes);
  const els = {}; const el = (key, make) => els[key] || (els[key] = make());

  /* ---------- التخطيط ---------- */
  function layout(L) {
    const N = Math.max(1, L.length), GAP = 10, MIN = 30;
    const cols = {
      client: M.clients.map(c => ({ id: c.id, v: L.filter(x => x.r.client === c.id).length })),
      type: M.types.map(t => ({ id: t.id, v: L.filter(x => x.r.type === t.id).length })),
      sup: M.suppliers.map(s => ({ id: s.id, v: L.filter(x => x.to === s.id).length })),
      sink: [SINK.WAIT, SINK.CANC].map(s => ({ id: s.id, v: L.filter(x => x.to === s.id).length }))
    };
    const H = BOT - TOP;
    const k = Math.min(...['client', 'type', 'sup'].map(c => (H - GAP * (cols[c].length - 1) - MIN * cols[c].length * .35) / Math.max(1, cols[c].reduce((a, n) => a + n.v, 0))), 3);
    const place = (arr, y0, gap) => { let y = y0; arr.forEach(n => { n.h = Math.max(n.v ? MIN : 22, n.v * k); n.bh = n.v * k; n.y = y; y += n.h + gap; }); return y - gap; };
    ['client', 'type', 'sup'].forEach(c => { const used = place(cols[c], 0, GAP); const off = TOP + Math.max(0, (H - used) / 2); cols[c].forEach(n => n.y += off); });
    /* بانتظار العروض والملغى داخل المنصة أسفل العمود */
    const used = place(cols.sink, 0, GAP); cols.sink.forEach(n => n.y += BOT - used);
    const nodes = {}; Object.entries(cols).forEach(([c, arr]) => arr.forEach(n => { n.col = c; nodes[n.id] = n; n.sOff = 0; n.tOff = 0; n.y0 = n.y + (n.h - n.bh) / 2; }));
    /* الروابط بترتيب ثابت حتى لا تتقاطع بلا داعٍ */
    const links = [];
    M.clients.forEach(c => M.types.forEach(t => { const R = L.filter(x => x.r.client === c.id && x.r.type === t.id); if (R.length) links.push({ key: c.id + '>' + t.id, lvl: 1, a: c.id, b: t.id, R }); }));
    M.types.forEach(t => [...M.suppliers.map(s => s.id), 'WAIT', 'CANC'].forEach(s => { const R = L.filter(x => x.r.type === t.id && x.to === s); if (R.length) links.push({ key: t.id + '>' + s, lvl: 2, a: t.id, b: s, R }); }));
    links.forEach(l => { const A = nodes[l.a], B = nodes[l.b], w = l.R.length * k; l.w = w; l.s0 = A.y0 + A.sOff; A.sOff += w; l.t0 = B.y0 + B.tOff; B.tOff += w;
      l.x0 = COL[A.col].x0; l.x1 = COL[B.col].x1; });
    return { k, nodes, links, cols };
  }
  const ribbon = (x0, s0, x1, t0, w) => { const xm = (x0 + x1) / 2; return `M${x0},${s0.toFixed(1)} C${xm},${s0.toFixed(1)} ${xm},${t0.toFixed(1)} ${x1},${t0.toFixed(1)} L${x1},${(t0 + w).toFixed(1)} C${xm},${(t0 + w).toFixed(1)} ${xm},${(s0 + w).toFixed(1)} ${x0},${(s0 + w).toFixed(1)} Z`; };

  /* ---------- الرسم ---------- */
  let LAY = null;
  function render() {
    const L = live(), f = selFn(); LAY = layout(L);
    const { nodes, links } = LAY;
    /* الأشرطة */
    const keys = new Set(links.map(l => l.key));
    Object.keys(els).filter(k => k.startsWith('rib:') && !keys.has(k.slice(4))).forEach(k => { els[k].remove(); delete els[k]; const h = els['hi:' + k.slice(4)]; if (h) { h.remove(); delete els['hi:' + k.slice(4)]; } });
    links.forEach(l => {
      const tcol = TY[l.lvl === 1 ? l.b : l.a].col;
      const p = el('rib:' + l.key, () => { const e = mk('path', { class: 'mk-rib', 'data-link': l.key, 'data-lvl': l.lvl }); gRib.append(e); return e; });
      p.setAttribute('d', ribbon(l.x0, l.s0, l.x1, l.t0, l.w)); p.style.fill = tcol;
      const st = STK.map(k => [k, l.R.filter(x => x.st === k).length]).filter(x => x[1]);
      p.dataset.tip = `${shortOf(l.a)} ← ${shortOf(l.b)}|${pl(l.R.length, 'طلبات', 'طلبًا', 'طلب واحد', 'طلبان')} · ${st.map(([k, n]) => `${ST[k].n} ${n}`).join(' · ')}`;
      const n = f ? l.R.filter(f).length : 0;
      const h = el('hi:' + l.key, () => { const e = mk('path', { class: 'mk-hi' }); gHi.append(e); return e; });
      h.setAttribute('d', ribbon(l.x0, l.s0, l.x1, l.t0, n * LAY.k)); h.style.fill = tcol; h.style.opacity = n ? '' : 0;
      p.classList.toggle('is-dim', !!f); p.classList.toggle('is-sel', !!(S.sel && (S.sel.k === 'l1' || S.sel.k === 'l2') && S.sel.id === l.key));
    });
    /* البطاقات */
    Object.values(nodes).forEach(n => {
      const g = el('n:' + n.id, () => {
        const e = mk('g', { class: `mk-node c-${n.col}`, 'data-node': n.id, 'data-col': n.col, tabindex: 0, role: 'button' });
        e.innerHTML = `<rect class="bg" rx="10"/><rect class="stripe" width="4" rx="2"/><text class="n"></text><text class="c"></text><g class="sb"></g>`;
        gNodes.append(e); return e;
      });
      const C = COL[n.col], w = C.x1 - C.x0, R = L.filter(x => n.col === 'client' ? x.r.client === n.id : n.col === 'type' ? x.r.type === n.id : x.to === n.id);
      g.style.transform = `translate(${C.x0}px,${n.y.toFixed(1)}px)`;
      const bg = $('.bg', g); bg.setAttribute('width', w); bg.setAttribute('height', n.h.toFixed(1));
      const sp = $('.stripe', g); sp.setAttribute('x', w - 4); sp.setAttribute('height', n.h.toFixed(1)); sp.style.fill = n.col === 'type' ? TY[n.id].col : n.col === 'sink' ? (n.id === 'WAIT' ? ST.wait.col : ST.canc.col) : '#C9D3DC';
      const big = n.h >= 44;
      const tn = $('.n', g); tn.textContent = shortOf(n.id); tn.setAttribute('x', w - 12); tn.setAttribute('y', big ? 20 : n.h / 2 + 4);
      const tc = $('.c', g); tc.textContent = nf(n.v); tc.setAttribute('x', 10); tc.setAttribute('y', big ? 20 : n.h / 2 + 4);
      /* شريط الحالات داخل البطاقة */
      const sb = $('.sb', g); sb.innerHTML = '';
      if (big && R.length) { let x = 10; const W = w - 24; STK.forEach(k => { const c = R.filter(r => r.st === k).length; if (!c) return; const ww = c / R.length * W; sb.append(mk('rect', { x: (w - 14 - (x - 10) - ww).toFixed(1), y: Math.min(34, n.h - 12), width: Math.max(1, ww - 1).toFixed(1), height: 5, rx: 1.5, style: `fill:${ST[k].col}` })); x += ww; }); }
      const late = R.filter(x => x.st === 'late').length;
      g.dataset.tip = `${nameOf(n.id)}|${pl(n.v, 'طلبات', 'طلبًا', 'طلب واحد', 'طلبان')}${late ? ` · ${late} متأخر` : ''}${n.col === 'sup' && SU[n.id] ? ` · ${new Set(R.map(x => x.r.client)).size} عملاء` : ''}`;
      g.setAttribute('aria-label', `${nameOf(n.id)}: ${n.v} طلبًا`);
      const isSel = S.sel && ((S.sel.k === 'client' && n.col === 'client') || (S.sel.k === 'type' && n.col === 'type') || (S.sel.k === 'sup' && (n.col === 'sup' || n.col === 'sink'))) && S.sel.id === n.id;
      const touched = f ? R.some(f) : true;
      g.classList.toggle('is-sel', !!isSel); g.classList.toggle('is-dim', !!f && !touched); g.classList.toggle('is-empty', !n.v);
    });
    /* إحصاءات المنصة */
    const all = L, matched = all.filter(x => x.r.supplier && x.st !== 'wait' && x.st !== 'canc'), ofr = matched.length ? matched.reduce((a, x) => a + x.r.offerDays, 0) / matched.length : 0;
    const base = all.filter(x => x.st !== 'canc').length;
    gStats.innerHTML = [[`${base ? nf(matched.length / base * 100) : 0}%`, 'الترسية'], [nf(ofr, 1), 'يوم للترسية'], [nf(new Set(matched.map(x => x.r.client + x.r.supplier)).size), 'مشروع']].map(([v, l], i) => `<g transform="translate(${(1 - i) * 64},0)"><text class="v">${v}</text><text class="l" y="15">${l}</text></g>`).join('');
    $('#mkDay').textContent = dayLabel(S.t);
    $('#pgScope').textContent = S.sel ? nameOf(S.sel.k.startsWith('l') ? S.sel.id.split('>')[0] : S.sel.id) + (S.sel.k.startsWith('l') ? ' ← ' + nameOf(S.sel.id.split('>')[1]) : '') : 'كل العملاء والموردين';
    $$('.pg-law button').forEach(b => b.setAttribute('aria-pressed', String(b.dataset.law === (S.sel && S.sel.k === 'client' ? S.sel.id : S.sel ? '' : 'all'))));
    svg.classList.toggle('has-focus', !!f);
    kpis(L, f); panel(L, f); widgets(L, f); if (S.view === 'table') table(L, f);
  }

  /* ---------- المؤشرات ---------- */
  function kpis(L0, f) {
    const L = f ? L0.filter(f) : L0, c = k => L.filter(x => x.st === k).length;
    const fin = L.filter(x => x.st === 'done'), onTime = fin.length ? Math.round(fin.filter(x => x.r.actual <= x.r.sla).length / fin.length * 100) : 0;
    const w = [
      { t: 'kpi', ti: 'إجمالي الطلبات', v: nf(L.length), d: '+14%', g: true, n: sar(L.reduce((a, x) => a + x.r.value, 0)) + ' ر.س', ic: 'file' },
      { t: 'kpi', ti: 'العملاء', v: String(new Set(L.map(x => x.r.client)).size), d: '', n: 'يطلبون عبر المنصة', ic: 'users' },
      { t: 'kpi', ti: 'الموردون', v: String(new Set(L.filter(x => SU[x.to]).map(x => x.to)).size), d: '', n: 'يلبّون الطلبات', ic: 'briefcase' },
      { t: 'kpi', ti: 'بانتظار العروض', v: String(c('wait')), d: '', n: 'لم تُرسَ بعد', ic: 'clock' },
      { t: 'kpi', ti: 'متأخرة', v: String(c('late')), d: '+2', g: false, n: L.length ? `${nf(c('late') / L.length * 100)}% من الطلبات` : '', ic: 'bell', hl: 1 },
      { t: 'kpi', ti: 'مكتملة في الموعد', v: onTime + '%', d: '+3 نقاط', g: true, n: `من ${pl(fin.length, 'طلبات', 'طلبًا', 'طلب واحد', 'طلبين')} مكتملة`, ic: 'check' }
    ];
    $('#pgKpis').innerHTML = JR.renderPage({ id: 'mk' }, { n: 'k', w }, JSON.stringify(S.sel) + S.t);
  }

  /* ---------- اللوحة الجانبية ---------- */
  const tag = k => `<span class="wf-st mk-st st-${k}">${ic(ST[k].icon)}${ST[k].n}</span>`;
  const stBar = R => `<div class="mk-stbar">${STK.map(k => { const c = R.filter(x => x.st === k).length; return c ? `<i style="flex:${c};background:${ST[k].col}" title="${ST[k].n}: ${c}"></i>` : ''; }).join('')}</div>
    <div class="wf-legend">${STK.map(k => { const c = R.filter(x => x.st === k).length; return c ? `<span>${tag(k)}<b class="num">${c}</b></span>` : ''; }).join('')}</div>`;
  const rank = (R, key, go) => { const m = {}; R.forEach(x => { const v = key(x); if (v) m[v] = (m[v] || 0) + 1; }); const a = Object.entries(m).sort((p, q) => q[1] - p[1]), mx = a.length ? a[0][1] : 1;
    return `<ul class="pg-bars">${a.slice(0, 6).map(([id, n]) => `<li><button type="button" data-go="${go(id)}">${TY[id] ? `<i class="mk-sw" style="background:${TY[id].col}"></i>` : ''}<span>${esc(shortOf(id))}</span><span class="trk"><i style="width:${(n / mx * 100).toFixed(0)}%"></i></span><b class="num">${n}</b></button></li>`).join('')}</ul>`; };
  const reqList = R => `<ul class="mk-reqs">${[...R].sort((a, b) => STK.indexOf(a.st === 'late' ? 'wait' : a.st) - STK.indexOf(b.st === 'late' ? 'wait' : b.st) || b.r.created - a.r.created).sort((a, b) => (b.st === 'late') - (a.st === 'late')).slice(0, 8).map(x => `<li><span class="t"><b>${esc(x.r.n)}</b><small>${x.r.code} · ${CL[x.r.client].short}${SU[x.r.supplier] && x.to === x.r.supplier ? ' ← ' + SU[x.r.supplier].short : ''}</small></span>${tag(x.st)}</li>`).join('')}</ul>`;
  function panel(L, f) {
    const P = $('#pgPanel');
    if (!f) {
      P.innerHTML = `<div class="pg-p-head"><span class="pg-p-kicker">${ic('info')}حتى ${dayLabel(S.t)}</span><h2>${pl(L.length, 'طلبات', 'طلبًا')} عبر المنصة</h2><p>اضغط على عميل أو نوع طلب أو مورد أو أي شريط لترى مساره من الطلب حتى التلبية.</p></div>
        <h3 class="pg-p-sec">حالة الطلبات</h3>${stBar(L)}
        <div class="mk-two"><div><h3 class="pg-p-sec">أكثر العملاء طلبًا</h3>${rank(L, x => x.r.client, id => 'client:' + id)}</div><div><h3 class="pg-p-sec">أكثر الموردين تلبية</h3>${rank(L, x => SU[x.to] ? x.to : null, id => 'sup:' + id)}</div></div>`;
      return;
    }
    const R = L.filter(f), { k, id } = S.sel || { k: 'ask' };
    const title = k === 'ask' ? 'نتيجة السؤال' : k.startsWith('l') ? `${shortOf(id.split('>')[0])} ← ${shortOf(id.split('>')[1])}` : nameOf(id);
    const kick = { client: `${ic('users')}عميل`, type: `${ic('layers')}نوع طلب`, sup: `${ic('briefcase')}${SINK[id] ? 'داخل المنصة' : 'مورد'}`, l1: `${ic('share')}عميل ← نوع`, l2: `${ic('share')}نوع ← مورد`, ask: `${ic('spark')}اسأل جيم` }[k];
    const late = R.filter(x => x.st === 'late').length, val = R.reduce((a, x) => a + x.r.value, 0);
    P.innerHTML = `<div class="pg-p-head"><button type="button" class="btn btn-ghost btn-sm pg-back" data-clear>${ic('chev-left')}كل المنصة</button><span class="pg-p-kicker">${kick}</span><h2>${esc(title)}</h2></div>
      <div class="pg-stats"><div><small>الطلبات</small><b class="num">${R.length}</b></div><div><small>القيمة (ر.س)</small><b class="num">${sar(val)}</b></div><div><small>متأخرة</small><b class="num">${late}</b></div></div>
      <h3 class="pg-p-sec">حالة الطلبات</h3>${stBar(R)}
      <div class="mk-two">${k !== 'client' && !(k === 'l1') ? `<div><h3 class="pg-p-sec">العملاء الطالبون</h3>${rank(R, x => x.r.client, i => 'client:' + i)}</div>` : ''}
        ${k !== 'type' ? `<div><h3 class="pg-p-sec">أنواع الطلبات</h3>${rank(R, x => x.r.type, i => 'type:' + i)}</div>` : ''}
        ${k !== 'sup' && k !== 'l2' ? `<div><h3 class="pg-p-sec">الموردون الملبّون</h3>${rank(R, x => SU[x.to] ? x.to : null, i => 'sup:' + i)}</div>` : ''}</div>
      <h3 class="pg-p-sec">الطلبات (المتأخرة أولًا)</h3>${reqList(R)}`;
  }

  /* ---------- الرسوم أسفل المخطط (أرقام فعلية) ---------- */
  function widgets(L0, f) {
    const L = f ? L0.filter(f) : L0;
    const cl = M.clients.filter(c => L.some(x => x.r.client === c.id)), sv = M.suppliers.filter(s => L.some(x => x.to === s.id));
    const heat = cl.length && sv.length ? { t: 'heat', ti: 'مصفوفة العملاء × الموردين', su: 'عدد الطلبات المرساة بين كل عميل ومورد', rows: cl.map(c => c.short), cols: sv.map(s => s.short), vals: cl.map(c => sv.map(s => L.filter(x => x.r.client === c.id && x.to === s.id).length)), num: 1, sp: 12 } : null;
    const per = M.types.map(t => { const D = L.filter(x => x.r.type === t.id && x.st === 'done'); return [t.n, D.length ? D.reduce((a, x) => a + x.r.actual, 0) / D.length : 0, t.id]; }).filter(x => x[1]);
    const lateBy = M.suppliers.map(s => { const R = L.filter(x => x.to === s.id && (x.st === 'late' || x.st === 'done' || x.st === 'prog')); return [s.short, R.length ? R.filter(x => x.st === 'late' || (x.st === 'done' && x.r.actual > x.r.sla)).length / R.length * 100 : null]; }).filter(x => x[1] != null).sort((a, b) => b[1] - a[1]);
    const w = [heat,
      per.length ? { t: 'bullet', ti: 'زمن التنفيذ الفعلي مقابل المدة لكل نوع', su: 'متوسط الأيام للطلبات المكتملة · الخط = المدة المستهدفة', it: per.map(([n, v, id]) => [n, v, M.requests.find(r => r.type === id).sla]), sp: 6 } : null,
      lateBy.length ? { t: 'hbars', ti: 'نسبة التأخير لدى كل مورد', su: 'المتأخرة أو المكتملة بعد المدة', x: lateBy.map(x => x[0]), vals: lateBy.map(x => x[1]), k: 'pct', ns: 1, sp: 6 } : null
    ].filter(Boolean);
    $('#pgGrid').innerHTML = w.length ? JR.renderPage({ id: 'mk-w' }, { n: 'w', w }, 'x') : '';
  }

  /* ---------- جدول ---------- */
  function table(L0, f) {
    const L = (f ? L0.filter(f) : L0).sort((a, b) => (b.st === 'late') - (a.st === 'late') || b.r.created - a.r.created);
    $('#pgTable').innerHTML = `<table class="table"><caption class="sr-only">الطلبات</caption><thead><tr><th>الرقم</th><th>الطلب</th><th>العميل</th><th>النوع</th><th>المورد</th><th>القيمة</th><th>الحالة</th><th>تاريخ الطلب</th></tr></thead><tbody>${L.map(x => `<tr><td><span class="num mono">${x.r.code}</span></td><td>${esc(x.r.n)}</td><td>${CL[x.r.client].short}</td><td><i class="mk-sw" style="background:${TY[x.r.type].col}"></i> ${TY[x.r.type].n}</td><td>${SU[x.to] ? SU[x.to].short : '—'}</td><td><span class="num">${sar(x.r.value)}</span></td><td>${tag(x.st)}</td><td><span class="num">${dayLabel(x.r.created)}</span></td></tr>`).join('')}</tbody></table>`;
  }
  $$('.pg-view button').forEach(b => b.addEventListener('click', () => {
    S.view = b.dataset.view; $$('.pg-view button').forEach(x => x.setAttribute('aria-pressed', String(x === b)));
    $('#pgTable').hidden = S.view !== 'table'; svg.style.visibility = S.view === 'table' ? 'hidden' : ''; render();
  }));

  /* ---------- التفاعل ---------- */
  const select = sel => { S.sel = sel; S.hl = null; $('#pgAnswer').hidden = true; render(); };
  const same = (a, b) => a && b && a.k === b.k && a.id === b.id;
  svg.addEventListener('click', e => {
    const n = e.target.closest('[data-node]'), l = e.target.closest('[data-link]');
    if (n) { const col = n.dataset.col, s = { k: col === 'client' ? 'client' : col === 'type' ? 'type' : 'sup', id: n.dataset.node }; return select(same(S.sel, s) ? null : s); }
    if (l) { const s = { k: 'l' + l.dataset.lvl, id: l.dataset.link }; return select(same(S.sel, s) ? null : s); }
    select(null);
  });
  svg.addEventListener('keydown', e => { if ((e.key === 'Enter' || e.key === ' ') && e.target.matches('[data-node]')) { e.preventDefault(); e.target.dispatchEvent(new MouseEvent('click', { bubbles: true })); } });
  root.addEventListener('click', e => {
    const go = e.target.closest('#pgPanel [data-go]'); if (go) { const [k, id] = go.dataset.go.split(':'); return select({ k, id }); }
    if (e.target.closest('[data-clear]')) return select(null);
  });
  root.addEventListener('keydown', e => { if (e.key === 'Escape') select(null); });

  $('#pgTypes').innerHTML = STK.map(k => `<button type="button" class="pg-type mk-chip" data-st="${k}" aria-pressed="true"><i class="mk-dot" style="background:${ST[k].col}"></i>${ic(ST[k].icon)}<span>${ST[k].n}</span></button>`).join('');
  $$('[data-st]').forEach(b => b.addEventListener('click', () => { const k = b.dataset.st; if (S.on.has(k)) { if (S.on.size === 1) return; S.on.delete(k); } else S.on.add(k); b.setAttribute('aria-pressed', String(S.on.has(k))); render(); }));
  $('.pg-law').innerHTML = `<button type="button" data-law="all" aria-pressed="true">الكل</button>` + M.clients.map(c => `<button type="button" data-law="${c.id}" aria-pressed="false">${c.short}</button>`).join('');
  $$('.pg-law button').forEach(b => b.addEventListener('click', () => select(b.dataset.law === 'all' ? null : { k: 'client', id: b.dataset.law })));

  /* الشريط الزمني */
  const yr = $('#pgYear'), play = $('#pgPlay');
  yr.min = 0; yr.max = M.today; yr.value = M.today;
  $('.pg-ticks').innerHTML = M.months.map(m => `<i>${m.slice(0, 3)}</i>`).join('');
  yr.addEventListener('input', () => { stopPlay(); S.t = +yr.value; render(); });
  let timer = null;
  const stopPlay = () => { clearInterval(timer); timer = null; play.innerHTML = ic('play'); play.setAttribute('aria-label', 'تشغيل نمو العلاقات يومًا بيوم'); };
  play.addEventListener('click', () => {
    if (timer) return stopPlay();
    play.innerHTML = ic('pause'); play.setAttribute('aria-label', 'إيقاف التشغيل');
    if (S.t >= M.today) S.t = 10;
    timer = setInterval(() => { S.t = Math.min(M.today, S.t + 4); yr.value = S.t; render(); if (S.t >= M.today) stopPlay(); }, reduceMotion ? 400 : 220);
  });
  stopPlay();

  /* البحث */
  const norm = s => s.replace(/[أإآ]/g, 'ا').replace(/ة/g, 'ه').replace(/ى/g, 'ي').trim();
  const search = $('#pgSearch'), sug = $('#pgSug');
  search.addEventListener('input', () => {
    const q = norm(search.value); if (!q) { sug.hidden = true; return; }
    const A = [...M.clients.map(c => ['client', c]), ...M.types.map(t => ['type', t]), ...M.suppliers.map(s => ['sup', s])].filter(([, o]) => norm(o.n).includes(q)).slice(0, 8);
    sug.innerHTML = A.map(([k, o]) => `<li><button type="button" data-go="${k}:${o.id}">${ic(k === 'client' ? 'users' : k === 'type' ? 'layers' : 'briefcase')}<span>${esc(o.n)}</span><small>${k === 'client' ? 'عميل' : k === 'type' ? 'نوع طلب' : 'مورد'}</small></button></li>`).join('') || '<li class="muted">لا نتائج</li>';
    sug.hidden = false;
  });
  sug.addEventListener('click', e => { const b = e.target.closest('[data-go]'); if (b) { const [k, id] = b.dataset.go.split(':'); select({ k, id }); } sug.hidden = true; search.value = ''; });
  document.addEventListener('click', e => { if (!e.target.closest('.pg-search')) sug.hidden = true; });

  /* اسأل جيم */
  const askIn = $('#pgAskIn'), ans = $('#pgAnswer');
  $('#pgAskChips').innerHTML = M.asks.map((a, i) => `<button type="button" class="chip" data-ask="${i}">${esc(a.q)}</button>`).join('');
  $$('[data-ask]').forEach(b => b.addEventListener('click', () => ask(M.asks[+b.dataset.ask].q)));
  $('#pgAsk').addEventListener('submit', e => { e.preventDefault(); if (askIn.value.trim()) ask(askIn.value.trim()); });
  let typing = null;
  const say = txt => { ans.hidden = false; const e = $('.pg-ans-txt', ans); clearInterval(typing); if (reduceMotion) { e.textContent = txt; return; } let i = 0; e.textContent = ''; ans.classList.add('typing'); typing = setInterval(() => { i += 3; e.textContent = txt.slice(0, i); if (i >= txt.length) { clearInterval(typing); ans.classList.remove('typing'); } }, 18); };
  const top = (R, key) => { const m = {}; R.forEach(x => { const k = key(x); if (k) m[k] = (m[k] || 0) + 1; }); return Object.entries(m).sort((a, b) => b[1] - a[1]); };
  function ask(q) {
    askIn.value = q; const nq = norm(q), all = M.requests.map(r => { const st = stat(r, S.t); return st ? { r, st, to: target(r, st) } : null; }).filter(Boolean);
    let a = M.asks.find(x => x.q === q) || (/متاخر|تاخير/.test(nq) ? M.asks[0] : /عروض|انتظار|ترسي/.test(nq) ? M.asks[2] : /علاقه|مشروع|اكبر/.test(nq) ? M.asks[3] : /مورد/.test(nq) ? M.asks[1] : null);
    if (!a) {
      const hit = [...M.clients.map(c => ['client', c]), ...M.types.map(t => ['type', t]), ...M.suppliers.map(s => ['sup', s])].find(([, o]) => norm(o.n).split(' ').some(w => w.length > 3 && nq.includes(w)));
      if (hit) { select({ k: hit[0], id: hit[1].id }); return say(`عرضت كل ما يخص «${hit[1].n}» على المخطط.`); }
      return say('لم أفهم السؤال بعد. جرّب أحد الأسئلة المقترحة أو اذكر اسم عميل أو مورد.');
    }
    if (a.kind === 'late') {
      const R = all.filter(x => x.st === 'late'); S.sel = null; S.hl = new Set(R.map(x => x.r.id)); S.on = new Set(STK); syncChips(); render();
      const ts = top(R, x => x.r.type), ss = top(R, x => x.to), cs = top(R, x => x.r.client);
      say(R.length ? `${pl(R.length, 'طلبات', 'طلبًا')} متأخرة الآن. أكثرها في «${TY[ts[0][0]].n}» (${ts[0][1]})، ولدى المورد «${SU[ss[0][0]].short}» (${ss[0][1]})، ولعميل «${CL[cs[0][0]].short}» (${cs[0][1]}).` : 'لا توجد طلبات متأخرة الآن.');
    } else if (a.kind === 'reach') {
      const R = all.filter(x => SU[x.to]), m = {}; R.forEach(x => (m[x.to] = m[x.to] || new Set()).add(x.r.client));
      const s = Object.entries(m).sort((p, q2) => q2[1].size - p[1].size); select({ k: 'sup', id: s[0][0] });
      say(`«${SU[s[0][0]].n}» يخدم ${s[0][1].size} عملاء، يليه «${SU[s[1][0]].short}» (${s[1][1].size}). كلما خدم المورد عملاء أكثر زاد أثر تأخره على المنصة.`);
    } else if (a.kind === 'wait') {
      const R = all.filter(x => x.st === 'wait'); select({ k: 'sup', id: 'WAIT' });
      const ts = top(R, x => x.r.type), avg = R.length ? R.reduce((s, x) => s + (S.t - x.r.created), 0) / R.length : 0;
      say(R.length ? `${pl(R.length, 'طلبات', 'طلبًا')} بانتظار عروض الموردين، متوسط انتظارها ${nf(avg, 1)} يومًا، وأكثرها «${TY[ts[0][0]].n}» (${ts[0][1]}).` : 'لا توجد طلبات بانتظار العروض.');
    } else if (a.kind === 'pair') {
      const p = top(all.filter(x => SU[x.to]), x => x.r.client + '|' + x.to)[0], [c, s] = p[0].split('|');
      S.sel = null; S.hl = new Set(all.filter(x => x.r.client === c && x.to === s).map(x => x.r.id)); render();
      say(`أكبر مشروع بين «${CL[c].n}» و«${SU[s].n}»: ${pl(p[1], 'طلبات', 'طلبًا')} مُرساة عبر المنصة.`);
    }
  }
  const syncChips = () => $$('[data-st]').forEach(b => b.setAttribute('aria-pressed', String(S.on.has(b.dataset.st))));
  $('#pgAnsClear').addEventListener('click', () => select(null));

  render();
})();
