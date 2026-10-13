/* ==========================================================
   جيم — إدارة أعمال المقاولات (لوحة تفاعلية، المحرك: construction، على dash-kit)
   مُعاد بناؤه كدالة تركيب قابلة لإعادة الاستخدام (window.JEEM_CONSTRUCTION_MOUNT) بدل IIFE
   ذاتي التشغيل — يقبل data حقيقية أو من مكتبة العرض التجريبي (construction-crm-demo.js).
   مواعيد اليوم تستخدم تواريخ ISO حقيقية (لا تاريخ ثابت كـ«18 يناير 2026»)؛ تبويبات حالة
   المواعيد (الإجمالي/قادمة/نشطة/مكتملة) تُحسب من قائمة المواعيد الفعلية نفسها بدل أرقام
   إجمالية منفصلة قد لا تطابقها — تجنّبًا لأي تناقض بين الرقم المعروض وما تعرضه القائمة.
   تحويل الرصيد الفوري فعل المستخدم الحالي نفسه، يُطبَّق على العرض لمدة الجلسة.
   ========================================================== */
(function () {
  function mount(root, data, opts = {}) {
    if (!root || !data || !window.DK) return;
    const K = DK, $ = s => root.querySelector(s);
    const S = { per: 7, req: null, quo: null, job: 0, tab: 'all', q: '', bank: 0 };
    const AST = { ok: ['مؤكد', 'ok', 'check'], go: ['قادم', 'info', 'clock'], act: ['جارٍ الآن', 'warn', 'play'], done: ['مكتمل', 'pur', 'check'] };

    const bankSel = $('#ccBank');
    if (bankSel) bankSel.innerHTML = (data.payoutAccounts || []).map((b, i) => `<option value="${i}">${K.esc(b.name)}</option>`).join('');
    if ($('#ccLastUpd') && data.lastUpdatedAt) {
      const days = Math.max(0, Math.floor((Date.now() - new Date(data.lastUpdatedAt)) / 86400000));
      $('#ccLastUpd').textContent = days === 0 ? 'آخر تحديث اليوم' : `آخر تحديث قبل ${K.nf(days)} ${days === 1 ? 'يوم' : 'أيام'}`;
    }

    function req() {
      const R = data.requests || { totalActivityPct: {}, categories: [] };
      const cats = R.categories;
      $('#ccReq').innerHTML = `<div class="cc-big"><b class="num">${R.totalActivityPct[S.per] ?? 0}%</b><small>إجمالي النشاط</small></div>
        <div class="cc-seg">${cats.map((c, i) => `<button type="button" data-req="${i}" class="${S.req != null && S.req !== i ? 'dim' : ''}" style="flex:${c.pct};--c:${c.color}" aria-pressed="${S.req === i}" data-tip="${K.esc(c.name)}|${c.pct}%"><i></i><span class="num">${c.pct}%</span></button>`).join('')}</div>
        <div class="cc-tiles">${cats.map((c, i) => `<button type="button" data-req="${i}" class="${S.req === i ? 'on' : ''}${S.req != null && S.req !== i ? ' dim' : ''}" style="--c:${c.color};--i:${i}" aria-pressed="${S.req === i}"><span class="ib">${K.ic(c.icon)}</span><b class="num">${S.per === 7 ? c.count7 : c.count30}</b><small>${K.esc(c.name)}</small></button>`).join('')}</div>`;
    }
    function quo() {
      const Q = (data.quotes && data.quotes[S.per]) || [];
      const tot = Q.reduce((a, q) => a + q.count, 0);
      $('#ccQuo').innerHTML = `<div class="cc-steps">${Q.map((q, i) => `<button type="button" data-quo="${i}" class="st${S.quo != null && S.quo !== i ? ' dim' : ''}${S.quo === i ? ' on' : ''}" style="--c:${q.color};--h:${30 + i * 26}%;--i:${i}" data-tip="${K.esc(q.name)}|${K.nf(q.count)} عرض · ${K.txt(q.valueK * 1000, 'ر.س')}"><span class="bub" style="background:${q.color}">${K.txt(q.valueK, 'ألف')}</span><span class="ln"></span><span class="bar"></span></button>`).join('')}<span class="add" aria-hidden="true">+</span></div>
        <p class="cc-tot"><b class="num">${K.nf(tot)}</b> إجمالي العروض المُنشأة</p>
        <div class="cc-chips">${Q.map((q, i) => `<button type="button" data-quo="${i}" aria-pressed="${S.quo === i}"><i style="background:${q.color}"></i>${K.esc(q.name)} · <span class="num">${K.nf(q.count)}</span></button>`).join('')}</div>`;
    }
    function job() {
      const J = data.jobs || [];
      const tot = J.reduce((a, j) => a + j.valueK, 0);
      $('#ccJob').innerHTML = `<div class="cc-dots">${J.map((j, i) => `<button type="button" data-job="${i}" class="grp${S.job === i ? ' on' : ''}" style="--c:${j.color}" aria-pressed="${S.job === i}"><span class="lb"><b class="num">${K.nf(j.count)}</b></span><span class="mx">${Array.from({ length: Math.min(60, Math.round(j.count / (i === 1 ? 2.2 : 1))) }, (_, k) => `<i style="--k:${k}"></i>`).join('')}</span><small>${K.esc(j.name)}</small><b>${K.money(j.valueK * 1000)}</b></button>`).join('')}</div>
        <div class="cc-total"><span class="ib">${K.ic('wallet')}</span>الإجمالي: <b>${K.money(tot * 1000)}</b><span class="sp"></span><span class="mut">تتبع الأعمال لحظيًا</span></div>`;
    }
    let APP = (data.appointments || []).map(a => ({ ...a }));
    function app() {
      const counts = { all: APP.length, go: 0, act: 0, done: 0 };
      const values = { all: 0, go: 0, act: 0, done: 0 };
      APP.forEach(a => { const k = a.status === 'ok' ? 'go' : a.status; if (counts[k] != null) { counts[k]++; values[k] += a.valueK; } values.all += a.valueK; });
      const T = [['all', 'الإجمالي'], ['go', 'قادمة'], ['act', 'نشطة'], ['done', 'مكتملة']];
      $('#ccTabs').innerHTML = T.map(([k, n]) => `<button type="button" role="tab" data-tab="${k}" aria-selected="${S.tab === k}"><small>${n} · <span class="num">${K.nf(counts[k])}</span></small><b>${K.money(values[k] * 1000)}</b></button>`).join('');
      const rows = APP.filter(a => (S.tab === 'all' || a.status === S.tab || (S.tab === 'go' && a.status === 'ok')) && (!S.q || a.name.includes(S.q)));
      $('#ccApp').innerHTML = rows.length ? `<table><thead><tr><th>العميل</th><th>التاريخ والوقت</th><th>الموقع</th><th>الحالة</th><th><span class="sr-only">إجراء</span></th></tr></thead><tbody>${rows.map(a => { const i = APP.indexOf(a), st = AST[a.status] || AST.go; return `<tr><td><span class="who"><span class="dk-av" style="--h:${i * 55}">${K.esc(a.name[0])}</span><span><b>${K.esc(a.name)}</b><small>${K.esc(a.code)}</small></span></span></td><td>${new Date(a.date).toLocaleDateString('ar-SA-u-ca-gregory-nu-latn', { day: 'numeric', month: 'long', year: 'numeric' })}<small class="mut" style="display:block">${K.esc(a.time)}</small></td><td>${K.esc(a.location)}</td><td><span class="dk-chip ${st[1]}">${K.ic(st[2])}${st[0]}</span></td><td>${a.status !== 'done' ? `<button type="button" class="dk-lnk" data-done="${i}">إنهاء</button>` : ''}</td></tr>`; }).join('')}</tbody></table>` : '<p class="dk-empty">لا مواعيد مطابقة</p>';
    }
    function pay() {
      const acc = (data.payoutAccounts || [])[S.bank] || { inTransit: 0, instantAvailable: 0, weeklyTransfers: { labels: [], vals: [] }, targetDaily: 0 };
      $('#ccPay').innerHTML = `<div class="cc-bal"><div><span class="ib" style="--c:#6A4FD8">${K.ic('database')}</span><small>في الطريق إلى البنك</small><b>${K.num(acc.inTransit, 'ر.س', 2)}</b></div><div><span class="ib" style="--c:#E8794A">${K.ic('spark')}</span><small>متاح للتحويل الفوري</small><b>${K.num(acc.instantAvailable, 'ر.س', 2)}</b><button type="button" class="dk-btn pri" data-payout ${acc.instantAvailable ? '' : 'disabled'}>حوّل الآن</button></div></div>
        <div class="cc-pb"><header><small>تحويلات الأسبوع</small><span class="dk-chip pur">من المستهدف <span class="num">${K.nf(acc.targetPctOfGoal ?? 0, 0)}%</span></span></header>${acc.weeklyTransfers.labels.length ? K.bars({ W: K.wOf($('#ccPay')), H: 150, labels: acc.weeklyTransfers.labels, series: [{ name: 'التحويلات', color: '#6A4FD8', vals: acc.weeklyTransfers.vals }], fmt: x => K.nf(x) + ' ر.س', target: acc.targetDaily, targetLabel: 'المستهدف اليومي' }) : ''}</div>`;
    }
    function all() { req(); quo(); job(); app(); pay(); }

    root.addEventListener('click', e => {
      const t = e.target, g = k => t.closest(`[data-${k}]`);
      let x;
      if ((x = g('req'))) { const i = +x.dataset.req; S.req = S.req === i ? null : i; return req(); }
      if ((x = g('quo'))) { const i = +x.dataset.quo; S.quo = S.quo === i ? null : i; return quo(); }
      if ((x = g('job'))) { S.job = +x.dataset.job; return job(); }
      if ((x = g('tab'))) { S.tab = x.dataset.tab; return app(); }
      if ((x = g('done'))) { APP[+x.dataset.done].status = 'done'; K.toast('أُغلق الموعد كمكتمل'); return app(); }
      if ((x = g('refresh'))) { x.classList.remove('spin'); void x.offsetWidth; x.classList.add('spin'); ({ req, quo, job })[x.dataset.refresh](); return; }
      if (g('payout')) {
        const acc = (data.payoutAccounts || [])[S.bank]; if (!acc) return;
        K.toast(`حُوّل ${K.nf(acc.instantAvailable, 2)} ر.س إلى الحساب`); acc.inTransit += acc.instantAvailable; acc.instantAvailable = 0; return pay();
      }
    });
    $('#ccQ').addEventListener('input', e => { S.q = e.target.value.trim(); app(); });
    bankSel?.addEventListener('change', e => { S.bank = +e.target.value; pay(); });
    K.seg(root, 'per', v => { S.per = +v; req(); quo(); });
    K.onResize(root, pay);

    all();
    return { destroy() {} };
  }

  window.JEEM_CONSTRUCTION_MOUNT = mount;
})();
