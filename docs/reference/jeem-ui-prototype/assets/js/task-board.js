/* ==========================================================
   جيم — لوحة مهام المشاريع (لوحة تفاعلية)
   مستوحاة من «Project Management Dashboard» (Nexdo): أعمدة كانبان بالسحب والإفلات،
   شرائح مشاريع للتصفية، بحث وترتيب وأولوية، وإنشاء مهمة فوري.
   ========================================================== */
(function () {
  const root = document.getElementById('tbRoot'); if (!root || !window.DK) return;
  const K = DK, $ = s => root.querySelector(s);
  const COLS = [['todo', 'لم تبدأ', '#9AA5B1'], ['doing', 'قيد التنفيذ', K.PAL[0]], ['review', 'قيد المراجعة', K.PAL[1]], ['done', 'مكتملة', K.PAL[4]]];
  const PRI = { high: ['عالية', 'bad', 3], med: ['متوسطة', 'warn', 2], low: ['منخفضة', '', 1] };
  const PROJ = [['متجر سمكارت', '#7CB342'], ['تطبيق إنستا سبلاي', '#6E62B0'], ['لوحة كوبي', '#28B6E6']];
  const S = { proj: null, q: '', sort: 'due', pri: 'all', drag: null };
  let id = 0;
  const T = [['تصميم ويب', 'high', 'تصميم الصفحة الرئيسية', 'todo', 0, 0, 10], ['لوحة متابعة', 'med', 'إنشاء مسار تسجيل الدخول', 'todo', 0, 2, 14], ['تحسين محركات البحث', 'low', 'تدقيق الموقع لتحسين الظهور', 'todo', 0, 0, 20], ['تطبيق', 'med', 'كتابة مقال المدونة', 'doing', 20, 1, 9], ['تسويق', 'high', 'تخطيط حملة إنستغرام', 'doing', 65, 1, 12], ['تطوير العلامة', 'med', 'إنشاء إرشادات الهوية', 'doing', 90, 0, 8], ['تصميم جرافيك', 'low', 'إنشاء كتيّب تسويقي', 'review', 80, 2, 11], ['تحرير صوتي', 'med', 'تسجيل حلقة البودكاست', 'review', 75, 0, 13], ['تحرير فيديو', 'med', 'تحرير فيديو ترويجي للتطبيق', 'review', 95, 1, 7], ['بحث سوق', 'med', 'تحليل استراتيجيات المنافسين', 'done', 100, 1, 5], ['المنتج', 'low', 'جلسات ملاحظات المستخدمين', 'done', 100, 0, 6], ['تطوير العلامة', 'high', 'إرشادات الهوية النهائية', 'done', 100, 2, 4]]
    .map(([cat, pri, t, st, pg, pj, d]) => ({ id: id++, cat, pri, t, st, pg, pj, d, who: [id % 6, (id + 2) % 6, (id + 4) % 6].slice(0, 2 + id % 2) }));
  const day = d => new Date(2026, 9, d).toLocaleDateString('ar-SA-u-ca-gregory-nu-latn', { weekday: 'short', day: 'numeric', month: 'short' });

  function projs() {
    $('#tbProjs').innerHTML = PROJ.map(([n, c], i) => `<button type="button" class="tb-pj" data-pj="${i}" aria-pressed="${S.proj === i}"><i style="background:${c}"></i>${n}<span class="num">${T.filter(x => x.pj === i).length}</span></button>`).join('') + `<button type="button" class="tb-pj add" data-addpj>${K.ic('plus')}إضافة مشروع</button>`;
  }
  const vis = () => T.filter(x => (S.proj == null || x.pj === S.proj) && (S.pri === 'all' || x.pri === S.pri) && (!S.q || x.t.includes(S.q) || x.cat.includes(S.q)));
  const sorter = { due: (a, b) => a.d - b.d, pri: (a, b) => PRI[b.pri][2] - PRI[a.pri][2], prog: (a, b) => b.pg - a.pg };
  function card(x, i) {
    const col = COLS.find(c => c[0] === x.st)[2];
    return `<article class="tb-card" draggable="true" tabindex="0" data-id="${x.id}" style="--i:${i}" aria-label="${K.esc(x.t)} — ${COLS.find(c => c[0] === x.st)[1]}، اضغط السهم لنقلها">
      <header><span class="dk-chip">${K.esc(x.cat)}</span><span class="dk-chip ${PRI[x.pri][1]}"><i class="pd"></i>${PRI[x.pri][0]}</span></header>
      <b>${K.esc(x.t)}</b><small>الاستحقاق: ${day(x.d)}</small>
      <span class="dk-avs">${x.who.map(w => `<span class="dk-av" style="--h:${w * 60}">${'رفنسعم'[w]}</span>`).join('')}</span>
      <span class="pr"><span class="dk-prog"><i style="width:${x.pg}%;background:${col}"></i></span><span class="num">${x.pg}%</span></span>
      <footer><i style="background:${PROJ[x.pj][1]}"></i>${PROJ[x.pj][0]}</footer></article>`;
  }
  function board() {
    const v = vis().sort(sorter[S.sort]);
    $('#tbBoard').innerHTML = COLS.map(([k, n, c]) => { const cs = v.filter(x => x.st === k);
      return `<section class="tb-col" data-col="${k}" aria-label="${n}"><header><i style="background:${c}"></i><b>${n}</b><span class="dk-chip"><span class="num">${cs.length}</span></span></header>
        ${k === 'todo' && S.adding ? `<form class="tb-new" id="tbNew"><input name="t" required placeholder="عنوان المهمة" aria-label="عنوان المهمة"><select name="p" aria-label="الأولوية"><option value="high">عالية</option><option value="med" selected>متوسطة</option><option value="low">منخفضة</option></select><select name="j" aria-label="المشروع">${PROJ.map((p, i) => `<option value="${i}">${p[0]}</option>`).join('')}</select><span><button type="submit" class="dk-btn pri">إضافة</button><button type="button" class="dk-btn" data-cancel>إلغاء</button></span></form>` : ''}
        <div class="tb-list">${cs.map(card).join('') || '<p class="dk-empty">أفلت مهمة هنا</p>'}</div></section>`; }).join('');
    sum(); if (S.adding) $('#tbNew [name=t]').focus();
  }
  function sum() {
    const v = vis(), n = v.length || 1;
    $('#tbSum').innerHTML = `<span class="bar">${COLS.map(([k, nm, c]) => { const m = v.filter(x => x.st === k).length; return m ? `<i style="flex:${m};background:${c}" data-tip="${nm}|${m} مهام"></i>` : ''; }).join('')}</span><span>${COLS.map(([k, nm, c]) => `<span class="lg"><i style="background:${c}"></i>${nm} <b class="num">${v.filter(x => x.st === k).length}</b></span>`).join('')}</span><span class="mut">الإنجاز <b class="num">${Math.round(v.filter(x => x.st === 'done').length / n * 100)}%</b></span>`;
  }
  function move(tid, st, focus) { const x = T.find(t => t.id === tid); if (!x || x.st === st) return; x.st = st; if (st === 'done') x.pg = 100; else if (st === 'todo') x.pg = 0; else if (x.pg === 100 || x.pg === 0) x.pg = st === 'review' ? 80 : 30; board(); K.toast(`«${x.t}» ← ${COLS.find(c => c[0] === st)[1]}`); if (focus) root.querySelector(`[data-id="${tid}"]`)?.focus(); }

  root.addEventListener('dragstart', e => { const c = e.target.closest('.tb-card'); if (!c) return; S.drag = +c.dataset.id; c.classList.add('dragging'); e.dataTransfer.effectAllowed = 'move'; e.dataTransfer.setData('text/plain', c.dataset.id); });
  root.addEventListener('dragend', e => { e.target.closest?.('.tb-card')?.classList.remove('dragging'); root.querySelectorAll('.tb-col.over').forEach(c => c.classList.remove('over')); });
  root.addEventListener('dragover', e => { const c = e.target.closest('.tb-col'); if (!c || S.drag == null) return; e.preventDefault(); root.querySelectorAll('.tb-col.over').forEach(x => x !== c && x.classList.remove('over')); c.classList.add('over'); });
  root.addEventListener('drop', e => { const c = e.target.closest('.tb-col'); if (!c || S.drag == null) return; e.preventDefault(); move(S.drag, c.dataset.col); S.drag = null; });
  root.addEventListener('keydown', e => { const c = e.target.closest?.('.tb-card'); if (!c || !['ArrowLeft', 'ArrowRight'].includes(e.key)) return; e.preventDefault(); const x = T.find(t => t.id === +c.dataset.id), i = COLS.findIndex(k => k[0] === x.st), ni = i + (e.key === 'ArrowLeft' ? 1 : -1); if (ni >= 0 && ni < 4) move(x.id, COLS[ni][0], true); });
  root.addEventListener('click', e => {
    const t = e.target;
    const p = t.closest('[data-pj]'); if (p) { const i = +p.dataset.pj; S.proj = S.proj === i ? null : i; projs(); return board(); }
    if (t.closest('[data-addpj]')) { PROJ.push(['مشروع جديد ' + (PROJ.length - 2), K.PAL[(PROJ.length + 1) % 6]]); return projs(); }
    if (t.closest('[data-new]')) { S.adding = true; return board(); }
    if (t.closest('[data-cancel]')) { S.adding = false; return board(); }
  });
  root.addEventListener('submit', e => { if (e.target.id !== 'tbNew') return; e.preventDefault(); const f = new FormData(e.target); T.unshift({ id: id++, cat: 'جديد', pri: f.get('p'), t: f.get('t'), st: 'todo', pg: 0, pj: +f.get('j'), d: 15, who: [0] }); S.adding = false; projs(); board(); K.toast('أُنشئت المهمة'); });
  $('#tbQ').addEventListener('input', e => { S.q = e.target.value.trim(); board(); });
  $('#tbSort').addEventListener('change', e => { S.sort = e.target.value; board(); });
  K.seg(root, 'pri', v => { S.pri = v; board(); });
  projs(); board();
})();
