/* ==========================================================
   جيم — إدارة العقارات (لوحة تفاعلية متحركة)
   الحركات: عدّ المؤشرات، أعمدة دخل/مصروفات تنمو من المحور، أشرطة أهداف تمتلئ،
   صفوف المعاملات تدخل متتابعة، فرز وتصفية ونسخ المعرّف.
   ========================================================== */
(function () {
  const root = document.getElementById('reRoot'); if (!root) return;
  const $ = (s, el = root) => el.querySelector(s), $$ = (s, el = root) => [...el.querySelectorAll(s)];
  const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
  const nf = (v, d = 0) => Number(v).toLocaleString('en-US', { maximumFractionDigits: d, minimumFractionDigits: d });
  const RM = matchMedia('(prefers-reduced-motion: reduce)').matches;
  const count = (el, to, fmt, dur = 1000) => { if (!el) return; if (RM) { el.textContent = fmt(to); return; } const from = +el.dataset.v || 0, t0 = performance.now(); el.dataset.v = to; const st = t => { const p = Math.min(1, (t - t0) / dur), e = 1 - Math.pow(1 - p, 3); el.textContent = fmt(from + (to - from) * e); if (p < 1) requestAnimationFrame(st); }; requestAnimationFrame(st); };
  const toast = msg => { let t = document.querySelector('.toast'); if (!t) { t = document.createElement('div'); t.className = 'toast'; t.setAttribute('role', 'status'); document.body.append(t); } t.innerHTML = `${ic('check')}<span>${msg}</span>`; requestAnimationFrame(() => t.classList.add('is-on')); clearTimeout(t._h); t._h = setTimeout(() => t.classList.remove('is-on'), 2400); };
  const MON = ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو', 'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر'];
  const S = { year: '2026', hide: new Set(), sort: ['date', -1], mon: 'all', st: 'all', q: '', sel: new Set() };

  /* ---------- 1) المؤشرات ---------- */
  const KPIS = [
    { k: 'p', ic: 'home', n: 'عدد العقارات', v: { 2026: 5467, 2025: 5648 }, d: { 2026: -3.22, 2025: 4.1 }, f: v => nf(v) },
    { k: 'a', ic: 'briefcase', n: 'الوسطاء المسجلون', v: { 2026: 607, 2025: 575 }, d: { 2026: 5.54, 2025: 2.8 }, f: v => nf(v) },
    { k: 'c', ic: 'users', n: 'العملاء', v: { 2026: 6754, 2025: 6915 }, d: { 2026: -2.33, 2025: 6.2 }, f: v => nf(v) },
    { k: 'r', ic: 'wallet', n: 'الإيرادات', v: { 2026: 14.2, 2025: 13.6 }, d: { 2026: 4.25, 2025: 3.1 }, f: v => nf(v, 1), u: 'مليون ر.س' }
  ];
  const SP = [[4, 6, 5, 7, 6, 8, 7], [3, 4, 4, 5, 6, 6, 7], [5, 5, 6, 5, 6, 6, 7], [3, 5, 4, 6, 7, 6, 8]];
  function kpis() {
    $('#reKpis').innerHTML = KPIS.map((k, i) => { const d = k.d[S.year], up = d >= 0, sp = up ? SP[i] : [...SP[i]].reverse(), mx = Math.max(...sp), mn = Math.min(...sp);
      const pts = sp.map((v, j) => `${(100 - j * 100 / 6).toFixed(1)},${(30 - (v - mn) / (mx - mn || 1) * 26 - 2).toFixed(1)}`).join(' ');
      return `<div class="re-kpi" style="--i:${i}"><span class="ib">${ic(k.ic, 'icon')}</span><div class="tx"><span class="lb">${k.n}</span><span class="v"><b class="num" data-k="${i}">0</b>${k.u ? `<small>${k.u}</small>` : ''}</span><span class="dl ${up ? 'up' : 'dn'}">${ic(up ? 'up' : 'down')}<span class="num">${up ? '+' : ''}${d}%</span><small>عن العام الماضي</small></span></div>
        <svg class="sp" viewBox="0 0 100 30" preserveAspectRatio="none" aria-hidden="true"><polyline points="${pts}" pathLength="1"/></svg></div>`; }).join('');
    KPIS.forEach((k, i) => count($(`[data-k="${i}"]`), k.v[S.year], k.f));
  }

  /* ---------- 2) تحليل الدخل: أعمدة متباعدة (دخل للأعلى، مصروفات للأسفل) ---------- */
  const INC = { 2026: [62, 74, 58, 81, 95, 88, 102, 97, 84, 110, 0, 0], 2025: [55, 60, 52, 70, 78, 74, 86, 90, 76, 92, 98, 104] };
  const EXP = { 2026: [38, 42, 35, 47, 51, 44, 58, 55, 49, 61, 0, 0], 2025: [34, 39, 31, 42, 46, 40, 49, 52, 45, 50, 56, 60] };
  function income() {
    const I = INC[S.year], E = EXP[S.year], mx = 120, n = I.filter(v => v).length;
    const ti = I.reduce((a, b) => a + b, 0), te = E.reduce((a, b) => a + b, 0);
    $('#reIncSum').innerHTML = `<span><i class="sw inc"></i>الدخل<b class="num">${nf(ti / 10, 1)}</b><small>مليون ر.س</small></span><span><i class="sw exp"></i>المصروفات<b class="num">${nf(te / 10, 1)}</b><small>مليون ر.س</small></span><span>صافي الربح<b class="num">${nf((ti - te) / 10, 1)}</b><small>مليون ر.س</small></span>`;
    $('#reInc').innerHTML = `<div class="re-axis"><span class="num">120</span><span class="num">60</span><span class="num">0</span><span class="num">60</span><span class="num">120</span></div><div class="re-bars">${MON.map((m, i) => {
      const fut = !I[i]; return `<button type="button" class="re-mo${fut ? ' fut' : ''}" data-mo="${i}" style="--i:${i}" ${fut ? 'disabled' : ''} aria-label="${m}: دخل ${nf(I[i] * 10)} ألف، مصروفات ${nf(E[i] * 10)} ألف">
        <span class="up">${S.hide.has('inc') ? '' : `<i style="height:${I[i] / mx * 100}%"></i>`}</span><span class="dn">${S.hide.has('exp') ? '' : `<i style="height:${E[i] / mx * 100}%"></i>`}</span><small>${m}</small></button>`; }).join('')}</div><div class="re-tip" hidden></div>`;
    $('#reIncNote').textContent = S.year === '2026' ? `حتى ${MON[n - 1]} ${S.year}` : `السنة كاملة ${S.year}`;
  }
  function incTip(btn) {
    const tip = $('#reInc .re-tip'); if (!btn) { tip.hidden = true; $$('.re-mo').forEach(b => b.classList.remove('hl')); return; }
    const i = +btn.dataset.mo, I = INC[S.year][i], E = EXP[S.year][i]; $$('.re-mo').forEach(b => b.classList.toggle('hl', b === btn));
    tip.hidden = false; tip.innerHTML = `<b>${MON[i]} ${S.year}</b><span><i class="sw inc"></i>الدخل<b class="num">${nf(I * 10)}</b> ألف</span><span><i class="sw exp"></i>المصروفات<b class="num">${nf(E * 10)}</b> ألف</span><span>الصافي<b class="num">${nf((I - E) * 10)}</b> ألف</span>`;
    const r = $('#reInc').getBoundingClientRect(), b = btn.getBoundingClientRect(); tip.style.left = Math.max(70, Math.min(r.width - 70, b.left - r.left + b.width / 2)) + 'px';
  }

  /* ---------- 3) رصيدي وأهدافي ---------- */
  function balance() {
    const ti = INC[S.year].reduce((a, b) => a + b, 0) * 10, te = EXP[S.year].reduce((a, b) => a + b, 0) * 10;
    const G = S.year === '2026' ? [['العقارات', 72, '5,467 من 7,600'], ['الإيرادات', 58, '14.2 من 24.5 مليون'], ['العملاء', 84, '6,754 من 8,000']] : [['العقارات', 91, '5,648 من 6,200'], ['الإيرادات', 77, '13.6 من 17.6 مليون'], ['العملاء', 96, '6,915 من 7,200']];
    $('#reBal').innerHTML = `<div class="re-card-bal"><span class="lb">الرصيد المتاح</span><span class="v"><b class="num" id="reBalV">0</b><small>ر.س</small></span>
      <div class="io"><span class="in">${ic('up')}<span><small>الدخل</small><b class="num">${nf(ti)}</b> ألف</span></span><span class="out">${ic('down')}<span><small>المصروفات</small><b class="num">${nf(te)}</b> ألف</span></span></div></div>
      <h3 class="re-gh">أهداف ${S.year}</h3><div class="re-goals">${G.map(([n, p, s], i) => `<div class="re-goal" style="--p:${p}%;--i:${i}"><span class="t"><b>${n}</b><span class="num">${p}%</span></span><span class="bar"><i></i></span><small>${s}</small></div>`).join('')}</div>`;
    count($('#reBalV'), (ti - te) * 1000 * .37, v => nf(v), 1300);
  }

  /* ---------- 4) سجل المعاملات الأخيرة ---------- */
  const NAMES = ['خالد العتيبي', 'نورة القحطاني', 'فهد الدوسري', 'سارة الشهري', 'عبدالله الزهراني', 'ريم المطيري', 'ماجد الغامدي', 'هيفاء الحربي', 'سلطان الشمري', 'لمى العنزي', 'تركي السبيعي', 'جواهر البقمي'];
  const ADDR = ['حي الملقا، الرياض', 'حي الشاطئ، جدة', 'حي الفيصلية، الدمام', 'حي العزيزية، مكة', 'حي النرجس، الرياض', 'حي الروضة، جدة', 'حي الخالدية، الطائف', 'حي الصفا، المدينة', 'حي الحمراء، الخبر', 'حي الياسمين، الرياض', 'حي السلامة، جدة', 'حي العليا، الرياض'];
  const TYPES = { res: 'سكني', land: 'أرض', com: 'تجاري' }, STS = { paid: ['مدفوع', 'check'], canc: ['ملغى', 'x'], pend: ['معلّق', 'clock'] };
  const TX = NAMES.map((n, i) => ({ id: `#${(48213 + i * 1379).toString().slice(0, 5)}`, n, ini: n.split(' ').map(w => w.replace('ال', '')[0]).join('\u200c'), m: [9, 9, 8, 8, 9, 7, 8, 9, 7, 6, 9, 8][i], d: [28, 21, 30, 12, 6, 25, 3, 15, 9, 18, 2, 19][i], t: ['res', 'land', 'com', 'res', 'res', 'com', 'land', 'res', 'com', 'land', 'res', 'com'][i], a: ADDR[i], st: ['paid', 'pend', 'paid', 'canc', 'paid', 'pend', 'paid', 'paid', 'canc', 'paid', 'pend', 'paid'][i], p: [1250000, 860000, 3400000, 720000, 1980000, 2650000, 540000, 1120000, 4100000, 930000, 1460000, 2890000][i] }));
  function tx() {
    let L = TX.filter(t => (S.mon === 'all' || t.m === +S.mon) && (S.st === 'all' || t.st === S.st) && (!S.q || (t.n + t.id + t.a).includes(S.q)));
    const [k, dir] = S.sort; L.sort((a, b) => (k === 'date' ? (a.m * 40 + a.d) - (b.m * 40 + b.d) : k === 'price' ? a.p - b.p : a.n.localeCompare(b.n, 'ar')) * dir);
    $$('#reTx th[data-sort]').forEach(th => th.setAttribute('aria-sort', th.dataset.sort === k ? (dir > 0 ? 'ascending' : 'descending') : 'none'));
    $('#reTx tbody').innerHTML = L.map((t, i) => `<tr style="--i:${i}" class="${S.sel.has(t.id) ? 'on' : ''}">
      <td><input type="checkbox" data-row="${t.id}" ${S.sel.has(t.id) ? 'checked' : ''} aria-label="تحديد ${t.n}"></td>
      <td><span class="re-who"><i class="av" style="--h:${i * 47 % 360}">${t.ini}</i><b>${t.n}</b></span></td>
      <td><span class="re-id"><span class="num">${t.id}</span><button type="button" data-copy="${t.id}" aria-label="نسخ المعرّف ${t.id}">${ic('copy')}</button></span></td>
      <td><span class="num">${t.d}</span> ${MON[t.m - 1]} <span class="num">2026</span></td>
      <td><span class="re-type t-${t.t}">${TYPES[t.t]}</span></td>
      <td class="addr">${ic('pin')}${t.a}</td>
      <td><span class="re-st s-${t.st}">${ic(STS[t.st][1])}${STS[t.st][0]}</span></td>
      <td><span class="num">${nf(t.p)}</span> <small>ر.س</small></td>
      <td><button type="button" class="re-act" data-edit="${t.id}" aria-label="تعديل ${t.id}">${ic('edit')}</button></td></tr>`).join('') || `<tr><td colspan="9" class="muted" style="text-align:center;padding:24px">لا معاملات تطابق التصفية.</td></tr>`;
    $('#reAll').checked = L.length > 0 && L.every(t => S.sel.has(t.id));
    const sum = L.filter(t => t.st === 'paid').reduce((a, t) => a + t.p, 0);
    $('#reTxSum').innerHTML = `<span class="num">${L.length}</span> معاملة · المدفوع <b class="num">${nf(sum)}</b> ر.س${S.sel.size ? ` · محدد <span class="num">${S.sel.size}</span>` : ''}`;
    $('#reTx').dataset.vis = L.map(t => t.id).join(',');
  }

  /* ---------- الأحداث ---------- */
  root.addEventListener('click', e => {
    const t = e.target;
    const sg = t.closest('[data-ser]'); if (sg) { const k = sg.dataset.ser; S.hide.has(k) ? S.hide.delete(k) : S.hide.add(k); if (S.hide.size === 2) S.hide.delete(k); sg.setAttribute('aria-pressed', String(!S.hide.has(k))); $$('[data-ser]').forEach(b => b.setAttribute('aria-pressed', String(!S.hide.has(b.dataset.ser)))); return income(); }
    const so = t.closest('th[data-sort]'); if (so) { const k = so.dataset.sort; S.sort = [k, S.sort[0] === k ? -S.sort[1] : -1]; return tx(); }
    const fs = t.closest('[data-stf]'); if (fs) { S.st = fs.dataset.stf; $$('[data-stf]').forEach(b => b.setAttribute('aria-pressed', String(b === fs))); return tx(); }
    const cp = t.closest('[data-copy]'); if (cp) { const id = cp.dataset.copy; (navigator.clipboard ? navigator.clipboard.writeText(id) : Promise.reject()).catch(() => {}); cp.classList.add('ok'); setTimeout(() => cp.classList.remove('ok'), 1200); return toast(`تم نسخ المعرّف ${id}`); }
    const ed = t.closest('[data-edit]'); if (ed) return toast(`فتح نموذج تعديل المعاملة ${ed.dataset.edit}`);
    if (t.closest('[data-goal]')) return toast('يمكنك تعديل الأهداف من إعدادات المشروع.');
  });
  root.addEventListener('change', e => {
    const t = e.target;
    if (t.id === 'reYear') { S.year = t.value; return all(); }
    if (t.id === 'reMon') { S.mon = t.value; return tx(); }
    if (t.id === 'reAll') { (($('#reTx').dataset.vis || '').split(',').filter(Boolean)).forEach(id => t.checked ? S.sel.add(id) : S.sel.delete(id)); return tx(); }
    if (t.dataset.row) { t.checked ? S.sel.add(t.dataset.row) : S.sel.delete(t.dataset.row); t.closest('tr').classList.toggle('on', t.checked); const L = ($('#reTx').dataset.vis || '').split(',').filter(Boolean); $('#reAll').checked = L.every(id => S.sel.has(id)); const s = $('#reTxSum'); s.innerHTML = s.innerHTML.replace(/ · محدد.*$/, '') + (S.sel.size ? ` · محدد <span class="num">${S.sel.size}</span>` : ''); }
  });
  $('#reQ').addEventListener('input', e => { S.q = e.target.value.trim(); tx(); });
  const inc = $('#reInc');
  inc.addEventListener('pointerover', e => incTip(e.target.closest('.re-mo:not(.fut)')));
  inc.addEventListener('pointerleave', () => incTip(null));
  inc.addEventListener('focusin', e => incTip(e.target.closest('.re-mo')));
  inc.addEventListener('focusout', () => incTip(null));
  function all() { kpis(); income(); balance(); tx(); }
  all();
})();
