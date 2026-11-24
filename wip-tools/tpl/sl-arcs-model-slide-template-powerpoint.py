# -*- coding: utf-8 -*-
# نموذج «شريحة نموذج آركس»: أربعة أعمدة (الانتباه، الصلة، الثقة، الرضا)، لكل عمود عنوان ملوّن وبطاقة بإطار
# مدوّر وشارة مثلثة بالحرف وأيقونة وثلاث نقاط ونص أسفلها. الشريحة الأولى كل الأعمدة ملوّنة،
# والأربع التالية تُبرز عمودًا واحدًا (الصنف on) وتجعل الباقي رماديًا.
KEY = 'sl-arcs-model-slide-template-powerpoint'
TITLE = {'ar': 'شريحة نموذج آركس', 'en': 'ARCS Model Slide Template'}
COMMENT = 'نموذج «شريحة نموذج آركس»: أربعة أعمدة بألوان ثابتة (سماوي A، أخضر R، أحمر C، برتقالي S): عنوان كبير، بطاقة بإطار 4px مدوّر الزاويتين وشارة مثلثة متدرّجة بالحرف، أيقونة خطية، ثلاث نقاط بدوائر تحقق وفواصل، ونص بين خطين رماديين. العمود بلا الصنف on رمادي بالكامل. الشارة المثلثة تنعكس للعربية.'
VARS = '--ink: #111111; --ink-2: #333333; --accent: #39C1EC; --gray: #D9D9D9; --gray-t: #E0E0E0; --gray-l: #A6A6A6; --bg: #FFFFFF; --gd: 50deg;'

STYLE = """  html[dir="rtl"] { --gd: -50deg; }
  .ttl { position: absolute; top: 38px; inset-inline-start: 64px; margin: 0; font-size: 42px; font-weight: 400; line-height: 1.3; color: var(--ink); }
  html[lang="ar"] .ttl { font-weight: 500; }
  .cl { position: absolute; top: 0; width: 307px; height: 720px; }
  .cl.on { z-index: 2; }
  .c1 { inset-inline-start: 64px; --c: #36BDEA; --g1: #3FC9F2; --g2: #0B97CE; }
  .c2 { inset-inline-start: 346px; --c: #5AAF2E; --g1: #67BC34; --g2: #3A7C1C; }
  .c3 { inset-inline-start: 628px; --c: #CB2534; --g1: #D52C3B; --g2: #951A22; }
  .c4 { inset-inline-start: 910px; --c: #F09A1B; --g1: #F4A51F; --g2: #BF750A; }
  /* العمود غير المُبرز: رمادي بالكامل */
  .cl:not(.on) { --c: var(--gray); --g1: #F4F4F4; --g2: #D4D4D4; }
  .cl .rule { position: absolute; inset-inline-start: 0; width: 242px; height: 0; border-top: 1px solid var(--gray); }
  .c4 .rule { width: 307px; }
  .r1 { top: 127px; } .r2 { top: 657px; }
  .cl h2 { position: absolute; top: 150px; inset-inline-start: 0; margin: 0; font-size: 31px; font-weight: 700; line-height: 44px; color: var(--c); white-space: nowrap; }
  .cl:not(.on) h2 { color: var(--gray-t); }
  html[lang="ar"] .cl h2 { font-size: 34px; }
  /* الإطار: الجزء العلوي (الأعلى + الأيسر) والسفلي (الأيسر + الأسفل) بينهما فجوة */
  .fr1 { position: absolute; top: 214px; inset-inline-start: 0; width: 305px; height: 156px; border-top: 4px solid var(--c); border-inline-start: 4px solid var(--c); border-start-start-radius: 32px; }
  .fr2 { position: absolute; top: 392px; inset-inline-start: 0; width: 239px; height: 157px; border-bottom: 4px solid var(--c); border-inline-start: 4px solid var(--c); border-end-start-radius: 30px; }
  .badge { position: absolute; top: 214px; inset-inline-start: 0; width: 157px; height: 156px; border-start-start-radius: 32px; background: linear-gradient(var(--gd), var(--g1) 20%, var(--g2) 100%); clip-path: polygon(0 0, 100% 0, 0 100%); }
  html[dir="rtl"] .badge { clip-path: polygon(100% 0, 0 0, 100% 100%); }
  .ltr { position: absolute; top: 224px; inset-inline-start: 24px; margin: 0; font-size: 64px; font-weight: 700; line-height: 84px; color: #fff; }
  .cl:not(.on) .ltr { color: var(--gray-l); }
  .ic { position: absolute; top: 258px; inset-inline-start: 140px; width: 78px; height: 78px; color: var(--c); }
  .cl:not(.on) .ic { color: #CCCCCC; }
  .ic svg { width: 100%; height: 100%; display: block; }
  .pts { position: absolute; top: 372px; inset-inline-start: 36px; width: 210px; margin: 0; padding: 0; list-style: none; border-top: 1px solid #E7E7E7; }
  .pts li { position: relative; height: 46px; border-bottom: 1px solid #E7E7E7; padding-inline-start: 35px; font-size: 18px; line-height: 45px; color: var(--ink-2); }
  .pts .ck { position: absolute; top: 12px; inset-inline-start: 0; width: 21px; height: 21px; border-radius: 50%; background: var(--c); color: #fff; display: grid; place-items: center; }
  .pts .ck svg { width: 12px; height: 12px; }
  .tx { position: absolute; top: 578px; inset-inline-start: 0; width: 240px; margin: 0; font-size: 18px; line-height: 23px; color: var(--ink-2); }
  html[lang="ar"] .tx { line-height: 28px; top: 574px; }"""

ICONS = {
    'speaker': '<svg viewBox="0 0 78 78" fill="none" stroke="currentColor" stroke-width="2" stroke-linejoin="round" stroke-linecap="round" aria-hidden="true"><path d="M10 27h14L50 7v65L24 52H10z"/><path d="M24 27v25"/><path d="M60 34c2.5 3 2.5 8 0 11"/></svg>',
    'atom': '<svg viewBox="0 0 78 78" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true"><ellipse cx="39" cy="39" rx="35" ry="13"/><ellipse cx="39" cy="39" rx="35" ry="13" transform="rotate(60 39 39)"/><ellipse cx="39" cy="39" rx="35" ry="13" transform="rotate(-60 39 39)"/><circle cx="39" cy="39" r="5"/></svg>',
    'jump': '<svg viewBox="0 0 78 78" fill="none" stroke="currentColor" stroke-width="2" stroke-linejoin="round" stroke-linecap="round" aria-hidden="true"><circle cx="31" cy="21" r="4"/><path d="M28 27l-6 14 7 1-3 15-6 9h4l6-9 3-10 5 9 1 10h4l-1-12-5-9 2-9 3 4 7-15-3-1-6 11-5-9z"/><path d="M48 9l1.2 2.5 2.8.4-2 2 .5 2.8-2.5-1.3-2.5 1.3.5-2.8-2-2 2.8-.4z" fill="currentColor"/><path d="M41 7l.7 1.4M54 12l1.5-.3M45 2.5v1.5"/></svg>',
    'smile': '<svg viewBox="0 0 78 78" fill="none" stroke="currentColor" stroke-width="2" stroke-linejoin="round" stroke-linecap="round" aria-hidden="true"><circle cx="39" cy="39" r="36"/><path d="M28 21l2.6 5.3 5.8.8-4.2 4.1 1 5.8-5.2-2.7-5.2 2.7 1-5.8-4.2-4.1 5.8-.8z"/><path d="M50 21l2.6 5.3 5.8.8-4.2 4.1 1 5.8-5.2-2.7-5.2 2.7 1-5.8-4.2-4.1 5.8-.8z"/><path d="M20 45h38c-2 9-9.6 14-19 14s-17-5-19-14z"/></svg>',
}
CK = '<span class="ck"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3.4" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12.5l4.5 4.5L19 7.5"/></svg></span>'
COLS = [('A', 'speaker'), ('R', 'atom'), ('C', 'jump'), ('S', 'smile')]


def col(i, on):
    letter, icon = COLS[i - 1]
    return ('<div class="cl c%d%s"><div class="rule r1"></div><h2>§N%d§</h2>'
            '<div class="fr1"></div><div class="fr2"></div><div class="badge"></div>'
            '<p class="ltr">%s</p><div class="ic">%s</div>'
            '<ul class="pts"><li>@CK@§B%d1§</li><li>@CK@§B%d2§</li><li>@CK@§B%d3§</li></ul>'
            '<p class="tx">§TX§</p><div class="rule r2"></div></div>'
            % (i, ' on' if on else '', i, letter, ICONS[icon], i, i, i)).replace('@CK@', CK)


def slide(on):
    return '\n    <h1 class="ttl">§TTL§</h1>\n    ' + '\n    '.join(col(i, i in on) for i in (1, 2, 3, 4)) + '\n  '


SLIDES = [slide({1, 2, 3, 4}), slide({1}), slide({2}), slide({3}), slide({4})]

STR = {
    'ar': {'TTL': 'قالب شريحة نموذج آركس', 'N1': 'الانتباه', 'N2': 'الصلة', 'N3': 'الثقة', 'N4': 'الرضا',
           'B11': 'الإشراك', 'B12': 'الإرشاد', 'B13': 'التفعيل',
           'B21': 'الإرشاد', 'B22': 'الافتراضات', 'B23': 'المعرفة',
           'B31': 'التوجيه', 'B32': 'الأداء', 'B33': 'التغذية الراجعة',
           'B41': 'التعزيز', 'B42': 'التقدير', 'B43': 'التلخيص',
           'TX': 'هذا نص توضيحي. أضف النص المطلوب هنا'},
    'en': {'TTL': 'ARCS Model Slide Template', 'N1': 'ATTENTION', 'N2': 'RELEVANCE', 'N3': 'CONFIDENCE', 'N4': 'SATISFACTION',
           'B11': 'Engage', 'B12': 'Signpost', 'B13': 'Activate',
           'B21': 'Signpost', 'B22': 'Assumptions', 'B23': 'Knowledge',
           'B31': 'Guidance', 'B32': 'Performance', 'B33': 'Feedback',
           'B41': 'Reinforce', 'B42': 'Acknowledge', 'B43': 'Summarize',
           'TX': 'This is a sample text. Insert your desired text here'},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'hr', 'icon': '🎯',
    'name': 'شريحة نموذج آركس', 'nameEn': 'ARCS Model Slide',
    'description': 'خمس شرائح لنموذج آركس التحفيزي: الأعمدة الأربعة معًا ثم إبراز الانتباه والصلة والثقة والرضا تباعًا',
    'descriptionEn': 'Five ARCS model slides: all four columns, then Attention, Relevance, Confidence and Satisfaction highlighted',
    'sections': [
        {'title': 'نموذج آركس كاملًا', 'titleEn': 'Full ARCS Model'},
        {'title': 'الانتباه', 'titleEn': 'Attention'},
        {'title': 'الصلة', 'titleEn': 'Relevance'},
        {'title': 'الثقة', 'titleEn': 'Confidence'},
        {'title': 'الرضا', 'titleEn': 'Satisfaction'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص فقط؛ الصنف on على العمود يحدد العمود الملوّن في كل شريحة والباقي رمادي.',
}

FONTS = {'ar': '<link href="https://fonts.googleapis.com/css2?family=Tajawal:wght@400;500;700;800&display=swap" rel="stylesheet">',
         'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@400;600;700;800&display=swap" rel="stylesheet">'}
FONT_STACK = {'ar': "'Tajawal', 'Segoe UI', Tahoma, sans-serif", 'en': "'Open Sans', 'Segoe UI', Arial, sans-serif"}
