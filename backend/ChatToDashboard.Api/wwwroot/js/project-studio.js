/* ==========================================================
   جيم — استوديو المشاريع (لوحة تفاعلية، المحرك: studio، على dash-kit)
   مُعاد بناؤه كدالة تركيب قابلة لإعادة الاستخدام (window.JEEM_STUDIO_MOUNT) بدل IIFE ذاتي
   التشغيل ببيانات ثابتة مضمّنة — يقبل data حقيقية أو من مكتبة العرض التجريبي
   (project-studio-demo.js). خط «اليوم» في الجدول الزمني يُحسب دومًا من التاريخ الفعلي
   الحالي (لا تاريخ تجريبي ثابت)، نفس نمط team-tasks/time-tracking. في وضع العرض التجريبي
   (opts.demo===true) فقط: ردّ تلقائي مُمثَّل على الرسالة المُرسَلة — في الوضع الحقيقي الرسالة
   المُرسَلة تُضاف للعرض لمدة الجلسة بلا أي ردّ مختلَق من زميل غير موجود.
   ========================================================== */
(function () {
  function mount(root, data, opts = {}) {
    if (!root || !data || !window.DK) return;
    const demo = !!opts.demo;
    const K = DK, $ = s => root.querySelector(s);
    const TEAM = data.team || [];
    const PROJECTS = data.projects || [];
    const ST = [
      ['up', 'قادمة', 'history', '#E8A13A'], ['done', 'مكتملة', 'check', '#2F6BFF'],
      ['prog', 'قيد التنفيذ', 'link', '#2F6BFF'], ['all', 'الإجمالي', 'layers', '#E8A13A'],
    ];
    const totals = data.statusTotals || {};
    const S = { st: null, who: null, view: 'grid', zoom: 1 };
    let TASKS = (data.todayTasks || []).map(t => ({ ...t }));
    const today = new Date(); today.setHours(0, 0, 0, 0);
    const winStart = new Date(today); winStart.setDate(winStart.getDate() - 1);
    const DAYS = Array.from({ length: 11 }, (_, i) => { const d = new Date(winStart); d.setDate(d.getDate() + i); return d; });
    const dayIdx = d => Math.round((d - winStart) / 86400000);
    const av = (w, sz = '') => { const m = TEAM[w]; if (!m) return ''; return `<span class="dk-av${sz}" style="--h:${w * 67 + 10}" title="${K.esc(m.name)}">${K.esc((m.name || '?')[0])}</span>`; };

    function status() {
      const cnt = k => k === 'all' ? PROJECTS.length : PROJECTS.filter(p => p.status === k).length;
      $('#psStatus').innerHTML = `<div class="ps-st ${S.view}">${ST.map(([k, n, ic, c], i) => `<button type="button" data-st="${k}" aria-pressed="${S.st === k}" class="${S.st && S.st !== k && k !== 'all' ? 'dim' : ''}" style="--c:${c};--i:${i}"><span class="ib">${K.ic(ic)}</span><small>${n}</small><b><span class="num">${totals[k] ?? cnt(k)}</span> مشروعًا</b><span class="mut">في الجدول: <span class="num">${cnt(k)}</span></span></button>`).join('')}</div>`;
    }
    function tasks() {
      $('#psTaskN').innerHTML = `<span class="num">${TASKS.filter(t => !t.done).length}</span> متبقية`;
      $('#psTasks').innerHTML = TASKS.map((t, i) => `<li class="${t.done ? 'done' : ''}" style="--i:${i}"><label><input type="checkbox" data-task="${i}" ${t.done ? 'checked' : ''}><span><b>${K.esc(t.title)}</b><small>المكلفون: ${(t.who || []).map(x => TEAM[x]?.name).filter(Boolean).join('، ')}</small></span></label><span class="dk-avs">${(t.who || []).map(x => av(x)).join('')}</span></li>`).join('');
    }
    function tl() {
      const colw = Math.round(86 * S.zoom), todayI = dayIdx(today);
      const rows = PROJECTS.filter(p => (!S.st || S.st === 'all' || p.status === S.st) && (S.who == null || (p.who || []).includes(S.who)))
        .map(p => ({ ...p, s: dayIdx(new Date(p.start)), e: dayIdx(new Date(p.end)) }));
      $('#psTl').innerHTML = `<div class="ps-gantt" style="--n:${DAYS.length};--cw:${colw}px"><div class="hd">${DAYS.map(d => `<span class="${dayIdx(d) === todayI ? 'td' : ''}"><b class="num">${String(d.getDate()).padStart(2, '0')}</b> ${d.toLocaleDateString('ar-SA-u-ca-gregory-nu-latn', { weekday: 'short' })}</span>`).join('')}</div>
        <div class="bd">${DAYS.map((d, i) => `<span class="cl${i % 2 ? ' alt' : ''}" style="inset-inline-start:${i * colw}px"></span>`).join('')}<span class="now" style="inset-inline-start:${(todayI + .45) * colw}px"><i>اليوم</i></span>
          ${rows.length ? rows.map((p, i) => `<button type="button" class="bar" style="inset-inline-start:${p.s * colw + 4}px;width:${(p.e - p.s + 1) * colw - 8}px;top:${14 + i * 54}px;--c:${p.color || K.PAL[0]};--i:${i}" data-tip="${K.esc(p.name)}|${ST.find(x => x[0] === p.status)?.[1] || ''}"><span><b>${K.esc(p.name)}</b><small>${new Date(p.start).toLocaleDateString('ar-SA-u-ca-gregory-nu-latn', { day: 'numeric', month: 'short' })} – ${new Date(p.end).toLocaleDateString('ar-SA-u-ca-gregory-nu-latn', { day: 'numeric', month: 'short' })}</small></span><span class="dk-avs">${(p.who || []).map(x => av(x)).join('')}</span></button>`).join('') : '<p class="dk-empty" style="padding-top:60px">لا مشاريع بهذه التصفية</p>'}</div></div>`;
      $('#psTl .bd').style.height = Math.max(200, 30 + rows.length * 54) + 'px';
      requestAnimationFrame(() => { const e = $('#psTl'); e.scrollLeft = -Math.max(0, (todayI - 1) * colw); });
    }
    function team() { $('#psTeam').innerHTML = TEAM.map((m, i) => `<li><button type="button" data-who="${i}" aria-pressed="${S.who === i}" class="${S.who != null && S.who !== i ? 'dim' : ''}">${av(i)}<span><b>${K.esc(m.name)}</b><small>${K.esc(m.role || '')}</small></span><span class="mut"><span class="num">${PROJECTS.filter(p => (p.who || []).includes(i)).length}</span> مشاريع</span></button></li>`).join(''); }
    let CHAT = (data.messages || []).map(m => ({ ...m }));
    function chat() {
      $('#psChat').innerHTML = CHAT.map(m => m.type === 'day' ? `<span class="day">${K.esc(m.label)}</span>` : `<div class="msg ${m.dir}">${m.kind === 'voice' ? `<button type="button" class="vc" data-voice aria-label="تشغيل الرسالة الصوتية">${K.ic('play')}<span class="wv">${Array.from({ length: 22 }, (_, i) => `<i style="--h:${30 + Math.abs(Math.sin(i * 1.7)) * 70}%;--i:${i}"></i>`).join('')}</span><span class="num">${K.esc(m.dur || '0:00')}</span></button>` : m.kind === 'img' ? `<span class="im" role="img" aria-label="صورة مرفقة"></span>` : `<p>${K.esc(m.text || '')}</p>`}<small>${K.esc(m.time || '')}</small></div>`).join('');
      const c = $('#psChat'); c.scrollTop = c.scrollHeight;
    }
    function scope() { const p = []; if (S.st && S.st !== 'all') p.push(['st', ST.find(x => x[0] === S.st)[1]]); if (S.who != null) p.push(['who', TEAM[S.who]?.name]); $('#psScope').innerHTML = p.map(([k, v]) => `<span class="dk-chip info">${K.esc(v)}<button type="button" data-rm="${k}" aria-label="إزالة">${K.ic('x')}</button></span>`).join(''); }
    function all() { status(); tasks(); tl(); team(); chat(); scope(); }

    root.addEventListener('click', e => {
      const t = e.target; let x;
      if ((x = t.closest('[data-st]'))) { const k = x.dataset.st; S.st = S.st === k || k === 'all' ? null : k; status(); tl(); return scope(); }
      if ((x = t.closest('[data-who]'))) { const i = +x.dataset.who; S.who = S.who === i ? null : i; team(); tl(); return scope(); }
      if ((x = t.closest('[data-rm]'))) { S[x.dataset.rm] = null; status(); team(); tl(); return scope(); }
      if ((x = t.closest('[data-voice]'))) { x.classList.toggle('on'); x.querySelector('use').setAttribute('href', x.classList.contains('on') ? '#i-pause' : '#i-play'); if (x.classList.contains('on')) setTimeout(() => { x.classList.remove('on'); x.querySelector('use')?.setAttribute('href', '#i-play'); }, 4000); return; }
    });
    root.addEventListener('change', e => { const c = e.target.closest('[data-task]'); if (c) { TASKS[+c.dataset.task].done = c.checked; tasks(); } });
    $('#psSend').addEventListener('submit', e => {
      e.preventDefault(); const v = e.target.m.value.trim(); if (!v) return;
      const tm = new Date().toLocaleTimeString('ar-SA-u-nu-latn', { hour: 'numeric', minute: '2-digit' });
      CHAT.push({ type: 'msg', dir: 'out', kind: 'text', text: v, time: tm }); e.target.reset(); chat();
      if (demo) setTimeout(() => { CHAT.push({ type: 'msg', dir: 'in', kind: 'text', text: 'تمام، سأراجعها الآن 👍', time: tm }); chat(); }, 1200);
    });
    K.seg(root, 'view', v => { S.view = v; status(); });
    K.seg(root, 'zoom', v => { S.zoom = +v; tl(); });

    all();
    return { destroy() {} };
  }

  window.JEEM_STUDIO_MOUNT = mount;
})();
