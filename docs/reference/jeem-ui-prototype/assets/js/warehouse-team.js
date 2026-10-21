/* ==========================================================
   جيم — نظرة على فريق المستودع (لوحة تفاعلية)
   مستوحاة من تصميم «Ware Sync Team Overview»، وأُعيدت بمحتوى عربي عام وهوية جيم.
   الحركات: عدّ المؤشرات، أعمدة صغيرة تنمو، عدّ تنازلي حي لتغيير المناوبة،
   مقاطع تقدم المهام تمتلئ حيًا، وأحداث تنزلق في سجل النشاط.
   ========================================================== */
(function () {
  const root = document.getElementById('whRoot'); if (!root) return;
  const $ = (s, el = root) => el.querySelector(s), $$ = (s, el = root) => [...el.querySelectorAll(s)];
  const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
  const esc = s => String(s).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
  const RM = matchMedia('(prefers-reduced-motion: reduce)').matches;
  const nf = v => Number(v).toLocaleString('en-US');
  const pad = n => String(n).padStart(2, '0');
  const count = (el, to, fmt = v => nf(Math.round(v)), dur = 900) => { if (!el) return; if (RM) { el.textContent = fmt(to); return; } const from = +el.dataset.v || 0, t0 = performance.now(); el.dataset.v = to; const st = t => { const p = Math.min(1, (t - t0) / dur), e = 1 - Math.pow(1 - p, 3); el.textContent = fmt(from + (to - from) * e); if (p < 1) requestAnimationFrame(st); }; requestAnimationFrame(st); };
  const toast = msg => { let t = document.querySelector('.toast'); if (!t) { t = document.createElement('div'); t.className = 'toast'; t.setAttribute('role', 'status'); document.body.append(t); } t.innerHTML = `${ic('check')}<span>${esc(msg)}</span>`; requestAnimationFrame(() => t.classList.add('is-on')); clearTimeout(t._h); t._h = setTimeout(() => t.classList.remove('is-on'), 2600); };
  const norm = s => String(s).replace(/[أإآ]/g, 'ا').replace(/ة/g, 'ه').replace(/ى/g, 'ي');

  /* ---------- البيانات التجريبية ---------- */
  const ROLES = { pick: 'منتقي', pack: 'مُعبّئ', fork: 'مشغّل رافعة', recv: 'استلام بضائع' };
  const ZONES = ['المنطقة أ إلى ب', 'المنطقة ج إلى د', 'المنطقة هـ', 'رصيف التحميل 1', 'رصيف التحميل 2', 'منطقة التعبئة'];
  const TASKS = { pick: 'انتقاء طلب', pack: 'تعبئة طلب', fork: 'نقل منصّة', recv: 'تفريغ شحنة' };
  let oid = 5840;
  const W = [
    ['رنا القحطاني', 'pick', 0, 8, 10, 94], ['مريم الشهري', 'pick', 1, 8, 12, 96], ['فيصل العنزي', 'fork', 3, 5, 9, 88],
    ['هند الزهراني', 'pack', 5, 11, 14, 92], ['عمر الدوسري', 'recv', 4, 3, 8, 81], ['لمى الحربي', 'pick', 2, 6, 10, 90],
    ['سعد المطيري', 'pack', 5, 9, 12, 93], ['نواف السبيعي', 'fork', 3, 4, 8, 86], ['أمل العتيبي', 'pick', 0, 7, 11, 91],
    ['تركي الغامدي', 'recv', 4, 2, 6, 78], ['جود الشمري', 'pack', 5, 10, 13, 95], ['ماجد البقمي', 'pick', 1, 5, 10, 84]
  ].map(([n, r, z, d, t, e], i) => ({ id: 'w' + i, n, r, z, done: d, tot: t, eff: e, on: true, task: TASKS[r], ord: '#ORD-' + (oid += 3), log: [] }));
  const OFF = [
    ['أليسا الحمدان', 'pack', 0, 12, 97], ['جمال الرويلي', 'pack', 1, 10, 89], ['سارة الفيفي', 'pick', 2, 11, 93], ['خالد العمري', 'fork', 3, 7, 85],
    ['منيرة القرني', 'recv', 4, 6, 82], ['بدر الشهراني', 'pick', 0, 9, 90], ['ريم المالكي', 'pack', 5, 13, 96], ['حسن الأحمدي', 'fork', 3, 8, 87]
  ].map(([n, r, z, d, e], i) => ({ id: 'o' + i, n, r, z, done: d, eff: e, on: false, ret: ['06:00 ص', '02:00 م', '10:00 م'][i % 3] }));
  const ALL = W.concat(OFF), byId = Object.fromEntries(ALL.map(w => [w.id, w]));
  const S = { role: 'all', q: '', more: false, unread: false, live: true, sel: null };
  const FEED = [];
  const ini = n => n.split(' ').slice(0, 2).map(x => x.replace(/^ال/, '')[0]).join('‌');
  const hue = id => (parseInt(id.slice(1), 10) * 53 + (id[0] === 'o' ? 20 : 190)) % 360;

  /* ---------- 1) المؤشرات ---------- */
  const HOURS = ['6ص', '8ص', '10ص', '12م', '2م', '4م', '6م'];
  function kpis() {
    const on = W.filter(w => w.on), tasks = W.reduce((a, w) => a + w.done, 0) + 60, eff = Math.round(on.reduce((a, w) => a + w.eff, 0) / (on.length || 1));
    const K = [['users', 'الموظفون في المناوبة', on.length, v => nf(Math.round(v)), [14, 18, 22, 26, 24, 26, 25]], ['layers', 'مهام اليوم المنجزة', tasks, v => nf(Math.round(v)), [8, 16, 22, 28, 20, 26, 22]], ['target', 'متوسط الكفاءة', eff, v => Math.round(v) + '%', [84, 88, 90, 93, 89, 91, 92]]];
    if (!$('#whKpis').children.length) $('#whKpis').innerHTML = K.map(([icn, n, , , bars], i) => { const mx = Math.max(...bars), mn = Math.min(...bars) * .8;
      return `<div class="wh-kpi" style="--i:${i}"><span class="lb">${ic(icn)}${n}</span><div class="row"><b class="num" data-k="${i}">0</b><span class="mb" role="img" aria-label="${n} حسب الساعة">${bars.map((b, j) => `<i style="--h:${((b - mn) / (mx - mn) * 80 + 20).toFixed(0)}%;--j:${j}" title="${HOURS[j]}: ${b}"></i>`).join('')}</span></div></div>`; }).join('');
    K.forEach(([, , v, f], i) => count($(`[data-k="${i}"]`), v, f));
  }

  /* ---------- 2) العدّ التنازلي لتغيير المناوبة ---------- */
  const STEP = 5 * 60 * 1000; let shiftAt = Math.ceil((Date.now() + 14 * 60 * 1000) / STEP) * STEP;
  function clock() {
    const left = Math.max(0, Math.round((shiftAt - Date.now()) / 1000)), d = new Date(shiftAt);
    $('#whMin').textContent = Math.ceil(left / 60); $('#whCd').classList.toggle('soon', left <= 300);
    $('#whH').textContent = pad(Math.floor(left / 3600)); $('#whM').textContent = pad(Math.floor(left % 3600 / 60)); $('#whS').textContent = pad(left % 60);
    $('#whAt').textContent = `${pad(d.getHours())}:${pad(d.getMinutes())}`; $('#whCd').setAttribute('aria-label', `تغيير المناوبة بعد ${Math.ceil(left / 60)} دقيقة`);
    if (!left) { shiftAt = Date.now() + 8 * 3600 * 1000; addFeed(null, 'بدأت مناوبة جديدة · تم تبديل الفرق', 'info'); }
  }

  /* ---------- 3) بطاقات الفرق ---------- */
  const match = w => (S.role === 'all' || w.r === S.role) && (!S.q || norm(w.n + ROLES[w.r] + ZONES[w.z]).includes(norm(S.q)));
  function card(w, i) {
    const segs = Array.from({ length: w.tot || 0 }, (_, j) => `<i class="${j < w.done ? 'f' : ''}" style="--j:${j}"></i>`).join('');
    const effSt = w.eff >= 90 ? 'ok' : w.eff >= 85 ? 'mid' : 'low';
    return `<article class="wh-card${w.on ? '' : ' off'}${S.sel === w.id ? ' sel' : ''}" data-w="${w.id}" style="--i:${i};--hu:${hue(w.id)}">
      <header><span class="av"><b>${esc(ini(w.n))}</b><em>${ROLES[w.r]}</em></span><span class="nm"><b>${esc(w.n)}</b><small>${ic('pin')}${ZONES[w.z]}</small></span>
        <span class="pill ${w.on ? 'on' : 'off'}">${ic(w.on ? 'check' : 'clock')}${w.on ? 'في المناوبة' : 'خارج المناوبة'}</span></header>
      ${w.on ? `<div class="prog"><span class="h"><span>تقدّم المهام</span><span class="eff ${effSt}">الكفاءة: <b class="num">${w.eff}%</b></span></span>
          <span class="segs" role="progressbar" aria-valuemin="0" aria-valuemax="${w.tot}" aria-valuenow="${w.done}" aria-label="تقدّم مهام ${esc(w.n)}">${segs}</span><small><span class="num">${w.done}/${w.tot}</span> مهام</small></div>
        <div class="cur"><span>المهمة الحالية:</span><b>${ic('layers')}${w.task}</b><code class="num">${w.ord}</code></div>
        <footer><button type="button" class="lnk" data-det="${w.id}" aria-expanded="${S.sel === w.id}">${S.sel === w.id ? 'إخفاء التفاصيل' : 'عرض التفاصيل'}</button><span class="sp"></span>
          <button type="button" class="btn btn-sm" data-adj="${w.id}">${ic('edit')}تعديل</button><button type="button" class="btn btn-sm btn-primary" data-re="${w.id}">${ic('share')}إعادة إسناد</button></footer>
        ${S.sel === w.id ? `<ol class="det">${(w.log.length ? w.log : [['07:05', 'بدء المناوبة'], ['08:40', `أكمل ${w.done ? 'أول طلب' : 'التجهيز'}`]]).slice(-4).map(([t, x]) => `<li><span class="num">${t}</span>${esc(x)}</li>`).join('')}</ol>` : ''}`
      : `<div class="sum"><span><small>آخر مناوبة</small><b><span class="num">${w.done}</span> مهمة</b></span><span><small>الكفاءة</small><b class="num">${w.eff}%</b></span><span><small>العودة</small><b>${w.ret}</b></span></div>
        <footer><span class="sp"></span><button type="button" class="btn btn-sm" data-call="${w.id}">${ic('bell')}استدعاء</button></footer>`}
    </article>`;
  }
  function teams() {
    const on = W.filter(w => w.on && match(w)), off = OFF.concat(W.filter(w => !w.on)).filter(match);
    $('#whOnN').textContent = on.length; $('#whOffN').textContent = 88 - OFF.length + off.length;
    $('#whOn').innerHTML = on.slice(0, S.more ? 99 : 4).map(card).join('') || '<p class="wh-none">لا موظفين يطابقون التصفية.</p>';
    $('#whOff').innerHTML = off.slice(0, S.more ? 99 : 4).map(card).join('') || '<p class="wh-none">لا موظفين يطابقون التصفية.</p>';
    $$('[data-more]').forEach(b => { b.textContent = S.more ? 'عرض أقل' : 'عرض الكل'; b.hidden = !S.more && on.length <= 4 && off.length <= 4; });
  }

  /* ---------- 4) سجل النشاط ---------- */
  const now = () => { const d = new Date(); return `${pad(d.getHours())}:${pad(d.getMinutes())}`; };
  function addFeed(w, html, kind = 'ok', unread = true) {
    FEED.unshift({ w: w && w.id, html, kind, t: now(), unread, fresh: true }); if (FEED.length > 40) FEED.pop(); feed();
  }
  function feed() {
    const L = FEED.filter(f => !S.unread || f.unread);
    $('#whUnread').textContent = FEED.filter(f => f.unread).length;
    $('#whFeed').innerHTML = L.map((f, i) => `<li class="k-${f.kind}${f.fresh && !RM ? ' fresh' : ''}${f.unread ? ' un' : ''}"${f.w ? ` data-go="${f.w}" tabindex="0" role="button"` : ''}><span class="dot" aria-hidden="true">${ic({ ok: 'check', move: 'share', start: 'play', info: 'info', warn: 'clock' }[f.kind] || 'info')}</span><p>${f.html}</p><time class="num">${f.t}</time></li>`).join('') || '<li class="none">لا أحداث غير مقروءة.</li>';
    FEED.forEach(f => f.fresh = false);
  }
  const who = w => `<b class="who" style="--hu:${hue(w.id)}">@${esc(w.n)}</b>`;
  const chip = s => `<code class="num">${esc(s)}</code>`;

  /* ---------- 5) محاكاة حية ---------- */
  function tick() {
    if (!S.live || document.hidden) return;
    const on = W.filter(w => w.on && w.done < w.tot); if (!on.length) return;
    const w = on[Math.floor(Math.random() * on.length)];
    if (Math.random() < .75) {
      w.done++; const o = w.ord; w.ord = '#ORD-' + (oid += 3); w.log.push([now(), `أكمل ${w.task} ${o}`]); w.eff = Math.min(99, w.eff + (Math.random() < .5 ? 1 : 0));
      addFeed(w, `${who(w)} أكمل ${w.task === 'نقل منصّة' ? 'نقل المنصّة' : 'الطلب'} ${chip(o)}`, 'ok');
      const el = $(`[data-w="${w.id}"]`);
      if (el) { const s = el.querySelectorAll('.segs i')[w.done - 1]; if (s) s.classList.add('f', 'pop'); const p = el.querySelector('.prog small .num'); if (p) p.textContent = `${w.done}/${w.tot}`; const c = el.querySelector('.cur code'); if (c) { c.textContent = w.ord; c.classList.remove('flash'); void c.offsetWidth; c.classList.add('flash'); } const e = el.querySelector('.eff .num'); if (e) e.textContent = w.eff + '%'; }
      kpis();
    } else {
      const dock = Math.random() < .5 ? 'الرصيف 1' : 'الرصيف 2';
      addFeed(w, `${who(w)} بدأ تفريغ ${chip(dock)}`, 'start');
    }
  }

  /* ---------- 6) نافذة إعادة الإسناد / التعديل ---------- */
  const dlg = $('#whDlg');
  function openDlg(kind, w) {
    const others = W.filter(x => x.on && x.id !== w.id).sort((a, b) => (a.tot - a.done) - (b.tot - b.done)); /* الأقل حملًا أولًا */
    dlg.innerHTML = `<div class="wh-dlg" role="dialog" aria-modal="true" aria-labelledby="whDlgT"><h3 id="whDlgT">${kind === 're' ? 'إعادة إسناد المهمة' : 'تعديل منطقة العمل'}</h3>
      <p>${kind === 're' ? `نقل ${esc(w.task)} ${chip(w.ord)} من ${who(w)} إلى:` : `نقل ${who(w)} إلى منطقة أخرى:`}</p>
      <div class="opts" role="radiogroup">${kind === 're' ? others.map((x, i) => `<label><input type="radio" name="whOpt" value="${x.id}" ${i ? '' : 'checked'}><span class="av sm" style="--hu:${hue(x.id)}">${esc(ini(x.n))}</span><span>${esc(x.n)}<small>${ROLES[x.r]} · متبقٍ <span class="num">${x.tot - x.done}</span> مهام${i ? '' : ' · الأقل حملًا'}</small></span></label>`).join('')
        : ZONES.map((z, i) => `<label><input type="radio" name="whOpt" value="${i}" ${i === w.z ? 'checked' : ''}><span>${z}</span></label>`).join('')}</div>
      <div class="act"><button type="button" class="btn" data-dlg-x>إلغاء</button><button type="button" class="btn btn-primary" data-dlg-ok="${kind}|${w.id}">تأكيد</button></div></div>`;
    dlg.hidden = false; dlg.querySelector('input:checked')?.focus();
  }
  const closeDlg = () => { dlg.hidden = true; dlg.innerHTML = ''; };
  function confirmDlg(kind, id) {
    const w = byId[id], v = dlg.querySelector('input:checked')?.value; if (v == null) return closeDlg();
    if (kind === 're') { const to = byId[v]; addFeed(to, `${who(to)} استلم ${chip(w.ord)} بإعادة إسناد من ${who(w)}`, 'move'); to.tot++; w.tot = Math.max(w.done, w.tot - 1); to.log.push([now(), `استلم ${w.ord}`]); w.ord = '#ORD-' + (oid += 3); toast(`أُعيد إسناد المهمة إلى ${to.n}`); }
    else { const z = +v; if (z !== w.z) { addFeed(w, `${who(w)} نُقل من ${chip(ZONES[w.z])} إلى ${chip(ZONES[z])}`, 'move'); w.z = z; toast(`نُقل ${w.n} إلى ${ZONES[z]}`); } }
    closeDlg(); teams();
  }

  /* ---------- الأحداث ---------- */
  root.addEventListener('click', e => {
    const t = e.target;
    const r = t.closest('[data-role]'); if (r) { S.role = r.dataset.role; $$('[data-role]').forEach(b => b.setAttribute('aria-pressed', String(b === r))); return teams(); }
    if (t.closest('[data-more]')) { S.more = !S.more; return teams(); }
    const d = t.closest('[data-det]'); if (d) { S.sel = S.sel === d.dataset.det ? null : d.dataset.det; return teams(); }
    const a = t.closest('[data-adj]'); if (a) return openDlg('adj', byId[a.dataset.adj]);
    const re = t.closest('[data-re]'); if (re) return openDlg('re', byId[re.dataset.re]);
    const c = t.closest('[data-call]'); if (c) { const w = byId[c.dataset.call]; addFeed(w, `أُرسل استدعاء إلى ${who(w)} للمناوبة القادمة`, 'warn'); return toast(`أُرسل الاستدعاء إلى ${w.n}`); }
    const ok = t.closest('[data-dlg-ok]'); if (ok) { const [k, id] = ok.dataset.dlgOk.split('|'); return confirmDlg(k, id); }
    if (t.closest('[data-dlg-x]') || t === dlg) return closeDlg();
    const g = t.closest('[data-go]'); if (g) return goTo(g.dataset.go);
    if (t.closest('[data-read]')) { FEED.forEach(f => f.unread = false); return feed(); }
    if (t.closest('[data-live]')) { S.live = !S.live; const b = $('[data-live]'); b.setAttribute('aria-pressed', String(S.live)); b.lastChild.textContent = S.live ? 'مباشر' : 'متوقف'; return; }
    if (t.closest('[data-new]')) return toast('هنا يُفتح نموذج إضافة مهمة: النوع، المنطقة، الطلب، والموظف.');
  });
  root.addEventListener('keydown', e => { const g = e.target.closest('[data-go]'); if (g && (e.key === 'Enter' || e.key === ' ')) { e.preventDefault(); goTo(g.dataset.go); } });
  addEventListener('keydown', e => { if (e.key === 'Escape' && !dlg.hidden) closeDlg(); });
  $('#whUnreadOnly').addEventListener('change', e => { S.unread = e.target.checked; feed(); });
  $('#whQ').addEventListener('input', e => { S.q = e.target.value.trim(); teams(); });
  function goTo(id) {
    const f = FEED.find(x => x.w === id && x.unread); if (f) { f.unread = false; feed(); }
    const w = byId[id]; if (!w) return;
    if (S.role !== 'all' && w.r !== S.role) { S.role = 'all'; $$('[data-role]').forEach(b => b.setAttribute('aria-pressed', String(b.dataset.role === 'all'))); }
    S.q = ''; $('#whQ').value = ''; S.more = true; teams();
    const el = $(`[data-w="${id}"]`); if (!el) return;
    el.scrollIntoView({ block: 'center', behavior: RM ? 'auto' : 'smooth' }); el.classList.remove('ping'); void el.offsetWidth; el.classList.add('ping');
  }

  /* ---------- البدء ---------- */
  [[W[0], 'ok', o => `${who(W[0])} أكمل الطلب ${chip('#ORD-5838')}`], [W[4], 'start', () => `${who(W[4])} بدأ تفريغ ${chip('الرصيف 2')}`], [W[2], 'move', () => `${who(W[2])} نُقل من ${chip('المنطقة أ')} إلى ${chip('المنطقة ب')}`], [W[1], 'ok', () => `${who(W[1])} أكمل الطلب ${chip('#ORD-5836')}`]].reverse()
    .forEach(([w, k, h]) => FEED.unshift({ w: w.id, html: h(), kind: k, t: '11:45', unread: k !== 'move', fresh: false }));
  kpis(); teams(); feed(); clock();
  setInterval(clock, 1000);
  setInterval(tick, 3500);
})();
