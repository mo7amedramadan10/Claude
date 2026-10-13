/* ==========================================================
   جيم — استوديو المشاريع (لوحة تفاعلية)
   مستوحاة من «Project Managing Dashboard» (Bold): بطاقات حالة تصفّي الجدول الزمني،
   مهام اليوم، جانت أفقي باتجاه RTL مع خط «اليوم»، ودردشة فريق برسالة صوتية.
   ========================================================== */
(function () {
  const root = document.getElementById('psRoot'); if (!root || !window.DK) return;
  const K = DK, $ = s => root.querySelector(s);
  const ST = [['up', 'قادمة', 'history', '#E8A13A', 4], ['done', 'مكتملة', 'check', '#2F6BFF', 27], ['prog', 'قيد التنفيذ', 'link', '#2F6BFF', 12], ['all', 'الإجمالي', 'layers', '#E8A13A', 43]];
  const TEAM = [['محمد رضا', 'مصمم واجهات'], ['مهدي غلام', 'مصمم تجربة'], ['نسيم مقصود', 'مصممة جرافيك'], ['زهراء باقر', 'مديرة منتج'], ['مصطفى طاهر', 'مصمم واجهات']];
  const S = { st: null, who: null, view: 'grid', zoom: 1 };
  const TL = [['موقع مجوهرات PLC', 7, 12, 'prog', [0, 1], '#E8A13A'], ['مشروع NFT – صفحة المنتجات', 9, 14, 'prog', [2, 3, 0], '#2F6BFF'], ['صفحة إطلاق NFT', 6, 8, 'done', [4], '#2F6BFF'], ['مجموعة NFT', 8, 10, 'up', [1], '#2F6BFF'], ['تطبيق TDI – التهيئة', 6, 9, 'prog', [3, 2], '#E8A13A'], ['موقع عيادة أسنان – صفحة الهبوط', 11, 15, 'up', [4, 1], '#E8A13A'], ['هوية مقهى بُن', 12, 16, 'done', [2], '#7CB342']];
  let TASKS = [['تصميم واجهة البطل للموقع', [1], false], ['حزمة رسوم NFT', [2, 0], false], ['مفاهيم ثلاثية الأبعاد لأبرون', [3, 4], false]];
  const DAYS = Array.from({ length: 11 }, (_, i) => { const d = new Date(2026, 4, 6 + i); return { n: 6 + i, w: d.toLocaleDateString('ar-SA-u-ca-gregory-nu-latn', { weekday: 'short' }) }; });
  const av = (w, sz = '') => `<span class="dk-av${sz}" style="--h:${w * 67 + 10}" title="${TEAM[w][0]}">${TEAM[w][0][0]}</span>`;

  function status() {
    const cnt = k => k === 'all' ? TL.length : TL.filter(t => t.status === k || t[3] === k).length;
    $('#psStatus').innerHTML = `<div class="ps-st ${S.view}">${ST.map(([k, n, ic, c, v], i) => `<button type="button" data-st="${k}" aria-pressed="${S.st === k}" class="${S.st && S.st !== k && k !== 'all' ? 'dim' : ''}" style="--c:${c};--i:${i}"><span class="ib">${K.ic(ic)}</span><small>${n}</small><b><span class="num">${v}</span> مشروعًا</b><span class="mut">في الجدول: <span class="num">${cnt(k)}</span></span></button>`).join('')}</div>`;
  }
  function tasks() {
    $('#psTaskN').innerHTML = `<span class="num">${TASKS.filter(t => !t[2]).length}</span> متبقية`;
    $('#psTasks').innerHTML = TASKS.map(([t, w, d], i) => `<li class="${d ? 'done' : ''}" style="--i:${i}"><label><input type="checkbox" data-task="${i}" ${d ? 'checked' : ''}><span><b>${K.esc(t)}</b><small>المكلفون: ${w.map(x => TEAM[x][0]).join('، ')}</small></span></label><span class="dk-avs">${w.map(x => av(x)).join('')}</span></li>`).join('');
  }
  function tl() {
    const n = DAYS.length, colw = Math.round(86 * S.zoom), today = 7;
    const rows = TL.filter(t => (!S.st || S.st === 'all' || t[3] === S.st) && (S.who == null || t[4].includes(S.who)));
    $('#psTl').innerHTML = `<div class="ps-gantt" style="--n:${n};--cw:${colw}px"><div class="hd">${DAYS.map(d => `<span class="${d.n === today ? 'td' : ''}"><b class="num">${String(d.n).padStart(2, '0')}</b> ${d.w}</span>`).join('')}</div>
      <div class="bd">${DAYS.map((d, i) => `<span class="cl${i % 2 ? ' alt' : ''}" style="inset-inline-start:${i * colw}px"></span>`).join('')}<span class="now" style="inset-inline-start:${(today - 6 + .45) * colw}px"><i>اليوم</i></span>
        ${rows.length ? rows.map(([nm, s, e, st, w, c], i) => `<button type="button" class="bar" style="inset-inline-start:${(s - 6) * colw + 4}px;width:${(e - s + 1) * colw - 8}px;top:${14 + i * 54}px;--c:${c};--i:${i}" data-tip="${K.esc(nm)}|${s}–${e} مايو · ${ST.find(x => x[0] === st)[1]}"><span><b>${K.esc(nm)}</b><small><span class="num">${String(s).padStart(2, '0')}</span> مايو – <span class="num">${String(e).padStart(2, '0')}</span> مايو</small></span><span class="dk-avs">${w.map(x => av(x)).join('')}</span></button>`).join('') : '<p class="dk-empty" style="padding-top:60px">لا مشاريع بهذه التصفية</p>'}</div></div>`;
    $('#psTl .bd').style.height = Math.max(200, 30 + rows.length * 54) + 'px';
    const sc = () => { const el = $('#psTl'); el.scrollLeft = -Math.max(0, (today - 6 - 1) * colw); }; requestAnimationFrame(sc);
  }
  function team() { $('#psTeam').innerHTML = TEAM.map(([n, r], i) => `<li><button type="button" data-who="${i}" aria-pressed="${S.who === i}" class="${S.who != null && S.who !== i ? 'dim' : ''}">${av(i)}<span><b>${n}</b><small>${r}</small></span><span class="mut"><span class="num">${TL.filter(t => t[4].includes(i)).length}</span> مشاريع</span></button></li>`).join(''); }
  let CHAT = [['in', 'voice', 'محمد', '10:12 ص'], ['day', '06 مايو'], ['out', 'text', 'تم رفع الإصدار الجديد من الواجهة 👌', '10:14 ص'], ['in', 'img', 'نسيم', '10:31 ص'], ['in', 'text', 'شاهدوا التصميم المحدث 🔥', '10:33 ص', 'نسيم']];
  function chat() {
    $('#psChat').innerHTML = CHAT.map(m => m[0] === 'day' ? `<span class="day">${m[1]}</span>` : `<div class="msg ${m[0]}">${m[1] === 'voice' ? `<button type="button" class="vc" data-voice aria-label="تشغيل الرسالة الصوتية">${K.ic('play')}<span class="wv">${Array.from({ length: 22 }, (_, i) => `<i style="--h:${30 + Math.abs(Math.sin(i * 1.7)) * 70}%;--i:${i}"></i>`).join('')}</span><span class="num">0:21</span></button>` : m[1] === 'img' ? `<span class="im" role="img" aria-label="صورة مرفقة: رسم أولي"></span>` : `<p>${K.esc(m[2])}</p>`}<small>${m[3]}</small></div>`).join('');
    const c = $('#psChat'); c.scrollTop = c.scrollHeight;
  }
  function scope() { const p = []; if (S.st && S.st !== 'all') p.push(['st', ST.find(x => x[0] === S.st)[1]]); if (S.who != null) p.push(['who', TEAM[S.who][0]]); $('#psScope').innerHTML = p.map(([k, v]) => `<span class="dk-chip info">${v}<button type="button" data-rm="${k}" aria-label="إزالة">${K.ic('x')}</button></span>`).join(''); }
  function all() { status(); tasks(); tl(); team(); chat(); scope(); }

  root.addEventListener('click', e => {
    const t = e.target; let x;
    if ((x = t.closest('[data-st]'))) { const k = x.dataset.st; S.st = S.st === k || k === 'all' ? null : k; status(); tl(); return scope(); }
    if ((x = t.closest('[data-who]'))) { const i = +x.dataset.who; S.who = S.who === i ? null : i; team(); tl(); return scope(); }
    if ((x = t.closest('[data-rm]'))) { S[x.dataset.rm] = null; status(); team(); tl(); return scope(); }
    if ((x = t.closest('[data-voice]'))) { x.classList.toggle('on'); x.querySelector('use').setAttribute('href', x.classList.contains('on') ? '#i-pause' : '#i-play'); if (x.classList.contains('on')) setTimeout(() => { x.classList.remove('on'); x.querySelector('use').setAttribute('href', '#i-play'); }, 4000); return; }
  });
  root.addEventListener('change', e => { const c = e.target.closest('[data-task]'); if (c) { TASKS[+c.dataset.task][2] = c.checked; tasks(); } });
  $('#psSend').addEventListener('submit', e => { e.preventDefault(); const v = e.target.m.value.trim(); if (!v) return; const tm = new Date().toLocaleTimeString('ar-SA-u-nu-latn', { hour: 'numeric', minute: '2-digit' }); CHAT.push(['out', 'text', v, tm]); e.target.reset(); chat(); setTimeout(() => { CHAT.push(['in', 'text', 'تمام، سأراجعها الآن 👍', tm]); chat(); }, 1200); });
  K.seg(root, 'view', v => { S.view = v; status(); });
  K.seg(root, 'zoom', v => { S.zoom = +v; tl(); });
  all();
})();
