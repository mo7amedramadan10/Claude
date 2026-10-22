/* ==========================================================
   جيم — تتبع الوقت للفرق (لوحة تفاعلية، المحرك: timetracking) — فاتح
   مُعاد بناؤه كدالة تركيب (window.JEEM_TIMETRACKING_MOUNT) بدل IIFE ذاتي التشغيل
   بمولّد بيانات عشوائي مبذور (seeded RNG) — الساعات الآن مُجمَّعة فعليًا من
   data.timeEntries الحقيقية حسب الفترة المعروضة (شهر/أسبوع/يوم)، بدل اختلاقها.
   الشهر الحالي ومؤشر "إجازة اليوم" حقيقيان (Date() الفعلي)، لا محاكاة.
   ========================================================== */
(function () {
  function mount(root, data, opts = {}) {
    if (!root || !data) return;
    const $ = (s, el = root) => el.querySelector(s), $$ = (s, el = root) => [...el.querySelectorAll(s)];
    const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
    const esc = s => String(s).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
    const RM = matchMedia('(prefers-reduced-motion: reduce)').matches;
    const hm = h => `${Math.floor(h)}س ${String(Math.round(h % 1 * 60)).padStart(2, '0')}د`;
    const hmH = h => { const m = Math.round(h * 60); return `<span class="num">${Math.floor(m / 60)}:${String(m % 60).padStart(2, '0')}</span><small class="u">ساعة</small>`; };
    const MONTHS = ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو', 'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر'];
    const WD = ['الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت'];
    const LEAVE_LABEL = { annual: ['سنوية', '#6E62B0'], sick: ['مرضية', '#D64545'], remote: ['عن بُعد', '#00BAB3'] };
    const STATUS = { online: ['متصل', 'on'], offline: ['غير متصل', 'off'], leave: ['في إجازة', 'lv'], remote: ['عن بُعد', 'rm'] };
    const toast = msg => {
      let t = document.querySelector('.toast');
      if (!t) { t = document.createElement('div'); t.className = 'toast'; t.setAttribute('role', 'status'); document.body.append(t); }
      t.innerHTML = `${ic('check')}<span>${esc(msg)}</span>`;
      requestAnimationFrame(() => t.classList.add('is-on'));
      clearTimeout(t._h); t._h = setTimeout(() => t.classList.remove('is-on'), 2600);
    };

    const now0 = new Date();
    const PROJ = (data.projects || []).map(p => [p.name, p.color || '#127DBD']);
    const PID = (data.projects || []).map(p => p.id);
    const BILL = (data.projects || []).map(p => p.billableRate || 0);
    const PPL = (data.people || []).map(p => [p.name, p.role || '']);
    const UID = (data.people || []).map(p => p.id);
    const PRESENCE = Object.fromEntries((data.presence || []).map(p => [p.userId, p.status]));
    const ENTRIES = (data.timeEntries || []).map(e => ({ ...e, dt: new Date(e.startedAt) }));
    const LEAVES = data.leaves || [];
    const S = { y: now0.getFullYear(), m: now0.getMonth(), per: 'month', proj: null, who: null };

    /* ---------- تجميع بيانات الفترة من time_entries الحقيقية ---------- */
    function periodData() {
      const dim = new Date(S.y, S.m + 1, 0).getDate();
      let units, rangeStart;
      if (S.per === 'month') {
        units = Array.from({ length: dim }, (_, i) => { const d = new Date(S.y, S.m, i + 1); return { lab: String(i + 1), full: `${i + 1} ${MONTHS[S.m]}`, off: d.getDay() === 5 || d.getDay() === 6 }; });
        rangeStart = new Date(S.y, S.m, 1);
      } else if (S.per === 'week') {
        rangeStart = new Date(S.y, S.m, 1);
        units = Array.from({ length: 7 }, (_, i) => { const d = new Date(rangeStart); d.setDate(rangeStart.getDate() + i); return { lab: WD[d.getDay()].replace('ال', ''), full: `${WD[d.getDay()]} ${d.getDate()} ${MONTHS[d.getMonth()]}`, off: d.getDay() === 5 || d.getDay() === 6 }; });
      } else {
        rangeStart = (S.y === now0.getFullYear() && S.m === now0.getMonth()) ? new Date(S.y, S.m, now0.getDate()) : new Date(S.y, S.m, 1);
        units = Array.from({ length: 10 }, (_, i) => ({ lab: `${8 + i}:00`, full: `${8 + i}:00`, off: false }));
      }
      const H = PPL.map(() => units.map(() => PROJ.map(() => 0)));
      ENTRIES.forEach(e => {
        const p = UID.indexOf(e.userId), j = PID.indexOf(e.projectId);
        if (p < 0 || j < 0) return;
        let u = -1;
        if (S.per === 'month') { if (e.dt.getFullYear() === S.y && e.dt.getMonth() === S.m) u = e.dt.getDate() - 1; }
        else if (S.per === 'week') { const diff = Math.floor((new Date(e.dt.getFullYear(), e.dt.getMonth(), e.dt.getDate()) - rangeStart) / 864e5); if (diff >= 0 && diff < 7) u = diff; }
        else { const sameDay = e.dt.getFullYear() === rangeStart.getFullYear() && e.dt.getMonth() === rangeStart.getMonth() && e.dt.getDate() === rangeStart.getDate(); if (sameDay) { const h = e.dt.getHours(); if (h >= 8 && h < 18) u = h - 8; } }
        if (u < 0) return;
        H[p][u][j] += (e.durationMin || 0) / 60;
      });
      const leave = PPL.map((_, p) => {
        if (S.per !== 'month') return [];
        const uid = UID[p];
        return LEAVES.filter(l => l.userId === uid).map(l => {
          const sd = new Date(l.startDate), ed = new Date(l.endDate);
          if (ed.getFullYear() < S.y || (ed.getFullYear() === S.y && ed.getMonth() < S.m)) return null;
          if (sd.getFullYear() > S.y || (sd.getFullYear() === S.y && sd.getMonth() > S.m)) return null;
          const s = (sd.getFullYear() === S.y && sd.getMonth() === S.m) ? sd.getDate() : 1;
          const e = (ed.getFullYear() === S.y && ed.getMonth() === S.m) ? ed.getDate() : dim;
          return { s, e, t: l.type };
        }).filter(Boolean);
      });
      leave.forEach((L, p) => L.forEach(l => { if (l.t !== 'remote') for (let d = l.s; d <= l.e; d++) H[p][d - 1] = PROJ.map(() => 0); }));
      return { units, H, leave, bill: BILL, dim, rangeStart };
    }
    let D = periodData();
    const sumP = (p, j) => D.H[p].reduce((a, u) => a + (j == null ? u.reduce((x, y) => x + y, 0) : u[j]), 0);
    const inScope = p => S.who == null || S.who === p;

    /* ---------- 1) المؤشرات ---------- */
    function kpis() {
      const ppl = PPL.map((_, p) => p).filter(inScope), tot = ppl.reduce((a, p) => a + (S.proj == null ? sumP(p) : sumP(p, S.proj)), 0);
      const billH = ppl.reduce((a, p) => a + PROJ.reduce((x, _, j) => x + (S.proj != null && j !== S.proj ? 0 : sumP(p, j) * D.bill[j] / 100), 0), 0);
      const active = PROJ.filter((_, j) => ppl.some(p => sumP(p, j) > 0)).length;
      const onLeave = UID.filter(uid => PRESENCE[uid] === 'leave').length;
      $('#tkKpis').innerHTML = [['clock', 'الوقت المسجّل', hmH(tot)], ['wallet', 'قابل للفوترة', `<span class="num">${tot ? Math.round(billH / tot * 100) : 0}%</span>`], ['layers', 'المشاريع النشطة', `<span class="num">${active}</span>`], ['calendar', 'في إجازة اليوم', `<span class="num">${onLeave}</span><small class="u">موظف</small>`]]
        .map(([i, n, v], k) => `<div class="tk-kpi" style="--i:${k}"><span class="ib">${ic(i)}</span><span><small>${n}</small><b>${v}</b></span></div>`).join('');
    }

    /* ---------- 2) حلقة الوقت حسب المشروع ---------- */
    function donut() {
      const ppl = PPL.map((_, p) => p).filter(inScope), v = PROJ.map((_, j) => ppl.reduce((a, p) => a + sumP(p, j), 0)), tot = v.reduce((a, b) => a + b, 0) || 1;
      const R = 70, L = 2 * Math.PI * R; let acc = 0;
      $('#tkDonut').innerHTML = `<svg viewBox="0 0 200 200" role="img" aria-label="الوقت حسب المشروع">${v.map((x, j) => {
        if (!x) return ''; const len = x / tot * L - 3, s = `<circle cx="100" cy="100" r="${R}" class="sg${S.proj != null && S.proj !== j ? ' dim' : ''}" stroke="${PROJ[j][1]}" stroke-dasharray="${Math.max(0, len).toFixed(1)} ${L}" stroke-dashoffset="${(-acc).toFixed(1)}" transform="rotate(-90 100 100)" data-proj="${j}" style="--i:${j}"><title>${esc(PROJ[j][0])}: ${hm(x)}</title></circle>`; acc += x / tot * L; return s;
      }).join('')}</svg>
        <span class="c"><b>${hmH(S.proj == null ? tot : v[S.proj])}</b><small>${S.proj == null ? `${v.filter(x => x).length} مشاريع` : esc(PROJ[S.proj][0])}</small></span>`;
      $('#tkLeg').innerHTML = PROJ.map(([n, c], j) => v[j] ? `<button type="button" data-proj="${j}" aria-pressed="${S.proj === j}"><i style="background:${c}"></i>${esc(n)}<b class="num">${Math.round(v[j] / tot * 100)}%</b></button>` : '').join('');
    }

    /* ---------- 3) أعمدة مكدّسة حسب الوحدة الزمنية ---------- */
    function stats() {
      const ppl = PPL.map((_, p) => p).filter(inScope), tot = D.units.map((_, u) => PROJ.map((_, j) => ppl.reduce((a, p) => a + D.H[p][u][j], 0)));
      const mx = Math.max(8, ...tot.map(t => t.reduce((a, b) => a + b, 0))) * 1.08, top = Math.ceil(mx / 8) * 8;
      $('#tkStatsY').innerHTML = [1, .75, .5, .25, 0].map(f => `<span class="num">${Math.round(top * f)}</span>`).join('');
      $('#tkStats').style.setProperty('--n', D.units.length);
      $('#tkStats').innerHTML = D.units.map((u, i) => {
        const t = tot[i], sum = t.reduce((a, b) => a + b, 0);
        return `<button type="button" class="col${u.off ? ' off' : ''}" style="--i:${i}" aria-label="${esc(u.full)}: ${hm(sum)}" data-tip="${esc(u.full)}|${u.off ? 'عطلة' : hm(sum) + (S.proj != null ? ' · ' + esc(PROJ[S.proj][0]) + ' ' + hm(t[S.proj]) : '')}">
          <span class="stk" style="height:${u.off ? 100 : top ? (sum / top * 100).toFixed(1) : 0}%">${u.off ? '' : t.map((h, j) => h ? `<i style="flex:${h};background:${PROJ[j][1]}" class="${S.proj != null && S.proj !== j ? 'dim' : ''}"></i>` : '').join('')}</span><small>${esc(u.lab)}</small></button>`;
      }).join('');
    }

    /* ---------- 4) الإجمالي حسب الشخص ---------- */
    function people() {
      $('#tkPpl').innerHTML = PPL.map(([n, r], p) => {
        const tot = sumP(p), st = STATUS[PRESENCE[UID[p]]] || STATUS.offline;
        return `<button type="button" class="pc${S.who === p ? ' on' : ''}" data-who="${p}" aria-pressed="${S.who === p}" style="--i:${p};--h:${p * 57 + 200}"><span class="ptop"><span class="av">${esc(n.split(' ').map(x => x.replace(/^ال/, '')[0]).join('‌'))}</span><span class="st ${st[1]}">${st[0]}</span></span><b>${esc(n)}</b><small>${esc(r)}</small>
          <span class="hrs">${ic('clock')}${hmH(tot)}</span><span class="bar">${PROJ.map((_, j) => { const h = sumP(p, j); return h ? `<i style="flex:${h};background:${PROJ[j][1]}" class="${S.proj != null && S.proj !== j ? 'dim' : ''}" title="${esc(PROJ[j][0])}: ${hm(h)}"></i>` : ''; }).join('')}</span></button>`;
      }).join('');
    }

    /* ---------- 5) تفصيل المشاريع (قابل للفوترة) ---------- */
    function projTable() {
      const ppl = PPL.map((_, p) => p).filter(inScope);
      const rows = PROJ.map(([n, c], j) => ({ j, n, c, h: ppl.reduce((a, p) => a + sumP(p, j), 0), b: D.bill[j] })).filter(r => r.h).sort((a, b) => b.h - a.h);
      $('#tkProj').innerHTML = rows.length ? `<table><thead><tr><th>المشروع</th><th>الساعات</th><th>قابل للفوترة</th></tr></thead><tbody>${rows.map((r, i) => `<tr class="${S.proj === r.j ? 'on' : ''}" data-proj="${r.j}" style="--i:${i}" tabindex="0"><td><span class="pn"><i style="background:${r.c}"></i>${esc(r.n)}</span></td><td>${hmH(r.h)}</td>
        <td><span class="bl"><span class="tr"><i style="width:${r.b}%"></i></span><span class="num">${r.b}%</span></span></td></tr>`).join('')}</tbody></table>` : `<p class="tk-none">لا ساعات مسجّلة لهذه الفترة.</p>`;
    }

    /* ---------- 6) جدول الإجازات ---------- */
    function leaveGrid() {
      const box = $('#tkLeave');
      if (S.per !== 'month') { box.innerHTML = `<p class="tk-none">جدول الإجازات يظهر في العرض الشهري.</p>`; return; }
      const n = D.dim, isCurMonth = S.y === now0.getFullYear() && S.m === now0.getMonth();
      box.innerHTML = `<div class="lg" style="--n:${n}"><span class="nm"></span>${Array.from({ length: n }, (_, d) => { const w = new Date(S.y, S.m, d + 1).getDay(); return `<span class="dh${w === 5 || w === 6 ? ' we' : ''}${isCurMonth && d + 1 === now0.getDate() ? ' td' : ''}">${d + 1}</span>`; }).join('')}
        ${PPL.map(([nm], p) => `<span class="nm${S.who === p ? ' on' : ''}">${esc(nm.split(' ')[0])}</span><span class="row" style="grid-column:2 / span ${n}">${D.leave[p].map(l => `<i class="lv" style="inset-inline-start:${((l.s - 1) / n * 100).toFixed(2)}%;width:${((l.e - l.s + 1) / n * 100).toFixed(2)}%;--c:${(LEAVE_LABEL[l.t] || LEAVE_LABEL.annual)[1]}" title="${esc(nm)}: ${(LEAVE_LABEL[l.t] || LEAVE_LABEL.annual)[0]} ${l.s}–${l.e} ${MONTHS[S.m]}"><span>${(LEAVE_LABEL[l.t] || LEAVE_LABEL.annual)[0]}</span></i>`).join('')}</span>`).join('')}</div>
        <div class="lk">${Object.values(LEAVE_LABEL).map(([n, c]) => `<span><i style="background:${c}"></i>${n}</span>`).join('')}</div>`;
    }

    function all() {
      kpis(); donut(); stats(); people(); projTable(); leaveGrid();
      if (S.per === 'month') $('#tkMonth').textContent = `${MONTHS[S.m]} ${S.y}`;
      else if (S.per === 'week') { const e = new Date(D.rangeStart); e.setDate(e.getDate() + 6); $('#tkMonth').textContent = `${D.rangeStart.getDate()}–${e.getDate()} ${MONTHS[S.m]}`; }
      else $('#tkMonth').textContent = `${D.rangeStart.getDate()} ${MONTHS[D.rangeStart.getMonth()]} ${D.rangeStart.getFullYear()}`;
      $('#tkCur').hidden = S.m === now0.getMonth() && S.y === now0.getFullYear();
      $('#tkScope').textContent = S.who != null ? `عرض: ${PPL[S.who][0]}` : S.proj != null ? `عرض: ${PROJ[S.proj][0]}` : '';
      $('#tkClear').hidden = S.who == null && S.proj == null;
    }
    function reload(dir) {
      D = periodData();
      const g = $('.tk-body');
      if (!RM && dir && g) { g.classList.remove('sl-l', 'sl-r'); void g.offsetWidth; g.classList.add(dir > 0 ? 'sl-l' : 'sl-r'); }
      all();
    }

    root.addEventListener('click', e => {
      const t = e.target;
      const pr = t.closest('[data-proj]'); if (pr) { const j = +pr.dataset.proj; S.proj = S.proj === j ? null : j; return all(); }
      const w = t.closest('[data-who]'); if (w) { const p = +w.dataset.who; S.who = S.who === p ? null : p; return all(); }
      const pe = t.closest('[data-per]'); if (pe) { S.per = pe.dataset.per; $$('[data-per]').forEach(b => b.setAttribute('aria-pressed', String(b === pe))); return reload(0); }
      const nv = t.closest('[data-nav]'); if (nv) { const d = +nv.dataset.nav; S.m += d; if (S.m < 0) { S.m = 11; S.y--; } if (S.m > 11) { S.m = 0; S.y++; } return reload(d); }
      if (t.closest('#tkCur')) { const d = (now0.getFullYear() * 12 + now0.getMonth()) > (S.y * 12 + S.m) ? 1 : -1; S.y = now0.getFullYear(); S.m = now0.getMonth(); return reload(d); }
      if (t.closest('#tkClear')) { S.proj = S.who = null; return all(); }
      if (t.closest('[data-export]')) { opts.onAction?.('exportReport', { year: S.y, month: S.m, period: S.per }); return toast('يُصدَّر تقرير الوقت (CSV/PDF) للفترة والتصفية الحالية.'); }
    });
    root.addEventListener('keydown', e => { const r = e.target.closest('tr[data-proj]'); if (r && (e.key === 'Enter' || e.key === ' ')) { e.preventDefault(); r.click(); } });
    root.addEventListener('pointerover', e => { const s = e.target.closest('#tkDonut [data-proj]'); $$('#tkLeg button').forEach(b => b.classList.toggle('hl', !!s && b.dataset.proj === s.dataset.proj)); });

    all();
    return { destroy() { } };
  }

  window.JEEM_TIMETRACKING_MOUNT = mount;
})();
