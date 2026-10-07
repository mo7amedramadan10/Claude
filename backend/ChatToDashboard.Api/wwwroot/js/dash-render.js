/* ==========================================================
   جيم — محرّك رسم لوحات المكتبة (JR)
   يرسم أي لوحة من وصف JSON مختصر: صفحات ← عناصر.
   كل عنصر: { t: النوع, ti: العنوان, su: الوصف, sp: العرض من 12, ...خيارات }
   البيانات تجريبية ومولّدة بشكل ثابت من اسم العنصر (نفس الأرقام كل مرة)،
   وتتغير عند تغيير الفلاتر لمحاكاة التفاعل.
   في التطبيق الفعلي: نفس الوصف + query لكل عنصر ← بيانات حقيقية من الـ API.
   ========================================================== */
(function () {
  const C = ['var(--chart-1)', 'var(--chart-2)', 'var(--chart-3)', 'var(--chart-4)'];
  const OTHER = '#B9C2CB', SOFT = '#BFD9EC';
  const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
  const esc = s => String(s ?? '').replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));

  /* ---------- الأبعاد الجاهزة ---------- */
  const D = {
    M12: ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو', 'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر'],
    W13: Array.from({ length: 13 }, (_, i) => 'أ' + (i + 1)),
    Q4: ['الربع 1', 'الربع 2', 'الربع 3', 'الربع 4'],
    Y5: ['2022', '2023', '2024', '2025', '2026'], Y11: Array.from({ length: 11 }, (_, i) => String(2016 + i)),
    DAY5: ['الأحد', 'الإثنين', 'الثلاثاء', 'الأربعاء', 'الخميس'], DAY7: ['السبت', 'الأحد', 'الإثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة'],
    HR8: ['8ص', '10ص', '12م', '2م', '4م', '6م', '8م', '10م'],
    DEPT: ['العمليات', 'المبيعات', 'تقنية المعلومات', 'المالية', 'الموارد البشرية', 'التسويق', 'المشاريع', 'السلامة'],
    CITY: ['الرياض', 'جدة', 'الدمام', 'مكة المكرمة', 'المدينة المنورة', 'الخبر', 'أبها', 'تبوك'],
    REG: ['الوسطى', 'الغربية', 'الشرقية', 'الجنوبية', 'الشمالية'],
    NAT: ['سعودي', 'مصري', 'هندي', 'باكستاني', 'فلبيني', 'أردني', 'سوداني', 'يمني'],
    GRADE: ['م1', 'م2', 'م3', 'م4', 'م5', 'م6', 'م7', 'م8'],
    AGE: ['أقل من 25', '25–34', '35–44', '45–54', '55+'],
    TEN: ['أقل من سنة', '1–3 سنوات', '3–5 سنوات', '5–10 سنوات', '10+ سنوات'],
    LVL: ['قيادي', 'مدير', 'أخصائي أول', 'أخصائي', 'موظف'],
    CH: ['الفروع', 'المتجر الإلكتروني', 'تطبيق الجوال', 'المناديب', 'الشركاء'],
    SUP: ['شركة الحلول المتقدمة', 'مجموعة الخليج', 'مؤسسة التوريد الأولى', 'شركة البناء الحديث', 'مصنع الشرق', 'شركة النقل السريع', 'التقنية الرقمية', 'مجموعة الفلاح', 'شركة المعادن', 'الشركة الوطنية'],
    PCAT: ['معدات تقنية', 'مواد خام', 'خدمات لوجستية', 'صيانة وقطع غيار', 'خدمات مهنية', 'تغليف', 'معدات سلامة', 'مستلزمات مكتبية'],
    PERSON: ['سارة العتيبي', 'محمد الحربي', 'نورة الشمري', 'يوسف الغامدي', 'ريم القحطاني', 'فهد الدوسري', 'خالد الزهراني', 'منى السبيعي', 'عبدالله المطيري', 'هند العنزي'],
    PRJ: ['منصة الخدمات الموحدة', 'مركز البيانات', 'تطوير البوابة', 'نظام الموارد', 'التحول الرقمي', 'تحديث الشبكة', 'مبنى المقر', 'أتمتة العمليات'],
    SITE: ['المصنع 1', 'المصنع 2', 'المستودع المركزي', 'موقع المشروع', 'المقر الرئيسي', 'محطة الضخ'],
    WH: ['مستودع الرياض', 'مستودع جدة', 'مستودع الدمام', 'مستودع القصيم', 'مستودع أبها'],
    STAT: ['مكتمل', 'قيد التنفيذ', 'متأخر', 'ملغى'],
    /* أبعاد التسويق الرقمي والمبيعات والاشتراكات */
    TRF: ['بحث عضوي', 'مباشر', 'تواصل اجتماعي', 'بحث مدفوع', 'إحالة', 'بريد إلكتروني', 'أخرى'],
    DEVC: ['جوال', 'حاسب مكتبي', 'جهاز لوحي'],
    SOC: ['إنستغرام', 'إكس', 'لينكدإن', 'فيسبوك', 'يوتيوب', 'تيك توك'],
    CNTRY: ['السعودية', 'الإمارات', 'مصر', 'الكويت', 'قطر', 'الأردن', 'البحرين', 'عُمان'],
    PAGE: ['/الرئيسية', '/الأسعار', '/المدونة', '/المنتجات', '/الحلول', '/تواصل-معنا', '/من-نحن', '/الوظائف'],
    KW: ['لوحات متابعة', 'تحليل بيانات', 'ذكاء الأعمال', 'تقارير الأداء', 'مؤشرات الأداء', 'برنامج تقارير', 'تحليلات المبيعات', 'لوحة مؤشرات عربية'],
    CMPN: ['حملة رمضان', 'اليوم الوطني', 'العودة للمدارس', 'الجمعة البيضاء', 'إطلاق المنتج', 'إعادة الاستهداف', 'العلامة التجارية', 'العروض الموسمية'],
    LS: ['مشترك', 'عميل محتمل', 'مؤهل تسويقيًا', 'مؤهل للبيع', 'فرصة', 'عميل'],
    DST: ['اكتشاف', 'تأهيل', 'عرض سعر', 'تفاوض', 'فوز'],
    PLAN: ['الأساسية', 'الاحترافية', 'الأعمال', 'المؤسسات'],
    AGEM: ['18–24', '25–34', '35–44', '45–54', '55+'],
    GEN: ['ذكور', 'إناث'],
    PROD: ['المنتج أ', 'المنتج ب', 'المنتج ج', 'المنتج د', 'المنتج هـ', 'المنتج و', 'المنتج ز', 'المنتج ح'],
    AGENT: ['سارة العتيبي', 'محمد الحربي', 'نورة الشمري', 'يوسف الغامدي', 'ريم القحطاني', 'فهد الدوسري']
  };
  const dim = s => { if (Array.isArray(s)) return s; if (!s) return []; const [k, n] = String(s).split(':'); const a = D[k] || []; return n ? a.slice(0, +n) : a; };

  /* ---------- أرقام ثابتة من البذرة ---------- */
  const seed = str => { let h = 2166136261 >>> 0; for (const ch of str) h = Math.imul(h ^ ch.charCodeAt(0), 16777619); return () => { h = Math.imul(h ^ (h >>> 15), 2246822507); h = Math.imul(h ^ (h >>> 13), 3266489909); h ^= h >>> 16; return (h >>> 0) / 4294967296; }; };
  const walk = (r, n, base, vol = .1, tr = .015) => { let v = base; return Array.from({ length: n }, () => (v = Math.max(base * .1, v * (1 + tr + (r() - .5) * vol * 2)))); };
  const ranked = (r, n, base, dec = .8) => { let v = base; return Array.from({ length: n }, () => { const x = v * (.86 + r() * .28); v *= dec; return x; }).sort((a, b) => b - a); };
  const parts = (r, n) => { const a = Array.from({ length: n }, (_, i) => (1 / (i + 1.25)) * (.7 + r() * .6)); const s = a.reduce((x, y) => x + y, 0); return a.map(x => x / s * 100).sort((x, y) => y - x); };

  /* ---------- التنسيق ---------- */
  const nf = (v, d = 0) => Number(v).toLocaleString('en-US', { maximumFractionDigits: d, minimumFractionDigits: d });
  const fmt = (v, k) => {
    const a = Math.abs(v);
    switch (k) {
      case 'pct': return nf(v, 1) + '%';
      case 'sar': return a >= 1e9 ? nf(v / 1e9, 2) + ' مليار' : a >= 1e6 ? nf(v / 1e6, 1) + 'م' : a >= 1e4 ? nf(v / 1e3, 0) + ' ألف' : nf(v, 0);
      case 'd': case 'x': case 's': return nf(v, 1);
      case 'r': return nf(v, 2);
      default: return a >= 1e6 ? nf(v / 1e6, 1) + 'م' : nf(v, a < 10 && v % 1 ? 1 : 0);
    }
  };
  const UNIT = { sar: 'ر.س', d: 'يوم', h: 'ساعة', min: 'دقيقة' };
  const deltaHtml = (d, good) => { const up = !String(d).trim().startsWith('-'); return `<span class="delta ${good ? 'up' : 'down'}">${ic(up ? 'up' : 'down')}<span class="num">${esc(String(d).replace(/^[+-]/, ''))}</span></span>`; };
  const legend = (names, cols) => `<div class="jr-legend">${names.map((n, i) => `<span><i style="background:${cols ? cols[i] : C[i] || OTHER}"></i>${esc(n)}</span>`).join('')}</div>`;
  const tip = (a, b) => ` data-tip="${esc(a)}${b != null ? '|' + esc(b) : ''}"`;

  /* ---------- الرسوم ---------- */
  const SVGW = 600, SVGH = 180;
  /* خط/مساحة: الزمن يُقرأ من اليمين لليسار (اتجاه الواجهة) */
  const linePath = (vals, min, max, h = SVGH) => vals.map((v, i) => `${(SVGW - i * SVGW / Math.max(1, vals.length - 1)).toFixed(1)},${(h - 8 - (v - min) / (max - min || 1) * (h - 24)).toFixed(1)}`);
  function lineChart(w, r, { area = false, cum = false } = {}) {
    const x = dim(w.x || 'M12'), names = w.s || (w.ly ? ['هذا العام', 'العام السابق'] : [w.ti]);
    const series = names.map((_, i) => { let v = walk(r, x.length, (w.b || 100) * (1 - i * .12), w.vol || .1, w.tr ?? .015); if (cum) { let s = 0; v = v.map(y => (s += y)); } return v; });
    const all = series.flat(), max = Math.max(...all) * 1.05, min = area || cum ? 0 : Math.min(...all) * .9;
    const paths = series.map((v, i) => { const p = linePath(v, min, max); const col = i === 1 && w.ly ? OTHER : C[i];
      return (area && i === 0 ? `<path d="M${p.join(' L')} L0,${SVGH} L${SVGW},${SVGH}Z" fill="${col}" opacity=".12"/>` : '') +
        `<polyline points="${p.join(' ')}" fill="none" stroke="${col}" stroke-width="2" ${i === 1 && w.ly ? 'stroke-dasharray="5 4"' : ''} vector-effect="non-scaling-stroke" stroke-linejoin="round"/>`; }).join('');
    const tg = w.tg != null ? (() => { const y = (SVGH - 8 - (w.tg - min) / (max - min) * (SVGH - 24)).toFixed(1); return `<line x1="0" x2="${SVGW}" y1="${y}" y2="${y}" stroke="var(--ink-3)" stroke-dasharray="3 3" vector-effect="non-scaling-stroke"/>`; })() : '';
    const k = w.k, last = series[0][series[0].length - 1], mx = Math.max(...series[0]), mi = Math.min(...series[0]);
    const cols = x.map((lab, i) => `<i${tip(lab, names.map((n, j) => `${n}: ${fmt(series[j][i], k)}`).join(' · '))}></i>`).join('');
    return `${names.length > 1 ? legend(names, names.map((_, i) => i === 1 && w.ly ? OTHER : C[i])) : ''}
      <div class="jr-plot"><div class="jr-yl"><span class="num">${fmt(max, k)}</span><span class="num">${fmt(min, k)}</span></div>
        <div class="jr-area"><svg viewBox="0 0 ${SVGW} ${SVGH}" preserveAspectRatio="none">${tg}${paths}</svg><div class="jr-hit">${cols}</div></div></div>
      <div class="jr-xl"><span>${esc(x[0])}</span><span>${esc(x[Math.floor(x.length / 2)])}</span><span>${esc(x[x.length - 1])}</span></div>
      <div class="jr-foot">الأخير <b class="num">${fmt(last, k)}</b> · الأعلى <b class="num">${fmt(mx, k)}</b> · الأدنى <b class="num">${fmt(mi, k)}</b>${w.tg != null ? ` · المستهدف <b class="num">${fmt(w.tg, k)}</b>` : ''}</div>`;
  }
  /* أعمدة رأسية (مفردة/مجمّعة/مكدّسة) */
  function colChart(w, r, mode) {
    const x = dim(w.x || 'M12'), names = mode === 'single' ? [w.ti] : (w.s || ['السابق', 'الحالي']).slice(0, 4);
    const data = names.map((_, i) => x.map(() => (w.b || 100) * (.45 + r() * .7) * (mode === 'stack' ? 1 / (i + 1) : 1 - i * .08)));
    const tot = x.map((_, j) => mode === 'stack' ? data.reduce((s, s2) => s + s2[j], 0) : Math.max(...data.map(d => d[j])));
    const max = Math.max(...tot) * 1.08, k = w.k, hiIdx = mode === 'single' ? tot.indexOf(Math.max(...tot)) : -1;
    const cols = (i) => mode === 'stack' ? C[i] : names.length === 2 && w.s ? [SOFT, C[0]][i] : C[i];
    const showVal = x.length <= 8;
    return `${names.length > 1 ? legend(names, names.map((_, i) => cols(i))) : ''}
      <div class="jr-cols ${mode}">${x.map((lab, j) => `<div class="jr-col"${tip(lab, names.map((n, i) => `${n}: ${fmt(data[i][j], k)}`).join(' · '))}>
        ${showVal || j === hiIdx ? `<b class="num">${fmt(tot[j], k)}</b>` : '<b></b>'}
        <div class="jr-bars">${mode === 'stack'
          ? `<div class="jr-stack" style="height:${(tot[j] / max * 100).toFixed(1)}%">${data.map((d, i) => `<i style="flex:${d[j].toFixed(2)};background:${cols(i)}"></i>`).join('')}</div>`
          : data.map((d, i) => `<i style="height:${(d[j] / max * 100).toFixed(1)}%;background:${mode === 'single' ? (j === hiIdx ? C[0] : SOFT) : cols(i)}"></i>`).join('')}</div>
        <span>${esc(lab)}</span></div>`).join('')}</div>`;
  }
  /* أشرطة أفقية مرتبة */
  function hbarChart(w, r, { rank = false } = {}) {
    const x = dim(w.x || 'DEPT:6'), k = w.k, v = (w.ns ? x.map(() => (w.b || 100) * (.4 + r() * .7)) : ranked(r, x.length, w.b || 100, w.dec || .82));
    if (k === 'pct' && !w.b) v.forEach((_, i) => v[i] = Math.min(99, 55 + r() * 42)); if (k === 'pct' && !w.ns) v.sort((a, b) => b - a);
    const max = (k === 'pct' && !w.b ? 100 : Math.max(...v) * 1.05), sum = v.reduce((a, b) => a + b, 0);
    return `<ul class="jr-hbars">${x.map((lab, i) => `<li${tip(lab, fmt(v[i], k) + (w.u ? ' ' + w.u : ''))}>${rank ? `<em class="num">${i + 1}</em>` : ''}<span class="lbl">${esc(lab)}</span><span class="trk"><span class="fill" style="width:${(v[i] / max * 100).toFixed(1)}%;${i && !w.mono ? 'opacity:.78' : ''}"></span>${w.tg ? `<i class="tgt" style="inset-inline-start:${w.tg}%"></i>` : ''}</span><span class="val num">${fmt(v[i], k)}${rank && k !== 'pct' ? `<small>${nf(v[i] / sum * 100, 1)}%</small>` : ''}</span></li>`).join('')}</ul>`;
  }
  /* مقارنة العام السابق بالحالي لكل بند */
  function compareChart(w, r) {
    const x = dim(w.x || 'SUP:6'), k = w.k;
    const ly = ranked(r, x.length, w.b || 100, .85), cy = ly.map(v => v * (.75 + r() * .55)), max = Math.max(...ly, ...cy) * 1.05;
    return `${legend(['العام السابق', 'هذا العام'], [SOFT, C[0]])}<ul class="jr-cmp">${x.map((lab, i) => { const d = (cy[i] / ly[i] - 1) * 100; return `<li${tip(lab, `السابق ${fmt(ly[i], k)} · الحالي ${fmt(cy[i], k)}`)}><span class="lbl">${esc(lab)}</span>
      <span class="pair"><i style="width:${(ly[i] / max * 100).toFixed(1)}%;background:${SOFT}"></i><i style="width:${(cy[i] / max * 100).toFixed(1)}%"></i></span>
      <span class="val num">${fmt(cy[i], k)}</span><span class="d ${(d >= 0) === !w.inv ? 'good' : 'bad'} num">${d >= 0 ? '+' : ''}${nf(d, 1)}%</span></li>`; }).join('')}</ul>`;
  }
  /* 100% مكدّس أفقي */
  function stack100(w, r) {
    const x = dim(w.x || 'DEPT:6'), names = (w.s || ['سعودي', 'غير سعودي']).slice(0, 4);
    return `${legend(names)}<ul class="jr-s100">${x.map(lab => { const p = parts(r, names.length); if (r() > .5 && names.length === 2) p.reverse(); return `<li${tip(lab, names.map((n, i) => `${n}: ${nf(p[i], 1)}%`).join(' · '))}><span class="lbl">${esc(lab)}</span><span class="trk">${p.map((v, i) => `<i style="flex:${v.toFixed(2)};background:${C[i]}">${v > 14 ? `<b class="num">${nf(v, 0)}%</b>` : ''}</i>`).join('')}</span></li>`; }).join('')}</ul>`;
  }
  /* دائرة توزيع: 4 فئات كحد أقصى + «أخرى» */
  function donut(w, r) {
    let x = dim(w.x || 'CH:4'); let p = parts(r, x.length);
    if (x.length > 5) { p = p.slice(0, 4).concat([p.slice(4).reduce((a, b) => a + b, 0)]); x = x.slice(0, 4).concat(['أخرى']); }
    const cols = x.map((n, i) => n === 'أخرى' ? OTHER : C[i]); const R = 40, L = 2 * Math.PI * R; let acc = 0;
    const segs = p.map((v, i) => { const len = v / 100 * L - 2; const s = `<circle cx="50" cy="50" r="${R}" fill="none" stroke="${cols[i]}" stroke-width="14" stroke-dasharray="${Math.max(0, len).toFixed(2)} ${L}" stroke-dashoffset="${(-acc).toFixed(2)}" transform="rotate(-90 50 50)"${tip(x[i], nf(v, 1) + '%')}/>`; acc += v / 100 * L; return s; }).join('');
    const tot = w.tot || (w.b ? fmt(w.b, w.k) : '');
    return `<div class="jr-donut"><div class="jr-dring"><svg viewBox="0 0 100 100">${segs}</svg>${tot ? `<span><b class="num">${esc(tot)}</b>${esc(w.tl || 'الإجمالي')}</span>` : ''}</div>
      <ul>${x.map((n, i) => `<li><i style="background:${cols[i]}"></i><span>${esc(n)}</span><b class="num">${nf(p[i], 1)}%</b></li>`).join('')}</ul></div>`;
  }
  /* حلقة إنجاز */
  function ring(w, r) {
    const v = w.v ?? Math.round(60 + r() * 35), R = 42, L = 2 * Math.PI * R, tone = w.tone || (v >= (w.tg || 80) ? 'var(--ok)' : C[0]);
    return `<div class="jr-ring"><div class="jr-rc"><svg viewBox="0 0 100 100"><circle cx="50" cy="50" r="${R}" fill="none" stroke="var(--line-2)" stroke-width="9"/><circle cx="50" cy="50" r="${R}" fill="none" stroke="${tone}" stroke-width="9" stroke-linecap="round" stroke-dasharray="${(v / 100 * L).toFixed(1)} ${L}" transform="rotate(-90 50 50)"/></svg><span><b class="num">${esc(w.vl || nf(v, w.dp ?? 1) + (w.vu ?? '%'))}</b>${esc(w.lab || '')}</span></div>
      ${w.it ? `<ul class="jr-ritems">${w.it.map(([a, b]) => `<li><span>${esc(a)}</span><b class="num">${esc(b)}</b></li>`).join('')}</ul>` : ''}</div>`;
  }
  /* عدادات نصف دائرية مقابل مستهدف */
  function gauges(w, r) {
    const x = dim(w.x || 'DEPT:5'), tg = w.tg ?? 80;
    return `<div class="jr-gauges">${x.map(lab => { const v = Math.round((tg - 14 + r() * 24) * 10) / 10, a = Math.min(v, 100) / 100, ta = tg / 100;
      const arc = f => { const ang = Math.PI * (1 - f); return `${(50 + 40 * Math.cos(ang)).toFixed(2)},${(50 - 40 * Math.sin(ang)).toFixed(2)}`; };
      const tA = Math.PI * (1 - ta);
      return `<div class="jr-g"${tip(lab, `${nf(v, 1)}% · المستهدف ${tg}%`)}><svg viewBox="0 0 100 58"><path d="M10,50 A40,40 0 0 1 90,50" fill="none" stroke="var(--line-2)" stroke-width="10"/><path d="M10,50 A40,40 0 0 1 ${arc(a)}" fill="none" stroke="${v >= tg ? 'var(--ok)' : C[0]}" stroke-width="10"/><line x1="${(50 + 33 * Math.cos(tA)).toFixed(1)}" y1="${(50 - 33 * Math.sin(tA)).toFixed(1)}" x2="${(50 + 47 * Math.cos(tA)).toFixed(1)}" y2="${(50 - 47 * Math.sin(tA)).toFixed(1)}" stroke="var(--ink)" stroke-width="2"/></svg><b class="num">${nf(v, 1)}%</b><span>${esc(lab)}</span></div>`; }).join('')}</div>`;
  }
  function funnel(w, r) {
    const x = dim(w.x); let v = w.b || 1000; const vals = x.map((_, i) => i ? (v = v * (.48 + r() * .35)) : v);
    return `<div class="jr-funnel">${x.map((lab, i) => `<div class="fr"${tip(lab, fmt(vals[i], w.k))}><span class="fb" style="width:${Math.max(18, vals[i] / vals[0] * 100).toFixed(1)}%;${i === x.length - 1 ? 'background:var(--chart-2)' : ''}"><b class="num">${fmt(vals[i], w.k)}</b></span><span class="fl">${esc(lab)}${i ? `<small class="num">${nf(vals[i] / vals[i - 1] * 100, 0)}%</small>` : ''}</span></div>`).join('')}</div>`;
  }
  /* خريطة حرارية / مصفوفة أرقام */
  function heat(w, r) {
    const rows = dim(w.rows || 'DAY5'), cols = dim(w.cols || 'HR8'), k = w.k;
    const vals = rows.map(() => cols.map(() => (w.b || 100) * (.15 + r() * .85))), max = Math.max(...vals.flat());
    return `<div class="jr-heat${w.num ? ' num-mode' : ''}" style="grid-template-columns:minmax(72px,auto) repeat(${cols.length},minmax(0,1fr))"><span></span>${cols.map(c => `<span class="h">${esc(c)}</span>`).join('')}
      ${rows.map((rl, i) => `<span class="d">${esc(rl)}</span>${vals[i].map((v, j) => `<i style="--o:${(.12 + v / max * .88).toFixed(2)}"${tip(rl + ' · ' + cols[j], fmt(v, k))}>${w.num ? `<b class="num">${fmt(v, k)}</b>` : ''}</i>`).join('')}`).join('')}</div>
      <div class="jr-scale"><span>أقل</span><i></i><span>أعلى</span></div>`;
  }
  function riskMatrix(w, r) {
    const L = ['نادر', 'غير مرجح', 'ممكن', 'مرجح', 'شبه مؤكد'], S = ['طفيف', 'محدود', 'متوسط', 'كبير', 'حرج'];
    return `<div class="jr-risk"><span class="ax-y">الاحتمال</span><div class="grid5">${[4, 3, 2, 1, 0].map(li => `<span class="rl">${L[li]}</span>${S.map((_, si) => { const sc = (li + 1) * (si + 1), n = Math.max(0, Math.round((r() * 9) * (sc > 12 ? .5 : 1)));
      return `<i class="${sc >= 15 ? 'hi' : sc >= 8 ? 'md' : 'lo'}"${tip(`${L[li]} × ${S[si]}`, n + ' حالة')}><b class="num">${n || ''}</b></i>`; }).join('')}`).join('')}<span></span>${S.map(s => `<span class="cl">${s}</span>`).join('')}</div><span class="ax-x">الأثر</span></div>`;
  }
  function waterfall(w, r) {
    const x = dim(w.x), b = w.b || 100; let run = b; const steps = x.map((lab, i) => { if (i === 0) return { lab, s: 0, e: b, tot: 1 }; if (i === x.length - 1) return { lab, s: 0, e: run, tot: 1 }; const d = (w.sg ? w.sg[i - 1] : (r() > .45 ? -1 : 1)) * b * (w.sg && w.sg.every(x => x < 0) ? .04 + r() * .12 : .05 + r() * .22); const s = run; run += d; return { lab, s, e: run, d }; });
    const max = Math.max(...steps.map(s => Math.max(s.s, s.e))) * 1.08;
    return `<div class="jr-wf">${steps.map(s => { const lo = Math.min(s.s, s.e), hi = Math.max(s.s, s.e); return `<div class="jr-col"${tip(s.lab, s.tot ? fmt(s.e, w.k) : (s.d >= 0 ? '+' : '') + fmt(s.d, w.k))}><b class="num">${s.tot ? fmt(s.e, w.k) : (s.d >= 0 ? '+' : '−') + fmt(Math.abs(s.d), w.k)}</b><div class="jr-bars"><i style="bottom:${(lo / max * 100).toFixed(1)}%;height:${Math.max(1.5, (hi - lo) / max * 100).toFixed(1)}%;background:${s.tot ? C[0] : s.d >= 0 ? 'var(--chart-2)' : 'var(--danger)'}"></i></div><span>${esc(s.lab)}</span></div>`; }).join('')}</div>`;
  }
  function diverging(w, r) {
    const x = dim(w.x || 'DEPT:6'), k = w.k || 'pct', v = x.map(() => (r() - .45) * (w.b || 20)).sort((a, b) => b - a), max = Math.max(...v.map(Math.abs)) * 1.1;
    return `<ul class="jr-div">${x.map((lab, i) => `<li${tip(lab, (v[i] >= 0 ? '+' : '') + fmt(v[i], k))}><span class="lbl">${esc(lab)}</span><span class="trk"><span class="neg">${v[i] < 0 ? `<i style="width:${(-v[i] / max * 100).toFixed(1)}%"></i>` : ''}</span><span class="pos">${v[i] >= 0 ? `<i style="width:${(v[i] / max * 100).toFixed(1)}%"></i>` : ''}</span></span><span class="val num ${(v[i] >= 0) === !w.inv ? 'good' : 'bad'}">${v[i] >= 0 ? '+' : ''}${fmt(v[i], k)}</span></li>`).join('')}</ul>`;
  }
  /* انتشار / فقاعات: الأماكن بالنسب داخل مربع (بلا تمطيط للنقاط) */
  function scatter(w, r, bubble) {
    const groups = (w.s || [w.ti]).slice(0, 4), names = bubble ? dim(w.x || 'PRJ:8') : null, n = w.n || 40;
    const pts = bubble ? names.map((nm, i) => ({ g: 0, x: 8 + r() * 84, y: 8 + r() * 84, z: 10 + r() * 22, nm })) : Array.from({ length: n }, () => { const g = Math.floor(r() * groups.length), x = 5 + r() * 90; return { g, x, y: Math.min(95, Math.max(5, x * .75 + (r() - .5) * 35 + g * 6)), z: 9 }; });
    return `${!bubble && groups.length > 1 ? legend(groups) : ''}<div class="jr-sc${bubble ? ' quad' : ''}">${bubble ? '<b class="q q1">متأخر ومكلف</b><b class="q q2">متأخر وموفّر</b><b class="q q3">في الموعد ومكلف</b><b class="q q4">في الموعد وموفّر</b>' : ''}
      ${pts.map(p => `<i style="inset-inline-start:${p.x.toFixed(1)}%;bottom:${p.y.toFixed(1)}%;width:${p.z.toFixed(0)}px;height:${p.z.toFixed(0)}px;background:${C[p.g]}"${tip(p.nm || groups[p.g], `${w.xl || 'س'}: ${nf(p.x, 0)} · ${w.yl || 'ص'}: ${nf(p.y, 0)}`)}></i>`).join('')}</div>
      <div class="jr-xl"><span>${esc(w.yl || '')} ↑</span><span>${esc(w.xl || '')} ←</span></div>`;
  }
  /* جدول: أعمدة بأنواع، مع شريط أو خط صغير أو وسم حالة */
  function table(w, r) {
    const cols = w.c || [['البند', 'l'], ['القيمة', 'sar'], ['التغيّر', 'dp']], rows = w.rows ? dim(w.rows) : dim('DEPT:6'), n = Math.min(rows.length, w.n || 8);
    const colMax = {}; const cell = (kind, ci, ri) => {
      const rr = seed(w.ti + ci + '|' + ri);
      if (kind[0] === '@') { const a = dim(kind.slice(1)); return `<td>${esc(a[(ri * 3 + ci) % a.length])}</td>`; }
      switch (kind) {
        case 'l': return `<td>${esc(rows[ri])}</td>`;
        case 'p': return `<td>${esc(dim('PERSON')[ri % 10])}</td>`;
        case 'dt': return `<td class="num">2026-0${1 + (ri % 9)}-${String(10 + ri).slice(-2)}</td>`;
        case 'id': return `<td class="num mono">${esc((w.idp || 'ID-') + (1001 + ri))}</td>`;
        case 'dp': { const d = (rr() - .35) * 30; return `<td><span class="num ${d >= 0 ? 'good' : 'bad'}">${d >= 0 ? '+' : ''}${nf(d, 1)}%</span></td>`; }
        case 'st': { const sts = w.sts || [['في المسار', 'ok'], ['تحت المراقبة', 'warn'], ['متأخر', 'bad']]; const s = sts[Math.floor(rr() * sts.length)]; return `<td><span class="jr-tag ${s[1]}">${esc(s[0])}</span></td>`; }
        case 'sp': { const v = walk(rr, 8, 50); const mx = Math.max(...v), mi = Math.min(...v); return `<td><svg class="jr-sp" viewBox="0 0 60 18" preserveAspectRatio="none"><polyline points="${v.map((y, i) => `${60 - i * 60 / 7},${(16 - (y - mi) / (mx - mi || 1) * 14).toFixed(1)}`).join(' ')}" fill="none" stroke="var(--chart-1)" stroke-width="1.5" vector-effect="non-scaling-stroke"/></svg></td>`; }
        case 'bar': { const v = 30 + rr() * 70; return `<td><span class="jr-cbar"><i style="width:${v.toFixed(0)}%"></i></span><span class="num">${nf(v, 0)}%</span></td>`; }
        default: { const base = { sar: 1e6 * (1 + rr() * 9), n: 20 + rr() * 900, pct: 40 + rr() * 58, d: 2 + rr() * 40, r: 1 + rr() * 4, s: 2.5 + rr() * 2.4 }[kind] ?? 100 * rr(); const v = base * (1 - ri * .06);
          return `<td><span class="num">${kind === 'pct' ? nf(v, 1) + '%' : kind === 'r' || kind === 's' ? nf(v, 2) : fmt(v, kind)}</span></td>`; }
      }
    };
    return `<div class="jr-tw"><table class="table jr-table"><thead><tr>${cols.map(([h]) => `<th>${esc(h)}</th>`).join('')}</tr></thead><tbody>${Array.from({ length: n }, (_, ri) => `<tr>${cols.map(([, k], ci) => cell(k, ci, ri)).join('')}</tr>`).join('')}</tbody>${w.tot ? `<tfoot><tr><td>الإجمالي</td>${cols.slice(1).map(() => '<td></td>').join('')}</tr></tfoot>` : ''}</table></div>`;
  }
  function cohort(w, r) {
    const rows = dim(w.rows || ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو']), n = w.cols || 6;
    return `<div class="jr-heat num-mode" style="grid-template-columns:minmax(72px,auto) repeat(${n},minmax(0,1fr))"><span></span>${Array.from({ length: n }, (_, i) => `<span class="h">ش${i}</span>`).join('')}
      ${rows.map((rl, ri) => { let v = 100; return `<span class="d">${esc(rl)}</span>${Array.from({ length: n }, (_, ci) => { if (ci >= n - ri) return '<i class="na"></i>'; if (ci) v *= .72 + r() * .2; return `<i style="--o:${(.12 + v / 100 * .88).toFixed(2)}"${tip(rl + ' · ش' + ci, nf(v, 1) + '%')}><b class="num">${nf(v, 0)}%</b></i>`; }).join('')}`; }).join('')}</div>`;
  }
  function insights(w) {
    return `<div class="jr-ins">${(w.it || []).map(([h, tx, v, tone]) => `<div class="jr-in ${tone || ''}"><b>${esc(h)}</b>${v ? `<strong class="num">${esc(v)}</strong>` : ''}<span>${esc(tx)}</span></div>`).join('')}</div>`;
  }
  function combo(w, r) {
    const bars = colChart(Object.assign({}, w, { s: null }), r, 'single');
    const x = dim(w.x || 'M12'), v = walk(r, x.length, w.lb || 50, .08, .005), p = linePath(v, Math.min(...v) * .9, Math.max(...v) * 1.05, 60);
    return `${legend(w.s || [w.ti, 'المعدل'], [C[0], C[1]])}${bars}<div class="jr-strip"><span class="jr-sl">${esc((w.s || [])[1] || 'المعدل')} <b class="num">${fmt(v[v.length - 1], w.lk)}</b></span><svg viewBox="0 0 ${SVGW} 60" preserveAspectRatio="none"><polyline points="${p.join(' ')}" fill="none" stroke="var(--chart-2)" stroke-width="2" vector-effect="non-scaling-stroke"/></svg></div>`;
  }
  /* ---------- المؤشرات ---------- */
  function kpi(w, r) {
    const d = w.d ?? ((r() - .3) * 16 >= 0 ? '+' : '-') + nf(Math.abs((r() - .3) * 16), 1) + '%';
    const good = w.g ?? !String(d).startsWith('-');
    const sp = w.spk ? (() => { const v = walk(r, 10, 50, .12, good ? .02 : -.02); const mx = Math.max(...v), mi = Math.min(...v); return `<svg class="jr-ksp" viewBox="0 0 100 24" preserveAspectRatio="none"><polyline points="${v.map((y, i) => `${100 - i * 100 / 9},${(22 - (y - mi) / (mx - mi || 1) * 20).toFixed(1)}`).join(' ')}" fill="none" stroke="var(--chart-1)" stroke-width="1.5" vector-effect="non-scaling-stroke"/></svg>`; })() : '';
    return `<div class="jr-kpi${w.hl ? ' hl' : ''}"><span class="lab">${w.ic ? ic(w.ic) : ''}${esc(w.ti)}</span><div class="v"><span class="num">${esc(w.v)}</span>${w.u ? `<small>${esc(w.u)}</small>` : ''}</div>
      ${w.d === '' ? (w.n ? `<div class="cmp">${esc(w.n)}</div>` : '') : `<div class="cmp">${deltaHtml(d, good)} ${esc(w.n || 'عن الفترة السابقة')}</div>`}${sp}</div>`;
  }
  function kpiyoy(w) {
    const good = w.g ?? !String(w.d).startsWith('-');
    return `<div class="jr-kpi yoy"><span class="lab">${esc(w.ti)}</span><div class="yy"><span><small>العام السابق</small><b class="num">${esc(w.ly)}</b></span><span><small>هذا العام</small><b class="num big">${esc(w.cy)}</b></span></div><div class="cmp"><span class="jr-tag ${good ? 'ok' : 'bad'} num">${esc(w.d)}</span> ${esc(w.n || '')}</div></div>`;
  }
  function hero(w) {
    return `<div class="jr-hero"><span class="lab">${esc(w.ti)}</span><div class="v"><span class="num">${esc(w.v)}</span>${w.u ? `<small>${esc(w.u)}</small>` : ''}</div>${(w.it || []).map(([a, b]) => `<div class="hi"><small>${esc(a)}</small><b class="num">${esc(b)}</b></div>`).join('')}</div>`;
  }
  function note(w) { return `<div class="jr-note">${(w.tx || []).map(t => `<p>${esc(t)}</p>`).join('')}</div>`; }

  const R = {
    line: (w, r) => lineChart(w, r), area: (w, r) => lineChart(w, r, { area: true }), cum: (w, r) => lineChart(w, r, { area: true, cum: true }),
    bars: (w, r) => colChart(w, r, 'single'), grouped: (w, r) => colChart(w, r, 'group'), stacked: (w, r) => colChart(w, r, 'stack'),
    hbars: (w, r) => hbarChart(w, r), rank: (w, r) => hbarChart(w, r, { rank: true }), compare: compareChart, s100: stack100,
    donut, ring, gauges, funnel, heat, risk: riskMatrix, waterfall, div: diverging, scatter: (w, r) => scatter(w, r, false), bubble: (w, r) => scatter(w, r, true),
    table, cohort, insights, combo, note
  };
  const DEF = { line: 8, area: 8, cum: 6, combo: 8, bars: 6, grouped: 6, stacked: 6, hbars: 4, rank: 4, compare: 6, s100: 6, donut: 4, ring: 4, gauges: 12, funnel: 4, heat: 6, risk: 4, waterfall: 6, div: 6, scatter: 6, bubble: 6, table: 6, cohort: 6, insights: 6, note: 4, hero: 4 };
  const SPANS = [2, 3, 4, 5, 6, 7, 8, 9, 10, 12];
  const snap = n => SPANS.reduce((a, b) => Math.abs(b - n) < Math.abs(a - n) ? b : a);

  /* ---------- تجميع الصفحة: صفوف المؤشرات + شبكة 12 عمودًا، وآخر عنصر في كل صف يتمدد ليكمله ---------- */
  const isK = w => w.t === 'kpi' || w.t === 'kpiyoy';
  function renderPage(d, page, salt = '') {
    const rs = w => seed(d.id + '|' + page.n + '|' + w.ti + '|' + salt), html = [], runs = [];
    page.w.forEach(w => { const k = isK(w), last = runs[runs.length - 1]; if (last && last.k === k) last.ws.push(w); else runs.push({ k, ws: [w] }); });
    runs.forEach(run => {
      if (run.k) { html.push(`<div class="jr-kpis span-12" style="--n:${Math.min(run.ws.length, 8)}">${run.ws.map(w => w.t === 'kpi' ? kpi(w, rs(w)) : kpiyoy(w)).join('')}</div>`); return; }
      const rows = []; let row = [], used = 0;
      run.ws.forEach(w => { const sp = w.sp || DEF[w.t] || 6; if (used + sp > 12 && row.length) { rows.push(row); row = []; used = 0; } row.push({ w, sp }); used += sp; });
      if (row.length) rows.push(row);
      rows.forEach(rw => { const tot = rw.reduce((a, b) => a + b.sp, 0); if (tot < 12) rw[rw.length - 1].sp += 12 - tot;
        rw.forEach(({ w, sp }) => html.push(w.t === 'hero'
          ? `<div class="span-${snap(sp)} jr-cell">${hero(w)}</div>`
          : `<article class="widget jr-w span-${snap(sp)}" data-type="${w.t}"><div class="w-head"><div><h3>${esc(w.ti)}</h3>${w.su ? `<p>${esc(w.su)}</p>` : ''}</div></div><div class="jr-body">${R[w.t] ? R[w.t](w, rs(w)) : ''}</div></article>`)); });
    });
    return html.join('');
  }

  /* ---------- تركيب لوحة كاملة: عنوان + صفحات + فلاتر ---------- */
  function mount(d, grid, { bar = null, onPage = null, startPage = 0 } = {}) {
    let pi = startPage, salt = '';
    const sel = {};
    const host = bar || (() => { let b = grid.parentElement.querySelector(':scope > .jr-bar'); if (!b) { b = document.createElement('div'); b.className = 'jr-bar'; grid.before(b); } return b; })();
    const draw = () => {
      host.innerHTML = `${d.p.length > 1 ? `<div class="jr-tabs" role="tablist" aria-label="صفحات اللوحة">${d.p.map((p, i) => `<button type="button" role="tab" aria-selected="${i === pi}" data-pg="${i}">${esc(p.n)}</button>`).join('')}</div>` : ''}
        ${(d.f || []).length ? `<div class="jr-filters" role="group" aria-label="فلاتر اللوحة">${d.f.map(([lab, opts], fi) => `<div class="jr-f"><span>${esc(lab)}</span><div>${opts.map(o => `<button type="button" class="chip${sel[fi] === o ? ' is-on' : ''}" data-f="${fi}" data-o="${esc(o)}" aria-pressed="${sel[fi] === o}">${esc(o)}</button>`).join('')}</div></div>`).join('')}${Object.keys(sel).length ? `<button type="button" class="btn btn-ghost btn-sm" data-clear>${ic('x')}مسح الفلاتر</button>` : ''}</div>` : ''}`;
      grid.innerHTML = renderPage(d, d.p[pi], salt);
      host.querySelectorAll('[data-pg]').forEach(b => b.addEventListener('click', () => { pi = +b.dataset.pg; draw(); onPage && onPage(pi); }));
      host.querySelectorAll('[data-f]').forEach(b => b.addEventListener('click', () => { const fi = b.dataset.f, o = b.dataset.o; if (sel[fi] === o) delete sel[fi]; else sel[fi] = o; salt = JSON.stringify(sel); draw(); }));
      host.querySelector('[data-clear]')?.addEventListener('click', () => { for (const k in sel) delete sel[k]; salt = ''; draw(); });
    };
    draw();
    return { setPage: i => { pi = i; draw(); } };
  }

  /* ---------- تلميح عام ---------- */
  let tipEl;
  document.addEventListener('mouseover', e => {
    const t = e.target.closest('[data-tip]'); if (!t) { tipEl && (tipEl.hidden = true); return; }
    if (!tipEl) { tipEl = document.createElement('div'); tipEl.className = 'jr-tip'; tipEl.setAttribute('role', 'tooltip'); document.body.appendChild(tipEl); }
    const [a, b] = t.dataset.tip.split('|'); tipEl.innerHTML = `<b>${esc(a)}</b>${b ? `<span>${esc(b)}</span>` : ''}`; tipEl.hidden = false;
    const rc = t.getBoundingClientRect(); tipEl.style.left = Math.min(innerWidth - tipEl.offsetWidth - 8, Math.max(8, rc.left + rc.width / 2 - tipEl.offsetWidth / 2)) + 'px'; tipEl.style.top = Math.max(8, rc.top - tipEl.offsetHeight - 8) + 'px';
  });
  document.addEventListener('scroll', () => tipEl && (tipEl.hidden = true), true);

  /* مصغّر: أنواع العناصر ← رموز المعرض */
  const GLYPH_OF = { line: 'line', area: 'line', cum: 'line', combo: 'line', bars: 'cbars', grouped: 'cbars', stacked: 'cbars', waterfall: 'cbars', hbars: 'hbars', rank: 'hbars', compare: 'hbars', s100: 'hbars', div: 'hbars', donut: 'donut', ring: 'donut', gauges: 'donut', funnel: 'funnel', heat: 'heat', cohort: 'heat', risk: 'heat', scatter: 'heat', bubble: 'heat', table: 'table', insights: 'table', note: 'table', hero: 'kpi', kpi: 'kpi', kpiyoy: 'kpi' };
  const thumbItems = d => {
    const ws = d.p[0].w, ks = ws.filter(isK).slice(0, 4), cs = ws.filter(w => !isK(w));
    const out = ks.map(w => ({ type: 'kpi', span: 12 / ks.length, title: w.ti }));
    let row = [], used = 0; cs.forEach(w => { const sp = w.sp || DEF[w.t] || 6; if (used + sp > 12) { row[row.length - 1].span += 12 - used; out.push(...row); row = []; used = 0; } row.push({ type: GLYPH_OF[w.t] || 'table', span: sp, title: w.ti }); used += sp; });
    if (row.length) { row[row.length - 1].span += 12 - used; out.push(...row); }
    return out.map(o => Object.assign(o, { span: snap(o.span) }));
  };

  window.JR = { renderPage, mount, thumbItems, dim, glyphOf: t => GLYPH_OF[t] || 'table', TYPES: Object.keys(R).concat(['kpi', 'kpiyoy', 'hero']) };
})();
