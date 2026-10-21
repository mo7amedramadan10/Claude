/* ==========================================================
   جيم — تحليل مسار المبيعات (لوحة تفاعلية، المحرك: analysis، على dash-kit)
   مُعاد بناؤه كدالة تركيب قابلة لإعادة الاستخدام (window.JEEM_ANALYSIS_MOUNT) بدل IIFE ذاتي
   التشغيل يولّد 3000 عميل محتمل اصطناعي بالكامل عبر توزيعات عشوائية مرجّحة — يقبل الآن
   data.leads حقيقية (صف واحد لكل عميل محتمل) من مصدر حقيقي أو مكتبة العرض التجريبي
   (crm-analysis-demo.js). كل أبعاد التصفية (الأشهر/الأسابيع/القطاع/المدينة/الحجم/المندوب)
   تُشتق من قيم data.leads الفعلية نفسها، لا من قوائم سعودية ثابتة — عدا إحداثيات المدن
   (بيانات مرجعية جغرافية موضوعية، لا بيانات مشروع) التي تبقى جدول بحث ثابت في المحرك؛ أي
   مدينة غير معروفة فيه تُستبعد من الخريطة وحدها دون التأثير على بقية الرسوم. التصفية
   المتقاطعة كلها حسابات حقيقية على البيانات المُمرَّرة، لا توليد عشوائي أو "ضعف مقصود".
   ========================================================== */
(function () {
  const ARABIC_MONTHS = ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو', 'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر'];
  const CITY_COORDS = {
    'الرياض': [46.7, 24.7], 'جدة': [39.2, 21.5], 'الدمام': [50.1, 26.4], 'المدينة المنورة': [39.6, 24.5],
    'أبها': [42.5, 18.2], 'تبوك': [36.6, 28.4], 'القصيم': [43.9, 26.3], 'حائل': [41.7, 27.5],
    'مكة المكرمة': [39.8, 21.4], 'الخبر': [50.2, 26.3], 'الطائف': [40.4, 21.3], 'نجران': [44.1, 17.5],
    'جازان': [42.6, 16.9], 'ينبع': [38.1, 24.1], 'عرعر': [41.0, 30.9], 'سكاكا': [40.2, 30.0],
  };

  function mount(root, data, opts = {}) {
    if (!root || !data || !window.DK) return;
    const K = DK, $ = s => root.querySelector(s);
    const STG = data.stages && data.stages.length ? data.stages : ['عملاء محتملون', 'مؤهلون', 'عرض مقدّم', 'تفاوض', 'صفقة مغلقة'];
    const RAW = data.leads || [];

    function dimIndex(getter, fallback) {
      const arr = []; const map = new Map();
      RAW.forEach(l => { const v = getter(l) || fallback; if (!map.has(v)) { map.set(v, arr.length); arr.push(v); } });
      return { arr, map };
    }
    const indDim = dimIndex(l => l.industry, 'غير محدد');
    const cityDim = dimIndex(l => l.city, 'غير محدد');
    const sizeDim = dimIndex(l => l.orgSize, 'غير محدد');
    const agDim = dimIndex(l => l.agent, 'غير محدد');
    const IND = indDim.arr, CITY = cityDim.arr, SIZE = sizeDim.arr, AG = agDim.arr;

    const monthKeyOf = d => d.getFullYear() * 12 + d.getMonth();
    const monthKeys = [...new Set(RAW.map(l => monthKeyOf(new Date(l.createdAt))))].sort((a, b) => a - b);
    const monthIdxMap = new Map(monthKeys.map((k, i) => [k, i]));
    const years = new Set(monthKeys.map(k => Math.floor(k / 12)));
    const MONTHS = monthKeys.map(k => ARABIC_MONTHS[k % 12] + (years.size > 1 ? ' ' + Math.floor(k / 12) : ''));
    const WEEKS = [];
    for (let m = 0; m < MONTHS.length; m++) for (let w = 0; w < 4; w++) WEEKS.push(`أسبوع ${w + 1} · ${MONTHS[m]}`);

    const L = RAW.map(l => {
      const cd = new Date(l.createdAt), m = monthIdxMap.get(monthKeyOf(cd)) ?? 0, day = cd.getDate() - 1;
      const w = m * 4 + Math.min(3, Math.floor(day / 7));
      const closedAt = l.closedAt ? new Date(l.closedAt) : null;
      const days = closedAt ? Math.max(0, Math.round((closedAt - cd) / 86400000)) : 0;
      return {
        m, w, ind: indDim.map.get(l.industry || 'غير محدد'), city: cityDim.map.get(l.city || 'غير محدد'),
        size: sizeDim.map.get(l.orgSize || 'غير محدد'), ag: agDim.map.get(l.agent || 'غير محدد'),
        st: l.stageIndex ?? 0, v: l.value || 0, days, ch: !!l.churned,
      };
    });

    const S = { f: {}, gran: 'm', fmt: 'first' };
    const NAMES = { m: v => MONTHS[v], w: v => 'أسبوع ' + WEEKS[v], ind: v => IND[v], city: v => CITY[v], size: v => SIZE[v], ag: v => AG[v], st: v => 'مرحلة: ' + STG[v] };
    const pass = (l, skip) => Object.entries(S.f).every(([k, v]) => k === skip || (k === 'st' ? l.st >= v : l[k] === v));
    const set = skip => L.filter(l => pass(l, skip));
    const closed = a => a.filter(l => l.st === STG.length - 1);
    const toggle = (k, v) => { if (S.f[k] === v) delete S.f[k]; else { S.f[k] = v; if (k === 'm') delete S.f.w; if (k === 'w') delete S.f.m; } all(); };

    function kpis() {
      const a = set(), c = closed(a), val = c.reduce((x, l) => x + l.v, 0), ch = c.filter(l => l.ch).length;
      $('#caKpis').innerHTML = [['قيمة الصفقات المغلقة', K.money(val)], ['الصفقات المغلقة', K.num(c.length, '', 0)], ['العملاء المحتملون', K.num(a.length, '', 0)], ['نسبة التحويل', K.pct(a.length ? c.length / a.length * 100 : 0)], ['متوسط قيمة الصفقة', K.money(c.length ? val / c.length : 0)], ['عملاء فُقدوا', K.num(ch, '', 0)], ['متوسط أيام الإغلاق', K.num(c.length ? c.reduce((x, l) => x + l.days, 0) / c.length : 0, 'يوم', 0)]]
        .map(([n, v], i) => `<div class="dk-kpi" style="--i:${i}"><b>${v}</b><small>${n}</small></div>`).join('');
      K.count($('#caKpis').parentElement);
    }
    function valChart() {
      const tk = S.gran, c = closed(set(tk)), n = tk === 'm' ? MONTHS.length : WEEKS.length, labs = tk === 'm' ? MONTHS : WEEKS;
      const vals = Array.from({ length: n }, (_, i) => c.filter(l => (tk === 'm' ? l.m : l.w) === i).reduce((x, l) => x + l.v, 0));
      const cnt = Array.from({ length: n }, (_, i) => c.filter(l => (tk === 'm' ? l.m : l.w) === i).length);
      $('#caVal').innerHTML = n ? K.bars({ W: K.wOf($('#caVal')), labels: labs, series: [{ name: 'القيمة المغلقة', color: K.PAL[0], vals }], key: tk, sel: S.f[tk] ?? null, H: 200, tipf: (v, i) => `${K.txt(v, 'ر.س')} · ${cnt[i]} صفقة · متوسط ${K.txt(cnt[i] ? v / cnt[i] : 0, 'ر.س')}`, aria: 'قيمة الصفقات المغلقة' }) : '<p class="dk-empty">لا بيانات بعد.</p>';
      const last = vals[n - 1] ?? 0, prev = vals.slice(0, -1), avg = prev.reduce((a, b) => a + b, 0) / (prev.length || 1);
      $('#caValNote').innerHTML = (n > 1 && last < avg * .85) ? `<span class="dk-chip bad">${K.ic('down')}تنبيه</span> آخر ${tk === 'm' ? 'شهر' : 'أسبوع'} أقل من المتوسط بنسبة <b class="num">${K.nf((1 - last / avg) * 100)}%</b>` : `<span class="dk-chip ok">${K.ic('check')}ضمن المعدل</span>`;
    }
    function funnelC() {
      const a = set('st'), vals = STG.map((_, i) => a.filter(l => l.st >= i).length);
      $('#caFn').innerHTML = K.funnel(STG.map((n, i) => ({ label: n, val: vals[i] })), S.fmt, { sel: S.f.st ?? null });
      const biggestDrop = vals.slice(1).reduce((worst, v, i) => (vals[i] - v) > (vals[worst] - vals[worst + 1]) ? i : worst, 0);
      $('#caFnNote').innerHTML = `من <b class="num">${K.nf(vals[0] || 0)}</b> عميل محتمل وصل <b class="num">${K.nf(vals[vals.length - 1] || 0)}</b> للإغلاق (<span class="num">${K.nf(vals[0] ? vals[vals.length - 1] / vals[0] * 100 : 0, 1)}%</span>). أكبر فاقد بين «${K.esc(STG[biggestDrop] || '')}» و«${K.esc(STG[biggestDrop + 1] || '')}».`;
    }
    function churnC() {
      const c = closed(set('m')), keep = MONTHS.map((_, i) => c.filter(l => l.m === i).length), lost = MONTHS.map((_, i) => c.filter(l => l.m === i && l.ch).length);
      $('#caChurn').innerHTML = MONTHS.length ? K.bars({ W: K.wOf($('#caChurn')), labels: MONTHS, series: [{ name: 'صفقات مغلقة', color: K.PAL[0], vals: keep }, { name: 'عملاء فُقدوا', color: K.PAL[1], vals: lost }], key: 'm', sel: S.f.m ?? null, H: 190, fmt: v => K.nf(v) + ' صفقة', aria: 'مغلقة مقابل مفقودة' }) : '';
      $('#caChurnLeg').innerHTML = K.legend([['صفقات مغلقة', K.PAL[0]], ['عملاء فُقدوا', K.PAL[1]]]);
    }
    function sizeC() { const c = closed(set('size')), v = SIZE.map((_, i) => c.filter(l => l.size === i).length); $('#caSize').innerHTML = SIZE.length ? K.bars({ W: K.wOf($('#caSize')), labels: SIZE.map(s => s.split(' ')[0]), series: [{ name: 'الصفقات', color: K.PAL[3], vals: v }], key: 'size', sel: S.f.size ?? null, H: 190, fmt: x => K.nf(x) + ' صفقة', aria: 'حسب الحجم' }) : ''; }
    function indC() { const c = closed(set('ind')), it = IND.map((n, i) => ({ label: n, val: c.filter(l => l.ind === i).length, key: i })).sort((a, b) => b.val - a.val); $('#caInd').innerHTML = K.hbars(it, { attr: 'ind', sel: S.f.ind ?? null, color: K.PAL[2], fmt: v => `<span class="num">${K.nf(v)}</span>`, u: 'صفقة' }); }
    function agC() { const c = closed(set('ag')), v = AG.map((_, i) => c.filter(l => l.ag === i).length); $('#caAgents').innerHTML = AG.length ? K.bars({ W: K.wOf($('#caAgents')), labels: AG.map(a => a.split(' ')[0]), series: [{ name: 'الصفقات', color: K.PAL[0], vals: v }], key: 'ag', sel: S.f.ag ?? null, H: 190, fmt: x => K.nf(x) + ' صفقة', aria: 'حسب المندوب' }) : ''; }
    function mapC() {
      const known = CITY.map((n, i) => ({ n, i, coords: CITY_COORDS[n] })).filter(c => c.coords);
      if (!known.length) { $('#caMap').innerHTML = '<p class="dk-empty">لا مدن معروفة الإحداثيات بعد.</p>'; return; }
      const base = closed(set('city')), v = known.map(c2 => base.filter(l => l.city === c2.i).length), mx = Math.max(...v) || 1;
      const W = K.wOf($('#caMap')), H = Math.min(320, Math.max(240, W * .55));
      const X = lon => 30 + (lon - 35) / 17 * (W - 60), Y = lat => H - 20 - (lat - 16) / 14 * (H - 40);
      $('#caMap').innerHTML = `<svg viewBox="0 0 ${W} ${H}" class="dk-svg ca-map" role="img" aria-label="الصفقات حسب المدينة"><rect class="bg" x="0" y="0" width="${W}" height="${H}" rx="14"/>${[18, 22, 26, 30].map(la => `<line class="gr" x1="0" x2="${W}" y1="${Y(la)}" y2="${Y(la)}"/>`).join('')}${[38, 42, 46, 50].map(lo => `<line class="gr" y1="0" y2="${H}" x1="${X(lo)}" x2="${X(lo)}"/>`).join('')}
        ${known.map((c2, k) => { const [lo, la] = c2.coords, rr = 10 + Math.sqrt(v[k] / mx) * 34, on = S.f.city === c2.i, dim = S.f.city != null && !on; return `<g class="pt${dim ? ' dim' : ''}${on ? ' on' : ''}" data-city="${c2.i}" tabindex="0" role="button" aria-label="${K.esc(c2.n)}: ${v[k]} صفقة" style="--i:${k}"><circle cx="${X(lo)}" cy="${Y(la)}" r="${rr.toFixed(1)}" data-tip="${K.esc(c2.n)}|${v[k]} صفقة"/><text class="cv" x="${X(lo)}" y="${Y(la) + 4}">${v[k]}</text><text class="cn" x="${X(lo)}" y="${Y(la) + rr + 13}">${K.esc(c2.n)}</text></g>`; }).join('')}</svg>`;
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
    return { destroy() {} };
  }

  window.JEEM_ANALYSIS_MOUNT = mount;
})();
