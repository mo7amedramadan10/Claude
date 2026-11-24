# -*- coding: utf-8 -*-
# نموذج «تهيئة العملاء»: ثلاث عشرة شريحة بهوية زرقاء وليمونية على خلفية شبكية (غلاف، مقدمة، من نحن، إنجازات وشهادات، خدمات،
# أهداف التهيئة، الجدول الزمني، الأدوار، خطة التواصل، المستندات، الأسئلة الشائعة، الخطوات التالية، شكر).
KEY = 'sl-client-onboarding'
TITLE = {'ar': 'تهيئة العملاء', 'en': 'Client Onboarding'}
COMMENT = 'نموذج «تهيئة العملاء»: خلفيات زرقاء أو رمادية بشبكة خفيفة، بطاقات بيضاء مدوّرة، دوائر ليمونية بسهم أزرق أو رقم، أرقام زرقاء كبيرة، وصور فوتوغرافية بقصّ مدوّر ذي زاوية مائلة (clip-path بمسار يُعكس للعربية).'
VARS = ('--ink: #222222; --navy: #14203C; --accent: #0B57E8; --blue: #0B57E8; --lime: #9EF131; --gray: #595959; '
        '--light: #F2F2F2; --ring: #EDEDED; --ice: #E6EEFC; --bg: #F2F2F2;')

FONTS = {'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@400;600;700;800&display=swap" rel="stylesheet">'}
FONT_STACK = {'en': "'Open Sans', 'Segoe UI', Arial, sans-serif"}

A = '/templates/freeform/sl-client-onboarding/assets/'


# ---------- مسارات القصّ للصور: مضلّع بزوايا مدوّرة، ونسخة معكوسة للعربية ----------
def rpath(pts, rs):
    n = len(pts); out = []
    def lerp(a, b, t): return (a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t)
    def dist(a, b): return ((a[0] - b[0]) ** 2 + (a[1] - b[1]) ** 2) ** .5
    segs = []
    for i in range(n):
        p, a, b = pts[i], pts[i - 1], pts[(i + 1) % n]
        r = rs[i] if isinstance(rs, list) else rs
        p_in = lerp(p, a, min(r / dist(p, a), .5)); p_out = lerp(p, b, min(r / dist(p, b), .5))
        segs.append((p_in, p, p_out))
    d = 'M%.1f %.1f ' % segs[0][2]
    for i in list(range(1, n)) + [0]:
        pi, p, po = segs[i]
        d += 'L%.1f %.1f Q%.1f %.1f %.1f %.1f ' % (pi[0], pi[1], p[0], p[1], po[0], po[1])
    return d + 'Z'


def clip(cls, w, pts, rs, flip=False):
    # flip: الصورة نفسها تُعكس في العربية (لأن طرفها يحمل جزءًا من الدائرة المتراكبة في الأصل)، فيبقى المسار كما هو
    d = rpath(pts, rs); m = rpath([(w - x, y) for x, y in pts], rs)
    if flip:
        return ("  .%s { clip-path: path('%s'); }\n  html[dir=\"rtl\"] .%s { transform: scaleX(-1); }\n" % (cls, d, cls))
    return ("  .%s { clip-path: path('%s'); }\n  html[dir=\"rtl\"] .%s { clip-path: path('%s'); }\n" % (cls, d, cls, m))


CLIPS = (clip('ph-s1', 466, [(0, 0), (466, 0), (466, 508), (164, 508), (110, 322), (0, 322)], [22, 22, 22, 22, 22, 22], True)
         + clip('ph-s5', 658, [(0, 0), (658, 0), (658, 319), (220, 319), (124, 230), (0, 230)], [22, 22, 22, 22, 22, 22])
         + clip('ph-s10', 581, [(0, 0), (581, 0), (581, 296), (408, 296), (342, 414), (0, 414)], [22, 22, 22, 22, 22, 22])
         + clip('ph-s12', 380, [(0, 0), (271, 0), (271, 140), (380, 196), (380, 471), (0, 471)], [22, 22, 22, 22, 22, 22], True))

STYLE = """  /* الخلفيات: أزرق أو رمادي بشبكة خطوطها كل 55.65px */
  .frame.blue { background-color: #0958E8; background-image: linear-gradient(90deg, rgb(255 255 255 / .055) 2px, transparent 2px), linear-gradient(180deg, rgb(255 255 255 / .055) 2px, transparent 2px); background-size: 55.652px 55.652px; background-position: 55px 53px; }
  .frame.gray { background-color: #F2F2F2; background-image: linear-gradient(90deg, rgb(20 30 80 / .022) 2px, transparent 2px), linear-gradient(180deg, rgb(20 30 80 / .022) 2px, transparent 2px); background-size: 55.652px 55.652px; background-position: 55px 53px; }
  .abs { position: absolute; margin: 0; }
  .card { position: absolute; background: #FFFFFF; border-radius: 14px; }
  /* العنوان والعنوان الفرعي */
  .ttl { position: absolute; top: 40px; inset-inline-start: 64px; margin: 0; font-size: 52px; line-height: 66px; font-weight: 700; letter-spacing: -1.6px; color: var(--navy); white-space: nowrap; }
  .sub { position: absolute; top: 113px; inset-inline-start: 64px; margin: 0; font-size: 20px; line-height: 30px; font-weight: 700; color: var(--blue); }
  .blue .ttl { color: #FFFFFF; } .blue .sub, .s2 .sub { color: var(--lime); }
  html[lang="ar"] .ttl { letter-spacing: 0; font-weight: 800; font-size: 50px; }
  html[lang="ar"] .sub { font-size: 21px; }
  /* نصوص */
  .h { margin: 0; font-size: 22.5px; line-height: 29px; font-weight: 700; color: var(--ink); text-transform: uppercase; }
  .p { margin: 0; font-size: 18px; line-height: 25.5px; font-weight: 400; color: var(--gray); }
  html[lang="ar"] .p { font-size: 18px; line-height: 27px; }
  html[lang="ar"] .h { font-size: 22px; line-height: 31px; }
  /* الدوائر الليمونية بسهم أو رقم، بحلقة حولها */
  .dot { position: absolute; border-radius: 50%; background: var(--lime); color: var(--blue); display: grid; place-items: center; box-shadow: 0 0 0 var(--rw, 10px) var(--rc, var(--ring)); }
  .dot svg { width: 52%; height: 52%; }
  html[dir="rtl"] .dot .arw { transform: scaleX(-1); }
  .num { font-weight: 700; font-size: 34px; line-height: 1; color: var(--blue); }
  /* دوائر الأيقونات الزرقاء الفاتحة */
  .ico { position: absolute; width: 62px; height: 62px; border-radius: 50%; background: var(--ice); color: var(--blue); display: grid; place-items: center; }
  .ico svg { width: 30px; height: 30px; }
  .blue .ico.t, .s9 .ico.t, .s13 .ico { background: rgb(255 255 255 / .14); color: #FFFFFF; }
  /* أرقام الإحصاءات */
  .big { margin: 0; font-size: 74px; line-height: 80px; font-weight: 700; color: var(--blue); letter-spacing: -1.5px; }
  .lbl { margin: 0; font-size: 20px; line-height: 28px; color: var(--gray); }
  .pill { position: absolute; border-radius: 999px; background: var(--lime); display: grid; place-items: center; font-size: 20px; font-weight: 700; color: var(--navy); text-transform: uppercase; }
  .ph { position: absolute; display: block; object-fit: cover; }

  /* ===== 1) الغلاف ===== */
  .s1 .sheet { position: absolute; top: 104px; inset-inline-start: 20px; width: 1240px; height: 587px; background: #FFFFFF; border-radius: 22px; }
  .s1 .logo { position: absolute; top: 140px; inset-inline-start: 80px; display: flex; align-items: center; gap: 18px; }
  .s1 .logo svg { width: 48px; height: 52px; color: var(--lime); }
  .s1 .logo p { margin: 0; font-size: 16px; line-height: 23.5px; letter-spacing: 2.4px; color: #444; text-transform: uppercase; }
  html[lang="ar"] .s1 .logo p { letter-spacing: 0; font-size: 17px; }
  .s1 .t1 { top: 262px; inset-inline-start: 64px; font-size: 152px; line-height: 170px; font-weight: 800; color: var(--navy); letter-spacing: -6px; }
  .s1 .t2 { top: 418px; inset-inline-start: 76px; font-size: 80px; line-height: 92px; font-weight: 700; color: var(--blue); letter-spacing: -2.5px; }
  .s1 .t3 { top: 520px; inset-inline-start: 79px; font-size: 32px; line-height: 42px; font-weight: 700; color: var(--navy); letter-spacing: -0.6px; }
  html[lang="ar"] .s1 .t1 { font-size: 132px; line-height: 160px; top: 252px; letter-spacing: 0; }
  html[lang="ar"] .s1 .t2 { font-size: 84px; line-height: 100px; top: 404px; letter-spacing: 0; }
  html[lang="ar"] .s1 .t3 { letter-spacing: 0; top: 516px; }
  .s1 .pill { top: 594px; inset-inline-start: 68px; width: 281px; height: 64px; }
  .s1 .ph { top: 143px; inset-inline-start: 754px; width: 466px; height: 508px; }
  .s1 .dot { top: 315px; inset-inline-start: 707px; width: 90px; height: 90px; --rc: #FFFFFF; }
  .s1 .dot svg { width: 50px; height: 50px; }

  /* ===== 2) المقدمة ===== */
  .s2 .side { position: absolute; top: 0; inset-inline-start: 0; width: 568px; height: 720px; background: #0958E8; border-start-end-radius: 22px; border-end-end-radius: 22px; }
  .s2 .ttl { color: #FFFFFF; }
  .s2 .dot.big1 { top: 286px; inset-inline-start: 59px; width: 90px; height: 90px; --rw: 0px; }
  .s2 .wh { top: 410px; inset-inline-start: 64px; color: #FFFFFF; }
  .s2 .wp1 { top: 455px; inset-inline-start: 64px; width: 420px; color: #E2EAFD; }
  .s2 .wp2 { top: 565px; inset-inline-start: 64px; width: 420px; color: #E2EAFD; }
  .s2 .card { width: 314px; height: 265px; border-radius: 12px; }
  .s2 .c1 { top: 75px; inset-inline-start: 601px; } .s2 .c2 { top: 75px; inset-inline-start: 950px; }
  .s2 .c3 { top: 389px; inset-inline-start: 601px; } .s2 .c4 { top: 389px; inset-inline-start: 950px; }
  .s2 .dot.n { width: 58px; height: 58px; --rw: 10px; }
  .s2 .n1 { top: 47px; inset-inline-start: 634px; } .s2 .n2 { top: 47px; inset-inline-start: 982px; }
  .s2 .n3 { top: 361px; inset-inline-start: 634px; } .s2 .n4 { top: 361px; inset-inline-start: 982px; }
  .s2 .ch { position: absolute; width: 260px; }
  .s2 .ch .h { margin-bottom: 18px; }
  .s2 .q1 { top: 150px; inset-inline-start: 636px; } .s2 .q2 { top: 150px; inset-inline-start: 985px; }
  .s2 .q3 { top: 463px; inset-inline-start: 636px; } .s2 .q4 { top: 463px; inset-inline-start: 985px; }

  /* ===== 3) من نحن ===== */
  .s3 .panel { position: absolute; top: 15px; inset-inline-start: 614px; width: 650px; height: 675px; background: #FFFFFF; border-radius: 26px; }
  .s3 .dot { top: 71px; inset-inline-start: 565px; width: 90px; height: 90px; --rw: 11px; }
  .s3 .intro { top: 63px; inset-inline-start: 700px; width: 470px; }
  .s3 .bh1 { top: 259px; inset-inline-start: 67px; } .s3 .bp1 { top: 295px; inset-inline-start: 68px; width: 480px; }
  .s3 .bh2 { top: 489px; inset-inline-start: 67px; } .s3 .bp2 { top: 524px; inset-inline-start: 67px; width: 480px; }
  .s3 .st { position: absolute; width: 250px; }
  .s3 .st .ico { position: static; margin-inline-start: 4px; margin-bottom: 14px; }
  .s3 .st .big { margin-top: 0; }
  .s3 .st1 { top: 255px; inset-inline-start: 700px; } .s3 .st2 { top: 255px; inset-inline-start: 996px; }
  .s3 .st3 { top: 485px; inset-inline-start: 700px; } .s3 .st4 { top: 485px; inset-inline-start: 996px; }

  /* ===== 4) الإنجازات والشهادات ===== */
  .s4 .ca { top: 215px; inset-inline-start: 20px; width: 513px; height: 482px; border-radius: 16px; }
  .s4 .cb { top: 215px; inset-inline-start: 548px; width: 712px; height: 482px; border-radius: 16px; }
  .s4 .dot { top: 170px; inset-inline-start: 71px; width: 92px; height: 92px; --rw: 10px; --rc: #0B57E8; }
  .s4 .dot svg { width: 48px; height: 48px; }
  html[dir="rtl"] .s4 .dot svg { transform: scaleX(-1); }
  .s4 .quote { top: 314px; inset-inline-start: 72px; width: 420px; font-size: 22.5px; line-height: 32px; font-weight: 700; color: var(--ink); }
  html[lang="ar"] .s4 .quote { font-size: 23px; line-height: 36px; }
  .s4 .pill { top: 556px; inset-inline-start: 73px; width: 281px; height: 63px; }
  .s4 .bp { top: 314px; inset-inline-start: 606px; width: 600px; }
  .s4 .st { position: absolute; width: 280px; }
  .s4 .st .ico { position: static; margin-inline-start: 4px; margin-bottom: 8px; }
  .s4 .st1 { top: 440px; inset-inline-start: 606px; } .s4 .st2 { top: 440px; inset-inline-start: 924px; }
  .s4 .st .big { font-size: 72px; line-height: 80px; }

  /* ===== 5) الخدمات ===== */
  .s5 .intro { top: 244px; inset-inline-start: 67px; width: 470px; }
  .s5 .ph { top: 33px; inset-inline-start: 600px; width: 658px; height: 319px; }
  .s5 .card { top: 429px; width: 400px; height: 270px; border-radius: 12px; }
  .s5 .c1 { inset-inline-start: 20px; } .s5 .c2 { inset-inline-start: 440px; } .s5 .c3 { inset-inline-start: 860px; }
  .s5 .dot { top: 390px; width: 76px; height: 76px; --rw: 11px; }
  .s5 .dot svg { width: 40px; height: 40px; }
  .s5 .d1 { inset-inline-start: 63px; } .s5 .d2 { inset-inline-start: 483px; } .s5 .d3 { inset-inline-start: 903px; }
  .s5 .sv { position: absolute; top: 508px; width: 330px; }
  .s5 .sv .h { margin-bottom: 4px; }
  .s5 .v1 { inset-inline-start: 67px; } .s5 .v2 { inset-inline-start: 487px; } .s5 .v3 { inset-inline-start: 907px; }

  /* ===== 6) أهداف التهيئة ===== */
  .s6 .base { position: absolute; top: 437px; inset-inline-start: 0; width: 1280px; height: 283px; background: #0958E8; border-radius: 30px 30px 0 0; }
  .s6 .card { top: 232px; width: 275px; height: 309px; border-radius: 12px; }
  .s6 .c1 { inset-inline-start: 20px; } .s6 .c2 { inset-inline-start: 342px; } .s6 .c3 { inset-inline-start: 663px; } .s6 .c4 { inset-inline-start: 985px; }
  .s6 .dot { top: 199px; width: 76px; height: 76px; --rw: 10px; }
  .s6 .dot .num { color: #FFFFFF; font-size: 44px; }
  .s6 .d1 { inset-inline-start: 55px; } .s6 .d2 { inset-inline-start: 377px; } .s6 .d3 { inset-inline-start: 698px; } .s6 .d4 { inset-inline-start: 1020px; }
  .s6 .go { position: absolute; width: 30px; height: 30px; color: var(--blue); }
  html[dir="rtl"] .s6 .go { transform: scaleX(-1); }
  .s6 .a1 { top: 295px; inset-inline-start: 304px; } .s6 .a2 { top: 303px; inset-inline-start: 625px; } .s6 .a3 { top: 310px; inset-inline-start: 947px; }
  .s6 .gl { position: absolute; top: 329px; width: 225px; }
  .s6 .gl .h { font-size: 20px; line-height: 25px; margin-bottom: 23px; }
  .s6 .g1 { inset-inline-start: 60px; } .s6 .g2 { inset-inline-start: 381px; } .s6 .g3 { inset-inline-start: 703px; } .s6 .g4 { inset-inline-start: 1024px; }
  .s6 .tip { top: 604px; inset-inline-start: 55px; width: 50px; height: 50px; --rw: 0px; }
  .s6 .tip svg { width: 28px; height: 28px; }
  .s6 .foot { top: 603px; inset-inline-start: 133px; width: 1090px; color: #FFFFFF; }

  /* ===== 7) الجدول الزمني و9) خطة التواصل: حبوب الأسابيع ===== */
  .wk { position: absolute; width: 225px; height: 61px; }
  .wk::before { content: ''; position: absolute; inset: -11px; border-radius: 999px; background: var(--ring); }
  .wk .pill { inset: 0; font-size: 20.5px; }
  .wk1 { inset-inline-start: 63px; } .wk2 { inset-inline-start: 362px; } .wk3 { inset-inline-start: 661px; } .wk4 { inset-inline-start: 960px; }
  .ph-col { position: absolute; width: 270px; }
  .pc1 { inset-inline-start: 67px; } .pc2 { inset-inline-start: 366px; } .pc3 { inset-inline-start: 666px; } .pc4 { inset-inline-start: 965px; }
  .s7 .panel { top: 249px; inset-inline-start: 20px; width: 1240px; height: 448px; border-radius: 16px; }
  .s7 .wk { top: 220px; }
  .s7 .intro { top: 45px; inset-inline-start: 783px; width: 450px; }
  .s7 .ph-col { top: 340px; }
  .s7 .ph-col .h { margin-bottom: 38px; }
  .s7 ul { list-style: none; margin: 0; padding: 0; }
  .s7 li { position: relative; padding-inline-start: 19px; font-size: 18px; line-height: 26px; color: var(--gray); margin-bottom: 10px; }
  html[lang="ar"] .s7 li { line-height: 27px; margin-bottom: 9px; }
  .s7 li::before { content: ''; position: absolute; inset-inline-start: 2px; top: 6px; width: 9px; height: 9px; border-radius: 50%; background: var(--blue); box-shadow: 0 0 0 2px #FFFFFF, 0 0 0 3.2px #6E9BF0; }
  .s7 .pc1, .s7 .pc2, .s7 .pc3, .s7 .pc4 { width: 250px; }
  .s7 .pc1 li, .s7 .pc2 li, .s7 .pc3 li, .s7 .pc4 li { margin-inline-start: -10px; }

  /* ===== 8) الأدوار والمسؤوليات ===== */
  .s8 .ca { top: 192px; inset-inline-start: 20px; width: 908px; height: 478px; border-radius: 16px; }
  .s8 .cb { top: 192px; inset-inline-start: 943px; width: 317px; height: 478px; border-radius: 16px; }
  .s8 table { position: absolute; top: 220px; inset-inline-start: 54px; width: 844px; border-collapse: collapse; }
  .s8 th { height: 68px; background: var(--lime); color: var(--blue); font-size: 22px; font-weight: 700; text-align: start; padding: 0; }
  .s8 td { height: 69px; font-size: 18px; color: var(--gray); text-align: start; padding: 0; border-bottom: 1px solid #9A9A9A; }
  .s8 tr:last-child td { border-bottom: 0; }
  .s8 tbody tr:nth-child(even) td { background: #F3FDEA; }
  .s8 td:first-child { font-weight: 700; color: var(--ink); }
  .s8 th:first-child, .s8 td:first-child { padding-inline-start: 21px; width: 273px; }
  .s8 th:nth-child(2), .s8 td:nth-child(2) { width: 288px; }
  .s8 .bp { top: 225px; inset-inline-start: 976px; width: 260px; }
  .s8 .st { position: absolute; top: 432px; inset-inline-start: 966px; width: 260px; }
  .s8 .st .ico { position: static; margin-bottom: 18px; }
  .s8 .st .big { margin-inline-start: 13px; font-size: 72px; line-height: 76px; } .s8 .st .lbl { margin-inline-start: 11px; }

  /* ===== 9) خطة التواصل ===== */
  .s9 .base { position: absolute; top: 270px; inset-inline-start: 0; width: 1280px; height: 450px; background: #0958E8; border-radius: 30px 30px 0 0; }
  .s9 .panel { top: 227px; inset-inline-start: 20px; width: 1240px; height: 315px; border-radius: 16px; }
  .s9 .wk { top: 197px; }
  .s9 .ph-col { top: 318px; width: 260px; }
  .s9 .ph-col .h { margin-bottom: 25px; }
  .s9 .pc1 { inset-inline-start: 64px; }
  .s9 .kp { position: absolute; top: 567px; display: grid; grid-template-columns: 62px auto auto; column-gap: 18px; align-items: center; }
  .s9 .kp .ico { position: static; grid-row: 1; }
  .s9 .kp .v { margin: 0; font-size: 46px; line-height: 50px; font-weight: 700; color: #FFFFFF; letter-spacing: -0.5px; }
  .s9 .kp .k { margin: 0; font-size: 20px; line-height: 25px; font-weight: 700; color: #FFFFFF; }
  .s9 .kp .p { grid-column: 2 / 4; color: #DCE6FD; margin-top: 4px; width: 300px; }
  html[lang="ar"] .s9 .kp .p { margin-top: 2px; }
  .s9 .k1 { inset-inline-start: 57px; } .s9 .k2 { inset-inline-start: 493px; }
  .s9 .k1 .v { width: 96px; } .s9 .k2 .v { width: 100px; }

  /* ===== 10) المستندات المطلوبة ===== */
  .s10 .base { position: absolute; top: 298px; inset-inline-start: 0; width: 1280px; height: 422px; background: var(--lime); }
  .s10 .ttl { line-height: 63px; top: 42px; white-space: normal; width: 520px; }
  .s10 .sub { top: 177px; }
  .s10 .ph { top: 259px; inset-inline-start: 59px; width: 581px; height: 414px; }
  .s10 .panel { top: 36px; inset-inline-start: 669px; width: 595px; height: 639px; border-radius: 26px; }
  .s10 .dc { position: absolute; width: 230px; }
  .s10 .dc .ico { position: static; margin-inline-start: 21px; margin-bottom: 21px; }
  .s10 .dc .h { margin-bottom: 11px; }
  .s10 .dc1 { top: 79px; inset-inline-start: 729px; } .s10 .dc2 { top: 79px; inset-inline-start: 1009px; }
  .s10 .dc3 { top: 392px; inset-inline-start: 729px; } .s10 .dc4 { top: 392px; inset-inline-start: 1009px; }
  .s10 .dc .ico { margin-inline-start: -10px; }

  /* ===== 11) الأسئلة الشائعة ===== */
  .s11 .ico.t { top: 62px; inset-inline-start: 686px; background: #DDE5F5; }
  .s11 .intro { top: 67px; inset-inline-start: 779px; width: 450px; }
  .s11 .panel { top: 175px; inset-inline-start: 54px; width: 1180px; height: 522px; border-radius: 22px; }
  .s11 .dot { width: 90px; height: 90px; inset-inline-start: 22px; --rw: 12px; }
  .s11 .f1 { top: 209px; } .s11 .f2 { top: 387px; } .s11 .f3 { top: 566px; }
  .s11 .qa { position: absolute; inset-inline-start: 155px; width: 1000px; }
  .s11 .qa .h { margin-bottom: 2px; }
  .s11 .a1 { top: 212px; } .s11 .a2 { top: 391px; } .s11 .a3 { top: 569px; }
  .s11 .hr { position: absolute; inset-inline-start: 142px; width: 1044px; height: 1px; background: #D9DFE6; }
  .s11 .r1 { top: 343px; } .s11 .r2 { top: 522px; }

  /* ===== 12) الخطوات التالية ===== */
  .s12 .ph { top: 204px; inset-inline-start: 54px; width: 380px; height: 471px; }
  .s12 .dot { top: 268px; inset-inline-start: 295px; width: 90px; height: 90px; --rw: 10px; --rc: #F4F4F4; }
  .s12 .bar { position: absolute; inset-inline-start: 505px; width: 750px; height: 61px; border-radius: 9px; background: #FFFFFF; display: flex; align-items: center; gap: 0; padding-inline-start: 26px; }
  .s12 .bar svg { width: 30px; height: 30px; color: var(--blue); flex: none; }
  .s12 .bar .h { font-size: 20px; margin-inline-start: 28px; color: var(--ink); }
  .s12 .bar.on { background: var(--lime); }
  .s12 .b1 { top: 110px; } .s12 .b2 { top: 256px; } .s12 .b3 { top: 401px; } .s12 .b4 { top: 547px; }
  .s12 .np { position: absolute; inset-inline-start: 529px; width: 690px; }
  .s12 .n1 { top: 179px; } .s12 .n2 { top: 325px; } .s12 .n3 { top: 470px; } .s12 .n4 { top: 616px; }

  /* ===== 13) الشكر ===== */
  .s13 .logo { position: absolute; top: 60px; inset-inline-start: 89px; display: flex; align-items: center; gap: 16px; }
  .s13 .logo svg { width: 46px; height: 52px; color: #FFFFFF; }
  .s13 .logo p { margin: 0; font-size: 16px; line-height: 23.5px; letter-spacing: 2.4px; color: #FFFFFF; text-transform: uppercase; }
  html[lang="ar"] .s13 .logo p { letter-spacing: 0; font-size: 17px; }
  .s13 .thx { top: 189px; inset-inline-start: 89px; font-size: 29px; line-height: 38px; font-weight: 700; color: var(--lime); letter-spacing: -1.4px; }
  .s13 .msg { top: 238px; inset-inline-start: 89px; width: 1110px; font-size: 59px; line-height: 77px; font-weight: 700; color: #FFFFFF; letter-spacing: -1px; }
  html[lang="ar"] .s13 .thx { letter-spacing: 0; }
  html[lang="ar"] .s13 .msg { letter-spacing: 0; font-size: 58px; line-height: 84px; top: 232px; }
  .s13 .ct { position: absolute; top: 581px; display: flex; align-items: center; gap: 22px; }
  .s13 .ct .ico { position: static; }
  .s13 .ct span { font-size: 18px; color: #FFFFFF; }
  .s13 .ct1 { inset-inline-start: 90px; } .s13 .ct2 { inset-inline-start: 397px; }
""" + CLIPS

# ---------- أيقونات SVG ----------
SW = 'fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round"'
ICONS = {
    'arrow': '<svg class="arw" viewBox="0 0 24 24" aria-hidden="true" fill="currentColor"><path d="M8.2 4.2h11.6v11.6h-4.3V11.5l-8.4 8.4-3-3 8.4-8.4H8.2z"/></svg>',
    'logo': '<svg viewBox="0 0 24 26" aria-hidden="true"><path d="M5.2 4.6C10 2.8 15 2.6 19.8 4.4c1.4 5.2 1.3 10.7-.2 16.1-5 1.6-10 1.7-15 .1C3.4 15.3 3.6 9.9 5.2 4.6z" fill="none" stroke="currentColor" stroke-width="3.6" stroke-linejoin="round"/></svg>',
    'quote': '<svg viewBox="0 0 24 24" aria-hidden="true" fill="currentColor"><path d="M2.5 20v-6.8C2.5 8.4 4.6 5 9.6 3.8l1 2.4C8 7.1 6.9 8.8 6.8 11.1h3.8V20zm11 0v-6.8c0-4.8 2.1-8.2 7.1-9.4l1 2.4c-2.6.9-3.7 2.6-3.8 4.9h3.8V20z"/></svg>',
    'building': '<svg viewBox="0 0 24 24" aria-hidden="true" ' + SW + '><path d="M3 20h18M4 20V7h16v13M4 11h16M4 15h16M8 7v13M12 7v13M16 7v13M6 7V4h12v3"/></svg>',
    'thumb': '<svg viewBox="0 0 24 24" aria-hidden="true" ' + SW + '><path d="M7 10v10H4V10zM7 10l4-7c1.6 0 2.4 1.2 2 2.8L12.4 9H19c1.2 0 2 1 1.8 2.2l-1.3 7A2 2 0 0 1 17.5 20H7"/></svg>',
    'user': '<svg viewBox="0 0 24 24" aria-hidden="true" ' + SW + '><circle cx="12" cy="12" r="9"/><circle cx="12" cy="10" r="3"/><path d="M6.6 18.2c1.2-2 3.1-3 5.4-3s4.2 1 5.4 3"/></svg>',
    'map': '<svg viewBox="0 0 24 24" aria-hidden="true" ' + SW + '><path d="M3 6.5 8.5 4.5l7 2 5.5-2v13l-5.5 2-7-2L3 19.5zM8.5 4.5v13M15.5 6.5v13M3 11h18M3 15h5.5M15.5 11.5H21"/></svg>',
    'watch': '<svg viewBox="0 0 24 24" aria-hidden="true" ' + SW + '><circle cx="12" cy="14" r="6.5"/><path d="M12 14l2.5-2.5M10 3.5h4M12 3.5v4M17.5 8l1.2-1.2"/></svg>',
    'bag': '<svg viewBox="0 0 24 24" aria-hidden="true" ' + SW + '><path d="M10 6.5 8.5 3.5h7L14 6.5M14.5 6.5c3 2 4.8 5.3 4.8 8.5 0 3.3-2.2 5-5.3 5"/><circle cx="8.5" cy="15.5" r="2.5"/><path d="M4.5 21c.6-1.8 2.2-2.8 4-2.8s3.4 1 4 2.8z"/><path d="M10 6.5h4"/></svg>',
    'bulb': '<svg viewBox="0 0 24 24" aria-hidden="true" ' + SW.replace('1.7', '1.9') + '><path d="M9 17.5v-1.7c0-1.5-3.5-3.2-3.5-7A6.5 6.5 0 0 1 12 2.5a6.5 6.5 0 0 1 6.5 6.3c0 3.8-3.5 5.5-3.5 7v1.7zM9.5 20.5h5M10.5 17.5V12l1.5 1.5 1.5-1.5v5.5"/></svg>',
    'h24': '<svg viewBox="0 0 24 24" aria-hidden="true" ' + SW.replace('1.7', '1.9') + '><path d="M20.5 12A8.5 8.5 0 1 1 12 3.5" /><path d="M15.5 4.3l.4.2M18 6l.3.3M19.8 8.6l.1.4" stroke-width="2.2"/><path d="M7.5 10.2c.2-1 1-1.6 2-1.6 1.2 0 2 .7 2 1.7 0 1.6-3.9 3-3.9 5h4M16.2 15.3V8.6l-3.4 4.8h4.7"/></svg>',
    'care': '<svg viewBox="0 0 24 24" aria-hidden="true" ' + SW.replace('1.7', '1.9') + '><path d="M15 9.5c-1.6-1.2-4-3.1-4-5a2.2 2.2 0 0 1 4-1.2 2.2 2.2 0 0 1 4 1.2c0 1.9-2.4 3.8-4 5z"/><path d="M2.5 14.5h3v6h-3zM5.5 15.5l3.5-1.6c1-.4 2-.4 3 0l2.6 1c.8.3.8 1.6-.2 1.7L10.5 17M5.5 19.5l4.8 1.2c.9.2 1.8 0 2.6-.4l7.7-4.6c.9-.6.2-2-.8-1.6l-5.4 2.4"/></svg>',
    'arrowR': '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M3 12h16M12.5 5l7 7-7 7" fill="none" stroke="currentColor" stroke-width="3.2" stroke-linecap="round" stroke-linejoin="round"/></svg>',
    'chat': '<svg viewBox="0 0 24 24" aria-hidden="true" ' + SW + '><path d="M3.5 5.5h11v7.5H8.5L5.5 15.5V13h-2zM14.5 9h6v7.5h-2V19l-3-2.5h-5.5V13"/></svg>',
    'truck': '<svg viewBox="0 0 24 24" aria-hidden="true" ' + SW + '><path d="M8 6.5h9v9.5H8zM17 9.5h2.5l2 3v3.5H17"/><circle cx="10.5" cy="17.5" r="1.7"/><circle cx="18.5" cy="17.5" r="1.7"/><path d="M2.5 9h4M3.5 12h3M4.5 15h2"/></svg>',
    'factory': '<svg viewBox="0 0 24 24" aria-hidden="true" ' + SW + '><path d="M3 20.5V13l5-3v3l5-3v3l5-3V5h2.5v15.5zM6 16.5h2M10.5 16.5h2M15 16.5h2M18 5V3"/></svg>',
    'book': '<svg viewBox="0 0 24 24" aria-hidden="true" ' + SW + '><rect x="5.5" y="3" width="13" height="18" rx="1.5"/><path d="M8.5 3v18M11 7.5h5M11 11h5M11 14.5h3M3.5 7h3M3.5 11h3M3.5 15h3"/></svg>',
    'eye': '<svg viewBox="0 0 24 24" aria-hidden="true" ' + SW + '><path d="M2.5 12c2.5-3.6 5.6-5.4 9.5-5.4s7 1.8 9.5 5.4c-2.5 3.6-5.6 5.4-9.5 5.4S5 15.6 2.5 12z"/><path d="M9 12.4c.6-1.4 1.6-2.1 3-2.1s2.4.7 3 2.1"/><circle cx="12" cy="13" r="1.4"/></svg>',
    'docs': '<svg viewBox="0 0 24 24" aria-hidden="true" ' + SW + '><path d="M3 5.5h8v14H3zM13 5.5h8v14h-8zM5 9h4M5 12h4M5 15h4M15 9h4M15 12h4M15 15h4"/></svg>',
    'sign': '<svg viewBox="0 0 24 24" aria-hidden="true" ' + SW + '><path d="M3.5 7.5h15a2 2 0 0 1 0 4H15l-4.5 5.5"/><path d="M8 11.5H6.5a2 2 0 1 0 0 4H10"/><path d="M12.5 11.5c0 1.6 1.4 2.5 2.7 2"/></svg>',
    'mail': '<svg viewBox="0 0 24 24" aria-hidden="true" ' + SW.replace('1.7', '1.9') + '><path d="M3 10.5 12 4l9 6.5V20H3z"/><path d="M3 10.5l9 6 9-6"/><path d="M8.5 7.5h7v4"/></svg>',
    'tie': '<svg viewBox="0 0 24 24" aria-hidden="true" ' + SW.replace('1.7', '1.9') + '><path d="M10 2.5h4M10.5 6.5h3M12 6.5V9"/><circle cx="12" cy="13" r="4.5"/><path d="M12 13l1.6-1.4M10 21.5h4M12 17.5v4"/></svg>',
    'down': '<svg viewBox="0 0 24 24" aria-hidden="true" ' + SW.replace('1.7', '1.9') + '><circle cx="12" cy="12" r="9"/><path d="M12 7v9M8.2 12.5 12 16.3l3.8-3.8"/></svg>',
    'eye4': '<svg viewBox="0 0 24 24" aria-hidden="true" ' + SW.replace('1.7', '2') + '><path d="M2.5 12.5c2.5-3.8 5.6-5.6 9.5-5.6s7 1.8 9.5 5.6"/><path d="M5 10.2c2-2.5 4.4-3.6 7-3.6s5 1.1 7 3.6M3.5 13.5c2.6 2.5 5.5 3.6 8.5 3.6s5.9-1.1 8.5-3.6"/><circle cx="12" cy="12.5" r="3.2"/></svg>',
    'envelope': '<svg viewBox="0 0 24 24" aria-hidden="true" ' + SW + '><path d="M3 7h10v8H3zM3 7l5 4.5L13 7M13 9.5h5a2.5 2.5 0 0 1 2.5 2.5v5.5H13zM8 15v5"/></svg>',
    'phone': '<svg viewBox="0 0 24 24" aria-hidden="true" ' + SW + '><path d="M8.5 7.5 7 6c-1.2 1-1.8 2.3-1.4 3.8.8 3.5 4.9 7.7 8.6 8.6 1.5.4 2.8-.2 3.8-1.4l-1.5-1.5-2.3 1.2c-1.7-.7-3.4-2.4-4.1-4.1l1.2-2.3z"/><path d="M12.5 3.5A8.5 8.5 0 0 1 20.5 11M19 13a8.5 8.5 0 0 1-8 8M4 13.5a8.5 8.5 0 0 0 4 6M20 3v3.5h-3.5"/></svg>',
}

I = ICONS


def dot(cls, inner):
    return '<div class="dot %s" aria-hidden="true">%s</div>' % (cls, inner)


def ico(name, cls=''):
    return '<div class="ico %s" aria-hidden="true">%s</div>' % (cls, I[name])


S1 = ('''
    <div class="sheet" aria-hidden="true"></div>
    <div class="logo">%s<p>§LOGO1§<br>§LOGO2§</p></div>
    <h1 class="abs t1">§C1§</h1>
    <h2 class="abs t2">§C2§</h2>
    <p class="abs t3">§C3§</p>
    <div class="pill">§PRESENTER§</div>
    <img class="ph ph-s1" src="%steam-meeting.jpg" alt="§ALT1§">
    %s
  ''' % (I['logo'], A, dot('', I['arrow'])))

S2 = ('''
    <div class="side" aria-hidden="true"></div>
    <h1 class="ttl">§T2§</h1><p class="sub">§SUB§</p>
    %s
    <h2 class="abs h wh">§WH§</h2>
    <p class="abs p wp1">§WP1§</p>
    <p class="abs p wp2">§WP2§</p>
    <div class="card c1"></div><div class="card c2"></div><div class="card c3"></div><div class="card c4"></div>
    %s%s%s%s
    <div class="ch q1"><h3 class="h">§I1H§</h3><p class="p">§CARDP§</p></div>
    <div class="ch q2"><h3 class="h">§I2H§</h3><p class="p">§CARDP§</p></div>
    <div class="ch q3"><h3 class="h">§I3H§</h3><p class="p">§CARDP§</p></div>
    <div class="ch q4"><h3 class="h">§I4H§</h3><p class="p">§CARDP§</p></div>
  ''' % (dot('big1', I['arrow']), dot('n n1', '<span class="num">1</span>'), dot('n n2', '<span class="num">2</span>'),
         dot('n n3', '<span class="num">3</span>'), dot('n n4', '<span class="num">4</span>')))


def stat(cls, icon, v, l, icls=''):
    return '<div class="st %s">%s<p class="big">%s</p><p class="lbl">%s</p></div>' % (cls, ico(icon, icls), v, l)


S3 = ('''
    <h1 class="ttl">§T3§</h1><p class="sub">§SUB§</p>
    <div class="panel" aria-hidden="true"></div>
    %s
    <p class="abs p intro">§A_INTRO§</p>
    <h2 class="abs h bh1">§STORY§</h2><p class="abs p bp1">§STORYP§</p>
    <h2 class="abs h bh2">§MISSION§</h2><p class="abs p bp2">§MISSIONP§</p>
    %s
    %s
    %s
    %s
  ''' % (dot('', I['arrow']), stat('st1', 'building', '12+', '§L1§'), stat('st2', 'thumb', '96%', '§L2§'),
         stat('st3', 'user', '850+', '§L3§'), stat('st4', 'map', '165', '§L4§')))

S4 = ('''
    <h1 class="ttl">§T4§</h1><p class="sub">§SUB§</p>
    <div class="card ca"></div><div class="card cb"></div>
    %s
    <p class="abs quote">§QUOTE§</p>
    <div class="pill">§CLIENT§</div>
    <p class="abs p bp">§B4P§</p>
    %s
    %s
  ''' % (dot('', I['quote']), stat('st1', 'watch', '+32%', '§L5§'), stat('st2', 'bag', '-24%', '§L6§')))

S5 = ('''
    <h1 class="ttl">§T5§</h1><p class="sub">§SUB§</p>
    <p class="abs p intro">§S5P§</p>
    <img class="ph ph-s5" src="%sservices-meeting.jpg" alt="§ALT2§">
    <div class="card c1"></div><div class="card c2"></div><div class="card c3"></div>
    %s%s%s
    <div class="sv v1"><h2 class="h">§SV1§</h2><p class="p">§SV1P§</p></div>
    <div class="sv v2"><h2 class="h">§SV2§</h2><p class="p">§SV2P§</p></div>
    <div class="sv v3"><h2 class="h">§SV3§</h2><p class="p">§SV3P§</p></div>
  ''' % (A, dot('d1', I['bulb']), dot('d2', I['h24']), dot('d3', I['care'])))

S6 = ('''
    <h1 class="ttl">§T6§</h1><p class="sub">§SUB§</p>
    <div class="base" aria-hidden="true"></div>
    <div class="card c1"></div><div class="card c2"></div><div class="card c3"></div><div class="card c4"></div>
    %s%s%s%s
    <span class="go a1" aria-hidden="true">%s</span><span class="go a2" aria-hidden="true">%s</span><span class="go a3" aria-hidden="true">%s</span>
    <div class="gl g1"><h2 class="h">§G1H1§<br>§G1H2§</h2><p class="p">§GP§</p></div>
    <div class="gl g2"><h2 class="h">§G2H1§<br>§G2H2§</h2><p class="p">§GP§</p></div>
    <div class="gl g3"><h2 class="h">§G3H1§<br>§G3H2§</h2><p class="p">§GP§</p></div>
    <div class="gl g4"><h2 class="h">§G4H1§<br>§G4H2§</h2><p class="p">§GP§</p></div>
    %s
    <p class="abs p foot">§G_FOOT§</p>
  ''' % (dot('d1', '<span class="num">1</span>'), dot('d2', '<span class="num">2</span>'), dot('d3', '<span class="num">3</span>'),
         dot('d4', '<span class="num">4</span>'), I['arrowR'], I['arrowR'], I['arrowR'], dot('tip', I['bulb'])))


def weeks():
    return ''.join('<div class="wk wk%d"><div class="pill">§WEEK§ %d</div></div>' % (i, i) for i in range(1, 5))


def lst(keys):
    return '<ul>' + ''.join('<li>§%s§</li>' % k for k in keys) + '</ul>'


S7 = ('''
    <h1 class="ttl">§T7§</h1><p class="sub">§SUB§</p>
    <p class="abs p intro">§A_INTRO§</p>
    <div class="card panel"></div>
    %s
    <div class="ph-col pc1"><h2 class="h">§PH§ 01<br>§P1N§</h2>%s</div>
    <div class="ph-col pc2"><h2 class="h">§PH§ 02<br>§P2N§</h2>%s</div>
    <div class="ph-col pc3"><h2 class="h">§PH§ 03<br>§P3N§</h2>%s</div>
    <div class="ph-col pc4"><h2 class="h">§PH§ 04<br>§P4N§</h2>%s</div>
  ''' % (weeks(), lst(['B11', 'B12', 'B13', 'B14']), lst(['B21', 'B22', 'B23', 'B24']), lst(['B31', 'B32', 'B33', 'B34']), lst(['B41', 'B42', 'B43', 'B44'])))

ROWS = [('R1', '§M1§'), ('R2', '§M2§'), ('R3', '§M3§'), ('R4', '§M4§'), ('R5', '§YOURTEAM§')]
S8 = ('''
    <h1 class="ttl">§T8§</h1><p class="sub">§SUB§</p>
    <div class="card ca"></div><div class="card cb"></div>
    <table><thead><tr><th>§TH1§</th><th>§TH2§</th><th>§TH3§</th></tr></thead><tbody>%s</tbody></table>
    <p class="abs p bp">§R_SIDE§</p>
    %s
  ''' % (''.join('<tr><td>§%s§</td><td>§SAMPLE§</td><td>%s</td></tr>' % r for r in ROWS), stat('', 'watch', '+19%', '§L5§')))

S9 = ('''
    <h1 class="ttl">§T9§</h1><p class="sub">§SUB§</p>
    <div class="base" aria-hidden="true"></div>
    <div class="card panel"></div>
    %s
    <div class="ph-col pc1"><h2 class="h">§PH§ 01<br>§C1N§</h2><p class="p">§GP§</p></div>
    <div class="ph-col pc2"><h2 class="h">§PH§ 02<br>§C2N§</h2><p class="p">§GP§</p></div>
    <div class="ph-col pc3"><h2 class="h">§PH§ 03<br>§C3N§</h2><p class="p">§GP§</p></div>
    <div class="ph-col pc4"><h2 class="h">§PH§ 04<br>§C4N§</h2><p class="p">§GP§</p></div>
    <div class="kp k1">%s<p class="v">98%%</p><p class="k">§K1A§<br>§K1B§</p><p class="p">§KP§</p></div>
    <div class="kp k2">%s<p class="v">§K2V§</p><p class="k">§K2A§<br>§K2B§</p><p class="p">§KP§</p></div>
  ''' % (weeks(), ico('chat', 't'), ico('truck', 't')))


def doc(cls, icon, h1, h2):
    return '<div class="dc %s">%s<h2 class="h">§%s§<br>§%s§</h2><p class="p">§GP§</p></div>' % (cls, ico(icon), h1, h2)


S10 = ('''
    <div class="base" aria-hidden="true"></div>
    <h1 class="ttl">§T10A§<br>§T10B§</h1><p class="sub">§SUB§</p>
    <img class="ph ph-s10" src="%sdocuments-review.jpg" alt="§ALT3§">
    <div class="card panel"></div>
    %s
    %s
    %s
    %s
  ''' % (A, doc('dc1', 'factory', 'D1A', 'D1B'), doc('dc2', 'book', 'D2A', 'D2B'), doc('dc3', 'eye', 'D3A', 'D3B'), doc('dc4', 'docs', 'D4A', 'D4B')))

S11 = ('''
    <h1 class="ttl">§T11§</h1><p class="sub">§SUB§</p>
    %s
    <p class="abs p intro">§F_INTRO§</p>
    <div class="card panel"></div>
    %s%s%s
    <div class="qa a1"><h2 class="h">§Q1§</h2><p class="p">§FA§</p></div>
    <div class="qa a2"><h2 class="h">§Q2§</h2><p class="p">§FA§</p></div>
    <div class="qa a3"><h2 class="h">§Q3§</h2><p class="p">§FA§</p></div>
    <span class="hr r1" aria-hidden="true"></span><span class="hr r2" aria-hidden="true"></span>
  ''' % (ico('sign', 't'), dot('f1', I['arrow']), dot('f2', I['arrow']), dot('f3', I['arrow'])))

S12 = ('''
    <h1 class="ttl">§T12§</h1><p class="sub">§SUB§</p>
    <img class="ph ph-s12" src="%skickoff.jpg" alt="§ALT4§">
    %s
    <div class="bar on b1">%s<h2 class="h">§N1§</h2></div>
    <div class="bar b2">%s<h2 class="h">§N2§</h2></div>
    <div class="bar b3">%s<h2 class="h">§N3§</h2></div>
    <div class="bar b4">%s<h2 class="h">§N4§</h2></div>
    <p class="np p n1">§NP§</p><p class="np p n2">§NP§</p><p class="np p n3">§NP§</p><p class="np p n4">§NP§</p>
  ''' % (A, dot('', I['arrow']), I['mail'], I['tie'], I['down'], I['eye4']))

S13 = ('''
    <div class="logo">%s<p>§LOGO1§<br>§LOGO2§</p></div>
    <p class="abs thx">§THX§</p>
    <h1 class="abs msg">§MSG§</h1>
    <div class="ct ct1">%s<span>§EMAIL§</span></div>
    <div class="ct ct2">%s<span>§PHONE§</span></div>
  ''' % (I['logo'], ico('envelope'), ico('phone')))

SLIDES = [S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13]
# الخلفية لكل شريحة: الصنف blue أو gray على الإطار نفسه (يُضاف بعد البناء عبر EXTRA_SCRIPT لا يجوز؛ فنضعه بمحدّد الصنف sN)
BLUE = [1, 4, 8, 13]

STYLE += ''.join('  .frame.s%d { background-color: #0958E8; background-image: linear-gradient(90deg, rgb(255 255 255 / .055) 2px, transparent 2px), linear-gradient(180deg, rgb(255 255 255 / .055) 2px, transparent 2px); background-size: 55.652px 55.652px; background-position: 55px 53px; }\n' % i for i in BLUE)
STYLE += ''.join('  .frame.s%d { background-color: #F2F2F2; background-image: linear-gradient(90deg, rgb(20 30 80 / .022) 2px, transparent 2px), linear-gradient(180deg, rgb(20 30 80 / .022) 2px, transparent 2px); background-size: 55.652px 55.652px; background-position: 55px 53px; }\n' % i for i in range(2, 13) if i not in BLUE)
STYLE += ''.join('  .s%d .ttl { color: #FFFFFF; } .s%d .sub { color: var(--lime); }\n' % (i, i) for i in (4, 8))

# ---------- النصوص ----------
SAMPLE_EN = {
    'This is a sample text.': 'هذا نص توضيحي.',
    'Add your desired text here.': 'أضف النص المطلوب هنا.',
    'Add text here.': 'أضف نصًا هنا.',
    'Add your text here.': 'أضف نصك هنا.',
}


def tr(s):
    out = s
    for k, v in SAMPLE_EN.items():
        out = out.replace(k, v)
    return out


EN = {
    'SUB': 'SUBHEADLINE GOES HERE', 'SAMPLE': 'This is a sample text.',
    'LOGO1': 'YOUR COMPANY', 'LOGO2': 'LOGO HERE',
    'C1': 'CLIENT', 'C2': 'ONBOARDING', 'C3': 'PRESENTATION TEMPLATE', 'PRESENTER': 'PRESENTER NAME',
    'T2': 'INTRODUCTION', 'WH': 'WELCOME TO THE TEAM',
    'WP1': 'This is a sample text. Add your desired text here. Add text here. Add your text here. This is a sample text. Add your desired text here.',
    'WP2': 'Add your text here. This is a sample text. Add your desired text here. This is a sample text.',
    'I1H': 'GET CONNECTED', 'I2H': 'ALIGN EXPECTATIONS', 'I3H': 'UNDERSTAND PROCESS', 'I4H': 'START CONFIDENTLY',
    'CARDP': 'This is a sample text. Add your desired text here. Add text here. Add text here.',
    'T3': 'ABOUT US',
    'A_INTRO': 'This is a sample text. Add your desired text here. Add text here. Add text here. This is a sample text. Add your desired text here. This is a sample text. This is a sample text. Add your desired text here.',
    'STORY': 'OUR STORY', 'STORYP': 'This is a sample text. Add your desired text here. Add text here. Add your text here. This is a sample text. Add your desired text here. This is a sample text.',
    'MISSION': 'OUR MISSION', 'MISSIONP': 'This is a sample text. Add your desired text here. Add text here. Add text here. This is a sample text. Add your desired text here. This is a sample text.',
    'L1': 'Industry Experience', 'L2': 'Client Satisfaction', 'L3': 'Clients Served', 'L4': 'Markets Supported',
    'T4': 'KEY ACHIEVEMENTS &amp; TESTIMONIALS',
    'QUOTE': 'Add a short testimonial from a customer that highlights the quality of your service, onboarding experience, product value, or ongoing partnership.',
    'CLIENT': 'CLIENT NAME',
    'B4P': 'This is a sample text. Add your desired text here. Add text here. Add text here. This is a sample text. Add your desired text here. This is a sample text. Add your text here. Add your desired text here.',
    'L5': 'Process Efficiency', 'L6': 'Operational Costs',
    'T5': 'OUR SERVICES',
    'S5P': 'This is a sample text. Add your desired text here. Add text here. Add your text here. This is a sample text. Add your desired text here. This is a sample text. Add your text here. Add your desired text here.',
    'SV1': 'CORE SOLUTION', 'SV1P': 'This is a sample text. Add your desired text here. Add text here. Add your text here. This is a sample text. Add your desired text here.',
    'SV2': 'SUPPORT &amp; SUCCESS', 'SV2P': 'This is a sample text. Add your desired text here. Add your text here. This is a sample text. Add your desired text here.',
    'SV3': 'ADDITIONAL SOLUTIONS', 'SV3P': 'This is a sample text. Add your desired text here. Add text here. Add text here. This is a sample text. Add text here.',
    'T6': 'ONBOARDING GOALS',
    'G1H1': 'ALIGN', 'G1H2': 'OBJECTIVES', 'G2H1': 'UNDERSTAND', 'G2H2': 'REQUIREMENTS', 'G3H1': 'BUILD', 'G3H2': 'CONNECTIONS', 'G4H1': 'PREPARE', 'G4H2': 'FOR DELIVERY',
    'GP': 'This is a sample text. Add text here. Add text here. Add your text here. Add text here.',
    'G_FOOT': 'This is a sample text. Add your desired text here. Add text here. Add text here. This is a sample text. Add your desired text here. Add your desired text here. This is a sample text. Add your desired text here. This is a sample text. Add your text here.',
    'T7': 'TIMELINE', 'WEEK': 'WEEK', 'PH': 'PHASE',
    'P1N': 'KICKOFF', 'P2N': 'SETUP', 'P3N': 'ENABLEMENT', 'P4N': 'LAUNCH',
    'B11': 'Introductions &amp; alignment', 'B12': 'Confirm objectives', 'B13': 'Review scope', 'B14': 'Establish channels',
    'B21': 'Collect required information', 'B22': 'Configure account', 'B23': 'Set up users', 'B24': 'Complete integrations',
    'B31': 'Conduct training', 'B32': 'Validate workflows', 'B33': 'Complete testing', 'B34': 'Address open issues',
    'B41': 'Final readiness review', 'B42': 'Launch solution', 'B43': 'Confirm user access', 'B44': 'Begin live operations',
    'T8': 'ROLES &amp; RESPONSIBILITIES', 'TH1': 'Role', 'TH2': 'Main Responsibility', 'TH3': 'Contact',
    'R1': 'Client Success Manager', 'R2': 'Project Lead', 'R3': 'Account Manager', 'R4': 'Technical Support', 'R5': 'Client Contact', 'YOURTEAM': 'Your Team',
    'M1': 'sucess@company.com', 'M2': 'project@company.com', 'M3': 'account@company.com', 'M4': 'tech@company.com',
    'R_SIDE': 'This is a sample text. Add your desired text here. Add text here. Add text here. This is a sample text. Add your desired text here. This is a sample text. Add text here.',
    'T9': 'COMMUNICATION PLAN', 'C1N': 'KICKOFF', 'C2N': 'WEEKLY', 'C3N': 'ON DEMAND', 'C4N': 'FINAL REVIEW',
    'K1A': 'Communication', 'K1B': 'Follow-Up', 'K2V': '1HR', 'K2A': 'Average', 'K2B': 'Response Time',
    'KP': 'This is a sample text. Add your desired text here. Add text here.',
    'T10A': 'DOCUMENTATION', 'T10B': 'REQUIRED',
    'D1A': 'COMPANY', 'D1B': 'INFORMATION', 'D2A': 'SERVICE', 'D2B': 'REQUIREMENTS', 'D3A': 'ACCESS &amp;', 'D3B': 'CREDENTIALS', 'D4A': 'SUPPORTING', 'D4B': 'DOCUMENTS',
    'T11': 'FAQs', 'F_INTRO': 'This is a sample text. Add your desired text here. Add text here. Add text here. This is a sample text.',
    'Q1': 'HOW LONG DOES ONBOARDING TAKE?', 'Q2': 'WHO SHOULD BE INVOLVED FROM OUR TEAM?', 'Q3': 'WHAT HAPPENS AFTER ONBOARDING IS COMPLETE?',
    'FA': 'This is a sample text. Add your desired text here. Add text here. Add your text here. This is a sample text. Add your desired text here. This is a sample text. This is a sample text. Add your desired text here. Add text here.',
    'T12': 'NEXT STEPS', 'N1': 'CONFIRM YOUR MAIN CONTACT', 'N2': 'ATTEND THE KICKOFF MEETING', 'N3': 'SUBMIT REQUIRED INFORMATION', 'N4': 'REVIEW THE ONBOARDING PLAN',
    'NP': 'This is a sample text. Add your desired text here. Add text here. Add your text here. Add your desired text here. This is a sample text. This is a sample text.',
    'THX': 'THANK YOU FOR JOINING US!',
    'MSG': 'We are excited to begin this partnership and look forward to creating meaningful results together.',
    'EMAIL': 'Add email here', 'PHONE': 'Add phone here',
    'ALT1': 'Team meeting in a modern office', 'ALT2': 'Presenter explaining services to a client team', 'ALT3': 'Team reviewing documents at a round table', 'ALT4': 'Presenter leading a kickoff meeting',
}

AR = {
    'SUB': 'العنوان الفرعي هنا', 'SAMPLE': 'هذا نص توضيحي.',
    'LOGO1': 'شعار شركتك', 'LOGO2': 'هنا',
    'C1': 'تهيئة', 'C2': 'العملاء', 'C3': 'نموذج عرض تقديمي', 'PRESENTER': 'اسم المقدّم',
    'T2': 'مقدمة', 'WH': 'مرحبًا بك في الفريق',
    'I1H': 'التواصل والتعارف', 'I2H': 'مواءمة التوقعات', 'I3H': 'فهم الإجراءات', 'I4H': 'البدء بثقة',
    'T3': 'من نحن', 'STORY': 'قصتنا', 'MISSION': 'رسالتنا',
    'L1': 'سنوات الخبرة في القطاع', 'L2': 'رضا العملاء', 'L3': 'عميلًا خدمناهم', 'L4': 'سوقًا ندعمها',
    'T4': 'أبرز الإنجازات وآراء العملاء',
    'QUOTE': 'أضف شهادة قصيرة من أحد العملاء تُبرز جودة خدمتك، أو تجربة التهيئة، أو قيمة المنتج، أو الشراكة المستمرة.',
    'CLIENT': 'اسم العميل',
    'L5': 'كفاءة العمليات', 'L6': 'التكاليف التشغيلية',
    'T5': 'خدماتنا', 'SV1': 'الحل الأساسي', 'SV2': 'الدعم والنجاح', 'SV3': 'حلول إضافية',
    'T6': 'أهداف التهيئة',
    'G1H1': 'مواءمة', 'G1H2': 'الأهداف', 'G2H1': 'فهم', 'G2H2': 'المتطلبات', 'G3H1': 'بناء', 'G3H2': 'العلاقات', 'G4H1': 'الاستعداد', 'G4H2': 'للتسليم',
    'T7': 'الجدول الزمني', 'WEEK': 'الأسبوع', 'PH': 'المرحلة',
    'P1N': 'الانطلاق', 'P2N': 'الإعداد', 'P3N': 'التمكين', 'P4N': 'الإطلاق',
    'B11': 'التعارف والمواءمة', 'B12': 'تأكيد الأهداف', 'B13': 'مراجعة النطاق', 'B14': 'تحديد قنوات التواصل',
    'B21': 'جمع المعلومات المطلوبة', 'B22': 'تهيئة الحساب', 'B23': 'إعداد المستخدمين', 'B24': 'إتمام عمليات التكامل',
    'B31': 'تقديم التدريب', 'B32': 'التحقق من سير العمل', 'B33': 'إتمام الاختبارات', 'B34': 'معالجة المسائل المفتوحة',
    'B41': 'المراجعة النهائية للجاهزية', 'B42': 'إطلاق الحل', 'B43': 'تأكيد وصول المستخدمين', 'B44': 'بدء التشغيل الفعلي',
    'T8': 'الأدوار والمسؤوليات', 'TH1': 'الدور', 'TH2': 'المسؤولية الرئيسية', 'TH3': 'جهة التواصل',
    'R1': 'مدير نجاح العملاء', 'R2': 'قائد المشروع', 'R3': 'مدير الحساب', 'R4': 'الدعم الفني', 'R5': 'جهة اتصال العميل', 'YOURTEAM': 'فريقك',
    'M1': 'بريد مدير النجاح', 'M2': 'بريد قائد المشروع', 'M3': 'بريد مدير الحساب', 'M4': 'بريد الدعم الفني',
    'T9': 'خطة التواصل', 'C1N': 'الانطلاق', 'C2N': 'أسبوعيًا', 'C3N': 'عند الطلب', 'C4N': 'المراجعة النهائية',
    'K1A': 'متابعة', 'K1B': 'التواصل', 'K2V': '1 ساعة', 'K2A': 'متوسط', 'K2B': 'زمن الاستجابة',
    'T10A': 'المستندات', 'T10B': 'المطلوبة',
    'D1A': 'معلومات', 'D1B': 'الشركة', 'D2A': 'متطلبات', 'D2B': 'الخدمة', 'D3A': 'الوصول', 'D3B': 'وبيانات الدخول', 'D4A': 'المستندات', 'D4B': 'الداعمة',
    'T11': 'الأسئلة الشائعة',
    'Q1': 'كم تستغرق عملية التهيئة؟', 'Q2': 'من ينبغي أن يشارك من فريقنا؟', 'Q3': 'ماذا يحدث بعد اكتمال التهيئة؟',
    'T12': 'الخطوات التالية', 'N1': 'أكّد جهة الاتصال الرئيسية', 'N2': 'احضر اجتماع الانطلاق', 'N3': 'قدّم المعلومات المطلوبة', 'N4': 'راجع خطة التهيئة',
    'THX': 'شكرًا لانضمامك إلينا!',
    'MSG': 'يسعدنا أن نبدأ هذه الشراكة، ونتطلع إلى تحقيق نتائج مثمرة معًا.',
    'EMAIL': 'أضف البريد الإلكتروني هنا', 'PHONE': 'أضف رقم الهاتف هنا',
    'ALT1': 'اجتماع فريق في مكتب حديث', 'ALT2': 'مقدّم يشرح الخدمات لفريق العميل', 'ALT3': 'فريق يراجع المستندات حول طاولة مستديرة', 'ALT4': 'مقدّم يقود اجتماع الانطلاق',
}
# نصوص العينة تُترجم آليًا من جملها الإنجليزية الثابتة
for k, v in EN.items():
    if k not in AR:
        AR[k] = tr(v)

STR = {'ar': AR, 'en': EN}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'cx', 'icon': '🤝',
    'name': 'تهيئة العملاء', 'nameEn': 'Client Onboarding',
    'description': 'ثلاث عشرة شريحة لتهيئة عميل جديد: من نحن، الخدمات، الأهداف، الجدول الزمني، الأدوار، التواصل، المستندات والأسئلة',
    'descriptionEn': '13 slides to onboard a new client: about us, services, goals, timeline, roles, communication, documents, FAQs',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'مقدمة', 'titleEn': 'Introduction'},
        {'title': 'من نحن', 'titleEn': 'About Us', 'needsData': True},
        {'title': 'أبرز الإنجازات وآراء العملاء', 'titleEn': 'Key Achievements & Testimonials', 'needsData': True},
        {'title': 'خدماتنا', 'titleEn': 'Our Services'},
        {'title': 'أهداف التهيئة', 'titleEn': 'Onboarding Goals'},
        {'title': 'الجدول الزمني', 'titleEn': 'Timeline'},
        {'title': 'الأدوار والمسؤوليات', 'titleEn': 'Roles & Responsibilities', 'needsData': True},
        {'title': 'خطة التواصل', 'titleEn': 'Communication Plan', 'needsData': True},
        {'title': 'المستندات المطلوبة', 'titleEn': 'Documentation Required'},
        {'title': 'الأسئلة الشائعة', 'titleEn': 'FAQs'},
        {'title': 'الخطوات التالية', 'titleEn': 'Next Steps'},
        {'title': 'شكرًا لانضمامك', 'titleEn': 'Thank You'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والمواضع وغيّر النصوص والأرقام فقط (الإحصاءات الزرقاء وصفوف جدول الأدوار)؛ الصور الأربع مقصوصة من الأصل بقصّ clip-path ثابت.',
}
