/*
 * jeem-motion.js — محرّك المشاهد المتحركة في صفحات «تصميم حر» من نوع «حركة».
 *
 * الصفحة تصف المشاهد فقط، والمحرّك يتولى التشغيل كله (التوقيت، إظهار مشهد واحد في كل لحظة، أزرار التحكم، شريط الزمن،
 * التكرار، تقليل الحركة)، فلا يكتب الموديل محرّكًا جديدًا في كل مرة. العقد:
 *
 *   <div data-jeem-motion data-loop="true">                       ← المسرح (16:9 افتراضيًا)
 *     <section class="jm-scene" data-slide data-duration="5" data-title="المشكلة">
 *       <h2 data-in="up">عنوان</h2>                                ← يدخل بحركة؛ data-delay بالثواني (وإلا بالتتابع)
 *       <b data-in="count">1,250 ر.س</b>                           ← رقم يُعدّ من الصفر إلى قيمته المكتوبة
 *       <svg><path data-in="draw" d="…"/></svg>                    ← خط يُرسم
 *     </section> …
 *   </div>
 *
 * الحركات: fade | up | down | start | end | zoom | pop | draw | count. تقليل الحركة (prefers-reduced-motion): لا انتقالات
 * ولا تشغيل تلقائي؛ كل مشهد يظهر مكتملًا وينتقل المستخدم بينها بشريط الزمن أو الأسهم. في وضع التعديل في المنصة
 * (صنف ff-edit-on على body) يتوقف التشغيل ويظهر المشهد الحالي مكتملًا ليُعدَّل.
 *
 * الوضع المُضمَّن: حركة كاملة داخل شريحة من عرض آخر —
 *   <div data-jeem-motion-embed='{"id":12,"title":"…"}' data-autoplay="true" data-loop="true" data-controls="true">
 *     <div data-jeem-motion data-w="1280" data-h="720"> …المشاهد… </div>
 *   </div>
 * المسرح يُصمَّم بمقاسه الأصلي ويُصغَّر ليلائم البلوك، ولكل بلوك شريط تحكم صغير يظهر عند التحويم؛ يبدأ من أول مشهد كلما
 * ظهرت شريحته ويتوقف حين تختفي، ولا يلمس window.goToSlide ولا لوحة المفاتيح (هي لتنقّل الشرائح). JEEM_MOTION_MOUNT(stage)
 * يشغّل مسرحًا أُضيف بعد التحميل (المحرر).
 */
(function () {
  'use strict';
  if (window.JEEM_MOTION_MOUNT) { scan(); return; }

  var EN = (document.documentElement.lang || '').toLowerCase().indexOf('en') === 0;
  var RTL = (document.documentElement.dir || getComputedStyle(document.documentElement).direction) === 'rtl';
  var reduce = window.matchMedia ? matchMedia('(prefers-reduced-motion: reduce)') : { matches: false };
  var T = EN
    ? { play: 'Play', pause: 'Pause', replay: 'Replay', loop: 'Loop', scene: 'Scene', of: 'of', seek: 'Timeline', reduced: 'Reduced motion: scenes are shown complete — use the timeline or arrow keys', ctl: 'Animation controls' }
    : { play: 'تشغيل', pause: 'إيقاف', replay: 'إعادة', loop: 'تكرار', scene: 'المشهد', of: 'من', seek: 'شريط الزمن', reduced: 'تقليل الحركة مفعّل: المشاهد تُعرض مكتملة — تنقّل بشريط الزمن أو الأسهم', ctl: 'التحكم في الحركة' };

  // ---------- الأنماط: تُحقن أول <head> فتغلب أنماط الصفحة عند التساوي فقط فيما ليس جوهريًا؛ إظهار المشهد نفسه مفروض ----------
  var css = document.createElement('style'); css.id = 'jeem-motion-css';
  css.textContent = [
    ':where([data-jeem-motion]){position:relative;width:100%;aspect-ratio:16/9;overflow:hidden}',
    '[data-jeem-motion]{position:relative}',
    '[data-jeem-motion]>.jm-scene{position:absolute!important;inset:0!important;opacity:0!important;visibility:hidden!important;pointer-events:none;transition:opacity .5s ease,visibility 0s linear .5s}',
    '[data-jeem-motion]>.jm-scene.on{opacity:1!important;visibility:visible!important;pointer-events:auto;transition:opacity .5s ease}',
    '.jm-scene [data-in]{transition:opacity var(--jm-d,.7s) ease,transform var(--jm-d,.7s) cubic-bezier(.2,.7,.2,1),stroke-dashoffset var(--jm-d,1.2s) ease}',
    '.jm-scene [data-in]:not(.jm-in){opacity:0}',
    '.jm-scene [data-in=up]:not(.jm-in){transform:translateY(28px)}',
    '.jm-scene [data-in=down]:not(.jm-in){transform:translateY(-28px)}',
    '.jm-scene [data-in=start]:not(.jm-in){transform:translateX(' + (RTL ? '40px' : '-40px') + ')}',
    '.jm-scene [data-in=end]:not(.jm-in){transform:translateX(' + (RTL ? '-40px' : '40px') + ')}',
    '.jm-scene [data-in=zoom]:not(.jm-in){transform:scale(.88)}',
    '.jm-scene [data-in=pop]:not(.jm-in){transform:scale(.6)}',
    '.jm-scene [data-in=draw]:not(.jm-in){opacity:1}',
    '.jm-scene [data-in=draw],.jm-scene [data-in=draw] :is(path,line,polyline,polygon,circle,rect,ellipse){stroke-dasharray:1;stroke-dashoffset:1;transition:stroke-dashoffset var(--jm-d,1.2s) ease}',
    '.jm-scene [data-in=draw].jm-in,.jm-scene [data-in=draw].jm-in :is(path,line,polyline,polygon,circle,rect,ellipse){stroke-dashoffset:0}',
    '.jm-scene [data-in].jm-in{opacity:1;transform:none}',
    '.jm-bar{display:flex;align-items:center;gap:10px;flex-wrap:wrap;margin:10px auto 0;padding:8px 12px;border-radius:12px;background:rgba(255,255,255,.92);border:1px solid rgba(0,0,0,.08);box-shadow:0 2px 10px rgba(0,0,0,.06);font:500 13px/1.2 system-ui,-apple-system,"Segoe UI",Tahoma,sans-serif;color:#1f2a37;box-sizing:border-box}',
    '.jm-bar button{font:inherit;font-weight:600;border:0;border-radius:8px;padding:7px 14px;cursor:pointer;background:#e8eef5;color:#1f2a37}',
    '.jm-bar button.jm-main{background:#2A78D6;color:#fff;min-width:84px}',
    '.jm-bar button:focus-visible,.jm-bar input:focus-visible{outline:2px solid #2A78D6;outline-offset:2px}',
    '.jm-bar input[type=range]{flex:1 1 160px;min-width:120px;accent-color:#2A78D6}',
    '.jm-bar label{display:inline-flex;align-items:center;gap:5px;cursor:pointer;white-space:nowrap}',
    '.jm-bar .jm-label{white-space:nowrap;color:#4b5563;max-width:40%;overflow:hidden;text-overflow:ellipsis}',
    '.jm-bar .jm-note{flex-basis:100%;font-size:12px;color:#6b7280}',
    // الوضع المُضمَّن: بلوك يقص المسرح المصغَّر، وشريط صغير فوق أسفله يظهر عند التحويم أو التركيز
    '[data-jeem-motion-embed]{position:relative;overflow:hidden;box-sizing:border-box}',
    '[data-jeem-motion-embed]>[data-jeem-motion]{position:absolute!important;left:0;top:0;margin:0!important;max-width:none!important;max-height:none!important;transform-origin:0 0}',
    '.jm-bar.jm-mini{position:absolute;inset-inline:8px;bottom:8px;margin:0;padding:4px 6px;gap:6px;flex-wrap:nowrap;z-index:3;font-size:12px;opacity:0;transition:opacity .2s ease}',
    '[data-jeem-motion-embed]:hover>.jm-mini,.jm-mini:focus-within{opacity:1}',
    '@media (hover:none){.jm-bar.jm-mini{opacity:1}}',
    '.jm-bar.jm-mini button{padding:4px 10px;min-width:0}',
    '.jm-bar.jm-mini .jm-label{max-width:none;flex:1 1 auto;min-width:0}',
    '[data-jeem-motion-embed][data-controls=false]>.jm-mini{display:none}',
    'body.ff-edit-on .jm-mini{display:none!important}',
    '@media (prefers-reduced-motion: reduce){[data-jeem-motion]>.jm-scene,.jm-scene [data-in],.jm-scene [data-in] *{transition:none!important;animation:none!important}}',
    'body.ff-edit-on [data-jeem-motion]>.jm-scene,body.ff-edit-on .jm-scene [data-in],body.ff-edit-on .jm-scene [data-in] *{transition:none!important}',
    '@media print{.jm-bar{display:none!important}:not([data-jeem-motion-embed])>[data-jeem-motion]>.jm-scene{position:relative!important;opacity:1!important;visibility:visible!important;break-after:page}:not([data-jeem-motion-embed])>[data-jeem-motion]{aspect-ratio:auto;overflow:visible}.jm-scene [data-in]{opacity:1!important;transform:none!important;stroke-dashoffset:0!important}}'
  ].join('\n');
  document.head.insertBefore(css, document.head.firstChild);

  // ---------- أدوات مشتركة ----------
  function num(v, d) { var n = parseFloat(v); return isFinite(n) ? n : d; }
  var AR = '٠١٢٣٤٥٦٧٨٩';
  function parseCount(text) {
    // يقرأ الرقم المكتوب (عربي أو لاتيني، بفواصل) ويبقي ما قبله وما بعده كما هو
    var t = String(text || '').replace(/[٠-٩]/g, function (d) { return AR.indexOf(d); }).replace(/٫/g, '.').replace(/٬/g, ',');
    var m = /(-?\d[\d,]*(?:\.\d+)?)/.exec(t); if (!m) return null;
    var raw = m[1], dec = (raw.split('.')[1] || '').length, val = parseFloat(raw.replace(/,/g, ''));
    return isFinite(val) ? { pre: t.slice(0, m.index), post: t.slice(m.index + raw.length), val: val, dec: dec, sep: raw.indexOf(',') >= 0 } : null;
  }
  function fmtCount(c, v) {
    var s = v.toFixed(c.dec); if (c.sep) { var p = s.split('.'); p[0] = p[0].replace(/\B(?=(\d{3})+(?!\d))/g, ','); s = p.join('.'); }
    return c.pre + s + c.post;
  }
  function editing() { return document.body.classList.contains('ff-edit-on'); }
  var all = [];
  new MutationObserver(function () { all.forEach(function (x) { x.check(); }); }).observe(document.body, { attributes: true, attributeFilter: ['class'] });
  if (reduce.addEventListener) reduce.addEventListener('change', function () { all.forEach(function (x) { x.check(true); }); });
  document.addEventListener('visibilitychange', function () { if (document.hidden) all.forEach(function (x) { x.pause(); }); });

  // ---------- مسرح واحد ----------
  function mount(root, embedded) {
    if (!root || root.__jm) return root && root.__jm;
    var scenes = [].slice.call(root.children).filter(function (x) { return x.classList && x.classList.contains('jm-scene'); });
    if (!scenes.length) return null;
    var host = embedded ? root.parentElement : null; // بلوك التضمين

    var model = scenes.map(function (s) {
      if (!embedded && !s.hasAttribute('data-slide')) s.setAttribute('data-slide', '');
      var items = [].slice.call(s.querySelectorAll('[data-in]')), auto = 0.25;
      var els = items.map(function (e, k) {
        var kind = (e.getAttribute('data-in') || 'fade').toLowerCase();
        var delay = e.hasAttribute('data-delay') ? Math.max(0, num(e.getAttribute('data-delay'), 0)) : (auto + k * 0.35);
        var dur = Math.max(0.1, num(e.getAttribute('data-dur'), kind === 'draw' ? 1.2 : kind === 'count' ? 1.4 : 0.7));
        e.style.setProperty('--jm-d', dur + 's');
        if (kind === 'draw') [e].concat([].slice.call(e.querySelectorAll('path,line,polyline,polygon,circle,rect,ellipse'))).forEach(function (p) { if (p.setAttribute && /^(path|line|polyline|polygon|circle|rect|ellipse)$/i.test(p.tagName)) p.setAttribute('pathLength', '1'); });
        return { el: e, kind: kind, delay: delay, dur: dur, count: kind === 'count' ? parseCount(e.textContent) : null, shown: '' };
      });
      var done = els.reduce(function (m, x) { return Math.max(m, x.delay + x.dur); }, 0);
      var dur = Math.min(60, Math.max(1.5, num(s.getAttribute('data-duration'), Math.max(4, done + 1.5))));
      return { el: s, els: els, dur: dur, done: Math.min(dur, done), title: s.getAttribute('data-title') || '' };
    });
    var starts = [], total = 0;
    model.forEach(function (m) { starts.push(total); total += m.dur; });

    // ---------- شريط التحكم ----------
    var bar = document.createElement('div'); bar.className = 'jm-bar' + (embedded ? ' jm-mini' : ''); bar.setAttribute('role', 'group'); bar.setAttribute('aria-label', T.ctl);
    if (embedded) {
      bar.innerHTML = '<button type="button" class="jm-main" data-jm="play"></button><button type="button" data-jm="replay">' + T.replay + '</button><span class="jm-label" data-jm="label" aria-live="polite"></span>';
      host.appendChild(bar);
    } else {
      bar.id = 'jm-controls';
      bar.innerHTML = '<button type="button" class="jm-main" data-jm="play" id="jm-play"></button><button type="button" data-jm="replay" id="jm-replay">' + T.replay + '</button>'
        + '<input type="range" data-jm="seek" id="jm-seek" min="0" max="' + Math.round(total * 10) + '" step="1" value="0" aria-label="' + T.seek + '">'
        + '<label><input type="checkbox" data-jm="loop" id="jm-loop"> ' + T.loop + '</label><span class="jm-label" data-jm="label" id="jm-label" aria-live="polite"></span>'
        + '<span class="jm-note" data-jm="note" id="jm-note" hidden>' + T.reduced + '</span>';
      root.insertAdjacentElement('afterend', bar);
    }
    var q = function (k) { return bar.querySelector('[data-jm="' + k + '"]'); };
    var btnPlay = q('play'), seek = q('seek'), loopBox = q('loop'), label = q('label'), note = q('note');
    if (loopBox) loopBox.checked = root.getAttribute('data-loop') !== 'false';
    function looping() { return loopBox ? loopBox.checked : (host.getAttribute('data-loop') !== 'false'); }
    function autoplay() { return !embedded || host.getAttribute('data-autoplay') !== 'false'; }

    // المقاس: الصفحة تلائم الشريط بعرض المسرح؛ المُضمَّن يُصغَّر المسرح (بمقاسه الأصلي) ليلائم البلوك ويتوسّطه
    var W = num(root.getAttribute('data-w'), 1280), H = num(root.getAttribute('data-h'), 720);
    function fit() {
      if (!embedded) { var w = root.getBoundingClientRect().width; if (w) bar.style.maxWidth = Math.round(w) + 'px'; return; }
      var cw = host.clientWidth, ch = host.clientHeight; if (!cw || !ch) return;
      var k = Math.min(cw / W, ch / H);
      root.style.width = W + 'px'; root.style.height = H + 'px';
      root.style.left = Math.round((cw - W * k) / 2) + 'px'; root.style.top = Math.round((ch - H * k) / 2) + 'px';
      root.style.transform = 'scale(' + k + ')';
    }

    // ---------- الزمن ----------
    var t = 0, playing = false, last = 0, raf = 0, cur = -1, shownNow = !embedded, parked = false;
    function still() { return reduce.matches || editing(); }
    function sceneAt(time) { for (var i = model.length - 1; i >= 0; i--) if (time >= starts[i] - 1e-6) return i; return 0; }
    function setText(x, v) { var s = fmtCount(x.count, v); if (s !== x.shown) { x.shown = s; x.el.textContent = s; } }
    function apply() {
      var i = Math.min(sceneAt(t), model.length - 1), local = t - starts[i], full = still() || parked;
      if (i !== cur) {
        scenes.forEach(function (s, k) { s.classList.toggle('on', k === i); s.setAttribute('aria-hidden', k === i ? 'false' : 'true'); });
        if (cur >= 0 && model[cur]) model[cur].els.forEach(function (x) { x.el.classList.remove('jm-in'); if (x.count) setText(x, x.count.val); });
        cur = i;
        if (!embedded) try { document.dispatchEvent(new CustomEvent('jeem:slide', { detail: { index: i } })); } catch (e) {}
      }
      model[i].els.forEach(function (x) {
        var on = full || local >= x.delay; x.el.classList.toggle('jm-in', on);
        if (x.count) { var p = full ? 1 : Math.min(1, Math.max(0, (local - x.delay) / x.dur)); setText(x, x.count.val * (1 - Math.pow(1 - p, 3))); }
      });
      if (seek) seek.value = Math.round(t * 10);
      label.textContent = T.scene + ' ' + (i + 1) + ' ' + T.of + ' ' + model.length + (model[i].title ? ' — ' + model[i].title : '');
    }
    function frame(now) {
      if (!playing) return;
      var dt = Math.min(0.1, (now - last) / 1000); last = now; t += dt;
      if (t >= total) { if (looping()) t = 0; else { t = total - 0.001; setPlaying(false); } }
      apply(); raf = requestAnimationFrame(frame);
    }
    function setPlaying(on) {
      playing = !!on && !editing() && (!embedded || shownNow);
      btnPlay.textContent = playing ? T.pause : T.play; btnPlay.setAttribute('aria-pressed', playing ? 'true' : 'false');
      cancelAnimationFrame(raf); if (playing) { last = performance.now(); raf = requestAnimationFrame(frame); }
    }
    // الانتقال إلى مشهد: أثناء التشغيل يبدأ من أوله، وفي الإيقاف يظهر مكتملًا
    function goTo(i) {
      i = Math.max(0, Math.min(model.length - 1, i | 0));
      t = playing ? starts[i] : starts[i] + Math.min(model[i].dur - 0.05, model[i].done + 0.01); apply();
    }
    // بلا تشغيل تلقائي (أو تقليل الحركة/التعديل) يقف على أول مشهد مكتملًا؛ «تشغيل» يبدأ من أوله
    function restart() { t = 0; cur = -1; if (still() || !autoplay()) { setPlaying(false); parked = true; goTo(0); } else { parked = false; apply(); setPlaying(true); } }

    btnPlay.addEventListener('click', function () { if (!playing && (parked || t >= total - 0.01)) { t = 0; cur = -1; parked = false; } setPlaying(!playing); });
    q('replay').addEventListener('click', function () { t = 0; cur = -1; parked = false; apply(); setPlaying(!still()); });
    if (seek) seek.addEventListener('input', function () { t = Math.min(total - 0.001, seek.value / 10); apply(); });

    // المُضمَّن: يعمل فقط وشريحته ظاهرة (المعروضة فعلًا، لا المكدّسة خلفها بشفافية)، ويبدأ من أوله كلما ظهرت
    var slide = embedded ? root.closest('section.slide,.slide,[data-slide]') : null, inView = false;
    function slideShown() {
      if (!slide) return true;
      for (var e = slide; e && e !== document.documentElement; e = e.parentElement) {
        var cs = getComputedStyle(e); if (cs.display === 'none' || cs.visibility === 'hidden' || parseFloat(cs.opacity) < 0.05) return false;
      }
      return true;
    }
    function checkShown() {
      if (!embedded) return;
      var now = inView && slideShown();
      if (now === shownNow) return;
      shownNow = now;
      if (now) restart(); else setPlaying(false);
    }
    var api = {
      embedded: embedded, total: total, scenes: model.length,
      get scene() { return cur; }, get time() { return t; },
      play: function () { setPlaying(true); }, pause: function () { setPlaying(false); },
      seek: function (s) { t = Math.max(0, Math.min(total - 0.001, +s || 0)); apply(); },
      goTo: goTo, fit: fit,
      update: function () { fit(); restart(); },
      check: function (force) {
        var ed = editing() ? '1' : '';
        if (force || bar.dataset.edit !== ed) {
          bar.dataset.edit = ed;
          if (note) note.hidden = !reduce.matches;
          if (still()) { setPlaying(false); cur = -1; apply(); } else if (embedded && shownNow) restart();
        }
        if (embedded) checkShown();
      }
    };
    root.__jm = api; all.push(api);

    if (embedded) {
      if (window.IntersectionObserver) new IntersectionObserver(function (es) { es.forEach(function (en) { inView = en.isIntersecting && en.intersectionRatio > 0.25; }); checkShown(); }, { threshold: [0, 0.25, 0.5, 1] }).observe(host);
      else inView = true;
      document.addEventListener('jeem:slide', function () { setTimeout(checkShown, 30); setTimeout(checkShown, 400); });
      if (slide) new MutationObserver(function () { checkShown(); }).observe(slide, { attributes: true, attributeFilter: ['class', 'style', 'hidden', 'aria-hidden'] });
      if (window.ResizeObserver) new ResizeObserver(fit).observe(host);
      fit(); setPlaying(false); parked = true; goTo(0); checkShown();
    } else {
      document.addEventListener('keydown', function (e) {
        if (editing() || /^(INPUT|TEXTAREA|SELECT)$/.test((e.target && e.target.tagName) || '') || (e.target && e.target.isContentEditable)) return;
        var fw = RTL ? 'ArrowLeft' : 'ArrowRight', bk = RTL ? 'ArrowRight' : 'ArrowLeft';
        if (e.key === ' ' && !(e.target && e.target.closest && e.target.closest('button'))) { e.preventDefault(); btnPlay.click(); }
        else if (e.key === fw || e.key === 'PageDown') { e.preventDefault(); goTo(cur + 1); }
        else if (e.key === bk || e.key === 'PageUp') { e.preventDefault(); goTo(cur - 1); }
      });
      if (window.ResizeObserver) new ResizeObserver(fit).observe(root); fit();
      bar.dataset.edit = editing() ? '1' : '';
      note.hidden = !reduce.matches; apply(); setPlaying(!still());
    }
    return api;
  }

  function isEmbedded(r) { return !!(r.parentElement && r.parentElement.hasAttribute('data-jeem-motion-embed')); }
  window.JEEM_MOTION_MOUNT = function (r) { return r ? mount(r, isEmbedded(r)) : null; };
  scan();

  function scan() {
    [].slice.call(document.querySelectorAll('[data-jeem-motion-embed]>[data-jeem-motion]')).forEach(function (r) { window.JEEM_MOTION_MOUNT(r); });
    if (window.JEEM_MOTION) return;
    // صفحة «حركة»: المسرح الذي ليس داخل بلوك تضمين
    var root = [].slice.call(document.querySelectorAll('[data-jeem-motion]')).filter(function (r) { return !isEmbedded(r); })[0];
    if (!root) { var first = [].slice.call(document.querySelectorAll('.jm-scene')).filter(function (s) { return !s.closest('[data-jeem-motion-embed]'); })[0]; root = first && first.parentElement; }
    if (!root) return;
    var api = window.JEEM_MOTION_MOUNT(root); if (!api) return;
    window.goToSlide = function (i) { api.pause(); api.goTo(i); };
    window.JEEM_MOTION = api;
  }
})();
