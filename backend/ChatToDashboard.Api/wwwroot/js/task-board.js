/* ==========================================================
   جيم — لوحة مهام المشاريع (لوحة تفاعلية، المحرك: board، على dash-kit)
   مُعاد بناؤه كدالة تركيب قابلة لإعادة الاستخدام (window.JEEM_BOARD_MOUNT) بدل IIFE ذاتي
   التشغيل — يقبل data حقيقية أو من مكتبة العرض التجريبي (task-board-demo.js). تواريخ
   الاستحقاق ISO حقيقية (لا يوم ثابت في شهر ثابت)؛ أحرف الأعضاء المختصرة في الصور الرمزية
   تُؤخذ من data.people الحقيقية بدل حرف عشوائي من نص ثابت. أعمدة الحالة الأربعة
   (لم تبدأ/قيد التنفيذ/قيد المراجعة/مكتملة) مفردات تصنيف ثابتة في المحرك، لا بيانات مشروع.
   السحب والإفلات، إضافة مشروع/مهمة كلها أفعال المستخدم الحالي، تُطبَّق على العرض لمدة الجلسة.
   ========================================================== */
(function () {
  function mount(root, data, opts = {}) {
    if (!root || !data || !window.DK) return;
    const K = DK, $ = s => root.querySelector(s);
    const COLS = [['todo', 'لم تبدأ', '#9AA5B1'], ['doing', 'قيد التنفيذ', K.PAL[0]], ['review', 'قيد المراجعة', K.PAL[1]], ['done', 'مكتملة', K.PAL[4]]];
    const PRI = { high: ['عالية', 'bad', 3], med: ['متوسطة', 'warn', 2], low: ['منخفضة', '', 1] };
    const PEOPLE = data.people || [];
    let PROJ = (data.projects || []).map(p => ({ ...p }));
    const S = { proj: null, q: '', sort: 'due', pri: 'all', drag: null, adding: false };
    let nid = 0;
    let T = (data.tasks || []).map(t => ({ ...t, id: nid++ }));
    const fmtDay = iso => new Date(iso).toLocaleDateString('ar-SA-u-ca-gregory-nu-latn', { weekday: 'short', day: 'numeric', month: 'short' });

    const teamAvs = $('#tbTeamAvs');
    if (teamAvs) {
      const shown = PEOPLE.slice(0, 3), rest = PEOPLE.length - shown.length;
      teamAvs.innerHTML = shown.map((p, w) => `<span class="dk-av" style="--h:${w * 90}">${K.esc((p.name || '?')[0])}</span>`).join('') + (rest > 0 ? `<span class="dk-av" style="--h:320">+${rest}</span>` : '');
    }

    function projs() {
      $('#tbProjs').innerHTML = PROJ.map((p, i) => `<button type="button" class="tb-pj" data-pj="${i}" aria-pressed="${S.proj === i}"><i style="background:${p.color}"></i>${K.esc(p.name)}<span class="num">${T.filter(x => x.pj === i).length}</span></button>`).join('') + `<button type="button" class="tb-pj add" data-addpj>${K.ic('plus')}إضافة مشروع</button>`;
    }
    const vis = () => T.filter(x => (S.proj == null || x.pj === S.proj) && (S.pri === 'all' || x.pri === S.pri) && (!S.q || x.t.includes(S.q) || x.cat.includes(S.q)));
    const sorter = { due: (a, b) => new Date(a.d) - new Date(b.d), pri: (a, b) => PRI[b.pri][2] - PRI[a.pri][2], prog: (a, b) => b.pg - a.pg };
    function card(x, i) {
      const col = COLS.find(c => c[0] === x.st)[2];
      return `<article class="tb-card" draggable="true" tabindex="0" data-id="${x.id}" style="--i:${i}" aria-label="${K.esc(x.t)} — ${COLS.find(c => c[0] === x.st)[1]}، اضغط السهم لنقلها">
        <header><span class="dk-chip">${K.esc(x.cat)}</span><span class="dk-chip ${PRI[x.pri][1]}"><i class="pd"></i>${PRI[x.pri][0]}</span></header>
        <b>${K.esc(x.t)}</b><small>الاستحقاق: ${fmtDay(x.d)}</small>
        <span class="dk-avs">${(x.who || []).map(w => `<span class="dk-av" style="--h:${w * 60}">${K.esc((PEOPLE[w]?.name || '?')[0])}</span>`).join('')}</span>
        <span class="pr"><span class="dk-prog"><i style="width:${x.pg}%;background:${col}"></i></span><span class="num">${x.pg}%</span></span>
        <footer><i style="background:${PROJ[x.pj]?.color || K.PAL[0]}"></i>${K.esc(PROJ[x.pj]?.name || '')}</footer></article>`;
    }
    function board() {
      const v = vis().sort(sorter[S.sort]);
      $('#tbBoard').innerHTML = COLS.map(([k, n, c]) => {
        const cs = v.filter(x => x.st === k);
        return `<section class="tb-col" data-col="${k}" aria-label="${n}"><header><i style="background:${c}"></i><b>${n}</b><span class="dk-chip"><span class="num">${cs.length}</span></span></header>
          ${k === 'todo' && S.adding ? `<form class="tb-new" id="tbNew"><input name="t" required placeholder="عنوان المهمة" aria-label="عنوان المهمة"><select name="p" aria-label="الأولوية"><option value="high">عالية</option><option value="med" selected>متوسطة</option><option value="low">منخفضة</option></select><select name="j" aria-label="المشروع">${PROJ.map((p, i) => `<option value="${i}">${K.esc(p.name)}</option>`).join('')}</select><span><button type="submit" class="dk-btn pri">إضافة</button><button type="button" class="dk-btn" data-cancel>إلغاء</button></span></form>` : ''}
          <div class="tb-list">${cs.map(card).join('') || '<p class="dk-empty">أفلت مهمة هنا</p>'}</div></section>`;
      }).join('');
      sum(); if (S.adding) $('#tbNew [name=t]')?.focus();
    }
    function sum() {
      const v = vis(), n = v.length || 1;
      $('#tbSum').innerHTML = `<span class="bar">${COLS.map(([k, nm, c]) => { const m = v.filter(x => x.st === k).length; return m ? `<i style="flex:${m};background:${c}" data-tip="${nm}|${m} مهام"></i>` : ''; }).join('')}</span><span>${COLS.map(([k, nm, c]) => `<span class="lg"><i style="background:${c}"></i>${nm} <b class="num">${v.filter(x => x.st === k).length}</b></span>`).join('')}</span><span class="mut">الإنجاز <b class="num">${Math.round(v.filter(x => x.st === 'done').length / n * 100)}%</b></span>`;
    }
    function move(tid, st, focus) {
      const x = T.find(t => t.id === tid); if (!x || x.st === st) return;
      x.st = st; if (st === 'done') x.pg = 100; else if (st === 'todo') x.pg = 0; else if (x.pg === 100 || x.pg === 0) x.pg = st === 'review' ? 80 : 30;
      board(); K.toast(`«${x.t}» ← ${COLS.find(c => c[0] === st)[1]}`);
      if (focus) root.querySelector(`[data-id="${tid}"]`)?.focus();
    }

    root.addEventListener('dragstart', e => { const c = e.target.closest('.tb-card'); if (!c) return; S.drag = +c.dataset.id; c.classList.add('dragging'); e.dataTransfer.effectAllowed = 'move'; e.dataTransfer.setData('text/plain', c.dataset.id); });
    root.addEventListener('dragend', e => { e.target.closest?.('.tb-card')?.classList.remove('dragging'); root.querySelectorAll('.tb-col.over').forEach(c => c.classList.remove('over')); });
    root.addEventListener('dragover', e => { const c = e.target.closest('.tb-col'); if (!c || S.drag == null) return; e.preventDefault(); root.querySelectorAll('.tb-col.over').forEach(x => x !== c && x.classList.remove('over')); c.classList.add('over'); });
    root.addEventListener('drop', e => { const c = e.target.closest('.tb-col'); if (!c || S.drag == null) return; e.preventDefault(); move(S.drag, c.dataset.col); S.drag = null; });
    root.addEventListener('keydown', e => { const c = e.target.closest?.('.tb-card'); if (!c || !['ArrowLeft', 'ArrowRight'].includes(e.key)) return; e.preventDefault(); const x = T.find(t => t.id === +c.dataset.id), i = COLS.findIndex(k => k[0] === x.st), ni = i + (e.key === 'ArrowLeft' ? 1 : -1); if (ni >= 0 && ni < 4) move(x.id, COLS[ni][0], true); });
    root.addEventListener('click', e => {
      const t = e.target;
      const p = t.closest('[data-pj]'); if (p) { const i = +p.dataset.pj; S.proj = S.proj === i ? null : i; projs(); return board(); }
      if (t.closest('[data-addpj]')) { PROJ.push({ name: 'مشروع جديد ' + (PROJ.length - 2), color: K.PAL[(PROJ.length + 1) % K.PAL.length] }); return projs(); }
      if (t.closest('[data-new]')) { S.adding = true; return board(); }
      if (t.closest('[data-cancel]')) { S.adding = false; return board(); }
    });
    root.addEventListener('submit', e => {
      if (e.target.id !== 'tbNew') return; e.preventDefault();
      const f = new FormData(e.target), due = new Date(); due.setDate(due.getDate() + 7);
      T.unshift({ id: nid++, cat: 'جديد', pri: f.get('p'), t: f.get('t'), st: 'todo', pg: 0, pj: +f.get('j'), d: due.toISOString().slice(0, 10), who: [] });
      S.adding = false; projs(); board(); K.toast('أُنشئت المهمة');
    });
    $('#tbQ').addEventListener('input', e => { S.q = e.target.value.trim(); board(); });
    $('#tbSort').addEventListener('change', e => { S.sort = e.target.value; board(); });
    K.seg(root, 'pri', v => { S.pri = v; board(); });

    projs(); board();
    return { destroy() {} };
  }

  window.JEEM_BOARD_MOUNT = mount;
})();
