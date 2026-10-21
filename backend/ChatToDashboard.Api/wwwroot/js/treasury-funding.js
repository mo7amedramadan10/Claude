/* ==========================================================
   جيم — التمويل والسيولة النقدية (لوحة تفاعلية، المحرك: funding، على dash-kit)
   مُعاد بناؤه كدالة تركيب قابلة لإعادة الاستخدام (window.JEEM_FUNDING_MOUNT) بدل IIFE ذاتي
   التشغيل ببيانات مولَّدة عشوائيًا بالكامل — يقبل data حقيقية أو من مكتبة العرض التجريبي
   (treasury-funding-demo.js). شريط أنظمة المدفوعات الحي: في وضع العرض التجريبي
   (opts.demo===true) فقط محاكاة تذبذب عشوائي كل 3 ثوانٍ؛ في الوضع الحقيقي يشترك في قناة
   JeemLive (opts.channel) لتحديثات حقيقية تُدفَع من الخادم، بلا أي توليد عشوائي محلي —
   نفس نمط warehouse-team.js's opts.channel/JeemLive بالضبط.
   ========================================================== */
(function () {
  function mount(root, data, opts = {}) {
    if (!root || !data || !window.DK) return;
    const demo = !!opts.demo;
    const K = DK, $ = s => root.querySelector(s);
    const NAVY = '#1F4E9C', GOLD = '#E3A21A';
    const years = data.maturities?.years?.length ? data.maturities.years : [new Date().getFullYear()];
    const S = { yr: years[0], mon: null, hide: new Set(), bank: null, live: !K.RM };
    let TK = (data.tick || []).map(t => ({ ...t }));

    const yrSeg = $('[data-seg="yr"]');
    if (yrSeg) yrSeg.innerHTML = years.map(y => `<button type="button" data-v="${y}" aria-pressed="${y === S.yr}">${y}</button>`).join('');

    function tick() { $('#tfTick').innerHTML = TK.map((t, i) => `<div class="tc" role="listitem" style="--i:${i}"><small>${K.esc(t.name)}</small><b>${K.num(t.value, 'ر.س', 2)}</b>${K.delta(t.deltaPct, 1, 0)}</div>`).join(''); }
    function mat() {
      const M = data.maturities?.byYear?.[S.yr] || { months: [], debt: [], cds: [], cap: [] };
      const rates = data.maturities?.rates?.[S.yr] || {};
      $('#tfRates').innerHTML = `<div><small>الدَّين</small><b style="color:${NAVY}">${K.pct(rates.debtPct ?? 0, 2)}</b></div><div><small>شهادات إيداع الوسطاء</small><b style="color:${GOLD}">${K.pct(rates.cdsPct ?? 0, 2)}</b></div>${S.mon != null && M.months[S.mon] != null ? `<div class="sel"><small>${K.esc(M.months[S.mon])}</small><b>${K.num((M.debt[S.mon] + M.cds[S.mon]), 'ر.س')}</b><button type="button" class="dk-lnk" data-mon="${S.mon}">إلغاء</button></div>` : ''}`;
      $('#tfMat').innerHTML = M.months.length ? K.bars({ W: K.wOf($('#tfMat')), H: 230, labels: M.months.map(m => String(m).slice(0, 3)), series: [{ name: 'الدَّين', color: NAVY, vals: M.debt }, { name: 'شهادات الإيداع', color: GOLD, vals: M.cds }], ghost: M.cap, key: 'mon', sel: S.mon, fmt: v => K.txt(v, 'ر.س'), aria: 'جدول الاستحقاقات' }) : '<p class="dk-empty">لا بيانات استحقاقات بعد.</p>';
      $('#tfMatLeg').innerHTML = K.legend([['الدَّين', NAVY], ['شهادات إيداع الوسطاء', GOLD], ['سقف الاستحقاق', '#E6EAF0']]);
    }
    const WH = data.financingRates || { labels: [], series: [] };
    function wh() {
      const series = WH.series.map((s, k) => ({ ...s, vals: S.hide.has(k) ? s.vals.map(() => null) : s.vals }));
      $('#tfWh').innerHTML = WH.labels.length ? K.line({ W: K.wOf($('#tfWh')), H: 220, labels: WH.labels, series, min0: false, fmt: v => K.nf(v, 2) + '%', aria: 'أسعار التمويل' }) : '<p class="dk-empty">لا بيانات أسعار بعد.</p>';
      $('#tfWhLeg').innerHTML = WH.series.map((s, k) => `<button type="button" data-hide="${k}" aria-pressed="${!S.hide.has(k)}"><i style="background:${s.color}"></i>${K.esc(s.name)}</button>`).join('');
    }
    function cash() {
      const C = data.cashPosition || [];
      $('#tfCash').innerHTML = C.map((c, i) => `<div class="row" style="--i:${i}"><span><small>${K.esc(c.label)}</small><b class="dk-count">${K.num(c.value, 'ر.س', 0)}</b>${K.delta(c.deltaPct, 1, 0)}</span><span class="mb" aria-hidden="true">${(c.spark || []).map((h, j) => `<i style="height:${h}%;background:${K.esc(c.color || K.PAL[0])};opacity:${.45 + j * .13};--j:${j}"></i>`).join('')}</span></div>`).join('');
      K.count($('#tfCash'));
    }
    const BK = data.banks || [];
    function bank() {
      const tot = BK.reduce((a, b) => a + b.value, 0) || 1, s = S.bank;
      $('#tfBank').innerHTML = `<div class="tot"><small>إجمالي الرصيد</small><b>${K.num(s == null ? tot : BK[s]?.value || 0, 'ر.س', 0)}</b><small>${s == null ? 'كل البنوك' : K.esc(BK[s]?.name || '')}</small></div>` +
        K.donut(BK.map((b, i) => ({ label: b.name, val: b.value, color: b.color, key: i })), { size: 170, th: 24, sel: s, attr: 'bank', tip: it => K.txt(it.val, 'ر.س'), center: s == null ? `<b class="num">${BK.length}</b><small>بنوك</small>` : `<b class="num">${K.nf(BK[s].value / tot * 100, 1)}%</b><small>من الإجمالي</small>` }) +
        `<ul class="lg">${BK.map((b, i) => `<li><button type="button" data-bank="${i}" aria-pressed="${s === i}" class="${s != null && s !== i ? 'dim' : ''}"><i style="background:${b.color}"></i>${K.esc(b.name)}<b class="num">${K.nf(b.value / tot * 100, 0)}%</b></button></li>`).join('')}</ul>`;
    }
    function intra() {
      const I = data.intraday || { hours: [], today: [], yesterday: [] };
      $('#tfIntra').innerHTML = I.hours.length ? K.bars({ W: K.wOf($('#tfIntra')), H: 210, labels: I.hours, series: [{ name: 'اليوم', color: '#2B8CE6', vals: I.today }], ghost: I.yesterday, fmt: v => K.txt(v, 'ر.س'), tipf: (v, i) => `اليوم ${K.txt(v, 'ر.س')} · أمس ${K.txt(I.yesterday[i] ?? 0, 'ر.س')}`, aria: 'نشاط اليوم' }) : '<p class="dk-empty">لا بيانات نشاط اليوم بعد.</p>';
    }
    function all() { tick(); mat(); wh(); cash(); bank(); intra(); }

    root.addEventListener('click', e => {
      const t = e.target; let x;
      if ((x = t.closest('[data-mon]'))) { const i = +x.dataset.mon; S.mon = S.mon === i ? null : i; return mat(); }
      if ((x = t.closest('[data-hide]'))) { const k = +x.dataset.hide; S.hide.has(k) ? S.hide.delete(k) : (S.hide.size < 2 && S.hide.add(k)); return wh(); }
      if ((x = t.closest('[data-bank]'))) { const i = +x.dataset.bank; S.bank = S.bank === i ? null : i; return bank(); }
      if ((x = t.closest('[data-pause]'))) { S.live = !S.live; x.setAttribute('aria-pressed', !S.live); x.textContent = S.live ? 'إيقاف التحديث' : 'استئناف التحديث'; $('#tfLive').classList.toggle('off', !S.live); }
    });
    K.seg(root, 'yr', v => { S.yr = +v; S.mon = null; mat(); });

    let tickTimer = null;
    if (demo) {
      tickTimer = setInterval(() => {
        if (!S.live || document.hidden) return;
        const r = Math.random;
        TK.forEach(t => { const ch = (r() - .45) * .004; t.value = Math.round(t.value * (1 + ch)); t.deltaPct = Math.max(-20, Math.min(25, +(t.deltaPct + ch * 400).toFixed(0))); });
        tick(); root.querySelectorAll('.tc').forEach(c => c.classList.add('fl'));
      }, 3000);
    }
    let liveSub = null;
    if (!demo && opts.channel && window.JeemLive) {
      liveSub = window.JeemLive.subscribe(opts.channel, msg => {
        if (msg && msg.kind === 'payment_tick' && Array.isArray(msg.tick)) { TK = msg.tick; tick(); root.querySelectorAll('.tc').forEach(c => c.classList.add('fl')); }
      });
    }
    if (!S.live) $('#tfLive')?.classList.add('off');
    K.onResize(root, () => { mat(); wh(); intra(); });

    all();
    return { destroy() { if (tickTimer) clearInterval(tickTimer); liveSub?.close(); } };
  }

  window.JEEM_FUNDING_MOUNT = mount;
})();
