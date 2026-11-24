# -*- coding: utf-8 -*-
# نموذج «مراجعة التخطيط الضريبي السنوي»: خمس عشرة شريحة بهوية كحلية/زرقاء/خضراء وزخارف «أشرطة» مائلة مدوّرة الطرف بتدرّجات شفافة.
import math

KEY = 'sl-annual-tax-planning-review'
TITLE = {'ar': 'مراجعة التخطيط الضريبي السنوي', 'en': 'Annual Tax Planning Review'}
COMMENT = 'نموذج «مراجعة التخطيط الضريبي السنوي»: غلاف كحلي، جدول أعمال، ملخص تنفيذي بمؤشرات، جدول مالي، أشرطة الالتزامات (chart-data)، بطاقات الاستراتيجية، الوفورات، الامتثال، المخاطر، الفرص، التعديلات، تخطيط الأرباع، الخلاصات، الأسئلة، والختام. الزخارف SVG خلف المحتوى وتنعكس في العربية.'
VARS = '--ink: #111418; --ink-2: #2A2E35; --navy: #13236A; --blue: #2653E8; --blue-2: #2D55D8; --green: #45D48A; --green-t: #3CC985; --green-d: #24B06B; --cyan: #74D3FF; --cyan-t: #22AEEF; --red: #8B0A0A; --line: #D6D8DC; --accent: #2653E8; --bg: #FFFFFF;'

FONTS = {'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@300;400;600;700;800&display=swap" rel="stylesheet">'}
FONT_STACK = {'en': "'Open Sans', 'Segoe UI', Tahoma, Arial, sans-serif"}

# ---------- زخارف SVG: «شعاع» بطرف مدوّر يتّسع نحو الطرف الآخر، أو «كبسولة» بطرفين مدوّرين ----------
_DEFS = []


def beam(x1, y1, x2, y2, r, w=None, stops=(), cap=False, op=1.0):
    """طرف مدوّر عند (x1,y1) بنصف قطر r، والطرف الآخر عند (x2,y2) بنصف عرض w (أو مدوّر إن cap)."""
    w = r if w is None else w
    dx, dy = x2 - x1, y2 - y1
    L = math.hypot(dx, dy); d = (dx / L, dy / L); n = (-d[1], d[0])
    pts = []
    for k in range(13):
        t = math.pi * k / 12
        pts.append((x1 + r * (n[0] * math.cos(t) - d[0] * math.sin(t)), y1 + r * (n[1] * math.cos(t) - d[1] * math.sin(t))))
    if cap:
        for k in range(13):
            t = math.pi * k / 12
            pts.append((x2 + r * (-n[0] * math.cos(t) + d[0] * math.sin(t)), y2 + r * (-n[1] * math.cos(t) + d[1] * math.sin(t))))
        ex, ey = x2 + d[0] * r, y2 + d[1] * r
    else:
        pts += [(x2 - n[0] * w, y2 - n[1] * w), (x2 + n[0] * w, y2 + n[1] * w)]
        ex, ey = x2, y2
    gid = 'tg%d' % (len(_DEFS) + 1)
    st = ''.join('<stop offset="%s" stop-color="%s" stop-opacity="%s"/>' % s for s in stops)
    _DEFS.append('<linearGradient id="%s" gradientUnits="userSpaceOnUse" x1="%.0f" y1="%.0f" x2="%.0f" y2="%.0f">%s</linearGradient>'
                 % (gid, x1 - d[0] * r, y1 - d[1] * r, ex, ey, st))
    dstr = 'M' + ' L'.join('%.1f %.1f' % p for p in pts) + 'Z'
    return '<path d="%s" fill="url(#%s)"%s/>' % (dstr, gid, '' if op == 1 else ' opacity="%s"' % op)


def poly(pts, x1, y1, x2, y2, stops, op=1.0):
    """مضلّع بتدرّج خطي بين نقطتين."""
    gid = 'tg%d' % (len(_DEFS) + 1)
    st = ''.join('<stop offset="%s" stop-color="%s" stop-opacity="%s"/>' % s for s in stops)
    _DEFS.append('<linearGradient id="%s" gradientUnits="userSpaceOnUse" x1="%d" y1="%d" x2="%d" y2="%d">%s</linearGradient>' % (gid, x1, y1, x2, y2, st))
    return '<polygon points="%s" fill="url(#%s)"%s/>' % (' '.join('%d,%d' % p for p in pts), gid, '' if op == 1 else ' opacity="%s"' % op)


def dec(*parts):
    return '<svg class="dec" viewBox="0 0 1280 720" aria-hidden="true">' + ''.join(parts) + '</svg>'


GREEN = ((0, '#4FD690', 1), (0.45, '#BDEED4', .8), (1, '#FFFFFF', 0))
LAV = ((0, '#9DB1F6', .95), (0.4, '#DDE4FC', .7), (0.8, '#FFFFFF', 0))
SKY = ((0, '#8EAEEA', .95), (1, '#FFFFFF', 0))
RLAV = ((0, '#5E80F0', .95), (0.65, '#D5DEFB', .7), (1, '#FFFFFF', 0))
BLUEP = ((0, '#2A56EC', 1), (1, '#6383F1', 1))
TEALP = ((0, '#64C5B2', .95), (0.6, '#79A8E4', .85), (1, '#3F66EE', .25))


def motif(ox, oy, s=1.0, m=1):
    """زخرفة «V» المتكررة: شعاع أخضر ينتهي برأس مدوّر، كبسولة فيروزية صاعدة إلى رأس أزرق، كبسولة زرقاء نازلة، وظلال خزامية."""
    P = lambda x, y: (ox + m * s * x, oy + s * y)
    r = 58 * s
    return (beam(*P(145, -180), *P(150, 560), r, 330 * s, LAV)
            + beam(*P(145, -180), *P(100, -640), r, 210 * s, SKY)
            + beam(*P(205, -85), *P(520, -640), r, 150 * s, RLAV)
            + beam(*P(0, 0), *P(-230, -560), r, 130 * s, GREEN)
            + beam(*P(145, -180), *P(205, -85), r, stops=BLUEP, cap=True)
            + beam(*P(0, 0), *P(145, -180), r, stops=TEALP, cap=True, op=.9))


ICON = {
    'up': '<svg viewBox="0 0 32 32" aria-hidden="true"><circle cx="16" cy="16" r="13.5" fill="none" stroke="currentColor" stroke-width="2.6"/><path d="M16 23V10M10.5 15.5 16 10l5.5 5.5" fill="none" stroke="currentColor" stroke-width="2.6" stroke-linecap="round" stroke-linejoin="round"/></svg>',
    'down': '<svg viewBox="0 0 32 32" aria-hidden="true"><circle cx="16" cy="16" r="13.5" fill="none" stroke="currentColor" stroke-width="2.6"/><path d="M16 9v13M10.5 16.5 16 22l5.5-5.5" fill="none" stroke="currentColor" stroke-width="2.6" stroke-linecap="round" stroke-linejoin="round"/></svg>',
    'check': '<svg viewBox="0 0 36 36" aria-hidden="true"><circle cx="18" cy="18" r="18" fill="currentColor"/><path d="M10.5 18.5l5 5 10-10.5" fill="none" stroke="#fff" stroke-width="3.2" stroke-linecap="round" stroke-linejoin="round"/></svg>',
    'minus': '<svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="12" fill="currentColor"/><path d="M7 12h10" stroke="#fff" stroke-width="2.6" stroke-linecap="round"/></svg>',
    'mail': '<svg viewBox="0 0 34 34" aria-hidden="true" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linejoin="round"><path d="M3 14l14-10 14 10v17H3z"/><path d="M3 14l14 10 14-10"/><rect x="9" y="7" width="16" height="13" fill="#13236A"/><path d="M12 11h10M12 14.5h10M12 18h6"/></svg>',
    'phone': '<svg viewBox="0 0 34 34" aria-hidden="true"><path d="M9 4l5 6-3 3c1.5 3.5 4.5 6.5 8 8l3-3 6 5-3 5C14 27 6 19 5 8z" fill="currentColor"/><path d="M19 9a7 7 0 0 1 6 6M20 4a12 12 0 0 1 10 10" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round"/></svg>',
    'globe': '<svg viewBox="0 0 34 34" aria-hidden="true" fill="none" stroke="currentColor" stroke-width="2.2"><circle cx="17" cy="17" r="15"/><ellipse cx="17" cy="17" rx="6.5" ry="15"/><path d="M2 17h30M17 2v30M4.5 9h25M4.5 25h25"/></svg>',
    'logo': '<svg viewBox="0 0 56 60" aria-hidden="true"><path fill="currentColor" fill-rule="evenodd" d="M9 5C22 0 37 0 49 7c5 15 5 30 0 45C37 60 20 60 6 54 1 40 1 22 9 5zM17 17c-2 9-2 18 0 26 9 3 18 3 26-1 2-8 2-16 0-24-8-4-17-4-26-1z"/></svg>',
}

STYLE = """  .dec { position: absolute; inset: 0; width: 1280px; height: 720px; }
  html[dir="rtl"] .dec { transform: scaleX(-1); }
  .frame > *:not(.dec) { z-index: 1; }
  h1, h2, h3, p { margin: 0; }
  .co { position: absolute; top: 73px; inset-inline-start: 42px; font-size: 17px; letter-spacing: 3.2px; line-height: 1.2; color: var(--ink); }
  html[lang="ar"] .co { letter-spacing: 0; font-size: 19px; }
  .ttl { position: absolute; top: 108px; inset-inline-start: 35px; font-size: 42px; font-weight: 400; line-height: 52px; color: var(--navy); white-space: nowrap; letter-spacing: -.3px; }
  .ttl b { font-weight: 700; }
  html[lang="ar"] .ttl { letter-spacing: 0; font-size: 42px; }
  .sub { position: absolute; top: 185px; inset-inline-start: 37px; font-size: 22px; line-height: 29px; color: var(--ink); white-space: nowrap; }
  .it { position: absolute; }
  .it h2 { font-size: 22px; font-weight: 700; line-height: 29px; }
  .it p { font-size: 18px; line-height: 23px; color: var(--ink); }
  .kh { font-size: 17px; font-weight: 700; letter-spacing: 3px; color: var(--navy); line-height: 1.2; }
  html[lang="ar"] .kh { letter-spacing: 0; font-size: 19px; }
  .kv { font-size: 50px; font-weight: 300; line-height: 1.1; margin-top: 10px; }
  .ki { position: absolute; width: 32px; height: 32px; }
  .ki svg { width: 32px; height: 32px; display: block; }
  .up { color: var(--green); } .down { color: var(--red); }
  .logo { position: absolute; display: flex; align-items: center; gap: 24px; font-size: 17px; letter-spacing: 3px; line-height: 24px; }
  html[lang="ar"] .logo { letter-spacing: 0; font-size: 19px; }
  .logo svg { width: 54px; height: 58px; flex: none; }
  .dark { background: var(--navy); color: #fff; }
  .dark .co { color: #fff; }
  /* 1 الغلاف */
  .s1 .logo { top: 80px; inset-inline-start: 40px; color: #fff; }
  .s1 .h1a { position: absolute; top: 268px; inset-inline-start: 38px; font-size: 61px; font-weight: 800; line-height: 76px; color: #fff; white-space: nowrap; }
  .s1 .h1b { position: absolute; top: 346px; inset-inline-start: 38px; font-size: 64px; font-weight: 300; line-height: 76px; color: #fff; white-space: nowrap; }
  .s1 .tag { position: absolute; top: 436px; inset-inline-start: 41px; font-size: 29px; letter-spacing: 3px; color: var(--green-t); font-weight: 400; white-space: nowrap; }
  html[lang="ar"] .s1 .tag { letter-spacing: 0; }
  .s1 .bar1 { position: absolute; top: 526px; inset-inline-start: 41px; width: 363px; height: 7px; border-radius: 4px; background: #fff; }
  .s1 .bar2 { position: absolute; top: 526px; inset-inline-start: 421px; width: 181px; height: 7px; border-radius: 4px; background: #2A5BE0; }
  .s1 .sub { top: 578px; inset-inline-start: 42px; color: #fff; }
  /* 2 جدول الأعمال */
  .s2 .ag { position: absolute; width: 197px; }
  .s2 .ag .n { font-size: 34px; font-weight: 300; color: var(--blue-2); line-height: 40px; }
  .s2 .ag h2 { font-size: 22px; font-weight: 700; line-height: 29px; margin-top: 24px; min-height: 58px; color: #000; }
  .s2 .ag.ln { border-bottom: 1px solid #333; padding-bottom: 32px; }
  .s2 .c1 { inset-inline-start: 35px; } .s2 .c2 { inset-inline-start: 264px; } .s2 .c3 { inset-inline-start: 504px; }
  .s2 .r1 { top: 316px; } .s2 .r2 { top: 508px; }
  .s2 .ag .n, .s2 .ag h2 { padding-inline-start: 2px; }
  /* 3 الملخص التنفيذي */
  .s3 .box { position: absolute; top: 193px; inset-inline-start: 0; width: 693px; height: 257px; background: var(--navy); border-start-end-radius: 44px; border-end-end-radius: 44px; padding: 36px 30px 0 38px; padding-inline-start: 38px; }
  .s3 .box h2 { font-size: 29px; font-weight: 700; line-height: 40px; color: var(--green); white-space: nowrap; }
  .s3 .box p { font-size: 22px; line-height: 29px; color: #fff; margin-top: 40px; white-space: nowrap; }
  .s3 .k { position: absolute; inset-inline-start: 855px; width: 353px; }
  .s3 .k .kt { font-size: 18px; line-height: 24px; margin-top: 12px; color: var(--ink); }
  .s3 .k1 { top: 125px; } .s3 .k2 { top: 302px; } .s3 .k3 { top: 482px; }
  .s3 .hr { position: absolute; inset-inline-start: 852px; width: 356px; height: 1px; background: var(--line); }
  .s3 .ki { inset-inline-start: 794px; }
  /* 4 النظرة المالية */
  .s4 .sub { top: 244px; }
  .s4 table { position: absolute; top: 118px; inset-inline-start: 510px; width: 735px; border-collapse: collapse; }
  .s4 th { background: var(--navy); color: #fff; font-size: 19px; font-weight: 700; height: 67px; text-align: start; padding-inline-start: 10px; }
  .s4 td { font-size: 20px; height: 66.5px; padding-inline-start: 10px; border-bottom: 1px solid var(--line); color: var(--ink); }
  .s4 tr:last-child td { border-bottom: 0; }
  .s4 .c1 { width: 215px; } .s4 .c2 { width: 178px; } .s4 .c3 { width: 162px; }
  .s4 .pos { color: var(--green-d); } .s4 .neg { color: #A01818; }
  .s4 .fc { position: absolute; top: 502px; width: 205px; }
  .s4 .fc h2 { font-size: 23px; font-weight: 700; line-height: 29px; color: var(--navy); padding-inline-start: 8px; min-height: 58px; }
  .s4 .fc p { font-size: 18px; line-height: 23px; color: var(--ink); padding-inline-start: 8px; border-top: 1px solid var(--line); margin-top: 11px; padding-top: 14px; }
  .s4 .f1 { inset-inline-start: 511px; } .s4 .f2 { inset-inline-start: 762px; } .s4 .f3 { inset-inline-start: 1013px; }
  /* 5 الالتزامات الضريبية */
  .s5 .row { position: absolute; inset-inline-start: 35px; width: 722px; height: 87px; display: flex; align-items: center; }
  .s5 .row + .row { border-top: 1px solid var(--line); }
  .s5 .row h2 { width: 159px; flex: none; font-size: 20px; font-weight: 700; line-height: 25px; padding-inline-start: 2px; color: #000; }
  .s5 .trk { position: relative; flex: 1; height: 61px; border-radius: 31px; background: #F1F1F2; }
  .s5 .fill { position: absolute; top: 0; inset-inline-start: 0; height: 100%; border-radius: 31px; display: flex; align-items: center; justify-content: flex-end; padding-inline-end: 20px; color: #fff; font-size: 22px; font-weight: 700; }
  .s5 .b1 .fill { background: var(--green); } .s5 .b2 .fill { background: var(--blue); } .s5 .b3 .fill { background: var(--cyan); } .s5 .b4 .fill { background: var(--navy); }
  .s5 .b1 { top: 273px; } .s5 .b2 { top: 360px; } .s5 .b3 { top: 452px; } .s5 .b4 { top: 541px; }
  .s5 .row.b1 { border-top: 0; }
  .s5 .k { position: absolute; inset-inline-start: 889px; width: 330px; }
  .s5 .k .kt { font-size: 18px; line-height: 24px; margin-top: 12px; }
  .s5 .k1 { top: 342px; } .s5 .k2 { top: 515px; }
  .s5 .ki { inset-inline-start: 840px; }
  /* 6 الاستراتيجية */
  .s6 .cd { position: absolute; top: 300px; width: 340px; height: 317px; }
  .s6 .cd::before { content: ""; position: absolute; top: 15px; inset-inline-start: 18px; width: 100%; height: 100%; border-radius: 40px; background: var(--c); }
  .s6 .cd .in { position: absolute; inset: 0; border: 3px solid var(--c); border-radius: 40px; background: #fff; padding: 94px 30px 0 39px; padding-inline-start: 39px; }
  .s6 .cd h2 { font-size: 22px; font-weight: 700; line-height: 29px; color: var(--hc); min-height: 58px; }
  .s6 .cd p { font-size: 18px; line-height: 23px; margin-top: 18px; color: var(--ink); }
  .s6 .d1 { inset-inline-start: 35px; --c: var(--navy); --hc: #000; } .s6 .d2 { inset-inline-start: 442px; --c: var(--blue); --hc: var(--blue); } .s6 .d3 { inset-inline-start: 849px; --c: var(--green); --hc: var(--green-t); }
  /* 7 الوفورات */
  .s7 .big { position: absolute; top: 228px; inset-inline-start: 36px; font-size: 76px; font-weight: 300; line-height: 1.1; color: var(--green-t); white-space: nowrap; }
  .s7 .kh { position: absolute; top: 344px; inset-inline-start: 38px; font-size: 20px; letter-spacing: 3.4px; }
  html[lang="ar"] .s7 .kh { letter-spacing: 0; font-size: 22px; }
  .s7 .bp { position: absolute; top: 409px; inset-inline-start: 37px; font-size: 20px; line-height: 25px; }
  .s7 .li { position: absolute; inset-inline-start: 815px; width: 430px; padding-inline-start: 84px; }
  .s7 .li .ck { position: absolute; top: 0; inset-inline-start: 14px; width: 37px; height: 37px; color: var(--green); }
  .s7 .li h2 { font-size: 22px; font-weight: 700; line-height: 26px; color: var(--navy); }
  .s7 .li p { font-size: 18px; line-height: 23px; margin-top: 20px; }
  .s7 .l1 { top: 228px; } .s7 .l2 { top: 366px; } .s7 .l3 { top: 504px; }
  .s7 .hr { position: absolute; inset-inline-start: 815px; width: 430px; height: 1px; background: var(--line); }
  /* 8 الامتثال */
  .s8 .sub { top: 244px; }
  .col { position: absolute; width: 280px; }
  .col h2 { font-size: 22px; font-weight: 700; line-height: 29px; color: var(--navy); padding-inline-start: 4px; min-height: 58px; }
  .col > p { font-size: 18px; line-height: 23px; padding-inline-start: 4px; margin-top: 21px; }
  .col ul { list-style: none; margin: 30px 0 0; padding: 0; border-top: 1px solid var(--line); }
  .col li { font-size: 15px; line-height: 20px; padding: 12px 0 12px; padding-inline-start: 9px; color: var(--ink); }
  .col li + li { border-top: 1px solid var(--line); }
  .s8 .col { top: 381px; } .s8 .x1 { inset-inline-start: 35px; } .s8 .x2 { inset-inline-start: 375px; } .s8 .x3 { inset-inline-start: 719px; }
  /* 9 المخاطر */
  .fs { position: absolute; border: 2px solid var(--navy); border-radius: 22px; background: #fff; }
  .fs .lg { position: absolute; top: -17px; background: #fff; padding-inline: 22px 36px; font-size: 22px; font-weight: 700; line-height: 30px; color: var(--navy); white-space: nowrap; }
  .s9 .fs { top: 280px; inset-inline-start: 35px; width: 810px; height: 375px; }
  .s9 .fs .lg { inset-inline-start: 40px; }
  .s9 .rk { position: absolute; width: 281px; padding-inline-start: 3px; }
  .s9 .rk .mi { position: absolute; top: 2px; inset-inline-start: -41px; width: 24px; height: 24px; color: var(--navy); }
  .s9 .rk h2 { font-size: 20px; font-weight: 700; line-height: 28px; color: var(--blue); }
  .s9 .rk p { font-size: 18px; line-height: 23px; margin-top: 18px; }
  .s9 .rk.ln { border-bottom: 1px solid #9A9A9A; padding-bottom: 30px; }
  .s9 .q1 { top: 64px; inset-inline-start: 96px; } .s9 .q2 { top: 64px; inset-inline-start: 475px; } .s9 .q3 { top: 221px; inset-inline-start: 96px; } .s9 .q4 { top: 221px; inset-inline-start: 475px; }
  /* 10 الفرص */
  .s10 .tri { position: absolute; top: 332px; width: 372px; height: 285px; }
  .s10 .tri svg.t { position: absolute; inset: 0; width: 372px; height: 285px; }
  .s10 .tri .cc { position: absolute; top: 24px; inset-inline-start: 160px; width: 52px; height: 52px; border-radius: 50%; background: #fff; display: grid; place-items: center; color: var(--green); }
  .s10 .tri .cc svg { width: 36px; height: 36px; }
  .s10 .tri .tx { position: absolute; top: 127px; inset-inline-start: 86px; width: 200px; text-align: center; }
  .s10 .tri h2 { font-size: 22px; font-weight: 700; line-height: 29px; color: var(--navy); min-height: 58px; }
  .s10 .tri p { font-size: 18px; line-height: 23px; margin-top: 21px; }
  .s10 .p1 { inset-inline-start: 6px; } .s10 .p2 { inset-inline-start: 298px; } .s10 .p3 { inset-inline-start: 591px; } .s10 .p4 { inset-inline-start: 883px; }
  /* 11 التعديلات */
  .s11 .sh { position: absolute; top: 300px; inset-inline-start: 58px; width: 964px; height: 373px; border-radius: 22px; background: var(--navy); }
  .s11 .fs { top: 280px; inset-inline-start: 35px; width: 967px; height: 375px; }
  .s11 .fs .lg { inset-inline-start: 15px; }
  .s11 .col { top: 45px; }
  .s11 .ck { width: 33px; height: 33px; color: var(--green); margin-bottom: 25px; margin-inline-start: 31px; }
  .s11 .ck svg { width: 33px; height: 33px; display: block; }
  .s11 .col h2, .s11 .col > p { padding-inline-start: 30px; }
  .s11 .col > p { margin-top: 21px; }
  .s11 .col ul { margin-top: 22px; margin-inline-start: 24px; width: 280px; }
  .s11 .col li { padding-inline-start: 10px; }
  .s11 .x1 { inset-inline-start: 0; } .s11 .x2 { inset-inline-start: 318px; } .s11 .x3 { inset-inline-start: 637px; }
  .s11 .col { width: 320px; }
  /* 12 تخطيط العام المقبل */
  .s12 .qt { position: absolute; width: 312px; }
  .s12 .qt svg { position: absolute; top: 0; inset-inline-start: 0; width: 312px; height: 216px; }
  .s12 .qt .q { position: absolute; top: 25px; inset-inline-start: 0; width: 312px; text-align: center; font-size: 34px; font-weight: 300; color: #fff; line-height: 40px; }
  html[lang="ar"] .s12 .qt .q { font-size: 26px; }
  .s12 .qt .tx { position: absolute; inset-inline-start: 14px; width: 280px; text-align: center; }
  .s12 .qt h2 { font-size: 22px; font-weight: 700; line-height: 29px; color: var(--hc); min-height: 58px; }
  .s12 .qt > .tx > p { font-size: 18px; line-height: 23px; margin-top: 24px; color: var(--ink); }
  .s12 .qt ul { list-style: none; margin: 22px 0 0; padding: 0; border-top: 1px solid var(--line); }
  .s12 .qt li { font-size: 15px; line-height: 20px; padding: 12px 0 12px; }
  .s12 .qt li + li { border-top: 1px solid var(--line); }
  .s12 .t1 { top: 330px; inset-inline-start: 19px; --hc: var(--green-d); } .s12 .t1 .tx { top: 100px; }
  .s12 .t2 { top: 270px; inset-inline-start: 320px; --hc: var(--cyan-t); } .s12 .t2 .tx { top: 115px; }
  .s12 .t3 { top: 166px; inset-inline-start: 618px; --hc: var(--blue); } .s12 .t3 .tx { top: 170px; }
  .s12 .t4 { top: 75px; inset-inline-start: 930px; --hc: var(--navy); } .s12 .t4 .tx { top: 216px; }
  /* 13 الخلاصات */
  .s13 .tk { position: absolute; inset-inline-start: 673px; width: 387px; padding-inline-start: 86px; }
  .s13 .tk .n { position: absolute; top: 0; inset-inline-start: 10px; font-size: 34px; font-weight: 300; line-height: 40px; color: var(--blue-2); }
  .s13 .tk h2 { font-size: 22px; font-weight: 700; line-height: 29px; color: #000; min-height: 58px; }
  .s13 .tk p { font-size: 18px; line-height: 23px; margin-top: 23px; }
  .s13 .tk p.n { margin: 0; font-size: 34px; line-height: 40px; }
  .s13 .n1 { top: 110px; } .s13 .n2 { top: 305px; } .s13 .n3 { top: 500px; }
  .s13 .hr { position: absolute; inset-inline-start: 673px; width: 387px; height: 1px; background: var(--line); }
  /* 14 الأسئلة */
  .s14 .qa { position: absolute; top: 225px; inset-inline-start: 34px; font-size: 156px; font-weight: 800; line-height: 1.1; color: var(--navy); letter-spacing: -2px; white-space: nowrap; }
  html[lang="ar"] .s14 .qa { font-size: 120px; top: 230px; letter-spacing: 0; }
  .s14 .qq { position: absolute; top: 444px; inset-inline-start: 37px; font-size: 33px; font-weight: 700; color: var(--green-t); white-space: nowrap; }
  .s14 .sub { top: 514px; }
  .s14 .logo { top: 362px; inset-inline-start: 925px; color: var(--navy); }
  .s14 .logo span { color: var(--ink); }
  /* 15 الختام */
  .s15 .ty { position: absolute; top: 108px; inset-inline-start: 35px; font-size: 62px; font-weight: 700; line-height: 74px; color: #fff; white-space: nowrap; }
  .s15 .cu { position: absolute; top: 180px; inset-inline-start: 35px; font-size: 62px; font-weight: 400; line-height: 74px; color: var(--green-t); white-space: nowrap; }
  .s15 .sub { top: 287px; color: #fff; }
  .s15 .ct { position: absolute; top: 426px; width: 236px; border-bottom: 1px solid #fff; padding-bottom: 26px; padding-inline-start: 3px; }
  .s15 .ct .ic { width: 36px; height: 36px; color: var(--green); }
  .s15 .ct .ic svg { width: 36px; height: 36px; display: block; }
  .s15 .ct h2 { font-size: 20px; font-weight: 700; line-height: 26px; color: #fff; margin-top: 20px; }
  .s15 .ct p { font-size: 19px; line-height: 26px; color: #fff; margin-top: 8px; white-space: nowrap; }
  .s15 .e1 { inset-inline-start: 43px; } .s15 .e2 { inset-inline-start: 355px; } .s15 .e3 { inset-inline-start: 683px; }
  .s15 .logo { top: 308px; inset-inline-start: 860px; color: #fff; }"""


def head(t):
    return '<p class="co">§CO§</p><h1 class="ttl">§%s§</h1>' % t


def kpi(cls, top, icon, color, h, v, p):
    return ('<div class="k %s"><p class="kh">§%s§</p><p class="kv" style="color:%s"><bdi dir="ltr">§%s§</bdi></p><p class="kt">§%s§</p></div>'
            % (cls, h, color, v, p) + '<span class="ki %s" style="top:%dpx">%s</span>' % (icon, top, ICON[icon]))


# ---------- الشرائح ----------
S1 = (dec(
    poly([(520, 0), (1280, 0), (1280, 720), (520, 720)], 520, 0, 720, 0, ((0, '#1E4A62', 0), (1, '#1E4A62', .55))),
    poly([(610, 0), (800, 0), (1150, 330), (1118, 428)], 640, 0, 1130, 420, ((0, '#2C7C80', 0), (0.5, '#2C7C80', .45), (1, '#33A08A', .8))),
    beam(767, 207, 1090, -20, 62, 62, ((0, '#2448C8', .95), (1, '#1A2F86', .3))),
    beam(840, 467, 1150, 760, 63, 110, ((0, '#2B7C7E', .9), (1, '#1C3A7A', 0))),
    beam(767, 207, 1138, 388, 62, stops=((0, '#2A57F0', 1), (0.45, '#2A5FC8', .9), (1, '#3DB48A', 1)), cap=True),
    beam(840, 467, 1320, 190, 63, 63, ((0, '#79D4FF', 1), (0.45, '#4C9AB4', .8), (1, '#2346A8', .35)), op=.9),
)
    + '<div class="logo">' + ICON['logo'] + '<span>§LOGO§</span></div>'
    + '<h1 class="h1a">§C1§</h1><h1 class="h1b">§C2§</h1><p class="tag">§C3§</p>'
    + '<span class="bar1"></span><span class="bar2"></span><p class="sub">§SUB§</p>')

S2 = (dec(motif(865, 463, 1.12)) + head('T2') + '<p class="sub">§SUB§</p>'
      + ''.join('<div class="ag %s %s%s"><p class="n">0%d</p><h2>§HP§</h2></div>' % (c, r, ' ln' if r == 'r1' else '', i + 1)
                for i, (r, c) in enumerate([('r1', 'c1'), ('r1', 'c2'), ('r1', 'c3'), ('r2', 'c1'), ('r2', 'c2'), ('r2', 'c3')])))

S3 = (dec(
    poly([(0, 130), (690, 130), (690, 195), (0, 195)], 0, 130, 0, 195, ((0, '#FFFFFF', 0), (1, '#E6EBFB', .9))),
    beam(260, 430, 240, 760, 60, 290, ((0, '#A8B8F6', .95), (1, '#F1F4FD', .7))),
    beam(437, 575, 680, 70, 8, 150, ((0, '#55D894', 1), (0.6, '#CBF1DD', .8), (1, '#FFFFFF', 0))),
    beam(383, 540, 238, 300, 62, stops=((0, '#64C5B2', 1), (1, '#7D9EEA', .85)), cap=True),
    beam(178, 458, 100, 250, 60, stops=((0, '#4E77F0', 1), (1, '#2A56EC', 1)), cap=True),
)
    + head('T3') + '<div class="box"><h2>§XH§</h2><p>§SUB§</p></div>'
    + kpi('k1', 171, 'up', 'var(--green-t)', 'KH', 'V1', 'KT')
    + kpi('k2', 351, 'down', 'var(--navy)', 'KH', 'V2', 'KT')
    + kpi('k3', 529, 'up', 'var(--blue-2)', 'KH', 'V3', 'KT')
    + '<span class="hr" style="top:277px"></span><span class="hr" style="top:454px"></span>')

S4 = (dec(
    beam(188, 465, -30, 230, 60, 200, LAV),
    beam(142, 518, 60, 760, 60, 240, ((0, '#B4C3F7', .9), (1, '#EEF1FD', .6))),
    beam(345, 615, 560, 230, 8, 150, GREEN),
    beam(305, 585, 188, 465, 60, stops=((0, '#64C5B2', 1), (1, '#7E9EEA', .85)), cap=True, op=.92),
    beam(142, 518, 188, 465, 60, stops=((0, '#6A8CF2', .95), (1, '#2D59EC', 1)), cap=True),
)
    + head('T4') + '<p class="sub">§SUB4§</p>'
    + '<table><tr><th class="c1">§TH§</th><th class="c2">§TH§</th><th class="c3">§TH§</th><th>§TH§</th></tr>'
    + ''.join('<tr><td>§TI§</td><td><bdi dir="ltr">%s</bdi></td><td class="%s"><bdi dir="ltr">%s</bdi></td><td>§TS§</td></tr>' % (a, 'neg' if b.startswith('-') else 'pos', b)
              for a, b in [('$ 124,500,000', '+12%'), ('$ 42,800,000', '+5%'), ('$ 18,250,000', '-3%'), ('$ 22,100,000', '+15%')])
    + '</table>'
    + ''.join('<div class="fc f%d"><h2>§HP2§</h2><p>§T3L§</p></div>' % i for i in (1, 2, 3)))

S5 = (dec(motif(900, 255, 1.0)) + head('T5') + '<p class="sub">§SUB§</p>'
      + ''.join('<div class="row b%d"><h2>§HP2§</h2><div class="trk"><div class="fill"><span dir="ltr">§BV%d§</span></div></div></div>' % (i, i) for i in (1, 2, 3, 4))
      + kpi('k1', 388, 'up', 'var(--green-t)', 'KH', 'V1', 'KT')
      + kpi('k2', 566, 'down', 'var(--navy)', 'KH', 'V2', 'KT'))

S6 = (dec(motif(900, 357, 1.0)) + head('T6') + '<p class="sub">§SUB§</p>'
      + ''.join('<div class="cd d%d"><div class="in"><h2>§HP2§</h2><p>§T2L§</p></div></div>' % i for i in (1, 2, 3)))

S7 = (dec(
    beam(560, 280, 560, -100, 60, 300, ((0, '#B7C5F7', 0), (0.15, '#B7C5F7', .0), (1, '#FFFFFF', 0))),
    poly([(290, 70), (830, 70), (830, 210), (680, 470), (430, 470)], 0, 70, 0, 470, ((0, '#FFFFFF', 0), (0.45, '#E3E8FC', .9), (1, '#7F98F2', 1))),
    poly([(275, 720), (430, 470), (680, 470), (820, 720)], 0, 470, 0, 720, ((0, '#7F98F2', 1), (0.6, '#C9D7F4', .9), (1, '#EEF2FC', .9))),
    beam(508, 300, 230, 760, 58, 120, ((0, '#7ADCA6', .9), (0.6, '#D2F4E1', .8), (1, '#FFFFFF', 0))),
    beam(508, 300, 600, 470, 60, stops=((0, '#68C4BE', 1), (1, '#8CB6E6', .9)), cap=True, op=.95),
    beam(550, 380, 593, 480, 60, stops=((0, '#2A56EC', 1), (1, '#6787F1', 1)), cap=True, op=.95),
)
    + head('T7') + '<p class="big"><bdi dir="ltr">§BIG§</bdi></p><p class="kh">§KH§</p><p class="bp">§T2L§</p>'
    + ''.join('<div class="li l%d"><span class="ck">%s</span><h2>§HP§</h2><p>§T2L§</p></div>' % (i, ICON['check']) for i in (1, 2, 3))
    + '<span class="hr" style="top:345px"></span><span class="hr" style="top:481px"></span>')

S8 = (dec(motif(900, 337, 1.0)) + head('T8') + '<p class="sub">§SUB§</p>'
      + ''.join('<div class="col x%d"><h2>§HP2§</h2><p>§T2L§</p><ul><li>§SM1§</li><li>§SM2§</li></ul></div>' % i for i in (1, 2, 3)))

S9 = (dec(motif(900, 424, 1.0)) + head('T9') + '<p class="sub">§SUB§</p>'
      + '<div class="fs"><p class="lg">§HP§</p>'
      + ''.join('<div class="rk q%d%s"><span class="mi">%s</span><h2>§HP§</h2><p>§T2L§</p></div>' % (i, ' ln' if i < 3 else '', ICON['minus']) for i in (1, 2, 3, 4))
      + '</div>')


def tri10(i):
    gid = 'gt10'
    return ('<div class="tri p%d"><svg class="t" viewBox="0 0 372 285" aria-hidden="true"><path d="M146 22 Q186 -12 226 22 L372 280 L0 280 Z" fill="url(#%s)"/></svg>'
            '<span class="cc">%s</span><div class="tx"><h2>§HP2§</h2><p>§T3L§</p></div></div>' % (i, gid, ICON['check']))


S10 = (dec(
    beam(1010, 248, 840, -120, 62, 140, ((0, '#7AD4FF', 1), (0.7, '#DDF4FF', .8), (1, '#FFFFFF', 0))),
    beam(1155, 170, 1380, -150, 62, 180, ((0, '#2653E8', 1), (0.7, '#B9C6F8', .8), (1, '#E6EBFD', .6))),
)
    + head('T10') + '<p class="sub">§SUB§</p>' + ''.join(tri10(i) for i in (1, 2, 3, 4)))

S11 = (dec(
    poly([(780, 0), (1280, 0), (1280, 230), (1150, 330), (940, 260)], 0, 0, 0, 330, ((0, '#FFFFFF', 0), (1, '#AFC0F7', .9))),
    beam(1090, 420, 1120, 760, 60, 220, ((0, '#7C9CF0', .9), (0.6, '#DCE4FB', .8), (1, '#F4F6FE', .6))),
    beam(900, 205, 760, 600, 6, 150, ((0, '#6EDB9E', 1), (1, '#FFFFFF', 0))),
    beam(945, 240, 1090, 420, 62, stops=((0, '#64C5B2', 1), (1, '#7FA4E8', .9)), cap=True),
    beam(1150, 325, 1090, 420, 62, stops=((0, '#6584F2', 1), (1, '#2A56EC', 1)), cap=True, op=.95),
)
    + head('T11') + '<p class="sub">§SUB§</p><span class="sh"></span>'
    + '<div class="fs"><p class="lg">§HP§</p>'
    + ''.join('<div class="col x%d"><div class="ck">%s</div><h2>§HP2§</h2><p>§T2L§</p><ul><li>§SM1§</li><li>§SM2§</li></ul></div>' % (i, ICON['check']) for i in (1, 2, 3))
    + '</div>')


def qtri(i):
    return ('<div class="qt t%d"><svg viewBox="0 0 312 216" aria-hidden="true"><path d="M110 28 Q156 -24 202 28 L312 216 L0 216 Z" fill="url(#gq%d)"/></svg>'
            '<p class="q">§Q%d§</p><div class="tx"><h2>§HP2§</h2><p>§T2L§</p><ul><li>§SM1§</li><li>§SM2§</li></ul></div></div>' % (i, i, i))


S12 = head('T12') + '<p class="sub">§SUB§</p>' + ''.join(qtri(i) for i in (1, 2, 3, 4))

S13 = (dec(
    beam(178, 465, 120, -60, 60, 330, ((0, '#7F9AF2', 1), (0.6, '#D6DEFB', .8), (1, '#FFFFFF', 0))),
    beam(178, 465, -120, 760, 60, 200, ((0, '#93A9F4', .95), (1, '#EEF1FD', .6))),
    beam(383, 382, 520, 760, 60, 150, ((0, '#55D894', 1), (0.6, '#C8F0DB', .8), (1, '#F0FBF5', .6))),
    beam(237, 560, 250, 760, 60, 250, ((0, '#C3D3F2', .7), (1, '#E6EEF6', .6))),
    beam(237, 560, 178, 465, 60, stops=((0, '#2653E8', 1), (1, '#6585F1', 1)), cap=True),
    beam(237, 560, 383, 382, 60, stops=((0, '#5A86EE', .85), (1, '#64C5B2', .95)), cap=True, op=.92),
    beam(1250, 413, 1050, 760, 60, 150, ((0, '#4FD690', 1), (0.6, '#C9F1DC', .8), (1, '#FFFFFF', 0))),
    beam(1140, 527, 1130, 760, 60, 190, ((0, '#7AD4FF', .9), (1, '#EAF7FD', .5))),
)
    + head('T13') + '<p class="sub">§SUB§</p>'
    + ''.join('<div class="tk n%d"><p class="n">0%d</p><h2>§HP2§</h2><p>§T2L§</p></div>' % (i, i) for i in (1, 2, 3))
    + '<span class="hr" style="top:277px"></span><span class="hr" style="top:467px"></span>')

S14 = (dec(
    beam(830, 395, 60, 395, 62, 520, ((0, '#78D4FF', 1), (0.6, '#C9EEFE', .9), (1, '#FFFFFF', .9))),
    beam(1215, 0, 470, -10, 62, 380, ((0, '#2A56F0', 1), (0.55, '#B6C3F7', .9), (1, '#FFFFFF', 0))),
    beam(1010, 648, 1360, 445, 65, 65, ((0, '#45D48A', 1), (1, '#C8F2DC', 1))),
)
    + '<p class="qa">§QA§</p><p class="qq">§QQ§</p><p class="sub">§SUB§</p>'
    + '<div class="logo">' + ICON['logo'] + '<span>§LOGO§</span></div>')

S15 = (dec(
    poly([(860, 0), (1280, 0), (1280, 360)], 860, 0, 1280, 0, ((0, '#13236A', 0), (1, '#1E3FA8', .9))),
    poly([(840, 720), (1280, 420), (1280, 720)], 840, 0, 1280, 0, ((0, '#1A2F86', .6), (1, '#22409F', 1))),
    beam(815, 330, 1400, 330, 68, 400, ((0, '#45D48A', 1), (0.45, '#2A6A8E', .9), (1, '#1C3A86', .3))),
    beam(62, 655, -120, 550, 60, 60, ((0, '#45D48A', 1), (1, '#2B6A8C', .9))),
    poly([(0, 690), (60, 720), (0, 720)], 0, 690, 60, 720, ((0, '#1E3FA8', 1), (1, '#1E3FA8', 1))),
)
    + head('T15').replace('<h1 class="ttl">§T15§</h1>', '<h1 class="ty">§TY§</h1><p class="cu">§CU§</p>') + '<p class="sub">§SUB§</p>'
    + '<div class="ct e1"><div class="ic">%s</div><h2>§EM§</h2><p>§EMT§</p></div>' % ICON['mail']
    + '<div class="ct e2"><div class="ic">%s</div><h2>§CN§</h2><p>§CNT§</p></div>' % ICON['phone']
    + '<div class="ct e3"><div class="ic">%s</div><h2>§WB§</h2><p>§WBT§</p></div>' % ICON['globe']
    + '<div class="logo">' + ICON['logo'] + '<span>§LOGO§</span></div>')

SLIDES = [S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, S14, S15]
SLIDES[0] = SLIDES[0]
# الشرائح الداكنة
SLIDES = [s for s in SLIDES]

_q = ''.join('<linearGradient id="gq%d" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="%s"/><stop offset=".55" stop-color="%s" stop-opacity=".55"/><stop offset="1" stop-color="#FFFFFF" stop-opacity="0"/></linearGradient>' % (i, c, c)
             for i, c in [(1, '#45D48A'), (2, '#1FB5F6'), (3, '#2653E8'), (4, '#13236A')])
GLOBAL_DEFS = ('<defs>' + ''.join(_DEFS) + _q
               + '<linearGradient id="gt10" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#45D48A"/><stop offset=".5" stop-color="#45D48A" stop-opacity=".45"/><stop offset="1" stop-color="#FFFFFF" stop-opacity="0"/></linearGradient>'
               + '</defs>')

STYLE += """
  .s1, .s15 { background: var(--navy); color: #fff; }
  .s1 .co, .s15 .co { color: #fff; }"""

JSON_BLOCKS = {'chart-data': {'max': 5.7, 'values': [4.2, 2.1, 1.8, 0.9], 'labels': ['$4.2M', '$2.1M', '$1.8M', '$0.9M']}}

EXTRA_SCRIPT = """  // أشرطة الالتزامات الضريبية (الشريحة 5) من كتلة chart-data — العرض نسبةً إلى max والقيمة داخل الشريط
  try {
    var cd = JSON.parse(document.getElementById('chart-data').textContent);
    document.querySelectorAll('.s5 .fill').forEach(function (f, i) {
      var v = cd.values[i]; if (v == null) return;
      f.style.width = Math.min(100, 20 + 80 * v / cd.max) + '%';  // 20% حدّ أدنى ليتسع الشريط لقيمته
      if (cd.labels && cd.labels[i] != null) f.querySelector('span').textContent = cd.labels[i];
    });
  } catch (e) {}"""

SUB_EN = 'Insert your desired text here. This is a sample text.<br>A brief statement about your topic.'
SUB_AR = 'أدخل النص المطلوب هنا. هذا نص توضيحي.<br>عبارة موجزة عن موضوعك.'
STR = {
    'en': {
        'CO': 'YOUR COMPANY NAME HERE', 'LOGO': 'YOUR COMPANY<br>LOGO HERE', 'SUB': SUB_EN,
        'C1': 'ANNUAL TAX', 'C2': 'PLANNING REVIEW', 'C3': 'PRESENTATION TEMPLATE',
        'T2': '<b>AGENDA</b> SLIDE', 'T3': '<b>EXECUTIVE SUMMARY</b> SLIDE', 'T4': '<b>FINANCIAL<br>OVERVIEW</b> SLIDE',
        'T5': '<b>TAX LIABILITY OVERVIEW</b> SLIDE', 'T6': '<b>TAX STRATEGY REVIEW</b> SLIDE', 'T7': '<b>TAX SAVINGS ACHIEVED</b> SLIDE',
        'T8': '<b>COMPLIANCE AND<br>REGULATORY UPDATES</b> SLIDE', 'T9': '<b>TAX RISK ANALYSIS</b> SLIDE',
        'T10': '<b>OPPORTUNITIES FOR NEXT YEAR</b> SLIDE', 'T11': '<b>RECOMMENDED ADJUSTMENTS</b> SLIDE',
        'T12': '<b>NEXT YEAR PLANNING</b> SLIDE', 'T13': '<b>KEY TAKEAWAYS</b> SLIDE', 'T15': '',
        'HP': 'Heading Placeholder', 'HP2': 'Heading<br>Placeholder', 'KH': 'HEADING PLACEHOLDER',
        'XH': 'Insert your heading statement here.<br>This is a sample text.',
        'V1': '$ 9.3M', 'V2': '21%', 'V3': '900K', 'KT': 'Insert your desired text here.',
        'SUB4': 'Insert your desired text here. This is<br>a sample text. A brief statement<br>about your topic.',
        'TH': 'Heading Here', 'TI': 'Insert text here', 'TS': 'Sample text',
        'T3L': 'Insert your desired<br>text here. This is a<br>sample text.', 'T2L': 'Insert your desired text here.<br>This is a sample text.',
        'BV1': '$4.2M', 'BV2': '$2.1M', 'BV3': '$1.8M', 'BV4': '$0.9M',
        'BIG': '$ 2.54 M', 'SM1': 'This is a sample text.', 'SM2': 'Insert your desired text here.',
        'Q1': 'Q1', 'Q2': 'Q2', 'Q3': 'Q3', 'Q4': 'Q4',
        'QA': 'Q&amp;A', 'QQ': 'DO YOU HAVE ANY QUESTIONS?',
        'TY': 'Thank You', 'CU': 'Contact Us',
        'EM': 'EMAIL', 'EMT': 'Enter email address here', 'CN': 'CONTACT', 'CNT': 'Enter contact info here',
        'WB': 'WEBSITE', 'WBT': 'Enter website address',
    },
    'ar': {
        'CO': 'اسم شركتك هنا', 'LOGO': 'شعار شركتك<br>هنا', 'SUB': SUB_AR,
        'C1': 'مراجعة التخطيط', 'C2': 'الضريبي السنوي', 'C3': 'قالب عرض تقديمي',
        'T2': 'شريحة <b>جدول الأعمال</b>', 'T3': 'شريحة <b>الملخص التنفيذي</b>', 'T4': 'شريحة <b>النظرة</b><br><b>المالية العامة</b>',
        'T5': 'شريحة <b>الالتزامات الضريبية</b>', 'T6': 'شريحة <b>مراجعة الاستراتيجية الضريبية</b>', 'T7': 'شريحة <b>الوفورات الضريبية المحققة</b>',
        'T8': 'شريحة <b>الامتثال</b><br><b>والمستجدات التنظيمية</b>', 'T9': 'شريحة <b>تحليل المخاطر الضريبية</b>',
        'T10': 'شريحة <b>فرص العام المقبل</b>', 'T11': 'شريحة <b>التعديلات الموصى بها</b>',
        'T12': 'شريحة <b>تخطيط العام المقبل</b>', 'T13': 'شريحة <b>أبرز الخلاصات</b>', 'T15': '',
        'HP': 'عنوان نائب', 'HP2': 'عنوان نائب', 'KH': 'عنوان نائب',
        'XH': 'أدخل عبارة العنوان هنا.<br>هذا نص توضيحي.',
        'V1': '$ 9.3M', 'V2': '21%', 'V3': '900K', 'KT': 'أدخل النص المطلوب هنا.',
        'SUB4': 'أدخل النص المطلوب هنا. هذا نص<br>توضيحي. عبارة موجزة عن<br>موضوعك.',
        'TH': 'العنوان هنا', 'TI': 'أدخل النص هنا', 'TS': 'نص توضيحي',
        'T3L': 'أدخل النص المطلوب<br>هنا. هذا نص<br>توضيحي.', 'T2L': 'أدخل النص المطلوب هنا.<br>هذا نص توضيحي.',
        'BV1': '$4.2M', 'BV2': '$2.1M', 'BV3': '$1.8M', 'BV4': '$0.9M',
        'BIG': '$ 2.54 M', 'SM1': 'هذا نص توضيحي.', 'SM2': 'أدخل النص المطلوب هنا.',
        'Q1': 'الربع 1', 'Q2': 'الربع 2', 'Q3': 'الربع 3', 'Q4': 'الربع 4',
        'QA': 'الأسئلة', 'QQ': 'هل لديكم أي أسئلة؟',
        'TY': 'شكرًا لكم', 'CU': 'تواصلوا معنا',
        'EM': 'البريد الإلكتروني', 'EMT': 'أدخل عنوان البريد الإلكتروني هنا', 'CN': 'الاتصال', 'CNT': 'أدخل بيانات الاتصال هنا',
        'WB': 'الموقع الإلكتروني', 'WBT': 'أدخل عنوان الموقع',
    },
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'finance', 'icon': '🧾',
    'name': 'مراجعة التخطيط الضريبي السنوي', 'nameEn': 'Annual Tax Planning Review',
    'description': '15 شريحة: غلاف، جدول أعمال، ملخص، نظرة مالية، الالتزامات، الاستراتيجية، الوفورات، الامتثال، المخاطر، الخطة',
    'descriptionEn': '15 slides: cover, agenda, summary, financials, liability, strategy, savings, compliance, risk, planning',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'جدول الأعمال', 'titleEn': 'Agenda'},
        {'title': 'الملخص التنفيذي', 'titleEn': 'Executive Summary', 'needsData': True},
        {'title': 'النظرة المالية العامة (جدول)', 'titleEn': 'Financial Overview (table)', 'needsData': True},
        {'title': 'الالتزامات الضريبية (أشرطة)', 'titleEn': 'Tax Liability Overview (bars)', 'needsData': True},
        {'title': 'مراجعة الاستراتيجية الضريبية', 'titleEn': 'Tax Strategy Review'},
        {'title': 'الوفورات الضريبية المحققة', 'titleEn': 'Tax Savings Achieved', 'needsData': True},
        {'title': 'الامتثال والمستجدات التنظيمية', 'titleEn': 'Compliance and Regulatory Updates'},
        {'title': 'تحليل المخاطر الضريبية', 'titleEn': 'Tax Risk Analysis'},
        {'title': 'فرص العام المقبل', 'titleEn': 'Opportunities for Next Year'},
        {'title': 'التعديلات الموصى بها', 'titleEn': 'Recommended Adjustments'},
        {'title': 'تخطيط العام المقبل (الأرباع)', 'titleEn': 'Next Year Planning (quarters)'},
        {'title': 'أبرز الخلاصات', 'titleEn': 'Key Takeaways'},
        {'title': 'الأسئلة', 'titleEn': 'Q&A'},
        {'title': 'شكرًا وتواصل معنا', 'titleEn': 'Thank You & Contact'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والزخارف وغيّر النصوص والأرقام فقط؛ أطوال أشرطة الالتزامات الضريبية وقيمها في كتلة chart-data (values وlabels وmax).',
}
