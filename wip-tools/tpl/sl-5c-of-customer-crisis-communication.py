# -*- coding: utf-8 -*-
# نموذج «العناصر الخمسة (5C) للتواصل مع العملاء في الأزمات»: ثلاث شرائح بثلاثة تخطيطات للعناصر نفسها
# (الوضوح، التعاطف، الاتساق، المصداقية، التحكم): خماسي بخمسة قطاعات حول دائرة، خمس بطاقات متسلسلة بأسهم، وخمسة معيّنات متعرّجة.
KEY = 'sl-5c-of-customer-crisis-communication'
TITLE = {'ar': 'العناصر الخمسة للتواصل في الأزمات', 'en': '5C of Customer Crisis Communication'}
COMMENT = 'نموذج «5C للتواصل مع العملاء في الأزمات»: الشريحة 1 خماسي SVG بخمسة قطاعات ملوّنة حول دائرة بيضاء مع تسميات مائلة ونصوص خارجية؛ الشريحة 2 خمس بطاقات ملوّنة متسلسلة بقصّ شيفروني وأسهم رمادية؛ الشريحة 3 خمسة معيّنات متعرّجة بموصّل رمادي وخطوط إرشاد إلى العناوين. الألوان الخمسة ثابتة في المتغيّرات.'
VARS = '--ink: #1F2328; --ink-2: #3D4248; --accent: #2F5597; --c-clarity: #6FAEEA; --c-compassion: #2F5597; --c-consistency: #5FB8A0; --c-credibility: #E0A416; --c-control: #E8603C; --line: #BFBFBF; --bg: #FFFFFF;'

STYLE = """  .ttl { position: absolute; top: 40px; inset-inline-start: 64px; margin: 0; font-size: 40px; font-weight: 300; line-height: 1.2; color: var(--ink); letter-spacing: -0.2px; }
  html[lang="ar"] .ttl { font-weight: 400; }
  .full { position: absolute; inset: 0; width: 1280px; height: 720px; }
  .ico svg { width: 100%; height: 100%; display: block; }
  /* ---- الشريحة 1: الخماسي ---- */
  .penta { transform-origin: 640px 365px; }
  html[dir="rtl"] .penta { transform: scaleX(-1); }
  .penta polygon { stroke: #fff; stroke-width: 7; stroke-linejoin: round; }
  .s1 .core { position: absolute; top: 273px; inset-inline-start: 548px; width: 184px; height: 184px; border-radius: 50%; background: #fff; box-shadow: 0 4px 14px rgb(0 0 0 / .18); display: grid; place-items: center; text-align: center; }
  .s1 .core span { font-size: 18px; font-weight: 700; line-height: 1.3; color: #000; text-transform: uppercase; }
  .s1 .lab { position: absolute; width: 170px; height: 28px; margin: 0; text-align: center; color: #fff; font-size: 22px; font-weight: 700; line-height: 28px; letter-spacing: .5px; text-transform: uppercase; transform: rotate(calc(var(--r) * var(--m, 1))); }
  html[dir="rtl"] .s1 .lab { --m: -1; letter-spacing: 0; }
  .s1 .pic { position: absolute; width: 44px; height: 44px; color: #fff; }
  .s1 .tx { position: absolute; width: 260px; margin: 0; font-size: 20px; line-height: 28px; color: var(--ink-2); }
  /* ---- الشريحة 2: البطاقات ---- */
  .card { position: absolute; top: 243px; width: 213px; height: 252px; border-radius: 14px; color: #fff; padding: 47px 14px 0; text-align: center; }
  .card .ico { width: 60px; height: 60px; margin: 0 auto; }
  .card h3 { margin: 36px 0 0; font-size: 20px; font-weight: 700; line-height: 24px; text-transform: uppercase; }
  .card p { margin: 8px 0 0; font-size: 17px; line-height: 25px; }
  /* القصّ الشيفروني على جهة البداية للبطاقات 2–5، ويُعكس في العربية */
  .card.notch { clip-path: polygon(0 0, 100% 0, 100% 100%, 0 100%, 32px 50%); }
  html[dir="rtl"] .card.notch { clip-path: polygon(100% 0, 0 0, 0 100%, 100% 100%, calc(100% - 32px) 50%); }
  .chev { position: absolute; top: 345px; width: 18px; height: 50px; color: #8A8A8A; }
  html[dir="rtl"] .chev { transform: scaleX(-1); }
  .s2 .note { position: absolute; top: 546px; inset-inline-start: 0; width: 1280px; margin: 0; text-align: center; font-size: 20px; color: var(--ink-2); }
  /* ---- الشريحة 3: المعيّنات ---- */
  .link { stroke: #D9D9D9; stroke-width: 40; stroke-linejoin: round; fill: none; }
  .dia { position: absolute; width: 142px; height: 142px; border-radius: 14px; transform: rotate(45deg); display: grid; place-items: center; color: #fff; }
  .dia .ico { width: 60px; height: 60px; transform: rotate(-45deg); }
  .ln { position: absolute; border-color: var(--line); border-style: solid; border-width: 0; }
  .ln.v { border-inline-start-width: 1px; } .ln.h { border-top-width: 1px; }
  .lb { position: absolute; width: 240px; }
  .lb h3 { margin: 0 0 14px; font-size: 24px; font-weight: 700; line-height: 28px; color: var(--ink); text-transform: uppercase; }
  .lb p { margin: 0; font-size: 17px; line-height: 25px; color: var(--ink-2); }
  .lb.end { text-align: end; } .lb.mid { text-align: center; }"""

# الأيقونات الخمس (خطوط بيضاء) بالترتيب: الوضوح، التعاطف، الاتساق، المصداقية، التحكم
SVG_OPEN = '<svg viewBox="0 0 48 48" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">'
ICONS = {
    'clarity': SVG_OPEN + '<circle cx="20" cy="20" r="13"/><path d="M30 30l12 12" stroke-width="3"/><path d="M11 20q9-10 18 0q-9 10-18 0z"/><circle cx="20" cy="20" r="3"/></svg>',
    'compassion': SVG_OPEN + '<path d="M24 11c-2-4-9-4-9 2c0 4 9 9 9 9s9-5 9-9c0-6-7-6-9-2z"/><path d="M6 32c5-2 9-3 13-3h8c2 0 3.5 1.2 3.5 2.8S29 34.5 27 34.5h-7m7 0l10-4.5c2-1 4.5 0 4.5 2L28 39H18L6 41"/></svg>',
    'consistency': SVG_OPEN + '<path d="M37 21a13.5 13.5 0 0 0-24-6"/><path d="M13 8v7h7"/><path d="M11 27a13.5 13.5 0 0 0 24 6"/><path d="M35 40v-7h-7"/></svg>',
    'credibility': SVG_OPEN + '<path d="M4 18l8-6h8l6 4 6-4h8l6 6"/><path d="M26 16l-5 4.5c-1.5 1.5 1 4 3 2.5l4-3.5"/><path d="M12 12l-2 15 8 6 6 2 6-4 6-4 2-15"/><path d="M22 35l4-3m-8-1l4-3"/></svg>',
    'control': SVG_OPEN + '<circle cx="24" cy="24" r="7"/><circle cx="24" cy="24" r="15"/><path d="M24 17V4M24 44V31M17 24H4M44 24H31M19 19l-9-9M38 38l-9-9M29 19l9-9M10 38l9-9"/><circle cx="24" cy="4" r="1.6"/><circle cx="24" cy="44" r="1.6"/><circle cx="4" cy="24" r="1.6"/><circle cx="44" cy="24" r="1.6"/><circle cx="10" cy="10" r="1.6"/><circle cx="38" cy="38" r="1.6"/><circle cx="38" cy="10" r="1.6"/><circle cx="10" cy="38" r="1.6"/></svg>',
}
ORDER = ['clarity', 'compassion', 'consistency', 'credibility', 'control']  # ترتيب العناصر في الشريحتين 2 و3
COLOR = {k: 'var(--c-%s)' % k for k in ORDER}
KEYSYM = {'clarity': 'CLARITY', 'compassion': 'COMPASSION', 'consistency': 'CONSISTENCY', 'credibility': 'CREDIBILITY', 'control': 'CONTROL'}


def slide1():
    # خماسي منتظم مركزه (640,365) ونصف قطره 253: الرؤوس من القمة باتجاه عقارب الساعة
    import math
    V = [(640 + 253 * math.cos(math.radians(-90 + 72 * i)), 365 + 253 * math.sin(math.radians(-90 + 72 * i))) for i in range(5)]
    segs = [('clarity', 0, 1), ('compassion', 1, 2), ('consistency', 2, 3), ('credibility', 3, 4), ('control', 4, 0)]
    polys = ''.join('<polygon points="640,365 %.1f,%.1f %.1f,%.1f" fill="%s"/>' % (V[a][0], V[a][1], V[b][0], V[b][1], COLOR[k]) for k, a, b in segs)
    svg = '<svg class="full penta" viewBox="0 0 1280 720" aria-hidden="true">%s</svg>' % polys
    labs = [('control', 445, 198, '-36deg'), ('clarity', 665, 198, '36deg'), ('compassion', 730, 401, '-72deg'),
            ('consistency', 555, 537, '0deg'), ('credibility', 380, 401, '72deg')]
    pics = [('control', 540, 243), ('clarity', 688, 240), ('compassion', 735, 363), ('consistency', 618, 470), ('credibility', 493, 370)]
    h = ['<h1 class="ttl">§TTL§</h1>', svg, '<div class="core"><span>§CORE§</span></div>']
    h += ['<p class="lab" style="inset-inline-start:%dpx; top:%dpx; --r:%s">§%s§</p>' % (x, y, r, KEYSYM[k]) for k, x, y, r in labs]
    h += ['<div class="pic ico" style="inset-inline-start:%dpx; top:%dpx">%s</div>' % (x, y, ICONS[k]) for k, x, y in pics]
    h += ['<p class="tx" style="inset-inline-end:912px; top:158px; text-align:end">§TX§</p>',
          '<p class="tx" style="inset-inline-start:912px; top:158px">§TX§</p>',
          '<p class="tx" style="inset-inline-end:912px; top:449px; text-align:end">§TX§</p>',
          '<p class="tx" style="inset-inline-start:912px; top:449px">§TX§</p>',
          '<p class="tx" style="inset-inline-start:510px; top:611px; text-align:center">§TX§</p>']
    return '\n    ' + '\n    '.join(h) + '\n  '


def slide2():
    lefts = [64, 298, 533, 767, 1002]
    h = ['<h1 class="ttl">§TTL§</h1>']
    for i, (k, x) in enumerate(zip(ORDER, lefts)):
        h.append('<div class="card%s" style="inset-inline-start:%dpx; background:%s"><div class="ico">%s</div><h3>§%s§</h3><p>§TX2§</p></div>'
                 % (' notch' if i else '', x, COLOR[k], ICONS[k], KEYSYM[k]))
        if i:
            h.append('<svg class="chev" viewBox="0 0 18 50" style="inset-inline-start:%dpx" aria-hidden="true"><polyline points="1,1 16,25 1,49" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>' % (x + 2))
    h.append('<p class="note">§NOTE§</p>')
    return '\n    ' + '\n    '.join(h) + '\n  '


def slide3():
    centers = [(318, 228), (480, 388), (640, 228), (800, 388), (962, 228)]
    h = ['<h1 class="ttl">§TTL§</h1>',
         '<svg class="full" viewBox="0 0 1280 720" aria-hidden="true"><path class="link" d="M318,228 L480,388 L640,228 L800,388 L962,228"/></svg>']
    h += ['<div class="dia" style="inset-inline-start:%dpx; top:%dpx; background:%s"><div class="ico">%s</div></div>' % (cx - 71, cy - 71, COLOR[k], ICONS[k]) for k, (cx, cy) in zip(ORDER, centers)]
    # خطوط الإرشاد
    h += ['<div class="ln v" style="inset-inline-start:248px; top:292px; height:58px"></div>',
          '<div class="ln v" style="inset-inline-start:430px; top:470px; height:83px"></div>',
          '<div class="ln h" style="inset-inline-start:290px; top:553px; width:140px"></div>',
          '<div class="ln v" style="inset-inline-start:640px; top:345px; height:177px"></div>',
          '<div class="ln v" style="inset-inline-start:860px; top:470px; height:83px"></div>',
          '<div class="ln h" style="inset-inline-start:860px; top:553px; width:140px"></div>',
          '<div class="ln v" style="inset-inline-start:1022px; top:292px; height:58px"></div>']
    h += ['<div class="lb" style="inset-inline-start:64px; top:372px"><h3>§CLARITY§</h3><p>§TX3§</p></div>',
          '<div class="lb" style="inset-inline-start:64px; top:539px"><h3>§COMPASSION§</h3><p>§TX3§</p></div>',
          '<div class="lb mid" style="inset-inline-start:520px; top:541px"><h3>§CONSISTENCY§</h3><p>§TX3§</p></div>',
          '<div class="lb end" style="inset-inline-end:64px; top:539px"><h3>§CREDIBILITY§</h3><p>§TX3§</p></div>',
          '<div class="lb end" style="inset-inline-end:64px; top:372px"><h3>§CONTROL§</h3><p>§TX3§</p></div>']
    return '\n    ' + '\n    '.join(h) + '\n  '


SLIDES = [slide1(), slide2(), slide3()]

STR = {
    'ar': {'TTL': 'قالب شريحة 5C للتواصل مع العملاء في الأزمات', 'CORE': 'التواصل في الأزمات',
           'CLARITY': 'الوضوح', 'COMPASSION': 'التعاطف', 'CONSISTENCY': 'الاتساق', 'CREDIBILITY': 'المصداقية', 'CONTROL': 'التحكم',
           'TX': 'أدرج النص المطلوب هنا. هذا نص توضيحي.', 'TX2': 'أدرج النص المطلوب هنا.', 'TX3': 'أدرج النص المطلوب هنا. هذا نص توضيحي.',
           'NOTE': 'يمكنك تعديل هذا النص'},
    'en': {'TTL': '5C of Customer Crisis Communication Slide Template', 'CORE': 'Crisis Communication',
           'CLARITY': 'Clarity', 'COMPASSION': 'Compassion', 'CONSISTENCY': 'Consistency', 'CREDIBILITY': 'Credibility', 'CONTROL': 'Control',
           'TX': 'Insert your desired text here. This is a sample text.', 'TX2': 'Insert your desired text here.', 'TX3': 'Insert your desired text here. This is a sample text.',
           'NOTE': 'You can edit this text'},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'cx', 'icon': '📣',
    'name': 'العناصر الخمسة للتواصل في الأزمات', 'nameEn': '5C of Customer Crisis Communication',
    'description': 'ثلاث شرائح للعناصر الخمسة (الوضوح، التعاطف، الاتساق، المصداقية، التحكم): خماسي، بطاقات متسلسلة، معيّنات',
    'descriptionEn': 'Three slides of the 5C (clarity, compassion, consistency, credibility, control): pentagon, cards, diamonds',
    'sections': [
        {'title': 'الخماسي حول الدائرة', 'titleEn': 'Pentagon Around the Core'},
        {'title': 'البطاقات المتسلسلة', 'titleEn': 'Sequential Cards'},
        {'title': 'المعيّنات المتعرّجة', 'titleEn': 'Zigzag Diamonds'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص فقط؛ الخماسي والموصّل رسوم SVG، والألوان الخمسة في متغيّرات --c-* وتسميات العناصر مكررة في الشرائح الثلاث.',
}
