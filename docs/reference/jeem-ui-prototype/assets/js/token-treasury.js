/* ==========================================================
   جيم — خزينة الأصول الرقمية (لوحة تفاعلية، ثيم بنفسجي داكن محصور في .tt)
   الحركات: عدّ الأرقام، أعمدة تنمو بالتتابع، منحنيات تنكشف من اليمين، حلقة تُكتسح،
   مؤشر عمودي يتبع الماوس ويحدّث الرقم، ومحاكي فائدة مركبة يتحدث مع كل تحريك.
   ========================================================== */
(function () {
  const root = document.getElementById('ttRoot'); if (!root) return;
  const $ = (s, el = root) => el.querySelector(s), $$ = (s, el = root) => [...el.querySelectorAll(s)];
  const RM = matchMedia('(prefers-reduced-motion: reduce)').matches;
  const nf = (v, d = 0) => Number(v).toLocaleString('en-US', { minimumFractionDigits: d, maximumFractionDigits: d });
  const kfmt = v => v >= 1e6 ? (v / 1e6).toFixed(2) + 'M' : v >= 1e3 ? (v / 1e3).toFixed(2) + 'K' : nf(v);
  const count = (el, to, fmt, dur = 1000) => { if (!el) return; if (RM) { el.textContent = fmt(to); return; } const from = +el.dataset.v || 0, t0 = performance.now(); el.dataset.v = to; const st = t => { const p = Math.min(1, (t - t0) / dur), e = 1 - Math.pow(1 - p, 3); el.textContent = fmt(from + (to - from) * e); if (p < 1) requestAnimationFrame(st); }; requestAnimationFrame(st); };
  let sd = 5; const rnd = () => (sd = (sd * 16807) % 2147483647) / 2147483647;
  const S = { cur: 'USD', rev: 'w', revP: 'w' };
  const FX = { USD: [1, 'USD'], SAR: [3.75, 'ر.س'] };

  /* ---------- 1) الأصول التشغيلية + التفصيل ---------- */
  function assets() {
    const [r, u] = FX[S.cur]; count($('#ttTot'), 1618801.89 * r, v => nf(v, 2)); $('#ttTotU').textContent = u;
    const A = [['ETH', 'إيثريوم', 611.85, 'eth', 2], ['SAR', 'ريال رقمي', 546013875 / 100, 'sar', 0], ['USDT', 'دولار مستقر', 411406.08, 'usdt', 2]];
    if (!$('#ttAs').children.length) $('#ttAs').innerHTML = A.map(([s, n, v, k, d], i) => `<button type="button" class="tt-as ${k}" style="--i:${i}" data-as="${i}" aria-pressed="false"><span class="sy">${s}</span><small>${n}</small><b class="num" data-a="${i}">0</b></button>`).join('');
    A.forEach(([, , v, , d], i) => count($(`[data-a="${i}"]`), v, x => nf(x, d)));
  }

  /* ---------- 2) الإيراد: الربح والعائد ---------- */
  const REV = { w: [2079, 0.1285], m: [9140, 0.5612], y: [104220, 6.7140] };
  function revenue() { const [p, y] = REV[S.rev]; count($('#ttProfit'), p, v => '$' + nf(v)); count($('#ttYield'), y, v => nf(v, 4) + '%'); }

  /* ---------- 3) المؤشرات الجانبية ---------- */
  const SIDE = [['القيمة السوقية', 2.34e6, 'chart'], ['القيمة المقفلة', 17.01e6, 'shield'], ['عدد الحائزين', 3400, 'users'], ['حجم 24 ساعة', 314.74e3, 'history']];
  function side() {
    if (!$('#ttSide').children.length) $('#ttSide').innerHTML = SIDE.map(([n, , icn], i) => `<div class="tt-sd" style="--i:${i}"><span><small>${n}</small><b class="num" data-s="${i}">0</b></span><span class="ib"><svg class="icon" aria-hidden="true"><use href="#i-${icn}"/></svg></span></div>`).join('');
    SIDE.forEach(([, v], i) => count($(`[data-s="${i}"]`), v, x => i === 2 ? nf(x) : kfmt(x)));
  }

  /* ---------- 4) نمو مشاركة المستخدمين (أعمدة؛ النمو في التلميح بدل محور ثانٍ) ---------- */
  const PART = { 2022: [1800, 2600, 4400, 2500], 2023: [3600, 6900, 9400, 13100], 2024: [12000, 14800, 16300, 19600] };
  function part() {
    const mx = 20000; let prev = null, j = 0;
    $('#ttPart').innerHTML = Object.entries(PART).map(([y, q]) => `<div class="yr"><div class="qs">${q.map((v, i) => { const g = prev ? (v / prev - 1) * 100 : null; prev = v; const k = j++;
      return `<button type="button" class="q" style="--h:${(v / mx * 100).toFixed(1)}%;--i:${k}" aria-label="${y} الربع ${i + 1}: ${nf(v)} مستخدم${g != null ? `، نمو ${nf(g, 1)}%` : ''}" data-tip="${y} · الربع ${i + 1}|${nf(v)} مستخدم${g != null ? ` · ${g >= 0 ? '+' : ''}${nf(g, 1)}% عن الربع السابق` : ''}"><i></i><small>ر${i + 1}</small></button>`; }).join('')}</div><span class="yl num">${y}</span></div>`).join('');
  }

  /* ---------- منحنى مساحي عام مع مؤشر يتبع الماوس ---------- */
  function area(box, vals, labs, opts) {
    const W = 600, H = opts.h || 200, P = 8, mx = Math.max(...vals) * 1.08, mn = opts.zero ? 0 : Math.min(...vals) * .9, n = vals.length;
    const x = i => W - P - i * (W - 2 * P) / (n - 1), y = v => H - 22 - (v - mn) / (mx - mn) * (H - 34);
    const pts = vals.map((v, i) => `${x(i).toFixed(1)},${y(v).toFixed(1)}`).join(' ');
    const id = 'g' + Math.random().toString(36).slice(2, 7);
    box.innerHTML = `<svg viewBox="0 0 ${W} ${H}" preserveAspectRatio="none" class="tt-area"><defs><linearGradient id="${id}" x1="0" x2="0" y1="0" y2="1"><stop offset="0" stop-color="#7B5CFF" stop-opacity=".55"/><stop offset="1" stop-color="#7B5CFF" stop-opacity="0"/></linearGradient></defs>
      <g class="rv"><polygon points="${x(0)},${H - 22} ${pts} ${x(n - 1)},${H - 22}" fill="url(#${id})"/><polyline points="${pts}" fill="none" stroke="#8F74FF" stroke-width="2" vector-effect="non-scaling-stroke" stroke-linejoin="round"/></g>
      <line class="cx" y1="6" y2="${H - 22}" opacity="0" vector-effect="non-scaling-stroke"/></svg><div class="tt-dot" hidden></div><div class="tt-tp" hidden></div>
      <div class="tt-xl">${opts.ticks.map(t => `<span>${t}</span>`).join('')}</div>`;
    const svg = box.querySelector('svg'), cx = svg.querySelector('.cx'), dot = box.querySelector('.tt-dot'), tp = box.querySelector('.tt-tp');
    const show = i => { const r = svg.getBoundingClientRect(), px = x(i) / W * r.width, py = y(vals[i]) / H * r.height; cx.setAttribute('x1', x(i)); cx.setAttribute('x2', x(i)); cx.setAttribute('opacity', 1);
      dot.hidden = false; dot.style.left = px + 'px'; dot.style.top = py + 'px'; tp.hidden = false; tp.innerHTML = opts.tip(i); tp.style.left = Math.max(60, Math.min(r.width - 60, px)) + 'px'; tp.style.top = Math.max(0, py - 12) + 'px'; opts.on && opts.on(i); };
    const hide = () => { cx.setAttribute('opacity', 0); dot.hidden = tp.hidden = true; opts.off && opts.off(); };
    svg.onpointermove = e => { const r = svg.getBoundingClientRect(), xx = (e.clientX - r.left) / r.width * W; show(Math.max(0, Math.min(n - 1, Math.round((W - P - xx) / ((W - 2 * P) / (n - 1)))))); };
    svg.onpointerleave = hide; svg.tabIndex = 0; svg.setAttribute('role', 'img'); svg.setAttribute('aria-label', opts.label);
    let k = null; svg.onkeydown = e => { if (e.key === 'ArrowLeft' || e.key === 'ArrowRight') { e.preventDefault(); k = k == null ? 0 : Math.max(0, Math.min(n - 1, k + (e.key === 'ArrowLeft' ? 1 : -1))); show(k); } }; svg.onblur = () => { k = null; hide(); };
    if (opts.peak != null) show(opts.peak);
  }

  /* ---------- 5) منحنى الإيراد ---------- */
  const RV = { w: { v: [21000, 28000, 52000, 66000, 71000, 90292, 74000, 95000, 88000, 128000], l: ['السبت', 'الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة'] },
    m: { v: [210, 260, 240, 330, 360, 410, 395, 470, 520, 610].map(v => v * 1000), l: ['أ1', 'أ2', 'أ3', 'أ4'] },
    y: { v: [1.2, 1.5, 1.4, 2.1, 2.6, 2.4, 3.1, 3.6, 3.4, 4.2, 4.8, 5.3].map(v => v * 1e6), l: ['يناير', 'أبريل', 'يوليو', 'أكتوبر'] } };
  function revChart() {
    const R = RV[S.revP], v = R.v;
    area($('#ttRev'), v, null, { h: 190, zero: true, ticks: R.l, label: 'منحنى الإيراد، الأحدث يسارًا', peak: v.indexOf(Math.max(...v.slice(0, -1))),
      tip: i => { const d = i ? (v[i] / v[i - 1] - 1) * 100 : 0; return `<b class="num">$${nf(v[i])}</b>${i ? `<span class="${d >= 0 ? 'up' : 'dn'} num">${d >= 0 ? '+' : ''}${nf(d, 2)}%</span>` : ''}`; } });
  }

  /* ---------- 6) المحروق مقابل المتداول ---------- */
  function supply() {
    const circ = 675152272, burn = 1252873, tot = circ + burn * 30, R = 70, L = 2 * Math.PI * R, bf = .12;
    $('#ttSup').innerHTML = `<svg viewBox="0 0 200 200" role="img" aria-label="المعروض المتداول ${nf(circ)} والمحروق حتى الآن ${nf(burn)}"><circle cx="100" cy="100" r="${R}" class="tr"/>
      <circle cx="100" cy="100" r="${R}" class="c" stroke-dasharray="${(L * (1 - bf) - 3).toFixed(1)} ${L}" transform="rotate(-90 100 100)"/>
      <circle cx="100" cy="100" r="${R}" class="b" stroke-dasharray="${(L * bf - 3).toFixed(1)} ${L}" stroke-dashoffset="${(-(L * (1 - bf))).toFixed(1)}" transform="rotate(-90 100 100)"><title>المحروق حتى الآن ${nf(burn)}</title></circle></svg>
      <div class="c-in"><small>المعروض المتداول</small><b class="num" id="ttCirc">0</b></div>
      <div class="call"><small>المحروق حتى الآن</small><b class="num">${nf(burn)}</b></div>`;
    count($('#ttCirc'), circ, v => nf(v), 1300);
  }

  /* ---------- 7) تاريخ القيمة المقفلة ---------- */
  function tvl() {
    const n = 120, v = []; let x = 1.2; for (let i = 0; i < n; i++) { x = Math.max(.4, x * (1 + (rnd() - .48) * .12)); v.push(x * 1e6); }
    const date = i => { const d = new Date(2023, 1, 21); d.setDate(d.getDate() - (n - 1 - i) * 9); return d.toLocaleDateString('ar-SA-u-ca-gregory-nu-latn', { year: 'numeric', month: 'short', day: 'numeric' }); };
    const head = i => { $('#ttTvlV').textContent = '$' + kfmt(v[i]); $('#ttTvlD').textContent = date(i); };
    area($('#ttTvl'), v, null, { h: 170, ticks: ['2020', '2021', '2022', '2023'], label: 'تاريخ القيمة المقفلة، الأحدث يسارًا', tip: i => `<b class="num">$${kfmt(v[i])}</b>`, on: head, off: () => head(n - 1) });
    head(n - 1);
  }

  /* ---------- 8) محاكي الفائدة المركبة ---------- */
  const SIM = [['المبلغ المبدئي', 'p', 100, 100000, 100, 1000, v => '$' + nf(v)], ['الإيداع الشهري', 'm', 0, 5000, 50, 100, v => '$' + nf(v)], ['العائد السنوي', 'r', 1, 30, .5, 8, v => nf(v, 1) + '%'], ['المدة', 'y', 1, 30, 1, 5, v => nf(v) + ' سنة']];
  function sim() {
    $('#ttSim').innerHTML = SIM.map(([n, k, mn, mx, st, d, f]) => `<label class="tt-sl"><span>${n}</span><b class="num" data-sv="${k}">${f(d)}</b><input type="range" min="${mn}" max="${mx}" step="${st}" value="${d}" data-sim="${k}" aria-label="${n}"></label>`).join('');
    calc();
  }
  function calc() {
    const g = k => +$(`[data-sim="${k}"]`).value, p = g('p'), m = g('m'), r = g('r') / 100 / 12, n = g('y') * 12;
    SIM.forEach(([, k, , , , , f]) => { $(`[data-sv="${k}"]`).textContent = f(g(k)); const el = $(`[data-sim="${k}"]`); el.style.setProperty('--p', ((g(k) - el.min) / (el.max - el.min) * 100) + '%'); });
    const fv = p * Math.pow(1 + r, n) + m * ((Math.pow(1 + r, n) - 1) / r), dep = p + m * n;
    count($('#ttGet'), fv, v => '$' + nf(v, 2), 500);
    $('#ttSplit').innerHTML = `<i style="width:${(dep / fv * 100).toFixed(1)}%"></i>`; $('#ttSplitL').innerHTML = `المودَع <b class="num">$${nf(dep)}</b> · الأرباح <b class="num">$${nf(fv - dep)}</b>`;
  }

  /* ---------- الأحداث ---------- */
  root.addEventListener('change', e => { const t = e.target;
    if (t.id === 'ttCur') { S.cur = t.value; return assets(); }
    if (t.id === 'ttRevSel') { S.rev = t.value; return revenue(); } });
  root.addEventListener('input', e => { if (e.target.dataset.sim) calc(); });
  root.addEventListener('click', e => {
    const p = e.target.closest('[data-revp]'); if (p) { S.revP = p.dataset.revp; $$('[data-revp]').forEach(b => b.setAttribute('aria-pressed', String(b === p))); return revChart(); }
    const a = e.target.closest('[data-as]'); if (a) { const on = a.getAttribute('aria-pressed') !== 'true'; $$('[data-as]').forEach(b => b.setAttribute('aria-pressed', String(on && b === a))); $('#ttAs').classList.toggle('has-on', on); }
  });

  /* ---------- الثيم: فاتح (افتراضي) أو داكن، ويُحفظ لكل مستخدم ---------- */
  (function () { let th = 'light'; try { th = localStorage.getItem('jeem.tt.theme') || 'light'; } catch (e) {}
    const set = t => { root.classList.toggle('tt-light', t === 'light'); $$('[data-theme-set]').forEach(b => b.setAttribute('aria-pressed', String(b.dataset.themeSet === t))); try { localStorage.setItem('jeem.tt.theme', t); } catch (e) {} };
    set(th); root.addEventListener('click', e => { const b = e.target.closest('[data-theme-set]'); if (b) set(b.dataset.themeSet); }); })();
  assets(); revenue(); side(); part(); revChart(); supply(); tvl(); sim();
})();
