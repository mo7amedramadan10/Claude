# -*- coding: utf-8 -*-
# نموذج «شريحة قمع AIDA»: خمس شرائح بالتخطيط نفسه (قمع مقلوب بأربع مراحل: الانتباه، الاهتمام، الرغبة، الفعل)،
# الأولى تُظهر المراحل كلها ملوّنة، وكل شريحة بعدها تُبرز مرحلة واحدة وتُطفئ الباقي بالرمادي.
import math

KEY = 'sl-aida-funnel-slide'
TITLE = {'ar': 'شريحة قمع أيدا', 'en': 'AIDA Funnel Slide'}
COMMENT = 'نموذج «شريحة قمع AIDA»: عنوان أعلى، قمع مقلوب SVG في المنتصف (قطاع رمادي فاتح وأربعة أشرطة هلالية بأطراف مدوّرة وأسهم خضراء متدرّجة)، وتسمية HTML فوق كل شريط، وأربعة حروف كبيرة بنص توضيحي حول القمع. الصنف off يُطفئ المرحلة بالرمادي.'
VARS = '--ink: #262626; --ink-2: #2B2B2B; --muted: #828282; --muted-2: #7F7F7F; --muted-3: #BFBFBF; --band-off: #D9D9D9; --body: #F2F2F2; --navy: #0E4E95; --blue: #1878DE; --sky: #34A3F5; --light: #7FCBFC; --accent: #1878DE; --bg: #FFFFFF;'

STYLE = """  .ttl { position: absolute; top: 40px; inset-inline-start: 64px; margin: 0; font-size: 41px; font-weight: 400; line-height: 1.2; color: var(--ink); letter-spacing: -0.2px; white-space: nowrap; }
  /* القمع: رسم SVG بإحداثيات الإطار نفسها (متماثل أفقيًا فلا يحتاج عكسًا) */
  .funnel { position: absolute; top: 120px; inset-inline-start: 400px; width: 480px; height: 540px; }
  .funnel .body { fill: var(--body); }
  .funnel .band { fill: var(--band-off); }
  .funnel .b1.on { fill: var(--navy); } .funnel .b2.on { fill: var(--blue); } .funnel .b3.on { fill: var(--sky); } .funnel .b4.on { fill: var(--light); }
  .funnel .ar { fill: url(#aida-g-top); } .funnel .ar-b { fill: url(#aida-g-bottom); }
  /* تسميات الأشرطة: نص HTML فوق كل شريط */
  .lb { position: absolute; inset-inline-start: 440px; width: 400px; margin: 0; text-align: center; font-size: 22px; font-weight: 700; line-height: 26px; color: var(--muted-2); letter-spacing: .2px; }
  .lb.on { color: #fff; }
  .lb1 { top: 225px; } .lb2 { top: 320px; } .lb3 { top: 417px; } .lb4 { top: 517px; }
  /* الحروف الكبيرة والنصوص التوضيحية */
  .ltr { position: absolute; margin: 0; font-size: 68px; font-weight: 700; line-height: 1; color: var(--muted-3); }
  .ltr.l1.on { color: var(--navy); } .ltr.l2.on { color: var(--blue); } .ltr.l3.on { color: var(--sky); } .ltr.l4.on { color: var(--light); }
  html[lang="ar"] .ltr { font-weight: 800; }
  .tx { position: absolute; margin: 0; width: 250px; font-size: 17px; line-height: 23px; color: var(--muted); }
  .tx.on { color: var(--ink-2); }
  .l1 { top: 203px; inset-inline-start: 318px; } .t1 { top: 279px; inset-inline-start: 120px; text-align: end; }
  .l2 { top: 311px; inset-inline-start: 889px; } .t2 { top: 387px; inset-inline-start: 890px; text-align: start; }
  .l3 { top: 392px; inset-inline-start: 395px; } .t3 { top: 470px; inset-inline-start: 196px; text-align: end; }
  .l4 { top: 489px; inset-inline-start: 790px; } .t4 { top: 564px; inset-inline-start: 791px; text-align: start; }"""

# تدرّجات الأسهم الخضراء (خارج الشرائح حتى تُرسم في الشرائح المخفية أيضًا)
GLOBAL_DEFS = ('<defs>'
               '<linearGradient id="aida-g-top" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#EAF4E6"/><stop offset=".45" stop-color="#A9DB9F"/><stop offset="1" stop-color="#5FBA4A"/></linearGradient>'
               '<linearGradient id="aida-g-bottom" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#B9DDD6"/><stop offset=".55" stop-color="#86CAA0"/><stop offset="1" stop-color="#5AB650"/></linearGradient>'
               '</defs>')

# جسم القمع: قطاع دائري رأسه عند (640,723) بنصف قطر 550 ونصف زاوية 17°، مقطوع من الأسفل عند y=590
_R, _AX, _AY, _HA = 550.0, 640.0, 723.0, math.radians(17)
_x1, _y1 = _AX - _R * math.sin(_HA), _AY - _R * math.cos(_HA)
_hw = (_AY - 590) * math.tan(_HA)
BODY = '<path class="body" d="M%.1f,%.1f A%d,%d 0 0 1 %.1f,%.1f L%.1f,590 L%.1f,590 Z"/>' % (_x1, _y1, _R, _R, 2 * _AX - _x1, _y1, _AX + _hw, _AX - _hw)


def band(n, off, cy, rc, y_top, y_bot):
    """شريط هلالي بأطراف مدوّرة (كما في الأصل: حافة علوية شديدة التقوّس وحافة سفلية أكثر استواءً تهبط عند الطرفين).
    off: بُعد مركز الطرف عن منتصف الإطار، cy/rc: مركز الطرف ونصف قطره، y_top/y_bot: الحافتان عند المنتصف (مقاسة من الصورة)."""
    xl, xr = 640 - off, 640 + off
    t0, b0 = cy - rc, cy + rc
    ct, cb = 2 * y_top - t0, 2 * y_bot - b0   # نقطتا التحكّم بحيث يمرّ المنحنى بالحافة عند المنتصف
    d = 'M%d,%d Q640,%d %d,%d A%d,%d 0 0 1 %d,%d Q640,%d %d,%d A%d,%d 0 0 1 %d,%d Z' % (xl, t0, ct, xr, t0, rc, rc, xr, b0, cb, xl, b0, rc, rc, xl, t0)
    return '<path class="band b%d§ON%d§" d="%s"/>' % (n, n, d)


BANDS = (band(1, 191, 288, 30, 208, 269) + band(2, 150, 372, 28, 303, 365) + band(3, 106, 457, 27, 399, 460) + band(4, 60, 541, 27, 497, 558))

# الأسهم: سهم صغير يُعرَّف حول نقطة الأصل (الرأس عند y=+17) ويُنقل ويُدار؛ الجانبيان مائلان 30° نحو القمع
_SMALL = '0,17 10,3 4,3 4,-17 -4,-17 -4,3 -10,3'
ARROWS = ('<polygon class="ar" points="%s" transform="translate(640 143)"/>' % _SMALL
          + '<polygon class="ar" points="%s" transform="translate(506 166) rotate(-30)"/>' % _SMALL
          + '<polygon class="ar" points="%s" transform="translate(774 166) rotate(30)"/>' % _SMALL
          + '<polygon class="ar-b" points="629,591 651,591 651,621 664,621 640,645 616,621 629,621"/>')

FUNNEL = ('<svg class="funnel" viewBox="400 120 480 540" aria-hidden="true">' + BODY + BANDS + ARROWS + '</svg>')


def slide(on):  # on: مجموعة المراحل المُبرزة من {1,2,3,4} (1 = الانتباه في الأعلى)
    g = lambda n: ' on' if n in on else ' off'
    html = FUNNEL
    for n in range(1, 5):
        html = html.replace('§ON%d§' % n, g(n))
    parts = ['\n    <h1 class="ttl">§TTL§</h1>\n    ' + html]
    for n in range(1, 5):
        parts.append('<p class="lb lb%d%s">§B%d§</p>' % (n, g(n), n))
    for n in range(1, 5):
        parts.append('<p class="ltr l%d%s">§L%d§</p><p class="tx t%d%s">§P%d§</p>' % (n, g(n), n, n, g(n), n))
    return '\n    '.join(parts) + '\n  '


SLIDES = [slide({1, 2, 3, 4}), slide({1}), slide({2}), slide({3}), slide({4})]

_AR_P = 'هذا نص توضيحي. أضف النص المطلوب هنا.'
_EN_P = 'This is a sample text. Insert your desired text here.'
STR = {
    'ar': {'TTL': 'قالب قمع أيدا التسويقي',
           'B1': 'الانتباه', 'B2': 'الاهتمام', 'B3': 'الرغبة', 'B4': 'الفعل',
           'L1': 'A', 'L2': 'I', 'L3': 'D', 'L4': 'A',
           'P1': _AR_P, 'P2': _AR_P, 'P3': _AR_P, 'P4': _AR_P},
    'en': {'TTL': 'AIDA Funnel PowerPoint Template',
           'B1': 'Awareness', 'B2': 'Interest', 'B3': 'Desire', 'B4': 'Action',
           'L1': 'A', 'L2': 'I', 'L3': 'D', 'L4': 'A',
           'P1': _EN_P, 'P2': _EN_P, 'P3': _EN_P, 'P4': _EN_P},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'marketing', 'icon': '🔻',
    'name': 'شريحة قمع أيدا', 'nameEn': 'AIDA Funnel Slide',
    'description': 'خمس شرائح لقمع أيدا بأربع مراحل: نظرة عامة ثم إبراز الانتباه والاهتمام والرغبة والفعل',
    'descriptionEn': 'Five slides of a 4-stage AIDA funnel: overview, then Awareness, Interest, Desire and Action highlighted',
    'sections': [
        {'title': 'القمع كاملًا', 'titleEn': 'Full Funnel'},
        {'title': 'الانتباه', 'titleEn': 'Awareness'},
        {'title': 'الاهتمام', 'titleEn': 'Interest'},
        {'title': 'الرغبة', 'titleEn': 'Desire'},
        {'title': 'الفعل', 'titleEn': 'Action'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص فقط؛ القمع رسم SVG والصنفان on/off على الأشرطة والتسميات والحروف يحددان المرحلة المُبرزة في كل شريحة.',
}
