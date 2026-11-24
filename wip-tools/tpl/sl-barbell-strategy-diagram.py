# -*- coding: utf-8 -*-
# نموذج «مخطط استراتيجية بار الأثقال»: شريحتان بالتخطيط نفسه (فاتحة ثم داكنة): دائرة خلفية في المنتصف وعليها بار أثقال
# بثلاثة أقراص ملوّنة في كل طرف، وستة عناصر (ثلاثة في كل جانب) بشريط ملوّن وعنوان ونص، موصولة بالأقراص بخطوط رفيعة.
KEY = 'sl-barbell-strategy-diagram'
TITLE = {'ar': 'مخطط استراتيجية بار الأثقال', 'en': 'Barbell Strategy Diagram'}
COMMENT = 'نموذج «مخطط استراتيجية بار الأثقال»: دائرة رمادية مركزية وبار أثقال SVG متماثل بثلاثة أقراص (أصفر، برتقالي، أزرق) في كل طرف، ستة عناصر بشريط عمودي ملوّن وعنوان ونص (الجانب الأول محاذاة البداية والثاني محاذاة النهاية) وخطوط موصّلة رمادية. الشريحة الثانية نسخة داكنة. الرسم متماثل فلا يحتاج عكسًا للعربية.'
VARS = '--ink: #111111; --ink-2: #404040; --accent: #2E7FAE; --blue: #2E7FAE; --blue-t: #24597A; --orange: #E35B33; --orange-t: #E05A38; --yellow: #EBB142; --yellow-t: #EBB142; --disc: #F2F2F2; --line: #A6A6A6; --rod: #3C3C3C; --bg: #FFFFFF;'

STYLE = """  .s2 { background: #2C3749; --ink: #FFFFFF; --ink-2: #EEF0F3; --disc: #253042; --line: #7F8898; --rod: #161E2A; --blue-t: #FFFFFF; --orange-t: #FFFFFF; --yellow-t: #FFFFFF; }
  .ttl { position: absolute; top: 41px; inset-inline-start: 64px; margin: 0; font-size: 41px; font-weight: 400; line-height: 1.3; color: var(--ink); }
  html[lang="ar"] .ttl { font-weight: 500; }
  .disc { position: absolute; top: 145px; inset-inline-start: 390px; width: 500px; height: 500px; border-radius: 50%; background: var(--disc); }
  .art { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; }
  .art .ln { fill: none; stroke: var(--line); stroke-width: 1.2; }
  .art .rod { fill: var(--rod); } .art .cap { fill: #5A5A5A; }
  .s2 .art .cap { fill: #5B5F66; }
  .art .py { fill: #EBB142; stroke: #9A6A16; } .art .po { fill: #E35B33; stroke: #8C2B13; } .art .pb { fill: #235F80; stroke: #143D55; }
  .s2 .art .pb { fill: #26607F; stroke: #1B4A63; }
  .art .p { stroke-width: 2.5; }
  .art .hy { fill: #F5D48C; } .art .ho { fill: #F29C7A; } .art .hb { fill: #7FA8C1; }
  .art .dy { fill: #A07D33; } .art .do { fill: #A23C1E; } .art .db { fill: #163F57; }
  .ctr { position: absolute; top: 283px; inset-inline-start: 520px; width: 240px; margin: 0; text-align: center; font-size: 22px; font-weight: 700; line-height: 30px; color: var(--ink); }
  /* العناصر الستة */
  .it { position: absolute; width: 300px; height: 98px; }
  .it .bar { position: absolute; top: 0; width: 14px; height: 98px; border-radius: 3px; background: var(--c); }
  .it h2 { position: absolute; top: -4px; margin: 0; font-size: 26px; font-weight: 700; line-height: 38px; color: var(--t); white-space: nowrap; }
  .it p { position: absolute; top: 49px; width: 240px; margin: 0; font-size: 18px; line-height: 23px; color: var(--ink-2); }
  html[lang="ar"] .it p { top: 46px; line-height: 27px; }
  .a { inset-inline-start: 64px; } .a .bar { inset-inline-start: 0; } .a h2 { inset-inline-start: 47px; } .a p { inset-inline-start: 45px; }
  .b { inset-inline-end: 64px; } .b .bar { inset-inline-end: 0; } .b h2 { inset-inline-end: 46px; } .b p { inset-inline-end: 45px; text-align: end; }
  .k1 { top: 174px; --c: var(--blue); --t: var(--blue-t); } .k2 { top: 346px; --c: var(--orange); --t: var(--orange-t); } .k3 { top: 518px; --c: var(--yellow); --t: var(--yellow-t); }"""


def plates(mirror):
    # ثلاثة أقراص لكل طرف؛ mirror يعكس الإحداثيات حول محور الرسم x=641
    f = (lambda x, w: 1281 - x - w) if mirror else (lambda x, w: x)
    out = ''
    for cls, x, y, w, h, hl, dk in (('py', 446, 369, 28, 134, 'hy', 'dy'), ('po', 475, 358, 28, 156, 'ho', 'do'), ('pb', 504, 345, 28, 181, 'hb', 'db')):
        X = f(x, w)
        out += '<rect class="p %s" x="%d" y="%d" width="%d" height="%d" rx="5"/>' % (cls, X, y, w, h)
        out += '<rect class="%s" x="%d" y="%d" width="16" height="8" rx="1.5"/>' % (hl, X + 6, y + 5)
        out += '<rect class="%s" x="%d" y="%d" width="16" height="8" rx="1.5"/>' % (dk, X + 6, y + h - 14)
    return out


ART = ('<svg class="art" viewBox="0 0 1280 720" aria-hidden="true">'
       # الخطوط الموصّلة (متماثلة حول x=641)
       '<path class="ln" d="M330,194 H519 V336"/><path class="ln" d="M951,194 H763 V336"/>'
       '<path class="ln" d="M225,340 V316 H489 V350"/><path class="ln" d="M1057,340 V316 H793 V350"/>'
       '<path class="ln" d="M288,537 H457 V510"/><path class="ln" d="M994,537 H825 V510"/>'
       # العمود والسدّادات والأقراص
       '<rect class="rod" x="419" y="429" width="443" height="12" rx="6"/>'
       '<rect class="cap" x="433" y="414" width="14" height="42"/><rect class="cap" x="834" y="414" width="14" height="42"/>'
       + plates(False) + plates(True) + '</svg>')


def item(side, k):
    return '<div class="it %s k%d"><div class="bar"></div><h2>§H§</h2><p>§P§</p></div>' % (side, k)


def slide():
    return ('\n    <h1 class="ttl">§TTL§</h1>\n    <div class="disc"></div>\n    ' + ART
            + '\n    <p class="ctr">§CTR§</p>\n    '
            + ''.join(item('a', k) for k in (1, 2, 3)) + '\n    ' + ''.join(item('b', k) for k in (1, 2, 3)) + '\n  ')


SLIDES = [slide(), slide()]

STR = {
    'ar': {'TTL': 'قالب مخطط استراتيجية بار الأثقال', 'CTR': 'مخطط استراتيجية بار الأثقال',
           'H': 'عنوان نائب', 'P': 'هذا نص توضيحي. أضف النص المطلوب هنا.'},
    'en': {'TTL': 'Barbell Strategy Diagram PowerPoint Template', 'CTR': 'Barbell Strategy Diagram',
           'H': 'Placeholder', 'P': 'This is a sample text. Insert your desired text here.'},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'finance', 'icon': '🏋️',
    'name': 'مخطط استراتيجية بار الأثقال', 'nameEn': 'Barbell Strategy Diagram',
    'description': 'شريحتان (فاتحة وداكنة) لبار أثقال بثلاثة أقراص في كل طرف وستة عناصر نصية موصولة بها',
    'descriptionEn': 'Two slides (light and dark): a barbell with three plates per side linked to six text items',
    'sections': [
        {'title': 'المخطط (خلفية فاتحة)', 'titleEn': 'Diagram (Light)'},
        {'title': 'المخطط (خلفية داكنة)', 'titleEn': 'Diagram (Dark)'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص فقط؛ بار الأثقال والخطوط الموصّلة رسم SVG ثابت، والعناصر الستة بالترتيب: الأزرق ثم البرتقالي ثم الأصفر في كل جانب.',
}

FONTS = {'ar': '<link href="https://fonts.googleapis.com/css2?family=Tajawal:wght@400;500;700;800&display=swap" rel="stylesheet">',
         'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@400;600;700;800&display=swap" rel="stylesheet">'}
FONT_STACK = {'ar': "'Tajawal', 'Segoe UI', Tahoma, sans-serif", 'en': "'Open Sans', 'Segoe UI', Arial, sans-serif"}
