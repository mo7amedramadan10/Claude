# -*- coding: utf-8 -*-
# نموذج «استراتيجية الباربل»: ثلاث شرائح؛ باربل مائل بطرفين غير متكافئين، ثم باربل متوازن فوق ثلاث مناطق، ثم باربل بقضيب منحنٍ.
KEY = 'sl-barbell-strategy'
TITLE = {'ar': 'استراتيجية الباربل', 'en': 'Barbell Strategy'}
COMMENT = 'نموذج «استراتيجية الباربل»: عنوان أعلى، ورسم باربل SVG (قضيب وأقراص وظلال) يملأ الإطار وينعكس في العربية، والنصوص كتل HTML بمواضع مطلقة منطقية.'
VARS = '--ink: #000000; --ink-2: #262626; --accent: #3381B0; --bg: #FFFFFF; --bar: #232A32; --plate: #475562; --plate-2: #36404A; --edge: #2B333C; --shadow: #CFCFCF; --gray: #BFBFBF; --orange: #E0603A; --teal: #275A7C; --green: #D1E2B6; --red: #EE9C86;'

FONTS = {'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@400;600;700&display=swap" rel="stylesheet">'}
FONT_STACK = {'en': "'Open Sans', 'Segoe UI', Tahoma, Arial, sans-serif"}

STYLE = """  .ttl { position: absolute; top: 40px; inset-inline-start: 64px; margin: 0; font-size: 42px; font-weight: 400; line-height: 1.3; color: var(--ink); }
  /* رسم الباربل يغطي الإطار كاملًا وينعكس في العربية */
  .bb { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; pointer-events: none; }
  html[dir="rtl"] .bb.mir { transform: scaleX(-1); }
  .bb .bar { fill: var(--bar); } .bb .pl { fill: var(--plate); stroke: var(--edge); stroke-width: 2; } .bb .pl2 { fill: var(--plate-2); stroke: var(--edge); stroke-width: 2; }
  .bb .col { fill: var(--plate-2); } .bb .sh { fill: var(--shadow); } .bb .ln { stroke: var(--gray); stroke-width: 3; fill: none; }
  .bb .curve { stroke: var(--bar); stroke-width: 14; stroke-linecap: round; fill: none; }
  .tx { position: absolute; margin: 0; text-align: center; }
  .lst { font-size: 18px; line-height: 22.5px; color: var(--ink-2); }
  .big { font-size: 27px; font-weight: 700; line-height: 31px; }
  /* الخط العربي أصغر بصريًا فيُكبَّر قليلًا */
  html[lang="ar"] .lst, html[lang="ar"] .s2 .zt { font-size: 20px; line-height: 26px; } html[lang="ar"] .big, html[lang="ar"] .s3 .ext { font-size: 28px; line-height: 34px; } html[lang="ar"] .s2 .inv { font-size: 25px; } html[lang="ar"] .s2 .zt1 { width: 150px; } html[lang="ar"] .s2 .zt3 { width: 175px; }
  /* الشريحة 1 */
  .s1 .badge { position: absolute; top: 149px; inset-inline-start: 465px; width: 350px; height: 53px; border-radius: 7px; background: var(--accent); color: #fff; display: grid; place-items: center; margin: 0; font-size: 29px; font-weight: 700; letter-spacing: .3px; }
  .s1 .cash { top: 213px; inset-inline-start: 204px; width: 200px; }
  .s1 .der { top: 213px; inset-inline-start: 882px; width: 200px; }
  .s1 .none { top: 320px; inset-inline-start: 475px; width: 400px; color: var(--teal); }
  .s1 .cons { top: 575px; inset-inline-start: 212px; width: 300px; color: var(--orange); }
  .s1 .aggr { top: 575px; inset-inline-start: 781px; width: 300px; color: #255E7D; }
  /* الشريحة 2 */
  .s2 .zone { position: absolute; top: 220px; height: 280px; }
  .s2 .z1 { inset-inline-start: 190px; width: 318px; background: var(--green); }
  .s2 .z2 { inset-inline-start: 532px; width: 216px; background: var(--red); }
  .s2 .z3 { inset-inline-start: 772px; width: 318px; background: var(--green); }
  .s2 .zt { position: absolute; margin: 0; font-size: 18px; line-height: 23px; color: #111; }
  .s2 .zt1 { top: 239px; inset-inline-start: 210px; width: 120px; text-align: start; }
  .s2 .zt3 { top: 239px; inset-inline-end: 213px; width: 150px; text-align: end; }
  .s2 .zt2 { top: 405px; inset-inline-start: 532px; width: 216px; text-align: center; }
  .s2 .inv { position: absolute; top: 530px; margin: 0; font-size: 23px; font-weight: 700; line-height: 30px; color: #000; text-align: center; }
  .s2 .i1 { inset-inline-start: 190px; width: 318px; } .s2 .i2 { inset-inline-start: 532px; width: 216px; } .s2 .i3 { inset-inline-start: 772px; width: 318px; }
  /* الشريحة 3 */
  .s3 .ext { top: 500px; width: 240px; font-size: 24px; font-weight: 700; line-height: 31px; color: #000; }
  .s3 .e1 { inset-inline-start: 213px; } .s3 .e2 { inset-inline-start: 820px; }"""


def rect(x, y, w, h, cls, rx=6):
    return '<rect class="%s" x="%s" y="%s" width="%s" height="%s" rx="%s"/>' % (cls, x, y, w, h, rx)


# الشريحة 1: باربل مائل (5.8°) حول مركز الأقراص اليسرى (343،424)؛ ثلاثة أقراص كبيرة يسارًا وقرص صغير يمينًا
BB1 = ('<svg class="bb mir" viewBox="0 0 1280 720" aria-hidden="true">'
       '<path class="ln" d="M221,167 V187 M221,177 H452 M828,177 H1059 M1059,167 V187"/>'
       '<path class="ln" d="M443,357 V377 M443,367 H906 M906,357 V377"/>'
       '<ellipse class="sh" cx="322" cy="546" rx="92" ry="12"/><ellipse class="sh" cx="922" cy="547" rx="80" ry="11"/>'
       '<g transform="rotate(5.8 343 424)">'
       + rect(204, 417, 829, 14, 'bar', 7)
       + rect(268, 386, 30, 76, 'col') + rect(291, 304, 35, 240, 'pl', 7) + rect(326, 306, 34, 236, 'pl2', 7) + rect(360, 304, 35, 240, 'pl', 7)
       + rect(948, 386, 26, 76, 'col') + rect(915, 362, 36, 124, 'pl', 6)
       + '</g></svg>')

# الشريحة 2: باربل أفقي متوازن (متماثل فلا يحتاج انعكاسًا)
BB2 = ('<svg class="bb" viewBox="0 0 1280 720" aria-hidden="true">'
       + rect(285, 353, 710, 14, 'bar', 7)
       + rect(348, 327, 32, 65, 'col') + rect(373, 287, 31, 147, 'pl') + rect(403, 287, 29, 147, 'pl2') + rect(431, 262, 31, 197, 'pl')
       + rect(900, 327, 50, 65, 'col') + rect(896, 287, 31, 147, 'pl') + rect(868, 287, 29, 147, 'pl2') + rect(838, 262, 31, 197, 'pl')
       + '</svg>')

# الشريحة 3: قضيب منحنٍ يتدلّى نحو الطرف الأخف
BB3 = ('<svg class="bb mir" viewBox="0 0 1280 720" aria-hidden="true">'
       '<ellipse class="sh" cx="308" cy="432" rx="92" ry="11"/><ellipse class="sh" cx="972" cy="433" rx="64" ry="11"/>'
       '<path class="curve" d="M231,327 C 300,316 360,310 470,309 C 650,309 850,333 1048,367"/>'
       '<g transform="rotate(3 342 317)">' + rect(266, 280, 30, 76, 'col') + rect(290, 199, 35, 236, 'pl', 7) + rect(325, 200, 35, 234, 'pl2', 7) + rect(359, 199, 36, 236, 'pl', 7) + '</g>'
       '<g transform="rotate(4 943 351)">' + rect(952, 313, 28, 76, 'col') + rect(925, 276, 36, 150, 'pl', 7) + '</g>'
       '</svg>')

SLIDES = [
    '\n    <h1 class="ttl">§TTL§</h1>\n    ' + BB1 + '\n    <p class="badge">§BADGE§</p>'
    '\n    <p class="tx lst cash">§CASH§</p>\n    <p class="tx lst der">§DER§</p>\n    <p class="tx big none">§NONE§</p>'
    '\n    <p class="tx big cons">§CONS§</p>\n    <p class="tx big aggr">§AGGR§</p>\n  ',
    '\n    <h1 class="ttl">§TTL§</h1>\n    <div class="zone z1"></div><div class="zone z2"></div><div class="zone z3"></div>\n    ' + BB2 +
    '\n    <p class="zt zt1">§LOW§</p>\n    <p class="zt zt3">§HIGH§</p>\n    <p class="zt zt2">§MID§</p>'
    '\n    <p class="inv i1">§INV1§</p>\n    <p class="inv i2">§AVOID§</p>\n    <p class="inv i3">§INV2§</p>\n  ',
    '\n    <h1 class="ttl">§TTL§</h1>\n    ' + BB3 + '\n    <p class="tx ext e1">§EXT1§</p>\n    <p class="tx ext e2">§EXT2§</p>\n  ',
]

STR = {
    'ar': {'TTL': 'قالب استراتيجية الباربل', 'BADGE': 'استراتيجية الباربل',
           'CASH': 'النقد<br>الذهب<br>سندات الخزانة', 'DER': 'المشتقات المالية', 'NONE': 'لا شيء هنا',
           'CONS': 'شديد<br>التحفّظ', 'AGGR': 'شديد<br>المجازفة',
           'LOW': 'تدريب منخفض المخاطر', 'HIGH': 'تدريب عالي المخاطر عالي العائد',
           'MID': 'مخاطرة معتدلة<br>عائد معتدل<br>(«المنطقة الحرام»)',
           'INV1': 'استثمر (80-90%)', 'AVOID': 'تجنّب', 'INV2': 'استثمر (10-20%)',
           'EXT1': 'متحفّظ<br>للغاية', 'EXT2': 'مجازف<br>للغاية'},
    'en': {'TTL': 'Barbell Strategy PowerPoint Template', 'BADGE': 'BARBELL STRATEGY',
           'CASH': 'Cash<br>Gold<br>Treasury Bonds', 'DER': 'Derivatives', 'NONE': 'NOTHING IN HERE',
           'CONS': 'HYPER-<br>CONSERVATIVE', 'AGGR': 'HYPER-<br>AGGRESSIVE',
           'LOW': 'Low Risk Training', 'HIGH': 'High-Risk High-Reward Training',
           'MID': 'Moderate Risk<br>Moderate Reward<br>("No Man\'s Land")',
           'INV1': 'Invest (80-90%)', 'AVOID': 'Avoid', 'INV2': 'Invest (10-20%)',
           'EXT1': 'Extremely<br>Conservative', 'EXT2': 'Extremely<br>Aggressive'},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'finance', 'icon': '🏋️',
    'name': 'استراتيجية الباربل', 'nameEn': 'Barbell Strategy',
    'description': 'ثلاث شرائح لاستراتيجية الباربل: طرفا المحفظة، وتوزيع الاستثمار على المناطق الثلاث، والطرفان المتطرفان',
    'descriptionEn': 'Three barbell strategy slides: the two portfolio ends, allocation across three zones, and the extremes',
    'sections': [
        {'title': 'الباربل: متحفّظ مقابل مجازف', 'titleEn': 'Barbell: Conservative vs. Aggressive'},
        {'title': 'توزيع الاستثمار على المناطق', 'titleEn': 'Investment Allocation by Zone', 'needsData': True},
        {'title': 'الطرفان المتطرفان', 'titleEn': 'The Two Extremes'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط ورسم الباربل SVG، وغيّر النصوص والنسب المئوية فقط.',
}
