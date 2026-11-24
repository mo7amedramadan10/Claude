# -*- coding: utf-8 -*-
# نموذج «اجتماع عمل»: شريحة واحدة بخلفية رسم توضيحي لقاعة اجتماعات (صورة من الأصل بعد إزالة النصوص)،
# عنوان وعنوان فرعي في المنتصف، وثلاث بطاقات أسفلها برأس كحلي وأيقونة ونص.
KEY = 'sl-business-meeting'
TITLE = {'ar': 'اجتماع عمل', 'en': 'Business Meeting'}
COMMENT = 'نموذج «اجتماع عمل»: خلفية مشهد قاعة اجتماعات بالأزرق، عنوان كبير أبيض في المنتصف وتحته عنوان فرعي، وثلاث بطاقات بيضاء برأس كحلي متدرّج فيه عنوان وأيقونة خطية.'
VARS = '--ink: #1F2328; --accent: #2B45A8; --navy: #1B2C7A; --navy-2: #2A3FA0; --bg: #2940A8;'

FONTS = {'ar': '<link href="https://fonts.googleapis.com/css2?family=Cairo:wght@400;500;700;800&display=swap" rel="stylesheet">',
         'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@400;600;700&display=swap" rel="stylesheet">'}
FONT_STACK = {'ar': "'Cairo', 'Segoe UI', Tahoma, sans-serif", 'en': "'Open Sans', 'Segoe UI', Arial, sans-serif"}

STYLE = """  .scene { position: absolute; inset: 0; width: 1280px; height: 720px; object-fit: cover; }
  .ttl { position: absolute; top: 24px; inset-inline-start: 0; width: 1280px; margin: 0; text-align: center; font-size: 60px; font-weight: 700; line-height: 1.25; color: #fff; }
  .sub { position: absolute; top: 108px; inset-inline-start: 0; width: 1280px; margin: 0; text-align: center; font-size: 21px; font-weight: 400; line-height: 1.4; color: #fff; letter-spacing: .3px; }
  html[lang="ar"] .ttl { font-size: 56px; line-height: 1.35; top: 16px; }
  html[lang="ar"] .sub { top: 106px; letter-spacing: 0; }
  .card { position: absolute; top: 519px; width: 331px; height: 147px; border-radius: 14px; background: linear-gradient(180deg, #FFFFFF, #EEF1F2); box-shadow: 0 4px 10px rgb(0 0 40 / .18); }
  .c1 { inset-inline-start: 109px; } .c2 { inset-inline-start: 475px; } .c3 { inset-inline-start: 840px; }
  .card .hd { position: absolute; top: 0; inset-inline-start: 0; width: 331px; height: 57px; border-radius: 14px; background: linear-gradient(90deg, var(--navy), var(--navy-2) 55%, #1C2D76); display: flex; align-items: center; justify-content: space-between; padding-inline: 20px 18px; }
  .card h2 { margin: 0; font-size: 22px; font-weight: 700; line-height: 1.2; color: #fff; }
  .card .ic { width: 42px; height: 42px; color: #fff; flex: none; }
  .card .ic svg { width: 42px; height: 42px; }
  .card p { position: absolute; top: 72px; inset-inline-start: 20px; width: 300px; margin: 0; font-size: 18.5px; line-height: 1.35; color: #222; }
  html[lang="ar"] .card p { font-size: 17.5px; line-height: 1.45; top: 68px; }"""

ICONS = {
    'gem': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width=".8" stroke-linejoin="round" aria-hidden="true"><path d="M6.5 5h11l4 5-9.5 10L2.5 10z"/><path d="M2.5 10h19M9 5l-2 5 5 10 5-10-2-5M7 10l2.5-5M17 10l-2.5-5M12 5v0"/></svg>',
    'team': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width=".9" aria-hidden="true"><circle cx="12" cy="9.3" r="2.3"/><circle cx="5.2" cy="7" r="2"/><circle cx="18.8" cy="7" r="2"/><path d="M7.6 17.5c0-2.6 2-4.6 4.4-4.6s4.4 2 4.4 4.6z"/><path d="M1.2 14.8c0-2.3 1.8-4.1 4-4.1 1 0 1.9.3 2.6.9"/><path d="M22.8 14.8c0-2.3-1.8-4.1-4-4.1-1 0-1.9.3-2.6.9"/></svg>',
    'target': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width=".9" aria-hidden="true"><circle cx="11" cy="13" r="8.5"/><circle cx="11" cy="13" r="5.6"/><circle cx="11" cy="13" r="2.7"/><path d="M11 13l9-9M16.8 4.3l.3 2.6 2.6.3 2.3-2.3-2.6-.3-.3-2.6z"/></svg>',
}


def card(n, icon):
    return ('<div class="card c%d"><div class="hd"><h2>§H%d§</h2><span class="ic">%s</span></div><p>§P%d§</p></div>'
            % (n, n, ICONS[icon], n))


SLIDES = ['\n    <img class="scene" src="/templates/freeform/sl-business-meeting/assets/scene.jpg" alt="§ALT§">'
          '\n    <h1 class="ttl">§TTL§</h1>\n    <p class="sub">§SUB§</p>\n    '
          + card(1, 'gem') + '\n    ' + card(2, 'team') + '\n    ' + card(3, 'target') + '\n  ']

STR = {
    'ar': {'TTL': 'اجتماع عمل', 'SUB': 'العنوان الفرعي هنا.',
           'ALT': 'رسم توضيحي لفريق عمل حول طاولة اجتماعات أمام شاشة عرض',
           'H1': 'عدّل النص هنا', 'P1': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
           'H2': 'عدّل النص هنا', 'P2': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
           'H3': 'عدّل النص هنا', 'P3': 'هذا نص توضيحي. أضف النص المطلوب هنا.'},
    'en': {'TTL': 'Business Meeting', 'SUB': 'SUBTITLE GOES HERE.',
           'ALT': 'Illustration of a team around a meeting table in front of a presentation screen',
           'H1': 'Edit Text Here', 'P1': 'This is a sample text. Insert your desired text here.',
           'H2': 'Edit Text Here', 'P2': 'This is a sample text. Insert your desired text here.',
           'H3': 'Edit Text Here', 'P3': 'This is a sample text. Insert your desired text here.'},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'general', 'icon': '🤝',
    'name': 'اجتماع عمل', 'nameEn': 'Business Meeting',
    'description': 'شريحة واحدة بمشهد قاعة اجتماعات وعنوان وثلاث بطاقات بأيقونات لنقاط الاجتماع',
    'descriptionEn': 'One slide with a meeting-room scene, a title and three icon cards for meeting points',
    'sections': [
        {'title': 'اجتماع عمل: ثلاث نقاط', 'titleEn': 'Business Meeting: Three Points'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وصورة الخلفية وغيّر النصوص فقط (العنوان والعنوان الفرعي ورأس كل بطاقة ونصها).',
}
