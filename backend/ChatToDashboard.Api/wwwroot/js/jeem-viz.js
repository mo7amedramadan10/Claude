/* جيم — محرّك الرسوم الموحّد للوحات المحادثة (SVG خالص، بلا أي مكتبة خارجية)
   بديل طبقة jeem-dx.js فوق DevExtreme بالعقد نفسه تمامًا: mount(holder, spec, {theme, fmt}) يعيد مقبضًا
   {instance, holder, rendered, svg(), freeze(), destroy()}، والمواصفة (spec) هي التي يبنيها app.js
   {kind: bar|line|multi|forecast|pie|radial|funnel|treemap|scatter|sparkline, …, onPointClick?}.
   اللغة البصرية هي لغة معاينة المعرض (dash-render.js): أشرطة أفقية مدوّرة بقيمة عند طرف كل شريط، أعمدة
   رأسية للسلاسل الزمنية، خط ناعم بعلامات مفرغة، حلقة بإجمالي في المنتصف وشرح جانبي بالنسب، ألوان من رموز
   الهوية (--chart-1..4) لا من لوحة ثابتة، RTL من اتجاه الصفحة، والحركة تُلغى مع prefers-reduced-motion.
   الناتج SVG قياسي داخل الحاوية فيلتقطه تصدير PowerPoint كما كان. */
(function () {
  'use strict';

  const NS = 'http://www.w3.org/2000/svg';
  const ANIM_MS = 450;
  const reduced = () => { try { return window.matchMedia('(prefers-reduced-motion: reduce)').matches; } catch { return false; } };
  const isRtl = () => (document.documentElement.getAttribute('dir') || 'rtl') !== 'ltr';

  // تاريخ/فترة؟ (شهر، ربع، سنة، تاريخ ISO) — الاتجاه الزمني أعمدة رأسية، والفئات أشرطة أفقية.
  const TEMPORAL = /^\d{4}([-/]\d{1,2}){0,2}$|^\d{1,2}[-/]\d{4}$|^(q|ر)\s?[1-4]|(^|\s)(يناير|فبراير|مارس|أبريل|مايو|يونيو|يوليو|أغسطس|سبتمبر|أكتوبر|نوفمبر|ديسمبر|أسبوع)(\s|$)|\b(jan|feb|mar|apr|may|jun|jul|aug|sep|sept|oct|nov|dec|week|wk)\b|^\d{4}\s*[-–]\s*\d{1,2}$/i;
  function looksTemporal(labels) {
    const sample = (labels || []).slice(0, 6).map(l => String(l ?? '').trim());
    return sample.length > 0 && sample.filter(s => TEMPORAL.test(s)).length >= Math.ceil(sample.length * 0.6);
  }

  // ---------- أدوات SVG ----------
  function svgEl(tag, attrs, parent) {
    const e = document.createElementNS(NS, tag);
    for (const k in attrs) if (attrs[k] != null) e.setAttribute(k, attrs[k]);
    if (parent) parent.appendChild(e);
    return e;
  }
  function text(parent, x, y, str, o = {}) {
    const t = svgEl('text', { x, y, 'text-anchor': o.anchor || 'middle', 'dominant-baseline': o.baseline || 'middle', 'font-size': o.size || 11, 'font-weight': o.weight || 400, fill: o.fill, 'font-family': o.font }, parent);
    t.textContent = str;
    return t;
  }
  // قصّ النص الطويل بحسب عرض تقريبي (6.2px لكل حرف عند 11px).
  function clip(str, maxPx, size = 11) {
    const s = String(str ?? '');
    const perChar = size * 0.56;
    const max = Math.max(3, Math.floor(maxPx / perChar));
    return s.length > max ? s.slice(0, max - 1) + '…' : s;
  }
  function nice(max) {
    if (!isFinite(max) || max <= 0) return 1;
    const p = Math.pow(10, Math.floor(Math.log10(max)));
    const f = max / p;
    const n = f <= 1 ? 1 : f <= 2 ? 2 : f <= 2.5 ? 2.5 : f <= 5 ? 5 : 10;
    return n * p;
  }
  function ticks(min, max, count = 4) {
    const span = nice((max - min) / count);
    const out = [];
    for (let v = Math.ceil(min / span) * span; v <= max + 1e-9; v += span) out.push(+v.toFixed(10));
    return out;
  }
  // منحنى ناعم (Catmull-Rom → Bezier)
  function smoothPath(pts) {
    if (pts.length < 2) return pts.length ? `M${pts[0][0]},${pts[0][1]}` : '';
    let d = `M${pts[0][0]},${pts[0][1]}`;
    for (let i = 0; i < pts.length - 1; i++) {
      const p0 = pts[i - 1] || pts[i], p1 = pts[i], p2 = pts[i + 1], p3 = pts[i + 2] || p2;
      const c1x = p1[0] + (p2[0] - p0[0]) / 6, c1y = p1[1] + (p2[1] - p0[1]) / 6;
      const c2x = p2[0] - (p3[0] - p1[0]) / 6, c2y = p2[1] - (p3[1] - p1[1]) / 6;
      d += ` C${c1x.toFixed(1)},${c1y.toFixed(1)} ${c2x.toFixed(1)},${c2y.toFixed(1)} ${p2[0]},${p2[1]}`;
    }
    return d;
  }

  // ---------- التلميح (tooltip) ----------
  function tipFor(holder, t) {
    let tip = holder.querySelector('.viz-tip');
    if (!tip) {
      tip = document.createElement('div');
      tip.className = 'viz-tip';
      tip.setAttribute('role', 'status');
      holder.appendChild(tip);
    }
    const show = (el, html) => {
      tip.innerHTML = html; tip.style.display = 'block';
      const hb = holder.getBoundingClientRect(), eb = el.getBoundingClientRect();
      const x = eb.left + eb.width / 2 - hb.left, y = eb.top - hb.top;
      tip.style.left = Math.max(4, Math.min(hb.width - tip.offsetWidth - 4, x - tip.offsetWidth / 2)) + 'px';
      tip.style.top = Math.max(0, y - tip.offsetHeight - 8) + 'px';
    };
    const hide = () => { tip.style.display = 'none'; };
    return { show, hide, el: tip };
  }
  function bindHover(el, holder, html, t) {
    const tp = tipFor(holder, t);
    el.addEventListener('mouseenter', () => tp.show(el, html));
    el.addEventListener('mouseleave', tp.hide);
    el.addEventListener('focus', () => tp.show(el, html));
    el.addEventListener('blur', tp.hide);
  }
  function bindClick(el, fn) {
    if (!fn) return;
    el.style.cursor = 'pointer';
    el.setAttribute('tabindex', '0');
    el.addEventListener('click', fn);
    el.addEventListener('keydown', e => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); fn(e); } });
  }
  const esc = s => String(s ?? '').replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));

  // ---------- الإطار المشترك: حجم الحاوية، إعادة الرسم عند التغيّر، المقبض ----------
  function frame(holder, draw, opts) {
    holder.classList.add('viz-holder');
    let svg = null, obs = null, frozen = false;
    const render = () => {
      const w = holder.clientWidth || 300, h = holder.clientHeight || 230;
      holder.querySelectorAll('svg, .viz-legend').forEach(n => n.remove());
      // اتجاه SVG يُثبَّت LTR حتى تكون text-anchor هندسية (start = يسار، end = يمين) بغضّ النظر عن اتجاه
      // الصفحة؛ النص العربي داخل كل عنصر يبقى مشكَّلًا ومرتَّبًا صحيحًا بذاته. الاتجاه المنطقي للوحة (أول فئة
      // في جهة البداية) يُحسب في كل رسم من isRtl().
      svg = svgEl('svg', { viewBox: `0 0 ${w} ${h}`, width: w, height: h, role: 'img', 'font-family': opts.theme.fontFamily, direction: 'ltr', style: 'direction:ltr' });
      if (!reduced() && !opts.noAnimation && !frozen) svg.classList.add('viz-anim');
      holder.insertBefore(svg, holder.firstChild);
      draw(svg, w, h);
    };
    render();
    try {
      let last = holder.clientWidth;
      obs = new ResizeObserver(() => { const cw = holder.clientWidth; if (cw && Math.abs(cw - last) > 2) { last = cw; frozen = true; render(); } });
      obs.observe(holder);
    } catch { /* بلا ResizeObserver: يبقى الرسم بحجمه الأول */ }
    const ms = reduced() || opts.noAnimation ? 0 : ANIM_MS + 60;
    return {
      instance: { render }, holder,
      rendered: new Promise(r => setTimeout(r, ms)),
      svg: () => holder.querySelector('svg'),
      freeze: async () => { frozen = true; const s = holder.querySelector('svg'); if (s) s.classList.remove('viz-anim'); },
      destroy: () => { try { obs && obs.disconnect(); } catch { /* تجاهل */ } holder.querySelectorAll('svg, .viz-tip, .viz-legend').forEach(n => n.remove()); },
    };
  }

  // شرح (legend) HTML تحت الرسم — بنفس شكل .jr-legend في المعاينة
  function legend(holder, items) {
    const box = document.createElement('div');
    box.className = 'viz-legend';
    box.innerHTML = items.map(it => `<span><i style="background:${esc(it.color)}"></i>${esc(it.name)}</span>`).join('');
    holder.appendChild(box);
    return box;
  }

  // ---------- أعمدة / خط (سلسلة واحدة) ----------
  function xy(holder, spec, opts) {
    const t = opts.theme, fmt = opts.fmt, rtl = isRtl();
    const labels = (spec.labels || []).map(l => String(l ?? ''));
    const values = (spec.values || []).map(v => (typeof v === 'number' && isFinite(v)) ? v : 0);
    const isLine = spec.kind === 'line';
    const horizontal = !isLine && spec.horizontal !== false && !looksTemporal(labels) && labels.length <= 14;
    return frame(holder, (svg, W, H) => {
      const max = Math.max(0, ...values), min = Math.min(0, ...values);
      if (horizontal) {
        // أشرطة أفقية: التسمية في جهة البداية، الشريط يمتد في اتجاه القراءة، والقيمة عند طرفه.
        const n = labels.length || 1, labelW = Math.min(150, Math.max(70, W * 0.26)), valW = 54, pad = 8;
        const rowH = Math.min(36, (H - 10) / n), barH = Math.min(18, rowH * 0.56);
        const x0 = rtl ? W - labelW - pad : labelW + pad, span = W - labelW - valW - pad * 2;
        const tk = ticks(min, max, 4);
        tk.forEach(v => {
          const x = x0 + (rtl ? -1 : 1) * ((v - min) / (max - min || 1)) * span;
          svgEl('line', { x1: x, x2: x, y1: 4, y2: H - 6, stroke: t.gridColor, 'stroke-width': 1, 'stroke-dasharray': '2 3' }, svg);
        });
        labels.forEach((lab, i) => {
          const y = 5 + i * rowH + rowH / 2, len = ((values[i] - min) / (max - min || 1)) * span;
          const x = rtl ? x0 - len : x0;
          text(svg, rtl ? W - 2 : 2, y, clip(lab, labelW - 6), { anchor: rtl ? 'end' : 'start', size: 11, fill: t.tickColor });
          const g = svgEl('g', { class: 'viz-bar', style: `transform-origin:${rtl ? x0 : x0}px ${y}px` }, svg);
          const r = svgEl('rect', { x, y: y - barH / 2, width: Math.max(0, len), height: barH, rx: Math.min(6, barH / 2), fill: spec.color || t.accent }, g);
          text(svg, rtl ? x0 - len - 6 : x0 + len + 6, y, fmt(values[i]), { anchor: rtl ? 'end' : 'start', size: 11, weight: 700, fill: t.ink });
          bindHover(r, holder, `${esc(lab)}: <b>${esc(fmt(values[i]))}</b>`, t);
          bindClick(r, () => spec.onPointClick && spec.onPointClick(i, lab));
        });
      } else {
        // أعمدة رأسية (زمني) أو خط: المحور الزمني يُقرأ في اتجاه الصفحة (الأول في جهة البداية).
        const padT = 18, padB = 28, axisW = 44, padS = 10;
        const left = rtl ? padS : axisW, right = rtl ? axisW : padS;
        const plotW = W - left - right, plotH = H - padT - padB, n = labels.length || 1;
        const lo = isLine ? Math.min(...values) : min, hi = isLine ? Math.max(...values) : max;
        const vmin = isLine ? lo - (hi - lo || Math.abs(hi) || 1) * 0.15 : lo, vmax = (hi || 1) + (hi - lo || Math.abs(hi) || 1) * 0.12;
        const yOf = v => padT + plotH - ((v - vmin) / (vmax - vmin || 1)) * plotH;
        const xOf = i => { const step = plotW / n; const c = left + step * i + step / 2; return rtl ? W - c + (left - right) : c; };
        ticks(vmin, vmax, 4).forEach(v => {
          const y = yOf(v);
          svgEl('line', { x1: left, x2: W - right, y1: y, y2: y, stroke: t.gridColor, 'stroke-width': 1 }, svg);
          text(svg, rtl ? W - axisW + 6 : axisW - 6, y, fmt(v), { anchor: rtl ? 'start' : 'end', size: 10, fill: t.tickColor });
        });
        const every = Math.ceil(n / Math.max(1, Math.floor(plotW / 56)));
        labels.forEach((lab, i) => { if (i % every === 0 || i === n - 1) text(svg, xOf(i), H - padB + 14, clip(lab, plotW / n * every - 4, 10), { size: 10, fill: t.tickColor }); });
        if (isLine) {
          const pts = values.map((v, i) => [+xOf(i).toFixed(1), +yOf(v).toFixed(1)]);
          const d = smoothPath(pts);
          if (spec.areaFill !== false) svgEl('path', { class: 'viz-area', d: d + ` L${pts[pts.length - 1][0]},${yOf(vmin)} L${pts[0][0]},${yOf(vmin)} Z`, fill: spec.color || t.accent, opacity: 0.08 }, svg);
          svgEl('path', { class: 'viz-line', d, fill: 'none', stroke: spec.color || t.accent, 'stroke-width': 3, 'stroke-linejoin': 'round', 'stroke-linecap': 'round' }, svg);
          pts.forEach((p, i) => {
            const c = svgEl('circle', { class: 'viz-pt', cx: p[0], cy: p[1], r: 4.5, fill: t.surface, stroke: spec.color || t.accent, 'stroke-width': 2 }, svg);
            if (n <= 12) text(svg, p[0], p[1] - 12, fmt(values[i]), { size: 10, weight: 700, fill: t.ink });
            bindHover(c, holder, `${esc(labels[i])}: <b>${esc(fmt(values[i]))}</b>`, t);
            bindClick(c, () => spec.onPointClick && spec.onPointClick(i, labels[i]));
          });
        } else {
          const step = plotW / n, bw = Math.min(40, step * 0.55);
          values.forEach((v, i) => {
            const x = xOf(i) - bw / 2, y = yOf(Math.max(0, v)), h = Math.abs(yOf(v) - yOf(0));
            const g = svgEl('g', { class: 'viz-bar viz-bar-v', style: `transform-origin:${xOf(i)}px ${yOf(0)}px` }, svg);
            const r = svgEl('rect', { x, y, width: bw, height: Math.max(0, h), rx: Math.min(6, bw / 3), fill: spec.color || t.accent }, g);
            if (n <= 16) text(svg, xOf(i), y - 8, fmt(v), { size: 10, weight: 700, fill: t.ink });
            bindHover(r, holder, `${esc(labels[i])}: <b>${esc(fmt(v))}</b>`, t);
            bindClick(r, () => spec.onPointClick && spec.onPointClick(i, labels[i]));
          });
          svgEl('line', { x1: left, x2: W - right, y1: yOf(0), y2: yOf(0), stroke: t.gridColor, 'stroke-width': 1 }, svg);
        }
      }
    }, opts);
  }

  // ---------- متعدد السلاسل (مجمّع/مكدّس/خطوط/مساحات) ----------
  function multi(holder, spec, opts) {
    const t = opts.theme, fmt = opts.fmt, rtl = isRtl();
    const cats = (spec.categories || []).map(c => String(c ?? ''));
    const series = (spec.series || []).map((s, k) => ({ name: s.name || `سلسلة ${k + 1}`, data: (s.data || []).map(v => (typeof v === 'number' && isFinite(v)) ? v : 0), color: t.palette[k % t.palette.length] }));
    const isLine = spec.type === 'line', stacked = !!spec.stacked && !isLine;
    const handle = frame(holder, (svg, W, H) => {
      const padT = 16, padB = 26, axisW = 44, padS = 10;
      const left = rtl ? padS : axisW, right = rtl ? axisW : padS;
      const plotW = W - left - right, plotH = H - padT - padB, n = cats.length || 1;
      const totals = cats.map((_, i) => stacked ? series.reduce((a, s) => a + Math.max(0, s.data[i] || 0), 0) : Math.max(0, ...series.map(s => s.data[i] || 0)));
      const allVals = series.flatMap(s => s.data);
      const lo = isLine ? Math.min(...allVals) : Math.min(0, ...allVals), hi = Math.max(...totals, isLine ? Math.max(...allVals) : 0);
      const vmin = isLine ? lo - (hi - lo || 1) * 0.15 : lo, vmax = (hi || 1) * 1.12;
      const yOf = v => padT + plotH - ((v - vmin) / (vmax - vmin || 1)) * plotH;
      const xOf = i => { const step = plotW / n; const c = left + step * i + step / 2; return rtl ? W - c + (left - right) : c; };
      ticks(vmin, vmax, 4).forEach(v => {
        const y = yOf(v);
        svgEl('line', { x1: left, x2: W - right, y1: y, y2: y, stroke: t.gridColor, 'stroke-width': 1 }, svg);
        text(svg, rtl ? W - axisW + 6 : axisW - 6, y, fmt(v), { anchor: rtl ? 'start' : 'end', size: 10, fill: t.tickColor });
      });
      const every = Math.ceil(n / Math.max(1, Math.floor(plotW / 56)));
      cats.forEach((lab, i) => { if (i % every === 0 || i === n - 1) text(svg, xOf(i), H - padB + 14, clip(lab, plotW / n * every - 4, 10), { size: 10, fill: t.tickColor }); });
      const showLabels = cats.length * series.length <= 30;
      if (isLine) {
        series.forEach(s => {
          const pts = s.data.map((v, i) => [+xOf(i).toFixed(1), +yOf(v).toFixed(1)]);
          const d = smoothPath(pts);
          if (spec.areaFill) svgEl('path', { class: 'viz-area', d: d + ` L${pts[pts.length - 1][0]},${yOf(vmin)} L${pts[0][0]},${yOf(vmin)} Z`, fill: s.color, opacity: 0.12 }, svg);
          svgEl('path', { class: 'viz-line', d, fill: 'none', stroke: s.color, 'stroke-width': 2.5, 'stroke-linejoin': 'round' }, svg);
          pts.forEach((p, i) => {
            const c = svgEl('circle', { class: 'viz-pt', cx: p[0], cy: p[1], r: 3.5, fill: t.surface, stroke: s.color, 'stroke-width': 2 }, svg);
            bindHover(c, holder, `${esc(cats[i])} · ${esc(s.name)}: <b>${esc(fmt(s.data[i]))}</b>`, t);
            bindClick(c, () => spec.onPointClick && spec.onPointClick(i, cats[i]));
          });
        });
      } else {
        const step = plotW / n, groupW = Math.min(stacked ? 44 : 60, step * 0.7);
        cats.forEach((lab, i) => {
          if (stacked) {
            let acc = 0;
            series.forEach(s => {
              const v = Math.max(0, s.data[i] || 0); if (!v) return;
              const y1 = yOf(acc + v), y2 = yOf(acc);
              const g = svgEl('g', { class: 'viz-bar viz-bar-v', style: `transform-origin:${xOf(i)}px ${yOf(0)}px` }, svg);
              const r = svgEl('rect', { x: xOf(i) - groupW / 2, y: y1, width: groupW, height: Math.max(0, y2 - y1), fill: s.color, rx: 2 }, g);
              if (showLabels && y2 - y1 > 14) text(svg, xOf(i), (y1 + y2) / 2, fmt(v), { size: 10, weight: 700, fill: '#fff' });
              bindHover(r, holder, `${esc(lab)} · ${esc(s.name)}: <b>${esc(fmt(v))}</b>`, t);
              bindClick(r, () => spec.onPointClick && spec.onPointClick(i, lab));
              acc += v;
            });
            if (showLabels) text(svg, xOf(i), yOf(acc) - 8, fmt(acc), { size: 10, weight: 700, fill: t.ink });
          } else {
            const bw = groupW / series.length, start = xOf(i) - groupW / 2;
            series.forEach((s, k) => {
              const v = s.data[i] || 0, x = start + (rtl ? series.length - 1 - k : k) * bw;
              const g = svgEl('g', { class: 'viz-bar viz-bar-v', style: `transform-origin:${x}px ${yOf(0)}px` }, svg);
              const r = svgEl('rect', { x: x + 1, y: yOf(Math.max(0, v)), width: Math.max(1, bw - 2), height: Math.abs(yOf(v) - yOf(0)), fill: s.color, rx: 3 }, g);
              if (showLabels) text(svg, x + bw / 2, yOf(Math.max(0, v)) - 7, fmt(v), { size: 9, weight: 600, fill: t.ink });
              bindHover(r, holder, `${esc(lab)} · ${esc(s.name)}: <b>${esc(fmt(v))}</b>`, t);
              bindClick(r, () => spec.onPointClick && spec.onPointClick(i, lab));
            });
          }
        });
        svgEl('line', { x1: left, x2: W - right, y1: yOf(0), y2: yOf(0), stroke: t.gridColor, 'stroke-width': 1 }, svg);
      }
    }, opts);
    legend(holder, series.map(s => ({ name: s.name, color: s.color })));
    return handle;
  }

  // ---------- التنبؤ: التاريخي + خط متوقّع متقطع + حدّي الثقة ----------
  function forecast(holder, spec, opts) {
    const t = opts.theme, fmt = opts.fmt, rtl = isRtl();
    const cats = (spec.categories || []).map(c => String(c ?? ''));
    const num = a => (a || []).map(v => (typeof v === 'number' && isFinite(v)) ? v : null);
    const hist = num(spec.historical), fc = num(spec.forecast), up = num(spec.upper), lo = num(spec.lower);
    const fcColor = t.palette[1] || t.amber, hColor = t.palette[0] || t.accent;
    const isLine = spec.type === 'line';
    const handle = frame(holder, (svg, W, H) => {
      const padT = 16, padB = 26, axisW = 44, padS = 10;
      const left = rtl ? padS : axisW, right = rtl ? axisW : padS;
      const plotW = W - left - right, plotH = H - padT - padB, n = cats.length || 1;
      const all = [...hist, ...fc, ...up, ...lo].filter(v => v != null);
      let vmin = Math.min(0, ...all), vmax = Math.max(...all, 1);
      if (Array.isArray(spec.visualRange) && spec.visualRange.length === 2) { vmin = spec.visualRange[0] ?? vmin; vmax = spec.visualRange[1] ?? vmax; }
      vmax = vmax + (vmax - vmin || 1) * 0.1;
      const yOf = v => padT + plotH - ((v - vmin) / (vmax - vmin || 1)) * plotH;
      const xOf = i => { const step = plotW / n; const c = left + step * i + step / 2; return rtl ? W - c + (left - right) : c; };
      ticks(vmin, vmax, 4).forEach(v => {
        const y = yOf(v);
        svgEl('line', { x1: left, x2: W - right, y1: y, y2: y, stroke: t.gridColor, 'stroke-width': 1 }, svg);
        text(svg, rtl ? W - axisW + 6 : axisW - 6, y, fmt(v), { anchor: rtl ? 'start' : 'end', size: 10, fill: t.tickColor });
      });
      const every = Math.ceil(n / Math.max(1, Math.floor(plotW / 56)));
      cats.forEach((lab, i) => { if (i % every === 0 || i === n - 1) text(svg, xOf(i), H - padB + 14, clip(lab, plotW / n * every - 4, 10), { size: 10, fill: t.tickColor }); });
      // نطاق الثقة كمساحة باهتة بين الحدّين
      const band = cats.map((_, i) => (up[i] != null && lo[i] != null) ? i : null).filter(i => i != null);
      if (band.length > 1) {
        const top = band.map(i => [+xOf(i).toFixed(1), +yOf(up[i]).toFixed(1)]), bot = band.slice().reverse().map(i => [+xOf(i).toFixed(1), +yOf(lo[i]).toFixed(1)]);
        svgEl('path', { d: smoothPath(top) + ' L' + bot.map(p => p.join(',')).join(' L') + ' Z', fill: fcColor, opacity: 0.1 }, svg);
        svgEl('path', { d: smoothPath(top), fill: 'none', stroke: fcColor, 'stroke-width': 1, 'stroke-dasharray': '2 3' }, svg);
        svgEl('path', { d: smoothPath(band.map(i => [+xOf(i).toFixed(1), +yOf(lo[i]).toFixed(1)])), fill: 'none', stroke: fcColor, 'stroke-width': 1, 'stroke-dasharray': '2 3' }, svg);
      }
      // التاريخي
      const hIdx = cats.map((_, i) => hist[i] != null ? i : null).filter(i => i != null);
      if (isLine) {
        const pts = hIdx.map(i => [+xOf(i).toFixed(1), +yOf(hist[i]).toFixed(1)]);
        svgEl('path', { class: 'viz-line', d: smoothPath(pts), fill: 'none', stroke: hColor, 'stroke-width': 2.5, 'stroke-linejoin': 'round' }, svg);
        hIdx.forEach(i => { const c = svgEl('circle', { cx: xOf(i), cy: yOf(hist[i]), r: 3.5, fill: t.surface, stroke: hColor, 'stroke-width': 2 }, svg); bindHover(c, holder, `${esc(cats[i])}: <b>${esc(fmt(hist[i]))}</b>`, t); });
      } else {
        const bw = Math.min(28, (plotW / n) * 0.55);
        hIdx.forEach(i => {
          const g = svgEl('g', { class: 'viz-bar viz-bar-v', style: `transform-origin:${xOf(i)}px ${yOf(0)}px` }, svg);
          const r = svgEl('rect', { x: xOf(i) - bw / 2, y: yOf(Math.max(0, hist[i])), width: bw, height: Math.abs(yOf(hist[i]) - yOf(0)), fill: hColor, rx: 4 }, g);
          bindHover(r, holder, `${esc(cats[i])}: <b>${esc(fmt(hist[i]))}</b>`, t);
        });
      }
      // المتوقّع: خط متقطع يبدأ من آخر نقطة تاريخية
      const fIdx = cats.map((_, i) => fc[i] != null ? i : null).filter(i => i != null);
      if (fIdx.length) {
        const start = hIdx.length ? [[+xOf(hIdx[hIdx.length - 1]).toFixed(1), +yOf(hist[hIdx[hIdx.length - 1]]).toFixed(1)]] : [];
        const pts = start.concat(fIdx.map(i => [+xOf(i).toFixed(1), +yOf(fc[i]).toFixed(1)]));
        svgEl('path', { class: 'viz-line', d: smoothPath(pts), fill: 'none', stroke: fcColor, 'stroke-width': 3, 'stroke-dasharray': '7 5', 'stroke-linecap': 'round' }, svg);
        fIdx.forEach(i => { const c = svgEl('circle', { cx: xOf(i), cy: yOf(fc[i]), r: 4, fill: fcColor }, svg); bindHover(c, holder, `${esc(cats[i])} · متوقّع: <b>${esc(fmt(fc[i]))}</b>${up[i] != null ? `<br><small>${esc(fmt(lo[i]))} – ${esc(fmt(up[i]))}</small>` : ''}`, t); });
      }
    }, opts);
    legend(holder, [{ name: spec.title || 'الفعلي', color: hColor }, { name: 'متوقّع', color: fcColor }, { name: 'نطاق الثقة', color: fcColor + '55' }]);
    return handle;
  }

  // ---------- دائري (حلقة) بإجمالي في المنتصف وشرح جانبي بالنسب ----------
  function pie(holder, spec, opts) {
    const t = opts.theme, fmt = opts.fmt, rtl = isRtl();
    const labels = (spec.labels || []).map(l => String(l ?? ''));
    const values = (spec.values || []).map(v => (typeof v === 'number' && isFinite(v) && v > 0) ? v : 0);
    const total = values.reduce((a, b) => a + b, 0);
    return frame(holder, (svg, W, H) => {
      const legendW = Math.min(220, Math.max(110, W * 0.42));
      const cx = rtl ? W - (W - legendW) / 2 : (W - legendW) / 2, cy = H / 2;
      const R = Math.min((W - legendW) / 2, H / 2) - 8, inner = R * 0.68;
      const numSize = Math.round(Math.max(14, Math.min(30, R * 0.42)));
      let a0 = -Math.PI / 2;
      const dir = rtl ? -1 : 1;
      values.forEach((v, i) => {
        if (!v) return;
        const ang = (v / (total || 1)) * Math.PI * 2;
        const a1 = a0 + dir * ang;
        const large = ang > Math.PI ? 1 : 0, sweep = rtl ? 0 : 1;
        const p = (r, a) => [cx + r * Math.cos(a), cy + r * Math.sin(a)];
        const [x1, y1] = p(R, a0), [x2, y2] = p(R, a1), [x3, y3] = p(inner, a1), [x4, y4] = p(inner, a0);
        const d = `M${x1.toFixed(1)},${y1.toFixed(1)} A${R},${R} 0 ${large} ${sweep} ${x2.toFixed(1)},${y2.toFixed(1)} L${x3.toFixed(1)},${y3.toFixed(1)} A${inner},${inner} 0 ${large} ${1 - sweep} ${x4.toFixed(1)},${y4.toFixed(1)} Z`;
        const path = svgEl('path', { class: 'viz-slice', d, fill: t.palette[i % t.palette.length], stroke: t.surface, 'stroke-width': 2, style: `transform-origin:${cx}px ${cy}px` }, svg);
        bindHover(path, holder, `${esc(labels[i])}: <b>${esc(fmt(v))}</b> (${Math.round((v / (total || 1)) * 100)}٪)`, t);
        bindClick(path, () => spec.onPointClick && spec.onPointClick(i, labels[i]));
        a0 = a1;
      });
      text(svg, cx, cy - (spec.centerLabel ? numSize * 0.25 : 0), fmt(total), { size: numSize, weight: 700, fill: t.accent });
      if (spec.centerLabel) text(svg, cx, cy + numSize * 0.55, clip(spec.centerLabel, inner * 1.8, 11), { size: 11, fill: t.tickColor });
      // الشرح الجانبي: نسبة + اسم، عمود واحد في الجهة المقابلة
      const lx = rtl ? W - (W - legendW) - 6 : W - legendW + 6;
      const items = labels.map((l, i) => ({ l, i })).filter(x => values[x.i] > 0);
      const rowH = Math.min(22, (H - 8) / Math.max(1, items.length)), startY = cy - (rowH * items.length) / 2 + rowH / 2;
      items.forEach(({ l, i }, k) => {
        const y = startY + k * rowH;
        const sx = rtl ? lx - 10 : lx;
        svgEl('rect', { x: sx, y: y - 5, width: 10, height: 10, rx: 3, fill: t.palette[i % t.palette.length] }, svg);
        text(svg, rtl ? sx - 6 : sx + 16, y, `${Math.round((values[i] / (total || 1)) * 100)}٪ ${clip(l, legendW - 50, 11)}`, { anchor: rtl ? 'end' : 'start', size: 11, fill: t.ink });
      });
    }, opts);
  }

  // ---------- مقياس دائري (نسبة من حد أقصى) ----------
  function radial(holder, spec, opts) {
    const t = opts.theme;
    const pct = Math.max(0, Math.min(100, +spec.pct || 0));
    return frame(holder, (svg, W, H) => {
      const cx = W / 2, cy = H / 2, R = Math.min(W, H) / 2 - 10, L = 2 * Math.PI * R;
      svgEl('circle', { cx, cy, r: R, fill: 'none', stroke: t.gridColor, 'stroke-width': R * 0.22 }, svg);
      svgEl('circle', { class: 'viz-ring', cx, cy, r: R, fill: 'none', stroke: spec.color || t.accent, 'stroke-width': R * 0.22, 'stroke-linecap': 'round', 'stroke-dasharray': `${((pct / 100) * L).toFixed(1)} ${L.toFixed(1)}`, transform: `rotate(-90 ${cx} ${cy})` }, svg);
      text(svg, cx, cy, Math.round(pct) + '٪', { size: Math.round(Math.max(18, R * 0.42)), weight: 700, fill: t.ink });
    }, opts);
  }

  // ---------- قمع تحويل ----------
  function funnel(holder, spec, opts) {
    const t = opts.theme, fmt = opts.fmt, rtl = isRtl();
    const labels = (spec.labels || []).map(l => String(l ?? ''));
    const values = (spec.values || []).map(v => (typeof v === 'number' && isFinite(v)) ? v : 0);
    const first = Math.max(values[0] || 0, ...values) || 1;
    return frame(holder, (svg, W, H) => {
      const n = labels.length || 1, labelW = Math.min(200, W * 0.42), gap = 4;
      const fx = rtl ? 0 : labelW + 10, fw = W - labelW - 10, rowH = (H - gap * (n - 1)) / n, cx = fx + fw / 2;
      labels.forEach((lab, i) => {
        const w1 = Math.max(fw * 0.12, fw * (values[i] / first)), w2 = Math.max(fw * 0.1, fw * ((values[i + 1] ?? values[i] * 0.85) / first));
        const y = i * (rowH + gap);
        const d = `M${cx - w1 / 2},${y} L${cx + w1 / 2},${y} L${cx + w2 / 2},${y + rowH} L${cx - w2 / 2},${y + rowH} Z`;
        const p = svgEl('path', { class: 'viz-slice', d, fill: t.palette[i % t.palette.length], stroke: t.surface, 'stroke-width': 2, style: `transform-origin:${cx}px ${y + rowH / 2}px` }, svg);
        const pct = Math.round((values[i] / first) * 100);
        text(svg, rtl ? W - 2 : 2, y + rowH / 2, `${clip(lab, labelW - 60, 11)} ${fmt(values[i])} (${pct}٪)`, { anchor: rtl ? 'end' : 'start', size: 11, weight: 600, fill: t.ink });
        bindHover(p, holder, `${esc(lab)}: <b>${esc(fmt(values[i]))}</b> — ${pct}٪ من الأولى`, t);
        bindClick(p, () => spec.onPointClick && spec.onPointClick(i, lab));
      });
    }, opts);
  }

  // ---------- خريطة شجرية (حصص هرمية) — تقسيم squarify ----------
  function squarify(items, x, y, w, h, out) {
    if (!items.length) return;
    const total = items.reduce((a, it) => a + it.value, 0) || 1;
    let row = [], rest = items.slice(), rx = x, ry = y, rw = w, rh = h;
    const worst = (r, s) => { const sum = r.reduce((a, it) => a + it.value, 0); const area = (sum / total) * w * h; const side = Math.min(rw, rh); return Math.max(...r.map(it => { const a = (it.value / total) * w * h; return Math.max((side * side * a) / (area * area), (area * area) / (side * side * a)); })); };
    while (rest.length) {
      const it = rest[0];
      if (!row.length || worst(row.concat(it)) <= worst(row)) { row.push(it); rest.shift(); continue; }
      layoutRow(); row = [];
    }
    if (row.length) layoutRow();
    function layoutRow() {
      const sum = row.reduce((a, it) => a + it.value, 0), area = (sum / total) * w * h;
      if (rw >= rh) { const cw = area / rh; let cy = ry; row.forEach(it => { const ch = (it.value / sum) * rh; out.push({ ...it, x: rx, y: cy, w: cw, h: ch }); cy += ch; }); rx += cw; rw -= cw; }
      else { const ch = area / rw; let cx = rx; row.forEach(it => { const cw = (it.value / sum) * rw; out.push({ ...it, x: cx, y: ry, w: cw, h: ch }); cx += cw; }); ry += ch; rh -= ch; }
    }
  }
  function treemap(holder, spec, opts) {
    const t = opts.theme, fmt = opts.fmt;
    const rows = (spec.rows || []).map((r, i) => ({ label: String(r.label ?? ''), value: Math.max(0, +r.value || 0), group: r.group || '', i })).filter(r => r.value > 0).sort((a, b) => b.value - a.value);
    const groups = [...new Set(rows.map(r => r.group))];
    const grouped = groups.length > 1 || (groups.length === 1 && groups[0]);
    const handle = frame(holder, (svg, W, H) => {
      const out = [];
      squarify(rows, 0, 0, W, H, out);
      out.forEach(it => {
        const color = t.palette[(grouped ? groups.indexOf(it.group) : rows.indexOf(it)) % t.palette.length];
        const r = svgEl('rect', { class: 'viz-slice', x: it.x + 1, y: it.y + 1, width: Math.max(0, it.w - 2), height: Math.max(0, it.h - 2), rx: 4, fill: color, style: `transform-origin:${it.x + it.w / 2}px ${it.y + it.h / 2}px` }, svg);
        if (it.w > 40 && it.h > 26) {
          text(svg, it.x + it.w / 2, it.y + it.h / 2 - (it.h > 40 ? 7 : 0), clip(it.label, it.w - 8, 11), { size: 11, weight: 600, fill: '#fff' });
          if (it.h > 40) text(svg, it.x + it.w / 2, it.y + it.h / 2 + 8, fmt(it.value), { size: 10, fill: '#fff' });
        }
        bindHover(r, holder, `${it.group ? esc(it.group) + ' · ' : ''}${esc(it.label)}: <b>${esc(fmt(it.value))}</b>`, t);
        bindClick(r, () => spec.onPointClick && spec.onPointClick(it.i, it.label));
      });
    }, opts);
    if (grouped) legend(holder, groups.map((g, k) => ({ name: g || 'أخرى', color: t.palette[k % t.palette.length] })));
    return handle;
  }

  // ---------- مبعثر (مقياسان لكل عنصر) ----------
  function scatter(holder, spec, opts) {
    const t = opts.theme, fmt = opts.fmt, rtl = isRtl();
    const rows = (spec.rows || []).map((r, i) => ({ x: +r.x || 0, y: +r.y || 0, label: String(r.label ?? ''), group: r.group || '', i }));
    const groups = [...new Set(rows.map(r => r.group))];
    const handle = frame(holder, (svg, W, H) => {
      const padT = 12, padB = spec.xTitle ? 36 : 24, axisW = 48, padS = 12;
      const left = rtl ? padS : axisW, right = rtl ? axisW : padS;
      const plotW = W - left - right, plotH = H - padT - padB;
      const xs = rows.map(r => r.x), ys = rows.map(r => r.y);
      const xmin = Math.min(0, ...xs), xmax = Math.max(...xs, 1) * 1.08, ymin = Math.min(0, ...ys), ymax = Math.max(...ys, 1) * 1.1;
      const xOf = v => { const px = left + ((v - xmin) / (xmax - xmin || 1)) * plotW; return rtl ? W - px + (left - right) : px; };
      const yOf = v => padT + plotH - ((v - ymin) / (ymax - ymin || 1)) * plotH;
      ticks(ymin, ymax, 4).forEach(v => { const y = yOf(v); svgEl('line', { x1: left, x2: W - right, y1: y, y2: y, stroke: t.gridColor }, svg); text(svg, rtl ? W - axisW + 6 : axisW - 6, y, fmt(v), { anchor: rtl ? 'start' : 'end', size: 10, fill: t.tickColor }); });
      ticks(xmin, xmax, 5).forEach(v => { const x = xOf(v); svgEl('line', { x1: x, x2: x, y1: padT, y2: padT + plotH, stroke: t.gridColor, 'stroke-dasharray': '2 3' }, svg); text(svg, x, padT + plotH + 12, fmt(v), { size: 10, fill: t.tickColor }); });
      if (spec.xTitle) text(svg, left + plotW / 2, H - 6, spec.xTitle, { size: 10, fill: t.tickColor });
      if (spec.yTitle) text(svg, rtl ? W - 8 : 8, padT - 4, spec.yTitle, { anchor: rtl ? 'end' : 'start', size: 10, fill: t.tickColor });
      rows.forEach(r => {
        const c = svgEl('circle', { class: 'viz-pt', cx: xOf(r.x), cy: yOf(r.y), r: 6, fill: t.palette[Math.max(0, groups.indexOf(r.group)) % t.palette.length], stroke: t.surface, 'stroke-width': 1.5, opacity: 0.9 }, svg);
        bindHover(c, holder, `<b>${esc(r.label)}</b><br>${esc(spec.xTitle || 'x')} ${esc(fmt(r.x))} · ${esc(spec.yTitle || 'y')} ${esc(fmt(r.y))}`, t);
        bindClick(c, () => spec.onPointClick && spec.onPointClick(r.i, r.label));
      });
    }, opts);
    if (groups.length > 1 && groups.some(Boolean)) legend(holder, groups.map((g, k) => ({ name: g || spec.title || 'أخرى', color: t.palette[k % t.palette.length] })));
    return handle;
  }

  // ---------- خط مصغّر لبطاقة المؤشر ----------
  function sparkline(holder, spec, opts) {
    const t = opts.theme, rtl = isRtl();
    const values = (spec.values || []).map(v => +v || 0);
    const color = spec.tone === 'up' ? t.ok : spec.tone === 'down' ? t.danger : spec.tone === 'flat' ? t.tickColor : (spec.color || t.accent);
    return frame(holder, (svg, W, H) => {
      const n = values.length, lo = Math.min(...values), hi = Math.max(...values);
      const pts = values.map((v, i) => { const x = 4 + (i / Math.max(1, n - 1)) * (W - 8); return [+(rtl ? W - x : x).toFixed(1), +(H - 4 - ((v - lo) / (hi - lo || 1)) * (H - 8)).toFixed(1)]; });
      const d = smoothPath(pts);
      svgEl('path', { d: d + ` L${pts[pts.length - 1][0]},${H} L${pts[0][0]},${H} Z`, fill: color, opacity: 0.12 }, svg);
      svgEl('path', { d, fill: 'none', stroke: color, 'stroke-width': 2, 'stroke-linejoin': 'round' }, svg);
      [pts[0], pts[pts.length - 1]].forEach(p => svgEl('circle', { cx: p[0], cy: p[1], r: 3, fill: t.surface, stroke: color, 'stroke-width': 2 }, svg));
    }, { ...opts, noAnimation: true });
  }

  // ثيم الرسم من رموز الهوية الحيّة (مقيّدة بـ .jeem-scope) تُقرأ من حاوية الرسم نفسها.
  function tokenTheme(fallback, el) {
    let css; try { css = getComputedStyle(el || document.documentElement); } catch { return fallback || {}; }
    const v = (name, alt) => { const x = css.getPropertyValue(name).trim(); return x || alt; };
    const palette = [v('--chart-1', ''), v('--chart-2', ''), v('--chart-3', ''), v('--chart-4', ''), v('--brand-mint', '')].filter(Boolean);
    const fb = fallback || {};
    return {
      ...fb,
      palette: palette.length >= 3 ? palette : (fb.palette || ['#127DBD', '#0E8F86', '#7A62C7', '#D48A28']),
      accent: v('--brand-blue', fb.accent || '#127DBD'), danger: v('--danger', fb.danger || '#C94B55'), amber: v('--warn', fb.amber || '#C98A10'),
      ok: v('--ok', fb.ok || '#1A8F5E'),
      gridColor: v('--line', fb.gridColor || '#E4E8EC'), tickColor: v('--ink-3', fb.tickColor || '#6B7B8A'), ink: v('--ink', fb.ink || '#1B2430'),
      surface: v('--surface', fb.surface || '#FFFFFF'), fontFamily: v('--font', fb.fontFamily || 'inherit'),
    };
  }

  function mount(holder, spec, opts) {
    opts = { ...(opts || {}), theme: tokenTheme((opts && opts.theme) || {}, holder) };
    if (typeof opts.fmt !== 'function') opts.fmt = v => (v == null ? '' : String(v));
    switch (spec.kind) {
      case 'bar': case 'line': return xy(holder, spec, opts);
      case 'multi': return multi(holder, spec, opts);
      case 'forecast': return forecast(holder, spec, opts);
      case 'pie': return pie(holder, spec, opts);
      case 'radial': return radial(holder, spec, opts);
      case 'funnel': return funnel(holder, spec, opts);
      case 'treemap': return treemap(holder, spec, opts);
      case 'scatter': return scatter(holder, spec, opts);
      case 'sparkline': return sparkline(holder, spec, opts);
      default: throw new Error('نوع رسم غير معروف: ' + spec.kind);
    }
  }

  const api = { ready: () => true, mount, looksTemporal };
  window.JEEM_VIZ = api;
  // الاسم القديم يبقى اسمًا بديلًا حتى لا يتعطل أي مستهلك ما زال يناديه.
  window.JEEM_DX = api;
})();
