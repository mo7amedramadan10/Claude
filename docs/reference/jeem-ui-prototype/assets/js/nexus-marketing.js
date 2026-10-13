/* ==========================================================
   جيم — لوحة التسويق «نكسس» (لوحة تفاعلية متحركة)
   الحركات: ظهور البطاقات بالترتيب، أعمدة مكدّسة تُبنى طبقة طبقة مع شرائط تربط الأشهر،
   تمييز عمود واحد، اكتساح نصف الحلقة، صفوف وأشرطة نسب تمتلئ،
   وانتقال سلس من القيم القديمة للجديدة عند تبديل المقياس.
   ========================================================== */
(function () {
  const root = document.getElementById('nxRoot'); if (!root) return;
  const $ = (s, el = root) => el.querySelector(s), $$ = (s, el = root) => [...el.querySelectorAll(s)];
  const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
  const nf = (v, d = 0) => Number(v).toLocaleString('en-US', { maximumFractionDigits: d, minimumFractionDigits: d });
  const RM = matchMedia('(prefers-reduced-motion: reduce)').matches;
  const ease = p => 1 - Math.pow(1 - p, 3);
  const count = (el, to, fmt, dur = 1000) => { if (!el) return; if (RM) { el.textContent = fmt(to); return; } const from = +el.dataset.v || 0, t0 = performance.now(); el.dataset.v = to; const st = t => { const p = Math.min(1, (t - t0) / dur); el.textContent = fmt(from + (to - from) * ease(p)); if (p < 1) requestAnimationFrame(st); }; requestAnimationFrame(st); };
  const toast = msg => { let t = document.querySelector('.toast'); if (!t) { t = document.createElement('div'); t.className = 'toast'; t.setAttribute('role', 'status'); document.body.append(t); } t.innerHTML = `${ic('check')}<span>${msg}</span>`; requestAnimationFrame(() => t.classList.add('is-on')); clearTimeout(t._h); t._h = setTimeout(() => t.classList.remove('is-on'), 2400); };

  /* ---------- البيانات ---------- */
  const CO = [['السعودية', '#4D4286'], ['الإمارات', '#7B6FC9'], ['مصر', '#127DBD'], ['الكويت', '#00BAB3'], ['أخرى', '#8FD9D4']];
  const MONTHS = ['أبريل', 'مايو', 'يونيو', 'يوليو', 'أغسطس', 'سبتمبر'];
  const DATA = {
    rev: [[700, 600, 500, 350, 180], [820, 690, 560, 390, 200], [610, 540, 450, 300, 170], [920, 780, 640, 420, 210], [520, 430, 380, 280, 150], [1180, 1010, 880, 620, 320]],
    ord: [[240, 190, 260, 120, 70], [280, 210, 300, 130, 80], [220, 180, 240, 110, 60], [310, 260, 330, 150, 90], [190, 150, 210, 100, 50], [400, 320, 420, 190, 110]]
  };
  const MET = { rev: { n: 'الإيرادات', u: 'ألف ر.س', f: v => nf(v / 1000, 2), uu: 'مليون ر.س' }, ord: { n: 'الطلبات', u: 'طلب', f: v => nf(v), uu: 'طلب' } };
  const S = { met: 'rev', span: 3, hl: null, sel: null, day: null, week: 'this', sort: ['profit', -1], more: false, sl: new Set() };
  const cols = () => DATA[S.met].slice(-S.span);
  const tot = a => a.reduce((x, y) => x + y, 0);

  /* ---------- 1) المؤشرات ---------- */
  const KPIS = [
    { ic: 'chart', n: 'مشاهدات الصفحات', v: 12450, f: v => nf(v), d: '15.8%', good: true, up: true },
    { ic: 'wallet', n: 'إجمالي الإيرادات', v: 363.95, f: v => nf(v, 2), u: 'ألف ر.س', d: '34.0%', good: false, up: false },
    { ic: 'percent', n: 'معدل الارتداد', v: 46.5, f: v => nf(v, 1) + '%', d: '4.2%', good: true, up: false, note: 'الأقل أفضل' }
  ];
  $('#nxKpis').innerHTML = KPIS.map((k, i) => `<div class="nx-kpi" style="--i:${i}"><span class="lb">${ic(k.ic)}${k.n}<button type="button" class="nx-info" data-info="${i}" aria-label="معلومات عن ${k.n}">${ic('info')}</button></span>
    <span class="v"><b class="num" data-k="${i}">0</b>${k.u ? `<small>${k.u}</small>` : ''}<span class="dl ${k.good ? 'ok' : 'bad'}">${ic(k.up ? 'up' : 'down')}<span class="num">${k.d}</span></span></span>${k.note ? `<small class="nt">${k.note}</small>` : ''}</div>`).join('');
  KPIS.forEach((k, i) => count($(`[data-k="${i}"]`), k.v, k.f, 1100));

  /* ---------- 2) نظرة على المبيعات: أعمدة مكدّسة + شرائط ---------- */
  const W = 720, H = 300, PAD = 16, BASE = H - 30;
  let shown = cols().map(c => c.slice());
  function geo(V) {
    const n = V.length, slot = (W - PAD * 2) / n, cw = Math.min(n === 3 ? 96 : 64, slot * .5), mx = Math.max(...V.map(tot)) * 1.15;
    const cx = i => W - PAD - slot * (i + .5), sy = v => v / mx * (BASE - 24);
    return V.map((c, i) => { let y = BASE; return { x: cx(i), cw, t: tot(c), seg: c.map(v => { const h = sy(v), s = { y0: y - h, y1: y, h }; y -= h; return s; }) }; });
  }
  const ST = G => Math.round(1140 / G.length);
  function drawSales(V, intro) {
    const G = geo(V), mlab = MONTHS.slice(-S.span), m = MET[S.met];
    let rib = '';
    for (let i = 0; i < G.length - 1; i++) { const a = G[i], b = G[i + 1], xa = a.x - a.cw / 2, xb = b.x + b.cw / 2, mid = (xa + xb) / 2;
      CO.forEach(([, c], l) => { const p = a.seg[l], q = b.seg[l]; rib += `<path class="rb${intro ? ' in' : ''}" data-l="${l}" fill="${c}" style="--d:${(i + 1) * ST(G) + 260}ms" d="M${xa},${p.y0 + 1.5} C${mid},${p.y0 + 1.5} ${mid},${q.y0 + 1.5} ${xb},${q.y0 + 1.5} L${xb},${q.y1 - 1.5} C${mid},${q.y1 - 1.5} ${mid},${p.y1 - 1.5} ${xa},${p.y1 - 1.5}Z"/>`; }); }
    const bars = G.map((g, i) => `<g class="col${S.sel === i ? ' sel' : ''}" data-c="${i}" tabindex="0" role="button" aria-label="${mlab[i]}: ${m.f(g.t)} ${m.uu}">
      <rect class="hit" x="${g.x - g.cw / 2 - 10}" y="0" width="${g.cw + 20}" height="${BASE}"/>
      ${g.seg.map((s, l) => `<rect class="sg${intro ? ' in' : ''}" data-l="${l}" x="${g.x - g.cw / 2}" y="${(s.y0 + 1.5).toFixed(1)}" width="${g.cw}" height="${Math.max(0, s.h - 3).toFixed(1)}" rx="6" fill="${CO[l][1]}" style="--d:${i * ST(G) + l * 80}ms"/>`).join('')}
      <text class="tl${intro ? ' in' : ''}" x="${g.x}" y="${(g.seg[4].y0 - 10).toFixed(1)}" style="--d:${i * ST(G) + 420}ms">${m.f(g.t)}</text>
      <text class="ml" x="${g.x}" y="${H - 8}">${mlab[i]}</text></g>`).join('');
    $('#nxSales').innerHTML = `<svg viewBox="0 0 ${W} ${H}" class="nx-stack${S.hl != null ? ' has-hl' : ''}"><line class="bl" x1="${PAD}" x2="${W - PAD}" y1="${BASE}" y2="${BASE}"/>${rib}${bars}</svg><div class="nx-tip" hidden></div>`;
    applyHl();
  }
  function salesHead() {
    const V = cols(), m = MET[S.met], t = tot(V.map(tot)), pv = DATA[S.met].slice(0, -S.span).slice(-S.span), prev = pv.length === S.span ? tot(pv.map(tot)) : 0, g = prev ? (t - prev) / prev * 100 : 0;
    count($('#nxSalesV'), t, v => m.f(v), 900); $('#nxSalesU').textContent = m.uu;
    $('#nxSalesD').innerHTML = !prev ? '<small>كل الأشهر المتاحة</small>' : `<span class="dl ${g >= 0 ? 'ok' : 'bad'}">${ic(g >= 0 ? 'up' : 'down')}<span class="num">${nf(Math.abs(g), 1)}%</span></span><small>عن الفترة السابقة</small>`;
  }
  function tweenSales(intro) {
    const to = cols();
    if (intro || RM || to.length !== shown.length) { shown = to.map(c => c.slice()); drawSales(shown, intro && !RM); return; }
    const from = shown.map(c => c.slice()), t0 = performance.now();
    const st = t => { const p = ease(Math.min(1, (t - t0) / 650)); shown = from.map((c, i) => c.map((v, l) => v + (to[i][l] - v) * p)); drawSales(shown, false); if (p < 1) requestAnimationFrame(st); };
    requestAnimationFrame(st);
  }
  function applyHl() {
    const svg = $('#nxSales svg'); if (!svg) return; svg.classList.toggle('has-hl', S.hl != null);
    $$('[data-l]', svg).forEach(e => e.classList.toggle('on', +e.dataset.l === S.hl));
    $$('#nxLeg button').forEach(b => b.classList.toggle('on', +b.dataset.l === S.hl));
  }
  $('#nxLeg').innerHTML = CO.map(([n, c], l) => `<button type="button" data-l="${l}"><i style="background:${c}"></i>${n}</button>`).join('');
  function salesTip(c, l) {
    const tip = $('#nxSales .nx-tip'); if (c == null) { tip.hidden = true; return; }
    const V = cols()[c], m = MET[S.met], G = geo(shown)[c], svg = $('#nxSales svg').getBoundingClientRect(), k = svg.width / W;
    tip.hidden = false; tip.innerHTML = `<b>${MONTHS.slice(-S.span)[c]}</b>${CO.map(([n, col], i) => `<span class="${l === i ? 'on' : ''}"><i style="background:${col}"></i>${n}<b class="num">${m.f(V[i])}</b></span>`).reverse().join('')}<span class="t">الإجمالي<b class="num">${m.f(tot(V))}</b></span>`;
    const x = G.x * k; tip.style.left = Math.max(80, Math.min(svg.width - 80, x)) + 'px'; tip.style.top = Math.max(0, (G.seg[4].y0 * k) - 20) + 'px';
  }
  const sv = $('#nxSales');
  sv.addEventListener('pointerover', e => { const s = e.target.closest('[data-l]'), c = e.target.closest('[data-c]'); if (s && s.closest('svg')) { S.hl = +s.dataset.l; applyHl(); } if (c) salesTip(+c.dataset.c, S.hl); });
  sv.addEventListener('pointerleave', () => { S.hl = null; applyHl(); salesTip(null); });
  sv.addEventListener('focusin', e => { const c = e.target.closest('[data-c]'); if (c) salesTip(+c.dataset.c, null); });
  sv.addEventListener('focusout', () => salesTip(null));
  sv.addEventListener('keydown', e => { const c = e.target.closest('[data-c]'); if (c && (e.key === 'Enter' || e.key === ' ')) { e.preventDefault(); selMonth(+c.dataset.c); } });
  function selMonth(i) { S.sel = S.sel === i ? null : i; $$('#nxSales .col').forEach(g => g.classList.toggle('sel', +g.dataset.c === S.sel)); dist(); }

  /* ---------- 3) المشتركون: عمود مميّز ---------- */
  const DAYS = ['السبت', 'الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة'];
  const SUB = { this: [1820, 2410, 2050, 3874, 1560, 2950, 3300], last: [1650, 2200, 2380, 2900, 1700, 2600, 3550] };
  function subs() {
    const A = SUB[S.week], mx = Math.max(...A) * 1.12, top = A.indexOf(Math.max(...A)); if (S.day == null) S.day = top;
    const t = { this: 24473, last: 23724 }[S.week], d = ((24473 - 23724) / 23724 * 100);
    count($('#nxSubV'), t, v => nf(v), 900);
    $('#nxSubD').innerHTML = S.week === 'this' ? `<span class="dl ok">${ic('up')}<span class="num">${nf(d, 1)}%</span></span><small>+<span class="num">749</span> مشتركًا هذا الأسبوع</small>` : `<small>الأسبوع الماضي</small>`;
    const box = $('#nxSub');
    if (!box.children.length) box.innerHTML = DAYS.map((n, i) => `<button type="button" class="nx-bar" data-day="${i}" style="--i:${i}"><span class="v num"></span><span class="b"><i></i></span><small>${n}</small></button>`).join('');
    $$('.nx-bar', box).forEach((b, i) => { b.style.setProperty('--h', (A[i] / mx * 100).toFixed(1) + '%'); b.classList.toggle('on', i === S.day); b.setAttribute('aria-pressed', String(i === S.day)); b.setAttribute('aria-label', `${DAYS[i]}: ${nf(A[i])} مشترك`); $('.v', b).textContent = nf(A[i]); });
  }

  /* ---------- 4) توزيع المبيعات (نصف حلقة) ---------- */
  const CH = [['الموقع', .4513, '#4D4286'], ['تطبيق الجوال', .2909, '#00BAB3'], ['أخرى', .2578, '#D9DEE8']];
  let distHl = null;
  function dist() {
    const base = 829.84, V = cols(), share = S.sel == null ? 1 : tot(V[S.sel]) / tot(V.map(tot));
    const m = S.sel == null ? 'كل الفترة' : MONTHS.slice(-S.span)[S.sel];
    $('#nxDistNote').textContent = m;
    $('#nxDistV').innerHTML = CH.map(([n, p, c], i) => `<button type="button" class="nx-ch${distHl === i ? ' on' : ''}" data-ch="${i}"><span><i style="background:${c}"></i>${n}</span><b><span class="num" data-dv="${i}">0</span> <small>ألف ر.س</small></b></button>`).join('');
    CH.forEach(([, p], i) => count($(`[data-dv="${i}"]`), base * p * share, v => nf(v, 2), 800));
    const R = 90, L = Math.PI * R; let acc = 0;
    $('#nxDonut').innerHTML = `<svg viewBox="0 0 220 124"><path class="tr" d="M20,112 A${R},${R} 0 0 1 200,112"/>${CH.map(([n, p, c], i) => { const len = p * L - 5, s = `<path class="ds${distHl === i ? ' on' : ''}${distHl != null && distHl !== i ? ' dim' : ''}" data-ch="${i}" d="M20,112 A${R},${R} 0 0 1 200,112" stroke="${c}" stroke-dasharray="${len.toFixed(1)} ${L + 10}" stroke-dashoffset="${(-acc).toFixed(1)}" style="--i:${i};--len:${len.toFixed(1)}"/>`; acc += p * L; return s; }).join('')}</svg>
      <span class="c"><b class="num">${nf((distHl == null ? 1 : CH[distHl][1]) * 100, 0)}%</b><small>${distHl == null ? 'كل القنوات' : CH[distHl][0]}</small></span>`;
  }

  /* ---------- 5) قائمة التكاملات ---------- */
  const INT = [['ميسر', 'مدفوعات', 40, 650.00, '#4D4286'], ['تابي', 'تقسيط', 80, 720.50, '#00BAB3'], ['سلة', 'متجر إلكتروني', 20, 432.25, '#127DBD'], ['زد', 'متجر إلكتروني', 60, 518.10, '#7B6FC9'], ['إعلانات جوجل', 'إعلانات', 55, 604.75, '#D9822B'], ['واتساب للأعمال', 'تواصل', 72, 389.40, '#1E9E6A']];
  function integ(intro) {
    const [k, d] = S.sort, L = INT.map((r, i) => ({ n: r[0], t: r[1], r: r[2], p: r[3], c: r[4], i })).sort((a, b) => (k === 'rate' ? a.r - b.r : k === 'profit' ? a.p - b.p : a.n.localeCompare(b.n, 'ar')) * d).slice(0, S.more ? 6 : 4);
    $$('#nxInt th[data-sort]').forEach(th => th.setAttribute('aria-sort', th.dataset.sort === k ? (d > 0 ? 'ascending' : 'descending') : 'none'));
    $('#nxInt tbody').innerHTML = L.map((r, j) => `<tr style="--i:${j}" class="${intro ? 'in' : ''}${S.sl.has(r.i) ? ' on' : ''}"><td><input type="checkbox" data-row="${r.i}" ${S.sl.has(r.i) ? 'checked' : ''} aria-label="تحديد ${r.n}"></td>
      <td><span class="nx-app"><i style="background:${r.c}">${r.n[0]}</i>${r.n}</span></td><td class="mut">${r.t}</td>
      <td><span class="nx-rate"><span class="tr"><i style="width:${r.r}%;--i:${j}"></i></span><span class="num">${r.r}%</span></span></td>
      <td><span class="num">${nf(r.p, 2)}</span> <small>ألف ر.س</small></td></tr>`).join('');
    $('#nxMore').textContent = S.more ? 'عرض أقل' : 'عرض الكل';
  }

  /* ---------- الأحداث ---------- */
  root.addEventListener('click', e => {
    const t = e.target;
    const mt = t.closest('[data-met]'); if (mt) { S.met = mt.dataset.met; $$('[data-met]').forEach(b => b.setAttribute('aria-selected', String(b === mt))); salesHead(); tweenSales(false); return dist(); }
    const sp = t.closest('[data-span]'); if (sp) { S.span = +sp.dataset.span; S.sel = null; $$('[data-span]').forEach(b => b.setAttribute('aria-pressed', String(b === sp))); salesHead(); tweenSales(true); return dist(); }
    const c = t.closest('#nxSales [data-c]'); if (c) return selMonth(+c.dataset.c);
    const lg = t.closest('#nxLeg [data-l]'); if (lg) { const l = +lg.dataset.l; S.hl = S.hl === l ? null : l; return applyHl(); }
    const dy = t.closest('[data-day]'); if (dy) { S.day = +dy.dataset.day; return subs(); }
    const wk = t.closest('[data-week]'); if (wk) { S.week = wk.dataset.week; S.day = null; $$('[data-week]').forEach(b => b.setAttribute('aria-pressed', String(b === wk))); return subs(); }
    const ch = t.closest('[data-ch]'); if (ch) { const i = +ch.dataset.ch; distHl = distHl === i ? null : i; return dist(); }
    const so = t.closest('th[data-sort]'); if (so) { const k = so.dataset.sort; S.sort = [k, S.sort[0] === k ? -S.sort[1] : -1]; return integ(false); }
    if (t.closest('#nxMore')) { S.more = !S.more; return integ(true); }
    const inf = t.closest('[data-info]'); if (inf) return toast(['عدد مرات عرض صفحات الموقع خلال الفترة', 'إجمالي الإيرادات من كل القنوات', 'نسبة الزيارات التي غادرت من الصفحة الأولى؛ الانخفاض تحسّن'][+inf.dataset.info]);
    if (t.closest('[data-export]')) return toast('يُصدَّر تقرير اللوحة بصيغة PDF أو Excel من هنا.');
    if (t.closest('[data-filter]')) return toast('لوحة التصفية: الدولة، القناة، الحملة.');
  });
  root.addEventListener('change', e => { const t = e.target; if (t.dataset.row) { const i = +t.dataset.row; t.checked ? S.sl.add(i) : S.sl.delete(i); t.closest('tr').classList.toggle('on', t.checked); } });

  salesHead(); tweenSales(true); subs(); dist(); integ(true);
})();
