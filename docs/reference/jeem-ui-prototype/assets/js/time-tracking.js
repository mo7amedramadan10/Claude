/* ==========================================================
   جيم — تتبع الوقت للفرق (لوحة تفاعلية)
   مستوحاة من «Time Tracking Dashboard UI/UX Design for Team Management».
   الحركات: حلقة المشاريع تُكتسح، أعمدة مكدّسة تنمو، تنقل بين الأشهر بانتقال،
   تصفية مترابطة بالمشروع أو الشخص تُبهت الباقي في كل الرسوم.
   ========================================================== */
(function () {
  const root = document.getElementById('tkRoot'); if (!root) return;
  const $ = (s, el = root) => el.querySelector(s), $$ = (s, el = root) => [...el.querySelectorAll(s)];
  const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
  const esc = s => String(s).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
  const RM = matchMedia('(prefers-reduced-motion: reduce)').matches;
  const hm = h => `${Math.floor(h)}س ${String(Math.round(h % 1 * 60)).padStart(2, '0')}د`;
  const hmH = h => { const m = Math.round(h * 60); return `<span class="num">${Math.floor(m / 60)}:${String(m % 60).padStart(2, '0')}</span><small class="u">ساعة</small>`; };
  const MONTHS = ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو', 'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر'];
  const WD = ['الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت'];

  const PROJ = [['بوابة الخدمات', '#127DBD'], ['تطبيق الجوال', '#00BAB3'], ['منصة البيانات', '#6E62B0'], ['الهوية البصرية', '#D9822B'], ['نظام الموارد', '#28B6E6'], ['التسويق الرقمي', '#C2569B'], ['الدعم الفني', '#7CB342'], ['التدريب', '#E5B33B'], ['أعمال داخلية', '#9AA5B1']];
  const PPL = [['سارة القحطاني', 'مصممة واجهات'], ['خالد العتيبي', 'مطور خلفيات'], ['نورة الشمري', 'مديرة مشروع'], ['فهد الدوسري', 'مطور واجهات'], ['ريم الحربي', 'محللة بيانات'], ['ماجد الغامدي', 'مختبر جودة']];
  const LEAVE = { an: ['سنوية', '#6E62B0'], sk: ['مرضية', '#D64545'], rm: ['عن بُعد', '#00BAB3'] };
  const S = { y: 2026, m: 9, per: 'month', proj: null, who: null };
  const seed = k => { let h = 2166136261; for (const c of k) h = Math.imul(h ^ c.charCodeAt(0), 16777619); return () => ((h = Math.imul(h ^ (h >>> 15), 2246822507) >>> 0) / 4294967296); };

  /* بيانات الفترة: ساعات[شخص][وحدة زمنية][مشروع] + إجازات */
  function data() {
    const r = seed(`${S.y}-${S.m}-${S.per}`), dim = new Date(S.y, S.m + 1, 0).getDate();
    let units;
    if (S.per === 'month') units = Array.from({ length: dim }, (_, i) => { const d = new Date(S.y, S.m, i + 1); return { lab: String(i + 1), full: `${i + 1} ${MONTHS[S.m]}`, off: d.getDay() === 5 || d.getDay() === 6 }; });
    else if (S.per === 'week') units = Array.from({ length: 7 }, (_, i) => ({ lab: WD[i].replace('ال', ''), full: WD[i], off: i >= 5 }));
    else units = Array.from({ length: 10 }, (_, i) => ({ lab: `${8 + i}:00`, full: `${8 + i}:00`, off: i === 4 }));
    const scale = S.per === 'day' ? .12 : 1;
    const H = PPL.map((_, p) => units.map(u => u.off ? PROJ.map(() => 0) : PROJ.map((_, j) => { const base = (j + p) % 4 === 0 ? 3.2 : (j * 3 + p) % 5 === 0 ? 1.6 : r() < .25 ? r() * 1.2 : 0; return +(base * (.42 + r() * .5) * scale).toFixed(2); })));
    const leave = PPL.map((_, p) => { const L = []; if (S.per !== 'month') return L; const n = 1 + (p * 7 + S.m) % 3; for (let k = 0; k < n; k++) { const s = 1 + Math.floor(r() * (dim - 4)), len = 1 + Math.floor(r() * 4), t = ['an', 'sk', 'rm'][Math.floor(r() * 3)]; L.push({ s, e: Math.min(dim, s + len - 1), t }); } return L; });
    if (S.per === 'month' && S.y === 2026 && S.m === 9) leave[2].push({ s: 12, e: 14, t: 'an' });
    leave.forEach((L, p) => L.forEach(l => { if (l.t !== 'rm') for (let d = l.s; d <= l.e; d++) H[p][d - 1] = PROJ.map(() => 0); }));
    const bill = PROJ.map((_, j) => j >= 6 ? 0 : 55 + ((j * 17 + S.m) % 40));
    return { units, H, leave, bill, dim };
  }
  let D = data();
  const sumP = (p, j) => D.H[p].reduce((a, u) => a + (j == null ? u.reduce((x, y) => x + y, 0) : u[j]), 0);
  const inScope = p => S.who == null || S.who === p;

  /* ---------- 1) المؤشرات ---------- */
  function kpis() {
    const ppl = PPL.map((_, p) => p).filter(inScope), tot = ppl.reduce((a, p) => a + (S.proj == null ? sumP(p) : sumP(p, S.proj)), 0);
    const billH = ppl.reduce((a, p) => a + PROJ.reduce((x, _, j) => x + (S.proj != null && j !== S.proj ? 0 : sumP(p, j) * D.bill[j] / 100), 0), 0);
    const active = PROJ.filter((_, j) => ppl.some(p => sumP(p, j) > 0)).length, onLeave = D.leave.filter(L => L.some(l => l.t !== 'rm' && l.s <= 13 && l.e >= 13)).length;
    $('#tkKpis').innerHTML = [['clock', 'الوقت المسجّل', hmH(tot)], ['wallet', 'قابل للفوترة', `<span class="num">${tot ? Math.round(billH / tot * 100) : 0}%</span>`], ['layers', 'المشاريع النشطة', `<span class="num">${active}</span>`], ['calendar', 'في إجازة اليوم', `<span class="num">${onLeave}</span><small class="u">موظف</small>`]]
      .map(([i, n, v], k) => `<div class="tk-kpi" style="--i:${k}"><span class="ib">${ic(i)}</span><span><small>${n}</small><b>${v}</b></span></div>`).join('');
  }

  /* ---------- 2) حلقة الوقت حسب المشروع ---------- */
  function donut() {
    const ppl = PPL.map((_, p) => p).filter(inScope), v = PROJ.map((_, j) => ppl.reduce((a, p) => a + sumP(p, j), 0)), tot = v.reduce((a, b) => a + b, 0) || 1;
    const R = 70, L = 2 * Math.PI * R; let acc = 0;
    $('#tkDonut').innerHTML = `<svg viewBox="0 0 200 200" role="img" aria-label="الوقت حسب المشروع">${v.map((x, j) => { if (!x) return ''; const len = x / tot * L - 3, s = `<circle cx="100" cy="100" r="${R}" class="sg${S.proj != null && S.proj !== j ? ' dim' : ''}" stroke="${PROJ[j][1]}" stroke-dasharray="${Math.max(0, len).toFixed(1)} ${L}" stroke-dashoffset="${(-acc).toFixed(1)}" transform="rotate(-90 100 100)" data-proj="${j}" style="--i:${j}"><title>${PROJ[j][0]}: ${hm(x)}</title></circle>`; acc += x / tot * L; return s; }).join('')}</svg>
      <span class="c"><b>${hmH(S.proj == null ? tot : v[S.proj])}</b><small>${S.proj == null ? `${v.filter(x => x).length} مشاريع` : esc(PROJ[S.proj][0])}</small></span>`;
    $('#tkLeg').innerHTML = PROJ.map(([n, c], j) => v[j] ? `<button type="button" data-proj="${j}" aria-pressed="${S.proj === j}"><i style="background:${c}"></i>${n}<b class="num">${Math.round(v[j] / tot * 100)}%</b></button>` : '').join('');
  }

  /* ---------- 3) أعمدة مكدّسة حسب الوحدة الزمنية ---------- */
  function stats() {
    const ppl = PPL.map((_, p) => p).filter(inScope), tot = D.units.map((_, u) => PROJ.map((_, j) => ppl.reduce((a, p) => a + D.H[p][u][j], 0)));
    const mx = Math.max(8, ...tot.map(t => t.reduce((a, b) => a + b, 0))) * 1.08, top = Math.ceil(mx / 8) * 8;
    $('#tkStatsY').innerHTML = [1, .75, .5, .25, 0].map(f => `<span class="num">${Math.round(top * f)}</span>`).join('');
    $('#tkStats').style.setProperty('--n', D.units.length);
    $('#tkStats').innerHTML = D.units.map((u, i) => { const t = tot[i], sum = t.reduce((a, b) => a + b, 0);
      return `<button type="button" class="col${u.off ? ' off' : ''}" style="--i:${i}" aria-label="${u.full}: ${hm(sum)}" data-tip="${u.full}|${u.off ? 'عطلة' : hm(sum) + (S.proj != null ? ' · ' + PROJ[S.proj][0] + ' ' + hm(t[S.proj]) : '')}">
        <span class="stk" style="height:${u.off ? 100 : (sum / top * 100).toFixed(1)}%">${u.off ? '' : t.map((h, j) => h ? `<i style="flex:${h};background:${PROJ[j][1]}" class="${S.proj != null && S.proj !== j ? 'dim' : ''}"></i>` : '').join('')}</span><small>${u.lab}</small></button>`; }).join('');
  }

  /* ---------- 4) الإجمالي حسب الشخص ---------- */
  function people() {
    const onLeaveToday = p => D.leave[p].find(l => l.s <= 13 && l.e >= 13);
    $('#tkPpl').innerHTML = PPL.map(([n, r], p) => { const tot = sumP(p), lv = S.per === 'month' && onLeaveToday(p), st = lv ? (lv.t === 'rm' ? ['عن بُعد', 'rm'] : ['في إجازة', 'lv']) : p % 4 === 3 ? ['غير متصل', 'off'] : ['متصل', 'on'];
      return `<button type="button" class="pc${S.who === p ? ' on' : ''}" data-who="${p}" aria-pressed="${S.who === p}" style="--i:${p};--h:${p * 57 + 200}"><span class="ptop"><span class="av">${n.split(' ').map(x => x.replace(/^ال/, '')[0]).join('‌')}</span><span class="st ${st[1]}">${st[0]}</span></span><b>${n}</b><small>${r}</small>
        <span class="hrs">${ic('clock')}${hmH(tot)}</span><span class="bar">${PROJ.map((_, j) => { const h = sumP(p, j); return h ? `<i style="flex:${h};background:${PROJ[j][1]}" class="${S.proj != null && S.proj !== j ? 'dim' : ''}" title="${PROJ[j][0]}: ${hm(h)}"></i>` : ''; }).join('')}</span></button>`; }).join('');
  }

  /* ---------- 5) تفصيل المشاريع (قابل للفوترة) ---------- */
  function projTable() {
    const ppl = PPL.map((_, p) => p).filter(inScope);
    const rows = PROJ.map(([n, c], j) => ({ j, n, c, h: ppl.reduce((a, p) => a + sumP(p, j), 0), b: D.bill[j] })).filter(r => r.h).sort((a, b) => b.h - a.h);
    $('#tkProj').innerHTML = `<table><thead><tr><th>المشروع</th><th>الساعات</th><th>قابل للفوترة</th></tr></thead><tbody>${rows.map((r, i) => `<tr class="${S.proj === r.j ? 'on' : ''}" data-proj="${r.j}" style="--i:${i}" tabindex="0"><td><span class="pn"><i style="background:${r.c}"></i>${r.n}</span></td><td>${hmH(r.h)}</td>
      <td><span class="bl"><span class="tr"><i style="width:${r.b}%"></i></span><span class="num">${r.b}%</span></span></td></tr>`).join('')}</tbody></table>`;
  }

  /* ---------- 6) جدول الإجازات ---------- */
  function leave() {
    const box = $('#tkLeave');
    if (S.per !== 'month') { box.innerHTML = `<p class="tk-none">جدول الإجازات يظهر في العرض الشهري.</p>`; return; }
    const n = D.dim;
    box.innerHTML = `<div class="lg" style="--n:${n}"><span class="nm"></span>${Array.from({ length: n }, (_, d) => { const w = new Date(S.y, S.m, d + 1).getDay(); return `<span class="dh${w === 5 || w === 6 ? ' we' : ''}${d + 1 === 13 && S.m === 9 && S.y === 2026 ? ' td' : ''}">${d + 1}</span>`; }).join('')}
      ${PPL.map(([nm], p) => `<span class="nm${S.who === p ? ' on' : ''}">${nm.split(' ')[0]}</span><span class="row" style="grid-column:2 / span ${n}">${D.leave[p].map(l => `<i class="lv" style="inset-inline-start:${((l.s - 1) / n * 100).toFixed(2)}%;width:${((l.e - l.s + 1) / n * 100).toFixed(2)}%;--c:${LEAVE[l.t][1]}" title="${nm}: ${LEAVE[l.t][0]} ${l.s}–${l.e} ${MONTHS[S.m]}"><span>${LEAVE[l.t][0]}</span></i>`).join('')}</span>`).join('')}</div>
      <div class="lk">${Object.values(LEAVE).map(([n, c]) => `<span><i style="background:${c}"></i>${n}</span>`).join('')}</div>`;
  }

  function all() { kpis(); donut(); stats(); people(); projTable(); leave(); $('#tkMonth').textContent = S.per === 'month' ? `${MONTHS[S.m]} ${S.y}` : S.per === 'week' ? `الأسبوع 2 · ${MONTHS[S.m]}` : `13 ${MONTHS[S.m]} ${S.y}`; $('#tkCur').hidden = S.m === 9 && S.y === 2026; $('#tkScope').textContent = S.who != null ? `عرض: ${PPL[S.who][0]}` : S.proj != null ? `عرض: ${PROJ[S.proj][0]}` : ''; $('#tkClear').hidden = S.who == null && S.proj == null; }
  function reload(dir) { D = data(); const g = $('.tk-body'); if (!RM && dir) { g.classList.remove('sl-l', 'sl-r'); void g.offsetWidth; g.classList.add(dir > 0 ? 'sl-l' : 'sl-r'); } all(); }

  root.addEventListener('click', e => {
    const t = e.target;
    const pr = t.closest('[data-proj]'); if (pr) { const j = +pr.dataset.proj; S.proj = S.proj === j ? null : j; return all(); }
    const w = t.closest('[data-who]'); if (w) { const p = +w.dataset.who; S.who = S.who === p ? null : p; return all(); }
    const pe = t.closest('[data-per]'); if (pe) { S.per = pe.dataset.per; $$('[data-per]').forEach(b => b.setAttribute('aria-pressed', String(b === pe))); return reload(0); }
    const nv = t.closest('[data-nav]'); if (nv) { const d = +nv.dataset.nav; S.m += d; if (S.m < 0) { S.m = 11; S.y--; } if (S.m > 11) { S.m = 0; S.y++; } return reload(d); }
    if (t.closest('#tkCur')) { const d = (2026 * 12 + 9) > (S.y * 12 + S.m) ? 1 : -1; S.y = 2026; S.m = 9; return reload(d); }
    if (t.closest('#tkClear')) { S.proj = S.who = null; return all(); }
    if (t.closest('[data-export]')) return toast();
  });
  root.addEventListener('keydown', e => { const r = e.target.closest('tr[data-proj]'); if (r && (e.key === 'Enter' || e.key === ' ')) { e.preventDefault(); r.click(); } });
  root.addEventListener('pointerover', e => { const s = e.target.closest('#tkDonut [data-proj]'); $$('#tkLeg button').forEach(b => b.classList.toggle('hl', !!s && b.dataset.proj === s.dataset.proj)); });
  function toast() { let t = document.querySelector('.toast'); if (!t) { t = document.createElement('div'); t.className = 'toast'; t.setAttribute('role', 'status'); document.body.append(t); } t.innerHTML = `${ic('check')}<span>يُصدَّر تقرير الوقت (CSV/PDF) للفترة والتصفية الحالية.</span>`; requestAnimationFrame(() => t.classList.add('is-on')); clearTimeout(t._h); t._h = setTimeout(() => t.classList.remove('is-on'), 2600); }
  all();
})();
