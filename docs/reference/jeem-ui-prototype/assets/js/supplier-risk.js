/* ==========================================================
   جيم — مخاطر الموردين والتسليم (لوحة تفاعلية بثلاث صفحات)
   مستوحاة من «Operations KPI Dashboard» (ZoomCharts / Power BI):
   نظرة عامة (مؤشرات بخطوط مصغرة + خلاصة تتغير بالسنة + جدول مخاطر إقليمي + شلال أثر الربح)،
   الموردون (مصفوفة أولوية بأرباع + موثوقية شهرية مقابل المستهدف + مخاطر التخزين)، المخزون.
   ========================================================== */
(function () {
  const root = document.getElementById('srRoot'); if (!root || !window.DK) return;
  const K = DK, $ = s => root.querySelector(s);
  const G = '#5E8C6A', R = '#D64545', A = '#E09A2B';
  const S = { pg: 'ov', yr: 2025, reg: null, sup: null, regSel: 'all' };
  const REG = ['دول البلطيق', 'شرق أوروبا', 'دول الشمال', 'جنوب أوروبا', 'غرب أوروبا'];
  const RT = { 2025: [[9.8, 95.4, 4.6], [-5.4, 82.0, 17.4], [-16.3, 88.5, 11.5], [-5.6, 95.2, 4.8], [-0.5, 96.0, 4.0]], 2024: [[6.1, 94.8, 5.2], [2.2, 90.4, 9.6], [-3.1, 92.2, 7.8], [1.8, 95.6, 4.4], [3.4, 96.5, 3.5]] };
  const SUP = [['لوجستيات المجمّدات الشرقية', 27, 15.2, 1, 'تأخر التسليم'], ['أطعمة القيمة بالجملة', 56, 11.5, 1, 'تعرض إيراد مرتفع'], ['مجموعة ألبان الشمال', 34, 7.8, 2, 'جودة'], ['مشروبات ممتازة', 24, 5.2, 3, ''], ['أطعمة البحر المتوسط', 37, 4.2, 3, ''], ['منتجات البلطيق الطازجة', 53, 4.0, 0, ''], ['الوجبات العالمية', 39, 2.6, 4, '']];
  const rag = (v, good, warn, hi = true) => (hi ? v >= good : v <= good) ? 'ok' : (hi ? v >= warn : v <= warn) ? 'warn' : 'bad';
  const spark = (k, c) => { const r = K.rng(k + S.yr); return K.spark(Array.from({ length: 12 }, (_, i) => 50 + Math.sin(i / 2 + (S.yr === 2025 ? 1 : 0)) * 10 + r() * 12), c, { h: 30 }); };

  function ov() {
    const y = S.yr === 2025, rt = RT[S.yr];
    const KP = [['OTIF %', K.pct(y ? 91.5 : 93.6, 1), y ? A : G], ['تغير OTIF (نقطة)', `<span class="num">${y ? '−2.07' : '+0.84'}</span>`, y ? R : G], ['الإيراد', K.money(y ? 298000 : 306900), G], ['تغير الإيراد', `<span class="num">${y ? '−2.9%' : '+4.1%'}</span>`, y ? R : G], ['مجمل الربح', K.money(y ? 103000 : 105400), G], ['تغير مجمل الربح', `<span class="num">${y ? '−2.3%' : '+3.2%'}</span>`, y ? R : G]];
    const worst = rt.reduce((a, r, i) => r[1] < rt[a][1] ? i : a, 0);
    const sel = S.reg;
    $('#srBody').innerHTML = `<div class="sr-kp">${KP.map(([n, v, c], i) => `<div class="dk-kpi" style="--i:${i};--c:${c}"><small>${n}</small><b style="color:${c}">${v}</b>${i % 2 === 0 ? spark(n, '#8A8F98') : ''}</div>`).join('')}<div class="dk-take sr-sum" style="--i:6">${y ? `أداء ${S.yr} تحت الضغط: الإيراد ومجمل الربح وOTIF أقل من العام الماضي. <b>${REG[worst]}</b> هي العائق التشغيلي الرئيسي بأدنى OTIF وأعلى نسبة تأخر.` : `أداء ${S.yr} متماسك: نمو في الإيراد والربح وOTIF قريب من المستهدف، مع ضعف نسبي في <b>${REG[worst]}</b>.`}</div></div>
      <div class="dk-grid">
        <section class="dk-card" style="--sp:6;--i:1"><header><h2>ملخص المخاطر الإقليمي</h2><p>اضغط منطقة لإبرازها في الشلال</p></header><div class="dk-tbl"><table><thead><tr><th>منطقة المورد</th><th>تغير الطلبات</th><th>OTIF</th><th>نسبة التأخر</th></tr></thead><tbody>${REG.map((n, i) => { const [o, ot, l] = rt[i]; return `<tr data-k="${i}" class="${sel === i ? 'on' : ''}" tabindex="0"><td>${n}</td><td><span class="dk-chip ${o >= 0 ? 'ok' : o > -6 ? 'warn' : 'bad'}"><span class="num">${o > 0 ? '+' : ''}${o}%</span></span></td><td><span class="dk-chip ${rag(ot, 95, 88)}">${K.ic(rag(ot, 95, 88) === 'ok' ? 'check' : rag(ot, 95, 88) === 'warn' ? 'info' : 'x')}<span class="num">${ot}%</span></span></td><td><span class="dk-chip ${rag(l, 5, 10, false)}"><span class="num">${l}%</span></span></td></tr>`; }).join('')}</tbody></table></div><p class="sr-key"><span class="dk-chip ok">سليم</span><span class="dk-chip warn">مراقبة</span><span class="dk-chip bad">أولوية خطر</span></p></section>
        <section class="dk-card" style="--sp:6;--i:2"><header><h2>أثر تغير مجمل الربح حسب منطقة المورد</h2><p>مساهمة كل منطقة في تغير الربح السنوي %</p></header><div id="srWf"></div></section>
        <section class="dk-card" style="--sp:6;--i:3"><header><h2>اتجاه مجمل الربح حسب الربع</h2><p>الأحمر = ربع دون المتوسط · الخط المتقطع = المتوسط</p></header><div id="srQ"></div></section>
        <section class="dk-card" style="--sp:6;--i:4"><header><h2>الخلاصة التنفيذية</h2></header><div class="dk-take">${y ? `${S.yr} أقل من العام الماضي في الإيراد ومجمل الربح وOTIF. <b>شرق أوروبا</b> أولوية بسبب ضعف موثوقية التسليم والأثر السلبي على الربح.` : `${S.yr} عام نمو؛ حافظ على مستويات خدمة دول البلطيق وغرب أوروبا، وراقب <b>دول الشمال</b>.`}</div><ul class="sr-acts"><li>${K.ic('target')}خطة تعافٍ أسبوعية للتسليم مع موردي ${REG[worst]}</li><li>${K.ic('shield')}رفع مخزون الأمان للفئات المبردة</li><li>${K.ic('chart')}مراجعة شهرية لهامش الطلبات المتأخرة</li></ul></section>
      </div>`;
    const imp = y ? [3.0, 0.4, -0.4, -0.9, -4.5] : [1.2, 0.6, 0.3, 0.4, 0.7];
    const order = [4, 0, 2, 3, 1];
    $('#srWf').innerHTML = K.waterfall([...order.map(i => ({ label: REG[i], val: imp[[4, 0, 2, 3, 1].indexOf(i)] })), { label: 'الإجمالي', val: imp.reduce((a, b) => a + b, 0), kind: 'total' }], { W: K.wOf($('#srWf')), H: 230, fmt: v => K.nf(v, 1) + '%', sel: sel == null ? null : order.indexOf(sel), attr: 'wfr' });
    const q = y ? [26, 22, 26.6, 27] : [25.1, 26.2, 26.4, 27.7], avg = q.reduce((a, b) => a + b, 0) / 4;
    $('#srQ').innerHTML = K.bars({ W: K.wOf($('#srQ')), H: 200, labels: ['الربع 1', 'الربع 2', 'الربع 3', 'الربع 4'], series: [{ name: 'مجمل الربح', color: G, vals: q.map(v => v * 1000) }], target: avg * 1000, targetLabel: `المتوسط ${K.txt(avg * 1000, 'ر.س')}`, fmt: v => K.txt(v, 'ر.س'), aria: 'مجمل الربح حسب الربع' });
    root.querySelectorAll('#srQ .col').forEach((g, i) => { if (q[i] < avg) g.querySelector('.b').setAttribute('fill', R); });
    root.querySelectorAll('#srWf .b.tot').forEach(b => b.style.fill = '#A9ADB4');
  }
  function sup() {
    const y = S.yr === 2025, k = y ? 1 : .8;
    const pts = SUP.map(([n, rev, risk, reg, act], i) => ({ label: n, x: rev * 1000, y: risk * k, r: 10 + rev / 8, color: risk * k > 10 ? R : risk * k > 7 ? A : G, key: i, reg })).filter(p => S.regSel === 'all' || p.reg === +S.regSel);
    const acts = SUP.filter(s => s[4]);
    $('#srBody').innerHTML = `<div class="sr-kp three"><div class="dk-kpi"><small>نسبة التأخر في التسليم</small><b style="color:${R}">${K.pct(y ? 8.5 : 6.1, 1)}</b><span class="dk-chip bad">ضغط على التسليم يتصاعد</span></div><div class="dk-kpi"><small>موردون عاليو المخاطر</small><b style="color:${A}"><span class="num">${y ? 2 : 1}</span></b><span class="dk-chip warn"><span class="num">${y ? 2 : 1}</span> مورد حرج</span></div><div class="dk-kpi"><small>الإيراد المعرّض للخطر</small><b style="color:${G}">${K.money(y ? 83120 : 51400)}</b><span class="dk-chip warn">التعرض المالي مستمر</span></div>
        <div class="sr-acts2"><b>إجراءات الموردين ذوي الأولوية</b>${acts.map(s => `<button type="button" data-sup="${SUP.indexOf(s)}" aria-pressed="${S.sup === SUP.indexOf(s)}"><b>${s[0]}</b><small>${s[4]}</small><small>${['مراجعة تعافٍ أسبوعية', 'خطة تحسين التسليم', 'مراقبة جودة مستمرة'][acts.indexOf(s)]}</small></button>`).join('')}</div></div>
      <div class="dk-grid">
        <section class="dk-card" style="--sp:6;--i:1"><header><h2>مصفوفة أولوية مخاطر الموردين</h2><p>الأفقي: متوسط الإيراد · الرأسي: درجة المخاطر · الحجم: الإيراد</p></header><div id="srSc"></div></section>
        <section class="dk-card" style="--sp:6;--i:2"><header><h2>موثوقية التسليم الشهرية مقابل المخاطر</h2><p>OTIF % والمستهدف 95% · الأحمر = شهر دون 90%</p></header><div id="srMo"></div></section>
        <section class="dk-card" style="--sp:12;--i:3"><header><h2>مخاطر التسليم والتخزين المبرد حسب الفئة</h2><p>الأفقي: نسبة التأخر · الرأسي: مخاطر التخزين · الحجم: الإيراد المعرّض</p></header><div id="srCat"></div></section>
      </div>`;
    $('#srSc').innerHTML = K.scatter(pts, { W: K.wOf($('#srSc')), H: 280, xmin: 20000, xmax: 70000, ymin: 0, ymax: 20, qx: 37000, qy: 7, quads: ['موردون للمراقبة', 'موردون حرجون', 'موردون مستقرون', 'موردون استراتيجيون'], sel: S.sup, attr: 'sup', fx: v => K.txt(v), fy: v => K.nf(v, 0), tip: p => `إيراد ${K.txt(p.x, 'ر.س')} · مخاطر ${K.nf(p.y, 1)}` });
    const ot = y ? [94.3, 95.6, 91.6, 92.8, 83.0, 92.0, 89.9, 89.5, 90.6, 93.4, 94.4, 90.7] : [95.1, 95.8, 94.2, 93.9, 92.6, 93.8, 94.4, 92.1, 93.3, 95.0, 95.6, 94.1];
    $('#srMo').innerHTML = K.bars({ W: K.wOf($('#srMo')), H: 230, labels: ['ينا', 'فبر', 'مار', 'أبر', 'ماي', 'يون', 'يول', 'أغس', 'سبت', 'أكت', 'نوف', 'ديس'], series: [{ name: 'OTIF', color: G, vals: ot }], ymax: 100, target: 95, targetLabel: 'المستهدف 95%', fmt: v => K.nf(v, 1) + '%', aria: 'OTIF شهري' });
    root.querySelectorAll('#srMo .col').forEach((g, i) => { if (ot[i] < 90) g.querySelector('.b').setAttribute('fill', R); });
    $('#srCat').innerHTML = K.scatter([['مجمّد', 13.2, 14.5, 46, R], ['محيط', 8.2, 7.9, 22, A], ['مبرد', 7.0, 5.6, 18, G], ['جاف', 5.6, 3.1, 9, G]].map(([n, x, yy, r, c], i) => ({ label: n, x: x * (y ? 1 : .8), y: yy * (y ? 1 : .85), r: 8 + r / 2.4, color: c, key: i })), { W: K.wOf($('#srCat')), H: 240, xmin: 4, xmax: 16, ymin: 0, ymax: 18, qx: 9, qy: 10, quads: ['حرج', 'مشكلة تخزين', 'مشكلة تسليم', 'مستقر'], fx: v => K.nf(v, 0) + '%', fy: v => K.nf(v, 0) + '%' });
  }
  function inv() {
    const y = S.yr === 2025, C = ['المجمّدات', 'الألبان', 'المخبوزات', 'المشروبات', 'الوجبات الخفيفة'], so = y ? [6.8, 4.9, 3.6, 2.1, 1.4] : [5.2, 4.1, 3.2, 2.4, 1.6], ws = y ? [3.9, 5.6, 7.2, 1.1, 0.8] : [3.1, 4.8, 6.5, 1.3, 0.9];
    $('#srBody').innerHTML = `<div class="dk-grid"><section class="dk-card" style="--sp:6;--i:1"><header><h2>معدل نفاد المخزون حسب الفئة</h2><p>المستهدف ≤ 3%</p></header>${K.hbars(C.map((n, i) => ({ label: n, val: so[i], color: so[i] > 5 ? R : so[i] > 3 ? A : G })), { max: 8, target: 3, fmt: x => `<span class="num">${K.nf(x, 1)}%</span>`, u: '%' })}</section>
      <section class="dk-card" style="--sp:6;--i:2"><header><h2>معدل الهدر حسب الفئة</h2><p>المستهدف ≤ 4%</p></header>${K.hbars(C.map((n, i) => ({ label: n, val: ws[i], color: ws[i] > 6 ? R : ws[i] > 4 ? A : G })), { max: 8, target: 4, fmt: x => `<span class="num">${K.nf(x, 1)}%</span>`, u: '%' })}</section>
      <section class="dk-card" style="--sp:12;--i:3"><header><h2>مخاطر المخزون</h2></header><div class="dk-tbl"><table><thead><tr><th>الفئة</th><th>أيام التغطية</th><th>نفاد المخزون</th><th>الهدر</th><th>الحالة</th></tr></thead><tbody>${C.map((n, i) => { const st = so[i] > 5 || ws[i] > 6 ? 'bad' : so[i] > 3 || ws[i] > 4 ? 'warn' : 'ok'; return `<tr><td>${n}</td><td><span class="num">${[9, 6, 3, 21, 28][i]}</span> أيام</td><td><span class="num">${so[i]}%</span></td><td><span class="num">${ws[i]}%</span></td><td><span class="dk-chip ${st}">${K.ic(st === 'ok' ? 'check' : st === 'warn' ? 'info' : 'x')}${K.ST[st][2]}</span></td></tr>`; }).join('')}</tbody></table></div></section></div>`;
  }
  const TT = { ov: 'نظرة تنفيذية على سلسلة الإمداد', sup: 'مخاطر الموردين والتسليم', inv: 'مخاطر المخزون' };
  function all(anim = true) { $('#srTitle').textContent = TT[S.pg]; $('#srRegW').hidden = S.pg !== 'sup'; const b = $('#srBody'); if (anim && !K.RM) { b.classList.remove('sl-l'); void b.offsetWidth; b.classList.add('sl-l'); } ({ ov, sup, inv })[S.pg](); }
  $('#srReg').innerHTML += REG.map((n, i) => `<option value="${i}">${n}</option>`).join('');
  root.addEventListener('click', e => {
    const t = e.target; let x;
    if ((x = t.closest('[data-pg]'))) { S.pg = x.dataset.pg; root.querySelectorAll('[data-pg]').forEach(b => b.setAttribute('aria-selected', String(b === x))); return all(); }
    if ((x = t.closest('tr[data-k]'))) { const i = +x.dataset.k; S.reg = S.reg === i ? null : i; return ov(); }
    if ((x = t.closest('[data-sup]'))) { const i = +x.dataset.sup; S.sup = S.sup === i ? null : i; return sup(); }
    if (t.closest('[data-reset]')) { Object.assign(S, { reg: null, sup: null, regSel: 'all' }); $('#srReg').value = 'all'; return all(); }
  });
  root.addEventListener('keydown', e => { const r = e.target.closest?.('tr[data-k]'); if (r && (e.key === 'Enter' || e.key === ' ')) { e.preventDefault(); r.click(); } });
  $('#srReg').addEventListener('change', e => { S.regSel = e.target.value; sup(); });
  K.seg(root, 'yr', v => { S.yr = +v; all(false); });
  K.onResize(root, () => all(false));
  all();
})();
