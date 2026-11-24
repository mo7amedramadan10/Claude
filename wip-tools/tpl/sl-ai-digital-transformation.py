# -*- coding: utf-8 -*-
# نموذج «التحول الرقمي بالذكاء الاصطناعي»: 15 شريحة (غلاف داكن، جدول أعمال، مقدمة، دور الذكاء الاصطناعي، تحديات،
# حلول، إطار واستراتيجية، تقنيات، عملية تنفيذ، بيانات وتحليلات برسم أعمدة، دراسة حالة، فوائد، مخاطر، نظرة مستقبلية، خاتمة).
KEY = 'sl-ai-digital-transformation'
TITLE = {'ar': 'التحول الرقمي بالذكاء الاصطناعي', 'en': 'AI Digital Transformation'}
COMMENT = 'نموذج «التحول الرقمي بالذكاء الاصطناعي»: 15 شريحة بخلفية بيضاء وبطاقات داكنة بتدرّج كحلي وبنفسجي، صور فوتوغرافية مقصوصة من الأصل، أيقونات SVG مضمّنة، قوس رباعي وحلقات مرقّمة SVG، ورسم أعمدة مزدوج يُرسم من chart-data.'
VARS = '--ink: #111318; --ink-2: #2B2F36; --gray: #5B5F66; --navy: #0E1F57; --navy-d: #070C22; --blue: #0B2F8A; --accent: #A35CF0; --purple-l: #B266F0; --purple-d: #5C0F8B; --lav: #ECDDFB; --lav-2: #F6EEFC; --line: #C9A6F5; --bg: #FFFFFF;'

STYLE = """  /* ---- مشترك ---- */
  .ttl { position: absolute; top: 100px; inset-inline-start: 73px; margin: 0; font-size: 42px; font-weight: 800; line-height: 1.27; color: var(--ink); letter-spacing: -0.3px; }
  .ttl .lt { font-weight: 400; }
  html[lang="ar"] .foot, html[lang="ar"] .s1 .sub, html[lang="ar"] .s1 .logo-t { letter-spacing: 0; }
  html[lang="ar"] .foot { font-size: 20px; }
  .s3 .ttl, .s4 .ttl { width: 520px; } .s5 .ttl { width: 480px; } .s8 .ttl { width: 440px; } .s11 .ttl { width: 460px; } .s13 .ttl { width: 500px; } .s15 .ttl { width: 460px; }
  .foot { position: absolute; top: 683px; inset-inline-end: 22px; margin: 0; font-size: 18px; letter-spacing: 3px; color: #222; font-weight: 400; }
  .sample { position: absolute; margin: 0; font-size: 18px; line-height: 1.4; color: var(--ink-2); }
  .deco { position: absolute; top: -50px; inset-inline-end: -50px; width: 260px; height: 260px; border: 2px solid #EFEFEF; border-radius: 50%; }
  .plus { position: absolute; top: 151px; inset-inline-end: 145px; width: 34px; height: 34px; color: var(--navy); }
  .plus::before, .plus::after { content: ''; position: absolute; background: currentColor; }
  .plus::before { top: 15px; left: 0; width: 34px; height: 4px; }
  .plus::after { left: 15px; top: 0; height: 34px; width: 4px; }
  .ph { position: absolute; display: block; object-fit: cover; }
  .dk { background: linear-gradient(150deg, #0F2E78 0%, #0A1536 48%, #04070F 100%); color: #fff; }
  html[dir="rtl"] .dk { background: linear-gradient(210deg, #0F2E78 0%, #0A1536 48%, #04070F 100%); }
  .card { position: absolute; border-radius: 24px; }
  .card.wt { background: #fff; box-shadow: 0 10px 34px rgb(0 0 0 / .08); }
  .card h2 { margin: 0; font-size: 20px; font-weight: 700; line-height: 1.3; }
  .card p { margin: 0; font-size: 17px; line-height: 1.4; }
  .ic { display: grid; place-items: center; border-radius: 50%; }
  .ic svg { width: 56%; height: 56%; }
  .sq { list-style: none; margin: 0; padding: 0; }
  .sq li { position: relative; padding-inline-start: 18px; font-size: 18px; line-height: 1.4; }
  .sq li::before { content: ''; position: absolute; inset-inline-start: 0; top: 10px; width: 6px; height: 6px; background: currentColor; }
  .num { position: absolute; margin: 0; font-weight: 700; color: var(--navy); }
  /* ---- 1 الغلاف ---- */
  .s1 { background: linear-gradient(118deg, #0D2060 0%, #070E2A 38%, #03050E 100%); color: #fff; }
  html[dir="rtl"] .s1 { background: linear-gradient(242deg, #0D2060 0%, #070E2A 38%, #03050E 100%); }
  .s1 .pill { position: absolute; top: 61px; height: 42px; border: 1.5px solid #fff; border-radius: 999px; display: grid; place-items: center; font-size: 18px; margin: 0; color: #fff; }
  .s1 .pill1 { inset-inline-start: 682px; width: 175px; } .s1 .pill2 { inset-inline-start: 871px; width: 110px; }
  .s1 .logo { position: absolute; top: 185px; inset-inline-start: 1017px; width: 60px; height: 60px; color: #fff; }
  .s1 .logo-t { position: absolute; top: 200px; inset-inline-start: 1108px; margin: 0; font-size: 13px; letter-spacing: 1.8px; line-height: 1.45; width: 150px; }
  .s1 .arr { position: absolute; top: 265px; inset-inline-start: 697px; width: 48px; height: 48px; color: #fff; }
  html[dir="rtl"] .s1 .arr { transform: scaleX(-1); }
  .s1 .h { position: absolute; top: 348px; inset-inline-start: 697px; margin: 0; width: 560px; font-size: 70px; font-weight: 800; line-height: 1.02; letter-spacing: -1px; }
  html[lang="ar"] .s1 .h { font-size: 58px; line-height: 1.15; }
  .s1 .sub { position: absolute; top: 520px; inset-inline-start: 697px; margin: 0; font-size: 26px; letter-spacing: 3px; font-weight: 400; }
  .s1 .sample { top: 575px; inset-inline-start: 697px; width: 490px; color: #fff; }
  /* ---- 2 جدول الأعمال ---- */
  .s2 .arr { position: absolute; top: 185px; inset-inline-start: 592px; width: 44px; height: 44px; color: var(--accent); }
  html[dir="rtl"] .s2 .arr { transform: scaleX(-1); }
  .s2 .blk { position: absolute; top: 432px; inset-inline-start: 0; width: 472px; height: 288px; background: linear-gradient(180deg, #0F2A6B 0%, #050A1F 100%); }
  .s2 .ph { top: 258px; inset-inline-start: 72px; width: 590px; height: 390px; }
  .s2 .row { position: absolute; inset-inline-start: 762px; width: 446px; height: 83px; border-bottom: 1.5px solid var(--line); }
  .s2 .row .n { position: absolute; top: 4px; inset-inline-start: 0; margin: 0; font-size: 30px; font-weight: 700; color: var(--navy); }
  .s2 .row p { position: absolute; top: 10px; inset-inline-start: 88px; margin: 0; font-size: 21px; color: var(--ink-2); }
  /* ---- 3 المقدمة ---- */
  .s3 .sample { top: 262px; inset-inline-start: 73px; width: 490px; }
  .s3 .h2 { position: absolute; top: 390px; inset-inline-start: 73px; margin: 0; font-size: 21px; font-weight: 700; color: #3B4252; }
  .s3 .big { position: absolute; top: 428px; inset-inline-start: 73px; margin: 0; font-size: 66px; font-weight: 400; color: var(--accent); line-height: 1.1; }
  .s3 .h3 { position: absolute; top: 540px; inset-inline-start: 73px; margin: 0; font-size: 21px; font-weight: 400; color: var(--ink); }
  .s3 .sq { position: absolute; top: 585px; inset-inline-start: 73px; color: var(--ink-2); }
  .s3 .sq li { line-height: 1.45; }
  .s3 .ph { top: 0; inset-inline-start: 640px; width: 640px; height: 720px; }
  /* ---- 4 دور الذكاء الاصطناعي ---- */
  .s4 .sample { inset-inline-start: 73px; width: 490px; }
  .s4 .p0 { top: 245px; } .s4 .p1 { top: 428px; } .s4 .p2 { top: 600px; }
  .s4 .k { position: absolute; inset-inline-start: 73px; margin: 0; font-size: 22px; font-weight: 700; color: var(--accent); }
  .s4 .h { position: absolute; inset-inline-start: 73px; margin: 0; font-size: 25px; font-weight: 700; color: var(--ink); }
  .s4 .ring { position: absolute; top: 158px; inset-inline-start: 560px; width: 196px; height: 196px; border: 2px solid #ECECEC; border-radius: 50%; }
  .s4 .ring2 { position: absolute; top: 400px; inset-inline-start: 950px; width: 220px; height: 220px; border: 2px solid #F0F0F0; border-radius: 50%; }
  .s4 .dc { position: absolute; top: 157px; inset-inline-start: 698px; width: 200px; height: 203px; border-radius: 30px; }
  .s4 .dc svg { position: absolute; top: 40px; left: 50px; width: 100px; height: 68px; }
  .s4 .dc p { position: absolute; top: 134px; inset-inline-start: 0; width: 100%; margin: 0; text-align: center; font-size: 18px; line-height: 1.35; }
  .s4 .ph1 { top: 108px; inset-inline-start: 930px; width: 278px; height: 280px; border-radius: 40px; border-start-start-radius: 18px; }
  .s4 .ph2 { top: 390px; inset-inline-start: 648px; width: 280px; height: 278px; border-radius: 40px; border-start-start-radius: 18px; }
  .s4 .bub { position: absolute; top: 492px; inset-inline-start: 895px; width: 273px; height: 108px; background: var(--lav); border-radius: 8px; padding: 14px 16px; border: 1px solid #DCC6F3; }
  .s4 .bub h2 { margin: 0 0 6px; font-size: 19px; font-weight: 700; color: var(--ink); }
  .s4 .bub p { margin: 0; font-size: 17px; line-height: 1.35; color: var(--ink-2); }
  .s4 .plus { top: 586px; inset-inline-end: 102px; width: 24px; height: 24px; }
  .s4 .plus::before { top: 10px; width: 24px; } .s4 .plus::after { left: 10px; height: 24px; }
  /* ---- 5 التحديات ---- */
  .s5 .sample { top: 245px; inset-inline-start: 73px; width: 1100px; }
  .s5 .lh { position: absolute; top: 458px; inset-inline-start: 73px; margin: 0; width: 170px; font-size: 26px; font-weight: 700; line-height: 1.25; color: var(--navy); }
  .s5 .lp { top: 537px; inset-inline-start: 73px; width: 285px; font-size: 17px; color: var(--gray); }
  .s5 .card { top: 375px; width: 270px; height: 278px; border-radius: 18px; padding: 32px 27px; }
  .s5 .c1 { inset-inline-start: 383px; } .s5 .c2 { inset-inline-start: 661px; } .s5 .c3 { inset-inline-start: 940px; }
  .s5 .ic { width: 62px; height: 62px; background: var(--lav); color: var(--navy); }
  .s5 .dk .ic { background: #fff; }
  .s5 .card h2 { margin-top: 44px; color: var(--navy); } .s5 .dk h2 { color: #fff; }
  .s5 .card p { margin-top: 12px; font-size: 15px; color: var(--gray); } .s5 .dk p { color: #fff; }
  /* ---- 6 الحلول ---- */
  .s6 .sample { top: 197px; inset-inline-start: 73px; width: 1100px; }
  .s6 .dc { position: absolute; top: 288px; inset-inline-start: 363px; width: 287px; height: 396px; border-radius: 24px; }
  .s6 .col { position: absolute; top: 332px; width: 240px; display: flex; flex-direction: column; align-items: center; text-align: center; }
  .s6 .k1 { inset-inline-start: 66px; } .s6 .k2 { inset-inline-start: 386px; color: #fff; } .s6 .k3 { inset-inline-start: 705px; } .s6 .k4 { inset-inline-start: 973px; }
  .s6 .tile { width: 160px; height: 120px; border-radius: 20px; background: var(--accent); color: #fff; display: grid; place-items: center; }
  .s6 .k2 .tile { background: #fff; color: var(--accent); }
  .s6 .tile svg { width: 56px; height: 56px; }
  .s6 .col h2 { margin: 30px 0 0; font-size: 22px; font-weight: 700; color: var(--ink); }
  .s6 .k2 h2 { color: #fff; }
  .s6 .col hr { width: 180px; border: 0; border-top: 1.5px solid #D9D9D9; margin: 22px 0 18px; }
  .s6 .k2 hr { border-top-color: #fff; }
  .s6 .col p { margin: 0; font-size: 17px; line-height: 1.38; color: var(--ink-2); }
  .s6 .k2 p { color: #fff; }
  /* ---- 7 الإطار والاستراتيجية ---- */
  .s7 .arc { position: absolute; top: 200px; inset-inline-start: 65px; width: 500px; height: 500px; }
  html[dir="rtl"] .s7 .arc { transform: scaleX(-1); }
  .s7 .it { position: absolute; width: 330px; }
  .s7 .it .k { position: absolute; top: 20px; inset-inline-start: 0; margin: 0; font-size: 22px; font-weight: 700; color: var(--navy); }
  .s7 .it .ln { position: absolute; top: 35px; inset-inline-start: 42px; width: 38px; height: 2px; background: var(--navy); }
  .s7 .it.v .k { color: var(--accent); } .s7 .it.v .ln { background: var(--accent); }
  .s7 .it.d .k { color: var(--purple-d); } .s7 .it.d .ln { background: var(--purple-d); }
  .s7 .it h2 { position: absolute; top: 0; inset-inline-start: 109px; margin: 0; font-size: 20px; font-weight: 700; color: var(--ink); }
  .s7 .it p { position: absolute; top: 30px; inset-inline-start: 109px; width: 300px; margin: 0; font-size: 17px; line-height: 1.38; color: var(--ink-2); }
  .s7 .i1 { top: 220px; inset-inline-start: 305px; } .s7 .i2 { top: 345px; inset-inline-start: 305px; }
  .s7 .i3 { top: 220px; inset-inline-start: 790px; } .s7 .i4 { top: 345px; inset-inline-start: 790px; }
  .s7 .dc { position: absolute; top: 468px; inset-inline-start: 660px; width: 545px; height: 204px; border-radius: 18px; }
  .s7 .dc h2 { position: absolute; top: 38px; inset-inline-start: 45px; margin: 0; font-size: 26px; font-weight: 700; }
  .s7 .dc p { position: absolute; top: 96px; inset-inline-start: 45px; width: 430px; margin: 0; font-size: 17px; line-height: 1.38; }
  .s7 .dc .ic { position: absolute; top: 24px; inset-inline-end: 22px; width: 58px; height: 58px; background: #fff; color: var(--navy); }
  /* ---- 8 التقنيات ---- */
  .s8 .ttl { top: 135px; }
  .s8 .wc { top: 285px; inset-inline-start: 73px; width: 385px; height: 380px; border-radius: 30px; }
  .s8 .wc h2 { position: absolute; top: 45px; inset-inline-start: 32px; }
  .s8 .wc p { position: absolute; top: 88px; inset-inline-start: 32px; width: 280px; color: var(--gray); }
  .s8 .big { position: absolute; margin: 0; font-weight: 700; color: var(--accent); line-height: 1; }
  .s8 .wc .big { top: 242px; inset-inline-start: 32px; font-size: 54px; }
  .s8 .card p.lbl { position: absolute; top: 310px; inset-inline-start: 32px; width: 220px; margin: 0; font-size: 20px; color: var(--ink); }
  .s8 .card p.big { color: var(--accent); width: auto; }
  .s8 .orb { position: absolute; top: 549px; inset-inline-start: 345px; width: 110px; height: 110px; border-radius: 50%; background: #fff; display: grid; place-items: center; }
  .s8 .orb .ic { width: 88px; height: 88px; background: var(--navy-d); color: #fff; }
  .s8 .dc { top: 118px; inset-inline-start: 640px; width: 590px; height: 392px; }
  .s8 .dc .r { position: absolute; inset-inline-start: 32px; width: 520px; height: 90px; }
  .s8 .dc .r .ic { position: absolute; top: 0; inset-inline-start: 0; width: 62px; height: 62px; background: #fff; color: var(--navy); }
  .s8 .dc .r h2 { position: absolute; top: -8px; inset-inline-start: 103px; font-size: 22px; }
  .s8 .dc .r p { position: absolute; top: 30px; inset-inline-start: 103px; width: 420px; }
  .s8 .r1 { top: 50px; } .s8 .r2 { top: 165px; } .s8 .r3 { top: 280px; }
  .s8 .bc { top: 532px; inset-inline-start: 478px; width: 752px; height: 133px; }
  .s8 .bc .big { top: 22px; inset-inline-start: 37px; font-size: 48px; }
  .s8 .bc p.lbl { top: 86px; inset-inline-start: 37px; }
  .s8 .bc p { position: absolute; top: 28px; inset-inline-start: 308px; width: 420px; font-size: 18px; color: var(--ink); }
  /* ---- 9 عملية التنفيذ ---- */
  .s9 .plus { top: 109px; inset-inline-end: 140px; } .s9 .deco { top: -80px; }
  .s9 .rings { position: absolute; top: 280px; inset-inline-start: 60px; width: 1160px; height: 320px; }
  html[dir="rtl"] .s9 .rings { transform: scaleX(-1); }
  .s9 .rn { position: absolute; width: 160px; margin: 0; text-align: center; font-size: 30px; color: var(--ink); line-height: 1; }
  .s9 .st { position: absolute; width: 215px; }
  .s9 .st h2 { margin: 0; font-size: 20px; font-weight: 700; color: var(--ink); }
  .s9 .st p { margin: 6px 0 0; font-size: 17px; line-height: 1.35; color: var(--ink-2); }
  .s9 .t1 { top: 208px; inset-inline-start: 262px; } .s9 .t2 { top: 208px; inset-inline-start: 637px; } .s9 .t3 { top: 208px; inset-inline-start: 1010px; }
  .s9 .b1 { top: 548px; inset-inline-start: 73px; } .s9 .b2 { top: 548px; inset-inline-start: 448px; } .s9 .b3 { top: 548px; inset-inline-start: 823px; }
  /* ---- 10 البيانات والتحليلات ---- */
  .s10 .sample { top: 193px; inset-inline-start: 73px; width: 720px; }
  .s10 .chart { position: absolute; top: 308px; inset-inline-start: 73px; width: 562px; height: 372px; border: 1.5px solid #333; border-radius: 14px; overflow: hidden; }
  .s10 .chart svg { width: 100%; height: 100%; }
  .s10 .chart text { font-family: var(--font); font-size: 19px; fill: var(--ink); }
  .s10 .card { inset-inline-start: 652px; width: 258px; height: 175px; border-radius: 14px; }
  .s10 .c1 { top: 305px; } .s10 .c2 { top: 505px; background: var(--accent); color: #fff; }
  .s10 .card h2 { position: absolute; top: 42px; inset-inline-start: 18px; font-size: 20px; font-weight: 400; }
  .s10 .card p { position: absolute; top: 96px; inset-inline-start: 18px; width: 225px; }
  .s10 .dots { position: absolute; top: 18px; inset-inline-end: 14px; display: flex; gap: 6px; }
  .s10 .dots i { width: 9px; height: 9px; border-radius: 50%; background: #fff; display: block; }
  .s10 .c1 .dots i:nth-child(2) { background: var(--accent); } .s10 .c2 .dots i:nth-child(3) { background: var(--navy-d); }
  .s10 .ph { top: 152px; inset-inline-start: 937px; width: 295px; height: 525px; border-radius: 14px; }
  /* ---- 11 دراسة الحالة ---- */
  .s11 .plus { top: 169px; inset-inline-end: 198px; } .s11 .deco { top: -25px; inset-inline-end: 0; }
  .s11 .big { position: absolute; top: 296px; inset-inline-start: 73px; margin: 0; font-size: 66px; font-weight: 700; color: var(--accent); line-height: 1; }
  .s11 .lbl { position: absolute; top: 388px; inset-inline-start: 73px; margin: 0; font-size: 22px; color: var(--ink); }
  .s11 .sq { position: absolute; top: 448px; inset-inline-start: 73px; color: var(--gray); }
  .s11 .sq li { margin-bottom: 8px; }
  .s11 .btn { position: absolute; top: 598px; inset-inline-start: 73px; width: 168px; height: 52px; margin: 0; border-radius: 999px; background: var(--navy-d); color: #fff; display: grid; place-items: center; font-size: 18px; }
  .s11 .ph { top: 272px; inset-inline-start: 409px; width: 386px; height: 375px; border-radius: 30px; }
  .s11 .it { position: absolute; inset-inline-start: 852px; width: 360px; height: 120px; }
  .s11 .it .k { position: absolute; top: 0; inset-inline-start: 0; margin: 0; font-size: 40px; font-weight: 300; color: var(--accent); line-height: 1; }
  .s11 .it h2 { position: absolute; top: 2px; inset-inline-start: 78px; margin: 0; font-size: 20px; font-weight: 700; color: var(--ink); }
  .s11 .it p { position: absolute; top: 32px; inset-inline-start: 78px; width: 290px; margin: 0; font-size: 17px; line-height: 1.3; color: var(--ink-2); }
  .s11 .it .ln { position: absolute; top: 103px; inset-inline-start: 86px; width: 30px; height: 2px; background: var(--accent); }
  .s11 .i1 { top: 290px; } .s11 .i2 { top: 425px; } .s11 .i3 { top: 560px; }
  /* ---- 12 الفوائد ---- */
  .s12 .plus { top: 40px; inset-inline-end: 245px; } .s12 .deco { top: -140px; inset-inline-end: -120px; width: 400px; height: 400px; }
  .s12 .sample { top: 187px; inset-inline-start: 73px; width: 600px; }
  .s12 .pc { top: 103px; inset-inline-start: 782px; width: 410px; height: 192px; border-radius: 22px; background: var(--accent); color: #fff; }
  .s12 .pc h2 { position: absolute; top: 44px; inset-inline-start: 30px; font-size: 24px; }
  .s12 .pc p { position: absolute; top: 100px; inset-inline-start: 30px; width: 340px; font-size: 18px; }
  .s12 .pc svg { position: absolute; top: 28px; inset-inline-end: 18px; width: 36px; height: 36px; }
  .s12 .wc { top: 310px; inset-inline-start: 78px; width: 355px; height: 355px; border-radius: 30px; }
  .s12 .wc p { position: absolute; top: 46px; inset-inline-start: 19px; width: 310px; font-size: 18px; color: var(--gray); }
  .s12 .wc h2 { position: absolute; top: 268px; inset-inline-start: 19px; width: 160px; font-size: 24px; line-height: 1.3; color: var(--ink); }
  .s12 .orb { position: absolute; top: 554px; inset-inline-start: 323px; width: 108px; height: 108px; border-radius: 50%; background: #fff; display: grid; place-items: center; }
  .s12 .orb .ic { width: 84px; height: 84px; background: var(--navy-d); color: #fff; }
  .s12 .dc { top: 310px; inset-inline-start: 449px; width: 756px; height: 355px; border-radius: 14px; overflow: hidden; background: #070C22; color: #fff; }
  .s12 .dc .ph { inset: 0; width: 100%; height: 100%; }
  .s12 .dc .ov { position: absolute; inset: 0; background: linear-gradient(150deg, rgb(10 25 70 / .7) 0%, rgb(8 14 40 / .5) 55%, rgb(3 6 16 / .7) 100%); }
  html[dir="rtl"] .s12 .dc .ov { background: linear-gradient(210deg, rgb(10 25 70 / .7) 0%, rgb(8 14 40 / .5) 55%, rgb(3 6 16 / .7) 100%); }
  .s12 .dc h2 { position: absolute; top: 44px; inset-inline-start: 46px; font-size: 32px; }
  .s12 .dc p { position: absolute; top: 108px; inset-inline-start: 46px; width: 670px; font-size: 20px; line-height: 1.4; }
  .s12 .dc .sq { position: absolute; top: 195px; inset-inline-start: 46px; }
  .s12 .dc .sq li { font-size: 20px; margin-bottom: 6px; }
  /* ---- 13 المخاطر ---- */
  .s13 .sample { top: 245px; inset-inline-start: 73px; width: 490px; }
  .s13 .ph { top: 77px; inset-inline-start: 627px; width: 600px; height: 232px; border-radius: 16px; }
  .s13 .band { position: absolute; top: 420px; inset-inline-start: 0; width: 1280px; height: 300px; background: var(--lav-2); }
  .s13 .it { position: absolute; top: 365px; width: 216px; display: flex; flex-direction: column; align-items: center; text-align: center; }
  .s13 .it .ic { width: 112px; height: 112px; color: #fff; }
  .s13 .it .ic svg { width: 50px; height: 50px; }
  .s13 .it h2 { margin: 38px 0 0; font-size: 20px; font-weight: 700; color: var(--ink); }
  .s13 .it p { margin: 10px 0 0; font-size: 17px; line-height: 1.35; color: var(--ink-2); }
  .s13 .i1 { inset-inline-start: 57px; } .s13 .i2 { inset-inline-start: 294px; } .s13 .i3 { inset-inline-start: 532px; } .s13 .i4 { inset-inline-start: 770px; } .s13 .i5 { inset-inline-start: 1008px; }
  .s13 .i1 .ic { background: var(--accent); } .s13 .i2 .ic { background: var(--blue); } .s13 .i3 .ic { background: var(--navy-d); } .s13 .i4 .ic { background: var(--purple-d); } .s13 .i5 .ic { background: var(--navy); }
  /* ---- 14 النظرة المستقبلية ---- */
  .s14 .deco { top: 560px; inset-inline-start: 640px; inset-inline-end: auto; width: 380px; height: 380px; border-color: #F2F2F2; }
  .s14 .deco2 { position: absolute; top: -140px; inset-inline-start: -120px; width: 300px; height: 300px; border: 2px solid #F0F0F0; border-radius: 50%; }
  .s14 .dc { top: 60px; inset-inline-start: 65px; width: 810px; height: 604px; border-radius: 30px; }
  .s14 .ph { top: 92px; inset-inline-start: 100px; width: 287px; height: 538px; border-radius: 18px; }
  .s14 .h { position: absolute; top: 140px; inset-inline-start: 430px; margin: 0; width: 340px; font-size: 42px; font-weight: 400; line-height: 1.25; color: #fff; }
  .s14 .sample { top: 280px; inset-inline-start: 430px; width: 390px; color: #fff; }
  .s14 .h2 { position: absolute; top: 423px; inset-inline-start: 430px; margin: 0; font-size: 23px; font-weight: 700; color: #fff; }
  .s14 .sq { position: absolute; top: 478px; inset-inline-start: 430px; color: #fff; }
  .s14 .sq li { margin-bottom: 8px; }
  .s14 .wc { inset-inline-start: 840px; width: 372px; height: 170px; }
  .s14 .wc h2 { position: absolute; top: 28px; inset-inline-start: 90px; font-size: 22px; color: var(--ink); }
  .s14 .wc p { position: absolute; top: 72px; inset-inline-start: 90px; width: 240px; color: var(--ink-2); }
  .s14 .wc p.bd { position: absolute; top: 12px; inset-inline-start: auto; inset-inline-end: 10px; width: 48px; height: 48px; border-radius: 50%; background: var(--accent); color: #fff; display: grid; place-items: center; font-size: 17px; font-weight: 700; margin: 0; }
  .s14 .w1 { top: 90px; } .s14 .w2 { top: 275px; } .s14 .w3 { top: 460px; }
  /* ---- 15 الخاتمة ---- */
  .s15 .sample { top: 243px; inset-inline-start: 73px; width: 720px; }
  .s15 .tl { position: absolute; top: 348px; inset-inline-start: 0; width: 1280px; height: 1px; background: #8A8A8A; }
  .s15 .tl2 { top: 437px; }
  .s15 .vl { position: absolute; top: 348px; width: 1px; height: 90px; background: #8A8A8A; }
  .s15 .o { position: absolute; top: 419px; width: 36px; height: 36px; border: 6px solid var(--accent); border-radius: 50%; background: #fff; }
  .s15 .st { position: absolute; top: 492px; width: 250px; }
  .s15 .st .k { margin: 0; font-size: 38px; font-weight: 700; color: var(--ink); line-height: 1; }
  .s15 .st h2 { margin: 28px 0 0; font-size: 22px; font-weight: 700; color: var(--ink); }
  .s15 .st p { margin: 12px 0 0; font-size: 17px; line-height: 1.38; color: var(--ink-2); }
  .s15 .x1 { inset-inline-start: 68px; } .s15 .x2 { inset-inline-start: 383px; } .s15 .x3 { inset-inline-start: 698px; } .s15 .x4 { inset-inline-start: 1013px; }
  .s15 .vl.x1, .s15 .o.x1 { inset-inline-start: 85px; } .s15 .vl.x2, .s15 .o.x2 { inset-inline-start: 400px; } .s15 .vl.x3, .s15 .o.x3 { inset-inline-start: 715px; } .s15 .vl.x4, .s15 .o.x4 { inset-inline-start: 1030px; }
  .s15 .o { margin-inline-start: -18px; } .s15 .vl { margin-inline-start: 0; }"""

# ---- أيقونات SVG بسيطة (خطوط) ----
S = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">%s</svg>'
F = '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">%s</svg>'
ICONS = {
    'arrow_ne': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="square" aria-hidden="true"><path d="M5 19L19 5M8 5h11v11"/></svg>',
    'arrow_se': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="square" aria-hidden="true"><path d="M5 5l14 14M19 8v11H8"/></svg>',
    'logo': S % '<path d="M6 4.5c4-1.5 9-1.5 13.5 1.2.6.4.8 1.1.6 1.8-1 3.6-1.2 7.8-.3 12 .2.8-.4 1.6-1.2 1.5-4.5-.6-9-.3-13.4.5-.8.2-1.6-.5-1.5-1.3.6-4.7.5-9.5-.5-14.3-.1-.6.2-1.2.8-1.4z" stroke-width="2"/>',
    'vr': S % '<path d="M3 9.5c0-1 .8-1.8 1.8-1.8h14.4c1 0 1.8.8 1.8 1.8v5.2c0 1-.8 1.8-1.8 1.8h-3.4c-.6 0-1.1-.3-1.5-.8l-1-1.3a1.6 1.6 0 0 0-2.6 0l-1 1.3c-.4.5-.9.8-1.5.8H4.8c-1 0-1.8-.8-1.8-1.8z"/><path d="M7 7.7V6.2h10v1.5M8 12.2h2M14 12.2h2"/>',
    'people3': F % '<circle cx="12" cy="6.5" r="2.6"/><circle cx="5.5" cy="8.5" r="2.1"/><circle cx="18.5" cy="8.5" r="2.1"/><path d="M12 10.3c-2.8 0-5 1.8-5 4.3V18h10v-3.4c0-2.5-2.2-4.3-5-4.3z"/><path d="M5.5 11.4c-2 0-3.5 1.3-3.5 3V17h3.4v-2.7c0-1.3.5-2.5 1.4-3.4a3.6 3.6 0 0 0-1.3-.5z"/><path d="M18.5 11.4c-.5 0-.9.2-1.3.5.9.9 1.4 2.1 1.4 3.4V17H22v-2.6c0-1.7-1.5-3-3.5-3z"/>',
    'people_chat': S % '<circle cx="8" cy="7" r="2.3"/><circle cx="16" cy="7" r="2.3"/><path d="M3.5 17v-1.5c0-2.3 2-4 4.5-4s4.5 1.7 4.5 4V17M12 15.5c0-2.3 1.8-4 4-4s4 1.7 4 4V17"/><path d="M9.5 3.5h5l-1 1.5h-3z"/>',
    'people_talk': S % '<circle cx="7.5" cy="7.5" r="2.4"/><circle cx="16.5" cy="9" r="2.1"/><path d="M3 18v-1.6c0-2.4 2-4.2 4.5-4.2s4.5 1.8 4.5 4.2V18M13 18v-1.3c0-2 1.6-3.5 3.5-3.5S20 14.7 20 16.7V18"/><path d="M12 5.5h4.5M12 3.5h6"/>',
    'eye': F % '<path d="M12 5C6.5 5 2.5 9.5 1.5 12c1 2.5 5 7 10.5 7s9.5-4.5 10.5-7C21.5 9.5 17.5 5 12 5zm0 11a4 4 0 1 1 0-8 4 4 0 0 1 0 8zm0-6a2 2 0 1 0 0 4 2 2 0 0 0 0-4z"/>',
    'bag': F % '<path d="M8 9V7a4 4 0 0 1 8 0v2h3l-1 11H6L5 9h3zm1.6 0h4.8V7a2.4 2.4 0 0 0-4.8 0v2z"/>',
    'layers': F % '<path d="M12 3l9 4.5-9 4.5-9-4.5L12 3z"/><path d="M4.5 11.8L12 15.5l7.5-3.7 1.5.7-9 4.5-9-4.5 1.5-.7z"/><path d="M4.5 15.8L12 19.5l7.5-3.7 1.5.7-9 4.5-9-4.5 1.5-.7z"/>',
    'puzzle': F % '<path d="M10 3a2 2 0 0 1 2 2v1h3a1 1 0 0 1 1 1v3h1a2 2 0 1 1 0 4h-1v4a1 1 0 0 1-1 1h-3v-1a2 2 0 1 0-4 0v1H5a1 1 0 0 1-1-1v-4h1a2 2 0 1 0 0-4H4V7a1 1 0 0 1 1-1h3V5a2 2 0 0 1 2-2z"/>',
    'search': S % '<circle cx="10" cy="10" r="6"/><path d="M14.5 14.5L20 20"/>',
    'bulb': S % '<path d="M9 18h6M10 21h4M8.5 14.5A6 6 0 1 1 15.5 14.5c-.8.7-1.3 1.6-1.5 2.5h-4c-.2-.9-.7-1.8-1.5-2.5z"/><path d="M12 8v3l1.5 1.5"/>',
    'rocket': S % '<path d="M14 4c3 0 6 2 6 6-2 3-6 6-10 7l-3-3c1-4 4-8 7-10z"/><circle cx="14.5" cy="9.5" r="1.5"/><path d="M8 14l-3 1 1-3M10 17l-1 3 3-1M6 18l-2 2"/>',
    'chart': S % '<path d="M3 20h18M5 20V9h3v11M10.5 20V4h3v16M16 20v-7h3v7"/><path d="M5 8l5-4 4 2 5-3" stroke-dasharray="1.5 1.5"/>',
    'people_group': F % '<circle cx="12" cy="7" r="2.6"/><circle cx="6" cy="9" r="2"/><circle cx="18" cy="9" r="2"/><path d="M7 18v-2.5c0-2.5 2.2-4.3 5-4.3s5 1.8 5 4.3V18H7z"/><path d="M2 17v-1.8c0-1.7 1.6-3 3.8-3 .4 0 .9.1 1.3.2A5.3 5.3 0 0 0 5.5 16V17H2zM22 17v-1.8c0-1.7-1.6-3-3.8-3-.4 0-.9.1-1.3.2a5.3 5.3 0 0 1 1.6 3.6V17H22z"/>',
    'doc_globe': S % '<path d="M6 3h8l4 4v6M6 3v18h7"/><path d="M14 3v4h4"/><circle cx="17" cy="17" r="4"/><path d="M13 17h8M17 13c-1.3 1.2-1.3 6.8 0 8M17 13c1.3 1.2 1.3 6.8 0 8"/>',
    'bulb_gear': S % '<path d="M9.5 17.5h5M10.5 20h3M8 13.5a5.5 5.5 0 1 1 8 0c-.7.7-1 1.4-1.2 2.2H9.2c-.2-.8-.5-1.5-1.2-2.2z"/><circle cx="12" cy="9.5" r="1.6"/><path d="M12 6.5v1M12 11.5v1M9 9.5h1M14 9.5h1"/>',
    'head_gear': F % '<path d="M12 2.5a7 7 0 0 0-7 7c0 2 .7 3.3 1.6 4.4L5.5 17h2l.5 3h5l.3-2.4c3.1-.9 5.7-3.4 5.7-7.1a7 7 0 0 0-7-7.5zm1.2 10.3l-.3 1.2h-1.8l-.3-1.2-1-.5-1.1.4-.9-1.5.9-.8v-1.1l-.9-.8.9-1.5 1.1.4 1-.5.3-1.2h1.8l.3 1.2 1 .5 1.1-.4.9 1.5-.9.8v1.1l.9.8-.9 1.5-1.1-.4-1 .5zM12 10.7a1.5 1.5 0 1 1 0-3 1.5 1.5 0 0 1 0 3z"/>',
    'crowd': F % '<circle cx="6" cy="6" r="2"/><circle cx="12" cy="5" r="2"/><circle cx="18" cy="6" r="2"/><path d="M3.5 9h5v10h-5zM9.5 8h5v12h-5zM15.5 9h5v10h-5z"/>',
    'mail': S % '<path d="M7 7h14v11H7z"/><path d="M7 8l7 5 7-5M3 10h3M2 13h4M3 16h3"/>',
    'hand_dollar': S % '<circle cx="12" cy="7" r="4"/><path d="M12 4.8v4.4M11 7.2h2a.8.8 0 0 0 0-1.6h-2a.8.8 0 0 0 0-1.6M4 14h3l4 1.5h4a1 1 0 0 1 0 2h-3M4 20h4l5 1 7-3-1-1.5-4 1M12 2v1M12 11v1"/>',
    'camera': S % '<rect x="3" y="7" width="18" height="12" rx="2"/><circle cx="12" cy="13" r="3.2"/><path d="M8 7l1.5-2.5h5L16 7M17.5 10h1"/>',
    'coins': S % '<ellipse cx="12" cy="6" rx="6" ry="2.2"/><path d="M6 6v4c0 1.2 2.7 2.2 6 2.2s6-1 6-2.2V6M6 10v4c0 1.2 2.7 2.2 6 2.2s6-1 6-2.2v-4M6 14v4c0 1.2 2.7 2.2 6 2.2s6-1 6-2.2v-4"/><path d="M12 9.5v2M11 10.5h2"/>',
    'target': S % '<circle cx="12" cy="12" r="8"/><circle cx="12" cy="12" r="4.5"/><circle cx="12" cy="12" r="1.2"/><path d="M12 12l6-6M18 6l2.5-.5-1-2L18 6z"/>',
}

PH = '/templates/freeform/' + KEY + '/assets/'
FOOT = '<p class="foot">§FOOT§</p>'
DECO = '<div class="deco" aria-hidden="true"></div><div class="plus" aria-hidden="true"></div>'


def img(name, cls, alt):
    return '<img class="ph %s" alt="§%s§" src="%s%s.jpg">' % (cls, alt, PH, name)


# ---- 7: قوس بأربعة قطاعات حول مركز (280,255) داخل 500×500 ----
import math


def sector(cx, cy, r1, r2, a1, a2, fill):
    """قطاع حلقي من الزاوية a1 إلى a2 (بالدرجات، عكس عقارب الساعة بصريًا = تناقص الزاوية)"""
    def pt(r, a):
        return (cx + r * math.cos(math.radians(a)), cy + r * math.sin(math.radians(a)))
    p1, p2, p3, p4 = pt(r2, a1), pt(r2, a2), pt(r1, a2), pt(r1, a1)
    large = 1 if abs(a2 - a1) > 180 else 0
    return '<path fill="%s" d="M%.1f,%.1f A%d,%d 0 %d 0 %.1f,%.1f L%.1f,%.1f A%d,%d 0 %d 1 %.1f,%.1f Z"/>' % (
        fill, p1[0], p1[1], r2, r2, large, p2[0], p2[1], p3[0], p3[1], r1, r1, large, p4[0], p4[1])


def arc7():
    cx, cy, r1, r2 = 285, 255, 150, 268
    segs = [(-108, -163, '#0E1F57', 'search'), (-167, -222, '#A35CF0', 'bulb'), (-226, -281, '#0B1437', 'rocket'), (-285, -340, '#5C0F8B', 'chart')]
    out = '<svg class="arc" viewBox="0 0 500 500" aria-hidden="true">'
    for a1, a2, col, ico in segs:
        out += sector(cx, cy, r1, r2, a1, a2, col)
        am = math.radians((a1 + a2) / 2)
        ix, iy = cx + 209 * math.cos(am), cy + 209 * math.sin(am)
        out += '<g transform="translate(%.1f %.1f)" color="#fff"><svg x="-22" y="-22" width="44" height="44" viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round">%s</svg></g>' % (ix, iy, ICONS[ico][ICONS[ico].index('>') + 1:-6])
    return out + '</svg>'


# ---- 9: حلقات مرقّمة بقوس خارجي وموصّلات ----
def rings9():
    cs = [(175, 103, '#5C0F8B', 1), (360, 223, '#0B1437', 0), (545, 103, '#8A2BE2', 1), (730, 223, '#0B2F8A', 0), (915, 103, '#B266F0', 1), (1100, 223, '#0E1F57', 0)]
    # إحداثيات داخل svg: y = y_أصل - 280 ، x = x_أصل - 60
    out = '<svg class="rings" viewBox="0 0 1160 320" aria-hidden="true">'
    # الموصّلات الرمادية
    out += '<path fill="none" stroke="#7A7F86" stroke-width="2" d="M115,183 V222 q0,12 12,12 H220 M300,143 V116 q0,-12 12,-12 H405 M485,183 V222 q0,12 12,12 H590 M670,143 V116 q0,-12 12,-12 H775 M855,183 V222 q0,12 12,12 H960"/>'
    for x, y, col, up in cs:
        x -= 60
        out += '<circle cx="%d" cy="%d" r="66" fill="none" stroke="%s" stroke-width="28"/>' % (x, y, col)
        # قوس زخرفي خارجي
        if up:
            a1, a2 = -92, 10
        else:
            a1, a2 = 190, 272
        p = lambda r, a: (x + r * math.cos(math.radians(a)), y + r * math.sin(math.radians(a)))
        s, e = p(89, a1), p(89, a2)
        out += '<path fill="none" stroke="%s" stroke-width="7" stroke-linecap="round" d="M%.1f,%.1f A89,89 0 0 1 %.1f,%.1f"/>' % (col, s[0], s[1], e[0], e[1])
    return out + '</svg>'


def rn9():
    pos = [(175, 383), (360, 503), (545, 383), (730, 503), (915, 383), (1100, 503)]
    return ''.join('<p class="rn" style="top:%dpx;inset-inline-start:%dpx">0%d</p>' % (y - 15, x - 80, i + 1) for i, (x, y) in enumerate(pos))


SLIDES = [
    # 1 الغلاف
    '\n    ' + img('robot-hand', '', 'ALT1') + '\n    <p class="pill pill1">§EDIT§</p><p class="pill pill2">2040</p>'
    + '\n    <div class="logo" aria-hidden="true">' + ICONS['logo'] + '</div><p class="logo-t">§LOGO§</p>'
    + '\n    <div class="arr" aria-hidden="true">' + ICONS['arrow_ne'] + '</div>'
    + '\n    <h1 class="h">§T1§</h1><p class="sub">§FOOT§</p><p class="sample">§SMP2§</p>\n  ',
    # 2 جدول الأعمال
    '\n    <h1 class="ttl">§T2§</h1><div class="arr" aria-hidden="true">' + ICONS['arrow_se'] + '</div><div class="blk" aria-hidden="true"></div>'
    + '\n    ' + img('woman-screen', '', 'ALT2')
    + ''.join('\n    <div class="row" style="top:%dpx"><p class="n">0%d</p><p>§A%d§</p></div>' % (258 + i * 83, i + 1, i + 1) for i in range(5))
    + '\n    ' + FOOT + '\n  ',
    # 3 المقدمة
    '\n    <h1 class="ttl">§T3§</h1><p class="sample">§SMP3§</p><h2 class="h2">§EDIT§</h2><p class="big">8,067</p><h3 class="h3">§EDIT§</h3>'
    + '\n    <ul class="sq"><li>§B1§</li><li>§B2§</li><li>§B3§</li></ul>'
    + '\n    ' + img('robot-face', '', 'ALT3') + '\n  ',
    # 4 دور الذكاء الاصطناعي
    '\n    <h1 class="ttl">§T4§</h1><p class="sample p0">§SMP2§</p>'
    + '\n    <p class="k" style="top:345px">01</p><h2 class="h" style="top:385px">§EDIT§</h2><p class="sample p1">§SMP2§</p>'
    + '\n    <p class="k" style="top:517px">02</p><h2 class="h" style="top:557px">§EDIT§</h2><p class="sample p2">§SMP2§</p>'
    + '\n    <div class="ring" aria-hidden="true"></div><div class="ring2" aria-hidden="true"></div>'
    + '\n    ' + img('robot-screens', 'ph1', 'ALT4A') + img('man-tablet', 'ph2', 'ALT4B')
    + '\n    <div class="dc dk">' + ICONS['vr'] + '<p>§SMP1§</p></div>'
    + '\n    <div class="bub"><h2>§EDIT§</h2><p>§SMP4§</p></div><div class="plus" aria-hidden="true"></div>'
    + '\n    ' + FOOT + '\n  ',
    # 5 التحديات
    '\n    ' + DECO + '<h1 class="ttl">§T5§</h1><p class="sample">§SMP5§</p>'
    + '\n    <h2 class="lh">§EDITY§</h2><p class="sample lp">§SMP2§</p>'
    + '\n    <div class="card c1 dk"><div class="ic">' + ICONS['people3'] + '</div><h2>§EDIT§</h2><p>§SMP6§</p></div>'
    + '\n    <div class="card c2 wt"><div class="ic">' + ICONS['people_chat'] + '</div><h2>§EDIT§</h2><p>§SMP6§</p></div>'
    + '\n    <div class="card c3 wt"><div class="ic">' + ICONS['people_talk'] + '</div><h2>§EDIT§</h2><p>§SMP6§</p></div>'
    + '\n    ' + FOOT + '\n  ',
    # 6 الحلول
    '\n    <h1 class="ttl">§T6§</h1><p class="sample">§SMP5§</p><div class="dc dk" aria-hidden="true"></div>'
    + ''.join('\n    <div class="col k%d"><div class="tile">%s</div><h2>§EDIT§</h2><hr><p>§SMP6§</p></div>' % (i + 1, ICONS[ic]) for i, ic in enumerate(['eye', 'bag', 'layers', 'puzzle']))
    + '\n    ' + FOOT + '\n  ',
    # 7 الإطار والاستراتيجية
    '\n    ' + DECO + '<h1 class="ttl">§T7§</h1>' + arc7()
    + '\n    <div class="it i1"><p class="k">01</p><span class="ln"></span><h2>§EDIT§</h2><p>§SMP7§</p></div>'
    + '\n    <div class="it i2 v"><p class="k">02</p><span class="ln"></span><h2>§EDIT§</h2><p>§SMP7§</p></div>'
    + '\n    <div class="it i3"><p class="k">03</p><span class="ln"></span><h2>§EDIT§</h2><p>§SMP7§</p></div>'
    + '\n    <div class="it i4 d"><p class="k">04</p><span class="ln"></span><h2>§EDIT§</h2><p>§SMP7§</p></div>'
    + '\n    <div class="dc dk"><h2>§EDIT§</h2><p>§SMP3§</p><div class="ic">' + ICONS['people_group'] + '</div></div>\n  ',
    # 8 التقنيات
    '\n    <h1 class="ttl">§T8§</h1>'
    + '\n    <div class="card wc wt"><h2>§EDIT§</h2><p>§SMP2§</p><p class="big">1.5M</p><p class="lbl">§EDIT§</p></div>'
    + '\n    <div class="orb"><div class="ic">' + ICONS['head_gear'] + '</div></div>'
    + '\n    <div class="card dc dk">'
    + ''.join('<div class="r r%d"><div class="ic">%s</div><h2>§EDIT§</h2><p>§SMP2§</p></div>' % (i + 1, ICONS[ic]) for i, ic in enumerate(['doc_globe', 'bulb_gear', 'head_gear']))
    + '</div>'
    + '\n    <div class="card bc wt"><p class="big">42.3K</p><p class="lbl">§EDIT§</p><p>§SMP8§</p></div>'
    + '\n    ' + FOOT + '\n  ',
    # 9 عملية التنفيذ
    '\n    ' + DECO + '<h1 class="ttl">§T9§</h1>' + rings9() + rn9()
    + ''.join('\n    <div class="st %s"><h2>§EDIT§</h2><p>§SMP9§</p></div>' % c for c in ['t1', 't2', 't3', 'b1', 'b2', 'b3'])
    + '\n    ' + FOOT + '\n  ',
    # 10 البيانات والتحليلات
    '\n    <h1 class="ttl">§T10§</h1><p class="sample">§SMP10§</p><div class="chart" id="chart" role="img" aria-label="§CHART_ALT§"></div>'
    + '\n    <div class="card c1 dk"><div class="dots" aria-hidden="true"><i></i><i></i><i></i></div><h2>§EDIT§</h2><p>§SMP4§</p></div>'
    + '\n    <div class="card c2"><div class="dots" aria-hidden="true"><i></i><i></i><i></i></div><h2>§EDIT§</h2><p>§SMP4§</p></div>'
    + '\n    ' + img('robot-woman', '', 'ALT10') + '\n  ',
    # 11 دراسة الحالة
    '\n    ' + DECO + '<h1 class="ttl">§T11§</h1><p class="big">79.7K</p><p class="lbl">§EDIT§</p>'
    + '\n    <ul class="sq"><li>§B1§</li><li>§B2§</li><li>§B3§</li><li>§B4§</li></ul><p class="btn">§EDIT§</p>'
    + '\n    ' + img('robot-gold', '', 'ALT11')
    + ''.join('\n    <div class="it i%d"><p class="k">0%d</p><h2>§EDIT§</h2><p>§SMP7§</p>%s</div>' % (i, i, '<span class="ln"></span>' if i < 3 else '') for i in (1, 2, 3))
    + '\n    ' + FOOT + '\n  ',
    # 12 الفوائد
    '\n    ' + DECO + '<h1 class="ttl">§T12§</h1><p class="sample">§SMP11§</p>'
    + '\n    <div class="card pc"><h2>§EDIT§</h2><p>§SMP7§</p>' + ICONS['crowd'] + '</div>'
    + '\n    <div class="card wc wt"><p>§SMP11§</p><h2>§EDITY§</h2></div><div class="orb"><div class="ic">' + ICONS['head_gear'] + '</div></div>'
    + '\n    <div class="card dc"><img class="ph" alt="§ALT12§" src="' + PH + 'coder-bg.jpg"><div class="ov" aria-hidden="true"></div><h2>§EDITY§</h2><p>§SMP11§</p><ul class="sq"><li>§B1§</li><li>§B2§</li><li>§B3§</li></ul></div>\n  ',
    # 13 المخاطر
    '\n    <h1 class="ttl">§T13§</h1><p class="sample">§SMP2§</p>' + img('man-robot', '', 'ALT13') + '<div class="band" aria-hidden="true"></div>'
    + ''.join('\n    <div class="it i%d"><div class="ic">%s</div><h2>§EDIT§</h2><p>§SMP9§</p></div>' % (i + 1, ICONS[ic]) for i, ic in enumerate(['mail', 'hand_dollar', 'camera', 'coins', 'target'])) + '\n  ',
    # 14 النظرة المستقبلية
    '\n    <div class="deco2" aria-hidden="true"></div><div class="deco" aria-hidden="true"></div><div class="card dc dk"></div>' + img('man-desk', '', 'ALT14')
    + '\n    <h1 class="h">§T14§</h1><p class="sample">§SMP8§</p><h2 class="h2">§EDIT§</h2><ul class="sq"><li>§B1§</li><li>§B2§</li><li>§B3§</li></ul>'
    + ''.join('\n    <div class="card wc wt w%d"><h2>§EDIT§</h2><p>§SMP6§</p><p class="bd">0%d</p></div>' % (i, i) for i in (1, 2, 3))
    + '\n    ' + FOOT + '\n  ',
    # 15 الخاتمة
    '\n    <h1 class="ttl">§T15§</h1><p class="sample">§SMP10§</p><div class="tl" aria-hidden="true"></div><div class="tl tl2" aria-hidden="true"></div>'
    + ''.join('<div class="vl x%d" aria-hidden="true"></div><div class="o x%d" aria-hidden="true"></div>' % (i, i) for i in (1, 2, 3, 4))
    + ''.join('\n    <div class="st x%d"><p class="k">0%d</p><h2>§EDIT§</h2><p>§SMP6§</p></div>' % (i, i) for i in (1, 2, 3, 4)) + '\n  ',
]

STR = {
    'en': {
        'FOOT': 'PRESENTATION TEMPLATE', 'EDIT': 'Edit Text Here', 'EDITY': 'Edit Your Text Here',
        'LOGO': 'YOUR COMPANY LOGO HERE',
        'T1': 'AI Digital Transformation',
        'T2': 'Agenda &amp; Overview <span class="lt">Slide</span>',
        'T3': 'Introduction To<br>Digital Transformation',
        'T4': 'Role of<br>Artificial Intelligence',
        'T5': 'Current<br>Business Challenges',
        'T6': 'AI-Powered Solutions <span class="lt">Slide</span>',
        'T7': 'Transformation Framework &amp; Strategy',
        'T8': 'Key Technologies<br>Involved <span class="lt">Slide</span>',
        'T9': 'Implementation Process <span class="lt">Slide</span>',
        'T10': 'Data &amp; Analytics Integration',
        'T11': 'Case Study &amp;<br>Success Story <span class="lt">Slide</span>',
        'T12': 'Benefits &amp; ROI <span class="lt">Slide</span>',
        'T13': 'Challenges &amp;<br>Risk Mitigation <span class="lt">Slide</span>',
        'T14': 'Future Outlook<br>&amp; Trends',
        'T15': 'Conclusion &amp;<br>Call to Action <span class="lt">Slide</span>',
        'SMP1': 'This is a sample text.',
        'SMP2': 'This is a sample text. Insert your desired text here. You can add sample text. This is a sample text.',
        'SMP3': 'This is a sample text. Insert your desired text here. You can add sample text. This is a sample text. Insert your sample text here. This is a sample text.',
        'SMP4': 'This is a sample text. Insert your desired text here.',
        'SMP5': 'This is a sample text. Insert your desired text here. You can add sample text. This is a sample text. Insert your sample text here. This is a sample text. Insert your desired text here. You can add sample text. This is a sample text.',
        'SMP6': 'This is a sample text. Insert your desired text here. You can add sample text.',
        'SMP7': 'This is a sample text. Insert your text here. You can add sample text.',
        'SMP8': 'This is a sample text. Insert your desired text here. You can add sample text. This is a sample text. Insert your sample text here.',
        'SMP9': 'This is a sample text. Insert your desired text here. You can add text.',
        'SMP10': 'This is a sample text. Insert your desired text here. You can add sample text. This is a sample text. Insert your sample text here. This is a sample text.',
        'SMP11': 'This is a sample text. Insert your text here. You can add sample text. This is a sample text. Insert your sample text here.',
        'A1': 'Insert your desired text here.', 'A2': 'You can add sample text', 'A3': 'Edit your text here', 'A4': 'Insert your sample text here.', 'A5': 'You can add sample text',
        'B1': 'This is a sample text', 'B2': 'Insert your desired text here', 'B3': 'You can add sample text', 'B4': 'Insert your sample text here',
        'ALT1': 'A robotic hand reaching toward a human hand', 'ALT2': 'A woman touching a holographic interface', 'ALT3': 'A humanoid robot face with glowing circuits',
        'ALT4A': 'A humanoid robot in front of data screens', 'ALT4B': 'A businessman holding a tablet', 'ALT10': 'A humanoid robot touching a data screen',
        'ALT11': 'A humanoid robot with glowing eyes', 'ALT12': 'A developer working at night', 'ALT13': 'A businessman facing a humanoid robot', 'ALT14': 'A man working on computers at night',
        'CHART_ALT': 'Column chart of yearly values from 2090 to 2094',
    },
    'ar': {
        'FOOT': 'قالب عرض تقديمي', 'EDIT': 'عدّل النص هنا', 'EDITY': 'عدّل نصك هنا',
        'LOGO': 'شعار شركتك هنا',
        'T1': 'التحول الرقمي بالذكاء الاصطناعي',
        'T2': '<span class="lt">شريحة</span> جدول الأعمال والنظرة العامة',
        'T3': 'مقدمة في التحول الرقمي',
        'T4': 'دور الذكاء الاصطناعي',
        'T5': 'تحديات الأعمال الحالية',
        'T6': '<span class="lt">شريحة</span> الحلول المدعومة بالذكاء الاصطناعي',
        'T7': 'إطار التحول واستراتيجيته',
        'T8': '<span class="lt">شريحة</span> التقنيات الرئيسية المعنية',
        'T9': '<span class="lt">شريحة</span> عملية التنفيذ',
        'T10': 'تكامل البيانات والتحليلات',
        'T11': '<span class="lt">شريحة</span> دراسة الحالة وقصة النجاح',
        'T12': '<span class="lt">شريحة</span> الفوائد والعائد على الاستثمار',
        'T13': '<span class="lt">شريحة</span> التحديات والحد من المخاطر',
        'T14': 'النظرة المستقبلية والاتجاهات',
        'T15': '<span class="lt">شريحة</span> الخاتمة والدعوة إلى العمل',
        'SMP1': 'هذا نص توضيحي.',
        'SMP2': 'هذا نص توضيحي. أضف النص المطلوب هنا. يمكنك إضافة نص توضيحي. هذا نص توضيحي.',
        'SMP3': 'هذا نص توضيحي. أضف النص المطلوب هنا. يمكنك إضافة نص توضيحي. هذا نص توضيحي. أضف نصك التوضيحي هنا. هذا نص توضيحي.',
        'SMP4': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
        'SMP5': 'هذا نص توضيحي. أضف النص المطلوب هنا. يمكنك إضافة نص توضيحي. هذا نص توضيحي. أضف نصك التوضيحي هنا. هذا نص توضيحي. أضف النص المطلوب هنا. يمكنك إضافة نص توضيحي. هذا نص توضيحي.',
        'SMP6': 'هذا نص توضيحي. أضف النص المطلوب هنا. يمكنك إضافة نص توضيحي.',
        'SMP7': 'هذا نص توضيحي. أضف نصك هنا. يمكنك إضافة نص توضيحي.',
        'SMP8': 'هذا نص توضيحي. أضف النص المطلوب هنا. يمكنك إضافة نص توضيحي. هذا نص توضيحي. أضف نصك التوضيحي هنا.',
        'SMP9': 'هذا نص توضيحي. أضف النص المطلوب هنا. يمكنك إضافة نص.',
        'SMP10': 'هذا نص توضيحي. أضف النص المطلوب هنا. يمكنك إضافة نص توضيحي. هذا نص توضيحي. أضف نصك التوضيحي هنا. هذا نص توضيحي.',
        'SMP11': 'هذا نص توضيحي. أضف نصك هنا. يمكنك إضافة نص توضيحي. هذا نص توضيحي. أضف نصك التوضيحي هنا.',
        'A1': 'أضف النص المطلوب هنا.', 'A2': 'يمكنك إضافة نص توضيحي', 'A3': 'عدّل نصك هنا', 'A4': 'أضف نصك التوضيحي هنا.', 'A5': 'يمكنك إضافة نص توضيحي',
        'B1': 'هذا نص توضيحي', 'B2': 'أضف النص المطلوب هنا', 'B3': 'يمكنك إضافة نص توضيحي', 'B4': 'أضف نصك التوضيحي هنا',
        'ALT1': 'يد آلية تمتد نحو يد بشرية', 'ALT2': 'امرأة تلمس واجهة ثلاثية الأبعاد', 'ALT3': 'وجه روبوت بشري بدوائر مضيئة',
        'ALT4A': 'روبوت بشري أمام شاشات بيانات', 'ALT4B': 'رجل أعمال يحمل جهازًا لوحيًا', 'ALT10': 'روبوت بشري يلمس شاشة بيانات',
        'ALT11': 'روبوت بشري بعينين مضيئتين', 'ALT12': 'مطوّر يعمل ليلًا', 'ALT13': 'رجل أعمال في مواجهة روبوت بشري', 'ALT14': 'رجل يعمل على حواسيب ليلًا',
        'CHART_ALT': 'رسم أعمدة للقيم السنوية من 2090 إلى 2094',
    },
}

# ---- بيانات الرسم (الشريحة 10): 11 عمودًا فوق المحور و11 تحته بالتناوب بين الكحلي والبنفسجي ----
JSON_BLOCKS = {'chart-data': {
    'labels': ['2090', '2091', '2092', '2093', '2094'],
    'pcts': ['10%', '25%', '34%', '43%', '56%'],
    'bars': [{'up': 55, 'down': 60, 'top': 'navy'}, {'up': 43, 'down': 40, 'top': 'purple'}, {'up': 80, 'down': 80, 'top': 'navy'},
             {'up': 65, 'down': 70, 'top': 'purple'}, {'up': 40, 'down': 35, 'top': 'purple'}, {'up': 92, 'down': 95, 'top': 'navy'},
             {'up': 75, 'down': 80, 'top': 'purple'}, {'up': 77, 'down': 83, 'top': 'navy'}, {'up': 77, 'down': 83, 'top': 'navy'},
             {'up': 55, 'down': 55, 'top': 'purple'}, {'up': 105, 'down': 105, 'top': 'navy'}],
}}

GLOBAL_DEFS = ('<defs><linearGradient id="gNavy" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#0E2A6E"/><stop offset="1" stop-color="#060A1C"/></linearGradient>'
               '<linearGradient id="gNavyR" x1="0" y1="1" x2="0" y2="0"><stop offset="0" stop-color="#0E2A6E"/><stop offset="1" stop-color="#060A1C"/></linearGradient></defs>')

EXTRA_SCRIPT = """  // رسم أعمدة الشريحة 10 من كتلة chart-data؛ الفئة الأولى في جهة البداية (اليمين للعربية)
  function drawChart() {
    var el = document.getElementById('chart'); if (!el) return;
    var d = JSON.parse(document.getElementById('chart-data').textContent);
    var W = 562, H = 372, axis = 187, bw = 24, x0 = 52, step = 44;
    var fx = function (x) { return RTL ? W - x : x; };
    var s = '<svg viewBox="0 0 ' + W + ' ' + H + '" aria-hidden="true">';
    s += '<line x1="' + fx(20) + '" y1="' + axis + '" x2="' + fx(540) + '" y2="' + axis + '" stroke="#333" stroke-width="1.5"/>';
    s += '<circle cx="' + fx(540) + '" cy="' + axis + '" r="5" fill="#222"/>';
    var col = function (c, up) { return c === 'navy' ? ('url(#' + (up ? 'gNavy' : 'gNavyR') + ')') : '#A35CF0'; };
    d.bars.forEach(function (b, i) {
      var cx = fx(x0 + i * step), down = b.top === 'navy' ? 'purple' : 'navy';
      s += '<rect x="' + (cx - bw / 2) + '" y="' + (axis - 4 - b.up) + '" width="' + bw + '" height="' + b.up + '" rx="4" fill="' + col(b.top, true) + '"/>';
      s += '<rect x="' + (cx - bw / 2) + '" y="' + (axis + 4) + '" width="' + bw + '" height="' + b.down + '" rx="4" fill="' + col(down, false) + '"/>';
    });
    d.labels.forEach(function (l, i) {
      var cx = fx(x0 + 10 + i * step * 2.5);
      s += '<text x="' + cx + '" y="42" text-anchor="middle">' + d.pcts[i] + '</text>';
      s += '<text x="' + cx + '" y="348" text-anchor="middle">' + l + '</text>';
    });
    el.innerHTML = s + '</svg>';
  }
  drawChart();"""

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'mgmt', 'icon': '🤖',
    'name': 'التحول الرقمي بالذكاء الاصطناعي', 'nameEn': 'AI Digital Transformation',
    'description': '15 شريحة: غلاف، جدول أعمال، تحديات، حلول، إطار، تقنيات، تنفيذ، بيانات، دراسة حالة، فوائد، مخاطر، خاتمة',
    'descriptionEn': '15 slides: cover, agenda, challenges, solutions, framework, process, data chart, case study, ROI, risks, CTA',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'جدول الأعمال والنظرة العامة', 'titleEn': 'Agenda & Overview'},
        {'title': 'مقدمة في التحول الرقمي', 'titleEn': 'Introduction To Digital Transformation', 'needsData': True},
        {'title': 'دور الذكاء الاصطناعي', 'titleEn': 'Role of Artificial Intelligence'},
        {'title': 'تحديات الأعمال الحالية', 'titleEn': 'Current Business Challenges'},
        {'title': 'الحلول المدعومة بالذكاء الاصطناعي', 'titleEn': 'AI-Powered Solutions'},
        {'title': 'إطار التحول واستراتيجيته', 'titleEn': 'Transformation Framework & Strategy'},
        {'title': 'التقنيات الرئيسية المعنية', 'titleEn': 'Key Technologies Involved', 'needsData': True},
        {'title': 'عملية التنفيذ', 'titleEn': 'Implementation Process'},
        {'title': 'تكامل البيانات والتحليلات (رسم)', 'titleEn': 'Data & Analytics Integration (chart)', 'needsData': True},
        {'title': 'دراسة الحالة وقصة النجاح', 'titleEn': 'Case Study & Success Story', 'needsData': True},
        {'title': 'الفوائد والعائد على الاستثمار', 'titleEn': 'Benefits & ROI'},
        {'title': 'التحديات والحد من المخاطر', 'titleEn': 'Challenges & Risk Mitigation'},
        {'title': 'النظرة المستقبلية والاتجاهات', 'titleEn': 'Future Outlook & Trends'},
        {'title': 'الخاتمة والدعوة إلى العمل', 'titleEn': 'Conclusion & Call to Action'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت من 15 شريحة: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والصور وغيّر النصوص والأرقام فقط؛ أرقام رسم الأعمدة في الشريحة العاشرة في كتلة JSON بالمعرّف chart-data (labels وpcts وbars).',
}
