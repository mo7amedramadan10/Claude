/* محرك العروض التقديمية «جيم» — يرسم مواصفة العرض (JSON) داخل المنصة وفي الصفحة المنشورة نفسها.
   JEEM_DECK.renderSlide(root, slide, spec, opts)   → يرسم شريحة واحدة (opts.editable → نصوص قابلة للتعديل، onEdit(slide))
   JEEM_DECK.thumbnail(slide, index)                → HTML مصغّرة
   JEEM_DECK.present(root, spec, opts)              → عرض كامل بتنقّل (الصفحة المنشورة): أسهم، مصغّرات، طباعة، كتل قابلة للتعديل عبر opts.editableStore
   الرسوم SVG خفيفة (أعمدة/خط/دائري) بلا مكتبات حتى تعمل الصفحة المنشورة مستقلة. */
(function () {
  'use strict';
  const PALETTE = ['#2A78D6', '#0E8F86', '#B86E00', '#7A4DD9', '#C2413B', '#4C9A2A', '#5A6B77', '#D14B8F'];
  const esc = s => String(s ?? '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
  const num = v => (typeof v === 'number' && isFinite(v)) ? v.toLocaleString('en-US', { maximumFractionDigits: 2 }) : esc(v);

  const CSS = `
.jd-slide{position:relative;aspect-ratio:16/9;width:100%;background:#fff;color:#13232E;border-radius:14px;overflow:hidden;font-family:var(--jd-font,'IBM Plex Sans Arabic'),'IBM Plex Sans Arabic',Tahoma,Arial,sans-serif;direction:rtl;box-shadow:0 10px 30px -18px rgba(0,0,0,.45);container-type:inline-size}
.jd-slide *{box-sizing:border-box}
.jd-slide .jd-in{position:absolute;inset:0;padding:5.5cqw 6cqw;display:flex;flex-direction:column;gap:2.2cqw}
.jd-slide .jd-accent{position:absolute;top:0;right:0;left:0;height:1.1cqw;background:var(--jd-accent,#2A78D6)}
.jd-slide h1{font-size:4.6cqw;margin:0;line-height:1.35;font-weight:700;color:var(--jd-primary,#0B3D5C)}
.jd-slide h2{font-size:3.4cqw;margin:0;line-height:1.35;font-weight:700;color:var(--jd-primary,#0B3D5C);padding-bottom:1cqw;border-bottom:2px solid #E3EAF0}
.jd-slide .jd-sub{font-size:2.1cqw;color:#5A6B77;margin:0}
.jd-slide .jd-body{flex:1;min-height:0;display:flex;flex-direction:column;gap:1.6cqw;font-size:2.2cqw;line-height:1.7}
.jd-slide ul.jd-bullets{margin:0;padding-inline-start:3cqw;display:flex;flex-direction:column;gap:1.1cqw}
.jd-slide ul.jd-bullets li::marker{color:var(--jd-accent,#2A78D6)}
.jd-slide .jd-cols{display:grid;grid-template-columns:repeat(var(--n,2),minmax(0,1fr));gap:3cqw;flex:1;min-height:0}
.jd-slide .jd-col{background:#F5F8FA;border:1px solid #E3EAF0;border-radius:1.4cqw;padding:2cqw}
.jd-slide .jd-col h3{margin:0 0 1cqw;font-size:2.4cqw;color:#0E8F86}
.jd-slide .jd-kpis{display:grid;grid-template-columns:repeat(var(--n,4),minmax(0,1fr));gap:2.4cqw;align-content:center;flex:1}
.jd-slide .jd-kpi{background:#F5F8FA;border:1px solid #E3EAF0;border-radius:1.6cqw;padding:3cqw 2cqw;text-align:center;display:flex;flex-direction:column;gap:.8cqw;justify-content:center}
.jd-slide .jd-kpi b{font-size:5cqw;color:var(--jd-accent,#2A78D6);font-variant-numeric:tabular-nums;line-height:1.2}
.jd-slide .jd-kpi span{font-size:2cqw;color:#5A6B77}
.jd-slide .jd-kpi i{font-style:normal;font-size:1.8cqw;font-weight:600}
.jd-slide .jd-kpi i.up{color:#1E7D45}.jd-slide .jd-kpi i.down{color:#C2413B}.jd-slide .jd-kpi i.flat{color:#5A6B77}
.jd-slide .jd-chart{flex:1;min-height:0;display:flex;align-items:stretch}
.jd-slide .jd-chart svg{width:100%;height:100%;font-family:inherit}
.jd-slide table{border-collapse:collapse;width:100%;font-size:1.9cqw}
.jd-slide th,.jd-slide td{border-bottom:1px solid #E3EAF0;padding:.9cqw 1.2cqw;text-align:start}
.jd-slide th{background:#F5F8FA;color:#0B3D5C;font-weight:700}
.jd-slide td.n,.jd-slide th.n{font-variant-numeric:tabular-nums}
.jd-slide .jd-quote{font-size:3.2cqw;line-height:1.7;margin:auto 0;padding-inline-start:3cqw;border-inline-start:.8cqw solid var(--jd-accent,#2A78D6);color:#13232E}
.jd-slide .jd-img{flex:1;min-height:0;display:flex;flex-direction:column;align-items:center;justify-content:center;gap:1cqw}
.jd-slide .jd-img img{max-width:100%;max-height:100%;object-fit:contain;border-radius:1cqw}
.jd-slide .jd-cap{font-size:1.8cqw;color:#5A6B77}
.jd-slide .jd-src{position:absolute;bottom:2cqw;left:6cqw;right:6cqw;font-size:1.5cqw;color:#8A98A3;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}
.jd-slide .jd-num{position:absolute;bottom:2cqw;left:6cqw;font-size:1.6cqw;color:#8A98A3}
.jd-slide .jd-missing{margin:auto;padding:2cqw 3cqw;border:1px dashed #C2413B;border-radius:1.2cqw;color:#C2413B;font-size:2cqw;text-align:center;max-width:70%}
.jd-slide.jd-title .jd-in{justify-content:center;align-items:center;text-align:center;background:linear-gradient(135deg,var(--jd-primary,#0B3D5C),var(--jd-secondary,#0E8F86));color:#fff}
.jd-slide .jd-logo{position:absolute;top:2.4cqw;left:6cqw;height:5cqw;max-width:20cqw;object-fit:contain;object-position:left center}
.jd-slide .jd-logo~.jd-in h2{padding-left:22cqw}
.jd-slide.jd-title .jd-logo{position:static;height:11cqw;max-width:34cqw;margin-bottom:1cqw;filter:drop-shadow(0 2px 6px rgba(0,0,0,.25))}
.jd-slide.jd-title .jd-foot{position:absolute;bottom:2.4cqw;left:6cqw;right:6cqw;text-align:center;font-size:1.7cqw;color:rgba(255,255,255,.8)}
.jd-slide.jd-section .jd-logo{top:auto;bottom:2.4cqw}
.jd-slide.jd-title h1{font-size:6cqw;color:#fff}.jd-slide.jd-title .jd-sub{color:#D7E9F2;font-size:2.6cqw}
.jd-slide.jd-section .jd-in{justify-content:center;background:#F5F8FA}
.jd-slide.jd-section h1{font-size:5cqw}.jd-slide.jd-section .jd-sub{font-size:2.4cqw}
.jd-slide [contenteditable]{outline:none;border-radius:.6cqw;transition:box-shadow .15s}
.jd-slide [contenteditable]:hover{box-shadow:0 0 0 2px #CFE0F2}
.jd-slide [contenteditable]:focus{box-shadow:0 0 0 2px #2A78D6;background:#F7FAFD}
.jd-slide .jd-tasks{display:flex;flex-direction:column;gap:1cqw;font-size:2.1cqw}
.jd-slide .jd-task{display:flex;align-items:center;gap:1.4cqw;padding:1cqw 1.4cqw;background:#F5F8FA;border:1px solid #E3EAF0;border-radius:1cqw}
.jd-slide .jd-task input[type=checkbox]{width:2.4cqw;height:2.4cqw;accent-color:#0E8F86;flex:none;margin:0}
.jd-slide .jd-task .t{flex:1;min-width:0;background:transparent;border:0;font:inherit;color:inherit;padding:.4cqw;direction:rtl}
.jd-slide .jd-task.done .t{color:#5A6B77;text-decoration:line-through}
.jd-slide .jd-task .x{all:unset;cursor:pointer;color:#8A98A3;padding:0 .8cqw;font-size:2.4cqw}.jd-slide .jd-task .x:hover{color:#C2413B}
.jd-slide .jd-add{all:unset;cursor:pointer;color:#0E8F86;font-weight:600;font-size:1.9cqw;padding:.6cqw 0}
.jd-slide .jd-tasks-bar{display:flex;gap:1.4cqw;align-items:center;font-size:1.8cqw;color:#5A6B77}
.jd-slide .jd-tasks-bar button{all:unset;cursor:pointer;background:#0B3D5C;color:#fff;padding:.8cqw 2cqw;border-radius:999px;font-weight:600}
.jd-slide .jd-tasks-bar button[disabled]{opacity:.5;cursor:default}
.jd-thumb{aspect-ratio:16/9;width:100%;border-radius:8px;overflow:hidden;background:#fff;border:2px solid transparent;cursor:pointer;position:relative;color:#13232E;font-family:inherit;direction:rtl}
.jd-thumb.active{border-color:#2A78D6}
.jd-thumb .jd-tt{position:absolute;inset:0;padding:8px 10px;font-size:11px;font-weight:700;color:#0B3D5C;overflow:hidden;line-height:1.4}
.jd-thumb .jd-tk{position:absolute;bottom:4px;left:6px;font-size:10px;color:#8A98A3}
.jd-thumb.jd-title{background:linear-gradient(135deg,var(--jd-primary,#0B3D5C),var(--jd-secondary,#0E8F86))}.jd-thumb.jd-title .jd-tt{color:#fff}
.jd-present{height:100%;display:flex;flex-direction:column;background:#0f1720;color:#E6EEF2;font-family:var(--jd-font,'IBM Plex Sans Arabic'),'IBM Plex Sans Arabic',Tahoma,Arial,sans-serif;direction:rtl}
.jd-present .jd-stage{flex:1;min-height:0;display:flex;align-items:center;justify-content:center;padding:18px}
.jd-present .jd-stage .jd-slide{max-height:100%;max-width:min(100%,calc((100vh - 150px) * 16 / 9))}
.jd-present .jd-bar{display:flex;align-items:center;gap:10px;padding:8px 14px;background:#15222B;border-top:1px solid #26394A;font-size:13px}
.jd-present .jd-bar button{all:unset;cursor:pointer;padding:6px 12px;border-radius:999px;border:1px solid #3B5368;color:#E6EEF2}
.jd-present .jd-bar button:hover{border-color:#3CC5B9}
.jd-present .jd-bar .jd-pos{flex:1;text-align:center;font-variant-numeric:tabular-nums}
.jd-present .jd-strip{display:flex;gap:8px;overflow-x:auto;padding:8px 14px;background:#0f1720;scrollbar-width:thin}
.jd-present .jd-strip .jd-thumb{flex:0 0 120px}
@media print{.jd-present{background:#fff;color:#000}.jd-present .jd-bar,.jd-present .jd-strip{display:none}.jd-present .jd-stage{display:block;padding:0}.jd-print .jd-slide{page-break-after:always;border-radius:0;box-shadow:none;max-width:none;margin:0 0 4px}}
@media (prefers-reduced-motion:reduce){.jd-slide *{transition:none!important}}
`;
  function ensureCss(doc) {
    if (doc.getElementById('jeem-deck-css')) return;
    const st = doc.createElement('style'); st.id = 'jeem-deck-css'; st.textContent = CSS; doc.head.appendChild(st);
  }

  // ---- الرسوم SVG ----
  function chartSvg(c, w, h) {
    const labels = c.labels || [], vals = (c.values || []).map(Number), type = c.type === 'column' ? 'bar' : (c.type || 'bar');
    if (!labels.length || !vals.length) return '<div class="jd-missing">لا توجد بيانات للرسم</div>';
    if (type === 'pie') return pieSvg(labels, vals, w, h, c.unit);
    const n = Math.min(labels.length, vals.length), max = Math.max(...vals.slice(0, n), 0) || 1;
    const padL = 10, padR = 10, padT = 18, padB = 40, cw = w - padL - padR, ch = h - padT - padB;
    let out = `<svg viewBox="0 0 ${w} ${h}" role="img" aria-label="${esc(c.unit || '')}">`;
    for (let g = 0; g <= 4; g++) { const y = padT + ch - ch * g / 4; out += `<line x1="${padL}" x2="${w - padR}" y1="${y}" y2="${y}" stroke="#E3EAF0"/><text x="${w - padR}" y="${y - 3}" font-size="10" fill="#8A98A3" text-anchor="end">${num(max * g / 4)}</text>`; }
    if (type === 'line') {
      const step = n > 1 ? cw / (n - 1) : 0;
      const pts = vals.slice(0, n).map((v, i) => [w - padR - i * step, padT + ch - ch * v / max]);
      out += `<polyline fill="none" stroke="${PALETTE[0]}" stroke-width="3" stroke-linejoin="round" points="${pts.map(p => p.join(',')).join(' ')}"/>`;
      pts.forEach((p, i) => { out += `<circle cx="${p[0]}" cy="${p[1]}" r="4" fill="#fff" stroke="${PALETTE[0]}" stroke-width="2"/><text x="${p[0]}" y="${p[1] - 9}" font-size="10" fill="#13232E" text-anchor="middle">${num(vals[i])}</text><text x="${p[0]}" y="${h - padB + 16}" font-size="10" fill="#5A6B77" text-anchor="middle">${esc(trunc(labels[i], 12))}</text>`; });
    } else {
      const slot = cw / n, bw = Math.min(slot * .62, 70);
      vals.slice(0, n).forEach((v, i) => {
        const x = w - padR - slot * i - slot / 2 - bw / 2, bh = ch * v / max, y = padT + ch - bh;
        out += `<rect x="${x}" y="${y}" width="${bw}" height="${bh}" rx="4" fill="${PALETTE[i % PALETTE.length]}"/><text x="${x + bw / 2}" y="${y - 5}" font-size="10" fill="#13232E" text-anchor="middle">${num(v)}</text><text x="${x + bw / 2}" y="${h - padB + 16}" font-size="10" fill="#5A6B77" text-anchor="middle">${esc(trunc(labels[i], 12))}</text>`;
      });
    }
    return out + '</svg>';
  }
  function pieSvg(labels, vals, w, h, unit) {
    const total = vals.reduce((a, b) => a + (b > 0 ? b : 0), 0) || 1, r = Math.min(w, h) * .36, cx = w * .35, cy = h / 2;
    let a0 = -Math.PI / 2, out = `<svg viewBox="0 0 ${w} ${h}" role="img">`;
    vals.forEach((v, i) => {
      if (!(v > 0)) return; const a1 = a0 + 2 * Math.PI * v / total, large = a1 - a0 > Math.PI ? 1 : 0;
      const p = (a) => [cx + r * Math.cos(a), cy + r * Math.sin(a)];
      const [x0, y0] = p(a0), [x1, y1] = p(a1);
      out += `<path d="M${cx},${cy} L${x0},${y0} A${r},${r} 0 ${large} 1 ${x1},${y1} Z" fill="${PALETTE[i % PALETTE.length]}" stroke="#fff" stroke-width="2"/>`; a0 = a1;
    });
    out += `<circle cx="${cx}" cy="${cy}" r="${r * .55}" fill="#fff"/><text x="${cx}" y="${cy + 5}" font-size="14" font-weight="700" fill="#0B3D5C" text-anchor="middle">${num(total)}</text>`;
    const lx = w * .62; labels.forEach((l, i) => { const y = h / 2 - (labels.length - 1) * 11 + i * 22; out += `<rect x="${lx}" y="${y - 7}" width="12" height="12" rx="3" fill="${PALETTE[i % PALETTE.length]}"/><text x="${lx + 18}" y="${y + 4}" font-size="11" fill="#13232E">${esc(trunc(l, 18))} — ${Math.round((vals[i] || 0) / total * 100)}%</text>`; });
    return out + '</svg>';
  }
  const trunc = (s, n) => { s = String(s ?? ''); return s.length > n ? s.slice(0, n - 1) + '…' : s; };
  const deltaTone = d => !d ? '' : /^[-−]/.test(String(d).trim()) ? 'down' : /^[+▲]/.test(String(d).trim()) || /\d/.test(d) ? 'up' : 'flat';

  // ---- شريحة واحدة ----
  function renderSlide(root, slide, spec, opts) {
    opts = opts || {}; ensureCss(root.ownerDocument);
    const ed = !!opts.editable, ce = ed ? ' contenteditable="true" spellcheck="false"' : '';
    const L = slide.layout || 'bullets';
    root.className = 'jd-slide jd-' + L; applyBrand(root, spec);
    const brand = spec.brand || {}; const logo = brand.logo ? `<img class="jd-logo" src="${esc(brand.logo)}" alt="${esc(brand.name || '')}">` : '';
    const footText = brand.footer || brand.name || '';
    let body = '';
    const title = `<h${L === 'title' || L === 'section' ? 1 : 2} data-f="title"${ce}>${esc(slide.title)}</h${L === 'title' || L === 'section' ? 1 : 2}>`;
    const sub = (slide.subtitle || ed) ? `<p class="jd-sub" data-f="subtitle"${ce}>${esc(slide.subtitle || (ed ? '' : ''))}</p>` : '';
    if (slide.missingReason && !(slide.bullets || slide.kpis || slide.chart?.values?.length || slide.table?.rows?.length)) body = `<div class="jd-missing">⚠️ ${esc(slide.missingReason)}</div>`;
    else switch (L) {
      case 'title': case 'section': body = ''; break;
      case 'two-column': body = `<div class="jd-cols" style="--n:${(slide.columns || []).length || 2}">${(slide.columns || []).map((c, i) => `<div class="jd-col"><h3 data-f="columns.${i}.title"${ce}>${esc(c.title || '')}</h3><ul class="jd-bullets">${(c.bullets || []).map((b, k) => `<li data-f="columns.${i}.bullets.${k}"${ce}>${esc(b)}</li>`).join('')}</ul></div>`).join('')}</div>`; break;
      case 'kpis': body = `<div class="jd-kpis" style="--n:${Math.min((slide.kpis || []).length || 1, 4)}">${(slide.kpis || []).map((k, i) => `<div class="jd-kpi"><b data-f="kpis.${i}.value"${ce}>${esc(k.value)}</b><span data-f="kpis.${i}.label"${ce}>${esc(k.label)}</span>${k.delta ? `<i class="${deltaTone(k.delta)}">${esc(k.delta)}</i>` : ''}</div>`).join('')}</div>`; break;
      case 'chart': body = `<div class="jd-chart">${chartSvg(slide.chart || {}, 640, 300)}</div>`; break;
      case 'table': { const t = slide.table || { columns: [], rows: [] }; const isNum = (i) => t.rows.every(r => r[i] == null || /^[\d.,%+\-−\s]+$/.test(String(r[i]))); body = `<div style="overflow:auto;flex:1;min-height:0"><table><thead><tr>${t.columns.map((c, i) => `<th class="${isNum(i) ? 'n' : ''}">${esc(c)}</th>`).join('')}</tr></thead><tbody>${t.rows.map(r => `<tr>${t.columns.map((_, i) => `<td class="${isNum(i) ? 'n' : ''}">${esc(r[i] ?? '')}</td>`).join('')}</tr>`).join('')}</tbody></table></div>`; break; }
      case 'image': body = `<div class="jd-img">${slide.image?.src ? `<img src="${esc(slide.image.src)}" alt="${esc(slide.image.caption || '')}">` : '<div class="jd-missing">لا توجد صورة</div>'}${slide.image?.caption ? `<div class="jd-cap" data-f="image.caption"${ce}>${esc(slide.image.caption)}</div>` : ''}</div>`; break;
      case 'quote': body = `<blockquote class="jd-quote" data-f="quote"${ce}>${esc(slide.quote || '')}</blockquote>`; break;
      case 'tasks': body = `<div class="jd-tasks" data-tasks></div>`; break;
      default: body = `<ul class="jd-bullets">${(slide.bullets || (ed ? [''] : [])).map((b, k) => `<li data-f="bullets.${k}"${ce}>${esc(b)}</li>`).join('')}</ul>`;
    }
    const idx = opts.index != null ? `<div class="jd-num">${opts.index + 1}${opts.total ? ' / ' + opts.total : ''}</div>` : '';
    const srcLine = L === 'title' ? '' : [slide.source ? 'المصدر: ' + slide.source : '', footText].filter(Boolean).join('  ·  ');
    root.innerHTML = `<div class="jd-accent"></div>${L === 'title' ? '' : logo}<div class="jd-in">${L === 'title' ? logo : ''}${title}${sub}<div class="jd-body">${body}</div></div>${srcLine ? `<div class="jd-src" title="${esc(srcLine)}">${esc(srcLine)}</div>` : ''}${L === 'title' && footText ? `<div class="jd-foot">${esc(footText)}</div>` : ''}${idx}`;
    if (L === 'tasks') mountTasks(root.querySelector('[data-tasks]'), slide, opts);
    if (ed) bindEditing(root, slide, opts);
    return root;
  }

  function bindEditing(root, slide, opts) {
    root.querySelectorAll('[data-f]').forEach(el => {
      el.addEventListener('keydown', e => { if (e.key === 'Enter' && !el.matches('li')) { e.preventDefault(); el.blur(); } });
      el.addEventListener('blur', () => { setPath(slide, el.dataset.f, el.textContent.trim()); opts.onEdit && opts.onEdit(slide); });
    });
  }
  function setPath(obj, path, value) {
    const parts = path.split('.'); let o = obj;
    for (let i = 0; i < parts.length - 1; i++) { const k = isNaN(parts[i]) ? parts[i] : +parts[i]; if (o[k] == null) o[k] = isNaN(parts[i + 1]) ? {} : []; o = o[k]; }
    const last = isNaN(parts.at(-1)) ? parts.at(-1) : +parts.at(-1); o[last] = value;
  }

  // ---- كتلة المهام القابلة للتعديل (jeem.store في الصفحة المنشورة؛ محليًا في المحرر) ----
  function mountTasks(box, slide, opts) {
    const key = (slide.editable && slide.editable.key) || ('tasks-' + slide.id);
    const store = opts.editableStore || null;
    let items = (slide.tasks || []).map(t => ({ t: t.t || t, done: !!t.done })), canEdit = true, version = null, dirty = false;
    const render = () => {
      box.innerHTML = items.map((it, i) => `<label class="jd-task${it.done ? ' done' : ''}"><input type="checkbox" ${it.done ? 'checked' : ''} ${canEdit ? '' : 'disabled'} data-i="${i}"><input class="t" value="${esc(it.t)}" data-i="${i}" ${canEdit ? '' : 'readonly'} aria-label="مهمة"><button class="x" data-x="${i}" title="حذف" ${canEdit ? '' : 'hidden'}>×</button></label>`).join('')
        + (canEdit ? `<button class="jd-add" data-add>+ إضافة مهمة</button>` : '')
        + (store ? `<div class="jd-tasks-bar"><span data-msg>${canEdit ? (dirty ? 'لديك تعديلات غير محفوظة' : '') : 'عرض فقط'}</span><button data-save ${dirty && canEdit ? '' : 'disabled'}>حفظ</button></div>` : '');
    };
    box.addEventListener('change', e => { if (e.target.type === 'checkbox') { items[+e.target.dataset.i].done = e.target.checked; dirty = true; sync(); render(); } });
    box.addEventListener('input', e => { if (e.target.classList.contains('t')) { items[+e.target.dataset.i].t = e.target.value; dirty = true; sync(); const b = box.querySelector('[data-save]'); if (b) b.disabled = false; } });
    box.addEventListener('click', async e => {
      if (e.target.matches('[data-x]')) { items.splice(+e.target.dataset.x, 1); dirty = true; sync(); render(); }
      else if (e.target.matches('[data-add]')) { items.push({ t: '', done: false }); dirty = true; sync(); render(); box.querySelectorAll('.t')[items.length - 1]?.focus(); }
      else if (e.target.matches('[data-save]') && store) {
        e.target.disabled = true; const msg = box.querySelector('[data-msg]'); msg.textContent = 'جارٍ الحفظ…';
        try { const r = await store.set(key, { items: items.filter(i => i.t.trim()) }, { version }); version = r.version; dirty = false; msg.textContent = 'تم الحفظ'; }
        catch (err) { msg.textContent = err.code === 'conflict' ? 'عدّل شخص آخر القائمة — أعد تحميل الصفحة' : err.code === 'sign_in_required' ? 'سجّل الدخول إلى جيم للتعديل' : 'تعذّر الحفظ: ' + err.message; e.target.disabled = false; }
      }
    });
    const sync = () => { if (!store) { slide.tasks = items; opts.onEdit && opts.onEdit(slide); } };
    render();
    if (store) (async () => {
      try { if (opts.viewer) { const v = await opts.viewer(); canEdit = !!v.canEdit; }
        const r = await store.getWithMeta(key); version = r.version || 0; if (r.value && r.value.items) items = r.value.items.map(i => ({ t: i.t || '', done: !!i.done })); render(); }
      catch (e) { render(); }
    })();
  }

  // متغيّرات الهوية على عنصر الشريحة/المصغّرة: الألوان والخط من spec.brand (هوية المشروع) أو spec.accent القديم.
  function brandVars(spec) {
    const b = (spec && spec.brand) || {}; const out = {};
    out['--jd-primary'] = b.primary || '#0B3D5C'; out['--jd-secondary'] = b.secondary || b.primary || '#0E8F86';
    out['--jd-accent'] = b.accent || b.primary || (spec && spec.accent) || '#2A78D6'; if (b.font) out['--jd-font'] = `'${b.font}'`;
    return out;
  }
  function applyBrand(root, spec) { const v = brandVars(spec); for (const k in v) root.style.setProperty(k, v[k]); if (!(spec && spec.brand && spec.brand.font)) root.style.removeProperty('--jd-font'); }
  function thumbnail(slide, index, spec) {
    const v = brandVars(spec); const st = Object.keys(v).map(k => `${k}:${v[k]}`).join(';');
    return `<div class="jd-thumb jd-${slide.layout || 'bullets'}" data-i="${index}" style="${st}"><div class="jd-tt">${esc(slide.title || '(بلا عنوان)')}</div><div class="jd-tk">${index + 1}</div></div>`;
  }

  // ---- العرض الكامل (الصفحة المنشورة) ----
  function present(root, spec, opts) {
    opts = opts || {}; ensureCss(root.ownerDocument);
    const slides = spec.slides || []; let i = 0;
    applyBrand(root, spec);
    root.innerHTML = `<div class="jd-present"><div class="jd-stage"><div class="jd-slide"></div></div><div class="jd-bar"><button data-prev>→ السابق</button><span class="jd-pos"></span><button data-print>طباعة / PDF</button><button data-next>التالي ←</button></div><div class="jd-strip"></div></div>`;
    const stage = root.querySelector('.jd-stage .jd-slide'), pos = root.querySelector('.jd-pos'), strip = root.querySelector('.jd-strip');
    strip.innerHTML = slides.map((s, k) => thumbnail(s, k, spec)).join('');
    const go = n => { i = Math.max(0, Math.min(slides.length - 1, n)); renderSlide(stage, slides[i], spec, { index: i, total: slides.length, editableStore: opts.editableStore, viewer: opts.viewer }); pos.textContent = `${i + 1} / ${slides.length}`; strip.querySelectorAll('.jd-thumb').forEach((t, k) => t.classList.toggle('active', k === i)); strip.querySelector('.jd-thumb.active')?.scrollIntoView({ inline: 'center', block: 'nearest' }); try { history.replaceState(null, '', '#' + (i + 1)); } catch (e) {} };
    root.querySelector('[data-prev]').onclick = () => go(i - 1); root.querySelector('[data-next]').onclick = () => go(i + 1);
    root.querySelector('[data-print]').onclick = () => { const all = root.ownerDocument.createElement('div'); all.className = 'jd-print'; slides.forEach((s, k) => { const d = root.ownerDocument.createElement('div'); all.appendChild(d); renderSlide(d, s, spec, { index: k, total: slides.length }); }); const st = root.querySelector('.jd-stage'); const keep = st.innerHTML; st.innerHTML = ''; st.appendChild(all); window.print(); setTimeout(() => { st.innerHTML = keep; go(i); }, 300); };
    strip.addEventListener('click', e => { const t = e.target.closest('.jd-thumb'); if (t) go(+t.dataset.i); });
    root.ownerDocument.addEventListener('keydown', e => { if (e.key === 'ArrowLeft' || e.key === 'PageDown' || e.key === ' ') go(i + 1); if (e.key === 'ArrowRight' || e.key === 'PageUp') go(i - 1); if (e.key === 'Home') go(0); if (e.key === 'End') go(slides.length - 1); });
    const h = parseInt((location.hash || '').slice(1), 10); go(isNaN(h) ? 0 : h - 1);
    return { go, get index() { return i; } };
  }

  window.JEEM_DECK = { renderSlide, thumbnail, present, chartSvg, CSS, brandVars };
})();
