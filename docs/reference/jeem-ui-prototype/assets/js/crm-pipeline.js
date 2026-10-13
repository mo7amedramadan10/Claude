/* ==========================================================
   جيم — إدارة العملاء (CRM) بلوحة مراحل قابلة للسحب
   مستوحاة من «CRM Dashboard App Design» وأُعيدت بمحتوى عربي عام.
   الحركات: أعمدة «عملاء جدد» تنمو، عدّاد الصفقات الناجحة يمتلئ بشرطات،
   بطاقات تدخل بالتتابع، سحب وإفلات بين المراحل مع تحديث الأعداد والنسبة حيًا.
   ========================================================== */
(function () {
  const root = document.getElementById('crRoot'); if (!root) return;
  const $ = (s, el = root) => el.querySelector(s), $$ = (s, el = root) => [...el.querySelectorAll(s)];
  const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
  const esc = s => String(s).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
  const RM = matchMedia('(prefers-reduced-motion: reduce)').matches;
  const nf = v => Number(v).toLocaleString('en-US');
  const norm = s => String(s).replace(/[أإآ]/g, 'ا').replace(/ة/g, 'ه').replace(/ى/g, 'ي').toLowerCase();
  const count = (el, to, fmt = v => nf(Math.round(v)), dur = 900) => { if (!el) return; if (RM) { el.textContent = fmt(to); return; } const from = +el.dataset.v || 0, t0 = performance.now(); el.dataset.v = to; const st = t => { const p = Math.min(1, (t - t0) / dur), e = 1 - Math.pow(1 - p, 3); el.textContent = fmt(from + (to - from) * e); if (p < 1) requestAnimationFrame(st); }; requestAnimationFrame(st); };
  const toast = msg => { let t = document.querySelector('.toast'); if (!t) { t = document.createElement('div'); t.className = 'toast'; t.setAttribute('role', 'status'); document.body.append(t); } t.innerHTML = `${ic('check')}<span>${esc(msg)}</span>`; requestAnimationFrame(() => t.classList.add('is-on')); clearTimeout(t._h); t._h = setTimeout(() => t.classList.remove('is-on'), 2600); };

  /* ---------- البيانات التجريبية ---------- */
  const TODAY = new Date(2026, 9, 11);
  const ST = [['contact', 'تم التواصل'], ['nego', 'تفاوض'], ['offer', 'أُرسل العرض'], ['won', 'صفقة مغلقة']];
  const MEM = [['me', 'أنا', 'مديرة العلاقات'], ['sa', 'سارة البيشي', 'مديرة منتج'], ['an', 'أنس القرشي', 'مدير مبيعات'], ['ja', 'جمال الكناني', 'مدير نمو'], ['ka', 'كارمن الرشيد', 'أخصائية تسويق رقمي']];
  const memBy = Object.fromEntries(MEM.map(m => [m[0], m]));
  const C = [
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
    ['حلول المصرف الآمن', 'تقنيات مالية مبتكرة وتحول رقمي للبنوك', 'won', '2026-09-30', 4, 7, 'an']
  ].map(([n, d, s, due, cm, at, m, addr, mail], i) => ({ id: 'c' + i, n, d, s, due, cm, at, m, addr: addr || ['طريق الملك فهد، الرياض', 'حي الشاطئ، جدة', 'حي الفيصلية، الدمام'][i % 3], mail: mail || `info@client${i + 1}.sa` }));
  const S = { q: '', me: false, mem: null, flt: 'all', sort: {}, sel: 'c9', drag: null };
  let won0 = 0;

  /* ---------- 1) شريط المؤشرات ---------- */
  const NEW = [[3, 7], [4, 9], [5, 10], [2, 8], [3, 4], [6, 9]];
  const DAYS = ['السبت', 'الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس'];
  function kpis(intro) {
    if (intro) $('#crNew').innerHTML = NEW.map(([a, b], i) => `<button type="button" class="nb" style="--i:${i}" data-tip="${DAYS[i]}|${b} عملاء جدد · ${a} من الإحالات"><span class="bars"><i class="h" style="height:${a * 10}%"></i><i class="s${i === NEW.length - 1 ? ' last' : ''}" style="height:${b * 10}%"></i></span><small>${DAYS[i].replace('ال', '')}</small></button>`).join('');
    const won = C.filter(c => c.s === "won").length, p = Math.round((won + 31) / (won + 46) * 100);
    gauge(Math.min(99, p)); count($('#crTasks'), C.filter(c => c.s !== 'won').length * 5 - 2); count($('#crPre'), 15890 + (won - won0) * 1250, v => '$' + nf(Math.round(v)));
  }
  function gauge(p) {
    const N = 40, R1 = 44, R2 = 56, on = Math.round(p / 100 * N);
    $('#crGauge').innerHTML = `<svg viewBox="-64 -64 128 70" role="img" aria-label="الصفقات الناجحة ${p}%">${Array.from({ length: N + 1 }, (_, i) => { const a = Math.PI * (1 - i / N), c = Math.cos(a), s = Math.sin(a);
      return `<line x1="${(c * R1).toFixed(1)}" y1="${(-s * R1).toFixed(1)}" x2="${(c * R2).toFixed(1)}" y2="${(-s * R2).toFixed(1)}" class="${i <= on ? 'on' : ''}" style="--i:${i}"/>`; }).join('')}</svg><span class="gv"><b class="num" id="crP">0</b><small>صفقات ناجحة</small></span>`;
    count($('#crP'), p, v => Math.round(v) + '%');
  }

  /* ---------- 2) اللوحة ---------- */
  const dueInfo = c => { if (!c.due) return ['بلا موعد', 'none']; const d = new Date(c.due), days = Math.round((d - TODAY) / 864e5), lab = d.toLocaleDateString('ar-SA-u-ca-gregory-nu-latn', { day: 'numeric', month: 'short' }); return [lab, c.s !== 'won' && days < 0 ? 'late' : c.s !== 'won' && days <= 3 ? 'soon' : '']; };
  const ini = n => n === 'أنا' ? 'أنا' : n.split(' ').slice(0, 2).map(x => x.replace(/^ال/, '')[0]).join('‌');
  const avatar = id => { const m = memBy[id], h = ({ me: 210, sa: 330, an: 160, ja: 30, ka: 270 })[id]; return `<span class="av" style="--h:${h}" title="${m[1]}">${esc(ini(m[1]))}</span>`; };
  const visible = c => (!S.q || norm(c.n + c.d).includes(norm(S.q))) && (!S.me || c.m === 'me') && (!S.mem || c.m === S.mem) && (S.flt === 'all' || (S.flt === 'late' ? dueInfo(c)[1] === 'late' : S.flt === 'none' ? !c.due : true));
  function card(c, i) {
    const [dl, dc] = dueInfo(c), sel = S.sel === c.id;
    return `<article class="cr-card${sel ? ' sel' : ''}" draggable="true" data-c="${c.id}" tabindex="0" role="button" aria-expanded="${sel}" aria-label="${esc(c.n)}، ${ST.find(s => s[0] === c.s)[1]}" style="--i:${i}">
      <header><b>${esc(c.n)}</b><button type="button" class="mn" data-menu="${c.id}" aria-label="خيارات ${esc(c.n)}" aria-haspopup="true">${ic('more')}</button></header>
      <p>${esc(c.d)}</p>
      ${sel ? `<ul class="info"><li>${ic('pin')}${esc(c.addr)}</li><li>${ic('send')}<span dir="ltr">${esc(c.mail)}</span></li></ul><div class="mg">${avatar(c.m)}<span><small>المسؤول</small>${esc(memBy[c.m][1])}</span></div>` : ''}
      <footer><span class="due ${dc}">${ic(dc === 'late' ? 'clock' : 'calendar')}${dl}${dc === 'late' ? ' · متأخر' : ''}</span><span class="sp"></span>${sel ? '' : avatar(c.m)}<span class="mt" title="تعليقات">${ic('chat')}<span class="num">${c.cm}</span></span><span class="mt" title="مرفقات">${ic('link')}<span class="num">${c.at}</span></span></footer>
      <div class="menu" hidden role="menu">${ST.filter(s => s[0] !== c.s).map(s => `<button type="button" role="menuitem" data-mv="${c.id}|${s[0]}">${ic('share')}نقل إلى «${s[1]}»</button>`).join('')}<button type="button" role="menuitem" data-mine="${c.id}">${ic('users')}تعيين لي</button><button type="button" role="menuitem" class="dg" data-arch="${c.id}">${ic('trash')}أرشفة</button></div>
    </article>`;
  }
  function board(intro) {
    $('#crBoard').innerHTML = ST.map(([k, n]) => { let L = C.filter(c => c.s === k && visible(c)); const so = S.sort[k];
      if (so === 'due') L.sort((a, b) => (a.due || '9') .localeCompare(b.due || '9')); else if (so === 'name') L.sort((a, b) => a.n.localeCompare(b.n, 'ar'));
      return `<section class="cr-col" data-col="${k}" aria-label="${n}"><header><h2>${n}</h2><button type="button" class="ct" data-sort="${k}" title="ترتيب: ${so === 'due' ? 'الموعد' : so === 'name' ? 'الاسم' : 'الإضافة'}"><span class="num">${C.filter(c => c.s === k).length}</span>${ic('filter')}</button></header>
        <div class="list">${L.map(card).join('') || `<p class="empty">${S.q || S.me || S.mem || S.flt !== 'all' ? 'لا عملاء يطابقون التصفية' : 'اسحب عميلًا إلى هنا'}</p>`}</div></section>`; }).join('');
    if (!intro) $$('.cr-card').forEach(e => e.style.animation = 'none');
    $('#crCount').textContent = C.filter(visible).length;
  }
  function move(id, to, how) {
    const c = C.find(x => x.id === id); if (!c || c.s === to) return; const from = c.s; c.s = to;
    board(false); kpis(false);
    const el = $(`[data-c="${id}"]`); if (el && !RM) { el.style.animation = ''; el.classList.add('land'); }
    toast(to === 'won' ? `تهانينا! أُغلقت صفقة «${c.n}»` : `نُقل «${c.n}» من «${ST.find(s => s[0] === from)[1]}» إلى «${ST.find(s => s[0] === to)[1]}»${how ? '' : ''}`);
  }

  /* ---------- 3) فريق العمل (تصفية حسب المسؤول) ---------- */
  function members() {
    $('#crMem').innerHTML = MEM.filter(m => m[0] !== 'me').map(([id, n, r]) => `<button type="button" data-mem="${id}" aria-pressed="${S.mem === id}" title="${r}">${avatar(id)}<span>${n}<small>${r}</small></span><em class="num">${C.filter(c => c.m === id).length}</em></button>`).join('');
  }

  /* ---------- 4) إضافة عميل ---------- */
  const dlg = $('#crDlg');
  function openAdd() {
    dlg.innerHTML = `<form class="cr-dlg" role="dialog" aria-modal="true" aria-labelledby="crDT"><h3 id="crDT">إضافة عميل</h3>
      <label>اسم العميل<input name="n" required maxlength="40" placeholder="مثال: شركة الأفق"></label>
      <label>الوصف<input name="d" maxlength="80" placeholder="نشاط العميل باختصار"></label>
      <div class="two"><label>المرحلة<select name="s">${ST.map(([k, n]) => `<option value="${k}">${n}</option>`).join('')}</select></label><label>الموعد<input name="due" type="date" value="2026-10-25"></label></div>
      <label>المسؤول<select name="m">${MEM.map(([k, n]) => `<option value="${k}">${n}</option>`).join('')}</select></label>
      <div class="act"><button type="button" class="btn" data-x>إلغاء</button><button type="submit" class="btn btn-dark">${ic('plus')}إضافة</button></div></form>`;
    dlg.hidden = false; $('input[name="n"]', dlg).focus();
    $('form', dlg).addEventListener('submit', e => { e.preventDefault(); const f = new FormData(e.target), c = { id: 'c' + C.length + '_' + Date.now(), n: f.get('n').trim(), d: f.get('d').trim() || '—', s: f.get('s'), due: f.get('due'), cm: 0, at: 0, m: f.get('m'), addr: '—', mail: '—' };
      if (!c.n) return; C.unshift(c); closeDlg(); S.q = ''; $('#crQ').value = ''; S.sel = c.id; board(false); kpis(false); members(); const el = $(`[data-c="${c.id}"]`); if (el) { el.classList.add('land'); el.scrollIntoView({ block: 'nearest', behavior: RM ? 'auto' : 'smooth' }); } toast(`أُضيف «${c.n}»`); });
  }
  const closeDlg = () => { dlg.hidden = true; dlg.innerHTML = ''; };
  const closeMenus = () => $$('.cr-card .menu').forEach(m => m.hidden = true);

  /* ---------- الأحداث ---------- */
  root.addEventListener('click', e => {
    const t = e.target;
    const mn = t.closest('[data-menu]'); if (mn) { e.stopPropagation(); const m = mn.closest('.cr-card').querySelector('.menu'), was = !m.hidden; closeMenus(); m.hidden = was; if (!was) m.querySelector('button').focus(); return; }
    const mv = t.closest('[data-mv]'); if (mv) { const [id, to] = mv.dataset.mv.split('|'); return move(id, to); }
    const mi = t.closest('[data-mine]'); if (mi) { const c = C.find(x => x.id === mi.dataset.mine); c.m = 'me'; board(false); members(); return toast(`عُيّن «${c.n}» لك`); }
    const ar = t.closest('[data-arch]'); if (ar) { const i = C.findIndex(x => x.id === ar.dataset.arch), c = C[i]; C.splice(i, 1); board(false); kpis(false); members(); return toast(`أُرشف «${c.n}»`); }
    closeMenus();
    const so = t.closest('[data-sort]'); if (so) { const k = so.dataset.sort, o = ['', 'due', 'name']; S.sort[k] = o[(o.indexOf(S.sort[k] || '') + 1) % 3]; board(false); return toast(`ترتيب «${ST.find(s => s[0] === k)[1]}»: ${({ '': 'حسب الإضافة', due: 'حسب الموعد', name: 'حسب الاسم' })[S.sort[k]]}`); }
    const mb = t.closest('[data-mem]'); if (mb) { S.mem = S.mem === mb.dataset.mem ? null : mb.dataset.mem; members(); return board(false); }
    if (t.closest('[data-me]')) { S.me = !S.me; $('[data-me]').setAttribute('aria-pressed', String(S.me)); return board(false); }
    if (t.closest('[data-add]')) return openAdd();
    if (t.closest('[data-x]') || t === dlg) return closeDlg();
    const go = t.closest('[data-go]'); if (go) { S.flt = 'all'; $('#crFlt').value = 'all'; S.mem = null; S.me = false; members(); board(false); const k = go.dataset.go; $(`[data-col="${k}"]`)?.scrollIntoView({ block: 'nearest', inline: 'center', behavior: RM ? 'auto' : 'smooth' }); $(`[data-col="${k}"]`)?.classList.add('flash'); return; }
    const cd = t.closest('.cr-card'); if (cd && !t.closest('.menu')) { S.sel = S.sel === cd.dataset.c ? null : cd.dataset.c; board(false); $(`[data-c="${cd.dataset.c}"]`)?.focus(); }
  });
  root.addEventListener('keydown', e => {
    const cd = e.target.closest('.cr-card');
    if (cd && e.target === cd) {
      if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); cd.click(); }
      /* نقل بلوحة المفاتيح: Shift + سهم يمين/يسار ينقل البطاقة للمرحلة السابقة/التالية (الاتجاه من اليمين لليسار) */
      if (e.shiftKey && (e.key === 'ArrowLeft' || e.key === 'ArrowRight')) { e.preventDefault(); const c = C.find(x => x.id === cd.dataset.c), i = ST.findIndex(s => s[0] === c.s), j = i + (e.key === 'ArrowLeft' ? 1 : -1); if (ST[j]) { move(c.id, ST[j][0]); $(`[data-c="${c.id}"]`)?.focus(); } }
    }
    if (e.key === 'Escape') { closeMenus(); if (!dlg.hidden) closeDlg(); }
  });
  document.addEventListener('click', e => { if (!root.contains(e.target)) closeMenus(); });
  /* السحب والإفلات */
  root.addEventListener('dragstart', e => { const cd = e.target.closest('.cr-card'); if (!cd) return; S.drag = cd.dataset.c; cd.classList.add('dragging'); e.dataTransfer.effectAllowed = 'move'; e.dataTransfer.setData('text/plain', S.drag); root.classList.add('is-drag'); });
  root.addEventListener('dragend', e => { e.target.closest?.('.cr-card')?.classList.remove('dragging'); root.classList.remove('is-drag'); $$('.cr-col').forEach(c => c.classList.remove('over')); S.drag = null; });
  root.addEventListener('dragover', e => { const col = e.target.closest('.cr-col'); if (!col || !S.drag) return; e.preventDefault(); $$('.cr-col').forEach(c => c.classList.toggle('over', c === col)); });
  root.addEventListener('drop', e => { const col = e.target.closest('.cr-col'); if (!col || !S.drag) return; e.preventDefault(); const id = S.drag; S.drag = null; root.classList.remove('is-drag'); $$('.cr-col').forEach(c => c.classList.remove('over')); move(id, col.dataset.col, 'drag'); });
  $('#crQ').addEventListener('input', e => { S.q = e.target.value.trim(); board(false); });
  $('#crFlt').addEventListener('change', e => { S.flt = e.target.value; board(false); });

  won0 = C.filter(c => c.s === 'won').length;
  kpis(true); members(); board(true);
})();
