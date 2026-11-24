# -*- coding: utf-8 -*-
# نموذج «مخطط نموذج العمل ببطاقات»: شريحتان بالتخطيط نفسه (فاتحة وداكنة)، تسع بطاقات بلسان ملوّن وإطار وأيقونة خطية ونص.
KEY = 'sl-card-style-business-model-canvas'
TITLE = {'ar': 'مخطط نموذج العمل ببطاقات', 'en': 'Card-Style Business Model Canvas'}
COMMENT = 'نموذج «مخطط نموذج العمل ببطاقات»: عنوان أعلى وتسع بطاقات (الشركاء، الأنشطة، الموارد، القيمة، العلاقة، القنوات، الشرائح، التكاليف، الإيرادات) لكل منها لسان ملوّن بعنوان أبيض غامق وإطار بلونه وأيقونة خطية ونص. الشريحة الثانية النسخة الداكنة.'
VARS = ('--ink: #000000; --ink-2: #404040; --accent: #4DB0E7; --bg: #FFFFFF; --dark: #262626; '
        '--slate: #4C6772; --orange: #E2772F; --sky: #4DB0E7; --tan: #EDD1A1; --tan-t: #9B732E; --tan-i: #C9A361; --green: #3A6F51; --teal: #2C4C57;')

STYLE = """  .ttl { position: absolute; top: 34px; inset-inline-start: 64px; margin: 0; font-size: 41px; font-weight: 400; line-height: 1.2; color: var(--ink); white-space: nowrap; }
  html[lang="ar"] .ttl { font-weight: 500; top: 30px; }
  .s2 { background: var(--dark); } .s2 .ttl { color: #fff; }
  .kp { --c: var(--slate); --ic: #7E8E96; } .ka, .cr { --c: var(--orange); --ic: var(--orange); } .vp, .cs2 { --c: var(--sky); --ic: var(--sky); }
  .kr, .rv { --c: var(--tan); --ic: var(--tan-i); --t: var(--tan-t); } .ch { --c: var(--green); --ic: var(--green); } .cs { --c: var(--teal); --ic: #6F8189; }
  .s2 .kp { --ic: #5D6C73; } .s2 .cs { --ic: #3C5560; } .s2 .ch { --ic: #4E8668; } .s2 .kr, .s2 .rv { --ic: #B08A4C; }
  /* البطاقة: لسان بعنوان ثم صندوق بإطار؛ الألسنة السفلية فوق إطارات البطاقات العلوية */
  .tab { position: absolute; height: 34px; margin: 0; padding-inline-start: 10px; background: var(--c); color: var(--t, #fff); border-start-start-radius: 7px; border-start-end-radius: 7px; display: flex; align-items: center; font-size: 15px; font-weight: 700; white-space: nowrap; z-index: 2; }
  html[lang="ar"] .tab { font-size: 16px; padding-inline-start: 12px; }
  .box { position: absolute; border: 3px solid var(--c); border-radius: 8px; border-start-start-radius: 0; background: var(--bg); z-index: 1; }
  .s2 .box { background: var(--dark); }
  .box .ic { position: absolute; top: 10px; inset-inline-start: 11px; width: 50px; height: 44px; color: var(--ic); }
  .box .ic svg { width: 100%; height: 100%; }
  html[dir="rtl"] .box .ic.m svg { transform: scaleX(-1); }
  .box p { position: absolute; top: 56px; inset-inline-start: 14px; width: 186px; margin: 0; font-size: 17.5px; line-height: 22.6px; color: var(--ink-2); }
  .s2 .box p { color: #fff; }
  .box.wide .ic { top: 16px; inset-inline-start: 16px; width: 44px; height: 40px; }
  .box.wide p { top: 14px; inset-inline-start: 95px; width: 452px; }"""

SW = 'fill="none" stroke="currentColor" stroke-width="1.1" stroke-linecap="round" stroke-linejoin="round"'
ICONS = {
    # مصافحة
    'kp': ('<svg viewBox="0 0 24 24" %s aria-hidden="true"><path d="M1.2 9.2 4.6 7.2l3.2 1.1M22.8 9.2 19.4 7.2l-4 .5-4.6 2.7a1.2 1.2 0 0 0 1.2 2.1l3.2-1.5 5 4.4"/>'
           '<path d="M3.4 10.6 4.8 13.8M20.6 10.6l-1.4 3.4"/><path d="M5.2 13.4l1.2 1M6.6 12.2l2.4 2.2a.9.9 0 0 1-1.3 1.3L5.6 13.8M9 13.2l2.2 2a.9.9 0 0 1-1.3 1.3L8.4 15.1M11.2 15.2l1.4 1.3a.9.9 0 0 1-1.3 1.3l-1-1M19.2 14.6l-5.6 3.8a.9.9 0 0 1-1.1-1.3"/></svg>' % SW),
    # طبقات
    'ka': '<svg viewBox="0 0 24 24" %s aria-hidden="true"><path d="M12 5.2 21 9.8l-9 4.6-9-4.6z"/><path d="M5.8 12.4 3 13.8l9 4.6 9-4.6-2.8-1.4"/></svg>' % SW,
    # ماسة
    'vp': '<svg viewBox="0 0 24 24" %s aria-hidden="true"><path d="M7 4.4h10l4 4.6-9 11-9-11z"/><path d="M3 9h18M9.6 4.4 8 9l4 11 4-11-1.6-4.6"/></svg>' % SW,
    # وجه مبتسم بقلبين
    'cr': ('<svg viewBox="0 0 24 24" %s aria-hidden="true"><circle cx="11" cy="13" r="8"/><path d="M7.4 12q1-1.5 2 0M12.6 12q1-1.5 2 0M7.4 15.4q3.6 3.4 7.2 0"/>'
           '<path d="M19.6 6.8l-1.6-1.5a.9.9 0 0 1 1.6-1.1.9.9 0 0 1 1.6 1.1zM20.6 18.4l-1.4-1.3a.8.8 0 0 1 1.4-1 .8.8 0 0 1 1.4 1zM6.6 5.6 5.4 4.5a.7.7 0 0 1 1.2-.9.7.7 0 0 1 1.2.9z"/></svg>' % SW),
    # هرم
    'cs': '<svg viewBox="0 0 24 24" %s aria-hidden="true"><path d="M12 3 20.5 20h-17z"/><path d="M9.2 8.6h5.6M7.4 12.2h9.2M5.6 15.8h12.8"/></svg>' % SW,
    # رأس بتروس
    'kr': ('<svg viewBox="0 0 24 24" %s aria-hidden="true"><path d="M8 21v-4.2A7.6 7.6 0 1 1 19 9.4l1.8 3.6-1.8.6v2.6a1 1 0 0 1-1 1h-2.6V21"/>'
           '<circle cx="10.6" cy="8.6" r="2.3" stroke-dasharray="1.2 .8" stroke-width="1.8"/><circle cx="10.6" cy="8.6" r=".8"/>'
           '<circle cx="14.4" cy="12.2" r="1.7" stroke-dasharray="1 .7" stroke-width="1.6"/><circle cx="14.4" cy="12.2" r=".5"/></svg>' % SW),
    # شبكة قنوات
    'ch': ('<svg viewBox="0 0 24 24" %s aria-hidden="true"><circle cx="12" cy="12" r="2.4"/><circle cx="12" cy="3.6" r="1.5"/><circle cx="12" cy="20.4" r="1.5"/>'
           '<circle cx="4.7" cy="7.8" r="1.5"/><circle cx="19.3" cy="7.8" r="1.5"/><circle cx="4.7" cy="16.2" r="1.5"/><circle cx="19.3" cy="16.2" r="1.5"/>'
           '<path d="M12 5.1v4.5M12 14.4v4.5M6 8.6l3.9 2.2M18 8.6l-3.9 2.2M6 15.4l3.9-2.2M18 15.4l-3.9-2.2"/></svg>' % SW),
    # هيكل تنظيمي
    'cost': ('<svg viewBox="0 0 24 24" %s aria-hidden="true"><rect x="9.5" y="3.4" width="5" height="3.6"/><path d="M12 7v3.6M5 10.6h14M5 10.6v3.2M12 10.6v3.2M19 10.6v3.2"/>'
             '<rect x="2.5" y="13.8" width="5" height="3.6"/><rect x="9.5" y="13.8" width="5" height="3.6"/><rect x="16.5" y="13.8" width="5" height="3.6"/></svg>' % SW),
    # أعمدة بسهم صاعد
    'rv': ('<svg viewBox="0 0 24 24" %s aria-hidden="true"><path d="M3 3v18h18"/><rect x="5.6" y="14.4" width="3" height="5"/><rect x="10.4" y="11.4" width="3" height="8"/>'
           '<rect x="15.2" y="7.4" width="3" height="12"/><path d="M5.4 11.6 10 7.8l2.4 1.6 5.4-5.2M15.4 4.2h2.6v2.6"/></svg>' % SW),
}
MIRROR = {'kr', 'rv'}

# (الصنف، لسان: x, y, عرض)، (صندوق: x, y, عرض, ارتفاع)، عريض؟، رمز النص
CARDS = [
    ('kp', (63, 105, 198), (63, 138, 222, 330), False, 'PL'),
    ('ka', (296, 105, 163), (296, 138, 222, 159), False, 'PS'),
    ('vp', (529, 105, 198), (529, 138, 223, 330), False, 'PS'),
    ('cr', (763, 105, 221), (763, 138, 222, 159), False, 'PS'),
    ('cs', (996, 105, 221), (996, 138, 222, 330), False, 'PS'),
    ('kr', (296, 276, 163), (296, 309, 222, 159), False, 'PS'),
    ('ch', (763, 276, 163), (764, 309, 222, 159), False, 'PS'),
    ('cs2', (63, 448, 198), (63, 481, 572, 191), True, 'PW'),
    ('rv', (645, 448, 198), (646, 481, 572, 191), True, 'PW'),
]
ICON_OF = {'cs2': 'cost'}
TAB_OF = {'kp': 'KP', 'ka': 'KA', 'vp': 'VP', 'cr': 'CR', 'cs': 'CS', 'kr': 'KR', 'ch': 'CH', 'cs2': 'CO', 'rv': 'RV'}


def card(c):
    cls, (tx, ty, tw), (bx, by, bw, bh), wide, ptxt = c
    ic = ICON_OF.get(cls, cls)
    return ('<h2 class="tab %s" style="top:%dpx;inset-inline-start:%dpx;width:%dpx">§%s§</h2>'
            '<div class="box %s%s" style="top:%dpx;inset-inline-start:%dpx;width:%dpx;height:%dpx"><div class="ic%s">%s</div><p>§%s§</p></div>'
            % (cls, ty, tx, tw, TAB_OF[cls], cls, ' wide' if wide else '', by, bx, bw, bh, ' m' if ic in MIRROR else '', ICONS[ic], ptxt))


def slide():
    return '\n    <h1 class="ttl">§TTL§</h1>\n    ' + '\n    '.join(card(c) for c in CARDS) + '\n  '


SLIDES = [slide(), slide()]

STR = {
    'ar': {'TTL': 'قالب مخطط نموذج العمل',
           'KP': 'الشركاء الرئيسيون', 'KA': 'الأنشطة الرئيسية', 'VP': 'القيمة المقدَّمة', 'CR': 'العلاقة مع العملاء',
           'CS': 'شرائح العملاء', 'KR': 'الموارد الرئيسية', 'CH': 'القنوات', 'CO': 'هيكل التكاليف', 'RV': 'مصادر الإيرادات',
           'PL': 'يمكنك تعديل هذا النص. هذا النص قابل للتعديل. يمكنك تعديل هذا النص.',
           'PS': 'يمكنك تعديل هذا النص. هذا النص قابل للتعديل.',
           'PW': 'يمكنك تعديل هذا النص. هذا النص قابل للتعديل. يمكنك تعديل هذا النص. يمكنك تعديل هذا النص. هذا النص قابل للتعديل. يمكنك تعديل هذا النص.'},
    'en': {'TTL': 'Business Model Canvas PowerPoint Template',
           'KP': 'KEY PARTNERS', 'KA': 'KEY ACTIVITIES', 'VP': 'VALUE PROPOSITION', 'CR': 'CUSTOMER RELATIONSHIP',
           'CS': 'CUSTOMER SEGMENTS', 'KR': 'KEY RESOURCES', 'CH': 'CHANNELS', 'CO': 'COST STRUCTURE', 'RV': 'REVENUE STREAM',
           'PL': 'You can edit this text. This text can be edited. You can edit this text.',
           'PS': 'You can edit this text. This text can be edited.',
           'PW': 'You can edit this text. This text can be edited. You can edit this text. You can edit this text. This text can be edited. You can edit this text.'},
}

FONTS = {'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@400;600;700&display=swap" rel="stylesheet">'}
FONT_STACK = {'en': "'Open Sans', 'Segoe UI', Arial, sans-serif"}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'mgmt', 'icon': '🧩',
    'name': 'مخطط نموذج العمل ببطاقات', 'nameEn': 'Card-Style Business Model Canvas',
    'description': 'شريحتان لمخطط نموذج العمل بتسع بطاقات ملوّنة: نسخة فاتحة ونسخة داكنة',
    'descriptionEn': 'Two Business Model Canvas slides with nine colored cards: light and dark versions',
    'sections': [
        {'title': 'المخطط (فاتح)', 'titleEn': 'Canvas (Light)'},
        {'title': 'المخطط (داكن)', 'titleEn': 'Canvas (Dark)'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والبطاقات التسع وألوانها وغيّر النصوص فقط؛ الشريحة الثانية نسخة داكنة من الأولى.',
}
