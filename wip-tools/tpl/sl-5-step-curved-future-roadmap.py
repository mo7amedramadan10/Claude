# -*- coding: utf-8 -*-
# نموذج «خارطة طريق مستقبلية منحنية بخمس خطوات»: شريحة واحدة: عنوان أعلى، مسار متدرّج (وردي إلى بنفسجي) من خمسة مقاطع SVG
# ينتهي بسهم، وخمسة دبابيس خريطة بظل، وخمس بطاقات رمادية بترويسة ملوّنة وذيل، وخطوط منقّطة تربط البطاقة بدبّوسها.
KEY = 'sl-5-step-curved-future-roadmap'
TITLE = {'ar': 'خارطة طريق مستقبلية منحنية بخمس خطوات', 'en': '5-Step Curved Future Roadmap'}
COMMENT = 'نموذج «خارطة طريق مستقبلية منحنية بخمس خطوات»: مسار SVG من مقاطع متدرّجة ينتهي بسهم، دبابيس خريطة بظلال، وخمس بطاقات رمادية بترويسات ملوّنة (وردي إلى بنفسجي) وذيل مثلث. الرسم يُعكس أفقيًا في العربية.'
VARS = '--ink: #1F1F1F; --ink-2: #2B2B2B; --accent: #7C6D8E; --pink: #A68791; --purple: #69628A; --card: #F2F2F2; --shadow: #A5A5A5; --c1: #A98891; --c2: #7D6F92; --c3: #8A768F; --c4: #73698B; --c5: #69628B; --bg: #FFFFFF;'

STYLE = """  .ttl { position: absolute; top: 42px; inset-inline-start: 64px; margin: 0; font-size: 40px; font-weight: 400; line-height: 1.2; color: var(--ink); letter-spacing: -0.3px; white-space: nowrap; }
  html[lang="ar"] .ttl { font-weight: 500; }
  /* الرسم الزخرفي (المسار والسهم والدبابيس والخطوط المنقّطة) يملأ الإطار ويُعكس في العربية */
  .art { position: absolute; inset: 0; width: 1280px; height: 720px; }
  html[dir="rtl"] .art { transform: scaleX(-1); }
  .art .sh { fill: var(--shadow); }
  .art .dots { fill: none; stroke: #B49AA3; stroke-width: 3.2; stroke-linecap: round; stroke-dasharray: 0 8; }
  /* البطاقات: ترويسة ملوّنة ونص متوسّط وذيل مثلث أسفل المنتصف */
  .card { position: absolute; width: 163px; height: 167px; background: var(--card); border-radius: 8px; text-align: center; }
  .card::after { content: ""; position: absolute; bottom: -12px; inset-inline: 0; margin-inline: auto; width: 0; height: 0; border-inline: 11px solid transparent; border-top: 12px solid var(--card); }
  .card h2 { margin: 0; height: 33px; line-height: 33px; border-start-start-radius: 8px; border-start-end-radius: 8px; font-size: 18px; font-weight: 700; color: #FFFFFF; }
  .card p { margin: 0; padding: 11px 14px 0; font-size: 16.5px; line-height: 22.5px; color: var(--ink-2); }
  .c1 { top: 167px; inset-inline-start: 88px; } .c1 h2 { background: var(--c1); }
  .c2 { top: 98px; inset-inline-start: 457px; } .c2 h2 { background: var(--c2); }
  .c3 { top: 505px; inset-inline-start: 300px; } .c3 h2 { background: var(--c3); }
  .c4 { top: 198px; inset-inline-start: 787px; } .c4 h2 { background: var(--c4); }
  .c5 { top: 167px; inset-inline-start: 995px; } .c5 h2 { background: var(--c5); }"""


# ---------- أدوات هندسية: شريط بعرض متغيّر من خط منتصف، وقصّ بنصف مستوى ----------
def _cr(p0, p1, p2, p3, t):
    t2, t3 = t * t, t * t * t
    return tuple(0.5 * ((2 * p1[i]) + (-p0[i] + p2[i]) * t + (2 * p0[i] - 5 * p1[i] + 4 * p2[i] - p3[i]) * t2 + (-p0[i] + 3 * p1[i] - 3 * p2[i] + p3[i]) * t3) for i in range(3))


def _samples(pts, n=14):
    out, ext = [], [pts[0]] + list(pts) + [pts[-1]]
    for i in range(1, len(ext) - 2):
        for k in range(n):
            out.append(_cr(ext[i - 1], ext[i], ext[i + 1], ext[i + 2], k / n))
    out.append(pts[-1])
    return out


def _rdp(points, eps):
    if len(points) < 3:
        return points
    (x1, y1), (x2, y2) = points[0], points[-1]
    dx, dy = x2 - x1, y2 - y1
    L = (dx * dx + dy * dy) ** 0.5 or 1e-9
    dmax, idx = 0, 0
    for i in range(1, len(points) - 1):
        d = abs(dy * (points[i][0] - x1) - dx * (points[i][1] - y1)) / L
        if d > dmax:
            dmax, idx = d, i
    if dmax > eps:
        return _rdp(points[:idx + 1], eps)[:-1] + _rdp(points[idx:], eps)
    return [points[0], points[-1]]


def ribbon_pts(pts, eps=0.4):
    s = _samples(pts)
    left, right = [], []
    for i, (x, y, w) in enumerate(s):
        a, b = s[max(i - 1, 0)], s[min(i + 1, len(s) - 1)]
        tx, ty = b[0] - a[0], b[1] - a[1]
        L = (tx * tx + ty * ty) ** 0.5 or 1e-9
        nx, ny = -ty / L, tx / L
        left.append((x + nx * w / 2, y + ny * w / 2)); right.append((x - nx * w / 2, y - ny * w / 2))
    return _rdp(left, eps) + _rdp(right[::-1], eps)


def clip(poly, a, b):
    """يبقي الجزء من المضلّع الواقع على يسار الاتجاه a→b (قاعدة ساذرلاند-هودجمان لحافة واحدة)."""
    def side(p): return (b[0] - a[0]) * (p[1] - a[1]) - (b[1] - a[1]) * (p[0] - a[0])
    out = []
    for i in range(len(poly)):
        p, q = poly[i], poly[(i + 1) % len(poly)]
        sp, sq = side(p), side(q)
        if sp >= 0: out.append(p)
        if (sp >= 0) != (sq >= 0):
            t = sp / (sp - sq)
            out.append((p[0] + (q[0] - p[0]) * t, p[1] + (q[1] - p[1]) * t))
    return out


def path(poly):
    return 'M' + ' '.join('%.1f,%.1f' % p for p in poly) + 'Z'


# المقاطع الخمسة (خط المنتصف بنقاط x, y, العرض) مقروءة من الصورة
SEG_A = ribbon_pts([(62, 552, 4), (160, 509, 5), (252, 468, 6)])
SEG_B = ribbon_pts([(268, 462, 5), (380, 422, 6), (484, 385, 7)])
SEG_C = ribbon_pts([(497, 380, 6), (550, 363, 7), (600, 349, 7), (650, 339, 9), (700, 336, 13), (745, 344, 26), (765, 362, 30)])
SEG_D = clip(clip(ribbon_pts([(772, 376, 22), (758, 393, 24), (742, 418, 25), (722, 444, 26), (690, 480, 28), (658, 505, 31), (645, 535, 34),
                              (650, 565, 36), (680, 581, 36), (700, 585, 35), (760, 586, 35), (800, 581, 34), (830, 573, 34), (860, 566, 34)]),
                   (735, 401), (765, 390)), (838, 558), (829, 592))
SEG_E = 'M857,548 Q985,516 1108,467 L1121,444 L1216,456 L1088,534 L1095,515 Q975,555 847,592 Z'


def grad(i, x1, x2):
    return '<linearGradient id="rg%d" gradientUnits="userSpaceOnUse" x1="%d" y1="0" x2="%d" y2="0"><stop offset="0" stop-color="#A68791"/><stop offset="1" stop-color="#69628A"/></linearGradient>' % (i, x1, x2)


GLOBAL_DEFS = '<defs>' + grad(1, 62, 252) + grad(2, 268, 484) + grad(3, 497, 777) + grad(4, 623, 838) + grad(5, 847, 1216) + '</defs>'

PINS = [(165, 415, 'c1'), (538, 290, 'c2'), (380, 407, 'c3'), (730, 487, 'c4'), (1075, 394, 'c5')]  # (x المنتصف, أعلى الدبوس, اللون)


def pin(cx, top, c):
    x = cx - 20
    d = ('M%d,%d c-11,0 -20,9 -20,20 c0,14 20,27 20,27 c0,0 20,-13 20,-27 c0,-11 -9,-20 -20,-20z' % (cx, top))
    return ('<ellipse class="sh" cx="%d" cy="%.1f" rx="22" ry="5"/>' % (cx, top + 59.5)
            + '<path d="%s" fill="var(--%s)"/><circle cx="%d" cy="%d" r="7.5" fill="#FFFFFF"/>' % (d, c, cx, top + 19))


ART = ('<svg class="art" viewBox="0 0 1280 720" aria-hidden="true">'
       '<path d="%s" fill="url(#rg1)"/><path d="%s" fill="url(#rg2)"/><path d="%s" fill="url(#rg3)"/><path d="%s" fill="url(#rg4)"/><path d="%s" fill="url(#rg5)"/>'
       % (path(SEG_A), path(SEG_B), path(SEG_C), path(SEG_D), SEG_E)
       + '<path class="dots" d="M169,354V408 M383,478V500 M868,386L762,517 M1076,354V390"/>'
       + ''.join(pin(*p) for p in PINS) + '</svg>')


def card(n):
    return '<div class="card c%d"><h2>§H%d§</h2><p>§P%d§</p></div>' % (n, n, n)


SLIDES = ['\n    <h1 class="ttl">§TTL§</h1>\n    ' + ART + '\n    ' + '\n    '.join(card(i) for i in range(1, 6)) + '\n  ']

_AR = {'TTL': 'قالب خارطة طريق مستقبلية منحنية بخمس خطوات'}
_EN = {'TTL': '5-Step Curved Future Roadmap PowerPoint Template'}
for _i in range(1, 6):
    _AR['H%d' % _i] = 'عنوان نائب'; _AR['P%d' % _i] = 'هذا نص توضيحي. أضف النص المطلوب هنا.'
    _EN['H%d' % _i] = 'Placeholder'; _EN['P%d' % _i] = 'This is a Insert your desired text here. sample text.'
STR = {'ar': _AR, 'en': _EN}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'mgmt', 'icon': '🧭',
    'name': 'خارطة طريق منحنية بخمس خطوات', 'nameEn': '5-Step Curved Future Roadmap',
    'description': 'شريحة واحدة لخارطة طريق منحنية بخمس خطوات: مسار متدرّج ينتهي بسهم، دبابيس خريطة وبطاقات بترويسات ملوّنة',
    'descriptionEn': 'One slide of a 5-step curved roadmap: gradient path ending in an arrow, map pins and cards with colored headers',
    'sections': [
        {'title': 'خارطة الطريق بخمس خطوات', 'titleEn': '5-Step Roadmap'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص فقط؛ المسار والدبابيس رسم SVG واحد (.art) والبطاقات .c1–.c5 بترويسة h2 ونص p.',
}
