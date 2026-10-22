/* ==========================================================
   جيم — الإيرادات المتكررة والاحتفاظ (لوحة تفاعلية)
   مستوحاة من «SaaS MRR, Retention and Unit Economics» (Vision Labs):
   شلال الدخل الشهري بدل رقم نمو واحد، صافي جديد أسبوعي، دفعات احتفاظ،
   مزيج باقات (عملاء مقابل إيراد)، وملاحظات قراءة تُبرز الرسم الذي يثبتها.
   ========================================================== */
(function () {
  const root = document.getElementById('smRoot'); if (!root || !window.DK) return;
  const K = DK, $ = s => root.querySelector(s);
  const S = { rng: 13, coh: 'logo', wf: null };
  const DEF = [['الدخل الشهري الافتتاحي', 'الدخل المتكرر من الاشتراكات النشطة في بداية المدى. يستثني الرسوم لمرة واحدة والخدمات والاستخدام الزائد.'], ['دخل جديد', 'دخل من عملاء لم يكن لديهم اشتراك نشط في بداية الفترة.'], ['توسّع', 'زيادة دخل العملاء الحاليين: ترقيات، مقاعد إضافية، انتقال لباقة أعلى.'], ['انكماش', 'انخفاض دخل عملاء بقوا: تخفيض الباقة أو إزالة مقاعد.'], ['إلغاء', 'دخل مفقود من عملاء ألغوا بالكامل خلال الفترة.'], ['الدخل الشهري الختامي', 'الافتتاحي + الجديد + التوسع − الانكماش − الإلغاء.']];
  const DATA = { 13: [453000, 118400, 41200, -17800, -38405], 26: [402000, 221300, 79800, -33600, -112105] };
  function kpis() {
    const [o, n, x, c, ch] = DATA[S.rng], cl = o + n + x + c + ch, nrr = (o + x + c + ch) / o * 100;
    $('#smKpis').innerHTML = [['الدخل الشهري المتكرر', K.money(cl), K.delta((cl / o - 1) * 100)], ['صافي الدخل الجديد', K.money(cl - o), ''], ['صافي الاحتفاظ بالإيراد', K.pct(nrr), `<span class="dk-chip ${nrr >= 100 ? 'ok' : 'warn'}">${nrr >= 100 ? 'فوق 100%' : 'تحت 100%'}</span>`], ['إلغاء الحسابات شهريًا', K.pct(3.78, 2), '<span class="dk-chip bad">مرتفع</span>'], ['متوسط الإيراد لكل حساب', K.money(306), ''], ['القيمة الدائمة ÷ تكلفة الاكتساب', `<span class="num">3.5x</span>`, '<span class="dk-chip ok">فوق 3x</span>'], ['فترة الاسترداد', K.num(7.5, 'شهر', 1), '']]
      .map(([n, v, x], i) => `<div class="dk-kpi" style="--i:${i}"><small>${n}</small><b>${v}</b><span class="row">${x}</span></div>`).join('');
  }
  function wf() {
    const [o, n, x, c, ch] = DATA[S.rng];
    $('#smWf').innerHTML = K.waterfall([{ label: 'افتتاحي', val: o, kind: 'total' }, { label: 'جديد', val: n }, { label: 'توسّع', val: x }, { label: 'انكماش', val: c }, { label: 'إلغاء', val: ch }, { label: 'ختامي', val: o + n + x + c + ch, kind: 'total' }], { W: K.wOf($('#smWf')), H: 240, sel: S.wf, attr: 'wf', fmt: v => K.txt(v, 'ر.س') });
    const d = S.wf == null ? null : DEF[S.wf];
    $('#smDef').innerHTML = d ? `<b>${d[0]}</b> — ${d[1]}` : `<span class="mut">كل حركة مسؤولية فريق مختلف: الجديد للمبيعات، التوسع والانكماش لنجاح العملاء، الإلغاء للمنتج والدعم.</span>`;
  }
  function net() {
    const n = S.rng, r = K.rng('net' + n), lab = Array.from({ length: n }, (_, i) => { const d = new Date(2026, 3, 6 + i * 7 - (n - 13) * 7); return d.toLocaleDateString('ar-SA-u-ca-gregory-nu-latn', { day: 'numeric', month: 'short' }); });
    const v = lab.map(() => Math.round(4000 + r() * 12000)); v[9 + (n - 13)] = -4559;
    $('#smNet').innerHTML = K.bars({ W: K.wOf($('#smNet')), H: 220, labels: lab, series: [{ name: 'صافي جديد', color: K.PAL[0], vals: v }], neg: true, fmt: x => K.txt(x, 'ر.س'), tipf: (x, i) => x < 0 ? `${K.txt(x, 'ر.س')} — إلغاء حساب مؤسسي واحد بقيمة 16,432 ر.س` : K.txt(x, 'ر.س'), aria: 'صافي الدخل الجديد أسبوعيًا' });
    root.querySelectorAll('#smNet .b.ng').forEach(b => b.setAttribute('fill', '#D64545'));
  }
  const COH = ['أغسطس 25', 'سبتمبر 25', 'أكتوبر 25', 'نوفمبر 25', 'ديسمبر 25', 'يناير 26', 'فبراير 26'];
  function coh() {
    const cols = Array.from({ length: 13 }, (_, i) => 'ش' + i), rev = S.coh === 'rev';
    const v = COH.map((_, ci) => cols.map((_, m) => { const age = 12 - ci; if (m > age) return null; const decay = (rev ? .022 : .034) - ci * .0045; return Math.round(100 * Math.pow(1 - decay, m) + (rev && m > 3 ? m * .6 : 0)); }));
    $('#smCoh').innerHTML = K.heat(COH, cols, v, { min: 50, max: 105, hue: '0,168,159', fmt: x => x + '%' });
  }
  const PLAN = [['المبتدئة', 834, 43657, 52, 5.28, 85.6], ['النمو', 690, 101240, 147, 3.9, 94.2], ['الاحترافية', 192, 157300, 819, 2.6, 99.1], ['المؤسسات', 84, 255659, 3044, 2.02, 100.1]];
  function plan() {
    const tc = PLAN.reduce((a, p) => a + p[1], 0), tr = PLAN.reduce((a, p) => a + p[2], 0);
    $('#smPlan').innerHTML = `<div class="sm-pm">${PLAN.map(([n, c, r], i) => `<div class="row" style="--i:${i}"><b>${n}</b><span class="pr"><span class="t"><i style="width:${(c / tc * 100).toFixed(1)}%;background:${K.PAL[3]}"></i></span><span class="num">${K.nf(c / tc * 100, 1)}%</span></span><span class="pr"><span class="t"><i style="width:${(r / tr * 100).toFixed(1)}%;background:${K.PAL[1]}"></i></span><span class="num">${K.nf(r / tr * 100, 1)}%</span></span></div>`).join('')}</div>${K.legend([['حصة العملاء', K.PAL[3]], ['حصة الإيراد', K.PAL[1]]])}`;
    $('#smPlanT').innerHTML = `<table><thead><tr><th>الباقة</th><th>الحسابات</th><th>متوسط الإيراد</th><th>الإلغاء/شهر</th><th>صافي الاحتفاظ</th></tr></thead><tbody>${PLAN.map(([n, c, r, a, ch, nrr]) => `<tr><td>${n}</td><td><span class="num">${K.nf(c)}</span></td><td>${K.num(a, 'ر.س', 0)}</td><td><span class="dk-chip ${ch > 4 ? 'bad' : ch > 3 ? 'warn' : 'ok'}"><span class="num">${ch}%</span></span></td><td><span class="num">${nrr}%</span></td></tr>`).join('')}</tbody></table>`;
  }
  const INS = [['smWfC', 'الدخل ثابت بينما قاعدة الحسابات تتسرب', 'صافي الاحتفاظ قرابة 98.6% مع إلغاء حسابات 3.78% شهريًا — الحسابات الباقية تنمو بما يكفي لتعويض المغادرة.'], ['smPlanC', 'المبتدئة 46% من العملاء و7.8% من الإيراد', 'المؤسسات 4.6% من الحسابات و45.9% من المال، والمبتدئة الأعلى إلغاءً.'], ['smNetC', 'إلغاء مؤسسي واحد ظاهر بوضوح', 'الأسبوع السالب الوحيد (−4,559 ر.س) سببه حساب كبير واحد، لا مشكلة واسعة.'], ['smCohC', 'الاحتفاظ يتحسن من دفعة لأخرى', 'كل دفعة أحدث أعلى من سابقتها في نفس العمر — أثر المنتج والتهيئة.']];
  function ins() { $('#smIns').innerHTML = INS.map(([t, h, p], i) => `<button type="button" data-ins="${t}" style="--i:${i}"><span class="n num">${i + 1}</span><b>${h}</b><small>${p}</small></button>`).join(''); }
  function all() { kpis(); wf(); net(); coh(); plan(); ins(); }
  root.addEventListener('click', e => {
    const t = e.target; let x;
    if ((x = t.closest('[data-wf]'))) { const i = +x.dataset.wf; S.wf = S.wf === i ? null : i; return wf(); }
    if ((x = t.closest('[data-ins]'))) { const c = document.getElementById(x.dataset.ins); c.scrollIntoView({ behavior: K.RM ? 'auto' : 'smooth', block: 'center' }); c.classList.remove('pulse'); void c.offsetWidth; c.classList.add('pulse'); }
  });
  K.seg(root, 'rng', v => { S.rng = +v; kpis(); wf(); net(); });
  K.seg(root, 'coh', v => { S.coh = v; coh(); });
  K.onResize(root, () => { wf(); net(); });
  all();
})();
