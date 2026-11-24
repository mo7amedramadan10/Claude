# -*- coding: utf-8 -*-
# نموذج «قمع مقلوب بثلاث مراحل»: أربع شرائح بالتخطيط نفسه، تختلف في المرحلة المُبرزة (الكل، السفلى، الوسطى، العليا).
KEY = 'sl-3-stage-inverted-funnel'
TITLE = {'ar': 'قمع مقلوب بثلاث مراحل', 'en': '3-Stage Inverted Funnel'}
COMMENT = 'نموذج «قمع مقلوب بثلاث مراحل»: عنوان أعلى، قمع ثلاثي الأبعاد SVG في المنتصف بثلاثة أقراص وأسهم، وثلاثة عناصر بأيقونة دائرية وعنوان ونص وخط سفلي. الصنف on يُبرز المرحلة والعنصر بالأزرق.'
VARS = '--ink: #1F2328; --ink-2: #3D4248; --accent: #2F7BF0; --blue-2: #1D5FD1; --gray: #C8CACD; --gray-2: #9A9EA3; --bg: #FFFFFF;'

STYLE = """  .ttl { position: absolute; top: 40px; inset-inline-start: 64px; margin: 0; font-size: 40px; font-weight: 300; line-height: 1.2; color: var(--ink); letter-spacing: -0.2px; }
  html[lang="ar"] .ttl { font-weight: 400; }
  .it { position: absolute; display: flex; align-items: flex-start; gap: 20px; }
  .it .ic { flex: none; width: 90px; height: 90px; border-radius: 50%; border: 2px solid var(--gray); color: var(--gray); display: grid; place-items: center; }
  .it .ic svg { width: 44px; height: 44px; }
  .it .tx { border-bottom: 2px solid var(--gray); padding-bottom: 32px; margin-top: -6px; }
  .it h2 { margin: 0 0 6px; font-size: 22px; font-weight: 700; line-height: 1.25; color: var(--ink); }
  .it p { margin: 0; font-size: 17px; line-height: 1.38; color: var(--ink-2); max-width: 240px; }
  .it2 p { margin-inline-start: auto; }
  .it.on .ic { border-color: var(--accent); color: var(--accent); }
  .it.on .tx { border-bottom-color: var(--accent); }
  .it1 { top: 185px; inset-inline-start: 65px; } .it1 .tx { width: 255px; }
  .it3 { top: 535px; inset-inline-start: 65px; } .it3 .tx { width: 377px; }
  .it2 { top: 353px; inset-inline-end: 63px; flex-direction: row-reverse; } .it2 .tx { width: 310px; text-align: end; }
  .funnel { position: absolute; top: 140px; inset-inline-start: 420px; width: 440px; height: 520px; }
  /* حالات القمع: المرحلة المُبرزة تحمل on */
  .funnel .face { fill: url(#fg); } .funnel .band { fill: url(#bg); } .funnel .arrow { fill: url(#ag); } .funnel .shadow { fill: #8C9096; opacity: .35; }
  .funnel .st.on .face { fill: url(#fb); } .funnel .st.on .band { fill: url(#bb); } .funnel .st.on .arrow { fill: url(#ab); } .funnel .st.on .shadow { fill: #3E6FB5; opacity: .45; }
  .funnel .body { fill: #E6E7E9; opacity: .85; } .funnel.all .body { fill: #BBD6F6; opacity: .8; }"""

ICONS = {
    'users': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><circle cx="12" cy="7.5" r="3"/><circle cx="5.5" cy="9" r="2.3"/><circle cx="18.5" cy="9" r="2.3"/><path d="M12 11.5c-3 0-5.5 1.8-5.5 4.3V18h11v-2.2c0-2.5-2.5-4.3-5.5-4.3z"/><path d="M5.5 12.2c-2.1 0-3.9 1.3-3.9 3.1V17h3.3v-1.3c0-1.3.6-2.5 1.6-3.3a4 4 0 0 0-1-.2z"/><path d="M18.5 12.2a4 4 0 0 0-1 .2c1 .8 1.6 2 1.6 3.3V17h3.3v-1.7c0-1.8-1.8-3.1-3.9-3.1z"/></svg>',
    'gears': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M9.2 2.5h2.6l.4 2a6 6 0 0 1 1.5.9l1.9-.7 1.3 2.3-1.5 1.3a6 6 0 0 1 0 1.8l1.5 1.3-1.3 2.3-1.9-.7a6 6 0 0 1-1.5.9l-.4 2H9.2l-.4-2a6 6 0 0 1-1.5-.9l-1.9.7-1.3-2.3 1.5-1.3a6 6 0 0 1 0-1.8L4.1 7l1.3-2.3 1.9.7a6 6 0 0 1 1.5-.9zm1.3 4.3a2.5 2.5 0 1 0 0 5 2.5 2.5 0 0 0 0-5z"/><path d="M16.3 13.6h1.9l.3 1.4a4 4 0 0 1 1 .6l1.4-.5.9 1.6-1.1 1a4 4 0 0 1 0 1.2l1.1 1-.9 1.6-1.4-.5a4 4 0 0 1-1 .6l-.3 1.4h-1.9l-.3-1.4a4 4 0 0 1-1-.6l-1.4.5-.9-1.6 1.1-1a4 4 0 0 1 0-1.2l-1.1-1 .9-1.6 1.4.5a4 4 0 0 1 1-.6zm1 3.1a1.7 1.7 0 1 0 0 3.4 1.7 1.7 0 0 0 0-3.4z"/></svg>',
    'boxes': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><rect x="9.3" y="3" width="5.4" height="5.4" rx=".6"/><rect x="6.2" y="9.3" width="5.4" height="5.4" rx=".6"/><rect x="12.4" y="9.3" width="5.4" height="5.4" rx=".6"/><rect x="3" y="15.6" width="5.4" height="5.4" rx=".6"/><rect x="9.3" y="15.6" width="5.4" height="5.4" rx=".6"/><rect x="15.6" y="15.6" width="5.4" height="5.4" rx=".6"/></svg>',
}

GLOBAL_DEFS = ('<defs>'
        '<linearGradient id="fb" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#A9CDFB"/><stop offset="1" stop-color="#3F86E9"/></linearGradient>'
        '<linearGradient id="bb" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#1F64AE"/><stop offset="1" stop-color="#0F3E72"/></linearGradient>'
        '<linearGradient id="ab" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#4F93F2"/><stop offset="1" stop-color="#1C5CCB"/></linearGradient>'
        '<linearGradient id="fg" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#E4E5E7"/><stop offset="1" stop-color="#B9BCC0"/></linearGradient>'
        '<linearGradient id="bg" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#A5A9AE"/><stop offset="1" stop-color="#7C8086"/></linearGradient>'
        '<linearGradient id="ag" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#C9CCD0"/><stop offset="1" stop-color="#9A9EA3"/></linearGradient>'
        '</defs>')


def arrow(cx, tip, head_y, half_head, half_shaft, base):
    pts = '%d,%d %d,%d %d,%d %d,%d %d,%d %d,%d %d,%d' % (cx, tip, cx + half_head, head_y, cx + half_shaft, head_y, cx + half_shaft, base, cx - half_shaft, base, cx - half_shaft, head_y, cx - half_head, head_y)
    tx = 0.4663 * base + 14  # تعويض skewX(-25) حول قاعدة السهم ثم إزاحة 14px يمينًا
    return ('<polygon class="shadow" points="%s" transform="translate(%.1f 6) skewX(-25)"/>' % (pts, tx)
            + '<polygon class="arrow" points="%s"/>' % pts)


def disk(cx, cy, rx, ry, band, rx2, ry2):
    y2 = cy + band
    path = 'M%d,%d A%d,%d 0 0 0 %d,%d L%d,%d A%d,%d 0 0 1 %d,%d Z' % (cx - rx, cy, rx, ry, cx + rx, cy, cx + rx2, y2, rx2, ry2, cx - rx2, y2)
    return '<path class="band" d="%s"/><ellipse class="face" cx="%d" cy="%d" rx="%d" ry="%d"/>' % (path, cx, cy, rx, ry)


def funnel(on):  # on: مجموعة المراحل المُبرزة من {1,2,3} (1 العليا)
    cls = 'funnel all' if on == {1, 2, 3} else 'funnel'
    g = lambda n: ' on' if n in on else ''
    return ('<svg class="%s" viewBox="420 140 440 520" aria-hidden="true">' % cls
            + '<polygon class="body" points="437,245 843,245 713,615 567,615"/>'
            + '<g class="st%s">' % g(3) + disk(640, 615, 75, 17, 25, 72, 16) + arrow(640, 553, 586, 27, 13, 616) + '</g>'
            + '<g class="st%s">' % g(2) + disk(640, 440, 136, 31, 28, 131, 30) + arrow(640, 362, 402, 32, 16, 441) + '</g>'
            + '<g class="st%s">' % g(1) + disk(640, 245, 203, 46, 38, 196, 44) + arrow(640, 148, 196, 48, 22, 250) + '</g>'
            + '</svg>')


def item(n, icon, on):
    return ('<div class="it it%d%s"><div class="ic">%s</div><div class="tx"><h2>§H%d§</h2><p>§P%d§</p></div></div>'
            % (n, ' on' if on else '', ICONS[icon], n, n))


def slide(on):  # on: مجموعة الأرقام المُبرزة؛ العنصر 1 ↔ القرص العلوي، 2 ↔ الأوسط، 3 ↔ السفلي
    return ('\n    <h1 class="ttl">§TTL§</h1>\n    ' + funnel(on) + '\n    '
            + item(1, 'users', 1 in on) + '\n    ' + item(2, 'gears', 2 in on) + '\n    ' + item(3, 'boxes', 3 in on) + '\n  ')


SLIDES = [slide({1, 2, 3}), slide({3}), slide({2}), slide({1})]

STR = {
    'ar': {'TTL': 'قالب القمع المقلوب ثلاثي المراحل',
           'H1': 'عنوان نائب', 'P1': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
           'H2': 'عنوان نائب', 'P2': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
           'H3': 'عنوان نائب', 'P3': 'هذا نص توضيحي. أضف النص المطلوب هنا.'},
    'en': {'TTL': '3-Stage Inverted Funnel PowerPoint Template',
           'H1': 'Placeholder', 'P1': 'This is a sample text. Insert your desired text here.',
           'H2': 'Placeholder', 'P2': 'This is a sample text. Insert your desired text here.',
           'H3': 'Placeholder', 'P3': 'This is a sample text. Insert your desired text here.'},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'sales', 'icon': '🔻',
    'name': 'قمع مقلوب بثلاث مراحل', 'nameEn': '3-Stage Inverted Funnel',
    'description': 'أربع شرائح لقمع مقلوب ثلاثي المراحل: نظرة عامة ثم إبراز كل مرحلة على حدة',
    'descriptionEn': 'Four slides of a 3-stage inverted funnel: overview, then each stage highlighted',
    'sections': [
        {'title': 'القمع كاملًا', 'titleEn': 'Full Funnel'},
        {'title': 'المرحلة الأولى (القاعدة)', 'titleEn': 'Stage 1 (Base)'},
        {'title': 'المرحلة الثانية (الوسط)', 'titleEn': 'Stage 2 (Middle)'},
        {'title': 'المرحلة الثالثة (القمة)', 'titleEn': 'Stage 3 (Top)'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص فقط؛ القمع رسم SVG والصنف on يحدد المرحلة والعنصر المُبرزين في كل شريحة.',
}
