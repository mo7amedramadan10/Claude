# -*- coding: utf-8 -*-
# نموذج «هوية العلامة التجارية»: ثلاث عشرة شريحة بلون كحلي وفيروزي: غلاف بدائرتين، فهرس، الشعارات، الرسالة، دليل استخدام الشعار،
# دليل الألوان، الأيقونات، أيقونات التطبيق، الخطوط، لوحة الإلهام البصرية (صور)، نماذج تطبيق العلامة (خطاب وصور)، والختام.
KEY = 'sl-brand-identity'
TITLE = {'ar': 'قالب هوية العلامة التجارية', 'en': 'Brand Identity Presentation Template'}
COMMENT = 'نموذج «هوية العلامة التجارية»: عناوين كحلية غامقة، أرقام فيروزية فاتحة، خطوط فاصلة فيروزية، وشعار تجريدي (مربع مستدير منقسم) يُرسم SVG بلون currentColor. الغلاف والختام بدائرة كحلية فوق هلال أزرق متدرّج.'
VARS = '--ink: #111111; --navy: #1B4865; --navy-2: #17384B; --teal: #61B6CB; --sky: #5FA6D4; --mint: #B7E3E6; --num: #A3D3E0; --pale: #CAE9FE; --accent: #1B4865; --bg: #FFFFFF;'

STYLE = """  /* عناصر مشتركة */
  .bg-arc { position: absolute; inset: 0; width: 1280px; height: 720px; }
  html[dir="rtl"] .bg-arc { transform: scaleX(-1); }
  .s13 .bg-arc { transform: scaleY(-1); } html[dir="rtl"] .s13 .bg-arc { transform: scale(-1, -1); }
  .h { position: absolute; top: 137px; inset-inline-start: 109px; margin: 0; font-size: 56px; font-weight: 700; line-height: 1.25; color: var(--navy); white-space: nowrap; letter-spacing: -0.35px; }
  html[lang="ar"] .h { font-size: 58px; font-weight: 800; letter-spacing: 0; top: 132px; }
  .p { position: absolute; margin: 0; font-size: 22px; line-height: 29px; color: var(--ink); }
  .q { position: absolute; margin: 0; font-size: 20px; line-height: 25px; color: var(--ink); }
  html[lang="ar"] .p, html[lang="ar"] .q { line-height: 1.45; }
  html[lang="ar"] .p { font-size: 23px; } html[lang="ar"] .q { font-size: 21px; }
  .rule { position: absolute; height: 0; border-top: 1px solid var(--teal); width: 128px; }
  .num { position: absolute; margin: 0; font-size: 54px; font-weight: 300; line-height: 1; color: var(--num); letter-spacing: -1px; }
  .logo { position: absolute; display: block; }
  .logo svg { width: 100%; height: 100%; display: block; overflow: visible; }
  .lt { position: absolute; margin: 0; font-size: 16px; line-height: 1.22; letter-spacing: 2.6px; text-transform: uppercase; }
  html[lang="ar"] .lt { letter-spacing: 0; line-height: 1.45; }
  /* ١ الغلاف */
  .s1 .cv { position: absolute; top: 186px; inset-inline-start: 110px; margin: 0; font-size: 71.5px; font-weight: 700; line-height: 85px; color: #62B4CB; }
  html[lang="ar"] .s1 .cv { font-size: 76px; line-height: 92px; top: 172px; font-weight: 800; }
  .s1 .sub { position: absolute; top: 383px; inset-inline-start: 111px; margin: 0; font-size: 29px; font-weight: 400; letter-spacing: 4.9px; color: #CDEEF3; line-height: 1.2; }
  html[lang="ar"] .s1 .sub { letter-spacing: 0; font-size: 32px; top: 382px; }
  .s1 .logo { top: 572px; inset-inline-start: 882px; width: 76px; height: 84px; color: var(--sky); }
  .s1 .lt { top: 587px; inset-inline-start: 999px; font-size: 22px; letter-spacing: 3px; color: #1A1A1A; line-height: 29px; }
  html[lang="ar"] .s1 .lt { letter-spacing: 0; font-size: 23px; }
  /* ٢ الفهرس */
  .s2 .it { position: absolute; width: 303px; height: 110px; }
  .s2 .it .num { top: 21px; inset-inline-start: 9px; font-size: 57px; }
  .s2 .it .p { top: -4px; inset-inline-start: 105px; width: 205px; }
  .s2 .it .bar { position: absolute; top: 89px; inset-inline-start: 0; width: 149px; height: 21px; border-radius: 11px; background: var(--navy); }
  .s2 .it .ln { position: absolute; top: 99px; inset-inline-start: 149px; width: 154px; border-top: 1px solid var(--navy); }
  .s2 .c1 { inset-inline-start: 111px; } .s2 .c2 { inset-inline-start: 490px; } .s2 .c3 { inset-inline-start: 867px; }
  .s2 .r1 { top: 260px; } .s2 .r2 { top: 437px; }
  /* ٣ الشعارات */
  .s3 .blk { position: absolute; inset-inline-start: 0; width: 459px; height: 197px; }
  .s3 .b1 { top: 239px; background: var(--navy); } .s3 .b2 { top: 436px; height: 196px; background: var(--teal); }
  .s3 .blk .logo { top: 67px; inset-inline-start: 109px; width: 58px; height: 64px; }
  .s3 .b1 .logo { color: var(--sky); } .s3 .b2 .logo { top: 58px; color: var(--mint); }
  .s3 .blk .lt { top: 81px; inset-inline-start: 199px; color: #FFFFFF; }
  .s3 .b2 .lt { top: 72px; }
  .s3 .kick { position: absolute; top: 241px; inset-inline-start: 511px; margin: 0; font-size: 22px; line-height: 1.2; color: var(--ink); }
  .s3 .h2 { position: absolute; top: 271px; inset-inline-start: 510px; margin: 0; font-size: 45px; font-weight: 700; line-height: 1.2; color: var(--navy); letter-spacing: -0.4px; }
  html[lang="ar"] .s3 .h2 { font-weight: 800; top: 268px; }
  .s3 .body { top: 365px; inset-inline-start: 511px; width: 320px; }
  .s3 .body2 { top: 481px; inset-inline-start: 511px; width: 320px; }
  .s3 .side .q { inset-inline-start: 966px; width: 200px; }
  .s3 .side .rule { inset-inline-start: 966px; }
  /* ٤ الرسالة */
  .s4 .corner { position: absolute; top: 0; inset-inline-end: 0; width: 564px; height: 283px; background: var(--navy); border-end-start-radius: 86px; }
  .s4 .corner .logo { top: 97px; inset-inline-start: 110px; width: 79px; height: 88px; color: var(--sky); }
  .s4 .corner .lt { top: 113px; inset-inline-start: 225px; font-size: 20px; letter-spacing: 3px; color: #FFFFFF; line-height: 25px; }
  html[lang="ar"] .s4 .corner .lt { letter-spacing: 0; font-size: 22px; line-height: 1.4; }
  .lead { top: 258px; inset-inline-start: 111px; width: 560px; }
  .cols .num { top: 372px; }
  .cols .rule { top: 432px; }
  .cols .q { top: 460px; width: 200px; }
  /* ٥ دليل استخدام الشعار */
  .s5 .intro { top: 128px; inset-inline-start: 721px; width: 380px; }
  .s5 .k1 .bar { margin-inline-start: -52px; } .s5 .k3 .bar { margin-inline-start: -46px; } .s5 .k1 h3 { inset-inline-end: 8px; }
  .s5 .card { position: absolute; top: 284px; width: 325px; height: 316px; border-start-start-radius: 46px; border-start-end-radius: 46px; background: linear-gradient(180deg, #CAE9FE 0%, #DDF0FE 35%, #F3F9FF 70%, rgb(255 255 255 / 0) 100%); }
  .s5 .k1 { inset-inline-start: 113px; } .s5 .k2 { inset-inline-start: 462px; } .s5 .k3 { inset-inline-start: 811px; }
  .s5 .card h3 { position: absolute; top: 33px; inset-inline: 0; margin: 0; text-align: center; font-size: 20px; font-weight: 700; line-height: 1.2; color: #0A0A0A; text-transform: uppercase; }
  .s5 .card .bar { position: absolute; top: 82px; inset-inline-start: 50%; margin-inline-start: -42px; width: 83px; height: 5px; border-radius: 3px; background: var(--navy); }
  .s5 .card .logo { color: var(--navy); }
  .s5 .k1 .logo { top: 138px; inset-inline-start: 31px; width: 68px; height: 75px; }
  .s5 .k1 .lt { top: 155px; inset-inline-start: 117px; color: #1A1A1A; }
  .s5 .k2 .logo { top: 138px; inset-inline-start: 86px; width: 143px; height: 158px; }
  .s5 .k3 .logo { top: 132px; inset-inline-start: 119px; width: 74px; height: 82px; }
  .s5 .k3 .lt { top: 246px; inset-inline: 0; text-align: center; font-size: 15px; color: #1A1A1A; }
  html[lang="ar"] .s5 .k3 .lt, html[lang="ar"] .s5 .k1 .lt { font-size: 17px; }
  /* ٦ دليل الألوان */
  .s6 .sw { position: absolute; width: 63px; height: 46px; }
  .s6 .blk { position: absolute; width: 252px; }
  .s6 .pr { top: 284px; inset-inline-start: 111px; height: 303px; background: var(--navy); color: #fff; border-end-start-radius: 40px; }
  .s6 .se { top: 284px; inset-inline-start: 363px; height: 303px; background: #62B5CB; color: #fff; }
  .s6 .ne { top: 210px; inset-inline-start: 615px; height: 302px; background: var(--pale); color: var(--ink); border-start-end-radius: 40px; }
  .s6 .blk h3 { position: absolute; top: 37px; inset-inline-start: 23px; margin: 0; font-size: 30px; font-weight: 400; line-height: 38px; }
  .s6 .ne h3 { top: 23px; inset-inline-start: 25px; }
  .s6 .blk .hex { position: absolute; top: 134px; inset-inline-start: 23px; margin: 0; font-size: 17px; font-style: italic; line-height: 1.2; }
  .s6 .ne .hex { top: 113px; inset-inline-start: 25px; }
  .s6 .blk .vals { position: absolute; top: 195px; inset-inline-start: 23px; margin: 0; font-size: 17px; line-height: 22px; display: flex; gap: 0; }
  .s6 .ne .vals { top: 173px; inset-inline-start: 25px; }
  .s6 .vals span { width: 98px; }
  .s6 .band { position: absolute; inset-inline-start: 615px; width: 252px; height: 25px; }
  .s6 .intro { top: 211px; inset-inline-start: 910px; width: 284px; }
  .s6 .side .q { inset-inline-start: 907px; width: 200px; }
  .s6 .side .rule { inset-inline-start: 907px; }
  /* ٧ الأيقونات */
  .s7 .colm { position: absolute; top: 0; width: 98px; height: 720px; background: #EBF5F8; }
  .s7 .band { position: absolute; inset-inline-start: 772px; width: 508px; height: 98px; background: #D1E9EF; }
  .s7 .cell { position: absolute; width: 98px; height: 98px; background: #DCEEF3; display: grid; place-items: center; color: var(--navy); }
  .s7 .cell svg { width: 76px; height: 76px; }
  .s7 .cols .num { top: 372px; }
  /* ٨ أيقونات التطبيق */
  .s8 .tile { position: absolute; width: 151px; height: 151px; border-radius: 26px; overflow: hidden; }
  .s8 .tile::before { content: ""; position: absolute; inset: 0; background: var(--t1); opacity: .75; clip-path: polygon(0 0, 79% 0, 14% 100%, 0 100%); }
  html[dir="rtl"] .s8 .tile::before { transform: scaleX(-1); }
  .s8 .tile .logo { top: 32px; inset-inline-start: 37px; width: 70px; height: 78px; }
  .s8 .a1 { top: 254px; inset-inline-start: 111px; --t1: #1D4B69; background: linear-gradient(180deg, #1D4B69, #15344A); color: #FFFFFF; }
  .s8 .a2 { top: 254px; inset-inline-start: 300px; --t1: #C6EAEE; background: linear-gradient(180deg, #CBEDEE, #A3D4E2); color: var(--navy); }
  .s8 .a3 { top: 452px; inset-inline-start: 111px; --t1: #62B7CC; background: linear-gradient(180deg, #62B7CC, #3892A8); color: #FFFFFF; }
  .s8 .a4 { top: 452px; inset-inline-start: 300px; --t1: #62AAD6; background: linear-gradient(180deg, #62AAD6, #2E80B2); color: #FFFFFF; }
  .s8 .lead { top: 301px; inset-inline-start: 511px; width: 560px; }
  .s8 .side .rule { top: 447px; }
  .s8 .side .q { top: 477px; width: 200px; }
  /* ٩ الخطوط */
  .s9 .tf { position: absolute; top: 250px; width: 460px; height: 360px; }
  .s9 .f1 { inset-inline-start: 111px; } .s9 .f2 { inset-inline-start: 640px; }
  .s9 .tf .lab { position: absolute; top: 0; inset-inline-start: 0; margin: 0; font-size: 20px; letter-spacing: 4.3px; line-height: 1.2; color: var(--ink); }
  html[lang="ar"] .s9 .tf .lab { letter-spacing: 0; }
  .s9 .tf .l1 { position: absolute; top: 34px; inset-inline-start: 0; width: 303px; border-top: 1px solid #D9D9D9; }
  .s9 .tf .nm { position: absolute; top: 52px; inset-inline-start: 0; margin: 0; font-size: 30px; font-weight: 700; line-height: 1.2; color: #0A0A0A; letter-spacing: -0.3px; white-space: nowrap; }
  .s9 .tf .l2 { position: absolute; top: 110px; inset-inline-start: 0; width: 455px; border-top: 1px solid #D9D9D9; }
  .s9 .tf .q { top: 129px; inset-inline-start: 0; width: 330px; }
  .s9 .tf .abc { position: absolute; top: 257px; inset-inline-start: -2px; margin: 0; font-size: 60px; font-weight: 400; line-height: 1.1; color: #0A0A0A; }
  .s9 .tf .alpha { position: absolute; top: 266px; inset-inline-start: 204px; margin: 0; font-size: 17px; line-height: 22.5px; color: #0A0A0A; width: 260px; }
  html[lang="ar"] .s9 .tf .alpha { font-size: 18px; line-height: 24px; top: 262px; }
  /* ١٠ لوحة الإلهام */
  .s10 .h, .s11 .h { line-height: 70px; top: 141px; white-space: normal; }
  html[lang="ar"] .s10 .h, html[lang="ar"] .s11 .h { line-height: 78px; top: 128px; }
  .s10 .lead, .s11 .lead { top: 311px; width: 410px; } .s11 .lead { width: 318px; }
  .duo .rule { top: 472px; }
  .duo .q { top: 500px; width: 200px; }
  .s10 .ph { position: absolute; object-fit: cover; display: block; }
  .s10 .blk { position: absolute; }
  .s10 .blk .q { color: #FFFFFF; }
  /* ١١ نماذج العلامة: الخطاب */
  .s11 .paper { position: absolute; top: 134px; inset-inline-start: 535px; width: 334px; height: 492px; border: 1px solid #BFBFBF; background: #fff; }
  .s11 .paper .tx { position: absolute; inset-inline-start: 26px; width: 256px; margin: 0; font-size: 15px; line-height: 19.5px; color: #0A0A0A; }
  html[lang="ar"] .s11 .paper .tx { line-height: 22px; font-size: 16px; }
  .s11 .paper b { font-weight: 700; }
  .s11 .paper .logo { top: 395px; inset-inline-start: 22px; width: 43px; height: 48px; color: var(--navy); }
  .s11 .paper .lt { top: 404px; inset-inline-start: 85px; font-size: 12px; letter-spacing: 3px; color: #1A1A1A; line-height: 16px; }
  html[lang="ar"] .s11 .paper .lt { font-size: 14px; letter-spacing: 0; line-height: 18px; }
  .s11 .cover { position: absolute; top: 134px; inset-inline-start: 882px; width: 333px; height: 492px; overflow: hidden; background: linear-gradient(225deg, #C2EBE9 0%, #A6DADE 35%, #7CC2D3 68%, #5DB2C9 100%); }
  .s11 .cover::before { content: ""; position: absolute; top: 60px; inset-inline-start: 40px; width: 190px; height: 190px; border-radius: 50%; background: radial-gradient(circle, rgb(58 152 178 / .95) 0%, rgb(84 170 192 / .5) 38%, rgb(120 195 210 / 0) 70%); }
  .s11 .cover .logo { top: 163px; inset-inline-start: 129px; width: 74px; height: 82px; color: var(--navy); }
  .s11 .cover .lt { top: 276px; inset-inline: 0; text-align: center; font-size: 18px; letter-spacing: 3.2px; color: #1F4A66; }
  html[lang="ar"] .s11 .cover .lt { font-size: 20px; letter-spacing: 0; }
  /* ١٢ نماذج العلامة: صور */
  .s12 .app { position: absolute; top: 265px; inset-inline-start: 111px; width: 226px; height: 317px; background: var(--navy); border-start-start-radius: 60px; }
  .s12 .app .ico { position: absolute; top: 94px; inset-inline-start: 60px; width: 100px; height: 100px; border-radius: 18px; background: #fff; }
  .s12 .app .ico .logo { top: 18px; inset-inline-start: 23px; width: 54px; height: 62px; color: var(--navy); }
  .s12 .app .badge { position: absolute; top: 84px; inset-inline-start: 140px; width: 30px; height: 30px; border-radius: 50%; background: #67B3CB; border: 2px solid #fff; color: #fff; font-size: 15px; display: grid; place-items: center; line-height: 1; }
  .s12 .app .lt { top: 210px; inset-inline: 0; text-align: center; color: #fff; font-size: 16px; letter-spacing: 2.8px; }
  html[lang="ar"] .s12 .app .lt { font-size: 18px; letter-spacing: 0; }
  .s12 .ph { position: absolute; display: block; object-fit: cover; }
  /* ١٣ الختام */
  .s13 .ty { position: absolute; top: 137px; inset-inline-start: 109px; margin: 0; font-size: 56px; line-height: 70px; color: #fff; font-weight: 700; letter-spacing: -0.6px; }
  .s13 .ty span { display: block; color: #62B4CB; font-weight: 400; letter-spacing: -0.3px; }
  html[lang="ar"] .s13 .ty { font-size: 58px; line-height: 76px; top: 126px; font-weight: 800; letter-spacing: 0; }
  html[lang="ar"] .s13 .ty span { font-weight: 500; }
  .s13 .lead { color: #fff; top: 311px; }
  .s13 .logo { top: 478px; inset-inline-start: 104px; width: 77px; height: 85px; color: #fff; }
  .s13 .lt.big { top: 493px; inset-inline-start: 211px; font-size: 20.5px; letter-spacing: 2.9px; color: #fff; line-height: 25px; }
  html[lang="ar"] .s13 .lt.big { letter-spacing: 0; font-size: 23px; line-height: 1.45; }
  .s13 .dot { position: absolute; width: 94px; height: 94px; border-radius: 50%; background: #fff; display: grid; place-items: center; color: var(--sky); }
  .s13 .dot svg { width: 62px; height: 62px; }
  .s13 .ct { position: absolute; inset-inline-start: 951px; width: 200px; }
  .s13 .ct h3 { margin: 0; font-size: 20px; font-weight: 700; line-height: 1.2; color: #0A0A0A; }
  .s13 .ct p { margin: 12px 0 0; font-size: 18px; line-height: 1.2; color: #0A0A0A; }
  .s13 .sep { position: absolute; inset-inline-start: 950px; width: 197px; border-top: 1px solid #CFE6EC; }"""

# الشعار التجريدي: قطعتان (علوية يمنى وسفلية يسرى) بلون currentColor
LOGO = ('<svg viewBox="5 4 89 99" fill="currentColor" aria-hidden="true">'
        '<path d="M19 20C20 10 24 4.5 32 5C49 6 68 10 83 15.5C90 18 93.5 23 93.5 32L93.5 58C93.5 67 90.5 72.5 82.5 75.5L80.5 75C81 61 81 47 79.5 37C78.5 30.5 74 27 66 24.8C51 21.2 35 19.6 19 20Z"/>'
        '<path d="M19 20C10.5 21.5 5.5 27 5.5 38L5.5 62C5.5 80 7.5 92 13.5 98C17 101.5 22 102.5 28 101.5C45 99 61 95.5 72 91.5C79 89 82.5 85 82.5 75.5L80.5 77C64 81 46 83 31 82.8C24.5 82.6 21.5 78.6 20.5 72C18.5 60 17.5 48 17.5 38C17.5 30 18 24 19 20Z"/>'
        '</svg>')


def logo(cls=''):
    return '<span class="logo%s">%s</span>' % ((' ' + cls) if cls else '', LOGO)


# خلفية الغلاف والختام: هلال متدرّج ودائرة كحلية (تُقلب رأسيًا في الختام)
ARC = ('<svg class="bg-arc" viewBox="0 0 1280 720" aria-hidden="true">'
       '<circle cx="276" cy="190" r="634" fill="url(#bi-arc)"/><circle cx="272" cy="345" r="517" fill="#1B4865"/></svg>')

GLOBAL_DEFS = ('<defs><linearGradient id="bi-arc" gradientUnits="userSpaceOnUse" x1="0" y1="0" x2="0" y2="720">'
               '<stop offset="0" stop-color="#99C7E2"/><stop offset=".55" stop-color="#A6D4E4"/><stop offset="1" stop-color="#B8E6E7"/></linearGradient></defs>')

# أيقونات خطية (64×64) لشريحة الأيقونات
LINE = 'fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round"'
ICONS7 = [
    # طائرة
    '<svg viewBox="0 0 64 64" %s><path d="M32 6c2 0 3 2.5 3 5v12l17 9v4l-17-5v13l5 4v3l-8-2-8 2v-3l5-4V31l-17 5v-4l17-9V11c0-2.5 1-5 3-5z"/></svg>' % LINE,
    # فتحة عدسة
    '<svg viewBox="0 0 64 64" %s><circle cx="32" cy="32" r="21"/><path d="M27.1 23.5L51.2 23.5M36.9 23.5L49.0 44.4M41.8 32.0L29.8 52.9M36.9 40.5L12.8 40.5M27.1 40.5L15.0 19.6M22.2 32.0L34.2 11.1"/></svg>' % LINE,
    # زر بأربعة ثقوب
    '<svg viewBox="0 0 64 64" %s><circle cx="32" cy="32" r="23"/><circle cx="32" cy="32" r="19"/><circle cx="27.5" cy="27.5" r="2.6"/><circle cx="36.5" cy="27.5" r="2.6"/><circle cx="27.5" cy="36.5" r="2.6"/><circle cx="36.5" cy="36.5" r="2.6"/></svg>' % LINE,
    # تحديث دائري
    '<svg viewBox="0 0 64 64" %s><path d="M22.5 15.5A19 19 0 0 1 46 19"/><path d="M41 14.5l5.5 4.5-6.5 2"/><path d="M51 30a19 19 0 0 1-10 19.5"/><path d="M45.5 53l-5-3.5 1-6.5"/><path d="M27.5 51A19 19 0 0 1 13.5 32"/><path d="M10 37.5l3.5-5.5 5.5 4"/></svg>' % LINE,
    # ذرّة
    '<svg viewBox="0 0 64 64" %s><ellipse cx="32" cy="32" rx="9" ry="24"/><ellipse cx="32" cy="32" rx="24" ry="9" transform="rotate(30 32 32)"/><ellipse cx="32" cy="32" rx="24" ry="9" transform="rotate(-30 32 32)"/><circle cx="32" cy="32" r="3.2"/></svg>' % LINE,
    # دائرة زائد
    '<svg viewBox="0 0 64 64" %s><circle cx="32" cy="32" r="23"/><path d="M32 22v20M22 32h20"/></svg>' % LINE,
    # قائمة مهام
    '<svg viewBox="0 0 64 64" %s><rect x="14" y="7" width="36" height="49"/><path d="M19 16l2.5 2.5 4.5-5M19 25l2.5 2.5 4.5-5M19 34l2.5 2.5 4.5-5M19 43l2.5 2.5 4.5-5M30 16.5h14M30 25.5h14M30 34.5h14M30 43.5h14"/></svg>' % LINE,
    # هدف وسهم
    '<svg viewBox="0 0 64 64" %s><circle cx="29" cy="35" r="22"/><circle cx="29" cy="35" r="15"/><circle cx="29" cy="35" r="7.5"/><path d="M29 35L52 12"/><path d="M47 9.5l-1 6 6 -1 4-4.5-5.5.5.5-5z"/></svg>' % LINE,
    # خلايا سداسية
    '<svg viewBox="0 0 64 64" %s><path d="M37.2 22.5L33.6 28.7L26.4 28.7L22.8 22.5L26.4 16.3L33.6 16.3ZM48.0 28.7L44.4 34.9L37.2 34.9L33.6 28.7L37.2 22.5L44.4 22.5ZM26.4 28.7L22.8 34.9L15.6 34.9L12.0 28.7L15.6 22.5L22.8 22.5ZM37.2 35.0L33.6 41.2L26.4 41.2L22.8 35.0L26.4 28.8L33.6 28.8ZM48.0 41.2L44.4 47.4L37.2 47.4L33.6 41.2L37.2 35.0L44.4 35.0ZM26.4 16.3L23 10.5M44.4 47.4L48 53.5M15.6 22.5L12 16.5" transform="translate(3 4)"/></svg>' % LINE,
    # نقاط
    '<svg viewBox="0 0 64 64" %s><circle cx="13" cy="22" r="3.3"/><circle cx="25" cy="22" r="3.3"/><circle cx="51" cy="22" r="3.3"/><circle cx="13" cy="33" r="3.3"/><circle cx="39" cy="33" r="3.3"/><circle cx="13" cy="44" r="3.3"/><circle cx="39" cy="44" r="3.3"/><circle cx="39" cy="22" r=".9" fill="currentColor"/><circle cx="25" cy="33" r=".9" fill="currentColor"/><circle cx="51" cy="33" r=".9" fill="currentColor"/><circle cx="25" cy="44" r=".9" fill="currentColor"/><circle cx="51" cy="44" r=".9" fill="currentColor"/></svg>' % LINE,
    # أعمدة
    '<svg viewBox="0 0 64 64" %s><path d="M10 6v50h48"/><rect x="16" y="24" width="7" height="28"/><rect x="27" y="10" width="7" height="42"/><rect x="38" y="22" width="7" height="30"/><rect x="49" y="38" width="7" height="14"/></svg>' % LINE,
    # أسهم مزدوجة
    '<svg viewBox="0 0 64 64" %s><path d="M8 14l14 18-14 18M23 14l14 18-14 18M38 14l14 18-14 18"/></svg>' % LINE,
]

ICONS13 = {
    'mail': '<svg viewBox="0 0 64 64" fill="currentColor" aria-hidden="true"><path d="M32 4l9 7H23z"/><rect x="18" y="11" width="28" height="22" fill="currentColor"/><path d="M22 16h20M22 21h20M22 26h14" stroke="#fff" stroke-width="2.2"/><path d="M6 25l12-9v12zM58 25l-12-9v12z"/><path d="M6 27v31h52V27L32 45z"/><path d="M8 57l18-16M56 57L38 41" stroke="#fff" stroke-width="2.4"/><path d="M6 27l26 18 26-18" fill="none" stroke="#fff" stroke-width="2.4"/></svg>',
    'phone': '<svg viewBox="0 0 64 64" aria-hidden="true"><path fill="currentColor" d="M14 14c2-2 4.5-2 6 0l5 7c1.2 1.8 1 3.6-.6 5l-3 2.6c2 4.8 7 10.8 12 13.6l2.8-3c1.4-1.6 3.4-1.8 5.2-.6l6.8 5c2 1.5 2 4 0 6l-3.4 3.4c-3 3-9.6 1.8-17-4C20 43 12 32.5 11 24.8 10.5 20.6 11.6 16.4 14 14z"/><path fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" d="M34 22a10 10 0 0 1 8 8M35 13a19 19 0 0 1 16 16M36 4a28 28 0 0 1 24 24"/></svg>',
    'globe': '<svg viewBox="0 0 64 64" fill="none" stroke="currentColor" stroke-width="3.4" aria-hidden="true"><circle cx="32" cy="32" r="25"/><ellipse cx="32" cy="32" rx="11" ry="25"/><path d="M32 7v50M7 32h50M11 20h42M11 44h42"/></svg>',
}


def item_cols(xs, n0=1):
    out = ''
    for i, x in enumerate(xs):
        k = n0 + i
        out += ('<p class="num" style="inset-inline-start:%dpx">%02d</p><span class="rule" style="inset-inline-start:%dpx"></span><p class="q" style="inset-inline-start:%dpx">§C%d§</p>'
                % (x, k, x, x, k))
    return out


S1 = ('\n    ' + ARC + '<h1 class="cv">§CV1§<br>§CV2§</h1><p class="sub">§CVSUB§</p>' + logo() + '<p class="lt">§LOGO1§<br>§LOGO2§</p>\n  ')

S2 = '\n    <h1 class="h">§T2§</h1>' + ''.join(
    '<div class="it c%d r%d"><p class="num">%02d</p><p class="p">§TOC§</p><span class="bar"></span><span class="ln"></span></div>' % (c, r, (r - 1) * 3 + c)
    for r in (1, 2) for c in (1, 2, 3)) + '\n  '

S3 = ('\n    <h1 class="h">§T3§</h1>'
      '<div class="blk b1">' + logo() + '<p class="lt">§LOGO1§<br>§LOGO2§</p></div>'
      '<div class="blk b2">' + logo() + '<p class="lt">§LOGO1§<br>§LOGO2§</p></div>'
      '<p class="kick">§KICK§</p><h2 class="h2">§OURBRAND§</h2><p class="p body">§BODY3§</p><p class="p body2">§BODY3B§</p>'
      '<div class="side"><p class="q" style="top:312px">§Q§</p><span class="rule" style="top:405px"></span><p class="q" style="top:434px">§Q§</p><span class="rule" style="top:526px"></span><p class="q" style="top:556px">§Q§</p></div>\n  ')

S4 = ('\n    <div class="corner">' + logo() + '<p class="lt">§LOGO1§<br>§LOGO2§</p></div><h1 class="h">§T4§</h1><p class="p lead">§LEAD§</p>'
      '<div class="cols">' + item_cols([111, 341, 570, 800]) + '</div>\n  ')

S5 = ('\n    <h1 class="h">§T5§</h1><p class="p intro">§INTRO5§</p>'
      '<div class="card k1"><h3>§K1§</h3><span class="bar"></span>' + logo() + '<p class="lt">§LOGO1§<br>§LOGO2§</p></div>'
      '<div class="card k2"><h3>§K2§</h3><span class="bar"></span>' + logo() + '</div>'
      '<div class="card k3"><h3>§K3§</h3><span class="bar"></span>' + logo() + '<p class="lt">§YLOGO§</p></div>\n  ')


def sw(x, y, c):
    return '<span class="sw" style="top:%dpx;inset-inline-start:%dpx;background:%s"></span>' % (y, x, c)


VALS = '<p class="vals"><span>100<br>200<br>300</span><span>100<br>200<br>300<br>400</span></p>'
S6 = ('\n    <h1 class="h">§T6§</h1>'
      + sw(111, 238, '#14374B') + sw(174, 238, '#4A9DCF') + sw(237, 238, '#86BCDE') + sw(300, 238, '#C3DEEF')
      + sw(363, 587, '#3893A8') + sw(426, 587, '#9FD3E0') + sw(489, 587, '#C0E2EB') + sw(552, 587, '#DEF0F4')
      + '<div class="blk pr"><h3>§CP§</h3><p class="hex">#000000</p>' + VALS + '</div>'
      + '<div class="blk se"><h3>§CS§</h3><p class="hex">#000000</p>' + VALS + '</div>'
      + '<div class="blk ne"><h3>§CN§</h3><p class="hex">#000000</p>' + VALS + '</div>'
      + '<span class="band" style="top:512px;background:#9CD6FE"></span><span class="band" style="top:537px;background:#58BBFF"></span><span class="band" style="top:562px;background:#0086E5"></span>'
      + '<p class="p intro">§INTRO6§</p><div class="side"><span class="rule" style="top:389px"></span><p class="q" style="top:418px">§C1§</p><span class="rule" style="top:512px"></span><p class="q" style="top:541px">§C1§</p></div>\n  ')

COLX = [772, 917, 1071]
ROWY = [127, 237, 345, 453]
S7 = ('\n    ' + ''.join('<span class="colm" style="inset-inline-start:%dpx"></span>' % x for x in COLX)
      + ''.join('<span class="band" style="top:%dpx"></span>' % y for y in ROWY)
      + ''.join('<span class="cell" style="top:%dpx;inset-inline-start:%dpx">%s</span>' % (y, x, ICONS7[r * 3 + c]) for r, y in enumerate(ROWY) for c, x in enumerate(COLX))
      + '<h1 class="h">§T7§</h1><p class="p lead">§LEAD§</p><div class="cols">' + item_cols([111, 312, 513]) + '</div>\n  ')

S8 = ('\n    <h1 class="h">§T8§</h1>'
      + ''.join('<span class="tile a%d">%s</span>' % (i, logo()) for i in (1, 2, 3, 4))
      + '<p class="p lead">§LEAD§</p><div class="side">'
      + ''.join('<span class="rule" style="inset-inline-start:%dpx"></span><p class="q" style="inset-inline-start:%dpx">§Q§</p>' % (x, x) for x in (511, 735, 972))
      + '</div>\n  ')


def face(cls, lab):
    return ('<div class="tf %s"><p class="lab">§%s§</p><span class="l1"></span><h3 class="nm">§TFN§</h3><span class="l2"></span>'
            '<p class="q">§TFT§</p><p class="abc">§ABC§</p><p class="alpha">§ALPHA§</p></div>' % (cls, lab))


S9 = '\n    <h1 class="h">§T9§</h1>' + face('f1', 'TFP') + face('f2', 'TFS') + '\n  '

DUO = '<div class="duo">' + ''.join('<span class="rule" style="inset-inline-start:%dpx"></span><p class="q" style="inset-inline-start:%dpx">§C1§</p>' % (x, x) for x in (111, 312)) + '</div>'


def ph(name, alt, x, y, w, h):
    return ('<img class="ph" src="/templates/freeform/sl-brand-identity/assets/%s.jpg" alt="§%s§" style="top:%dpx;inset-inline-start:%dpx;width:%dpx;height:%dpx">'
            % (name, alt, y, x, w, h))


S10 = ('\n    <h1 class="h">§T10A§<br>§T10B§</h1><p class="p lead">§BODY10§</p>' + DUO
       + ph('mood-laptop', 'ALT1', 564, 0, 420, 235) + ph('mood-tower', 'ALT2', 564, 235, 265, 422)
       + ph('mood-code', 'ALT3', 829, 235, 451, 250) + ph('mood-meeting', 'ALT4', 829, 485, 451, 235)
       + '<div class="blk" style="top:0;inset-inline-start:984px;width:296px;height:235px;background:var(--navy)"><p class="q" style="top:108px;inset-inline-start:34px;width:220px">§C1§</p></div>'
       + '<span class="blk" style="top:657px;inset-inline-start:564px;width:265px;height:63px;background:#61B6CB"></span>\n  ')

S11 = ('\n    <h1 class="h">§T11A§<br>§T11B§</h1><p class="p lead">§BODY10§</p>' + DUO
       + '<div class="paper"><p class="tx" style="top:81px"><b>§NAME§</b><br>§ROLE§</p><p class="tx" style="top:168px">§LET1§</p>'
       '<p class="tx" style="top:244px">§LET2§</p><p class="tx" style="top:302px">§SINC§<br>§TXT§</p>' + logo() + '<p class="lt">§YL1§<br>§YL2§</p></div>'
       '<div class="cover">' + logo() + '<p class="lt">§YLOGO§</p></div>\n  ')

S12 = ('\n    <h1 class="h">§T12§</h1><div class="app"><span class="ico">' + logo() + '</span><span class="badge">1</span><p class="lt">§APP§</p></div>'
       + ph('mock-card', 'ALT5', 337, 265, 355, 318) + ph('mock-letterhead', 'ALT6', 692, 60, 488, 523) + '\n  ')

S13 = ('\n    ' + ARC + '<h1 class="ty">§TY§<span>§CWU§</span></h1><p class="p lead">§LEAD§</p>' + logo() + '<p class="lt big">§LOGO1§<br>§LOGO2§</p>'
       + '<span class="dot" style="top:189px;inset-inline-start:803px">%s</span>' % ICONS13['mail']
       + '<span class="dot" style="top:308px;inset-inline-start:844px">%s</span>' % ICONS13['phone']
       + '<span class="dot" style="top:431px;inset-inline-start:858px">%s</span>' % ICONS13['globe']
       + '<div class="ct" style="top:214px"><h3>§EM§</h3><p>§YT§</p></div><span class="sep" style="top:303px"></span>'
       + '<div class="ct" style="top:330px;inset-inline-start:955px"><h3>§CO§</h3><p>§YT§</p></div><span class="sep" style="top:420px"></span>'
       + '<div class="ct" style="top:447px;inset-inline-start:955px"><h3>§WE§</h3><p>§YT§</p></div>\n  ')

SLIDES = [S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13]

STR = {
    'ar': {
        'CV1': 'هوية', 'CV2': 'العلامة التجارية', 'CVSUB': 'قالب عرض تقديمي', 'LOGO1': 'شعار شركتك', 'LOGO2': 'يوضع هنا',
        'T2': 'جدول المحتويات', 'TOC': 'أدخل النص<br>المطلوب هنا.',
        'T3': 'شعارات العلامة', 'KICK': 'القصة وراء', 'OURBRAND': 'علامتنا التجارية',
        'BODY3': 'أدخل النص المطلوب هنا. هذا نص توضيحي. عبارة موجزة عن موضوعك.', 'BODY3B': 'أدخل النص المطلوب هنا. هذا نص توضيحي.',
        'Q': 'أدخل النص<br>المطلوب هنا.', 'C1': 'أدخل النص<br>المطلوب هنا.', 'C2': 'أدخل النص<br>المطلوب هنا.', 'C3': 'أدخل النص<br>المطلوب هنا.', 'C4': 'أدخل النص<br>المطلوب هنا.',
        'T4': 'رسالتنا', 'LEAD': 'أدخل النص المطلوب هنا. هذا نص توضيحي.<br>عبارة موجزة عن موضوعك.',
        'T5': 'دليل استخدام الشعار', 'INTRO5': 'أدخل النص المطلوب هنا. هذا نص توضيحي. عبارة موجزة عن موضوعك.',
        'K1': 'الشعار الأساسي', 'K2': 'الرمز', 'K3': 'الشعار العمودي', 'YLOGO': 'شعارك هنا',
        'T6': 'دليل الألوان', 'CP': 'اللون<br>الأساسي', 'CS': 'اللون<br>الثانوي', 'CN': 'اللون<br>المحايد',
        'INTRO6': 'أدخل النص المطلوب هنا. هذا نص توضيحي. عبارة موجزة عن موضوعك.',
        'T7': 'الأيقونات', 'T8': 'أيقونات التطبيق',
        'T9': 'الخطوط', 'TFP': 'الخط الأساسي', 'TFS': 'الخط الثانوي', 'TFN': 'اسم الخط هنا', 'TFT': 'أدخل النص المطلوب هنا.<br>هذا نص توضيحي.',
        'ABC': 'أبجد', 'ALPHA': 'ا ب ت ث ج ح خ د ذ ر ز س ش<br>ص ض ط ظ ع غ ف ق ك ل م ن<br>هـ و ي ء ة ى<br>0123456789',
        'T10A': 'لوحة الإلهام', 'T10B': 'البصري', 'BODY10': 'أدخل النص المطلوب هنا. هذا نص توضيحي. عبارة موجزة عن موضوعك.',
        'ALT1': 'شاب يستخدم هاتفه وحاسوبه المحمول في مقهى', 'ALT2': 'برج زجاجي حديث من زاوية منخفضة', 'ALT3': 'امرأة تنعكس عليها شيفرة برمجية ملوّنة', 'ALT4': 'اجتماع عمل حول طاولة عليها حاسوب وقهوة',
        'T11A': 'نماذج تطبيق', 'T11B': 'العلامة', 'NAME': 'أدخل الاسم هنا', 'ROLE': 'المنصب هنا',
        'LET1': 'أدخل النص المطلوب هنا. هذا نص توضيحي. عبارة موجزة عن موضوعك.', 'LET2': 'أدخل النص المطلوب هنا. هذا نص توضيحي.',
        'SINC': 'مع خالص التحية،', 'TXT': 'النص هنا', 'YL1': 'شعارك', 'YL2': 'هنا',
        'T12': 'نماذج تطبيق العلامة', 'APP': 'تطبيقك هنا', 'ALT5': 'بطاقة عمل عليها الشعار', 'ALT6': 'ورقة مراسلات رسمية عليها الشعار',
        'TY': 'شكرًا لكم', 'CWU': 'تواصل معنا', 'EM': 'البريد الإلكتروني', 'CO': 'الهاتف', 'WE': 'الموقع الإلكتروني', 'YT': 'أدخل النص هنا',
    },
    'en': {
        'CV1': 'BRAND', 'CV2': 'IDENTITY', 'CVSUB': 'PRESENTATION TEMPLATE', 'LOGO1': 'YOUR COMPANY', 'LOGO2': 'LOGO HERE',
        'T2': 'Table of Contents', 'TOC': 'Insert your<br>desired text here.',
        'T3': 'Brandmarks', 'KICK': 'The story behind', 'OURBRAND': 'Our Brand',
        'BODY3': 'Insert your desired text here. This is a sample text. A brief statement about your topic.', 'BODY3B': 'Insert your desired text here. This is a sample text.',
        'Q': 'Insert your desired<br>text here.', 'C1': 'Insert your<br>desired text here.', 'C2': 'Insert your<br>desired text here.', 'C3': 'Insert your<br>desired text here.', 'C4': 'Insert your<br>desired text here.',
        'T4': 'Our Mission', 'LEAD': 'Insert your desired text here. This is a sample text.<br>A brief statement about your topic.',
        'T5': 'Logo Usage Guide', 'INTRO5': 'Insert your desired text here. This is a sample text. A brief statement about your topic.',
        'K1': 'PRIMARY LOGO', 'K2': 'LOGOGRAM', 'K3': 'VERTICAL LOGO', 'YLOGO': 'YOUR LOGO HERE',
        'T6': 'Color Guide', 'CP': 'Primary<br>Color', 'CS': 'Secondary<br>Color', 'CN': 'Neutral<br>Color',
        'INTRO6': 'Insert your desired text here. This is a sample text. A brief statement about your topic.',
        'T7': 'Iconography', 'T8': 'App Icons',
        'T9': 'Typeface', 'TFP': 'Primary', 'TFS': 'Secondary', 'TFN': 'Typeface Name Here', 'TFT': 'Insert your desired text here.<br>This is a sample text.',
        'ABC': 'AbCd', 'ALPHA': 'ABCDEFGHIJKLMNOPQRSTU<br>VWXYZ<br>abcdefghijklmnopqrstuvwxyz<br>0123456789',
        'T10A': 'Imagery', 'T10B': 'Moodboard', 'BODY10': 'Insert your desired text here. This is a sample text. A brief statement about your topic.',
        'ALT1': 'Man using his phone and laptop in a cafe', 'ALT2': 'Modern glass tower seen from below', 'ALT3': 'Woman lit by colorful projected code', 'ALT4': 'Business meeting at a table with laptop and coffee',
        'T11A': 'Brand', 'T11B': 'Mockups', 'NAME': 'Insert name here', 'ROLE': 'Role here',
        'LET1': 'Insert your desired text here. This is a sample text.  A brief statement about your topic.', 'LET2': 'Insert your desired text here. This is a sample text.',
        'SINC': 'Sincerely,', 'TXT': 'Text here', 'YL1': 'YOUR', 'YL2': 'LOGO HERE',
        'T12': 'Brand Mockups', 'APP': 'YOUR APP HERE', 'ALT5': 'Business card with the logo', 'ALT6': 'Letterhead paper with the logo',
        'TY': 'Thank You', 'CWU': 'Connect With Us', 'EM': 'Email', 'CO': 'Contact', 'WE': 'Website', 'YT': 'Insert your text here',
    },
}

FONTS = {'ar': '<link href="https://fonts.googleapis.com/css2?family=Tajawal:wght@400;500;700;800&display=swap" rel="stylesheet">',
         'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:ital,wght@0,300;0,400;0,600;0,700;1,400&display=swap" rel="stylesheet">'}
FONT_STACK = {'ar': "'Tajawal', 'Segoe UI', Tahoma, sans-serif", 'en': "'Open Sans', 'Segoe UI', Arial, sans-serif"}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'marketing', 'icon': '🎨',
    'name': 'هوية العلامة التجارية', 'nameEn': 'Brand Identity',
    'description': 'دليل هوية في 13 شريحة: غلاف وفهرس وشعارات ورسالة واستخدام الشعار والألوان والأيقونات والخطوط والصور ونماذج التطبيق',
    'descriptionEn': '13-slide brand guide: cover, contents, brandmarks, mission, logo usage, colors, icons, typeface, imagery, mockups',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'جدول المحتويات', 'titleEn': 'Table of Contents'},
        {'title': 'شعارات العلامة', 'titleEn': 'Brandmarks'},
        {'title': 'رسالتنا', 'titleEn': 'Our Mission'},
        {'title': 'دليل استخدام الشعار', 'titleEn': 'Logo Usage Guide'},
        {'title': 'دليل الألوان', 'titleEn': 'Color Guide'},
        {'title': 'الأيقونات', 'titleEn': 'Iconography'},
        {'title': 'أيقونات التطبيق', 'titleEn': 'App Icons'},
        {'title': 'الخطوط', 'titleEn': 'Typeface'},
        {'title': 'لوحة الإلهام البصري', 'titleEn': 'Imagery Moodboard'},
        {'title': 'نماذج تطبيق العلامة (الخطاب)', 'titleEn': 'Brand Mockups (Letter)'},
        {'title': 'نماذج تطبيق العلامة (صور)', 'titleEn': 'Brand Mockups (Photos)'},
        {'title': 'شكرًا وتواصل معنا', 'titleEn': 'Thank You & Contact'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والألوان وغيّر النصوص فقط؛ الشعار رسم SVG مشترك يمكن استبداله بشعار الشركة، والصور في مجلد assets تبقى كما هي ما لم يُطلب تغييرها.',
}
