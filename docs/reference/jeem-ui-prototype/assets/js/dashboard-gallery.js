/* ==========================================================
   جيم — مكتبة لوحات المتابعة: معاينة كاملة لكل لوحة بصفحاتها وفلاترها
   الرابط: dashboard-gallery.html#<id> يفتح لوحة محددة.
   ========================================================== */
(function () {
  const CAT = window.JEEM_CATALOG, JR = window.JR;
  const nav = document.getElementById('dgNav'), grid = document.getElementById('dgGrid'), head = document.getElementById('dgHead');
  if (!CAT || !JR || !nav) return;
  const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
  const LIB = CAT.dashboards().filter(d => d.p);
  const CATS = CAT.DASH_CATS.filter(([k]) => LIB.some(d => d.cat === k));
  let q = '';
  const qi = document.querySelector('.dg-search input'); if (qi) qi.placeholder = `ابحث في ${LIB.length} لوحة…`;

  const drawNav = () => {
    const cur = location.hash.slice(1);
    nav.innerHTML = (() => { const NETS = [['policy-network.html', 'شبكة الأنظمة واللوائح'], ['procurement-network.html', 'شبكة العملاء وطلبات الشراء'], ['procurement-workflow.html', 'سير عمل المشتريات ومدة الخدمة'], ['procurement-marketplace.html', 'منصة المشتريات: العملاء والموردون'], ['client-supplier-network.html', 'شبكة العملاء والموردين'], ['venue-ops.html', 'مراقبة شبكة المنشأة (داكن)'], ['venue-ops-light.html', 'مراقبة شبكة المنشأة (فاتح)'], ['ecommerce-pulse.html', 'نبض المتجر الإلكتروني'], ['fleet-ops.html', 'إدارة الأسطول والتوصيل'], ['real-estate.html', 'إدارة العقارات'], ['nexus-marketing.html', 'لوحة التسويق «نكسس»'], ['sales-overview.html', 'المبيعات والتسويق'], ['supplier-scorecard.html', 'تقييم أداء الموردين'], ['sales-team.html', 'أداء فريق المبيعات'], ['warehouse-team.html', 'نظرة على فريق المستودع'], ['liquidity-pools.html', 'مجمّعات السيولة المركّزة'], ['token-treasury.html', 'خزينة الأصول الرقمية'], ['crm-pipeline.html', 'إدارة العملاء — مراحل البيع'], ['team-tasks.html', 'إدارة مهام الفريق'], ['time-tracking.html', 'تتبع الوقت للفرق'], ['crm-analysis.html', 'تحليل مسار المبيعات'], ['finance-ops.html', 'الإدارة المالية'], ['crm-leads.html', 'العملاء المحتملون'], ['campaign-project.html', 'تفاصيل مشروع حملة'], ['task-board.html', 'لوحة مهام المشاريع'], ['construction-crm.html', 'إدارة المقاولات'], ['project-studio.html', 'استوديو المشاريع'], ['treasury-funding.html', 'التمويل والسيولة'], ['saas-metrics.html', 'إيرادات الاشتراكات'], ['supply-command.html', 'قيادة سلسلة الإمداد'], ['supplier-risk.html', 'مخاطر الموردين']].filter(([, t]) => !q || (t + ' تفاعلي').includes(q)); return NETS.length ? `<div class="sb-label"><span>${ic('share')}لوحات تفاعلية</span><em class="num">${NETS.length}</em></div><ul class="nav">${NETS.map(([f, t]) => `<li><a class="nav-link" href="${f}"><span class="nav-text">${t}</span><span class="badge badge-blue">جديد</span></a></li>`).join('')}</ul>` : ''; })() + CATS.map(([k, n, i]) => {
      const items = LIB.filter(d => d.cat === k && (!q || (d.name + d.en + d.desc).toLowerCase().includes(q)));
      if (!items.length) return '';
      return `<div class="sb-label"><span>${ic(i)}${n}</span><em class="num">${items.length}</em></div><ul class="nav">${items.map(d => `<li><a class="nav-link${d.id === cur ? ' is-active' : ''}" href="#${d.id}"${d.id === cur ? ' aria-current="page"' : ''}><span class="nav-text">${d.name}</span>${d.p.length > 1 ? `<span class="badge num">${d.p.length}</span>` : ''}</a></li>`).join('')}</ul>`;
    }).join('') || '<p class="dg-empty">لا توجد لوحات مطابقة.</p>';
  };

  const open = () => {
    const id = location.hash.slice(1) || LIB[0].id, d = LIB.find(x => x.id === id) || LIB[0];
    const catName = (CATS.find(c => c[0] === d.cat) || [, ''])[1];
    document.getElementById('dgCrumb').textContent = d.name;
    document.title = `${d.name} · مكتبة لوحات جيم`;
    head.innerHTML = `
      <div class="dg-title"><span class="badge badge-blue">${catName}</span><h1>${d.name}</h1><span class="dg-en" dir="ltr">${d.en}</span></div>
      <p>${d.desc}</p>
      <div class="dg-meta"><span>${ic('layers')}${d.p.length} ${d.p.length > 1 ? 'صفحات' : 'صفحة'}</span><span>${ic('dashboard')}${CAT.countWidgets(d)} عنصرًا</span>${d.sources.map(s => `<span class="src-tag">${ic('database')}${s}</span>`).join('')}
        <span class="dg-demo">${ic('info')}بيانات تجريبية للعرض · تتغير مع الفلاتر</span></div>`;
    JR.mount(d, grid);
    drawNav();
    nav.querySelector('.is-active')?.scrollIntoView({ block: 'nearest' });
    document.querySelector('.dg-canvas').scrollTo?.({ top: 0 }); window.scrollTo({ top: 0 });
  };

  document.querySelector('.dg-search input').addEventListener('input', e => { q = e.target.value.trim().toLowerCase(); drawNav(); });
  addEventListener('hashchange', open);
  open();
})();
