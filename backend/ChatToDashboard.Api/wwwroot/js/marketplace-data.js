/* ==========================================================
   جيم — بيانات تجريبية لمنصة المشتريات: العملاء ← أنواع الطلبات ← المنصة ← الموردون
   كل الأسماء والأرقام للعرض فقط. اليوم 0 = 1 يناير 2026، واليوم الحالي = 273 (30 سبتمبر).
   الهيكل المتوقع من الـ API:
     clients:   { id, n, short }
     types:     { id, n, col }                ← لون ثابت لكل نوع (بالترتيب)
     suppliers: { id, n, short, types: [] }
     requests:  { id, code, n, client, type, supplier|null, value, created, offerDays, sla, actual, cancelAt? }
       الحالة في أي يوم t تُحسب: قبل الترسية = بانتظار العروض، بعدها قيد التنفيذ، وبعد المدة دون إنجاز = متأخر، ثم مكتمل.
   ========================================================== */
(function () {
  const types = [
    { id: 'T1', n: 'أجهزة ومعدات', col: '#127DBD' },
    { id: 'T2', n: 'مستهلكات', col: '#00BAB3' },
    { id: 'T3', n: 'صيانة وتشغيل', col: '#6E62B0' },
    { id: 'T4', n: 'تقنية معلومات', col: '#28B6E6' },
    { id: 'T5', n: 'خدمات استشارية', col: '#D9822B' }
  ];
  const clients = [
    { id: 'C1', n: 'تجمع الرياض الصحي الأول', short: 'التجمع الصحي الأول', w: { T1: 5, T2: 6, T3: 3, T4: 1, T5: .5 } },
    { id: 'C2', n: 'أمانة منطقة الرياض', short: 'أمانة الرياض', w: { T1: 2, T2: 1, T3: 6, T4: 2, T5: 1 } },
    { id: 'C3', n: 'جامعة الملك سعود', short: 'جامعة الملك سعود', w: { T1: 3, T2: 1, T3: 1, T4: 6, T5: 1 } },
    { id: 'C4', n: 'الهيئة الملكية لمدينة الرياض', short: 'الهيئة الملكية', w: { T1: .5, T2: .2, T3: 3, T4: 2, T5: 6 } },
    { id: 'C5', n: 'الشركة الوطنية للمياه', short: 'الوطنية للمياه', w: { T1: 4, T2: 2, T3: 5, T4: 1, T5: 1 } },
    { id: 'C6', n: 'مستشفى الملك فيصل التخصصي', short: 'مستشفى الملك فيصل', w: { T1: 5, T2: 5, T3: 1, T4: 1, T5: .3 } }
  ];
  const suppliers = [
    { id: 'V1', n: 'شركة الرعاية المتقدمة للتجهيزات الطبية', short: 'الرعاية المتقدمة', types: ['T1', 'T2'], late: .32, speed: 1.2 },
    { id: 'V2', n: 'مؤسسة الحلول الطبية', short: 'الحلول الطبية', types: ['T1', 'T2'], late: .1, speed: .9 },
    { id: 'V3', n: 'شركة المستهلكات الوطنية', short: 'المستهلكات الوطنية', types: ['T2'], late: .18, speed: 1 },
    { id: 'V4', n: 'شركة البناء الحديث للصيانة', short: 'البناء الحديث', types: ['T3'], late: .45, speed: 1.4 },
    { id: 'V5', n: 'مؤسسة الخليج للتشغيل', short: 'الخليج للتشغيل', types: ['T3', 'T1'], late: .15, speed: 1 },
    { id: 'V6', n: 'شركة التقنية الرقمية', short: 'التقنية الرقمية', types: ['T4'], late: .08, speed: .8 },
    { id: 'V7', n: 'مجموعة الأنظمة الذكية', short: 'الأنظمة الذكية', types: ['T4', 'T3'], late: .25, speed: 1.1 },
    { id: 'V8', n: 'بيت الخبرة للاستشارات', short: 'بيت الخبرة', types: ['T5'], late: .06, speed: .9 }
  ];
  const ITEMS = {
    T1: ['أجهزة تنفس صناعي', 'شاشات مراقبة', 'مضخات مياه', 'معدات مختبر', 'أجهزة تعقيم', 'مولدات كهربائية', 'أسرّة طبية', 'أجهزة أشعة محمولة'],
    T2: ['قفازات وكمامات', 'محاليل وريدية', 'مستلزمات غسيل الكلى', 'مواد تنظيف', 'مستلزمات مختبر', 'أدوات جراحية'],
    T3: ['صيانة التكييف المركزي', 'تشغيل محطات الضخ', 'صيانة الإنارة', 'نظافة المرافق', 'صيانة المصاعد', 'صيانة الأجهزة الطبية'],
    T4: ['أجهزة حاسب محمولة', 'خوادم', 'تراخيص برمجيات', 'أجهزة شبكات', 'منصة رقمية', 'شاشات تفاعلية'],
    T5: ['دراسة جدوى', 'استشارات إدارة مشاريع', 'دراسة أثر بيئي', 'تدقيق هندسي', 'تطوير الإجراءات']
  };
  const SLA = { T1: 45, T2: 20, T3: 30, T4: 35, T5: 60 };
  let sd = 4242; const rnd = () => (sd = (sd * 16807) % 2147483647) / 2147483647;
  const pick = w => { const ks = Object.keys(w), tot = ks.reduce((a, k) => a + w[k], 0); let r = rnd() * tot; for (const k of ks) { r -= w[k]; if (r <= 0) return k; } return ks[ks.length - 1]; };

  const requests = [];
  for (let i = 0; i < 190; i++) {
    const c = clients[Math.floor(Math.pow(rnd(), 1.15) * clients.length)], t = pick(c.w);
    const pool = suppliers.filter(v => v.types.includes(t)), v = pool[Math.floor(rnd() * pool.length)];
    const created = Math.round(Math.pow(rnd(), .8) * 268);
    const offerDays = Math.round(3 + rnd() * (t === 'T5' ? 24 : 14));
    const isLate = rnd() < v.late;
    const actual = Math.round(SLA[t] * (isLate ? 1.1 + rnd() * .7 : .45 + rnd() * .5) * v.speed);
    const r = { id: 'M' + (i + 1), code: 'PR-' + (3100 + i), n: ITEMS[t][Math.floor(rnd() * ITEMS[t].length)], client: c.id, type: t, supplier: v.id,
      value: Math.round((t === 'T5' ? 8e5 : t === 'T2' ? 6e4 : 2e5) * (1 + rnd() * 6)), created, offerDays, sla: SLA[t], actual };
    if (rnd() < .06) { r.cancelAt = created + Math.round(2 + rnd() * offerDays); r.supplier = null; }
    requests.push(r);
  }

  window.JEEM_MARKET = {
    today: 273, months: ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو', 'يوليو', 'أغسطس', 'سبتمبر'],
    types, clients, suppliers, requests,
    status: {
      wait: { n: 'بانتظار العروض', col: '#B4BFCA', icon: 'clock' },
      prog: { n: 'قيد التنفيذ', col: '#5F86AE', icon: 'play' },
      late: { n: 'متأخر', col: 'var(--danger)', icon: 'bell' },
      done: { n: 'مكتمل', col: 'var(--ok)', icon: 'check' },
      canc: { n: 'ملغى', col: '#7C7C78', icon: 'x' }
    },
    asks: [
      { q: 'أين تتركز الطلبات المتأخرة؟', kind: 'late' },
      { q: 'أي الموردين يخدمون أكبر عدد من العملاء؟', kind: 'reach' },
      { q: 'ما الطلبات التي ما زالت بانتظار عروض الموردين؟', kind: 'wait' },
      { q: 'ما أكبر مشروع بين عميل ومورد؟', kind: 'pair' }
    ]
  };
})();
