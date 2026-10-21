/* ==========================================================
   جيم — وضع النزول داخل الشبكة (Drill-down) لكل اللوحات الشبكية
   يُحمَّل قبل network-graph.js ويعمل على أي بيانات JEEM_NETWORK.

   المستويات:
     1) الشبكة الكاملة (المحرك الأصلي).
     2) عقدة واحدة: مسار السياق ← العقدة ← بطاقات مجمّعة حسب النوع ← تفرّع البطاقة المختارة لعناصرها.
     3) عنصر واحد: تفاصيله في اللوحة الجانبية + عدسة على موقعه في الخريطة المصغّرة،
        ويمكن النزول داخله (يصبح المركز الجديد) أو إظهاره في الشبكة الكاملة بعدسة تصويب.
   الحركة وظيفية: الخطوط تُرسم بالاتجاه، ونقاط ضوء تسير عليها، والبطاقة المختارة تبقى مميزة،
   والعدسة تربط التفاصيل بمكانها. تُلغى الحركة مع prefers-reduced-motion.
   ========================================================== */
(function () {
  window.JEEM_DRILL = true;
  const root = document.getElementById('pgRoot'); if (!root) return;
  root.addEventListener('pg:ready', init, { once: true });

  function init() {
    const A = window.JEEM_NET; if (!A) return;
    const { T, C, G, U, byId, esc, ic, nf } = A, RM = A.reduceMotion, NS = 'http://www.w3.org/2000/svg';
    const stage = root.querySelector('.pg-stage'); if (!stage) return;
    const relOf = (a, b) => { const e = A.E.find(e => (e.a === a && e.b === b) || (e.a === b && e.b === a)); if (!e) return ''; return e.a === a ? G.rels[e.r] : ((C.rev || {})[e.r] || G.rels[e.r]); };
    const nbs = n => [...n.nb].map(id => byId[id]).filter(A.visible);
    const metric = n => C.issueVal ? C.issueVal(n, U) : n.deg;

    /* ---------- الهيكل ---------- */
    const D = document.createElement('div'); D.className = 'dr'; D.hidden = true; D.setAttribute('role', 'region'); D.setAttribute('aria-label', 'وضع النزول داخل الشبكة');
    D.innerHTML = `<div class="dr-top"><nav class="dr-crumbs" aria-label="مسار النزول"></nav><span class="dr-sp"></span>
        <button type="button" class="btn btn-sm" data-dr-show>${ic('target')}أظهرها في الشبكة</button>
        <button type="button" class="btn btn-sm btn-primary" data-dr-close>${ic('x')}رجوع للشبكة</button></div>
      <div class="dr-body"><div class="dr-chain"></div><div class="dr-core"></div><div class="dr-cats" role="group" aria-label="المرتبطات حسب النوع"></div><div class="dr-items" role="list"></div></div>
      <svg class="dr-links" aria-hidden="true"></svg>
      <button type="button" class="dr-mini" data-dr-show aria-label="موقعك في الشبكة الكاملة — اضغط لإظهاره"><svg viewBox="0 0 160 104"></svg><small>موقعك في الشبكة</small></button>`;
    stage.append(D);
    const lens = document.createElement('div'); lens.className = 'dr-lens'; lens.hidden = true; lens.innerHTML = '<i></i><b></b>'; stage.append(lens);
    const $ = s => D.querySelector(s);
    const R = { cur: null, chain: [], cat: null, item: null };

    /* السياق: أقصر مسار من المحور الأكبر إلى العقدة */
    function context(id) {
      const hubs = A.N.filter(n => n.t === C.hub && A.visible(n)).sort((a, b) => b.deg - a.deg);
      const start = hubs.find(h => h.id !== id); if (!start) return [];
      const prev = { [start.id]: null }, q = [start.id];
      while (q.length) { const x = q.shift(); if (x === id) break; byId[x].nb.forEach(y => { if (!(y in prev) && A.visible(byId[y])) { prev[y] = x; q.push(y); } }); }
      if (!(id in prev)) return [];
      const path = []; let x = prev[id]; while (x) { path.unshift(x); x = prev[x]; }
      return path.slice(-3);
    }

    /* ---------- الرسم ---------- */
    const orbRing = (r, seed) => { let s = seed; const rnd = () => (s = (s * 16807) % 2147483647) / 2147483647; return Array.from({ length: 96 }, (_, i) => { const a = i / 96 * Math.PI * 2, rr = r + (rnd() - .5) * 5; return `${(Math.cos(a) * rr).toFixed(1)},${(Math.sin(a) * rr).toFixed(1)}`; }).join(' '); };
    function render(intro) {
      const n = byId[R.cur], col = T[n.t].col, groups = {};
      nbs(n).forEach(o => (groups[o.t] = groups[o.t] || []).push(o));
      const cats = Object.entries(groups).sort((a, b) => b[1].length - a[1].length);
      if (!R.cat || !groups[R.cat]) R.cat = cats[0] && cats[0][0];
      /* المسار */
      $('.dr-crumbs').innerHTML = `<button type="button" data-dr-close>${ic('share')}كل الشبكة</button>${R.chain.concat([R.cur]).map((id, i, L) => `<span class="sep" aria-hidden="true">›</span>${i === L.length - 1 ? `<b aria-current="page">${esc(byId[id].n)}</b>` : `<button type="button" data-dr-go="${i}">${esc(byId[id].n)}</button>`}`).join('')}`;
      $('.dr-chain').innerHTML = R.chain.map((id, i) => { const c = byId[id]; return `<button type="button" class="dr-anc" data-dr-go="${i}" style="--c:${T[c.t].col};--i:${i}">${A.glyph(c.t, 16)}<span>${esc(c.n)}</span><small>${T[c.t].n}</small></button>`; }).join('');
      /* المركز */
      const st = C.stats ? C.stats(n, U) : [];
      $('.dr-core').innerHTML = `<div class="dr-orb${intro ? ' in' : ''}" style="--c:${col}"><svg viewBox="-70 -70 140 140" aria-hidden="true"><polygon class="wv" points="${orbRing(52, n.id.length * 97 + 13)}"/><circle class="rg" r="44"/><circle class="ball" r="30"/></svg></div>
        <span class="dr-kind" style="--c:${col}">${A.glyph(n.t)}${T[n.t].n}${C.since ? ` · ${esc(C.since(n, U))}` : ''}</span><h3>${esc(n.n)}</h3>
        <div class="dr-chips">${st.map(([a, b]) => `<span><small>${a}</small><b class="num">${b}</b></span>`).join('')}<span><small>الارتباطات</small><b class="num">${nbs(n).length}</b></span></div>
        ${n.s != null ? `<div class="dr-sent" title="${C.sent ? C.sent.title : ''}"><span class="neg">${n.s < 0 ? `<i style="width:${Math.min(100, -n.s)}%"></i>` : ''}</span><span class="pos">${n.s > 0 ? `<i style="width:${Math.min(100, n.s)}%"></i>` : ''}</span></div>` : ''}
        <button type="button" class="btn btn-sm" data-dr-ask>${ic('spark')}اسأل جيم عنها</button>`;
      /* البطاقات المجمّعة */
      $('.dr-cats').innerHTML = cats.map(([t, L], i) => { const top = [...L].sort((a, b) => metric(b) - metric(a))[0]; return `<button type="button" class="dr-cat${t === R.cat ? ' on' : ''}" data-dr-cat="${t}" aria-pressed="${t === R.cat}" style="--c:${T[t].col};--i:${i}">
        <span class="h">${A.glyph(t, 16)}<b class="num">${L.length}</b></span><span class="t">${T[t].pl}</span><small>الأبرز: ${esc(top.n)}</small></button>`; }).join('') || '<p class="muted">لا مرتبطات ظاهرة لهذه العقدة.</p>';
      items(true);
      mini();
      requestAnimationFrame(() => links(intro));
    }
    function items(intro) {
      const n = byId[R.cur], L = nbs(n).filter(o => o.t === R.cat).sort((a, b) => metric(b) - metric(a));
      $$('.dr-cat').forEach(b => { const on = b.dataset.drCat === R.cat; b.classList.toggle('on', on); b.setAttribute('aria-pressed', String(on)); });
      $('.dr-items').innerHTML = L.length ? `<span class="dr-ih">${T[R.cat] ? T[R.cat].pl : ''} · <span class="num">${L.length}</span></span>` + L.map((o, i) => `<div class="dr-it${o.id === R.item ? ' on' : ''}${intro ? ' in' : ''}" role="listitem" style="--c:${T[o.t].col};--i:${Math.min(i, 14)}" data-dr-id="${o.id}">
          <button type="button" class="m" data-dr-item="${o.id}"><span class="n">${A.glyph(o.t)}<b>${esc(o.n)}</b></span><small>${esc(relOf(R.cur, o.id))}${C.tip ? ' · ' + esc(C.tip(o, U)) : ''}</small>
            ${o.cm != null ? `<span class="bar" title="الامتثال ${o.cm}%"><i style="width:${o.cm}%"></i></span>` : ''}</button>
          ${o.nb.size > 1 ? `<button type="button" class="dn" data-dr-down="${o.id}" aria-label="انزل داخل ${esc(o.n)}" title="انزل داخلها">${ic('chev-left')}</button>` : ''}</div>`).join('') : '';
      requestAnimationFrame(() => links(intro, true));
    }
    const $$ = s => [...D.querySelectorAll(s)];

    /* الخطوط والنقاط المضيئة: تُحسب من مواقع العناصر الفعلية */
    function links(intro, onlyItems) {
      const svg = $('.dr-links'), b = D.getBoundingClientRect(); svg.setAttribute('viewBox', `0 0 ${b.width} ${b.height}`);
      const P = (el, side) => { const r = el.getBoundingClientRect(); return [side === 'r' ? r.right - b.left : r.left - b.left, r.top - b.top + r.height / 2, r.top - b.top, r.bottom - b.top]; };
      const curve = (a, c) => { const mx = (a[0] + c[0]) / 2; return `M${a[0].toFixed(1)},${a[1].toFixed(1)} C${mx.toFixed(1)},${a[1].toFixed(1)} ${mx.toFixed(1)},${c[1].toFixed(1)} ${c[0].toFixed(1)},${c[1].toFixed(1)}`; };
      const orb = $('.dr-orb'), out = [];
      const ib = $('.dr-items').getBoundingClientRect(), inView = r => r[1] > ib.top - b.top + 4 && r[1] < ib.bottom - b.top - 4;
      if (orb) {
        $$('.dr-anc').forEach((el, i, L) => { const nx = i < L.length - 1 ? L[i + 1] : orb; out.push(['anc', curve(P(el, 'l'), P(nx, 'r')), i]); });
        $$('.dr-cat').forEach((el, i) => out.push([el.classList.contains('on') ? 'cat on' : 'cat', curve(P(orb, 'l'), P(el, 'r')), i]));
        const sel = $('.dr-cat.on');
        if (sel) $$('.dr-it').forEach((el, i) => { const p = P(el, 'r'); if (inView(p)) out.push([el.classList.contains('on') ? 'it on' : 'it', curve(P(sel, 'l'), p), i]); });
      }
      const anim = intro && !RM;
      svg.innerHTML = out.map(([k, d, i]) => `<path class="${k}${anim && (!onlyItems || k.startsWith('it')) ? ' in' : ''}" d="${d}" pathLength="1" style="--i:${i}"/>`).join('')
        + (RM ? '' : out.filter(o => o[0] !== 'anc').slice(0, 40).map(([k, d], j) => `<circle class="pt ${k.split(' ')[0]}${k.includes('on') ? ' on' : ''}" r="${k.includes('on') ? 3 : 2}"><animateMotion dur="${(2.2 + (j % 5) * .35).toFixed(2)}s" begin="${anim ? (0.9 + (j % 7) * .21).toFixed(2) : -(j * .37 % 2).toFixed(2)}s" repeatCount="indefinite" path="${d}"/></circle>`).join(''));
    }

    /* الخريطة المصغّرة: موقعك في الشبكة الكاملة */
    function mini() {
      const V = A.N.filter(A.visible); if (!V.length) return;
      const xs = V.map(n => n.x), ys = V.map(n => n.yy), x0 = Math.min(...xs), x1 = Math.max(...xs), y0 = Math.min(...ys), y1 = Math.max(...ys);
      const k = Math.min(148 / (x1 - x0 || 1), 92 / (y1 - y0 || 1)), px = n => 6 + (n.x - x0) * k + (148 - (x1 - x0) * k) / 2, py = n => 6 + (n.yy - y0) * k + (92 - (y1 - y0) * k) / 2;
      const path = new Set(R.chain.concat([R.cur])), tgt = byId[R.item || R.cur];
      $('.dr-mini svg').innerHTML = A.E.filter(e => A.edgeVisible(e)).map(e => `<line x1="${px(e.A).toFixed(1)}" y1="${py(e.A).toFixed(1)}" x2="${px(e.B).toFixed(1)}" y2="${py(e.B).toFixed(1)}"${path.has(e.a) && path.has(e.b) ? ' class="on"' : ''}/>`).join('')
        + V.map(n => `<circle cx="${px(n).toFixed(1)}" cy="${py(n).toFixed(1)}" r="${path.has(n.id) ? 2.6 : 1.4}" fill="${path.has(n.id) ? T[n.t].col : '#B7C1CB'}"/>`).join('')
        + `<g class="ret" style="transform:translate(${px(tgt).toFixed(1)}px,${py(tgt).toFixed(1)}px)"><circle r="8"/><path d="M-13,0 H-5 M5,0 H13 M0,-13 V-5 M0,5 V13"/></g>`;
    }

    /* ---------- التنقل ---------- */
    function open(id, chain) {
      const n = byId[id]; if (!n) return;
      R.cur = id; R.chain = chain || context(id); R.cat = null; R.item = null;
      const first = D.hidden; D.hidden = false; stage.classList.add('is-drill'); lens.hidden = true;
      A.select(id, false);
      render(true);
      if (first) { D.classList.remove('leave'); D.classList.add('enter'); setTimeout(() => D.classList.remove('enter'), 600); $('[data-dr-close]').focus({ preventScroll: true }); const r = stage.getBoundingClientRect(); if (r.top < 0 || r.bottom > innerHeight) stage.scrollIntoView({ block: 'start', behavior: RM ? 'auto' : 'smooth' }); }
    }
    function close(show) {
      const id = R.item || R.cur;
      D.classList.add('leave'); stage.classList.remove('is-drill');
      setTimeout(() => { D.hidden = true; D.classList.remove('leave'); }, RM ? 0 : 280);
      if (show && id) { A.select(id, true); target(id); }
    }
    /* عدسة التصويب على العقدة في الشبكة الكاملة بعد انتقال الكاميرا */
    function target(id) {
      const n = byId[id], t0 = performance.now(); lens.hidden = false; lens.classList.remove('go'); void lens.offsetWidth; lens.classList.add('go');
      const follow = () => { const [x, y] = A.toScreen(n); lens.style.transform = `translate(${x}px,${y}px)`; if (performance.now() - t0 < 2600) requestAnimationFrame(follow); else lens.hidden = true; };
      follow();
    }
    function pickItem(id) {
      R.item = R.item === id ? null : id;
      $$('.dr-it').forEach(el => el.classList.toggle('on', el.dataset.drId === R.item));
      A.select(R.item || R.cur, false); mini(); links(false);
    }

    root.addEventListener('pg:drill', e => open(e.detail.id));
    D.addEventListener('click', e => {
      const t = e.target;
      if (t.closest('[data-dr-close]')) return close(false);
      if (t.closest('[data-dr-show]')) return close(true);
      const g = t.closest('[data-dr-go]'); if (g) { const i = +g.dataset.drGo; return open(R.chain[i], R.chain.slice(0, i)); }
      const c = t.closest('[data-dr-cat]'); if (c) { R.cat = c.dataset.drCat; R.item = null; return items(true); }
      const d = t.closest('[data-dr-down]'); if (d) return open(d.dataset.drDown, R.chain.concat([R.cur]));
      const it = t.closest('[data-dr-item]'); if (it) return pickItem(it.dataset.drItem);
      if (t.closest('[data-dr-ask]')) { const n = byId[R.item || R.cur]; close(false); if (C.askNode) A.ask(C.askNode(n)); }
    });
    D.addEventListener('dblclick', e => { const it = e.target.closest('[data-dr-item]'); if (it && byId[it.dataset.drItem].nb.size > 1) open(it.dataset.drItem, R.chain.concat([R.cur])); });
    $('.dr-items').addEventListener('scroll', () => links(false), { passive: true });
    addEventListener('keydown', e => { if (e.key === 'Escape' && !D.hidden) close(false); });
    new ResizeObserver(() => { if (!D.hidden) links(false); }).observe(D);
  }
})();
