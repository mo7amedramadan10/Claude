/* ==========================================================
   جيم — إدارة مهام الفريق (لوحة تفاعلية، المحرك: teamtasks) — فاتح افتراضيًا + داكن
   مُعاد بناؤه كدالة تركيب (window.JEEM_TEAMTASKS_MOUNT) بدل IIFE ذاتي التشغيل ببيانات
   ثابتة — المهام/التقويم/الاجتماعات/التذكيرات/النشاط/المشاريع كلها من data، والوقت
   الحالي ويوم الأسبوع الحاليان حقيقيان (لا محاكاة) حسب الطلب: "في الإنتاج: الوقت
   الفعلي ومنطقة المستخدم". عرض الشهر يعرض فقط الأيام اللي معاها بيانات حقيقية
   (data.monthActivity) بدل اختلاق أرقام — المعاينة التجريبية فقط بتوفّرها.
   ========================================================== */
(function () {
  function mount(root, data, opts = {}) {
    if (!root || !data) return;
    const $ = (s, el = root) => el.querySelector(s), $$ = (s, el = root) => [...el.querySelectorAll(s)];
    const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
    const esc = s => String(s).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
    const RM = matchMedia('(prefers-reduced-motion: reduce)').matches;
    const pad = n => String(Math.floor(n)).padStart(2, '0');
    const hms = s => `${pad(s / 3600)}:${pad(s % 3600 / 60)}:${pad(s % 60)}`;
    const hm = m => `${pad(m / 60)}:${pad(m % 60)}`;
    const toast = msg => {
      let t = document.querySelector('.toast');
      if (!t) { t = document.createElement('div'); t.className = 'toast'; t.setAttribute('role', 'status'); document.body.append(t); }
      t.innerHTML = `${ic('check')}<span>${esc(msg)}</span>`;
      requestAnimationFrame(() => t.classList.add('is-on'));
      clearTimeout(t._h); t._h = setTimeout(() => t.classList.remove('is-on'), 2600);
    };
    const hashHue = id => { let h = 0; for (const c of String(id)) h = (h * 31 + c.charCodeAt(0)) % 360; return h; };
    const ini = n => String(n).split(' ').slice(0, 2).map(x => x.replace(/^ال/, '')[0] || '').join('‌');

    /* ---------- الوقت الحقيقي: أسبوع العمل الأحد–الخميس للأسبوع الحالي ---------- */
    const DAYNAMES = ['الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس'];
    const MONTHNAMES = ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو', 'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر'];
    const now0 = new Date(), dow0 = now0.getDay();
    const sunday = new Date(now0); sunday.setDate(now0.getDate() - dow0);
    const DAYS = DAYNAMES.map((n, i) => { const d = new Date(sunday); d.setDate(sunday.getDate() + i); return [n, d.getDate()]; });
    const TODAY = dow0 <= 4 ? dow0 : -1;
    const nowMin = () => { const d = new Date(); return d.getHours() * 60 + d.getMinutes() + d.getSeconds() / 60; };
    const PR = { high: ['عالية', 'bad'], med: ['متوسطة', 'mid'], low: ['منخفضة', 'ok'], done: ['منجزة', 'done'] };

    /* ---------- البيانات ---------- */
    const TASKS = (data.tasks || []).map(t => ({ id: t.id, n: t.title, p: t.project || '', spent: t.spentSec || 0, lim: t.dailyLimitSec || 3600, fav: !!t.favorite }));
    const S = { act: data.activeTaskId || TASKS[0]?.id, run: !!data.running, view: 'week', pri: 'all', open: null, theme: 'light', pj: null };
    const CAL = (data.calendarItems || []).map(e => ({ id: e.id, d: e.day, s: e.startMin, len: e.durationMin, pr: e.priority, n: e.title, desc: e.description || '', who: e.assignees || [], cm: e.commentsCount || 0, task: e.taskId || null }));
    const MEET = (data.meetings || []).map(m => ({ id: m.id, t: m.startMin, n: m.title, k: m.kind || 'video' }));
    const REM = (data.reminders || []).map(r => ({ id: r.id, t: r.atMin, n: r.title, p: r.priority, d: !!r.done }));
    const ACT = data.activityDaily || [];
    const PROJ = data.projectTimeShare || [];
    const MONTH_ACT = data.monthActivity || [];

    /* ---------- الترويسة ---------- */
    const dateEl = $('#tmDate');
    if (dateEl) dateEl.textContent = `${DAYNAMES[TODAY] || DAYS[0][0]} ${now0.getDate()} ${MONTHNAMES[now0.getMonth()]}`;

    /* ---------- 1) المؤقت ---------- */
    function timer() {
      const t = TASKS.find(x => x.id === S.act);
      if (!t) { $('#tmTimer').innerHTML = `<div class="ttl"><b>لا توجد مهمة نشطة</b></div>`; return; }
      const p = Math.min(1, t.spent / t.lim), R = 46, L = 2 * Math.PI * R;
      $('#tmTimer').innerHTML = `<div class="ttl"><b>${esc(t.n)}</b><small>${esc(t.p)}</small></div>
        <button type="button" class="big${S.run ? ' run' : ''}" data-toggle aria-label="${S.run ? 'إيقاف مؤقت' : 'تشغيل'} ${esc(t.n)}"><svg viewBox="0 0 110 110" aria-hidden="true"><circle cx="55" cy="55" r="${R}" class="tr"/><circle cx="55" cy="55" r="${R}" class="pg" stroke-dasharray="${(p * L).toFixed(1)} ${L}" transform="rotate(-90 55 55)"/></svg>${ic(S.run ? 'pause' : 'play', 'icon')}</button>
        <div class="tl"><span><small>المسجّل</small><b class="num" id="tmSpent">${hms(t.spent)}</b></span><span><small>الحد</small><b class="num">${hms(t.lim)}</b></span></div>`;
    }
    function tasks() {
      $('#tmTasks').innerHTML = TASKS.map(t => `<li class="${t.id === S.act ? 'on' : ''}"><button type="button" class="pl" data-act="${esc(t.id)}" aria-label="${t.id === S.act && S.run ? 'إيقاف' : 'تشغيل'} ${esc(t.n)}">${ic(t.id === S.act && S.run ? 'pause' : 'play')}</button><span><b>${esc(t.n)}</b><small>${esc(t.p)}</small></span><button type="button" class="st" data-fav="${esc(t.id)}" aria-pressed="${t.fav}" aria-label="تمييز ${esc(t.n)}">${ic('star')}</button></li>`).join('');
      $('#tmTaskN').textContent = TASKS.length;
    }

    /* ---------- 2) الاجتماعات ---------- */
    function meetings() {
      const nm = nowMin(), next = MEET.filter(m => m.t > nm).sort((a, b) => a.t - b.t)[0];
      $('#tmMeet').innerHTML = MEET.map(m => {
        const past = m.t + 45 < nm, soon = m === next, mins = Math.round(m.t - nm);
        return `<div class="mt${soon ? ' soon' : ''}${past ? ' past' : ''}"><span class="h"><span class="num">${hm(m.t)}</span>${ic(m.k)}</span><p>${esc(m.n)}</p>${soon ? `<em>بعد <span class="num">${mins >= 60 ? Math.floor(mins / 60) + ':' + pad(mins % 60) : mins}</span> ${mins >= 60 ? 'ساعة' : 'دقيقة'}</em>` : past ? '<em>انتهى</em>' : ''}</div>`;
      }).join('') + `<button type="button" class="mt add" data-meet>${ic('plus')}<span>جدولة اجتماع</span></button>`;
      $('#tmMeetN').textContent = MEET.length;
    }

    /* ---------- 3) التقويم ---------- */
    const H0 = 8 * 60, H1 = 17 * 60, PX = .9;
    const match = e => S.pri === 'all' || e.pr === S.pri;
    function cal() {
      if (S.view === 'month') return month();
      const days = S.view === 'today' ? [TODAY >= 0 ? TODAY : 0] : [0, 1, 2, 3, 4], nm = nowMin();
      $('#tmCal').className = 'tm-cal' + (S.view === 'today' ? ' one' : '');
      $('#tmCal').innerHTML = `<div class="hrs">${Array.from({ length: (H1 - H0) / 60 + 1 }, (_, i) => `<span style="top:${i * 60 * PX}px" class="num">${hm(H0 + i * 60)}</span>`).join('')}</div>`
        + days.map(d => `<section class="dc${d === TODAY ? ' today' : ''}" aria-label="${esc(DAYS[d][0])} ${DAYS[d][1]}"><header><span>${esc(DAYS[d][0])}</span><b class="num">${DAYS[d][1]}</b></header>
          <div class="slots" style="height:${(H1 - H0) * PX}px"><div class="brk" style="top:${(12 * 60 - H0) * PX}px;height:${60 * PX}px">${ic('clock')}استراحة</div>
          ${CAL.filter(e => e.d === d && match(e)).map((e, i) => {
            const run = e.task && e.task === S.act && S.run, t = e.task && TASKS.find(x => x.id === e.task);
            return `<button type="button" class="ev p-${(PR[e.pr] || PR.low)[1]}${S.open === e.id ? ' open' : ''}${run ? ' run' : ''}" data-ev="${esc(e.id)}" style="top:${(e.s - H0) * PX}px;height:${e.len * PX - 4}px;--i:${i}" aria-label="${esc(e.n)}، ${hm(e.s)}–${hm(e.s + e.len)}، أولوية ${(PR[e.pr] || PR.low)[0]}">
              <span class="pr"><i></i>${(PR[e.pr] || PR.low)[0]}</span><b>${esc(e.n)}</b><small>${esc(e.desc)}</small>
              <span class="ft"><span class="avs">${e.who.slice(0, 3).map(w => `<i style="--h:${hashHue(w)}">${esc(ini(w))}</i>`).join('')}${e.who.length > 3 ? `<i class="more">+${e.who.length - 3}</i>` : ''}</span>${run ? `<span class="tmr num" data-live>${hms(t.spent).slice(0, 5)}</span>` : ''}<span class="cm">${ic('chat')}<span class="num">${e.cm}</span></span></span></button>`;
          }).join('')}
          ${d === TODAY && nm >= H0 && nm <= H1 ? `<div class="now" id="tmNow" style="top:${(nm - H0) * PX}px"><span class="num">${hm(nm)}</span></div>` : ''}</div></section>`).join('');
    }
    function month() {
      $('#tmCal').className = 'tm-cal month';
      const y = now0.getFullYear(), m = now0.getMonth(), numDays = new Date(y, m + 1, 0).getDate(), first = new Date(y, m, 1).getDay(), cells = [];
      const byDay = Object.fromEntries(MONTH_ACT.map(x => [x.day, x]));
      for (let i = 0; i < first; i++) cells.push('<span class="mc x"></span>');
      for (let d = 1; d <= numDays; d++) {
        const wd = (first + d - 1) % 7, isT = d === now0.getDate(), ma = byDay[d];
        cells.push(`<button type="button" class="mc${isT ? ' today' : ''}${wd > 4 ? ' we' : ''}" data-day="${d}" aria-label="${d} ${MONTHNAMES[m]}${ma ? `: ${ma.count} مهام` : ''}"><b class="num">${d}</b>${ma && ma.count ? `<span class="dots">${(ma.priorities || []).slice(0, 4).map(p => `<i class="${p}"></i>`).join('')}</span><small class="num">${ma.count} مهام</small>` : ''}</button>`);
      }
      $('#tmCal').innerHTML = `<div class="mh">${['الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت'].map(d => `<span>${d}</span>`).join('')}</div><div class="mg">${cells.join('')}</div>`;
    }
    function evPanel() {
      const e = CAL.find(x => x.id === S.open), P = $('#tmEv');
      if (!e) { P.hidden = true; return; }
      P.hidden = false;
      P.innerHTML = `<header><span class="pr p-${(PR[e.pr] || PR.low)[1]}"><i></i>${(PR[e.pr] || PR.low)[0]}</span><button type="button" class="x" data-close aria-label="إغلاق">${ic('x')}</button></header><h3>${esc(e.n)}</h3><p>${esc(e.desc)}</p>
        <ul><li>${ic('calendar')}${esc(DAYS[e.d]?.[0] || '')} ${DAYS[e.d]?.[1] || ''} · <span class="num">${hm(e.s)}–${hm(e.s + e.len)}</span></li><li>${ic('users')}${e.who.map(esc).join('، ') || '—'}</li><li>${ic('chat')}<span class="num">${e.cm}</span> تعليقًا</li></ul>
        <div class="act">${e.pr !== 'done' ? `<button type="button" class="btn btn-sm" data-done="${esc(e.id)}">${ic('check')}تحديد كمنجزة</button>` : ''}${e.task ? `<button type="button" class="btn btn-sm btn-primary" data-act="${esc(e.task)}">${ic('play')}تشغيل المؤقت</button>` : ''}</div>`;
    }

    /* ---------- 4) الشريط الجانبي ---------- */
    function activity() {
      const seen = ACT.filter((_, i) => i <= TODAY);
      const avg = seen.length ? Math.round(seen.reduce((a, b) => a + (b.pct || 0), 0) / seen.length) : 0;
      $('#tmActV').textContent = avg + '%';
      $('#tmAct').innerHTML = DAYNAMES.map((n, i) => {
        const rec = ACT.find(a => a.day === i), fut = i > TODAY || !rec;
        return `<div class="ab${i === TODAY ? ' today' : ''}${fut ? ' fut' : ''}" style="--h:${fut ? 100 : rec.pct}%;--i:${i}" data-tip="${esc(n)}|${fut ? 'لم يبدأ بعد' : rec.pct + '% من ساعات العمل'}"><i></i><span>${esc(n.replace('ال', ''))}</span><b class="num">${fut ? '—' : rec.pct + '%'}</b></div>`;
      }).join('');
    }
    function projects(hl) {
      const R = 40, L = 2 * Math.PI * R; let acc = 0;
      $('#tmProj').innerHTML = `<div class="dn"><svg viewBox="0 0 100 100" role="img" aria-label="توزيع الوقت على ${PROJ.length} مشاريع">${PROJ.map((p, i) => {
        const len = p.pct / 100 * L - 4, s = `<circle cx="50" cy="50" r="${R}" stroke="${esc(p.color)}" stroke-dasharray="${len.toFixed(1)} ${L}" stroke-dashoffset="${(-acc).toFixed(1)}" transform="rotate(-90 50 50)" class="${hl != null && hl !== i ? 'dim' : ''}" style="--i:${i}" data-pj="${i}"/>`;
        acc += p.pct / 100 * L; return s;
      }).join('')}</svg><span><b class="num">${hl != null ? PROJ[hl].pct + '%' : PROJ.length}</b><small>${hl != null ? esc(PROJ[hl].name) : 'مشاريع'}</small></span></div>
        <ul>${PROJ.map((p, i) => `<li><button type="button" data-pj="${i}" class="${hl === i ? 'on' : ''}"><i style="border-color:${esc(p.color)}"></i>${esc(p.name)}<b class="num">${p.pct}%</b></button></li>`).join('')}</ul>`;
    }
    function reminders() {
      const nm = nowMin();
      $('#tmRem').innerHTML = REM.slice().sort((a, b) => a.t - b.t).map(r => `<li class="${r.d ? 'dn' : ''}${!r.d && r.t < nm ? ' late' : ''}"><label><input type="checkbox" data-rem="${esc(r.id)}" ${r.d ? 'checked' : ''}><span class="t num">${hm(r.t)}</span><span class="n">${esc(r.n)}</span></label><span class="pr p-${r.p === 'high' ? 'bad' : 'ok'}"><i></i>${r.p === 'high' ? 'عالية' : 'منخفضة'}</span></li>`).join('');
    }

    /* ---------- الأحداث ---------- */
    root.addEventListener('click', e => {
      const t = e.target;
      if (t.closest('[data-toggle]')) { S.run = !S.run; timer(); tasks(); opts.onAction?.('toggleTimer', { taskId: S.act, running: S.run }); return cal(); }
      const a = t.closest('[data-act]'); if (a) {
        const id = a.dataset.act;
        if (S.act === id) S.run = !S.run;
        else { S.act = id; S.run = true; toast(`بدأ المؤقت: ${TASKS.find(x => x.id === id)?.n || ''}`); }
        timer(); tasks(); cal(); opts.onAction?.('toggleTimer', { taskId: S.act, running: S.run }); return;
      }
      const f = t.closest('[data-fav]'); if (f) { const x = TASKS.find(y => y.id === f.dataset.fav); if (x) { x.fav = !x.fav; opts.onAction?.('toggleFavorite', { taskId: x.id, favorite: x.fav }); } return tasks(); }
      const v = t.closest('[data-view]'); if (v) { S.view = v.dataset.view; $$('[data-view]').forEach(b => b.setAttribute('aria-pressed', String(b === v))); S.open = null; evPanel(); return cal(); }
      const ev = t.closest('[data-ev]'); if (ev) { S.open = S.open === ev.dataset.ev ? null : ev.dataset.ev; cal(); return evPanel(); }
      if (t.closest('[data-close]')) { S.open = null; cal(); return evPanel(); }
      const dn = t.closest('[data-done]'); if (dn) {
        const x = CAL.find(y => y.id === dn.dataset.done); if (!x) return;
        x.pr = 'done';
        const rec = ACT.find(a => a.day === TODAY); if (rec) rec.pct = Math.min(100, (rec.pct || 0) + 6);
        activity(); cal(); evPanel(); opts.onAction?.('completeTask', { itemId: x.id }); return toast(`أُنجزت «${x.n}»`);
      }
      const md = t.closest('[data-day]'); if (md) { S.view = 'week'; $$('[data-view]').forEach(b => b.setAttribute('aria-pressed', String(b.dataset.view === 'week'))); return cal(); }
      const pj = t.closest('[data-pj]'); if (pj) { const i = +pj.dataset.pj; projects(S.pj === i ? (S.pj = null) : (S.pj = i)); return; }
      if (t.closest('[data-meet]')) { const nm = Math.ceil(nowMin() / 15) * 15 + 60; const mt = { id: 'm_' + Date.now(), t: nm, n: 'اجتماع جديد مع الفريق', k: 'video' }; MEET.push(mt); meetings(); opts.onAction?.('scheduleMeeting', { startMin: nm }); return toast(`جُدول اجتماع الساعة ${hm(nm)}`); }
      const th = t.closest('[data-theme-set]'); if (th) return theme(th.dataset.themeSet);
      if (t.closest('[data-manage]')) return toast('تفتح هنا صفحة إدارة المهام أو التذكيرات كاملة.');
    });
    root.addEventListener('change', e => {
      if (e.target.id === 'tmPri') { S.pri = e.target.value; return cal(); }
      const r = e.target.dataset.rem; if (r != null) { const x = REM.find(y => y.id === r); if (x) { x.d = e.target.checked; opts.onAction?.('toggleReminder', { reminderId: x.id, done: x.d }); reminders(); } }
    });
    root.addEventListener('pointerover', e => { const p = e.target.closest('#tmProj [data-pj]'); if (p && S.pj == null) projects(+p.dataset.pj); });
    root.addEventListener('pointerout', e => { const p = e.target.closest('#tmProj'); if (p && !p.contains(e.relatedTarget) && S.pj == null) projects(null); });
    const onKeydown = e => { if (e.key === 'Escape' && S.open) { S.open = null; cal(); evPanel(); } };
    document.addEventListener('keydown', onKeydown);
    function theme(t) {
      S.theme = t; root.classList.toggle('tm-dark', t === 'dark');
      $$('[data-theme-set]').forEach(b => b.setAttribute('aria-pressed', String(b.dataset.themeSet === t)));
      try { localStorage.setItem('jeem.tm.theme', t); } catch (e) { }
    }

    /* ---------- النبض الحي: كل ثانية ---------- */
    const tick1 = setInterval(() => {
      if (document.hidden) return;
      if (S.run) {
        const t = TASKS.find(x => x.id === S.act);
        if (t) {
          t.spent++;
          const el = $('#tmSpent'); if (el) el.textContent = hms(t.spent);
          const pg = $('#tmTimer .pg'); if (pg) { const L = 2 * Math.PI * 46; pg.setAttribute('stroke-dasharray', `${(Math.min(1, t.spent / t.lim) * L).toFixed(1)} ${L}`); }
          const lv = $('[data-live]'); if (lv) lv.textContent = hms(t.spent).slice(0, 5);
        }
      }
      const n = $('#tmNow'), nm = nowMin(); if (n) { n.style.top = (nm - H0) * PX + 'px'; n.querySelector('span').textContent = hm(nm); }
      const clk = $('#tmClock'); if (clk) clk.textContent = hm(nm);
    }, 1000);
    const tick2 = setInterval(() => { if (!document.hidden) { meetings(); reminders(); } }, 30000);

    let th = 'light'; try { th = localStorage.getItem('jeem.tm.theme') || 'light'; } catch (e) { }
    theme(th); timer(); tasks(); meetings(); cal(); activity(); projects(null); reminders();
    const clk0 = $('#tmClock'); if (clk0) clk0.textContent = hm(nowMin());

    return { destroy() { clearInterval(tick1); clearInterval(tick2); document.removeEventListener('keydown', onKeydown); } };
  }

  window.JEEM_TEAMTASKS_MOUNT = mount;
})();
