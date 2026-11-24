# -*- coding: utf-8 -*-
# نموذج «قمع دائري بستة مستويات»: سبع شرائح بالتخطيط نفسه — قمع حلزوني SVG من ست حلقات ثلاثية الأبعاد داخل مخروط شفاف،
# وستة عناصر (ثلاثة في كل جهة) بأيقونة وعنوان ونص. الشريحة الأولى ملوّنة كاملة، وكل شريحة تالية تُبرز حلقة واحدة وعنصرها.
KEY = 'sl-6-level-circular-funnel'
TITLE = {'ar': 'قمع دائري بستة مستويات', 'en': '6-Level Circular Funnel'}
COMMENT = 'نموذج «قمع دائري بستة مستويات»: عنوان أعلى، قمع حلزوني SVG من ست حلقات ملوّنة داخل مخروط شفاف في المنتصف، وثلاثة عناصر بأيقونة وعنوان ونص في كل جهة. الشريحة الأولى ملوّنة كاملة، والصنف on يُبرز الحلقة والعنصر في باقي الشرائح.'
VARS = '--ink: #111111; --ink-2: #222222; --accent: #2E6FD3; --gray: #A9A9A9; --bg: #FFFFFF; --c1: #F5BE35; --d1: #D89A1E; --c2: #E15B38; --d2: #B94224; --c3: #8CB63C; --d3: #6D9229; --c4: #4FA596; --d4: #3A8577; --c5: #2E6FD3; --d5: #1F52A8; --c6: #2F4A8C; --d6: #22376A;'

STYLE = """  .ttl { position: absolute; top: 40px; inset-inline-start: 65px; margin: 0; font-size: 40px; font-weight: 300; line-height: 1.2; color: var(--ink); letter-spacing: -0.2px; }
  html[lang="ar"] .ttl { font-weight: 400; }
  /* القمع: المخروط ثم الحلقات من الأسفل إلى الأعلى حتى تختفي مؤخرة كل حلقة خلف التي فوقها (حلزون) */
  .funnel { position: absolute; top: 120px; inset-inline-start: 360px; width: 560px; height: 540px; }
  html[dir="rtl"] .funnel { transform: scaleX(-1); }
  .funnel .cone { fill: url(#coneB); }
  .funnel .face { fill: var(--c); } .funnel .side { fill: var(--d); } .funnel .wall { fill: var(--d); opacity: .92; }
  .funnel .r1 { --c: var(--c1); --d: var(--d1); } .funnel .r2 { --c: var(--c2); --d: var(--d2); } .funnel .r3 { --c: var(--c3); --d: var(--d3); }
  .funnel .r4 { --c: var(--c4); --d: var(--d4); } .funnel .r5 { --c: var(--c5); --d: var(--d5); } .funnel .r6 { --c: var(--c6); --d: var(--d6); }
  /* في شرائح الإبراز: المخروط رمادي والحلقات غير المُبرزة رمادية */
  .frame:not(.s1) .funnel .cone { fill: url(#coneG); }
  .frame:not(.s1) .funnel .ring:not(.on) { --c: #D4D4D4; --d: #B6B6B6; }
  /* العناصر الستة */
  .it { position: absolute; width: 250px; color: var(--c); }
  .it .ic { height: 42px; display: flex; align-items: flex-end; }
  .it .ic svg { height: 40px; width: 40px; }
  .it h2 { margin: 12px 0 8px; font-size: 22px; font-weight: 700; line-height: 1.2; color: var(--c); }
  .it p { margin: 0; font-size: 17px; line-height: 1.38; color: var(--ink-2); }
  .it.L { text-align: end; } .it.L .ic { justify-content: flex-end; }
  .it1 { --c: var(--c2); top: 203px; inset-inline-end: 936px; }
  .it2 { --c: var(--c4); top: 368px; inset-inline-end: 870px; } .it2 .ic svg { width: 52px; }
  .it3 { --c: var(--c6); top: 532px; inset-inline-end: 792px; }
  .it4 { --c: var(--c1); top: 116px; inset-inline-start: 918px; } .it4 .ic svg { width: 48px; height: 46px; }
  .it5 { --c: #6E9A2E; top: 290px; inset-inline-start: 857px; } .it5 .ic svg { width: 46px; }
  .it6 { --c: var(--c5); top: 462px; inset-inline-start: 818px; } .it6 .ic svg { width: 48px; }
  .frame:not(.s1) .it:not(.on) { --c: var(--gray); }
  .frame:not(.s1) .it:not(.on) p { color: var(--gray); }"""

ICONS = {
    # ترسان
    'gears': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M13.6 2h2.8l.4 2.1a6.5 6.5 0 0 1 1.6.9l2-.7 1.4 2.4-1.6 1.4a6.5 6.5 0 0 1 0 1.8l1.6 1.4-1.4 2.4-2-.7a6.5 6.5 0 0 1-1.6.9l-.4 2.1h-2.8l-.4-2.1a6.5 6.5 0 0 1-1.6-.9l-2 .7-1.4-2.4 1.6-1.4a6.5 6.5 0 0 1 0-1.8L8.2 6.7l1.4-2.4 2 .7a6.5 6.5 0 0 1 1.6-.9zM15 6.3a2.7 2.7 0 1 0 0 5.4 2.7 2.7 0 0 0 0-5.4z"/><path d="M5.2 13h2l.3 1.5a4.5 4.5 0 0 1 1.1.6l1.4-.5 1 1.7-1.1 1a4.5 4.5 0 0 1 0 1.3l1.1 1-1 1.7-1.4-.5a4.5 4.5 0 0 1-1.1.6L7.2 23h-2l-.3-1.5a4.5 4.5 0 0 1-1.1-.6l-1.4.5-1-1.7 1.1-1a4.5 4.5 0 0 1 0-1.3l-1.1-1 1-1.7 1.4.5a4.5 4.5 0 0 1 1.1-.6zm1 3.2a1.8 1.8 0 1 0 0 3.6 1.8 1.8 0 0 0 0-3.6z"/></svg>',
    # شاشة ووحدة حاسوب
    'pc': '<svg viewBox="0 0 30 24" fill="currentColor" aria-hidden="true"><path d="M1.5 3h19a1.5 1.5 0 0 1 1.5 1.5v10a1.5 1.5 0 0 1-1.5 1.5H13v2.5h3.5V20h-11v-1.5H9V16H1.5A1.5 1.5 0 0 1 0 14.5v-10A1.5 1.5 0 0 1 1.5 3zm1 2v9h17V5h-17z"/><path d="M24 2h5a1 1 0 0 1 1 1v18a1 1 0 0 1-1 1h-5a1 1 0 0 1-1-1V3a1 1 0 0 1 1-1zm1 2.5v2h3v-2h-3zm0 3.5v2h3V8h-3zm1.5 8.5a1.3 1.3 0 1 0 0 2.6 1.3 1.3 0 0 0 0-2.6z"/></svg>',
    # مستند
    'doc': '<svg viewBox="0 0 22 24" fill="currentColor" aria-hidden="true"><path d="M2 0h11l7 7v15a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2V2a2 2 0 0 1 2-2zm0 2v20h16V8h-6V2H2zm3 9h12v1.8H5V11zm0 3.6h12v1.8H5v-1.8zm0 3.6h8V20H5v-1.8zM5 7h5v1.8H5V7z"/></svg>',
    # مصباح بأشعة
    'bulb': '<svg viewBox="0 0 26 26" fill="currentColor" aria-hidden="true"><path d="M13 5.5a6 6 0 0 0-3.7 10.7c.6.5 1 1.2 1.1 2h5.2c.1-.8.5-1.5 1.1-2A6 6 0 0 0 13 5.5zm0-1.6a7.6 7.6 0 0 1 4.8 13.5c-.3.3-.5.7-.5 1.1v2.8a1.5 1.5 0 0 1-1.5 1.5h-5.6a1.5 1.5 0 0 1-1.5-1.5v-2.8c0-.4-.2-.8-.5-1.1A7.6 7.6 0 0 1 13 3.9zm-2.6 15.9v1.4h5.2v-1.4h-5.2zM12.2 0h1.6v2.6h-1.6V0zM23.4 12.2H26v1.6h-2.6v-1.6zM0 12.2h2.6v1.6H0v-1.6zM20.9 3.9l1.2 1.2-1.9 1.9-1.2-1.2 1.9-1.9zM5.1 3.9 7 5.8 5.8 7 3.9 5.1l1.2-1.2zm15.6 15.6 1.9 1.9-1.2 1.2-1.9-1.9 1.2-1.2zM5.6 19.5l1.2 1.2-1.9 1.9-1.2-1.2 1.9-1.9zM11 25h4v1h-4v-1z"/></svg>',
    # مبنى بأعمدة
    'bank': '<svg viewBox="0 0 30 24" fill="currentColor" aria-hidden="true"><path d="M15 0l14 6.5V9H1V6.5L15 0zM3 10.5h3v9H3v-9zm5.5 0h3v9h-3v-9zm5 0h3v9h-3v-9zm5 0h3v9h-3v-9zm5.5 0h3v9h-3v-9zM1 21h28v3H1v-3z"/></svg>',
    # فقاعتا حوار
    'chat': '<svg viewBox="0 0 30 22" fill="currentColor" aria-hidden="true"><path d="M2 0h16a2 2 0 0 1 2 2v9a2 2 0 0 1-2 2H9l-5 4v-4H2a2 2 0 0 1-2-2V2a2 2 0 0 1 2-2z"/><path d="M22 5h6a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2h-2v4l-5-4h-5a2 2 0 0 1-2-2v-.5h4.5a3.5 3.5 0 0 0 3.5-3.5V5z"/></svg>',
}

GLOBAL_DEFS = ('<defs>'
        '<linearGradient id="coneB" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#F4F8FD" stop-opacity=".3"/><stop offset=".25" stop-color="#E4EFFA"/><stop offset="1" stop-color="#C3DBF4"/></linearGradient>'
        '<linearGradient id="coneG" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#FAFAFA" stop-opacity=".3"/><stop offset=".25" stop-color="#F2F2F2"/><stop offset="1" stop-color="#E3E3E3"/></linearGradient>'
        '</defs>')


def ell(cx, cy, rx, ry):  # مسار قطع ناقص مغلق
    return 'M%g,%g a%g,%g 0 1 0 %g,0 a%g,%g 0 1 0 %g,0 Z' % (cx - rx, cy, rx, ry, 2 * rx, rx, ry, -2 * rx)


def ring(n, cx, cy, rx, ry, rx2, ry2, d, rot, on):
    """حلقة ثلاثية الأبعاد: الجدار الجانبي الخارجي (أسفل)، الجدار الداخلي الظاهر عبر الفتحة، ثم الوجه العلوي."""
    side = 'M%g,%g A%g,%g 0 0 0 %g,%g L%g,%g A%g,%g 0 0 1 %g,%g Z' % (cx - rx, cy, rx, ry, cx + rx, cy, cx + rx, cy + d, rx, ry, cx - rx, cy + d)
    wall = ell(cx, cy, rx2, ry2) + ' ' + ell(cx, cy + d, rx2, ry2)
    face = ell(cx, cy, rx, ry) + ' ' + ell(cx, cy, rx2, ry2)
    return ('<g class="ring r%d%s" transform="rotate(%g %g %g)"><path class="side" d="%s"/><path class="wall" fill-rule="evenodd" d="%s"/><path class="face" fill-rule="evenodd" d="%s"/></g>'
            % (n, ' on' if on else '', rot, cx, cy, side, wall, face))


RINGS = [  # (cx, cy, rx, ry, rx2, ry2, عمق, ميل) — مقاسة من الصورة
    (633, 232, 225, 52, 176, 36, 28, 0),
    (630, 350, 180, 42, 150, 28, 22, -8),
    (637, 440, 128, 36, 105, 24, 18, -16),
    (641, 515, 108, 22, 92, 14, 12, -4),
    (645, 568, 92, 22, 76, 14, 14, -14),
    (630, 625, 65, 17, 52, 10, 12, -10),
]


def funnel(on):  # on: رقم الحلقة المُبرزة (None = الكل ملوّن)
    body = '<polygon class="cone" points="372,128 892,128 668,645 604,645"/>'
    # نرسم من الأسفل إلى الأعلى حتى تختفي مؤخرة كل حلقة خلف الحلقة التي فوقها
    for i in range(6, 0, -1):
        body += ring(i, *RINGS[i - 1], on == i)
    return '<svg class="funnel" viewBox="360 120 560 540" aria-hidden="true">' + body + '</svg>'


ITEM_ICON = {1: 'gears', 2: 'pc', 3: 'doc', 4: 'bulb', 5: 'bank', 6: 'chat'}


def item(n, on):
    side = 'L' if n <= 3 else 'R'
    return ('<div class="it it%d %s%s"><div class="ic">%s</div><h2>§H%d§</h2><p>§P%d§</p></div>'
            % (n, side, ' on' if on else '', ICONS[ITEM_ICON[n]], n, n))


# ترتيب الإبراز: الحلقة k ↔ العنصر: 1→4 (مصباح)، 2→1 (تروس)، 3→5 (مبنى)، 4→2 (حاسوب)، 5→6 (حوار)، 6→3 (مستند)
RING_ITEM = {1: 4, 2: 1, 3: 5, 4: 2, 5: 6, 6: 3}


def slide(on):
    it_on = RING_ITEM.get(on)
    return ('\n    <h1 class="ttl">§TTL§</h1>\n    ' + funnel(on) + '\n    '
            + '\n    '.join(item(n, n == it_on) for n in range(1, 7)) + '\n  ')


SLIDES = [slide(None)] + [slide(k) for k in range(1, 7)]

STR = {
    'ar': {'TTL': 'قالب القمع الدائري بستة مستويات',
           'H1': 'عنوان نائب', 'P1': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
           'H2': 'عنوان نائب', 'P2': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
           'H3': 'عنوان نائب', 'P3': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
           'H4': 'عنوان نائب', 'P4': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
           'H5': 'عنوان نائب', 'P5': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
           'H6': 'عنوان نائب', 'P6': 'هذا نص توضيحي. أضف النص المطلوب هنا.'},
    'en': {'TTL': '6-Level Circular Funnel PowerPoint Template',
           'H1': 'Placeholder', 'P1': 'This is a sample text. Insert your desired text here.',
           'H2': 'Placeholder', 'P2': 'This is a sample text. Insert your desired text here.',
           'H3': 'Placeholder', 'P3': 'This is a sample text. Insert your desired text here.',
           'H4': 'Placeholder', 'P4': 'This is a sample text. Insert your desired text here.',
           'H5': 'Placeholder', 'P5': 'This is a sample text. Insert your desired text here.',
           'H6': 'Placeholder', 'P6': 'This is a sample text. Insert your desired text here.'},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'sales', 'icon': '🌀',
    'name': 'قمع دائري بستة مستويات', 'nameEn': '6-Level Circular Funnel',
    'description': 'سبع شرائح لقمع حلزوني من ست حلقات ملوّنة: نظرة عامة ثم إبراز كل مستوى وعنصره على حدة',
    'descriptionEn': 'Seven slides of a 6-ring spiral funnel: overview, then each level and its item highlighted',
    'sections': [
        {'title': 'القمع كاملًا', 'titleEn': 'Full Funnel'},
        {'title': 'المستوى الأول (الأصفر)', 'titleEn': 'Level 1 (Yellow)'},
        {'title': 'المستوى الثاني (البرتقالي)', 'titleEn': 'Level 2 (Orange)'},
        {'title': 'المستوى الثالث (الأخضر)', 'titleEn': 'Level 3 (Green)'},
        {'title': 'المستوى الرابع (الأزرق المخضرّ)', 'titleEn': 'Level 4 (Teal)'},
        {'title': 'المستوى الخامس (الأزرق)', 'titleEn': 'Level 5 (Blue)'},
        {'title': 'المستوى السادس (الكحلي)', 'titleEn': 'Level 6 (Navy)'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص فقط؛ القمع رسم SVG من ست حلقات، والصنف on على الحلقة والعنصر يحدد المستوى المُبرز في كل شريحة.',
}
