# -*- coding: utf-8 -*-
# نموذج «أكوال» (جميع الأرباع، جميع المستويات): خمس شرائح بالتخطيط نفسه — شبكة أربعة أرباع ملوّنة في المنتصف
# بأيقونة خطية وعنوان، وذيل سهمي صغير يخرج من كل ربع نحو نص جانبي، ومحاور (فردي/جماعي أعلى وأسفل، داخلي/خارجي
# على الجانبين). الشريحة الأولى كاملة الألوان، وكل شريحة تالية تُبرز ربعًا واحدًا وتُرمّد الباقي.
KEY = 'sl-aqal-model'
TITLE = {'ar': 'نموذج أكوال', 'en': 'AQAL Model'}
COMMENT = 'نموذج «أكوال»: عنوان وعنوان فرعي مائل أعلى، شبكة 2×2 من أرباع ملوّنة (أزرق، أخضر، رمادي، برتقالي) بأيقونة خطية وعنوان أبيض، ذيل سهمي من كل ربع إلى نص جانبي، ومحاور فردي/جماعي وداخلي/خارجي. الصنف off يرمّد الربع والنص الجانبي؛ الذيول تُعكس أفقيًا في العربية.'
VARS = '--ink: #111111; --ink-2: #585858; --muted: #595959; --dim: #BFBFBF; --accent: #0070C6; --blue: #0070C6; --green: #8BC43F; --gray: #A6A6A6; --orange: #FEAA00; --off: #F2F2F2; --bg: #FFFFFF;'

STYLE = """  .ttl { position: absolute; top: 42px; inset-inline-start: 64px; margin: 0; font-size: 40px; font-weight: 400; line-height: 1.2; color: var(--ink); letter-spacing: -0.2px; }
  .sub { position: absolute; top: 103px; inset-inline-start: 66px; margin: 0; font-size: 22px; font-weight: 400; font-style: italic; line-height: 1.3; color: var(--ink); }
  html[lang="ar"] .sub { font-style: normal; font-weight: 500; }
  /* المحاور */
  .ax { position: absolute; margin: 0; font-size: 18px; font-weight: 400; line-height: 1.2; color: var(--muted); text-align: center; letter-spacing: .3px; }
  .ax-top { top: 141px; inset-inline-start: 0; width: 1280px; }
  .ax-bot { top: 638px; inset-inline-start: 0; width: 1280px; }
  .ax-side { top: 389px; width: 120px; }
  .ax-start { inset-inline-start: 272px; transform: rotate(-90deg); }
  .ax-end { inset-inline-start: 892px; transform: rotate(90deg); }
  html[dir="rtl"] .ax-start { transform: rotate(90deg); }
  html[dir="rtl"] .ax-end { transform: rotate(-90deg); }
  /* الأرباع */
  .q { position: absolute; width: 270px; height: 210px; background: var(--c); color: #fff; }
  .q.off { background: var(--off); color: var(--ink); }
  .q1 { top: 188px; inset-inline-start: 368px; } .q2 { top: 188px; inset-inline-start: 642px; }
  .q3 { top: 403px; inset-inline-start: 368px; } .q4 { top: 403px; inset-inline-start: 642px; }
  .q .tail { position: absolute; top: 22px; width: 21px; height: 44px; background: var(--c); }
  .q.off .tail { background: var(--off); }
  .q1 .tail, .q3 .tail { inset-inline-end: 100%; clip-path: polygon(100% 0, 0 50%, 100% 100%); }
  .q2 .tail, .q4 .tail { inset-inline-start: 100%; clip-path: polygon(0 0, 100% 50%, 0 100%); }
  html[dir="rtl"] .q .tail { transform: scaleX(-1); }
  .q .ic { position: absolute; top: 40px; inset-inline-start: 0; width: 100%; height: 76px; display: grid; place-items: center; }
  .q .ic svg { width: 76px; height: 76px; }
  .q h2 { position: absolute; top: 139px; inset-inline-start: 0; width: 100%; margin: 0; text-align: center; font-size: 24px; font-weight: 700; line-height: 1.2; letter-spacing: .4px; }
  html[lang="ar"] .q h2 { font-weight: 800; letter-spacing: 0; }
  /* النصوص الجانبية */
  .st { position: absolute; width: 176px; margin: 0; font-size: 17px; line-height: 1.33; color: var(--ink-2); }
  .st.off { color: var(--dim); }
  .st1 { top: 192px; inset-inline-start: 131px; text-align: end; } .st2 { top: 192px; inset-inline-start: 977px; }
  .st3 { top: 538px; inset-inline-start: 131px; text-align: end; } .st4 { top: 538px; inset-inline-start: 977px; }
  .foot { position: absolute; top: 684px; inset-inline-start: 20px; margin: 0; font-size: 17px; font-style: italic; line-height: 1.3; color: var(--ink); }
  html[lang="ar"] .foot { font-style: normal; }"""

# أيقونات خطية بيضاء (أو بلون الحبر في الربع المُرمَّد)
ICONS = {
    'target': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.1" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="11" cy="13" r="8.5"/><circle cx="11" cy="13" r="5.3"/><circle cx="11" cy="13" r="2.2"/><path d="M11 13l7.5-7.5"/><path d="M18.5 5.5V3l2.2 2.2-2.2.3zM18.5 5.5H21l-2.2-2.2"/></svg>',
    'head': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.1" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M8.5 21.5v-3.2H6.6a1 1 0 0 1-1-1V15H4.2l1.6-3.3V10a7 7 0 0 1 7-7.5c3.9 0 7 3.1 7 7 0 2.1-.9 4-2.4 5.3V21.5"/><circle cx="12.6" cy="9.3" r="2.2"/><path d="M12.6 6.4v.9M12.6 11.3v.9M9.7 9.3h.9M14.6 9.3h.9M10.6 7.3l.6.6M14 10.7l.6.6M10.6 11.3l.6-.6M14 7.9l.6-.6"/><circle cx="9.3" cy="13.2" r="1.5"/><path d="M9.3 11.2v.6M9.3 14.6v.6M7.3 13.2h.6M10.7 13.2h.6"/></svg>',
    'lotus': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.1" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 19.5c-2.6-2.2-3.8-5-3.8-8.3 0-2.4 1.4-4.6 3.8-6.2 2.4 1.6 3.8 3.8 3.8 6.2 0 3.3-1.2 6.1-3.8 8.3z"/><path d="M12 19.5c-3.4-.3-6.4-2.3-8-5.4 1.7-1 3.9-1.3 5.7-.8"/><path d="M12 19.5c3.4-.3 6.4-2.3 8-5.4-1.7-1-3.9-1.3-5.7-.8"/><path d="M8.3 11.3c-1.6-1.6-3.6-2.5-5.6-2.3.2 2.6 1.3 5 3 6.7"/><path d="M15.7 11.3c1.6-1.6 3.6-2.5 5.6-2.3-.2 2.6-1.3 5-3 6.7"/><path d="M2 20.3h20"/></svg>',
    'laptop': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.1" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="4.5" y="5" width="15" height="10.5" rx=".6"/><path d="M2 17.8h20v.8a1.2 1.2 0 0 1-1.2 1.2H3.2A1.2 1.2 0 0 1 2 18.6z"/><path d="M4.5 15.5L3 17.8M19.5 15.5l1.5 2.3"/><circle cx="12" cy="10.2" r="3.2"/><path d="M8.8 10.2h6.4M12 7v6.4M10 8c.9 1.3 3.1 1.3 4 0M10 12.4c.9-1.3 3.1-1.3 4 0"/></svg>',
}

QUADS = [  # (الرقم، اللون، الأيقونة)
    (1, 'var(--blue)', 'target'), (2, 'var(--green)', 'head'), (3, 'var(--gray)', 'lotus'), (4, 'var(--orange)', 'laptop'),
]


def quad(n, color, icon, on):
    return ('<div class="q q%d%s" style="--c:%s"><span class="tail"></span><div class="ic">%s</div><h2>§Q%d§</h2></div>'
            % (n, '' if on else ' off', color, ICONS[icon], n))


def slide(on, foot=False):  # on: مجموعة الأرباع المُبرزة؛ 1 أعلى البداية، 2 أعلى النهاية، 3 أسفل البداية، 4 أسفل النهاية
    h = ['\n    <h1 class="ttl">§TTL§</h1>', '<p class="sub">§SUB§</p>',
         '<p class="ax ax-top">§AX_TOP§</p>', '<p class="ax ax-bot">§AX_BOT§</p>',
         '<p class="ax ax-side ax-start">§AX_START§</p>', '<p class="ax ax-side ax-end">§AX_END§</p>']
    for n, c, ic in QUADS:
        h.append(quad(n, c, ic, n in on))
    for n in (1, 2, 3, 4):
        h.append('<p class="st st%d%s">§S%d§</p>' % (n, '' if n in on else ' off', n))
    if foot:
        h.append('<p class="foot">§FOOT§</p>')
    return '\n    '.join(h) + '\n  '


SLIDES = [slide({1, 2, 3, 4}, foot=True), slide({1}), slide({2}), slide({4}), slide({3})]

SAMPLE = {'ar': 'هذا نص توضيحي. أضف النص المطلوب هنا.', 'en': 'This is a sample text. Insert your desired text here.'}
STR = {
    'ar': {'TTL': 'قالب نموذج أكوال', 'SUB': 'جميع الأرباع، جميع المستويات',
           'AX_TOP': 'الفردي', 'AX_BOT': 'الجماعي', 'AX_START': 'الداخلي', 'AX_END': 'الخارجي',
           'Q1': 'القصدي', 'Q2': 'السلوكي', 'Q3': 'الثقافي', 'Q4': 'الأنظمة',
           'S1': SAMPLE['ar'], 'S2': SAMPLE['ar'], 'S3': SAMPLE['ar'], 'S4': SAMPLE['ar'],
           'FOOT': 'مبنيّ على إطار النظرية التكاملية لكين ويلبر – قالب شريحة عرض النموذج التكاملي'},
    'en': {'TTL': 'AQAL Model PowerPoint Template', 'SUB': 'All Quadrants, All Levels',
           'AX_TOP': 'INDIVIDUAL', 'AX_BOT': 'COLLECTIVE', 'AX_START': 'INTERIOR', 'AX_END': 'EXTERIOR',
           'Q1': 'INTENTIONAL', 'Q2': 'BEHAVIORAL', 'Q3': 'CULTURAL', 'Q4': 'SYSTEMS',
           'S1': SAMPLE['en'], 'S2': SAMPLE['en'], 'S3': SAMPLE['en'], 'S4': SAMPLE['en'],
           'FOOT': 'Based on Ken Wilber’s Integral Theory Framework – Integral Model Presentation Slide Template'},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'mgmt', 'icon': '🧭',
    'name': 'نموذج أكوال', 'nameEn': 'AQAL Model',
    'description': 'خمس شرائح لنموذج أكوال رباعي الأرباع: نظرة عامة ثم إبراز كل ربع (القصدي، السلوكي، الأنظمة، الثقافي)',
    'descriptionEn': 'Five slides of the four-quadrant AQAL model: overview, then each quadrant highlighted',
    'sections': [
        {'title': 'الأرباع الأربعة', 'titleEn': 'All Four Quadrants'},
        {'title': 'الربع القصدي', 'titleEn': 'Intentional Quadrant'},
        {'title': 'الربع السلوكي', 'titleEn': 'Behavioral Quadrant'},
        {'title': 'ربع الأنظمة', 'titleEn': 'Systems Quadrant'},
        {'title': 'الربع الثقافي', 'titleEn': 'Cultural Quadrant'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص فقط؛ الأرباع كتل CSS بالصنف off للمُرمَّد في كل شريحة، والأيقونات SVG مضمّنة.',
}
