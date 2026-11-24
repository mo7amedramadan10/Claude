# -*- coding: utf-8 -*-
# نموذج «شريحة اجتماع عمل»: نص وجدول أعمال في جهة البداية، ومشهد فريق حول طاولة على أرضية زرقاء في جهة النهاية.
KEY = 'sl-business-meeting-slide'
TITLE = {'ar': 'شريحة اجتماع عمل', 'en': 'Business Meeting Slide Template'}
COMMENT = 'نموذج «شريحة اجتماع عمل»: خلفية مشهد اجتماع (صورة من الأصل بلا نصوص، تُعكس في العربية)، عنوان بكلمتين بلونين، عنوان فرعي، خط أزرق، فقرة، وثلاثة بنود جدول أعمال في صناديق رمادية.'
VARS = '--ink: #2B2B2B; --ink-2: #3A3A3A; --accent: #71B8F4; --box: #F2F2F2; --box-line: #D3D3D3; --bg: #FFFFFF;'

FONTS = {'ar': '<link href="https://fonts.googleapis.com/css2?family=Cairo:wght@400;500;700;800&display=swap" rel="stylesheet">',
         'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@400;600;700&display=swap" rel="stylesheet">'}
FONT_STACK = {'ar': "'Cairo', 'Segoe UI', Tahoma, sans-serif", 'en': "'Open Sans', 'Segoe UI', Arial, sans-serif"}

STYLE = """  .scene { position: absolute; inset: 0; width: 1280px; height: 720px; object-fit: cover; }
  html[dir="rtl"] .scene { transform: scaleX(-1); }
  .ttl { position: absolute; top: 47px; inset-inline-start: 66px; margin: 0; font-size: 52px; font-weight: 700; line-height: 1.3; color: var(--ink); white-space: nowrap; }
  .ttl em { font-style: normal; color: var(--accent); }
  .sub { position: absolute; top: 117px; inset-inline-start: 67px; margin: 0; font-size: 30px; font-weight: 400; line-height: 1.3; color: var(--ink-2); }
  .bar { position: absolute; top: 178px; inset-inline-start: 64px; width: 204px; height: 6px; border-radius: 3px; background: var(--accent); }
  .txt { position: absolute; top: 207px; inset-inline-start: 67px; width: 490px; margin: 0; font-size: 17px; line-height: 1.4; color: #444; }
  .ag { position: absolute; inset-inline-start: 67px; width: 318px; height: 47px; margin: 0; border: 1px solid var(--box-line); border-radius: 8px; background: var(--box); display: flex; align-items: center; padding-inline-start: 19px; font-size: 22px; font-weight: 700; color: var(--ink-2); }
  .ag1 { top: 301px; } .ag2 { top: 376px; } .ag3 { top: 451px; }
  html[lang="ar"] .ttl { top: 42px; font-size: 54px; line-height: 1.45; }
  html[lang="ar"] .sub { top: 118px; font-size: 29px; }
  html[lang="ar"] .txt { font-size: 17px; line-height: 1.5; top: 202px; }
  html[lang="ar"] .ag { font-size: 21px; }"""

SLIDES = ['\n    <img class="scene" src="/templates/freeform/sl-business-meeting-slide/assets/scene.jpg" alt="§ALT§">'
          '\n    <h1 class="ttl">§T1§ <em>§T2§</em></h1>\n    <p class="sub">§SUB§</p>\n    <span class="bar" aria-hidden="true"></span>'
          '\n    <p class="txt">§TXT§</p>'
          '\n    <p class="ag ag1">§A1§</p>\n    <p class="ag ag2">§A2§</p>\n    <p class="ag ag3">§A3§</p>\n  ']

STR = {
    'ar': {'T1': 'اجتماع', 'T2': 'العمل', 'SUB': 'قالب شريحة',
           'TXT': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا.',
           'A1': 'بند جدول الأعمال 01', 'A2': 'بند جدول الأعمال 02', 'A3': 'بند جدول الأعمال 03',
           'ALT': 'رسم توضيحي لفريق عمل يتناقش حول طاولة اجتماعات'},
    'en': {'T1': 'BUSINESS', 'T2': 'MEETING', 'SUB': 'SLIDE TEMPLATE',
           'TXT': 'This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here.',
           'A1': 'Agenda Item 01', 'A2': 'Agenda Item 02', 'A3': 'Agenda Item 03',
           'ALT': 'Illustration of a team discussing around a meeting table'},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'general', 'icon': '📋',
    'name': 'شريحة اجتماع عمل', 'nameEn': 'Business Meeting Slide',
    'description': 'شريحة واحدة بعنوان الاجتماع وفقرة تعريفية وثلاثة بنود جدول أعمال بجوار مشهد فريق عمل',
    'descriptionEn': 'One slide with a meeting title, intro text and three agenda items beside a team scene',
    'sections': [
        {'title': 'اجتماع العمل وجدول الأعمال', 'titleEn': 'Business Meeting Agenda'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وصورة الخلفية وغيّر النصوص فقط؛ يمكن تغيير نصوص بنود جدول الأعمال الثلاثة مع إبقاء عددها وصناديقها.',
}
