/* مكتبة بيانات تجريبية ثابتة — معاينة المعرض فقط (openWarehouseGalDemo في app.js). لا صلة
   لها بمسار بناء اللوحات الحقيقي. الشكل مطابق لما تتوقعه warehouse-team.js's JEEM_WAREHOUSE_MOUNT. */
(function () {
  const ROLES = { pick: 'منتقي', pack: 'مُعبّئ', fork: 'مشغّل رافعة', recv: 'استلام بضائع' };
  const ZONES = ['المنطقة أ إلى ب', 'المنطقة ج إلى د', 'المنطقة هـ', 'رصيف التحميل 1', 'رصيف التحميل 2', 'منطقة التعبئة'];
  const TASKS = { pick: 'انتقاء طلب', pack: 'تعبئة طلب', fork: 'نقل منصّة', recv: 'تفريغ شحنة' };
  let oid = 5840;
  const W = [
    ['رنا القحطاني', 'pick', 0, 8, 10, 94], ['مريم الشهري', 'pick', 1, 8, 12, 96], ['فيصل العنزي', 'fork', 3, 5, 9, 88],
    ['هند الزهراني', 'pack', 5, 11, 14, 92], ['عمر الدوسري', 'recv', 4, 3, 8, 81], ['لمى الحربي', 'pick', 2, 6, 10, 90],
    ['سعد المطيري', 'pack', 5, 9, 12, 93], ['نواف السبيعي', 'fork', 3, 4, 8, 86], ['أمل العتيبي', 'pick', 0, 7, 11, 91],
    ['تركي الغامدي', 'recv', 4, 2, 6, 78], ['جود الشمري', 'pack', 5, 10, 13, 95], ['ماجد البقمي', 'pick', 1, 5, 10, 84],
  ].map(([n, r, z, d, t, e], i) => ({
    id: 'w' + i, name: n, role: ROLES[r], zone: ZONES[z], done: d, tot: t, eff: e, on: true,
    task: TASKS[r], ord: '#ORD-' + (oid += 3), log: [],
  }));
  const OFF = [
    ['أليسا الحمدان', 'pack', 0, 12, 97], ['جمال الرويلي', 'pack', 1, 10, 89], ['سارة الفيفي', 'pick', 2, 11, 93], ['خالد العمري', 'fork', 3, 7, 85],
    ['منيرة القرني', 'recv', 4, 6, 82], ['بدر الشهراني', 'pick', 0, 9, 90], ['ريم المالكي', 'pack', 5, 13, 96], ['حسن الأحمدي', 'fork', 3, 8, 87],
  ].map(([n, r, z, d, e], i) => ({ id: 'o' + i, name: n, role: ROLES[r], zone: ZONES[z], done: d, eff: e, on: false, ret: ['06:00 ص', '02:00 م', '10:00 م'][i % 3] }));

  const FEED = [
    [W[0], 'ok', `<b class="who">@${W[0].name}</b> أكمل الطلب <code class="num">#ORD-5838</code>`],
    [W[4], 'start', `<b class="who">@${W[4].name}</b> بدأ تفريغ <code class="num">الرصيف 2</code>`],
    [W[2], 'move', `<b class="who">@${W[2].name}</b> نُقل من <code class="num">المنطقة أ</code> إلى <code class="num">المنطقة ب</code>`],
    [W[1], 'ok', `<b class="who">@${W[1].name}</b> أكمل الطلب <code class="num">#ORD-5836</code>`],
  ].reverse().map(([w, k, html]) => ({ w: w.id, html, kind: k, t: '11:45', unread: k !== 'move', fresh: false }));

  window.JEEM_WAREHOUSE_DEMO = {
    id: 'warehouse-team', name: 'نظرة على فريق المستودع',
    staff: W, off: OFF, feed: FEED,
    shift: { nextAt: new Date(Date.now() + 14 * 60 * 1000).toISOString() },
    hourly: { staff: [14, 18, 22, 26, 24, 26, 25], tasks: [8, 16, 22, 28, 20, 26, 22], eff: [84, 88, 90, 93, 89, 91, 92] },
  };
})();
