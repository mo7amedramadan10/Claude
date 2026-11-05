/* جسر «بيانات العرض» — يُضمَّن داخل صفحة HTML مرفوعة إلى جيم:
   <script src="/js/jeem-page.js"></script>
   الصفحة تعمل داخل iframe معزول (أصل معتم) فلا تستطيع نداء واجهة المنصة مباشرة؛ هذا الملف يرسل الطلبات إلى
   صفحة المشاهدة الأم بـ postMessage وهي تنفّذها بصلاحيات من يفتح الرابط. خارج المنصة (ملف محلي أو معاينة)
   يعمل على localStorage حتى تبقى الصفحة قابلة للتجربة.
   الواجهة: jeem.store.get(key) → القيمة أو null؛ jeem.store.set(key, value, {version}) → {version}؛
   jeem.store.list()؛ jeem.viewer() → {signedIn, canEdit, name, editPolicy}؛ jeem.onChange(key, fn) (استطلاع كل 20 ثانية).
   البيانات الحيّة (صفحة منشورة من «تصميم حر» أو «منشئ العروض» ببيانات): jeem.data.load() تجلب آخر نتائج الاستعلامات
   من المنصة وتحدّث window.JEEM_DATA وتبث حدث document «jeem:data» {detail:{changed, refreshedAt, data, spec}}؛
   jeem.data.get(key) → {sql, rows, rowCount}؛ jeem.data.watch(fn, ms) استطلاع دوري (افتراضيًا كل 60 ثانية) يستدعي fn عند التغيّر.
   الاستطلاع يبدأ تلقائيًا داخل المنصة لصفحة فيها JEEM_DATA أو عرض (deck-spec). الصفحة تعيد الرسم في مستمع «jeem:data». */
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
      if (type === 'data') return Promise.resolve({ live: false, local: true });
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
  // ---- البيانات الحيّة ----
  const data = {
    meta: { refreshedAt: (window.JEEM_DATA_META && window.JEEM_DATA_META.refreshedAt) || null, live: null },
    get(key) { return window.JEEM_DATA ? window.JEEM_DATA[key] || null : null; },
    async load() {
      const r = await call('data', {});
      if (!r || r.live === false) { data.meta.live = false; return { changed: false, live: false }; }
      data.meta.live = true;
      const changed = r.refreshedAt !== data.meta.refreshedAt;
      data.meta.refreshedAt = r.refreshedAt || data.meta.refreshedAt;
      if (r.queries) {
        const d = window.JEEM_DATA || (window.JEEM_DATA = {});
        for (const k in r.queries) { const q = r.queries[k] || {}; d[k] = { sql: d[k] && d[k].sql, rows: q.rows || [], rowCount: q.rowCount || 0 }; }
      }
      window.JEEM_DATA_META = Object.assign(window.JEEM_DATA_META || {}, { refreshedAt: data.meta.refreshedAt, failed: r.failed || 0 });
      const detail = { changed, live: true, refreshedAt: data.meta.refreshedAt, data: window.JEEM_DATA || null, spec: r.spec || null, failed: r.failed || 0 };
      if (changed) document.dispatchEvent(new CustomEvent('jeem:data', { detail }));
      return detail;
    },
    watch(fn, intervalMs) {
      const timer = setInterval(async () => { try { const d = await data.load(); if (d.live === false) { clearInterval(timer); return; } if (d.changed && fn) fn(d); } catch (e) { /* تجاهل مؤقت */ } }, Math.max(15000, intervalMs || 60000));
      data.__watchers = (data.__watchers || 0) + 1;
      return () => clearInterval(timer);
    },
    autoInterval: 60000,
  };
  window.jeem = Object.assign(window.jeem || {}, { __page: true, inViewer, store, viewer: () => call('viewer', {}), onChange, data });
  // داخل المنصة: صفحة فيها بيانات (JEEM_DATA أو deck-spec) تستطلع التغيّر تلقائيًا ما لم تعطّله بـ jeem.data.autoInterval = 0.
  if (inViewer) setTimeout(() => { if (data.autoInterval > 0 && (window.JEEM_DATA || document.getElementById('deck-spec'))) data.watch(null, data.autoInterval); }, 0);
  document.dispatchEvent(new CustomEvent('jeem:ready', { detail: { inViewer } }));
})();
