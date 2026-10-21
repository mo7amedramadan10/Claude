/* ==========================================================
   جيم — نماذج العناصر الجاهزة (تبويب «النماذج» في المحادثة)
   كل نموذج = عنصر جاهز يُضاف للوحة الحالية بضغطة.
   في التطبيق الفعلي: القائمة تأتي من الـ API حسب مصادر المشروع،
   والإضافة = POST يضيف widget لـ JSON اللوحة ويرجع بياناته الحقيقية.
   البيانات هنا تجريبية للعرض فقط.
   ========================================================== */
(function () {
  const panel = document.getElementById('thread-templates');
  const grid = document.querySelector('.canvas .grid');
  if (!panel || !grid) return;

  const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
  const CAT = window.JEEM_CATALOG, T = CAT.THUMB;
  const TEMPLATES = CAT.widgets();
  const C1 = 'var(--chart-1)', C2 = 'var(--chart-2)', C3 = 'var(--chart-3)', C4 = 'var(--chart-4)';

  /* ---- مصغّرات النماذج (SVG بسيط) ---- */
  /* ---- محتوى العنصر بعد إضافته للوحة ---- */
  const bars = (rows, max, unit = 'م') => `<ul class="hbars">${rows.map(([n, v], i) => `<li><span class="lbl">${n}</span><span class="trk"><span class="fill" style="width:${(v / max * 100).toFixed(1)}%;animation-delay:${i * .06}s"></span></span><span class="val num">${v}${unit}</span></li>`).join('')}</ul>`;
  const R = {
    kpi: () => `<div class="value"><span class="num">8.9</span><small>مليون ر.س</small></div><div class="cmp"><span class="delta up">${ic('up')}<span class="num">9.2%</span></span> مقارنة بنفس الفترة من 2025</div><div class="tpl-spark"><svg viewBox="0 0 120 30" preserveAspectRatio="none"><polyline points="0,26 20,20 40,23 60,14 80,16 100,9 120,4" fill="none" stroke="${C1}" stroke-width="2" vector-effect="non-scaling-stroke"/></svg></div>`,
    compare: () => `<div class="legend"><span><i style="background:#BFD9EC"></i>2025</span><span><i style="background:${C1}"></i>2026</span></div><div class="cbars">${[['الربع الأول', 11.2, 13.1], ['الربع الثاني', 13.0, 15.7], ['الربع الثالث', 15.4, 19.4]].map(([q, a, b]) => `<div class="cb"><div class="pair"><i style="height:${a / 20 * 100}%;background:#BFD9EC" title="2025: ${a}م"></i><i style="height:${b / 20 * 100}%;background:${C1}" title="2026: ${b}م"></i></div><span>${q}</span></div>`).join('')}</div>`,
    line: () => `<div class="tpl-line">${T.line}</div><div class="cmp" style="display:flex;justify-content:space-between"><span>يناير</span><span><b class="num">+1,284</b> عميل جديد</span><span>سبتمبر</span></div>`,
    progress: () => bars([['الرياض', 104], ['الشرقية', 98], ['مكة المكرمة', 91], ['المدينة المنورة', 76]], 110, '%'),
    table: () => `<table class="table"><thead><tr><th>العميل</th><th>المشتريات</th><th>الطلبات</th></tr></thead><tbody>${[['مجموعة الراجحي للتجارة', '2.4م', 182], ['شركة البحر الأحمر', '1.9م', 141], ['مستشفيات الشفاء', '1.6م', 96], ['مصنع الخليج', '1.1م', 77]].map(r => `<tr><td>${r[0]}</td><td><span class="num">${r[1]}</span></td><td><span class="num">${r[2]}</span></td></tr>`).join('')}</tbody></table>`,
    donut: () => `<div style="display:flex;align-items:center;gap:20px;flex-wrap:wrap"><div class="cdonut" style="background:conic-gradient(${C1} 0 46%, #fff 0 46.6%, ${C2} 0 76%, #fff 0 76.6%, ${C3} 0 92%, #fff 0 92.6%, ${C4} 0)"><span><b class="num">50.6م</b>الإجمالي</span></div><ul class="donut-list" style="flex:1;min-width:160px">${[['الفروع', C1, '46%'], ['المتجر الإلكتروني', C2, '30%'], ['المناديب', C3, '16%'], ['الشركاء', C4, '8%']].map(([n, c, p]) => `<li><i style="background:${c}"></i><span>${n}</span><b></b><span class="num">${p}</span></li>`).join('')}</ul></div>`,
    heat: () => `<div class="heat">${['الأحد', 'الإثنين', 'الثلاثاء', 'الأربعاء', 'الخميس'].map((d, r) => `<span class="d">${d}</span>${Array.from({ length: 8 }, (_, c) => { const v = [.2, .35, .6, .9, .7, .5, .8, .4][(c + r * 3) % 8]; return `<i style="opacity:${v}" title="${d} · ${9 + c * 2}:00"></i>`; }).join('')}`).join('')}<span></span>${['9ص', '11', '1م', '3', '5', '7', '9', '11م'].map(h => `<span class="h">${h}</span>`).join('')}</div>`,
    funnel: () => `<div class="funnel">${[['عروض الأسعار', 3240, 100], ['تفاوض', 1820, 72], ['موافقة مبدئية', 960, 48], ['صفقات مغلقة', 612, 30]].map(([n, v, w], i) => `<div class="fr"><span class="fb" style="width:${w}%;${i === 3 ? 'background:' + C4 : ''}"><b class="num">${v.toLocaleString('en-US')}</b></span><span class="fl">${n}</span></div>`).join('')}</div>`
  };

  const CATS = [['all', 'الكل'], ['kpi', 'مؤشرات'], ['chart', 'رسوم بيانية'], ['table', 'جداول']];

  /* ---- رسم المعرض: «عناصر جاهزة» + «مكتبة المؤشرات» ---- */
  const LIB = window.JEEM_KPI_LIBRARY;
  const fmt = n => n.toLocaleString('en-US');
  panel.innerHTML = `
    <div class="tpl-head">
      <div class="tpl-views" role="tablist" aria-label="نوع النماذج">
        <button type="button" role="tab" class="is-on" aria-selected="true" data-view="widgets">${ic('grid')}عناصر جاهزة <span class="num">${TEMPLATES.length}</span></button>
        ${LIB ? `<button type="button" role="tab" aria-selected="false" data-view="kpis">${ic('target')}مكتبة المؤشرات <span class="num">${fmt(LIB.rows.filter((r, i) => CAT.kpiStatus(i) === 'published').length)}</span></button>` : ''}
      </div>
      <div class="tpl-ctrl" data-for="widgets">
        <label class="tpl-search">${ic('search')}<span class="sr-only">بحث في النماذج</span><input type="search" placeholder="ابحث في النماذج…"></label>
        <div class="tpl-cats" role="group" aria-label="تصنيف النماذج">${CATS.map(([k, n], i) => `<button type="button" class="chip${i ? '' : ' is-on'}" data-cat="${k}" aria-pressed="${i ? 'false' : 'true'}">${n}</button>`).join('')}</div>
      </div>
      <div class="tpl-ctrl" data-for="kpis" hidden></div>
    </div>
    <section class="tpl-view" data-view="widgets">
      <p class="tpl-hint">${ic('info')}اضغط على أي نموذج ليُضاف للوحة الحالية ببيانات مشروعك.</p>
      <div class="tpl-grid">${TEMPLATES.map(t => `
        <button type="button" class="tpl-card" data-tpl="${t.id}" data-cat="${t.cat}" aria-label="إضافة ${t.name} للوحة">
          <span class="tpl-thumb">${T[t.render] || T.kpi}</span>
          <span class="tpl-body"><b>${t.name}</b><small>${t.desc}</small><em>${ic('database')}${t.src}</em></span>
          <span class="tpl-add">${ic('plus')}<span>إضافة</span></span>
        </button>`).join('')}</div>
    </section>
    <section class="tpl-view" data-view="kpis" hidden></section>`;

  /* ---- التبديل بين العرضين ---- */
  panel.querySelectorAll('.tpl-views [data-view]').forEach(b => b.addEventListener('click', () => {
    const v = b.dataset.view;
    panel.querySelectorAll('.tpl-views [data-view]').forEach(x => { const on = x === b; x.classList.toggle('is-on', on); x.setAttribute('aria-selected', String(on)); });
    panel.querySelectorAll('.tpl-ctrl, .tpl-view').forEach(el => el.hidden = (el.dataset.for || el.dataset.view) !== v);
    if (v === 'kpis') initLibrary();
    panel.scrollTop = 0;
  }));

  /* ---- البحث والتصنيف ---- */
  const cards = [...panel.querySelectorAll('.tpl-card')];
  let cat = 'all', q = '';
  const filter = () => cards.forEach(c => { const t = TEMPLATES.find(x => x.id === c.dataset.tpl); c.hidden = !((cat === 'all' || t.cat === cat) && (t.name + t.desc).includes(q)); });
  panel.querySelector('[data-for="widgets"] .tpl-search input').addEventListener('input', e => { q = e.target.value.trim(); filter(); });
  panel.querySelectorAll('.tpl-cats .chip').forEach(b => b.addEventListener('click', () => {
    cat = b.dataset.cat;
    panel.querySelectorAll('.tpl-cats .chip').forEach(x => { const on = x === b; x.classList.toggle('is-on', on); x.setAttribute('aria-pressed', String(on)); });
    filter();
  }));

  /* ---- الإضافة للوحة الحالية ---- */
  const toast = msg => {
    let t = document.querySelector('.toast');
    if (!t) { t = document.createElement('div'); t.className = 'toast'; t.setAttribute('role', 'status'); document.body.appendChild(t); }
    t.innerHTML = `${ic('check')}${msg}`; t.classList.remove('is-on'); void t.offsetWidth; t.classList.add('is-on');
    clearTimeout(t._h); t._h = setTimeout(() => t.classList.remove('is-on'), 2600);
  };
  cards.forEach(card => card.addEventListener('click', () => {
    const t = TEMPLATES.find(x => x.id === card.dataset.tpl);
    const w = document.createElement('article');
    w.className = `widget span-${t.span} is-new` + (t.render === 'kpi' ? ' kpi' : '');
    w.innerHTML = `
      <div class="w-head"><div><h3>${t.name}</h3><p>${t.desc}</p></div>
        <div class="w-tools"><button type="button" aria-label="حذف العنصر" data-remove>${ic('trash')}</button></div></div>
      ${R[t.render] ? R[t.render]() : R.kpi()}
      <span class="tpl-src">${ic('database')}${t.src}</span>`;
    grid.appendChild(w);
    w.querySelector('[data-remove]').addEventListener('click', () => { w.remove(); card.classList.remove('is-added'); toast('تم حذف العنصر من اللوحة'); });
    w.scrollIntoView({ behavior: 'smooth', block: 'center' });
    setTimeout(() => w.classList.remove('is-new'), 2400);
    card.classList.add('is-added');
    card.querySelector('.tpl-add span').textContent = 'أُضيف · أضف مرة أخرى';
    toast(`تمت إضافة «${t.name}» للوحة`);
  }));

  /* ==========================================================
     مكتبة المؤشرات — 1,899 مؤشرًا من Spider Strategies
     بحث (عربي/إنجليزي) + تصنيف (أقسام/قطاعات) + نوع القياس.
     العرض تدريجي (40 عنصرًا كل مرة) للحفاظ على الأداء.
     في التطبيق الفعلي: GET /kpi-library?q=&cat=&type=&page=
     ========================================================== */
  const norm = x => x.toLowerCase().replace(/[أإآ]/g, 'ا').replace(/ة/g, 'ه').replace(/ى/g, 'ي').replace(/[\u064B-\u0652ـ]/g, '');
  const MT_KEY = ['pct', 'time', 'money', 'score', 'count', 'other'];
  const MT_ICON = ['percent', 'clock', 'wallet', 'star', 'hash', 'target'];
  const PAGE = 40;
  let libReady = false;

  function initLibrary() {
    if (libReady || !LIB) return; libReady = true;
    const ctrl = panel.querySelector('.tpl-ctrl[data-for="kpis"]');
    const view = panel.querySelector('.tpl-view[data-view="kpis"]');
    const idx = LIB.rows.map(r => norm(r[2] + ' ' + r[3] + ' ' + LIB.cats[r[0]][0] + ' ' + LIB.cats[r[0]][1]));
    const deps = LIB.cats.map((c, i) => [c, i]).filter(([c, i]) => c[2] === 0 && CAT.catOn(i));
    const inds = LIB.cats.map((c, i) => [c, i]).filter(([c, i]) => c[2] === 1 && CAT.catOn(i));
    const opt = ([c, i]) => `<option value="${i}">${c[0]}</option>`;
    ctrl.innerHTML = `
      <label class="tpl-search">${ic('search')}<span class="sr-only">بحث في مكتبة المؤشرات</span><input type="search" placeholder="ابحث بالعربي أو English… مثل: رضا، churn"></label>
      <div class="kpi-filters">
        <label class="sr-only" for="kpiCat">التصنيف</label>
        <select id="kpiCat" class="select">
          <option value="">كل التصنيفات (${deps.length + inds.length})</option>
          <optgroup label="الأقسام الوظيفية">${deps.map(opt).join('')}</optgroup>
          <optgroup label="القطاعات">${inds.map(opt).join('')}</optgroup>
        </select>
      </div>
      <div class="tpl-cats" role="group" aria-label="نوع القياس">
        <button type="button" class="chip is-on" data-mt="" aria-pressed="true">الكل</button>
        ${LIB.mtypes.map((m, i) => `<button type="button" class="chip" data-mt="${i}" aria-pressed="false">${m}</button>`).join('')}
      </div>`;
    view.innerHTML = `
      <p class="kpi-count" aria-live="polite"></p>
      <ul class="kpi-list" role="list"></ul>
      <button type="button" class="btn btn-ghost kpi-more" hidden></button>
      <p class="kpi-credit">${ic('info')}المصدر: مكتبة <a href="https://www.spiderstrategies.com/kpi/" target="_blank" rel="noopener">Spider Strategies</a> · الترجمة ونوع القياس من جيم، والقيم المعروضة تجريبية حتى ربط المؤشر بمصدر.</p>`;
    const list = view.querySelector('.kpi-list'), more = view.querySelector('.kpi-more'), count = view.querySelector('.kpi-count');
    let fq = '', fc = '', fm = '', hits = [], shown = 0;
    const added = new Set();

    const item = i => {
      const r = LIB.rows[i], c = LIB.cats[r[0]], on = added.has(i);
      return `<li><button type="button" class="kpi-item${on ? ' is-added' : ''}" data-k="${i}" aria-label="إضافة مؤشر ${r[2]} للوحة">
        <span class="kpi-ic mt-${MT_KEY[r[4]]}">${ic(MT_ICON[r[4]])}</span>
        <span class="kpi-txt"><b>${r[2]}</b><small dir="ltr">${r[3]}</small>
          <em><span>${c[0]}</span><span class="mt mt-${MT_KEY[r[4]]}">${LIB.mtypes[r[4]]}</span></em></span>
        <span class="kpi-plus">${ic(on ? 'check' : 'plus')}</span></button></li>`;
    };
    const page = () => {
      const next = hits.slice(shown, shown + PAGE);
      list.insertAdjacentHTML('beforeend', next.map(item).join(''));
      shown += next.length;
      const left = hits.length - shown;
      more.hidden = left <= 0;
      more.textContent = `عرض المزيد (${fmt(left)} متبقٍ)`;
    };
    const run = () => {
      const qs = norm(fq.trim()).split(/\s+/).filter(Boolean);
      hits = [];
      LIB.rows.forEach((r, i) => {
        if (!CAT.kpiStatus || CAT.kpiStatus(i) !== 'published') return;
        if (fc !== '' && r[0] !== +fc) return;
        if (fm !== '' && r[4] !== +fm) return;
        if (qs.length && !qs.every(w => idx[i].includes(w))) return;
        hits.push(i);
      });
      list.innerHTML = ''; shown = 0;
      count.innerHTML = hits.length
        ? `<b class="num">${fmt(hits.length)}</b> مؤشر${fc !== '' ? ` في «${LIB.cats[+fc][0]}»` : ''}`
        : `لا توجد نتائج — جرّب كلمة أخرى أو اسأل جيم في تبويب «بناء اللوحة».`;
      page();
    };
    let t;
    ctrl.querySelector('input').addEventListener('input', e => { fq = e.target.value; clearTimeout(t); t = setTimeout(run, 120); });
    ctrl.querySelector('select').addEventListener('change', e => { fc = e.target.value; run(); });
    ctrl.querySelectorAll('[data-mt]').forEach(b => b.addEventListener('click', () => {
      fm = b.dataset.mt;
      ctrl.querySelectorAll('[data-mt]').forEach(x => { const on = x === b; x.classList.toggle('is-on', on); x.setAttribute('aria-pressed', String(on)); });
      run();
    }));
    more.addEventListener('click', page);
    list.addEventListener('click', e => {
      const b = e.target.closest('.kpi-item'); if (!b) return;
      const i = +b.dataset.k;
      addKpi(i, () => { added.delete(i); const el = list.querySelector(`[data-k="${i}"]`); if (el) { el.classList.remove('is-added'); el.querySelector('.kpi-plus').innerHTML = ic('plus'); } });
      added.add(i); b.classList.add('is-added'); b.querySelector('.kpi-plus').innerHTML = ic('check');
    });
    run();
  }

  /* ---- عنصر المؤشر على اللوحة: شكله حسب نوع القياس، وقيمه تجريبية ثابتة لكل مؤشر ---- */
  const seed = str => { let h = 2166136261; for (const ch of str) h = Math.imul(h ^ ch.charCodeAt(0), 16777619); return () => ((h = Math.imul(h ^ (h >>> 15), 2246822507) ^ Math.imul(h ^ (h >>> 13), 3266489909)) >>> 0) / 4294967296; };
  const spark = (rnd, color, up = true) => {
    const pts = Array.from({ length: 7 }, (_, i) => { const base = up ? 24 - i * 3 : 6 + i * 3; return `${i * 20},${Math.max(3, Math.min(27, base + (rnd() - .5) * 8)).toFixed(1)}`; }).join(' ');
    return `<div class="tpl-spark"><svg viewBox="0 0 120 30" preserveAspectRatio="none"><polyline points="${pts}" fill="none" stroke="${color}" stroke-width="2" vector-effect="non-scaling-stroke"/></svg></div>`;
  };
  const delta = (v, good) => `<span class="delta ${good ? 'up' : 'down'}">${ic(v >= 0 ? 'up' : 'down')}<span class="num">${Math.abs(v).toFixed(1)}%</span></span>`;
  const KR = [
    /* نسبة */ rnd => { const target = 70 + Math.round(rnd() * 25), v = Math.min(99, Math.round(target * (.82 + rnd() * .3))), ok = v >= target;
      return `<div class="value"><span class="num">${v}%</span><small>المستهدف <span class="num">${target}%</span></small></div>
        <div class="kpi-bar" role="img" aria-label="القيمة ${v}% والمستهدف ${target}%"><span style="width:${v}%;background:${ok ? 'var(--ok)' : C1}"></span><i style="inset-inline-start:${target}%" title="المستهدف ${target}%"></i></div>
        <div class="cmp">${ok ? `متجاوز للمستهدف بـ <span class="num">${v - target}</span> نقطة` : `أقل من المستهدف بـ <span class="num">${target - v}</span> نقطة`}</div>`; },
    /* زمن */ rnd => { const v = (1 + rnd() * 14).toFixed(1), d = -(2 + rnd() * 12);
      return `<div class="value"><span class="num">${v}</span><small>يوم في المتوسط</small></div><div class="cmp">${delta(d, true)} عن الربع السابق (الأقل أفضل)</div>${spark(rnd, C2, false)}`; },
    /* مالية */ rnd => { const v = (1 + rnd() * 40).toFixed(1), d = (rnd() - .25) * 20;
      return `<div class="value"><span class="num">${v}</span><small>مليون ر.س</small></div><div class="cmp">${delta(d, d >= 0)} مقارنة بنفس الفترة من 2025</div>${spark(rnd, C1, d >= 0)}`; },
    /* درجة */ rnd => { const v = (3.2 + rnd() * 1.7).toFixed(1), d = (rnd() - .3) * 10;
      return `<div class="value"><span class="num">${v}</span><small>من <span class="num">5</span></small></div><div class="cmp">${delta(d, d >= 0)} عن آخر قياس</div>${spark(rnd, C3, d >= 0)}`; },
    /* عدد */ rnd => { const m = ['أبريل', 'مايو', 'يونيو', 'يوليو', 'أغسطس', 'سبتمبر'], vals = m.map(() => 40 + Math.round(rnd() * 160)), max = Math.max(...vals);
      return `<div class="value"><span class="num">${fmt(vals[5])}</span><small>في سبتمبر</small></div>
        <div class="mini-cols">${vals.map((v, i) => `<span title="${m[i]}: ${v}"><i style="height:${(v / max * 100).toFixed(0)}%;${i === 5 ? 'background:' + C1 : ''}"></i><small>${m[i].slice(0, 3)}</small></span>`).join('')}</div>`; },
    /* أخرى */ rnd => { const v = Math.round(20 + rnd() * 80), d = (rnd() - .3) * 12;
      return `<div class="value"><span class="num">${v}</span><small>القيمة الحالية</small></div><div class="cmp">${delta(d, d >= 0)} عن الفترة السابقة</div>${spark(rnd, C4, d >= 0)}`; }
  ];
  function addKpi(i, onRemove) {
    const r = LIB.rows[i], c = LIB.cats[r[0]];
    const w = document.createElement('article');
    w.className = 'widget span-4 kpi is-new';
    w.innerHTML = `
      <div class="w-head"><div><h3>${r[2]}</h3><p dir="ltr" class="w-en">${r[3]}</p></div>
        <div class="w-tools"><button type="button" aria-label="حذف العنصر" data-remove>${ic('trash')}</button></div></div>
      ${KR[r[4]](seed(r[3]))}
      <span class="tpl-src">${ic('target')}مكتبة المؤشرات · ${c[0]} · <span class="tpl-demo">قيم تجريبية — اربطه بمصدر</span></span>`;
    grid.appendChild(w);
    w.querySelector('[data-remove]').addEventListener('click', () => { w.remove(); onRemove(); toast('تم حذف المؤشر من اللوحة'); });
    w.scrollIntoView({ behavior: 'smooth', block: 'center' });
    setTimeout(() => w.classList.remove('is-new'), 2400);
    toast(`تمت إضافة «${r[2]}» للوحة`);
  }
})();
