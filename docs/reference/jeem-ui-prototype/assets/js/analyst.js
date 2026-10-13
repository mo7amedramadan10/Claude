/* ==========================================================
   جيم — المحلل الذكي (صفحة داخل المشروع، منفصلة عن لوحات المتابعة)
   المحادثة (يسار) تقود مساحة التحليل (يمين):
   - كل سؤال يمر بمراحل ظاهرة: فهم السؤال ← اختيار المصادر ← كتابة الاستعلام ← التحقق ← الصياغة.
   - كل إجابة لها حالة توثيق: «موثّقة» (طابقت إجماليات المصدر) أو «غير موثّقة» مع السبب وزر «تحقق الآن».
   - مساحة التحليل: ملخص، جدول قابل للبحث والفرز والتنزيل أو رسم، ومصادر البيانات والاستعلام،
     وأزرار تحليل جاهزة: لخّص / اشرح أهم المقارنات / صِف الأفضل، مع رسم مقارنة تفاعلي.
   البيانات تجريبية؛ في الإنتاج يستبدل الذكاء الاصطناعي السيناريوهات الجاهزة بالاستعلام الفعلي على مصادر المشروع.
   ========================================================== */
(function () {
  const root = document.getElementById('anRoot'); if (!root) return;
  const $ = (s, el = root) => el.querySelector(s), $$ = (s, el = root) => [...el.querySelectorAll(s)];
  const esc = s => String(s).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
  const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
  const nf = (v, d = 0) => Number(v).toLocaleString('en-US', { maximumFractionDigits: d, minimumFractionDigits: d });
  const sar = v => v >= 1e6 ? nf(v / 1e6, 2) + ' م' : nf(v / 1e3) + ' ألف';
  const norm = s => s.replace(/[أإآ]/g, 'ا').replace(/ة/g, 'ه').replace(/ى/g, 'ي');
  const reduceMotion = matchMedia('(prefers-reduced-motion: reduce)').matches;
  const COL = ['#127DBD', '#00BAB3', '#6E62B0', '#D9822B', '#28B6E6'];

  /* ---------- مصادر المشروع ---------- */
  const SOURCES = [
    { id: 'erp', n: 'نظام المبيعات (ERP)', t: 'sales_orders · order_lines', ic: 'server', fresh: 'قبل 4 دقائق', on: true },
    { id: 'crm', n: 'إدارة علاقات العملاء (CRM)', t: 'customers · opportunities', ic: 'users', fresh: 'قبل 3 أيام', on: true, stale: true },
    { id: 'tgt', n: 'المستهدفات 2026.xlsx', t: 'targets', ic: 'sheet', fresh: 'رُفع 12 سبتمبر', on: false },
    { id: 'inv', n: 'المخزون (WMS)', t: 'stock_levels', ic: 'layers', fresh: 'قبل 10 دقائق', on: false }
  ];

  /* ---------- بيانات تجريبية ---------- */
  let sd = 2026; const rnd = () => (sd = (sd * 16807) % 2147483647) / 2147483647;
  const PRODUCTS = [['لابتوب برو 14', 'أجهزة', 6200], ['لابتوب إير 13', 'أجهزة', 4300], ['شاشة 27 بوصة', 'أجهزة', 1450], ['طابعة ليزر ملونة', 'أجهزة', 2100], ['جهاز لوحي 11', 'أجهزة', 2600], ['خادم مكتبي صغير', 'أجهزة', 9800], ['جهاز عرض للقاعات', 'أجهزة', 3400],
    ['لوحة مفاتيح لاسلكية', 'إكسسوارات', 240], ['فأرة لاسلكية', 'إكسسوارات', 140], ['سماعة رأس للاجتماعات', 'إكسسوارات', 520], ['حقيبة لابتوب', 'إكسسوارات', 310], ['شاحن سريع 100 واط', 'إكسسوارات', 260], ['كاميرا ويب 4K', 'إكسسوارات', 690], ['ذاكرة خارجية 1 تيرابايت', 'إكسسوارات', 450], ['موزع منافذ USB-C', 'إكسسوارات', 380]]
    .map(([n, c, p], i) => { const q = Math.round((c === 'أجهزة' ? 900 : 3800) * (.4 + rnd() * 1.2)); return { id: 'P' + (i + 1), n, cat: c, price: p, qty: q, rev: q * p, margin: Math.round((c === 'أجهزة' ? 14 : 31) + rnd() * 14), growth: Math.round((rnd() - .25) * 46), central: Math.round(30 + rnd() * 35), stock: Math.round(20 + rnd() * 75), rating: +(3.6 + rnd() * 1.3).toFixed(1) }; });
  const REPS = ['سارة العتيبي', 'محمد الحربي', 'نورة الشمري', 'يوسف الغامدي', 'ريم القحطاني', 'فهد الدوسري', 'خالد الزهراني', 'منى السبيعي'].map((n, i) => { const t = Math.round(2.2e6 + rnd() * 2.4e6), a = Math.round(t * (.68 + rnd() * .5)); return { id: 'S' + (i + 1), n, region: ['الوسطى', 'الغربية', 'الشرقية', 'الوسطى', 'الغربية', 'الشرقية', 'الجنوبية', 'الوسطى'][i], target: t, actual: a, deals: Math.round(18 + rnd() * 40), win: Math.round(22 + rnd() * 25), cycle: Math.round(24 + rnd() * 40) }; });
  const REGIONS = ['الوسطى', 'الغربية', 'الشرقية', 'الجنوبية', 'الشمالية'].map((n, i) => { const r = Math.round([9.4, 7.1, 6.3, 2.8, 1.6][i] * 1e6 * (.9 + rnd() * .2)); return { id: 'G' + (i + 1), n, rev: r, orders: Math.round(r / (2400 + rnd() * 1800)), growth: Math.round((rnd() - .2) * 30), customers: Math.round(r / 52000) }; });
  const CUSTOMERS = ['مجموعة الخليج القابضة', 'شركة الحلول المتقدمة', 'مؤسسة التوريد الأولى', 'الشركة الوطنية للخدمات', 'مجموعة الفلاح', 'شركة التقنية الرقمية', 'مدارس المستقبل الأهلية', 'مستشفى الرعاية', 'شركة البناء الحديث', 'جامعة الأمير الأهلية'].map((n, i) => { const r = Math.round((1.9 - i * .13) * 1e6 * (.85 + rnd() * .3)); return { id: 'C' + (i + 1), n, region: REGIONS[i % 4].n, rev: r, orders: Math.round(12 + rnd() * 50), last: Math.round(2 + rnd() * 80), late: Math.round(rnd() * 4) }; });

  /* ---------- مقاييس المقارنة (0–100) ---------- */
  const PM = [['الاستقرار', p => 100 - Math.abs(p.growth) * 1.4], ['هامش الربح', p => p.margin * 2.2], ['النمو', p => 50 + p.growth * 1.6], ['انتشار المناطق', p => 100 - Math.abs(p.central - 40) * 1.6], ['مخاطر المخزون', p => 100 - p.stock]];
  const RM = [['تحقيق المستهدف', r => r.actual / r.target * 80], ['نسبة الفوز', r => r.win * 2], ['عدد الصفقات', r => r.deals * 1.6], ['سرعة الإغلاق', r => 110 - r.cycle * 1.3], ['متوسط الصفقة', r => r.actual / r.deals / 1600]];
  const clamp = v => Math.max(4, Math.min(100, Math.round(v)));

  /* ---------- السيناريوهات (ما سيولّده الذكاء الاصطناعي من الاستعلام الفعلي) ---------- */
  const SC = {
    products: () => ({
      title: 'منتجات الأجهزة والإكسسوارات', badge: 'منتجات', src: ['erp', 'inv'], verified: true,
      answer: `يوجد <b>${PRODUCTS.length} منتجًا</b> في الفئتين: ${PRODUCTS.filter(p => p.cat === 'أجهزة').length} أجهزة و${PRODUCTS.filter(p => p.cat === 'إكسسوارات').length} إكسسوارات. أعلاها إيرادًا ${links(top(PRODUCTS, 'rev', 3))}.`,
      summary: `هذه قائمة بكل منتجات الأجهزة والإكسسوارات المبيعة منذ بداية 2026 مع أهم مؤشراتها. إجمالي إيرادها ${sar(sum(PRODUCTS, 'rev'))} ر.س، وتستحوذ الأجهزة على ${Math.round(sum(PRODUCTS.filter(p => p.cat === 'أجهزة'), 'rev') / sum(PRODUCTS, 'rev') * 100)}% منه رغم أن الإكسسوارات أعلى هامشًا في المتوسط.`,
      cols: [['المنتج', 'n', 'l'], ['الفئة', 'cat', 't'], ['الكمية', 'qty', 'n'], ['الإيراد (ر.س)', 'rev', 'sar'], ['الهامش', 'margin', 'pct'], ['النمو', 'growth', 'dp']], rows: PRODUCTS, key: 'rev',
      sql: `SELECT p.name, p.category, SUM(l.qty) AS qty, SUM(l.qty * l.unit_price) AS revenue,\n       AVG(l.margin_pct) AS margin, dbo.yoy_growth(p.id) AS growth\nFROM sales.order_lines l JOIN sales.products p ON p.id = l.product_id\nWHERE p.category IN (N'أجهزة', N'إكسسوارات') AND l.order_date >= '2026-01-01'\nGROUP BY p.id, p.name, p.category ORDER BY revenue DESC;`,
      metrics: PM, cmp: top(PRODUCTS, 'rev', 3)
    }),
    growth: () => { const L = PRODUCTS.filter(p => p.cat === 'أجهزة').sort((a, b) => b.growth - a.growth).slice(0, 3); return {
      title: 'الأجهزة الأعلى نموًا في المنطقة الوسطى', badge: 'منتجات', src: ['erp', 'crm'], verified: false, why: 'بيانات العملاء من CRM لم تُحدَّث منذ 3 أيام، وقد تتغير حصة المنطقة الوسطى بعد المزامنة.',
      answer: `هناك <b>3 أجهزة</b> نموها الأعلى في المنطقة الوسطى: ${links(L)}.`,
      summary: `قارنت مبيعات الأجهزة في المنطقة الوسطى بالفترة نفسها من العام الماضي. أعلى نمو لـ«${L[0].n}» (+${L[0].growth}%)، وتمثّل الوسطى ${L[0].central}% من مبيعاته. انتبه: حصص المناطق مأخوذة من CRM الذي لم يُحدَّث منذ 3 أيام.`,
      cols: [['المنتج', 'n', 'l'], ['النمو', 'growth', 'dp'], ['حصة الوسطى', 'central', 'pct'], ['الإيراد (ر.س)', 'rev', 'sar'], ['الهامش', 'margin', 'pct'], ['التقييم', 'rating', 'r']], rows: L, key: 'growth',
      sql: `SELECT p.name, dbo.yoy_growth(p.id, N'الوسطى') AS growth, r.share AS central_share, SUM(l.qty*l.unit_price) AS revenue\nFROM sales.order_lines l JOIN sales.products p ON p.id = l.product_id\nJOIN crm.customers c ON c.id = l.customer_id\nCROSS APPLY dbo.region_share(p.id, N'الوسطى') r\nWHERE p.category = N'أجهزة' AND c.region = N'الوسطى'\nGROUP BY p.id, p.name, r.share ORDER BY growth DESC OFFSET 0 ROWS FETCH NEXT 3 ROWS ONLY;`,
      metrics: PM, cmp: L }; },
    reps: () => ({
      title: 'أداء المناديب في الربع الثالث', badge: 'فريق المبيعات', src: ['erp', 'tgt'], verified: true,
      answer: `حقق <b>${REPS.filter(r => r.actual >= r.target).length} من ${REPS.length} مناديب</b> مستهدفهم في الربع الثالث. الأعلى تحقيقًا ${links(top(REPS, r => r.actual / r.target, 3))}.`,
      summary: `مقارنة المحقق بالمستهدف لكل مندوب في الربع الثالث 2026. إجمالي المحقق ${sar(sum(REPS, 'actual'))} ر.س من مستهدف ${sar(sum(REPS, 'target'))} ر.س (${Math.round(sum(REPS, 'actual') / sum(REPS, 'target') * 100)}%). المستهدفات من ملف «المستهدفات 2026».`,
      cols: [['المندوب', 'n', 'l'], ['المنطقة', 'region', 't'], ['المستهدف', 'target', 'sar'], ['المحقق', 'actual', 'sar'], ['التحقيق', r => Math.round(r.actual / r.target * 100), 'pct'], ['نسبة الفوز', 'win', 'pct'], ['دورة البيع (يوم)', 'cycle', 'n']], rows: REPS, key: 'actual',
      sql: `SELECT s.rep_name, s.region, t.q3_target, SUM(o.total) AS actual, AVG(o.win_rate) AS win_rate, AVG(o.cycle_days) AS cycle\nFROM sales.orders o JOIN sales.reps s ON s.id = o.rep_id\nJOIN xlsx.targets t ON t.rep_id = s.id\nWHERE o.order_date BETWEEN '2026-07-01' AND '2026-09-30'\nGROUP BY s.rep_name, s.region, t.q3_target ORDER BY actual DESC;`,
      metrics: RM, cmp: top(REPS, r => r.actual / r.target, 3)
    }),
    regions: () => ({
      title: 'المبيعات حسب المنطقة', badge: 'مناطق', src: ['erp'], verified: true,
      answer: `المنطقة <b>الوسطى</b> الأعلى إيرادًا بـ${sar(REGIONS[0].rev)} ر.س، يليها ${links(REGIONS.slice(1, 3))}.`,
      summary: `توزيع إيرادات 2026 على المناطق الخمس. الوسطى تمثل ${Math.round(REGIONS[0].rev / sum(REGIONS, 'rev') * 100)}% من الإجمالي، وأسرع المناطق نموًا «${top(REGIONS, 'growth', 1)[0].n}» (+${top(REGIONS, 'growth', 1)[0].growth}%).`,
      cols: [['المنطقة', 'n', 'l'], ['الإيراد (ر.س)', 'rev', 'sar'], ['الطلبات', 'orders', 'n'], ['العملاء', 'customers', 'n'], ['النمو', 'growth', 'dp']], rows: REGIONS, key: 'rev',
      sql: `SELECT c.region, SUM(o.total) AS revenue, COUNT(DISTINCT o.id) AS orders, COUNT(DISTINCT o.customer_id) AS customers,\n       dbo.yoy_growth_region(c.region) AS growth\nFROM sales.orders o JOIN crm.customers c ON c.id = o.customer_id\nWHERE o.order_date >= '2026-01-01' GROUP BY c.region ORDER BY revenue DESC;`,
      metrics: [['حجم الإيراد', g => g.rev / 1e5], ['النمو', g => 50 + g.growth * 2], ['عدد العملاء', g => g.customers * .6], ['متوسط الطلب', g => g.rev / g.orders / 60], ['الاستقرار', g => 100 - Math.abs(g.growth) * 2]], cmp: REGIONS.slice(0, 3)
    }),
    customers: () => ({
      title: 'أكبر العملاء وتأخر الطلبات', badge: 'عملاء', src: ['erp', 'crm'], verified: false, why: 'بيانات CRM عمرها 3 أيام؛ «آخر طلب» قد لا يشمل الطلبات الأحدث.',
      answer: `أكبر 10 عملاء يمثلون ${Math.round(sum(CUSTOMERS, 'rev') / sum(REGIONS, 'rev') * 100)}% من الإيراد. لدى <b>${CUSTOMERS.filter(c => c.late).length} منهم</b> طلبات متأخرة، أبرزهم ${links(top(CUSTOMERS, 'late', 2))}.`,
      summary: `قائمة بأكبر 10 عملاء حسب الإيراد مع عدد الطلبات وآخر طلب والطلبات المتأخرة. العملاء الذين لم يطلبوا منذ أكثر من 60 يومًا يحتاجون متابعة: ${CUSTOMERS.filter(c => c.last > 60).map(c => '«' + c.n + '»').join('، ') || 'لا يوجد'}.`,
      cols: [['العميل', 'n', 'l'], ['المنطقة', 'region', 't'], ['الإيراد (ر.س)', 'rev', 'sar'], ['الطلبات', 'orders', 'n'], ['آخر طلب (يوم)', 'last', 'n'], ['متأخرة', 'late', 'n']], rows: CUSTOMERS, key: 'rev',
      sql: `SELECT TOP 10 c.name, c.region, SUM(o.total) AS revenue, COUNT(o.id) AS orders,\n       DATEDIFF(day, MAX(o.order_date), GETDATE()) AS days_since_last, SUM(CASE WHEN o.is_late = 1 THEN 1 ELSE 0 END) AS late\nFROM sales.orders o JOIN crm.customers c ON c.id = o.customer_id\nGROUP BY c.name, c.region ORDER BY revenue DESC;`,
      metrics: [['حجم الإيراد', c => c.rev / 2.2e4], ['تكرار الطلب', c => c.orders * 1.7], ['النشاط الحديث', c => 100 - c.last], ['الالتزام بالتسليم', c => 100 - c.late * 22], ['النمو المتوقع', c => 40 + (c.orders - c.last / 3)]], cmp: CUSTOMERS.slice(0, 3)
    })
  };
  function top(L, k, n) { const f = typeof k === 'function' ? k : x => x[k]; return [...L].sort((a, b) => f(b) - f(a)).slice(0, n); }
  function sum(L, k) { return L.reduce((a, x) => a + x[k], 0); }
  function links(L) { return L.map(x => `<button type="button" class="an-ent" data-ent="${x.id}">${esc(x.n)}</button>`).join('، ').replace(/، ([^،]*)$/, ' و$1'); }
  const route = q => { const n = norm(q); return /مندوب|مناديب|فريق|مستهدف/.test(n) ? 'reps' : /منطق|مناطق/.test(n) && !/نمو/.test(n) ? 'regions' : /عميل|عملاء|متاخر/.test(n) ? 'customers' : /نمو|الاعلي نمو|الوسطي/.test(n) ? 'growth' : /منتج|اجهز|اكسسوار/.test(n) ? 'products' : null; };

  /* ---------- الحالة ---------- */
  const S = { results: [], cur: -1, view: 'table', sort: null, q: '', tab: 'compare', cmpSel: 0, busy: false };
  const STEPS = ['فهم السؤال', 'اختيار المصادر', 'كتابة الاستعلام', 'التحقق من الأرقام', 'صياغة الإجابة'];

  /* ---------- المحادثة ---------- */
  const thread = $('#anThread');
  function addUser(q) { thread.insertAdjacentHTML('beforeend', `<div class="an-u"><p>${esc(q)}</p></div>`); scroll(); }
  function addAI(i) {
    const R = S.results[i];
    const el = document.createElement('div'); el.className = 'an-a'; el.dataset.i = i;
    el.innerHTML = `<span class="an-mark"></span><div class="an-body"><div class="an-steps"></div><div class="an-ans" hidden></div></div>`;
    thread.append(el); scroll();
    return el;
  }
  function renderAI(el, R, i) {
    $('.an-mark', el).className = `an-mark ${R.verified ? 'ok' : 'warn'}`; $('.an-mark', el).innerHTML = R.verified ? ic('check') : '!';
    $('.an-steps', el).innerHTML = `<details><summary>${ic('check')}تمّ في ${R.secs} ثوانٍ · ${R.src.length === 1 ? 'مصدر واحد' : R.src.length + ' مصادر'}</summary><ol>${STEPS.map(s => `<li>${ic('check')}${s}</li>`).join('')}</ol></details>`;
    const a = $('.an-ans', el); a.hidden = false;
    a.innerHTML = `${R.verified ? '' : `<span class="an-unv">${ic('info')}غير موثّقة</span>`}<p class="an-lead">${R.answer}</p>
      <div class="an-foot"><button type="button" class="an-open" data-open="${i}">${ic('layers')}عرض في مساحة التحليل</button><span class="an-srcs">${R.src.map(id => `<span>${ic(SOURCES.find(s => s.id === id).ic)}${SOURCES.find(s => s.id === id).n}</span>`).join('')}</span></div>`;
  }
  const scroll = () => thread.scrollTo({ top: thread.scrollHeight, behavior: reduceMotion ? 'auto' : 'smooth' });

  function run(q, scKey, instant) {
    if (S.busy) return; const key = scKey || route(q);
    addUser(q);
    if (!key) { thread.insertAdjacentHTML('beforeend', `<div class="an-a"><span class="an-mark warn">؟</span><div class="an-body"><p class="an-lead">لم أجد في مصادر المشروع ما يجيب عن هذا السؤال بدقة. جرّب السؤال عن المنتجات أو المناطق أو المناديب أو العملاء، أو أضف مصدرًا يحتوي البيانات المطلوبة.</p></div></div>`); return scroll(); }
    const R = Object.assign(SC[key](), { q, scn: key, secs: 3 + Math.round(Math.random() * 4) });
    S.results.push(R); const i = S.results.length - 1;
    const el = addAI(i);
    if (instant) { renderAI(el, R, i); return; }
    S.busy = true; $('#anSend').disabled = true;
    const st = $('.an-steps', el); let k = 0;
    const paint = () => { st.innerHTML = `<div class="an-live">${STEPS.map((s, j) => `<span class="${j < k ? 'done' : j === k ? 'now' : ''}">${j < k ? ic('check') : j === k ? '<i class="an-spin"></i>' : '<i class="an-o"></i>'}${s}${j === k ? '…' : ''}</span>`).join('')}</div>`; $('.an-mark', el).className = 'an-mark busy'; $('.an-mark', el).innerHTML = '<i class="an-spin"></i>'; scroll(); };
    paint();
    const t = setInterval(() => { k++; if (k < STEPS.length) return paint(); clearInterval(t); renderAI(el, R, i); S.busy = false; $('#anSend').disabled = false; open(i); }, reduceMotion ? 120 : 650);
  }

  /* ---------- مساحة التحليل ---------- */
  function open(i) {
    S.cur = i; S.sort = null; S.q = ''; S.cmpSel = 0; S.tab = 'compare';
    $$('.an-a', thread).forEach(a => a.classList.toggle('is-cur', +a.dataset.i === i));
    canvas();
  }
  const cell = (r, [, k, kind]) => { const v = typeof k === 'function' ? k(r) : r[k];
    switch (kind) { case 'l': return `<td><button type="button" class="an-ent" data-ent="${r.id}">${esc(v)}</button></td>`; case 't': return `<td>${esc(v)}</td>`; case 'sar': return `<td><span class="num">${nf(v)}</span></td>`; case 'pct': return `<td><span class="num">${v}%</span></td>`;
      case 'dp': return `<td><span class="num ${v >= 0 ? 'tx-ok' : 'tx-bad'}">${v >= 0 ? '+' : ''}${v}%</span></td>`; case 'r': return `<td><span class="num">${v} ★</span></td>`; default: return `<td><span class="num">${nf(v)}</span></td>`; } };
  const val = (r, k) => typeof k === 'function' ? k(r) : r[k];
  function rowsOf(R) {
    let L = R.rows.filter(r => !S.q || norm(r.n).includes(norm(S.q)));
    if (S.sort) { const c = R.cols[S.sort.i]; L = [...L].sort((a, b) => { const x = val(a, c[1]), y = val(b, c[1]); return (typeof x === 'string' ? x.localeCompare(y, 'ar') : x - y) * S.sort.d; }); }
    return L;
  }
  function canvas() {
    const C = $('#anCanvas'), R = S.results[S.cur];
    if (!R) { C.innerHTML = `<div class="an-empty"><img src="assets/img/logo-mark.svg" alt=""><h2>اسأل عن بيانات المشروع</h2><p>النتيجة تظهر هنا: ملخص، وجدول أو رسم، ومصادر البيانات، وتحليل مقارن.</p></div>`; return; }
    C.innerHTML = `
      <header class="an-h">
        <div><h1>${esc(R.title)}</h1><div class="an-meta"><span class="an-badge">${ic('layers')}${R.badge}</span>${R.verified ? `<span class="an-ver ok">${ic('check')}موثّقة</span>` : `<span class="an-ver warn">${ic('info')}غير موثّقة</span>`}<span class="muted">${R.rows.length} ${R.rows.length > 10 ? 'صفًا' : 'صفوف'} · «${esc(R.q)}»</span></div></div>
        <button type="button" class="btn btn-sm" data-src aria-expanded="false">${ic('database')}مصادر البيانات</button>
      </header>
      ${R.verified ? '' : `<div class="an-warnbox">${ic('info')}<span>${esc(R.why)}</span><button type="button" class="btn btn-sm" data-verify>${ic('check')}تحقق الآن</button></div>`}
      <div class="an-srcpanel" hidden>
        <ul>${R.src.map(id => { const s = SOURCES.find(x => x.id === id); return `<li>${ic(s.ic)}<span><b>${s.n}</b><small class="mono">${s.t}</small></span><em class="${s.stale ? 'tx-warn' : ''}">${s.fresh}</em></li>`; }).join('')}</ul>
        <details><summary>${ic('file')}الاستعلام المستخدم (SQL)</summary><pre class="an-sql" dir="ltr">${esc(R.sql)}</pre></details>
      </div>
      <section class="an-sec" data-sec="sum">
        <div class="an-sh"><button type="button" class="an-col" aria-expanded="true">${ic('chev-down')}الوصف المختصر</button>
          <label class="an-search">${ic('search')}<span class="sr-only">ابحث في النتائج</span><input type="search" placeholder="ابحث في النتائج" value="${esc(S.q)}"></label>
          <button type="button" class="an-ib" data-dl aria-label="تنزيل CSV" title="تنزيل CSV">${ic('download')}</button>
          <span class="an-seg" role="group" aria-label="طريقة العرض"><button type="button" data-v="table" aria-pressed="${S.view === 'table'}" aria-label="جدول">${ic('sheet')}</button><button type="button" data-v="chart" aria-pressed="${S.view === 'chart'}" aria-label="رسم">${ic('chart')}</button></span></div>
        <div class="an-sb"><p class="an-sum">${esc(R.summary)}</p><div class="an-data"></div></div>
      </section>
      <section class="an-sec" data-sec="ana">
        <div class="an-sh"><span class="an-st">${ic('spark')}التحليل</span></div>
        <div class="an-tabs" role="tablist">${[['sum', 'لخّص', 'file'], ['compare', 'اشرح أهم المقارنات', 'spark'], ['top', 'صِف الأفضل', 'star']].map(([k, n, i]) => `<button type="button" role="tab" data-tab="${k}" aria-selected="${S.tab === k}">${ic(i)}${n}</button>`).join('')}</div>
        <div class="an-tb"></div>
      </section>
      <div class="an-acts"><button type="button" class="btn btn-sm" data-act="report">${ic('file')}حفظ كتقرير</button><button type="button" class="btn btn-sm" data-act="share">${ic('share')}مشاركة النتيجة</button><button type="button" class="btn btn-sm" data-act="follow">${ic('chat')}سؤال متابعة</button></div>`;
    data(); tab();
    $('.an-search input', C).addEventListener('input', e => { S.q = e.target.value; data(); });
  }
  function data() {
    const R = S.results[S.cur], L = rowsOf(R), box = $('#anCanvas .an-data');
    if (S.view === 'table') {
      box.innerHTML = `<div class="an-tw"><table class="table an-table"><thead><tr>${R.cols.map((c, i) => `<th><button type="button" data-sort="${i}">${c[0]}${S.sort && S.sort.i === i ? (S.sort.d > 0 ? ' ▲' : ' ▼') : ''}</button></th>`).join('')}</tr></thead><tbody>${L.map(r => `<tr data-row="${r.id}">${R.cols.map(c => cell(r, c)).join('')}</tr>`).join('') || `<tr><td colspan="${R.cols.length}" class="muted">لا نتائج مطابقة.</td></tr>`}</tbody></table></div>`;
    } else {
      const k = R.key, mx = Math.max(...L.map(r => Math.abs(val(r, k))), 1), lab = R.cols.find(c => c[1] === k);
      box.innerHTML = `<p class="an-cl">${lab ? lab[0] : ''}</p><ul class="an-hb">${[...L].sort((a, b) => val(b, k) - val(a, k)).map(r => `<li data-row="${r.id}"${tipA(r.n, nf(val(r, k)))}><span>${esc(r.n)}</span><span class="t"><i style="width:${(Math.abs(val(r, k)) / mx * 100).toFixed(1)}%"></i></span><b class="num">${lab && lab[2] === 'sar' ? sar(val(r, k)) : nf(val(r, k))}${lab && (lab[2] === 'pct' || lab[2] === 'dp') ? '%' : ''}</b></li>`).join('')}</ul>`;
    }
  }
  const tipA = (a, b) => ` data-tip="${esc(a)}|${esc(b)}"`;
  function tab() {
    const R = S.results[S.cur], B = $('#anCanvas .an-tb');
    $$('#anCanvas [data-tab]').forEach(b => b.setAttribute('aria-selected', String(b.dataset.tab === S.tab)));
    if (S.tab === 'sum') {
      const k = R.key, L = top(R.rows, x => val(x, k), R.rows.length), lab = (R.cols.find(c => c[1] === k) || [''])[0];
      B.innerHTML = `<ul class="an-pts"><li><b>الأعلى:</b> «${esc(L[0].n)}» في ${lab}.</li><li><b>الأدنى:</b> «${esc(L[L.length - 1].n)}».</li><li><b>التركّز:</b> أعلى 3 يمثلون ${Math.round(L.slice(0, 3).reduce((a, x) => a + Math.abs(val(x, k)), 0) / Math.max(1, L.reduce((a, x) => a + Math.abs(val(x, k)), 0)) * 100)}% من الإجمالي.</li><li><b>التوصية:</b> ${R.key === 'actual' ? 'راجع خطة المناديب دون 80% من المستهدف قبل نهاية الربع.' : R.key === 'growth' ? 'زد مخزون الأجهزة الأعلى نموًا في مستودع الرياض قبل موسم العودة.' : 'ركّز المتابعة على البنود في الثلث الأعلى، وابحث أسباب تراجع الثلث الأدنى.'}</li></ul>`;
      return;
    }
    if (S.tab === 'top') {
      B.innerHTML = `<div class="an-tops">${R.cmp.map((x, i) => `<article style="--c:${COL[i]}"><span class="rk">${i + 1}</span><b>${esc(x.n)}</b><p>${describe(R, x)}</p></article>`).join('')}</div>`;
      return;
    }
    const sel = R.cmp[S.cmpSel];
    B.innerHTML = `<h3 class="an-cmph">مقارنة أفضل ${R.cmp.length} في «${esc(R.title)}»:</h3>
      <div class="an-cmp">
        <ul class="an-items">${R.cmp.map((x, i) => `<li${i === S.cmpSel ? ' class="is-on"' : ''}><button type="button" data-pick="${i}"><span class="ch">${ic('chev-left')}</span><span><b>${esc(x.n)}</b><small>${describe(R, x)}</small></span></button></li>`).join('')}</ul>
        <div class="an-chart">
          <div class="an-ch"><span>قارن</span><span class="an-seg sm" role="group" aria-label="اختر البند">${R.cmp.map((x, i) => `<button type="button" data-pick="${i}" aria-pressed="${i === S.cmpSel}">${esc(x.n.split(' ').slice(0, 2).join(' '))}</button>`).join('')}</span></div>
          <div class="an-leg">${R.metrics.map((m, i) => `<span><i style="background:${COL[i]}"></i>${m[0]}</span>`).join('')}</div>
          <div class="an-pills" role="img" aria-label="قيم ${esc(sel.n)}">${R.metrics.map((m, i) => { const v = clamp(m[1](sel)); return `<div class="an-pill"${tipA(m[0], v + ' من 100')}><span class="trk"><i style="height:${v}%;background:${COL[i]}"></i><b style="bottom:calc(${v}% - 11px)"></b></span><em class="num">${v}</em></div>`; }).join('')}</div>
          <div class="an-mini">${R.cmp.map((x, i) => `<button type="button" data-pick="${i}" class="${i === S.cmpSel ? 'is-on' : ''}"><i style="background:${COL[i]}"></i>${esc(x.n)}</button>`).join('')}</div>
        </div>
      </div>`;
  }
  function describe(R, x) {
    if (R.key === 'actual') return `حقق ${Math.round(x.actual / x.target * 100)}% من مستهدفه بـ${x.deals} صفقة، ونسبة فوز ${x.win}%.`;
    if (x.cat) return `${x.cat === 'أجهزة' ? 'جهاز' : 'إكسسوار'} بإيراد ${sar(x.rev)} ر.س، هامش ${x.margin}% ونمو ${x.growth >= 0 ? '+' : ''}${x.growth}% عن العام الماضي.`;
    if (x.customers) return `إيراد ${sar(x.rev)} ر.س من ${x.customers} عميلًا، والنمو ${x.growth >= 0 ? '+' : ''}${x.growth}%.`;
    return `إيراد ${sar(x.rev)} ر.س من ${x.orders} طلبًا، وآخر طلب قبل ${x.last} يومًا${x.late ? `، ولديه ${x.late} طلبات متأخرة` : ''}.`;
  }

  /* ---------- الأحداث ---------- */
  root.addEventListener('click', e => {
    const t = e.target;
    const o = t.closest('[data-open]'); if (o) return open(+o.dataset.open);
    const en = t.closest('.an-ent'); if (en) { const a = en.closest('.an-a'); if (a && +a.dataset.i !== S.cur) open(+a.dataset.i); S.view = 'table'; data(); const tr = $(`#anCanvas [data-row="${en.dataset.ent}"]`); if (tr) { tr.scrollIntoView({ block: 'center', behavior: reduceMotion ? 'auto' : 'smooth' }); tr.classList.remove('flash'); void tr.offsetWidth; tr.classList.add('flash'); } return; }
    const v = t.closest('[data-v]'); if (v) { S.view = v.dataset.v; $$('#anCanvas [data-v]').forEach(b => b.setAttribute('aria-pressed', String(b === v))); return data(); }
    const so = t.closest('[data-sort]'); if (so) { const i = +so.dataset.sort; S.sort = S.sort && S.sort.i === i ? { i, d: -S.sort.d } : { i, d: -1 }; return data(); }
    const tb = t.closest('[data-tab]'); if (tb) { S.tab = tb.dataset.tab; return tab(); }
    const pk = t.closest('[data-pick]'); if (pk) { S.cmpSel = +pk.dataset.pick; return tab(); }
    if (t.closest('.an-col')) { const sec = t.closest('.an-sec'), b = $('.an-col', sec), x = b.getAttribute('aria-expanded') === 'true'; b.setAttribute('aria-expanded', String(!x)); $('.an-sb', sec).hidden = x; return; }
    if (t.closest('[data-src]')) { const p = $('#anCanvas .an-srcpanel'), b = t.closest('[data-src]'); p.hidden = !p.hidden; b.setAttribute('aria-expanded', String(!p.hidden)); return; }
    if (t.closest('[data-verify]')) { const R = S.results[S.cur], b = t.closest('[data-verify]'); b.disabled = true; b.innerHTML = '<i class="an-spin"></i>جارٍ المزامنة والتحقق…';
      setTimeout(() => { R.verified = true; const el = $(`.an-a[data-i="${S.cur}"]`, thread); if (el) renderAI(el, R, S.cur); canvas(); toast('تمت مزامنة CRM ومطابقة الأرقام مع إجماليات المصدر. الإجابة موثّقة الآن.'); }, reduceMotion ? 100 : 1600); return; }
    if (t.closest('[data-dl]')) return download();
    const ac = t.closest('[data-act]'); if (ac) { const m = { report: 'حُفظت النتيجة كتقرير في «الملفات».', share: 'نُسخ رابط النتيجة. يراها من لديه صلاحية على المشروع فقط.' }[ac.dataset.act]; if (m) toast(m); else { $('#anIn').focus(); $('#anIn').value = 'وماذا عن '; } return; }
    const sg = t.closest('[data-sug]'); if (sg) return run(sg.dataset.sug);
    const sc = t.closest('[data-srcchip]'); if (sc) { const s = SOURCES.find(x => x.id === sc.dataset.srcchip); s.on = !s.on; return chips(); }
    if (t.closest('#anAttach')) { const p = $('#anPick'); p.hidden = !p.hidden; $('#anAttach').setAttribute('aria-expanded', String(!p.hidden)); return; }
    const ps = t.closest('[data-pick-src]'); if (ps) { const s = SOURCES.find(x => x.id === ps.dataset.pickSrc); s.on = !s.on; chips(); return; }
    if (!t.closest('#anPick')) $('#anPick').hidden = true;
  });
  function download() {
    const R = S.results[S.cur], L = rowsOf(R), csv = '﻿' + [R.cols.map(c => c[0]).join(','), ...L.map(r => R.cols.map(c => `"${String(val(r, c[1])).replace(/"/g, '""')}"`).join(','))].join('\n');
    const a = document.createElement('a'); a.href = URL.createObjectURL(new Blob([csv], { type: 'text/csv;charset=utf-8' })); a.download = R.title + '.csv'; a.click(); setTimeout(() => URL.revokeObjectURL(a.href), 1000); toast('نُزّل الملف بصيغة CSV.');
  }
  const toast = msg => { let t = document.querySelector('.toast'); if (!t) { t = document.createElement('div'); t.className = 'toast'; t.setAttribute('role', 'status'); document.body.append(t); } t.innerHTML = `${ic('check')}<span>${esc(msg)}</span>`; requestAnimationFrame(() => t.classList.add('is-on')); clearTimeout(t._h); t._h = setTimeout(() => t.classList.remove('is-on'), 2800); };

  /* ---------- الإدخال والمصادر المرفقة ---------- */
  const SUG = ['قارن أداء المناديب في الربع الثالث', 'ما المبيعات حسب المنطقة؟', 'من أكبر العملاء ولديهم طلبات متأخرة؟', 'ما المنتجات المتاحة من الأجهزة والإكسسوارات؟'];
  $('#anSug').innerHTML = SUG.slice(0, 2).map(s => `<button type="button" class="an-sg" data-sug="${esc(s)}">${esc(s)}</button>`).join('') + `<button type="button" class="an-sg more" data-more>+${SUG.length - 2}</button>`;
  $('#anSug').addEventListener('click', e => { if (e.target.closest('[data-more]')) { $('#anSug').innerHTML = SUG.map(s => `<button type="button" class="an-sg" data-sug="${esc(s)}">${esc(s)}</button>`).join(''); } });
  function chips() {
    $('#anChips').innerHTML = SOURCES.filter(s => s.on).map(s => `<button type="button" class="an-chip" data-srcchip="${s.id}" aria-label="إزالة ${s.n}">${ic(s.ic)}${s.n}<span>${s.stale ? '· قديم' : ''}</span>${ic('x')}</button>`).join('') || '<span class="muted">لا مصادر مرفقة · سيستخدم جيم كل مصادر المشروع</span>';
    $('#anPick').innerHTML = `<p>المصادر المستخدمة في الإجابة</p>${SOURCES.map(s => `<button type="button" data-pick-src="${s.id}" role="menuitemcheckbox" aria-checked="${s.on}">${ic(s.ic)}<span><b>${s.n}</b><small>${s.fresh}</small></span>${s.on ? ic('check') : ''}</button>`).join('')}`;
  }
  $('#anForm').addEventListener('submit', e => { e.preventDefault(); const q = $('#anIn').value.trim(); if (!q || S.busy) return; $('#anIn').value = ''; run(q); });
  $('#anIn').addEventListener('keydown', e => { if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); $('#anForm').requestSubmit(); } });
  $('#anNew').addEventListener('click', () => { thread.innerHTML = ''; S.results = []; S.cur = -1; canvas(); toast('بدأت جلسة تحليل جديدة.'); });

  /* ---------- البدء: جلسة سابقة ثم سؤال قيد التحليل ---------- */
  chips();
  run('ما المنتجات المتاحة من فئتي الأجهزة والإكسسوارات؟', 'products', true);
  run('اعرض الأجهزة الأعلى نموًا في المنطقة الوسطى', 'growth', true);
  open(1);
  setTimeout(() => run('قارن أداء المناديب في الربع الثالث', 'reps'), 900);
})();
