/* ==========================================================
   جيم — نبض المتجر الإلكتروني (لوحة تفاعلية بأسلوب البطاقات الداكنة)
   - إجمالي المبيعات: شريط أيام قابل للسحب، والرقم يتبع اليوم المختار.
   - الحملات النشطة: حصص القنوات بأشكال قابلة للضغط.
   - التفاعل الأسبوعي: حلقة تطبيق الجوال مقابل الموقع.
   - الحملات (قائمة قابلة للطي): منحنى الوصول بمؤشر يتبع الماوس.
   - اتجاه المبيعات: أعمدة الأيام (المحقق مقابل المستهدف)، والضغط على يوم يحدّث الأرقام.
   - أداء المنتجات: بطاقات تتنقل بالأسهم.
   - الفترة (يومي/أسبوعي/شهري) وتبويبات الأقسام، ولون التمييز (ليموني/هوية أحد).
   ========================================================== */
(function () {
  const root = document.getElementById('skRoot'); if (!root) return;
  const $ = (s, el = root) => el.querySelector(s), $$ = (s, el = root) => [...el.querySelectorAll(s)];
  const esc = s => String(s).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
  const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
  const nf = (v, d = 0) => Number(v).toLocaleString('en-US', { maximumFractionDigits: d, minimumFractionDigits: d });
  const k = v => v >= 1e6 ? nf(v / 1e6, 2) + ' م' : v >= 1e3 ? nf(v / 1e3, 1) + ' ألف' : nf(v);
  /* الرقم والوحدة منفصلان حتى لا ينعكس ترتيب «ألف» داخل الرقم ذي الاتجاه LTR */
  const kn = v => v >= 1e6 ? [nf(v / 1e6, 2), 'مليون ر.س'] : v >= 1e3 ? [nf(v / 1e3, 1), 'ألف ر.س'] : [nf(v), 'ر.س'];
  const big = (el, v) => { count(el, v, x => kn(x)[0]); el.nextElementSibling.textContent = kn(v)[1]; };
  const reduceMotion = matchMedia('(prefers-reduced-motion: reduce)').matches;

  const DAYS = ['السبت', 'الأحد', 'الإثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة'];
  const PERIOD = { day: { n: 'يومي', mul: 1 / 7, lab: 'اليوم' }, week: { n: 'أسبوعي', mul: 1, lab: 'هذا الأسبوع' }, month: { n: 'شهري', mul: 4.3, lab: 'هذا الشهر' } };
  const S = { period: 'week', day: 5, scrub: 3, camp: 0, chan: 0, prod: 0, tab: 'all' };
  const BASE = { sales: [3900, 4410, 4630, 5230, 4800, 6120, 5100], target: [5600, 5500, 5900, 6300, 5700, 6400, 5400] };
  const CAMPAIGNS = [
    { n: 'مهرجان الأجهزة', reach: [180, 240, 310, 420, 580, 610, 640], date: '26 سبتمبر' },
    { n: 'عروض الصيف', reach: [90, 150, 160, 210, 260, 330, 300], date: '12 أغسطس' },
    { n: 'العودة للمدارس', reach: [120, 200, 280, 300, 360, 420, 470], date: '30 أغسطس' }
  ];
  const PRODUCTS = [
    { n: 'كاميرا مراقبة 360°', units: 1240, rev: 461000, d: '+18%', ic: 'cam' },
    { n: 'سماعات لاسلكية', units: 2310, rev: 1270000, d: '+31%', ic: 'buds' },
    { n: 'ساعة ذكية رياضية', units: 980, rev: 734000, d: '-4%', ic: 'watch' },
    { n: 'شاحن متنقل 20 ألف', units: 3120, rev: 405000, d: '+9%', ic: 'bank' }
  ];
  const GLYPH = {
    cam: '<svg viewBox="0 0 64 64"><circle cx="32" cy="26" r="16" fill="#2B2B2B" stroke="#555" stroke-width="2"/><circle cx="32" cy="26" r="7" fill="#111" stroke="#777"/><circle cx="34" cy="24" r="2" fill="#9AF"/><path d="M26 42h12l3 10H23z" fill="#3A3A3A"/></svg>',
    buds: '<svg viewBox="0 0 64 64"><path d="M20 14c-6 0-9 5-9 10s3 9 7 9v17h6V22c0-5-1-8-4-8z" fill="#EDEDED"/><path d="M44 14c6 0 9 5 9 10s-3 9-7 9v17h-6V22c0-5 1-8 4-8z" fill="#EDEDED"/></svg>',
    watch: '<svg viewBox="0 0 64 64"><rect x="24" y="4" width="16" height="12" rx="3" fill="#333"/><rect x="24" y="48" width="16" height="12" rx="3" fill="#333"/><rect x="18" y="14" width="28" height="36" rx="9" fill="#2A2A2A" stroke="#666" stroke-width="2"/><path d="M27 32h10M32 26v6" stroke="var(--sk-acc)" stroke-width="2.5" stroke-linecap="round"/></svg>',
    bank: '<svg viewBox="0 0 64 64"><rect x="18" y="8" width="28" height="48" rx="7" fill="#2F2F2F" stroke="#666" stroke-width="2"/><rect x="25" y="16" width="14" height="4" rx="2" fill="var(--sk-acc)"/><circle cx="32" cy="46" r="3" fill="#888"/></svg>'
  };
  const series = key => BASE[key].map(v => v * PERIOD[S.period].mul);

  /* ---------- الأرقام المتحركة ---------- */
  function count(el, to, fmt) {
    if (reduceMotion) { el.textContent = fmt(to); return; }
    const from = +el.dataset.v || 0, t0 = performance.now(); el.dataset.v = to;
    const step = t => { const p = Math.min(1, (t - t0) / 600), e = 1 - Math.pow(1 - p, 3); el.textContent = fmt(from + (to - from) * e); if (p < 1) requestAnimationFrame(step); };
    requestAnimationFrame(step);
  }

  /* ---------- 1) إجمالي المبيعات + شريط الأيام القابل للسحب ---------- */
  function total() {
    const s = series('sales'), cum = s.slice(0, S.scrub + 1).reduce((a, b) => a + b, 0);
    big($('#skTotal'), cum);
    $('#skTotalSub').textContent = `${PERIOD[S.period].lab} حتى ${DAYS[S.scrub]}`;
    const p = (S.scrub + .5) / 7 * 100; $('.sk-scrub .fill').style.width = p + '%'; $('.sk-scrub .knob').style.insetInlineStart = `calc(${p}% - 9px)`;
    $('.sk-scrub').setAttribute('aria-valuenow', S.scrub); $('.sk-scrub').setAttribute('aria-valuetext', DAYS[S.scrub]);
    $$('.sk-days span').forEach((d, i) => d.classList.toggle('on', i <= S.scrub));
  }
  const scrub = $('.sk-scrub');
  const scrubAt = e => { const r = scrub.getBoundingClientRect(), x = (r.right - e.clientX) / r.width; S.scrub = Math.max(0, Math.min(6, Math.floor(x * 7))); total(); };
  scrub.addEventListener('pointerdown', e => { scrub.setPointerCapture(e.pointerId); scrubAt(e); scrub.onpointermove = scrubAt; });
  scrub.addEventListener('pointerup', () => { scrub.onpointermove = null; });
  scrub.addEventListener('keydown', e => { if (e.key === 'ArrowLeft') S.scrub = Math.min(6, S.scrub + 1); else if (e.key === 'ArrowRight') S.scrub = Math.max(0, S.scrub - 1); else return; e.preventDefault(); total(); });

  /* ---------- 2) الحملات النشطة (بطاقة التمييز) ---------- */
  const CH = [['متجر الجوال', 40, 'solid'], ['الموقع', 30, 'dots'], ['الشركاء', 18, 'hatch'], ['التواصل', 12, 'line']];
  function camps() {
    count($('#skCamp'), Math.round(24 * (S.period === 'month' ? 1.6 : S.period === 'day' ? .5 : 1)), v => nf(v));
    $('#skShapes').innerHTML = CH.slice(0, 3).map(([n, v, cls], i) => `<button type="button" class="sk-shape ${cls}${S.chan === i ? ' on' : ''}" style="--h:${40 + v * 1.4}%" data-chan="${i}" aria-pressed="${S.chan === i}" data-tip="${n}|${v}% من الحملات"><b class="num">${v}%</b></button>`).join('');
    $('#skChanLab').textContent = `${CH[S.chan][0]} · ${CH[S.chan][1]}% من الحملات`;
  }

  /* ---------- 3) التفاعل الأسبوعي (حلقة) ---------- */
  function engage() {
    const app = S.period === 'day' ? 64 : S.period === 'month' ? 71 : 68, R = 34, L = 2 * Math.PI * R;
    $('#skDonut').innerHTML = `<svg viewBox="0 0 100 100" role="img" aria-label="تطبيق الجوال ${app}% والموقع ${100 - app}%"><circle cx="50" cy="50" r="${R}" class="trk"/><circle cx="50" cy="50" r="${R}" class="a" stroke-dasharray="${(app / 100 * L - 3).toFixed(1)} ${L}" transform="rotate(-90 50 50)" data-tip="تطبيق الجوال|${app}%"/><circle cx="50" cy="50" r="${R}" class="b" stroke-dasharray="${((100 - app) / 100 * L - 3).toFixed(1)} ${L}" stroke-dashoffset="${(-app / 100 * L).toFixed(1)}" transform="rotate(-90 50 50)" data-tip="الموقع|${100 - app}%"/></svg><span class="num">${app}%</span>`;
    $('#skEngLeg').innerHTML = `<li><i class="a"></i>تطبيق الجوال <b class="num">${app}%</b></li><li><i class="b"></i>الموقع <b class="num">${100 - app}%</b></li>`;
  }

  /* ---------- 4) قائمة الحملات + منحنى الوصول ---------- */
  function campaigns() {
    $('#skCampList').innerHTML = CAMPAIGNS.map((c, i) => `<div class="sk-acc${S.camp === i ? ' open' : ''}">
      <button type="button" class="sk-acch" data-camp="${i}" aria-expanded="${S.camp === i}"><span>${c.n}</span>${ic('chev-down')}</button>
      ${S.camp === i ? `<div class="sk-accb"><div class="sk-curve" data-c="${i}"></div></div>` : ''}</div>`).join('');
    const box = $('.sk-curve'); if (!box) return;
    const c = CAMPAIGNS[S.camp], v = c.reach.map(x => x * (S.period === 'month' ? 4 : S.period === 'day' ? .2 : 1)), W = 260, H = 170, mx = Math.max(...v) * 1.15;
    const pt = (i) => [W - 14 - i * (W - 70) / 6, H - 16 - v[i] / mx * (H - 34)];
    const d = v.map((_, i) => { const [x, y] = pt(i); if (!i) return `M${x},${y}`; const [px, py] = pt(i - 1); const cx = (px + x) / 2; return `C${cx},${py} ${cx},${y} ${x},${y}`; }).join(' ');
    box.innerHTML = `<svg viewBox="0 0 ${W} ${H}" preserveAspectRatio="none"><path d="${d}" class="ln"/><path d="${d} L${pt(6)[0]},${H} L${pt(0)[0]},${H}Z" class="ar"/><line class="cx" y1="8" y2="${H - 10}"/><circle class="dot" r="5"/></svg>
      <div class="sk-ylab">${['اليوم 1', 'اليوم 2', 'اليوم 3', 'اليوم 4', 'اليوم 5'].map(x => `<span>${x}</span>`).join('')}</div><div class="sk-ctip"><b class="num"></b><small>شخص</small></div><span class="sk-cdate">${c.date}</span>`;
    const svg = $('svg', box), set = i => { const [x, y] = pt(i), r = svg.getBoundingClientRect(); $('.cx', box).setAttribute('x1', x); $('.cx', box).setAttribute('x2', x); $('.dot', box).setAttribute('cx', x); $('.dot', box).setAttribute('cy', y); const tp = $('.sk-ctip', box); tp.style.left = (x / W * r.width) + 'px'; tp.style.top = (y / H * r.height - 34) + 'px'; $('b', tp).textContent = nf(v[i]); };
    set(4);
    box.addEventListener('pointermove', e => { const r = svg.getBoundingClientRect(), x = (r.right - e.clientX) / r.width * W; set(Math.max(0, Math.min(6, Math.round((x - 14) / ((W - 70) / 6))))); });
    box.addEventListener('pointerleave', () => set(4));
  }

  /* ---------- 5) اتجاه المبيعات: أعمدة الأيام ---------- */
  function trend() {
    const s = series('sales'), t = series('target'), mx = Math.max(...t) * 1.08;
    $('#skBars').innerHTML = DAYS.map((d, i) => `<button type="button" class="sk-col${S.day === i ? ' on' : ''}" data-day="${i}" aria-pressed="${S.day === i}" aria-label="${d}: ${nf(s[i])} من ${nf(t[i])} ر.س">
      ${S.day === i ? `<span class="sk-flag"><span class="num">${nf(s[i])}</span> ر.س</span>` : ''}
      <span class="cap" style="--t:${(t[i] / mx * 100).toFixed(1)}%"><i class="rest"></i><i class="done" style="height:${(s[i] / t[i] * 100).toFixed(1)}%"></i></span><small>${d}</small></button>`).join('');
    const sel = s[S.day], tg = t[S.day], g = Math.round((sel / s[(S.day + 6) % 7] - 1) * 100);
    big($('#skTrendV'), sel); big($('#skTrendT'), tg);
    $('#skGrowth').innerHTML = `${ic(g >= 0 ? 'up' : 'down')}<span class="num">${g >= 0 ? '+' : ''}${g}%</span> عن ${DAYS[(S.day + 6) % 7]}`; $('#skGrowth').className = 'sk-grow ' + (g >= 0 ? 'up' : 'down');
    $('#skTrendDay').textContent = `${DAYS[S.day]} · تحقيق ${Math.round(sel / tg * 100)}% من المستهدف`;
  }

  /* ---------- 6) أداء المنتجات ---------- */
  function products() {
    const L = [PRODUCTS[S.prod % PRODUCTS.length], PRODUCTS[(S.prod + 1) % PRODUCTS.length]];
    $('#skProds').innerHTML = L.map(p => `<article class="sk-prod"><span class="nm">${esc(p.n)}</span><span class="im">${GLYPH[p.ic]}</span><span class="mt"><b class="num">${nf(Math.round(p.units * PERIOD[S.period].mul))}</b> وحدة · <em class="${p.d[0] === '-' ? 'dn' : 'up'} num">${p.d}</em></span></article>`).join('');
  }

  /* ---------- الأحداث ---------- */
  root.addEventListener('click', e => {
    const t = e.target;
    const p = t.closest('[data-period]'); if (p) { S.period = p.dataset.period; $$('[data-period]').forEach(b => b.setAttribute('aria-pressed', String(b === p))); return all(); }
    const ch = t.closest('[data-chan]'); if (ch) { S.chan = +ch.dataset.chan; return camps(); }
    const cp = t.closest('[data-camp]'); if (cp) { S.camp = S.camp === +cp.dataset.camp ? -1 : +cp.dataset.camp; return campaigns(); }
    const dy = t.closest('[data-day]'); if (dy) { S.day = +dy.dataset.day; return trend(); }
    const pr = t.closest('[data-prod]'); if (pr) { S.prod = (S.prod + (+pr.dataset.prod) + PRODUCTS.length) % PRODUCTS.length; return products(); }
    const tb = t.closest('[data-tab]'); if (tb) { S.tab = tb.dataset.tab; $$('[data-tab]').forEach(b => b.setAttribute('aria-selected', String(b === tb))); $$('[data-area]').forEach(c => c.classList.toggle('dim', S.tab !== 'all' && !c.dataset.area.split(' ').includes(S.tab))); return; }
    const nv = t.closest('[data-nav]'); if (nv) { $$('[data-nav]').forEach(b => b.setAttribute('aria-current', String(b === nv))); if (nv.dataset.nav !== 'ov') toast(`قسم «${nv.textContent.trim()}» يُبنى بنفس الأسلوب عند ربط بياناته.`); return; }
    const ac = t.closest('[data-accent]'); if (ac) { root.dataset.accent = ac.dataset.accent; $$('[data-accent]').forEach(b => b.setAttribute('aria-pressed', String(b === ac))); return; }
    const ex = t.closest('[data-expand]'); if (ex) { const c = ex.closest('.sk-card'); c.classList.toggle('big'); ex.setAttribute('aria-expanded', String(c.classList.contains('big'))); if (c.querySelector('.sk-curve')) campaigns(); return; }
    const al = t.closest('[data-alert]'); if (al) { al.classList.toggle('on'); al.setAttribute('aria-pressed', String(al.classList.contains('on'))); toast(al.classList.contains('on') ? 'سيصلك تنبيه عند تغيّر هذا المؤشر بأكثر من 10%.' : 'أُلغي التنبيه.'); return; }
  });
  const toast = msg => { let t = document.querySelector('.toast'); if (!t) { t = document.createElement('div'); t.className = 'toast'; t.setAttribute('role', 'status'); document.body.append(t); } t.innerHTML = `${ic('check')}<span>${esc(msg)}</span>`; requestAnimationFrame(() => t.classList.add('is-on')); clearTimeout(t._h); t._h = setTimeout(() => t.classList.remove('is-on'), 2600); };
  function all() { total(); camps(); engage(); campaigns(); trend(); products(); }
  addEventListener('resize', () => campaigns());
  $('.sk-days').innerHTML = DAYS.map(d => `<span>${d.replace('ال', '').slice(0, 3)}</span>`).join('');
  all();
})();
