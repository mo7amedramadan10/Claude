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
   الاستطلاع يبدأ تلقائيًا داخل المنصة لصفحة فيها JEEM_DATA أو عرض (deck-spec). الصفحة تعيد الرسم في مستمع «jeem:data».
   سجلات الإدخال (نماذج، طلبات، استبيانات): const r = jeem.records('الطلبات'); await r.add({الاسم:'…', العدد:3}) → السجل
   {id, data, createdAt, createdBy, mine, canChange}؛ r.list({limit, order:'desc', after, mine}) → {items, total, hasMore, next}؛
   r.update(id, {حقل: قيمة}) يدمج الحقول (null يحذف حقلًا)؛ r.remove(id)؛ r.watch(fn, ms) يستدعي fn(items) عند التغيّر.
   كل إضافة صف مستقل فلا تعارض بين المُدخلين، والسجلات تُحفظ في المنصة (المحرر والرابط المنشور يتشاركانها)؛
   من يُدخل في الرابط المنشور يحدده صاحب التصميم (أعضاء المنظمة افتراضيًا، أو أي شخص). */
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

  function platformOrigin() {
    const me = document.currentScript || document.querySelector('script[src*="jeem-page.js"]');
    try { return me && me.src ? new URL(me.src).origin : location.origin; } catch (e) { return location.origin; }
  }
  // نفس عقد صفحة المشاهدة (presentation.html) لكن بنداء مباشر من صفحة بأصل المنصة.
  async function viaApi(base, type, payload) {
    const key = payload && payload.key;
    const okKey = typeof key === 'string' && /^[\w.-]{1,100}$/.test(key);
    const fail = async r => {
      const body = await r.json().catch(() => ({}));
      const code = r.status === 409 ? 'conflict' : r.status === 401 ? 'sign_in_required' : r.status === 403 ? 'forbidden' : r.status === 404 ? 'not_found' : 'error';
      throw Object.assign(new Error(body.error || ('خطأ (' + r.status + ')')), { code, status: r.status, version: body.version });
    };
    const call = async (path, init) => { const r = await fetch(base + path, init); if (!r.ok) await fail(r); return r.json(); };
    if (type === 'viewer') return call('/access');
    if (type === 'data') return call('/live-data', { cache: 'no-store' });
    if (type === 'list') return call('/data');
    if (type.startsWith('rec-')) return recordsVia((path, init) => call(path, init), type, payload);
    if (!okKey) throw Object.assign(new Error('مفتاح غير صالح'), { code: 'bad_key', status: 400 });
    if (type === 'get') return call('/data/' + encodeURIComponent(key), { cache: 'no-store' });
    if (type === 'set') return call('/data/' + encodeURIComponent(key), { method: 'PUT', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ value: payload.value === undefined ? null : payload.value, version: typeof payload.version === 'number' ? payload.version : null }) });
    throw Object.assign(new Error('نوع غير معروف: ' + type), { code: 'bad_type', status: 400 });
  }
  // سجلات الإدخال على واجهة المنصة: نفس المسارات تحت /api/presentations/{id} و/api/freeform/{id}.
  function recordsVia(call, type, p) {
    const col = p && p.collection;
    if (typeof col !== 'string' || !col) throw Object.assign(new Error('اسم المجموعة مطلوب'), { code: 'bad_collection', status: 400 });
    const base = '/records/' + encodeURIComponent(col);
    const json = (method, body) => ({ method, headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
    if (type === 'rec-list') {
      const q = new URLSearchParams();
      if (p.limit) q.set('limit', p.limit); if (p.order) q.set('order', p.order); if (p.after) q.set('after', p.after); if (p.mine) q.set('mine', 'true');
      return call(base + (q.toString() ? '?' + q : ''), { cache: 'no-store' });
    }
    if (type === 'rec-add') return call(base, json('POST', { data: p.data }));
    if (type === 'rec-update') return call(base + '/' + encodeURIComponent(p.id), json('PUT', { data: p.data, replace: !!p.replace }));
    if (type === 'rec-remove') return call(base + '/' + encodeURIComponent(p.id), { method: 'DELETE' }).catch(e => { if (e instanceof SyntaxError) return { ok: true }; throw e; });
    throw Object.assign(new Error('نوع غير معروف: ' + type), { code: 'bad_type', status: 400 });
  }
  // سجلات خارج المنصة تمامًا (ملف محلي أثناء التأليف): مصفوفة في localStorage بالعقد نفسه.
  function localRecords(ns, type, p) {
    const k = ns + 'records:' + p.collection;
    const all = JSON.parse(localStorage.getItem(k) || '[]');
    const save = () => localStorage.setItem(k, JSON.stringify(all));
    const now = new Date().toISOString();
    if (type === 'rec-list') { let items = all.slice(); if (p.order === 'desc') items.reverse(); if (p.after) items = items.filter(r => p.order === 'desc' ? r.id < p.after : r.id > p.after); const take = Math.min(p.limit || 200, 1000); return { items: items.slice(0, take), total: all.length, hasMore: items.length > take, onlyOwn: false, local: true }; }
    if (type === 'rec-add') { const r = { id: (all.length ? all[all.length - 1].id : 0) + 1, data: p.data || {}, createdAt: now, updatedAt: now, createdBy: null, mine: true, canChange: true }; all.push(r); save(); return r; }
    const r = all.find(x => String(x.id) === String(p.id));
    if (!r) throw Object.assign(new Error('السجل غير موجود.'), { code: 'not_found', status: 404 });
    if (type === 'rec-update') { r.data = p.replace ? (p.data || {}) : Object.assign({}, r.data, p.data); for (const f in r.data) if (r.data[f] === null) delete r.data[f]; r.updatedAt = now; save(); return r; }
    if (type === 'rec-remove') { all.splice(all.indexOf(r), 1); save(); return { ok: true }; }
    throw Object.assign(new Error('نوع غير معروف: ' + type), { code: 'bad_type', status: 400 });
  }
  // بديل محلي خارج المنصة: نفس العقد على localStorage (مفيد أثناء التأليف).
  function local(type, payload) {
    // صفحة فتحتها مساحة عمل «تصميم حر» في نافذة (blob:) تحمل meta jeem-design: المخزن نفسه الذي تستخدمه المعاينة هناك
    // (jeem.ff.<id>:<key>) فتتشارك النافذة ومساحة العمل البيانات حتى النشر؛ وإلا مسار الصفحة كما كان.
    const designMeta = document.querySelector('meta[name="jeem-design"]');
    const designId = designMeta && designMeta.content;
    // تصميم منشور: النافذة تحمل أصل المنصة (blob من صفحتها) فتنادي واجهة بيانات العرض المنشور مباشرة بكوكي المستخدم،
    // فتتشارك النافذة ومساحة العمل والرابط العام البيانات نفسها.
    const presentationId = designMeta && designMeta.getAttribute('data-presentation');
    // أصل المنصة من مصدر هذا السكربت (داخل blob: لا تُحل الروابط النسبية حتى في fetch).
    // السجلات مخزَّنة بمعرّف التصميم: النافذة تنادي واجهة التصميم مباشرة (منشورًا أو لا) فترى ما يراه المحرر.
    if (designId && type.startsWith('rec-')) return viaApi(platformOrigin() + '/api/freeform/' + encodeURIComponent(designId), type, payload);
    if (presentationId) return viaApi(platformOrigin() + '/api/presentations/' + encodeURIComponent(presentationId), type, payload);
    const ns = designId ? 'jeem.ff.' + designId + ':' : 'jeem.store.' + (location.pathname || 'page') + ':';
    try {
      if (type === 'get') { const raw = localStorage.getItem(ns + payload.key); return Promise.resolve(raw ? JSON.parse(raw) : { value: null, version: 0 }); }
      if (type === 'set') { const cur = JSON.parse(localStorage.getItem(ns + payload.key) || '{"version":0}'); const next = { value: payload.value, version: (cur.version || 0) + 1, updatedAt: new Date().toISOString() }; localStorage.setItem(ns + payload.key, JSON.stringify(next)); return Promise.resolve({ version: next.version }); }
      if (type === 'list') return Promise.resolve({ keys: Object.keys(localStorage).filter(k => k.startsWith(ns)).map(k => ({ key: k.slice(ns.length) })) });
      if (type === 'viewer') return Promise.resolve({ signedIn: false, canEdit: true, name: null, editPolicy: 'anyone', local: true });
      if (type === 'data') return Promise.resolve({ live: false, local: true });
      if (type.startsWith('rec-')) return Promise.resolve(localRecords(ns, type, payload));
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
  // ---- سجلات الإدخال ----
  function records(name) {
    if (typeof name !== 'string' || !/^[\p{L}\p{N}_.-]{1,60}$/u.test(name)) throw Object.assign(new Error('اسم مجموعة غير صالح: حروف وأرقام وشرطات حتى 60 حرفًا.'), { code: 'bad_collection' });
    const req = (type, extra) => call(type, Object.assign({ collection: name }, extra));
    const api = {
      name,
      add(fields) { if (!fields || typeof fields !== 'object' || Array.isArray(fields)) return Promise.reject(Object.assign(new Error('السجل يجب أن يكون كائنًا بحقول.'), { code: 'bad_record' })); return req('rec-add', { data: fields }); },
      list(opts) { return req('rec-list', Object.assign({}, opts || {})); },
      async all(opts) { const out = []; let after = null; for (let i = 0; i < 50; i++) { const r = await req('rec-list', Object.assign({ limit: 1000 }, opts || {}, after ? { after } : {})); out.push(...(r.items || [])); if (!r.hasMore) break; after = r.next; } return out; },
      update(id, fields, opts) { return req('rec-update', { id, data: fields || {}, replace: !!(opts && opts.replace) }); },
      remove(id) { return req('rec-remove', { id }); },
      watch(fn, intervalMs) {
        let last = null;
        const tick = async () => { try { const r = await req('rec-list', { limit: 1000 }); const sig = JSON.stringify((r.items || []).map(x => [x.id, x.updatedAt])) + r.total; if (sig !== last) { const first = last === null; last = sig; if (!first && fn) fn(r.items || [], r); } } catch (e) { /* تجاهل مؤقت */ } };
        tick(); const timer = setInterval(tick, Math.max(10000, intervalMs || 20000));
        return () => clearInterval(timer);
      },
    };
    return api;
  }
  window.jeem = Object.assign(window.jeem || {}, { __page: true, inViewer, store, viewer: () => call('viewer', {}), onChange, data, records });
  // داخل المنصة: صفحة فيها بيانات (JEEM_DATA أو deck-spec) تستطلع التغيّر تلقائيًا ما لم تعطّله بـ jeem.data.autoInterval = 0.
  if (inViewer) setTimeout(() => { if (data.autoInterval > 0 && (window.JEEM_DATA || document.getElementById('deck-spec'))) data.watch(null, data.autoInterval); }, 0);
  document.dispatchEvent(new CustomEvent('jeem:ready', { detail: { inViewer } }));
})();
