# -*- coding: utf-8 -*-
# نموذج «التعريف بالشركة»: عشر شرائح بهوية زرقاء/فيروزية/ذهبية/مرجانية: غلاف داكن، من نحن، الرؤية والرسالة، المحطات،
# الخدمات، فريق القيادة، محفظة العملاء، الحضور في الأسواق (خريطة)، لمحة مالية (رسم وجدول)، وشريحة ختام.
KEY = 'sl-company-introduction'
TITLE = {'ar': 'التعريف بالشركة', 'en': 'Company Introduction'}
COMMENT = 'نموذج «التعريف بالشركة»: عشر شرائح بخلفية شبكية خفيفة وزخارف مثلثات قطرية (طبقة .dg تنعكس في العربية)، بطاقات بزاوية مشطوفة ومثلث ملوّن (SVG)، صور فوتوغرافية مقصوصة، خريطة عالم SVG، ورسم أعمدة من كتلة chart-data.'
VARS = '--ink: #000000; --ink-2: #595959; --ink-3: #404040; --accent: #3C59FF; --blue: #3C59FF; --navy: #031174; --teal: #01E3BD; --gold: #EDA709; --coral: #FA7258; --gray: #F2F2F2; --bg: #FFFFFF;'
FONTS = {'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:ital,wght@0,300;0,400;0,600;0,700;0,800;1,700&display=swap" rel="stylesheet">'}
FONT_STACK = {'en': "'Open Sans', 'Segoe UI', Arial, sans-serif"}

STYLE = """  /* الخلفية الشبكية والطبقات الزخرفية */
  .grid { position: absolute; inset: 0; background-image: linear-gradient(90deg, rgb(0 0 0 / .03) 1.2px, transparent 1.2px), linear-gradient(rgb(0 0 0 / .03) 1.2px, transparent 1.2px); background-size: 67.37px 67.37px; background-position: 0 23px; }
  .s1 .grid { background-image: linear-gradient(90deg, rgb(255 255 255 / .06) 1.5px, transparent 1.5px), linear-gradient(rgb(255 255 255 / .06) 1.5px, transparent 1.5px); }
  .frame.s1 { background: linear-gradient(90deg, #0B1A81, #041275 45%); }
  .deco { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; pointer-events: none; }
  html[dir="rtl"] .dg { transform: scaleX(-1); }
  .card { position: absolute; overflow: visible; }
  .t { position: absolute; margin: 0; }
  /* العناوين المشتركة */
  .ttl { position: absolute; top: 61px; inset-inline-start: 84px; margin: 0; font-size: 53px; font-weight: 700; line-height: 62px; color: var(--ink); letter-spacing: -.3px; white-space: nowrap; }
  .sub { position: absolute; top: 142px; inset-inline-start: 84px; margin: 0; font-size: 22px; font-weight: 700; line-height: 28px; color: var(--blue); }
  .p { font-size: 18px; line-height: 23.3px; color: var(--ink-2); }
  .tri { position: absolute; width: 19px; height: 19px; clip-path: polygon(0 0, 100% 0, 0 100%); }
  html[dir="rtl"] .tri { clip-path: polygon(0 0, 100% 0, 100% 100%); }
  .ic { position: absolute; color: var(--ink-3); }
  .ic svg { width: 100%; height: 100%; }
  /* 1) الغلاف */
  .s1 .logo { position: absolute; top: 111px; inset-inline-start: 243px; width: 44px; height: 48px; }
  .s1 .logot { top: 111px; inset-inline-start: 305px; font-size: 17px; line-height: 26px; letter-spacing: 2.4px; color: #fff; }
  .s1 .big { top: 283px; inset-inline-start: 236px; font-size: 100px; font-weight: 700; line-height: 100px; color: #fff; letter-spacing: -.5px; white-space: nowrap; }
  .s1 .sbox, .s10 .btn { position: absolute; display: flex; align-items: center; color: #fff; background: var(--blue); box-shadow: -7px 7px 0 var(--teal); }
  html[dir="rtl"] .s1 .sbox, html[dir="rtl"] .s10 .btn { box-shadow: 7px 7px 0 var(--teal); }
  .s1 .sbox { top: 515px; inset-inline-start: 245px; width: 571px; height: 81px; padding-inline-start: 48px; font-size: 30px; letter-spacing: 3px; font-weight: 400; }
  .s1 .date { top: 642px; inset-inline-start: 261px; font-size: 20px; line-height: 26px; color: var(--teal); }
  .s1 .tag { top: 642px; inset-inline-start: 405px; font-size: 20px; line-height: 26px; color: #E6A316; }
  /* 2) من نحن */
  .s2 .story { top: 242px; inset-inline-start: 84px; width: 460px; }
  .s2 .rule { position: absolute; top: 358px; inset-inline-start: 82px; width: 533px; height: 2px; background: linear-gradient(90deg, #8C8C8C, #E0E0E0); }
  html[dir="rtl"] .s2 .rule { background: linear-gradient(-90deg, #8C8C8C, #E0E0E0); }
  .s2 .num, .s7 .num, .s8 .num { font-size: 37px; font-weight: 700; line-height: 46px; color: var(--ink); letter-spacing: -.8px; white-space: nowrap; }
  .s2 .lbl, .s7 .lbl, .s8 .lbl { font-size: 18px; line-height: 22px; color: var(--ink-2); white-space: nowrap; }
  .s2 .photo { position: absolute; top: 62px; inset-inline-start: 654px; width: 596px; height: 627px; object-fit: cover; }
  html[dir="rtl"] .s2 .photo { transform: scaleX(-1); }
  /* 3) الرؤية والرسالة */
  .s3 .h { font-size: 24px; font-weight: 700; line-height: 30px; color: var(--ink); }
  .s3 .cv { top: 164px; inset-inline-start: 893px; font-size: 30px; font-weight: 700; line-height: 34px; color: #fff; }
  .s3 .cvrule { position: absolute; top: 275px; inset-inline-start: 884px; width: 286px; height: 1px; background: rgb(255 255 255 / .5); }
  .s3 .vals { top: 320px; inset-inline-start: 893px; padding: 0; list-style: none; font-size: 35px; line-height: 44px; color: #fff; }
  .s3 .vals li { height: 78.5px; white-space: nowrap; }
  /* 4) المحطات */
  .s4 .yr { top: 263px; font-size: 54px; font-weight: 700; line-height: 64px; color: #fff; letter-spacing: -1.5px; }
  .s4 .h { top: 481px; font-size: 22px; font-weight: 700; line-height: 29px; color: var(--ink); }
  .s4 .p { top: 561px; width: 222px; }
  /* 5) الخدمات */
  .s5 .no { font-size: 30px; font-weight: 700; line-height: 36px; }
  .s5 .h { font-size: 26px; font-weight: 700; line-height: 35px; color: var(--ink); }
  .s5 .p { width: 242px; }
  /* 6) فريق القيادة */
  .s6 .face { position: absolute; top: 282px; width: 278px; height: 205px; object-fit: cover; }
  .s6 .nm { top: 514px; font-size: 26px; font-weight: 700; line-height: 34px; color: var(--ink); white-space: nowrap; }
  .s6 .role { top: 557px; font-size: 20px; line-height: 26px; color: var(--gold); white-space: nowrap; }
  .s6 .p { top: 600px; width: 232px; }
  /* 7) محفظة العملاء */
  .s7 .bar { position: absolute; top: 299px; width: 5px; height: 105px; }
  .s7 .h { top: 481px; inset-inline-start: 84px; font-size: 26px; font-weight: 700; line-height: 35px; color: var(--ink); }
  .s7 .p { top: 572px; inset-inline-start: 84px; width: 630px; }
  .s7 .q { top: 262px; inset-inline-start: 866px; width: 345px; font-size: 36px; font-weight: 300; line-height: 40px; color: #fff; }
  .s7 .qrule { position: absolute; top: 542px; inset-inline-start: 855px; width: 315px; height: 1px; background: rgb(255 255 255 / .5); }
  .s7 .who { top: 574px; inset-inline-start: 866px; font-size: 20px; line-height: 26px; color: #fff; }
  .s7 .who b { font-weight: 700; }
  /* 8) الحضور في الأسواق */
  .s8 .map { position: absolute; top: 200px; inset-inline-start: 90px; width: 710px; height: 400px; }
  .s8 .map .land { fill: #D3DEFE; } .s8 .map circle { fill: var(--blue); }
  .s8 .km { top: 597px; inset-inline-start: 84px; font-size: 26px; font-weight: 700; line-height: 34px; color: var(--ink); }
  .s8 .kml { top: 642px; inset-inline-start: 84px; font-size: 22px; line-height: 30px; color: var(--ink-2); white-space: nowrap; }
  /* 9) لمحة مالية */
  .s9 .rg { top: 245px; inset-inline-start: 84px; font-size: 18px; font-weight: 700; line-height: 22px; color: var(--ink); }
  .s9 .chart { position: absolute; top: 290px; inset-inline-start: 60px; width: 460px; height: 410px; }
  .s9 table { position: absolute; top: 187px; inset-inline-start: 582px; width: 650px; border-collapse: collapse; table-layout: fixed; font-size: 18px; color: var(--ink-2); }
  .s9 th { height: 50px; background: var(--blue); color: #fff; font-size: 20px; font-weight: 700; text-align: start; padding: 0; }
  .s9 td { height: 50.7px; padding: 0; text-align: start; }
  .s9 th:first-child, .s9 td:first-child { padding-inline-start: 19px; }
  .s9 tbody tr:nth-child(even) td { background: #F6F6F6; }
  .s9 .kpi { top: 556px; inset-inline-start: 600px; width: 280px; text-align: center; }
  .s9 .kpi b { display: block; font-size: 52px; font-weight: 700; line-height: 62px; color: var(--ink); letter-spacing: -1px; }
  .s9 .kpi span { display: block; margin-top: 8px; font-size: 18px; line-height: 22px; color: var(--ink-2); }
  .s9 .gbar { position: absolute; top: 537px; inset-inline-start: 879px; width: 5px; height: 135px; background: var(--gold); }
  .s9 .p { top: 556px; inset-inline-start: 917px; width: 248px; }
  /* 10) الختام */
  .s10 .end { top: 231px; inset-inline-start: 120px; font-size: 64px; font-weight: 700; line-height: 68px; color: var(--ink); letter-spacing: -.5px; white-space: nowrap; }
  .s10 .btn { top: 424px; inset-inline-start: 113px; width: 315px; height: 82px; justify-content: center; font-size: 32px; font-weight: 700; letter-spacing: 4px; box-shadow: -7px 8px 0 var(--teal); }
  html[dir="rtl"] .s10 .btn { box-shadow: 7px 8px 0 var(--teal); }
  .s10 .ct { inset-inline-start: 182px; font-size: 18px; font-weight: 700; font-style: italic; line-height: 22px; color: var(--gold); white-space: nowrap; }
  html[lang="ar"] .s10 .ct { font-style: normal; }
  html[lang="ar"] .ttl, html[lang="ar"] .s1 .big, html[lang="ar"] .s10 .end { letter-spacing: 0; }
  html[lang="ar"] .s1 .logot, html[lang="ar"] .s1 .sbox, html[lang="ar"] .s10 .btn { letter-spacing: 0; }"""

GOLD, TEAL, BLUE, CORAL = '#EDA709', '#01E3BD', '#3C59FF', '#FA7258'
SCOL = {GOLD: 'sgold', TEAL: 'steal', BLUE: 'sblue', CORAL: 'scoral'}

GLOBAL_DEFS = ('<defs>'
    '<linearGradient id="gfill" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#F2F2F2"/><stop offset="1" stop-color="#FFFFFF"/></linearGradient>'
    + ''.join('<linearGradient id="%s" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="%s"/><stop offset=".55" stop-color="%s" stop-opacity=".8"/><stop offset="1" stop-color="%s" stop-opacity="0"/></linearGradient>' % (v, k, k, k) for k, v in SCOL.items())
    + '<linearGradient id="arrowg" x1="0" y1="0" x2="1263" y2="0" gradientUnits="userSpaceOnUse"><stop offset="0" stop-color="#01E3BD"/><stop offset=".24" stop-color="#209BDE"/><stop offset=".44" stop-color="#3C5CFF"/><stop offset="1" stop-color="#3C59FF"/></linearGradient>'
    + '<linearGradient id="lineg" x1="640" y1="0" x2="1158" y2="0" gradientUnits="userSpaceOnUse"><stop offset="0" stop-color="#3C59FF" stop-opacity="0"/><stop offset="1" stop-color="#3C59FF"/></linearGradient>'
    + '</defs>')


def poly(pts, fill, extra=''):
    return '<polygon points="%s" fill="%s"%s/>' % (' '.join('%s,%s' % p for p in pts), fill, extra)


def deco(*shapes, cls='deco dg'):
    return '<svg class="%s" viewBox="0 0 1280 720" aria-hidden="true">%s</svg>' % (cls, ''.join(shapes))


def line(x1, y1, x2, y2, col, w=2):
    return '<line x1="%d" y1="%d" x2="%d" y2="%d" stroke="%s" stroke-width="%s"/>' % (x1, y1, x2, y2, col, w)


def card(x, y, w, h, kind, color, ch, tri, bar=None):
    """بطاقة بزاوية مشطوفة: kind = fill (تعبئة بلون وشريط سفلي) أو outline (إطار يتلاشى للأسفل)."""
    tx, ty, ts = tri
    pts = [(0, ch), (ch, 0), (w, 0), (w, h), (0, h)]
    if kind == 'fill':
        body = poly(pts, color) + ('<rect x="0" y="%d" width="%d" height="5" fill="%s"/>' % (h - 5, w, bar) if bar else '')
    else:
        body = (poly(pts, 'url(#gfill)')
                + '<path d="M1,%d L1,%d L%d,1 L%d,1 L%d,%d" fill="none" stroke="url(#%s)" stroke-width="2"/>' % (h, ch, ch, w - 1, w - 1, h, SCOL[color]))
    body += poly([(tx, ty), (tx + ts, ty), (tx, ty + ts)], color if kind != 'fill' or color != BLUE else TEAL)
    return ('<svg class="card dg" style="top:%dpx;inset-inline-start:%dpx;width:%dpx;height:%dpx" viewBox="0 0 %d %d" aria-hidden="true">%s</svg>'
            % (y, x, w, h, w, h, body))


def tri(x, y, color, s=19):
    return '<span class="tri" style="top:%dpx;inset-inline-start:%dpx;width:%dpx;height:%dpx;background:%s"></span>' % (y, x, s, s, color)


def t(cls, text, style=''):
    return '<p class="t %s"%s>%s</p>' % (cls, (' style="%s"' % style) if style else '', text)


def pos(x, y, w=None, h=None):
    s = 'top:%dpx;inset-inline-start:%dpx;' % (y, x)
    if w: s += 'width:%dpx;' % w
    if h: s += 'height:%dpx;' % h
    return s


ICONS = {
    'buildings': '<svg viewBox="0 0 62 48" fill="currentColor" aria-hidden="true"><path fill-rule="evenodd" d="M10 0h12v48H10zM13 6h3v4h-3zM18 6h2v4h-2zM13 14h3v4h-3zM18 14h2v4h-2zM13 22h3v4h-3zM18 22h2v4h-2z"/><path fill-rule="evenodd" d="M0 20h10v28H0zM3 25h4v4H3zM3 33h4v4H3z"/><path fill-rule="evenodd" d="M22 24h18v24H22zM25 29h4v4h-4zM33 29h4v4h-4zM25 37h4v4h-4zM33 37h4v4h-4z"/><path fill-rule="evenodd" d="M44 6h18v42H44zM47 11h4v4h-4zM55 11h4v4h-4zM47 19h4v4h-4zM55 19h4v4h-4zM47 27h4v4h-4zM55 27h4v4h-4zM47 35h4v4h-4zM55 35h4v4h-4z"/></svg>',
    'target': '<svg viewBox="0 0 60 60" fill="none" stroke="currentColor" aria-hidden="true"><circle cx="28" cy="32" r="25" stroke-width="5"/><circle cx="28" cy="32" r="15" stroke-width="5"/><circle cx="28" cy="32" r="5" fill="currentColor" stroke="none"/><path d="M29 31L50 10" stroke-width="4"/><path d="M45 4l1 9 9 1 4-6-7-1-1-7z" fill="currentColor" stroke="none"/></svg>',
    'tower': '<svg viewBox="0 0 40 56" fill="currentColor" aria-hidden="true"><path fill-rule="evenodd" d="M6 2h28v50H6zM10 8h5v4h-5zM18 8h4v4h-4zM25 8h5v4h-5zM10 16h5v4h-5zM18 16h4v4h-4zM25 16h5v4h-5zM10 24h5v4h-5zM18 24h4v4h-4zM25 24h5v4h-5zM10 32h5v4h-5zM18 32h4v4h-4zM25 32h5v4h-5zM16 42h8v10h-8z"/><rect x="0" y="52" width="40" height="4"/></svg>',
    'globe': '<svg viewBox="0 0 52 52" fill="currentColor" aria-hidden="true"><path d="M26 0a26 26 0 1 0 0 52 26 26 0 0 0 0-52zm0 5a21 21 0 1 1 0 42 21 21 0 0 1 0-42z"/><path d="M14 11c4-3 9-4 13-3l-2 4-5 1-1 5-6 2-2 6 4 3 4-2 6 3 3 7-3 9-4 2-2-8-4-4-5-6v-8l-4-2c1-3 3-5 4-7zm22 2c4 3 7 7 8 12l-5 2-3-4-4 1-1-5 4-2z"/></svg>',
    'megaphone': '<svg viewBox="0 0 50 48" fill="currentColor" aria-hidden="true"><circle cx="9" cy="8" r="7"/><path d="M2 18h13l9 2v6l-8-1v23H5V30l-3-2z"/><path d="M24 14l18-8v24l-18-8z"/><rect x="42" y="4" width="6" height="28" rx="2"/></svg>',
    'city': '<svg viewBox="0 0 56 44" fill="currentColor" aria-hidden="true"><path fill-rule="evenodd" d="M10 0h14v44H10zM13 4h3v4h-3zM19 4h3v4h-3zM13 12h3v4h-3zM19 12h3v4h-3zM13 20h3v4h-3zM19 20h3v4h-3z"/><path fill-rule="evenodd" d="M0 20h10v24H0zM3 24h4v4H3zM3 32h4v4H3z"/><path fill-rule="evenodd" d="M24 24h14v20H24zM27 28h3v4h-3zM32 28h3v4h-3zM27 35h3v4h-3zM32 35h3v4h-3z"/><path fill-rule="evenodd" d="M38 2h18v42H38zM41 6h4v4h-4zM49 6h4v4h-4zM41 14h4v4h-4zM49 14h4v4h-4zM41 22h4v4h-4zM49 22h4v4h-4zM41 30h4v4h-4zM49 30h4v4h-4z"/></svg>',
    'checklist': '<svg viewBox="0 0 32 40" fill="none" stroke="currentColor" stroke-width="2.6" aria-hidden="true"><rect x="2" y="2" width="28" height="36" rx="1"/><path d="M7 10l2 2 3-4M7 19l2 2 3-4M7 28l2 2 3-4M16 10h10M16 19h10M16 28h10"/></svg>',
    'network': '<svg viewBox="0 0 44 40" fill="currentColor" aria-hidden="true"><path d="M8 26L18 18M18 18L32 7M18 18l14 8M32 26l-6 9M8 26l14 9M32 7l0 19" stroke="currentColor" stroke-width="2" fill="none"/><circle cx="18" cy="18" r="6"/><circle cx="32" cy="7" r="5"/><circle cx="8" cy="26" r="5"/><circle cx="34" cy="26" r="5"/><circle cx="22" cy="35" r="4.5"/></svg>',
    'handshake': '<svg viewBox="0 0 44 28" fill="currentColor" aria-hidden="true"><path d="M0 4l8-3 4 12-7 4zM44 4l-8-3-4 12 7 4z"/><path d="M10 6l8-3 6 2 7-2 5 2 3 9-4 3-9-8-5 3c-2 1-4 0-4-2l3-3-5 1-2 9z"/><path d="M13 16l6 6c1 1 3 1 4 0l1-1 1 1c1 1 3 1 4-1l1-1 1 1c1 1 3 0 3-2l-9-8-4 2c-3 1-6 0-6-2z"/></svg>',
    'mail': '<svg viewBox="0 0 26 20" fill="none" stroke="currentColor" stroke-width="1.6" aria-hidden="true"><rect x="1" y="1" width="24" height="18"/><path d="M1 1l12 10L25 1M1 19l9-9M25 19l-9-9"/></svg>',
    'phone': '<svg viewBox="0 0 28 28" fill="currentColor" aria-hidden="true"><path d="M6 1l5 7-3 3c1 3 5 8 9 9l3-3 7 5-4 5C12 27 1 16 1 5z"/></svg>',
    'logo': '<svg viewBox="0 0 44 48" aria-hidden="true"><path d="M9 6 Q23 1 38 7 Q42 24 36 40 Q21 46 6 42 Q2 23 9 6Z" fill="none" stroke="#fff" stroke-width="6.5" stroke-linejoin="round"/></svg>',
}


def ic(name, x, y, w, h, color=None):
    return '<span class="ic" style="%s%s">%s</span>' % (pos(x, y, w, h), ('color:%s' % color) if color else '', ICONS[name])


GRID = '<div class="grid"></div>'
# زخارف مشتركة: الزاوية العليا والسفلى للغلاف والختام، والمثلث الباهت أعلى البداية
TL_COVER = poly([(0, 0), (205, 0), (0, 205)], BLUE) + poly([(105, 0), (268, 0), (105, 163)], TEAL) + poly([(74, 0), (180, 0), (74, 106)], '#E6A316')
BR_COVER = poly([(1040, 720), (1280, 480), (1280, 720)], BLUE) + poly([(1057, 656), (1232, 481), (1232, 656)], TEAL)
TL_PALE = poly([(0, 0), (259, 0), (0, 259)], '#F1F4FF') + poly([(8, 8), (93, 8), (8, 93)], BLUE)

MAP_D = 'M688 544 689 546 693 544 691 543ZM742 541 740 540 731 547 731 550 734 551ZM745 530 744 536 747 536 748 531ZM506 492 503 496 499 498 498 505 496 508 496 513 498 516 501 516 503 513 507 499ZM629 509 630 517 633 525 633 530 635 532 646 529 654 524 663 523 669 527 673 527 679 535 682 536 695 535 697 534 698 528 703 518 702 510 690 498 688 491 684 486 682 486 681 494 679 496 670 491 670 486 668 485 662 486 658 491 653 490 649 492 642 501 633 504ZM666 468 668 470 673 471 677 475 677 477 681 479 690 475 686 470 680 467 672 467 670 469ZM606 454 605 456 606 459 602 460 601 462 608 472 612 473 613 470 607 462 609 458ZM636 451 634 451 626 459 620 462 621 465 625 469 630 470 633 469 637 460ZM278 442 273 443 268 448 266 448 268 456 259 472 270 491 276 495 281 502 280 517 275 538 275 543 272 552 271 566 272 573 275 578 280 582 287 583 282 576 282 570 287 565 287 556 290 550 294 547 294 544 299 540 302 540 305 535 310 533 319 523 323 514 329 510 338 501 338 493 340 488 345 483 346 477 341 473 324 468 312 455 304 453 295 445ZM271 432 272 434 277 433 277 432ZM681 378 679 379 679 383 675 388 673 388 669 392 666 392 664 394 666 397 668 395 671 395 680 391 680 385 682 382ZM682 369 680 373 685 374 687 371 685 369ZM683 346 682 351 683 354 685 354 685 350ZM398 333 394 333 391 336 392 342 397 341ZM402 321 400 323 400 326 403 328 402 343 406 344 404 347 413 346 417 341 418 335 415 330 407 329 406 328 407 325ZM250 309 253 310 258 309 255 305 251 305ZM387 298 383 296 376 298 372 301 369 300 367 301 369 305 380 305 385 303ZM228 289 227 292 231 293 231 291ZM223 272 221 277 227 280 229 276 226 272ZM176 281 178 284 183 284 188 279 191 286 202 288 197 290 192 290 192 291 201 294 207 293 209 291 216 292 219 291 220 289 215 284 212 277 209 280 197 278 190 272 179 272 179 277ZM246 271 244 283 249 285 258 285 264 283 270 287 277 296 276 302 274 304 268 305 269 307 273 306 280 311 285 312 284 311 290 309 283 302 283 300 286 297 293 302 295 297 293 295 288 294 283 290 286 287 286 285 271 275 260 275 255 270ZM102 302 102 304 108 309 100 313 109 315 109 318 107 320 102 321 101 323 102 328 105 328 111 331 114 330 117 332 116 335 118 335 129 322 130 323 128 326 129 327 137 323 149 326 154 329 167 341 169 346 174 351 178 352 181 355 179 357 180 362 179 381 184 391 188 395 191 396 195 405 197 406 196 404 197 399 208 414 212 417 216 426 226 430 234 431 237 434 249 439 253 443 255 440 255 436 248 434 245 431 247 427 247 422 242 424 241 427 238 429 232 428 228 423 227 416 229 408 235 404 254 403 259 409 259 400 268 391 268 382 269 381 270 383 271 380 275 376 279 375 281 372 281 369 285 365 290 363 291 366 293 366 294 362 290 359 289 353 286 354 285 356 282 355 288 349 294 349 305 345 306 347 302 353 302 356 307 357 311 356 311 353 305 350 307 345 307 340 302 338 303 336 299 336 297 334 295 326 292 321 290 321 287 325 283 325 281 323 281 319 274 314 266 315 269 331 264 337 265 338 264 346 261 346 258 343 255 336 249 335 245 332 239 332 238 329 232 324 239 310 247 308 248 306 244 304 250 302 251 299 256 299 258 296 257 295 258 290 255 287 253 288 252 293 249 297 246 296 245 292 243 292 242 294 234 279 235 275 239 272 238 269 236 269 233 272 232 287 235 289 236 293 232 298 230 297 230 295 228 295 225 298 211 296 209 297 208 301 202 298 196 299 191 294 169 292 160 295 158 297 151 297 143 295 129 295 120 292 114 293 108 296ZM673 264 673 267 677 268 687 266 681 263ZM239 261 240 265 256 265 261 262 261 260 255 261 240 260ZM212 262 206 259 207 263 206 264 202 264 195 261 193 262 192 265 198 266 199 268 203 268 211 265ZM221 259 225 264 228 261 227 258ZM527 256 524 257 525 259 524 260 521 260 518 262 512 273 514 277 516 278 519 277 518 273 519 269 524 260 527 259 528 257ZM193 256 188 257 184 262 191 260ZM769 310 755 301 746 297 737 295 736 298 734 299 727 296 716 295 714 290 712 289 700 288 695 283 681 279 678 284 664 283 660 285 657 283 658 278 656 273 639 275 635 272 630 271 616 272 615 270 627 261 627 257 623 255 615 255 609 250 600 256 579 259 580 265 577 268 568 268 566 270 570 274 569 277 563 275 556 277 555 275 554 276 554 286 558 287 561 291 560 292 557 290 555 292 555 295 549 299 545 297 551 293 551 290 549 287 549 271 548 270 544 270 540 279 545 289 543 291 537 288 533 284 531 284 530 287 527 289 512 289 503 294 499 294 497 291 498 296 490 300 488 303 486 303 484 307 481 307 479 305 478 298 476 295 478 294 487 296 490 294 490 292 484 288 480 288 473 283 470 283 469 280 460 277 457 280 448 284 441 293 435 304 425 312 424 323 430 322 433 318 438 330 441 330 443 325 448 319 446 312 450 308 454 298 458 298 461 301 454 311 455 314 458 317 463 316 468 318 465 320 459 321 460 327 459 328 456 327 454 330 454 333 451 336 447 336 439 339 431 337 429 339 427 338 422 341 422 344 418 346 415 351 407 353 407 356 412 362 412 368 411 369 397 369 396 384 398 386 408 386 412 383 414 376 419 370 429 365 438 374 444 377 443 373 436 366 436 364 439 362 450 372 453 383 455 385 457 384 456 376 462 375 464 377 465 385 470 387 481 387 482 395 477 402 465 402 455 398 450 403 448 403 434 396 432 394 431 387 416 388 408 392 407 391 401 392 400 395 396 397 395 404 388 410 381 423 384 429 382 436 390 450 397 455 421 452 432 458 432 469 437 476 439 484 439 491 436 501 442 512 442 518 448 528 449 534 452 535 462 534 472 526 475 517 479 513 480 502 489 495 489 485 487 476 495 464 502 458 510 444 497 445 493 441 492 438 487 434 479 418 479 415 475 408 476 406 478 405 482 409 495 432 496 438 503 437 518 430 523 426 526 421 521 416 512 418 504 408 507 404 513 410 525 412 528 414 540 414 544 417 546 421 551 421 552 431 555 440 560 447 563 445 565 435 578 422 586 419 592 426 593 432 597 431 599 434 599 437 601 439 602 438 605 439 606 444 613 446 616 445 620 439 618 433 613 427 618 421 623 422 626 420 633 419 639 414 643 407 642 396 639 393 639 391 642 388 638 386 636 383 640 379 643 378 649 380 652 383 653 391 655 393 657 391 657 387 655 383 656 379 662 371 668 371 673 366 679 353 680 344 678 342 674 343 669 338 683 325 695 324 705 326 712 319 724 318 725 319 723 323 708 335 709 346 713 347 719 339 720 335 719 329 727 324 736 324 744 320 751 319 750 314 755 310 758 310 765 314 767 314ZM215 244 215 246 223 250 222 246ZM604 243 605 246 611 244 610 242 606 241ZM587 234 592 239 595 239 598 241 602 240 602 237 598 235 594 230 592 230ZM293 222 268 221 257 224 256 226 243 228 242 230 248 233 254 232 256 233 261 231 263 232 259 235 251 237 255 241 254 243 251 243 243 235 238 232 233 233 232 239 234 241 239 241 240 247 247 245 255 250 254 251 250 250 246 254 250 256 264 255 265 254 263 250 270 243 270 240 287 229ZM387 223 370 224 369 217 352 214 347 214 339 218 329 218 327 219 329 223 328 224 313 222 304 224 296 228 289 233 291 236 290 238 278 244 276 248 283 253 299 254 304 256 310 270 310 279 312 281 315 280 318 283 317 287 312 294 314 302 321 314 326 318 331 318 335 310 335 304 339 300 345 298 352 291 371 281 370 271 376 261 375 235 388 224Z'

# ---------------- الشرائح ----------------
S1 = ('\n    ' + GRID
      + deco(poly([(650, 720), (1280, 90), (1280, 720)], '#101A6F'), TL_COVER,
             line(1130, 285, 1280, 135, '#E6A316'), line(1093, 370, 1243, 220, '#E6A316'),
             line(0, 535, 148, 386, '#3F58E8'), line(30, 463, 73, 421, '#3F58E8'), BR_COVER)
      + '\n    <span class="logo">' + ICONS['logo'] + '</span>'
      + t('logot', '§LOGO§')
      + '\n    <h1 class="t big">§CT1§<br>§CT2§</h1>'
      + '\n    <p class="sbox">§CSUB§</p>'
      + t('date', '§CDATE§') + t('tag', '§CTAG§') + '\n  ')

S2 = ('\n    ' + GRID
      + deco(TL_PALE, poly([(1238, 0), (1280, 0), (1280, 720), (520, 720), (654, 586), (654, 62), (1176, 62)], BLUE))
      + '\n    <img class="photo" src="/templates/freeform/sl-company-introduction/assets/team-meeting.jpg" alt="§ALT2§">'
      + deco(poly([(654, 658), (705, 607), (705, 689), (654, 689)], BLUE), poly([(1250, 62), (1142, 171), (1250, 171)], BLUE),
             poly([(1148, 31), (1235, 31), (1148, 117)], TEAL), poly([(637, 89), (708, 89), (637, 159)], CORAL),
             poly([(526, 696), (584, 696), (584, 637)], GOLD))
      + '\n    <h1 class="ttl">§T2§</h1><p class="sub">§SUB2§</p>'
      + t('p story', '§STORY§') + '<div class="rule"></div>'
      + tri(85, 431, GOLD) + t('num', '§N21§', pos(120, 420)) + t('lbl', '§L21§', pos(120, 472))
      + tri(358, 431, CORAL) + t('num', '§N22§', pos(392, 420)) + t('lbl', '§L22§', pos(392, 472))
      + tri(85, 550, TEAL) + t('num', '§N23§', pos(120, 539)) + t('lbl', '§L23§', pos(120, 590)) + '\n  ')

S3 = ('\n    ' + GRID
      + '\n    <h1 class="ttl">§T3§</h1><p class="sub">§SUB3§</p>'
      + card(73, 241, 361, 412, 'fill', '#F2F2F2', 58, (9, 9, 64), GOLD).replace('fill="#F2F2F2"/><rect', 'fill="#F2F2F2"/><rect', 1)
      + card(459, 241, 361, 412, 'fill', '#F2F2F2', 58, (9, 9, 64), CORAL)
      + card(846, 75, 361, 578, 'fill', BLUE, 60, (10, 8, 71), TEAL)
      + ic('buildings', 125, 320, 62, 48, '#BFBFBF') + t('h', '§VIS§', pos(121, 408)) + t('p', '§VISP§', pos(121, 456, 268))
      + ic('target', 513, 315, 60, 60, '#BFBFBF') + t('h', '§MIS§', pos(507, 408)) + t('p', '§MISP§', pos(507, 456, 268))
      + t('cv', '§CV§') + '<div class="cvrule"></div>'
      + '<ul class="t vals"><li>§V1§</li><li>§V2§</li><li>§V3§</li><li>§V4§</li></ul>\n  ')
# إصلاح لون مثلث البطاقتين الرماديتين: يأخذ لون الشريط السفلي
S3 = S3.replace('fill="#F2F2F2"/><rect x="0" y="407" width="361" height="5" fill="%s"/><polygon points="9,9 73,9 9,73" fill="#F2F2F2"' % GOLD,
                'fill="#F2F2F2"/><rect x="0" y="407" width="361" height="5" fill="%s"/><polygon points="9,9 73,9 9,73" fill="%s"' % (GOLD, GOLD))
S3 = S3.replace('fill="#F2F2F2"/><rect x="0" y="407" width="361" height="5" fill="%s"/><polygon points="9,9 73,9 9,73" fill="#F2F2F2"' % CORAL,
                'fill="#F2F2F2"/><rect x="0" y="407" width="361" height="5" fill="%s"/><polygon points="9,9 73,9 9,73" fill="%s"' % (CORAL, CORAL))

YX = [84, 376, 668, 961]
S4 = ('\n    ' + GRID
      + deco(poly([(1205, 0), (1280, 0), (1280, 181), (1205, 108)], '#EEF0FF'),
             poly([(1071, 0), (1139, 0), (1204, 65), (1204, 135)], TEAL), poly([(1144, 0), (1228, 0), (1228, 84)], GOLD),
             '<rect x="0" y="238" width="1176" height="110" fill="url(#arrowg)"/>',
             poly([(1175, 205), (1263, 293), (1175, 381)], BLUE),
             '<rect x="640" y="220" width="518" height="3" fill="url(#lineg)"/><rect x="640" y="362" width="518" height="3" fill="url(#lineg)"/>')
      + '\n    <h1 class="ttl">§T4§</h1><p class="sub">§SUB4§</p>'
      + ''.join(t('yr', '§Y%d§' % (i + 1), 'inset-inline-start:%dpx' % x) for i, x in enumerate(YX))
      + ic('tower', 88, 400, 40, 56) + ic('globe', 378, 403, 52, 52) + ic('megaphone', 671, 404, 50, 48) + ic('city', 960, 405, 56, 44)
      + ''.join(t('h', '§M%dH§' % (i + 1), 'inset-inline-start:%dpx' % x) + t('p', '§MP§', 'inset-inline-start:%dpx' % x) for i, x in enumerate(YX))
      + '\n  ')

S5 = ('\n    ' + GRID
      + deco(poly([(117, 490), (117, 720), (0, 720), (0, 607)], '#FCF5E3'), poly([(70, 603), (70, 720), (0, 720), (0, 673)], '#5064E6'),
             poly([(152, 568), (152, 720), (0, 720)], TEAL, ' fill-opacity=".9"'))
      + '\n    <h1 class="ttl">§T5§</h1><p class="sub" style="top:205px">§SUB5§</p>'
      + t('p', '§P5§', pos(84, 292, 335))
      + card(513, 37, 362, 330, 'outline', GOLD, 57, (10, 9, 57)) + card(895, 0, 361, 330, 'outline', TEAL, 57, (9, 10, 57))
      + card(513, 379, 362, 341, 'outline', BLUE, 57, (10, 24, 57)) + card(895, 341, 361, 339, 'outline', CORAL, 57, (9, 22, 57))
      + t('no', '01', pos(569, 86) + 'color:' + GOLD) + t('h', '§S1H§', pos(569, 146)) + t('p', '§SP§', pos(569, 228))
      + t('no', '02', pos(951, 54) + 'color:' + TEAL) + t('h', '§S2H§', pos(951, 109)) + t('p', '§SP§', pos(951, 191))
      + t('no', '03', pos(569, 461) + 'color:' + BLUE) + t('h', '§S3H§', pos(569, 526)) + t('p', '§SP§', pos(569, 571))
      + t('no', '04', pos(951, 424) + 'color:' + CORAL) + t('h', '§S4H§', pos(951, 492)) + t('p', '§SP§', pos(951, 537)) + '\n  ')

CX6 = [(45, GOLD, 1), (459, TEAL, 2), (874, BLUE, 3)]
S6 = ('\n    ' + GRID
      + deco(poly([(1064, 0), (1130, 0), (1280, 150), (1280, 216)], '#FCF5E3'), poly([(1160, 27), (1203, 27), (1280, 104), (1280, 147)], '#5064E6'),
             poly([(1181, 0), (1280, 0), (1280, 99)], '#1FDDAA'))
      + '\n    <h1 class="ttl">§T6§</h1><p class="sub">§SUB6§</p>'
      + ''.join(card(x, 241, 361, 479, 'outline', c, 57, (9, 10, 58))
                + '<img class="face" style="inset-inline-start:%dpx" src="/templates/freeform/sl-company-introduction/assets/leader-%d.jpg" alt="§NM%d§">' % (x + 41, n, n)
                + t('nm', '§NM%d§' % n, 'inset-inline-start:%dpx' % (x + 55)) + t('role', '§RL%d§' % n, 'inset-inline-start:%dpx' % (x + 55))
                + t('p', '§P6§', 'inset-inline-start:%dpx' % (x + 55)) for x, c, n in CX6) + '\n  ')

S7 = ('\n    ' + GRID + deco(TL_PALE)
      + '\n    <h1 class="ttl">§T7§</h1><p class="sub">§SUB7§</p>'
      + ic('checklist', 88, 247, 32, 40) + ic('network', 335, 247, 44, 40) + ic('handshake', 586, 252, 44, 28)
      + t('num', '120+', pos(84, 312)) + t('lbl', '§L71§', pos(84, 363))
      + t('num', '35+', pos(335, 312)) + t('lbl', '§L72§', pos(335, 363))
      + t('num', '92%', pos(585, 312)) + t('lbl', '§L73§', pos(585, 363))
      + '<span class="bar" style="inset-inline-start:293px;background:%s"></span><span class="bar" style="inset-inline-start:523px;background:%s"></span>' % (GOLD, TEAL)
      + t('h', '§H7§') + t('p', '§P7§')
      + card(812, 91, 426, 581, 'fill', BLUE, 59, (9, 8, 69), TEAL).replace('height="5"', 'height="6"')
      + deco(poly([(1194, 685), (1252, 685), (1252, 627)], GOLD),
             poly([(872, 192), (890, 192), (880, 217), (862, 217)], '#fff') + poly([(894, 192), (912, 192), (902, 217), (884, 217)], '#fff'))
      + t('q', '§Q7§') + '<div class="qrule"></div>' + t('who', '<b>§QN§</b><br>§QR§') + '\n  ')

S8 = ('\n    ' + GRID
      + deco(poly([(1170, 0), (1280, 0), (1280, 266), (1170, 156)], '#EEF0FF'),
             poly([(977, 0), (1076, 0), (1170, 94), (1170, 137)], TEAL), poly([(1081, 0), (1208, 0), (1208, 127)], GOLD))
      + '\n    <h1 class="ttl">§T8§</h1><p class="sub">§SUB8§</p>'
      + '\n    <svg class="map" viewBox="90 200 710 400" aria-hidden="true"><path class="land" d="' + MAP_D + '"/>'
      + '<circle cx="274" cy="369" r="7.5"/><circle cx="410" cy="337" r="7.5"/><circle cx="500" cy="401" r="7.5"/><circle cx="612" cy="438" r="7.5"/></svg>'
      + t('km', '§KM§') + t('kml', '§KML§')
      + card(870, 137, 360, 524, 'fill', '#F2F2F2', 60, (9, 10, 64), CORAL)
      + ''.join(tri(925, 208 + 111 * i, c) + t('num', n, pos(960, 198 + 111 * i)) + t('lbl', '§L8%d§' % (i + 1), pos(960, 249 + 111 * i))
                for i, (c, n) in enumerate([(CORAL, '1.325+'), (GOLD, '190'), (TEAL, '2.510'), (BLUE, '12.023+')]))
      + '\n  ')
S8 = S8.replace('<polygon points="9,10 73,10 9,74" fill="#F2F2F2"/>', '<polygon points="9,10 73,10 9,74" fill="%s"/>' % CORAL)

S9 = ('\n    ' + GRID
      + deco(poly([(1115, 0), (1240, 0), (1240, 72), (1044, 72)], '#FCF5E3'), poly([(1186, 0), (1265, 0), (1239, 26), (1160, 26)], '#5064E6'),
             poly([(1234, 0), (1280, 0), (1280, 72), (1162, 72)], '#20DCA8'), poly([(1162, 72), (1280, 72), (1280, 110), (1124, 110)], TEAL))
      + '\n    <h1 class="ttl">§T9§</h1><p class="sub">§SUB9§</p>'
      + t('rg', '§RG§')
      + '\n    <svg class="chart" id="rev-chart" viewBox="60 290 460 410" direction="ltr" style="direction:ltr" role="img" aria-label="§RG§"></svg>'
      + '\n    <table><colgroup><col style="width:278px"><col style="width:129px"><col style="width:128px"><col></colgroup>'
      + '<thead><tr><th>§TM§</th><th>2026</th><th>2028</th><th>2030</th></tr></thead><tbody>'
      + '<tr><td>§TR1§</td><td>8.4</td><td>10.2</td><td>12.6</td></tr>'
      + '<tr><td>§TR2§</td><td>1.8</td><td>2.3</td><td>3.1</td></tr>'
      + '<tr><td>§TR3§</td><td>28</td><td>34</td><td>41</td></tr>'
      + '<tr><td>§TR4§</td><td>76</td><td>98</td><td>124</td></tr></tbody></table>'
      + card(582, 488, 650, 232, 'outline', GOLD, 57, (9, 8, 59))
      + '<div class="t kpi"><b>+23%</b><span>§RG2§</span></div><span class="gbar"></span>' + t('p', '§P9§') + '\n  ')

S10 = ('\n    ' + GRID
       + deco(poly([(684, 720), (1280, 124), (1280, 720)], '#F4F7FE'), TL_COVER,
              line(1060, 86, 1146, 0, '#E6A316'), line(1024, 170, 1174, 20, '#E6A316'),
              line(922, 720, 1010, 632, '#4B5FE0'), line(968, 720, 992, 697, '#4B5FE0'), BR_COVER)
       + '\n    <h1 class="t end">§END1§<br>§END2§</h1>'
       + '<p class="btn">§EDIT§</p>'
       + ic('mail', 128, 569, 26, 20, '#404040') + t('ct', '§MAIL§', 'top:567px')
       + ic('phone', 128, 618, 27, 27, '#000') + t('ct', '<span dir="ltr">+1 (234) 567 8901</span>', 'top:620px') + '\n  ')

SLIDES = [S1, S2, S3, S4, S5, S6, S7, S8, S9, S10]

JSON_BLOCKS = {'chart-data': {'labels': ['2026', '2028', '2030'], 'values': [8.4, 10.2, 12.6], 'max': 14, 'step': 2}}

EXTRA_SCRIPT = """  // رسم أعمدة نمو الإيرادات من كتلة chart-data (الفئة الأولى في جهة البداية)
  function drawChart() {
    var el = document.getElementById('rev-chart'); if (!el) return;
    var d = JSON.parse(document.getElementById('chart-data').textContent);
    var X0 = 60, X1 = 520, Y0 = 664, YT = 308, sy = (Y0 - YT) / d.max, s = '';
    function px(x) { return RTL ? X0 + X1 - x : x; }
    for (var v = 0; v <= d.max; v += d.step) {
      var y = Y0 - v * sy;
      s += '<line x1="' + px(112) + '" x2="' + px(512) + '" y1="' + y + '" y2="' + y + '" stroke="' + (v ? '#E3E3E3' : '#D0D0D0') + '"' + (v ? ' stroke-dasharray="5 4"' : '') + '/>';
      s += '<text x="' + px(98) + '" y="' + (y + 5) + '" text-anchor="' + (RTL ? 'start' : 'end') + '" font-size="14" fill="#595959">' + v + '</text>';
    }
    var cw = 400 / d.values.length;
    d.values.forEach(function (v, i) {
      var cx = px(112 + (i + .5) * cw), h = v * sy;
      s += '<rect x="' + (cx - 42.5) + '" y="' + (Y0 - h) + '" width="85" height="' + h + '" fill="#3C59FF"/>';
      s += '<text x="' + cx + '" y="' + (Y0 - h - 12) + '" text-anchor="middle" font-size="18" font-weight="700" fill="#404040">' + v + '</text>';
      s += '<text x="' + cx + '" y="' + 690 + '" text-anchor="middle" font-size="14" fill="#595959">' + d.labels[i] + '</text>';
    });
    el.innerHTML = s;
  }
  drawChart();"""

SP = 'This is a sample text. Add your desired text here. Add text here. This is a sample text. This is a sample text.'
SP_AR = 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصًا هنا. هذا نص توضيحي. هذا نص توضيحي.'
STR = {
    'en': {
        'LOGO': 'YOUR COMPANY<br>LOGO HERE', 'CT1': 'COMPANY', 'CT2': 'INTRODUCTION', 'CSUB': 'PRESENTATION TEMPLATE',
        'CDATE': 'Mar 2040', 'CTAG': 'Add company tagline here',
        'T2': 'ABOUT US', 'SUB2': 'Who we are', 'ALT2': 'Team meeting in a modern office',
        'STORY': 'Company story here. This is a sample text. Add your desired text here. Add text here. This is a sample text. Add text here. Add your desired text here',
        'N21': '18+ Years', 'L21': 'Industry Experience', 'N22': '32 Markets', 'L22': 'Industry Experience', 'N23': '850+', 'L23': 'Projects Delivered',
        'T3': 'VISION &amp; MISSION', 'SUB3': 'Add subheading here', 'VIS': 'VISION', 'MIS': 'MISSION',
        'VISP': 'Add company vision here. This is a sample text. Add your desired text here. Add text here. This is a sample text. Add text here. Add your desired text here.',
        'MISP': 'Add company mission here. This is a sample text. Add your desired text here. Add text here. This is a sample text. Add text here. Add your desired text here.',
        'CV': 'CORE<br>VALUES', 'V1': '01 · Integrity', 'V2': '02 · Innovation', 'V3': '03 · Excellence', 'V4': '04 · Partnership',
        'T4': 'KEY MILESTONES', 'SUB4': 'Our Journey', 'Y1': '2009', 'Y2': '2014', 'Y3': '2025', 'Y4': '2037',
        'M1H': 'COMPANY<br>FOUNDED', 'M2H': 'MARKET<br>EXPANSION', 'M3H': 'DIGITAL<br>TRANSFORMATION', 'M4H': 'THE NEXT<br>CHAPTER',
        'MP': 'This is a sample text. Add your desired text here. Add text here. This is a sample text.',
        'T5': 'OUR<br>SERVICES', 'SUB5': 'What we do',
        'P5': 'This is a sample text. Add your desired text here. Add text here. This is a sample text. Add text here. Add your desired text here',
        'S1H': 'BUSINESS<br>STRATEGY', 'S2H': 'TECHNOLOGY<br>SOLUTIONS', 'S3H': 'OPERATIONS', 'S4H': 'ADVISORY', 'SP': SP,
        'T6': 'LEADERSHIP TEAM', 'SUB6': 'Our Journey', 'NM1': 'EMMA CARTER', 'NM2': 'DANIEL BROOK', 'NM3': 'JAMES MARS',
        'RL1': 'Chief Executive Officer', 'RL2': 'Chief Commercial Officer', 'RL3': 'Head of Innovation',
        'P6': 'This is a sample text. Add text here. This is a sample text. Add your text here.',
        'T7': 'CLIENT PORTFOLIO', 'SUB7': 'Our Journey', 'L71': 'Projects delivered', 'L72': 'Active clients', 'L73': 'Client retention',
        'H7': 'STRONG RELATIONSHIPS ARE<br>BUILT THROUGH CONSISTENT RESULTS.',
        'P7': 'This is a sample text. Add your desired text here. Add text here. This is a sample text. Add your desired text here. Add text here. This is a sample text. Add your desired text here. Add text here. This is a sample text.',
        'Q7': 'They brought clarity to a complex challenge, delivered with confidence, and went beyond our expectations.',
        'QN': 'Sarah Johnson', 'QR': 'Head of Marketing, GlobalTech',
        'T8': 'MARKET PRESENCE', 'SUB8': 'Our Journey', 'KM': 'KEY MARKETS', 'KML': 'Southeast Asia · Asia Pacific · Europe · North America',
        'L81': 'Cities', 'L82': 'Countries', 'L83': 'Industry Segments', 'L84': 'Active Clients',
        'T9': 'FINANCIAL SNAPSHOT', 'SUB9': 'Our Journey', 'RG': 'REVENUE GROWTH', 'TM': 'Metric',
        'TR1': 'Revenue', 'TR2': 'EBITDA', 'TR3': 'Active Clients', 'TR4': 'Projects Delivered', 'RG2': 'Revenue Growth',
        'P9': 'This is a sample text. Add your desired text here. Add text here. This is a sample text. This is a sample text.',
        'END1': 'Thank You for Learning About', 'END2': 'Our Company', 'EDIT': 'EDIT HERE', 'MAIL': 'Insert your email here',
    },
    'ar': {
        'LOGO': 'ضع شعار<br>شركتك هنا', 'CT1': 'التعريف', 'CT2': 'بالشركة', 'CSUB': 'قالب عرض تقديمي',
        'CDATE': 'مارس 2040', 'CTAG': 'أضف الشعار النصي للشركة هنا',
        'T2': 'من نحن', 'SUB2': 'تعرّف علينا', 'ALT2': 'اجتماع فريق في مكتب حديث',
        'STORY': 'قصة الشركة هنا. هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصًا هنا. هذا نص توضيحي. أضف نصًا هنا. أضف النص المطلوب هنا',
        'N21': '18+ عامًا', 'L21': 'خبرة في القطاع', 'N22': '32 سوقًا', 'L22': 'خبرة في القطاع', 'N23': '850+', 'L23': 'مشروعًا منجزًا',
        'T3': 'الرؤية والرسالة', 'SUB3': 'أضف عنوانًا فرعيًا هنا', 'VIS': 'الرؤية', 'MIS': 'الرسالة',
        'VISP': 'أضف رؤية الشركة هنا. هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصًا هنا. هذا نص توضيحي. أضف نصًا هنا. أضف النص المطلوب هنا.',
        'MISP': 'أضف رسالة الشركة هنا. هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصًا هنا. هذا نص توضيحي. أضف نصًا هنا. أضف النص المطلوب هنا.',
        'CV': 'القيم<br>الجوهرية', 'V1': '01 · النزاهة', 'V2': '02 · الابتكار', 'V3': '03 · التميّز', 'V4': '04 · الشراكة',
        'T4': 'أبرز المحطات', 'SUB4': 'مسيرتنا', 'Y1': '2009', 'Y2': '2014', 'Y3': '2025', 'Y4': '2037',
        'M1H': 'تأسيس<br>الشركة', 'M2H': 'التوسع<br>في الأسواق', 'M3H': 'التحول<br>الرقمي', 'M4H': 'الفصل<br>التالي',
        'MP': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصًا هنا. هذا نص توضيحي.',
        'T5': 'خدمات<br>شركتنا', 'SUB5': 'ماذا نقدّم',
        'P5': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصًا هنا. هذا نص توضيحي. أضف نصًا هنا. أضف النص المطلوب هنا',
        'S1H': 'استراتيجية<br>الأعمال', 'S2H': 'الحلول<br>التقنية', 'S3H': 'العمليات', 'S4H': 'الاستشارات', 'SP': SP_AR,
        'T6': 'فريق القيادة', 'SUB6': 'مسيرتنا', 'NM1': 'إيما كارتر', 'NM2': 'دانيال بروك', 'NM3': 'جيمس مارس',
        'RL1': 'الرئيسة التنفيذية', 'RL2': 'الرئيس التجاري', 'RL3': 'رئيس الابتكار',
        'P6': 'هذا نص توضيحي. أضف نصًا هنا. هذا نص توضيحي. أضف نصك هنا.',
        'T7': 'محفظة العملاء', 'SUB7': 'مسيرتنا', 'L71': 'مشروعًا منجزًا', 'L72': 'عميلًا نشطًا', 'L73': 'نسبة الاحتفاظ بالعملاء',
        'H7': 'العلاقات القوية تُبنى<br>بنتائج ثابتة ومتواصلة.',
        'P7': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصًا هنا. هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصًا هنا. هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصًا هنا. هذا نص توضيحي.',
        'Q7': 'أضفوا الوضوح على تحدٍّ معقّد، ونفّذوا بثقة، وتجاوزوا توقعاتنا.',
        'QN': 'سارة جونسون', 'QR': 'رئيسة التسويق، جلوبال تك',
        'T8': 'حضورنا في الأسواق', 'SUB8': 'مسيرتنا', 'KM': 'الأسواق الرئيسية', 'KML': 'جنوب شرق آسيا · آسيا والمحيط الهادئ · أوروبا · أمريكا الشمالية',
        'L81': 'مدينة', 'L82': 'دولة', 'L83': 'قطاعًا صناعيًا', 'L84': 'عميلًا نشطًا',
        'T9': 'لمحة مالية', 'SUB9': 'مسيرتنا', 'RG': 'نمو الإيرادات', 'TM': 'المؤشر',
        'TR1': 'الإيرادات', 'TR2': 'الأرباح قبل الاستقطاعات', 'TR3': 'العملاء النشطون', 'TR4': 'المشاريع المنجزة', 'RG2': 'نمو الإيرادات',
        'P9': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصًا هنا. هذا نص توضيحي. هذا نص توضيحي.',
        'END1': 'شكرًا لتعرّفكم', 'END2': 'على شركتنا', 'EDIT': 'عدّل هنا', 'MAIL': 'أدخل بريدك الإلكتروني هنا',
    },
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'general', 'icon': '🏢',
    'name': 'التعريف بالشركة', 'nameEn': 'Company Introduction',
    'description': 'عشر شرائح: غلاف، من نحن، الرؤية والرسالة، المحطات، الخدمات، القيادة، العملاء، الأسواق، لمحة مالية، ختام',
    'descriptionEn': 'Ten slides: cover, about, vision, milestones, services, leaders, clients, markets, financials, close',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'من نحن', 'titleEn': 'About Us', 'needsData': True},
        {'title': 'الرؤية والرسالة والقيم', 'titleEn': 'Vision & Mission'},
        {'title': 'أبرز المحطات', 'titleEn': 'Key Milestones'},
        {'title': 'خدماتنا', 'titleEn': 'Our Services'},
        {'title': 'فريق القيادة', 'titleEn': 'Leadership Team'},
        {'title': 'محفظة العملاء', 'titleEn': 'Client Portfolio', 'needsData': True},
        {'title': 'الحضور في الأسواق', 'titleEn': 'Market Presence', 'needsData': True},
        {'title': 'لمحة مالية (رسم وجدول)', 'titleEn': 'Financial Snapshot (chart & table)', 'needsData': True},
        {'title': 'الختام والتواصل', 'titleEn': 'Thank You & Contact'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والزخارف وغيّر النصوص والأرقام فقط؛ أعمدة نمو الإيرادات تُرسم من كتلة chart-data، وأرقام الجدول والمؤشرات نصوص في مكانها.',
}
