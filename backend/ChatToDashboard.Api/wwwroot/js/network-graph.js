/* ==========================================================
   جيم — محرك اللوحات الشبكية التفاعلية (عام لأي مجال)
   رسم شبكي بمحاكاة قوى مكتوبة يدويًا (Vanilla JS + SVG، بلا مكتبات):
   - ضغطة على عقدة: تتوسط الشاشة، يتنوّر جيرانها، وتتحدث اللوحة الجانبية والرسوم أسفلها.
   - تمرير الماوس: تلميح بالاسم والقيم، وعلى الخط: نوع العلاقة.
   - سحب العقد، تكبير بعجلة الماوس، سحب الخلفية للتحريك.
   - تصفية حسب النوع، شريط زمني بتشغيل تلقائي، بحث، عرض كجدول، و«اسأل جيم».

   البيانات الحقيقية (nodes/edges) تُجلب من GET /api/graph/{networkKey} — لا توجد بيانات
   تجريبية مكتوبة هنا. كل ما يخص المجال (الأنواع، العلاقات، أسماء المؤشرات، الرسوم، الأعمدة،
   الكلمات المفتاحية) يبقى كإعداد في ملف المجال الخاص به (مثل policy-network.js)، الذي يضبط
   window.JEEM_NETWORK_CONFIG = { networkKey, types, rels, cfg, asks } قبل تحميل هذا الملف.
   ========================================================== */
(function () {
  // #pgRoot lives inside a `.screen` that's `display:none` until the user opens this tab (see
  // app.css's .screen{display:none}/.screen.active{display:flex}), so this can't just run at
  // script-load time the way the reference's own standalone page did — the stage would measure
  // 0×0 and the camera's initial fit() would compute garbage. app.js's showScreen() dispatch
  // calls this once the screen is actually visible (same lazy-load convention as
  // loadAnalystScreen/loadSourcesScreen elsewhere in this app).
  let started = false;
  window.loadPolicyNetworkScreen = function () {
    if (started) return;
    started = true;
    const CFG = window.JEEM_NETWORK_CONFIG;
    const root = document.getElementById('pgRoot');
    if (!CFG || !root) return;

    fetch('/api/graph/' + encodeURIComponent(CFG.networkKey))
      .then(res => res.ok ? res.json() : Promise.reject(res.status))
      .then(data => start({ types: CFG.types, rels: CFG.rels, cfg: CFG.cfg, asks: CFG.asks || [], nodes: data.nodes, edges: data.edges }))
      .catch(() => {
        const stage = root.querySelector('.pg-stage');
        if (stage) stage.innerHTML = '<p class="muted" style="padding:24px">تعذّر تحميل بيانات الشبكة — لا توجد بيانات متاحة لهذا المشروع بعد.</p>';
      });
  };

  function start(G) {
    const root = document.getElementById('pgRoot');
  const JR = window.JR;
  if (!JR) return;
  const $ = (s, el = root) => el.querySelector(s), $$ = (s, el = root) => [...el.querySelectorAll(s)];
  const esc = s => String(s).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
  const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
  const nf = v => Number(v).toLocaleString('en-US');
  const NS = 'http://www.w3.org/2000/svg';
  const T = G.types, TYPES = Object.keys(T);
  const C = G.cfg, HUB = C.hub, ISSUE = C.issue;
  const reduceMotion = matchMedia('(prefers-reduced-motion: reduce)').matches;

  /* ---------- النموذج ---------- */
  const N = G.nodes.map(n => Object.assign({}, n));
  const byId = Object.fromEntries(N.map(n => [n.id, n]));
  const E = G.edges.map(e => Object.assign({}, e, { A: byId[e.a], B: byId[e.b] })).filter(e => e.A && e.B);
  N.forEach(n => { n.nb = new Set(); n.deg = 0; });
  E.forEach(e => { e.A.nb.add(e.B.id); e.B.nb.add(e.A.id); e.A.deg++; e.B.deg++; e.y = Math.max(e.y || 0, e.A.y, e.B.y); });
  const radius = n => T[n.t].r + (C.radius ? C.radius(n) : 0);
  N.forEach(n => { n.r = radius(n); });

  /* موضع ابتدائي ثابت (بذرة) حتى يظهر الشكل نفسه في كل مرة */
  let sd = 7; const rnd = () => (sd = (sd * 16807) % 2147483647) / 2147483647;
  const LAWPOS = C.hubPos || {};
  N.forEach(n => {
    const A0 = C.anchorFor ? C.anchorFor(n) : LAWPOS[n.id];
    if (A0) { n.x = A0[0] + (rnd() - .5) * 10; n.yy = A0[1] + (rnd() - .5) * 10; }
    else { const a = rnd() * Math.PI * 2, d = 120 + rnd() * 220; n.x = Math.cos(a) * d; n.yy = Math.sin(a) * d * .7; }
    n.vx = 0; n.vy = 0;
  });

  const U = { nf, get byId() { return byId; }, get N() { return N; }, vis: n => visible(n), T };
  /* ---------- الحالة ---------- */
  const S = { on: new Set(TYPES), year: C.time.max, sel: null, hl: null, law: 'all', view: 'graph', k: 1, tx: 0, ty: 0, alpha: 1, q: '' };
  const visible = n => S.on.has(n.t) && n.y <= S.year;
  const edgeVisible = e => visible(e.A) && visible(e.B) && e.y <= S.year;

  /* ---------- البناء ---------- */
  const svg = $('#pgSvg'), world = document.createElementNS(NS, 'g'), gE = document.createElementNS(NS, 'g'), gL = document.createElementNS(NS, 'g'), gN = document.createElementNS(NS, 'g');
  world.append(gE, gL, gN); svg.append(world);
  const shapeSvg = (t, r) => {
    switch (T[t].shape) {
      case 'square': return `<rect x="${-r}" y="${-r}" width="${2 * r}" height="${2 * r}" rx="3"/>`;
      case 'diamond': return `<path d="M0 ${-r * 1.2} L${r * 1.2} 0 L0 ${r * 1.2} L${-r * 1.2} 0Z"/>`;
      case 'hex': { const p = Array.from({ length: 6 }, (_, i) => { const a = Math.PI / 3 * i + Math.PI / 6; return `${(Math.cos(a) * r * 1.1).toFixed(1)},${(Math.sin(a) * r * 1.1).toFixed(1)}`; }); return `<polygon points="${p.join(' ')}"/>`; }
      default: return `<circle r="${r}"/>`;
    }
  };
  const glyph = (t, s = 14) => `<svg class="pg-glyph" viewBox="-8 -8 16 16" width="${s}" height="${s}" aria-hidden="true" style="fill:${T[t].col}">${shapeSvg(t, 5.5)}</svg>`;
  /* تقسيم الاسم الطويل إلى سطرين */
  const lines = s => { if (s.length <= 20) return [s]; const out = []; let a = ''; s.split(' ').forEach(x => { if ((a + ' ' + x).trim().length > 20 && a) { out.push(a); a = x; } else a = (a + ' ' + x).trim(); }); out.push(a); return out.length > 2 ? [out[0], out.slice(1).join(' ').slice(0, 18) + '…'] : out; };

  E.forEach(e => {
    const l = document.createElementNS(NS, 'line'); l.setAttribute('class', `pg-e r-${e.r}`); e.el = l;
    const hit = document.createElementNS(NS, 'line'); hit.setAttribute('class', 'pg-ehit'); hit.dataset.tip = `${e.A.n}|${G.rels[e.r]} ← ${e.B.n}`; if ((C.dashed || []).includes(e.r)) l.classList.add('dashed'); if ((C.far || []).includes(e.r)) l.classList.add('far'); e.hit = hit;
    gE.append(l, hit);
    const lab = document.createElementNS(NS, 'text'); lab.setAttribute('class', 'pg-elab'); lab.textContent = G.rels[e.r]; e.lab = lab; gL.append(lab);
  });
  N.forEach(n => {
    const g = document.createElementNS(NS, 'g');
    g.setAttribute('class', `pg-n t-${n.t}`); g.setAttribute('tabindex', '0'); g.setAttribute('role', 'button');
    g.setAttribute('aria-label', `${T[n.t].n}: ${n.n}`);
    g.dataset.id = n.id;
    g.dataset.tip = `${n.n}|${T[n.t].n} · ${C.tip(n, U)}`;
    if (T[n.t].outline) g.classList.add('outline');
    if (T[n.t].minor) g.classList.add('minor'); if (n.t === HUB || T[n.t].major) g.classList.add('hub');
    g.style.setProperty('--c', T[n.t].col);
    g.innerHTML = `${n.t === HUB ? `<circle class="pg-halo" r="${n.r + 7}"/>` : ''}<circle class="pg-ring" r="${n.r + 5}"/><g class="pg-shape">${shapeSvg(n.t, n.r)}</g>
      <text class="pg-lab" y="${n.r + 14}">${lines(n.n).map((l, i) => `<tspan x="0" dy="${i ? 13 : 0}">${esc(l)}</tspan>`).join('')}</text>`;
    n.el = g; gN.append(g);
  });

  /* ---------- محاكاة القوى ---------- */
  const springLen = e => ((C.springs || {})[e.r] || 130) + e.A.r + e.B.r;
  function tick() {
    const V = N.filter(visible), a = S.alpha;
    for (let i = 0; i < V.length; i++) for (let j = i + 1; j < V.length; j++) {
      const p = V[i], q = V[j]; let dx = q.x - p.x, dy = q.yy - p.yy, d2 = dx * dx + dy * dy || .01; const d = Math.sqrt(d2);
      let f = 5200 / d2; const min = p.r + q.r + 44; if (d < min) f += (min - d) * .35;
      dx /= d; dy /= d; p.vx -= dx * f; p.vy -= dy * f; q.vx += dx * f; q.vy += dy * f;
    }
    E.forEach(e => { if (!edgeVisible(e)) return; const p = e.A, q = e.B; let dx = q.x - p.x, dy = q.yy - p.yy; const d = Math.sqrt(dx * dx + dy * dy) || .01, f = (d - springLen(e)) * ((C.springK || {})[e.r] || .045); dx /= d; dy /= d;
      const wp = p.t === HUB ? .35 : 1, wq = q.t === HUB ? .35 : 1; p.vx += dx * f * wp; p.vy += dy * f * wp; q.vx -= dx * f * wq; q.vy -= dy * f * wq; });
    V.forEach(n => {
      /* المحاور الرئيسية تُشد إلى مواقعها المحددة (إن وُجدت) فتبقى الشبكة مقروءة كمجموعات حول كل محور */
      const anc = C.anchorFor ? C.anchorFor(n) : n.t === HUB && LAWPOS[n.id];
      if (anc && C.anchor) { n.vx -= (n.x - anc[0]) * C.anchor; n.vy -= (n.yy - anc[1]) * C.anchor; }
      else { const gk = n.t === HUB ? .014 : .005; n.vx -= n.x * gk; n.vy -= n.yy * gk; }
      if (n.fx != null) { n.x = n.fx; n.yy = n.fy; n.vx = n.vy = 0; return; }
      n.vx *= .58; n.vy *= .58; n.x += Math.max(-30, Math.min(30, n.vx)) * a; n.yy += Math.max(-30, Math.min(30, n.vy)) * a;
    });
    S.alpha = Math.max(0, S.alpha * .985 - .0005);
  }
  for (let i = 0; i < 160; i++) tick(); S.alpha = .35;

  function draw() {
    E.forEach(e => {
      const on = edgeVisible(e);
      e.el.style.display = e.hit.style.display = on ? '' : 'none';
      if (!on) { e.lab.style.display = 'none'; return; }
      for (const l of [e.el, e.hit]) { l.setAttribute('x1', e.A.x.toFixed(1)); l.setAttribute('y1', e.A.yy.toFixed(1)); l.setAttribute('x2', e.B.x.toFixed(1)); l.setAttribute('y2', e.B.yy.toFixed(1)); }
      /* تسمية العلاقة قرب الطرف الآخر (بعيدًا عن العقدة المحددة) حتى لا تتكدس حولها */
      const t = S.sel === e.a ? .64 : S.sel === e.b ? .36 : .5;
      e.lab.setAttribute('x', (e.A.x + (e.B.x - e.A.x) * t).toFixed(1)); e.lab.setAttribute('y', (e.A.yy + (e.B.yy - e.A.yy) * t - 4).toFixed(1));
    });
    N.forEach(n => { n.el.setAttribute('transform', `translate(${n.x.toFixed(1)},${n.yy.toFixed(1)})`); });
  }
  let raf = 0;
  const loop = () => { if (S.alpha > .004) tick(); draw(); applyCam(); raf = (S.alpha > .004 || camAnim) ? requestAnimationFrame(loop) : 0; };
  const kick = (a = .4) => { S.alpha = Math.max(S.alpha, reduceMotion ? 0 : a); if (reduceMotion) { for (let i = 0; i < 120; i++) { S.alpha = .3; tick(); } S.alpha = 0; } if (!raf) raf = requestAnimationFrame(loop); };

  /* ---------- الكاميرا: تكبير وتحريك ---------- */
  const stage = $('.pg-stage');
  let camAnim = null;
  const size = () => { const r = stage.getBoundingClientRect(); return [r.width, r.height]; };
  function applyCam() {
    if (camAnim) { const t = Math.min(1, (performance.now() - camAnim.t0) / camAnim.d), e = 1 - Math.pow(1 - t, 3);
      S.k = camAnim.k0 + (camAnim.k1 - camAnim.k0) * e; S.tx = camAnim.x0 + (camAnim.x1 - camAnim.x0) * e; S.ty = camAnim.y0 + (camAnim.y1 - camAnim.y0) * e; if (t >= 1) camAnim = null; }
    const [w, h] = size(); world.setAttribute('transform', `translate(${(w / 2 + S.tx).toFixed(1)},${(h / 2 + S.ty).toFixed(1)}) scale(${S.k.toFixed(3)})`);
    svg.classList.toggle('zoom-in', S.k >= 1.15); svg.classList.toggle('zoom-out', S.k < .75);
    $('#pgZoomVal').textContent = Math.round(S.k * 100) + '%';
  }
  const camTo = (k, tx, ty) => { k = Math.max(.35, Math.min(2.6, k)); if (reduceMotion) { S.k = k; S.tx = tx; S.ty = ty; applyCam(); return; } camAnim = { t0: performance.now(), d: 520, k0: S.k, k1: k, x0: S.tx, x1: tx, y0: S.ty, y1: ty }; if (!raf) raf = requestAnimationFrame(loop); };
  const fit = (ids) => {
    const L = (ids ? N.filter(n => ids.has(n.id)) : N).filter(visible); if (!L.length) return;
    const xs = L.map(n => n.x), ys = L.map(n => n.yy), [w, h] = size();
    const x0 = Math.min(...xs) - 70, x1 = Math.max(...xs) + 70, y0 = Math.min(...ys) - 50, y1 = Math.max(...ys) + 70;
    const k = Math.min(w / (x1 - x0), h / (y1 - y0), ids && L.length < 4 ? 1.6 : 1.9);
    camTo(k, -(x0 + x1) / 2 * k, -(y0 + y1) / 2 * k);
  };
  const centerOn = n => { const k = Math.max(S.k, 1.2); camTo(k, -n.x * k + (size()[0] > 700 ? 0 : 0), -n.yy * k); };

  svg.addEventListener('wheel', e => {
    e.preventDefault(); camAnim = null;
    const r = svg.getBoundingClientRect(), [w, h] = size(), mx = e.clientX - r.left - w / 2, my = e.clientY - r.top - h / 2;
    const k1 = Math.max(.35, Math.min(2.6, S.k * Math.exp(-e.deltaY * .0015))), f = k1 / S.k;
    S.tx = mx - (mx - S.tx) * f; S.ty = my - (my - S.ty) * f; S.k = k1; applyCam();
  }, { passive: false });

  /* سحب الخلفية للتحريك، وسحب العقدة لتحريكها */
  let drag = null, lastTap = {};
  /* النزول داخل عقدة (نقرتان أو Shift+Enter أو زر «انزل داخلها») — تتولاه graph-drill.js */
  const drill = id => root.dispatchEvent(new CustomEvent('pg:drill', { detail: { id } }));
  svg.addEventListener('pointerdown', e => {
    const g = e.target.closest('.pg-n'); camAnim = null;
    drag = { id: g ? g.dataset.id : null, sx: e.clientX, sy: e.clientY, tx: S.tx, ty: S.ty, moved: false };
    svg.setPointerCapture(e.pointerId);
  });
  svg.addEventListener('pointermove', e => {
    if (!drag) return; const dx = e.clientX - drag.sx, dy = e.clientY - drag.sy;
    if (Math.abs(dx) + Math.abs(dy) > 4) drag.moved = true; if (!drag.moved) return;
    if (drag.id) { const n = byId[drag.id], r = svg.getBoundingClientRect(), [w, h] = size(); n.fx = (e.clientX - r.left - w / 2 - S.tx) / S.k; n.fy = (e.clientY - r.top - h / 2 - S.ty) / S.k; n.x = n.fx; n.yy = n.fy; svg.classList.add('dragging'); kick(.25); }
    else { S.tx = drag.tx + dx; S.ty = drag.ty + dy; svg.classList.add('panning'); applyCam(); }
  });
  svg.addEventListener('pointerup', e => {
    if (!drag) return; svg.classList.remove('dragging', 'panning');
    if (drag.id) { const n = byId[drag.id]; n.fx = n.fy = null; if (!drag.moved) { const now = performance.now(); if (lastTap.id === drag.id && now - lastTap.t < 380) { lastTap = {}; drill(drag.id); } else { lastTap = { id: drag.id, t: now }; select(drag.id, true); } } }
    else if (!drag.moved) select(null);
    drag = null;
  });
  gN.addEventListener('keydown', e => { const g = e.target.closest('.pg-n'); if (g && (e.key === 'Enter' || e.key === ' ')) { e.preventDefault(); if (e.shiftKey) drill(g.dataset.id); else select(g.dataset.id, true); } });
  root.addEventListener('keydown', e => { if (e.key === 'Escape') { clearAsk(); select(null); } });
  $('#pgZoomIn').addEventListener('click', () => camTo(S.k * 1.25, S.tx * 1.25, S.ty * 1.25));
  $('#pgZoomOut').addEventListener('click', () => camTo(S.k / 1.25, S.tx / 1.25, S.ty / 1.25));
  $('#pgFit').addEventListener('click', () => fit(S.sel ? focusOf(byId[S.sel]) : S.hl));

  /* نطاق التنوير عند تحديد عقدة: جيرانها، أو ما يحدده cfg.focus (مثل الوصول للموردين عبر العلاقات) */
  const focusOf = n => C.focus ? C.focus(n, U) : new Set([n.id, ...n.nb]);
  /* ---------- التنوير والتحديد ---------- */
  function paint() {
    const focus = S.sel ? focusOf(byId[S.sel]) : S.hl;
    svg.classList.toggle('has-focus', !!focus);
    N.forEach(n => {
      const v = visible(n);
      n.el.classList.toggle('is-hidden', !v);
      n.el.classList.toggle('is-on', !!focus && focus.has(n.id));
      n.el.classList.toggle('is-sel', n.id === S.sel);
      n.el.setAttribute('aria-pressed', String(n.id === S.sel));
      n.el.setAttribute('tabindex', v ? '0' : '-1');
    });
    E.forEach(e => {
      const on = !!focus && (S.sel && !C.focus ? (e.a === S.sel || e.b === S.sel) : focus.has(e.a) && focus.has(e.b) && (!S.sel || !C.focusEdge || C.focusEdge(e, byId[S.sel])));
      e.el.classList.toggle('is-on', on); e.lab.style.display = on && S.sel && byId[S.sel].deg <= 10 && edgeVisible(e) ? '' : 'none';
    });
    $$('.pg-law button').forEach(b => b.setAttribute('aria-pressed', String(b.dataset.law === (S.sel && byId[S.sel].t === HUB ? S.sel : S.sel ? '' : 'all'))));
  }
  function select(id, center) {
    S.sel = id && visible(byId[id]) ? id : null;
    if (S.sel) S.hl = null, $('#pgAnswer').hidden = true;
    paint(); panel(); kpis(); widgets(); if (S.view === 'table') table();
    if (S.sel && center) fit(focusOf(byId[S.sel]));
    if (!raf) raf = requestAnimationFrame(loop);
  }

  /* ---------- مؤشرات أعلى اللوحة ---------- */
  const scopeNodes = () => {
    if (S.sel && C.scope) { const s = C.scope(byId[S.sel], U); return N.filter(n => s.has(n.id) && visible(n)); }
    if (S.sel) { const s = new Set([S.sel]); byId[S.sel].nb.forEach(x => { s.add(x); if (byId[S.sel].t === HUB) byId[x].nb.forEach(y => { if (byId[y].t !== HUB) s.add(y); }); }); return N.filter(n => s.has(n.id) && visible(n)); }
    return N.filter(visible);
  };
  function kpis() {
    const L = scopeNodes();
    $('#pgScope').textContent = S.sel ? `ضمن نطاق «${byId[S.sel].n}»` : C.scopeAll;
    $('#pgKpis').innerHTML = JR.renderPage({ id: 'pg' }, { n: 'kpi', w: C.kpis(L, U, S.sel ? byId[S.sel] : null) }, S.sel || 'all');
  }

  /* ---------- اللوحة الجانبية ---------- */
  const sentBar = s => `<div class="pg-sent" role="img" aria-label="${C.sent.title} ${s}"><span class="neg">${s < 0 ? `<i style="width:${Math.min(100, -s)}%"></i>` : ''}</span><span class="pos">${s > 0 ? `<i style="width:${Math.min(100, s)}%"></i>` : ''}</span></div><div class="pg-sent-lab"><span>${C.sent.neg}</span><b class="num">${s > 0 ? '+' : ''}${s}</b><span>${C.sent.pos}</span></div>`;
  function panel() {
    const P = $('#pgPanel');
    if (!S.sel) {
      const V = N.filter(visible), top = [...V].sort((a, b) => b.deg - a.deg).slice(0, 6), comp = V.filter(n => n.t === ISSUE).sort((a, b) => C.issueVal(b, U) - C.issueVal(a, U)).slice(0, 5), mx = comp[0] ? C.issueVal(comp[0], U) : 1;
      P.innerHTML = `<div class="pg-p-head"><span class="pg-p-kicker">${ic('info')}نظرة عامة</span><h2>${V.length} عقدة · ${E.filter(edgeVisible).length} رابطًا</h2><p>اضغط على أي عقدة لترى كل ما يرتبط بها، أو اسحبها لتحريكها.</p></div>
        <h3 class="pg-p-sec">الأكثر ارتباطًا</h3><ul class="pg-links">${top.map(n => `<li><button type="button" data-go="${n.id}">${glyph(n.t)}<span>${esc(n.n)}</span><em class="num">${n.deg}</em></button></li>`).join('')}</ul>
        <h3 class="pg-p-sec">${C.issueTitle}</h3><ul class="pg-bars">${comp.map(n => `<li><button type="button" data-go="${n.id}"><span>${esc(n.n)}</span><span class="trk"><i style="width:${(C.issueVal(n, U) / mx * 100).toFixed(0)}%"></i></span><b class="num">${C.issueFmt(n, U)}</b></button></li>`).join('')}</ul>`;
    } else {
      const n = byId[S.sel], rel = E.filter(e => edgeVisible(e) && (e.a === n.id || e.b === n.id));
      const groups = {}; rel.forEach(e => { const other = e.a === n.id ? e.B : e.A, lab = e.a === n.id ? G.rels[e.r] : ((C.rev || {})[e.r] || G.rels[e.r]); (groups[lab] = groups[lab] || []).push(other); });
      P.innerHTML = `<div class="pg-p-head"><button type="button" class="btn btn-ghost btn-sm pg-back" data-go="">${ic('chev-left')}كل الشبكة</button>
          <span class="pg-p-kicker" style="--c:${T[n.t].col}">${glyph(n.t)}${T[n.t].n} · ${C.since(n, U)}</span><h2>${esc(n.n)}</h2><p>${esc(n.d)}</p></div>
        <div class="pg-stats">${C.stats(n, U).map(([a, b]) => `<div><small>${a}</small><b class="num">${b}</b></div>`).join('')}<div><small>الارتباطات</small><b class="num">${rel.length}</b></div></div>
        ${n.s != null ? `<h3 class="pg-p-sec">${C.sent.title}</h3>${sentBar(n.s)}` : ''}
        <h3 class="pg-p-sec">الارتباطات</h3>
        ${Object.entries(groups).map(([lab, L]) => `<div class="pg-grp"><span>${esc(lab)}</span><ul class="pg-links">${L.map(o => `<li><button type="button" data-go="${o.id}">${glyph(o.t)}<span>${esc(o.n)}</span>${C.linkVal && C.linkVal(o, U) ? `<em class="num">${C.linkVal(o, U)}</em>` : ''}</button></li>`).join('')}</ul></div>`).join('') || '<p class="muted">لا توجد ارتباطات ظاهرة بالفلاتر الحالية.</p>'}
        <div class="pg-p-actions">${window.JEEM_DRILL ? `<button type="button" class="btn btn-sm" data-drill="${n.id}">${ic('layers')}انزل داخلها</button>` : ''}<button type="button" class="btn btn-sm" data-ask-node>${ic('spark')}اسأل جيم عنها</button></div>`;
    }
  }
  root.addEventListener('click', e => {
    const go = e.target.closest('[data-go]'); if (go) { select(go.dataset.go || null, true); if (!go.dataset.go) fit(); return; }
    const dr = e.target.closest('[data-drill]'); if (dr) { drill(dr.dataset.drill); return; }
    if (e.target.closest('[data-ask-node]')) { const n = byId[S.sel]; ask(C.askNode(n)); return; }
  });

  /* ---------- الرسوم أسفل الشبكة (تتغير مع العقدة المحددة) ---------- */
  function widgets() {
    $('#pgGrid').innerHTML = JR.renderPage({ id: 'pg-w' }, { n: 'w', w: C.widgets(S.sel ? byId[S.sel] : null, U) }, S.sel || 'all');
  }

  /* ---------- عرض كجدول (بديل الرسم لقارئات الشاشة والطباعة) ---------- */
  function table() {
    const V = N.filter(visible).sort((a, b) => TYPES.indexOf(a.t) - TYPES.indexOf(b.t) || b.deg - a.deg);
    $('#pgTable').innerHTML = `<table class="table"><caption class="sr-only">عقد الشبكة الظاهرة</caption><thead><tr><th>النوع</th><th>الاسم</th><th>الارتباطات</th>${C.cols.map(([h]) => `<th>${h}</th>`).join('')}</tr></thead><tbody>${V.map(n => `<tr${n.id === S.sel ? ' class="is-sel"' : ''}><td>${glyph(n.t)} ${T[n.t].n}</td><td><button type="button" class="link" data-go="${n.id}">${esc(n.n)}</button></td><td><span class="num">${n.deg}</span></td>${C.cols.map(([, f]) => `<td><span class="num">${f(n, U)}</span></td>`).join('')}</tr>`).join('')}</tbody></table>`;
  }
  $$('.pg-view button').forEach(b => b.addEventListener('click', () => {
    S.view = b.dataset.view; $$('.pg-view button').forEach(x => x.setAttribute('aria-pressed', String(x === b)));
    $('#pgTable').hidden = S.view !== 'table'; svg.style.visibility = S.view === 'table' ? 'hidden' : ''; $('.pg-zoom').hidden = S.view === 'table';
    if (S.view === 'table') table();
  }));

  /* ---------- التصفية حسب النوع ---------- */
  $('#pgTypes').innerHTML = TYPES.map(t => `<button type="button" class="pg-type" data-t="${t}" aria-pressed="true">${glyph(t)}<span>${T[t].pl}</span><em class="num">${N.filter(n => n.t === t).length}</em></button>`).join('');
  $$('.pg-type').forEach(b => b.addEventListener('click', () => {
    const t = b.dataset.t; if (S.on.has(t)) { if (S.on.size === 1) return; S.on.delete(t); } else S.on.add(t);
    b.setAttribute('aria-pressed', String(S.on.has(t))); refresh();
  }));

  /* ---------- الشريط الزمني ---------- */
  const yr = $('#pgYear'), yrOut = $('#pgYearOut'), play = $('#pgPlay');
  const TM = C.time;
  yr.min = TM.min; yr.max = TM.max; $('.pg-ticks').innerHTML = TM.ticks.map(t => `<i>${t}</i>`).join('');
  const setYear = y => { S.year = +y; yr.value = y; yrOut.textContent = TM.out(+y); refresh(); };
  yr.addEventListener('input', () => { stopPlay(); setYear(yr.value); });
  let timer = null;
  const stopPlay = () => { clearInterval(timer); timer = null; play.innerHTML = ic('play'); play.setAttribute('aria-label', TM.play); };
  play.addEventListener('click', () => {
    if (timer) return stopPlay();
    play.innerHTML = ic('pause'); play.setAttribute('aria-label', 'إيقاف التشغيل');
    let y = S.year >= TM.max ? TM.min : S.year; setYear(y); select(null); fit();
    timer = setInterval(() => { y++; if (y > TM.max) return stopPlay(); setYear(y); }, 1100);
  });

  function refresh() {
    if (S.sel && !visible(byId[S.sel])) S.sel = null;
    paint(); panel(); kpis(); widgets(); if (S.view === 'table') table(); kick(.45);
  }

  /* ---------- أزرار المحاور الرئيسية (أنظمة / عملاء…) ---------- */
  $('.pg-law').innerHTML = `<button type="button" data-law="all" aria-pressed="true">${C.hubsAll}</button>` + C.hubs.map(h => `<button type="button" data-law="${h.id}" aria-pressed="false">${h.label}</button>`).join('');
  $$('.pg-law button').forEach(b => b.addEventListener('click', () => {
    clearAsk();
    if (b.dataset.law === 'all') { select(null); fit(); }
    else { select(b.dataset.law); fit(focusOf(byId[b.dataset.law])); }
  }));

  /* ---------- البحث ---------- */
  const norm = s => s.replace(/[أإآ]/g, 'ا').replace(/ة/g, 'ه').replace(/ى/g, 'ي').replace(/ـ/g, '').trim();
  const search = $('#pgSearch'), sug = $('#pgSug');
  search.addEventListener('input', () => {
    const q = norm(search.value); if (!q) { sug.hidden = true; return; }
    const L = N.filter(n => norm(n.n).includes(q)).slice(0, 7);
    sug.innerHTML = L.length ? L.map(n => `<li><button type="button" data-go="${n.id}">${glyph(n.t)}<span>${esc(n.n)}</span><small>${T[n.t].n}</small></button></li>`).join('') : '<li class="muted">لا نتائج</li>';
    sug.hidden = false;
  });
  sug.addEventListener('click', e => { const b = e.target.closest('[data-go]'); if (!b) return; const n = byId[b.dataset.go]; S.on.add(n.t); if (n.y > S.year) setYear(TM.max); $$('.pg-type').forEach(x => x.setAttribute('aria-pressed', String(S.on.has(x.dataset.t)))); sug.hidden = true; search.value = ''; });
  document.addEventListener('click', e => { if (!e.target.closest('.pg-search')) sug.hidden = true; });

  /* ---------- اسأل جيم ---------- */
  const askIn = $('#pgAskIn'), ans = $('#pgAnswer');
  $('#pgAskChips').innerHTML = G.asks.map((a, i) => `<button type="button" class="chip" data-ask="${i}">${esc(a.q)}</button>`).join('');
  $$('[data-ask]').forEach(b => b.addEventListener('click', () => ask(G.asks[+b.dataset.ask].q)));
  $('#pgAsk').addEventListener('submit', e => { e.preventDefault(); if (askIn.value.trim()) ask(askIn.value.trim()); });
  function clearAsk() { S.hl = null; ans.hidden = true; paint(); }
  let typing = null;
  function ask(q) {
    askIn.value = q;
    let hit = G.asks.find(a => a.q === q);
    if (!hit) {
      const nq = norm(q), kw = C.kw;
      const byName = N.filter(n => nq.includes(norm(n.n)) || norm(n.n).split(' ').filter(w => w.length > 3).some(w => nq.includes(w)));
      const base = S.sel ? [byId[S.sel], ...[...byId[S.sel].nb].map(x => byId[x])] : byName.length ? byName : N;
      const tk = kw.find(([k]) => nq.includes(norm(k)));
      let ids = new Set(base.filter(n => !tk || n.t === tk[1] || n.t === HUB).map(n => n.id));
      if (byName.length && !S.sel) byName.forEach(n => n.nb.forEach(x => ids.add(x)));
      hit = { ids: [...ids], a: C.answer(N.filter(n => ids.has(n.id)), U) };
    }
    S.sel = null; S.hl = new Set(hit.ids);
    hit.ids.forEach(id => S.on.add(byId[id].t)); $$('.pg-type').forEach(x => x.setAttribute('aria-pressed', String(S.on.has(x.dataset.t))));
    if (S.year < TM.max) setYear(TM.max); else refresh();
    fit(S.hl);
    ans.hidden = false; const txt = $('.pg-ans-txt', ans); clearInterval(typing);
    if (reduceMotion) txt.textContent = hit.a; else { let i = 0; txt.textContent = ''; ans.classList.add('typing'); typing = setInterval(() => { i += 3; txt.textContent = hit.a.slice(0, i); if (i >= hit.a.length) { clearInterval(typing); ans.classList.remove('typing'); } }, 18); }
  }
  $('#pgAnsClear').addEventListener('click', () => { clearAsk(); fit(); });

  /* ---------- البدء ---------- */
  addEventListener('resize', () => applyCam());
  setYear(TM.max); paint(); panel(); kpis(); widgets(); draw();
  requestAnimationFrame(() => { fit(); kick(.3); });
  /* واجهة صغيرة لطبقات إضافية (مثل وضع النزول) */
  window.JEEM_NET = { root, S, N, E, T, C, G, U, byId, visible, edgeVisible, select, fit, ask, glyph, esc, ic, nf, reduceMotion,
    toScreen: n => { const [w, h] = size(); return [w / 2 + S.tx + n.x * S.k, h / 2 + S.ty + n.yy * S.k]; }, busy: () => !!camAnim };
  root.dispatchEvent(new CustomEvent('pg:ready'));
  }
})();
