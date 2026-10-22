/* ==========================================================
   جيم — الإدارة المالية (لوحة تفاعلية)
   مستوحاة من «Financial Management Dashboard» (Finlytics): تدفق نقدي متباعد
   (داخل فوق / خارج تحت / الصافي نقطة)، متابعة أرباح وخسائر بخط تقاطع،
   خريطة مناطق بالتركيز، ومخطط عمليات مؤتمت بمسارات متحركة.
   ========================================================== */
(function () {
  const root = document.getElementById('foRoot'); if (!root || !window.DK) return;
  const K = DK, $ = s => root.querySelector(s), RM = K.RM;
  const C = { in: '#00A89F', out: '#D9822B', net: '#1B2430', rev: K.PAL[0], exp: K.PAL[1], pro: K.PAL[4] };
  const S = { cur: 'SAR', cf: 'd', pl: 'h', hide: new Set(), hl: null, focus: null };
  const fx = v => S.cur === 'SAR' ? v : v / 3.75, U = () => S.cur === 'SAR' ? 'ر.س' : 'دولار';
  const m = (v) => K.num(fx(v), U());
  const t = v => K.txt(fx(v), U());

  /* ---------- المؤشرات ---------- */
  const KP = [['rev', 'إجمالي الإيرادات', 'chart', 'التحليلات', 45672000, 5.0], ['exp', 'المصروفات', 'wallet', 'مدير التقارير', 28903000, -3.2], ['pro', 'صافي الربح', 'percent', 'قائمة الدخل', 34215000, 18.0]];
  function kpis() {
    $('#foKpis').innerHTML = KP.map(([k, n, i, l, v, d], j) => `<button type="button" class="dk-kpi fo-k" data-hl="${k}" aria-pressed="${S.hl === k}" style="--i:${j};--c:${C[k]}"><span class="row"><span class="ib">${K.ic(i)}</span><small>${n}</small><span class="sp"></span><span class="dk-chip">${l}${K.ic('link')}</span></span><span class="row"><b class="dk-count">${m(v)}</b>${K.delta(d, k === 'exp' ? -1 : 1)}</span></button>`).join('');
    K.count($('#foKpis'));
  }

  /* ---------- التدفق النقدي المتباعد ---------- */
  function cfData() {
    const n = S.cf === 'd' ? 31 : 12, r = K.rng('cf' + S.cf), sc = S.cf === 'd' ? 1 : S.cf === 'w' ? 6 : 26;
    let run = 120000 * sc; return Array.from({ length: n }, (_, i) => { const a = Math.round((40000 + r() * 50000) * sc), b = Math.round((25000 + r() * 45000) * sc); run += a - b; return { lab: S.cf === 'd' ? String(i + 1) : S.cf === 'w' ? 'أ' + (i + 1) : ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو', 'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر'][i], in: a, out: b, net: a - b, run }; });
  }
  function cf() {
    const d = cfData(), el = $('#foCf'), W = K.wOf(el), H = 240, top = 12, bot = H - 26, mid = (top + bot) / 2;
    const mx = K.niceMax(Math.max(...d.map(x => Math.max(x.in, x.out))) * 1.05), Y = v => mid - v / mx * (mid - top), n = d.length, R = W - 52, L = 8, step = (R - L) / n, bw = Math.min(22, step * .62);
    let s = `<svg viewBox="0 0 ${W} ${H}" class="dk-svg fo-cf" role="img" aria-label="التدفق النقدي">`;
    [mx, mx / 2, 0, -mx / 2, -mx].forEach(v => { s += `<line class="gr${v === 0 ? ' z' : ''}" x1="${L}" x2="${R + 4}" y1="${Y(v)}" y2="${Y(v)}"/><text class="yt" x="${W - 4}" y="${Y(v) + 4}">${K.esc(K.txt(fx(Math.abs(v))))}</text>`; });
    d.forEach((x, i) => { const cx = R - (i + .5) * step;
      s += `<g class="col" style="--i:${i}" data-tip="${K.esc(x.lab)}|${K.esc(`داخل ${t(x.in)} · خارج ${t(x.out)} · الصافي ${t(x.net)} · الرصيد ${t(x.run)}`)}"><rect class="hit" x="${cx - step / 2}" y="${top}" width="${step}" height="${bot - top}"/>`;
      if (!S.hide.has('in')) s += `<rect class="b" x="${cx - bw / 2}" y="${Y(x.in)}" width="${bw}" height="${mid - Y(x.in) - 1}" rx="3" fill="${C.in}"/>`;
      if (!S.hide.has('out')) s += `<rect class="b ng" x="${cx - bw / 2}" y="${mid + 1}" width="${bw}" height="${Y(-x.out) - mid - 1}" rx="3" fill="${C.out}"/>`;
      if (!S.hide.has('net')) s += `<circle class="nd" cx="${cx}" cy="${Y(x.net)}" r="3.5" fill="${C.net}"/>`;
      if (n <= 12 || i % 2 === 0) s += `<text class="xt" x="${cx}" y="${H - 8}">${K.esc(x.lab)}</text>`; s += '</g>'; });
    el.innerHTML = s + '</svg>';
    $('#foCfLeg').innerHTML = [['in', 'نقد داخل', C.in], ['out', 'نقد خارج', C.out], ['net', 'صافي النقد', C.net]].map(([k, n, c]) => `<button type="button" data-hide="${k}" aria-pressed="${!S.hide.has(k)}"><i style="background:${c}${k === 'net' ? ';border-radius:50%' : ''}"></i>${n}</button>`).join('') + `<span class="mut">الرصيد الجاري آخر الفترة: <b>${t(d[n - 1].run)}</b></span>`;
  }

  /* ---------- الأرباح والخسائر ---------- */
  function pl() {
    const cfg = { h: [13, i => `${10 + Math.floor(i / 2)}:${i % 2 ? '30' : '00'}`, 1], d: [30, i => String(i + 1), 24], w: [12, i => 'أ' + (i + 1), 160], m: [12, i => ['ينا', 'فبر', 'مار', 'أبر', 'ماي', 'يون', 'يول', 'أغس', 'سبت', 'أكت', 'نوف', 'ديس'][i], 700] }[S.pl];
    const [n, lab, sc] = cfg, r = K.rng('pl' + S.pl); let a = 120, b = 95; const rev = [], exp = [];
    for (let i = 0; i < n; i++) { a += 5 + r() * 18 - (i === 5 ? 20 : 0); b += 3 + r() * 13; rev.push(Math.round(a * sc * 1000)); exp.push(Math.round(b * sc * 1000)); }
    const pro = rev.map((v, i) => v - exp[i]);
    const ser = [['rev', 'إجمالي الدخل', rev], ['exp', 'المصروفات', exp], ['pro', 'صافي الربح', pro]].map(([k, nm, v]) => ({ name: nm, color: C[k], vals: v.map(fx), area: k === 'pro', fade: S.hl && S.hl !== k }));
    $('#foPl').innerHTML = K.line({ W: K.wOf($('#foPl')), labels: Array.from({ length: n }, (_, i) => lab(i)), series: ser, H: 230, fmt: v => K.txt(v, U()), aria: 'الأرباح والخسائر' });
    $('#foPlLeg').innerHTML = K.legend([['إجمالي الدخل', C.rev], ['المصروفات', C.exp], ['صافي الربح', C.pro]]);
  }

  /* ---------- خريطة المناطق (شبكة مبسطة بمواقع جغرافية تقريبية) ---------- */
  const RG = [['أمريكا الشمالية', 1, 1, 2, 8.4, 4.1], ['أوروبا', 3, 1, 1, 12.6, 6.8], ['آسيا', 4, 1, 2, 15.2, 9.3], ['الشرق الأوسط', 4, 2, 1, 9.7, 12.4], ['أفريقيا', 3, 2, 1, 4.1, -2.6], ['أمريكا الجنوبية', 2, 3, 1, 3.3, 1.8], ['أوقيانوسيا', 5, 3, 1, 2.2, -0.9]];
  function map() {
    $('#foMap').innerHTML = RG.map(([n, c, rw, sp, v, d], i) => `<button type="button" class="tl${S.focus === i ? ' on' : ''}${S.focus != null && S.focus !== i ? ' dim' : ''}" data-rg="${i}" style="grid-column:${c} / span ${sp};grid-row:${rw};--i:${i};--a:${(.25 + v / 16 * .75).toFixed(2)}" aria-pressed="${S.focus === i}"><b>${n}</b><span>${m(v * 1e6)}</span></button>`).join('');
    const f = S.focus; $('#foFocus').hidden = f == null;
    $('#foMapInfo').innerHTML = f == null ? `<span class="mut">اضغط منطقة للتركيز على مخزونها</span>` : `<b>${RG[f][0]}</b> · قيمة المخزون ${m(RG[f][4] * 1e6)} · التغير ${K.delta(RG[f][5], 1)} · <span class="num">${Math.round(RG[f][4] * 37)}</span> صنفًا نشطًا`;
  }

  /* ---------- مدير العمليات ---------- */
  const LISTS = { ins: [['تغطية الأتمتة', 'نسبة العمليات المؤتمتة بالكامل', 72], ['مسار الأعمال – أ', 'تنبيه عند انخفاض الهامش', 54], ['مطابقة الفواتير', 'مطابقة آلية مع أوامر الشراء', 88]], exc: [['تغطية الأتمتة', 'استثناءات قيد المعالجة', 31], ['مسار الأعمال – أ', 'تنبيه عند تجاوز الحد', 47], ['مدفوعات متأخرة', 'تصعيد بعد 15 يومًا', 19]] };
  const Q = { ins: '', exc: '' };
  function lists() {
    ['ins', 'exc'].forEach(k => { const q = Q[k].trim(); $('#fo' + (k === 'ins' ? 'Ins' : 'Exc')).innerHTML = LISTS[k].map((x, i) => [x, i]).filter(([x]) => !q || x[0].includes(q) || x[1].includes(q)).map(([[a, b, p], i]) => `<li style="--i:${i}"><div><b>${K.esc(a)}</b><small>${K.esc(b)} · <span class="num">${p}%</span></small><span class="dk-prog"><i style="width:${p}%;background:${k === 'ins' ? K.PAL[0] : K.PAL[1]}"></i></span></div><button type="button" data-del="${k}:${i}" aria-label="حذف ${K.esc(a)}">${K.ic('x')}</button></li>`).join('') || '<li class="dk-empty">لا نتائج</li>'; });
  }
  const NODES = [['بيانات العملاء', 'database', .88, .5], ['تغطية الأتمتة', 'spark', .66, .2], ['ضبط الاستثناءات', 'shield', .66, .8], ['تحسين الأداء', 'chart', .38, .35], ['رضا العملاء', 'users', .14, .6]];
  const EDGES = [[0, 1], [0, 2], [1, 3], [2, 3], [3, 4], [2, 4]];
  let selN = null;
  function flow() {
    const el = $('#foFlow'), W = Math.max(280, el.clientWidth || 400), H = 250, P = NODES.map(([, , x, y]) => [x * W, 24 + y * (H - 48)]);
    $('#foFlow').innerHTML = `<svg viewBox="0 0 ${W} ${H}" class="dk-svg fo-fl" role="img" aria-label="مخطط العمليات">${EDGES.map(([a, b], i) => { const [x1, y1] = P[a], [x2, y2] = P[b], mx = (x1 + x2) / 2; return `<path class="ed${selN != null && (a === selN || b === selN) ? ' on' : ''}${selN != null && a !== selN && b !== selN ? ' dim' : ''}" d="M${x1},${y1} C${mx},${y1} ${mx},${y2} ${x2},${y2}" style="--i:${i}"/>`; }).join('')}
      ${NODES.map(([n, ic], i) => `<g class="nd${selN === i ? ' on' : ''}" data-node="${i}" tabindex="0" role="button" aria-label="${n}" transform="translate(${P[i][0]},${P[i][1]})"><rect x="-62" y="-17" width="124" height="34" rx="10"/><text x="0" y="5">${n}</text></g>`).join('')}</svg>
      <p class="fo-fi" aria-live="polite">${selN == null ? 'اضغط مرحلة لعرض مسارها' : `<b>${NODES[selN][0]}</b> · كفاءة <span class="num">${[96, 72, 64, 81, 88][selN]}%</span> · ${EDGES.filter(e => e.includes(selN)).length} مسارات مرتبطة`}</p>`;
  }

  function all() { kpis(); cf(); pl(); map(); lists(); flow(); }
  root.addEventListener('click', e => {
    const tg = e.target;
    const h = tg.closest('[data-hl]'); if (h) { S.hl = S.hl === h.dataset.hl ? null : h.dataset.hl; kpis(); return pl(); }
    const hd = tg.closest('[data-hide]'); if (hd) { const k = hd.dataset.hide; S.hide.has(k) ? S.hide.delete(k) : S.hide.add(k); return cf(); }
    const rg = tg.closest('[data-rg]'); if (rg) { const i = +rg.dataset.rg; S.focus = S.focus === i ? null : i; return map(); }
    if (tg.closest('#foFocus')) { S.focus = null; return map(); }
    const nd = tg.closest('[data-node]'); if (nd) { const i = +nd.dataset.node; selN = selN === i ? null : i; return flow(); }
    const dl = tg.closest('[data-del]'); if (dl) { const [k, i] = dl.dataset.del.split(':'); LISTS[k].splice(+i, 1); return lists(); }
    const ad = tg.closest('[data-add]'); if (ad) { const k = ad.dataset.add; LISTS[k].unshift(['قاعدة أتمتة جديدة', 'بانتظار الإعداد', 0]); K.toast('أُضيفت قاعدة جديدة'); return lists(); }
    if (tg.closest('[data-export]')) return K.toast('يُصدَّر التقرير المالي (Excel/PDF) بالعملة والفترة الحالية');
    if (tg.closest('[data-print]')) return K.toast('جاهز للطباعة');
  });
  root.addEventListener('input', e => { const q = e.target.closest('[data-q]'); if (q) { Q[q.dataset.q] = q.value; lists(); } });
  $('#foCur').addEventListener('change', e => { S.cur = e.target.value; kpis(); cf(); pl(); map(); });
  K.seg(root, 'cf', v => { S.cf = v; cf(); });
  K.seg(root, 'pl', v => { S.pl = v; pl(); });
  const hr = 9; $('#foHi').textContent = hr < 12 ? 'صباح الخير، سارة' : 'مساء الخير، سارة';
  K.onResize(root, () => { cf(); pl(); flow(); });
  all();
})();
