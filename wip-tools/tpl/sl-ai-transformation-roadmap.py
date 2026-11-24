# -*- coding: utf-8 -*-
# نموذج «خارطة طريق التحول بالذكاء الاصطناعي»: 17 شريحة (غلاف داكن بتدرّج أزرق-بنفسجي، جدول محتويات بحلقات،
# ملخص تنفيذي ببطاقات، مسوّغات التغيير برسمين، سلّم نضج بسبعة مستويات، رؤية بدوائر، ركائز بأربعة أعمدة، مصفوفة
# أولويات، مخطط جانت، خط زمني ربعي، طبقات بيانات، فين ثلاثي، ترس الحوكمة، إطار أخلاقي، مؤشرات أداء برسم خطي،
# درج الخطوات التالية، شكر وتواصل). لا صور فوتوغرافية: كل الأشكال CSS/SVG.
import math

KEY = 'sl-ai-transformation-roadmap'
TITLE = {'ar': 'خارطة طريق التحول بالذكاء الاصطناعي', 'en': 'AI Transformation Roadmap'}
COMMENT = 'نموذج «خارطة طريق التحول بالذكاء الاصطناعي»: 17 شريحة بخلفية بيضاء وعناوين بنفسجية وزخرفة خطوط متموّجة في الزوايا، بطاقات وأشكال بتدرّجات بنفسجية وزرقاء، ورسوم بيانية تُرسم من كتل chart-data.'
VARS = '--ink: #1E1E1E; --ink-2: #3A3A3A; --purple: #6D1677; --violet: #8F1FB0; --magenta: #B02BC0; --navy: #01028E; --blue: #0100D7; --sky: #0090EB; --lav: #C9C9E2; --lav-2: #C5C5F5; --gold: #FFC107; --ok: #5CB85C; --danger: #E53935; --accent: #6D1677; --bg: #FFFFFF;'

# ---------------------------------------------------------------- مساعدات هندسية
def pt(cx, cy, r, a, ry=None):
    ry = r if ry is None else ry
    return (cx + r * math.cos(math.radians(a)), cy + ry * math.sin(math.radians(a)))


def arc(cx, cy, r, a1, a2, ry=None):
    """قوس بالاتجاه الموجب (مع عقارب الساعة في إحداثيات SVG) من الزاوية a1 إلى a2."""
    ry = r if ry is None else ry
    x1, y1 = pt(cx, cy, r, a1, ry); x2, y2 = pt(cx, cy, r, a2, ry)
    large = 1 if (a2 - a1) % 360 > 180 else 0
    return 'M%.1f,%.1f A%d,%d 0 %d 1 %.1f,%.1f' % (x1, y1, r, ry, large, x2, y2)


def ring(cx, cy, R, r, fill):
    d = ('M%d,%d a%d,%d 0 1 0 %d,0 a%d,%d 0 1 0 -%d,0 ' % (cx - R, cy, R, R, 2 * R, R, R, 2 * R)
         + 'M%d,%d a%d,%d 0 1 0 %d,0 a%d,%d 0 1 0 -%d,0' % (cx - r, cy, r, r, 2 * r, r, r, 2 * r))
    return '<path fill-rule="evenodd" fill="%s" d="%s"/>' % (fill, d)


def dots(cx, cy, r, a1, a2, step, rad, fill):
    out = []
    a = a1
    while a <= a2:
        x, y = pt(cx, cy, r, a)
        out.append('<circle cx="%.1f" cy="%.1f" r="%s" fill="%s"/>' % (x, y, rad, fill))
        a += step
    return ''.join(out)


def star(cx, cy, r):
    pts = []
    for i in range(10):
        rr = r if i % 2 == 0 else r * 0.45
        x, y = pt(cx, cy, rr, -90 + i * 36)
        pts.append('%.1f,%.1f' % (x, y))
    return '<polygon points="%s" fill="#fff"/>' % ' '.join(pts)


def gold_star(cx, cy):
    return '<circle cx="%d" cy="%d" r="17" fill="var(--gold)"/>' % (cx, cy) + star(cx, cy, 9)


def gear(cx, cy, R, root, n=10):
    pts = []
    for i in range(n):
        c = -90 + i * 360.0 / n
        for rr, da in ((root, -18), (root, -10), (R, -6.5), (R, 6.5), (root, 10), (root, 18)):
            x, y = pt(cx, cy, rr, c + da)
            pts.append('%.1f,%.1f' % (x, y))
    return '<polygon points="%s" fill="url(#gg)"/>' % ' '.join(pts)


# ---------------------------------------------------------------- أيقونات SVG مضمّنة
ICONS = {
    'arrow': '<svg class="ar-ic" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="12" cy="12" r="9.5"/><path d="M8 12h8M12.5 8.5 16 12l-3.5 3.5"/></svg>',
    'up': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="12" cy="12" r="9.5"/><path d="M12 16.5v-9M8.5 11 12 7.5l3.5 3.5"/></svg>',
    'down': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="12" cy="12" r="9.5"/><path d="M12 7.5v9M8.5 13l3.5 3.5 3.5-3.5"/></svg>',
    'bars': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><rect x="2" y="14" width="4" height="8"/><rect x="8" y="9" width="4" height="13"/><rect x="14" y="5" width="4" height="17"/><rect x="20" y="1" width="4" height="21"/></svg>',
    'bars3': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><rect x="3" y="12" width="4" height="9"/><rect x="10" y="7" width="4" height="14"/><rect x="17" y="3" width="4" height="18"/></svg>',
    'pie': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M11 3.1A9.5 9.5 0 1 0 20.9 13H11z"/><path d="M13 2a9.5 9.5 0 0 1 9 9h-9z"/></svg>',
    'cloud': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M7 19a4.5 4.5 0 0 1-.8-8.93A6 6 0 0 1 17.8 9.6 4.7 4.7 0 0 1 17.5 19z"/></svg>',
    'doc': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round" aria-hidden="true"><path d="M6 2.5h7.5l4.5 4.5v14.5H6z"/><path d="M13.5 2.5V7h4.5M9 12h6M9 15.5h6"/></svg>',
    'mail': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linejoin="round" aria-hidden="true"><path d="M3 9.5 12 3l9 6.5V21H3z"/><path d="M3 9.5l9 6 9-6"/><path d="M7 11.5V5h10v6.5"/><path d="M9.5 7.5h5M9.5 9.5h5"/></svg>',
    'phone': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M6 3.5h3l1.5 4-2 1.5a11 11 0 0 0 6.5 6.5l1.5-2 4 1.5v3a2 2 0 0 1-2 2A16.5 16.5 0 0 1 4 5.5a2 2 0 0 1 2-2z"/><path d="M14.5 6a4 4 0 0 1 3.5 3.5M14.5 2.5a7.5 7.5 0 0 1 7 7"/></svg>',
    'globe': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" aria-hidden="true"><circle cx="12" cy="12" r="9.5"/><ellipse cx="12" cy="12" rx="4" ry="9.5"/><path d="M2.5 12h19M12 2.5v19M4 7.5h16M4 16.5h16"/></svg>',
    'logo': '<svg viewBox="0 0 56 56" fill="none" stroke="#fff" stroke-width="6.5" stroke-linejoin="round" aria-hidden="true"><path d="M9 11q-1-5 4-5l34-1.5q5 0 5 5L50 45q0 5-5 5L12 48q-5 0-5-5z"/></svg>',
}

# ---------------------------------------------------------------- الزخرفة المشتركة (الخطوط المتموّجة في الزوايا)
DECO_PATHS = ('<path d="M0,13 H290 C320,13 325,48 355,48 H400" stroke="url(#dgb)"/>'
              '<path d="M0,26 H210 C240,26 245,60 275,60 H330" stroke="url(#dgb)"/>'
              '<path d="M0,40 H300 C330,40 335,80 365,80 H400" stroke="url(#dgp)"/>')


def deco(pos):
    return '<svg class="deco %s" viewBox="0 0 400 100" aria-hidden="true">%s</svg>' % (pos, DECO_PATHS)


GLOBAL_DEFS = ('<defs>'
               '<linearGradient id="dgb" gradientUnits="userSpaceOnUse" x1="0" y1="0" x2="300" y2="0"><stop offset="0" stop-color="#9B9BF5" stop-opacity="0"/><stop offset="1" stop-color="#9B9BF5"/></linearGradient>'
               '<linearGradient id="dgp" gradientUnits="userSpaceOnUse" x1="0" y1="0" x2="300" y2="0"><stop offset="0" stop-color="#E9A9EC" stop-opacity="0"/><stop offset="1" stop-color="#E9A9EC"/></linearGradient>'
               '<linearGradient id="np" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#0E0A8C"/><stop offset="1" stop-color="#6D1677"/></linearGradient>'
               '<linearGradient id="pn" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#6D1677"/><stop offset="1" stop-color="#0E0A8C"/></linearGradient>'
               '<linearGradient id="pb" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#A826B8"/><stop offset="1" stop-color="#2A0ED0"/></linearGradient>'
               '<linearGradient id="gg" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#A826B8"/><stop offset="1" stop-color="#2A0ED0"/></linearGradient>'
               '<linearGradient id="gray" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#C4C4CE"/><stop offset=".5" stop-color="#FFFFFF"/><stop offset="1" stop-color="#C4C4CE"/></linearGradient>'
               '<linearGradient id="vb" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#B9B9FF"/><stop offset="1" stop-color="#F6F6FF"/></linearGradient>'
               '<linearGradient id="vp" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#E3A8F3"/><stop offset="1" stop-color="#FBF2FC"/></linearGradient>'
               '<linearGradient id="cv" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#0B008D"/><stop offset=".4" stop-color="#3A0A88"/><stop offset="1" stop-color="#6D1677"/></linearGradient>'
               '<linearGradient id="mc" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#9A28B8"/><stop offset="1" stop-color="#4A12B0"/></linearGradient>'
               '<linearGradient id="mx" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#F4F4F4"/><stop offset="1" stop-color="#E6E6E6"/></linearGradient>'
               '<linearGradient id="c1g" x1="1" y1="0" x2="0" y2="1"><stop offset=".2" stop-color="#2A0FA0" stop-opacity=".35"/><stop offset="1" stop-color="#9A2EB0" stop-opacity=".6"/></linearGradient>'
               '<linearGradient id="c2g" x1="0" y1="1" x2="1" y2="0"><stop offset=".15" stop-color="#1C0C9E" stop-opacity=".7"/><stop offset=".8" stop-color="#7E1E9C" stop-opacity=".75"/></linearGradient>'
               '<radialGradient id="c3g"><stop offset=".4" stop-color="#6A1FC0"/><stop offset="1" stop-color="#B444D8"/></radialGradient>'
               '<filter id="sh" x="-10%" y="-30%" width="120%" height="180%"><feDropShadow dx="0" dy="4" stdDeviation="5" flood-color="#000" flood-opacity=".22"/></filter>'
               '</defs>')

# ---------------------------------------------------------------- CSS
STYLE = """  /* ---- مشترك ---- */
  .biz { position: absolute; top: 74px; inset-inline-start: 36px; margin: 0; font-size: 15px; letter-spacing: 3.6px; color: #222; font-weight: 400; }
  html[lang="ar"] .biz { letter-spacing: 0; font-size: 17px; }
  .ttl { position: absolute; top: 112px; inset-inline-start: 36px; margin: 0; font-size: 44px; font-weight: 700; line-height: 1.3; color: var(--purple); }
  .lead { position: absolute; margin: 0; font-size: 20px; line-height: 1.45; color: #222; }
  .deco { position: absolute; width: 400px; height: 100px; }
  .deco path { fill: none; stroke-width: 3; stroke-linecap: round; }
  .deco.tr { top: 0; inset-inline-end: 0; } .deco.br { top: 620px; inset-inline-end: 0; }
  .deco.bl { top: 620px; inset-inline-start: 0; transform: rotate(180deg); }
  html[dir="rtl"] .deco.tr, html[dir="rtl"] .deco.br { transform: scaleX(-1); }
  html[dir="rtl"] .deco.bl { transform: scaleY(-1); }
  .abs { position: absolute; margin: 0; }
  .h2 { font-weight: 700; line-height: 1.25; color: var(--ink); }
  .tx { font-size: 17px; line-height: 1.38; color: #222; }
  .ar-ic { width: 22px; height: 22px; flex: none; }
  html[dir="rtl"] .ar-ic { transform: scaleX(-1); }
  .bul { position: absolute; margin: 0; display: flex; gap: 10px; align-items: flex-start; }
  .bul p { margin: 0; }
  .mir { position: absolute; }
  html[dir="rtl"] .mir { transform: scaleX(-1); }
  .num { font-weight: 700; color: var(--blue); }
  .logo { position: absolute; width: 56px; height: 56px; }
  .logo-t { position: absolute; margin: 0; font-size: 13px; letter-spacing: 2.6px; line-height: 1.45; color: #fff; width: 170px; }
  html[lang="ar"] .logo-t { letter-spacing: 0; font-size: 15px; }
  /* ---- 1 الغلاف ---- */
  .s1 { background: linear-gradient(90deg, rgb(21 1 143 / 0) 55%, #15018F 88%), linear-gradient(180deg, #0B008D 0%, #3A0A88 40%, #6D1677 100%); }
  html[dir="rtl"] .s1 { background: linear-gradient(270deg, rgb(21 1 143 / 0) 55%, #15018F 88%), linear-gradient(180deg, #0B008D 0%, #3A0A88 40%, #6D1677 100%); }
  .s1 .art { position: absolute; inset: 0; width: 1280px; height: 720px; }
  html[dir="rtl"] .s1 .art { transform: scaleX(-1); }
  .s1 .h { position: absolute; top: 205px; inset-inline-start: 88px; margin: 0; width: 780px; font-size: 60px; font-weight: 800; line-height: 1.27; color: #fff; text-transform: uppercase; }
  html[lang="ar"] .s1 .h { font-size: 56px; }
  .s1 .sub { position: absolute; top: 370px; inset-inline-start: 88px; margin: 0; font-size: 28px; letter-spacing: 3px; color: #fff; font-weight: 400; text-transform: uppercase; }
  html[lang="ar"] .s1 .sub { letter-spacing: 0; }
  .s1 .logo { top: 598px; inset-inline-start: 78px; } .s1 .logo-t { top: 605px; inset-inline-start: 170px; }
  /* ---- 2 جدول المحتويات ---- */
  .s2 .lead { top: 222px; inset-inline-start: 36px; width: 400px; }
  .s2 .rings { position: absolute; top: 340px; inset-inline-start: 0; width: 520px; height: 380px; }
  html[dir="rtl"] .s2 .rings { transform: scaleX(-1); }
  .s2 .vl { position: absolute; top: 58px; inset-inline-start: 893px; width: 5px; height: 600px; background: linear-gradient(180deg, #8E1C9C 0%, #1C0AD0 35%, #1C0AD0 65%, rgb(28 10 208 / 0) 100%); }
  .s2 .it { position: absolute; width: 300px; }
  .s2 .it .n { margin: 0; font-size: 40px; font-weight: 700; color: var(--blue); line-height: 1; }
  .s2 .it p { margin: 10px 0 0; font-size: 21px; color: #222; }
  /* ---- 3 الملخص التنفيذي ---- */
  .s3 .lead { top: 206px; inset-inline-start: 36px; width: 560px; }
  .s3 .kp { position: absolute; top: 397px; width: 180px; }
  .s3 .kp h3 { margin: 0; font-size: 19px; font-weight: 700; color: var(--ink); }
  .s3 .kp .v { margin: 12px 0 0; font-size: 46px; font-weight: 700; line-height: 1; color: var(--blue); }
  .s3 .kp .v sup { font-size: 24px; vertical-align: top; position: relative; top: 2px; }
  .s3 .kp .v.pu { color: var(--purple); }
  .s3 .kp p { margin: 16px 0 0; font-size: 17px; line-height: 1.35; color: #222; width: 160px; }
  .s3 .card { position: absolute; width: 272px; height: 258px; border-radius: 40px; background: var(--navy); color: #fff; }
  .s3 .card.g { background: linear-gradient(135deg, #0A00DB 0%, #4A10B5 55%, #6D1677 100%); }
  html[dir="rtl"] .s3 .card.g { background: linear-gradient(225deg, #0A00DB 0%, #4A10B5 55%, #6D1677 100%); }
  .s3 .card h3 { position: absolute; top: 66px; inset-inline-start: 30px; margin: 0; width: 150px; font-size: 26px; font-weight: 700; line-height: 1.25; }
  .s3 .card p { position: absolute; top: 150px; inset-inline-start: 30px; margin: 0; width: 200px; font-size: 20px; line-height: 1.3; }
  /* ---- 4 مسوّغات التغيير ---- */
  .s4 .hd { position: absolute; top: 193px; inset-inline-start: 42px; margin: 0; font-size: 20px; font-weight: 700; color: var(--ink); }
  .s4 .ul { position: absolute; top: 222px; inset-inline-start: 36px; width: 604px; height: 2px; background: var(--purple); }
  .s4 .ch { position: absolute; inset-inline-start: 40px; width: 390px; height: 180px; }
  .s4 .ch text { font-size: 12px; fill: #555; }
  .s4 .kv { position: absolute; inset-inline-start: 457px; margin: 0; font-size: 42px; font-weight: 700; color: var(--purple); line-height: 1; display: flex; align-items: center; gap: 18px; }
  .s4 .kv.bl { color: var(--blue); } .s4 .kv sup { font-size: 22px; vertical-align: top; }
  .s4 .kv svg { width: 32px; height: 32px; }
  .s4 .kt { position: absolute; inset-inline-start: 457px; width: 160px; margin: 0; font-size: 17px; line-height: 1.35; color: #222; }
  .s4 .st { position: absolute; top: 112px; inset-inline-start: 722px; margin: 0; width: 440px; font-size: 30px; font-weight: 700; line-height: 1.3; color: var(--ink); }
  .s4 .sl { position: absolute; top: 222px; inset-inline-start: 722px; width: 152px; height: 3px; background: var(--purple); }
  .s4 .sp { position: absolute; top: 250px; inset-inline-start: 722px; margin: 0; width: 490px; font-size: 21px; line-height: 1.4; color: #222; }
  .s4 .row { position: absolute; inset-inline-start: 708px; width: 508px; height: 103px; border-bottom: 1px solid #D5D5D5; }
  .s4 .row.last { border-bottom: 0; }
  .s4 .row h4 { position: absolute; top: 20px; inset-inline-start: 8px; margin: 0; width: 160px; font-size: 20px; font-weight: 700; line-height: 1.25; color: var(--purple); }
  .s4 .row p { position: absolute; top: 20px; inset-inline-start: 182px; margin: 0; width: 310px; font-size: 18px; line-height: 1.3; color: #222; }
  /* ---- 5 تقييم النضج ---- */
  .s5 .ttl { width: 560px; }
  .s5 .lv { position: absolute; width: 160px; border-start-start-radius: 16px; border-start-end-radius: 16px; color: #fff; }
  .s5 .lv .lb { position: absolute; top: -32px; inset-inline-start: 8px; margin: 0; font-size: 16px; letter-spacing: 3px; color: #333; white-space: nowrap; }
  html[lang="ar"] .s5 .lv .lb { letter-spacing: 0; font-size: 17px; }
  .s5 .lv p { position: absolute; top: 40px; inset-inline-start: 10px; margin: 0; width: 145px; font-size: 18px; line-height: 1.3; }
  .s5 .chev { position: absolute; top: 585px; inset-inline-start: 20px; width: 1240px; height: 75px; }
  html[dir="rtl"] .s5 .chev { transform: scaleX(-1); }
  .s5 .chev polygon { fill: #fff; filter: url(#sh); }
  .s5 .ch-t { position: absolute; top: 606px; margin: 0; font-size: 17px; font-weight: 700; letter-spacing: 4px; color: var(--ink); text-align: center; }
  html[lang="ar"] .s5 .ch-t { letter-spacing: 0; font-size: 19px; }
  /* ---- 6 الرؤية ---- */
  .s6 .ttl { width: 560px; } .s6 .lead { top: 260px; inset-inline-start: 36px; width: 390px; }
  .s6 .art { position: absolute; top: 60px; inset-inline-start: 320px; width: 740px; height: 600px; }
  html[dir="rtl"] .s6 .art { transform: scaleX(-1); }
  .s6 .cc { position: absolute; margin: 0; color: #fff; text-align: center; }
  .s6 .cc.main { top: 335px; inset-inline-start: 563px; width: 240px; font-size: 17px; font-weight: 700; letter-spacing: 3px; line-height: 1.4; }
  html[lang="ar"] .s6 .cc.main { letter-spacing: 0; font-size: 19px; }
  .s6 .cc.sat { width: 170px; font-size: 17px; line-height: 1.35; }
  .s6 .st { position: absolute; top: 330px; inset-inline-start: 858px; margin: 0; width: 370px; font-size: 20px; font-weight: 700; line-height: 1.3; color: var(--ink); }
  /* ---- 7 الركائز ---- */
  .s7 .ttl { width: 640px; }
  .s7 .band { position: absolute; top: 383px; inset-inline-start: 0; width: 1280px; height: 200px; background: linear-gradient(180deg, #C5C6FF 0%, rgb(255 255 255 / 0) 100%); }
  .s7 .pl { position: absolute; top: 242px; width: 280px; height: 141px; border-start-start-radius: 16px; border-start-end-radius: 16px; color: #fff; }
  .s7 .pl h3 { position: absolute; top: 32px; inset-inline-start: 30px; margin: 0; width: 220px; font-size: 20px; font-weight: 700; line-height: 1.3; text-align: center; }
  .s7 .pl .bd { position: absolute; top: 141px; inset-inline-start: 22px; width: 236px; height: 257px; background: inherit; }
  .s7 .pl .bd p { position: absolute; top: 8px; inset-inline-start: 18px; margin: 0; width: 200px; font-size: 18px; line-height: 1.3; text-align: center; }
  .s7 .pl .bd i { position: absolute; top: 107px; inset-inline-start: 66px; width: 105px; height: 1px; background: #fff; }
  .s7 .pl .bd b { position: absolute; top: 130px; inset-inline-start: 0; width: 236px; margin: 0; font-size: 30px; font-weight: 700; text-align: center; }
  /* ---- 8 المصفوفة ---- */
  .s8 .ttl { width: 1000px; }
  .s8 .mx { position: absolute; top: 212px; inset-inline-start: 60px; width: 740px; height: 400px; }
  html[dir="rtl"] .s8 .mx { transform: scaleX(-1); }
  .s8 .ax { position: absolute; margin: 0; font-size: 20px; font-weight: 700; color: var(--ink); }
  .s8 .ax.y { top: 380px; inset-inline-start: -32px; width: 140px; text-align: center; transform: rotate(-90deg); }
  html[dir="rtl"] .s8 .ax.y { transform: rotate(90deg); }
  .s8 .ax.x { top: 585px; inset-inline-start: 340px; }
  .s8 .bt { position: absolute; margin: 0; font-size: 18px; line-height: 1.3; color: #222; }
  .s8 .pan { position: absolute; top: 215px; inset-inline-start: 815px; width: 465px; height: 505px; border-start-start-radius: 80px; color: #fff; background: linear-gradient(135deg, #6D1677 0%, #4612A8 50%, #2B0BC9 100%); }
  html[dir="rtl"] .s8 .pan { background: linear-gradient(225deg, #6D1677 0%, #4612A8 50%, #2B0BC9 100%); }
  .s8 .pan h3 { position: absolute; top: 40px; inset-inline-start: 50px; margin: 0; width: 330px; font-size: 30px; font-weight: 700; line-height: 1.3; }
  .s8 .pan i { position: absolute; top: 150px; inset-inline-start: 50px; width: 153px; height: 3px; background: #fff; }
  .s8 .pan .p { position: absolute; top: 177px; inset-inline-start: 50px; margin: 0; width: 340px; font-size: 21px; line-height: 1.3; }
  .s8 .pan .bul { inset-inline-start: 50px; width: 330px; font-size: 18px; line-height: 1.3; }
  /* ---- 9 جانت ---- */
  .s9 .big { position: absolute; top: 185px; margin: 0; font-size: 44px; font-weight: 400; color: var(--blue); line-height: 1; }
  .s9 .bar { position: absolute; top: 245px; height: 8px; background: var(--blue); }
  .s9 .hh { position: absolute; top: 185px; margin: 0; font-size: 20px; font-weight: 700; color: var(--ink); }
  .s9 .ht { position: absolute; top: 222px; margin: 0; width: 230px; font-size: 18px; line-height: 1.3; color: #222; }
  .s9 .thead { position: absolute; top: 290px; inset-inline-start: 375px; display: flex; }
  .s9 .thead p { margin: 0; width: 145px; height: 38px; line-height: 38px; text-align: center; color: #fff; font-size: 18px; font-weight: 500; background: var(--navy); }
  .s9 .thead p:nth-child(even) { background: var(--blue); }
  .s9 .cols { position: absolute; top: 329px; inset-inline-start: 375px; display: flex; }
  .s9 .cols i { display: block; width: 145px; height: 321px; background: #F3F3F3; }
  .s9 .cols i:nth-child(even) { background: #E9E9E9; }
  .s9 .rw { position: absolute; inset-inline-start: 36px; width: 1209px; height: 64px; border-bottom: 1px solid #DADADA; }
  .s9 .rw p { position: absolute; top: 0; inset-inline-start: 9px; margin: 0; line-height: 64px; font-size: 19px; color: #222; }
  .s9 .gantt { position: absolute; top: 329px; inset-inline-start: 375px; width: 870px; height: 321px; }
  html[dir="rtl"] .s9 .gantt { transform: scaleX(-1); }
  /* ---- 10 الخط الزمني ---- */
  .s10 .qb { position: absolute; top: 380px; inset-inline-start: 0; width: 1280px; height: 80px; background: linear-gradient(90deg, #0A00DB 0%, #5A12C8 45%, #9C1EB8 100%); }
  html[dir="rtl"] .s10 .qb { background: linear-gradient(270deg, #0A00DB 0%, #5A12C8 45%, #9C1EB8 100%); }
  .s10 .q { position: absolute; top: 403px; margin: 0; font-size: 28px; font-weight: 700; color: #fff; line-height: 1.2; }
  .s10 .el { position: absolute; top: 330px; inset-inline-start: 0; width: 1280px; height: 200px; }
  html[dir="rtl"] .s10 .el { transform: scaleX(-1); }
  .s10 .col { position: absolute; top: 208px; width: 1px; height: 142px; background: #D8D8D8; }
  .s10 .ch { position: absolute; top: 230px; margin: 0; font-size: 20px; font-weight: 700; color: #0A0AA0; }
  .s10 .ct { position: absolute; top: 265px; margin: 0; width: 160px; font-size: 18px; line-height: 1.3; color: #222; }
  .s10 .ml { position: absolute; top: 460px; width: 1px; height: 95px; background: #CFCFCF; }
  .s10 .ms { position: absolute; top: 557px; display: flex; align-items: center; gap: 16px; margin: 0; font-size: 19px; font-weight: 700; color: #9A9A9A; letter-spacing: .5px; }
  .s10 .ms svg { width: 36px; height: 36px; flex: none; }
  /* ---- 11 الطبقات ---- */
  .s11 .ly { position: absolute; top: 170px; inset-inline-start: 40px; width: 680px; height: 490px; }
  html[dir="rtl"] .s11 .ly { transform: scaleX(-1); }
  .s11 .lh { position: absolute; inset-inline-start: 440px; width: 237px; margin: 0; font-size: 20px; font-weight: 700; color: var(--ink); text-align: end; }
  .s11 .lt { position: absolute; inset-inline-start: 420px; width: 257px; margin: 0; font-size: 17px; line-height: 1.38; color: #222; text-align: end; }
  .s11 .bul { width: 200px; font-size: 17px; line-height: 1.38; color: #222; }
  .s11 .bul p { color: #222; }
  /* ---- 12 فين ---- */
  .s12 .ttl { width: 440px; } .s12 .lead { top: 260px; inset-inline-start: 36px; width: 390px; }
  .s12 .vn { position: absolute; top: 80px; inset-inline-start: 400px; width: 630px; height: 570px; }
  html[dir="rtl"] .s12 .vn { transform: scaleX(-1); }
  .s12 .vt { position: absolute; margin: 0; width: 180px; font-size: 18px; line-height: 1.3; color: var(--ink); text-align: center; }
  .s12 .vh { position: absolute; margin: 0; width: 220px; font-size: 20px; font-weight: 700; line-height: 1.3; color: var(--ink); }
  /* ---- 13 الترس ---- */
  .s13 .gr { position: absolute; top: 170px; inset-inline-start: 430px; width: 420px; height: 420px; }
  .s13 .gh { position: absolute; margin: 0; width: 260px; font-size: 20px; font-weight: 700; line-height: 1.25; color: var(--ink); }
  .s13 .gl { position: absolute; width: 410px; height: 2px; background: var(--purple); }
  .s13 .gl::before { content: ''; position: absolute; top: -7px; width: 16px; height: 16px; border-radius: 50%; background: var(--purple); }
  .s13 .gl.l::before { inset-inline-start: -8px; } .s13 .gl.r::before { inset-inline-end: -8px; }
  .s13 .gt { position: absolute; margin: 0; width: 290px; font-size: 17px; line-height: 1.38; color: #222; }
  .s13 .r { text-align: end; }
  /* ---- 14 الإطار الأخلاقي ---- */
  .s14 .cd { position: absolute; top: 205px; width: 305px; height: 395px; background: #fff; border-start-start-radius: 80px; box-shadow: -4px -4px 34px rgb(0 0 0 / .1), 0 10px 24px rgb(0 0 0 / .06); }
  html[dir="rtl"] .s14 .cd { box-shadow: 4px -4px 34px rgb(0 0 0 / .1), 0 10px 24px rgb(0 0 0 / .06); }
  .s14 .cd h3 { position: absolute; top: 78px; inset-inline-start: 30px; margin: 0; width: 250px; font-size: 22px; font-weight: 700; line-height: 1.3; color: var(--ink); }
  .s14 .cd .p { position: absolute; top: 170px; inset-inline-start: 35px; margin: 0; width: 260px; font-size: 17px; line-height: 1.38; color: #222; }
  .s14 .cd .bul { inset-inline-start: 35px; width: 250px; font-size: 17px; line-height: 1.38; color: #222; }
  .s14 .cd .bul svg { color: var(--purple); }
  .s14 .br { position: absolute; top: 240px; width: 290px; height: 380px; }
  html[dir="rtl"] .s14 .br { transform: scaleX(-1); }
  /* ---- 15 المؤشرات ---- */
  .s15 .k { position: absolute; width: 172px; height: 172px; border: 1px solid #CFCFCF; border-radius: 14px; }
  .s15 .k h3 { position: absolute; top: 22px; inset-inline-start: 15px; margin: 0; font-size: 18px; font-weight: 400; color: #222; }
  .s15 .k .v { position: absolute; top: 52px; inset-inline-start: 15px; margin: 0; font-size: 34px; font-weight: 700; line-height: 1; color: var(--blue); }
  .s15 .k .v sup { font-size: 17px; vertical-align: top; position: relative; top: 2px; }
  .s15 .k.pu .v { color: var(--purple); }
  .s15 .k .pb { position: absolute; top: 110px; inset-inline-start: 15px; width: 72px; height: 6px; }
  .s15 .k .pb::before { content: ''; position: absolute; inset-inline-start: 0; top: 0; width: 55px; height: 6px; border-radius: 3px; background: var(--blue); }
  .s15 .k .pb::after { content: ''; position: absolute; inset-inline-end: 0; top: 0; width: 12px; height: 6px; border-radius: 3px; background: var(--blue); }
  .s15 .k.pu .pb::before, .s15 .k.pu .pb::after { background: var(--purple); }
  .s15 .k .d { position: absolute; top: 132px; inset-inline-start: 15px; margin: 0; display: flex; align-items: center; gap: 8px; font-size: 16px; color: #9A9A9A; }
  .s15 .k .d svg { width: 20px; height: 20px; color: var(--ok); }
  .s15 .note { position: absolute; top: 583px; inset-inline-start: 85px; margin: 0; width: 300px; font-size: 18px; line-height: 1.35; color: #222; }
  .s15 .bn { position: absolute; top: 187px; inset-inline-start: 505px; width: 710px; height: 133px; border-radius: 22px; color: #fff; background: linear-gradient(90deg, #7E1A8A 0%, #5A12C0 50%, #0A00DB 100%); }
  html[dir="rtl"] .s15 .bn { background: linear-gradient(270deg, #7E1A8A 0%, #5A12C0 50%, #0A00DB 100%); }
  .s15 .bn svg { position: absolute; top: 28px; inset-inline-start: 32px; width: 70px; height: 70px; }
  .s15 .bn h3 { position: absolute; top: 36px; inset-inline-start: 145px; margin: 0; font-size: 24px; font-weight: 700; }
  .s15 .bn p { position: absolute; top: 75px; inset-inline-start: 145px; margin: 0; font-size: 19px; }
  .s15 .ch { position: absolute; top: 345px; inset-inline-start: 505px; width: 710px; height: 305px; }
  .s15 .ch text { font-size: 13px; fill: #444; }
  .s15 .pill { position: absolute; top: 572px; inset-inline-start: 558px; height: 42px; display: flex; align-items: center; border-radius: 21px; background: #F1C3F6; padding-inline-end: 40px; }
  .s15 .pill b { height: 42px; line-height: 42px; padding: 0 20px; border-radius: 21px; background: linear-gradient(135deg, #6D1677, #4A12A8); color: #fff; font-size: 22px; font-weight: 700; }
  .s15 .pill span { padding-inline-start: 14px; font-size: 15px; color: var(--purple); }
  /* ---- 16 الخطوات التالية ---- */
  .s16 .ttl { width: 480px; } .s16 .lead { top: 260px; inset-inline-start: 36px; width: 390px; }
  .s16 .st { position: absolute; inset: 0; width: 1280px; height: 720px; }
  html[dir="rtl"] .s16 .st { transform: scaleX(-1); }
  .s16 .ic { position: absolute; width: 46px; height: 46px; color: #fff; }
  .s16 .n { position: absolute; margin: 0; font-size: 40px; font-weight: 300; line-height: 1; color: var(--blue); }
  .s16 .sh { position: absolute; margin: 0; font-size: 22px; font-weight: 700; color: var(--ink); }
  .s16 .sp { position: absolute; margin: 0; width: 280px; font-size: 17px; line-height: 1.38; color: #222; }
  /* ---- 17 شكرًا ---- */
  .s17 .ttl { font-size: 54px; width: 500px; }
  html[lang="ar"] .s17 .ttl { font-size: 50px; }
  .s17 .lead { top: 282px; inset-inline-start: 36px; width: 360px; font-size: 19px; }
  .s17 .rg { position: absolute; top: 0; inset-inline-start: 0; width: 900px; height: 720px; }
  html[dir="rtl"] .s17 .rg { transform: scaleX(-1); }
  .s17 .logo { top: 560px; inset-inline-start: 112px; } .s17 .logo-t { top: 570px; inset-inline-start: 200px; }
  .s17 .cl { position: absolute; inset-inline-start: 759px; margin: 0; font-size: 20px; font-weight: 700; color: var(--ink); }
  .s17 .cv { position: absolute; inset-inline-start: 759px; margin: 0; font-size: 18px; color: #222; }
  .s17 .ci { position: absolute; inset-inline-start: 1082px; width: 48px; height: 48px; color: var(--purple); }"""


# ---------------------------------------------------------------- بناء الشرائح
def common(ttl_sym, tr=True, bl=True):
    return ('<p class="biz">§BIZ§</p><h1 class="ttl">§%s§</h1>' % ttl_sym) + (deco('tr') if tr else '') + (deco('bl') if bl else '')


def bul(x, y, sym, color=''):
    return '<div class="bul" style="top:%dpx;inset-inline-start:%dpx%s">%s<p>§%s§</p></div>' % (y, x, (';color:' + color) if color else '', ICONS['arrow'], sym)


# 1 الغلاف
def cover_art():
    lines = ('<g fill="none" stroke="#fff" stroke-width="3.5" stroke-linecap="round">'
             '<path d="M0,62 H760 C830,62 870,150 950,150"/><path d="M0,98 H790 C860,98 890,185 965,185"/><path d="M0,134 H820 C890,134 920,222 975,222"/></g>'
             '<circle cx="965" cy="150" r="10" fill="#8B2BB0"/><circle cx="982" cy="222" r="12" fill="#8B2BB0"/>')
    faint = ('<g fill="none" stroke="#C98BE0" stroke-opacity=".3" stroke-width="3">'
             '<path d="M90,460 H545 C585,460 585,395 625,395 H830"/><path d="M330,500 H555 C595,500 600,432 640,432 H885"/><path d="M0,540 H560 C600,540 605,500 645,500 H900"/></g>')
    return ('<svg class="art" viewBox="0 0 1280 720" aria-hidden="true">'
            '<circle cx="1210" cy="105" r="252" fill="url(#c1g)"/>'
            '<circle cx="940" cy="805" r="262" fill="url(#c2g)"/>'
            + faint + lines +
            '<circle cx="937" cy="475" r="119" fill="url(#c3g)"/><circle cx="940" cy="475" r="48" fill="#AF72D1"/>'
            '</svg>')


S1 = ('\n    ' + cover_art() + '\n    <h1 class="h">§COVER§</h1><p class="sub">§COVER_SUB§</p>'
      '\n    <div class="logo">' + ICONS['logo'] + '</div><p class="logo-t">§LOGO§</p>\n  ')

# 2 جدول المحتويات
def toc_item(x, y, n):
    return '<div class="it" style="top:%dpx;inset-inline-start:%dpx"><p class="n">%02d</p><p>§SEC§</p></div>' % (y, x, n)


S2 = ('\n    ' + common('T2', tr=False, bl=False)
      + '<p class="lead">§LEAD§</p>'
      + '<svg class="rings" viewBox="0 340 520 380" aria-hidden="true">' + ring(105, 622, 265, 158, 'url(#np)') + ring(290, 785, 192, 118, 'url(#np)')
      + '<circle cx="420" cy="470" r="47" fill="#6D1677"/><circle cx="407" cy="572" r="16" fill="#0E0A8C"/></svg>'
      + '<div class="vl"></div>'
      + ''.join(toc_item(565, y, i + 1) for i, y in enumerate((95, 228, 370, 512)))
      + ''.join(toc_item(945, y, i + 5) for i, y in enumerate((95, 228, 370)))
      + '\n  ')

# 3 الملخص التنفيذي
def kp(x, val, cls):
    return '<div class="kp" style="inset-inline-start:%dpx"><h3>§HH§</h3><p class="v %s">%s</p><p>§INS§</p></div>' % (x, cls, val)


def card3(x, y, cls=''):
    return '<div class="card %s" style="top:%dpx;inset-inline-start:%dpx"><h3>§HH§</h3><p>§INS§</p></div>' % (cls, y, x)


S3 = ('\n    ' + common('T3') + '<p class="lead">§LEAD2§</p>'
      + kp(36, '$4.5M', '') + kp(240, '35<sup>%</sup>', 'pu') + kp(448, '300<sup>+</sup>', '')
      + card3(665, 90) + card3(957, 90) + card3(662, 376) + card3(953, 376, 'g') + '\n  ')

# 4 مسوّغات التغيير
def row4(y, last=False):
    return '<div class="row%s" style="top:%dpx"><h4>§HPH§</h4><p>§INS2§</p></div>' % (' last' if last else '', y)


S4 = ('\n    ' + common('T4') + '<p class="hd">§HH§</p><div class="ul"></div>'
      '<svg class="ch" id="c4a" viewBox="0 0 390 180" style="top:245px" aria-hidden="true"></svg>'
      '<svg class="ch" id="c4b" viewBox="0 0 390 180" style="top:472px" aria-hidden="true"></svg>'
      '<p class="kv" style="top:293px">88 <span style="color:var(--ok)">' + ICONS['up'] + '</span></p><p class="kt" style="top:345px">§INS§</p>'
      '<p class="kv bl" style="top:520px">99.4<sup>%</sup> <span style="color:var(--danger)">' + ICONS['down'] + '</span></p><p class="kt" style="top:573px">§INS§</p>'
      '<h2 class="st">§HST§</h2><div class="sl"></div><p class="sp">§LEAD2§</p>'
      + row4(342) + row4(449) + row4(556, True) + '\n  ')

# 5 سلّم النضج
LV = [(37, 420, 'var(--purple)'), (211, 405, 'var(--purple)'), (385, 368, 'var(--purple)'), (559, 316, 'var(--navy)'), (733, 279, 'var(--navy)'), (907, 232, 'var(--blue)'), (1081, 157, 'var(--blue)')]
S5 = ('\n    ' + common('T5')
      + ''.join('<div class="lv" style="top:%dpx;inset-inline-start:%dpx;height:%dpx;background:%s"><p class="lb">§LVL§ %d</p><p>§INS3§</p></div>' % (t, x, 585 - t, c, i + 1) for i, (x, t, c) in enumerate(LV))
      + '<svg class="chev" viewBox="20 585 1240 75" aria-hidden="true"><polygon points="20,585 530,585 548,616 530,647 20,647"/><polygon points="548,585 880,585 897,616 880,647 548,647"/><polygon points="897,585 1228,585 1245,616 1228,647 897,647"/></svg>'
      + ''.join('<p class="ch-t" style="inset-inline-start:%dpx;width:%dpx">§CHEV§</p>' % (x, w) for x, w in ((40, 470), (560, 310), (910, 305)))
      + '\n  ')

# 6 الرؤية
def vision_art():
    cx, cy = 683, 358
    sat = [(950, 160), (428, 558), (942, 558)]
    conn = ''
    for sx, sy in sat:
        a = math.degrees(math.atan2(sy - cy, sx - cx))
        x1, y1 = pt(cx, cy, 170, a); x2, y2 = pt(sx, sy, 100, a + 180)
        conn += '<line x1="%.1f" y1="%.1f" x2="%.1f" y2="%.1f" stroke="#0A00DB" stroke-width="1"/>' % (x1, y1, x2, y2)
    return ('<svg class="art" viewBox="320 60 740 600" aria-hidden="true">' + conn
            + '<path d="%s" fill="none" stroke="#0A00DB" stroke-width="5" stroke-linecap="round"/>' % arc(cx, cy, 170, 12, 341)
            + dots(cx, cy, 141, -90, 211, 6.6, 3.2, '#A020C0')
            + '<circle cx="%d" cy="%d" r="122" fill="url(#mc)"/>' % (cx, cy)
            + ''.join('<circle cx="%d" cy="%d" r="100" fill="url(#mc)"/>' % s for s in sat) + '</svg>')


S6 = ('\n    ' + common('T6') + '<p class="lead">§LEAD§</p>' + vision_art()
      + '<p class="cc main">§CHEV§</p>'
      + ''.join('<p class="cc sat" style="top:%dpx;inset-inline-start:%dpx">§INS3§</p>' % (y, x) for x, y in ((865, 125), (343, 535), (857, 535)))
      + '<h2 class="st">§HST2§</h2>\n  ')

# 7 الركائز
S7 = ('\n    ' + common('T7') + '<div class="band"></div>'
      + ''.join('<div class="pl" style="inset-inline-start:%dpx;background:%s"><h3>§HST§</h3><div class="bd"><p>§LEAD3§</p><i></i><b>%s</b></div></div>' % (x, c, v)
                for x, c, v in ((36, 'var(--purple)', '20%'), (346, 'var(--navy)', '35%'), (655, 'var(--blue)', '65%'), (965, 'var(--sky)', '8%')))
      + '\n  ')

# 8 المصفوفة
def bubble(cx, cy, r, fill, letter):
    rings = ''.join('<circle cx="%d" cy="%d" r="%d" fill="none" stroke="%s" stroke-width="1.2"/>' % (cx, cy, r + d, c) for d, c in ((10, '#C878D8'), (18, '#D898E0'), (28, '#E8B8EC')))
    return rings + '<circle cx="%d" cy="%d" r="%d" fill="%s"/>' % (cx, cy, r, fill)


S8 = ('\n    ' + common('T8')
      + '<svg class="mx" viewBox="60 212 740 400" aria-hidden="true">'
        '<rect x="78" y="228" width="702" height="337" fill="url(#mx)"/><rect x="445" y="228" width="10" height="337" fill="#fff"/><line x1="78" y1="383" x2="780" y2="383" stroke="#BDBDBD"/>'
        '<path d="M70,570 V225 M70,570 H790" fill="none" stroke="#5C1A78" stroke-width="2"/><polygon points="63,228 70,214 77,228" fill="#5C1A78"/><polygon points="787,563 801,570 787,577" fill="#5C1A78"/>'
      + bubble(215, 278, 28, '#0E0A8C', 'B') + bubble(576, 282, 36, '#8A8AAA', 'A') + bubble(252, 458, 28, '#0A00DB', 'C') + bubble(500, 420, 27, '#6D1677', 'D') + '</svg>'
      + ''.join('<p class="abs" style="top:%dpx;inset-inline-start:%dpx;color:#fff;font-size:22px;font-weight:500">%s</p>' % (y, x, l) for x, y, l in ((207, 264, 'B'), (567, 268, 'A'), (244, 444, 'C'), (492, 406, 'D')))
      + '<p class="ax y">§AXY§</p><p class="ax x">§AXX§</p>'
      + ''.join('<p class="bt" style="top:%dpx;inset-inline-start:%dpx;width:%dpx">§INS§</p>' % (y, x, w) for x, y, w in ((283, 265, 170), (667, 230, 120), (310, 475, 170), (574, 415, 170)))
      + '<div class="pan"><h3>§HST§</h3><i></i><p class="p">§LEAD§</p>' + bul(50, 297, 'INS2') + bul(50, 380, 'INS2') + '</div>\n  ')

# 9 جانت
def gbar(x0, x1, cy):
    y0 = cy - 16
    return '<polygon points="%d,%d %d,%d %d,%d %d,%d %d,%d" fill="#C5C5F5"/>' % (x0, y0, x1 - 18, y0, x1, cy, x1 - 18, y0 + 32, x0, y0 + 32)


S9 = ('\n    ' + common('T9')
      + '<p class="big" style="inset-inline-start:373px">95+</p><div class="bar" style="inset-inline-start:375px;width:68px"></div><p class="hh" style="inset-inline-start:486px">§HH§</p><p class="ht" style="inset-inline-start:486px">§INS2§</p>'
      + '<p class="big" style="inset-inline-start:773px">203+</p><div class="bar" style="inset-inline-start:775px;width:108px"></div><p class="hh" style="inset-inline-start:917px">§HH§</p><p class="ht" style="inset-inline-start:917px">§INS2§</p>'
      + '<div class="cols">' + '<i></i>' * 6 + '</div>'
      + ''.join('<div class="rw" style="top:%dpx"><p>§INS4§</p></div>' % (329 + i * 64) for i in range(5))
      + '<div class="thead">' + ''.join('<p>§M§%d</p>' % (i + 1) for i in range(6)) + '</div>'
      + '<svg class="gantt" viewBox="375 329 870 321" aria-hidden="true">' + gbar(390, 695, 361) + gold_star(720, 361) + gbar(565, 865, 425) + gbar(565, 983, 489) + gold_star(1008, 489) + gbar(490, 1047, 553) + gbar(565, 1123, 617) + '</svg>'
      + '\n  ')

# 10 الخط الزمني
EL_X = (255, 468, 710, 943, 1178)
S10 = ('\n    ' + common('T10')
       + ''.join('<div class="col" style="inset-inline-start:%dpx"></div>' % x for x in (292, 505, 747, 981, 1215))
       + ''.join('<p class="ch" style="inset-inline-start:%dpx">§HH§</p><p class="ct" style="inset-inline-start:%dpx">§INS§</p>' % (x, x) for x in (112, 324, 566, 800, 1034))
       + '<div class="qb"></div>'
       + ''.join('<p class="q" style="inset-inline-start:%dpx">§Q§%d</p>' % (x, q) for x, q in ((110, 1), (320, 2), (565, 3), (800, 4), (1033, 1)))
       + '<svg class="el" viewBox="0 330 1280 200" aria-hidden="true">'
       + ''.join('<ellipse cx="%d" cy="425" rx="42" ry="85" fill="url(#gray)"/><path d="%s" fill="none" stroke="#9A1FB5" stroke-width="5" stroke-linecap="round"/>' % (x, arc(x, 425, 42, 55, 305, 85)) for x in EL_X)
       + '</svg>'
       + ''.join('<div class="ml" style="inset-inline-start:%dpx"></div><p class="ms" style="inset-inline-start:%dpx"><svg viewBox="0 0 36 36" aria-hidden="true"><circle cx="18" cy="18" r="17" fill="var(--gold)"/>%s</svg>§MS§ %d</p>' % (x, x - 18, star(18, 18, 9), i + 1) for i, x in enumerate((340, 640, 1052)))
       + '\n  ')

# 11 الطبقات
def layer(cy, fill, op):
    return '<rect x="-100" y="-100" width="200" height="200" rx="40" fill="%s" fill-opacity="%s" transform="translate(200 %d) scale(1 .53) rotate(45)"/>' % (fill, op, cy)


S11 = ('\n    ' + common('T11')
       + '<svg class="ly" viewBox="40 170 680 490" aria-hidden="true">'
       + layer(268, '#CDEEFC', 1) + layer(340, '#6D1677', .85) + layer(412, '#0E0A8C', .9) + layer(484, '#0A00DB', .95) + layer(556, '#E5E5F0', 1)
       + '<g fill="none" stroke-width="3"><path d="M355,327 L490,245 H680" stroke="#6D1677"/><path d="M355,413 H680" stroke="#0E0A8C"/><path d="M355,503 L490,583 H680" stroke="#0A00DB"/></g>'
       + '<circle cx="355" cy="327" r="6" fill="#6D1677"/><circle cx="355" cy="413" r="6" fill="#0E0A8C"/><circle cx="355" cy="503" r="6" fill="#0A00DB"/></svg>'
       + ''.join('<p class="lh" style="top:%dpx">§HH§</p><p class="lt" style="top:%dpx">§INS2§</p>' % (h, t) for h, t in ((207, 268), (376, 437), (545, 606)))
       + ''.join(bul(x, y, 'INS', c) for (x, y, c) in ((740, 220, '#8F1FB0'), (990, 220, '#8F1FB0'), (740, 380, '#0E0A8C'), (990, 380, '#0E0A8C'), (740, 548, '#0A00DB'), (990, 548, '#0A00DB')))
       + '\n  ')

# 12 فين
def venn_circle(cx, cy, fill, stroke, gap_a, gap_w=22):
    a1 = gap_a + gap_w / 2.0; a2 = gap_a - gap_w / 2.0 + 360
    return ('<circle cx="%d" cy="%d" r="160" fill="%s" fill-opacity=".9"/>' % (cx, cy, fill)
            + '<path d="%s" fill="none" stroke="%s" stroke-width="4" stroke-linecap="round"/>' % (arc(cx, cy, 158, a1, a2), stroke))


S12 = ('\n    ' + common('T12') + '<p class="lead">§LEAD§</p>'
       + '<svg class="vn" viewBox="400 80 630 570" aria-hidden="true">'
       + venn_circle(713, 255, 'url(#vb)', '#0E0A8C', -8) + venn_circle(587, 462, 'url(#vp)', '#6D1677', 175) + venn_circle(838, 462, 'url(#vb)', '#0A00DB', 8) + '</svg>'
       + ''.join('<p class="vt" style="top:%dpx;inset-inline-start:%dpx">§LEAD3§</p>' % (y, x) for x, y in ((623, 180), (453, 435), (778, 435)))
       + '<p class="vh" style="top:105px;inset-inline-start:850px">§HST§</p><p class="vh" style="top:352px;inset-inline-start:1005px">§HST§</p><p class="vh" style="top:558px;inset-inline-start:270px;text-align:end;width:210px">§HST§</p>'
       + '\n  ')

# 13 الترس
S13 = ('\n    ' + common('T13')
       + '<svg class="gr" viewBox="430 170 420 420" aria-hidden="true">' + gear(640, 380, 205, 165) + '<circle cx="640" cy="380" r="71" fill="#fff"/></svg>'
       + ''.join('<h3 class="gh" style="top:%dpx;inset-inline-start:72px">§HST§</h3><div class="gl l" style="top:%dpx;inset-inline-start:50px"></div><p class="gt" style="top:%dpx;inset-inline-start:72px">§INS2§</p>' % (h, h + 67, h + 85) for h in (212, 412))
       + ''.join('<h3 class="gh r" style="top:%dpx;inset-inline-end:80px">§HST§</h3><div class="gl r" style="top:%dpx;inset-inline-end:50px"></div><p class="gt r" style="top:%dpx;inset-inline-end:80px">§INS2§</p>' % (h, h + 67, h + 85) for h in (212, 412))
       + '\n  ')

# 14 الإطار الأخلاقي
def card14(x):
    return ('<div class="cd" style="inset-inline-start:%dpx"><h3>§HST§</h3><p class="p">§INS2§</p>' % x + bul(35, 240, 'INS') + bul(35, 317, 'SMP') + '</div>'
            + '<svg class="br" viewBox="0 0 290 380" style="inset-inline-start:%dpx" aria-hidden="true"><path d="M273,8 V340 Q273,368 245,368 H0" fill="none" stroke="#6D1677" stroke-width="4"/><circle cx="273" cy="8" r="8" fill="#6D1677"/></svg>' % (x + 70))


S14 = '\n    ' + common('T14') + card14(55) + card14(470) + card14(886) + '\n  '

# 15 المؤشرات
def kcard(x, y, val, cls):
    return '<div class="k %s" style="top:%dpx;inset-inline-start:%dpx"><h3>§HH§</h3><p class="v">%s</p><div class="pb"></div><p class="d">%s 3.4%%</p></div>' % (cls, y, x, val, ICONS['up'])


S15 = ('\n    ' + common('T15')
       + kcard(77, 187, '<sup>$</sup>43.5M', 'pu') + kcard(272, 187, '76<sup>%</sup>', '') + kcard(77, 378, '300<sup>+</sup>', 'pu') + kcard(272, 378, '45<sup>+</sup>', '')
       + '<p class="note">§INS2§</p>'
       + '<div class="bn">' + ICONS['bars'] + '<h3>§HH§</h3><p>§INS2§</p></div>'
       + '<svg class="ch" id="c15" viewBox="0 0 710 305" aria-hidden="true"></svg>'
       + '<div class="pill"><b>4.5%</b><span>§INS5§</span></div>\n  ')

# 16 الخطوات التالية
def stairs_art():
    return ('<svg class="st" viewBox="0 0 1280 720" aria-hidden="true">'
            '<rect x="690" y="60" width="700" height="180" rx="90" fill="#C9C9E2"/>'
            '<circle cx="667" cy="220" r="97" fill="#0A00DB"/>'
            '<rect x="780" y="145" width="520" height="600" rx="25" fill="#fff"/>'
            '<circle cx="595" cy="318" r="97" fill="#0E0A8C"/>'
            '<rect x="690" y="255" width="610" height="500" rx="25" fill="#fff"/>'
            '<circle cx="523" cy="410" r="97" fill="#8F1FB0"/>'
            '<rect x="600" y="345" width="700" height="400" rx="25" fill="#fff"/>'
            '<rect x="505" y="455" width="800" height="300" rx="25" fill="#fff"/>'
            '<path d="M0,520 H330 a30,30 0 0 1 30,-30 V440 a30,30 0 0 1 30,-30 H500 V640 H0 z" fill="#6D1677"/>'
            '</svg>')


S16 = ('\n    ' + common('T16') + '<p class="lead">§LEAD§</p>' + stairs_art()
       + ''.join('<div class="ic" style="top:%dpx;inset-inline-start:%dpx">%s</div>' % (y, x, ICONS[k]) for x, y, k in ((710, 127, 'bars3'), (622, 197, 'pie'), (550, 297, 'cloud'), (457, 387, 'doc')))
       + ''.join('<p class="n" style="top:%dpx;inset-inline-start:%dpx;color:%s">%02d</p><h3 class="sh" style="top:%dpx;inset-inline-start:%dpx">§HH§</h3><p class="sp" style="top:%dpx;inset-inline-start:%dpx">§INS2§</p>'
                 % (y, x, c, n, y - 2, x + 70, y + 36, x + 70) for n, x, y, c in ((4, 803, 170, '#B8B8D2'), (3, 722, 280, 'var(--blue)'), (2, 637, 390, 'var(--blue)'), (1, 540, 503, 'var(--violet)')))
       + '\n  ')

# 17 شكرًا
S17 = ('\n    <svg class="rg" viewBox="0 0 900 720" aria-hidden="true">' + ring(245, 220, 470, 300, 'url(#pn)') + ring(575, 790, 275, 120, 'url(#np)') + '</svg>'
       + '<p class="biz">§BIZ§</p><h1 class="ttl">§T17§</h1><p class="lead">§LEAD§</p>' + deco('tr') + deco('br')
       + '<div class="logo">' + ICONS['logo'] + '</div><p class="logo-t">§LOGO§</p>'
       + ''.join('<p class="cl" style="top:%dpx">§%s§</p><p class="cv" style="top:%dpx">§INS6§</p><div class="ci" style="top:%dpx">%s</div>' % (y, s, y + 37, y - 11, ICONS[ic]) for y, s, ic in ((183, 'EMAIL', 'mail'), (313, 'CONTACT', 'phone'), (437, 'WEB', 'globe')))
       + '\n  ')

SLIDES = [S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, S14, S15, S16, S17]

# ---------------------------------------------------------------- النصوص
STR = {
    'ar': {
        'BIZ': 'اسم شركتك هنا', 'COVER': 'خارطة طريق التحول بالذكاء الاصطناعي', 'COVER_SUB': 'قالب عرض تقديمي', 'LOGO': 'شعار شركتك هنا',
        'T2': 'جدول المحتويات', 'SEC': 'عنوان القسم النائب',
        'LEAD': 'أدرج النص المطلوب هنا. هذا نص توضيحي. عبارة موجزة عن موضوعك.',
        'LEAD2': 'أدرج النص المطلوب هنا. هذا نص توضيحي. عبارة موجزة عن موضوعك.',
        'LEAD3': 'أدرج النص المطلوب هنا. هذا نص توضيحي.',
        'T3': 'الملخص التنفيذي', 'HH': 'العنوان هنا', 'INS': 'أدرج النص المطلوب هنا.',
        'T4': 'مسوّغات التغيير', 'HST': 'أدرج عبارة العنوان هنا.', 'HPH': 'عنوان نائب', 'INS2': 'أدرج النص المطلوب هنا. هذا نص توضيحي.',
        'T5': 'تقييم نضج الذكاء الاصطناعي الحالي', 'LVL': 'المستوى', 'INS3': 'أدرج النص المطلوب هنا. نص توضيحي.', 'CHEV': 'أدرج العنوان هنا',
        'T6': 'رؤية الذكاء الاصطناعي والمبادئ الأساسية', 'HST2': 'أدرج عبارة العنوان هنا. هذا نص توضيحي.',
        'T7': 'الركائز الاستراتيجية ونتائج الأعمال المنشودة',
        'T8': 'حالات الاستخدام ذات الأولوية وخريطة الأثر', 'AXY': 'قيمة الأعمال', 'AXX': 'الجدوى / الجهد',
        'T9': 'خارطة طريق التحول المرحلي', 'INS4': 'أدرج النص المطلوب هنا', 'M': 'ش',
        'T10': 'خارطة طريق التحول المرحلي', 'Q': 'ر', 'MS': 'المعلم',
        'T11': 'استراتيجية البيانات والبنية التحتية',
        'T12': 'استراتيجية المواهب والمهارات والثقافة',
        'T13': 'مخاطر الحوكمة',
        'T14': 'إطار الذكاء الاصطناعي الأخلاقي', 'SMP': 'هذا نص توضيحي.',
        'T15': 'مؤشرات أداء الذكاء الاصطناعي ومتابعتها', 'INS5': 'أدرج النص هنا',
        'T16': 'الخطوات التالية لتحول الذكاء الاصطناعي',
        'T17': 'شكرًا لكم<br>تواصلوا معنا', 'EMAIL': 'البريد الإلكتروني', 'CONTACT': 'الاتصال', 'WEB': 'الموقع الإلكتروني', 'INS6': 'أدرج نصك هنا',
    },
    'en': {
        'BIZ': 'YOUR BUSINESS NAME HERE', 'COVER': 'AI Transformation Roadmap', 'COVER_SUB': 'Presentation Template', 'LOGO': 'YOUR COMPANY LOGO HERE',
        'T2': 'Table of Contents', 'SEC': 'Section Title Placeholder',
        'LEAD': 'Insert your desired text here. This is a sample text. A brief statement about your topic.',
        'LEAD2': 'Insert your desired text here. This is a sample text. A brief statement about your topic.',
        'LEAD3': 'Insert your desired text here. This is a sample text.',
        'T3': 'Executive Summary', 'HH': 'Heading Here', 'INS': 'Insert your desired text here.',
        'T4': 'The Case for Change', 'HST': 'Insert your heading statement here.', 'HPH': 'Heading Placeholder', 'INS2': 'Insert your desired text here. This is a sample text.',
        'T5': 'Current State AI Maturity Assessment', 'LVL': 'LEVEL', 'INS3': 'Insert your desired text here. A sample text.', 'CHEV': 'INSERT HEADING HERE',
        'T6': 'AI Vision Statement &amp; Core Principles', 'HST2': 'Insert your heading statement here. This is a sample text.',
        'T7': 'Strategic Pillars &amp; Desired Business Outcomes',
        'T8': 'Prioritized AI Use Cases &amp; Impact Mapping', 'AXY': 'Business Value', 'AXX': 'Feasibility / Effort',
        'T9': 'The Phased Transformation Roadmap', 'INS4': 'Insert desired text here', 'M': 'M',
        'T10': 'The Phased Transformation Roadmap', 'Q': 'Q', 'MS': 'MILESTONE',
        'T11': 'Data &amp; Infrastructure Strategy',
        'T12': 'Talent, Skills &amp; Culture Strategy',
        'T13': 'Governance Risks',
        'T14': 'Ethical AI Framework', 'SMP': 'This is a sample text.',
        'T15': 'AI KPIs and Tracking', 'INS5': 'Insert text here',
        'T16': 'AI Transformation Next Steps',
        'T17': 'Thank You<br>Connect With Us', 'EMAIL': 'Email', 'CONTACT': 'Contact', 'WEB': 'Website', 'INS6': 'Insert your text here',
    },
}

# ---------------------------------------------------------------- كتل بيانات الرسوم
def chart(labels_ar, labels_en, mx, ticks, series):
    return {'ar': {'labels': labels_ar, 'max': mx, 'ticks': ticks, 'series': series},
            'en': {'labels': labels_en, 'max': mx, 'ticks': ticks, 'series': series}}


ITEMS_AR = ['البند 1', 'البند 2', 'البند 3', 'البند 4']; ITEMS_EN = ['Item 1', 'Item 2', 'Item 3', 'Item 4']
JSON_BLOCKS = {
    'chart-data': chart(ITEMS_AR, ITEMS_EN, 15, [0, 5, 10, 15], [
        {'color': '#6D1677', 'values': [4.2, 2.3, 3.5, 4.4]}, {'color': '#B02BC0', 'values': [2.7, 4.6, 2.0, 2.9]}, {'color': '#0E0A8C', 'values': [1.8, 2.1, 2.7, 5.0]}]),
    'chart-data-2': chart(ITEMS_AR, ITEMS_EN, 10, [0, 5, 10], [
        {'color': '#0E0A8C', 'values': [2.2, 2.2, 3.2, 5.0]}, {'color': '#6D1677', 'values': [4.4, 2.5, 3.6, 4.6]}, {'color': '#B02BC0', 'values': [2.6, 4.4, 1.8, 2.9]}]),
    'chart-data-3': chart(['السنة 1', 'السنة 2', 'السنة 3', 'السنة 4'], ['YEAR 1', 'YEAR 2', 'YEAR 3', 'YEAR 4'], 6, [0, 1, 2, 3, 4, 5, 6], [
        {'color': '#0A00DB', 'values': [2, 2, 3, 5]}, {'color': '#6D1677', 'values': [4.3, 2.5, 3.5, 4.5]}, {'color': '#B02BC0', 'values': [2.4, 4.4, 1.8, 2.8]}]),
}

EXTRA_SCRIPT = """  // ----- الرسوم البيانية من كتل chart-data (الفئة الأولى في جهة البداية) -----
  var NS = 'http://www.w3.org/2000/svg';
  function el(n, a) { var e = document.createElementNS(NS, n); for (var k in a) e.setAttribute(k, a[k]); return e; }
  function data(id) { return JSON.parse(document.getElementById(id).textContent); }
  function frame(svg, d, W, H, padS, padE, top, bottom) {
    var X = function (x) { return RTL ? W - x : x; };
    var plotW = W - padS - padE, plotH = H - top - bottom;
    d.ticks.forEach(function (t) {
      var y = top + plotH * (1 - t / d.max);
      svg.appendChild(el('line', { x1: X(padS), y1: y, x2: X(W - padE), y2: y, stroke: t === 0 ? '#BBBBBB' : '#DDDDDD', 'stroke-width': 1 }));
      var tx = el('text', { x: X(padS - 6), y: y + 4, 'text-anchor': RTL ? 'start' : 'end' }); tx.textContent = t; svg.appendChild(tx);
    });
    var n = d.labels.length, slot = plotW / n;
    d.labels.forEach(function (l, i) {
      var tx = el('text', { x: X(padS + slot * (i + 0.5)), y: H - 6, 'text-anchor': 'middle' }); tx.textContent = l; svg.appendChild(tx);
    });
    return { X: X, cx: function (i) { return padS + slot * (i + 0.5); }, y: function (v) { return top + plotH * (1 - v / d.max); } };
  }
  function stackedBars(svgId, dataId) {
    var svg = document.getElementById(svgId), d = data(dataId), f = frame(svg, d, 390, 180, 30, 8, 12, 28);
    d.labels.forEach(function (_, i) {
      var acc = 0;
      d.series.forEach(function (s) {
        var y1 = f.y(acc), y2 = f.y(acc + s.values[i]); acc += s.values[i];
        svg.appendChild(el('rect', { x: f.X(f.cx(i)) - 19, y: y2, width: 38, height: y1 - y2, fill: s.color }));
      });
    });
  }
  function lines(svgId, dataId, W, H, r) {
    var svg = document.getElementById(svgId), d = data(dataId), f = frame(svg, d, W, H, 30, 10, 14, 32);
    d.series.forEach(function (s) {
      var pts = s.values.map(function (v, i) { return f.X(f.cx(i)) + ',' + f.y(v); }).join(' ');
      svg.appendChild(el('polyline', { points: pts, fill: 'none', stroke: s.color, 'stroke-width': r > 6 ? 3 : 2.5, 'stroke-linejoin': 'round' }));
      s.values.forEach(function (v, i) { svg.appendChild(el('circle', { cx: f.X(f.cx(i)), cy: f.y(v), r: r, fill: s.color })); });
    });
  }
  stackedBars('c4a', 'chart-data'); lines('c4b', 'chart-data-2', 390, 180, 5); lines('c15', 'chart-data-3', 710, 305, 9);"""

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'mgmt', 'icon': '🧭',
    'name': 'خارطة طريق التحول بالذكاء الاصطناعي', 'nameEn': 'AI Transformation Roadmap',
    'description': '17 شريحة لخارطة التحول بالذكاء الاصطناعي: الرؤية والنضج والأولويات والجدول الزمني والحوكمة والمؤشرات',
    'descriptionEn': '17 AI transformation roadmap slides: vision, maturity, priorities, timeline, governance and KPIs',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'جدول المحتويات', 'titleEn': 'Table of Contents'},
        {'title': 'الملخص التنفيذي', 'titleEn': 'Executive Summary', 'needsData': True},
        {'title': 'مسوّغات التغيير', 'titleEn': 'The Case for Change', 'needsData': True},
        {'title': 'تقييم نضج الذكاء الاصطناعي الحالي', 'titleEn': 'Current State AI Maturity Assessment'},
        {'title': 'رؤية الذكاء الاصطناعي والمبادئ الأساسية', 'titleEn': 'AI Vision Statement & Core Principles'},
        {'title': 'الركائز الاستراتيجية ونتائج الأعمال', 'titleEn': 'Strategic Pillars & Desired Business Outcomes', 'needsData': True},
        {'title': 'حالات الاستخدام ذات الأولوية وخريطة الأثر', 'titleEn': 'Prioritized AI Use Cases & Impact Mapping'},
        {'title': 'خارطة طريق التحول المرحلي (جانت)', 'titleEn': 'The Phased Transformation Roadmap (Gantt)', 'needsData': True},
        {'title': 'خارطة طريق التحول المرحلي (خط زمني)', 'titleEn': 'The Phased Transformation Roadmap (Timeline)'},
        {'title': 'استراتيجية البيانات والبنية التحتية', 'titleEn': 'Data & Infrastructure Strategy'},
        {'title': 'استراتيجية المواهب والمهارات والثقافة', 'titleEn': 'Talent, Skills & Culture Strategy'},
        {'title': 'مخاطر الحوكمة', 'titleEn': 'Governance Risks'},
        {'title': 'إطار الذكاء الاصطناعي الأخلاقي', 'titleEn': 'Ethical AI Framework'},
        {'title': 'مؤشرات أداء الذكاء الاصطناعي ومتابعتها', 'titleEn': 'AI KPIs and Tracking', 'needsData': True},
        {'title': 'الخطوات التالية', 'titleEn': 'AI Transformation Next Steps'},
        {'title': 'شكرًا وتواصلوا معنا', 'titleEn': 'Thank You / Connect With Us'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص والأرقام فقط؛ أرقام الرسوم في كتل JSON بالمعرّفات chart-data وchart-data-2 وchart-data-3، والأشكال الزخرفية SVG/CSS لا تُمس.',
}
