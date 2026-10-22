/* ==========================================================
   جيم — لوحتان تفاعليتان تعرضان أنواع الرسوم الجديدة في محرك JR:
   1) المبيعات والتسويق: حالة فارغة ← «اعرض ببيانات تجريبية» ← البيانات تدخل بحركة (عدّاد دوّار).
   2) تقييم أداء الموردين: رادار لمقارنة حتى 3 موردين على 6 محاور.
   ========================================================== */
(function () {
  const root = document.getElementById('scRoot'); if (!root || !window.JR) return;
  const $ = s => root.querySelector(s), $$ = s => [...root.querySelectorAll(s)];
  const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
  const toast = msg => { let t = document.querySelector('.toast'); if (!t) { t = document.createElement('div'); t.className = 'toast'; t.setAttribute('role', 'status'); document.body.append(t); } t.innerHTML = `${ic('check')}<span>${msg}</span>`; requestAnimationFrame(() => t.classList.add('is-on')); clearTimeout(t._h); t._h = setTimeout(() => t.classList.remove('is-on'), 2600); };
  const grid = $('#scGrid');

  /* ================= 1) المبيعات والتسويق ================= */
  if (root.dataset.page === 'sales') {
    const S = { demo: false, per: 'm' };
    const page = () => ({ n: 'نظرة عامة', w: [
      { t: 'kpi', ti: 'إجمالي الدخل', ic: 'wallet', v: S.per === 'm' ? '1,248,320' : '3,912,640', u: 'ر.س', d: '+12.9%' },
      { t: 'kpi', ti: 'صافي الربح', ic: 'chart', v: S.per === 'm' ? '384,520' : '1,106,300', u: 'ر.س', d: '-0.3%' },
      { t: 'kpi', ti: 'المشاهدات', ic: 'users', v: S.per === 'm' ? '5,211,832' : '15,604,210', d: '+10.3%' },
      { t: 'kpi', ti: 'معدل التحويل', ic: 'percent', v: '4.83', u: '%', d: '+8.1%' },
      { t: 'line', ti: 'الإيرادات مقابل المستهدف', su: 'المستهدف خط متقطع حتى يُقرأ كمرجع لا كسلسلة ثانية', s: ['الإيرادات', 'المستهدف'], dash: [1], x: S.per === 'm' ? 'M12' : 'Q4', k: 'sar', b: S.per === 'm' ? 1.1e6 : 3.4e6, sp: 8 },
      { t: 'iconrank', ti: 'الجلسات حسب الدولة', su: 'أعلى 5 دول', x: 'CNTRY:5', av: ['SA', 'AE', 'EG', 'KW', 'QA'], sp: 4 },
      { t: 'bars', ti: 'المبيعات حسب المنطقة', su: 'بدل الرسم العنكبوتي: الأعمدة أوضح لمقارنة القيم', x: 'REG', k: 'sar', b: 800000, spot: 'max', sp: 4 },
      { t: 'dco', ti: 'المبيعات حسب المنصة', x: ['سلة', 'زد', 'نون', 'أمازون'], vals: [45, 25, 18, 12], tot: S.per === 'm' ? '2.4م' : '7.1م', tl: 'ر.س', sp: 4 },
      { t: 'semi', ti: 'المستخدمون المسجلون', su: 'تقسيم الإجمالي حسب الباقة', x: ['مميز', 'أساسي'], vals: S.per === 'm' ? [1809, 515] : [5260, 1490], tl: 'مستخدم', sp: 4 },
      { t: 'ribbon', ti: 'المبيعات حسب القناة عبر الفترات', su: 'الشرائط تتبع حصة كل قناة من فترة لأخرى', x: S.per === 'm' ? ['يوليو', 'أغسطس', 'سبتمبر'] : 'Q4', s: ['الفروع', 'المتجر الإلكتروني', 'تطبيق الجوال', 'المناديب'], k: 'sar', b: S.per === 'm' ? 520000 : 1500000, sp: 12 }
    ] });
    const draw = (anim) => {
      grid.innerHTML = JR.renderPage({ id: 'sales-sc' }, page(), S.per, { empty: !S.demo });
      if (S.demo) { $$('.jr-w').forEach(w => w.classList.add('sc-in')); JR.animate(grid, { roll: anim }); }
      root.classList.toggle('is-demo', S.demo);
      $('#scState').innerHTML = S.demo ? `${ic('spark')}بيانات تجريبية — ليست أرقامًا حقيقية` : `${ic('database')}لا يوجد مصدر بيانات مرتبط`;
      $('[data-demo]').setAttribute('aria-pressed', String(S.demo)); $('[data-demo] span').textContent = S.demo ? 'إيقاف البيانات التجريبية' : 'اعرض ببيانات تجريبية';
    };
    root.addEventListener('click', e => {
      const t = e.target;
      if (t.closest('[data-demo]')) { S.demo = !S.demo; return draw(true); }
      const p = t.closest('[data-per]'); if (p) { S.per = p.dataset.per; $$('[data-per]').forEach(b => b.setAttribute('aria-pressed', String(b === p))); return draw(true); }
      if (t.closest('[data-connect]')) return toast('هنا يفتح معالج ربط المصدر: قاعدة بيانات، Excel، أو واجهة برمجية.');
    });
    draw(false);
  }

  /* ================= 2) تقييم أداء الموردين ================= */
  if (root.dataset.page === 'sup') {
    const AX = ['الجودة', 'الالتزام بالمواعيد', 'السعر', 'الاستجابة', 'الامتثال', 'الاستدامة'];
    const SUP = [
      ['شركة الحلول المتقدمة', 'ح', [92, 84, 70, 88, 95, 72]], ['مجموعة الخليج', 'خ', [78, 91, 85, 74, 82, 60]], ['مؤسسة التوريد الأولى', 'ت', [65, 70, 93, 68, 77, 55]],
      ['شركة البناء الحديث', 'ب', [84, 62, 76, 80, 71, 83]], ['مصنع الشرق', 'ش', [70, 79, 68, 62, 90, 74]], ['شركة الإمداد الوطنية', 'م', [88, 86, 64, 90, 86, 91]]
    ];
    const COL = ['var(--chart-1)', 'var(--chart-2)', '#D9822B'];
    const avg = v => Math.round(v.reduce((a, b) => a + b, 0) / v.length);
    const S = { sel: [0, 1] };
    const radarW = () => ({ t: 'radar', ti: 'ملف الأداء على 6 محاور', su: 'اختر حتى 3 موردين للمقارنة', x: AX, s: S.sel.map(i => SUP[i][0]), vals: S.sel.map(i => SUP[i][2]), cols: S.sel.map((_, j) => COL[j]), sp: 7 });
    const ranked = SUP.map((s, i) => ({ i, v: avg(s[2]) })).sort((a, b) => b.v - a.v);
    const page = { n: 'التقييم', w: [
      { t: 'kpi', ti: 'موردون مقيَّمون', ic: 'users', v: '42', d: '+6', n: 'هذا الربع' },
      { t: 'kpi', ti: 'متوسط التقييم', ic: 'target', v: '78', u: 'من 100', d: '+3.2%' },
      { t: 'kpi', ti: 'تحت الحد الأدنى (60)', ic: 'info', v: '5', d: '-2', g: true, n: 'عن الربع السابق' },
      { t: 'kpi', ti: 'عقود تنتهي خلال 90 يومًا', ic: 'calendar', v: '7', d: '', n: 'تحتاج قرار تجديد' },
      radarW(),
      { t: 'iconrank', ti: 'التقييم الإجمالي', su: 'اضغط موردًا لإضافته للمقارنة', x: ranked.map(r => SUP[r.i][0]), vals: ranked.map(r => r.v), av: ranked.map(r => SUP[r.i][1]), nopct: true, u: 'من 100', sp: 5 },
      { t: 'semi', ti: 'حالة العقود', x: ['سارية', 'تنتهي قريبًا', 'منتهية'], vals: [31, 7, 4], cols: ['var(--chart-1)', '#D9822B', '#B9C2CB'], tl: 'عقدًا', sp: 4 },
      { t: 'bullet', ti: 'الالتزام بالمواعيد مقابل المستهدف', su: 'نسبة التسليم في الموعد · المستهدف 90%', it: SUP.map(s => [s[0], s[2][1], 90]), hi: true, risk: .85, sp: 8 }
    ] };
    grid.innerHTML = JR.renderPage({ id: 'sup-sc' }, page, ''); JR.animate(grid);
    const radarCard = () => grid.querySelector('[data-type="radar"]');
    function redrawRadar() {
      const tmp = document.createElement('div'); tmp.innerHTML = JR.renderPage({ id: 'sup-sc' }, { n: 'x', w: [radarW()] }, '');
      radarCard().querySelector('.jr-body').replaceWith(tmp.querySelector('.jr-body'));
      $$('[data-sup]').forEach(b => { const i = +b.dataset.sup, on = S.sel.includes(i); b.setAttribute('aria-pressed', String(on)); b.disabled = !on && S.sel.length >= 3; b.style.setProperty('--c', on ? COL[S.sel.indexOf(i)] : 'transparent'); });
      grid.querySelectorAll('[data-type="iconrank"] li').forEach((li, j) => li.classList.toggle('sc-on', S.sel.includes(ranked[j].i)));
    }
    const toggle = i => { if (S.sel.includes(i)) { if (S.sel.length > 1) S.sel = S.sel.filter(x => x !== i); } else if (S.sel.length < 3) S.sel = S.sel.concat([i]); else return toast('يمكن مقارنة 3 موردين كحد أقصى. أزل واحدًا أولًا.'); redrawRadar(); };
    $('#scSup').innerHTML = SUP.map(([n], i) => `<button type="button" class="sc-chip" data-sup="${i}" aria-pressed="false"><i></i>${n}</button>`).join('');
    root.addEventListener('click', e => {
      const b = e.target.closest('[data-sup]'); if (b) return toggle(+b.dataset.sup);
      const li = e.target.closest('[data-type="iconrank"] li'); if (li) { const j = [...li.parentNode.children].indexOf(li); return toggle(ranked[j].i); }
    });
    redrawRadar();
  }
  /* ================= 3) أداء فريق المبيعات ================= */
  if (root.dataset.page === 'team') {
    const nf = v => Number(v).toLocaleString('en-US');
    const PER = { jan: 'يناير', feb: 'فبراير', mar: 'مارس' };
    /* المندوب: [الاسم، الدور، {الفترة: [الإيراد بالألف، % من الهدف، الهدف بالألف]}] */
    const REPS = [
      ['سارة العتيبي', 'مندوبة أولى', { jan: [412, 103, 400], feb: [388, 97, 400], mar: [452, 113, 400] }],
      ['محمد الحربي', 'مندوب', { jan: [298, 85, 350], feb: [341, 97, 350], mar: [326, 93, 350] }],
      ['نورة الشمري', 'مندوبة', { jan: [276, 92, 300], feb: [231, 77, 300], mar: [305, 102, 300] }],
      ['يوسف الغامدي', 'مندوب', { jan: [188, 63, 300], feb: [204, 68, 300], mar: [246, 82, 300] }],
      ['ريم القحطاني', 'مندوبة', { jan: [254, 85, 300], feb: [279, 93, 300], mar: [231, 77, 300] }],
      ['فهد الدوسري', 'مندوب جديد', { jan: [96, 48, 200], feb: [132, 66, 200], mar: [158, 79, 200] }]
    ];
    const CUST = ['مستشفى الحمادي', 'مجموعة صافولا', 'جامعة الملك سعود', 'شركة المراعي', 'مدارس الرواد', 'بنك البلاد', 'تجمع الرياض الصحي', 'شركة جرير', 'أمانة جدة', 'مصرف الراجحي'];
    const PKG = ['باقة المؤسسات', 'باقة الأعمال', 'باقة احترافية', 'تجديد سنوي', 'خدمات تنفيذ'];
    const STS = ['ok', 'ok', 'pend', 'ok', 'bad', 'pend', 'ok', 'info', 'ok', 'pend'];
    const S = { per: 'feb', rep: null };
    const team = () => { const L = REPS.map(r => r[2][S.per]); const rev = L.reduce((a, b) => a + b[0], 0), tg = L.reduce((a, b) => a + b[2], 0); return { rev, tg, p: Math.round(rev / tg * 100) }; };
    const txs = () => CUST.map((c, i) => { const r = REPS[(i * 7 + S.per.length) % REPS.length]; return [c, `${PKG[(i + S.per.length) % PKG.length]} · ${r[0]}`, nf(Math.round((18 + ((i * 37 + S.per.charCodeAt(0)) % 90)) * 1000)) + ' ر.س', STS[(i + S.per.charCodeAt(1)) % STS.length], `${(i % 9) + 1} ${PER[S.per]}`, r[0]]; });
    const goalW = () => { const T = team(); if (S.rep != null) { const r = REPS[S.rep], d = r[2][S.per]; return { t: 'goal', ti: 'رسالة الهدف', su: 'تُكتب تلقائيًا من البيانات', p: d[1], per: PER[S.per], who: r[0], rem: d[1] >= 100 ? '' : nf((d[2] - d[0]) * 1000) + ' ر.س', sp: 4 }; }
      return { t: 'goal', ti: 'رسالة الهدف', su: 'تُكتب تلقائيًا من البيانات', p: T.p, per: PER[S.per], who: 'الفريق', rem: T.p >= 100 ? '' : nf((T.tg - T.rev) * 1000) + ' ر.س', days: S.per === 'mar' ? 9 : '', sp: 4 }; };
    const feedW = () => ({ t: 'feed', ti: 'آخر المعاملات', su: S.rep != null ? `معاملات ${REPS[S.rep][0]}` : 'كل الفريق', it: txs().filter(t => S.rep == null || t[5] === REPS[S.rep][0]).slice(0, 6), sp: 4 });
    const page = () => { const T = team(), mult = { jan: .92, feb: 1, mar: 1.08 }[S.per]; return { n: 'الفريق', w: [
      { t: 'kpi', ti: 'إيرادات الفريق', ic: 'wallet', v: nf(T.rev * 1000), u: 'ر.س', d: S.per === 'jan' ? '+4.1%' : S.per === 'feb' ? '+2.6%' : '+11.8%', tg: T.p, tgv: nf(T.tg * 1000), per: `${PER[S.per]} 2026` },
      { t: 'kpi', ti: 'الصفقات المغلقة', ic: 'check', v: String(Math.round(118 * mult)), d: '+6', tg: Math.round(70 * mult), tgv: '160', per: `${PER[S.per]} 2026` },
      { t: 'kpi', ti: 'العملاء الجدد', ic: 'users', v: String(Math.round(42 * mult)), d: S.per === 'feb' ? '-3' : '+5', tg: Math.round(65 * mult), tgv: '70', per: `${PER[S.per]} 2026`, risk: 80 },
      goalW(),
      { t: 'people', ti: 'أداء المندوبين', su: 'اضغط مندوبًا لترى رسالته ومعاملاته', it: REPS.map(r => [r[0], r[2][S.per][0] * 1000, r[2][S.per][1], r[1]]), k: 'sar', u: 'ر.س', sp: 8 },
      { t: 'stacked', ti: 'المبيعات اليومية حسب القناة', x: 'DAY7', s: ['مباشر', 'شركاء', 'المتجر الإلكتروني'], k: 'sar', b: 60000 * mult, sp: 8 },
      feedW()
    ] }; };
    const swap = (type, w) => { const tmp = document.createElement('div'); tmp.innerHTML = JR.renderPage({ id: 'team-sc' }, { n: 'x', w: [w] }, S.per); const card = grid.querySelector(`[data-type="${type}"]`); card.querySelector('.w-head').replaceWith(tmp.querySelector('.w-head')); card.querySelector('.jr-body').replaceWith(tmp.querySelector('.jr-body')); };
    const draw = () => { grid.innerHTML = JR.renderPage({ id: 'team-sc' }, page(), S.per); JR.animate(grid, { roll: true }); mark(); };
    const mark = () => grid.querySelectorAll('[data-person]').forEach(li => { const on = S.rep != null && li.dataset.person === REPS[S.rep][0]; li.classList.toggle('on', on); li.setAttribute('aria-pressed', String(on)); });
    const pick = name => { const i = REPS.findIndex(r => r[0] === name); S.rep = S.rep === i ? null : i; swap('goal', goalW()); swap('feed', feedW()); mark(); };
    root.addEventListener('click', e => {
      const p = e.target.closest('[data-per]'); if (p) { S.per = p.dataset.per; $$('[data-per]').forEach(b => b.setAttribute('aria-pressed', String(b === p))); return draw(); }
      const li = e.target.closest('[data-person]'); if (li) return pick(li.dataset.person);
    });
    root.addEventListener('keydown', e => { const li = e.target.closest('[data-person]'); if (li && (e.key === 'Enter' || e.key === ' ')) { e.preventDefault(); pick(li.dataset.person); } });
    draw();
  }
})();
