# -*- coding: utf-8 -*-
# نموذج «هندسة جمالية»: 12 شريحة بزخارف هندسية (أقواس، دوائر، درجات، أغصان) بألوان كحلي ومرجاني وبيج ووردي،
# مع صور فوتوغرافية رمادية. الزخارف تُرسم SVG مضمّنًا بدالة واحدة وتنعكس للعربية بـ scaleX(-1).
KEY = 'sl-aesthetic-geometry'
TITLE = {'ar': 'هندسة جمالية', 'en': 'Aesthetic Geometry'}
COMMENT = 'نموذج «هندسة جمالية»: غلاف وجدول أعمال وتعريف ورسالة ورؤية وخدمات وفريق ورئيس تنفيذي ومحطات (شريحتان) وجوائز وخاتمة؛ زخارف هندسية SVG مطلقة الإحداثيات، وصور في assets.'
VARS = '--ink: #0B2644; --ink-2: #474747; --accent: #E57E6B; --bg: #FFFFFF; --navy: #0A243F; --steel: #284361; --slate: #64849D; --pink: #FAE5E2; --beige: #EFE2CF; --blush: #FBE9E7; --line: #22395A;'

NAVY, CORAL, SLATE, STEEL, PINK, BEIGE, BLUSH = '#0A243F', '#E57E6B', '#64849D', '#284361', '#FAE5E2', '#EFE2CF', '#FBE9E7'
LEAF = '#F8D5CE'  # لون الغصن فوق المرجاني


# ---------- دوال رسم الزخارف (إحداثيات الإطار 1280×720) ----------
def rect(x, y, w, h, fill):
    return '<rect x="%d" y="%d" width="%d" height="%d" fill="%s"/>' % (x, y, w, h, fill)


def circ(cx, cy, r, fill):
    return '<circle cx="%d" cy="%d" r="%d" fill="%s"/>' % (cx, cy, r, fill)


def arch(x, y, w, h, fill):  # مستطيل بقمة نصف دائرية
    r = w / 2
    return '<path d="M%d,%d V%.1f A%.1f,%.1f 0 0 1 %d,%.1f V%d Z" fill="%s"/>' % (x, y + h, y + r, r, r, x + w, y + r, y + h, fill)


def arch_o(x, y, w, h, stroke):  # قوس مخطط (بلا تعبئة)
    r = w / 2
    return '<path d="M%d,%d V%.1f A%.1f,%.1f 0 0 1 %d,%.1f V%d" fill="none" stroke="%s" stroke-width="1.4"/>' % (x, y + h, y + r, r, r, x + w, y + r, y + h, stroke)


def arch_dn(x, y, w, h, fill):  # مستطيل بقاعدة نصف دائرية
    r = w / 2
    return '<path d="M%d,%d V%.1f A%.1f,%.1f 0 0 0 %d,%.1f V%d Z" fill="%s"/>' % (x, y, y + h - r, r, r, x + w, y + h - r, y, fill)


def bowl(cx, cy, r, fill):  # نصف دائرة سفلي
    return '<path d="M%d,%d A%d,%d 0 0 0 %d,%d Z" fill="%s"/>' % (cx - r, cy, r, r, cx + r, cy, fill)


def dome(cx, cy, r, fill):  # نصف دائرة علوي
    return '<path d="M%d,%d A%d,%d 0 0 1 %d,%d Z" fill="%s"/>' % (cx - r, cy, r, r, cx + r, cy, fill)


def dots(x, y, cols, rows, gap, fill, r=5):
    return ''.join('<circle cx="%d" cy="%d" r="%d" fill="%s"/>' % (x + c * gap, y + rr * gap, r, fill) for rr in range(rows) for c in range(cols))


def rings(cx, cy, r0, n, step, stroke, ybot, sw=1.2):  # أقواس متداخلة مخططة بجوانب عمودية
    return ''.join('<path d="M%.1f,%d V%d A%d,%d 0 0 1 %.1f,%d V%d" fill="none" stroke="%s" stroke-width="%.1f"/>' % (cx - r, ybot, cy, r, r, cx + r, cy, ybot, stroke, sw) for r in [r0 - i * step for i in range(n)])


def rings_u(cx, cy, r0, n, step, stroke, sw=1.2):  # خطوط عمودية من الأعلى تنحني إلى اليمين
    return ''.join('<path d="M%.1f,0 V%d A%d,%d 0 0 0 %d,%d H1280" fill="none" stroke="%s" stroke-width="%.1f"/>' % (cx - r, cy, r, r, cx, cy + r, stroke, sw) for r in [r0 - i * step for i in range(n)])


def rings_q(cx, cy, r0, n, step, stroke, sw=1.2):  # أرباع أقواس متداخلة من اليسار إلى الأعلى
    return ''.join('<path d="M%.1f,%d A%d,%d 0 0 1 %d,%.1f" fill="none" stroke="%s" stroke-width="%.1f"/>' % (cx - r, cy, r, r, cx, cy - r, stroke, sw) for r in [r0 - i * step for i in range(n)])


def corner_lines(stroke):  # شريحة المحطات: خطوط من الأعلى تنعطف أفقيًا نحو اليمين
    out = ''
    for k in range(5):
        x, yh, r = 725 + 12 * k, 95 - 12 * k, 60 - 12 * k
        out += '<path d="M%d,0 V%d A%d,%d 0 0 0 %d,%d H1280" fill="none" stroke="%s" stroke-width="1.2"/>' % (x, yh - r, r, r, x + r, yh, stroke)
    return out


def quarter(x, y, w, h, r, fill):  # مستطيل بزاوية علوية-بدائية مدوّرة
    return '<path d="M%d,%d V%d A%d,%d 0 0 1 %d,%d H%d V%d Z" fill="%s"/>' % (x, y + h, y + r, r, r, x + r, y, x + w, y + h, fill)


def stairs_rise(far_x, flat_x, ytop, ybot, plateau_x, n, sh, fill):  # درجات تصعد من الركن البعيد السفلي إلى المسطح العلوي
    sw = (plateau_x - far_x) / n
    pts = [(flat_x, ytop), (flat_x, ybot), (far_x, ybot)]
    for i in range(n):
        pts += [(far_x + i * sw, ybot - (i + 1) * sh), (far_x + (i + 1) * sw, ybot - (i + 1) * sh)]
    pts.append((plateau_x, ytop))
    return '<polygon points="%s" fill="%s"/>' % (' '.join('%.1f,%.1f' % p for p in pts), fill)


def stairs_fall(far_x, flat_x, ytop, ybase, plateau_x, n, sh, fill):  # درجات تهبط من الركن البعيد العلوي
    sw = (plateau_x - far_x) / n
    pts = [(flat_x, ytop), (far_x, ytop)]
    for i in range(n):
        pts += [(far_x + i * sw, ytop + (i + 1) * sh), (far_x + (i + 1) * sw, ytop + (i + 1) * sh)]
    pts += [(plateau_x, ybase), (flat_x, ybase)]
    return '<polygon points="%s" fill="%s"/>' % (' '.join('%.1f,%.1f' % p for p in pts), fill)


def sprig(x, ybase, h, color, sw=1.5):  # غصن بأوراق مخططة
    out = '<path d="M%d,%d Q%d,%.1f %d,%d" fill="none" stroke="%s" stroke-width="%.1f"/>' % (x, ybase, x + 10, ybase - h * 0.55, x + 2, ybase - h, color, sw)
    n = 5
    for i in range(n):
        t = (i + 1) / (n + 1)
        sy = ybase - h * t
        sx = x + 2 * t + 10 * 2 * t * (1 - t)
        L = 30 * (1 - 0.35 * t)
        for side in (-1, 1):
            tx, ty = sx + side * 0.75 * L, sy - 0.85 * L
            out += '<path d="M%.1f,%.1f Q%.1f,%.1f %.1f,%.1f Q%.1f,%.1f %.1f,%.1f" fill="none" stroke="%s" stroke-width="%.1f"/>' % (
                sx, sy, sx + side * 0.05 * L, sy - 0.75 * L, tx, ty, sx + side * 0.7 * L, sy - 0.15 * L, sx, sy, color, sw)
            out += '<path d="M%.1f,%.1f L%.1f,%.1f" fill="none" stroke="%s" stroke-width="%.1f"/>' % (sx, sy, sx + side * 0.5 * L, sy - 0.55 * L, color, sw * 0.7)
    return out


def line(x1, y1, x2, y2, stroke, sw=1.5):
    return '<line x1="%d" y1="%d" x2="%d" y2="%d" stroke="%s" stroke-width="%.1f"/>' % (x1, y1, x2, y2, stroke, sw)


def deco(*parts):
    return '<svg class="deco" viewBox="0 0 1280 720" aria-hidden="true">' + ''.join(parts) + '</svg>'


# ---------- تركيبات الزخارف ----------
ART_COVER = deco(
    dots(685, 42, 3, 4, 30, CORAL),
    arch(1000, 0, 125, 200, SLATE),
    circ(822, 172, 75, CORAL),
    rings(1265, 335, 105, 5, 12, SLATE, 470),
    arch(820, 60, 310, 660, PINK),
    arch(690, 150, 125, 400, SLATE),
    arch(890, 185, 160, 375, NAVY),
    arch_o(905, 235, 60, 335, CORAL),
    arch(805, 290, 90, 240, BEIGE),
    sprig(850, 500, 170, NAVY),
    circ(1145, 165, 45, CORAL),
    rect(1110, 355, 130, 135, BEIGE),
    arch(1130, 395, 110, 95, NAVY),
    bowl(1055, 430, 65, SLATE),
    stairs_fall(750, 660, 440, 720, 830, 3, 30, PINK),
    arch(640, 520, 175, 200, NAVY),
    arch_o(655, 560, 105, 160, CORAL),
    circ(940, 600, 48, SLATE),
    dots(850, 640, 3, 3, 30, NAVY),
    quarter(965, 490, 165, 230, 80, BEIGE),
    stairs_rise(975, 1200, 500, 720, 1075, 4, 55, CORAL),
    sprig(1110, 690, 160, LEAF),
    circ(1330, 700, 180, STEEL),
)

ART_AGENDA = deco(
    rect(942, 0, 338, 720, '#112C4B'),
    rect(1035, 0, 165, 105, BEIGE),
    arch_dn(1052, 0, 130, 105, '#112C4B'),
    circ(1115, 95, 47, CORAL),
    dots(990, 85, 3, 4, 30, CORAL),
    rings(1290, 245, 125, 5, 12, SLATE, 470),
    arch(980, 262, 220, 460, PINK),
    bowl(1180, 355, 57, SLATE),
    quarter(942, 490, 133, 230, 70, BEIGE),
    stairs_rise(1000, 1200, 490, 720, 1075, 4, 45, CORAL),
    sprig(1120, 690, 150, LEAF),
    circ(1340, 650, 200, STEEL),
)

ART_ABOUT = deco(
    rect(0, 0, 640, 720, NAVY),
    rect(80, 0, 165, 105, BEIGE),
    arch_dn(97, 0, 130, 105, NAVY),
    circ(162, 95, 47, CORAL),
    dots(233, 70, 4, 3, 30, CORAL),
    rings(-10, 245, 125, 5, 12, SLATE, 470),
    circ(-60, 650, 200, STEEL),
    arch(100, 262, 200, 460, PINK),
    bowl(95, 322, 57, SLATE),
    rect(100, 490, 300, 230, CORAL),
    arch(235, 150, 165, 570, CORAL),
    stairs_fall(215, 337, 500, 690, 283, 4, 47, BEIGE),
    sprig(165, 700, 170, LEAF),
)

ART_MISSION = deco(
    dots(85, 455, 3, 4, 30, CORAL),
    quarter(115, 490, 285, 230, 125, BEIGE),
    stairs_rise(120, 400, 490, 720, 240, 4, 50, CORAL),
    sprig(270, 700, 170, LEAF),
    rect(746, 185, 534, 535, NAVY),
    rect(1138, 145, 85, 83, CORAL),
    rings(1290, 470, 95, 5, 12, CORAL, 560),
    arch(1125, 440, 135, 280, CORAL),
    bowl(1125, 510, 35, '#16324F'),
    sprig(1185, 700, 160, LEAF),
    arch(1215, 580, 130, 140, BEIGE),
)

ART_VISION = deco(
    rect(338, 185, 367, 535, NAVY),
    rings_q(835, 355, 125, 5, 12, SLATE),
    circ(795, 440, 90, STEEL),
    bowl(887, 445, 45, SLATE),
    arch(705, 500, 85, 220, BEIGE),
    rect(885, 490, 160, 230, CORAL),
    stairs_fall(1060, 1045, 490, 720, 1165, 4, 50, BEIGE),
    sprig(965, 700, 170, LEAF),
    dots(1130, 455, 3, 4, 30, CORAL),
)

ART_SERVICES = deco(
    rect(0, 170, 563, 550, BLUSH),
    circ(471, 168, 46, CORAL),
    rings(-40, 455, 110, 5, 12, SLATE, 455),
    circ(-15, 575, 125, STEEL),
    sprig(35, 660, 150, PINK),
    bowl(110, 560, 57, SLATE),
    dots(82, 412, 3, 4, 30, CORAL),
    line(914, 170, 914, 655, '#D9D9D9', 1),
    line(615, 412, 1215, 412, '#D9D9D9', 1),
)

ART_TEAM = deco(
    rect(345, 73, 935, 647, NAVY),
    circ(497, 72, 46, CORAL),
    rings_u(1290, 195, 135, 6, 12, CORAL),
    arch(0, 420, 105, 300, CORAL),
    sprig(30, 700, 180, LEAF),
    bowl(110, 555, 55, STEEL),
    dots(82, 412, 3, 4, 30, CORAL),
)

ART_CEO = deco(
    rect(820, 0, 460, 720, NAVY),
    rect(0, 216, 359, 431, CORAL),
    rect(780, 553, 84, 83, CORAL),
    dots(680, 47, 4, 3, 30, CORAL),
    '<polygon points="820,135 805,147 820,160" fill="#FFFFFF"/>',
    rings(1290, 470, 95, 5, 12, SLATE, 560),
    arch(1125, 440, 135, 280, CORAL),
    bowl(1125, 510, 35, '#16324F'),
    sprig(1185, 700, 160, LEAF),
    arch(1215, 580, 130, 140, BEIGE),
)

ART_MS1 = deco(
    corner_lines(CORAL),
    circ(880, 87, 46, SLATE),
    line(300, 424, 1080, 424, '#BFBFBF', 1.5),
    arch(1155, 490, 105, 230, CORAL),
    bowl(1155, 548, 30, STEEL),
    dots(1055, 610, 3, 3, 30, CORAL),
    arch(1225, 580, 110, 140, STEEL),
)

ART_MS2 = deco(
    dots(845, 47, 4, 3, 30, CORAL),
    arch_dn(984, 0, 126, 245, BLUSH),
    dome(1108, 148, 55, SLATE),
    line(102, 424, 804, 424, '#BFBFBF', 1.5),
)

ART_AWARDS = deco(
    rect(0, 0, 769, 720, NAVY),
    rings(1290, 470, 95, 5, 12, SLATE, 560),
    arch(1125, 440, 135, 280, CORAL),
    sprig(1185, 700, 160, LEAF),
    arch(1215, 580, 130, 140, BEIGE),
    bowl(1125, 660, 35, STEEL),
    dots(990, 562, 3, 3, 30, CORAL),
)

# ---------- الأيقونات ----------
CHECK = '<span class="ck"><svg viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12.5l4.5 4.5L19 7.5"/></svg></span>'
ICONS = {
    'bulb': '<svg viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="1.2" stroke-linecap="round" aria-hidden="true"><path d="M12 3.2a5.6 5.6 0 0 0-3.3 10.1c.8.6 1.3 1.4 1.3 2.3v.4h4v-.4c0-.9.5-1.7 1.3-2.3A5.6 5.6 0 0 0 12 3.2z"/><path d="M10 18.2h4M10.6 20.3h2.8"/><circle cx="12" cy="9" r="1.5"/><path d="M12 6.4v1M12 10.6v1M9.4 9h1M13.6 9h1"/></svg>',
    'gears': '<svg viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="1.1" stroke-linecap="round" aria-hidden="true"><circle cx="9.5" cy="9.5" r="3.2"/><circle cx="9.5" cy="9.5" r="1.1"/><path d="M12.7 9.5L14.1 9.5M11.8 11.8L12.8 12.8M9.5 12.7L9.5 14.1M7.2 11.8L6.2 12.8M6.3 9.5L4.9 9.5M7.2 7.2L6.2 6.2M9.5 6.3L9.5 4.9M11.8 7.2L12.8 6.2"/><circle cx="16.5" cy="16.5" r="2.3"/><circle cx="16.5" cy="16.5" r=".8"/><path d="M18.8 16.5L20.0 16.5M18.1 18.1L19.0 19.0M16.5 18.8L16.5 20.0M14.9 18.1L14.0 19.0M14.2 16.5L13.0 16.5M14.9 14.9L14.0 14.0M16.5 14.2L16.5 13.0M18.1 14.9L19.0 14.0"/></svg>',
    'server': '<svg viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="1.2" aria-hidden="true"><rect x="4" y="4" width="16" height="4.5" rx="1"/><rect x="4" y="10" width="16" height="4.5" rx="1"/><path d="M7 6.2h.01M7 12.2h.01M12 14.5v3M8 19.5h8M8 17.5v2M16 17.5v2"/></svg>',
    'chart': '<svg viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="1.2" aria-hidden="true"><path d="M4 19h16"/><rect x="6" y="12" width="3" height="7"/><rect x="10.5" y="8" width="3" height="11"/><rect x="15" y="4" width="3" height="15"/></svg>',
    'ribbon': '<svg viewBox="0 0 24 24" fill="none" stroke="#1F3A5A" stroke-width="1.3" aria-hidden="true"><circle cx="12" cy="9" r="5"/><circle cx="12" cy="9" r="2.4"/><path d="M9 13.5 7.5 21l4.5-2.5L16.5 21 15 13.5"/></svg>',
    'trophy': '<svg viewBox="0 0 24 24" fill="none" stroke="#1F3A5A" stroke-width="1.3" aria-hidden="true"><path d="M7 4h10v5a5 5 0 0 1-10 0z"/><path d="M7 6H4.5a2.5 2.5 0 0 0 2.5 4M17 6h2.5a2.5 2.5 0 0 1-2.5 4M12 14v3M8.5 20h7M9.5 17h5v3h-5z"/></svg>',
    'medal': '<svg viewBox="0 0 24 24" fill="none" stroke="#1F3A5A" stroke-width="1.3" aria-hidden="true"><path d="M7 3h10l-3 7H10z M9 3l1 2.5M15 3l-1 2.5"/><circle cx="12" cy="15.5" r="4.5"/><circle cx="12" cy="15.5" r="2"/></svg>',
    'mail': '<svg viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="1.6" aria-hidden="true"><rect x="4" y="6" width="16" height="12" rx="1.5"/><path d="M4.5 7l7.5 6 7.5-6"/></svg>',
    'phone': '<svg viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="1.6" aria-hidden="true"><path d="M5 10.5c0-2 1.5-4 7-4s7 2 7 4v1.5l-3 .5-1-2.5c-2-.6-4-.6-6 0l-1 2.5-3-.5z"/><path d="M12 13v5M9 18h6M9.5 15.5h5"/></svg>',
    'logo': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3.4" stroke-linejoin="round" aria-hidden="true"><path d="M5.5 5.8C6.5 3.8 9 3.5 12 3.6s6 .3 7 2.4c.8 1.8.6 4.4.5 6.5s.4 4.4-.5 6.2c-1 2-4 2.2-7 2.1s-5.6.1-6.8-2c-.9-1.6-.6-4.1-.6-6.3s-.4-4.7.9-6.7z"/></svg>',
}

IMG = '<img class="ph" alt="§ALT_%s§" src="/templates/freeform/' + KEY + '/assets/%s.jpg" style="top:%dpx; inset-inline-start:%dpx; width:%dpx; height:%dpx;">'


def img(name, key, x, y, w, h):
    return IMG % (key, name, y, x, w, h)


STYLE = """  .frame { overflow: hidden; }
  .frame.s1, .frame.s2, .frame.s12 { background: var(--navy); }
  .deco { position: absolute; inset: 0; width: 1280px; height: 720px; pointer-events: none; }
  html[dir="rtl"] .deco { transform: scaleX(-1); }  /* الزخارف غير المتماثلة تنعكس للعربية */
  .ph { position: absolute; object-fit: cover; display: block; }
  .ttl { position: absolute; margin: 0; font-size: 44px; font-weight: 700; line-height: 1.15; color: var(--ink); white-space: nowrap; }
  .ttl .lt { font-weight: 300; }
  html[lang="ar"] .ttl .lt { font-weight: 400; }
  .ttl.w, .p.w, .h.w { color: #fff; }
  .ul { position: absolute; height: 4px; width: 122px; background: var(--accent); }
  .p { position: absolute; margin: 0; font-size: 17px; line-height: 1.35; color: var(--ink-2); }
  .p.lh { line-height: 1.55; }
  .h { position: absolute; margin: 0; font-size: 22px; font-weight: 700; color: #333; line-height: 1.2; }
  /* 1: الغلاف */
  .s1 .logo { position: absolute; top: 52px; inset-inline-start: 72px; width: 50px; height: 50px; color: var(--accent); }
  .s1 .logo svg { width: 50px; height: 50px; }
  .s1 .co { position: absolute; top: 58px; inset-inline-start: 143px; margin: 0; font-size: 16px; line-height: 1.45; letter-spacing: 2.5px; color: #fff; font-weight: 500; }
  .s1 .t1 { position: absolute; top: 304px; inset-inline-start: 64px; margin: 0; font-size: 72px; line-height: 80px; font-weight: 800; color: #fff; letter-spacing: 1px; white-space: nowrap; }
  .s1 .t1 span { color: var(--accent); }
  .s1 .ul { top: 510px; inset-inline-start: 64px; width: 103px; }
  .s1 .sub { position: absolute; top: 548px; inset-inline-start: 64px; margin: 0; font-size: 26px; letter-spacing: 4px; color: #fff; font-weight: 400; white-space: nowrap; }
  html[lang="ar"] .s1 .co, html[lang="ar"] .s1 .t1, html[lang="ar"] .s1 .sub { letter-spacing: 0; }
  html[lang="ar"] .s1 .co { font-size: 18px; }
  html[lang="ar"] .s1 .sub { font-size: 28px; }
  /* 2: جدول الأعمال */
  .s2 .ttl { top: 44px; inset-inline-start: 64px; font-size: 54px; }
  .s2 .ul { top: 133px; inset-inline-start: 64px; width: 103px; }
  .s2 .p { top: 162px; inset-inline-start: 64px; width: 520px; font-size: 18px; line-height: 1.3; }
  .agenda { position: absolute; top: 262px; width: 352px; margin: 0; padding: 0; list-style: none; }
  .agenda.c1 { inset-inline-start: 64px; } .agenda.c2 { inset-inline-start: 456px; }
  .agenda li { height: 99px; border-top: 1px solid var(--line); display: flex; align-items: center; }
  .agenda li:last-child { border-bottom: 1px solid var(--line); height: 100px; }
  .agenda b { font-size: 44px; font-weight: 800; color: var(--accent); width: 88px; flex: none; line-height: 1; }
  .agenda span { font-size: 22px; color: #fff; }
  /* 3: من نحن */
  .s3 .ttl { top: 78px; inset-inline-start: 757px; }
  .s3 .ul { top: 150px; inset-inline-start: 757px; width: 105px; }
  .s3 .p1 { top: 178px; inset-inline-start: 757px; width: 400px; }
  .s3 .p2 { top: 286px; inset-inline-start: 757px; width: 400px; }
  /* 4 و5: الرسالة والرؤية */
  .s4 .ttl { top: 68px; inset-inline-start: 64px; }
  .s4 .ul { top: 133px; inset-inline-start: 64px; width: 105px; }
  .s4 .p1 { top: 168px; inset-inline-start: 64px; width: 250px; }
  .s4 .p2 { top: 258px; inset-inline-start: 64px; width: 250px; }
  .s4 .pt { top: 66px; inset-inline-start: 436px; width: 780px; }
  .chk { position: absolute; top: 253px; margin: 0; padding: 0; list-style: none; }
  .s4 .chk { inset-inline-start: 808px; } .s5 .chk { inset-inline-start: 398px; }
  .chk li { display: flex; gap: 18px; align-items: flex-start; height: 105px; }
  .chk .ck { flex: none; width: 44px; height: 44px; border-radius: 50%; background: var(--steel); display: grid; place-items: center; }
  .chk .ck svg { width: 24px; height: 24px; }
  .chk p { margin: -3px 0 0; width: 185px; font-size: 17px; line-height: 1.33; color: #fff; }
  .s5 .pt { top: 66px; inset-inline-start: 64px; width: 660px; }
  .s5 .ttl { top: 63px; inset-inline-start: 885px; }
  .s5 .ul { top: 130px; inset-inline-start: 885px; width: 123px; }
  .s5 .p1 { top: 168px; inset-inline-start: 885px; width: 335px; }
  .s5 .p2 { top: 235px; inset-inline-start: 885px; width: 335px; }
  /* 6: الخدمات */
  .s6 .ttl { top: 60px; inset-inline-start: 64px; }
  .s6 .ul { top: 133px; inset-inline-start: 64px; }
  .s6 .big { position: absolute; top: 228px; inset-inline-start: 64px; width: 265px; margin: 0; font-size: 24px; font-weight: 700; line-height: 1.3; color: #4A4A4A; }
  .s6 .pt { top: 66px; inset-inline-start: 615px; width: 600px; }
  .svc { position: absolute; width: 230px; }
  .svc .ic { width: 64px; height: 64px; background: #27405F; display: grid; place-items: center; }
  .svc .ic svg { width: 36px; height: 36px; }
  .svc h3 { margin: 24px 0 8px; font-size: 22px; font-weight: 700; color: #333; }
  .svc p { margin: 0; font-size: 17px; line-height: 1.35; color: var(--ink-2); width: 215px; }
  .svc.a { top: 170px; inset-inline-start: 615px; } .svc.b { top: 170px; inset-inline-start: 957px; }
  .svc.c { top: 460px; inset-inline-start: 615px; } .svc.d { top: 460px; inset-inline-start: 957px; }
  /* 7: الفريق */
  .s7 .ttl { top: 84px; inset-inline-start: 64px; line-height: 56px; }
  .s7 .ul { top: 210px; inset-inline-start: 64px; }
  .s7 .p1 { top: 246px; inset-inline-start: 64px; width: 245px; }
  .s7 .pt { top: 110px; inset-inline-start: 700px; width: 420px; line-height: 1.45; }
  .card { position: absolute; top: 209px; width: 228px; }
  .card .ph { position: static; width: 228px; height: 249px; }
  .card h3 { margin: 37px 0 6px; font-size: 22px; font-weight: 700; color: #fff; }
  .card .role { margin: 0 0 20px; font-size: 18px; color: #fff; }
  .card p { margin: 0; font-size: 17px; line-height: 1.35; color: #fff; width: 215px; }
  .card.a { inset-inline-start: 428px; } .card.b { inset-inline-start: 700px; } .card.c { inset-inline-start: 972px; }
  /* 8: الرئيس التنفيذي */
  .s8 .ttl { top: 62px; inset-inline-start: 64px; }
  .s8 .ul { top: 133px; inset-inline-start: 64px; }
  .s8 .role { position: absolute; top: 218px; inset-inline-start: 405px; margin: 0; font-size: 24px; color: var(--accent); }
  .s8 .name { position: absolute; top: 258px; inset-inline-start: 405px; margin: 0; font-size: 36px; font-weight: 800; color: var(--ink); white-space: nowrap; }
  .s8 .p1 { top: 340px; inset-inline-start: 405px; width: 290px; line-height: 1.45; }
  .s8 .p2 { top: 463px; inset-inline-start: 405px; width: 290px; line-height: 1.45; }
  .s8 .quote { position: absolute; top: 138px; inset-inline-start: 872px; margin: 0; font-size: 64px; font-weight: 800; color: #fff; line-height: 1; }
  .s8 .q1 { top: 213px; inset-inline-start: 868px; width: 355px; }
  .s8 .q2 { top: 326px; inset-inline-start: 868px; width: 355px; }
  /* 9 و10: المحطات */
  .s9 .ttl, .s10 .ttl { top: 62px; inset-inline-start: 64px; }
  .s9 .ul, .s10 .ul { top: 133px; inset-inline-start: 64px; }
  .tln { position: absolute; top: 386px; width: 76px; height: 76px; border-radius: 50%; background: var(--steel); color: #fff; font-size: 26px; font-weight: 700; display: grid; place-items: center; margin: 0; }
  .ms { position: absolute; width: 215px; }
  .ms h3 { margin: 0 0 8px; font-size: 22px; font-weight: 700; color: #333; }
  .ms p { margin: 0; font-size: 17px; line-height: 1.33; color: var(--ink-2); }
  .s9 .n1 { inset-inline-start: 300px; } .s9 .n2 { inset-inline-start: 534px; } .s9 .n3 { inset-inline-start: 768px; } .s9 .n4 { inset-inline-start: 1002px; }
  .s9 .m1 { top: 505px; inset-inline-start: 300px; } .s9 .m2 { top: 215px; inset-inline-start: 534px; } .s9 .m3 { top: 505px; inset-inline-start: 768px; } .s9 .m4 { top: 215px; inset-inline-start: 1001px; }
  .s10 .n1 { inset-inline-start: 64px; } .s10 .n2 { inset-inline-start: 298px; } .s10 .n3 { inset-inline-start: 532px; } .s10 .n4 { inset-inline-start: 766px; }
  .s10 .m1 { top: 505px; inset-inline-start: 64px; } .s10 .m2 { top: 215px; inset-inline-start: 298px; } .s10 .m3 { top: 505px; inset-inline-start: 532px; } .s10 .m4 { top: 215px; inset-inline-start: 766px; }
  /* 11: الجوائز */
  .s11 .ttl { top: 63px; inset-inline-start: 64px; }
  .s11 .ul { top: 133px; inset-inline-start: 64px; }
  .s11 .p1 { top: 168px; inset-inline-start: 64px; width: 250px; line-height: 1.45; }
  .s11 .p2 { top: 267px; inset-inline-start: 64px; width: 250px; line-height: 1.45; }
  .aw { position: absolute; inset-inline-start: 829px; display: flex; gap: 28px; align-items: flex-start; }
  .aw .ic { flex: none; width: 86px; height: 86px; border-radius: 50%; display: grid; place-items: center; }
  .aw .ic svg { width: 44px; height: 44px; }
  .aw h3 { margin: 2px 0 8px; font-size: 22px; font-weight: 700; color: #333; }
  .aw p { margin: 0; width: 230px; font-size: 17px; line-height: 1.35; color: var(--ink-2); }
  .aw.a { top: 80px; } .aw.a .ic { background: #E6E5D3; }
  .aw.b { top: 222px; } .aw.b .ic { background: #A1B4C2; }
  .aw.c { top: 364px; } .aw.c .ic { background: #F6CBC5; }
  /* 12: الخاتمة */
  .s12 .ttl { top: 126px; inset-inline-start: 64px; font-size: 56px; font-weight: 800; }
  .s12 .ul { top: 218px; inset-inline-start: 64px; }
  .s12 .p1 { top: 262px; inset-inline-start: 64px; width: 420px; font-size: 18px; line-height: 1.4; }
  .s12 .div { position: absolute; top: 430px; inset-inline-start: 64px; width: 511px; height: 1px; background: #3A5370; }
  .ct { position: absolute; inset-inline-start: 65px; display: flex; gap: 20px; align-items: center; }
  .ct .ic { flex: none; width: 48px; height: 48px; border-radius: 50%; background: var(--accent); display: grid; place-items: center; }
  .ct .ic svg { width: 26px; height: 26px; }
  .ct p { margin: 0; font-size: 20px; line-height: 1.65; color: #fff; }
  .ct.a { top: 486px; } .ct.b { top: 598px; }"""


def chk_list():
    return '<ul class="chk">' + ''.join('<li>%s<p>§CHK§</p></li>' % CHECK for _ in range(4)) + '</ul>'


def ms(n, cls, y):
    return '<div class="ms %s"><h3>§Y%d§</h3><p>§MSD§</p></div>' % (cls, n)


SLIDES = [
    # 1 الغلاف
    ART_COVER + '<div class="logo">' + ICONS['logo'] + '</div><p class="co">§CO1§<br>§CO2§</p>'
    '<h1 class="t1">§T1A§<br><span>§T1B§</span></h1><div class="ul"></div><p class="sub">§SUB1§</p>',
    # 2 جدول الأعمال
    ART_AGENDA + '<h1 class="ttl w">§AG§</h1><div class="ul"></div><p class="p w">§AGP§</p>'
    '<ul class="agenda c1"><li><b>01</b><span>§A1§</span></li><li><b>02</b><span>§A2§</span></li><li><b>03</b><span>§A3§</span></li><li><b>04</b><span>§A4§</span></li></ul>'
    '<ul class="agenda c2"><li><b>05</b><span>§A5§</span></li><li><b>06</b><span>§A6§</span></li><li><b>07</b><span>§A7§</span></li><li><b>08</b><span>§A8§</span></li></ul>',
    # 3 من نحن
    ART_ABOUT + '<h1 class="ttl">§AB1§ <span class="lt">§AB2§</span></h1><div class="ul"></div>'
    '<p class="p lh p1">§ABP1§</p><p class="p lh p2">§ABP2§</p>' + img('office', 'OFFICE', 537, 438, 678, 214),
    # 4 الرسالة
    ART_MISSION + '<h1 class="ttl"><span class="lt">§MI1§</span> §MI2§</h1><div class="ul"></div>'
    '<p class="p p1">§MIP1§</p><p class="p p2">§MIP2§</p><p class="p pt">§MIT§</p>' + img('team-hands', 'HANDS', 400, 186, 346, 534) + chk_list(),
    # 5 الرؤية
    ART_VISION + '<p class="p pt">§VIT§</p>' + img('meeting', 'MEETING', 0, 186, 338, 534) + chk_list()
    + '<h1 class="ttl"><span class="lt">§VI1§</span> §VI2§</h1><div class="ul"></div><p class="p p1">§VIP1§</p><p class="p p2">§VIP2§</p>',
    # 6 الخدمات
    ART_SERVICES + '<h1 class="ttl">§SV§</h1><div class="ul"></div><p class="big">§SVB§</p>' + img('stairs', 'STAIRS', 194, 358, 369, 362)
    + '<p class="p pt">§SVT§</p>'
    + ''.join('<div class="svc %s"><div class="ic">%s</div><h3>§S0%d§</h3><p>§SVD§</p></div>' % (c, ICONS[i], n) for c, i, n in (('a', 'bulb', 1), ('b', 'gears', 2), ('c', 'server', 3), ('d', 'chart', 4))),
    # 7 الفريق
    ART_TEAM + '<h1 class="ttl"><span class="lt">§TM1§</span><br>§TM2§</h1><div class="ul"></div><p class="p p1">§TMP§</p><p class="p w pt">§TMT§</p>'
    + ''.join('<div class="card %s">%s<h3>§PH§</h3><p class="role">§PH§</p><p>§TMD§</p></div>' % (c, img('member-%d' % n, 'MEMBER', 0, 0, 228, 249)) for c, n in (('a', 1), ('b', 2), ('c', 3))),
    # 8 الرئيس التنفيذي
    ART_CEO + '<h1 class="ttl"><span class="lt">§CEO1§</span> §CEO2§</h1><div class="ul"></div>' + img('ceo', 'CEO', 0, 216, 359, 431)
    + '<p class="role">§CEOR§</p><h2 class="name">§CEON§</h2><p class="p p1">§CEOP1§</p><p class="p p2">§CEOP2§</p>'
    '<p class="quote">&ldquo;</p><p class="p w q1">§CEOQ1§</p><p class="p w q2">§CEOQ2§</p>',
    # 9 المحطات 1-4
    ART_MS1 + '<h1 class="ttl">§MS§</h1><div class="ul"></div>' + img('building-1', 'BUILDING', 0, 210, 246, 429)
    + ''.join('<p class="tln n%d">0%d</p>' % (i, i) for i in range(1, 5))
    + ms(1, 'm1', 0) + ms(2, 'm2', 0) + ms(3, 'm3', 0) + ms(4, 'm4', 0),
    # 10 المحطات 5-8
    ART_MS2 + '<h1 class="ttl">§MS§</h1><div class="ul"></div>' + img('building-2', 'BUILDING', 1033, 210, 247, 429)
    + ''.join('<p class="tln n%d">0%d</p>' % (i, i + 4) for i in range(1, 5))
    + ms(5, 'm1', 0) + ms(6, 'm2', 0) + ms(7, 'm3', 0) + ms(8, 'm4', 0),
    # 11 الجوائز
    ART_AWARDS + '<h1 class="ttl w">§AW§</h1><div class="ul"></div><p class="p w p1">§AWP1§</p><p class="p w p2">§AWP2§</p>' + img('fireworks', 'FIREWORKS', 406, 186, 363, 534)
    + ''.join('<div class="aw %s"><div class="ic">%s</div><div><h3>§PH§</h3><p>§AWD§</p></div></div>' % (c, ICONS[i]) for c, i in (('a', 'ribbon'), ('b', 'trophy'), ('c', 'medal'))),
    # 12 الخاتمة
    ART_COVER + '<h1 class="ttl w">§TY§</h1><div class="ul"></div><p class="p w p1">§TYP§</p><div class="div"></div>'
    '<div class="ct a"><div class="ic">' + ICONS['mail'] + '</div><p>§EM§<br>§EMV§</p></div>'
    '<div class="ct b"><div class="ic">' + ICONS['phone'] + '</div><p>§PHN§<br><span dir="ltr">§PHV§</span></p></div>',
]

SAMPLE_EN = 'This is a sample text. Insert your desired text here.'
SAMPLE_AR = 'هذا نص توضيحي. أضف النص المطلوب هنا.'
INS_EN, INS_AR = 'Insert your desired text here.', 'أضف النص المطلوب هنا.'
TIS_EN, TIS_AR = 'This is a sample text.', 'هذا نص توضيحي.'

STR = {
    'en': {
        'CO1': 'YOUR COMPANY', 'CO2': 'LOGO HERE', 'T1A': 'AESTHETIC', 'T1B': 'GEOMETRY', 'SUB1': 'PRESENTATION TEMPLATE',
        'AG': 'Agenda', 'AGP': SAMPLE_EN + ' ' + SAMPLE_EN,
        'A1': 'About Us', 'A2': 'Our Mission', 'A3': 'Our Vision', 'A4': 'Our Service', 'A5': 'Our Team', 'A6': 'Meet Our CEO', 'A7': 'Milestones', 'A8': 'Our Awards',
        'AB1': 'About', 'AB2': 'Us', 'ABP1': SAMPLE_EN + ' ' + TIS_EN + ' ' + SAMPLE_EN, 'ABP2': SAMPLE_EN + ' ' + TIS_EN,
        'MI1': 'Our', 'MI2': 'Mission', 'MIP1': SAMPLE_EN + ' ' + TIS_EN, 'MIP2': SAMPLE_EN + ' ' + SAMPLE_EN,
        'MIT': SAMPLE_EN + ' ' + TIS_EN + ' ' + SAMPLE_EN + ' ' + SAMPLE_EN, 'CHK': SAMPLE_EN,
        'VI1': 'Our', 'VI2': 'Vision', 'VIT': SAMPLE_EN + ' ' + TIS_EN + ' ' + SAMPLE_EN, 'VIP1': SAMPLE_EN + ' ' + TIS_EN, 'VIP2': SAMPLE_EN,
        'SV': 'Services', 'SVT': SAMPLE_EN + ' ' + SAMPLE_EN + ' ' + TIS_EN, 'SVB': SAMPLE_EN,
        'S01': 'Service 01', 'S02': 'Service 02', 'S03': 'Service 03', 'S04': 'Service 04', 'SVD': INS_EN + ' ' + INS_EN,
        'TM1': 'Our', 'TM2': 'Team', 'TMP': SAMPLE_EN + ' ' + TIS_EN, 'TMT': SAMPLE_EN + ' ' + TIS_EN + ' ' + TIS_EN, 'PH': 'Placeholder', 'TMD': INS_EN + ' ' + INS_EN,
        'CEO1': 'Meet Our', 'CEO2': 'CEO', 'CEOR': 'Company CEO', 'CEON': 'Jack Johnson',
        'CEOP1': SAMPLE_EN + ' ' + TIS_EN + ' ' + SAMPLE_EN, 'CEOP2': SAMPLE_EN,
        'CEOQ1': 'This is a sample text. Add your desired text here. Add text here. This is a sample text. Add your desired text here. Add text here. Add text here.',
        'CEOQ2': 'This is a sample text. Add your desired text here. Add text here. This is a sample text.',
        'MS': 'Milestones', 'MSD': INS_EN + ' ' + INS_EN,
        'Y1': '2019-20', 'Y2': '2020-21', 'Y3': '2021-22', 'Y4': '2022-23', 'Y5': '2023-24', 'Y6': '2024-25', 'Y7': '2025-26', 'Y8': '2026-27',
        'AW': 'Awards', 'AWP1': SAMPLE_EN + ' ' + TIS_EN, 'AWP2': SAMPLE_EN, 'AWD': SAMPLE_EN,
        'TY': 'THANK YOU', 'TYP': 'This is a sample text. Insert your desired text here. This is a sample text.. Insert your desired text here. This is a sample text.',
        'EM': 'Email', 'EMV': 'Add email address', 'PHN': 'Phone', 'PHV': '+1 (234) 123 4567',
        'ALT_OFFICE': 'Office workspace', 'ALT_HANDS': 'Team hands together', 'ALT_MEETING': 'Team meeting', 'ALT_STAIRS': 'Stairs in a modern building',
        'ALT_MEMBER': 'Team member portrait', 'ALT_CEO': 'CEO portrait', 'ALT_BUILDING': 'Building facade', 'ALT_FIREWORKS': 'Fireworks over the city',
    },
    'ar': {
        'CO1': 'ضع شعار', 'CO2': 'شركتك هنا', 'T1A': 'هندسة', 'T1B': 'جمالية', 'SUB1': 'قالب عرض تقديمي',
        'AG': 'جدول الأعمال', 'AGP': SAMPLE_AR + ' ' + SAMPLE_AR,
        'A1': 'من نحن', 'A2': 'رسالتنا', 'A3': 'رؤيتنا', 'A4': 'خدماتنا', 'A5': 'فريقنا', 'A6': 'رئيسنا التنفيذي', 'A7': 'المحطات البارزة', 'A8': 'جوائزنا',
        'AB1': 'من', 'AB2': 'نحن', 'ABP1': SAMPLE_AR + ' ' + TIS_AR + ' ' + SAMPLE_AR, 'ABP2': SAMPLE_AR + ' ' + TIS_AR,
        'MI1': 'رسالة', 'MI2': 'الشركة', 'MIP1': SAMPLE_AR + ' ' + TIS_AR, 'MIP2': SAMPLE_AR + ' ' + SAMPLE_AR,
        'MIT': SAMPLE_AR + ' ' + TIS_AR + ' ' + SAMPLE_AR + ' ' + SAMPLE_AR, 'CHK': SAMPLE_AR,
        'VI1': 'رؤية', 'VI2': 'الشركة', 'VIT': SAMPLE_AR + ' ' + TIS_AR + ' ' + SAMPLE_AR, 'VIP1': SAMPLE_AR + ' ' + TIS_AR, 'VIP2': SAMPLE_AR,
        'SV': 'الخدمات', 'SVT': SAMPLE_AR + ' ' + SAMPLE_AR + ' ' + TIS_AR, 'SVB': SAMPLE_AR,
        'S01': 'الخدمة 01', 'S02': 'الخدمة 02', 'S03': 'الخدمة 03', 'S04': 'الخدمة 04', 'SVD': INS_AR + ' ' + INS_AR,
        'TM1': 'فريق', 'TM2': 'العمل', 'TMP': SAMPLE_AR + ' ' + TIS_AR, 'TMT': SAMPLE_AR + ' ' + TIS_AR + ' ' + TIS_AR, 'PH': 'عنوان نائب', 'TMD': INS_AR + ' ' + INS_AR,
        'CEO1': 'تعرّف على', 'CEO2': 'رئيسنا التنفيذي', 'CEOR': 'الرئيس التنفيذي للشركة', 'CEON': 'جاك جونسون',
        'CEOP1': SAMPLE_AR + ' ' + TIS_AR + ' ' + SAMPLE_AR, 'CEOP2': SAMPLE_AR,
        'CEOQ1': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف النص هنا. هذا نص توضيحي. أضف النص المطلوب هنا. أضف النص هنا. أضف النص هنا.',
        'CEOQ2': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف النص هنا. هذا نص توضيحي.',
        'MS': 'المحطات البارزة', 'MSD': INS_AR + ' ' + INS_AR,
        'Y1': '2019-20', 'Y2': '2020-21', 'Y3': '2021-22', 'Y4': '2022-23', 'Y5': '2023-24', 'Y6': '2024-25', 'Y7': '2025-26', 'Y8': '2026-27',
        'AW': 'الجوائز', 'AWP1': SAMPLE_AR + ' ' + TIS_AR, 'AWP2': SAMPLE_AR, 'AWD': SAMPLE_AR,
        'TY': 'شكرًا لكم', 'TYP': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي.',
        'EM': 'البريد الإلكتروني', 'EMV': 'أضف عنوان البريد الإلكتروني', 'PHN': 'الهاتف', 'PHV': '+1 (234) 123 4567',
        'ALT_OFFICE': 'مساحة عمل مكتبية', 'ALT_HANDS': 'أيدي الفريق متشابكة', 'ALT_MEETING': 'اجتماع الفريق', 'ALT_STAIRS': 'درج في مبنى حديث',
        'ALT_MEMBER': 'صورة عضو الفريق', 'ALT_CEO': 'صورة الرئيس التنفيذي', 'ALT_BUILDING': 'واجهة مبنى', 'ALT_FIREWORKS': 'ألعاب نارية فوق المدينة',
    },
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'general', 'icon': '🎨',
    'name': 'هندسة جمالية', 'nameEn': 'Aesthetic Geometry',
    'description': '12 شريحة بزخارف هندسية: غلاف، جدول أعمال، من نحن، الرسالة، الرؤية، الخدمات، الفريق، الرئيس، محطات، جوائز، خاتمة',
    'descriptionEn': '12 geometric slides: cover, agenda, about, mission, vision, services, team, CEO, milestones, awards, thank you',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'جدول الأعمال', 'titleEn': 'Agenda'},
        {'title': 'من نحن', 'titleEn': 'About Us'},
        {'title': 'رسالتنا', 'titleEn': 'Our Mission'},
        {'title': 'رؤيتنا', 'titleEn': 'Our Vision'},
        {'title': 'الخدمات', 'titleEn': 'Services'},
        {'title': 'فريقنا', 'titleEn': 'Our Team'},
        {'title': 'الرئيس التنفيذي', 'titleEn': 'Meet Our CEO'},
        {'title': 'المحطات البارزة (1–4)', 'titleEn': 'Milestones (1–4)', 'needsData': True},
        {'title': 'المحطات البارزة (5–8)', 'titleEn': 'Milestones (5–8)', 'needsData': True},
        {'title': 'الجوائز', 'titleEn': 'Awards'},
        {'title': 'شكرًا لكم', 'titleEn': 'Thank You', 'needsData': True},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والزخارف الهندسية (SVG بصنف deco) وغيّر النصوص والأرقام والسنوات وبيانات التواصل فقط؛ الصور في assets تبقى بمساراتها.',
}
