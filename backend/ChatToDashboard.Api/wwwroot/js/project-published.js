/* ==========================================================
   جيم — شاشة «اللوحات المنشورة» داخل المشروع (§4.23)
   مستويان من التبويبات:
   1) تبويبات اللوحات (بنمط تبويب ملف ملتصق بالإطار) — لوحة لكل تبويب، تُحمَّل عند أول
      فتح فقط وتحتفظ بصفحتها الداخلية وفلاترها عند التنقل بين اللوحات والعودة.
   2) تبويبات صفحات اللوحة نفسها — داخل إطار اللوحة المضمّنة (وضع ?embed=1 في app.js).
   كل لوحة (المبنية من المحادثة والتفاعلية) تُعرض داخل iframe معزول بحالته الخاصة، يتبادل
   مع هذه الصفحة رسائل postMessage: الارتفاع ورقم الصفحة الحالية وعدد الصفحات، وأمر
   الانتقال لصفحة معيّنة (لوضع العرض والرابط العميق).
   الرابط يحفظ الموضع: #d=<معرّف النشر>&p=<رقم الصفحة>.
   البيانات: GET /api/published-dashboards — لا بيانات تجريبية هنا.
   ========================================================== */
(function () {
  const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
  const esc = s => String(s ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
  const RM = () => matchMedia('(prefers-reduced-motion: reduce)').matches;
  const fmtDate = iso => {
    if (!iso) return '';
    const d = new Date(iso + (/[zZ]|[+-]\d\d:\d\d$/.test(iso) ? '' : 'Z'));
    return d.toLocaleDateString('ar-SA-u-ca-gregory-nu-latn', { day: '2-digit', month: 'long', year: 'numeric' });
  };
  const ROLE_TXT = { owner: 'مالك', editor: 'محرر', viewer: 'مشاهد' };
  const STEP = 12000; // وضع العرض: 12 ثانية لكل صفحة

  const S = {
    items: [], sig: '', cur: 0, mounted: {}, page: {}, pageCount: {},
    canManage: false, myRole: '', show: false, t0: 0, last: 0, raf: 0, onScreen: false,
  };
  const body = () => document.getElementById('pd-body');
  const q = s => body()?.querySelector(s);
  const qa = s => [...(body()?.querySelectorAll(s) || [])];
  const pagesOf = p => S.pageCount[p.id] || Math.max(1, p.pages.length);

  function toast(msg) {
    let t = document.querySelector('.toast');
    if (!t) { t = document.createElement('div'); t.className = 'toast'; t.setAttribute('role', 'status'); document.body.appendChild(t); }
    t.innerHTML = `${ic('check')}<span>${esc(msg)}</span>`;
    t.classList.remove('is-on'); void t.offsetWidth; t.classList.add('is-on');
    clearTimeout(t._h); t._h = setTimeout(() => t.classList.remove('is-on'), 2800);
  }

  /* ---------- التحميل ---------- */
  async function load() {
    S.onScreen = true;
    let data;
    try {
      const res = await fetch('/api/published-dashboards');
      data = res.ok ? await res.json() : null;
    } catch { data = null; }
    if (!data) {
      body().innerHTML = `<div class="pd-empty">${ic('info', 'icon')}<h3>تعذّر تحميل اللوحات المنشورة</h3><p>حاول مرة أخرى بعد قليل.</p></div>`;
      return;
    }
    S.canManage = !!data.canManage;
    S.myRole = data.myRole || 'viewer';
    document.getElementById('pd-my-role').innerHTML = `${ic('shield')}دورك في المشروع: <b>${esc(ROLE_TXT[S.myRole] || S.myRole)}</b>`;
    document.getElementById('pd-viewer-note').classList.toggle('hidden', S.canManage);
    document.getElementById('pd-publish-btn').classList.toggle('hidden', !S.canManage);
    const badge = document.getElementById('published-badge');
    badge.textContent = data.items.length; badge.classList.toggle('hidden', !data.items.length);

    // إعادة البناء فقط إن تغيّرت القائمة فعلًا — العودة للشاشة دون تغيير تُبقي كل لوحة
    // مركّبة بصفحتها وفلاترها كما تُركت.
    const sig = data.items.map(p => `${p.id}:${p.version}:${p.audience}:${p.refreshMode}:${p.refreshEvery || ''}`).join('|');
    if (sig === S.sig && q('#pdRoot')) { activate(fromHash(), { push: true }); return; }
    S.items = data.items; S.sig = sig; S.mounted = {}; S.pageCount = {};
    render();
  }

  function render() {
    const host = body();
    if (!S.items.length) {
      host.innerHTML = `<div class="pd-empty">${ic('dashboard', 'icon')}<h3>لا توجد لوحات منشورة بعد</h3>
        <p>${S.canManage ? 'انشر لوحة نشطة من «اللوحات النشطة» أو من زر «نشر لوحة» لتظهر هنا لأعضاء المشروع.' : 'ستظهر هنا اللوحات التي ينشرها مالك المشروع أو محرروه.'}</p>
        ${S.canManage ? `<button type="button" class="btn btn-primary" data-empty-publish>${ic('publish')}نشر لوحة</button>` : ''}</div>`;
      host.querySelector('[data-empty-publish]')?.addEventListener('click', () => openPublishModal());
      return;
    }
    host.innerHTML = `<div class="pd" id="pdRoot">
        <div class="pd-prog" aria-hidden="true"><i id="pdProg"></i></div>
        <div class="pd-strip">
          <button type="button" class="pd-arrow" data-scroll="-1" aria-label="تبويبات سابقة" hidden>${ic('chev-left', 'icon icon-sm flip')}</button>
          <div class="pd-tabs" id="pdTabs" role="tablist" aria-label="اللوحات المنشورة"></div>
          <button type="button" class="pd-arrow" data-scroll="1" aria-label="تبويبات تالية" hidden>${ic('chev-left')}</button>
          <button type="button" class="btn btn-sm pd-all" data-all aria-haspopup="true" aria-expanded="false" aria-controls="pdMenu">${ic('grid')}<span class="lbl">كل اللوحات</span> <span class="badge num" id="pdCount">0</span></button>
          <button type="button" class="btn btn-sm pd-all" data-show aria-pressed="false">${ic('play')}<span>وضع العرض</span></button>
          <div class="pd-menu" id="pdMenu" hidden><label class="sr-only" for="pdQ">ابحث في اللوحات المنشورة</label><input id="pdQ" type="search" placeholder="ابحث باسم اللوحة أو الجمهور…"><ul id="pdList"></ul></div>
        </div>
        <div id="pdPanels">${S.items.map(p => `<section class="pd-panel" role="tabpanel" id="pd-panel-${esc(p.id)}" aria-labelledby="pd-tab-${esc(p.id)}" tabindex="0" hidden></section>`).join('')}</div>
      </div>
      <p class="pd-hint">${ic('info')} الأسهم ← → للتنقل بين التبويبات${S.canManage ? ' · Ctrl + سهم أو السحب لإعادة ترتيب اللوحات' : ''} · «وضع العرض» يمر على صفحات كل لوحة ثم ينتقل للتالية · Esc للخروج</p>`;
    wire();
    tabs();
    activate(fromHash(), { push: true });
  }

  /* ---------- التبويبات العليا ---------- */
  function tabs() {
    q('#pdTabs').innerHTML = S.items.map((p, i) => {
      const n = pagesOf(p), live = p.refreshMode === 'live';
      return `<button type="button" role="tab" id="pd-tab-${esc(p.id)}" aria-controls="pd-panel-${esc(p.id)}" aria-selected="${i === S.cur}" tabindex="${i === S.cur ? 0 : -1}" data-i="${i}" draggable="${S.canManage}" title="${esc(p.name)}">
        <span class="st ${live ? 'live' : 'sch'}" role="img" aria-label="${live ? 'بيانات حية' : 'تحديث مجدول'}"></span><span class="nm">${esc(p.name)}</span>${n > 1 ? `<span class="pc" aria-label="${n} صفحات"><span class="num">${n}</span>${ic('layers')}</span>` : ''}${p.kind === 'interactive' ? '<span class="pc tag">تفاعلية</span>' : ''}</button>`;
    }).join('');
    q('#pdCount').textContent = S.items.length;
    arrows();
  }

  function meta(p) {
    const live = p.refreshMode === 'live', n = pagesOf(p);
    const refresh = (live ? 'بيانات حية' : 'تحديث مجدول') + (p.refreshEvery ? ` · ${esc(p.refreshEvery)}` : '');
    return `<header class="pd-meta">
      <div class="t"><h2>${esc(p.name)}</h2>${p.desc && p.desc !== p.name ? `<p>${esc(p.desc)}</p>` : ''}
        <div class="m"><span>${ic('users')}نُشرت بواسطة <b>${esc(p.publishedByName)}</b></span><span>${ic('calendar')}${esc(fmtDate(p.publishedAt))}</span><span>${ic('history')}الإصدار <b class="num">${p.version}</b></span><span>${ic('shield')}الجمهور: <b>${esc(p.audienceLabel)}</b></span><span class="${live ? 'lv' : ''}">${ic(live ? 'spark' : 'clock')}${refresh}</span>${n > 1 ? `<span data-pages>${ic('layers')}<b class="num">${n}</b> صفحات</span>` : '<span data-pages hidden></span>'}</div></div>
      <div class="a"><button type="button" class="btn btn-sm" data-copy>${ic('link')}نسخ رابط هذه الصفحة</button><button type="button" class="btn btn-sm" data-pdf>${ic('download')}تصدير PDF</button>${S.canManage ? `<button type="button" class="btn btn-sm btn-soft" data-edit>${ic('edit')}تعديل في المحادثة واللوحات</button><button type="button" class="btn btn-sm" data-settings>${ic('settings')}إعدادات النشر</button><button type="button" class="btn btn-sm" data-unpublish>${ic('trash')}إلغاء النشر</button>` : ''}</div></header>`;
  }

  /* تحميل كسول: يُنشأ إطار اللوحة عند أول فتح لتبويبها فقط، ويبقى حيًا بعدها */
  function mount(i) {
    const p = S.items[i], el = document.getElementById('pd-panel-' + p.id);
    if (!el || S.mounted[p.id]) return;
    const pg = S.page[p.id] || 0;
    el.innerHTML = meta(p) + `<div class="pd-frame"><div class="pd-load">${ic('history')}جارٍ تحميل اللوحة…</div><iframe src="${esc(p.embedUrl)}${pg ? '&p=' + pg : ''}" title="${esc(p.name)}"></iframe></div>`;
    S.mounted[p.id] = true;
  }
  const frameOf = p => document.querySelector(`#pd-panel-${CSS.escape(p.id)} iframe`);

  function activate(i, { focus = false, push = true } = {}) {
    if (!S.items.length) return;
    i = (i + S.items.length) % S.items.length; S.cur = i; const p = S.items[i];
    qa('#pdTabs [role=tab]').forEach((t, k) => { t.setAttribute('aria-selected', String(k === i)); t.tabIndex = k === i ? 0 : -1; });
    qa('.pd-panel').forEach(el => { el.hidden = el.id !== 'pd-panel-' + p.id; });
    mount(i);
    const el = document.getElementById('pd-panel-' + p.id);
    if (!RM()) { el.classList.remove('in'); void el.offsetWidth; el.classList.add('in'); }
    const tb = document.getElementById('pd-tab-' + p.id);
    tb.scrollIntoView({ block: 'nearest', inline: 'nearest', behavior: RM() ? 'auto' : 'smooth' });
    if (focus) tb.focus();
    if (push) hash();
    arrows(); menu(false);
  }

  /* الرابط العميق — يُكتب فقط وهذه الشاشة ظاهرة */
  function hash() {
    if (!S.onScreen || !S.items.length) return;
    const p = S.items[S.cur], pg = S.page[p.id] || 0;
    history.replaceState(null, '', `${location.pathname}${location.search}#d=${encodeURIComponent(p.id)}${pg ? '&p=' + pg : ''}`);
  }
  function fromHash() {
    const m = new URLSearchParams(location.hash.slice(1)), id = m.get('d'), pg = Math.max(0, parseInt(m.get('p'), 10) || 0);
    const i = S.items.findIndex(p => p.id === id);
    if (i < 0) return S.cur < S.items.length ? S.cur : 0;
    if (pg !== (S.page[id] || 0)) {
      S.page[id] = pg;
      if (S.mounted[id]) setFramePage(S.items[i], pg);
    }
    return i;
  }

  /* أسهم التمرير تظهر فقط عند وجود تبويبات خارج العرض */
  function arrows() {
    const t = q('#pdTabs'); if (!t) return;
    const max = t.scrollWidth - t.clientWidth, x = Math.abs(t.scrollLeft);
    q('[data-scroll="-1"]').hidden = q('[data-scroll="1"]').hidden = max < 4;
    q('[data-scroll="-1"]').disabled = x < 4; q('[data-scroll="1"]').disabled = x > max - 4;
    q('.pd-strip').classList.toggle('ov', max >= 4);
  }

  /* ---------- قائمة «كل اللوحات» ---------- */
  function menu(open) {
    const m = q('#pdMenu'), b = q('[data-all]'); if (!m) return;
    if (open == null) open = m.hidden;
    m.hidden = !open; b.setAttribute('aria-expanded', String(open));
    if (open) { q('#pdQ').value = ''; list(''); q('#pdQ').focus(); }
  }
  function list(term) {
    const r = S.items.map((p, i) => [p, i]).filter(([p]) => !term || p.name.includes(term) || (p.audienceLabel || '').includes(term));
    q('#pdList').innerHTML = r.map(([p, i]) => {
      const n = pagesOf(p);
      return `<li><button type="button" data-go="${i}" class="${i === S.cur ? 'on' : ''}"><span class="st ${p.refreshMode === 'live' ? 'live' : 'sch'}" aria-hidden="true"></span><span><b>${esc(p.name)}</b><small>${n > 1 ? `<span class="num">${n}</span> صفحات · ` : ''}${esc(p.audienceLabel)} · الإصدار <span class="num">${p.version}</span></small></span></button></li>`;
    }).join('') || '<li class="mut">لا لوحات مطابقة</li>';
  }

  /* ---------- التخاطب مع الإطار المضمّن ---------- */
  function setFramePage(p, pg) {
    frameOf(p)?.contentWindow?.postMessage({ jeemSetPage: pg }, location.origin);
  }
  addEventListener('message', e => {
    if (e.origin !== location.origin || !e.data || typeof e.data !== 'object') return;
    const p = S.items.find(x => frameOf(x)?.contentWindow === e.source); if (!p) return;
    const f = frameOf(p);
    if (e.data.jeemEmbedH) { f.style.height = Math.max(200, +e.data.jeemEmbedH) + 'px'; f.parentElement.classList.add('ok'); }
    if (typeof e.data.jeemPages === 'number' && e.data.jeemPages !== S.pageCount[p.id]) {
      S.pageCount[p.id] = e.data.jeemPages;
      const pill = document.querySelector(`#pd-panel-${CSS.escape(p.id)} [data-pages]`);
      if (pill) { pill.hidden = e.data.jeemPages < 2; pill.innerHTML = `${ic('layers')}<b class="num">${e.data.jeemPages}</b> صفحات`; }
    }
    if (typeof e.data.jeemPage === 'number') {
      S.page[p.id] = e.data.jeemPage;
      if (S.items[S.cur] === p) hash();
    }
  });

  /* ---------- وضع العرض: يمر على صفحات اللوحة الحالية ثم ينتقل للوحة التالية ---------- */
  function show(on) {
    const root = q('#pdRoot'); if (!root) return;
    S.show = on;
    const b = q('[data-show]'); b.setAttribute('aria-pressed', String(on));
    b.innerHTML = `${ic(on ? 'pause' : 'play')}<span>${on ? 'إيقاف العرض' : 'وضع العرض'}</span>`;
    root.classList.toggle('is-show', on); cancelAnimationFrame(S.raf);
    if (on) {
      if (root.requestFullscreen && !document.fullscreenElement) root.requestFullscreen().catch(() => { });
      S.t0 = S.last = performance.now(); tick();
    } else {
      if (document.fullscreenElement) document.exitFullscreen().catch(() => { });
      q('#pdProg').style.width = '0';
    }
  }
  function tick() {
    if (!S.show) return;
    const root = q('#pdRoot'), now = performance.now();
    // يتوقف مؤقتًا عند مرور الفأرة: يُزاح زمن البدء بقدر الوقت المنقضي فيبقى التقدم ثابتًا
    if (root.matches(':hover')) S.t0 += now - S.last;
    S.last = now;
    const k = (now - S.t0) / STEP;
    q('#pdProg').style.width = Math.min(100, k * 100) + '%';
    if (k >= 1) { next(); S.t0 = now; }
    S.raf = requestAnimationFrame(tick);
  }
  function next() {
    const p = S.items[S.cur], pg = S.page[p.id] || 0;
    if (S.mounted[p.id] && pg < pagesOf(p) - 1) { S.page[p.id] = pg + 1; setFramePage(p, pg + 1); hash(); return; }
    const n = (S.cur + 1) % S.items.length, np = S.items[n];
    S.page[np.id] = 0;
    if (S.mounted[np.id]) setFramePage(np, 0);
    activate(n);
  }

  /* ---------- إعادة الترتيب (المالك والمحرر) ---------- */
  async function move(a, b) {
    if (a === b) return;
    const before = S.items.slice(), curId = S.items[S.cur].id;
    const [it] = S.items.splice(a, 1); S.items.splice(b, 0, it);
    S.cur = S.items.findIndex(p => p.id === curId);
    tabs(); document.getElementById('pd-tab-' + it.id)?.focus();
    S.sig = S.items.map(p => `${p.id}:${p.version}:${p.audience}:${p.refreshMode}:${p.refreshEvery || ''}`).join('|');
    try {
      const res = await fetch('/api/published-dashboards/order', {
        method: 'PUT', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ids: S.items.map(p => p.id) }),
      });
      if (!res.ok) throw new Error();
      toast(`نُقلت «${it.name}» إلى الموضع ${b + 1}`);
    } catch {
      S.items = before; S.cur = S.items.findIndex(p => p.id === curId); tabs();
      toast('تعذّر حفظ الترتيب — أُعيد الترتيب السابق');
    }
  }

  /* ---------- الأحداث ---------- */
  let drag = null;
  function wire() {
    const root = q('#pdRoot');
    root.addEventListener('click', e => {
      const t = e.target; let x;
      if ((x = t.closest('#pdTabs [role=tab]'))) return activate(+x.dataset.i);
      if ((x = t.closest('[data-scroll]'))) { q('#pdTabs').scrollBy({ left: -(+x.dataset.scroll) * 260, behavior: RM() ? 'auto' : 'smooth' }); return; }
      if (t.closest('[data-all]')) return menu();
      if ((x = t.closest('[data-go]'))) return activate(+x.dataset.go, { focus: true });
      if (t.closest('[data-show]')) return show(!S.show);
      const p = S.items[S.cur];
      if (t.closest('[data-copy]')) {
        hash();
        const u = location.href;
        (navigator.clipboard?.writeText(u) || Promise.reject()).then(() => toast('نُسخ رابط اللوحة والصفحة الحالية'), () => toast('الرابط: ' + u));
        return;
      }
      if (t.closest('[data-pdf]')) {
        // الإطار من نفس الأصل: طباعته تطبع اللوحة وحدها بقواعد @media print المعتادة
        const w = frameOf(p)?.contentWindow;
        if (w) { w.focus(); w.print(); } return;
      }
      if (t.closest('[data-edit]')) return editInChat(p);
      if (t.closest('[data-settings]')) return openPublishModal(p.dashboardId);
      if (t.closest('[data-unpublish]')) return unpublish(p);
    });
    q('#pdQ').addEventListener('input', e => list(e.target.value.trim()));
    q('#pdTabs').addEventListener('scroll', arrows, { passive: true });
    /* لوحة المفاتيح (نمط tablist): ← → للتنقل (RTL: اليسار = التالي)، Home/End، وCtrl+سهم لإعادة الترتيب */
    q('#pdTabs').addEventListener('keydown', e => {
      const i = S.cur; let n = null;
      if (e.key === 'ArrowLeft') n = i + 1; else if (e.key === 'ArrowRight') n = i - 1;
      else if (e.key === 'Home') n = 0; else if (e.key === 'End') n = S.items.length - 1; else return;
      e.preventDefault();
      if (e.ctrlKey && (e.key === 'ArrowLeft' || e.key === 'ArrowRight')) {
        if (S.canManage) move(i, Math.max(0, Math.min(S.items.length - 1, n)));
        return;
      }
      activate(n, { focus: true });
    });
    root.addEventListener('keydown', e => {
      if (e.key !== 'Escape') return;
      if (!q('#pdMenu').hidden) { menu(false); q('[data-all]').focus(); } else if (S.show) show(false);
    });
    /* السحب لإعادة الترتيب */
    const tl = q('#pdTabs');
    tl.addEventListener('dragstart', e => { const t = e.target.closest('[role=tab]'); if (!t || !S.canManage) return e.preventDefault(); drag = +t.dataset.i; t.classList.add('drag'); e.dataTransfer.effectAllowed = 'move'; e.dataTransfer.setData('text/plain', String(drag)); });
    tl.addEventListener('dragover', e => { const t = e.target.closest('[role=tab]'); if (drag == null || !t) return; e.preventDefault(); qa('#pdTabs .over').forEach(x => x.classList.remove('over')); t.classList.add('over'); });
    tl.addEventListener('drop', e => { const t = e.target.closest('[role=tab]'); if (drag == null || !t) return; e.preventDefault(); const a = drag; drag = null; move(a, +t.dataset.i); });
    tl.addEventListener('dragend', () => { drag = null; qa('#pdTabs .drag, #pdTabs .over').forEach(x => x.classList.remove('drag', 'over')); });
  }
  document.addEventListener('click', e => {
    const m = q('#pdMenu');
    if (m && !m.hidden && !e.target.closest('#pdMenu, [data-all]')) menu(false);
  });
  addEventListener('resize', arrows);
  addEventListener('hashchange', () => {
    if (!S.onScreen || !S.items.length) return;
    const i = fromHash(); if (i !== S.cur) activate(i, { push: false });
  });
  document.addEventListener('fullscreenchange', () => { if (!document.fullscreenElement && S.show) show(false); });

  /* ---------- إجراءات المالك والمحرر ---------- */
  async function editInChat(p) {
    if (!state.historyLoaded) await loadHistory();
    if (!state.history.some(h => h.id === p.dashboardId)) {
      toast('ليست لديك صلاحية تعديل هذه اللوحة — يعدّلها مالكها أو محرروها');
      return;
    }
    openHistoryEntry(p.dashboardId);
  }

  async function unpublish(p) {
    if (!confirm(`إلغاء نشر «${p.name}» من المشروع؟ اللوحة نفسها تبقى في السجل كما هي.`)) return;
    const res = await fetch('/api/published-dashboards/' + encodeURIComponent(p.id), { method: 'DELETE' });
    if (!res.ok) { const j = await res.json().catch(() => null); toast(j?.error || 'تعذّر إلغاء النشر'); return; }
    toast(`أُلغي نشر «${p.name}»`);
    S.sig = ''; S.cur = 0; load();
  }

  /* ---------- نافذة «نشر على المشروع» ---------- */
  const modal = () => document.getElementById('pubproj-modal');
  let existing = [];
  function modalError(msg) { const b = document.getElementById('pubproj-error'); b.textContent = msg || ''; b.classList.toggle('hidden', !msg); }

  async function openPublishModal(dashboardId) {
    modalError('');
    if (!state.historyLoaded) await loadHistory();
    // المرشحة للنشر: اللوحات النشطة التي يملكها المستخدم أو يحررها
    const isAdmin = state.currentUser?.role === 'Admin';
    const candidates = state.history.filter(h => h.isActive && (h.myRole === 'owner' || h.myRole === 'editor' || isAdmin));
    const sel = document.getElementById('pubproj-dashboard');
    if (!candidates.length) {
      toast('لا توجد لوحات نشطة تملكها أو تحررها — فعّل لوحة أولًا من «السجل»');
      return;
    }
    sel.innerHTML = candidates.map(h => `<option value="${esc(h.id)}">${esc(h.question)}</option>`).join('');
    if (dashboardId && candidates.some(h => h.id === dashboardId)) sel.value = dashboardId;
    sel.disabled = !!dashboardId;
    // النشرات الحالية — تُجلب دائمًا هنا لأن النافذة قد تُفتح من «اللوحات النشطة» قبل زيارة
    // هذه الشاشة، فلا يُعتمد على قائمة الشاشة المحمّلة.
    try {
      const res = await fetch('/api/published-dashboards');
      existing = res.ok ? (await res.json()).items : S.items;
    } catch { existing = S.items; }
    fillFromExisting();
    modal().classList.remove('hidden');
    (sel.disabled ? modal().querySelector('input[name="pubproj-aud"]:checked') : sel).focus();
  }
  // نشر سابق لنفس اللوحة؟ تُملأ إعداداته ويوضَّح أن الحفظ يرفع رقم الإصدار
  function fillFromExisting() {
    const id = document.getElementById('pubproj-dashboard').value;
    const ex = existing.find(p => p.dashboardId === id);
    const set = (name, v) => { const r = modal().querySelector(`input[name="${name}"][value="${v}"]`); if (r) r.checked = true; };
    set('pubproj-aud', ex?.audience || 'all');
    set('pubproj-mode', ex?.refreshMode || 'live');
    document.getElementById('pubproj-every').value = ex?.refreshEvery || '';
    document.getElementById('pubproj-version-note').textContent = ex
      ? `هذه اللوحة منشورة بالإصدار ${ex.version} — الحفظ يعيد نشرها بالإصدار ${ex.version + 1} في نفس موضعها.`
      : 'تُضاف اللوحة في آخر تبويبات «اللوحات المنشورة».';
    document.getElementById('pubproj-save').textContent = ex ? 'إعادة النشر' : 'نشر';
  }
  function closeModal() { modal().classList.add('hidden'); }

  function wireModal() {
    document.getElementById('pubproj-dashboard').addEventListener('change', fillFromExisting);
    document.getElementById('pubproj-cancel').addEventListener('click', closeModal);
    modal().addEventListener('keydown', e => { if (e.key === 'Escape') closeModal(); });
    document.getElementById('pubproj-form').addEventListener('submit', async e => {
      e.preventDefault(); modalError('');
      const btn = document.getElementById('pubproj-save'); btn.disabled = true;
      try {
        const body = {
          dashboardId: document.getElementById('pubproj-dashboard').value,
          audience: modal().querySelector('input[name="pubproj-aud"]:checked')?.value || 'all',
          refreshMode: modal().querySelector('input[name="pubproj-mode"]:checked')?.value || 'live',
          refreshEvery: document.getElementById('pubproj-every').value.trim() || null,
        };
        const res = await fetch('/api/published-dashboards', {
          method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body),
        });
        const json = await res.json().catch(() => null);
        if (!res.ok) { modalError(json?.error || 'تعذّر النشر.'); return; }
        closeModal();
        toast(json.version > 1 ? `أُعيد نشر «${json.name}» بالإصدار ${json.version}` : `نُشرت «${json.name}» على المشروع`);
        // افتح الشاشة على اللوحة المنشورة نفسها
        S.sig = '';
        history.replaceState(null, '', `${location.pathname}${location.search}#d=${encodeURIComponent(json.id)}`);
        if (S.onScreen) load(); else showScreen('published');
      } finally { btn.disabled = false; }
    });
    document.getElementById('pd-publish-btn').addEventListener('click', () => openPublishModal());
  }
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', wireModal); else wireModal();

  /* عند مغادرة الشاشة: يتوقف وضع العرض ويُزال الرابط العميق من شريط العنوان */
  function leave() {
    if (!S.onScreen) return;
    if (S.show) show(false);
    S.onScreen = false;
    if (location.hash) history.replaceState(null, '', location.pathname + location.search);
  }

  window.JEEM_PUBLISHED = { load, leave, openPublishModal };
})();
