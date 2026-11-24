# -*- coding: utf-8 -*-
# نموذج «خارطة طريق بخمس محطات»: شريحتان بالتخطيط نفسه (فاتحة ثم داكنة): عنوان أعلى، طريق متعرّج ثلاثي الأبعاد SVG
# بخط منتصف متقطّع وست نقاط برتقالية، وست كتل نصية بأيقونة خطية وعنوان ونص وخط رابط رفيع إلى الطريق.
KEY = 'sl-5-milestone-roadmap-infographic'
TITLE = {'ar': 'خارطة طريق بخمس محطات', 'en': '5-Milestone Roadmap Infographic'}
COMMENT = 'نموذج «خارطة طريق بخمس محطات»: طريق متعرّج SVG بعرض متغيّر مع سماكة سفلية وخط متقطّع ونقاط برتقالية، وست كتل نصية بأيقونات خطية. الشريحة الأولى فاتحة والثانية داكنة بالتخطيط نفسه.'
VARS = '--ink: #1F1F1F; --ink-2: #3A3A3A; --accent: #CE9417; --navy: #1C2744; --navy-2: #33446A; --road-side: #C8D3E8; --icon: #41507A; --line: #8C8C8C; --bg: #FFFFFF; --bg-dark: #262626;'

STYLE = """  .ttl { position: absolute; top: 42px; inset-inline-start: 64px; margin: 0; font-size: 40px; font-weight: 400; line-height: 1.2; color: var(--ink); letter-spacing: -0.3px; white-space: nowrap; }
  html[lang="ar"] .ttl { font-weight: 500; }
  /* الطريق: رسم SVG يملأ الإطار؛ يُعكس أفقيًا في العربية ليبدأ من جهة البداية */
  .road { position: absolute; inset: 0; width: 1280px; height: 720px; }
  html[dir="rtl"] .road { transform: scaleX(-1); }
  .road .side { fill: var(--road-side); }
  .road .body { fill: url(#road-g); }
  .road .dash { fill: none; stroke: #FFFFFF; stroke-width: 4.5; stroke-dasharray: 30 22; stroke-linecap: butt; }
  .road .dot { fill: var(--accent); stroke: #FFFFFF; stroke-width: 5; }
  /* الكتل النصية */
  .it { position: absolute; }
  .it .ic { width: 52px; height: 52px; color: var(--icon); }
  .it .ic svg { width: 100%; height: 100%; fill: none; stroke: currentColor; stroke-width: 1.1; stroke-linecap: round; stroke-linejoin: round; }
  .it h2 { margin: 8px 0 8px; font-size: 22px; font-weight: 700; line-height: 1.25; color: var(--ink); }
  .it p { margin: 0; font-size: 16px; line-height: 20px; color: var(--ink-2); }
  .it.end { text-align: end; } .it.end .ic { margin-inline-start: auto; }
  .ln { position: absolute; width: 1px; background: var(--line); }
  .it1 { top: 342px; inset-inline-start: 64px; width: 236px; } .it1 .ic { width: 54px; height: 54px; }
  .it2 { top: 124px; inset-inline-start: 297px; width: 236px; } .it2 .ic { width: 44px; height: 44px; }
  .it3 { top: 506px; inset-inline-start: 193px; width: 236px; } .it3 .ic { width: 50px; height: 50px; }
  .it4 { top: 242px; inset-inline-start: 591px; width: 172px; } .it4 .ic { width: 56px; height: 44px; }
  .it5 { top: 139px; inset-inline-start: 812px; width: 236px; } .it5 .ic { width: 48px; height: 48px; }
  .it6 { top: 514px; inset-inline-start: 1028px; width: 172px; }
  .ic6 { position: absolute; top: 522px; inset-inline-start: 964px; width: 50px; height: 50px; color: var(--icon); }
  .ic6 svg { width: 100%; height: 100%; fill: none; stroke: currentColor; stroke-width: 1.1; stroke-linecap: round; stroke-linejoin: round; }
  .ln1 { top: 206px; inset-inline-start: 75px; height: 114px; }
  .ln2 { top: 258px; inset-inline-start: 315px; height: 34px; }
  .ln3 { top: 332px; inset-inline-start: 416px; height: 128px; }
  .ln4 { top: 412px; inset-inline-start: 617px; height: 136px; }
  .ln5 { top: 284px; inset-inline-start: 847px; height: 52px; }
  .ln6 { top: 446px; inset-inline-start: 1073px; height: 56px; }
  /* الشريحة الداكنة: الخلفية والنصوص والطريق بألوان معكوسة */
  .s2 { background: var(--bg-dark); }
  .s2 .ttl, .s2 .it h2 { color: #FFFFFF; } .s2 .it p { color: #E6E6E6; }
  .s2 .it .ic, .s2 .ic6 { color: #D6DBE6; } .s2 .ln { background: #7A7A7A; }
  .s2 .road .side { fill: #2F3E66; } .s2 .road .body { fill: url(#road-g2); }"""


# ---------- بناء شكل الطريق بعرض متغيّر من خط منتصف بنقاط (x, y, العرض) ----------
def _cr(p0, p1, p2, p3, t):
    t2, t3 = t * t, t * t * t
    return tuple(0.5 * ((2 * p1[i]) + (-p0[i] + p2[i]) * t + (2 * p0[i] - 5 * p1[i] + 4 * p2[i] - p3[i]) * t2 + (-p0[i] + 3 * p1[i] - 3 * p2[i] + p3[i]) * t3) for i in range(3))


def _samples(pts, n=14):
    out = []
    ext = [pts[0]] + list(pts) + [pts[-1]]
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


def ribbon(pts, eps=0.5):
    """مضلّع الطريق: الإزاحة العمودية عن خط المنتصف بنصف العرض على الجانبين."""
    s = _samples(pts)
    left, right = [], []
    for i, (x, y, w) in enumerate(s):
        a, b = s[max(i - 1, 0)], s[min(i + 1, len(s) - 1)]
        tx, ty = b[0] - a[0], b[1] - a[1]
        L = (tx * tx + ty * ty) ** 0.5 or 1e-9
        nx, ny = -ty / L, tx / L
        left.append((x + nx * w / 2, y + ny * w / 2)); right.append((x - nx * w / 2, y - ny * w / 2))
    poly = _rdp(left, eps) + _rdp(right[::-1], eps)
    return 'M' + ' '.join('%.1f,%.1f' % p for p in poly) + 'Z'


def centerline(pts):
    """خط المنتصف كمنحنيات بيزيه مكعّبة مكافئة لمنحنى كاتمول-روم."""
    P = [(p[0], p[1]) for p in pts]
    ext = [P[0]] + P + [P[-1]]
    d = 'M%.1f,%.1f' % P[0]
    for i in range(1, len(ext) - 2):
        p0, p1, p2, p3 = ext[i - 1], ext[i], ext[i + 1], ext[i + 2]
        c1 = (p1[0] + (p2[0] - p0[0]) / 6, p1[1] + (p2[1] - p0[1]) / 6)
        c2 = (p2[0] - (p3[0] - p1[0]) / 6, p2[1] - (p3[1] - p1[1]) / 6)
        d += ' C%.1f,%.1f %.1f,%.1f %.1f,%.1f' % (c1 + c2 + p2)
    return d


# خط منتصف الطريق (مقروء من الصورة): يبدأ من الحافة اليسرى، ينعطف نزولًا، يمر أفقيًا، ينحدر إلى الأسفل، يصعد في حدبة ثم يخرج يمينًا
ROAD_PTS = [(-40, 154, 42), (80, 159, 42), (150, 164, 46), (200, 171, 52), (230, 198, 64), (222, 234, 66), (207, 260, 62), (198, 282, 58),
            (208, 301, 50), (245, 309, 46), (300, 309, 45), (360, 310, 48), (400, 314, 56), (440, 336, 70), (470, 378, 86),
            (505, 432, 96), (550, 510, 92), (600, 562, 86), (650, 584, 80), (705, 590, 78), (755, 582, 84), (788, 552, 98),
            (810, 505, 94), (814, 455, 94), (812, 410, 96), (824, 374, 80), (860, 362, 62), (900, 362, 54), (960, 373, 55),
            (1000, 385, 56), (1100, 420, 59), (1200, 458, 61), (1320, 502, 64)]
DOTS = [(75, 160), (315, 307), (416, 315), (617, 570), (847, 369), (1073, 410)]

GLOBAL_DEFS = ('<defs>'
               '<path id="road-shape" d="%s"/>' % ribbon(ROAD_PTS)
               + '<path id="road-center" d="%s"/>' % centerline(ROAD_PTS)
               + '<linearGradient id="road-g" gradientUnits="userSpaceOnUse" x1="0" y1="140" x2="1280" y2="640"><stop offset="0" stop-color="#34466C"/><stop offset=".5" stop-color="#222E4C"/><stop offset="1" stop-color="#141B2C"/></linearGradient>'
               '<linearGradient id="road-g2" gradientUnits="userSpaceOnUse" x1="0" y1="0" x2="1280" y2="0"><stop offset="0" stop-color="#6A86BE"/><stop offset=".55" stop-color="#3F5587"/><stop offset="1" stop-color="#1B2A48"/></linearGradient>'
               '</defs>')

ROAD = ('<svg class="road" viewBox="0 0 1280 720" aria-hidden="true">'
        '<use href="#road-shape" class="side" transform="translate(0 13)"/><use href="#road-shape" class="body"/><use href="#road-center" class="dash"/>'
        + ''.join('<circle class="dot" cx="%d" cy="%d" r="11.5"/>' % d for d in DOTS) + '</svg>')


def gear(cx, cy, r, n=8):
    import math
    out = '<circle cx="%.1f" cy="%.1f" r="%.1f"/><circle cx="%.1f" cy="%.1f" r="%.1f"/>' % (cx, cy, r, cx, cy, r * 0.38)
    for k in range(n):
        a = 2 * math.pi * k / n
        out += '<path d="M%.1f %.1fL%.1f %.1f"/>' % (cx + r * math.cos(a), cy + r * math.sin(a), cx + (r + 1.3) * math.cos(a), cy + (r + 1.3) * math.sin(a))
    return out


ICONS = {
    'rocket': '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M9.6 14.4c2.3-5 5.6-8.6 10.4-10.4-.5 4.6-2.4 8.2-5.5 11l-4.9-.6z"/><path d="M9.6 14.4 5 13.3l2.5-2.7 3.1.3M9.6 14.4l1.1 4.6 2.7-2.5-.3-3.1"/><circle cx="15" cy="9" r="1.4"/><path d="M4.3 20.2c.2-2 1-3.2 2.8-3.6M7.8 16.2l-1.2 1.2"/></svg>',
    'gears': '<svg viewBox="0 0 24 24" aria-hidden="true">' + gear(15.5, 7.5, 3.4) + gear(7.5, 16.5, 3.4) + gear(16.5, 17.5, 2.2, 7) + '</svg>',
    'briefcase': '<svg viewBox="0 0 24 24" aria-hidden="true"><rect x="2.5" y="7.5" width="19" height="13" rx="1.4"/><path d="M9 7.5V5.6c0-.6.5-1.1 1.1-1.1h3.8c.6 0 1.1.5 1.1 1.1v1.9M2.5 12.5h19M10.3 11.2h3.4v2.6h-3.4z"/></svg>',
    'people': '<svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="7.2" r="2.3"/><circle cx="5.2" cy="8.6" r="1.9"/><circle cx="18.8" cy="8.6" r="1.9"/><path d="M7.8 17.5c0-2.6 1.9-4.4 4.2-4.4s4.2 1.8 4.2 4.4M1.8 17c0-2.1 1.4-3.6 3.4-3.6.6 0 1.2.1 1.7.4M22.2 17c0-2.1-1.4-3.6-3.4-3.6-.6 0-1.2.1-1.7.4"/></svg>',
    'board': '<svg viewBox="0 0 24 24" aria-hidden="true"><rect x="2.5" y="3.5" width="19" height="13" rx=".8"/><circle cx="12" cy="10" r="3.6"/><path d="M12 6.4V10h3.6M12 16.5v2.8M8.6 22l3.4-2.7 3.4 2.7M12 1.5v2"/></svg>',
    'target': '<svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="10.5" cy="13.5" r="8"/><circle cx="10.5" cy="13.5" r="4.6"/><circle cx="10.5" cy="13.5" r="1.3"/><path d="M10.5 13.5 17.6 6.4M17.6 6.4V3.3M17.6 6.4h3.1"/></svg>',
}


def item(n, icon, end=False):
    return '<div class="it it%d%s"><div class="ic">%s</div><h2>§H%d§</h2><p>§P%d§</p></div>' % (n, ' end' if end else '', ICONS[icon], n, n)


SLIDE = ('\n    <h1 class="ttl">§TTL§</h1>\n    ' + ROAD + '\n    '
         + ''.join('<div class="ln ln%d"></div>' % i for i in range(1, 7)) + '\n    '
         + item(1, 'rocket') + '\n    ' + item(2, 'gears') + '\n    ' + item(3, 'briefcase', True) + '\n    '
         + item(4, 'people') + '\n    ' + item(5, 'board') + '\n    '
         + '<div class="ic6">' + ICONS['target'] + '</div><div class="it it6"><h2>§H6§</h2><p>§P6§</p></div>\n  ')

SLIDES = [SLIDE, SLIDE]

_AR = {'TTL': 'قالب خارطة طريق بخمس محطات'}
_EN = {'TTL': '5-Milestone Roadmap Infographic PowerPoint Template'}
for _i in range(1, 7):
    _AR['H%d' % _i] = 'عنوان نائب'; _AR['P%d' % _i] = 'هذا نص توضيحي. أضف النص المطلوب هنا.'
    _EN['H%d' % _i] = 'Placeholder'; _EN['P%d' % _i] = 'This is a sample text. Insert your desired text here.'
STR = {'ar': _AR, 'en': _EN}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'projects', 'icon': '🛣️',
    'name': 'خارطة طريق بخمس محطات', 'nameEn': '5-Milestone Roadmap Infographic',
    'description': 'شريحتان لخارطة طريق متعرّجة بست محطات وأيقونات: نسخة فاتحة ونسخة داكنة بالتخطيط نفسه',
    'descriptionEn': 'Two slides of a winding roadmap with six milestones and icons: a light and a dark version',
    'sections': [
        {'title': 'خارطة الطريق (فاتحة)', 'titleEn': 'Roadmap (light)'},
        {'title': 'خارطة الطريق (داكنة)', 'titleEn': 'Roadmap (dark)'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص فقط؛ الطريق رسم SVG مشترك (road-shape/road-center) والمحطات كتل .it1–.it6 وخطوط الربط .ln1–.ln6.',
}
