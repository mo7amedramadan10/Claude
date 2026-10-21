/* ==========================================================
   جيم — اللوحات المنشورة في المشروع
   مستويان من التبويبات:
   1) تبويبات اللوحات (أعلى، بنمط تبويب ملف ملتصق باللوحة) — لوحة لكل تبويب، تُحمَّل عند أول فتح وتحتفظ بحالتها.
   2) تبويبات صفحات اللوحة نفسها (داخل اللوحة، بنمط مقطعي أصغر) — يرسمها JR.mount من d.p.
   الرابط يحفظ الموضع: #d=<معرّف اللوحة>&p=<رقم الصفحة> ليُفتح نفس المكان عند المشاركة.
   ========================================================== */
(function () {
  const root = document.getElementById('pdRoot'); if (!root || !window.JR) return;
  const $ = s => root.querySelector(s), $$ = s => [...root.querySelectorAll(s)];
  const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
  const esc = s => String(s).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
  const RM = matchMedia('(prefers-reduced-motion: reduce)').matches;
  const LIB = window.JEEM_DASH_LIBRARY || [];
  const store = { get(k) { try { return localStorage.getItem(k); } catch (e) { return null; } }, set(k, v) { try { localStorage.setItem(k, v); } catch (e) { } } };

  /* اللوحات المنشورة على المشروع (في الإنتاج: GET /api/projects/{id}/published-dashboards) */
  const PUB = [
    { id: 'perf', lib: 'pl-sales-performance', by: 'محمد رمضان', at: '10 أكتوبر 2026', ver: 3, aud: 'الإدارة التنفيذية', ref: 'كل ساعة', live: true },
    { id: 'pipe', lib: 'pl-sales-pipeline', by: 'سارة القحطاني', at: '08 أكتوبر 2026', ver: 5, aud: 'فريق المبيعات', ref: 'كل 15 دقيقة', live: true },
    { id: 'ar', lib: 'pl-receivables-collections', by: 'محمد رمضان', at: '02 أكتوبر 2026', ver: 2, aud: 'الإدارة المالية', ref: 'يوميًا 6:00 ص', live: false },
    { id: 'prod', lib: 'pl-sales-product', by: 'خالد العتيبي', at: '28 سبتمبر 2026', ver: 1, aud: 'مديرو المنتجات', ref: 'كل ساعة', live: true },
    { id: 'funnel', lib: 'db-sales-funnel-performance', by: 'سارة القحطاني', at: '21 سبتمبر 2026', ver: 4, aud: 'كل أعضاء المشروع', ref: 'كل ساعة', live: true },
    { id: 'crm', embed: 'crm-analysis.html', name: 'تحليل مسار المبيعات (تفاعلية)', desc: 'لوحة تفاعلية بتصفية متقاطعة بين كل عناصرها.', by: 'محمد رمضان', at: '11 أكتوبر 2026', ver: 1, aud: 'فريق المبيعات', ref: 'لحظي', live: true }
  ].map(p => { const d = p.lib && LIB.find(x => x.id === p.lib); return Object.assign(p, d ? { d, name: d.name, desc: d.desc, pages: d.p.map(x => x.n) } : { pages: [] }); }).filter(p => p.d || p.embed);

  /* الترتيب لكل مستخدم في المتصفح (في الإنتاج: حقل sort_order يحدده مالك المشروع) */
  const KEY = 'jeem.pub.order.sales';
  try { const o = JSON.parse(store.get(KEY) || '[]'); if (o.length) PUB.sort((a, b) => (o.indexOf(a.id) + 1 || 99) - (o.indexOf(b.id) + 1 || 99)); } catch (e) { }
  const S = { cur: 0, mounted: {}, page: {}, show: false, t0: 0, raf: 0 };
  /* الدور من roles.js (في الإنتاج: من الـ Claims) — المشاهد يتصفح فقط بدون ترتيب أو تعديل */
  const canEdit = () => (document.documentElement.dataset.projectRole || 'owner') !== 'viewer';

  /* ---------- التبويبات العليا ---------- */
  function tabs() {
    $('#pdTabs').innerHTML = PUB.map((p, i) => `<button type="button" role="tab" id="pd-tab-${p.id}" aria-controls="pd-panel-${p.id}" aria-selected="${i === S.cur}" tabindex="${i === S.cur ? 0 : -1}" data-i="${i}" draggable="${canEdit()}" title="${esc(p.name)}">
      <span class="st ${p.live ? 'live' : 'sch'}" aria-label="${p.live ? 'بيانات حية' : 'تحديث مجدول'}"></span><span class="nm">${esc(p.name)}</span>${p.pages.length > 1 ? `<span class="pc" aria-label="${p.pages.length} صفحات"><span class="num">${p.pages.length}</span>${ic('layers')}</span>` : ''}${p.embed ? `<span class="pc tag">تفاعلية</span>` : ''}</button>`).join('');
    $('#pdCount').textContent = PUB.length;
    arrows();
  }
  function panels() {
    $('#pdPanels').innerHTML = PUB.map(p => `<section class="pd-panel" role="tabpanel" id="pd-panel-${p.id}" aria-labelledby="pd-tab-${p.id}" tabindex="0" hidden></section>`).join('');
  }
  function meta(p) {
    return `<header class="pd-meta">
      <div class="t"><h2>${esc(p.name)}</h2><p>${esc(p.desc || '')}</p>
        <div class="m"><span>${ic('users')}نُشرت بواسطة <b>${esc(p.by)}</b></span><span>${ic('calendar')}${esc(p.at)}</span><span>${ic('history')}الإصدار <b class="num">${p.ver}</b></span><span>${ic('shield')}الجمهور: <b>${esc(p.aud)}</b></span><span class="${p.live ? 'lv' : ''}">${ic(p.live ? 'spark' : 'clock')}${p.live ? 'بيانات حية' : 'تحديث مجدول'} · ${esc(p.ref)}</span>${p.pages.length > 1 ? `<span>${ic('layers')}<b class="num">${p.pages.length}</b> صفحات</span>` : ''}</div></div>
      <div class="a"><button type="button" class="btn btn-sm" data-copy>${ic('link')}نسخ رابط هذه الصفحة</button><button type="button" class="btn btn-sm" data-pdf>${ic('download')}تصدير PDF</button>${canEdit() ? `<a class="btn btn-sm btn-soft" href="project-workspace.html">${ic('edit')}تعديل في المحادثة واللوحات</a>` : ''}</div></header>`;
  }
  /* تحميل كسول: تُركّب اللوحة عند أول فتح فقط، وتحتفظ بصفحتها الداخلية عند التنقل */
  function mount(i) {
    const p = PUB[i], el = document.getElementById('pd-panel-' + p.id);
    if (S.mounted[p.id]) return;
    if (p.embed) {
      el.innerHTML = meta(p) + `<div class="pd-frame"><div class="pd-load">${ic('history')}جارٍ تحميل اللوحة…</div><iframe src="${p.embed}?embed=1" title="${esc(p.name)}" loading="lazy"></iframe></div>`;
      S.mounted[p.id] = { embed: true };
    } else {
      el.innerHTML = meta(p) + `<div class="pd-dash"><div class="jr-bar pd-inner"></div><div class="grid"></div></div>`;
      const start = Math.min(S.page[p.id] || 0, p.pages.length - 1);
      S.mounted[p.id] = JR.mount(p.d, el.querySelector('.grid'), { bar: el.querySelector('.pd-inner'), startPage: start, onPage: pi => { S.page[p.id] = pi; hash(); label(el, p); } });
      S.page[p.id] = start; label(el, p);
    }
  }
  /* تسمية تبويبات الصفحات الداخلية للوصولية: «صفحات لوحة X» */
  function label(el, p) { const t = el.querySelector('.jr-tabs'); if (t) t.setAttribute('aria-label', `صفحات لوحة ${p.name}`); }
  function activate(i, { focus = false, push = true } = {}) {
    i = (i + PUB.length) % PUB.length; S.cur = i; const p = PUB[i];
    $$('#pdTabs [role=tab]').forEach((t, k) => { t.setAttribute('aria-selected', String(k === i)); t.tabIndex = k === i ? 0 : -1; });
    $$('.pd-panel').forEach(el => { el.hidden = el.id !== 'pd-panel-' + p.id; });
    mount(i);
    const el = document.getElementById('pd-panel-' + p.id); if (!RM) { el.classList.remove('in'); void el.offsetWidth; el.classList.add('in'); }
    const tb = document.getElementById('pd-tab-' + p.id); tb.scrollIntoView({ block: 'nearest', inline: 'nearest', behavior: RM ? 'auto' : 'smooth' }); if (focus) tb.focus();
    if (push) hash(); arrows(); menu(false);
  }
  function hash() { const p = PUB[S.cur]; history.replaceState(null, '', `#d=${p.id}${S.page[p.id] ? '&p=' + S.page[p.id] : ''}`); }
  function fromHash() { const m = new URLSearchParams(location.hash.slice(1)), id = m.get('d'), pg = +m.get('p') || 0, i = PUB.findIndex(p => p.id === id); if (i >= 0) { S.page[id] = pg; return i; } return 0; }

  /* أسهم التمرير تظهر فقط عند وجود تبويبات خارج العرض */
  function arrows() { const t = $('#pdTabs'), max = t.scrollWidth - t.clientWidth; $('[data-scroll="-1"]').hidden = $('[data-scroll="1"]').hidden = max < 4; const x = Math.abs(t.scrollLeft); $('[data-scroll="-1"]').disabled = x < 4; $('[data-scroll="1"]').disabled = x > max - 4; root.querySelector('.pd-strip').classList.toggle('ov', max >= 4); }

  /* ---------- قائمة «كل اللوحات» ---------- */
  function menu(open) {
    const m = $('#pdMenu'), b = $('[data-all]'); if (open == null) open = m.hidden; m.hidden = !open; b.setAttribute('aria-expanded', String(open));
    if (open) { $('#pdQ').value = ''; list(''); $('#pdQ').focus(); }
  }
  function list(q) {
    const r = PUB.map((p, i) => [p, i]).filter(([p]) => !q || p.name.includes(q) || (p.aud || '').includes(q));
    $('#pdList').innerHTML = r.map(([p, i]) => `<li><button type="button" data-go="${i}" class="${i === S.cur ? 'on' : ''}"><span class="st ${p.live ? 'live' : 'sch'}"></span><span><b>${esc(p.name)}</b><small>${p.pages.length > 1 ? `${p.pages.length} صفحات · ` : ''}${esc(p.aud)} · الإصدار ${p.ver}</small></span></button></li>`).join('') || '<li class="mut">لا لوحات مطابقة</li>';
  }

  /* ---------- وضع العرض: يمر على صفحات كل لوحة ثم ينتقل للوحة التالية ---------- */
  const STEP = 12000;
  function show(on) {
    S.show = on; const b = $('[data-show]'); b.setAttribute('aria-pressed', String(on)); b.querySelector('span').textContent = on ? 'إيقاف العرض' : 'وضع العرض';
    root.classList.toggle('is-show', on); cancelAnimationFrame(S.raf);
    if (on) { if (root.requestFullscreen && !document.fullscreenElement) root.requestFullscreen().catch(() => { }); S.t0 = performance.now(); tick(); }
    else { if (document.fullscreenElement) document.exitFullscreen().catch(() => { }); $('#pdProg').style.width = '0'; }
  }
  function tick() {
    if (!S.show) return;
    const paused = root.matches(':hover') && !document.fullscreenElement, k = (performance.now() - S.t0) / STEP;
    if (paused) S.t0 = performance.now() - (parseFloat($('#pdProg').style.width) || 0) / 100 * STEP;
    $('#pdProg').style.width = Math.min(100, k * 100) + '%';
    if (k >= 1) { next(); S.t0 = performance.now(); }
    S.raf = requestAnimationFrame(tick);
  }
  function next() {
    const p = PUB[S.cur], m = S.mounted[p.id];
    if (m && m.setPage && (S.page[p.id] || 0) < p.pages.length - 1) { S.page[p.id] = (S.page[p.id] || 0) + 1; m.setPage(S.page[p.id]); label(document.getElementById('pd-panel-' + p.id), p); hash(); }
    else { const n = (S.cur + 1) % PUB.length; S.page[PUB[n].id] = 0; if (S.mounted[PUB[n].id]?.setPage) S.mounted[PUB[n].id].setPage(0); activate(n); }
  }

  /* ---------- الأحداث ---------- */
  root.addEventListener('click', e => {
    const t = e.target; let x;
    if ((x = t.closest('#pdTabs [role=tab]'))) return activate(+x.dataset.i);
    if ((x = t.closest('[data-scroll]'))) { $('#pdTabs').scrollBy({ left: -(+x.dataset.scroll) * 260, behavior: RM ? 'auto' : 'smooth' }); return; }
    if (t.closest('[data-all]')) return menu();
    if ((x = t.closest('[data-go]'))) return activate(+x.dataset.go, { focus: true });
    if (t.closest('[data-show]')) return show(!S.show);
    if (t.closest('[data-copy]')) { const u = location.href; (navigator.clipboard?.writeText(u) || Promise.reject()).then(() => toast('نُسخ رابط اللوحة والصفحة الحالية'), () => toast('الرابط: ' + location.hash)); return; }
    if (t.closest('[data-pdf]')) return toast(`يُصدَّر «${PUB[S.cur].name}» كملف PDF بكل صفحاتها`);
  });
  document.addEventListener('click', e => { if (!$('#pdMenu').hidden && !e.target.closest('#pdMenu, [data-all]')) menu(false); });
  $('#pdQ').addEventListener('input', e => list(e.target.value.trim()));
  $('#pdTabs').addEventListener('scroll', arrows, { passive: true });
  addEventListener('resize', arrows);
  /* لوحة المفاتيح على التبويبات: الأسهم (RTL: اليسار = التالي)، Home/End، وCtrl+سهم لإعادة الترتيب */
  $('#pdTabs').addEventListener('keydown', e => {
    const i = S.cur; let n = null;
    if (e.key === 'ArrowLeft') n = i + 1; else if (e.key === 'ArrowRight') n = i - 1; else if (e.key === 'Home') n = 0; else if (e.key === 'End') n = PUB.length - 1; else return;
    e.preventDefault();
    if (e.ctrlKey && (e.key === 'ArrowLeft' || e.key === 'ArrowRight') && canEdit()) { const j = Math.max(0, Math.min(PUB.length - 1, n)); if (j !== i) { move(i, j); toast(`نُقلت «${PUB[j].name}» إلى الموضع ${j + 1}`); } return; }
    activate(n, { focus: true });
  });
  root.addEventListener('keydown', e => { if (e.key === 'Escape') { if (!$('#pdMenu').hidden) { menu(false); $('[data-all]').focus(); } else if (S.show) show(false); } });
  /* السحب لإعادة الترتيب */
  let drag = null;
  $('#pdTabs').addEventListener('dragstart', e => { const t = e.target.closest('[role=tab]'); if (!t || !canEdit()) return e.preventDefault(); drag = +t.dataset.i; t.classList.add('drag'); e.dataTransfer.effectAllowed = 'move'; e.dataTransfer.setData('text/plain', String(drag)); });
  $('#pdTabs').addEventListener('dragover', e => { const t = e.target.closest('[role=tab]'); if (drag == null || !t) return; e.preventDefault(); $$('#pdTabs .over').forEach(x => x.classList.remove('over')); t.classList.add('over'); });
  $('#pdTabs').addEventListener('drop', e => { const t = e.target.closest('[role=tab]'); if (drag == null || !t) return; e.preventDefault(); move(drag, +t.dataset.i); drag = null; });
  $('#pdTabs').addEventListener('dragend', () => { drag = null; $$('#pdTabs .drag, #pdTabs .over').forEach(x => x.classList.remove('drag', 'over')); });
  function move(a, b) { const curId = PUB[S.cur].id, [it] = PUB.splice(a, 1); PUB.splice(b, 0, it); store.set(KEY, JSON.stringify(PUB.map(p => p.id))); S.cur = PUB.findIndex(p => p.id === curId); tabs(); document.getElementById('pd-tab-' + PUB[b].id).focus(); }
  /* ارتفاع الإطار للوحة التفاعلية المضمّنة (ترسله الصفحة المضمّنة) */
  addEventListener('message', e => { const h = e.data && e.data.jeemEmbedH; if (!h) return; $$('.pd-frame iframe').forEach(f => { if (f.contentWindow === e.source) { f.style.height = h + 'px'; f.parentElement.classList.add('ok'); } }); });
  addEventListener('hashchange', () => { const i = fromHash(); if (i !== S.cur) activate(i, { push: false }); else { const p = PUB[i]; S.mounted[p.id]?.setPage?.(S.page[p.id] || 0); } });
  document.addEventListener('fullscreenchange', () => { if (!document.fullscreenElement && S.show) show(false); });
  function toast(msg) { let t = document.querySelector('.toast'); if (!t) { t = document.createElement('div'); t.className = 'toast'; t.setAttribute('role', 'status'); document.body.append(t); } t.innerHTML = `${ic('check')}<span>${esc(msg)}</span>`; requestAnimationFrame(() => t.classList.add('is-on')); clearTimeout(t._h); t._h = setTimeout(() => t.classList.remove('is-on'), 2600); }

  document.querySelector('[data-show-ext]')?.addEventListener('click', () => show(!S.show));
  if (!PUB.length) { root.innerHTML = `<div class="empty-state">${ic('dashboard', 'icon')}<h3>لا توجد لوحات منشورة بعد</h3><p>انشر لوحة من «المحادثة واللوحات» لتظهر هنا لكل أعضاء المشروع.</p><a class="btn btn-primary" href="project-workspace.html">الذهاب للمحادثة واللوحات</a></div>`; return; }
  tabs(); panels(); activate(fromHash(), { push: false });
})();
