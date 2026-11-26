/* ==========================================================
   عامل خدمة تطبيق «جيم» للجوال — نطاقه /m/ فقط.
   يخزّن غلاف التطبيق وحده (HTML/CSS/JS/الخطوط/الأيقونات) ليفتح بسرعة وبلا اتصال.
   لا يخزّن أبدًا أي رد من /api/ (بيانات عملاء وصلاحيات تتغيّر) ولا الصفحات المنشورة (/p/، /d/).
   الاستراتيجية: الشبكة أولًا (فيظهر كل نشر جديد فورًا)، والمخزَّن احتياط عند انقطاع الاتصال فقط.
   ========================================================== */
const VERSION = 'jeem-m-v1';
const SHELL = [
  '/m/',
  '/m/app.css',
  '/m/app.js',
  '/m/manifest.webmanifest',
  '/m/icons/icon.svg',
  '/m/icons/icon-192.png',
  '/css/tokens.css',
  '/img/logo-lockup.svg',
  '/fonts/tanseek-arabic-light.woff2',
  '/fonts/tanseek-arabic-medium.woff2',
  '/fonts/tanseek-arabic-bold.woff2',
];
const SHELL_SET = new Set(SHELL);

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(VERSION)
      .then((cache) => cache.addAll(SHELL.map((u) => new Request(u, { cache: 'no-cache', credentials: 'same-origin' }))))
      .then(() => self.skipWaiting())
  );
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys()
      .then((keys) => Promise.all(keys.filter((k) => k.startsWith('jeem-m-') && k !== VERSION).map((k) => caches.delete(k))))
      .then(() => self.clients.claim())
  );
});

self.addEventListener('fetch', (event) => {
  const req = event.request;
  if (req.method !== 'GET') return;
  const url = new URL(req.url);
  if (url.origin !== self.location.origin) return;
  // ممنوع تخزين الواجهات البرمجية أو الصفحات المنشورة: تمر إلى الشبكة كما هي.
  if (url.pathname.startsWith('/api/')) return;

  // التنقّل داخل التطبيق (/m و /m/…): كلها تفتح الغلاف نفسه.
  const isNav = req.mode === 'navigate' && (url.pathname === '/m' || url.pathname.startsWith('/m/'));
  const key = isNav ? '/m/' : url.pathname;
  if (!isNav && !SHELL_SET.has(key)) return;

  event.respondWith((async () => {
    try {
      const res = await fetch(req);
      if (res.ok && res.type === 'basic') {
        const copy = res.clone();
        caches.open(VERSION).then((cache) => cache.put(key, copy)).catch(() => {});
      }
      return res;
    } catch (err) {
      const cached = await caches.match(key);
      if (cached) return cached;
      throw err;
    }
  })());
});
