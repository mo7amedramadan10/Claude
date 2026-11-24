# -*- coding: utf-8 -*-
# نموذج «إنفوجرافيك إدارة النزاعات»: شريحة واحدة بمشهد رجلَي أعمال متنازعَين في المنتصف وأربع بطاقات أساليب بأسهم نحوه، وشريط نص وثلاثة عناصر سفلية.
KEY = 'sl-conflict-management-infographic-slide'
TITLE = {'ar': 'إنفوجرافيك إدارة النزاعات', 'en': 'Conflict Management Infographic Slide Template'}
COMMENT = 'نموذج «إنفوجرافيك إدارة النزاعات»: عنوان أعلى، مشهد مقصوص من الأصل في المنتصف (يُعكس في العربية)، أربع بطاقات متدرجة (التواصل، التفاوض، الوساطة، التسوية) بفقاعة أيقونة وظل سفلي وسهم نحو المشهد، شريط رمادي بعنوان، وثلاثة عناصر بأيقونة دائرية.'
VARS = ('--ink: #000000; --ink-2: #404040; --accent: #3F7BC7; --bg: #FFFFFF; --gray: #F2F2F2; --band-ink: #7F7F7F;'
        ' --c1: #2F5A99; --c1b: #4A80C8; --c2: #3784D2; --c2b: #74A9DF; --c3: #D06A0E; --c3b: #F0962E; --c4: #C84414; --c4b: #EE7444;')

FONTS = {'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@400;600;700&display=swap" rel="stylesheet">',
         'ar': '<link href="https://fonts.googleapis.com/css2?family=Tajawal:wght@400;500;700;800&display=swap" rel="stylesheet">'}
FONT_STACK = {'en': "'Open Sans', 'Segoe UI', Arial, sans-serif"}

STYLE = """  .ttl { position: absolute; top: 38px; inset-inline-start: 64px; margin: 0; font-size: 42px; font-weight: 400; line-height: 1.3; color: var(--ink); white-space: nowrap; letter-spacing: -0.3px; }
  html[lang="ar"] .ttl { font-size: 44px; top: 34px; font-weight: 500; letter-spacing: 0; }
  .scene { position: absolute; top: 100px; inset-inline-start: 400px; width: 478px; height: 412px; z-index: 2; }
  html[dir="rtl"] .scene { transform: scaleX(-1); }
  /* بطاقة أسلوب: ظل سفلي رمادي + جسم متدرج + فقاعة أيقونة فوقه */
  .cd { position: absolute; width: 190px; height: 123px; }
  .cd .sh { position: absolute; inset: 0; border-radius: 20px; background: var(--gray); }
  .cd .bx { position: absolute; top: 0; inset-inline: 0; height: 99px; border-radius: 18px; display: flex; align-items: flex-end; justify-content: center; padding-bottom: 22px; }
  .cd .bx span { color: #fff; font-size: 17px; font-weight: 700; letter-spacing: 0.1px; }
  html[lang="ar"] .cd .bx span { font-size: 20px; padding-bottom: 0; }
  .cd .bb { position: absolute; top: -56px; inset-inline-start: 41px; width: 108px; height: 88px; border-radius: 54px 54px 30px 30px; display: grid; place-items: center; }
  .cd .bb svg { width: 60px; height: 60px; }
  .cd1 { top: 160px; inset-inline-start: 96px; } .cd1 .bx { background: linear-gradient(180deg, var(--c1b), var(--c1)); } .cd1 .bb { background: #DAE5F1; color: #3A6FB4; }
  .cd2 { top: 370px; inset-inline-start: 96px; } .cd2 .bx { background: linear-gradient(180deg, var(--c2b), var(--c2)); } .cd2 .bb { background: #DCEBFA; color: #3F7FCD; }
  .cd3 { top: 160px; inset-inline-start: 991px; } .cd3 .bx { background: linear-gradient(180deg, var(--c3b), var(--c3)); } .cd3 .bb { background: #FCE8D3; color: #DA7A1E; }
  .cd4 { top: 370px; inset-inline-start: 991px; } .cd4 .bx { background: linear-gradient(180deg, var(--c4b), var(--c4)); } .cd4 .bb { background: #FBE0D5; color: #D4552A; }
  /* الأسهم (للنسخة الإنجليزية؛ تُعكس في العربية) */
  .arr { position: absolute; top: 160px; inset-inline-start: 0; width: 1280px; height: 300px; z-index: 1; }
  html[dir="rtl"] .arr { transform: scaleX(-1); }
  /* الشريط والعناصر السفلية */
  .band { position: absolute; top: 506px; inset-inline-start: 0; width: 1280px; height: 56px; background: var(--gray); display: grid; place-items: center; margin: 0; font-size: 25px; font-weight: 700; color: var(--band-ink); }
  .it { position: absolute; top: 586px; width: 260px; }
  .it .hd { display: flex; align-items: center; gap: 12px; margin: 0; }
  .it .ci { flex: none; width: 29px; height: 29px; border-radius: 50%; display: grid; place-items: center; color: #fff; }
  .it .ci svg { width: 17px; height: 17px; }
  .it h2 { margin: 0; font-size: 20px; font-weight: 700; line-height: 1.3; }
  .it p { margin: 8px 0 0; padding-inline-start: 38px; font-size: 17px; line-height: 1.38; color: var(--ink-2); max-width: 270px; }
  html[lang="ar"] .it p { font-size: 17px; line-height: 1.5; }
  .it1 { inset-inline-start: 83px; } .it1 .ci { background: #EE6A3A; } .it1 h2 { color: #E2643A; }
  .it2 { inset-inline-start: 506px; } .it2 .ci { background: #3F7BC7; } .it2 h2 { color: #3F7BC7; }
  .it3 { inset-inline-start: 928px; } .it3 .ci { background: #F29A3B; } .it3 h2 { color: #EF9535; }"""

ICONS = {
    'comm': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M8.5 1.5h7a1.5 1.5 0 0 1 1.5 1.5v3.6a1.5 1.5 0 0 1-1.5 1.5h-3.2L10 9.8V8.1H8.5A1.5 1.5 0 0 1 7 6.6V3a1.5 1.5 0 0 1 1.5-1.5z"/><circle cx="9.9" cy="4.8" r=".75" fill="#fff"/><circle cx="12" cy="4.8" r=".75" fill="#fff"/><circle cx="14.1" cy="4.8" r=".75" fill="#fff"/><circle cx="4.5" cy="12.6" r="2.3"/><path d="M.6 20.5c0-2.6 1.7-4.4 3.9-4.4s3.9 1.8 3.9 4.4z"/><circle cx="19.5" cy="12.6" r="2.3"/><path d="M15.6 20.5c0-2.6 1.7-4.4 3.9-4.4s3.9 1.8 3.9 4.4z"/><path d="M9 18.3h5.2v-1l2 1.5-2 1.5v-1H9z"/></svg>',
    'nego': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><circle cx="12" cy="4" r="2.4"/><path d="M7.8 11.2c0-2.6 1.9-4.3 4.2-4.3s4.2 1.7 4.2 4.3z"/><path d="M3 13.4l2.6-1.1 2.6 1.1v.9H3z"/><circle cx="5.6" cy="16.1" r="1.9"/><path d="M1.8 23c0-2.6 1.7-4.3 3.8-4.3s3.8 1.7 3.8 4.3z"/><path d="M15.8 13.4l2.6-1.1 2.6 1.1v.9h-5.2z"/><circle cx="18.4" cy="16.1" r="1.9"/><path d="M14.6 23c0-2.6 1.7-4.3 3.8-4.3s3.8 1.7 3.8 4.3z"/></svg>',
    'medi': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><circle cx="12" cy="3.4" r="2.2"/><path d="M8.2 10c0-2.4 1.7-4 3.8-4s3.8 1.6 3.8 4z"/><circle cx="4.6" cy="14.2" r="2.2"/><path d="M.8 21c0-2.4 1.7-4 3.8-4s3.8 1.6 3.8 4z"/><circle cx="19.4" cy="14.2" r="2.2"/><path d="M15.6 21c0-2.4 1.7-4 3.8-4s3.8 1.6 3.8 4z"/><path d="M9.8 17.6h4.4l-1 .9h-2.4z" opacity=".75"/></svg>',
    'comp': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M.5 8.2l2.4-1.4 3.4 6.1-2.3 1.4z"/><path d="M23.5 8.2l-2.4-1.4-3.4 6.1 2.3 1.4z"/><path d="M6.9 8.6l3.6-1.4c.8-.3 1.6-.2 2.3.2l1.2.7-3 2.2c-.6.4-.6 1.3.1 1.7.4.3 1 .2 1.4-.1l2.4-1.8 3 4.6-1 1-.2.1c.4.5.3 1.2-.2 1.6-.5.3-1.1.2-1.4-.2l-.1.1c.3.5.2 1.1-.3 1.4-.5.3-1.1.2-1.4-.3l-.2.1c.2.5 0 1-.5 1.3-.5.2-1-.1-1.2-.5l-1.6-1.6c-.4-.4-1-.4-1.4 0l-.1.1L6.4 14z"/><path d="M17.1 8.6l-2.6-.9-.5.3 3.8 5.8 1-.6z"/></svg>',
    'user': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><circle cx="12" cy="7.5" r="4"/><path d="M4 21c0-4.4 3.6-7 8-7s8 2.6 8 7z"/></svg>',
}

# الأسهم من حافة البطاقة نحو المشهد بإحداثيات الشريحة (الجزء من y=160)
ARROWS = ('<svg class="arr" viewBox="0 160 1280 300" aria-hidden="true">'
          '<line x1="286" y1="226" x2="384" y2="226" stroke="#35639E" stroke-width="6"/><polygon points="398,226 380,215 380,237" fill="#35639E"/>'
          '<line x1="286" y1="404" x2="383" y2="376" stroke="#6F9DD3" stroke-width="6"/><polygon points="397,372 377,366 383,387" fill="#6F9DD3"/>'
          '<line x1="991" y1="226" x2="895" y2="226" stroke="#D27613" stroke-width="6"/><polygon points="880,226 898,215 898,237" fill="#D27613"/>'
          '<line x1="991" y1="404" x2="896" y2="376" stroke="#D85A2A" stroke-width="6"/><polygon points="882,372 896,387 902,366" fill="#D85A2A"/>'
          '</svg>')


def card(n, icon):
    return ('<div class="cd cd%d"><span class="sh"></span><div class="bx"><span>§C%d§</span></div><div class="bb">%s</div></div>'
            % (n, n, ICONS[icon]))


def item(n):
    return ('<div class="it it%d"><div class="hd"><span class="ci">%s</span><h2>§H%d§</h2></div><p>§P%d§</p></div>'
            % (n, ICONS['user'], n, n))


SLIDES = ['\n    <h1 class="ttl">§TTL§</h1>\n    ' + ARROWS
          + '\n    <img class="scene" src="/templates/freeform/sl-conflict-management-infographic-slide/assets/scene.jpg" alt="§ALT§">'
          + '\n    ' + card(1, 'comm') + '\n    ' + card(2, 'nego') + '\n    ' + card(3, 'medi') + '\n    ' + card(4, 'comp')
          + '\n    <p class="band">§BAND§</p>\n    ' + item(1) + '\n    ' + item(2) + '\n    ' + item(3) + '\n  ']

STR = {
    'ar': {'TTL': 'قالب شريحة إنفوجرافيك إدارة النزاعات',
           'ALT': 'رسم توضيحي لرجلَي أعمال متنازعَين بينهما صاعقة وميزان عدل',
           'C1': 'التواصل', 'C2': 'التفاوض', 'C3': 'الوساطة', 'C4': 'التسوية',
           'BAND': 'نصك هنا',
           'H1': 'عنوان نائب', 'P1': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
           'H2': 'عنوان نائب', 'P2': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
           'H3': 'عنوان نائب', 'P3': 'هذا نص توضيحي. أضف النص المطلوب هنا.'},
    'en': {'TTL': 'Conflict Management Infographic Slide Template',
           'ALT': 'Illustration of two businessmen in conflict with a lightning bolt and a balance scale',
           'C1': 'COMMUNICATION', 'C2': 'NEGOTIATION', 'C3': 'MEDIATION', 'C4': 'COMPROMISE',
           'BAND': 'Your Text Here',
           'H1': 'Placeholder', 'P1': 'This is a sample text. Insert your desired text here.',
           'H2': 'Placeholder', 'P2': 'This is a sample text. Insert your desired text here.',
           'H3': 'Placeholder', 'P3': 'This is a sample text. Insert your desired text here.'},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'hr', 'icon': '⚖️',
    'name': 'إنفوجرافيك إدارة النزاعات', 'nameEn': 'Conflict Management Infographic',
    'description': 'شريحة واحدة بأربعة أساليب لإدارة النزاع حول مشهد خلاف، وشريط نص وثلاثة عناصر سفلية',
    'descriptionEn': 'One slide with four conflict-management styles around a dispute scene, a text band and three items',
    'sections': [
        {'title': 'أساليب إدارة النزاعات', 'titleEn': 'Conflict Management Styles'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والمشهد الأوسط وغيّر النصوص فقط؛ تبقى البطاقات الأربع بألوانها وأسهمها والعناصر الثلاثة السفلية بعددها.',
}
