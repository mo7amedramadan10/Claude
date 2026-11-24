# -*- coding: utf-8 -*-
# نموذج «شريحة مؤشرات أداء الأعمال»: شريحتان بالتخطيط نفسه (فاتحة ثم داكنة): عدّاد نصف دائري بستة نطاقات وإبرة،
# رسم أعمدة مجمّعة بثلاث سلاسل لأربع سنوات، وثلاث بطاقات مؤشرات ملوّنة بخط اتجاه صغير.
import math

KEY = 'sl-business-kpi-slide'
TITLE = {'ar': 'شريحة مؤشرات أداء الأعمال', 'en': 'Business KPI Slide'}
COMMENT = 'نموذج «شريحة مؤشرات أداء الأعمال»: بطاقة عدّاد (إبرته من كتلة gauge-data)، بطاقة رسم أعمدة مجمّعة (من chart-data)، وثلاث بطاقات مؤشرات متدرّجة (أخضر، أحمر، أصفر). الشريحة الثانية نسخة داكنة.'
VARS = ('--ink: #1E4A5F; --ink-2: #555B60; --accent: #1E4A5F; --bg: #ECECEC; --card: #FFFFFF; --line: #DCDCDC; --muted: #555B60;'
        ' --g1: #5E8B7F; --g2: #86B697; --r1: #E35530; --r2: #F2A08B; --y1: #DE993E; --y2: #FDCB56;')

FONTS = {'ar': '<link href="https://fonts.googleapis.com/css2?family=Cairo:wght@400;500;600;700;800&display=swap" rel="stylesheet">',
         'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@400;600;700&display=swap" rel="stylesheet">'}
FONT_STACK = {'ar': "'Cairo', 'Segoe UI', Tahoma, sans-serif", 'en': "'Open Sans', 'Segoe UI', Arial, sans-serif"}

STYLE = """  /* الشريحة الداكنة تعيد تعريف ألوان البطاقات والنصوص */
  .s2 { --bg: #13303F; --card: rgb(8 24 34 / .38); --ink: #FFFFFF; --muted: #FFFFFF; --line: #2A4859; background: linear-gradient(200deg, #1F4C62 0%, #15384A 38%, #112A38 70%, #0E202B 100%); }
  .s1 { background: var(--bg); }
  .card { position: absolute; top: 63px; height: 440px; border-radius: 18px; background: var(--card); }
  .cg { inset-inline-start: 65px; width: 497px; }
  .cc { inset-inline-start: 578px; width: 639px; }
  /* العدّاد: يُعكس في العربية فيصير الصفر في جهة البداية */
  .gauge { position: absolute; top: 120px; inset-inline-start: 130px; width: 370px; height: 220px; }
  html[dir="rtl"] .gauge { transform: scaleX(-1); }
  .gauge .needle, .gauge .hub { fill: #1A3443; } .gauge .hole { fill: #FFFFFF; }
  .s2 .gauge .needle, .s2 .gauge .hub { fill: #FFFFFF; } .s2 .gauge .hole { display: none; }
  .gmin, .gmax { position: absolute; top: 325px; margin: 0; font-size: 18px; line-height: 1.3; color: var(--muted); }
  .gmin { inset-inline-start: 166px; width: 20px; text-align: center; }
  .gmax { inset-inline-start: 413px; width: 82px; text-align: center; }
  .gval { position: absolute; top: 350px; inset-inline-start: 65px; width: 497px; margin: 0; text-align: center; font-size: 37px; font-weight: 700; line-height: 1.4; color: var(--ink); }
  .gtxt { position: absolute; top: 408px; inset-inline-start: 164px; width: 300px; margin: 0; text-align: center; font-size: 18px; line-height: 1.3; color: var(--muted); }
  .s1 .gtxt, .s1 .gmin, .s1 .gmax { color: #4A4F54; }
  .ctl { position: absolute; top: 84px; inset-inline-start: 601px; margin: 0; font-size: 26px; font-weight: 700; line-height: 1.4; color: var(--ink); }
  .crule { position: absolute; top: 126px; inset-inline-start: 601px; width: 592px; height: 1px; background: var(--line); }
  .chart { position: absolute; top: 160px; inset-inline-start: 590px; width: 615px; height: 280px; overflow: visible; }
  .chart .ax { fill: #555B60; font-size: 15px; } .chart .base { stroke: #D9D9D9; }
  .s2 .chart .ax { fill: #E9EEF1; } .s2 .chart .base { stroke: #C9D3D8; }
  .lg { position: absolute; top: 456px; display: flex; align-items: center; gap: 8px; margin: 0; font-size: 15px; line-height: 1.4; color: #333; }
  .s2 .lg { color: #fff; }
  .lg i { width: 16px; height: 16px; border-radius: 50%; flex: none; }
  .lg1 { inset-inline-start: 601px; } .lg2 { inset-inline-start: 748px; } .lg3 { inset-inline-start: 895px; }
  .lg1 i { background: linear-gradient(90deg, #5F8E7E, #86B697); } .lg2 i { background: linear-gradient(90deg, #F2A08B, #E35530); } .lg3 i { background: linear-gradient(90deg, #FCC955, #E2A040); }
  /* بطاقات المؤشرات السفلية */
  .kpi { position: absolute; top: 524px; height: 136px; border-radius: 16px; color: #fff; }
  .k1 { inset-inline-start: 65px; width: 373px; background: linear-gradient(to top right, var(--g1), var(--g2)); }
  .k2 { inset-inline-start: 455px; width: 373px; background: linear-gradient(to top right, var(--r1), var(--r2)); }
  .k3 { inset-inline-start: 845px; width: 372px; background: linear-gradient(to top right, var(--y1), var(--y2)); }
  html[dir="rtl"] .k1 { background: linear-gradient(to top left, var(--g1), var(--g2)); }
  html[dir="rtl"] .k2 { background: linear-gradient(to top left, var(--r1), var(--r2)); }
  html[dir="rtl"] .k3 { background: linear-gradient(to top left, var(--y1), var(--y2)); }
  .kpi h2 { position: absolute; top: 18px; inset-inline-start: 21px; margin: 0; font-size: 22px; font-weight: 700; line-height: 1.4; }
  .kpi p { position: absolute; top: 58px; inset-inline-start: 21px; margin: 0; font-size: 41px; font-weight: 700; line-height: 1.3; }
  .kpi .spark { position: absolute; top: 46px; inset-inline-start: 248px; width: 104px; height: 48px; }
  html[dir="rtl"] .kpi .spark { transform: scaleX(-1); }
  html[lang="ar"] .kpi h2 { top: 14px; font-size: 21px; }
  html[lang="ar"] .ctl { top: 80px; font-size: 25px; }
  html[lang="ar"] .gtxt { width: 250px; inset-inline-start: 189px; line-height: 1.45; top: 404px; }
  /* الأرقام والعملة تُعزل باتجاه LTR حتى لا ينقلب «$ 75k» في العربية */
  .num { direction: ltr; unicode-bidi: isolate; }"""


def gauge():
    cx, cy, R, r = 314, 316, 178, 99
    cols = ['#E65631', '#DE943B', '#FEC955', '#7FB092', '#618981', '#1D475D']
    parts = []
    for i, c in enumerate(cols):
        a0, a1 = math.pi - i * math.pi / 6, math.pi - (i + 1) * math.pi / 6
        p = lambda rad, a: (cx + rad * math.cos(a), cy - rad * math.sin(a))
        (x0, y0), (x1, y1), (x2, y2), (x3, y3) = p(R, a0), p(R, a1), p(r, a1), p(r, a0)
        parts.append('<path fill="%s" d="M%.1f,%.1f A%d,%d 0 0 1 %.1f,%.1f L%.1f,%.1f A%d,%d 0 0 0 %.1f,%.1f Z"/>'
                     % (c, x0, y0, R, R, x1, y1, x2, y2, r, r, x3, y3))
    # الإبرة مرسومة نحو اليمين ثم تُدار بالقيمة من كتلة البيانات
    needle = ('<g class="ndl" transform="rotate(-42 314 310)"><path class="needle" d="M314,295 L458,309 L458,311 L314,325 Z"/>'
              '<circle class="hub" cx="314" cy="310" r="23"/><circle class="hole" cx="314" cy="310" r="9.5"/></g>')
    return ('<svg class="gauge" viewBox="130 120 370 220" aria-hidden="true">' + ''.join(parts) + needle + '</svg>')


SPARK = ('<svg class="spark" viewBox="0 0 104 48" fill="none" stroke="#fff" stroke-width="2.2" aria-hidden="true">'
         '<path d="M10.5,40 L33,17 M39,17.5 L62,27.5 M68,26 L90,9"/>'
         '<circle cx="7" cy="40" r="5"/><circle cx="33" cy="14" r="5"/><circle cx="68" cy="26" r="5"/><circle cx="94" cy="5" r="5"/></svg>')


def slide(n):
    sfx = '' if n == 1 else '-2'
    return ('\n    <div class="card cg"></div><div class="card cc"></div>'
            '\n    ' + gauge().replace('class="gauge"', 'class="gauge" data-gauge="gauge-data%s"' % sfx) +
            '\n    <p class="gmin">§GMIN§</p><p class="gmax"><span class="num">§GMAX§</span></p>'
            '\n    <p class="gval"><span class="num">§GVAL§</span></p><p class="gtxt">§GTXT§</p>'
            '\n    <h2 class="ctl">§CTL§</h2><span class="crule" aria-hidden="true"></span>'
            '\n    <svg class="chart" data-chart="chart-data%s" width="615" height="280" viewBox="0 0 615 280" aria-hidden="true"></svg>' % sfx +
            '\n    <p class="lg lg1"><i></i>§L1§</p><p class="lg lg2"><i></i>§L2§</p><p class="lg lg3"><i></i>§L3§</p>'
            '\n    <div class="kpi k1"><h2>§K1§</h2><p><span class="num">§V1§</span></p>' + SPARK + '</div>'
            '\n    <div class="kpi k2"><h2>§K2§</h2><p><span class="num">§V2§</span></p>' + SPARK + '</div>'
            '\n    <div class="kpi k3"><h2>§K3§</h2><p><span class="num">§V3§</span></p>' + SPARK + '</div>\n  ')


SLIDES = [slide(1), slide(2)]

GLOBAL_DEFS = ('<defs>'
               '<linearGradient id="kb-g" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#6A9884"/><stop offset=".5" stop-color="#84B494"/><stop offset="1" stop-color="#6C9A86"/></linearGradient>'
               '<linearGradient id="kb-r" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#F3A08A"/><stop offset=".55" stop-color="#E96A52"/><stop offset="1" stop-color="#E25634"/></linearGradient>'
               '<linearGradient id="kb-y" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#FAC65A"/><stop offset=".55" stop-color="#F1B246"/><stop offset="1" stop-color="#E0A040"/></linearGradient>'
               '</defs>')

CHART = {'max': 50, 'step': 5, 'categories': ['2025', '2026', '2027', '2028'],
         'series': [{'fill': 'kb-g', 'values': [34, 42, 46, 45]},
                    {'fill': 'kb-r', 'values': [23, 17, 27, 26]},
                    {'fill': 'kb-y', 'values': [16, 25, 35, 21]}]}
GAUGE = {'min': 0, 'max': 10000, 'value': 7620}
JSON_BLOCKS = {'gauge-data': GAUGE, 'chart-data': CHART, 'gauge-data-2': GAUGE, 'chart-data-2': CHART}

EXTRA_SCRIPT = r"""  // ----- العدّاد: زاوية الإبرة من القيمة (الصفر في جهة البداية بعد عكس الرسم للعربية) -----
  var NS = 'http://www.w3.org/2000/svg';
  function J(id) { return JSON.parse(document.getElementById(id).textContent); }
  function E(tag, a, p) { var e = document.createElementNS(NS, tag); for (var k in a) e.setAttribute(k, a[k]); if (p) p.appendChild(e); return e; }
  Array.prototype.forEach.call(document.querySelectorAll('svg[data-gauge]'), function (s) {
    var d = J(s.getAttribute('data-gauge')), f = Math.max(0, Math.min(1, (d.value - d.min) / (d.max - d.min)));
    s.querySelector('.ndl').setAttribute('transform', 'rotate(' + (180 * f - 180) + ' 314 310)');
  });
  // ----- رسم الأعمدة المجمّعة: الفئة الأولى في جهة البداية -----
  Array.prototype.forEach.call(document.querySelectorAll('svg[data-chart]'), function (s) {
    var d = J(s.getAttribute('data-chart')), W = 615, y0 = 238, k = 225 / d.max, n = d.series.length, bw = 26, gap = 3;
    while (s.firstChild) s.removeChild(s.firstChild);
    var X = function (x) { return RTL ? W - x : x; }, R = function (x, w) { return RTL ? W - x - w : x; };
    for (var t = 0; t <= d.max; t += d.step) {
      var tx = E('text', { x: X(31), y: y0 - t * k, 'text-anchor': RTL ? 'start' : 'end', 'dominant-baseline': 'central', 'class': 'ax' }, s); tx.textContent = t;
    }
    E('line', { x1: X(47), y1: y0 + .5, x2: X(603), y2: y0 + .5, 'class': 'base', 'stroke-width': 1 }, s);
    var step = 556 / d.categories.length, grp = n * bw + (n - 1) * gap;
    d.categories.forEach(function (c, i) {
      var cx = 47 + step * (i + .5) - 3, x0 = cx - grp / 2;
      d.series.forEach(function (se, j) {
        var h = se.values[i] * k;
        E('rect', { x: R(x0 + j * (bw + gap), bw), y: y0 - h, width: bw, height: h, fill: 'url(#' + se.fill + ')' }, s);
      });
      var lb = E('text', { x: X(cx), y: y0 + 23, 'text-anchor': 'middle', 'dominant-baseline': 'central', 'class': 'ax' }, s); lb.textContent = c;
    });
  });"""

STR = {
    'ar': {'GMIN': '0', 'GMAX': '10,000', 'GVAL': '7,620', 'GTXT': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
           'CTL': 'عنوان نائب', 'L1': 'عنوان نائب', 'L2': 'عنوان نائب', 'L3': 'عنوان نائب',
           'K1': 'نمو العملاء', 'V1': '4,500', 'K2': 'إجمالي الطلبات', 'V2': '3,900', 'K3': 'إجمالي الإيرادات', 'V3': '$ 75k'},
    'en': {'GMIN': '0', 'GMAX': '10,000', 'GVAL': '7,620', 'GTXT': 'This is a sample text. Insert your desired text here.',
           'CTL': 'Placeholder', 'L1': 'Placeholder', 'L2': 'Placeholder', 'L3': 'Placeholder',
           'K1': 'Customer Growth', 'V1': '4,500', 'K2': 'Total Orders', 'V2': '3,900', 'K3': 'Total Revenue', 'V3': '$ 75k'},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'mgmt', 'icon': '📟',
    'name': 'شريحة مؤشرات أداء الأعمال', 'nameEn': 'Business KPI Slide',
    'description': 'لوحة مؤشرات بشريحتين فاتحة وداكنة: عدّاد، رسم أعمدة مجمّعة، وثلاث بطاقات مؤشرات',
    'descriptionEn': 'KPI dashboard in light and dark slides: gauge, clustered bar chart and three KPI cards',
    'sections': [
        {'title': 'لوحة المؤشرات (فاتحة)', 'titleEn': 'KPI Dashboard (Light)', 'needsData': True},
        {'title': 'لوحة المؤشرات (داكنة)', 'titleEn': 'KPI Dashboard (Dark)', 'needsData': True},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص والأرقام فقط؛ إبرة العدّاد من كتلتي gauge-data وgauge-data-2 (min/max/value) مع تحديث نص القيمة والحدّين، وأعمدة الرسم من كتلتي chart-data وchart-data-2 (الفئات وقيم السلاسل الثلاث).',
}
