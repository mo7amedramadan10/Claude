# -*- coding: utf-8 -*-
# نموذج «الملف التعريفي للشركة — دوائر»: سبع عشرة شريحة بأسلوب الدوائر المتدرّجة (كحلي #1B4865، فيروزي #61B6CB، فاتح #BFE9E8، أزرق #5FA8D3).
# الأشكال الزخرفية مرسومة SVG بإحداثيات الأصل الإنجليزي داخل طبقة .full بحجم الإطار، وتنعكس كاملة في العربية بـ scaleX(-1)؛
# النصوص كلها HTML بخصائص منطقية فتنعكس تلقائيًا. الخريطة النقطية لا تنعكس (جغرافيا) بل يتبدل موضعها فقط.
KEY = 'sl-circles-company-profile'
TITLE = {'ar': 'الملف التعريفي للشركة — دوائر', 'en': 'Circles Company Profile'}
COMMENT = ('نموذج «الملف التعريفي للشركة — دوائر»: غلاف بدائرة كحلية كبيرة وفقاعات متدرّجة، فهرس، تاريخ ومحطات على قوس، رؤية ورسالة، '
           'عمليات حول العالم بخريطة نقطية، فريق القيادة والثقافة، المنتجات، المشكلة، الحل، السوق (حلقة بيانات)، نموذج العمل، '
           'لمحة مالية (حلقة بيانات)، الإنجازات، آراء العملاء، خارطة الطريق، كلمة الرئيس التنفيذي، وشكرًا وتواصل.')
VARS = ('--ink: #1A1A1A; --navy: #1B4865; --accent: #61B6CB; --teal: #61B6CB; --lt: #BFE9E8; --blue: #5FA8D3; '
        '--num: #9DD0DD; --rule: #BFE6E8; --bg: #FFFFFF;')
A = '/templates/freeform/sl-circles-company-profile/assets/'

FONTS = {'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@300;400;600;700&display=swap" rel="stylesheet">'}
FONT_STACK = {'en': "'Open Sans', 'Segoe UI', Arial, sans-serif"}


# ---------- أدوات ----------
def off(F, L):
    """إزاحة خط الأساس عن أعلى صندوق السطر لخط Open Sans (صعود 1.069 ونزول 0.293)."""
    return (L - 1.362 * F) / 2 + 1.069 * F


def T(tag, cls, x, base, F, L, tok, w=None, center=None, extra=''):
    """نص بموضع مطلق محسوب من خط الأساس في الأصل. center = إحداثي المنتصف الأفقي (مع w)."""
    top = round(base - off(F, L))
    st = 'top:%dpx;' % top
    if center is not None:
        st += 'inset-inline-start:%dpx;width:%dpx;text-align:center;' % (round(center - w / 2), w)
    else:
        st += 'inset-inline-start:%dpx;' % x + ('width:%dpx;' % w if w else '')
    st += 'font-size:%gpx;line-height:%gpx;' % (F, L) + extra
    return '<%s class="t %s" style="%s">§%s§</%s>' % (tag, cls, st, tok, tag)


def ring(cx, cy, R, r):
    """حلقة بثقب (fill-rule evenodd)."""
    return ('M%g,%g a%g,%g 0 1,0 %g,0 a%g,%g 0 1,0 %g,0 Z M%g,%g a%g,%g 0 1,0 %g,0 a%g,%g 0 1,0 %g,0 Z'
            % (cx - R, cy, R, R, 2 * R, R, R, -2 * R, cx - r, cy, r, r, 2 * r, r, r, -2 * r))


def C(cx, cy, r, fill, extra=''):
    return '<circle cx="%g" cy="%g" r="%g" fill="%s"%s/>' % (cx, cy, r, fill, extra)


def RING(cx, cy, R, r, fill, extra=''):
    return '<path fill-rule="evenodd" d="%s" fill="%s"%s/>' % (ring(cx, cy, R, r), fill, extra)


def full(inner):
    return '<svg class="full" viewBox="0 0 1280 720" aria-hidden="true">' + inner + '</svg>'


# الزخرفة المشتركة أعلى اليسار (حلقة وفقاعتان)
def deco(dark=False):
    g, b = ('url(#gRd)', 'url(#gBd)') if dark else ('url(#gR)', 'url(#gB)')
    return RING(64, 15, 130, 52, g) + C(242, 91, 18, b) + C(64, 227, 33, b)


TTL_F, TTL_L = 52, 64


def title(tok, cls=''):
    return T('h1', 'ttl ' + cls, 111, 193, TTL_F, TTL_L, tok)


# ---------- الأيقونات ----------
I = {
    'tree': '<rect x="7.5" y="2" width="9" height="5" rx=".6"/><path d="M11.3 7h1.4v3.3H19v3.2h-1.4v-1.8h-4.9v1.8h-1.4v-1.8H6.4v1.8H5v-3.2h6.3z"/><path d="M12 11.6l1.6 1.6-1.6 1.6-1.6-1.6z"/><circle cx="5.7" cy="18.5" r="2"/><circle cx="12" cy="18.5" r="2"/><circle cx="18.3" cy="18.5" r="2"/>',
    'case': '<path d="M9 3h6a1.2 1.2 0 0 1 1.2 1.2V6H20a2 2 0 0 1 2 2v3.6H2V8a2 2 0 0 1 2-2h3.8V4.2A1.2 1.2 0 0 1 9 3zm.4 1.6V6h5.2V4.6z"/><path d="M2 12.8h8.4v1.5h3.2v-1.5H22V19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2z"/>',
    'hub': '<path d="M12 12L12 4.5M12 12l6.5-3.8M12 12l6.5 3.8M12 12v7.5M12 12l-6.5 3.8M12 12L5.5 8.2" stroke="currentColor" stroke-width="1.3" fill="none"/><circle cx="12" cy="12" r="3.6"/><circle cx="12" cy="3.6" r="1.9"/><circle cx="19.4" cy="7.8" r="1.9"/><circle cx="19.4" cy="16.2" r="1.9"/><circle cx="12" cy="20.4" r="1.9"/><circle cx="4.6" cy="16.2" r="1.9"/><circle cx="4.6" cy="7.8" r="1.9"/><circle cx="12" cy="11" r="1.1" fill="#fff"/><path d="M10 14.2a2 2 0 0 1 4 0z" fill="#fff"/>',
    'inbox': '<circle cx="15.5" cy="6.5" r="4.5"/><path d="M13.4 6.6l1.5 1.5 2.8-2.9" stroke="#fff" stroke-width="1.3" fill="none"/><path d="M2 11h3.5l2 3h9l2-3H22v9a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1z"/><path d="M4 10.2l1.6-3.2H9.6v1.4H6.5l-.9 1.8z"/>',
    'clip': '<path d="M8.5 3h7v2.6h-7z"/><path d="M6.5 4.2h1v2.6h9V4.2h1A1.5 1.5 0 0 1 19 5.7v15.1a1.2 1.2 0 0 1-1.2 1.2H6.2A1.2 1.2 0 0 1 5 20.8V5.7a1.5 1.5 0 0 1 1.5-1.5zm.7 4.3v11.7h9.6V8.5z"/><path d="M8.3 10.5l.8.8 1.4-1.5M8.3 13.8l.8.8 1.4-1.5M8.3 17.1l.8.8 1.4-1.5M12 10.9h4M12 14.2h4M12 17.5h4" stroke="currentColor" stroke-width="1" fill="none"/>',
    'cal': '<path d="M7 2h1.6v2.4H7zM15.4 2H17v2.4h-1.6z"/><path d="M3 5h18v4H3z"/><path d="M3 10h18v11H3zm2 1.6v2.2h2.8v-2.2zm4.2 0v2.2H12v-2.2zm4.2 0v2.2h2.8v-2.2zm4.2 0v2.2H19v-2.2zM5 15.2v2.2h2.8v-2.2zm4.2 0v2.2H12v-2.2zm4.2 0v2.2h2.8v-2.2zm4.2 0v2.2H19v-2.2z" fill-rule="evenodd"/>',
    'bars': '<path d="M3 3h1.5v16.5H21V21H3z"/><path d="M7 13h2.6v5H7zM11 10h2.6v8H11zM15 12h2.6v6H15zM18.6 5H21v13h-2.4z"/><path d="M6.5 11l4-4 2.5 2 3.5-4" stroke="currentColor" stroke-width="1.3" fill="none"/><path d="M15 4.5h2.8v2.8z"/>',
    'flow': '<path d="M5 2.5l2.5 2.5L5 7.5 2.5 5z"/><circle cx="18" cy="5" r="2.5"/><circle cx="5" cy="19" r="2.5"/><path d="M15.5 16.5h5v5h-5z"/><path d="M8.5 5h5.5M8.5 19h5.5M5 9v6" stroke="currentColor" stroke-width="1.3" fill="none"/><path d="M8 3.4L9.8 5 8 6.6zM14 17.4l1.8 1.6-1.8 1.6zM3.4 13.6L5 15.4l1.6-1.8z"/>',
    'mail': '<g fill="none" stroke="currentColor" stroke-width="1.4" stroke-linejoin="round"><path d="M3 10l9-6.5 9 6.5v10.5H3z"/><path d="M3 10l9 6.5 9-6.5"/><path d="M6.5 12.3V6.5h11v5.8"/><path d="M9 9h6M9 11.3h6"/></g>',
    'phone': '<g fill="none" stroke="currentColor" stroke-width="1.4" stroke-linecap="round" stroke-linejoin="round"><path d="M6.6 3.5l2.6 3.9-1.8 1.9c.9 2.4 2.8 4.3 5.3 5.4l1.9-1.8 3.9 2.6-1.4 3c-6.8.4-13-5.8-12.5-12.6z"/><path d="M14 6.5a4 4 0 0 1 3.5 3.5M14.5 3a7.5 7.5 0 0 1 6.5 6.5"/></g>',
    'globe': '<g fill="none" stroke="currentColor" stroke-width="1.4"><circle cx="12" cy="12" r="9"/><ellipse cx="12" cy="12" rx="4" ry="9"/><path d="M3 12h18M4.5 7.2h15M4.5 16.8h15"/></g>',
}


def icon(name, cls=''):
    return '<svg class="ico %s" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">%s</svg>' % (cls, I[name])


def logo(cls):
    """شعار تقريبي: مربع مستدير مائل كحلقة مفتوحة الزاويتين."""
    return ('<svg class="logo %s" viewBox="0 0 60 64" aria-hidden="true">'
            '<path fill-rule="evenodd" fill="currentColor" d="M9 8.5Q31 -1 54 7.5Q61 33 54.5 56Q32 65 7.5 57.5Q-1 33 9 8.5Z M17.5 19Q32 13.5 45.5 18.5Q50 33 45.5 45.5Q32 50.5 17.5 46Q13 32.5 17.5 19Z"/>'
            '<path d="M3 21L20 14M41 50L58 43" stroke="var(--gap)" stroke-width="4"/></svg>' % cls)


STAR = '<svg viewBox="0 0 24 24" aria-hidden="true"><path fill="#FFC000" d="M12 1.8l3.1 6.6 7.2.9-5.3 5 1.4 7.1L12 17.9l-6.4 3.5 1.4-7.1-5.3-5 7.2-.9z"/></svg>'

# علامة اقتباس متدرّجة (شريحة 16): مسار بحجم 60×120
QSHAPE = 'M0,0 A60,60 0 0 1 60,60 V120 H0 V62 H30 A30,30 0 0 0 0,32 Z'

GLOBAL_DEFS = ('<defs>'
    '<linearGradient id="gR" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#ADDAE6"/><stop offset="1" stop-color="#98C6E3"/></linearGradient>'
    '<linearGradient id="gB" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#D0EEEF"/><stop offset="1" stop-color="#9AC8E5"/></linearGradient>'
    '<linearGradient id="gRd" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#97CBD6"/><stop offset="1" stop-color="#4485AA"/></linearGradient>'
    '<linearGradient id="gBd" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#98C7CD"/><stop offset="1" stop-color="#4485A9"/></linearGradient>'
    '<linearGradient id="gCov" gradientUnits="userSpaceOnUse" x1="0" y1="0" x2="0" y2="720"><stop offset="0" stop-color="#99C7E2"/><stop offset=".45" stop-color="#A3D2E3"/><stop offset="1" stop-color="#BDE8E8"/></linearGradient>'
    '<linearGradient id="gCov2" gradientUnits="userSpaceOnUse" x1="0" y1="0" x2="0" y2="720"><stop offset="0" stop-color="#BCE8E8"/><stop offset=".55" stop-color="#A2D1E3"/><stop offset="1" stop-color="#9AC8E2"/></linearGradient>'
    '<linearGradient id="gTR" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#BDE9E7"/><stop offset="1" stop-color="#74B1D9"/></linearGradient>'
    '<linearGradient id="gBL" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#BBE8E4"/><stop offset=".6" stop-color="#93C6D4"/><stop offset="1" stop-color="#6FA8C4"/></linearGradient>'
    '<linearGradient id="gBig" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#E2F4F5"/><stop offset="1" stop-color="#B2D7E9"/></linearGradient>'
    '<linearGradient id="gArc" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#BFE9E8"/><stop offset="1" stop-color="#9BC9E3"/></linearGradient>'
    '<linearGradient id="gQ" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#B8E5E3"/><stop offset="1" stop-color="#4686AB"/></linearGradient>'
    '<linearGradient id="gRib1" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#1B4865"/><stop offset=".55" stop-color="#4F8DA6"/><stop offset="1" stop-color="#61B6CB"/></linearGradient>'
    '<linearGradient id="gRib2" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#61B6CB"/><stop offset=".55" stop-color="#C9ECEE"/><stop offset="1" stop-color="#BFE9E8"/></linearGradient>'
    '<linearGradient id="gRib3" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#BFE9E8"/><stop offset=".5" stop-color="#A6D3E6"/><stop offset="1" stop-color="#5FA8D3"/></linearGradient>'
    '<linearGradient id="gArcB" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#5FA8D3"/><stop offset="1" stop-color="#62A9D6"/></linearGradient>'
    '<linearGradient id="gRoad" gradientUnits="userSpaceOnUse" x1="400" y1="720" x2="1280" y2="130"><stop offset="0" stop-color="#F0F6FB"/><stop offset=".35" stop-color="#BEE3EA"/><stop offset=".7" stop-color="#BFE9E8"/><stop offset="1" stop-color="#BFE9E8"/></linearGradient>'
    '</defs>')


# ---------- الشرائح ----------
S = []

# 1) الغلاف
S.append(full(
    '<circle cx="261" cy="185" r="651" fill="url(#gCov)"/>'
    + C(277, 345, 513, '#1B4865')
    + RING(902, 133, 102, 40, 'url(#gTR)', ' opacity=".9"')
    + RING(38, 708, 130, 52, 'url(#gBL)')
    + C(717, 214, 30, 'url(#gBd)') + C(224, 635, 38, 'url(#gBd)')
    + C(1137, 240, 40, 'url(#gB)') + C(944, 327, 18, 'url(#gB)') + C(924, 576, 64, 'url(#gB)')
    + C(1173, 665, 155, 'url(#gBig)'))
    + logo('lg1')
    + '<p class="t lgt" style="top:158px;inset-inline-start:205px">§LOGO§</p>'
    + T('p', 'c-a', 111, 332, 64, 76, 'C1A')
    + T('h1', 'c-b', 111, 410, 66, 76, 'C1B')
    + T('p', 'c-c', 111, 476, 27, 34, 'C1C'))

# 2) جدول المحتويات
cells2 = ''
for i, (cx, ry) in enumerate([(111, 0), (490, 0), (867, 0), (111, 177), (490, 177), (867, 177)]):
    cells2 += (T('p', 'tocn', cx + 9, 325 + ry, 56, 64, 'N%d' % (i + 1))
               + T('p', 'body', cx + 105, 290 + ry, 22, 29, 'IY2')
               + '<i class="pill" style="top:%dpx;inset-inline-start:%dpx"></i><i class="hl" style="top:%dpx;inset-inline-start:%dpx;width:154px"></i>' % (349 + ry, cx, 358 + ry, cx + 149))
S.append(full(deco() + C(1168, 724, 153, 'url(#gBig)')) + title('TTL2') + cells2)

# 3) التاريخ والمحطات
arc3 = '<circle cx="1281" cy="745" r="710" fill="none" stroke="#1B4865" stroke-width="1.2"/>'
dots3 = ''.join(C(x, y, 22, c, ' stroke="#fff" stroke-width="7"') for x, y, c in [(875, 157, '#5FA8D3'), (742, 280, '#BFE9E8'), (646, 424, '#61B6CB'), (586, 587, '#1B4865')])
yrs3 = ''.join(T('p', 'year', x, b, 31, 36, 'YEAR') for x, b in [(806, 110), (656, 238), (551, 378), (481, 551)])
its3 = ''.join(T('p', 'body', x, b, 18, 23, 'IT2') for x, b in [(930, 158), (794, 281), (691, 425), (658, 587)])
S.append(full(deco() + arc3 + dots3 + C(1056, 375, 38, 'url(#gB)') + RING(1222, 675, 250, 180, 'url(#gArc)'))
         + title('TTL3') + T('p', 'body', 111, 306, 20, 25, 'P3', w=440)
         + T('p', 'kpi', 111, 433, 58, 64, 'K3A') + T('p', 'body', 111, 472, 20, 25, 'IY1')
         + T('p', 'kpi', 111, 585, 58, 64, 'K3B') + T('p', 'body', 111, 623, 20, 25, 'IY1')
         + yrs3 + its3)

# 4) الرسالة والرؤية
def card4(x, bg, bar, lab, big, txt):
    return ('<div class="card4" style="top:360px;inset-inline-start:%dpx;background:%s"></div>' % (x, bg)
            + T('p', 'on4', x + 31, 438, 25, 30, lab) + T('h2', 'big4', x + 30, 487, 42, 50, big)
            + '<i class="bar4" style="top:511px;inset-inline-start:%dpx;background:%s"></i>' % (x + 30, bar)
            + T('p', 'wt', x + 214, 431, 18, 22.5, txt, w=210))
S.append(full(deco() + RING(1185, 670, 250, 180, 'url(#gArc)')) + title('TTL4')
         + T('p', 'body', 111, 281, 23, 29, 'P4', w=560)
         + card4(111, 'var(--navy)', 'var(--teal)', 'OUR', 'VIS', 'IT4')
         + card4(640, 'var(--teal)', 'var(--navy)', 'OUR2', 'MIS', 'IT4')
         + full(C(1078, 366, 37, 'url(#gB)', ' opacity=".8"')))

# 5) العمليات حول العالم: خريطة نقطية تُرسم من سلسلة DOTS، ودبابيس
PIN = 'M12 0a12 12 0 0 0-12 12c0 8.6 12 23 12 23s12-14.4 12-23A12 12 0 0 0 12 0zm0 7a5 5 0 1 1 0 10 5 5 0 0 1 0-10z'


def pin(x, y, w, col):  # x,y: رأس الدبوس في الأصل (المنتصف الأعلى)
    s = w / 24.0
    return '<path fill="%s" fill-rule="evenodd" transform="translate(%g %g) scale(%g)" d="%s"/>' % (col, x - 540 - 12 * s, y - 15, s, PIN)


map5 = ('<svg class="map" id="map5" viewBox="0 0 740 510" aria-hidden="true"><g class="dots"></g>'
        + pin(694, 91, 42, '#61B6CB') + pin(1149, 97, 26, '#1B4865') + pin(911, 242, 26, '#1B4865')
        + pin(1182, 303, 26, '#1B4865') + pin(779, 357, 26, '#1B4865') + '</svg>')


def card5(x, bg, bar, num, sup):
    return ('<div class="card5" style="top:392px;inset-inline-start:%dpx;background:%s"></div>' % (x, bg)
            + T('p', 'num5', x + 19, 469, 56, 64, num, extra='') .replace('§%s§' % num, '§%s§<sup>+</sup>' % num)
            + '<i class="bar4" style="top:499px;inset-inline-start:%dpx;width:60px;height:7px;background:%s"></i>' % (x + 19, bar)
            + T('p', 'wt', x + 19, 545, 20, 25, 'IY2s', w=220))


S.append(full(deco()) + map5 + title('TTL5') + T('p', 'body', 111, 311, 23, 29, 'P5', w=500)
         + card5(111, 'var(--navy)', 'var(--teal)', 'K5A', 1) + card5(390, 'var(--teal)', 'var(--navy)', 'K5B', 1)
         + T('p', 'body', 835, 541, 23, 29, 'IT5', w=330))

# 6) فريق القيادة والثقافة
pan6 = ('<rect x="901" y="0" width="337" height="720" fill="#1B4865"/>' + C(416.5, 534, 544.4, '#FFFFFF')
        + '<rect x="1238" y="284" width="42" height="436" fill="#61B6CB"/>')
team6 = ''
for i, (x, cx) in enumerate([(110, 213), (375, 470), (638, 733)]):
    team6 += ('<img class="ph6" src="%steam-%d.jpg" alt="§ALT6§" style="top:315px;inset-inline-start:%dpx">' % (A, i + 1, x)
              + T('p', 'nm6', 0, 559, 20, 26, 'NAME', w=240, center=cx - 4)
              + '<i class="hl" style="top:579px;inset-inline-start:%dpx;width:190px"></i><i class="dot6" style="top:572px;inset-inline-start:%dpx"></i>' % (cx - 95, cx - 7)
              + T('p', 'rl6', 0, 616, 18, 24, 'ROLE', w=240, center=cx - 4))
cul6 = ''.join(T('p', 'wt', 988, b, 18, 22.5, 'IT6', w=190) for b in (300, 436, 583))
S.append(full(deco() + pan6) + title('TTL6') + team6
         + T('h2', 'cu6a', 988, 172, 40, 50, 'OUR3') + T('h2', 'cu6b', 988, 222, 40, 50, 'CUL') + cul6)

# 7) المنتجات والخدمات: ثلاث كبسولات
def arc(cx, cy, r, a0, a1):  # زوايا بالدرجات من الشمال باتجاه عقارب الساعة
    import math
    p = lambda a: (cx + r * math.sin(math.radians(a)), cy - r * math.cos(math.radians(a)))
    (x0, y0), (x1, y1) = p(a0), p(a1)
    big = 1 if (a1 - a0) % 360 > 180 else 0
    return 'M%.1f,%.1f A%g,%g 0 %d 1 %.1f,%.1f' % (x0, y0, r, r, big, x1, y1)


lines7 = ('<g fill="none" stroke="#1B4865" stroke-width="1.2">'
          + '<path d="%s"/><path d="M527,612 V720"/>' % arc(527, 459, 153, 148, 212)
          + '<path d="%s"/><path d="M814,0 V214"/>' % arc(814, 367, 153, -32, 32)
          + '<path d="%s"/><path d="M1101,498 V720"/>' % arc(1101, 345, 153, 148, 212) + '</g>')


def pill7(x, y, h, bg, cls, hb, tb):
    return ('<div class="pill7 %s" style="top:%dpx;inset-inline-start:%dpx;height:%dpx;background:%s"></div>' % (cls, y, x, h, bg)
            + T('h3', 'h7 ' + cls, 0, hb, 22, 28, 'HH', w=240, center=x + 128.5)
            + T('p', 'b7 ' + cls, 0, tb, 18, 22.5, 'IT7', w=220, center=x + 128.5))


S.append(full(deco() + lines7) + title('TTL7') + T('p', 'body', 111, 318, 23, 29, 'P7', w=270)
         + pill7(398, 231, 356, 'var(--navy)', 'w', 369, 418) + pill7(686, 239, 425, 'var(--teal)', 'w', 407, 456)
         + pill7(973, 49, 425, 'var(--lt)', 'k', 221, 270))

# 8) المشكلة: حلقات متحدة المركز وأقواس ملونة
rings8 = ''.join('<circle cx="838" cy="360" r="%d" fill="none" stroke="#EFEFEF" stroke-width="2.5"/>' % r for r in (342, 265, 188, 111))
arcs8 = ('<g fill="none" stroke-width="9" stroke-linecap="round">'
         '<path stroke="#61B6CB" d="%s"/>' % arc(838, 360, 342, 249, 343)
         + '<path stroke="#1B4865" d="%s"/>' % arc(838, 360, 268, 160, 251)
         + '<path stroke="url(#gArcB)" d="%s"/>' % arc(838, 360, 190, 279, 429)
         + '<path stroke="#61B6CB" d="%s"/>' % arc(838, 360, 111, 26, 177) + '</g>')
box8 = ''.join('<div class="box8" style="top:%dpx;inset-inline-start:%dpx"></div>' % (y, x) + T('p', 'body', x + 17, b, 18, 23, 'IT8', w=230)
               for x, y, b in [(553, 197, 241), (339, 316, 360), (897, 362, 408), (936, 540, 584)])
S.append(full(deco() + rings8 + arcs8) + title('TTL8') + T('p', 'body', 111, 485, 23, 29, 'P8', w=320) + box8)

# 9) الحل وعرض القيمة: أربع حلقات متصلة بأشرطة
rib9 = ''
for i, g in enumerate(['gRib1', 'gRib2', 'gRib3']):
    x0 = [205, 495, 785][i]; x1 = [495, 785, 1083][i]
    # شريط مائل من أعلى يمين الحلقة إلى أسفل يسار التالية
    rib9 += '<path fill="url(#%s)" d="M%d,%d L%d,%d L%d,%d L%d,%d Z"/>' % (g, x0 + 48, 315, x0 + 88, 330, x1 - 30, 485, x1 - 75, 482)
rings9 = ''.join(RING(x, 395, 94, 60, c) + C(x, 395, 60, '#fff') for x, c in [(205, '#1B4865'), (495, '#61B6CB'), (785, '#BFE9E8'), (1083, '#5FA8D3')])
ic9 = ''.join('<div class="ic9" style="top:370px;inset-inline-start:%dpx;color:%s">%s</div>' % (x - 25, c, icon(n))
              for x, c, n in [(205, '#1B4865', 'tree'), (495, '#61B6CB', 'case'), (785, '#61B6CB', 'hub'), (1083, '#5FA8D3', 'inbox')])
tx9 = ''.join(T('h3', 'h9', 0, 543, 22, 28, 'HH', w=260, center=cx) + T('p', 'body', 0, 592, 18, 22.5, 'IT7', w=210, center=cx)
              for cx in (202, 490, 783, 1079))
S.append(full(deco() + rib9 + rings9) + ic9 + title('TTL9') + tx9)

# 10) السوق: حلقة بيانات من chart-data
def card10(y, bg, hk, num, sup):
    return ('<div class="card10" style="top:%dpx;background:%s"></div>' % (y, bg)
            + T('h3', 'h10', 136, y + 38, 20, 26, 'HH')
            + T('p', 'num10', 136, y + 106, 50, 58, num).replace('§%s§' % num, '§%s§<sup>%s</sup>' % (num, sup))
            + T('p', 'wt', 335, y + 80, 18, 23, 'IT2', w=260))


S.append(full(deco() + '<g id="donut1" class="donut"></g>') + title('TTL10')
         + T('p', 'body', 111, 311, 23, 29, 'P5', w=500)
         + card10(375, 'linear-gradient(90deg,#55AFC5,#5AB2C8)', 'HH', 'K10A', '+')
         + card10(536, 'linear-gradient(90deg,#5FA8D6 20%,#94C2FF)', 'HH', 'K10B', '%')
         + T('p', 'pct pb', 0, 94, 24, 30, 'V10A', w=140, center=1067) + T('p', 'body', 0, 131, 20, 25, 'ITH', w=150, center=1065)
         + T('p', 'pct w', 0, 158, 24, 30, 'V10B', w=140, center=831) + T('p', 'wt', 0, 195, 20, 25, 'ITH', w=150, center=821)
         + T('p', 'pct w', 0, 440, 24, 30, 'V10C', w=140, center=763) + T('p', 'wt', 0, 487, 20, 25, 'ITH', w=150, center=763)
         + T('p', 'kc10', 0, 357, 56, 64, 'K10C', w=300, center=997)
         + T('p', 'body', 0, 398, 20, 25, 'IY2c', w=240, center=997))

# 11) نموذج العمل
grid11 = ('<i class="vl" style="top:248px;inset-inline-start:339px"></i><i class="vl" style="top:248px;inset-inline-start:640px"></i>'
          '<i class="vl" style="top:248px;inset-inline-start:912px"></i>'
          '<i class="rl" style="top:390px;inset-inline-start:339px;width:301px"></i><i class="rl" style="top:390px;inset-inline-start:912px;width:303px"></i>')
cells11 = ''
for x, b, ic, ix in [(109, 264, 'clip', 292), (358, 264, 'cal', 596), (659, 264, 'hub', 862), (933, 264, 'tree', 1158),
                     (358, 435, 'bars', 596), (933, 435, 'flow', 1158)]:
    cells11 += (T('h3', 'h11', x, b, 21, 27, 'HH') + T('p', 'body', x, b + 36, 20, 23, 'IY2', w=200)
                + '<div class="ic11" style="top:%dpx;inset-inline-start:%dpx">%s</div>' % (b - 15, ix, icon(ic)))
band11 = '<div class="band11"></div>' + ''.join(
    T('p', 'n11', x, 620, 40, 46, 'N%d' % (k + 1)) + T('p', 'wt', x + 61, 599, 19, 23, 'IT2', w=290)
    for k, x in enumerate((136, 487, 857)))
S.append(full(deco()) + title('TTL11') + grid11 + cells11 + band11)

# 12) لمحة مالية: حلقة بيانات من chart-data-2
st12 = ''
for x, b, col in [(570, 312, '#1B4865'), (912, 312, '#61B6CB'), (570, 538, '#BFE9E8'), (912, 538, '#5FA8D3')]:
    st12 += (T('p', 'kpi k12', x, b, 48, 56, 'K3A', extra='color:%s;' % col) + T('p', 'body', x, b + 46, 20, 25, 'IY2', w=220))
st12 += ('<i class="pill" style="top:418px;inset-inline-start:570px"></i><i class="hl" style="top:427px;inset-inline-start:719px;width:134px"></i>'
         '<i class="pill" style="top:418px;inset-inline-start:912px"></i><i class="hl" style="top:427px;inset-inline-start:1061px;width:134px"></i>')
S.append(full(deco() + '<g id="donut2" class="donut"></g>') + title('TTL12') + st12
         + T('p', 'kpi kc12', 0, 458, 56, 64, 'K3A', w=240, center=285))

# 13) أبرز الإنجازات: قوس بنقاط ودوائر
arc13 = ('<path d="%s" fill="none" stroke="#1B4865" stroke-width="1.2"/>' % arc(735, 649, 242, 248, 472)
         + '<path d="M0,560 H510 M960,560 H1280" stroke="#1B4865" stroke-width="1.2"/>'
         + ''.join(C(x, y, 15, c) for x, y, c in [(510, 560, '#1B4865'), (600, 450, '#61B6CB'), (735, 407, '#5FA8D3'), (872, 450, '#61B6CB'), (960, 560, '#1B4865')]))
circ13 = ''
for cx, cy in [(365, 560), (483, 318), (732, 238), (983, 318), (1110, 560)]:
    circ13 += ('<div class="c13" style="top:%dpx;inset-inline-start:%dpx"></div>' % (cy - 90, cx - 90)
               + T('h3', 'h13', 0, cy - 22, 21, 27, 'HH', w=170, center=cx - 5)
               + T('p', 'body', 0, cy + 14, 18, 23, 'IY2', w=170, center=cx - 5))
S.append(full(deco() + arc13) + title('TTL13') + T('p', 'body', 111, 273, 23, 29, 'P7', w=270) + circ13)

# 14) آراء العملاء: ثلاث دوائر متداخلة شفافة
circ14 = ''.join(C(x, 428, 189, c, ' opacity=".78"') for x, c in [(300, '#1B4865'), (645, '#61B6CB'), (990, '#5FA8D3')])
q14 = (C(155, 280, 54, '#fff') + '<g fill="#1B4865">'
       + '<path transform="translate(149 305) rotate(180) scale(.5)" d="%s"/><path transform="translate(183 305) rotate(180) scale(.5)" d="%s"/></g>' % (QSHAPE, QSHAPE))
tx14 = ''
for cx in (290, 637, 992):
    tx14 += ('<div class="stars" style="top:339px;inset-inline-start:%dpx">%s</div>' % (cx - 82, STAR * 5)
             + T('p', 'wt t14', 0, 421, 20, 25.5, 'TEST', w=290, center=cx)
             + T('h3', 'n14', 0, 522, 20, 26, 'NAME2', w=240, center=cx)
             + T('p', 'r14', 0, 557, 17, 22, 'ROLE2', w=240, center=cx))
S.append(full(deco() + circ14 + q14) + title('TTL14') + tx14)

# 15) النظرة المستقبلية: شريط منحنٍ ودوائر
road15 = ('<path fill="url(#gRoad)" d="M392,720 C430,560 520,450 640,390 C760,330 880,270 1000,210 '
          'C1080,170 1160,145 1280,128 L1280,138 C1170,150 1080,185 1010,228 C900,295 800,360 700,455 C620,540 570,620 560,720 Z"/>')
circ15 = ''
for cx, cy in [(512, 532), (777, 330), (1072, 160)]:
    circ15 += ('<div class="c15" style="top:%dpx;inset-inline-start:%dpx"></div>' % (cy - 141, cx - 141)
               + T('h3', 'h13', 0, cy - 34, 21, 27, 'HH', w=220, center=cx - 6)
               + T('p', 'body', 0, cy + 16, 18, 22.5, 'IT7', w=190, center=cx - 6))
S.append(full(deco() + road15) + title('TTL15') + T('p', 'body', 111, 311, 23, 29, 'P15', w=390) + circ15)

# 16) كلمة الرئيس التنفيذي
S.append(full('<rect width="1280" height="720" fill="#fff"/>' + C(-827, 360, 1747, '#1B4865') + deco(True)
              + RING(1170, 703, 250, 180, 'url(#gArc)') + C(995, 398, 38, 'url(#gB)')
              + '<path fill="url(#gQ)" transform="translate(640 133)" d="%s"/><path fill="url(#gQ)" transform="translate(731 133)" d="%s"/>' % (QSHAPE, QSHAPE))
         + '<img class="ceo" src="%sceo.png" alt="§ALT16§">' % A
         + T('h1', 'ttl wt', 111, 193, TTL_F, TTL_L, 'TTL16A') + T('h1', 'ttl tl', 111, 263, TTL_F, TTL_L, 'TTL16B')
         + T('p', 'wt', 111, 357, 28, 38, 'P16A', w=420) + T('p', 'wt', 111, 531, 28, 38, 'P16B', w=420)
         + T('h3', 'nm16', 1003, 219, 24, 32, 'NAME16', w=180) + T('p', 'body', 1003, 289, 20, 26, 'ROLE16', w=240))

# 17) شكرًا وتواصل
con17 = ''
for cx, cy, ic, x, b, tk in [(838, 233, 'mail', 951, 235, 'EM'), (884, 349, 'phone', 955, 350, 'CT'), (904, 460, 'globe', 955, 468, 'WB')]:
    con17 += ('<div class="ci17" style="top:%dpx;inset-inline-start:%dpx">%s</div>' % (cy - 35, cx - 35, icon(ic))
              + T('h3', 'h17', x, b, 21, 27, tk) + T('p', 'body', x, b + 36, 19, 24, 'ITY'))
con17 += '<i class="rl" style="top:303px;inset-inline-start:950px;width:197px"></i><i class="rl" style="top:420px;inset-inline-start:950px;width:197px"></i>'
S.append(full('<circle cx="261" cy="535" r="651" fill="url(#gCov2)"/>' + C(277, 375, 513, '#1B4865') + deco(True))
         + T('h1', 'ttl wt', 111, 196, TTL_F, TTL_L, 'TTL17A') + T('h1', 'ttl tl', 111, 268, TTL_F, TTL_L, 'TTL17B')
         + T('p', 'wt', 111, 333, 23, 29, 'P4', w=560)
         + logo('lg17') + '<p class="t lgt" style="top:501px;inset-inline-start:203px">§LOGO§</p>' + con17)

SLIDES = S

STYLE = """  .full { position: absolute; inset: 0; width: 1280px; height: 720px; }
  html[dir="rtl"] .full { transform: scaleX(-1); }
  .t { position: absolute; margin: 0; }
  .ttl { font-weight: 700; color: var(--navy); letter-spacing: -0.3px; white-space: nowrap; }
  .ttl.wt { color: #fff; } .ttl.tl { color: var(--teal); }
  .body { color: var(--ink); font-weight: 400; }
  .wt { color: #fff; font-weight: 400; }
  .kpi { color: var(--navy); font-weight: 700; letter-spacing: -0.5px; white-space: nowrap; direction: ltr; unicode-bidi: isolate; }
  html[dir="rtl"] .kpi { text-align: right; }
  .kpi sup, .num5 sup, .num10 sup, .kc10 sup { font-size: .5em; vertical-align: top; position: relative; top: -.05em; margin-inline-end: 1px; }
  .lgt { color: #fff; font-size: 15px; line-height: 19.5px; letter-spacing: 2.2px; text-transform: uppercase; }
  html[lang="ar"] .lgt { letter-spacing: 0; font-size: 16px; }
  .logo { position: absolute; width: 58px; height: 63px; }
  .lg1 { top: 145px; inset-inline-start: 115px; color: #5AB4CC; --gap: #1B4865; }
  .lg17 { top: 489px; inset-inline-start: 113px; color: #fff; --gap: #1B4865; }
  .ico { width: 100%; height: 100%; display: block; }
  /* 1 */
  .c-a { color: #5CB3CB; font-weight: 400; white-space: nowrap; }
  .c-b { color: #5CB3CB; font-weight: 700; white-space: nowrap; letter-spacing: -0.5px; }
  .c-c { color: #fff; letter-spacing: 4.6px; white-space: nowrap; }
  html[lang="ar"] .c-c { letter-spacing: 1px; }
  /* 2 */
  .tocn { color: var(--num); font-weight: 300; }
  .pill { position: absolute; width: 149px; height: 21px; border-radius: 11px; background: var(--navy); }
  .hl { position: absolute; height: 1px; background: var(--navy); }
  /* 3 */
  .year { color: #CFCFCF; letter-spacing: 3px; font-weight: 400; }
  /* 4 */
  .card4 { position: absolute; width: 454px; height: 227px; border-radius: 36px; }
  .on4 { color: #fff; font-weight: 400; }
  .big4 { color: #fff; font-weight: 700; }
  .bar4 { position: absolute; width: 60px; height: 8px; border-radius: 4px; }
  /* 5 */
  .map { position: absolute; top: 15px; inset-inline-start: 540px; width: 740px; height: 510px; }
  .map .dots { fill: #D3D3D3; }
  .card5 { position: absolute; width: 249px; height: 233px; border-radius: 28px; }
  .num5 { color: #fff; font-weight: 700; white-space: nowrap; direction: ltr; unicode-bidi: isolate; }
  /* 6 */
  .ph6 { position: absolute; width: 205px; height: 205px; border-radius: 50%; object-fit: cover; }
  .nm6 { color: var(--navy); font-weight: 700; }
  .rl6 { color: var(--teal); }
  .dot6 { position: absolute; width: 15px; height: 15px; border-radius: 50%; background: var(--navy); }
  .cu6a { color: #fff; font-weight: 700; } .cu6b { color: var(--teal); font-weight: 700; }
  /* 7 */
  .pill7 { position: absolute; width: 257px; border-radius: 129px; }
  .h7 { font-weight: 700; } .h7.w, .b7.w { color: #fff; } .h7.k, .b7.k { color: #111; }
  /* 8 */
  .box8 { position: absolute; width: 258px; height: 99px; border: 2.5px solid #0E2233; border-radius: 13px; background: #fff; box-shadow: 4px 6px 8px rgb(0 0 0 / .18); }
  /* 9 */
  .ic9 { position: absolute; width: 50px; height: 50px; }
  .h9 { color: #111; font-weight: 700; }
  /* 10 */
  .card10 { position: absolute; inset-inline-start: 108px; width: 503px; height: 139px; border-radius: 20px; }
  html[dir="rtl"] .card10[style*="94C2FF"] { background: linear-gradient(270deg,#5FA8D6 20%,#94C2FF) !important; }
  .h10 { color: #fff; font-weight: 700; }
  .num10 { color: #fff; font-weight: 700; white-space: nowrap; direction: ltr; unicode-bidi: isolate; }
  html[dir="rtl"] .num10 { text-align: right; }
  .pct { font-weight: 700; } .pct.pb { color: var(--blue); } .pct.w { color: #fff; }
  .kc10 { color: var(--blue); font-weight: 700; direction: ltr; unicode-bidi: isolate; }
  /* 11 */
  .h11 { color: var(--navy); font-weight: 700; }
  .vl { position: absolute; width: 1px; height: 295px; background: var(--rule); }
  .rl { position: absolute; height: 1px; background: var(--rule); }
  .ic11 { position: absolute; width: 30px; height: 32px; color: var(--teal); }
  .band11 { position: absolute; top: 551px; inset-inline-start: 108px; width: 1106px; height: 104px; border-radius: 20px; background: var(--blue); }
  .n11 { color: #C3EBF0; font-weight: 300; }
  /* 12 */
  .k12 { font-size: 48px; }
  .kc12 { text-align: center !important; }
  /* 13 */
  .c13 { position: absolute; width: 180px; height: 180px; border-radius: 50%; background: #fff; border: 3px solid #BDE5E7; box-shadow: 0 0 0 15px #fff, 0 10px 22px 14px rgb(0 0 0 / .07); }
  .h13 { color: var(--navy); font-weight: 700; }
  /* 14 */
  .stars { position: absolute; display: flex; gap: 7px; } .stars svg { width: 27px; height: 27px; }
  .n14 { color: #fff; font-weight: 700; }
  .r14 { color: #EAF4F8; }
  /* 15 */
  .c15 { position: absolute; width: 282px; height: 282px; border-radius: 50%; border: 4px solid transparent; background: linear-gradient(#fff, #fff) padding-box, linear-gradient(#EAF3FB, #6EAEDA) border-box; box-shadow: 0 6px 14px rgb(30 70 110 / .14); }
  /* 16 */
  .ceo { position: absolute; top: 150px; inset-inline-start: 585px; width: 527px; height: 570px; }
  .nm16 { color: var(--navy); font-weight: 700; }
  /* 17 */
  .ci17 { position: absolute; width: 70px; height: 70px; border-radius: 50%; background: #fff; color: var(--blue); padding: 18px; }
  .h17 { color: #111; font-weight: 700; }
  html[lang="ar"] .ttl, html[lang="ar"] .c-b, html[lang="ar"] .kpi { letter-spacing: 0; }"""

EXTRA_SCRIPT = r"""  // ----- الخريطة النقطية: كل نقطة 4 خانات بأساس 36 (x ثم y نسبةً إلى زاوية الخريطة) -----
  (function () {
    var s = '7d0a7m0b730d6u0f7v0g7a0h7k0i930i8u0l720m7s0meq0l6e0n6v0n7g0o780r860p8q0r920q9a0q9k0r8i0sev0r5s0t6c0u6l0t7l0u7x0udb0uf30u570w6w0w8y0wfd0whp0w7a0x840y8c0x9a0yhj0x4s10720z8r0z60126b126q117o128j1193129j114j136i137w149s13fl1489158w169b176u1881188g184q1b7o1a8o1afu19g41a6o1b7f1c931b9i1c9p1bkb1bki1b551e5t1d6f1d7v1e851fbk1e1u1g5h1g8d1f8w1f9b1ffm1ffv1hga1hii1f681h6l1j781h7l1i8l1ibd1hg31jj91h1l1j201k461j941k9k1kfe1jia1jir1jkd1j601l7e1m7r1m811l8c1m8s1m9d1nb71lfp1nj01lje1mjs1l1e1o421ofz1ph71nhe1oij1njz1ok71o151p1s1p261q6e1q771q8l1r8y1qbk1qgk1ris1rj51qjm1p1k1s7i1s7v1sfa1rfj1rg91sgu1shy1ti61rid1skg1s1y1t3x1t481u641t821u891t951t9k1ubb1ujc1u161w2d1v4g1w7o1w8s1w9c1wf41vfq1vfy1wh21vha1vhk1win1wiy1vjk1wjt1wk11v1e1y1o1y241y8g1xgj1ygr1yhw1zhw24k91xkj1y2i1z4r207w209120fh1zg420i61zif20j51z1w2245215i2285238m219h23fa21gc22gy22hf21iu23172429232n244d228c258v25fn23fw24h724jj23js23k223kb24kb290t251h261p262x2639264n2464267p259825dj26f526ho25ib26im25j226jb2522287y27ey28g428gj28gq27h128102a2f292p2b332a5c295s2a8g2a8o29932a9h2bfh29gc2ahg29i429ir2bjn2ajw2a0q2b1u2cfa2bfq2bhu2cih2c182d1g2d3g2d3o2d612e7q2d7z2e882e8v2dfy2dh92dhn2eiy2ej72djg2dk32d1n2f222g292f2j2g392g692f6g2f9d2gdh2gfk2hg72gi92gke2g0z2i2t2h302i5r2i8f2i8n2i942iev2hf32ii12hio2ijo2ijx2i4c2j5d2j8x2kfb2jfs2kgf2jhg2jhu2kj32kjd2k1a2l1i2m1r2m1z2m3f2m3m2m5y2l7n2m7u2l832lec2lg02lgu2nih2liv2mk72l0q2o132o2c2n2k2n2t2o372o6e2o6m2p8b2ofi2ohm2njl2pkf2p452q522q8k2q8s2qdi2pes2pf22rg72pi32pib2qk02p242r2z2s5b2s5v2t842s912seh2rfa2sfq2rgy2qha2shs2tip2rj62rjr2r0x2t1e2t1n2u1v2u3h2u4e2s7v2tfz2uge2uhh2tiz2uje2tk82u0n2w0n30162w292x2h2w2o2v392w6g2w9a2wew2whz2vih2vjx2wki2w5g2x6o2x8e2x8m2y8w2yf92yfi2xg62yi82yjj2y0c300y312v30332z51315r3080308731ek2zfq2zgk2zhc30hp30it2zj9301d311k331s3220313m314t3259329433e033f232fx32gd32hx33im32jq32k53128342g352o343f335n347t348r35c733hj33i834j133jy340s36153537366i358h35c035ee36fe36fm36h637ig35jh370j382y384u385e376q3782398z38bo37ce39dg38e538g237gk37hr38i239ir38j9371c3a1u3a223a3o3952397m398b39bv3acl3adr3bel39ey3bf739fr3bg939hf3ahf3ghf3miy3bk43b0z3b1n3b2c3b2q3b3g3b7u3c8m3bbh3cc63cid3djo3cjv3b0p3e303e393f3u3d583e5i3e6d3d6l3d6w3dcr3ed63edz3dea3efc3ffy3egg3egr3ehx3fil3ej43e2j3f4t3g803h8g3gde3gdm3gei3ges3gfk3fh63gho3fi63gjh3f1x3j253i2s3i3k3i503j643h7k3h883ibm3hbv3ic43jcd3if33hg63hja3h2c3l3r3l4m3k7s3kdv3ke53jfu3kgl3kgy3kiw3j333l6d3m6m3mcl3led3mfn3ngc3mhq3nif3lip3l2k3o2v3p2v3v3e3n3x3pbd3nbq3od63odd3odm3oen3ney3nf73nfe3og33oh73ohy3ni63oj43ojd3o0e3p223q373r4t3q7l3q833p9b3q9m3pbj3qc33qcc3qe43qgt3q2c3s3o3s4j3s6i3t7u3scw3sdt3seu3sgm3shl3sih3sir3siy3t433v5x3t9g3ucj3uef3uf43ufj3vfr3vfy3uge3th03thd3v2n3vbo3xdi3vdz3xen3xg73whs3wi03wi83wjb3v2d3y3c3x3j3x3u3x4l3z633z7o3y7y3yb73xbf3yc13yca3ycp3yd33xda3ydo3ze73xfc3yh63yik3yj43y343zev40gk40gt3ziu412p4243425w43cw41ec43f442f448fk42fz42hm41ic412g442w433t454c447u45bc44bx43cg43de44dt44e244ek45ek4afs44gd43gz44hf44hu45i244ja44244537473e463m454l476645ay46c646d646g645gq46h746im46j347b547bj48cn47dk48eu48fg48i849iu482b4a2o492y4a454a5s4a6k4acg4bct4ae64afy4agi4bhn4aig4a3w4b4d4c604cc04cd04bdr4cdy4cfa4dfp4cg94cgy4chg4chw4cjb4cjk4d354f3l4e4l4d664ebb4ddd4ded4eez4fh74diq4ej14e2g4f2q4g3e4f4u4gb24fbk4gd54feq4fgp4fi44g2y4j3r4i404h6d4ice4icm4hdm4if44ifh4hfz4hgh4ihq4iib4h474j5u4j6l4jc64kcu4je04ke74jfq4jg84kgx4kh64khh4kii4jj64kje4j2k4m394m4g4l4o4l614lbb4mdf4mds4mel4let4nhz4mio4miw4mjn4lju4l2c4n3j4n524n5n4ocz4od74nee4nfa4nfk4og24pgp4n2q4p324p3s4q404q4a4r4u4p674p6g4q6r4rbh4qby4pcg4qco4qf04qft4qge4phd4qho4qi74pj54r4l4t5a4sc74sdl4sdw4sgx4th54rhv4sif4rjg4r294u2u4v3d4t5t4u604udc4ue34tea4uek4ufd4ugk4uit4ujn4v2j4v344w3m4v514w5l4w6j4wb64vcu4vd44wer4vf54wfm4wfz4wga4vhj4wi44wim4wj04wja4xju4w3u4z424y4a4y4s4x694xa74xbx4zcl4xdr4ygs4xic4y4h4z6v50bp50c550cc4zdj50ey50fs50h54zhd50hw502u523a523j52585160526o52ah51bc52cy52db52dy52e752eg52ff52g551gg52hp51jh512m5332544o534y545i535r5368546g53a153b455bj55cq54en53f754gp54gy54ik53is54j154j955jp543s564656ch56d557dn56ev56fm56fy56h757i355ia563z584f585758ad58aw58bs58c157g858hg582y5a3b593j595e5a5p5a5y5aam59c959cx5add5adv59e459ee59f45afe5agh59ho59hw59jj5b2c5b2j5c2r5b4l5c4s5b4z5c6u5bba5bbk5ccp5dep5cft5bgr5bgz5cif5bis5cj05bjc5bjr5c3o5f485e665eb25dby5ecg5ed45fdj5eex5fg35eha5ei65eil5f355f3w5g5j5gas5gdr5ge75gej5ffm5fge5ghh5ghs5gj65g2x5j3d5h4g5h565h5r5h705hah5jbd5ibr5hc75hda5idz5hf65ife5ifv5jgl5i2g5j2p5l435j4x5k5y5kck5kct5lcy5lee5kg85kgt5jh55ji05jic5jiy5jjl5j285m3j5m4q5l5f5laz5lb75lbl5mc05ncc5ndh5mep5mex5mfm5nho5miq5mjd5l345o3x5o4b5o565p565u5n5p645o6b5oas5odp5ng15ngz5nhd5nij5o2v5q3b5q3p5q4j5pal5pbs5qd65qe75pf45qfe5qgh5qhy5qi75pj55p2i5r4w5s5u5sbg5rcg5tej5rg95sgp5sh75thj5six5s2a5t3h5v455taz5ub65tc75tdc5udj5vet5ufq5tfy5ugy5vhr5uic5ujc5t2o5v3v5w4q5w5i5vby5vec5vf15xfi5wir5w2y5x385y4b5x4i5yar5xbp5yfa5xg95ygj5yhe5yi25xik5yj35x265z3o6042605c5z5o60ak5zbe60do5zeg61eo5zgu60h560ka5z2g612q623f624v625261a461c461cc61fu62g261hm62hu61ia61jb622z63ac63bx65cx63dg64e964ey64f764fn63gf64296636663u654c664l6557665g66ff66go65he66i365ig66io652k683m684367a568al67ch68cq69d869do68ek68es67gy67h6692w6a3f6a4r6ac36adv6af56ag169g969ho6ahv6aix6ajg6aka692e6c4i6c506bd06ced6cfl6cfs6bgh6cgp6cia6c2p6e3u6e426f596edh6dhi6di16eii6ek46d316g386f3m6g4a6fci6gcq6gd96geq6fez6ffd6fgx6fha6fip6g4n6jdr6hej6ify6ig56igd6ihq6j2g6j2u6l3f6k4u6j516k586lc66kdk6ke16kf66jfp6lgs6kh46jhh6ki76j436m4d6mea6met6mfh6lgk6lhy6mig6liv6l2m6n376o3m6o3u6naw6pb06pdc6ndu6ojt6o2y6q4j6rb76qd66qeg6qfv6qg36qgc6rgz6phc6qhn6qim6q3f6s3z6t3z6z4y6sdm6se26sen6sey6rf66sfd6ri66riv6sj46s2j6u4a6t4r6tfm6tgj6ugs6th56thy6tid6t2s6w336w3n6wde6wdv6wea6w396y4k6yas6zb26ybc6xd46xet6yfv6xg56xhj6yhq6x4870bm70dp70e270eh6zf16zfa6zfi70gd70gx6zhc70hx71i570ij70iq6z2y723h733r724u72a071c271c971ci73cu72db73go71h573ic72ix72j272b874bg74di74eo73fr74fz743976ao77ax75bv76d476eb76f176fb76gx763n789y78cn77dq78dx78fl78g677ge77gm77hi77hq77hy78ik783279b27abo7ac57acd79cv7aej79eu7aib7ais793g7dbg7bdg7bf47cg07cgt7dh47bhc7bi37c387f387la07eaj7eat7eb87fbz7dd87dfd7dft7ega7fgk7fj07dbs7gci7fcq7gdn7gdv7ge37gfk7ghk7fhs7gij7g3m7i9t7ibj7jcb7icy7ih27iia7iir7ia57kay7jc47kde7kei7keq7lg27kgw7khe7khz7j9k7mad7lap7lbb7le87nga7ngh7m3g7nb37obv7ncn7odr7ofn7nfu7ogp7nh77nhl7oi57oih7o9q7qa07pak7qbn7qc97qcv7qdi7qdz7phs7q387s3m7s587sau7sbe7sch7rd37ree7sg07s4b7u9j7ua87tb47vbz7ten7tg77vgl7uhd7uhy7t3e7x9u7vaf7vcq7wdm7vdw7xe57vge7xhm7w5n7xbj7xbs7yc87ycz7y3l804880a27zal80as7zaz81b880ci7zd780ef7zfw7zhs7zi2803t819k829r82aa82c181dq8300844083bf84cb85cs83e084e883g283gb83hg83bs85ck8648889v88a487af88an88b288d087da87jb884g8a9i89au8aba8abm89bz89c68bcq8bdt89g18adf8cg98cic8d9n8ea78dbh8fcd8ed58e4k8g9x8fae8gan8gb48gbw8fcx8gdn8haw8jbo8jcj8hg68ii78jjj8j5h8la28kbc8kc68jcq8jdh8k5p8l9r8lby8md28mda8m5b8naa8nak8nb68pce8pdv8ne38p4r8p5v8p9w8qay8qbg8qbo8qco8qgc8q618t6a8saq8rc68rcx8rdi8sdq8s558t5m8ta88uai8ubx8ud58u5d8va18vb28wck8wdz8v5t8z6e8ybb8xbs8ycb8xct8ydc8x658z6q8zbi8zc290d191do8zix8z5k916k93dv92hm9156935d945z946x95ca94ci94di94i094bc97bp95bx96cs95db955r986797d398it97j198519a5k9a6e996s9abj9adm9ahu9aim9a5a9c5z9c6m9cc39ccb9ccl9bcu9cbr9dd99d4y9h5r9g679g729gbc9gdg9ghz9gix9f5g9h6g9h6v9h7a9ibx9ich9hd19hiq9ik69h579j5z9kbi9kc69kct9j5k9m7g9n7o9mbq9mda9m4v9o5t9p669o729oce9ncn9nd19pi59oke9n5c9q529r7a9s7t9sbk9sbx9rc59r5i9w5m9w6m9t7k9ucj9uct9tda9t629v6a9w739w809wca9wij9w5a9y5v9xbr9xd09x53a07d9z7pa0c09zcma1d7a05ra266a27wa2cca2cta276a4bma3dfa45ga65za67ia5bta7c2a6cza658a75na97ra9cia868aa6ya97baac9aacrabd7a9black1abkbab5dae5tae62af73ae7kaebvaec3afcjafczaedfaee2ae6cah6uag5kah7bai7rahcbaid7aik6ai5walbiakbrakctakjtak65am6xam7hamckamdxaljzankcam5pao6oao75an7papc2and1an6capbuarc9apjgapjnaqkiar7casblarcgasdtatdyatk4as5vat62at6iau6uatcpatcwatjtaukbav72av7navjbaw6aay6oaz7faybxaxc5axjjay5qazboazcdb0clb0j3b0jzb0kiaz61b16hb26wb278b1cub2dtb3iwb1jrb1kcb2c0b4jab3k4b35sb668b6bqb6c8b6jlb56pb974b7cib8crb7isb760ba6gb96xbajebajtbak0bakcbabzbcizbbj6bb5sbdbsbdcabdcpbejlbekibd67bf6fbg73bgk7bf60bi6mbi6tbhirbhjcbi5qbkc4bjcdbjclbkixbljsbkk0bkkfbkbwblj4bljjbm6gbo6ybnk7bo5ybp67bpjcbr5qbs6pbrc5brcdbritbsj2btjsbsbxbtjjbtk0btkcbt62bw6fbv6ubv5vbziybzj8byk4bzkhby68c16mc0irc15oc56fc4k9c461c5kjc56lc85vcakcca6acb5pce62cdkgci5yck5pcm5wcr65cs5qcw60cy5rd361d55qdb5zdd5tdj62dm5yds68dw', g = document.querySelector('#map5 .dots'), out = '';
    for (var i = 0; i + 3 < s.length; i += 4) {
      var x = parseInt(s.substr(i, 2), 36), y = parseInt(s.substr(i + 2, 2), 36);
      out += '<circle cx="' + x + '" cy="' + y + '" r="2.8"/>';
    }
    g.innerHTML = out;
  })();
  // ----- الحلقات البيانية: من كتل chart-data* (الزاوية تبدأ من الأعلى باتجاه عقارب الساعة في الإحداثيات الإنجليزية؛ الطبقة كلها تنعكس في العربية) -----
  function donut(gid, blockId) {
    var d = JSON.parse(document.getElementById(blockId).textContent), g = document.getElementById(gid);
    var tot = d.series.reduce(function (a, s) { return a + s.value; }, 0), a = 0, h = '';
    function pt(r, ang) { var t = ang * Math.PI / 180; return [d.cx + r * Math.sin(t), d.cy - r * Math.cos(t)]; }
    d.series.forEach(function (s) {
      var a1 = a + s.value / tot * 360, mid = (a + a1) / 2, ex = s.explode || 0, t = mid * Math.PI / 180;
      var dx = ex * Math.sin(t), dy = -ex * Math.cos(t), big = (a1 - a) > 180 ? 1 : 0;
      var p0 = pt(d.ro, a), p1 = pt(d.ro, a1), p2 = pt(d.ri, a1), p3 = pt(d.ri, a);
      h += '<path fill="' + s.color + '" transform="translate(' + dx.toFixed(1) + ' ' + dy.toFixed(1) + ')" d="M' + p0 + ' A' + d.ro + ',' + d.ro + ' 0 ' + big + ' 1 ' + p1
        + ' L' + p2 + ' A' + d.ri + ',' + d.ri + ' 0 ' + big + ' 0 ' + p3 + ' Z"><title>' + s.label + '</title></path>';
      a = a1;
    });
    g.innerHTML = h;
  }
  donut('donut1', 'chart-data'); donut('donut2', 'chart-data-2');"""

JSON_BLOCKS = {
    'chart-data': {
        'ar': {'cx': 1003, 'cy': 360, 'ro': 327, 'ri': 183, 'unit': '%', 'series': [
            {'label': 'الشريحة الأولى', 'value': 10, 'color': '#BFE9E8'}, {'label': 'الشريحة الثانية', 'value': 8.3, 'color': '#4E97FE'},
            {'label': 'الشريحة الثالثة', 'value': 54.4, 'color': '#1B4865'}, {'label': 'الشريحة الرابعة', 'value': 27.3, 'color': '#5CA9D7'}]},
        'en': {'cx': 1003, 'cy': 360, 'ro': 327, 'ri': 183, 'unit': '%', 'series': [
            {'label': 'Segment 1', 'value': 10, 'color': '#BFE9E8'}, {'label': 'Segment 2', 'value': 8.3, 'color': '#4E97FE'},
            {'label': 'Segment 3', 'value': 54.4, 'color': '#1B4865'}, {'label': 'Segment 4', 'value': 27.3, 'color': '#5CA9D7'}]},
    },
    'chart-data-2': {
        'ar': {'cx': 290, 'cy': 442, 'ro': 201, 'ri': 113, 'unit': '$', 'series': [
            {'label': 'البند الأول', 'value': 58.6, 'color': '#1B4865'}, {'label': 'البند الثاني', 'value': 23, 'color': '#61B6CB'},
            {'label': 'البند الثالث', 'value': 10.6, 'color': '#BFE9E8', 'explode': 16}, {'label': 'البند الرابع', 'value': 7.8, 'color': '#5FA8D3'}]},
        'en': {'cx': 290, 'cy': 442, 'ro': 201, 'ri': 113, 'unit': '$', 'series': [
            {'label': 'Item 1', 'value': 58.6, 'color': '#1B4865'}, {'label': 'Item 2', 'value': 23, 'color': '#61B6CB'},
            {'label': 'Item 3', 'value': 10.6, 'color': '#BFE9E8', 'explode': 16}, {'label': 'Item 4', 'value': 7.8, 'color': '#5FA8D3'}]},
    },
}

STR = {
    'en': {
        'LOGO': 'Your Company<br>Logo Here', 'C1A': 'CIRCLES', 'C1B': 'COMPANY PROFILE', 'C1C': 'PRESENTATION&nbsp; TEMPLATE',
        'TTL2': 'Table of Contents', 'IY2': 'Insert your desired<br>text here.', 'IY1': 'Insert your desired text here.',
        'N1': '01', 'N2': '02', 'N3': '03', 'N4': '04', 'N5': '05', 'N6': '06',
        'TTL3': 'Company History<br>&amp; Milestones', 'P3': 'Insert your desired text here. This is a sample<br>text. A brief statement about your topic.',
        'K3A': '<sup>$</sup>4.5B', 'K3B': '300+', 'YEAR': 'YEAR', 'IT2': 'Insert your desired text here.<br>This is a sample text.',
        'TTL4': 'Our Mission &amp; Vision', 'P4': 'Insert your desired text here. This is a sample text.<br>A brief statement about your topic.',
        'OUR': 'Our', 'VIS': 'Vision', 'OUR2': 'Our', 'MIS': 'Mission',
        'IT4': 'Insert your desired text<br>here. This is a sample<br>text. A brief statement<br>about your topic.',
        'TTL5': 'Our Operations<br>Across the Globe', 'P5': 'Insert your desired text here. This is a sample<br>text. A brief statement about your topic.',
        'K5A': '300', 'K5B': '3.5K', 'IY2s': 'Insert your desired<br>text here.', 'IT5': 'Insert your desired text here.<br>This is a sample text.',
        'TTL6': 'Leadership Team &amp;<br>Our Culture', 'ALT6': 'Team member photo', 'NAME': 'Name Placeholder', 'ROLE': 'Role Placeholder',
        'OUR3': 'Our', 'CUL': 'Culture', 'IT6': 'Insert your desired<br>text here. This is a<br>sample text.',
        'TTL7': 'Our Core Product<br>&amp; Service', 'P7': 'Insert your desired text<br>here. This is a sample<br>text. A brief statement<br>about your topic.',
        'HH': 'Heading Here', 'IT7': 'Insert your desired<br>text here. This is a<br>sample text.',
        'TTL8': 'The Problem<br>We Solve', 'P8': 'Insert your desired text here.<br>This is a sample text. A brief<br>statement about your topic.',
        'IT8': 'Insert your desired text<br>here. This is a sample text.',
        'TTL9': 'The Solution &amp; Unique<br>Value Proposition',
        'TTL10': 'Market Overview<br>&amp; Opportunity', 'K10A': '120', 'K10B': '91',
        'V10A': '14.3%', 'V10B': '24.3%', 'V10C': '44.3%', 'ITH': 'Insert text<br>here', 'K10C': '<sup>$</sup>5.3B', 'IY2c': 'Insert your desired<br>text here.',
        'TTL11': 'Business Model',
        'TTL12': 'Financial Snapshot',
        'TTL13': 'Key Achievements',
        'TTL14': 'What Our Clients Say', 'TEST': 'Insert your client testimonial<br>here. Insert your desired text<br>here. This is a sample text.',
        'NAME2': 'Name Here', 'ROLE2': 'Role/Company',
        'TTL15': 'Future Outlook<br>&amp; Roadmap', 'P15': 'Insert your desired text here. This is<br>a sample text. A brief statement<br>about your topic.',
        'TTL16A': 'Message from', 'TTL16B': 'The CEO', 'ALT16': 'Chief executive officer photo',
        'P16A': 'Insert your desired text here.<br>This is a sample text. A brief<br>statement about your topic.',
        'P16B': 'Insert your desired text here.<br>This is a sample text.', 'NAME16': 'Name<br>Placeholder', 'ROLE16': 'Role Description',
        'TTL17A': 'Thank You', 'TTL17B': 'Connect With Us', 'EM': 'Email', 'CT': 'Contact', 'WB': 'Website', 'ITY': 'Insert your text here',
    },
    'ar': {
        'LOGO': 'ضع شعار<br>شركتك هنا', 'C1A': 'دوائر', 'C1B': 'الملف التعريفي للشركة', 'C1C': 'قالب عرض تقديمي',
        'TTL2': 'جدول المحتويات', 'IY2': 'أدخل النص<br>المطلوب هنا.', 'IY1': 'أدخل النص المطلوب هنا.',
        'N1': '01', 'N2': '02', 'N3': '03', 'N4': '04', 'N5': '05', 'N6': '06',
        'TTL3': 'تاريخ الشركة<br>وأبرز المحطات', 'P3': 'أدخل النص المطلوب هنا. هذا نص توضيحي.<br>عبارة موجزة عن موضوعك.',
        'K3A': '<sup>$</sup>4.5B', 'K3B': '300+', 'YEAR': 'السنة', 'IT2': 'أدخل النص المطلوب هنا.<br>هذا نص توضيحي.',
        'TTL4': 'رسالتنا ورؤيتنا', 'P4': 'أدخل النص المطلوب هنا. هذا نص توضيحي.<br>عبارة موجزة عن موضوعك.',
        'OUR': 'رؤية', 'VIS': 'الشركة', 'OUR2': 'رسالة', 'MIS': 'الشركة',
        'IT4': 'أدخل النص المطلوب هنا.<br>هذا نص توضيحي.<br>عبارة موجزة<br>عن موضوعك.',
        'TTL5': 'عملياتنا<br>حول العالم', 'P5': 'أدخل النص المطلوب هنا. هذا نص توضيحي.<br>عبارة موجزة عن موضوعك.',
        'K5A': '300', 'K5B': '3.5K', 'IY2s': 'أدخل النص<br>المطلوب هنا.', 'IT5': 'أدخل النص المطلوب هنا.<br>هذا نص توضيحي.',
        'TTL6': 'فريق القيادة<br>وثقافتنا', 'ALT6': 'صورة عضو الفريق', 'NAME': 'الاسم الكامل', 'ROLE': 'المسمّى الوظيفي',
        'OUR3': 'ثقافة', 'CUL': 'شركتنا', 'IT6': 'أدخل النص المطلوب<br>هنا. هذا نص<br>توضيحي.',
        'TTL7': 'منتجاتنا وخدماتنا<br>الأساسية', 'P7': 'أدخل النص المطلوب<br>هنا. هذا نص توضيحي.<br>عبارة موجزة<br>عن موضوعك.',
        'HH': 'العنوان هنا', 'IT7': 'أدخل النص المطلوب<br>هنا. هذا نص<br>توضيحي.',
        'TTL8': 'المشكلة التي<br>نحلّها', 'P8': 'أدخل النص المطلوب هنا.<br>هذا نص توضيحي. عبارة<br>موجزة عن موضوعك.',
        'IT8': 'أدخل النص المطلوب هنا.<br>هذا نص توضيحي.',
        'TTL9': 'الحل وعرض<br>القيمة الفريد',
        'TTL10': 'نظرة على السوق<br>والفرص المتاحة', 'K10A': '120', 'K10B': '91',
        'V10A': '14.3%', 'V10B': '24.3%', 'V10C': '44.3%', 'ITH': 'أدخل النص<br>هنا', 'K10C': '<sup>$</sup>5.3B', 'IY2c': 'أدخل النص<br>المطلوب هنا.',
        'TTL11': 'نموذج العمل',
        'TTL12': 'لمحة مالية',
        'TTL13': 'أبرز الإنجازات',
        'TTL14': 'ماذا يقول عملاؤنا', 'TEST': 'أدخل شهادة عميلك هنا.<br>أدخل النص المطلوب هنا.<br>هذا نص توضيحي.',
        'NAME2': 'الاسم هنا', 'ROLE2': 'المنصب/الشركة',
        'TTL15': 'النظرة المستقبلية<br>وخارطة الطريق', 'P15': 'أدخل النص المطلوب هنا. هذا نص<br>توضيحي. عبارة موجزة<br>عن موضوعك.',
        'TTL16A': 'كلمة', 'TTL16B': 'الرئيس التنفيذي', 'ALT16': 'صورة الرئيس التنفيذي',
        'P16A': 'أدخل النص المطلوب هنا.<br>هذا نص توضيحي. عبارة<br>موجزة عن موضوعك.',
        'P16B': 'أدخل النص المطلوب هنا.<br>هذا نص توضيحي.', 'NAME16': 'الاسم<br>الكامل', 'ROLE16': 'وصف المنصب',
        'TTL17A': 'شكرًا لكم', 'TTL17B': 'تواصلوا معنا', 'EM': 'البريد الإلكتروني', 'CT': 'الهاتف', 'WB': 'الموقع الإلكتروني', 'ITY': 'أدخل النص هنا',
    },
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'general', 'icon': '🔵',
    'name': 'الملف التعريفي للشركة — دوائر', 'nameEn': 'Circles Company Profile',
    'description': 'سبع عشرة شريحة بالدوائر المتدرّجة: غلاف، فهرس، تاريخ، رؤية، عمليات، فريق، منتجات، سوق، نموذج عمل، مالية، تواصل',
    'descriptionEn': '17 gradient-circle slides: cover, contents, history, vision, operations, team, products, market, finance, contact',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'جدول المحتويات', 'titleEn': 'Table of Contents'},
        {'title': 'تاريخ الشركة وأبرز المحطات', 'titleEn': 'Company History & Milestones', 'needsData': True},
        {'title': 'الرسالة والرؤية', 'titleEn': 'Mission & Vision'},
        {'title': 'عملياتنا حول العالم', 'titleEn': 'Operations Across the Globe', 'needsData': True},
        {'title': 'فريق القيادة والثقافة', 'titleEn': 'Leadership Team & Culture'},
        {'title': 'المنتجات والخدمات الأساسية', 'titleEn': 'Core Product & Service'},
        {'title': 'المشكلة التي نحلّها', 'titleEn': 'The Problem We Solve'},
        {'title': 'الحل وعرض القيمة', 'titleEn': 'Solution & Value Proposition'},
        {'title': 'نظرة على السوق والفرص (حلقة بيانات)', 'titleEn': 'Market Overview & Opportunity (donut)', 'needsData': True},
        {'title': 'نموذج العمل', 'titleEn': 'Business Model'},
        {'title': 'لمحة مالية (حلقة بيانات)', 'titleEn': 'Financial Snapshot (donut)', 'needsData': True},
        {'title': 'أبرز الإنجازات', 'titleEn': 'Key Achievements'},
        {'title': 'آراء العملاء', 'titleEn': 'What Our Clients Say'},
        {'title': 'النظرة المستقبلية وخارطة الطريق', 'titleEn': 'Future Outlook & Roadmap'},
        {'title': 'كلمة الرئيس التنفيذي', 'titleEn': 'Message from the CEO'},
        {'title': 'شكرًا وتواصل معنا', 'titleEn': 'Thank You & Contact'},
    ],
    'brief': ('صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والزخارف وغيّر النصوص والأرقام فقط؛ '
              'نسب حلقتي البيانات في كتلتي JSON بالمعرّفين chart-data (السوق) وchart-data-2 (اللمحة المالية)، والنِّسب الظاهرة نصوص HTML بجوارهما.'),
}
