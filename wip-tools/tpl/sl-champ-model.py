# -*- coding: utf-8 -*-
# نموذج «نموذج تشامب» (CHAMP): شريحتان؛ أربعة أعمدة ملوّنة بحرف كبير وشريط اسم وأيقونة ونص، ثم أربعة صفوف أفقية بدائرة أيقونة وحرف ونص.
KEY = 'sl-champ-model'
TITLE = {'ar': 'نموذج تشامب', 'en': 'CHAMP Model'}
COMMENT = 'نموذج «نموذج تشامب»: الشريحة الأولى أربعة أعمدة (رمادي مزرق، أزرق، برتقالي، أخضر) في كل منها حرف أبيض كبير وشريط داكن بالاسم وأيقونة وفقرتان؛ الثانية أربعة صفوف بدائرة أيقونة داكنة وشريط بالاسم ومربع داكن بالحرف ونص.'
VARS = ('--ink: #000000; --accent: #3598DB; --bg: #FFFFFF; '
        '--c1: #44546B; --c1d: #333E50; --c2: #3598DB; --c2d: #1F74AD; --c3: #ED7D31; --c3d: #C55B11; --c4: #70AD46; --c4d: #548235;')

STYLE = """  .ttl { position: absolute; top: 38px; inset-inline-start: 64px; margin: 0; font-size: 42px; font-weight: 400; line-height: 1.2; color: var(--ink); white-space: nowrap; }
  html[lang="ar"] .ttl { font-weight: 500; top: 34px; }
  .k1 { --c: var(--c1); --cd: var(--c1d); } .k2 { --c: var(--c2); --cd: var(--c2d); }
  .k3 { --c: var(--c3); --cd: var(--c3d); } .k4 { --c: var(--c4); --cd: var(--c4d); }
  .ico { color: #fff; } .ico svg { width: 100%; height: 100%; }
  /* الأيقونات غير المتماثلة تنعكس في العربية */
  html[dir="rtl"] .ico.m svg { transform: scaleX(-1); }
  /* الحروف اللاتينية الكبيرة بخط الأصل في اللغتين */
  .col .big, .row .sq { font-family: 'Open Sans', 'Segoe UI', Arial, sans-serif; }
  /* الشريحة 1: أعمدة */
  .col { position: absolute; top: 114px; width: 285px; height: 550px; background: var(--c); color: #fff; }
  .col.k1 { inset-inline-start: 63px; } .col.k2 { inset-inline-start: 352px; } .col.k3 { inset-inline-start: 642px; } .col.k4 { inset-inline-start: 931px; }
  .col .big { position: absolute; top: 0; inset-inline-start: 0; width: 100%; height: 186px; margin: 0; display: flex; align-items: center; justify-content: center; font-size: 106px; font-weight: 700; line-height: 1; letter-spacing: -1px; }
  .col .band { position: absolute; top: 186px; inset-inline-start: 0; width: 100%; height: 53px; margin: 0; background: var(--cd); display: flex; align-items: center; justify-content: center; font-size: 21px; font-weight: 400; }
  .col .ico { position: absolute; top: 269px; inset-inline-start: 100px; width: 85px; height: 85px; }
  .col .tx { position: absolute; top: 375px; inset-inline-start: 30px; width: 228px; }
  .col p { margin: 0 0 23px; font-size: 17px; line-height: 23px; }
  /* الشريحة 2: صفوف */
  .row { position: absolute; height: 121px; inset-inline-start: 0; width: 1280px; color: #fff; }
  .row.k1 { top: 124px; } .row.k2 { top: 257px; } .row.k3 { top: 391px; } .row.k4 { top: 524px; }
  .row .lb { position: absolute; top: 0; inset-inline-start: 115px; width: 338px; height: 100%; background: var(--c); }
  .row .cir { position: absolute; top: -2px; inset-inline-start: 53px; width: 125px; height: 125px; border-radius: 50%; background: var(--cd); display: grid; place-items: center; }
  .row .cir .ico { width: 50px; height: 50px; }
  .row h2 { position: absolute; top: 0; inset-inline-start: 216px; height: 100%; margin: 0; display: flex; align-items: center; font-size: 21px; font-weight: 400; }
  .row .rb { position: absolute; top: 0; inset-inline-start: 459px; width: 757px; height: 100%; background: var(--c); }
  .row .sq { position: absolute; top: 0; inset-inline-start: 459px; width: 154px; height: 100%; margin: 0; background: var(--cd); display: flex; align-items: center; justify-content: center; font-size: 60px; font-weight: 700; line-height: 1; }
  .row p { position: absolute; top: 0; inset-inline-start: 664px; width: 472px; height: 100%; margin: 0; display: flex; align-items: center; font-size: 17px; line-height: 23px; }"""

ICONS = {
    # عدّاء يقفز حاجزًا
    'hurdle': ('<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">'
               '<circle cx="15.2" cy="3.4" r="2" fill="currentColor" stroke="none"/>'
               '<path stroke-width="2.6" d="M13.6 7.4 10.6 12.2"/>'
               '<path stroke-width="1.9" d="M13.4 7.6 17.6 8.2 20 10.2M12.6 8.3 8.8 7.6 6.8 9.6"/>'
               '<path stroke-width="2.2" d="M10.6 12.2 15.6 12.6 19.6 14M10.6 12.2 6.4 14.2 3.6 12.8"/>'
               '<path stroke-width="1.6" d="M6.6 16.4H14M7.4 16.4V22.4M13.2 16.4V20.4M7.4 22.4H9.4"/></svg>'),
    # مبنى مؤسسي بأعمدة
    'bank': ('<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">'
             '<path fill-rule="evenodd" d="M12 1.6 2.2 6.8V8.4h19.6V6.8zM12 4a1 1 0 1 0 0 2 1 1 0 0 0 0-2z"/>'
             '<rect x="3" y="9.4" width="18" height="1.2"/>'
             '<rect x="4.2" y="11.2" width="1.6" height="7.4"/><rect x="7.4" y="11.2" width="1.6" height="7.4"/><rect x="10.6" y="11.2" width="1.6" height="7.4"/>'
             '<rect x="13.8" y="11.2" width="1.6" height="7.4"/><rect x="17" y="11.2" width="1.6" height="7.4"/>'
             '<rect x="3" y="19.2" width="18" height="1.4"/><rect x="1.6" y="21.2" width="20.8" height="1.6"/></svg>'),
    # سبائك ذهب مكدّسة
    'gold': ('<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linejoin="round" aria-hidden="true">'
             '<path d="M9.4 2.6h5.2l1.2 4.4H8.2z"/>'
             '<path d="M5.6 9h5.2l1.2 4.4H4.4zM13.2 9h5.2l1.2 4.4h-7.6z"/>'
             '<path d="M2 15.4h5.2l1.2 4.4H.8zM9.4 15.4h5.2l1.2 4.4H8.2zM16.8 15.4H22l1.2 4.4h-7.6z"/>'
             '<path d="M.8 21.4h22.4"/></svg>'),
    # سهم دائري وقائمة مرتّبة
    'prio': ('<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" aria-hidden="true">'
             '<path d="M6.4 20.6A7.6 7.6 0 0 1 6.4 4.6" stroke-linecap="round"/><path d="M3.6 3.4 7.2 4.4 5.6 7.6" stroke-linecap="round" stroke-linejoin="round"/>'
             '<rect x="10.4" y="2.4" width="11" height="5" rx=".5"/><rect x="10.4" y="9.6" width="11" height="5" rx=".5"/><rect x="10.4" y="16.8" width="11" height="5" rx=".5"/></svg>'),
}
ORDER = ['hurdle', 'bank', 'gold', 'prio']
MIR = lambda n: ' m' if ORDER[n - 1] in ('hurdle', 'prio') else ''


def col(n):
    return ('<div class="col k%d"><p class="big">§L%d§</p><h2 class="band">§H%d§</h2><div class="ico%s">%s</div>'
            '<div class="tx"><p>§PA§</p><p>§PB§</p></div></div>' % (n, n, n, MIR(n), ICONS[ORDER[n - 1]]))


def row(n):
    return ('<div class="row k%d"><div class="lb"></div><div class="cir"><div class="ico%s">%s</div></div><h2>§H%d§</h2>'
            '<div class="rb"></div><p class="sq">§L%d§</p><p>§PR§</p></div>' % (n, MIR(n), ICONS[ORDER[n - 1]], n, n))


SLIDES = [
    '\n    <h1 class="ttl">§TTL§</h1>\n    ' + '\n    '.join(col(n) for n in range(1, 5)) + '\n  ',
    '\n    <h1 class="ttl">§TTL§</h1>\n    ' + '\n    '.join(row(n) for n in range(1, 5)) + '\n  ',
]

STR = {
    'ar': {'TTL': 'قالب نموذج تشامب', 'L1': 'Ch', 'L2': 'A', 'L3': 'M', 'L4': 'P',
           'H1': 'التحديات', 'H2': 'الصلاحية', 'H3': 'المال', 'H4': 'تحديد الأولويات',
           'PA': 'أدخل النص المطلوب هنا. هذا نص توضيحي.',
           'PB': 'أدخل النص المطلوب هنا. أدخل النص المطلوب هنا.',
           'PR': 'أدخل النص المطلوب هنا. هذا نص توضيحي. أدخل النص المطلوب هنا. أدخل النص المطلوب هنا.'},
    'en': {'TTL': 'CHAMP Model PowerPoint Template', 'L1': 'Ch', 'L2': 'A', 'L3': 'M', 'L4': 'P',
           'H1': 'CHALLENGES', 'H2': 'AUTHORITY', 'H3': 'MONEY', 'H4': 'PRIORITIZATION',
           'PA': 'Insert your desired text here. This is a sample text.',
           'PB': 'Insert your desired text here. Insert your desired text here.',
           'PR': 'Insert your desired text here. This is a sample text. Insert your desired text here. Insert your desired text here.'},
}

FONTS = {'ar': '<link href="https://fonts.googleapis.com/css2?family=Tajawal:wght@400;500;700;800&family=Open+Sans:wght@700&display=swap" rel="stylesheet">',
         'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@400;600;700&display=swap" rel="stylesheet">'}
FONT_STACK = {'en': "'Open Sans', 'Segoe UI', Arial, sans-serif"}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'sales', 'icon': '🏆',
    'name': 'نموذج تشامب لتأهيل العملاء', 'nameEn': 'CHAMP Model',
    'description': 'شريحتان لنموذج تشامب: التحديات والصلاحية والمال والأولويات في أعمدة ثم في صفوف',
    'descriptionEn': 'Two CHAMP slides: Challenges, Authority, Money and Prioritization as columns, then as rows',
    'sections': [
        {'title': 'أعمدة النموذج الأربعة', 'titleEn': 'Four Columns'},
        {'title': 'صفوف النموذج الأربعة', 'titleEn': 'Four Rows'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والألوان الأربعة وغيّر النصوص فقط؛ الحروف الكبيرة (Ch, A, M, P) اختصار النموذج.',
}
