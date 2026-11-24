# -*- coding: utf-8 -*-
# نموذج «إدارة التغيير بالذكاء الاصطناعي»: 15 شريحة بهوية بنفسجية/وردية، صور فوتوغرافية مقصوصة من الأصل،
# أزرار سوداء بيضاوية مع دائرة وردية بسهم، بطاقات رمادية فاتحة، رسمان بيانيان SVG ودوائر متداخلة بالقيم.
KEY = 'sl-ai-driven-change-management'
TITLE = {'ar': 'إدارة التغيير بالذكاء الاصطناعي', 'en': 'AI-Driven Change Management'}
COMMENT = ('نموذج «إدارة التغيير بالذكاء الاصطناعي»: غلاف بصورة، فهرس، مقدمة، أهمية الذكاء الاصطناعي، التحدي، النموذج المعزّز، '
           'قبل وبعد، الأدوات، الأثر على القوى العاملة، خطة التواصل، دراسة حالة، مؤشرات الأداء (رسمان)، المخاطر (دوائر)، '
           'الجدول الزمني، الخطوات التالية. الإطار 1280×720 بخصائص CSS منطقية فتنعكس الصفحة العربية تلقائيًا.')
VARS = ('--ink: #111111; --ink-2: #333333; --ink-3: #444444; --accent: #7B02DB; --purple-2: #6A0DAD; --purple-3: #AF42FF; '
        '--purple-4: #CA82FF; --indigo: #4B0082; --pink: #FF47FF; --pink-2: #FF7DFE; --pink-3: #FFC9FF; --lav: #E1D6FF; '
        '--card: #F2F2F2; --track: #EDEDED; --bg: #FFFFFF;')

STYLE = """  /* خلفيات الشرائح: بقعتان ناعمتان (بنفسجي فاتح ووردي) تُعكسان في العربية */
  .frame::before { content: ''; position: absolute; inset: 0; pointer-events: none; }
  .bgA::before { background: radial-gradient(420px 320px at 2% 2%, #ECE4FF 0%, rgb(236 228 255 / 0) 70%), radial-gradient(360px 300px at 100% 100%, #FBE2FF 0%, rgb(251 226 255 / 0) 70%); }
  .bgB::before { background: radial-gradient(420px 320px at 100% 0%, #ECE4FF 0%, rgb(236 228 255 / 0) 70%), radial-gradient(360px 300px at 0% 100%, #FBE2FF 0%, rgb(251 226 255 / 0) 70%); }
  .bgC::before { background: radial-gradient(420px 320px at 100% 0%, #FBE2FF 0%, rgb(251 226 255 / 0) 70%), radial-gradient(360px 300px at 0% 100%, #ECE4FF 0%, rgb(236 228 255 / 0) 70%); }
  html[dir="rtl"] .frame::before { transform: scaleX(-1); }
  .frame > * { position: absolute; }
  /* عناصر مشتركة */
  .ttl { top: 100px; inset-inline-start: 84px; margin: 0; font-size: 44px; font-weight: 700; line-height: 52px; color: var(--ink); letter-spacing: -0.3px; }
  .ttl .lt { font-weight: 400; }
  html[lang="ar"] .ttl { font-weight: 800; }
  .p { margin: 0; font-size: 18px; line-height: 25px; color: var(--ink-2); }
  .sub { margin: 0; font-size: 22px; font-weight: 700; line-height: 1.25; color: var(--ink); }
  .btn { display: flex; align-items: center; gap: 8px; }
  .btn .pill { background: var(--ink); color: #fff; font-weight: 700; border-radius: 999px; display: flex; align-items: center; justify-content: center; margin: 0; }
  .btn .arr { background: var(--pink-2); color: #fff; border-radius: 50%; display: grid; place-items: center; flex: none; }
  .btn .arr svg { width: 55%; height: 55%; }
  html[dir="rtl"] .arr svg { transform: scaleX(-1); }
  .card { background: var(--card); border-radius: 14px; }
  .ph { object-fit: cover; display: block; }
  .ic { color: var(--accent); display: block; }
  .ic svg { width: 100%; height: 100%; }
  /* 1 الغلاف */
  .s1 .hero { top: 60px; inset-inline-start: 73px; width: 1134px; height: 600px; border-radius: 28px; overflow: hidden; background: #9E629E; }
  .s1 .hero img { position: absolute; top: 0; inset-inline-start: 0; width: 1134px; height: 380px; }
  .s1 .hero::after { content: ''; position: absolute; inset-inline-start: 0; top: 300px; width: 100%; height: 90px; background: linear-gradient(rgb(158 98 158 / 0), #9E629E); }
  .s1 .panel { top: 440px; inset-inline-start: 73px; width: 682px; height: 220px; background: var(--bg); border-start-end-radius: 28px; }
  .s1 .big { top: 488px; inset-inline-start: 100px; margin: 0; font-size: 52px; font-weight: 800; line-height: 58px; text-transform: uppercase; letter-spacing: -0.5px; background: linear-gradient(90deg, #6A0DAD 0%, #9C27F0 55%, #FF2FFF 100%); -webkit-background-clip: text; background-clip: text; color: transparent; }
  html[dir="rtl"] .s1 .big { background: linear-gradient(270deg, #6A0DAD 0%, #9C27F0 55%, #FF2FFF 100%); -webkit-background-clip: text; background-clip: text; }
  html[lang="ar"] .s1 .big { font-size: 50px; line-height: 60px; top: 482px; }
  .s1 .tag { top: 620px; inset-inline-start: 100px; margin: 0; font-size: 26px; font-weight: 400; letter-spacing: 2px; color: var(--ink); text-transform: uppercase; }
  html[lang="ar"] .s1 .tag { letter-spacing: 0; font-size: 24px; }
  .s1 .btn { top: 615px; inset-inline-start: 520px; }
  .s1 .btn .pill { width: 138px; height: 44px; font-size: 17px; }
  .s1 .btn .arr { width: 44px; height: 44px; }
  .s1 .logo { top: 530px; inset-inline-start: 878px; display: flex; align-items: center; gap: 24px; color: #F2E6F2; }
  .s1 .logo svg { width: 58px; height: 58px; flex: none; }
  .s1 .logo p { margin: 0; font-size: 14px; letter-spacing: 1.5px; line-height: 19px; text-transform: uppercase; width: 150px; }
  html[lang="ar"] .s1 .logo p { letter-spacing: 0; font-size: 16px; }
  /* 2 الفهرس */
  .s2 .pic { top: 68px; inset-inline-start: 73px; width: 651px; height: 584px; border-radius: 22px; overflow: hidden; background: #640062; }
  .s2 .pic img { position: absolute; top: 0; inset-inline-start: 0; width: 651px; height: 470px; }
  .s2 .pic::after { content: ''; position: absolute; inset-inline-start: 0; top: 330px; width: 100%; height: 254px; background: linear-gradient(rgb(100 0 98 / 0), rgb(100 0 98 / .85) 55%, #640062); }
  .s2 .toc { top: 552px; inset-inline-start: 138px; margin: 0; font-size: 54px; font-weight: 700; line-height: 60px; color: #fff; }
  .s2 .it { inset-inline-start: 787px; width: 421px; height: 58px; border: 1.5px solid var(--ink); border-radius: 29px; display: flex; align-items: center; }
  .s2 .it .n { width: 54px; height: 54px; border-radius: 50%; background: var(--ink); color: #fff; font-weight: 700; font-size: 20px; display: grid; place-items: center; margin-inline-start: 1px; flex: none; }
  .s2 .it p { margin: 0 0 0 24px; margin-inline: 24px 0; font-size: 22px; color: var(--ink); }
  /* 3 المقدمة */
  .s3 .pic { top: 58px; inset-inline-start: 73px; width: 539px; height: 470px; border-radius: 22px; }
  .s3 .cap { top: 570px; inset-inline-start: 73px; width: 520px; color: var(--ink-3); }
  .s3 .ttl { inset-inline-start: 667px; }
  .s3 .sub { top: 278px; inset-inline-start: 667px; font-size: 24px; }
  .s3 .p1 { top: 318px; inset-inline-start: 667px; width: 500px; }
  .s3 .btn { top: 423px; inset-inline-start: 673px; }
  .s3 .btn .pill { width: 254px; height: 50px; font-size: 22px; }
  .s3 .btn .arr { width: 50px; height: 50px; }
  .s3 .th { top: 527px; width: 254px; height: 135px; border-radius: 6px; }
  .s3 .th1 { inset-inline-start: 666px; } .s3 .th2 { inset-inline-start: 953px; }
  /* 4 لماذا الذكاء الاصطناعي */
  .s4 .btn { top: 350px; inset-inline-start: 84px; }
  .s4 .btn .pill { width: 214px; height: 44px; font-size: 20px; }
  .s4 .btn .arr { width: 42px; height: 42px; }
  .s4 .sub { top: 430px; inset-inline-start: 84px; color: var(--ink-2); }
  .s4 .p1 { top: 470px; inset-inline-start: 84px; width: 360px; color: var(--ink-3); }
  .s4 .bar { top: 607px; inset-inline-start: 84px; width: 398px; height: 32px; background: #F7F7F7; border-radius: 4px; overflow: hidden; }
  .s4 .bar .fill { position: absolute; inset-inline-start: 0; top: 0; height: 100%; width: 75%; background: var(--accent); border-radius: 4px; color: #fff; font-weight: 700; font-size: 17px; display: flex; align-items: center; padding-inline-start: 19px; margin: 0; }
  .s4 .bar .pct { position: absolute; inset-inline-end: 0; top: 0; height: 100%; display: flex; align-items: center; font-weight: 700; font-size: 17px; color: var(--ink); margin: 0; }
  .s4 .row { inset-inline-start: 578px; width: 629px; height: 118px; }
  .s4 .row .n { position: absolute; top: 0; height: 100%; inset-inline-start: 26px; display: flex; align-items: center; margin: 0; font-size: 44px; font-weight: 400; color: var(--accent); }
  .s4 .row .sub { position: absolute; top: 22px; inset-inline-start: 122px; }
  .s4 .row .p { position: absolute; top: 55px; inset-inline-start: 122px; width: 470px; font-size: 17px; line-height: 23px; }
  /* 5 التحدي */
  .s5 .btn { top: 143px; inset-inline-start: 643px; }
  .s5 .btn .pill { width: 162px; height: 44px; font-size: 20px; }
  .s5 .btn .arr { width: 42px; height: 42px; }
  .s5 .p1 { top: 210px; inset-inline-start: 643px; width: 530px; font-size: 20px; line-height: 26px; }
  .s5 .col { top: 347px; width: 354px; height: 314px; }
  .s5 .col .arr { position: absolute; top: 18px; inset-inline-start: 24px; width: 44px; height: 44px; background: var(--pink-2); border-radius: 50%; color: #fff; display: grid; place-items: center; }
  .s5 .col .arr svg { width: 24px; height: 24px; }
  .s5 .col .n { position: absolute; top: 80px; inset-inline-start: 24px; margin: 0; font-size: 44px; font-weight: 700; color: var(--accent); }
  .s5 .col .sub { position: absolute; top: 162px; inset-inline-start: 24px; }
  .s5 .col .p { position: absolute; top: 208px; inset-inline-start: 24px; width: 290px; font-size: 17px; line-height: 23px; }
  /* 6 النموذج المعزّز */
  .s6 .p1 { top: 254px; inset-inline-start: 84px; width: 530px; }
  .s6 .pic { top: 70px; inset-inline-start: 661px; width: 536px; height: 266px; border-radius: 14px; }
  .s6 .line { top: 498px; inset-inline-start: 93px; width: 1094px; height: 1px; background: #C895CD; }
  .s6 .st { height: 60px; display: flex; align-items: center; gap: 24px; }
  .s6 .st .n { margin: 0; font-size: 46px; font-weight: 700; color: var(--accent); line-height: 1; }
  .s6 .st p { margin: 0; font-size: 22px; color: var(--ink); white-space: nowrap; }
  .s6 .ar { width: 48px; height: 16px; color: var(--pink); }
  .s6 .ar svg { width: 100%; height: 100%; }
  html[dir="rtl"] .s6 .ar svg { transform: scaleX(-1); }
  .s6 .r1 { top: 402px; } .s6 .r2 { top: 538px; }
  .s6 .c1 { inset-inline-start: 86px; } .s6 .c2 { inset-inline-start: 466px; } .s6 .c3 { inset-inline-start: 890px; }
  .s6 .a1 { inset-inline-start: 400px; } .s6 .a2 { inset-inline-start: 805px; }
  /* 7 قبل وبعد */
  .s7 .pic { top: 198px; width: 530px; height: 268px; border-radius: 16px; overflow: hidden; background: #423A51; }
  .s7 .pic img { position: absolute; top: 0; inset-inline-start: 0; width: 530px; height: 268px; }
  .s7 .pc1 { inset-inline-start: 84px; } .s7 .pc2 { inset-inline-start: 666px; }
  .s7 .box { top: 420px; width: 500px; height: 265px; background: var(--accent); border-radius: 16px; color: #fff; }
  .s7 .bx1 { inset-inline-start: 99px; } .s7 .bx2 { inset-inline-start: 682px; }
  .s7 .box .sub { position: absolute; top: 42px; inset-inline-start: 33px; color: #fff; font-size: 26px; }
  .s7 .box .ic { position: absolute; top: 17px; inset-inline-end: 20px; width: 72px; height: 72px; border-radius: 50%; background: #EDE6F5; display: grid; place-items: center; }
  .s7 .box .ic svg { width: 44px; height: 44px; }
  .s7 .box .p { position: absolute; top: 127px; inset-inline-start: 33px; width: 420px; color: #fff; font-size: 18px; line-height: 25.5px; }
  /* 8 الأدوات */
  .s8 .p1 { top: 292px; inset-inline-start: 84px; width: 360px; color: var(--ink-3); }
  .s8 .btn { top: 448px; inset-inline-start: 84px; }
  .s8 .btn .pill { width: 210px; height: 44px; font-size: 20px; }
  .s8 .btn .arr { width: 42px; height: 42px; }
  .s8 .sub { top: 533px; inset-inline-start: 93px; color: var(--ink-2); }
  .s8 .p2 { top: 580px; inset-inline-start: 93px; width: 320px; font-size: 17px; line-height: 22.5px; color: var(--ink-3); }
  .s8 .row { inset-inline-start: 578px; width: 629px; height: 180px; }
  .s8 .row img { position: absolute; top: 17px; inset-inline-start: 22px; width: 213px; height: 146px; border-radius: 8px; }
  .s8 .row .sub { position: absolute; top: 32px; inset-inline-start: 271px; font-size: 24px; color: var(--indigo); }
  .s8 .row .p { position: absolute; top: 72px; inset-inline-start: 271px; width: 320px; }
  /* 9 الأثر على القوى العاملة */
  .s9 .pic { top: 58px; inset-inline-start: 73px; width: 507px; height: 604px; border-radius: 22px; }
  .s9 .ttl { inset-inline-start: 640px; }
  .s9 .p1 { top: 243px; inset-inline-start: 640px; width: 530px; font-size: 19px; line-height: 25px; }
  .s9 .btn { top: 336px; inset-inline-start: 641px; }
  .s9 .btn .pill { width: 213px; height: 50px; font-size: 22px; }
  .s9 .btn .arr { width: 50px; height: 50px; }
  .s9 .panel { top: 457px; inset-inline-start: 283px; width: 934px; height: 216px; background: var(--bg); border-radius: 16px; box-shadow: 0 6px 24px rgb(80 0 140 / .10); }
  .s9 .col { top: 490px; width: 260px; }
  .s9 .col .ic { width: 48px; height: 48px; }
  .s9 .col .sub { margin-top: 15px; }
  .s9 .col .p { margin-top: 20px; width: 240px; font-size: 17px; line-height: 23px; }
  .s9 .k1 { inset-inline-start: 327px; } .s9 .k2 { inset-inline-start: 630px; } .s9 .k3 { inset-inline-start: 934px; }
  /* 10 خطة التواصل */
  .s10 .pic { top: 240px; width: 353px; height: 215px; border-radius: 14px; overflow: hidden; background: #BDB3C8; }
  .s10 .pic img { position: absolute; top: 0; inset-inline-start: 0; width: 353px; height: 176px; }
  .s10 .box { top: 415px; width: 323px; height: 260px; }
  .s10 .box .sub { position: absolute; top: 37px; inset-inline-start: 25px; }
  .s10 .box .p { position: absolute; top: 73px; inset-inline-start: 25px; width: 305px; font-size: 15px; line-height: 22px; }
  .s10 .box .bar { position: absolute; inset-inline-start: 25px; width: 305px; height: 46px; background: var(--track); border-radius: 8px; overflow: hidden; }
  .s10 .box .b1 { top: 136px; } .s10 .box .b2 { top: 194px; }
  .s10 .bar .fill { position: absolute; inset-inline-start: 0; top: 0; height: 100%; border-radius: 8px; color: #fff; font-weight: 700; font-size: 17px; display: flex; align-items: center; padding-inline-start: 15px; margin: 0; background: var(--accent); }
  .s10 .bar .pk { background: var(--pink); }
  .s10 .bar .pct { position: absolute; inset-inline-end: 15px; top: 0; height: 100%; display: flex; align-items: center; font-weight: 700; font-size: 17px; color: var(--ink); margin: 0; }
  .s10 .g1 { inset-inline-start: 84px; } .s10 .g2 { inset-inline-start: 476px; } .s10 .g3 { inset-inline-start: 865px; }
  .s10 .x1 { inset-inline-start: 99px; } .s10 .x2 { inset-inline-start: 491px; } .s10 .x3 { inset-inline-start: 880px; }
  /* 11 دراسة حالة */
  .s11 .p1 { top: 203px; inset-inline-start: 84px; width: 430px; }
  .s11 .pic { top: 338px; inset-inline-start: 84px; width: 448px; height: 314px; border-radius: 16px; }
  .s11 .vert { top: 115px; inset-inline-start: 613px; width: 120px; height: 488px; border-radius: 20px; background: var(--lav); border: 2px solid #222; }
  .s11 .vert p { position: absolute; margin: 0; top: 50%; inset-inline-start: 50%; transform: translate(-50%, -50%) rotate(-90deg); white-space: nowrap; font-size: 22px; font-weight: 700; color: var(--ink); }
  html[dir="rtl"] .s11 .vert p { transform: translate(50%, -50%) rotate(90deg); }
  .s11 .dash { inset-inline-start: 733px; width: 117px; height: 0; border-top: 2px dashed #B07FE0; }
  .s11 .tag { inset-inline-start: 850px; width: 310px; height: 47px; border-radius: 24px; background: linear-gradient(90deg, #FF47FF, #FF9CFF); display: flex; align-items: center; margin: 0; padding-inline-start: 33px; font-size: 24px; font-weight: 700; color: var(--ink); }
  html[dir="rtl"] .s11 .tag { background: linear-gradient(270deg, #FF47FF, #FF9CFF); }
  .s11 .tp { inset-inline-start: 883px; width: 270px; }
  /* 12 مؤشرات الأداء */
  .s12 .p1 { top: 232px; inset-inline-start: 84px; width: 530px; }
  .s12 .btn { top: 97px; inset-inline-start: 745px; }
  .s12 .btn .pill { width: 245px; height: 42px; font-size: 20px; }
  .s12 .btn .arr { width: 42px; height: 42px; }
  .s12 .p2 { top: 172px; inset-inline-start: 745px; width: 380px; }
  .s12 .card { top: 325px; width: 548px; height: 352px; border-radius: 20px; }
  .s12 .cd1 { inset-inline-start: 73px; } .s12 .cd2 { inset-inline-start: 658px; }
  .s12 .kpi { position: absolute; top: 28px; inset-inline-start: 34px; margin: 0; font-size: 66px; font-weight: 700; color: var(--accent); line-height: 1.2; }
  .s12 .cd1 .p { position: absolute; top: 47px; inset-inline-start: 255px; width: 230px; font-size: 17px; line-height: 23px; }
  .s12 .cd2 .p { position: absolute; top: 47px; inset-inline-start: 44px; width: 470px; font-size: 17px; line-height: 23px; }
  .s12 .chart { position: absolute; top: 135px; inset-inline-start: 20px; width: 520px; height: 200px; }
  .s12 .chart svg { width: 100%; height: 100%; overflow: visible; }
  .s12 .chart text { font-family: var(--font); font-size: 16px; fill: var(--ink); }
  .s12 .tip { position: absolute; top: 128px; inset-inline-start: 172px; width: 92px; height: 36px; background: #fff; border-radius: 6px; display: grid; place-items: center; margin: 0; font-size: 20px; font-weight: 700; color: var(--accent); box-shadow: 0 2px 6px rgb(0 0 0 / .08); }
  .s12 .tip::after { content: ''; position: absolute; top: 100%; inset-inline-start: 50%; margin-inline-start: -7px; border: 7px solid transparent; border-top-color: #fff; }
  /* 13 المخاطر */
  .s13 .circ { top: 265px; inset-inline-start: 72px; width: 416px; height: 416px; }
  .s13 .circ svg { width: 100%; height: 100%; }
  .s13 .val { margin: 0; inset-inline-start: 72px; width: 416px; text-align: center; font-size: 44px; font-weight: 700; color: #fff; }
  .s13 .v1 { top: 308px; } .s13 .v2 { top: 422px; } .s13 .v3 { top: 562px; }
  .s13 .ln { height: 1px; background: #BDBDBD; }
  .s13 .l1 { top: 298px; inset-inline-start: 360px; width: 185px; } .s13 .l2 { top: 440px; inset-inline-start: 365px; width: 180px; } .s13 .l3 { top: 583px; inset-inline-start: 378px; width: 167px; }
  .s13 .lab { inset-inline-start: 563px; margin: 0; font-size: 22px; font-weight: 700; line-height: 1; }
  .s13 .lab1 { top: 288px; color: var(--purple-4); } .s13 .lab2 { top: 431px; color: var(--purple-3); } .s13 .lab3 { top: 573px; color: var(--accent); }
  .s13 .lp { inset-inline-start: 563px; width: 300px; font-size: 17px; line-height: 23px; }
  .s13 .lp1 { top: 330px; } .s13 .lp2 { top: 472px; } .s13 .lp3 { top: 615px; }
  .s13 .pic { top: 55px; inset-inline-start: 842px; width: 438px; height: 243px; border-start-start-radius: 16px; border-end-start-radius: 16px; }
  .s13 .box { top: 325px; inset-inline-start: 898px; width: 382px; height: 340px; border-start-start-radius: 14px; border-end-start-radius: 14px; border-start-end-radius: 0; border-end-end-radius: 0; }
  .s13 .box .sub { position: absolute; top: 40px; inset-inline-start: 32px; font-size: 26px; }
  .s13 .box .p { position: absolute; top: 95px; inset-inline-start: 32px; width: 320px; font-size: 19px; line-height: 25.5px; color: var(--ink); }
  .s13 .box ul { position: absolute; top: 196px; inset-inline-start: 32px; margin: 0; padding: 0; list-style: none; }
  .s13 .box li { display: flex; align-items: center; gap: 14px; font-size: 17px; line-height: 30px; color: var(--ink); }
  .s13 .box li::before { content: '✓'; color: var(--accent); font-weight: 700; font-size: 16px; }
  /* 14 الجدول الزمني */
  .s14 .p1 { top: 190px; inset-inline-start: 84px; width: 980px; }
  .s14 .axis { top: 310px; inset-inline-start: 0; width: 1280px; height: 1px; background: #222; }
  .s14 .col { top: 331px; width: 263px; height: 331px; border-radius: 10px; border: 1px solid #E8B8F0; background: linear-gradient(#FFE1FF, #FFF5FF 70%, #FFFFFF); }
  .s14 .col .ic { position: absolute; top: -62px; inset-inline-start: 90px; width: 82px; height: 82px; border-radius: 50%; background: linear-gradient(160deg, #B04CFF, #7B02DB); color: #fff; display: grid; place-items: center; }
  .s14 .col .ic svg { width: 46px; height: 46px; }
  .s14 .col .yr { position: absolute; top: 48px; inset-inline-start: 0; width: 100%; text-align: center; margin: 0; font-size: 36px; font-weight: 700; color: var(--ink); }
  .s14 .col .dv { position: absolute; top: 121px; inset-inline-start: 36px; width: 190px; height: 1px; background: #111; }
  .s14 .col .sub { position: absolute; top: 153px; inset-inline-start: 0; width: 100%; text-align: center; }
  .s14 .col .p { position: absolute; top: 200px; inset-inline-start: 20px; width: 223px; text-align: center; font-size: 17px; line-height: 23px; }
  .s14 .t1 { inset-inline-start: 84px; } .s14 .t2 { inset-inline-start: 367px; } .s14 .t3 { inset-inline-start: 651px; } .s14 .t4 { inset-inline-start: 934px; }
  /* 15 الخطوات التالية */
  .s15 .p1 { top: 237px; inset-inline-start: 84px; width: 480px; }
  .s15 .pic { top: 70px; inset-inline-start: 641px; width: 558px; height: 275px; border-radius: 16px; }
  .s15 .panel { top: 378px; inset-inline-start: 84px; width: 1115px; height: 307px; border-radius: 16px; border: 1px solid #E0B0F0; background: linear-gradient(120deg, #F6EBFF, #FDE6FF 60%, #FFE0FF); }
  html[dir="rtl"] .s15 .panel { background: linear-gradient(240deg, #F6EBFF, #FDE6FF 60%, #FFE0FF); }
  .s15 .mid { top: 395px; inset-inline-start: 450px; width: 385px; height: 272px; border-radius: 10px; border: 2px solid #A020F0; background: linear-gradient(120deg, #EBD2F9, #F4E2FC 50%, #FBD9FD); }
  html[dir="rtl"] .s15 .mid { background: linear-gradient(240deg, #EBD2F9, #F4E2FC 50%, #FBD9FD); }
  .s15 .hd { display: flex; align-items: center; gap: 20px; }
  .s15 .hd .n { width: 36px; height: 36px; border-radius: 50%; border: 2px solid var(--pink); color: var(--ink); font-size: 17px; display: grid; place-items: center; margin: 0; flex: none; }
  .s15 .hd .sub { font-size: 24px; }
  .s15 .h1 { top: 444px; inset-inline-start: 121px; } .s15 .h2 { top: 438px; inset-inline-start: 482px; } .s15 .h3 { top: 438px; inset-inline-start: 873px; }
  .s15 .q { width: 290px; }
  .s15 .q1 { top: 517px; inset-inline-start: 121px; width: 280px; } .s15 .q2 { top: 522px; inset-inline-start: 484px; width: 300px; } .s15 .q3 { top: 522px; inset-inline-start: 875px; }"""

ARROW = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M6 18L18 6M9 6h9v9"/></svg>'
LONG_ARROW = '<svg viewBox="0 0 48 16" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M2 8h42M38 2l6 6-6 6"/></svg>'
LOGO = '<svg viewBox="0 0 58 58" fill="none" stroke="#fff" stroke-width="5" stroke-linejoin="round" aria-hidden="true"><path d="M14 9C26 5 40 6 49 10c4 10 3 24-1 36-12 5-26 5-36 1C7 36 7 20 14 9z"/></svg>'
ICONS = {
    'target': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12 2a10 10 0 1 0 10 10h-2.2A7.8 7.8 0 1 1 12 4.2V2z"/><path d="M12 6.4a5.6 5.6 0 1 0 5.6 5.6h-2.2A3.4 3.4 0 1 1 12 8.6V6.4z"/><path d="M16.5 3.5l.4 3.6-4.2 4.2a1.2 1.2 0 0 1-1.7-1.7l4.2-4.2 3.6.4-1.2-2.3-1.1.0z"/><path d="M19.6 2.1l.5 2.8 2.8.5-2.2 2.2-3.3-.4-.4-3.3z"/></svg>',
    'users': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><circle cx="12" cy="8" r="3.2"/><circle cx="5" cy="9.5" r="2.4"/><circle cx="19" cy="9.5" r="2.4"/><path d="M12 12.2c-3.3 0-6 2-6 4.6V19h12v-2.2c0-2.6-2.7-4.6-6-4.6z"/><path d="M5 12.8c-2.3 0-4.2 1.4-4.2 3.3V18h3.6v-1.2c0-1.4.6-2.7 1.7-3.6A4.6 4.6 0 0 0 5 12.8z"/><path d="M19 12.8a4.6 4.6 0 0 0-1.1.4c1.1.9 1.7 2.2 1.7 3.6V18h3.6v-1.9c0-1.9-1.9-3.3-4.2-3.3z"/></svg>',
    'bulb': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12 2.5a6.5 6.5 0 0 0-4 11.6c.6.5 1 1.2 1 2V17h6v-.9c0-.8.4-1.5 1-2A6.5 6.5 0 0 0 12 2.5zm0 2a4.5 4.5 0 0 1 2.8 8c-.5.4-.8.9-1 1.5h-3.6c-.2-.6-.5-1.1-1-1.5A4.5 4.5 0 0 1 12 4.5z"/><path d="M12 7.5a2.5 2.5 0 1 0 0 5 2.5 2.5 0 0 0 0-5zm0 1.5a1 1 0 1 1 0 2 1 1 0 0 1 0-2z"/><rect x="9" y="18" width="6" height="1.6" rx=".8"/><rect x="9.8" y="20.4" width="4.4" height="1.6" rx=".8"/><path d="M4 11h2v1.4H4zM18 11h2v1.4h-2zM11.3 0h1.4v2h-1.4zM5.6 4.6l1-1 1.4 1.4-1 1zM16 5l1.4-1.4 1 1-1.4 1.4z"/></svg>',
    'hands': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M1 7.5h4.2l3.3 3.3a2 2 0 0 0 2.8 0l1.7-1.7-1.2-1.2L10 9.7a.4.4 0 0 1-.6 0L6 6.2V5.5H1z"/><path d="M23 7.5h-4.2l-3.6-3.3a2.4 2.4 0 0 0-3 0L9.6 6.5l1 1.1 2.4-2.1a.9.9 0 0 1 1.1 0L18 9.1v.5l-3.4 3.4-1.2-1.2-1.3 1.3-1.2-1.2-1.3 1.3-1.2-1.2L7 13.4V14l1.9 1.9a1.3 1.3 0 0 0 1.8 0l.4-.4 1.2 1.2a1.3 1.3 0 0 0 1.8 0l.4-.4 1.2 1.2a1.3 1.3 0 0 0 1.8 0l.4-.4 4.2-4.2V5.5H23z"/></svg>',
}


def btn(label, cls=''):  # زر أسود بيضاوي + دائرة وردية بسهم
    return '<div class="btn %s"><p class="pill">%s</p><span class="arr">%s</span></div>' % (cls, label, ARROW)


def img(name, cls, alt):
    return '<img class="ph %s" alt="%s" src="/templates/freeform/%s/assets/%s.jpg">' % (cls, alt, KEY, name)


S1 = ('<div class="hero">' + img('cover', '', '§ALT1§') + '</div><div class="panel"></div>'
      '<h1 class="big">§S1_T1§<br>§S1_T2§</h1><p class="tag">§S1_SUB§</p>' + btn('§ADD§')
      + '<div class="logo">' + LOGO + '<p>§S1_LOGO§</p></div>')

S2 = ('<div class="pic">' + img('toc', '', '§ALT2§') + '</div><h1 class="toc">§S2_T§</h1>'
      + ''.join('<div class="it" style="top:%dpx"><span class="n">0%d</span><p>§S2_%d§</p></div>' % (66 + round(i * 88.5), i + 1, i + 1) for i in range(7)))

S3 = (img('brain', 'pic', '§ALT3§') + '<p class="p cap">§S3_CAP§</p>'
      '<h1 class="ttl"><span class="lt">§S3_T1§</span><br>§S3_T2§</h1><h2 class="sub">§EDIT§</h2><p class="p p1">§S3_P§</p>'
      + btn('§EDIT§') + img('thumb-room', 'th th1', '§ALT3B§') + img('thumb-brain', 'th th2', '§ALT3C§'))

S4 = ('<h1 class="ttl">§S4_T1§<br>§S4_T2§<br>§S4_T3§</h1>' + btn('§EDIT§') + '<h2 class="sub">§EDIT§</h2><p class="p p1">§S4_P§</p>'
      '<div class="bar"><p class="fill">§EDIT§</p><p class="pct">80%</p></div>'
      + ''.join('<div class="card row" style="top:%dpx"><p class="n">0%d</p><h2 class="sub">§EDIT§</h2><p class="p">§S4_R§</p></div>' % (250 + i * 141, i + 1) for i in range(3)))

S5 = ('<h1 class="ttl">§S5_T1§<br>§S5_T2§<br>§S5_T3§</h1>' + btn('§ADD§') + '<p class="p p1">§S5_P§</p>'
      + ''.join('<div class="card col" style="inset-inline-start:%dpx"><span class="arr">%s</span><p class="n">0%d</p><h2 class="sub">§EDIT§</h2><p class="p">§S5_C§</p></div>' % (84 + i * 379, ARROW, i + 1) for i in range(3)))


def step(n, cls, key):
    return '<div class="st %s"><p class="n">0%d</p><p>§%s§</p></div>' % (cls, n, key)


S6 = ('<h1 class="ttl">§S6_T1§<br>§S6_T2§</h1><p class="p p1">§S6_P§</p>' + img('vr', 'pic', '§ALT6§') + '<div class="line"></div>'
      + step(1, 'r1 c1', 'S6_1') + '<span class="ar r1 a1">%s</span>' % LONG_ARROW + step(2, 'r1 c2', 'S6_2') + '<span class="ar r1 a2">%s</span>' % LONG_ARROW + step(3, 'r1 c3', 'S6_3')
      + step(4, 'r2 c1', 'S6_4') + '<span class="ar r2 a1">%s</span>' % LONG_ARROW + step(5, 'r2 c2', 'S6_5') + '<span class="ar r2 a2">%s</span>' % LONG_ARROW + step(6, 'r2 c3', 'S6_6'))
STYLE += "\n  .s6 .ar { position: absolute; top: 424px; } .s6 .ar.r2 { top: 560px; }"

S7 = ('<h1 class="ttl">§S7_T§</h1>'
      '<div class="pic pc1">' + img('papers', '', '§ALT7A§') + '</div><div class="pic pc2">' + img('meeting', '', '§ALT7B§') + '</div>'
      '<div class="box bx1"><h2 class="sub">§EDIT§</h2><span class="ic">' + ICONS['target'] + '</span><p class="p">§S7_P§</p></div>'
      '<div class="box bx2"><h2 class="sub">§EDIT§</h2><span class="ic">' + ICONS['users'] + '</span><p class="p">§S7_P§</p></div>')

S8 = ('<h1 class="ttl">§S8_T1§<br>§S8_T2§<br>§S8_T3§</h1><p class="p p1">§S8_P§</p>' + btn('§ADD§') + '<h2 class="sub">§EDIT§</h2><p class="p p2">§S8_P§</p>'
      + ''.join('<div class="card row" style="top:%dpx">%s<h2 class="sub">§EDIT§</h2><p class="p">§S8_R§</p></div>' % (59 + i * 211, img('tool-%d' % (i + 1), '', '§ALT8_%d§' % (i + 1))) for i in range(3)))

S9 = (img('workforce', 'pic', '§ALT9§') + '<h1 class="ttl">§S9_T1§<br>§S9_T2§</h1><p class="p p1">§S9_P§</p>' + btn('§EDIT§') + '<div class="panel"></div>'
      + ''.join('<div class="col k%d"><span class="ic">%s</span><h2 class="sub">§EDIT§</h2><p class="p">§S9_C§</p></div>' % (i + 1, ICONS[ic]) for i, ic in enumerate(['bulb', 'target', 'users'])))

S10_BARS = [(50, 51, 80, 68), (70, 69, 40, 44), (60, 66, 90, 79)]  # (النسبة المكتوبة، العرض المرئي كما في الأصل) لكل شريط
S10 = ('<h1 class="ttl">§S10_T1§<br>§S10_T2§</h1>'
       + ''.join('<div class="pic g%d">%s</div><div class="card box x%d"><h2 class="sub">§EDIT§</h2><p class="p">§S10_P§</p>'
                 '<div class="bar b1"><p class="fill" style="width:%d%%">§ADD§</p><p class="pct">%d%%</p></div>'
                 '<div class="bar b2"><p class="fill pk" style="width:%d%%">§ADD§</p><p class="pct">%d%%</p></div></div>'
                 % (i + 1, img('engage-%d' % (i + 1), '', '§ALT10_%d§' % (i + 1)), i + 1, aw, a, bw, b) for i, (a, aw, b, bw) in enumerate(S10_BARS)))

S11 = ('<h1 class="ttl">§S11_T1§ <span class="lt">§S11_T2§</span></h1><p class="p p1">§S11_P§</p>' + img('case', 'pic', '§ALT11§')
       + '<div class="vert"><p>§S11_V§</p></div>'
       + ''.join('<div class="dash" style="top:%dpx"></div><p class="tag" style="top:%dpx">§EDIT§</p><p class="p tp" style="top:%dpx">§S11_I§</p>' % (189 + i * 170, 167 + i * 170, 232 + i * 170) for i in range(3)))

S12 = ('<h1 class="ttl">§S12_T1§<br>§S12_T2§</h1><p class="p p1">§S12_P§</p>' + btn('§EDIT§') + '<p class="p p2">§S12_P2§</p>'
       '<div class="card cd1"><p class="kpi">$299</p><p class="p">§S12_K§</p><div class="chart" id="chart-bars"></div></div>'
       '<div class="card cd2"><p class="p">§S12_L§</p><div class="chart" id="chart-lines"></div><p class="tip">$3.98</p></div>')

S13 = ('<h1 class="ttl">§S13_T1§<br>§S13_T2§</h1>'
       '<div class="circ"><svg viewBox="0 0 416 416" aria-hidden="true"><circle cx="208" cy="208" r="208" fill="#CA82FF"/><circle cx="208" cy="265" r="152" fill="#AF42FF"/><circle cx="208" cy="320" r="97" fill="#7901D4"/></svg></div>'
       '<p class="val v1">$290</p><p class="val v2">$190</p><p class="val v3">$90</p>'
       '<div class="ln l1"></div><div class="ln l2"></div><div class="ln l3"></div>'
       '<p class="lab lab1">§EDIT§</p><p class="p lp lp1">§S13_I§</p><p class="lab lab2">§EDIT§</p><p class="p lp lp2">§S13_I§</p><p class="lab lab3">§EDIT§</p><p class="p lp lp3">§S13_I§</p>'
       + img('risks', 'pic', '§ALT13§') +
       '<div class="card box"><h2 class="sub">§EDIT§</h2><p class="p">§S13_B§</p><ul><li>§S13_L1§</li><li>§S13_L2§</li><li>§S13_L3§</li><li>§S13_L1§</li></ul></div>')

S14 = ('<h1 class="ttl">§S14_T§</h1><p class="p p1">§S14_P§</p><div class="axis"></div>'
       + ''.join('<div class="col t%d"><span class="ic">%s</span><p class="yr">%d</p><div class="dv"></div><h2 class="sub">§EDIT§</h2><p class="p">§S14_C§</p></div>' % (i + 1, ICONS[ic], 2027 + i) for i, ic in enumerate(['bulb', 'target', 'users', 'hands'])))

S15 = ('<h1 class="ttl">§S15_T1§<br>§S15_T2§</h1><p class="p p1">§S15_P§</p>' + img('meeting', 'pic', '§ALT7B§')
       + '<div class="panel"></div><div class="mid"></div>'
       + ''.join('<div class="hd h%d"><p class="n">%d</p><h2 class="sub">§EDIT§</h2></div><p class="p q q%d">§S15_C§</p>' % (i + 1, i + 1, i + 1) for i in range(3)))

BG = ['', 'bgA', 'bgA', 'bgC', 'bgA', 'bgB', 'bgA', 'bgB', 'bgA', 'bgB', 'bgA', 'bgB', 'bgA', 'bgB', 'bgA']
SLIDES = ['<div class="bgfix"></div>' + s if False else s for s in [S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, S14, S15]]
# صنف الخلفية يُضاف عبر CSS بحسب رقم الشريحة
STYLE += '\n' + '\n'.join('  .frame.s%d::before { %s }' % (i + 1, {'bgA': 'background: radial-gradient(420px 320px at 2% 2%, #ECE4FF 0%, rgb(236 228 255 / 0) 70%), radial-gradient(360px 300px at 100% 100%, #FBE2FF 0%, rgb(251 226 255 / 0) 70%);',
                                                                     'bgB': 'background: radial-gradient(420px 320px at 100% 0%, #ECE4FF 0%, rgb(236 228 255 / 0) 70%), radial-gradient(360px 300px at 0% 100%, #FBE2FF 0%, rgb(251 226 255 / 0) 70%);',
                                                                     'bgC': 'background: radial-gradient(420px 320px at 100% 0%, #FBE2FF 0%, rgb(251 226 255 / 0) 70%), radial-gradient(360px 300px at 0% 100%, #ECE4FF 0%, rgb(236 228 255 / 0) 70%);'}[b]) for i, b in enumerate(BG) if b)

JSON_BLOCKS = {
    'chart-data': {'ar': {'categories': ['بيانات 1', 'بيانات 2', 'بيانات 3', 'بيانات 4', 'بيانات 5'], 'values': [2.3, 4.5, 4.0, 2.7, 4.0], 'colors': ['#FF70FE', '#7B00D5', '#7B00D5', '#FF70FE', '#7B00D5'], 'max': 6, 'step': 2},
                   'en': {'categories': ['Data 1', 'Data 2', 'Data 3', 'Data 4', 'Data 5'], 'values': [2.3, 4.5, 4.0, 2.7, 4.0], 'colors': ['#FF70FE', '#7B00D5', '#7B00D5', '#FF70FE', '#7B00D5'], 'max': 6, 'step': 2}},
    'chart-data-2': {'ar': {'categories': ['بيانات 1', 'بيانات 2', 'بيانات 3', 'بيانات 4', 'بيانات 5'], 'series': [{'color': '#5B1FA8', 'values': [5.2, 3.1, 3.5, 4.4, 5.4]}, {'color': '#6736FF', 'values': [2.4, 2.4, 3.3, 4.6, 6.0]}, {'color': '#FF70FE', 'values': [2.8, 5.2, 3.6, 2.2, 3.3]}], 'max': 6, 'step': 2},
                     'en': {'categories': ['Data 1', 'Data 2', 'Data 3', 'Data 4', 'Data 5'], 'series': [{'color': '#5B1FA8', 'values': [5.2, 3.1, 3.5, 4.4, 5.4]}, {'color': '#6736FF', 'values': [2.4, 2.4, 3.3, 4.6, 6.0]}, {'color': '#FF70FE', 'values': [2.8, 5.2, 3.6, 2.2, 3.3]}], 'max': 6, 'step': 2}},
}

EXTRA_SCRIPT = """  // ----- الرسمان البيانيان في شريحة مؤشرات الأداء (من كتلتي chart-data وchart-data-2) -----
  function readJson(id) { var el = document.getElementById(id); return el ? JSON.parse(el.textContent) : null; }
  var NS = 'http://www.w3.org/2000/svg';
  function el(n, a) { var e = document.createElementNS(NS, n); for (var k in a) e.setAttribute(k, a[k]); return e; }
  function grid(svg, W, H, max, step, left, bottom, top) {
    for (var v = 0; v <= max; v += step) {
      var y = top + (H - top - bottom) * (1 - v / max);
      svg.appendChild(el('line', { x1: left, x2: W - 10, y1: y, y2: y, stroke: '#D9D9D9', 'stroke-width': 1 }));
      var t = el('text', { x: RTL ? W - 2 : left - 14, y: y + 5, 'text-anchor': RTL ? 'start' : 'end' }); t.textContent = v; svg.appendChild(t);
    }
  }
  function xOf(i, n, left, W) { var inner = W - left - 10, c = left + inner * (i + 0.5) / n; return RTL ? (W - c + left - 10) : c; }
  function drawBars() {
    var d = readJson('chart-data'), host = document.getElementById('chart-bars'); if (!d || !host) return;
    var W = 520, H = 200, left = 40, bottom = 36, top = 12, svg = el('svg', { viewBox: '0 0 ' + W + ' ' + H });
    grid(svg, W, H, d.max, d.step, left, bottom, top);
    d.values.forEach(function (v, i) {
      var x = xOf(i, d.values.length, left, W), y0 = H - bottom, y = top + (H - top - bottom) * (1 - v / d.max);
      svg.appendChild(el('rect', { x: x - 20, y: y, width: 40, height: y0 - y, fill: d.colors[i % d.colors.length] }));
      var t = el('text', { x: x, y: H - 8, 'text-anchor': 'middle' }); t.textContent = d.categories[i]; svg.appendChild(t);
    });
    host.appendChild(svg);
  }
  function drawLines() {
    var d = readJson('chart-data-2'), host = document.getElementById('chart-lines'); if (!d || !host) return;
    var W = 520, H = 200, left = 40, bottom = 36, top = 12, svg = el('svg', { viewBox: '0 0 ' + W + ' ' + H });
    grid(svg, W, H, d.max, d.step, left, bottom, top);
    var n = d.categories.length;
    d.categories.forEach(function (c, i) { var t = el('text', { x: xOf(i, n, left, W), y: H - 8, 'text-anchor': 'middle' }); t.textContent = c; svg.appendChild(t); });
    d.series.forEach(function (s) {
      var pts = s.values.map(function (v, i) { return xOf(i, n, left, W) + ',' + (top + (H - top - bottom) * (1 - v / d.max)); }).join(' ');
      svg.appendChild(el('polyline', { points: pts, fill: 'none', stroke: s.color, 'stroke-width': 3.5, 'stroke-linejoin': 'round', 'stroke-linecap': 'round' }));
    });
    host.appendChild(svg);
  }
  drawBars(); drawLines();"""

SAMPLE_AR = 'هذا نص توضيحي. أضف النص المطلوب هنا.'
LONG_AR = 'هذا نص توضيحي. أضف نصك هنا. يمكنك إضافة نص توضيحي. هذا نص توضيحي. أضف نصك هنا.'
LONG_EN = 'This is a sample text. Insert your text here. You can add sample text. This is a sample text. Insert your text here.'

STR = {
    'ar': {
        'ADD': 'أضف نصًا', 'EDIT': 'حرّر النص هنا',
        'S1_T1': 'إدارة التغيير', 'S1_T2': 'بالذكاء الاصطناعي', 'S1_SUB': 'قالب عرض تقديمي', 'S1_LOGO': 'شعار شركتك هنا',
        'ALT1': 'يد آلية ويد بشرية تتقاربان', 'ALT2': 'فريق يتابع لوحات بيانات على شاشة كبيرة', 'ALT3': 'رجل أعمال أمام دماغ رقمي متوهج',
        'ALT3B': 'اجتماع أمام شاشة لوحات بيانات', 'ALT3C': 'شخص أمام دماغ رقمي', 'ALT6': 'شخصان أحدهما يرتدي نظارة واقع افتراضي',
        'ALT7A': 'موظفون محاطون بأوراق متطايرة', 'ALT7B': 'اجتماع فريق مع دماغ رقمي متوهج', 'ALT8_1': 'فريق يناقش حول دماغ رقمي',
        'ALT8_2': 'شخصان بنظارات واقع افتراضي حول كرة ضوئية', 'ALT8_3': 'مصافحة بين رجلي أعمال', 'ALT9': 'فريق يعمل أمام واجهة دماغ رقمية',
        'ALT10_1': 'زملاء يتحدثون بابتسام', 'ALT10_2': 'فريق يتبادل الرسائل', 'ALT10_3': 'شبكة تواصل بين الموظفين',
        'ALT11': 'فريق يحلل بيانات على شاشة شفافة', 'ALT13': 'اجتماع أمام شاشات تحليلية',
        'S2_T': 'جدول المحتويات', 'S2_1': 'هذا نص توضيحي', 'S2_2': 'أضف النص المطلوب هنا', 'S2_3': 'يمكنك إضافة نص توضيحي', 'S2_4': 'أضف نصك التوضيحي هنا.',
        'S2_5': 'هذا نص توضيحي', 'S2_6': 'أضف النص المطلوب هنا', 'S2_7': 'يمكنك إضافة نص توضيحي',
        'S3_T1': 'مقدمة في', 'S3_T2': 'إدارة التغيير', 'S3_CAP': LONG_AR, 'S3_P': 'هذا نص توضيحي. أضف النص المطلوب هنا. يمكنك إضافة نص توضيحي. هذا نص توضيحي.',
        'S4_T1': 'لماذا يهم الذكاء', 'S4_T2': 'الاصطناعي في', 'S4_T3': 'إدارة التغيير', 'S4_P': LONG_AR, 'S4_R': LONG_AR,
        'S5_T1': 'تحدي', 'S5_T2': 'التحول', 'S5_T3': 'المؤسسي', 'S5_P': LONG_AR, 'S5_C': 'هذا نص توضيحي. أضف نصك هنا. يمكنك إضافة نص توضيحي. هذا نص توضيحي.',
        'S6_T1': 'نموذج إدارة التغيير', 'S6_T2': 'المعزّز بالذكاء الاصطناعي', 'S6_P': 'هذا نص توضيحي. أضف النص المطلوب هنا. يمكنك إضافة نص. هذا نص توضيحي. أضف نصك هنا.',
        'S6_1': 'حرّر النص هنا', 'S6_2': 'هذا نص توضيحي', 'S6_3': 'يمكنك إضافة نص.', 'S6_4': 'هذا نص توضيحي', 'S6_5': 'يمكنك إضافة نص.', 'S6_6': 'هذا نص توضيحي',
        'S7_T': 'قبل تبنّي الذكاء الاصطناعي وبعده', 'S7_P': 'هذا نص توضيحي. أضف نصك هنا. يمكنك إضافة نص توضيحي. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا.',
        'S8_T1': 'أدوات وتقنيات', 'S8_T2': 'الذكاء الاصطناعي', 'S8_T3': 'المستخدمة حاليًا', 'S8_P': LONG_AR, 'S8_R': 'هذا نص توضيحي. أضف نصك هنا. يمكنك إضافة نص توضيحي. هذا نص توضيحي.',
        'S9_T1': 'الأثر', 'S9_T2': 'على القوى العاملة', 'S9_P': 'هذا نص توضيحي. أضف النص المطلوب هنا. يمكنك إضافة نص. هذا نص توضيحي. أضف نصك هنا.', 'S9_C': SAMPLE_AR,
        'S10_T1': 'إشراك الموظفين', 'S10_T2': 'وخطة التواصل', 'S10_P': 'هذا نص توضيحي. أضف النص المطلوب هنا. يمكنك إضافة نص توضيحي.',
        'S11_T1': 'دراسة حالة', 'S11_T2': 'شريحة', 'S11_P': 'هذا نص توضيحي. أضف النص المطلوب هنا. يمكنك إضافة نص توضيحي. هذا نص توضيحي. أضف النص المطلوب هنا.',
        'S11_V': 'حرّر النص هنا. هذا نص توضيحي.', 'S11_I': SAMPLE_AR,
        'S12_T1': 'مؤشرات الأداء', 'S12_T2': 'ومقاييس النجاح', 'S12_P': 'هذا نص توضيحي. أضف النص المطلوب هنا. يمكنك إضافة نص. هذا نص توضيحي. أضف نصك هنا.',
        'S12_P2': 'هذا نص توضيحي. أضف النص المطلوب هنا. يمكنك إضافة نص. هذا نص توضيحي. أضف نصك هنا.', 'S12_K': SAMPLE_AR,
        'S12_L': 'هذا نص توضيحي. أضف النص المطلوب هنا. يمكنك إضافة نص. هذا نص توضيحي. أضف نصك هنا.',
        'S13_T1': 'المخاطر', 'S13_T2': 'والاعتبارات الأخلاقية', 'S13_I': 'هذا نص توضيحي. أضف النص المطلوب هنا. يمكنك إضافة نص.',
        'S13_B': 'هذا نص توضيحي. أضف النص المطلوب هنا. يمكنك إضافة نص توضيحي. هذا نص توضيحي.', 'S13_L1': 'هذا نص توضيحي.', 'S13_L2': 'أضف النص المطلوب هنا.', 'S13_L3': 'يمكنك إضافة نص توضيحي.',
        'S14_T': 'الجدول الزمني وخارطة الطريق', 'S14_P': 'هذا نص توضيحي. أضف النص المطلوب هنا. يمكنك إضافة نص توضيحي. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. يمكنك إضافة نص توضيحي.',
        'S14_C': 'هذا نص توضيحي. أضف النص المطلوب هنا. يمكنك إضافة نص توضيحي. هذا نص توضيحي.',
        'S15_T1': 'الخطوات التالية', 'S15_T2': 'ودعوة إلى العمل', 'S15_P': 'هذا نص توضيحي. أضف النص المطلوب هنا. يمكنك إضافة نص توضيحي. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي.',
        'S15_C': 'هذا نص توضيحي. أضف النص المطلوب هنا. يمكنك إضافة نص. هذا نص توضيحي. أضف نصك هنا.',
    },
    'en': {
        'ADD': 'Add Text', 'EDIT': 'Edit Text Here',
        'S1_T1': 'AI-Driven Change', 'S1_T2': 'Management', 'S1_SUB': 'Presentation Template', 'S1_LOGO': 'Your company logo here',
        'ALT1': 'Robotic hand and human hand reaching each other', 'ALT2': 'Team watching dashboards on a large screen', 'ALT3': 'Businessman facing a glowing digital brain',
        'ALT3B': 'Meeting in front of a dashboard wall', 'ALT3C': 'Person facing a digital brain', 'ALT6': 'Two people, one wearing a VR headset',
        'ALT7A': 'Employees surrounded by flying papers', 'ALT7B': 'Team meeting with a glowing digital brain', 'ALT8_1': 'Team discussing around a digital brain',
        'ALT8_2': 'Two people in VR headsets around a glowing sphere', 'ALT8_3': 'Handshake between two businessmen', 'ALT9': 'Team working in front of a digital brain interface',
        'ALT10_1': 'Colleagues talking and smiling', 'ALT10_2': 'Team exchanging messages', 'ALT10_3': 'Network of communication between employees',
        'ALT11': 'Team analyzing data on a transparent screen', 'ALT13': 'Meeting in front of analytics screens',
        'S2_T': 'Table of Contents', 'S2_1': 'This is a sample text', 'S2_2': 'Insert your desired text here', 'S2_3': 'You can add sample text', 'S2_4': 'Insert your sample text here.',
        'S2_5': 'This is a sample text', 'S2_6': 'Insert your desired text here', 'S2_7': 'You can add sample text',
        'S3_T1': 'Intro to', 'S3_T2': 'Change Management', 'S3_CAP': LONG_EN, 'S3_P': 'This is a sample text. Insert your desired text here. You can add sample text. This is a sample text.',
        'S4_T1': 'Why AI', 'S4_T2': 'Matters In Change', 'S4_T3': 'Management', 'S4_P': LONG_EN, 'S4_R': LONG_EN,
        'S5_T1': 'The Challenge', 'S5_T2': 'of Organizational', 'S5_T3': 'Transformation', 'S5_P': LONG_EN, 'S5_C': 'This is a sample text. Insert your text here. You can add sample text. This is a sample text.',
        'S6_T1': 'AI-Enhanced Change', 'S6_T2': 'Management Model', 'S6_P': 'This is a sample text. Insert your desired text here. You can add text. This is a sample text. Insert your text here.',
        'S6_1': 'Edit text here', 'S6_2': 'This is a sample text', 'S6_3': 'You can add text.', 'S6_4': 'This is a sample text', 'S6_5': 'You can add text.', 'S6_6': 'This is a sample text',
        'S7_T': 'Before &amp; After AI Adoption', 'S7_P': 'This is a sample text. Insert your text here. You can add sample text. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here.',
        'S8_T1': 'AI Tools &amp;', 'S8_T2': 'Technologies', 'S8_T3': 'Currently in Use', 'S8_P': LONG_EN, 'S8_R': 'This is a sample text. Insert your text here. You can add sample text. This is a sample text.',
        'S9_T1': 'Impact', 'S9_T2': 'on Workforce', 'S9_P': 'This is a sample text. Insert your desired text here. You can add text. This is a sample text. Insert your text here.', 'S9_C': 'This is a sample text. Insert your desired text here.',
        'S10_T1': 'Employee Engagement &amp;', 'S10_T2': 'Communication Plan', 'S10_P': 'This is a sample text. Insert your desired text here. You can add sample text.',
        'S11_T1': 'Case Study', 'S11_T2': 'Slide', 'S11_P': 'This is a sample text. Insert your desired text here. You can add sample text. This is a sample text. Insert your desired text here.',
        'S11_V': 'Edit text here. This is a sample text.', 'S11_I': 'This is a sample text. Insert your desired text here.',
        'S12_T1': 'KPIs &amp;', 'S12_T2': 'Success Metrics', 'S12_P': 'This is a sample text. Insert your desired text here. You can add text. This is a sample text. Insert your text here.',
        'S12_P2': 'This is a sample text. Insert your desired text here. You can add text. This is a sample text. Insert your text here.', 'S12_K': 'This is a sample text. Insert your desired text here.',
        'S12_L': 'This is a sample text. Insert your desired text here. You can add text. This is a sample text. Insert your text here.',
        'S13_T1': 'Risks &amp;', 'S13_T2': 'Ethical Concerns', 'S13_I': 'This is a sample text. Insert your desired text here. You can add text.',
        'S13_B': 'This is a sample text. Insert your desired text here. You can add sample text. This is a sample text.', 'S13_L1': 'This is a sample text.', 'S13_L2': 'Insert your desired text here.', 'S13_L3': 'You can add sample text.',
        'S14_T': 'Timeline &amp; Roadmap', 'S14_P': 'This is a sample text. Insert your desired text here. You can add sample text. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. You can add sample text.',
        'S14_C': 'This is a sample text. Insert your desired text here. You can add sample text. This is a sample text.',
        'S15_T1': 'Next Steps', 'S15_T2': '&amp; Call To Action', 'S15_P': 'This is a sample text. Insert your desired text here. You can add sample text. This is a sample text. Insert your desired text here. This is a sample text.',
        'S15_C': 'This is a sample text. Insert your desired text here. You can add text. This is a sample text. Insert your text here.',
    },
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'mgmt', 'icon': '🤖',
    'name': 'إدارة التغيير بالذكاء الاصطناعي', 'nameEn': 'AI-Driven Change Management',
    'description': '15 شريحة لإدارة التغيير بالذكاء الاصطناعي: غلاف، فهرس، نموذج، أدوات، أثر، مؤشرات، مخاطر، جدول زمني، خطوات تالية',
    'descriptionEn': '15 slides on AI-driven change management: cover, contents, model, tools, impact, KPIs, risks, roadmap, next steps',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'جدول المحتويات', 'titleEn': 'Table of Contents'},
        {'title': 'مقدمة في إدارة التغيير', 'titleEn': 'Intro to Change Management'},
        {'title': 'لماذا يهم الذكاء الاصطناعي', 'titleEn': 'Why AI Matters', 'needsData': True},
        {'title': 'تحدي التحول المؤسسي', 'titleEn': 'The Challenge of Transformation'},
        {'title': 'نموذج إدارة التغيير المعزّز', 'titleEn': 'AI-Enhanced Change Model'},
        {'title': 'قبل التبنّي وبعده', 'titleEn': 'Before & After AI Adoption'},
        {'title': 'الأدوات والتقنيات المستخدمة', 'titleEn': 'AI Tools in Use'},
        {'title': 'الأثر على القوى العاملة', 'titleEn': 'Impact on Workforce'},
        {'title': 'إشراك الموظفين وخطة التواصل', 'titleEn': 'Engagement & Communication Plan', 'needsData': True},
        {'title': 'دراسة حالة', 'titleEn': 'Case Study'},
        {'title': 'مؤشرات الأداء (رسمان)', 'titleEn': 'KPIs & Success Metrics (charts)', 'needsData': True},
        {'title': 'المخاطر والاعتبارات الأخلاقية', 'titleEn': 'Risks & Ethical Concerns', 'needsData': True},
        {'title': 'الجدول الزمني وخارطة الطريق', 'titleEn': 'Timeline & Roadmap', 'needsData': True},
        {'title': 'الخطوات التالية ودعوة إلى العمل', 'titleEn': 'Next Steps & Call To Action'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص والأرقام فقط؛ أرقام الرسمين في كتلتي JSON بالمعرّفين chart-data وchart-data-2، والصور في assets تبقى بمساراتها.',
}
