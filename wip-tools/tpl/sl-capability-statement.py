# -*- coding: utf-8 -*-
# نموذج «بيان القدرات»: شريحتان بالتخطيط نفسه (خلفية سفلية بيضاء ثم كحلية)؛ ترويسة بشعار واسم الشركة و«من نحن» وبيانات التواصل
# على شكل مائل متدرّج، ثم الكفاءات الأساسية وصندوقا نص، والأداء السابق وأبرز العملاء بعلامات صح.
KEY = 'sl-capability-statement'
TITLE = {'ar': 'بيان القدرات', 'en': 'Capability Statement'}
COMMENT = 'نموذج «بيان القدرات»: ترويسة رمادية فاتحة بشعار دائري واسم الشركة وفقرة «من نحن» وبيانات التواصل فوق شكل مائل متدرّج من الكحلي إلى الأخضر؛ أسفلها أشرطة عناوين بنفسجية متدرّجة: الكفاءات الأساسية (خمس بطاقات بأيقونات دائرية) وصندوقا نص، والأداء السابق وأبرز العملاء بعلامات صح. الشريحة الثانية بخلفية سفلية كحلية.'
VARS = ('--ink: #262626; --ink-2: #404040; --accent: #3B3BA0; --bg: #FFFFFF; --band: #F2F2F2; --navy: #2E2E86; '
        '--bar-a: #323292; --bar-b: #7171D3; --green: #4AC664; --green-d: #2F9A4C;')

STYLE = """  .top { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 251px; background: var(--band); }
  .s2 .top { background: #FFFFFF; }
  .low { position: absolute; top: 251px; inset-inline-start: 0; width: 1280px; height: 469px; background: linear-gradient(to right, #26266E, #2E2C80 50%, #333290); }
  html[dir="rtl"] .low { background: linear-gradient(to left, #26266E, #2E2C80 50%, #333290); }
  /* الأشكال المائلة الزخرفية: مرسومة للإنجليزية وتنعكس في العربية */
  .deco { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; pointer-events: none; }
  html[dir="rtl"] .deco { transform: scaleX(-1); }
  .ab { position: absolute; margin: 0; }
  .logo { top: 42px; inset-inline-start: 76px; width: 121px; height: 121px; border-radius: 50%; background: #FFFFFF; display: grid; place-items: center; }
  .s2 .logo { background: #F0F0F0; }
  .logo svg { width: 82px; height: 82px; }
  .cname { top: 166px; inset-inline-start: 46px; width: 180px; text-align: center; font-size: 22px; font-weight: 700; line-height: 29px; color: var(--ink-2); }
  html[lang="ar"] .cname { font-weight: 700; font-size: 23px; }
  .about { top: 76px; inset-inline-start: 250px; font-size: 26px; font-weight: 700; line-height: 34px; color: var(--ink); }
  html[lang="ar"] .about { font-weight: 800; }
  .about-p { top: 122px; inset-inline-start: 250px; width: 492px; font-size: 17.5px; line-height: 23px; color: var(--ink-2); }
  .ci { position: absolute; color: #FFFFFF; } .ci svg { width: 100%; height: 100%; }
  .ci-ph { top: 85px; inset-inline-start: 1060px; width: 28px; height: 28px; }
  html[dir="rtl"] .ci-ph svg { transform: scaleX(-1); }
  .ci-gl { top: 159px; inset-inline-start: 909px; width: 28px; height: 28px; }
  .ph { top: 80px; inset-inline-start: 1115px; font-size: 15px; line-height: 19.5px; color: #FFFFFF; }
  .ph bdi { direction: ltr; unicode-bidi: isolate; }
  .hr { position: absolute; top: 134px; inset-inline-start: 951px; width: 276px; height: 1px; background: rgb(255 255 255 / .55); }
  .web { top: 154px; inset-inline-start: 950px; font-size: 15px; line-height: 19.5px; color: #FFFFFF; }
  /* أشرطة العناوين */
  .bar { position: absolute; height: 40px; margin: 0; border-radius: 6px; display: flex; align-items: center; gap: 12px; padding-inline-start: 21px; color: #FFFFFF; font-size: 20px; font-weight: 700; background: linear-gradient(to right, var(--bar-a), #363094 25%, var(--bar-b)); }
  html[dir="rtl"] .bar { background: linear-gradient(to left, var(--bar-a), #363094 25%, var(--bar-b)); }
  .s2 .bar { background: linear-gradient(to right, #343293, #4A47A6 30%, #7372CF); }
  html[dir="rtl"] .s2 .bar { background: linear-gradient(to left, #343293, #4A47A6 30%, #7372CF); }
  .bar .bi { flex: none; width: 27px; height: 27px; } .bar .bi svg { width: 100%; height: 100%; }
  .bar1 { top: 287px; inset-inline-start: 55px; width: 612px; }
  .bar2 { top: 287px; inset-inline-start: 692px; width: 533px; padding-inline-start: 24px; }
  .bar3 { top: 477px; inset-inline-start: 692px; width: 533px; padding-inline-start: 16px; } .bar3 .bi { width: 32px; }
  /* بطاقات الكفاءات */
  .cc { position: absolute; width: 173px; height: 73px; border-radius: 8px; background: var(--band); display: flex; align-items: center; padding-inline-start: 35px; }
  .s2 .cc { background: #FFFFFF; }
  .cc h3 { margin: 0; width: 132px; font-size: 15.5px; font-weight: 700; line-height: 19.5px; color: #111111; }
  html[lang="ar"] .cc h3 { font-size: 16px; line-height: 21px; }
  .cc .cir { position: absolute; top: 14px; inset-inline-start: -22px; width: 45px; height: 45px; border-radius: 50%; display: grid; place-items: center; color: #FFFFFF; background: linear-gradient(180deg, #47C26C, #3B7E80 50%, #3A4A8C); }
  .s2 .cc .cir { background: linear-gradient(180deg, #4FCB66, #36A346 60%, #2E8C47); }
  .cc .cir svg { width: 27px; height: 27px; }
  .tb { position: absolute; top: 534px; width: 290px; height: 146px; border-radius: 10px; background: var(--band); margin: 0; padding: 18px 20px 0; font-size: 17.5px; line-height: 23px; color: var(--ink-2); }
  .s2 .tb { background: #FFFFFF; }
  /* عناصر القوائم بعلامة صح */
  .ck { position: absolute; width: 175px; }
  .ck .dot { position: absolute; top: 8px; inset-inline-start: 0; width: 22px; height: 22px; border-radius: 50%; background: var(--navy); color: #FFFFFF; display: grid; place-items: center; }
  .s2 .ck .dot { background: var(--green); }
  .ck .dot svg { width: 13px; height: 13px; }
  .ck p { margin: 0; padding-inline-start: 31px; width: 160px; font-size: 15px; line-height: 19px; color: var(--ink-2); }
  .s2 .ck p { color: #FFFFFF; }"""

GLOBAL_DEFS = ('<defs>'
               '<linearGradient id="csTop" x1="237" y1="0" x2="525" y2="0" gradientUnits="userSpaceOnUse"><stop offset="0" stop-color="#48CA5D"/><stop offset=".12" stop-color="#63C878"/>'
               '<stop offset=".5" stop-color="#5E9D88"/><stop offset=".8" stop-color="#56659C"/><stop offset=".84" stop-color="#36458A"/><stop offset="1" stop-color="#33328F"/></linearGradient>'
               '<linearGradient id="csBig" x1="820" y1="270" x2="1060" y2="-30" gradientUnits="userSpaceOnUse"><stop offset="0" stop-color="#36448C"/><stop offset=".3" stop-color="#4C6A92"/>'
               '<stop offset=".55" stop-color="#4E9183"/><stop offset=".8" stop-color="#55B479"/><stop offset="1" stop-color="#5DCF70"/></linearGradient>'
               '<linearGradient id="csBot" x1="694" y1="0" x2="1213" y2="0" gradientUnits="userSpaceOnUse"><stop offset="0" stop-color="#34408A"/><stop offset=".3" stop-color="#557894"/>'
               '<stop offset=".75" stop-color="#5EA885"/><stop offset="1" stop-color="#60BE7C"/></linearGradient>'
               '<linearGradient id="csLogo" x1="0" y1="1" x2="1" y2="0"><stop offset="0" stop-color="#34318F"/><stop offset=".45" stop-color="#3F7F8A"/><stop offset="1" stop-color="#4DC55E"/></linearGradient>'
               '</defs>')

DECO = ('<svg class="deco" viewBox="0 0 1280 720" aria-hidden="true">'
        '<polygon points="292,0 525,0 476,37 237,37" fill="url(#csTop)"/>'
        '<polygon points="440,0 525,0 476,37 392,37" fill="#2B2A80" opacity=".35"/>'
        '<polygon points="1066,0 1280,0 1280,167 1155,251 690,251" fill="url(#csBig)"/>'
        '<polygon points="1066,0 1136,0 760,251 690,251" fill="#18306A" opacity=".2"/>'
        '<polygon points="1280,132 1280,167 1155,251 1103,251" fill="#1C4A57" opacity=".22"/>'
        '<polygon points="694,680 1153,680 1213,720 754,720" fill="url(#csBot)"/>'
        '<polygon points="694,680 748,680 808,720 754,720" fill="#1E2C6A" opacity=".35"/>'
        '<polygon points="1108,680 1153,680 1213,720 1168,720" fill="#2B9C48" opacity=".4"/>'
        '</svg>')

LOGO = ('<svg viewBox="0 0 100 100" aria-hidden="true">'
        '<path fill="url(#csLogo)" fill-rule="evenodd" d="M86 33A40 40 0 1 0 86 67L72 60A24 24 0 1 1 72 40zM40 42h48v16H40z"/>'
        '<path fill="#FFFFFF" d="M12 30Q30 14 54 16L50 20Q30 20 16 34z"/><path fill="#FFFFFF" d="M8 62Q12 84 36 90L32 86Q14 80 12 62z"/>'
        '<path fill="url(#csLogo)" d="M24 20Q14 30 14 44Q10 30 18 22z"/></svg>')

ICONS = {
    'check': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12.5l4.5 4.5L19 7.5"/></svg>',
    'phone': ('<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.2" stroke-linejoin="round" aria-hidden="true">'
              '<path d="M5.2 3.4 8 3l1.6 4.2-1.9 1.4a11 11 0 0 0 5.7 5.7l1.4-1.9 4.2 1.6-.4 2.8a2 2 0 0 1-2.2 1.6A15 15 0 0 1 3.6 5.6a2 2 0 0 1 1.6-2.2z"/>'
              '<path d="M14 2.6a7.6 7.6 0 0 1 7.4 7.4M14 5.6a4.6 4.6 0 0 1 4.4 4.4" stroke-linecap="round"/></svg>'),
    'globe': ('<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.2" aria-hidden="true"><circle cx="12" cy="12" r="9.6"/>'
              '<ellipse cx="12" cy="12" rx="4.4" ry="9.6"/><path d="M2.4 12h19.2M4 7h16M4 17h16M12 2.4v19.2"/></svg>'),
    # رأس بتروس
    'head': ('<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path fill-rule="evenodd" d="M7 22.5v-4.8A8.6 8.6 0 1 1 20.4 10l1.8 3.6-1.8.6v2.6a1.4 1.4 0 0 1-1.4 1.4h-2.4v4.3z'
             'M10.6 4.4l.9.1.3 1.2.8.4 1.1-.6 1.3 1-.5 1.2.4.8 1.2.3v1.6l-1.2.3-.4.8.5 1.2-1.3 1-1.1-.6-.8.4-.3 1.2h-1.6l-.3-1.2-.8-.4-1.1.6-1.3-1 .5-1.2-.4-.8L6 10.8V9.2l1.2-.3.4-.8-.5-1.2 1.3-1 1.1.6.8-.4.3-1.2zm.1 3.6a2 2 0 1 0 0 4 2 2 0 0 0 0-4z"/></svg>'),
    'bars': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><rect x="3" y="15" width="3.4" height="6"/><rect x="8.2" y="11" width="3.4" height="10"/><rect x="13.4" y="7" width="3.4" height="14"/><rect x="18.6" y="3" width="3.4" height="18"/></svg>',
    'users': ('<svg viewBox="0 0 32 24" fill="currentColor" aria-hidden="true"><circle cx="16" cy="7" r="4"/><circle cx="7" cy="9" r="3"/><circle cx="25" cy="9" r="3"/>'
              '<path d="M8.6 21v-1.4c0-4 3.3-7 7.4-7s7.4 3 7.4 7V21z"/><path d="M1 20v-1c0-2.8 2.6-4.9 5.8-4.9 1 0 1.9.2 2.7.6A8.6 8.6 0 0 0 7 20z"/>'
              '<path d="M31 20v-1c0-2.8-2.6-4.9-5.8-4.9-1 0-1.9.2-2.7.6A8.6 8.6 0 0 1 25 20z"/></svg>'),
    # موظفة
    'staff': ('<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12 2.4c-3.4 0-5.6 2.6-5.4 6 .1 2.4-.6 4.2-1.6 5.4 1.4.6 3 .8 4.4.6A4.8 4.8 0 0 1 12 15a4.8 4.8 0 0 1 2.6-.6c1.4.2 3 0 4.4-.6-1-1.2-1.7-3-1.6-5.4.2-3.4-2-6-5.4-6z"/>'
              '<path d="M3.6 21.6c.4-3.4 2.6-5.4 5.8-5.8L12 18.4l2.6-2.6c3.2.4 5.4 2.4 5.8 5.8z"/></svg>'),
    # فريق حول طاولة
    'team': ('<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M4.6 12.4h14.8a7.4 5.4 0 0 1-14.8 0z"/>'
             '<circle cx="12" cy="4.6" r="1.8"/><circle cx="6.4" cy="6.6" r="1.7"/><circle cx="17.6" cy="6.6" r="1.7"/><circle cx="3" cy="11" r="1.6"/><circle cx="21" cy="11" r="1.6"/>'
             '<path d="M9.8 9.6a2.2 2.2 0 0 1 4.4 0v1.6H9.8zM4.6 10.6a1.8 1.8 0 0 1 3.6-.6v1.2H4.6zM15.8 10a1.8 1.8 0 0 1 3.6.6v.6h-3.6z"/></svg>'),
    # نافذة برمجية
    'code': ('<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">'
             '<rect x="2.4" y="3.6" width="19.2" height="16.8" rx="1"/><path d="M2.4 7.6h19.2"/><path d="M9.6 11l-2.4 2.4 2.4 2.4M14.4 11l2.4 2.4-2.4 2.4M12.8 10.4l-1.6 6"/></svg>'),
    # سحابة بشبكة
    'cloud': ('<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.1" stroke-linecap="round" aria-hidden="true">'
              '<path fill="currentColor" stroke="none" d="M7.6 15.2a2.6 2.6 0 0 1 .3-5.2 3.8 3.8 0 0 1 7.2-1 3.1 3.1 0 1 1 1.4 6.2z"/>'
              '<path d="M12 15.2v4M9 15.2l-2 3M15 15.2l2 3M6.6 12.4H3.4M17.4 12.4h3.2M8 9 5.6 6.6M16 9l2.4-2.4M12 6V3"/>'
              '<circle cx="12" cy="20.2" r=".9"/><circle cx="6.4" cy="19" r=".9"/><circle cx="17.6" cy="19" r=".9"/><circle cx="2.6" cy="12.4" r=".9"/><circle cx="21.4" cy="12.4" r=".9"/>'
              '<circle cx="5" cy="6" r=".9"/><circle cx="19" cy="6" r=".9"/><circle cx="12" cy="2.2" r=".9"/></svg>'),
    # هيكل تنظيمي
    'org': ('<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><rect x="9.4" y="3" width="5.2" height="4"/><rect x="2.6" y="16" width="5.2" height="4"/><rect x="9.4" y="16" width="5.2" height="4"/><rect x="16.2" y="16" width="5.2" height="4"/>'
            '<path d="M11.4 7h1.2v4H19.8v5h-1.2v-3.8H12.6V16h-1.2v-3.8H5.4V16H4.2v-5h7.2z"/></svg>'),
}

COMP = [('staff', 'C1', 77, 347), ('team', 'C2', 284, 347), ('code', 'C3', 491, 347), ('cloud', 'C4', 77, 432), ('org', 'C5', 284, 432)]
COLS = (692, 873, 1054)


def checks(rows, n):
    out = []
    for i in range(n):
        x, y = COLS[i % 3], rows[i // 3]
        out.append('<div class="ck" style="top:%dpx;inset-inline-start:%dpx"><span class="dot">%s</span><p>§CK§</p></div>' % (y - 8, x, ICONS['check']))
    return ''.join(out)


def slide():
    return ('\n    <div class="top"></div>' + '\n    ' + DECO
            + '\n    <div class="ab logo">' + LOGO + '</div><p class="ab cname">§CNAME§</p>'
            + '\n    <h1 class="ab about">§ABOUT§</h1><p class="ab about-p">§ABOUT_P§</p>'
            + '\n    <span class="ci ci-ph">' + ICONS['phone'] + '</span><p class="ab ph"><bdi dir="ltr">(999) 999-9999</bdi><br><bdi dir="ltr">(999) 999-9999</bdi></p><div class="hr"></div>'
            + '<span class="ci ci-gl">' + ICONS['globe'] + '</span><p class="ab web">§WEB§<br>§MAIL§</p>'
            + '\n    <h2 class="bar bar1"><span class="bi">' + ICONS['head'] + '</span>§CORE§</h2>'
            + '\n    ' + ''.join('<div class="cc" style="top:%dpx;inset-inline-start:%dpx"><span class="cir">%s</span><h3>§%s§</h3></div>' % (y, x, ICONS[ic], k) for ic, k, x, y in COMP)
            + '\n    <p class="tb" style="inset-inline-start:55px">§TB§</p><p class="tb" style="inset-inline-start:376px;width:291px">§TB§</p>'
            + '\n    <h2 class="bar bar2"><span class="bi">' + ICONS['bars'] + '</span>§PAST§</h2>'
            + '\n    ' + checks((359, 415), 6)
            + '\n    <h2 class="bar bar3"><span class="bi">' + ICONS['users'] + '</span>§CLIENTS§</h2>'
            + '\n    ' + checks((550, 606), 5) + '\n  ')


def slide2():
    return slide().replace('<div class="top"></div>', '<div class="top"></div><div class="low"></div>', 1)


SLIDES = [slide(), slide2()]

STR = {
    'ar': {'CNAME': 'اسم<br>الشركة', 'ABOUT': 'من نحن',
           'ABOUT_P': 'هذا نص توضيحي. أدخل النص المطلوب هنا. هذا نص توضيحي. أدخل النص المطلوب هنا. هذا نص توضيحي. أدخل النص المطلوب هنا. هذا نص توضيحي.',
           'WEB': 'الموقع الإلكتروني للشركة', 'MAIL': 'البريد الإلكتروني للدعم',
           'CORE': 'الكفاءات الأساسية', 'PAST': 'الأداء السابق', 'CLIENTS': 'أبرز العملاء',
           'C1': 'تعزيز الكوادر البشرية', 'C2': 'إدارة المشاريع', 'C3': 'إدارة البرامج', 'C4': 'تطوير البرمجيات', 'C5': 'الإدارة التنفيذية',
           'TB': 'هذا نص توضيحي. أدخل النص المطلوب هنا.', 'CK': 'أدخل النص المطلوب هنا.'},
    'en': {'CNAME': 'Company<br>Name', 'ABOUT': 'ABOUT US',
           'ABOUT_P': 'This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text.',
           'WEB': 'slidemodel.com', 'MAIL': 'support@slidemodel.com',
           'CORE': 'CORE COMPETENCIES', 'PAST': 'PAST PERFORMANCE', 'CLIENTS': 'FEATURED CLIENTS',
           'C1': 'Staff Augmentation', 'C2': 'Project Management', 'C3': 'Program Management', 'C4': 'Software Development', 'C5': 'Executive Administration',
           'TB': 'This is a sample text. Insert your desired text here.', 'CK': 'Insert your desired text here.'},
}

FONTS = {'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@400;600;700&display=swap" rel="stylesheet">'}
FONT_STACK = {'en': "'Open Sans', 'Segoe UI', Arial, sans-serif"}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'general', 'icon': '🏢',
    'name': 'بيان القدرات', 'nameEn': 'Capability Statement',
    'description': 'شريحتان لبيان قدرات الشركة: من نحن، التواصل، الكفاءات الأساسية، الأداء السابق وأبرز العملاء',
    'descriptionEn': 'Two capability statement slides: about us, contact, core competencies, past performance, clients',
    'sections': [
        {'title': 'بيان القدرات (فاتح)', 'titleEn': 'Capability Statement (Light)'},
        {'title': 'بيان القدرات (كحلي)', 'titleEn': 'Capability Statement (Navy)'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والأشكال المائلة وغيّر النصوص فقط (اسم الشركة، من نحن، بيانات التواصل، الكفاءات، الأداء السابق، العملاء)؛ الشريحة الثانية نسخة بخلفية سفلية كحلية.',
}
