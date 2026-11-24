# -*- coding: utf-8 -*-
# نموذج «عرض التدريب على الذكاء الاصطناعي»: تسع شرائح بخلفية متدرّجة كحلية/بنفسجية وزخارف دوائر إلكترونية —
# غلاف بوجه رقمي، جدول محتويات بشرائح معالجة مرقّمة، تعريف مع روبوت، ثلاث بطاقات أنواع، ثلاث شرائح تفصيل بهولوغرام،
# خط زمني بحلقات سنوية، وشريحة ختام بحرفي الذكاء الاصطناعي المجسّمين.
import math, random

KEY = 'sl-ai-training'
TITLE = {'ar': 'عرض التدريب على الذكاء الاصطناعي', 'en': 'AI Training Presentation'}
COMMENT = 'نموذج «عرض التدريب على الذكاء الاصطناعي»: تسع شرائح على خلفية متدرّجة من الكحلي إلى البنفسجي الكهربائي مع زخارف دوائر إلكترونية SVG، عناوين بخطوط زخرفية بنقاط، شرائح معالجة مرقّمة، بطاقات بزوايا مقطوعة، حلقات زمنية بأقواس شفافة، والرسوم التوضيحية (الوجه الرقمي، الروبوت، الهولوغرام، اليد الروبوتية) صور في assets.'
VARS = '--ink: #0E1A70; --accent: #7652FC; --bg: #1B129F; --lav: #AA97FF; --lav-2: #A99AFF; --violet: #4B1DFF; --navy: #0E1A70; --bright: #3305EE; --text: #F2F0FF;'

A = '/templates/freeform/sl-ai-training/assets/'

STYLE = """  /* أنماط مشتركة */
  .frame { color: #fff; background: linear-gradient(to top right, #0E1A70 0%, #1B129F 48%, #3305EE 100%); }
  html[dir="rtl"] .frame { background: linear-gradient(to top left, #0E1A70 0%, #1B129F 48%, #3305EE 100%); }
  /* طبقة الزخارف (دوائر إلكترونية) — تنعكس في العربية لأنها زخرفية فقط */
  .deco, .tl, .tline { position: absolute; pointer-events: none; }
  .deco { top: 0; inset-inline-start: 0; width: 1280px; height: 720px; }
  html[dir="rtl"] .deco, html[dir="rtl"] .tl, html[dir="rtl"] .tline { transform: scaleX(-1); }
  .deco path, .deco circle, .tl path, .tline path, .tline circle { fill: none; stroke: var(--lav); stroke-width: 2; stroke-linejoin: round; stroke-linecap: round; }
  .deco .dot, .tl .dot, .tline .dot { fill: var(--accent); stroke: none; }
  .deco .sq { fill: var(--lav); stroke: none; opacity: .5; }
  .deco .soft { opacity: .35; } .deco .faint { opacity: .18; }
  .deco .blob { stroke: #fff; opacity: .06; stroke-width: 90; }
  .deco .ticks { stroke: #fff; opacity: .045; stroke-width: 48; stroke-dasharray: 9 15; stroke-linecap: butt; }
  .ttl { position: absolute; margin: 0; font-size: 46px; font-weight: 700; line-height: 1.2; color: #fff; }
  html[lang="ar"] .ttl { font-weight: 800; }
  .ttl.top { top: 44px; inset-inline-start: 99px; }
  .tl.top { top: 30px; inset-inline-start: 0; width: 360px; height: 100px; }
  .p { position: absolute; margin: 0; font-size: 19px; line-height: 28px; color: var(--text); width: 432px; }
  .p + .p { margin-top: 0; }
  .ph { position: absolute; object-fit: cover; }
  .ph.fade-s { -webkit-mask-image: linear-gradient(to right, transparent, #000 110px); mask-image: linear-gradient(to right, transparent, #000 110px); }
  html[dir="rtl"] .ph.fade-s { -webkit-mask-image: linear-gradient(to left, transparent, #000 110px); mask-image: linear-gradient(to left, transparent, #000 110px); }

  /* 1) الغلاف */
  .s1 { background: linear-gradient(to bottom right, #1C2272, #0E1A70 50%, #0D1F78); }
  html[dir="rtl"] .s1 { background: linear-gradient(to bottom left, #1C2272, #0E1A70 50%, #0D1F78); }
  .s1 .face { top: 0; inset-inline-end: 0; width: 705px; height: 720px; }
  html[dir="rtl"] .s1 .face { transform: scaleX(-1); -webkit-mask-image: linear-gradient(to right, transparent, #000 110px); mask-image: linear-gradient(to right, transparent, #000 110px); }
  .s1 .logo { position: absolute; top: 60px; inset-inline-start: 64px; width: 40px; height: 40px; }
  .s1 .logo-t { position: absolute; top: 61px; inset-inline-start: 110px; margin: 0; font-size: 24px; font-weight: 700; color: #fff; line-height: 1.4; }
  .s1 .t1 { position: absolute; top: 317px; inset-inline-start: 65px; margin: 0; font-size: 70px; font-weight: 700; line-height: 1.1; color: #fff; letter-spacing: -0.5px; white-space: nowrap; }
  .s1 .t2 { position: absolute; top: 395px; inset-inline-start: 65px; margin: 0; font-size: 70px; font-weight: 700; line-height: 1.1; letter-spacing: -0.5px; white-space: nowrap; background: linear-gradient(90deg, #ECAE80 0%, #DC9FB8 45%, #9A74F0 80%, #6A56F3 100%); -webkit-background-clip: text; background-clip: text; color: transparent; }
  html[dir="rtl"] .s1 .t2 { background-image: linear-gradient(270deg, #ECAE80 0%, #DC9FB8 45%, #9A74F0 80%, #6A56F3 100%); }
  html[lang="ar"] .s1 .t1, html[lang="ar"] .s1 .t2 { font-size: 64px; font-weight: 800; letter-spacing: 0; line-height: 1.2; }
  html[lang="ar"] .s1 .t1 { top: 306px; } html[lang="ar"] .s1 .t2 { top: 386px; }
  .s1 .person { position: absolute; top: 602px; inset-inline-start: 70px; width: 40px; height: 44px; }
  .s1 .sep { position: absolute; top: 598px; inset-inline-start: 137px; width: 1px; height: 50px; background: rgb(170 151 255 / .45); }
  .s1 .name { position: absolute; top: 596px; inset-inline-start: 170px; margin: 0; font-size: 22px; font-weight: 700; line-height: 1.2; color: #fff; }
  .s1 .role { position: absolute; top: 625px; inset-inline-start: 170px; margin: 0; font-size: 20px; font-weight: 400; line-height: 1.2; color: #fff; }

  /* 2) جدول المحتويات — شرائح معالجة مرقّمة */
  .s2 .chip { position: absolute; width: 120px; height: 120px; }
  .s2 .chip svg { width: 120px; height: 120px; }
  .s2 .chip rect { fill: none; stroke: var(--lav-2); stroke-width: 2; }
  .s2 .chip path { fill: none; stroke: var(--lav-2); stroke-width: 2; stroke-linecap: round; }
  .s2 .chip span { position: absolute; inset: 16px; display: grid; place-items: center; font-size: 48px; font-weight: 700; color: #fff; line-height: 1; }
  .s2 .it { position: absolute; width: 250px; }
  .s2 .it h2 { position: absolute; top: 0; inset-inline-start: 0; margin: 0; font-size: 26px; font-weight: 700; line-height: 1.25; color: #fff; width: 250px; }
  .s2 .it p { position: absolute; inset-inline-start: 0; margin: 0; font-size: 17px; line-height: 22px; color: var(--text); width: 240px; }
  .s2 .c1 { top: 232px; inset-inline-start: 185px; } .s2 .c2 { top: 232px; inset-inline-start: 700px; }
  .s2 .c3 { top: 430px; inset-inline-start: 185px; } .s2 .c4 { top: 430px; inset-inline-start: 700px; }
  .s2 .i1 { top: 242px; inset-inline-start: 333px; } .s2 .i1 p { top: 44px; }
  .s2 .i2 { top: 242px; inset-inline-start: 848px; } .s2 .i2 p { top: 44px; }
  .s2 .i3 { top: 440px; inset-inline-start: 333px; } .s2 .i3 p { top: 44px; }
  .s2 .i4 { top: 409px; inset-inline-start: 848px; } .s2 .i4 p { top: 75px; }
  html[lang="ar"] .s2 .i4 { top: 440px; } html[lang="ar"] .s2 .i4 p { top: 44px; }
  html[lang="ar"] .s2 .it h2 { white-space: nowrap; width: auto; }

  /* 3) التعريف — روبوت + عنوان بخطين زخرفيين */
  .s3 .robot { top: 40px; inset-inline-start: 0; width: 480px; height: 680px;
    -webkit-mask-image: linear-gradient(to right, #000 calc(100% - 80px), transparent), linear-gradient(to bottom, transparent, #000 60px);
    mask-image: linear-gradient(to right, #000 calc(100% - 80px), transparent), linear-gradient(to bottom, transparent, #000 60px);
    -webkit-mask-composite: source-in; mask-composite: intersect; }
  html[dir="rtl"] .s3 .robot { -webkit-mask-image: linear-gradient(to left, #000 calc(100% - 80px), transparent), linear-gradient(to bottom, transparent, #000 60px); mask-image: linear-gradient(to left, #000 calc(100% - 80px), transparent), linear-gradient(to bottom, transparent, #000 60px); }
  .s3 .kick { position: absolute; top: 97px; inset-inline-start: 638px; margin: 0; font-size: 26px; font-weight: 400; line-height: 1.2; color: #fff; }
  .s3 .ttl { top: 143px; inset-inline-start: 638px; line-height: 52px; }
  .s3 .tl { top: 90px; inset-inline-start: 540px; width: 640px; height: 200px; }
  .s3 .p { inset-inline-start: 639px; width: 428px; } .s3 .p1 { top: 303px; } .s3 .p2 { top: 444px; }

  /* 4) أنواع الذكاء الاصطناعي — ثلاث بطاقات بزاويتين مقطوعتين */
  .s4 .card { position: absolute; top: 185px; width: 340px; height: 430px; }
  .s4 .card .bg { position: absolute; inset: 0; background: rgb(255 255 255 / .075); clip-path: polygon(0 0, calc(100% - 52px) 0, 100% 52px, 100% 100%, 52px 100%, 0 calc(100% - 52px)); }
  html[dir="rtl"] .s4 .card .bg { transform: scaleX(-1); }
  .s4 .card .ic { position: absolute; top: 52px; inset-inline-start: 115px; width: 110px; height: 110px; }
  .s4 .card .ic svg { width: 110px; height: 110px; }
  .s4 .card .ic path, .s4 .card .ic circle, .s4 .card .ic polygon, .s4 .card .ic rect { fill: none; stroke: #B6A9FF; stroke-width: 2; stroke-linejoin: round; stroke-linecap: round; }
  .s4 .card .ic .fill { fill: var(--violet); }
  .s4 .card .ic .ai { stroke: #D6CCFF; stroke-width: 2.2; }
  .s4 .card h2 { position: absolute; top: 178px; inset-inline: 0; margin: 0; text-align: center; font-size: 30px; font-weight: 700; line-height: 1.2; color: #fff; }
  html[lang="ar"] .s4 .card h2 { font-size: 26px; font-weight: 800; top: 181px; }
  .s4 .card p { position: absolute; top: 234px; inset-inline: 46px; margin: 0; text-align: center; font-size: 17px; line-height: 22px; color: var(--text); }
  .s4 .k1 { inset-inline-start: 92px; } .s4 .k2 { inset-inline-start: 469px; } .s4 .k3 { inset-inline-start: 847px; }

  /* 5-7) شرائح التفصيل — عنوان بخط سفلي + هولوغرام */
  :is(.s5, .s6, .s7) .ttl { top: 86px; inset-inline-start: 103px; }
  :is(.s5, .s6, .s7) .ul { position: absolute; top: 158px; inset-inline-start: 99px; width: 404px; height: 8px; }
  :is(.s5, .s6, .s7) .ul path { fill: none; stroke: var(--lav); stroke-width: 2; } :is(.s5, .s6, .s7) .ul .dot { fill: var(--lav); }
  :is(.s5, .s6, .s7) .tl { top: 70px; inset-inline-start: 445px; width: 175px; height: 70px; }
  :is(.s5, .s6, .s7) .p { inset-inline-start: 103px; } :is(.s5, .s6, .s7) .p1 { top: 190px; } :is(.s5, .s6, .s7) .p2 { top: 331px; }
  :is(.s5, .s6, .s7) .holo { inset-inline-start: 640px; width: 520px;
    -webkit-mask-image: linear-gradient(to right, transparent, #000 50px, #000 calc(100% - 50px), transparent), linear-gradient(to bottom, transparent, #000 50px, #000 calc(100% - 40px), transparent);
    mask-image: linear-gradient(to right, transparent, #000 50px, #000 calc(100% - 50px), transparent), linear-gradient(to bottom, transparent, #000 50px, #000 calc(100% - 40px), transparent);
    -webkit-mask-composite: source-in; mask-composite: intersect; }
  /* العناوين العربية أطول: خط أصغر وإزاحة الخط الزخرفي حتى لا يتداخل معها */
  html[lang="ar"] :is(.s5, .s6, .s7) .ttl { font-size: 40px; top: 90px; white-space: nowrap; }
  html[lang="ar"] :is(.s5, .s6, .s7) .tl { inset-inline-start: 575px; }
  .s5 .holo { top: 160px; height: 500px; } .s6 .holo { top: 130px; height: 530px; } .s7 .holo { top: 80px; height: 580px; }

  /* 8) الخط الزمني — حلقات بأقواس شفافة */
  .s8 .tline { top: 0; inset-inline-start: 0; width: 1280px; height: 720px; }
  .s8 .tline .ring { stroke: rgb(170 151 255 / .22); stroke-width: 14; stroke-dasharray: 3.6 5.2; stroke-linecap: butt; }
  .s8 .tline .cres { stroke: rgb(255 255 255 / .13); stroke-width: 40; }
  .s8 .yr { position: absolute; width: 140px; margin: 0; text-align: center; font-size: 38px; font-weight: 700; line-height: 1; color: #fff; }
  .s8 .lb { position: absolute; width: 280px; margin: 0; text-align: center; }
  .s8 .lb h2 { margin: 0; font-size: 26px; font-weight: 700; line-height: 1.2; color: #fff; }
  .s8 .lb p { margin: 10px 0 0; font-size: 17px; line-height: 22px; color: var(--text); }
  html[lang="ar"] .s8 .lb p { font-size: 16px; }
  .s8 .y1 { top: 238px; inset-inline-start: 158px; } .s8 .y2 { top: 238px; inset-inline-start: 695px; }
  .s8 .y3 { top: 519px; inset-inline-start: 427px; } .s8 .y4 { top: 519px; inset-inline-start: 965px; }
  .s8 .l1 { top: 235px; inset-inline-start: 365px; } .s8 .l2 { top: 235px; inset-inline-start: 895px; }
  .s8 .l3 { top: 457px; inset-inline-start: 98px; } .s8 .l4 { top: 477px; inset-inline-start: 630px; }

  /* 9) الختام — عنوان بثلاثة أسطر + يد روبوتية */
  .s9 .kick { position: absolute; top: 84px; inset-inline-start: 104px; margin: 0; font-size: 36px; font-weight: 300; line-height: 1.2; color: #fff; }
  html[lang="ar"] .s9 .kick { font-weight: 400; }
  .s9 .ttl { top: 141px; inset-inline-start: 103px; line-height: 52px; }
  .s9 .ul { position: absolute; top: 256px; inset-inline-start: 99px; width: 404px; height: 8px; }
  .s9 .ul path { fill: none; stroke: var(--lav); stroke-width: 2; } .s9 .ul .dot { fill: var(--lav); }
  .s9 .tl.a { top: 25px; inset-inline-start: 0; width: 210px; height: 70px; }
  .s9 .tl.b { top: 170px; inset-inline-start: 445px; width: 175px; height: 70px; }
  .s9 .p { inset-inline-start: 103px; } .s9 .p1 { top: 288px; } .s9 .p2 { top: 429px; }
  .s9 .hand { top: 30px; inset-inline-end: 0; width: 700px; height: 640px;
    -webkit-mask-image: linear-gradient(to right, transparent, #000 90px), linear-gradient(to bottom, transparent, #000 50px, #000 calc(100% - 50px), transparent);
    mask-image: linear-gradient(to right, transparent, #000 90px), linear-gradient(to bottom, transparent, #000 50px, #000 calc(100% - 50px), transparent);
    -webkit-mask-composite: source-in; mask-composite: intersect; }
  html[dir="rtl"] .s9 .hand { -webkit-mask-image: linear-gradient(to left, transparent, #000 90px), linear-gradient(to bottom, transparent, #000 50px, #000 calc(100% - 50px), transparent); mask-image: linear-gradient(to left, transparent, #000 90px), linear-gradient(to bottom, transparent, #000 50px, #000 calc(100% - 50px), transparent); }"""

GLOBAL_DEFS = ('<defs><linearGradient id="lg" x1="0" y1="1" x2="1" y2="0"><stop offset="0" stop-color="#F7C21A"/><stop offset=".55" stop-color="#F08A2A"/><stop offset="1" stop-color="#E63946"/></linearGradient></defs>')


# ---------- مولّد زخارف الدوائر الإلكترونية (مسارات عشوائية بذرة ثابتة) ----------
def _traces(seed, x0, y0, x1, y1, orient, n, cls):
    """مسارات تبدأ من حافة المنطقة وتتقدّم للداخل مع انعطافة 45° ونقطة في النهاية.
    orient: 'L' تتقدّم يسارًا من الحافة اليمنى، 'R' يمينًا، 'D' نزولًا من الحافة العليا، 'U' صعودًا من السفلى."""
    rnd = random.Random(seed); out = []
    for _ in range(n):
        if orient in 'LR':
            y = rnd.uniform(y0 + 6, y1 - 6); x = x1 if orient == 'L' else x0; sx = -1 if orient == 'L' else 1
            pts = [(x, y)]
            for _k in range(rnd.choice([1, 2, 2, 3])):
                run = rnd.uniform(40, 170); x += sx * run; pts.append((x, y))
                jog = rnd.uniform(14, 40) * rnd.choice([-1, 1])
                ny = min(max(y + jog, y0 + 6), y1 - 6); x += sx * abs(ny - y); y = ny; pts.append((x, y))
            x += sx * rnd.uniform(20, 90); pts.append((x, y))
            if orient == 'L' and x < x0 + 8: pts[-1] = (x0 + 8, y)
            if orient == 'R' and x > x1 - 8: pts[-1] = (x1 - 8, y)
        else:
            x = rnd.uniform(x0 + 6, x1 - 6); y = y0 if orient == 'D' else y1; sy = 1 if orient == 'D' else -1
            pts = [(x, y)]
            for _k in range(rnd.choice([1, 2, 2, 3])):
                run = rnd.uniform(40, 150); y += sy * run; pts.append((x, y))
                jog = rnd.uniform(12, 36) * rnd.choice([-1, 1])
                nx = min(max(x + jog, x0 + 6), x1 - 6); y += sy * abs(nx - x); x = nx; pts.append((x, y))
            y += sy * rnd.uniform(20, 80); pts.append((x, y))
            if orient == 'D' and y > y1 - 8: pts[-1] = (x, y1 - 8)
            if orient == 'U' and y < y0 + 8: pts[-1] = (x, y0 + 8)
        d = 'M' + ' L'.join('%.0f,%.0f' % p for p in pts)
        out.append('<path class="%s" d="%s"/><circle class="dot %s" cx="%.0f" cy="%.0f" r="4"/>' % (cls, d, cls, pts[-1][0], pts[-1][1]))
        if rnd.random() < .35:  # مربّعات صغيرة متفرّقة كما في الأصل
            out.append('<rect class="sq" x="%.0f" y="%.0f" width="4" height="4"/>' % (rnd.uniform(x0, x1), rnd.uniform(y0, y1)))
    return ''.join(out)


def _arc(cx, cy, r, a0, a1, cls, ccw=False):
    """قوس SVG من الزاوية a0 إلى a1 (بالدرجات، 0 يمينًا و90 أسفل)."""
    p0 = (cx + r * math.cos(math.radians(a0)), cy + r * math.sin(math.radians(a0)))
    p1 = (cx + r * math.cos(math.radians(a1)), cy + r * math.sin(math.radians(a1)))
    span = (a1 - a0) % 360
    if ccw: span = 360 - span
    large = 1 if span > 180 else 0
    return '<path class="%s" d="M%.1f,%.1f A%d,%d 0 %d %d %.1f,%.1f"/>' % (cls, p0[0], p0[1], r, r, large, 0 if ccw else 1, p1[0], p1[1])


def deco_a():  # الشرائح 2 و4 و8: زخارف أعلى اليمين وأسفل اليسار وأسفل اليمين + حلقة مركزية باهتة
    return ('<svg class="deco" viewBox="0 0 1280 720" aria-hidden="true">'
            + _arc(1260, -60, 258, 168, 96, 'blob', ccw=True)
            + _arc(-40, 510, 150, -50, 50, 'blob')
            + _arc(640, 360, 250, 0, 359.9, 'ticks')
            + '<g class="soft">' + _traces(11, 690, 8, 1280, 140, 'L', 12, '') + _traces(12, 1130, 60, 1275, 350, 'D', 6, '') + '</g>'
            + '<g class="soft">' + _traces(13, 10, 400, 130, 700, 'D', 7, '') + _traces(14, 0, 640, 270, 712, 'R', 6, '') + '</g>'
            + '<g class="faint">' + _traces(15, 1195, 430, 1275, 715, 'D', 6, '') + '</g>'
            + '</svg>')


def deco_b(ring_right=False):  # الشرائح 3 و5–7 و9: زخارف أعلى اليسار وأعلى اليمين وأسفل اليمين + حلقة كبيرة
    if ring_right:
        ring = _arc(760, 300, 330, 222, 50, 'blob', ccw=True) + _arc(760, 300, 240, 230, 60, 'ticks', ccw=True)
    else:
        ring = _arc(330, 800, 415, 220, 285, 'blob') + _arc(240, 700, 212, 185, 290, 'ticks')
    return ('<svg class="deco" viewBox="0 0 1280 720" aria-hidden="true">' + ring
            + '<g class="soft">' + _traces(21, 0, 8, 270, 70, 'R', 7, '') + _traces(22, 8, 40, 75, 360, 'D', 6, '') + '</g>'
            + '<g class="faint">' + _traces(23, 690, 8, 1280, 130, 'L', 9, '') + _traces(24, 1200, 60, 1275, 260, 'D', 4, '') + '</g>'
            + '<g class="soft">' + _traces(25, 1185, 420, 1275, 715, 'U', 7, '') + '</g>'
            + '</svg>')


TL_TOP = ('<svg class="tl top" viewBox="0 0 360 100" aria-hidden="true">'
          '<path d="M0,24 L30,6 L52,6"/><circle class="dot" cx="52" cy="6" r="4"/>'
          '<path d="M22,46 L47,46 L95,93 L340,93"/><circle class="dot" cx="22" cy="46" r="4"/><circle class="dot" cx="340" cy="93" r="4"/></svg>')
TL_DET = ('<svg class="tl" viewBox="0 0 175 70" aria-hidden="true">'
          '<path d="M10,9 L37,9 L85,55 L160,55"/><circle class="dot" cx="10" cy="9" r="4"/><circle class="dot" cx="160" cy="55" r="4"/></svg>')
UL = '<svg class="ul" viewBox="0 0 404 8" aria-hidden="true"><path d="M4,4 L400,4"/><circle class="dot" cx="4" cy="4" r="3.5"/><circle class="dot" cx="400" cy="4" r="3.5"/></svg>'

CHIP = ('<svg viewBox="0 0 120 120" aria-hidden="true"><rect x="16" y="16" width="88" height="88" rx="12"/>'
        '<path d="M43,16 V2 M72,16 V2 M43,104 V118 M72,104 V118 M16,45 H2 M16,74 H2 M104,45 H118 M104,74 H118"/></svg>')

ICON_AGENTIC = ('<svg viewBox="0 0 110 110" aria-hidden="true"><rect class="fill" x="33" y="33" width="44" height="44" rx="8"/>'
                '<path d="M55,33 V14 M55,77 V96 M33,55 H14 M77,55 H96"/><circle cx="55" cy="10" r="4"/><circle cx="55" cy="100" r="4"/><circle cx="10" cy="55" r="4"/><circle cx="100" cy="55" r="4"/>'
                '<path class="ai" d="M44,65 L51,45 L58,65 M46.5,58.5 H55.5 M64,45 V65"/></svg>')
ICON_GEN = ('<svg viewBox="0 0 110 110" aria-hidden="true">'
            '<path class="fill" d="M55,24 C44,8 16,16 20,40 C6,48 8,72 24,76 C24,94 46,98 55,86 C64,98 86,94 86,76 C102,72 104,48 90,40 C94,16 66,8 55,24 Z"/>'
            '<path d="M55,24 V86"/><rect class="fill" x="41" y="42" width="28" height="28" rx="5"/>'
            '<path d="M41,50 H30 L26,46 M41,62 H32 L28,66 M69,50 H80 L84,46 M69,62 H78 L82,66 M48,42 V36 L44,32 M62,42 V36 L66,32 M48,70 V76 L44,80 M62,70 V76 L66,80"/>'
            '<circle cx="23" cy="43" r="3"/><circle cx="25" cy="69" r="3"/><circle cx="87" cy="43" r="3"/><circle cx="85" cy="69" r="3"/><circle cx="41" cy="29" r="3"/><circle cx="69" cy="29" r="3"/><circle cx="41" cy="83" r="3"/><circle cx="69" cy="83" r="3"/>'
            '<path class="ai" d="M47,64 L53,48 L59,64 M49.5,59 H56.5 M63,48 V64"/></svg>')
ICON_SUPER = ('<svg viewBox="0 0 110 110" aria-hidden="true"><polygon class="fill" points="32,42 78,42 92,56 55,96 18,56"/>'
              '<path d="M18,56 H8 M92,56 H102 M32,42 L24,34 M78,42 L86,34 M45,42 V32 M65,42 V32 M30,70 L22,78 M80,70 L88,78 M55,96 V104"/>'
              '<circle cx="5" cy="56" r="3"/><circle cx="105" cy="56" r="3"/><circle cx="21" cy="31" r="3"/><circle cx="89" cy="31" r="3"/><circle cx="45" cy="29" r="3"/><circle cx="65" cy="29" r="3"/><circle cx="19" cy="81" r="3"/><circle cx="91" cy="81" r="3"/><circle cx="55" cy="107" r="3"/>'
              '<path d="M55,6 L57.5,13 L64,15.5 L57.5,18 L55,25 L52.5,18 L46,15.5 L52.5,13 Z M36,18 L37.5,22 L41.5,23.5 L37.5,25 L36,29 L34.5,25 L30.5,23.5 L34.5,22 Z M74,18 L75.5,22 L79.5,23.5 L75.5,25 L74,29 L72.5,25 L68.5,23.5 L72.5,22 Z"/>'
              '<path class="ai" d="M44,72 L51,52 L58,72 M46.5,65.5 H55.5 M64,52 V72"/></svg>')

PERSON = '<svg class="person" viewBox="0 0 40 44" aria-hidden="true"><g fill="none" stroke="#7A6AF0" stroke-width="2" stroke-linecap="round"><circle cx="20" cy="11" r="9"/><path d="M2,42 C2,27 38,27 38,42"/></g></svg>'
LOGO = '<svg class="logo" viewBox="0 0 40 40" aria-hidden="true"><rect x="5" y="5" width="30" height="30" rx="9" fill="none" stroke="url(#lg)" stroke-width="7" transform="rotate(-10 20 20)"/></svg>'


def tline():  # الشريحة 8: الخط الزمني بحلقاته وأقواسه وموصّلاته
    s = '<svg class="tline" viewBox="0 0 1280 720" aria-hidden="true">'
    s += '<path d="M100,393 H1180"/><circle class="dot" cx="100" cy="393" r="4"/><circle class="dot" cx="1180" cy="393" r="4"/>'
    rings = [(228, 257, -95, 25, False), (765, 257, -80, 120, True), (497, 538, -95, 45, False), (1035, 538, -85, 130, True)]
    for cx, cy, a0, a1, ccw in rings:
        s += _arc(cx, cy, 50, 0, 359.9, 'ring') + _arc(cx, cy, 78, a0, a1, 'cres', ccw=ccw)
    # موصّلات: العلوية من نقطة تحت الحلقة إلى الخط، والسفلية من الخط إلى نقطة فوق الحلقة
    for x in (225, 767):
        s += '<path d="M%d,341 V352 L%d,393"/><circle class="dot" cx="%d" cy="341" r="4"/><circle class="dot" cx="%d" cy="393" r="4"/>' % (x, x + 37, x, x + 37)
    for x in (490, 1020):
        s += '<path d="M%d,393 V404 L%d,440"/><circle class="dot" cx="%d" cy="393" r="4"/><circle class="dot" cx="%d" cy="440" r="4"/>' % (x, x + 33, x, x + 33)
    return s + '</svg>'


def det(n, title_key, alt_key, img):  # الشرائح 5–7
    return ('\n    ' + deco_b() + '\n    <h1 class="ttl">§%s§</h1>\n    ' % title_key + UL + TL_DET
            + '\n    <p class="p p1">§LP1§</p>\n    <p class="p p2">§LP2§</p>'
            + '\n    <img class="ph holo" alt="§%s§" src="%s%s.jpg">\n  ' % (alt_key, A, img))


S1 = ('\n    <img class="ph face fade-s" alt="§ALT_FACE§" src="%sface.jpg">\n    ' % A + LOGO + '<p class="logo-t">§LOGO§</p>'
      '\n    <h1 class="t1">§T1§</h1>\n    <h1 class="t2">§T2§</h1>\n    ' + PERSON + '<div class="sep"></div>'
      '<p class="name">§NAME§</p><p class="role">§ROLE§</p>\n  ')

S2 = ('\n    ' + deco_a() + '\n    <h1 class="ttl top">§TTL2§</h1>' + TL_TOP
      + ''.join('\n    <div class="chip c%d">%s<span>0%d</span></div><div class="it i%d"><h2>§S2H%d§</h2><p>§S2P§</p></div>' % (i, CHIP, i, i, i) for i in (1, 2, 3, 4))
      + '\n  ')

S3 = ('\n    ' + deco_b(ring_right=True) + '\n    <img class="ph robot" alt="§ALT_ROBOT§" src="%srobot.jpg">' % A
      + '\n    <p class="kick">§S3K§</p>\n    <h1 class="ttl">§S3T§</h1>'
      + '\n    <svg class="tl" viewBox="0 0 640 200" aria-hidden="true"><path d="M450,18 L470,18 L525,65 L605,65"/><circle class="dot" cx="450" cy="18" r="4"/><circle class="dot" cx="605" cy="65" r="4"/>'
        '<path d="M17,132 L40,132 L95,185 L430,185"/><circle class="dot" cx="17" cy="132" r="4"/><circle class="dot" cx="430" cy="185" r="4"/></svg>'
      + '\n    <p class="p p1">§LP1§</p>\n    <p class="p p2">§LP2§</p>\n  ')

S4 = ('\n    ' + deco_a() + '\n    <h1 class="ttl top">§TTL4§</h1>' + TL_TOP
      + ''.join('\n    <div class="card k%d"><div class="bg"></div><div class="ic">%s</div><h2>§C%dH§</h2><p>§C4P§</p></div>' % (i, ic, i) for i, ic in ((1, ICON_AGENTIC), (2, ICON_GEN), (3, ICON_SUPER)))
      + '\n  ')

S8 = ('\n    ' + deco_a() + '\n    <h1 class="ttl top">§TTL8§</h1>' + TL_TOP + '\n    ' + tline()
      + '\n    <p class="yr y1">2023</p><p class="yr y2">2025</p><p class="yr y3">2024</p><p class="yr y4">2026</p>'
      + ''.join('\n    <div class="lb l%d"><h2>§PH§</h2><p>§SP§</p></div>' % i for i in (1, 2, 3, 4)) + '\n  ')

S9 = ('\n    ' + deco_b() + '\n    <img class="ph hand" alt="§ALT_HAND§" src="%sai-hand.jpg">' % A
      + '\n    <p class="kick">§S9K§</p>\n    <h1 class="ttl">§S9T§</h1>' + UL
      + '\n    <svg class="tl a" viewBox="0 0 210 70" aria-hidden="true"><path d="M195,15 L120,15 L75,58 L42,58"/><circle class="dot" cx="195" cy="15" r="4"/><circle class="dot" cx="42" cy="58" r="4"/>'
        '<path d="M65,37 L45,37 L0,8"/><circle class="dot" cx="65" cy="37" r="4"/></svg>'
      + '\n    <svg class="tl b" viewBox="0 0 175 70" aria-hidden="true"><path d="M10,9 L37,9 L85,55 L160,55"/><circle class="dot" cx="10" cy="9" r="4"/><circle class="dot" cx="160" cy="55" r="4"/></svg>'
      + '\n    <p class="p p1">§LP1§</p>\n    <p class="p p2">§LP2§</p>\n  ')

SLIDES = [S1, S2, S3, S4,
          det(5, 'S5T', 'ALT_H5', 'holo-agentic'), det(6, 'S6T', 'ALT_H6', 'holo-generative'), det(7, 'S7T', 'ALT_H7', 'holo-super'),
          S8, S9]

STR = {
    'ar': {
        'ALT_FACE': 'رسم توضيحي لوجه رقمي بشبكة عصبية', 'LOGO': 'شعار الشركة',
        'T1': 'التدريب على', 'T2': 'الذكاء الاصطناعي', 'NAME': 'اسم مقدّم العرض', 'ROLE': 'المسمّى الوظيفي',
        'TTL2': 'شريحة جدول المحتويات',
        'S2H1': 'المقدمة', 'S2H2': 'أنواع الذكاء الاصطناعي', 'S2H3': 'تطور الذكاء الاصطناعي', 'S2H4': 'المخاطر والتحديات',
        'S2P': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي.',
        'ALT_ROBOT': 'روبوت يحمل حرفي الذكاء الاصطناعي', 'S3K': 'المقدمة', 'S3T': 'شريحة ما هو<br>الذكاء الاصطناعي',
        'LP1': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي.',
        'LP2': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي.',
        'TTL4': 'شريحة أنواع الذكاء الاصطناعي',
        'C1H': 'الذكاء الاصطناعي الوكيل', 'C2H': 'الذكاء الاصطناعي التوليدي', 'C3H': 'الذكاء الاصطناعي الفائق',
        'C4P': 'أضف النص المطلوب هنا. هذا نص توضيحي. هذا نص توضيحي. أضف النص المطلوب هنا.',
        'S5T': 'الذكاء الاصطناعي الوكيل', 'S6T': 'الذكاء الاصطناعي التوليدي', 'S7T': 'الذكاء الاصطناعي الفائق',
        'ALT_H5': 'هولوغرام شريحة ذكاء اصطناعي فوق منصة إسقاط', 'ALT_H6': 'هولوغرام شريحة ذكاء اصطناعي بتروس فوق منصة إسقاط', 'ALT_H7': 'هولوغرام دماغ إلكتروني بتروس فوق منصة إسقاط',
        'TTL8': 'شريحة تطور الذكاء الاصطناعي', 'PH': 'عنوان نائب', 'SP': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
        'ALT_HAND': 'يد روبوتية تلمس حرفي الذكاء الاصطناعي المجسّمين', 'S9K': 'ابدأ مع', 'S9T': 'الذكاء<br>الاصطناعي',
    },
    'en': {
        'ALT_FACE': 'Illustration of a digital face with a neural mesh', 'LOGO': 'Company Logo',
        'T1': 'AI TRAINING', 'T2': 'PRESENTATION', 'NAME': 'Presenter Name', 'ROLE': 'Designation',
        'TTL2': 'Table of Content Slide',
        'S2H1': 'INTRODUCTION', 'S2H2': 'TYPES OF AI', 'S2H3': 'EVOLUTION OF AI', 'S2H4': 'RISKS &amp; CHALLENGES',
        'S2P': 'This is a sample text. Insert your desired text here. This is a sample text.',
        'ALT_ROBOT': 'Robot holding the letters AI', 'S3K': 'INTRODUCTION', 'S3T': 'What Is Artificial<br>Intelligence Slide',
        'LP1': 'This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text.',
        'LP2': 'This is a sample text. Insert your desired text here. This is a sample text.',
        'TTL4': 'Types of AI Slide',
        'C1H': 'Agentic AI', 'C2H': 'Generative AI', 'C3H': 'Super AI',
        'C4P': 'Insert your desired text here. This is a sample text. This is a sample text. Insert your desired text here.',
        'S5T': 'Agentic AI', 'S6T': 'Generative AI', 'S7T': 'Super AI',
        'ALT_H5': 'Hologram of an AI chip above a projection base', 'ALT_H6': 'Hologram of an AI chip with gears above a projection base', 'ALT_H7': 'Hologram of a circuit brain with gears above a projection base',
        'TTL8': 'The Evolution of AI Slide', 'PH': 'Placeholder', 'SP': 'This is a sample text. Insert your desired text here.',
        'ALT_HAND': 'Robotic hand touching 3D AI letters', 'S9K': 'Get Started With', 'S9T': 'Artificial<br>Intelligence',
    },
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'hr', 'icon': '🤖',
    'name': 'عرض التدريب على الذكاء الاصطناعي', 'nameEn': 'AI Training Presentation',
    'description': 'تسع شرائح: غلاف، محتويات، تعريف، أنواع الذكاء الاصطناعي، ثلاث شرائح تفصيل، تطور زمني، والبدء',
    'descriptionEn': 'Nine slides: cover, contents, definition, AI types, three detail slides, evolution timeline, get started',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'جدول المحتويات', 'titleEn': 'Table of Contents'},
        {'title': 'ما هو الذكاء الاصطناعي', 'titleEn': 'What Is Artificial Intelligence'},
        {'title': 'أنواع الذكاء الاصطناعي', 'titleEn': 'Types of AI'},
        {'title': 'الذكاء الاصطناعي الوكيل', 'titleEn': 'Agentic AI'},
        {'title': 'الذكاء الاصطناعي التوليدي', 'titleEn': 'Generative AI'},
        {'title': 'الذكاء الاصطناعي الفائق', 'titleEn': 'Super AI'},
        {'title': 'تطور الذكاء الاصطناعي (خط زمني)', 'titleEn': 'The Evolution of AI (timeline)', 'needsData': True},
        {'title': 'ابدأ مع الذكاء الاصطناعي', 'titleEn': 'Get Started With AI'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط (الخلفية المتدرّجة، زخارف الدوائر الإلكترونية، الشرائح المرقّمة، البطاقات، الحلقات الزمنية) وغيّر النصوص والسنوات فقط؛ الرسوم التوضيحية صور ثابتة في assets لا تُستبدل.',
}
