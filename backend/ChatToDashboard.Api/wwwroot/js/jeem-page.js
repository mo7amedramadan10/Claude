/* جسر «بيانات العرض» — يُضمَّن داخل صفحة HTML مرفوعة إلى جيم:
   <script src="/js/jeem-page.js"></script>
   الصفحة تعمل داخل iframe معزول (أصل معتم) فلا تستطيع نداء واجهة المنصة مباشرة؛ هذا الملف يرسل الطلبات إلى
   صفحة المشاهدة الأم بـ postMessage وهي تنفّذها بصلاحيات من يفتح الرابط. خارج المنصة (ملف محلي أو معاينة)
   يعمل على localStorage حتى تبقى الصفحة قابلة للتجربة.
   الواجهة: jeem.store.get(key) → القيمة أو null؛ jeem.store.set(key, value, {version}) → {version}؛
   jeem.store.list()؛ jeem.viewer() → {signedIn, canEdit, name, editPolicy}؛ jeem.onChange(key, fn) (استطلاع كل 20 ثانية). */
(function () {
  'use strict';
  if (window.jeem && window.jeem.__page) return;
  const inViewer = (() => { try { return window.parent !== window && !!window.parent.postMessage; } catch (e) { return false; } })();
  const pending = new Map();
  let seq = 0;
  const versions = new Map();

  function call(type, payload) {
    if (!inViewer) return local(type, payload);
    return new Promise((resolve, reject) => {
      const id = 'jeem-' + (++seq) + '-' + Math.random().toString(36).slice(2);
      const timer = setTimeout(() => { pending.delete(id); reject(new Error('لا استجابة من صفحة المشاهدة (هل الصفحة مفتوحة داخل جيم؟)')); }, 15000);
      pending.set(id, { resolve, reject, timer });
      window.parent.postMessage({ jeem: 1, id, type, payload }, '*');
    });
  }
  window.addEventListener('message', ev => {
    const m = ev.data;
    if (!m || m.jeem !== 1 || !m.id || !pending.has(m.id)) return;
    const { resolve, reject, timer } = pending.get(m.id);
    clearTimeout(timer); pending.delete(m.id);
    if (m.error) { const e = new Error(m.error.message || 'خطأ'); e.code = m.error.code; e.status = m.error.status; reject(e); }
    else resolve(m.result);
  });

  // بديل محلي خارج المنصة: نفس العقد على localStorage (مفيد أثناء التأليف).
  function local(type, payload) {
    const ns = 'jeem.store.' + (location.pathname || 'page') + ':';
    try {
      if (type === 'get') { const raw = localStorage.getItem(ns + payload.key); return Promise.resolve(raw ? JSON.parse(raw) : { value: null, version: 0 }); }
      if (type === 'set') { const cur = JSON.parse(localStorage.getItem(ns + payload.key) || '{"version":0}'); const next = { value: payload.value, version: (cur.version || 0) + 1, updatedAt: new Date().toISOString() }; localStorage.setItem(ns + payload.key, JSON.stringify(next)); return Promise.resolve({ version: next.version }); }
      if (type === 'list') return Promise.resolve({ keys: Object.keys(localStorage).filter(k => k.startsWith(ns)).map(k => ({ key: k.slice(ns.length) })) });
      if (type === 'viewer') return Promise.resolve({ signedIn: false, canEdit: true, name: null, editPolicy: 'anyone', local: true });
    } catch (e) { return Promise.reject(e); }
    return Promise.reject(new Error('نوع غير معروف: ' + type));
  }

  const store = {
    async get(key) { const r = await call('get', { key }); versions.set(key, r.version || 0); return r.value; },
    async getWithMeta(key) { const r = await call('get', { key }); versions.set(key, r.version || 0); return r; },
    async set(key, value, opts) {
      const version = opts && typeof opts.version === 'number' ? opts.version : (opts && opts.force ? null : (versions.has(key) ? versions.get(key) : null));
      const r = await call('set', { key, value, version });
      versions.set(key, r.version); return r;
    },
    list() { return call('list', {}); },
  };
  const watchers = [];
  function onChange(key, fn, intervalMs) {
    const w = { key, fn, last: versions.get(key) || 0, timer: null };
    w.timer = setInterval(async () => {
      try { const r = await call('get', { key }); if ((r.version || 0) !== w.last) { w.last = r.version || 0; versions.set(key, w.last); fn(r.value, r); } } catch (e) { /* تجاهل مؤقت */ }
    }, Math.max(5000, intervalMs || 20000));
    watchers.push(w);
    return () => clearInterval(w.timer);
  }
  window.jeem = Object.assign(window.jeem || {}, { __page: true, inViewer, store, viewer: () => call('viewer', {}), onChange });
  document.dispatchEvent(new CustomEvent('jeem:ready', { detail: { inViewer } }));
})();
