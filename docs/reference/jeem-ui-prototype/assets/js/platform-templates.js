/* ==========================================================
   جيم — مكتبة النماذج (مسؤول المنصّة)
   إدارة كل النماذج من مكان واحد:
     • عناصر المحادثة   (تبويب «النماذج» داخل المحادثة)
     • نماذج اللوحات    (معرض «لوحة جديدة» — 10 عناصر لكل نموذج)
     • مكتبة المؤشرات   (1,899 مؤشرًا — قالب عام + وصف مخصص اختياري)
     • القواعد العامة   (تُرسل مع أي نموذج)
   لكل نموذج «وصف الذكاء الاصطناعي»: النص المرسل لجيم عند اختياره.
   في التطبيق الفعلي:
     GET/POST/PUT/DELETE /platform/widget-templates
     GET/POST/PUT/DELETE /platform/dashboard-templates
     GET/PUT /platform/kpi-library/{id}   ·  GET/PUT /platform/ai-rules
   ========================================================== */
(function () {
  const CAT = window.JEEM_CATALOG, LIB = window.JEEM_KPI_LIBRARY;
  const root = document.getElementById('tplLib');
  if (!CAT || !root) return;

  const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
  const esc = s => String(s ?? '').replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
  const fmt = n => n.toLocaleString('en-US');
  const $ = (s, r = document) => r.querySelector(s), $$ = (s, r = document) => [...r.querySelectorAll(s)];
  const STATUS = { published: ['منشور', 'ok'], draft: ['مسودة', 'warn'], off: ['موقوف', 'off'] };
  const PLANS = [['basic', 'الأساسية'], ['business', 'الأعمال'], ['enterprise', 'المؤسسات']];
  const TINTS = [['', 'أزرق جيم'], ['var(--brand-purple)', 'بنفسجي'], ['var(--brand-teal)', 'فيروزي'], ['#0E6399', 'أزرق داكن'], ['#0E8FB8', 'سماوي'], ['var(--brand-gray)', 'رمادي']];
  const KINDS = {
    w: { tab: 'عناصر المحادثة', one: 'عنصر', add: 'عنصر جديد', hash: 'widgets', icon: 'chat', where: 'يظهر في تبويب «النماذج» داخل المحادثة، ويُضاف للوحة الحالية بضغطة.' },
    d: { tab: 'نماذج اللوحات', one: 'نموذج لوحة', add: 'نموذج لوحة جديد', hash: 'dashboards', icon: 'dashboard', where: 'يظهر في معرض «لوحة جديدة»، وينشئ لوحة كاملة من 10 عناصر.' },
    k: { tab: 'مكتبة المؤشرات', one: 'مؤشر', add: 'مؤشر جديد', hash: 'kpis', icon: 'target', where: 'يظهر في «النماذج ← مكتبة المؤشرات» داخل المحادثة.' },
    r: { tab: 'القواعد العامة', hash: 'rules', icon: 'shield' }
  };
  const statusPill = s => `<span class="status ${STATUS[s || 'published'][1]}">${STATUS[s || 'published'][0]}</span>`;
  const catName = (kind, c) => kind === 'w' ? (CAT.WIDGET_CATS.find(x => x[0] === c) || [, c])[1] : (CAT.DASH_CATS.find(x => x[0] === c) || [, c])[1];
  const toast = (msg, undo) => {
    let t = $('.toast');
    if (!t) { t = document.createElement('div'); t.className = 'toast'; t.setAttribute('role', 'status'); document.body.appendChild(t); }
    t.innerHTML = `${ic('check')}<span>${msg}</span>${undo ? '<button type="button" class="toast-undo">تراجع</button>' : ''}`;
    if (undo) t.querySelector('.toast-undo').addEventListener('click', () => { clearTimeout(t._h); t.classList.remove('is-on'); undo(); });
    t.classList.remove('is-on'); void t.offsetWidth; t.classList.add('is-on');
    clearTimeout(t._h); t._h = setTimeout(() => t.classList.remove('is-on'), undo ? 6000 : 2800);
  };

  /* تعويض المتغيرات مع تمييزها بصريًا في المعاينة */
  const fillHtml = (text, extra = {}) => {
    const ctx = Object.fromEntries(CAT.VARS.map(v => [v.key, v.sample])); Object.assign(ctx, extra);
    return esc(text).replace(/\{\{\s*([^}]+?)\s*\}\}/g, (m, k) => ctx[k] != null && ctx[k] !== ''
      ? `<b class="pv-var" title="{{${esc(k)}}}">${esc(ctx[k])}</b>` : `<mark class="pv-bad" title="متغير غير معروف">${m}</mark>`);
  };

  const codeVars = t => esc(t).replace(/\{\{\s*([^}]+?)\s*\}\}/g, '<code dir="ltr">{{$1}}</code>');

  /* ---------- الحالة ---------- */
  const st = { w: { q: '', s: '', c: '' }, d: { q: '', s: '', c: '' }, k: { q: '', s: '', c: '', m: '', p: 0 } };
  let kind = Object.keys(KINDS).find(k => '#' + KINDS[k].hash === location.hash) || 'w';
  const newBtn = $('[data-tpl-new]');

  /* ---------- الأرقام أعلى الصفحة ---------- */
  const kpiCount = () => LIB ? LIB.rows.length : 0;
  const stats = () => {
    const W = CAT.widgets(true), D = CAT.dashboards(true);
    const drafts = W.concat(D).filter(x => x.status !== 'published').length;
    let custom = 0, off = 0; for (let i = 0; i < kpiCount(); i++) { if (CAT.kpiPrompt(i)) custom++; if (CAT.kpiStatus(i) !== 'published') off++; }
    $('#tplStats').innerHTML = `
      <div style="background:var(--surface)"><span class="stat-label">${ic('chat')}عناصر المحادثة</span><span class="stat-value num">${W.length}</span><span class="stat-foot"><span class="num">${W.filter(x => x.status === 'published').length}</span> منشور</span></div>
      <div><span class="stat-label">${ic('dashboard')}نماذج اللوحات</span><span class="stat-value num">${D.length}</span><span class="stat-foot">10 عناصر لكل نموذج</span></div>
      <div><span class="stat-label">${ic('target')}مكتبة المؤشرات</span><span class="stat-value num">${fmt(kpiCount())}</span><span class="stat-foot"><span class="num">${fmt(kpiCount() - off)}</span> ظاهر · <span class="num">${LIB ? LIB.cats.filter((c, i) => !CAT.catOn(i)).length : 0}</span> تصنيف موقوف · <span class="num">${custom}</span> وصف مخصص</span></div>
      <div><span class="stat-label">${ic('edit')}مسودات وموقوفة</span><span class="stat-value num">${drafts}</span><span class="stat-foot">لا تظهر للمستخدمين</span></div>`;
  };

  /* ---------- التبويبات ---------- */
  const tabs = () => {
    const n = { w: CAT.widgets(true).length, d: CAT.dashboards(true).length, k: kpiCount() };
    $('#tplTabs').innerHTML = Object.entries(KINDS).map(([k, v]) => `<button type="button" role="tab" data-kind="${k}" aria-selected="${k === kind}">${ic(v.icon)}${v.tab}${n[k] != null ? ` <span class="tab-n num">${fmt(n[k])}</span>` : ''}</button>`).join('');
    $$('#tplTabs [data-kind]').forEach(b => b.addEventListener('click', () => { kind = b.dataset.kind; history.replaceState(null, '', '#' + KINDS[kind].hash); render(); }));
    newBtn.hidden = kind === 'r';
    $('span', newBtn).textContent = KINDS[kind].add || '';
  };

  const segmented = (cur, opts, key) => `<div class="segmented" role="group" aria-label="الحالة">${opts.map(([v, t]) => `<button type="button" data-${key}="${v}" aria-pressed="${cur === v}">${t}</button>`).join('')}</div>`;
  const STATUS_OPTS = [['', 'الكل'], ['published', 'منشور'], ['draft', 'مسودة'], ['off', 'موقوف']];

  /* ---------- قائمة العناصر / اللوحات ---------- */
  const listPane = () => {
    const s = st[kind], all = kind === 'w' ? CAT.widgets(true) : CAT.dashboards(true);
    const cats = kind === 'w' ? CAT.WIDGET_CATS : CAT.DASH_CATS.filter(c => c[0] !== 'all');
    const rows = all.filter(x => (!s.s || x.status === s.s) && (!s.c || x.cat === s.c) && (x.name + x.desc + x.prompt).includes(s.q));
    const thumbOf = x => kind === 'w' ? `<span class="lib-thumb">${CAT.THUMB[x.render] || CAT.THUMB.kpi}</span>` : `<span class="lib-thumb is-dash">${CAT.thumb(x)}</span>`;
    $('#tplPane').innerHTML = `
      <p class="lib-where">${ic('info')}${KINDS[kind].where}</p>
      <div class="toolbar lib-toolbar">
        <label class="search">${ic('search')}<span class="sr-only">بحث</span><input type="search" value="${esc(s.q)}" placeholder="ابحث بالاسم أو داخل وصف الذكاء الاصطناعي"></label>
        ${segmented(s.s, STATUS_OPTS, 'st')}
        <label class="sr-only" for="libCat">التصنيف</label>
        <select id="libCat" class="select"><option value="">كل التصنيفات</option>${cats.map(([k, n]) => `<option value="${k}"${s.c === k ? ' selected' : ''}>${n}</option>`).join('')}</select>
      </div>
      <div class="dtable-wrap">
        <table class="dtable lib-table">
          <thead><tr><th>النموذج</th><th>التصنيف</th><th>وصف الذكاء الاصطناعي</th><th>الحالة</th><th>الإصدار</th><th><span class="sr-only">إجراءات</span></th></tr></thead>
          <tbody>${rows.map(x => `
            <tr data-id="${x.id}">
              <td><div class="lib-name">${thumbOf(x)}<div><strong>${esc(x.name)}${x.custom ? ' <span class="badge badge-blue">مضاف</span>' : x.patched ? ' <span class="badge">معدّل</span>' : ''}</strong><span>${esc(x.desc)}</span></div></div></td>
              <td><span class="badge">${catName(kind, x.cat)}</span></td>
              <td><p class="lib-prompt">${codeVars(x.prompt)}</p></td>
              <td>${statusPill(x.status)}</td>
              <td class="muted"><span class="num">v${x.version || 1}</span><span class="lib-date num">${x.updated || ''}</span></td>
              <td><div class="row-actions">
                <button class="btn btn-sm" type="button" data-act="edit">${ic('edit')}تعديل</button>
                <button class="btn btn-ghost btn-sm btn-icon" type="button" data-act="dup" aria-label="نسخ كنموذج جديد" title="نسخ كنموذج جديد">${ic('layers')}</button>
                <button class="btn btn-ghost btn-sm btn-icon" type="button" data-act="toggle" aria-label="${x.status === 'published' ? 'إيقاف' : 'نشر'}" title="${x.status === 'published' ? 'إيقاف' : 'نشر'}">${ic(x.status === 'published' ? 'pause' : 'play')}</button>
              </div></td>
            </tr>`).join('') || `<tr><td colspan="6"><div class="lib-empty">لا توجد نماذج مطابقة.</div></td></tr>`}</tbody>
        </table>
      </div>
      <div class="table-foot"><span>عرض <span class="num">${rows.length}</span> من <span class="num">${all.length}</span></span></div>`;
    bindToolbar(listPane);
    $$('#tplPane tr[data-id]').forEach(tr => {
      const get = () => CAT.get(kind, tr.dataset.id);
      $('[data-act="edit"]', tr).addEventListener('click', () => openEditor(kind, get()));
      $('[data-act="dup"]', tr).addEventListener('click', () => { const c = JSON.parse(JSON.stringify(get())); delete c.id; c.name += ' (نسخة)'; c.status = 'draft'; openEditor(kind, c, true); });
      $('[data-act="toggle"]', tr).addEventListener('click', () => {
        const x = get(); x.status = x.status === 'published' ? 'off' : 'published'; CAT.save(kind, x);
        toast(x.status === 'published' ? `تم نشر «${x.name}»` : `تم إيقاف «${x.name}» ولن يظهر للمستخدمين`); refresh();
      });
    });
  };

  const bindToolbar = again => {
    const s = st[kind];
    const inp = $('#tplPane .search input');
    inp.addEventListener('input', e => { s.q = e.target.value.trim(); s.p = 0; clearTimeout(inp._t); inp._t = setTimeout(() => { again(); const n = $('#tplPane .search input'); n.focus(); n.setSelectionRange(n.value.length, n.value.length); }, 180); });
    $$('#tplPane [data-st]').forEach(b => b.addEventListener('click', () => { s.s = b.dataset.st; s.p = 0; again(); }));
    $('#libCat')?.addEventListener('change', e => { s.c = e.target.value; s.p = 0; again(); });
    $('#libMt')?.addEventListener('change', e => { s.m = e.target.value; s.p = 0; again(); });
  };

  /* ---------- مكتبة المؤشرات ---------- */
  const PAGE = 25;
  const norm = x => x.toLowerCase().replace(/[أإآ]/g, 'ا').replace(/ة/g, 'ه').replace(/ى/g, 'ي');
  const GROUPS = [[0, 'الأقسام الوظيفية'], [1, 'القطاعات']];
  /* إحصاءات كل تصنيف: العدد، الظاهر فعليًا، الموقوف فرديًا */
  const catStats = () => {
    const m = LIB.cats.map(() => ({ total: 0, own: 0, custom: 0 }));
    LIB.rows.forEach((r, i) => { const c = m[r[0]]; c.total++; if (CAT.kpiOwnStatus(i) === 'published') c.own++; if (CAT.kpiPrompt(i)) c.custom++; });
    return m;
  };
  /* إيقاف/إظهار تصنيف أو مجموعة تصنيفات، مع «تراجع» */
  const setCats = (ids, on) => {
    const prev = ids.map(i => [i, CAT.catOn(i)]);
    const changed = prev.filter(([, was]) => was !== on);
    if (!changed.length) { toast(on ? 'كل التصنيفات المحددة ظاهرة بالفعل' : 'كل التصنيفات المحددة موقوفة بالفعل'); return; }
    CAT.setCats(changed.map(p => p[0]), on);
    const n = changed.reduce((a, [i]) => a + LIB.rows.filter(r => r[0] === i).length, 0);
    const what = changed.length === 1 ? `تصنيف «${LIB.cats[changed[0][0]][0]}»` : `${changed.length} ${changed.length > 10 ? "تصنيفًا" : "تصنيفات"}`;
    toast(on ? `تم إظهار ${what} · ${fmt(n)} مؤشرًا` : `تم إيقاف ${what} · ${fmt(n)} مؤشرًا لن يظهر للمستخدمين`,
      () => { changed.forEach(([i, was]) => CAT.setCats(i, was)); refresh(); toast('تم التراجع'); });
    refresh();
  };

  const kpiPane = () => {
    const s = st.k; s.v = s.v || 'cats';
    const head = `
      <p class="lib-where">${ic('info')}${KINDS.k.where} المصدر: مكتبة Spider Strategies، والترجمة ونوع القياس من جيم.</p>
      <div class="kpi-tpl-card">
        <div class="kt-head"><div><b>${ic('layers')}القالب العام لوصف المؤشرات</b><span>يُرسل لجيم مع أي مؤشر ليس له وصف مخصص، بعد تعويض اسم المؤشر وتصنيفه ونوع قياسه.</span></div>
          <button class="btn btn-sm" type="button" data-kt>${ic('edit')}تعديل القالب</button></div>
        <pre class="kt-text">${esc(CAT.kpiTemplate).replace(/\{\{\s*([^}]+?)\s*\}\}/g, '<code dir="ltr">{{$1}}</code>')}</pre>
      </div>
      <div class="kv-switch">${segmented(s.v, [['cats', 'حسب التصنيف'], ['list', 'كل المؤشرات']], 'kv')}</div>`;
    $('#tplPane').innerHTML = head + (s.v === 'cats' ? catsView() : listView());
    $('[data-kt]').addEventListener('click', () => openEditor('kt'));
    $$('#tplPane [data-kv]').forEach(b => b.addEventListener('click', () => { s.v = b.dataset.kv; kpiPane(); }));
    (s.v === 'cats' ? bindCats : bindList)();
  };

  /* -- عرض التصنيفات: تشغيل/إيقاف تصنيف كامل -- */
  const catsView = () => {
    const s = st.k, m = catStats(), qs = norm(s.cq || '');
    const cs = s.cs || '';
    const groups = GROUPS.map(([g, title]) => {
      const ids = LIB.cats.map((c, i) => i).filter(i => LIB.cats[i][2] === g);
      const shown = ids.filter(i => (!qs || norm(LIB.cats[i][0] + ' ' + LIB.cats[i][1]).includes(qs)) && (!cs || (cs === 'on') === CAT.catOn(i)));
      const onN = ids.filter(i => CAT.catOn(i)).length;
      if (!shown.length) return '';
      return `
        <section class="cat-group" data-group="${g}">
          <div class="cg-head">
            <div><h3>${title}</h3><span><span class="num">${onN}</span> ظاهر من <span class="num">${ids.length}</span> · <span class="num">${fmt(ids.reduce((a, i) => a + m[i].total, 0))}</span> مؤشرًا</span></div>
            <div class="cg-actions">
              <button class="btn btn-sm" type="button" data-grp-on="${g}"${onN === ids.length ? ' disabled' : ''}>${ic('play')}إظهار الكل</button>
              <button class="btn btn-sm btn-ghost danger-txt" type="button" data-grp-off="${g}"${onN === 0 ? ' disabled' : ''}>${ic('pause')}إيقاف الكل</button>
            </div>
          </div>
          <div class="cat-grid">${shown.map(i => {
            const c = LIB.cats[i], on = CAT.catOn(i), x = m[i], vis = on ? x.own : 0;
            return `
            <article class="cat-card${on ? '' : ' is-off'}" data-cat="${i}">
              <div class="cc-top">
                <div class="cc-name"><b>${c[0]}</b><small dir="ltr">${c[1]}</small></div>
                <label class="switch" title="${on ? 'إيقاف التصنيف' : 'إظهار التصنيف'}"><input type="checkbox" data-cat-toggle="${i}"${on ? ' checked' : ''} aria-label="إظهار تصنيف ${c[0]} للمستخدمين"><span></span></label>
              </div>
              <div class="cc-bar" role="img" aria-label="${vis} ظاهر من ${x.total}"><i style="width:${x.total ? (vis / x.total * 100).toFixed(1) : 0}%"></i></div>
              <div class="cc-meta">
                <span><b class="num">${fmt(vis)}</b> ظاهر من <span class="num">${fmt(x.total)}</span></span>
                ${on && x.total - x.own ? `<span class="muted"><span class="num">${x.total - x.own}</span> موقوف فرديًا</span>` : ''}
                ${x.custom ? `<span class="badge badge-blue"><span class="num">${x.custom}</span> وصف مخصص</span>` : ''}
              </div>
              <div class="cc-foot">${on ? statusPill('published') : `<span class="status off">موقوف — لا يظهر أي مؤشر منه</span>`}
                <button class="btn btn-ghost btn-sm" type="button" data-cat-open="${i}">عرض المؤشرات ${ic('chev-left')}</button></div>
            </article>`; }).join('')}</div>
        </section>`;
    }).join('');
    return `
      <div class="toolbar lib-toolbar">
        <label class="search">${ic('search')}<span class="sr-only">بحث في التصنيفات</span><input type="search" data-cq value="${esc(s.cq || '')}" placeholder="ابحث باسم التصنيف"></label>
        ${segmented(cs, [['', 'كل التصنيفات'], ['on', 'ظاهرة'], ['off', 'موقوفة']], 'cs')}
      </div>
      <p class="cat-note">${ic('info')}إيقاف التصنيف يخفي كل مؤشراته من المحادثة فورًا، ولا يغيّر حالة المؤشرات نفسها: عند إظهاره مرة أخرى يرجع كل مؤشر لحالته السابقة.</p>
      ${groups || '<div class="lib-empty">لا توجد تصنيفات مطابقة.</div>'}`;
  };
  const bindCats = () => {
    const s = st.k, inp = $('#tplPane [data-cq]');
    inp.addEventListener('input', e => { s.cq = e.target.value.trim(); clearTimeout(inp._t); inp._t = setTimeout(() => { kpiPane(); const n = $('#tplPane [data-cq]'); n.focus(); n.setSelectionRange(n.value.length, n.value.length); }, 150); });
    $$('#tplPane [data-cs]').forEach(b => b.addEventListener('click', () => { s.cs = b.dataset.cs; kpiPane(); }));
    $$('#tplPane [data-cat-toggle]').forEach(c => c.addEventListener('change', () => setCats([+c.dataset.catToggle], c.checked)));
    const grp = (g, on) => setCats(LIB.cats.map((c, i) => i).filter(i => LIB.cats[i][2] === +g), on);
    $$('#tplPane [data-grp-on]').forEach(b => b.addEventListener('click', () => grp(b.dataset.grpOn, true)));
    $$('#tplPane [data-grp-off]').forEach(b => b.addEventListener('click', () => grp(b.dataset.grpOff, false)));
    $$('#tplPane [data-cat-open]').forEach(b => b.addEventListener('click', () => { Object.assign(s, { v: 'list', c: b.dataset.catOpen, q: '', s: '', m: '', p: 0 }); kpiPane(); }));
  };

  /* -- عرض كل المؤشرات -- */
  let LIST_HITS = 0;
  const listView = () => {
    const s = st.k, qs = norm(s.q).split(/\s+/).filter(Boolean);
    const hits = [];
    LIB.rows.forEach((r, i) => {
      if (s.c !== '' && r[0] !== +s.c) return;
      if (s.m !== '' && r[4] !== +s.m) return;
      const status = CAT.kpiStatus(i);
      if (s.s === 'custom' ? !CAT.kpiPrompt(i) : s.s && status !== s.s) return;
      if (qs.length) { const t = norm(r[2] + ' ' + r[3]); if (!qs.every(w => t.includes(w))) return; }
      hits.push(i);
    });
    const pages = Math.max(1, Math.ceil(hits.length / PAGE)); s.p = Math.min(s.p, pages - 1); LIST_HITS = pages;
    const slice = hits.slice(s.p * PAGE, s.p * PAGE + PAGE);
    const opt = (c, i) => `<option value="${i}"${s.c === String(i) ? ' selected' : ''}>${c[0]}${CAT.catOn(i) ? '' : ' (موقوف)'}</option>`;
    const sel = s.c !== '' ? +s.c : null, selOn = sel != null && CAT.catOn(sel);
    return `
      <div class="toolbar lib-toolbar">
        <label class="search">${ic('search')}<span class="sr-only">بحث في المؤشرات</span><input type="search" value="${esc(s.q)}" placeholder="ابحث بالعربي أو English"></label>
        ${segmented(s.s, [['', 'الكل'], ['published', 'منشور'], ['off', 'موقوف'], ['custom', 'وصف مخصص']], 'st')}
        <label class="sr-only" for="libCat">التصنيف</label>
        <select id="libCat" class="select"><option value="">كل التصنيفات</option>${GROUPS.map(([g, t]) => `<optgroup label="${t}">${LIB.cats.map((c, i) => c[2] === g ? opt(c, i) : '').join('')}</optgroup>`).join('')}</select>
        <label class="sr-only" for="libMt">نوع القياس</label>
        <select id="libMt" class="select"><option value="">كل أنواع القياس</option>${LIB.mtypes.map((m, i) => `<option value="${i}"${s.m === String(i) ? ' selected' : ''}>${m}</option>`).join('')}</select>
      </div>
      ${sel != null ? `
      <div class="cat-banner${selOn ? '' : ' is-off'}">
        <div><b>تصنيف «${LIB.cats[sel][0]}»</b><span>${selOn ? 'ظاهر للمستخدمين. يمكنك إيقاف مؤشرات بعينها، أو إيقاف التصنيف كله.' : 'موقوف: لا يظهر أي مؤشر منه للمستخدمين، مهما كانت حالة المؤشر.'}</span></div>
        <label class="switch"><input type="checkbox" data-cat-toggle="${sel}"${selOn ? ' checked' : ''} aria-label="إظهار تصنيف ${LIB.cats[sel][0]} للمستخدمين"><span></span></label>
      </div>` : ''}
      <div class="dtable-wrap">
        <table class="dtable lib-table">
          <thead><tr><th>المؤشر</th><th>التصنيف</th><th>نوع القياس</th><th>الوصف المرسل</th><th>الحالة</th><th><span class="sr-only">إجراءات</span></th></tr></thead>
          <tbody>${slice.map(i => {
            const r = LIB.rows[i], catOff = !CAT.catOn(r[0]), own = CAT.kpiOwnStatus(i), cp = CAT.kpiPrompt(i);
            return `
            <tr data-k="${i}"${catOff ? ' class="is-cat-off"' : ''}>
              <td><div class="lib-kpi"><strong>${esc(r[2])}${CAT.isCustomKpi(i) ? ' <span class="badge badge-blue">مضاف</span>' : ''}</strong><span dir="ltr">${esc(r[3])}</span></div></td>
              <td class="muted">${LIB.cats[r[0]][0]}</td>
              <td><span class="badge">${LIB.mtypes[r[4]]}</span></td>
              <td>${cp ? `<span class="badge badge-blue">مخصص</span><p class="lib-prompt one">${codeVars(cp)}</p>` : '<span class="muted">القالب العام</span>'}</td>
              <td>${catOff ? '<span class="status off">موقوف مع التصنيف</span>' : statusPill(own)}</td>
              <td><div class="row-actions">
                <button class="btn btn-sm" type="button" data-act="edit">${ic('edit')}تعديل</button>
                <button class="btn btn-ghost btn-sm btn-icon" type="button" data-act="toggle"${catOff ? ' disabled title="التصنيف كله موقوف — أظهِر التصنيف أولًا"' : ` title="${own === 'published' ? 'إيقاف' : 'نشر'}"`} aria-label="${own === 'published' ? 'إيقاف' : 'نشر'} المؤشر">${ic(own === 'published' ? 'pause' : 'play')}</button>
              </div></td>
            </tr>`; }).join('') || `<tr><td colspan="6"><div class="lib-empty">لا توجد مؤشرات مطابقة.</div></td></tr>`}</tbody>
        </table>
      </div>
      <div class="table-foot"><span>عرض <span class="num">${hits.length ? fmt(s.p * PAGE + 1) + '–' + fmt(s.p * PAGE + slice.length) : 0}</span> من <span class="num">${fmt(hits.length)}</span></span>
        <div class="pager"><button type="button" data-pg="-1" ${s.p ? '' : 'disabled'} aria-label="السابق">السابق</button><button type="button" aria-current="true"><span class="num">${s.p + 1} / ${pages}</span></button><button type="button" data-pg="1" ${s.p < pages - 1 ? '' : 'disabled'} aria-label="التالي">التالي</button></div></div>`;
  };
  const bindList = () => {
    const s = st.k;
    bindToolbar(kpiPane);
    $('#tplPane [data-cat-toggle]')?.addEventListener('change', e => setCats([+e.target.dataset.catToggle], e.target.checked));
    $$('#tplPane [data-pg]').forEach(b => b.addEventListener('click', () => { s.p += +b.dataset.pg; kpiPane(); $('#tplPane').scrollIntoView({ block: 'start', behavior: 'smooth' }); }));
    $$('#tplPane tr[data-k]').forEach(tr => {
      const i = +tr.dataset.k;
      $('[data-act="edit"]', tr).addEventListener('click', () => openEditor('k', i));
      $('[data-act="toggle"]', tr).addEventListener('click', () => {
        const r = LIB.rows[i], on = CAT.kpiOwnStatus(i) === 'published';
        CAT.saveKpi(i, { cat: r[0], ar: r[2], en: r[3], mt: r[4], status: on ? 'off' : 'published' });
        toast(on ? `تم إيقاف «${r[2]}»` : `تم نشر «${r[2]}»`); refresh();
      });
    });
  };

  /* ---------- القواعد العامة ---------- */
  const rulesPane = () => {
    $('#tplPane').innerHTML = `
      <p class="lib-where">${ic('info')}تُرسل هذه القواعد لجيم قبل وصف أي نموذج (عنصر أو لوحة أو مؤشر)، في كل المنظمات.</p>
      <div class="rules-grid">
        <div class="field">
          <label for="rulesTa">القواعد العامة المرسلة مع كل نموذج</label>
          ${varChips('all')}
          <textarea id="rulesTa" class="prompt-box" rows="12" dir="rtl">${esc(CAT.rules)}</textarea>
          <div class="prompt-meta"><span class="pm-warn" hidden></span><span><span class="num pm-count">0</span> حرف</span></div>
          <div class="rules-actions"><button class="btn btn-ghost" type="button" data-rules-reset>${ic('history')}استعادة الافتراضي</button><button class="btn btn-primary" type="button" data-rules-save>${ic('check')}حفظ القواعد</button></div>
        </div>
        <div class="field">
          <label>المتغيرات المتاحة</label>
          <div class="dtable-wrap"><table class="dtable vars-table"><thead><tr><th>المتغير</th><th>المعنى</th><th>يُستخدم في</th></tr></thead><tbody>
            ${CAT.VARS.map(v => `<tr><td><code dir="ltr">{{${v.key}}}</code></td><td>${v.label}${v.sample ? `<span class="muted vs">مثال: ${esc(v.sample)}</span>` : ''}</td><td><span class="badge">${{ all: 'الكل', dashboard: 'اللوحات', kpi: 'المؤشرات' }[v.scope]}</span></td></tr>`).join('')}
          </tbody></table></div>
        </div>
      </div>`;
    const ta = $('#rulesTa');
    bindPrompt(ta, $('#tplPane'));
    $('[data-rules-save]').addEventListener('click', () => { CAT.rules = ta.value.trim(); toast('تم حفظ القواعد العامة — تُطبّق على كل الطلبات الجديدة'); });
    $('[data-rules-reset]').addEventListener('click', () => { ta.value = CAT.defaults.rules; ta.dispatchEvent(new Event('input')); });
  };

  /* ---------- أدوات حقل الوصف ---------- */
  function varChips(scope) {
    const vs = CAT.VARS.filter(v => v.scope === 'all' || v.scope === scope);
    return `<div class="var-chips" role="group" aria-label="إدراج متغير">${vs.map(v => `<button type="button" class="var-chip" data-var="${v.key}" title="${v.label}"><code dir="ltr">{{${v.key}}}</code><span>${v.label}</span></button>`).join('')}</div>`;
  }
  function bindPrompt(ta, scope, onChange) {
    const meta = () => {
      const bad = CAT.unknownVars(ta.value), w = $('.pm-warn', scope);
      $('.pm-count', scope).textContent = fmt(ta.value.length);
      w.hidden = !bad.length; w.innerHTML = bad.length ? `${ic('info')}متغير غير معروف: <code dir="ltr">${bad.map(b => `{{${esc(b)}}}`).join('، ')}</code>` : '';
      onChange && onChange();
    };
    ta.addEventListener('input', meta);
    $$('[data-var]', scope).forEach(b => b.addEventListener('click', () => {
      const t = `{{${b.dataset.var}}}`, a = ta.selectionStart ?? ta.value.length, z = ta.selectionEnd ?? a;
      ta.setRangeText(t, a, z, 'end'); ta.focus(); meta();
    }));
    meta();
  }

  /* ==========================================================
     المحرر (نافذة واحدة لكل الأنواع)
     ========================================================== */
  const dlg = $('#tplEditor');
  let ED = null; // { kind, item, isNew, idx }

  const field = (id, label, input, hint = '') => `<div class="field"><label for="${id}">${label}</label>${input}${hint ? `<span class="hint">${hint}</span>` : ''}</div>`;
  const statusField = s => `<div class="field"><span class="flabel" id="ed-st-l">الحالة</span><div class="segmented" role="group" aria-labelledby="ed-st-l">${Object.entries(STATUS).map(([k, [t]]) => `<button type="button" data-status="${k}" aria-pressed="${(s || 'published') === k}">${t}</button>`).join('')}</div><span class="hint">المسودة والموقوف لا يظهران للمستخدمين.</span></div>`;
  const plansField = p => `<div class="field"><span class="flabel">متاح لخطط</span><div class="plan-checks">${PLANS.map(([k, t]) => `<label class="check"><input type="checkbox" name="plan" value="${k}"${(p || []).includes(k) ? ' checked' : ''}>${t}</label>`).join('')}</div></div>`;
  const promptField = (val, scope, hint) => `
    <div class="field ed-prompt">
      <label for="ed-prompt">وصف الذكاء الاصطناعي <span class="req" aria-hidden="true">*</span></label>
      <span class="hint">${hint}</span>
      ${varChips(scope)}
      <textarea id="ed-prompt" class="prompt-box" rows="9" dir="rtl" required>${esc(val)}</textarea>
      <div class="prompt-meta"><span class="pm-warn" hidden></span><span><span class="num pm-count">0</span> حرف</span></div>
    </div>`;

  /* -- نموذج اللوحة: محرر العناصر العشرة -- */
  const DEF_CHARTS = ['line', 'donut', 'hbars', 'cbars', 'table', 'funnel'];
  const blankWidgets = () => Array.from({ length: 4 }, (_, i) => ({ type: 'kpi', title: '' })).concat(DEF_CHARTS.map(t => ({ type: t, title: '' })));
  const typeLabel = t => t === 'kpi' ? 'مؤشر رقمي' : (CAT.CHART_TYPES.find(c => c[0] === t) || [, t])[1];
  const widgetRows = ws => ws.map((w, i) => `
    <li class="dw-row" data-i="${i}">
      <span class="dw-n num">${i + 1}</span>
      <span class="dw-ic">${CAT.GLYPH[w.type]}</span>
      ${i < 4 ? `<span class="dw-type is-fixed">مؤشر رقمي</span>` : `<label class="sr-only" for="dw-t${i}">نوع العنصر ${i + 1}</label><select id="dw-t${i}" class="select dw-type" data-k="type">${CAT.CHART_TYPES.map(([k, n]) => `<option value="${k}"${k === w.type ? ' selected' : ''}>${n}</option>`).join('')}</select>`}
      <label class="sr-only" for="dw-n${i}">عنوان العنصر ${i + 1}</label><input id="dw-n${i}" data-k="title" value="${esc(w.title)}" placeholder="${i < 4 ? 'مثال: إجمالي المبيعات' : 'مثال: المبيعات الشهرية'}" required>
      <label class="sr-only" for="dw-p${i}">تعليمات إضافية للعنصر ${i + 1}</label><input id="dw-p${i}" data-k="prompt" class="dw-prompt" value="${esc(w.prompt || '')}" placeholder="تعليمات إضافية لجيم (اختياري)">
    </li>`).join('');

  function openEditor(k, item, isNew) {
    ED = { kind: k, isNew: !!isNew || item == null, orig: null };
    let title, sub, form, side = true;
    if (k === 'w') {
      const x = ED.item = item ? JSON.parse(JSON.stringify(item)) : { render: 'kpi', cat: 'kpi', span: 4, name: '', desc: '', src: '', prompt: '', status: 'draft', plans: PLANS.map(p => p[0]) };
      title = ED.isNew ? 'عنصر جديد' : `تعديل: ${x.name}`; sub = KINDS.w.where;
      form = `
        <div class="ed-row">${field('ed-name', 'الاسم الظاهر للمستخدم', `<input id="ed-name" value="${esc(x.name)}" required placeholder="مثال: المبيعات حسب المنطقة">`)}
        ${field('ed-cat', 'التصنيف', `<select id="ed-cat" class="select">${CAT.WIDGET_CATS.map(([c, n]) => `<option value="${c}"${c === x.cat ? ' selected' : ''}>${n}</option>`).join('')}</select>`)}</div>
        ${field('ed-desc', 'الوصف المختصر (يظهر تحت الاسم)', `<input id="ed-desc" value="${esc(x.desc)}" placeholder="جملة واحدة توضح ما يعرضه العنصر">`)}
        <div class="field"><span class="flabel" id="ed-r-l">شكل العرض</span><div class="type-pick" role="radiogroup" aria-labelledby="ed-r-l">${CAT.RENDER_TYPES.map(([r, n]) => `<label class="tp${r === x.render ? ' is-on' : ''}"><input type="radio" name="render" value="${r}"${r === x.render ? ' checked' : ''}><span class="tp-th">${CAT.THUMB[r]}</span><span>${n}</span></label>`).join('')}</div></div>
        <div class="ed-row">${field('ed-span', 'العرض داخل اللوحة', `<select id="ed-span" class="select">${[[3, 'ربع'], [4, 'ثلث'], [5, '5 من 12'], [6, 'نصف'], [7, '7 من 12'], [8, 'ثلثان'], [12, 'كامل العرض']].map(([v, n]) => `<option value="${v}"${+x.span === v ? ' selected' : ''}>${n}</option>`).join('')}</select>`)}
        ${field('ed-src', 'المصدر المقترح', `<input id="ed-src" value="${esc(x.src)}" placeholder="مثال: ERP · sales_orders" dir="ltr" style="text-align:right">`, 'يظهر للمستخدم كتلميح، والمصدر الفعلي يختاره جيم من مصادر المشروع.')}</div>
        ${promptField(x.prompt, 'all', 'يُرسل لجيم عند ضغط المستخدم على العنصر. اكتب ما يحسبه العنصر، ومن أين، وكيف يُعرض، وماذا يفعل إذا نقصت البيانات.')}
        <div class="ed-row">${statusField(x.status)}${plansField(x.plans)}</div>`;
    } else if (k === 'd') {
      const x = ED.item = item ? JSON.parse(JSON.stringify(item)) : { layout: 'a', tint: '', cat: 'sales', name: '', desc: '', sources: [], widgets: blankWidgets(), prompt: 'ابنِ لوحة «» من {{sources}} للفترة {{period}}، بالعناصر التالية:\n{{widgets}}\n- ', status: 'draft', plans: PLANS.map(p => p[0]) };
      ED.orig = x.widgets ? JSON.parse(JSON.stringify(x.widgets)) : null;
      title = ED.isNew ? 'نموذج لوحة جديد' : `تعديل: ${x.name}`; sub = KINDS.d.where;
      form = `
        ${ED.isNew && !item ? field('ed-base', 'ابدأ من', `<select id="ed-base" class="select"><option value="">هيكل فارغ (4 مؤشرات + 6 رسوم)</option>${CAT.dashboards(true).filter(d => !d.p).map(d => `<option value="${d.id}">نسخة من «${esc(d.name)}»</option>`).join('')}</select>`) : ''}
        <div class="ed-row">${field('ed-name', 'اسم النموذج', `<input id="ed-name" value="${esc(x.name)}" required placeholder="مثال: أداء الفروع">`)}
        ${field('ed-cat', 'التصنيف', `<select id="ed-cat" class="select">${CAT.DASH_CATS.filter(c => c[0] !== 'all').map(([c, n]) => `<option value="${c}"${c === x.cat ? ' selected' : ''}>${n}</option>`).join('')}</select>`)}</div>
        ${field('ed-desc', 'الوصف (يظهر في المعرض)', `<input id="ed-desc" value="${esc(x.desc)}" placeholder="لمن هذه اللوحة وماذا تجيب عنه">`)}
        <div class="ed-row">${field('ed-sources', 'المصادر المطلوبة', `<input id="ed-sources" value="${esc((x.sources || []).join('، '))}" placeholder="افصل بينها بفاصلة">`, 'النموذج يظهر فقط في المشاريع التي فيها مصدر مطابق.')}
        ${x.p ? '' : field('ed-layout', 'التوزيع', `<select id="ed-layout" class="select">${['a', 'b', 'c', 'd'].map((l, i) => `<option value="${l}"${l === x.layout ? ' selected' : ''}>توزيع ${['أ', 'ب', 'ج', 'د'][i]}</option>`).join('')}</select>`)}
        ${field('ed-tint', 'اللون', `<select id="ed-tint" class="select">${TINTS.map(([v, n]) => `<option value="${v}"${v === (x.tint || '') ? ' selected' : ''}>${n}</option>`).join('')}</select>`)}</div>
        ${x.p ? `<div class="field"><span class="flabel">صفحات اللوحة <span class="muted" style="font-weight:300">· ${x.p.length} ${x.p.length > 1 ? 'صفحات' : 'صفحة'} و${CAT.countWidgets(x)} عنصرًا${x.f && x.f.length ? ' · الفلاتر: ' + esc(x.f.map(f => f[0]).join('، ')) : ''}${x.ref ? ` · مرجع التصميم: ${esc(x.ref.by)} «${esc(x.ref.items.map(r => r.title).join(' / '))}»` : ''}</span></span>
          <div class="pv-pages">${x.p.map((pg, i) => `<details${i ? '' : ' open'}><summary><b>${esc(pg.n)}</b><span class="num">${pg.w.length} عناصر</span></summary><ol class="pv-list">${pg.w.map(w => `<li><span class="pv-ic">${CAT.GLYPH[window.JR ? window.JR.glyphOf(w.t) : 'table']}</span>${esc(w.ti)}</li>`).join('')}</ol></details>`).join('')}</div>
          <span class="hint">هيكل الصفحات والعناصر من مكتبة اللوحات. <a href="dashboard-gallery.html#${esc(x.id || '')}" target="_blank" rel="noopener">معاينة اللوحة كاملة</a></span></div>`
        : `<div class="field"><span class="flabel">العناصر العشرة <span class="muted" style="font-weight:300">· الأربعة الأولى مؤشرات رقمية، والستة رسوم</span></span><ol class="dw-list">${widgetRows(x.widgets)}</ol></div>`}
        ${promptField(x.prompt, 'dashboard', 'يُرسل لجيم عند اختيار النموذج. استخدم <code dir="ltr">{{widgets}}</code> ليُدرج جيم قائمة العناصر العشرة وتعليماتها تلقائيًا.')}
        <div class="ed-row">${statusField(x.status)}${plansField(x.plans)}<label class="check" style="align-self:center"><input type="checkbox" id="ed-pop"${x.popular ? ' checked' : ''}>شارة «الأكثر استخدامًا»</label></div>`;
    } else if (k === 'k') {
      const i = item, r = i == null ? [0, 0, '', '', 0] : LIB.rows[i];
      ED.idx = i; ED.item = { cat: r[0], ar: r[2], en: r[3], mt: r[4], prompt: i == null ? '' : CAT.kpiPrompt(i), status: i == null ? 'published' : CAT.kpiOwnStatus(i) };
      const x = ED.item;
      title = i == null ? 'مؤشر جديد' : `تعديل: ${r[2]}`; sub = KINDS.k.where;
      form = `
        <div class="ed-row">${field('ed-ar', 'اسم المؤشر (عربي)', `<input id="ed-ar" value="${esc(x.ar)}" required placeholder="مثال: نسبة رضا العملاء">`)}
        ${field('ed-en', 'اسم المؤشر (English)', `<input id="ed-en" value="${esc(x.en)}" dir="ltr" placeholder="Customer satisfaction rate">`)}</div>
        <div class="ed-row">${field('ed-kcat', 'التصنيف', `<select id="ed-kcat" class="select"><optgroup label="الأقسام الوظيفية">${LIB.cats.map((c, j) => c[2] === 0 ? `<option value="${j}"${j === x.cat ? ' selected' : ''}>${c[0]}</option>` : '').join('')}</optgroup><optgroup label="القطاعات">${LIB.cats.map((c, j) => c[2] === 1 ? `<option value="${j}"${j === x.cat ? ' selected' : ''}>${c[0]}</option>` : '').join('')}</optgroup></select>`)}
        ${field('ed-mt', 'نوع القياس', `<select id="ed-mt" class="select">${LIB.mtypes.map((m, j) => `<option value="${j}"${j === x.mt ? ' selected' : ''}>${m}</option>`).join('')}</select>`, 'يحدد شكل العنصر على اللوحة.')}</div>
        <div class="field"><span class="flabel" id="ed-pm-l">مصدر الوصف</span><div class="segmented" role="group" aria-labelledby="ed-pm-l"><button type="button" data-pmode="tpl" aria-pressed="${!x.prompt}">القالب العام</button><button type="button" data-pmode="custom" aria-pressed="${!!x.prompt}">وصف مخصص لهذا المؤشر</button></div></div>
        ${promptField(x.prompt || CAT.kpiTemplate, 'kpi', 'عند اختيار «القالب العام» يُستخدم قالب المكتبة كما هو. اختر «وصف مخصص» لكتابة طريقة حساب أدق لهذا المؤشر.')}
        ${statusField(x.status)}
        ${i != null && !CAT.catOn(r[0]) ? `<div class="cat-banner is-off"><div><b>تصنيف «${LIB.cats[r[0]][0]}» موقوف</b><span>هذا المؤشر لن يظهر للمستخدمين حتى يُظهر التصنيف، مهما كانت حالته هنا.</span></div></div>` : ''}`;
    } else if (k === 'kt') {
      ED.item = { prompt: CAT.kpiTemplate };
      title = 'القالب العام لوصف المؤشرات'; sub = `يُطبّق على ${fmt(kpiCount())} مؤشرًا، عدا المؤشرات التي لها وصف مخصص.`;
      form = promptField(ED.item.prompt, 'kpi', 'استخدم متغيرات المؤشر ليصبح الوصف خاصًا بكل مؤشر تلقائيًا.');
    }
    const canReset = (k === 'w' || k === 'd') && item && item.patched;
    const canDel = (k === 'w' || k === 'd') && item && item.custom;
    const kReset = k === 'k' && item != null && CAT.isPatchedKpi(item) && !CAT.isCustomKpi(item);
    dlg.innerHTML = `
      <form method="dialog" class="ed" novalidate>
        <div class="modal-head">
          <div><span class="badge ${k === 'd' ? 'badge-blue' : ''}">${k === 'kt' ? 'مكتبة المؤشرات' : KINDS[k].one}${ED.item.version && !ED.isNew ? ` · <span class="num">v${ED.item.version}</span>` : ''}</span><h2 id="ed-title">${esc(title)}</h2><p>${sub}</p></div>
          <button class="btn btn-ghost btn-icon btn-sm" type="button" data-close aria-label="إغلاق">${ic('x')}</button>
        </div>
        <div class="ed-body">
          <div class="ed-form">${form}</div>
          ${side ? `<aside class="ed-side" aria-label="معاينة">
            <div class="ed-thumb"></div>
            <div class="ed-out-head"><h3>النص النهائي المرسل لجيم</h3>
              <label class="check sm"><input type="checkbox" id="ed-withrules">مع القواعد العامة</label></div>
            <div class="prompt-out" aria-live="polite"></div>
            <p class="ed-note">${ic('info')}القيم المميّزة مُعوّضة بقيم تجريبية من مشروع «منصة المبيعات». في الاستخدام الفعلي تُستبدل بقيم مشروع المستخدم.</p>
          </aside>` : ''}
        </div>
        <div class="modal-foot">
          ${canDel ? `<button class="btn btn-ghost danger-txt" type="button" data-del>${ic('trash')}حذف النموذج</button>` : ''}
          ${canReset || kReset ? `<button class="btn btn-ghost" type="button" data-reset>${ic('history')}استعادة الأصل</button>` : ''}
          ${k === 'kt' ? `<button class="btn btn-ghost" type="button" data-kt-reset>${ic('history')}استعادة الافتراضي</button>` : ''}
          <span class="note ed-err" role="alert"></span>
          <button class="btn btn-ghost" type="button" data-close>إلغاء</button>
          <button class="btn btn-primary" type="submit">${ic('check')}${ED.isNew ? 'إضافة النموذج' : 'حفظ التغييرات'}</button>
        </div>
      </form>`;
    dlg.setAttribute('aria-labelledby', 'ed-title');
    wire();
    dlg.showModal();
    $('input:not([type=checkbox]):not([type=radio]), textarea', dlg)?.focus();
  }

  /* قراءة النموذج من الحقول */
  const read = () => {
    const k = ED.kind, x = ED.item, v = id => $('#' + id, dlg)?.value.trim();
    const status = $('[data-status][aria-pressed="true"]', dlg)?.dataset.status;
    const plans = $$('input[name=plan]:checked', dlg).map(c => c.value);
    const prompt = $('#ed-prompt', dlg).value.trim();
    if (k === 'w') Object.assign(x, { name: v('ed-name'), cat: v('ed-cat'), desc: v('ed-desc'), render: $('input[name=render]:checked', dlg).value, span: +v('ed-span'), src: v('ed-src'), prompt, status, plans });
    if (k === 'd') {
      const ws = x.p ? null : $$('.dw-row', dlg).map((row, i) => {
        const type = i < 4 ? 'kpi' : $('[data-k=type]', row).value, o = ED.orig[i];
        const base = o && o.type === type ? Object.assign({}, o) : { type, sub: '' };
        return Object.assign(base, { title: $('[data-k=title]', row).value.trim(), prompt: $('[data-k=prompt]', row).value.trim() });
      });
      Object.assign(x, { name: v('ed-name'), cat: v('ed-cat'), desc: v('ed-desc'), sources: v('ed-sources').split(/[،,]/).map(s => s.trim()).filter(Boolean), layout: v('ed-layout') || x.layout, tint: v('ed-tint'), ...(ws ? { widgets: ws } : {}), prompt, status, plans, popular: $('#ed-pop', dlg).checked });
    }
    if (k === 'k') {
      const custom = $('[data-pmode="custom"]', dlg).getAttribute('aria-pressed') === 'true';
      Object.assign(x, { ar: v('ed-ar'), en: v('ed-en'), cat: +v('ed-kcat'), mt: +v('ed-mt'), prompt: custom ? prompt : '', status });
    }
    if (k === 'kt') x.prompt = prompt;
    return x;
  };

  const preview = () => {
    const k = ED.kind, x = read();
    let text = x.prompt, extra = {};
    if (k === 'd') extra.widgets = CAT.widgetList(x);
    if (k === 'k' || k === 'kt') {
      const c = k === 'k' ? x : { ar: 'نسبة رضا العملاء', en: 'Customer satisfaction rate', cat: 0, mt: 0 };
      if (k === 'k' && !x.prompt) text = CAT.kpiTemplate;
      extra = { kpi_name: c.ar || '—', kpi_name_en: c.en || '—', category: LIB.cats[c.cat][0], measure_type: LIB.mtypes[c.mt] };
    }
    const withRules = $('#ed-withrules', dlg)?.checked;
    $('.prompt-out', dlg).innerHTML = (withRules ? `<div class="po-rules">${fillHtml(CAT.rules, extra)}</div>` : '') + `<div>${fillHtml(text, extra) || '<span class="muted">اكتب الوصف ليظهر هنا…</span>'}</div>`;
    const th = $('.ed-thumb', dlg);
    if (k === 'w') th.innerHTML = `<div class="et-w s${x.span}"><span class="tpl-thumb">${CAT.THUMB[x.render]}</span><b>${esc(x.name) || 'اسم العنصر'}</b><small>${esc(x.desc) || 'الوصف المختصر'}</small></div><span class="et-cap">كما يظهر في تبويب «النماذج» · العرض ${x.span} من 12</span>`;
    else if (k === 'd') th.innerHTML = `${CAT.thumb(x)}<span class="et-cap">${esc(x.name) || 'اسم النموذج'} · ${x.p ? CAT.countWidgets(x) : 10} عناصر</span>`;
    else if (k === 'k') th.innerHTML = `<div class="et-k"><b>${esc(x.ar) || 'اسم المؤشر'}</b><small dir="ltr">${esc(x.en)}</small><span class="badge">${LIB.mtypes[x.mt]}</span><span class="badge">${x.prompt ? 'وصف مخصص' : 'القالب العام'}</span></div>`;
    else th.innerHTML = `<div class="et-k"><b>مثال: نسبة رضا العملاء</b><small dir="ltr">Customer satisfaction rate</small><span class="badge">خدمة العملاء</span><span class="badge">نسبة</span></div>`;
  };

  function wire() {
    const ta = $('#ed-prompt', dlg);
    bindPrompt(ta, dlg, preview);
    $$('[data-close]', dlg).forEach(b => b.addEventListener('click', () => dlg.close()));
    $('form', dlg).addEventListener('input', preview);
    $('form', dlg).addEventListener('change', preview);
    $$('[data-status]', dlg).forEach(b => b.addEventListener('click', () => { $$('[data-status]', dlg).forEach(x => x.setAttribute('aria-pressed', String(x === b))); }));
    $$('input[name=render]', dlg).forEach(r => r.addEventListener('change', () => $$('.tp', dlg).forEach(l => l.classList.toggle('is-on', $('input', l).checked))));
    /* نوع القياس في المؤشر: القالب العام ↔ وصف مخصص */
    const pm = $$('[data-pmode]', dlg);
    if (pm.length) {
      const setMode = custom => {
        pm.forEach(b => b.setAttribute('aria-pressed', String((b.dataset.pmode === 'custom') === custom)));
        ta.readOnly = !custom; ta.classList.toggle('is-ro', !custom);
        if (!custom) ta.value = CAT.kpiTemplate;
        $('.ed-prompt .var-chips', dlg).hidden = !custom;
        ta.dispatchEvent(new Event('input'));
      };
      pm.forEach(b => b.addEventListener('click', () => setMode(b.dataset.pmode === 'custom')));
      setMode(!!ED.item.prompt);
    }
    /* لوحة: تغيير نوع الرسم يحدّث الأيقونة، و«ابدأ من» ينسخ هيكل نموذج */
    const list = $('.dw-list', dlg);
    if (list) {
      list.addEventListener('change', e => { if (e.target.dataset.k === 'type') $('.dw-ic', e.target.closest('.dw-row')).innerHTML = CAT.GLYPH[e.target.value]; });
      $('#ed-base', dlg)?.addEventListener('change', e => {
        const b = e.target.value && CAT.get('d', e.target.value);
        const ws = b ? JSON.parse(JSON.stringify(b.widgets)) : blankWidgets();
        ED.orig = JSON.parse(JSON.stringify(ws)); list.innerHTML = widgetRows(ws);
        if (b) { $('#ed-layout', dlg).value = b.layout || 'a'; $('#ed-tint', dlg).value = b.tint || ''; if (!$('#ed-sources', dlg).value) $('#ed-sources', dlg).value = b.sources.join('، '); if (/«»/.test(ta.value) || !ta.value.trim()) { ta.value = b.prompt; ta.dispatchEvent(new Event('input')); } }
        preview();
      });
    }
    $('#ed-withrules', dlg)?.addEventListener('change', preview);
    /* حذف / استعادة */
    const del = $('[data-del]', dlg);
    del?.addEventListener('click', () => {
      if (!del.dataset.sure) { del.dataset.sure = 1; del.innerHTML = `${ic('trash')}اضغط مرة أخرى للتأكيد`; setTimeout(() => { if (del.isConnected) { delete del.dataset.sure; del.innerHTML = `${ic('trash')}حذف النموذج`; } }, 3000); return; }
      CAT.remove(ED.kind, ED.item.id); dlg.close(); toast(`تم حذف «${ED.item.name}»`); refresh();
    });
    $('[data-reset]', dlg)?.addEventListener('click', () => {
      if (ED.kind === 'k') { CAT.resetKpi(ED.idx); toast('تمت استعادة المؤشر الأصلي (يتطلب إعادة تحميل الصفحة لاستعادة الاسم)'); }
      else { CAT.reset(ED.kind, ED.item.id); toast(`تمت استعادة «${CAT.get(ED.kind, ED.item.id).name}» للأصل`); }
      dlg.close(); refresh();
    });
    $('[data-kt-reset]', dlg)?.addEventListener('click', () => { ta.value = CAT.defaults.kpiTemplate; ta.dispatchEvent(new Event('input')); });
    /* حفظ */
    $('form', dlg).addEventListener('submit', e => {
      e.preventDefault();
      const x = read(), err = $('.ed-err', dlg);
      const bad = [];
      if ((ED.kind === 'w' || ED.kind === 'd') && !x.name) bad.push('#ed-name');
      if (ED.kind === 'k' && !x.ar) bad.push('#ed-ar');
      if (ED.kind === 'd') $$('.dw-row [data-k=title]', dlg).forEach(i => { if (!i.value.trim()) bad.push('#' + i.id); });
      if (!$('#ed-prompt', dlg).value.trim()) bad.push('#ed-prompt');
      $$('[aria-invalid]', dlg).forEach(i => i.removeAttribute('aria-invalid'));
      if (bad.length) { bad.forEach(s => $(s, dlg)?.setAttribute('aria-invalid', 'true')); err.textContent = 'أكمل الحقول المطلوبة المميّزة باللون الأحمر.'; $(bad[0], dlg)?.focus(); return; }
      if (CAT.unknownVars($('#ed-prompt', dlg).value).length) { err.textContent = 'في الوصف متغير غير معروف، صحّحه أو احذفه قبل الحفظ.'; $('#ed-prompt', dlg).focus(); return; }
      let msg;
      if (ED.kind === 'kt') { CAT.kpiTemplate = x.prompt; msg = `تم حفظ القالب العام — يُطبّق على ${fmt(kpiCount())} مؤشرًا`; }
      else if (ED.kind === 'k') { CAT.saveKpi(ED.idx, x); msg = ED.idx == null ? `تمت إضافة «${x.ar}» للمكتبة` : `تم حفظ «${x.ar}»`; }
      else { const s = CAT.save(ED.kind, x); msg = `${ED.isNew ? 'تمت إضافة' : 'تم حفظ'} «${s.name}» · الإصدار ${s.version}${s.status === 'published' ? ' — يظهر للمستخدمين الآن' : ''}`; }
      dlg.close(); toast(msg); refresh();
    });
    preview();
  }
  dlg.addEventListener('click', e => { if (e.target === dlg) dlg.close(); });

  newBtn.addEventListener('click', () => kind === 'k' ? openEditor('k', null, true) : openEditor(kind, null, true));

  /* ---------- رسم الصفحة ---------- */
  function render() { tabs(); ({ w: listPane, d: listPane, k: kpiPane, r: rulesPane })[kind](); }
  function refresh() { stats(); render(); }
  refresh();
})();
