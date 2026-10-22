/* ==========================================================
   جيم — مجمّعات السيولة المركّزة (لوحة تفاعلية، ثيم داكن محصور في .lp)
   الحركات: عدّ المؤشرات، أعمدة الحجم تنمو ثم تنتقل سلسًا عند تبديل الفترة،
   تمرير على الأعمدة يحدّث الرقم الرئيسي، صفوف تدخل متتابعة، نطاق السعر يظهر عند فتح الصف.
   ========================================================== */
(function () {
  const root = document.getElementById('lpRoot'); if (!root) return;
  const $ = (s, el = root) => el.querySelector(s), $$ = (s, el = root) => [...el.querySelectorAll(s)];
  const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
  const esc = s => String(s).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
  const RM = matchMedia('(prefers-reduced-motion: reduce)').matches;
  const usd = (v, d = 2) => '$' + Number(v).toLocaleString('en-US', { minimumFractionDigits: d, maximumFractionDigits: d });
  const short = v => v >= 1e9 ? '$' + (v / 1e9).toFixed(2) + 'B' : v >= 1e6 ? '$' + (v / 1e6).toFixed(2) + 'M' : '$' + Math.round(v / 1e3) + 'K';
  const count = (el, to, fmt, dur = 1000) => { if (!el) return; if (RM) { el.textContent = fmt(to); return; } const from = +el.dataset.v || 0, t0 = performance.now(); el.dataset.v = to; const st = t => { const p = Math.min(1, (t - t0) / dur), e = 1 - Math.pow(1 - p, 3); el.textContent = fmt(from + (to - from) * e); if (p < 1) requestAnimationFrame(st); }; requestAnimationFrame(st); };
  const toast = msg => { let t = document.querySelector('.toast'); if (!t) { t = document.createElement('div'); t.className = 'toast'; t.setAttribute('role', 'status'); document.body.append(t); } t.innerHTML = `${ic('check')}<span>${esc(msg)}</span>`; requestAnimationFrame(() => t.classList.add('is-on')); clearTimeout(t._h); t._h = setTimeout(() => t.classList.remove('is-on'), 2600); };
  let sd = 11; const rnd = () => (sd = (sd * 16807) % 2147483647) / 2147483647;

  /* ---------- البيانات التجريبية ---------- */
  const TOK = { USDC: 210, SUI: 190, USDT: 160, CETUS: 170, WETH: 230, WBTC: 30, DEEP: 270, NAVX: 300 };
  const POOLS = [
    ['USDC', 'SUI', .25, 'رائجة', true, 10684011.54, 10766872.37, 26917.18, 161.67, [3.62, 3.48, 3.80]],
    ['CETUS', 'SUI', .25, 'رائجة', true, 8420330.10, 6210448.90, 15526.12, 128.40, [0.071, 0.064, 0.082]],
    ['USDT', 'USDC', .01, 'مستقرة', false, 22451900.75, 18904221.30, 1890.42, 6.12, [1.0003, 0.9995, 1.0008]],
    ['WETH', 'USDC', .05, 'مستقرة', true, 6790110.33, 4120774.62, 2060.39, 24.85, [3420, 3180, 3650]],
    ['SUI', 'USDT', .25, 'رائجة', true, 5318420.05, 3904118.71, 9760.30, 88.20, [3.61, 3.70, 3.95]],
    ['WBTC', 'USDC', .05, 'مستقرة', false, 4402119.80, 2210945.12, 1105.47, 18.40, [64200, 61000, 66800]],
    ['DEEP', 'SUI', 1, 'جديدة', true, 1290442.67, 1840221.04, 18402.21, 312.75, [0.043, 0.047, 0.058]],
    ['NAVX', 'SUI', 1, 'جديدة', true, 880130.42, 640122.88, 6401.23, 204.10, [0.112, 0.095, 0.124]],
    ['USDC', 'USDT', .01, 'مستقرة', false, 310220.11, 82014.55, 8.20, 2.05, [1.0001, 0.9992, 1.0010]]
  ].map(([a, b, fee, lab, inc, liq, vol, fees, apr, rg], i) => ({ id: 'p' + i, a, b, fee, lab, inc, liq, vol, fees, apr, price: rg[0], lo: rg[1], hi: rg[2], small: liq < 500000 }));
  const POS = [{ pool: 'p0', amt: 12450, lo: 3.45, hi: 3.78, fees: 184.32 }, { pool: 'p4', amt: 4200, lo: 3.75, hi: 4.10, fees: 21.07 }];
  const PER = {
    day: { n: 36, unit: 'ساعة', vol: 24783752516.42, tvl: 153897416.49, cum: 7783752516.42, lab: i => `${String((i * 40 / 60 | 0) % 24).padStart(2, '0')}:${String(i * 40 % 60).padStart(2, '0')}` },
    week: { n: 36, unit: 'يوم', vol: 168204118902.10, tvl: 151204990.12, cum: 7783752516.42, lab: i => `يوم ${i + 1}` },
    month: { n: 36, unit: 'يوم', vol: 702118402557.80, tvl: 148990411.73, cum: 7783752516.42, lab: i => `${(i % 30) + 1} سبتمبر` }
  };
  const SERIES = {}; Object.keys(PER).forEach(k => { SERIES[k] = Array.from({ length: PER[k].n }, (_, i) => Math.round((.25 + rnd() * .75) * (k === 'day' ? 1 : k === 'week' ? 6.8 : 28) * 9e8 * (1 + Math.sin(i / 4) * .25))); });
  const S = { per: 'day', tab: 'pool', q: '', all: false, inc: 'all', lab: 'all', sort: 'vol', open: null, hover: null };
  let shown = SERIES.day.slice();

  /* ---------- 1) المؤشرات ---------- */
  function stats() {
    const P = PER[S.per];
    count($('#lpTvl'), P.tvl, v => usd(v)); count($('#lpCum'), P.cum, v => usd(v)); count($('#lpVol'), P.vol, v => usd(v));
    count($('#lpHead'), P.vol, v => usd(v)); $('#lpHeadL').textContent = { day: 'حجم التداول · آخر 24 ساعة', week: 'حجم التداول · آخر أسبوع', month: 'حجم التداول · آخر شهر' }[S.per];
  }

  /* ---------- 2) أعمدة الحجم (الأحدث يسارًا) ---------- */
  function bars(intro) {
    const box = $('#lpBars'), A = shown, mx = Math.max(...SERIES[S.per]) * 1.05;
    if (!box.children.length || intro) box.innerHTML = A.map((v, i) => `<button type="button" class="lp-b${intro ? ' in' : ''}" data-b="${i}" style="--i:${i}"><i></i></button>`).join('');
    $$('.lp-b', box).forEach((b, i) => { b.style.setProperty('--h', (A[i] / mx * 100).toFixed(1) + '%'); b.setAttribute('aria-label', `${PER[S.per].lab(i)}: ${short(A[i])}`); b.classList.toggle('on', S.hover === i); });
  }
  function tween() {
    const to = SERIES[S.per], from = shown.slice(), t0 = performance.now();
    if (RM) { shown = to.slice(); return bars(false); }
    const st = t => { const p = Math.min(1, (t - t0) / 600), e = 1 - Math.pow(1 - p, 3); shown = from.map((v, i) => v + (to[i] - v) * e); bars(false); if (p < 1) requestAnimationFrame(st); };
    requestAnimationFrame(st);
  }
  function hover(i) {
    S.hover = i; const tip = $('#lpTip');
    $$('.lp-b').forEach((b, j) => b.classList.toggle('on', j === i)); $('#lpBars').classList.toggle('has-on', i != null);
    if (i == null) { tip.hidden = true; $('#lpHead').textContent = usd(PER[S.per].vol); return; }
    const v = SERIES[S.per][i], b = $(`[data-b="${i}"]`), r = $('#lpChart').getBoundingClientRect(), rb = b.getBoundingClientRect();
    $('#lpHead').textContent = usd(v); tip.hidden = false; tip.innerHTML = `<b>${PER[S.per].lab(i)}</b><span class="num">${short(v)}</span>`;
    tip.style.left = Math.max(50, Math.min(r.width - 50, rb.left - r.left + rb.width / 2)) + 'px'; tip.style.top = (rb.top - r.top + rb.height * (1 - v / (Math.max(...SERIES[S.per]) * 1.05)) - 8) + 'px';
  }

  /* ---------- 3) جدول المجمّعات ---------- */
  const tk = t => `<span class="tk" style="--h:${TOK[t] || 200}"><b>${t.slice(0, 1)}</b></span>`;
  const pair = p => `<span class="pair">${tk(p.a)}${tk(p.b)}</span><span class="pn"><b>${p.a} – ${p.b}</b><small>${esc(p.lab)}</small></span><span class="fee num">${p.fee}%</span>`;
  const range = p => { const lo = Math.min(p.lo, p.price) * .97, hi = Math.max(p.hi, p.price) * 1.03, pos = v => ((v - lo) / (hi - lo) * 100).toFixed(1);
    return `<div class="lp-range"><span class="t">نطاق السيولة المركّزة</span><div class="rb"><i class="band" style="inset-inline-start:${pos(p.lo)}%;width:${(pos(p.hi) - pos(p.lo)).toFixed(1)}%"></i><i class="cur" style="inset-inline-start:${pos(p.price)}%"><em class="num">${p.price}</em></i></div>
      <div class="ends"><span>الحد الأدنى <b class="num">${p.lo}</b></span><span>السعر الحالي <b class="num">${p.price}</b> ${p.b}</span><span>الحد الأعلى <b class="num">${p.hi}</b></span></div></div>`; };
  function spark(p) { let s = p.id.charCodeAt(1) * 7; const r = () => (s = (s * 9301 + 49297) % 233280) / 233280; const v = Array.from({ length: 14 }, () => p.apr * (.7 + r() * .6)), mx = Math.max(...v), mn = Math.min(...v);
    return `<svg viewBox="0 0 140 40" class="sp"><polyline points="${v.map((y, i) => `${(140 - i * 140 / 13).toFixed(1)},${(36 - (y - mn) / (mx - mn || 1) * 32).toFixed(1)}`).join(' ')}"/></svg>`; }
  function table() {
    if (S.tab === 'pos') return positions();
    const q = S.q.toUpperCase();
    let L = POOLS.filter(p => (S.all || !p.small) && (!q || (p.a + p.b).includes(q)) && (S.inc === 'all' || (S.inc === 'yes') === p.inc) && (S.lab === 'all' || p.lab === S.lab));
    L.sort((a, b) => b[S.sort] - a[S.sort]);
    $('#lpCount').textContent = L.length;
    $('#lpTable').innerHTML = `<table><thead><tr><th>المجمّع</th>${[['liq', 'السيولة'], ['vol', 'الحجم (24 س)'], ['fees', 'الرسوم (24 س)']].map(([k, n]) => `<th><button type="button" data-sort="${k}" aria-sort="${S.sort === k ? 'descending' : 'none'}">${n}</button></th>`).join('')}<th>مكافآت التعدين</th><th><button type="button" data-sort="apr" aria-sort="${S.sort === 'apr' ? 'descending' : 'none'}">العائد السنوي (24 س)</button></th><th><span class="sr-only">إجراء</span></th></tr></thead>
      <tbody>${L.map((p, i) => `<tr class="row${S.open === p.id ? ' open' : ''}" style="--i:${i}" data-row="${p.id}" tabindex="0" aria-expanded="${S.open === p.id}"><td><div class="pc">${pair(p)}</div></td><td><span class="num">${usd(p.liq)}</span></td><td><span class="num">${usd(p.vol)}</span></td><td><span class="num">${usd(p.fees)}</span></td>
        <td>${p.inc ? `<span class="rw">${tk(p.b)}${tk('CETUS')}</span>` : '<span class="mut">—</span>'}</td><td><span class="apr num">${p.apr.toFixed(2)}%</span></td><td><button type="button" class="add" data-add="${p.id}" aria-label="أضف سيولة إلى ${p.a}–${p.b}">${ic('plus')}</button></td></tr>
        ${S.open === p.id ? `<tr class="ex"><td colspan="7"><div class="exb">${range(p)}<div class="ap"><span class="t">العائد السنوي · 14 يومًا</span>${spark(p)}</div><div class="ks"><span>نسبة الرسوم إلى السيولة <b class="num">${(p.fees / p.liq * 100).toFixed(3)}%</b></span><span>دوران السيولة <b class="num">${(p.vol / p.liq).toFixed(2)}×</b></span><button type="button" class="btn btn-sm lp-btn" data-add="${p.id}">${ic('plus')}إضافة سيولة</button></div></div></td></tr>` : ''}`).join('')}</tbody></table>
      ${L.length ? '' : '<p class="lp-none">لا مجمّعات تطابق التصفية.</p>'}`;
  }
  function positions() {
    $('#lpCount').textContent = POS.length;
    $('#lpTable').innerHTML = POS.length ? `<div class="lp-pos">${POS.map((x, i) => { const p = POOLS.find(q => q.id === x.pool), inR = p.price >= x.lo && p.price <= x.hi;
      return `<article style="--i:${i}"><header><div class="pc">${pair(p)}</div><span class="st ${inR ? 'in' : 'out'}">${ic(inR ? 'check' : 'info')}${inR ? 'داخل النطاق — يكسب رسومًا' : 'خارج النطاق — لا يكسب رسومًا'}</span></header>
        ${range(Object.assign({}, p, { lo: x.lo, hi: x.hi }))}<div class="ks"><span>القيمة <b class="num">${usd(x.amt)}</b></span><span>رسوم مكتسبة <b class="num">${usd(x.fees)}</b></span><span class="sp"></span><button type="button" class="btn btn-sm lp-btn" data-claim="${i}">${ic('download')}تحصيل الرسوم</button><button type="button" class="btn btn-sm lp-btn" data-rm="${i}">${ic('minus')}سحب</button></div></article>`; }).join('')}</div>`
      : '<p class="lp-none">لا مراكز لديك بعد. أضف سيولة إلى أي مجمّع.</p>';
  }

  /* ---------- 4) نافذة إضافة السيولة ---------- */
  const dlg = $('#lpDlg');
  function openAdd(id) {
    const p = POOLS.find(x => x.id === id);
    dlg.innerHTML = `<div class="lp-dlg" role="dialog" aria-modal="true" aria-labelledby="lpDT"><h3 id="lpDT">إضافة سيولة · ${p.a} – ${p.b}</h3>
      <label class="f"><span>المبلغ (دولار)</span><input type="number" id="lpAmt" min="100" step="100" value="1000"></label>
      <label class="f"><span>عرض النطاق حول السعر الحالي: <b class="num" id="lpWv">±5%</b></span><input type="range" id="lpW" min="1" max="30" value="5"></label>
      ${range(Object.assign({}, p, { lo: +(p.price * .95).toPrecision(4), hi: +(p.price * 1.05).toPrecision(4) })).replace('lp-range', 'lp-range lp-prev')}
      <p class="est">العائد التقديري: <b class="num" id="lpEst"></b> · <small>النطاق الأضيق يرفع العائد ويزيد احتمال الخروج منه</small></p>
      <div class="act"><button type="button" class="btn lp-btn" data-x>إلغاء</button><button type="button" class="btn btn-primary" data-ok="${id}">تأكيد الإضافة</button></div></div>`;
    dlg.hidden = false; const upd = () => { const w = +$('#lpW').value, p2 = Object.assign({}, p, { lo: +(p.price * (1 - w / 100)).toPrecision(4), hi: +(p.price * (1 + w / 100)).toPrecision(4) }); $('#lpWv').textContent = '±' + w + '%'; $('.lp-prev', dlg).outerHTML = range(p2).replace('lp-range', 'lp-range lp-prev'); $('#lpEst').textContent = (p.apr * Math.min(4, 5 / w)).toFixed(1) + '%'; };
    $('#lpW').addEventListener('input', upd); upd(); $('#lpAmt').focus();
  }
  const closeDlg = () => { dlg.hidden = true; dlg.innerHTML = ''; };

  /* ---------- الأحداث ---------- */
  root.addEventListener('click', e => {
    const t = e.target;
    const pr = t.closest('[data-per]'); if (pr) { S.per = pr.dataset.per; $$('[data-per]').forEach(b => b.setAttribute('aria-pressed', String(b === pr))); stats(); return tween(); }
    const tb = t.closest('[data-tab]'); if (tb) { S.tab = tb.dataset.tab; $$('[data-tab]').forEach(b => b.setAttribute('aria-selected', String(b === tb))); $('#lpTools').hidden = S.tab === 'pos'; return table(); }
    const so = t.closest('[data-sort]'); if (so) { S.sort = so.dataset.sort; $('#lpSort').value = S.sort; return table(); }
    const ad = t.closest('[data-add]'); if (ad) { e.stopPropagation(); return openAdd(ad.dataset.add); }
    const ok = t.closest('[data-ok]'); if (ok) { const amt = Math.max(100, +$('#lpAmt').value || 0), w = +$('#lpW').value, p = POOLS.find(x => x.id === ok.dataset.ok); POS.push({ pool: p.id, amt, lo: +(p.price * (1 - w / 100)).toPrecision(4), hi: +(p.price * (1 + w / 100)).toPrecision(4), fees: 0 }); p.liq += amt; closeDlg(); table(); return toast(`أُضيفت ${usd(amt, 0)} إلى ${p.a}–${p.b}. تجدها في «مراكزي».`); }
    if (t.closest('[data-x]') || t === dlg) return closeDlg();
    const cl = t.closest('[data-claim]'); if (cl) { const x = POS[+cl.dataset.claim]; toast(`حُصّلت رسوم ${usd(x.fees)}`); x.fees = 0; return table(); }
    const rm = t.closest('[data-rm]'); if (rm) { POS.splice(+rm.dataset.rm, 1); toast('سُحب المركز'); return table(); }
    if (t.closest('[data-create]')) return toast('هنا يُفتح معالج إنشاء مجمّع: الأصلان، شريحة الرسوم، والسعر الابتدائي.');
    if (t.closest('[data-addany]')) return openAdd(POOLS[0].id);
    const rw = t.closest('[data-row]'); if (rw) { S.open = S.open === rw.dataset.row ? null : rw.dataset.row; return table(); }
  });
  root.addEventListener('keydown', e => { const rw = e.target.closest('[data-row]'); if (rw && (e.key === 'Enter' || e.key === ' ') && e.target === rw) { e.preventDefault(); S.open = S.open === rw.dataset.row ? null : rw.dataset.row; table(); } });
  addEventListener('keydown', e => { if (e.key === 'Escape' && !dlg.hidden) closeDlg(); });
  $('#lpQ').addEventListener('input', e => { S.q = e.target.value.trim(); table(); });
  $('#lpAll').addEventListener('change', e => { S.all = e.target.checked; table(); });
  $('#lpInc').addEventListener('change', e => { S.inc = e.target.value; table(); });
  $('#lpLab').addEventListener('change', e => { S.lab = e.target.value; table(); });
  $('#lpSort').addEventListener('change', e => { S.sort = e.target.value; table(); });
  const bx = $('#lpBars');
  bx.addEventListener('pointerover', e => { const b = e.target.closest('[data-b]'); if (b) hover(+b.dataset.b); });
  bx.addEventListener('pointerleave', () => hover(null));
  bx.addEventListener('focusin', e => { const b = e.target.closest('[data-b]'); if (b) hover(+b.dataset.b); });
  bx.addEventListener('focusout', () => hover(null));


  /* ---------- الثيم: فاتح (افتراضي) أو داكن، ويُحفظ لكل مستخدم ---------- */
  (function () { let th = 'light'; try { th = localStorage.getItem('jeem.lp.theme') || 'light'; } catch (e) {}
    const set = t => { root.classList.toggle('lp-light', t === 'light'); $$('[data-theme-set]').forEach(b => b.setAttribute('aria-pressed', String(b.dataset.themeSet === t))); try { localStorage.setItem('jeem.lp.theme', t); } catch (e) {} };
    set(th); root.addEventListener('click', e => { const b = e.target.closest('[data-theme-set]'); if (b) set(b.dataset.themeSet); }); })();
  stats(); bars(true); table();
})();
