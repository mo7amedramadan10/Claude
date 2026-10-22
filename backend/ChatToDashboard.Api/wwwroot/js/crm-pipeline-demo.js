/* بيانات تجريبية ثابتة لمعاينة "إدارة العملاء — مراحل البيع" في معرض النماذج فقط —
   نفس أرقام وأسماء النموذج الأولي بالضبط، بشكل data المطلوب لـ window.JEEM_CRM_MOUNT. */
(function () {
  const ST = [
    { key: 'contact', name: 'تم التواصل' },
    { key: 'nego', name: 'تفاوض' },
    { key: 'offer', name: 'أُرسل العرض' },
    { key: 'won', name: 'صفقة مغلقة', isWon: true },
  ];
  const MEM = [
    { id: 'sa', name: 'سارة البيشي', role: 'مديرة منتج' },
    { id: 'an', name: 'أنس القرشي', role: 'مدير مبيعات' },
    { id: 'ja', name: 'جمال الكناني', role: 'مدير نمو' },
    { id: 'ka', name: 'كارمن الرشيد', role: 'أخصائية تسويق رقمي' },
  ];
  const ADDR = ['طريق الملك فهد، الرياض', 'حي الشاطئ، جدة', 'حي الفيصلية، الدمام'];
  const RAW = [
    ['جسر البيانات', 'حماية البيانات المؤسسية والشخصية بحلول متكاملة', 'contact', '2026-10-18', 2, 1, 'sa'],
    ['ذكاء للحلول', 'حلول مبتكرة قائمة على الذكاء الاصطناعي', 'contact', '2026-10-21', 1, 3, 'an'],
    ['رافد النمو', 'استقطاب العملاء وأتمتة التسويق للمنشآت الصغيرة', 'contact', '', 4, 7, 'me'],
    ['مدار الاستشارات', 'استشارات إدارية للقطاع غير الربحي', 'contact', '2026-10-08', 0, 2, 'ja'],
    ['منصة مهارة', 'منصة لتطوير مهارات المتخصصين', 'nego', '2026-10-09', 4, 1, 'an'],
    ['وافي للرعاية', 'دعم واستشارات نفسية عن بُعد', 'nego', '', 7, 2, 'me'],
    ['سفن للشحن', 'نقل دولي للمواد والمنتجات الكيميائية', 'nego', '2026-10-23', 2, 6, 'ja'],
    ['ركاز للتعدين', 'خدمات مسح وتحليل جيولوجي', 'nego', '2026-11-02', 1, 0, 'ka'],
    ['غذاء صحي', 'أغذية ومكملات صحية للأفراد', 'offer', '2026-10-10', 1, 3, 'sa'],
    ['دار العقار الأول', 'تطوير عقاري للفئة الفاخرة والعقارات التجارية', 'offer', '2026-10-16', 1, 1, 'an', 'حي الملقا، الرياض', 'contact@daraqar.sa'],
    ['جامعة المستقبل', 'منصة تعليم إلكتروني للطلاب الدوليين', 'offer', '2026-10-30', 3, 2, 'ka'],
    ['سحابة الأعمال', 'خدمات سحابية لتخزين البيانات ومعالجتها', 'won', '2026-09-24', 2, 1, 'me'],
    ['ميديا المتقدمة', 'دورة كاملة للإعلان الرقمي والتسويق عبر الشبكات', 'won', '2026-10-06', 1, 3, 'ka'],
    ['حلول المصرف الآمن', 'تقنيات مالية مبتكرة وتحول رقمي للبنوك', 'won', '2026-09-30', 4, 7, 'an'],
  ];
  const customers = RAW.map(([name, description, stage, due, cm, at, ownerId, addr, mail], i) => ({
    id: 'c' + i, name, description, stage, due,
    commentsCount: cm, attachmentsCount: at, ownerId,
    address: addr || ADDR[i % 3], email: mail || `info@client${i + 1}.sa`,
  }));
  const won = customers.filter(c => c.stage === 'won').length;
  const successRate = Math.min(99, Math.round((won + 31) / (won + 46) * 100));
  const tasksInProgress = customers.filter(c => c.stage !== 'won').length * 5 - 2;
  const prepayments = 15890;

  window.JEEM_CRM_DEMO = {
    id: 'crm-pipeline', name: 'إدارة العملاء — مراحل البيع',
    stages: ST, members: MEM, customers,
    newCustomersDaily: [
      { label: 'السبت', total: 7, referrals: 3 },
      { label: 'الأحد', total: 9, referrals: 4 },
      { label: 'الاثنين', total: 10, referrals: 5 },
      { label: 'الثلاثاء', total: 8, referrals: 2 },
      { label: 'الأربعاء', total: 4, referrals: 3 },
      { label: 'الخميس', total: 9, referrals: 6 },
    ],
    pipelineStats: { successRate, tasksInProgress, prepayments },
    currentUserName: 'أنا', currentUserRole: 'مديرة العلاقات',
    today: '2026-10-11',
  };
})();
