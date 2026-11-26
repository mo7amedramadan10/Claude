/* ==========================================================
   تطبيق «جيم» للجوال (/m) — المرحلة 1: الهيكل والدخول والحساب والخروج.
   للاستعراض فقط: لا يطلب في مساره الأساسي إلا واجهات الدخول (/api/auth/*)، فيعمل لحساب «اطلاع فقط».
   الجلسة بالكوكي نفسه (ctd_auth) على الأصل نفسه؛ لا مفاتيح ولا أسرار في التخزين المحلي.
   ========================================================== */
(function () {
  'use strict';

  // مفاتيح التخزين المحلي (تفضيلات غير سرية فقط)
  const K_ORG = 'jeemMobileOrg';      // {slug, name} آخر منظمة دخل منها المستخدم
  const K_LANG = 'jeemMobileLang';    // ar | en
  const K_THEME = 'jeemMobileTheme';  // auto | light | dark
  const K_ENT = 'jeemMobileEntity';   // معرّف الكيان المختار (يُتجاهل إن لم يعد مسموحًا)

  const $ = (id) => document.getElementById(id);
  const store = {
    get(k) { try { return localStorage.getItem(k); } catch { return null; } },
    set(k, v) { try { localStorage.setItem(k, v); } catch { /* وضع خاص: نتجاهل */ } },
    del(k) { try { localStorage.removeItem(k); } catch { /* نتجاهل */ } },
  };

  // ---------- النصوص بلغتين ----------
  const T = {
    ar: {
      loading: 'جارٍ التحميل…', retry: 'إعادة المحاولة', close: 'إغلاق',
      'offline.title': 'لا يوجد اتصال',
      'offline.body': 'تعذّر الوصول إلى منصة جيم. تأكد من اتصالك بالشبكة (أو بالشبكة الداخلية) ثم حاول مجددًا.',
      'login.tag': 'لوحاتك وتقاريرك المنشورة، في جيبك.', 'login.title': 'تسجيل الدخول',
      'login.org': 'رابط المنظمة', 'login.orgHint': 'اسم المنظمة كما في رابط الدخول (…/o/اسم-المنظمة) أو الرابط كاملًا.',
      'login.changeOrg': 'تغيير', 'login.user': 'اسم المستخدم', 'login.pass': 'كلمة المرور',
      'login.show': 'إظهار كلمة المرور', 'login.hide': 'إخفاء كلمة المرور',
      'login.submit': 'دخول', 'login.busy': 'جارٍ الدخول…',
      'login.needOrg': 'اكتب رابط المنظمة أو اسمها.', 'login.needUser': 'اسم المستخدم وكلمة المرور مطلوبان.',
      'login.orgNotFound': 'لم نجد منظمة بهذا الاسم — تأكد من رابط الدخول الخاص بمنظمتك.',
      'login.failed': 'تعذّر الدخول. حاول مجددًا.', 'login.network': 'لا يوجد اتصال بالمنصة. تأكد من الشبكة ثم حاول مجددًا.',
      'lang.other': 'English',
      'home.hello': 'أهلًا،',
      'vw.title': 'للاطلاع', 'vw.sub': 'كل ما نُشر في كيانك: منشوراتك وما شاركه زملاؤك معك.',
      'vw.refresh': 'تحديث القائمة', 'vw.search': 'ابحث بالعنوان أو اسم المشارِك', 'vw.tabsLabel': 'تصفية القائمة',
      'vw.kindsLabel': 'تصفية بالنوع', 'vw.all': 'الكل', 'vw.new': 'جديد', 'vw.shared': 'مشترك معي', 'vw.mine': 'منشوراتي',
      'vw.allKinds': 'كل الأنواع', 'vw.loading': 'جارٍ تحميل القائمة…',
      'vw.count': '{n} عنصر', 'vw.countOf': '{n} من {all}',
      'vw.mineBy': 'منشورك', 'vw.from': 'من {name}', 'vw.colleague': 'زميل',
      'vw.via.project': 'مع الكيان', 'vw.via.org': 'مع المنظمة', 'vw.published': 'نُشر {ago}',
      'vw.newBadge': 'جديد', 'vw.editedMine': 'عدّلته بعد النشر — حدّث النشر من المنصة',
      'vw.editedOther': 'فيه تعديلات لم تُنشر بعد — الرابط يعرض النسخة المنشورة',
      'vw.opens': 'يفتح داخل التطبيق', 'vw.refreshed': 'تم تحديث القائمة.',
      'vw.empty.none': 'لا شيء منشور في كيانك بعد. ما تنشره أنت أو يشاركه زملاؤك معك وينشرونه يظهر هنا.',
      'vw.empty.new': 'لا جديد منذ آخر فتح — اطّلعت على كل شيء.',
      'vw.empty.mine': 'لم تنشر شيئًا في هذا الكيان بعد.',
      'vw.empty.shared': 'لا شيء مشترك معك في هذا الكيان بعد.',
      'vw.empty.filter': 'لا عناصر تطابق البحث أو النوع المختار.',
      'vw.empty.error': 'تعذّر تحميل القائمة. تأكد من الاتصال ثم حاول مجددًا.',
      'vw.empty.forbidden': 'حسابك لا يملك صلاحية «للاطلاع». اطلب من مسؤول المنصة إضافتها.',
      'kind.doc': 'مستند', 'kind.slides': 'شرائح', 'kind.design': 'تصميم', 'kind.dashboard': 'لوحة', 'kind.motion': 'حركة', 'kind.track': 'لوحة متابعة',
      'ent.title': 'اختر الكيان', 'ent.note': 'تعرض القائمة ما نُشر في الكيان المختار.', 'ent.generic': 'الكيان', 'ent.change': 'تغيير الكيان، الحالي: {name}',
      'ent.switched': 'صرت تعرض: {name}',
      'viewer.back': 'رجوع إلى القائمة', 'viewer.ext': 'فتح في المتصفح', 'viewer.loading': 'جارٍ فتح الصفحة…',
      'viewer.offline': 'لا يوجد اتصال الآن — الصفحة المنشورة تحتاج اتصالًا لعرض أحدث الأرقام.',
      'account.theme': 'المظهر', 'theme.auto': 'تلقائي', 'theme.light': 'فاتح', 'theme.dark': 'داكن',
      'account.title': 'حسابي', 'account.lang': 'اللغة', 'account.full': 'فتح المنصة كاملة',
      'account.logout': 'تسجيل الخروج', 'account.loggedOut': 'تم تسجيل الخروج.',
      'session.expired': 'انتهت الجلسة — سجّل الدخول مجددًا.',
    },
    en: {
      loading: 'Loading…', retry: 'Try again', close: 'Close',
      'offline.title': 'No connection',
      'offline.body': "Couldn't reach the Jeem platform. Check your network (or the internal network/VPN) and try again.",
      'login.tag': 'Your published dashboards and reports, in your pocket.', 'login.title': 'Sign in',
      'login.org': 'Organization link', 'login.orgHint': 'The organization name from your sign-in link (…/o/name) or the full link.',
      'login.changeOrg': 'Change', 'login.user': 'Username', 'login.pass': 'Password',
      'login.show': 'Show password', 'login.hide': 'Hide password',
      'login.submit': 'Sign in', 'login.busy': 'Signing in…',
      'login.needOrg': 'Enter your organization link or name.', 'login.needUser': 'Username and password are required.',
      'login.orgNotFound': "We couldn't find that organization — check your organization's sign-in link.",
      'login.failed': "Couldn't sign in. Please try again.", 'login.network': 'No connection to the platform. Check your network and try again.',
      'lang.other': 'العربية',
      'home.hello': 'Hello,',
      'vw.title': 'For Review', 'vw.sub': 'Everything published in your entity: yours and what colleagues shared with you.',
      'vw.refresh': 'Refresh list', 'vw.search': 'Search by title or who shared it', 'vw.tabsLabel': 'Filter the list',
      'vw.kindsLabel': 'Filter by type', 'vw.all': 'All', 'vw.new': 'New', 'vw.shared': 'Shared with me', 'vw.mine': 'My publications',
      'vw.allKinds': 'All types', 'vw.loading': 'Loading the list…',
      'vw.count': '{n} items', 'vw.countOf': '{n} of {all}',
      'vw.mineBy': 'Published by you', 'vw.from': 'From {name}', 'vw.colleague': 'a colleague',
      'vw.via.project': 'with the entity', 'vw.via.org': 'with the organization', 'vw.published': 'Published {ago}',
      'vw.newBadge': 'New', 'vw.editedMine': 'Edited after publishing — republish from the platform',
      'vw.editedOther': 'Has unpublished edits — the link shows the published version',
      'vw.opens': 'opens in the app', 'vw.refreshed': 'List refreshed.',
      'vw.empty.none': 'Nothing is published in your entity yet. What you publish, or colleagues share with you and publish, appears here.',
      'vw.empty.new': "Nothing new since your last visit — you're all caught up.",
      'vw.empty.mine': "You haven't published anything in this entity yet.",
      'vw.empty.shared': 'Nothing has been shared with you in this entity yet.',
      'vw.empty.filter': 'No items match your search or type.',
      'vw.empty.error': "Couldn't load the list. Check your connection and try again.",
      'vw.empty.forbidden': "Your account doesn't have access to For Review. Ask a platform admin to add it.",
      'kind.doc': 'Document', 'kind.slides': 'Slides', 'kind.design': 'Design', 'kind.dashboard': 'Dashboard', 'kind.motion': 'Motion', 'kind.track': 'Tracking dashboard',
      'ent.title': 'Choose an entity', 'ent.note': 'The list shows what is published in the chosen entity.', 'ent.generic': 'Entity', 'ent.change': 'Change entity, current: {name}',
      'ent.switched': 'Now showing: {name}',
      'viewer.back': 'Back to the list', 'viewer.ext': 'Open in browser', 'viewer.loading': 'Opening the page…',
      'viewer.offline': "You're offline — the published page needs a connection to show the latest numbers.",
      'account.theme': 'Appearance', 'theme.auto': 'Auto', 'theme.light': 'Light', 'theme.dark': 'Dark',
      'account.title': 'My account', 'account.lang': 'Language', 'account.full': 'Open the full platform',
      'account.logout': 'Sign out', 'account.loggedOut': 'Signed out.',
      'session.expired': 'Your session ended — please sign in again.',
    },
  };
  let lang = store.get(K_LANG) === 'en' ? 'en' : 'ar';
  const t = (k) => (T[lang] && T[lang][k]) || T.ar[k] || k;

  function applyLang() {
    const root = document.documentElement;
    root.lang = lang;
    root.dir = lang === 'ar' ? 'rtl' : 'ltr';
    document.querySelectorAll('[data-i]').forEach((el) => { el.textContent = t(el.dataset.i); });
    document.querySelectorAll('[data-i-aria]').forEach((el) => { el.setAttribute('aria-label', t(el.dataset.iAria)); });
    document.querySelectorAll('.seg-btn[data-lang]').forEach((b) => b.setAttribute('aria-pressed', String(b.dataset.lang === lang)));
    document.querySelectorAll('[data-i-ph]').forEach((el) => { el.placeholder = t(el.dataset.iPh); });
    syncPassEye();
    renderUser();
    if (me) { renderViewing(); renderEntityChip(); }
  }
  function setLang(next) {
    lang = next === 'en' ? 'en' : 'ar';
    store.set(K_LANG, lang);
    applyLang();
  }

  // ---------- طلبات الخادم ----------
  // كل الطلبات على الأصل نفسه بالكوكي؛ «offline» = تعذّر الاتصال (لا رد من الخادم).
  async function api(path, opts = {}) {
    let res;
    try {
      res = await fetch(path, {
        method: opts.method || 'GET',
        credentials: 'same-origin',
        cache: 'no-store',
        headers: Object.assign({ Accept: 'application/json' }, opts.body ? { 'Content-Type': 'application/json' } : {}),
        body: opts.body ? JSON.stringify(opts.body) : undefined,
      });
    } catch {
      return { ok: false, status: 0, offline: true, data: null };
    }
    let data = null;
    try { data = await res.json(); } catch { /* رد بلا جسم */ }
    return { ok: res.ok, status: res.status, offline: false, data };
  }

  // ---------- الشاشات ----------
  const SCREENS = ['scr-splash', 'scr-offline', 'scr-login', 'scr-home'];
  function show(id) {
    SCREENS.forEach((s) => { $(s).hidden = s !== id; });
    if (id !== 'scr-splash') $('scr-splash').removeAttribute('aria-busy');
  }

  let me = null;           // المستخدم الحالي من /api/auth/me
  let org = readOrg();     // {slug, name}

  function readOrg() {
    try { const o = JSON.parse(store.get(K_ORG) || 'null'); return o && o.slug ? o : null; } catch { return null; }
  }

  function initials(name) {
    const parts = String(name || '').trim().split(/\s+/).filter(Boolean);
    if (!parts.length) return '؟';
    // الحروف العربية تتصل ببعضها فتُقرأ كلمة، فنكتفي بحرف واحد؛ وللاتيني حرفان.
    const first = parts[0][0];
    if (/[\u0600-\u06FF]/.test(first)) return first;
    return (first + (parts[1] ? parts[1][0] : '')).toUpperCase();
  }

  function renderUser() {
    if (!me) return;
    const name = me.displayName || me.username;
    $('home-name').textContent = name;
    $('home-org').textContent = org && org.name ? org.name : '';
    $('avatar').textContent = initials(name);
    $('acc-avatar').textContent = initials(name);
    $('acc-name').textContent = name;
    $('acc-sub').textContent = [me.username, org && org.name].filter(Boolean).join(' · ');
  }

  async function boot() {
    show('scr-splash');
    const r = await api('/api/auth/me');
    if (r.offline) { show('scr-offline'); return; }
    if (r.ok && r.data) { me = r.data; enterApp(); return; }
    showLogin();
  }

  function enterApp() {
    renderUser();
    show('scr-home');
    $('home-main').focus({ preventScroll: true });
    loadViewing();
    loadEntities();
  }

  // ---------- الدخول ----------
  // يقبل الاسم المختصر أو رابط الدخول كاملًا (…/o/slug) أو رابطًا منشورًا (/p/slug/… أو /d/slug/…).
  function parseOrgSlug(raw) {
    let s = String(raw || '').trim();
    if (!s) return '';
    const m = s.match(/\/(?:o|p|d)\/([^/?#]+)/);
    if (m) s = m[1];
    try { s = decodeURIComponent(s); } catch { /* يبقى كما هو */ }
    return s.replace(/^\/+|\/+$/g, '').trim().toLowerCase();
  }

  function showLogin(message) {
    applyOrgChip();
    setError(message || '');
    show('scr-login');
    const first = org ? $('f-user') : $('f-org');
    first.focus({ preventScroll: true });
  }

  function applyOrgChip() {
    const has = !!(org && org.slug);
    $('org-chip').hidden = !has;
    $('org-field').hidden = has;
    if (has) $('org-chip-name').textContent = org.name || org.slug;
    else if (!$('f-org').value && org) $('f-org').value = org.slug;
  }

  function setError(msg) {
    const el = $('login-error');
    el.textContent = msg;
    el.hidden = !msg;
  }

  async function resolveOrg(slug) {
    const r = await api('/api/auth/org/' + encodeURIComponent(slug));
    if (r.offline) return { offline: true };
    if (!r.ok || !r.data) return { notFound: true };
    return { org: { slug: r.data.slug || slug, name: r.data.name || slug } };
  }

  async function onLogin(ev) {
    ev.preventDefault();
    setError('');
    const btn = $('login-submit');
    const userEl = $('f-user'), passEl = $('f-pass'), orgEl = $('f-org');
    [userEl, passEl, orgEl].forEach((el) => el.removeAttribute('aria-invalid'));

    let target = org;
    if (!target) {
      const slug = parseOrgSlug(orgEl.value);
      if (!slug) { orgEl.setAttribute('aria-invalid', 'true'); setError(t('login.needOrg')); orgEl.focus(); return; }
      target = { slug, name: '' };
    }
    const username = userEl.value.trim();
    const password = passEl.value;
    if (!username || !password) {
      (username ? passEl : userEl).setAttribute('aria-invalid', 'true');
      setError(t('login.needUser'));
      (username ? passEl : userEl).focus();
      return;
    }

    btn.disabled = true;
    btn.firstElementChild.textContent = t('login.busy');
    try {
      // التحقق من المنظمة أولًا (لاسمها ولرسالة أوضح من «اسم المستخدم أو كلمة المرور غير صحيحة»).
      if (!target.name) {
        const o = await resolveOrg(target.slug);
        if (o.offline) { setError(t('login.network')); return; }
        if (o.notFound) { orgEl.setAttribute('aria-invalid', 'true'); setError(t('login.orgNotFound')); orgEl.focus(); return; }
        target = o.org;
      }
      const r = await api('/api/auth/login', { method: 'POST', body: { username, password, orgSlug: target.slug } });
      if (r.offline) { setError(t('login.network')); return; }
      if (!r.ok) {
        setError((r.data && r.data.error) || t('login.failed'));
        passEl.value = '';
        passEl.focus();
        return;
      }
      org = target;
      store.set(K_ORG, JSON.stringify(org));
      me = r.data;
      passEl.value = '';
      setPassVisible(false);
      enterApp();
    } finally {
      btn.disabled = false;
      btn.firstElementChild.textContent = t('login.submit');
    }
  }

  function setPassVisible(on) {
    $('f-pass').type = on ? 'text' : 'password';
    $('pass-eye').setAttribute('aria-pressed', String(on));
    syncPassEye();
  }
  function syncPassEye() {
    const on = $('pass-eye').getAttribute('aria-pressed') === 'true';
    $('pass-eye').setAttribute('aria-label', t(on ? 'login.hide' : 'login.show'));
  }

  // ---------- حسابي ----------
  let sheetReturnFocus = null;
  function openSheet() {
    sheetReturnFocus = document.activeElement;
    $('account-backdrop').hidden = false;
    $('account-sheet').hidden = false;
    $('open-account').setAttribute('aria-expanded', 'true');
    $('account-close').focus();
  }
  function closeSheet() {
    if ($('account-sheet').hidden) return;
    $('account-backdrop').hidden = true;
    $('account-sheet').hidden = true;
    $('open-account').setAttribute('aria-expanded', 'false');
    if (sheetReturnFocus && document.contains(sheetReturnFocus)) sheetReturnFocus.focus();
  }
  // حبس التركيز داخل الورقة أثناء فتحها
  function trapFocus(ev) {
    const dlg = ['ent-sheet', 'account-sheet', 'scr-viewer'].map($).find((d) => !d.hidden);
    if (ev.key !== 'Tab' || !dlg) return;
    const items = [...dlg.querySelectorAll('button, a[href], iframe')].filter((el) => !el.disabled && el.offsetParent !== null);
    if (!items.length) return;
    const first = items[0], last = items[items.length - 1];
    if (ev.shiftKey && document.activeElement === first) { ev.preventDefault(); last.focus(); }
    else if (!ev.shiftKey && document.activeElement === last) { ev.preventDefault(); first.focus(); }
  }

  async function logout() {
    const btn = $('logout-btn');
    btn.disabled = true;
    await api('/api/auth/logout', { method: 'POST' });
    btn.disabled = false;
    me = null;
    resetViewing();
    closeSheet();
    toast(t('account.loggedOut'));
    showLogin();
  }

  let toastTimer = 0;
  function toast(msg) {
    const el = $('toast');
    el.textContent = msg;
    el.hidden = false;
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => { el.hidden = true; }, 3200);
  }

  // عند العودة للتطبيق بعد غياب: تحقّق أن الجلسة ما زالت صالحة (قد تنتهي أو يُوقف الحساب).
  async function recheckSession() {
    if (!me || document.visibilityState !== 'visible') return;
    const r = await api('/api/auth/me');
    if (r.offline) return; // بلا اتصال: لا نُخرج المستخدم
    if (!r.ok) { me = null; resetViewing(); closeSheet(); showLogin(t('session.expired')); return; }
    me = r.data;
    renderUser();
    loadViewing({ quiet: true });
  }

  // ---------- «للاطلاع»: كل ما نُشر في الكيان الحالي (منشوراتي + ما شورك معي) ----------
  // المصدر الوحيد GET /api/viewing (مقصور على الكيان الحالي في الخادم). «جديد» محلي لكل مشاهد بالمفتاح نفسه الذي
  // تستخدمه المنصة (jeemViewSeen: مفتاح العنصر ← تاريخ النشر عند آخر فتح) فيتّفق الجوال وسطح المكتب على المتصفح نفسه.
  // لا تُخزَّن القائمة ولا المعاينات خارج الذاكرة (بيانات عملاء وصلاحيات تتغيّر).
  const K_SEEN = 'jeemViewSeen';
  const KINDS = ['doc', 'slides', 'design', 'dashboard', 'motion', 'track'];
  const VW = { items: null, state: 'idle', tab: 'all', q: '', kind: '', shown: [], html: {}, queue: [], active: 0, io: null, ro: null, seq: 0 };

  const nf = () => new Intl.NumberFormat('en-US');
  const fmt = (k, vars) => t(k).replace(/\{(\w+)\}/g, (_, v) => (vars && vars[v] != null ? vars[v] : ''));
  const esc = (v) => String(v == null ? '' : v).replace(/[&<>"']/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
  const utc = (iso) => (iso && !/[zZ]|[+-]\d\d:?\d\d$/.test(iso) ? iso + 'Z' : iso);
  const kindOf = (x) => (x.source === 'dashboard' ? 'track' : KINDS.includes(x.kind) ? x.kind : 'design');

  function seenMap() { try { return JSON.parse(store.get(K_SEEN) || '{}') || {}; } catch { return {}; } }
  function markSeen(x) { const s = seenMap(); s[x.key] = x.publishedAt; store.set(K_SEEN, JSON.stringify(s)); }
  function isNew(x, seen) { if (x.mine) return false; const v = (seen || seenMap())[x.key]; return !v || new Date(x.publishedAt) > new Date(utc(v)); }

  // «قبل ٣ أيام» بجمع عربي صحيح (القاعدة نفسها في المنصة)، و«3 days ago» بالإنجليزية
  function ago(iso) {
    const ms = Date.now() - new Date(iso).getTime(); if (!(ms >= 0)) return '';
    const m = Math.floor(ms / 60000), h = Math.floor(m / 60), d = Math.floor(h / 24), mo = Math.floor(d / 30), y = Math.floor(mo / 12);
    if (lang === 'en') {
      const en = (n, u) => `${n} ${u}${n === 1 ? '' : 's'} ago`;
      return m < 1 ? 'just now' : m < 60 ? en(m, 'minute') : h < 24 ? en(h, 'hour') : d < 30 ? en(d, 'day') : mo < 12 ? en(mo, 'month') : en(y, 'year');
    }
    const ar = (n, one, two, few, many) => (n === 1 ? one : n === 2 ? two : n <= 10 ? `${n} ${few}` : `${n} ${many}`);
    if (m < 1) return 'الآن';
    if (m < 60) return 'قبل ' + ar(m, 'دقيقة', 'دقيقتين', 'دقائق', 'دقيقة');
    if (h < 24) return 'قبل ' + ar(h, 'ساعة', 'ساعتين', 'ساعات', 'ساعة');
    if (d < 30) return 'قبل ' + ar(d, 'يوم', 'يومين', 'أيام', 'يومًا');
    if (mo < 12) return 'قبل ' + ar(mo, 'شهر', 'شهرين', 'أشهر', 'شهرًا');
    return 'قبل ' + ar(y, 'سنة', 'سنتين', 'سنوات', 'سنة');
  }

  async function loadViewing(opts = {}) {
    const seq = ++VW.seq;
    if (!opts.quiet || !VW.items) { VW.state = 'loading'; renderViewing(); }
    const btn = $('vw-refresh'); btn.classList.add('spin'); btn.disabled = true;
    const r = await api('/api/viewing' + entQuery('?'));
    btn.classList.remove('spin'); btn.disabled = false;
    if (seq !== VW.seq) return;
    if (r.status === 401) { me = null; resetViewing(); showLogin(t('session.expired')); return; }
    if (r.ok && r.data) {
      if (r.data.entityId) { ENT.current = r.data.entityId; renderEntityChip(); }
      VW.items = (r.data.items || []).map((x) => Object.assign({}, x, { publishedAt: utc(x.publishedAt) }))
        .sort((a, b) => String(b.publishedAt || '').localeCompare(String(a.publishedAt || '')));
      VW.state = 'ready';
      if (opts.announce) toast(t('vw.refreshed'));
    } else if (!(opts.quiet && VW.items)) {
      // تحديث صامت فاشل (بلا اتصال مثلًا) يُبقي القائمة المعروضة كما هي
      VW.state = r.status === 403 ? 'forbidden' : 'error';
    }
    renderViewing();
  }

  function baseItems() {
    const q = VW.q.trim().toLowerCase(); const seen = seenMap();
    return (VW.items || []).filter((x) => (VW.tab === 'all' || (VW.tab === 'new' ? isNew(x, seen) : VW.tab === 'mine' ? !!x.mine : !x.mine))
      && (!q || String(x.title || '').toLowerCase().includes(q) || String(x.ownerName || '').toLowerCase().includes(q)));
  }

  function kindIcon(k) { return `<span class="kico k-${k}" aria-hidden="true"><svg><use href="#mk-${k}"/></svg></span>`; }

  function renderChips(base) {
    const box = $('vw-chips');
    const counts = {}; base.forEach((x) => { const k = kindOf(x); counts[k] = (counts[k] || 0) + 1; });
    const kinds = KINDS.filter((k) => counts[k] || VW.kind === k);
    box.hidden = !(VW.items && VW.items.length);
    const chip = (k, label, n) => `<button type="button" class="chip${k ? ' k-' + k : ' all'}" data-kind="${k}" aria-pressed="${VW.kind === k}">${k ? kindIcon(k) : ''}<span>${esc(label)}</span><span class="n">${nf().format(n)}</span></button>`;
    box.innerHTML = chip('', t('vw.allKinds'), base.length) + kinds.map((k) => chip(k, t('kind.' + k), counts[k] || 0)).join('');
  }

  // مصغّرة لوحة المتابعة: شكل عناصرها فقط (النوع والحجم من الخادم، بلا بيانات)
  const GLYPH = {
    kpi: '<svg viewBox="0 0 40 20"><rect x="4" y="5" width="16" height="5" rx="1.5" fill="var(--chart-1)"/><rect x="4" y="13" width="26" height="2.5" rx="1.2" fill="#D3DAE1"/></svg>',
    line: '<svg viewBox="0 0 80 30" preserveAspectRatio="none"><polyline points="2,26 14,20 26,22 38,14 50,15 62,8 78,4" fill="none" stroke="var(--chart-1)" stroke-width="2.4" vector-effect="non-scaling-stroke"/></svg>',
    donut: '<svg viewBox="0 0 30 30"><circle cx="15" cy="15" r="9" fill="none" stroke="var(--chart-1)" stroke-width="5" stroke-dasharray="26 60" transform="rotate(-90 15 15)"/><circle cx="15" cy="15" r="9" fill="none" stroke="var(--chart-2)" stroke-width="5" stroke-dasharray="17 60" stroke-dashoffset="-27" transform="rotate(-90 15 15)"/></svg>',
    bars: '<svg viewBox="0 0 40 24">' + [30, 22, 16, 10].map((w, i) => `<rect x="${38 - w}" y="${2 + i * 6}" width="${w}" height="4" rx="1.2" fill="var(--chart-1)"/>`).join('') + '</svg>',
    cols: '<svg viewBox="0 0 40 24">' + [10, 16, 12, 20].map((h, i) => `<rect x="${4 + i * 9}" y="${22 - h}" width="6" height="${h}" rx="1" fill="var(--chart-1)"/>`).join('') + '</svg>',
    table: '<svg viewBox="0 0 40 24"><rect x="2" y="2" width="36" height="4" rx="1" fill="#D3DAE1"/>' + [0, 1, 2].map((i) => `<rect x="2" y="${9 + i * 5}" width="${18 - i * 3}" height="2.5" rx="1" fill="var(--chart-1)"/><rect x="28" y="${9 + i * 5}" width="10" height="2.5" rx="1" fill="#D3DAE1"/>`).join('') + '</svg>',
    funnel: '<svg viewBox="0 0 40 24">' + [34, 26, 18, 10].map((w, i) => `<rect x="${(40 - w) / 2}" y="${2 + i * 5.5}" width="${w}" height="4" rx="1" fill="var(--chart-1)"/>`).join('') + '</svg>',
    heat: '<svg viewBox="0 0 40 24">' + Array.from({ length: 18 }, (_, i) => `<rect x="${3 + (i % 6) * 6}" y="${3 + Math.floor(i / 6) * 6.5}" width="5" height="5.5" rx="1" fill="var(--chart-1)" opacity="${[0.25, 0.5, 0.8, 1, 0.4, 0.65][(i * 5) % 6]}"/>`).join('') + '</svg>',
  };
  function glyphOf(type) {
    const ty = String(type || '').toLowerCase();
    if (ty === 'kpi') return 'kpi';
    if (/line|area|forecast|spark/.test(ty)) return 'line';
    if (/pie|donut|radial|gauge/.test(ty)) return 'donut';
    if (/table/.test(ty)) return 'table';
    if (/funnel|stage/.test(ty)) return 'funnel';
    if (/heat|treemap|matrix/.test(ty)) return 'heat';
    if (/column|multi/.test(ty)) return 'cols';
    return 'bars';
  }
  function dashSketch(x) {
    if (x.renderKind) return `<div class="vw-paper engine"><svg aria-hidden="true"><use href="#mk-track"/></svg></div>`;
    const ws = (x.widgets || []).slice(0, 12);
    if (!ws.length) return `<div class="vw-paper engine"><svg aria-hidden="true"><use href="#mk-track"/></svg></div>`;
    const cells = ws.map((w) => {
      const g = glyphOf(w.type); const sz = String(w.size || '').toLowerCase();
      const span = g === 'kpi' ? 'w3' : g === 'table' || sz === 'full' || sz === 'large' ? 'w12' : sz === 'small' ? 'w3' : 'w6';
      return `<i class="${span}${g === 'kpi' ? '' : ' tall'}">${GLYPH[g]}</i>`;
    }).join('');
    return `<div class="vw-paper sketch"><div class="sk">${cells}</div></div>`;
  }

  function viaText(x) {
    if (x.mine) return t('vw.mineBy');
    const from = fmt('vw.from', { name: x.ownerName || t('vw.colleague') });
    return x.sharedVia === 'project' || x.sharedVia === 'org' ? `${from} · ${t('vw.via.' + x.sharedVia)}` : from;
  }

  function renderViewing() {
    if (!$('vw-list')) return;
    const list = $('vw-list'), empty = $('vw-empty');
    document.querySelectorAll('#vw-tabs .tab').forEach((b) => {
      const on = b.dataset.tab === VW.tab; b.setAttribute('aria-selected', String(on)); b.tabIndex = on ? 0 : -1;
    });
    const seen = seenMap();
    const newN = (VW.items || []).filter((x) => isNew(x, seen)).length;
    $('vw-new-n').textContent = newN ? nf().format(newN) : '';
    list.setAttribute('aria-busy', String(VW.state === 'loading'));

    if (VW.state === 'loading' && !VW.items) {
      $('vw-chips').hidden = true; $('vw-count').textContent = t('vw.loading'); empty.hidden = true;
      list.innerHTML = Array.from({ length: 3 }, () => '<div class="vw-card skel" aria-hidden="true"><div class="vw-art"></div><div class="vw-foot"><div class="vw-meta"><div class="line"></div><div class="line"></div></div></div></div>').join('');
      return;
    }
    if (VW.state === 'error' || VW.state === 'forbidden') {
      list.innerHTML = ''; $('vw-chips').hidden = true; $('vw-count').textContent = '';
      $('vw-empty-text').textContent = t(VW.state === 'error' ? 'vw.empty.error' : 'vw.empty.forbidden');
      $('vw-empty-retry').hidden = VW.state !== 'error'; empty.hidden = false;
      return;
    }

    const base = baseItems(); renderChips(base);
    const items = VW.shown = base.filter((x) => !VW.kind || kindOf(x) === VW.kind);
    const all = (VW.items || []).length;
    $('vw-count').textContent = all ? (items.length === all ? fmt('vw.count', { n: nf().format(all) }) : fmt('vw.countOf', { n: nf().format(items.length), all: nf().format(all) })) : '';
    $('vw-empty-retry').hidden = true;
    if (!items.length) {
      list.innerHTML = '';
      $('vw-empty-text').textContent = t(!all ? 'vw.empty.none' : VW.q || VW.kind ? 'vw.empty.filter' : 'vw.empty.' + (VW.tab === 'all' ? 'filter' : VW.tab));
      empty.hidden = false;
      return;
    }
    empty.hidden = true;
    list.innerHTML = items.map((x, i) => {
      const k = kindOf(x); const fresh = isNew(x, seen); const via = viaText(x);
      const art = x.source === 'dashboard' ? dashSketch(x)
        : `<div class="vw-paper"><span class="ph"><svg aria-hidden="true"><use href="#mk-${k}"/></svg></span></div>`;
      const edited = x.editedAfterPublish ? `<small class="edited"><svg aria-hidden="true"><use href="#mi-edit"/></svg>${esc(t(x.mine ? 'vw.editedMine' : 'vw.editedOther'))}</small>` : '';
      const label = [x.title, t('kind.' + k), via, fresh ? t('vw.newBadge') : '', fmt('vw.published', { ago: ago(x.publishedAt) }), t('vw.opens')].filter(Boolean).join('، ');
      return `<a class="vw-card k-${k}" href="${esc(x.url)}" data-card="${i}" data-key="${esc(x.key)}" aria-label="${esc(label)}">
        <div class="vw-art">${art}${fresh ? `<span class="badge-new"><svg aria-hidden="true"><use href="#mi-new"/></svg>${esc(t('vw.newBadge'))}</span>` : ''}</div>
        <div class="vw-foot">${kindIcon(k)}<div class="vw-meta"><b>${esc(x.title)}</b><small>${esc(via)} · ${esc(fmt('vw.published', { ago: ago(x.publishedAt) }))}</small>${edited}</div>
        <svg class="ico vw-go" aria-hidden="true"><use href="#mi-open"/></svg></div></a>`;
    }).join('');
    scaleThumbs();
    observeThumbs();
  }

  // ---- معاينات التصاميم: الصفحة المنشورة نفسها في إطار معزول صامت، تُجلب عند الظهور فقط وبحد اثنين معًا ----
  const QUIET = '<style id="m-thumb-quiet">#jeem-strip,#jm-controls,.jm-bar{display:none!important}html{--jeem-strip-h:0px!important}'
    + '@media (prefers-reduced-motion: reduce){*,*::before,*::after{animation:none!important;transition:none!important}}</style>'
    + '<script>(function(){var n=function(){};try{HTMLElement.prototype.focus=n;Element.prototype.scrollIntoView=n;window.scrollTo=n;window.scrollBy=n;window.focus=n;}catch(e){}})();<\/script>';
  function thumbDoc(html) {
    let out = html;
    const h = out.search(/<head[^>]*>/i);
    out = h >= 0 ? out.slice(0, out.indexOf('>', h) + 1) + QUIET + out.slice(out.indexOf('>', h) + 1) : QUIET + out;
    let kit = '';
    if (/data-jeem-(chart|value)\b/.test(out) && !/jeem-blocks\.js/.test(out)) kit += '<script src="/js/jeem-viz.js"><\/script><script src="/js/jeem-blocks.js"><\/script>';
    if (/data-jeem-motion\b/.test(out) && !/jeem-motion\.js/.test(out)) kit += '<script src="/js/jeem-motion.js"><\/script>';
    if (kit) { const b = out.toLowerCase().lastIndexOf('</body>'); out = b >= 0 ? out.slice(0, b) + kit + out.slice(b) : out + kit; }
    return out;
  }
  function scaleThumbs() {
    const p = $('vw-list').querySelector('.vw-paper');
    if (p && p.clientWidth) $('vw-list').style.setProperty('--s', (p.clientWidth / 1100).toFixed(4));
  }
  function observeThumbs() {
    if (!('IntersectionObserver' in window)) return;
    if (!VW.io) VW.io = new IntersectionObserver((en) => en.forEach((e) => { if (e.isIntersecting) { VW.io.unobserve(e.target); enqueueThumb(e.target); } }), { rootMargin: '300px 0px' });
    VW.io.disconnect();
    $('vw-list').querySelectorAll('.vw-card:not(.k-track)').forEach((c) => VW.io.observe(c));
  }
  function enqueueThumb(card) { VW.queue.push(card); pumpThumbs(); }
  function pumpThumbs() {
    while (VW.active < 2 && VW.queue.length) {
      const card = VW.queue.shift(); if (!document.contains(card)) continue;
      VW.active++; loadThumb(card).finally(() => { VW.active--; pumpThumbs(); });
    }
  }
  async function loadThumb(card) {
    const x = VW.shown[+card.dataset.card]; const paper = card.querySelector('.vw-paper');
    if (!x || !x.id || !paper || x.source !== 'freeform') return;
    let html = VW.html[x.id];
    if (html == null) {
      const r = await api('/api/viewing/thumb/' + encodeURIComponent(x.id) + entQuery('?'));
      html = r.ok && r.data ? r.data.html || '' : '';
      if (!r.offline) VW.html[x.id] = html;
    }
    if (!html || !document.contains(card)) return;
    const f = document.createElement('iframe');
    f.setAttribute('sandbox', 'allow-scripts'); f.setAttribute('tabindex', '-1'); f.setAttribute('aria-hidden', 'true');
    f.setAttribute('loading', 'lazy'); f.title = '';
    f.srcdoc = thumbDoc(html);
    paper.appendChild(f);
    f.addEventListener('load', () => { const ph = paper.querySelector('.ph'); if (ph) ph.remove(); }, { once: true });
  }

  function bindViewing() {
    $('vw-refresh').addEventListener('click', () => { VW.html = {}; loadViewing({ quiet: true, announce: true }); });
    $('vw-empty-retry').addEventListener('click', () => loadViewing());
    let qTimer = 0;
    $('vw-search').addEventListener('input', () => { clearTimeout(qTimer); qTimer = setTimeout(() => { VW.q = $('vw-search').value; renderViewing(); }, 150); });
    const tabs = [...document.querySelectorAll('#vw-tabs .tab')];
    tabs.forEach((b) => b.addEventListener('click', () => { VW.tab = b.dataset.tab; renderViewing(); }));
    // لوحة المفاتيح على التبويبات (نمط tablist): الأسهم بحسب الاتجاه، و Home/End
    $('vw-tabs').addEventListener('keydown', (ev) => {
      const i = tabs.findIndex((b) => b.dataset.tab === VW.tab); let j = -1;
      const fwd = lang === 'ar' ? 'ArrowLeft' : 'ArrowRight', back = lang === 'ar' ? 'ArrowRight' : 'ArrowLeft';
      if (ev.key === fwd) j = (i + 1) % tabs.length; else if (ev.key === back) j = (i - 1 + tabs.length) % tabs.length;
      else if (ev.key === 'Home') j = 0; else if (ev.key === 'End') j = tabs.length - 1;
      if (j < 0) return;
      ev.preventDefault(); VW.tab = tabs[j].dataset.tab; renderViewing(); tabs[j].focus();
    });
    $('vw-chips').addEventListener('click', (ev) => {
      const b = ev.target.closest('.chip'); if (!b) return;
      VW.kind = b.dataset.kind; renderViewing();
      const again = $('vw-chips').querySelector(`.chip[data-kind="${b.dataset.kind}"]`); if (again) again.focus();
    });
    $('vw-list').addEventListener('click', (ev) => {
      const a = ev.target.closest('.vw-card'); if (!a || a.classList.contains('skel')) return;
      const x = VW.shown[+a.dataset.card]; if (!x) return;
      // نقرة عادية: العارض داخل التطبيق؛ مع Ctrl/Cmd/Shift أو الزر الأوسط يترك المتصفح يفتحها كما يريد
      if (ev.ctrlKey || ev.metaKey || ev.shiftKey || ev.button === 1) { markSeen(x); setTimeout(renderViewing, 0); return; }
      ev.preventDefault();
      openViewer(x);
    });
    if ('ResizeObserver' in window) { VW.ro = new ResizeObserver(scaleThumbs); VW.ro.observe($('vw-list')); }
  }
  function resetViewing() {
    VW.items = null; VW.state = 'idle'; VW.html = {}; VW.queue = []; VW.shown = []; VW.seq++;
    VW.tab = 'all'; VW.q = ''; VW.kind = '';
    ENT.list = null; ENT.current = null; closeViewer(true);
    if ($('vw-search')) $('vw-search').value = '';
    if ($('vw-list')) $('vw-list').innerHTML = '';
  }

  // ---------- الكيان: اختيار ما تعرضه القائمة (لمن له أكثر من كيان) ----------
  // المنصة تحسم «الكيان الحالي» في الخادم (أول كيان للمستخدم)، فيمرّر التطبيق اختياره بمعامل entity في /api/viewing
  // ويتحقق الخادم أنه مما يحق له وإلا تجاهله. الاختيار تفضيل محلي غير سري (معرّف فقط).
  const ENT = { list: null, types: [], current: null, choice: store.get(K_ENT) || null };
  function entQuery(prefix) { return ENT.choice ? `${prefix}entity=${encodeURIComponent(ENT.choice)}` : ''; }
  function entTypeLabel(e) {
    const ty = (ENT.types || []).find((x) => x.id === (e && e.typeId));
    if (lang === 'en') return (ty && ty.singularEn) || t('ent.generic');
    return (ty && ty.singular) || t('ent.generic');
  }
  function entShown() {
    const id = ENT.choice || ENT.current;
    return (ENT.list || []).find((e) => e.id === id) || null;
  }
  async function loadEntities() {
    const r = await api('/api/entities');
    if (!r.ok || !r.data) { ENT.list = null; renderEntityChip(); return; } // حساب بلا صلاحية عليها: لا شريحة، والقائمة على الكيان الافتراضي
    ENT.list = r.data.entities || []; ENT.types = r.data.types || [];
    if (ENT.choice && !ENT.list.some((e) => e.id === ENT.choice)) { ENT.choice = null; store.del(K_ENT); loadViewing({ quiet: true }); }
    renderEntityChip();
  }
  function renderEntityChip() {
    const chip = $('ent-chip'); if (!chip) return;
    const e = entShown();
    chip.hidden = !(ENT.list && ENT.list.length > 1 && e);
    if (chip.hidden) return;
    $('ent-chip-type').textContent = entTypeLabel(e) + ':';
    $('ent-chip-name').textContent = e.name;
    chip.setAttribute('aria-label', fmt('ent.change', { name: e.name }));
  }
  let entReturn = null;
  function openEntSheet() {
    entReturn = document.activeElement;
    const cur = entShown();
    $('ent-list').innerHTML = (ENT.list || []).map((e) => `<button type="button" class="ent-opt" role="radio" aria-checked="${cur && cur.id === e.id}" data-id="${esc(e.id)}" tabindex="${cur && cur.id === e.id ? 0 : -1}">
      <span><span class="ent-type">${esc(entTypeLabel(e))}</span><span class="ent-name">${esc(e.name)}</span></span>
      <svg class="ent-tick" aria-hidden="true"><use href="#mi-check"/></svg></button>`).join('');
    $('ent-backdrop').hidden = false; $('ent-sheet').hidden = false;
    $('ent-chip').setAttribute('aria-expanded', 'true');
    ($('ent-list').querySelector('[aria-checked="true"]') || $('ent-list').querySelector('.ent-opt') || $('ent-close')).focus();
  }
  function closeEntSheet() {
    if ($('ent-sheet').hidden) return;
    $('ent-backdrop').hidden = true; $('ent-sheet').hidden = true;
    $('ent-chip').setAttribute('aria-expanded', 'false');
    if (entReturn && document.contains(entReturn)) entReturn.focus();
  }
  function pickEntity(id) {
    const e = (ENT.list || []).find((x) => x.id === id); if (!e) return;
    closeEntSheet();
    if (id === (ENT.choice || ENT.current)) return;
    ENT.choice = id; store.set(K_ENT, id);
    VW.html = {}; VW.kind = ''; renderEntityChip();
    loadViewing().then(() => toast(fmt('ent.switched', { name: e.name })));
  }

  // ---------- العارض داخل التطبيق ----------
  // يفتح الرابط المنشور نفسه (/p/… أو /d/…) ملء الشاشة بزر رجوع — صفحتا النشر فيهما الأرقام الحيّة والتنقّل والتصدير
  // ومساعد جيم، فلا يُعاد بناؤها. زر الرجوع في الجهاز يغلقه (pushState)، وإغلاقه يفرّغ الإطار فتتوقف أي طلبات دورية فيه.
  const VIEW = { key: null, url: null, pushed: false };
  function openViewer(x) {
    markSeen(x);
    const k = kindOf(x);
    VIEW.key = x.key; VIEW.url = x.url;
    $('viewer-title').textContent = x.title || '';
    $('viewer-sub').textContent = `${t('kind.' + k)} · ${viaText(x)}`;
    $('viewer-ext').href = x.url;
    $('scr-viewer').hidden = false;
    $('scr-home').inert = true;
    document.body.classList.add('viewer-open');
    if (!VIEW.pushed) { history.pushState({ mView: x.key }, '', '#view'); VIEW.pushed = true; }
    loadFrame();
    $('viewer-back').focus();
  }
  function loadFrame() {
    $('viewer-offline').hidden = true;
    if (navigator.onLine === false) { $('viewer-loading').hidden = true; $('viewer-offline').hidden = false; return; }
    $('viewer-loading').hidden = false;
    // إطار جديد في كل مرة: أول تحميل لإطار يُضاف للصفحة لا يُنشئ قيدًا في سجل التصفّح (بخلاف تغيير src لإطار قائم)
    removeFrame();
    const f = document.createElement('iframe');
    f.id = 'viewer-frame'; f.title = $('viewer-title').textContent;
    f.setAttribute('allow', 'fullscreen; clipboard-write; microphone');
    f.addEventListener('load', () => { $('viewer-loading').hidden = true; });
    f.src = VIEW.url;
    $('scr-viewer').querySelector('.viewer-body').appendChild(f);
  }
  function removeFrame() { const f = $('viewer-frame'); if (f) f.remove(); }
  function closeViewer(silent) {
    if ($('scr-viewer').hidden) return;
    $('scr-viewer').hidden = true;
    $('scr-home').inert = false;
    document.body.classList.remove('viewer-open');
    removeFrame();
    VIEW.pushed = false;
    const key = VIEW.key; VIEW.key = null;
    if (silent) return;
    renderViewing();
    const card = [...$('vw-list').querySelectorAll('.vw-card')].find((c) => c.dataset.key === key);
    (card || $('home-main')).focus({ preventScroll: false });
  }

  // ---------- المظهر: تلقائي (يتبع النظام) أو فاتح أو داكن ----------
  const darkMq = window.matchMedia ? matchMedia('(prefers-color-scheme: dark)') : null;
  function themePref() { const v = store.get(K_THEME); return v === 'light' || v === 'dark' ? v : 'auto'; }
  function applyTheme() {
    const pref = themePref();
    const dark = pref === 'dark' || (pref === 'auto' && darkMq && darkMq.matches);
    if (dark) document.documentElement.setAttribute('data-theme', 'dark'); else document.documentElement.removeAttribute('data-theme');
    document.querySelectorAll('.seg-btn[data-theme-set]').forEach((b) => b.setAttribute('aria-pressed', String(b.dataset.themeSet === pref)));
    const meta = document.querySelector('meta[name="theme-color"]'); if (meta) meta.content = dark ? '#181D23' : '#127DBD';
  }

  function bindPhase3() {
    $('ent-chip').addEventListener('click', openEntSheet);
    $('ent-close').addEventListener('click', closeEntSheet);
    $('ent-backdrop').addEventListener('click', closeEntSheet);
    $('ent-list').addEventListener('click', (ev) => { const b = ev.target.closest('.ent-opt'); if (b) pickEntity(b.dataset.id); });
    // مجموعة اختيار: الأسهم تنقل بين الخيارات (بلا اختيار حتى Enter/مسافة)
    $('ent-list').addEventListener('keydown', (ev) => {
      const opts = [...$('ent-list').querySelectorAll('.ent-opt')]; const i = opts.indexOf(document.activeElement); if (i < 0) return;
      let j = -1;
      if (ev.key === 'ArrowDown' || ev.key === (lang === 'ar' ? 'ArrowLeft' : 'ArrowRight')) j = (i + 1) % opts.length;
      else if (ev.key === 'ArrowUp' || ev.key === (lang === 'ar' ? 'ArrowRight' : 'ArrowLeft')) j = (i - 1 + opts.length) % opts.length;
      if (j < 0) return;
      ev.preventDefault(); opts.forEach((o) => { o.tabIndex = -1; }); opts[j].tabIndex = 0; opts[j].focus();
    });
    $('viewer-back').addEventListener('click', () => { if (VIEW.pushed && history.state && history.state.mView) history.back(); else closeViewer(); });
    $('viewer-retry').addEventListener('click', loadFrame);
    window.addEventListener('popstate', () => { if (!$('scr-viewer').hidden) { VIEW.pushed = false; closeViewer(); } });
    window.addEventListener('offline', () => { if (!$('scr-viewer').hidden && !$('viewer-loading').hidden) { $('viewer-loading').hidden = true; $('viewer-offline').hidden = false; } });
    document.querySelectorAll('.seg-btn[data-theme-set]').forEach((b) => b.addEventListener('click', () => { store.set(K_THEME, b.dataset.themeSet); applyTheme(); }));
    if (darkMq) (darkMq.addEventListener ? darkMq.addEventListener('change', applyTheme) : darkMq.addListener(applyTheme));
    // فتح التطبيق على «#view» (إعادة تحميل والعارض مفتوح): نبدأ من القائمة
    if (location.hash === '#view') history.replaceState(null, '', location.pathname + location.search);
    applyTheme();
  }

  // ---------- الربط ----------
  $('login-form').addEventListener('submit', onLogin);
  $('pass-eye').addEventListener('click', () => setPassVisible($('f-pass').type === 'password'));
  $('org-change').addEventListener('click', () => {
    $('f-org').value = org ? org.slug : '';
    org = null;
    store.del(K_ORG);
    applyOrgChip();
    $('f-org').focus();
  });
  $('offline-retry').addEventListener('click', boot);
  $('open-account').addEventListener('click', openSheet);
  $('account-close').addEventListener('click', closeSheet);
  $('account-backdrop').addEventListener('click', closeSheet);
  $('logout-btn').addEventListener('click', logout);
  bindViewing();
  bindPhase3();
  document.querySelectorAll('.seg-btn[data-lang]').forEach((b) => b.addEventListener('click', () => setLang(b.dataset.lang)));
  document.querySelectorAll('[data-lang-switch]').forEach((b) => b.addEventListener('click', () => setLang(lang === 'ar' ? 'en' : 'ar')));
  document.addEventListener('keydown', (ev) => {
    if (ev.key === 'Escape') {
      if (!$('ent-sheet').hidden) closeEntSheet();
      else if (!$('account-sheet').hidden) closeSheet();
      else if (!$('scr-viewer').hidden) $('viewer-back').click();
    }
    trapFocus(ev);
  });
  document.addEventListener('visibilitychange', recheckSession);
  window.addEventListener('online', () => { if (!$('scr-offline').hidden) boot(); });

  // ---------- عامل الخدمة: غلاف التطبيق فقط ----------
  if ('serviceWorker' in navigator && (location.protocol === 'https:' || location.hostname === 'localhost' || location.hostname === '127.0.0.1')) {
    window.addEventListener('load', () => {
      navigator.serviceWorker.register('/m/sw.js', { scope: '/m/' }).catch(() => { /* التطبيق يعمل بدونه */ });
    });
  }

  applyLang();
  boot();
})();
