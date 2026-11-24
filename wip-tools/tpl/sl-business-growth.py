# -*- coding: utf-8 -*-
# نموذج «استراتيجية نمو الأعمال»: اثنتا عشرة شريحة بالأزرق الزاهي وأشكال عضوية متموّجة: غلاف، ملخص تنفيذي، نظرة على الشركة،
# فرص السوق، تحديات الأعمال، الحلول، استراتيجية النمو، نظرة مالية، جدول زمني، النتائج الرئيسية، الخطوات التالية، وشكر.
KEY = 'sl-business-growth'
TITLE = {'ar': 'استراتيجية نمو الأعمال', 'en': 'Business Growth Strategy'}
COMMENT = 'نموذج «استراتيجية نمو الأعمال»: خلفيات بيضاء أو زرقاء بأشكال عضوية (SVG واحد لكل شريحة يُعكس في العربية)، بطاقات زرقاء فاتحة مدوّرة، أيقونات على بقع فاتحة، وصور فوتوغرافية بزوايا مدوّرة.'
VARS = ('--ink: #1A1A1A; --accent: #2D59F6; --blue: #2D59F6; --lt: #D5DEFD; --lt2: #E2E8FE; --card: #EEF2FE; --navy: #141B5C;'
        ' --deep: #0936D1; --gray: #6F6F78; --gray-2: #575A63; --bg: #FFFFFF;')

FONTS = {'en': '<link href="https://fonts.googleapis.com/css2?family=Arimo:wght@400;700&display=swap" rel="stylesheet">'}
FONT_STACK = {'en': "'Arimo', Arial, 'Helvetica Neue', sans-serif"}

STYLE = """  /* عناصر مشتركة */
  .deco { position: absolute; inset: 0; width: 1280px; height: 720px; }
  html[dir="rtl"] .deco { transform: scaleX(-1); }
  .sh { position: absolute; top: 57px; inset-inline-start: 93px; margin: 0; font-size: 26px; line-height: 31px; color: var(--blue); white-space: nowrap; }
  .ttl { position: absolute; top: 106px; inset-inline-start: 93px; margin: 0; font-size: 58px; font-weight: 700; line-height: 70px; letter-spacing: -1.7px; color: var(--ink); white-space: nowrap; }
  .txt { position: absolute; margin: 0; font-size: 20px; line-height: 25.5px; color: var(--gray); word-spacing: 3px; }
  .ph { position: absolute; object-fit: cover; }
  .ic svg { width: 100%; height: 100%; display: block; }
  .num { direction: ltr; unicode-bidi: isolate; }
  html[lang="ar"] .ttl { letter-spacing: 0; font-weight: 800; font-size: 54px; }
  html[lang="ar"] .sh { font-size: 25px; }
  html[lang="ar"] .txt { word-spacing: 0; line-height: 27px; }
  /* بطاقة مؤشر ببقعة أيقونة (الشريحتان 2 و10) */
  .kc { position: absolute; width: 369px; height: 383px; border-radius: 28px; background: var(--card); }
  .kc .blob { position: absolute; top: 35px; inset-inline-start: 110px; width: 132px; height: 138px; color: var(--lt); }
  .kc .blob > svg { position: absolute; inset: 0; width: 100%; height: 100%; }
  .kc .ic { position: absolute; top: 50px; inset-inline-start: 52px; width: 46px; height: 46px; color: var(--blue); }
  .kc .v { position: absolute; top: 200px; inset-inline-start: 0; width: 100%; margin: 0; text-align: center; font-size: 71px; font-weight: 700; line-height: 85px; letter-spacing: -1px; color: var(--ink); }
  .kc .l { position: absolute; top: 298px; inset-inline-start: 0; width: 100%; margin: 0; text-align: center; font-size: 23.5px; line-height: 29px; color: var(--gray); }
  /* الشعار */
  .logo { position: absolute; display: flex; align-items: center; gap: 27px; }
  .logo svg { width: 54px; height: 58px; flex: none; color: var(--navy); }
  .logo p { margin: 0; font-size: 18px; line-height: 23px; letter-spacing: 3.6px; color: var(--navy); }
  html[lang="ar"] .logo p { letter-spacing: 0; font-size: 19px; line-height: 25px; font-weight: 500; }

  /* 1) الغلاف */
  .s1 .logo { top: 99px; inset-inline-start: 104px; }
  .s1 .t1 { position: absolute; top: 241px; inset-inline-start: 116px; margin: 0; font-size: 71px; font-weight: 700; line-height: 92px; letter-spacing: -1.3px; color: var(--ink); white-space: nowrap; }
  .s1 .t2 { position: absolute; top: 324px; inset-inline-start: 112px; margin: 0; font-size: 134px; font-weight: 700; line-height: 165px; letter-spacing: -2.2px; color: var(--ink); white-space: nowrap; }
  .s1 .pill { position: absolute; top: 500px; inset-inline-start: 106px; width: 690px; height: 59px; margin: 0; border-radius: 10px; background: var(--blue); color: #fff; font-size: 46px; line-height: 59px; letter-spacing: 1.7px; padding-inline-start: 16px; white-space: nowrap; }
  .s1 .lead { position: absolute; top: 608px; inset-inline-start: 116px; margin: 0; font-size: 26.5px; line-height: 32px; color: #3E4A66; }
  html[lang="ar"] .s1 .t1 { font-size: 72px; font-weight: 800; letter-spacing: 0; top: 222px; line-height: 105px; }
  html[lang="ar"] .s1 .t2 { font-size: 132px; font-weight: 800; letter-spacing: 0; top: 315px; line-height: 180px; }
  html[lang="ar"] .s1 .pill { font-size: 40px; letter-spacing: 0; font-weight: 500; padding-inline-start: 22px; }

  /* 2) الملخص التنفيذي */
  .s2 .k1 { top: 212px; inset-inline-start: 77px; } .s2 .k2 { top: 212px; inset-inline-start: 464px; } .s2 .k3 { top: 212px; inset-inline-start: 850px; }
  .s2 .foot { position: absolute; top: 657px; inset-inline-start: 93px; margin: 0; font-size: 23.5px; line-height: 30px; color: #fff; white-space: nowrap; word-spacing: 2px; }
  html[lang="ar"] .s2 .foot { font-size: 22px; }

  /* 3) نظرة على الشركة */
  .s3 .sh { top: 137px; }
  .s3 .ttl { top: 184px; line-height: 62px; white-space: normal; width: 520px; }
  .s3 .yr { position: absolute; top: 376px; inset-inline-start: 91px; margin: 0; font-size: 94px; font-weight: 700; line-height: 113px; letter-spacing: -1px; color: var(--blue); }
  .s3 .txt { top: 518px; inset-inline-start: 93px; width: 456px; }
  .s3 .ph { top: 45px; inset-inline-start: 701px; width: 542px; height: 614px; border-radius: 34px; }
  html[lang="ar"] .s3 .ttl { line-height: 70px; top: 176px; }

  /* 4) فرص السوق */
  .s4 .ph { top: 0; inset-inline-start: 723px; width: 503px; height: 577px; border-end-start-radius: 50px; border-end-end-radius: 50px; }
  .it { position: absolute; }
  .it .ic { position: absolute; color: #fff; }
  .it h2 { position: absolute; margin: 0; font-size: 23.8px; font-weight: 700; line-height: 28px; white-space: nowrap; }
  .it .txt { color: #fff; }
  .s4 .it h2 { color: #fff; inset-inline-start: 190px; }
  .s4 .it .txt { inset-inline-start: 190px; width: 416px; color: #EEF2FF; }
  .s4 .i1 .ic { top: 328px; inset-inline-start: 97px; width: 48px; height: 58px; } .s4 .i1 h2 { top: 338px; } .s4 .i1 .txt { top: 380px; }
  .s4 .i2 .ic { top: 486px; inset-inline-start: 89px; width: 64px; height: 58px; } .s4 .i2 h2 { top: 495px; } .s4 .i2 .txt { top: 537px; }
  .rule { position: absolute; height: 1px; }
  .s4 .r1 { top: 461px; inset-inline-start: 60px; width: 610px; background: rgb(255 255 255 / .45); }
  .s4 .r2 { top: 638px; inset-inline-start: 60px; width: 1160px; background: rgb(255 255 255 / .45); }
  .s4 .foot { position: absolute; top: 666px; inset-inline-start: 93px; margin: 0; font-size: 18.5px; line-height: 24px; color: #fff; white-space: nowrap; }

  /* 5) تحديات الأعمال */
  .s5 { background: var(--blue); }
  .s5 .ph { top: 0; inset-inline-start: 0; width: 700px; height: 530px; }
  html[dir="rtl"] .s5 .ph { transform: scaleX(-1); }
  .s5 .panel { position: absolute; top: 0; inset-inline-start: 640px; width: 640px; height: 720px; background: var(--card); border-start-start-radius: 60px; border-end-start-radius: 60px; }
  .s5 .big { position: absolute; top: 405px; inset-inline-start: 93px; margin: 0; font-size: 57px; font-weight: 700; line-height: 63px; letter-spacing: -1.4px; color: #fff; }
  .s5 .lead { top: 556px; inset-inline-start: 93px; width: 446px; color: #C9D2FF; }
  .s5 .it .ic { inset-inline-start: 700px; width: 48px; height: 48px; color: var(--blue); }
  .s5 .it h2 { inset-inline-start: 792px; color: var(--ink); }
  .s5 .it .txt { inset-inline-start: 792px; width: 400px; color: var(--gray-2); }
  .s5 .i1 .ic { top: 84px; } .s5 .i1 h2 { top: 83px; } .s5 .i1 .txt { top: 125px; }
  .s5 .i2 .ic { top: 236px; } .s5 .i2 h2 { top: 235px; } .s5 .i2 .txt { top: 277px; }
  .s5 .i3 .ic { top: 389px; } .s5 .i3 h2 { top: 388px; } .s5 .i3 .txt { top: 430px; }
  .s5 .i4 .ic { top: 542px; } .s5 .i4 h2 { top: 540px; } .s5 .i4 .txt { top: 582px; }
  .s5 .rule { inset-inline-start: 783px; width: 437px; background: #A9B2E6; }
  .s5 .r1 { top: 205px; } .s5 .r2 { top: 357px; } .s5 .r3 { top: 510px; }
  html[lang="ar"] .s5 .big { letter-spacing: 0; font-weight: 800; line-height: 72px; top: 396px; }

  /* 6) الحلول */
  .sc { position: absolute; top: 224px; width: 242px; height: 251px; border-radius: 22px; background: var(--card); }
  .sc .blob { position: absolute; top: 24px; inset-inline-start: 82px; width: 76px; height: 78px; color: var(--lt); }
  .sc .blob > svg { position: absolute; inset: 0; width: 100%; height: 100%; }
  .sc .ic { position: absolute; top: 25px; inset-inline-start: 25px; width: 27px; height: 27px; color: var(--blue); }
  .sc h2 { position: absolute; top: 124px; inset-inline-start: 0; width: 100%; margin: 0; text-align: center; font-size: 19.5px; font-weight: 700; line-height: 24px; color: var(--navy); }
  .sc .txt { top: 161px; inset-inline-start: 26px; width: 190px; text-align: center; }
  .s6 .c1 { inset-inline-start: 83px; } .s6 .c2 { inset-inline-start: 337px; } .s6 .c3 { inset-inline-start: 590px; }
  .s6 .ph { top: 169px; inset-inline-start: 878px; width: 334px; height: 306px; border-radius: 28px; }
  .s6 .big { position: absolute; top: 537px; inset-inline-start: 112px; width: 480px; margin: 0; font-size: 53px; line-height: 62px; color: #fff; }
  .s6 .side { top: 561px; inset-inline-start: 750px; width: 456px; }
  html[lang="ar"] .s6 .big { word-spacing: 0; letter-spacing: 0; }
  html[lang="ar"] .s10 .big, html[lang="ar"] .s2 .foot, html[lang="ar"] .s4 .foot { letter-spacing: 0; word-spacing: 0; }
  html[lang="ar"] .s6 .big { font-weight: 500; line-height: 68px; top: 530px; }

  /* 7) استراتيجية النمو */
  .s7 .intro { top: 205px; inset-inline-start: 93px; width: 405px; }
  .gc { position: absolute; border-radius: 22px; background: var(--card); }
  .gc h2 { position: absolute; margin: 0; font-size: 23.8px; font-weight: 700; line-height: 28px; color: var(--ink); white-space: nowrap; }
  .gc .n { position: absolute; margin: 0; font-weight: 700; color: var(--blue); }
  .gc .txt { color: var(--gray-2); }
  .s7 .g1 { top: 303px; inset-inline-start: 83px; width: 498px; height: 363px; }
  .s7 .g1 .n { top: 25px; inset-inline-end: 35px; font-size: 52px; line-height: 62px; }
  .s7 .g1 .ic { position: absolute; top: 64px; inset-inline-start: 33px; width: 71px; height: 88px; color: var(--blue); }
  .s7 .g1 h2 { top: 205px; inset-inline-start: 42px; }
  .s7 .g1 .txt { top: 246px; inset-inline-start: 42px; width: 362px; }
  .s7 .g2, .s7 .g3, .s7 .g4 { inset-inline-start: 613px; width: 607px; height: 166px; }
  .s7 .g2 { top: 105px; } .s7 .g3 { top: 303px; } .s7 .g4 { top: 500px; }
  .s7 .g2 .n, .s7 .g3 .n, .s7 .g4 .n { top: 26px; inset-inline-start: 43px; font-size: 55px; line-height: 66px; }
  .s7 .g2 h2, .s7 .g3 h2, .s7 .g4 h2 { top: 39px; inset-inline-start: 135px; }
  .s7 .g2 .txt, .s7 .g3 .txt, .s7 .g4 .txt { top: 81px; inset-inline-start: 135px; width: 416px; }
  .s7 .g4 h2 { top: 43px; } .s7 .g4 .txt { top: 85px; }

  /* 8) نظرة مالية */
  .fc { position: absolute; top: 54px; width: 376px; height: 221px; border-radius: 18px; background: var(--card); text-align: center; }
  .s8 .f1 { inset-inline-start: 38px; } .s8 .f2 { inset-inline-start: 452px; } .s8 .f3 { inset-inline-start: 866px; }
  .fc .y { position: absolute; top: 30px; inset-inline-start: 0; width: 100%; margin: 0; font-size: 26px; line-height: 32px; color: var(--blue); }
  .fc .y.b { font-weight: 700; top: 33px; }
  .fc .v { position: absolute; top: 76px; inset-inline-start: 0; width: 100%; margin: 0; font-size: 52px; font-weight: 700; line-height: 64px; color: #000; }
  .fc .pill { position: absolute; top: 148px; inset-inline-start: 126px; width: 124px; height: 36px; margin: 0; border-radius: 18px; background: var(--blue); color: #fff; display: flex; align-items: center; justify-content: center; gap: 12px; font-size: 20.5px; }
  .fc .pill svg { width: 15px; height: 13px; flex: none; }
  .s8 .sh { top: 400px; inset-inline-start: 92px; color: #fff; }
  .s8 .ttl { top: 439px; inset-inline-start: 92px; color: #fff; }
  .s8 .txt { top: 549px; inset-inline-start: 92px; width: 508px; color: #E1E7FF; }
  .s8 .ph { top: 326px; inset-inline-start: 707px; width: 527px; height: 361px; border-radius: 30px; }

  /* 9) الجدول الزمني */
  .dot { position: absolute; width: 38px; height: 38px; border-radius: 50%; background: var(--navy); color: #fff; display: grid; place-items: center; font-size: 16px; font-weight: 700; }
  .s9 .d1 { top: 264px; inset-inline-start: 64px; } .s9 .d2 { top: 551px; inset-inline-start: 294px; } .s9 .d3 { top: 254px; inset-inline-start: 672px; } .s9 .d4 { top: 552px; inset-inline-start: 873px; }
  .s9 h2 { position: absolute; margin: 0; font-size: 24px; font-weight: 700; line-height: 28px; color: var(--ink); white-space: nowrap; }
  .s9 .txt { width: 220px; }
  .s9 .h1 { top: 269px; inset-inline-start: 123px; } .s9 .p1 { top: 311px; inset-inline-start: 123px; }
  .s9 .h2 { top: 556px; inset-inline-start: 352px; } .s9 .p2 { top: 598px; inset-inline-start: 352px; }
  .s9 .h3 { top: 259px; inset-inline-start: 730px; } .s9 .p3 { top: 301px; inset-inline-start: 730px; }
  .s9 .h4 { top: 557px; inset-inline-start: 931px; } .s9 .p4 { top: 599px; inset-inline-start: 931px; }

  /* 10) النتائج الرئيسية */
  .s10 .k1 { top: 243px; inset-inline-start: 77px; } .s10 .k2 { top: 243px; inset-inline-start: 464px; } .s10 .k3 { top: 243px; inset-inline-start: 850px; }
  .s10 .kc .blob { top: 36px; }
  .s10 .big { position: absolute; top: 75px; inset-inline-start: 768px; width: 470px; margin: 0; font-size: 47px; line-height: 56px; color: #fff; letter-spacing: .4px; }
  html[lang="ar"] .s10 .big { font-weight: 500; font-size: 44px; line-height: 62px; top: 66px; }

  /* 11) الخطوات التالية */
  .s11 { background: var(--blue); }
  .s11 .sh, .s11 .ttl { color: #fff; }
  .s11 .ph { top: 84px; inset-inline-start: 683px; width: 547px; height: 568px; border-radius: 28px; }
  .s11 .it .ic { inset-inline-start: 93px; width: 54px; height: 54px; }
  .s11 .it h2 { inset-inline-start: 175px; color: #fff; }
  .s11 .it .txt { inset-inline-start: 175px; width: 416px; color: #D3DCFF; }
  .s11 .i1 .ic { top: 236px; } .s11 .i1 h2 { top: 243px; } .s11 .i1 .txt { top: 284px; }
  .s11 .i2 .ic { top: 391px; } .s11 .i2 h2 { top: 398px; } .s11 .i2 .txt { top: 440px; }
  .s11 .i3 .ic { top: 551px; } .s11 .i3 h2 { top: 554px; } .s11 .i3 .txt { top: 595px; }
  .s11 .rule { inset-inline-start: 173px; width: 433px; background: rgb(255 255 255 / .3); }
  .s11 .r1 { top: 366px; } .s11 .r2 { top: 521px; }

  /* 12) الشكر */
  .s12 { background: var(--blue); }
  .s12 .ty { position: absolute; top: 189px; inset-inline-start: 119px; margin: 0; font-size: 74px; font-weight: 700; line-height: 90px; letter-spacing: -2.4px; color: #fff; white-space: nowrap; }
  .s12 .uline { position: absolute; top: 298px; inset-inline-start: 130px; width: 123px; height: 5px; border-radius: 3px; background: #fff; }
  .s12 .big { position: absolute; top: 189px; inset-inline-start: 733px; width: 420px; margin: 0; font-size: 43px; line-height: 46px; color: #fff; }
  .s12 .txt { top: 292px; inset-inline-start: 734px; width: 324px; color: #C8D3FF; }
  .s12 .logo { top: 630px; inset-inline-start: 111px; }
  .s12 .ct { position: absolute; top: 632px; display: flex; align-items: center; gap: 21px; margin: 0; font-size: 21px; line-height: 45px; color: #fff; white-space: nowrap; word-spacing: 2px; }
  .s12 .ct .bl { position: relative; width: 45px; height: 45px; flex: none; color: #EEF2FE; }
  .s12 .ct .bl > svg { position: absolute; inset: 0; width: 100%; height: 100%; }
  .s12 .ct .ic { position: absolute; top: 12px; inset-inline-start: 12px; width: 21px; height: 21px; color: var(--blue); }
  .s12 .c1 { inset-inline-start: 608px; } .s12 .c2 { inset-inline-start: 923px; }
  html[lang="ar"] .s12 .ty { letter-spacing: 0; font-weight: 800; font-size: 72px; line-height: 100px; top: 180px; }
  html[lang="ar"] .s12 .big { font-weight: 500; line-height: 60px; top: 175px; }
  html[lang="ar"] .s12 .txt { top: 300px; }
  html[lang="ar"] .s12 .big { width: 330px; }"""

# ---------- أيقونات (viewBox 24) ----------
def ic(body, stroke=None):
    if stroke:
        return '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="%s" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">%s</svg>' % (stroke, body)
    return '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">%s</svg>' % body


ICONS = {
    'user': ic('<circle cx="12" cy="7" r="4.2"/><path d="M4 21v-1.6C4 15.6 7.2 13.4 12 13.4s8 2.2 8 6V21z"/>'),
    'love': ic('<path d="M12 1.5a10.5 10.5 0 1 0 0 21 10.5 10.5 0 0 0 0-21zM6.4 8.6c.5-.8 1.6-.6 1.9.1.3-.7 1.4-.9 1.9-.1.5.9-.3 1.8-1.9 2.9-1.6-1.1-2.4-2-1.9-2.9zm7.4 0c.5-.8 1.6-.6 1.9.1.3-.7 1.4-.9 1.9-.1.5.9-.3 1.8-1.9 2.9-1.6-1.1-2.4-2-1.9-2.9zM12 18.6c-2.7 0-4.9-1.6-5.6-3.9h1.6c.6 1.5 2.2 2.4 4 2.4s3.4-.9 4-2.4h1.6c-.7 2.3-2.9 3.9-5.6 3.9z" fill-rule="evenodd"/>'),
    'cart': ic('<path d="M1 3h3.2l.8 3h16.5l-1.8 9.5H7.1l.4 1.6h12.3v1.6H6.2L3 4.6H1zM6.6 7.6l.4 1.6h3.2V7.6zm5.2 0v1.6h3.2V7.6zm4.8 0v1.6h3.3l.3-1.6zM7.5 10.8l.4 1.6h2.3v-1.6zm4.3 0v1.6h3.2v-1.6zm4.8 0v1.6h2.7l.3-1.6zM8.3 14h1.9v-.1H8.3zm3.5-.1v.1h3.2v-.1zm4.8 0v.1h2.2v-.1z"/><circle cx="8" cy="21" r="1.5"/><circle cx="17.8" cy="21" r="1.5"/>'),
    'finger': ic('<path d="M5.5 7.5C7 5 9.3 3.6 12 3.6s5 1.4 6.5 3.9"/><path d="M4.5 12c0-4.4 3.4-7.6 7.5-7.6s7.5 3.2 7.5 7.6c0 1.4-.2 2.8-.6 4"/><path d="M7.2 18.5c-.6-1.8-.9-3.5-.9-5.6 0-3.4 2.6-6 5.7-6s5.7 2.6 5.7 6c0 2.3-.3 4.4-1 6.4"/><path d="M9.8 21c-.8-2-1.2-4.4-1.2-7.2 0-2 1.5-3.6 3.4-3.6s3.4 1.6 3.4 3.6c0 2.6-.3 5-1 7.2"/><path d="M12 13.6c0 3 .4 5.4 1.2 7.4"/><path d="M3.6 9.3c-.5 1-.8 2-.8 3.2"/><path d="M20.4 9.3c.5 1 .8 2 .8 3.2"/>', 1.1),
    'fireworks': ic('<path d="M12 12V3.5M12 12l6-6M12 12h8.5M12 12l6 6M12 12v8.5M12 12l-6 6M12 12H3.5M12 12L6 6"/><path d="M12 12l3-7.5M12 12l7.5-3M12 12l7.5 3M12 12l3 7.5M12 12l-3 7.5M12 12l-7.5 3M12 12l-7.5-3M12 12L9 4.5"/><circle cx="2" cy="2.5" r=".6"/><circle cx="22" cy="2.5" r=".6"/><circle cx="2" cy="21.5" r=".6"/><circle cx="22" cy="21.5" r=".6"/>', 0.7),
    'trophy': ic('<path d="M6 2.5h12v2.2h3.5V7c0 2.8-2 5-4.6 5.4A6 6 0 0 1 13.2 15v2.5h3.3V21h-9v-3.5h3.3V15a6 6 0 0 1-3.7-2.6C4.5 12 2.5 9.8 2.5 7V4.7H6zm0 4.2H4.4V7c0 1.6 1 3 2.4 3.4A6 6 0 0 1 6 8zm12 0V8c0 .9-.2 1.7-.5 2.4 1.4-.4 2.4-1.8 2.4-3.4v-.3z"/>'),
    'coins': ic('<ellipse cx="9" cy="5.5" rx="7" ry="3"/><path d="M2 6.8v2.4c0 1.7 3.1 3 7 3s7-1.3 7-3V6.8c-1.3 1.3-4 2.1-7 2.1S3.3 8.1 2 6.8z"/><path d="M2 10.6V13c0 1.6 2.6 2.8 6.1 3-.1-.4-.1-.8-.1-1.2 0-.9.4-1.7 1.1-2.4L9 12.4c-3 0-5.7-.8-7-1.8z"/><ellipse cx="16" cy="14.5" rx="6.5" ry="2.8"/><path d="M9.5 15.8v2.4c0 1.6 2.9 2.8 6.5 2.8s6.5-1.2 6.5-2.8v-2.4c-1.2 1.2-3.7 2-6.5 2s-5.3-.8-6.5-2z"/><path d="M9.5 19.4v1c0 1.6 2.9 2.8 6.5 2.8s6.5-1.2 6.5-2.8v-1c-1.2 1.2-3.7 2-6.5 2s-5.3-.8-6.5-2z"/>'),
    'pulse': ic('<path d="M1.5 12.5h5l2-4.5 3 9 3-11 2.5 6.5h5.5"/>', 1.8),
    'brain': ic('<path d="M9 2.5c-1.6 0-2.9 1-3.3 2.4C3.9 5.2 2.5 6.8 2.5 8.7c0 .7.2 1.3.5 1.9-.7.7-1.1 1.6-1.1 2.6 0 1.8 1.3 3.3 3 3.7.3 1.8 1.9 3.1 3.8 3.1.4 0 .8-.1 1.2-.2l.6 2.7h2.2V2.9A3.9 3.9 0 0 0 9 2.5zm6 0c1.6 0 2.9 1 3.3 2.4 1.8.3 3.2 1.9 3.2 3.8 0 .7-.2 1.3-.5 1.9.7.7 1.1 1.6 1.1 2.6 0 1.8-1.3 3.3-3 3.7-.3 1.8-1.9 3.1-3.8 3.1-.4 0-.8-.1-1.2-.2l-.6 2.7h-1.7V2.9c.4-.3 1.6-.4 3.2-.4z"/><path d="M5.5 9.5c1.2 0 2 .7 2.5 1.6M7 14c.8-.6 1.8-.7 2.6-.4M15 6.5c-.2 1.2.3 2.2 1.3 2.8M17.8 12.3c-1 0-1.9.6-2.3 1.4M15.5 17.2c.4-.6.5-1.3.4-2" fill="none" stroke="#fff" stroke-width=".9" stroke-linecap="round"/>'),
    'crown': ic('<path d="M3 8.5l4.6 3.6L12 5.5l4.4 6.6L21 8.5 19.3 17H4.7z"/><rect x="4.7" y="18.2" width="14.6" height="2" rx=".5"/><circle cx="12" cy="3.4" r="1.3"/><circle cx="2.3" cy="7.3" r="1.2"/><circle cx="21.7" cy="7.3" r="1.2"/>'),
    'tree': '<svg viewBox="2.6 0.5 18.8 23.5" fill="currentColor" aria-hidden="true"><path d="M12 0.5l4.6 7.2h-2.5l4.4 6.3h-2.7l5.2 6.6H13.3V24h-2.6v-3.4H3l5.2-6.6H5.5l4.4-6.3H7.4z"/></svg>',
    'masks': ic('<path d="M10 3.5l9.5 1.2-.9 7.6c-.4 3.2-3.2 5.3-6 4.8-.3-.1-.7-.2-1-.3.3-.7.4-1.4.5-2.2l.7-6.3L10 8.6z"/><path d="M13.3 9.7a.9.9 0 1 0 1.5.2zM17.2 10.2a.9.9 0 1 0 1.5.2zM13.4 12.9c.8 1.4 3 1.7 4.2.5" fill="none" stroke="#fff" stroke-width="1"/><path d="M1.8 9.3l9.6-1.1.9 7.6c.4 3.2-1.9 5.9-4.8 6.2-2.9.3-5.4-2-5.8-5.2z"/><path d="M4.4 13.1a.9.9 0 1 0 1.4-.4zM8.3 12.6a.9.9 0 1 0 1.4-.4zM5.7 18.6c.9-1.4 3-1.7 4.2-.6" fill="none" stroke="#fff" stroke-width="1"/>'),
    'dollar': ic('<path d="M12 1.5v21M17 5.8c-1-1.3-2.8-2-5-2-2.9 0-5 1.5-5 3.8 0 5.4 10.2 2.8 10.2 8.5 0 2.4-2.2 4-5.2 4-2.4 0-4.4-.9-5.4-2.4"/>', 1.6),
    'hands': ic('<path d="M12 12.8l-1.6-4.6L8.5 2.6c-.2-.6.6-1 1-.5l2.5 4 2.5-4c.4-.5 1.2-.1 1 .5l-1.9 5.6zM11.3 13.1L6.4 13.2.6 12.8c-.6-.1-.7-.9-.1-1.1l4.4-1.6-2.9-3.7c-.4-.5.2-1.1.7-.8zM12.7 13.1l4.9.1 5.8-.4c.6-.1.7-.9.1-1.1l-4.4-1.6 2.9-3.7c.4-.5-.2-1.1-.7-.8zM11.4 14l-2.9 4-3.5 4.6c-.4.5.2 1.1.8.8l4-2.6.4 4.6c.1.6.9.6 1 0zM12.6 14l2.9 4 3.5 4.6c.4.5-.2 1.1-.8.8l-4-2.6-.4 4.6c-.1.6-.9.6-1 0z"/>'),
    'eye': ic('<path d="M12 5C6.5 5 2.4 9.3 1 12c1.4 2.7 5.5 7 11 7s9.6-4.3 11-7c-1.4-2.7-5.5-7-11-7zm0 2c4.2 0 7.4 3 8.7 5-1.3 2-4.5 5-8.7 5s-7.4-3-8.7-5C4.6 10 7.8 7 12 7zm0 1.2a3.8 3.8 0 1 0 0 7.6 3.8 3.8 0 0 0 0-7.6zm0 2a1.8 1.8 0 1 1 0 3.6 1.8 1.8 0 0 1 0-3.6z" fill-rule="evenodd"/>'),
    'mail': ic('<rect x="2.5" y="5" width="19" height="14" rx="1"/><path d="M3 5.6l9 7.2 9-7.2M3 18.4l6.6-6.2M21 18.4l-6.6-6.2"/>', 1.3),
    'phone': ic('<path d="M7 2.8l3 4.3-2.1 2.4c1.3 2.9 3.7 5.3 6.6 6.6l2.4-2.1 4.3 3-1.5 3.4c-.3.7-1.1 1.1-1.9 1C9.7 20.6 3.4 14.3 2.6 6.2c-.1-.8.3-1.6 1-1.9z"/><path d="M14 3.5a6.5 6.5 0 0 1 6.5 6.5M14 6.6a3.4 3.4 0 0 1 3.4 3.4" fill="none" stroke="currentColor" stroke-width="1.4" stroke-linecap="round"/>'),
}

BLOB = ('<svg viewBox="0 0 100 100" preserveAspectRatio="none" aria-hidden="true"><path fill="currentColor" d="M62,2 C84,4 97,24 98,50 C99,79 86,99 62,98 C40,97 25,93 12,80 C1,69 0,52 3,39 C8,17 36,0 62,2 Z"/></svg>')
BLOB2 = ('<svg viewBox="0 0 100 100" preserveAspectRatio="none" aria-hidden="true"><path fill="currentColor" d="M48,1 C78,0 99,18 99,47 C99,77 82,99 55,99 C30,99 14,92 6,74 C-1,58 2,38 9,24 C16,10 30,2 48,1 Z"/></svg>')
BLOB3 = ('<svg viewBox="0 0 100 100" preserveAspectRatio="none" aria-hidden="true"><path fill="currentColor" d="M45,3 C70,0 98,4 99,28 C100,52 92,78 74,93 C61,103 40,99 26,90 C10,80 1,62 1,45 C1,22 20,6 45,3 Z"/></svg>')
BLOBS = [BLOB, BLOB2, BLOB3]

LOGO = ('<svg viewBox="0 0 54 58" aria-hidden="true"><path fill="currentColor" fill-rule="evenodd" d="M9,4 C22,-1 40,0 50,6 C54,18 54,40 50,52 C38,58 18,58 5,54 C0,42 0,18 9,4 Z '
        'M15,13 C24,10 36,10 43,14 C45,23 45,35 43,44 C34,48 22,48 12,45 C10,36 10,22 15,13 Z"/></svg>')


def deco(paths):
    return '<svg class="deco" viewBox="0 0 1280 720" aria-hidden="true">' + paths + '</svg>'


def kcard(n, icon, blob, cls='k%d'):
    return ('<div class="kc %s"><div class="blob">%s<span class="ic">%s</span></div><p class="v"><span class="num">§V%d§</span></p><p class="l">§L%d§</p></div>'
            % (cls % n, BLOBS[blob], ICONS[icon], n, n))


TOPR = '<ellipse cx="1190" cy="-20" rx="115" ry="68" fill="#D5DEFD"/><ellipse cx="1252" cy="-30" rx="135" ry="98" fill="#2D59F6"/>'
WAVE_TR = 'M750,0 C790,60 830,100 885,100 C960,100 1030,40 1100,38 C1170,36 1200,90 1230,150 C1250,190 1265,210 1280,210 V0 Z'

S1 = ('\n    ' + deco('<path fill="#D5DEFD" d="M740,70 C765,115 820,124 880,121 C950,117 1000,107 1045,113 L1040,60 Z"/>'
                     '<path fill="#2D59F6" d="M550,0 C576,42 605,78 662,84 C722,90 765,78 842,74 C920,70 990,88 1032,116 C1082,152 1108,228 1158,280 C1196,318 1238,326 1280,322 L1280,0 Z"/>'
                     '<ellipse cx="1160" cy="782" rx="118" ry="125" fill="#D5DEFD"/><ellipse cx="1255" cy="780" rx="128" ry="147" fill="#2D59F6"/>')
      + '\n    <div class="logo">' + LOGO + '<p>§LOGO1§<br>§LOGO2§</p></div>'
      + '\n    <h1 class="t1">§C_T1§</h1>\n    <p class="t2">§C_T2§</p>\n    <p class="pill">§C_PILL§</p>\n    <p class="lead">§C_LEAD§</p>\n  ')

S2 = ('\n    ' + deco('<path fill="#D5DEFD" d="M1060,0 H1280 V150 H1150 C1100,150 1060,110 1060,60 Z"/>'
                     '<path fill="#2D59F6" d="M1108,0 H1280 V167 H1198 C1148,167 1108,127 1108,77 Z"/>'
                     '<path fill="#E2E8FE" d="M0,640 H1120 C1148,640 1170,662 1170,690 V720 H0 Z"/>'
                     '<path fill="#2D59F6" d="M0,618 H1092 C1120,618 1142,640 1142,668 V720 H0 Z"/>')
      + '\n    <p class="sh">§SH§</p>\n    <h1 class="ttl">§E_TTL§</h1>\n    '
      + kcard(1, 'user', 0).replace('§V1§', '§E_V1§').replace('§L1§', '§E_L1§') + '\n    '
      + kcard(2, 'love', 1).replace('§V2§', '§E_V2§').replace('§L2§', '§E_L2§') + '\n    '
      + kcard(3, 'cart', 2).replace('§V3§', '§E_V3§').replace('§L3§', '§E_L3§')
      + '\n    <p class="foot">§FOOT1§</p>\n  ')

S3 = ('\n    ' + deco('<path fill="#D5DEFD" d="M0,0 H131 V40 C131,100 90,140 30,146 H0 Z"/>'
                     '<path fill="#2D59F6" d="M0,0 H97 V62 C97,112 58,150 0,160 Z"/>'
                     '<path fill="#2D59F6" d="M1150,100 H1238 Q1280,100 1280,140 V490 H1150 Z"/>'
                     '<path fill="#D5DEFD" d="M868,720 Q876,668 930,660 H1150 V470 Q1240,468 1280,490 V720 Z"/>'
                     '<path fill="#2D59F6" d="M718,720 Q746,668 800,662 H888 Q872,690 870,720 Z"/>')
      + '\n    <img class="ph" src="/templates/freeform/sl-business-growth/assets/company.jpg" alt="§O_ALT§">'
      + '\n    <p class="sh">§SH§</p>\n    <h1 class="ttl">§O_TTL1§<br>§O_TTL2§</h1>\n    <p class="yr"><span class="num">§O_YR§</span></p>'
      + '\n    <p class="txt">§O_TXT§</p>\n  ')


def item(n, icon, cls='i%d'):
    return ('<div class="it %s"><span class="ic">%s</span><h2>§%%s_H%d§</h2><p class="txt">§%%s_P%d§</p></div>' % (cls % n, ICONS[icon], n, n))


S4 = ('\n    ' + deco('<ellipse cx="255" cy="292" rx="146" ry="64" fill="#EEF2FE"/>'
                     '<path fill="#2D59F6" d="M0,245 C80,262 140,286 222,284 C330,281 420,260 520,262 C620,264 690,279 760,279 L1180,262 C1222,252 1252,256 1280,268 V720 H0 Z"/>')
      + '\n    <img class="ph" src="/templates/freeform/sl-business-growth/assets/market.jpg" alt="§M_ALT§">'
      + '\n    <p class="sh">§SH§</p>\n    <h1 class="ttl">§M_TTL§</h1>'
      + '\n    ' + (item(1, 'finger') % ('M', 'M')) + '\n    <span class="rule r1"></span>'
      + '\n    ' + (item(2, 'fireworks') % ('M', 'M')) + '\n    <span class="rule r2"></span>'
      + '\n    <p class="foot">§M_FOOT§</p>\n  ')

S5 = ('\n    <img class="ph" src="/templates/freeform/sl-business-growth/assets/challenges.jpg" alt="§B_ALT§">'
      + '\n    <div class="panel"></div>'
      + '\n    <h1 class="big">§B_TTL1§<br>§B_TTL2§</h1>\n    <p class="txt lead">§B_TXT§</p>'
      + '\n    ' + (item(1, 'trophy') % ('B', 'B')) + '\n    <span class="rule r1"></span>'
      + '\n    ' + (item(2, 'coins') % ('B', 'B')) + '\n    <span class="rule r2"></span>'
      + '\n    ' + (item(3, 'pulse') % ('B', 'B')) + '\n    <span class="rule r3"></span>'
      + '\n    ' + (item(4, 'brain') % ('B', 'B')) + '\n  ')


def scard(n, icon, blob):
    return ('<div class="sc c%d"><div class="blob">%s<span class="ic">%s</span></div><h2>§S_H%d§</h2><p class="txt">§S_P%d§</p></div>'
            % (n, BLOBS[blob], ICONS[icon], n, n))


S6 = ('\n    ' + deco('<circle cx="160" cy="1060" r="680" fill="#2D59F6"/>' + TOPR)
      + '\n    <p class="sh">§SH§</p>\n    <h1 class="ttl">§S_TTL§</h1>'
      + '\n    ' + scard(1, 'user', 0) + '\n    ' + scard(2, 'crown', 1) + '\n    ' + scard(3, 'pulse', 2)
      + '\n    <img class="ph" src="/templates/freeform/sl-business-growth/assets/office.jpg" alt="§S_ALT§">'
      + '\n    <p class="big">§S_BIG§</p>\n    <p class="txt side">§S_TXT§</p>\n  ')

S7 = ('\n    ' + deco('<path fill="#0936D1" d="M0,524 C150,560 400,542 600,540 C850,538 1100,560 1280,600 V720 H0 Z"/>')
      + '\n    <p class="sh">§SH§</p>\n    <h1 class="ttl">§G_TTL§</h1>\n    <p class="txt intro">§G_INTRO§</p>'
      + '\n    <div class="gc g1"><p class="n"><span class="num">01</span></p><span class="ic">' + ICONS['tree'] + '</span><h2>§G_H1§</h2><p class="txt">§G_P1§</p></div>'
      + '\n    <div class="gc g2"><p class="n"><span class="num">02</span></p><h2>§G_H2§</h2><p class="txt">§G_P2§</p></div>'
      + '\n    <div class="gc g3"><p class="n"><span class="num">03</span></p><h2>§G_H3§</h2><p class="txt">§G_P3§</p></div>'
      + '\n    <div class="gc g4"><p class="n"><span class="num">04</span></p><h2>§G_H4§</h2><p class="txt">§G_P4§</p></div>\n  ')

UP = '<svg viewBox="0 0 15 13" aria-hidden="true"><path fill="#fff" d="M7.5,0 L15,13 H0 Z"/></svg>'


def fcard(n, bold):
    return ('<div class="fc f%d"><p class="y%s"><span class="num">§F_Y%d§</span></p><p class="v"><span class="num">§F_V%d§</span></p>'
            '<p class="pill">%s<span class="num">§F_D%d§</span></p></div>' % (n, ' b' if bold else '', n, n, UP, n))


S8 = ('\n    ' + deco('<path fill="#2D59F6" d="M0,258 C160,228 360,228 520,275 C680,322 760,400 860,500 C960,600 1040,660 1082,720 H0 Z"/>' + TOPR)
      + '\n    ' + fcard(1, False) + '\n    ' + fcard(2, True) + '\n    ' + fcard(3, True)
      + '\n    <p class="sh">§SH§</p>\n    <h1 class="ttl">§F_TTL§</h1>\n    <p class="txt">§F_TXT§</p>'
      + '\n    <img class="ph" src="/templates/freeform/sl-business-growth/assets/team.jpg" alt="§F_ALT§">\n  ')

ROAD = 'M-20,486 C60,460 120,437 200,437 C300,437 420,495 520,495 C600,495 620,432 700,432 C790,432 900,480 1050,497 C1150,505 1220,474 1300,446'
S9 = ('\n    ' + deco('<path fill="#E2E8FE" d="M905,60 C950,135 1005,160 1060,160 C1122,160 1165,125 1208,110 L1200,0 H905 Z"/>'
                     '<path fill="#2D59F6" d="' + WAVE_TR + '"/>'
                     '<ellipse cx="102" cy="772" rx="113" ry="120" fill="#D5DEFD"/><circle cx="40" cy="760" r="125" fill="#2D59F6"/>'
                     '<path d="' + ROAD + '" fill="none" stroke="#2D59F6" stroke-width="56"/>'
                     '<path d="' + ROAD + '" fill="none" stroke="#fff" stroke-width="2.5" stroke-dasharray="9 7" opacity=".9"/>'
                     '<g stroke="#141B5C" stroke-width="2.6" stroke-dasharray="6 4"><path d="M83,302 V422"/><path d="M313,483 V551"/><path d="M691,292 V405"/><path d="M892,497 V552"/></g>')
      + '\n    <p class="sh">§SH§</p>\n    <h1 class="ttl">§T_TTL§</h1>'
      + ''.join('\n    <span class="dot d%d">%d</span><h2 class="h%d">§T_H%d§</h2><p class="txt p%d">§T_P%d§</p>' % (i, i, i, i, i, i) for i in range(1, 5))
      + '\n  ')

S10 = ('\n    ' + deco('<circle cx="1120" cy="-330" r="673" fill="#2D59F6"/>')
       + '\n    <p class="sh">§SH§</p>\n    <h1 class="ttl">§R_TTL§</h1>\n    <p class="big">§R_BIG§</p>\n    '
       + kcard(1, 'love', 0).replace('§V1§', '§R_V1§').replace('§L1§', '§R_L1§') + '\n    '
       + kcard(2, 'masks', 1).replace('§V2§', '§R_V2§').replace('§L2§', '§R_L2§') + '\n    '
       + kcard(3, 'dollar', 2).replace('§V3§', '§R_V3§').replace('§L3§', '§R_L3§') + '\n  ')

S11 = ('\n    ' + deco('<path fill="#E2E8FE" d="M812,0 H1280 V720 H648 L760,560 L870,84 Z"/>'
                      '<path fill="#D5DEFD" d="M1229,360 C1255,362 1270,372 1280,380 V720 H640 L700,652 L1229,640 Z"/>'
                      '<path fill="#2D59F6" d="M1200,92 H1236 C1258,92 1272,96 1280,100 V378 C1268,370 1250,365 1229,364 H1200 Z"/>')
       + '\n    <img class="ph" src="/templates/freeform/sl-business-growth/assets/steps.jpg" alt="§N_ALT§">'
       + '\n    <p class="sh">§SH§</p>\n    <h1 class="ttl">§N_TTL§</h1>'
       + '\n    ' + (item(1, 'brain') % ('N', 'N')) + '\n    <span class="rule r1"></span>'
       + '\n    ' + (item(2, 'hands') % ('N', 'N')) + '\n    <span class="rule r2"></span>'
       + '\n    ' + (item(3, 'eye') % ('N', 'N')) + '\n  ')


def contact(n, icon, tok):
    return ('<p class="ct c%d"><span class="bl">%s<span class="ic">%s</span></span><span class="num">§%s§</span></p>' % (n, BLOBS[1], ICONS[icon], tok))


S12 = ('\n    ' + deco('<path fill="#D5DEFD" d="M0,0 H225 C190,30 150,62 100,62 C50,62 20,35 0,15 Z"/>'
                      '<path fill="#EEF2FE" d="' + WAVE_TR + '"/>'
                      '<path fill="#E2E8FE" d="M0,610 C60,625 120,600 200,570 C255,549 295,536 330,540 C400,552 462,640 516,720 H0 Z"/>')
       + '\n    <h1 class="ty">§K_TY§</h1>\n    <span class="uline" aria-hidden="true"></span>'
       + '\n    <p class="big">§K_BIG§</p>\n    <p class="txt">§K_TXT§</p>'
       + '\n    <div class="logo">' + LOGO + '<p>§LOGO1§<br>§LOGO2§</p></div>'
       + '\n    ' + contact(1, 'mail', 'K_MAIL') + '\n    ' + contact(2, 'phone', 'K_TEL') + '\n  ')

SLIDES = [S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12]

ITXT_EN = 'Add text here. Insert your desired text here. This is a sample text. A sample text.'
ITXT_AR = 'أضف النص هنا. أدرج النص المطلوب هنا. هذا نص توضيحي. نص توضيحي.'
LONG_EN = 'This is a sample text. Add text here. Insert your desired text here. This is a simple text. This is a sample text. Add your text here.'
LONG_AR = 'هذا نص توضيحي. أضف النص هنا. أدرج النص المطلوب هنا. هذا نص بسيط. هذا نص توضيحي. أضف نصك هنا.'

STR = {
    'en': {
        'SH': 'Add subheading here', 'LOGO1': 'YOUR COMPANY', 'LOGO2': 'LOGO HERE',
        'C_T1': 'BUSINESS GROWTH', 'C_T2': 'STRATEGY', 'C_PILL': 'PRESENTATION TEMPLATE', 'C_LEAD': 'This is a sample text. Add your desired text here.',
        'E_TTL': 'Executive Summary Slide', 'E_V1': '4.180+', 'E_L1': 'Active Clients', 'E_V2': '96,2%', 'E_L2': 'Customer Satisfaction', 'E_V3': '1.252+', 'E_L3': 'Global Markets',
        'FOOT1': '*Add text here. Insert your desired text here. This is a simple text. This is a sample text.',
        'O_TTL1': 'Company', 'O_TTL2': 'Overview Slide', 'O_YR': '1972', 'O_ALT': 'Modern office building courtyard with concrete columns and seating',
        'O_TXT': 'This is a sample text. Add text here. Insert your desired text here. This is a simple text. This is a sample text. Add your text here. Add text here. Insert your desired text here.',
        'M_TTL': 'Market Opportunity', 'M_ALT': 'Atrium of a brick office building with white crossing columns',
        'M_H1': 'Digital Transformation', 'M_P1': 'Add text here. Insert your desired text here. This is a sample text. Add text here.',
        'M_H2': 'Global Business Growth', 'M_P2': 'Add text here. Insert your desired text here. This is a sample text. Add text here.',
        'M_FOOT': '*Add text here. Insert your desired text here. This is a simple text. This is a sample text. Add your text here.',
        'B_TTL1': 'Business', 'B_TTL2': 'Challenges', 'B_TXT': LONG_EN, 'B_ALT': 'Young professional looking down while working at a desk',
        'B_H1': 'Market Competition', 'B_P1': ITXT_EN, 'B_H2': 'Cost Pressure', 'B_P2': ITXT_EN,
        'B_H3': 'Rapid Change', 'B_P3': ITXT_EN, 'B_H4': 'Tech Innovation', 'B_P4': ITXT_EN,
        'S_TTL': 'Our Solutions Slide', 'S_ALT': 'Open-plan office with desks, plants and ceiling lights',
        'S_H1': 'Active Clients', 'S_P1': 'This is a sample text. Add text here', 'S_H2': 'Digital Innovation', 'S_P2': 'This is a sample text. Add text here',
        'S_H3': 'Process Excellence', 'S_P3': 'This is a sample text. Add text here', 'S_BIG': 'Core strengths for business growth.', 'S_TXT': LONG_EN,
        'G_TTL': 'Growth Strategy', 'G_INTRO': 'This is a sample text. Add text here. Insert your desired text here. This is a text.',
        'G_H1': 'Market Expansion', 'G_P1': 'Add text here. Insert your desired text here. This is a sample text. A sample text. Insert your desired text here.',
        'G_H2': 'Operational Excellence', 'G_P2': ITXT_EN, 'G_H3': 'Digital Innovation', 'G_P3': ITXT_EN, 'G_H4': 'Customer Success', 'G_P4': ITXT_EN,
        'F_Y1': '2025', 'F_V1': '$8.4M', 'F_D1': '+13%', 'F_Y2': '2030', 'F_V2': '$10.7M', 'F_D2': '+21%', 'F_Y3': '2035', 'F_V3': '$17.5M', 'F_D3': '+27%',
        'F_TTL': 'Financial Overview', 'F_ALT': 'Team working at computers in a bright office with plants',
        'F_TXT': 'This is a sample text. Add text here. Insert your desired text here. This is a text. Add text here. This is a sample text. Insert your desired text here.',
        'T_TTL': 'Project Timeline Slide', 'T_H1': 'Planning', 'T_H2': 'Design', 'T_H3': 'Execution', 'T_H4': 'Evaluation',
        'T_P1': 'Add text here. Insert your desired text here.', 'T_P2': 'Add text here. Insert your desired text here.',
        'T_P3': 'Add text here. Insert your desired text here.', 'T_P4': 'Add text here. Insert your desired text here.',
        'R_TTL': 'Key Results Slide', 'R_BIG': 'Driving Measurable Business Results.',
        'R_V1': '86%', 'R_L1': 'Customer Satisfaction', 'R_V2': '98%', 'R_L2': 'Project Success Rate', 'R_V3': '21%', 'R_L3': 'Revenue Growth',
        'N_TTL': 'Next Steps Slide', 'N_ALT': 'Two colleagues talking in a living room with laptops',
        'N_H1': 'Finalize Strategy', 'N_P1': ITXT_EN, 'N_H2': 'Launch Implementation', 'N_P2': ITXT_EN, 'N_H3': 'Monitor Performance', 'N_P3': ITXT_EN,
        'K_TY': 'THANK YOU.', 'K_BIG': 'Thank you for your time and support.', 'K_TXT': 'Add text here. Insert your desired text here. This is a sample text.',
        'K_MAIL': 'Add your email here', 'K_TEL': '+1 (234) 567-890',
    },
    'ar': {
        'SH': 'أضف عنوانًا فرعيًا هنا', 'LOGO1': 'شعار شركتك', 'LOGO2': 'هنا',
        'C_T1': 'استراتيجية نمو', 'C_T2': 'الأعمال', 'C_PILL': 'قالب عرض تقديمي', 'C_LEAD': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
        'E_TTL': 'شريحة الملخص التنفيذي', 'E_V1': '4.180+', 'E_L1': 'العملاء النشطون', 'E_V2': '96,2%', 'E_L2': 'رضا العملاء', 'E_V3': '1.252+', 'E_L3': 'الأسواق العالمية',
        'FOOT1': '*أضف النص هنا. أدرج النص المطلوب هنا. هذا نص بسيط. هذا نص توضيحي.',
        'O_TTL1': 'نظرة عامة', 'O_TTL2': 'على الشركة', 'O_YR': '1972', 'O_ALT': 'فناء مبنى مكاتب حديث بأعمدة خرسانية ومقاعد',
        'O_TXT': 'هذا نص توضيحي. أضف النص هنا. أدرج النص المطلوب هنا. هذا نص بسيط. هذا نص توضيحي. أضف نصك هنا. أضف النص هنا. أدرج النص المطلوب هنا.',
        'M_TTL': 'فرص السوق', 'M_ALT': 'ردهة مبنى مكاتب من الطوب بأعمدة بيضاء متقاطعة',
        'M_H1': 'التحول الرقمي', 'M_P1': 'أضف النص هنا. أدرج النص المطلوب هنا. هذا نص توضيحي. أضف النص هنا.',
        'M_H2': 'نمو الأعمال عالميًا', 'M_P2': 'أضف النص هنا. أدرج النص المطلوب هنا. هذا نص توضيحي. أضف النص هنا.',
        'M_FOOT': '*أضف النص هنا. أدرج النص المطلوب هنا. هذا نص بسيط. هذا نص توضيحي. أضف نصك هنا.',
        'B_TTL1': 'تحديات', 'B_TTL2': 'الأعمال', 'B_TXT': LONG_AR, 'B_ALT': 'شاب ينظر إلى الأسفل أثناء عمله على مكتب',
        'B_H1': 'المنافسة في السوق', 'B_P1': ITXT_AR, 'B_H2': 'ضغط التكاليف', 'B_P2': ITXT_AR,
        'B_H3': 'التغيّر السريع', 'B_P3': ITXT_AR, 'B_H4': 'الابتكار التقني', 'B_P4': ITXT_AR,
        'S_TTL': 'شريحة حلولنا', 'S_ALT': 'مكتب مفتوح بمكاتب ونباتات وإضاءة سقفية',
        'S_H1': 'العملاء النشطون', 'S_P1': 'هذا نص توضيحي. أضف النص هنا', 'S_H2': 'الابتكار الرقمي', 'S_P2': 'هذا نص توضيحي. أضف النص هنا',
        'S_H3': 'تميّز العمليات', 'S_P3': 'هذا نص توضيحي. أضف النص هنا', 'S_BIG': 'نقاط قوة أساسية لنمو الأعمال.', 'S_TXT': LONG_AR,
        'G_TTL': 'استراتيجية النمو', 'G_INTRO': 'هذا نص توضيحي. أضف النص هنا. أدرج النص المطلوب هنا. هذا نص.',
        'G_H1': 'التوسع في السوق', 'G_P1': 'أضف النص هنا. أدرج النص المطلوب هنا. هذا نص توضيحي. نص توضيحي. أدرج النص المطلوب هنا.',
        'G_H2': 'التميّز التشغيلي', 'G_P2': ITXT_AR, 'G_H3': 'الابتكار الرقمي', 'G_P3': ITXT_AR, 'G_H4': 'نجاح العملاء', 'G_P4': ITXT_AR,
        'F_Y1': '2025', 'F_V1': '$8.4M', 'F_D1': '+13%', 'F_Y2': '2030', 'F_V2': '$10.7M', 'F_D2': '+21%', 'F_Y3': '2035', 'F_V3': '$17.5M', 'F_D3': '+27%',
        'F_TTL': 'نظرة مالية عامة', 'F_ALT': 'فريق يعمل على الحواسيب في مكتب مضيء بالنباتات',
        'F_TXT': 'هذا نص توضيحي. أضف النص هنا. أدرج النص المطلوب هنا. هذا نص. أضف النص هنا. هذا نص توضيحي. أدرج النص المطلوب هنا.',
        'T_TTL': 'شريحة الجدول الزمني للمشروع', 'T_H1': 'التخطيط', 'T_H2': 'التصميم', 'T_H3': 'التنفيذ', 'T_H4': 'التقييم',
        'T_P1': 'أضف النص هنا. أدرج النص المطلوب هنا.', 'T_P2': 'أضف النص هنا. أدرج النص المطلوب هنا.',
        'T_P3': 'أضف النص هنا. أدرج النص المطلوب هنا.', 'T_P4': 'أضف النص هنا. أدرج النص المطلوب هنا.',
        'R_TTL': 'شريحة النتائج الرئيسية', 'R_BIG': 'تحقيق نتائج أعمال قابلة للقياس.',
        'R_V1': '86%', 'R_L1': 'رضا العملاء', 'R_V2': '98%', 'R_L2': 'معدل نجاح المشاريع', 'R_V3': '21%', 'R_L3': 'نمو الإيرادات',
        'N_TTL': 'شريحة الخطوات التالية', 'N_ALT': 'زميلان يتحدثان في غرفة معيشة ومعهما حاسوبان محمولان',
        'N_H1': 'اعتماد الاستراتيجية', 'N_P1': ITXT_AR, 'N_H2': 'إطلاق التنفيذ', 'N_P2': ITXT_AR, 'N_H3': 'متابعة الأداء', 'N_P3': ITXT_AR,
        'K_TY': 'شكرًا لكم.', 'K_BIG': 'شكرًا لوقتكم ودعمكم.', 'K_TXT': 'أضف النص هنا. أدرج النص المطلوب هنا. هذا نص توضيحي.',
        'K_MAIL': 'أضف بريدك الإلكتروني هنا', 'K_TEL': '+1 (234) 567-890',
    },
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'mgmt', 'icon': '📈',
    'name': 'استراتيجية نمو الأعمال', 'nameEn': 'Business Growth Strategy',
    'description': '12 شريحة: غلاف، ملخص، الشركة، فرص السوق، التحديات، الحلول، الاستراتيجية، المالية، الجدول الزمني، النتائج، الخطوات، شكر',
    'descriptionEn': '12 slides: cover, summary, company, market, challenges, solutions, strategy, finance, timeline, results, next steps',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'الملخص التنفيذي', 'titleEn': 'Executive Summary', 'needsData': True},
        {'title': 'نظرة عامة على الشركة', 'titleEn': 'Company Overview', 'needsData': True},
        {'title': 'فرص السوق', 'titleEn': 'Market Opportunity'},
        {'title': 'تحديات الأعمال', 'titleEn': 'Business Challenges'},
        {'title': 'حلولنا', 'titleEn': 'Our Solutions'},
        {'title': 'استراتيجية النمو', 'titleEn': 'Growth Strategy'},
        {'title': 'نظرة مالية عامة', 'titleEn': 'Financial Overview', 'needsData': True},
        {'title': 'الجدول الزمني للمشروع', 'titleEn': 'Project Timeline'},
        {'title': 'النتائج الرئيسية', 'titleEn': 'Key Results', 'needsData': True},
        {'title': 'الخطوات التالية', 'titleEn': 'Next Steps'},
        {'title': 'شكرًا لكم', 'titleEn': 'Thank You'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والأشكال الزخرفية والصور وغيّر النصوص والأرقام فقط (مؤشرات الملخص والنتائج، سنة التأسيس، وأرقام النظرة المالية ونسب نموها).',
}
