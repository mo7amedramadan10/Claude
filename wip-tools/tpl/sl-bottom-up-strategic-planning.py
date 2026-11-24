# -*- coding: utf-8 -*-
# نموذج «التخطيط الاستراتيجي التصاعدي — تحليل SWOT»: خمس شرائح بالتخطيط نفسه — حلقة رباعية ملوّنة في المنتصف بدائرة
# «SWOT» وأربع دوائر حروف، وأربع بطاقات عنوان بأيقونات وقوائم نقاط. الأولى كلها ملوّنة، ثم إبراز كل ربع على حدة والباقي رمادي.
KEY = 'sl-bottom-up-strategic-planning'
TITLE = {'ar': 'التخطيط الاستراتيجي التصاعدي — تحليل سوات', 'en': 'Bottom-up Strategic Planning SWOT'}
COMMENT = 'نموذج «التخطيط الاستراتيجي التصاعدي»: حلقة SWOT رباعية (فيروزي، كهرماني، أزرق، أحمر) حول دائرة بيضاء، دوائر حروف بارزة، خطّا فصل رماديان متقاطعان، وأربع بطاقات عنوان بدائرة أيقونة وقائمة نقاط بعلامة صح. الصنف on يلوّن الربع والصنف off يجعله رماديًا.'
VARS = '--ink: #262626; --tx: #333333; --accent: #2BCFB7; --bg: #FFFFFF;'

FONTS = {'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@400;600;700&display=swap" rel="stylesheet">'}
FONT_STACK = {'en': "'Open Sans', 'Segoe UI', Arial, sans-serif"}

# ألوان الأرباع: (داكن، فاتح، لون الحلقة، لون الحرف، رمادي الحلقة عند الإطفاء)
Q = {
    's': ('#1E9F8C', '#2BCDB5', '#2BCFB7', '#16685C', '#E9E9E9'),
    'w': ('#C3870F', '#F4AC1C', '#F6AF19', '#8A5A08', '#D9D9D9'),
    'o': ('#1B6F8E', '#22A0CF', '#2299CB', '#135C7C', '#DCDCDC'),
    't': ('#D62C13', '#EC6352', '#EF5B48', '#8F1B0E', '#F0F0F0'),
}

STYLE = """  .ttl { position: absolute; top: 39px; inset-inline-start: 64px; margin: 0; font-size: 41.8px; font-weight: 400; line-height: 1.3; color: var(--ink); white-space: nowrap; letter-spacing: -.2px; }
  html[lang="ar"] .ttl { font-size: 38px; font-weight: 500; top: 34px; }
  /* خطّا الفصل المتقاطعان */
  .sep { position: absolute; background: #D9D9D9; filter: blur(1.6px); }
  .sep.h { top: 384px; inset-inline-start: 130px; width: 985px; height: 5px; background: linear-gradient(90deg, rgb(217 217 217 / 0), #D6D6D6 25%, #D6D6D6 75%, rgb(217 217 217 / 0)); }
  .sep.v { top: 140px; inset-inline-start: 627px; width: 5px; height: 532px; background: linear-gradient(180deg, rgb(217 217 217 / 0), #D6D6D6 25%, #D6D6D6 75%, rgb(217 217 217 / 0)); }
  /* الحلقة الرباعية */
  .ring { position: absolute; top: 220px; inset-inline-start: 462px; width: 334px; height: 334px; }
  html[dir="rtl"] .ring { transform: scaleX(-1); }
  .core { position: absolute; top: 279px; inset-inline-start: 521px; width: 216px; height: 216px; border-radius: 50%; background: radial-gradient(circle at 50% 30%, #FFFFFF 0%, #F6F6F6 55%, #DDDDDD 100%); box-shadow: 0 8px 18px rgb(0 0 0 / .28); text-align: center; }
  .core b { display: block; margin-top: 60px; font-size: 50px; line-height: 58px; font-weight: 700; color: var(--ink); letter-spacing: .5px; }
  .core span { display: block; margin-top: 6px; font-size: 23px; line-height: 30px; color: var(--tx); }
  html[lang="ar"] .core b { font-weight: 800; font-size: 50px; margin-top: 56px; line-height: 64px; }
  html[lang="ar"] .core span { margin-top: -2px; }
  /* دوائر الحروف */
  .lc { position: absolute; width: 78px; height: 78px; border-radius: 50%; display: grid; place-items: center; font-size: 42px; font-weight: 700; line-height: 1; box-shadow: 0 6px 10px rgb(0 0 0 / .3); }
  html[lang="ar"] .lc { font-size: 40px; font-weight: 800; padding-bottom: 6px; }
  .lc.on { background: linear-gradient(180deg, var(--l) 0%, var(--d) 100%); color: var(--lt); }
  .lc.off { background: linear-gradient(180deg, #F7F7F7 0%, #CFCFCF 100%); color: #A6A6A6; }
  .lc.q-s { top: 236px; inset-inline-start: 469px; } .lc.q-w { top: 236px; inset-inline-start: 707px; }
  .lc.q-o { top: 459px; inset-inline-start: 469px; } .lc.q-t { top: 459px; inset-inline-start: 707px; }
  /* بطاقات العنوان: شريط متدرّج + دائرة أيقونة */
  .pill { position: absolute; width: 263px; height: 51px; border-radius: 26px; display: flex; align-items: center; margin: 0; font-size: 25.5px; font-weight: 700; line-height: 1; white-space: nowrap; }
  html[lang="ar"] .pill { font-size: 25px; font-weight: 800; padding-top: 6px; }
  .pill.on { background: linear-gradient(90deg, var(--d), var(--l)); color: #fff; }
  html[dir="rtl"] .pill.on { background: linear-gradient(270deg, var(--d), var(--l)); }
  .pill.off { background: linear-gradient(90deg, #D4D4D4, #F3F3F3); color: #A6A6A6; }
  html[dir="rtl"] .pill.off { background: linear-gradient(270deg, #D4D4D4, #F3F3F3); }
  .pill.ps { inset-inline-start: 120px; padding-inline-start: 37px; }
  .pill.pe { inset-inline-start: 897px; justify-content: flex-end; padding-inline-end: 37px; }
  .ic { position: absolute; width: 86px; height: 86px; border-radius: 50%; display: grid; place-items: center; }
  .ic.on { background: linear-gradient(180deg, var(--l), var(--d)); }
  .ic.off { background: linear-gradient(180deg, #E6E6E6, #BDBDBD); }
  .ic i { width: 60px; height: 60px; border-radius: 50%; background: radial-gradient(circle at 50% 35%, #FFFFFF, #EFEFEF); display: grid; place-items: center; box-shadow: inset 0 -2px 4px rgb(0 0 0 / .08); color: #6B6B6B; }
  .ic.off i { color: #B5B5B5; }
  .ic svg { width: 36px; height: 36px; }
  .ic.cs { inset-inline-start: 63px; } .ic.ce { inset-inline-start: 1131px; }
  .top { top: 113px; } .pill.top { top: 130px; }
  .bot { top: 573px; } .pill.bot { top: 591px; }
  /* قوائم النقاط */
  .ls { position: absolute; margin: 0; padding: 0; list-style: none; }
  .ls li { display: flex; align-items: center; gap: 11px; height: 27.7px; font-size: 17.5px; color: var(--ink); white-space: nowrap; }
  html[lang="ar"] .ls li { font-size: 18px; }
  .ls li i { flex: none; width: 19px; height: 19px; border-radius: 50%; display: grid; place-items: center; color: #fff; }
  .ls.on li i { background: linear-gradient(180deg, var(--l), var(--d)); }
  .ls.off li i { background: linear-gradient(180deg, #EBEBEB, #C9C9C9); }
  .ls li i svg { width: 13px; height: 13px; }
  .ls.ps { inset-inline-start: 139px; } .ls.pe { inset-inline-start: 917px; }
  .ls.top { top: 214px; } .ls.bot { top: 452px; }"""

# متغيّرات لون كل ربع على أصنافه
STYLE += '\n' + '\n'.join('  .q-%s { --d: %s; --l: %s; --lt: %s; }' % (k, v[0], v[1], v[3]) for k, v in Q.items())


def ic(body):
    return '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.1" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">%s</svg>' % body


ICONS = {
    's': ic('<circle cx="12" cy="8.6" r="2.3"/><circle cx="6" cy="9.8" r="1.9"/><circle cx="18" cy="9.8" r="1.9"/><path d="M8 17c0-2.4 1.8-4.2 4-4.2s4 1.8 4 4.2"/><path d="M2.6 16.6c0-2 1.5-3.4 3.4-3.4.9 0 1.6.3 2.2.7"/><path d="M21.4 16.6c0-2-1.5-3.4-3.4-3.4-.9 0-1.6.3-2.2.7"/>'),
    'w': ic('<path d="M8.5 3.5h7v8h-7z"/><path d="M6.5 11.5h11v2.2h-11z"/><path d="M5.5 8v3.5M18.5 8v3.5M12 13.7v4.3M7.5 20.5l4.5-2.5 4.5 2.5M12 18v2.5"/><circle cx="7.5" cy="21" r=".6"/><circle cx="16.5" cy="21" r=".6"/><circle cx="12" cy="21" r=".6"/>'),
    'o': ic('<path d="M6 21V3.5h11V21"/><path d="M6 3.5l7 2v17l-7-1.5"/><path d="M4 21h16"/><circle cx="11.2" cy="13" r=".5"/>'),
    't': ic('<path d="M3 3v18h18"/><rect x="6" y="6" width="3" height="13"/><rect x="11" y="10" width="3" height="9"/><rect x="16" y="14" width="3" height="5"/><path d="M15.5 4.5l4 4M19.5 5.5v3h-3"/>'),
}
CHECK = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M6 12.5l4 4 8-8.5"/></svg>'


def ring(on):
    """حلقة SVG من أربعة أرباع (احداثيات الإطار: المركز 629,387 ونصف القطر 167)."""
    cx, cy, r = 167, 167, 167
    quad = {'s': 'M167,167 V0 A167,167 0 0 0 0,167 Z', 'w': 'M167,167 H334 A167,167 0 0 0 167,0 Z',
            'o': 'M167,167 H0 A167,167 0 0 0 167,334 Z', 't': 'M167,167 V334 A167,167 0 0 0 334,167 Z'}
    body = ''.join('<path d="%s" fill="%s"/>' % (quad[k], Q[k][2] if k in on else Q[k][4]) for k in 'swot')
    return '<svg class="ring" viewBox="0 0 334 334" aria-hidden="true">%s</svg>' % body


def quadrant(k, on):
    st = 'on' if k in on else 'off'
    side = 'ps' if k in 'so' else 'pe'
    vpos = 'top' if k in 'sw' else 'bot'
    csd = 'cs' if k in 'so' else 'ce'
    K = k.upper()
    items = ''.join('<li><i>%s</i>§%s%d§</li>' % (CHECK, 'B', n) for n in (1, 2, 3, 4))
    return ('<p class="pill %s %s %s q-%s">§L%s§</p>' % (st, side, vpos, k, K)
            + '<div class="ic %s %s %s q-%s"><i>%s</i></div>' % (st, csd, vpos, k, ICONS[k])
            + '<ul class="ls %s %s %s q-%s">%s</ul>' % (st, side, vpos, k, items))


def slide(on):
    return ('\n    <h1 class="ttl">§TTL§</h1>'
            '\n    <div class="sep h"></div><div class="sep v"></div>'
            '\n    ' + ring(on) +
            '\n    <div class="core"><b>§SW§</b><span>§AN§</span></div>' +
            ''.join('\n    <div class="lc %s q-%s">§C%s§</div>' % ('on' if k in on else 'off', k, k.upper()) for k in 'swot') +
            ''.join('\n    ' + quadrant(k, on) for k in 'swot') + '\n  ')


SLIDES = [slide('swot'), slide('s'), slide('w'), slide('o'), slide('t')]

STR = {
    'ar': {'TTL': 'قالب التخطيط الاستراتيجي التصاعدي لتحليل سوات',
           'SW': 'سوات', 'AN': 'تحليل',
           'CS': 'ق', 'CW': 'ض', 'CO': 'ف', 'CT': 'ت',
           'LS': 'نقاط القوة', 'LW': 'نقاط الضعف', 'LO': 'الفرص', 'LT': 'التهديدات',
           'B1': 'عنوان نائب', 'B2': 'هذا نص توضيحي', 'B3': 'عنوان نائب', 'B4': 'نص توضيحي'},
    'en': {'TTL': 'Bottom-up Strategic Planning SWOT PowerPoint Template',
           'SW': 'SWOT', 'AN': 'Analysis',
           'CS': 'S', 'CW': 'W', 'CO': 'O', 'CT': 'T',
           'LS': 'Strengths', 'LW': 'Weaknesses', 'LO': 'Opportunities', 'LT': 'Threats',
           'B1': 'Placeholder', 'B2': 'This is a sample text', 'B3': 'Placeholder', 'B4': 'Sample Text'},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'mgmt', 'icon': '🧭',
    'name': 'تخطيط استراتيجي تصاعدي (سوات)', 'nameEn': 'Bottom-up Strategic Planning SWOT',
    'description': 'خمس شرائح لتحليل سوات بحلقة رباعية: نظرة عامة ثم إبراز القوة والضعف والفرص والتهديدات كلٌّ على حدة',
    'descriptionEn': 'Five SWOT slides with a four-part ring: overview, then strengths, weaknesses, opportunities and threats each highlighted',
    'sections': [
        {'title': 'تحليل سوات كاملًا', 'titleEn': 'Full SWOT Analysis'},
        {'title': 'نقاط القوة', 'titleEn': 'Strengths'},
        {'title': 'نقاط الضعف', 'titleEn': 'Weaknesses'},
        {'title': 'الفرص', 'titleEn': 'Opportunities'},
        {'title': 'التهديدات', 'titleEn': 'Threats'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص فقط؛ الحلقة رسم SVG، والصنفان on/off يحددان الربع المُبرز في كل شريحة.',
}
