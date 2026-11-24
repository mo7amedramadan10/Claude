# -*- coding: utf-8 -*-
# نموذج «لوحة أداء المتجر الزرقاء»: ثلاث شرائح لوحات مؤشرات بيع بالتجزئة على خلفية زرقاء متدرّجة، بطاقات بيضاء برسوم SVG
# تُرسم كلها بجافاسكربت من كتل chart-data… (القيم مقروءة من الأصل)، ومؤشرات KPI نصوص HTML قابلة للتعديل.
KEY = 'sl-blue-store-performance-dashboard'
TITLE = {'ar': 'لوحة أداء المتجر', 'en': 'Blue Store Performance Dashboard'}
COMMENT = 'نموذج «لوحة أداء المتجر الزرقاء»: عنوان أبيض، بطاقات بيضاء مدوّرة بعنوان أزرق وأيقونة دائرية وخط فاصل، ورسوم SVG تُرسم من كتل JSON بالمعرّفات chart-data… (الفئة الأولى في جهة البداية)، وخريطة العالم SVG ثابتة.'
VARS = ('--ink: #262626; --accent: #3361BF; --bg: #3662BE; --navy: #0F2557; --blue: #3361BF; --blue-2: #8BACE7; --blue-3: #D0DFF7; '
        '--gray: #D5DAE4; --title: #2E55A5; --line: #D9D9D9; --muted: #595959; --panel: #123066;')

FONTS = {'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@400;600;700;800&display=swap" rel="stylesheet">'}
FONT_STACK = {'en': "'Open Sans', 'Segoe UI', Tahoma, Arial, sans-serif"}

STYLE = """  .frame { background: linear-gradient(180deg, #3662BE 0%, #3A66C2 22%, #6C8DD6 52%, #8BACE7 92%); }
  .ttl { position: absolute; top: 30px; inset-inline-start: 38px; margin: 0; font-size: 36px; font-weight: 700; line-height: 50px; color: #fff; white-space: nowrap; }
  html[lang="ar"] .ttl { font-size: 36px; font-weight: 800; } html[lang="ar"] .s2 .ttl { font-size: 33px; }
  /* البطاقة: عنوان أزرق، أيقونة دائرية في جهة النهاية، خط فاصل */
  .card { position: absolute; background: #fff; border-radius: 20px; }
  .card h2 { position: absolute; top: 14px; inset-inline-start: 16px; margin: 0; font-size: 19px; font-weight: 700; line-height: 26px; color: var(--title); white-space: nowrap; }
  .card .ico { position: absolute; top: 9px; inset-inline-end: 17px; width: 37px; height: 37px; border-radius: 50%; background: var(--blue); color: #fff; display: grid; place-items: center; }
  .card .ico svg { width: 22px; height: 22px; }
  .card .ln { position: absolute; top: 51px; inset-inline: 15px; height: 1px; background: var(--line); }
  .card .ch { position: absolute; top: 0; inset-inline-start: 0; pointer-events: none; overflow: visible; }
  .card .unit { position: absolute; inset-inline-start: 13px; margin: 0; font-size: 16px; line-height: 20px; color: #262626; }
  .card.blue { background: linear-gradient(225deg, #A9BEEB 0%, #6E8FD7 38%, #3D66C4 72%, #5A7DCD 100%); }
  .card.blue h2 { color: #fff; } .card.blue .ln { background: rgb(255 255 255 / .35); } .card.blue .ico { background: #fff; color: var(--blue); }
  .foot { position: absolute; top: 261px; inset-inline-start: 16px; margin: 0; font-size: 15px; line-height: 20px; color: #000; }
  .fy { position: absolute; top: 217px; width: 194px; height: 40px; border-radius: 5px; background: #fff; display: grid; place-items: center; margin: 0; font-size: 26px; font-weight: 700; color: #2F5BB7; }
  html[lang="ar"] .fy { font-size: 24px; }
  /* لوحة المؤشرات الجانبية (الشريحة 1) */
  .s1 .panel { position: absolute; top: 0; inset-inline-end: 0; width: 194px; height: 720px; background: linear-gradient(180deg, #163775, #123066 40%); }
  .kp { position: absolute; inset-inline-start: 1103px; width: 158px; border-radius: 14px; display: flex; flex-direction: column; justify-content: flex-end; padding: 0 14px 16px; color: #333842; }
  .kp .ki { position: absolute; top: 16px; inset-inline-end: 16px; width: 42px; height: 42px; }
  .kp b { display: block; font-size: 37px; font-weight: 700; line-height: 44px; letter-spacing: -.5px; }
  .kp span { display: block; font-size: 18px; line-height: 23px; margin-top: 4px; }
  .kp.k1 { top: 17px; height: 161px; background: #D5DCE5; } .kp.k2 { top: 190px; height: 161px; background: var(--blue-3); }
  .kp.k3 { top: 365px; height: 160px; background: var(--blue-2); color: #fff; } .kp.k4 { top: 537px; height: 161px; background: #385CB2; color: #fff; }
  .kp.k4 b { margin-bottom: -2px; }
  /* شريط المؤشرات العلوي (الشريحتان 2 و3) */
  .strip { position: absolute; top: 93px; height: 97px; background: #fff; border-radius: 18px; }
  .kb { position: absolute; top: 99px; height: 84px; border-radius: 11px; display: flex; flex-direction: column; align-items: center; justify-content: center; text-align: center; color: #fff; padding: 0 8px; }
  .kb span { font-size: 16px; line-height: 17px; } .kb b { font-size: 25px; font-weight: 700; line-height: 30px; }
  html[lang="ar"] .kb span { font-size: 15px; line-height: 18px; }
  .kb.c1 { background: var(--blue-2); } .kb.c2 { background: var(--blue); } .kb.c3 { background: var(--navy); } .kb.c4 { background: var(--blue-3); color: #262626; }
  .kb.c4 span { color: #3A3A3A; }
  .s2 .coname { position: absolute; top: 112px; inset-inline-start: 48px; width: 200px; margin: 0; text-align: center; font-size: 21px; font-weight: 700; line-height: 29px; color: #3D5FB0; }
  /* جدول إيرادات المتاجر */
  .tbl { position: absolute; top: 60px; inset-inline-start: 15px; width: 567px; border-collapse: collapse; font-size: 11.5px; color: #262626; table-layout: fixed; }
  .tbl th, .tbl td { border: 1px solid #C8C8C8; height: 18.7px; padding: 0 4px; text-align: center; font-weight: 400; line-height: 16px; white-space: nowrap; }
  .tbl th { height: 23px; background: #F2F2F2; font-weight: 700; color: #1F2A44; }
  .tbl th:first-child, .tbl td:first-child { text-align: start; padding-inline-start: 9px; }
  html[lang="ar"] .tbl { font-size: 11px; } html[lang="ar"] .tbl th { font-size: 10.5px; white-space: normal; line-height: 11px; }
  .map { position: absolute; top: 90px; inset-inline-start: 82px; width: 270px; height: 137px; }
  .ch text { font-family: var(--font); }"""

# أيقونات (24×24)
I = {
    'layers': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round" aria-hidden="true"><path d="M12 3 2.5 8 12 13l9.5-5z"/><path d="m2.5 12 9.5 5 9.5-5"/><path d="m2.5 16 9.5 5 9.5-5"/></svg>',
    'cart': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round" aria-hidden="true"><path d="M2.5 4h2.6l2.3 11h11l2.2-8.2H6"/><path d="M9.5 6.8v8.2M13.3 6.8v8.2M17 6.8v8.2M6.7 11h13"/><circle cx="9" cy="19" r="1.4" fill="currentColor"/><circle cx="17" cy="19" r="1.4" fill="currentColor"/></svg>',
    'store': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M4.5 3h15l2 5.5a2.6 2.6 0 0 1-5 .9 2.6 2.6 0 0 1-4.5 0 2.6 2.6 0 0 1-4.5 0 2.6 2.6 0 0 1-5-.9z"/><path d="M4.5 12.3V21h15v-8.7a4 4 0 0 1-3-.6 4 4 0 0 1-4.5 0 4 4 0 0 1-4.5 0 4 4 0 0 1-3 .6zm4 2.7h7v4h-7z"/></svg>',
    'tag': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M3 3h8.2L21 12.8 12.8 21 3 11.2zm4.3 2.6a1.8 1.8 0 1 0 0 3.6 1.8 1.8 0 0 0 0-3.6z"/></svg>',
    'globe': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" aria-hidden="true"><circle cx="12" cy="12" r="9.2"/><path d="M5.5 6.5c2 .6 2.4 2 1.6 3.4-.8 1.3.4 2.6 1.9 2.4 1.6-.2 2.2 1.4 1.4 2.8-.7 1.3-.2 3 1.2 4.6M14.5 3.2c-.8 1.5-.2 2.8 1.3 3 1.7.3 1.6 2 .4 2.7-1.3.8-.6 2.7 1.1 2.6 1.5-.1 2.6.6 3.4 1.8M15 16.5c.8.9 1.5 1.6 2.6 1.8"/></svg>',
    'register': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M5 3h8v5H5zM7 4.5v2h4v-2z"/><path d="M3.5 10h17l1 9.5H2.5zM6 12v1.6h2V12zm3.5 0v1.6h2V12zm3.5 0v1.6h2V12zM6 15v1.6h2V15zm3.5 0v1.6h2V15zm3.5 0v1.6h2V15z"/><path d="M15 6.5h5V10h-5z" opacity=".85"/><path d="M2.5 20.5h19V22h-19z"/></svg>',
    'basket': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round" aria-hidden="true"><path d="M2.5 9.5h19l-2 10.5h-15z"/><path d="M7 9.5 10.5 4M17 9.5 13.5 4M7.5 13v4M12 13v4M16.5 13v4"/></svg>',
    'coin': '<svg viewBox="0 0 48 48" fill="none" stroke="currentColor" stroke-width="2" stroke-linejoin="round" aria-hidden="true"><circle cx="17" cy="28" r="13"/><circle cx="17" cy="28" r="9.5"/><path d="M20 23.5c-.6-1.3-1.8-2-3.2-2-1.9 0-3.2 1-3.2 2.5 0 3.6 6.6 2 6.6 5.6 0 1.6-1.4 2.6-3.4 2.6-1.6 0-2.9-.8-3.4-2.2M17 19.5v2M17 32.2v2.2"/><path d="M27 37h6V12h6L32.5 4 26 12h6"/></svg>',
    'hand': '<svg viewBox="0 0 48 48" fill="none" stroke="currentColor" stroke-width="2" stroke-linejoin="round" aria-hidden="true"><circle cx="26" cy="19" r="8"/><path d="M28.4 15.6c-.4-.9-1.3-1.4-2.3-1.4-1.4 0-2.3.7-2.3 1.8 0 2.6 4.7 1.4 4.7 4 0 1.1-1 1.9-2.4 1.9-1.1 0-2-.6-2.4-1.6M26 12.8v1.4M26 22.3v1.5M26 2v4M20 4l1.5 3M32 4l-1.5 3"/><path d="M4 30h6v13H4zM10 32h7c2 0 3.5 1 4.5 2.5H28a2.5 2.5 0 0 1 0 5H18M10 41.5l11 1.5c2 .3 3.6-.2 5.2-1.2L42 32.5a2.6 2.6 0 0 0-3.2-4L29.5 34"/></svg>',
    'shrink': '<svg viewBox="0 0 48 48" fill="none" stroke="currentColor" stroke-width="2" stroke-linejoin="round" aria-hidden="true"><path d="M8 6h34v34H8z"/><path d="M8 25h17v15"/><path d="M36 12 25.5 22.5M25.5 13.5v9h9"/></svg>',
    'pie': '<svg viewBox="0 0 48 48" fill="none" stroke="currentColor" stroke-width="2" stroke-linejoin="round" aria-hidden="true"><path d="M22 6.2A18 18 0 1 0 42 25H22z" stroke-dasharray="3.2 2.4"/><path d="M26 3v18h18A18 18 0 0 0 26 3z"/><path d="M26 21 40 10M26 21l17-4"/></svg>',
}


def card(x, y, w, h, title, icon, inner, cls='card', ttl_style=''):
    head = ('<h2%s>%s</h2>' % (' style="%s"' % ttl_style if ttl_style else '', title)) if title else ''
    ic = ('<span class="ico">%s</span>' % I[icon]) if icon else ''
    return ('\n    <div class="%s" style="top:%dpx;inset-inline-start:%dpx;width:%dpx;height:%dpx">%s%s<div class="ln"%s></div>%s</div>'
            % (cls, y, x, w, h, head, ic, '' if ttl_style == '' else ' style="top:66px"', inner))


def chart(cid, w, h):
    return '<svg class="ch" data-chart="%s" width="%d" height="%d" viewBox="0 0 %d %d" aria-hidden="true"></svg>' % (cid, w, h, w, h)


def kpi(n, icon, val, lab):
    return '\n    <div class="kp k%d"><span class="ki">%s</span><b>§%s§</b><span>§%s§</span></div>' % (n, I[icon], val, lab)


def kb(cls, x, w, lab, val):
    return '\n    <div class="kb %s" style="inset-inline-start:%dpx;width:%dpx"><span>§%s§</span><b>§%s§</b></div>' % (cls, x, w, lab, val)


S1 = ('\n    <div class="panel"></div>\n    <h1 class="ttl">§T1§</h1>'
      + card(38, 87, 507, 290, '§C11§', 'layers', chart('chart-data', 507, 290) + '<div class="ln" style="top:255px"></div><p class="foot">§FOOT§</p>')
      + card(557, 87, 506, 290, '§C12§', 'cart', chart('chart-data-2', 506, 290))
      + card(38, 393, 507, 290, '§C13§', 'layers', chart('chart-data-3', 507, 290)
             + '<p class="fy" style="inset-inline-start:32px">§FY1§</p><p class="fy" style="inset-inline-start:274px;width:195px">§FY2§</p>', cls='card blue')
      + card(557, 393, 506, 290, '', None, chart('chart-data-4', 506, 290), cls='card nohead')
      + kpi(1, 'coin', 'K1V', 'K1L') + kpi(2, 'hand', 'K2V', 'K2L') + kpi(3, 'shrink', 'K3V', 'K3L') + kpi(4, 'pie', 'K4V', 'K4L') + '\n  ')

TBL_W = [80, 82, 81, 81, 65, 85, 93]
ROWS = [('31,067', '1,510', '31,854', '9,587', '21,450', '65.87%'), ('35,424', '3,520', '32,956', '16,568', '16,584', '65.62%'),
        ('36,840', '3,785', '33,256', '17,985', '16,854', '52.18%'), ('35,085', '2,667', '33,875', '13,632', '21,500', '56.48%'),
        ('21,780', '1,549', '21,054', '7,560', '13,789', '66.52%'), ('24,523', '1,584', '23,687', '9,500', '14,324', '58.35%')]
TABLE = ('<table class="tbl"><colgroup>' + ''.join('<col style="width:%dpx">' % w for w in TBL_W) + '</colgroup><thead><tr>'
         + ''.join('<th>§TH%d§</th>' % i for i in range(1, 8)) + '</tr></thead><tbody>'
         + ''.join('<tr><td>§ST§ %d</td>%s</tr>' % (r + 1, ''.join('<td>%s</td>' % (('$' + v) if not v.endswith('%') else v) for v in row)) for r, row in enumerate(ROWS))
         + '</tbody></table>')

S2 = ('\n    <h1 class="ttl">§T2§</h1>\n    <div class="strip" style="inset-inline-start:38px;width:1202px"></div><p class="coname">§CO§</p>'
      + kb('c1', 262, 237, 'B1L', 'B1V') + kb('c2', 506, 237, 'B2L', 'B2V') + kb('c3', 751, 238, 'B3L', 'B3V') + kb('c4', 995, 238, 'B4L', 'B4V')
      + card(38, 202, 596, 210, '§C21§', 'store', TABLE)
      + card(643, 202, 597, 210, '§C22§', 'tag', chart('chart-data-5', 597, 210))
      + card(38, 425, 448, 260, '§C23§', 'globe', chart('chart-data-6', 448, 260) + '§MAP§')
      + card(496, 425, 284, 260, '§C24§', 'register', chart('chart-data-7', 284, 260))
      + card(792, 425, 448, 260, '§C25§', 'basket', chart('chart-data-8', 448, 260)) + '\n  ')

S3 = ('\n    <h1 class="ttl">§T3§</h1>\n    <div class="strip" style="inset-inline-start:38px;width:837px"></div>'
      + kb('c1', 48, 199, 'D1L', 'D1V') + kb('c2', 254, 199, 'D2L', 'D2V') + kb('c3', 459, 200, 'D3L', 'D3V') + kb('c4', 665, 200, 'D4L', 'D4V')
      + card(38, 198, 284, 235, '§C31§', None, '<p class="unit" style="top:58px">§MIL§</p>' + chart('chart-data-9', 284, 235))
      + card(38, 445, 284, 238, '§C32§', None, '<p class="unit" style="top:57px">§MIL§</p>' + chart('chart-data-10', 284, 238))
      + card(333, 198, 541, 235, '§C33§', None, '<p class="unit" style="top:58px">§MIL§</p>' + chart('chart-data-11', 541, 235))
      + card(333, 445, 541, 238, '§C34§', None, chart('chart-data-12', 541, 238))
      + card(886, 92, 354, 291, '§C35§', None, '<p class="unit" style="top:75px">§MIL§</p>' + chart('chart-data-13', 354, 291), ttl_style='width:240px;white-space:normal;line-height:20px;top:12px')
      + card(886, 393, 354, 290, '§C36§', None, '<p class="unit" style="top:52px">§MIL§</p>' + chart('chart-data-14', 354, 290)) + '\n  ')
# بطاقة «الفعلية مقابل المتوقعة» السفلى خطها الفاصل أعلى (عنوان بسطر واحد)
S3 = S3.replace('height:290px"><h2>§C36§</h2><div class="ln"></div>', 'height:290px"><h2 style="top:8px">§C36§</h2><div class="ln" style="top:44px"></div>')

SLIDES = [S1, S2, S3]

# مسارات الخريطة (مولّدة من بيانات Natural Earth 110m بإسقاط ميلر ومبسّطة)
MAP_PATHS = {
"na": "M29.3 24.6L32.6 25.6L34.2 24.7L37.7 23.9L38.2 24.5L38.9 23.5L40.7 24.9L41.7 24L41.8 25L43.9 24.4L48.6 25.6L49.6 26.3L48.5 26.9L49.9 27.2L52.5 26.8L53.3 27.6L54.2 26.9L53.4 26.4L53.9 26L55.4 25.8L56.7 26.8L58.9 27.3L61.2 27.1L61.1 26.3L61.7 26.1L62.9 26.5L62.9 27.7L64.3 25.4L62.6 24.1L62.7 22.6L63.6 21.5L64.6 21.8L66.4 23.9L65.7 24.6L67.1 24.8L67.1 26.2L68.1 25.2L69 26L68.8 27L69.5 27.8L70.8 25.8L70.9 24.3L73 24.6L74.1 25.9L73.5 26.6L74 27.9L72.5 28.8L70.7 28.6L69.5 30.8L67 32.2L66.9 33L66.1 33.1L64.3 35.4L64 37.6L65.1 37.8L65.8 39.6L66.8 39.4L71.2 41.5L73.3 41.6L73.4 43.6L75.1 45.7L76 44.3L75.1 42.1L77.6 40.2L76.1 37.7L77 36.6L76.4 33.8L79.6 33.6L81.5 35.1L82.8 35.2L83 37.6L84.3 38.4L86.6 36L89 39.7L88.6 40.4L92.9 43.2L93.2 44.7L92.2 45.4L90 46.6L85.2 46.6L81.7 49.9L85.1 47.7L86.2 47.6L86.9 48.1L86.2 48.7L86.6 50.5L88.9 50.8L89.6 49.7L90.1 50.8L86 53L85.4 52.9L85.4 52.1L86.7 51.3L84.6 51.5L82 53.4L82.5 54.7L79.7 55.3L81 55.3L79.5 55.5L78.8 57.1L78.4 56.6L78.7 57.5L78 58.6L77.7 56.9L77.8 57.8L77.3 57.7L78.2 60L74 63.5L75 67.2L74.7 68.5L73.7 68L72.2 64.7L71.2 64.9L70.2 64.3L67.8 64.5L67.9 65.3L64.6 64.9L62 66.8L61.6 70.7L63.1 73.5L64.2 74.1L66.4 73.5L67.3 71.8L69.7 71.4L68.3 75.8L71.3 75.7L72.4 76.3L72.4 80L73.9 81.2L75.3 80.6L76.5 80.9L77.1 81.9L76.6 82.4L75.7 81.1L74.7 81.6L74.7 82.4L72.4 81.5L70.8 80.4L69.4 77.8L66.6 77.3L64 75.6L62.6 76L57.4 74L55.9 72.7L55.5 70.5L53.1 68.2L49.6 63.3L48.9 63.2L49 64.5L52.9 70L52.5 70.4L50.9 68.9L50.8 67.9L48.7 66.5L49.4 65.8L48.4 65L47 62.1L44.5 60.8L41.7 55.9L42.1 51.1L41.5 48.6L42.7 48.7L43.1 49.6L42.9 47.8L39.4 46L39.1 44.5L38.2 44.1L34.4 38.5L32.5 38.4L30.1 36.9L24.7 35.4L23.8 35.6L24 36.4L21.2 37.3L22 34.9L19.5 37.1L20 37.7L19.3 38.4L16.2 40.8L11.4 42.4L16.7 39.1L17.2 37.6L15.7 38.1L14.7 37.4L13.5 37.9L13.1 36.4L12.1 36.6L11 35.8L10.7 34L11.6 32.8L14.4 32L13.9 31.3L14.4 30.8L11.3 31.2L8.9 29.7L11.6 28.6L12.3 28.6L12.2 29.2L13.7 29.2L9.9 26.3L10.3 25.7L11.7 25.6L13.6 23.7L17.6 22.3L19.2 23.2ZM75.2 20.3L74.3 19.5L74.7 18.9L76.5 19.1L77.8 20.3ZM64.8 17.1L64.4 17.7L62.4 17.2L63.9 16.1ZM62.4 11.2L63.3 11.7L62 12.7L61 11ZM68.9 18L65.7 17.3L65.3 15.8L62.2 14.4L62.4 13.8L66.3 14.4L66.8 15.5L68.1 16.2L74.2 16L75 16.6L74.7 17.6ZM51.6 12.2L52.6 12.4L52.4 12.9L51 13.4L49.9 12.8ZM93.3 45.6L92.4 47L92.9 46.7L93.1 47.3L94.9 47.6L94.7 48.3L95.2 48.1L95.5 49.2L95.2 50.1L94.4 49.9L94.3 49L93.5 49.8L92.8 49.1L90.6 49.2L92 46.1ZM72.1 30.4L74.9 32.1L74.3 32.5L72.7 31.6L70.9 32.9L70.6 32.2L69.6 32.3L70.2 31.7L70.6 29.7ZM75.9 20.9L76.6 20.4L79.3 21.8L79.4 22.4L80.8 22.1L83.4 23.5L84.8 25.3L83.4 25.9L86.4 27L87.4 28.2L88.6 28.3L87.1 30.6L85 28.9L84 29L83.9 29.7L86 31.3L86.5 32.5L86.2 33.3L83.4 32.1L85.4 34.2L81.7 33.1L78.9 31L76.7 31.5L76.1 31.1L76.6 30.2L79.5 30L79.5 28.9L80.5 27.7L80 26.7L77.3 25.6L77.8 25.3L77 24.5L75.8 24L74 24.5L71.3 24.2L67.9 23.1L68.6 22.5L67.6 22.5L67.3 21.1L68.7 19.2L70.6 18.9L70.1 19.8L70.7 20.7L71.4 19.5L73.3 18.9L74.6 20.4L74.4 21.4ZM64.1 18.4L67.1 18.8L64.3 21.4L63.4 21.4L63 19.4ZM42.9 15.4L45.7 13.2L47.8 13L47.2 14.8L43.9 15.7ZM35.5 42.8L36.2 42.7L36 43.9L36.6 44.7L35.2 43.4ZM55.9 10.3L59.4 11.1L60.2 12.6L56.1 11.8L56.8 11.3ZM42.4 48.3L40.8 48L39 46.9L38.7 46.1L40.7 46.6ZM43.8 17.9L46.8 18.3L48.4 19.3L45.6 20.7L44.7 22.3L42.7 23L40.6 21.6L42 19L41.3 18.1ZM54.1 15.8L55.6 15.6L55.3 17.1L50.8 18L49.7 18L49.6 17.5L51.2 16.8L46.7 16.8L48.4 14.8L53.2 16.4L52.1 14.9L52.8 14.3L53.6 14.5ZM55.1 19.9L55.9 20.5L56.7 22.8L59.3 24.1L59.2 24.7L58 24.8L58.2 25.8L55.5 25.3L50 26.1L49.6 25.5L47.9 25.3L47 24.2L50.7 23.7L46.6 23.4L46.2 22.9L47.9 22.4L45.4 22.1L46.6 20.4L48.6 19.6L49.4 19.8L49 20.5L50.7 20.1L51.7 20.8L52.6 20.1L53.9 21.9L54.2 21.3L53.7 19.9ZM59.7 20.4L58.8 19.5L59.7 18.8L62 18.9L61.5 20L62.6 20.6L62.5 21.9L60.5 22.3L58.1 20.7L58.1 20.3ZM55.1 19.2L56.6 19.4L56 20.4ZM61.1 14.5L61.7 15.2L61.4 17.1L60.1 17.2L59.3 17L59.4 16.1L58.1 16.2L58.1 15.1ZM63 8.1L64.3 7.4L63.9 7L65.7 6.9L66.7 7.8L69.1 8.5L70.6 10.2L68.2 12L65.3 11.9L64.5 11.2L65.1 10.1L62.9 9.6L62.5 8.8ZM66.3 5.8L70.9 4.4L72.6 5L73.2 4L75.5 3.5L80.4 3.3L88.6 4.4L88.6 4.9L84.3 6.5L85.9 6.5L82.9 8L81.6 9.4L77.3 10.2L78.4 10.4L77.8 10.7L78.5 11.6L75.2 13.7L76.6 14.4L74.6 15.3L67.9 14.8L67.8 14.1L69.2 13.7L68.8 12.6L71.3 13.2L69 11.8L71.2 10.2L69.8 8.7L72.4 8.9L73.6 8.3L69.3 8.2ZM78.6 27.5L77.3 28L77.1 27.3L78.1 26.4ZM62.8 24.9L63.3 25.4L62.8 25.8L60.2 25L61.3 24ZM20.1 38.6L20.9 39.1L19.5 40L19 39.2ZM81.2 74.2L79.2 73.9L80.7 73.7L80.1 72.7L81.9 72.7L83.8 73.7ZM99.9 4.4L102.4 3.3L105.1 3.4L106 2.7L114.7 2.7L119.4 4.2L118 4.9L111.5 5.5L114.1 5.3L116.4 6L117.8 5.4L118.4 6L117.6 7.1L123.2 5.7L125.4 6.1L125.8 6.8L122.4 8.5L120 8.8L121.7 8.9L120.2 11.2L120.2 13L121.1 14L118.7 14.6L120.1 15.4L120.3 16.7L119.5 16.9L120.5 18.1L118.8 18.2L119.7 18.8L119.4 19.4L117.3 19.6L118.3 21.2L116.8 20.6L116.4 21L118.4 22.2L118.7 23.3L117.3 23.5L115.8 22.2L116.1 23.2L115.2 23.9L118.2 24L114.2 26.2L111.2 26.6L109.3 28.5L105.1 30L104.1 32.4L102.9 33.3L103.2 34.2L102.5 36.3L101.4 36.4L100.3 35.4L98.8 35.4L96.3 32.2L94.5 27.8L95.3 26.3L96.4 25.9L96.8 24.3L94.9 25.1L94 24.7L94.2 23.1L96.5 23.4L94.5 22.1L93.1 21.9L94 20.6L91.1 16.3L89 15.4L83.6 15.5L81.4 14L84.9 13.4L80 12.3L80.1 11.7L85.7 10.1L86 9.5L84 8.9L88.3 6.8L88 6L92.1 5.2L94.4 5.2L95.2 5.8L97.2 4.8L101.6 6.2L99.8 5.2ZM73.3 70.1L76.2 70.7L79.4 72.4L76.7 72.7L77.2 72.3L76 71.4L73.7 70.6L71.3 71.1Z",
"sa": "M83.5 131.4L84.2 132.7L86.2 133.5L83.9 134.5L81.7 133.9L79 131.6L81.7 132.9L82.3 131.7ZM91.2 114.3L92.4 116.4L91.7 117.9L88.2 118.5L88.4 120.1L86.2 120.5L86.3 121.4L87.4 121.8L86.1 122.7L85.8 124.1L84.5 124.6L84.3 125.3L85.8 126.1L83.1 129.5L83.9 131.1L81.9 131.7L81.4 132.7L78.6 130.4L78.3 127.5L79.4 125.9L78.3 125.6L79.2 123.2L80.1 123.5L80.5 121.7L80 121.4L79.7 122.5L79.3 122.4L80.1 118.9L79.8 117L81.4 113L82.4 102.8L81.4 101L78 98.9L73.9 91.4L75.2 89.8L74.3 89.5L74.9 87.3L77.2 84.9L76.9 81.4L78.2 80.7L78.8 79.5L81.2 78.5L81.6 78.7L81 79.2L81.2 81L81.4 79.6L82.5 78.7L83.9 79.9L86.3 80.2L88.6 79.8L88 80L88.2 80.3L92.1 83.3L94.5 83.5L94.1 86.1L95.3 86.2L96.5 84.7L97.5 86.5L97 87.7L98.5 88L98.6 88.8L99.1 88.3L101.3 89L101.6 89.8L105 90L108.6 91.9L108.7 94.6L106 97.7L105.5 101.4L104.3 104.5L103.5 105.4L99.3 106.9L98.3 109.9L94.6 114.7L92.8 115.1ZM89.1 130.6L91.1 129.9L91.7 130.3L90.5 131Z",
"eu": "M146.4 9.6L146.6 9.1L147.7 9L151.2 10.8L149.3 11.5L147.8 14.3L146.9 14.4L145.3 13.4L146 12.8L143.4 11L142.8 9.7L144.9 9.1L145.3 9.7ZM152.9 29.3L151.6 29.7L151 31.3L148.4 33.3L147.8 34.9L149.1 36.3L147.6 37.8L146.9 40.6L146 40.5L145.6 41.4L144.7 41.4L142.8 37L141.3 38.3L139.2 38L138.7 34.2L139.4 33.4L142.9 31.2L146.1 27L149.4 24.4L152.3 23.9L153.4 22.8L156.1 22.6L158.5 23.6L157.5 23.9L158.3 24.8L156.4 25.4L156.3 26.3L157.5 27.2L156.8 28.2L157.7 29.6L157.2 30.6L157.8 31.5L157.5 32.3L158.6 33.1L156.1 35.8L152.2 36.6L151 35.6L151.2 32.7L154 30.4ZM155.6 9L154.4 9.9L152.3 10.1L150.1 9.8L148 8.5L150.3 8.1L151.4 8.5L152.2 8ZM153.5 12.7L151.9 13.3L150.5 12.9L151.1 12.5L150.6 12L152.2 11.7ZM96.3 84.7L95.3 86.2L94.1 86.1L94.5 83.5ZM142.4 41.8L143.2 42.8L144.4 42.4L145.6 43.1L148.2 42L152 42.5L152.1 42L151 41.6L150.8 39.9L151.9 38.9L153.1 39.7L153.3 38.2L152.5 37.3L156 37L155.5 39.2L156.1 40.6L158.2 41.2L158.1 42L159.5 43.5L158.5 43.8L158.8 44.8L160.3 44.5L161.5 46.3L165.1 47.2L164.9 48.6L161.2 50.4L161.3 50.9L158.1 50.1L156.6 51.7L156 54.3L154.6 54.5L154.5 55.4L152.8 55.5L153.3 56L152 55.9L153 57.7L152.1 58.5L152.4 59.3L151.3 58.9L149.6 55.9L149.5 54.5L144.9 50.9L144.2 51.3L144.4 52.5L148.9 56L147.7 55.7L147.8 57.1L147.1 57.9L146.6 56.1L141.7 52.2L139.9 53.3L137.3 53.4L137.3 54.5L135.1 56L134.5 58.2L133.4 59L130.6 59.6L130.1 58.8L128.3 58.9L128 53.4L129 52.8L133.6 53.1L134.1 50.7L131.6 48.8L131.6 48.1L133.8 48.2L133.5 47.1L134.3 47.5L136 46.7L136.2 45.9L137.9 45.2L138.5 43.8L141.1 43.3L141.6 42.8L141.1 41.3L141.4 39.6L142.9 38.9L142.7 39.8L143.2 40.3L142.2 41.3ZM130.4 43L129.9 44.6L127.5 45L128.1 44L127.7 43L129.3 41.7L130.8 42.3ZM146.1 57.8L146.6 57.7L146.3 59.1L144.3 58.2L144.4 57.8ZM141.5 55.3L141.9 55.1L142.4 55.7L142.3 56.9L141.6 57.1L141.1 55.3ZM132.7 43.4L132.3 42.2L131.2 41.7L131.2 41L130.8 41.5L130.4 39.9L131.2 37.9L132.7 37.9L131.9 39.1L133.5 39L132.7 40.8L133.4 40.8L135.4 43.9L136.3 44.1L136.1 45.6L135.4 46.1L132.8 46.2L131.1 46.9L130.7 46.7L132.4 45.4L131 44.9L131.8 44.6L131.6 43.4ZM124.1 28.8L123.9 29.6L124.8 30.4L121 32.4L117.9 31.8L118.7 31.3L117 30.7L118.3 30.1L116.8 29.8L117.3 29L118.4 28.8L119.6 29.7L120.7 29L121.6 29.3Z",
"af": "M164.4 91.3L164.1 92.7L165.4 95.9L165.6 98.9L164.6 100.5L161.1 102.9L161.6 106.3L159.4 107.6L159.2 110L156.2 113.3L154.3 114.3L151.9 114.2L149.7 115L148.7 114.2L148.7 112.4L146.4 108.7L145.7 104.7L143.8 101.5L143.8 99.8L145.3 95.9L143.9 91.6L141.6 88.7L142.1 85L141.4 84.2L139.4 84.6L138.2 83.1L133.5 84.3L128.2 84.2L125.7 82.4L122.5 78.7L121.8 76.7L122.9 74.1L122.3 71.2L124.2 67.7L127.8 64.7L128 62.5L129.8 61.2L130.6 59.8L133.4 60.3L136.1 59.1L142.1 58.5L143.3 58.8L142.8 61.5L149.3 64.4L150.1 62.8L151.2 62.3L156.7 63.9L158.2 63.4L159 63.9L160.7 63.6L161.2 65.1L160.6 66.4L159.3 64.8L159.5 65.7L161.6 70.2L162.6 71.1L163.1 73.7L164.5 75.8L167.5 78.5L167 79L168.5 80L173.3 78.8L171.4 83.8L165.2 89.8ZM172.2 97.2L172.8 99.7L172.3 99.7L170.3 106.9L169.1 107.5L168 107L167.5 105.2L168.3 102.6L168 101L168.3 100.1L169.7 99.8L171.9 96.9Z",
"as": "M174.4 54.5L174.4 53.6L172.7 52L174.8 51.4L174.8 49.9L171.8 50.3L170 52L172.8 55.9L172.2 56L171.6 57.2L171.9 58.3L173.1 58.9L175.4 58.8L175.4 57.1L174.5 56.1L174.7 55.4L176.1 55.3L175.3 54.2L174.6 55.1ZM270 30.6L268.1 31L269.5 33L269.4 33.8L268 33.5L265.3 34.5L262.7 36.5L261.7 35.8L259.7 36.6L259.4 36.2L257.7 36.5L256.5 38.3L257.4 39L257.3 40.6L256.6 40.6L256.3 41.5L256.6 42L255.3 42.5L255 43.7L253.9 43.9L252.6 45.9L251.6 41.4L251.9 39.9L252.6 38.8L253.8 38.5L257.8 35.1L258.4 33.5L257.4 33.6L255.1 35.8L254.5 34.4L252.5 34.8L250.7 36.7L251.3 37.4L248.4 37.8L248.5 37L247.3 36.8L246.4 37.3L241.6 37.5L236.3 42.1L238.6 43.1L239.9 42.6L241 44.6L240 48.4L238.7 50.4L236.2 53.1L235.2 53.6L234.2 53.2L230.6 56.4L232.1 58.9L231.8 60.4L229.9 61L229.6 59L230.1 58.8L228.5 57.8L229 56.5L228.2 56.2L225.8 57.1L226.6 55.8L226.2 55.3L223.5 56.8L223.1 57.2L224.2 58.4L226.8 58.4L226.9 58.8L225.8 59.1L224.4 60.5L226.4 63.2L225.9 64.1L226.6 64.8L226.3 66.1L224 69L221.9 70.4L218.1 71.5L217.8 72.4L217.4 71.5L216.4 71.3L214.4 72.8L217 77.7L216.9 79L213.9 81.4L213.8 80.4L210.1 77.7L209.4 80.9L212.2 83.7L213.2 86.9L211 85.8L210.1 83L208.9 81.5L208.8 82L209.1 79.2L207.9 75L206.5 75.9L205.6 75.7L205.7 74L203.6 70.5L202.9 70.4L202.7 71.2L200.2 71.5L199.9 72.5L195.2 75.8L194.9 80L193.2 81.8L190.1 75.7L189.5 71.6L187.9 71.9L184.8 68.3L181.1 68.6L178 68.1L177.4 67L175.1 67.2L173.6 66.4L172.6 64.5L171 64.7L173.1 68.9L173.5 67.8L173.8 69.5L175.5 69.4L177.3 67.6L177.6 69.3L179.9 70.8L178.4 72.4L178.3 73.5L176.5 74.8L171.5 77.3L167.6 78.3L166.7 74.6L164.4 71.6L163.9 69.7L161 66.2L160.7 63.6L162.1 59.1L157.3 59.5L155.7 59.1L154.7 57.7L155.1 57L154.6 56.6L156.9 55.1L161.4 54.3L163.8 55.3L165.3 55.2L166.3 54.4L162.5 51.4L163.7 50.5L163.3 50.1L164.4 49.7L163.7 49.2L164.8 48.9L165.1 47.2L161.5 46.3L160.3 44.5L158.8 44.8L158.5 43.8L159.5 43.5L158.1 42L158.2 41.2L156.1 40.6L155.5 39.2L155.6 37.8L156.8 36.4L156.1 35.8L158.6 33.1L157.5 32.3L157.8 31.5L157.2 30.6L157.7 29.6L156.8 28.2L157.5 27.2L156.3 26.3L156.4 25.4L159.1 24.3L162.4 25.4L165.8 27.5L165.8 28.3L163.8 29.3L159.9 28.5L161.1 29.5L161.2 31.3L162.8 31.9L162.4 30.8L162.9 30.4L164.7 31.1L165.3 30.8L164.8 30L166.6 28.7L168 29.2L168.4 28.4L167.6 26.1L169.7 26.5L170.1 27.2L169.2 27.4L169.2 28.1L169.8 28.5L175.3 25.7L175.9 25.8L175.1 26.5L179.1 25.7L180 26.4L180.8 25.6L180 24.8L180.4 24.4L182.6 24.8L186.4 26.7L186.9 26L185.2 24.9L185 22.8L187.5 20L189.4 20.3L188.9 22.3L189.6 23.6L189.4 25.5L190.3 26.3L188.5 28.9L189.3 29.1L191.3 27.1L191.2 25.5L190.4 25.4L190.2 24.7L190.8 23.3L189.8 22.2L191.2 21.3L191 20.3L191.8 21L191.5 22.4L192.3 22.6L191.9 21.6L193.2 21.1L194.7 21L196.1 21.8L195.4 19.1L200.1 18.7L199.5 17.9L200.4 16.9L204.9 15.5L207.5 15.7L210.6 14.9L211.5 13.6L213.3 12.9L214.5 13.4L213.5 13.8L215.2 14.1L215.4 14.8L218.3 14.5L220.6 15.8L220.4 16.6L217 18.3L219.8 18.6L220.1 19.5L221.7 18.9L224.1 19.2L224.3 19.8L227.4 20.1L227.4 19L230.2 19.2L231.4 20L231.8 20.9L231.3 21.5L233.5 23.1L234.2 21.7L235.4 22.3L239.9 22.1L239.4 20.9L240.4 20.2L247.1 21.2L249.7 23L254.2 23L254.9 23.6L254.8 24.5L255.7 24.9L260.9 24.7L262.2 25.9L263.1 25.5L262.5 24.6L262.8 24L268.9 25L270 25.6ZM240.8 89.8L243.4 90.7L245.7 92.4L245.4 93.4L248 95.8L245.9 95.5L243.6 93.6L242 94.8L239.3 93.9L238.2 94.2L239 93.3L238.4 91.9L235.2 90.5L234.7 90.9L234 89.9L235.3 89.5L234.2 89.5L232.9 88.5L235.5 88.4L235.8 89.9L236.6 90.4L238.1 89.1ZM248.5 92.2L246.2 92.1L248.1 91.9L249.1 90.9ZM228.8 94.9L227.6 95.5L228.8 94.3L230.5 94.1ZM221.6 83.7L222.8 82.6L224.4 83.8L223 85.4L224.2 87.2L223.4 87.2L222.1 90.8L217.7 90L216.8 86.8L218.4 86.4ZM227.2 87.2L228.8 86.6L228.3 87.5L225.1 87.7L225 88.2L225.7 88.9L227.5 88.3L226.1 89.3L227.4 91.8L226.7 91.8L227 91.2L226.1 91.3L225.7 89.8L225.2 90L225.3 92L224.8 92.1L224.1 89.9L224.9 87.7L225.7 86.8ZM216.4 92.7L219.5 93L221.8 94.1L214.8 93.4L214 93L214.5 92.3ZM213.3 88.6L214.6 90.1L214.4 92.2L213.5 92.2L211.1 89.9L206.5 83.7L208.1 83.9L210.5 86.3L211.9 86.8ZM205.3 7.3L207 6.9L208.4 7.8L210.1 9.5L210 11L208.3 11.2L205 10.1L204.4 8.8L203.4 8.5ZM212.1 10.3L214 11.3L213.8 11.9L209.6 12.5L210.9 10.4ZM239.1 15.4L243.8 16.2L243.2 17.4L239.2 17.7L237.7 16.7L238.1 15.7ZM246.2 16.6L248 17L246 17.4L244.6 16.8L244.8 16.3ZM239.9 19.5L241.5 18.8L242.7 19.7ZM168.6 8.1L171.2 7.7L171.4 8.2L172.5 7.5L173.6 7.9L170.7 9.1L169.9 8.7L170.3 8.1ZM152 42.5L149.7 42.4L151 41.6L152.1 42ZM175.1 18.9L176.9 17.6L176.7 17L180.9 15.2L186.1 14.1L186.6 14.7L178.9 18.1L176.6 20.9L176.7 22.1L178.2 23.2L175.3 23.1L175.1 22.5L173.7 22.2L174.3 20.3L175.8 19.1ZM242.2 43.1L243.5 47.8L242.4 47.5L241.9 48.9L242.6 50.6L242.1 50L241.6 50.7L241.3 43.6L242 43.1L241.7 42.6ZM160.1 50.7L162.4 51.2L160.4 52.2L159.3 51.3ZM154.6 54.5L156 54.3L156.7 55L154.8 56ZM196.3 82.2L196.2 83L195.3 83.3L194.8 81.7L195.1 80.4ZM217.1 74L216.5 73.1L218.1 72.6ZM226.3 69.2L225.6 71.1L225.1 69.8L226.1 68.5ZM229.8 81.5L229.9 82.4L229.6 83.1L229.4 82.4L229 82.7L229 83.6L228.2 83.2L227.7 81.9L226.4 82.4L227.6 81.3L229.1 81.1L229.1 80.5ZM226.8 74L226.3 77L228 77.4L228.1 78.4L227.2 77.6L225.5 77.4L225.7 76.9L224.9 76.2L225.5 73.8ZM241.4 56.9L240.2 60.4L237.9 60.8L236.8 61.8L236.3 60.8L233.2 61.4L234 62L233.5 63.4L232.7 63.5L232.8 62.7L232.1 61.9L234.5 60.1L236.8 60L237.5 58.5L238 58.9L239.6 57.7L240.2 55.1L241 54.9ZM243.5 52.6L244 52.2L244.2 53.2L242.4 54.4L241.2 53.7L240.8 54.7L240 54.7L239.9 53.8L241 53.1L241.5 51.1ZM234.3 61.8L235.4 61L236.1 61.5L234.8 62.4Z",
"oc": "M267.7 119.6L267 120.7L266 120.7L266.4 119.9L265.4 119.1L266 117.2L264.5 114.8L265.7 115.4L267 117.4L268.9 117.5ZM262.3 122.7L264.6 120L264.9 120.7L265.5 120.3L264.8 123L263.6 123.4L263 124.9L262 125.6L259.9 124.8ZM245.8 120.2L246.2 120.3L245.9 122.4L244.5 122.7L243.6 120.1ZM229.6 112.9L227.7 114.3L224.9 114.3L223.5 115.2L221.3 114.5L221.8 112.4L220 107.9L220.1 107.5L220.7 108L220 106.5L220.6 104.4L220.7 105L222.5 103.6L225.6 102.8L227.3 100.2L227.9 100.8L227.6 100.4L229.3 98.6L230.3 98.3L232.2 99.1L233 97.3L234.4 97L233.9 96.3L234.3 96.2L236.5 97.1L237.4 96.8L237.7 97.1L236.6 99.2L240.2 101.3L241 100.2L241.9 95.9L242.9 98.8L243.4 98.5L244 99.2L244.8 102.2L246.6 103.3L249.9 107.8L249.8 111.8L247.5 117.3L244.7 118.7L243.8 117.7L242.7 118.5L240.5 117.8L239.7 116.2L238.6 115.7L238.7 114.7L237.6 115.4L238.4 113.4L237 115.1L235.7 113.2L233.5 112.3Z"
}

# خريطة العالم: مسارات مبسّطة (إسقاط ميلر) لكل قارة؛ ثابتة ولا تنعكس في العربية لأنها جغرافيا
MAP_COLORS = [('na', '#0F2557'), ('sa', '#2B4C93'), ('eu', '#3D63BC'), ('af', '#8BACE7'), ('as', '#D0DFF7'), ('oc', '#D5DAE4')]
MAP = ('<svg class="map" viewBox="0 0 270 137" aria-hidden="true">'
       + ''.join('<path fill="%s" d="%s"/>' % (c, MAP_PATHS[k]) for k, c in MAP_COLORS) + '</svg>')
SLIDES[1] = SLIDES[1].replace('§MAP§', MAP)
STYLE += '\n  .card.nohead .ln { display: none; }\n  .s3 .card h2 { inset-inline-start: 13px; } .s3 .card .ln { inset-inline: 12px; }'

GLOBAL_DEFS = '<defs id="gdefs"></defs>'

MONTHS = {'en': ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
          'ar': ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو', 'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر']}


def items(lang, n, word=None):
    w = word or {'en': 'Item', 'ar': 'العنصر'}[lang]
    return ['%s %d' % (w, i) for i in range(1, n + 1)]


def both(fn):
    return {'ar': fn('ar'), 'en': fn('en')}


JSON_BLOCKS = {
    # الشريحة 1: أعمدة أفقية مجمّعة لكل موقع
    'chart-data': both(lambda l: {
        'categories': items(l, 4, {'en': 'Location', 'ar': 'الموقع'}[l]), 'max': 3000, 'ticks': [0, 1000, 2000, 3000], 'prefix': '$',
        'series': [{'name': '2025', 'color': '#3561BF', 'values': [2500, 1800, 2500, 1000]},
                   {'name': '2026', 'color': '#8BACE7', 'values': [2500, 2400, 2600, 1500]},
                   {'name': '2027', 'color': '#D0DFF7', 'values': [2500, 2500, 2700, 1800]}]}),
    'chart-data-2': both(lambda l: {
        'categories': ['Item 1', 'Item 2', 'Item 3', 'Item 4', 'item 5', 'Item 6'] if l == 'en' else items(l, 6),
        'values': [165, 145, 130, 105, 85, 75], 'max': 200, 'ticks': [0, 50, 100, 150, 200], 'color': '#3561BF'}),
    # مقياسان نصف دائريين: الجزء الأبيض = القيمة ÷ الحد الأقصى
    'chart-data-3': {'max': 18, 'values': [1.45, 2.45]},
    'chart-data-4': both(lambda l: {
        'categories': ['2026', '2027'], 'max': 15, 'ticks': [0, 5, 10, 15],
        'series': [{'name': '%s 1' % {'en': 'Product', 'ar': 'المنتج'}[l], 'color': '#D0DFF7', 'values': [5.2, 2.8]},
                   {'name': '%s 2' % {'en': 'Product', 'ar': 'المنتج'}[l], 'color': '#8BACE7', 'values': [3.1, 4.2]},
                   {'name': '%s 3' % {'en': 'Product', 'ar': 'المنتج'}[l], 'color': '#3361BF', 'values': [1.8, 1.9]},
                   {'name': '%s 4' % {'en': 'Product', 'ar': 'المنتج'}[l], 'color': '#D5DAE4', 'values': [2.4, 2.0]},
                   {'name': '%s 5' % {'en': 'Product', 'ar': 'المنتج'}[l], 'color': '#0F2557', 'values': [1.7, 1.6]}]}),
    # الشريحة 2: أعمدة صافي المبيعات وخط إجمالي الربح على المحور نفسه (بلا محور ثانٍ)
    'chart-data-5': both(lambda l: {
        'categories': items(l, 5), 'max': 100, 'ticks': [0, 50, 100],
        'axisTitle': {'en': 'Net sales', 'ar': 'صافي المبيعات'}[l], 'lineTitle': {'en': 'Gross profit', 'ar': 'إجمالي الربح'}[l],
        'values': [82.51, 80.45, 55.24, 25.47, 34.56], 'colors': ['#0F2557', '#8BACE7', '#3361BF', '#0F2557', '#D0DFF7'],
        'line': [82, 77, 54, 25, 34]}),
    'chart-data-6': {'items': [{'value': 75, 'color': '#0F2557'}, {'value': 45, 'color': '#2B4C93'}, {'value': 68, 'color': '#3361BF'},
                               {'value': 35, 'color': '#8BACE7'}, {'value': 65, 'color': '#D0DFF7'}, {'value': 25, 'color': '#D5DAE4'}]},
    'chart-data-7': both(lambda l: {'total': 21876, 'share': 35, 'yes': {'en': 'Buyers', 'ar': 'المشترون'}[l],
                                    'no': {'en': 'Non\nBuyers', 'ar': 'غير\nالمشترين'}[l], 'color': '#8BACE7', 'rest': '#F2F2F2'}),
    'chart-data-8': {'categories': ['2/19/2026', '2/20/2026', '2/21/2026', '2/22/2026', '2/23/2026', '2/24/2026', '2/25/2026'],
                     'values': [450, 380, 335, 452, 365, 475, 385], 'min': 250, 'max': 550, 'ticks': [250, 350, 450, 550], 'color': '#3A63B8'},
    # الشريحة 3
    'chart-data-9': both(lambda l: {'categories': items(l, 5), 'values': [95, 74, 65, 32, 15], 'max': 95,
                                    'colors': ['#C3D4F3', '#7FA2E3', '#3361BF', '#0F2557', '#D5DAE4']}),
    'chart-data-10': both(lambda l: {'categories': items(l, 5), 'values': [35, 31, 28, 26, 20], 'max': 38,
                                     'colors': ['#D5DAE4', '#D5DAE4', '#3361BF', '#0F2557', '#D5DAE4']}),
    'chart-data-11': both(lambda l: {'categories': MONTHS[l], 'min': 0, 'max': 4, 'ticks': [0, 2, 4],
                                     'series': [{'name': '2025', 'color': '#8BACE7', 'values': [1.9, 1.85, 1.0, 1.25, 1.3, 1.3, 1.5, 1.5, 1.2, 1.5, 1.9, 2.5]},
                                                {'name': '2026', 'color': '#3361BF', 'values': [0.5, 0.5, 0.2, 0.3, 0.3, 0.4, 0.4, 0.5, 0.5, 0.6, 0.6, 1.0]}]}),
    'chart-data-12': both(lambda l: {'categories': MONTHS[l], 'min': 2, 'max': 2.2, 'ticks': [2, 2.1, 2.2],
                                     'series': [{'name': {'en': 'Units per transaction', 'ar': 'الوحدات لكل معاملة'}[l], 'color': '#8BACE7',
                                                 'values': [2.08, 2.07, 2.06, 2.065, 2.05, 2.08, 2.09, 2.07, 2.08, 2.08, 2.07, 2.10]},
                                                {'name': {'en': 'Price per transaction', 'ar': 'السعر لكل معاملة'}[l], 'color': '#3361BF',
                                                 'values': [2.11, 2.09, 2.06, 2.08, 2.08, 2.10, 2.10, 2.08, 2.06, 2.05, 2.12, 2.09]}]}),
    'chart-data-13': both(lambda l: {'categories': items(l, 4), 'max': 120,
                                     'series': [{'name': 'FY 25', 'color': '#C3D4F3', 'values': [91, 53, 74, 53]},
                                                {'name': 'FY 26 E', 'color': '#3361BF', 'values': [110, 93, 59, 101]}]}),
    'chart-data-14': both(lambda l: {'categories': items(l, 4), 'max': 135,
                                     'series': [{'name': 'FY 25', 'color': '#8BACE7', 'values': [126, 73, 102, 131]},
                                                {'name': 'FY 26 E', 'color': '#0F2557', 'values': [70, 128, 52, 82]}]}),
}

EXTRA_SCRIPT = r"""  // ----- محرك الرسوم: كل رسم يُقرأ من كتلة JSON بمعرّفه ويُرسم بإحداثيات البطاقة؛ X() يعكس الإحداثيات للعربية -----
  var NS = 'http://www.w3.org/2000/svg', GD = document.getElementById('gdefs'), gid = 0, gcache = {};
  function J(id) { return JSON.parse(document.getElementById(id).textContent); }
  function E(tag, a, p) { var e = document.createElementNS(NS, tag); for (var k in a) e.setAttribute(k, a[k]); if (p) p.appendChild(e); return e; }
  function fmt(v) { return Number(v).toLocaleString('en-US'); }
  function host(id) {
    var s = document.querySelector('svg[data-chart="' + id + '"]'); while (s.firstChild) s.removeChild(s.firstChild);
    var W = +s.getAttribute('width');
    return { s: s, W: W, X: function (x) { return RTL ? W - x : x; }, R: function (x, w) { return RTL ? W - x - w : x; },
      T: function (x, y, str, o) { o = o || {}; var t = E('text', { x: RTL ? W - x : x, y: y, 'font-size': o.size || 14, fill: o.fill || '#595959', 'text-anchor': o.anchor || 'middle', 'dominant-baseline': 'central', 'font-weight': o.weight || 400 }, s);
        if (o.rot) t.setAttribute('transform', 'rotate(' + (RTL && o.mirrorRot ? -o.rot : o.rot) + ' ' + (RTL ? W - x : x) + ' ' + y + ')'); t.textContent = str; return t; } };
  }
  // تدرّج: v = من اللون أعلى إلى الشفافية أسفل، h = من الشفافية في جهة البداية إلى اللون في النهاية
  function grad(color, dir, from) {
    var key = color + dir + (from || ''); if (gcache[key]) return 'url(#' + gcache[key] + ')';
    var id = 'jg' + (++gid), h = dir === 'h', rev = h && RTL;
    var g = E('linearGradient', { id: id, x1: rev ? 1 : 0, y1: 0, x2: h ? (rev ? 0 : 1) : 0, y2: h ? 0 : 1 }, GD);
    if (h) { E('stop', { offset: 0, 'stop-color': color, 'stop-opacity': from == null ? 0.05 : from }, g); E('stop', { offset: 1, 'stop-color': color }, g); }
    else { E('stop', { offset: 0, 'stop-color': color }, g); E('stop', { offset: 1, 'stop-color': color, 'stop-opacity': 0.02 }, g); }
    gcache[key] = id; return 'url(#' + id + ')';
  }
  function smooth(pts) {
    var d = 'M' + pts[0][0] + ',' + pts[0][1];
    for (var i = 0; i < pts.length - 1; i++) { var p0 = pts[i - 1] || pts[i], p1 = pts[i], p2 = pts[i + 1], p3 = pts[i + 2] || p2;
      d += ' C' + (p1[0] + (p2[0] - p0[0]) / 6) + ',' + (p1[1] + (p2[1] - p0[1]) / 6) + ' ' + (p2[0] - (p3[0] - p1[0]) / 6) + ',' + (p2[1] - (p3[1] - p1[1]) / 6) + ' ' + p2[0] + ',' + p2[1]; }
    return d;
  }
  function line(h, x1, y1, x2, y2, c) { E('line', { x1: h.X(x1), y1: y1, x2: h.X(x2), y2: y2, stroke: c || '#D9D9D9', 'stroke-width': 1 }, h.s); }
  function arcPath(cx, cy, r, a0, a1) { // زاوية 0 = أعلى، باتجاه عقارب الساعة (يُعكس للعربية عبر X)
    var p = function (a) { return [cx + r * Math.sin(a), cy - r * Math.cos(a)]; }, s = p(a0), e = p(a1), big = a1 - a0 > Math.PI ? 1 : 0;
    return { s: s, e: e, big: big };
  }
  function ring(h, cx, cy, r, w, a0, a1, color) {
    if (a1 - a0 >= 2 * Math.PI - 1e-6) { E('circle', { cx: h.X(cx), cy: cy, r: r, fill: 'none', stroke: color, 'stroke-width': w }, h.s); return; }
    var a = arcPath(cx, cy, r, a0, a1);
    E('path', { d: 'M' + h.X(a.s[0]) + ',' + a.s[1] + ' A' + r + ',' + r + ' 0 ' + a.big + ' ' + (RTL ? 0 : 1) + ' ' + h.X(a.e[0]) + ',' + a.e[1], fill: 'none', stroke: color, 'stroke-width': w }, h.s);
  }

  // 1) أعمدة أفقية مجمّعة
  (function () { var d = J('chart-data'), h = host('chart-data'), x0 = 114, k = 246 / d.max, ys = [93, 125, 158, 191];
    d.ticks.forEach(function (t) { var x = x0 + t * k; line(h, x, 78, x, 205, t ? '#E3E3E3' : '#D9D9D9'); h.T(x, 231, d.prefix + fmt(t), { size: 15 }); });
    d.categories.forEach(function (c, i) { h.T(97, ys[i], c, { anchor: 'end', size: 15 });
      d.series.forEach(function (s, j) { var w = s.values[i] * k; E('rect', { x: h.R(x0, w), y: ys[i] - 8 + j * 5.5, width: w, height: 5.5, fill: s.color }, h.s); }); });
    d.series.forEach(function (s, j) { var y = 97 + j * 38; E('rect', { x: h.R(419, 10), y: y - 5, width: 10, height: 10, fill: s.color }, h.s); h.T(435, y, s.name, { anchor: 'start', size: 16 }); });
  })();
  // 2) أعمدة رأسية متدرّجة
  (function () { var d = J('chart-data-2'), h = host('chart-data-2'), base = 233, k = 160 / d.max, xs = [99, 167, 236, 304, 373, 441];
    d.ticks.forEach(function (t) { h.T(49, base - t * k, fmt(t), { anchor: 'end', size: 15 }); });
    line(h, 66, 73, 66, base); line(h, 66, base, 475, base);
    d.values.forEach(function (v, i) { var bh = v * k; E('rect', { x: h.R(xs[i] - 12.5, 25), y: base - bh, width: 25, height: bh - 1, fill: grad(d.color, 'v') }, h.s); h.T(xs[i], 256, d.categories[i], { size: 15 }); });
  })();
  // 3) مقياسان نصف دائريين
  (function () { var d = J('chart-data-3'), h = host('chart-data-3'), cs = [130, 371], cy = 187, r1 = 97, r0 = 64, vy = [169, 157];
    function band(cx, t0, t1, fill, stroke) { // t: 0 = بداية القوس (يسار)، 1 = نهايته
      var a = function (t, r) { var th = Math.PI * t; return [h.X(cx - r * Math.cos(th)), cy - r * Math.sin(th)]; }, sw = RTL ? 0 : 1;
      var p1 = a(t0, r1), p2 = a(t1, r1), p3 = a(t1, r0), p4 = a(t0, r0);
      E('path', { d: 'M' + p1 + ' A' + r1 + ',' + r1 + ' 0 0 ' + sw + ' ' + p2 + ' L' + p3 + ' A' + r0 + ',' + r0 + ' 0 0 ' + (1 - sw) + ' ' + p4 + 'Z', fill: fill, stroke: stroke || 'none', 'stroke-width': 1.6, 'stroke-linejoin': 'round' }, h.s);
    }
    d.values.forEach(function (v, i) { var f = Math.max(0, Math.min(1, v / d.max));
      band(cs[i], 0, 1, '#8BACE7', '#fff'); if (f > 0) band(cs[i], 0, f, '#fff');
      h.T(cs[i], vy[i], String(v), { size: 33, weight: 700, fill: '#fff' }); });
  })();
  // 4) أعمدة مكدّسة
  (function () { var d = J('chart-data-4'), h = host('chart-data-4'), base = 235, k = 200 / d.max, xs = [76, 225];
    d.ticks.forEach(function (t) { h.T(37, base - t * k, fmt(t), { anchor: 'end', size: 15 }); });
    line(h, 49, 35, 49, base); line(h, 49, base, 346, base);
    d.categories.forEach(function (c, i) { var y = base; d.series.forEach(function (s) { var bh = s.values[i] * k; y -= bh; E('rect', { x: h.R(xs[i], 93), y: y, width: 93, height: bh, fill: s.color }, h.s); }); h.T(xs[i] + 46.5, 258, c, { size: 15 }); });
    d.series.slice().reverse().forEach(function (s, j) { var y = 50 + j * 48; E('rect', { x: h.R(391, 12), y: y - 6, width: 12, height: 12, fill: s.color }, h.s); h.T(409, y, s.name, { anchor: 'start', size: 15, fill: '#262626' }); });
  })();
  // 5) أعمدة صافي المبيعات + خط إجمالي الربح على المقياس نفسه
  (function () { var d = J('chart-data-5'), h = host('chart-data-5'), base = 164, k = 85 / d.max, xs = [134, 217, 299, 381, 463];
    h.T(29, 128, d.axisTitle, { size: 17, weight: 700, fill: '#000', rot: -90 });
    d.ticks.forEach(function (t) { h.T(78, base - t * k, fmt(t), { anchor: 'end', size: 15, fill: '#262626' }); });
    d.values.forEach(function (v, i) { var bh = v * k; E('rect', { x: h.R(xs[i] - 26, 52), y: base - bh, width: 52, height: bh, fill: grad(d.colors[i], 'v') }, h.s);
      h.T(xs[i], base - bh - 15, v.toFixed(2), { size: 15, fill: '#262626' }); h.T(xs[i], 187, d.categories[i], { size: 15, fill: '#262626' }); });
    E('path', { d: smooth(d.line.map(function (v, i) { return [h.X(xs[i]), base - v * k]; })), fill: 'none', stroke: '#D9D9D9', 'stroke-width': 4, 'stroke-linecap': 'round' }, h.s);
    h.T(574, 128, d.lineTitle, { size: 17, weight: 700, fill: '#000', rot: -90 });
  })();
  // 6) حلقات النِّسب حول الخريطة
  (function () { var d = J('chart-data-6'), h = host('chart-data-6'), pos = [[49, 95], [49, 155], [49, 215], [409, 95], [409, 155], [409, 215]];
    d.items.forEach(function (it, i) { var p = pos[i]; ring(h, p[0], p[1], 21, 12, 0, 2 * Math.PI, '#F0F0F0'); ring(h, p[0], p[1], 21, 12, 0, 2 * Math.PI * it.value / 100, it.color);
      h.T(p[0], p[1], it.value + '%', { size: 14, weight: 700, fill: '#111' }); });
  })();
  // 7) حلقة معدل التحويل
  (function () { var d = J('chart-data-7'), h = host('chart-data-7'), cx = 137, cy = 155;
    ring(h, cx, cy, 60.5, 31, 0, 2 * Math.PI, d.rest); ring(h, cx, cy, 60.5, 31, 0, 2 * Math.PI * d.share / 100, d.color);
    h.T(cx, 150, fmt(d.total), { size: 16, fill: '#262626' }); h.T(cx, 174, '(' + d.share + '%)', { size: 20, weight: 700, fill: '#111' });
    h.T(190, 110, d.yes, { anchor: 'start', size: 15, weight: 700, fill: '#262626' });
    d.no.split('\n').forEach(function (s, j) { h.T(64, 185 + j * 20, s, { size: 15, weight: 700, fill: '#262626' }); });
  })();
  // 8) خط قيمة السلة بتسميات تواريخ مائلة
  (function () { var d = J('chart-data-8'), h = host('chart-data-8'), base = 170, k = 86 / (d.max - d.min), xs = [80, 133, 185, 238, 290, 343, 396];
    d.ticks.forEach(function (t) { h.T(40, base - (t - d.min) * k, fmt(t), { anchor: 'end', size: 14, fill: '#262626' }); });
    line(h, 53, base, 423, base);
    var pts = d.values.map(function (v, i) { return [h.X(xs[i]), base - (v - d.min) * k]; });
    E('path', { d: smooth(pts), fill: 'none', stroke: d.color, 'stroke-width': 3, 'stroke-linecap': 'round' }, h.s);
    d.values.forEach(function (v, i) { h.T(xs[i], pts[i][1] - 14, fmt(v), { size: 14, fill: '#262626' }); h.T(xs[i] + 6, 182, d.categories[i], { anchor: 'end', size: 14, fill: '#262626', rot: -45, mirrorRot: true }); });
  })();
  // 9، 10) أعمدة أفقية متدرّجة لأكثر المنتجات مبيعًا
  function topBars(id, ys) { var d = J(id), h = host(id), x0 = 80, k = 174 / d.max;
    line(h, 75, ys[0] - 12, 75, ys[ys.length - 1] + 12);
    d.values.forEach(function (v, i) { var w = v * k; h.T(13, ys[i], d.categories[i], { anchor: 'start', size: 15, fill: '#8C8C8C' });
      E('rect', { x: h.R(x0, w), y: ys[i] - 7.5, width: w, height: 15, fill: grad(d.colors[i], 'h') }, h.s); h.T(x0 + w + 7, ys[i] + 1, fmt(v), { anchor: 'start', size: 15, fill: '#595959' }); });
  }
  topBars('chart-data-9', [102, 127, 152, 177, 202]); topBars('chart-data-10', [102, 128, 153, 179, 205]);
  // 11، 12) خطوط شهرية بسلسلتين
  function lines(id, L) { var d = J(id), h = host(id), k = (L.base - L.top) / (d.max - d.min);
    d.ticks.forEach(function (t) { h.T(L.tickX, L.base - (t - d.min) * k, String(t), { anchor: 'end', size: 15, fill: '#7F7F7F' }); });
    line(h, L.left, L.base, L.right, L.base);
    d.categories.forEach(function (c, i) { h.T(L.x0 + i * L.step, L.labY, c, { size: RTL ? 10.5 : 15, fill: '#7F7F7F' }); });
    d.series.forEach(function (s, j) { var pts = s.values.map(function (v, i) { return [h.X(L.x0 + i * L.step), L.base - (v - d.min) * k]; });
      E('path', { d: smooth(pts), fill: 'none', stroke: s.color, 'stroke-width': 3, 'stroke-linecap': 'round' }, h.s);
      var lx = L.leg[j]; E('line', { x1: h.X(lx), y1: L.legY, x2: h.X(lx + 30), y2: L.legY, stroke: s.color, 'stroke-width': 3 }, h.s); h.T(lx + 33, L.legY, s.name, { anchor: 'start', size: 15, fill: '#262626' }); });
  }
  lines('chart-data-11', { base: 190, top: 91, tickX: 22, left: 35, right: 522, x0: 57, step: 40.2, labY: 213, leg: [379, 460], legY: 73 });
  lines('chart-data-12', { base: 191, top: 84, tickX: 42, left: 52, right: 517, x0: 74, step: 38.3, labY: 215, leg: [137, 324], legY: 82 });
  // 13، 14) أعمدة مجمّعة متدرّجة (فعلي مقابل متوقع)
  function pairs(id, L) { var d = J(id), h = host(id), k = L.h / d.max, gx = [38, 121, 205, 289];
    line(h, 12, L.base, 346, L.base);
    d.categories.forEach(function (c, i) { d.series.forEach(function (s, j) { var bh = s.values[i] * k;
      E('rect', { x: h.R(gx[i] + j * 17, 17), y: L.base - bh, width: 17, height: bh - 2, fill: grad(s.color, 'v') }, h.s); });
      h.T(gx[i] + 17, L.labY, c, { size: 15, fill: '#7F7F7F' }); });
    d.series.forEach(function (s, j) { var lx = L.leg[j]; E('rect', { x: h.R(lx, 8), y: L.legY - 7, width: 8, height: 14, fill: grad(s.color, 'v') }, h.s); h.T(lx + 13, L.legY, s.name, { anchor: 'start', size: 16, fill: '#262626' }); });
  }
  pairs('chart-data-13', { base: 240, h: 120, labY: 264, leg: [200, 266], legY: 89 });
  pairs('chart-data-14', { base: 244, h: 135, labY: 268, leg: [206, 271], legY: 64 });
"""

STR = {
    'en': {
        'T1': 'RETAIL DASHBOARD – SALES KPI’s', 'C11': 'Sales Square Foot / Location', 'C12': 'Units per Transaction', 'C13': 'Sales Square Foot / Location',
        'FOOT': 'Net income per square foot', 'FY1': 'FY 26', 'FY2': 'FY 27',
        'K1V': '2.7bn', 'K1L': 'Net Profit', 'K2V': '4.2bn', 'K2L': 'Revenues', 'K3V': '2.7bn', 'K3L': 'Net Profit', 'K4V': '2.7bn', 'K4L': 'Customer retention Rate',
        'T2': 'RETAIL DASHBOARD – SALES BY GEO,KEY STORES &amp; CONV. RATE', 'CO': 'COMPANY<br>NAME',
        'B1L': 'Global Sales', 'B1V': '$291,987', 'B2L': 'Daily Average Sales', 'B2V': '$45', 'B3L': 'Value of Customer Transaction (Average)', 'B3V': '$54',
        'B4L': 'Avg. Units by Customers', 'B4V': '6PCS',
        'C21': 'Key Store Revenues', 'TH1': 'Name', 'TH2': 'Total sales', 'TH3': 'Discount', 'TH4': 'Net sales', 'TH5': 'COGS', 'TH6': 'Gross profit', 'TH7': 'Gross margin', 'ST': 'Store',
        'C22': 'Sales by Category, `000s', 'C23': 'Sales by Geography', 'C24': 'Conversion Rate', 'C25': 'Basket Value (avg)',
        'T3': 'RETAIL DASHBOARD – STRATEGY – SALES &amp; INVENTORY',
        'D1L': 'Global Sales', 'D1V': '$315.21 mm', 'D2L': 'Total no of customers', 'D2V': '$4.65 mm', 'D3L': 'Value of Customer Transaction (Average)', 'D3V': '$65.35',
        'D4L': 'Avg.<br>Units by customers', 'D4V': '2.32',
        'C31': 'Top products sold FY 26', 'C32': 'Top products sold FY 27', 'C33': 'Sales per employee – Visitors vs. Total customers', 'C34': 'Price unit every transaction',
        'C35': 'Sales actual vs. forecasted value', 'C36': 'Sales actual vs. forecasted', 'MIL': 'Millions',
    },
    'ar': {
        'T1': 'لوحة البيع بالتجزئة – مؤشرات أداء المبيعات', 'C11': 'المبيعات لكل قدم مربع / الموقع', 'C12': 'الوحدات لكل معاملة', 'C13': 'المبيعات لكل قدم مربع / الموقع',
        'FOOT': 'صافي الدخل لكل قدم مربع', 'FY1': 'FY 26', 'FY2': 'FY 27',
        'K1V': '2.7bn', 'K1L': 'صافي الربح', 'K2V': '4.2bn', 'K2L': 'الإيرادات', 'K3V': '2.7bn', 'K3L': 'صافي الربح', 'K4V': '2.7bn', 'K4L': 'معدل الاحتفاظ بالعملاء',
        'T2': 'لوحة التجزئة – المبيعات حسب المنطقة والمتاجر الرئيسية ومعدل التحويل', 'CO': 'اسم<br>الشركة',
        'B1L': 'المبيعات العالمية', 'B1V': '$291,987', 'B2L': 'متوسط المبيعات اليومية', 'B2V': '$45', 'B3L': 'قيمة معاملة العميل (المتوسط)', 'B3V': '$54',
        'B4L': 'متوسط الوحدات لكل عميل', 'B4V': '6 قطع',
        'C21': 'إيرادات المتاجر الرئيسية', 'TH1': 'الاسم', 'TH2': 'إجمالي المبيعات', 'TH3': 'الخصم', 'TH4': 'صافي المبيعات', 'TH5': 'تكلفة المبيعات', 'TH6': 'إجمالي الربح', 'TH7': 'هامش الربح', 'ST': 'المتجر',
        'C22': 'المبيعات حسب الفئة (بالآلاف)', 'C23': 'المبيعات حسب المنطقة الجغرافية', 'C24': 'معدل التحويل', 'C25': 'قيمة السلة (المتوسط)',
        'T3': 'لوحة البيع بالتجزئة – الاستراتيجية – المبيعات والمخزون',
        'D1L': 'المبيعات العالمية', 'D1V': '$315.21 مليون', 'D2L': 'إجمالي عدد العملاء', 'D2V': '$4.65 مليون', 'D3L': 'قيمة معاملة العميل (المتوسط)', 'D3V': '$65.35',
        'D4L': 'متوسط<br>الوحدات لكل عميل', 'D4V': '2.32',
        'C31': 'أكثر المنتجات مبيعًا FY 26', 'C32': 'أكثر المنتجات مبيعًا FY 27', 'C33': 'المبيعات لكل موظف – الزوار مقابل إجمالي العملاء', 'C34': 'سعر الوحدة في كل معاملة',
        'C35': 'المبيعات الفعلية مقابل القيمة المتوقعة', 'C36': 'المبيعات الفعلية مقابل المتوقعة', 'MIL': 'بالملايين',
    },
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'sales', 'icon': '🏬',
    'name': 'لوحة أداء المتجر الزرقاء', 'nameEn': 'Blue Store Performance Dashboard',
    'description': 'ثلاث لوحات بيع بالتجزئة: مؤشرات المبيعات، المبيعات حسب المنطقة والمتاجر ومعدل التحويل، والاستراتيجية والمخزون',
    'descriptionEn': 'Three retail dashboards: sales KPIs, sales by geography, stores and conversion, and strategy and inventory',
    'sections': [
        {'title': 'مؤشرات أداء المبيعات', 'titleEn': 'Sales KPIs', 'needsData': True},
        {'title': 'المبيعات حسب المنطقة والمتاجر ومعدل التحويل', 'titleEn': 'Sales by Geo, Key Stores & Conversion', 'needsData': True},
        {'title': 'الاستراتيجية: المبيعات والمخزون', 'titleEn': 'Strategy: Sales & Inventory', 'needsData': True},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط ومحرك الرسوم، وغيّر النصوص والأرقام فقط؛ قيم الرسوم وتسمياتها في كتل JSON بالمعرّفات chart-data إلى chart-data-14، والمؤشرات والجدول نصوص HTML.',
}
