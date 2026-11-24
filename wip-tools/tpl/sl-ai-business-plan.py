# -*- coding: utf-8 -*-
# نموذج «خطة عمل بالذكاء الاصطناعي»: 16 شريحة بهوية داكنة (كحلي/بنفسجي) وأشكال سداسية مشطوفة،
# شرائح غلاف وفهرس وفاصل داكنة، وشرائح محتوى بيضاء بعنوان كحلي ولوحات داكنة مائلة وبطاقات بإطار بنفسجي،
# ورسمان بيانيان (أعمدة مجمّعة ومكدّسة) ودائرتان مفرغتان تُرسم كلها من كتل chart-data.
KEY = 'sl-ai-business-plan'
TITLE = {'ar': 'خطة عمل بالذكاء الاصطناعي', 'en': 'AI Business Plan'}
COMMENT = ('نموذج «خطة عمل بالذكاء الاصطناعي»: 16 شريحة. الأشكال الزخرفية واللوحات الداكنة وإطارات البطاقات مرسومة SVG في '
           'طبقة deco تُعكس في العربية بـ scaleX(-1)، والنصوص HTML فوقها بإحداثيات منطقية. الرسوم من كتل chart-data.')
VARS = ('--ink: #1E1E1E; --ink-2: #2B2B2B; --navy-t: #163A6B; --accent: #7532BF; --purple-t: #6B2FB3; --navy: #1C3367; '
        '--plum: #3A1B61; --pink: #F3698D; --cyan: #69E2DB; --blue: #3D68C7; --blue-t: #4F7BE0; --lilac: #AB7FE1; '
        '--gray: #D9D9D9; --bg: #FFFFFF;')

# ---------- أدوات هندسية ----------


def P(pts):
    return ' '.join('%g,%g' % p for p in pts)


def cham(x, y, w, h, c, kind='\\'):
    """مستطيل بزاويتين مشطوفتين: '\\' يشطف الأعلى-يمين والأسفل-يسار، '/' يشطف الأعلى-يسار والأسفل-يمين."""
    if kind == '\\':
        return [(x, y), (x + w - c, y), (x + w, y + c), (x + w, y + h), (x + c, y + h), (x, y + h - c)]
    return [(x + c, y), (x + w, y), (x + w, y + h - c), (x + w - c, y + h), (x, y + h), (x, y + c)]


def poly(pts, cls):
    return '<polygon class="%s" points="%s"/>' % (cls, P(pts))


def deco(inner, cls=''):
    return '<svg class="deco%s" viewBox="0 0 1280 720" aria-hidden="true">%s</svg>' % ((' ' + cls) if cls else '', inner)


def bgrect(g):
    return '<rect width="1280" height="720" fill="url(#%s)"/>' % g


# عنقود الأشكال العلوي (كما في الشريحة 3 أعلى اليسار)؛ يُنقل بـ translate في شرائح أخرى
def cluster_top(dx=0, dy=0, white_hex=None):
    s = ''.join([
        poly(cham(190, -40, 110, 170, 50), 'cy'),
        poly(cham(295, -30, 85, 120, 40), 'bl'),
        poly(cham(10, 185, 160, 135, 55), 'pk'),
        poly(cham(370, 130, 105, 105, 45), 'cy'),
        poly(cham(235, 235, 65, 150, 30), 'nv'),
        poly(cham(40, -145, 260, 340, 130), 'ln'),
        poly(cham(170, 65, 265, 295, 130, '/'), 'ln'),
    ])
    if white_hex:
        s += poly(cham(*white_hex, kind='/'), 'wh')
    return '<g transform="translate(%d %d)">%s</g>' % (dx, dy, s)


# زوج الزاوية السفلية اليمنى: سداسي وردي باهت وسداسي أبيض
def corner_pair(y):
    return poly(cham(1115, y, 110, 130, 45, '/'), 'pk') + poly(cham(1170, y + 118, 125, 125, 45, '/'), 'wh')


# عنقود أعلى اليمين لشرائح المحتوى (9، 11)
CLUSTER_TR = ''.join([
    poly(cham(855, -10, 160, 225, 70), 'nv'),
    poly(cham(855, -50, 150, 160, 60), 'pk'),
    poly(cham(1012, -90, 62, 240, 35, '/'), 'ln'),
    poly(cham(1070, 55, 110, 105, 45, '/'), 'pk'),
    poly(cham(1070, 55, 110, 105, 45, '/'), 'ln'),
    poly(cham(1170, -50, 80, 100, 35), 'cy'),
])

# ---------- الشعار والأيقونات ----------
LOGO = ('<svg viewBox="0 0 48 48" aria-hidden="true"><path d="M11 7h26a4 4 0 0 1 4 4v26a4 4 0 0 1-4 4H11a4 4 0 0 1-4-4V11a4 4 0 0 1 4-4z" '
        'fill="none" stroke="currentColor" stroke-width="6" transform="rotate(-8 24 24)"/></svg>')
STAR = '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12 3l2.6 5.6 6.1.7-4.5 4.2 1.2 6-5.4-3-5.4 3 1.2-6L3.3 9.3l6.1-.7z"/></svg>'
DASH = '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><rect x="6" y="10.5" width="12" height="3" rx="1.5"/></svg>'
ARROW_C = ('<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">'
           '<circle cx="12" cy="12" r="9"/><path d="M8 12h8m-3-3 3 3-3 3"/></svg>')
CURSOR = ('<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linejoin="round" aria-hidden="true">'
          '<path d="M4 3l17 7-7.5 2.5L11 20z"/></svg>')
UP = ('<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" aria-hidden="true">'
      '<circle cx="12" cy="12" r="9.5"/><path d="M12 17V7m-4 4 4-4 4 4"/></svg>')
DOWN = ('<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" aria-hidden="true">'
        '<circle cx="12" cy="12" r="9.5"/><path d="M12 7v10m-4-4 4 4 4-4"/></svg>')
MAIL = ('<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linejoin="round" aria-hidden="true">'
        '<path d="M3 9l9-6 9 6v11H3z"/><path d="M3 9l9 6 9-6M7 11V5h10v6"/></svg>')
GLOBE = ('<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" aria-hidden="true">'
         '<circle cx="12" cy="12" r="9"/><path d="M3 12h18M12 3c3 3 3 15 0 18M12 3c-3 3-3 15 0 18"/></svg>')
PHONE = ('<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linejoin="round" aria-hidden="true">'
         '<path d="M6 3h3l2 5-2.5 1.5a11 11 0 0 0 6 6L16 13l5 2v3a2 2 0 0 1-2 2A16 16 0 0 1 4 5a2 2 0 0 1 2-2z"/><path d="M15 4a5 5 0 0 1 5 5M15 7a2 2 0 0 1 2 2"/></svg>')

# ---------- التدرّجات المشتركة ----------


def lg(i, stops, x1=0, y1=0, x2=1, y2=1):
    return '<linearGradient id="%s" x1="%g" y1="%g" x2="%g" y2="%g">%s</linearGradient>' % (
        i, x1, y1, x2, y2, ''.join('<stop offset="%s" stop-color="%s"%s/>' % (o, c, (' stop-opacity="%s"' % a) if a is not None else '') for o, c, a in stops))


GLOBAL_DEFS = '<defs>' + ''.join([
    lg('bg1', [('0', '#3A1B61', None), ('.42', '#1C3367', None), ('1', '#1C3367', None)], 0, 0, 1, .6),
    lg('bg2', [('0', '#1C3367', None), ('.45', '#1C3367', None), ('1', '#3A1B61', None)]),
    lg('bg3', [('0', '#341F62', None), ('.45', '#253270', None), ('.7', '#2C3379', None), ('1', '#4B2E9A', None)], 0, .3, 1, .7),
    lg('bg15', [('0', '#3A1B61', None), ('.45', '#203469', None), ('.75', '#2C3379', None), ('1', '#4B2E9A', None)], 0, 0, 1, .6),
    lg('bg16', [('0', '#7032BB', None), ('.4', '#263270', None), ('1', '#1C3367', None)], 0, 1, 1, 0),
    lg('pnl', [('0', '#2C2662', None), ('.35', '#223172', None), ('.55', '#1F3066', None), ('.85', '#4A2F97', None), ('1', '#7032BB', None)], 1, 0, 0, 1),
    lg('pnl2', [('0', '#2C2662', None), ('.35', '#223172', None), ('.55', '#1F3066', None), ('.85', '#4A2F97', None), ('1', '#7032BB', None)], 0, 0, 1, 1),
    lg('dk', [('0', '#361C65', None), ('.45', '#1D3267', None), ('1', '#6E32B8', None)]),
    lg('team', [('0', '#3A1B5F', None), ('.2', '#312263', None), ('.3', '#2B2666', None), ('.4', '#202F66', None), ('.46', '#1C3168', None), ('.72', '#2A2F7E', None), ('1', '#7234B3', None)], 0, 0, 1, 0),
    lg('band14', [('0', '#391C5F', None), ('.5', '#1F3368', None), ('1', '#4C3297', None)], 0, 0, 1, 0),
    lg('pk', [('0', '#F3698D', '.95'), ('1', '#F3698D', '0')]),
    lg('cy', [('0', '#69E2DB', '.95'), ('1', '#69E2DB', '0')]),
    lg('bl', [('0', '#3D68C7', '.8'), ('1', '#3D68C7', '0')]),
    lg('nv', [('0', '#1A2B6B', '1'), ('1', '#1A2B6B', '.25')]),
    lg('lav', [('0', '#DBC9EF', None), ('1', '#FFFFFF', None)], 0, 0, 0, 1),
    lg('rose', [('0', '#FBC8D9', None), ('1', '#FFFFFF', None)], 0, 0, 0, 1),
    lg('col1', [('0', '#3A1C66', None), ('.45', '#2A2A6C', None), ('1', '#8A4CC9', None)], 0, 0, .1, 1),
    lg('gray', [('0', '#E0E0E0', '1'), ('1', '#E0E0E0', '0')], 0, 0, 0, 1),
]) + '</defs>'

# ---------- CSS ----------
STYLE = """  .deco { position: absolute; inset: 0; width: 1280px; height: 720px; pointer-events: none; }
  html[dir="rtl"] .deco, html[dir="rtl"] .mir { transform: scaleX(-1); }
  .deco .ln { fill: none; stroke: #FFFFFF; stroke-width: 2.5; } .deco .wh { fill: #FFFFFF; }
  .deco .pk { fill: url(#pk); } .deco .cy { fill: url(#cy); } .deco .bl { fill: url(#bl); } .deco .nv { fill: url(#nv); }
  .deco .pnl { fill: url(#pnl); } .deco .pnl2 { fill: url(#pnl2); }
  .deco .pb { fill: none; stroke: var(--accent); stroke-width: 2.5; stroke-linejoin: miter; }
  .deco .pbw { fill: #FFFFFF; stroke: var(--accent); stroke-width: 2.5; }
  .deco .pf { fill: var(--accent); }
  .frame > * { position: absolute; margin: 0; }
  .frame p, .frame h1, .frame h2, .frame h3 { margin: 0; }
  /* أبناء الحاويات المطلقة (بطاقات، لوحات الفريق، المؤشرات) مطلقون داخلها */
  .s4 .card > *, .s13 .card > *, .s7 .mc > *, .s9 .dc > *, .s12 .kp > *, .s15 .qi > *, .s16 .ci > *, .s11 .tm > *, .s6 .col > * { position: absolute; margin: 0; }
  /* عناصر مشتركة */
  .co { top: 72px; inset-inline-start: 42px; font-size: 16px; letter-spacing: 3px; text-transform: uppercase; color: var(--ink); white-space: nowrap; }
  .dark .co { color: #FFFFFF; }
  html[lang="ar"] .co { letter-spacing: 0; font-size: 18px; }
  .ttl { top: 112px; inset-inline-start: 36px; font-size: 40px; font-weight: 800; color: var(--navy-t); line-height: 1.2; white-space: nowrap; }
  .ttl span { font-weight: 400; }
  .stmt { font-size: 24px; font-weight: 700; line-height: 1.33; color: var(--ink); }
  .txt { font-size: 17px; line-height: 1.4; color: var(--ink-2); }
  .w { color: #FFFFFF; }
  .lg { display: flex; align-items: center; gap: 24px; color: var(--accent); }
  .lg svg { width: 56px; height: 56px; flex: none; }
  .lg p { font-size: 16px; letter-spacing: 3px; text-transform: uppercase; color: #FFFFFF; line-height: 1.45; white-space: nowrap; }
  html[lang="ar"] .lg p { letter-spacing: 0; font-size: 18px; }
  .lg.wl { color: #FFFFFF; }
  .badge { width: 90px; height: 76px; color: #FFFFFF; font-size: 30px; font-weight: 700; display: grid; place-items: center; z-index: 2; }
  .badge::before { content: ''; position: absolute; inset: 0; background: var(--accent); z-index: -1; clip-path: polygon(24% 0, 100% 0, 100% 100%, 24% 100%, 0 70%, 0 30%); }
  html[dir="rtl"] .badge::before { clip-path: polygon(0 0, 76% 0, 100% 30%, 100% 70%, 76% 100%, 0 100%); }
  .hd { font-size: 24px; font-weight: 700; line-height: 1.33; color: var(--purple-t); }
  .hd2 { font-size: 19px; font-weight: 700; line-height: 1.3; color: var(--purple-t); }
  .num { font-size: 40px; font-weight: 700; line-height: 1.2; }
  /* ---- 1 الغلاف ---- */
  .s1 .lg { top: 85px; inset-inline-start: 70px; }
  .s1 .ai { top: 200px; inset-inline-start: 76px; font-size: 118px; font-weight: 800; color: #FFFFFF; line-height: 1; letter-spacing: -2px; }
  html[lang="ar"] .s1 .ai { font-size: 76px; top: 215px; letter-spacing: 0; }
  .s1 .bp { top: 338px; inset-inline-start: 76px; font-size: 62px; font-weight: 800; color: #FFFFFF; line-height: 1; letter-spacing: -1px; }
  html[lang="ar"] .s1 .bp { font-size: 58px; letter-spacing: 0; }
  .s1 .pt { top: 432px; inset-inline-start: 78px; font-size: 34px; font-weight: 300; color: #FFFFFF; letter-spacing: 5px; line-height: 1.2; }
  html[lang="ar"] .s1 .pt { letter-spacing: 0; font-weight: 400; }
  /* ---- 2 الفهرس ---- */
  .s2 .ttl { color: #FFFFFF; font-size: 58px; line-height: 1.3; top: 112px; white-space: normal; width: 420px; }
  .s2 .it { width: 170px; }
  .s2 .it .n { font-size: 28px; font-weight: 300; color: var(--cyan); line-height: 1.2; }
  .s2 .it h2 { font-size: 25px; font-weight: 700; color: #FFFFFF; line-height: 1.28; margin-top: 18px; }
  .s2 .it1 { top: 118px; inset-inline-start: 565px; } .s2 .it2 { top: 295px; inset-inline-start: 565px; } .s2 .it3 { top: 471px; inset-inline-start: 565px; }
  .s2 .it4 { top: 118px; inset-inline-start: 885px; } .s2 .it5 { top: 295px; inset-inline-start: 885px; }
  .s2 .box { top: 555px; inset-inline-start: 878px; width: 340px; font-size: 24px; font-weight: 700; line-height: 1.33; color: #FFFFFF; }
  /* ---- 3 فاصل ---- */
  .s3 .big { top: 262px; inset-inline-start: 205px; font-size: 200px; font-weight: 800; color: #FFFFFF; line-height: 1; letter-spacing: 8px; }
  .s3 .tp { top: 175px; inset-inline-start: 645px; font-size: 40px; font-weight: 700; line-height: 1.25; color: var(--cyan); width: 480px; }
  .s3 .stmt { top: 455px; inset-inline-start: 650px; width: 440px; color: #FFFFFF; }
  .s3 .txt { top: 543px; inset-inline-start: 650px; color: #FFFFFF; }
  .s3 .lg { top: 575px; inset-inline-start: 70px; }
  /* ---- 4 المشكلة (و13 الطلب) ---- */
  .s4 .stmt { top: 200px; inset-inline-start: 36px; width: 440px; }
  .s4 .txt { top: 288px; inset-inline-start: 36px; }
  .s4 .st { width: 260px; color: #FFFFFF; }
  .s4 .st .v { font-size: 36px; font-weight: 300; line-height: 1.2; }
  .s4 .st .bar { width: 60px; height: 8px; background: var(--pink); margin: 12px 0 22px; }
  .s4 .st .txt { color: #FFFFFF; }
  .s4 .st1 { top: 165px; inset-inline-start: 665px; } .s4 .st2 { top: 165px; inset-inline-start: 968px; }
  .s4 .card { width: 384px; height: 238px; }
  .s4 .card .badge { top: -33px; inset-inline-start: 14px; }
  .s4 .card .hd { top: 70px; inset-inline-start: 74px; width: 170px; }
  .s4 .card .txt { top: 150px; inset-inline-start: 74px; width: 250px; }
  .s4 .c1 { top: 380px; inset-inline-start: 36px; } .s4 .c2 { top: 380px; inset-inline-start: 442px; } .s4 .c3 { top: 380px; inset-inline-start: 852px; }
  /* ---- 5 الحل ---- */
  .s5 .stmt { top: 198px; inset-inline-start: 36px; width: 440px; }
  .s5 .txt { top: 286px; inset-inline-start: 36px; }
  .s5 .tl { width: 170px; }
  .s5 .tl .n { font-size: 28px; font-weight: 700; color: var(--purple-t); line-height: 1.2; }
  .s5 .tl .hd2 { margin-top: 52px; }
  .s5 .tl .txt { margin-top: 20px; }
  .s5 .t1 { top: 425px; inset-inline-start: 42px; } .s5 .t2 { top: 425px; inset-inline-start: 282px; } .s5 .t3 { top: 425px; inset-inline-start: 528px; }
  .s5 .bx { width: 200px; }
  .s5 .bx .txt { margin-top: 22px; }
  .s5 .b1 { top: 160px; inset-inline-start: 653px; } .s5 .b2 { top: 412px; inset-inline-start: 965px; }
  /* ---- 6 آلية العمل ---- */
  .s6 .stmt { top: 150px; inset-inline-start: 708px; width: 440px; color: #FFFFFF; }
  .s6 .txt.w { top: 238px; inset-inline-start: 708px; }
  .s6 .col { width: 262px; height: 243px; border-inline-start: 1px solid var(--accent); border-bottom: 1px solid var(--accent); }
  .s6 .col .bd { top: -78px; inset-inline-start: -1px; width: 80px; height: 78px; background: var(--accent); color: #FFFFFF; font-size: 30px; font-weight: 700; display: grid; place-items: center; clip-path: polygon(0 0, 100% 0, 100% 72%, 76% 100%, 0 100%); }
  html[dir="rtl"] .s6 .col .bd { clip-path: polygon(0 0, 100% 0, 100% 100%, 24% 100%, 0 72%); }
  .s6 .col .hd2 { top: 32px; inset-inline-start: 20px; width: 130px; }
  .s6 .col .txt { top: 98px; inset-inline-start: 20px; width: 240px; }
  .s6 .k1 { top: 425px; inset-inline-start: 40px; } .s6 .k2 { top: 425px; inset-inline-start: 335px; } .s6 .k3 { top: 425px; inset-inline-start: 623px; } .s6 .k4 { top: 425px; inset-inline-start: 915px; }
  /* ---- 7 فرصة السوق ---- */
  .s7 .mc { width: 236px; height: 418px; }
  .s7 .mc .num { top: 50px; inset-inline-start: 22px; }
  .s7 .mc .sm { top: 118px; inset-inline-start: 22px; font-size: 17px; color: var(--ink-2); }
  .s7 .mc .hd { top: 175px; inset-inline-start: 22px; color: var(--ink); width: 190px; }
  .s7 .mc .txt { top: 268px; inset-inline-start: 22px; width: 195px; }
  .s7 .mc .ft { bottom: 0; inset-inline-start: 0; width: 148px; height: 34px; clip-path: polygon(0 0, 88% 0, 100% 40%, 100% 100%, 0 100%); }
  .s7 .m1 { top: 200px; inset-inline-start: 36px; } .s7 .m1 .num { color: var(--accent); } .s7 .m1 .ft { background: var(--accent); }
  .s7 .m2 { top: 200px; inset-inline-start: 303px; } .s7 .m2 .num { color: var(--blue); } .s7 .m2 .ft { background: var(--blue); }
  .s7 .m3 { top: 200px; inset-inline-start: 570px; } .s7 .m3 .num { color: var(--pink); } .s7 .m3 .ft { background: var(--pink); }
  .s7 .stmt { top: 208px; inset-inline-start: 838px; width: 400px; }
  .s7 .txt { top: 330px; inset-inline-start: 838px; width: 300px; }
  .chart { position: absolute; }
  .chart text { font-size: 15px; fill: #595959; }
  .s7 .chart { top: 410px; inset-inline-start: 830px; width: 420px; height: 250px; }
  /* ---- 8 الزخم ---- */
  .s8 .tc { width: 240px; }
  .s8 .tc .hd2 { color: var(--ink); margin-top: 24px; }
  .s8 .tc .txt { margin-top: 34px; width: 200px; }
  .s8 .tc .num { font-size: 42px; }
  .s8 .x1 { top: 282px; inset-inline-start: 64px; } .s8 .x1 .num { color: #9C64E3; }
  .s8 .x2 { top: 348px; inset-inline-start: 365px; } .s8 .x2 .num { color: var(--blue); }
  .s8 .x3 { top: 282px; inset-inline-start: 683px; } .s8 .x3 .num { color: var(--pink); }
  .s8 .x4 { top: 282px; inset-inline-start: 955px; } .s8 .x4 .num { color: var(--cyan); }
  /* ---- 9 نموذج العمل ---- */
  .s9 .stmt { top: 225px; inset-inline-start: 36px; width: 380px; }
  .s9 .txt { top: 345px; inset-inline-start: 36px; width: 320px; }
  .s9 .bul { display: flex; gap: 16px; align-items: flex-start; font-size: 15px; line-height: 1.35; color: var(--ink-2); width: 300px; }
  .s9 .bul svg { width: 18px; height: 18px; flex: none; color: var(--ink); margin-top: 1px; }
  .s9 .u1 { top: 455px; inset-inline-start: 38px; } .s9 .u2 { top: 543px; inset-inline-start: 38px; }
  .s9 .dc { width: 188px; height: 217px; color: #FFFFFF; }
  .s9 .dc h2 { top: 48px; inset-inline-start: 19px; font-size: 20px; font-weight: 700; line-height: 1.25; width: 90px; }
  .s9 .dc p { top: 125px; inset-inline-start: 19px; font-size: 17px; line-height: 1.35; width: 160px; }
  .s9 .d1 { top: 220px; inset-inline-start: 451px; } .s9 .d2 { top: 220px; inset-inline-start: 653px; } .s9 .d3 { top: 220px; inset-inline-start: 855px; } .s9 .d4 { top: 220px; inset-inline-start: 1057px; }
  .s9 .bh { top: 568px; inset-inline-start: 451px; width: 794px; text-align: center; font-size: 19px; font-weight: 700; color: var(--ink); }
  .s9 .bt { top: 602px; inset-inline-start: 451px; width: 794px; text-align: center; font-size: 17px; color: var(--ink-2); }
  /* ---- 10 الميزة التنافسية ---- */
  .s10 .row { inset-inline-start: 0; width: 1280px; height: 99px; }
  .s10 .r1 { top: 255px; background: #F5F5F5; } .s10 .r2 { top: 354px; background: #F1F1F1; } .s10 .r3 { top: 453px; background: #F5F5F5; } .s10 .r4 { top: 552px; background: #F1F1F1; }
  .s10 .colg { top: 180px; width: 247px; height: 380px; background: linear-gradient(180deg, #E0E0E0 0, rgb(224 224 224 / .55) 55%, rgb(224 224 224 / 0) 100%); clip-path: polygon(0 0, calc(100% - 45px) 0, 100% 45px, 100% 100%, 0 100%); }
  .s10 .ga { inset-inline-start: 505px; } .s10 .gc { inset-inline-start: 995px; width: 250px; }
  .s10 .col1 { top: 180px; inset-inline-start: 307px; width: 198px; height: 470px; background: linear-gradient(175deg, #3A1C66 0, #2A2A6C 45%, #8A4CC9 100%); clip-path: polygon(0 0, calc(100% - 35px) 0, 100% 35px, 100% 100%, 0 100%); }
  .s10 .col1 i { position: absolute; inset-inline-start: 0; width: 100%; height: 99px; background: rgb(255 255 255 / .13); }
  .s10 .col1 i.a { top: 174px; } .s10 .col1 i.b { top: 372px; }
  .s10 .hh { top: 207px; inset-inline-start: 40px; font-size: 20px; font-weight: 700; color: var(--ink); }
  .s10 .ch { top: 207px; width: 198px; text-align: center; font-size: 20px; font-weight: 700; color: var(--purple-t); text-transform: uppercase; white-space: nowrap; }
  .s10 .h1 { inset-inline-start: 307px; color: #FFFFFF; } .s10 .h2 { inset-inline-start: 505px; width: 247px; } .s10 .h3 { inset-inline-start: 752px; width: 243px; } .s10 .h4 { inset-inline-start: 995px; width: 250px; }
  .s10 .rl { inset-inline-start: 40px; width: 240px; font-size: 18px; line-height: 1.3; color: var(--ink); }
  .s10 .ic { width: 32px; height: 32px; border-radius: 50%; display: grid; place-items: center; color: #FFFFFF; }
  .s10 .ic svg { width: 20px; height: 20px; }
  .s10 .ic.c { background: var(--cyan); } .s10 .ic.p { background: var(--accent); } .s10 .ic.g { background: var(--gray); }
  /* ---- 11 الفريق ---- */
  .s11 .tm { width: 401px; height: 203px; background: linear-gradient(90deg, #3A1B5F 0, #312263 20%, #2B2666 30%, #202F66 40%, #1C3168 46%, #2A2F7E 72%, #7234B3 100%); clip-path: polygon(0 0, calc(100% - 28px) 0, 100% 28px, 100% 100%, 0 100%); }
  html[dir="rtl"] .s11 .tm { background: linear-gradient(270deg, #3A1B5F 0, #312263 20%, #2B2666 30%, #202F66 40%, #1C3168 46%, #2A2F7E 72%, #7234B3 100%); clip-path: polygon(28px 0, 100% 0, 100% 100%, 0 100%, 0 28px); }
  .s11 .tm h2 { top: 38px; inset-inline-start: 14px; font-size: 22px; font-weight: 700; color: #FFFFFF; }
  .s11 .tm p { top: 74px; inset-inline-start: 14px; font-size: 16px; color: #FFFFFF; letter-spacing: 1px; text-transform: uppercase; }
  html[lang="ar"] .s11 .tm p { letter-spacing: 0; }
  .s11 .p1 { top: 220px; inset-inline-start: 36px; } .s11 .p2 { top: 220px; inset-inline-start: 441px; } .s11 .p3 { top: 220px; inset-inline-start: 844px; }
  .s11 .ph { top: 165px; height: 258px; object-fit: cover; }
  .s11 .ph1 { inset-inline-start: 190px; width: 247px; } .s11 .ph2 { inset-inline-start: 598px; width: 242px; } .s11 .ph3 { inset-inline-start: 1008px; width: 235px; }
  .s11 .dsc { width: 360px; }
  .s11 .dsc h3 { font-size: 19px; font-weight: 700; line-height: 1.35; color: var(--ink); }
  .s11 .dsc p { font-size: 18px; line-height: 1.4; color: var(--ink-2); margin-top: 32px; width: 300px; }
  .s11 .q1 { top: 463px; inset-inline-start: 50px; } .s11 .q2 { top: 463px; inset-inline-start: 457px; } .s11 .q3 { top: 463px; inset-inline-start: 862px; }
  /* ---- 12 البيانات المالية ---- */
  .s12 .stmt { top: 183px; inset-inline-start: 36px; width: 440px; }
  .s12 .txt { top: 272px; inset-inline-start: 36px; }
  .s12 .chart { top: 340px; inset-inline-start: 36px; width: 640px; height: 310px; }
  .s12 .call { top: 330px; inset-inline-start: 146px; width: 261px; height: 80px; border: 2px solid var(--accent); background: #FFFFFF; display: flex; align-items: center; gap: 22px; padding-inline-start: 16px; }
  .s12 .call b { font-size: 34px; font-weight: 300; color: var(--blue-t); }
  .s12 .call span { font-size: 15px; line-height: 1.3; color: var(--ink-2); width: 110px; }
  .s12 .stmt.w { top: 152px; inset-inline-start: 730px; width: 440px; }
  .s12 .txt.w { top: 242px; inset-inline-start: 730px; }
  .s12 .kp { width: 232px; height: 128px; color: #FFFFFF; border-bottom: 1px solid rgb(255 255 255 / .6); }
  .s12 .kp.nb { border-bottom: 0; }
  .s12 .kp h3 { top: 0; inset-inline-start: 0; font-size: 20px; font-weight: 700; }
  .s12 .kp b { top: 36px; inset-inline-start: 0; font-size: 46px; font-weight: 300; line-height: 1.25; }
  .s12 .kp svg { top: 56px; inset-inline-end: 6px; width: 30px; height: 30px; }
  .s12 .kp .up { color: var(--cyan); } .s12 .kp .dn { color: var(--pink); }
  .s12 .k1 { top: 322px; inset-inline-start: 730px; } .s12 .k2 { top: 322px; inset-inline-start: 1008px; } .s12 .k3 { top: 498px; inset-inline-start: 730px; } .s12 .k4 { top: 498px; inset-inline-start: 1008px; }
  /* ---- 13 الطلب ---- */
  .s13 .card { width: 354px; height: 193px; }
  .s13 .card .badge { top: -30px; inset-inline-start: 12px; width: 70px; height: 66px; }
  .s13 .card .hd2 { top: 50px; inset-inline-start: 64px; width: 140px; font-size: 20px; }
  .s13 .card .txt { top: 118px; inset-inline-start: 64px; width: 250px; }
  .s13 .a1 { top: 222px; inset-inline-start: 36px; } .s13 .a2 { top: 222px; inset-inline-start: 424px; } .s13 .a3 { top: 470px; inset-inline-start: 36px; } .s13 .a4 { top: 470px; inset-inline-start: 424px; }
  .s13 .stmt { top: 262px; inset-inline-start: 846px; width: 380px; color: #FFFFFF; }
  .s13 .txt.w { top: 384px; inset-inline-start: 846px; width: 330px; }
  .s13 .kh { top: 494px; inset-inline-start: 848px; font-size: 20px; font-weight: 700; color: var(--cyan); }
  .s13 .kv { top: 540px; inset-inline-start: 848px; font-size: 44px; font-weight: 300; color: #FFFFFF; line-height: 1.2; }
  /* ---- 14 استخدام التمويل ---- */
  .s14 .stmt { top: 225px; inset-inline-start: 36px; width: 380px; }
  .s14 .txt { top: 345px; inset-inline-start: 36px; }
  .s14 .chart { top: 80px; inset-inline-start: 500px; width: 730px; height: 340px; }
  .s14 .dl { width: 160px; text-align: center; }
  .s14 .dl b { display: block; font-size: 44px; font-weight: 300; color: var(--blue-t); line-height: 1.2; }
  .s14 .dl span { display: block; font-size: 19px; font-weight: 700; color: var(--ink); line-height: 1.3; margin-top: 14px; }
  .s14 .l1 { top: 182px; inset-inline-start: 585px; } .s14 .l2 { top: 182px; inset-inline-start: 978px; }
  .s14 .ds { width: 300px; text-align: center; font-size: 17px; line-height: 1.35; color: var(--ink-2); }
  .s14 .e1 { top: 436px; inset-inline-start: 515px; } .s14 .e2 { top: 436px; inset-inline-start: 906px; }
  .s14 .div { top: 85px; inset-inline-start: 865px; width: 1px; height: 428px; background: #C9B7E4; }
  .s14 .band { top: 512px; inset-inline-start: 0; width: 1280px; height: 208px; background: linear-gradient(90deg, #391C5F, #1F3368 50%, #4C3297); }
  html[dir="rtl"] .s14 .band { background: linear-gradient(270deg, #391C5F, #1F3368 50%, #4C3297); }
  .s14 .fi { display: flex; gap: 28px; align-items: flex-start; width: 360px; color: #FFFFFF; }
  .s14 .fi svg { width: 50px; height: 50px; flex: none; color: var(--accent); }
  .s14 .fi h3 { font-size: 19px; font-weight: 700; line-height: 1.3; text-transform: uppercase; }
  .s14 .fi p { font-size: 16px; line-height: 1.4; margin-top: 18px; }
  .s14 .f1 { top: 545px; inset-inline-start: 38px; } .s14 .f2 { top: 545px; inset-inline-start: 452px; } .s14 .f3 { top: 545px; inset-inline-start: 855px; }
  /* ---- 15 أسئلة وأجوبة ---- */
  .s15 .lg { top: 95px; inset-inline-start: 70px; }
  .s15 .qa { top: 262px; inset-inline-start: 76px; font-size: 130px; font-weight: 800; color: #FFFFFF; line-height: 1; letter-spacing: -2px; }
  html[lang="ar"] .s15 .qa { font-size: 82px; top: 290px; letter-spacing: 0; }
  .s15 .dq { top: 462px; inset-inline-start: 82px; font-size: 30px; font-weight: 700; color: #FFFFFF; line-height: 1.2; }
  .s15 .txt { top: 535px; inset-inline-start: 82px; font-size: 19px; color: #FFFFFF; width: 460px; }
  .s15 .qi { width: 300px; color: #FFFFFF; }
  .s15 .qi .n { inset-inline-start: 0; top: -6px; font-size: 30px; font-weight: 300; }
  .s15 .qi h3 { inset-inline-start: 80px; top: 0; font-size: 19px; font-weight: 700; }
  .s15 .qi p { inset-inline-start: 80px; top: 38px; font-size: 17px; line-height: 1.4; width: 260px; }
  .s15 .qi i { inset-inline-start: 80px; top: 128px; width: 137px; height: 2px; background: var(--cyan); }
  .s15 .i1 { top: 190px; inset-inline-start: 780px; } .s15 .i2 { top: 371px; inset-inline-start: 780px; } .s15 .i3 { top: 552px; inset-inline-start: 780px; }
  /* ---- 16 شكرًا ---- */
  .s16 .co { top: 148px; }
  .s16 .ty { top: 250px; inset-inline-start: 36px; font-size: 58px; font-weight: 700; color: #FFFFFF; line-height: 1.3; }
  .s16 .txt { top: 437px; inset-inline-start: 48px; font-size: 19px; color: #FFFFFF; width: 460px; }
  .s16 .lg { top: 580px; inset-inline-start: 56px; }
  .s16 .ci { width: 360px; color: #FFFFFF; height: 100px; border-bottom: 1px solid #FFFFFF; }
  .s16 .ci.nb { border-bottom: 0; }
  .s16 .ci svg { inset-inline-start: 0; top: 0; width: 40px; height: 40px; color: var(--accent); }
  .s16 .ci h3 { inset-inline-start: 90px; top: 4px; font-size: 19px; font-weight: 700; text-transform: uppercase; }
  .s16 .ci p { inset-inline-start: 90px; top: 38px; font-size: 17px; white-space: nowrap; }
  .s16 .c1 { top: 296px; inset-inline-start: 712px; } .s16 .c2 { top: 441px; inset-inline-start: 712px; } .s16 .c3 { top: 586px; inset-inline-start: 712px; }"""

# ---------- الشرائح ----------


def logo(cls=''):
    return '<div class="lg%s">%s<p>§LOGO§</p></div>' % ((' ' + cls) if cls else '', LOGO)


def s1():
    d = bgrect('bg1') + ''.join([
        poly(cham(460, 0, 190, 165, 70), 'wh'),
        poly(cham(640, 165, 220, 165, 70), 'pk'),
        poly(cham(955, 75, 235, 170, 75), 'pk'),
        poly(cham(1135, 275, 95, 160, 45), 'cy'),
        poly(cham(890, 435, 165, 165, 70), 'cy'),
        poly(cham(1050, 500, 175, 175, 70), 'bl'),
        poly(cham(600, 430, 230, 170, 70), 'nv'),
        poly(cham(860, 110, 365, 410, 175), 'ln'),
        poly(cham(680, 335, 365, 420, 180, '/'), 'ln'),
        poly(cham(0, 580, 140, 140, 60), 'pf'),
        poly(cham(140, 560, 85, 85, 40, '/'), 'cy'),
        poly(cham(0, 550, 240, 300, 165), 'ln'),
    ])
    return deco(d) + logo() + '<h1 class="ai">§AI§</h1><h2 class="bp">§BP§</h2><p class="pt">§PT§</p>'


def s2():
    d = bgrect('bg2') + ''.join([
        poly(cham(240, 355, 80, 120, 35), 'bl'),
        poly(cham(170, 410, 85, 130, 35), 'cy'),
        poly(cham(-30, 485, 120, 200, 50), 'pk'),
        poly(cham(215, 640, 80, 100, 35), 'pk'),
        poly(cham(320, 500, 80, 110, 35), 'bl'),
        poly(cham(48, 315, 210, 245, 100), 'ln'),
        poly(cham(155, 460, 210, 245, 100), 'ln'),
        corner_pair(340),
        '<polyline class="ln" points="800,720 800,585 885,500 1280,500"/>',
    ])
    items = ''.join('<div class="it it%d"><div class="n">0%d</div><h2>§HP§</h2></div>' % (i, i) for i in range(1, 6))
    return deco(d) + '<p class="co">§CO§</p><h1 class="ttl">§TOC§</h1>' + items + '<p class="box">§STMT§</p>'


def s3():
    d = bgrect('bg3') + cluster_top() + corner_pair(505)
    return (deco(d) + '<div class="big">01</div><h1 class="tp">§TOPIC§</h1><p class="stmt">§STMT§</p><p class="txt">§TXT§</p>' + logo())


def card4(i, x):
    pts = [(x + 36, 380), (x + 384, 380), (x + 384, 558), (x + 324, 618), (x, 618), (x, 416)]
    return pts


def s4():
    panel = [(765, 0), (1280, 0), (1280, 345), (1245, 380), (875, 380), (875, 445), (830, 445), (830, 380), (600, 380), (600, 165)]
    d = poly(panel, 'pnl') + ''.join(poly(card4(i, x), 'pbw') for i, x in ((1, 36), (2, 442), (3, 852)))
    cards = ''.join('<div class="card c%d"><div class="badge">0%d</div><h2 class="hd">§HP§</h2><p class="txt">§TXT§</p></div>' % (i, i) for i in (1, 2, 3))
    stats = ('<div class="st st1"><div class="v">$45.3M</div><div class="bar"></div><p class="txt">§TXT§</p></div>'
             '<div class="st st2"><div class="v">74%</div><div class="bar"></div><p class="txt">§TXT§</p></div>')
    return deco(d) + '<p class="co">§CO§</p><h1 class="ttl">§T4§</h1><p class="stmt">§STMT§</p><p class="txt">§TXT§</p>' + stats + cards


def s5():
    d = ''.join([
        '<circle cx="912" cy="346" r="133" fill="none" stroke="#7532BF" stroke-width="84"/>',
        poly([(692, 88), (907, 88), (907, 285), (833, 358), (617, 358), (617, 162)], 'pbw'),
        poly([(918, 360), (1133, 360), (1208, 435), (1208, 632), (993, 632), (918, 557)], 'pbw'),
        '<line x1="0" y1="480" x2="905" y2="480" stroke="#7532BF" stroke-width="2"/>',
    ])
    tl = ''.join('<div class="tl t%d"><div class="n">0%d</div><h3 class="hd2">§HP§</h3><p class="txt">§TXT§</p></div>' % (i, i) for i in (1, 2, 3))
    bx = ''.join('<div class="bx b%d"><h2 class="hd">§HP§</h2><p class="txt">§TXT§</p></div>' % i for i in (1, 2))
    return deco(d) + '<p class="co">§CO§</p><h1 class="ttl">§T5§</h1><p class="stmt">§STMT§</p><p class="txt">§TXT§</p>' + tl + bx


def s6():
    d = poly([(770, 0), (1280, 0), (1280, 240), (1140, 380), (640, 380), (640, 130)], 'pnl')
    cols = ''.join('<div class="col k%d"><div class="bd">0%d</div><h3 class="hd2">§HP§</h3><p class="txt">§TXT§</p></div>' % (i, i) for i in (1, 2, 3, 4))
    return (deco(d) + '<p class="co">§CO§</p><h1 class="ttl">§T6§</h1><p class="stmt w">§STMT§</p><p class="txt w">§TXT§</p>' + cols)


def s7():
    def outline(x, cls):
        return poly([(x, 200), (x + 194, 200), (x + 236, 242), (x + 236, 618), (x, 618)], cls)
    d = ('<g class="m1">%s</g><g class="m2">%s</g><g class="m3">%s</g>' % (outline(36, 'pb'), outline(303, 'pb'), outline(570, 'pb')))
    d = d.replace('<g class="m2"><polygon class="pb"', '<g class="m2"><polygon class="pb" style="stroke:#3D68C7"').replace('<g class="m3"><polygon class="pb"', '<g class="m3"><polygon class="pb" style="stroke:#F3698D"')
    cards = ''.join('<div class="mc m%d"><div class="num">%s</div><p class="sm">§SM§</p><h2 class="hd">§TP§</h2><p class="txt">§STM§</p><div class="ft"></div></div>' % (i, v) for i, v in ((1, '30.5K'), (2, '3.5B$'), (3, '87%')))
    return (deco(d) + '<p class="co">§CO§</p><h1 class="ttl">§T7§</h1>' + cards + '<p class="stmt">§STMT§</p><p class="txt">§TXT§</p><svg class="chart" id="c7" viewBox="0 0 420 250" aria-hidden="true"></svg>')


def s8():
    d = ''.join([
        poly([(36, 188), (245, 188), (330, 272), (330, 628), (36, 628)], 'pf') .replace('class="pf"', 'fill="url(#lav)"'),
        poly([(710, 220), (920, 220), (920, 575), (625, 575), (625, 302)], 'pf').replace('class="pf"', 'fill="url(#rose)"'),
        '<polyline class="pb" points="36,720 36,188 245,188 330,272 330,628 545,628 625,548 625,302 710,220 920,220 920,575 1135,575 1215,495 1215,0"/>',
    ])
    cols = ''.join('<div class="tc x%d"><div class="num">%s</div><h3 class="hd2">§TP§</h3><p class="txt">§STM§</p></div>' % (i, v) for i, v in ((1, '30.5K'), (2, '3.5B$'), (3, '87%'), (4, '3.5B$')))
    return deco(d) + '<p class="co">§CO§</p><h1 class="ttl">§T8§</h1>' + cols


def hexpt(cx, cy, r):
    import math
    return [(cx + r * math.cos(math.radians(a)), cy + r * math.sin(math.radians(a))) for a in range(0, 360, 60)]


def s9():
    d = CLUSTER_TR
    for x in (451, 653, 855, 1057):
        cx = x + 96
        d += '<rect x="%d" y="220" width="188" height="217" fill="url(#dk)"/>' % x
        d += '<line x1="%d" y1="455" x2="%d" y2="510" stroke="#7532BF" stroke-width="1.5"/>' % (cx, cx)
        for cy in (437, 527):
            d += poly(hexpt(cx, cy, 20), 'pbw') + poly(hexpt(cx, cy, 10), 'pf')
    d = '<rect x="451" y="527" width="794" height="133" fill="url(#lav)"/>' + d
    cards = ''.join('<div class="dc d%d"><h2>§HH§</h2><p>§TXS§</p></div>' % i for i in (1, 2, 3, 4))
    buls = ''.join('<div class="bul u%d">%s<p>§TXT§</p></div>' % (i, ARROW_C) for i in (1, 2))
    return (deco(d) + '<p class="co">§CO§</p><h1 class="ttl">§T9§</h1><p class="stmt">§STMT§</p><p class="txt">§TXT§</p>' + buls + cards
            + '<p class="bh">§STH§</p><p class="bt">§STM§</p>')


def s10():
    rows = ''.join('<div class="row r%d"></div>' % i for i in (1, 2, 3, 4))
    cols = '<div class="colg ga mir"></div><div class="colg gc mir"></div><div class="col1 mir"><i class="a"></i><i class="b"></i></div>'
    heads = ('<p class="hh">§HH§</p><p class="ch h1">§YB§</p><p class="ch h2">§CA§</p><p class="ch h3">§CB§</p><p class="ch h4">§CC§</p>')
    data = [['c', 'p', 'g', 'g'], ['c', 'p', 'g', 'p'], ['c', 'g', 'p', 'p'], ['c', 'g', 'p', 'g']]
    xs = [406, 628, 873, 1120]
    body = ''
    for r, row in enumerate(data):
        y = 255 + r * 99
        body += '<p class="rl" style="top:%dpx">§RS§</p>' % (y + 32)
        for k, kind in enumerate(row):
            body += '<div class="ic %s" style="top:%dpx;inset-inline-start:%dpx">%s</div>' % (kind, y + 34, xs[k] - 16, STAR if kind != 'g' else DASH)
    return rows + cols + '<p class="co">§CO§</p><h1 class="ttl">§T10§</h1>' + heads + body


def s11():
    pan = ''.join('<div class="tm p%d"><h2>§NAME§</h2><p>§ROLE§</p></div>' % i for i in (1, 2, 3))
    imgs = ''.join('<img class="ph ph%d mir" alt="§ALT%d§" src="/templates/freeform/sl-ai-business-plan/assets/team-%d.jpg">' % (i, i, i) for i in (1, 2, 3))
    dsc = ''.join('<div class="dsc q%d"><h3>§STS§</h3><p>§TXH§</p></div>' % i for i in (1, 2, 3))
    return deco(CLUSTER_TR) + '<p class="co">§CO§</p><h1 class="ttl">§T11§</h1>' + pan + imgs + dsc


def s12():
    d = poly([(810, 0), (1280, 0), (1280, 580), (1140, 720), (680, 720), (680, 130)], 'pnl')
    kps = ''.join('<div class="kp k%d%s"><h3>§HH§</h3><b>%s</b>%s</div>' % (i, ' nb' if i > 2 else '', v, ic)
                  for i, v, ic in ((1, '$3.5B', UP.replace('<svg', '<svg class="up"')), (2, '$3.2M', UP.replace('<svg', '<svg class="up"')),
                                   (3, '$1.1M', DOWN.replace('<svg', '<svg class="dn"')), (4, '$920k', UP.replace('<svg', '<svg class="up"'))))
    return (deco(d) + '<p class="co">§CO§</p><h1 class="ttl">§T12§</h1><p class="stmt">§STS§</p><p class="txt">§TXT§</p>'
            '<svg class="chart" id="c12" viewBox="0 0 640 310" aria-hidden="true"></svg><div class="call"><b>8.6%</b><span>§SMD§</span></div>'
            '<p class="stmt w">§STMT§</p><p class="txt w">§TXT§</p>' + kps)


def s13():
    d = poly([(840, 0), (1280, 0), (1280, 720), (725, 720), (725, 118)], 'pnl2') + cluster_top(740, -115, (780, 115, 105, 100, 40))
    for x, y in ((36, 222), (424, 222), (36, 470), (424, 470)):
        d += poly([(x + 24, y), (x + 354, y), (x + 354, y + 138), (x + 299, y + 193), (x, y + 193), (x, y + 24)], 'pbw')
    cards = ''.join('<div class="card a%d"><div class="badge">0%d</div><h3 class="hd2">§HP§</h3><p class="txt">§TXT§</p></div>' % (i, i) for i in (1, 2, 3, 4))
    return (deco(d) + '<p class="co">§CO§</p><h1 class="ttl">§T13§</h1>' + cards + '<p class="stmt">§STMT§</p><p class="txt w">§TXT§</p>'
            '<p class="kh">§HH§</p><p class="kv">$3.5B</p>')


def s14():
    fi = ''.join('<div class="fi f%d">%s<div><h3>§HPU§</h3><p>§STM2§</p></div></div>' % (i, CURSOR) for i in (1, 2, 3))
    return ('<p class="co">§CO§</p><h1 class="ttl">§T14§</h1><p class="stmt">§STMT§</p><p class="txt">§TXT§</p>'
            '<svg class="chart" id="c14" viewBox="0 0 730 340" aria-hidden="true"></svg>'
            '<div class="dl l1"><b>35%</b><span>§HP§</span></div><div class="dl l2"><b>65%</b><span>§HP§</span></div>'
            '<p class="ds e1">§STM§</p><p class="ds e2">§STM§</p><div class="div"></div><div class="band"></div>' + fi)


def s15():
    d = bgrect('bg15') + cluster_top(390, -95, (330, 165, 105, 100, 40)) + corner_pair(500)
    items = ''.join('<div class="qi i%d"><div class="n">0%d</div><h3>§HP§</h3><p>§TXT§</p>%s</div>' % (i, i, '<i></i>' if i < 3 else '') for i in (1, 2, 3))
    return deco(d) + logo() + '<h1 class="qa">§QA§</h1><h2 class="dq">§DQ§</h2><p class="txt">§TXQ§</p>' + items


def s16():
    d = bgrect('bg16') + cluster_top(760, -130, (800, 170, 105, 100, 40)) + corner_pair(480)
    ci = ''.join('<div class="ci c%d%s">%s<h3>%s</h3><p>§TXD§</p></div>' % (i, ' nb' if i == 3 else '', ic, lab)
                 for i, ic, lab in ((1, MAIL, '§EMAIL§'), (2, GLOBE, '§WEB§'), (3, PHONE, '§CONTACT§')))
    return deco(d) + '<p class="co">§CO§</p><h1 class="ty">§TY§</h1><p class="txt">§TXQ§</p>' + logo('wl') + ci


SLIDES = [s1(), s2(), s3(), s4(), s5(), s6(), s7(), s8(), s9(), s10(), s11(), s12(), s13(), s14(), s15(), s16()]
# الشرائح الداكنة تحمل الصنف dark للون نص الشركة
DARK = {1, 2, 3, 15, 16}
SLIDES = [('<span class="dark" hidden></span>' + h) if (i + 1) in DARK else h for i, h in enumerate(SLIDES)]
# ملاحظة: الصنف dark يُطبَّق عبر .sN مباشرة في CSS أدناه بدل الحاوية
STYLE += '\n  .s2 .co, .s16 .co { color: #FFFFFF; }'

# ---------- الرسوم البيانية ----------
JSON_BLOCKS = {
    'chart-data': {
        'ar': {'labels': ['البند 1', 'البند 2', 'البند 3', 'البند 4'], 'max': 6, 'step': 2,
               'series': [{'name': 'السلسلة 1', 'color': '#7532BF', 'values': [4.3, 2.5, 3.5, 4.5]},
                          {'name': 'السلسلة 2', 'color': '#3D68C7', 'values': [2.3, 4.4, 1.8, 2.8]},
                          {'name': 'السلسلة 3', 'color': '#F3698D', 'values': [2.0, 2.0, 3.0, 5.0]}]},
        'en': {'labels': ['Item 1', 'Item 2', 'Item 3', 'Item 4'], 'max': 6, 'step': 2,
               'series': [{'name': 'Series 1', 'color': '#7532BF', 'values': [4.3, 2.5, 3.5, 4.5]},
                          {'name': 'Series 2', 'color': '#3D68C7', 'values': [2.3, 4.4, 1.8, 2.8]},
                          {'name': 'Series 3', 'color': '#F3698D', 'values': [2.0, 2.0, 3.0, 5.0]}]},
    },
    'chart-data-2': {
        'ar': {'labels': ['الشهر 1', 'الشهر 2', 'الشهر 3', 'الشهر 4'], 'max': 16, 'step': 4,
               'series': [{'name': 'السلسلة 1', 'color': '#7532BF', 'values': [4.3, 2.4, 3.4, 4.4]},
                          {'name': 'السلسلة 2', 'color': '#F3698D', 'values': [2.3, 4.4, 1.9, 2.9]},
                          {'name': 'السلسلة 3', 'color': '#69E2DB', 'values': [2.0, 2.0, 2.8, 5.0]}]},
        'en': {'labels': ['M1', 'M2', 'M3', 'M4'], 'max': 16, 'step': 4,
               'series': [{'name': 'Series 1', 'color': '#7532BF', 'values': [4.3, 2.4, 3.4, 4.4]},
                          {'name': 'Series 2', 'color': '#F3698D', 'values': [2.3, 4.4, 1.9, 2.9]},
                          {'name': 'Series 3', 'color': '#69E2DB', 'values': [2.0, 2.0, 2.8, 5.0]}]},
    },
    'chart-data-3': {'values': [35, 65], 'color': '#7532BF', 'rest': '#AB7FE1'},
}

EXTRA_SCRIPT = r"""  function J(id) { return JSON.parse(document.getElementById(id).textContent); }
  function fx(x, W) { return RTL ? W - x : x; }  // انعكاس أفقي للعربية
  // أعمدة مجمّعة (الشريحة 7) ومكدّسة (الشريحة 12) من كتل البيانات
  function bars(id, dataId, W, H, px0, px1, y0, y1, bw, gap, stacked) {
    var d = J(dataId), svg = document.getElementById(id), s = '', n = d.labels.length, i, k;
    var unit = (y0 - y1) / d.max, step = (px1 - px0) / n;
    for (var v = 0; v <= d.max; v += d.step) {
      var y = y0 - v * unit;
      s += '<line x1="' + fx(px0, W) + '" x2="' + fx(px1, W) + '" y1="' + y + '" y2="' + y + '" stroke="#D9D9D9" stroke-width="1"/>';
      s += '<text x="' + fx(px0 - 12, W) + '" y="' + (y + 5) + '" text-anchor="' + (RTL ? 'start' : 'end') + '">' + v + '</text>';
    }
    for (i = 0; i < n; i++) {
      var cx = fx(px0 + step * (i + .5), W);
      if (stacked) {
        var acc = 0;
        for (k = 0; k < d.series.length; k++) {
          var val = d.series[k].values[i], top = y0 - (acc + val) * unit;
          s += '<rect x="' + (cx - bw / 2) + '" y="' + top + '" width="' + bw + '" height="' + (val * unit) + '" fill="' + d.series[k].color + '"/>';
          acc += val;
        }
      } else {
        var m = d.series.length, total = m * bw + (m - 1) * gap;
        for (k = 0; k < m; k++) {
          var kk = RTL ? m - 1 - k : k, vv = d.series[kk].values[i];
          var x = cx - total / 2 + k * (bw + gap);
          s += '<rect x="' + x + '" y="' + (y0 - vv * unit) + '" width="' + bw + '" height="' + (vv * unit) + '" fill="' + d.series[kk].color + '"/>';
        }
      }
      s += '<text x="' + cx + '" y="' + (y0 + 25) + '" text-anchor="middle">' + d.labels[i] + '</text>';
    }
    svg.innerHTML = s;
  }
  bars('c7', 'chart-data', 420, 250, 40, 415, 203, 15, 18, 6, false);
  bars('c12', 'chart-data-2', 640, 310, 40, 630, 280, 13, 76, 0, true);
  // دائرتان مفرغتان (الشريحة 14): القوس يبدأ من الأعلى باتجاه القراءة
  function arc(cx, cy, r, a0, a1) {
    var r0 = a0 * Math.PI / 180, r1 = a1 * Math.PI / 180, big = Math.abs(a1 - a0) > 180 ? 1 : 0, sw = a1 > a0 ? 1 : 0;
    return 'M' + (cx + r * Math.cos(r0)) + ',' + (cy + r * Math.sin(r0)) + ' A' + r + ',' + r + ' 0 ' + big + ' ' + sw + ' ' + (cx + r * Math.cos(r1)) + ',' + (cy + r * Math.sin(r1));
  }
  (function () {
    var d = J('chart-data-3'), svg = document.getElementById('c14'), s = '', W = 730, cs = [165, 558];
    d.values.forEach(function (v, i) {
      var cx = fx(cs[i], W), cy = 165, r = 137.5, sweep = v / 100 * 360 * (RTL ? -1 : 1);
      s += '<circle cx="' + cx + '" cy="' + cy + '" r="' + r + '" fill="none" stroke="' + d.rest + '" stroke-width="41"/>';
      s += '<path d="' + arc(cx, cy, r, -90, -90 + (Math.abs(sweep) >= 360 ? sweep * .9999 : sweep)) + '" fill="none" stroke="' + d.color + '" stroke-width="41"/>';
    });
    svg.innerHTML = s;
  })();"""

# ---------- النصوص ----------
_ar_common = {
    'CO': 'اسم شركتك هنا', 'LOGO': 'شعار شركتك<br>هنا',
    'HP': 'عنوان نائب', 'HH': 'العنوان هنا', 'TP': 'موضوع نائب', 'HPU': 'عنوان<br>نائب',
    'STMT': 'أدرج عبارتك الرئيسية هنا. عبارة موجزة عن موضوعك.',
    'STS': 'أدرج عبارتك الرئيسية هنا. هذا نص توضيحي.',
    'STH': 'أدرج عبارتك الرئيسية هنا.',
    'TXT': 'أدرج النص المطلوب هنا. هذا نص توضيحي.',
    'TXS': 'أدرج النص المطلوب هنا.', 'TXD': 'أدرج النص المطلوب هنا.',
    'TXH': 'أدرج النص المطلوب هنا. هذا نص توضيحي هنا.',
    'TXQ': 'أدرج النص المطلوب هنا. هذا نص توضيحي. عبارة موجزة عن موضوعك.',
    'STM': 'أدرج العبارة المطلوبة هنا. هذا نص توضيحي.',
    'STM2': 'أدرج العبارة المطلوبة هنا.<br>أدرج النص المطلوب هنا.',
    'RS': 'أدرج العبارة المطلوبة هنا.',
    'SM': 'أدرج النص المطلوب هنا', 'SMD': 'أدرج النص المطلوب هنا.',
}
_en_common = {
    'CO': 'Your Company Name Here', 'LOGO': 'Your Company<br>Logo Here',
    'HP': 'Heading Placeholder', 'HH': 'Heading Here', 'TP': 'Topic Placeholder', 'HPU': 'Heading<br>Placeholder',
    'STMT': 'Insert your heading statement here. A brief statement about your topic.',
    'STS': 'Insert your heading statement here. This is a sample text.',
    'STH': 'Insert your heading statement here.',
    'TXT': 'Insert your desired text here. This is a sample text.',
    'TXS': 'Insert your desired text here.', 'TXD': 'Insert your desired text here.',
    'TXH': 'Insert your desired text here. This is a sample text here.',
    'TXQ': 'Insert your desired text here. This is a sample text. A brief statement about your topic.',
    'STM': 'Insert your desired statement here. This is a sample text.',
    'STM2': 'Insert your desired statement here.<br>Insert your desired text here.',
    'RS': 'Insert your desired statement here.',
    'SM': 'Insert desired text here', 'SMD': 'Insert desired text here.',
}
STR = {
    'ar': dict(_ar_common, **{
        'AI': 'الذكاء الاصطناعي', 'BP': 'خطة العمل', 'PT': 'قالب عرض تقديمي',
        'TOC': 'جدول<br>المحتويات', 'TOPIC': 'عنوان الموضوع<br>النائب',
        'T4': '<span>شريحة</span> المشكلة', 'T5': '<span>شريحة</span> الحل', 'T6': '<span>شريحة</span> آلية العمل',
        'T7': '<span>شريحة</span> فرصة السوق', 'T8': '<span>شريحة</span> الزخم', 'T9': '<span>شريحة</span> نموذج العمل',
        'T10': '<span>شريحة</span> الميزة التنافسية', 'T11': '<span>شريحة</span> الفريق', 'T12': '<span>شريحة</span> البيانات المالية',
        'T13': '<span>شريحة</span> الطلب', 'T14': '<span>شريحة</span> استخدام التمويل',
        'YB': 'شركتك', 'CA': 'الشركة أ', 'CB': 'الشركة ب', 'CC': 'الشركة ج',
        'NAME': 'الاسم هنا', 'ROLE': 'المنصب هنا',
        'ALT1': 'صورة عضو الفريق الأول', 'ALT2': 'صورة عضو الفريق الثاني', 'ALT3': 'صورة عضو الفريق الثالث',
        'QA': 'أسئلة وأجوبة', 'DQ': 'هل لديكم أي أسئلة؟',
        'TY': 'شكرًا لكم<br>تواصلوا معنا', 'EMAIL': 'البريد الإلكتروني', 'WEB': 'الموقع الإلكتروني', 'CONTACT': 'التواصل',
    }),
    'en': dict(_en_common, **{
        'AI': 'AI', 'BP': 'BUSINESS PLAN', 'PT': 'PRESENTATION TEMPLATE',
        'TOC': 'TABLE OF<br>CONTENTS', 'TOPIC': 'TOPIC TITLE<br>PLACEHOLER',
        'T4': 'THE PROBLEM <span>SLIDE</span>', 'T5': 'THE SOLUTION <span>SLIDE</span>', 'T6': 'HOW IT WORKS <span>SLIDE</span>',
        'T7': 'MARKET OPPORTUNITY <span>SLIDE</span>', 'T8': 'TRACTION <span>SLIDE</span>', 'T9': 'BUSINESS MODEL <span>SLIDE</span>',
        'T10': 'COMPETITIVE EDGE <span>SLIDE</span>', 'T11': 'THE TEAM <span>SLIDE</span>', 'T12': 'FINANCIALS <span>SLIDE</span>',
        'T13': 'THE ASK <span>SLIDE</span>', 'T14': 'USE OF FUNDS <span>SLIDE</span>',
        'YB': 'Your Business', 'CA': 'Company A', 'CB': 'Company B', 'CC': 'Company C',
        'NAME': 'NAME HERE', 'ROLE': 'Role Here',
        'ALT1': 'Photo of the first team member', 'ALT2': 'Photo of the second team member', 'ALT3': 'Photo of the third team member',
        'QA': 'Q&amp;A', 'DQ': 'DO YOU HAVE ANY QUESTIONS?',
        'TY': 'Thank You<br>Connect With Us', 'EMAIL': 'Email', 'WEB': 'Website', 'CONTACT': 'Contact',
    }),
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'mgmt', 'icon': '🤖',
    'name': 'خطة عمل بالذكاء الاصطناعي', 'nameEn': 'AI Business Plan',
    'description': '16 شريحة لخطة عمل: غلاف وفهرس، المشكلة والحل، السوق والزخم ونموذج العمل، الفريق والمالية والطلب والختام',
    'descriptionEn': '16 business-plan slides: cover, contents, problem, solution, market, traction, model, team, financials, ask, Q&A',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'جدول المحتويات', 'titleEn': 'Table of Contents'},
        {'title': 'فاصل الموضوع 01', 'titleEn': 'Topic Divider 01'},
        {'title': 'المشكلة (أرقام)', 'titleEn': 'The Problem (figures)', 'needsData': True},
        {'title': 'الحل', 'titleEn': 'The Solution'},
        {'title': 'آلية العمل', 'titleEn': 'How It Works'},
        {'title': 'فرصة السوق (رسم)', 'titleEn': 'Market Opportunity (chart)', 'needsData': True},
        {'title': 'الزخم (أرقام)', 'titleEn': 'Traction (figures)', 'needsData': True},
        {'title': 'نموذج العمل', 'titleEn': 'Business Model'},
        {'title': 'الميزة التنافسية', 'titleEn': 'Competitive Edge'},
        {'title': 'الفريق', 'titleEn': 'The Team'},
        {'title': 'البيانات المالية (رسم)', 'titleEn': 'Financials (chart)', 'needsData': True},
        {'title': 'الطلب التمويلي (رقم)', 'titleEn': 'The Ask (figure)', 'needsData': True},
        {'title': 'استخدام التمويل (دائرتان)', 'titleEn': 'Use of Funds (donuts)', 'needsData': True},
        {'title': 'أسئلة وأجوبة', 'titleEn': 'Q&A'},
        {'title': 'شكرًا وتواصل', 'titleEn': 'Thank You'},
    ],
    'brief': ('صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص والأرقام فقط؛ '
              'أرقام الرسوم في كتل JSON بالمعرّفات chart-data وchart-data-2 (أعمدة) وchart-data-3 (دائرتان)، والأشكال الزخرفية SVG لا تُمس.'),
}
