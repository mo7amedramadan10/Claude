/* ==========================================================
   جيم — العملاء المحتملون والطلبات الواردة (لوحة تفاعلية، المحرك: leads، على dash-kit)
   مُعاد بناؤه كدالة تركيب قابلة لإعادة الاستخدام (window.JEEM_LEADS_MOUNT) بدل IIFE ذاتي
   التشغيل ببيانات مولَّدة عشوائيًا بالكامل للمؤشرات والنشاط — يقبل data حقيقية أو من مكتبة
   العرض التجريبي (crm-leads-demo.js). خوارزمية تعبئة الفقاعات (pack) هندسة عرض بحتة تبقى في
   المحرك. مراحل خط سير الطلبات الأربع مفردات تصنيف ثابتة. نقل طلب أو تمييزه بأولوية أفعال
   المستخدم الحالي، تُطبَّق على العرض لمدة الجلسة.
   ========================================================== */
(function () {
  function mount(root, data, opts = {}) {
    if (!root || !data || !window.DK) return;
    const K = DK, $ = s => root.querySelector(s);
    const SRC = data.sources || [];
    const STG = ['تفاوض', 'عرض مُرسل', 'قيد المراجعة', 'مكسوبة'];
    const TAG = { new: ['عميل جديد', 'ok'], pri: ['أولوية', 'bad'], ret: ['عائد', 'info'] };
    const periods = data.periods && data.periods.length ? data.periods : [30];
    const S = { per: periods.includes(30) ? 30 : periods[0], src: null, tag: 'all', zoom: 1, menu: null };
    let REQ = (data.requests || []).map((r, id) => ({ ...r, id }));

    const perSeg = $('[data-seg="per"]');
    if (perSeg) perSeg.innerHTML = periods.map(p => `<button type="button" data-v="${p}" aria-pressed="${p === S.per}">${p} ${p === 7 ? 'أيام' : 'يومًا'}</button>`).join('');

    function pack(rs) {
      const P = [];
      rs.forEach((r, i) => {
        if (!i) return P.push([0, 0, r]);
        let best = null;
        P.forEach(([ax, ay, ar]) => { for (let a = 0; a < 360; a += 6) { const t = a * Math.PI / 180, x = ax + Math.cos(t) * (ar + r + 4), y = ay + Math.sin(t) * (ar + r + 4);
          if (P.every(([bx, by, br]) => Math.hypot(x - bx, y - by) >= br + r + 3.5)) { const d = Math.hypot(x, y * 1.25); if (!best || d < best[3]) best = [x, y, r, d]; } } });
        P.push(best.slice(0, 3));
      });
      return P;
    }
    function packC() {
      if (!SRC.length) { $('#clPack').innerHTML = '<p class="dk-empty">لا مصادر عملاء بعد.</p>'; return; }
      const rs = SRC.map(s => Math.sqrt(s.pct) * 13), P = pack(rs), xs = P.map(p => [p[0] - p[2], p[0] + p[2]]).flat(), ys = P.map(p => [p[1] - p[2], p[1] + p[2]]).flat();
      const x0 = Math.min(...xs) - 6, y0 = Math.min(...ys) - 6, w = Math.max(...xs) - x0 + 6, h = Math.max(...ys) - y0 + 6;
      $('#clPack').innerHTML = `<svg viewBox="${x0} ${y0} ${w} ${h}" class="dk-svg cl-bub" style="transform:scale(${S.zoom})" role="img" aria-label="العملاء الجدد حسب المصدر">${P.map(([x, y, r], i) => `<g class="bb${S.src === i ? ' on' : ''}${S.src != null && S.src !== i ? ' dim' : ''}" data-src="${i}" tabindex="0" role="button" aria-label="${K.esc(SRC[i].name)} ${SRC[i].pct}%" style="--i:${i}"><circle cx="${x.toFixed(1)}" cy="${y.toFixed(1)}" r="${r.toFixed(1)}" fill="${SRC[i].color}" data-tip="${K.esc(SRC[i].name)}|${SRC[i].pct}% · ${K.nf(SRC[i].count ?? 0)} عميل"/><text x="${x.toFixed(1)}" y="${(y - 4).toFixed(1)}" class="n">${K.esc(SRC[i].name)}</text><text x="${x.toFixed(1)}" y="${(y + 14).toFixed(1)}" class="p">${SRC[i].pct}%</text></g>`).join('')}</svg>`;
    }
    function kpis() {
      const K3 = data.kpis && data.kpis[S.per] || [];
      $('#clKpis').innerHTML = K3.map((k, i) => `<div class="dk-kpi" style="--i:${i}"><small>${K.esc(k.name)}</small><span class="row"><b>${k.value}</b><span class="dk-chip ${(k.deltaPct >= 0) === (k.good > 0) ? 'ok' : 'bad'}"><span class="num">${k.deltaPct > 0 ? '+' : '−'}${Math.abs(k.deltaPct)}%</span></span></span>${k.spark ? K.spark(k.spark, '#8A9A8E', { h: 26 }) : ''}</div>`).join('');
    }
    function req() {
      const vis = REQ.filter(q => (S.src == null || q.src === S.src) && (S.tag === 'all' || q.tag === S.tag));
      $('#clReq').innerHTML = STG.map((st, si) => {
        const cs = vis.filter(q => q.st === si);
        return `<div class="cl-row"><div class="cl-rh"><b>${st}</b><span class="dk-chip"><span class="num">${cs.length}</span></span><span class="sp"></span><button type="button" class="ib" data-scroll="${si}:-1" aria-label="السابق">${K.ic('chev-left', 'icon icon-sm flip')}</button><button type="button" class="ib" data-scroll="${si}:1" aria-label="التالي">${K.ic('chev-left')}</button></div>
          <div class="cl-track" id="clT${si}">${cs.length ? cs.map((q, i) => `<article class="cl-card" style="--i:${i}"><header><span class="dk-chip ${TAG[q.tag]?.[1] || ''}">${TAG[q.tag]?.[0] || ''}</span><span class="sp"></span><button type="button" class="mn" data-menu="${q.id}" aria-label="خيارات ${K.esc(q.co)}" aria-expanded="${S.menu === q.id}">⋯</button>${S.menu === q.id ? `<div class="pop" role="menu">${si < 3 ? `<button type="button" role="menuitem" data-move="${q.id}">نقل إلى «${STG[si + 1]}»</button>` : ''}<button type="button" role="menuitem" data-pri="${q.id}">${q.tag === 'pri' ? 'إلغاء الأولوية' : 'تمييز كأولوية'}</button></div>` : ''}</header>
            <b>${K.esc(q.co)}</b><p>${K.esc(q.d)}</p><span class="who"><span class="dk-av" style="--h:${q.id * 47 % 360}">${K.esc((q.p || '?')[0])}</span><span><b>${K.esc(q.p)}</b><small>${K.esc(q.role)}</small></span></span>
            <footer><span>${K.ic('calendar')}${new Date(q.dt).toLocaleDateString('ar-SA-u-ca-gregory-nu-latn', { day: 'numeric', month: 'long' })}</span><span class="sp"></span><span>${K.ic('link')}<span class="num">${q.at}</span></span><span>${K.ic('chat')}<span class="num">${q.cm}</span></span></footer></article>`).join('') : '<p class="dk-empty">لا طلبات بهذه التصفية</p>'}</div></div>`;
      }).join('');
    }
    function act() {
      const hrs = (data.activityHours && data.activityHours.length) ? data.activityHours : ['8ص', '9ص', '10ص', '11ص', '12م', '1م', '2م', '3م', '4م', '5م'];
      const matrix = (data.activityByPeriod && data.activityByPeriod[S.per]) || [];
      const days = matrix.length ? Array.from({ length: matrix[0].length }, (_, d) => String(d + 1)) : [];
      $('#clAct').innerHTML = matrix.length ? K.heat(hrs, days, matrix, { hue: '120,96,72', show: false, fmt: x => x + ' تفاعل' }) : '<p class="dk-empty">لا بيانات نشاط بعد.</p>';
    }
    function scope() { $('#clScope').innerHTML = S.src == null ? '' : `<span class="dk-chip info">المصدر: ${K.esc(SRC[S.src]?.name || '')}<button type="button" data-clr aria-label="إلغاء">${K.ic('x')}</button></span>`; }
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
    return { destroy() {} };
  }

  window.JEEM_LEADS_MOUNT = mount;
})();
