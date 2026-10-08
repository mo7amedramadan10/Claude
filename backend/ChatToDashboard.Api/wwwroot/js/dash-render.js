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
        `<polyline points="${p.join(' ')}" fill="none" stroke="${col}" stroke-width="2" ${(i === 1 && w.ly) || (w.dash || []).includes(i) ? 'stroke-dasharray="5 4"' : ''} vector-effect="non-scaling-stroke" stroke-linejoin="round"/>`; }).join('');
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
    const max = Math.max(...tot) * 1.08, k = w.k, hiIdx = mode === 'single' ? (w.spot === 'last' ? tot.length - 1 : typeof w.spot === 'number' ? w.spot : tot.indexOf(Math.max(...tot))) : -1;
    const cols = (i) => mode === 'stack' ? C[i] : names.length === 2 && w.s ? [SOFT, C[0]][i] : C[i];
    const showVal = x.length <= 8;
    return `${names.length > 1 ? legend(names, names.map((_, i) => cols(i))) : ''}
      <div class="jr-cols ${mode}">${x.map((lab, j) => `<div class="jr-col"${tip(lab, names.map((n, i) => `${n}: ${fmt(data[i][j], k)}`).join(' · '))}>
        ${showVal || j === hiIdx ? `<b class="num">${fmt(tot[j], k)}</b>` : '<b></b>'}
        <div class="jr-bars">${mode === 'stack'
          ? `<div class="jr-stack" style="height:${(tot[j] / max * 100).toFixed(1)}%">${data.map((d, i) => `<i style="flex:${d[j].toFixed(2)};background:${cols(i)}"></i>`).join('')}</div>`
          : data.map((d, i) => `<i style="height:${(d[j] / max * 100).toFixed(1)}%;background:${mode === 'single' ? (j === hiIdx ? C[0] : w.spot != null ? '#E3E8EE' : SOFT) : cols(i)}"></i>`).join('')}</div>
        <span>${esc(lab)}</span></div>`).join('')}</div>`;
  }
  /* أشرطة أفقية مرتبة */
  function hbarChart(w, r, { rank = false } = {}) {
    const x = dim(w.x || 'DEPT:6'), k = w.k, v = w.vals ? w.vals.slice() : (w.ns ? x.map(() => (w.b || 100) * (.4 + r() * .7)) : ranked(r, x.length, w.b || 100, w.dec || .82));
    if (!w.vals && k === 'pct' && !w.b) v.forEach((_, i) => v[i] = Math.min(99, 55 + r() * 42)); if (!w.vals && k === 'pct' && !w.ns) v.sort((a, b) => b - a);
    const max = (k === 'pct' && !w.b ? 100 : Math.max(...v, 1e-9) * 1.05), sum = v.reduce((a, b) => a + b, 0);
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
    const x = dim(w.x); let v = w.b || 1000; const vals = x.map((_, i) => i ? (v = v * (.48 + r() * .35)) : v).map(y => w.int ? Math.max(1, Math.round(y)) : y);
    return `<div class="jr-funnel">${x.map((lab, i) => `<div class="fr"${tip(lab, fmt(vals[i], w.k))}><span class="fb" style="width:${Math.max(18, vals[i] / vals[0] * 100).toFixed(1)}%;${i === x.length - 1 ? 'background:var(--chart-2)' : ''}"><b class="num">${fmt(vals[i], w.k)}</b></span><span class="fl">${esc(lab)}${i ? `<small class="num">${nf(vals[i] / vals[i - 1] * 100, 0)}%</small>` : ''}</span></div>`).join('')}</div>`;
  }
  /* خريطة حرارية / مصفوفة أرقام */
  function heat(w, r) {
    const rows = dim(w.rows || 'DAY5'), cols = dim(w.cols || 'HR8'), k = w.k;
    const vals = w.vals || rows.map(() => cols.map(() => (w.b || 100) * (.15 + r() * .85))), max = Math.max(...vals.flat(), 1e-9);
    return `<div class="jr-heat${w.num ? ' num-mode' : ''}" style="grid-template-columns:minmax(72px,auto) repeat(${cols.length},minmax(0,1fr))"><span></span>${cols.map(c => `<span class="h">${esc(c)}</span>`).join('')}
      ${rows.map((rl, i) => `<span class="d">${esc(rl)}</span>${vals[i].map((v, j) => `<i style="--o:${v ? (.12 + v / max * .88).toFixed(2) : .03}"${tip(rl + ' · ' + cols[j], fmt(v, k))}>${w.num && v ? `<b class="num">${fmt(v, k)}</b>` : ''}</i>`).join('')}`).join('')}</div>
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
      /* قيم فعلية بدل التجريبية: w.vals[صف][عمود] */
      const fv = w.vals && w.vals[ri] ? w.vals[ri][ci] : null;
      if (fv != null) { if (kind === 'st') { const t = (w.sts || []).find(x => x[0] === fv); return `<td><span class="jr-tag ${t ? t[1] : 'info'}">${esc(fv)}</span></td>`; } return `<td><span class="num${kind === 'id' ? ' mono' : ''}">${esc(fv)}</span></td>`; }
      if (kind[0] === '@') { const a = dim(kind.slice(1)); return `<td>${esc(a[(ri * 3 + ci) % a.length])}</td>`; }
      switch (kind) {
        case 'l': return `<td>${esc(rows[ri])}</td>`;
        case 'p': return `<td>${esc(dim('PERSON')[ri % 10])}</td>`;
        case 'dt': return `<td><span class="num">2026-0${1 + (ri % 9)}-${String(10 + ri).slice(-2)}</span></td>`;
        case 'id': return `<td><span class="num mono">${esc((w.idp || 'ID-') + (1001 + ri))}</span></td>`;
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
      ${w.d === '' ? (w.n ? `<div class="cmp">${esc(w.n)}</div>` : '') : `<div class="cmp">${deltaHtml(d, good)} ${esc(w.n || 'عن الفترة السابقة')}</div>`}${sp}${w.tg != null ? `<div class="jr-ktg ${w.tg >= 100 ? 'ok' : w.tg >= (w.risk || 80) ? 'mid' : 'low'}"><span class="h"><span>المستهدف${w.tgv ? ` <b class="num">${esc(w.tgv)}</b>` : ''}</span><b class="num">${nf(w.tg, 0)}%</b></span><span class="bar"><i style="width:${Math.min(100, w.tg)}%"></i></span>${w.per ? `<small>${ic('calendar')}${esc(w.per)}</small>` : ''}</div>` : ''}</div>`;
  }
  function kpiyoy(w) {
    const good = w.g ?? !String(w.d).startsWith('-');
    return `<div class="jr-kpi yoy"><span class="lab">${esc(w.ti)}</span><div class="yy"><span><small>العام السابق</small><b class="num">${esc(w.ly)}</b></span><span><small>هذا العام</small><b class="num big">${esc(w.cy)}</b></span></div><div class="cmp"><span class="jr-tag ${good ? 'ok' : 'bad'} num">${esc(w.d)}</span> ${esc(w.n || '')}</div></div>`;
  }
  function hero(w) {
    return `<div class="jr-hero"><span class="lab">${esc(w.ti)}</span><div class="v"><span class="num">${esc(w.v)}</span>${w.u ? `<small>${esc(w.u)}</small>` : ''}</div>${(w.it || []).map(([a, b]) => `<div class="hi"><small>${esc(a)}</small><b class="num">${esc(b)}</b></div>`).join('')}</div>`;
  }
  function note(w) { return `<div class="jr-note">${(w.tx || []).map(t => `<p>${esc(t)}</p>`).join('')}</div>`; }
  /* مؤشر رصاصي: الفعلي مقابل المستهدف لكل بند (it: [[الاسم، الفعلي، المستهدف، مفتاح اختياري]]) — اللون حالة: ضمن / قريب / متجاوز */
  function bullet(w) {
    const risk = w.risk || .8, inv = !!w.inv, hi = !!w.hi;
    return `<ul class="wf-bullets">${(w.it || []).map(([lab, v, tg, key]) => { const r = tg ? v / tg : 0, st = hi ? (r >= 1 ? 'ok' : r >= risk ? 'warn' : 'bad') : inv ? (r <= 1 ? 'ok' : 'bad') : (r >= 1 ? 'bad' : r >= risk ? 'warn' : 'ok');
      return `<li${key ? ` data-stage-go="${esc(key)}"` : ''}${tip(lab, `الفعلي ${nf(v, 1)} · المستهدف ${nf(tg, 0)}`)}><span class="lbl">${esc(lab)}</span><span class="trk"><i class="st-${st}" style="width:${(Math.min(2, r) / 2 * 100).toFixed(1)}%"></i><b></b></span><span class="val num">${nf(v, 1)} <small>/ ${nf(tg, 0)}</small></span></li>`; }).join('')}</ul>`;
  }

  /* ================= أنواع جديدة (من مراجعة Consist وNexus) ================= */
  /* رادار: ملف أداء على 5–8 محاور بنفس المقياس، لكيان واحد أو مقارنة 2–3 كيانات فوق بعض.
     w.x المحاور، w.s أسماء السلاسل، w.vals [[قيمة لكل محور] لكل سلسلة] على مقياس w.max (افتراضي 100). */
  function radar(w, r) {
    const ax = dim(w.x || ['الجودة', 'الالتزام بالمواعيد', 'السعر', 'الاستجابة', 'الامتثال', 'الاستدامة']), names = (w.s || [w.ti]).slice(0, 3), mx = w.max || 100, n = ax.length;
    const vals = w.vals || names.map((_, i) => ax.map(() => Math.round(mx * (.45 + r() * .5 - i * .05))));
    const R0 = 78, pt = (i, v) => { const a = -Math.PI / 2 + i * 2 * Math.PI / n; return [Math.cos(a) * R0 * v / mx, Math.sin(a) * R0 * v / mx]; };
    const cols = w.cols || names.map((_, i) => C[i]);
    const grid = [.25, .5, .75, 1].map(f => `<polygon class="g" points="${ax.map((_, i) => pt(i, mx * f).map(c => c.toFixed(1)).join(',')).join(' ')}"/>`).join('')
      + ax.map((_, i) => { const [x, y] = pt(i, mx); return `<line class="g" x1="0" y1="0" x2="${x.toFixed(1)}" y2="${y.toFixed(1)}"/>`; }).join('');
    const labs = ax.map((a, i) => { const [x, y] = pt(i, mx * 1.2), anc = Math.abs(x) < 6 ? 'middle' : x > 0 ? 'start' : 'end';
      return `<text x="${x.toFixed(1)}" y="${(y + 3).toFixed(1)}" text-anchor="${anc}">${esc(a)}${names.length === 1 ? `<tspan class="v" x="${x.toFixed(1)}" dy="12">${nf(vals[0][i])}</tspan>` : ''}</text>`; }).join('');
    const polys = vals.map((vs, s) => `<g class="ser" data-s="${s}" style="--c:${cols[s]};--i:${s}"><polygon points="${vs.map((v, i) => pt(i, v).map(c => c.toFixed(1)).join(',')).join(' ')}"/>${vs.map((v, i) => { const [x, y] = pt(i, v); return `<circle cx="${x.toFixed(1)}" cy="${y.toFixed(1)}" r="2.6"${tip(`${names[s]} · ${ax[i]}`, `${nf(v)} من ${mx}`)}/>`; }).join('')}</g>`).join('');
    return `${names.length > 1 ? legend(names, cols) : ''}<div class="jr-radar"><svg viewBox="-150 -112 300 224" role="img" aria-label="${esc(w.ti)}">${grid}${polys}${labs}</svg></div>`;
  }
  /* نصف دائرة مقسّمة: إجمالي واحد مقسوم لجزئين أو ثلاثة (ليس عدادًا مقابل هدف). w.x الأجزاء، w.vals القيم */
  function semi(w, r) {
    const x = dim(w.x || ['مميز', 'أساسي']).slice(0, 3), v = w.vals || parts(r, x.length).map(p => p * (w.b || 2400) / 100), tot = v.reduce((a, b) => a + b, 0), k = w.k;
    const L = Math.PI * 40; let acc = 0; const cols = w.cols || x.map((_, i) => C[i]);
    const segs = v.map((val, i) => { const len = val / tot * L - (i < v.length - 1 ? 1.6 : 0), s = `<path d="M10,50 A40,40 0 0 1 90,50" class="sg" stroke="${cols[i]}" stroke-dasharray="${Math.max(0, len).toFixed(2)} ${L + 4}" stroke-dashoffset="${(-acc).toFixed(2)}" style="--i:${i}"${tip(x[i], `${fmt(val, k)} · ${nf(val / tot * 100, 1)}%`)}/>`; acc += val / tot * L; return s; }).join('');
    return `<div class="jr-semi"><div class="arc"><svg viewBox="0 0 100 56"><path d="M10,50 A40,40 0 0 1 90,50" class="tr"/>${segs}</svg><span><b class="num">${esc(w.tot || fmt(tot, k))}</b>${esc(w.tl || 'الإجمالي')}</span></div>
      <ul>${x.map((n, i) => `<li style="--c:${cols[i]}"><span>${esc(n)}</span><b class="num">${fmt(v[i], k)}</b><small class="num">${nf(v[i] / tot * 100, 1)}%</small></li>`).join('')}</ul></div>`;
  }
  /* دائرة بعناوين خارجية: 3–5 أجزاء، خط من كل جزء إلى اسمه ونسبته (بدل مفتاح منفصل) */
  function donutCo(w, r) {
    let x = dim(w.x || 'CH:4').slice(0, 5); const p = w.vals ? (() => { const s = w.vals.reduce((a, b) => a + b, 0); return w.vals.map(v => v / s * 100); })() : parts(r, x.length);
    const cols = w.cols || x.map((_, i) => C[i]), R0 = 46, L = 2 * Math.PI * R0; let acc = 0;
    const segs = p.map((v, i) => { const len = v / 100 * L - 2.5, s = `<circle r="${R0}" class="sg" stroke="${cols[i]}" stroke-dasharray="${Math.max(0, len).toFixed(2)} ${L}" stroke-dashoffset="${(-acc).toFixed(2)}" transform="rotate(-90)" style="--i:${i}"${tip(x[i], nf(v, 1) + '%')}/>`; acc += v / 100 * L; return s; }).join('');
    let a0 = 0; const used = [];
    const labs = p.map((v, i) => { const mid = (a0 + v / 2) / 100 * 2 * Math.PI - Math.PI / 2; a0 += v; const c = Math.cos(mid), s = Math.sin(mid), side = c >= 0 ? 1 : -1;
      let ly = s * 72; used.filter(u => u[0] === side).forEach(u => { if (Math.abs(u[1] - ly) < 26) ly = u[1] + (ly >= u[1] ? 26 : -26); }); used.push([side, ly]);
      const x1 = c * (R0 + 10), y1 = s * (R0 + 10), x2 = c * 66, x3 = side * 92;
      return `<g class="co" style="--i:${i}"><polyline points="${x1.toFixed(1)},${y1.toFixed(1)} ${x2.toFixed(1)},${ly.toFixed(1)} ${x3},${ly.toFixed(1)}" stroke="${cols[i]}"/><circle cx="${x1.toFixed(1)}" cy="${y1.toFixed(1)}" r="2.2" fill="${cols[i]}"/>
        <text x="${x3 + side * 4}" y="${(ly - 3).toFixed(1)}" text-anchor="${side > 0 ? 'start' : 'end'}">${esc(x[i])}</text><text class="v" x="${x3 + side * 4}" y="${(ly + 11).toFixed(1)}" text-anchor="${side > 0 ? 'start' : 'end'}">${nf(v, 1)}%</text></g>`; }).join('');
    const tot = w.tot || (w.b ? fmt(w.b, w.k) : '');
    return `<div class="jr-dco"><svg viewBox="-160 -100 320 200" role="img" aria-label="${esc(w.ti)}"><circle r="${R0}" class="tr"/>${segs}${labs}${tot ? `<text class="t" y="2">${esc(tot)}</text><text class="tl" y="16">${esc(w.tl || 'الإجمالي')}</text>` : ''}</svg></div>`;
  }
  /* ترتيب بأيقونة أو رمز: دول، منصات، جهات. w.av رموز قصيرة (SA، AE…) أو تُؤخذ أول حرف */
  function iconRank(w, r) {
    const x = dim(w.x || 'CNTRY:5'), k = w.k, v = w.vals ? w.vals.slice() : ranked(r, x.length, w.b || 900, w.dec || .78), max = Math.max(...v) * 1.04, sum = v.reduce((a, b) => a + b, 0);
    const av = w.av || x.map(n => n.replace('ال', '')[0]);
    return `<ul class="jr-irank">${x.map((lab, i) => `<li${tip(lab, w.nopct ? fmt(v[i], k) + (w.u ? ' ' + w.u : '') : `${fmt(v[i], k)} · ${nf(v[i] / sum * 100, 1)}%`)} style="--i:${i};--h:${(i * 57 + 200) % 360}"><span class="av">${esc(av[i])}</span><span class="m"><span class="t"><span class="lbl">${esc(lab)}</span><span class="val"><b class="num">${fmt(v[i], k)}</b>${w.nopct ? '' : `<small class="num">${nf(v[i] / sum * 100, 1)}%</small>`}</span></span><span class="trk"><span class="fill" style="width:${(v[i] / max * 100).toFixed(1)}%"></span></span></span></li>`).join('')}</ul>`;
  }
  /* أعمدة مكدّسة بشرائط تربط كل فئة بين الفترات: تُظهر كيف تتغير حصة كل فئة. الفترة الأولى يمينًا */
  function ribbon(w, r) {
    const x = dim(w.x || 'Q4').slice(0, 6), names = (w.s || dim('CH:4')).slice(0, 5), k = w.k, n = x.length;
    const data = w.vals || x.map((_, j) => names.map((_, i) => (w.b || 100) * (1 / (i + 1.3)) * (.6 + r() * .8)));
    const tot = data.map(c => c.reduce((a, b) => a + b, 0)), mx = Math.max(...tot) * 1.14, W = 600, H = 230, B = H - 26, slot = (W - 20) / n, cw = Math.min(70, slot * .44);
    const cols = w.cols || names.map((_, i) => [C[0], C[1], C[2], C[3], '#9DD5CA'][i]);
    const G = data.map((c, j) => { let y = B; const cx = W - 10 - slot * (j + .5); return { cx, seg: c.map(v => { const h = v / mx * (B - 22), s = [y - h, y]; y -= h; return s; }) }; });
    let rb = ''; for (let j = 0; j < n - 1; j++) { const a = G[j], b = G[j + 1], xa = a.cx - cw / 2, xb = b.cx + cw / 2, m = (xa + xb) / 2;
      names.forEach((_, i) => { const p = a.seg[i], q = b.seg[i]; rb += `<path class="rb" fill="${cols[i]}" style="--i:${j}" d="M${xa.toFixed(1)},${(p[0] + 1).toFixed(1)} C${m.toFixed(1)},${(p[0] + 1).toFixed(1)} ${m.toFixed(1)},${(q[0] + 1).toFixed(1)} ${xb.toFixed(1)},${(q[0] + 1).toFixed(1)} L${xb.toFixed(1)},${(q[1] - 1).toFixed(1)} C${m.toFixed(1)},${(q[1] - 1).toFixed(1)} ${m.toFixed(1)},${(p[1] - 1).toFixed(1)} ${xa.toFixed(1)},${(p[1] - 1).toFixed(1)}Z"/>`; }); }
    const bars = G.map((g, j) => g.seg.map((s, i) => `<rect class="sg" x="${(g.cx - cw / 2).toFixed(1)}" y="${(s[0] + 1).toFixed(1)}" width="${cw.toFixed(1)}" height="${Math.max(0, s[1] - s[0] - 2).toFixed(1)}" rx="4" fill="${cols[i]}" style="--i:${j * names.length + i}"${tip(`${x[j]} · ${names[i]}`, `${fmt(data[j][i], k)} · ${nf(data[j][i] / tot[j] * 100, 1)}%`)}/>`).join('')
      + `<text class="tl" x="${g.cx.toFixed(1)}" y="${(g.seg[g.seg.length - 1][0] - 7).toFixed(1)}">${fmt(tot[j], k)}</text><text class="xl" x="${g.cx.toFixed(1)}" y="${H - 6}">${esc(x[j])}</text>`).join('');
    return `${legend(names, cols)}<div class="jr-ribbon"><svg viewBox="0 0 ${W} ${H}" role="img" aria-label="${esc(w.ti)}"><line class="bl" x1="10" x2="${W - 10}" y1="${B}" y2="${B}"/>${rb}${bars}</svg></div>`;
  }

  /* ================= أنواع جديدة (من مراجعة SaleGrow) ================= */
  /* بطاقة أشخاص أو فرق: حروف أولى + الرقم + نسبة الهدف (اختياري). w.it [[الاسم، القيمة، النسبة من الهدف؟، الدور؟]] */
  function people(w, r) {
    const it = w.it || dim(w.x || 'PERSON:5').map(n => [n, Math.round((w.b || 100) * (.5 + r() * .6)), Math.round(55 + r() * 55)]);
    const L = it.map((a, i) => ({ n: a[0], v: a[1], p: a[2], role: a[3], i })).sort((a, b) => b.v - a.v), k = w.k;
    const ini = n => n.split(' ').slice(0, 2).map(x => x.replace(/^ال/, '')[0]).join('‌');
    return `<ul class="jr-people">${L.map((o, j) => { const st = o.p == null ? '' : o.p >= 100 ? 'ok' : o.p >= (w.risk || 80) ? 'mid' : 'low';
      return `<li class="${j === 0 ? 'top' : ''}" style="--i:${j};--h:${(o.i * 61 + 190) % 360}" data-person="${esc(o.n)}" tabindex="0" role="button"${tip(o.n, `${fmt(o.v, k)}${w.u ? ' ' + w.u : ''}${o.p != null ? ` · ${o.p}% من الهدف` : ''}`)}>
        ${j === 0 ? `<span class="crown" aria-label="الأعلى">${ic('star')}</span>` : `<span class="rk num">${j + 1}</span>`}<span class="av">${esc(ini(o.n))}</span><b class="nm">${esc(o.n)}</b>${o.role ? `<small>${esc(o.role)}</small>` : ''}
        <span class="v"><span class="num">${fmt(o.v, k)}</span>${w.u ? `<small>${esc(w.u)}</small>` : ''}</span>
        ${o.p != null ? `<span class="pg ${st}"><i style="width:${Math.min(100, o.p)}%"></i></span><small class="pp num">${o.p}%</small>` : ''}</li>`; }).join('')}</ul>`;
  }
  /* قائمة نشاط: آخر المعاملات أو الطلبات بحالة (أيقونة + لون + نص). w.it [[العنوان، الوصف، القيمة، الحالة ok|pend|bad|info، الوقت]] */
  const FEED_ST = { ok: ['check', 'مكتمل'], pend: ['clock', 'معلّق'], bad: ['x', 'مرفوض'], info: ['info', 'جديد'] };
  function feed(w) {
    const it = w.it || [];
    return `<ul class="jr-feed">${it.map(([t, s, v, st, tm, who], i) => { const S = FEED_ST[st] || FEED_ST.info;
      return `<li class="st-${st}" style="--i:${i}"${who ? ` data-who="${esc(who)}"` : ''}><span class="av" aria-hidden="true">${ic(S[0])}</span><span class="m"><b>${esc(t)}</b><small>${esc(s || '')}</small></span><span class="e"><b class="num">${esc(v)}</b><span class="tag">${esc(w.lab && w.lab[st] || S[1])}</span>${tm ? `<small>${esc(tm)}</small>` : ''}</span></li>`; }).join('') || '<li class="none">لا نشاط في هذه الفترة.</li>'}</ul>`;
  }
  /* رسالة الهدف: جملة تُكتب من البيانات وتتغير نبرتها حسب نسبة التحقيق. w.p النسبة، w.per الفترة، w.rem المتبقي، w.who */
  function goal(w) {
    const p = w.p ?? 0, st = p >= 100 ? 'ok' : p >= (w.risk || 80) ? 'mid' : p >= 50 ? 'low' : 'bad';
    const head = { ok: 'تجاوزت الهدف، أداء ممتاز', mid: 'أداء جيد، اقتربت من الهدف', low: 'في المسار، لكن الوتيرة تحتاج تسريعًا', bad: 'بعيد عن الهدف، يلزم تدخل' }[st];
    const icn = { ok: 'check', mid: 'up', low: 'clock', bad: 'info' }[st];
    return `<div class="jr-goal st-${st}"><span class="gi">${ic(icn, 'icon')}</span><div class="gt"><b>${esc(w.head || head)}</b>
      <p>${w.who ? esc(w.who) + ' — ' : ''}حققت <b class="num">${nf(p, 0)}%</b> من هدف ${esc(w.per || 'الفترة')}${w.rem ? `، والمتبقي <b class="num">${esc(w.rem)}</b>` : ''}${w.days ? ` خلال <b class="num">${w.days}</b> يومًا` : ''}.</p>
      <span class="gp"><i style="width:${Math.min(100, p)}%"></i></span></div></div>`;
  }

  /* ---------- الحالة الفارغة لكل نوع: تظهر قبل ربط مصدر البيانات ---------- */
  const EMPTY_SHAPE = {
    line: () => `<svg viewBox="0 0 600 120" preserveAspectRatio="none" class="es-line"><line x1="0" x2="600" y1="30" y2="30"/><line x1="0" x2="600" y1="70" y2="70"/><line class="b" x1="0" x2="600" y1="110" y2="110"/></svg>`,
    cbars: () => `<div class="es-cols">${[40, 62, 30, 75, 50, 66, 38].map(h => `<i style="height:${h}%"></i>`).join('')}</div>`,
    hbars: () => `<div class="es-rows">${[88, 70, 56, 42, 30].map(h => `<i style="width:${h}%"></i>`).join('')}</div>`,
    donut: () => `<svg viewBox="0 0 100 100" class="es-ring"><circle cx="50" cy="50" r="36"/></svg>`,
    semi: () => `<svg viewBox="0 0 100 56" class="es-ring"><path d="M10,50 A40,40 0 0 1 90,50"/></svg>`,
    radar: () => `<svg viewBox="-60 -60 120 120" class="es-web">${[.33, .66, 1].map(f => `<polygon points="${[0, 1, 2, 3, 4, 5].map(i => { const a = -Math.PI / 2 + i * Math.PI / 3; return `${(Math.cos(a) * 50 * f).toFixed(1)},${(Math.sin(a) * 50 * f).toFixed(1)}`; }).join(' ')}"/>`).join('')}</svg>`,
    table: () => `<div class="es-rows t">${[0, 1, 2, 3].map(() => '<i></i>').join('')}</div>`,
    funnel: () => `<div class="es-rows c">${[90, 72, 54, 36].map(h => `<i style="width:${h}%"></i>`).join('')}</div>`,
    heat: () => `<div class="es-heat">${Array.from({ length: 24 }, () => '<i></i>').join('')}</div>`
  };
  const emptyKind = t => ({ radar: 'radar', semi: 'semi', iconrank: 'hbars', ribbon: 'cbars', bullet: 'hbars', people: 'table', feed: 'table', goal: 'hbars' })[t] || (GLYPH_OF[t] === 'kpi' ? 'kpi' : GLYPH_OF[t]) || 'table';
  function emptyBody(w) {
    const k = emptyKind(w.t), shape = (EMPTY_SHAPE[k] || EMPTY_SHAPE.table)();
    return `<div class="jr-empty" data-kind="${k}"><div class="es-shape" aria-hidden="true">${shape}</div><div class="es-msg">${ic('database')}<b>لا توجد بيانات بعد</b><small>اربط مصدرًا أو اعرض ببيانات تجريبية</small></div></div>`;
  }

  const R = {
    line: (w, r) => lineChart(w, r), area: (w, r) => lineChart(w, r, { area: true }), cum: (w, r) => lineChart(w, r, { area: true, cum: true }),
    bars: (w, r) => colChart(w, r, 'single'), grouped: (w, r) => colChart(w, r, 'group'), stacked: (w, r) => colChart(w, r, 'stack'),
    hbars: (w, r) => hbarChart(w, r), rank: (w, r) => hbarChart(w, r, { rank: true }), compare: compareChart, s100: stack100,
    donut, ring, gauges, funnel, heat, risk: riskMatrix, waterfall, div: diverging, scatter: (w, r) => scatter(w, r, false), bubble: (w, r) => scatter(w, r, true),
    table, cohort, insights, combo, note, bullet, radar, semi, iconrank: iconRank, ribbon,
    dco: donutCo, people, feed, goal
  };
  const DEF = { line: 8, area: 8, cum: 6, combo: 8, bars: 6, grouped: 6, stacked: 6, hbars: 4, rank: 4, compare: 6, s100: 6, donut: 4, ring: 4, gauges: 12, funnel: 4, heat: 6, risk: 4, waterfall: 6, div: 6, scatter: 6, bubble: 6, table: 6, cohort: 6, insights: 6, note: 4, hero: 4, bullet: 6, radar: 6, semi: 4, iconrank: 4, ribbon: 8, dco: 4, people: 8, feed: 4, goal: 4 };
  const SPANS = [2, 3, 4, 5, 6, 7, 8, 9, 10, 12];
  const snap = n => SPANS.reduce((a, b) => Math.abs(b - n) < Math.abs(a - n) ? b : a);

  /* ---------- تجميع الصفحة: صفوف المؤشرات + شبكة 12 عمودًا، وآخر عنصر في كل صف يتمدد ليكمله ---------- */
  const isK = w => w.t === 'kpi' || w.t === 'kpiyoy';
  function renderPage(d, page, salt = '', opt = {}) {
    const rs = w => seed(d.id + '|' + page.n + '|' + w.ti + '|' + salt), html = [], runs = [];
    page.w.forEach(w => { const k = isK(w), last = runs[runs.length - 1]; if (last && last.k === k) last.ws.push(w); else runs.push({ k, ws: [w] }); });
    runs.forEach(run => {
      if (run.k) { html.push(`<div class="jr-kpis span-12" style="--n:${Math.min(run.ws.length, 8)}">${run.ws.map(w => opt.empty ? `<div class="jr-kpi is-empty"><span class="lab">${w.ic ? ic(w.ic) : ''}${esc(w.ti)}</span><div class="v"><span class="num">—</span></div><div class="cmp">لا توجد بيانات بعد</div></div>` : w.t === 'kpi' ? kpi(w, rs(w)) : kpiyoy(w)).join('')}</div>`); return; }
      const rows = []; let row = [], used = 0;
      run.ws.forEach(w => { const sp = w.sp || DEF[w.t] || 6; if (used + sp > 12 && row.length) { rows.push(row); row = []; used = 0; } row.push({ w, sp }); used += sp; });
      if (row.length) rows.push(row);
      rows.forEach(rw => { const tot = rw.reduce((a, b) => a + b.sp, 0); if (tot < 12) rw[rw.length - 1].sp += 12 - tot;
        rw.forEach(({ w, sp }) => html.push(w.t === 'hero'
          ? `<div class="span-${snap(sp)} jr-cell">${hero(w)}</div>`
          : `<article class="widget jr-w span-${snap(sp)}" data-type="${w.t}"><div class="w-head"><div><h3>${esc(w.ti)}</h3>${w.su ? `<p>${esc(w.su)}</p>` : ''}</div></div><div class="jr-body">${opt.empty ? emptyBody(w) : R[w.t] ? R[w.t](w, rs(w)) : ''}</div></article>`)); });
    });
    return html.join('');
  }

  /* ---------- حركة الدخول (مشتركة لكل اللوحات) ----------
     عدّ الأرقام في المؤشرات، رسم الخطوط تدريجيًا، اكتساح الحلقات، تتابع ظهور البطاقات.
     تُعطَّل تلقائيًا مع إعداد «تقليل الحركة». */
  const RM = matchMedia('(prefers-reduced-motion: reduce)').matches;
  function countUp(el, dur = 900) {
    const txt = el.textContent, m = txt.match(/-?[\d,]*\.?\d+/); if (!m || RM) return;
    const raw = m[0], dec = (raw.split('.')[1] || '').length, to = parseFloat(raw.replace(/,/g, '')), pre = txt.slice(0, m.index), post = txt.slice(m.index + raw.length), comma = raw.includes(',') || Math.abs(to) >= 1000;
    const t0 = performance.now(), f = v => (comma ? Number(v).toLocaleString('en-US', { minimumFractionDigits: dec, maximumFractionDigits: dec }) : v.toFixed(dec));
    const step = t => { const p = Math.min(1, (t - t0) / dur), e = 1 - Math.pow(1 - p, 3); el.textContent = pre + f(to * e) + post; if (p < 1) requestAnimationFrame(step); else el.textContent = txt; };
    requestAnimationFrame(step);
  }
  /* عدّاد دوّار: كل خانة رقمية تلف من 0 إلى قيمتها (للمؤشرات الكبيرة عند وصول البيانات) */
  function roll(el, dur = 1100) {
    if (RM || el.dataset.rolled) return; const txt = el.textContent; el.dataset.rolled = 1; el.setAttribute('aria-label', txt);
    let di = 0; el.innerHTML = [...txt].map(ch => /\d/.test(ch) ? `<span class="jr-od" aria-hidden="true"><span style="--n:${ch};--d:${(di++ * 70)}ms">${'0123456789'.split('').map(x => `<i>${x}</i>`).join('')}</span></span>` : `<span aria-hidden="true">${esc(ch)}</span>`).join('');
    requestAnimationFrame(() => requestAnimationFrame(() => el.classList.add('go')));
    setTimeout(() => { el.textContent = txt; delete el.dataset.rolled; el.classList.remove('go'); }, dur + di * 70 + 200);
  }
  function animate(root, o = {}) {
    if (!root || RM) return;
    root.querySelectorAll('.jr-kpi:not(.is-empty) .v .num, .jr-hero .v .num, .jr-kpi .yy .big').forEach(el => o.roll ? roll(el) : countUp(el));
    /* كشف المنحنى من اليمين لليسار (اتجاه الزمن) بقصّ بدل طول الخط، لأن pathLength لا يعمل مع non-scaling-stroke */
    root.querySelectorAll('.jr-area polyline, .jr-area path').forEach(pl => pl.classList.add('jr-reveal'));
    root.querySelectorAll('.jr-dring circle[stroke-dasharray]').forEach((c, i) => { c.style.setProperty('--d', (i * 90) + 'ms'); c.classList.add('jr-sweep'); });
    /* jr-enter (مش jr-in): اسم .jr-in مستخدم بالفعل لبطاقة عنصر insights — نفس الاسم هنا كان
       هيتعارض مع تنسيقها (padding/background/border) على كل بطاقة/مؤشر في اللوحة. */
    root.querySelectorAll('.jr-w, .jr-kpis > *').forEach((el, i) => { el.style.setProperty('--i', Math.min(i, 12)); el.classList.add('jr-enter'); });
  }

  /* ---------- تركيب لوحة كاملة: عنوان + صفحات + فلاتر ---------- */
  function mount(d, grid, { bar = null, onPage = null, startPage = 0 } = {}) {
    let pi = startPage, salt = '';
    const sel = {};
    const host = bar || (() => { let b = grid.parentElement.querySelector(':scope > .jr-bar'); if (!b) { b = document.createElement('div'); b.className = 'jr-bar'; grid.before(b); } return b; })();
    const draw = () => {
      host.innerHTML = `${d.p.length > 1 ? `<div class="jr-tabs" role="tablist" aria-label="صفحات اللوحة">${d.p.map((p, i) => `<button type="button" role="tab" aria-selected="${i === pi}" data-pg="${i}">${esc(p.n)}</button>`).join('')}</div>` : ''}
        ${(d.f || []).length ? `<div class="jr-filters" role="group" aria-label="فلاتر اللوحة">${d.f.map(([lab, opts], fi) => `<div class="jr-f"><span>${esc(lab)}</span><div>${opts.map(o => `<button type="button" class="chip${sel[fi] === o ? ' is-on' : ''}" data-f="${fi}" data-o="${esc(o)}" aria-pressed="${sel[fi] === o}">${esc(o)}</button>`).join('')}</div></div>`).join('')}${Object.keys(sel).length ? `<button type="button" class="btn btn-ghost btn-sm" data-clear>${ic('x')}مسح الفلاتر</button>` : ''}</div>` : ''}`;
      grid.innerHTML = renderPage(d, d.p[pi], salt); animate(grid);
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
  const GLYPH_OF = { line: 'line', area: 'line', cum: 'line', combo: 'line', bars: 'cbars', grouped: 'cbars', stacked: 'cbars', waterfall: 'cbars', hbars: 'hbars', rank: 'hbars', compare: 'hbars', s100: 'hbars', div: 'hbars', donut: 'donut', ring: 'donut', gauges: 'donut', funnel: 'funnel', heat: 'heat', cohort: 'heat', risk: 'heat', scatter: 'heat', bubble: 'heat', table: 'table', insights: 'table', note: 'table', bullet: 'hbars', hero: 'kpi', kpi: 'kpi', kpiyoy: 'kpi', radar: 'donut', semi: 'donut', dco: 'donut', iconrank: 'hbars', ribbon: 'cbars', people: 'table', feed: 'table', goal: 'kpi' };
  const thumbItems = d => {
    const ws = d.p[0].w, ks = ws.filter(isK).slice(0, 4), cs = ws.filter(w => !isK(w));
    const out = ks.map(w => ({ type: 'kpi', span: 12 / ks.length, title: w.ti }));
    let row = [], used = 0; cs.forEach(w => { const sp = w.sp || DEF[w.t] || 6; if (used + sp > 12) { row[row.length - 1].span += 12 - used; out.push(...row); row = []; used = 0; } row.push({ type: GLYPH_OF[w.t] || 'table', span: sp, title: w.ti }); used += sp; });
    if (row.length) { row[row.length - 1].span += 12 - used; out.push(...row); }
    return out.map(o => Object.assign(o, { span: snap(o.span) }));
  };

  window.JR = { renderPage, mount, animate, countUp, roll, thumbItems, dim, glyphOf: t => GLYPH_OF[t] || 'table', TYPES: Object.keys(R).concat(['kpi', 'kpiyoy', 'hero']) };
})();
