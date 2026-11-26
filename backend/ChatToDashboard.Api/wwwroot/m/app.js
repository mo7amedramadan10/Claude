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
      'home.hello': 'أهلًا،', 'home.soonTitle': 'للاطلاع',
      'home.soonBody': 'هنا ستظهر اللوحات والتصاميم المنشورة في كيانك والمشتركة معك.',
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
      'home.hello': 'Hello,', 'home.soonTitle': 'For Review',
      'home.soonBody': 'Dashboards and designs published in your entity and shared with you will appear here.',
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
    syncPassEye();
    renderUser();
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
    if (ev.key !== 'Tab' || $('account-sheet').hidden) return;
    const items = [...$('account-sheet').querySelectorAll('button, a[href]')].filter((el) => !el.disabled && el.offsetParent !== null);
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
    if (!r.ok) { me = null; closeSheet(); showLogin(t('session.expired')); return; }
    me = r.data;
    renderUser();
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
  document.querySelectorAll('.seg-btn[data-lang]').forEach((b) => b.addEventListener('click', () => setLang(b.dataset.lang)));
  document.querySelectorAll('[data-lang-switch]').forEach((b) => b.addEventListener('click', () => setLang(lang === 'ar' ? 'en' : 'ar')));
  document.addEventListener('keydown', (ev) => {
    if (ev.key === 'Escape') closeSheet();
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
