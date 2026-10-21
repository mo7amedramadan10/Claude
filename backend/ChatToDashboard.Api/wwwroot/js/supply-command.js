/* ==========================================================
   جيم — مركز قيادة سلسلة الإمداد (لوحة تفاعلية بثلاث صفحات، المحرك: command، على dash-kit)
   دالة تركيب قابلة لإعادة الاستخدام (window.JEEM_COMMAND_MOUNT). عقد البيانات المتوقَّع:
   data.years: سنتان على الأقل (['2024','2025']) · data.channels/warehouses/categories/
   suppliers/customerPriorities: أبعاد حقيقية من مشروعك (لا قوائم سعودية ثابتة).
   كل مقياس مُفصَّل حسب نوع الطلب reg (اعتيادي) / promo (ترويجي) لأن هذا تصنيف حقيقي
   للطلب نفسه، لا بُعد مُولَّد. اتجاه المؤشر الهرمي (سنة ← ربع ← شهر) يُشتق كله من
   data.trend[ord][metric].months (قيمة شهرية واحدة لكل شهر عبر كل السنوات) بدل تخزين
   مجاميع سنة/ربع جاهزة، لضمان تطابقها حسابيًا دائمًا مع الشهور (جمع للمقاييس التراكمية،
   ومتوسط لمقاييس النسب كـ OTIF ونفاد المخزون). نص خلاصة مصفوفة الخسائر ونص أثر الهامش في
   صفحة الربحية لم يعودا جملتين ثابتتين (كانتا تذكران فئة/قناة بعينها دائمًا) بل تُحتسبان من
   أعلى خلية فعلية في المصفوفة ومن نسبة انضغاط الهامش الفعلية.
   ========================================================== */
(function () {
  const ARABIC_MONTHS = ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو', 'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر'];
  const QUARTERS = ['الربع 1', 'الربع 2', 'الربع 3', 'الربع 4'];
  const RATE_METRICS = ['otif', 'so'];
  const WF_STEPS = [
    ['إجمالي الإيراد', 'total'], ['تعديل مجمل الربح', null], ['خسارة نفاد المخزون', null],
    ['خسارة المرتجعات', null], ['غرامة التأخير', null], ['تكلفة الهدر', null],
    ['تكلفة البضاعة', null], ['الصافي', 'total'],
  ];

  function mount(root, data, opts = {}) {
    if (!root || !data || !window.DK) return;
    const K = DK, $ = s => root.querySelector(s);
    const CH = data.channels && data.channels.length ? data.channels : ['القناة 1', 'القناة 2'];
    const WH = data.warehouses && data.warehouses.length ? data.warehouses : ['المستودع 1'];
    const CAT = data.categories && data.categories.length ? data.categories : ['الفئة 1'];
    const SUP = data.suppliers && data.suppliers.length ? data.suppliers : ['المورد 1'];
    const PRI = data.customerPriorities && data.customerPriorities.length ? data.customerPriorities : ['عاديون'];
    const YEARS = data.years && data.years.length >= 2 ? data.years.slice(0, 2) : ['2024', '2025'];
    const S = { pg: 'cmd', ord: 'reg', met: 'rev', drill: [], otifCat: false, dim: 'ch' };
    const PG = {
      cmd: ['مركز قيادة سلسلة الإمداد', 'موثوقية التسليم وأداء الإيراد وأنماط الطلب عبر كل القنوات'],
      risk: ['رادار المخاطر', 'أين يتركز الإيراد المعرّض للخطر وما مصدره'],
      profit: ['التكلفة التشغيلية وأثر الربحية', 'تحويل إخفاقات سلسلة الإمداد إلى أثر مالي من خسارة الإيراد حتى انضغاط الهامش'],
    };
    const MET = {
      rev: ['الإيراد', v => K.txt(v, 'ر.س')], gp: ['الربح', v => K.txt(v, 'ر.س')],
      otif: ['OTIF', v => K.nf(v, 1) + '%'], late: ['المتأخرة', v => K.nf(v) + ' طلب'],
      so: ['نفاد المخزون', v => K.nf(v, 2) + '%'],
    };

    function kpis() {
      const k = (data.kpis && data.kpis[S.ord]) || {};
      const otifOk = (k.otifPct ?? 0) >= (k.otifTargetPct ?? 95);
      $('#scKpis').innerHTML = [
        ['wallet', 'إجمالي الإيراد', K.money(k.revenue ?? 0), `العام الماضي ${K.txt(k.revenuePrevYear ?? 0, 'ر.س')} · ${K.delta(k.revenueDeltaPct ?? 0, 1)}`],
        ['chart', 'مجمل الربح', K.money(k.grossProfit ?? 0), `الهامش <span class="num">${K.nf(k.grossProfitMarginPct ?? 0, 2)}%</span> · ${K.delta(k.grossProfitDeltaPct ?? 0, 1)}`],
        ['clock', 'التسليم الكامل في الوقت (OTIF)', K.pct(k.otifPct ?? 0, 2), `المستهدف <span class="num">${K.nf(k.otifTargetPct ?? 95, 0)}%</span> · <span class="dk-chip ${otifOk ? 'ok' : 'bad'}">${K.ic(otifOk ? 'up' : 'down')}${otifOk ? 'ضمن المستهدف' : 'دون المستهدف'}</span>`],
        ['list', 'الطلبات المتأخرة', K.num(Math.round(k.lateOrders ?? 0), '', 0), `أيام الشحن <span class="num">${K.nf(k.avgShipDays ?? 0, 2)}</span>`],
        ['layers', 'معدل نفاد المخزون', K.pct(k.stockoutPct ?? 0, 2), `<span class="num">${K.nf(k.stockoutOrdersAffected ?? 0)}</span> طلبًا تأثر`],
      ].map(([ic, n, v, x], i) => `<div class="dk-kpi" style="--i:${i}"><span class="row"><small>${n}</small><span class="sp"></span><span class="ib">${K.ic(ic)}</span></span><b>${v}</b><span class="row">${x}</span></div>`).join('');
    }

    /* ---------- اتجاه المؤشر بنزول هرمي: سنة ← ربع ← شهر، من سلسلة شهرية واحدة ---------- */
    function monthsOf(metric) { return ((data.trend && data.trend[S.ord] && data.trend[S.ord][metric]) || {}).months || []; }
    function rollup(vals, metric) {
      if (!vals.length) return 0;
      const sum = vals.reduce((a, b) => a + b, 0);
      return RATE_METRICS.includes(metric) ? sum / vals.length : sum;
    }
    function series() {
      const months = monthsOf(S.met), lv = S.drill.length;
      let labels, vals;
      if (lv === 0) {
        labels = YEARS;
        vals = YEARS.map((_, yi) => rollup(months.slice(yi * 12, yi * 12 + 12), S.met));
      } else if (lv === 1) {
        const yi = S.drill[0];
        labels = QUARTERS;
        vals = QUARTERS.map((_, qi) => rollup(months.slice(yi * 12 + qi * 3, yi * 12 + qi * 3 + 3), S.met));
      } else {
        const [yi, qi] = S.drill;
        labels = [0, 1, 2].map(i => ARABIC_MONTHS[(yi * 12 + qi * 3 + i) % 12]);
        vals = [0, 1, 2].map(i => months[yi * 12 + qi * 3 + i] ?? 0);
      }
      return { labels, vals };
    }
    function trend() {
      const { labels, vals } = series(), m = MET[S.met];
      const crumbs = ['السنوات', ...S.drill.map((d, i) => i === 0 ? YEARS[d] : QUARTERS[d])];
      return `<div class="sc2-crumb">${S.drill.length ? `<button type="button" class="dk-btn" data-up>${K.ic('chev-left', 'icon icon-sm flip')}رجوع</button>` : ''}<span>${crumbs.join(' ‹ ')}</span>${S.drill.length < 2 ? '<small class="mut">اضغط عمودًا للنزول</small>' : ''}</div>` +
        K.bars({ W: K.wOf($('#scTrend')) || 500, H: 240, labels, series: [{ name: m[0], color: '#3B2F8F', vals }], key: 'dr', fmt: m[1], target: S.met === 'otif' ? ((data.kpis && data.kpis[S.ord] && data.kpis[S.ord].otifTargetPct) ?? 95) : null, targetLabel: `المستهدف ${K.nf(((data.kpis && data.kpis[S.ord] && data.kpis[S.ord].otifTargetPct) ?? 95), 0)}%`, aria: 'اتجاه ' + m[0] });
    }
    function otif() {
      const o = (data.otif && data.otif[S.ord]) || { onTime: 0, late: 0 };
      if (!S.otifCat) {
        return K.donut([{ label: 'في الوقت', val: o.onTime, color: '#3B2F8F', key: 'ok' }, { label: 'تأخر', val: o.late, color: '#E8794A', key: 'bad' }], { size: 180, th: 22, attr: 'otif', center: `<b class="num">${K.nf(o.onTime + o.late)}</b><small>إجمالي الطلبات</small>`, tip: it => K.nf(it.val) + ' طلب' }) +
          `<div class="dk-leg"><span><i style="background:#3B2F8F"></i>في الوقت</span><button type="button" class="dk-lnk" data-otif="bad"><i style="background:#E8794A;width:10px;height:10px;border-radius:3px;display:inline-block"></i>تأخر — فصّل حسب الفئة</button></div>`;
      }
      const v = (data.lateByCategory && data.lateByCategory[S.ord]) || CAT.map(() => 0);
      return `<button type="button" class="dk-btn" data-otifback>${K.ic('chev-left', 'icon icon-sm flip')}رجوع</button><p class="mut">الطلبات المتأخرة حسب الفئة</p>` +
        K.hbars(CAT.map((c, i) => ({ label: c, val: v[i] || 0, key: i, color: '#E8794A' })).sort((a, b) => b.val - a.val), { fmt: x => `<span class="num">${x}</span>`, u: 'طلب' });
    }

    function page() {
      const b = $('#scBody');
      if (S.pg === 'cmd') {
        const chRev = (data.channelRevenue && data.channelRevenue[S.ord]) || CH.map(() => 0);
        const chGp = (data.channelGrossProfit && data.channelGrossProfit[S.ord]) || CH.map(() => 0);
        const target = (data.kpis && data.kpis[S.ord] && data.kpis[S.ord].otifTargetPct) ?? 95, crit = target - 10;
        const pri = (data.otifByPriority && data.otifByPriority[S.ord]) || PRI.map(() => 0);
        const wh = (data.otifByWarehouse && data.otifByWarehouse[S.ord]) || WH.map(() => 0);
        b.innerHTML = `<div class="dk-grid">
          <section class="dk-card" style="--sp:5;--i:1"><header><h2>اتجاه المؤشرات</h2><span class="sp"></span><div class="dk-seg sm" data-seg="met" role="group" aria-label="المؤشر">${Object.entries(MET).map(([key, [n]]) => `<button type="button" data-v="${key}" aria-pressed="${S.met === key}">${n}</button>`).join('')}</div><p>سنة ← ربع ← شهر</p></header><div id="scTrend"></div></section>
          <section class="dk-card" style="--sp:3;--md:6;--i:2"><header><h2>OTIF حسب الطلب</h2><p>نوع التسليم مع تفصيل الفئة</p></header><div id="scOtif" class="sc2-otif"></div></section>
          <section class="dk-card" style="--sp:4;--md:6;--i:3"><header><h2>الإيراد ومجمل الربح حسب القناة</h2></header><div id="scCh"></div>${K.legend([['الإيراد', '#3B2F8F'], ['مجمل الربح', '#9C8CE6']])}</section>
          <section class="dk-card" style="--sp:6;--i:4"><header><h2>أولوية العملاء حسب OTIF</h2><p>الخط الأحمر = المستهدف ${K.nf(target, 0)}%</p></header>${K.hbars(PRI.map((n, i) => ({ label: n, val: pri[i] || 0, color: (pri[i] || 0) >= target ? '#1E9E6A' : '#3B2F8F' })), { max: 100, target, fmt: x => `<span class="num">${K.nf(x, 1)}%</span>`, u: '%' })}</section>
          <section class="dk-card" style="--sp:6;--i:5"><header><h2>المستودعات حسب OTIF</h2><p>الخط الأحمر = المستهدف ${K.nf(target, 0)}%</p></header>${K.hbars(WH.map((n, i) => ({ label: n, val: wh[i] || 0, color: (wh[i] || 0) >= target ? '#1E9E6A' : (wh[i] || 0) < crit ? '#D64545' : '#3B2F8F' })), { max: 100, target, fmt: x => `<span class="num">${K.nf(x, 1)}%</span>`, u: '%' })}</section></div>`;
        $('#scTrend').innerHTML = trend(); $('#scOtif').innerHTML = otif();
        $('#scCh').innerHTML = K.bars({ W: K.wOf($('#scCh')), H: 200, labels: CH, series: [{ name: 'الإيراد', color: '#3B2F8F', vals: chRev }, { name: 'مجمل الربح', color: '#9C8CE6', vals: chGp }], fmt: v => K.txt(v, 'ر.س'), aria: 'حسب القناة' });
        K.seg(b, 'met', v => { S.met = v; S.drill = []; $('#scTrend').innerHTML = trend(); });
      } else if (S.pg === 'risk') {
        const DIM = {
          ch: ['القناة', CH, (data.revenueAtRisk && data.revenueAtRisk.ch && data.revenueAtRisk.ch[S.ord]) || CH.map(() => 0)],
          wh: ['المستودع', WH, (data.revenueAtRisk && data.revenueAtRisk.wh && data.revenueAtRisk.wh[S.ord]) || WH.map(() => 0)],
          cat: ['الفئة', CAT, (data.revenueAtRisk && data.revenueAtRisk.cat && data.revenueAtRisk.cat[S.ord]) || CAT.map(() => 0)],
          sup: ['المورد', SUP, (data.revenueAtRisk && data.revenueAtRisk.sup && data.revenueAtRisk.sup[S.ord]) || SUP.map(() => 0)],
        }[S.dim];
        const mx = (data.lossMatrixPct && data.lossMatrixPct[S.ord]) || CH.map(() => CAT.map(() => 0));
        let maxV = -Infinity, maxR = 0, maxC = 0;
        mx.forEach((row, ri) => row.forEach((v, ci) => { if ((v ?? 0) > maxV) { maxV = v; maxR = ri; maxC = ci; } }));
        const insight = (CH[maxR] != null && CAT[maxC] != null && maxV > -Infinity)
          ? `أعلى خسارة: <b>${K.esc(CAT[maxC])}</b> عبر <b>${K.esc(CH[maxR])}</b> — نسبة <b>${K.nf(maxV, 1)}%</b> من إيراد هذا التقاطع.`
          : '';
        b.innerHTML = `<div class="dk-grid"><section class="dk-card" style="--sp:5;--i:1"><header><h2>الإيراد المعرّض للخطر</h2><span class="sp"></span><div class="dk-seg sm" data-seg="dim" role="group" aria-label="البعد">${[['sup', 'المورد'], ['ch', 'القناة'], ['wh', 'المستودع'], ['cat', 'الفئة']].map(([k2, n]) => `<button type="button" data-v="${k2}" aria-pressed="${S.dim === k2}">${n}</button>`).join('')}</div><p>حسب ${DIM[0]} · مرتب تنازليًا</p></header>${K.hbars(DIM[1].map((n, i) => ({ label: n, val: DIM[2][i] || 0, color: '#3B2F8F' })).sort((a, c) => c.val - a.val), { fmt: x => K.money(x), u: 'ر.س' })}</section>
          <section class="dk-card" style="--sp:7;--i:2"><header><h2>مصفوفة الخسائر: القناة × الفئة</h2><p>نسبة الإيراد المفقود % · كلما غمق اللون زادت الخسارة</p></header>${K.heat(CH, CAT, mx, { hue: '59,47,143', fmt: x => x + '%' })}
            ${insight ? `<div class="dk-take">${insight}</div>` : ''}</section></div>`;
        K.seg(b, 'dim', v => { S.dim = v; page(); });
      } else {
        const k = (data.kpis && data.kpis[S.ord]) || {};
        const wfVals = (data.efficiencyWaterfallVals && data.efficiencyWaterfallVals[S.ord]) || WF_STEPS.map(() => 0);
        const mg = (data.marginByDeliveryPct && data.marginByDeliveryPct[S.ord]) || { onTime: 0, failed: 0 };
        const prc = (data.profitRiskByChannel && data.profitRiskByChannel[S.ord]) || CH.map(() => 0);
        let topIdx = 0; prc.forEach((v, i) => { if ((v || 0) > (prc[topIdx] || 0)) topIdx = i; });
        const lossRatio = mg.onTime ? Math.max(0, (mg.onTime - mg.failed) / mg.onTime) : 0;
        const insight2 = CH[topIdx] != null ? `الطلبات المتأخرة تفقد قرابة <b>${K.nf(lossRatio * 100, 0)}%</b> من الهامش؛ تقليل التأخير في <b>${K.esc(CH[topIdx])}</b> وحده يعيد أكبر جزء من الأثر.` : '';
        b.innerHTML = `<div class="dk-grid"><section class="dk-card" style="--sp:12;--i:1"><header><h2>تحليل الكفاءة</h2><p>من الإيراد إلى صافي الأثر: خسائر نفاد المخزون والمرتجعات وغرامات التأخير والهدر وتكلفة البضاعة</p></header><div id="scWf"></div></section>
          <section class="dk-card" style="--sp:5;--i:2"><header><h2>الهامش: في الوقت مقابل المتأخر</h2><p>نسبة مجمل الربح %</p></header><div id="scMg"></div></section>
          <section class="dk-card" style="--sp:7;--i:3"><header><h2>الإيراد المعرّض حسب القناة</h2></header>${K.hbars(CH.map((n, i) => ({ label: n, val: prc[i] || 0, color: '#3B2F8F' })), { fmt: x => K.money(x), u: 'ر.س' })}${insight2 ? `<div class="dk-take">${insight2}</div>` : ''}</section></div>`;
        $('#scWf').innerHTML = K.waterfall(WF_STEPS.map(([label, kind], i) => ({ label, val: wfVals[i] || 0, kind: kind || undefined })), { W: K.wOf($('#scWf')), H: 250, fmt: v => K.txt(v, 'ر.س') });
        $('#scMg').innerHTML = K.bars({ W: K.wOf($('#scMg')), H: 200, labels: ['في الوقت', 'تسليم فاشل'], series: [{ name: 'هامش الربح', color: '#3B2F8F', vals: [mg.onTime || 0, mg.failed || 0] }], fmt: v => K.nf(v, 1) + '%', aria: 'الهامش' });
      }
    }

    function all() {
      const [t, s] = PG[S.pg]; $('#scTitle').textContent = t; $('#scSub').textContent = s;
      kpis();
      const b = $('#scBody');
      if (!K.RM) { b.classList.remove('sl-l'); void b.offsetWidth; b.classList.add('sl-l'); }
      page();
    }
    root.addEventListener('click', e => {
      const t = e.target; let x;
      if ((x = t.closest('[data-pg]'))) { S.pg = x.dataset.pg; root.querySelectorAll('[data-pg]').forEach(bt => bt.setAttribute('aria-selected', String(bt === x))); return all(); }
      if ((x = t.closest('[data-dr]'))) { if (S.drill.length < 2) { S.drill.push(+x.dataset.dr); $('#scTrend').innerHTML = trend(); } return; }
      if (t.closest('[data-up]')) { S.drill.pop(); $('#scTrend').innerHTML = trend(); return; }
      if ((x = t.closest('[data-otif]'))) { if (x.dataset.otif === 'bad') { S.otifCat = true; $('#scOtif').innerHTML = otif(); } return; }
      if (t.closest('[data-otifback]')) { S.otifCat = false; $('#scOtif').innerHTML = otif(); return; }
      if (t.closest('[data-reset]')) { Object.assign(S, { met: 'rev', drill: [], otifCat: false, dim: 'ch' }); return all(); }
    });
    K.seg(root, 'ord', v => { S.ord = v; all(); });
    K.onResize(root, page);
    all();
    return { destroy() {} };
  }

  window.JEEM_COMMAND_MOUNT = mount;
})();
