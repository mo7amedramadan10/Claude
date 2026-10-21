/* ==========================================================
   جيم — مركز قيادة سلسلة الإمداد (لوحة تفاعلية بثلاث صفحات)
   مستوحاة من «Supplier Analytics Dashboard» (ZoomCharts / Power BI):
   مركز القيادة (اتجاه مؤشر بنزول سنة ← ربع ← شهر، حلقة OTIF بتفصيل، قنوات، أولوية، مستودعات)،
   رادار المخاطر (إيراد معرّض حسب البعد + مصفوفة خسائر)، أثر الربحية (شلال الكفاءة).
   ========================================================== */
(function () {
  const root = document.getElementById('scRoot'); if (!root || !window.DK) return;
  const K = DK, $ = s => root.querySelector(s);
  const S = { pg: 'cmd', ord: 'reg', met: 'rev', drill: [], otifCat: false, dim: 'ch' };
  const f = () => S.ord === 'reg' ? 1 : .34;
  const PG = { cmd: ['مركز قيادة سلسلة الإمداد', 'موثوقية التسليم وأداء الإيراد وأنماط الطلب عبر كل القنوات'], risk: ['رادار المخاطر', 'أين يتركز الإيراد المعرّض للخطر وما مصدره'], profit: ['التكلفة التشغيلية وأثر الربحية', 'تحويل إخفاقات سلسلة الإمداد إلى أثر مالي من خسارة الإيراد حتى انضغاط الهامش'] };
  const CH = ['التجزئة', 'المطاعم والفنادق', 'الموزعون', 'التجارة الإلكترونية'], WH = ['المستودع الغربي', 'المستودع الشرقي', 'مستودع الشمال'], CAT = ['المخبوزات', 'المشروبات', 'الألبان', 'المجمّدات', 'الوجبات الخفيفة'];

  function kpis() {
    const k = f();
    $('#scKpis').innerHTML = [['money', 'إجمالي الإيراد', K.money(528080 * k), `العام الماضي ${K.txt(267960 * k, 'ر.س')} · ${K.delta(97.1, 1)}`], ['chart', 'مجمل الربح', K.money(185310 * k), `الهامش <span class="num">35.09%</span> · ${K.delta(0.1, 1)}`], ['clock', 'التسليم الكامل في الوقت (OTIF)', K.pct(S.ord === 'reg' ? 92.53 : 89.1, 2), `المستهدف <span class="num">95%</span> · <span class="dk-chip bad">${K.ic('down')}دون المستهدف</span>`], ['truck', 'الطلبات المتأخرة', K.num(Math.round(142 * k), '', 0), `أيام الشحن <span class="num">7.51</span>`], ['layers', 'معدل نفاد المخزون', K.pct(S.ord === 'reg' ? 3.26 : 4.4, 2), `<span class="num">${Math.round(62 * k)}</span> طلبًا تأثر`]]
      .map(([ic, n, v, x], i) => `<div class="dk-kpi" style="--i:${i}"><span class="row"><small>${n}</small><span class="sp"></span><span class="ib">${K.ic(ic === 'money' ? 'wallet' : ic)}</span></span><b>${v}</b><span class="row">${x}</span></div>`).join('');
  }
  /* ---------- اتجاه المؤشر بنزول هرمي ---------- */
  const MET = { rev: ['الإيراد', v => K.txt(v, 'ر.س')], gp: ['الربح', v => K.txt(v, 'ر.س')], otif: ['OTIF', v => K.nf(v, 1) + '%'], late: ['المتأخرة', v => K.nf(v) + ' طلب'], so: ['نفاد المخزون', v => K.nf(v, 2) + '%'] };
  function series() {
    const lv = S.drill.length, base = { rev: 528080, gp: 185310, otif: 92.5, late: 142, so: 3.26 }[S.met], rate = ['otif', 'so'].includes(S.met);
    const labels = lv === 0 ? ['2024', '2025'] : lv === 1 ? ['الربع 1', 'الربع 2', 'الربع 3', 'الربع 4'] : ['الشهر 1', 'الشهر 2', 'الشهر 3'].map((m, i) => ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو', 'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر'][S.drill[1] * 3 + i]);
    const r = K.rng(S.met + S.drill.join('-') + S.ord);
    const vals = labels.map((_, i) => rate ? +(base * (.96 + r() * .08) - (lv === 0 && i === 0 ? 1.2 : 0)).toFixed(2) : Math.round(base * f() / (lv === 0 ? (i ? 1 : 1.97) : lv === 1 ? 4 : 12) * (.8 + r() * .4)));
    return { labels, vals };
  }
  function trend() {
    const { labels, vals } = series(), m = MET[S.met];
    const crumbs = ['السنوات', ...S.drill.map((d, i) => i === 0 ? ['2024', '2025'][d] : 'الربع ' + (d + 1))];
    return `<div class="sc2-crumb">${S.drill.length ? `<button type="button" class="dk-btn" data-up>${K.ic('chev-left', 'icon icon-sm flip')}رجوع</button>` : ''}<span>${crumbs.join(' ‹ ')}</span>${S.drill.length < 2 ? '<small class="mut">اضغط عمودًا للنزول</small>' : ''}</div>` +
      K.bars({ W: K.wOf($('#scTrend')) || 500, H: 240, labels, series: [{ name: m[0], color: '#3B2F8F', vals }], key: 'dr', fmt: m[1], target: S.met === 'otif' ? 95 : null, targetLabel: 'المستهدف 95%', aria: 'اتجاه ' + m[0] });
  }
  function otif() {
    if (!S.otifCat) { const ok = S.ord === 'reg' ? 1759 : 590, bad = S.ord === 'reg' ? 142 : 72;
      return K.donut([{ label: 'في الوقت', val: ok, color: '#3B2F8F', key: 'ok' }, { label: 'تأخر', val: bad, color: '#E8794A', key: 'bad' }], { size: 180, th: 22, attr: 'otif', center: `<b class="num">${K.nf(ok + bad)}</b><small>إجمالي الطلبات</small>`, tip: it => K.nf(it.val) + ' طلب' }) + `<div class="dk-leg"><span><i style="background:#3B2F8F"></i>في الوقت</span><button type="button" class="dk-lnk" data-otif="bad"><i style="background:#E8794A;width:10px;height:10px;border-radius:3px;display:inline-block"></i>تأخر — فصّل حسب الفئة</button></div>`; }
    const v = [38, 31, 27, 29, 17].map(x => Math.round(x * f()));
    return `<button type="button" class="dk-btn" data-otifback>${K.ic('chev-left', 'icon icon-sm flip')}رجوع</button><p class="mut">الطلبات المتأخرة حسب الفئة</p>` + K.hbars(CAT.map((c, i) => ({ label: c, val: v[i], key: i, color: '#E8794A' })).sort((a, b) => b.val - a.val), { fmt: x => `<span class="num">${x}</span>`, u: 'طلب' });
  }
  function page() {
    const b = $('#scBody'); const k = f();
    if (S.pg === 'cmd') {
      const chRev = [212, 140, 105, 71].map(v => v * 1000 * k), chGp = [70, 51, 40, 24].map(v => v * 1000 * k);
      b.innerHTML = `<div class="dk-grid">
        <section class="dk-card" style="--sp:5;--i:1"><header><h2>اتجاه المؤشرات</h2><span class="sp"></span><div class="dk-seg sm" data-seg="met" role="group" aria-label="المؤشر">${Object.entries(MET).map(([key, [n]]) => `<button type="button" data-v="${key}" aria-pressed="${S.met === key}">${n}</button>`).join('')}</div><p>سنة ← ربع ← شهر</p></header><div id="scTrend"></div></section>
        <section class="dk-card" style="--sp:3;--md:6;--i:2"><header><h2>OTIF حسب الطلب</h2><p>نوع التسليم مع تفصيل الفئة</p></header><div id="scOtif" class="sc2-otif"></div></section>
        <section class="dk-card" style="--sp:4;--md:6;--i:3"><header><h2>الإيراد ومجمل الربح حسب القناة</h2></header><div id="scCh"></div>${K.legend([['الإيراد', '#3B2F8F'], ['مجمل الربح', '#9C8CE6']])}</section>
        <section class="dk-card" style="--sp:6;--i:4"><header><h2>أولوية العملاء حسب OTIF</h2><p>الخط الأحمر = المستهدف 95%</p></header>${K.hbars([['استراتيجيون', 94.1], ['منخفضة', 92.6], ['عاديون', 91.8]].map(([n, v]) => ({ label: n, val: v - (S.ord === 'reg' ? 0 : 3), color: v >= 95 ? '#1E9E6A' : '#3B2F8F' })), { max: 100, target: 95, fmt: x => `<span class="num">${K.nf(x, 1)}%</span>`, u: '%' })}</section>
        <section class="dk-card" style="--sp:6;--i:5"><header><h2>المستودعات حسب OTIF</h2><p>الخط الأحمر = المستهدف 95%</p></header>${K.hbars(WH.map((n, i) => ({ label: n, val: [95.4, 82.6, 88.5][i] - (S.ord === 'reg' ? 0 : 2.5), color: [95.4, 82.6, 88.5][i] >= 95 ? '#1E9E6A' : [95.4, 82.6, 88.5][i] < 85 ? '#D64545' : '#3B2F8F' })), { max: 100, target: 95, fmt: x => `<span class="num">${K.nf(x, 1)}%</span>`, u: '%' })}</section></div>`;
      $('#scTrend').innerHTML = trend(); $('#scOtif').innerHTML = otif();
      $('#scCh').innerHTML = K.bars({ W: K.wOf($('#scCh')), H: 200, labels: CH, series: [{ name: 'الإيراد', color: '#3B2F8F', vals: chRev }, { name: 'مجمل الربح', color: '#9C8CE6', vals: chGp }], fmt: v => K.txt(v, 'ر.س'), aria: 'حسب القناة' });
      K.seg(b, 'met', v => { S.met = v; S.drill = []; $('#scTrend').innerHTML = trend(); });
    } else if (S.pg === 'risk') {
      const DIM = { ch: ['القناة', CH, [168, 120, 101, 74]], wh: ['المستودع', WH, [96, 210, 141]], cat: ['الفئة', CAT, [52, 88, 74, 131, 41]], sup: ['المورد', ['أطعمة القيمة', 'لوجستيات المجمّدات', 'ألبان الشمال', 'منتجات البلطيق', 'مشروبات ممتازة'], [148, 121, 77, 46, 32]] }[S.dim];
      const mx = [[3.1, 5.2, 4.4, 8.9, 2.0], [2.4, 3.9, 6.1, 7.2, 1.7], [1.8, 4.6, 3.2, 9.8, 2.6], [1.2, 2.2, 2.9, 5.1, 1.4]];
      b.innerHTML = `<div class="dk-grid"><section class="dk-card" style="--sp:5;--i:1"><header><h2>الإيراد المعرّض للخطر</h2><span class="sp"></span><div class="dk-seg sm" data-seg="dim" role="group" aria-label="البعد">${[['sup', 'المورد'], ['ch', 'القناة'], ['wh', 'المستودع'], ['cat', 'الفئة']].map(([k2, n]) => `<button type="button" data-v="${k2}" aria-pressed="${S.dim === k2}">${n}</button>`).join('')}</div><p>حسب ${DIM[0]} · مرتب تنازليًا</p></header>${K.hbars(DIM[1].map((n, i) => ({ label: n, val: DIM[2][i] * 1000 * k, color: '#3B2F8F' })).sort((a, c) => c.val - a.val), { fmt: x => K.money(x), u: 'ر.س' })}</section>
        <section class="dk-card" style="--sp:7;--i:2"><header><h2>مصفوفة الخسائر: القناة × الفئة</h2><p>نسبة الإيراد المفقود % · كلما غمق اللون زادت الخسارة</p></header>${K.heat(CH, CAT, mx.map(r => r.map(v => +(v * (S.ord === 'reg' ? 1 : 1.3)).toFixed(1))), { hue: '59,47,143', fmt: x => x + '%' })}
          <div class="dk-take">أعلى خسارة: <b>المجمّدات عبر الموزعين</b> — سلسلة التبريد هي نقطة الضعف الرئيسية.</div></section></div>`;
      K.seg(b, 'dim', v => { S.dim = v; page(); });
    } else {
      b.innerHTML = `<div class="dk-grid"><section class="dk-card" style="--sp:12;--i:1"><header><h2>تحليل الكفاءة</h2><p>من الإيراد إلى صافي الأثر: خسائر نفاد المخزون والمرتجعات وغرامات التأخير والهدر وتكلفة البضاعة</p></header><div id="scWf"></div></section>
        <section class="dk-card" style="--sp:5;--i:2"><header><h2>الهامش: في الوقت مقابل المتأخر</h2><p>نسبة مجمل الربح %</p></header><div id="scMg"></div></section>
        <section class="dk-card" style="--sp:7;--i:3"><header><h2>الإيراد المعرّض حسب القناة</h2></header>${K.hbars(CH.map((n, i) => ({ label: n, val: [42, 31, 18, 9][i] * 1000 * k, color: '#3B2F8F' })), { fmt: x => K.money(x), u: 'ر.س' })}<div class="dk-take">الطلبات المتأخرة تفقد قرابة <b>ثلثي الهامش</b>؛ تقليل التأخير في التجزئة وحده يعيد أكبر جزء من الأثر.</div></section></div>`;
      $('#scWf').innerHTML = K.waterfall([{ label: 'إجمالي الإيراد', val: 528080 * k, kind: 'total' }, { label: 'تعديل مجمل الربح', val: 167500 * k }, { label: 'خسارة نفاد المخزون', val: -15210 * k }, { label: 'خسارة المرتجعات', val: -937 * k }, { label: 'غرامة التأخير', val: -863 * k }, { label: 'تكلفة الهدر', val: -796 * k }, { label: 'تكلفة البضاعة', val: -342770 * k }, { label: 'الصافي', val: 335004 * k, kind: 'total' }], { W: K.wOf($('#scWf')), H: 250, fmt: v => K.txt(v, 'ر.س') });
      $('#scMg').innerHTML = K.bars({ W: K.wOf($('#scMg')), H: 200, labels: ['في الوقت', 'تسليم فاشل'], series: [{ name: 'هامش الربح', color: '#3B2F8F', vals: [35.4, 12.1] }], fmt: v => K.nf(v, 1) + '%', aria: 'الهامش' });
    }
  }
  function all() { const [t, s] = PG[S.pg]; $('#scTitle').textContent = t; $('#scSub').textContent = s; kpis(); const b = $('#scBody'); b.classList.remove('sl-l'); void b.offsetWidth; if (!K.RM) b.classList.add('sl-l'); page(); }
  root.addEventListener('click', e => {
    const t = e.target; let x;
    if ((x = t.closest('[data-pg]'))) { S.pg = x.dataset.pg; root.querySelectorAll('[data-pg]').forEach(b => b.setAttribute('aria-selected', String(b === x))); return all(); }
    if ((x = t.closest('[data-dr]'))) { if (S.drill.length < 2) { S.drill.push(+x.dataset.dr); $('#scTrend').innerHTML = trend(); } return; }
    if (t.closest('[data-up]')) { S.drill.pop(); $('#scTrend').innerHTML = trend(); return; }
    if ((x = t.closest('[data-otif]'))) { if (x.dataset.otif === 'bad') { S.otifCat = true; $('#scOtif').innerHTML = otif(); } return; }
    if (t.closest('[data-otifback]')) { S.otifCat = false; $('#scOtif').innerHTML = otif(); return; }
    if (t.closest('[data-reset]')) { Object.assign(S, { met: 'rev', drill: [], otifCat: false, dim: 'ch' }); return all(); }
  });
  K.seg(root, 'ord', v => { S.ord = v; all(); });
  K.onResize(root, page);
  all();
})();
