/* ==========================================================
   جيم — العملاء المحتملون والطلبات الواردة (لوحة تفاعلية)
   مستوحاة من «CRM Dashboard – B2B SaaS» (Sunlead): فقاعات مصادر مُعبّأة،
   مؤشرات بتغير ملوّن، صفوف طلبات بأسهم تمرير، وخريطة نشاط الساعات.
   ========================================================== */
(function () {
  const root = document.getElementById('clRoot'); if (!root || !window.DK) return;
  const K = DK, $ = s => root.querySelector(s);
  const SRC = [['الموقع الإلكتروني', 45, '#DFC6AE'], ['التواصل الاجتماعي', 32, '#C9D6EA'], ['الإحالات', 10, '#C4DCC9'], ['الإعلانات', 7, '#B9DCD3'], ['أخرى', 6, '#EBCACA']];
  const STG = ['تفاوض', 'عرض مُرسل', 'قيد المراجعة', 'مكسوبة'];
  const TAG = { new: ['عميل جديد', 'ok'], pri: ['أولوية', 'bad'], ret: ['عائد', 'info'] };
  const S = { per: 30, src: null, tag: 'all', zoom: 1, menu: null };
  let REQ = [
    ['أبكس للأنظمة', 'نظام صلاحيات متعدد الأدوار لمنصة SaaS', 'جمال الفارس', 'تطوير المبيعات', 'new', 0, 0, '18 يونيو', 2, 4],
    ['فيلوس موبايل', 'لوحة متابعة لشبكة طاقة متجددة', 'كلارا عوض', 'مؤهِّلة عملاء', 'pri', 0, 1, '22 يونيو', 3, 11],
    ['هورايزن كلاود', 'إدارة بنية بيانات تحتية للخدمات', 'داود حسن', 'الرئيس التنفيذي', 'ret', 0, 0, '14 يونيو', 1, 6],
    ['مرسال للتقنية', 'تكامل بوابات الدفع مع الفوترة', 'سلمى يوسف', 'مديرة منتج', 'new', 0, 2, '11 يونيو', 0, 2],
    ['كريست فيو الصحية', 'مراجعة امتثال لمنصة الطب الاتصالي', 'إيلينا رستم', 'مديرة مبيعات', 'new', 1, 0, '02 يوليو', 1, 3],
    ['النقل الحضري', 'منصة سلاسل إمداد بتقارير لحظية', 'مروان رايد', 'مدير حسابات', 'pri', 1, 1, '29 يونيو', 4, 8],
    ['أفق الحلول', 'تحليلات تسويق متعددة القنوات', 'فرح الحسن', 'مسؤولة نمو', 'ret', 1, 3, '25 يونيو', 2, 5],
    ['نبراس التعليمية', 'نظام تعلم إلكتروني للشركات', 'طارق سعيد', 'مستشار', 'new', 2, 2, '20 يونيو', 0, 1],
    ['دار المعمار', 'بوابة عملاء للمشاريع الهندسية', 'منى حداد', 'مديرة عمليات', 'pri', 2, 0, '17 يونيو', 3, 9],
    ['سحابة الخليج', 'ترحيل بيانات العملاء للسحابة', 'زياد العلي', 'مدير تقنية', 'ret', 3, 1, '09 يونيو', 5, 12]
  ].map(([co, d, p, role, tag, st, src, dt, at, cm], id) => ({ id, co, d, p, role, tag, st, src, dt, at, cm }));

  function pack(rs) { const P = []; rs.forEach((r, i) => { if (!i) return P.push([0, 0, r]); let best = null;
    P.forEach(([ax, ay, ar]) => { for (let a = 0; a < 360; a += 6) { const t = a * Math.PI / 180, x = ax + Math.cos(t) * (ar + r + 4), y = ay + Math.sin(t) * (ar + r + 4);
      if (P.every(([bx, by, br]) => Math.hypot(x - bx, y - by) >= br + r + 3.5)) { const d = Math.hypot(x, y * 1.25); if (!best || d < best[3]) best = [x, y, r, d]; } } });
    P.push(best.slice(0, 3)); }); return P; }
  function packC() {
    const k = S.per / 30, rs = SRC.map(s => Math.sqrt(s[1]) * 13), P = pack(rs), xs = P.map(p => [p[0] - p[2], p[0] + p[2]]).flat(), ys = P.map(p => [p[1] - p[2], p[1] + p[2]]).flat();
    const x0 = Math.min(...xs) - 6, y0 = Math.min(...ys) - 6, w = Math.max(...xs) - x0 + 6, h = Math.max(...ys) - y0 + 6;
    $('#clPack').innerHTML = `<svg viewBox="${x0} ${y0} ${w} ${h}" class="dk-svg cl-bub" style="transform:scale(${S.zoom})" role="img" aria-label="العملاء الجدد حسب المصدر">${P.map(([x, y, r], i) => `<g class="bb${S.src === i ? ' on' : ''}${S.src != null && S.src !== i ? ' dim' : ''}" data-src="${i}" tabindex="0" role="button" aria-label="${SRC[i][0]} ${SRC[i][1]}%" style="--i:${i}"><circle cx="${x.toFixed(1)}" cy="${y.toFixed(1)}" r="${r.toFixed(1)}" fill="${SRC[i][2]}" data-tip="${SRC[i][0]}|${SRC[i][1]}% · ${Math.round(SRC[i][1] * 6.4 * k)} عميل"/><text x="${x.toFixed(1)}" y="${(y - 4).toFixed(1)}" class="n">${SRC[i][0]}</text><text x="${x.toFixed(1)}" y="${(y + 14).toFixed(1)}" class="p">${SRC[i][1]}%</text></g>`).join('')}</svg>`;
  }
  function kpis() {
    const k = S.per, r = K.rng('cl' + k), mk = (b, s) => Array.from({ length: 12 }, (_, i) => b + (r() - .5) * s + i * s * .03);
    const K3 = [['متوسط قيمة الطلب', K.num(148.5 * (k === 7 ? .96 : k === 90 ? 1.04 : 1), 'ر.س', 2), 5.2, 1, mk(140, 12)], ['معدل الإلغاء', K.pct(k === 7 ? 2.1 : k === 90 ? 1.6 : 1.8), -0.4, -1, mk(2, .5)], ['معدل التحويل', K.pct(k === 7 ? 3.9 : k === 90 ? 4.4 : 4.2), 1.1, 1, mk(4, .6)]];
    $('#clKpis').innerHTML = K3.map(([n, v, d, g, sp], i) => `<div class="dk-kpi" style="--i:${i}"><small>${n}</small><span class="row"><b>${v}</b><span class="dk-chip ${(d >= 0) === (g > 0) ? 'ok' : 'bad'}"><span class="num">${d > 0 ? '+' : '−'}${Math.abs(d)}%</span></span></span>${K.spark(sp, '#8A9A8E', { h: 26 })}</div>`).join('');
  }
  function req() {
    const vis = REQ.filter(q => (S.src == null || q.src === S.src) && (S.tag === 'all' || q.tag === S.tag));
    $('#clReq').innerHTML = STG.map((st, si) => { const cs = vis.filter(q => q.st === si);
      return `<div class="cl-row"><div class="cl-rh"><b>${st}</b><span class="dk-chip"><span class="num">${cs.length}</span></span><span class="sp"></span><button type="button" class="ib" data-scroll="${si}:-1" aria-label="السابق">${K.ic('chev-left', 'icon icon-sm flip')}</button><button type="button" class="ib" data-scroll="${si}:1" aria-label="التالي">${K.ic('chev-left')}</button></div>
        <div class="cl-track" id="clT${si}">${cs.length ? cs.map((q, i) => `<article class="cl-card" style="--i:${i}"><header><span class="dk-chip ${TAG[q.tag][1]}">${TAG[q.tag][0]}</span><span class="sp"></span><button type="button" class="mn" data-menu="${q.id}" aria-label="خيارات ${K.esc(q.co)}" aria-expanded="${S.menu === q.id}">⋯</button>${S.menu === q.id ? `<div class="pop" role="menu">${si < 3 ? `<button type="button" role="menuitem" data-move="${q.id}">نقل إلى «${STG[si + 1]}»</button>` : ''}<button type="button" role="menuitem" data-pri="${q.id}">${q.tag === 'pri' ? 'إلغاء الأولوية' : 'تمييز كأولوية'}</button></div>` : ''}</header>
          <b>${K.esc(q.co)}</b><p>${K.esc(q.d)}</p><span class="who"><span class="dk-av" style="--h:${q.id * 47 % 360}">${q.p[0]}</span><span><b>${K.esc(q.p)}</b><small>${K.esc(q.role)}</small></span></span>
          <footer><span>${K.ic('calendar')}${q.dt}</span><span class="sp"></span><span>${K.ic('link')}<span class="num">${q.at}</span></span><span>${K.ic('chat')}<span class="num">${q.cm}</span></span></footer></article>`).join('') : '<p class="dk-empty">لا طلبات بهذه التصفية</p>'}</div></div>`; }).join('');
  }
  function act() {
    const r = K.rng('act' + S.per), days = Math.min(S.per, 30), hrs = ['8ص', '9ص', '10ص', '11ص', '12م', '1م', '2م', '3م', '4م', '5م'];
    const v = hrs.map((_, h) => Array.from({ length: days }, (_, d) => { const we = (d % 7 === 5 || d % 7 === 6); return Math.round((we ? 1 : 4) * (1 + Math.sin(h / 2)) * (.5 + r())); }));
    $('#clAct').innerHTML = K.heat(hrs, Array.from({ length: days }, (_, d) => String(d + 1)), v, { hue: '120,96,72', show: false, fmt: x => x + ' تفاعل' });
  }
  function scope() { $('#clScope').innerHTML = S.src == null ? '' : `<span class="dk-chip info">المصدر: ${SRC[S.src][0]}<button type="button" data-clr aria-label="إلغاء">${K.ic('x')}</button></span>`; }
  function all() { packC(); kpis(); req(); act(); scope(); }

  root.addEventListener('click', e => {
    const t = e.target;
    const s = t.closest('[data-src]'); if (s) { const i = +s.dataset.src; S.src = S.src === i ? null : i; packC(); req(); return scope(); }
    if (t.closest('[data-clr]')) { S.src = null; packC(); req(); return scope(); }
    const m = t.closest('[data-menu]'); if (m) { const id = +m.dataset.menu; S.menu = S.menu === id ? null : id; return req(); }
    const mv = t.closest('[data-move]'); if (mv) { const q = REQ.find(x => x.id === +mv.dataset.move); q.st++; S.menu = null; K.toast(`نُقل «${q.co}» إلى «${STG[q.st]}»`); return req(); }
    const pr = t.closest('[data-pri]'); if (pr) { const q = REQ.find(x => x.id === +pr.dataset.pri); q.tag = q.tag === 'pri' ? 'new' : 'pri'; S.menu = null; return req(); }
    const sc = t.closest('[data-scroll]'); if (sc) { const [i, d] = sc.dataset.scroll.split(':'); $('#clT' + i).scrollBy({ left: -d * 240, behavior: K.RM ? 'auto' : 'smooth' }); return; }
    if (S.menu != null && !t.closest('.pop')) { S.menu = null; req(); }
  });
  $('#clZoom').addEventListener('change', e => { S.zoom = +e.target.value; packC(); });
  K.seg(root, 'per', v => { S.per = +v; kpis(); act(); packC(); });
  K.seg(root, 'tag', v => { S.tag = v; req(); });
  all();
})();
