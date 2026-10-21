/* ==========================================================
   جيم — إدارة أعمال المقاولات (لوحة تفاعلية)
   مستوحاة من «Construction Management CRM Dashboard» (elixirfactory):
   شريط نسب مجزأ، عروض أسعار متدرجة بفقاعات قيم، مصفوفة نقاط للأعمال،
   مواعيد بتبويبات قيمية، ومدفوعات بتحويل فوري.
   ========================================================== */
(function () {
  const root = document.getElementById('ccRoot'); if (!root || !window.DK) return;
  const K = DK, $ = s => root.querySelector(s);
  const C = { pur: '#6A4FD8', grn: '#8DBF2E', org: '#E8794A' };
  const S = { per: 7, req: null, quo: null, job: 1, tab: 'all', q: '', bank: 0 };
  const sc = () => S.per === 7 ? 1 : 3.6;

  function req() {
    const v = [32, 41, 24].map(x => x), n = [24, 15, 5].map(x => Math.round(x * sc())), L = [['جديدة', C.pur, 'plus'], ['مكتملة', C.grn, 'check'], ['متأخرة', C.org, 'clock']];
    $('#ccReq').innerHTML = `<div class="cc-big"><b class="num">${S.per === 7 ? 64 : 71}%</b><small>إجمالي النشاط</small></div>
      <div class="cc-seg">${v.map((p, i) => `<button type="button" data-req="${i}" class="${S.req != null && S.req !== i ? 'dim' : ''}" style="flex:${p};--c:${L[i][1]}" aria-pressed="${S.req === i}" data-tip="${L[i][0]}|${p}%"><i></i><span class="num">${p}%</span></button>`).join('')}</div>
      <div class="cc-tiles">${L.map(([nm, c, ic], i) => `<button type="button" data-req="${i}" class="${S.req === i ? 'on' : ''}${S.req != null && S.req !== i ? ' dim' : ''}" style="--c:${c};--i:${i}" aria-pressed="${S.req === i}"><span class="ib">${K.ic(ic)}</span><b class="num">${n[i]}</b><small>${nm}</small></button>`).join('')}</div>`;
  }
  function quo() {
    const Q = [['معتمدة', 16, 17, C.org], ['مطلوبة', 9, 11, C.grn], ['مسودة', 11, 20, C.pur]], tot = Q.reduce((a, q) => a + q[1], 0);
    $('#ccQuo').innerHTML = `<div class="cc-steps">${Q.map(([n, c, v, col], i) => `<button type="button" data-quo="${i}" class="st${S.quo != null && S.quo !== i ? ' dim' : ''}${S.quo === i ? ' on' : ''}" style="--c:${col};--h:${30 + i * 26}%;--i:${i}" data-tip="${n}|${Math.round(c * sc())} عرض · ${K.txt(v * 1000 * sc(), 'ر.س')}"><span class="bub" style="background:${col}">${K.txt(v * sc(), 'ألف')}</span><span class="ln"></span><span class="bar"></span></button>`).join('')}<span class="add" aria-hidden="true">+</span></div>
      <p class="cc-tot"><b class="num">${Math.round(tot * sc())}</b> إجمالي العروض المُنشأة</p>
      <div class="cc-chips">${Q.map(([n, c, , col], i) => `<button type="button" data-quo="${i}" aria-pressed="${S.quo === i}"><i style="background:${col}"></i>${n} · <span class="num">${Math.round(c * sc())}</span></button>`).join('')}</div>`;
  }
  function job() {
    const J = [['تتطلب إجراء', 57, 76, C.grn], ['بانتظار الفوترة', 128, 257, C.pur], ['نشطة', 35, 34, C.org]], tot = J.reduce((a, j) => a + j[2], 0);
    $('#ccJob').innerHTML = `<div class="cc-dots">${J.map(([n, c, v, col], i) => `<button type="button" data-job="${i}" class="grp${S.job === i ? ' on' : ''}" style="--c:${col}" aria-pressed="${S.job === i}"><span class="lb"><b class="num">${c}</b></span><span class="mx">${Array.from({ length: Math.min(60, Math.round(c / (i === 1 ? 2.2 : 1))) }, (_, k) => `<i style="--k:${k}"></i>`).join('')}</span><small>${n}</small><b>${K.money(v * 1000)}</b></button>`).join('')}</div>
      <div class="cc-total"><span class="ib">${K.ic('wallet')}</span>الإجمالي: <b>${K.money(tot * 1000)}</b><span class="sp"></span><span class="mut">تتبع الأعمال لحظيًا</span></div>`;
  }
  const APP = [['محمد الجهني', 'C-001', '18 يناير 2026', '9:00 ص', 'اجتماع عبر الإنترنت', 'ok', 4200], ['نوال الغامدي', 'C-014', '18 يناير 2026', '10:30 ص', 'موقع العميل – الملقا', 'go', 3100], ['عادل البقمي', 'C-022', '18 يناير 2026', '12:00 م', 'المكتب الرئيسي', 'act', 2600], ['ريم العمري', 'C-031', '18 يناير 2026', '1:30 م', 'موقع المشروع – النرجس', 'go', 5400], ['تركي الدوسري', 'C-040', '18 يناير 2026', '3:00 م', 'اجتماع عبر الإنترنت', 'act', 1500], ['ليان القحطاني', 'C-047', '18 يناير 2026', '4:30 م', 'موقع العميل – حطين', 'ok', 3900]];
  const AST = { ok: ['مؤكد', 'ok', 'check'], go: ['قادم', 'info', 'clock'], act: ['جارٍ الآن', 'warn', 'play'], done: ['مكتمل', 'pur', 'check'] };
  function app() {
    const T = [['all', 'الإجمالي', 30, 34], ['go', 'قادمة', 13, 16], ['act', 'نشطة', 5, 4], ['done', 'مكتملة', 12, 13]];
    $('#ccTabs').innerHTML = T.map(([k, n, c, v]) => `<button type="button" role="tab" data-tab="${k}" aria-selected="${S.tab === k}"><small>${n} · <span class="num">${c}</span></small><b>${K.money(v * 1000)}</b></button>`).join('');
    const rows = APP.filter(a => (S.tab === 'all' || a[5] === S.tab || (S.tab === 'go' && a[5] === 'ok')) && (!S.q || a[0].includes(S.q)));
    $('#ccApp').innerHTML = rows.length ? `<table><thead><tr><th>العميل</th><th>التاريخ والوقت</th><th>الموقع</th><th>الحالة</th><th><span class="sr-only">إجراء</span></th></tr></thead><tbody>${rows.map(a => { const i = APP.indexOf(a), st = AST[a[5]]; return `<tr><td><span class="who"><span class="dk-av" style="--h:${i * 55}">${a[0][0]}</span><span><b>${a[0]}</b><small>${a[1]}</small></span></span></td><td>${a[2]}<small class="mut" style="display:block">${a[3]}</small></td><td>${a[4]}</td><td><span class="dk-chip ${st[1]}">${K.ic(st[2])}${st[0]}</span></td><td>${a[5] !== 'done' ? `<button type="button" class="dk-lnk" data-done="${i}">إنهاء</button>` : ''}</td></tr>`; }).join('')}</tbody></table>` : '<p class="dk-empty">لا مواعيد مطابقة</p>';
  }
  const BAL = [[4291.56, 1992.12], [6120.4, 845.9]];
  function pay() {
    const [h, inst] = BAL[S.bank], r = K.rng('pay' + S.bank), days = ['السبت', 'الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة'], v = days.map(() => Math.round(400 + r() * 1400));
    $('#ccPay').innerHTML = `<div class="cc-bal"><div><span class="ib" style="--c:${C.pur}">${K.ic('database')}</span><small>في الطريق إلى البنك</small><b>${K.num(h, 'ر.س', 2)}</b></div><div><span class="ib" style="--c:${C.org}">${K.ic('spark')}</span><small>متاح للتحويل الفوري</small><b>${K.num(inst, 'ر.س', 2)}</b><button type="button" class="dk-btn pri" data-payout ${inst ? '' : 'disabled'}>حوّل الآن</button></div></div>
      <div class="cc-pb"><header><small>تحويلات الأسبوع</small><span class="dk-chip pur">من المستهدف <span class="num">60%</span></span></header>${K.bars({ W: K.wOf($('#ccPay')), H: 150, labels: days, series: [{ name: 'التحويلات', color: C.pur, vals: v }], fmt: x => K.nf(x) + ' ر.س', target: 1500, targetLabel: 'المستهدف اليومي' })}</div>`;
  }
  function all() { req(); quo(); job(); app(); pay(); }
  root.addEventListener('click', e => {
    const t = e.target, g = (k) => t.closest(`[data-${k}]`);
    let x;
    if ((x = g('req'))) { const i = +x.dataset.req; S.req = S.req === i ? null : i; return req(); }
    if ((x = g('quo'))) { const i = +x.dataset.quo; S.quo = S.quo === i ? null : i; return quo(); }
    if ((x = g('job'))) { S.job = +x.dataset.job; return job(); }
    if ((x = g('tab'))) { S.tab = x.dataset.tab; return app(); }
    if ((x = g('done'))) { APP[+x.dataset.done][5] = 'done'; K.toast('أُغلق الموعد كمكتمل'); return app(); }
    if ((x = g('refresh'))) { const k = x.dataset.refresh; x.classList.remove('spin'); void x.offsetWidth; x.classList.add('spin'); ({ req, quo, job })[k](); return; }
    if (g('payout')) { const b = BAL[S.bank]; K.toast(`حُوّل ${K.nf(b[1], 2)} ر.س إلى الحساب`); b[0] += b[1]; b[1] = 0; return pay(); }
  });
  $('#ccQ').addEventListener('input', e => { S.q = e.target.value.trim(); app(); });
  $('#ccBank').addEventListener('change', e => { S.bank = +e.target.value; pay(); });
  K.seg(root, 'per', v => { S.per = +v; req(); quo(); });
  K.onResize(root, pay);
  all();
})();
