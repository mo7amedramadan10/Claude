/* ==========================================================
   جيم — مراقبة شبكة المنشأة أثناء الفعاليات (لوحة تفاعلية بالوضع الداكن)
   - شجرة البنية التحتية: نقاط رئيسية (يمين) تتفرع إلى صفوف من الأجهزة، والنبضات المضيئة تمثّل حركة البيانات.
   - كل بطاقة: صحة الجهاز (حلقة)، عدد المتصلين، وشارة التنبيهات. القيم تتحدث مباشرة كل ثانيتين.
   - ضغطة على جهاز: يتنوّر مساره من النقطة الرئيسية، وتعرض اللوحة الجانبية تفاصيله وحركته الحية وتنبيهاته.
   - تبديل البوابة، تكبير وتصغير وسحب، إيقاف البث المباشر، و«اسأل جيم».
   البيانات تجريبية ومولّدة؛ في الإنتاج تأتي من نظام المراقبة عبر WebSocket أو استعلام دوري.
   ========================================================== */
(function () {
  const root = document.getElementById('voRoot'); if (!root) return;
  const $ = (s, el = root) => el.querySelector(s), $$ = (s, el = root) => [...el.querySelectorAll(s)];
  const esc = s => String(s).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
  const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
  const nf = (v, d = 0) => Number(v).toLocaleString('en-US', { maximumFractionDigits: d, minimumFractionDigits: d });
  const reduceMotion = matchMedia('(prefers-reduced-motion: reduce)').matches;

  /* ---------- الهيكل: 3 نقاط رئيسية و12 جهازًا في 4 صفوف ---------- */
  const W = 1000, H = 720, CW = 196, CH = 84;
  const ROOTS = [
    { id: 'R1', n: 'حساس المدخل', kind: 'حساس', x: 892, y: 170, rows: [0] },
    { id: 'R2', n: 'نقطة الوصول الرئيسية', kind: 'نقطة وصول', x: 892, y: 380, rows: [1, 2] },
    { id: 'R3', n: 'وحدة التحكم الرئيسية', kind: 'وحدة تحكم', x: 892, y: 620, rows: [3] }
  ];
  const ROWS = [
    [['N1', 'عقدة القطاع الشمالي', 'عقدة'], ['N2', 'عقدة القطاع الجنوبي', 'عقدة'], ['N3', 'موجّه تدفق الشرق', 'موجّه']],
    [['N4', 'مجموعة البوابات الإلكترونية أ', 'بوابات'], ['N5', 'مجموعة البوابات الإلكترونية ب', 'بوابات'], ['N6', 'حساس حركة المرور', 'حساس']],
    [['N7', 'ماسح نقطة التفتيش', 'ماسح'], ['N8', 'مراقب دخول كبار الضيوف', 'كاميرا'], ['N9', 'نشاط الساحة الرئيسية', 'حساس']],
    [['N10', 'حساس تدفق الزوار', 'حساس'], ['N11', 'نشاط الممرات', 'حساس'], ['N12', 'مراقب مسار الدخول', 'كاميرا']]
  ];
  const ROWY = [90, 260, 430, 620], COLX = [652, 436, 220];
  const GATES = { A: { n: 'البوابة أ', seed: 11, crowd: 26400 }, B: { n: 'البوابة ب', seed: 27, crowd: 30000 }, C: { n: 'البوابة ج', seed: 43, crowd: 18750 } };
  const EVENTS = ['مباريات الدوري', 'الحفلات الموسيقية', 'المؤتمرات', 'الفعاليات المجتمعية'];

  let sd = 1; const rnd = () => (sd = (sd * 16807) % 2147483647) / 2147483647;
  const S = { gate: 'B', sel: null, live: !reduceMotion, zoom: 1, panX: 0, panY: 0, hl: null };
  let NODES = [];
  function build() {
    sd = GATES[S.gate].seed * 7919;
    NODES = [];
    ROOTS.forEach(r => NODES.push({ id: r.id, n: r.n, kind: r.kind, root: true, x: r.x, y: r.y, health: 70 + rnd() * 25, users: 6000 + rnd() * 3000, alerts: rnd() < .4 ? Math.round(rnd() * 25) : 0, tp: 2 + rnd() * 6, lat: 8 + rnd() * 14, hist: [] }));
    ROWS.forEach((row, ri) => row.forEach(([id, n, kind], ci) => {
      const parent = ROOTS.find(r => r.rows.includes(ri)).id, bad = rnd() < .28;
      NODES.push({ id, n, kind, parent, row: ri, x: COLX[ci], y: ROWY[ri], health: bad ? 46 + rnd() * 18 : 68 + rnd() * 30, users: 4000 + rnd() * 5000, alerts: bad ? 3 + Math.round(rnd() * 10) : 0, tp: .4 + rnd() * 3, lat: 6 + rnd() * 30, hist: [] });
    }));
    NODES.forEach(n => { for (let i = 0; i < 30; i++) n.hist.push(n.tp * (.7 + rnd() * .6)); });
  }
  const byId = id => NODES.find(n => n.id === id);
  const status = n => n.alerts >= 10 ? 'bad' : n.alerts ? 'warn' : 'ok';
  const pathTo = n => n.root ? [n.id] : [n.parent, n.id];

  /* ---------- مسارات الوصلات (متعامدة، مثل مخطط البنية التحتية) ---------- */
  const TRUNK = 772;
  const linkPath = n => {
    const R = byId(n.parent), ry = R.y, by = n.y - CH / 2 - 26;
    return `M${R.x - CW / 2},${ry} L${TRUNK},${ry} L${TRUNK},${by} L${n.x},${by} L${n.x},${n.y - CH / 2}`;
  };

  /* ---------- البناء ---------- */
  const stage = $('.vo-stage'), inner = $('.vo-inner'), svg = $('#voSvg');
  svg.setAttribute('viewBox', `0 0 ${W} ${H}`); inner.style.width = W + 'px'; inner.style.height = H + 'px';
  const ring = (v, st) => { const R = 9, L = 2 * Math.PI * R; return `<svg class="vo-ring st-${st}" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="${R}" class="trk"/><circle cx="12" cy="12" r="${R}" class="val" stroke-dasharray="${(v / 100 * L).toFixed(1)} ${L.toFixed(1)}" transform="rotate(-90 12 12)"/></svg>`; };
  function drawTree() {
    svg.innerHTML = `<defs><filter id="voGlow" x="-50%" y="-50%" width="200%" height="200%"><feGaussianBlur stdDeviation="3"/></filter></defs><g class="vo-links"></g><g class="vo-pulses"></g>`;
    const gl = $('.vo-links', svg);
    NODES.filter(n => !n.root).forEach(n => { const p = document.createElementNS('http://www.w3.org/2000/svg', 'path'); p.setAttribute('d', linkPath(n)); p.setAttribute('class', 'vo-link'); p.dataset.to = n.id; gl.append(p); n.path = p; n.len = p.getTotalLength(); });
    /* نقاط الالتقاء على الجذع */
    ROOTS.forEach(r => { const c = document.createElementNS('http://www.w3.org/2000/svg', 'circle'); c.setAttribute('cx', TRUNK); c.setAttribute('cy', r.y); c.setAttribute('r', 3.5); c.setAttribute('class', 'vo-joint'); gl.append(c); });
    $$('.vo-card', inner).forEach(e => e.remove());
    NODES.forEach(n => {
      const b = document.createElement('button'); b.type = 'button'; b.className = 'vo-card' + (n.root ? ' is-root' : ''); b.dataset.id = n.id;
      b.style.left = (n.x - CW / 2) + 'px'; b.style.top = (n.y - CH / 2) + 'px'; b.style.width = CW + 'px'; b.style.height = CH + 'px';
      inner.append(b); n.el = b;
    });
    paintCards();
  }
  function paintCards() {
    NODES.forEach(n => {
      const st = status(n);
      n.el.className = `vo-card st-${st}${n.root ? ' is-root' : ''}${S.sel === n.id ? ' is-sel' : ''}${focusSet() && !focusSet().has(n.id) ? ' is-dim' : ''}`;
      n.el.innerHTML = `<span class="vo-ch"><span class="vo-kind">${ic(n.root ? 'server' : 'layers')}</span><b>${esc(n.n)}</b>${n.alerts ? `<em class="vo-badge st-${st}" aria-label="${n.alerts} تنبيه">${ic('bell')}${n.alerts}</em>` : ''}</span>
        <span class="vo-cm"><span class="vo-h">${ring(n.health, n.health >= 75 ? 'ok' : n.health >= 60 ? 'warn' : 'bad')}<b class="num">${nf(n.health)}%</b></span><span class="vo-u">${ic('users')}<b class="num">${nf(n.users)}</b></span></span>`;
      n.el.setAttribute('aria-label', `${n.n}: الصحة ${nf(n.health)}%، ${nf(n.users)} متصل${n.alerts ? `، ${n.alerts} تنبيهات` : ''}`);
      n.el.setAttribute('aria-pressed', String(S.sel === n.id));
      n.el.dataset.tip = `${n.n}|${n.kind} · الصحة ${nf(n.health)}% · ${nf(n.users)} متصل · ${nf(n.tp, 1)} جيجابت/ث · كمون ${nf(n.lat)} مللي ث`;
    });
    const fs = focusSet();
    NODES.filter(n => !n.root).forEach(n => { n.path.classList.toggle('is-on', !!fs && fs.has(n.id) && fs.has(n.parent)); n.path.classList.toggle('is-dim', !!fs && !(fs.has(n.id) && fs.has(n.parent))); n.path.classList.toggle('st-bad', status(n) === 'bad'); });
  }
  const focusSet = () => { if (S.hl) return S.hl; if (!S.sel) return null; const n = byId(S.sel); return new Set(n.root ? [n.id, ...NODES.filter(x => x.parent === n.id).map(x => x.id)] : pathTo(n)); };

  /* ---------- النبضات المضيئة على الوصلات ---------- */
  const pulses = [];
  function spawnPulses() {
    const g = $('.vo-pulses', svg); g.innerHTML = ''; pulses.length = 0;
    NODES.filter(n => !n.root).forEach((n, i) => { for (let k = 0; k < 2; k++) { const c = document.createElementNS('http://www.w3.org/2000/svg', 'g'); c.innerHTML = '<circle r="7" class="halo" filter="url(#voGlow)"/><circle r="2.6" class="core"/>'; g.append(c); pulses.push({ c, n, o: (i * .17 + k * .5) % 1, sp: .00012 + (n.tp / 3.4) * .00012 }); } });
  }
  let raf = 0, last = 0;
  function frame(ts) {
    const dt = last ? ts - last : 16; last = ts;
    const fs = focusSet();
    pulses.forEach(p => { p.o = (p.o + dt * p.sp * (S.live ? 1 : 0)) % 1; const pt = p.n.path.getPointAtLength(p.o * p.n.len); p.c.setAttribute('transform', `translate(${pt.x.toFixed(1)},${pt.y.toFixed(1)})`); p.c.setAttribute('class', `vo-pulse${status(p.n) === 'bad' ? ' st-bad' : ''}${fs && !(fs.has(p.n.id)) ? ' is-dim' : ''}`); });
    raf = requestAnimationFrame(frame);
  }

  /* ---------- التحديث المباشر ---------- */
  let tick = 0;
  function liveTick() {
    if (!S.live) return;
    tick++;
    NODES.forEach(n => {
      n.users = Math.max(500, n.users * (1 + (Math.random() - .45) * .02));
      n.health = Math.min(99, Math.max(35, n.health + (Math.random() - .5) * 1.6));
      n.tp = Math.max(.2, n.tp * (1 + (Math.random() - .48) * .08)); n.hist.push(n.tp); if (n.hist.length > 30) n.hist.shift();
      n.lat = Math.max(3, n.lat + (Math.random() - .5) * 2);
      if (Math.random() < .03) n.alerts += 1; else if (n.alerts && Math.random() < .03) n.alerts -= 1;
    });
    paintCards(); header(); side(); if (tick % 3 === 0) bottom();
  }
  setInterval(liveTick, 2000);

  /* ---------- الترويسة والمؤشرات العلوية ---------- */
  function header() {
    const users = NODES.reduce((a, n) => a + n.users, 0), dev = Math.round(users * .82), alerts = NODES.reduce((a, n) => a + n.alerts, 0);
    const lat = NODES.reduce((a, n) => a + n.lat, 0) / NODES.length, lvl = lat < 12 ? 1 : lat < 18 ? 2 : lat < 24 ? 3 : lat < 30 ? 4 : 5;
    $('#voTitle').textContent = GATES[S.gate].n;
    $('#voTop').innerHTML = `
      <div><span>${ic('users')}الحضور</span><b class="num">${nf(GATES[S.gate].crowd)}</b></div>
      <div><span>${ic('server')}الأجهزة المتصلة</span><b class="num">${nf(dev)}</b></div>
      <div><span>${ic('bell')}التنبيهات النشطة</span><b class="num ${alerts > 30 ? 'tx-bad' : ''}">${alerts}</b></div>
      <div><span>${ic('clock')}الكمون</span><span class="vo-lvl" role="img" aria-label="مستوى الكمون ${lvl} من 5">${[1, 2, 3, 4, 5].map(i => `<i class="${i <= lvl ? 'on l' + lvl : ''}"></i>`).join('')}</span><small class="num">${nf(lat)} مللي ث</small></div>`;
  }

  /* ---------- اللوحة الجانبية ---------- */
  const spark = (arr, cls = '') => { const mx = Math.max(...arr), mi = Math.min(...arr); return `<svg class="vo-spark ${cls}" viewBox="0 0 300 70" preserveAspectRatio="none"><polyline points="${arr.map((v, i) => `${(300 - i * 300 / (arr.length - 1)).toFixed(1)},${(64 - (v - mi) / (mx - mi || 1) * 56).toFixed(1)}`).join(' ')}"/><polygon points="300,70 ${arr.map((v, i) => `${(300 - i * 300 / (arr.length - 1)).toFixed(1)},${(64 - (v - mi) / (mx - mi || 1) * 56).toFixed(1)}`).join(' ')} 0,70"/></svg>`; };
  const ALERT_T = ['ارتفاع حمل الاتصال فوق 85%', 'انقطاع مؤقت لجهاز فرعي', 'تأخر استجابة يتجاوز 40 مللي ث', 'محاولة اتصال غير مصرح بها', 'ارتفاع حرارة الوحدة', 'فقد حزم بيانات 2.4%'];
  function side() {
    const P = $('#voSide');
    if (!S.sel) {
      const hs = Math.round(NODES.reduce((a, n) => a + n.health, 0) / NODES.length), al = NODES.reduce((a, n) => a + n.alerts, 0), wifi = Math.round(NODES.filter(n => !n.root).reduce((a, n) => a + n.users, 0) / 820);
      const lat = NODES.reduce((a, n) => a + n.lat, 0) / NODES.length, dev = NODES.length * 37 + 103;
      P.innerHTML = `<h3 class="vo-sec">مؤشرات الشبكة الرئيسية</h3>
        <div class="vo-km">
          <div>${ring(hs, hs >= 75 ? 'ok' : 'warn')}<b class="num">${hs}<small>/100</small></b><span>متوسط صحة الشبكة</span></div>
          <div>${ic('bell', 'icon')}<b class="num">${Math.round(al / 3)}</b><span>تنبيهات لكل فعالية</span></div>
          <div>${ic('chart', 'icon')}<b class="num">${Math.min(99, wifi)}% <em class="${wifi > 75 ? 'tx-warn' : ''}">${wifi > 75 ? 'مرتفع' : 'طبيعي'}</em></b><span>حمل الشبكة اللاسلكية</span></div>
          <div>${ic('clock', 'icon')}<b class="num">${lat < 15 ? 'منخفض' : lat < 25 ? 'متوسط' : 'مرتفع'}</b><span>الكمون</span></div>
          <div>${ic('server', 'icon')}<b class="num">${dev}</b><span>الأجهزة الطرفية</span></div>
        </div>
        <h3 class="vo-sec">استهلاك الشبكة لكل فعالية</h3>
        <div class="vo-use">${[['متوسط الذروة', 33.4], ['متوسط الاستهلاك', 16.2], ['أقل استهلاك', 14.8]].map(([l, v], i) => `<div>${ring([82, 48, 41][i], 'acc')}<b class="num">${nf(v * (GATES[S.gate].crowd / 30000), 1)} <small>جيجابايت</small></b><span>${l}</span></div>`).join('')}</div>
        <div class="vo-bars" role="img" aria-label="الاستهلاك حسب نوع الفعالية">${EVENTS.map((e, i) => { const a = [82, 64, 40, 37][i] * (GATES[S.gate].crowd / 30000), b = [46, 33, 44, 6][i]; return `<div class="vo-bg"${tip(e, `الذروة ${nf(a)} · المتوسط ${nf(b)} جيجابايت`)}><span class="bb"><i class="s1" style="height:${a}%"></i><i class="s2" style="height:${b}%"></i></span><small>${e}</small></div>`; }).join('')}</div>
        <div class="vo-leg"><span><i class="s1"></i>الذروة</span><span><i class="s2"></i>المتوسط</span></div>`;
    } else {
      const n = byId(S.sel), st = status(n), kids = NODES.filter(x => x.parent === n.id);
      sd = n.id.charCodeAt(1) * 97 + (n.alerts || 1);
      const alerts = Array.from({ length: Math.min(6, n.alerts) }, (_, i) => ({ t: ALERT_T[(i + n.id.length) % ALERT_T.length], m: 2 + Math.round(rnd() * 40), lv: i < n.alerts / 3 ? 'bad' : 'warn' }));
      P.innerHTML = `<button type="button" class="vo-back" data-clear>${ic('chev-left')}كل الشبكة</button>
        <div class="vo-dh"><span class="vo-kind">${ic(n.root ? 'server' : 'layers')}</span><div><b>${esc(n.n)}</b><small>${n.kind}${n.root ? ' · نقطة رئيسية' : ' · متصل بـ' + esc(byId(n.parent).n)}</small></div><span class="vo-tag st-${st}">${st === 'ok' ? 'يعمل' : st === 'warn' ? 'تحذير' : 'حرج'}</span></div>
        <div class="vo-km k4">
          <div>${ring(n.health, n.health >= 75 ? 'ok' : n.health >= 60 ? 'warn' : 'bad')}<b class="num">${nf(n.health)}%</b><span>الصحة</span></div>
          <div>${ic('users', 'icon')}<b class="num">${nf(n.users)}</b><span>متصل الآن</span></div>
          <div>${ic('chart', 'icon')}<b class="num">${nf(n.tp, 1)}</b><span>جيجابت/ث</span></div>
          <div>${ic('clock', 'icon')}<b class="num">${nf(n.lat)}</b><span>مللي ث كمون</span></div>
        </div>
        <h3 class="vo-sec">حركة البيانات · آخر دقيقة ${S.live ? '<em class="vo-livedot">مباشر</em>' : ''}</h3>${spark(n.hist, st === 'bad' ? 'st-bad' : '')}
        ${kids.length ? `<h3 class="vo-sec">الأجهزة المتفرعة (${kids.length})</h3><ul class="vo-kids">${kids.map(k => `<li><button type="button" data-go="${k.id}"><span class="vo-dot st-${status(k)}"></span><span>${esc(k.n)}</span><b class="num">${nf(k.health)}%</b>${k.alerts ? `<em class="vo-badge st-${status(k)}">${k.alerts}</em>` : ''}</button></li>`).join('')}</ul>` : ''}
        <h3 class="vo-sec">التنبيهات (${n.alerts})</h3>
        <ul class="vo-alerts">${alerts.map(a => `<li class="st-${a.lv}">${ic(a.lv === 'bad' ? 'x' : 'bell')}<span>${a.t}</span><small class="num">قبل ${a.m} د</small></li>`).join('') || `<li class="st-ok">${ic('check')}<span>لا توجد تنبيهات نشطة</span></li>`}</ul>
        <div class="vo-act"><button type="button" class="vo-btn" data-act="restart">${ic('history')}إعادة تشغيل</button><button type="button" class="vo-btn" data-act="isolate">${ic('shield')}عزل الجهاز</button><button type="button" class="vo-btn pri" data-act="ask">${ic('spark')}اسأل جيم</button></div>`;
    }
  }
  const tip = (a, b) => ` data-tip="${esc(a)}|${esc(b)}"`;

  /* ---------- الجدول السفلي: تفصيل البنية التحتية ---------- */
  const open = new Set(['R2']);
  function bottom() {
    const mx = Math.max(...NODES.filter(n => n.root).map(n => n.users + NODES.filter(k => k.parent === n.id).reduce((a, k) => a + k.users, 0)));
    $('#voTable').innerHTML = `<table class="vo-tbl"><thead><tr><th>العقدة</th><th>الحالة</th><th>معدل النقل</th><th>المتصلون</th></tr></thead><tbody>${NODES.filter(n => n.root).map(r => {
      const kids = NODES.filter(k => k.parent === r.id), st = [r, ...kids].some(k => status(k) === 'bad') ? 'bad' : [r, ...kids].some(k => status(k) === 'warn') ? 'warn' : 'ok', u = r.users + kids.reduce((a, k) => a + k.users, 0), tp = r.tp + kids.reduce((a, k) => a + k.tp, 0);
      return `<tr class="grp"><td><button type="button" class="vo-exp" data-exp="${r.id}" aria-expanded="${open.has(r.id)}">${ic('chev-down')}${esc(r.n)}</button></td><td><span class="vo-tag st-${st}">${st === 'ok' ? 'يعمل' : st === 'warn' ? 'تحذير' : 'حرج'}</span></td><td><span class="num">${nf(tp, 1)} جيجابت/ث</span></td><td><span class="vo-tb"><i style="width:${(u / mx * 100).toFixed(0)}%"></i></span><b class="num">${nf(u)}</b></td></tr>` +
        (open.has(r.id) ? kids.map(k => `<tr class="sub" data-go="${k.id}"><td>${esc(k.n)}</td><td><span class="vo-tag st-${status(k)}">${status(k) === 'ok' ? 'يعمل' : status(k) === 'warn' ? 'تحذير' : 'حرج'}</span></td><td><span class="num">${nf(k.tp, 1)} جيجابت/ث</span></td><td><span class="vo-tb"><i style="width:${(k.users / mx * 100 * 2.4).toFixed(0)}%"></i></span><b class="num">${nf(k.users)}</b></td></tr>`).join('') : '');
    }).join('')}</tbody></table>`;
  }

  /* ---------- التكبير والتحريك ---------- */
  function fitScale() { const w = stage.clientWidth; return Math.min(1.4, (w - 8) / W); }
  function applyZoom() { const s = fitScale() * S.zoom; inner.style.transform = `translate(${S.panX}px,${S.panY}px) scale(${s})`; stage.style.height = Math.max(380, H * fitScale() + 8) + 'px'; $('#voZoom').textContent = Math.round(S.zoom * 100) + '%'; }
  $('#voZin').addEventListener('click', () => { S.zoom = Math.min(2, S.zoom + .15); applyZoom(); });
  $('#voZout').addEventListener('click', () => { S.zoom = Math.max(.6, S.zoom - .15); applyZoom(); });
  $('#voZfit').addEventListener('click', () => { S.zoom = 1; S.panX = S.panY = 0; applyZoom(); });
  let drag = null;
  stage.addEventListener('pointerdown', e => { if (e.target.closest('.vo-card,.vo-zoom')) return; drag = { x: e.clientX, y: e.clientY, px: S.panX, py: S.panY }; stage.setPointerCapture(e.pointerId); stage.classList.add('panning'); });
  stage.addEventListener('pointermove', e => { if (!drag) return; S.panX = drag.px + e.clientX - drag.x; S.panY = drag.py + e.clientY - drag.y; applyZoom(); });
  stage.addEventListener('pointerup', e => { if (drag && Math.abs(e.clientX - drag.x) + Math.abs(e.clientY - drag.y) < 4) select(null); drag = null; stage.classList.remove('panning'); });
  addEventListener('resize', applyZoom);

  /* ---------- التفاعل ---------- */
  function select(id) { S.sel = id; S.hl = null; $('#voAnswer').hidden = true; paintCards(); side(); }
  inner.addEventListener('click', e => { const c = e.target.closest('.vo-card'); if (c) select(S.sel === c.dataset.id ? null : c.dataset.id); });
  root.addEventListener('click', e => {
    const g = e.target.closest('[data-go]'); if (g) return select(g.dataset.go);
    if (e.target.closest('[data-clear]')) return select(null);
    const x = e.target.closest('[data-exp]'); if (x) { open.has(x.dataset.exp) ? open.delete(x.dataset.exp) : open.add(x.dataset.exp); return bottom(); }
    const a = e.target.closest('[data-act]'); if (a) {
      const n = byId(S.sel);
      if (a.dataset.act === 'restart') { n.alerts = Math.max(0, n.alerts - 3); n.health = Math.min(98, n.health + 6); toast(`أُعيد تشغيل «${n.n}»، وانخفضت التنبيهات.`); paintCards(); side(); bottom(); }
      if (a.dataset.act === 'isolate') toast(`عُزل «${n.n}» عن الشبكة مؤقتًا، وحُوّلت حركته إلى الأجهزة المجاورة.`);
      if (a.dataset.act === 'ask') ask(`ما سبب تنبيهات «${n.n}»؟`);
    }
    const gb = e.target.closest('[data-gate]'); if (gb) { S.gate = gb.dataset.gate; $$('[data-gate]').forEach(b => b.setAttribute('aria-pressed', String(b === gb))); S.sel = null; S.hl = null; build(); drawTree(); spawnPulses(); header(); side(); bottom(); }
  });
  root.addEventListener('keydown', e => { if (e.key === 'Escape') select(null); });
  const liveBtn = $('#voLive');
  const setLive = v => { S.live = v; liveBtn.setAttribute('aria-pressed', String(v)); liveBtn.innerHTML = `<i class="vo-livedot-i"></i>${v ? 'مباشر' : 'متوقف'}`; root.classList.toggle('is-paused', !v); };
  liveBtn.addEventListener('click', () => setLive(!S.live));
  const toast = msg => { let t = document.querySelector('.toast'); if (!t) { t = document.createElement('div'); t.className = 'toast'; t.setAttribute('role', 'status'); document.body.append(t); } t.innerHTML = `${ic('check')}<span>${esc(msg)}</span>`; requestAnimationFrame(() => t.classList.add('is-on')); clearTimeout(t._h); t._h = setTimeout(() => t.classList.remove('is-on'), 2600); };

  /* ---------- اسأل جيم ---------- */
  const ASKS = ['أي الأجهزة في حالة حرجة الآن؟', 'أين الضغط الأعلى على الشبكة؟', 'هل البوابات جاهزة لذروة الدخول؟'];
  $('#voChips').innerHTML = ASKS.map((q, i) => `<button type="button" class="vo-chip" data-ask="${i}">${q}</button>`).join('');
  $$('[data-ask]').forEach(b => b.addEventListener('click', () => ask(ASKS[+b.dataset.ask])));
  $('#voAsk').addEventListener('submit', e => { e.preventDefault(); const v = $('#voAskIn').value.trim(); if (v) ask(v); });
  let typing = null;
  const say = txt => { const A = $('#voAnswer'), el = $('p', A); A.hidden = false; clearInterval(typing); if (reduceMotion) { el.textContent = txt; return; } let i = 0; el.textContent = ''; typing = setInterval(() => { i += 3; el.textContent = txt.slice(0, i); if (i >= txt.length) clearInterval(typing); }, 18); };
  function ask(q) {
    $('#voAskIn').value = q;
    const crit = NODES.filter(n => status(n) === 'bad'), busy = [...NODES].filter(n => !n.root).sort((a, b) => b.users - a.users).slice(0, 3), gates = NODES.filter(n => n.kind === 'بوابات');
    let ids, a;
    if (/حرج|حرجة/.test(q)) { ids = crit.flatMap(pathTo); a = crit.length ? `${crit.length} أجهزة في حالة حرجة: ${crit.map(n => `«${n.n}» (${n.alerts} تنبيهًا)`).join('، ')}. أبدأ بأعلاها تنبيهًا وتحقق من حمل الاتصال فيها.` : 'لا توجد أجهزة في حالة حرجة الآن.'; }
    else if (/ضغط|حمل|الأعلى/.test(q)) { ids = busy.flatMap(pathTo); a = `أعلى ضغط على «${busy[0].n}» بـ${nf(busy[0].users)} متصل، يليه «${busy[1].n}» (${nf(busy[1].users)}). يُنصح بتوزيع جزء من الحركة على «${busy[2].n}».`; }
    else if (/بواب/.test(q)) { ids = gates.flatMap(pathTo); const low = gates.filter(g => g.health < 70); a = low.length ? `${low.map(g => `«${g.n}»`).join(' و')} صحتها أقل من 70%، ويُتوقع أن تبطئ الدخول في الذروة. الباقي جاهز.` : `مجموعتا البوابات جاهزتان: الصحة ${gates.map(g => nf(g.health) + '%').join(' و')}، ومعدل التدفق ضمن الطبيعي.`; }
    else if (S.sel) { const n = byId(S.sel); ids = pathTo(n); a = n.alerts ? `أغلب تنبيهات «${n.n}» سببها ارتفاع حمل الاتصال (${nf(n.users)} متصل) وكمون ${nf(n.lat)} مللي ث. إعادة التشغيل تخفّض التنبيهات مؤقتًا، والحل الدائم إضافة نقطة وصول مساندة.` : `«${n.n}» يعمل بشكل طبيعي بلا تنبيهات.`; }
    else { ids = []; a = 'جرّب أحد الأسئلة المقترحة، أو اختر جهازًا ثم اسأل عنه.'; }
    S.hl = ids.length ? new Set(ids) : null; if (!S.sel) side(); paintCards(); say(a);
  }
  $('#voAnsClear').addEventListener('click', () => { S.hl = null; $('#voAnswer').hidden = true; paintCards(); });

  /* ---------- البدء ---------- */
  build(); drawTree(); spawnPulses(); header(); side(); bottom(); applyZoom(); setLive(S.live);
  raf = requestAnimationFrame(frame);
})();
