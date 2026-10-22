/* ==========================================================
   جيم — التمويل والسيولة النقدية (لوحة تفاعلية)
   مستوحاة من «Charts and Stats»: أعمدة استحقاق مجمّعة بخلفية سقف، خطوط أسعار
   قابلة للإخفاء، شريط أنظمة مدفوعات يتحدث حيًا، مركز نقدي، حلقة بنوك، ونشاط الساعات مقابل أمس.
   ========================================================== */
(function () {
  const root = document.getElementById('tfRoot'); if (!root || !window.DK) return;
  const K = DK, $ = s => root.querySelector(s);
  const NAVY = '#1F4E9C', GOLD = '#E3A21A';
  const S = { yr: 2026, mon: null, hide: new Set(), bank: null, live: !K.RM };
  const MON = ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو', 'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر'];
  const TK = [['سريع', 989859145], ['تسوية الأوراق المالية', 990102001], ['نظام المقاصة', 117859145], ['سداد', 989859145], ['الشيكات', 989859145]].map(([n, v], i) => ({ n, v, d: [12, 15, 12, 12, 12][i] }));

  function tick() { $('#tfTick').innerHTML = TK.map((t, i) => `<div class="tc" role="listitem" style="--i:${i}"><small>${t.n}</small><b>${K.num(t.v, 'ر.س', 2)}</b>${K.delta(t.d, 1, 0)}</div>`).join(''); }
  function mat() {
    const r = K.rng('mat' + S.yr), debt = MON.map(() => Math.round(260 + r() * 70)), cds = MON.map(() => Math.round(220 + r() * 50)), cap = MON.map((_, i) => Math.max(debt[i], cds[i]) + 60 + Math.round(r() * 40));
    debt[4] += 120; cds[4] += 90;
    $('#tfRates').innerHTML = `<div><small>الدَّين</small><b style="color:${NAVY}">${K.pct(S.yr === 2026 ? 5.41 : 5.12, 2)}</b></div><div><small>شهادات إيداع الوسطاء</small><b style="color:${GOLD}">${K.pct(S.yr === 2026 ? 5.13 : 4.96, 2)}</b></div>${S.mon != null ? `<div class="sel"><small>${MON[S.mon]}</small><b>${K.num((debt[S.mon] + cds[S.mon]) * 1e6, 'ر.س')}</b><button type="button" class="dk-lnk" data-mon="${S.mon}">إلغاء</button></div>` : ''}`;
    $('#tfMat').innerHTML = K.bars({ W: K.wOf($('#tfMat')), H: 230, labels: MON.map(m => m.slice(0, 3)), series: [{ name: 'الدَّين', color: NAVY, vals: debt.map(v => v * 1e6) }, { name: 'شهادات الإيداع', color: GOLD, vals: cds.map(v => v * 1e6) }], ghost: cap.map(v => v * 1e6), key: 'mon', sel: S.mon, fmt: v => K.txt(v, 'ر.س'), aria: 'جدول الاستحقاقات' });
    $('#tfMatLeg').innerHTML = K.legend([['الدَّين', NAVY], ['شهادات إيداع الوسطاء', GOLD], ['سقف الاستحقاق', '#E6EAF0']]);
  }
  const WH = [['أسعار الأسواق (سايبور 3 أشهر)', GOLD, 2.8], ['سلف البنك المركزي', NAVY, 1.9], ['شهادات إيداع الوسطاء', '#28B6E6', 1.4]];
  function wh() {
    const n = 26, labels = Array.from({ length: n }, (_, i) => MON[Math.floor(i / 26 * 7)].slice(0, 3) + ' ' + (1 + (i * 7) % 28));
    const series = WH.map(([nm, c, b], k) => { const r = K.rng('wh' + k); let v = b; return { name: nm, color: c, vals: S.hide.has(k) ? labels.map(() => null) : labels.map(() => (v = Math.max(.3, v + (r() - .5) * .35)) && +v.toFixed(2)) }; });
    $('#tfWh').innerHTML = K.line({ W: K.wOf($('#tfWh')), H: 220, labels, series, min0: false, fmt: v => K.nf(v, 2) + '%', aria: 'أسعار التمويل' });
    $('#tfWhLeg').innerHTML = WH.map(([n, c], k) => `<button type="button" data-hide="${k}" aria-pressed="${!S.hide.has(k)}"><i style="background:${c}"></i>${n}</button>`).join('');
  }
  function cash() {
    const C = [['الرصيد الافتتاحي', 950000, 12, NAVY], ['الرصيد الحالي', 1523125, 12, '#28B6E6'], ['حركة جديدة', 102120, 13, '#E8794A']];
    $('#tfCash').innerHTML = C.map(([n, v, d, c], i) => { const r = K.rng('c' + i), b = Array.from({ length: 5 }, (_, j) => 30 + j * 14 + r() * 10); return `<div class="row" style="--i:${i}"><span><small>${n}</small><b class="dk-count">${K.num(v, 'ر.س', 0)}</b>${K.delta(d, 1, 0)}</span><span class="mb" aria-hidden="true">${b.map((h, j) => `<i style="height:${h}%;background:${c};opacity:${.45 + j * .13};--j:${j}"></i>`).join('')}</span></div>`; }).join('');
    K.count($('#tfCash'));
  }
  const BK = [['البنك الأهلي', 412300, K.PAL[0]], ['مصرف الراجحي', 298400, '#28B6E6'], ['بنك الرياض', 176800, '#9DB4CC'], ['البنك السعودي الفرنسي', 121500, '#1F4E9C'], ['بنك البلاد', 64263, GOLD], ['بنك الجزيرة', 52000, '#D7DDE5']];
  function bank() {
    const tot = BK.reduce((a, b) => a + b[1], 0), s = S.bank;
    $('#tfBank').innerHTML = `<div class="tot"><small>إجمالي الرصيد</small><b>${K.num(s == null ? tot : BK[s][1], 'ر.س', 0)}</b><small>${s == null ? 'كل البنوك' : BK[s][0]}</small></div>` + K.donut(BK.map(([n, v, c], i) => ({ label: n, val: v, color: c, key: i })), { size: 170, th: 24, sel: s, attr: 'bank', tip: it => K.txt(it.val, 'ر.س'), center: s == null ? `<b class="num">${BK.length}</b><small>بنوك</small>` : `<b class="num">${K.nf(BK[s][1] / tot * 100, 1)}%</b><small>من الإجمالي</small>` }) + `<ul class="lg">${BK.map(([n, v, c], i) => `<li><button type="button" data-bank="${i}" aria-pressed="${s === i}" class="${s != null && s !== i ? 'dim' : ''}"><i style="background:${c}"></i>${n}<b class="num">${K.nf(v / tot * 100, 0)}%</b></button></li>`).join('')}</ul>`;
  }
  function intra() {
    const hrs = Array.from({ length: 12 }, (_, i) => `${8 + i}:00`), r = K.rng('intra'), today = hrs.map((_, i) => Math.round((80 + Math.sin(i / 2) * 60 + r() * 60) * 1e6)), yest = today.map(v => Math.round(v * (1.05 + r() * .4)));
    $('#tfIntra').innerHTML = K.bars({ W: K.wOf($('#tfIntra')), H: 210, labels: hrs, series: [{ name: 'اليوم', color: '#2B8CE6', vals: today }], ghost: yest, fmt: v => K.txt(v, 'ر.س'), tipf: (v, i) => `اليوم ${K.txt(v, 'ر.س')} · أمس ${K.txt(yest[i], 'ر.س')}`, aria: 'نشاط اليوم' });
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
  /* تحديث حي لشريط الأنظمة كل 3 ثوانٍ */
  setInterval(() => { if (!S.live || document.hidden) return; const r = Math.random; TK.forEach(t => { const ch = (r() - .45) * .004; t.v = Math.round(t.v * (1 + ch)); t.d = Math.max(-20, Math.min(25, +(t.d + ch * 400).toFixed(0))); }); tick(); root.querySelectorAll('.tc').forEach(c => { c.classList.add('fl'); }); }, 3000);
  if (!S.live) $('#tfLive').classList.add('off');
  K.onResize(root, () => { mat(); wh(); intra(); });
  all();
})();
