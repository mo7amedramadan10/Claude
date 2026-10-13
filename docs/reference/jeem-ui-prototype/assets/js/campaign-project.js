/* ==========================================================
   جيم — تفاصيل مشروع حملة (لوحة تفاعلية)
   مستوحاة من «Project Management Dashboard» (Flowio). الأصل يرسم الإنفاق والتحويلات
   بمحورين؛ هنا رسمان متزامنان (Small multiples) بنفس محور الزمن، مع توقع متقطع.
   ========================================================== */
(function () {
  const root = document.getElementById('cpRoot'); if (!root || !window.DK) return;
  const K = DK, $ = s => root.querySelector(s);
  const S = { rng: 15, archived: false };
  const ST = { done: ['مكتملة', 'ok', 'check'], prog: ['قيد التنفيذ', 'warn', 'clock'], on: ['مستمرة', 'info', 'history'], pend: ['معلقة', '', 'pause'] };
  const ORDER = ['pend', 'prog', 'on', 'done'];
  let TASKS = [['إعداد التصاميم الإعلانية', 'done', 'علي', '04 نوفمبر'], ['إطلاق إعلانات ميتا', 'prog', 'سارة', '08 نوفمبر'], ['متابعة نسبة النقر يوميًا', 'on', 'منى', '12 نوفمبر'], ['إعداد التقرير النهائي', 'pend', 'خالد', '16 نوفمبر']];
  let FEED = [['wallet', 'سارة حدّثت الميزانية من 5,000 إلى 6,000 ر.س', 'قبل ساعتين', 0], ['image', 'علي رفع 5 تصاميم إعلانية جديدة', 'قبل 3 ساعات', 3], ['up', 'تنبيه أداء: ارتفاع نسبة النقر', 'قبل يوم', 1], ['spark', 'اقتراح ذكي: زيادة الإنفاق على حملة التخفيضات', 'قبل يومين', 5], ['check', 'خالد أكمل اختبار A/B لصفحة الهبوط', 'قبل 3 أيام', 4]];
  let FILES = [['تصاميم_الربيع_v3.pdf', '2.4', 'علي', '10 فبراير'], ['نص_الإعلان_النهائي.docx', '156 ك.ب', 'منى', '08 فبراير'], ['الميزانية.xlsx', '88 ك.ب', 'سارة', '05 فبراير'], ['الجدول_الزمني.pptx', '1.2', 'خالد', '12 فبراير'], ['ملاحظات_العميل.docx', '234 ك.ب', 'منى', '11 فبراير']];

  function kpis() {
    const spent = S.rng === 15 ? 4800 : 9100;
    $('#cpKpis').innerHTML = [['الميزانية الكلية', K.money(12000), ''], ['المنفق', K.money(spent), `<span class="dk-prog"><i style="width:${spent / 120}%"></i></span>`], ['التحويلات', K.num(S.rng === 15 ? 84 : 151, '', 0), ''], ['معدل التحويل', K.pct(4.5), ''], ['الأعضاء المكلفون', K.num(4, '', 0), '<span class="dk-avs"><span class="dk-av" style="--h:10">س</span><span class="dk-av" style="--h:120">ع</span><span class="dk-av" style="--h:220">م</span><span class="dk-av" style="--h:300">خ</span></span>'], ['الأيام المتبقية', K.num(12, 'يومًا', 0), '']]
      .map(([n, v, x], i) => `<div class="dk-kpi" style="--i:${i}"><small>${n}</small><b>${v}</b>${x}</div>`).join('');
    K.count(root);
  }
  function perf() {
    const n = S.rng, f = 7, r = K.rng('cp' + n), dates = Array.from({ length: n + f }, (_, i) => { const d = new Date(2026, 0, 31 - n + i); return d.toLocaleDateString('ar-SA-u-ca-gregory-nu-latn', { day: 'numeric', month: 'short' }); });
    let a = 180, b = 2; const sp = [], cv = [];
    for (let i = 0; i < n + f; i++) { a += 6 + r() * 14 - (i === Math.floor(n * .55) ? 60 : 0); b += .3 + r() * .5 - (i === Math.floor(n * .55) ? 2 : 0); sp.push(Math.round(a)); cv.push(+b.toFixed(1)); }
    const cut = (arr, fc) => arr.map((v, i) => fc ? (i >= n - 1 ? v : null) : (i < n ? v : null));
    const W = K.wOf($('#cpSpend')), lab = dates;
    $('#cpSpend').innerHTML = K.line({ W, H: 170, labels: lab, key: 'x', series: [{ name: 'الإنفاق', color: K.PAL[3], vals: cut(sp), dots: n <= 15 }, { name: 'الإنفاق المتوقع', color: K.PAL[3], vals: cut(sp, 1), dash: 1 }], fmt: v => K.nf(v) + ' ر.س', aria: 'الإنفاق اليومي', band: [n, n + f - 1] });
    $('#cpConv').innerHTML = K.line({ W, H: 170, labels: lab, key: 'x', series: [{ name: 'التحويلات', color: K.PAL[2], vals: cut(cv), dots: n <= 15 }, { name: 'التحويلات المتوقعة', color: K.PAL[2], vals: cut(cv, 1), dash: 1 }], fmt: v => K.nf(v, 1) + ' تحويل', aria: 'التحويلات اليومية', band: [n, n + f - 1] });
    $('#cpLeg').innerHTML = `<div class="dk-leg"><span><i style="background:${K.PAL[3]}"></i>الإنفاق</span><span><i style="background:${K.PAL[2]}"></i>التحويلات</span><span><i class="dash"></i>توقع</span><span><i style="background:color-mix(in srgb,#00A89F 15%,transparent)"></i>فترة التوقع</span></div>`;
  }
  function feed() { $('#cpFeed').innerHTML = FEED.map(([ic, t, w, c], i) => `<li style="--i:${i}"><span class="fi" style="--c:${K.PAL[c]}">${K.ic(ic)}</span><span><b>${K.esc(t)}</b><small>${w}</small></span></li>`).join(''); }
  function tasks() {
    $('#cpTasks').innerHTML = `<table><thead><tr><th>المهمة</th><th>الحالة</th><th>المسؤول</th><th>الاستحقاق</th></tr></thead><tbody>${TASKS.map(([t, s, a, d], i) => `<tr style="--i:${i}"><td><b>${K.esc(t)}</b></td><td><button type="button" class="dk-chip ${ST[s][1]} cp-st" data-st="${i}" aria-label="الحالة: ${ST[s][0]} — اضغط للتغيير">${K.ic(ST[s][2])}${ST[s][0]}</button></td><td><span class="dk-av" style="--h:${a.charCodeAt(0) * 7 % 360}">${a[0]}</span> ${K.esc(a)}</td><td>${K.esc(d)}</td></tr>`).join('')}</tbody></table>`;
  }
  function files() { $('#cpFiles').innerHTML = FILES.map(([n, sz, by, d], i) => { const ext = n.split('.').pop(); return `<li style="--i:${i}"><span class="ft ${ext}">${ext}</span><span><b>${K.esc(n)}</b><small>${/[a-z]/i.test(sz) || sz.includes('ك') ? sz : sz + ' م.ب'} · رفعه ${K.esc(by)} · ${K.esc(d)}</small></span><button type="button" data-dl="${i}" aria-label="تنزيل ${K.esc(n)}">${K.ic('download')}</button></li>`; }).join(''); }
  const log = (ic, t, c = 0) => { FEED.unshift([ic, t, 'الآن', c]); FEED = FEED.slice(0, 6); feed(); };

  root.addEventListener('click', e => {
    const t = e.target, a = t.closest('[data-act]');
    if (a) { const k = a.dataset.act;
      if (k === 'add') { $('#cpAdd').hidden = false; $('#cpAdd [name=t]').focus(); }
      if (k === 'cancel') $('#cpAdd').hidden = true;
      if (k === 'edit') K.toast('فتح نموذج تعديل الحملة');
      if (k === 'report') { K.toast('يُنشأ تقرير الحملة (PDF)'); log('file', 'أُنشئ تقرير أداء الحملة', 0); }
      if (k === 'archive') { S.archived = !S.archived; a.setAttribute('aria-pressed', S.archived); a.querySelector('span').textContent = S.archived ? 'إلغاء الأرشفة' : 'أرشفة'; const st = $('#cpStatus'); st.className = 'dk-chip ' + (S.archived ? '' : 'ok'); st.textContent = S.archived ? 'مؤرشفة' : 'نشطة'; log('download', S.archived ? 'أُرشفت الحملة' : 'أُعيد تفعيل الحملة', 3); }
      return; }
    const s = t.closest('[data-st]'); if (s) { const i = +s.dataset.st, nx = ORDER[(ORDER.indexOf(TASKS[i][1]) + 1) % 4]; TASKS[i][1] = nx; tasks(); log('check', `«${TASKS[i][0]}» أصبحت ${ST[nx][0]}`, 4); root.querySelector(`[data-st="${i}"]`).focus(); return; }
    const d = t.closest('[data-dl]'); if (d) return K.toast('تنزيل ' + FILES[+d.dataset.dl][0]);
  });
  $('#cpAdd').addEventListener('submit', e => { e.preventDefault(); const f = new FormData(e.target), dd = new Date(f.get('d')); TASKS.push([f.get('t'), 'pend', f.get('a'), isNaN(dd) ? '—' : dd.toLocaleDateString('ar-SA-u-ca-gregory-nu-latn', { day: '2-digit', month: 'long' })]); tasks(); log('plus', `أُضيفت مهمة «${f.get('t')}»`, 0); e.target.reset(); e.target.hidden = true; });
  $('#cpFile').addEventListener('change', e => { [...e.target.files].forEach(f => FILES.unshift([f.name, f.size > 1e6 ? (f.size / 1e6).toFixed(1) : Math.round(f.size / 1e3) + ' ك.ب', 'أنت', 'الآن'])); files(); log('publish', `رُفع ${e.target.files.length} ملف`, 2); e.target.value = ''; });
  /* تزامن القراءة بين الرسمين */
  root.addEventListener('pointerover', e => { const g = e.target.closest('.cp-sm .hv'); root.querySelectorAll('.cp-sm .hv.on').forEach(x => x.classList.remove('on')); if (g) root.querySelectorAll(`.cp-sm .hv[data-x="${g.dataset.x}"]`).forEach(x => x.classList.add('on')); });
  K.seg(root, 'rng', v => { S.rng = +v; kpis(); perf(); });
  K.onResize(root, perf);
  kpis(); perf(); feed(); tasks(); files();
})();
