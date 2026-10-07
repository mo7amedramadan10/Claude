/* ==========================================================
   جيم — المحلل الذكي (Phase 2: الواجهة الحقيقية)
   يستهلك POST /api/analyst/ask (SSE حقيقي) وGET /api/analyst/results/{id} — لا بيانات تجريبية.
   مراحل العرض (sources/understand/query/verify/compose) بتُحدَّث لحظيًا من أحداث SSE الفعلية،
   مش من تايمر. حالة التوثيق (verified/unverified) من VerificationJson الحقيقي اللي الباك إند
   بيرجّعه — في المرحلة ١-٢ كل نتيجة org_data بتطلع "unverified" بسبب صريح (الفحوصات الكاملة
   C1-C4 لسه المرحلة ٤)، مش حكم وهمي.
   أزرار «لخّص/قارن/صِف الأفضل» (مرحلة ٥) و«تحقق الآن» (مرحلة ٤) و«حفظ كتقرير/مشاركة» (مرحلة ٦)
   ظاهرة زي التصميم بالظبط لكن بتاخد رسالة «قريبًا» بدل نداء نقطة نهاية مش موجودة بعد.
   ========================================================== */
(function () {
  const root = document.getElementById('anRoot'); if (!root) return;
  const $ = (s, elp = root) => elp.querySelector(s), $$ = (s, elp = root) => [...elp.querySelectorAll(s)];
  const reduceMotion = matchMedia('(prefers-reduced-motion: reduce)').matches;
  // esc()/tplIc() are app.js's own globals (loaded after this file, but only ever called here
  // from functions that run later, at user-interaction time — never at this IIFE's own
  // top-level, so load order between the two script tags doesn't matter).
  const escAn = s => (typeof esc === 'function' ? esc(s) : String(s ?? ''));
  const ic = (n, c = 'icon icon-sm') => (typeof tplIc === 'function' ? tplIc(n, c) : `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`);
  const nf = (v, d = 0) => { const n = Number(v); return Number.isFinite(n) ? n.toLocaleString('en-US', { maximumFractionDigits: d, minimumFractionDigits: d }) : String(v ?? ''); };
  const norm = s => String(s ?? '').replace(/[أإآ]/g, 'ا').replace(/ة/g, 'ه').replace(/ى/g, 'ي');

  const STAGE_LABELS = { sources: 'اختيار المصادر', understand: 'فهم السؤال', query: 'كتابة الاستعلام', verify: 'التحقق من الأرقام', compose: 'صياغة الإجابة' };
  const STAGE_ORDER = ['sources', 'understand', 'query', 'verify', 'compose'];

  const AN = {
    conversationId: null,
    resultsCache: {}, // resultId -> fetched full result object
    curResultId: null,
    view: 'table', sort: null, q: '',
    tab: 'summary',
    busy: false,
    loaded: false,
  };

  /* ---------- تدفق SSE ---------- */
  async function streamSSE(url, body, handlers) {
    let res;
    try {
      res = await fetch(url, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
    } catch {
      handlers.error?.({ message: 'تعذّر الاتصال بالخادم.' });
      handlers.done?.({});
      return;
    }
    if (!res.ok || !res.body) {
      // Deliberately NOT showing whatever text the server's generic exception handler sent
      // (e.g. a raw .NET exception message for a mid-request configuration failure) — spec
      // section 13: errors shown to a normal user must stay in clean Arabic, never leak
      // internals. A real, curated Arabic message from the ask pipeline itself arrives as a
      // proper SSE "error" event instead (handled above this branch, response.ok=true there);
      // this branch only ever fires for a connection-level failure before that pipeline even
      // started, so one fixed message is honest and safe either way.
      handlers.error?.({ message: 'تعذّر تنفيذ الطلب — حاول مرة أخرى بعد قليل.' });
      handlers.done?.({});
      return;
    }
    const reader = res.body.getReader();
    const decoder = new TextDecoder();
    let buf = '';
    while (true) {
      let chunk;
      try { chunk = await reader.read(); } catch { break; }
      if (chunk.done) break;
      buf += decoder.decode(chunk.value, { stream: true });
      let idx;
      while ((idx = buf.indexOf('\n\n')) >= 0) {
        const frame = buf.slice(0, idx); buf = buf.slice(idx + 2);
        let eventName = 'message', dataLine = '';
        for (const line of frame.split('\n')) {
          if (line.startsWith('event: ')) eventName = line.slice(7).trim();
          else if (line.startsWith('data: ')) dataLine += line.slice(6);
        }
        let payload = {};
        try { payload = dataLine ? JSON.parse(dataLine) : {}; } catch { /* تجاهل إطار تالف */ }
        handlers[eventName]?.(payload);
      }
    }
  }

  /* ---------- المحادثة (يسار) ---------- */
  const thread = $('#anThread');
  const scroll = () => thread.scrollTo({ top: thread.scrollHeight, behavior: reduceMotion ? 'auto' : 'smooth' });
  function addUser(q) { thread.insertAdjacentHTML('beforeend', `<div class="an-u"><p>${escAn(q)}</p></div>`); scroll(); }

  function addAssistantShell() {
    const el = document.createElement('div');
    el.className = 'an-a';
    el.innerHTML = `<span class="an-mark busy"><i class="an-spin"></i></span><div class="an-body"><div class="an-steps"></div><div class="an-ans" hidden></div></div>`;
    thread.append(el); scroll();
    return el;
  }

  function renderStages(elp, stageState) {
    const box = $('.an-steps', elp);
    box.innerHTML = `<div class="an-live">${STAGE_ORDER.map(id => {
      const st = stageState[id] || 'pending';
      const icon = st === 'done' ? ic('check') : st === 'failed' ? ic('x') : st === 'running' ? '<i class="an-spin"></i>' : '<i class="an-o"></i>';
      const cls = st === 'done' ? 'done' : st === 'running' ? 'now' : st === 'failed' ? 'failed' : '';
      return `<span class="${cls}">${icon}${escAn(STAGE_LABELS[id] || id)}${st === 'running' ? '…' : ''}</span>`;
    }).join('')}</div>`;
    scroll();
  }

  function linkify(text, keyColumn, rows) {
    const safe = escAn(text);
    if (!keyColumn || !rows?.length) return safe;
    const names = [...new Set(rows.map(r => String(r[keyColumn] ?? '')).filter(Boolean))]
      .sort((a, b) => b.length - a.length)
      .map(n => escAn(n))
      .filter(n => n.length > 0);
    if (!names.length) return safe;
    const pattern = new RegExp('(' + names.map(n => n.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')).join('|') + ')', 'g');
    return safe.replace(pattern, m => `<button type="button" class="an-ent" data-row-key="${m}">${m}</button>`);
  }

  function renderFinalAnswer(elp, msg, verified, result) {
    $('.an-mark', elp).className = `an-mark ${verified === false ? 'warn' : verified === true ? 'ok' : ''}`;
    $('.an-mark', elp).innerHTML = verified === false ? '!' : verified === true ? ic('check') : ic('spark');
    const ans = $('.an-ans', elp); ans.hidden = false;
    const bodyHtml = linkify(msg.html, result?.keyColumn, result?.rows);
    ans.innerHTML = `
      ${msg.knowledgeScope === 'general' ? `<span class="an-unv" style="color:var(--ink-3);background:var(--surface-2);border-color:var(--line)">${ic('info')}من خارج البيانات</span>` : ''}
      ${verified === false ? `<span class="an-unv">${ic('info')}غير موثّقة</span>` : ''}
      <p class="an-lead">${bodyHtml}</p>
      ${msg.resultId ? `<div class="an-foot"><button type="button" class="an-open" data-open-result="${escAn(msg.resultId)}">${ic('layers')}عرض في مساحة التحليل</button></div>` : ''}`;
    if (!msg.resultId) elp.classList.remove('is-cur');
    scroll();
  }

  function renderError(elp, payload) {
    $('.an-mark', elp).className = 'an-mark warn';
    $('.an-mark', elp).innerHTML = '!';
    const ans = $('.an-ans', elp); ans.hidden = false;
    ans.innerHTML = `<p class="an-lead">${escAn(payload.message || 'حدث خطأ غير متوقع.')}</p>`;
    scroll();
  }

  /* ---------- السؤال ---------- */
  function askAnalyst(question) {
    if (AN.busy || !question.trim()) return;
    AN.busy = true;
    $('#anSend').disabled = true;
    addUser(question);
    const elp = addAssistantShell();
    const stageState = {};
    renderStages(elp, stageState);

    const sourceIds = [...new Set([...(state.onSystems || []), ...(state.onFiles || [])])];
    streamSSE('/api/analyst/ask', { conversationId: AN.conversationId, question, sourceIds }, {
      conversation(p) { AN.conversationId = p.conversationId || AN.conversationId; },
      stage(p) { stageState[p.id] = p.state; renderStages(elp, stageState); },
      result(p) { elp.dataset.resultId = p.resultId; },
      message(p) {
        let verified;
        if (p.resultId) {
          // The real result (with its verification block) is fetched right after, so the
          // bubble's own badge/warning state reflects whatever the server actually recorded —
          // never guessed from the message event alone.
          fetchAndOpenResult(p.resultId, elp, p);
        } else {
          renderFinalAnswer(elp, p, p.knowledgeScope === 'org_data' ? false : undefined, null);
        }
      },
      error(p) { renderError(elp, p); },
      done() { AN.busy = false; $('#anSend').disabled = false; },
    });
  }

  async function fetchAndOpenResult(resultId, elp, msg) {
    try {
      const res = await fetch('/api/analyst/results/' + encodeURIComponent(resultId));
      if (!res.ok) { renderFinalAnswer(elp, msg, false, null); return; }
      const result = await res.json();
      AN.resultsCache[resultId] = result;
      renderFinalAnswer(elp, msg, result.verification?.status === 'verified', result);
      elp.classList.add('is-cur');
      openResult(resultId, elp);
    } catch {
      renderFinalAnswer(elp, msg, false, null);
    }
  }

  /* ---------- مساحة التحليل (يمين) ---------- */
  function openResult(resultId, sourceEl) {
    AN.curResultId = resultId;
    AN.sort = null; AN.q = ''; AN.tab = 'summary';
    $$('.an-a', thread).forEach(a => a.classList.toggle('is-cur', a === sourceEl || a.dataset.resultId === resultId));
    canvas();
  }

  function formatCellValue(v) {
    if (v === null || v === undefined) return '';
    if (typeof v === 'number') return `<span class="num">${nf(v, Number.isInteger(v) ? 0 : 2)}</span>`;
    return escAn(v);
  }

  function isNumericColumn(rows, col) {
    return rows.length > 0 && rows.every(r => r[col] === null || r[col] === undefined || typeof r[col] === 'number');
  }

  function rowsOf(result) {
    let rows = result.rows;
    if (AN.q) {
      const qn = norm(AN.q);
      rows = rows.filter(r => Object.values(r).some(v => v != null && norm(String(v)).includes(qn)));
    }
    if (AN.sort) {
      const col = result.columns[AN.sort.i];
      rows = [...rows].sort((a, b) => {
        const x = a[col], y = b[col];
        const cmp = (typeof x === 'number' && typeof y === 'number') ? x - y : String(x ?? '').localeCompare(String(y ?? ''), 'ar');
        return cmp * AN.sort.d;
      });
    }
    return rows;
  }

  function canvas() {
    const C = $('#anCanvas'), result = AN.resultsCache[AN.curResultId];
    if (!result) {
      C.innerHTML = `<div class="an-empty"><img src="img/logo-mark.svg" alt=""><h2>اسأل عن بيانات المشروع</h2><p>النتيجة تظهر هنا: ملخص، وجدول أو رسم، ومصادر البيانات، وتحليل مقارن.</p></div>`;
      return;
    }
    const verified = result.verification?.status === 'verified';
    C.innerHTML = `
      <header class="an-h">
        <div><h1>${escAn(result.title)}</h1>
          <div class="an-meta">
            ${verified ? `<span class="an-ver ok">${ic('check')}موثّقة</span>` : `<span class="an-ver warn">${ic('info')}غير موثّقة</span>`}
            <span class="muted">${result.rows.length.toLocaleString('ar')} ${result.rows.length === 1 ? 'صف' : 'صفوف'}${result.isTruncated ? ' (مقتطعة)' : ''}</span>
          </div>
        </div>
        <button type="button" class="btn btn-sm" data-src aria-expanded="false">${ic('database')}مصادر البيانات</button>
      </header>
      ${verified ? '' : `<div class="an-warnbox">${ic('info')}<span>${escAn(result.verification?.reason || 'لم يتم التحقق من هذه الأرقام بعد.')}</span><button type="button" class="btn btn-sm" data-verify>${ic('check')}تحقق الآن</button></div>`}
      <div class="an-srcpanel" hidden>
        <ul>${(result.sources || []).map(s => `<li>${ic(s.sourceKind === 'file' ? 'sheet' : 'server')}<span><b>${escAn(s.sourceDisplayName)}</b><small class="mono">${escAn((s.tables || []).join(' · '))}</small></span><em class="${s.isStale ? 'tx-warn' : ''}">${s.sourceLastUpdatedAt ? new Date(s.sourceLastUpdatedAt).toLocaleDateString('ar-EG') : ''}</em></li>`).join('') || `<li><span class="muted">لم يتمكن النظام من تحديد المصدر بدقة.</span></li>`}</ul>
        ${result.executedSql ? `<details><summary>${ic('file')}الاستعلام المستخدم (SQL)</summary><pre class="an-sql" dir="ltr">${escAn(result.executedSql)}</pre></details>` : ''}
      </div>
      <section class="an-sec" data-sec="sum">
        <div class="an-sh"><button type="button" class="an-col" aria-expanded="true">${ic('chev-down')}الوصف المختصر</button>
          <label class="an-search">${ic('search')}<span class="sr-only">ابحث في النتائج</span><input type="search" placeholder="ابحث في النتائج" value="${escAn(AN.q)}"></label>
          <button type="button" class="an-ib" data-dl aria-label="تنزيل CSV" title="تنزيل CSV">${ic('download')}</button>
          <span class="an-seg" role="group" aria-label="طريقة العرض"><button type="button" data-v="table" aria-pressed="${AN.view === 'table'}" aria-label="جدول">${ic('sheet')}</button><button type="button" data-v="chart" aria-pressed="${AN.view === 'chart'}" aria-label="رسم">${ic('chart')}</button></span></div>
        <div class="an-sb"><div class="an-data"></div></div>
      </section>
      <section class="an-sec" data-sec="ana">
        <div class="an-sh"><span class="an-st">${ic('spark')}التحليل</span></div>
        <div class="an-tabs" role="tablist">${[['summary', 'لخّص', 'file'], ['compare', 'اشرح أهم المقارنات', 'spark'], ['best', 'صِف الأفضل', 'star']].map(([k, n, i]) => `<button type="button" role="tab" data-tab="${k}" aria-selected="${AN.tab === k}">${ic(i)}${n}</button>`).join('')}</div>
        <div class="an-tb"></div>
      </section>
      <div class="an-acts"><button type="button" class="btn btn-sm" data-act="report">${ic('file')}حفظ كتقرير</button><button type="button" class="btn btn-sm" data-act="share">${ic('share')}مشاركة النتيجة</button><button type="button" class="btn btn-sm" data-act="follow">${ic('chat')}سؤال متابعة</button></div>`;
    data(); tabPanel();
    $('.an-search input', C).addEventListener('input', e => { AN.q = e.target.value; data(); });
  }

  function data() {
    const result = AN.resultsCache[AN.curResultId], rows = rowsOf(result), box = $('#anCanvas .an-data');
    if (!result) return;
    if (AN.view === 'table') {
      box.innerHTML = `<div class="an-tw"><table class="table an-table"><thead><tr>${result.columns.map((c, i) => `<th><button type="button" data-sort="${i}">${escAn(c)}${AN.sort && AN.sort.i === i ? (AN.sort.d > 0 ? ' ▲' : ' ▼') : ''}</button></th>`).join('')}</tr></thead>
        <tbody>${rows.map(r => `<tr data-row-key="${escAn(result.keyColumn ? r[result.keyColumn] : '')}">${result.columns.map(c => `<td>${formatCellValue(r[c])}</td>`).join('')}</tr>`).join('') || `<tr><td colspan="${result.columns.length}" class="muted">لا نتائج مطابقة.</td></tr>`}</tbody></table></div>`;
    } else {
      const measure = result.primaryMeasure && result.columns.includes(result.primaryMeasure) ? result.primaryMeasure : result.columns.find(c => isNumericColumn(result.rows, c));
      if (!measure) { box.innerHTML = `<p class="muted">لا يوجد عمود رقمي مناسب للرسم.</p>`; return; }
      const sorted = [...rows].sort((a, b) => Math.abs(Number(b[measure]) || 0) - Math.abs(Number(a[measure]) || 0));
      const mx = Math.max(...sorted.map(r => Math.abs(Number(r[measure]) || 0)), 1);
      const label = result.keyColumn || result.columns[0];
      box.innerHTML = `<p class="an-cl">${escAn(measure)}</p><ul class="an-hb">${sorted.map(r => `<li data-row-key="${escAn(r[label])}"><span>${escAn(r[label])}</span><span class="t"><i style="width:${(Math.abs(Number(r[measure]) || 0) / mx * 100).toFixed(1)}%"></i></span><b class="num">${nf(r[measure], Number.isInteger(r[measure]) ? 0 : 2)}</b></li>`).join('')}</ul>`;
    }
  }

  function tabPanel() {
    const result = AN.resultsCache[AN.curResultId], B = $('#anCanvas .an-tb');
    $$('#anCanvas [data-tab]').forEach(b => b.setAttribute('aria-selected', String(b.dataset.tab === AN.tab)));
    if (AN.tab === 'summary') {
      const measure = result.primaryMeasure && result.columns.includes(result.primaryMeasure) ? result.primaryMeasure : null;
      if (!measure) { B.innerHTML = `<p class="muted">التحليل التلقائي يحتاج مقياسًا رقميًا أساسيًا لهذه النتيجة.</p>`; return; }
      const sorted = [...result.rows].sort((a, b) => (Number(b[measure]) || 0) - (Number(a[measure]) || 0));
      const label = result.keyColumn || result.columns[0];
      const total = sorted.reduce((s, r) => s + Math.abs(Number(r[measure]) || 0), 0);
      const top3 = sorted.slice(0, 3).reduce((s, r) => s + Math.abs(Number(r[measure]) || 0), 0);
      B.innerHTML = `<ul class="an-pts">
        <li><b>الأعلى:</b> «${escAn(sorted[0]?.[label])}».</li>
        <li><b>الأدنى:</b> «${escAn(sorted[sorted.length - 1]?.[label])}».</li>
        <li><b>التركّز:</b> أعلى 3 يمثلون ${total ? Math.round(top3 / total * 100) : 0}% من الإجمالي.</li>
      </ul>`;
      return;
    }
    B.innerHTML = `<div class="an-soon" style="padding:18px;text-align:center;color:var(--ink-3)">${ic('info')}<p style="margin-top:8px">هذه الميزة قيد التطوير وستتوفر قريبًا.</p></div>`;
  }

  /* ---------- الأحداث ---------- */
  root.addEventListener('click', e => {
    const t = e.target;
    const openBtn = t.closest('[data-open-result]'); if (openBtn) { const id = openBtn.dataset.openResult; const a = openBtn.closest('.an-a'); openResult(id, a); return; }
    const ent = t.closest('.an-ent'); if (ent) {
      const key = ent.dataset.rowKey;
      const row = $(`#anCanvas [data-row-key="${CSS.escape(key)}"]`);
      if (row) { AN.view = 'table'; data(); const tr = $(`#anCanvas [data-row-key="${CSS.escape(key)}"]`); if (tr) { tr.scrollIntoView({ block: 'center', behavior: reduceMotion ? 'auto' : 'smooth' }); tr.classList.remove('flash'); void tr.offsetWidth; tr.classList.add('flash'); } }
      return;
    }
    const v = t.closest('[data-v]'); if (v) { AN.view = v.dataset.v; $$('#anCanvas [data-v]').forEach(b => b.setAttribute('aria-pressed', String(b === v))); return data(); }
    const so = t.closest('[data-sort]'); if (so) { const i = +so.dataset.sort; AN.sort = AN.sort && AN.sort.i === i ? { i, d: -AN.sort.d } : { i, d: -1 }; canvasRerenderTable(); return; }
    const tb = t.closest('[data-tab]'); if (tb) { AN.tab = tb.dataset.tab; return tabPanel(); }
    if (t.closest('.an-col')) { const sec = t.closest('.an-sec'), b = $('.an-col', sec), x = b.getAttribute('aria-expanded') === 'true'; b.setAttribute('aria-expanded', String(!x)); $('.an-sb', sec).hidden = x; return; }
    if (t.closest('[data-src]')) { const p = $('#anCanvas .an-srcpanel'), b = t.closest('[data-src]'); p.hidden = !p.hidden; b.setAttribute('aria-expanded', String(!p.hidden)); return; }
    if (t.closest('[data-verify]')) { toast('ميزة التحقق التلقائي الكامل قيد التطوير وستتوفر قريبًا.'); return; }
    if (t.closest('[data-dl]')) { if (AN.curResultId) window.open('/api/analyst/results/' + encodeURIComponent(AN.curResultId) + '/export.csv', '_blank'); return; }
    const ac = t.closest('[data-act]'); if (ac) {
      if (ac.dataset.act === 'follow') { $('#anIn').focus(); return; }
      toast('هذه الميزة قيد التطوير وستتوفر قريبًا.');
      return;
    }
    const sg = t.closest('[data-sug]'); if (sg) { askAnalyst(sg.dataset.sug); return; }
    const sc = t.closest('[data-srcchip]'); if (sc) { toggleSource(sc.dataset.srcchip); return; }
    if (t.closest('#anAttach')) { const p = $('#anPick'); p.hidden = !p.hidden; $('#anAttach').setAttribute('aria-expanded', String(!p.hidden)); return; }
    const ps = t.closest('[data-pick-src]'); if (ps) { toggleSource(ps.dataset.pickSrc); return; }
    if (t.closest('#anMic')) { toast('الإدخال الصوتي قيد التطوير وسيتوفر قريبًا.'); return; }
    if (!t.closest('#anPick') && !t.closest('#anAttach')) $('#anPick').hidden = true;
  });

  function canvasRerenderTable() {
    // Re-sort the header's own ▲/▼ markers too, not just the row order — cheapest correct
    // way is just re-running canvas() fully rather than patching both independently.
    canvas();
  }

  function toast(msg) {
    let t = document.querySelector('.toast');
    if (!t) { t = document.createElement('div'); t.className = 'toast'; t.setAttribute('role', 'status'); document.body.appendChild(t); }
    t.innerHTML = `${ic('check')}<span>${escAn(msg)}</span>`;
    t.classList.remove('is-on'); void t.offsetWidth; t.classList.add('is-on');
    clearTimeout(t._h); t._h = setTimeout(() => t.classList.remove('is-on'), 2800);
  }

  /* ---------- المصادر المرفقة (نفس state.systems/onSystems اللي شاشة «المحادثة» بتستخدمها) ---------- */
  function toggleSource(id) {
    const set = (state.systems || []).some(s => s.id === id) ? state.onSystems : state.onFiles;
    if (set.has(id)) set.delete(id); else set.add(id);
    chips();
  }
  function sourceLabel(id) {
    const sys = (state.systems || []).find(s => s.id === id);
    if (sys) return { name: sys.name, icon: sys.kind === 'integration' ? 'database' : 'server', stale: false };
    const file = (state.sourceFiles || []).find(f => f.id === id);
    if (file) return { name: file.name || file.displayName || id, icon: 'sheet', stale: false };
    return { name: id, icon: 'file', stale: false };
  }
  function chips() {
    const onIds = [...(state.onSystems || []), ...(state.onFiles || [])];
    $('#anChips').innerHTML = onIds.length
      ? onIds.map(id => { const s = sourceLabel(id); return `<button type="button" class="an-chip" data-srcchip="${escAn(id)}" aria-label="إزالة ${escAn(s.name)}">${ic(s.icon)}${escAn(s.name)}${ic('x')}</button>`; }).join('')
      : '<span class="muted">لا مصادر مرفقة · سيستخدم جيم كل مصادر المشروع</span>';
    const allSources = [...(state.systems || []).map(s => ({ id: s.id, name: s.name, icon: s.kind === 'integration' ? 'database' : 'server' })),
      ...(state.sourceFiles || []).map(f => ({ id: f.id, name: f.name || f.displayName || f.id, icon: 'sheet' }))];
    $('#anPick').innerHTML = `<p>المصادر المستخدمة في الإجابة</p>${allSources.map(s => {
      const on = state.onSystems?.has(s.id) || state.onFiles?.has(s.id);
      return `<button type="button" data-pick-src="${escAn(s.id)}" role="menuitemcheckbox" aria-checked="${on}">${ic(s.icon)}<span><b>${escAn(s.name)}</b></span>${on ? ic('check') : ''}</button>`;
    }).join('') || '<p class="muted">لا توجد مصادر متاحة لهذا المشروع بعد.</p>'}`;
  }

  /* ---------- أسئلة مقترحة ثابتة ---------- */
  const SUG = ['لخّص أداء هذا الشهر', 'ما أكبر 5 عملاء من حيث الإيراد؟', 'قارن الأداء بين المناطق', 'ما المنتجات الأعلى نموًا؟'];
  function renderSuggestions() {
    $('#anSug').innerHTML = SUG.slice(0, 2).map(s => `<button type="button" class="an-sg" data-sug="${escAn(s)}">${escAn(s)}</button>`).join('') + `<button type="button" class="an-sg more" data-more>+${SUG.length - 2}</button>`;
  }
  $('#anSug').addEventListener('click', e => { if (e.target.closest('[data-more]')) $('#anSug').innerHTML = SUG.map(s => `<button type="button" class="an-sg" data-sug="${escAn(s)}">${escAn(s)}</button>`).join(''); });

  /* ---------- النموذج ---------- */
  $('#anForm').addEventListener('submit', e => { e.preventDefault(); const q = $('#anIn').value.trim(); if (!q || AN.busy) return; $('#anIn').value = ''; askAnalyst(q); });
  $('#anIn').addEventListener('keydown', e => { if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); $('#anForm').requestSubmit(); } });
  $('#anNew').addEventListener('click', () => {
    thread.innerHTML = ''; AN.conversationId = null; AN.curResultId = null; AN.resultsCache = {};
    canvas(); toast('بدأت جلسة تحليل جديدة.');
  });

  /* ---------- نقطة الدخول (تُنادى من app.js عند فتح شاشة المحلل الذكي) ---------- */
  window.loadAnalystScreen = function () {
    if (!AN.loaded) {
      AN.loaded = true;
      renderSuggestions();
      canvas();
    }
    chips();
  };
})();
