/* جيم — بلوكات البيانات داخل صفحات «تصميم حر» (تُحقن في المعاينة وعند النشر متى وُجدت في الصفحة):
   - رسم: <div data-jeem-chart='{"q":"q3","type":"bar","x":"المنطقة","y":"الإيراد","title":"…"}'></div>
     الأنواع: bar | line | pie | donut | funnel | treemap | scatter؛ و"y" مصفوفة أعمدة = سلاسل متعددة (bar/line، "stacked": true).
   - قيمة: <span data-jeem-value="q2.الإجمالي" data-fmt="currency|percent|number|compact" data-agg="sum|avg|min|max|count" data-row="0">—</span>
   الأرقام من window.JEEM_DATA (نتائج الاستعلامات المسجَّلة) لا من الموديل، فتتحدّث مع «تحديث البيانات» والصفحة المنشورة الحيّة
   (حدث jeem:data). الرسم بمحرّك المنصة نفسه (jeem-viz.js) بألوان القالب: data-colors أو متغيّرات الصفحة (--primary/--accent…). */
(function () {
  'use strict';
  if (window.__jeemBlocks) return; window.__jeemBlocks = true;
  var CSS = '[data-jeem-chart]{display:flex;flex-direction:column;min-height:180px;min-width:0}'
    + '[data-jeem-chart]>.jb-title{font-weight:700;font-size:.95em;margin:0 0 6px}'
    + '[data-jeem-chart]>.jb-plot{flex:1 1 auto;min-height:160px;position:relative}'
    + '[data-jeem-chart]>.jb-empty{flex:1;display:flex;align-items:center;justify-content:center;text-align:center;padding:12px;border:1.5px dashed currentColor;border-radius:10px;opacity:.55;font-size:.85em}'
    + '.viz-holder{position:relative}.viz-holder svg{display:block;overflow:visible}'
    + '.viz-tip{position:absolute;display:none;z-index:5;pointer-events:none;max-width:240px;padding:6px 10px;border-radius:8px;background:#fff;color:#1B2430;border:1px solid rgba(0,0,0,.12);box-shadow:0 6px 18px rgba(0,0,0,.14);font-size:12px;line-height:1.5;white-space:nowrap}'
    + '.viz-legend{display:flex;flex-wrap:wrap;justify-content:center;gap:4px 12px;margin-top:6px;font-size:11px;opacity:.85}.viz-legend span{display:inline-flex;align-items:center;gap:5px}.viz-legend i{width:10px;height:10px;border-radius:3px}'
    + '@media (prefers-reduced-motion:no-preference){.viz-anim rect,.viz-anim path,.viz-anim circle{animation:jbIn .45s ease-out both}@keyframes jbIn{from{opacity:0}}}';
  function ensureCss() { if (document.getElementById('jeem-blocks-css')) return; var st = document.createElement('style'); st.id = 'jeem-blocks-css'; st.textContent = CSS; (document.head || document.documentElement).appendChild(st); }

  function data() { return window.JEEM_DATA || {}; }
  function rowsOf(q) { var d = data()[q]; return d && Array.isArray(d.rows) ? d.rows : null; }
  function toNum(v) {
    if (typeof v === 'number') return isFinite(v) ? v : 0;
    if (v == null) return 0;
    var s = String(v).replace(/[٠-٩]/g, function (c) { return c.charCodeAt(0) - 1632; }).replace(/[,٬\s%٪]/g, '');
    var n = parseFloat(s); return isFinite(n) ? n : 0;
  }
  function isNumeric(v) { return typeof v === 'number' || (typeof v === 'string' && /^\s*-?[\d٠-٩.,٬]+\s*%?\s*$/.test(v)); }
  // اسم العمود: المطلوب إن وُجد (بلا حساسية لحالة الأحرف)، وإلا أول عمود نصي للتسميات وأول عمود رقمي للقيم
  function col(rows, name, numeric) {
    var keys = rows.length ? Object.keys(rows[0]) : [];
    if (name) { for (var i = 0; i < keys.length; i++) if (keys[i].toLowerCase() === String(name).toLowerCase()) return keys[i]; }
    for (var j = 0; j < keys.length; j++) if (numeric === isNumeric(rows[0][keys[j]])) return keys[j];
    return keys[numeric ? keys.length - 1 : 0];
  }
  function fmtNum(v, f, unit) {
    var n = toNum(v), a = Math.abs(n), s;
    if (f === 'percent') return (Math.abs(n) <= 1 && n !== 0 ? n * 100 : n).toLocaleString('en-US', { maximumFractionDigits: 1 }) + '٪';
    if (f === 'compact' || f === 'currency' || a >= 100000) s = a >= 1e9 ? (n / 1e9).toLocaleString('en-US', { maximumFractionDigits: 1 }) + 'B' : a >= 1e6 ? (n / 1e6).toLocaleString('en-US', { maximumFractionDigits: 1 }) + 'M' : a >= 1e3 && f !== 'number' ? (n / 1e3).toLocaleString('en-US', { maximumFractionDigits: 1 }) + 'K' : n.toLocaleString('en-US', { maximumFractionDigits: 2 });
    else s = n.toLocaleString('en-US', { maximumFractionDigits: a < 10 ? 2 : 0 });
    return f === 'currency' ? s + ' ' + (unit || 'ر.س') : (unit ? s + ' ' + unit : s);
  }

  // ألوان الرسم من القالب: data-colors / "colors" في المواصفة، وإلا متغيّرات CSS شائعة في الصفحة، وإلا ألوان المحرّك
  var VARS = ['--chart-1', '--chart-2', '--chart-3', '--chart-4', '--primary', '--brand', '--accent', '--blue', '--navy', '--main', '--secondary', '--accent-2', '--teal', '--green', '--orange', '--purple'];
  function palette(el, cfg) {
    if (Array.isArray(cfg.colors) && cfg.colors.length) return cfg.colors;
    var cs = getComputedStyle(document.documentElement), out = [];
    VARS.forEach(function (v) { var c = cs.getPropertyValue(v).trim(); if (c && out.indexOf(c) < 0 && !/^(#fff|#ffffff|white|transparent)$/i.test(c)) out.push(c); });
    return extend(out.slice(0, 5));
  }
  // القالب بلونين أو أقل: درجات أفتح منها حتى أربعة ألوان (المحرّك يحتاج ثلاثة على الأقل وإلا عاد إلى ألوانه الافتراضية)
  function rgbOf(c) { var d = document.createElement('i'); d.style.color = c; document.body.appendChild(d); var m = getComputedStyle(d).color.match(/\d+(\.\d+)?/g); d.remove(); return m ? m.slice(0, 3).map(Number) : null; }
  function extend(list) {
    if (!list.length || list.length >= 4) return list;
    var base = list.slice(), k = 0;
    while (list.length < 4 && k < 6) {
      var rgb = rgbOf(base[k % base.length]); if (!rgb) break; var t = 0.35 + 0.25 * Math.floor(k / base.length);
      list.push('rgb(' + rgb.map(function (v) { return Math.round(v + (255 - v) * t); }).join(',') + ')'); k++;
    }
    return list;
  }
  function applyTheme(holder, el, cfg) {
    var pal = palette(el, cfg), cs = getComputedStyle(el);
    pal.forEach(function (c, i) { if (i < 4) holder.style.setProperty('--chart-' + (i + 1), c); });
    if (pal[0]) holder.style.setProperty('--brand-blue', pal[0]);
    holder.style.setProperty('--ink', cs.color); holder.style.setProperty('--ink-3', cs.color); holder.style.setProperty('--font', cs.fontFamily);
    holder.style.setProperty('--line', 'rgba(127,127,127,.22)'); holder.style.setProperty('--surface', '#ffffff');
  }

  function specOf(cfg, rows) {
    var type = String(cfg.type || 'bar').toLowerCase(), limit = Math.max(1, Math.min(60, +cfg.limit || 30));
    var r = rows.slice(0, limit), x = col(rows, cfg.x, false);
    var ys = Array.isArray(cfg.y) ? cfg.y : [cfg.y]; ys = ys.map(function (y) { return col(rows, y, true); });
    if (ys.length > 1 && (type === 'bar' || type === 'line'))
      return { kind: 'multi', type: type, stacked: !!cfg.stacked, categories: r.map(function (o) { return o[x]; }), series: ys.map(function (y) { return { name: y, data: r.map(function (o) { return toNum(o[y]); }) }; }) };
    var y = ys[0], labels = r.map(function (o) { return o[x]; }), values = r.map(function (o) { return toNum(o[y]); });
    if (type === 'pie' || type === 'donut') {
      if (labels.length > 8) { var rest = values.slice(7).reduce(function (a, b) { return a + b; }, 0); labels = labels.slice(0, 7).concat(['أخرى']); values = values.slice(0, 7).concat([rest]); }
      return { kind: 'pie', labels: labels, values: values, centerLabel: cfg.centerLabel || y };
    }
    if (type === 'funnel') return { kind: 'funnel', labels: labels, values: values };
    if (type === 'treemap') return { kind: 'treemap', rows: r.map(function (o) { return { label: o[x], value: toNum(o[y]) }; }) };
    if (type === 'scatter') { var y2 = col(rows, cfg.y2 || (Array.isArray(cfg.y) ? cfg.y[1] : null), true); return { kind: 'scatter', xTitle: y, yTitle: y2, rows: r.map(function (o) { return { x: toNum(o[y]), y: toNum(o[y2]), label: o[x] }; }) }; }
    return { kind: type === 'line' ? 'line' : 'bar', labels: labels, values: values, horizontal: cfg.horizontal };
  }

  function renderChart(el) {
    var cfg; try { cfg = JSON.parse(el.getAttribute('data-jeem-chart') || '{}'); } catch (e) { cfg = {}; }
    if (el.getAttribute('data-colors')) cfg.colors = el.getAttribute('data-colors').split(/[,\s]+/).filter(Boolean);
    if (el.__jb) { try { el.__jb.destroy(); } catch (e) {} el.__jb = null; }
    el.innerHTML = '';
    if (cfg.title) { var t = document.createElement('div'); t.className = 'jb-title'; t.textContent = cfg.title; el.appendChild(t); }
    var rows = cfg.q ? rowsOf(cfg.q) : null;
    if (!rows || !rows.length || !window.JEEM_VIZ) {
      var e = document.createElement('div'); e.className = 'jb-empty';
      e.textContent = !cfg.q ? 'رسم غير مربوط ببيانات بعد' : !rows ? 'لا توجد نتيجة للاستعلام ' + cfg.q + ' — حدّث البيانات' : 'الاستعلام بلا صفوف';
      el.appendChild(e); return;
    }
    var plot = document.createElement('div'); plot.className = 'jb-plot'; el.appendChild(plot);
    applyTheme(plot, el, cfg);
    try { el.__jb = window.JEEM_VIZ.mount(plot, specOf(cfg, rows), { fmt: function (v) { return fmtNum(v, cfg.fmt, cfg.unit); } }); }
    catch (err) { plot.textContent = 'تعذّر رسم البيانات: ' + err.message; }
  }

  function renderValue(el) {
    var ref = String(el.getAttribute('data-jeem-value') || ''), dot = ref.indexOf('.');
    var q = dot < 0 ? ref : ref.slice(0, dot), name = dot < 0 ? null : ref.slice(dot + 1), rows = rowsOf(q);
    if (!rows || !rows.length) return; // يبقى النص الحالي (آخر قيمة محفوظة أو «—»)
    var c = col(rows, name, true), agg = (el.getAttribute('data-agg') || '').toLowerCase(), v;
    if (agg === 'count') v = rows.length;
    else if (agg) { var nums = rows.map(function (o) { return toNum(o[c]); }); v = agg === 'sum' ? nums.reduce(function (a, b) { return a + b; }, 0) : agg === 'avg' ? nums.reduce(function (a, b) { return a + b; }, 0) / nums.length : agg === 'min' ? Math.min.apply(null, nums) : agg === 'max' ? Math.max.apply(null, nums) : nums[0]; }
    else { var row = rows[Math.max(0, Math.min(rows.length - 1, +(el.getAttribute('data-row') || 0)))]; v = row[c]; }
    var f = el.getAttribute('data-fmt'); el.textContent = (isNumeric(v) || typeof v === 'number') ? fmtNum(v, f || 'number', el.getAttribute('data-unit')) : String(v == null ? '—' : v);
  }

  function renderAll() {
    ensureCss();
    [].forEach.call(document.querySelectorAll('[data-jeem-chart]'), function (el) { if (!el.closest('#jeem-strip')) renderChart(el); });
    [].forEach.call(document.querySelectorAll('[data-jeem-value]'), renderValue);
  }
  window.JEEM_BLOCKS = { render: renderAll, renderChart: renderChart, renderValue: renderValue, spec: specOf, format: fmtNum };
  if (document.getElementById('app-shell')) return; // داخل المنصة نفسها: الواجهة البرمجية فقط (معاينة نافذة «رسمة من بياناتي»)
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', renderAll); else renderAll();
  document.addEventListener('jeem:data', renderAll);
  window.addEventListener('load', function () { if (document.fonts && document.fonts.ready) document.fonts.ready.then(renderAll); });
})();
