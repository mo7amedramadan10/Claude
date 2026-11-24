# -*- coding: utf-8 -*-
# نموذج «استراتيجية الأعمال والتخطيط»: سبع عشرة شريحة بالبنفسجي (#4834ED) والأخضر والبرتقالي والسلموني على خلفيات بيضاء وكحلية.
# الحواف المحززة (لوحات تلتقي فيها زاويتان مدوّرتان) تُبنى بقطع مستطيلة متجاورة بزوايا منطقية فتنعكس تلقائيًا في العربية.
KEY = 'sl-business-strategy-planning'
TITLE = {'ar': 'استراتيجية الأعمال والتخطيط', 'en': 'Business Strategy & Planning'}
COMMENT = 'نموذج «استراتيجية الأعمال والتخطيط»: إطار 1280×720 بإحداثيات مطلقة منطقية؛ --accent البنفسجي للكتل، --green و--orange و--salmon للتمييز، --navy للفواصل، --light للوحات الفاتحة. اللوحات المحززة قطع متجاورة بزوايا مدوّرة، والرسم الحلقي في الشريحة 15 من كتلة chart-data.'
VARS = '--ink:#000000; --accent:#4834ED; --navy:#190B86; --green:#6CF170; --orange:#FFC272; --salmon:#FF9E85; --light:#EEF2FB; --dark:#283035; --body:#595959; --muted:#8A8A8A; --bg:#FFFFFF;'


def px(v):
    return '%gpx' % v


def box(x, y, w, h, bg=None, r=(0, 0, 0, 0), cls='', extra=''):
    """مستطيل مطلق؛ r = (أعلى البداية، أعلى النهاية، أسفل النهاية، أسفل البداية) بالمنطق LTR."""
    st = 'top:%s;inset-inline-start:%s;width:%s;height:%s;' % (px(y), px(x), px(w), px(h))
    if bg:
        st += 'background:%s;' % bg
    names = ('border-start-start-radius', 'border-start-end-radius', 'border-end-end-radius', 'border-end-start-radius')
    st += ''.join('%s:%s;' % (n, px(v)) for n, v in zip(names, r) if v)
    return '<div class="b %s" style="%s%s"></div>' % (cls, st, extra)


def T(tok, x, y, cls='', w=None, tag='p', extra='', end=False):
    st = 'top:%s;%s:%s;' % (px(y), 'inset-inline-end' if end else 'inset-inline-start', px(x))
    if w:
        st += 'width:%s;' % px(w)
    return '<%s class="a %s" style="%s%s">§%s§</%s>' % (tag, cls, st, extra, tok, tag)


def svg(x, y, w, h, inner, cls='', vb=None, extra=''):
    return ('<svg class="a %s" style="top:%s;inset-inline-start:%s;width:%s;height:%s;%s" viewBox="%s" aria-hidden="true">%s</svg>'
            % (cls, px(y), px(x), px(w), px(h), extra, vb or '0 0 %g %g' % (w, h), inner))


# ---------- الأيقونات (viewBox 24) ----------
import math


def star_pts(n=10, R=12, r=9.3, cx=12, cy=12, rot=-90):
    pts = []
    for i in range(2 * n):
        a = math.radians(rot + i * 180.0 / n)
        rad = R if i % 2 == 0 else r
        pts.append('%.2f,%.2f' % (cx + rad * math.cos(a), cy + rad * math.sin(a)))
    return ' '.join(pts)


STAR = '<polygon points="%s" fill="currentColor" stroke="currentColor" stroke-width="1.6" stroke-linejoin="round"/>' % star_pts(10, 10.6, 8.2)
SPARK = '<path fill="currentColor" d="M12 0C12.9 7.6 16.4 11.1 24 12C16.4 12.9 12.9 16.4 12 24C11.1 16.4 7.6 12.9 0 12C7.6 11.1 11.1 7.6 12 0Z"/>'
FLOWER = ('<g fill="currentColor"><circle cx="6.5" cy="6.5" r="5.2"/><circle cx="17.5" cy="6.5" r="5.2"/><circle cx="6.5" cy="17.5" r="5.2"/>'
          '<circle cx="17.5" cy="17.5" r="5.2"/><rect x="7" y="7" width="10" height="10" transform="rotate(45 12 12)"/></g>'
          '<g fill="var(--cut,#fff)"><circle cx="12" cy="1" r="2.6"/><circle cx="12" cy="23" r="2.6"/><circle cx="1" cy="12" r="2.6"/><circle cx="23" cy="12" r="2.6"/></g>')
SQUIG = ('<g fill="currentColor"><ellipse cx="6" cy="12.5" rx="3.9" ry="8.6" transform="rotate(32 6 12.5)"/>'
         '<ellipse cx="12" cy="11.5" rx="3.9" ry="8.6" transform="rotate(32 12 11.5)"/><ellipse cx="18" cy="11" rx="3.9" ry="8.4" transform="rotate(32 18 11)"/></g>')
SQ4 = ('<g fill="currentColor"><rect x="0.5" y="0.5" width="10.6" height="10.6" rx="3.6"/><rect x="12.9" y="0.5" width="10.6" height="10.6" rx="3.6"/>'
       '<rect x="0.5" y="12.9" width="10.6" height="10.6" rx="3.6"/><rect x="12.9" y="12.9" width="10.6" height="10.6" rx="3.6"/></g>')
PIE = '<g fill="currentColor"><path d="M11 2.2A10 10 0 1 0 21.8 13H11z"/><path d="M13 0.6V11h10.4A10.4 10.4 0 0 0 13 0.6z"/></g>'
CASE = ('<g fill="currentColor"><path d="M8.5 2.5h7a1.6 1.6 0 0 1 1.6 1.6V6h-2V4.5h-6.2V6h-2V4.1a1.6 1.6 0 0 1 1.6-1.6z"/>'
        '<rect x="0.5" y="6.6" width="23" height="7.4" rx="1.6"/><rect x="0.5" y="15" width="23" height="7.6" rx="1.6"/>'
        '<rect x="10" y="12.4" width="4" height="3.6" rx=".6" fill="#fff"/></g>')
ARROW = '<path fill="currentColor" d="M5 0H24V19L17.6 12.6 4.6 25.6 -1.6 19.4 11.4 6.4Z"/>'
LOGO = '<rect x="4" y="5" width="34" height="34" rx="11" fill="none" stroke="currentColor" stroke-width="6.5" transform="rotate(-12 21 22) skewX(-6)"/>'
MAIL = '<g fill="none" stroke="currentColor" stroke-width="1.8"><rect x="1" y="1" width="33" height="24"/><path d="M1 1l16.5 13L34 1M1 25l12-11M34 25L22 14"/></g>'
PHONE = ('<path fill="currentColor" d="M7.2 1.6l4.4 6.2c.5.7.4 1.6-.2 2.2l-2.5 2.3c1.8 3.6 4.4 6.3 8 8.1l2.3-2.5c.6-.6 1.5-.7 2.2-.2l6.2 4.4c.8.6.9 1.7.3 2.4l-2.4 2.8c-1 1.2-2.7 1.6-4.2 1.1C10.8 25.9 4.6 19.7 2.1 11.2 1.6 9.7 2 8 3.2 7l2.8-2.4c.7-.6 1.8-.5 2.4.3z"/>'
         '<path fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" d="M19 2.5a10 10 0 0 1 9.5 9.5M18.5 7a5.5 5.5 0 0 1 5 5"/>')
BUL = '<svg class="bi" viewBox="0 0 24 24" aria-hidden="true">' + SPARK + '</svg>'


def icon(x, y, w, h, body, color, cls='', vb='0 0 24 24'):
    return svg(x, y, w, h, body, cls, vb, 'color:%s;' % color)


# علامة الاقتباس المحددة (زخرفة)
QUOTE_PATH = 'M1.5 1.5H51V60C51 84 37 99 13 104C26 95 32 84 32.5 70H1.5Z'


def quote(x, y, stroke):
    g = '<path d="%s" fill="none" stroke="url(#%s)" stroke-width="1.6"/>' % (QUOTE_PATH, stroke)
    return svg(x, y, 117, 106, '<g>' + g + '</g><g transform="translate(62 0)">' + g + '</g>', 'quote')


def bullets(x, y, toks, w=270, cls='wh'):
    lis = ''.join('<li>%s<span>§%s§</span></li>' % (BUL, t) for t in toks)
    return '<ul class="a bl %s" style="top:%s;inset-inline-start:%s;width:%s;">%s</ul>' % (cls, px(y), px(x), px(w), lis)


def notched_v(x, y_top, w, cuts, y_end, color, r_n=40, tl=0, tr=0, bl_last=0, br_last=0):
    """لوحة رأسية بحزّات على حافة البداية عند الارتفاعات cuts (نصف قطر r_n)."""
    ys = [y_top] + cuts + [y_end]
    out = []
    for i in range(len(ys) - 1):
        a, b = ys[i], ys[i + 1]
        first, last = i == 0, i == len(ys) - 2
        r = (tl if first else r_n, tr if first else 0, br_last if last else 0, bl_last if last else r_n)
        out.append(box(x, a, w, b - a, color, r))
    return ''.join(out)


def grid(cls=''):
    return '<div class="grid %s" aria-hidden="true"></div>' % cls


def head(title_tok, x=68, y=69, cls='', sub_y=149, sub_cls='sub'):
    return T(title_tok, x, y, 'ttl ' + cls, tag='h1') + T('SUB', x, sub_y, sub_cls)


STYLE = r"""  .frame h1, .frame h2, .frame p, .frame ul { margin: 0; }
  .a, .b { position: absolute; }
  .a { margin: 0; }
  .wh { color: #fff; }
  .ttl { font-size: 56px; line-height: 62px; font-weight: 700; letter-spacing: -.6px; color: var(--ink); white-space: nowrap; }
  .sub { font-size: 22px; line-height: 28px; color: var(--muted); }
  .subw { font-size: 24px; line-height: 30px; color: #fff; }
  .p18 { font-size: 18px; line-height: 25.5px; color: var(--body); }
  .p18w { font-size: 18px; line-height: 25.5px; color: rgb(255 255 255 / .9); }
  .h26 { font-size: 26px; line-height: 32px; font-weight: 700; color: var(--dark); }
  .h22 { font-size: 22px; line-height: 28px; font-weight: 700; color: var(--dark); }
  .grid { position: absolute; inset: 0; background: repeating-linear-gradient(90deg, transparent 0 115.4px, var(--gl, #F1F2F5) 115.4px 116.4px); }
  .quote { overflow: visible; }
  .qt { font-size: 30px; line-height: 39px; font-weight: 700; color: var(--dark); }
  .bl { list-style: none; padding: 0; display: flex; flex-direction: column; gap: 9px; font-size: 18px; line-height: 25px; }
  .bl li { position: relative; padding-inline-start: 30px; }
  .bl .bi { position: absolute; inset-inline-start: 2px; top: 5px; width: 14px; height: 14px; color: var(--green); }
  .bl.dk li { color: var(--body); }
  .pill { display: grid; place-items: center; border-radius: 999px; font-size: 18px; font-weight: 700; letter-spacing: .3px; }
  .frame .wh { color: #fff; }
  html[lang="ar"] .ttl { letter-spacing: 0; }
  html[lang="ar"] .p18, html[lang="ar"] .p18w, html[lang="ar"] .bl, html[lang="ar"] .s11 .pw { font-size: 18px; }
  html[dir="rtl"] .flip, html[dir="rtl"] .quote { transform: scaleX(-1); }
  /* شعار الشركة */
  .logo-t { font-size: 16px; line-height: 23px; letter-spacing: 2.4px; text-transform: uppercase; }
  html[lang="ar"] .logo-t { letter-spacing: 0; font-size: 17px; font-weight: 500; }

  /* 1) الغلاف */
  .s1 { background: var(--light); }
  .s1 .card { background: linear-gradient(90deg, #F3F6FC, #FFFFFF 48%); }
  html[dir="rtl"] .s1 .card { background: linear-gradient(270deg, #F3F6FC, #FFFFFF 48%); }
  .s1 .cv { font-size: 76px; line-height: 77px; font-weight: 700; letter-spacing: -1px; white-space: nowrap; }
  .s1 .pt { font-size: 30px; line-height: 36px; letter-spacing: 6.6px; color: var(--accent); white-space: nowrap; }
  html[lang="ar"] .s1 .pt { letter-spacing: 2px; }
  .s1 .tg { font-size: 20px; line-height: 26px; color: #5F6368; }

  /* 2) نظرة تنفيذية */
  .s2, .s4, .s7, .s11, .s12, .s13, .s16 { background: var(--accent); }
  .s2 .grid, .s12 .grid, .s13 .grid { --gl: rgb(255 255 255 / .07); }
  .s2 .big { font-size: 124px; line-height: 140px; font-weight: 700; letter-spacing: -5px; color: #fff; white-space: nowrap; }
  .kv { font-size: 44px; line-height: 50px; font-weight: 700; color: var(--accent); letter-spacing: -.5px; }
  .s2 .fd { font-size: 30px; line-height: 36px; font-weight: 700; color: var(--dark); }

  /* 3 و6) رأس رمادي بعلامة اقتباس */
  .gray { background: linear-gradient(180deg, #F1F1F2 0%, #F4F4F5 45%, #FFFFFF 100%); }
  .s3 .crd { position: absolute; top: 272px; width: 348px; height: 220px; border-radius: 26px; overflow: hidden; }
  .s3 .crd .n { position: absolute; top: 32px; inset-inline-start: 36px; font-size: 30px; line-height: 36px; font-weight: 700; }
  .s3 .crd h2 { position: absolute; top: 112px; inset-inline-start: 36px; font-size: 26px; line-height: 34px; font-weight: 700; color: var(--dark); }
  .s3 .deco { position: absolute; top: 82px; inset-inline-start: 203px; width: 160px; height: 160px; }

  /* 4) التحليل الرباعي */
  .s4 .dv { border-inline-start: 1.5px dashed #DCDCDC; }
  .s4 .dh { border-top: 1.5px dashed #DCDCDC; }
  html[lang="ar"] .lt { font-size: 170px; margin-top: 34px; }
  .lt { font-size: 200px; line-height: 200px; font-weight: 700; color: transparent; -webkit-text-stroke: 2px var(--c); -webkit-mask-image: linear-gradient(180deg, #000 25%, transparent 92%); mask-image: linear-gradient(180deg, #000 25%, transparent 92%); }

  /* 5) الجدول الزمني الأفقي */
  .yr { font-size: 30px; line-height: 36px; font-weight: 700; color: #fff; }
  .dot { position: absolute; width: 20px; height: 20px; border-radius: 50%; background: var(--accent); border: 3.5px solid #fff; }
  .vl { position: absolute; width: 4px; background: linear-gradient(180deg, var(--accent), rgb(72 52 237 / 0)); }
  .s5 .mh { font-size: 23px; line-height: 29px; font-weight: 700; color: #000; }

  /* 6) خارطة النمو */
  .s6 .v6 { font-size: 60px; line-height: 70px; font-weight: 700; letter-spacing: -4px; color: #000; white-space: nowrap; }
  .s6 .g6 { font-size: 20px; line-height: 25px; font-weight: 700; color: #000; white-space: nowrap; }
  .s6 .st { font-size: 22px; line-height: 28px; font-weight: 700; color: #fff; white-space: nowrap; }
  .s6 .sh { font-size: 20px; line-height: 25px; font-weight: 700; color: #000; }

  /* 7) خارطة طريق الأعمال */
  .s7 .grid { --gl: rgb(255 255 255 / .08); -webkit-mask-image: linear-gradient(180deg, #000, transparent 45%); mask-image: linear-gradient(180deg, #000, transparent 45%); }
  .s7 .qw { font-size: 30px; line-height: 39px; color: #fff; }
  .s7 .sl { font-size: 30px; line-height: 36px; font-weight: 700; }
  .s7 .ln { background: linear-gradient(90deg, rgb(72 52 237 / 0), #DCD8FB 8%, #A79DF4 38%, #6F60F0 55%, var(--accent) 62%); }
  html[dir="rtl"] .s7 .ln { background: linear-gradient(270deg, rgb(72 52 237 / 0), #DCD8FB 8%, #A79DF4 38%, #6F60F0 55%, var(--accent) 62%); }
  .s7 .pd { position: absolute; width: 14px; height: 14px; border-radius: 50%; background: var(--accent); }

  /* 8) فاصل الفصل */
  .s8 { background: radial-gradient(circle at 100% 0%, #4936EE 0%, #2C1BB2 32%, #1A0C8C 62%, #190B86 100%); }
  html[dir="rtl"] .s8 { background: radial-gradient(circle at 0% 0%, #4936EE 0%, #2C1BB2 32%, #1A0C8C 62%, #190B86 100%); }
  .s8 .grid { --gl: rgb(255 255 255 / .16); }
  .s8 .num { font-size: 600px; line-height: 600px; font-weight: 700; letter-spacing: -72px; color: var(--accent); white-space: nowrap; text-shadow: -26px 0 50px rgb(18 8 100 / .45); }
  html[dir="rtl"] .s8 .num { text-shadow: 26px 0 50px rgb(18 8 100 / .45); }
  .s8 .band { background: #190B86; }
  .s8 .hl { background: linear-gradient(90deg, rgb(120 104 245 / 0), #7B6DF3 35%, #9184F5); }
  html[dir="rtl"] .s8 .hl { background: linear-gradient(270deg, rgb(120 104 245 / 0), #7B6DF3 35%, #9184F5); }
  .s8 .ch { font-size: 100px; line-height: 100px; font-weight: 700; letter-spacing: -1.5px; color: #fff; white-space: nowrap; }
  .s8 .cs { font-size: 18px; line-height: 24px; color: #DAD7F7; white-space: nowrap; }

  /* 9) الدرج */
  .s9 .bar { background: var(--accent); }
  .s9 .gl { background: var(--green); }
  .s9 .lb { font-size: 20px; line-height: 26px; font-weight: 700; color: #000; white-space: nowrap; }
  .s9 .bv { font-size: 40px; line-height: 46px; font-weight: 700; color: var(--green); white-space: nowrap; letter-spacing: -.5px; }
  .s9 .bt { font-size: 18px; line-height: 24px; font-weight: 700; color: #fff; }
  .s9 .bp { font-size: 16px; line-height: 23px; color: #DEDBFB; }
  .s9 .pc { font-size: 50px; line-height: 56px; font-weight: 700; color: var(--accent); }
  .s9 .rg { font-size: 22px; line-height: 29px; font-weight: 700; color: #000; }

  /* 10) المربعات الاستراتيجية */
  .s10 .dv { border-inline-start: 1.5px dashed #DEDEDE; }
  .s10 .dh { border-top: 1.5px dashed #DEDEDE; }

  /* 11) أولويات الأعمال */
  .s11 .grid { --gl: rgb(255 255 255 / .13); -webkit-mask-image: linear-gradient(180deg, #000 45%, transparent 52%); mask-image: linear-gradient(180deg, #000 45%, transparent 52%); }
  .s11 .big { font-size: 128px; line-height: 130px; font-weight: 700; letter-spacing: -2px; color: var(--dark); white-space: nowrap; }
  .s11 .pw { font-size: 18px; line-height: 25.5px; color: #DCD9FB; }

  /* 12 و13) الجدول الزمني الرأسي */
  .vy { font-size: 28px; line-height: 34px; font-weight: 700; color: var(--accent); text-align: end; }
  .vd { font-size: 18px; line-height: 24px; color: var(--body); text-align: end; }
  .vline { background: linear-gradient(180deg, var(--orange), rgb(255 194 114 / .55) 55%, rgb(255 194 114 / 0)); }

  /* 14) السوق المستهدف */
  .s14 .grid { -webkit-mask-image: linear-gradient(180deg, transparent 26%, #000 34%); mask-image: linear-gradient(180deg, transparent 26%, #000 34%); }
  .s14 .sv { font-size: 44px; line-height: 50px; font-weight: 700; color: var(--accent); letter-spacing: -.5px; }
  .s14 .dh { border-top: 1.5px dashed #DDDDDD; }
  .circ { position: absolute; border-radius: 50%; }
  .s14 .ab { font-size: 20px; line-height: 26px; font-weight: 700; }
  .s14 .mv { font-size: 48px; line-height: 54px; font-weight: 700; letter-spacing: -1.5px; white-space: nowrap; }
  .s14 .mt { font-size: 16px; line-height: 23px; }
  html[lang="ar"] .s14 .mv { font-size: 40px; }

  /* 15) تقرير الأعمال (رسم حلقي من chart-data) */
  .s15 .grid, .s16 .grid { --gl: #F6F7F9; }
  .s15 .donut { position: absolute; top: 82px; inset-inline-start: 648px; width: 556px; height: 556px; }
  .s15 .donut svg { position: absolute; inset: 0; width: 556px; height: 556px; }
  html[dir="rtl"] .s15 .donut svg { transform: scaleX(-1); }
  .s15 .dl { position: absolute; width: 220px; text-align: center; }
  .s15 .dl span { display: block; font-size: 18px; line-height: 24px; color: var(--body); }
  .s15 .dl b { display: block; font-size: 54px; line-height: 58px; font-weight: 700; color: #000; letter-spacing: -1px; }
  .s15 .bm { font-size: 26px; line-height: 32px; font-weight: 700; color: var(--accent); }

  /* 16) جبل الجليد */
  .s16 .ice { object-fit: cover; }
  .s16 .ct { text-align: center; }
  .s16 .vb { font-size: 30px; line-height: 40px; color: var(--green); }

  /* 17) الختام */
  .s17 { background: radial-gradient(ellipse 60% 70% at 100% 15%, rgb(106 88 240 / .55), rgb(106 88 240 / 0) 70%), #190B86; }
  html[dir="rtl"] .s17 { background: radial-gradient(ellipse 60% 70% at 0% 15%, rgb(106 88 240 / .55), rgb(106 88 240 / 0) 70%), #190B86; }
  .s17 .cl { font-size: 96px; line-height: 103px; font-weight: 400; letter-spacing: -1.5px; color: #fff; white-space: nowrap; }
  html[lang="ar"] .s17 .cl { font-weight: 500; }
  .s17 .btn { border-radius: 26px; background: var(--green); color: #000; font-size: 26px; font-weight: 700; display: grid; place-items: center; }
  .s17 .hl { background: linear-gradient(90deg, rgb(120 104 245 / .25), #8073F2 40%, rgb(120 104 245 / .35)); }
  .s17 .ct { font-size: 18px; line-height: 24px; color: #D6D3F5; white-space: nowrap; }"""

GLOBAL_DEFS = ('<defs>'
               '<linearGradient id="qg" x1="0" y1="1" x2="1" y2="0"><stop offset="0" stop-color="#6E5FF0" stop-opacity="0"/><stop offset=".55" stop-color="#8F83F3" stop-opacity=".6"/><stop offset="1" stop-color="#5A47EF"/></linearGradient>'
               '<linearGradient id="qw" x1="0" y1="1" x2="1" y2="0"><stop offset="0" stop-color="#fff" stop-opacity="0"/><stop offset=".55" stop-color="#fff" stop-opacity=".25"/><stop offset="1" stop-color="#fff" stop-opacity=".55"/></linearGradient>'
               '</defs>')

# ---------- الشرائح ----------
# 1) الغلاف
BARS1 = [(511, 110, '#EAE8FD'), (617, 150, '#D3CDFA'), (722, 240, '#BDB5F7'), (828, 390, '#A79DF4'), (933, 370, '#8E81F2'), (1039, 420, '#6E5EF0'), (1143, 290, '#5440EE')]
s1 = (box(70, 43, 1164, 633, None, (0, 30, 26, 0), 'card')
      + ''.join(box(x, 43, 91, h, 'linear-gradient(180deg,%s,%s00)' % (c, c), (0, 30 if i == 6 else 0, 0, 0)) for i, (x, h, c) in enumerate(BARS1))
      + icon(105, 104, 48, 50, LOGO, 'var(--accent)', 'flip', '0 0 42 44')
      + '<p class="a logo-t" style="top:107px;inset-inline-start:168px;">§LOGO1§<br>§LOGO2§</p>'
      + T('COVER', 101, 314, 'cv', tag='h1')
      + T('PRES', 103, 507, 'pt')
      + T('TAG', 103, 614, 'tg')
      + T('DATE', 102, 614, 'tg', end=True))

# 2) نظرة تنفيذية
s2 = (grid()
      + head('EXO', 64, 69, 'wh', 206, 'subw')
      + T('BIG2', 70, 370, 'big')
      + T('ATH', 71, 514, 'h22 wh')
      + T('P2A', 72, 590, 'p18w', 450)
      + notched_v(705, 46, 535, [143, 548], 760, '#fff', 40, 24, 22)
      + icon(797, 118, 48, 48, PIE, 'var(--green)', 'flip')
      + T('K1', 868, 116, 'kv') + T('K1T', 795, 183, 'h22') + T('PS1', 795, 217, 'p18')
      + icon(797, 288, 50, 46, CASE, 'var(--green)')
      + T('K2', 868, 285, 'kv') + T('K2T', 795, 352, 'h22') + T('PS1', 795, 386, 'p18')
      + box(784, 467, 375, 4, 'var(--accent)')
      + T('FD', 795, 528, 'fd') + T('P2B', 795, 590, 'p18', 385))

# 3) الإطار الاستراتيجي
DECO3 = {
    'star': '<svg class="deco" viewBox="0 0 160 160" style="color:#DCFBDB;top:66px;inset-inline-start:200px;width:190px;height:190px" aria-hidden="true"><polygon points="%s" fill="currentColor" stroke="currentColor" stroke-width="9" stroke-linejoin="round"/></svg>' % star_pts(11, 76, 56, 80, 80, -80),
    'sq': '<svg class="deco" viewBox="0 0 160 160" style="color:#FFEFDD" aria-hidden="true"><g fill="currentColor"><rect x="0" y="0" width="80" height="80" rx="24"/><rect x="95" y="0" width="80" height="80" rx="24"/><rect x="0" y="95" width="80" height="80" rx="24"/><rect x="95" y="95" width="80" height="80" rx="24"/></g></svg>',
    'fire': '<svg class="deco flip" viewBox="0 0 160 160" style="color:#FFE6DF" aria-hidden="true"><path fill="currentColor" d="M88 0C96 34 128 50 142 84 157 120 140 160 140 160H-2S-18 128-4 98C4 82 16 70 22 56 30 72 38 80 46 84 46 50 62 22 88 0Z"/><path fill="#fff" d="M70 96C78 112 94 120 96 140 97 150 94 160 94 160H44S40 150 42 140C46 122 60 116 70 96Z"/></svg>',
}


def card3(i, x, num_color, deco):
    return ('<div class="crd" style="inset-inline-start:%dpx">' % x
            + box(0, 0, 262, 220, '#fff', (0, 30, 0, 0)) + box(262, 0, 86, 220, '#fff', (30, 0, 0, 0))
            + DECO3[deco]
            + '<p class="n" style="color:%s">§N%d§</p><h2>§C%d§</h2></div>' % (num_color, i, i))


s3 = (box(770, 0, 510, 720, None, cls='gray')
      + head('T3')
      + quote(1120, 33, 'qg')
      + T('Q3', 855, 80, 'qt', 400)
      + box(45, 238, 1190, 520, 'var(--accent)', (40, 40, 0, 0))
      + card3(1, 76, 'var(--green)', 'star') + card3(2, 465, 'var(--orange)', 'sq') + card3(3, 853, 'var(--salmon)', 'fire')
      + T('P_LONG', 99, 534, 'p18w', 300) + T('P_LONG', 488, 534, 'p18w', 300) + T('P_LONG', 877, 534, 'p18w', 300))


def leftcol(title, ty, para_y, bul_y, tx=68):
    return (T(title, tx, ty, 'ttl wh', tag='h1') + T('SUB', 64, ty + 137, 'subw')
            + T('PS', 64, para_y, 'p18w', 310) + bullets(64, bul_y, ['B1', 'B2', 'B3'], 290))


# 4) التحليل الرباعي
def swot(i, x, y, icx, icy, color, lx, ly):
    return ('<span class="a lt" aria-hidden="true" style="top:%dpx;inset-inline-start:%dpx;--c:%s">§L%d§</span>' % (ly, lx, color[1], i)
            + icon(icx, icy, 64, 64, STAR, color[0])
            + T('SW%d' % i, x, y, 'h26') + T('P_LONG', x, y + 46, 'p18', 300))


s4 = (box(428, 0, 852, 695, '#fff', (0, 0, 0, 36)) + box(380, 40, 60, 120, '#fff')
      + box(0, 0, 428, 97, 'var(--accent)', (0, 0, 30, 0)) + box(0, 97, 428, 623, 'var(--accent)', (0, 30, 0, 0))
      + leftcol('T4', 69, 402, 492)
      + box(855, 52, 2, 616, None, cls='dv') + box(513, 349, 702, 2, None, cls='dh')
      + swot(1, 518, 185, 515, 76, ('var(--accent)', '#8C80F2'), 540, 48)
      + swot(2, 917, 185, 914, 78, ('var(--green)', '#A8F0AA'), 935, 48)
      + swot(3, 518, 512, 515, 406, ('var(--orange)', '#FFD99A'), 538, 359)
      + swot(4, 917, 512, 914, 406, ('var(--salmon)', '#FFBCA9'), 935, 362))

# 5) الجدول الزمني لتطوير السوق
XS5 = [65, 371, 678, 984]
s5 = (grid()
      + head('T5')
      + box(0, 234, 1177, 13, 'var(--green)') + box(0, 247, 1215, 96, 'var(--accent)')
      + svg(1215, 247, 48, 96, '<path d="M0 0A48 48 0 0 0 48 48A48 48 0 0 0 0 96Z" fill="var(--accent)"/>', 'flip')
      + ''.join(T('Y%d' % (i + 1), x - 9, 281, 'yr') for i, x in enumerate(XS5))
      + ''.join('<div class="vl" style="top:351px;inset-inline-start:%dpx;height:300px"></div><div class="dot" style="top:331px;inset-inline-start:%dpx"></div>' % (x - 2, x - 10) for x in XS5)
      + ''.join(T('M%d' % (i + 1), x + 33, 390, 'mh', 235) + T('PM', x + 33, 498, 'p18', 210) for i, x in enumerate(XS5)))

# 6) خارطة النمو
SEG6 = [(45, 279, '#B6AFF2'), (279, 513, '#9286F2'), (513, 980, '#4834ED'), (980, 1229, '#190B84')]


def seg6(x0, x1, color, last):
    w = (x1 - x0) if last else (x1 + 32 - x0)
    return box(x0, 399, w, 32, color, (0, 0, 0, 32)) + box(x0, 431, w, 32, color, (32, 0, 0, 0))


XD6 = [55, 299, 535, 1002]
XH6 = [86, 328, 570, 1037]
ICO6 = [(90, 510, 46, 40, SQUIG), (338, 510, 38, 38, FLOWER), (575, 508, 42, 42, SQ4), (1038, 507, 46, 46, SPARK)]
s6 = (box(770, 0, 510, 720, None, cls='gray')
      + head('T6')
      + quote(1120, 33, 'qg')
      + T('Q6', 855, 80, 'qt', 400)
      + box(45, 238, 1184, 165, 'var(--green)', (20, 20, 0, 0))
      + icon(103, 295, 50, 50, ARROW, '#000', 'flip', '-2 -2 28 28')
      + T('V6', 190, 285, 'v6') + T('G6', 407, 294, 'g6') + T('P6', 565, 296, 'p18', 620, extra='color:#222;')
      + ''.join(seg6(a, b, c, i == 3) for i, (a, b, c) in enumerate(SEG6))
      + svg(1229, 399, 32, 64, '<path d="M0 0A32 32 0 0 0 32 32A32 32 0 0 0 0 64Z" fill="#190B84"/>', 'flip')
      + ''.join(T('ST%d' % (i + 1), x, 419, 'st') for i, x in enumerate([100, 342, 583, 1051]))
      + ''.join('<div class="vl" style="top:473px;inset-inline-start:%dpx;height:210px"></div><div class="dot" style="top:453px;inset-inline-start:%dpx"></div>' % (x - 2, x - 10) for x in XD6)
      + ''.join(icon(x, y, w, h, b, '#D9D9D9') for x, y, w, h, b in ICO6)
      + ''.join(T('H6_%d' % (i + 1), x, 577, 'sh', 220) for i, x in enumerate(XH6)))

# 7) خارطة طريق الأعمال
XS7 = [131, 448, 766]
COL7 = ['var(--green)', 'var(--orange)', 'var(--salmon)']
ICO7 = [(133, 491, 46, 38, SQUIG), (447, 491, 38, 38, FLOWER), (768, 488, 42, 42, SQ4)]
s7 = (grid()
      + box(54, 299, 1066, 421, '#fff', (50, 40, 0, 0)) + box(1120, 299, 160, 421, '#fff', (40, 0, 0, 0))
      + T('T7', 64, 69, 'ttl wh', tag='h1') + T('SUB', 64, 206, 'subw')
      + quote(1120, 33, 'qw')
      + T('Q7', 768, 81, 'qw', 460)
      + ''.join(T('SG%d' % (i + 1), x, 366, 'sl', extra='color:%s;' % COL7[i]) for i, x in enumerate(XS7))
      + box(70, 449, 1210, 3, None, cls='ln')
      + ''.join('<div class="pd" style="top:444px;inset-inline-start:%dpx"></div>' % (x - 7) for x in [141, 456, 776])
      + ''.join(icon(x, y, w, h, b, COL7[i]) for i, (x, y, w, h, b) in enumerate(ICO7))
      + ''.join(T('N7_%d' % (i + 1), x, 569, 'h26') + T('P7', x, 611, 'p18', 270) for i, x in enumerate(XS7)))

# 8) فاصل الفصل
s8 = (grid()
      + T('CHN', 671, 48, 'num', tag='span', extra='display:block;')
      + box(0, 568, 1280, 152, None, cls='band') + box(0, 567, 1280, 2, None, cls='hl')
      + '<div class="a pill" style="top:118px;inset-inline-start:117px;width:91px;height:53px;background:var(--green);color:#000;font-size:24px">02</div>'
      + T('CH', 108, 305, 'ch', tag='h1')
      + T('CHS', 109, 619, 'cs') + T('ATH2', 55, 619, 'cs', end=True))

# 9) الدرج
BAR9 = [(345, 406, 389, 359), (568, 312, 296, 267), (791, 239, 223, 193), (1014, 146, 131, 103)]
s9 = (grid()
      + head('T9')
      + box(0, 224, 700, 496, 'var(--light)', (0, 40, 0, 0))
      + T('PC9', 64, 402, 'pc') + T('RG9', 64, 466, 'rg') + T('PM', 64, 545, 'p18', 215)
      + ''.join(box(x, top, 212, 720 - top, None, cls='bar') + box(x, gy, 212, 4, None, cls='gl')
                + T('AT', x + 10, ly, 'lb') + T('BV%d' % (i + 1), x + 29, 536, 'bv') + T('AT', x + 30, 593, 'bt')
                + T('PS1', x + 28, 637, 'bp', 175) for i, (x, top, gy, ly) in enumerate(BAR9)))

# 10) المربعات الاستراتيجية
PILLS10 = [(849, 65, 92, 'var(--accent)', '#fff', 'HI'), (582, 340, 91, 'var(--orange)', '#000', 'LO'),
           (1129, 340, 91, 'var(--green)', '#000', 'HI'), (849, 634, 92, 'var(--salmon)', '#fff', 'LO')]
s10 = (box(0, 0, 517, 105, 'var(--light)', (0, 0, 30, 0)) + box(0, 105, 517, 615, 'var(--light)', (0, 30, 0, 0))
       + box(0, 517, 485, 75, 'var(--accent)', (0, 40, 30, 0)) + box(0, 592, 485, 128, 'var(--accent)', (0, 30, 0, 0))
       + T('T10', 68, 69, 'ttl', tag='h1') + T('SUB', 68, 208, 'sub')
       + T('P_LONG4', 64, 344, 'p18', 262)
       + icon(72, 578, 43, 43, STAR, 'var(--green)')
       + T('BX', 143, 580, 'h26 wh')
       + box(893, 104, 2, 530, None, cls='dv') + box(673, 360, 456, 2, None, cls='dh')
       + ''.join('<div class="a pill" style="top:%dpx;inset-inline-start:%dpx;width:%dpx;height:39px;background:%s;color:%s">§%s§</div>' % (y, x, w, bg, fg, t) for x, y, w, bg, fg, t in PILLS10)
       + T('QW', 598, 162, 'h26') + T('P_MID', 598, 208, 'p18', 250)
       + T('SB', 978, 162, 'h26') + T('P_MID', 978, 208, 'p18', 250)
       + T('OS', 601, 462, 'h26') + T('P_MID', 601, 508, 'p18', 250)
       + T('RC', 971, 462, 'h26') + T('P_MID', 971, 508, 'p18', 250))

# 11) أولويات الأعمال
s11 = (grid()
       + T('T11', 68, 78, 'ttl wh', tag='h1') + T('SUB', 68, 218, 'subw', extra='color:#D4D0FB;')
       + icon(482, 94, 28, 28, STAR, 'var(--green)') + T('CE', 524, 95, 'h22 wh') + T('P_LONG', 524, 138, 'pw', 300)
       + icon(864, 94, 28, 28, STAR, 'var(--green)') + T('CD', 907, 95, 'h22 wh') + T('P_LONG', 907, 138, 'pw', 300)
       + box(69, 366, 739, 282, '#fff', (24, 24, 24, 24)) + box(808, 366, 403, 282, '#fff', (24, 24, 24, 24))
       + T('BIG11', 122, 410, 'big') + T('ATH', 124, 580, 'h22', extra='color:var(--accent);')
       + bullets(874, 434, ['B1', 'B2', 'B3'], 270, 'dk'))

# 12 و13) الجدول الزمني الرأسي


def vtl(first_year, cuts, card_y0, card_y1, dot_y, row0, rad):
    rows = ''
    for i in range(5):
        y = row0 + i * 121.75
        rows += (T('VY%d' % (first_year + i), 556, round(y), 'vy', 100)
                 + T('VD', 556, round(y + 40), 'vd', 100)
                 + T('PV', 763, round(y + 7), 'p18', 425))
    return (grid()
            + leftcol('T12', 69, 393, 484, 64)
            + notched_v(515, card_y0, 725, cuts, card_y1, '#fff', 40, rad[0], rad[1], rad[2], rad[3])
            + '<div class="dot" style="top:%dpx;inset-inline-start:708px;width:16px;height:16px;border:0;background:var(--orange)"></div>' % (dot_y - 8)
            + box(715, dot_y + 8, 3, 560, None, cls='vline')
            + rows)


s12 = vtl(2028, [143, 548], 46, 760, 95, 106, (24, 22, 0, 0))
s13 = vtl(2033, [172, 578], -40, 673, 49, 60, (0, 0, 22, 22))

# 14) السوق المستهدف
s14 = (grid()
       + head('T14', 64)
       + T('P14', 64, 209, 'p18', 520)
       + T('SV1', 64, 332, 'sv') + T('PT', 64, 392, 'p18', 285)
       + T('SV2', 425, 332, 'sv') + T('PT', 425, 392, 'p18', 285)
       + box(54, 487, 636, 2, None, cls='dh')
       + T('SV3', 64, 533, 'sv') + T('PT', 64, 592, 'p18', 285)
       + T('SV4', 425, 533, 'sv') + T('PT', 425, 592, 'p18', 285)
       + '<div class="circ" style="top:-70px;inset-inline-start:770px;width:800px;height:800px;background:var(--accent)"></div>'
       + '<div class="circ" style="top:184px;inset-inline-start:802px;width:476px;height:476px;background:var(--orange)"></div>'
       + '<div class="circ" style="top:360px;inset-inline-start:838px;width:280px;height:280px;background:var(--green)"></div>'
       + T('TAM', 992, 51, 'ab wh') + T('TAMV', 1068, 49, 'mv wh') + T('PC', 1068, 104, 'mt wh', 200)
       + T('SAM', 939, 240, 'ab') + T('SAMV', 1004, 236, 'mv') + T('PC', 1006, 296, 'mt', 200)
       + T('SOM', 904, 440, 'ab') + T('SOMV', 898, 471, 'mv') + T('PC', 898, 528, 'mt', 200))

# 15) تقرير الأعمال
s15 = (grid()
       + T('T15', 64, 69, 'ttl', tag='h1') + T('SUB', 64, 220, 'sub')
       + T('P15', 64, 338, 'p18', 450)
       + box(0, 517, 485, 75, 'var(--light)', (0, 40, 30, 0)) + box(0, 592, 485, 128, 'var(--light)', (0, 30, 0, 0))
       + icon(72, 586, 43, 43, STAR, 'var(--green)') + T('BM', 143, 587, 'bm')
       + box(557, 0, 723, 97, 'var(--accent)', (0, 0, 0, 30))
       + box(557, 97, 608, 598, 'var(--accent)', (30, 0, 30, 30)) + box(1165, 97, 115, 598, 'var(--accent)', (0, 0, 0, 30))
       + '<div class="donut" role="img" aria-label="§DONUT_ARIA§"></div>')

# 16) جبل الجليد
s16 = (box(0, 0, 177, 421, '#fff', (0, 0, 45, 0)) + box(177, 0, 845, 421, '#fff', (0, 0, 45, 45)) + box(1022, 0, 258, 421, '#fff', (0, 0, 0, 45))
       + '<img class="a ice" src="/templates/freeform/sl-business-strategy-planning/assets/iceberg.jpg" alt="§ICE_ALT§" style="top:190px;inset-inline-start:388px;width:504px;height:492px">'
       + T('T16', 0, 69, 'ttl ct', 1280, tag='h1') + T('SUB', 0, 149, 'sub ct', 1280)
       + '<div class="a pill" style="top:164px;inset-inline-start:146px;width:92px;height:39px;background:var(--orange);color:#000">§HI§</div>'
       + T('PC16', 40, 229, 'h26 ct', 300) + T('P_MID', 65, 276, 'p18 ct', 250)
       + '<div class="a pill" style="top:168px;inset-inline-start:1046px;width:91px;height:39px;background:var(--green);color:#000">§HI§</div>'
       + T('CV16', 939, 233, 'h26 ct', 300) + T('P_MID', 964, 280, 'p18 ct', 250)
       + svg(72, 462, 38, 38, FLOWER, '', '0 0 24 24', 'color:#fff;--cut:var(--accent);')
       + T('VB', 76, 537, 'vb', 430)
       + icon(860, 503, 30, 30, SPARK, 'var(--green)')
       + T('CU', 914, 505, 'h22 wh') + T('P_LONG', 914, 552, 'p18w', 300, extra='color:#DCDAFB;'))

# 17) الختام
BARS17 = [(557, 110, .10), (663, 150, .16), (768, 240, .20), (874, 500, .26), (980, 450, .32), (1085, 420, .40), (1190, 290, .55)]
s17 = (''.join(box(x, 0, 91 if i < 6 else 90, h, 'linear-gradient(180deg,rgb(130 112 250 / %g),rgb(130 112 250 / 0))' % a) for i, (x, h, a) in enumerate(BARS17))
       + icon(114, 84, 46, 48, LOGO, 'var(--green)', 'flip', '0 0 42 44')
       + '<p class="a logo-t wh" style="top:86px;inset-inline-start:177px;">§LOGO1§<br>§LOGO2§</p>'
       + T('CL', 113, 210, 'cl', tag='h1')
       + '<div class="a btn" style="top:459px;inset-inline-start:931px;width:242px;height:52px">§GIT§</div>'
       + box(0, 574, 1280, 2, None, cls='hl')
       + icon(119, 630, 35, 26, MAIL, 'var(--green)', '', '0 0 35 26')
       + T('EM', 175, 631, 'ct')
       + icon(453, 626, 33, 33, PHONE, 'var(--green)', 'flip', '0 0 30 30')
       + T('PH', 503, 631, 'ct'))

SLIDES = [s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13, s14, s15, s16, s17]

JSON_BLOCKS = {'chart-data': {
    'en': {'items': [{'label': 'Add text here.', 'value': 40}, {'label': 'Add text here.', 'value': 50}, {'label': 'Add text here.', 'value': 40}]},
    'ar': {'items': [{'label': 'أضف النص هنا.', 'value': 40}, {'label': 'أضف النص هنا.', 'value': 50}, {'label': 'أضف النص هنا.', 'value': 40}]},
}}

EXTRA_SCRIPT = r"""  // الرسم الحلقي في شريحة تقرير الأعمال: الشرائح متناسبة مع القيم، الأولى أعلى جهة النهاية بدءًا من الساعة 12
  (function () {
    var host = document.querySelector('.s15 .donut'); if (!host) return;
    var d = JSON.parse(document.getElementById('chart-data').textContent), items = d.items || [];
    var W = 556, c = 278, R = 278, r = 85, total = items.reduce(function (s, it) { return s + (+it.value || 0); }, 0) || 1;
    var NS = 'http://www.w3.org/2000/svg', s = document.createElementNS(NS, 'svg');
    s.setAttribute('viewBox', '0 0 ' + W + ' ' + W); s.setAttribute('aria-hidden', 'true');
    function el(n, a) { var e = document.createElementNS(NS, n); for (var k in a) e.setAttribute(k, a[k]); s.appendChild(e); return e; }
    el('circle', { cx: c, cy: c, r: R, fill: '#EEF2FB' });
    var ang = -90, html = '';
    items.forEach(function (it) {
      var sweep = (+it.value || 0) / total * 360, a = ang * Math.PI / 180;
      el('line', { x1: c, y1: c, x2: c + (R + 6) * Math.cos(a), y2: c + (R + 6) * Math.sin(a), stroke: '#4834ED', 'stroke-width': 9 });
      var m = (ang + sweep / 2) * Math.PI / 180, lx = c + 172 * Math.cos(m), ly = c + 172 * Math.sin(m);
      html += '<div class="dl" style="top:' + (ly - 44).toFixed(1) + 'px;inset-inline-start:' + (lx - 110).toFixed(1) + 'px"><span></span><b></b></div>';
      ang += sweep;
    });
    el('circle', { cx: c, cy: c, r: r, fill: '#4834ED' });
    host.appendChild(s); host.insertAdjacentHTML('beforeend', html);
    host.querySelectorAll('.dl').forEach(function (n, i) { n.querySelector('span').textContent = items[i].label; n.querySelector('b').textContent = items[i].value + '%'; });
  })();"""

STR = {
    'en': {
        'LOGO1': 'YOUR COMPANY', 'LOGO2': 'LOGO HERE', 'COVER': 'Business<br>Strategy &amp; Planning', 'PRES': 'PRESENTATION TEMPLATE',
        'TAG': 'Add company tagline here', 'DATE': 'Add date here',
        'SUB': 'Add subheading here', 'EXO': 'Executive<br>Overview', 'BIG2': '$3.2M', 'ATH': 'Add text here',
        'P2A': 'This is a sample text. Add your desired text here. Add text here. Add your desired text here.',
        'K1': '+20%', 'K1T': 'Market Opportunity', 'PS1': 'This is a sample text. Add your text here.',
        'K2': '+3.2%', 'K2T': 'Business Performance', 'FD': 'Future Direction',
        'P2B': 'This is a sample text. Add your desired text here. This is a sample text. Add text here.',
        'T3': 'Strategic Framework', 'Q3': 'Connecting Strategy<br>With Business Impact',
        'N1': '01', 'N2': '02', 'N3': '03', 'C1': 'Strategic<br>Focus', 'C2': 'Execution<br>Model', 'C3': 'Business<br>Impact',
        'P_LONG': 'This is a sample text. Add your desired text here. Add text here. Add your desired text here.',
        'T4': 'SWOT<br>Analysis', 'PS': 'This is a sample text. Add your desired text here. Add text here.',
        'B1': 'Add bullet point here', 'B2': 'This is a sample text. Add your desired text here.', 'B3': 'This is a sample bullet point text. Add text here',
        'L1': 'S', 'L2': 'W', 'L3': 'O', 'L4': 'T', 'SW1': 'Strengths', 'SW2': 'Weaknesses', 'SW3': 'Opportunities', 'SW4': 'Threats',
        'T5': 'Market Dev. Timeline', 'Y1': '2027', 'Y2': '2028', 'Y3': '2029', 'Y4': '2030',
        'M1': 'Building The<br>Foundation For<br>Future Growth', 'M2': 'Expanding Reach<br>Across Priority<br>Market Segments',
        'M3': 'Optimizing<br>Operations for<br>Business Efficiency', 'M4': 'Expanding<br>Capabilities for<br>Market Leadership',
        'PM': 'This is a sample. Add your desired text here. This is a sample text.',
        'T6': 'Growth Roadmap', 'Q6': 'From Current Position<br>to Future Growth', 'V6': '$3.200', 'G6': '16% YoY<br>Growth',
        'P6': 'This is a sample text. Add your desired text here. Add text here.<br>Add your desired text here. Add text here. This is a sample text.',
        'ST1': 'Stage 01', 'ST2': 'Stage 02', 'ST3': 'Stage 03', 'ST4': 'Stage 04',
        'H6_1': 'Build Stronger<br>Foundations', 'H6_2': 'Expand<br>Market Reach', 'H6_3': 'Strengthen Market<br>Leadership', 'H6_4': 'Scale Proven<br>Models',
        'T7': 'Business<br>Roadmap', 'Q7': 'Every stage should strengthen<br>the next stage of the customer<br>and business journey.',
        'SG1': 'STAGE 01', 'SG2': 'STAGE 02', 'SG3': 'STAGE 03',
        'N7_1': 'Strategic Focus', 'N7_2': 'Execution Model', 'N7_3': 'Business Impact', 'P7': 'Add your desired text here. This is a sample text.',
        'CHN': '02', 'CH': 'Chapter<br>Separator', 'CHS': 'Add chapter summary here. This is a sample. Add your desired text here.', 'ATH2': 'Add text here.',
        'T9': '4-Item Step Stair Diagram', 'PC9': '23%', 'RG9': 'Revenue<br>Growth YoY', 'AT': 'Add text',
        'BV1': '~$291', 'BV2': '~$372', 'BV3': '~$528', 'BV4': '~$867',
        'T10': 'Strategic<br>Quadrants', 'P_LONG4': 'This is a sample text. Add your desired text here. Add text here. This is a sample text.',
        'BX': 'Prioritizing Growth<br>Opportunities', 'HI': 'HIGH', 'LO': 'LOW',
        'QW': 'Quick Wins', 'SB': 'Strategic Bets', 'OS': 'Operational Support', 'RC': 'Reconsider', 'P_MID': 'This is a sample text. Add your desired text here.',
        'T11': 'Business<br>Priorities', 'CE': 'Customer Expansion', 'CD': 'Capability Development', 'BIG11': '$4.127,99',
        'T12': 'Vertical<br>Timeline', 'VD': '07/10',
        'VY2028': '2028', 'VY2029': '2029', 'VY2030': '2030', 'VY2031': '2031', 'VY2032': '2032',
        'VY2033': '2033', 'VY2034': '2034', 'VY2035': '2035', 'VY2036': '2036', 'VY2037': '2037',
        'PV': 'This is a sample. Add your desired text here. This is a sample text. This is a sample. Add text here.',
        'T14': 'Target Market', 'P14': 'This is a sample text. Add your desired text here. This is a sample text. This is a sample. Add text here. Add text here.',
        'SV1': '20%', 'SV2': '12.8%', 'SV3': '18', 'SV4': '48.2K', 'PT': 'This is a sample text. Add your text here. This is a sample text.',
        'TAM': 'TAM', 'TAMV': '11.5 B', 'SAM': 'SAM', 'SAMV': '3.2 B', 'SOM': 'SOM', 'SOMV': '1.4 B', 'PC': 'This is a sample text.<br>Add your text here.',
        'T15': 'Business<br>Report Slide',
        'P15': 'This is a sample text. Add your desired text here. Add text here. Add text here. This is a sample text. Add your desired text here. Add text here. Add text here. This is a sample text.',
        'BM': 'A More Connected<br>Business Model', 'DONUT_ARIA': 'Donut chart with three segments',
        'T16': 'The Iceberg Principles', 'ICE_ALT': 'Iceberg above and below the waterline', 'PC16': 'People &amp; Culture', 'CV16': 'Company Value',
        'VB': 'Visible business results<br>are supported by deeper.', 'CU': 'Customer Understanding',
        'CL': 'Creating the<br>Next Stage<br>of Growth.', 'GIT': 'GET IN TOUCH', 'EM': 'Insert your email here', 'PH': '<span dir="ltr">+1 (234) 567 8901</span>',
    },
    'ar': {
        'LOGO1': 'شعار شركتك', 'LOGO2': 'هنا', 'COVER': 'استراتيجية الأعمال<br>والتخطيط', 'PRES': 'قالب عرض تقديمي',
        'TAG': 'أضف الشعار النصي للشركة هنا', 'DATE': 'أضف التاريخ هنا',
        'SUB': 'أضف عنوانًا فرعيًا هنا', 'EXO': 'نظرة تنفيذية<br>عامة', 'BIG2': '$3.2M', 'ATH': 'أضف النص هنا',
        'P2A': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف النص هنا. أضف النص المطلوب هنا.',
        'K1': '‎+20%', 'K1T': 'فرصة السوق', 'PS1': 'هذا نص توضيحي. أضف نصك هنا.',
        'K2': '‎+3.2%', 'K2T': 'أداء الأعمال', 'FD': 'التوجه المستقبلي',
        'P2B': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص هنا.',
        'T3': 'الإطار الاستراتيجي', 'Q3': 'ربط الاستراتيجية<br>بأثرها في الأعمال',
        'N1': '01', 'N2': '02', 'N3': '03', 'C1': 'التركيز<br>الاستراتيجي', 'C2': 'نموذج<br>التنفيذ', 'C3': 'أثر<br>الأعمال',
        'P_LONG': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف النص هنا. أضف النص المطلوب هنا.',
        'T4': 'التحليل<br>الرباعي', 'PS': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف النص هنا.',
        'B1': 'أضف نقطة هنا', 'B2': 'هذا نص توضيحي. أضف النص المطلوب هنا.', 'B3': 'هذا نص توضيحي لنقطة. أضف النص هنا',
        'L1': 'ق', 'L2': 'ض', 'L3': 'ف', 'L4': 'ت', 'SW1': 'نقاط القوة', 'SW2': 'نقاط الضعف', 'SW3': 'الفرص', 'SW4': 'التهديدات',
        'T5': 'الجدول الزمني لتطوير السوق', 'Y1': '2027', 'Y2': '2028', 'Y3': '2029', 'Y4': '2030',
        'M1': 'بناء الأساس<br>للنمو<br>المستقبلي', 'M2': 'توسيع الانتشار<br>في شرائح السوق<br>ذات الأولوية',
        'M3': 'تحسين العمليات<br>لرفع كفاءة<br>الأعمال', 'M4': 'توسيع القدرات<br>لتحقيق الريادة<br>في السوق',
        'PM': 'هذا مثال. أضف النص المطلوب هنا. هذا نص توضيحي.',
        'T6': 'خارطة النمو', 'Q6': 'من الوضع الحالي<br>إلى النمو المستقبلي', 'V6': '$3.200', 'G6': 'نمو سنوي<br>بنسبة 16%',
        'P6': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف النص هنا.<br>أضف النص المطلوب هنا. أضف النص هنا. هذا نص توضيحي.',
        'ST1': 'المرحلة 01', 'ST2': 'المرحلة 02', 'ST3': 'المرحلة 03', 'ST4': 'المرحلة 04',
        'H6_1': 'بناء أسس<br>أقوى', 'H6_2': 'توسيع<br>الانتشار في السوق', 'H6_3': 'تعزيز الريادة<br>في السوق', 'H6_4': 'توسيع النماذج<br>المثبتة',
        'T7': 'خارطة طريق<br>الأعمال', 'Q7': 'ينبغي لكل مرحلة أن تعزّز<br>المرحلة التالية من رحلة<br>العميل والأعمال.',
        'SG1': 'المرحلة 01', 'SG2': 'المرحلة 02', 'SG3': 'المرحلة 03',
        'N7_1': 'التركيز الاستراتيجي', 'N7_2': 'نموذج التنفيذ', 'N7_3': 'أثر الأعمال', 'P7': 'أضف النص المطلوب هنا. هذا نص توضيحي.',
        'CHN': '02', 'CH': 'فاصل<br>الفصل', 'CHS': 'أضف ملخص الفصل هنا. هذا مثال. أضف النص المطلوب هنا.', 'ATH2': 'أضف النص هنا.',
        'T9': 'مخطط الدرج بأربع خطوات', 'PC9': '23%', 'RG9': 'نمو الإيرادات<br>على أساس سنوي', 'AT': 'أضف نصًا',
        'BV1': '‎~$291', 'BV2': '‎~$372', 'BV3': '‎~$528', 'BV4': '‎~$867',
        'T10': 'المربعات<br>الاستراتيجية', 'P_LONG4': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف النص هنا. هذا نص توضيحي.',
        'BX': 'ترتيب أولويات<br>فرص النمو', 'HI': 'مرتفع', 'LO': 'منخفض',
        'QW': 'مكاسب سريعة', 'SB': 'رهانات استراتيجية', 'OS': 'دعم تشغيلي', 'RC': 'إعادة النظر', 'P_MID': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
        'T11': 'أولويات<br>الأعمال', 'CE': 'التوسع في العملاء', 'CD': 'تطوير القدرات', 'BIG11': '$4.127,99',
        'T12': 'جدول زمني<br>رأسي', 'VD': '07/10',
        'VY2028': '2028', 'VY2029': '2029', 'VY2030': '2030', 'VY2031': '2031', 'VY2032': '2032',
        'VY2033': '2033', 'VY2034': '2034', 'VY2035': '2035', 'VY2036': '2036', 'VY2037': '2037',
        'PV': 'هذا مثال. أضف النص المطلوب هنا. هذا نص توضيحي. هذا مثال. أضف النص هنا.',
        'T14': 'السوق المستهدف', 'P14': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. هذا مثال. أضف النص هنا. أضف النص هنا.',
        'SV1': '20%', 'SV2': '12.8%', 'SV3': '18', 'SV4': '48.2K', 'PT': 'هذا نص توضيحي. أضف نصك هنا. هذا نص توضيحي.',
        'TAM': 'الكلي', 'TAMV': '11.5 مليار', 'SAM': 'المتاح', 'SAMV': '3.2 مليار', 'SOM': 'الممكن', 'SOMV': '1.4 مليار', 'PC': 'هذا نص توضيحي.<br>أضف نصك هنا.',
        'T15': 'شريحة تقرير<br>الأعمال',
        'P15': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف النص هنا. أضف النص هنا. هذا نص توضيحي. أضف النص المطلوب هنا. أضف النص هنا. أضف النص هنا. هذا نص توضيحي.',
        'BM': 'نموذج أعمال<br>أكثر ترابطًا', 'DONUT_ARIA': 'رسم حلقي من ثلاث شرائح',
        'T16': 'مبادئ جبل الجليد', 'ICE_ALT': 'جبل جليد فوق سطح الماء وتحته', 'PC16': 'الأفراد والثقافة', 'CV16': 'قيمة الشركة',
        'VB': 'النتائج الظاهرة للأعمال<br>تقوم على أسس أعمق.', 'CU': 'فهم العملاء',
        'CL': 'نصنع<br>المرحلة التالية<br>من النمو.', 'GIT': 'تواصل معنا', 'EM': 'أدخل بريدك الإلكتروني هنا', 'PH': '<span dir="ltr">+1 (234) 567 8901</span>',
    },
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'mgmt', 'icon': '🧭',
    'name': 'استراتيجية الأعمال والتخطيط', 'nameEn': 'Business Strategy & Planning',
    'description': 'سبع عشرة شريحة: نظرة تنفيذية، إطار استراتيجي، تحليل رباعي، خرائط طريق، جداول زمنية، السوق المستهدف وتقرير الأعمال',
    'descriptionEn': '17 slides: executive overview, framework, SWOT, roadmaps, timelines, target market, business report',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'نظرة تنفيذية عامة', 'titleEn': 'Executive Overview', 'needsData': True},
        {'title': 'الإطار الاستراتيجي', 'titleEn': 'Strategic Framework'},
        {'title': 'التحليل الرباعي', 'titleEn': 'SWOT Analysis'},
        {'title': 'الجدول الزمني لتطوير السوق', 'titleEn': 'Market Dev. Timeline'},
        {'title': 'خارطة النمو', 'titleEn': 'Growth Roadmap', 'needsData': True},
        {'title': 'خارطة طريق الأعمال', 'titleEn': 'Business Roadmap'},
        {'title': 'فاصل الفصل', 'titleEn': 'Chapter Separator'},
        {'title': 'مخطط الدرج بأربع خطوات', 'titleEn': '4-Item Step Stair Diagram', 'needsData': True},
        {'title': 'المربعات الاستراتيجية', 'titleEn': 'Strategic Quadrants'},
        {'title': 'أولويات الأعمال', 'titleEn': 'Business Priorities', 'needsData': True},
        {'title': 'جدول زمني رأسي (1)', 'titleEn': 'Vertical Timeline (1)'},
        {'title': 'جدول زمني رأسي (2)', 'titleEn': 'Vertical Timeline (2)'},
        {'title': 'السوق المستهدف', 'titleEn': 'Target Market', 'needsData': True},
        {'title': 'شريحة تقرير الأعمال', 'titleEn': 'Business Report Slide', 'needsData': True},
        {'title': 'مبادئ جبل الجليد', 'titleEn': 'The Iceberg Principles'},
        {'title': 'الختام والتواصل', 'titleEn': 'Closing & Contact'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والإحداثيات وغيّر النصوص والأرقام فقط؛ أجزاء الرسم الحلقي في شريحة تقرير الأعمال تُرسم من كتلة chart-data (التسمية والقيمة لكل جزء).',
}
