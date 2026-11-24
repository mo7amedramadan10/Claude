# -*- coding: utf-8 -*-
# نموذج «عرض تقرير أعمال»: 19 شريحة بهوية زرقاء (#0070C0) وصور رمادية: غلاف، جدول أعمال، شرائح نص وصور،
# آراء العملاء، مؤشرات أداء، حصة سوقية (رسم أعمدة أفقية من chart-data)، قمع مبيعات، دائرة خماسية، تحليل SWOT،
# هرم أحجية، مقارنة منتجات (مصفوفة من chart-data-2)، وشريحة تواصل.
import math

KEY = 'sl-business-report'
TITLE = {'ar': 'عرض تقرير أعمال', 'en': 'Business Report Presentation'}
COMMENT = 'نموذج «عرض تقرير أعمال»: كتل زرقاء مستطيلة وصور رمادية بإحداثيات مطلقة 1280×720؛ الصور تُعكس في العربية بالصنف flip، والرسوم (القمع، الدائرة الخماسية، SWOT، الهرم) SVG تُعكس بالصنف dec؛ رسم الحصة السوقية ومصفوفة المقارنة يُرسمان من كتلتي chart-data وchart-data-2.'
VARS = '--ink: #262626; --ink-2: #404040; --ink-3: #595959; --accent: #0070C0; --blue: #0070C0; --sky: #45B0FF; --pale: #C1E5FF; --deep: #005595; --navy: #0D3A63; --gray: #F2F2F2; --line: #D9D9D9; --bg: #FFFFFF;'
FONTS = {'ar': '<link href="https://fonts.googleapis.com/css2?family=Tajawal:wght@300;400;500;700;800&display=swap" rel="stylesheet">',
         'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@300;400;600;700;800&display=swap" rel="stylesheet">'}
FONT_STACK = {'ar': "'Tajawal', 'Segoe UI', Tahoma, sans-serif", 'en': "'Open Sans', 'Segoe UI', Arial, sans-serif"}

A = '/templates/freeform/sl-business-report/assets/'


def pos(top, start, w=None, h=None, extra=''):
    s = 'top:%spx; inset-inline-start:%spx;' % (top, start)
    if w is not None: s += ' width:%spx;' % w
    if h is not None: s += ' height:%spx;' % h
    return ' style="%s%s"' % (s, extra)


def img(name, top, start, w, h, alt, flip=True, cls=''):
    return '<img class="ph%s%s" src="%s%s.jpg" alt="§%s§"%s>' % (' flip' if flip else '', cls, A, name, alt, pos(top, start, w, h))


def box(top, start, w, h, cls='bx'):
    return '<div class="%s"%s></div>' % (cls, pos(top, start, w, h))


STYLE = """  .frame h1, .frame h2, .frame h3, .frame p { margin: 0; }
  .abs { position: absolute; }
  .ph { position: absolute; display: block; object-fit: cover; }
  html[dir="rtl"] .flip, html[dir="rtl"] .dec { transform: scaleX(-1); }
  .bx { position: absolute; background: var(--blue); }
  .gr { position: absolute; background: var(--gray); }
  .bar { position: absolute; width: 186px; height: 14px; background: var(--blue); }
  .sq { position: absolute; width: 102px; height: 126px; background: var(--blue); }
  .tx { position: absolute; font-size: 17.5px; line-height: 22.5px; color: var(--ink-2); }
  .tx p + p { margin-top: 22px; }
  .wt { color: #fff; }
  .t48 { position: absolute; font-size: 46px; line-height: 57px; font-weight: 700; color: var(--blue); letter-spacing: -0.3px; }
  .t48.wt { color: #fff; }
  .t40 { position: absolute; font-size: 39px; line-height: 48px; font-weight: 700; color: var(--blue); letter-spacing: -0.3px; }
  .st { position: absolute; top: 36px; inset-inline-start: 64px; font-size: 46px; line-height: 62px; font-weight: 700; color: var(--blue); letter-spacing: -0.3px; white-space: nowrap; }
  .st .lt { font-weight: 400; }
  .ih { font-size: 20.5px; line-height: 28px; font-weight: 700; color: var(--ink); letter-spacing: -0.3px; }
  html[lang="ar"] .ih { letter-spacing: 0; }
  .it p.d { font-size: 17.5px; line-height: 22.5px; color: var(--ink-2); margin-top: 9px; }
  .it { position: absolute; }
  .it.e { text-align: end; }
  html[lang="ar"] .t48, html[lang="ar"] .t40, html[lang="ar"] .st { letter-spacing: 0; }
  html[lang="ar"] .tx, html[lang="ar"] .it p.d { font-size: 18px; }
  /* 1 الغلاف */
  .s1 .logo { position: absolute; top: 62px; inset-inline-start: 70px; width: 54px; height: 58px; }
  .s1 .lgt { position: absolute; top: 72px; inset-inline-start: 147px; font-size: 16.5px; line-height: 23px; letter-spacing: 2.3px; color: #fff; }
  .s1 .k1 { position: absolute; top: 261px; inset-inline-start: 64px; font-size: 64px; line-height: 64px; font-weight: 300; color: var(--ink); }
  .s1 .k2 { position: absolute; top: 330px; inset-inline-start: 64px; font-size: 61px; line-height: 64px; font-weight: 700; color: var(--blue); letter-spacing: -0.5px; }
  .s1 .k3 { position: absolute; top: 415px; inset-inline-start: 65px; font-size: 29px; line-height: 40px; color: var(--ink); letter-spacing: 1.4px; }
  html[lang="ar"] .s1 .k1 { font-weight: 400; } html[lang="ar"] .s1 .k2, html[lang="ar"] .s1 .k3, html[lang="ar"] .s1 .lgt { letter-spacing: 0; }
  .s1 .pi { position: absolute; top: 606px; inset-inline-start: 64px; width: 60px; height: 60px; border: 2px solid var(--line); border-radius: 9px; color: var(--blue); display: grid; place-items: center; }
  .s1 .pi svg { width: 34px; height: 34px; }
  .s1 .pn { position: absolute; top: 607px; inset-inline-start: 145px; font-size: 18px; line-height: 26px; color: var(--ink-2); }
  .s1 .pn b { display: block; font-size: 19px; color: var(--ink-2); }
  /* 2-3 جدول الأعمال */
  .band { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 282px; background: linear-gradient(180deg, #E9E9E9 0%, #F1F1F1 45%, #F8F8F8 100%); }
  .band img { position: absolute; top: 0; inset-inline-end: 0; width: 735px; height: 282px; object-fit: cover; -webkit-mask-image: linear-gradient(90deg, transparent 0, #000 22%); mask-image: linear-gradient(90deg, transparent 0, #000 22%); }
  .ag-n { position: absolute; font-size: 63px; line-height: 64px; font-weight: 700; color: var(--blue); letter-spacing: -0.5px; }
  .ag-t { position: absolute; width: 322px; }
  /* 7-8 */
  .cb { position: absolute; display: flex; align-items: center; justify-content: center; text-align: center; padding: 0 54px; background: var(--blue); color: #fff; font-size: 20.5px; line-height: 25.5px; }
  /* 11 آراء العملاء */
  .s11 .card { position: absolute; top: 242px; width: 264px; height: 334px; background: var(--blue); }
  .s11 .card svg { position: absolute; top: 37px; inset-inline-start: 18px; width: 56px; height: 44px; color: var(--gray); }
  html[dir="rtl"] .s11 .card svg { transform: scaleX(-1); }
  .s11 .card p { position: absolute; top: 127px; inset-inline-start: 22px; width: 218px; font-size: 17.5px; line-height: 22.5px; color: #fff; }
  .s11 .nm { position: absolute; top: 595px; font-size: 20px; line-height: 28px; font-weight: 700; color: var(--ink); }
  /* 12 مؤشرات الأداء */
  .s12 .ico { position: absolute; width: 62px; height: 62px; color: var(--blue); }
  .s12 .ico.w { color: #fff; }
  .s12 .kt { position: absolute; font-size: 19.5px; line-height: 25.5px; color: var(--ink-2); }
  .s12 .kt.w { color: #fff; }
  .s12 .kb { position: absolute; display: flex; flex-direction: column; align-items: center; justify-content: flex-start; padding-top: 36px; background: var(--blue); color: #fff; }
  .s12 .kb.w { background: #fff; color: var(--blue); }
  .s12 .kb span { font-size: 20px; line-height: 26px; }
  .s12 .kb b { font-size: 58px; line-height: 60px; font-weight: 800; margin-top: 7px; letter-spacing: -0.5px; }
  /* 13 الحصة السوقية */
  .s13 .chart { position: absolute; top: 140px; inset-inline-start: 0; width: 720px; height: 560px; }
  .s13 .nb { position: absolute; inset-inline-start: 727px; width: 103px; height: 103px; display: grid; place-items: center; font-size: 39px; font-weight: 700; color: #fff; }
  .s13 .nb.c1 { background: var(--pale); color: var(--ink); } .s13 .nb.c2 { background: var(--blue); } .s13 .nb.c3 { background: var(--deep); }
  /* 14 القمع */
  .s14 svg.dec, .s15 svg.dec, .s16 svg.dec, .s17 svg.dec { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; }
  .num { position: absolute; display: grid; place-items: center; font-weight: 700; color: #fff; }
  /* 15 الدائرة الخماسية */
  .s15 .gn { position: absolute; width: 130px; height: 130px; display: grid; place-items: center; font-size: 44px; font-weight: 700; color: var(--blue); }
  .s15 .dc { position: absolute; width: 72px; height: 72px; display: grid; place-items: center; color: #fff; }
  .s15 .dc svg { width: 38px; height: 38px; }
  .s15 .ctr { position: absolute; top: 334px; inset-inline-start: 570px; width: 140px; text-align: center; font-size: 18px; line-height: 25.5px; font-weight: 700; color: #000; }
  /* 16 SWOT */
  .s16 .sl { position: absolute; width: 140px; height: 140px; display: grid; place-items: center; font-size: 100px; font-weight: 700; color: #fff; line-height: 1; }
  .s16 .sh { font-size: 24px; line-height: 32px; font-weight: 700; color: var(--blue); }
  .s16 .sh.l { color: var(--sky); }
  .s16 .it p.d { color: var(--ink-3); line-height: 25px; margin-top: 18px; }
  .s16 .si { position: absolute; width: 64px; height: 64px; color: var(--blue); }
  .s16 .si.l { color: var(--sky); }
  /* 18 مقارنة المنتجات */
  .s18 .rl { position: absolute; inset-inline-start: 63px; width: 230px; height: 61px; border-radius: 10px; background: var(--gray); padding-inline-start: 16px; display: flex; align-items: center; font-size: 17.5px; color: var(--ink-2); }
  .s18 .wb { position: absolute; top: 138px; font-size: 17px; line-height: 24px; color: var(--ink-2); }
  .s18 .grid { position: absolute; top: 160px; inset-inline-start: 290px; width: 940px; height: 470px; }
  .s18 .sep { position: absolute; top: 638px; inset-inline-start: 64px; width: 1152px; height: 1px; background: #E6E6E6; }
  .s18 .lg { position: absolute; top: 650px; display: flex; align-items: center; gap: 12px; font-size: 17.5px; color: var(--ink-2); }
  .s18 .lg i { width: 22px; height: 22px; border-radius: 50%; display: grid; place-items: center; color: #fff; }
  .s18 .lg i svg { width: 12px; height: 12px; }
  /* 19 التواصل */
  .s19 .vl { position: absolute; top: 226px; inset-inline-start: 65px; width: 3px; height: 217px; background: var(--sky); }
  .s19 .big { position: absolute; top: 523px; inset-inline-start: 64px; width: 360px; font-size: 52px; line-height: 64px; font-weight: 700; color: var(--blue); letter-spacing: -0.3px; }
  .s19 .cr { position: absolute; inset-inline-start: 716px; width: 502px; height: 116px; background: var(--blue); color: #fff; }
  .s19 .cr .ci { position: absolute; top: 23px; inset-inline-start: 22px; width: 40px; height: 40px; border-radius: 50%; background: var(--deep); display: grid; place-items: center; }
  .s19 .cr .ci svg { width: 20px; height: 20px; }
  .s19 .cr h3 { position: absolute; top: 12px; inset-inline-start: 80px; font-size: 21px; line-height: 30px; font-weight: 700; }
  .s19 .cr p { position: absolute; top: 50px; inset-inline-start: 81px; width: 352px; font-size: 17.5px; line-height: 23px; }
  .s19 .cr p.ph2 { line-height: 26px; }
  .s19 .cr.em h3 { top: 20px; } .s19 .cr.em p { top: 56px; }"""

# ===== أيقونات (خطية) =====
def ic(body, sw=1.3, vb='0 0 24 24'):
    return '<svg viewBox="%s" fill="none" stroke="currentColor" stroke-width="%s" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">%s</svg>' % (vb, sw, body)

ICONS = {
    'user': ic('<circle cx="12" cy="8" r="4"/><path d="M4.5 21c0-4.2 3.4-7 7.5-7s7.5 2.8 7.5 7"/>', 1.4),
    'users2': ic('<circle cx="9" cy="8" r="3.6"/><path d="M2.5 20c0-3.8 2.9-6.5 6.5-6.5s6.5 2.7 6.5 6.5"/><path d="M14.6 4.8a3.4 3.4 0 1 1 1.4 6.6"/><path d="M17.5 13.6c2.4.6 4 2.9 4 6.4"/>', 1.2),
    'coin': ic('<circle cx="12" cy="12" r="10"/><circle cx="12" cy="12" r="7.3"/><path d="M10 8h3a2 2 0 0 1 0 4h-3zm0 4h3.4a2 2 0 0 1 0 4H10zM10 8v8M11 6.6V8m2-1.4V8m-2 8v1.4m2-1.4v1.4"/>', 1.0),
    'bank': ic('<path d="M3 8.5 12 4l9 4.5z"/><path d="M3.5 8.5h17M5 10.5h14M4 19.5h11M3 21.5h12"/><path d="M6.5 11v7.5M9.5 11v7.5M12.5 11v6"/><circle cx="18.5" cy="18" r="3.6"/><path d="M19.6 16.6h-1.5a.8.8 0 0 0 0 1.5h.8a.8.8 0 0 1 0 1.5h-1.5m1.1-3.9v.9m0 3v.9"/>', 1.0),
    'coinup': ic('<circle cx="10" cy="9" r="6.5"/><path d="M8.4 6h2.2a1.5 1.5 0 0 1 0 3H8.4zm0 3h2.5a1.5 1.5 0 0 1 0 3H8.4zM8.4 6v6"/><path d="M2.5 21l5-4.5 3 2.5 6.5-6"/><path d="M14.5 13h3v3"/>', 1.0),
    'handshake': ic('<path d="M2 9l3-2 4 1.5L12 7l3 1.5 4-1.5 3 2"/><path d="M5 7l-3 6 3 1.5M19 7l3 6-3 1.5"/><path d="M8 13l2.2 2.2a1.1 1.1 0 0 0 1.6-1.6M10.5 12l2.6 2.6a1.1 1.1 0 0 0 1.6-1.6l-3.2-3.2M12.5 10.5l2.8 2.8a1.1 1.1 0 0 0 1.6-1.6L13 8M7 14.5l1.6 1.6a1.1 1.1 0 0 0 1.6-1.6"/>', 1.0),
    'team': ic('<circle cx="12" cy="8" r="2.6"/><circle cx="5.5" cy="9.5" r="2.1"/><circle cx="18.5" cy="9.5" r="2.1"/><path d="M7.6 17c0-2.6 2-4.6 4.4-4.6s4.4 2 4.4 4.6M2 17c0-2.2 1.6-3.8 3.5-3.8 1 0 1.8.4 2.4 1M22 17c0-2.2-1.6-3.8-3.5-3.8-1 0-1.8.4-2.4 1"/>', 1.0),
    'gears': ic('<circle cx="15" cy="8" r="2"/><path d="M15 3.5v1.6M15 10.9v1.6M10.5 8h1.6M17.9 8h1.6M11.8 4.8l1.1 1.1M17.1 10.1l1.1 1.1M11.8 11.2l1.1-1.1M17.1 5.9l1.1-1.1"/><circle cx="15" cy="8" r="3.2"/><circle cx="8" cy="16" r="1.7"/><circle cx="8" cy="16" r="2.9"/><path d="M8 11.9v1.2M8 18.9v1.2M3.9 16h1.2M10.9 16h1.2M5.1 13.1l.9.9M10 18l.9.9M5.1 18.9l.9-.9M10 14l.9-.9"/>', 1.0),
    'diamond': ic('<path d="M6 4h12l4 5-10 11L2 9z"/><path d="M2 9h20M9 4l-2.5 5L12 20l5.5-11L15 4"/>', 1.0),
    'board': ic('<path d="M2.5 4h19M4 4v11h16V4M12 15v5M8 21l4-6 4 6"/><path d="M6.5 7.5l.8.8 1.4-1.6M6.5 11l.8.8 1.4-1.6M10.5 7.8h6M10.5 11.3h6"/>', 1.0),
    'people3': ic('<circle cx="12" cy="9" r="2.6"/><circle cx="5" cy="6" r="2.3"/><circle cx="19" cy="6" r="2.3"/><path d="M7.2 19c0-2.8 2.1-5 4.8-5s4.8 2.2 4.8 5M1 14c0-2.4 1.8-4.2 4-4.2 1.2 0 2.3.5 3 1.4M23 14c0-2.4-1.8-4.2-4-4.2-1.2 0-2.3.5-3 1.4"/>', 0.9),
    'chair': ic('<rect x="7" y="2" width="10" height="10" rx="3"/><path d="M5 10v3h14v-3M4.5 10h1M18.5 10h1M12 13v4.5M8 17.5h8M8 17.5l-2 2.5M16 17.5l2 2.5M12 17.5V21"/><circle cx="6" cy="21" r=".7"/><circle cx="18" cy="21" r=".7"/><circle cx="12" cy="21.5" r=".7"/>', 0.9),
    'door': ic('<path d="M3 22h18M6 22V2h12v20"/><path d="M6 2l7 3v17"/><path d="M15 12h2M16.2 11l.8 1-.8 1"/>', 0.9),
    'chartdown': ic('<path d="M2 22h20M2 2v20"/><rect x="5" y="4" width="3" height="16"/><rect x="10" y="9" width="3" height="11"/><rect x="15" y="14" width="3" height="6"/><path d="M8 4l12 9M20 13v-3.5M20 13h-3.5"/>', 0.9),
    'pin': ic('<path d="M12 21s-6-6.2-6-11a6 6 0 0 1 12 0c0 4.8-6 11-6 11z"/><circle cx="12" cy="10" r="2.2"/>', 1.3),
    'mail': ic('<rect x="3" y="5.5" width="18" height="13" rx="1"/><path d="M3.5 6l8.5 7 8.5-7M3.5 18l6.5-6M20.5 18l-6.5-6"/>', 1.3),
    'phone': ic('<path d="M5 3.5h3.2l1.5 4-2.2 1.6a12 12 0 0 0 7.4 7.4l1.6-2.2 4 1.5V19a1.5 1.5 0 0 1-1.6 1.5C10 20 4 14 3.5 5.1A1.5 1.5 0 0 1 5 3.5z"/>', 1.3),
}
CHECK = '<svg viewBox="0 0 12 12" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M3 6.3l2 2 4-4.3"/></svg>'
QUOTE = '<svg viewBox="0 0 56 44" fill="currentColor" aria-hidden="true"><path d="M0 44V25C0 10 6 2 20 0l2 6c-7 2-10 6-10 13h11v25zM33 44V25c0-15 6-23 20-25l2 6c-7 2-10 6-10 13h11v25z"/></svg>'
LOGO = '<svg class="logo" viewBox="0 0 54 58" aria-hidden="true"><path fill="#fff" fill-rule="evenodd" d="M10 2 C24 -1 44 1 50 5 C55 9 55 40 51 48 C47 55 24 58 9 55 C2 53 0 44 1 30 C2 14 3 4 10 2 Z M17 13 C14 14 13 18 13 28 C13 38 14 42 17 44 C22 46 36 45 39 42 C42 39 42 20 40 15 C38 11 22 11 17 13 Z" transform="rotate(-6 27 29)"/></svg>'

# ===== الشرائح =====
S1 = ('\n    ' + box(80, 0, 882, 454, 'gr') + box(0, 0, 382, 186) + LOGO
      + '<p class="lgt">§LOGO1§<br>§LOGO2§</p>'
      + '\n    ' + img('cover-man', 194, 602, 678, 526, 'ALT_MAN')
      + box(240, 592, 22, 186)
      + '\n    <h1 class="k1">§C_K1§</h1><h1 class="k2">§C_K2§</h1><p class="k3">§C_K3§</p>'
      + '\n    <div class="pi">' + ICONS['user'] + '</div><p class="pn">§PRES§<b>§PNAME§</b></p>\n  ')


def agenda(n):
    cols, rows = (104, 488, 871), ((321, 398),) + (((498, 575),) if n == 6 else ())
    h = ('\n    <div class="band"><img class="flip" src="' + A + 'agenda-room.jpg" alt="§ALT_ROOM§"></div>'
         + box(0, 1072, 143, 114) + '\n    <h1 class="t48"' + pos(101, 104) + '>§AG_T§</h1>'
         + '<p class="tx"' + pos(184, 104, 450) + '>§AG_D§</p>' + '<div class="bar"' + pos(276, 104) + '></div>')
    k = 0
    for (tn, tt) in rows:
        for c in cols:
            k += 1
            h += '\n    <p class="ag-n"' + pos(tn, c) + '>0%d</p><p class="tx ag-t"%s>§AG_I§</p>' % (k, pos(tt, c, 322))
    if n == 3: h += box(650, 64, 1152, 70)
    return h + '\n  '


S4 = ('\n    ' + img('towers', 0, 0, 762, 314, 'ALT_TOW') + img('arch-tower', 0, 978, 302, 314, 'ALT_TOW')
      + box(194, 64, 484, 526) + '\n    <h1 class="t48 wt"' + pos(229, 114, 400) + '>§TTL_Y§</h1>'
      + '<p class="tx wt"' + pos(427, 113, 385) + '>§TX5§</p>'
      + '\n    <div class="bar"' + pos(306, 586) + '></div><h2 class="t40"' + pos(361, 586) + '>§PH§</h2>'
      + '<div class="tx"' + pos(431, 586, 620) + '><p>§TX3§</p><p>§TX2§</p></div>\n  ')

S5 = ('\n    ' + img('desk-charts', 54, 64, 1088, 356, 'ALT_DESK') + '<div class="sq"' + pos(0, 0) + '></div>'
      + box(184, 732, 484, 391) + '\n    <h1 class="t48 wt"' + pos(229, 781, 400) + '>§TTL_Y§</h1>'
      + '<p class="tx wt"' + pos(427, 781, 380) + '>§TXB§</p>'
      + '\n    <div class="bar"' + pos(404, 64) + '></div><h2 class="t40"' + pos(441, 64) + '>§PH§</h2>'
      + '<p class="tx"' + pos(508, 64, 620, None, ' line-height:24.7px;') + '>§TX3§</p>\n  ')

S6 = ('\n    ' + img('desk-laptop', 54, 128, 1088, 356, 'ALT_DESK') + '<div class="sq"' + pos(0, 1178) + '></div>'
      + box(184, 64, 484, 391) + '\n    <h1 class="t48 wt"' + pos(229, 114, 400) + '>§TTL_Y§</h1>'
      + '<p class="tx wt"' + pos(427, 113, 380) + '>§TXB§</p>'
      + '\n    <div class="bar"' + pos(404, 586) + '></div><h2 class="t40"' + pos(441, 586) + '>§PH§</h2>'
      + '<p class="tx"' + pos(508, 586, 620, None, ' line-height:24.7px;') + '>§TX3§</p>\n  ')

S7 = ('\n    <h1 class="t48"' + pos(67, 64, 520) + '>§TTL_Y§</h1>'
      + '<div class="tx"' + pos(206, 64, 525) + '><p>§TX4§</p><p>§TX2§</p></div>'
      + '\n    ' + img('laptop-top', 54, 640, 576, 608, 'ALT_LAP') + '<div class="sq"' + pos(0, 1178) + '></div>'
      + '<p class="cb"' + pos(452, 398, 522, 172) + '>§TXB§</p>\n  ')

S8 = ('\n    ' + img('team-charts', 69, 64, 576, 578, 'ALT_TEAM') + '<div class="sq"' + pos(0, 0) + '></div>'
      + '<p class="cb"' + pos(452, 356, 520, 172) + '>§TXB§</p>'
      + '\n    <h1 class="t48"' + pos(67, 686, 520) + '>§TTL_Y§</h1>'
      + '<div class="tx"' + pos(206, 686, 530, None, ' line-height:24.7px;') + '><p>§TX4§</p><p>§TX2§</p></div>\n  ')

S9 = ('\n    <h2 class="t40"' + pos(75, 66) + '>§HPH§</h2><div class="tx"' + pos(151, 66, 555, None, ' line-height:24.6px;') + '><p>§TX4b§</p><p>§TX2b§</p></div>'
      + '\n    <h2 class="t40"' + pos(75, 660) + '>§HPH§</h2><div class="tx"' + pos(151, 660, 555, None, ' line-height:24.6px;') + '><p>§TX4b§</p><p>§TX2b§</p></div>'
      + '\n    ' + img('meeting-room', 407, 66, 556, 248, 'ALT_ROOM') + img('typing', 407, 660, 556, 248, 'ALT_TYP')
      + '<div class="bar"' + pos(392, 66) + '></div><div class="bar"' + pos(392, 660) + '></div>'
      + '<div class="sq"' + pos(594, 588, 104) + '></div>\n  ')

S10 = ''.join('\n    <h2 class="t40"%s>§HPH2§</h2><p class="tx"%s>§TX4c§</p>' % (pos(45, x, 360, None, ' line-height:51px;'), pos(175, x, 352, None, ' line-height:24.7px;')) for x in (66, 468, 864))
S10 += ('\n    ' + img('typing-2', 407, 66, 353, 248, 'ALT_TYP') + img('meeting-room-2', 407, 468, 349, 248, 'ALT_ROOM') + img('team-charts-2', 407, 864, 352, 248, 'ALT_TEAM')
        + ''.join('<div class="bar"%s></div>' % pos(392, x) for x in (66, 468, 864)) + '<div class="sq"' + pos(594, 1178) + '></div>\n  ')


def stitle(k):
    return '<h1 class="st"><span class="lt">§%s_L1§</span>§%s_B§<span class="lt">§%s_L2§</span></h1>' % (k, k, k)


S11 = ('\n    ' + box(0, 1079, 201, 720, 'gr') + stitle('T11').replace('class="st"', 'class="st" style="top:63px"')
       + '<p class="tx"' + pos(144, 64, 495) + '>§TX2§</p>'
       + ''.join('\n    <div class="card"%s>%s<p>§QT§</p></div><p class="nm"%s>§NAME§</p>' % (pos(242, x), QUOTE, pos(595, x + 1)) for x in (368, 660, 952))
       + '\n    <div class="sq"' + pos(594, 0) + '></div>\n  ')

S12 = ('\n    ' + stitle('T12') + '<div class="bar"' + pos(120, 67, 185, 16) + '></div>'
       + box(136, 65, 280, 511, 'gr') + box(133, 368, 549, 245, 'gr') + box(403, 368, 549, 244, 'gr') + box(134, 938, 278, 512)
       + '\n    <div class="ico"' + pos(170, 100) + '>' + ICONS['users2'] + '</div><p class="kt"' + pos(252, 102, 198) + '>§KT§</p>'
       + '<div class="kb"' + pos(462, 102, 208, 152) + '><span>§PH0§</span><b>40%</b></div>'
       + '\n    <div class="ico"' + pos(179, 413) + '>' + ICONS['coin'] + '</div><p class="kt"' + pos(260, 414, 230) + '>§KT§</p>'
       + '<div class="kb"' + pos(180, 668, 206, 152) + '><span>§PH0§</span><b>75M</b></div>'
       + '\n    <div class="ico"' + pos(436, 417) + '>' + ICONS['bank'] + '</div><p class="kt"' + pos(516, 414, 230) + '>§KT§</p>'
       + '<div class="kb"' + pos(448, 668, 206, 154) + '><span>§PH0§</span><b>62%</b></div>'
       + '\n    <div class="ico w"' + pos(170, 981) + '>' + ICONS['coinup'] + '</div><p class="kt w"' + pos(252, 977, 198) + '>§KT§</p>'
       + '<div class="kb w"' + pos(462, 974, 208, 152) + '><span>§PH0§</span><b>47%</b></div>\n  ')

S13 = ('\n    ' + stitle('T13') + '<div class="bar"' + pos(121, 67, 184) + '></div>'
       + '<svg class="chart" id="share-chart" viewBox="0 140 720 560" aria-hidden="true"></svg>'
       + ''.join('\n    <div class="nb c%d"%s>0%d</div><div class="it"%s><h3 class="ih">§PH§</h3><p class="d">§TX1§</p></div>'
                 % (i + 1, pos(t, 727), i + 1, pos(t + 1, 864, 300)) for i, t in enumerate((190, 354, 538))) + '\n  ')

# ----- القمع (14) -----
FUN = [  # (TL, TR, bulge, lensLow, BL, BR, bottomC)
    ((486, 182), (778, 182), 173, 200, (498, 263), (767, 263), 280),
    ((507, 302), (755, 302), 293, 315, (520, 382), (743, 382), 397),
    ((527, 422), (736, 422), 413, 434, (540, 500), (725, 500), 515),
    ((546, 542), (715, 542), 533, 553, (558, 618), (703, 618), 633),
]


def funnel_svg():
    g = ''
    for (tl, tr, bul, low, bl, br, bc) in FUN:
        cx = (tl[0] + tr[0]) / 2
        cl = 2 * low - tl[1]; cb = 2 * bc - bl[1]; cu = 2 * bul - tl[1]
        g += ('<path fill="#0070C0" d="M%d,%d Q%.1f,%d %d,%d L%d,%d Q%.1f,%d %d,%d Z"/>' % (tl[0], tl[1], cx, cl, tr[0], tr[1], br[0], br[1], cx, cb, bl[0], bl[1])
              + '<path fill="#45B0FF" d="M%d,%d Q%.1f,%d %d,%d Q%.1f,%d %d,%d Z"/>' % (tl[0], tl[1], cx, cu, tr[0], tr[1], cx, cl, tl[0], tl[1]))
    for (x1, x2) in ((399, 470), (865, 792)):
        g += '<line x1="%d" y1="185" x2="%d" y2="612" stroke="#BFBFBF" stroke-width="1.2" stroke-dasharray="3 3"/>' % (x1, x2)
        g += '<path fill="#BFBFBF" d="M%d,622 l-5,-9 h10 z"/>' % x2
    for (x, y) in ((423, 343), (463, 581), (859, 224), (820, 462)):
        g += '<circle cx="%d" cy="%d" r="9" fill="#0070C0"/>' % (x, y)
    return '<svg class="dec" viewBox="0 0 1280 720" aria-hidden="true">' + g + '</svg>'


def item(top, start=None, end=None, w=240, cls=''):
    p = ('top:%dpx; inset-inline-start:%dpx; width:%dpx;' % (top, start, w)) if start is not None else ('top:%dpx; inset-inline-start:%dpx; width:%dpx;' % (top, end - w, w))
    return '<div class="it%s" style="%s"><h3 class="ih">§PH§</h3><p class="d">§TX1§</p></div>' % ('' if start is not None else ' e', p)


S14 = ('\n    ' + stitle('T14') + '<div class="bar"' + pos(121, 67, 184) + '></div>\n    ' + funnel_svg()
       + ''.join('<p class="num"%s>0%d</p>' % (pos(cy - 22, 600, 76, 44, ' font-size:30px;'), i + 1) for i, cy in enumerate((240, 355, 474, 588)))
       + '\n    ' + item(173, start=904) + item(298, end=377) + item(417, start=864) + item(538, end=418) + '\n  ')

# ----- الدائرة الخماسية (15) -----
GRAY = [(770, 203), (850, 450), (640, 602), (430, 450), (511, 203)]
DARK = [(640, 200), (814, 325), (746, 528), (533, 528), (466, 325)]


def penta_svg():
    g = ''
    for i, (gx, gy) in enumerate(GRAY):
        a, b = DARK[i], DARK[(i + 1) % 5]
        # القوس الداخلي (الأصغر) بنصف قطر 130 حول مركز الدائرة الرمادية
        cross = (a[0] - gx) * (b[1] - gy) - (a[1] - gy) * (b[0] - gx)
        g += '<path d="M%d,%d A130,130 0 0 %d %d,%d" fill="none" stroke="#45B0FF" stroke-opacity=".7" stroke-width="66"/>' % (a[0], a[1], 1 if cross > 0 else 0, b[0], b[1])
    for (gx, gy) in GRAY:
        g = '<circle cx="%d" cy="%d" r="65" fill="#F2F2F2"/>' % (gx, gy) + g
    for (x, y) in DARK:
        g += '<circle cx="%d" cy="%d" r="36" fill="#0070C0"/>' % (x, y)
    return '<svg class="dec" viewBox="0 0 1280 720" aria-hidden="true">' + g + '</svg>'


S15 = ('\n    ' + stitle('T15') + '<div class="bar"' + pos(121, 67, 184) + '></div>\n    ' + penta_svg()
       + ''.join('<p class="gn"%s>0%d</p>' % (pos(gy - 65, gx - 65), i + 1) for i, (gx, gy) in enumerate(GRAY))
       + ''.join('<div class="dc"%s>%s</div>' % (pos(y - 36, x - 36), ICONS[k]) for (x, y), k in zip(DARK, ('handshake', 'team', 'gears', 'diamond', 'board')))
       + '<p class="ctr">§CTR§</p>'
       + '\n    ' + item(150, start=914) + item(363, start=974) + item(572, start=833) + item(515, end=330) + item(226, end=330) + '\n  ')

# ----- SWOT (16) -----
def swot_svg():
    cx, cy, R, r, gap = 640, 405, 228, 78, 5
    g = ''
    cols = ('#0070C0', '#45B0FF', '#0070C0', '#45B0FF')  # أعلى يسار، أعلى يمين، أسفل يمين، أسفل يسار
    for q, (sx, sy) in enumerate(((-1, -1), (1, -1), (1, 1), (-1, 1))):
        ox, oy = cx + sx * gap, cy + sy * gap
        Ro, ri = R - gap, r - gap
        # ربع حلقة: من المحور الأفقي إلى الرأسي
        p1 = (ox + sx * Ro, oy); p2 = (ox, oy + sy * Ro); p3 = (ox, oy + sy * ri); p4 = (ox + sx * ri, oy)
        sweep = 1 if sx * sy > 0 else 0
        g += '<path fill="%s" d="M%.1f,%.1f A%d,%d 0 0 %d %.1f,%.1f L%.1f,%.1f A%d,%d 0 0 %d %.1f,%.1f Z"/>' % (
            cols[q], p1[0], p1[1], Ro, Ro, sweep, p2[0], p2[1], p3[0], p3[1], ri, ri, 1 - sweep, p4[0], p4[1])
    return '<svg class="dec" viewBox="0 0 1280 720" aria-hidden="true">' + g + '</svg>'


def swot_item(top, k, cls, start=None, end=None):
    w = 260
    p = ('inset-inline-start:%dpx;' % start) if start is not None else ('inset-inline-start:%dpx; text-align:end;' % (end - w))
    return '<div class="it" style="top:%dpx; %s width:%dpx;"><h3 class="sh%s">§%s§</h3><p class="d">§TX1§</p></div>' % (top, p, w, cls, k)


S16 = ('\n    ' + stitle('T16') + '<div class="bar"' + pos(121, 67, 184) + '></div>\n    ' + swot_svg()
       + ''.join('<p class="sl"%s>§L_%s§</p>' % (pos(cy - 70, cx - 70), k) for k, cx, cy in (('S', 530, 312), ('W', 752, 312), ('O', 530, 512), ('T', 746, 512)))
       + '\n    ' + swot_item(203, 'SW_S', '', end=297) + swot_item(203, 'SW_W', ' l', start=986)
       + swot_item(486, 'SW_O', ' l', end=297) + swot_item(486, 'SW_T', '', start=986)
       + '<div class="si"' + pos(210, 328) + '>' + ICONS['people3'] + '</div><div class="si l"' + pos(207, 896) + '>' + ICONS['chair'] + '</div>'
       + '<div class="si l"' + pos(494, 327) + '>' + ICONS['door'] + '</div><div class="si"' + pos(492, 896) + '>' + ICONS['chartdown'] + '</div>\n  ')

# ----- هرم الأحجية (17) -----
def knob(bx, by, dx, dy, fill):
    """نتوء أحجية: قاعدته عند (bx,by) على خط الفصل ويتجه نحو (dx,dy)؛ يُلوَّن بلون القطعة المالكة بحدّ أبيض."""
    n, r, d = 10, 24, 33  # نصف عرض العنق، نصف القطر، بعد المركز عن القاعدة
    px, py = -dy, dx
    cx, cy = bx + dx * d, by + dy * d
    off = math.sqrt(r * r - n * n)
    a1 = (bx + px * n, by + py * n); a2 = (bx - px * n, by - py * n)
    c1 = (cx + px * n - dx * off, cy + py * n - dy * off); c2 = (cx - px * n - dx * off, cy - py * n - dy * off)
    q = lambda pts: ' '.join('%.1f,%.1f' % p for p in pts)
    return ('<circle cx="%.1f" cy="%.1f" r="%d" fill="%s" stroke="#fff" stroke-width="3"/>' % (cx, cy, r, fill)
            + '<polygon fill="%s" points="%s"/>' % (fill, q([(a1[0] - dx * 3, a1[1] - dy * 3), (c1[0] + dx * 6, c1[1] + dy * 6), (c2[0] + dx * 6, c2[1] + dy * 6), (a2[0] - dx * 3, a2[1] - dy * 3)]))
            + '<path fill="none" stroke="#fff" stroke-width="3" d="M%.1f,%.1f L%.1f,%.1f M%.1f,%.1f L%.1f,%.1f"/>' % (a1[0] + dx * 1.5, a1[1] + dy * 1.5, c1[0], c1[1], a2[0] + dx * 1.5, a2[1] + dy * 1.5, c2[0], c2[1]))


def pyramid_svg():
    D, L = '#0070C0', '#45B0FF'
    st = ' stroke="#fff" stroke-width="3" stroke-linejoin="round"'
    g = ('<polygon fill="%s"%s points="640,117 767,373 513,373"/>' % (D, st)
         + '<polygon fill="%s"%s points="513,373 640,373 640,523 438.5,523"/>' % (L, st)
         + '<polygon fill="%s"%s points="640,373 767,373 841.5,523 640,523"/>' % (D, st)
         + '<polygon fill="%s"%s points="438.5,523 640,523 640,663 369,663"/>' % (D, st)
         + '<polygon fill="%s"%s points="640,523 841.5,523 912,663 640,663"/>' % (L, st))
    g += knob(577, 373, 0, -1, L) + knob(702, 373, 0, -1, D) + knob(640, 441, 1, 0, L)
    g += knob(577, 523, 0, -1, D) + knob(702, 523, 0, -1, L) + knob(640, 593, -1, 0, L)
    return '<svg class="dec" viewBox="0 0 1280 720" aria-hidden="true">' + g + '</svg>'


S17 = ('\n    ' + stitle('T17') + '<div class="bar"' + pos(121, 67, 184) + '></div>\n    ' + pyramid_svg()
       + ''.join('<p class="num"%s>0%d</p>' % (pos(cy - 26, cx - 45, 90, 52, ' font-size:40px;'), i + 1) for i, (cx, cy) in enumerate(((640, 260), (570, 433), (741, 433), (506, 593), (774, 593))))
       + '\n    ' + item(148, start=748) + item(333, end=443) + item(333, start=838) + item(508, end=348) + item(508, start=934) + '\n  ')

S18 = ('\n    ' + stitle('T18') + '<div class="bar"' + pos(121, 67, 184) + '></div>'
       + '<p class="wb"' + pos(138, 308) + '>§WORST§</p><p class="wb"' + pos(138, 1036, 180, None, ' text-align:end;') + '>§BEST§</p>'
       + ''.join('<p class="rl"%s>§ROWL§</p>' % pos(t, 63) for t in (174, 242, 310, 379, 447, 516))
       + '\n    <svg class="grid" id="cmp-chart" viewBox="290 160 940 470" aria-hidden="true"></svg><div class="sep"></div>'
       + ''.join('<p class="lg"%s><i style="background:%s">%s</i>§PROD§ 0%d</p>' % (pos(650, x), c, CHECK, i + 1) for i, (x, c) in enumerate(((68, '#0070C0'), (268, '#45B0FF'), (468, '#C1E5FF'), (667, '#0D3A63'))))
       + '\n  ')

S19 = ('\n    <div class="tx"' + pos(57, 66, 330, None, ' line-height:22px;') + '><p>§TX19A§</p><p>§TX19B§</p></div><div class="vl"></div>'
       + '<h1 class="big">§EDIT§</h1>'
       + '\n    ' + img('contact-laptop', 0, 436, 408, 720, 'ALT_LAP')
       + '<div class="cr"' + pos(67, 716) + '><div class="ci">' + ICONS['pin'] + '</div><h3>§ADDR§</h3><p>§ADDR_T§</p></div>'
       + '<div class="cr em"' + pos(203, 716) + '><div class="ci">' + ICONS['mail'] + '</div><h3>§MAIL§</h3><p>§MAIL_T§</p></div>'
       + '<div class="cr"' + pos(340, 716) + '><div class="ci">' + ICONS['phone'] + '</div><h3>§PHONE§</h3><p class="ph2"><bdi dir="ltr">+1 (212) 999 9090</bdi><br><bdi dir="ltr">+1 (212) 999 0909</bdi></p></div>\n  ')

SLIDES = [S1, agenda(6), agenda(3), S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, S14, S15, S16, S17, S18, S19]

JSON_BLOCKS = {
    # الحصة السوقية: الفئات من الأعلى إلى الأسفل، والقيمة نسبة مئوية من الحد الأقصى max
    'chart-data': {'labels': ['2028', '2027', '2026'], 'values': [19, 47, 26], 'max': 60, 'colors': ['#C1E5FF', '#0070C0', '#005595']},
    # مقارنة المنتجات: لكل صف (معيار) درجة كل منتج من 1 (الأسوأ) إلى 10 (الأفضل)
    'chart-data-2': {'scale': 10, 'colors': ['#0070C0', '#45B0FF', '#C1E5FF', '#0D3A63'],
                     'rows': [[4, 3, 8, 6], [9, 1, 7, 4], [3, 7, 6, 8], [9, 4, 7, 3], [10, 2, 5, 4], [9, 3, 7, 4]]},
}

EXTRA_SCRIPT = r"""  // ----- رسم الحصة السوقية (أعمدة أفقية) ومصفوفة المقارنة؛ X() يعكس الإحداثيات للعربية -----
  var NS = 'http://www.w3.org/2000/svg';
  function J(id) { return JSON.parse(document.getElementById(id).textContent); }
  function E(tag, a, p) { var e = document.createElementNS(NS, tag); for (var k in a) e.setAttribute(k, a[k]); if (p) p.appendChild(e); return e; }
  (function () {
    var d = J('chart-data'), s = document.getElementById('share-chart'), W = 720, X = function (x) { return RTL ? W - x : x; };
    var x0 = 133, full = 600, top = 188, step = 177, bh = 103;
    E('line', { x1: X(132), y1: 152, x2: X(132), y2: 680, stroke: '#D9D9D9', 'stroke-width': 1 }, s);
    d.labels.forEach(function (lb, i) {
      var w = d.values[i] / d.max * full, y = top + i * step;
      E('rect', { x: RTL ? W - x0 - w : x0, y: y, width: w, height: bh, fill: d.colors[i % d.colors.length] }, s);
      var t = E('text', { x: X(93), y: y + bh / 2, 'text-anchor': 'middle', 'dominant-baseline': 'central', 'font-size': 17, fill: '#404040' }, s); t.textContent = lb;
    });
  })();
  (function () {
    var d = J('chart-data-2'), s = document.getElementById('cmp-chart'), L = 307, R = 1218, T = 174, B = 577;
    var n = d.scale, cw = (R - L) / n, rh = (B - T) / d.rows.length, X = function (x) { return RTL ? L + R - x : x; };
    var st = { stroke: '#BFBFBF', 'stroke-width': 1.3, 'stroke-dasharray': '1.5 2.5' };
    for (var c = 0; c <= n; c++) E('line', Object.assign({ x1: L + c * cw, y1: T, x2: L + c * cw, y2: B }, st), s);
    for (var r = 0; r <= d.rows.length; r++) E('line', Object.assign({ x1: L, y1: T + r * rh, x2: R, y2: T + r * rh }, st), s);
    for (c = 1; c <= n; c++) { var t = E('text', { x: X(L + (c - .5) * cw), y: 601, 'text-anchor': 'middle', 'dominant-baseline': 'central', 'font-size': 17, 'font-weight': 700, fill: '#404040' }, s); t.textContent = c; }
    d.rows.forEach(function (row, ri) {
      row.forEach(function (v, pi) {
        var cx = X(L + (v - .5) * cw), cy = T + (ri + .5) * rh;
        E('circle', { cx: cx, cy: cy, r: 11, fill: d.colors[pi] }, s);
        E('path', { d: 'M' + (cx - 4.5) + ',' + (cy + .3) + ' l3,3 l6,-6.4', fill: 'none', stroke: '#fff', 'stroke-width': 2, 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, s);
      });
    });
  })();"""

STR = {
    'ar': {
        'LOGO1': 'شعار شركتك', 'LOGO2': 'هنا', 'ALT_MAN': 'رجل يراجع تقريرًا ماليًا بآلة حاسبة',
        'C_K1': 'عرض', 'C_K2': 'تقرير الأعمال', 'C_K3': 'قالب عرض تقديمي', 'PRES': 'مقدّم العرض', 'PNAME': 'الاسم',
        'ALT_ROOM': 'قاعة اجتماعات', 'AG_T': 'جدول الأعمال', 'AG_D': 'أدخل النص المطلوب هنا. هذا نص توضيحي. عبارة موجزة عن شركتك.',
        'AG_I': 'أدخل النص المطلوب هنا. هذا نص توضيحي. عبارة موجزة عن شركتك.',
        'ALT_TOW': 'أبراج زجاجية', 'TTL_Y': 'أدخل العنوان المطلوب هنا',
        'TX5': 'هذا نص توضيحي. أدخل النص المطلوب هنا. هذا نص توضيحي. أدخل النص المطلوب هنا. هذا نص توضيحي. أدخل النص المطلوب هنا. هذا نص توضيحي. أدخل النص المطلوب هنا.',
        'PH': 'عنوان نائب',
        'TX3': 'هذا نص توضيحي. أدخل النص المطلوب هنا. هذا نص توضيحي. أدخل النص المطلوب هنا. هذا نص توضيحي. أدخل النص المطلوب هنا. هذا نص توضيحي. أدخل النص المطلوب هنا.',
        'TX2': 'هذا نص توضيحي. أدخل النص المطلوب هنا. هذا نص توضيحي. أدخل النص المطلوب هنا.',
        'TXB': 'هذا نص توضيحي. أدخل النص المطلوب هنا. هذا نص توضيحي. هذا نص توضيحي.',
        'ALT_DESK': 'مكتب عمل بحاسوب محمول وتقارير', 'ALT_LAP': 'يدان تكتبان على حاسوب محمول', 'ALT_TEAM': 'فريق يراجع لوحات بيانات',
        'TX4': 'هذا نص توضيحي. أدخل النص المطلوب هنا. هذا نص توضيحي. أدخل النص المطلوب هنا. هذا نص توضيحي. أدخل النص المطلوب هنا. هذا نص توضيحي. أدخل النص المطلوب هنا.',
        'HPH': 'عنوان رئيسي نائب', 'HPH2': 'عنوان رئيسي نائب',
        'TX4b': 'هذا نص توضيحي. أدخل النص المطلوب هنا. هذا نص توضيحي. أدخل النص المطلوب هنا. هذا نص توضيحي. أدخل النص المطلوب هنا. هذا نص توضيحي. أدخل النص المطلوب هنا.',
        'TX2b': 'هذا نص توضيحي. أدخل النص المطلوب هنا. هذا نص توضيحي. أدخل النص المطلوب هنا.',
        'TX4c': 'هذا نص توضيحي. أدخل النص المطلوب هنا. هذا نص توضيحي. أدخل النص المطلوب هنا. هذا نص توضيحي. أدخل النص المطلوب هنا.',
        'ALT_TYP': 'كتابة على حاسوب محمول',
        'T11_L1': 'شريحة ', 'T11_B': 'آراء العملاء', 'T11_L2': '', 'QT': 'هذا نص توضيحي. أدخل النص المطلوب هنا. هذا نص توضيحي. أدخل النص المطلوب هنا.', 'NAME': 'الاسم',
        'T12_L1': 'شريحة ', 'T12_B': 'المؤشرات الأربعة', 'T12_L2': '', 'KT': 'هذا نص توضيحي. أدخل النص المطلوب هنا.', 'PH0': 'عنوان نائب',
        'T13_L1': 'شريحة ', 'T13_B': 'الحصة السوقية', 'T13_L2': '', 'TX1': 'هذا نص توضيحي. أدخل النص المطلوب هنا.',
        'T14_L1': 'شريحة ', 'T14_B': 'قمع المبيعات', 'T14_L2': '',
        'T15_L1': 'شريحة ', 'T15_B': 'المخطط الدائري الخماسي', 'T15_L2': '', 'CTR': 'مخطط إنفوجرافيك متقارب خماسي الاتجاهات',
        'T16_L1': 'شريحة ', 'T16_B': 'التحليل الرباعي', 'T16_L2': '',
        'L_S': 'ق', 'L_W': 'ض', 'L_O': 'ف', 'L_T': 'ت', 'SW_S': 'نقاط القوة', 'SW_W': 'نقاط الضعف', 'SW_O': 'الفرص', 'SW_T': 'التهديدات',
        'T17_L1': 'شريحة ', 'T17_B': 'هرم الأحجية', 'T17_L2': '',
        'T18_L1': 'شريحة ', 'T18_B': 'مقارنة المنتجات', 'T18_L2': '', 'WORST': 'الأسوأ', 'BEST': 'الأفضل', 'ROWL': 'نص نائب', 'PROD': 'المنتج',
        'TX19A': 'أدخل النص المطلوب هنا. هذا نص توضيحي. عبارة موجزة عن شركتك.', 'TX19B': 'أدخل النص المطلوب هنا. هذا نص توضيحي.',
        'EDIT': 'عدّل هذا النص هنا', 'ADDR': 'العنوان', 'ADDR_T': 'أدخل النص المطلوب هنا. هذا نص توضيحي. أدخل النص المطلوب هنا.',
        'MAIL': 'البريد الإلكتروني', 'MAIL_T': 'عنوان البريد الإلكتروني للشركة', 'PHONE': 'الهاتف',
    },
    'en': {
        'LOGO1': 'YOUR COMPANY', 'LOGO2': 'LOGO HERE', 'ALT_MAN': 'Man reviewing a financial report with a calculator',
        'C_K1': 'REPORT', 'C_K2': 'PRESENTATION', 'C_K3': 'PRESENTATION TEMPLATE', 'PRES': 'Presenter', 'PNAME': 'Name',
        'ALT_ROOM': 'Meeting room', 'AG_T': 'Agenda', 'AG_D': 'Insert your desired text here. This is a sample text. A brief statement about your company.',
        'AG_I': 'Insert your desired text here. This is a sample text. A brief statement about your company.',
        'ALT_TOW': 'Glass towers', 'TTL_Y': 'Insert Your Desired Title Here',
        'TX5': 'This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here.',
        'PH': 'Placeholder',
        'TX3': 'This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here.',
        'TX2': 'This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here.',
        'TXB': 'This is a sample text. Insert your desired text here. This is a sample text. This is a sample text.',
        'ALT_DESK': 'Work desk with a laptop and reports', 'ALT_LAP': 'Hands typing on a laptop', 'ALT_TEAM': 'Team reviewing data dashboards',
        'TX4': 'This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here.',
        'HPH': 'Header Placeholder', 'HPH2': 'Header Placeholder',
        'TX4b': 'This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here.',
        'TX2b': 'This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here.',
        'TX4c': 'This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here.',
        'ALT_TYP': 'Typing on a laptop',
        'T11_L1': '', 'T11_B': 'Testimonials', 'T11_L2': ' Slide', 'QT': 'This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here.', 'NAME': 'Name',
        'T12_L1': '', 'T12_B': '4-Item KPI', 'T12_L2': ' Slide', 'KT': 'This is a sample text. Insert your desired text here.', 'PH0': 'Placeholder',
        'T13_L1': '', 'T13_B': 'Market Share', 'T13_L2': ' Slide', 'TX1': 'This is a sample text. Insert your desired text here.',
        'T14_L1': '', 'T14_B': 'Sales Funnel', 'T14_L2': ' Slide',
        'T15_L1': '', 'T15_B': '5 Segment Circular Diagram', 'T15_L2': ' Slide', 'CTR': '5-Way Converging Infographic Diagram',
        'T16_L1': '', 'T16_B': 'SWOT Analysis', 'T16_L2': ' Slide',
        'L_S': 'S', 'L_W': 'W', 'L_O': 'O', 'L_T': 'T', 'SW_S': 'STRENGTHS', 'SW_W': 'WEAKNESSES', 'SW_O': 'OPPORTUNITIES', 'SW_T': 'THREATS',
        'T17_L1': '', 'T17_B': 'Puzzle Pyramid', 'T17_L2': ' Slide',
        'T18_L1': '', 'T18_B': 'Product Comparison', 'T18_L2': ' Slide', 'WORST': 'WORST', 'BEST': 'BEST', 'ROWL': 'Placeholder Text', 'PROD': 'Product',
        'TX19A': 'Insert your desired text here. This is a sample text. A brief statement about your company.', 'TX19B': 'Insert your desired text here. This is a sample text.',
        'EDIT': 'Edit this text here', 'ADDR': 'Address', 'ADDR_T': 'Insert your desired text here. This is a sample text. Insert your desired text here.',
        'MAIL': 'Email', 'MAIL_T': 'Companyname@gmail.com', 'PHONE': 'Phone',
    },
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'general', 'icon': '📘',
    'name': 'عرض تقرير أعمال', 'nameEn': 'Business Report Presentation',
    'description': 'تسع عشرة شريحة زرقاء بصور: غلاف وجدول أعمال ونصوص ومؤشرات وحصة سوقية وقمع وSWOT ومقارنة منتجات وتواصل',
    'descriptionEn': '19 blue photo slides: cover, agenda, text layouts, KPIs, market share, funnel, SWOT, product comparison, contact',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'جدول الأعمال (ستة بنود)', 'titleEn': 'Agenda (6 items)'},
        {'title': 'جدول الأعمال (ثلاثة بنود)', 'titleEn': 'Agenda (3 items)'},
        {'title': 'عنوان ونص مع صور أبراج', 'titleEn': 'Title & Text with Towers'},
        {'title': 'صورة عريضة مع عنوان أيمن', 'titleEn': 'Wide Photo, Title Right'},
        {'title': 'صورة عريضة مع عنوان أيسر', 'titleEn': 'Wide Photo, Title Left'},
        {'title': 'عنوان ونص مع صورة جانبية', 'titleEn': 'Title & Text with Side Photo'},
        {'title': 'صورة جانبية مع عنوان ونص', 'titleEn': 'Side Photo with Title & Text'},
        {'title': 'عمودان بصورتين', 'titleEn': 'Two Columns with Photos'},
        {'title': 'ثلاثة أعمدة بصور', 'titleEn': 'Three Columns with Photos'},
        {'title': 'آراء العملاء', 'titleEn': 'Testimonials'},
        {'title': 'أربعة مؤشرات أداء', 'titleEn': '4-Item KPI', 'needsData': True},
        {'title': 'الحصة السوقية (رسم)', 'titleEn': 'Market Share (chart)', 'needsData': True},
        {'title': 'قمع المبيعات', 'titleEn': 'Sales Funnel'},
        {'title': 'المخطط الدائري الخماسي', 'titleEn': '5 Segment Circular Diagram'},
        {'title': 'التحليل الرباعي', 'titleEn': 'SWOT Analysis'},
        {'title': 'هرم الأحجية', 'titleEn': 'Puzzle Pyramid'},
        {'title': 'مقارنة المنتجات (مصفوفة)', 'titleEn': 'Product Comparison (matrix)', 'needsData': True},
        {'title': 'التواصل', 'titleEn': 'Contact'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص والأرقام فقط؛ أعمدة الحصة السوقية في كتلة chart-data (labels/values/max) ودرجات مقارنة المنتجات في كتلة chart-data-2 (rows: لكل معيار درجة كل منتج من 1 إلى 10).',
}
