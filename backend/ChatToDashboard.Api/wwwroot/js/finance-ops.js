/* ==========================================================
   جيم — الإدارة المالية (لوحة تفاعلية، المحرك: finance، على dash-kit)
   دالة تركيب قابلة لإعادة الاستخدام (window.JEEM_FINANCE_MOUNT). عقد البيانات المتوقَّع:
   data.kpis.{rev,exp,pro}: {value, deltaPct} · data.cashFlow.{d,w,m}: {opening, points:[{in,out}]}
   بعدد نقاط 31/12/12 · data.profitLoss.{h,d,w,m}: {revenue:[], expenses:[]} بعدد نقاط
   13/30/12/12 · data.regions: [{name, gridCol, gridRow, gridSpan, inventoryValue, deltaPct,
   activeItems}] · data.automationInsights/automationExceptions: [{title, desc, pct}].
   التحويل بين ر.س والدولار يستخدم ربط الريال الرسمي الثابت (3.75) لا رقمًا وهميًا. تحية
   الترحيب كانت "سارة" ثابتة بساعة مزيّفة (hr=9) — أصبحت تأخذ اسم المستخدم الحقيقي الحالي
   (opts.userName) والساعة الفعلية. تاريخ التقرير كان نصًا ثابتًا (18/01/2026) — أصبح تاريخ
   اليوم الفعلي. "عدد الأصناف النشطة" لكل منطقة كان يُحتسب بصيغة وهمية (القيمة × 37) — أصبح
   حقلًا حقيقيًا (activeItems) من البيانات. بنية مخطط العمليات (5 مراحل وروابطها ومواقعها)
   بقيت ثابتة في المحرك — تصميم عملية عامة لا بيانات أعمال مُولَّدة — لكن نسب كفاءة كل مرحلة
   أصبحت من data.operationsFlowEfficiency بدل أرقام ثابتة.
   ========================================================== */
(function () {
  const KP = [
    ['rev', 'إجمالي الإيرادات', 'chart', 'التحليلات'], ['exp', 'المصروفات', 'wallet', 'مدير التقارير'],
    ['pro', 'صافي الربح', 'percent', 'قائمة الدخل'],
  ];
  const ARABIC_MONTHS_SHORT = ['ينا', 'فبر', 'مار', 'أبر', 'ماي', 'يون', 'يول', 'أغس', 'سبت', 'أكت', 'نوف', 'ديس'];
  const ARABIC_MONTHS_FULL = ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو', 'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر'];
  const NODES = [['بيانات العملاء', 'database', .88, .5], ['تغطية الأتمتة', 'spark', .66, .2], ['ضبط الاستثناءات', 'shield', .66, .8], ['تحسين الأداء', 'chart', .38, .35], ['رضا العملاء', 'users', .14, .6]];
  const EDGES = [[0, 1], [0, 2], [1, 3], [2, 3], [3, 4], [2, 4]];
  const CF_N = { d: 31, w: 12, m: 12 };
  const PL_N = { h: 13, d: 30, w: 12, m: 12 };

  function mount(root, data, opts = {}) {
    if (!root || !data || !window.DK) return;
    const K = DK, $ = s => root.querySelector(s);
    const C = { in: '#00A89F', out: '#D9822B', net: '#1B2430', rev: K.PAL[0], exp: K.PAL[1], pro: K.PAL[4] };
    const S = { cur: 'SAR', cf: 'd', pl: 'h', hide: new Set(), hl: null, focus: null };
    const fx = v => S.cur === 'SAR' ? v : v / 3.75, U = () => S.cur === 'SAR' ? 'ر.س' : 'دولار';
    const m = v => K.num(fx(v), U());
    const t = v => K.txt(fx(v), U());
    const REG = data.regions && data.regions.length ? data.regions : [];
    const LISTS = { ins: (data.automationInsights || []).map(x => [x.title, x.desc, x.pct]), exc: (data.automationExceptions || []).map(x => [x.title, x.desc, x.pct]) };
    const Q = { ins: '', exc: '' };
    let selN = null;

    function kpis() {
      $('#foKpis').innerHTML = KP.map(([k, n, i, l], j) => {
        const kp = (data.kpis && data.kpis[k]) || { value: 0, deltaPct: 0 };
        return `<button type="button" class="dk-kpi fo-k" data-hl="${k}" aria-pressed="${S.hl === k}" style="--i:${j};--c:${C[k]}"><span class="row"><span class="ib">${K.ic(i)}</span><small>${n}</small><span class="sp"></span><span class="dk-chip">${l}${K.ic('link')}</span></span><span class="row"><b class="dk-count">${m(kp.value || 0)}</b>${K.delta(kp.deltaPct || 0, k === 'exp' ? -1 : 1)}</span></button>`;
      }).join('');
      K.count($('#foKpis'));
    }

    /* ---------- التدفق النقدي المتباعد ---------- */
    function cfPoints() {
      const src = (data.cashFlow && data.cashFlow[S.cf]) || { opening: 0, points: [] };
      const pts = src.points.length ? src.points : Array.from({ length: CF_N[S.cf] }, () => ({ in: 0, out: 0 }));
      let run = src.opening || 0;
      return pts.map((p, i) => {
        run += (p.in || 0) - (p.out || 0);
        const lab = S.cf === 'd' ? String(i + 1) : S.cf === 'w' ? 'أ' + (i + 1) : ARABIC_MONTHS_FULL[i % 12];
        return { lab, in: p.in || 0, out: p.out || 0, net: (p.in || 0) - (p.out || 0), run };
      });
    }
    function cf() {
      const d = cfPoints(), el = $('#foCf'), W = K.wOf(el), H = 240, top = 12, bot = H - 26, mid = (top + bot) / 2;
      const mx = K.niceMax(Math.max(1, ...d.map(x => Math.max(x.in, x.out))) * 1.05), Y = v => mid - v / mx * (mid - top), n = d.length, R = W - 52, L = 8, step = (R - L) / n, bw = Math.min(22, step * .62);
      let s = `<svg viewBox="0 0 ${W} ${H}" class="dk-svg fo-cf" role="img" aria-label="التدفق النقدي">`;
      [mx, mx / 2, 0, -mx / 2, -mx].forEach(v => { s += `<line class="gr${v === 0 ? ' z' : ''}" x1="${L}" x2="${R + 4}" y1="${Y(v)}" y2="${Y(v)}"/><text class="yt" x="${W - 4}" y="${Y(v) + 4}">${K.esc(K.txt(fx(Math.abs(v))))}</text>`; });
      d.forEach((x, i) => { const cx = R - (i + .5) * step;
        s += `<g class="col" style="--i:${i}" data-tip="${K.esc(x.lab)}|${K.esc(`داخل ${t(x.in)} · خارج ${t(x.out)} · الصافي ${t(x.net)} · الرصيد ${t(x.run)}`)}"><rect class="hit" x="${cx - step / 2}" y="${top}" width="${step}" height="${bot - top}"/>`;
        if (!S.hide.has('in')) s += `<rect class="b" x="${cx - bw / 2}" y="${Y(x.in)}" width="${bw}" height="${mid - Y(x.in) - 1}" rx="3" fill="${C.in}"/>`;
        if (!S.hide.has('out')) s += `<rect class="b ng" x="${cx - bw / 2}" y="${mid + 1}" width="${bw}" height="${Y(-x.out) - mid - 1}" rx="3" fill="${C.out}"/>`;
        if (!S.hide.has('net')) s += `<circle class="nd" cx="${cx}" cy="${Y(x.net)}" r="3.5" fill="${C.net}"/>`;
        if (n <= 12 || i % 2 === 0) s += `<text class="xt" x="${cx}" y="${H - 8}">${K.esc(x.lab)}</text>`; s += '</g>'; });
      el.innerHTML = s + '</svg>';
      $('#foCfLeg').innerHTML = [['in', 'نقد داخل', C.in], ['out', 'نقد خارج', C.out], ['net', 'صافي النقد', C.net]].map(([k, n, c]) => `<button type="button" data-hide="${k}" aria-pressed="${!S.hide.has(k)}"><i style="background:${c}${k === 'net' ? ';border-radius:50%' : ''}"></i>${n}</button>`).join('') + `<span class="mut">الرصيد الجاري آخر الفترة: <b>${t(d.length ? d[d.length - 1].run : 0)}</b></span>`;
    }

    /* ---------- الأرباح والخسائر ---------- */
    function plLabel(i) {
      if (S.pl === 'h') return `${10 + Math.floor(i / 2)}:${i % 2 ? '30' : '00'}`;
      if (S.pl === 'd') return String(i + 1);
      if (S.pl === 'w') return 'أ' + (i + 1);
      return ARABIC_MONTHS_SHORT[i % 12];
    }
    function pl() {
      const src = (data.profitLoss && data.profitLoss[S.pl]) || {};
      const n = PL_N[S.pl], rev = (src.revenue && src.revenue.length ? src.revenue : Array(n).fill(0));
      const exp = (src.expenses && src.expenses.length ? src.expenses : Array(n).fill(0));
      const pro = rev.map((v, i) => v - (exp[i] || 0));
      const ser = [['rev', 'إجمالي الدخل', rev], ['exp', 'المصروفات', exp], ['pro', 'صافي الربح', pro]].map(([k, nm, v]) => ({ name: nm, color: C[k], vals: v.map(fx), area: k === 'pro', fade: S.hl && S.hl !== k }));
      $('#foPl').innerHTML = K.line({ W: K.wOf($('#foPl')), labels: Array.from({ length: n }, (_, i) => plLabel(i)), series: ser, H: 230, fmt: v => K.txt(v, U()), aria: 'الأرباح والخسائر' });
      $('#foPlLeg').innerHTML = K.legend([['إجمالي الدخل', C.rev], ['المصروفات', C.exp], ['صافي الربح', C.pro]]);
    }

    /* ---------- خريطة المناطق ---------- */
    function map() {
      $('#foMap').innerHTML = REG.map((r, i) => `<button type="button" class="tl${S.focus === i ? ' on' : ''}${S.focus != null && S.focus !== i ? ' dim' : ''}" data-rg="${i}" style="grid-column:${r.gridCol || 1} / span ${r.gridSpan || 1};grid-row:${r.gridRow || 1};--i:${i};--a:${(.25 + Math.min(1, (r.inventoryValue || 0) / 16e6) * .75).toFixed(2)}" aria-pressed="${S.focus === i}"><b>${K.esc(r.name)}</b><span>${m(r.inventoryValue || 0)}</span></button>`).join('');
      const f = S.focus; $('#foFocus').hidden = f == null;
      $('#foMapInfo').innerHTML = f == null ? `<span class="mut">اضغط منطقة للتركيز على مخزونها</span>` : `<b>${K.esc(REG[f].name)}</b> · قيمة المخزون ${m(REG[f].inventoryValue || 0)} · التغير ${K.delta(REG[f].deltaPct || 0, 1)} · <span class="num">${K.nf(REG[f].activeItems || 0)}</span> صنفًا نشطًا`;
    }

    /* ---------- مدير العمليات ---------- */
    function lists() {
      ['ins', 'exc'].forEach(k => { const q = Q[k].trim(); $('#fo' + (k === 'ins' ? 'Ins' : 'Exc')).innerHTML = LISTS[k].map((x, i) => [x, i]).filter(([x]) => !q || x[0].includes(q) || x[1].includes(q)).map(([[a, b, p], i]) => `<li style="--i:${i}"><div><b>${K.esc(a)}</b><small>${K.esc(b)} · <span class="num">${p}%</span></small><span class="dk-prog"><i style="width:${p}%;background:${k === 'ins' ? K.PAL[0] : K.PAL[1]}"></i></span></div><button type="button" data-del="${k}:${i}" aria-label="حذف ${K.esc(a)}">${K.ic('x')}</button></li>`).join('') || '<li class="dk-empty">لا نتائج</li>'; });
    }
    function flow() {
      const eff = data.operationsFlowEfficiency && data.operationsFlowEfficiency.length === 5 ? data.operationsFlowEfficiency : [0, 0, 0, 0, 0];
      const el = $('#foFlow'), W = Math.max(280, el.clientWidth || 400), H = 250, P = NODES.map(([, , x, y]) => [x * W, 24 + y * (H - 48)]);
      $('#foFlow').innerHTML = `<svg viewBox="0 0 ${W} ${H}" class="dk-svg fo-fl" role="img" aria-label="مخطط العمليات">${EDGES.map(([a, b], i) => { const [x1, y1] = P[a], [x2, y2] = P[b], mx = (x1 + x2) / 2; return `<path class="ed${selN != null && (a === selN || b === selN) ? ' on' : ''}${selN != null && a !== selN && b !== selN ? ' dim' : ''}" d="M${x1},${y1} C${mx},${y1} ${mx},${y2} ${x2},${y2}" style="--i:${i}"/>`; }).join('')}
        ${NODES.map(([n], i) => `<g class="nd${selN === i ? ' on' : ''}" data-node="${i}" tabindex="0" role="button" aria-label="${n}" transform="translate(${P[i][0]},${P[i][1]})"><rect x="-62" y="-17" width="124" height="34" rx="10"/><text x="0" y="5">${n}</text></g>`).join('')}</svg>
        <p class="fo-fi" aria-live="polite">${selN == null ? 'اضغط مرحلة لعرض مسارها' : `<b>${NODES[selN][0]}</b> · كفاءة <span class="num">${K.nf(eff[selN], 0)}%</span> · ${EDGES.filter(e => e.includes(selN)).length} مسارات مرتبطة`}</p>`;
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
    const hr = new Date().getHours(), name = opts.userName || '';
    $('#foHi').textContent = (hr < 12 ? 'صباح الخير' : 'مساء الخير') + (name ? `، ${name}` : '');
    const dateEl = $('#foDate b'); if (dateEl) { const now = new Date(); dateEl.textContent = `${String(now.getDate()).padStart(2, '0')}/${String(now.getMonth() + 1).padStart(2, '0')}/${now.getFullYear()}`; }
    K.onResize(root, () => { cf(); pl(); flow(); });
    all();
    return { destroy() {} };
  }

  window.JEEM_FINANCE_MOUNT = mount;
})();
