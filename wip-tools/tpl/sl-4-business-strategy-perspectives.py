# -*- coding: utf-8 -*-
# نموذج «منظورات استراتيجية الأعمال»: خمس شرائح بالتخطيط نفسه: أربعة أسهم ملوّنة تتجه إلى دائرة مركزية
# رباعية الأجزاء؛ الشريحة الأولى كاملة الألوان، وكل شريحة تالية تُبرز منظورًا واحدًا وتُرمّد الباقي.
KEY = 'sl-4-business-strategy-perspectives'
TITLE = {'ar': 'منظورات استراتيجية الأعمال', 'en': 'Business Strategy Perspectives'}
COMMENT = 'نموذج «منظورات استراتيجية الأعمال»: عنوان أعلى، أربعة أسهم (أعلى/جهة البداية/جهة النهاية/أسفل) تتجه إلى دائرة مركزية بأربعة أرباع ولوحة بيضاء في الوسط، وعنوان منظور عند كل سهم. الصنف off يرمّد السهم وعنوانه؛ الأشكال SVG تُعكس أفقيًا في العربية.'
VARS = '--ink: #272727; --ink-2: #3D4248; --accent: #3081AC; --green: #85B142; --teal: #5FB7A1; --orange: #E45A35; --blue: #3081AC; --q-red: #B83C1A; --q-green: #688B39; --q-navy: #246180; --q-teal: #41907D; --off: #F2F2F2; --off-ink: #737373; --off-ic: #7F7F7F; --off-lbl: #BFBFBF; --bg: #FFFFFF;'

STYLE = """  .ttl { position: absolute; top: 42px; inset-inline-start: 64px; margin: 0; font-size: 40px; font-weight: 300; line-height: 1.2; color: var(--ink); letter-spacing: -0.2px; }
  html[lang="ar"] .ttl { font-weight: 400; }
  /* الرسم: الأسهم والدائرة المركزية (يُعكس في العربية) */
  .diag { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; }
  html[dir="rtl"] .diag { transform: scaleX(-1); }
  .diag .a { fill: var(--c); }
  .diag .a.off { fill: var(--off); }
  .diag .gap { stroke: #fff; stroke-width: 14; fill: none; }
  .diag .div { stroke: #fff; stroke-width: 6; }
  /* عناوين المنظورات */
  .lbl { position: absolute; margin: 0; font-size: 20px; font-weight: 700; line-height: 1.25; color: var(--ink); text-align: center; }
  .lbl.off { color: var(--off-lbl); }
  .lbl-top { top: 108px; inset-inline-start: 0; width: 1280px; }
  .lbl-bottom { top: 660px; inset-inline-start: 0; width: 1280px; }
  .lbl-side { top: 378px; width: 212px; }
  .lbl-start { inset-inline-start: 81px; transform: rotate(-90deg); }
  .lbl-end { inset-inline-start: 987px; transform: rotate(90deg); }
  html[dir="rtl"] .lbl-start { transform: rotate(90deg); }
  html[dir="rtl"] .lbl-end { transform: rotate(-90deg); }
  /* كتل النص والأيقونات داخل الأسهم */
  .blk { position: absolute; color: #fff; }
  .blk.off { --c: var(--off); color: var(--off-ink); }
  .blk .ic { position: absolute; color: #fff; }
  .blk.off .ic { color: var(--off-ic); }
  .blk .ic svg { width: 100%; height: 100%; }
  .blk p { position: absolute; margin: 0; font-size: 17px; line-height: 1.35; }
  .blk-top .ic { top: 178px; inset-inline-start: 464px; width: 48px; height: 46px; }
  .blk-top p { top: 178px; inset-inline-start: 552px; width: 240px; }
  .blk-bottom .ic { top: 576px; inset-inline-start: 462px; width: 60px; height: 52px; }
  .blk-bottom p { top: 577px; inset-inline-start: 552px; width: 240px; }
  .blk-start .ic { top: 318px; inset-inline-start: 332px; width: 52px; height: 60px; }
  .blk-start p { top: 393px; inset-inline-start: 265px; width: 185px; text-align: center; }
  .blk-end .ic { top: 324px; inset-inline-start: 890px; width: 50px; height: 54px; }
  .blk-end p { top: 393px; inset-inline-start: 823px; width: 185px; text-align: center; }
  /* نص المركز وتسميات الأرباع */
  .ctr { position: absolute; top: 369px; inset-inline-start: 540px; width: 200px; margin: 0; text-align: center; font-size: 17px; font-weight: 800; line-height: 1.3; color: var(--ink); }
  .ql { position: absolute; width: 120px; height: 24px; margin: 0; text-align: center; font-size: 19px; font-weight: 700; line-height: 24px; color: #fff; }
  .ql1 { top: 337px; inset-inline-start: 492px; transform: rotate(-45deg); }
  .ql2 { top: 335px; inset-inline-start: 665px; transform: rotate(45deg); }
  .ql3 { top: 471px; inset-inline-start: 508px; transform: rotate(45deg); }
  .ql4 { top: 470px; inset-inline-start: 657px; transform: rotate(-45deg); }
  /* في العربية تنعكس الزاوية وتُزاح التسمية قليلًا إلى الخارج لأن الحروف العربية أطول */
  html[dir="rtl"] .ql1 { transform: translate(6px, -6px) rotate(45deg); }
  html[dir="rtl"] .ql2 { transform: translate(-6px, -6px) rotate(-45deg); }
  html[dir="rtl"] .ql3 { transform: translate(6px, 6px) rotate(-45deg); }
  html[dir="rtl"] .ql4 { transform: translate(-6px, 6px) rotate(45deg); }
  html[lang="ar"] .ql { font-size: 18px; }"""

# الأيقونات: الأجزاء الداخلية بلون السهم (var(--c)) فتبدو مفرّغة
ICONS = {
    'customer': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><rect x="2.5" y="2.5" width="19" height="10.5" rx="1"/><rect x="5" y="5" width="14" height="1.6" fill="var(--c)"/><rect x="5" y="8" width="9" height="1.6" fill="var(--c)"/><rect x="11" y="13" width="2" height="2.2"/><circle cx="5.5" cy="16.3" r="2"/><circle cx="12" cy="15.6" r="2.3"/><circle cx="18.5" cy="16.3" r="2"/><path d="M1.5 23c0-2.3 1.8-3.8 4-3.8s4 1.5 4 3.8z"/><path d="M7.6 23c0-2.6 2-4.3 4.4-4.3s4.4 1.7 4.4 4.3z"/><path d="M14.5 23c0-2.3 1.8-3.8 4-3.8s4 1.5 4 3.8z"/></svg>',
    'head': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12.5 1.5a8 8 0 0 0-8 8c0 2.3 1 4.4 2.5 5.9V23h6.5v-2.8h1.6a1.9 1.9 0 0 0 1.9-1.9v-2.6h1.6l-1.6-3.2V9.5a8 8 0 0 0-4.5-8z"/><circle cx="11.6" cy="8.2" r="2.8" fill="var(--c)"/><rect x="10.3" y="10.8" width="2.6" height="1.8" rx=".4" fill="var(--c)"/><rect x="10.7" y="13" width="1.8" height=".9" rx=".3" fill="var(--c)"/></svg>',
    'gears': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M9.2 2.5h2.6l.4 2a6 6 0 0 1 1.5.9l1.9-.7 1.3 2.3-1.5 1.3a6 6 0 0 1 0 1.8l1.5 1.3-1.3 2.3-1.9-.7a6 6 0 0 1-1.5.9l-.4 2H9.2l-.4-2a6 6 0 0 1-1.5-.9l-1.9.7-1.3-2.3 1.5-1.3a6 6 0 0 1 0-1.8L4.1 7l1.3-2.3 1.9.7a6 6 0 0 1 1.5-.9zm1.3 4.3a2.5 2.5 0 1 0 0 5 2.5 2.5 0 0 0 0-5z"/><path d="M16.3 13.6h1.9l.3 1.4a4 4 0 0 1 1 .6l1.4-.5.9 1.6-1.1 1a4 4 0 0 1 0 1.2l1.1 1-.9 1.6-1.4-.5a4 4 0 0 1-1 .6l-.3 1.4h-1.9l-.3-1.4a4 4 0 0 1-1-.6l-1.4.5-.9-1.6 1.1-1a4 4 0 0 1 0-1.2l-1.1-1 .9-1.6 1.4.5a4 4 0 0 1 1-.6zm1 3.1a1.7 1.7 0 1 0 0 3.4 1.7 1.7 0 0 0 0-3.4z"/></svg>',
    'coins': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><ellipse cx="7.5" cy="5.5" rx="6" ry="2.4"/><path d="M1.5 5.5v9c0 1.3 2.7 2.4 6 2.4s6-1.1 6-2.4v-9c0 1.3-2.7 2.4-6 2.4s-6-1.1-6-2.4z"/><path d="M1.9 8.6c.9 1 3 1.6 5.6 1.6s4.7-.6 5.6-1.6v.9c-.9 1-3 1.6-5.6 1.6S2.8 10.5 1.9 9.5z" fill="var(--c)"/><path d="M1.9 11.6c.9 1 3 1.6 5.6 1.6s4.7-.6 5.6-1.6v.9c-.9 1-3 1.6-5.6 1.6s-4.7-.6-5.6-1.6z" fill="var(--c)"/><ellipse cx="16" cy="13" rx="6.3" ry="2.5" stroke="var(--c)" stroke-width=".8"/><path d="M9.7 13v6.6c0 1.4 2.8 2.5 6.3 2.5s6.3-1.1 6.3-2.5V13c0 1.4-2.8 2.5-6.3 2.5S9.7 14.4 9.7 13z" stroke="var(--c)" stroke-width=".8"/><path d="M10.1 16.3c1 1 3.2 1.7 5.9 1.7s4.9-.7 5.9-1.7v.9c-1 1-3.2 1.7-5.9 1.7s-4.9-.7-5.9-1.7z" fill="var(--c)"/></svg>',
}

# الأسهم الأربعة: مستطيل + رأس عريض يتجه إلى المركز (640,403)؛ الفجوات البيضاء خطوط فوقها
ARROWS = [
    ('top', 'var(--green)', '429,158 851,158 851,236 916,236 640,394 364,236 429,236'),
    ('start', 'var(--orange)', '241,275 364,275 364,236 656,403 364,570 364,531 241,531'),
    ('end', 'var(--teal)', '1039,275 916,275 916,236 624,403 916,570 916,531 1039,531'),
    ('bottom', 'var(--blue)', '429,648 851,648 851,570 916,570 640,412 364,570 429,570'),
]
GAPS = '<path class="gap" d="M640,394 L350,228 M640,394 L930,228 M640,412 L350,578 M640,412 L930,578"/>'
# اللوحة البيضاء المركزية: مربع 126×126 تتوسطه دائرة نصف قطرها 75 فتبرز الدائرة من الأضلاع وتبرز زوايا المربع
CIRCLE = ('<circle cx="640" cy="403" r="155" fill="#fff"/>'
          '<path d="M640,403 L494,403 A146,146 0 0 1 640,257 Z" fill="var(--q-red)"/>'
          '<path d="M640,403 L640,257 A146,146 0 0 1 786,403 Z" fill="var(--q-green)"/>'
          '<path d="M640,403 L494,403 A146,146 0 0 0 640,549 Z" fill="var(--q-navy)"/>'
          '<path d="M640,403 L640,549 A146,146 0 0 0 786,403 Z" fill="var(--q-teal)"/>'
          '<path class="div" d="M494,403 H786 M640,257 V549"/>'
          '<circle cx="640" cy="403" r="75" fill="#fff"/><rect x="577" y="340" width="126" height="126" fill="#fff"/>')


def diagram(on):
    polys = ''.join('<polygon class="a a-%s%s" style="--c:%s" points="%s"/>' % (n, '' if on in ('all', n) else ' off', c, p) for n, c, p in ARROWS)
    return '<svg class="diag" viewBox="0 0 1280 720" aria-hidden="true">' + polys + GAPS + CIRCLE + '</svg>'


def block(name, icon, on):
    off = '' if on in ('all', name) else ' off'
    return '<div class="blk blk-%s%s" style="--c:var(--%s)"><div class="ic">%s</div><p>§P§</p></div>' % (
        name, off, {'top': 'green', 'start': 'orange', 'end': 'teal', 'bottom': 'blue'}[name], ICONS[icon])


def label(name, cls, sym, on):
    off = '' if on in ('all', name) else ' off'
    return '<h2 class="lbl %s%s">§%s§</h2>' % (cls, off, sym)


def slide(on):  # on: 'all' أو اسم المنظور المُبرز
    return ('\n    <h1 class="ttl">§TTL§</h1>\n    ' + diagram(on) + '\n    '
            + label('top', 'lbl-top', 'L1', on) + label('end', 'lbl-side lbl-end', 'L2', on)
            + label('bottom', 'lbl-bottom', 'L3', on) + label('start', 'lbl-side lbl-start', 'L4', on) + '\n    '
            + block('top', 'customer', on) + block('start', 'head', on) + block('end', 'gears', on) + block('bottom', 'coins', on) + '\n    '
            + '<p class="ctr">§C§</p>'
            + ''.join('<span class="ql ql%d">§Q§</span>' % i for i in (1, 2, 3, 4)) + '\n  ')


SLIDES = [slide('all'), slide('top'), slide('end'), slide('bottom'), slide('start')]

STR = {
    'ar': {'TTL': 'قالب منظورات استراتيجية الأعمال',
           'L1': 'منظور العملاء', 'L2': 'منظور العمليات<br>الداخلية', 'L3': 'المنظور المالي', 'L4': 'منظور التعلّم<br>والنمو',
           'P': 'هذا نص توضيحي. أضف النص المطلوب هنا.', 'Q': 'عنوان نائب',
           'C': 'منظورات<br>استراتيجية<br>الأعمال'},
    'en': {'TTL': 'Business Strategy Perspectives PowerPoint Template',
           'L1': 'Customer Perspective', 'L2': 'Internal Processes<br>Perspective', 'L3': 'Financial Perspective', 'L4': 'Learning &amp; Growth<br>Perspective',
           'P': 'This is a sample text. Insert your desired text here.', 'Q': 'Placeholder',
           'C': 'BUSINESS<br>STRATEGY<br>PERSPECTIVES'},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'mgmt', 'icon': '🎯',
    'name': 'منظورات استراتيجية الأعمال', 'nameEn': 'Business Strategy Perspectives',
    'description': 'خمس شرائح لأربعة منظورات استراتيجية حول دائرة مركزية: نظرة عامة ثم إبراز كل منظور على حدة',
    'descriptionEn': 'Five slides of four strategy perspectives around a central circle: overview, then each highlighted',
    'sections': [
        {'title': 'المنظورات الأربعة', 'titleEn': 'All Four Perspectives'},
        {'title': 'منظور العملاء', 'titleEn': 'Customer Perspective'},
        {'title': 'منظور العمليات الداخلية', 'titleEn': 'Internal Processes Perspective'},
        {'title': 'المنظور المالي', 'titleEn': 'Financial Perspective'},
        {'title': 'منظور التعلّم والنمو', 'titleEn': 'Learning & Growth Perspective'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص فقط؛ الأسهم والدائرة رسم SVG والصنف off يرمّد المنظورات غير المُبرزة في كل شريحة.',
}
