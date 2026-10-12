/* ==========================================================
   جيم — الإيرادات المتكررة والاحتفاظ (لوحة تفاعلية، المحرك: saas، على dash-kit)
   مُعاد بناؤه كدالة تركيب قابلة لإعادة الاستخدام (window.JEEM_SAAS_MOUNT) بدل IIFE ذاتي
   التشغيل ببيانات ثابتة مضمّنة — يقبل الآن data من مصدر حقيقي أو من مكتبة العرض التجريبي
   (saas-metrics-demo.js). تعريفات حركات شلال الدخل (DEF) مسرد مصطلحات ثابت (glossary) وليست
   بيانات مشروع، فتبقى مضمّنة في المحرك — راجع data.ranges/mrr/netNew/cohorts/plans/insights.
   ========================================================== */
(function () {
  function mount(root, data, opts = {}) {
    if (!root || !data || !window.DK) return;
    const K = DK, $ = s => root.querySelector(s);
    const ranges = data.ranges && data.ranges.length ? data.ranges : [13];
    const S = { rng: ranges[0], coh: 'logo', wf: null };
    const DEF = [
      ['الدخل الشهري الافتتاحي', 'الدخل المتكرر من الاشتراكات النشطة في بداية المدى. يستثني الرسوم لمرة واحدة والخدمات والاستخدام الزائد.'],
      ['دخل جديد', 'دخل من عملاء لم يكن لديهم اشتراك نشط في بداية الفترة.'],
      ['توسّع', 'زيادة دخل العملاء الحاليين: ترقيات، مقاعد إضافية، انتقال لباقة أعلى.'],
      ['انكماش', 'انخفاض دخل عملاء بقوا: تخفيض الباقة أو إزالة مقاعد.'],
      ['إلغاء', 'دخل مفقود من عملاء ألغوا بالكامل خلال الفترة.'],
      ['الدخل الشهري الختامي', 'الافتتاحي + الجديد + التوسع − الانكماش − الإلغاء.'],
    ];
    const MRR = data.mrr || {};
    function vals(rng) { const m = MRR[rng] || {}; return [m.opening || 0, m.new || 0, m.expansion || 0, m.contraction || 0, m.churn || 0]; }
    function kpis() {
      const [o, n, x, c, ch] = vals(S.rng), cl = o + n + x + c + ch, nrr = o ? (o + x + c + ch) / o * 100 : 0;
      $('#smKpis').innerHTML = [
        ['الدخل الشهري المتكرر', K.money(cl), K.delta(o ? (cl / o - 1) * 100 : 0)],
        ['صافي الدخل الجديد', K.money(cl - o), ''],
        ['صافي الاحتفاظ بالإيراد', K.pct(nrr), `<span class="dk-chip ${nrr >= 100 ? 'ok' : 'warn'}">${nrr >= 100 ? 'فوق 100%' : 'تحت 100%'}</span>`],
        ['إلغاء الحسابات شهريًا', K.pct(data.churnRatePct ?? 0, 2), `<span class="dk-chip ${(data.churnRatePct ?? 0) > 4 ? 'bad' : 'warn'}">${(data.churnRatePct ?? 0) > 4 ? 'مرتفع' : 'منتبه'}</span>`],
        ['متوسط الإيراد لكل حساب', K.money(data.arpa ?? 0), ''],
        ['القيمة الدائمة ÷ تكلفة الاكتساب', `<span class="num">${K.nf(data.ltvCac ?? 0, 1)}x</span>`, `<span class="dk-chip ${(data.ltvCac ?? 0) >= 3 ? 'ok' : 'warn'}">${(data.ltvCac ?? 0) >= 3 ? 'فوق 3x' : 'تحت 3x'}</span>`],
        ['فترة الاسترداد', K.num(data.paybackMonths ?? 0, 'شهر', 1), ''],
      ].map(([n, v, x], i) => `<div class="dk-kpi" style="--i:${i}"><small>${n}</small><b>${v}</b><span class="row">${x}</span></div>`).join('');
    }
    function wf() {
      const [o, n, x, c, ch] = vals(S.rng);
      $('#smWf').innerHTML = K.waterfall([
        { label: 'افتتاحي', val: o, kind: 'total' }, { label: 'جديد', val: n }, { label: 'توسّع', val: x },
        { label: 'انكماش', val: c }, { label: 'إلغاء', val: ch }, { label: 'ختامي', val: o + n + x + c + ch, kind: 'total' },
      ], { W: K.wOf($('#smWf')), H: 240, sel: S.wf, attr: 'wf', fmt: v => K.txt(v, 'ر.س') });
      const d = S.wf == null ? null : DEF[S.wf];
      $('#smDef').innerHTML = d ? `<b>${d[0]}</b> — ${d[1]}` : `<span class="mut">كل حركة مسؤولية فريق مختلف: الجديد للمبيعات، التوسع والانكماش لنجاح العملاء، الإلغاء للمنتج والدعم.</span>`;
    }
    function net() {
      const series = (data.netNew && data.netNew[S.rng]) || { labels: [], vals: [] };
      $('#smNet').innerHTML = K.bars({
        W: K.wOf($('#smNet')), H: 220, labels: series.labels, series: [{ name: 'صافي جديد', color: K.PAL[0], vals: series.vals }],
        neg: true, fmt: x => K.txt(x, 'ر.س'), aria: 'صافي الدخل الجديد أسبوعيًا',
      });
      root.querySelectorAll('#smNet .b.ng').forEach(b => b.setAttribute('fill', '#D64545'));
    }
    const COH = data.cohorts || { months: [], ages: [], logo: [], rev: [] };
    function coh() {
      const v = S.coh === 'rev' ? COH.rev : COH.logo;
      if (!COH.months.length || !v.length) { $('#smCoh').innerHTML = '<p class="dk-empty">لا بيانات احتفاظ كافية بعد.</p>'; return; }
      $('#smCoh').innerHTML = K.heat(COH.months, COH.ages, v, { min: 0, max: 105, hue: '0,168,159', fmt: x => x + '%' });
    }
    const PLAN = data.plans || [];
    function plan() {
      const tc = PLAN.reduce((a, p) => a + p.accounts, 0) || 1, tr = PLAN.reduce((a, p) => a + p.revenue, 0) || 1;
      $('#smPlan').innerHTML = `<div class="sm-pm">${PLAN.map((p, i) => `<div class="row" style="--i:${i}"><b>${K.esc(p.name)}</b><span class="pr"><span class="t"><i style="width:${(p.accounts / tc * 100).toFixed(1)}%;background:${K.PAL[3]}"></i></span><span class="num">${K.nf(p.accounts / tc * 100, 1)}%</span></span><span class="pr"><span class="t"><i style="width:${(p.revenue / tr * 100).toFixed(1)}%;background:${K.PAL[1]}"></i></span><span class="num">${K.nf(p.revenue / tr * 100, 1)}%</span></span></div>`).join('')}</div>${K.legend([['حصة العملاء', K.PAL[3]], ['حصة الإيراد', K.PAL[1]]])}`;
      $('#smPlanT').innerHTML = `<table><thead><tr><th>الباقة</th><th>الحسابات</th><th>متوسط الإيراد</th><th>الإلغاء/شهر</th><th>صافي الاحتفاظ</th></tr></thead><tbody>${PLAN.map(p => `<tr><td>${K.esc(p.name)}</td><td><span class="num">${K.nf(p.accounts)}</span></td><td>${K.num(p.arpa, 'ر.س', 0)}</td><td><span class="dk-chip ${p.churnPct > 4 ? 'bad' : p.churnPct > 3 ? 'warn' : 'ok'}"><span class="num">${K.nf(p.churnPct, 2)}%</span></span></td><td><span class="num">${K.nf(p.nrr, 1)}%</span></td></tr>`).join('')}</tbody></table>`;
    }
    const INS = data.insights || [];
    function ins() { $('#smIns').innerHTML = INS.map((x, i) => `<button type="button" data-ins="${K.esc(x.anchor)}" style="--i:${i}"><span class="n num">${i + 1}</span><b>${K.esc(x.title)}</b><small>${K.esc(x.text)}</small></button>`).join(''); }
    function all() { kpis(); wf(); net(); coh(); plan(); ins(); }

    root.addEventListener('click', e => {
      const t = e.target; let x;
      if ((x = t.closest('[data-wf]'))) { const i = +x.dataset.wf; S.wf = S.wf === i ? null : i; return wf(); }
      if ((x = t.closest('[data-ins]'))) { const c = root.querySelector('#' + x.dataset.ins); if (!c) return; c.scrollIntoView({ behavior: K.RM ? 'auto' : 'smooth', block: 'center' }); c.classList.remove('pulse'); void c.offsetWidth; c.classList.add('pulse'); }
    });
    K.seg(root, 'rng', v => { S.rng = +v; kpis(); wf(); net(); });
    K.seg(root, 'coh', v => { S.coh = v; coh(); });
    K.onResize(root, () => { wf(); net(); });

    all();
    return { destroy() {} };
  }

  window.JEEM_SAAS_MOUNT = mount;
})();
