# -*- coding: utf-8 -*-
# نموذج «قاعدة الميزانية 50-30-20»: أربع شرائح بالتخطيط نفسه؛ حلقة دائرية بثلاث شرائح (الاحتياجات 50٪، الرغبات 30٪،
# المدخرات 20٪) تُرسم بـ JS من chart-data، وثلاثة عناصر جانبية بأيقونة وعنوان ونص. الشريحة الأولى تُظهر الكل،
# وكل شريحة تالية تُبرز قطاعًا واحدًا وعنصره وتُرمِّد الباقي.
KEY = 'sl-50-30-20-budget-rule'
TITLE = {'ar': 'قاعدة الميزانية 50-30-20', 'en': '50-30-20 Budget Rule'}
COMMENT = 'نموذج «قاعدة الميزانية 50-30-20»: عنوان أعلى، حلقة دائرية SVG في المنتصف بثلاثة قطاعات (برتقالي/أزرق/رمادي داكن) تُرسم من كتلة chart-data، ودائرة بيضاء في المركز، وثلاثة عناصر جانبية بأيقونة وعنوان ونص. السمة data-on على الرسم والصنف on على العنصر يحددان القطاع المُبرز.'
VARS = '--ink: #1F2328; --ink-2: #3D4248; --accent: #1B9CE0; --orange: #F0A030; --blue: #1B9CE0; --dark: #4A4A4A; --gray: #B8B8B8; --gray-2: #C4C4C4; --bg: #FFFFFF;'

STYLE = """  .ttl { position: absolute; top: 40px; inset-inline-start: 64px; margin: 0; font-size: 40px; font-weight: 300; line-height: 1.2; color: var(--ink); letter-spacing: -0.2px; }
  html[lang="ar"] .ttl { font-weight: 400; }
  /* الحلقة الدائرية: تُرسم بـ JS من chart-data؛ data-on = رقم القطاع المُبرز أو all */
  .donut { position: absolute; top: 150px; inset-inline-start: 370px; width: 500px; height: 500px; direction: ltr; }
  .donut .lab { font-family: var(--font); font-weight: 700; font-size: 40px; fill: #fff; }
  .donut .lab.off { fill: #8E8E8E; }
  .donut .pct { font-size: 22px; }
  .donut .hole { fill: url(#g-hole); filter: url(#sh); }
  .donut .mid { font-family: var(--font); font-weight: 700; font-size: 24px; fill: var(--ink-2); }
  /* العناصر الجانبية */
  .it { position: absolute; width: 240px; display: flex; flex-direction: column; align-items: flex-start; }
  .it .ic { width: 60px; height: 60px; }
  .it .ic svg { width: 100%; height: 100%; }
  .it h2 { margin: 14px 0 8px; font-size: 28px; font-weight: 700; line-height: 1.1; }
  .it p { margin: 0; font-size: 17px; line-height: 23px; color: var(--ink-2); }
  .it1 { top: 315px; inset-inline-end: 947px; align-items: flex-end; text-align: end; color: var(--orange); }
  .it2 { top: 150px; inset-inline-start: 912px; color: var(--blue); }
  .it3 { top: 465px; inset-inline-start: 912px; color: var(--ink-2); }
  .it.off { color: var(--gray); } .it.off p { color: #BDBDBD; }"""

ICONS = {
    # منزل بداخله حقيبة (الاحتياجات)
    'home': '<svg viewBox="0 0 48 48" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 23 24 7l19 16"/><path d="M10 20v20h28V20"/><rect x="17" y="26" width="14" height="10" rx="1.5"/><path d="M21 26v-2.5h6V26M17 30.5h14"/></svg>',
    # حقيبة تسوّق (الرغبات)
    'bag': '<svg viewBox="0 0 48 48" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M9 15h30l-2.5 27h-25z"/><path d="M17.5 15v-3a6.5 6.5 0 0 1 13 0v3"/><path d="M17.5 20v2M30.5 20v2"/></svg>',
    # صندوق كنز (المدخرات)
    'chest': '<svg viewBox="0 0 48 48" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 24v15h38V24"/><path d="M5 24v-5a8 8 0 0 1 8-8h22a8 8 0 0 1 8 8v5z"/><path d="M5 24h38"/><rect x="20" y="22" width="8" height="7" rx="1"/><path d="M24 25.5v1.5"/><circle cx="13" cy="15.5" r="1.6"/><circle cx="24" cy="14" r="1.6"/><circle cx="35" cy="15.5" r="1.6"/><path d="M44 10v2M43 11h2M4 36v2M3 37h2"/></svg>',
}

GLOBAL_DEFS = ('<defs>'
        '<linearGradient id="g-1" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#F3A638"/><stop offset="1" stop-color="#CD7C0C"/></linearGradient>'
        '<linearGradient id="g-2" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#2BA7E7"/><stop offset="1" stop-color="#1C6DA9"/></linearGradient>'
        '<linearGradient id="g-3" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#5E5E5E"/><stop offset="1" stop-color="#434343"/></linearGradient>'
        '<linearGradient id="g-off" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#EAEAEA"/><stop offset="1" stop-color="#C0C0C0"/></linearGradient>'
        '<radialGradient id="g-hole" cx=".5" cy=".4" r=".7"><stop offset="0" stop-color="#FFFFFF"/><stop offset="1" stop-color="#EFEFEF"/></radialGradient>'
        '<filter id="sh" x="-20%" y="-20%" width="140%" height="140%"><feDropShadow dx="0" dy="5" stdDeviation="7" flood-color="#000" flood-opacity=".28"/></filter>'
        '</defs>')

JSON_BLOCKS = {
    'chart-data': {
        'ar': {'labels': ['الاحتياجات', 'الرغبات', 'المدخرات'], 'values': [50, 30, 20], 'fills': ['g-1', 'g-2', 'g-3']},
        'en': {'labels': ['Needs', 'Wants', 'Savings'], 'values': [50, 30, 20], 'fills': ['g-1', 'g-2', 'g-3']},
    }
}

# رسم الحلقة: القطاع الأول يبدأ من الأسفل ويدور نحو جهة البداية (يسارًا في LTR ويمينًا في RTL) بنصف قطر أكبر كما في الأصل
EXTRA_SCRIPT = """  function drawDonut() {
    var data = JSON.parse(document.getElementById('chart-data').textContent);
    var cx = 250, cy = 250, hole = 95, total = data.values.reduce(function (a, b) { return a + b; }, 0), m = RTL ? -1 : 1;
    function pt(r, a) { a = a * Math.PI / 180; return (cx + m * r * Math.sin(a)).toFixed(1) + ',' + (cy - r * Math.cos(a)).toFixed(1); }
    Array.prototype.forEach.call(document.querySelectorAll('.donut'), function (svg) {
      var on = svg.getAttribute('data-on') || 'all', a0 = 180, paths = '', labels = '';
      data.values.forEach(function (v, i) {
        var sweep = v / total * 360, a1 = a0 + sweep, r = i === 0 ? 238 : 220, act = on === 'all' || +on === i + 1;
        paths += '<path d="M' + cx + ',' + cy + ' L' + pt(r, a0) + ' A' + r + ',' + r + ' 0 ' + (sweep > 180 ? 1 : 0) + ' ' + (RTL ? 0 : 1) + ' ' + pt(r, a1) + ' Z" fill="url(#' + (act ? data.fills[i] : 'g-off') + ')" stroke="#fff" stroke-width="3"/>';
        var mid = pt(160, a0 + sweep / 2).split(',');
        labels += '<text class="lab' + (act ? '' : ' off') + '" x="' + mid[0] + '" y="' + (+mid[1] + 14) + '" text-anchor="middle">' + v + '<tspan class="pct">%</tspan></text>';
        a0 = a1;
      });
      svg.innerHTML = paths + labels + '<circle class="hole" cx="' + cx + '" cy="' + cy + '" r="' + hole + '"/>';
    });
  }
  drawDonut();"""


def item(n, icon, on):
    return ('<div class="it it%d%s"><div class="ic">%s</div><h2>§H%d§</h2><p>§P%d§</p></div>'
            % (n, '' if on else ' off', ICONS[icon], n, n))


def slide(on):  # on: رقم القطاع المُبرز (1 الاحتياجات، 2 الرغبات، 3 المدخرات) أو 'all'
    return ('\n    <h1 class="ttl">§TTL§</h1>\n    <svg class="donut" viewBox="0 0 500 500" data-on="%s" aria-hidden="true"></svg>\n    '
            '<div class="mid"><span>§MID1§</span><span>§MID2§</span></div>\n    ' % on
            + item(1, 'home', on in ('all', 1)) + '\n    ' + item(2, 'bag', on in ('all', 2)) + '\n    ' + item(3, 'chest', on in ('all', 3)) + '\n  ')


STYLE += """
  .mid { position: absolute; top: 355px; inset-inline-start: 522px; width: 190px; height: 86px; display: flex; flex-direction: column; align-items: center; justify-content: center; text-align: center; font-size: 24px; font-weight: 700; line-height: 1.25; color: var(--ink-2); }"""

SLIDES = [slide('all'), slide(1), slide(2), slide(3)]

# علامة LRM (\u200e) قبل الأرقام في العنوان العربي حتى لا تنعكس بعد الحروف العربية
STR = {
    'ar': {'TTL': 'قالب شريحة قاعدة الميزانية \u200e50-30-20', 'MID1': 'قاعدة', 'MID2': '50-30-20',
           'H1': 'الاحتياجات', 'P1': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
           'H2': 'الرغبات', 'P2': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
           'H3': 'المدخرات', 'P3': 'هذا نص توضيحي. أضف النص المطلوب هنا.'},
    'en': {'TTL': '50-30-20 Budget Rule Slide Template', 'MID1': '50-30-20', 'MID2': 'RULE',
           'H1': 'Needs', 'P1': 'This is a sample text. Insert your desired text here.',
           'H2': 'Wants', 'P2': 'This is a sample text. Insert your desired text here.',
           'H3': 'Savings', 'P3': 'This is a sample text. Insert your desired text here.'},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'finance', 'icon': '💰',
    'name': 'قاعدة الميزانية 50-30-20', 'nameEn': '50-30-20 Budget Rule',
    'description': 'أربع شرائح لحلقة الميزانية 50-30-20: نظرة عامة ثم إبراز الاحتياجات والرغبات والمدخرات',
    'descriptionEn': 'Four slides of the 50-30-20 budget donut: overview, then needs, wants and savings highlighted',
    'sections': [
        {'title': 'القاعدة كاملة', 'titleEn': 'Full Rule', 'needsData': True},
        {'title': 'الاحتياجات (50٪)', 'titleEn': 'Needs (50%)', 'needsData': True},
        {'title': 'الرغبات (30٪)', 'titleEn': 'Wants (30%)', 'needsData': True},
        {'title': 'المدخرات (20٪)', 'titleEn': 'Savings (20%)', 'needsData': True},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص والأرقام فقط؛ نسب الحلقة في كتلة JSON بالمعرّف chart-data (values) وتُرسم بـ JS، والسمة data-on على الرسم والصنف off على العناصر يحددان القطاع المُبرز في كل شريحة.',
}
