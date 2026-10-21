/* ==========================================================
   جيم — خريطة سير العمل ومستويات الخدمة (لوحة تفاعلية)
   مراحل ثابتة بالترتيب، والطلبات نقاط تتحرك بينها عبر الزمن:
   - العملاء مدخل الخريطة (يمين)، والموردون أسفلها مرتبطون بمرحلة التوريد.
   - لون النقطة = حالة الالتزام بمدة المرحلة (ضمن المدة / معرض للتأخر / متجاوز) مع شكل مختلف لكل حالة.
   - ضغطة على مرحلة أو طلب أو عميل أو مورد تُنوّر ما يخصه وتحدّث المؤشرات واللوحة الجانبية والرسوم.
   - شريط زمني يعيد تشغيل حركة الطلبات يومًا بيوم.
   البيانات: window.JEEM_WORKFLOW (assets/js/workflow-data.js)
   ========================================================== */
(function () {
  const W = window.JEEM_WORKFLOW, JR = window.JR, root = document.getElementById('wfRoot');
  if (!W || !root) return;
  const $ = (s, el = root) => el.querySelector(s), $$ = (s, el = root) => [...el.querySelectorAll(s)];
  const esc = s => String(s).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
  const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
  const nf = (v, d = 0) => Number(v).toLocaleString('en-US', { maximumFractionDigits: d, minimumFractionDigits: d });
  const sar = v => v >= 1e6 ? nf(v / 1e6, 1) + ' م' : nf(v / 1e3) + ' ألف';
  const NS = 'http://www.w3.org/2000/svg', mk = (t, a = {}) => { const e = document.createElementNS(NS, t); for (const k in a) e.setAttribute(k, a[k]); return e; };
  const reduceMotion = matchMedia('(prefers-reduced-motion: reduce)').matches;
  const ST = Object.fromEntries(W.stages.map(s => [s.id, s])), TERM = Object.fromEntries(W.terminals.map(t => [t.id, t]));
  const CL = Object.fromEntries(W.clients.map(c => [c.id, c])), SU = Object.fromEntries(W.suppliers.map(s => [s.id, s]));
  const RQ = Object.fromEntries(W.requests.map(r => [r.id, r]));
  const STATUS = { ok: { n: 'ضمن المدة', icon: 'check', shape: 'circle' }, warn: { n: 'معرض للتأخر', icon: 'clock', shape: 'square' }, bad: { n: 'متجاوز للمدة', icon: 'x', shape: 'diamond' } };
  const dayLabel = d => { const m = Math.min(8, Math.floor(d / 30.4)); return `${Math.round(d - m * 30.4) + 1} ${W.months[m]}`; };
  const CW = 168, CH = 112;
  /* جمع العدد العربي: 3–10 بالجمع، وغير ذلك بالمفرد المنصوب */
  const pl = (n, few, many, one, two) => n === 1 && one ? one : n === 2 && two ? two : `${n} ${n >= 3 && n <= 10 ? few : many}`;

  /* ---------- الحالة ---------- */
  const S = { t: W.today, sel: null, hl: null, client: 'all', on: new Set(['ok', 'warn', 'bad']), view: 'map' };

  /* حالة كل طلب في يوم معيّن */
  function at(r, t) {
    if (t < r.start) return { phase: 'future' };
    let from = r.start; const segs = [];
    for (const [s, d] of r.visits) { segs.push({ s, from, to: from + d, d }); from += d; }
    const cur = segs.find(g => t < g.to);
    const slaSum = segs.filter(g => g.from <= t).reduce((a, g) => a + ST[g.s].sla, 0);
    if (!cur) return { phase: 'end', end: r.end, finishedAt: from, segs, total: from - r.start, slaSum, onTime: from - r.start <= slaSum };
    const inDays = t - cur.from, ratio = inDays / ST[cur.s].sla;
    return { phase: 'active', stage: cur.s, inDays, ratio, st: ratio >= 1 ? 'bad' : ratio >= W.risk ? 'warn' : 'ok', segs: segs.filter(g => g.from <= t), age: t - r.start, slaSum };
  }
  const inScope = r => (S.client === 'all' || r.client === S.client);
  const now = () => W.requests.map(r => ({ r, a: at(r, S.t) }));
  const active = () => now().filter(x => x.a.phase === 'active' && inScope(x.r));
  const finished = () => now().filter(x => x.a.phase === 'end' && inScope(x.r));
  /* مدد المراحل المكتملة حتى اليوم t */
  const doneVisits = (filter = () => true) => { const out = []; W.requests.forEach(r => { if (!inScope(r) || !filter(r)) return; let f = r.start; r.visits.forEach(([s, d]) => { if (f + d <= S.t) out.push({ r, s, d }); f += d; }); }); return out; };
  /* أعداد الانتقالات بين المراحل حتى اليوم t */
  function flowCounts(filter = () => true) {
    const c = {}; const add = (a, b) => { c[a + '>' + b] = (c[a + '>' + b] || 0) + 1; };
    W.requests.forEach(r => { if (!inScope(r) || !filter(r)) return; let f = r.start; r.visits.forEach(([s, d], i) => { f += d; if (f > S.t) return; const nx = r.visits[i + 1]; if (nx) add(s, nx[0]); else add(s, r.end === 'done' ? 'DONE' : 'REJ'); }); });
    return c;
  }

  /* ---------- الرسم ---------- */
  const svg = $('#wfSvg');
  svg.setAttribute('viewBox', '0 0 1200 664');
  svg.innerHTML = `<defs><marker id="wfArrow" viewBox="0 0 10 10" refX="8" refY="5" markerWidth="10" markerHeight="10" markerUnits="userSpaceOnUse" orient="auto-start-reverse"><path d="M0,1 L9,5 L0,9z" fill="#9AA6B2"/></marker>
    <marker id="wfArrowOn" viewBox="0 0 10 10" refX="8" refY="5" markerWidth="12" markerHeight="12" markerUnits="userSpaceOnUse" orient="auto-start-reverse"><path d="M0,1 L9,5 L0,9z" fill="#127DBD"/></marker></defs>`;
  const L = n => { const g = mk('g', { class: 'wf-' + n }); svg.append(g); return g; };
  const gLinks = L('links'), gFlows = L('flows'), gPart = L('parts'), gNodes = L('nodes'), gDots = L('dots'), gLabels = L('labels');

  /* مسارات الانتقال */
  const box = id => ST[id] || TERM[id];
  const pathFor = f => {
    const A = box(f.a), B = box(f.b), row1 = y => y < 300;
    if (f.a === f.b) { const dx = f.a === 'S2' ? 22 : 0; return `M${A.x + dx + 34},${A.y - CH / 2} C${A.x + dx + 34},${A.y - CH / 2 - 62} ${A.x + dx - 34},${A.y - CH / 2 - 62} ${A.x + dx - 34},${A.y - CH / 2}`; }
    if (f.kind === 'rework') return `M${A.x + 40},${A.y - CH / 2} C${A.x + 40},${A.y - CH / 2 - 92} ${B.x - 60},${B.y - CH / 2 - 92} ${B.x - 60},${B.y - CH / 2}`;
    if (f.a === 'S5' && f.b === 'S6') return `M${A.x - CW / 2},${A.y} C${A.x - CW / 2 - 64},${A.y} ${B.x - CW / 2 - 64},${B.y} ${B.x - CW / 2},${B.y}`;
    if (f.kind === 'skip') return `M${A.x},${A.y + CH / 2} L${B.x},${B.y - CH / 2}`;
    if (f.b === 'REJ') return `M${A.x + (f.a === 'S4' ? 46 : -46)},${A.y + CH / 2} Q${(A.x + B.x) / 2 + (f.a === 'S4' ? 0 : 0)},${B.y - 6} ${B.x + (f.a === 'S4' ? -52 : 52)},${B.y}`;
    if (f.b === 'DONE') return `M${A.x + CW / 2},${A.y} L${B.x - 46},${B.y}`;
    return row1(A.y) ? `M${A.x - CW / 2},${A.y} L${B.x + CW / 2},${B.y}` : `M${A.x + CW / 2},${A.y} L${B.x - CW / 2},${B.y}`;
  };
  const F = W.flows.map(f => {
    const g = mk('g', { class: `wf-flow k-${f.kind}`, 'data-flow': f.a + '>' + f.b });
    const p = mk('path', { d: pathFor(f), class: 'wf-fp', 'marker-end': 'url(#wfArrow)' }), hit = mk('path', { d: pathFor(f), class: 'wf-fhit' });
    g.append(p, hit); gFlows.append(g);
    const len = p.getTotalLength(), mid = p.getPointAtLength(len / 2);
    const lab = mk('g', { class: `wf-flab k-${f.kind}`, transform: `translate(${mid.x.toFixed(1)},${(mid.y - (f.kind === 'main' ? 12 : 0)).toFixed(1)})` });
    lab.innerHTML = `<rect x="-26" y="-10" width="52" height="20" rx="10"/><text y="4"></text>`;
    gLabels.append(lab);
    return Object.assign({}, f, { g, p, hit, lab, len, parts: [] });
  });

  /* بطاقات المراحل */
  W.stages.forEach(s => {
    const g = mk('g', { class: 'wf-card', 'data-stage': s.id, tabindex: 0, role: 'button', transform: `translate(${s.x},${s.y})` });
    g.innerHTML = `<rect class="wf-cbg" x="${-CW / 2}" y="${-CH / 2}" width="${CW}" height="${CH}" rx="14"/>
      <rect class="wf-cbar" x="${-CW / 2}" y="${-CH / 2}" width="4" height="${CH}" rx="2"/>
      <text class="wf-cname" x="${CW / 2 - 12}" y="${-CH / 2 + 22}">${esc(s.n)}</text>
      <text class="wf-csla" x="${CW / 2 - 12}" y="${-CH / 2 + 40}"></text>
      <text class="wf-cwip" x="${-CW / 2 + 12}" y="${-CH / 2 + 32}"></text>
      <g class="wf-bullet" transform="translate(0,${-CH / 2 + 54})"><rect class="trk" x="-72" y="-3" width="144" height="6" rx="3"/><rect class="fill" x="0" y="-3" width="0" height="6" rx="3"/><line class="tick" x1="0" x2="0" y1="-6" y2="6"/></g>`;
    gNodes.append(g);
  });
  W.terminals.forEach(t => {
    const g = mk('g', { class: `wf-term tone-${t.tone}`, 'data-term': t.id, transform: `translate(${t.x},${t.y})` });
    g.innerHTML = `<rect x="-46" y="-22" width="92" height="44" rx="22"/><text class="n" y="-3">${t.n}</text><text class="c" y="13"></text>`;
    gNodes.append(g);
  });

  /* العملاء (مدخل) والموردون (أسفل) */
  const CX = 1112, cy = i => 64 + i * 66;
  W.clients.forEach((c, i) => {
    const g = mk('g', { class: 'wf-pill wf-client', 'data-client': c.id, tabindex: 0, role: 'button', transform: `translate(${CX},${cy(i)})` });
    g.innerHTML = `<rect x="-78" y="-24" width="156" height="48" rx="12"/><text class="n" x="66" y="-4">${esc(c.short)}</text><text class="m" x="66" y="13"></text>`;
    gNodes.append(g);
    const p = mk('path', { class: 'wf-link', 'data-client': c.id, d: `M${CX - 78},${cy(i)} C${CX - 112},${cy(i)} ${1080},${150} ${960 + CW / 2},${150}` });
    gLinks.append(p);
  });
  W.suppliers.forEach((v, i) => {
    const x = 1112 - i * 126, y = 596;
    const g = mk('g', { class: 'wf-pill wf-sup', 'data-sup': v.id, tabindex: 0, role: 'button', transform: `translate(${x},${y})` });
    g.innerHTML = `<rect x="-60" y="-22" width="120" height="44" rx="12"/><text class="n" x="50" y="-4">${esc(v.short)}</text><text class="m" x="50" y="12"></text>`;
    gNodes.append(g);
    const p = mk('path', { class: 'wf-link', 'data-sup': v.id, d: `M${x},${y - 22} C${x},${520} ${580},${510} ${580 + (i - 4) * 9},${410 + CH / 2}` });
    gLinks.append(p);
  });
  const lab2 = mk('text', { class: 'wf-side', x: 1112, y: 26 }); lab2.textContent = 'العملاء'; gLabels.append(lab2);
  const lab4 = mk('text', { class: 'wf-side wf-note', x: 600, y: 652 }); lab4.textContent = 'المدد بالأيام · الرقم الكبير في كل مرحلة = الطلبات قيد التنفيذ · سُمك الخط = عدد الانتقالات'; gLabels.append(lab4);
  const lab3 = mk('text', { class: 'wf-side', x: 1178, y: 552 }); lab3.textContent = 'الموردون ← التوريد'; gLabels.append(lab3);

  /* نقاط الطلبات: عنصر دائم لكل طلب يتحرك بين المواقع */
  const shape = st => st === 'warn' ? '<rect x="-4.2" y="-4.2" width="8.4" height="8.4" rx="1.5"/>' : st === 'bad' ? '<path d="M0,-5.6 L5.6,0 L0,5.6 L-5.6,0Z"/>' : '<circle r="4.6"/>';
  const D = {};
  W.requests.forEach(r => { const g = mk('g', { class: 'wf-dot', 'data-req': r.id, tabindex: -1 }); D[r.id] = g; gDots.append(g); });

  /* ---------- التحديث الكامل ---------- */
  function render() {
    const A = now(), act = A.filter(x => x.a.phase === 'active');
    const fc = flowCounts(), mx = Math.max(1, ...Object.values(fc));
    const focusReqs = selReqs();
    /* المراحل */
    W.stages.forEach(s => {
      const g = $(`[data-stage="${s.id}"]`, svg), here = act.filter(x => x.a.stage === s.id && inScope(x.r));
      const dv = doneVisits().filter(v => v.s === s.id), avg = dv.length ? dv.reduce((a, v) => a + v.d, 0) / dv.length : 0, ratio = avg / s.sla;
      const tone = !dv.length ? 'none' : ratio >= 1 ? 'bad' : ratio >= W.risk ? 'warn' : 'ok';
      g.setAttribute('class', `wf-card tone-${tone}${S.sel && S.sel.k === 'stage' && S.sel.id === s.id ? ' is-sel' : ''}`);
      $('.wf-csla', g).textContent = `المدة ${s.sla} · الفعلي ${dv.length ? nf(avg, 1) : '—'}`;
      $('.wf-cwip', g).textContent = here.length;
      const w = Math.min(2, ratio) / 2 * 144; const f = $('.wf-bullet .fill', g); f.setAttribute('x', (72 - w).toFixed(1)); f.setAttribute('width', w.toFixed(1));
      g.dataset.tip = `${s.n}|مدة الخدمة ${s.sla} يومًا · المتوسط الفعلي ${dv.length ? nf(avg, 1) : '—'} · ${here.length} قيد التنفيذ · ${here.filter(x => x.a.st === 'bad').length} متجاوز`;
      g.setAttribute('aria-label', `${s.n}: ${here.length} طلبات قيد التنفيذ`);
      /* مواضع النقاط داخل البطاقة: صفّان × 12 */
      here.sort((a, b) => b.a.ratio - a.a.ratio).forEach((x, i) => { x.slot = i < 24 ? [s.x + 66 - (i % 12) * 12, s.y + 22 + Math.floor(i / 12) * 14] : [s.x - 72, s.y + 36]; });
    });
    /* المحطات النهائية */
    const fin = A.filter(x => x.a.phase === 'end' && inScope(x.r));
    W.terminals.forEach(t => { const g = $(`[data-term="${t.id}"]`, svg), n = fin.filter(x => (x.a.end === 'done' ? 'DONE' : 'REJ') === t.id).length; $('.c', g).textContent = n + ' طلبًا'; });
    /* الانتقالات */
    const pathSet = selFlows();
    F.forEach(f => {
      const n = fc[f.a + '>' + f.b] || 0, on = pathSet && pathSet.has(f.a + '>' + f.b);
      f.p.style.strokeWidth = (1.4 + 7 * n / mx).toFixed(1);
      f.g.classList.toggle('is-zero', !n); f.g.classList.toggle('is-on', !!on); f.g.classList.toggle('is-dim', !!pathSet && !on);
      f.p.setAttribute('marker-end', on ? 'url(#wfArrowOn)' : 'url(#wfArrow)');
      $('text', f.lab).textContent = f.label ? `${f.label} ${n}` : n; f.lab.classList.toggle('is-zero', !n); f.lab.classList.toggle('is-dim', !!pathSet && !on);
      const tw = f.label ? 14 + (f.label.length + String(n).length) * 6.4 : 30; const rc = $('rect', f.lab); rc.setAttribute('x', -tw / 2); rc.setAttribute('width', tw);
      f.hit.dataset.tip = `${box(f.a).n} ← ${box(f.b).n}|${n} انتقالًا حتى ${dayLabel(S.t)}`;
      f.n = n;
    });
    /* العملاء والموردون */
    W.clients.forEach(c => {
      const g = $(`.wf-client[data-client="${c.id}"]`, svg), mine = act.filter(x => x.r.client === c.id), late = mine.filter(x => x.a.st === 'bad').length;
      $('.m', g).textContent = `${mine.length} مفتوح · ${late} متجاوز`;
      g.classList.toggle('is-sel', (S.sel && S.sel.k === 'client' && S.sel.id === c.id) || S.client === c.id);
      g.classList.toggle('is-dim', S.client !== 'all' && S.client !== c.id);
      g.dataset.tip = `${c.n}|${pl(mine.length, 'طلبات', 'طلبًا', 'طلب واحد', 'طلبان')} مفتوحة · ${late} متجاوزة للمدة`;
      const l = $(`.wf-link[data-client="${c.id}"]`, svg); l.style.strokeWidth = (1 + mine.length / 3).toFixed(1); l.classList.toggle('is-on', !!(S.sel && S.sel.k === 'client' && S.sel.id === c.id));
    });
    W.suppliers.forEach(v => {
      const g = $(`.wf-sup[data-sup="${v.id}"]`, svg), st = supStats(v.id);
      $('.m', g).textContent = st.n ? `التزام ${st.pct}% · ${st.active} يورّد` : `${st.active} يورّد`;
      g.classList.toggle('is-sel', !!(S.sel && S.sel.k === 'sup' && S.sel.id === v.id));
      g.classList.toggle('tone-bad', st.n > 0 && st.pct < 60); g.classList.toggle('tone-warn', st.n > 0 && st.pct >= 60 && st.pct < 80);
      g.dataset.tip = `${v.n}|${st.n ? `سلّم ${pl(st.n, 'طلبات', 'طلبًا', 'طلب واحد', 'طلبان')}، ${st.pct}% في الموعد` : 'لا تسليمات مكتملة بعد'} · ${st.active} قيد التوريد`;
      const l = $(`.wf-link[data-sup="${v.id}"]`, svg); l.style.strokeWidth = (1 + st.active).toFixed(1); l.classList.toggle('is-on', !!(S.sel && S.sel.k === 'sup' && S.sel.id === v.id));
    });
    /* النقاط */
    A.forEach(x => {
      const g = D[x.r.id], vis = x.a.phase === 'active' && inScope(x.r) && S.on.has(x.a.st);
      let pos;
      if (x.a.phase === 'active') pos = act.find(y => y.r.id === x.r.id).slot || [ST[x.a.stage].x, ST[x.a.stage].y];
      else if (x.a.phase === 'future') { const i = W.clients.findIndex(c => c.id === x.r.client); pos = [CX - 60, cy(i)]; }
      else { const t = TERM[x.a.end === 'done' ? 'DONE' : 'REJ']; pos = [t.x, t.y]; }
      g.style.transform = `translate(${pos[0].toFixed(1)}px,${pos[1].toFixed(1)}px)`;
      const st = x.a.st || 'ok';
      if (g.dataset.st !== st) { g.innerHTML = shape(st); g.dataset.st = st; }
      g.setAttribute('class', `wf-dot st-${st}${vis ? '' : ' is-gone'}${focusReqs ? (focusReqs.has(x.r.id) ? ' is-on' : ' is-dim') : ''}${S.sel && S.sel.k === 'req' && S.sel.id === x.r.id ? ' is-sel' : ''}`);
      g.setAttribute('tabindex', vis ? '0' : '-1');
      if (vis) { g.dataset.tip = `${x.r.code} · ${x.r.n}|${ST[x.a.stage].n}: ${nf(x.a.inDays, 1)} من ${ST[x.a.stage].sla} يوم · ${STATUS[x.a.st].n} · ${CL[x.r.client].short}`; g.setAttribute('aria-label', `${x.r.code} ${x.r.n}، ${STATUS[x.a.st].n}`); g.setAttribute('role', 'button'); }
      else { delete g.dataset.tip; g.removeAttribute('role'); }
    });
    svg.classList.toggle('has-focus', !!(focusReqs || pathSet));
    $$('[data-stage]', svg).forEach(g => g.classList.toggle('is-dim', !!(S.sel && S.sel.k === 'stage' && S.sel.id !== g.dataset.stage) || !!(S.sel && S.sel.k === 'req' && !(pathSet && [...pathSet].some(k => k.split('>').includes(g.dataset.stage))))));
    $('#wfDay').textContent = dayLabel(S.t);
    $('#pgScope').textContent = S.client === 'all' ? 'كل العملاء' : CL[S.client].n;
    $$('.pg-law button').forEach(b => b.setAttribute('aria-pressed', String(b.dataset.law === S.client)));
    kpis(); panel(); widgets(); if (S.view === 'table') table();
    particles(fc, mx);
  }

  /* الطلبات والمسارات المرتبطة بالتحديد الحالي */
  function selReqs() {
    if (S.hl) return S.hl;
    if (!S.sel) return null;
    const A = now();
    if (S.sel.k === 'stage') return new Set(A.filter(x => x.a.phase === 'active' && x.a.stage === S.sel.id).map(x => x.r.id));
    if (S.sel.k === 'client') return new Set(W.requests.filter(r => r.client === S.sel.id).map(r => r.id));
    if (S.sel.k === 'sup') return new Set(W.requests.filter(r => r.supplier === S.sel.id).map(r => r.id));
    if (S.sel.k === 'req') return new Set([S.sel.id]);
    return null;
  }
  function selFlows() {
    if (S.sel && S.sel.k === 'req') { const r = RQ[S.sel.id], a = at(r, S.t), out = new Set(); const segs = a.segs || []; segs.forEach((g, i) => { const nx = segs[i + 1]; if (nx) out.add(g.s + '>' + nx.s); }); if (a.phase === 'end') out.add(segs[segs.length - 1].s + '>' + (a.end === 'done' ? 'DONE' : 'REJ')); return out; }
    if (S.sel && S.sel.k === 'flow') return new Set([S.sel.id]);
    return null;
  }
  function supStats(id) {
    const mine = W.requests.filter(r => r.supplier === id && inScope(r)); let n = 0, ok = 0, active = 0, late = [];
    mine.forEach(r => { let f = r.start; r.visits.forEach(([s, d]) => { if (s === 'S8') { if (f + d <= S.t) { n++; if (d <= ST.S8.sla) ok++; else late.push({ r, d }); } else if (f <= S.t) active++; } f += d; }); });
    return { n, ok, active, pct: n ? Math.round(ok / n * 100) : 0, late, mine };
  }

  /* ---------- جسيمات تتحرك على الانتقالات (إحساس بالتدفق) ---------- */
  let praf = 0;
  function particles(fc, mx) {
    if (reduceMotion) return;
    F.forEach(f => { const want = f.n ? Math.min(4, Math.ceil(f.n / 7)) : 0; while (f.parts.length < want) { const c = mk('circle', { r: 2.6, class: 'wf-pt k-' + f.kind }); gPart.append(c); f.parts.push({ c, o: f.parts.length / Math.max(1, want) }); } while (f.parts.length > want) f.parts.pop().c.remove(); });
    if (!praf) praf = requestAnimationFrame(stepParts);
  }
  function stepParts(ts) {
    F.forEach(f => f.parts.forEach(p => { const u = ((ts / (f.kind === 'main' ? 2600 : 3400)) + p.o) % 1, pt = f.p.getPointAtLength(u * f.len); p.c.setAttribute('cx', pt.x.toFixed(1)); p.c.setAttribute('cy', pt.y.toFixed(1)); }));
    praf = document.hidden ? 0 : requestAnimationFrame(stepParts);
  }
  document.addEventListener('visibilitychange', () => { if (!document.hidden && !praf && !reduceMotion) praf = requestAnimationFrame(stepParts); });

  /* ---------- المؤشرات ---------- */
  function kpis() {
    const act = active().filter(x => S.on.has(x.a.st)), fin = finished(), done = fin.filter(x => x.a.end === 'done');
    const ok = act.filter(x => x.a.st === 'ok').length, bad = act.filter(x => x.a.st === 'bad').length;
    const cyc = done.length ? done.reduce((a, x) => a + x.a.total, 0) / done.length : 0, onTime = done.length ? Math.round(done.filter(x => x.a.onTime).length / done.length * 100) : 0;
    const per = W.stages.map(s => { const dv = doneVisits().filter(v => v.s === s.id); return { s, r: dv.length ? dv.reduce((a, v) => a + v.d, 0) / dv.length / s.sla : 0 }; }).sort((a, b) => b.r - a.r)[0];
    const rew = W.requests.filter(r => inScope(r) && r.start <= S.t), rw = rew.filter(r => new Set(r.visits.map(v => v[0])).size < r.visits.length).length;
    const w = [
      { t: 'kpi', ti: 'قيد التنفيذ', v: String(act.length), d: '', n: `${ok} ضمن المدة`, ic: 'layers' },
      { t: 'kpi', ti: 'متجاوزة للمدة الآن', v: String(bad), d: '+3', g: false, n: act.length ? `${Math.round(bad / act.length * 100)}% من المفتوحة` : '', ic: 'bell', hl: 1 },
      { t: 'kpi', ti: 'المكتملة في الموعد', v: onTime + '%', d: '+4 نقاط', g: true, n: `من ${done.length} طلبًا مكتملًا`, ic: 'check' },
      { t: 'kpi', ti: 'متوسط زمن الدورة', v: nf(cyc), u: 'يومًا', d: '-6', g: true, n: 'من الاستلام للإغلاق', ic: 'clock' },
      { t: 'kpi', ti: 'عنق الزجاجة', v: per.s.n, d: '', n: `الفعلي ${nf(per.r * 100)}% من المدة`, ic: 'filter' },
      { t: 'kpi', ti: 'نسبة الإعادة', v: (rew.length ? Math.round(rw / rew.length * 100) : 0) + '%', d: '-2 نقطة', g: true, n: 'طلبات أُعيدت لمرحلة سابقة', ic: 'history' }
    ];
    $('#pgKpis').innerHTML = JR.renderPage({ id: 'wf' }, { n: 'k', w }, S.client + S.t);
  }

  /* ---------- اللوحة الجانبية ---------- */
  const statusTag = st => `<span class="wf-st st-${st}">${ic(STATUS[st].icon)}${STATUS[st].n}</span>`;
  const bar = (v, sla) => { const w = Math.min(2, v / sla) / 2 * 100, st = v >= sla ? 'bad' : v / sla >= W.risk ? 'warn' : 'ok'; return `<span class="wf-bar st-${st}"><i style="width:${w.toFixed(0)}%"></i><b></b></span>`; };
  const reqRow = x => `<li><button type="button" data-req="${x.r.id}"><span class="wf-ico st-${x.a.st}">${shape(x.a.st).replace('<', '<svg viewBox="-7 -7 14 14" width="12" height="12"><').replace(/\/>$/, '/></svg>')}</span><span class="t"><b>${esc(x.r.n)}</b><small>${x.r.code} · ${CL[x.r.client].short}</small></span>${bar(x.a.inDays, ST[x.a.stage].sla)}<em class="num">${nf(x.a.inDays, 1)}/${ST[x.a.stage].sla}</em></button></li>`;
  function panel() {
    const P = $('#pgPanel'), act = active();
    if (!S.sel) {
      const late = act.filter(x => x.a.st === 'bad').sort((a, b) => b.a.ratio - a.a.ratio).slice(0, 7);
      P.innerHTML = `<div class="pg-p-head"><span class="pg-p-kicker">${ic('info')}حتى ${dayLabel(S.t)}</span><h2>${act.length} طلبًا في سير العمل</h2><p>اضغط على مرحلة أو طلب (نقطة) أو عميل أو مورد لترى تفاصيله ومساره.</p></div>
        <div class="wf-legend">${Object.keys(STATUS).map(k => `<span>${statusTag(k)}<b class="num">${act.filter(x => x.a.st === k).length}</b></span>`).join('')}</div>
        <h3 class="pg-p-sec">الأكثر تجاوزًا للمدة الآن</h3><ul class="wf-reqs">${late.map(reqRow).join('') || '<li class="muted">لا توجد طلبات متجاوزة.</li>'}</ul>`;
      return;
    }
    const k = S.sel.k;
    const head = (kick, title, sub) => `<div class="pg-p-head"><button type="button" class="btn btn-ghost btn-sm pg-back" data-clear>${ic('chev-left')}كل سير العمل</button><span class="pg-p-kicker">${kick}</span><h2>${esc(title)}</h2>${sub ? `<p>${sub}</p>` : ''}</div>`;
    if (k === 'stage') {
      const s = ST[S.sel.id], here = act.filter(x => x.a.stage === s.id).sort((a, b) => b.a.ratio - a.a.ratio), dv = doneVisits().filter(v => v.s === s.id).map(v => v.d).sort((a, b) => a - b);
      const avg = dv.length ? dv.reduce((a, b) => a + b, 0) / dv.length : 0, p90 = dv.length ? dv[Math.floor(dv.length * .9)] : 0, br = dv.length ? Math.round(dv.filter(d => d > s.sla).length / dv.length * 100) : 0;
      P.innerHTML = head(`${ic('layers')}مرحلة`, s.n, `مدة الخدمة المستهدفة: ${s.sla} ${s.sla > 2 && s.sla < 11 ? 'أيام' : 'يوم'}`) +
        `<div class="pg-stats"><div><small>قيد التنفيذ</small><b class="num">${here.length}</b></div><div><small>متوسط فعلي</small><b class="num">${nf(avg, 1)}</b></div><div><small>أطول 10%</small><b class="num">${nf(p90, 1)}</b></div><div><small>تجاوز المدة</small><b class="num">${br}%</b></div></div>
        <h3 class="pg-p-sec">أسباب التأخير الشائعة</h3><ul class="wf-why">${s.why.map(w => `<li>${esc(w)}</li>`).join('')}</ul>
        <h3 class="pg-p-sec">الطلبات في المرحلة (الأيام / المدة)</h3><ul class="wf-reqs">${here.map(reqRow).join('') || '<li class="muted">لا توجد طلبات في هذه المرحلة الآن.</li>'}</ul>`;
    } else if (k === 'req') {
      const r = RQ[S.sel.id], a = at(r, S.t), segs = a.segs || [];
      const stTxt = a.phase === 'active' ? statusTag(a.st) : a.phase === 'end' ? `<span class="wf-st st-${a.end === 'done' ? (a.onTime ? 'ok' : 'bad') : 'bad'}">${ic(a.end === 'done' ? 'check' : 'x')}${a.end === 'done' ? (a.onTime ? 'اكتمل في الموعد' : 'اكتمل متأخرًا') : 'مرفوض أو ملغى'}</span>` : '';
      P.innerHTML = head(`${ic('file')}طلب شراء · ${r.code}`, r.n, `${CL[r.client].n} · ${sar(r.value)} ر.س · ${r.pri}${r.supplier ? ' · المورد: ' + SU[r.supplier].short : ''}`) +
        `<div class="wf-rstat">${stTxt}<span class="num">عمر الطلب ${nf(a.phase === 'end' ? a.total : a.age || 0)} يومًا مقابل ${a.slaSum || 0} مستهدف</span></div>
        <h3 class="pg-p-sec">مسار الطلب</h3><ol class="wf-journey">${segs.map((g, i) => { const cur = a.phase === 'active' && i === segs.length - 1, d = cur ? a.inDays : g.d, rep = segs.slice(0, i).some(x => x.s === g.s);
          return `<li class="${cur ? 'is-cur' : ''}"><span class="dot"></span><span class="t"><b>${ST[g.s].n}${rep ? ' <em>(إعادة)</em>' : ''}</b><small>${dayLabel(g.from)}${cur ? ' · الآن' : ''}</small></span>${bar(d, ST[g.s].sla)}<em class="num">${nf(d, 1)}/${ST[g.s].sla}</em></li>`; }).join('')}
          ${a.phase === 'end' ? `<li class="end"><span class="dot"></span><span class="t"><b>${a.end === 'done' ? 'مكتمل' : 'مرفوض أو ملغى'}</b><small>${dayLabel(a.finishedAt)}</small></span></li>` : ''}</ol>
        <div class="pg-p-actions"><button type="button" class="btn btn-sm" data-ask-req>${ic('spark')}اسأل جيم عنه</button><button type="button" class="btn btn-sm btn-primary" data-pin>${ic('bell')}تنبيه المسؤول</button></div>`;
    } else if (k === 'client') {
      const c = CL[S.sel.id], mine = act.filter(x => x.r.client === c.id).sort((a, b) => b.a.ratio - a.a.ratio), fin = finished().filter(x => x.r.client === c.id && x.a.end === 'done');
      const on = fin.length ? Math.round(fin.filter(x => x.a.onTime).length / fin.length * 100) : 0;
      P.innerHTML = head(`${ic('users')}عميل`, c.n, '') + `<div class="pg-stats"><div><small>مفتوحة</small><b class="num">${mine.length}</b></div><div><small>متجاوزة</small><b class="num">${mine.filter(x => x.a.st === 'bad').length}</b></div><div><small>مكتملة في الموعد</small><b class="num">${on}%</b></div></div>
        <h3 class="pg-p-sec">طلباته المفتوحة</h3><ul class="wf-reqs">${mine.map(reqRow).join('') || '<li class="muted">لا توجد طلبات مفتوحة.</li>'}</ul>`;
    } else if (k === 'sup') {
      const v = SU[S.sel.id], st = supStats(v.id), act8 = act.filter(x => x.r.supplier === v.id && x.a.stage === 'S8');
      P.innerHTML = head(`${ic('briefcase')}مورد`, v.n, `مدة التوريد المستهدفة ${ST.S8.sla} يومًا`) + `<div class="pg-stats"><div><small>قيد التوريد</small><b class="num">${st.active}</b></div><div><small>تسليمات مكتملة</small><b class="num">${st.n}</b></div><div><small>في الموعد</small><b class="num">${st.pct}%</b></div></div>
        <h3 class="pg-p-sec">قيد التوريد الآن</h3><ul class="wf-reqs">${act8.map(reqRow).join('') || '<li class="muted">لا توجد توريدات جارية.</li>'}</ul>
        <h3 class="pg-p-sec">تسليمات متأخرة سابقة</h3><ul class="wf-why">${st.late.map(l => `<li><button type="button" class="link" data-req="${l.r.id}">${esc(l.r.n)}</button> · ${nf(l.d)} يومًا</li>`).join('') || '<li class="muted">لا يوجد.</li>'}</ul>`;
    } else if (k === 'flow') {
      const [a, b] = S.sel.id.split('>'), f = F.find(x => x.a === a && x.b === b), list = W.requests.filter(r => inScope(r) && r.visits.some((v, i) => v[0] === a && r.visits[i + 1] && r.visits[i + 1][0] === b));
      P.innerHTML = head(`${ic('history')}انتقال`, `${box(a).n} ← ${box(b).n}`, f && f.label ? f.label : '') + `<div class="pg-stats"><div><small>عدد الانتقالات</small><b class="num">${f ? f.n : 0}</b></div><div><small>طلبات</small><b class="num">${list.length}</b></div></div>
        <h3 class="pg-p-sec">الطلبات التي سلكت هذا الانتقال</h3><ul class="wf-why">${list.slice(0, 12).map(r => `<li><button type="button" class="link" data-req="${r.id}">${esc(r.n)}</button> · ${CL[r.client].short}</li>`).join('')}</ul>`;
    }
  }

  /* ---------- الرسوم أسفل الخريطة (أرقام فعلية من البيانات) ---------- */
  function widgets() {
    const act = active(), clients = W.clients.filter(c => S.client === 'all' || c.id === S.client);
    const per = W.stages.map(s => { const dv = doneVisits().filter(v => v.s === s.id); return { s, avg: dv.length ? dv.reduce((a, v) => a + v.d, 0) / dv.length : 0 }; });
    const late = act.filter(x => x.a.st === 'bad').sort((a, b) => b.a.ratio - a.a.ratio).slice(0, 8);
    const sups = W.suppliers.map(v => ({ v, st: supStats(v.id) })).filter(x => x.st.n).sort((a, b) => a.st.pct - b.st.pct);
    const w = [
      { t: 'bullet', ti: 'الوقت الفعلي مقابل مدة الخدمة لكل مرحلة', su: `متوسط الأيام للمراحل المكتملة حتى ${dayLabel(S.t)} · الخط = المدة المستهدفة`, it: per.map(p => [p.s.n, p.avg, p.s.sla, p.s.id]), risk: W.risk, sp: 6 },
      { t: 'hbars', ti: 'التزام الموردين بمدة التوريد', su: `نسبة التسليمات خلال ${ST.S8.sla} يومًا`, x: sups.length ? sups.map(x => x.v.short) : ['لا تسليمات بعد'], vals: sups.length ? sups.map(x => x.st.pct) : [0], k: 'pct', ns: 1, tg: 80, sp: 6 },
      { t: 'hbars', ti: 'الطلبات المتجاوزة للمدة حسب العميل', su: 'المفتوحة الآن', x: clients.map(c => c.short), vals: clients.map(c => act.filter(x => x.r.client === c.id && x.a.st === 'bad').length), ns: 1, sp: 4 },
      late.length
        ? { t: 'table', ti: 'الطلبات المتجاوزة للمدة الآن', su: 'مرتبة بنسبة التجاوز', rows: late.map(x => x.r.n), c: [['الطلب', 'l'], ['الرقم', 'id'], ['العميل', 'l'], ['المرحلة', 'l'], ['الأيام', 'n'], ['المدة', 'n'], ['الحالة', 'st']], sts: [[STATUS.bad.n, 'bad']], vals: late.map(x => [null, x.r.code, CL[x.r.client].short, ST[x.a.stage].n, nf(x.a.inDays, 1), ST[x.a.stage].sla, STATUS.bad.n]), n: late.length, sp: 8 }
        : { t: 'note', ti: 'الطلبات المتجاوزة للمدة الآن', tx: ['لا توجد طلبات متجاوزة ضمن الفلاتر الحالية.'], sp: 8 }
    ];
    $('#pgGrid').innerHTML = JR.renderPage({ id: 'wf-w' }, { n: 'w', w }, S.client + S.t);
  }

  /* ---------- عرض كجدول ---------- */
  function table() {
    const L = active().filter(x => S.on.has(x.a.st)).sort((a, b) => b.a.ratio - a.a.ratio);
    $('#pgTable').innerHTML = `<table class="table"><caption class="sr-only">الطلبات قيد التنفيذ</caption><thead><tr><th>الرقم</th><th>الطلب</th><th>العميل</th><th>المرحلة الحالية</th><th>الأيام في المرحلة</th><th>المدة</th><th>الحالة</th><th>عمر الطلب</th></tr></thead><tbody>${L.map(x => `<tr${S.sel && S.sel.id === x.r.id ? ' class="is-sel"' : ''}><td><span class="num mono">${x.r.code}</span></td><td><button type="button" class="link" data-req="${x.r.id}">${esc(x.r.n)}</button></td><td>${CL[x.r.client].short}</td><td>${ST[x.a.stage].n}</td><td><span class="num">${nf(x.a.inDays, 1)}</span></td><td><span class="num">${ST[x.a.stage].sla}</span></td><td>${statusTag(x.a.st)}</td><td><span class="num">${nf(x.a.age)}</span></td></tr>`).join('')}</tbody></table>`;
  }
  $$('.pg-view button').forEach(b => b.addEventListener('click', () => {
    S.view = b.dataset.view; $$('.pg-view button').forEach(x => x.setAttribute('aria-pressed', String(x === b)));
    $('#pgTable').hidden = S.view !== 'table'; svg.style.visibility = S.view === 'table' ? 'hidden' : ''; if (S.view === 'table') table();
  }));

  /* ---------- التفاعل ---------- */
  const select = sel => { S.sel = sel; S.hl = null; $('#pgAnswer').hidden = true; render(); };
  svg.addEventListener('click', e => {
    const d = e.target.closest('[data-req]'), st = e.target.closest('[data-stage]'), c = e.target.closest('.wf-client'), v = e.target.closest('.wf-sup'), f = e.target.closest('[data-flow]');
    if (d && !d.classList.contains('is-gone')) return select({ k: 'req', id: d.dataset.req });
    if (st) return select(S.sel && S.sel.k === 'stage' && S.sel.id === st.dataset.stage ? null : { k: 'stage', id: st.dataset.stage });
    if (c) return select({ k: 'client', id: c.dataset.client });
    if (v) return select({ k: 'sup', id: v.dataset.sup });
    if (f) return select({ k: 'flow', id: f.dataset.flow });
    select(null);
  });
  svg.addEventListener('keydown', e => { if (e.key !== 'Enter' && e.key !== ' ') return; const t = e.target; if (t.matches('[data-req],[data-stage],.wf-client,.wf-sup')) { e.preventDefault(); t.dispatchEvent(new MouseEvent('click', { bubbles: true })); } });
  root.addEventListener('click', e => {
    const r = e.target.closest('#pgPanel [data-req], #pgTable [data-req], #pgGrid [data-req]'); if (r) return select({ k: 'req', id: r.dataset.req });
    const sg = e.target.closest('[data-stage-go]'); if (sg) return select({ k: 'stage', id: sg.dataset.stageGo });
    if (e.target.closest('[data-clear]')) return select(null);
    if (e.target.closest('[data-ask-req]')) return ask(`لماذا تأخر «${RQ[S.sel.id].n}»؟`);
    if (e.target.closest('[data-pin]')) toast(`أُرسل تنبيه لمسؤول المرحلة بخصوص ${RQ[S.sel.id].code}.`);
  });
  root.addEventListener('keydown', e => { if (e.key === 'Escape') select(null); });
  const toast = msg => { let t = document.querySelector('.toast'); if (!t) { t = document.createElement('div'); t.className = 'toast'; t.setAttribute('role', 'status'); document.body.append(t); } t.innerHTML = `${ic('check')}<span>${esc(msg)}</span>`; requestAnimationFrame(() => t.classList.add('is-on')); clearTimeout(t._h); t._h = setTimeout(() => t.classList.remove('is-on'), 2600); };

  /* فلاتر الحالة */
  $('#pgTypes').innerHTML = Object.keys(STATUS).map(k => `<button type="button" class="pg-type wf-stchip st-${k}" data-st="${k}" aria-pressed="true">${ic(STATUS[k].icon)}<span>${STATUS[k].n}</span></button>`).join('');
  $$('[data-st]').forEach(b => b.addEventListener('click', () => { const k = b.dataset.st; if (S.on.has(k)) { if (S.on.size === 1) return; S.on.delete(k); } else S.on.add(k); b.setAttribute('aria-pressed', String(S.on.has(k))); render(); }));
  /* العملاء */
  $('.pg-law').innerHTML = `<button type="button" data-law="all" aria-pressed="true">كل العملاء</button>` + W.clients.map(c => `<button type="button" data-law="${c.id}" aria-pressed="false">${c.short}</button>`).join('');
  $$('.pg-law button').forEach(b => b.addEventListener('click', () => { S.client = b.dataset.law; S.sel = null; S.hl = null; render(); }));

  /* الشريط الزمني */
  const yr = $('#pgYear'), play = $('#pgPlay');
  yr.min = 0; yr.max = W.today; yr.value = W.today;
  $('.pg-ticks').innerHTML = W.months.map(m => `<i>${m.slice(0, 3)}</i>`).join('');
  yr.addEventListener('input', () => { stopPlay(); S.t = +yr.value; render(); });
  let timer = null;
  const stopPlay = () => { clearInterval(timer); timer = null; play.innerHTML = ic('play'); play.setAttribute('aria-label', 'تشغيل حركة الطلبات يومًا بيوم'); };
  play.addEventListener('click', () => {
    if (timer) return stopPlay();
    play.innerHTML = ic('pause'); play.setAttribute('aria-label', 'إيقاف التشغيل');
    if (S.t >= W.today) S.t = 0;
    timer = setInterval(() => { S.t = Math.min(W.today, S.t + 3); yr.value = S.t; render(); if (S.t >= W.today) stopPlay(); }, 160);
  });
  stopPlay();

  /* البحث */
  const norm = s => s.replace(/[أإآ]/g, 'ا').replace(/ة/g, 'ه').replace(/ى/g, 'ي').trim();
  const search = $('#pgSearch'), sug = $('#pgSug');
  search.addEventListener('input', () => {
    const q = norm(search.value); if (!q) { sug.hidden = true; return; }
    const L = W.requests.filter(r => norm(r.n + ' ' + r.code).includes(q)).slice(0, 6).map(r => `<li><button type="button" data-req="${r.id}">${ic('file')}<span>${esc(r.n)}</span><small>${r.code}</small></button></li>`)
      .concat(W.suppliers.filter(v => norm(v.n).includes(q)).map(v => `<li><button type="button" data-supgo="${v.id}">${ic('briefcase')}<span>${esc(v.n)}</span><small>مورد</small></button></li>`));
    sug.innerHTML = L.join('') || '<li class="muted">لا نتائج</li>'; sug.hidden = false;
  });
  sug.addEventListener('click', e => { const b = e.target.closest('[data-supgo]'); if (b) select({ k: 'sup', id: b.dataset.supgo }); sug.hidden = true; search.value = ''; });
  document.addEventListener('click', e => { if (!e.target.closest('.pg-search')) sug.hidden = true; });

  /* اسأل جيم */
  const askIn = $('#pgAskIn'), ans = $('#pgAnswer');
  $('#pgAskChips').innerHTML = W.asks.map((a, i) => `<button type="button" class="chip" data-ask="${i}">${esc(a.q)}</button>`).join('');
  $$('[data-ask]').forEach(b => b.addEventListener('click', () => ask(W.asks[+b.dataset.ask].q)));
  $('#pgAsk').addEventListener('submit', e => { e.preventDefault(); if (askIn.value.trim()) ask(askIn.value.trim()); });
  let typing = null;
  function say(txt) { ans.hidden = false; const el = $('.pg-ans-txt', ans); clearInterval(typing); if (reduceMotion) { el.textContent = txt; return; } let i = 0; el.textContent = ''; ans.classList.add('typing'); typing = setInterval(() => { i += 3; el.textContent = txt.slice(0, i); if (i >= txt.length) { clearInterval(typing); ans.classList.remove('typing'); } }, 18); }
  function ask(q) {
    askIn.value = q; const nq = norm(q);
    let a = W.asks.find(x => x.q === q);
    if (!a) a = /عنق|اختناق|ابطا|أبطأ/.test(nq) ? W.asks[0] : /متجاوز|متاخر|تاخر|sla/i.test(nq) && !/مورد/.test(nq) ? W.asks[1] : /اعيد|اعاده|مواصفات/.test(nq) ? W.asks[2] : /مورد|توريد/.test(nq) ? W.asks[4] : /عميل|عملاء/.test(nq) ? W.asks[5] : null;
    const act = active();
    if (S.sel && S.sel.k === 'req' && /لماذا|ليه/.test(nq)) {
      const r = RQ[S.sel.id], x = at(r, S.t), worst = (x.segs || []).map((g, i) => ({ g, d: x.phase === 'active' && i === x.segs.length - 1 ? x.inDays : g.d })).sort((p, q2) => q2.d / ST[q2.g.s].sla - p.d / ST[p.g.s].sla)[0];
      const rep = (x.segs || []).length - new Set((x.segs || []).map(g => g.s)).size;
      return say(worst ? `أطول تأخير كان في «${ST[worst.g.s].n}»: ${nf(worst.d, 1)} يومًا مقابل ${ST[worst.g.s].sla} مستهدفة${rep ? `، وأُعيد الطلب ${rep} مرة لمرحلة سابقة` : ''}. السبب الشائع في هذه المرحلة: ${ST[worst.g.s].why[0]}.` : 'لم يبدأ هذا الطلب بعد.');
    }
    if (!a) { S.sel = null; S.hl = new Set(W.requests.filter(r => norm(r.n).split(' ').some(w => w.length > 3 && nq.includes(w))).map(r => r.id)); render(); return say(S.hl.size ? `وجدت ${pl(S.hl.size, 'طلبات', 'طلبًا', 'طلب واحد', 'طلبان')} مطابقة، ظاهرة الآن على الخريطة.` : 'لم أجد طلبات مطابقة. جرّب أحد الأسئلة المقترحة.'); }
    if (a.kind === 'stage') {
      const per = W.stages.map(s => { const dv = doneVisits().filter(v => v.s === s.id); return { s, avg: dv.length ? dv.reduce((x, v) => x + v.d, 0) / dv.length : 0, n: act.filter(x => x.a.stage === s.id).length }; }).sort((p, q2) => q2.avg / q2.s.sla - p.avg / p.s.sla)[0];
      select({ k: 'stage', id: per.s.id });
      say(`عنق الزجاجة هو «${per.s.n}»: المتوسط الفعلي ${nf(per.avg, 1)} يومًا مقابل ${per.s.sla} مستهدفة (${nf(per.avg / per.s.sla * 100)}%)، وفيها ${pl(per.n, 'طلبات', 'طلبًا', 'طلب واحد', 'طلبان')} الآن. أكثر الأسباب: ${per.s.why[0]}.`);
    } else if (a.kind === 'sla') {
      const late = act.filter(x => x.a.st === 'bad'); S.sel = null; S.hl = new Set(late.map(x => x.r.id)); render();
      const byStage = {}; late.forEach(x => byStage[x.a.stage] = (byStage[x.a.stage] || 0) + 1); const top = Object.entries(byStage).sort((p, q2) => q2[1] - p[1])[0];
      say(late.length ? `${pl(late.length, 'طلبات', 'طلبًا', 'طلب واحد', 'طلبان')} متجاوزة لمدة مرحلتها الآن بقيمة ${sar(late.reduce((s, x) => s + x.r.value, 0))} ر.س، أكثرها في «${ST[top[0]].n}» (${top[1]}). أكبرها تجاوزًا: «${late.sort((p, q2) => q2.a.ratio - p.a.ratio)[0].r.n}».` : 'لا توجد طلبات متجاوزة الآن.');
    } else if (a.kind === 'flow') {
      select({ k: 'flow', id: a.a + '>' + a.b }); const f = F.find(x => x.a === a.a && x.b === a.b), rw = F.filter(x => x.kind === 'rework').reduce((s, x) => s + x.n, 0);
      say(`أُعيد ${pl(f.n, 'طلبات', 'طلبًا', 'طلب واحد', 'طلبان')} من الدراسة الفنية للعميل لاستكمال المواصفات، ومجموع الإعادات في كل المراحل ${rw}. كل إعادة تضيف في المتوسط ${nf(ST.S2.sla * .9 + ST.S3.sla * .6, 0)} يومًا لعمر الطلب.`);
    } else if (a.kind === 'oldest') {
      const o = act.sort((p, q2) => q2.a.age - p.a.age)[0]; if (!o) return say('لا توجد طلبات مفتوحة.');
      select({ k: 'req', id: o.r.id });
      say(`أقدم طلب مفتوح «${o.r.n}» (${o.r.code}) عمره ${nf(o.a.age)} يومًا، مرّ بـ${pl(o.a.segs.length, 'مراحل', 'مرحلة')} وهو الآن في «${ST[o.a.stage].n}» منذ ${nf(o.a.inDays, 1)} يومًا.`);
    } else if (a.kind === 'supplier') {
      const L = W.suppliers.map(v => ({ v, s: supStats(v.id) })).filter(x => x.s.n).sort((p, q2) => p.s.pct - q2.s.pct), w0 = L[0];
      if (!w0) return say('لا توجد تسليمات مكتملة بعد.');
      select({ k: 'sup', id: w0.v.id });
      say(`الأقل التزامًا «${w0.v.n}»: ${w0.s.pct}% فقط من تسليماته خلال ${ST.S8.sla} يومًا، ولديه ${pl(w0.s.active, 'توريدات', 'توريدًا', 'توريد واحد', 'توريدان')} جارية. يليه «${L[1] ? L[1].v.short : '—'}» (${L[1] ? L[1].s.pct : 0}%).`);
    } else if (a.kind === 'client') {
      const L = W.clients.map(c => ({ c, n: act.filter(x => x.r.client === c.id && x.a.st === 'bad').length, o: act.filter(x => x.r.client === c.id).length })).sort((p, q2) => q2.n - p.n), c0 = L[0];
      select({ k: 'client', id: c0.c.id });
      say(`«${c0.c.n}» الأكثر تأثرًا: ${c0.n} من ${pl(c0.o, 'طلبات', 'طلبًا', 'طلب واحد', 'طلبان')} مفتوحة متجاوزة للمدة. يليه «${L[1].c.short}» (${L[1].n} من ${L[1].o}).`);
    }
  }
  $('#pgAnsClear').addEventListener('click', () => { S.hl = null; ans.hidden = true; select(null); });

  render();
})();
