/* مؤشّر تحميل «جيم»: شعار المنصة بمساراته الأصلية وأعمدته الثلاثة تتحرك صعودًا وهبوطًا — يُستخدم في كل حالات
   الانتظار (بناء اللوحة والصفحات، تحديث الفلاتر، فتح الرابط العام، صفحة العرض المنشور) بدل الدوّار العام.
   الاستخدام: JEEM_LOADER.html({title, hint, size}) أو JEEM_LOADER.node(...). يحترم prefers-reduced-motion (بلا حركة). */
(function () {
  const CSS = `
.jl{display:inline-flex;flex-direction:column;align-items:center;gap:10px;text-align:center}
.jl svg{display:block;width:var(--jl-size,56px);height:auto;overflow:visible}
.jl .jl-bar{transform-box:fill-box;transform-origin:bottom center;animation:jl-bounce 1.1s cubic-bezier(.45,0,.35,1) infinite}
.jl .jl-bar.b2{animation-delay:.18s}.jl .jl-bar.b3{animation-delay:.36s}
.jl b{font-size:14px;color:#1f2430}.jl small{font-size:12px;color:#6b7280;line-height:1.6;max-width:320px}
@keyframes jl-bounce{0%,100%{transform:scaleY(.45)}50%{transform:scaleY(1.08)}}
@media (prefers-reduced-motion: reduce){.jl .jl-bar{animation:none}}`;
  const SVG = '<svg class="jl-char" viewBox="56 -720 774 1102" aria-hidden="true" focusable="false">'
    + '<path d="M283 342Q240 326 205.5 298.5Q171 271 146.5 235.5Q122 200 109.0 159.5Q96 119 96 76Q96 15 117.0 -35.0Q138 -85 177.0 -121.0Q216 -157 270.5 -177.0Q325 -197 393 -197H530Q556 -197 575.0 -213.0Q594 -229 594 -264V-418Q594 -449 575.0 -466.0Q556 -483 530 -483H236V-680H528Q586 -680 634.0 -660.0Q682 -640 716.5 -605.0Q751 -570 769.5 -522.0Q788 -474 788 -418V-260Q788 -203 769.5 -155.5Q751 -108 716.5 -73.5Q682 -39 632.5 -19.5Q583 0 522 0H379Q363 0 347.5 5.0Q332 10 320.0 20.0Q308 30 300.5 45.0Q293 60 293 80Q293 101 301.0 116.5Q309 132 322.0 142.0Q335 152 351.5 157.0Q368 162 385 162H403Z" fill=\"#127DBD\"/>'
    + '<rect class="jl-bar b1" x="400" y="232" width="110" height="110" rx="16" fill="#127DBD"/>'
    + '<rect class="jl-bar b2" x="540" y="137" width="110" height="205" rx="16" fill="#127DBD"/>'
    + '<rect class="jl-bar b3" x="680" y="42" width="110" height="300" rx="16" fill="#28B6E6"/>'
    + '</svg>';
  const esc = s => String(s == null ? '' : s).replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
  function ensureCss() {
    if (document.getElementById('jeem-loader-css')) return;
    const st = document.createElement('style'); st.id = 'jeem-loader-css'; st.textContent = CSS; document.head.appendChild(st);
  }
  function html(o) {
    o = o || {}; ensureCss();
    const size = o.size ? ` style="--jl-size:${parseInt(o.size, 10)}px"` : '';
    return `<div class="jl" role="status" aria-live="polite"${size}>${SVG}${o.title ? `<b>${esc(o.title)}</b>` : ''}${o.hint ? `<small>${esc(o.hint)}</small>` : ''}</div>`;
  }
  function node(o) { const d = document.createElement('div'); d.innerHTML = html(o); return d.firstElementChild; }
  window.JEEM_LOADER = { html, node, SVG };
})();
