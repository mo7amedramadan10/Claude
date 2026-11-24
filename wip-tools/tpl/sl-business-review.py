# -*- coding: utf-8 -*-
# نموذج «مراجعة الأعمال 2015»: إحدى وثلاثون شريحة بأربعة محاور لونية (أخضر تنفيذ البرنامج، فيروزي المواءمة الاستراتيجية،
# أرجواني تطوير الأعمال، ذهبي إدارة الموارد). الرسوم كلها SVG يرسمها محرك واحد من كتلة chart-data بإحداثيات الإطار نفسه
# (1280×720)، ويعكس X() الإحداثيات للعربية؛ النصوص HTML بخصائص منطقية والفقاعات والأيقونات عناصر مطلقة.
import math

KEY = 'sl-business-review'
TITLE = {'ar': 'مراجعة الأعمال 2015', 'en': 'Business Review 2015'}
COMMENT = 'نموذج «مراجعة الأعمال»: غلاف بصورة، فهرس ملوّن، فواصل أقسام بصور، لوحات 2×2 لكل محور، ثم شرائح تفصيلية لكل رسم. كل الرسوم تُرسم من كتلة chart-data (المعرّف = data-c).'
VARS = ('--ink: #595959; --accent: #13AFB7; --bg: #FFFFFF; --c1: #127A8F; --grn: #8CC63F; --grn2: #6A992B; --teal: #13AFB7; '
        '--teal2: #0B7F86; --teal3: #075A60; --mag: #A5357F; --mag2: #D57DBB; --mag3: #5B1A47; --yel: #F2C811; --yel2: #BF9A06; '
        '--gray: #7F7F7F; --red: #D9472B;')

FONTS = {'ar': '<link href="https://fonts.googleapis.com/css2?family=Tajawal:wght@400;500;700;800&display=swap" rel="stylesheet">',
         'en': '<link href="https://fonts.googleapis.com/css2?family=Arimo:wght@400;700&family=Carlito:wght@400;700&display=swap" rel="stylesheet">'}
FONT_STACK = {'ar': "'Tajawal', 'Segoe UI', Tahoma, sans-serif", 'en': "'Arimo', 'Liberation Sans', Arial, sans-serif"}

# ---------------------------------------------------------------- أدوات
def r1(v):
    return ('%.1f' % v).rstrip('0').rstrip('.')


def pos(top, start, w=None, h=None, extra=''):
    s = 'top:%spx; inset-inline-start:%spx;' % (r1(top), r1(start))
    if w is not None:
        s += ' width:%spx;' % r1(w)
    if h is not None:
        s += ' height:%spx;' % r1(h)
    return s + extra


def tx(tag, cls, top, start, w, token, extra=''):
    return '<%s class="%s" style="%s">§%s§</%s>' % (tag, cls, pos(top, start, w, extra=extra), token, tag)


CHARTS = {}


def L(en, ar):
    return {'en': en, 'ar': ar}


AR_N = {1: '1', 2: '2', 3: '3', 4: '4', 5: '5', 6: '6'}
def CAT(n): return L('Category %d' % n, 'الفئة %d' % n)
def CATU(n): return L('CATEGORY %d' % n, 'الفئة %d' % n)
def SER(n): return L('Series %d' % n, 'السلسلة %d' % n)
def SERX(n): return L('Series%d' % n, 'السلسلة %d' % n)
def SERU(n): return L('SERIES %d' % n, 'السلسلة %d' % n)
DATES = ['5/1/2002', '6/1/2002', '7/1/2002', '8/1/2002', '9/1/2002']
QTR = [L('1st Qtr', 'الربع الأول'), L('2nd Qtr', 'الربع الثاني')]
STOCK_LG = [L('Volume', 'الحجم'), L('Open', 'الافتتاح'), L('High', 'الأعلى'), L('Low', 'الأدنى'), L('Close', 'الإغلاق')]


def ch(cid, spec):
    """يسجّل مواصفة رسم ويعيد حاويته (تغطي الإطار كاملًا؛ الإحداثيات إحداثيات الإطار)."""
    CHARTS[cid] = spec
    return '<div class="ch" data-c="%s"></div>' % cid


def loc(v, lang):
    if isinstance(v, dict):
        if set(v.keys()) == {'en', 'ar'}:
            return v[lang]
        return {k: loc(x, lang) for k, x in v.items()}
    if isinstance(v, list):
        return [loc(x, lang) for x in v]
    return v


def bub(text, cx, cy, d, kind, dirn, ln, fs=22):
    """فقاعة قيمة دائرية بسهم (r أحمر، k رمادي، w أبيض)."""
    return ('<div class="bub %s %s" style="%s --l:%dpx; font-size:%dpx;"><span>%s</span><i class="sh"></i><i class="hd"></i></div>'
            % (kind, dirn, pos(cy - d / 2, cx - d / 2, d, d), ln, fs, text))


def kv(text, cx, cy, fs=27):
    return '<p class="kv" style="%s font-size:%dpx;">%s</p>' % (pos(cy - fs * 0.6, cx - 100, 200), fs, text)


def ico(name, cx, cy, size, color):
    return '<div class="ico" style="%s color:%s;">%s</div>' % (pos(cy - size / 2, cx - size / 2, size, size), color, ICONS[name])


# ---------------------------------------------------------------- الأيقونات (48×48)
def man(cx, top, s=1.0):
    r = 4.4 * s
    t = top + 2 * r + 1.3 * s
    w = 13.5 * s
    h = 15 * s
    q = 3.2 * s
    return ('<circle cx="%s" cy="%s" r="%s"/>' % (r1(cx), r1(top + r), r1(r))
            + '<path d="M%s %sq0 -%s %s -%sh%sq%s 0 %s %sv%sh-%sz"/>' % (r1(cx - w / 2), r1(t + q), r1(q), r1(q), r1(q), r1(w - 2 * q), r1(q), r1(q), r1(q), r1(h - q), r1(w))
            + '<rect x="%s" y="%s" width="%s" height="%s"/>' % (r1(cx - w / 2 + 1.6 * s), r1(t + h - 0.5), r1(w / 2 - 2.2 * s), r1(14 * s))
            + '<rect x="%s" y="%s" width="%s" height="%s"/>' % (r1(cx + 0.6 * s), r1(t + h - 0.5), r1(w / 2 - 2.2 * s), r1(14 * s)))


def bust(cx, top, s=1.0):
    r = 5 * s
    t = top + 2 * r + 1.2 * s
    return ('<circle cx="%s" cy="%s" r="%s"/>' % (r1(cx), r1(top + r), r1(r))
            + '<path d="M%s %sa%s %s 0 0 1 %s 0v%sh-%sz"/>' % (r1(cx - 8 * s), r1(t + 7 * s), r1(8 * s), r1(7 * s), r1(16 * s), r1(3 * s), r1(16 * s)))


def gear(cx, cy, ro, ri, hole, n=8):
    pts = []
    for i in range(n * 2):
        a0 = (i - 0.5) * math.pi / n
        a1 = (i + 0.5) * math.pi / n
        rr = ro if i % 2 == 0 else ri
        for a in (a0 + 0.06, a1 - 0.06):
            pts.append('%s %s' % (r1(cx + rr * math.cos(a)), r1(cy + rr * math.sin(a))))
    return ('<path fill-rule="evenodd" d="M%s Z M%s %s a%s %s 0 1 0 %s 0 a%s %s 0 1 0 -%s 0z"/>'
            % (' L'.join(pts), r1(cx - hole), r1(cy), r1(hole), r1(hole), r1(2 * hole), r1(hole), r1(hole), r1(2 * hole)))


def SV(inner):
    return '<svg viewBox="0 0 48 48" fill="currentColor" aria-hidden="true">' + inner + '</svg>'


STK = 'fill="none" stroke="currentColor" stroke-width="2.6" stroke-linecap="round" stroke-linejoin="round"'
ICONS = {
    'gear': SV(gear(24, 24, 22, 16, 7.5)),
    'cash': SV('<g %s><rect x="3" y="10" width="42" height="22" rx="2"/><path d="M3 36h42M3 40h42"/></g><circle cx="24" cy="21" r="8.5"/>'
               '<path d="M26.6 17.6c-.6-.9-1.5-1.3-2.6-1.3-1.5 0-2.6.8-2.6 2 0 2.8 5.4 1.5 5.4 4.4 0 1.3-1.1 2.1-2.8 2.1-1.3 0-2.3-.6-2.8-1.6M24 14.6v1.8M24 26v1.8" fill="none" style="stroke:var(--ib,#fff)" stroke-width="1.6"/>' % STK),
    'handcash': SV('<g %s><path d="M17 4l20 8-7 17-20-8z"/><circle cx="23.5" cy="16.5" r="3.5"/><path d="M10 26l-4 8 7 10h12l6-9-3-7-10 3"/><path d="M18 33l4-1"/></g>' % STK),
    'coins': SV(''.join('<rect x="%d" y="%s" width="14" height="4.6" rx="2.3"/>' % (x, r1(44 - 5.4 * (k + 1))) for x, cnt in ((2, 4), (17, 7), (32, 3)) for k in range(cnt))),
    'bizman': SV(man(31, 1, 1.0) + '<rect x="37" y="28" width="10" height="8" rx="1"/><circle cx="12" cy="35" r="11" fill="none" stroke="currentColor" stroke-width="2"/>'
                 + bust(7.5, 29, .42) + bust(12, 27, .5) + bust(16.5, 29, .42)),
    'swapmen': SV(man(9, 4, .8) + man(39, 4, .8) + '<g %s><path d="M19 22a6 6 0 0 1 10 0"/><path d="M29 30a6 6 0 0 1-10 0"/></g><path d="M27 18l4 4-5 1zM21 34l-4-4 5-1z"/>' % STK),
    'swap': SV(bust(14, 3, .95) + bust(33, 22, .95) + '<g %s><path d="M27 7c7 0 11 4 11 10"/><path d="M20 41c-7 0-11-4-11-10"/></g><path d="M34 15l4 6 4-6zM5 33l4-6 4 6z"/>' % STK),
    'podium': SV(bust(24, 3, 1.0) + '<path d="M31 18l6-12 3 1.5-5 13z"/><path d="M8 27h32v4H8zM12 31h24v15H12z"/><path d="M14 22l3 5" stroke="currentColor" stroke-width="2"/>'),
    'present': SV(man(35, 5, .85) + '<rect x="2" y="7" width="22" height="18" rx="1"/><path d="M5 30l6-5M21 30l-6-5" stroke="currentColor" stroke-width="2"/>'
                  '<path d="M6 21v-5h3v5zM11 21v-9h3v9zM16 21v-7h3v7z" style="fill:var(--ib,#fff)"/><path d="M24 15l6 4" stroke="currentColor" stroke-width="2.4"/>'),
    'meeting': SV('<rect x="15" y="3" width="18" height="11" rx="1"/>' + bust(7, 18, .75) + bust(41, 18, .75)
                  + '<rect x="2" y="31" width="44" height="4"/><path d="M6 35v11M42 35v11M17 35h14v11H17z"/>'),
    'team': SV(man(10, 10, .62) + man(38, 10, .62) + man(24, 3, .78) + '<path d="M22 20l2 8 2-8z" style="fill:var(--ib,#fff)"/>'),
    'walk': SV('<circle cx="27" cy="6" r="4.5"/><path d="M22 13h9l3 13-3 1-2-8-2 9 5 18h-4l-5-15-4 15h-4l5-20 1-9-4 6-3-2z"/><rect x="8" y="22" width="10" height="9" rx="1"/>'),
    'boardman': SV(man(13, 5, .9) + '<rect x="25" y="13" width="21" height="27" rx="1" fill="none" stroke="currentColor" stroke-width="2"/>'
                   '<path d="M29 35v-5h3v5zM34 35v-9h3v9zM39 35v-12h3v12z"/><path d="M19 19l7 3" stroke="currentColor" stroke-width="2.4"/>'),
    'talk': SV(man(17, 8, .9) + '<path d="M28 4h16a3 3 0 0 1 3 3v6a3 3 0 0 1-3 3h-9l-5 4 1-4h-3a3 3 0 0 1-3-3V7a3 3 0 0 1 3-3z"/>'
               '<circle cx="31" cy="10" r="1.5" style="fill:var(--ib,#fff)"/><circle cx="36" cy="10" r="1.5" style="fill:var(--ib,#fff)"/><circle cx="41" cy="10" r="1.5" style="fill:var(--ib,#fff)"/>'),
    'pctman': SV(man(13, 3, .95) + '<path d="M18 16l14-4 1 3-14 4z"/><rect x="31" y="22" width="7" height="7"/><rect x="40" y="38" width="7" height="7"/><path d="M45 20l3 2-15 24-3-2z"/>'),
    'bagman': SV(man(15, 2, .95) + '<path d="M34 28h8l-2 3c5 2 7 6 7 10 0 4-3 6-9 6s-9-2-9-6c0-4 2-8 7-10z"/>'
                 '<path d="M39.5 35v8M41.5 36.5c-.5-.6-1.2-.9-2-.9-1.1 0-1.9.6-1.9 1.5 0 2 4 1.1 4 3.1 0 .9-.8 1.5-2.1 1.5-.9 0-1.7-.4-2-1.1" fill="none" style="stroke:var(--ib,#fff)" stroke-width="1.3"/>'),
    'bag': SV('<path d="M19 3h12l-3 7c7 3 12 10 12 18 0 8-6 12-15 12S11 36 11 28c0-8 4-15 11-18z"/>'
              '<circle cx="13" cy="35" r="9.5" style="stroke:var(--ib,#fff)" stroke-width="2"/><circle cx="13" cy="35" r="5.5" fill="none" style="stroke:var(--ib,#fff)" stroke-width="1.6"/>'
              '<circle cx="24" cy="39" r="8.5" style="stroke:var(--ib,#fff)" stroke-width="2"/><circle cx="24" cy="39" r="4.8" fill="none" style="stroke:var(--ib,#fff)" stroke-width="1.6"/>'),
    'cart': SV('<g %s><path d="M2 9h6l5 21h22l4-15"/><path d="M38 8l7-5"/></g><circle cx="16" cy="38" r="3.5"/><circle cx="31" cy="38" r="3.5"/>'
               '<path d="M14 13h8v8h-8zM23 9l7 2-2 8-7-2zM31 14h7l-2 7h-6z"/>' % STK),
    'servers': SV(''.join('<rect x="%d" y="6" width="13" height="36" rx="1.5"/>' % x for x in (2, 17.5, 33))
                  + ''.join('<path d="M%d %dh9" style="stroke:var(--ib,#fff)" stroke-width="2"/>' % (x + 2, y) for x in (2, 17.5, 33) for y in (14, 20, 26))),
    'folders': SV('<path d="M4 16h11l3 3h18v17H4z"/><path d="M14 8h10l3 3h15v15h-4V14H14z"/><path d="M20 36v6M8 42h28" stroke="currentColor" stroke-width="2.4"/><rect x="34" y="4" width="11" height="9" rx="1" fill="none" stroke="currentColor" stroke-width="2"/>'),
    'netpc': SV('<rect x="2" y="4" width="18" height="13" rx="1"/><rect x="28" y="4" width="18" height="13" rx="1"/><rect x="15" y="26" width="18" height="13" rx="1"/>'
                '<path d="M11 17v5h26v-5M24 22v4M18 44h12" fill="none" stroke="currentColor" stroke-width="2.2"/><path d="M5 7h12v7H5zM31 7h12v7H31zM18 29h12v7H18z" style="fill:var(--ib,#fff)"/>'),
    'timer': SV('<circle cx="24" cy="27" r="17" fill="none" stroke="currentColor" stroke-width="4"/><path d="M24 27V15a12 12 0 0 1 12 12z"/><rect x="20" y="3" width="8" height="4"/><path d="M37 11l3-3 3 3-3 3z"/>'),
    'logo': ('<svg viewBox="0 0 72 76" aria-hidden="true"><path d="M8 12c16-8 40-8 56 0 4 18 4 38-2 56-16 6-36 6-52 0C4 50 4 30 8 12zm12 14c-2 8-2 18 1 26 9 2 19 2 28-1 2-8 2-17 1-25-10-2-20-2-30 0z" fill="#F7C51B" fill-rule="evenodd"/>'
             '<path d="M8 12c16-8 40-8 56 0l-1 9C49 16 26 16 10 22z" fill="#E1251B"/></svg>'),
}

# ---------------------------------------------------------------- الأنماط (تهشير ونقاط) للرسوم
PATS = {
    # فيروزي
    'ht1': ('h', '#0E7D84', '#3FB5BB'), 'dt1': ('d', '#8FDCE0', '#13A3AA'), 'dt2': ('d', '#86C6C8', '#3E9EA2'),
    'ht3': ('h', '#0B6168', '#2E9EA4'), 'ht4': ('h', '#2ED3DC', '#A4F1F4'),
    'dt3': ('d', '#0B6F75', '#2DB8C0'), 'dt4': ('d', '#11A3AA', '#7DD9DD'), 'dt5': ('d', '#0B5E63', '#1FA8AF'),
    'hc1': ('h', '#0B5C61', '#2E9EA4'), 'hc2': ('h', '#0F7B81', '#4BBFC5'), 'hc3': ('h', '#14AEB6', '#70E0E5'),
    'hc4': ('h', '#3FDCE4', '#A6F3F6'), 'hc5': ('h', '#7CEDF2', '#CFFAFB'),
    'ha1': ('h', '#0D5D62', '#1F8A90'), 'ha2': ('h', '#1BD3DC', '#8DF0F4'), 'ha3': ('h', '#0F8C93', '#3BB9BF'),
    'hs1': ('h', '#0B5F63', '#1E8B90'), 'hs2': ('h', '#0E7277', '#2B9AA0'), 'hs3': ('h', '#12A6AD', '#5FD5DA'),
    'hs4': ('h', '#11939A', '#4CC4CA'), 'hs5': ('h', '#13A2A9', '#62D2D7'),
    'ds1': ('d', '#0A5A5F', '#1E9097'), 'ds2': ('d', '#137A80', '#3FB5BB'), 'ds3': ('d', '#1D8C92', '#55C3C8'),
    # ذهبي
    'hy1': ('h', '#C9A208', '#E3C22E'), 'hy2': ('h', '#F2C811', '#F9E37A'),
    'dy1': ('d', '#F2C811', '#D2AA0A'), 'dy2': ('d', '#B8960A', '#8C7206'),
}


def pattern(pid, kind, bg, fg):
    if kind == 'h':
        return ('<pattern id="%s" patternUnits="userSpaceOnUse" width="5" height="5" patternTransform="rotate(45)">'
                '<rect width="5" height="5" fill="%s"/><rect width="1.7" height="5" fill="%s"/></pattern>' % (pid, bg, fg))
    return ('<pattern id="%s" patternUnits="userSpaceOnUse" width="4" height="4"><rect width="4" height="4" fill="%s"/>'
            '<circle cx="2" cy="2" r="1" fill="%s"/></pattern>' % (pid, bg, fg))


GLOBAL_DEFS = '<defs>' + ''.join(pattern(k, *v) for k, v in PATS.items()) + '</defs>'

STYLE = """  html[lang="en"] { --fa: 'Arimo', 'Liberation Sans', Arial, sans-serif; --fc: 'Carlito', Calibri, 'Arimo', sans-serif; }
  html[lang="ar"] { --fa: 'Tajawal', 'Segoe UI', Tahoma, sans-serif; --fc: 'Tajawal', 'Segoe UI', Tahoma, sans-serif; }
  .pg { position: absolute; inset: 0; }
  .pg > * { position: absolute; margin: 0; }
  .bgw { background: linear-gradient(180deg, #FFFFFF 0%, #FFFFFF 22%, #F1F1F1 100%); }
  .th-g { --th: var(--grn); } .th-t { --th: var(--teal); } .th-m { --th: var(--mag); } .th-y { --th: var(--yel); }
  /* الترويسة المشتركة */
  .hdr { top: 33px; inset-inline-start: 42px; font: 700 24px/1.2 var(--fa); color: var(--c1); white-space: nowrap; }
  .hdr .g, .lg3 .l2 { color: var(--grn); } .hdr .t, .lg3 .l3 { color: var(--teal); }
  .sec { top: 33px; inset-inline-end: 33px; font: 700 24px/1.2 var(--fa); color: var(--th); white-space: nowrap; }
  html[lang="ar"] .hdr, html[lang="ar"] .sec { top: 30px; font-size: 25px; }
  .pnl { background: #F0F0F0; }
  .sth { font: 400 24px/1.2 var(--fa); color: #595959; white-space: nowrap; }
  /* شعار «مراجعة الأعمال 2015» في الفواصل والفهرس */
  .band { top: 0; inset-inline-start: 0; width: 1280px; height: 222px; background: #F2F2F2; }
  .lg3 { top: 50px; inset-inline-start: 44px; font: 700 32px/31px var(--fa); color: var(--c1); }
  .lg3 span { display: block; white-space: nowrap; } .lg3 .l3 { font-size: 64px; line-height: 58px; letter-spacing: -1px; }
  html[lang="ar"] .lg3 { font-size: 30px; } html[lang="ar"] .lg3 .l3 { font-size: 62px; }
  .ph { top: 222px; inset-inline-start: 0; width: 1280px; height: 498px; object-fit: cover; }
  .stl { inset-inline-start: 663px; font: 700 42px/51px var(--fa); color: var(--gray); white-space: nowrap; }
  .stl .c1 { color: var(--c1); } .stl .g { color: var(--grn); } .stl .t { color: var(--teal); } .stl .m { color: var(--mag); } .stl .y { color: var(--yel); }
  /* الغلاف */
  .cvp { top: 0; inset-inline-start: 0; width: 1280px; height: 441px; object-fit: cover; }
  .cvb { top: 441px; inset-inline-start: 0; width: 1280px; height: 279px; background: linear-gradient(180deg, #2F2F2F, #242424); }
  .vd { top: 515px; inset-inline-start: 524px; width: 1px; height: 133px; background: #5C5C5C; }
  .cvl { top: 543px; inset-inline-start: 52px; display: flex; align-items: center; gap: 4px; color: #fff; font: 700 42px/1 var(--fa); letter-spacing: -1px; white-space: nowrap; }
  .cvl svg { width: 70px; height: 75px; flex: none; } .cvl span { padding-top: 16px; }
  .cvt { top: 552px; inset-inline-end: 50px; font: 700 48px/1.2 var(--fa); white-space: nowrap; }
  .cvt .b { color: #2BC4EE; } .cvt .g { color: #A8D46F; } .cvt .w { color: #EFEFEF; }
  /* الفهرس */
  .cnt { top: 126px; inset-inline-start: 563px; font: 400 58px/1.15 var(--fa); color: #595959; }
  .crow, .cnum { height: 70px; color: #fff; font: 400 24px/1.2 var(--fa); display: flex; align-items: center; }
  .crow { inset-inline-start: 552px; width: 545px; padding-inline-start: 22px; }
  .cnum { inset-inline-start: 1103px; width: 177px; justify-content: center; }
  .cpar { top: 501px; inset-inline-start: 43px; width: 428px; font: 400 24px/29px var(--fc); color: #595959; }
  /* نصوص الشرائح التفصيلية */
  .tt { font: 700 58px/1.15 var(--fa); color: var(--gray); text-shadow: 1px 2px 2px rgb(0 0 0 / .22); }
  .bd { font: 400 24px/43.5px var(--fa); color: #595959; }
  .bl { list-style: none; padding: 0; font: 400 24px/43.5px var(--fa); color: var(--th); }
  .bl li { position: relative; padding-inline-start: 30px; white-space: nowrap; }
  .bl li::before { content: '•'; position: absolute; inset-inline-start: 4px; }
  .kv { font-family: var(--fa); line-height: 1.2; color: #595959; text-align: center; white-space: nowrap; }
  .ico svg { width: 100%; height: 100%; display: block; }
  /* الرسوم والمفاتيح */
  .ch { position: absolute; inset: 0; pointer-events: none; }
  .ch svg { position: absolute; inset: 0; }
  .ch text { font-family: var(--fa); } .ch text.c { font-family: var(--fc); }
  .lg { position: absolute; display: flex; align-items: center; justify-content: center; gap: 4px 14px; color: #595959; font-family: var(--fa); white-space: nowrap; }
  .lg.c { font-family: var(--fc); } .lg.col { flex-direction: column; align-items: flex-start; justify-content: flex-start; }
  .lg span { display: inline-flex; align-items: center; }
  .lg .mk { display: inline-block; flex: none; width: 8px; height: 8px; margin-inline-end: 4px; }
  .lg .mk.ln { width: 26px; height: 3px; } .lg .mk.bx { width: 26px; height: 8px; }
  /* فقاعات القيم بسهم */
  .bub { display: grid; place-items: center; border-radius: 50%; color: #fff; font-family: var(--fa); font-weight: 700; line-height: 1; box-shadow: 0 2px 5px rgb(0 0 0 / .35); z-index: 2; --ac: #fff; }
  .bub.r { background: var(--red); --ac: var(--red); } .bub.k { background: #595959; --ac: #595959; } .bub.w { background: #fff; color: #44546A; --ac: #fff; }
  .bub .sh { position: absolute; inset-inline-start: calc(50% - 1px); width: 2px; height: var(--l); background: var(--ac); }
  .bub .hd { position: absolute; inset-inline-start: calc(50% - 5px); width: 0; height: 0; border-inline: 5px solid transparent; }
  .bub.up .sh { bottom: 100%; } .bub.up .hd { bottom: calc(100% + var(--l) - 2px); border-bottom: 9px solid var(--ac); }
  .bub.dn .sh { top: 100%; } .bub.dn .hd { top: calc(100% + var(--l) - 2px); border-top: 9px solid var(--ac); }
  .gband { background: var(--grn); }
  .bt { font: 400 24px/29px var(--fa); color: #fff; }
  .vdiv { background: rgb(255 255 255 / .75); }
  .circ { display: grid; place-items: center; border-radius: 50%; color: #fff; box-shadow: inset 0 -8px 12px rgb(0 0 0 / .22), inset 0 3px 6px rgb(255 255 255 / .15), 0 2px 5px rgb(0 0 0 / .25); }
  .circ svg { width: 74px; height: 74px; }
  .tile { display: grid; place-items: center; color: #fff; } .tile svg { width: 82px; height: 82px; }
  .tx2 { font: 400 24px/28px var(--fa); color: #595959; }
  .bub.w .sh, .bub.w .hd { filter: drop-shadow(0 1px 1px rgb(0 0 0 / .3)); }"""


EXTRA_SCRIPT = r"""  // ----- محرك الرسوم: كل حاوية .ch[data-c] تُرسم من كتلة chart-data بإحداثيات الإطار (1280×720)؛ X() يعكس للعربية -----
  var NS = 'http://www.w3.org/2000/svg', CD = JSON.parse(document.getElementById('chart-data').textContent), FW = 1280;
  function E(tag, a, p) { var e = document.createElementNS(NS, tag); for (var k in a) if (a[k] != null) e.setAttribute(k, a[k]); if (p) p.appendChild(e); return e; }
  function X(x) { return RTL ? FW - x : x; }
  function AN(a) { return a; }  // اتجاه rtl الموروث يعكس معنى start/end تلقائيًا
  function fmt(v, pct) { var r = Math.round(v * 100) / 100; return r + (pct ? '%' : ''); }
  function draw(host, c) {
    var s = E('svg', { viewBox: '0 0 1280 720', width: 1280, height: 720, 'aria-hidden': 'true' }, host);
    var b = c.box, bx = b[0], by = b[1], bw = b[2], bh = b[3], mn = c.min || 0, mx = c.max || 1, gc = c.gc || '#D9D9D9';
    var cats = c.cats || [], se = c.series || [], n = cats.length || (se[0] && se[0].v.length) || 1;
    function Y(v) { return by + bh - (v - mn) / (mx - mn) * bh; }
    function XV(v) { return bx + (v - mn) / (mx - mn) * bw; }
    function CX(i) { return bx + bw / n * (i + 0.5); }
    function CY(i) { return by + bh - bh / n * (i + 0.5); }
    function T(x, y, str, o) { o = o || {}; var f = o.f != null ? o.f : c.f;
      var t = E('text', { x: X(x), y: y, 'text-anchor': AN(o.a || 'middle'), 'dominant-baseline': 'central', 'font-size': o.fs || c.fs || 15, fill: o.fill || '#595959', 'font-weight': o.fw, 'letter-spacing': o.ls, 'class': f === 'c' ? 'c' : null }, s);
      t.textContent = str; return t; }
    function R(x, y, w, h, fill, o) { o = o || {}; return E('rect', { x: RTL ? FW - x - w : x, y: y, width: Math.max(0, w), height: Math.max(0, h), fill: fill, opacity: o.op, stroke: o.st, 'stroke-width': o.sw }, s); }
    function Ln(x1, y1, x2, y2, st, sw) { return E('line', { x1: X(x1), y1: y1, x2: X(x2), y2: y2, stroke: st || gc, 'stroke-width': sw || 1 }, s); }
    function PL(pts, o) { return E(o.closed ? 'polygon' : 'polyline', { points: pts.map(function (p) { return X(p[0]).toFixed(1) + ',' + p[1].toFixed(1); }).join(' '), fill: o.fill || 'none', stroke: o.st, 'stroke-width': o.sw, 'stroke-linejoin': 'round', opacity: o.op }, s); }
    function head(x, y, dir, col, w) { w = w || 5; PL([[x - w, y - dir * 9], [x + w, y - dir * 9], [x, y]], { closed: 1, fill: col }); }
    var t = c.t, horiz = t === 'hbar' || t === 'pct';
    // الشبكة الأفقية ومحور القيم (للرسوم الرأسية)
    if (!horiz && c.hg) for (var i = 0; i <= c.hg; i++) { var gv = mn + (mx - mn) * i / c.hg, gy = Y(gv); if (!c.nohg) Ln(bx, gy, bx + bw, gy); if (c.ax) T(bx - (c.axo || 10), gy, fmt(gv), { a: 'end', fs: c.afs, f: c.af }); }
    // الشبكة الرأسية ومحور القيم (للرسوم الأفقية)
    if (horiz && c.vg) for (var j = 0; j <= c.vg; j++) { var hv = mn + (mx - mn) * j / c.vg, hx = XV(hv); Ln(hx, by, hx, by + bh); if (c.ax) T(hx, c.ay, fmt(hv, t === 'pct'), { fs: c.afs, f: c.af }); }
    if (c.fr) { Ln(bx, by, bx, by + bh); Ln(bx + bw, by, bx + bw, by + bh); for (var k = 1; k < n; k++) Ln(bx + bw / n * k, by, bx + bw / n * k, by + bh); }
    if (c.cg) cats.forEach(function (_, i) { Ln(CX(i), by, CX(i), by + bh); });
    if (c.base) Ln(bx, by + bh, bx + bw, by + bh, c.base === 1 ? '#BFBFBF' : c.base);
    if (c.ticks) for (var q = 0; q <= n; q++) Ln(bx + bw / n * q, by + bh, bx + bw / n * q, by + bh + 5, '#BFBFBF');
    function capTop(x, y, w) { if (c.cap) R(x, y, w, 3, '#000', { op: 0.28 }); }
    if (t === 'col') {
      var bar = c.bar, off = c.off == null ? bar : c.off, gw = off * (se.length - 1) + bar;
      cats.forEach(function (_, i) { var x0 = CX(i) - gw / 2 + (c.dx || 0);
        se.forEach(function (sr, jj) { var v = sr.v[i], x = x0 + jj * off, y = Y(v); R(x, y, bar, Y(mn) - y, sr.fills ? sr.fills[i] : sr.f, { op: sr.op }); capTop(x, y, bar); }); });
    } else if (t === 'stack') {
      cats.forEach(function (_, i) { var acc = mn, x = CX(i) - c.bar / 2;
        se.forEach(function (sr) { var v = sr.v[i], y1 = Y(acc), y2 = Y(acc + v); R(x, y2, c.bar, y1 - y2, sr.fills ? sr.fills[i] : sr.f); capTop(x, y2, c.bar);
          if (c.lab) T(CX(i), (y1 + y2) / 2 + 1, fmt(v), { fill: sr.lc || '#fff', fs: c.lfs, f: c.lf }); acc += v; }); });
    } else if (t === 'hbar' || t === 'pct') {
      cats.forEach(function (_, i) { var cy = CY(i), y = cy - c.bar / 2, v = se[0].v[i], f = c.fills ? c.fills[i] : se[0].f, x1 = XV(v);
        R(XV(mn), y, x1 - XV(mn), c.bar, f, { st: c.bst, sw: c.bst ? 1 : null });
        if (t === 'pct') R(x1, y, XV(mx) - x1, c.bar, se[1].f, { st: c.bst, sw: c.bst ? 1 : null });
        if (c.cap) R((t === 'pct' ? XV(mx) : x1) - 3, y, 3, c.bar, '#000', { op: 0.3 });
        if (c.clx != null) T(c.clx, cy, cats[i], { fs: c.cfs, f: c.cf }); else if (c.cl !== false) T(bx - (c.clo || 12), cy, cats[i], { a: 'end', fs: c.cfs, f: c.cf });
        if (t === 'pct') { var mr = c.mr; E('circle', { cx: X(x1), cy: cy, r: mr, fill: c.mc, stroke: 'rgba(255,255,255,.35)', 'stroke-width': 1.5 }, s);
          T(x1, cy + 0.5, fmt(v), { fill: '#fff', fs: c.mfs, fw: 700 }); Ln(x1, cy + mr, x1, cy + mr + c.ml, c.mc, 2); head(x1, cy + mr + c.ml + 2, 1, c.mc, 4.5); } });
    } else if (t === 'area') {
      var acc2 = se[0].v.map(function () { return mn; }), PX = function (i) { return bx + bw * i / (n - 1); };
      se.forEach(function (sr) { var top = sr.v.map(function (v, i) { return (c.stacked ? acc2[i] : mn) + v; });
        var pts = top.map(function (v, i) { return [PX(i), Y(v)]; }), bot = (c.stacked ? acc2 : acc2.map(function () { return mn; })).map(function (v, i) { return [PX(i), Y(v)]; }).reverse();
        PL(pts.concat(bot), { closed: 1, fill: sr.f }); if (sr.st) PL(pts, { st: sr.st, sw: c.esw || 1.6 }); if (c.stacked) acc2 = top; });
      if (c.cy) cats.forEach(function (lb, i) { T(PX(i), c.cy, lb, { fs: c.cfs, f: c.cf }); });
    } else if (t === 'line') {
      se.forEach(function (sr) { PL(sr.v.map(function (v, i) { return [CX(i), Y(v)]; }), { st: sr.f, sw: c.lw || 2.2 }); });
    } else if (t === 'stock') {
      var cl = [];
      cats.forEach(function (_, i) { var x = CX(i), w2 = c.bar, yv = Y(c.vol[i]);
        R(x - w2 / 2, yv, w2, Y(mn) - yv, c.bf ? c.bf[i] : c.vf); capTop(x - w2 / 2, yv, w2);
        Ln(x, Y(c.high[i]), x, Y(c.low[i]), c.wk, c.wkw || 3);
        var o = c.open[i], z = c.close[i], hi = Math.max(o, z), lo = Math.min(o, z); R(x - w2 / 2, Y(hi), w2, Y(lo) - Y(hi), z >= o ? c.up : c.dn);
        cl.push([x, Y(z)]); });
      PL(cl, { st: c.cc, sw: c.ccw || 3 });
    } else if (t === 'candle') {
      cats.forEach(function (_, i) { var x = CX(i), w3 = c.bar, o = c.open[i], z = c.close[i], hi = Math.max(o, z), lo = Math.min(o, z), down = z < o;
        Ln(x, Y(c.high[i]), x, Y(c.low[i]), c.col, 1.3);
        R(x - w3 / 2, Y(hi), w3, Y(lo) - Y(hi), down ? c.col : '#fff', { st: c.col, sw: 1.2 }); if (down || c.capAll) R(x - w3 / 2, Y(hi), w3, 4, '#000', { op: down ? 0.3 : 0.12 }); });
    } else if (t === 'donut') {
      var tot = 0, a = 0; c.vals.forEach(function (v) { tot += v; });
      var P = function (an, rr) { return [X(c.cx + rr * Math.sin(an)), c.cy - rr * Math.cos(an)]; }, sw = RTL ? 0 : 1;
      c.vals.forEach(function (v, kk) { var a1 = a + v / tot * Math.PI * 2, big = a1 - a > Math.PI ? 1 : 0, p0 = P(a, c.r), p1 = P(a1, c.r), p2 = P(a1, c.ir), p3 = P(a, c.ir);
        E('path', { d: 'M' + p0 + 'A' + c.r + ',' + c.r + ' 0 ' + big + ' ' + sw + ' ' + p1 + 'L' + p2 + 'A' + c.ir + ',' + c.ir + ' 0 ' + big + ' ' + (1 - sw) + ' ' + p3 + 'Z', fill: c.fills[kk], stroke: '#fff', 'stroke-width': 1.5 }, s);
        if (c.lab) { var m = P((a + a1) / 2, (c.r + c.ir) / 2); E('text', { x: m[0], y: m[1], 'text-anchor': 'middle', 'dominant-baseline': 'central', 'font-size': c.lfs || 14, fill: '#595959', 'class': c.lf === 'c' ? 'c' : null }, s).textContent = Math.round(v / tot * 100) + '%'; }
        a = a1; });
    }
    // تسميات الفئات أسفل الرسوم الرأسية
    if (!horiz && t !== 'area' && t !== 'donut' && c.cy && cats.length) cats.forEach(function (lb, i) { T(CX(i), c.cy, lb, { fs: c.cfs, f: c.cf, ls: c.cls }); });
    // المفتاح (HTML)
    if (c.lg) { var g = c.lg, d = document.createElement('div'), fs = g.fs || 15, lh = g.lh || Math.round(fs * 1.4);
      d.className = 'lg' + (g.col ? ' col' : '') + (g.f === 'c' ? ' c' : '');
      var css = 'top:' + (g.y - lh / 2) + 'px; font-size:' + fs + 'px; line-height:' + lh + 'px;';
      css += g.cx != null ? ' inset-inline-start:' + (g.cx - 300) + 'px; width:600px;' : ' inset-inline-start:' + g.x + 'px;';
      if (g.gap != null) css += ' column-gap:' + g.gap + 'px;'; if (g.col) css += ' row-gap:0;';
      if (g.w) css += ' width:' + g.w + 'px; flex-wrap:wrap;' + (g.cx != null ? ' inset-inline-start:' + (g.cx - g.w / 2) + 'px;' : '');
      d.style.cssText = css;
      (g.items || se).forEach(function (it) { var sp = document.createElement('span'), k = it.k || g.k || 'sq';
        if (k !== 'none') { var mk = document.createElement('i'); mk.className = 'mk ' + k; mk.style.background = it.m || it.f; sp.appendChild(mk); }
        sp.appendChild(document.createTextNode(it.n)); d.appendChild(sp); });
      host.appendChild(d); }
  }
  Array.prototype.forEach.call(document.querySelectorAll('.ch[data-c]'), function (h) { var c = CD[h.getAttribute('data-c')]; if (c) draw(h, c); });"""


# ---------------------------------------------------------------- مكوّنات مشتركة
A = '/templates/freeform/' + KEY + '/assets/'
HDR = '<p class="hdr"><span>§HB1§</span> <span class="g">§HB2§</span> <span class="t">2015</span></p>'
LOGO3 = '<div class="lg3"><span class="l1">§HB1§</span><span class="l2">§HB2§</span><span class="l3">2015</span></div>'
SECS = {'g': 'SX_EXEC', 't': 'SX_ALIGN', 'm': 'SX_DEV', 'y': 'SX_RES'}


def page(th, body, bg='bgw'):
    return '\n    <div class="pg %s th-%s">' % (bg, th) + HDR + '<p class="sec">§%s§</p>' % SECS[th] + body + '</div>\n  '


def section(img, alt, title_html, top):
    return ('\n    <div class="pg"><div class="band"></div>' + LOGO3 + '<h1 class="stl" style="top:%dpx;">%s</h1>' % (top, title_html)
            + '<img class="ph" src="%s%s.jpg" alt="§%s§"></div>\n  ' % (A, img, alt))


def panels():
    return ('<div class="pnl" style="%s"></div><div class="pnl" style="%s"></div>' % (pos(88, 23, 613, 617), pos(88, 645, 612, 617))
            + ''.join('<p class="sth" style="%s">§STH§</p>' % pos(y, x) for y, x in ((119, 41), (410, 41), (119, 698), (410, 698))))


def title(top, start, width=None, token='TGH', extra=''):
    return '<h1 class="tt" style="%s">§%s§</h1>' % (pos(top, start, width, extra=extra), token)


def para(cy, start, width, token, lh=43.5, fs=24, extra=''):
    return '<p class="bd" style="%s font-size:%dpx; line-height:%spx;">§%s§</p>' % (pos(cy - lh / 2, start, width, extra=extra), fs, r1(lh), token)


def bullets(cy, start):
    return '<ul class="bl" style="%s"><li>§BU1§</li><li>§BU2§</li><li>§BU3§</li></ul>' % pos(cy - 21.75, start)


def ser(n, f, v, m=None, **kw):
    d = {'n': n, 'f': f, 'v': v}
    if m:
        d['m'] = m
    d.update(kw)
    return d


# ---------------------------------------------------------------- الشرائح 1–8
def s1():
    return ('\n    <div class="pg"><img class="cvp" src="%scover.jpg" alt="§ALT_COVER§"><div class="cvb"></div><div class="vd"></div>' % A
            + '<div class="cvl">' + ICONS['logo'] + '<span>§LOGO§</span></div>'
            + '<h1 class="cvt"><span class="b">§HB1§</span> <span class="g">§HB2§</span> <span class="w">2015</span></h1></div>\n  ')


def s2():
    rows = [('#00758C', '#005A6B', 'CT0', 3), ('#8CC63F', '#6B982A', 'CT1', 8), ('#00B0B5', '#008488', 'CT2', 14),
            ('#A6367F', '#7B2760', 'CT3', 20), ('#F4C913', '#BE9A08', 'CT4', 26)]
    tops = [258, 332, 405, 479, 553]
    h = '\n    <div class="pg bgw"><div class="band"></div>' + LOGO3 + '<h1 class="cnt">§CONTENTS§</h1>'
    for (c1, c2, tok, num), tp in zip(rows, tops):
        h += '<div class="crow" style="top:%dpx; background:%s;">§%s§</div><div class="cnum" style="top:%dpx; background:%s;">%d</div>' % (tp, c1, tok, tp, c2, num)
    return h + '<p class="cpar">§CPAR§</p></div>\n  '


def s3():
    return section('ops', 'ALT_OPS', '<span class="c1">§S3A§</span><br><span class="g">§S3B§</span> <span class="t">§S3C§</span>', 58)


G1, G2, GR = '#6A992B', '#8CC63F', '#D9D9D9'


def s4():
    b = panels()
    b += ch('s4a', {'t': 'col', 'box': [205, 180, 227, 146], 'max': 5, 'hg': 5, 'cats': [CAT(1)], 'cy': 344, 'cfs': 16, 'bar': 61, 'off': 92,
                    'series': [ser(SER(1), G1, [4.3]), ser(SER(2), G2, [2.4])], 'lg': {'cx': 318, 'y': 374, 'fs': 16}})
    b += ch('s4b', {'t': 'col', 'box': [863, 180, 226, 146], 'max': 5, 'hg': 5, 'cats': [CAT(1)], 'cy': 344, 'cfs': 16, 'bar': 60, 'off': 93,
                    'series': [ser(SER(1), G2, [2.7]), ser(SER(2), G1, [4.2])], 'lg': {'cx': 976, 'y': 374, 'fs': 16}})
    b += ch('s4c', {'t': 'stack', 'box': [205, 462, 230, 158], 'max': 8, 'hg': 4, 'cats': [CATU(1), CATU(2)], 'cy': 637, 'cfs': 14, 'bar': 69,
                    'lab': 1, 'lfs': 15, 'series': [ser(SER(1), G1, [4.3, 2.5], m='#31849B', fills=[G1, G2]), ser(SER(2), GR, [2.4, 4.4])],
                    'lg': {'cx': 318, 'y': 664, 'fs': 15}})
    b += ch('s4d', {'t': 'stack', 'box': [860, 498, 232, 152], 'max': 8, 'fr': 1, 'base': 1, 'cats': [CATU(1), CATU(2)], 'cy': 668, 'cfs': 13, 'cf': 'c', 'cls': 2,
                    'bar': 65, 'lab': 1, 'lfs': 14, 'lf': 'c', 'series': [ser(SER(1), G2, [4.3, 2.5]), ser(SER(2), G1, [2.4, 4.4])],
                    'lg': {'cx': 977, 'y': 469, 'fs': 14}})
    b += bub('$7', 318, 273, 44, 'r', 'up', 40) + bub('$2', 975, 234, 46, 'k', 'dn', 40) + bub('25%', 318, 572, 44, 'r', 'up', 40, 19) + bub('46%', 975, 532, 46, 'k', 'dn', 40, 20)
    b += ico('gear', 147, 242, 54, G1) + ico('cash', 513, 240, 56, G2) + ico('bizman', 146, 522, 60, G1) + ico('swapmen', 513, 524, 62, G2)
    b += ico('handcash', 803, 242, 54, G2) + ico('coins', 1170, 244, 54, G1) + ico('swap', 803, 537, 52, G2) + ico('podium', 1170, 537, 54, G1)
    b += kv('$8.2b', 146, 297) + kv('$3.5b', 513, 297) + kv('$4.3b', 146, 589) + kv('$2.5b', 513, 589)
    b += kv('$3.7b', 803, 297) + kv('$5.5b', 1170, 297) + kv('$7.3b', 803, 589) + kv('$5.5b', 1170, 589)
    return page('g', b)


TL1, TL2, TL3 = '#0E7D84', '#13AFB7', '#075A60'


def s5():
    b = panels()
    b += ch('s5a', {'t': 'hbar', 'box': [246, 195, 293, 150], 'max': 5, 'vg': 5, 'ax': 1, 'ay': 360, 'afs': 14, 'cats': [CAT(1), CAT(2)], 'cfs': 15, 'clo': 15,
                    'bar': 36, 'cap': 1, 'series': [ser(SER(1), 'url(#ht1)', [4.3, 2.5])]})
    b += ch('s5b', {'t': 'col', 'box': [863, 180, 226, 146], 'max': 5, 'hg': 5, 'cats': [CAT(1)], 'cy': 344, 'cfs': 16, 'bar': 60, 'off': 93,
                    'series': [ser(SER(1), 'url(#dt1)', [2.7], m=TL1), ser(SER(2), 'url(#dt2)', [4.2], m='#6FCBD0')], 'lg': {'cx': 976, 'y': 374, 'fs': 16}})
    b += ch('s5c', {'t': 'area', 'stacked': 1, 'box': [123, 465, 298, 173], 'max': 40, 'hg': 4, 'cats': DATES, 'cy': 656, 'cfs': 13, 'cf': 'c',
                    'series': [ser(SER(1), 'url(#ht3)', [16, 30.5, 21, 6, 7], m='#0B6168'), ser(SER(2), 'url(#ht4)', [6, 6.3, 1, 10.4, 14], m='#22C3CB')],
                    'lg': {'x': 470, 'y': 549, 'col': 1, 'fs': 15, 'f': 'c', 'lh': 27, 'items': [{'n': SER(2), 'm': '#22C3CB'}, {'n': SER(1), 'm': '#0B6168'}]}})
    b += ch('s5d', {'t': 'stack', 'box': [853, 476, 241, 144], 'max': 15, 'hg': 3, 'base': 1, 'cats': [CAT(3), CAT(4)], 'cy': 640, 'cfs': 14, 'cf': 'c', 'bar': 48,
                    'series': [ser(SER(1), 'url(#dt3)', [3.5, 4.5], m='#0B6F75'), ser(SER(2), 'url(#dt4)', [1.8, 2.8], m='#3DBEC5'), ser(SER(3), 'url(#dt5)', [3, 5], m='#0B5E63')],
                    'lg': {'cx': 977, 'y': 671, 'fs': 15, 'f': 'c'}})
    b += bub('$5', 975, 234, 46, 'k', 'dn', 40) + bub('$25', 975, 577, 44, 'r', 'up', 42, 19)
    b += ico('present', 122, 230, 42, TL1) + ico('meeting', 122, 303, 42, TL1)
    b += ico('team', 793, 215, 48, TL2) + ico('walk', 1177, 215, 46, TL2) + ico('boardman', 793, 527, 52, TL2) + ico('talk', 1177, 527, 52, TL2)
    b += kv('$7.5b', 793, 267) + kv('$12.2b', 1177, 267) + kv('40%', 793, 586) + kv('70%', 1177, 586)
    return page('t', b)


M1, M2, M3 = '#A5357F', '#D57DBB', '#5B1A47'
STOCK = {'vol': [35, 60, 75, 67.5, 74], 'open': [22, 13, 19, 25, 17], 'close': [13, 19, 25, 17.5, 21.5], 'high': [28, 29, 29, 29.5, 29.5], 'low': [5, 6, 6.5, 5.3, 12]}
CANDLE = {'open': [44, 25, 38, 50, 34], 'close': [25, 38, 50, 34, 18], 'high': [55, 57, 57, 58, 36], 'low': [11, 12, 13, 11, 5]}
OHLC_LG = [{'n': x, 'k': 'none'} for x in STOCK_LG[1:5]]


def stock_lg(cx, y, fs, w=None):
    g = {'cx': cx, 'y': y, 'fs': fs, 'gap': 26, 'items': [{'n': STOCK_LG[0], 'k': 'bx', 'm': M3}] + [{'n': x, 'k': 'none'} for x in STOCK_LG[1:4]] + [{'n': STOCK_LG[4], 'k': 'ln', 'm': M2}]}
    if w:
        g['w'] = w
    return g


def s6():
    b = panels()
    b += ch('s6a', {'t': 'pct', 'box': [160, 180, 312, 160], 'min': 0, 'max': 100, 'vg': 5, 'ax': 1, 'ay': 370, 'afs': 15, 'cats': [CAT(1), CAT(2), CAT(3), CAT(4)],
                    'cfs': 15, 'clo': 13, 'bar': 26, 'mr': 14, 'mc': M3, 'mfs': 12, 'ml': 18,
                    'series': [ser(SER(1), M1, [100, 75, 50, 25]), ser(SER(2), M2, [])], 'lg': {'x': 512, 'y': 258, 'col': 1, 'fs': 15, 'f': 'c', 'lh': 28}})
    sp = {'t': 'stock', 'box': [730, 180, 440, 134], 'max': 80, 'base': '#BFBFBF', 'ticks': 1, 'cats': DATES, 'cy': 326, 'cfs': 12, 'bar': 36, 'vf': M1,
          'up': '#FFFFFF', 'dn': '#E4A9D3', 'wk': '#595959', 'cc': M2, 'lg': stock_lg(950, 354, 17)}
    sp.update(STOCK)
    b += ch('s6b', sp)
    b += ch('s6c', {'t': 'line', 'box': [80, 472, 382, 178], 'max': 6, 'hg': 6, 'cats': [CAT(1), CAT(2), CAT(3), CAT(4)], 'cy': 666, 'cfs': 15,
                    'series': [ser(SER(1), M3, [4.3, 2.5, 3.5, 4.5]), ser(SER(2), M2, [2.4, 4.4, 1.8, 2.8]), ser(SER(3), M1, [2, 2, 3, 5])],
                    'lg': {'x': 484, 'y': 544, 'col': 1, 'fs': 17, 'k': 'ln', 'lh': 25.5}})
    cd = {'t': 'candle', 'box': [761, 474, 410, 167], 'max': 80, 'hg': 4, 'ax': 1, 'axo': 15, 'afs': 15, 'bar': 34, 'col': M1, 'cats': ['', '', '', '', ''],
          'lg': {'cx': 958, 'y': 671, 'fs': 15, 'f': 'c', 'gap': 22, 'items': OHLC_LG}}
    cd.update(CANDLE)
    b += ch('s6d', cd)
    return page('m', b)


Y1, Y2 = '#F2C811', '#BF9A06'
YDATA = {'s1': [64, 46, 56, 26.5, 17.5], 's2': [24, 40, 24, 42, 10]}


def s7():
    b = panels()
    b += ch('s7a', {'t': 'stack', 'box': [175, 212, 230, 151], 'max': 100, 'fr': 1, 'base': 1, 'cats': [CATU(1), CATU(2)], 'cy': 381, 'cfs': 13, 'cf': 'c', 'cls': 2,
                    'bar': 64, 'cap': 1, 'lab': 1, 'lfs': 14, 'lf': 'c',
                    'series': [ser(SER(1), Y2, [30, 60], lc='#FFFFFF'), ser(SER(2), Y1, [50, 20], lc='#7F6A10')], 'lg': {'cx': 290, 'y': 183, 'fs': 15, 'f': 'c'}})
    b += ch('s7b', {'t': 'area', 'box': [752, 183, 295, 172], 'max': 70, 'hg': 7, 'cats': DATES, 'cy': 368, 'cfs': 11,
                    'series': [ser(SER(1), 'url(#hy1)', YDATA['s1'], m=Y2, st='#8C7406'), ser(SER(2), 'url(#hy2)', YDATA['s2'], m='#E6BC0D', st='#B08D06')],
                    'lg': {'x': 1091, 'y': 261, 'col': 1, 'fs': 15, 'f': 'c', 'lh': 27}})
    b += ch('s7c', {'t': 'donut', 'box': [0, 0, 1, 1], 'cx': 263, 'cy': 567, 'r': 77, 'ir': 39, 'vals': [72, 28], 'fills': [Y1, Y2], 'lab': 1, 'lfs': 14, 'lf': 'c',
                    'lg': {'x': 410, 'y': 553, 'col': 1, 'fs': 15, 'f': 'c', 'lh': 27, 'items': [{'n': QTR[0], 'm': Y1}, {'n': QTR[1], 'm': Y2}]}})
    b += ch('s7d', {'t': 'hbar', 'box': [880, 483, 288, 150], 'max': 80, 'vg': 4, 'ax': 1, 'ay': 649, 'afs': 15, 'af': 'c', 'cats': [CAT(1), CAT(2)], 'cfs': 15, 'cf': 'c',
                    'clo': 15, 'bar': 38, 'cap': 1, 'fills': ['url(#dy2)', 'url(#dy1)'], 'series': [ser(SER(1), Y1, [43.3, 62.5])]})
    b += bub('6%', 291, 296, 46, 'k', 'up', 40, 20)
    b += ico('pctman', 115, 264, 52, Y1) + ico('bagman', 481, 264, 52, Y2) + ico('bag', 761, 517, 42, Y1) + ico('cart', 760, 592, 44, Y2)
    b += kv('$7.3b', 115, 317) + kv('$5.5b', 481, 317)
    return page('y', b)


def s8():
    return section('exec', 'ALT_EXEC', '<span class="g">§S8A§</span> §S8B§', 83)


# ---------------------------------------------------------------- الشرائح 9–19
def twin(th, left, right, bottom='PS'):
    """تخطيط «رسمان جنبًا إلى جنب»: عنوان في الوسط، لوحتان رماديتان، وفقرة سفلية في الوسط."""
    return page(th, '<h1 class="tt" style="%s">§TGH§</h1>' % pos(122, 0, 1280, extra=' text-align:center;')
                + '<div class="pnl" style="%s"></div><div class="pnl" style="%s"></div>' % (pos(210, 0, 636, 338), pos(210, 642, 638, 338))
                + para(296, 61, 185, 'PS5') + para(296, 726, 185, 'PS5') + left + right
                + para(610, 180, 920, bottom, extra=' text-align:center;'))


def title2(start=42):
    return title(168.5, start, 340, extra=' line-height:69px;')


def s9():
    b = ch('s9', {'t': 'col', 'box': [90, 142, 460, 440], 'max': 5, 'hg': 10, 'ax': 1, 'axo': 14, 'afs': 13, 'cats': [L('Category Name', 'اسم الفئة')], 'cy': 600, 'cfs': 15,
                  'bar': 104, 'off': 63.5, 'cap': 1, 'series': [ser(SER(1), G2, [4.3], op=0.85, m='#7FAE4F'), ser(SER(2), G1, [2.4], op=0.85), ser(SER(3), '#3F6420', [2], op=0.8, m='#4A6B26')],
                  'lg': {'cx': 311, 'y': 630, 'fs': 15, 'gap': 10}})
    b += bub('$7', 168, 487, 42, 'r', 'up', 91) + title(124, 610) + para(236, 610, 585, 'PS') + bullets(380, 610) + para(524, 610, 585, 'PS')
    return page('g', b)


def s10():
    b = title2() + para(392, 57, 326, 'PS')
    b += ch('s10', {'t': 'stack', 'box': [418, 225, 440, 412], 'max': 8, 'fr': 1, 'base': 1, 'cats': [CATU(1), CATU(2)], 'cy': 656, 'cfs': 13, 'cf': 'c', 'cls': 2,
                    'bar': 123, 'cap': 1, 'lab': 1, 'lfs': 21, 'series': [ser(SER(1), G1, [4.3, 2.5], lc='#FFFFFF', m='#4F7A1E'), ser(SER(2), '#BFBFBF', [2.4, 4.4], lc='#595959', m='#808080')],
                    'lg': {'cx': 640, 'y': 196, 'fs': 15, 'f': 'c'}})
    b += bub('25%', 638, 475, 78, 'r', 'up', 68, 26) + bullets(229, 897) + para(392, 897, 326, 'PS')
    return page('g', b)


def s11():
    b = title(140, 92) + para(253, 92, 585, 'PS') + bullets(398, 92) + para(542, 92, 585, 'PS')
    b += ch('s11', {'t': 'col', 'box': [788, 177, 420, 420], 'max': 4.5, 'hg': 9, 'cats': [CAT(1)], 'cy': 615, 'cfs': 15, 'bar': 154, 'off': 75, 'cap': 1,
                    'series': [ser(SER(1), G2, [2.7], op=0.88, m='#7FAE4F'), ser(SER(2), G1, [4.2], op=0.85)], 'lg': {'cx': 997, 'y': 645, 'fs': 15}})
    b += bub('$2', 999, 439, 66, 'k', 'dn', 58, 34)
    return page('g', b)


def s12():
    b = ch('s12', {'t': 'stack', 'box': [86, 216, 456, 401], 'max': 8, 'fr': 1, 'base': 1, 'cats': [CATU(1), CATU(2)], 'cy': 636, 'cfs': 13, 'cf': 'c', 'cls': 2,
                   'bar': 127, 'cap': 1, 'lab': 1, 'lfs': 20, 'series': [ser(SER(1), G2, [4.3, 2.5], m='#4F7A1E'), ser(SER(2), G1, [2.4, 4.4], m=G2)],
                   'lg': {'cx': 316, 'y': 188, 'fs': 15, 'f': 'c'}})
    b += bub('46%', 314, 416, 66, 'k', 'dn', 58, 26) + title(124, 610) + para(236, 610, 585, 'PS')
    b += '<div class="gband" style="%s"></div>' % pos(361, 600, 680, 167)
    b += '<p class="bt" style="%s">§SEE§</p>' % pos(402.5, 620, 191, extra=' text-align:end;')
    b += ico('swap', 883, 445, 74, '#FFFFFF') + '<div class="vdiv" style="%s"></div>' % pos(398, 946, 1, 95) + ico('podium', 1003, 445, 74, '#FFFFFF')
    b += '<p class="bt" style="%s">§SEE§</p>' % pos(402.5, 1069, 200)
    b += para(561, 610, 630, 'PS2')
    return page('g', b)


def s13():
    l = ch('s13a', {'t': 'stack', 'box': [296, 289, 285, 195], 'max': 8, 'fr': 1, 'base': 1, 'cats': [CATU(1), CATU(2)], 'cy': 503, 'cfs': 13, 'cf': 'c', 'cls': 2,
                    'bar': 79, 'cap': 1, 'lab': 1, 'lfs': 20, 'series': [ser(SER(1), G1, [4.3, 2.5], m='#4F7A1E'), ser(SER(2), '#BFBFBF', [2.4, 4.4], lc='#595959', m='#808080')],
                    'lg': {'cx': 440, 'y': 261, 'fs': 15, 'f': 'c'}})
    r = ch('s13b', {'t': 'col', 'box': [957, 251, 275, 202], 'max': 4.5, 'hg': 9, 'cats': [CAT(1)], 'cy': 471, 'cfs': 16, 'bar': 102, 'off': 50, 'cap': 1,
                    'series': [ser(SER(1), G2, [2.7], op=0.88, m='#7FAE4F'), ser(SER(2), G1, [4.2], op=0.85)], 'lg': {'cx': 1097, 'y': 501, 'fs': 16}})
    return twin('g', l, r)


def s14():
    return section('align', 'ALT_ALIGN', '<span class="t">§S14A§</span> §S14B§', 83)


HC = ['url(#hc1)', 'url(#hc2)', 'url(#hc3)', 'url(#hc4)', 'url(#hc5)']
HV = [4.3, 2.5, 5.5, 3.5, 2.5]
NUM5 = ['1', '2', '3', '4', '5']


def s15():
    b = ch('s15', {'t': 'hbar', 'box': [147, 189, 502, 389], 'max': 6, 'vg': 6, 'ax': 1, 'ay': 599, 'afs': 13, 'af': 'c', 'cats': NUM5, 'cfs': 15, 'clo': 14,
                   'bar': 38, 'cap': 1, 'fills': HC, 'series': [ser(SER(1), TL2, HV)]})
    for nm, y in (('meeting', 538), ('present', 461), ('netpc', 383), ('folders', 306), ('servers', 228)):
        b += ico(nm, 83, y, 46, TL2)
    b += title(124, 701) + para(236, 701, 510, 'PS') + bullets(380, 701) + para(524, 701, 510, 'PS')
    return page('t', b)


def left_block():
    return title2() + para(370, 41, 292, 'PS')


def right_block():
    return bullets(229, 918) + para(392, 918, 326, 'PS')


def s16():
    b = left_block() + ch('s16', {'t': 'area', 'stacked': 1, 'box': [430, 236, 436, 350], 'max': 120, 'hg': 6, 'ax': 1, 'axo': 15, 'afs': 13, 'af': 'c',
                                  'cats': ['1/1/1900', '1/2/1900', '1/3/1900', '1/4/1900', '1/5/1900'], 'cy': 605, 'cfs': 13, 'cf': 'c', 'esw': 2,
                                  'series': [ser(SERX(1), 'url(#ha1)', [31, 61, 40, 11, 14], m='#0D5D62', st='#0A4A4E'),
                                             ser(SERX(2), 'url(#ha2)', [11, 12, 4, 21, 27], m='#1BD3DC', st='#3F8F93'),
                                             ser(SERX(3), 'url(#ha3)', [21, 30, 24, 30, 51], m='#0F8C93', st='#0A5E63')],
                                  'lg': {'cx': 648, 'y': 206, 'fs': 15, 'f': 'c'}})
    return page('t', b + right_block())


def s17():
    b = left_block() + ch('s17', {'t': 'col', 'box': [386, 212, 492, 353], 'max': 9, 'hg': 9, 'cats': [''], 'bar': 68, 'off': 73, 'cap': 1,
                                  'series': [ser(SERX(1), 'url(#hs1)', [5.3], m='#0B5F63'), ser(SERX(2), 'url(#hs2)', [8.4], m='#0E7277'), ser(SERX(3), 'url(#hs3)', [4.6], m='#12A6AD'),
                                             ser(SERX(4), 'url(#hs4)', [7.2], m='#11939A'), ser(SERX(5), 'url(#hs5)', [4.8], m='#13A2A9')],
                                  'lg': {'cx': 637, 'y': 591, 'fs': 15, 'f': 'c'}})
    for t, x, y in (('$2', 484, 391), ('$7', 557, 270), ('$5', 629, 430), ('$9', 703, 316), ('$3', 777, 412)):
        b += bub(t, x, y, 44, 'w', 'dn', 42, 22)
    return page('t', b + right_block())


DS = [('url(#ds1)', '#0A5A5F'), ('url(#ds2)', '#0E8C93'), ('url(#ds3)', '#109FA6')]
SVAL = [[3.5, 4.5], [1.8, 2.8], [3, 5]]


def s18():
    b = ch('s18', {'t': 'stack', 'box': [106, 205, 316, 345], 'max': 14, 'hg': 7, 'ax': 1, 'axo': 14, 'afs': 13, 'af': 'c', 'cats': ['1', '2'], 'cy': 571, 'cfs': 13, 'cf': 'c',
                   'bar': 104, 'cap': 1, 'series': [ser(SERX(k + 1), DS[k][0], SVAL[k], m=DS[k][1]) for k in range(3)], 'lg': {'cx': 256, 'y': 602, 'fs': 15, 'f': 'c'}})
    b += bub('$25', 263, 461, 76, 'r', 'up', 71, 26) + title(140, 519) + para(250, 519, 625, 'PS')
    for (nm, col), x in zip((('boardman', TL2), ('talk', TL1), ('timer', TL3)), (574, 827, 1080)):
        b += '<div class="circ" style="%s background:%s; --ib:%s;">%s</div>' % (pos(383, x - 66, 132, 132), col, col, ICONS[nm])
    b += para(565, 519, 625, 'PS2')
    return page('t', b)


def s19():
    l = ch('s19a', {'t': 'stack', 'box': [340, 260, 227, 217], 'max': 14, 'hg': 7, 'ax': 1, 'axo': 14, 'afs': 13, 'af': 'c', 'cats': ['1', '2'], 'cy': 497, 'cfs': 13, 'cf': 'c',
                    'bar': 75, 'cap': 1, 'series': [ser(SERX(k + 1), DS[k][0], SVAL[k], m=DS[k][1]) for k in range(3)]})
    r = ch('s19b', {'t': 'hbar', 'box': [963, 262, 266, 210], 'max': 6, 'vg': 3, 'ax': 1, 'ay': 497, 'afs': 13, 'af': 'c', 'cats': NUM5, 'cfs': 13, 'cf': 'c', 'clo': 12,
                    'bar': 22, 'cap': 1, 'fills': HC, 'series': [ser(SER(1), TL2, HV)]})
    return twin('t', l, r)


# ---------------------------------------------------------------- الشرائح 20–31
def s20():
    return section('dev', 'ALT_DEV', '<span class="m">§S20A§</span> §S20B§', 83)


def s21():
    b = title(164, 42) + para(282, 51, 350, 'PS') + para(542, 51, 350, 'PS5')
    b += ch('s21', {'t': 'pct', 'box': [587, 285, 517, 255], 'min': 0, 'max': 100, 'vg': 10, 'ax': 1, 'ay': 560, 'afs': 14, 'cats': [CAT(1), CAT(2), CAT(3), CAT(4)],
                    'cfs': 14, 'clo': 12, 'bar': 37, 'cap': 1, 'mr': 19, 'mc': M3, 'mfs': 15, 'ml': 22,
                    'series': [ser(SER(1), M1, [100, 75, 50, 25], m=M3), ser(SER(2), M2, [], m='#B05A93')], 'lg': {'x': 1145, 'y': 410, 'col': 1, 'fs': 14, 'f': 'c', 'lh': 27}})
    return page('m', b)


def s22():
    b = ch('s22', {'t': 'line', 'box': [72, 160, 582, 418], 'max': 8, 'hg': 8, 'nohg': 1, 'ax': 1, 'axo': 10, 'afs': 11, 'cats': [CATU(k) for k in range(1, 7)], 'cy': 591, 'cfs': 11,
                   'cg': 1, 'base': 1, 'lw': 2,
                   'series': [ser(SERU(1), '#7E2860', [4.3, 2.5, 3.5, 5.5, 1.3, 5.7]), ser(SERU(2), M1, [2.4, 1.4, 1.8, 2.8, 3.5, 3.3]), ser(SERU(3), '#4A1238', [0.5, 2, 3, 5, 3.5, 6.9])],
                   'lg': {'cx': 361, 'y': 616, 'fs': 11, 'k': 'ln'}})
    b += title(124, 701) + para(236, 701, 510, 'PS') + bullets(380, 701) + para(524, 701, 510, 'PS')
    return page('m', b)


BARF = ['#5B1A47', '#7E2860', '#A5357F', '#D27AB8', '#E3A8D2']


def s23():
    sp = {'t': 'stock', 'box': [386, 230, 483, 315], 'max': 80, 'hg': 16, 'gc': '#EDEDED', 'base': 1, 'cats': DATES, 'cy': 557, 'cfs': 11, 'bar': 39, 'cap': 1,
          'bf': BARF, 'up': '#FFFFFF', 'dn': '#FFFFFF', 'wk': '#FFFFFF', 'cc': M2, 'lg': stock_lg(630, 584, 16)}
    sp.update(STOCK)
    return page('m', left_block() + ch('s23', sp) + right_block())


def s24():
    b = '<div class="pnl" style="%s"></div>' % pos(123, 0, 1280, 394)
    cd = {'t': 'candle', 'box': [90, 171, 498, 263], 'max': 70, 'hg': 7, 'ax': 1, 'axo': 15, 'afs': 15, 'af': 'c', 'bar': 40, 'col': M1, 'cats': ['', '', '', '', ''],
          'lg': {'cx': 336, 'y': 464, 'fs': 15, 'f': 'c', 'gap': 22, 'items': OHLC_LG}}
    cd.update(CANDLE)
    b += ch('s24', cd) + title(216, 697) + para(329, 697, 510, 'PS') + para(584, 225, 830, 'PS', extra=' text-align:center;')
    return page('m', b)


def s25():
    cd = {'t': 'candle', 'box': [306, 265, 300, 193], 'max': 70, 'hg': 7, 'ax': 1, 'axo': 14, 'afs': 15, 'af': 'c', 'bar': 24, 'col': M1, 'cats': ['', '', '', '', ''],
          'lg': {'cx': 452, 'y': 488, 'fs': 15, 'f': 'c', 'gap': 22, 'items': OHLC_LG}}
    cd.update(CANDLE)
    sp = {'t': 'stock', 'box': [946, 258, 290, 173], 'max': 80, 'base': 1, 'ticks': 1, 'cats': DATES, 'cy': 443, 'cfs': 11, 'bar': 23, 'cap': 1,
          'bf': BARF, 'up': '#FFFFFF', 'dn': '#FFFFFF', 'wk': '#FFFFFF', 'wkw': 2, 'cc': M2, 'ccw': 2.5, 'lg': stock_lg(1091, 471, 16, w=240)}
    sp['lg']['lh'] = 25
    sp.update(STOCK)
    return twin('m', ch('s25a', cd), ch('s25b', sp))


def s26():
    return section('res', 'ALT_RES', '<span class="y">§S26A§</span> §S26B§', 83)


def s27():
    b = left_block() + ch('s27', {'t': 'stack', 'box': [469, 240, 320, 334], 'max': 90, 'fr': 1, 'base': 1, 'cats': [CATU(1), CATU(2)], 'cy': 593, 'cfs': 13, 'cf': 'c', 'cls': 2,
                                  'bar': 89, 'cap': 1, 'lab': 1, 'lfs': 15, 'lf': 'c',
                                  'series': [ser(SER(1), Y2, [30, 60], lc='#FFFFFF', m='#8C7406'), ser(SER(2), Y1, [50, 20], lc='#7F6A10', m='#C9A20A')],
                                  'lg': {'cx': 632, 'y': 212, 'fs': 15, 'f': 'c'}})
    b += ico('pctman', 411, 382, 62, Y1) + ico('bagman', 849, 382, 62, Y2) + kv('$7.3b', 411, 447) + kv('$5.5b', 849, 447)
    b += para(309, 941, 298, 'PS')
    return page('y', b)


def s28():
    b = title(127, 42) + '<div class="pnl" style="%s"></div>' % pos(220, 0, 1280, 307)
    for k, (x, v) in enumerate(zip((183, 487, 792, 1097), ([72, 28], [57, 43], [40, 60], [22, 78]))):
        b += ch('s28' + 'abcd'[k], {'t': 'donut', 'box': [0, 0, 1, 1], 'cx': x, 'cy': 356, 'r': 97, 'ir': 49, 'vals': v, 'fills': ['url(#dy1)', 'url(#dy2)'],
                                     'lg': {'cx': x, 'y': 481, 'fs': 15, 'f': 'c', 'items': [{'n': QTR[0], 'm': '#D9B20D'}, {'n': QTR[1], 'm': '#8C7406'}]}})
    b += para(567, 41, 1150, 'PS')
    return page('y', b)


def yarea(cid, box, cy, cfs, lg=None):
    sp = {'t': 'area', 'box': box, 'max': 70, 'hg': 7, 'gc': '#E6E6E6', 'cats': DATES, 'cy': cy, 'cfs': cfs, 'esw': 2,
          'series': [ser(SER(1), 'url(#hy1)', YDATA['s1'], m='#6E5A05', st='#6E5A05'), ser(SER(2), 'url(#hy2)', YDATA['s2'], m='#C9A20A', st='#8C7406')]}
    if lg:
        sp['lg'] = lg
    return ch(cid, sp)


def s29():
    b = title(197, 79) + para(310, 78, 510, 'PS')
    b += yarea('s29', [623, 163, 457, 262], 438, 11, {'x': 1124, 'y': 287, 'col': 1, 'fs': 15, 'f': 'c', 'lh': 27})
    b += para(498, 78, 1090, 'PL4')
    return page('y', b)


def s30():
    b = title(127, 42) + ch('s30', {'t': 'hbar', 'box': [124, 253, 480, 357], 'max': 70, 'vg': 7, 'ax': 1, 'ay': 631, 'afs': 15, 'af': 'c', 'cats': [CAT(1), CAT(2)],
                                    'clx': 75, 'cfs': 15, 'cf': 'c', 'bar': 87, 'cap': 1, 'fills': ['url(#dy2)', 'url(#dy1)'], 'series': [ser(SER(1), Y1, [43.2, 62.5])]})
    b += ico('bag', 78, 300, 44, Y1) + ico('cart', 77, 479, 44, Y2)
    b += para(213, 678, 546, 'PS')
    for x, col, nm, tx2 in ((668, Y1, 'bag', 802), (964, Y2, 'cart', 1104)):
        b += '<div class="tile" style="%s background:%s; --ib:%s;">%s</div>' % (pos(361, x, 117, 117), col, col, ICONS[nm])
        b += '<p class="tx2" style="%s">§EYT§</p>' % pos(392, tx2, 140)
    b += para(551, 677, 546, 'PS2')
    return page('y', b)


def s31():
    l = yarea('s31a', [301, 268, 273, 210], 491, 10)
    r = ch('s31b', {'t': 'hbar', 'box': [964, 276, 271, 175], 'max': 80, 'vg': 4, 'ax': 1, 'ay': 469, 'afs': 15, 'af': 'c', 'cats': [CAT(1), CAT(2)], 'cl': False,
                    'bar': 41, 'cap': 1, 'fills': ['url(#dy2)', 'url(#dy1)'], 'series': [ser(SER(1), Y1, [43, 62.5])]})
    return twin('y', l, r)

# ---- (SLIDES_BEGIN) ----
SLIDES = [globals()['s%d' % k]() for k in range(1, 32)]
# ---- (SLIDES_END) ----

JSON_BLOCKS = {'chart-data': {'ar': loc(CHARTS, 'ar'), 'en': loc(CHARTS, 'en')}}

PS5_EN = 'This is a sample text. You can replace this text. Enter your text here.'
PS5_AR = 'هذا نص توضيحي. يمكنك استبدال هذا النص. أدخل النص الخاص بك هنا.'
STR = {
    'en': {'HB1': 'BUSINESS', 'HB2': 'REVIEW', 'LOGO': 'YourLogo', 'ALT_COVER': 'A person writing notes next to a laptop',
           'CONTENTS': 'Contents', 'CT0': 'Operational Program Highlights', 'CT1': 'I. PROGRAM EXECUTION', 'CT2': 'ii. STRATEGIC ALIGNMENT',
           'CT3': 'iii. Business Development', 'CT4': 'iv. Resource Management',
           'CPAR': 'This is a sample paragraph. You can replace this paragraph. Enter your text here. This is a sample paragraph. You can replace this paragraph. Enter your text here.',
           'S3A': 'OPERATIONAL', 'S3B': 'PROGRAM', 'S3C': 'HIGHLIGHTS', 'ALT_OPS': 'Hands typing on a laptop keyboard next to a phone',
           'S8A': 'PROGRAM', 'S8B': 'EXECUTION', 'ALT_EXEC': 'A hand signing a document with a fountain pen',
           'SX_EXEC': 'PROGRAM EXECUTION', 'SX_ALIGN': 'STRATEGIC ALIGNMENT', 'SX_DEV': 'BUSINESS DEVELOPMENT', 'SX_RES': 'RESOURCE MANAGEMENT',
           'STH': 'Section title goes here', 'TGH': 'Title Goes Here',
           'PS': PS5_EN + ' ' + PS5_EN, 'PS5': PS5_EN, 'PS2': PS5_EN + ' This is a sample text.',
           'PL4': ' '.join([PS5_EN] * 4),
           'BU1': 'Sample bullet point 1', 'BU2': 'Sample bullet point 2', 'BU3': 'Sample bullet point 3',
           'SEE': 'This is a sample text. Enter your text here.', 'EYT': 'Enter your text here',
           'S14A': 'STRATEGIC', 'S14B': 'ALIGNMENT', 'ALT_ALIGN': 'A blue clock face surrounded by rippling rings',
           'S20A': 'BUSINESS', 'S20B': 'DEVELOPMENT', 'ALT_DEV': 'A man taking notes with a laptop on an orange table',
           'S26A': 'RESOURCE', 'S26B': 'MANAGEMENT', 'ALT_RES': 'Stacks of coins with a euro coin in front'},
    'ar': {'HB1': 'مراجعة', 'HB2': 'الأعمال', 'LOGO': 'شعارك', 'ALT_COVER': 'شخص يدوّن ملاحظات بجوار حاسوب محمول',
           'CONTENTS': 'المحتويات', 'CT0': 'أبرز ملامح البرنامج التشغيلي', 'CT1': 'أولًا. تنفيذ البرنامج', 'CT2': 'ثانيًا. المواءمة الاستراتيجية',
           'CT3': 'ثالثًا. تطوير الأعمال', 'CT4': 'رابعًا. إدارة الموارد',
           'CPAR': 'هذه فقرة توضيحية. يمكنك استبدال هذه الفقرة. أدخل النص الخاص بك هنا. هذه فقرة توضيحية. يمكنك استبدال هذه الفقرة. أدخل النص الخاص بك هنا.',
           'S3A': 'أبرز ملامح', 'S3B': 'البرنامج', 'S3C': 'التشغيلي', 'ALT_OPS': 'يدان تكتبان على لوحة مفاتيح حاسوب محمول بجوار هاتف',
           'S8A': 'تنفيذ', 'S8B': 'البرنامج', 'ALT_EXEC': 'يد توقّع وثيقة بقلم حبر',
           'SX_EXEC': 'تنفيذ البرنامج', 'SX_ALIGN': 'المواءمة الاستراتيجية', 'SX_DEV': 'تطوير الأعمال', 'SX_RES': 'إدارة الموارد',
           'STH': 'عنوان القسم هنا', 'TGH': 'العنوان هنا',
           'PS': PS5_AR + ' ' + PS5_AR, 'PS5': PS5_AR, 'PS2': PS5_AR + ' هذا نص توضيحي.',
           'PL4': ' '.join([PS5_AR] * 4),
           'BU1': 'نقطة توضيحية 1', 'BU2': 'نقطة توضيحية 2', 'BU3': 'نقطة توضيحية 3',
           'SEE': 'هذا نص توضيحي. أدخل النص الخاص بك هنا.', 'EYT': 'أدخل النص هنا',
           'S14A': 'المواءمة', 'S14B': 'الاستراتيجية', 'ALT_ALIGN': 'ساعة زرقاء تحيط بها حلقات متموجة',
           'S20A': 'تطوير', 'S20B': 'الأعمال', 'ALT_DEV': 'رجل يدوّن ملاحظات وحاسوب محمول على طاولة برتقالية',
           'S26A': 'إدارة', 'S26B': 'الموارد', 'ALT_RES': 'أكوام من العملات المعدنية وفي مقدمتها عملة يورو'},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'mgmt', 'icon': '📈',
    'name': 'مراجعة الأعمال السنوية', 'nameEn': 'Business Review 2015',
    'description': 'إحدى وثلاثون شريحة: غلاف وفهرس وأربعة محاور بفواصل مصوّرة ولوحات 2×2 وشرائح رسوم تفصيلية',
    'descriptionEn': '31 slides: cover, contents, four themes with photo dividers, 2×2 dashboards and detailed chart slides',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'المحتويات', 'titleEn': 'Contents'},
        {'title': 'فاصل: أبرز ملامح البرنامج التشغيلي', 'titleEn': 'Divider: Operational Program Highlights'},
        {'title': 'لوحة تنفيذ البرنامج', 'titleEn': 'Program Execution Dashboard', 'needsData': True},
        {'title': 'لوحة المواءمة الاستراتيجية', 'titleEn': 'Strategic Alignment Dashboard', 'needsData': True},
        {'title': 'لوحة تطوير الأعمال', 'titleEn': 'Business Development Dashboard', 'needsData': True},
        {'title': 'لوحة إدارة الموارد', 'titleEn': 'Resource Management Dashboard', 'needsData': True},
        {'title': 'فاصل: تنفيذ البرنامج', 'titleEn': 'Divider: Program Execution'},
        {'title': 'أعمدة متداخلة بثلاث سلاسل', 'titleEn': 'Overlapping Columns (3 Series)', 'needsData': True},
        {'title': 'أعمدة مكدّسة مع نقاط', 'titleEn': 'Stacked Columns with Bullets', 'needsData': True},
        {'title': 'مقارنة سلسلتين', 'titleEn': 'Two-Series Comparison', 'needsData': True},
        {'title': 'أعمدة مكدّسة مع شريط أيقونات', 'titleEn': 'Stacked Columns with Icon Band', 'needsData': True},
        {'title': 'رسمان جنبًا إلى جنب (تنفيذ)', 'titleEn': 'Two Charts Side by Side (Execution)', 'needsData': True},
        {'title': 'فاصل: المواءمة الاستراتيجية', 'titleEn': 'Divider: Strategic Alignment'},
        {'title': 'أشرطة أفقية بخمس فئات', 'titleEn': 'Five-Category Horizontal Bars', 'needsData': True},
        {'title': 'مساحات مكدّسة', 'titleEn': 'Stacked Area Chart', 'needsData': True},
        {'title': 'أعمدة بخمس سلاسل وقيم', 'titleEn': 'Five-Series Columns with Values', 'needsData': True},
        {'title': 'أعمدة مكدّسة مع ثلاث أيقونات', 'titleEn': 'Stacked Columns with Three Icons', 'needsData': True},
        {'title': 'رسمان جنبًا إلى جنب (مواءمة)', 'titleEn': 'Two Charts Side by Side (Alignment)', 'needsData': True},
        {'title': 'فاصل: تطوير الأعمال', 'titleEn': 'Divider: Business Development'},
        {'title': 'أشرطة نسبية مع مؤشرات', 'titleEn': '100% Bars with Markers', 'needsData': True},
        {'title': 'رسم خطي بست فئات', 'titleEn': 'Six-Category Line Chart', 'needsData': True},
        {'title': 'رسم الحجم والأسعار', 'titleEn': 'Volume and Price Chart', 'needsData': True},
        {'title': 'رسم الشموع', 'titleEn': 'Candlestick Chart', 'needsData': True},
        {'title': 'رسمان جنبًا إلى جنب (تطوير)', 'titleEn': 'Two Charts Side by Side (Development)', 'needsData': True},
        {'title': 'فاصل: إدارة الموارد', 'titleEn': 'Divider: Resource Management'},
        {'title': 'أعمدة مكدّسة مع مؤشرين', 'titleEn': 'Stacked Columns with Two KPIs', 'needsData': True},
        {'title': 'أربع حلقات ربعية', 'titleEn': 'Four Quarterly Donuts', 'needsData': True},
        {'title': 'رسم مساحات متداخلة', 'titleEn': 'Overlapping Area Chart', 'needsData': True},
        {'title': 'أشرطة أفقية مع مربعين', 'titleEn': 'Horizontal Bars with Two Tiles', 'needsData': True},
        {'title': 'رسمان جنبًا إلى جنب (موارد)', 'titleEn': 'Two Charts Side by Side (Resources)', 'needsData': True},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص والأرقام فقط؛ أرقام كل الرسوم في كتلة JSON بالمعرّف chart-data (المفتاح = data-c للحاوية)، والقيم في الفقاعات ومؤشرات $…b نصوص HTML تُعدَّل مباشرة.',
}
