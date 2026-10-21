/* ==========================================================
   جيم — dash-kit: أدوات مشتركة للوحات التفاعلية (الدفعة 11 لوحة)
   window.DK = { ic, esc, rng, nf, num, money, pct, toast, count, seg,
                 bars, hbars, line, donut, waterfall, scatter, funnel, heat, spark, legend, PAL, ST }
   القواعد: الزمن من اليمين لليسار (الأقدم يمين)، محور واحد، الوحدة خارج .num،
   الحالة = لون + شكل + نص، والحركة تُلغى مع prefers-reduced-motion.
   ========================================================== */
(function () {
  const RM = matchMedia('(prefers-reduced-motion: reduce)').matches;
  /* لوحة فئوية مُتحقق منها (ترتيب ثابت): أزرق، برتقالي، فيروزي، بنفسجي، أخضر، وردي */
  const PAL = ['#127DBD', '#D9822B', '#00A89F', '#6E62B0', '#7CB342', '#C2569B'];
  const ST = { ok: ['#1E9E6A', 'check', 'سليم'], warn: ['#C98A10', 'info', 'تنبيه'], bad: ['#D64545', 'x', 'حرج'] };
  const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
  const esc = s => String(s).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
  const rng = k => { let h = 2166136261; for (const c of String(k)) h = Math.imul(h ^ c.charCodeAt(0), 16777619); return () => ((h = Math.imul(h ^ (h >>> 15), 2246822507) >>> 0) / 4294967296); };
  const nf = (v, d = 0) => Number(v).toLocaleString('en-US', { minimumFractionDigits: d, maximumFractionDigits: d });
  /* رقم مختصر: يعيد [نص الرقم، الوحدة] */
  const abbr = v => { const r = abbr0(v); r[0] = r[0].replace(/\.0+$/, ''); return r; };
  const abbr0 = v => { const a = Math.abs(v); return a >= 1e9 ? [nf(v / 1e9, 2), 'مليار'] : a >= 1e6 ? [nf(v / 1e6, a >= 1e8 ? 0 : 1), 'مليون'] : a >= 1e4 ? [nf(v / 1e3, a >= 1e5 ? 0 : 1), 'ألف'] : [nf(v, Math.abs(v) < 10 && v % 1 ? 1 : 0), '']; };
  const num = (v, u = '', d) => { const [n, au] = d == null ? abbr(v) : [nf(v, d), '']; const unit = [au, u].filter(Boolean).join(' '); return `<span class="num">${n}</span>${unit ? `<small class="u">${unit}</small>` : ''}`; };
  const money = (v, cur = 'ر.س') => num(v, cur);
  const pct = (v, d = 1) => `<span class="num">${nf(v, d)}%</span>`;
  const txt = (v, u = '') => { const [n, au] = abbr(v); return `${n}${au ? ' ' + au : ''}${u ? ' ' + u : ''}`; };
  const delta = (v, good = 1, d = 1) => { const up = v >= 0, ok = up === (good > 0); return `<span class="dk-dl ${ok ? 'ok' : 'bad'}">${ic(up ? 'up' : 'down')}<span class="num">${up ? '+' : '−'}${nf(Math.abs(v), d)}%</span></span>`; };

  function toast(msg) { let t = document.querySelector('.toast'); if (!t) { t = document.createElement('div'); t.className = 'toast'; t.setAttribute('role', 'status'); document.body.append(t); } t.innerHTML = `${ic('check')}<span>${esc(msg)}</span>`; requestAnimationFrame(() => t.classList.add('is-on')); clearTimeout(t._h); t._h = setTimeout(() => t.classList.remove('is-on'), 2600); }

  /* عدّاد: يعدّ كل .num داخل العنصر من 0 إلى قيمته */
  function count(root) {
    if (RM) return;
    root.querySelectorAll('.dk-count .num').forEach(el => {
      const raw = el.textContent, m = raw.match(/-?[\d,]+(\.\d+)?/); if (!m) return;
      const to = parseFloat(m[0].replace(/,/g, '')), d = (m[1] || '').length - 1 > 0 ? m[1].length - 1 : 0, t0 = performance.now();
      const step = t => { const k = Math.min(1, (t - t0) / 700), e = 1 - Math.pow(1 - k, 3); el.textContent = raw.replace(m[0], nf(to * e, d)); if (k < 1) requestAnimationFrame(step); else el.textContent = raw; };
      requestAnimationFrame(step);
    });
  }
  /* مجموعة أزرار مقطعية: [data-seg=name] داخلها أزرار [data-v] */
  function seg(root, name, cb) {
    const g = root.querySelector(`[data-seg="${name}"]`); if (!g) return;
    g.addEventListener('click', e => { const b = e.target.closest('[data-v]'); if (!b) return; g.querySelectorAll('[data-v]').forEach(x => x.setAttribute('aria-pressed', String(x === b))); cb(b.dataset.v, b); });
  }
  const legend = items => `<div class="dk-leg">${items.map(([n, c, k]) => `<span${k != null ? ` data-key="${esc(k)}"` : ''}><i style="background:${c}"></i>${esc(n)}</span>`).join('')}</div>`;

  /* ---------- محاور مشتركة ---------- */
  const niceMax = v => { if (v <= 0) return 1; const p = Math.pow(10, Math.floor(Math.log10(v))), f = v / p; return (f <= 1 ? 1 : f <= 2 ? 2 : f <= 2.5 ? 2.5 : f <= 5 ? 5 : 10) * p; };
  const yAxis = (W, H, top, bot, min, max, fmt, ticks = 4) => { let s = ''; for (let i = 0; i <= ticks; i++) { const v = min + (max - min) * i / ticks, y = bot - (bot - top) * i / ticks; s += `<line class="gr${v === 0 && min < 0 ? ' z' : ''}" x1="8" x2="${W - 44}" y1="${y}" y2="${y}"/><text class="yt" x="${W - 4}" y="${y + 4}">${fmt(v)}</text>`; } return s; };
  /* موضع س معكوس: الفهرس 0 في اليمين */
  const xr = (W, i, n, L = 12, R = W - 50) => R - (i + .5) * (R - L) / n;

  /* أعمدة رأسية: series=[{name,color,vals}], stacked, sel(فهرس فئة مختار), key(للتصفية) */
  function bars(o) {
    const W = Math.max(300, o.W || 600);
    const { labels, series, H = 220, stacked = false, fmt = v => txt(v), tipf, ghost, target, key = 'x', sel = null, neg = false } = o;
    const n = labels.length, top = 14, bot = H - 26;
    const tot = labels.map((_, i) => stacked ? series.reduce((a, s) => a + Math.max(0, s.vals[i]), 0) : Math.max(...series.map(s => s.vals[i]), ghost ? ghost[i] : 0));
    const lo = neg ? Math.min(0, ...series.flatMap(s => s.vals)) : 0; let mxv = o.ymax || niceMax(Math.max(...tot, target || 0) * 1.05), mn = lo < 0 ? -niceMax(-lo * 1.05) : 0, tk = 4;
    if (mn < 0) { const st = niceMax((Math.max(...tot) - lo) / 4); mn = -Math.ceil(-lo / st) * st; mxv = Math.ceil(Math.max(...tot) * 1.02 / st) * st; tk = Math.round((mxv - mn) / st); }
    const Y = v => bot - (v - mn) / (mxv - mn) * (bot - top), bw = (W - 62) / n, gw = stacked ? Math.min(38, bw * .62) : series.length === 1 ? Math.min(40, bw * .58) : Math.min(22, bw * .8 / series.length);
    let s = `<svg viewBox="0 0 ${W} ${H}" class="dk-svg" role="img" aria-label="${esc(o.aria || 'أعمدة')}">` + yAxis(W, H, top, bot, mn, mxv, v => esc(txt(v)), tk);
    labels.forEach((lb, i) => {
      const cx = xr(W, i, n), dim = sel != null && sel !== i ? ' dim' : '';
      /* §9 (لوحة المفاتيح): العمود قابل للتركيز ويعمل بـ Enter/Space (معالج role=button العام أدناه)
         متى مُرّر key صراحة — أي أن المحرك يستخدمه للتصفية المتقاطعة، لا مجرد تلميح. */
      const pick = 'key' in o ? ` tabindex="0" role="button" aria-label="${esc(lb)}: ${esc(series.map(se => se.vals[i] == null ? '' : `${se.name} ${txt(se.vals[i])}`).filter(Boolean).join('، '))}" aria-pressed="${sel === i}"` : '';
      s += `<g class="col${dim}" data-${key}="${i}" style="--i:${i}"${pick}>`;
      s += `<rect class="hit" x="${cx - bw / 2}" y="${top}" width="${bw}" height="${bot - top}"/>`;
      if (ghost) s += `<rect class="gh" x="${cx - gw * series.length / 2 - 2}" y="${Y(ghost[i])}" width="${gw * series.length + 4}" height="${Y(0) - Y(ghost[i])}" rx="4"/>`;
      let acc = 0;
      series.forEach((se, k) => {
        const v = se.vals[i]; if (v == null) return;
        let x, y, h;
        if (stacked) { x = cx - gw / 2; y = Y(acc + v); h = Y(acc) - y; acc += v; if (k > 0) h -= 2; }
        else { x = cx + (series.length / 2 - k - 1) * gw; y = v >= 0 ? Y(v) : Y(0); h = Math.abs(Y(v) - Y(0)); }
        s += `<rect class="b${v < 0 ? ' ng' : ''}" x="${x + (stacked ? 0 : 1)}" y="${y}" width="${stacked ? gw : gw - 2}" height="${Math.max(1, h)}" rx="${stacked ? 2 : 3}" fill="${se.color}" data-tip="${esc(lb)} · ${esc(se.name)}|${esc(tipf ? tipf(v, i, k) : fmt(v))}"/>`;
      });
      s += `<text class="xt" x="${cx}" y="${H - 8}">${esc(lb)}</text></g>`;
    });
    if (target != null) s += `<line class="tg" x1="8" x2="${W - 44}" y1="${Y(target)}" y2="${Y(target)}"/><text class="tgl" x="12" y="${Y(target) - 5}">${esc(o.targetLabel || 'المستهدف')}</text>`;
    return s + '</svg>';
  }

  /* أشرطة أفقية (HTML): items=[{label,val,color,key,sub}] */
  function hbars(items, o = {}) {
    const mx = o.max || Math.max(...items.map(i => i.val)) || 1, fmt = o.fmt || (v => num(v));
    return `<ul class="dk-hb">${items.map((it, i) => `<li class="${o.sel != null && o.sel !== it.key ? 'dim' : ''}${o.sel === it.key ? ' on' : ''}" data-${o.attr || 'k'}="${esc(it.key ?? i)}" tabindex="0" role="button" style="--i:${i}" data-tip="${esc(it.label)}|${esc(o.tip ? o.tip(it) : txt(it.val, o.u || ''))}"><span class="l">${esc(it.label)}</span><span class="t"><i style="width:${(it.val / mx * 100).toFixed(1)}%;background:${it.color || o.color || PAL[0]}"></i>${o.target != null ? `<b class="tg" style="inset-inline-start:${(o.target / mx * 100).toFixed(1)}%"></b>` : ''}</span><span class="v">${fmt(it.val, it)}</span></li>`).join('')}</ul>`;
  }

  /* خطوط/مساحة: series=[{name,color,vals,dash,area}], هوفر بخط عمودي */
  function line(o) {
    const W = Math.max(300, o.W || 600);
    const { labels, series, H = 220, fmt = v => txt(v), min0 = true, key = 'x', band } = o;
    const n = labels.length, top = 14, bot = H - 26, all = series.flatMap(s => s.vals.filter(v => v != null));
    let mn = min0 ? 0 : Math.min(...all), mx = Math.max(...all); if (!min0) { const pad = (mx - mn) * .15 || 1; mn = Math.max(0, mn - pad); } mx = min0 ? niceMax(mx * 1.05) : mx + (mx - mn) * .12;
    const X = i => xr(W, i, n), Y = v => bot - (v - mn) / (mx - mn) * (bot - top);
    const f2 = v => Math.abs(mx - mn) < 10 ? nf(v, 1) : txt(v);
    let s = `<svg viewBox="0 0 ${W} ${H}" class="dk-svg dk-line" role="img" aria-label="${esc(o.aria || 'خط زمني')}">` + yAxis(W, H, top, bot, mn, mx, v => esc(f2(v)));
    if (band) s += `<rect class="band" x="${X(band[1]) - (X(0) - X(1)) / 2}" y="${top}" width="${(band[1] - band[0] + 1) * (X(0) - X(1))}" height="${bot - top}"/>`;
    series.forEach((se, k) => {
      const pts = se.vals.map((v, i) => v == null ? null : [X(i), Y(v)]), segs = []; let cur = [];
      pts.forEach(p => { if (p) cur.push(p); else if (cur.length) { segs.push(cur); cur = []; } }); if (cur.length) segs.push(cur);
      segs.forEach(sg => { const d = 'M' + sg.map(p => p.map(x => x.toFixed(1)).join(',')).join('L');
        if (se.area) s += `<path class="ar${se.fade ? ' fd' : ''}" d="${d}L${sg[sg.length - 1][0].toFixed(1)},${bot}L${sg[0][0].toFixed(1)},${bot}Z" fill="${se.color}"/>`;
        s += `<path class="ln jr-reveal${se.dash ? ' ds' : ''}${se.fade ? ' fd' : ''}" d="${d}" stroke="${se.color}" style="--i:${k}"/>`; });
      if (se.dots) pts.forEach((p, i) => p && (s += `<circle class="dt" cx="${p[0]}" cy="${p[1]}" r="3.5" fill="${se.color}"/>`));
    });
    labels.forEach((lb, i) => { if (n <= 14 || i % Math.ceil(n / 12) === 0) s += `<text class="xt" x="${X(i)}" y="${H - 8}">${esc(lb)}</text>`; });
    const cw = X(0) - (n > 1 ? X(1) : X(0) - 40);
    labels.forEach((lb, i) => { s += `<g class="hv" data-${key}="${i}"><rect x="${X(i) - cw / 2}" y="${top}" width="${cw}" height="${bot - top}" data-tip="${esc(lb)}|${esc(series.filter(se => se.vals[i] != null).map(se => `${se.name}: ${fmt(se.vals[i])}`).join(' · '))}"/><line x1="${X(i)}" x2="${X(i)}" y1="${top}" y2="${bot}"/>${series.map(se => se.vals[i] == null ? '' : `<circle cx="${X(i)}" cy="${Y(se.vals[i])}" r="4" fill="${se.color}"/>`).join('')}</g>`; });
    return s + '</svg>';
  }

  /* حلقة: items=[{label,val,color,key}] */
  function donut(items, o = {}) {
    const R = o.r || 70, th = o.th || 22, L = 2 * Math.PI * R, tot = items.reduce((a, b) => a + b.val, 0) || 1; let acc = 0;
    const segs = items.map((it, j) => { const len = it.val / tot * L, gap = items.length > 1 ? 3 : 0, s = `<circle cx="100" cy="100" r="${R}" class="sg${o.sel != null && o.sel !== it.key ? ' dim' : ''}"${o.attr ? ` tabindex="0" role="button" aria-label="${esc(it.label)}: ${esc(txt(it.val))}" aria-pressed="${o.sel === it.key}"` : ''} stroke="${it.color}" stroke-width="${th}" stroke-dasharray="${Math.max(0, len - gap).toFixed(1)} ${L}" stroke-dashoffset="${(-acc).toFixed(1)}" transform="rotate(-90 100 100)" data-${o.attr || 'k'}="${esc(it.key ?? j)}" style="--i:${j};--L:${L.toFixed(0)}" data-tip="${esc(it.label)}|${esc((o.tip ? o.tip(it) : txt(it.val)) + ' · ' + nf(it.val / tot * 100, 1) + '%')}"/>`; acc += len; return s; }).join('');
    return `<div class="dk-donut" style="--sz:${o.size || 180}px"><svg viewBox="0 0 200 200" role="img" aria-label="${esc(o.aria || 'حلقة')}"><circle cx="100" cy="100" r="${R}" class="tr" stroke-width="${th}"/>${segs}</svg><span class="c">${o.center || ''}</span></div>`;
  }

  /* شلال: steps=[{label,val,kind:'total'|'delta'}] — الموجب أخضر، السالب أحمر، الإجمالي رمادي داكن */
  function waterfall(steps, o = {}) {
    const W = Math.max(300, o.W || 600);
    const H = o.H || 230, top = 18, bot = H - 30, n = steps.length; let run = 0; const bars = [];
    steps.forEach(st => { if (st.kind === 'total') { bars.push({ ...st, a: 0, b: st.val }); run = st.val; } else { bars.push({ ...st, a: run, b: run + st.val }); run += st.val; } });
    const lo = Math.min(0, ...bars.map(b => Math.min(b.a, b.b))), hi = Math.max(...bars.map(b => Math.max(b.a, b.b)));
    const mn = lo < 0 ? -niceMax(-lo * 1.1) : 0, mx = niceMax(hi * 1.08), Y = v => bot - (v - mn) / (mx - mn) * (bot - top), bw = Math.min(54, (W - 62) / n * .62), fmt = o.fmt || (v => txt(v));
    let s = `<svg viewBox="0 0 ${W} ${H}" class="dk-svg dk-wf" role="img" aria-label="${esc(o.aria || 'شلال')}">` + yAxis(W, H, top, bot, mn, mx, v => esc(txt(v)));
    bars.forEach((b, i) => { const cx = xr(W, i, n), y = Y(Math.max(b.a, b.b)), h = Math.max(2, Math.abs(Y(b.a) - Y(b.b))), cls = b.kind === 'total' ? 'tot' : b.val >= 0 ? 'up' : 'dn';
      const lab = b.kind === 'total' ? fmt(Math.abs(b.val)) : fmt(Math.abs(b.val)).replace(/^[\d.,]+%?/, m => '\u2066' + (b.val >= 0 ? '+' : '−') + m + '\u2069');
      s += `<g class="col${o.sel != null && o.sel !== i ? ' dim' : ''}" data-${o.attr || 'wf'}="${i}" style="--i:${i}" tabindex="0" role="button" aria-label="${esc(b.label)}: ${esc(lab)}"><rect class="hit" x="${cx - (W - 62) / n / 2}" y="${top}" width="${(W - 62) / n}" height="${H - top}"/><rect class="b ${cls}" x="${cx - bw / 2}" y="${y}" width="${bw}" height="${h}" rx="3" data-tip="${esc(b.label)}|${esc(lab)}"/>${(W - 62) / n >= 64 || b.kind === 'total' ? `<text class="vl" x="${cx}" y="${y - 6}">${esc(lab)}</text>` : ''}<text class="xt" x="${cx}" y="${H - 10}">${esc(b.label)}</text></g>`;
      if (i < n - 1) { const nx = xr(W, i + 1, n); s += `<line class="cn" x1="${cx - bw / 2}" x2="${nx + bw / 2}" y1="${Y(b.b)}" y2="${Y(b.b)}"/>`; } });
    return s + '</svg>';
  }

  /* تشتت/فقاعات بأرباع: pts=[{x,y,r,label,color,key}] */
  function scatter(pts, o = {}) {
    const W = Math.max(300, o.W || 600);
    const H = o.H || 260, L = 16, R = W - 50, top = 16, bot = H - 34;
    const xs = pts.map(p => p.x), ys = pts.map(p => p.y), x0 = o.xmin ?? Math.min(...xs) * .9, x1 = o.xmax ?? Math.max(...xs) * 1.08, y0 = o.ymin ?? 0, y1 = o.ymax ?? niceMax(Math.max(...ys) * 1.1);
    /* في RTL: المحور الأفقي يتزايد من اليمين لليسار */
    const X = v => R - (v - x0) / (x1 - x0) * (R - L), Y = v => bot - (v - y0) / (y1 - y0) * (bot - top), fx = o.fx || (v => txt(v)), fy = o.fy || (v => txt(v));
    let s = `<svg viewBox="0 0 ${W} ${H}" class="dk-svg dk-sc" role="img" aria-label="${esc(o.aria || 'مصفوفة')}">`;
    for (let i = 0; i <= 4; i++) { const yv = y0 + (y1 - y0) * i / 4, xv = x0 + (x1 - x0) * i / 4; s += `<line class="gr" x1="${L}" x2="${R}" y1="${Y(yv)}" y2="${Y(yv)}"/><text class="yt" x="${W - 4}" y="${Y(yv) + 4}">${esc(fy(yv))}</text><text class="xt" x="${X(xv)}" y="${H - 14}">${esc(fx(xv))}</text>`; }
    if (o.qx != null) s += `<line class="q" x1="${X(o.qx)}" x2="${X(o.qx)}" y1="${top}" y2="${bot}"/>`;
    if (o.qy != null) s += `<line class="q" x1="${L}" x2="${R}" y1="${Y(o.qy)}" y2="${Y(o.qy)}"/>`;
    if (o.quads) { const [tr, tl, br, bl] = o.quads; s += `<text class="qd" x="${R - 6}" y="${top + 12}" text-anchor="start">${esc(tr)}</text><text class="qd" x="${L + 6}" y="${top + 12}" text-anchor="end">${esc(tl)}</text><text class="qd" x="${R - 6}" y="${bot - 6}" text-anchor="start">${esc(br)}</text><text class="qd" x="${L + 6}" y="${bot - 6}" text-anchor="end">${esc(bl)}</text>`; }
    pts.forEach((p, i) => { s += `<g class="pt${o.sel != null && o.sel !== p.key ? ' dim' : ''}${o.sel === p.key ? ' on' : ''}" data-${o.attr || 'k'}="${esc(p.key ?? i)}" style="--i:${i}" tabindex="0" role="button" aria-label="${esc(p.label)}"><circle cx="${X(p.x)}" cy="${Y(p.y)}" r="${p.r || 8}" fill="${p.color || PAL[0]}" data-tip="${esc(p.label)}|${esc(o.tip ? o.tip(p) : fx(p.x) + ' · ' + fy(p.y))}"/>${o.labels !== false ? `<text class="pl" x="${X(p.x) - (p.r || 8) - 4}" y="${Y(p.y) + 4}">${esc(p.label)}</text>` : ''}</g>`; });
    if (o.xl) s += `<text class="al" x="${(L + R) / 2}" y="${H - 1}">${esc(o.xl)}</text>`;
    return s + '</svg>';
  }

  /* قمع أفقي (HTML) مع صيغة التسمية: first|prev|delta */
  function funnel(stages, mode = 'first', o = {}) {
    const f = stages[0].val || 1;
    return `<ol class="dk-fn">${stages.map((s, i) => { const p = i ? stages[i - 1].val : s.val, lab = mode === 'first' ? nf(s.val / f * 100, 1) + '%' : mode === 'prev' ? nf(s.val / (p || 1) * 100, 1) + '%' : i ? '−' + nf((p - s.val) / (p || 1) * 100, 1) + '%' : '—';
      return `<li class="${o.sel != null && o.sel !== i ? 'dim' : ''}${o.sel === i ? ' on' : ''}" data-${o.attr || 'st'}="${i}" tabindex="0" role="button" style="--i:${i}"><span class="l">${esc(s.label)}</span><span class="t"><i style="width:${Math.max(2, s.val / f * 100).toFixed(1)}%"><b><span class="num">${nf(s.val)}</span> · <span class="num">${lab}</span></b></i></span></li>`; }).join('')}</ol>`;
  }

  /* خريطة حرارية (HTML): rows, cols, v[r][c], scale(v)->لون, fmt */
  function heat(rows, cols, v, o = {}) {
    const all = v.flat().filter(x => x != null), mn = o.min ?? Math.min(...all), mx = o.max ?? Math.max(...all), hue = o.hue || '18,125,189';
    const a = x => .08 + .87 * ((x - mn) / ((mx - mn) || 1));
    return `<div class="dk-heat" style="--c:${cols.length}"><span></span>${cols.map(c => `<span class="ch">${esc(c)}</span>`).join('')}${rows.map((r, i) => `<span class="rh${o.selRow === i ? ' on' : ''}">${esc(r)}</span>${cols.map((c, j) => { const x = v[i][j]; return x == null ? '<span class="cl na"></span>' : `<span class="cl${o.sel && (o.sel[0] !== i && o.sel[1] !== j) ? ' dim' : ''}" data-r="${i}" data-c="${j}" style="--i:${i + j};background:rgba(${hue},${a(x).toFixed(2)});color:${a(x) > .55 ? '#fff' : 'inherit'}" data-tip="${esc(r)} · ${esc(c)}|${esc(o.fmt ? o.fmt(x) : nf(x))}">${o.show === false ? '' : esc(o.fmt ? o.fmt(x) : nf(x))}</span>`; }).join('')}`).join('')}</div>`;
  }

  /* خط مصغّر: الأقدم يمين */
  function spark(vals, color = PAL[0], o = {}) {
    const w = o.w || 120, h = o.h || 32, mn = Math.min(...vals), mx = Math.max(...vals), n = vals.length;
    const P = vals.map((v, i) => [w - 2 - i * (w - 4) / (n - 1), h - 3 - (v - mn) / ((mx - mn) || 1) * (h - 6)]);
    const d = 'M' + P.map(p => p.map(x => x.toFixed(1)).join(',')).join('L');
    return `<svg class="dk-spark" viewBox="0 0 ${w} ${h}" aria-hidden="true"><path d="${d}L${P[n - 1][0]},${h}L${P[0][0]},${h}Z" fill="${color}" opacity=".12"/><path class="jr-reveal" d="${d}" fill="none" stroke="${color}" stroke-width="2" vector-effect="non-scaling-stroke"/><circle cx="${P[n - 1][0]}" cy="${P[n - 1][1]}" r="2.5" fill="${color}"/></svg>`;
  }

  /* مفاتيح الوصول: Enter/Space على [role=button] غير الأزرار */
  document.addEventListener('keydown', e => { const t = e.target; if ((e.key === 'Enter' || e.key === ' ') && t.getAttribute && t.getAttribute('role') === 'button' && t.tagName !== 'BUTTON') { e.preventDefault(); t.dispatchEvent(new MouseEvent('click', { bubbles: true })); } });

  /* عرض الحاوية الفعلي ليبقى حجم النص ثابتًا، وإعادة الرسم عند تغيّر المقاس */
  const wOf = el => Math.round(Math.max(300, (el && el.clientWidth) || 600));
  function onResize(root, fn) { let w = root.clientWidth, t; new ResizeObserver(() => { if (Math.abs(root.clientWidth - w) < 24) return; w = root.clientWidth; clearTimeout(t); t = setTimeout(fn, 120); }).observe(root); }
  window.DK = { wOf, onResize, RM, PAL, ST, ic, esc, rng, nf, abbr, num, money, pct, txt, delta, toast, count, seg, legend, bars, hbars, line, donut, waterfall, scatter, funnel, heat, spark, niceMax };
})();
