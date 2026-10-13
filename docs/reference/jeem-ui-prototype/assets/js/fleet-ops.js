/* ==========================================================
   جيم — إدارة الأسطول والتوصيل (لوحة تفاعلية متحركة)
   الحركات: عدّ المؤشرات، رسم المنحنيات تدريجيًا، شاحنات تتحرك على الخريطة،
   تقدّم الرحلات حيًا، أعمدة التحميل، واكتساح حلقة حالة الطلبات.
   ========================================================== */
(function () {
  const root = document.getElementById('flRoot'); if (!root) return;
  const $ = (s, el = root) => el.querySelector(s), $$ = (s, el = root) => [...el.querySelectorAll(s)];
  const esc = s => String(s).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
  const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
  const nf = (v, d = 0) => Number(v).toLocaleString('en-US', { maximumFractionDigits: d, minimumFractionDigits: d });
  const RM = matchMedia('(prefers-reduced-motion: reduce)').matches;
  const NS = 'http://www.w3.org/2000/svg';
  const count = (el, to, fmt, dur = 900) => { if (RM) { el.textContent = fmt(to); return; } const from = +el.dataset.v || 0, t0 = performance.now(); el.dataset.v = to; const st = t => { const p = Math.min(1, (t - t0) / dur), e = 1 - Math.pow(1 - p, 3); el.textContent = fmt(from + (to - from) * e); if (p < 1) requestAnimationFrame(st); }; requestAnimationFrame(st); };
  const ST = { move: { n: 'في الطريق', c: 'var(--fl-ok)' }, pick: { n: 'استلام', c: 'var(--fl-teal)' }, stop: { n: 'متوقف', c: 'var(--fl-orange)' }, late: { n: 'متأخر', c: 'var(--fl-bad)' } };

  /* ---------- 1) مؤشرات العمليات ---------- */
  const KPIS = [
    { ic: 'layers', n: 'إجمالي الشحنات اليوم', v: 1284, f: v => nf(v), d: '+12.5%', g: true, s: 'من كل الشحنات', c: 'var(--fl-ok)' },
    { ic: 'truck', n: 'الشاحنات النشطة', v: 482, f: v => nf(v), d: '+12.5%', g: true, s: 'على الطريق الآن', c: 'var(--fl-teal)' },
    { ic: 'clock', n: 'متوسط زمن التسليم', v: 252, f: v => `${Math.floor(v / 60)}:${String(Math.round(v % 60)).padStart(2, '0')}`, u: 'ساعة', d: '−0:18', g: true, s: 'أسرع من المتوسط', c: 'var(--brand-blue)' },
    { ic: 'check', n: 'نسبة التسليم في الموعد', v: 98.2, f: v => nf(v, 1) + '%', d: '-0.4%', g: false, s: 'المستهدف 99%', c: 'var(--fl-bad)' }
  ];
  $('#flKpis').innerHTML = KPIS.map((k, i) => `<div class="fl-kpi" style="--c:${k.c};--i:${i}"><span class="lb">${ic(k.ic)}${k.n}</span><div class="row"><span class="vv"><b class="num" data-k="${i}">0</b>${k.u ? `<small>${k.u}</small>` : ''}</span><span class="dl ${k.g ? 'up' : 'dn'} num">${k.d}</span></div><div class="row sm"><span>${k.s}</span><span>مقارنة بالأمس</span></div></div>`).join('');
  const kpis = () => KPIS.forEach((k, i) => count($(`[data-k="${i}"]`), k.v * (S.period === 'day' ? 1 : S.period === 'week' ? 6.4 : 27) / (i >= 2 ? (S.period === 'day' ? 1 : S.period === 'week' ? 6.4 : 27) : 1), k.f, 1100));

  /* ---------- 2) أداء التسليم (منحنيان يُرسمان تدريجيًا) ---------- */
  const S = { period: 'day', hide: new Set(), sel: null, filter: new Set(Object.keys(ST)), q: '' };
  const HOURS = ['06:00', '08:00', '10:00', '12:00', '14:00', '16:00', '18:00'];
  function perf() {
    const m = S.period === 'day' ? 1 : S.period === 'week' ? 6.2 : 26, D = [30, 50, 130, 225, 350, 465, 565].map(v => v * m), L = [5, 12, 20, 32, 52, 76, 78].map(v => v * m);
    const W = 520, H = 320, P = 34, mx = Math.ceil(Math.max(...D) * 1.06 / 100) * 100, x = i => W - P / 2 - i * (W - P * 1.6) / 6, y = v => H - 26 - v / mx * (H - 46);
    const path = A => A.map((v, i) => `${i ? 'L' : 'M'}${x(i).toFixed(1)},${y(v).toFixed(1)}`).join(' ');
    const ser = [['del', 'مُسلَّم', D, 'var(--fl-ok)'], ['late', 'متأخر', L, 'var(--fl-teal)']];
    $('#flPerf').innerHTML = `<svg viewBox="0 0 ${W} ${H}" class="fl-line"><g class="grid">${[0, .2, .4, .6, .8, 1].map(t => `<line x1="${P}" x2="${W - 8}" y1="${y(mx * t)}" y2="${y(mx * t)}"/><text x="${W - 4}" y="${y(mx * t) + 4}">${nf(mx * t)}</text>`).join('')}${HOURS.map((h, i) => `<text class="xl" x="${x(i)}" y="${H - 6}">${h}</text>`).join('')}</g>
      ${ser.filter(s => !S.hide.has(s[0])).map(([k, n, A, c]) => `<path class="ln" d="${path(A)}" stroke="${c}" pathLength="1"/>${A.map((v, i) => `<circle class="pt" cx="${x(i)}" cy="${y(v)}" r="4" stroke="${c}" style="--i:${i}"/>`).join('')}`).join('')}
      <line class="cx" y1="12" y2="${H - 26}" style="opacity:0"/></svg><div class="fl-tip" hidden></div>`;
    const svg = $('#flPerf svg'), tip = $('#flPerf .fl-tip'), cx = $('.cx', svg);
    svg.onpointermove = e => { const r = svg.getBoundingClientRect(), xx = (e.clientX - r.left) / r.width * W, i = Math.max(0, Math.min(6, Math.round((W - P / 2 - xx) / ((W - P * 1.6) / 6)))); cx.setAttribute('x1', x(i)); cx.setAttribute('x2', x(i)); cx.style.opacity = 1;
      tip.hidden = false; tip.innerHTML = `<b>${HOURS[i]}</b>${ser.filter(s => !S.hide.has(s[0])).map(([, n, A, c]) => `<span><i style="background:${c}"></i>${n}<b class="num">${nf(A[i])}</b></span>`).join('')}`; tip.style.left = (x(i) / W * r.width) + 'px'; };
    svg.onpointerleave = () => { tip.hidden = true; cx.style.opacity = 0; };
    $('#flLeg').innerHTML = ser.map(([k, n, , c]) => `<button type="button" data-ser="${k}" aria-pressed="${!S.hide.has(k)}"><i style="background:${c}"></i>${n}</button>`).join('');
  }

  /* ---------- 3) الخريطة الحية: شاحنات تتحرك على الطرق ---------- */
  const ROADS = [
    'M20,300 C140,280 220,230 330,240 S520,300 640,250 S760,200 780,190',
    'M60,60 C120,120 160,190 240,230 S360,330 420,400',
    'M780,90 C680,110 600,160 520,150 S380,90 300,110 S150,150 60,160',
    'M520,420 C520,350 560,300 600,250 S700,150 730,60',
    'M200,420 C230,360 300,320 380,330 S520,370 600,420'
  ];
  const HUBS = [['مركز الشمال', 170, 205], ['مركز المطار', 610, 210], ['مركز السلي', 380, 330]];
  const TRUCKS = [['TRK-042', 0, .08, 'move'], ['TRK-017', 1, .3, 'stop'], ['TRK-063', 2, .2, 'move'], ['TRK-088', 3, .55, 'late'], ['TRK-105', 2, .7, 'pick'], ['TRK-121', 4, .4, 'move'], ['TRK-009', 0, .62, 'move']].map(([id, r, o, st], i) => ({ id, r, o, st, sp: st === 'stop' ? 0 : st === 'late' ? .000012 : .000028 + i * .000002, eta: ['14:30', '18:15', '15:05', '16:40', '13:55', '17:20', '15:45'][i], from: ['الرياض', 'الرياض', 'الدمام', 'جدة', 'الرياض', 'المدينة', 'الخرج'][i], to: ['جدة', 'القصيم', 'الرياض', 'الطائف', 'الدمام', 'تبوك', 'الرياض'][i], load: [82, 64, 91, 55, 40, 77, 68][i] }));
  const map = $('#flMap');
  map.innerHTML = `<svg viewBox="0 0 800 440" preserveAspectRatio="xMidYMid slice" aria-label="خريطة تتبع الأسطول">
    <rect width="800" height="440" fill="var(--fl-land)"/>
    <path d="M420,0 C450,60 520,80 600,70 S760,110 800,150 L800,200 C740,160 640,140 560,140 S430,110 400,0Z" fill="var(--fl-water)"/>
    <path d="M0,330 C80,320 130,360 160,440 L0,440Z" fill="var(--fl-water)" opacity=".8"/>
    ${[[90, 90, 70, 40], [300, 170, 60, 34], [700, 300, 80, 46], [460, 250, 46, 30], [240, 380, 70, 28], [620, 380, 50, 30]].map(([x, y, a, b]) => `<ellipse cx="${x}" cy="${y}" rx="${a}" ry="${b}" fill="var(--fl-park)"/>`).join('')}
    ${ROADS.map(d => `<path d="${d}" class="rd-b"/><path d="${d}" class="rd"/>`).join('')}
    <g class="hubs">${HUBS.map(([n, x, y]) => `<g transform="translate(${x},${y})"><rect x="-44" y="-13" width="88" height="26" rx="13"/><text y="4">${n}</text></g>`).join('')}</g>
    <g id="flTrucks"></g></svg>
    <div class="fl-msearch"><label>${ic('search')}<span class="sr-only">ابحث عن شاحنة</span><input type="search" id="flQ" placeholder="ابحث عن شاحنة"></label></div>`;
  const rPaths = ROADS.map(d => { const p = document.createElementNS(NS, 'path'); p.setAttribute('d', d); return p; });
  const tg = $('#flTrucks');
  TRUCKS.forEach(t => { const g = document.createElementNS(NS, 'g'); g.setAttribute('class', 'fl-truck'); g.setAttribute('tabindex', 0); g.setAttribute('role', 'button'); g.dataset.id = t.id;
    g.innerHTML = `<g class="lb"><rect x="-30" y="-36" width="60" height="20" rx="10"/><text y="-22">${t.id}</text></g><circle r="11" style="fill:${ST[t.st].c}"/><path class="ar" d="M-4,-5 L5,0 L-4,5 Z"/>`;
    tg.append(g); t.g = g; });
  let last = 0;
  function frame(ts) {
    const dt = last ? Math.min(60, ts - last) : 16; last = ts;
    TRUCKS.forEach(t => { t.o = (t.o + dt * t.sp) % 1; const p = rPaths[t.r], L = p.getTotalLength(), a = p.getPointAtLength(t.o * L), b = p.getPointAtLength(Math.min(L, t.o * L + 2)), ang = Math.atan2(b.y - a.y, b.x - a.x) * 180 / Math.PI;
      t.g.setAttribute('transform', `translate(${a.x.toFixed(1)},${a.y.toFixed(1)})`); t.g.querySelector('.ar').setAttribute('transform', `rotate(${ang.toFixed(0)})`);
      const vis = S.filter.has(t.st) && (!S.q || t.id.toLowerCase().includes(S.q)); t.g.classList.toggle('hide', !vis); t.g.classList.toggle('sel', S.sel === t.id); });
    if (!RM) requestAnimationFrame(frame);
  }
  requestAnimationFrame(frame);
  $('#flMapLeg').innerHTML = Object.entries(ST).map(([k, s]) => `<button type="button" data-st="${k}" aria-pressed="true"><i style="background:${s.c}"></i>${s.n}</button>`).join('');

  /* ---------- 4) الأسطول النشط ---------- */
  function fleet() {
    const L = TRUCKS.filter(t => S.filter.has(t.st));
    $('#flFleet').innerHTML = L.map(t => `<button type="button" class="fl-trip${S.sel === t.id ? ' on' : ''}" data-trk="${t.id}">
      <span class="h"><b class="mono">${t.id}</b><span class="eta">• الوصول ${t.eta}</span><span class="tag st-${t.st}">${ST[t.st].n}</span></span>
      <span class="b"><span class="van">${ic('truck', 'icon')}</span><span class="ct"><b>${t.from}</b><small>المصدر</small></span><span class="tr"><i style="width:${Math.round(t.o * 100)}%"></i><em style="inset-inline-start:calc(${Math.round(t.o * 100)}% - 7px)"></em></span><span class="ct"><b>${t.to}</b><small>الوجهة</small></span></span></button>`).join('') || '<p class="muted">لا شاحنات بهذه الحالة.</p>';
  }

  /* ---------- 5) حالة التحميل ---------- */
  function loading() {
    const m = S.period === 'day' ? 1 : S.period === 'week' ? 6 : 25;
    const L = [['شاحنات قيد التحميل', 8, v => String(Math.round(v)).padStart(2, '0'), '', [40, 70, 55, 62, 48]], ['متوسط الحمولة', 72, v => Math.round(v) + '%', '', [60, 72, 68, 75, 80]], ['الحجم الإجمالي اليوم', 18.2 * m, v => nf(v, 1), 'م³', [50, 58, 46, 66, 70]], ['الوزن الإجمالي', 4.1 * m, v => nf(v, 1), 'طن', [35, 52, 60, 48, 66]]];
    $('#flLoad').innerHTML = L.map(([n, v, f, u, bars], i) => `<div class="fl-ls"><span class="vv"><b class="num" data-l="${i}">0</b>${u ? `<small>${u}</small>` : ''}</span><span>${n}</span><span class="mb">${bars.map((h, j) => `<i style="height:${h}%;--j:${j}"></i>`).join('')}</span></div>`).join('');
    L.forEach(([, v, f], i) => count($(`[data-l="${i}"]`), v, f));
  }

  /* ---------- 6) حالة الطلبات (حلقة متدرجة) ---------- */
  function orders() {
    const tot = KPIS[0].v * (S.period === 'day' ? 1 : S.period === 'week' ? 6.4 : 27), P = [['في الطريق', 50, 'var(--fl-ok)'], ['مُسلَّم', 35, 'var(--fl-teal)'], ['متأخر', 15, 'var(--fl-orange)']];
    const R = 70, C = 2 * Math.PI * R; let acc = 0;
    $('#flOrders').innerHTML = `<div class="fl-dn"><svg viewBox="0 0 180 180"><circle cx="90" cy="90" r="${R + 12}" class="o"/>${P.map(([n, v, c], i) => { const s = `<circle cx="90" cy="90" r="${R}" class="sg" stroke="${c}" stroke-dasharray="${(v / 100 * C - 4).toFixed(1)} ${C}" stroke-dashoffset="${(-acc).toFixed(1)}" transform="rotate(-90 90 90)" style="--i:${i}" data-tip="${n}|${v}% · ${nf(tot * v / 100)} طلب"/>`; acc += v / 100 * C; return s; }).join('')}</svg>
      <span class="c"><b class="num" id="flTot">0</b><small>إجمالي الشحنات</small></span></div>
      <ul class="fl-ol">${P.map(([n, v, c]) => `<li><i style="background:${c}"></i><span>${n}</span><b class="num">${v}% · ${nf(tot * v / 100)}</b></li>`).join('')}</ul>`;
    count($('#flTot'), tot, v => nf(v), 1200);
  }

  /* ---------- الأحداث ---------- */
  root.addEventListener('click', e => {
    const t = e.target;
    const p = t.closest('[data-period]'); if (p) { S.period = p.dataset.period; $$('[data-period]').forEach(b => b.setAttribute('aria-pressed', String(b === p))); return all(); }
    const s = t.closest('[data-ser]'); if (s) { const k = s.dataset.ser; S.hide.has(k) ? S.hide.delete(k) : S.hide.add(k); if (S.hide.size === 2) S.hide.delete(k); return perf(); }
    const st = t.closest('[data-st]'); if (st) { const k = st.dataset.st; if (S.filter.has(k)) { if (S.filter.size > 1) S.filter.delete(k); } else S.filter.add(k); $$('[data-st]').forEach(b => b.setAttribute('aria-pressed', String(S.filter.has(b.dataset.st)))); return fleet(); }
    const tr = t.closest('[data-trk]') || t.closest('.fl-truck'); if (tr) { const id = tr.dataset.trk || tr.dataset.id; S.sel = S.sel === id ? null : id; fleet(); const card = $(`[data-trk="${id}"]`); if (card && S.sel) card.scrollIntoView({ block: 'nearest', behavior: RM ? 'auto' : 'smooth' }); return; }
    if (t.closest('[data-ship]')) return toast('نموذج «شحنة سريعة» يُفتح هنا لاختيار المصدر والوجهة والشاحنة.');
  });
  $('#flQ').addEventListener('input', e => { S.q = e.target.value.trim().toLowerCase(); });
  const toast = msg => { let t = document.querySelector('.toast'); if (!t) { t = document.createElement('div'); t.className = 'toast'; t.setAttribute('role', 'status'); document.body.append(t); } t.innerHTML = `${ic('check')}<span>${esc(msg)}</span>`; requestAnimationFrame(() => t.classList.add('is-on')); clearTimeout(t._h); t._h = setTimeout(() => t.classList.remove('is-on'), 2600); };
  function all() { kpis(); perf(); fleet(); loading(); orders(); }
  all();
  /* تحديث حي: تقدّم الرحلات ووقت آخر تحديث */
  const tick = () => TRUCKS.forEach(t => { const c = $(`[data-trk="${t.id}"]`); if (!c) return; const v = Math.round(t.o * 100); $('.tr i', c).style.width = v + '%'; $('.tr em', c).style.insetInlineStart = `calc(${v}% - 7px)`; });
  setInterval(() => { if (!document.hidden) { tick(); const d = new Date(); $('#flUpd').textContent = `اليوم ${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`; } }, 3000);
})();
