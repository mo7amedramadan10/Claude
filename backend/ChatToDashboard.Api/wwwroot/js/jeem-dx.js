/* جيم — طبقة الرسم الموحّدة فوق DevExtreme (viz فقط، انظر lib/devextreme/README.md)
   كل رسم في لوحات المحادثة (أعمدة، خط، دائري، متعدد السلاسل، التنبؤ، المقياس الدائري) يمرّ من
   هنا: ثيم واحد (ألوان جيم، الخط، الشبكة)، اتجاه RTL، تعطيل الحركة مع prefers-reduced-motion،
   وتنسيق أرقام واحد (fmt من app.js). اللوحات التفاعلية (dash-kit) ومعاينة المعرض لا تستخدمها.
   المواصفة (spec) التي تُبنى في app.js محايدة عن المكتبة: {kind, labels, values, ...} — mount()
   هو المكان الوحيد الذي يعرف خيارات DevExtreme. */
(function () {
  'use strict';

  const DX = window.DevExpress;
  function ready() { return !!(DX && DX.viz && DX.viz.dxChart && DX.viz.dxPieChart); }

  // الترخيص: من /api/ui-config.js (user-secrets على الخادم). بلا مفتاح = نسخة تجريبية بتنبيه.
  try {
    const key = (window.JEEM_UI_CONFIG && window.JEEM_UI_CONFIG.devExtremeLicenseKey) || '';
    if (DX && DX.config && key) DX.config({ licenseKey: key });
  } catch { /* لا يُسقط الصفحة لو تعذّر الضبط */ }

  const reduced = () => { try { return window.matchMedia('(prefers-reduced-motion: reduce)').matches; } catch { return false; } };
  const isRtl = () => (document.documentElement.getAttribute('dir') || 'rtl') !== 'ltr';
  const ANIM_MS = 450;

  // تاريخ/فترة؟ (شهر، ربع، سنة، تاريخ ISO) — الاتجاه الزمني يبقى أعمدة رأسية، والفئات أفقية.
  const TEMPORAL = /^\d{4}([-/]\d{1,2}){0,2}$|^\d{1,2}[-/]\d{4}$|^(q|ر)\s?[1-4]|يناير|فبراير|مارس|أبريل|مايو|يونيو|يوليو|أغسطس|سبتمبر|أكتوبر|نوفمبر|ديسمبر|jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec|^\d{4}\s*[-–]\s*\d{1,2}$|أسبوع|week/i;
  function looksTemporal(labels) {
    const sample = labels.slice(0, 6).map(l => String(l ?? '').trim());
    return sample.length > 0 && sample.filter(s => TEMPORAL.test(s)).length >= Math.ceil(sample.length * 0.6);
  }

  function font(t, size, weight) {
    return { family: t.fontFamily, size: size || 11, weight: weight || 400, color: t.tickColor };
  }

  function common(t, opts) {
    return {
      rtlEnabled: isRtl(),
      redrawOnResize: true,
      animation: { enabled: !reduced() && !opts.noAnimation, duration: ANIM_MS, easing: 'easeOutCubic' },
      palette: t.palette,
      export: { enabled: false },
      loadingIndicator: { enabled: false },
      tooltip: {
        enabled: true, shared: false, zIndex: 2000,
        font: font(t, 12), color: t.surface, border: { color: t.gridColor, width: 1 }, cornerRadius: 8, paddingLeftRight: 10,
        customizeTooltip: info => ({ text: `${info.argumentText ?? ''}${info.seriesName && info.seriesName !== info.argumentText ? ` · ${info.seriesName}` : ''}: ${opts.fmt(info.value)}` }),
      },
    };
  }

  function axes(t, opts, rotated) {
    const grid = { visible: true, color: t.gridColor };
    return {
      commonAxisSettings: {
        color: t.gridColor, tick: { visible: false }, minorTick: { visible: false },
        label: { font: font(t, 11) },
      },
      argumentAxis: {
        grid: { visible: false }, color: t.gridColor,
        label: { overlappingBehavior: rotated ? 'none' : 'rotate', rotationAngle: isRtl() ? 45 : -45, wordWrap: 'none', textOverflow: 'ellipsis' },
      },
      valueAxis: {
        grid, color: 'transparent', visualRange: opts.visualRange,
        label: { customizeText: e => opts.fmt(e.value) },
        tick: { visible: false },
      },
    };
  }

  function size(holder) {
    return { width: holder.clientWidth || undefined, height: holder.clientHeight || undefined };
  }

  function wrap(instance, holder, destroyExtra) {
    const ms = reduced() ? 0 : ANIM_MS + 60;
    const handle = {
      instance, holder,
      rendered: new Promise(r => setTimeout(r, ms)),
      svg: () => holder.querySelector('svg'),
      // للتصدير: يثبّت الرسم في حالته النهائية فورًا (بلا حركة).
      freeze: async () => { try { instance.option('animation.enabled', false); instance.render(); } catch { /* تجاهل */ } },
      destroy: () => { try { instance.dispose(); } catch { /* تم التخلص منه */ } if (destroyExtra) destroyExtra(); },
    };
    return handle;
  }

  // ---------- أعمدة / خط (سلسلة واحدة) ----------
  function xy(holder, spec, opts) {
    const t = opts.theme;
    const isLine = spec.kind === 'line';
    const rotated = !isLine && spec.horizontal !== false && !looksTemporal(spec.labels) && spec.labels.length <= 14;
    const data = spec.labels.map((l, i) => ({ arg: String(l ?? ''), val: spec.values[i] }));
    const options = {
      ...common(t, opts), ...axes(t, opts, rotated),
      ...size(holder),
      rotated,
      dataSource: data,
      commonSeriesSettings: { argumentField: 'arg', valueField: 'val' },
      legend: { visible: false },
      series: [isLine ? {
        type: 'spline', color: t.accent, width: 3,
        point: { visible: true, size: 7, color: t.surface, border: { visible: true, width: 2, color: t.accent }, hoverStyle: { size: 9 } },
        label: { visible: spec.labels.length <= 12, font: font(t, 11, 700), backgroundColor: 'none', customizeText: e => opts.fmt(e.value) },
      } : {
        type: 'bar', color: t.accent, cornerRadius: 6, barPadding: 0.45,
        label: { visible: true, font: { ...font(t, 11, 700), color: t.ink }, backgroundColor: 'none', position: 'outside', customizeText: e => opts.fmt(e.value) },
        hoverStyle: { color: t.palette[3] || t.accent },
      }],
      onPointClick: spec.onPointClick ? e => spec.onPointClick(e.target.index, e.target.argument) : undefined,
    };
    if (rotated) options.argumentAxis.inverted = true; // أول فئة في الأعلى عند الأفقي
    return wrap(new DX.viz.dxChart(holder, options), holder);
  }

  // ---------- متعدد السلاسل (مجمّع/مكدّس/مساحات) ----------
  function multi(holder, spec, opts) {
    const t = opts.theme;
    const isLine = spec.type === 'line';
    const data = spec.categories.map((c, i) => {
      const row = { arg: String(c ?? '') };
      spec.series.forEach((s, k) => { row['s' + k] = s.data[i]; });
      return row;
    });
    const type = spec.areaFill ? 'splinearea' : isLine ? 'spline' : spec.stacked ? 'stackedbar' : 'bar';
    const options = {
      ...common(t, opts), ...axes(t, opts, false), ...size(holder),
      dataSource: data,
      commonSeriesSettings: {
        argumentField: 'arg', type,
        ...(isLine ? { width: 2, point: { visible: !spec.areaFill, size: 6 } } : { cornerRadius: 4, barPadding: spec.stacked ? 0.45 : 0.3 }),
        ...(spec.areaFill ? { opacity: 0.35 } : {}),
        // المكدّس: التسمية داخل الشريحة بالأبيض؛ المجمّع والخط: خارج النقطة بلون النص.
        label: {
          visible: !spec.areaFill && spec.categories.length * spec.series.length <= 30,
          font: spec.stacked ? { ...font(t, 10, 700), color: '#fff' } : { ...font(t, 10, 600), color: t.ink },
          position: spec.stacked ? 'inside' : 'outside', backgroundColor: 'none', customizeText: e => opts.fmt(e.value),
        },
      },
      series: spec.series.map((s, k) => ({ valueField: 's' + k, name: s.name, color: t.palette[k % t.palette.length] })),
      legend: { visible: true, verticalAlignment: 'bottom', horizontalAlignment: 'center', font: font(t, 11), itemTextPosition: isRtl() ? 'left' : 'right', margin: { top: 8 } },
      onPointClick: spec.onPointClick ? e => spec.onPointClick(e.target.index, e.target.argument) : undefined,
    };
    return wrap(new DX.viz.dxChart(holder, options), holder);
  }

  // ---------- التنبؤ: التاريخي + خط متوقّع متقطع + حدّي الثقة ----------
  function forecast(holder, spec, opts) {
    const t = opts.theme;
    const isLine = spec.type === 'line';
    const fc = t.palette[1];
    const data = spec.categories.map((c, i) => ({ arg: String(c ?? ''), h: spec.historical[i], f: spec.forecast[i], u: spec.upper[i], l: spec.lower[i] }));
    const options = {
      ...common(t, opts), ...axes(t, { ...opts, visualRange: spec.visualRange }, false), ...size(holder),
      dataSource: data,
      commonSeriesSettings: { argumentField: 'arg', ignoreEmptyPoints: true },
      series: [
        isLine ? { valueField: 'h', name: spec.title, type: 'spline', color: t.palette[0], width: 2, point: { visible: false } }
               : { valueField: 'h', name: spec.title, type: 'bar', color: t.palette[0], cornerRadius: 4, barPadding: 0.4 },
        { valueField: 'u', name: 'الحد الأعلى للتوقع', type: 'spline', color: fc, width: 1, dashStyle: 'dot', point: { visible: false } },
        { valueField: 'l', name: 'الحد الأدنى للتوقع', type: 'spline', color: fc, width: 1, dashStyle: 'dot', point: { visible: false } },
        { valueField: 'f', name: 'متوقّع', type: 'spline', color: fc, width: 3, dashStyle: 'dash', point: { visible: true, size: 5, color: fc } },
      ],
      legend: { visible: true, verticalAlignment: 'bottom', horizontalAlignment: 'center', font: font(t, 11), itemTextPosition: isRtl() ? 'left' : 'right' },
    };
    return wrap(new DX.viz.dxChart(holder, options), holder);
  }

  // ---------- دائري (حلقة) بتسمية في المنتصف ----------
  function pie(holder, spec, opts) {
    const t = opts.theme;
    const total = spec.values.reduce((a, b) => a + b, 0) || 1;
    const data = spec.labels.map((l, i) => ({ arg: String(l ?? ''), val: spec.values[i] }));
    const box = Math.min(holder.clientWidth || 300, holder.clientHeight || 230);
    const numSize = Math.round(Math.max(16, Math.min(34, box * 0.15)));
    const capSize = Math.round(Math.max(10, numSize * 0.42));
    const options = {
      ...common(t, opts), ...size(holder),
      type: 'doughnut', innerRadius: 0.7, startAngle: 90, segmentsDirection: isRtl() ? 'clockwise' : 'anticlockwise',
      dataSource: data,
      series: [{
        argumentField: 'arg', valueField: 'val',
        label: { visible: false },
        smallValuesGrouping: { mode: 'none' },
        border: { visible: true, width: 2, color: t.surface },
        hoverStyle: { hatching: { direction: 'none' } },
      }],
      legend: {
        visible: true, horizontalAlignment: isRtl() ? 'left' : 'right', verticalAlignment: 'top', itemTextPosition: isRtl() ? 'left' : 'right',
        font: font(t, 11), columnCount: 1, markerSize: 10,
        customizeText: e => `${Math.round((spec.values[e.pointIndex] / total) * 100)}٪ ${e.pointName}`,
      },
      tooltip: { ...common(t, opts).tooltip, customizeTooltip: info => ({ text: `${info.argumentText}: ${opts.fmt(info.value)} (${Math.round(info.percent * 100)}٪)` }) },
      centerTemplate: (comp, el) => {
        const g = el; while (g.firstChild) g.removeChild(g.firstChild);
        const ns = 'http://www.w3.org/2000/svg';
        const num = document.createElementNS(ns, 'text');
        num.setAttribute('text-anchor', 'middle'); num.setAttribute('y', -Math.round(capSize * 0.3));
        num.setAttribute('font-family', t.fontFamily); num.setAttribute('font-size', numSize); num.setAttribute('font-weight', '700'); num.setAttribute('fill', t.accent);
        num.textContent = opts.fmt(total);
        const cap = document.createElementNS(ns, 'text');
        cap.setAttribute('text-anchor', 'middle'); cap.setAttribute('y', Math.round(capSize * 1.2));
        cap.setAttribute('font-family', t.fontFamily); cap.setAttribute('font-size', capSize); cap.setAttribute('fill', t.tickColor);
        cap.textContent = spec.centerLabel || '';
        g.appendChild(num); g.appendChild(cap);
        return g;
      },
      onPointClick: spec.onPointClick ? e => spec.onPointClick(e.target.index, e.target.argument) : undefined,
    };
    return wrap(new DX.viz.dxPieChart(holder, options), holder);
  }

  // ---------- مقياس دائري (نسبة من حد أقصى) ----------
  function radial(holder, spec, opts) {
    const t = opts.theme;
    const pct = Math.max(0, Math.min(100, spec.pct));
    const box = Math.min(holder.clientWidth || 200, holder.clientHeight || 200);
    const options = {
      ...common(t, opts), ...size(holder),
      type: 'doughnut', innerRadius: 0.78, startAngle: 90, segmentsDirection: 'clockwise',
      dataSource: [{ arg: 'v', val: pct }, { arg: 'r', val: 100 - pct }],
      series: [{ argumentField: 'arg', valueField: 'val', label: { visible: false }, border: { visible: false }, hoverStyle: { hatching: { direction: 'none' } } }],
      palette: [spec.color, t.gridColor],
      legend: { visible: false }, tooltip: { enabled: false },
      centerTemplate: (comp, el) => {
        while (el.firstChild) el.removeChild(el.firstChild);
        const num = document.createElementNS('http://www.w3.org/2000/svg', 'text');
        num.setAttribute('text-anchor', 'middle'); num.setAttribute('y', Math.round(box * 0.05));
        num.setAttribute('font-family', t.fontFamily); num.setAttribute('font-size', Math.round(Math.max(18, box * 0.14))); num.setAttribute('font-weight', '700'); num.setAttribute('fill', t.ink);
        num.textContent = Math.round(pct) + '٪';
        el.appendChild(num);
        return el;
      },
    };
    return wrap(new DX.viz.dxPieChart(holder, options), holder);
  }

  // ---------- قمع تحويل ----------
  function funnel(holder, spec, opts) {
    const t = opts.theme;
    const first = spec.values[0] || 1;
    const data = spec.labels.map((l, i) => ({ arg: String(l ?? ''), val: spec.values[i] }));
    const options = {
      ...common(t, opts), ...size(holder),
      rtlEnabled: isRtl(),
      dataSource: data, argumentField: 'arg', valueField: 'val',
      algorithm: 'dynamicSlope', inverted: false, neckWidth: 0.28, neckHeight: 0,
      item: { border: { visible: true, width: 2, color: t.surface }, hoverStyle: { hatching: { direction: 'none' } } },
      label: {
        visible: true, position: 'outside', horizontalAlignment: isRtl() ? 'left' : 'right',
        font: font(t, 11, 600), backgroundColor: 'none', connector: { visible: true, color: t.gridColor, width: 1 },
        customizeText: e => `${e.item.argument} ${opts.fmt(e.value)} (${Math.round((e.value / first) * 100)}٪)`,
      },
      legend: { visible: false },
      tooltip: { ...common(t, opts).tooltip, customizeTooltip: info => ({ text: `${info.item.argument}: ${opts.fmt(info.value)} — ${Math.round((info.value / first) * 100)}٪ من الأولى` }) },
      onItemClick: spec.onPointClick ? e => spec.onPointClick(data.findIndex(d => d.arg === e.item.argument), e.item.argument) : undefined,
    };
    return wrap(new DX.viz.dxFunnel(holder, options), holder);
  }

  // ---------- خريطة شجرية (حصص هرمية) ----------
  function treemap(holder, spec, opts) {
    const t = opts.theme;
    const grouped = spec.rows.some(r => r.group);
    const items = grouped
      ? Object.values(spec.rows.reduce((acc, r) => { (acc[r.group || 'أخرى'] ||= { name: r.group || 'أخرى', items: [] }).items.push({ name: r.label, value: r.value }); return acc; }, {}))
      : spec.rows.map(r => ({ name: r.label, value: r.value }));
    const options = {
      ...common(t, opts), ...size(holder),
      rtlEnabled: isRtl(),
      dataSource: items, valueField: 'value', labelField: 'name', childrenField: 'items',
      colorizer: { type: 'discrete', palette: t.palette, colorizeGroups: grouped },
      tile: { border: { width: 2, color: t.surface }, label: { visible: true, font: { ...font(t, 11, 600), color: '#fff' }, wordWrap: 'normal', textOverflow: 'ellipsis' } },
      group: { border: { width: 2, color: t.surface }, color: t.gridColor, label: { visible: true, font: font(t, 11, 700) }, headerHeight: 22, padding: 3 },
      tooltip: { ...common(t, opts).tooltip, customizeTooltip: info => ({ text: `${info.node.label()}: ${opts.fmt(info.value)}` }) },
      onClick: spec.onPointClick ? e => { if (e.node && e.node.isLeaf()) spec.onPointClick(spec.rows.findIndex(r => r.label === e.node.label()), e.node.label()); } : undefined,
    };
    return wrap(new DX.viz.dxTreeMap(holder, options), holder);
  }

  // ---------- مبعثر (مقياسان لكل عنصر) ----------
  function scatter(holder, spec, opts) {
    const t = opts.theme;
    const groups = [...new Set(spec.rows.map(r => r.group || ''))];
    const data = spec.rows.map(r => ({ x: r.x, y: r.y, name: r.label, g: r.group || '' }));
    const options = {
      ...common(t, opts), ...axes(t, opts, false), ...size(holder),
      dataSource: data,
      commonSeriesSettings: { type: 'scatter', argumentField: 'x', valueField: 'y', point: { size: 10, border: { visible: true, width: 1, color: t.surface } } },
      seriesTemplate: { nameField: 'g', customizeSeries: g => ({ name: g || spec.title, color: t.palette[Math.max(0, groups.indexOf(g)) % t.palette.length] }) },
      argumentAxis: { grid: { visible: true, color: t.gridColor }, label: { font: font(t, 11), customizeText: e => opts.fmt(e.value) }, title: { text: spec.xTitle || '', font: font(t, 11) } },
      valueAxis: { grid: { visible: true, color: t.gridColor }, label: { customizeText: e => opts.fmt(e.value) }, title: { text: spec.yTitle || '', font: font(t, 11) } },
      legend: { visible: groups.length > 1 && groups.some(Boolean), verticalAlignment: 'bottom', horizontalAlignment: 'center', font: font(t, 11), itemTextPosition: isRtl() ? 'left' : 'right' },
      tooltip: { ...common(t, opts).tooltip, customizeTooltip: info => ({ text: `${info.point.data.name}: ${spec.xTitle || 'x'} ${opts.fmt(info.argument)} · ${spec.yTitle || 'y'} ${opts.fmt(info.value)}` }) },
      onPointClick: spec.onPointClick ? e => spec.onPointClick(data.indexOf(e.target.data), e.target.data.name) : undefined,
    };
    return wrap(new DX.viz.dxChart(holder, options), holder);
  }

  // ---------- خط مصغّر لبطاقة المؤشر ----------
  function sparkline(holder, spec, opts) {
    const t = opts.theme;
    const options = {
      rtlEnabled: isRtl(), ...size(holder),
      dataSource: spec.values.map((v, i) => ({ i, v })), argumentField: 'i', valueField: 'v',
      type: 'area', lineColor: spec.color || t.accent, lineWidth: 2, showMinMax: false, showFirstLast: true,
      firstLastColor: spec.color || t.accent, pointSize: 4, pointSymbol: 'circle',
      tooltip: { enabled: false }, margin: { top: 3, bottom: 3, left: 3, right: 3 },
    };
    return wrap(new DX.viz.dxSparkline(holder, options), holder);
  }

  // ثيم الرسم من رموز التصميم الحية (tokens.css) لا من كائن THEME في app.js: الصفحة لها مظهر واحد
  // تحدده رموز CSS، وكائن THEME يحمل لوحة «داكنة» قديمة لا يقابلها أي CSS، فكانت الرسوم تنقلب داكنة
  // على بطاقات فاتحة عند الضغط على زر المظهر. القيم الممرَّرة تبقى احتياطًا عند غياب أي رمز.
  // تُقرأ من حاوية الرسم نفسها لا من <html>: رموز الهوية مقيّدة بـ .jeem-scope (انظر tokens.css).
  function tokenTheme(fallback, el) {
    let css; try { css = getComputedStyle(el || document.documentElement); } catch { return fallback; }
    const v = (name, alt) => { const x = css.getPropertyValue(name).trim(); return x || alt; };
    const palette = [v('--chart-1', ''), v('--chart-2', ''), v('--chart-3', ''), v('--chart-4', ''), v('--brand-mint', '')].filter(Boolean);
    return {
      ...fallback,
      palette: palette.length >= 3 ? palette : fallback.palette,
      accent: v('--brand-blue', fallback.accent), danger: v('--danger', fallback.danger), amber: v('--warn', fallback.amber),
      gridColor: v('--line', fallback.gridColor), tickColor: v('--ink-3', fallback.tickColor), ink: v('--ink', fallback.ink),
      surface: v('--surface', fallback.surface), fontFamily: v('--font', fallback.fontFamily),
    };
  }

  function mount(holder, spec, opts) {
    if (!ready()) throw new Error('DevExtreme viz غير محمّل');
    opts = { ...opts, theme: tokenTheme(opts.theme || {}, holder) };
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

  window.JEEM_DX = { ready, mount, looksTemporal };
})();
