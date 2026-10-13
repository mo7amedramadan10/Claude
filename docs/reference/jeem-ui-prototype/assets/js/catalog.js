/* ==========================================================
   جيم — كتالوج النماذج (مصدر واحد للبيانات)
   يُستخدم في: تبويب «النماذج» في المحادثة، ومعرض «لوحة جديدة»،
   وصفحة «مكتبة النماذج» لمسؤول المنصّة.
   كل نموذج يحمل «وصف الذكاء الاصطناعي» (prompt): النص الذي يُرسل لجيم
   عند اختيار النموذج، بعد تعويض المتغيرات {{...}} بقيم المشروع.
   في التطبيق الفعلي: جداول WidgetTemplates / DashboardTemplates على مستوى
   المنصّة + GET/PUT من الـ API. هنا التعديلات تُحفظ في localStorage
   (jeem.catalog.v1) لمحاكاة أن تعديل المسؤول يظهر فورًا للمستخدمين.
   ========================================================== */
window.JEEM_CATALOG = (function () {
  const C1 = 'var(--chart-1)', C2 = 'var(--chart-2)', C3 = 'var(--chart-3)', C4 = 'var(--chart-4)';

  /* ---------- المتغيرات المتاحة داخل الوصف ---------- */
  const VARS = [
    { key: 'project_name', label: 'اسم المشروع', sample: 'منصة المبيعات', scope: 'all' },
    { key: 'org_name', label: 'اسم المنظمة', sample: 'شركة س', scope: 'all' },
    { key: 'sources', label: 'مصادر المشروع المتصلة', sample: 'ERP المبيعات، CRM، المستهدفات-2026.xlsx', scope: 'all' },
    { key: 'period', label: 'الفترة المختارة في اللوحة', sample: 'من يناير إلى سبتمبر 2026', scope: 'all' },
    { key: 'currency', label: 'عملة المنظمة', sample: 'ريال سعودي', scope: 'all' },
    { key: 'widgets', label: 'قائمة عناصر اللوحة العشرة', sample: '', scope: 'dashboard' },
    { key: 'kpi_name', label: 'اسم المؤشر (عربي)', sample: 'نسبة رضا العملاء', scope: 'kpi' },
    { key: 'kpi_name_en', label: 'اسم المؤشر (إنجليزي)', sample: 'Customer satisfaction rate', scope: 'kpi' },
    { key: 'category', label: 'تصنيف المؤشر', sample: 'خدمة العملاء', scope: 'kpi' },
    { key: 'measure_type', label: 'نوع القياس', sample: 'نسبة', scope: 'kpi' }
  ];

  /* ---------- القواعد العامة: تُرسل مع أي نموذج ---------- */
  const RULES = `أنت جيم، مساعد بناء لوحات المؤشرات داخل مشروع «{{project_name}}» في منظمة «{{org_name}}».
- استخدم فقط البيانات الموجودة في مصادر المشروع المتصلة: {{sources}}. لا تخترع أرقامًا.
- إذا لم يتوفر مصدر أو عمود مطلوب، اسأل المستخدم بسؤال واحد واضح بدل التخمين.
- الفترة الافتراضية: {{period}}، والعملة: {{currency}}.
- اكتب العناوين والتسميات بالعربية الفصحى المختصرة، والأرقام بالأرقام الإنجليزية.
- أرجع العنصر بصيغة JSON المعتمدة للعناصر (type, title, query, visual)، مع شرح طريقة الحساب في حقل calc.`;

  /* ---------- القالب العام لمؤشرات المكتبة (1,899 مؤشرًا) ---------- */
  const KPI_TEMPLATE = `أنشئ عنصر مؤشر لـ«{{kpi_name}}» ({{kpi_name_en}}) ضمن تصنيف «{{category}}».
- نوع القياس: {{measure_type}}. اختر الشكل المناسب: النسبة بشريط تقدم مقابل المستهدف، والزمن بقيمة واتجاه (الأقل أفضل)، والقيمة المالية بخط زمني، والعدد بأعمدة شهرية.
- ابحث في {{sources}} عن الجداول والأعمدة التي تحسب هذا المؤشر، واعرض على المستخدم طريقة الحساب المقترحة ليؤكدها قبل الحفظ.
- الفترة: {{period}}، مع المقارنة بالفترة السابقة.`;

  /* ---------- عناصر المحادثة («النماذج») ---------- */
  const WIDGETS = [
    { id: 'kpi',      cat: 'kpi',   span: 3,  name: 'مؤشر رقمي مع اتجاه',      desc: 'صافي الربح مع مقارنة سنوية',                 src: 'ERP · gl_entries' },
    { id: 'progress', cat: 'kpi',   span: 5,  name: 'تحقيق المستهدف بالمناطق', desc: 'نسبة الإنجاز لكل منطقة مقابل مستهدفها',      src: 'المستهدفات-2026.xlsx' },
    { id: 'compare',  cat: 'chart', span: 4,  name: 'مقارنة سنوية',             desc: 'مبيعات كل ربع: 2025 مقابل 2026',             src: 'ERP · sales_orders' },
    { id: 'line',     cat: 'chart', span: 4,  name: 'نمو العملاء الجدد',         desc: 'منحنى تراكمي للعملاء الجدد شهريًا',          src: 'ERP · customers' },
    { id: 'donut',    cat: 'chart', span: 4,  name: 'توزيع قنوات البيع',         desc: 'حصة كل قناة من إجمالي المبيعات',             src: 'ERP · sales_orders' },
    { id: 'funnel',   cat: 'chart', span: 5,  name: 'مراحل الصفقات',             desc: 'من عرض السعر حتى إغلاق الصفقة',              src: 'CRM · opportunities' },
    { id: 'heat',     cat: 'chart', span: 7,  name: 'أوقات الذروة',              desc: 'الطلبات حسب اليوم والساعة',                  src: 'ERP · sales_orders' },
    { id: 'table',    cat: 'table', span: 7,  name: 'أعلى العملاء',              desc: 'ترتيب العملاء حسب قيمة المشتريات',           src: 'ERP · customers' }
  ];
  const WPROMPT = {
    kpi: `أنشئ مؤشرًا رقميًا لـ«صافي الربح» للفترة {{period}}.
- الحساب: إجمالي الإيرادات − إجمالي المصروفات، من القيود المرحّلة والمعتمدة فقط في {{sources}}.
- اعرض القيمة بالمليون {{currency}} بخانة عشرية واحدة.
- أضف نسبة التغيّر مقارنة بنفس الفترة من السنة السابقة، وخطًا صغيرًا لآخر 7 أشهر.`,
    progress: `اعرض نسبة تحقيق المستهدف لكل منطقة للفترة {{period}}.
- الحساب: (المبيعات الفعلية ÷ المستهدف) × 100. الفعلي من {{sources}} والمستهدف من ملف المستهدفات المعتمد.
- رتّب المناطق تنازليًا واكتفِ بأعلى 6، وميّز المناطق التي تجاوزت 100%.
- إذا لم يوجد ملف مستهدفات فأبلغ المستخدم ولا تفترض قيمًا.`,
    compare: `قارن إجمالي المبيعات لكل ربع بين السنة الحالية والسنة السابقة كأعمدة متجاورة، بالمليون {{currency}}.
- استبعد المرتجعات والطلبات الملغاة.
- إذا كان الربع الحالي غير مكتمل فاذكر ذلك تحت الرسم.`,
    line: `ارسم منحنى تراكميًا لعدد العملاء الجدد شهريًا من بداية السنة حتى نهاية الفترة ({{period}}).
- العميل الجديد = أول طلب مكتمل له خلال الشهر.
- اكتب إجمالي العملاء الجدد في الفترة تحت المنحنى.`,
    donut: `اعرض توزيع المبيعات حسب قناة البيع للفترة {{period}} كنسب مئوية من الإجمالي.
- بحد أقصى 4 شرائح، وما زاد يُجمع في «أخرى».
- اكتب الإجمالي بالمليون {{currency}} في منتصف الدائرة.`,
    funnel: `اعرض قمع الصفقات من {{sources}} للفترة {{period}}: عدد الصفقات في كل مرحلة (عرض سعر، تفاوض، موافقة مبدئية، مغلقة ناجحة).
- أضف نسبة التحويل بين كل مرحلتين متتاليتين.`,
    heat: `اعرض خريطة حرارية لعدد الطلبات حسب يوم الأسبوع (الأحد–الخميس) والساعة (9 ص–11 م) خلال آخر 4 أسابيع.
- استخدم توقيت الرياض، ودرّج اللون حسب عدد الطلبات.`,
    table: `اعرض جدولًا بأعلى 10 عملاء حسب قيمة المشتريات للفترة {{period}}.
- الأعمدة: اسم العميل، قيمة المشتريات، عدد الطلبات. رتّب تنازليًا حسب القيمة.
- لا تعرض بيانات التواصل الشخصية للعملاء.`
  };
  WIDGETS.forEach(w => Object.assign(w, { render: w.id, prompt: WPROMPT[w.id], status: 'published', plans: ['basic', 'business', 'enterprise'], version: 3, updated: '2026-09-14', by: 'أحمد الزهراني' }));
  const WIDGET_CATS = [['kpi', 'مؤشرات'], ['chart', 'رسوم بيانية'], ['table', 'جداول']];
  const RENDER_TYPES = [['kpi', 'مؤشر رقمي مع اتجاه'], ['progress', 'أشرطة تقدم'], ['compare', 'أعمدة مقارنة'], ['line', 'منحنى زمني'], ['donut', 'دائرة توزيع'], ['funnel', 'قمع مراحل'], ['heat', 'خريطة حرارية'], ['table', 'جدول']];

  /* مصغّرات عناصر المحادثة */
  const T = {
    kpi: `<svg viewBox="0 0 120 64"><rect x="10" y="12" width="40" height="6" rx="3" fill="#D6DCE2"/><rect x="10" y="26" width="58" height="14" rx="4" fill="${C1}"/><polyline points="10,56 30,50 50,53 70,44 90,46 110,34" fill="none" stroke="${C4}" stroke-width="3" stroke-linecap="round"/></svg>`,
    compare: `<svg viewBox="0 0 120 64">${[18, 30, 24, 38, 32].map((h, i) => `<rect x="${12 + i * 21}" y="${58 - h}" width="7" height="${h}" rx="2" fill="#BFD9EC"/><rect x="${20 + i * 21}" y="${58 - h - 8}" width="7" height="${h + 8}" rx="2" fill="${C1}"/>`).join('')}</svg>`,
    line: `<svg viewBox="0 0 120 64"><path d="M8 52 L28 44 L48 47 L68 34 L88 30 L112 14 L112 58 L8 58Z" fill="${C2}" opacity=".15"/><polyline points="8,52 28,44 48,47 68,34 88,30 112,14" fill="none" stroke="${C2}" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"/></svg>`,
    progress: `<svg viewBox="0 0 120 64">${[88, 70, 54, 36].map((w, i) => `<rect x="10" y="${8 + i * 14}" width="100" height="7" rx="3.5" fill="#EEF1F4"/><rect x="${110 - w}" y="${8 + i * 14}" width="${w}" height="7" rx="3.5" fill="${i === 0 ? C4 : C1}"/>`).join('')}</svg>`,
    table: `<svg viewBox="0 0 120 64"><rect x="8" y="8" width="104" height="9" rx="3" fill="#E4E8EC"/>${[0, 1, 2, 3].map(i => `<rect x="${78}" y="${23 + i * 10}" width="34" height="5" rx="2.5" fill="#C9D1D8"/><rect x="${8}" y="${23 + i * 10}" width="${44 - i * 8}" height="5" rx="2.5" fill="${C1}"/>`).join('')}</svg>`,
    donut: `<svg viewBox="0 0 120 64"><circle cx="60" cy="32" r="22" fill="none" stroke="${C1}" stroke-width="10" stroke-dasharray="62 200" transform="rotate(-90 60 32)"/><circle cx="60" cy="32" r="22" fill="none" stroke="${C2}" stroke-width="10" stroke-dasharray="40 200" stroke-dashoffset="-64" transform="rotate(-90 60 32)"/><circle cx="60" cy="32" r="22" fill="none" stroke="${C3}" stroke-width="10" stroke-dasharray="24 200" stroke-dashoffset="-106" transform="rotate(-90 60 32)"/><circle cx="60" cy="32" r="22" fill="none" stroke="${C4}" stroke-width="10" stroke-dasharray="10 200" stroke-dashoffset="-132" transform="rotate(-90 60 32)"/></svg>`,
    heat: `<svg viewBox="0 0 120 64">${Array.from({ length: 28 }, (_, i) => { const o = [.15, .3, .5, .75, 1][(i * 7 + (i % 5) * 3) % 5]; return `<rect x="${12 + (i % 7) * 14}" y="${8 + Math.floor(i / 7) * 13}" width="12" height="11" rx="2" fill="${C1}" opacity="${o}"/>`; }).join('')}</svg>`,
    funnel: `<svg viewBox="0 0 120 64">${[100, 78, 54, 32].map((w, i) => `<rect x="${(120 - w) / 2}" y="${6 + i * 14}" width="${w}" height="11" rx="3" fill="${i === 3 ? C4 : C1}" opacity="${1 - i * .15}"/>`).join('')}</svg>`
  };


  /* ---------- نماذج اللوحات الكاملة («لوحة جديدة») ---------- */
  const K = (title, value, unit, delta, up, trend) => ({ type: 'kpi', title, value, unit, delta, up, trend });
  const M = ['يناير', 'سبتمبر'];
  const DASHBOARDS = [
    { id: 'sales-exec', layout: 'a', tint: '', cat: 'sales', name: 'أداء المبيعات التنفيذي', desc: 'نظرة شاملة للإدارة العليا على المبيعات والمستهدفات والمناطق.', sources: ['ERP المبيعات', 'المستهدفات'], popular: true,
      widgets: [K('إجمالي المبيعات', '50.6', 'مليون ر.س', '12.4%', true), K('عدد الطلبات', '13,420', 'طلب', '6.1%', true), K('متوسط قيمة الطلب', '3,770', 'ر.س', '1.8%', false, [6, 8, 7, 12, 14, 16, 20]), K('تحقيق المستهدف', '97%', '', '4 نقاط', true),
        { type: 'line', title: 'المبيعات الشهرية', sub: 'بالمليون ريال', labels: M, values: [4.6, 4.9, 5.2, 5.0, 5.6, 5.9, 6.1, 6.4, 6.9], note: 'أعلى شهر: سبتمبر' },
        { type: 'donut', title: 'التوزيع حسب القطاع', sub: 'من الإجمالي', total: '50.6م', items: [['التجزئة', 42], ['الصناعي', 28], ['الحكومي', 20], ['الضيافة', 10]] },
        { type: 'hbars', title: 'المبيعات حسب المنطقة', sub: 'بالمليون ريال', unit: 'م', rows: [['الرياض', 17.2], ['مكة المكرمة', 11.8], ['الشرقية', 10.4], ['المدينة المنورة', 4.9], ['القصيم', 3.6]] },
        { type: 'cbars', title: 'مقارنة ربع سنوية', sub: '2025 مقابل 2026', legend: ['2025', '2026'], groups: [['الربع الأول', 11.2, 13.1], ['الربع الثاني', 13.0, 15.7], ['الربع الثالث', 15.4, 19.4]] },
        { type: 'table', title: 'أعلى المنتجات', sub: 'حسب قيمة المبيعات', cols: ['المنتج', 'المبيعات', 'الطلبات'], rows: [['أنظمة التكييف المركزي', '6.84م', 1240], ['معدات التعبئة', '5.12م', 980], ['مستلزمات المكاتب', '4.37م', 2110], ['الإضاءة الذكية', '3.95م', 760]] },
        { type: 'funnel', title: 'مراحل الصفقات', sub: 'الربع الحالي', rows: [['عروض الأسعار', 3240], ['تفاوض', 1820], ['موافقة مبدئية', 960], ['صفقات مغلقة', 612]] }] },

    { id: 'pmo', layout: 'b', tint: 'var(--brand-purple)', cat: 'projects', name: 'متابعة المشاريع والمبادرات', desc: 'حالة المشاريع ونسب الإنجاز والميزانيات والمخاطر لمكتب إدارة المشاريع.', sources: ['نظام المشاريع', 'الميزانية'], popular: true,
      widgets: [K('المشاريع النشطة', '48', 'مشروع', '6', true), K('متوسط الإنجاز', '64%', '', '5 نقاط', true), K('مشاريع متأخرة', '9', 'مشروع', '2', false, [6, 8, 10, 12, 11, 16, 20]), K('الصرف من الميزانية', '58%', '', '3 نقاط', true),
        { type: 'line', title: 'نسبة الإنجاز التراكمية', sub: 'الفعلي على مستوى المحفظة', labels: M, values: [8, 15, 22, 29, 36, 44, 51, 58, 64], note: 'المخطط: 70%' },
        { type: 'donut', title: 'المشاريع حسب الحالة', sub: 'عدد المشاريع', total: '48', totalLabel: 'مشروع', items: [['في المسار', 58], ['تحت المراقبة', 23], ['متأخر', 19]] },
        { type: 'hbars', title: 'الإنجاز حسب القطاع', sub: 'متوسط نسبة الإنجاز', unit: '%', rows: [['التحول الرقمي', 78], ['البنية التحتية', 66], ['الخدمات', 61], ['الموارد البشرية', 54], ['الحوكمة', 47]] },
        { type: 'cbars', title: 'الميزانية: المخطط مقابل الفعلي', sub: 'بالمليون ريال', legend: ['المخطط', 'الفعلي'], groups: [['الربع الأول', 42, 38], ['الربع الثاني', 55, 51], ['الربع الثالث', 61, 49]] },
        { type: 'table', title: 'المشاريع الأعلى مخاطرة', sub: 'تحتاج تدخل', cols: ['المشروع', 'الإنجاز', 'التأخير (يوم)'], rows: [['منصة الخدمات الموحدة', '41%', 46], ['مركز البيانات الثانوي', '38%', 32], ['أتمتة المشتريات', '55%', 21], ['بوابة الموردين', '62%', 14]] },
        { type: 'funnel', title: 'مراحل المبادرات', sub: 'من الفكرة إلى الإغلاق', rows: [['مقترحة', 120], ['معتمدة', 74], ['قيد التنفيذ', 48], ['مكتملة', 22]] }] },

    { id: 'hr', layout: 'c', tint: 'var(--brand-teal)', cat: 'hr', name: 'الموارد البشرية والقوى العاملة', desc: 'التوظيف والدوران الوظيفي والحضور والتوطين في لوحة واحدة.', sources: ['نظام الموارد البشرية', 'الحضور'],
      widgets: [K('إجمالي الموظفين', '2,846', 'موظف', '3.2%', true), K('نسبة التوطين', '71%', '', '4 نقاط', true), K('معدل الدوران', '8.4%', '', '1.1 نقطة', true, [20, 18, 17, 15, 14, 12, 10]), K('نسبة الحضور', '95.6%', '', '0.4 نقطة', false, [8, 9, 7, 10, 12, 11, 14]),
        { type: 'line', title: 'عدد الموظفين شهريًا', sub: 'صافي بعد التعيين والاستقالات', labels: M, values: [2700, 2712, 2731, 2745, 2770, 2788, 2805, 2829, 2846] },
        { type: 'donut', title: 'التوزيع حسب الفئة', sub: 'من إجمالي الموظفين', total: '2,846', totalLabel: 'موظف', items: [['تشغيلي', 48], ['إداري', 27], ['فني', 17], ['قيادي', 8]] },
        { type: 'hbars', title: 'الموظفون حسب الإدارة', sub: 'عدد', unit: '', rows: [['العمليات', 912], ['المبيعات', 604], ['التقنية', 418], ['المالية', 236], ['الموارد البشرية', 121]] },
        { type: 'cbars', title: 'التعيينات مقابل الاستقالات', sub: 'ربع سنوي', legend: ['استقالات', 'تعيينات'], groups: [['الربع الأول', 58, 96], ['الربع الثاني', 64, 88], ['الربع الثالث', 51, 79]] },
        { type: 'table', title: 'وظائف شاغرة طويلة', sub: 'أكثر من 60 يوم', cols: ['الوظيفة', 'الأيام', 'المتقدمون'], rows: [['مهندس بيانات', 94, 38], ['مدير مشاريع أول', 81, 22], ['أخصائي أمن سيبراني', 77, 15], ['محاسب تكاليف', 64, 41]] },
        { type: 'heat', title: 'الغياب حسب اليوم', sub: 'آخر 4 أسابيع', rows: ['الأحد', 'الإثنين', 'الثلاثاء', 'الأربعاء', 'الخميس'], cols: ['أ1', 'أ2', 'أ3', 'أ4'] }] },

    { id: 'finance', layout: 'd', tint: '#0E6399', cat: 'finance', name: 'المالية والميزانية', desc: 'الإيرادات والمصروفات والتدفق النقدي والانحراف عن الميزانية.', sources: ['النظام المالي', 'الميزانية'],
      widgets: [K('الإيرادات', '128.4', 'مليون ر.س', '9.6%', true), K('المصروفات', '96.1', 'مليون ر.س', '4.2%', false, [8, 9, 11, 12, 14, 15, 18]), K('صافي الربح', '32.3', 'مليون ر.س', '14.8%', true), K('الانحراف عن الميزانية', '-3.1%', '', 'ضمن الحد', true),
        { type: 'line', title: 'التدفق النقدي الشهري', sub: 'بالمليون ريال', labels: M, values: [9.2, 8.1, 10.4, 11.0, 9.6, 12.3, 13.1, 12.4, 14.0] },
        { type: 'donut', title: 'المصروفات حسب البند', sub: 'من الإجمالي', total: '96.1م', items: [['الرواتب', 52], ['التشغيل', 24], ['المشتريات', 16], ['أخرى', 8]] },
        { type: 'hbars', title: 'الإيرادات حسب النشاط', sub: 'بالمليون ريال', unit: 'م', rows: [['الخدمات', 54.2], ['المنتجات', 41.8], ['العقود الحكومية', 22.6], ['الاستشارات', 9.8]] },
        { type: 'cbars', title: 'الميزانية مقابل الفعلي', sub: 'المصروفات ربع السنوية', legend: ['الميزانية', 'الفعلي'], groups: [['الربع الأول', 31, 30.2], ['الربع الثاني', 33, 32.9], ['الربع الثالث', 34, 33.0]] },
        { type: 'table', title: 'أكبر المستحقات', sub: 'متأخرة السداد', cols: ['العميل', 'المبلغ', 'الأيام'], rows: [['جهة حكومية أ', '4.2م', 96], ['شركة البحر الأحمر', '2.8م', 71], ['مجموعة النخبة', '1.9م', 58], ['مستشفيات الشفاء', '1.1م', 44]] },
        { type: 'funnel', title: 'دورة التحصيل', sub: 'من الفاتورة إلى التحصيل', rows: [['فواتير صادرة', 1840], ['مستلمة', 1610], ['معتمدة', 1320], ['محصّلة', 1105]] }] },

    { id: 'procurement', layout: 'c', tint: 'var(--brand-purple)', cat: 'procurement', name: 'المشتريات والموردين', desc: 'أوامر الشراء وأداء الموردين ودورة الاعتماد والتوفير المحقق.', sources: ['نظام المشتريات'],
      widgets: [K('أوامر الشراء', '1,284', 'أمر', '7.5%', true), K('قيمة المشتريات', '42.7', 'مليون ر.س', '5.3%', true), K('متوسط مدة الاعتماد', '6.2', 'يوم', '1.4 يوم', true, [20, 18, 16, 15, 12, 10, 9]), K('التوفير المحقق', '3.4', 'مليون ر.س', '22%', true),
        { type: 'line', title: 'قيمة المشتريات الشهرية', sub: 'بالمليون ريال', labels: M, values: [4.1, 4.4, 4.9, 4.6, 5.0, 4.8, 5.3, 4.7, 5.1] },
        { type: 'donut', title: 'المشتريات حسب الفئة', sub: 'من القيمة', total: '42.7م', items: [['تقنية', 38], ['خدمات', 27], ['مواد تشغيل', 22], ['أثاث ومعدات', 13]] },
        { type: 'hbars', title: 'أعلى الموردين', sub: 'بالمليون ريال', unit: 'م', rows: [['شركة الحلول المتقدمة', 7.8], ['مؤسسة التوريد الأولى', 5.6], ['مجموعة الخليج', 4.9], ['شركة النظم الذكية', 3.7], ['مصنع الوطنية', 2.9]] },
        { type: 'cbars', title: 'التسليم في الموعد', sub: 'عدد الأوامر', legend: ['متأخر', 'في الموعد'], groups: [['الربع الأول', 61, 342], ['الربع الثاني', 48, 389], ['الربع الثالث', 37, 407]] },
        { type: 'table', title: 'تقييم الموردين', sub: 'من 100', cols: ['المورد', 'الجودة', 'الالتزام'], rows: [['شركة الحلول المتقدمة', 94, 91], ['مجموعة الخليج', 88, 79], ['مؤسسة التوريد الأولى', 82, 86], ['مصنع الوطنية', 76, 70]] },
        { type: 'funnel', title: 'دورة الشراء', sub: 'من الطلب إلى الاستلام', rows: [['طلبات شراء', 1720], ['معتمدة', 1450], ['أوامر شراء', 1284], ['مستلمة', 1102]] }] },

    { id: 'cx', layout: 'b', tint: 'var(--brand-teal)', cat: 'cx', name: 'تجربة العملاء والشكاوى', desc: 'رضا العملاء وزمن الاستجابة والشكاوى حسب القناة والسبب.', sources: ['نظام التذاكر', 'الاستبيانات'],
      widgets: [K('رضا العملاء', '4.3', 'من 5', '0.2', true), K('التذاكر المفتوحة', '312', 'تذكرة', '11%', true, [20, 19, 16, 15, 12, 11, 9]), K('زمن أول رد', '18', 'دقيقة', '6 دقائق', true, [22, 20, 17, 15, 14, 11, 9]), K('مؤشر التوصية NPS', '+41', '', '5 نقاط', true),
        { type: 'line', title: 'التذاكر الواردة', sub: 'شهريًا', labels: M, values: [1820, 1760, 1904, 1688, 1622, 1590, 1510, 1476, 1402] },
        { type: 'donut', title: 'التذاكر حسب القناة', sub: 'من الإجمالي', total: '14.7K', totalLabel: 'تذكرة', items: [['الاتصال', 41], ['التطبيق', 31], ['البريد', 18], ['التواصل الاجتماعي', 10]] },
        { type: 'hbars', title: 'أسباب الشكاوى', sub: 'عدد', unit: '', rows: [['تأخر التسليم', 412], ['جودة المنتج', 286], ['الفوترة', 198], ['خدمة الموظفين', 141], ['أخرى', 87]] },
        { type: 'cbars', title: 'الالتزام بمستوى الخدمة', sub: 'نسبة التذاكر', legend: ['المستهدف', 'الفعلي'], groups: [['الربع الأول', 90, 84], ['الربع الثاني', 90, 88], ['الربع الثالث', 90, 93]] },
        { type: 'table', title: 'أداء الفرق', sub: 'هذا الشهر', cols: ['الفريق', 'تذاكر محلولة', 'الرضا'], rows: [['فريق الرياض', 482, 4.5], ['فريق جدة', 391, 4.3], ['فريق الدمام', 302, 4.1], ['الدعم الرقمي', 276, 4.4]] },
        { type: 'heat', title: 'أوقات الذروة', sub: 'التذاكر حسب اليوم والساعة', rows: ['الأحد', 'الإثنين', 'الثلاثاء', 'الأربعاء', 'الخميس'], cols: ['9ص', '11', '1م', '3', '5', '7'] }] },

    { id: 'ops', layout: 'd', tint: 'var(--brand-gray)', cat: 'ops', name: 'العمليات والتشغيل', desc: 'الإنتاجية وجاهزية الأصول وأوامر العمل والصيانة.', sources: ['نظام التشغيل', 'الصيانة'],
      widgets: [K('أوامر العمل المنجزة', '6,912', 'أمر', '8.3%', true), K('جاهزية الأصول', '96.2%', '', '1.5 نقطة', true), K('أعطال حرجة', '14', 'عطل', '6', true, [20, 17, 18, 13, 12, 10, 8]), K('تكلفة الصيانة', '2.8', 'مليون ر.س', '3.1%', false, [10, 11, 10, 13, 14, 15, 17]),
        { type: 'line', title: 'الإنتاجية اليومية', sub: 'وحدة / يوم', labels: M, values: [820, 846, 871, 860, 902, 915, 934, 951, 968] },
        { type: 'donut', title: 'أوامر العمل حسب النوع', sub: 'من الإجمالي', total: '6,912', totalLabel: 'أمر', items: [['وقائية', 54], ['تصحيحية', 29], ['طارئة', 11], ['تحسين', 6]] },
        { type: 'hbars', title: 'الأعطال حسب الموقع', sub: 'عدد', unit: '', rows: [['المصنع 1', 46], ['المصنع 2', 38], ['المستودع المركزي', 21], ['محطة الضخ', 17], ['المكاتب', 9]] },
        { type: 'cbars', title: 'المخطط مقابل المنفذ', sub: 'أوامر الصيانة الوقائية', legend: ['المخطط', 'المنفذ'], groups: [['الربع الأول', 1200, 1104], ['الربع الثاني', 1250, 1198], ['الربع الثالث', 1300, 1281]] },
        { type: 'table', title: 'الأصول الأكثر توقفًا', sub: 'ساعات التوقف', cols: ['الأصل', 'ساعات', 'أعطال'], rows: [['خط التعبئة 3', 64, 7], ['ضاغط الهواء 2', 41, 5], ['رافعة شوكية 12', 33, 6], ['مولد احتياطي', 22, 2]] },
        { type: 'heat', title: 'الأعطال حسب الوردية', sub: 'آخر 5 أيام', rows: ['الأحد', 'الإثنين', 'الثلاثاء', 'الأربعاء', 'الخميس'], cols: ['صباحية', 'مسائية', 'ليلية'] }] },

    { id: 'marketing', layout: 'b', tint: '#0E8FB8', cat: 'sales', name: 'التسويق والحملات', desc: 'أداء الحملات والعملاء المحتملين وتكلفة الاستحواذ والعائد.', sources: ['CRM', 'منصات الإعلان'],
      widgets: [K('العملاء المحتملون', '8,420', 'عميل', '18%', true), K('معدل التحويل', '7.3%', '', '0.9 نقطة', true), K('تكلفة الاستحواذ', '186', 'ر.س', '12%', true, [22, 20, 19, 16, 14, 12, 10]), K('العائد على الإنفاق', '4.1x', '', '0.6x', true),
        { type: 'line', title: 'العملاء المحتملون شهريًا', sub: 'من كل القنوات', labels: M, values: [620, 704, 760, 812, 870, 935, 1010, 1160, 1349] },
        { type: 'donut', title: 'المصادر', sub: 'من العملاء المحتملين', total: '8,420', totalLabel: 'عميل', items: [['البحث', 36], ['التواصل الاجتماعي', 31], ['الإحالة', 21], ['البريد', 12]] },
        { type: 'hbars', title: 'أفضل الحملات', sub: 'حسب العائد', unit: 'x', rows: [['حملة العودة للمدارس', 6.2], ['اليوم الوطني', 5.4], ['عروض الصيف', 4.1], ['إطلاق المنتج الجديد', 3.3], ['رمضان', 2.9]] },
        { type: 'cbars', title: 'الإنفاق مقابل الإيراد', sub: 'بالمليون ريال', legend: ['الإنفاق', 'الإيراد'], groups: [['الربع الأول', 1.2, 4.1], ['الربع الثاني', 1.4, 5.2], ['الربع الثالث', 1.6, 6.9]] },
        { type: 'table', title: 'أداء القنوات', sub: 'هذا الربع', cols: ['القناة', 'عملاء', 'تحويل'], rows: [['البحث المدفوع', 3030, '8.1%'], ['إنستغرام', 1690, '6.4%'], ['سناب شات', 920, '5.2%'], ['البريد', 1010, '9.7%']] },
        { type: 'funnel', title: 'رحلة العميل', sub: 'من الزيارة للشراء', rows: [['زيارات', 184000], ['تفاعل', 41200], ['عميل محتمل', 8420], ['شراء', 615]] }] }
  ];
  const DASH_CATS = [['all', 'كل النماذج', 'grid'], ['hr', 'الموارد البشرية', 'users'], ['finance', 'المالية', 'wallet'], ['sales', 'المبيعات وإدارة العملاء', 'chart'], ['marketing', 'التسويق الرقمي', 'megaphone'], ['ads', 'الإعلانات المدفوعة', 'target'], ['social', 'التواصل الاجتماعي والمحتوى', 'share'], ['procurement', 'المشتريات والعقود', 'briefcase'],
    ['supply', 'سلاسل الإمداد', 'layers'], ['ops', 'العمليات والتشغيل', 'settings'], ['hse', 'السلامة والصحة المهنية', 'shield'], ['projects', 'المشاريع', 'target'], ['retail', 'التجزئة والتجارة الإلكترونية', 'cart'],
    ['saas', 'الاشتراكات', 'spark'], ['banking', 'البنوك والتأمين', 'sheet'], ['health', 'الرعاية الصحية', 'star'], ['gov', 'القطاع الحكومي', 'file'], ['hospitality', 'الضيافة والسفر', 'calendar'], ['cx', 'خدمة العملاء ونجاحهم', 'headset'], ['dev', 'التطوير والتطبيقات', 'code'], ['other', 'أخرى', 'more']];

  /* ---------- مصغّرة اللوحة: 10 مربعات بنفس توزيع الشبكة ---------- */
  const GLYPH = {
    kpi: '<svg viewBox="0 0 40 20"><rect x="4" y="5" width="16" height="5" rx="1.5" fill="var(--chart-1)"/><rect x="4" y="13" width="26" height="2.5" rx="1.2" fill="#D3DAE1"/></svg>',
    line: '<svg viewBox="0 0 80 30" preserveAspectRatio="none"><polyline points="2,26 14,20 26,22 38,14 50,15 62,8 78,4" fill="none" stroke="var(--chart-1)" stroke-width="2.4" vector-effect="non-scaling-stroke"/></svg>',
    donut: '<svg viewBox="0 0 30 30"><circle cx="15" cy="15" r="9" fill="none" stroke="var(--chart-1)" stroke-width="5" stroke-dasharray="26 60" transform="rotate(-90 15 15)"/><circle cx="15" cy="15" r="9" fill="none" stroke="var(--chart-2)" stroke-width="5" stroke-dasharray="17 60" stroke-dashoffset="-27" transform="rotate(-90 15 15)"/><circle cx="15" cy="15" r="9" fill="none" stroke="var(--chart-3)" stroke-width="5" stroke-dasharray="12 60" stroke-dashoffset="-45" transform="rotate(-90 15 15)"/></svg>',
    hbars: '<svg viewBox="0 0 40 24">' + [30, 22, 16, 10].map((w, i) => `<rect x="${38 - w}" y="${2 + i * 6}" width="${w}" height="4" rx="1.2" fill="var(--chart-1)"/>`).join('') + '</svg>',
    cbars: '<svg viewBox="0 0 40 24">' + [10, 14, 18].map((h, i) => `<rect x="${5 + i * 12}" y="${22 - h}" width="4" height="${h}" rx="1" fill="#BFD9EC"/><rect x="${10 + i * 12}" y="${20 - h}" width="4" height="${h + 2}" rx="1" fill="var(--chart-1)"/>`).join('') + '</svg>',
    table: '<svg viewBox="0 0 40 24"><rect x="2" y="2" width="36" height="4" rx="1" fill="#D3DAE1"/>' + [0, 1, 2].map(i => `<rect x="2" y="${9 + i * 5}" width="${18 - i * 3}" height="2.5" rx="1" fill="var(--chart-1)"/><rect x="28" y="${9 + i * 5}" width="10" height="2.5" rx="1" fill="#D3DAE1"/>`).join('') + '</svg>',
    funnel: '<svg viewBox="0 0 40 24">' + [34, 26, 18, 10].map((w, i) => `<rect x="${(40 - w) / 2}" y="${2 + i * 5.5}" width="${w}" height="4" rx="1" fill="${i === 3 ? 'var(--chart-4)' : 'var(--chart-1)'}"/>`).join('') + '</svg>',
    heat: '<svg viewBox="0 0 40 24">' + Array.from({ length: 18 }, (_, i) => `<rect x="${3 + (i % 6) * 6}" y="${3 + Math.floor(i / 6) * 6.5}" width="5" height="5.5" rx="1" fill="var(--chart-1)" opacity="${[.25, .5, .8, 1, .4, .65][(i * 5) % 6]}"/>`).join('') + '</svg>'
  };
  /* تنويع التوزيع: كل نموذج له ترتيب ومساحات مختلفة للرسوم الستة (كل صف = 12) */
  const LAYOUTS = {
    a: { order: [0, 1, 2, 3, 4, 5], spans: [8, 4, 6, 6, 7, 5] },
    b: { order: [3, 1, 0, 2, 5, 4], spans: [7, 5, 8, 4, 5, 7] },
    c: { order: [1, 0, 4, 5, 2, 3], spans: [4, 8, 7, 5, 6, 6] },
    d: { order: [2, 3, 0, 5, 1, 4], spans: [6, 6, 8, 4, 5, 7] }
  };
  const arrange = d => {
    if (d.p && window.JR) return window.JR.thumbItems(d);
    const L = LAYOUTS[d.layout || 'a'], kpis = d.widgets.slice(0, 4), charts = d.widgets.slice(4);
    return kpis.map(w => ({ ...w, span: 3 })).concat(L.order.map((o, i) => ({ ...charts[o], span: L.spans[i] })));
  };
  const thumb = d => `<span class="dash-thumb" style="${d.tint ? `--chart-1:${d.tint}` : ''}">${arrange(d).map(w => `<i class="s${w.span} t-${w.type}">${GLYPH[w.type]}</i>`).join('')}</span>`;
  const DPROMPT = {
    'sales-exec': `ابنِ لوحة «أداء المبيعات التنفيذي» للإدارة العليا من {{sources}} للفترة {{period}}، بالعناصر العشرة التالية بنفس الترتيب:
{{widgets}}
- ركّز على الانحراف عن المستهدف وأكبر المساهمين في التغيّر (منطقة، قطاع، منتج).
- اكتب أعلى اللوحة ملخصًا من سطرين بأهم ملاحظة يحتاجها صاحب القرار.`,
    pmo: `ابنِ لوحة «متابعة المشاريع والمبادرات» لمكتب إدارة المشاريع من {{sources}} للفترة {{period}}، بالعناصر التالية:
{{widgets}}
- المشروع «متأخر» إذا تجاوز الفارق بين الإنجاز الفعلي والمخطط 10 نقاط أو تجاوز تاريخ انتهائه.
- ابرز المشاريع التي تحتاج تدخل الإدارة أولًا.`,
    hr: `ابنِ لوحة «الموارد البشرية والقوى العاملة» من {{sources}} للفترة {{period}}، بالعناصر التالية:
{{widgets}}
- معدل الدوران = الاستقالات وإنهاء الخدمة ÷ متوسط عدد الموظفين.
- لا تعرض أي بيانات شخصية أو رواتب على مستوى الموظف.`,
    finance: `ابنِ لوحة «المالية والميزانية» من {{sources}} للفترة {{period}} بعملة {{currency}}، بالعناصر التالية:
{{widgets}}
- اعتمد القيود المرحّلة فقط، واحسب الانحراف = (الفعلي − الميزانية) ÷ الميزانية.
- ميّز البنود التي تجاوزت الميزانية بأكثر من 5%.`,
    procurement: `ابنِ لوحة «المشتريات والموردين» من {{sources}} للفترة {{period}}، بالعناصر التالية:
{{widgets}}
- مدة الاعتماد = من تاريخ إنشاء طلب الشراء إلى اعتماد أمر الشراء، بالأيام.
- التوفير = (القيمة التقديرية − قيمة الترسية) ÷ القيمة التقديرية.`,
    cx: `ابنِ لوحة «تجربة العملاء والشكاوى» من {{sources}} للفترة {{period}}، بالعناصر التالية:
{{widgets}}
- زمن أول رد بالدقائق خلال ساعات العمل فقط.
- صنّف أسباب الشكاوى من حقل التصنيف إن وُجد، وإلا اقترح تصنيفًا للمستخدم.`,
    ops: `ابنِ لوحة «العمليات والتشغيل» من {{sources}} للفترة {{period}}، بالعناصر التالية:
{{widgets}}
- جاهزية الأصول = ساعات التشغيل المتاحة ÷ الساعات المخططة.
- العطل «حرج» حسب تصنيف الأولوية في نظام الصيانة.`,
    marketing: `ابنِ لوحة «التسويق والحملات» من {{sources}} للفترة {{period}}، بالعناصر التالية:
{{widgets}}
- تكلفة الاستحواذ = إنفاق الحملات ÷ عدد العملاء الجدد من نفس الحملات.
- العائد على الإنفاق = الإيراد المنسوب للحملة ÷ إنفاقها.`
  };
  DASHBOARDS.forEach(d => Object.assign(d, { prompt: DPROMPT[d.id], status: 'published', plans: ['basic', 'business', 'enterprise'], version: 2, updated: '2026-09-21', by: 'أحمد الزهراني' }));
  /* مكتبة اللوحات الجاهزة (55 لوحة متعددة الصفحات) — assets/js/dash-library.js */
  (window.JEEM_DASH_LIBRARY || []).forEach(d => DASHBOARDS.push(Object.assign({ layout: 'a', status: 'published', plans: ['business', 'enterprise'], version: 1, updated: '2026-10-06', by: 'أحمد الزهراني' }, d)));
  const countWidgets = d => d.p ? d.p.reduce((a, p) => a + p.w.length, 0) : (d.widgets || []).length;
  const CHART_TYPES = [['line', 'منحنى زمني'], ['donut', 'دائرة توزيع'], ['hbars', 'أشرطة أفقية'], ['cbars', 'أعمدة مقارنة'], ['table', 'جدول'], ['funnel', 'قمع مراحل'], ['heat', 'خريطة حرارية']];

  /* ---------- طبقة التعديلات (محاكاة الـ API) ---------- */
  const KEY = 'jeem.catalog.v1';
  const blank = () => ({ patch: { w: {}, d: {}, k: {} }, custom: { w: [], d: [], k: [] }, rules: null, kpiTpl: null, catOff: {} });
  let S = blank();
  try { const raw = localStorage.getItem(KEY); if (raw) S = Object.assign(blank(), JSON.parse(raw)); } catch (e) { }
  const persist = () => { try { localStorage.setItem(KEY, JSON.stringify(S)); } catch (e) { } };
  const clone = o => JSON.parse(JSON.stringify(o));
  const BASE = { w: WIDGETS, d: DASHBOARDS };
  const today = () => new Date().toISOString().slice(0, 10);

  const list = (kind, all) => BASE[kind].map(b => S.patch[kind][b.id] ? Object.assign(clone(b), S.patch[kind][b.id], { patched: true }) : b)
    .concat(S.custom[kind].map(x => Object.assign({}, x, { custom: true })))
    .filter(x => all || x.status === 'published');

  /* مكتبة المؤشرات: دمج التعديلات والمؤشرات المضافة في نفس البيانات */
  const LIB = window.JEEM_KPI_LIBRARY;
  if (LIB) {
    LIB.baseCount = LIB.rows.length;
    Object.entries(S.patch.k).forEach(([i, p]) => { const r = LIB.rows[+i]; if (!r) return; if (p.ar) r[2] = p.ar; if (p.en) r[3] = p.en; if (p.mt != null) r[4] = p.mt; if (p.cat != null) r[0] = p.cat; });
    S.custom.k.forEach(k => LIB.rows.push([k.cat, 0, k.ar, k.en, k.mt]));
  }
  const kpiMeta = i => { const b = LIB && i >= LIB.baseCount ? S.custom.k[i - LIB.baseCount] : S.patch.k[i]; return b || {}; };

  return {
    VARS, RENDER_TYPES, CHART_TYPES, WIDGET_CATS, DASH_CATS, THUMB: T, GLYPH, LAYOUTS, arrange, thumb,
    widgets: all => list('w', all),
    dashboards: all => list('d', all),
    get: (kind, id) => list(kind, true).find(x => x.id === id),
    save(kind, item) {
      const it = clone(item); delete it.patched; delete it.custom;
      it.version = (it.version || 0) + 1; it.updated = today(); it.by = 'أحمد الزهراني';
      const ci = S.custom[kind].findIndex(x => x.id === it.id);
      if (ci > -1) S.custom[kind][ci] = it;
      else if (BASE[kind].some(b => b.id === it.id)) S.patch[kind][it.id] = it;
      else { it.id = (kind === 'w' ? 'w-' : 'd-') + Date.now().toString(36); it.version = 1; S.custom[kind].push(it); }
      persist(); return it;
    },
    remove(kind, id) { S.custom[kind] = S.custom[kind].filter(x => x.id !== id); persist(); },
    reset(kind, id) { delete S.patch[kind][id]; persist(); },
    /* مكتبة المؤشرات */
    /* الحالة الفعلية = حالة المؤشر نفسه، إلا لو تصنيفه كله موقوف */
    kpiOwnStatus: i => kpiMeta(i).status || 'published',
    kpiStatus: i => (LIB && LIB.rows[i] && S.catOff[LIB.rows[i][0]]) ? 'off' : (kpiMeta(i).status || 'published'),
    /* إيقاف / إظهار تصنيف كامل بكل مؤشراته — لا يغيّر حالة المؤشرات نفسها، فتعود كما كانت عند الإظهار */
    catOn: c => !(S.catOff || {})[c],
    setCats(cats, on) { S.catOff = S.catOff || {}; [].concat(cats).forEach(c => { if (on) delete S.catOff[c]; else S.catOff[c] = true; }); persist(); },
    kpiPrompt: i => kpiMeta(i).prompt || '',
    isCustomKpi: i => !!LIB && i >= LIB.baseCount,
    isPatchedKpi: i => !!S.patch.k[i],
    saveKpi(i, data) {
      if (LIB && i >= LIB.baseCount) { Object.assign(S.custom.k[i - LIB.baseCount], data); }
      else if (i == null) { S.custom.k.push(data); i = LIB.rows.length; }
      else S.patch.k[i] = Object.assign(S.patch.k[i] || {}, data);
      const r = LIB.rows[i] || (LIB.rows[i] = [0, 0, '', '', 5]);
      r[0] = data.cat; r[2] = data.ar; r[3] = data.en; r[4] = data.mt;
      persist(); return i;
    },
    resetKpi(i) { delete S.patch.k[i]; persist(); },
    get rules() { return S.rules || RULES; }, set rules(v) { S.rules = v === RULES ? null : v; persist(); },
    get kpiTemplate() { return S.kpiTpl || KPI_TEMPLATE; }, set kpiTemplate(v) { S.kpiTpl = v === KPI_TEMPLATE ? null : v; persist(); },
    defaults: { rules: RULES, kpiTemplate: KPI_TEMPLATE },
    /* تعويض المتغيرات */
    fill(text, extra = {}) {
      const ctx = Object.fromEntries(VARS.map(v => [v.key, v.sample]));
      Object.assign(ctx, extra);
      return (text || '').replace(/\{\{\s*([a-z_]+)\s*\}\}/g, (m, k) => ctx[k] != null && ctx[k] !== '' ? ctx[k] : m);
    },
    unknownVars: text => [...new Set([...(text || '').matchAll(/\{\{\s*([^}]+?)\s*\}\}/g)].map(m => m[1]).filter(k => !VARS.some(v => v.key === k)))],
    countWidgets,
    widgetList: d => d.p ? d.p.map((p, i) => `صفحة ${i + 1} «${p.n}»: ${p.w.map(w => w.ti).join('، ')}`).join('\n') : arrange(d).map((w, i) => `${i + 1}. ${w.title} (${w.type === 'kpi' ? 'مؤشر رقمي' : (CHART_TYPES.find(c => c[0] === w.type) || [, w.type])[1]})${w.prompt ? ' — ' + w.prompt : ''}`).join('\n'),
    resetAll() { S = blank(); persist(); }
  };
})();
