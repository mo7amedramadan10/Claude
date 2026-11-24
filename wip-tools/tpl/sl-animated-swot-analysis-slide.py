# -*- coding: utf-8 -*-
# نموذج «شريحة تحليل سوات المتحركة»: غلاف برتقالي بدائرة وسطى، شريحة نظرة عامة بمربع دوّار وأربع دوائر ذهبية،
# ثم أربع شرائح تفصيلية بالتخطيط نفسه (دائرة ذهبية بأيقونة + شريط العنوان + بطاقة برتقالية للنص).
KEY = 'sl-animated-swot-analysis-slide'
TITLE = {'ar': 'شريحة تحليل سوات المتحركة', 'en': 'Animated SWOT Analysis Slide'}
COMMENT = 'نموذج «شريحة تحليل سوات المتحركة»: غلاف برتقالي بحزم مائلة على شكل X ودائرة وسطى، شريحة نظرة عامة بمربع برتقالي مقطوع الزوايا وأربع دوائر ذهبية بأيقونات وعناوين، وأربع شرائح تفصيلية: دائرة ذهبية كبيرة داخل هالة وقرص وردي، شريط عنوان، وبطاقة برتقالية متدرجة للنص.'
VARS = '--ink: #2B2B2B; --accent: #E65B3B; --orange: #E65B3B; --orange-2: #CC4A2D; --gold: #D0921D; --gold-2: #EAB450; --pink: #FAE4DE; --pink-2: #FDF8F6; --gray: #F2F2F2; --bg: #FFFFFF;'

STYLE = """  /* ---- الغلاف (الشريحة 1) ---- */
  .s1 { background: linear-gradient(180deg, #E55B37 0%, #DB5230 60%, #BE3F1F 100%); }
  .cv-x { position: absolute; inset: 0; width: 1280px; height: 720px; }
  .cv-x .lay { fill: url(#cvbg); }
  .cv-x .lay.l { filter: url(#cvsl); } .cv-x .lay.r { filter: url(#cvsr); }
  .cv-glow { position: absolute; top: 60px; inset-inline-start: 340px; width: 600px; height: 600px; border-radius: 50%; background: radial-gradient(circle, rgb(255 140 110 / .45) 0%, rgb(255 140 110 / 0) 65%); }
  .cv-circle { position: absolute; top: 180px; inset-inline-start: 460px; width: 360px; height: 360px; border-radius: 50%; background: linear-gradient(225deg, #E9725A 0%, #E35C3B 60%, #E35A38 100%); box-shadow: 0 10px 34px rgb(90 20 5 / .35); display: grid; place-items: center; }
  .cv-ttl { margin: 0; font-size: 78px; font-weight: 700; color: #fff; letter-spacing: -1px; line-height: 1; }
  html[lang="ar"] .cv-ttl { font-weight: 800; letter-spacing: 0; }
  /* ---- الشعار الذهبي أعلى الشرائح 2–6 ---- */
  .banner { position: absolute; top: 0; inset-inline-start: 418px; width: 444px; height: 90px; background: linear-gradient(180deg, #EAB54B 0%, #DDA032 55%, #CF8F1A 100%); clip-path: polygon(0 0, 100% 0, calc(100% - 34px) 100%, 34px 100%); display: grid; place-items: center; }
  .bn-ttl { margin: 0; font-size: 56px; font-weight: 700; color: #fff; line-height: 1; letter-spacing: -0.5px; }
  html[lang="ar"] .bn-ttl { font-weight: 800; letter-spacing: 0; }
  /* ---- الدوائر الذهبية والأيقونات ---- */
  .gc { position: absolute; border-radius: 50%; background: linear-gradient(180deg, #EBB652 0%, #CD8B1A 100%); color: #fff; display: grid; place-items: center; }
  .gc svg { width: 54%; height: 54%; fill: none; stroke: currentColor; stroke-width: 1.1; stroke-linecap: round; stroke-linejoin: round; }
  /* ---- الشريحة 2: نظرة عامة ---- */
  .ring { position: absolute; top: -52px; inset-inline-start: 193px; width: 894px; height: 894px; border-radius: 50%; background: var(--pink); clip-path: inset(142px 0 0 0); }
  .ring2 { position: absolute; top: 40px; inset-inline-start: 285px; width: 710px; height: 710px; border-radius: 50%; background: var(--pink-2); clip-path: inset(50px 0 0 0); }
  .pin { position: absolute; top: 298px; inset-inline-start: 540px; width: 200px; height: 200px; }
  .pin .sq { fill: #E66545; } .pin .wedge { fill: #E3593A; }
  .s2 .gc { width: 88px; height: 88px; }
  .s2 .gc1 { top: 249px; inset-inline-start: 492px; } .s2 .gc2 { top: 249px; inset-inline-start: 700px; }
  .s2 .gc3 { top: 456px; inset-inline-start: 492px; } .s2 .gc4 { top: 456px; inset-inline-start: 700px; }
  .lb { position: absolute; margin: 0; font-size: 24px; font-weight: 700; color: var(--ink); line-height: 30px; white-space: nowrap; }
  .lb1, .lb3 { inset-inline-end: 687px; text-align: end; } .lb2, .lb4 { inset-inline-start: 690px; }
  .lb1, .lb2 { top: 203px; } .lb3, .lb4 { top: 561px; }
  /* ---- الشرائح 3–6: التفصيل ---- */
  .gray { position: absolute; top: 90px; inset-inline-start: 0; width: 1280px; height: 630px; background: var(--gray); }
  .sliver { width: 88px; height: 88px; inset-inline-start: -85px; } .sv1 { top: 83px; } .sv2 { top: 643px; }
  .disc { position: absolute; top: 203px; inset-inline-start: 1px; width: 404px; height: 404px; border-radius: 50%; background: linear-gradient(45deg, #F8D5CB 0%, #F5E2DC 50%, #F0ECEA 100%); }
  html[dir="rtl"] .disc { background: linear-gradient(-45deg, #F8D5CB 0%, #F5E2DC 50%, #F0ECEA 100%); }
  .chev { position: absolute; top: 218px; inset-inline-start: 0; width: 120px; height: 374px; }
  html[dir="rtl"] .chev { transform: scaleX(-1); }  /* السهم يشير نحو جهة النهاية فينعكس للعربية */
  .halo { position: absolute; top: 301px; inset-inline-start: 99px; width: 208px; height: 208px; border-radius: 50%; background: #EFEFEF; box-shadow: 0 0 0 3px rgb(255 255 255 / .75); }
  .lbar { position: absolute; top: 338px; inset-inline-start: 280px; width: 280px; height: 134px; background: linear-gradient(90deg, #F3F3F3 0%, #FAFAFA 45%, #FCFCFC 100%); }
  html[dir="rtl"] .lbar { background: linear-gradient(270deg, #F3F3F3 0%, #FAFAFA 45%, #FCFCFC 100%); }
  .lbar h2 { position: absolute; top: 0; inset-inline-start: 30px; margin: 0; line-height: 134px; font-size: 35px; font-weight: 700; color: var(--ink); white-space: nowrap; }
  /* الأصل يصغّر العناوين الطويلة لتبقى داخل الشريط */
  .lbar h2.lb-m { font-size: 28px; } .lbar h2.lb-s { font-size: 24px; }
  html[lang="ar"] .lbar h2, html[lang="ar"] .lbar h2.lb-m, html[lang="ar"] .lbar h2.lb-s { font-size: 34px; }
  .gcbig { top: 325px; inset-inline-start: 123px; width: 160px; height: 160px; }
  .gcbig svg { width: 92px; height: 92px; }
  .card { position: absolute; top: 156px; inset-inline-start: 560px; width: 740px; height: 506px; border-start-start-radius: 28px; border-end-start-radius: 28px; background: linear-gradient(45deg, #E35736 0%, #E9775A 50%, #F0A08A 100%); padding: 72px 74px 40px; color: #fff; }
  html[dir="rtl"] .card { background: linear-gradient(315deg, #E35736 0%, #E9775A 50%, #F0A08A 100%); }
  .card h3 { margin: 0 0 18px; font-size: 30px; font-weight: 700; line-height: 1.2; }
  .card p { margin: 0 0 36px; max-width: 592px; font-size: 20px; line-height: 1.27; font-weight: 400; }
  html[lang="ar"] .card p { line-height: 1.5; }"""

ICONS = {
    'people': '<svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="7.2" r="2.4"/><circle cx="5.6" cy="9" r="1.9"/><circle cx="18.4" cy="9" r="1.9"/><path d="M7.4 17.5v-1.8a4.6 4.6 0 0 1 9.2 0v1.8z"/><path d="M1.8 17.5v-1.4a3.6 3.6 0 0 1 5.4-3.1"/><path d="M22.2 17.5v-1.4a3.6 3.6 0 0 0-5.4-3.1"/></svg>',
    'chair': '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M9 2.8h6a1.3 1.3 0 0 1 1.3 1.3v6.2a1 1 0 0 1-1 1H8.7a1 1 0 0 1-1-1V4.1A1.3 1.3 0 0 1 9 2.8z"/><path d="M6.5 12.6h11v1.6a.8.8 0 0 1-.8.8H7.3a.8.8 0 0 1-.8-.8z"/><path d="M5.3 8.6v4M18.7 8.6v4M4.6 8.6h1.4M18 8.6h1.4"/><path d="M12 15v3.6"/><path d="M7.4 20.2l4.6-1.6 4.6 1.6M12 18.6v1.8"/><circle cx="7.2" cy="21" r=".7"/><circle cx="16.8" cy="21" r=".7"/><circle cx="12" cy="21.2" r=".7"/></svg>',
    'door': '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M5.5 3.5h11v17h-11z"/><path d="M8.5 5.2l8-1.7v17l-8-1.7z"/><circle cx="13.8" cy="12" r=".7"/><path d="M4 20.5h17"/></svg>',
    'chartdown': '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M3.5 3.5v17h17"/><rect x="6" y="7" width="3" height="11"/><rect x="10.5" y="10" width="3" height="8"/><rect x="15" y="13" width="3" height="5"/><path d="M11 5l8.5 8.5"/><path d="M19.5 9v4.5H15"/></svg>',
}

# قناع يقصّ أربع دوائر من زوايا المربع البرتقالي (الإحداثيات بإحداثيات الإطار كما في viewBox الرسم)
GLOBAL_DEFS = ('<defs>'
               '<linearGradient id="cvbg" gradientUnits="userSpaceOnUse" x1="0" y1="0" x2="0" y2="720"><stop offset="0" stop-color="#E55B37"/><stop offset=".6" stop-color="#DB5230"/><stop offset="1" stop-color="#BE3F1F"/></linearGradient>'
               '<filter id="cvsl" x="-20%" y="-5%" width="140%" height="110%"><feDropShadow dx="16" dy="0" stdDeviation="22" flood-color="#6E1A06" flood-opacity=".5"/></filter>'
               '<filter id="cvsr" x="-20%" y="-5%" width="140%" height="110%"><feDropShadow dx="-16" dy="0" stdDeviation="22" flood-color="#6E1A06" flood-opacity=".5"/></filter>'
               '<mask id="pinm" maskUnits="userSpaceOnUse" x="540" y="298" width="200" height="200">'
               '<rect x="540" y="298" width="200" height="200" fill="#fff"/>'
               '<circle cx="536" cy="293" r="54" fill="#000"/><circle cx="744" cy="293" r="54" fill="#000"/>'
               '<circle cx="536" cy="500" r="54" fill="#000"/><circle cx="744" cy="500" r="54" fill="#000"/>'
               '</mask></defs>')


def pinwheel():
    # أربعة مثلثات داكنة على منتصفات الأضلاع، وما بينها «X» أفتح على القطرين
    tris = [(595, 298, 685, 298, 640, 372), (595, 498, 685, 498, 640, 424), (540, 352, 540, 444, 612, 398), (740, 352, 740, 444, 668, 398)]
    wedges = ''.join('<polygon class="wedge" points="%d,%d %d,%d %d,%d"/>' % t for t in tris)
    return ('<svg class="pin" viewBox="540 298 200 200" aria-hidden="true"><g mask="url(#pinm)">'
            '<rect class="sq" x="540" y="298" width="200" height="200"/>' + wedges + '</g></svg>')


def _layers():
    # ثلاث طبقات «شيفرون» لكل جانب؛ كل طبقة تلقي ظلًا نحو الوسط (الأقرب للحافة فوق)
    out = ''
    for x0, xt in ((290, 432), (175, 320), (60, 208)):
        out += '<polygon class="lay l" points="-60,0 %d,0 %d,360 %d,720 -60,720"/>' % (x0, xt, x0)
        out += '<polygon class="lay r" points="1340,0 %d,0 %d,360 %d,720 1340,720"/>' % (1280 - x0, 1280 - xt, 1280 - x0)
    return out


COVER = ('\n    <svg class="cv-x" viewBox="0 0 1280 720" aria-hidden="true">' + _layers()
         + '</svg>\n    <div class="cv-glow"></div>\n    <div class="cv-circle"><h1 class="cv-ttl">§SWOT§</h1></div>\n  ')

OVERVIEW = ('\n    <div class="ring"></div><div class="ring2"></div>\n    '
            '<div class="banner"><h1 class="bn-ttl">§SWOT§</h1></div>\n    ' + pinwheel() + '\n    '
            '<div class="gc gc1">' + ICONS['people'] + '</div><div class="gc gc2">' + ICONS['chair'] + '</div>'
            '<div class="gc gc3">' + ICONS['door'] + '</div><div class="gc gc4">' + ICONS['chartdown'] + '</div>\n    '
            '<h2 class="lb lb1">§S§</h2><h2 class="lb lb2">§W§</h2><h2 class="lb lb3">§O§</h2><h2 class="lb lb4">§T§</h2>\n  ')


def detail(sym, icon, top_sliver=True, size=''):
    sl = ('<div class="gc sliver sv1"></div>' if top_sliver else '') + '<div class="gc sliver sv2"></div>'
    return ('\n    <div class="gray"></div>\n    <div class="banner"><h1 class="bn-ttl">§SWOT§</h1></div>\n    ' + sl
            + '\n    <div class="disc"></div>'
            '<svg class="chev" viewBox="0 0 120 374" aria-hidden="true"><polygon points="0,0 118,118 118,256 0,374" fill="#E65B3B"/>'
            '<polygon points="0,137 118,165 118,209 0,237" fill="#fff" opacity=".1"/><polygon points="0,160 118,180 118,194 0,214" fill="#fff" opacity=".06"/></svg>\n    '
            '<div class="lbar"><h2%s>§%s§</h2></div>\n    <div class="halo"></div><div class="gc gcbig">%s</div>\n    '
            '<div class="card"><h3>§PH§</h3><p>§P1§</p><p>§P2§</p></div>\n  '
            % (' class="%s"' % size if size else '', sym, ICONS[icon]))


SLIDES = [COVER, OVERVIEW, detail('S', 'people'), detail('W', 'chair', size='lb-m'), detail('O', 'door', top_sliver=False, size='lb-s'), detail('T', 'chartdown')]

_EN_P = 'This is a sample text. Insert your desired text here.'
_AR_P = 'هذا نص توضيحي. أضف النص المطلوب هنا.'
STR = {
    'ar': {'SWOT': 'سوات', 'S': 'نقاط القوة', 'W': 'نقاط الضعف', 'O': 'الفرص', 'T': 'التهديدات',
           'PH': 'عنوان نائب', 'P1': ' '.join([_AR_P] * 3), 'P2': ' '.join([_AR_P] * 2)},
    'en': {'SWOT': 'SWOT', 'S': 'STRENGTHS', 'W': 'WEAKNESSES', 'O': 'OPPORTUNITIES', 'T': 'THREATS',
           'PH': 'Placeholder', 'P1': ' '.join([_EN_P] * 3), 'P2': ' '.join([_EN_P] * 2)},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'mgmt', 'icon': '🔶',
    'name': 'شريحة تحليل سوات المتحركة', 'nameEn': 'Animated SWOT Analysis Slide',
    'description': 'ست شرائح: غلاف، نظرة عامة على العناصر الأربعة، ثم شريحة لكل من القوة والضعف والفرص والتهديدات',
    'descriptionEn': 'Six slides: cover, overview of the four elements, then one slide each for S, W, O and T',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'نظرة عامة على سوات', 'titleEn': 'SWOT Overview'},
        {'title': 'نقاط القوة', 'titleEn': 'Strengths'},
        {'title': 'نقاط الضعف', 'titleEn': 'Weaknesses'},
        {'title': 'الفرص', 'titleEn': 'Opportunities'},
        {'title': 'التهديدات', 'titleEn': 'Threats'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص فقط؛ الأشكال (المربع الدوّار، الدوائر الذهبية، السهم والبطاقة) CSS وSVG ثابتة والأيقونات SVG مضمّنة.',
}

FONTS = {'ar': '<link href="https://fonts.googleapis.com/css2?family=Tajawal:wght@400;500;700;800&display=swap" rel="stylesheet">',
         'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@400;600;700;800&display=swap" rel="stylesheet">'}
FONT_STACK = {'ar': "'Tajawal', 'Segoe UI', Tahoma, sans-serif", 'en': "'Open Sans', 'Segoe UI', Arial, sans-serif"}
