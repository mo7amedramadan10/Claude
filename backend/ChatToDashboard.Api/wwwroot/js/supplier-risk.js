/* ==========================================================
   جيم — مخاطر الموردين والتسليم (لوحة تفاعلية بثلاث صفحات، المحرك: risk، على dash-kit)
   مُعاد بناؤه كدالة تركيب قابلة لإعادة الاستخدام (window.JEEM_RISK_MOUNT) بدل IIFE ذاتي
   التشغيل بأرقام ثابتة مضمّنة لسنتين فقط (2024/2025) — يقبل data حقيقية أو من مكتبة العرض
   التجريبي (supplier-risk-demo.js) لأي عدد من السنوات. نصوص الخلاصة التنفيذية كانت نصين
   ثابتين مرتبطين بسنة بعينها (2025 مقابل غيرها) — أصبحت تُبنى من شرط محسوب فعليًا (هل
   الإيراد تراجع عن العام السابق؟) بدل اسم السنة نفسه، فتصلح لأي سنتين متتاليتين حقيقيتين.
   ========================================================== */
(function () {
  function mount(root, data, opts = {}) {
    if (!root || !data || !window.DK) return;
    const K = DK, $ = s => root.querySelector(s);
    const G = '#5E8C6A', R = '#D64545', A = '#E09A2B';
    const REG = (data.regions || []).map(r => r.name);
    const YEARS = data.years && data.years.length ? data.years : [new Date().getFullYear()];
    const S = { pg: 'ov', yr: YEARS[YEARS.length - 1], reg: null, sup: null, regSel: 'all' };
    const RT = data.regionMetrics || {};
    const SUP = data.suppliers || [];
    const rag = (v, good, warn, hi = true) => (hi ? v >= good : v <= good) ? 'ok' : (hi ? v >= warn : v <= warn) ? 'warn' : 'bad';
    function spark(metricKey) {
      const series = (data.kpiSparklines && data.kpiSparklines[metricKey] && data.kpiSparklines[metricKey][S.yr]) || [];
      return series.length ? K.spark(series, '#8A8F98', { h: 30 }) : '';
    }

    function ov() {
      const K0 = (data.kpis || {})[S.yr] || {};
      const rt = RT[S.yr] || REG.map(() => [0, 0, 0]);
      const down = (K0.revenueChangePct ?? 0) < 0;
      const KP = [
        ['OTIF %', K.pct(K0.otifPct ?? 0, 1), down ? A : G, 'otif'],
        ['تغير OTIF (نقطة)', `<span class="num">${(K0.otifChangePts ?? 0) >= 0 ? '+' : '−'}${K.nf(Math.abs(K0.otifChangePts ?? 0), 2)}</span>`, down ? R : G, null],
        ['الإيراد', K.money(K0.revenue ?? 0), G, 'revenue'],
        ['تغير الإيراد', `<span class="num">${(K0.revenueChangePct ?? 0) >= 0 ? '+' : '−'}${K.nf(Math.abs(K0.revenueChangePct ?? 0), 1)}%</span>`, down ? R : G, null],
        ['مجمل الربح', K.money(K0.grossProfit ?? 0), G, 'grossProfit'],
        ['تغير مجمل الربح', `<span class="num">${(K0.grossProfitChangePct ?? 0) >= 0 ? '+' : '−'}${K.nf(Math.abs(K0.grossProfitChangePct ?? 0), 1)}%</span>`, down ? R : G, null],
      ];
      const worst = rt.length ? rt.reduce((a, r, i) => r[1] < rt[a][1] ? i : a, 0) : 0;
      const sel = S.reg;
      const summary = down
        ? `أداء ${S.yr} تحت الضغط: الإيراد ومجمل الربح وOTIF أقل من العام الماضي. <b>${K.esc(REG[worst] || '')}</b> هي العائق التشغيلي الرئيسي بأدنى OTIF وأعلى نسبة تأخر.`
        : `أداء ${S.yr} متماسك: نمو في الإيراد والربح وOTIF قريب من المستهدف، مع ضعف نسبي في <b>${K.esc(REG[worst] || '')}</b>.`;
      $('#srBody').innerHTML = `<div class="sr-kp">${KP.map(([n, v, c, sk], i) => `<div class="dk-kpi" style="--i:${i};--c:${c}"><small>${n}</small><b style="color:${c}">${v}</b>${sk ? spark(sk) : ''}</div>`).join('')}<div class="dk-take sr-sum" style="--i:6">${summary}</div></div>
        <div class="dk-grid">
          <section class="dk-card" style="--sp:6;--i:1"><header><h2>ملخص المخاطر الإقليمي</h2><p>اضغط منطقة لإبرازها في الشلال</p></header><div class="dk-tbl"><table><thead><tr><th>منطقة المورد</th><th>تغير الطلبات</th><th>OTIF</th><th>نسبة التأخر</th></tr></thead><tbody>${REG.map((n, i) => { const [o, ot, l] = rt[i] || [0, 0, 0]; return `<tr data-k="${i}" class="${sel === i ? 'on' : ''}" tabindex="0"><td>${K.esc(n)}</td><td><span class="dk-chip ${o >= 0 ? 'ok' : o > -6 ? 'warn' : 'bad'}"><span class="num">${o > 0 ? '+' : ''}${o}%</span></span></td><td><span class="dk-chip ${rag(ot, 95, 88)}">${K.ic(rag(ot, 95, 88) === 'ok' ? 'check' : rag(ot, 95, 88) === 'warn' ? 'info' : 'x')}<span class="num">${ot}%</span></span></td><td><span class="dk-chip ${rag(l, 5, 10, false)}"><span class="num">${l}%</span></span></td></tr>`; }).join('')}</tbody></table></div><p class="sr-key"><span class="dk-chip ok">سليم</span><span class="dk-chip warn">مراقبة</span><span class="dk-chip bad">أولوية خطر</span></p></section>
          <section class="dk-card" style="--sp:6;--i:2"><header><h2>أثر تغير مجمل الربح حسب منطقة المورد</h2><p>مساهمة كل منطقة في تغير الربح السنوي %</p></header><div id="srWf"></div></section>
          <section class="dk-card" style="--sp:6;--i:3"><header><h2>اتجاه مجمل الربح حسب الربع</h2><p>الأحمر = ربع دون المتوسط · الخط المتقطع = المتوسط</p></header><div id="srQ"></div></section>
          <section class="dk-card" style="--sp:6;--i:4"><header><h2>الخلاصة التنفيذية</h2></header><div class="dk-take">${down ? `${S.yr} أقل من العام السابق في الإيراد ومجمل الربح وOTIF. <b>${K.esc(REG[worst] || '')}</b> أولوية بسبب ضعف موثوقية التسليم والأثر السلبي على الربح.` : `${S.yr} عام نمو؛ حافظ على مستويات الخدمة القوية، وراقب <b>${K.esc(REG[worst] || '')}</b>.`}</div><ul class="sr-acts"><li>${K.ic('target')}خطة تعافٍ أسبوعية للتسليم مع موردي ${K.esc(REG[worst] || '')}</li><li>${K.ic('shield')}رفع مخزون الأمان للفئات المبردة</li><li>${K.ic('chart')}مراجعة شهرية لهامش الطلبات المتأخرة</li></ul></section>
        </div>`;
      const imp = (data.profitImpact && data.profitImpact[S.yr]) || REG.map(() => 0);
      const order = imp.map((_, i) => i).sort((a, b) => imp[b] - imp[a]);
      $('#srWf').innerHTML = K.waterfall([...order.map(i => ({ label: REG[i], val: imp[i] })), { label: 'الإجمالي', val: imp.reduce((a, b) => a + b, 0), kind: 'total' }], { W: K.wOf($('#srWf')), H: 230, fmt: v => K.nf(v, 1) + '%', sel: sel == null ? null : order.indexOf(sel), attr: 'wfr' });
      const q = (data.quarterlyProfit && data.quarterlyProfit[S.yr]) || [0, 0, 0, 0], avg = q.reduce((a, b) => a + b, 0) / 4;
      $('#srQ').innerHTML = K.bars({ W: K.wOf($('#srQ')), H: 200, labels: ['الربع 1', 'الربع 2', 'الربع 3', 'الربع 4'], series: [{ name: 'مجمل الربح', color: G, vals: q.map(v => v * 1000) }], target: avg * 1000, targetLabel: `المتوسط ${K.txt(avg * 1000, 'ر.س')}`, fmt: v => K.txt(v, 'ر.س'), aria: 'مجمل الربح حسب الربع' });
      root.querySelectorAll('#srQ .col').forEach((g, i) => { if (q[i] < avg) g.querySelector('.b').setAttribute('fill', R); });
      root.querySelectorAll('#srWf .b.tot').forEach(b => b.style.fill = '#A9ADB4');
    }
    function sup() {
      const SK = (data.supplierKpis || {})[S.yr] || {};
      const pts = SUP.map((s, i) => ({ label: s.name, x: s.revenueK * 1000, y: s.riskScore, r: 10 + s.revenueK / 8, color: s.riskScore > 10 ? R : s.riskScore > 7 ? A : G, key: i, reg: s.region })).filter(p => S.regSel === 'all' || p.reg === +S.regSel);
      const acts = SUP.filter(s => s.action);
      $('#srBody').innerHTML = `<div class="sr-kp three"><div class="dk-kpi"><small>نسبة التأخر في التسليم</small><b style="color:${R}">${K.pct(SK.lateDeliveryPct ?? 0, 1)}</b><span class="dk-chip bad">ضغط على التسليم يتصاعد</span></div><div class="dk-kpi"><small>موردون عاليو المخاطر</small><b style="color:${A}"><span class="num">${K.nf(SK.highRiskCount ?? 0)}</span></b><span class="dk-chip warn"><span class="num">${K.nf(SK.highRiskCount ?? 0)}</span> مورد حرج</span></div><div class="dk-kpi"><small>الإيراد المعرّض للخطر</small><b style="color:${G}">${K.money(SK.revenueAtRiskSar ?? 0)}</b><span class="dk-chip warn">التعرض المالي مستمر</span></div>
          <div class="sr-acts2"><b>إجراءات الموردين ذوي الأولوية</b>${acts.map((s, j) => `<button type="button" data-sup="${SUP.indexOf(s)}" aria-pressed="${S.sup === SUP.indexOf(s)}"><b>${K.esc(s.name)}</b><small>${K.esc(s.action)}</small><small>${K.esc(s.actionPlan || '')}</small></button>`).join('')}</div></div>
        <div class="dk-grid">
          <section class="dk-card" style="--sp:6;--i:1"><header><h2>مصفوفة أولوية مخاطر الموردين</h2><p>الأفقي: متوسط الإيراد · الرأسي: درجة المخاطر · الحجم: الإيراد</p></header><div id="srSc"></div></section>
          <section class="dk-card" style="--sp:6;--i:2"><header><h2>موثوقية التسليم الشهرية مقابل المخاطر</h2><p>OTIF % والمستهدف 95% · الأحمر = شهر دون 90%</p></header><div id="srMo"></div></section>
          <section class="dk-card" style="--sp:12;--i:3"><header><h2>مخاطر التسليم والتخزين المبرد حسب الفئة</h2><p>الأفقي: نسبة التأخر · الرأسي: مخاطر التخزين · الحجم: الإيراد المعرّض</p></header><div id="srCat"></div></section>
        </div>`;
      $('#srSc').innerHTML = K.scatter(pts, { W: K.wOf($('#srSc')), H: 280, xmin: 20000, xmax: 70000, ymin: 0, ymax: 20, qx: 37000, qy: 7, quads: ['موردون للمراقبة', 'موردون حرجون', 'موردون مستقرون', 'موردون استراتيجيون'], sel: S.sup, attr: 'sup', fx: v => K.txt(v), fy: v => K.nf(v, 0), tip: p => `إيراد ${K.txt(p.x, 'ر.س')} · مخاطر ${K.nf(p.y, 1)}` });
      const ot = (data.monthlyOtif && data.monthlyOtif[S.yr]) || Array(12).fill(0);
      $('#srMo').innerHTML = K.bars({ W: K.wOf($('#srMo')), H: 230, labels: ['ينا', 'فبر', 'مار', 'أبر', 'ماي', 'يون', 'يول', 'أغس', 'سبت', 'أكت', 'نوف', 'ديس'], series: [{ name: 'OTIF', color: G, vals: ot }], ymax: 100, target: 95, targetLabel: 'المستهدف 95%', fmt: v => K.nf(v, 1) + '%', aria: 'OTIF شهري' });
      root.querySelectorAll('#srMo .col').forEach((g, i) => { if (ot[i] < 90) g.querySelector('.b').setAttribute('fill', R); });
      const cats = (data.categoryRisk && data.categoryRisk[S.yr]) || [];
      $('#srCat').innerHTML = K.scatter(cats.map((c, i) => ({ label: c.name, x: c.latePct, y: c.storageRisk, r: 8 + c.revenueExposed / 2.4, color: c.color, key: i })), { W: K.wOf($('#srCat')), H: 240, xmin: 4, xmax: 16, ymin: 0, ymax: 18, qx: 9, qy: 10, quads: ['حرج', 'مشكلة تخزين', 'مشكلة تسليم', 'مستقر'], fx: v => K.nf(v, 0) + '%', fy: v => K.nf(v, 0) + '%' });
    }
    function inv() {
      const I = (data.inventory && data.inventory[S.yr]) || { categories: [] };
      const C = I.categories;
      $('#srBody').innerHTML = `<div class="dk-grid"><section class="dk-card" style="--sp:6;--i:1"><header><h2>معدل نفاد المخزون حسب الفئة</h2><p>المستهدف ≤ 3%</p></header>${K.hbars(C.map(c => ({ label: c.name, val: c.stockoutPct, color: c.stockoutPct > 5 ? R : c.stockoutPct > 3 ? A : G })), { max: 8, target: 3, fmt: x => `<span class="num">${K.nf(x, 1)}%</span>`, u: '%' })}</section>
        <section class="dk-card" style="--sp:6;--i:2"><header><h2>معدل الهدر حسب الفئة</h2><p>المستهدف ≤ 4%</p></header>${K.hbars(C.map(c => ({ label: c.name, val: c.wastePct, color: c.wastePct > 6 ? R : c.wastePct > 4 ? A : G })), { max: 8, target: 4, fmt: x => `<span class="num">${K.nf(x, 1)}%</span>`, u: '%' })}</section>
        <section class="dk-card" style="--sp:12;--i:3"><header><h2>مخاطر المخزون</h2></header><div class="dk-tbl"><table><thead><tr><th>الفئة</th><th>أيام التغطية</th><th>نفاد المخزون</th><th>الهدر</th><th>الحالة</th></tr></thead><tbody>${C.map(c => { const st = c.stockoutPct > 5 || c.wastePct > 6 ? 'bad' : c.stockoutPct > 3 || c.wastePct > 4 ? 'warn' : 'ok'; return `<tr><td>${K.esc(c.name)}</td><td><span class="num">${K.nf(c.coverageDays)}</span> أيام</td><td><span class="num">${c.stockoutPct}%</span></td><td><span class="num">${c.wastePct}%</span></td><td><span class="dk-chip ${st}">${K.ic(st === 'ok' ? 'check' : st === 'warn' ? 'info' : 'x')}${K.ST[st][2]}</span></td></tr>`; }).join('')}</tbody></table></div></section></div>`;
    }
    const TT = { ov: 'نظرة تنفيذية على سلسلة الإمداد', sup: 'مخاطر الموردين والتسليم', inv: 'مخاطر المخزون' };
    function all(anim = true) { $('#srTitle').textContent = TT[S.pg]; $('#srRegW').hidden = S.pg !== 'sup'; const b = $('#srBody'); if (anim && !K.RM) { b.classList.remove('sl-l'); void b.offsetWidth; b.classList.add('sl-l'); } ({ ov, sup, inv })[S.pg](); }

    const yrSeg = $('[data-seg="yr"]');
    if (yrSeg) yrSeg.innerHTML = YEARS.map(y => `<button type="button" data-v="${y}" aria-pressed="${y === S.yr}">${y}</button>`).join('');
    const regSel = $('#srReg');
    if (regSel) regSel.innerHTML += REG.map((n, i) => `<option value="${i}">${K.esc(n)}</option>`).join('');

    root.addEventListener('click', e => {
      const t = e.target; let x;
      if ((x = t.closest('[data-pg]'))) { S.pg = x.dataset.pg; root.querySelectorAll('[data-pg]').forEach(b => b.setAttribute('aria-selected', String(b === x))); return all(); }
      if ((x = t.closest('tr[data-k]'))) { const i = +x.dataset.k; S.reg = S.reg === i ? null : i; return ov(); }
      if ((x = t.closest('[data-sup]'))) { const i = +x.dataset.sup; S.sup = S.sup === i ? null : i; return sup(); }
      if (t.closest('[data-reset]')) { Object.assign(S, { reg: null, sup: null, regSel: 'all' }); if (regSel) regSel.value = 'all'; return all(); }
    });
    root.addEventListener('keydown', e => { const r = e.target.closest?.('tr[data-k]'); if (r && (e.key === 'Enter' || e.key === ' ')) { e.preventDefault(); r.click(); } });
    regSel?.addEventListener('change', e => { S.regSel = e.target.value; sup(); });
    K.seg(root, 'yr', v => { S.yr = +v; all(false); });
    K.onResize(root, () => all(false));

    all();
    return { destroy() {} };
  }

  window.JEEM_RISK_MOUNT = mount;
})();
