/* ==========================================================
   جيم — معرض نماذج اللوحات الكاملة («لوحة جديدة»)
   زي شاشة البداية في PowerPoint: تختار نموذج لوحة كامل (10 عناصر)
   أو تبدأ بلوحة فارغة أو تطلب من جيم يبنيها.
   في التطبيق الفعلي:
     GET  /dashboard-templates?projectId=..   ← القائمة (مع توافق المصادر)
     POST /projects/{id}/dashboards { templateId } ← ينشئ لوحة جديدة ببيانات حقيقية
   البيانات هنا تجريبية للعرض فقط.
   ========================================================== */
(function () {
  const grid = document.querySelector('.canvas .grid');
  if (!grid) return;
  const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
  const C = ['var(--chart-1)', 'var(--chart-2)', 'var(--chart-3)', 'var(--chart-4)'];
  const fmt = n => typeof n === 'number' ? n.toLocaleString('en-US') : n;

  /* ---------- رسم العناصر ---------- */
  const RENDER = {
    kpi: w => `<div class="value"><span class="num">${w.value}</span><small>${w.unit || ''}</small></div>
      <div class="cmp"><span class="delta ${w.up ? 'up' : 'down'}">${ic(w.up ? 'up' : 'down')}<span class="num">${w.delta}</span></span> ${w.cmp || 'مقارنة بالفترة السابقة'}</div>
      <div class="tpl-spark"><svg viewBox="0 0 120 30" preserveAspectRatio="none"><polyline points="${(w.trend || [20, 18, 22, 15, 16, 10, 6]).map((v, i, a) => `${i * 120 / (a.length - 1)},${v}`).join(' ')}" fill="none" stroke="var(--chart-1)" stroke-width="2" vector-effect="non-scaling-stroke"/></svg></div>`,
    line: w => {
      const max = Math.max(...w.values), min = Math.min(...w.values) * .9, n = w.values.length;
      const pts = w.values.map((v, i) => `${(i * 600 / (n - 1)).toFixed(1)},${(150 - (v - min) / (max - min) * 130).toFixed(1)}`);
      return `<div class="tpl-line"><svg viewBox="0 0 600 160" preserveAspectRatio="none"><path d="M${pts.join(' L')} L600 160 L0 160Z" fill="var(--chart-1)" opacity=".10"/><polyline points="${pts.join(' ')}" fill="none" stroke="var(--chart-1)" stroke-width="2.5" vector-effect="non-scaling-stroke" stroke-linejoin="round"/></svg></div>
        <div class="cmp" style="display:flex;justify-content:space-between"><span>${w.labels[0]}</span><span>${w.note || ''}</span><span>${w.labels[1]}</span></div>`;
    },
    donut: w => {
      let acc = 0; const stops = w.items.map(([, p], i) => { const s = `${C[i]} ${acc}% ${acc + p - .6}%, #fff 0 ${acc + p}%`; acc += p; return s; }).join(', ');
      return `<div style="display:flex;align-items:center;gap:20px;flex-wrap:wrap"><div class="cdonut" style="background:conic-gradient(${stops})"><span><b class="num">${w.total}</b>${w.totalLabel || 'الإجمالي'}</span></div>
        <ul class="donut-list" style="flex:1;min-width:150px">${w.items.map(([n, p], i) => `<li><i style="background:${C[i]}"></i><span>${n}</span><b></b><span class="num">${p}%</span></li>`).join('')}</ul></div>`;
    },
    hbars: w => { const max = Math.max(...w.rows.map(r => r[1])); return `<ul class="hbars">${w.rows.map(([n, v], i) => `<li><span class="lbl">${n}</span><span class="trk"><span class="fill" style="width:${(v / max * 100).toFixed(1)}%;animation-delay:${i * .05}s"></span></span><span class="val num">${fmt(v)}${w.unit || ''}</span></li>`).join('')}</ul>`; },
    cbars: w => { const max = Math.max(...w.groups.flatMap(g => [g[1], g[2]])); return `<div class="legend"><span><i style="background:#BFD9EC"></i>${w.legend[0]}</span><span><i style="background:var(--chart-1)"></i>${w.legend[1]}</span></div>
      <div class="cbars">${w.groups.map(([q, a, b]) => `<div class="cb"><div class="pair"><i style="height:${a / max * 100}%;background:#BFD9EC" title="${w.legend[0]}: ${a}"></i><i style="height:${b / max * 100}%;background:var(--chart-1)" title="${w.legend[1]}: ${b}"></i></div><span>${q}</span></div>`).join('')}</div>`; },
    table: w => `<table class="table"><thead><tr>${w.cols.map(c => `<th>${c}</th>`).join('')}</tr></thead><tbody>${w.rows.map(r => `<tr>${r.map((c, i) => `<td>${i ? `<span class="num">${fmt(c)}</span>` : c}</td>`).join('')}</tr>`).join('')}</tbody></table>`,
    funnel: w => { const max = w.rows[0][1]; return `<div class="funnel">${w.rows.map(([n, v], i) => `<div class="fr"><span class="fb" style="width:${Math.max(22, v / max * 100)}%;${i === w.rows.length - 1 ? 'background:var(--chart-4)' : ''}"><b class="num">${fmt(v)}</b></span><span class="fl">${n}</span></div>`).join('')}</div>`; },
    heat: w => `<div class="heat" style="grid-template-columns:64px repeat(${w.cols.length},1fr)">${w.rows.map((d, r) => `<span class="d">${d}</span>${w.cols.map((_, c) => `<i style="opacity:${[.2, .35, .55, .8, 1, .6, .4, .7][(c * 3 + r * 5) % 8]}"></i>`).join('')}`).join('')}<span></span>${w.cols.map(h => `<span class="h">${h}</span>`).join('')}</div>`
  };
  /* مساحة كل نوع في الشبكة (من 12) */
  const SPAN = { kpi: 3, line: 8, donut: 4, hbars: 6, cbars: 6, table: 7, funnel: 5, heat: 5 };

  /* ---------- النماذج (كل نموذج = 4 مؤشرات + 6 رسوم = 10 عناصر) ---------- */
  const CAT = window.JEEM_CATALOG;
  const DASHBOARDS = CAT.dashboards(), CATS = CAT.DASH_CATS, GLYPH = CAT.GLYPH, arrange = CAT.arrange, thumb = CAT.thumb;
  /* بيانات تجريبية لأي عنصر جديد أضافه مسؤول المنصّة بلا بيانات عرض */
  const DEMO = {
    kpi: { value: '—', unit: '', delta: '0%', up: true },
    line: { labels: ['يناير', 'سبتمبر'], values: [42, 48, 45, 53, 58, 55, 63, 67, 72] },
    donut: { total: '100%', items: [['الفئة أ', 45], ['الفئة ب', 30], ['الفئة ج', 15], ['أخرى', 10]] },
    hbars: { unit: '', rows: [['البند الأول', 84], ['البند الثاني', 66], ['البند الثالث', 52], ['البند الرابع', 31]] },
    cbars: { legend: ['السابق', 'الحالي'], groups: [['الربع الأول', 40, 46], ['الربع الثاني', 44, 52], ['الربع الثالث', 47, 58]] },
    table: { cols: ['البند', 'القيمة', 'التغيّر'], rows: [['البند الأول', 1240, '8%'], ['البند الثاني', 980, '5%'], ['البند الثالث', 760, '-2%']] },
    funnel: { rows: [['المرحلة الأولى', 1000], ['المرحلة الثانية', 640], ['المرحلة الثالثة', 380], ['المرحلة الأخيرة', 190]] },
    heat: { rows: ['الأحد', 'الإثنين', 'الثلاثاء', 'الأربعاء', 'الخميس'], cols: ['9ص', '12', '3م', '6', '9م'] }
  };
  const draw = w => RENDER[w.type](Object.assign({}, DEMO[w.type], w));

  /* ---------- النافذة ---------- */
  const dlg = document.createElement('dialog');
  dlg.className = 'modal gallery'; dlg.id = 'dashboard-gallery'; dlg.setAttribute('aria-labelledby', 'gal-title');
  dlg.innerHTML = `
    <div class="gal-head">
      <div><h2 id="gal-title">لوحة جديدة</h2><p>اختر لوحة جاهزة من ${DASHBOARDS.length} نموذجًا، أو ابدأ من الصفر، أو اطلب من جيم.</p></div>
      <label class="tpl-search gal-search">${ic('search')}<span class="sr-only">بحث في نماذج اللوحات</span><input type="search" placeholder="ابحث في النماذج… مثال: ميزانية"></label>
      <button class="btn btn-ghost btn-icon" type="button" data-gal-close aria-label="إغلاق">${ic('x')}</button>
    </div>
    <div class="gal-body">
      <nav class="gal-cats" aria-label="تصنيفات النماذج">${CATS.filter(([k]) => k === 'all' || DASHBOARDS.some(d => d.cat === k)).map(([k, n, i], x) => `<button type="button" data-cat="${k}" aria-pressed="${x ? 'false' : 'true'}">${ic(i)}<span>${n}</span><em class="num">${k === 'all' ? DASHBOARDS.length : DASHBOARDS.filter(d => d.cat === k).length}</em></button>`).join('')}</nav>
      <div class="gal-main">
        <section class="gal-view" data-view="list">
          <div class="gal-quick">
            <button type="button" class="quick-card" data-start="blank"><span class="qi">${ic('plus', 'icon icon-lg')}</span><span><b>لوحة فارغة</b><small>ابدأ من الصفر وأضف العناصر بنفسك أو من «النماذج»</small></span></button>
            <button type="button" class="quick-card ai" data-start="ai"><span class="qi"><img src="assets/img/logo-mark.svg" alt=""></span><span><b>اطلب من جيم</b><small>اكتب وصف اللوحة اللي محتاجها وجيم يبنيها من مصادرك</small></span></button>
            <div class="quick-card net"><span class="qi">${ic('share', 'icon icon-lg')}</span><span><b>لوحات تفاعلية <em class="badge badge-blue">جديد</em></b><small>شبكات حية تضغط عليها وتستكشفها</small><span class="qn-links"><a href="policy-network.html">الأنظمة واللوائح</a><a href="procurement-network.html">العملاء وطلبات الشراء</a><a href="procurement-workflow.html">سير العمل ومدة الخدمة</a><a href="procurement-marketplace.html">العملاء والموردون</a><a href="client-supplier-network.html">شبكة العملاء والموردين</a><a href="venue-ops.html">مراقبة المنشأة (داكن)</a><a href="venue-ops-light.html">مراقبة المنشأة (فاتح)</a><a href="ecommerce-pulse.html">نبض المتجر الإلكتروني</a><a href="fleet-ops.html">الأسطول والتوصيل</a><a href="real-estate.html">إدارة العقارات</a><a href="nexus-marketing.html">لوحة التسويق «نكسس»</a><a href="sales-overview.html">المبيعات والتسويق</a><a href="supplier-scorecard.html">تقييم أداء الموردين</a><a href="sales-team.html">أداء فريق المبيعات</a><a href="warehouse-team.html">فريق المستودع</a><a href="liquidity-pools.html">مجمّعات السيولة</a><a href="token-treasury.html">خزينة الأصول الرقمية</a><a href="crm-pipeline.html">إدارة العملاء</a><a href="team-tasks.html">مهام الفريق</a><a href="time-tracking.html">تتبع الوقت</a><a href="crm-analysis.html">تحليل مسار المبيعات</a><a href="finance-ops.html">الإدارة المالية</a><a href="crm-leads.html">العملاء المحتملون</a><a href="campaign-project.html">تفاصيل مشروع حملة</a><a href="task-board.html">لوحة مهام المشاريع</a><a href="construction-crm.html">إدارة المقاولات</a><a href="project-studio.html">استوديو المشاريع</a><a href="treasury-funding.html">التمويل والسيولة</a><a href="saas-metrics.html">إيرادات الاشتراكات</a><a href="supply-command.html">قيادة سلسلة الإمداد</a><a href="supplier-risk.html">مخاطر الموردين</a></span></span></div>
          </div>
          <h3 class="gal-sub">النماذج <span class="muted" style="font-weight:300">· لوحات كاملة متعددة الصفحات بالمؤشرات والرسوم والفلاتر</span></h3>
          <div class="gal-grid">${DASHBOARDS.map(d => `
            <button type="button" class="dash-card" data-dash="${d.id}" data-cat="${d.cat}">
              ${thumb(d)}
              <span class="dc-body"><b>${d.name}${d.popular ? '<span class="badge badge-blue">الأكثر استخدامًا</span>' : ''}</b><small>${d.desc}</small>
              <span class="dc-meta"><span>${ic('dashboard')}${d.p ? `${d.p.length > 1 ? d.p.length + ' صفحات · ' : ''}${CAT.countWidgets(d)} عنصرًا` : '10 عناصر'}</span><span>${ic('database')}${d.sources.join('، ')}</span></span></span>
            </button>`).join('')}</div>
          <p class="gal-empty" hidden>لا توجد نماذج مطابقة. جرّب «اطلب من جيم».</p>
        </section>
        <section class="gal-view gal-preview" data-view="preview" hidden></section>
      </div>
    </div>`;
  document.body.appendChild(dlg);

  const $ = (s, r = dlg) => r.querySelector(s), $$ = (s, r = dlg) => [...r.querySelectorAll(s)];
  const show = v => $$('.gal-view').forEach(x => { x.hidden = x.dataset.view !== v; });
  let cat = 'all', q = '';
  const filter = () => {
    let n = 0;
    $$('.dash-card').forEach(c => { const d = DASHBOARDS.find(x => x.id === c.dataset.dash); const ok = (cat === 'all' || d.cat === cat) && (d.name + d.desc + d.sources.join()).includes(q); c.hidden = !ok; n += ok; });
    $('.gal-empty').hidden = n > 0;
  };
  $$('.gal-cats button').forEach(b => b.addEventListener('click', () => { cat = b.dataset.cat; $$('.gal-cats button').forEach(x => x.setAttribute('aria-pressed', String(x === b))); show('list'); filter(); }));
  $('.gal-search input').addEventListener('input', e => { q = e.target.value.trim(); show('list'); filter(); });
  $('[data-gal-close]').addEventListener('click', () => dlg.close());
  dlg.addEventListener('click', e => { if (e.target === dlg) dlg.close(); });

  /* معاينة النموذج قبل الاستخدام */
  const preview = d => {
    const p = $('.gal-preview');
    p.innerHTML = `
      <button type="button" class="btn btn-ghost btn-sm gal-back">${ic('chev-left', 'icon icon-sm')}<span>كل النماذج</span></button>
      <div class="pv-grid">
        <div class="pv-thumb">${thumb(d)}</div>
        <div class="pv-info">
          <h3>${d.name}</h3><p>${d.desc}</p>
          <div class="pv-src">${d.sources.map(s => `<span class="src-tag">${ic('database')}${s}</span>`).join('')}</div>
          ${d.p ? `<h4>${d.p.length > 1 ? `${d.p.length} صفحات · ` : ''}${CAT.countWidgets(d)} عنصرًا</h4>
          <div class="pv-pages">${d.p.map((pg, i) => `<details${i ? '' : ' open'}><summary><b>${pg.n}</b><span class="num">${pg.w.length} عناصر</span></summary><ol class="pv-list">${pg.w.map(w => `<li><span class="pv-ic">${GLYPH[window.JR ? window.JR.glyphOf(w.t) : 'table']}</span>${w.ti}</li>`).join('')}</ol></details>`).join('')}</div>
          ${d.f && d.f.length ? `<p class="pv-filters">${ic('settings')}الفلاتر: ${d.f.map(f => f[0]).join('، ')}</p>` : ''}
          <a class="btn btn-ghost btn-sm" href="dashboard-gallery.html#${d.id}" target="_blank" rel="noopener">${ic('expand')}معاينة كاملة بكل الصفحات</a>`
          : `<h4>العناصر العشرة</h4>
          <ol class="pv-list">${arrange(d).map(w => `<li><span class="pv-ic">${GLYPH[w.type]}</span>${w.title}</li>`).join('')}</ol>`}
          <div class="pv-actions"><button type="button" class="btn btn-primary" data-use="${d.id}">${ic('check', 'icon icon-sm')}استخدم هذا النموذج</button><span class="muted" style="font-size:var(--fs-xs)">تُنشأ لوحة جديدة ببيانات مشروعك، وتقدر تعدّلها بعدها من المحادثة.</span></div>
        </div>
      </div>`;
    $('.gal-back', p).addEventListener('click', () => show('list'));
    $('[data-use]', p).addEventListener('click', () => apply(d));
    show('preview'); $('[data-use]', p).focus();
  };
  $$('.dash-card').forEach(c => c.addEventListener('click', () => preview(DASHBOARDS.find(d => d.id === c.dataset.dash))));

  /* ---------- تطبيق النموذج على مساحة العمل ---------- */
  const toast = msg => {
    let t = document.querySelector('.toast');
    if (!t) { t = document.createElement('div'); t.className = 'toast'; t.setAttribute('role', 'status'); document.body.appendChild(t); }
    t.innerHTML = `${ic('check')}${msg}`; t.classList.remove('is-on'); void t.offsetWidth; t.classList.add('is-on');
    clearTimeout(t._h); t._h = setTimeout(() => t.classList.remove('is-on'), 2800);
  };
  const setHeader = (title, meta, insight) => {
    const h = document.querySelector('.dash-title h1'); if (h) h.textContent = title;
    const m = document.querySelector('.dash-meta'); if (m) m.innerHTML = meta;
    const ins = document.querySelector('.insight p'); if (ins) ins.innerHTML = insight;
  };
  const clearBar = () => grid.parentElement.querySelector(':scope > .jr-bar')?.remove();
  const apply = d => {
    if (d.p && window.JR) {
      window.JR.mount(d, grid);
      const n = CAT.countWidgets(d);
      setHeader(d.name, `<span class="live">بيانات حية من ${d.sources[0]}</span><span class="dot-sep"></span><span>أُنشئت من نموذج «${d.name}» · الآن</span>`,
        `هذه لوحة جديدة من نموذج «${d.name}»: ${d.p.length > 1 ? d.p.length + ' صفحات و' : ''}${n} عنصرًا. بدّل الصفحات والفلاتر من أعلى اللوحة، واطلب من جيم أي تعديل.`);
      dlg.close(); window.scrollTo({ top: 0, behavior: 'smooth' });
      toast(`تم إنشاء لوحة «${d.name}» · ${n} عنصرًا`);
      return;
    }
    clearBar();
    grid.innerHTML = arrange(d).map((w, i) => `
      <article class="widget span-${w.span}${w.type === 'kpi' ? ' kpi' : ''}" style="animation-delay:${i * .05}s">
        <div class="w-head">${w.type === 'kpi' ? `<span class="label">${w.title}</span>` : `<div><h3>${w.title}</h3><p>${w.sub || ''}</p></div>`}</div>
        ${draw(w)}
      </article>`).join('');
    setHeader(d.name, `<span class="live">بيانات حية من ${d.sources[0]}</span><span class="dot-sep"></span><span>أُنشئت من نموذج «${d.name}» · الآن</span>`,
      `هذه لوحة جديدة من نموذج «${d.name}» بعناصرها العشرة. اطلب من جيم أي تعديل، أو أضف عناصر من تبويب «النماذج».`);
    dlg.close(); window.scrollTo({ top: 0, behavior: 'smooth' });
    toast(`تم إنشاء لوحة «${d.name}» · 10 عناصر`);
  };

  /* البدايات السريعة */
  $('[data-start="blank"]').addEventListener('click', () => {
    clearBar();
    grid.innerHTML = `<div class="span-12 empty-dash">${ic('dashboard', 'icon icon-lg')}<b>لوحة فارغة</b><span>اكتب طلبك في المحادثة، أو افتح تبويب «النماذج» وأضف العناصر بضغطة.</span></div>`;
    setHeader('لوحة جديدة', '<span>لوحة فارغة · الآن</span>', 'ابدأ بسؤال في المحادثة، وجيم هيبني العناصر من مصادر مشروعك.');
    dlg.close(); document.querySelector('#tab-templates')?.click();
  });
  $('[data-start="ai"]').addEventListener('click', () => {
    dlg.close(); document.querySelector('#tab-build')?.click();
    const ta = document.querySelector('#q'); if (ta) { ta.value = 'اعمل لي لوحة جديدة توضح '; ta.focus(); ta.setSelectionRange(ta.value.length, ta.value.length); }
  });

  /* فتح المعرض من أي زر عليه data-open-gallery */
  document.querySelectorAll('[data-open-gallery]').forEach(b => b.addEventListener('click', () => {
    q = ''; cat = 'all'; $('.gal-search input').value = '';
    $$('.gal-cats button').forEach((x, i) => x.setAttribute('aria-pressed', String(i === 0)));
    filter(); show('list'); dlg.showModal();
  }));
})();
