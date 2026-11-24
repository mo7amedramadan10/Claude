# -*- coding: utf-8 -*-
# نموذج «نموذج أيدا بأربع خطوات»: خمس شرائح بالتخطيط نفسه؛ الأولى بكل الألوان، والباقي يُبرز مرحلة واحدة ويرمّد الباقي.
KEY = 'sl-4-step-aida-model'
TITLE = {'ar': 'نموذج أيدا بأربع خطوات', 'en': '4-Step AIDA Model'}
COMMENT = 'نموذج «نموذج أيدا بأربع خطوات»: عنوان أعلى، قمع مسطّح SVG في المنتصف من أربع مراحل (أزرق، أخضر مزرق، أخضر، أحمر) كل مرحلة بشريط داكن أعلاها وحرف أبيض كبير، وعمودان من العناوين والنصوص عن يمينه ويساره. الصنف off يرمّد المرحلة والنص غير المُبرزين.'
VARS = '--ink: #262626; --ink-2: #4A4A4A; --accent: #3081AC; --c1: #3081AC; --c1d: #2B759A; --c2: #5FB7A1; --c2d: #55A491; --c3: #8AB84B; --c3d: #7CA444; --c4: #D8543B; --c4d: #BF4B34; --g1: #D9D9D9; --g2: #F2F2F2; --g3: #BFBFBF; --gray-t: #C4C4C4; --bg: #FFFFFF;'

STYLE = """  .ttl { position: absolute; top: 42px; inset-inline-start: 65px; margin: 0; font-size: 40px; font-weight: 300; line-height: 1.2; color: var(--ink); }
  html[lang="ar"] .ttl { font-weight: 400; }
  /* القمع المسطّح: متماثل حول منتصف الشريحة فلا يحتاج عكسًا */
  .funnel { position: absolute; inset: 0; width: 1280px; height: 720px; }
  .funnel .d1 { fill: var(--c1d); } .funnel .b1 { fill: var(--c1); }
  .funnel .d2 { fill: var(--c2d); } .funnel .b2 { fill: var(--c2); }
  .funnel .d3 { fill: var(--c3d); } .funnel .b3 { fill: var(--c3); }
  .funnel .d4 { fill: var(--c4d); } .funnel .b4 { fill: var(--c4); }
  .funnel .offd { fill: var(--g1); } .funnel .offb { fill: var(--g2); }
  /* الحروف البيضاء داخل المراحل */
  .ltr { position: absolute; margin: 0; width: 120px; inset-inline-start: 579px; text-align: center; font-size: 75px; font-weight: 700; line-height: 1; color: #fff; }
  .l1 { top: 197px; } .l2 { top: 318px; } .l3 { top: 439px; } .l4 { top: 562px; }
  .ltr.off { color: var(--g3); }
  /* العمودان: العناوين والنصوص */
  .hd { position: absolute; margin: 0; font-size: 22px; font-weight: 700; line-height: 1.2; white-space: nowrap; }
  .tx { position: absolute; margin: 0; font-size: 17px; line-height: 1.35; width: 250px; color: var(--ink-2); }
  html[lang="ar"] .tx { line-height: 1.45; }
  .colL .hd, .colL .tx { inset-inline-start: 105px; } .colR .hd, .colR .tx { inset-inline-start: 961px; }
  .h1 { top: 172px; color: var(--c1); } .h2 { top: 294px; color: var(--c2); } .h3 { top: 416px; color: var(--c3); } .h4 { top: 540px; color: var(--c4); }
  .t1 { top: 209px; } .t2 { top: 331px; } .t3 { top: 452px; } .t4 { top: 576px; }
  .hd.off { color: var(--g3); } .tx.off { color: var(--gray-t); }"""

# كل مرحلة: شريط داكن أعلى ثم جسم بزاويتين مائلتين 45° تصلان بعرض المرحلة التالية
STAGES = [
    ('M373,153 h533 a8,8 0 0 1 8,8 v26 h-549 v-26 a8,8 0 0 1 8,-8 z', '365,187 914,187 914,217 857,274 422,274 365,217'),
    ('M422,274 h434 v39 h-434 z', '422,313 856,313 856,338 798,396 480,396 422,338'),
    ('M480,396 h318 v38 h-318 z', '480,434 798,434 798,460 740,518 538,518 480,460'),
    ('M538,518 h202 v38 h-202 z', '538,556 740,556 740,585 680,645 598,645 538,585'),
]


def funnel(active):
    out = '<svg class="funnel" viewBox="0 0 1280 720" aria-hidden="true">'
    for k, (band, body) in enumerate(STAGES, 1):
        on = active is None or active == k
        out += '<path class="%s" d="%s"/><polygon class="%s" points="%s"/>' % ('d%d' % k if on else 'offd', band, 'b%d' % k if on else 'offb', body)
    return out + '</svg>'


def col(side, k, head, active):
    o = ' off' if (active is not None and active != k) else ''
    return '<h2 class="hd h%d%s">%s</h2><p class="tx t%d%s">§P§</p>' % (k, o, head, k, o)


def slide(active):
    rows_l = ''.join(col('L', k, '§H%d§' % k, active) for k in (1, 2, 3, 4))
    rows_r = ''.join(col('R', k, '§H%d§' % k if active is None else '§EDIT§', active) for k in (1, 2, 3, 4))
    letters = ''.join('<p class="ltr l%d%s">§L%d§</p>' % (k, ' off' if (active is not None and active != k) else '', k) for k in (1, 2, 3, 4))
    return ('\n    <h1 class="ttl">§TTL§</h1>\n    ' + funnel(active) + '\n    ' + letters
            + '\n    <div class="colL">' + rows_l + '</div>\n    <div class="colR">' + rows_r + '</div>\n  ')


SLIDES = [slide(None), slide(1), slide(2), slide(3), slide(4)]

STR = {
    'ar': {'TTL': 'قالب نموذج أيدا بأربع خطوات', 'L1': 'A', 'L2': 'I', 'L3': 'D', 'L4': 'A',
           'H1': 'الوعي', 'H2': 'الاهتمام', 'H3': 'الرغبة', 'H4': 'الفعل', 'EDIT': 'عدّل النص هنا',
           'P': 'هذا نص توضيحي. أدخل النص المطلوب هنا.'},
    'en': {'TTL': '4-Step AIDA Model PowerPoint Template', 'L1': 'A', 'L2': 'I', 'L3': 'D', 'L4': 'A',
           'H1': 'Awareness', 'H2': 'Interest', 'H3': 'Desire', 'H4': 'Action', 'EDIT': 'Edit Text Here',
           'P': 'This is a sample text. Insert your desired text here.'},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'marketing', 'icon': '🔻',
    'name': 'نموذج أيدا بأربع خطوات', 'nameEn': '4-Step AIDA Model',
    'description': 'خمس شرائح لقمع أيدا الملوّن (الوعي، الاهتمام، الرغبة، الفعل) بعمودي نص: نظرة عامة ثم إبراز كل مرحلة',
    'descriptionEn': 'Five slides of a colored AIDA funnel (Awareness, Interest, Desire, Action) with text columns: overview, then each stage',
    'sections': [
        {'title': 'النموذج كاملًا', 'titleEn': 'Full Model'},
        {'title': 'المرحلة الأولى: الوعي', 'titleEn': 'Stage 1: Awareness'},
        {'title': 'المرحلة الثانية: الاهتمام', 'titleEn': 'Stage 2: Interest'},
        {'title': 'المرحلة الثالثة: الرغبة', 'titleEn': 'Stage 3: Desire'},
        {'title': 'المرحلة الرابعة: الفعل', 'titleEn': 'Stage 4: Action'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص فقط؛ القمع رسم SVG والصنف off يرمّد المرحلة والعنوان والنص غير المُبرزين في كل شريحة.',
}
