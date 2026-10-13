/* ==========================================================
   جيم — تفاصيل مشروع حملة (لوحة تفاعلية، المحرك: campaign، على dash-kit)
   مُعاد بناؤه كدالة تركيب قابلة لإعادة الاستخدام (window.JEEM_CAMPAIGN_MOUNT) بدل IIFE ذاتي
   التشغيل — يقبل data حقيقية أو من مكتبة العرض التجريبي (campaign-project-demo.js).
   أفعال الواجهة (إضافة مهمة، رفع ملف، أرشفة) كلها أفعال المستخدم الحالي نفسه، تُطبَّق على
   العرض فقط لمدة الجلسة (بلا طبقة حفظ دائم بعد) — لا تلفيق لأي فعل منسوب لشخص آخر.
   ========================================================== */
(function () {
  function mount(root, data, opts = {}) {
    if (!root || !data || !window.DK) return;
    const K = DK, $ = s => root.querySelector(s);
    const ranges = data.ranges && data.ranges.length ? data.ranges : [15];
    const S = { rng: ranges[0], archived: !!data.archived };
    const ST = { done: ['مكتملة', 'ok', 'check'], prog: ['قيد التنفيذ', 'warn', 'clock'], on: ['مستمرة', 'info', 'history'], pend: ['معلقة', '', 'pause'] };
    const ORDER = ['pend', 'prog', 'on', 'done'];
    let TASKS = (data.tasks || []).map(t => ({ ...t }));
    let FEED = (data.feed || []).map(f => ({ ...f }));
    let FILES = (data.files || []).map(f => ({ ...f }));
    const BUDGET = data.budget || {};

    if ($('.cp-name')) $('.cp-name').textContent = data.name || 'الحملة';
    if ($('#cpStatus')) { $('#cpStatus').textContent = S.archived ? 'مؤرشفة' : 'نشطة'; $('#cpStatus').className = 'dk-chip ' + (S.archived ? '' : 'ok'); }
    if ($('#cpDue')) $('#cpDue').textContent = data.dueDate ? new Date(data.dueDate).toLocaleDateString('ar-SA-u-ca-gregory-nu-latn', { day: '2-digit', month: '2-digit', year: 'numeric' }) : '—';
    if ($('#cpRoas')) $('#cpRoas').textContent = K.nf(data.roas ?? 0, 1) + 'x';
    const dateInput = $('#cpAdd [name=d]');
    if (dateInput) { const d = new Date(); d.setDate(d.getDate() + 7); dateInput.value = d.toISOString().slice(0, 10); }

    function kpis() {
      const b = BUDGET[S.rng] || { spent: 0, conversions: 0 };
      $('#cpKpis').innerHTML = [
        ['الميزانية الكلية', K.money(BUDGET.total ?? 0), ''],
        ['المنفق', K.money(b.spent), `<span class="dk-prog"><i style="width:${(BUDGET.total ? b.spent / BUDGET.total * 100 : 0).toFixed(1)}%"></i></span>`],
        ['التحويلات', K.num(b.conversions, '', 0), ''],
        ['معدل التحويل', K.pct(BUDGET.convRatePct ?? 0), ''],
        ['الأعضاء المكلفون', K.num((data.members || []).length, '', 0), `<span class="dk-avs">${(data.members || []).map(m => `<span class="dk-av" style="--h:${(m.name || '').charCodeAt(0) * 7 % 360}">${K.esc((m.name || '?')[0])}</span>`).join('')}</span>`],
        ['الأيام المتبقية', K.num(data.daysRemaining ?? 0, 'يومًا', 0), ''],
      ].map(([n, v, x], i) => `<div class="dk-kpi" style="--i:${i}"><small>${n}</small><b>${v}</b>${x}</div>`).join('');
      K.count(root);
    }
    function perf() {
      const P = (data.performance && data.performance[S.rng]) || { labels: [], spendActual: [], spendForecast: [], convActual: [], convForecast: [] };
      const W = K.wOf($('#cpSpend')), n = P.labels.length;
      const forecastStart = P.spendActual.length ? P.spendActual.length - 1 : 0;
      $('#cpSpend').innerHTML = n ? K.line({ W, H: 170, labels: P.labels, key: 'x', series: [{ name: 'الإنفاق', color: K.PAL[3], vals: P.spendActual, dots: n <= 15 }, { name: 'الإنفاق المتوقع', color: K.PAL[3], vals: P.spendForecast, dash: 1 }], fmt: v => K.nf(v) + ' ر.س', aria: 'الإنفاق اليومي', band: P.spendForecast.some(Boolean) ? [forecastStart, n - 1] : null }) : '<p class="dk-empty">لا بيانات أداء بعد.</p>';
      $('#cpConv').innerHTML = n ? K.line({ W, H: 170, labels: P.labels, key: 'x', series: [{ name: 'التحويلات', color: K.PAL[2], vals: P.convActual, dots: n <= 15 }, { name: 'التحويلات المتوقعة', color: K.PAL[2], vals: P.convForecast, dash: 1 }], fmt: v => K.nf(v, 1) + ' تحويل', aria: 'التحويلات اليومية', band: P.convForecast.some(Boolean) ? [forecastStart, n - 1] : null }) : '';
      $('#cpLeg').innerHTML = `<div class="dk-leg"><span><i style="background:${K.PAL[3]}"></i>الإنفاق</span><span><i style="background:${K.PAL[2]}"></i>التحويلات</span><span><i class="dash"></i>توقع</span><span><i style="background:color-mix(in srgb,#00A89F 15%,transparent)"></i>فترة التوقع</span></div>`;
    }
    function feed() { $('#cpFeed').innerHTML = FEED.slice(0, 6).map((f, i) => `<li style="--i:${i}"><span class="fi" style="--c:${K.PAL[f.color % K.PAL.length] || K.PAL[0]}">${K.ic(f.icon)}</span><span><b>${K.esc(f.text)}</b><small>${K.esc(f.when)}</small></span></li>`).join(''); }
    function tasks() {
      $('#cpTasks').innerHTML = `<table><thead><tr><th>المهمة</th><th>الحالة</th><th>المسؤول</th><th>الاستحقاق</th></tr></thead><tbody>${TASKS.map((t, i) => `<tr style="--i:${i}"><td><b>${K.esc(t.title)}</b></td><td><button type="button" class="dk-chip ${ST[t.status]?.[1] || ''} cp-st" data-st="${i}" aria-label="الحالة: ${ST[t.status]?.[0] || t.status} — اضغط للتغيير">${K.ic(ST[t.status]?.[2] || 'pause')}${ST[t.status]?.[0] || t.status}</button></td><td><span class="dk-av" style="--h:${(t.assignee || '').charCodeAt(0) * 7 % 360}">${K.esc((t.assignee || '?')[0])}</span> ${K.esc(t.assignee || '')}</td><td>${K.esc(t.due || '—')}</td></tr>`).join('')}</tbody></table>`;
    }
    function files() { $('#cpFiles').innerHTML = FILES.map((f, i) => { const ext = (f.name || '').split('.').pop(); return `<li style="--i:${i}"><span class="ft ${K.esc(ext)}">${K.esc(ext)}</span><span><b>${K.esc(f.name)}</b><small>${K.esc(f.sizeLabel || '')} · رفعه ${K.esc(f.by || '')} · ${K.esc(f.date || '')}</small></span><button type="button" data-dl="${i}" aria-label="تنزيل ${K.esc(f.name)}">${K.ic('download')}</button></li>`; }).join(''); }
    const log = (icon, text, color = 0) => { FEED.unshift({ icon, text, when: 'الآن', color }); FEED = FEED.slice(0, 6); feed(); };
    function all() { kpis(); perf(); feed(); tasks(); files(); }

    root.addEventListener('click', e => {
      const t = e.target, a = t.closest('[data-act]');
      if (a) {
        const k = a.dataset.act;
        if (k === 'add') { $('#cpAdd').hidden = false; $('#cpAdd [name=t]').focus(); }
        if (k === 'cancel') $('#cpAdd').hidden = true;
        if (k === 'edit') K.toast('فتح نموذج تعديل الحملة');
        if (k === 'report') { K.toast('يُنشأ تقرير الحملة (PDF)'); log('file', 'أُنشئ تقرير أداء الحملة', 0); }
        if (k === 'archive') {
          S.archived = !S.archived; a.setAttribute('aria-pressed', S.archived); a.querySelector('span').textContent = S.archived ? 'إلغاء الأرشفة' : 'أرشفة';
          const st = $('#cpStatus'); st.className = 'dk-chip ' + (S.archived ? '' : 'ok'); st.textContent = S.archived ? 'مؤرشفة' : 'نشطة';
          log('download', S.archived ? 'أُرشفت الحملة' : 'أُعيد تفعيل الحملة', 3);
        }
        return;
      }
      const s = t.closest('[data-st]');
      if (s) { const i = +s.dataset.st, nx = ORDER[(ORDER.indexOf(TASKS[i].status) + 1) % 4]; TASKS[i].status = nx; tasks(); log('check', `«${TASKS[i].title}» أصبحت ${ST[nx][0]}`, 4); root.querySelector(`[data-st="${i}"]`)?.focus(); return; }
      const d = t.closest('[data-dl]'); if (d) return K.toast('تنزيل ' + FILES[+d.dataset.dl].name);
    });
    $('#cpAdd').addEventListener('submit', e => {
      e.preventDefault(); const f = new FormData(e.target), dd = new Date(f.get('d'));
      TASKS.push({ title: f.get('t'), status: 'pend', assignee: f.get('a'), due: isNaN(dd) ? '—' : dd.toLocaleDateString('ar-SA-u-ca-gregory-nu-latn', { day: '2-digit', month: 'long' }) });
      tasks(); log('plus', `أُضيفت مهمة «${f.get('t')}»`, 0); e.target.reset(); e.target.hidden = true;
    });
    $('#cpFile').addEventListener('change', e => {
      [...e.target.files].forEach(f => FILES.unshift({ name: f.name, sizeLabel: f.size > 1e6 ? (f.size / 1e6).toFixed(1) + ' م.ب' : Math.round(f.size / 1e3) + ' ك.ب', by: 'أنت', date: 'الآن' }));
      files(); log('publish', `رُفع ${e.target.files.length} ملف`, 2); e.target.value = '';
    });
    root.addEventListener('pointerover', e => { const g = e.target.closest('.cp-sm .hv'); root.querySelectorAll('.cp-sm .hv.on').forEach(x => x.classList.remove('on')); if (g) root.querySelectorAll(`.cp-sm .hv[data-x="${g.dataset.x}"]`).forEach(x => x.classList.add('on')); });
    K.seg(root, 'rng', v => { S.rng = +v; kpis(); perf(); });
    K.onResize(root, perf);

    all();
    return { destroy() {} };
  }

  window.JEEM_CAMPAIGN_MOUNT = mount;
})();
