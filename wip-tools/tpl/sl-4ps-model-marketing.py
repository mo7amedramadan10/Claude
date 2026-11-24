# -*- coding: utf-8 -*-
# نموذج «المزيج التسويقي 4Ps»: خمس شرائح بالتخطيط نفسه — حلقة بأربعة أرباع حول دائرة «المزيج التسويقي» وأربع بطاقات
# بأيقونة وعنوان ونص؛ الشريحة الأولى كلها ملوّنة ثم تُبرز كل شريحة ربعًا واحدًا (المنتج، السعر، الترويج، المكان) ويبقى الباقي رماديًا.
import math

KEY = 'sl-4ps-model-marketing'
TITLE = {'ar': 'نموذج المزيج التسويقي 4Ps', 'en': '4Ps Model Marketing'}
COMMENT = 'نموذج «المزيج التسويقي 4Ps»: عنوان أعلى، حلقة SVG بأربعة أرباع ملوّنة حول دائرة مركزية، وأربع بطاقات بإطار يتبع قوس الحلقة وأيقونة دائرية وعنوان ونص. الصنف on يُبرز الربع والبطاقة والتسمية بلونهما، وبدونه يصيران رماديين.'
VARS = '--ink: #1B1B1B; --ink-2: #333333; --accent: #01B29E; --teal: #01B29E; --orange: #E77143; --slate: #546674; --blue: #018DC8; --gray: #F2F2F2; --gray-2: #D9D9D9; --gray-3: #BFBFBF; --gray-4: #7A7A7A; --bg: #FFFFFF;'

STYLE = """  .ttl { position: absolute; top: 44px; inset-inline-start: 65px; margin: 0; font-size: 40px; font-weight: 300; line-height: 1.2; color: var(--ink); letter-spacing: -0.3px; }
  html[lang="ar"] .ttl { font-weight: 400; letter-spacing: 0; }
  /* الرسم الزخرفي (إطارات البطاقات وأرباع الحلقة) يُعكس في العربية */
  .dg { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; pointer-events: none; }
  html[dir="rtl"] .dg { transform: scaleX(-1); }
  .q1, .c1, .l1 { --c: var(--teal); } .q2, .c2, .l2 { --c: var(--orange); } .q3, .c3, .l3 { --c: var(--slate); } .q4, .c4, .l4 { --c: var(--blue); }
  .dg .seg { fill: var(--gray); stroke: var(--gray); stroke-width: 6; stroke-linejoin: round; }
  .dg .ol { fill: none; stroke: var(--gray-3); stroke-width: 2; stroke-linejoin: round; }
  .dg .q.on .seg { fill: var(--c); stroke: var(--c); }
  .dg .q.on .ol { stroke: var(--c); }
  /* الدائرة المركزية */
  .core { position: absolute; top: 289px; inset-inline-start: 540px; width: 200px; height: 200px; border-radius: 50%; background: linear-gradient(180deg, #F2F2F2 0%, #DCDCDC 100%); box-shadow: 0 6px 14px rgb(0 0 0 / .28), 0 0 0 1px rgb(0 0 0 / .04); }
  .core p { position: absolute; inset: 0; margin: 0; display: grid; place-items: center; padding: 0 18px; text-align: center; font-size: 26px; font-weight: 800; line-height: 1.15; color: #111; }
  html[lang="ar"] .core p { font-size: 25px; }
  /* تسميات الأرباع: نص مائل 45° داخل الحلقة (في الأصل على قوس) */
  .lbl { position: absolute; width: 230px; margin: 0; text-align: center; font-size: 26px; font-weight: 700; letter-spacing: 1px; line-height: 32px; color: #777; transform: rotate(var(--r)); }
  .lbl.on { color: #fff; }
  html[dir="rtl"] .lbl { transform: rotate(calc(-1 * var(--r))); letter-spacing: 0; font-size: 28px; }
  .l1 { top: 260px; inset-inline-start: 412px; --r: -45deg; }
  .l2 { top: 260px; inset-inline-end: 412px; --r: 45deg; }
  .l3 { top: 486px; inset-inline-start: 412px; --r: 45deg; }
  .l4 { top: 486px; inset-inline-end: 412px; --r: -45deg; }
  /* البطاقات */
  .card { position: absolute; width: 491px; height: 265px; }
  .c1, .c3 { inset-inline-start: 66px; } .c2, .c4 { inset-inline-end: 66px; text-align: end; }
  .c1, .c2 { top: 119px; } .c3, .c4 { top: 393px; }
  .card .ic { position: absolute; width: 76px; height: 76px; border-radius: 50%; background: var(--gray-2); color: #7A7A7A; display: grid; place-items: center; }
  .card .ic svg { width: 38px; height: 38px; }
  .c1 .ic, .c3 .ic { inset-inline-start: 27px; } .c2 .ic, .c4 .ic { inset-inline-end: 28px; }
  .c1 .ic, .c2 .ic { top: 0; } .c3 .ic, .c4 .ic { top: 189px; }
  .card h2 { position: absolute; margin: 0; font-size: 24px; font-weight: 700; line-height: 1.2; color: var(--gray-4); }
  .card p { position: absolute; margin: 0; width: 300px; font-size: 17px; line-height: 22px; color: var(--gray-4); white-space: pre-line; }
  html[lang="ar"] .card p { font-size: 18px; }
  .c1 h2, .c1 p, .c3 h2, .c3 p { inset-inline-start: 26px; } .c2 h2, .c2 p, .c4 h2, .c4 p { inset-inline-end: 32px; }
  .c1 h2, .c2 h2 { top: 91px; } .c1 p, .c2 p { top: 133px; }
  .c3 h2, .c4 h2 { top: 27px; } .c3 p, .c4 p { top: 69px; }
  .card.on .ic { background: var(--c); color: #fff; }
  .card.on h2 { color: var(--c); }
  .card.on p { color: var(--ink-2); }"""

ICONS = {
    'boxes': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><rect x="9.3" y="3" width="5.4" height="5.4" rx=".5"/><rect x="6.2" y="9.3" width="5.4" height="5.4" rx=".5"/><rect x="12.4" y="9.3" width="5.4" height="5.4" rx=".5"/><rect x="3" y="15.6" width="5.4" height="5.4" rx=".5"/><rect x="9.3" y="15.6" width="5.4" height="5.4" rx=".5"/><rect x="15.6" y="15.6" width="5.4" height="5.4" rx=".5"/></svg>',
    'money': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M6.2 8.6 18.4 5.2l.9 3.2-1.9.5-.4-1.3L8.3 10.3H6.2z"/><path fill-rule="evenodd" d="M2.5 10.6h18.4v8.6H2.5zm1.8 1.8v5h14.8v-5zm7.4.9a1.6 1.6 0 1 1 0 3.2 1.6 1.6 0 0 1 0-3.2z"/></svg>',
    'mega': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><g transform="rotate(-30 12 12)"><path d="M14 4.5v15l-6.2-4.1H4.5a1.2 1.2 0 0 1-1.2-1.2V9.8a1.2 1.2 0 0 1 1.2-1.2h3.3z"/><path d="M16 8.3a4.5 4.5 0 0 1 0 7.4v-2a2.5 2.5 0 0 0 0-3.4z"/><path d="M6.2 16.3h2.6l1 3.6H7.3z"/></g></svg>',
    'globe': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path fill-rule="evenodd" d="M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20zm0 2a8 8 0 1 1 0 16 8 8 0 0 1 0-16z"/><path d="M8.2 5.3c1.6.4 3 .3 3.4 1.5.5 1.3-1.6 1.7-1.3 3 .3 1.4 2.6 1 2.9 2.6.3 1.5-1.8 2.3-1.4 4 .2 1 .8 1.6.6 2.4C8.3 18 5.3 15.3 5.3 12c0-2.9 1.1-5.3 2.9-6.7zM15.5 6.5c1.8 1.1 3.2 3.2 3.2 5.5 0 1.4-.4 2.7-1.1 3.8-.8-.6-.5-1.6-1.4-2.1-1-.6-2.2.3-2.5-1.1-.3-1.3 1.3-1.6 1.2-2.9-.1-1.2-1.6-1.7-1.2-2.9.3-.6 1.2-.5 1.8-.3z"/></svg>',
}

CX, CY = 640, 389          # مركز الحلقة
R_OUT, R_IN, GAP = 220, 118, 9   # نصفا القطر وفجوة الربع قبل إضافة حدّ 6px (الفعلي: 223/115 وفجوة ±6)
R_OL = 236                 # نصف قطر قوس إطار البطاقة


def seg_path():
    # الربع العلوي الأيسر؛ الأرباع الأخرى بتحويلات مرآة
    ax = CX - math.sqrt(R_OUT ** 2 - GAP ** 2); ay = CY - GAP
    bx = CX - GAP; by = CY - math.sqrt(R_OUT ** 2 - GAP ** 2)
    cx_ = CX - GAP; cy_ = CY - math.sqrt(R_IN ** 2 - GAP ** 2)
    dx = CX - math.sqrt(R_IN ** 2 - GAP ** 2); dy = CY - GAP
    return 'M%.1f,%.1f A%d,%d 0 0 1 %.1f,%.1f L%.1f,%.1f A%d,%d 0 0 0 %.1f,%.1f Z' % (ax, ay, R_OUT, R_OUT, bx, by, cx_, cy_, R_IN, R_IN, dx, dy)


def outline_path():
    # إطار البطاقة العلوية اليسرى: خط علوي ينحني بزاوية مدوّرة إلى قوس يتبع الحلقة ثم خط سفلي وزوايا مدوّرة.
    # نقطة التحكم في المنحنى التربيعي = تقاطع مماس القوس عند نقطة البداية مع الخط العلوي، فيكون الالتقاء أملس.
    y1 = 178
    px = CX - math.sqrt(R_OL ** 2 - (CY - y1) ** 2)       # بداية القوس عند y1
    tx, ty = (CY - y1), (CX - px)                           # اتجاه المماس (عمودي على نصف القطر) نحو الأعلى واليمين
    qx = px + tx * (y1 - 156) / ty                          # تقاطع المماس مع y=156
    fx = CX - math.sqrt(R_OL ** 2 - (CY - 383) ** 2)       # نهاية القوس عند y=383
    return ('M77,156 H%.1f Q%.1f,156 %.1f,%d A%d,%d 0 0 0 %.1f,383 H77 A10,10 0 0 1 67,373 V166 A10,10 0 0 1 77,156 Z'
            % (qx - 34, qx, px, y1, R_OL, R_OL, fx))


TF = ['', 'matrix(-1 0 0 1 1280 0)', 'matrix(1 0 0 -1 0 %d)' % (2 * CY), 'matrix(-1 0 0 -1 1280 %d)' % (2 * CY)]


def diagram(on):
    out = '<svg class="dg" viewBox="0 0 1280 720" aria-hidden="true">'
    for i in range(4):
        n = i + 1
        out += '<g class="q q%d%s"%s><path class="ol" d="%s"/><path class="seg" d="%s"/></g>' % (
            n, ' on' if n in on else '', (' transform="%s"' % TF[i]) if TF[i] else '', outline_path(), seg_path())
    return out + '</svg>'


def card(n, icon, on):
    return ('<div class="card c%d%s"><div class="ic">%s</div><h2>§H%d§</h2><p>§P§</p></div>'
            % (n, ' on' if on else '', ICONS[icon], n))


def slide(on):  # on: مجموعة الأرباع المُبرزة {1: المنتج، 2: السعر، 3: الترويج، 4: المكان}
    labels = ''.join('<p class="lbl l%d%s">§L%d§</p>' % (n, ' on' if n in on else '', n) for n in (1, 2, 3, 4))
    return ('\n    <h1 class="ttl">§TTL§</h1>\n    ' + diagram(on)
            + '\n    <div class="core"><p>§MIX§</p></div>\n    ' + labels + '\n    '
            + card(1, 'boxes', 1 in on) + '\n    ' + card(2, 'money', 2 in on) + '\n    '
            + card(3, 'mega', 3 in on) + '\n    ' + card(4, 'globe', 4 in on) + '\n  ')


SLIDES = [slide({1, 2, 3, 4}), slide({1}), slide({2}), slide({3}), slide({4})]

STR = {
    'ar': {'TTL': 'قالب نموذج المزيج التسويقي 4Ps', 'MIX': 'المزيج التسويقي',
           'H1': 'المنتج', 'H2': 'السعر', 'H3': 'الترويج', 'H4': 'المكان',
           'L1': 'المنتج', 'L2': 'السعر', 'L3': 'الترويج', 'L4': 'المكان',
           'P': 'أضف النص المطلوب هنا.\nهذا نص توضيحي.'},
    'en': {'TTL': '4Ps Model Marketing PowerPoint Template', 'MIX': 'MARKETING MIX',
           'H1': 'Product', 'H2': 'Price', 'H3': 'Promotion', 'H4': 'Place',
           'L1': 'PRODUCT', 'L2': 'PRICE', 'L3': 'PROMOTION', 'L4': 'PLACE',
           'P': 'Insert your desired text here.\nThis is a sample text.'},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'marketing', 'icon': '🎯',
    'name': 'نموذج المزيج التسويقي 4Ps', 'nameEn': '4Ps Marketing Mix Model',
    'description': 'خمس شرائح لنموذج 4Ps: نظرة عامة ثم إبراز المنتج والسعر والترويج والمكان كلٌّ في شريحة',
    'descriptionEn': 'Five slides of the 4Ps marketing mix: overview, then Product, Price, Promotion and Place highlighted',
    'sections': [
        {'title': 'المزيج كاملًا', 'titleEn': 'Full Marketing Mix'},
        {'title': 'المنتج', 'titleEn': 'Product'},
        {'title': 'السعر', 'titleEn': 'Price'},
        {'title': 'الترويج', 'titleEn': 'Promotion'},
        {'title': 'المكان', 'titleEn': 'Place'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص فقط؛ الحلقة والإطارات رسم SVG والصنف on يحدد الربع والبطاقة والتسمية المُبرزة في كل شريحة.',
}
