# -*- coding: utf-8 -*-
# نموذج «قمع أيدا بأربع خطوات»: خمس شرائح بالتخطيط نفسه؛ الأولى بكل الألوان، والباقي يُبرز مرحلة واحدة (الوعي، الاهتمام، الرغبة، الفعل) ويرمّد الباقي.
KEY = 'sl-4-step-aida-funnel'
TITLE = {'ar': 'قمع أيدا بأربع خطوات', 'en': '4-Step AIDA Funnel'}
COMMENT = 'نموذج «قمع أيدا بأربع خطوات»: عنوان أعلى، أربعة شرائط رمادية بطرفين مدبّبين، قمع SVG بأربع مراحل زرقاء متدرجة في المنتصف بأسهم خضراء، وعلى كل شريط حرف كبير وأيقونة وعنوان ونص. الصنف off يرمّد المرحلة غير المُبرزة.'
VARS = '--ink: #282828; --ink-2: #2B2B2B; --accent: #0E4E95; --c1: #0E4E95; --c2: #1578DE; --c3: #38A1F2; --c4: #7FCBFC; --band: #F2F2F2; --g1: #BFBFBF; --g2: #D9D9D9; --gray-h: #7F7F7F; --gray-t: #8A8A8A; --bg: #FFFFFF;'

STYLE = """  .ttl { position: absolute; top: 42px; inset-inline-start: 65px; margin: 0; font-size: 40px; font-weight: 300; line-height: 1.2; color: var(--ink); }
  html[lang="ar"] .ttl { font-weight: 400; }
  /* الشرائط الرمادية بطرفين مدبّبين (شكل متماثل فلا يحتاج عكسًا) */
  .band { position: absolute; height: 86px; background: var(--band); clip-path: polygon(30px 0, calc(100% - 30px) 0, 100% 50%, calc(100% - 30px) 100%, 30px 100%, 0 50%); }
  .b1 { top: 191px; inset-inline-start: 115px; width: 1037px; }
  .b2 { top: 298px; inset-inline-start: 195px; width: 892px; }
  .b3 { top: 404px; inset-inline-start: 232px; width: 798px; }
  .b4 { top: 514px; inset-inline-start: 275px; width: 735px; }
  /* القمع: رسم SVG بإحداثيات الإطار؛ يُعكس في العربية لأنه ليس في منتصف الشريحة */
  .funnel { position: absolute; inset: 0; width: 1280px; height: 720px; }
  html[dir="rtl"] .funnel { transform: scaleX(-1); }
  .funnel .f1 { fill: var(--c1); } .funnel .f2 { fill: var(--c2); } .funnel .f3 { fill: var(--c3); } .funnel .f4 { fill: var(--c4); }
  .funnel .off1, .funnel .off3 { fill: var(--g1); } .funnel .off2, .funnel .off4 { fill: var(--g2); }
  .funnel .arrow { fill: url(#ag); }
  /* الحروف الكبيرة */
  .ltr { position: absolute; margin: 0; width: 120px; text-align: center; font-size: 69px; font-weight: 800; line-height: 1; }
  .l1 { top: 199px; inset-inline-start: 146px; color: var(--c1); }
  .l2 { top: 307px; inset-inline-start: 237px; color: var(--c2); }
  .l3 { top: 412px; inset-inline-start: 292px; color: var(--c3); }
  .l4 { top: 522px; inset-inline-start: 315px; color: var(--c4); }
  .ltr.off { color: var(--g1); }
  /* العناوين والنصوص */
  .hd { position: absolute; margin: 0; font-size: 26px; font-weight: 700; line-height: 1.2; white-space: nowrap; }
  .h1 { top: 217px; inset-inline-start: 745px; color: var(--c1); }
  .h2 { top: 324px; inset-inline-start: 674px; color: var(--c2); }
  .h3 { top: 431px; inset-inline-start: 607px; color: var(--c3); }
  .h4 { top: 541px; inset-inline-start: 594px; color: var(--c4); }
  .hd.off { color: var(--gray-h); }
  .tx { position: absolute; margin: 0; font-size: 17px; line-height: 1.2; width: 250px; color: var(--ink-2); }
  html[lang="ar"] .tx { line-height: 1.4; }
  .t1 { top: 211px; inset-inline-start: 915px; width: 147px; }
  .t2 { top: 319px; inset-inline-start: 813px; }
  .t3 { top: 425px; inset-inline-start: 743px; }
  .t4 { top: 535px; inset-inline-start: 732px; }
  .tx.off { color: var(--gray-t); }
  /* الأيقونات البيضاء داخل القمع */
  .ic { position: absolute; width: 44px; height: 44px; inset-inline-start: 490px; color: #fff; }
  .ic svg { width: 44px; height: 44px; }
  .i1 { top: 210px; } .i2 { top: 318px; } .i3 { top: 425px; } .i4 { top: 535px; }
  .ic.off { color: #8F8F8F; }"""

ICONS = {
    1: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.4" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M4 10.5v3.5l3.5 1 2.5 5 2.2-.8-1.6-4.2 6.4 2.5V5L7.5 9.5H4z"/><path d="M19.5 9.5a2.6 2.6 0 0 1 0 4.6"/><path d="M18 3l1.6-1.4M21 6.5l2-.6"/></svg>',
    2: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.4" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="11" cy="13" r="8"/><circle cx="11" cy="13" r="4.5"/><circle cx="11" cy="13" r="1" fill="currentColor"/><path d="M11 13l7.5-7.5"/><path d="M18.5 5.5h3V2.5M18.5 5.5v-3"/></svg>',
    3: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.4" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 20.5s-7.5-4.6-7.5-10.2A4.1 4.1 0 0 1 12 8a4.1 4.1 0 0 1 7.5 2.3c0 5.6-7.5 10.2-7.5 10.2z"/><path d="M15 12l.5 1.3 1.3.5-1.3.5-.5 1.3-.5-1.3-1.3-.5 1.3-.5z"/><path d="M4.5 5.5l1.5 1.5M19 3l-1.5 1.5M12 3v2"/></svg>',
    4: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.3" stroke-linejoin="round" aria-hidden="true"><path d="M16.6,8.5 L16.6,10.5 L14.5,10.8 L14.0,12.1 L15.2,13.8 L13.8,15.2 L12.1,14.0 L10.8,14.5 L10.5,16.6 L8.5,16.6 L8.2,14.5 L6.9,14.0 L5.2,15.2 L3.8,13.8 L5.0,12.1 L4.5,10.8 L2.4,10.5 L2.4,8.5 L4.5,8.2 L5.0,6.9 L3.8,5.2 L5.2,3.8 L6.9,5.0 L8.2,4.5 L8.5,2.4 L10.5,2.4 L10.8,4.5 L12.1,5.0 L13.8,3.8 L15.2,5.2 L14.0,6.9 L14.5,8.2 Z"/><circle cx="9.5" cy="9.5" r="2.4"/><path d="M22.2,16.7 L22.2,18.3 L20.7,18.4 L20.2,19.4 L21.0,20.7 L19.9,21.7 L18.7,20.6 L17.7,20.8 L17.2,22.3 L15.7,22.0 L15.9,20.4 L15.1,19.7 L13.6,20.2 L12.9,18.9 L14.2,18.0 L14.2,17.0 L12.9,16.1 L13.6,14.8 L15.1,15.3 L15.9,14.6 L15.7,13.0 L17.2,12.7 L17.7,14.2 L18.7,14.4 L19.9,13.3 L21.0,14.3 L20.2,15.6 L20.7,16.6 Z"/><circle cx="17.5" cy="17.5" r="1.4"/></svg>',
}

GLOBAL_DEFS = ('<defs><linearGradient id="ag" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#B9E2AC"/><stop offset="1" stop-color="#5DB36F"/></linearGradient></defs>')

BODY = 'M304,219 C350,266 470,400 470,500 L470,627 L553,627 L553,500 C553,400 673,266 719,219 Z'
BANDS = [(0, 290), (290, 395), (395, 497), (497, 720)]
TOP_ARROW = '-3.5,0 3.5,0 3.5,26 11,26 0,44 -11,26 -3.5,26'


def funnel(n, active):  # n رقم الشريحة (لتفرّد معرّفات القصّ)، active المرحلة المُبرزة أو None
    cls = lambda k: 'f%d' % k if active is None or active == k else 'off%d' % k
    clips = ''.join('<clipPath id="c%d_%d"><rect x="0" y="%d" width="1280" height="%d"/></clipPath>' % (n, k + 1, y0, y1 - y0) for k, (y0, y1) in enumerate(BANDS))
    stages = ''.join('<path class="%s" d="%s" clip-path="url(#c%d_%d)"/>' % (cls(k + 1), BODY, n, k + 1) for k in range(4))
    return ('<svg class="funnel" viewBox="0 0 1280 720" aria-hidden="true"><defs>%s</defs>' % clips
            + '<g class="arrow"><polygon points="%s" transform="translate(510 129)"/><polygon points="%s" transform="translate(386 131) rotate(-24)"/><polygon points="%s" transform="translate(634 131) rotate(24)"/><polygon points="496,614 530,614 530,634 552,634 513,661 474,634 496,634"/></g>' % (TOP_ARROW, TOP_ARROW, TOP_ARROW)
            + '<rect class="%s" x="266" y="170" width="492" height="49" rx="18"/>' % cls(1)
            + stages + '</svg>')


def row(k, active):
    o = ' off' if (active is not None and active != k) else ''
    return ('<p class="ltr l%d%s">§L%d§</p><div class="ic i%d%s">%s</div><h2 class="hd h%d%s">§H%d§</h2><p class="tx t%d%s">§P%d§</p>'
            % (k, o, k, k, o, ICONS[k], k, o, k, k, o, k))


def slide(n, active):
    return ('\n    <h1 class="ttl">§TTL§</h1>\n    ' + ''.join('<div class="band b%d"></div>' % k for k in (1, 2, 3, 4))
            + '\n    ' + funnel(n, active) + '\n    ' + '\n    '.join(row(k, active) for k in (1, 2, 3, 4)) + '\n  ')


SLIDES = [slide(1, None), slide(2, 1), slide(3, 2), slide(4, 3), slide(5, 4)]

STR = {
    'ar': {'TTL': 'قالب قمع أيدا', 'L1': 'A', 'L2': 'I', 'L3': 'D', 'L4': 'A',
           'H1': 'الوعي', 'H2': 'الاهتمام', 'H3': 'الرغبة', 'H4': 'الفعل',
           'P1': 'أدخل النص المطلوب هنا.',
           'P2': 'هذا نص توضيحي. أدخل النص المطلوب هنا.', 'P3': 'هذا نص توضيحي. أدخل النص المطلوب هنا.', 'P4': 'هذا نص توضيحي. أدخل النص المطلوب هنا.'},
    'en': {'TTL': 'AIDA Funnel PowerPoint Template', 'L1': 'A', 'L2': 'I', 'L3': 'D', 'L4': 'A',
           'H1': 'Awareness', 'H2': 'Interest', 'H3': 'Desire', 'H4': 'Action',
           'P1': 'Insert your desired text here.',
           'P2': 'This is a sample text. Insert your desired text here.', 'P3': 'This is a sample text. Insert your desired text here.', 'P4': 'This is a sample text. Insert your desired text here.'},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'sales', 'icon': '🔽',
    'name': 'قمع أيدا بأربع خطوات', 'nameEn': '4-Step AIDA Funnel',
    'description': 'خمس شرائح لقمع أيدا (الوعي، الاهتمام، الرغبة، الفعل): نظرة عامة ثم إبراز كل مرحلة على حدة',
    'descriptionEn': 'Five slides of the AIDA funnel (Awareness, Interest, Desire, Action): overview, then each stage highlighted',
    'sections': [
        {'title': 'القمع كاملًا', 'titleEn': 'Full Funnel'},
        {'title': 'المرحلة الأولى: الوعي', 'titleEn': 'Stage 1: Awareness'},
        {'title': 'المرحلة الثانية: الاهتمام', 'titleEn': 'Stage 2: Interest'},
        {'title': 'المرحلة الثالثة: الرغبة', 'titleEn': 'Stage 3: Desire'},
        {'title': 'المرحلة الرابعة: الفعل', 'titleEn': 'Stage 4: Action'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص فقط؛ القمع رسم SVG والصنف off يرمّد المرحلة والصف غير المُبرزين في كل شريحة.',
}
