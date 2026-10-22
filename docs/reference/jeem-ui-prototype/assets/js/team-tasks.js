/* ==========================================================
   جيم — إدارة مهام الفريق (لوحة تفاعلية) — فاتح افتراضيًا + داكن بمفتاح
   مستوحاة من «Team Management Dashboard». الحركات: مؤقت حي للمهمة الحالية بحلقة تقدّم،
   خط «الآن» يتحرك في التقويم، أعمدة النشاط تنمو، حلقة المشاريع تُكتسح، تذكيرات تُشطب.
   ========================================================== */
(function () {
  const root = document.getElementById('tmRoot'); if (!root) return;
  const $ = (s, el = root) => el.querySelector(s), $$ = (s, el = root) => [...el.querySelectorAll(s)];
  const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
  const esc = s => String(s).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
  const RM = matchMedia('(prefers-reduced-motion: reduce)').matches;
  const pad = n => String(Math.floor(n)).padStart(2, '0');
  const hms = s => `${pad(s / 3600)}:${pad(s % 3600 / 60)}:${pad(s % 60)}`;
  const hm = m => `${pad(m / 60)}:${pad(m % 60)}`;
  const toast = msg => { let t = document.querySelector('.toast'); if (!t) { t = document.createElement('div'); t.className = 'toast'; t.setAttribute('role', 'status'); document.body.append(t); } t.innerHTML = `${ic('check')}<span>${esc(msg)}</span>`; requestAnimationFrame(() => t.classList.add('is-on')); clearTimeout(t._h); t._h = setTimeout(() => t.classList.remove('is-on'), 2600); };

  /* ---------- الزمن المحاكى: الثلاثاء 13 أكتوبر، يبدأ 10:28 ويتقدم حيًا ---------- */
  const T0 = Date.now(), START = 10 * 60 + 28;
  const nowMin = () => START + (Date.now() - T0) / 60000;
  const DAYS = [['الأحد', 11], ['الاثنين', 12], ['الثلاثاء', 13], ['الأربعاء', 14], ['الخميس', 15]], TODAY = 2;
  const PR = { high: ['عالية', 'bad'], med: ['متوسطة', 'mid'], low: ['منخفضة', 'ok'], done: ['منجزة', 'done'] };
  const PPL = ['سارة', 'فهد', 'نورة', 'خالد', 'ريم', 'ماجد'];

  /* ---------- البيانات التجريبية ---------- */
  const TASKS = [
    { id: 't1', n: 'اختيار لوحة الألوان', p: 'تطبيق الألعاب', spent: 4 * 3600 + 7 * 60 + 11, lim: 6 * 3600, fav: true },
    { id: 't2', n: 'تصميم صفحة الهبوط', p: 'تطبيق الضبط الموسيقي', spent: 3600 + 12 * 60, lim: 4 * 3600, fav: false },
    { id: 't3', n: 'تحليل المنافسين وظيفيًا', p: 'منصة طبيب', spent: 25 * 60, lim: 3 * 3600, fav: false }
  ];
  const S = { act: 't1', run: true, view: 'week', pri: 'all', open: null, theme: 'light' };
  const CAL = [
    [0, 9 * 60, 120, 'done', 'مراجعة الموجز', 'مراجعة أهداف المشروع ونطاقه', [0, 1], 18], [0, 13 * 60, 150, 'med', 'إنشاء لوحة الإلهام', 'جمع مراجع بصرية للهوية', [2], 4],
    [1, 9 * 60, 210, 'done', 'تصميم الخطوط والتخطيط', 'اختيار الخطوط وعناصر التخطيط', [3, 4], 9], [1, 14 * 60, 90, 'low', 'مراجعة مع العميل', 'عرض المسودة الأولى', [0], 2],
    [2, 9 * 60 + 30, 150, 'med', 'اختيار لوحة الألوان', 'إنشاء نظام ألوان متناسق', [1, 5], 3, 't1'], [2, 13 * 60, 120, 'high', 'نماذج الواجهات', 'تحويل الإطارات إلى واجهات نهائية', [2, 3], 7],
    [3, 9 * 60, 180, 'high', 'تصميم واجهة المستخدم', 'واجهات جذابة ومتسقة بصريًا', [0, 1, 2, 3], 34], [3, 13 * 60, 150, 'low', 'إنشاء نموذج أولي', 'نموذج تفاعلي لعرض التصميم', [4], 5],
    [4, 9 * 60, 90, 'low', 'اختبار المستخدمين', 'جلسات مع 5 مستخدمين', [5], 1], [4, 10 * 60 + 45, 150, 'high', 'عرض على العميل', 'عرض المشروع وجمع الملاحظات', [0, 2, 4], 12]
  ].map(([d, s, len, pr, n, desc, who, cm, task], i) => ({ id: 'e' + i, d, s, len, pr, n, desc, who, cm, task }));
  const MEET = [{ t: 10 * 60 + 45, n: 'عرض المشروع وجمع الملاحظات', k: 'video' }, { t: 13 * 60, n: 'اجتماع مع فريق تجربة المستخدم', k: 'video' }, { t: 15 * 60, n: 'تهيئة العضو الجديد', k: 'users' }];
  const REM = [{ t: 9 * 60 + 30, n: 'مراجعة نتائج الاختبار', p: 'low', d: true }, { t: 10 * 60 + 45, n: 'عرض على العميل', p: 'high' }, { t: 16 * 60 + 15, n: 'إضافة رابط المشروع للعميل', p: 'high' }, { t: 17 * 60, n: 'تحديث خطة الأسبوع القادم', p: 'low' }];
  const ACT = [92, 41, 70, 0, 0];
  const PROJ = [['تطبيق الألعاب', 44, '#127DBD'], ['متجر ماغنوم', 24, '#00BAB3'], ['منصة طبيب', 18, '#6E62B0'], ['منتصف الليل', 14, '#D9822B']];

  /* ---------- 1) المؤقت ---------- */
  function timer() {
    const t = TASKS.find(x => x.id === S.act), p = Math.min(1, t.spent / t.lim), R = 46, L = 2 * Math.PI * R;
    $('#tmTimer').innerHTML = `<div class="ttl"><b>${esc(t.n)}</b><small>${esc(t.p)}</small></div>
      <button type="button" class="big${S.run ? ' run' : ''}" data-toggle aria-label="${S.run ? 'إيقاف مؤقت' : 'تشغيل'} ${esc(t.n)}"><svg viewBox="0 0 110 110" aria-hidden="true"><circle cx="55" cy="55" r="${R}" class="tr"/><circle cx="55" cy="55" r="${R}" class="pg" stroke-dasharray="${(p * L).toFixed(1)} ${L}" transform="rotate(-90 55 55)"/></svg>${ic(S.run ? 'pause' : 'play', 'icon')}</button>
      <div class="tl"><span><small>المسجّل</small><b class="num" id="tmSpent">${hms(t.spent)}</b></span><span><small>الحد</small><b class="num">${hms(t.lim)}</b></span></div>`;
  }
  function tasks() {
    $('#tmTasks').innerHTML = TASKS.map(t => `<li class="${t.id === S.act ? 'on' : ''}"><button type="button" class="pl" data-act="${t.id}" aria-label="${t.id === S.act && S.run ? 'إيقاف' : 'تشغيل'} ${esc(t.n)}">${ic(t.id === S.act && S.run ? 'pause' : 'play')}</button><span><b>${esc(t.n)}</b><small>${esc(t.p)}</small></span><button type="button" class="st" data-fav="${t.id}" aria-pressed="${t.fav}" aria-label="تمييز ${esc(t.n)}">${ic('star')}</button></li>`).join('');
    $('#tmTaskN').textContent = TASKS.length;
  }

  /* ---------- 2) الاجتماعات ---------- */
  function meetings() {
    const nm = nowMin(), next = MEET.filter(m => m.t > nm).sort((a, b) => a.t - b.t)[0];
    $('#tmMeet').innerHTML = MEET.map(m => { const past = m.t + 45 < nm, soon = m === next, mins = Math.round(m.t - nm);
      return `<div class="mt${soon ? ' soon' : ''}${past ? ' past' : ''}"><span class="h"><span class="num">${hm(m.t)}</span>${ic(m.k)}</span><p>${esc(m.n)}</p>${soon ? `<em>بعد <span class="num">${mins >= 60 ? Math.floor(mins / 60) + ':' + pad(mins % 60) : mins}</span> ${mins >= 60 ? 'ساعة' : 'دقيقة'}</em>` : past ? '<em>انتهى</em>' : ''}</div>`; }).join('')
      + `<button type="button" class="mt add" data-meet>${ic('plus')}<span>جدولة اجتماع</span></button>`;
    $('#tmMeetN').textContent = MEET.length;
  }

  /* ---------- 3) التقويم ---------- */
  const H0 = 8 * 60, H1 = 17 * 60, PX = .9; /* بكسل لكل دقيقة */
  const match = e => S.pri === 'all' || e.pr === S.pri;
  function cal() {
    const days = S.view === 'today' ? [TODAY] : [0, 1, 2, 3, 4], nm = nowMin();
    if (S.view === 'month') return month();
    $('#tmCal').className = 'tm-cal' + (S.view === 'today' ? ' one' : '');
    $('#tmCal').innerHTML = `<div class="hrs">${Array.from({ length: (H1 - H0) / 60 + 1 }, (_, i) => `<span style="top:${i * 60 * PX}px" class="num">${hm(H0 + i * 60)}</span>`).join('')}</div>`
      + days.map(d => `<section class="dc${d === TODAY ? ' today' : ''}" aria-label="${DAYS[d][0]} ${DAYS[d][1]}"><header><span>${DAYS[d][0]}</span><b class="num">${DAYS[d][1]}</b></header>
        <div class="slots" style="height:${(H1 - H0) * PX}px"><div class="brk" style="top:${(12 * 60 - H0) * PX}px;height:${60 * PX}px">${ic('clock')}استراحة</div>
        ${CAL.filter(e => e.d === d && match(e)).map((e, i) => { const run = e.task && e.task === S.act && S.run, t = e.task && TASKS.find(x => x.id === e.task);
          return `<button type="button" class="ev p-${PR[e.pr][1]}${S.open === e.id ? ' open' : ''}${run ? ' run' : ''}" data-ev="${e.id}" style="top:${(e.s - H0) * PX}px;height:${e.len * PX - 4}px;--i:${i}" aria-label="${esc(e.n)}، ${hm(e.s)}–${hm(e.s + e.len)}، أولوية ${PR[e.pr][0]}">
            <span class="pr"><i></i>${PR[e.pr][0]}</span><b>${esc(e.n)}</b><small>${esc(e.desc)}</small>
            <span class="ft"><span class="avs">${e.who.slice(0, 3).map(w => `<i style="--h:${w * 55 + 190}">${PPL[w][0]}</i>`).join('')}${e.who.length > 3 ? `<i class="more">+${e.who.length - 3}</i>` : ''}</span>${run ? `<span class="tmr num" data-live>${hms(t.spent).slice(0, 5)}</span>` : ''}<span class="cm">${ic('chat')}<span class="num">${e.cm}</span></span></span></button>`; }).join('')}
        ${d === TODAY && nm >= H0 && nm <= H1 ? `<div class="now" id="tmNow" style="top:${(nm - H0) * PX}px"><span class="num">${hm(nm)}</span></div>` : ''}</div></section>`).join('');
  }
  function month() {
    $('#tmCal').className = 'tm-cal month';
    const first = new Date(2026, 9, 1).getDay(), cells = [];
    for (let i = 0; i < first; i++) cells.push('<span class="mc x"></span>');
    for (let d = 1; d <= 31; d++) { const wd = (first + d - 1) % 7, ev = wd > 4 ? 0 : ((d * 7) % 5) + 1, isT = d === 13;
      cells.push(`<button type="button" class="mc${isT ? ' today' : ''}${wd > 4 ? ' we' : ''}" data-day="${d}" aria-label="${d} أكتوبر: ${ev} مهام"><b class="num">${d}</b>${ev ? `<span class="dots">${Array.from({ length: Math.min(ev, 4) }, (_, j) => `<i class="${['bad', 'mid', 'ok', 'done'][(d + j) % 4]}"></i>`).join('')}</span><small class="num">${ev} مهام</small>` : ''}</button>`); }
    $('#tmCal').innerHTML = `<div class="mh">${['الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت'].map(d => `<span>${d}</span>`).join('')}</div><div class="mg">${cells.join('')}</div>`;
  }
  function evPanel() {
    const e = CAL.find(x => x.id === S.open), P = $('#tmEv');
    if (!e) { P.hidden = true; return; }
    P.hidden = false; P.innerHTML = `<header><span class="pr p-${PR[e.pr][1]}"><i></i>${PR[e.pr][0]}</span><button type="button" class="x" data-close aria-label="إغلاق">${ic('x')}</button></header><h3>${esc(e.n)}</h3><p>${esc(e.desc)}</p>
      <ul><li>${ic('calendar')}${DAYS[e.d][0]} ${DAYS[e.d][1]} أكتوبر · <span class="num">${hm(e.s)}–${hm(e.s + e.len)}</span></li><li>${ic('users')}${e.who.map(w => PPL[w]).join('، ')}</li><li>${ic('chat')}<span class="num">${e.cm}</span> تعليقًا</li></ul>
      <div class="act">${e.pr !== 'done' ? `<button type="button" class="btn btn-sm" data-done="${e.id}">${ic('check')}تحديد كمنجزة</button>` : ''}${e.task ? `<button type="button" class="btn btn-sm btn-primary" data-act="${e.task}">${ic('play')}تشغيل المؤقت</button>` : ''}</div>`;
  }

  /* ---------- 4) الشريط الجانبي ---------- */
  function activity() {
    const avg = Math.round(ACT.slice(0, TODAY + 1).reduce((a, b) => a + b, 0) / (TODAY + 1));
    $('#tmActV').textContent = avg + '%';
    $('#tmAct').innerHTML = ACT.map((v, i) => `<div class="ab${i === TODAY ? ' today' : ''}${i > TODAY ? ' fut' : ''}" style="--h:${i > TODAY ? 100 : v}%;--i:${i}" data-tip="${DAYS[i][0]}|${i > TODAY ? 'لم يبدأ بعد' : v + '% من ساعات العمل'}"><i></i><span>${DAYS[i][0].replace('ال', '')}</span><b class="num">${i > TODAY ? '—' : v + '%'}</b></div>`).join('');
  }
  function projects(hl) {
    const R = 40, L = 2 * Math.PI * R; let acc = 0;
    $('#tmProj').innerHTML = `<div class="dn"><svg viewBox="0 0 100 100" role="img" aria-label="توزيع الوقت على 4 مشاريع">${PROJ.map(([n, v, c], i) => { const len = v / 100 * L - 4, s = `<circle cx="50" cy="50" r="${R}" stroke="${c}" stroke-dasharray="${len.toFixed(1)} ${L}" stroke-dashoffset="${(-acc).toFixed(1)}" transform="rotate(-90 50 50)" class="${hl != null && hl !== i ? 'dim' : ''}" style="--i:${i}" data-pj="${i}"/>`; acc += v / 100 * L; return s; }).join('')}</svg><span><b class="num">${hl != null ? PROJ[hl][1] + '%' : PROJ.length}</b><small>${hl != null ? esc(PROJ[hl][0]) : 'مشاريع'}</small></span></div>
      <ul>${PROJ.map(([n, v, c], i) => `<li><button type="button" data-pj="${i}" class="${hl === i ? 'on' : ''}"><i style="border-color:${c}"></i>${esc(n)}<b class="num">${v}%</b></button></li>`).join('')}</ul>`;
  }
  function reminders() {
    const nm = nowMin();
    $('#tmRem').innerHTML = REM.slice().sort((a, b) => a.t - b.t).map((r, i) => `<li class="${r.d ? 'dn' : ''}${!r.d && r.t < nm ? ' late' : ''}"><label><input type="checkbox" data-rem="${REM.indexOf(r)}" ${r.d ? 'checked' : ''}><span class="t num">${hm(r.t)}</span><span class="n">${esc(r.n)}</span></label><span class="pr p-${r.p === 'high' ? 'bad' : 'ok'}"><i></i>${r.p === 'high' ? 'عالية' : 'منخفضة'}</span></li>`).join('');
  }

  /* ---------- الأحداث ---------- */
  root.addEventListener('click', e => {
    const t = e.target;
    if (t.closest('[data-toggle]')) { S.run = !S.run; timer(); tasks(); return cal(); }
    const a = t.closest('[data-act]'); if (a) { const id = a.dataset.act; if (S.act === id) S.run = !S.run; else { S.act = id; S.run = true; toast(`بدأ المؤقت: ${TASKS.find(x => x.id === id).n}`); } timer(); tasks(); cal(); return; }
    const f = t.closest('[data-fav]'); if (f) { const x = TASKS.find(y => y.id === f.dataset.fav); x.fav = !x.fav; return tasks(); }
    const v = t.closest('[data-view]'); if (v) { S.view = v.dataset.view; $$('[data-view]').forEach(b => b.setAttribute('aria-pressed', String(b === v))); S.open = null; evPanel(); return cal(); }
    const ev = t.closest('[data-ev]'); if (ev) { S.open = S.open === ev.dataset.ev ? null : ev.dataset.ev; cal(); return evPanel(); }
    if (t.closest('[data-close]')) { S.open = null; cal(); return evPanel(); }
    const dn = t.closest('[data-done]'); if (dn) { const x = CAL.find(y => y.id === dn.dataset.done); x.pr = 'done'; ACT[TODAY] = Math.min(100, ACT[TODAY] + 6); activity(); cal(); evPanel(); return toast(`أُنجزت «${x.n}»`); }
    const md = t.closest('[data-day]'); if (md) { S.view = 'week'; $$('[data-view]').forEach(b => b.setAttribute('aria-pressed', String(b.dataset.view === 'week'))); return cal(); }
    const pj = t.closest('[data-pj]'); if (pj) { const i = +pj.dataset.pj; projects(S.pj === i ? (S.pj = null) : (S.pj = i)); return; }
    if (t.closest('[data-meet]')) { const nm = Math.ceil(nowMin() / 15) * 15 + 60; MEET.push({ t: nm, n: 'اجتماع جديد مع الفريق', k: 'video' }); meetings(); return toast(`جُدول اجتماع الساعة ${hm(nm)}`); }
    const th = t.closest('[data-theme-set]'); if (th) return theme(th.dataset.themeSet);
    if (t.closest('[data-manage]')) return toast('تفتح هنا صفحة إدارة المهام أو التذكيرات كاملة.');
  });
  root.addEventListener('change', e => {
    if (e.target.id === 'tmPri') { S.pri = e.target.value; return cal(); }
    const r = e.target.dataset.rem; if (r != null) { REM[+r].d = e.target.checked; reminders(); }
  });
  root.addEventListener('pointerover', e => { const p = e.target.closest('#tmProj [data-pj]'); if (p && S.pj == null) projects(+p.dataset.pj); });
  root.addEventListener('pointerout', e => { const p = e.target.closest('#tmProj'); if (p && !p.contains(e.relatedTarget) && S.pj == null) projects(null); });
  addEventListener('keydown', e => { if (e.key === 'Escape' && S.open) { S.open = null; cal(); evPanel(); } });
  function theme(t) { S.theme = t; root.classList.toggle('tm-dark', t === 'dark'); $$('[data-theme-set]').forEach(b => b.setAttribute('aria-pressed', String(b.dataset.themeSet === t))); try { localStorage.setItem('jeem.tm.theme', t); } catch (e) {} }

  /* ---------- النبض الحي: كل ثانية ---------- */
  setInterval(() => {
    if (document.hidden) return;
    if (S.run) { const t = TASKS.find(x => x.id === S.act); t.spent++; const el = $('#tmSpent'); if (el) el.textContent = hms(t.spent); const pg = $('#tmTimer .pg'); if (pg) { const L = 2 * Math.PI * 46; pg.setAttribute('stroke-dasharray', `${(Math.min(1, t.spent / t.lim) * L).toFixed(1)} ${L}`); } const lv = $('[data-live]'); if (lv) lv.textContent = hms(t.spent).slice(0, 5); }
    const n = $('#tmNow'), nm = nowMin(); if (n) { n.style.top = (nm - H0) * PX + 'px'; n.querySelector('span').textContent = hm(nm); }
    $('#tmClock').textContent = hm(nm);
  }, 1000);
  setInterval(() => { if (!document.hidden) { meetings(); reminders(); } }, 30000);

  let th = 'light'; try { th = localStorage.getItem('jeem.tm.theme') || 'light'; } catch (e) {}
  theme(th); timer(); tasks(); meetings(); cal(); activity(); projects(null); reminders(); $('#tmClock').textContent = hm(nowMin());
})();
