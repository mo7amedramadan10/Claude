# -*- coding: utf-8 -*-
# نموذج «النمو المدعوم بالذكاء الاصطناعي»: 12 شريحة بهوية بنفسجية/صفراء وخلفية مثلثات شفافة في زاويتي الإطار.
import random

KEY = 'sl-ai-powered-growth'
TITLE = {'ar': 'النمو المدعوم بالذكاء الاصطناعي', 'en': 'AI Powered Growth'}
COMMENT = 'نموذج «النمو المدعوم بالذكاء الاصطناعي»: غلاف، ملخص تنفيذي بثلاث نسب، لماذا يهمّ الذكاء الاصطناعي (صورة + 3 أعمدة)، التحديات (4 بنود مرقّمة)، إطار الحلول (بطاقات)، القدرات (لوحة بنفسجية + صورة)، خارطة طريق بأربع سنوات، الأثر على الأعمال، مقاييس النجاح (رسم أعمدة من chart-data)، لماذا تختار الذكاء الاصطناعي، اقتباس، وشكر. الخلفية مثلثات SVG شفافة تُعكس في العربية.'
VARS = '--ink: #262626; --ink-2: #333333; --gray: #6B6B6B; --accent: #3B2BD1; --accent-2: #2F22B8; --lilac: #A77BF0; --lilac-2: #B388FF; --lav: #DCD6FF; --yellow: #FFC000; --yellow-2: #E8A93A; --line: #E4E4EA; --card: #F7F7F9; --bg: #FFFFFF;'

# ---------- خلفية المثلثات (شبكة مجزّأة بألوان فاتحة تتلاشى بعيدًا عن الزاوية) ----------
def mesh(x0, y0, x1, y1, corner, palette, strength, seed, cell=72):
    rnd = random.Random(seed)
    cols = int((x1 - x0) / cell) + 1; rows = int((y1 - y0) / cell) + 1
    pts = {}
    for i in range(cols + 1):
        for j in range(rows + 1):
            px = x0 + i * cell + (0 if i in (0, cols) else rnd.uniform(-cell * .35, cell * .35))
            py = y0 + j * cell + (0 if j in (0, rows) else rnd.uniform(-cell * .35, cell * .35))
            pts[(i, j)] = (px, py)
    cx, cy = corner
    diag = ((x1 - x0) ** 2 + (y1 - y0) ** 2) ** .5
    out = []
    for i in range(cols):
        for j in range(rows):
            a, b, c, d = pts[(i, j)], pts[(i + 1, j)], pts[(i + 1, j + 1)], pts[(i, j + 1)]
            tris = ((a, b, c), (a, c, d)) if rnd.random() < .5 else ((a, b, d), (b, c, d))
            for t in tris:
                mx = sum(p[0] for p in t) / 3; my = sum(p[1] for p in t) / 3
                dist = ((mx - cx) ** 2 + (my - cy) ** 2) ** .5 / diag
                op = max(0.0, 1 - dist) ** 2.4 * strength * rnd.uniform(.3, 1)
                if op < .02: continue
                out.append('<polygon points="%s" fill="%s" opacity="%.2f"/>' % (' '.join('%.0f,%.0f' % p for p in t), rnd.choice(palette), op))
    return ''.join(out)


PURPLES = ['#C7B6F3', '#D9CCF8', '#E9E1FB', '#B9A4EE', '#EFEAFD']
STRONG = ['#A98BEF', '#B9A2F2', '#CDBDF6', '#9478EA', '#DDD2F8']
BLUES = ['#C2CCF4', '#D5DCF8', '#E6EAFB', '#B4C0F0', '#EEF1FC']


MESH_SOFT = (mesh(760, -40, 1300, 340, (1280, 0), PURPLES, .42, 7) + mesh(-40, 440, 500, 740, (0, 720), BLUES, .38, 11))
MESH_STRONG = (mesh(740, -40, 1300, 400, (1280, 0), STRONG, 1.25, 7, 64) + mesh(-40, 420, 520, 740, (0, 720), BLUES, .5, 11))  # شريحة الشكر: عنقود مشبع


def poly(strong=False):  # الشبكة معرّفة مرة واحدة في GLOBAL_DEFS وتُستدعى بـ use حتى تبقى الصفحة صغيرة
    return '<svg class="poly" viewBox="0 0 1280 720" aria-hidden="true"><use href="#%s"/></svg>' % ('mesh-strong' if strong else 'mesh-soft')


# ---------- الأيقونات (SVG مضمّن 24×24) ----------
I = {
    'logo': '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M5 4.5C9 3.4 15 3.4 19 4.5c1.4 4.5 1.4 10.5 0 15-4 1.1-10 1.1-14 0-1.4-4.5-1.4-10.5 0-15z" fill="none" stroke="currentColor" stroke-width="3.2" stroke-linejoin="round" transform="rotate(-12 12 12)"/></svg>',
    'brain': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M9.5 2.5a3 3 0 0 0-3 2.6A3.2 3.2 0 0 0 3.6 9.3 3.3 3.3 0 0 0 3 13.6a3.3 3.3 0 0 0 2.3 4.6 3 3 0 0 0 5.7.9V3.4a3 3 0 0 0-1.5-.9zm5 0a3 3 0 0 1 3 2.6 3.2 3.2 0 0 1 2.9 4.2 3.3 3.3 0 0 1 .6 4.3 3.3 3.3 0 0 1-2.3 4.6 3 3 0 0 1-5.7.9V3.4a3 3 0 0 1 1.5-.9z"/><path d="M11 6.5c-1.5.3-2.4 1.2-2.6 2.4M13 10.5c1.6.2 2.6 1 2.9 2.3M11 13c-1.4.2-2.3.9-2.8 2M13 16.3c1.3.1 2.2.7 2.6 1.6" fill="none" stroke="#fff" stroke-width=".9" stroke-linecap="round" opacity=".9"/></svg>',
    'chev': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M11.2 3 2 12l9.2 9 2.6-2.6L7.4 12l6.4-6.4z"/><path d="M17.2 3 8 12l9.2 9 2.6-2.6L13.4 12l6.4-6.4z" opacity=".85"/><path d="M22 5.6 15.6 12l6.4 6.4V5.6z" opacity=".7"/></svg>',
    'smile': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path fill-rule="evenodd" d="M12 1.5a10.5 10.5 0 1 1 0 21 10.5 10.5 0 0 1 0-21zM8.6 8.2a1.5 1.5 0 1 0 0 3 1.5 1.5 0 0 0 0-3zm6.8 0a1.5 1.5 0 1 0 0 3 1.5 1.5 0 0 0 0-3zM7.2 14c.8 2.5 2.6 3.9 4.8 3.9s4-1.4 4.8-3.9H7.2z"/></svg>',
    'smile-o': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.4" stroke-linecap="round" aria-hidden="true"><circle cx="12" cy="12" r="10"/><circle cx="8.6" cy="9.5" r="1.1" fill="currentColor" stroke="none"/><circle cx="15.4" cy="9.5" r="1.1" fill="currentColor" stroke="none"/><path d="M7.5 14.3c1 2 2.6 3 4.5 3s3.5-1 4.5-3"/></svg>',
    'star': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12 1.8l3.1 6.5 7.1.9-5.2 4.9 1.3 7.1L12 17.7l-6.3 3.5 1.3-7.1L1.8 9.2l7.1-.9z"/></svg>',
    'torch': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12 1.5c1.2 2 3.6 3.4 3.6 6.3A3.6 3.6 0 0 1 12 11.4a3.6 3.6 0 0 1-3.6-3.6c0-1.1.4-2 1-2.7.2 1 .8 1.6 1.5 1.8-.3-1.9.2-3.8 1.1-5.4z"/><path d="M7.8 12.6h8.4l-1 2.2H8.8zM9.4 15.8h5.2l-1.4 6.7h-2.4z"/></svg>',
    'puzzle': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M3 3h6.2a2 2 0 1 1 3.6 0H19v6.2a2 2 0 1 0 0 3.6V19h-6.2a2 2 0 1 0-3.6 0H3v-6.2a2 2 0 1 1 0-3.6z"/><path d="M17 15h4v6h-6v-2.3a1.6 1.6 0 1 1 2-2.3z" opacity=".75"/></svg>',
    'people': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><circle cx="6" cy="9" r="2.2"/><circle cx="12" cy="7.5" r="2.4"/><circle cx="18" cy="9" r="2.2"/><path d="M2.5 16c0-2.1 1.6-3.6 3.5-3.6.9 0 1.7.3 2.3.8A5 5 0 0 0 7 16v1.3H2.5zM8 17c0-2.6 1.8-4.5 4-4.5s4 1.9 4 4.5v1.5H8zm9-1a5 5 0 0 0-1.3-2.8c.6-.5 1.4-.8 2.3-.8 1.9 0 3.5 1.5 3.5 3.6v1.3H17z"/><path d="M3 22l5-4.5 3.5 2.2L17 15h-2.5l.4-2.5 5.6.6-.7 5.4-1.3-2.3-6.7 5.2-3.5-2.2L4.2 23z" opacity=".95"/></svg>',
    'bulb': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" aria-hidden="true"><path d="M12 2.5a6.5 6.5 0 0 0-3.9 11.7c.8.6 1.2 1.3 1.2 2.1v.7h5.4v-.7c0-.8.4-1.5 1.2-2.1A6.5 6.5 0 0 0 12 2.5z"/><path d="M9.6 19.5h4.8M10.4 21.8h3.2M12 7v4"/></svg>',
    'atom': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" aria-hidden="true"><ellipse cx="12" cy="12" rx="10" ry="4"/><ellipse cx="12" cy="12" rx="10" ry="4" transform="rotate(60 12 12)"/><ellipse cx="12" cy="12" rx="10" ry="4" transform="rotate(-60 12 12)"/><circle cx="12" cy="12" r="1.8" fill="currentColor" stroke="none"/></svg>',
    'cycle': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 12a8 8 0 0 1-13.7 5.6M4 12a8 8 0 0 1 13.7-5.6"/><path d="M17 3.2v3.4h-3.4M7 20.8v-3.4h3.4"/></svg>',
    'mail': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true"><rect x="3" y="5.5" width="18" height="13" rx="1.5"/><path d="M3.5 6.5 12 13l8.5-6.5"/></svg>',
    'phone': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M6.6 3.2c.6-.2 1.2 0 1.6.5l2 2.9c.4.6.3 1.3-.2 1.8L8.6 9.7a12.6 12.6 0 0 0 5.7 5.7l1.3-1.4c.5-.5 1.2-.6 1.8-.2l2.9 2c.5.4.7 1 .5 1.6l-.9 2.4c-.3.8-1.1 1.3-2 1.2C10.5 20.2 3.8 13.5 3 6.1c-.1-.9.4-1.7 1.2-2z"/></svg>',
    'pin': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12 1.8a7 7 0 0 0-7 7c0 5.3 7 13.4 7 13.4s7-8.1 7-13.4a7 7 0 0 0-7-7zm0 9.7a2.7 2.7 0 1 1 0-5.4 2.7 2.7 0 0 1 0 5.4z"/></svg>',
    'money': '<svg viewBox="0 0 32 24" fill="none" stroke="currentColor" stroke-width="1.3" stroke-linejoin="round" aria-hidden="true"><path d="M10 8l12-3 2 10-12 3z"/><circle cx="17" cy="12.5" r="2.4"/><path d="M9.5 8.5c-3-1.5-6-1-8 1 2 .2 3.5 1 4.5 2.5-1.5.5-2.5 1.6-3 3 2-.5 4 0 5.5 1.2M22.5 5c3-1.5 6-1 8 1-2 .2-3.5 1-4.5 2.5 1.5.5 2.5 1.6 3 3-2-.5-4 0-5.5 1.2"/></svg>',
    'coins': '<svg viewBox="0 0 32 24" fill="none" stroke="currentColor" stroke-width="1.3" aria-hidden="true"><ellipse cx="13" cy="6" rx="9" ry="3.2"/><path d="M4 6v4c0 1.8 4 3.2 9 3.2s9-1.4 9-3.2V6M4 10v4c0 1.8 4 3.2 9 3.2s9-1.4 9-3.2v-4"/><ellipse cx="20" cy="17" rx="9" ry="3.2"/><path d="M11 17v2.5c0 1.8 4 3.2 9 3.2s9-1.4 9-3.2V17"/></svg>',
    'venn': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.2" aria-hidden="true"><circle cx="12" cy="8" r="5.5"/><circle cx="8.5" cy="14" r="5.5"/><circle cx="15.5" cy="14" r="5.5"/></svg>',
}

STYLE = """  .poly { position: absolute; inset: 0; width: 1280px; height: 720px; pointer-events: none; }
  html[dir="rtl"] .poly { transform: scaleX(-1); }
  .abs { position: absolute; margin: 0; }
  .ttl { position: absolute; margin: 0; font-size: 60px; font-weight: 800; line-height: 1.1; color: var(--ink-2); letter-spacing: -1.5px; }
  html[lang="ar"] .ttl { letter-spacing: 0; font-size: 54px; }
  .sub { position: absolute; margin: 0; font-size: 25px; font-weight: 400; color: var(--lilac); line-height: 1.2; }
  .p { margin: 0; font-size: 17px; line-height: 1.45; color: var(--gray); }
  .p18 { font-size: 18px; }
  .h { margin: 0; font-size: 22px; font-weight: 700; line-height: 1.25; color: var(--ink); }
  .ic { display: grid; place-items: center; }
  .ic svg { width: 100%; height: 100%; }
  .ring { border-radius: 50%; background: #fff; position: relative; display: grid; place-items: center; }
  .ring::before { content: ''; position: absolute; inset: 0; border-radius: 50%; padding: 3px; background: linear-gradient(180deg, #E4D9FF, var(--accent)); -webkit-mask: linear-gradient(#000 0 0) content-box, linear-gradient(#000 0 0); mask: linear-gradient(#000 0 0) content-box, linear-gradient(#000 0 0); -webkit-mask-composite: xor; mask-composite: exclude; }
  .photo { position: absolute; object-fit: cover; display: block; }
  .logo { position: absolute; top: 88px; inset-inline-start: 112px; display: flex; gap: 26px; align-items: center; }
  .logo .ic { width: 56px; height: 58px; color: var(--accent); }
  .logo p { margin: 0; font-size: 17px; letter-spacing: 3px; line-height: 1.3; color: var(--ink); text-transform: uppercase; width: 200px; }
  html[lang="ar"] .logo p { letter-spacing: 0; font-size: 19px; font-weight: 500; }
  .vline { position: absolute; width: 1px; background: var(--line); }
  .hline { position: absolute; height: 1px; background: var(--line); }
  /* 1 الغلاف */
  .s1 .big { position: absolute; margin: 0; font-size: 112px; font-weight: 900; line-height: 1; letter-spacing: 3px; color: #111; }
  html[lang="ar"] .s1 .big { font-size: 84px; letter-spacing: 0; font-weight: 800; }
  .s1 .t1 { top: 290px; inset-inline-start: 118px; }
  .s1 .t2 { top: 412px; inset-inline-start: 110px; width: 600px; height: 127px; background: var(--yellow); border-radius: 12px; display: flex; align-items: center; padding-inline-start: 8px; }
  html[lang="ar"] .s1 .t2 { font-size: 68px; }
  .s1 .t3 { top: 571px; inset-inline-start: 110px; width: 598px; height: 52px; background: var(--accent); border-radius: 6px; color: var(--yellow); font-size: 32px; font-weight: 400; text-transform: uppercase; display: grid; place-items: center; letter-spacing: 0; }
  html[lang="ar"] .s1 .t3 { font-size: 27px; font-weight: 500; }
  .s1 .dash { top: 445px; inset-inline-start: 963px; width: 58px; height: 3px; background: var(--accent); }
  .s1 .note { top: 490px; inset-inline-start: 963px; width: 235px; font-size: 19px; line-height: 1.35; color: #555; }
  .s1 .date { top: 596px; inset-inline-start: 963px; font-size: 20px; font-weight: 700; color: var(--accent); }
  /* 2 الملخص التنفيذي */
  .s2 .ttl { top: 65px; inset-inline-start: 98px; }
  .s2 .band { top: 213px; inset-inline-start: 35px; width: 1210px; height: 148px; background: var(--accent); border-radius: 18px; }
  .s2 .box { top: 355px; inset-inline-start: 48px; width: 1182px; height: 305px; background: #FAFAFC; border: 1px solid #B9B0F0; border-radius: 0 0 18px 18px; }
  .s2 .tab { top: 188px; height: 58px; width: 272px; background: var(--yellow); border-radius: 8px; color: var(--accent); font-size: 21px; font-weight: 700; display: flex; align-items: center; padding-inline-start: 21px; }
  .s2 .tab1 { inset-inline-start: 77px; } .s2 .tab2 { inset-inline-start: 745px; }
  .s2 .bt { top: 275px; font-size: 18px; color: var(--lav); line-height: 1.4; }
  .s2 .bt1 { inset-inline-start: 98px; width: 585px; } .s2 .bt2 { inset-inline-start: 766px; width: 430px; }
  .s2 .stat { top: 410px; width: 300px; }
  .s2 .stat .n { margin: 0; font-size: 58px; font-weight: 800; line-height: 1; color: var(--accent); letter-spacing: -2px; }
  .s2 .stat .h { margin-top: 22px; width: 190px; }
  .s2 .stat .p { margin-top: 18px; width: 270px; }
  .s2 .st1 { inset-inline-start: 113px; } .s2 .st2 { inset-inline-start: 498px; } .s2 .st3 { inset-inline-start: 885px; }
  .s2 .vl1 { inset-inline-start: 446px; } .s2 .vl2 { inset-inline-start: 833px; } .s2 .vline { top: 410px; height: 190px; }
  /* 3 لماذا يهمّ */
  .s3 .photo { top: 35px; inset-inline-start: 40px; width: 500px; height: 640px; border-radius: 18px; }
  .s3 .ttl { top: 68px; inset-inline-start: 696px; }
  html[lang="ar"] .s3 .ttl { font-size: 44px; white-space: nowrap; top: 74px; }
  .s3 .sub { top: 152px; inset-inline-start: 696px; }
  .s3 .lead { top: 228px; inset-inline-start: 696px; width: 470px; font-size: 18px; }
  .s3 .panel { top: 360px; inset-inline-start: 320px; width: 960px; height: 360px; background: #fff; border-start-start-radius: 40px; box-shadow: 0 -8px 30px rgb(40 20 120 / .08); }
  .s3 .col { top: 409px; width: 240px; }
  .s3 .col .ring { width: 70px; height: 70px; }
  .s3 .col .ic { width: 36px; height: 36px; color: var(--yellow); }
  .s3 .col .h { margin-top: 50px; width: 220px; }
  .s3 .col .p { margin-top: 22px; width: 225px; }
  .s3 .c1 { inset-inline-start: 388px; } .s3 .c2 { inset-inline-start: 700px; } .s3 .c3 { inset-inline-start: 1006px; }
  .s3 .vline { top: 413px; height: 247px; } .s3 .vl1 { inset-inline-start: 651px; } .s3 .vl2 { inset-inline-start: 957px; }
  /* 4 التحديات */
  .s4 .ttl { top: 70px; inset-inline-start: 83px; width: 480px; }
  .s4 .sub { top: 222px; inset-inline-start: 83px; }
  .s4 .photo { top: 293px; inset-inline-start: 62px; width: 528px; height: 382px; border-radius: 14px; }
  .s4 .it { inset-inline-start: 716px; width: 480px; display: flex; gap: 32px; align-items: flex-start; }
  .s4 .it .num { margin: 0; flex: none; width: 60px; font-size: 36px; font-weight: 800; color: var(--accent); line-height: 1; letter-spacing: -1px; }
  .s4 .it .h { font-size: 24px; }
  .s4 .it .p { margin-top: 8px; width: 330px; }
  .s4 .i1 { top: 85px; } .s4 .i2 { top: 240px; } .s4 .i3 { top: 393px; } .s4 .i4 { top: 560px; }
  .s4 .hline { inset-inline-start: 712px; width: 498px; } .s4 .hl1 { top: 200px; } .s4 .hl2 { top: 355px; }
  .s4 .box { top: 524px; inset-inline-start: 534px; width: 662px; height: 152px; background: var(--accent); border-radius: 10px; }
  .s4 .box .ic { position: absolute; top: 44px; inset-inline-start: 42px; width: 68px; height: 68px; color: #fff; }
  .s4 .i4 .num, .s4 .i4 .h { color: var(--yellow); } .s4 .i4 .p { color: #fff; }
  /* 5 إطار الحلول */
  .s5 .card { width: 367px; height: 297px; background: var(--card); border: 1px solid #D8D8E0; border-radius: 14px; }
  .s5 .card .ic { position: absolute; top: 52px; inset-inline-start: 46px; width: 52px; height: 48px; color: var(--accent); }
  .s5 .card .h { position: absolute; top: 136px; inset-inline-start: 46px; font-size: 25px; }
  .s5 .card .p { position: absolute; top: 186px; inset-inline-start: 46px; width: 260px; }
  .s5 .k1 { top: 52px; inset-inline-start: 43px; } .s5 .k2 { top: 52px; inset-inline-start: 427px; }
  .s5 .k3 { top: 372px; inset-inline-start: 487px; }
  .s5 .k4 { top: 372px; inset-inline-start: 871px; background: var(--accent); border-color: var(--accent); }
  .s5 .k4 .ic { color: var(--yellow); } .s5 .k4 .h { color: var(--yellow); } .s5 .k4 .p { color: #fff; }
  .s5 .photo { top: 372px; inset-inline-start: 43px; width: 426px; height: 297px; border-radius: 14px; }
  .s5 .ttl { top: 100px; inset-inline-start: 832px; width: 420px; }
  .s5 .sub { top: 258px; inset-inline-start: 832px; }
  /* 6 القدرات */
  .s6 .panel { top: 0; inset-inline-start: 0; width: 610px; height: 720px; background: var(--accent); border-start-end-radius: 80px; }
  .s6 .it { inset-inline-start: 87px; width: 450px; display: flex; gap: 32px; align-items: flex-start; }
  .s6 .it .num { flex: none; width: 44px; height: 44px; border-radius: 50%; background: var(--yellow); color: var(--accent); font-size: 17px; font-weight: 700; display: grid; place-items: center; margin: 0; }
  .s6 .it .h { color: #fff; margin-top: 9px; }
  .s6 .it .p { margin-top: 8px; width: 320px; color: var(--lav); font-size: 18px; }
  .s6 .i1 { top: 87px; } .s6 .i2 { top: 235px; } .s6 .i3 { top: 383px; } .s6 .i4 { top: 530px; }
  .s6 .hline { inset-inline-start: 78px; width: 455px; background: rgb(255 255 255 / .18); }
  .s6 .hl1 { top: 207px; } .s6 .hl2 { top: 355px; } .s6 .hl3 { top: 503px; }
  .s6 .ttl { top: 90px; inset-inline-start: 737px; width: 500px; }
  html[lang="ar"] .s6 .ttl { font-size: 42px; width: 480px; }
  .s6 .lead { top: 258px; inset-inline-start: 737px; width: 440px; font-size: 18px; }
  .s6 .photo { top: 385px; inset-inline-start: 544px; width: 736px; height: 335px; border-start-start-radius: 40px; }
  /* 7 خارطة الطريق */
  .s7 .ttl { top: 65px; inset-inline-start: 83px; }
  .s7 .lead { top: 156px; inset-inline-start: 83px; width: 900px; font-size: 18px; }
  .s7 .pin { top: 258px; width: 80px; height: 95px; }
  .s7 .pin svg.shape { position: absolute; inset: 0; width: 80px; height: 95px; color: var(--accent); }
  .s7 .pin .ic { position: absolute; top: 20px; inset-inline-start: 22px; width: 36px; height: 36px; color: #fff; }
  .s7 .arrows { position: absolute; top: 375px; inset-inline-start: 70px; width: 1200px; height: 98px; }
  html[dir="rtl"] .s7 .arrows { transform: scaleX(-1); }
  .s7 .yr { top: 398px; margin: 0; font-size: 52px; font-weight: 800; color: var(--accent); line-height: 1; letter-spacing: -1px; }
  .s7 .st { top: 495px; width: 230px; }
  .s7 .st .h { font-size: 24px; }
  .s7 .st .p { margin-top: 16px; width: 200px; }
  .s7 .x1 { inset-inline-start: 98px; } .s7 .x2 { inset-inline-start: 390px; } .s7 .x3 { inset-inline-start: 695px; } .s7 .x4 { inset-inline-start: 1001px; }
  .s7 .p1 { inset-inline-start: 104px; } .s7 .p2 { inset-inline-start: 392px; } .s7 .p3 { inset-inline-start: 693px; } .s7 .p4 { inset-inline-start: 1001px; }
  /* 8 الأثر */
  .s8 .ttl { top: 65px; inset-inline-start: 83px; }
  .s8 .sub { top: 152px; inset-inline-start: 83px; }
  .s8 .card { top: 232px; height: 430px; border-radius: 12px; border: 1px solid #5B48E0; background: #fff; }
  .s8 .c1 { inset-inline-start: 75px; width: 318px; background: var(--accent); border-color: var(--accent); }
  .s8 .c1 .ic { position: absolute; top: 44px; inset-inline-start: 50px; width: 64px; height: 64px; color: #fff; }
  .s8 .c1 .n { position: absolute; top: 148px; inset-inline-start: 50px; margin: 0; font-size: 72px; font-weight: 800; color: var(--yellow); line-height: 1; letter-spacing: -2px; }
  .s8 .c1 .h { position: absolute; top: 238px; inset-inline-start: 50px; color: #fff; font-size: 24px; width: 200px; }
  .s8 .c1 .p { position: absolute; top: 310px; inset-inline-start: 50px; width: 220px; color: var(--lav); }
  .s8 .c2 { inset-inline-start: 410px; width: 322px; }
  .s8 .m { position: absolute; inset-inline-start: 36px; width: 260px; display: flex; gap: 32px; align-items: flex-start; }
  .s8 .m .ic { flex: none; width: 82px; height: 60px; color: var(--lilac-2); margin-top: 6px; }
  .s8 .m .n { margin: 0; font-size: 50px; font-weight: 800; color: var(--accent); line-height: 1; letter-spacing: -1.5px; }
  .s8 .m .h { margin-top: 8px; font-size: 19px; color: #555; width: 120px; }
  .s8 .m1 { top: 54px; } .s8 .m2 { top: 270px; }
  .s8 .c2 .hline { top: 214px; inset-inline-start: 34px; width: 254px; }
  .s8 .c3 { inset-inline-start: 747px; width: 460px; }
  .s8 .c3 .h { position: absolute; top: 48px; inset-inline-start: 52px; font-size: 24px; }
  .s8 .c3 ul { position: absolute; top: 112px; inset-inline-start: 52px; margin: 0; padding: 0; list-style: none; width: 370px; }
  .s8 .c3 li { position: relative; padding-inline-start: 30px; font-size: 17px; line-height: 1.5; color: #555; margin-bottom: 8px; }
  .s8 .c3 li::before { content: ''; position: absolute; top: 7px; inset-inline-start: 2px; width: 7px; height: 7px; border-radius: 50%; background: var(--accent); box-shadow: 0 0 0 2px #fff, 0 0 0 3.5px var(--accent); }
  /* 9 المقاييس */
  .s9 .ttl { top: 65px; inset-inline-start: 83px; }
  .s9 .sub { top: 152px; inset-inline-start: 83px; }
  .s9 .panel { top: 243px; inset-inline-start: 0; width: 812px; height: 477px; background: var(--accent); border-start-end-radius: 60px; }
  .s9 .ct { top: 283px; inset-inline-start: 83px; color: #fff; font-size: 22px; }
  .s9 .chart { position: absolute; top: 340px; inset-inline-start: 95px; width: 600px; height: 335px; }
  .s9 .chart svg { width: 100%; height: 100%; }
  .s9 .chart text { font-family: var(--font); font-size: 13px; fill: #fff; }
  .s9 .kpi { inset-inline-start: 905px; width: 300px; }
  .s9 .kpi .n { margin: 0; font-size: 50px; font-weight: 800; color: var(--accent); line-height: 1; letter-spacing: -1.5px; }
  .s9 .kpi .h { margin-top: 24px; }
  .s9 .kpi .p { margin-top: 8px; width: 290px; }
  .s9 .k1 { top: 265px; } .s9 .k2 { top: 495px; }
  .s9 .hline { top: 458px; inset-inline-start: 888px; width: 310px; }
  /* 10 لماذا تختار */
  .s10 .ttl { top: 65px; inset-inline-start: 0; width: 1280px; text-align: center; }
  .s10 .sub { top: 152px; inset-inline-start: 0; width: 1280px; text-align: center; }
  .s10 .col { top: 238px; width: 300px; display: flex; flex-direction: column; align-items: center; text-align: center; }
  .s10 .col .ring { width: 90px; height: 90px; }
  .s10 .col .ic { width: 50px; height: 50px; color: var(--accent); }
  .s10 .col .h { margin-top: 24px; }
  .s10 .col .p { margin-top: 12px; width: 275px; }
  .s10 .c1 { inset-inline-start: 65px; } .s10 .c2 { inset-inline-start: 490px; } .s10 .c3 { inset-inline-start: 915px; }
  .s10 .vline { top: 238px; height: 237px; } .s10 .vl1 { inset-inline-start: 427px; } .s10 .vl2 { inset-inline-start: 851px; }
  .s10 .bar { top: 528px; inset-inline-start: 52px; width: 1176px; height: 148px; background: var(--accent); border-radius: 14px; }
  .s10 .bar .ic { position: absolute; top: 35px; inset-inline-start: 62px; width: 56px; height: 76px; color: var(--yellow); }
  .s10 .bar .vline { top: 40px; inset-inline-start: 148px; height: 70px; background: rgb(255 255 255 / .35); }
  .s10 .bar .p { position: absolute; top: 48px; inset-inline-start: 180px; width: 910px; color: #fff; font-size: 22px; line-height: 1.35; }
  /* 11 الاقتباس */
  .s11 { background: var(--accent); }
  .s11 .deco { position: absolute; inset: 0; width: 1280px; height: 720px; fill: none; stroke: rgb(255 255 255 / .35); stroke-width: 1.2; }
  html[dir="rtl"] .s11 .deco { transform: scaleX(-1); }
  .s11 .lbl { top: 66px; inset-inline-start: 98px; color: #fff; font-size: 20px; font-weight: 700; letter-spacing: .5px; text-transform: uppercase; }
  .s11 .qm { top: 196px; inset-inline-start: 98px; width: 86px; height: 70px; color: var(--yellow); }
  .s11 .q { top: 230px; inset-inline-start: 212px; width: 1000px; color: #fff; font-size: 40px; line-height: 1.3; font-weight: 400; }
  html[lang="ar"] .s11 .q { font-size: 36px; line-height: 1.5; }
  .s11 .qline { top: 557px; inset-inline-start: 0; width: 162px; height: 1px; background: rgb(255 255 255 / .6); }
  .s11 .who { top: 540px; inset-inline-start: 210px; color: #fff; font-size: 24px; font-weight: 700; }
  .s11 .role { top: 582px; inset-inline-start: 210px; color: var(--yellow-2); font-size: 22px; }
  /* 12 الشكر */
  .s12 .big { top: 278px; inset-inline-start: 115px; margin: 0; font-size: 110px; font-weight: 900; line-height: 1; letter-spacing: -4px; color: #111; width: 800px; }
  html[lang="ar"] .s12 .big { font-size: 96px; letter-spacing: 0; font-weight: 800; line-height: 1.15; }
  .s12 .btn { top: 413px; inset-inline-start: 657px; width: 232px; height: 70px; background: var(--yellow); border-radius: 35px; color: #111; font-size: 24px; font-weight: 700; display: grid; place-items: center; text-transform: uppercase; }
  html[lang="ar"] .s12 .btn { inset-inline-start: 640px; top: 420px; }
  .s12 .ct { top: 575px; display: flex; align-items: center; gap: 18px; }
  .s12 .ct .ic { flex: none; width: 48px; height: 48px; background: var(--accent); color: var(--yellow); border-radius: 10px; border-end-start-radius: 0; padding: 12px; }
  .s12 .ct p { margin: 0; font-size: 20px; color: #4A4A55; }
  .s12 .ct1 { inset-inline-start: 120px; } .s12 .ct2 { inset-inline-start: 447px; } .s12 .ct3 { inset-inline-start: 780px; }"""

GLOBAL_DEFS = ('<defs>'
               '<symbol id="mesh-soft" viewBox="0 0 1280 720">' + MESH_SOFT + '</symbol>'
               '<symbol id="mesh-strong" viewBox="0 0 1280 720">' + MESH_STRONG + '</symbol>'
               '<linearGradient id="yg" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#FFF3C4" stop-opacity=".2"/><stop offset=".35" stop-color="#FFE38A" stop-opacity=".75"/><stop offset=".75" stop-color="#FFC400"/><stop offset="1" stop-color="#FFC000"/></linearGradient>'
               '</defs>')


def logo():
    return '<div class="logo"><div class="ic">%s</div><p>§LOGO§</p></div>' % I['logo']


def photo(name, alt_key):
    return '<img class="photo" alt="§%s§" src="/templates/freeform/%s/assets/%s.jpg">' % (alt_key, KEY, name)


PIN = '<svg class="shape" viewBox="0 0 80 95" fill="currentColor" aria-hidden="true"><path d="M40 0a40 40 0 0 1 40 40c0 24-24 38-40 55C24 78 0 64 0 40A40 40 0 0 1 40 0z"/></svg>'


def arrows():
    polys = []
    for i in range(4):
        x0 = i * 300; x1 = x0 + 310
        pts = [(x0, 0), (x1 - 32, 0), (x1, 49), (x1 - 32, 98), (x0, 98)]
        if i: pts.append((x0 + 32, 49))
        polys.append('<polygon points="%s" fill="url(#yg)"/>' % ' '.join('%d,%d' % p for p in pts))
    return '<svg class="arrows" viewBox="0 0 1200 98" aria-hidden="true">%s</svg>' % ''.join(polys)


DECO11 = ('<svg class="deco" viewBox="0 0 1280 720" aria-hidden="true">'
          '<rect x="778" y="28" width="290" height="192"/><rect x="1085" y="28" width="300" height="192"/>'
          '<path d="M778 598 A270 270 0 0 1 1048 328 M778 598 H1068 M1068 598 V480"/>'
          '<path d="M1085 598 A270 270 0 0 1 1355 328 M1085 598 H1280 M1085 480 V598"/>'
          '<path d="M1215 410 A200 200 0 0 0 1255 310"/>'
          '</svg>')

SLIDES = [
    # 1 الغلاف
    poly() + logo()
    + '<h1 class="big t1">§S1_T1§</h1><h1 class="big t2">§S1_T2§</h1><p class="abs t3">§S1_T3§</p>'
    + '<div class="abs dash"></div><p class="abs note">§S1_NOTE§</p><p class="abs date">§S1_DATE§</p>',
    # 2 الملخص التنفيذي
    poly() + '<h1 class="ttl">§S2_T§</h1><div class="abs band"></div><div class="abs box"></div>'
    + '<p class="abs tab tab1">§S2_TAB1§</p><p class="abs tab tab2">§S2_TAB2§</p>'
    + '<p class="abs bt bt1">§S2_BT1§</p><p class="abs bt bt2">§S2_BT2§</p>'
    + ''.join('<div class="abs stat st%d"><p class="n">§S2_N%d§</p><h2 class="h">§S2_H%d§</h2><p class="p">§S2_P§</p></div>' % (i, i, i) for i in (1, 2, 3))
    + '<div class="vline vl1"></div><div class="vline vl2"></div>',
    # 3 لماذا يهمّ
    poly() + photo('presenter', 'S3_ALT') + '<h1 class="ttl">§S3_T§</h1><p class="sub">§SUBHEAD§</p><p class="abs p lead">§S3_LEAD§</p>'
    + '<div class="abs panel"></div>'
    + ''.join('<div class="abs col c%d"><div class="ring"><div class="ic">%s</div></div><h2 class="h">§S3_H%d§</h2><p class="p">§S3_P§</p></div>' % (i, I[ic], i) for i, ic in ((1, 'brain'), (2, 'chev'), (3, 'smile')))
    + '<div class="vline vl1"></div><div class="vline vl2"></div>',
    # 4 التحديات
    poly() + '<h1 class="ttl">§S4_T§</h1><p class="sub">§SUBHEAD§</p>' + photo('meeting', 'S4_ALT')
    + '<div class="abs box"><div class="ic">%s</div></div>' % I['venn']
    + ''.join('<div class="abs it i%d"><p class="num">0%d.</p><div><h2 class="h">§S4_H%d§</h2><p class="p">§S4_P§</p></div></div>' % (i, i, i) for i in (1, 2, 3, 4))
    + '<div class="hline hl1"></div><div class="hline hl2"></div>',
    # 5 إطار الحلول
    poly() + '<h1 class="ttl">§S5_T§</h1><p class="sub">§SUBHEAD§</p>' + photo('team', 'S5_ALT')
    + ''.join('<div class="abs card k%d"><div class="ic">%s</div><h2 class="h">§S5_H%d§</h2><p class="p">§S5_P§</p></div>' % (i, I[ic], i) for i, ic in ((1, 'brain'), (2, 'chev'), (3, 'smile'), (4, 'star'))),
    # 6 القدرات
    poly() + '<div class="abs panel"></div>'
    + ''.join('<div class="abs it i%d"><p class="num">0%d</p><div><h2 class="h">§S6_H%d§</h2><p class="p">§S6_P§</p></div></div>' % (i, i, i) for i in (1, 2, 3, 4))
    + '<div class="hline hl1"></div><div class="hline hl2"></div><div class="hline hl3"></div>'
    + '<h1 class="ttl">§S6_T§</h1><p class="abs p lead">§S6_LEAD§</p>' + photo('robot-hand', 'S6_ALT'),
    # 7 خارطة الطريق
    poly() + '<h1 class="ttl">§S7_T§</h1><p class="abs p lead">§S7_LEAD§</p>' + arrows()
    + ''.join('<div class="abs pin p%d">%s<div class="ic">%s</div></div>' % (i, PIN, I[ic]) for i, ic in ((1, 'torch'), (2, 'puzzle'), (3, 'people'), (4, 'bulb')))
    + ''.join('<p class="abs yr x%d">%d</p><div class="abs st x%d"><h2 class="h">§S7_H%d§</h2><p class="p">§S7_P§</p></div>' % (i, 2025 + i, i, i) for i in (1, 2, 3, 4)),
    # 8 الأثر
    poly() + '<h1 class="ttl">§S8_T§</h1><p class="sub">§SUBHEAD§</p>'
    + '<div class="abs card c1"><div class="ic">%s</div><p class="n">91%%</p><h2 class="h">§S8_H1§</h2><p class="p">§S8_P§</p></div>' % I['smile-o']
    + '<div class="abs card c2"><div class="m m1"><div class="ic">%s</div><div><p class="n">42%%</p><h2 class="h">§S8_H2§</h2></div></div><div class="hline"></div><div class="m m2"><div class="ic">%s</div><div><p class="n">28%%</p><h2 class="h">§S8_H3§</h2></div></div></div>' % (I['money'], I['coins'])
    + '<div class="abs card c3"><h2 class="h">§S8_H4§</h2><ul>' + ''.join('<li>§S8_B%d§</li>' % i for i in range(1, 7)) + '</ul></div>',
    # 9 المقاييس
    poly() + '<h1 class="ttl">§S9_T§</h1><p class="sub">§SUBHEAD§</p><div class="abs panel"></div><h2 class="abs h ct">§S9_CT§</h2>'
    + '<div class="chart"><svg id="chart1" viewBox="0 0 600 335" direction="ltr" aria-hidden="true"></svg></div>'
    + ''.join('<div class="abs kpi k%d"><p class="n">§S9_N%d§</p><h2 class="h">§S9_H%d§</h2><p class="p">§S9_P§</p></div>' % (i, i, i) for i in (1, 2))
    + '<div class="hline"></div>',
    # 10 لماذا تختار
    poly() + '<h1 class="ttl">§S10_T§</h1><p class="sub">§SUBHEAD§</p>'
    + ''.join('<div class="abs col c%d"><div class="ring"><div class="ic">%s</div></div><h2 class="h">§S10_H%d§</h2><p class="p">§S10_P§</p></div>' % (i, I[ic], i) for i, ic in ((1, 'brain'), (2, 'atom'), (3, 'cycle')))
    + '<div class="vline vl1"></div><div class="vline vl2"></div>'
    + '<div class="abs bar"><div class="ic">%s</div><div class="vline"></div><p class="p">§S10_BAR§</p></div>' % I['torch'],
    # 11 الاقتباس
    DECO11 + '<p class="abs lbl">§S11_LBL§</p>'
    + '<svg class="abs qm" viewBox="0 0 86 70" fill="currentColor" aria-hidden="true"><path d="M0 70V36C0 16 10 4 32 0l6 12c-10 3-16 9-17 18h17v40zm48 0V36c0-20 10-32 32-36l6 12c-10 3-16 9-17 18h17v40z"/></svg>'
    + '<p class="abs q">§S11_Q§</p><div class="abs qline"></div><p class="abs who">§S11_WHO§</p><p class="abs role">§S11_ROLE§</p>',
    # 12 الشكر
    poly(True) + logo() + '<h1 class="abs big">§S12_T§</h1><p class="abs btn">§S12_BTN§</p>'
    + ''.join('<div class="abs ct ct%d"><div class="ic">%s</div><p>§S12_C%d§</p></div>' % (i, I[ic], i) for i, ic in ((1, 'mail'), (2, 'phone'), (3, 'pin'))),
]

JSON_BLOCKS = {'chart-data': {
    'ar': {'categories': ['x', 'y', 'z'], 'max': 5, 'step': 0.5, 'series': [
        {'name': 'السلسلة 1', 'color': '#B388FF', 'values': [4.3, 2.5, 3.5]},
        {'name': 'السلسلة 2', 'color': '#F5F5F5', 'values': [2.4, 4.4, 1.8]},
        {'name': 'السلسلة 3', 'color': '#FFC000', 'values': [2.0, 2.0, 3.0]}]},
    'en': {'categories': ['x', 'y', 'z'], 'max': 5, 'step': 0.5, 'series': [
        {'name': 'Series 1', 'color': '#B388FF', 'values': [4.3, 2.5, 3.5]},
        {'name': 'Series 2', 'color': '#F5F5F5', 'values': [2.4, 4.4, 1.8]},
        {'name': 'Series 3', 'color': '#FFC000', 'values': [2.0, 2.0, 3.0]}]},
}}

EXTRA_SCRIPT = """  // رسم الأعمدة المجمّعة من chart-data (الفئة الأولى في جهة البداية)
  function drawChart() {
    var svg = document.getElementById('chart1'); if (!svg) return;
    var d = JSON.parse(document.getElementById('chart-data').textContent);
    var W = 600, H = 335, L = RTL ? 20 : 52, R = RTL ? 548 : 580, T = 18, B = 295, ax = RTL ? R : L;
    var ns = 'http://www.w3.org/2000/svg', out = [];
    function el(n, a, t) { var e = document.createElementNS(ns, n); for (var k in a) e.setAttribute(k, a[k]); if (t != null) e.textContent = t; svg.appendChild(e); }
    var y = function (v) { return B - (v / d.max) * (B - T); };
    for (var v = 0; v <= d.max + 1e-9; v += d.step) {
      el('line', { x1: L, x2: R, y1: y(v), y2: y(v), stroke: 'rgba(255,255,255,.25)', 'stroke-width': 1 });
      el('text', { x: RTL ? R + 10 : L - 10, y: y(v) + 4, 'text-anchor': RTL ? 'start' : 'end' }, String(Math.round(v * 10) / 10));
    }
    el('line', { x1: ax, x2: ax, y1: T, y2: B, stroke: '#fff', 'stroke-width': 1.2 });
    el('line', { x1: L, x2: R, y1: B, y2: B, stroke: '#fff', 'stroke-width': 1.2 });
    var n = d.categories.length, gw = (R - L) / n, bw = 44, gap = 2, k = d.series.length;
    d.categories.forEach(function (c, i) {
      var gi = RTL ? n - 1 - i : i, cx = L + gw * gi + gw / 2, x0 = cx - (k * bw + (k - 1) * gap) / 2;
      d.series.forEach(function (s, j) {
        var jj = RTL ? k - 1 - j : j, val = s.values[i];
        el('rect', { x: x0 + jj * (bw + gap), y: y(val), width: bw, height: B - y(val), fill: s.color });
      });
      el('text', { x: cx, y: B + 22, 'text-anchor': 'middle' }, c);
      el('line', { x1: cx + (RTL ? -gw / 2 : gw / 2), x2: cx + (RTL ? -gw / 2 : gw / 2), y1: B, y2: B + 6, stroke: '#fff', 'stroke-width': 1 });
    });
  }
  drawChart();"""

SUB = {'ar': 'أضف عنوانًا فرعيًا هنا', 'en': 'Add subheading here'}
STR = {
    'ar': {
        'SUBHEAD': SUB['ar'], 'LOGO': 'شعار شركتك هنا',
        'S1_T1': 'النمو المدعوم', 'S1_T2': 'بالذكاء الاصطناعي', 'S1_T3': 'قالب عرض تقديمي',
        'S1_NOTE': 'هذا نص توضيحي. أدرج النص المطلوب هنا. أضف النص هنا', 'S1_DATE': '29 أغسطس 2035',
        'S2_T': 'قالب شريحة الملخص التنفيذي', 'S2_TAB1': 'نظرة عامة على المشروع', 'S2_TAB2': 'رؤية الشركة',
        'S2_BT1': 'أضف النص هنا. أدرج النص المطلوب هنا. هذا نص توضيحي. أدرج النص المطلوب هنا. هذا نص توضيحي.',
        'S2_BT2': 'أضف نص الرؤية هنا. أدرج النص المطلوب هنا. هذا نص توضيحي. أدرج نصك.',
        'S2_N1': '96%', 'S2_N2': '42%', 'S2_N3': '68%',
        'S2_H1': 'جاهزية تبنّي الذكاء الاصطناعي', 'S2_H2': 'خفض التكاليف التشغيلية', 'S2_H3': 'اتخاذ قرارات أسرع',
        'S2_P': 'هذا نص توضيحي. أدرج النص المطلوب هنا.',
        'S3_T': 'لماذا يهمّ الذكاء الاصطناعي', 'S3_ALT': 'مدرّب يقف أمام جدار من الطوب ويشرح لجمهوره',
        'S3_LEAD': 'أضف النص هنا. أدرج النص المطلوب هنا. هذا نص بسيط. أدرج النص المطلوب هنا.',
        'S3_H1': 'قرارات أعمال أذكى', 'S3_H2': 'أتمتة أسرع للعمليات', 'S3_H3': 'تجربة عملاء أفضل',
        'S3_P': 'أضف النص هنا. أدرج النص المطلوب هنا.',
        'S4_T': 'التحديات الحالية', 'S4_ALT': 'اجتماع عمل بين زميلين حول طاولة',
        'S4_H1': 'جزر البيانات', 'S4_H2': 'سير عمل يدوي', 'S4_H3': 'قرارات بطيئة', 'S4_H4': 'منافسة متصاعدة',
        'S4_P': 'أضف النص هنا. أدرج النص المطلوب هنا. هذا نص بسيط.',
        'S5_T': 'إطار حلول الذكاء الاصطناعي', 'S5_ALT': 'فريق عمل يتناقش حول طاولة في مكتب حديث',
        'S5_H1': 'الاستكشاف', 'S5_H2': 'التحليل', 'S5_H3': 'الأتمتة', 'S5_H4': 'التوسّع',
        'S5_P': 'أدرج النص المطلوب هنا. أضف نصك هنا. هذا نص توضيحي.',
        'S6_T': 'القدرات الأساسية للنمو بالذكاء الاصطناعي', 'S6_ALT': 'يد روبوت بيضاء تشير على خلفية فاتحة',
        'S6_H1': 'تعلّم الآلة', 'S6_H2': 'التحليلات التنبؤية', 'S6_H3': 'اللغة الطبيعية', 'S6_H4': 'أتمتة العمليات',
        'S6_P': 'هذا نص توضيحي. أدرج النص المطلوب هنا. أضف النص هنا.',
        'S6_LEAD': 'هذا نص توضيحي. أدرج النص المطلوب هنا. هذا نص بسيط. أضف نصك هنا.',
        'S7_T': 'شريحة خارطة طريق التنفيذ', 'S7_LEAD': 'هذا نص توضيحي. أدرج النص المطلوب هنا. هذا نص بسيط. أضف النص هنا.',
        'S7_H1': 'التأسيس', 'S7_H2': 'التكامل', 'S7_H3': 'التوسّع', 'S7_H4': 'الابتكار',
        'S7_P': 'هذا نص توضيحي. أدرج النص المطلوب هنا.',
        'S8_T': 'شريحة الأثر على الأعمال', 'S8_H1': 'رضا العملاء', 'S8_H2': 'نمو الإيرادات', 'S8_H3': 'خفض التكاليف', 'S8_H4': 'أضف النص هنا',
        'S8_P': 'هذا نص توضيحي. أدرج النص المطلوب هنا.',
        'S8_B1': 'هذه نقطة توضيحية', 'S8_B2': 'أضف محتوى النقطة هنا', 'S8_B3': 'هذا نص توضيحي', 'S8_B4': 'أضف النص المطلوب هنا',
        'S8_B5': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف النص هنا', 'S8_B6': 'أضف محتوى نقطتك هنا',
        'S9_T': 'شريحة مقاييس النجاح', 'S9_CT': 'أضف عنوان الرسم هنا',
        'S9_N1': '45%', 'S9_N2': '3.6×', 'S9_H1': 'دقة النموذج', 'S9_H2': 'سرعة سير العمل',
        'S9_P': 'أدرج النص المطلوب هنا. أضف النص المطلوب هنا.',
        'S10_T': 'لماذا تختار الذكاء الاصطناعي', 'S10_H1': 'رؤى ذكية', 'S10_H2': 'ذكاء تكيّفي', 'S10_H3': 'أتمتة العمليات',
        'S10_P': 'أضف النص هنا. أدرج النص المطلوب هنا. أدرج النص المطلوب هنا.',
        'S10_BAR': 'أضف النص هنا. أدرج النص المطلوب هنا. هذا نص بسيط. أدرج النص المطلوب هنا. أضف النص هنا. أدرج النص المطلوب هنا. هذا نص بسيط.',
        'S11_LBL': 'الرؤية المستقبلية / شريحة اقتباس',
        'S11_Q': 'ابنِ منظمة جاهزة للمستقبل تعزّز فيها الأنظمة الذكية الخبرة البشرية، وتبسّط العمليات، وتطلق نموًا مستدامًا للأعمال في كل إدارة.',
        'S11_WHO': 'سارة ويلي', 'S11_ROLE': 'المدير التنفيذي للتقنية',
        'S12_T': 'نقدّر وقتكم', 'S12_BTN': 'تواصل معنا', 'S12_C1': 'أضف بريدك الإلكتروني هنا', 'S12_C2': '+1 (234) 567-890', 'S12_C3': 'أضف عنوانك',
    },
    'en': {
        'SUBHEAD': SUB['en'], 'LOGO': 'Your Company Logo Here',
        'S1_T1': 'AI POWERED', 'S1_T2': 'GROWTH', 'S1_T3': 'Presentation Template',
        'S1_NOTE': 'This is a sample text. Insert your desired text here. Add text here', 'S1_DATE': '29 Aug 2035',
        'S2_T': 'Executive Summary Slide Template', 'S2_TAB1': 'Project Overview', 'S2_TAB2': 'Company Vision',
        'S2_BT1': 'Add text here. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text.',
        'S2_BT2': 'Add vision text here. Insert your desired text here. This is a sample text. Insert your text.',
        'S2_N1': '96%', 'S2_N2': '42%', 'S2_N3': '68%',
        'S2_H1': 'AI Adoption Readiness', 'S2_H2': 'Operational Cost Reduction', 'S2_H3': 'Faster Decision Making',
        'S2_P': 'This is a sample text. Insert your desired text here.',
        'S3_T': 'Why AI Matters', 'S3_ALT': 'A trainer standing by a brick wall presenting to an audience',
        'S3_LEAD': 'Add text here. Insert your desired text here. This is a simple text. Insert your desired text here.',
        'S3_H1': 'Smarter Business Decisions', 'S3_H2': 'Faster Process Automation', 'S3_H3': 'Better Customer Experience',
        'S3_P': 'Add text here. Insert your desired text here.',
        'S4_T': 'Current Challenges', 'S4_ALT': 'Two colleagues in a meeting across a table',
        'S4_H1': 'Data Silos', 'S4_H2': 'Manual Workflows', 'S4_H3': 'Slow Decisions', 'S4_H4': 'Rising Competition',
        'S4_P': 'Add text here. Insert your desired text here. This is a simple text.',
        'S5_T': 'AI Solution Framework', 'S5_ALT': 'A team discussing around a table in a modern office',
        'S5_H1': 'Discover', 'S5_H2': 'Analyze', 'S5_H3': 'Automate', 'S5_H4': 'Scale',
        'S5_P': 'Insert your desired text here. Add your text here. This is a sample text.',
        'S6_T': 'AI Growth Key Capabilities', 'S6_ALT': 'A white robotic hand pointing on a light background',
        'S6_H1': 'Machine Learning', 'S6_H2': 'Predictive Analytics', 'S6_H3': 'Natural Language', 'S6_H4': 'Process Automation',
        'S6_P': 'This is a sample text. Insert your desired text here. Add text here.',
        'S6_LEAD': 'This is a sample text. Insert your desired text here. This is a simple text. Add your text here.',
        'S7_T': 'Implementation Roadmap Slide', 'S7_LEAD': 'This is a sample text. Insert your desired text here. This is a simple text. Add text here.',
        'S7_H1': 'Foundation', 'S7_H2': 'Integration', 'S7_H3': 'Expansion', 'S7_H4': 'Innovation',
        'S7_P': 'This is a sample text. Insert your desired text here.',
        'S8_T': 'Business Impact Slide', 'S8_H1': 'Customer Satisfaction', 'S8_H2': 'Revenue Growth', 'S8_H3': 'Cost Reduction', 'S8_H4': 'Add text here',
        'S8_P': 'This is a sample text. Insert your desired text here.',
        'S8_B1': 'This is sample bullet point', 'S8_B2': 'Add bullet point content here', 'S8_B3': 'This is a sample text', 'S8_B4': 'Add your desired text here',
        'S8_B5': 'This is a sample text. Add your desired text here. Add text here', 'S8_B6': 'Add your bullet point content here',
        'S9_T': 'Success Metrics Slide', 'S9_CT': 'Add chart title here',
        'S9_N1': '45%', 'S9_N2': '3.6×', 'S9_H1': 'Model Accuracy', 'S9_H2': 'Workflow Speed',
        'S9_P': 'Insert your desired text here. Add desired text here.',
        'S10_T': 'Why Choose AI', 'S10_H1': 'Smart Insights', 'S10_H2': 'Adaptive Intelligence', 'S10_H3': 'Process Automation',
        'S10_P': 'Add text here. Insert your desired text here. Insert your desired text here.',
        'S10_BAR': 'Add text here. Insert your desired text here. This is a simple text. Insert your desired text here. Add text here. Insert your desired text here. This is a simple text.',
        'S11_LBL': 'Future Vision / Quote Slide',
        'S11_Q': 'Build a future-ready organization where intelligent systems enhance human expertise, streamline operations, and unlock sustainable business growth across every department.',
        'S11_WHO': 'Sarah Wily', 'S11_ROLE': 'CTO',
        'S12_T': 'We appreciate your time', 'S12_BTN': 'Get in Touch', 'S12_C1': 'Add your email here', 'S12_C2': '+1 (234) 567-890', 'S12_C3': 'Add your address',
    },
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'mgmt', 'icon': '🤖',
    'name': 'النمو المدعوم بالذكاء الاصطناعي', 'nameEn': 'AI Powered Growth',
    'description': '12 شريحة: غلاف، ملخص تنفيذي، لماذا يهمّ، التحديات، إطار الحلول، القدرات، خارطة طريق، الأثر، المقاييس، اقتباس، شكر',
    'descriptionEn': '12 slides: cover, summary, why AI, challenges, framework, capabilities, roadmap, impact, metrics, quote, thanks',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'الملخص التنفيذي (3 نسب)', 'titleEn': 'Executive Summary (3 stats)', 'needsData': True},
        {'title': 'لماذا يهمّ الذكاء الاصطناعي', 'titleEn': 'Why AI Matters'},
        {'title': 'التحديات الحالية', 'titleEn': 'Current Challenges'},
        {'title': 'إطار حلول الذكاء الاصطناعي', 'titleEn': 'AI Solution Framework'},
        {'title': 'القدرات الأساسية للنمو', 'titleEn': 'AI Growth Key Capabilities'},
        {'title': 'خارطة طريق التنفيذ', 'titleEn': 'Implementation Roadmap', 'needsData': True},
        {'title': 'الأثر على الأعمال', 'titleEn': 'Business Impact', 'needsData': True},
        {'title': 'مقاييس النجاح (رسم أعمدة)', 'titleEn': 'Success Metrics (bar chart)', 'needsData': True},
        {'title': 'لماذا تختار الذكاء الاصطناعي', 'titleEn': 'Why Choose AI'},
        {'title': 'الرؤية المستقبلية / اقتباس', 'titleEn': 'Future Vision / Quote'},
        {'title': 'الشكر والتواصل', 'titleEn': 'Thanks & Contact'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والصور وغيّر النصوص والأرقام فقط؛ أرقام رسم الأعمدة في شريحة المقاييس في كتلة JSON بالمعرّف chart-data (فئات وثلاث سلاسل) ويُرسم بالسكربت المضمّن.',
}
