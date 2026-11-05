// معاينة تفاعلية ببيانات تجريبية لنماذج «تصميم حر» — للمعرض فقط، لا تُشحن في أي بناء حقيقي.
// يبني لكل نموذج صفحة HTML كاملة من أقسامه (بالكلمات المفتاحية في عناوين الأقسام) بأرقام توضيحية ثابتة،
// بنفس نوع المخرج: مستند ورقي، شرائح بتنقّل، صفحة تصميم تفاعلية، لوحة بفلاتر، أو مشهد متحرك بتحكّم.
(function () {
  'use strict';
  const esc = s => String(s ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[c]);
  const KIND_COLOR = { doc: ['#2A5BD7', '#0B3D5C'], slides: ['#9A4A15', '#5C2E0B'], design: ['#6A3FC9', '#3C2A6E'], dashboard: ['#B3261E', '#5C1612'], motion: ['#0E7A5C', '#0B4A3C'] };
  const NAMES = ['الرياض', 'جدة', 'الدمام', 'مكة', 'المدينة', 'تبوك'];
  const MONTHS = ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو'];
  const PEOPLE = ['سارة العتيبي', 'محمد القحطاني', 'نورة الشهري', 'خالد الدوسري', 'ريم الحربي'];
  const fmt = n => Number(n).toLocaleString('en-US');
  // أرقام حتمية من بذرة نصية حتى تبقى المعاينة ثابتة بين الفتحات
  function rng(seed) { let h = 2166136261; for (const c of seed) { h ^= c.charCodeAt(0); h = Math.imul(h, 16777619); } return () => { h = Math.imul(h ^ (h >>> 15), 2246822507); h = Math.imul(h ^ (h >>> 13), 3266489909); return ((h ^= h >>> 16) >>> 0) / 4294967296; }; }
  const kw = (t, re) => re.test(t);

  // ---------- مكوّنات مشتركة ----------
  function kpis(r, n = 4) {
    const labels = ['الإيراد', 'الصفقات', 'نسبة الإنجاز', 'رضا العملاء', 'المتأخر', 'التذاكر'];
    return `<div class="kpis">${Array.from({ length: n }, (_, i) => { const v = i === 2 ? Math.round(60 + r() * 35) + '٪' : i === 3 ? (4 + r()).toFixed(1) + ' / 5' : fmt(Math.round(r() * 900 + 100) * (i === 0 ? 1000 : 1)); const d = Math.round(r() * 30 - 10); return `<div class="kpi"><small>${labels[i]}</small><b>${v}</b><span class="${d >= 0 ? 'up' : 'dn'}">${d >= 0 ? '▲' : '▼'} ${Math.abs(d)}٪ عن الفترة السابقة</span></div>`; }).join('')}</div>`;
  }
  function bars(r, title) {
    const vals = NAMES.map(() => Math.round(r() * 80 + 20)); const max = Math.max(...vals);
    return `<figure class="chart"><figcaption>${esc(title || 'التوزيع')}</figcaption><svg viewBox="0 0 320 150" role="img" aria-label="${esc(title || 'التوزيع')}">${vals.map((v, i) => `<rect x="${12 + i * 52}" y="${130 - v}" width="36" height="${v}" rx="4" class="bar"/><text x="${30 + i * 52}" y="${124 - v}" text-anchor="middle" class="val">${v}</text><text x="${30 + i * 52}" y="145" text-anchor="middle" class="lbl">${NAMES[i]}</text>`).join('')}</svg></figure>`;
  }
  function line(r, title) {
    const vals = MONTHS.map(() => Math.round(r() * 70 + 30)); const pts = vals.map((v, i) => `${20 + i * 56},${130 - v}`).join(' ');
    return `<figure class="chart"><figcaption>${esc(title || 'الاتجاه')}</figcaption><svg viewBox="0 0 320 150"><polyline points="${pts}" class="line"/>${vals.map((v, i) => `<circle cx="${20 + i * 56}" cy="${130 - v}" r="4" class="dot"/><text x="${20 + i * 56}" y="145" text-anchor="middle" class="lbl">${MONTHS[i]}</text>`).join('')}</svg></figure>`;
  }
  function donut(r, title) {
    const parts = [['مفتوح', 0], ['قيد التنفيذ', 0], ['مكتمل', 0]].map(([l]) => [l, Math.round(r() * 40 + 10)]); const tot = parts.reduce((a, p) => a + p[1], 0); let acc = 0;
    const cols = ['#2A78D6', '#E0A100', '#0E8F86'];
    return `<figure class="chart"><figcaption>${esc(title || 'التوزيع بالحالة')}</figcaption><div class="donut"><svg viewBox="0 0 120 120">${parts.map((p, i) => { const a0 = acc / tot * 2 * Math.PI, a1 = (acc + p[1]) / tot * 2 * Math.PI; acc += p[1]; const x0 = 60 + 50 * Math.cos(a0), y0 = 60 + 50 * Math.sin(a0), x1 = 60 + 50 * Math.cos(a1), y1 = 60 + 50 * Math.sin(a1); return `<path d="M60 60 L${x0} ${y0} A50 50 0 ${a1 - a0 > Math.PI ? 1 : 0} 1 ${x1} ${y1} Z" fill="${cols[i]}"/>`; }).join('')}<circle cx="60" cy="60" r="28" fill="#fff"/><text x="60" y="65" text-anchor="middle" class="val">${tot}</text></svg><ul>${parts.map((p, i) => `<li><i style="background:${cols[i]}"></i>${p[0]} <b>${p[1]}</b></li>`).join('')}</ul></div></figure>`;
  }
  function table(r, title) {
    return `<table><caption>${esc(title || 'التفاصيل')}</caption><thead><tr><th>البند</th><th>المسؤول</th><th>القيمة</th><th>الحالة</th></tr></thead><tbody>${PEOPLE.map((p, i) => { const s = ['✓ مكتمل', '● قيد التنفيذ', '! متأخر'][i % 3]; return `<tr><td>بند ${i + 1}</td><td>${p}</td><td>${fmt(Math.round(r() * 9000 + 1000))}</td><td class="s${i % 3}">${s}</td></tr>`; }).join('')}</tbody></table>`;
  }
  function tasks(r, title) {
    const items = ['مراجعة المتطلبات مع الفريق', 'اعتماد الميزانية', 'تجهيز بيئة الاختبار', 'إرسال التقرير للإدارة'];
    return `<div class="tasks"><h3>${esc(title || 'المهام')}</h3>${items.map((t, i) => `<label><input type="checkbox" ${i === 0 ? 'checked' : ''}> <span>${t}</span> <small>${PEOPLE[i]} · ${15 + i} نوفمبر</small></label>`).join('')}<p class="hint">✎ جرّب التعليم — في الصفحة الحقيقية تُحفظ عبر jeem.store</p></div>`;
  }
  function cards(r, title) {
    const items = ['السرعة', 'الاعتمادية', 'الدعم', 'التكامل', 'الأمان', 'التقارير'];
    return `<div class="grid">${items.slice(0, 6).map((t, i) => `<div class="card"><span class="ic">${['⚡', '🛡️', '💬', '🔗', '🔒', '📈'][i]}</span><b>${t}</b><p>نص توضيحي قصير يشرح هذه النقطة بإيجاز.</p></div>`).join('')}</div>`;
  }
  function timeline(r) {
    const steps = ['الانطلاق', 'التحليل', 'التصميم', 'التنفيذ', 'الإطلاق'];
    return `<div class="tl">${steps.map((s, i) => `<button type="button" class="tl-s${i === 0 ? ' on' : ''}" data-i="${i}"><i></i><b>${s}</b><small>${MONTHS[i]} 2026</small></button>`).join('')}</div><div class="tl-detail" id="tl-detail">المحطة: الانطلاق — اجتماع البدء واعتماد النطاق. (اضغط محطة أخرى)</div>`;
  }
  function faq() {
    return ['كيف أبدأ؟', 'ما المدة المتوقعة؟', 'كيف أطلب الدعم؟', 'هل يمكن التعديل لاحقًا؟'].map((q, i) => `<details${i === 0 ? ' open' : ''}><summary>${q}</summary><p>إجابة توضيحية مختصرة لهذا السؤال، تُكتب في الصفحة الحقيقية من محتواك.</p></details>`).join('');
  }
  function form() {
    return `<form class="frm" onsubmit="event.preventDefault();document.getElementById('frm-ok').hidden=false"><label>الاسم <input required placeholder="الاسم الكامل"></label><label>الإدارة <select><option>المبيعات</option><option>التقنية</option><option>المالية</option></select></label><label>التاريخ <input type="date"></label><label>التفاصيل <textarea rows="2" placeholder="اكتب التفاصيل"></textarea></label><button type="submit">إرسال</button><p id="frm-ok" class="ok" hidden>✓ أُرسل الطلب (توضيحي)</p></form>`;
  }
  function bullets() { return `<ul class="bl"><li>نقطة أولى توضيحية تلخّص فكرة القسم.</li><li>نقطة ثانية بتفصيل مختصر.</li><li>نقطة ثالثة بإجراء أو توصية.</li></ul>`; }
  function para() { return `<p>نص توضيحي يمثّل محتوى هذا القسم. في الصفحة الحقيقية يكتبه جيم من طلبك وبيانات مشروعك، بهوية مشروعك وألوانه.</p>`; }

  // محتوى القسم من عنوانه
  function sectionBody(s, r) {
    const t = s.title;
    if (kw(t, /مؤشر|أرقام|نتائج|عدّاد|الستة|بارزة/)) return kpis(r);
    if (kw(t, /اتجاه|مقابل|إنجاز|يومي|شهري|رسم/)) return line(r, t);
    if (kw(t, /توزيع|تركيبة|بالإدارة|بالبند|بالفئة/)) return kw(t, /حالة|تركيبة/) ? donut(r, t) : bars(r, t);
    if (kw(t, /جدول|تفاصيل|شواغر|مواعيد|تذاكر|حضور|تكلفة|تعريفات|مسؤوليات|سجل|عملاؤنا/)) return table(r, t);
    if (kw(t, /مهام|متابعة|خطوات|أجندة|جدول الأعمال|خطة/)) return tasks(r, t);
    if (kw(t, /أسئلة|الأقسام والأسئلة/)) return faq();
    if (kw(t, /نموذج|بيانات الطلب|المرفقات|المراجعة والإرسال|التواصل/)) return form();
    if (kw(t, /فريق|خدمات|ميزات|بطاقات|شبكة|أهداف|وحدة|لماذا/)) return cards(r, t);
    if (kw(t, /زمني|محطات|المحطة/)) return timeline(r);
    return para() + bullets();
  }

  const CSS = (c, kind) => `
    *{box-sizing:border-box} body{margin:0;font-family:"IBM Plex Sans Arabic",Tahoma,system-ui,sans-serif;color:#1B2733;background:${kind === 'doc' ? '#E9EEF2' : '#fff'};line-height:1.7}
    h1,h2,h3{margin:0 0 .4em;color:${c[1]}} h2{font-size:20px;border-inline-start:4px solid ${c[0]};padding-inline-start:10px;margin-top:28px}
    .kpis{display:grid;grid-template-columns:repeat(auto-fit,minmax(150px,1fr));gap:12px;margin:12px 0}
    .kpi{background:#fff;border:1px solid #E3E8EE;border-top:3px solid ${c[0]};border-radius:10px;padding:12px}.kpi small{color:#5A6B77}.kpi b{display:block;font-size:24px;margin:4px 0}.kpi span{font-size:12px}.up{color:#0E8F4F}.dn{color:#B3261E}
    .chart{margin:12px 0;background:#fff;border:1px solid #E3E8EE;border-radius:10px;padding:10px}.chart figcaption{font-weight:600;margin-bottom:6px}.chart svg{width:100%;max-height:220px;display:block}
    .bar{fill:${c[0]}}.val{font-size:11px;fill:#1B2733}.lbl{font-size:10px;fill:#5A6B77}.line{fill:none;stroke:${c[0]};stroke-width:2.5}.dot{fill:#fff;stroke:${c[0]};stroke-width:2}
    .donut{display:flex;gap:16px;align-items:center}.donut svg{width:140px;flex:none}.donut ul{list-style:none;padding:0;margin:0}.donut li{display:flex;gap:8px;align-items:center}.donut i{width:12px;height:12px;border-radius:3px;display:inline-block}
    table{width:100%;border-collapse:collapse;margin:12px 0;background:#fff;font-size:14px}caption{text-align:start;font-weight:600;padding:6px 0}th,td{border-bottom:1px solid #E3E8EE;padding:8px;text-align:start}th{background:#F4F7FA}.s0{color:#0E8F4F}.s1{color:#B07800}.s2{color:#B3261E}
    .tasks{background:#fff;border:1px solid #E3E8EE;border-radius:10px;padding:12px}.tasks label{display:flex;gap:8px;align-items:center;padding:6px 0;border-bottom:1px dashed #E3E8EE}.tasks label:has(:checked) span{text-decoration:line-through;color:#8A98A5}.tasks small{margin-inline-start:auto;color:#8A98A5}.hint{font-size:12px;color:#8A98A5;margin:8px 0 0}
    .grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(170px,1fr));gap:12px}.card{background:#fff;border:1px solid #E3E8EE;border-radius:12px;padding:14px}.card .ic{font-size:24px}.card p{font-size:13px;color:#5A6B77;margin:4px 0 0}
    .tl{display:flex;gap:6px;overflow:auto;padding:10px 0}.tl-s{all:unset;cursor:pointer;flex:1;min-width:110px;text-align:center;padding:8px;border-radius:10px}.tl-s i{display:block;width:14px;height:14px;border-radius:50%;background:#C9D4DE;margin:0 auto 6px}.tl-s.on i{background:${c[0]}}.tl-s.on{background:#F4F7FA}.tl-s b{display:block}.tl-s small{color:#8A98A5}.tl-detail{background:#F4F7FA;border-radius:10px;padding:12px}
    details{background:#fff;border:1px solid #E3E8EE;border-radius:10px;padding:8px 12px;margin:6px 0}summary{cursor:pointer;font-weight:600}
    .frm{display:grid;gap:10px;max-width:520px}.frm label{display:grid;gap:4px;font-size:14px}.frm input,.frm select,.frm textarea{font:inherit;padding:8px;border:1px solid #C9D4DE;border-radius:8px}.frm button{font:inherit;background:${c[0]};color:#fff;border:0;border-radius:8px;padding:10px;cursor:pointer}.ok{color:#0E8F4F;font-weight:600}
    .bl li{margin:4px 0}.badge{display:inline-block;background:#FFF4CC;color:#7A5800;border:1px solid #F2D77A;border-radius:999px;padding:2px 10px;font-size:12px}
    @media(prefers-reduced-motion:reduce){*{animation:none!important;transition:none!important}}`;

  // ---------- أنواع المخرجات ----------
  function docPage(t, r, c) {
    const body = t.sections.map(s => kw(s.title, /غلاف|ترويسة|الموضوع/) ? '' : `<h2>${esc(s.title)}</h2>${sectionBody(s, r)}`).join('');
    return `<style>${CSS(c, 'doc')} .paper{max-width:820px;margin:24px auto;background:#fff;padding:40px 48px;border-radius:8px;box-shadow:0 4px 24px rgba(16,32,48,.12)}.hd{display:flex;justify-content:space-between;border-bottom:2px solid ${c[0]};padding-bottom:12px;margin-bottom:20px;color:#5A6B77;font-size:13px}.ft{margin-top:30px;padding-top:10px;border-top:1px solid #E3E8EE;color:#8A98A5;font-size:12px;display:flex;justify-content:space-between}@page{size:A4;margin:16mm}</style>
      <div class="paper"><div class="hd"><span><b style="color:${c[1]}">شركة الأفق للتقنية</b><br>الإدارة العامة</span><span>المرجع: ${t.key.toUpperCase()}-26<br>التاريخ: 5 نوفمبر 2026</span></div>
      <h1>${esc(t.name)}</h1><p class="badge">بيانات توضيحية</p>${body}<div class="ft"><span>سرّي — للاستخدام الداخلي</span><span>صفحة 1</span></div></div>`;
  }
  function slidesPage(t, r, c) {
    const slides = t.sections.map((s, i) => `<section class="slide${i === 0 ? ' cover' : ''}">${i === 0 ? `<h1>${esc(t.name)}</h1><p>${esc(t.description)}</p><p class="badge">بيانات توضيحية</p>` : `<h2>${esc(s.title)}</h2><div class="sc">${sectionBody(s, r)}</div>`}</section>`).join('');
    return `<style>${CSS(c, 'slides')} body{background:#0F1720;overflow:hidden}.deck{position:fixed;inset:0}.slide{position:absolute;inset:0;padding:5vw 6vw;background:#fff;display:none;overflow:auto}.slide.on{display:block}.slide.cover{background:linear-gradient(135deg,${c[1]},${c[0]});color:#fff;display:none;place-content:center;text-align:center}.slide.cover.on{display:grid}.cover h1{color:#fff;font-size:44px}.sc{font-size:17px}
      .nav{position:fixed;bottom:14px;inset-inline:0;display:flex;justify-content:center;gap:10px;align-items:center;color:#fff;z-index:2}.nav button{font:inherit;background:rgba(255,255,255,.15);color:#fff;border:1px solid rgba(255,255,255,.4);border-radius:8px;padding:6px 14px;cursor:pointer}.prog{position:fixed;top:0;inset-inline-start:0;height:4px;background:${c[0]};transition:width .3s;z-index:2}</style>
      <div class="deck">${slides}</div><div class="prog" id="prog"></div><div class="nav"><button type="button" id="p">→ السابق</button><span id="pos"></span><button type="button" id="n">التالي ←</button></div>
      <script>(function(){var s=[].slice.call(document.querySelectorAll('.slide')),i=0;function go(k){i=Math.max(0,Math.min(s.length-1,k));s.forEach(function(e,j){e.classList.toggle('on',j===i)});document.getElementById('pos').textContent=(i+1)+' / '+s.length;document.getElementById('prog').style.width=((i+1)/s.length*100)+'%'}document.getElementById('n').onclick=function(){go(i+1)};document.getElementById('p').onclick=function(){go(i-1)};document.addEventListener('keydown',function(e){if(e.key==='ArrowLeft'||e.key===' ')go(i+1);if(e.key==='ArrowRight')go(i-1)});var x0=null;document.addEventListener('touchstart',function(e){x0=e.touches[0].clientX});document.addEventListener('touchend',function(e){if(x0==null)return;var d=e.changedTouches[0].clientX-x0;if(Math.abs(d)>40)go(d<0?i-1:i+1);x0=null});go(0)})();</script>`;
  }
  function designPage(t, r, c) {
    const isCatalog = /catalog/.test(t.key), isForm = /form/.test(t.key);
    const secs = t.sections.map((s, i) => `<section id="s${i}"><h2>${esc(s.title)}</h2>${isCatalog && i === 1 ? catalog(r) : isForm && i > 0 ? '' : sectionBody(s, r)}</section>`).join('');
    const formSteps = isForm ? `<div class="steps">${t.sections.slice(1).map((s, i) => `<span class="st${i === 0 ? ' on' : ''}">${i + 1}. ${esc(s.title)}</span>`).join('')}</div><div class="stp on">${form()}</div>` : '';
    return `<style>${CSS(c, 'design')} nav{position:sticky;top:0;background:#fff;border-bottom:1px solid #E3E8EE;padding:10px 24px;display:flex;gap:16px;align-items:center;z-index:2}nav b{color:${c[1]};margin-inline-end:auto}nav a{color:#5A6B77;text-decoration:none;font-size:14px}main{max-width:1000px;margin:0 auto;padding:20px 24px}.hero{background:linear-gradient(135deg,${c[1]},${c[0]});color:#fff;border-radius:16px;padding:48px 32px;margin:16px 0}.hero h1{color:#fff;font-size:36px}.hero button{font:inherit;background:#fff;color:${c[1]};border:0;border-radius:10px;padding:10px 18px;cursor:pointer;font-weight:600}
      .steps{display:flex;gap:8px;flex-wrap:wrap;margin:10px 0}.st{padding:6px 12px;border-radius:999px;background:#F4F7FA;color:#8A98A5;font-size:13px}.st.on{background:${c[0]};color:#fff}.flt{display:flex;gap:6px;margin:10px 0;flex-wrap:wrap}.flt button{font:inherit;border:1px solid #C9D4DE;background:#fff;border-radius:999px;padding:4px 12px;cursor:pointer}.flt button.on{background:${c[0]};color:#fff;border-color:${c[0]}}.cat .card[hidden]{display:none}</style>
      <nav><b>${esc(t.name)}</b>${t.sections.slice(0, 4).map((s, i) => `<a href="#s${i}">${esc(s.title)}</a>`).join('')}</nav>
      <main>${/landing|profile|company/.test(t.key) ? `<div class="hero"><h1>${esc(t.name)}</h1><p>${esc(t.description)} — <span class="badge">بيانات توضيحية</span></p><button type="button" onclick="alert('نداء إجراء توضيحي')">ابدأ الآن</button></div>` : `<p class="badge">بيانات توضيحية</p>`}${formSteps}${secs}</main>
      <script>document.querySelectorAll('.tl-s').forEach(function(b){b.onclick=function(){document.querySelectorAll('.tl-s').forEach(function(x){x.classList.remove('on')});b.classList.add('on');document.getElementById('tl-detail').textContent='المحطة: '+b.querySelector('b').textContent+' — تفاصيل توضيحية لهذه المحطة.'}});
      document.querySelectorAll('.flt button').forEach(function(b){b.onclick=function(){document.querySelectorAll('.flt button').forEach(function(x){x.classList.remove('on')});b.classList.add('on');document.querySelectorAll('.cat .card').forEach(function(k){k.hidden=b.dataset.t!=='all'&&k.dataset.t!==b.dataset.t})}});</script>`;
  }
  function catalog(r) {
    const tags = ['أجهزة', 'برمجيات', 'خدمات']; const items = Array.from({ length: 6 }, (_, i) => ({ n: ['حاسوب محمول', 'نظام إدارة', 'صيانة سنوية', 'شاشة عرض', 'تطبيق جوال', 'تدريب'][i], t: tags[i % 3], p: Math.round(r() * 9000 + 500) }));
    return `<div class="flt"><button type="button" class="on" data-t="all">الكل</button>${tags.map(x => `<button type="button" data-t="${x}">${x}</button>`).join('')}</div><div class="grid cat">${items.map(it => `<div class="card" data-t="${it.t}"><span class="ic">📦</span><b>${it.n}</b><p>${it.t} · ${fmt(it.p)} ر.س</p></div>`).join('')}</div>`;
  }
  function dashboardPage(t, r, c) {
    const secs = t.sections.map(s => `<section><h2>${esc(s.title)}</h2>${sectionBody(s, r)}</section>`).join('');
    return `<style>${CSS(c, 'dashboard')} body{background:#F4F7FA}header{background:#fff;border-bottom:1px solid #E3E8EE;padding:12px 24px;display:flex;gap:12px;align-items:center;position:sticky;top:0;z-index:2}header b{color:${c[1]};font-size:18px;margin-inline-end:auto}header select{font:inherit;padding:6px 10px;border:1px solid #C9D4DE;border-radius:8px}main{max-width:1200px;margin:0 auto;padding:16px 24px}section{margin-bottom:8px}h2{font-size:16px;margin-top:16px}</style>
      <header><b>${esc(t.name)}</b><span class="badge">بيانات توضيحية</span><label>الفترة <select id="per"><option>هذا الشهر</option><option>هذا الربع</option><option>هذه السنة</option></select></label><label>المنطقة <select><option>الكل</option>${NAMES.map(n => `<option>${n}</option>`).join('')}</select></label></header>
      <main>${secs}</main>
      <script>document.getElementById('per').onchange=function(){var m=[1,2.8,11][this.selectedIndex];document.querySelectorAll('.kpi b').forEach(function(b){var v=b.dataset.v||(b.dataset.v=b.textContent);var n=parseFloat(v.replace(/,/g,''));if(!isNaN(n)&&!/٪|\\//.test(v))b.textContent=Math.round(n*m).toLocaleString('en-US')})};</script>`;
  }
  function motionPage(t, r, c) {
    const steps = t.sections.filter(s => !/التحكم|المشهد/.test(s.title)); const labels = steps.length ? steps.map(s => s.title) : ['الاستلام', 'التجهيز', 'الشحن', 'التسليم'];
    const n = Math.max(labels.length, 3);
    return `<style>${CSS(c, 'motion')} body{background:#0F1720;color:#fff;display:grid;place-items:center;min-height:100vh}.stage{width:min(900px,94vw)}svg{width:100%;display:block}.ctl{display:flex;gap:10px;justify-content:center;align-items:center;margin-top:12px}.ctl button{font:inherit;background:rgba(255,255,255,.12);color:#fff;border:1px solid rgba(255,255,255,.4);border-radius:8px;padding:6px 14px;cursor:pointer}.pb{height:4px;background:rgba(255,255,255,.2);border-radius:2px;margin-top:10px}.pb i{display:block;height:100%;background:${c[0]};width:0;animation:pb var(--d) linear forwards}
      .n{opacity:0;animation:pop .6s forwards}.n circle{fill:${c[0]}}.n text{fill:#fff;font-size:13px;text-anchor:middle}.cap{opacity:0;animation:pop .6s forwards;font-size:20px;fill:#fff;text-anchor:middle}@keyframes pop{to{opacity:1}}@keyframes pb{to{width:100%}}.path{stroke:rgba(255,255,255,.3);stroke-width:3;fill:none;stroke-dasharray:1000;stroke-dashoffset:1000;animation:dash var(--d) linear forwards}@keyframes dash{to{stroke-dashoffset:0}}.paused *{animation-play-state:paused!important}.cnt{font-size:34px;fill:#fff;text-anchor:middle;font-weight:700}</style>
      <div class="stage" id="st" style="--d:${n * 2.5}s"><h1 style="color:#fff;text-align:center">${esc(t.name)} <span class="badge">توضيحي</span></h1>
      <svg viewBox="0 0 900 260"><path class="path" d="M60 150 ${labels.map((_, i) => `L${60 + (i + 1) * (780 / n)} ${i % 2 ? 100 : 150}`).join(' ')}"/>${labels.map((l, i) => `<g class="n" style="animation-delay:${i * 2.5 + .3}s"><circle cx="${60 + (i + 1) * (780 / n)}" cy="${i % 2 ? 100 : 150}" r="22"/><text x="${60 + (i + 1) * (780 / n)}" y="${i % 2 ? 105 : 155}">${i + 1}</text><text class="cnt" x="${60 + (i + 1) * (780 / n)}" y="${i % 2 ? 60 : 210}">${/stats/.test(t.key) ? fmt(Math.round(r() * 900 + 100)) : ''}</text></g><text class="cap" x="${60 + (i + 1) * (780 / n)}" y="${i % 2 ? 150 : 100}" style="animation-delay:${i * 2.5 + .8}s">${esc(l)}</text>`).join('')}</svg>
      <div class="pb"><i></i></div><div class="ctl"><button type="button" id="pp">⏸ إيقاف</button><button type="button" id="rp">↻ إعادة</button><label><input type="checkbox" id="lp"> تكرار</label></div></div>
      <script>(function(){var st=document.getElementById('st'),p=false;document.getElementById('pp').onclick=function(){p=!p;st.classList.toggle('paused',p);this.textContent=p?'▶ تشغيل':'⏸ إيقاف'};function rs(){var h=st.innerHTML;st.innerHTML=h;}document.getElementById('rp').onclick=rs;st.querySelector('.pb i').addEventListener('animationend',function(){if(document.getElementById('lp').checked)rs()});st.addEventListener('animationend',function(e){if(e.target.matches('.pb i')&&document.getElementById('lp').checked)rs()});})();</script>`;
  }

  function build(t) {
    const r = rng(t.key); const c = KIND_COLOR[t.kind] || KIND_COLOR.design;
    const body = ({ doc: docPage, slides: slidesPage, dashboard: dashboardPage, motion: motionPage })[t.kind] || designPage;
    return `<!doctype html><html lang="ar" dir="rtl"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>${esc(t.name)} — معاينة توضيحية</title></head><body>${body(t, r, c)}</body></html>`;
  }
  window.JEEM_FF_DEMO = { build };
})();
