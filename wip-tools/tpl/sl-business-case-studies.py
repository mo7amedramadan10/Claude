# -*- coding: utf-8 -*-
# نموذج «دراسات حالة الأعمال»: سبع عشرة شريحة بهوية كحلية مع أخضر ليموني وأزرق فاتح وأشكال شيفرون مائلة.
# كل الأشكال الزخرفية والخلفيات في SVG بإطار كامل 1280×720 (الصنف deco) يُعكس أفقيًا للعربية؛ النصوص HTML بخصائص منطقية.
KEY = 'sl-business-case-studies'
TITLE = {'ar': 'دراسات حالة الأعمال', 'en': 'Business Case Studies'}
COMMENT = 'نموذج «دراسات حالة الأعمال»: غلاف وفهرس وفاصل قسم، ثم الملخص التنفيذي، الخلفية، المشكلة، بحث السوق، المنافسين، الأهداف، الحل، خارطة التنفيذ، النتائج، الفوائد، آراء أصحاب المصلحة، الخاتمة، الأسئلة، والشكر. الأشكال في svg.deco تنعكس للعربية.'
VARS = '--ink: #1A3040; --navy: #1A3040; --accent: #C0D360; --green: #C0D360; --lb: #ADD1D9; --dgreen: #3A7D44; --txt: #141414; --bg: #FFFFFF;'

FONTS = {'ar': '<link href="https://fonts.googleapis.com/css2?family=Tajawal:wght@300;400;500;700;800&display=swap" rel="stylesheet">',
         'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@300;400;600;700;800&display=swap" rel="stylesheet">'}
FONT_STACK = {'ar': "'Tajawal', 'Segoe UI', Tahoma, sans-serif", 'en': "'Open Sans', 'Segoe UI', Arial, sans-serif"}

# ---------- أدوات صغيرة ----------
def P(top, start, width=None, extra=''):
    s = 'top:%dpx; inset-inline-start:%dpx;' % (top, start)
    if width is not None:
        s += ' width:%dpx;' % width
    return s + extra

UP = '<svg class="up" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" aria-hidden="true"><path d="M3 21 21 3M8 3h13v13"/></svg>'
CHEV = '<svg class="chev" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M2 3.5h5l7.2 8.5L7 20.5H2l7.2-8.5zM10.8 3.5h5L23 12l-7.2 8.5h-5L18 12z"/></svg>'
LOGO_MARK = ('<svg class="lg-mark" viewBox="0 0 56 60" aria-hidden="true"><path fill="currentColor" fill-rule="evenodd" '
             'd="M7 6C20 0 40 1 52 7c4 14 4 32-2 47-15 6-32 7-46 2C0 42 1 21 7 6zm9 10c-2 9-2 20 1 29 9 2 19 2 28-1 3-9 3-19 1-28-10-3-20-3-30 0z"/></svg>')
QUOTE_O = ('<svg class="qo" viewBox="0 0 48 36" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">'
           '<path d="M3 34V20C3 9 8 3 18 1l1 4c-6 2-9 6-9 12h9v17zM27 34V20c0-11 5-17 15-19l1 4c-6 2-9 6-9 12h9v17z"/></svg>')
QUOTE_F = ('<svg class="qf" viewBox="0 0 40 30" fill="currentColor" aria-hidden="true">'
           '<path d="M2 28V17C2 8 6 2 14 0l2 4c-4 2-6 5-6 9h6v15zM22 28V17c0-9 4-15 12-17l2 4c-4 2-6 5-6 9h6v15z"/></svg>')
CHECK = '<svg viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="2.6" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M6 12.5l4 4 8-9"/></svg>'
ICON_MAIL = '<svg viewBox="0 0 36 38" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true"><path d="M2 15 18 4l16 11v21H2z"/><path d="M2 15l16 12 16-12"/><path d="M8 19V8h20v11"/><path d="M12 12h12M12 16h12"/></svg>'
ICON_WEB = '<svg viewBox="0 0 36 36" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true"><circle cx="18" cy="18" r="16"/><ellipse cx="18" cy="18" rx="7" ry="16"/><path d="M2 18h32M5 9h26M5 27h26"/></svg>'
ICON_PHONE = '<svg viewBox="0 0 36 36" fill="currentColor" aria-hidden="true"><path d="M8 4l5 7-3 4c2 4 5 7 9 9l4-3 7 5-3 5C16 31 5 20 4 8z"/><path d="M20 3a14 14 0 0 1 13 13h-3A11 11 0 0 0 20 6zm0 6a8 8 0 0 1 7 7h-3a5 5 0 0 0-4-4z"/></svg>'


def kb(top, start, width, cls=''):
    return '<i class="kb%s" style="%s"></i>' % (' ' + cls if cls else '', P(top, start, width))


def co(top=74, start=42):
    return '<p class="co" style="%s">§CO§</p>' % P(top, start)


def logo(top, start):
    return ('<div class="logo" style="%s">%s<span>§LOGO§</span></div>' % (P(top, start), LOGO_MARK))


def deco(inner):
    return '<svg class="deco" viewBox="0 0 1280 720" aria-hidden="true">' + inner + '</svg>'


def poly(pts, fill, extra=''):
    return '<polygon points="%s" fill="%s"%s/>' % (pts, fill, extra)


# الركن العلوي المتكرر (أخضر + أزرق فاتح + كحلي) بإحداثيات الشريحة 6
CORNER = (poly('1026,0 1133,0 1133,128', '#C0D360') + poly('1133,115 1133,198 1063,198', '#ADD1D9')
          + poly('1133,0 1225,0 1280,66 1280,198 1190,198 1133,125', '#1A3040'))
# أشكال الغلاف (شيفرون أزرق فاتح + قطعة متدرجة + مثلثات خضراء)
COVER = (poly('556,0 662,0 662,128', '#C0D360')
         + poly('665,132 836,132 1140,492 945,720 643,720 835,492 790,436', '#ADD1D9')
         + poly('531,132 665,132 852,360 789,436', 'url(#gpiece)')
         + poly('1177,494 1280,494 1280,607 1266,607', 'url(#ggr)')
         + poly('1266,607 1280,607 1280,720 1172,720', '#C0D360'))
TRI_BR = poly('1280,597 1280,720 1178,720', '#C0D360')

GLOBAL_DEFS = ('<defs>'
               '<linearGradient id="gpiece" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#617F8D"/><stop offset="1" stop-color="#1C3343"/></linearGradient>'
               '<linearGradient id="ggr" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#26363A"/><stop offset=".75" stop-color="#C0D360"/></linearGradient>'
               '<linearGradient id="ggr2" x1="0" y1="0" x2="1" y2="0"><stop offset=".25" stop-color="#C0D360"/><stop offset="1" stop-color="#1E3038"/></linearGradient>'
               '<linearGradient id="gcard" x1="1" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#AFC3DE"/><stop offset=".55" stop-color="#FFFFFF"/></linearGradient>'
               '<linearGradient id="gshade" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#193040"/><stop offset=".45" stop-color="#1B3546"/><stop offset="1" stop-color="#1A3445"/></linearGradient>'
               '</defs>')

STYLE = """  /* عام */
  .deco { position: absolute; inset: 0; width: 1280px; height: 720px; }
  html[dir="rtl"] .deco, html[dir="rtl"] .chev { transform: scaleX(-1); }
  .frame > *:not(.deco) { position: absolute; margin: 0; }
  .frame p, .frame h1, .frame h2 { margin: 0; }
  .co { font-size: 18px; letter-spacing: 4px; line-height: 1; color: var(--txt); white-space: nowrap; }
  html[lang="ar"] .co { letter-spacing: 0; font-size: 19px; }
  .ttl { top: 112px; inset-inline-start: 36px; font-size: 42px; font-weight: 700; line-height: 1.25; color: var(--navy); white-space: nowrap; letter-spacing: .5px; }
  .ttl .lt { font-weight: 300; }
  html[lang="ar"] .ttl { font-weight: 800; font-size: 42px; }
  .tx { font-size: 17.6px; line-height: 23px; color: var(--txt); }
  html[lang="ar"] .tx { font-size: 18px; }
  .st { font-size: 22.5px; line-height: 29px; font-weight: 700; color: var(--txt); }
  html[lang="ar"] .st { font-size: 22px; }
  .num { font-size: 27px; font-weight: 300; line-height: 1; color: var(--txt); }
  .vl { width: 1px; background: #3A3F45; }
  .kv { font-size: 36px; font-weight: 700; line-height: 1; color: var(--navy); white-space: nowrap; margin-top: 2px; }
  .kv sup { font-size: 24px; vertical-align: 9px; margin-inline-end: 1px; }
  .up { width: 24px; height: 24px; color: #8DB542; }
  .chev { width: 20px; height: 20px; color: var(--navy); }
  .kb { height: 6px; }
  .kb::before { content: ''; position: absolute; top: 0; inset-inline-start: 0; width: 91px; height: 6px; background: var(--lb); }
  .kb::after { content: ''; position: absolute; top: 3px; inset-inline-start: 91px; inset-inline-end: 0; height: 1px; background: #1A3040; }
  .kb.w::after { background: #fff; } .kb.n::before { background: var(--navy); } .kb.g::before { background: var(--green); }
  .logo { display: flex; align-items: center; gap: 24px; color: #fff; }
  .logo .lg-mark { width: 54px; height: 58px; color: var(--green); flex: none; }
  .logo span { font-size: 17px; line-height: 24px; letter-spacing: 3.5px; width: 200px; padding-top: 9px; }
  html[lang="ar"] .logo span { letter-spacing: 0; font-size: 18px; }
  .hp { font-size: 20px; line-height: 25px; font-weight: 700; color: var(--navy); }
  .hh { font-size: 20.5px; font-weight: 700; line-height: 1; color: var(--navy); white-space: nowrap; }
  html[lang="ar"] .hp, html[lang="ar"] .hh { font-size: 21px; }

  /* 1 الغلاف */
  .s1 .h1a { top: 226px; inset-inline-start: 70px; font-size: 70px; font-weight: 700; line-height: 1.1; white-space: nowrap; }
  .s1 .h1b { top: 300px; inset-inline-start: 68px; font-size: 86px; font-weight: 700; line-height: 1.1; white-space: nowrap; }
  .s1 .gl { top: 435px; inset-inline-start: 72px; width: 122px; height: 3px; background: var(--green); }
  .s1 .sub { top: 474px; inset-inline-start: 71px; font-size: 30px; letter-spacing: 4.2px; line-height: 1.2; white-space: nowrap; }
  .s1 .date { top: 606px; inset-inline-start: 72px; font-size: 20px; font-weight: 700; letter-spacing: 7px; color: var(--green); white-space: nowrap; }
  html[lang="ar"] .s1 .sub, html[lang="ar"] .s1 .date { letter-spacing: 0; }
  html[lang="ar"] .s1 .date { font-size: 24px; }

  /* 2 الفهرس */
  .s2 .co { color: var(--lb); }
  .s2 .toc { top: 140px; inset-inline-start: 57px; font-size: 62px; font-weight: 700; line-height: 76px; color: var(--navy); width: 460px; }
  .s2 .lead { top: 574px; inset-inline-start: 67px; font-size: 22.8px; line-height: 29px; color: var(--txt); }
  html[lang="ar"] .s2 .lead { font-size: 22px; }
  .row { inset-inline-start: 580px; width: 640px; height: 84px; display: flex; align-items: center; }
  .row::before { content: ''; position: absolute; bottom: 0; inset-inline-start: 0; width: 90px; height: 7px; background: var(--lb); }
  .row::after { content: ''; position: absolute; bottom: 3px; inset-inline-start: 90px; inset-inline-end: 0; height: 1px; background: var(--navy); }
  .row .chev { position: absolute; inset-inline-start: 31px; top: 20px; width: 36px; height: 34px; }
  .row b { position: absolute; inset-inline-start: 92px; top: 19px; font-size: 26px; line-height: 30px; color: var(--navy); white-space: nowrap; }
  html[lang="ar"] .row b { font-size: 26px; }
  .row i { position: absolute; inset-inline-end: 10px; top: 13px; font-style: normal; font-size: 40px; font-weight: 300; line-height: 1; color: var(--navy); }

  /* 3 فاصل القسم */
  .s3 .tt { top: 343px; inset-inline-start: 70px; font-size: 57px; font-weight: 700; line-height: 70px; width: 520px; }
  .s3 .lead { top: 535px; inset-inline-start: 72px; font-size: 25px; line-height: 31px; }
  html[lang="ar"] .s3 .lead { font-size: 25px; }
  .s3 .big { top: 464px; inset-inline-end: 66px; font-family: Arial, Helvetica, sans-serif; font-size: 209px; font-weight: 700; line-height: 1; color: transparent; -webkit-text-stroke: 2.5px var(--green); letter-spacing: -4px; }

  /* 4 الملخص التنفيذي */
  .s4 .st { width: 360px; }
  .card { width: 356px; height: 242px; }
  .card .kv, .card .up, .card .kb, .card .tx { position: absolute; }
  .c1 { background: var(--navy); clip-path: polygon(0 0, calc(100% - 41px) 0, 100% 41px, 100% calc(100% - 41px), calc(100% - 41px) 100%, 0 100%); }
  .c2 { background: #DEEDF0; clip-path: polygon(39px 0, 100% 0, 100% 100%, 39px 100%, 0 calc(100% - 39px), 0 39px); }
  .c3 { background: #D9E2EE; } .c4 { background: #ADD1D9; }
  .c3 { clip-path: polygon(0 0, calc(100% - 41px) 0, 100% 41px, 100% calc(100% - 41px), calc(100% - 41px) 100%, 0 100%); }
  .c4 { clip-path: polygon(39px 0, 100% 0, 100% 100%, 39px 100%, 0 calc(100% - 39px), 0 39px); }
  html[dir="rtl"] .c1, html[dir="rtl"] .c3 { clip-path: polygon(41px 0, 100% 0, 100% 100%, 41px 100%, 0 calc(100% - 41px), 0 41px); }
  html[dir="rtl"] .c2, html[dir="rtl"] .c4 { clip-path: polygon(0 0, calc(100% - 39px) 0, 100% 39px, 100% calc(100% - 39px), calc(100% - 39px) 100%, 0 100%); }
  .c1 .kv, .c1 .tx { color: #fff; }

  /* 5 الخلفية */
  .s5 .ttl { line-height: 52px; }
  .s5 .kv, .s5 .tx.w { color: #fff; }

  /* 6 المشكلة */
  .s6 .qo { top: 219px; inset-inline-start: 53px; width: 44px; height: 34px; color: #9DB7C7; }
  html[dir="rtl"] .s6 .qo { transform: scaleX(-1); }
  .s6 .ps { top: 254px; inset-inline-start: 124px; font-size: 29.7px; line-height: 39px; font-weight: 700; color: #fff; }
  html[lang="ar"] .s6 .ps { font-size: 30px; }
  .pc { top: 395px; width: 273px; height: 250px; border-top: 4px solid var(--navy); background: linear-gradient(to bottom left, #B4C6E0 0%, #FFFFFF 62%); text-align: center; }
  html[dir="rtl"] .pc { background: linear-gradient(to bottom right, #B4C6E0 0%, #FFFFFF 62%); }
  .pc .n { position: absolute; top: 28px; inset-inline-start: 0; width: 100%; font-size: 32px; font-weight: 300; line-height: 1; color: var(--navy); }
  .pc .hp { position: absolute; top: 103px; inset-inline-start: 0; width: 100%; }
  .pc .tx { position: absolute; top: 172px; inset-inline-start: 24px; width: 225px; }

  /* 7 بحث السوق */
  .s7 .mv { font-size: 34px; font-weight: 700; line-height: 1; white-space: nowrap; }
  .s7 .hp.w, .s7 .tx.w { color: #fff; }
  .s7 .b3 { width: 180px; }

  /* 8 المنافسون */
  .s8 .ch { font-size: 20.5px; font-weight: 700; line-height: 1; color: var(--navy); white-space: nowrap; }
  html[lang="ar"] .s8 .ch { font-size: 20px; }
  .s8 .ch.w { color: #fff; }
  .s8 .tx { width: 200px; }

  /* 9 الأهداف */
  .s9 .st2 { top: 194px; inset-inline-start: 44px; font-size: 24.7px; line-height: 32px; font-weight: 700; color: var(--navy); }
  html[lang="ar"] .s9 .st2 { font-size: 25px; }
  .gv { font-size: 50px; font-weight: 300; line-height: 1; color: var(--navy); white-space: nowrap; }
  .gb { font-size: 17px; line-height: 23px; font-weight: 700; color: var(--txt); width: 280px; }
  html[lang="ar"] .gb { font-size: 18px; }
  .s9 .on .hh, .s9 .on.gb, .s9 .tx.w, .s9 .hh.w { color: #fff; }
  .s9 .gv.g { color: var(--green); }
  .s9 .tx { width: 270px; }

  /* 10 الحل */
  .box { top: 178px; width: 387px; height: 213px; border: 3px solid #141414; background: #fff; }
  .box h2 { margin: 0; height: 52px; display: grid; place-items: center; font-size: 23.7px; font-weight: 700; color: var(--navy); }
  html[lang="ar"] .box h2 { font-size: 24px; }
  .box > span { position: absolute; }
  .box .chev, .box .tx { position: absolute; }
  .box .chev { width: 18px; height: 18px; }
  .s10 .hp { white-space: nowrap; }

  /* 11 خارطة التنفيذ */
  .yr { font-size: 17px; letter-spacing: 4px; line-height: 1; color: var(--txt); }
  html[lang="ar"] .yr { letter-spacing: 0; font-size: 19px; }
  .q { font-size: 41px; font-weight: 300; line-height: 1; color: var(--navy); }
  .s11 .chev, .s13 .chev { width: 18px; height: 18px; }
  .s11 .tb { width: 190px; }

  /* 12 النتائج */
  .s12 .st.w, .s12 .tx.w { color: #fff; }
  .s12 .num.g, .s16 .num.g { color: var(--green); }
  .s12 .vl { background: #C8CDD2; }

  /* 13 الفوائد */
  .ck { width: 38px; height: 38px; border-radius: 50%; background: var(--navy); display: grid; place-items: center; }
  .ck.l { background: var(--lb); }
  .ck svg { width: 26px; height: 26px; }
  .s13 .tx3 { width: 212px; }
  .s13 .tb { width: 170px; }

  /* 14 آراء أصحاب المصلحة */
  .s14 .qf { width: 34px; height: 26px; color: var(--green); }
  html[dir="rtl"] .s14 .qf { transform: scaleX(-1); }
  .s14 .st { width: 330px; }
  .nm { font-size: 18.5px; line-height: 23px; color: var(--txt); text-transform: uppercase; }
  html[lang="ar"] .nm { font-size: 18px; }

  /* 15 الخاتمة */
  .s15 .vl { background: #D5D8DB; }
  .sh { font-size: 19px; line-height: 25px; font-weight: 700; color: var(--navy); text-align: end; width: 200px; }
  html[lang="ar"] .sh { font-size: 20px; }
  .sh.w { color: #fff; }

  /* 16 الأسئلة */
  .s16 .qa { top: 192px; inset-inline-start: 60px; font-size: 158px; font-weight: 700; line-height: 1.1; white-space: nowrap; }
  .s16 .qq { top: 396px; inset-inline-start: 72px; font-size: 30px; font-weight: 700; line-height: 1.2; letter-spacing: .5px; white-space: nowrap; }
  html[lang="ar"] .s16 .qq { font-size: 32px; letter-spacing: 0; }
  html[lang="ar"] .s16 .qa { font-size: 88px; top: 228px; }
  .s16 .lead, .s17 .lead { font-size: 19.5px; line-height: 25px; color: #fff; letter-spacing: .4px; }
  html[lang="ar"] .s16 .lead, html[lang="ar"] .s17 .lead { font-size: 20px; letter-spacing: 0; }
  .s16 .logo { gap: 30px; } .s17 .logo { gap: 38px; }
  .s16 .hp, .s16 .tx, .s17 .hh, .s17 .tx { color: #fff; }

  /* 17 الشكر */
  .s17 .ty { font-size: 68px; font-weight: 700; line-height: 1.1; white-space: nowrap; }
  .s17 .ty.g { color: var(--green); }
  .s17 .ic { width: 36px; height: 38px; color: #fff; }"""

# ---------- الشرائح ----------
def s1():
    return ('\n    ' + deco(COVER) + '\n    ' + logo(87, 961)
            + '\n    <h1 class="h1a">§C_T1§</h1><h1 class="h1b">§C_T2§</h1><i class="gl"></i>'
            + '\n    <p class="sub">§C_SUB§</p><p class="date">§C_DATE§</p>\n  ')


def s2():
    rows = ''.join('\n    <div class="row" style="top:%dpx;">%s<b>§HP§</b><i>0%d</i></div>' % (t, CHEV, i + 1)
                   for i, t in enumerate([146, 248, 359, 461, 570]))
    return ('\n    ' + co(100, 63) + '\n    <h1 class="toc">§TOC§</h1>' + rows
            + '\n    <p class="lead">§LEAD§</p>\n  ')


def s3():
    return ('\n    ' + deco('<g transform="matrix(1 0 0 -1 0 720)">' + COVER + '</g>') + '\n    ' + logo(83, 62)
            + '\n    <h1 class="tt">§SEC_TT§</h1><p class="lead">§LEAD§</p><p class="big">01</p>\n  ')


def kpi(top, start, val, cls='', kbcls='', up=True, w=248):
    h = '<p class="kv %s" style="%s">%s</p>' % (cls, P(top, start), val)
    if up:
        h += '<span style="%s">%s</span>' % (P(top + 8, start + w - 24), UP)
    return h


def s4():
    d = deco(poly('1112,0 1280,0 1280,300 1222,300 1222,240 1266,187', '#C0D360')
             + poly('1222,240 1266,187 1280,187 1280,300 1222,300', 'url(#ggr)'))
    cards = ''
    for cls, top, start, off, kbc in (('c1', 181, 493, (52, 34, 109), 'w'), ('c2', 181, 867, (60, 63, 119), ''),
                                      ('c3', 436, 493, (59, 34, 117), ''), ('c4', 436, 867, (59, 63, 117), 'n')):
        vt, vs, lt = off
        cards += ('\n    <div class="card %s" style="%s">' % (cls, P(top, start))
                  + '<p class="kv" style="%s"><bdi dir="ltr"><sup>+</sup>18.5%%</bdi></p>' % P(vt, vs)
                  + '<span class="up" style="%s">%s</span>' % (P(vt + 9, vs + 216), UP)
                  + kb(lt - 3, vs, 248, kbc)
                  + '<p class="tx" style="%s">§DT§</p></div>' % P(lt + 20, vs))
    return ('\n    ' + d + '\n    ' + co() + '\n    <h1 class="ttl">§T4§</h1>'
            + '\n    <p class="st" style="%s">§ST§</p>' % P(228, 37)
            + '\n    <p class="tx" style="%s">§DS§</p>' % P(337, 37, 370)
            + '\n    <p class="num" style="%s">01</p><i class="vl" style="%s"></i><p class="tx" style="%s">§DT§</p>' % (P(427, 37), P(423, 81, 1, 'height:54px;'), P(426, 105))
            + '\n    <p class="num" style="%s">02</p><i class="vl" style="%s"></i><p class="tx" style="%s">§DT§</p>' % (P(528, 37), P(524, 81, 1, 'height:54px;'), P(527, 105))
            + cards + '\n  ')


def s5():
    d = deco(poly('0,425 1045,425 1133,530 1133,720 990,720 910,625 0,625', '#1A3040')
             + poly('1133,425 1239,425 1146,538 1280,702 1280,720 1133,720', '#C0D360')
             + poly('1133,425 1239,425 1146,538 1133,530', 'url(#ggr2)'))
    k = ''
    for start, val in ((44, '$5.3B'), (391, '<sup>+</sup>97.3%'), (731, '<sup>+</sup>500')):
        k += ('\n    <p class="kv" style="%s"><bdi dir="ltr">%s</bdi></p><span class="up" style="%s">%s</span>' % (P(461, start), val, P(466, start + 214), UP)
              + kb(514, start - 1, 248, 'w') + '<p class="tx w" style="%s">§DT§</p>' % P(536, start))
    return ('\n    ' + d + '\n    ' + co() + '\n    <h1 class="ttl">§T5§</h1>'
            + '\n    <p class="st" style="%s">§ST2§</p>' % P(250, 37, 460)
            + '\n    <p class="tx" style="%s">§DS§</p>' % P(337, 37, 320)
            + '\n    <p class="num" style="%s">01</p><i class="vl" style="%s"></i><p class="tx" style="%s">§DT§</p>' % (P(257, 534), P(253, 579, 1, 'height:54px;'), P(326, 527))
            + '\n    <p class="num" style="%s">02</p><i class="vl" style="%s"></i><p class="tx" style="%s">§DT§</p>' % (P(257, 829), P(253, 873, 1, 'height:54px;'), P(326, 822))
            + k + '\n  ')


def s6():
    d = deco(CORNER + '<rect x="36" y="198" width="1244" height="173" fill="#1A3040"/>')
    cards = ''.join('\n    <div class="pc" style="%s"><p class="n">0%d</p><p class="hp">§HP2§</p><p class="tx">§DT3§</p></div>' % (P(395, x), i + 1)
                    for i, x in enumerate([39, 341, 647, 949]))
    return ('\n    ' + d + '\n    ' + co() + '\n    <h1 class="ttl">§T6§</h1>' + QUOTE_O
            + '\n    <p class="ps">§PS§</p>' + cards + '\n  ')


def s7():
    shapes = CORNER
    texts = ''
    cols = ((358, '#ADD1D9', '#1A3040', 129, '#1A3040', '#1A3040', '', 54), (661, '#1A3040', '#C0D360', 147, '#C0D360', '#1A3040', ' w', 59),
            (963, '#3A7D44', '#C0D360', 75, '#C0D360', '#3A7D44', ' w', 70))
    for x, c, a, ha, c4, c16, w, mo in cols:
        shapes += ('<rect x="%d" y="216" width="37" height="214" fill="%s"/><rect x="%d" y="216" width="37" height="%d" fill="%s"/>' % (x + 3, c, x + 3, ha, a)
                   + poly('%d,430 %d,430 %d,490 %d,720 %d,720' % (x, x + 214, x + 274, x + 274, x), c))
        texts += ('\n    <p class="mv" style="%s"><bdi dir="ltr">4M</bdi></p><p class="tx" style="%s">§ITS§</p>' % (P(238, x + mo, None, 'color:%s;' % c4), P(282, x + mo))
                  + '<p class="mv" style="%s"><bdi dir="ltr">16M</bdi></p><p class="tx" style="%s">§ITS§</p>' % (P(341, x + mo, None, 'color:%s;' % c16), P(383, x + mo))
                  + '<p class="hp%s" style="%s">§HP2§</p><p class="tx b3%s" style="%s">§DT3§</p>' % (w, P(498, x + 42), w, P(569, x + 39)))
    left = ''
    for t in (302, 468):
        left += ('\n    <p class="kv" style="%s"><bdi dir="ltr"><sup>+</sup>18.5%%</bdi></p><span class="up" style="%s">%s</span>' % (P(t, 36), P(t + 6, 250), UP)
                 + kb(t + 54, 35, 248) + '<p class="tx" style="%s">§DT§</p>' % P(t + 78, 36))
    return '\n    ' + deco(shapes) + '\n    ' + co() + '\n    <h1 class="ttl">§T7§</h1>' + left + texts + '\n  '


def s8():
    tops = [261, 344, 434, 517, 601, 684]
    sh = ''
    for i in (0, 2, 4):
        sh += '<rect x="0" y="%d" width="1280" height="%d" fill="#DCE5EE"/>' % (tops[i], tops[i + 1] - tops[i])
    for j, x in enumerate([243, 492, 740, 988]):
        main, odd = ('#1A3040', '#697C8D') if j == 0 else ('#DEEAF3', '#CEDCE9')
        cut = 50 if j else 50
        sh += poly('%d,178 %d,178 %d,%d %d,684 %d,684' % (x, x + 233 - cut, x + 233, 178 + cut, x + 233, x), main)
        for i in (0, 2, 4):
            sh += '<rect x="%d" y="%d" width="233" height="%d" fill="%s"/>' % (x, tops[i], tops[i + 1] - tops[i], odd)
    mat = ['++-+', '+-+-', '+-+-', '+-++', '++-+']
    for i, r in enumerate(mat):
        cy = (tops[i] + tops[i + 1]) / 2
        for j, ch in enumerate(r):
            cx = [359, 608, 855, 1104][j]
            if ch == '+':
                g = '#1A3040' if j == 0 and i % 2 else ('#5C6E7C' if j == 0 else '#E9F1CC')
                sh += ('<circle cx="%d" cy="%.1f" r="14.5" fill="#C0D360"/><path d="M%d %.1fh16M%d %.1fv16" stroke="%s" stroke-width="2.6"/>'
                       % (cx, cy, cx - 8, cy, cx, cy - 8, g))
            else:
                sh += ('<circle cx="%d" cy="%.1f" r="14.5" fill="#33627F"/><path d="M%d %.1fh16" stroke="#C9D9E6" stroke-width="2.6"/>'
                       % (cx, cy, cx - 8, cy))
    heads = ('\n    <p class="ch" style="%s">§TH0§</p><p class="ch w" style="%s">§TH1§</p>' % (P(208, 36), P(208, 262))
             + ''.join('<p class="ch" style="%s">§TH%d§</p>' % (P(208, x), k + 2) for k, x in enumerate([514, 762, 1010])))
    cells = ''.join('\n    <p class="tx" style="%s">§DTR§</p>' % P(int((tops[i] + tops[i + 1]) / 2) - 23, 32) for i in range(5))
    return '\n    ' + deco(sh) + '\n    ' + co() + '\n    <h1 class="ttl">§T8§</h1>' + heads + cells + '\n  '


def s9():
    d = deco('<rect x="0" y="526" width="1280" height="129" fill="#DCE4ED"/>'
             + poly('640,296 950,296 950,603 898,655 640,655', '#1A3040')
             + poly('895,0 1058,0 1058,195 1045,183', '#C0D360') + poly('1045,183 1058,183 1058,298 952,298', 'url(#ggr)')
             + poly('1058,0 1200,0 1280,97 1280,298 1145,298 1058,195', '#1A3040'))
    cols = ''
    for x, v, on in ((36, '93.7%', 0), (343, '3x', 0), (660, '325%', 1), (980, '$4.5B', 0)):
        w = ' w' if on else ''
        cols += ('\n    <p class="hh%s" style="%s">§HH§</p><p class="gv%s" style="%s"><bdi dir="ltr">%s</bdi></p>' % (w, P(336, x), ' g' if on else '', P(377, x - 1), v)
                 + '<p class="gb%s" style="%s">§GB§</p><p class="tx%s" style="%s">§DT§</p>' % (' on' if on else '', P(451, x), w, P(559, x - 1)))
    return ('\n    ' + d + '\n    ' + co() + '\n    <h1 class="ttl">§T9§</h1><p class="st2">§ST2§</p>' + kb(289, 35, 295)
            + cols + '\n  ')


def s10():
    boxes = ''
    for x, bg, fg in ((35, '#1A3040', '#fff'), (447, '#ADD1D9', '#1A3040'), (859, '#C0D360', '#1A3040')):
        boxes += ('\n    <div class="box" style="%s"><h2 style="background:%s; color:%s;">§HH§</h2>' % (P(178, x), bg, fg)
                  + '<span style="%s">%s</span><p class="tx" style="%s">§DT§</p>' % (P(80, 19), CHEV, P(73, 58))
                  + '<span style="%s">%s</span><p class="tx" style="%s">§DT§</p></div>' % (P(148, 19), CHEV, P(141, 58)))
    stats = ''
    for x, v in ((36, '8.5'), (344, '7%'), (649, '300+'), (957, '4.5K+')):
        stats += ('\n    <p class="hp" style="%s">§HP§</p><p class="gv" style="%s"><bdi dir="ltr">%s</bdi></p>' % (P(440, x), P(484, x - 1), v)
                  + kb(556, x - 1, 236) + '<p class="tx" style="%s">§DT3§</p>' % P(594, x, 230))
    return '\n    ' + co() + '\n    <h1 class="ttl">§T10§</h1>' + boxes + stats + '\n  '


def s11():
    sh = CORNER
    tx = ''
    for i, (x, y, to) in enumerate(((36, 296, 116), (351, 251, 115), (661, 201, 111), (978, 152, 111))):
        ly = y + 88  # خط القياس
        sh += ('<rect x="%d" y="%d" width="91" height="6" fill="#ADD1D9"/>' % (x - 1, ly - 3)
               + '<polyline points="%d,%d %d,%d %d,%d" fill="none" stroke="#1A3040" stroke-width="1"/>' % (x + 90, ly, x + 270, ly, x + 302, ly - 38))
        tx += ('\n    <p class="yr" style="%s">§YEAR§</p><p class="q" style="%s">Q%d</p>' % (P(y, x), P(y + 28, x - 1), i + 1)
               + '<p class="tx" style="%s">§DT§</p>' % P(y + to, x)
               + ''.join('<span style="%s">%s</span><p class="tx tb" style="%s">§IT2§</p>' % (P(y + to + d + 13, x + 1), CHEV, P(y + to + d, x + 39))
                         for d in (75, 143, 220)))
    return '\n    ' + deco(sh) + '\n    ' + co() + '\n    <h1 class="ttl">§T11§</h1>' + tx + '\n  '


def s12():
    d = deco(poly('737,0 875,0 919,53 782,53', '#C0D360') + poly('945,0 1280,0 1280,720 711,720 711,297 775,297 986,48', '#1A3040')
             + poly('876,0 945,0 986,48 775,297 711,297 918,50', '#ADD1D9'))
    k = ''
    for t, x in ((233, 36), (233, 384), (433, 36), (433, 384)):
        k += ('\n    <p class="kv" style="%s"><bdi dir="ltr"><sup>+</sup>18.5%%</bdi></p><span class="up" style="%s">%s</span>' % (P(t + 4, x), P(t + 8, x + 259), UP)
              + kb(t + 56, x - 1, 296) + '<p class="tx" style="%s">§DT§</p>' % P(t + 79, x))
    return ('\n    ' + d + '\n    ' + co() + '\n    <h1 class="ttl">§T12§</h1>' + k
            + '\n    <p class="st w" style="%s">§ST§</p><p class="tx w" style="%s">§DS§</p>' % (P(262, 847, 360), P(371, 847, 370))
            + '\n    <p class="num g" style="%s">01</p><i class="vl" style="%s"></i><p class="tx w" style="%s">§DT§</p>' % (P(461, 846), P(456, 891, 1, 'height:54px;'), P(460, 915))
            + '\n    <p class="num g" style="%s">02</p><i class="vl" style="%s"></i><p class="tx w" style="%s">§DT§</p>' % (P(562, 846), P(557, 891, 1, 'height:54px;'), P(561, 915))
            + '\n  ')


def s13():
    sh = CORNER
    tx = ''
    for i, x in enumerate([41, 282, 524, 766, 1008]):
        sh += ('<path d="M%d 215H%d" stroke="#1A3040" stroke-width="1.2"/><path d="M%d 210l6 5-6 5" fill="none" stroke="#1A3040" stroke-width="1.2"/>' % (x + 44, x + 213, x + 207)
               + '<rect x="%d" y="420" width="207" height="1.2" fill="#1A3040"/>' % (x + 3))
        tx += ('\n    <span class="ck%s" style="%s">%s</span>' % (' l' if i % 2 else '', P(196, x), CHECK)
               + '<p class="hp" style="%s">§HP2§</p><p class="tx tx3" style="%s">§DT4§</p>' % (P(259, x + 3), P(327, x + 2))
               + '<span style="%s">%s</span><p class="tx tb" style="%s">§IT2§</p>' % (P(465, x + 3), CHEV, P(459, x + 43))
               + '<span style="%s">%s</span><p class="tx tb" style="%s">§IT2§</p>' % (P(550, x + 3), CHEV, P(544, x + 43)))
    return '\n    ' + deco(sh) + '\n    ' + co() + '\n    <h1 class="ttl">§T13§</h1>' + tx + '\n  '


def s14():
    sh = '<g transform="translate(922 0) scale(1.39) translate(-1022 0)">' + CORNER + '</g>'
    tx = ''
    for i, o in enumerate((0, 416, 828)):
        if i == 1:
            sh += '<rect x="%d" y="275" width="381" height="270" fill="url(#gcard)"/>' % (452)
        sh += ('<path d="M%d 178V545H%d M%d 275H%d V643L%d 683" fill="none" stroke="#1A3040" stroke-width="1"/>'
               % (35 + o, 352 + o, 103 + o, 417 + o, 385 + o))
        tx += ('\n    <span style="%s">%s</span>' % (P(258, 57 + o), QUOTE_F)
               + '<p class="st" style="%s">§ST§</p><p class="tx" style="%s">§DS§</p>' % (P(322, 63 + o), P(430, 63 + o, 320))
               + '<p class="nm" style="%s"><b>§NAME§</b><br>§ROLE§</p>' % P(570, 63 + o))
    return '\n    ' + deco(sh) + '\n    ' + co() + '\n    <h1 class="ttl">§T14§</h1>' + tx + '\n  '


def s15():
    sh = '<g transform="matrix(1 0 0 -1 0 720)">' + CORNER + '</g>'
    tx = ''
    # من الأعلى إلى الأسفل: (x, y, لون الكتلة، لون الشريط، أبيض؟)
    for x, y, c, b, w, to in ((704, 204, '#ADD1D9', '#1A3040', 0, 277), (583, 325, '#1A3040', '#ADD1D9', 1, 279),
                              (462, 446, '#ADD1D9', '#1A3040', 0, 282), (341, 567, '#1A3040', '#ADD1D9', 1, 282)):
        sh += (poly('%d,%d %d,%d %d,%d %d,%d %d,%d' % (x + 40, y, x + 255, y, x + 255, y + 114, x, y + 114, x, y + 40), c)
               + '<rect x="%d" y="%d" width="174" height="14" fill="%s"/>' % (x + 121, y + 76, b))
        tx += ('\n    <p class="sh%s" style="%s">§HP2§</p>' % (' w' if w else '', P(y + 13, x + 38))
               + '<p class="tx" style="%s">§DT§</p>' % P(y + 17, x + to))
    return ('\n    ' + deco(sh) + '\n    ' + co() + '\n    <h1 class="ttl">§T15§</h1>'
            + '\n    <p class="st" style="%s">§ST§</p><p class="tx" style="%s">§DS§</p>' % (P(209, 42, 360), P(321, 42, 320))
            + '\n    <p class="num" style="%s">01</p><i class="vl" style="%s"></i><p class="tx" style="%s">§DT§</p>' % (P(413, 41), P(408, 86, 1, 'height:54px;'), P(411, 110))
            + '\n    <p class="num" style="%s">02</p><i class="vl" style="%s"></i><p class="tx" style="%s">§DT§</p>' % (P(514, 41), P(509, 86, 1, 'height:54px;'), P(512, 110))
            + tx + '\n  ')


def s16():
    d = deco(poly('365,0 449,0 864,480 661,720 509,720 738,447', '#ADD1D9')
             + poly('664,360 738,447 509,720 360,720', 'url(#gpiece)') + TRI_BR)
    items = ''
    for t in (122, 307, 496):
        items += ('\n    <p class="num g" style="%s">0%d</p><p class="hp" style="%s">§HP§</p>' % (P(t + 4, 895), (t > 122) + (t > 307) + 1, P(t, 975))
                  + '<p class="tx" style="%s">§DT§</p>' % P(t + 37, 975) + kb(t + 131, 885, 334, 'w g'))
    return ('\n    ' + d + '\n    ' + logo(52, 60)
            + '\n    <h1 class="qa">§QA§</h1><p class="qq">§QQ§</p><p class="lead" style="%s">§LEAD2§</p>' % P(467, 72)
            + items + '\n  ')


def s17():
    d = deco(poly('0,0 362,0 643,330 470,538 0,538', 'url(#gshade)') + poly('581,0 665,0 864,237 446,720 361,720 667,357 643,330 750,203', '#ADD1D9')
             + poly('362,0 581,0 750,203 643,330', 'url(#gpiece)') + TRI_BR)
    items = ''
    for t, bt, lt, x, ic, k in ((80, 145, 182, 891, ICON_MAIL, 'EM'), (290, 351, 384, 891, ICON_WEB, 'WEB'), (494, 552, 587, 884, ICON_PHONE, 'CON')):
        items += ('\n    <span class="ic" style="%s">%s</span>' % (P(t, 885), ic) + kb(bt, 885, 296, 'w')
                  + '<p class="hh" style="%s">§%s§</p><p class="tx" style="%s">§IT§</p>' % (P(lt, x), k, P(lt + 32, x)))
    return ('\n    ' + d + '\n    ' + logo(580, 52)
            + '\n    <h1 class="ty g" style="%s">§TY1§</h1><h1 class="ty" style="%s">§TY2§</h1>' % (P(178, 34), P(266, 34))
            + '<p class="lead" style="%s">§LEAD2§</p>' % P(398, 46) + items + '\n  ')


SLIDES = [s1(), s2(), s3(), s4(), s5(), s6(), s7(), s8(), s9(), s10(), s11(), s12(), s13(), s14(), s15(), s16(), s17()]
# الشرائح الداكنة تأخذ خلفية متدرجة: يضاف الصنف dark إلى الإطار عبر عنصر خلفية أول
for _i in (0, 2, 15):
    SLIDES[_i] = '<i class="bgd"></i>' + SLIDES[_i]
STYLE += """
  .bgd { position: absolute; inset: 0; background: linear-gradient(90deg, #193040 4%, #1B3546 28%, #1B3546 46%, #193040 58%); }
  .s3 .bgd { background: linear-gradient(90deg, #19303E 0%, #1D3748 42%, #1F3447 56%, #24364C 75%, #25374F 100%); }
  .s1, .s3, .s16, .s17 { color: #fff; }
  .s17 { background: #193040; }
  html[dir="rtl"] .bgd { transform: scaleX(-1); }"""

STR = {
    'en': {
        'CO': 'YOUR COMPANY NAME HERE', 'LOGO': 'YOUR COMPANY LOGO HERE',
        'C_T1': 'BUSINESS', 'C_T2': 'CASE STUDIES', 'C_SUB': 'PRESENTATION TEMPLATE', 'C_DATE': 'INSERT DATE HERE',
        'TOC': 'TABLE OF CONTENTS', 'HP': 'Heading Placeholder', 'HP2': 'Heading<br>Placeholder',
        'LEAD': 'Insert your heading statement here.<br>A brief statement about your topic.',
        'SEC_TT': 'TOPIC TITLE PLACEHOLER',
        'T4': 'EXECUTIVE SUMMARY <span class="lt">SLIDE</span>',
        'ST': 'Insert your heading statement here. A brief statement about your topic.',
        'ST2': 'Insert your heading statement here.<br>A brief statement about your topic.',
        'DS': 'Insert your desired statement here. This is a sample text.',
        'DT': 'Insert your desired text here.<br>This is a sample text.',
        'DT3': 'Insert your desired text here. This is a sample text.',
        'DT4': 'Insert your desired text here. This is a sample text.',
        'DTR': 'Insert your desired<br>text here.', 'IT2': 'Insert your desired text here.', 'IT': 'Insert your desired text here.',
        'ITS': 'Insert text here.',
        'T5': 'COMPANY BACKGROUND<br>&amp; CONTEXT <span class="lt">SLIDE</span>',
        'T6': 'PROBLEM STATEMENT <span class="lt">SLIDE</span>',
        'PS': 'Insert your main problem statement here.<br>A brief statement about your topic.',
        'T7': 'MARKET RESEARCH <span class="lt">SLIDE</span>',
        'T8': 'COMPETITIVE LANDSCAPE <span class="lt">SLIDE</span>',
        'TH0': 'HEADING HERE', 'TH1': 'YOUR COMPANY', 'TH2': 'COMPETITOR 1', 'TH3': 'COMPETITOR 2', 'TH4': 'COMPETITOR 3',
        'T9': 'STRATEGIC OBJECTIVES &amp; GOALS <span class="lt">SLIDE</span>',
        'HH': 'HEADING HERE', 'GB': 'Insert your heading statement here. This is a sample text.',
        'T10': 'PROPOSED SOLUTION &amp; METHODOLOGY <span class="lt">SLIDE</span>',
        'T11': 'IMPLEMENTATION ROADMAP <span class="lt">SLIDE</span>', 'YEAR': 'YEAR',
        'T12': 'RESULTS &amp; IMPACT <span class="lt">SLIDE</span>',
        'T13': 'QUALITATIVE BENEFITS <span class="lt">SLIDE</span>',
        'T14': 'STAKEHOLDER FEEDBACK <span class="lt">SLIDE</span>', 'NAME': 'NAME PLACEHOLDER', 'ROLE': 'ROLE HERE',
        'T15': 'CONCLUSION &amp; FUTURE OUTLOOK <span class="lt">SLIDE</span>',
        'QA': 'Q&amp;A', 'QQ': 'DO YOU HAVE ANY QUESTIONS?',
        'LEAD2': 'Insert your desired text here. This is a sample text.<br>A brief statement about your topic.',
        'TY1': 'Thank You', 'TY2': 'Connect With Us', 'EM': 'EMAIL', 'WEB': 'WEBSITE', 'CON': 'CONTACT',
    },
    'ar': {
        'CO': 'اسم شركتك هنا', 'LOGO': 'شعار شركتك هنا',
        'C_T1': 'دراسات حالة', 'C_T2': 'الأعمال', 'C_SUB': 'قالب عرض تقديمي', 'C_DATE': 'أدرج التاريخ هنا',
        'TOC': 'جدول المحتويات', 'HP': 'عنوان نائب', 'HP2': 'عنوان<br>نائب',
        'LEAD': 'أدرج عبارتك الرئيسية هنا.<br>عبارة موجزة عن موضوعك.',
        'SEC_TT': 'عنوان الموضوع النائب',
        'T4': '<span class="lt">شريحة</span> الملخص التنفيذي',
        'ST': 'أدرج عبارتك الرئيسية هنا. عبارة موجزة عن موضوعك.',
        'ST2': 'أدرج عبارتك الرئيسية هنا.<br>عبارة موجزة عن موضوعك.',
        'DS': 'أدرج العبارة المطلوبة هنا. هذا نص توضيحي.',
        'DT': 'أدرج النص المطلوب هنا.<br>هذا نص توضيحي.',
        'DT3': 'أدرج النص المطلوب هنا. هذا نص توضيحي.',
        'DT4': 'أدرج النص المطلوب هنا. هذا نص توضيحي.',
        'DTR': 'أدرج النص<br>المطلوب هنا.', 'IT2': 'أدرج النص المطلوب هنا.', 'IT': 'أدرج النص المطلوب هنا.',
        'ITS': 'أدرج النص هنا.',
        'T5': '<span class="lt">شريحة</span> خلفية الشركة<br>وسياقها',
        'T6': '<span class="lt">شريحة</span> بيان المشكلة',
        'PS': 'أدرج بيان المشكلة الرئيسية هنا.<br>عبارة موجزة عن موضوعك.',
        'T7': '<span class="lt">شريحة</span> بحث السوق',
        'T8': '<span class="lt">شريحة</span> المشهد التنافسي',
        'TH0': 'العنوان هنا', 'TH1': 'شركتك', 'TH2': 'المنافس 1', 'TH3': 'المنافس 2', 'TH4': 'المنافس 3',
        'T9': '<span class="lt">شريحة</span> الأهداف والغايات الاستراتيجية',
        'HH': 'العنوان هنا', 'GB': 'أدرج عبارتك الرئيسية هنا. هذا نص توضيحي.',
        'T10': '<span class="lt">شريحة</span> الحل المقترح والمنهجية',
        'T11': '<span class="lt">شريحة</span> خارطة طريق التنفيذ', 'YEAR': 'السنة',
        'T12': '<span class="lt">شريحة</span> النتائج والأثر',
        'T13': '<span class="lt">شريحة</span> الفوائد النوعية',
        'T14': '<span class="lt">شريحة</span> آراء أصحاب المصلحة', 'NAME': 'اسم الشخص', 'ROLE': 'المنصب هنا',
        'T15': '<span class="lt">شريحة</span> الخاتمة والآفاق المستقبلية',
        'QA': 'أسئلة وأجوبة', 'QQ': 'هل لديكم أي أسئلة؟',
        'LEAD2': 'أدرج النص المطلوب هنا. هذا نص توضيحي.<br>عبارة موجزة عن موضوعك.',
        'TY1': 'شكرًا لكم', 'TY2': 'تواصلوا معنا', 'EM': 'البريد الإلكتروني', 'WEB': 'الموقع الإلكتروني', 'CON': 'التواصل',
    },
}

_sec = [('الغلاف', 'Cover', False), ('جدول المحتويات', 'Table of Contents', False), ('فاصل القسم', 'Section Divider', False),
        ('الملخص التنفيذي', 'Executive Summary', True), ('خلفية الشركة وسياقها', 'Company Background & Context', True),
        ('بيان المشكلة', 'Problem Statement', False), ('بحث السوق', 'Market Research', True),
        ('المشهد التنافسي', 'Competitive Landscape', False), ('الأهداف والغايات الاستراتيجية', 'Strategic Objectives & Goals', True),
        ('الحل المقترح والمنهجية', 'Proposed Solution & Methodology', True), ('خارطة طريق التنفيذ', 'Implementation Roadmap', False),
        ('النتائج والأثر', 'Results & Impact', True), ('الفوائد النوعية', 'Qualitative Benefits', False),
        ('آراء أصحاب المصلحة', 'Stakeholder Feedback', False), ('الخاتمة والآفاق المستقبلية', 'Conclusion & Future Outlook', False),
        ('الأسئلة والأجوبة', 'Q&A', False), ('الشكر والتواصل', 'Thank You & Contact', False)]

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'mgmt', 'icon': '📁',
    'name': 'دراسات حالة الأعمال', 'nameEn': 'Business Case Studies',
    'description': '17 شريحة لدراسة حالة: الملخص، الخلفية، المشكلة، السوق، المنافسون، الأهداف، الحل، التنفيذ، النتائج، الخاتمة',
    'descriptionEn': '17 case-study slides: summary, background, problem, market, competitors, goals, solution, roadmap, results',
    'sections': [dict({'title': a, 'titleEn': b}, **({'needsData': True} if n else {})) for a, b, n in _sec],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والأشكال الزخرفية (svg.deco) وغيّر النصوص والأرقام فقط؛ علامات +/− في جدول المنافسين دوائر داخل SVG الشريحة.',
}
