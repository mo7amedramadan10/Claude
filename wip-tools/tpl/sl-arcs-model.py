# -*- coding: utf-8 -*-
# نموذج «آركس» (الانتباه، الصلة، الثقة، الرضا): أربع شرائح بأربعة ألوان ثابتة (كحلي، أزرق، برتقالي، أخضر):
# 1) أربعة أعمدة بحرف كبير وشريط عنوان وأيقونة وثلاث نقاط؛ 2) أربعة صفوف بدائرة أيقونة وعنوان وحرف ونقطتين؛
# 3) أربعة مربعات بحروف على خط متقطع مع نصوص وأيقونات أعلى وأسفل؛ 4) أربع بتلات بيضاوية متقاطعة حول «دافعية الطالب»
# وبطاقات رمادية موصولة بخطوط.
KEY = 'sl-arcs-model'
TITLE = {'ar': 'نموذج آركس', 'en': 'ARCS Model'}
COMMENT = 'نموذج «آركس»: أربعة ألوان ثابتة لكل عنصر (كحلي A، أزرق R، برتقالي C، أخضر S) في أربعة تخطيطات: أعمدة، صفوف، مربعات على خط متقطع، وبتلات بيضاوية متقاطعة SVG موصولة ببطاقات. الأيقونات SVG مضمّنة؛ رسم البتلات والموصّلات يُعكس أفقيًا في العربية.'
VARS = '--ink: #111111; --ink-2: #404040; --accent: #3598DB; --navy: #44546B; --navy-d: #364354; --navy-c: #364153; --blue: #1F74AD; --blue-d: #195D8A; --blue-c: #16517B; --blue-l: #3598DB; --orange: #ED7D31; --orange-d: #BE6426; --orange-c: #C55B11; --green: #70AD46; --green-d: #5A8939; --green-c: #538234; --card: #F2F2F2; --shadow: #D9D9D9; --bg: #FFFFFF;'

STYLE = """  .ttl { position: absolute; top: 42px; inset-inline-start: 64px; margin: 0; font-size: 40px; font-weight: 400; line-height: 1.2; color: var(--ink); letter-spacing: -0.2px; }
  .ic svg { width: 100%; height: 100%; display: block; }
  /* نقطة بدائرة تحقق */
  .bul { position: absolute; margin: 0; padding-inline-start: 32px; font-size: 17px; line-height: 1.45; color: #fff; }
  .bul .ck { position: absolute; top: 3px; inset-inline-start: 0; width: 20px; height: 20px; border-radius: 50%; background: rgb(255 255 255 / .62); color: var(--c); display: grid; place-items: center; }
  .bul .ck svg { width: 12px; height: 12px; }
  /* الشريحة 1: أربعة أعمدة */
  .s1 .col { position: absolute; top: 55px; width: 285px; height: 610px; background: var(--c); color: #fff; }
  .s1 .c1 { inset-inline-start: 63px; } .s1 .c2 { inset-inline-start: 352px; } .s1 .c3 { inset-inline-start: 642px; } .s1 .c4 { inset-inline-start: 932px; }
  .s1 .big { position: absolute; top: 20px; inset-inline-start: 0; width: 100%; margin: 0; text-align: center; font-size: 120px; font-weight: 700; line-height: 1.1; }
  .s1 .band { position: absolute; top: 169px; inset-inline-start: 0; width: 100%; height: 86px; margin: 0; background: var(--d); display: grid; place-items: center; font-size: 20px; font-weight: 700; letter-spacing: .3px; }
  .s1 .ic { position: absolute; top: 280px; inset-inline-start: 0; width: 100%; height: 64px; display: grid; place-items: center; }
  .s1 .ic svg { width: 64px; height: 64px; }
  .s1 .bul { inset-inline-start: 53px; width: 172px; }
  .s1 .c4 .b1 { top: 371px; } .s1 .c4 .b2 { top: 441px; } .s1 .c4 .b3 { top: 511px; }  /* عمود S في الأصل أعلى قليلًا */
  .s1 .b1 { top: 385px; } .s1 .b2 { top: 455px; } .s1 .b3 { top: 525px; }
  /* الشريحة 2: أربعة صفوف */
  .s2 .row { position: absolute; inset-inline-start: 57px; width: 1167px; height: 122px; color: #fff; }
  .s2 .r1 { top: 98px; } .s2 .r2 { top: 231px; } .s2 .r3 { top: 365px; } .s2 .r4 { top: 498px; }
  .s2 .lbl { position: absolute; top: 0; inset-inline-start: 61px; width: 340px; height: 122px; margin: 0; background: var(--c); padding-inline-start: 84px; display: flex; align-items: center; font-size: 22px; font-weight: 700; letter-spacing: .3px; }
  .s2 .ic { position: absolute; top: 0; inset-inline-start: 0; width: 123px; height: 122px; border-radius: 50%; background: var(--k); display: grid; place-items: center; }
  .s2 .ic svg { width: 60px; height: 60px; }
  .s2 .big { position: absolute; top: 0; inset-inline-start: 404px; width: 156px; height: 122px; margin: 0; background: var(--d); display: grid; place-items: center; font-size: 72px; font-weight: 700; line-height: 1; }
  .s2 .txt { position: absolute; top: 0; inset-inline-start: 560px; width: 607px; height: 122px; background: var(--c); }
  .s2 .bul { top: 44px; width: 172px; }
  .s2 .bul.p1 { inset-inline-start: 50px; } .s2 .bul.p2 { inset-inline-start: 283px; }
  /* الشريحة 3: مربعات على خط متقطع */
  .s3 .dash { position: absolute; top: 389px; inset-inline-start: 246px; width: 786px; height: 0; border-top: 3px dashed #404040; }
  .s3 .sq { position: absolute; top: 304px; width: 176px; height: 172px; background: var(--c); color: #fff; display: grid; place-items: center; margin: 0; font-size: 110px; font-weight: 700; line-height: 1; }
  .s3 .sq::before { content: ""; position: absolute; top: 29px; inset-inline-start: 31px; width: 156px; height: 152px; background: var(--shadow); z-index: -1; }
  .s3 .q1 { inset-inline-start: 70px; } .s3 .q2 { inset-inline-start: 391px; } .s3 .q3 { inset-inline-start: 712px; } .s3 .q4 { inset-inline-start: 1032px; }
  .s3 .cap { position: absolute; width: 200px; margin: 0; text-align: center; font-size: 20px; font-weight: 700; color: var(--ink); letter-spacing: .3px; }
  .s3 .smp { position: absolute; width: 200px; margin: 0; text-align: center; font-size: 17px; line-height: 1.35; color: var(--ink-2); }
  .s3 .ic { position: absolute; width: 66px; height: 66px; color: var(--c); }
  .s3 .t1 { top: 150px; inset-inline-start: 60px; } .s3 .h1 { top: 240px; inset-inline-start: 60px; }
  .s3 .t3 { top: 150px; inset-inline-start: 700px; } .s3 .h3 { top: 240px; inset-inline-start: 700px; }
  .s3 .h2 { top: 520px; inset-inline-start: 380px; } .s3 .t2 { top: 560px; inset-inline-start: 380px; }
  .s3 .h4 { top: 520px; inset-inline-start: 1020px; } .s3 .t4 { top: 560px; inset-inline-start: 1020px; }
  .s3 .i2 { top: 195px; inset-inline-start: 447px; } .s3 .i4 { top: 193px; inset-inline-start: 1087px; }
  .s3 .i1 { top: 522px; inset-inline-start: 127px; } .s3 .i3 { top: 522px; inset-inline-start: 767px; }
  /* الشريحة 4: بتلات وبطاقات */
  .s4 .diag { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; }
  html[dir="rtl"] .s4 .diag { transform: scaleX(-1); }
  .s4 .diag .pet { stroke: #fff; stroke-width: 2; }
  .s4 .diag .ln { fill: none; stroke-width: 2; stroke-linecap: round; }
  .s4 .ltr { position: absolute; width: 80px; height: 80px; margin: 0; display: grid; place-items: center; color: #fff; font-size: 74px; font-weight: 700; line-height: 1; }
  .s4 .lA { top: 175px; inset-inline-start: 265px; } .s4 .lS { top: 335px; inset-inline-start: 110px; }
  .s4 .lR { top: 335px; inset-inline-start: 420px; } .s4 .lC { top: 485px; inset-inline-start: 265px; }
  .s4 .ctr { position: absolute; top: 337px; inset-inline-start: 247px; width: 116px; height: 76px; margin: 0; display: grid; place-items: center; text-align: center; font-size: 15px; font-weight: 700; line-height: 1.2; color: var(--ink); }
  .s4 .card { position: absolute; inset-inline-start: 767px; width: 449px; height: 125px; background: var(--card); border-radius: 16px; }
  .s4 .k1 { top: 137px; } .s4 .k2 { top: 271px; } .s4 .k3 { top: 405px; } .s4 .k4 { top: 539px; }
  .s4 .card h2 { position: absolute; top: 20px; inset-inline-start: 31px; margin: 0; font-size: 20px; font-weight: 700; color: var(--c); letter-spacing: .3px; }
  .s4 .card p { position: absolute; top: 54px; inset-inline-start: 31px; width: 240px; margin: 0; font-size: 17px; line-height: 1.35; color: var(--ink-2); }
  .s4 .card .ic { position: absolute; top: 30px; inset-inline-start: 348px; width: 66px; height: 66px; color: var(--i); }"""

ICONS = {
    'mega': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M3 11.5v3.2l2 .6v3.4a1 1 0 0 0 1 1h2a1 1 0 0 0 1-1v-2.6l7 2.1V8.3L5 10.6"/><path d="M5 10.6v4.7M4.6 12.1l-1.9-.2"/><path d="M16 9.1c1.6.6 1.6 4.1 0 4.7"/><path d="M19.5 4.6l-2 2.2M21.6 9.3l-2.9.4M18.5 2.6l-.7 2.8"/></svg>',
    'atom': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.1" stroke-linecap="round" aria-hidden="true"><ellipse cx="12" cy="12" rx="10" ry="4"/><ellipse cx="12" cy="12" rx="10" ry="4" transform="rotate(60 12 12)"/><ellipse cx="12" cy="12" rx="10" ry="4" transform="rotate(-60 12 12)"/><circle cx="12" cy="12" r="1.6" fill="currentColor" stroke="none"/><circle cx="12" cy="12" r="2.6"/></svg>',
    'jump': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="11" cy="7.2" r="2"/><path d="M11 9.4v5.2l-3.4 4.6M11 14.6l3.6 3.2 1.2 3.4"/><path d="M11 9.6l4-1.6 1.8-3.4M11 9.8l-3.6 2.6"/><path d="M18.6 1.2l.6 1.3 1.4.2-1 1 .2 1.4-1.2-.7-1.3.7.3-1.4-1-1 1.4-.2z"/></svg>',
    'star-bubble': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M3 4.5h18v12H10l-4 4v-4H3z"/><path d="M12 6.8l1.4 2.9 3.2.4-2.3 2.2.6 3.2-2.9-1.5-2.9 1.5.6-3.2-2.3-2.2 3.2-.4z"/></svg>',
    'check': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12.5l4.5 4.5L19 7.5"/></svg>',
}

# (الرمز، اللون الرئيسي، الأغمق للشريط/الحرف، الأغمق للدائرة، الأيقونة)
COLS = [
    ('A', 'navy', 'mega'), ('R', 'blue', 'atom'), ('C', 'orange', 'jump'), ('S', 'green', 'star-bubble'),
]


def bul(cls, n):
    return '<p class="bul %s"><span class="ck">%s</span>§BUL§</p>' % (cls, ICONS['check'])


def slide1():
    h = []
    for i, (L, c, ic) in enumerate(COLS, 1):
        h.append('<div class="col c%d" style="--c:var(--%s);--d:var(--%s-d)"><p class="big">%s</p><h2 class="band">§N%d§</h2><div class="ic">%s</div>%s%s%s</div>'
                 % (i, c, c, L, i, ICONS[ic], bul('b1', i), bul('b2', i), bul('b3', i)))
    return '\n    ' + '\n    '.join(h) + '\n  '


def slide2():
    h = []
    for i, (L, c, ic) in enumerate(COLS, 1):
        h.append('<div class="row r%d" style="--c:var(--%s);--d:var(--%s-d);--k:var(--%s-c)"><h2 class="lbl">§N%d§</h2><div class="ic">%s</div><p class="big">%s</p><div class="txt">%s%s</div></div>'
                 % (i, c, c, c, i, ICONS[ic], L, bul('p1', i), bul('p2', i)))
    return '\n    ' + '\n    '.join(h) + '\n  '


def slide3():
    h = ['<h1 class="ttl">§TTL§</h1>', '<div class="dash"></div>']
    for i, (L, c, ic) in enumerate(COLS, 1):
        h.append('<p class="sq q%d" style="--c:var(--%s)">%s</p>' % (i, 'blue-l' if c == 'blue' else c, L))
        h.append('<h2 class="cap h%d">§N%d§</h2><p class="smp t%d">§SMP§</p><div class="ic i%d" style="--c:var(--%s)">%s</div>'
                 % (i, i, i, i, 'blue-l' if c == 'blue' else c, ICONS[ic]))
    return '\n    ' + '\n    '.join(h) + '\n  '


# البتلات: A أعلى، C أسفل، S جهة البداية، R جهة النهاية؛ التقاطعات بقصّ كل بيضاوي بالآخر
PETALS = ('<svg class="diag" viewBox="0 0 1280 720" aria-hidden="true">'
          '<ellipse class="pet" cx="186" cy="375" rx="120" ry="85" fill="var(--green)"/>'
          '<ellipse class="pet" cx="424" cy="375" rx="120" ry="85" fill="var(--blue-l)"/>'
          '<ellipse class="pet" cx="305" cy="258" rx="85" ry="122" fill="var(--navy)"/>'
          '<ellipse class="pet" cx="305" cy="492" rx="85" ry="122" fill="var(--orange)"/>'
          '<ellipse cx="305" cy="258" rx="85" ry="122" fill="#538234" clip-path="url(#arcsS)"/>'
          '<ellipse cx="305" cy="258" rx="85" ry="122" fill="#90B9D7" clip-path="url(#arcsR)"/>'
          '<ellipse cx="305" cy="492" rx="85" ry="122" fill="#A8D18D" clip-path="url(#arcsS)"/>'
          '<ellipse cx="305" cy="492" rx="85" ry="122" fill="#C55B11" clip-path="url(#arcsR)"/>'
          '<circle cx="305" cy="375" r="58" fill="#fff" stroke="#fff" stroke-width="2"/>'
          '<path class="ln" stroke="#4D5361" d="M400,200 H735"/>'
          '<path class="ln" stroke="var(--blue-l)" d="M545,335 H735"/>'
          '<path class="ln" stroke="var(--orange)" d="M395,565 H510 Q520,565 520,555 V478 Q520,468 530,468 H735"/>'
          '<path class="ln" stroke="var(--green)" d="M136,465 V645 Q136,655 146,655 H395 Q405,655 405,645 V610 Q405,600 415,600 H735"/>'
          '</svg>')
GLOBAL_DEFS = ('<defs><clipPath id="arcsS"><ellipse cx="186" cy="375" rx="120" ry="85"/></clipPath>'
               '<clipPath id="arcsR"><ellipse cx="424" cy="375" rx="120" ry="85"/></clipPath></defs>')
CARD_ICON = ['mega', 'atom', 'star-bubble', 'jump']           # كما في الأصل: أيقونة الثقة فقاعة بنجمة والرضا شخص بنجمة
CARD_ICON_COLOR = ['navy', 'blue-l', 'green', 'orange']


def slide4():
    h = ['<h1 class="ttl">§TTL§</h1>', PETALS,
         '<p class="ltr lA">A</p>', '<p class="ltr lS">S</p>', '<p class="ltr lR">R</p>', '<p class="ltr lC">C</p>',
         '<p class="ctr">§CTR§</p>']
    for i, (L, c, ic) in enumerate(COLS, 1):
        h.append('<div class="card k%d" style="--c:var(--%s);--i:var(--%s)"><h2>§N%d§</h2><p>§SMP2§</p><div class="ic">%s</div></div>'
                 % (i, 'blue-l' if c == 'blue' else c, CARD_ICON_COLOR[i - 1], i, ICONS[CARD_ICON[i - 1]]))
    return '\n    ' + '\n    '.join(h) + '\n  '


SLIDES = [slide1(), slide2(), slide3(), slide4()]

STR = {
    'ar': {'TTL': 'قالب نموذج آركس', 'N1': 'الانتباه', 'N2': 'الصلة', 'N3': 'الثقة', 'N4': 'الرضا',
           'BUL': 'أضف النص المطلوب هنا.', 'SMP': 'هذا نص توضيحي. أضف النص المطلوب هنا',
           'SMP2': 'هذا نص توضيحي. أضف النص المطلوب هنا', 'CTR': 'دافعية الطالب'},
    'en': {'TTL': 'ARCS Model PowerPoint Template', 'N1': 'ATTENTION', 'N2': 'RELEVANCE', 'N3': 'CONFIDENCE', 'N4': 'SATISFACTION',
           'BUL': 'Insert your desired text here.', 'SMP': 'This is a sample text. Insert your desired text here',
           'SMP2': 'This is a sample text. Insert your desired text here', 'CTR': 'STUDENT MOTIVATION'},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'hr', 'icon': '🎯',
    'name': 'نموذج آركس للدافعية', 'nameEn': 'ARCS Motivation Model',
    'description': 'أربع شرائح لنموذج آركس (الانتباه، الصلة، الثقة، الرضا): أعمدة، صفوف، مربعات على خط، وبتلات متقاطعة',
    'descriptionEn': 'Four slides of the ARCS model (Attention, Relevance, Confidence, Satisfaction): columns, rows, squares, petals',
    'sections': [
        {'title': 'الأعمدة الأربعة', 'titleEn': 'Four Columns'},
        {'title': 'الصفوف الأربعة', 'titleEn': 'Four Rows'},
        {'title': 'المربعات على خط', 'titleEn': 'Squares on a Line'},
        {'title': 'البتلات حول دافعية الطالب', 'titleEn': 'Petals around Student Motivation'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص فقط؛ ألوان العناصر الأربعة متغيّرات CSS، والبتلات والموصّلات SVG في الشريحة الرابعة.',
}

# خط الأصل أقرب إلى Open Sans (Segoe UI)
FONTS = {'ar': '<link href="https://fonts.googleapis.com/css2?family=Tajawal:wght@400;500;700;800&display=swap" rel="stylesheet">',
         'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@400;600;700;800&display=swap" rel="stylesheet">'}
FONT_STACK = {'ar': "'Tajawal', 'Segoe UI', Tahoma, sans-serif", 'en': "'Open Sans', 'Segoe UI', Arial, sans-serif"}
