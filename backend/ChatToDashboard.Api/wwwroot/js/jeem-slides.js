// شريط مصغّرات الشرائح لصفحات «شرائح» في تصميم حر: يكتشف `section.slide` (أو .slide / [data-slide]) ويعرض أسفل الصفحة
// شريطًا ثابتًا بمصغّرات حيّة (نسخ مُصغَّرة بـ transform) للتنقّل بالنقر، مع زر إخفاء/إظهار يُحفظ محليًا، ويختفي في الطباعة.
// التنقّل: window.goToSlide(i) إن عرّفتها الصفحة، وإلا نقل صنف التفعيل (on/active/current/…) إلى الشريحة، وإلا scrollIntoView.
// تُحقن تلقائيًا في المعاينة وعند النشر إن لم تضمّنها الصفحة بنفسها.
(function () {
  'use strict';
  if (window.__jeemSlides) return; window.__jeemSlides = true;
  var ACTIVE = /^(on|active|current|is-active|show|visible|selected)$/;
  var reduced = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  function slides() { var s = document.querySelectorAll('section.slide, .slide, [data-slide]'); if (s.length < 2) s = document.querySelectorAll('main > section, body > section'); return Array.prototype.slice.call(s); }
  function activeClass(list) { for (var i = 0; i < list.length; i++) { var cl = list[i].classList; for (var j = 0; j < cl.length; j++) if (ACTIVE.test(cl[j])) return cl[j]; } return null; }
  function isVisible(el) { var r = el.getBoundingClientRect(); var cs = getComputedStyle(el); return cs.display !== 'none' && cs.visibility !== 'hidden' && cs.opacity !== '0' && r.width > 0 && r.height > 0 && r.bottom > 0 && r.top < innerHeight; }
  function currentIndex(list) { var cls = activeClass(list); if (cls) { for (var i = 0; i < list.length; i++) if (list[i].classList.contains(cls)) return i; } var best = -1, bestArea = 0; for (var k = 0; k < list.length; k++) { var r = list[k].getBoundingClientRect(); var vis = Math.max(0, Math.min(r.bottom, innerHeight) - Math.max(r.top, 0)) * Math.max(0, Math.min(r.right, innerWidth) - Math.max(r.left, 0)); if (vis > bestArea) { bestArea = vis; best = k; } } return best; }
  function goTo(list, i) {
    if (typeof window.goToSlide === 'function') { try { window.goToSlide(i); return; } catch (e) {} }
    var cls = activeClass(list);
    if (cls) { list.forEach(function (s, k) { s.classList.toggle(cls, k === i); }); try { window.dispatchEvent(new CustomEvent('jeem:slide', { detail: { index: i } })); } catch (e) {} if (!isVisible(list[i])) list[i].scrollIntoView({ behavior: reduced ? 'auto' : 'smooth', block: 'start' }); return; }
    list[i].scrollIntoView({ behavior: reduced ? 'auto' : 'smooth', block: 'start' });
  }
  var css = '#jeem-strip{position:fixed;inset-inline:0;bottom:0;z-index:2147483000;font-family:inherit;direction:rtl;pointer-events:none}' +
    '#jeem-strip .js-bar{pointer-events:auto;background:rgba(15,23,32,.86);backdrop-filter:blur(6px);color:#fff;border-top:1px solid rgba(255,255,255,.15);padding:8px 12px 10px;display:flex;align-items:center;gap:10px}' +
    '#jeem-strip .js-row{display:flex;gap:8px;overflow-x:auto;overflow-y:hidden;flex:1;scrollbar-width:thin;padding:2px}' +
    '#jeem-strip .js-th{flex:none;width:128px;height:72px;border-radius:6px;overflow:hidden;position:relative;background:#fff;border:2px solid transparent;cursor:pointer;padding:0}' +
    '#jeem-strip .js-th.on{border-color:#4FB3FF;box-shadow:0 0 0 2px rgba(79,179,255,.35)}#jeem-strip .js-th:focus-visible{outline:2px solid #fff}' +
    '#jeem-strip .js-th .js-n{position:absolute;bottom:3px;inset-inline-start:4px;background:rgba(0,0,0,.6);color:#fff;font-size:11px;padding:0 6px;border-radius:999px;line-height:16px}' +
    '#jeem-strip .js-th .js-c{position:absolute;top:0;inset-inline-start:0;transform-origin:top right;pointer-events:none}html[dir=ltr] #jeem-strip .js-th .js-c{transform-origin:top left}' +
    '#jeem-strip .js-tg{pointer-events:auto;position:absolute;bottom:100%;inset-inline-end:12px;background:rgba(15,23,32,.86);color:#fff;border:1px solid rgba(255,255,255,.2);border-bottom:0;border-radius:8px 8px 0 0;padding:4px 12px;font-size:12px;cursor:pointer}' +
    '#jeem-strip.hid .js-bar{display:none}#jeem-strip.hid .js-tg{border-radius:8px 8px 0 0;bottom:0;border-bottom:1px solid rgba(255,255,255,.2)}' +
    '#jeem-strip .js-cnt{font-size:12px;opacity:.8;white-space:nowrap}@media print{#jeem-strip{display:none!important}}@media (max-width:600px){#jeem-strip .js-th{width:96px;height:54px}}';
  function build() {
    var list = slides(); if (list.length < 2) return;
    var st = document.createElement('style'); st.id = 'jeem-strip-style'; st.textContent = css; document.head.appendChild(st);
    var root = document.createElement('div'); root.id = 'jeem-strip'; root.setAttribute('aria-label', 'مصغّرات الشرائح');
    var hidden = false; try { hidden = localStorage.getItem('jeemStripHidden') === '1'; } catch (e) {}
    var tg = document.createElement('button'); tg.type = 'button'; tg.className = 'js-tg';
    var bar = document.createElement('div'); bar.className = 'js-bar';
    var row = document.createElement('div'); row.className = 'js-row'; row.setAttribute('role', 'tablist');
    var cnt = document.createElement('span'); cnt.className = 'js-cnt';
    bar.appendChild(row); bar.appendChild(cnt); root.appendChild(tg); root.appendChild(bar); document.body.appendChild(root);
    function setHidden(h) { hidden = h; root.classList.toggle('hid', h); tg.textContent = h ? '▴ إظهار المصغّرات' : '▾ إخفاء المصغّرات'; tg.setAttribute('aria-expanded', h ? 'false' : 'true'); try { localStorage.setItem('jeemStripHidden', h ? '1' : '0'); } catch (e) {} }
    tg.addEventListener('click', function () { setHidden(!hidden); });
    // ارتفاع الشريط المرئي يُعلن للصفحة في المتغيّر --jeem-strip-h على <html> حتى تحجز له مساحة (الشريط ثابت أسفل الصفحة ويغطي ما تحته).
    function announceHeight() { var h = hidden ? tg.getBoundingClientRect().height : root.getBoundingClientRect().height; try { document.documentElement.style.setProperty('--jeem-strip-h', Math.round(h) + 'px'); } catch (e) {} }
    if ('ResizeObserver' in window) new ResizeObserver(announceHeight).observe(root); else setTimeout(announceHeight, 50);
    var _setHidden = setHidden; setHidden = function (h) { _setHidden(h); setTimeout(announceHeight, 0); };
    var thumbs = [];
    function slideSize(el) { var r = el.getBoundingClientRect(); if (r.width > 50 && r.height > 50) return { w: r.width, h: r.height }; var v = list.filter(isVisible)[0]; if (v) { var rv = v.getBoundingClientRect(); if (rv.width > 50) return { w: rv.width, h: rv.height }; } return { w: innerWidth, h: Math.max(innerHeight, innerWidth * 9 / 16) }; }
    function renderThumb(k) {
      var th = thumbs[k]; var s = list[k]; var size = slideSize(s); var scale = 128 / size.w;
      var clone = s.cloneNode(true); clone.removeAttribute('id'); clone.querySelectorAll('[id]').forEach(function (x) { x.removeAttribute('id'); }); clone.querySelectorAll('script,video,iframe').forEach(function (x) { x.parentNode.removeChild(x); });
      clone.className = s.className; clone.setAttribute('aria-hidden', 'true');
      var cs = clone.style; cs.setProperty('display', 'block', 'important'); cs.setProperty('position', 'static', 'important'); cs.setProperty('opacity', '1', 'important'); cs.setProperty('visibility', 'visible', 'important'); cs.setProperty('transform', 'none', 'important'); cs.setProperty('transition', 'none', 'important'); cs.setProperty('animation', 'none', 'important'); cs.setProperty('width', size.w + 'px', 'important'); cs.setProperty('height', size.h + 'px', 'important'); cs.setProperty('margin', '0', 'important'); cs.setProperty('inset', 'auto', 'important'); cs.setProperty('overflow', 'hidden', 'important');
      var wrap = th.querySelector('.js-c') || document.createElement('div'); wrap.className = 'js-c'; wrap.innerHTML = ''; wrap.style.width = size.w + 'px'; wrap.style.height = size.h + 'px'; wrap.style.transform = 'scale(' + scale + ')'; wrap.appendChild(clone);
      if (!wrap.parentNode) th.insertBefore(wrap, th.firstChild);
      th.style.height = Math.round(128 * size.h / size.w) + 'px';
    }
    list.forEach(function (s, k) {
      var th = document.createElement('button'); th.type = 'button'; th.className = 'js-th'; th.setAttribute('role', 'tab'); th.title = 'الشريحة ' + (k + 1) + (s.querySelector('h1,h2,h3') ? ': ' + s.querySelector('h1,h2,h3').textContent.trim().slice(0, 60) : '');
      var n = document.createElement('span'); n.className = 'js-n'; n.textContent = k + 1; th.appendChild(n);
      th.addEventListener('click', function () { goTo(list, k); setTimeout(mark, 60); });
      row.appendChild(th); thumbs.push(th); renderThumb(k);
    });
    function mark() { var i = currentIndex(list); thumbs.forEach(function (t, k) { t.classList.toggle('on', k === i); t.setAttribute('aria-selected', k === i ? 'true' : 'false'); }); cnt.textContent = (i + 1) + ' / ' + list.length; var on = thumbs[i]; if (on && on.scrollIntoView) try { on.scrollIntoView({ block: 'nearest', inline: 'center', behavior: reduced ? 'auto' : 'smooth' }); } catch (e) {} }
    var pending = null; function rerender() { clearTimeout(pending); pending = setTimeout(function () { list.forEach(function (_, k) { renderThumb(k); }); mark(); }, 250); }
    try { new MutationObserver(function (ms) { var touched = false; ms.forEach(function (m) { if (m.type === 'attributes') { if (m.attributeName === 'class') mark(); } else touched = true; }); if (touched) rerender(); }).observe(document.body, { attributes: true, attributeFilter: ['class'], subtree: true, childList: true, characterData: true }); } catch (e) {}
    window.addEventListener('scroll', mark, { passive: true }); window.addEventListener('resize', rerender);
    window.addEventListener('jeem:slide', mark);
    setHidden(hidden); mark(); setTimeout(rerender, 800);
  }
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', function () { setTimeout(build, 50); }); else setTimeout(build, 50);
})();
