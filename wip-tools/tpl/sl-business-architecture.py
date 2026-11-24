# -*- coding: utf-8 -*-
# نموذج «هندسة الأعمال»: قوس نصف دائري تعلوه خمس فقاعات مراحل (اكتشاف، تحليل، توصية، تحوّل، قياس) بأيقونات،
# وفي أسفله ثلاثة مكعبات ثلاثية الأبعاد (قدرات الأعمال، مسارات القيمة، الخريطة التنظيمية). شريحتان: فاتحة وداكنة.
import math

KEY = 'sl-business-architecture'
TITLE = {'ar': 'هندسة الأعمال', 'en': 'Business Architecture'}
COMMENT = 'نموذج «هندسة الأعمال»: رسم SVG واحد بحجم الإطار (القوس والفقاعات والمكعبات والتروس والهيكل التنظيمي) يُعكس أفقيًا في العربية، وفوقه النصوص والأيقونات بوسوم HTML. الشريحة الثانية (.frame.s2) هي النسخة الداكنة بالخلفية الزرقاء.'
VARS = ('--ink: #000000; --accent: #345B9C; --bg: #FFFFFF; --label: #404040; --arc: #BFBFBF;'
        ' --c-d: #6FAFED; --c-a: #345B9C; --c-r: #64B5A2; --c-t: #8AB84C; --c-m: #618135;')

FONTS = {'ar': '<link href="https://fonts.googleapis.com/css2?family=Tajawal:wght@400;500;700;800&display=swap" rel="stylesheet">',
         'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@400;600;700&display=swap" rel="stylesheet">'}
FONT_STACK = {'ar': "'Tajawal', 'Segoe UI', Tahoma, sans-serif", 'en': "'Open Sans', 'Segoe UI', Tahoma, Arial, sans-serif"}

STYLE = """  .frame.s2 { --ink: #FFFFFF; --label: #FFFFFF; --arc: #24466C; background: linear-gradient(135deg, #183559 0%, #132C4A 55%, #0E233B 100%); }
  .ttl { position: absolute; top: 38px; inset-inline-start: 64px; margin: 0; font-size: 42px; font-weight: 400; line-height: 1.2; color: var(--ink); letter-spacing: -0.3px; }
  html[lang="ar"] .ttl { font-weight: 500; letter-spacing: 0; }
  /* الرسم كاملًا بإحداثيات الإطار؛ يُعكس في العربية */
  .art { position: absolute; inset: 0; width: 1280px; height: 720px; }
  html[dir="rtl"] .art { transform: scaleX(-1); }
  .art .arc { fill: none; stroke: var(--arc); stroke-width: 3; }
  .art .disc { fill: #fff; filter: url(#ba-sh); }
  .art .gear { stroke: #fff; stroke-width: 1.5; fill-rule: evenodd; }
  /* المراحل: عنوان فوق كل فقاعة */
  .lbl { position: absolute; width: 220px; margin: 0; text-align: center; font-size: 20px; font-weight: 700; line-height: 24px; color: var(--label); letter-spacing: 0.2px; }
  html[lang="ar"] .lbl { font-size: 23px; letter-spacing: 0; }
  .lb-d { top: 477px; inset-inline-start: 154px; } .lb-a { top: 228px; inset-inline-start: 260px; } .lb-r { top: 114px; inset-inline-start: 530px; }
  .lb-t { top: 228px; inset-inline-start: 795px; } .lb-m { top: 477px; inset-inline-start: 904px; }
  /* الأيقونات داخل الدوائر البيضاء */
  .ic { position: absolute; width: 72px; height: 72px; color: #3F3F3F; }
  .ic svg { width: 100%; height: 100%; }
  .ic .pale { stroke: none; }
  .ic-d { top: 546px; inset-inline-start: 228px; } .ic-a { top: 294px; inset-inline-start: 334px; } .ic-r { top: 179px; inset-inline-start: 600px; }
  .ic-t { top: 294px; inset-inline-start: 864px; } .ic-m { top: 546px; inset-inline-start: 979px; }
  .hub { position: absolute; top: 364px; inset-inline-start: 500px; width: 280px; margin: 0; text-align: center; font-size: 29px; font-weight: 700; line-height: 38px; color: var(--ink); }
  /* تسميات المكعبات */
  .bx { position: absolute; width: 200px; margin: 0; text-align: center; font-size: 20px; font-weight: 400; line-height: 23px; color: #fff; }
  html[lang="ar"] .bx { font-size: 21px; line-height: 25px; font-weight: 500; }
  .bx-v { top: 516px; inset-inline-start: 540px; } .bx-c { top: 608px; inset-inline-start: 385px; } .bx-o { top: 608px; inset-inline-start: 692px; }"""

GLOBAL_DEFS = ('<defs>'
               '<filter id="ba-sh" x="-30%" y="-30%" width="160%" height="160%"><feDropShadow dx="2" dy="3" stdDeviation="3" flood-color="#000" flood-opacity=".28"/></filter>'
               '<linearGradient id="ba-ring" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#fff" stop-opacity=".14"/><stop offset=".55" stop-color="#fff" stop-opacity="0"/><stop offset="1" stop-color="#000" stop-opacity=".16"/></linearGradient>'
               '<linearGradient id="ba-vt" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#3460A2"/><stop offset="1" stop-color="#26457A"/></linearGradient>'
               '<linearGradient id="ba-lt" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#456AAA"/><stop offset="1" stop-color="#7598D2"/></linearGradient>'
               '<linearGradient id="ba-rt" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#668C34"/><stop offset="1" stop-color="#86AE4C"/></linearGradient>'
               '</defs>')


def bubble(cx, cy, deg, var):
    """فقاعة: دائرة ملوّنة نصف قطرها 64 بذيل مثلثي باتجاه نقطة القوس، ودائرة بيضاء بظل."""
    R, tip, half = 64, 78, 17
    a = math.radians(deg)
    p = lambda ang, r: (cx + r * math.cos(ang), cy + r * math.sin(ang))
    t = p(a, tip); s1 = p(a + math.radians(half), R); s2 = p(a - math.radians(half), R)
    d = 'M%.1f %.1f L%.1f %.1f A%d %d 0 1 1 %.1f %.1f Z' % (t[0], t[1], s1[0], s1[1], R, R, s2[0], s2[1])
    return ('<path d="%s" fill="var(%s)"/><path d="%s" fill="url(#ba-ring)"/><circle class="disc" cx="%d" cy="%d" r="47"/>'
            % (d, var, d, cx, cy))


def gear(cx, cy, ro, n, color):
    """ترس بعدد أسنان n ونصف قطر خارجي ro وثقب في المنتصف (evenodd)."""
    ri, hole, step = ro * 0.74, ro * 0.38, 2 * math.pi / n
    pts = []
    for i in range(n):
        a = i * step
        for ang, r in ((a - step * 0.30, ri), (a - step * 0.18, ro), (a + step * 0.18, ro), (a + step * 0.30, ri)):
            pts.append('%.1f %.1f' % (cx + r * math.cos(ang), cy + r * math.sin(ang)))
    d = 'M' + ' L'.join(pts) + ' Z M%.1f %.1f a%.1f %.1f 0 1 0 %.1f 0 a%.1f %.1f 0 1 0 %.1f 0 Z' % (cx - hole, cy, hole, hole, 2 * hole, hole, hole, -2 * hole)
    return '<path class="gear" d="%s" fill="%s"/>' % (d, color)


def chevron(x, color):
    return '<polygon points="%d,467 %d,467 %d,485 %d,503 %d,503 %d,485" fill="%s" stroke="#fff" stroke-width="1.2"/>' % (x, x + 27, x + 37, x + 27, x, x + 10, color)


ART = ('<svg class="art" viewBox="0 0 1280 720" aria-hidden="true">'
       # القوس ونقاطه
       '<path class="arc" d="M367 580 A273 273 0 0 1 913 580"/>'
       '<circle cx="367" cy="580" r="6.5" fill="#1A7BD9"/><circle cx="445" cy="387" r="6.5" fill="#335E93"/><circle cx="640" cy="307" r="6.5" fill="#62B5A3"/>'
       '<circle cx="832" cy="387" r="6.5" fill="#8BB64D"/><circle cx="913" cy="580" r="6.5" fill="#678C3E"/>'
       # الفقاعات الخمس
       + bubble(264, 582, 0, '--c-d') + bubble(370, 330, 37, '--c-a') + bubble(636, 215, 88, '--c-r')
       + bubble(900, 330, 140, '--c-t') + bubble(1015, 582, 180, '--c-m')
       # المكعب الأوسط (مسارات القيمة)
       + '<polygon points="515,536 640,584 640,720 515,720" fill="#3359A0"/><polygon points="640,584 765,536 765,720 640,720" fill="#7699D3"/>'
       '<polygon points="515,536 640,497 765,536 640,584" fill="url(#ba-vt)"/>'
       + chevron(588, '#335D99') + chevron(624, '#335D99') + chevron(660, '#7F7F7F')
       # المكعب الأيسر (قدرات الأعمال)
       + '<polygon points="360,630 485,677 485,720 360,720" fill="#7699D3"/><polygon points="485,677 610,630 610,720 485,720" fill="#A3BBE1"/>'
       '<polygon points="360,630 485,583 610,630 485,677" fill="url(#ba-lt)"/>'
       # المكعب الأيمن (الخريطة التنظيمية)
       '<polygon points="670,630 795,677 795,720 670,720" fill="#8CB74C"/><polygon points="795,677 920,630 920,720 795,720" fill="#BAD495"/>'
       '<polygon points="670,630 795,583 920,630 795,677" fill="url(#ba-rt)"/>'
       # التروس
       + gear(474, 555, 14, 8, '#6DAAE8') + gear(456, 581, 18, 9, '#6DAAE8') + gear(495, 590, 25, 10, '#7C7C7C')
       # الهيكل التنظيمي
       + '<g stroke="#6D9A3C" stroke-width="4" fill="none"><path d="M797 533V586M762 561H832M762 559V587M832 559V587"/></g>'
       '<g fill="#8CB849" stroke="#fff" stroke-width="1.5"><rect x="787" y="516" width="19" height="19"/><rect x="753" y="586" width="19" height="19"/><rect x="787" y="586" width="19" height="19"/><rect x="822" y="586" width="19" height="19"/></g>'
       '</svg>')

_S = 'fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"'
ICONS = {
    # مصباح
    'd': ('#E8F1FA', '<path d="M29 15a9 9 0 0 0-5 16.4V35h10v-3.6A9 9 0 0 0 29 15z"/><path d="M25 38.5h8M26 42h6"/><path d="M29 7v3.5M18 12l2.4 2.4M40 12l-2.4 2.4M14 24h3.5M40.5 24H44"/>'),
    # مجهر
    'a': ('#E7EEF8', '<path d="M31 9l6 3.5-7.5 13-6-3.5z"/><path d="M33.5 10.5l1.5-2.6"/><path d="M26 25.5l-2 3.4"/><path d="M36 25a9.5 9.5 0 0 1-3 16"/><path d="M19 44h22M22 44v-3.5h16V44"/><path d="M25 35h8"/>'),
    # حافظة وقلم
    'r': ('#BBDCD5', '<rect x="21" y="14" width="22" height="30" rx="2"/><path d="M27 14v-3h10v3"/><path d="M27 26h11M27 31h11M27 36h11"/><path d="M12 15l3-3 13 13-1 4-4-1z"/>'),
    # دورة التحوّل
    't': ('#C9DCAE', '<path d="M18 27a11 11 0 0 1 19-7.5"/><path d="M38 13v7h-7"/><path d="M40 31a11 11 0 0 1-19 7.5"/><path d="M20 45v-7h7"/><path d="M24 22v8M21 27l3 3 3-3"/>'),
    # رسم أعمدة بسهم صاعد
    'm': ('#C6D4B8', '<path d="M13 44h32"/><rect x="18" y="33" width="5" height="11"/><rect x="26" y="28" width="5" height="16"/><rect x="34" y="21" width="5" height="23"/><path d="M15 31l9-8 6 4 11-11"/><path d="M36 15.5h5.5V21"/>'),
}


def icon(k):
    pale, body = ICONS[k]
    return ('<div class="ic ic-%s"><svg viewBox="0 0 58 58" aria-hidden="true"><circle class="pale" cx="23" cy="23" r="15" fill="%s"/><g %s>%s</g></svg></div>'
            % (k, pale, _S, body))


def slide():
    h = '\n    ' + ART + '\n    <h1 class="ttl">§TTL§</h1>'
    for k in 'darmt':
        h += '\n    <p class="lbl lb-%s">§L_%s§</p>%s' % (k, k.upper(), icon(k))
    h += ('\n    <p class="hub">§HUB§</p>'
          '\n    <p class="bx bx-v">§BX_V§</p><p class="bx bx-c">§BX_C§</p><p class="bx bx-o">§BX_O§</p>\n  ')
    return h


SLIDES = [slide(), slide()]  # الثانية داكنة عبر .frame.s2

STR = {
    'ar': {'TTL': 'قالب هندسة الأعمال',
           'L_D': 'الاكتشاف', 'L_A': 'التحليل', 'L_R': 'التوصية', 'L_T': 'التحوّل', 'L_M': 'القياس',
           'HUB': 'هندسة<br>الأعمال',
           'BX_V': 'مسارات<br>القيمة', 'BX_C': 'قدرات<br>الأعمال', 'BX_O': 'الخريطة<br>التنظيمية'},
    'en': {'TTL': 'Business Architecture PowerPoint Template',
           'L_D': 'DISCOVER', 'L_A': 'ANALYZE', 'L_R': 'RECOMMEND', 'L_T': 'TRANSFORM', 'L_M': 'MEASURE',
           'HUB': 'Business<br>Architecture',
           'BX_V': 'Value<br>Streams', 'BX_C': 'Business<br>Capabilities', 'BX_O': 'Organization<br>Map'},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'mgmt', 'icon': '🏛️',
    'name': 'هندسة الأعمال', 'nameEn': 'Business Architecture',
    'description': 'شريحتان (فاتحة وداكنة) لمراحل هندسة الأعمال الخمس على قوس فوق ثلاثة مكعبات: القدرات والقيمة والتنظيم',
    'descriptionEn': 'Two slides (light and dark): five business architecture stages on an arc above capability, value and org cubes',
    'sections': [
        {'title': 'هندسة الأعمال (فاتحة)', 'titleEn': 'Business Architecture (Light)'},
        {'title': 'هندسة الأعمال (داكنة)', 'titleEn': 'Business Architecture (Dark)'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص فقط؛ الرسم SVG واحد بإحداثيات الإطار، والشريحة الثانية نسخة داكنة بقاعدة .frame.s2.',
}
