# -*- coding: utf-8 -*-
# نموذج «ألتيوس للأعمال»: 13 شريحة (غلاف، نبذة، عن الشركة، التاريخ، الرسالة، الرؤية، الخدمات، الفريق،
# الأعمال، القيم، حقائق مالية، المبيعات والإيرادات برسم أعمدة، شكر) بهوية زرقاء وصور معمارية.
KEY = 'sl-altius-business'
TITLE = {'ar': 'ألتيوس — عرض أعمال', 'en': 'Altius Business Presentation'}
COMMENT = 'نموذج «ألتيوس للأعمال»: غلاف بشريط قطري أزرق ومصغّرتين لشريحتين، ثم شرائح نبذة/عن الشركة/التاريخ/الرسالة/الرؤية/الخدمات/الفريق/الأعمال/القيم/حقائق مالية/مبيعات برسم أعمدة، وشريحة شكر. الصور في assets، والرسم يُبنى من كتلة chart-data.'
VARS = '--ink: #222222; --ink-2: #3A3A3A; --navy: #152376; --blue: #2B48E6; --blue-2: #1B2FC9; --accent: #2B48E6; --gray: #F2F2F2; --muted: #6B6B6B; --line: #D9D9D9; --bg: #FFFFFF;'

STYLE = """  .pg { position: absolute; inset: 0; }
  p, h1, h2, h3 { margin: 0; }
  .abs { position: absolute; }
  /* عنوان الشريحة: جزء غامق + لاحقة «شريحة» خفيفة (تسبق العنوان في العربية) */
  .ttl { position: absolute; top: 40px; inset-inline-start: 64px; display: flex; gap: .28em; align-items: baseline; font-size: 44px; font-weight: 700; line-height: 1.15; color: var(--navy); white-space: nowrap; }
  .ttl .sfx { font-weight: 400; }
  html[dir="rtl"] .ttl .sfx { order: -1; }
  .ttl .hl { color: var(--blue); }
  .lead { position: absolute; font-size: 24px; font-weight: 700; line-height: 1.42; color: var(--navy); }
  .txt { position: absolute; font-size: 17px; line-height: 1.45; color: var(--ink-2); }
  .hr { position: absolute; height: 1px; background: var(--line); }
  .card { position: absolute; border-radius: 16px; background: var(--gray); color: var(--ink); }
  .card.blue { background: linear-gradient(135deg, #3556F5 0%, #1B2FC9 100%); color: #fff; }
  .card.blue h3, .card.blue p { color: #fff; }
  .ic-circle { flex: none; border-radius: 50%; background: linear-gradient(160deg, #3F5FF6, #1C31CF); color: #fff; display: grid; place-items: center; }
  .ic-circle svg { width: 50%; height: 50%; }
  .item { position: absolute; display: flex; gap: 24px; align-items: flex-start; }
  .item .ic-circle { width: 100px; height: 100px; font-size: 36px; font-weight: 700; }
  .item h3 { font-size: 22px; font-weight: 700; line-height: 1.3; color: var(--ink); margin-bottom: 6px; }
  .item p { font-size: 17px; line-height: 1.4; color: var(--ink-2); width: 175px; }
  /* الأشكال القطرية الزرقاء (الغلاف، عن الشركة، الشكر) — تنعكس للعربية */
  .diag, .diag2 { position: absolute; top: 0; inset-inline-start: 760px; width: 520px; height: 720px; }
  .diag { clip-path: polygon(63.5% 0, 100% 0, 100% 100%, 0 100%); background: linear-gradient(165deg, #3657F3 0%, #2238D8 45%, #0D1C8A 100%); }
  .diag2 { clip-path: polygon(100% 58%, 100% 100%, 66% 100%); background: linear-gradient(180deg, #152AB8, #0A1670); opacity: .75; }
  html[dir="rtl"] .diag, html[dir="rtl"] .diag2, html[dir="rtl"] .glass1, html[dir="rtl"] .glass, html[dir="rtl"] .glass13, html[dir="rtl"] .fillet { transform: scaleX(-1); }
  .dots { position: absolute; background-image: radial-gradient(circle, var(--dc) 2.3px, transparent 2.9px); background-size: 22px 22px; }
  .dots-w { --dc: #fff; top: 29px; inset-inline-end: 40px; width: 88px; height: 88px; }
  .dots-c { --dc: #5FD3E8; top: 424px; inset-inline-start: 624px; width: 112px; height: 134px; background-size: 22.5px 22.5px; }
  /* الغلاف وشريحة الشكر */
  .logo { position: absolute; top: 62px; inset-inline-start: 65px; display: flex; gap: 24px; align-items: center; }
  .logo svg { width: 58px; height: 58px; color: var(--blue); }
  .logo p { font-size: 18px; letter-spacing: 3px; line-height: 1.35; color: var(--navy); font-weight: 500; }
  .big { position: absolute; top: 264px; inset-inline-start: 62px; font-size: 146px; font-weight: 800; line-height: 1; letter-spacing: -2px; color: var(--blue); white-space: nowrap; }
  html[lang="ar"] .big { font-size: 118px; letter-spacing: 0; top: 250px; }
  .bars { position: absolute; top: 427px; inset-inline-start: 65px; display: flex; gap: 8px; }
  .bars i { display: block; height: 7px; border-radius: 4px; background: var(--blue); }
  .bars i:nth-child(1) { width: 66px; } .bars i:nth-child(2) { width: 20px; } .bars i:nth-child(3) { width: 218px; background: #B7C0F3; }
  .sub { position: absolute; top: 464px; inset-inline-start: 65px; font-size: 26px; letter-spacing: 1.2px; color: #555; white-space: nowrap; }
  html[lang="ar"] .sub { letter-spacing: 0; }
  .presenter { position: absolute; top: 614px; inset-inline-start: 65px; display: flex; gap: 24px; align-items: center; }
  .presenter .av { width: 50px; height: 50px; border-radius: 50%; background: #EEF0F5; color: #8A93C9; display: grid; place-items: center; }
  .presenter .av svg { width: 24px; height: 24px; }
  .presenter .pn { font-size: 22px; font-weight: 700; color: #111; }
  .presenter .pp { font-size: 18px; color: #666; margin-top: 2px; }
  .glass1 { position: absolute; top: 85px; inset-inline-start: 840px; width: 290px; height: 265px; clip-path: polygon(8% 0, 100% 0, 92% 100%, 0 100%); background: linear-gradient(180deg, #EEF1FC, #BFCBF8); }
  .mini { position: absolute; background: #fff; border-radius: 14px; padding: 6px; box-shadow: 0 14px 34px rgb(0 0 0 / .28); overflow: hidden; }
  .mini .pg { position: absolute; top: 6px; inset-inline-start: 6px; width: 1280px; height: 720px; background: #fff; transform-origin: top left; }
  html[dir="rtl"] .mini .pg { transform-origin: top right; }
  .mini .sfx { display: none; }
  .mini-team { top: 123px; inset-inline-start: 663px; width: 390px; height: 222px; }
  .mini-team .pg { transform: scale(.2953); }
  .mini-about { top: 360px; inset-inline-start: 740px; width: 460px; height: 260px; }
  .mini-about .pg { transform: scale(.35); }
  .thanks .big { top: 300px; font-size: 93px; letter-spacing: -1px; }
  html[lang="ar"] .thanks .big { font-size: 96px; letter-spacing: 0; top: 282px; }
  .thanks .sub { font-size: 27px; letter-spacing: 1.2px; }
  .thanks .dots-c { width: 135px; height: 134px; inset-inline-start: 626px; }
  .glass13 { position: absolute; top: 158px; inset-inline-start: 748px; width: 370px; height: 246px; }
  /* 2 — نبذة عن الشركة */
  .ov .band { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 240px; background: var(--blue); }
  .ov .ttl { color: #fff; }
  .ov .ov-lead { position: absolute; top: 123px; inset-inline-start: 64px; width: 560px; font-size: 20px; line-height: 1.4; color: #fff; }
  .ov .ring { position: absolute; top: -4px; inset-inline-start: 761px; width: 450px; height: 450px; border-radius: 50%; background: #1B35C9; }
  .fillet { position: absolute; top: 240px; width: 40px; height: 40px; }
  .fillet.fl { inset-inline-start: 720px; background: radial-gradient(circle at 0 100%, transparent 39px, var(--blue) 40px); }
  .fillet.fr { inset-inline-start: 1210px; background: radial-gradient(circle at 100% 100%, transparent 39px, var(--blue) 40px); }
  .ov .ov-photo { position: absolute; top: 92px; inset-inline-start: 837px; width: 296px; height: 296px; border-radius: 50%; object-fit: cover; }
  .ov .ov-name { position: absolute; top: 478px; inset-inline-start: 830px; width: 310px; text-align: center; font-size: 32px; font-weight: 700; color: var(--navy); }
  .ov .ov-role { position: absolute; top: 522px; inset-inline-start: 830px; width: 310px; text-align: center; font-size: 18px; color: #777; }
  .ov .ov-line { top: 557px; inset-inline-start: 830px; width: 310px; }
  .ov .ov-small { position: absolute; top: 578px; inset-inline-start: 870px; width: 230px; text-align: center; font-size: 17px; line-height: 1.5; color: #777; }
  .ov .quote { position: absolute; top: 352px; inset-inline-start: 65px; width: 86px; height: 70px; color: var(--blue); }
  .ov .vline { position: absolute; top: 330px; inset-inline-start: 193px; width: 1.5px; height: 270px; background: #CFCFCF; }
  .ov .q1, .ov .q2 { position: absolute; inset-inline-start: 228px; width: 460px; font-size: 18px; line-height: 1.4; color: var(--ink-2); }
  .ov .q1 { top: 330px; } .ov .q2 { top: 455px; }
  .ov .ov-ph { position: absolute; top: 572px; inset-inline-start: 228px; font-size: 18px; font-weight: 700; color: var(--navy); }
  /* 3 — عن الشركة */
  .about .diag { background: linear-gradient(165deg, #3C5CF4 0%, #2A48E4 55%, #1F3BD6 100%); }
  .about .diag2 { clip-path: polygon(80% 0, 100% 0, 100% 100%, 38% 100%); background: linear-gradient(170deg, #1D33C9, #0C1B86); opacity: 1; }
  .about .ab-p1 { top: 118px; inset-inline-start: 64px; width: 590px; }
  .about .ab-p2 { top: 216px; inset-inline-start: 64px; width: 590px; }
  .about .kpi { top: 308px; width: 210px; height: 157px; display: flex; flex-direction: column; align-items: center; justify-content: center; }
  .about .k1 { inset-inline-start: 65px; } .about .k2 { inset-inline-start: 295px; }
  .about .kpi .v { font-size: 44px; font-weight: 700; line-height: 1; letter-spacing: 1px; }
  .about .kpi .l { font-size: 18px; margin-top: 10px; }
  .about .i1 { top: 530px; inset-inline-start: 65px; } .about .i2 { top: 530px; inset-inline-start: 408px; }
  .about .city { position: absolute; top: 38px; inset-inline-start: 650px; width: 530px; height: 397px; object-fit: cover; clip-path: polygon(34% 0, 100% 0, 68% 100%, 0 100%); }
  html[dir="rtl"] .about .city { clip-path: polygon(0 0, 66% 0, 100% 100%, 32% 100%); }
  .glass { position: absolute; top: 330px; inset-inline-start: 780px; width: 295px; height: 185px; clip-path: polygon(23% 0, 100% 0, 77% 100%, 0 100%); background: linear-gradient(135deg, rgb(255 255 255 / .55), rgb(150 170 255 / .35)); }
  .about .dots-w { top: 565px; inset-inline-start: 905px; inset-inline-end: auto; }
  /* 4 — تاريخ الشركة */
  .hist .hist-photo { position: absolute; top: 0; inset-inline-start: 755px; width: 525px; height: 455px; object-fit: cover; border-end-start-radius: 24px; }
  .hist .h-p { top: 182px; inset-inline-start: 64px; width: 590px; }
  .hist .h-lead { top: 400px; inset-inline-start: 64px; width: 170px; font-size: 26px; line-height: 1.25; }
  .hc { width: 287px; height: 255px; padding: 30px 36px; }
  .hc .ic-circle { width: 54px; height: 54px; }
  .hc h3 { margin: 20px 0 8px; font-size: 22px; font-weight: 700; }
  .hc p { font-size: 17px; line-height: 1.4; width: 200px; color: var(--ink-2); }
  .hc.blue .ic-circle { background: #fff; color: var(--blue); }
  .hist .c0 { top: 118px; inset-inline-start: 690px; } .hist .c1 { top: 400px; inset-inline-start: 287px; }
  .hist .c2 { top: 400px; inset-inline-start: 608px; } .hist .c3 { top: 400px; inset-inline-start: 929px; }
  /* 5 — الرسالة */
  .mis .m-lead { top: 125px; inset-inline-start: 64px; width: 320px; }
  .mis .m-p { top: 318px; inset-inline-start: 64px; width: 320px; }
  .mc { width: 275px; height: 327px; padding: 38px 39px; }
  .mc .v { font-size: 44px; font-weight: 700; line-height: 1.1; color: var(--blue); letter-spacing: 1px; }
  .mc.blue .v { color: #fff; }
  .mc h3 { margin: 16px 0 8px; font-size: 20px; font-weight: 700; }
  .mc .t { font-size: 19px; line-height: 1.4; width: 200px; color: var(--ink-2); }
  .mis .mc1 { top: 65px; inset-inline-start: 444px; } .mis .mc2 { top: 213px; inset-inline-start: 763px; }
  .mis .m-photo1 { position: absolute; top: 65px; inset-inline-start: 763px; width: 390px; height: 222px; object-fit: cover; border-radius: 16px; }
  .mis .m-photo2 { position: absolute; top: 393px; inset-inline-start: 823px; width: 457px; height: 327px; object-fit: cover; }
  .mis .n1 { top: 530px; inset-inline-start: 65px; } .mis .n2 { top: 530px; inset-inline-start: 408px; }
  /* 6 — الرؤية */
  .vis .v-panel { position: absolute; top: 65px; inset-inline-start: 565px; width: 715px; height: 655px; background: var(--gray); border-start-start-radius: 24px; }
  .vis .v-lead { top: 180px; inset-inline-start: 64px; width: 215px; }
  .vis .v-p { top: 333px; inset-inline-start: 64px; width: 215px; }
  .vis .v-photo { position: absolute; top: 178px; inset-inline-start: 330px; width: 430px; height: 458px; object-fit: cover; border-radius: 18px; }
  .vc { position: absolute; inset-inline-start: 786px; width: 430px; height: 144px; border-radius: 16px; background: #fff; display: flex; align-items: center; padding: 0 36px; gap: 40px; color: var(--ink); }
  .vc.blue { background: linear-gradient(135deg, #3556F5 0%, #1B2FC9 100%); color: #fff; }
  .vc .vi { flex: none; width: 70px; height: 70px; color: var(--blue); }
  .vc.blue .vi { color: #fff; }
  .vc .vi svg { width: 70px; height: 70px; }
  .vc h3 { font-size: 22px; font-weight: 700; margin-bottom: 6px; }
  .vc p { font-size: 17px; line-height: 1.4; width: 240px; color: var(--ink-2); }
  .vc.blue p { color: #fff; }
  .vis .vc1 { top: 178px; } .vis .vc2 { top: 336px; } .vis .vc3 { top: 494px; }
  /* 7 — الخدمات */
  .svc .sv-band { position: absolute; top: 345px; inset-inline-start: 0; width: 1280px; height: 375px; background: #656BD3; }
  .svc .sv-photo { position: absolute; top: 541px; inset-inline-start: 0; width: 1280px; height: 179px; object-fit: cover; }
  html[dir="rtl"] .svc .sv-photo { transform: scaleX(-1); }
  .sc { width: 275px; height: 392px; padding: 40px 36px; }
  .sc .si { width: 82px; height: 72px; color: var(--navy); }
  .sc.blue .si { color: #fff; }
  .sc .si svg { width: 82px; height: 72px; }
  .sc h3 { margin: 34px 0 10px; font-size: 22px; font-weight: 700; }
  .sc p { font-size: 19px; line-height: 1.35; width: 200px; color: var(--ink-2); }
  .svc .s0 { top: 148px; inset-inline-start: 65px; } .svc .s1 { top: 148px; inset-inline-start: 357px; }
  .svc .s2 { top: 148px; inset-inline-start: 649px; } .svc .s3 { top: 148px; inset-inline-start: 941px; }
  .svc .sv-foot { position: absolute; top: 596px; inset-inline-start: 240px; width: 800px; text-align: center; font-size: 17px; line-height: 1.5; color: #fff; }
  /* 8 — الفريق */
  .team .mem { position: absolute; top: 155px; width: 263px; text-align: center; }
  .team .mem img { display: block; width: 263px; height: 321px; object-fit: cover; }
  .team .mem .des { margin-top: 26px; font-size: 21px; color: #666; }
  .team .mem .nm { margin-top: 8px; font-size: 28px; font-weight: 700; color: var(--navy); }
  .team .mem .sm { margin: 12px auto 0; width: 235px; font-size: 17.5px; line-height: 1.4; color: var(--ink-2); }
  .team .m1 { inset-inline-start: 65px; } .team .m2 { inset-inline-start: 358px; } .team .m3 { inset-inline-start: 652px; } .team .m4 { inset-inline-start: 945px; }
  /* 9 — الأعمال */
  .pf .pf-lead { top: 418px; inset-inline-start: 64px; width: 280px; }
  .pf .pf-p { top: 527px; inset-inline-start: 64px; width: 350px; }
  .pf img { position: absolute; object-fit: cover; border-radius: 12px; }
  .pf .pa { top: 127px; inset-inline-start: 492px; width: 170px; height: 158px; }
  .pf .pb { top: 127px; inset-inline-start: 674px; width: 326px; height: 328px; }
  .pf .pc { top: 238px; inset-inline-start: 1013px; width: 203px; height: 343px; }
  .pf .pd { top: 466px; inset-inline-start: 475px; width: 398px; height: 182px; }
  .pf .pf-ic { position: absolute; top: 466px; inset-inline-start: 884px; width: 116px; height: 116px; border-radius: 12px; background: var(--blue); color: #fff; display: grid; place-items: center; }
  .pf .pf-ic svg { width: 56px; height: 56px; }
  /* 10 — القيم */
  .cv .cv-lead { top: 128px; inset-inline-start: 64px; width: 275px; }
  .cv .cv-h1 { top: 258px; inset-inline-start: 64px; width: 272px; }
  .cv .cv-big { position: absolute; top: 268px; inset-inline-start: 64px; font-size: 56px; font-weight: 700; line-height: 1.2; color: var(--blue); letter-spacing: 1px; }
  .cv .cv-h2 { top: 350px; inset-inline-start: 64px; width: 272px; }
  .cv .cv-p1 { top: 386px; inset-inline-start: 64px; width: 272px; }
  .cv .cv-p2 { top: 510px; inset-inline-start: 64px; width: 272px; }
  .cv .cv-photo { position: absolute; top: 113px; inset-inline-start: 368px; width: 435px; height: 607px; object-fit: cover; border-start-start-radius: 18px; border-start-end-radius: 18px; }
  .cv-item { position: absolute; inset-inline-start: 830px; display: flex; gap: 42px; align-items: flex-start; }
  .cv-item .ic-circle { width: 85px; height: 85px; }
  .cv-item .tx { width: 262px; margin-top: -6px; }
  .cv-item h3 { font-size: 22px; font-weight: 700; margin-bottom: 8px; color: var(--ink); }
  .cv-item p { font-size: 17px; line-height: 1.4; color: var(--ink-2); }
  .cv .r1 { top: 143px; } .cv .r2 { top: 314px; } .cv .r3 { top: 485px; }
  .cv .cv-s { inset-inline-start: 957px; width: 262px; } .cv .s1 { top: 283px; } .cv .s2 { top: 455px; } .cv .s3 { top: 626px; }
  /* 11 — حقائق مالية */
  .fin .fn-photo { position: absolute; top: 130px; inset-inline-start: 0; width: 435px; height: 590px; object-fit: cover; border-start-end-radius: 20px; }
  .fin .fn-lead { top: 128px; inset-inline-start: 489px; width: 275px; }
  .fin .fn-h1 { top: 243px; inset-inline-start: 489px; width: 272px; }
  .fin .fn-p1 { top: 268px; inset-inline-start: 489px; width: 272px; }
  .fin .fn-p2 { top: 393px; inset-inline-start: 489px; width: 272px; }
  .fin .fn-lead2 { top: 135px; inset-inline-start: 820px; width: 260px; }
  .fin .fn-h2 { top: 246px; inset-inline-start: 820px; width: 398px; }
  .fc { width: 186px; height: 180px; padding: 34px 28px; }
  .fc .v { font-size: 36px; font-weight: 700; line-height: 1.1; color: var(--blue); letter-spacing: 1px; }
  .fc.blue .v { color: #fff; }
  .fc .t { margin-top: 14px; font-size: 14px; line-height: 1.4; color: var(--ink-2); }
  .fin .f1 { top: 277px; inset-inline-start: 820px; } .fin .f2 { top: 277px; inset-inline-start: 1028px; }
  .fin .f3 { top: 470px; inset-inline-start: 820px; } .fin .f4 { top: 470px; inset-inline-start: 1028px; }
  /* 12 — المبيعات والإيرادات */
  .sr .sr-lead { top: 155px; inset-inline-start: 64px; width: 310px; }
  .kp { position: absolute; width: 165px; }
  .kp .v { font-size: 34px; font-weight: 700; line-height: 1.15; color: var(--blue); letter-spacing: 1px; }
  .kp .n { margin-top: 8px; font-size: 18px; font-weight: 700; color: var(--ink); }
  .kp .t { margin-top: 6px; width: 140px; font-size: 14px; line-height: 1.35; color: var(--ink-2); }
  .sr .kp1 { top: 372px; inset-inline-start: 64px; } .sr .kp2 { top: 372px; inset-inline-start: 237px; } .sr .kp3 { top: 517px; inset-inline-start: 64px; }
  .sr .sr-photo { position: absolute; top: 155px; inset-inline-start: 430px; width: 285px; height: 497px; object-fit: cover; border-radius: 16px; }
  .sr .sr-p { top: 155px; inset-inline-start: 763px; width: 455px; }
  .sr .chart { position: absolute; top: 270px; inset-inline-start: 763px; width: 455px; height: 400px; }
  .sr .chart text { font-family: var(--font); font-size: 15px; fill: #555; }"""

ICON = ('<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="%s" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">%s</svg>')
ICONS = {
    'diamond': ICON % ('1.4', '<path d="M3 9.5 7 4h10l4 5.5L12 20z"/><path d="M3 9.5h18M9 9.5l3 10.5 3-10.5M7 4l2 5.5M17 4l-2 5.5"/>'),
    'building': ICON % ('1.4', '<rect x="6" y="3" width="12" height="18" rx="1"/><path d="M9 7h2M13 7h2M9 11h2M13 11h2M9 15h2M13 15h2M10.5 21v-3h3v3"/>'),
    'bulb': ICON % ('1.4', '<path d="M9.5 18h5M10.5 21h3M12 3a6 6 0 0 0-3.4 10.9c.6.5.9 1.3.9 2.1h5c0-.8.3-1.6.9-2.1A6 6 0 0 0 12 3z"/>'),
    'briefcase': ICON % ('1.4', '<rect x="3" y="7" width="18" height="13" rx="2"/><path d="M9 7V5a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2M3 12h18M12 11v3"/>'),
    'chart': ICON % ('1.4', '<path d="M3 21h18M6 18v-6M11 18V6M16 18v-9M20 18V3"/>'),
    'group': ICON % ('1.3', '<circle cx="12" cy="7" r="3"/><circle cx="5" cy="9" r="2"/><circle cx="19" cy="9" r="2"/><path d="M7 19v-2a5 5 0 0 1 10 0v2M1.5 17v-1.5a3.5 3.5 0 0 1 4-3.4M22.5 17v-1.5a3.5 3.5 0 0 0-4-3.4"/>'),
    'bank': ICON % ('1.1', '<path d="M3 9l9-5 9 5H3zM5 9v8M9.5 9v8M14.5 9v8M19 9v8M3 17h18M3 20h18"/>'),
    'signpost': ICON % ('1.1', '<path d="M12 3v18M9 21h6M12 6h7l2 2-2 2h-7M12 13H5l-2 2 2 2h7"/>'),
    'blueprint': ICON % ('1', '<path d="M5 4h12v14H5zM5 4a2 2 0 1 0 0 4M8 8h4v4H8zM8 14h5"/><path d="M15 12l4-4 2 2-4 4h-2z"/>'),
    'city': ICON % ('1', '<path d="M3 21V9h6v12M9 21V5h7v16M16 21v-9h5v9M3 21h18M5 12h2M5 15h2M5 18h2M11 8h3M11 11h3M11 14h3M11 17h3M18 15h1M18 18h1"/>'),
    'crane': ICON % ('1', '<path d="M3 21h8M7 21V7M4 7h16l-1 3h-2l-1-3M7 4v3M18 7v6M16 13h4v3h-4zM7 10l-3-3M7 13l-3-3M7 16l-3-3M7 19l-3-3"/>'),
    'mixer': ICON % ('1', '<path d="M2 16V9l5-4h6l3 5v6H2z"/><path d="M16 11h4l2 3v2h-6zM7 5l6 6M2 12h5"/><circle cx="6" cy="18" r="2"/><circle cx="18" cy="18" r="2"/>'),
    'person': ICON % ('1.6', '<circle cx="12" cy="8" r="4"/><path d="M4 21a8 8 0 0 1 16 0"/>'),
    'logo': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3.2" stroke-linejoin="round" aria-hidden="true"><path d="M5.5 4.5c4-1.4 9-1.4 13 0 1.4 4 1.4 9.5 0 14-4 1.4-9 1.4-13 0-1.4-4.5-1.4-10 0-14z" transform="rotate(-10 12 12)"/></svg>',
    'quote': '<svg viewBox="0 0 90 70" fill="currentColor" aria-hidden="true"><path d="M0 36v-16A20 20 0 0 1 20 0h8v12h-8a8 8 0 0 0-8 8v6h18v34a10 10 0 0 1-10 10H10A10 10 0 0 1 0 60z"/><path d="M50 36v-16A20 20 0 0 1 70 0h8v12h-8a8 8 0 0 0-8 8v6h18v34a10 10 0 0 1-10 10H60a10 10 0 0 1-10-10z"/></svg>',
}

GLOBAL_DEFS = ('<defs><linearGradient id="gl" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#BCC6F4" stop-opacity=".9"/><stop offset="1" stop-color="#E2E7FF" stop-opacity=".35"/></linearGradient></defs>')

A = '/templates/freeform/' + KEY + '/assets/'


def img(name, cls, alt):
    return '<img class="%s" src="%s%s.jpg" alt="%s">' % (cls, A, name, alt)


def item(cls, icon_html, h, p):
    return '<div class="item %s"><div class="ic-circle">%s</div><div><h3>§%s§</h3><p>§%s§</p></div></div>' % (cls, icon_html, h, p)


def ttl(key, sfx=True):
    return '<h1 class="ttl"><span>§%s§</span>%s</h1>' % (key, '<span class="sfx">§SLIDE§</span>' if sfx else '')


def cover_common(big_cls=''):
    return ('<div class="diag"></div><div class="diag2"></div><div class="dots dots-w"></div><div class="dots dots-c"></div>'
            '<div class="logo">%s<p>§LOGO§</p></div>' % ICONS['logo']
            + '<div class="bars"><i></i><i></i><i></i></div>'
            + '<div class="presenter"><div class="av">%s</div><div><p class="pn">§PRES§</p><p class="pp">§PH§</p></div></div>' % ICONS['person'])


# 3 — عن الشركة (تُستخدم أيضًا مصغّرةً في الغلاف)
ABOUT = ('<div class="pg about"><div class="diag"></div><div class="diag2"></div>'
         + img('city', 'city', '§ALT_CITY§') + '<div class="glass"></div><div class="dots dots-w"></div>'
         + '<h1 class="ttl"><span>§T3A§</span><span class="hl">§T3B§</span></h1>'
         + '<p class="txt ab-p1">§P3§</p><p class="txt ab-p2">§P2§</p>'
         + '<div class="card blue kpi k1"><p class="v">33%</p><p class="l">§K1L§</p></div>'
         + '<div class="card blue kpi k2"><p class="v">$165K</p><p class="l">§K2L§</p></div>'
         + item('i1', ICONS['diamond'], 'PH', 'S1') + item('i2', ICONS['building'], 'PH', 'S1') + '</div>')

# 8 — الفريق (تُستخدم أيضًا مصغّرةً في الغلاف)
TEAM = ('<div class="pg team">' + ttl('T8')
        + ''.join('<div class="mem m%d">%s<p class="des">§DES§</p><p class="nm">§NAME§</p><p class="sm">§S1§</p></div>' % (i, img('p%d' % i, '', '§ALT_P%d§' % i)) for i in (1, 2, 3, 4))
        + '</div>')

SLIDES = [
    # 1 — الغلاف
    ('<div class="pg cover">' + cover_common() + '<div class="glass1"></div>'
     + '<h1 class="big">§BIG§</h1><p class="sub">§SUB§</p>'
     + '<div class="mini mini-team" aria-hidden="true">' + TEAM + '</div>'
     + '<div class="mini mini-about" aria-hidden="true">' + ABOUT + '</div></div>'),
    # 2 — نبذة عن الشركة
    ('<div class="pg ov"><div class="band"></div><div class="ring"></div><div class="fillet fl"></div><div class="fillet fr"></div>'
     + ttl('T2', False) + '<p class="ov-lead">§P2§</p>'
     + img('founder', 'ov-photo', '§ALT_F§')
     + '<p class="ov-name">§NAME§</p><p class="ov-role">§ROLE§</p><div class="hr ov-line"></div><p class="ov-small">§S1§</p>'
     + '<div class="quote">' + ICONS['quote'] + '</div><div class="vline"></div>'
     + '<p class="q1">§P4§</p><p class="q2">§P2§</p><p class="ov-ph">§PH§</p></div>'),
    # 3 — عن الشركة
    ABOUT,
    # 4 — تاريخ الشركة
    ('<div class="pg hist">' + img('hist', 'hist-photo', '§ALT_B1§') + ttl('T4')
     + '<p class="txt h-p">§P3§</p><p class="lead h-lead">§S1§</p>'
     + ''.join('<div class="card hc %s"><div class="ic-circle">%s</div><h3>§PH§</h3><p>§S1§</p></div>' % (c, ICONS[ic]) for c, ic in (('c0', 'bulb'), ('c1', 'briefcase'), ('c2 blue', 'chart'), ('c3', 'group')))
     + '</div>'),
    # 5 — الرسالة
    ('<div class="pg mis">' + ttl('T5', False) + '<p class="lead m-lead">§P2§</p><p class="txt m-p">§P2B§</p>'
     + '<div class="card mc mc1"><p class="v">$75M</p><h3>§PH§</h3><p class="t">§S1§</p></div>'
     + img('tower', 'm-photo1', '§ALT_B2§') + img('gray', 'm-photo2', '§ALT_B3§')
     + '<div class="card mc mc2 blue"><p class="v">60K</p><h3>§PH§</h3><p class="t">§S1§</p></div>'
     + item('n1', '01', 'PH', 'S1') + item('n2', '02', 'PH', 'S1') + '</div>'),
    # 6 — الرؤية
    ('<div class="pg vis"><div class="v-panel"></div>' + ttl('T6') + '<p class="lead v-lead">§S1§</p><p class="txt v-p">§P2§</p>'
     + img('towers', 'v-photo', '§ALT_B4§')
     + ''.join('<div class="vc %s"><div class="vi">%s</div><div><h3>§PH§</h3><p>§S1§</p></div></div>' % (c, ICONS[ic]) for c, ic in (('vc1 blue', 'bank'), ('vc2', 'group'), ('vc3', 'signpost')))
     + '</div>'),
    # 7 — الخدمات
    ('<div class="pg svc"><div class="sv-band"></div>' + img('band', 'sv-photo', '')  + ttl('T7')
     + ''.join('<div class="card sc %s"><div class="si">%s</div><h3>§PH§</h3><p>§S1§</p></div>' % (c, ICONS[ic]) for c, ic in (('s0', 'blueprint'), ('s1 blue', 'city'), ('s2', 'crane'), ('s3', 'mixer')))
     + '<p class="sv-foot">§P5§</p></div>'),
    # 8 — الفريق
    TEAM,
    # 9 — الأعمال
    ('<div class="pg pf">' + ttl('T9') + '<p class="lead pf-lead">§S1§</p><p class="txt pf-p">§P2§</p>'
     + img('pf-a', 'pa', '§ALT_B4§') + img('pf-b', 'pb', '§ALT_B2§') + img('pf-c', 'pc', '§ALT_B5§') + img('pf-d', 'pd', '§ALT_B5§')
     + '<div class="pf-ic">' + ICONS['city'] + '</div></div>'),
    # 10 — القيم
    ('<div class="pg cv">' + ttl('T10') + '<p class="lead cv-lead">§S1§</p><div class="hr cv-h1"></div><p class="cv-big">$102+</p><div class="hr cv-h2"></div>'
     + '<p class="txt cv-p1">§P2§</p><p class="txt cv-p2">§S1§</p>' + img('corner', 'cv-photo', '§ALT_B5§')
     + ''.join('<div class="cv-item %s"><div class="ic-circle">%s</div><div class="tx"><h3>§PH§</h3><p>§S1§</p></div></div><div class="hr cv-s %s"></div>' % (r, ICONS[ic], s) for r, s, ic in (('r1', 's1', 'diamond'), ('r2', 's2', 'bank'), ('r3', 's3', 'building')))
     + '</div>'),
    # 11 — حقائق مالية
    ('<div class="pg fin">' + ttl('T11') + img('dark', 'fn-photo', '§ALT_B6§')
     + '<p class="lead fn-lead">§S1§</p><div class="hr fn-h1"></div><p class="txt fn-p1">§P2§</p><p class="txt fn-p2">§S1§</p>'
     + '<p class="lead fn-lead2">§INS§</p><div class="hr fn-h2"></div>'
     + ''.join('<div class="card fc %s"><p class="v">%s</p><p class="t">§INS§</p></div>' % (c, v) for c, v in (('f1', '+38%'), ('f2', '$68K'), ('f3', '+22%'), ('f4 blue', '+1.5M')))
     + '</div>'),
    # 12 — المبيعات والإيرادات
    ('<div class="pg sr">' + ttl('T12') + '<p class="lead sr-lead">§P2§</p>'
     + '<div class="kp kp1"><p class="v">$25M</p><p class="n">§RN§</p><p class="t">§INS§</p></div>'
     + '<div class="kp kp2"><p class="v">$15M</p><p class="n">§EN§</p><p class="t">§INS§</p></div>'
     + '<div class="kp kp3"><p class="v">15.1K</p><p class="n">§CN§</p><p class="t">§INS§</p></div>'
     + img('white', 'sr-photo', '§ALT_B7§') + '<p class="txt sr-p">§P2§</p>'
     + '<svg class="chart" id="chart" viewBox="0 0 455 400" role="img" aria-label="§CHART_ALT§"></svg></div>'),
    # 13 — الشكر
    ('<div class="pg cover thanks">' + cover_common()
     + '<svg class="glass13" viewBox="0 0 370 246" aria-hidden="true"><polygon points="103,3 366,3 266,243 3,243" fill="url(#gl)" stroke="#fff" stroke-width="2.5" stroke-linejoin="round"/></svg>'
     + '<h1 class="big">§THANKS§</h1><p class="sub">§EDIT§</p></div>'),
]

S_AR = 'هذا نص توضيحي. أضف النص المطلوب هنا.'
S_EN = 'This is a sample text. Insert your desired text here.'
STR = {
    'ar': {
        'SLIDE': 'شريحة', 'PH': 'عنوان نائب', 'S1': S_AR, 'P2': S_AR + ' ' + S_AR, 'P2B': 'أضف النص المطلوب هنا. ' + S_AR + ' أضف النص المطلوب هنا.',
        'P3': S_AR + ' ' + S_AR + ' ' + S_AR + ' هذا نص توضيحي.', 'P4': S_AR + ' ' + S_AR + ' ' + S_AR + ' هذا نص توضيحي.',
        'P5': S_AR + ' ' + S_AR + ' ' + S_AR + ' هذا نص توضيحي.', 'INS': 'أضف النص المطلوب هنا.',
        'LOGO': 'ضع شعار<br>شركتك هنا', 'BIG': 'ألتيوس', 'SUB': 'قالب عرض تقديمي للأعمال', 'PRES': 'اسم مقدّم العرض',
        'T2': 'نبذة عن الشركة', 'NAME': 'الاسم', 'ROLE': 'الشركاء المؤسسون',
        'T3A': 'نبذة عن', 'T3B': 'شركتنا', 'K1L': 'نمو المستخدمين', 'K2L': 'الإيرادات',
        'T4': 'تاريخ شركتنا', 'T5': 'رسالتنا', 'T6': 'رؤيتنا', 'T7': 'خدماتنا', 'T8': 'تعرّف على فريقنا', 'DES': 'المسمّى الوظيفي',
        'T9': 'معرض الأعمال', 'T10': 'القيم الأساسية', 'T11': 'حقائق مالية', 'T12': 'المبيعات والإيرادات',
        'RN': 'إجمالي الإيرادات', 'EN': 'إجمالي المصروفات', 'CN': 'إجمالي العملاء',
        'THANKS': 'شكرًا لكم', 'EDIT': 'عدّل نصك هنا',
        'CHART_ALT': 'رسم أعمدة للإيرادات والمصروفات من 2022 إلى 2025',
        'ALT_CITY': 'منظر جوي لمدينة ليلًا', 'ALT_F': 'صورة الشريك المؤسس', 'ALT_B1': 'واجهة مبنى زجاجي حديث', 'ALT_B2': 'برج بواجهة بيضاء وزرقاء',
        'ALT_B3': 'مبنى حديث بتدرّج رمادي', 'ALT_B4': 'أبراج زجاجية من الأسفل', 'ALT_B5': 'زاوية مبنى حديث تحت سماء صافية', 'ALT_B6': 'برج زجاجي داكن من الأسفل', 'ALT_B7': 'برج أبيض بنوافذ متكررة',
        'ALT_P1': 'عضو الفريق الأول', 'ALT_P2': 'عضو الفريق الثاني', 'ALT_P3': 'عضو الفريق الثالث', 'ALT_P4': 'عضو الفريق الرابع',
    },
    'en': {
        'SLIDE': 'Slide', 'PH': 'Placeholder', 'S1': S_EN, 'P2': S_EN + ' ' + S_EN, 'P2B': 'Insert your desired text here. ' + S_EN + ' Insert your desired text here.',
        'P3': S_EN + ' ' + S_EN + ' ' + S_EN + ' This is a sample text.', 'P4': S_EN + ' ' + S_EN + ' ' + S_EN + ' This is a sample text.',
        'P5': S_EN + ' ' + S_EN + ' ' + S_EN + ' This is a sample text.', 'INS': 'Insert your desired text here.',
        'LOGO': 'YOUR COMPANY<br>LOGO HERE', 'BIG': 'ALTIUS', 'SUB': 'BUSINESS PRESENTATION TEMPLATE', 'PRES': 'Presenter Name',
        'T2': 'Company Overview', 'NAME': 'Name', 'ROLE': 'Co-Founders',
        'T3A': 'About Our', 'T3B': 'Company', 'K1L': 'User Growth', 'K2L': 'Revenue',
        'T4': 'Our Company History', 'T5': 'Our Mission', 'T6': 'Our Vision', 'T7': 'Our Service', 'T8': 'Meet Our Team', 'DES': 'Designation',
        'T9': 'Portfolio', 'T10': 'Core Values', 'T11': 'Financial Facts', 'T12': 'Sales & Revenue',
        'RN': 'Total Revenue', 'EN': 'Total Expense', 'CN': 'Total Customers',
        'THANKS': 'Thank You', 'EDIT': 'Edit your text here',
        'CHART_ALT': 'Bar chart of revenue and expense from 2022 to 2025',
        'ALT_CITY': 'Aerial city view at night', 'ALT_F': 'Co-founder portrait', 'ALT_B1': 'Modern glass building facade', 'ALT_B2': 'Tower with white and blue facade',
        'ALT_B3': 'Modern building in grayscale', 'ALT_B4': 'Glass towers from below', 'ALT_B5': 'Corner of a modern building under a clear sky', 'ALT_B6': 'Dark glass tower from below', 'ALT_B7': 'White tower with repeating windows',
        'ALT_P1': 'Team member one', 'ALT_P2': 'Team member two', 'ALT_P3': 'Team member three', 'ALT_P4': 'Team member four',
    },
}

JSON_BLOCKS = {'chart-data': {
    'ar': {'categories': ['2022', '2023', '2024', '2025'], 'max': 50, 'step': 5,
           'series': [{'name': 'الإيرادات', 'color': '#2B45E0', 'values': [34, 42, 46, 45]}, {'name': 'المصروفات', 'color': '#1A2A6C', 'values': [23, 17, 27, 26]}]},
    'en': {'categories': ['2022', '2023', '2024', '2025'], 'max': 50, 'step': 5,
           'series': [{'name': 'Revenue', 'color': '#2B45E0', 'values': [34, 42, 46, 45]}, {'name': 'Expense', 'color': '#1A2A6C', 'values': [23, 17, 27, 26]}]},
}}

EXTRA_SCRIPT = """  // رسم الأعمدة المجمّعة في شريحة «المبيعات والإيرادات» من كتلة chart-data؛ الفئة الأولى في جهة البداية
  function drawChart() {
    var svg = document.getElementById('chart'), blk = document.getElementById('chart-data'); if (!svg || !blk) return;
    var d = JSON.parse(blk.textContent), W = 455, top = 17, base = 360, axis = 38, bw = 26;
    var ns = 'http://www.w3.org/2000/svg';
    function mk(t, a, txt) { var e = document.createElementNS(ns, t); for (var k in a) e.setAttribute(k, a[k]); if (txt != null) e.textContent = txt; svg.appendChild(e); return e; }
    function X(x) { return RTL ? W - x : x; }
    for (var v = 0; v <= d.max; v += d.step) {
      var y = base - v / d.max * (base - top);
      mk('text', { x: X(axis - 14), y: y + 5, 'text-anchor': RTL ? 'start' : 'end' }, String(v));
    }
    mk('line', { x1: X(axis), x2: X(W), y1: base + .5, y2: base + .5, stroke: '#D9D9D9', 'stroke-width': 1 });
    var n = d.categories.length, gw = (W - axis) / n;
    d.categories.forEach(function (c, i) {
      var cx = axis + gw * i + gw / 2;
      d.series.forEach(function (s, j) {
        var h = s.values[i] / d.max * (base - top), x0 = cx + (j - 1) * bw, x1 = x0 + bw;
        mk('rect', { x: Math.min(X(x0), X(x1)), y: base - h, width: bw, height: h, fill: s.color });
      });
      mk('text', { x: X(cx), y: base + 24, 'text-anchor': 'middle' }, c);
    });
  }
  drawChart();"""

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'general', 'icon': '🏙️',
    'name': 'ألتيوس — عرض أعمال', 'nameEn': 'Altius Business Presentation',
    'description': '13 شريحة: غلاف، نبذة، عن الشركة، التاريخ، الرسالة، الرؤية، الخدمات، الفريق، الأعمال، القيم، مالية، مبيعات، شكر',
    'descriptionEn': '13 slides: cover, overview, about, history, mission, vision, services, team, portfolio, values, finance, sales, thanks',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'نبذة عن الشركة', 'titleEn': 'Company Overview'},
        {'title': 'عن شركتنا (مؤشران)', 'titleEn': 'About Our Company (2 KPIs)', 'needsData': True},
        {'title': 'تاريخ الشركة', 'titleEn': 'Company History'},
        {'title': 'الرسالة (مؤشران)', 'titleEn': 'Mission (2 KPIs)', 'needsData': True},
        {'title': 'الرؤية', 'titleEn': 'Vision'},
        {'title': 'الخدمات', 'titleEn': 'Services'},
        {'title': 'الفريق', 'titleEn': 'Team'},
        {'title': 'معرض الأعمال', 'titleEn': 'Portfolio'},
        {'title': 'القيم الأساسية (رقم)', 'titleEn': 'Core Values (figure)', 'needsData': True},
        {'title': 'حقائق مالية (4 مؤشرات)', 'titleEn': 'Financial Facts (4 KPIs)', 'needsData': True},
        {'title': 'المبيعات والإيرادات (رسم)', 'titleEn': 'Sales & Revenue (chart)', 'needsData': True},
        {'title': 'شكرًا', 'titleEn': 'Thank You'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والصور وغيّر النصوص والأرقام فقط؛ أرقام رسم الأعمدة في كتلة JSON بالمعرّف chart-data، والغلاف يضم مصغّرتين لشريحتي «الفريق» و«عن الشركة» تتبعان نصوصهما.',
}
