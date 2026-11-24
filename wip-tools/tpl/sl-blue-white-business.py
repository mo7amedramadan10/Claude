# -*- coding: utf-8 -*-
# نموذج «أعمال بالأزرق والأبيض»: إحدى عشرة شريحة بأسلوب شركات — غلاف بأشرطة مائلة ودائرة متدرّجة، ترحيب بصورة في دائرة،
# مقدمة، نبذة بمؤشر نسبة، رؤية ورسالة، خدمات، ماذا نقدّم، الفريق، مشاريع مميزة، معرض أعمال، وتواصل.
KEY = 'sl-blue-white-business'
TITLE = {'ar': 'عرض أعمال بالأزرق والأبيض', 'en': 'Blue & White Business Presentation'}
COMMENT = 'نموذج «أعمال بالأزرق والأبيض»: خلفية بيضاء مع كتل زرقاء (#3463FB) مائلة ومتدرّجة، حلقات رمادية رفيعة وشبكات نقاط زخرفية، صور معمارية مقصوصة بأشكال مائلة أو مدوّرة، وتذييل «أزرق وأبيض» برقم الشريحة. الزخارف والصور المائلة تنعكس في العربية.'
VARS = '--ink: #262626; --tx: #404040; --accent: #3463FB; --navy: #153156; --gray: #F2F2F2; --muted: #B3B3B3; --pnum: #7C9BFB; --bg: #FFFFFF;'

A = '/templates/freeform/sl-blue-white-business/assets/'

FONTS = {'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@400;600;700&display=swap" rel="stylesheet">'}
FONT_STACK = {'en': "'Open Sans', 'Segoe UI', Arial, sans-serif"}


# ---------- أدوات الزخرفة ----------
def inside(x, y, poly):
    """هل النقطة داخل المضلّع (لتلوين النقاط فوق الكتل الزرقاء بالأبيض)؟"""
    c = False
    for i in range(len(poly)):
        x1, y1 = poly[i]; x2, y2 = poly[i - 1]
        if (y1 > y) != (y2 > y) and x < (x2 - x1) * (y - y1) / (y2 - y1) + x1:
            c = not c
    return c


def dots(x0, y0, cols, rows, gap=25, r=2.6, blue=None, col='#D9D9D9'):
    out = []
    for j in range(rows):
        for i in range(cols):
            x, y = x0 + i * gap, y0 + j * gap
            f = '#FFFFFF' if (blue and any(inside(x, y, p) for p in blue)) else col
            out.append('<circle cx="%d" cy="%d" r="%s" fill="%s"/>' % (x, y, r, f))
    return ''.join(out)


def rings(cx, cy, radii, col='#E3E3E3', clip=None):
    if clip:  # clip = (id, x, y, w, h)
        cid, x, y, w, h = clip
        return ('<clipPath id="%s"><rect x="%d" y="%d" width="%d" height="%d"/></clipPath><g clip-path="url(#%s)">' % (cid, x, y, w, h, cid)
                + rings(cx, cy, radii, col) + '</g>')
    return ''.join('<circle cx="%d" cy="%d" r="%d" fill="none" stroke="%s" stroke-width="1.3"/>' % (cx, cy, r, col) for r in radii)


def poly(pts):
    return ' '.join('%s,%s' % p for p in pts)


def svg(cls, body):
    return '<svg class="dec %s" viewBox="0 0 1280 720" aria-hidden="true">%s</svg>' % (cls, body)


def footer(x, n, bar=False):
    b = '<div class="fbar"></div>' if bar else ''
    return b + '<p class="ft" style="inset-inline-start:%dpx"><b>§FB§</b> §FT§</p><span class="pn">%d</span>' % (x, n)


# أيقونات خطية (stroke) بـ currentColor
def ic(body, sw=1.4):
    return '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="%s" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">%s</svg>' % (sw, body)


ICONS = {
    'people': ic('<circle cx="12" cy="8" r="2.6"/><circle cx="5.5" cy="9.5" r="2.1"/><circle cx="18.5" cy="9.5" r="2.1"/><path d="M7.4 17.5c0-2.6 2-4.4 4.6-4.4s4.6 1.8 4.6 4.4"/><path d="M1.6 17c0-2.2 1.7-3.8 3.9-3.8 1 0 1.8.3 2.5.8"/><path d="M22.4 17c0-2.2-1.7-3.8-3.9-3.8-1 0-1.8.3-2.5.8"/>', 1.1),
    'chart': ic('<path d="M3 3v18h18"/><rect x="6" y="13" width="3" height="6"/><rect x="11" y="9.5" width="3" height="9.5"/><rect x="16" y="6" width="3" height="13"/><path d="M5 11l5-5 3 2 6-5"/><path d="M16.5 3h2.5v2.5"/>', 1.1),
    'eye': ic('<path d="M2 12s3.6-6.5 10-6.5S22 12 22 12s-3.6 6.5-10 6.5S2 12 2 12z"/><circle cx="12" cy="12" r="3.2"/><circle cx="12" cy="12" r="1.1"/>'),
    'rocket': ic('<path d="M14.5 4.2c2.4-1.4 4.6-1.6 5.4-.8.8.8.6 3-.8 5.4-1.6 2.7-4.5 5.6-7.6 7.4l-4-4c1.8-3.1 4.3-6.4 7-8z"/><circle cx="15.3" cy="8.7" r="1.6"/><path d="M8.6 11.3L5 11l-2 2.4 3.6.8"/><path d="M12.7 15.4l.3 3.6-2.4 2-.8-3.6"/><path d="M6.4 15.6c-1.4.4-2.4 1.8-2.6 4.6 2.8-.2 4.2-1.2 4.6-2.6"/>'),
    'mixer': ic('<rect x="2" y="10.5" width="13" height="5" rx=".6"/><path d="M15 9h3.5l2.5 3v3.5h-6z"/><path d="M3 10.5l1.5-4.5c.6-1.8 2.4-2.8 4.3-2.4l4.6 1c1.5.3 2.3 1.9 1.7 3.3l-1 2.6"/><path d="M6 4.2l2.2 6.3M9.8 4.6l1.4 5.9"/><circle cx="5.5" cy="17.5" r="1.7"/><circle cx="17.5" cy="17.5" r="1.7"/>'),
    'excavator': ic('<rect x="2.5" y="16" width="13" height="4" rx="2"/><circle cx="5" cy="18" r=".7"/><circle cx="9" cy="18" r=".7"/><circle cx="13" cy="18" r=".7"/><path d="M4 16v-4.5h8.5V16"/><path d="M6 11.5V8h4.5l2 3.5"/><path d="M12.5 11.5l3.5-5.5 4.5 2.5-.5 4.5"/><path d="M20 13c.8.2 1.5 1 1.5 2-1.2.3-2.4-.2-3-1.2z"/>'),
    'crane': ic('<path d="M7 21V5M10 21V5M7 5l3-2M7 5h15M10 5l12 0M7 9h3M7 13h3M7 17h3M7 9l3 4M10 9l-3 4M7 17l3-4"/><path d="M4 5h3M4 5v2M18 5v6"/><rect x="16.2" y="11" width="3.6" height="2.8"/><path d="M5 21h7"/>'),
    'building': ic('<rect x="6" y="2.5" width="12" height="19"/><path d="M8.5 5.5h1.5M11.3 5.5h1.5M14 5.5h1.5M8.5 8.5h1.5M11.3 8.5h1.5M14 8.5h1.5M8.5 11.5h1.5M11.3 11.5h1.5M14 11.5h1.5M8.5 14.5h1.5M11.3 14.5h1.5M14 14.5h1.5"/><path d="M10.5 21.5v-4h3v4M3.5 21.5h17"/>', 1.1),
    'gears': ic('<circle cx="15" cy="7.5" r="2"/><path d="M15 2.5v1.6M15 10.9v1.6M10 7.5h1.6M18.4 7.5H20M11.5 4l1.1 1.1M17.4 9.9l1.1 1.1M11.5 11l1.1-1.1M17.4 5.1L18.5 4"/><circle cx="15" cy="7.5" r="3.4"/><circle cx="8" cy="15.5" r="2"/><circle cx="8" cy="15.5" r="3.4"/><path d="M8 10.5v1.6M8 18.9v1.6M3 15.5h1.6M11.4 15.5H13M4.5 12l1.1 1.1M10.4 17.9l1.1 1.1M4.5 19l1.1-1.1M10.4 13.1l1.1-1.1"/>', 1.1),
    'user': ic('<circle cx="12" cy="7.5" r="4.2"/><path d="M4 21v-1.8C4 15.8 7.6 13.5 12 13.5s8 2.3 8 5.7V21z"/>', 1.1),
    'check': ic('<path d="M7.5 12.4l3 3 6-6.2"/>', 2.2),
    'pin': ic('<path d="M12 21s-6-5.6-6-10.5a6 6 0 0 1 12 0C18 15.4 12 21 12 21z"/><circle cx="12" cy="10.3" r="2.1"/>'),
    'mail': ic('<rect x="3" y="5.5" width="18" height="13" rx="1.2"/><path d="M3.5 6.2l8.5 7 8.5-7M3.5 18l6.5-6M20.5 18l-6.5-6"/>'),
    'phone': ic('<path d="M6.6 3.5l2.6 3.7-1.8 2.2c1.2 2.6 3.2 4.6 5.8 5.8l2.2-1.8 3.7 2.6-1.4 3.2c-.3.6-1 1-1.7.9C9.9 19.4 4.6 14.1 3.9 7.1c-.1-.7.3-1.4.9-1.7z"/>'),
}

STYLE = """  /* أنماط مشتركة */
  .dec { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; pointer-events: none; }
  html[dir="rtl"] .dec, html[dir="rtl"] .flip { transform: scaleX(-1); }
  .ph { position: absolute; display: block; object-fit: cover; }
  .t { position: absolute; margin: 0; font-size: 44px; font-weight: 400; line-height: 54px; color: var(--ink); white-space: nowrap; }
  .t b { font-weight: 700; }
  html[lang="ar"] .t { font-size: 46px; line-height: 58px; } html[lang="ar"] .t b { font-weight: 800; }
  .p { position: absolute; margin: 0; font-size: 17px; line-height: 25px; color: var(--tx); }
  html[lang="ar"] .p { font-size: 18px; line-height: 27px; }
  .ft { position: absolute; top: 645px; margin: 0; font-size: 17px; line-height: 24px; color: var(--muted); white-space: nowrap; }
  .ft b { color: var(--accent); font-weight: 700; }
  .pn { position: absolute; top: 632px; inset-inline-end: 65px; font-size: 40px; line-height: 48px; font-weight: 700; color: var(--pnum); }
  .fbar { position: absolute; top: 655px; inset-inline-start: 0; width: 418px; height: 65px; background: linear-gradient(90deg, #0A2A98, #3463FB); clip-path: polygon(0 0, 100% 0, calc(100% - 20px) 100%, 0 100%); }
  html[dir="rtl"] .fbar { background: linear-gradient(270deg, #0A2A98, #3463FB); clip-path: polygon(0 0, 100% 0, 100% 100%, 20px 100%); }
  .h { margin: 0; font-size: 29px; font-weight: 700; line-height: 38px; color: var(--ink); }
  html[lang="ar"] .h { font-size: 30px; font-weight: 800; line-height: 40px; }

  /* 1) الغلاف */
  .s1 .vbar { position: absolute; top: 262px; inset-inline-start: 54px; width: 8px; height: 190px; border-radius: 4px; background: linear-gradient(180deg, #3463FB, #DCE3FF); }
  .s1 .ct { position: absolute; top: 250px; inset-inline-start: 102px; margin: 0; font-size: 79px; line-height: 78px; font-weight: 700; color: var(--navy); text-transform: uppercase; letter-spacing: .5px; white-space: nowrap; }
  html[lang="ar"] .s1 .ct { top: 238px; font-size: 76px; line-height: 92px; font-weight: 800; }
  .s1 .cs { position: absolute; top: 444px; inset-inline-start: 104px; margin: 0; font-size: 37px; line-height: 44px; font-weight: 400; color: var(--accent); letter-spacing: 3.4px; text-transform: uppercase; white-space: nowrap; }
  html[lang="ar"] .s1 .cs { letter-spacing: 0; font-size: 36px; font-weight: 500; }
  .s1 .uln { position: absolute; top: 512px; inset-inline-start: 106px; width: 110px; height: 4px; border-radius: 2px; background: linear-gradient(90deg, #3463FB, #DCE3FF); }
  html[dir="rtl"] .s1 .uln { background: linear-gradient(270deg, #3463FB, #DCE3FF); }
  .s1 .logo { position: absolute; top: 577px; inset-inline-start: 87px; width: 56px; height: 62px; }
  .s1 .lt { position: absolute; top: 587px; inset-inline-start: 166px; margin: 0; font-size: 17px; line-height: 23px; letter-spacing: 1.6px; color: #555; text-transform: uppercase; }
  html[lang="ar"] .s1 .lt { letter-spacing: 0; top: 584px; line-height: 26px; }

  /* 2) الترحيب */
  .s2 .t { top: 70px; inset-inline-start: 72px; }
  .s2 .p { inset-inline-start: 73px; width: 385px; } .s2 .pa { top: 204px; } .s2 .pb { top: 279px; }
  .s2 .man { top: 40px; inset-inline-start: 665px; width: 505px; height: 625px; clip-path: path('M4,372 A248,248 0 0,1 500,372 A248,248 0 0,1 4,372 Z M200,0 H350 V170 H200 Z'); }
  .s2 .pill { position: absolute; top: 413px; inset-inline-start: 127px; width: 686px; height: 147px; border-radius: 74px; background: var(--gray); }
  .s2 .nm { position: absolute; top: 452px; inset-inline-start: 186px; margin: 0; font-size: 32px; line-height: 40px; font-weight: 700; color: #111; white-space: nowrap; }
  .s2 .rl { position: absolute; top: 497px; inset-inline-start: 187px; margin: 0; font-size: 20px; line-height: 26px; color: #111; }
  .s2 .pc { top: 450px; inset-inline-start: 400px; width: 355px; }
  html[lang="ar"] .s2 .nm { font-weight: 800; font-size: 30px; }

  /* 3) المقدمة */
  .s3 .bld { top: 0; inset-inline-start: 0; width: 875px; height: 720px; clip-path: polygon(345px 0, 875px 0, 440px 720px, 0 720px, 0 585px); }
  .s3 .t { top: 94px; inset-inline-start: 860px; font-weight: 700; font-size: 46px; }
  html[lang="ar"] .s3 .t { font-weight: 800; }
  .s3 .p0 { top: 170px; inset-inline-start: 861px; width: 350px; }
  .s3 .card { position: absolute; top: 318px; inset-inline-start: 490px; width: 726px; height: 263px; border-radius: 40px; background: var(--gray); }
  .s3 .it { position: absolute; top: 366px; width: 280px; }
  .s3 .it svg { display: block; width: 52px; height: 46px; color: var(--accent); margin-inline-start: 1px; }
  .s3 .it .h { margin-top: 10px; }
  .s3 .it .p { position: static; margin-top: 10px; }
  .s3 .i1 { inset-inline-start: 552px; } .s3 .i2 { inset-inline-start: 884px; }

  /* 4) نبذة عن الشركة */
  .s4 .tw { top: 99px; inset-inline-start: 103px; width: 424px; height: 498px; border-radius: 40px; }
  .s4 .t { top: 93px; inset-inline-start: 620px; }
  .s4 .p0 { top: 228px; inset-inline-start: 621px; width: 525px; }
  .s4 .kpi { position: absolute; top: 360px; inset-inline-start: 621px; width: 595px; height: 212px; border-radius: 36px; background: linear-gradient(30deg, #0238E2 0%, #3463FB 45%, #7E9DFA 100%); color: #fff; }
  html[dir="rtl"] .s4 .kpi { background: linear-gradient(-30deg, #0238E2 0%, #3463FB 45%, #7E9DFA 100%); }
  .s4 .num { position: absolute; top: 52px; inset-inline-start: 50px; margin: 0; font-size: 96px; line-height: 110px; font-weight: 700; letter-spacing: -2px; white-space: nowrap; }
  .s4 .num small { font-size: 42px; letter-spacing: 0; }
  .s4 .kp { position: absolute; top: 60px; inset-inline-start: 236px; width: 320px; margin: 0; font-size: 22px; line-height: 31px; }
  html[lang="ar"] .s4 .kp { font-size: 23px; line-height: 33px; top: 56px; }

  /* 5) الرؤية والرسالة */
  .s5 .city { top: 416px; inset-inline-start: 0; width: 1052px; height: 304px; clip-path: path('M0,0 H1000 Q1052,0 1050,50 L999,304 H0 Z'); }
  .s5 .vm { position: absolute; top: 58px; inset-inline-start: 136px; width: 392px; height: 520px; border-radius: 58px 50px 50px 58px; transform: skewX(-12deg); background: linear-gradient(210deg, #7E9DFA 0%, #3463FB 50%, #0337E2 100%); }
  html[dir="rtl"] .s5 .vm { transform: skewX(12deg); background: linear-gradient(150deg, #7E9DFA 0%, #3463FB 50%, #0337E2 100%); }
  .s5 .sep { position: absolute; top: 318px; inset-inline-start: 180px; width: 305px; height: 1px; background: rgb(255 255 255 / .35); }
  .s5 .ic { position: absolute; width: 78px; height: 78px; border-radius: 50%; background: var(--accent); border: 3px solid #fff; color: #fff; display: grid; place-items: center; }
  .s5 .ic svg { width: 38px; height: 38px; }
  .s5 .ic1 { top: 149px; inset-inline-start: 123px; } .s5 .ic2 { top: 410px; inset-inline-start: 64px; }
  .s5 .vh { position: absolute; margin: 0; font-size: 26px; line-height: 34px; font-weight: 700; color: #fff; }
  html[lang="ar"] .s5 .vh { font-weight: 800; }
  .s5 .vp { position: absolute; margin: 0; font-size: 17px; line-height: 25px; color: #fff; width: 295px; }
  html[lang="ar"] .s5 .vp { font-size: 18px; line-height: 27px; }
  .s5 .vh1 { top: 108px; inset-inline-start: 235px; } .s5 .vp1 { top: 152px; inset-inline-start: 235px; }
  .s5 .vh2 { top: 380px; inset-inline-start: 171px; } .s5 .vp2 { top: 425px; inset-inline-start: 171px; }
  .s5 .t { top: 93px; inset-inline-start: 620px; }
  .s5 .p0 { top: 228px; inset-inline-start: 621px; width: 525px; }

  /* 6) خدمات الشركة */
  .s6 .fac { top: 0; inset-inline-start: 290px; width: 590px; height: 720px; clip-path: polygon(180px 0, 590px 0, 415px 720px, 0 720px); }
  .s6 .t { top: 86px; inset-inline-start: 112px; font-size: 45px; line-height: 52px; white-space: normal; width: 240px; }
  html[lang="ar"] .s6 .t { font-size: 44px; line-height: 56px; width: 230px; }
  .s6 .p0 { top: 272px; inset-inline-start: 113px; width: 250px; }
  .s6 .sv { position: absolute; display: flex; gap: 41px; align-items: flex-start; }
  .s6 .sv .ic { flex: none; width: 97px; height: 97px; border-radius: 50%; background: var(--accent); color: #fff; display: grid; place-items: center; margin-top: -1px; }
  .s6 .sv .ic svg { width: 54px; height: 54px; }
  .s6 .sv .h { font-size: 30px; }
  .s6 .sv .p { position: static; margin-top: 7px; width: 260px; }
  .s6 .sv1 { top: 153px; inset-inline-start: 786px; } .s6 .sv2 { top: 327px; inset-inline-start: 740px; } .s6 .sv3 { top: 501px; inset-inline-start: 696px; }

  /* 7) ماذا نقدّم */
  .s7 .roof { top: 0; inset-inline-start: 0; width: 563px; height: 375px; }
  .s7 .cd { position: absolute; width: 211px; height: 329px; border-radius: 32px; background: var(--accent); color: #fff; text-align: center; padding: 0 18px; }
  .s7 .cd svg { display: block; width: 64px; height: 64px; margin: 0 auto; }
  .s7 .cd h3 { margin: 36px 0 0; font-size: 26px; line-height: 34px; font-weight: 700; }
  html[lang="ar"] .s7 .cd h3 { font-weight: 800; font-size: 27px; }
  .s7 .cd p { margin: 12px 0 0; font-size: 17px; line-height: 25px; }
  html[lang="ar"] .s7 .cd p { font-size: 18px; line-height: 27px; }
  .s7 .c1 { top: 242px; inset-inline-start: 64px; padding-top: 52px; } .s7 .c2 { top: 187px; inset-inline-start: 292px; padding-top: 46px; } .s7 .c3 { top: 133px; inset-inline-start: 521px; padding-top: 50px; }
  .s7 .t { top: 180px; inset-inline-start: 822px; font-weight: 700; font-size: 47px; }
  html[lang="ar"] .s7 .t { font-weight: 800; }
  .s7 .p0 { top: 265px; inset-inline-start: 823px; width: 320px; }

  /* 8) الفريق */
  .s8 .t { top: 40px; inset-inline-start: 0; width: 1280px; text-align: center; font-weight: 700; }
  html[lang="ar"] .s8 .t { font-weight: 800; }
  .s8 .sub { position: absolute; top: 101px; inset-inline-start: 0; width: 1280px; margin: 0; text-align: center; font-size: 21px; line-height: 30px; color: var(--tx); }
  .s8 .mb { position: absolute; width: 228px; height: 228px; }
  .s8 .mb img { position: absolute; top: 0; inset-inline-start: 0; width: 228px; height: 228px; border-radius: 50%; object-fit: cover; }
  .s8 .mb .n { position: absolute; top: 155px; inset-inline-start: 1px; width: 227px; height: 38px; border-radius: 19px; background: linear-gradient(90deg, #0439E1, #7D99FC); color: #fff; margin: 0; text-align: center; font-size: 17px; line-height: 38px; box-shadow: 0 5px 8px rgb(0 0 0 / .22); }
  html[dir="rtl"] .s8 .mb .n { background: linear-gradient(270deg, #0439E1, #7D99FC); }
  .s8 .mb .d { position: absolute; top: 195px; inset-inline-start: 29px; width: 170px; height: 30px; border-radius: 15px; background: var(--gray); color: var(--tx); margin: 0; text-align: center; font-size: 14px; line-height: 30px; box-shadow: 0 4px 6px rgb(0 0 0 / .18); }

  /* 9) مشاريع مميزة */
  .s9 .side { position: absolute; top: 0; inset-inline-start: 0; width: 355px; height: 720px; background: var(--accent); }
  .s9 .t { top: 61px; inset-inline-start: 405px; font-size: 46px; }
  .s9 .p0 { top: 130px; inset-inline-start: 406px; width: 385px; }

  /* 10) معرض الأعمال */
  .s10 .pp1 { top: 188px; inset-inline-start: 72px; width: 363px; height: 282px; clip-path: polygon(63px 0, 363px 0, 298px 282px, 0 282px); }
  .s10 .pp2 { top: 103px; inset-inline-start: 408px; width: 450px; height: 255px; clip-path: polygon(52px 0, 450px 0, 387px 255px, 0 255px); }
  .s10 .pp3 { top: 372px; inset-inline-start: 345px; width: 296px; height: 229px; clip-path: polygon(50px 0, 296px 0, 240px 229px, 0 229px); }
  .s10 .ck { position: absolute; inset-inline-start: 900px; display: flex; gap: 28px; align-items: flex-start; }
  .s10 .ck i { flex: none; width: 26px; height: 26px; border-radius: 50%; background: var(--accent); color: #fff; display: grid; place-items: center; margin-top: 10px; }
  .s10 .ck i svg { width: 22px; height: 22px; }
  .s10 .ck .p { position: static; width: 205px; }
  .s10 .k1 { top: 131px; } .s10 .k2 { top: 208px; } .s10 .k3 { top: 285px; }
  .s10 .t { top: 448px; inset-inline-start: 723px; }
  .s10 .p0 { top: 514px; inset-inline-start: 724px; width: 385px; }

  /* 11) تواصل معنا */
  .s11 .off { top: 205px; inset-inline-start: 58px; width: 475px; height: 515px; clip-path: polygon(132px 0, 475px 0, 342px 515px, 0 515px); }
  .s11 .t { top: 176px; inset-inline-start: 801px; }
  .s11 .p0 { top: 253px; inset-inline-start: 802px; width: 330px; }
  .s11 .ci { position: absolute; display: flex; gap: 19px; align-items: flex-start; }
  .s11 .ci i { flex: none; width: 40px; height: 40px; border-radius: 50%; background: var(--accent); color: #fff; display: grid; place-items: center; margin-top: 4px; }
  .s11 .ci i svg { width: 22px; height: 22px; }
  .s11 .ci h3 { margin: 0; font-size: 24px; line-height: 30px; font-weight: 700; color: var(--ink); }
  html[lang="ar"] .s11 .ci h3 { font-weight: 800; font-size: 24px; }
  .s11 .ci .p { position: static; margin-top: 6px; width: 180px; }
  .s11 .ci .p.ph2 { line-height: 29px; }
  .s11 .a2 .p { width: 140px; }
  .s11 .a1 { top: 516px; inset-inline-start: 496px; } .s11 .a2 { top: 519px; inset-inline-start: 743px; } .s11 .a3 { top: 518px; inset-inline-start: 981px; }"""

# ---------- الشريحة 1: الغلاف ----------
LOGO = ('<svg class="logo" viewBox="0 0 56 62" aria-hidden="true">'
        '<path d="M18 6 Q30 2 46 4 Q54 5 54 14 L54 34 Q53 40 47 38 L46 18 Q46 14 42 14 L22 15 Q16 15 18 6Z" fill="#3463FB"/>'
        '<path d="M4 24 Q4 16 12 16 L36 16 Q40 16 38 22 L14 23 Q10 23 10 27 L11 48 Q11 52 16 52 L44 51 Q50 51 49 56 Q36 60 14 58 Q4 57 4 48Z" fill="#B9CBF2"/>'
        '</svg>')

COVER_DEC = svg('', (
    rings(915, 432, [195, 207, 219, 231, 243, 255, 267], '#E8E8E8')
    + '<path d="M868,262 V150 Q868,126 892,126 H916 Q936,126 946,108 L966,74 Q974,60 990,60 H1100" fill="none" stroke="#D3D3D3" stroke-width="2"/>'
    + '<path d="M543,630 V568 H676" fill="none" stroke="#D3D3D3" stroke-width="2"/><circle cx="543" cy="630" r="3.5" fill="#D3D3D3"/><circle cx="676" cy="568" r="3.5" fill="#D3D3D3"/>'
    + dots(662, 61, 7, 4) + dots(427, 587, 4, 4)
    + '<defs><linearGradient id="cv-c" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#3463FB"/><stop offset=".55" stop-color="#2A4FDB"/><stop offset="1" stop-color="#13268A"/></linearGradient>'
      '<linearGradient id="cv-low" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#D3DCFD"/><stop offset="1" stop-color="#F7F9FF"/></linearGradient>'
      '<linearGradient id="cv-med" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#93AAFA"/><stop offset="1" stop-color="#C3CFFC"/></linearGradient>'
      '<linearGradient id="cv-d" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#1A39B4"/><stop offset="1" stop-color="#3463FB"/></linearGradient>'
      '<linearGradient id="cv-r" x1="0" y1="0" x2=".4" y2="1"><stop offset="0" stop-color="#8EA6F6"/><stop offset=".45" stop-color="#4F70E2"/><stop offset="1" stop-color="#0B2B9B"/></linearGradient>'
      '<radialGradient id="cv-g"><stop offset="0" stop-color="#fff"/><stop offset=".45" stop-color="#fff" stop-opacity=".75"/><stop offset="1" stop-color="#fff" stop-opacity="0"/></radialGradient></defs>'
    + '<circle cx="915" cy="434" r="182" fill="url(#cv-c)"/>'
    + '<path d="M558,720 L622,608 Q632,592 652,592 H800 V720 Z" fill="url(#cv-low)"/>'
    + '<polygon points="1132,0 1248,0 790,720 708,720" fill="#E4EAFE" fill-opacity=".72"/>'
    + '<path d="M1248,0 H1280 V190 L1203,318 Q1182,352 1150,352 H1024 Z" fill="url(#cv-med)" fill-opacity=".92"/>'
    + '<path d="M1024,352 H1158 Q1185,352 1170,378 L960,720 H790 L1000,374 Q1012,352 1024,352 Z" fill="url(#cv-d)"/>'
    + '<polygon points="1280,190 1280,720 960,720" fill="url(#cv-r)"/>'
    + '<circle cx="1250" cy="700" r="95" fill="url(#cv-g)"/>'
    + '<path d="M1248,0 L790,720" stroke="#fff" stroke-width="2.4"/>'
    + '<path d="M1280,188 L1170,370 M1170,370 L960,720" stroke="#fff" stroke-width="2.4"/>'
    + '<path d="M1024,352 H1158 Q1185,352 1170,378" fill="none" stroke="#fff" stroke-width="2.4"/>'
    + '<line x1="648" y1="508" x2="948" y2="508" stroke="#A9BAF5" stroke-width="2"/><circle cx="648" cy="508" r="8" fill="#3463FB"/>'
    + dots(1100, 563, 7, 4, col='#FFFFFF')
))

S1 = ('\n    ' + COVER_DEC + '\n    <div class="vbar"></div>\n    <h1 class="ct">§C1§<br>§C2§</h1>\n    <p class="cs">§C3§</p>\n    <div class="uln"></div>\n    '
      + LOGO + '\n    <p class="lt">§LG1§<br>§LG2§</p>\n  ')

# ---------- الشريحة 2: الترحيب ----------
S2 = ('\n    ' + svg('', rings(850, 412, [305, 320, 335, 350, 365], '#E8E8E8', ('rc2', 0, 0, 920, 720)) + dots(435, 98, 4, 4)) +
      '\n    <img class="ph man flip" src="' + A + 'welcome.jpg" alt="§ALT2§">'
      '\n    ' + svg('', dots(1045, 247, 6, 4, blue=[[(917 + 248 * __import__('math').cos(a / 20 * 6.2832), 412 + 248 * __import__('math').sin(a / 20 * 6.2832)) for a in range(20)]])) +
      '\n    <h1 class="t"><b>§S2A§</b> §S2B§</h1>'
      '\n    <p class="p pa">§S2P1§</p>\n    <p class="p pb">§S2P2§</p>'
      '\n    <div class="pill"></div><p class="nm">§S2N§</p><p class="rl">§S2R§</p><p class="p pc">§S2P3§</p>'
      '\n    ' + footer(466, 2, bar=True) + '\n  ')

# ---------- الشريحة 3: المقدمة ----------
S3_STRIPE = [(200, 0), (345, 0), (0, 585), (0, 340)]
S3 = ('\n    ' + svg('', rings(1300, 745, [300, 315, 330, 345], '#E3E3E3')) +
      '\n    <img class="ph bld flip" src="' + A + 'intro.jpg" alt="§ALT3§">'
      '\n    ' + svg('', '<polygon points="%s" fill="#3463FB"/>' % poly(S3_STRIPE) + dots(108, 70, 4, 4, blue=[S3_STRIPE])) +
      '\n    <h1 class="t">§S3T§</h1>\n    <p class="p p0">§S3P§</p>'
      '\n    <div class="card"></div>'
      '\n    <div class="it i1">' + ICONS['people'] + '<h2 class="h">§PH§</h2><p class="p">§S3I§</p></div>'
      '\n    <div class="it i2">' + ICONS['chart'] + '<h2 class="h">§PH§</h2><p class="p">§S3I§</p></div>'
      '\n    ' + footer(553, 3) + '\n  ')

# ---------- الشريحة 4: نبذة عن الشركة ----------
S4_SIDE = [(0, 0), (355, 0), (355, 720), (0, 720)]
S4 = ('\n    ' + svg('', '<rect width="355" height="720" fill="#3463FB"/>' + rings(1300, -60, [270, 285, 300, 315], '#E3E3E3')
              + dots(260, 22, 7, 2, blue=[S4_SIDE]) + dots(6, 605, 3, 4, blue=[S4_SIDE])) +
      '\n    <img class="ph tw" src="' + A + 'tower.jpg" alt="§ALT4§">'
      '\n    <h1 class="t"><b>§S4A§</b><br>§S4B§</h1>\n    <p class="p p0">§S4P§</p>'
      '\n    <div class="kpi"><p class="num"><span dir="ltr">56<small>%</small></span></p><p class="kp">§S4K§</p></div>'
      '\n    ' + footer(398, 4) + '\n  ')

# ---------- الشريحة 5: الرؤية والرسالة ----------
S5 = ('\n    ' + svg('', rings(1300, 310, [265, 280, 295, 310], '#E3E3E3') + dots(882, 17, 7, 2)) +
      '\n    <img class="ph city flip" src="' + A + 'city.jpg" alt="§ALT5§">'
      '\n    ' + svg('', '<path d="M1043,490 H1088 Q1128,490 1120,532 L1080,720 H995 Z" fill="#3463FB"/>') +
      '\n    <div class="vm"></div><div class="sep"></div>'
      '\n    <div class="ic ic1">' + ICONS['eye'] + '</div><div class="ic ic2">' + ICONS['rocket'] + '</div>'
      '\n    <h2 class="vh vh1">§S5V§</h2><p class="vp vp1">§S5VP§</p>'
      '\n    <h2 class="vh vh2">§S5M§</h2><p class="vp vp2">§S5VP§</p>'
      '\n    <h1 class="t">§S5A§<br><b>§S5B§</b></h1>\n    <p class="p p0">§S4P§</p>'
      '\n    <span class="pn">5</span>\n  ')

# ---------- الشريحة 6: الخدمات ----------
def sv(n, icon):
    return '<div class="sv sv%d"><div class="ic">%s</div><div><h2 class="h">§PH§</h2><p class="p">§S6I§</p></div></div>' % (n, ICONS[icon])


S6 = ('\n    ' + svg('', '<path d="M0,0 H115 L15,405 Q8,430 0,432 Z" fill="#3463FB"/>' + rings(40, 470, [190, 205, 220, 235], '#E6E6E6') + dots(87, 468, 7, 4)) +
      '\n    <img class="ph fac flip" src="' + A + 'facade.jpg" alt="§ALT6§">'
      '\n    <h1 class="t">§S6A§<br><b>§S6B§</b></h1>\n    <p class="p p0">§S6P§</p>'
      '\n    ' + sv(1, 'mixer') + '\n    ' + sv(2, 'excavator') + '\n    ' + sv(3, 'crane') +
      '\n    ' + footer(64, 6) + '\n  ')

# ---------- الشريحة 7: ماذا نقدّم ----------
def cd(n, icon):
    return '<div class="cd c%d">%s<h3>§PH§</h3><p>§S7I§</p></div>' % (n, ICONS[icon])


S7 = ('\n    ' + svg('', rings(1100, 650, [195, 210, 225, 240, 255], '#E3E3E3') + dots(1049, 75, 7, 4)) +
      '\n    <img class="ph roof flip" src="' + A + 'roof.jpg" alt="§ALT7§">'
      '\n    ' + cd(1, 'building') + cd(2, 'gears') + cd(3, 'user') +
      '\n    <h1 class="t">§S7T§</h1>\n    <p class="p p0">§S7P§</p>'
      '\n    ' + footer(64, 7) + '\n  ')

# ---------- الشريحة 8: الفريق ----------
TEAM = [(115, 155), (388, 155), (663, 155), (937, 155), (252, 385), (526, 385), (800, 385)]
S8 = ('\n    ' + svg('', rings(250, 745, [240, 255, 270, 285, 300], '#E3E3E3') + dots(1168, 38, 4, 4)) +
      '\n    <h1 class="t">§S8T§</h1>\n    <p class="sub">§S8S§</p>' +
      ''.join('\n    <div class="mb" style="top:%dpx; inset-inline-start:%dpx"><img src="%steam%d.jpg" alt="§ALT8§"><p class="n">§NAME§</p><p class="d">%s</p></div>'
              % (y, x, A, i + 1, '§CEO§' if i == 0 else '§DESG§') for i, (x, y) in enumerate(TEAM)) +
      '\n    ' + footer(64, 8) + '\n  ')

# ---------- الشريحة 9: مشاريع مميزة ----------
S9_SIDE = [(0, 0), (355, 0), (355, 720), (0, 720)]
S9 = ('\n    ' + svg('', '<rect width="355" height="720" fill="#3463FB"/>' + rings(1150, 760, [270, 285, 300, 315], '#E3E3E3') + dots(267, 512, 7, 4, blue=[S9_SIDE])) +
      '\n    <img class="ph" style="top:239px; inset-inline-start:102px; width:370px; height:193px" src="' + A + 'f-tower.jpg" alt="§ALT9§">'
      '\n    <img class="ph" style="top:239px; inset-inline-start:482px; width:370px; height:138px" src="' + A + 'f-roof.jpg" alt="§ALT9§">'
      '\n    <img class="ph" style="top:0; inset-inline-start:862px; width:315px; height:377px" src="' + A + 'f-facade.jpg" alt="§ALT9§">'
      '\n    <img class="ph" style="top:387px; inset-inline-start:482px; width:695px; height:219px" src="' + A + 'f-glass.jpg" alt="§ALT9§">'
      '\n    <h1 class="t">§S9T§</h1>\n    <p class="p p0">§S9P§</p>'
      '\n    ' + footer(406, 9) + '\n  ')

# ---------- الشريحة 10: معرض الأعمال ----------
S10_BLUE = [(0, 0), (545, 0), (365, 720), (0, 720)]
S10 = ('\n    ' + svg('', '<polygon points="%s" fill="#3463FB"/>' % poly(S10_BLUE) + rings(1140, 690, [250, 265, 280, 295], '#E3E3E3') + dots(14, 57, 4, 4, blue=[S10_BLUE])) +
      '\n    <img class="ph pp1 flip" src="' + A + 'p1.jpg" alt="§ALT10§">'
      '\n    <img class="ph pp2 flip" src="' + A + 'p2.jpg" alt="§ALT10§">'
      '\n    <img class="ph pp3 flip" src="' + A + 'p3.jpg" alt="§ALT10§">' +
      ''.join('\n    <div class="ck k%d"><i>%s</i><p class="p">§S10C§</p></div>' % (n, ICONS['check']) for n in (1, 2, 3)) +
      '\n    <h1 class="t">§S10A§ <b>§S10B§</b></h1>\n    <p class="p p0">§S9P§</p>'
      '\n    ' + footer(435, 10) + '\n  ')

# ---------- الشريحة 11: تواصل معنا ----------
S11_BLUE = [(447, 0), (768, 0), (645, 482), (324, 482)]
S11 = ('\n    ' + svg('', rings(190, 245, [195, 210, 225, 240], '#E3E3E3') + dots(57, 158, 4, 7) + '<polygon points="%s" fill="#3463FB"/>' % poly(S11_BLUE) + dots(673, 67, 7, 4, blue=[S11_BLUE])) +
      '\n    <img class="ph off flip" src="' + A + 'office.jpg" alt="§ALT11§">'
      '\n    <h1 class="t"><b>§S11A§</b> §S11B§</h1>\n    <p class="p p0">§S11P§</p>'
      '\n    <div class="ci a1"><i>' + ICONS['pin'] + '</i><div><h3>§ADR§</h3><p class="p">§ADRT§</p></div></div>'
      '\n    <div class="ci a2"><i>' + ICONS['mail'] + '</i><div><h3>§EML§</h3><p class="p">§EMLT§</p></div></div>'
      '\n    <div class="ci a3"><i>' + ICONS['phone'] + '</i><div><h3>§PHN§</h3><p class="p ph2"><span dir="ltr">+1 (212) 999 9090</span><br><span dir="ltr">+1 (212) 999 0909</span></p></div></div>'
      '\n    ' + footer(479, 11) + '\n  ')

SLIDES = [S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11]

STR = {
    'ar': {
        'FB': 'أزرق وأبيض', 'FT': 'عرض أعمال',
        'C1': 'أزرق وأبيض', 'C2': 'للأعمال', 'C3': 'قالب عرض تقديمي', 'LG1': 'شعار شركتك', 'LG2': 'هنا',
        'S2A': 'مرحبًا', 'S2B': 'بكم',
        'S2P1': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي.',
        'S2P2': 'أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي.',
        'S2N': 'جون دو', 'S2R': 'الرئيس التنفيذي',
        'S2P3': 'أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي.',
        'S3T': 'مقدمة', 'S3P': 'أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي.',
        'PH': 'عنوان نائب', 'S3I': 'أضف النص المطلوب هنا. هذا نص توضيحي.',
        'S4A': 'نبذة عن', 'S4B': 'شركتنا',
        'S4P': 'أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا.',
        'S4K': 'أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا.',
        'S5V': 'الرؤية', 'S5M': 'الرسالة', 'S5VP': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. هذا نص توضيحي.',
        'S5A': 'رؤيتنا', 'S5B': 'ورسالتنا',
        'S6A': 'تعرّف على', 'S6B': 'خدمات شركتنا', 'S6P': 'أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا.',
        'S6I': 'أضف النص المطلوب هنا. هذا نص توضيحي.',
        'S7I': 'أضف النص المطلوب هنا. هذا نص توضيحي.',
        'S7T': 'ماذا نقدّم', 'S7P': 'أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا.',
        'S8T': 'تعرّف على فريقنا', 'S8S': 'فريق صغير. نتائج كبيرة.', 'NAME': 'الاسم', 'CEO': 'الرئيس التنفيذي', 'DESG': 'المسمى الوظيفي',
        'S9T': 'مشاريع مميزة', 'S9P': 'أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا.',
        'S10C': 'أضف النص المطلوب هنا. هذا نص توضيحي.', 'S10A': 'معرض', 'S10B': 'أعمالنا',
        'S11A': 'تواصل', 'S11B': 'معنا', 'S11P': 'أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. أضف النص المطلوب هنا. هذا نص توضيحي.',
        'ADR': 'العنوان', 'ADRT': 'أضف النص المطلوب هنا.', 'EML': 'البريد الإلكتروني', 'EMLT': 'أدخل البريد الإلكتروني هنا', 'PHN': 'الهاتف',
        'ALT2': 'صورة شخص يرحّب', 'ALT3': 'واجهة مبنى زجاجي', 'ALT4': 'برج زجاجي', 'ALT5': 'أفق مدينة', 'ALT6': 'واجهة مبنى معدنية',
        'ALT7': 'سقف مبنى', 'ALT8': 'صورة عضو الفريق', 'ALT9': 'صورة مشروع', 'ALT10': 'صورة من معرض الأعمال', 'ALT11': 'مكتب عمل',
    },
    'en': {
        'FB': 'Blue &amp; White', 'FT': 'Business Presentation',
        'C1': 'Blue &amp; White', 'C2': 'Business', 'C3': 'Presentation Template', 'LG1': 'Your Company', 'LG2': 'Logo Here',
        'S2A': 'Welcome', 'S2B': 'Slide',
        'S2P1': 'This is a sample text. Insert your desired text here. This is a sample text.',
        'S2P2': 'Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text.',
        'S2N': 'JOHN DOE', 'S2R': 'CEO',
        'S2P3': 'Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text.',
        'S3T': 'Introduction', 'S3P': 'Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text.',
        'PH': 'Placeholder', 'S3I': 'Insert your desired text here. This is a sample text.',
        'S4A': 'About Our', 'S4B': 'Company',
        'S4P': 'Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here.',
        'S4K': 'Insert your desired text here. This is a sample text. Insert your desired text here.',
        'S5V': 'Vision', 'S5M': 'Mission', 'S5VP': 'This is a sample text. Insert your desired text here. This is a sample text. This is a sample text.',
        'S5A': 'Our', 'S5B': 'Vision &amp; Mission',
        'S6A': 'Our', 'S6B': 'Company Services', 'S6P': 'Insert your desired text here. This is a sample text. Insert your desired text here.',
        'S6I': 'Insert your desired text here. This is a sample text.',
        'S7I': 'Insert your desired text here. This is a sample text.',
        'S7T': 'What We Do', 'S7P': 'Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here.',
        'S8T': 'Meet Our Team', 'S8S': 'Small Team. Big Results.', 'NAME': 'Name', 'CEO': 'CEO', 'DESG': 'Designation',
        'S9T': 'Featured Projects', 'S9P': 'Insert your desired text here. This is a sample text. Insert your desired text here.',
        'S10C': 'Insert your desired text here. This is a sample text.', 'S10A': 'Our', 'S10B': 'Portfolio',
        'S11A': 'Contact', 'S11B': 'Us', 'S11P': 'Insert your desired text here. This is a sample text. Insert your desired text here. Insert your desired text here. This is a sample text.',
        'ADR': 'Address', 'ADRT': 'Insert your desired text here.', 'EML': 'Email', 'EMLT': 'Enter Email Address Here', 'PHN': 'Phone',
        'ALT2': 'Person offering a handshake', 'ALT3': 'Glass building facade', 'ALT4': 'Glass tower', 'ALT5': 'City skyline', 'ALT6': 'Metal building facade',
        'ALT7': 'Building roofline', 'ALT8': 'Team member photo', 'ALT9': 'Project photo', 'ALT10': 'Portfolio photo', 'ALT11': 'Open-plan office',
    },
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'general', 'icon': '🏢',
    'name': 'أعمال بالأزرق والأبيض', 'nameEn': 'Blue & White Business',
    'description': 'إحدى عشرة شريحة لعرض شركة: غلاف، ترحيب، مقدمة، نبذة، رؤية ورسالة، خدمات، الفريق، المشاريع، الأعمال، التواصل',
    'descriptionEn': 'Eleven company slides: cover, welcome, intro, about, vision & mission, services, team, projects, portfolio, contact',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'مرحبًا بكم', 'titleEn': 'Welcome Slide'},
        {'title': 'مقدمة', 'titleEn': 'Introduction'},
        {'title': 'نبذة عن شركتنا', 'titleEn': 'About Our Company', 'needsData': True},
        {'title': 'رؤيتنا ورسالتنا', 'titleEn': 'Our Vision & Mission'},
        {'title': 'خدمات شركتنا', 'titleEn': 'Our Company Services'},
        {'title': 'ماذا نقدّم', 'titleEn': 'What We Do'},
        {'title': 'تعرّف على فريقنا', 'titleEn': 'Meet Our Team'},
        {'title': 'مشاريع مميزة', 'titleEn': 'Featured Projects'},
        {'title': 'معرض أعمالنا', 'titleEn': 'Our Portfolio'},
        {'title': 'تواصل معنا', 'titleEn': 'Contact Us'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والصور وغيّر النصوص والأرقام فقط (ومنها نسبة 56% في شريحة النبذة).',
}
