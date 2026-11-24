# -*- coding: utf-8 -*-
# نموذج «نظرة عامة على استراتيجية الأعمال»: اثنتا عشرة شريحة بهوية بنفسجية (#3A0CA3) على أبيض مع زخرفة وردية باهتة أعلى الشريحة.
# الأشكال الزخرفية والموصلات والموجة والأهرام في SVG بإطار كامل (الصنف deco) يُعكس أفقيًا للعربية؛ النصوص HTML بخصائص منطقية.
KEY = 'sl-business-strategy-overview'
TITLE = {'ar': 'نظرة عامة على استراتيجية الأعمال', 'en': 'Business Strategy Overview'}
COMMENT = 'نموذج «نظرة عامة على استراتيجية الأعمال»: غلاف، جدول الأعمال، الملخص التنفيذي، سياق الأعمال، التحدي الاستراتيجي، الأهداف، عرض القيمة، الركائز، المبادرات (مخطط زمني)، مؤشرات الأداء (رسمان من chart-data وchart-data-2)، خارطة الطريق، والخطوات التالية. الأشكال في svg.deco تنعكس للعربية.'
VARS = '--ink: #0D0D0D; --ink-2: #2B2B2B; --accent: #3A0CA3; --purple: #3A0CA3; --blue: #0034A4; --pink: #FFF9FD; --lav: #F0ECFD; --gray-bg: #F4F3F9; --line: #D8D8D8; --muted: #3E3D57; --bg: #FFFFFF;'

FONTS = {'ar': '<link href="https://fonts.googleapis.com/css2?family=Tajawal:wght@300;400;500;700;800&display=swap" rel="stylesheet">',
         'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@300;400;600;700;800&display=swap" rel="stylesheet">'}
FONT_STACK = {'ar': "'Tajawal', 'Segoe UI', Tahoma, sans-serif", 'en': "'Open Sans', 'Segoe UI', Arial, sans-serif"}


def P(top, start, width=None, extra=''):
    s = 'top:%spx; inset-inline-start:%spx;' % (top, start)
    if width is not None:
        s += ' width:%spx;' % width
    return s + extra


def IMG(name, alt, cls, style=''):
    return '<img class="%s" src="/templates/freeform/%s/assets/%s.jpg" alt="§%s§" style="%s">' % (cls, KEY, name, alt, style)


# ---------- أيقونات ----------
def SV(body, fill=False, sw=1.6):
    if fill:
        return '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">' + body + '</svg>'
    return ('<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="%s" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">' % sw) + body + '</svg>'


I_CHECK = SV('<path d="M6 12.5l4 4 8-9"/>', sw=2.2)
I_PLUS = SV('<path d="M12 4v16M4 12h16"/>', sw=1.6)
I_BULB = SV('<path d="M9 17h6M9.5 20h5M12 5.5a5 5 0 0 0-3 9c.6.5 1 1.2 1 2v.5h4V16.5c0-.8.4-1.5 1-2a5 5 0 0 0-3-9z"/><path d="M12 1.5v1.5M4.5 4.5l1.1 1.1M19.5 4.5l-1.1 1.1M2 11.5h1.6M20.4 11.5H22"/>', sw=1.3)
I_SLIDERS = SV('<path d="M3.5 7h17M3.5 12h17M3.5 17h17"/><circle cx="9" cy="7" r="1.6" fill="#0637AE"/><circle cx="14" cy="12" r="1.6" fill="#0637AE"/><circle cx="9" cy="17" r="1.6" fill="#0637AE"/>', sw=1.2)
I_APERTURE = SV('<circle cx="12" cy="12" r="9"/><circle cx="12" cy="12" r="6"/><path d="M12 6l2.6 4.5M18 12h-5.2M15 17.2l-2.6-4.5M9 17.2l2.6-4.5M6 12h5.2M9 6.8l2.6 4.5"/>', sw=1)
I_LIGHT = SV('<path d="M8 6.5l6-2v8l-6-2z"/><path d="M14 5.5h2v6h-2"/><path d="M5.5 6.5L3 5M5.5 8.5H3M5.5 10.5L3 12"/><path d="M12 12.5v4M12 16.5l-4 4.5M12 16.5l4 4.5M12 16.5v4.5"/><circle cx="8" cy="21" r=".7"/><circle cx="16" cy="21" r=".7"/><circle cx="12" cy="21" r=".7"/>', sw=1.1)
I_CAMERA = SV('<rect x="3" y="6.5" width="18" height="13" rx="3"/><path d="M8.5 6.5l1.2-2h4.6l1.2 2"/><circle cx="12" cy="13" r="3.8"/><circle cx="12" cy="13" r="2.4"/><path d="M17.5 9.3h.8"/>', sw=1.1)
I_ROCKET = SV('<path d="M14.5 4.5c2.5-1.2 4.5-1.3 5-.8s.4 2.5-.8 5L13 14.5 9.5 11z"/><circle cx="15.3" cy="8.7" r="1.4"/><path d="M9.5 11L6.5 10.5 9 8h4M13 14.5l.5 3L16 15v-4"/><path d="M7.5 15c-1.8.4-2.8 2-3 4.5 2.5-.2 4.1-1.2 4.5-3"/>', sw=1.1)
I_ARROW = SV('<path d="M8 8l8 8M16 9.5V16H9.5"/>', sw=1.5)
# أيقونات الموجة (مصمتة بيضاء)
W_ICONS = [
    SV('<rect x="5" y="2" width="14" height="8.5" rx="1"/><path d="M11 10.5l1 1.6 1-1.6z"/><circle cx="6.5" cy="13.8" r="1.7"/><circle cx="17.5" cy="13.8" r="1.7"/><circle cx="12" cy="15" r="2"/><path d="M3 21v-1.6c0-1.5 1.5-2.6 3.5-2.6s3.5 1.1 3.5 2.6V21zM14 21v-1.6c0-1.5 1.5-2.6 3.5-2.6S21 17.9 21 19.4V21zM8.2 21.5V21c0-1.8 1.7-3.1 3.8-3.1s3.8 1.3 3.8 3.1v.5z"/><path d="M7 4.5h6M7 6.5h9M7 8.5h5" stroke="#3A0CA3" stroke-width=".7"/>', fill=True),
    SV('<path d="M9 5.5V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v1.5h-1.3V4.3h-3.4v1.2z"/><rect x="3" y="6.5" width="18" height="13" rx="1.2"/><path d="M3 12.3h18" stroke="#3A0CA3" stroke-width=".9"/>', fill=True),
    SV('<rect x="10" y="2" width="11" height="9" rx="1"/><path d="M12.5 11l-1.3 2 2.8-2z"/><circle cx="15.5" cy="6.4" r="1.6" fill="#3A0CA3"/><path d="M14.8 8.3h1.4" stroke="#3A0CA3" stroke-width=".7"/><circle cx="7.5" cy="14" r="2.5"/><path d="M2.5 21.5v-1.2c0-2 2.2-3.4 5-3.4s5 1.4 5 3.4v1.2z"/>', fill=True),
    SV('<rect x="11" y="2" width="9" height="8" rx="1"/><path d="M13 10l-1 1.8 2.6-1.8z"/><path d="M14.3 4.6c0-.9.7-1.3 1.3-1.3s1.3.4 1.3 1.2c0 1-1.2 1-1.2 2M15.6 8.1v.1" stroke="#3A0CA3" stroke-width=".9" fill="none"/><circle cx="6" cy="12.5" r="2.2"/><circle cx="12.5" cy="15" r="2.3"/><path d="M1.5 20v-1.2c0-1.7 2-3 4.5-3 1.2 0 2.2.3 3 .8-1 .7-1.6 1.6-1.6 2.7V20zM8.3 21.8v-1c0-1.9 1.9-3.3 4.2-3.3s4.2 1.4 4.2 3.3v1z"/>', fill=True),
    SV('<circle cx="7" cy="7" r="1.7"/><circle cx="12" cy="5.8" r="1.8"/><circle cx="17" cy="7" r="1.7"/><path d="M4.3 12.5V11c0-1.2 1.2-2 2.7-2s2.7.8 2.7 2v1.5zM9.2 11.5V10c0-1.4 1.3-2.3 2.8-2.3s2.8.9 2.8 2.3v1.5zM14.3 12.5V11c0-1.2 1.2-2 2.7-2s2.7.8 2.7 2v1.5z"/><path d="M2 18.5c0-3.6 4.5-5.7 10-5.7s10 2.1 10 5.7z"/>', fill=True),
]
LOGO = ('<svg class="logo" viewBox="0 0 56 60" aria-hidden="true"><path fill="currentColor" fill-rule="evenodd" '
        'd="M7 6C20 0 40 1 52 7c4 14 4 32-2 47-15 6-32 7-46 2C0 42 1 21 7 6zm9 10c-2 9-2 20 1 29 9 2 19 2 28-1 3-9 3-19 1-28-10-3-20-3-30 0z"/></svg>')


# الزخرفة الوردية الباهتة أعلى الشريحة: قطعة دائرة كبيرة مع شق أفقي علوي وشق عمودي (dx إزاحة أفقية)
def deco(dx):
    return ('<svg class="deco" viewBox="0 0 1280 720" aria-hidden="true"><g transform="translate(%d 0)">' % dx
            + '<path fill="#FFF9FD" d="M560,225 A444,444 0 0 1 630,0 L1400,0 L1400,225 Z"/>'
            + '<path fill="#FFFFFF" d="M873,22 L898,0 L1259,0 L1281,20 L1281,22 Z"/><rect fill="#FFFFFF" x="1054" y="22" width="51" height="204"/>'
            + '</g></svg>')


DECO12 = ('<svg class="deco" viewBox="0 0 1280 720" aria-hidden="true"><g transform="matrix(-1 0 0 0.943 940 0)">'
          '<path fill="#FFF9FD" d="M560,225 A444,444 0 0 1 630,0 L1400,0 L1400,225 Z"/>'
          '<path fill="#FFFFFF" d="M866,22 L894,0 L1100,0 L1100,22 Z"/></g></svg>')

STYLE = """  .frame { color: var(--ink); }
  .deco { position: absolute; inset: 0; width: 1280px; height: 720px; }
  html[dir="rtl"] .deco { transform: scaleX(-1); }
  .ttl { position: absolute; top: 98px; inset-inline-start: 73px; margin: 0; font-size: 52px; line-height: 57px; font-weight: 300; color: #000; letter-spacing: -0.3px; white-space: nowrap; }
  .ttl b { font-weight: 700; }
  html[lang="ar"] .ttl { font-weight: 400; letter-spacing: 0; }
  html[lang="ar"] .ttl b { font-weight: 800; }
  .t { position: absolute; margin: 0; }
  .p19 { font-size: 19px; line-height: 25.5px; color: var(--ink); }
  .p17 { font-size: 17px; line-height: 22.5px; color: var(--ink); }
  .h26 { font-size: 25px; line-height: 32px; font-weight: 700; color: #000; }
  .pu { color: var(--purple); }
  .ph { position: absolute; object-fit: cover; display: block; }
  /* 1 الغلاف */
  .s1 .band { position: absolute; top: 76px; inset-inline-start: 0; width: 1280px; height: 569px; background: #FAF9FE; }
  .s1 .logo { position: absolute; top: 111px; inset-inline-start: 89px; width: 72px; height: 78px; color: #000; }
  html[dir="rtl"] .s1 .logo { transform: scaleX(-1); }
  .s1 .lg-t { top: 129px; inset-inline-start: 193px; font-size: 17px; line-height: 23px; letter-spacing: 1.3px; text-transform: uppercase; width: 200px; }
  .s1 .cv { top: 241px; inset-inline-start: 89px; font-size: 53px; line-height: 57px; font-weight: 300; text-transform: uppercase; letter-spacing: -0.5px; white-space: nowrap; }
  .s1 .cv b { font-weight: 700; letter-spacing: -1px; }
  html[lang="ar"] .s1 .cv { font-weight: 400; } html[lang="ar"] .s1 .cv b { font-weight: 800; }
  .s1 .cv-sub { top: 365px; inset-inline-start: 91px; font-size: 22px; line-height: 28px; letter-spacing: 3.6px; text-transform: uppercase; }
  .s1 .bar { position: absolute; top: 430px; inset-inline-start: 88px; width: 53px; height: 6px; border-radius: 3px; background: var(--purple); }
  .s1 .pills { position: absolute; top: 549px; inset-inline-start: 86px; display: flex; gap: 10px; }
  .s1 .pills span { height: 52px; padding: 0 33px; border: 1px solid var(--purple); border-radius: 26px; display: flex; align-items: center; font-size: 18px; white-space: nowrap; }
  .s1 .cover { top: 75px; inset-inline-end: 78px; width: 480px; height: 569px; border-radius: 20px; }
  /* 2 جدول الأعمال */
  .ag { position: absolute; width: 560px; height: 120px; }
  .ag .num { position: absolute; top: 0; inset-inline-start: 0; font-size: 60px; line-height: 60px; font-weight: 300; color: var(--blue); letter-spacing: -1px; }
  .ag .sl { position: absolute; top: 35px; inset-inline-start: 60px; width: 50px; height: 58px; }
  html[dir="rtl"] .ag .sl { transform: scaleX(-1); }
  .ag .pg { position: absolute; top: 61px; inset-inline-start: 95px; font-size: 16.5px; line-height: 20px; white-space: nowrap; }
  .ag .h { position: absolute; top: 3px; inset-inline-start: 208px; margin: 0; font-size: 25px; line-height: 32px; font-weight: 700; color: var(--purple); white-space: nowrap; }
  .ag .p { position: absolute; top: 42px; inset-inline-start: 208px; width: 300px; margin: 0; font-size: 17px; line-height: 23px; }
  /* 3 الملخص التنفيذي */
  .s3 .team { top: 0; inset-inline-end: 0; width: 522px; height: 720px; }
  html[dir="rtl"] .s3 .team { transform: scaleX(-1); }
  .s3 .panel { position: absolute; top: 374px; inset-inline-start: 0; width: 1127px; height: 346px; background: #F2F2F2; border-start-end-radius: 66px; }
  .s3 .col { position: absolute; top: 434px; width: 300px; }
  .s3 .col .n { margin: 0; font-size: 52px; line-height: 56px; font-weight: 700; color: var(--purple); }
  .s3 .col h2 { margin: 42px 0 0; }
  .s3 .col p { margin: 14px 0 0; }
  /* 4 و8: كتلة «حرّر نصك هنا» والقائمة بعلامات */
  .eh { font-size: 29px; line-height: 36px; font-weight: 700; color: #808080; white-space: nowrap; }
  .eh span { color: var(--purple); }
  .chk { position: absolute; margin: 0; padding: 0; list-style: none; }
  .chk li { display: flex; align-items: center; gap: 18px; height: 37px; margin-bottom: 14px; font-size: 19.5px; color: #333; white-space: nowrap; }
  .chk li i { flex: none; width: 37px; height: 37px; border-radius: 9px; background: #fff; box-shadow: 0 8px 14px rgb(40 30 90 / .14); color: var(--purple); display: grid; place-items: center; }
  .chk li i svg { width: 17px; height: 17px; }
  .s4 .lst { position: absolute; top: 330px; inset-inline-start: 683px; width: 474px; margin: 0; padding: 0; list-style: none; }
  .s4 .lst li { height: 65px; margin-bottom: 10px; padding-top: 6px; border-bottom: 1px solid var(--line); font-size: 20px; line-height: 26px; white-space: nowrap; }
  .s4 .lst li:last-child { border-bottom: 0; }
  .s4 .lst li.pu { color: var(--purple); }
  /* 5 التحدي الاستراتيجي */
  .s5 .lbg { position: absolute; top: 0; inset-inline-start: 0; width: 505px; height: 720px; background: var(--gray-bg); }
  .s5 .laptop { top: 317px; inset-inline-start: 73px; width: 383px; height: 320px; }
  .plus { position: absolute; width: 42px; height: 42px; border-radius: 50%; background: #EEEEF2; box-shadow: 0 4px 10px rgb(0 0 0 / .14); color: #0C3293; display: grid; place-items: center; }
  .plus svg { width: 26px; height: 26px; }
  .s5 .pct { position: absolute; inset-inline-start: 1114px; width: 97px; height: 97px; border-radius: 50%; background: var(--purple); color: #EDE7FF; display: grid; place-items: center; font-size: 30px; font-weight: 700; }
  .s5 .sep { position: absolute; inset-inline-start: 544px; width: 668px; height: 1px; background: #D5D8DF; }
  /* 6 الأهداف الاستراتيجية */
  .s6 .hub { position: absolute; top: 383px; inset-inline-start: 73px; width: 223px; height: 198px; background: #fff; box-shadow: 0 12px 40px rgb(30 20 80 / .12); text-align: center; }
  .s6 .hub svg { position: absolute; top: 30px; inset-inline-start: 79px; width: 66px; height: 66px; color: var(--purple); }
  .s6 .hub p { position: absolute; top: 114px; inset-inline-start: 0; width: 223px; margin: 0; font-size: 21.5px; line-height: 29px; font-weight: 700; color: var(--purple); }
  .s6 .sq { position: absolute; width: 81px; height: 81px; color: #fff; display: grid; place-items: center; box-shadow: 0 10px 22px rgb(30 20 80 / .14); }
  .s6 .sq svg { width: 46px; height: 46px; }
  .s6 .ob { position: absolute; width: 300px; }
  .s6 .ob h2 { margin: 0; }
  .s6 .ob p { margin: 3px 0 0; }
  /* 7 عرض القيمة */
  .s7 .vc { position: absolute; top: 417px; width: 108px; height: 108px; border-radius: 50%; background: var(--purple); color: #fff; display: grid; place-items: center; }
  .s7 .vc svg { width: 60px; height: 60px; }
  .s7 .vt { position: absolute; width: 300px; text-align: center; }
  .s7 .vt h2, .s7 .vt p { margin: 0; }
  .s7 .vt.top p { margin-bottom: 13px; }
  .s7 .vt.bot h2 { margin-bottom: 4px; }
  /* 8 الركائز */
  .s8 .rn { position: absolute; width: 78px; height: 78px; border-radius: 50%; background: var(--purple); border: 2px solid #fff; box-shadow: 0 0 0 1px #8F78DC; color: #fff; display: grid; place-items: center; font-size: 28px; font-weight: 700; }
  /* 9 المبادرات */
  .s9 .pnl { position: absolute; top: 279px; inset-inline-start: 73px; width: 1142px; height: 378px; border-radius: 12px; background: #F2EEFD; }
  .s9 .bl { position: absolute; top: 300px; font-size: 17px; line-height: 22px; white-space: nowrap; }
  .s9 .bl::before { content: "•"; margin-inline-end: 10px; }
  .s9 .mo { position: absolute; top: 605px; width: 90px; text-align: center; font-size: 17px; line-height: 22px; color: #222; }
  .s9 .gb { position: absolute; border-radius: 7px; background: var(--purple); color: #fff; display: grid; place-items: center; font-size: 18px; font-weight: 700; white-space: nowrap; }
  .s9 .nt { position: absolute; font-size: 17px; line-height: 22px; color: var(--muted); white-space: nowrap; }
  /* 10 مؤشرات الأداء */
  .s10 .donut { position: absolute; top: 379px; inset-inline-start: 72px; width: 230px; height: 230px; filter: drop-shadow(0 4px 10px rgb(30 20 80 / .10)); }
  html[dir="rtl"] .s10 .donut { transform: scaleX(-1); }
  .s10 .dv { position: absolute; top: 474px; inset-inline-start: 127px; width: 120px; text-align: center; font-size: 32px; line-height: 40px; font-weight: 700; }
  .s10 .rk { position: absolute; top: 395px; inset-inline-start: 68px; width: 72px; height: 72px; border-radius: 50%; background: var(--purple); color: #fff; display: grid; place-items: center; }
  .s10 .rk svg { width: 40px; height: 40px; }
  html[dir="rtl"] .s10 .rk svg { transform: scaleX(-1); }
  .s10 .kp { position: absolute; top: 63px; inset-inline-start: 620px; width: 629px; height: 594px; border-radius: 18px; background: #EFEBFC; }
  .s10 .st { position: absolute; top: 107px; width: 188px; text-align: center; }
  .s10 .st b { display: block; font-size: 22px; line-height: 28px; color: var(--purple); }
  .s10 .st span { display: block; margin-top: 5px; font-size: 17px; line-height: 22px; }
  .s10 .vs { position: absolute; top: 113px; width: 0; height: 47px; border-inline-start: 1px dashed #A9A5B5; }
  .s10 .chart { position: absolute; top: 200px; inset-inline-start: 654px; width: 561px; height: 330px; }
  .s10 .hd { position: absolute; top: 556px; inset-inline-start: 654px; width: 559px; height: 0; border-top: 1px dashed #B5B1C3; }
  .s10 .big { position: absolute; top: 582px; inset-inline-end: 66px; font-size: 34px; line-height: 42px; font-weight: 700; color: var(--purple); }
  /* 11 خارطة الطريق */
  .s11 .tbg { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 463px; background: var(--gray-bg); }
  .s11 .tl { position: absolute; top: 461px; inset-inline-start: 130px; width: 895px; height: 2px; background: #DCDDE5; }
  .s11 .dot { position: absolute; top: 454px; width: 16px; height: 16px; border-radius: 50%; background: var(--purple); }
  .s11 .rc { position: absolute; width: 240px; }
  .s11 .rc h2, .s11 .rc p { margin: 0; }
  .s11 .rc .sub { position: absolute; top: 37px; font-size: 18px; line-height: 23px; white-space: nowrap; }
  .s11 .rc .yr { position: absolute; top: 152px; }
  .s11 .rc .bd { position: absolute; top: 199px; width: 240px; }
  .s11 .card { top: 295px; inset-inline-start: 348px; width: 271px; height: 367px; }
  .s11 .rc.on { color: #fff; } .s11 .rc.on h2 { color: #F2EEFF; } .s11 .rc.on .sub, .s11 .rc.on .bd { color: #E9E4FA; }
  .s11 .rc.on .bd { top: 213px; }
  .s11 .rc .yy { position: absolute; top: 107px; font-size: 62px; line-height: 64px; font-weight: 300; color: #EDE9FA; }
  /* 12 الخطوات التالية */
  .s12 .panel { top: 0; inset-inline-end: 0; width: 601px; height: 720px; }
  .s12 .ph-frame { position: absolute; top: 59px; inset-inline-start: 497px; width: 284px; height: 599px; border-radius: 44px; background: #1B1C1E; border: 3px solid #8E9196; }
  .s12 .btn { position: absolute; width: 4px; border-radius: 2px; background: #8E9196; }
  .s12 .phone { top: 67px; inset-inline-start: 505px; width: 270px; height: 583px; border-radius: 34px; }
  .s12 .notch { position: absolute; top: 77px; inset-inline-start: 598px; width: 83px; height: 22px; border-radius: 11px; background: #0B0B0C; }
  .s12 .ring { position: absolute; inset-inline-start: 820px; width: 90px; height: 90px; border-radius: 50%; border: 1.5px solid rgb(255 255 255 / .85); color: #fff; display: grid; place-items: center; font-size: 30px; font-weight: 700; }
  .s12 .ns { position: absolute; inset-inline-start: 954px; width: 262px; color: #fff; }
  .s12 .ns h2 { margin: 0; color: #fff; }
  .s12 .ns p { margin: 6px 0 0; color: #F1EEFB; }
  .s12 .arr { position: absolute; top: 646px; inset-inline-start: 1182px; width: 34px; height: 34px; border-radius: 50%; border: 1.5px solid #fff; color: #fff; display: grid; place-items: center; }
  .s12 .arr svg { width: 20px; height: 20px; }
  html[dir="rtl"] .s12 .arr svg { transform: scaleX(-1); }"""

# ---------- الشرائح ----------
S1 = ('<div class="band"></div>' + LOGO
      + '<p class="t lg-t">§LOGO§</p>'
      + '<h1 class="t cv">§CV1§<br><b>§CV2§</b></h1>'
      + '<p class="t cv-sub">§CV3§</p><div class="bar"></div>'
      + '<p class="t p19" style="%s">§P1§</p>' % P(464, 91, 500, ' font-size:20px; line-height:25px;')
      + '<div class="pills"><span>§PL1§</span><span>§PL2§</span><span>§PL3§</span></div>'
      + IMG('cover', 'ALT1', 'ph cover'))

SLASH = '<svg class="sl" viewBox="0 0 50 58" aria-hidden="true"><path d="M1 57L49 1" stroke="#3A0CA3" stroke-width="1.6" stroke-dasharray="5 4"/></svg>'


def ag(i):
    row, col = i % 3, i // 3
    return ('<div class="ag" style="%s"><span class="num">0%d.</span>%s<span class="pg">§PG%d§</span><h2 class="h">§EDIT§</h2><p class="p">§S_SHORT§</p></div>'
            % (P(238 + round(156.5 * row), 73 + 584 * col), i + 1, SLASH, i + 1))


S2 = deco(98) + '<h1 class="ttl">§T2§</h1>' + ''.join(ag(i) for i in range(6))

S3 = (IMG('team', 'ALT3', 'ph team') + '<div class="panel"></div>'
      + '<h1 class="ttl">§T3§</h1>'
      + '<p class="t p19" style="%s">§P3§</p>' % P(257, 73, 600)
      + ''.join('<div class="col" style="inset-inline-start:%dpx"><p class="n">0%d</p><h2 class="h26">§EDIT§</h2><p class="p17">§S_LONG§</p></div>' % (x, i + 1)
                for i, x in enumerate((81, 436, 790))))


def left_block(dy):
    return ('<h2 class="t eh" style="%s">§EH1§ <span>§EH2§</span></h2>' % P(364 + dy, 73)
            + '<p class="t p17" style="%s">§S_MID§</p>' % P(425 + dy, 73, 440)
            + '<ul class="chk" style="%s">' % P(535 + dy, 73)
            + ''.join('<li><i>%s</i>§CK%d§</li>' % (I_CHECK, k) for k in (1, 2, 3)) + '</ul>')


S4 = (deco(0) + '<h1 class="ttl">§T4§</h1>'
      + '<p class="t p19" style="%s">§P4§</p>' % P(257, 73, 480)
      + left_block(0)
      + '<p class="t p17" style="%s">§P4B§</p>' % P(179, 683, 490)
      + '<ul class="lst"><li>§L1§</li><li>§L2§</li><li class="pu">§L3§</li><li>§L4§</li><li>§L5§</li></ul>')

S5 = ('<div class="lbg"></div><h1 class="ttl">§T5§</h1>' + IMG('laptop', 'ALT5', 'ph laptop')
      + '<div class="plus" style="%s">%s</div>' % (P(296, 52), I_PLUS)
      + ''.join('<h2 class="t h26" style="%s">§EDIT§</h2><p class="t p17" style="%s">§S_LONG2§</p><div class="pct" style="top:%dpx">§PC%d§</div>'
                % (P(142 + round(192.5 * i), 567), P(186 + round(192.5 * i), 567, 460), 153 + round(192.5 * i), i + 1) for i in range(3))
      + '<div class="sep" style="top:302px"></div><div class="sep" style="top:494px"></div>')

CONN = ('<svg class="deco" viewBox="0 0 1280 720" aria-hidden="true" fill="none" stroke-width="1.3" stroke-dasharray="4 3">'
        '<path d="M296 462.5H437.5V409" stroke="#2D5BD0"/><path d="M296 477.5H884.5V409" stroke="#E3622B"/>'
        '<path d="M296 492.5H884.5V558" stroke="#465DF8"/><path d="M296 507.5H437.5V558" stroke="#3A0CA3"/></svg>')
SQ = [(328, 396, '#0637AE', I_SLIDERS), (328, 843, '#E3622B', I_APERTURE), (558, 396, '#3A0CA3', I_LIGHT), (558, 843, '#465DF8', I_CAMERA)]
S6 = ('<h1 class="ttl">§T6§</h1>' + '<p class="t p19" style="%s">§P6§</p>' % P(197, 73, 1110)
      + CONN + '<div class="hub">%s<p>§HUB§</p></div>' % I_BULB
      + ''.join('<div class="sq" style="%s background:%s">%s</div><div class="ob" style="%s"><h2 class="h26">§EDIT§</h2><p class="p17">§S_SHORT§</p></div>'
                % (P(t, s), c, ic, P(t - 2, s + 102)) for t, s, c, ic in SQ))


def wave():
    R, r, st, y = 135.5, 81.25, 216.75, 471
    cx = [208 + st * k for k in range(5)]
    d = 'M%.2f,%d' % (cx[0] - R, y)
    for k in range(5):  # الحافة الأولى من اليسار إلى اليمين
        rad = R if k % 2 == 0 else r
        sweep = 0 if k % 2 == 0 else 1
        d += ' A%.2f,%.2f 0 0 %d %.2f,%d' % (rad, rad, sweep, cx[k] + rad, y)
    d += ' L%.2f,%d' % (cx[4] + r, y)
    for k in range(4, -1, -1):  # الحافة الثانية عودة
        rad = r if k % 2 == 0 else R
        sweep = 1 if k % 2 == 0 else 0
        d += ' A%.2f,%.2f 0 0 %d %.2f,%d' % (rad, rad, sweep, cx[k] - rad, y)
    d += ' Z'
    return ('<svg class="deco" viewBox="0 0 1280 720" aria-hidden="true"><path d="%s" fill="#F0ECFD" stroke="#3A0CA3" stroke-width="1.6" stroke-dasharray="5 4"/></svg>' % d), cx


WAVE, WCX = wave()
S7 = (deco(0) + '<h1 class="ttl">§T7§</h1>' + WAVE
      + ''.join('<div class="vc" style="inset-inline-start:%dpx">%s</div>' % (round(c) - 54, W_ICONS[k]) for k, c in enumerate(WCX))
      + ''.join(('<div class="vt top" style="%s"><p class="p17">§S_SHORT§</p><h2 class="h26">§EDIT§</h2></div>' % P(281, round(c) - 150)) if k % 2 == 0 else
                ('<div class="vt bot" style="%s"><h2 class="h26">§EDIT§</h2><p class="p17">§S_SHORT§</p></div>' % P(582, round(c) - 150))
                for k, c in enumerate(WCX)))


def pyr(a, l, b, r):
    pts = '%d,%d %d,%d %d,%d %d,%d' % (a + l + b + r)
    return ('<polygon points="%s" fill="#A582F8" stroke="#fff" stroke-width="2" stroke-linejoin="round"/>' % pts
            + '<path d="M%d %dL%d %d" stroke="#fff" stroke-width="2"/>' % (a + b))


PYR = ('<svg class="deco" viewBox="0 0 1280 720" aria-hidden="true">'
       + pyr((910, 246), (777, 488), (910, 602), (1030, 465))
       + pyr((745, 432), (641, 620), (745, 657), (849, 620))
       + pyr((1088, 368), (948, 626), (1088, 673), (1228, 626)) + '</svg>')
S8 = (deco(127) + '<h1 class="ttl">§T8§</h1>' + '<p class="t p19" style="%s">§P8§</p>' % P(197, 73, 590)
      + left_block(-33) + PYR
      + ''.join('<div class="rn" style="%s">§R%d§</div>' % (P(cy - 39, cx - 39), k + 1) for k, (cx, cy) in enumerate(((683, 520), (856, 308), (1159, 503)))))

GRID = ('<svg class="deco" viewBox="0 0 1280 720" aria-hidden="true" stroke="#A9A5B5" stroke-width="1" stroke-dasharray="6 4">'
        + ''.join('<path d="M%.1f 358V588"/>' % (104.5 + 90 * k) for k in range(13)) + '</svg>')
S9 = ('<h1 class="ttl">§T9§</h1>' + '<p class="t p19" style="%s">§P6§</p>' % P(192, 73, 1110)
      + '<div class="pnl"></div>'
      + '<span class="bl" style="inset-inline-start:104px">§B1§</span><span class="bl" style="inset-inline-start:540px">§B2§</span><span class="bl" style="inset-inline-start:803px">§B3§</span>'
      + GRID
      + ''.join('<span class="mo" style="inset-inline-start:%dpx">§M%d§</span>' % (104 + 90 * k, k + 1) for k in range(12))
      + '<div class="gb" style="%s height:59px">§EDIT§</div>' % P(356, 104, 180)
      + '<div class="gb" style="%s height:58px">§ADD§</div>' % P(416, 284, 143)
      + '<div class="gb" style="%s height:58px">§ADD§</div>' % P(416, 430, 124)
      + '<div class="gb" style="%s height:57px">§EDIT§</div>' % P(476, 554, 360)
      + '<div class="gb" style="%s height:58px">§EDIT§</div>' % P(526, 914, 270)
      + '<span class="nt" style="%s">§N1§</span>' % P(440, 581)
      + '<span class="nt" style="%s">§N2§</span>' % P(498, 296))

S10 = ('<h1 class="ttl">§T10§</h1>' + '<p class="t p19" style="%s">§P10§</p>' % P(257, 73, 500)
       + '<svg class="donut" id="donut" viewBox="-115 -115 230 230" aria-hidden="true"></svg>'
       + '<div class="dv" id="donut-label">45%</div>'
       + '<div class="rk">%s</div>' % I_ROCKET
       + '<h2 class="t h26" style="%s">§EDIT§</h2>' % P(552, 308)
       + '<p class="t p17" style="%s">§S_SHORT§</p>' % P(588, 308, 300)
       + '<div class="kp"></div>'
       + '<div class="st" style="inset-inline-start:653px"><b>500K</b><span>§EDITN§</span></div>'
       + '<div class="st" style="inset-inline-start:839px"><b>80%</b><span>§EDITN§</span></div>'
       + '<div class="st" style="inset-inline-start:1027px"><b>120%</b><span>§EDITN§</span></div>'
       + '<div class="vs" style="inset-inline-start:841px"></div><div class="vs" style="inset-inline-start:1029px"></div>'
       + '<svg class="chart" id="chart" viewBox="0 0 561 330" aria-hidden="true"></svg>'
       + '<div class="hd"></div>'
       + '<p class="t p17" style="%s">§S_SHORT2§</p>' % P(579, 654, 345)
       + '<div class="big">$2400</div>')


def rcol(x, k, on=False):
    yr = '<p class="yy">2027</p>' if on else '<h2 class="h26 yr">%d</h2>' % (2025 + k)
    return ('<div class="rc%s" style="%s"><h2 class="h26">§EDIT§</h2><p class="sub">§SUBT§</p>%s<p class="p17 bd">§S_LONG11§</p></div>'
            % (' on' if on else '', P(335, x), yr))


S11 = ('<div class="tbg"></div><h1 class="ttl">§T11§</h1>' + '<div class="tl"></div>'
       + ''.join('<div class="dot" style="inset-inline-start:%dpx"></div>' % x for x in (74, 701, 989))
       + IMG('card-2027', 'ALT11', 'ph card')
       + rcol(73, 1) + rcol(700, 3) + rcol(988, 4)
       + '<div class="rc on" style="%s"><h2 class="h26">§EDIT§</h2><p class="sub">§SUBT§</p><p class="yy">2027</p><p class="p17 bd">§S_LONG11§</p></div>' % P(321, 372)
       + '<div class="plus" style="%s">%s</div>' % (P(274, 596), I_PLUS))

S12 = (DECO12 + IMG('panel', 'ALT12', 'ph panel')
       + '<h1 class="ttl" style="top:276px">§T12§</h1>' + '<p class="t p19" style="%s">§P12§</p>' % P(471, 73, 345)
       + '<div class="ph-frame"></div>'
       + ''.join('<div class="btn" style="%s height:%dpx"></div>' % (P(t, 493), h) for t, h in ((173, 28), (220, 46), (280, 46)))
       + '<div class="btn" style="%s height:66px"></div>' % P(250, 781)
       + IMG('phone', 'ALT12B', 'ph phone') + '<div class="notch"></div>'
       + ''.join('<div class="ring" style="top:%dpx">0%d</div><div class="ns" style="top:%dpx"><h2 class="h26">§EDIT§</h2><p class="p19">§S_SHORT3§</p></div>'
                 % (124 + round(174.5 * i), i + 1, 122 + round(174.5 * i)) for i in range(3))
       + '<div class="arr">%s</div>' % I_ARROW)

SLIDES = [S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12]

# ---------- النصوص ----------
SENT = {
    'This is a sample text.': 'هذا نص توضيحي.',
    'Insert your desired text here.': 'أضف النص المطلوب هنا.',
    'You can add sample text.': 'يمكنك إضافة نص توضيحي.',
    'Insert your sample text here.': 'أدخل نصك التوضيحي هنا.',
    'You can add text.': 'يمكنك إضافة نص.',
    'Insert your text here.': 'أدخل نصك هنا.',
}


def tr(en):
    import re
    parts = re.findall(r'[^.]+\.', en)
    assert ''.join(parts).replace(' ', '') == en.replace(' ', ''), en
    return ' '.join(SENT[p.strip()] for p in parts)


A, B, C, D = 'This is a sample text.', 'Insert your desired text here.', 'You can add sample text.', 'Insert your sample text here.'
EN_P = {
    'P1': ' '.join([A, B, C, A]),
    'P3': ' '.join([A, B, C, A, D, C, A]),
    'P4': ' '.join([A, B, C, A]),
    'P4B': ' '.join([A, B, C, A, D, C]),
    'P6': ' '.join([A, B, C, A, D, C, A, B, C]),
    'P8': ' '.join([A, B, C, A, D, C, A]),
    'P10': ' '.join([A, B, C, A, D, C]),
    'P12': ' '.join([A, B, C, A]),
    'S_SHORT': ' '.join([A, 'Insert your', 'desired text here.', 'You can add text.']).replace('Insert your desired', 'Insert your desired'),
    'S_SHORT2': ' '.join([A, B, C]),
    'S_SHORT3': ' '.join([A, B]),
    'S_LONG': ' '.join([A, B, C, A, D]),
    'S_LONG11': ' '.join([A, B, C, D]),
    'S_LONG2': ' '.join([A, B, C, A, D, C]),
    'S_MID': ' '.join([A, B, C, A, D, C]),
    'L1': ' '.join([A, B]), 'L2': ' '.join([C, A]), 'L3': ' '.join([A, D]), 'L4': ' '.join([C, A]),
    'L5': ' '.join([D, 'You can add text.']),
    'CK1': A, 'CK2': C, 'CK3': D,
    'N1': ' '.join([A, B]),
}
EN_P['S_SHORT'] = ' '.join([A, B, 'You can add text.'])

STR = {'en': dict(EN_P), 'ar': {k: tr(v) for k, v in EN_P.items()}}
# «You can add sample text» في المخطط الزمني بلا نقطة في الأصل
STR['en']['N2'] = 'You can add sample text'; STR['ar']['N2'] = 'يمكنك إضافة نص توضيحي'
STR['en']['B1'] = 'This is a sample text. Insert your text here.'; STR['ar']['B1'] = 'هذا نص توضيحي. أدخل نصك هنا.'
STR['en']['B2'] = 'You can add sample text.'; STR['ar']['B2'] = 'يمكنك إضافة نص توضيحي.'
STR['en']['B3'] = 'This is a sample text. Insert your text here.'; STR['ar']['B3'] = 'هذا نص توضيحي. أدخل نصك هنا.'

STR['en'].update({
    'LOGO': 'Your Company Logo Here', 'CV1': 'Business', 'CV2': 'Strategy Overview', 'CV3': 'Presentation Template',
    'PL1': 'Add Text', 'PL2': 'Edit Text Here', 'PL3': 'This is a sample text',
    'ALT1': 'Business team meeting around a table', 'ALT3': 'Executive team reviewing data together',
    'ALT5': 'Laptop showing an analytics dashboard', 'ALT11': 'Team discussing the roadmap',
    'ALT12': 'Presenter pointing at a dashboard', 'ALT12B': 'Hands writing notes on a desk',
    'T2': '<b>Agenda</b> Slide', 'T3': '<b>Executive Summary</b><br>Presentation Slide',
    'T4': '<b>Business Context</b><br>Presentation Slide', 'T5': '<b>Strategic</b><br><b>Challenge</b> Slide',
    'T6': '<b>Strategic Objectives</b> Slide', 'T7': '<b>Value Proposition</b><br>Presentation Slide',
    'T8': '<b>Strategic Pillars</b> Slide', 'T9': '<b>Key Initiatives</b> Slide',
    'T10': '<b>KPIs &amp; Success</b><br><b>Metrics</b> Slide', 'T11': '<b>Roadmap</b><br><b>&amp; Timeline</b> Slide',
    'T12': '<b>Next Steps</b><br><b>&amp; Governance</b><br>Slide',
    'EDIT': 'Edit Text Here', 'EDITN': 'Edit Text Here', 'ADD': 'Add Text', 'SUBT': 'This is a sample text',
    'EH1': 'Edit Your', 'EH2': 'Text Here', 'HUB': 'Edit Your<br>Text Here',
    'PC1': '15%', 'PC2': '24%', 'PC3': '8%', 'R1': 'I', 'R2': 'II', 'R3': 'III',
    'M1': 'JAN', 'M2': 'FEB', 'M3': 'MAR', 'M4': 'APR', 'M5': 'MAY', 'M6': 'JUN', 'M7': 'JUL', 'M8': 'AUG', 'M9': 'SEP', 'M10': 'OCT', 'M11': 'NOV', 'M12': 'DEC',
})
for i, pg in enumerate(('03', '14', '18', '22', '27', '29')):
    STR['en']['PG%d' % (i + 1)] = 'Page ' + pg
    STR['ar']['PG%d' % (i + 1)] = 'صفحة ' + pg
STR['ar'].update({
    'LOGO': 'شعار شركتك<br>هنا', 'CV1': 'نظرة عامة على', 'CV2': 'استراتيجية الأعمال', 'CV3': 'قالب عرض تقديمي',
    'PL1': 'أضف نصًا', 'PL2': 'حرّر النص هنا', 'PL3': 'هذا نص توضيحي',
    'ALT1': 'فريق أعمال في اجتماع حول طاولة', 'ALT3': 'فريق تنفيذي يراجع البيانات معًا',
    'ALT5': 'حاسوب محمول يعرض لوحة تحليلات', 'ALT11': 'فريق يناقش خارطة الطريق',
    'ALT12': 'مقدّم يشير إلى لوحة بيانات', 'ALT12B': 'يدان تدوّنان ملاحظات على مكتب',
    'T2': 'شريحة <b>جدول الأعمال</b>', 'T3': '<b>الملخص التنفيذي</b><br>شريحة عرض تقديمي',
    'T4': '<b>سياق الأعمال</b><br>شريحة عرض تقديمي', 'T5': 'شريحة <b>التحدي</b><br><b>الاستراتيجي</b>',
    'T6': 'شريحة <b>الأهداف الاستراتيجية</b>', 'T7': '<b>عرض القيمة</b><br>شريحة عرض تقديمي',
    'T8': 'شريحة <b>الركائز الاستراتيجية</b>', 'T9': 'شريحة <b>المبادرات الرئيسية</b>',
    'T10': 'شريحة <b>مؤشرات الأداء</b><br><b>ومقاييس النجاح</b>', 'T11': 'شريحة <b>خارطة الطريق</b><br><b>والجدول الزمني</b>',
    'T12': 'شريحة<br><b>الخطوات التالية</b><br><b>والحوكمة</b>',
    'EDIT': 'حرّر النص هنا', 'EDITN': 'حرّر النص هنا', 'ADD': 'أضف نصًا', 'SUBT': 'هذا نص توضيحي',
    'EH1': 'حرّر', 'EH2': 'نصك هنا', 'HUB': 'حرّر<br>نصك هنا',
    'PC1': '15%', 'PC2': '24%', 'PC3': '8%', 'R1': '1', 'R2': '2', 'R3': '3',
    'M1': 'يناير', 'M2': 'فبراير', 'M3': 'مارس', 'M4': 'أبريل', 'M5': 'مايو', 'M6': 'يونيو', 'M7': 'يوليو', 'M8': 'أغسطس', 'M9': 'سبتمبر', 'M10': 'أكتوبر', 'M11': 'نوفمبر', 'M12': 'ديسمبر',
})

JSON_BLOCKS = {
    'chart-data': {'ar': {'labels': ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو'], 'values': [46, 64, 81, 100, 62, 100], 'max': 100},
                   'en': {'labels': ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'], 'values': [46, 64, 81, 100, 62, 100], 'max': 100}},
    'chart-data-2': {'value': 45, 'secondary': 25},
}

EXTRA_SCRIPT = """  var NS = 'http://www.w3.org/2000/svg';
  function el(tag, attrs, parent) { var e = document.createElementNS(NS, tag); for (var k in attrs) e.setAttribute(k, attrs[k]); if (parent) parent.appendChild(e); return e; }
  // رسم الأعمدة الشهرية من chart-data (الفئة الأولى في جهة البداية)
  function drawChart() {
    var d = JSON.parse(document.getElementById('chart-data').textContent), svg = document.getElementById('chart');
    if (!svg) return; svg.innerHTML = '';
    var W = 561, base = 276, maxH = 259, bw = 90, step = 94.2, mx = d.max || Math.max.apply(null, d.values);
    d.values.forEach(function (v, i) {
      var h = Math.max(4, v / mx * maxH), x = i * step; if (RTL) x = W - x - bw;
      el('rect', { x: x.toFixed(1), y: (base - h).toFixed(1), width: bw, height: h.toFixed(1), rx: 12, fill: '#3A0CA3' }, svg);
      var t = el('text', { x: (x + bw / 2).toFixed(1), y: 320, 'text-anchor': 'middle', 'font-size': 17, fill: '#3E3D57', 'font-family': 'inherit' }, svg);
      t.textContent = d.labels[i];
    });
  }
  // الحلقة: نسبة الإنجاز (بنفسجي) ثم الجزء الثانوي (رمادي) ثم الباقي (أبيض)
  function drawDonut() {
    var d = JSON.parse(document.getElementById('chart-data-2').textContent), svg = document.getElementById('donut');
    if (!svg) return; svg.innerHTML = '';
    var R = 106, r = 57;
    function arc(a0, a1, fill) {
      var p = function (a, rad) { var t = (a - 90) * Math.PI / 180; return (rad * Math.cos(t)).toFixed(2) + ',' + (rad * Math.sin(t)).toFixed(2); };
      var big = a1 - a0 > 180 ? 1 : 0;
      el('path', { d: 'M' + p(a0, R) + ' A' + R + ',' + R + ' 0 ' + big + ' 1 ' + p(a1, R) + ' L' + p(a1, r) + ' A' + r + ',' + r + ' 0 ' + big + ' 0 ' + p(a0, r) + ' Z', fill: fill }, svg);
    }
    var a = d.value * 3.6, b = a + (d.secondary || 0) * 3.6;
    arc(b, 359.99, '#FFFFFF'); arc(0, a, '#3A0CA3'); if (d.secondary) arc(a, b, '#D9D9D9');
    el('circle', { cx: 0, cy: 0, r: r, fill: '#FFFFFF' }, svg);
    document.getElementById('donut-label').textContent = d.value + '%';
  }
  drawChart(); drawDonut();"""

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'mgmt', 'icon': '🧭',
    'name': 'نظرة عامة على استراتيجية الأعمال', 'nameEn': 'Business Strategy Overview',
    'description': 'اثنتا عشرة شريحة: غلاف، جدول أعمال، ملخص، سياق، تحدٍّ، أهداف، قيمة، ركائز، مبادرات، مؤشرات، خارطة طريق',
    'descriptionEn': 'Twelve slides: cover, agenda, summary, context, challenge, objectives, value, pillars, initiatives, KPIs, roadmap, steps',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'جدول الأعمال', 'titleEn': 'Agenda'},
        {'title': 'الملخص التنفيذي', 'titleEn': 'Executive Summary'},
        {'title': 'سياق الأعمال', 'titleEn': 'Business Context'},
        {'title': 'التحدي الاستراتيجي', 'titleEn': 'Strategic Challenge', 'needsData': True},
        {'title': 'الأهداف الاستراتيجية', 'titleEn': 'Strategic Objectives'},
        {'title': 'عرض القيمة', 'titleEn': 'Value Proposition'},
        {'title': 'الركائز الاستراتيجية', 'titleEn': 'Strategic Pillars'},
        {'title': 'المبادرات الرئيسية (مخطط زمني)', 'titleEn': 'Key Initiatives (Timeline)'},
        {'title': 'مؤشرات الأداء ومقاييس النجاح', 'titleEn': 'KPIs & Success Metrics', 'needsData': True},
        {'title': 'خارطة الطريق والجدول الزمني', 'titleEn': 'Roadmap & Timeline'},
        {'title': 'الخطوات التالية والحوكمة', 'titleEn': 'Next Steps & Governance'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص والأرقام فقط؛ أعمدة الأشهر في كتلة chart-data ونسبة الحلقة في chart-data-2.',
}
