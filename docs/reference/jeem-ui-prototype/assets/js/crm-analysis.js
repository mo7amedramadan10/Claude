/* ==========================================================
   جيم — تحليل مسار المبيعات (لوحة تفاعلية)
   مستوحاة من «Power BI CRM Dashboard» (ZoomCharts): تصفية متقاطعة بنمط Power BI —
   كل عنصر يصفّي الباقي ويُبرز اختياره، مع شهري/أسبوعي وصيغ تسمية القمع.
   ========================================================== */
(function () {
  const root = document.getElementById('caRoot'); if (!root || !window.DK) return;
  const K = DK, $ = s => root.querySelector(s), r = K.rng('crm-analysis');
  const MONTHS = ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو'];
  const IND = ['النقل والخدمات اللوجستية', 'البنوك والتمويل', 'تقنية المعلومات', 'الجهات الحكومية', 'الخدمات المهنية', 'التعليم', 'الزراعة والتعدين', 'التجزئة والتوزيع', 'الاتصالات', 'التصنيع', 'الإعلام والترفيه', 'الإنشاءات والهندسة'];
  const SIZE = ['متوسطة (201–500)', 'صغيرة (11–200)', 'متناهية (1–10)', 'كبرى (1001+)', 'كبيرة (501–1000)'];
  const AG = ['ليلى السبيعي', 'جاسم المطيري', 'مشعل الحربي', 'كوثر العنزي', 'عمر الزهراني', 'هند الشهري', 'سعد القرني', 'دانة الرشيد'];
  const CITY = [['الرياض', 46.7, 24.7], ['جدة', 39.2, 21.5], ['الدمام', 50.1, 26.4], ['المدينة المنورة', 39.6, 24.5], ['أبها', 42.5, 18.2], ['تبوك', 36.6, 28.4], ['القصيم', 43.9, 26.3], ['حائل', 41.7, 27.5]];
  const STG = ['عملاء محتملون', 'مؤهلون', 'عرض مقدّم', 'تفاوض', 'صفقة مغلقة'];
  const pick = w => { let t = r() * w.reduce((a, b) => a + b, 0); for (let i = 0; i < w.length; i++) if ((t -= w[i]) < 0) return i; return w.length - 1; };
  /* 3000 عميل محتمل: الشهر، الأسبوع، القطاع، المدينة، الحجم، المندوب، أبعد مرحلة، القيمة، أيام الإغلاق، فُقد؟ */
  const L = Array.from({ length: 3000 }, () => {
    const m = pick([1.2, 1.1, 1.3, .9, .8]), d = Math.floor(r() * 28), st = pick([5, 2.2, 1.3, .9, 1.1]);
    return { m, w: m * 4 + Math.floor(d / 7), ind: pick([6, 5, 4.5, 3.5, 3, 2.2, 2, 2, 2, 1.8, 1.4, 1.2]), city: pick([5, 3.5, 2.5, 1.5, 1, .8, 1.2, .7]), size: pick([5, 4.4, 3, 1, .8]), ag: pick([3.2, 2.2, 2.1, 1.3, 1.3, 1, 1, .8]), st, v: Math.round(1200 + r() * 5200 + (st === 4 ? r() * 2000 : 0)), days: Math.round(20 + r() * 80), ch: st === 4 && r() < .5 && m !== 4 ? 1 : 0 };
  });
  /* ضعف مقصود في مايو ليظهر التنبيه */
  L.forEach(l => { if (l.m === 4 && l.st === 4 && r() < .35) l.st = 3; });
  const WEEKS = Array.from({ length: 20 }, (_, i) => `${1 + (i % 4) * 7} ${MONTHS[Math.floor(i / 4)]}`);
  const S = { f: {}, gran: 'm', fmt: 'first' };
  const NAMES = { m: v => MONTHS[v], w: v => 'أسبوع ' + WEEKS[v], ind: v => IND[v], city: v => CITY[v][0], size: v => SIZE[v], ag: v => AG[v], st: v => 'مرحلة: ' + STG[v] };
  const pass = (l, skip) => Object.entries(S.f).every(([k, v]) => k === skip || (k === 'st' ? l.st >= v : l[k] === v));
  const set = skip => L.filter(l => pass(l, skip));
  const closed = a => a.filter(l => l.st === 4);
  const toggle = (k, v) => { if (S.f[k] === v) delete S.f[k]; else { S.f[k] = v; if (k === 'm') delete S.f.w; if (k === 'w') delete S.f.m; } all(); };

  function kpis() {
    const a = set(), c = closed(a), val = c.reduce((x, l) => x + l.v, 0), ch = c.filter(l => l.ch).length;
    $('#caKpis').innerHTML = [['قيمة الصفقات المغلقة', K.money(val)], ['الصفقات المغلقة', K.num(c.length, '', 0)], ['العملاء المحتملون', K.num(a.length, '', 0)], ['نسبة التحويل', K.pct(a.length ? c.length / a.length * 100 : 0)], ['متوسط قيمة الصفقة', K.money(c.length ? val / c.length : 0)], ['عملاء فُقدوا', K.num(ch, '', 0)], ['متوسط أيام الإغلاق', K.num(c.length ? c.reduce((x, l) => x + l.days, 0) / c.length : 0, 'يوم', 0)]]
      .map(([n, v], i) => `<div class="dk-kpi" style="--i:${i}"><b>${v}</b><small>${n}</small></div>`).join('');
    K.count($('#caKpis').parentElement);
  }
  function valChart() {
    const tk = S.gran, c = closed(set(tk)), n = tk === 'm' ? 5 : 20, labs = tk === 'm' ? MONTHS : WEEKS;
    const vals = Array.from({ length: n }, (_, i) => c.filter(l => (tk === 'm' ? l.m : l.w) === i).reduce((x, l) => x + l.v, 0));
    const cnt = Array.from({ length: n }, (_, i) => c.filter(l => (tk === 'm' ? l.m : l.w) === i).length);
    $('#caVal').innerHTML = K.bars({ W: K.wOf($('#caVal')), labels: labs, series: [{ name: 'القيمة المغلقة', color: K.PAL[0], vals }], key: tk, sel: S.f[tk] ?? null, H: 200, tipf: (v, i) => `${K.txt(v, 'ر.س')} · ${cnt[i]} صفقة · متوسط ${K.txt(cnt[i] ? v / cnt[i] : 0, 'ر.س')}`, aria: 'قيمة الصفقات المغلقة' });
    const last = vals[n - 1], prev = vals.slice(0, -1), avg = prev.reduce((a, b) => a + b, 0) / (prev.length || 1);
    $('#caValNote').innerHTML = last < avg * .85 ? `<span class="dk-chip bad">${K.ic('down')}تنبيه</span> آخر ${tk === 'm' ? 'شهر' : 'أسبوع'} أقل من المتوسط بنسبة <b class="num">${K.nf((1 - last / avg) * 100)}%</b>` : `<span class="dk-chip ok">${K.ic('check')}ضمن المعدل</span>`;
  }
  function funnelC() {
    const a = set('st'), vals = STG.map((_, i) => a.filter(l => l.st >= i).length);
    $('#caFn').innerHTML = K.funnel(STG.map((n, i) => ({ label: n, val: vals[i] })), S.fmt, { sel: S.f.st ?? null });
    $('#caFnNote').innerHTML = `من <b class="num">${K.nf(vals[0])}</b> عميل محتمل وصل <b class="num">${K.nf(vals[4])}</b> للإغلاق (<span class="num">${K.nf(vals[4] / (vals[0] || 1) * 100, 1)}%</span>). أكبر فاقد بين «${STG[0]}» و«${STG[1]}».`;
  }
  function churnC() {
    const c = closed(set('m')), keep = MONTHS.map((_, i) => c.filter(l => l.m === i).length), lost = MONTHS.map((_, i) => c.filter(l => l.m === i && l.ch).length);
    $('#caChurn').innerHTML = K.bars({ W: K.wOf($('#caChurn')), labels: MONTHS, series: [{ name: 'صفقات مغلقة', color: K.PAL[0], vals: keep }, { name: 'عملاء فُقدوا', color: K.PAL[1], vals: lost }], key: 'm', sel: S.f.m ?? null, H: 190, fmt: v => K.nf(v) + ' صفقة', aria: 'مغلقة مقابل مفقودة' });
    $('#caChurnLeg').innerHTML = K.legend([['صفقات مغلقة', K.PAL[0]], ['عملاء فُقدوا', K.PAL[1]]]);
  }
  function sizeC() { const c = closed(set('size')), v = SIZE.map((_, i) => c.filter(l => l.size === i).length); $('#caSize').innerHTML = K.bars({ W: K.wOf($('#caSize')), labels: SIZE.map(s => s.split(' ')[0]), series: [{ name: 'الصفقات', color: K.PAL[3], vals: v }], key: 'size', sel: S.f.size ?? null, H: 190, fmt: x => K.nf(x) + ' صفقة', aria: 'حسب الحجم' }); }
  function indC() { const c = closed(set('ind')), it = IND.map((n, i) => ({ label: n, val: c.filter(l => l.ind === i).length, key: i })).sort((a, b) => b.val - a.val); $('#caInd').innerHTML = K.hbars(it, { attr: 'ind', sel: S.f.ind ?? null, color: K.PAL[2], fmt: v => `<span class="num">${K.nf(v)}</span>`, u: 'صفقة' }); }
  function agC() { const c = closed(set('ag')), v = AG.map((_, i) => c.filter(l => l.ag === i).length); $('#caAgents').innerHTML = K.bars({ W: K.wOf($('#caAgents')), labels: AG.map(a => a.split(' ')[0]), series: [{ name: 'الصفقات', color: K.PAL[0], vals: v }], key: 'ag', sel: S.f.ag ?? null, H: 190, fmt: x => K.nf(x) + ' صفقة', aria: 'حسب المندوب' }); }
  function mapC() {
    const c = closed(set('city')), v = CITY.map((_, i) => c.filter(l => l.city === i).length), mx = Math.max(...v) || 1, W = K.wOf($('#caMap')), H = Math.min(320, Math.max(240, W * .55));
    const X = lon => 30 + (lon - 35) / 17 * (W - 60), Y = lat => H - 20 - (lat - 16) / 14 * (H - 40);
    $('#caMap').innerHTML = `<svg viewBox="0 0 ${W} ${H}" class="dk-svg ca-map" role="img" aria-label="الصفقات حسب المدينة"><rect class="bg" x="0" y="0" width="${W}" height="${H}" rx="14"/>${[18, 22, 26, 30].map(la => `<line class="gr" x1="0" x2="${W}" y1="${Y(la)}" y2="${Y(la)}"/>`).join('')}${[38, 42, 46, 50].map(lo => `<line class="gr" y1="0" y2="${H}" x1="${X(lo)}" x2="${X(lo)}"/>`).join('')}
      ${CITY.map(([n, lo, la], i) => { const rr = 10 + Math.sqrt(v[i] / mx) * 34, on = S.f.city === i, dim = S.f.city != null && !on; return `<g class="pt${dim ? ' dim' : ''}${on ? ' on' : ''}" data-city="${i}" tabindex="0" role="button" aria-label="${n}: ${v[i]} صفقة" style="--i:${i}"><circle cx="${X(lo)}" cy="${Y(la)}" r="${rr.toFixed(1)}" data-tip="${n}|${v[i]} صفقة"/><text class="cv" x="${X(lo)}" y="${Y(la) + 4}">${v[i]}</text><text class="cn" x="${X(lo)}" y="${Y(la) + rr + 13}">${n}</text></g>`; }).join('')}</svg>`;
  }
  function scope() {
    const ks = Object.keys(S.f);
    $('#caScope').innerHTML = ks.map(k => `<span class="dk-chip info">${K.esc(NAMES[k](S.f[k]))}<button type="button" data-rm="${k}" aria-label="إزالة التصفية">${K.ic('x')}</button></span>`).join('');
    $('#caReset').hidden = !ks.length;
  }
  function all() { kpis(); valChart(); funnelC(); churnC(); sizeC(); indC(); agC(); mapC(); scope(); }

  root.addEventListener('click', e => {
    const rm = e.target.closest('[data-rm]'); if (rm) { delete S.f[rm.dataset.rm]; return all(); }
    if (e.target.closest('#caReset')) { S.f = {}; return all(); }
    for (const k of ['m', 'w', 'size', 'ag', 'ind', 'city', 'st']) { const el = e.target.closest(`[data-${k}]`); if (el && !el.closest('[data-seg]')) return toggle(k, +el.dataset[k]); }
  });
  K.seg(root, 'gran', v => { S.gran = v; if (v === 'm') delete S.f.w; else delete S.f.m; all(); });
  K.seg(root, 'fmt', v => { S.fmt = v; funnelC(); });
  K.onResize(root, all);
  all();
})();
