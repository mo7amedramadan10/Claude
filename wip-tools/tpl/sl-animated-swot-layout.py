# -*- coding: utf-8 -*-
# نموذج «تخطيط سوات المتحرك»: غلاف بكلمة SWOT ضخمة بتدرّج لوني فوق أغصان باهتة، ثم أربع شرائح بالتخطيط نفسه:
# حرف كبير متدرّج فوق غصن باهت (يتنقّل موضعه أفقيًا من شريحة لأخرى)، عنوان متدرّج في المنتصف، وعمودا نص.
# زخرفتا الزاويتين موجتان بنفسجيتان/ورديتان بخطوط بيضاء رفيعة.
KEY = 'sl-animated-swot-layout'
TITLE = {'ar': 'تخطيط سوات المتحرك', 'en': 'Animated SWOT Layout'}
COMMENT = 'نموذج «تخطيط سوات المتحرك»: خلفية بيضاء وموجتان متدرّجتان (بنفسجي ← وردي) في زاويتين متقابلتين، وكلمة SWOT بتدرّج لكل حرف (سماوي، أزرق، بنفسجي، أرجواني، توتي) فوق أغصان رمادية زرقاء باهتة؛ شرائح التفصيل: حرف متدرّج + عنوان متدرّج + عمودا نص. التدرّجات النصية background-clip:text وتنعكس للعربية.'
VARS = '--ink: #111111; --accent: #7B2BFF; --leaf: #E3E7F1; --bg: #FFFFFF; --ga: 90deg;'

STYLE = """  html[dir="rtl"] { --ga: 270deg; }
  /* ---- الموجتان في الزاويتين ---- */
  .blob { position: absolute; pointer-events: none; }
  .blob svg { width: 100%; height: 100%; }
  .blob .w1 { fill: url(#swl-b1); } .blob .w2 { fill: url(#swl-b2); } .blob .w3 { fill: url(#swl-b3); }
  .blob .ln { fill: none; stroke: #fff; stroke-width: 1.1; opacity: .75; vector-effect: non-scaling-stroke; }
  .tr { top: 0; inset-inline-end: 0; } .bl { bottom: 0; inset-inline-start: 0; }
  .bl svg { transform: rotate(180deg); }
  html[dir="rtl"] .tr svg { transform: scaleX(-1); } html[dir="rtl"] .bl svg { transform: rotate(180deg) scaleX(-1); }
  .s1 .tr { width: 256px; height: 168px; } .s1 .bl { width: 276px; height: 248px; }
  .tr.sm { width: 183px; height: 121px; } .bl.sm { width: 196px; height: 176px; }
  /* ---- الأغصان الباهتة ---- */
  .twig { position: absolute; fill: var(--leaf); overflow: visible; }
  .twig .st { fill: none; stroke: var(--leaf); stroke-width: 2.5; stroke-linecap: round; }
  html[dir="rtl"] .twig { transform: scaleX(-1); }
  /* ---- النص المتدرّج ---- */
  .gt { background-clip: text; -webkit-background-clip: text; color: transparent; -webkit-text-fill-color: transparent; }
  /* ---- الغلاف ---- */
  .cv { position: absolute; top: 127px; inset-inline-start: 0; width: 1280px; margin: 0; text-align: center; font-size: 360px; font-weight: 700; line-height: 440px; letter-spacing: -9px; }
  .cv span { background-image: linear-gradient(var(--ga), #00D6FF 0%, #0095F2 17.4%, #2752FF 18.6%, #7A2BFF 54.2%, #8524FF 55.2%, #D200FF 79%, #D300E6 80%, #C2206C 100%); }
  html[lang="ar"] .cv { font-size: 300px; font-weight: 800; letter-spacing: 0; top: 120px; }
  /* ---- شرائح التفصيل ---- */
  .lt { position: absolute; top: 95px; width: 260px; margin: 0; text-align: center; font-size: 180px; font-weight: 700; line-height: 220px; }
  .lt span { background-image: linear-gradient(var(--ga), var(--g1), var(--g2)); }
  html[lang="ar"] .lt { font-size: 170px; font-weight: 800; top: 80px; }
  .hd { position: absolute; top: 297px; inset-inline-start: 0; width: 1280px; margin: 0; text-align: center; font-size: 40px; font-weight: 700; line-height: 56px; }
  .hd span { background-image: linear-gradient(var(--ga), var(--g1), var(--g2)); }
  .col { position: absolute; top: 421px; width: 380px; margin: 0; font-size: 24.8px; line-height: 34px; color: var(--ink); }
  .c1 { inset-inline-start: 167px; } .c2 { inset-inline-start: 680px; }
  html[lang="ar"] .col { font-size: 26px; line-height: 40px; }
  .s2 { --g1: #00CBFF; --g2: #0085EE; } .s3 { --g1: #2752FF; --g2: #7A2BFF; }
  .s4 { --g1: #7A2BFF; --g2: #D200FF; } .s5 { --g1: #D600F0; --g2: #C2206C; }
  .s2 .lt { inset-inline-start: 301px; } .s3 .lt { inset-inline-start: 440px; }
  .s4 .lt { inset-inline-start: 597px; } .s5 .lt { inset-inline-start: 713px; }"""

# شكل الموجة في صندوق 256×168 (الزاوية العليا اليمنى)؛ الطبقات الداخلية مصغّرة نحو الزاوية
WAVE = 'M0,0 C22,26 92,30 136,56 C168,76 168,122 192,150 C208,168 238,174 256,146 L256,0 Z'


def blob(cls):
    return ('<div class="blob %s"><svg viewBox="0 0 256 168" preserveAspectRatio="none" aria-hidden="true">'
            '<path class="w1" d="%s"/>'
            '<path class="w2" d="%s" transform="translate(256 0) scale(.72) translate(-256 0)"/>'
            '<path class="w3" d="%s" transform="translate(256 0) scale(.42) translate(-256 0)"/>'
            '<path class="ln" d="M60,0 C78,18 124,26 152,48 C182,72 184,112 204,130 C220,146 242,146 256,128"/>'
            '<path class="ln" d="M150,0 C160,14 186,22 202,40 C220,60 220,84 236,96 C244,102 252,102 256,98"/>'
            '</svg></div>' % (cls, WAVE, WAVE, WAVE))


def leaf(x, y, rx, ry, rot):
    # ورقة لوزية مدبّبة الطرفين على محورها الطويل
    if ry > rx: rx, ry, rot = ry, rx, rot + 90
    return ('<path d="M%d,%d Q%d,%.1f %d,%d Q%d,%.1f %d,%d Z" transform="rotate(%d %d %d)"/>'
            % (x - rx, y, x, y - ry * 1.7, x + rx, y, x, y + ry * 1.7, x - rx, y, rot, x, y))


def twig(x, y, w, h, leaves, stem):
    # غصن باهت: ساق منحنية وأوراق بيضاوية؛ الإحداثيات داخل صندوق 100×100 تُمطّ إلى w×h
    return ('<svg class="twig" style="top:%dpx;inset-inline-start:%dpx;width:%dpx;height:%dpx" viewBox="0 0 100 100" preserveAspectRatio="none" aria-hidden="true">'
            '<path class="st" d="%s" vector-effect="non-scaling-stroke"/>%s</svg>'
            % (y, x, w, h, stem, ''.join(leaf(*l) for l in leaves)))


SPRIG = [(22, 18, 13, 9, -35), (62, 14, 13, 9, 35), (16, 46, 13, 9, -20), (70, 42, 14, 9, 25),
         (24, 74, 14, 9, -15), (74, 70, 14, 9, 15), (48, 8, 7, 12, 0)]
STEM = 'M48,100 C46,70 50,40 48,10'

COVER = ('\n    ' + blob('tr') + blob('bl')
         + twig(140, 150, 300, 330, SPRIG, STEM) + twig(530, 150, 330, 370, SPRIG, STEM) + twig(925, 160, 230, 290, SPRIG, STEM)
         + '\n    <h1 class="cv"><span class="gt">§SWOT§</span></h1>\n  ')


def detail(n):
    # الأغصان خلف الحرف مباشرة
    tw = {2: (360, 125, 130, 130), 3: (478, 92, 190, 190), 4: (620, 125, 150, 140), 5: (790, 110, 140, 130)}[n]
    return ('\n    ' + blob('tr sm') + blob('bl sm') + twig(tw[0], tw[1], tw[2], tw[3], SPRIG, STEM)
            + '\n    <p class="lt" aria-hidden="true"><span class="gt">§L%d§</span></p>'
              '\n    <h2 class="hd"><span class="gt">§H%d§</span></h2>'
              '\n    <p class="col c1">§T%dA§</p><p class="col c2">§T%dB§</p>\n  ' % (n, n, n, n))


SLIDES = [COVER] + [detail(n) for n in (2, 3, 4, 5)]

_EN_A = 'Insert your text here. Insert your text here. Insert your text here. Insert your text here. Insert your text here.'
_EN_B = 'Insert your desired text here. Insert your text here. Insert your desired text here. Insert your text here. Insert your text here.'
_AR_A = 'أدخل النص هنا. أدخل النص هنا. أدخل النص هنا. أدخل النص هنا. أدخل النص هنا.'
_AR_B = 'أدخل النص المطلوب هنا. أدخل النص هنا. أدخل النص المطلوب هنا. أدخل النص هنا. أدخل النص هنا.'
STR = {
    'ar': {'SWOT': 'سوات', 'L2': 'ق', 'L3': 'ض', 'L4': 'ف', 'L5': 'ت',
           'H2': 'نقاط القوة', 'H3': 'نقاط الضعف', 'H4': 'الفرص', 'H5': 'التهديدات',
           'T2A': _AR_A, 'T2B': _AR_A, 'T3A': _AR_B, 'T3B': _AR_B, 'T4A': _AR_A, 'T4B': _AR_A, 'T5A': _AR_B, 'T5B': _AR_B},
    'en': {'SWOT': 'SWOT', 'L2': 'S', 'L3': 'W', 'L4': 'O', 'L5': 'T',
           'H2': 'Strengths', 'H3': 'Weaknesses', 'H4': 'Opportunities', 'H5': 'Threats',
           'T2A': _EN_A, 'T2B': _EN_A, 'T3A': _EN_B, 'T3B': _EN_B, 'T4A': _EN_A, 'T4B': _EN_A, 'T5A': _EN_B, 'T5B': _EN_B},
}

GLOBAL_DEFS = ('<defs>'
               '<linearGradient id="swl-b1" x1="0" y1="1" x2="1" y2="0"><stop offset="0" stop-color="#5E44F2"/><stop offset=".6" stop-color="#8A35DE"/><stop offset="1" stop-color="#B02CC4"/></linearGradient>'
               '<linearGradient id="swl-b2" x1="0" y1="1" x2="1" y2="0"><stop offset="0" stop-color="#9B2FD2"/><stop offset="1" stop-color="#F02A92"/></linearGradient>'
               '<linearGradient id="swl-b3" x1="0" y1="1" x2="1" y2="0"><stop offset="0" stop-color="#F72A88"/><stop offset="1" stop-color="#FF2366"/></linearGradient>'
               '</defs>')

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'mgmt', 'icon': '🌿',
    'name': 'تخطيط سوات المتحرك', 'nameEn': 'Animated SWOT Layout',
    'description': 'خمس شرائح: غلاف بكلمة سوات متدرّجة، ثم شريحة لكل من القوة والضعف والفرص والتهديدات بعمودي نص',
    'descriptionEn': 'Five slides: gradient SWOT cover, then one slide each for strengths, weaknesses, opportunities and threats',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'نقاط القوة', 'titleEn': 'Strengths'},
        {'title': 'نقاط الضعف', 'titleEn': 'Weaknesses'},
        {'title': 'الفرص', 'titleEn': 'Opportunities'},
        {'title': 'التهديدات', 'titleEn': 'Threats'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص فقط؛ الحروف والعناوين نص متدرّج (background-clip:text) والموجتان والأغصان SVG زخرفية ثابتة.',
}

FONTS = {'ar': '<link href="https://fonts.googleapis.com/css2?family=Tajawal:wght@400;500;700;800&display=swap" rel="stylesheet">',
         'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@400;600;700;800&display=swap" rel="stylesheet">'}
FONT_STACK = {'ar': "'Tajawal', 'Segoe UI', Tahoma, sans-serif", 'en': "'Open Sans', 'Segoe UI', Arial, sans-serif"}
