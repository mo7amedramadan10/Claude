# -*- coding: utf-8 -*-
# نموذج «الإسناد الخارجي لعمليات الأعمال»: خمس عشرة شريحة بخلفية بيضاء وزخرفة أشرطة زرقاء مائلة متدرّجة،
# بألوان أزرق فاتح ونيلي وأزرق فولاذي وأخضر. كل الرسوم أشكال CSS/SVG؛ لا صور فوتوغرافية في الأصل.
KEY = 'sl-business-process-outsourcing'
TITLE = {'ar': 'الإسناد الخارجي لعمليات الأعمال', 'en': 'Business Process Outsourcing'}
COMMENT = 'نموذج «الإسناد الخارجي لعمليات الأعمال»: غلاف، جدول أعمال، أرقام رئيسية، دوافع، نطاق العمليات، الوضع الحالي والمستقبلي، تدفق العمليات، نموذج التشغيل، تقديم الخدمات، الحوكمة والمؤشرات، روافع القيمة، مصفوفة المخاطر، خارطة الطريق، الخطوات التالية، والختام. الرسوم الزخرفية SVG تُعكس في العربية بالصنف mir.'
VARS = ('--ink: #111317; --ink-2: #2B2F36; --accent: #69AEE9; --light: #69AEE9; --indigo: #3340AE; --steel: #548CBB; '
        '--green: #5EAB8B; --gray: #C8C8C8; --line: #D3E5F6; --bg: #FFFFFF; --to-end: to right; --diag: 135deg;')

STYLE = """  html[dir="rtl"] { --to-end: to left; --diag: 225deg; }
  /* الزخرفة والرسوم ذات الاتجاه تُعكس في العربية */
  html[dir="rtl"] .mir { transform: scaleX(-1); }
  .full { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; pointer-events: none; }
  .fade { -webkit-mask-image: linear-gradient(to bottom, #000 45%, transparent 100%); mask-image: linear-gradient(to bottom, #000 45%, transparent 100%); }
  .abs { position: absolute; margin: 0; }
  .ctr { display: flex; align-items: center; justify-content: center; text-align: center; }
  /* الترويسة المشتركة */
  .biz { position: absolute; top: 73px; inset-inline-start: 42px; margin: 0; font-size: 17px; line-height: 1.2; letter-spacing: 4px; color: var(--ink); font-weight: 400; white-space: nowrap; }
  .ttl { position: absolute; top: 113px; inset-inline-start: 36px; margin: 0; font-size: 45px; line-height: 1.15; font-weight: 300; color: var(--light); white-space: nowrap; letter-spacing: -0.5px; }
  .ttl b { font-weight: 700; }
  html[lang="ar"] .biz { letter-spacing: 0; font-size: 19px; font-weight: 500; }
  html[lang="ar"] .ttl { font-weight: 400; letter-spacing: 0; }
  html[lang="ar"] .ttl b { font-weight: 800; }
  .deco { position: absolute; }
  .deco-s { top: 550px; inset-inline-start: 0; width: 230px; height: 170px; }
  .intro { position: absolute; top: 221px; inset-inline-start: 37px; width: 450px; margin: 0; font-size: 22px; line-height: 29px; color: var(--ink); }
  .sub { position: absolute; top: 187px; inset-inline-start: 36px; width: 480px; margin: 0; font-size: 20px; line-height: 25px; color: var(--ink); }
  /* عنصر مرقّم: رقم كبير وعنوان ونص */
  .ni { position: absolute; inset-inline-start: 38px; width: 330px; }
  .ni .n { position: absolute; top: 0; inset-inline-start: 0; margin: 0; font-size: 36px; line-height: 1; font-weight: 300; color: var(--steel); }
  .ni h3 { position: absolute; top: 4px; inset-inline-start: 57px; margin: 0; font-size: 22px; line-height: 1.2; font-weight: 700; color: var(--ink); white-space: nowrap; }
  .ni p { position: absolute; top: 45px; inset-inline-start: 57px; width: 260px; margin: 0; font-size: 17.5px; line-height: 23px; color: var(--ink); }
  .ni1 { top: 360px; } .ni2 { top: 500px; }
  /* عبارة رئيسية بشريط جانبي */
  .stmt-bar { position: absolute; inset-inline-start: 35px; width: 14px; height: 75px; background: var(--light); }
  .stmt { position: absolute; inset-inline-start: 78px; width: 460px; margin: 0; font-size: 24px; line-height: 32px; font-weight: 700; color: var(--ink); }
  /* حبة بإطار متدرّج */
  .gpill { position: absolute; border: 3px solid transparent; border-radius: 26px; background: linear-gradient(#fff, #fff) padding-box, linear-gradient(var(--to-end), var(--light), var(--indigo)) border-box; font-size: 19.5px; font-weight: 700; color: var(--ink); }
  .icon svg { width: 100%; height: 100%; display: block; }

  /* 1 الغلاف */
  .s1 .tl { position: absolute; top: 0; inset-inline-start: 0; width: 260px; height: 125px; }
  .s1 .deco1 { top: 20px; inset-inline-end: 0; width: 945px; height: 700px; transform: scaleX(-1); }
  html[dir="rtl"] .s1 .deco1 { transform: none; }
  .logo { position: absolute; width: 58px; height: 64px; color: var(--light); }
  .s1 .logo { top: 119px; inset-inline-start: 46px; }
  .lgt { position: absolute; margin: 0; font-size: 15px; line-height: 19px; letter-spacing: 3px; width: 175px; }
  html[lang="ar"] .lgt { letter-spacing: 0; font-size: 17px; }
  .s1 .lgt { top: 132px; inset-inline-start: 137px; color: var(--light); }
  .s1 .c1 { top: 258px; inset-inline-start: 41px; font-size: 66px; line-height: 1; font-weight: 700; color: var(--indigo); white-space: nowrap; letter-spacing: -0.3px; }
  .s1 .c2 { top: 342px; inset-inline-start: 40px; font-size: 66px; line-height: 1; font-weight: 300; color: var(--indigo); white-space: nowrap; }
  html[lang="ar"] .s1 .c1 { font-weight: 800; top: 240px; } html[lang="ar"] .s1 .c2 { font-weight: 400; top: 326px; }
  .s1 .c3 { top: 436px; inset-inline-start: 42px; font-size: 28.5px; line-height: 1.2; letter-spacing: 3px; color: var(--ink); white-space: nowrap; }
  html[lang="ar"] .s1 .c3 { letter-spacing: 0; font-size: 30px; }
  .s1 .lb { top: 552px; font-size: 19px; line-height: 1.2; font-weight: 700; color: var(--ink); white-space: nowrap; }
  .s1 .vl { top: 593px; font-size: 17.5px; line-height: 1.3; color: var(--ink); white-space: nowrap; }
  .s1 .k1 { inset-inline-start: 42px; } .s1 .k2 { inset-inline-start: 342px; }

  /* 2 جدول الأعمال */
  .s2 .deco2 { top: 320px; inset-inline-start: 0; width: 540px; height: 400px; }
  .s2 .intro { width: 410px; }
  .ag { position: absolute; inset-inline-start: 596px; width: 600px; height: 100px; }
  .ag .tab { position: absolute; top: 0; inset-inline-start: 0; width: 82px; height: 118px; }
  .ag .band { position: absolute; top: 0; inset-inline-start: 82px; width: 517px; height: 99px; background: linear-gradient(var(--to-end), #D9E9FA 0%, #F1F7FD 45%, rgb(255 255 255 / 0) 100%); }
  .ag .num { position: absolute; top: 0; inset-inline-start: 0; width: 82px; height: 99px; margin: 0; display: flex; align-items: center; justify-content: center; font-size: 28px; font-weight: 300; color: var(--light); }
  .ag .tx { position: absolute; top: 0; inset-inline-start: 116px; height: 99px; margin: 0; display: flex; align-items: center; font-size: 29px; color: var(--ink); white-space: nowrap; }
  .ag1 { top: 119px; } .ag2 { top: 219px; } .ag3 { top: 320px; } .ag4 { top: 420px; } .ag5 { top: 520px; }

  /* 3 ما هو الإسناد */
  .s3 .card { position: absolute; top: 222px; width: 229px; height: 300px; border: 3px solid var(--line); border-radius: 30px; }
  .s3 .sq { position: absolute; top: 239px; width: 197px; height: 204px; border-radius: 22px; color: #fff; font-size: 44px; font-weight: 700; }
  .s3 .up { position: absolute; top: 417px; width: 52px; height: 52px; border-radius: 50%; background: #fff; color: var(--green); padding: 13px; }
  .s3 .ch3 { position: absolute; top: 483px; width: 229px; margin: 0; text-align: center; font-size: 22px; line-height: 1.2; font-weight: 700; }
  .s3 .cp { position: absolute; top: 522px; width: 229px; padding: 0 28px; margin: 0; text-align: center; font-size: 17.5px; line-height: 23px; }
  .s3 .x1 { inset-inline-start: 518px; } .s3 .x2 { inset-inline-start: 768px; } .s3 .x3 { inset-inline-start: 1018px; }
  .s3 .sq.x1 { inset-inline-start: 534px; background: var(--light); } .s3 .sq.x2 { inset-inline-start: 784px; background: var(--indigo); } .s3 .sq.x3 { inset-inline-start: 1034px; background: var(--steel); }
  .s3 .up.x1 { inset-inline-start: 606px; } .s3 .up.x2 { inset-inline-start: 856px; } .s3 .up.x3 { inset-inline-start: 1106px; }

  /* 4 لماذا تلجأ المؤسسات */
  .s4 .bx { position: absolute; width: 590px; height: 165px; border: 3px solid; border-radius: 20px; background: #fff; }
  .s4 .b1 { top: 215px; inset-inline-start: 35px; border-color: var(--light); } .s4 .b2 { top: 215px; inset-inline-start: 655px; border-color: var(--indigo); }
  .s4 .b3 { top: 399px; inset-inline-start: 35px; border-color: var(--steel); } .s4 .b4 { top: 399px; inset-inline-start: 655px; border-color: var(--green); }
  .s4 .pn { position: absolute; top: 6px; width: 105px; height: 147px; border-radius: 10px; color: #fff; display: grid; place-items: center; }
  .s4 .pn svg { width: 54px; height: 54px; }
  .s4 .b1 .pn, .s4 .b3 .pn { inset-inline-start: 5px; } .s4 .b2 .pn, .s4 .b4 .pn { inset-inline-end: 6px; }
  .s4 .b1 .pn { background: var(--light); } .s4 .b2 .pn { background: var(--indigo); } .s4 .b3 .pn { background: var(--steel); } .s4 .b4 .pn { background: var(--green); }
  .s4 .bt { position: absolute; width: 250px; }
  .s4 .bt h3 { white-space: nowrap; }
  .s4 .bt h3 { margin: 0 0 13px; font-size: 22px; line-height: 1.2; font-weight: 700; }
  .s4 .bt p { margin: 0; font-size: 17.5px; line-height: 23px; }
  .s4 .t1, .s4 .t3 { inset-inline-start: 192px; } .s4 .t2, .s4 .t4 { inset-inline-end: 192px; text-align: end; }
  .s4 .t1, .s4 .t2 { top: 252px; } .s4 .t3, .s4 .t4 { top: 437px; }
  .s4 .mask { position: absolute; top: 242px; inset-inline-start: 490px; width: 292px; height: 292px; border-radius: 50%; background: #fff; }
  .s4 .ring { position: absolute; top: 263px; inset-inline-start: 511px; width: 250px; height: 250px; border-radius: 50%; background: linear-gradient(var(--diag), #6AB0EB 0%, #4F86D6 55%, #3442AE 100%); }
  .s4 .core { position: absolute; top: 280px; inset-inline-start: 528px; width: 216px; height: 216px; border-radius: 50%; background: linear-gradient(var(--diag), #3442AE 0%, #4A73CC 45%, #66AAE8 100%); color: #fff; flex-direction: column; }
  .s4 .core b { font-size: 44px; line-height: 1.1; }
  .s4 .core span { font-size: 17.5px; line-height: 23px; width: 170px; margin-top: 6px; color: #EEF4FC; }

  /* 5 العمليات ضمن النطاق */
  .s5 .cell { position: absolute; }
  .s5 .g1 { top: 60px; inset-inline-start: 564px; width: 360px; height: 303px; background: linear-gradient(to bottom, rgb(255 255 255 / 0) 5%, #ADD1F5 100%); }
  .s5 .g2 { top: 60px; inset-inline-start: 924px; width: 356px; height: 303px; background: linear-gradient(to bottom, rgb(255 255 255 / 0) 5%, #ADB2E9 100%); }
  .s5 .g3 { top: 363px; inset-inline-start: 564px; width: 360px; height: 275px; background: linear-gradient(to bottom, #C3DFD3 0%, rgb(255 255 255 / 0) 95%); }
  .s5 .g4 { top: 363px; inset-inline-start: 924px; width: 356px; height: 275px; background: linear-gradient(to bottom, #BFD4E7 0%, rgb(255 255 255 / 0) 95%); }
  .s5 .ct { position: absolute; width: 300px; }
  .s5 .ct h3 { margin: 0 0 13px; font-size: 23px; line-height: 1.2; font-weight: 700; }
  .s5 .ct > p { margin: 0; font-size: 17.5px; line-height: 23px; width: 260px; }
  .s5 .ct hr { width: 110px; margin: 13px 0 0; border: 0; border-top: 1.3px solid var(--ink); }
  .s5 .ct ul { list-style: none; margin: 36px 0 0; padding: 0; }
  .s5 .ct li { position: relative; padding-inline-start: 32px; font-size: 17.5px; line-height: 23px; margin-bottom: 27px; white-space: nowrap; }
  .s5 .ct li svg { position: absolute; top: 4px; inset-inline-start: 7px; width: 15px; height: 15px; color: #222; }
  html[dir="rtl"] .s5 .ct li svg { transform: scaleX(-1); }
  .s5 .c1, .s5 .c3 { inset-inline-start: 603px; } .s5 .c2, .s5 .c4 { inset-inline-start: 962px; }
  .s5 .c1, .s5 .c2 { top: 75px; } .s5 .c3, .s5 .c4 { top: 418px; }

  /* 6 الوضع الحالي مقابل المستقبلي */
  .s6 .hd { position: absolute; top: 193px; height: 47px; border-radius: 24px; font-size: 19.5px; font-weight: 700; }
  .s6 .hd1 { inset-inline-start: 35px; width: 582px; border: 3px solid #D4D4D4; color: var(--ink); }
  .s6 .hd2 { inset-inline-start: 641px; width: 581px; background: var(--indigo); color: #fff; }
  .s6 .sb { position: absolute; top: 265px; width: 178px; height: 68px; border-radius: 10px; font-size: 15.5px; line-height: 19px; padding: 0 16px; }
  .s6 .cur { border: 1px solid #D2D2D2; background: #fff; color: var(--ink); }
  .s6 .fut { background: var(--indigo); color: #fff; }
  .s6 .p1 { inset-inline-start: 35px; } .s6 .p2 { inset-inline-start: 232px; } .s6 .p3 { inset-inline-start: 438px; }
  .s6 .p4 { inset-inline-start: 641px; } .s6 .p5 { inset-inline-start: 838px; } .s6 .p6 { inset-inline-start: 1044px; }
  .s6 .cc { position: absolute; top: 374px; width: 176px; height: 176px; border-radius: 50%; color: #fff; font-size: 17.5px; line-height: 23px; font-weight: 700; padding: 0 30px; }
  .s6 .cc1 { inset-inline-start: 233px; background: #BEBEBE; }
  .s6 .cc2 { inset-inline-start: 839px; background: linear-gradient(var(--diag), #69AEE9 10%, #4F7FD0 60%, #3442AE 100%); }
  .s6 .bb { position: absolute; top: 590px; width: 302px; height: 68px; border-radius: 10px; font-size: 17.5px; font-weight: 700; }
  .s6 .bb1 { inset-inline-start: 170px; border: 2px solid #D6D6D6; background: linear-gradient(var(--to-end), #D2D2D2, #fff 85%); color: var(--ink); }
  .s6 .bb2 { inset-inline-start: 776px; background: linear-gradient(var(--to-end), #69AEE9, #3E55B6 80%, #3442AE); color: #fff; }

  /* 7 تدفق العمليات */
  .s7 .col { position: absolute; top: 232px; height: 380px; border: 3px solid var(--line); border-radius: 22px; }
  .s7 .k1 { inset-inline-start: 35px; } .s7 .k2 { inset-inline-start: 247px; } .s7 .k3 { inset-inline-start: 459px; }
  .s7 .col.k1, .s7 .col.k2, .s7 .col.k3 { width: 182px; }
  .s7 .col.k4 { inset-inline-start: 670px; width: 224px; border-color: #9FC9F0; background: linear-gradient(to bottom, #69AEE9 0%, #A9CFF3 50%, #fff 90%); }
  .s7 .ch { position: absolute; top: 258px; height: 26px; margin: 0; text-align: center; font-size: 18.5px; line-height: 26px; font-weight: 700; color: #2A7FCE; }
  .s7 .ch.k1, .s7 .ch.k2, .s7 .ch.k3 { width: 182px; } .s7 .ch.k4 { inset-inline-start: 670px; width: 224px; color: #fff; }
  .s7 .bx { position: absolute; height: 68px; border-radius: 10px; border: 1px solid #C6DBEE; background: #fff; font-size: 15.5px; line-height: 19px; color: var(--ink); padding: 0 8px; }
  .s7 .bx.k1 { inset-inline-start: 50px; } .s7 .bx.k2 { inset-inline-start: 262px; } .s7 .bx.k3 { inset-inline-start: 474px; }
  .s7 .bx.k1, .s7 .bx.k2, .s7 .bx.k3 { width: 151px; }
  .s7 .bx.k4 { inset-inline-start: 690px; width: 182px; border-color: #69AEE9; padding: 0 20px; }
  .s7 .r1 { top: 308px; } .s7 .r2 { top: 399px; } .s7 .r3 { top: 489px; }
  .s7 .pnl { position: absolute; top: 133px; inset-inline-start: 925px; width: 336px; height: 500px; border: 2px solid #CBE0F4; border-bottom: 3px solid #3E4BB0; border-radius: 22px; background: linear-gradient(to bottom, #D3E6F8 0%, #fff 42%, #fff 62%, #CDD1F2 100%); }
  .s7 .ph { position: absolute; inset-inline-start: 925px; width: 336px; height: 26px; margin: 0; text-align: center; font-size: 18.5px; line-height: 26px; font-weight: 700; }
  .s7 .ph1 { top: 159px; color: #2A7FCE; } .s7 .ph2 { top: 583px; color: var(--indigo); }
  .s7 .qx { position: absolute; width: 150px; height: 68px; border-radius: 10px; border: 1px solid #A9CFF3; background: #fff; font-size: 15.5px; line-height: 19px; padding: 0 10px; }
  .s7 .qa { inset-inline-start: 938px; } .s7 .qb { inset-inline-start: 1098px; }
  .s7 .q1 { top: 210px; } .s7 .q2 { top: 300px; } .s7 .q3 { top: 489px; border-color: var(--indigo); }

  /* 8 نموذج التشغيل */
  .s8 .stmt-bar { top: 280px; } .s8 .stmt { top: 286px; }
  .s8 .pc { position: absolute; inset-inline-start: 133px; margin: 0; font-size: 44px; line-height: 1; font-weight: 300; color: var(--indigo); }
  .s8 .pt { position: absolute; inset-inline-start: 290px; width: 245px; margin: 0; font-size: 17.5px; line-height: 23px; }
  .s8 .dv { position: absolute; inset-inline-start: 132px; width: 468px; height: 0; border-top: 1px solid #D5D5D5; }
  .s8 .pn { position: absolute; inset-inline-start: 678px; width: 484px; height: 174px; border-radius: 20px; }
  .s8 .pn1 { top: 73px; background: var(--light); } .s8 .pn2 { top: 277px; background: var(--indigo); } .s8 .pn3 { top: 481px; background: var(--green); }
  .s8 .ph { position: absolute; inset-inline-start: 678px; width: 484px; height: 24px; margin: 0; text-align: center; font-size: 17px; line-height: 24px; font-weight: 700; color: #fff; }
  .s8 .wb { position: absolute; width: 149px; height: 68px; border-radius: 10px; background: #fff; font-size: 14.5px; line-height: 19px; padding: 0 12px; }
  .s8 .wa { inset-inline-start: 692px; } .s8 .wb2 { inset-inline-start: 846px; } .s8 .wc { inset-inline-start: 1002px; }

  /* 9 نموذج تقديم الخدمات */
  .s9 .gpill { top: 180px; inset-inline-start: 35px; width: 1195px; height: 47px; background: linear-gradient(#fff, #fff) padding-box, linear-gradient(var(--to-end), var(--light), var(--indigo) 50%, var(--green)) border-box; }
  .s9 .chh { position: absolute; top: 245px; height: 75px; margin: 0; font-size: 22px; font-weight: 700; color: #fff; }
  .s9 .h1 { inset-inline-start: 35px; width: 386px; } .s9 .h2 { inset-inline-start: 445px; width: 375px; } .s9 .h3 { inset-inline-start: 845px; width: 373px; }
  .s9 .gc { position: absolute; top: 342px; height: 45px; margin: 0; font-size: 17.5px; color: var(--ink); white-space: nowrap; }
  .s9 .gc1 { inset-inline-start: 40px; width: 240px; } .s9 .gc2 { inset-inline-start: 286px; width: 212px; } .s9 .gc3 { inset-inline-start: 519px; width: 212px; }
  .s9 .gc4 { inset-inline-start: 753px; width: 212px; } .s9 .gc5 { inset-inline-start: 985px; width: 212px; }
  .s9 .cd { position: absolute; top: 411px; width: 288px; height: 175px; border: 3px solid var(--line); border-radius: 26px; }
  .s9 .ct { position: absolute; top: 453px; width: 230px; }
  .s9 .ct h3 { margin: 0 0 22px; font-size: 19.5px; line-height: 25px; font-weight: 700; width: 160px; }
  html[lang="ar"] .s9 .ct h3 { width: 200px; }
  .s9 .ct p { margin: 0; font-size: 17.5px; line-height: 23px; width: 225px; }
  .s9 .d1 { inset-inline-start: 35px; } .s9 .d2 { inset-inline-start: 338px; } .s9 .d3 { inset-inline-start: 640px; } .s9 .d4 { inset-inline-start: 943px; }
  .s9 .e1 { inset-inline-start: 72px; } .s9 .e2 { inset-inline-start: 374px; } .s9 .e3 { inset-inline-start: 677px; } .s9 .e4 { inset-inline-start: 979px; }

  /* 10 الحوكمة ومؤشرات الأداء */
  .s10 .side { position: absolute; top: 0; inset-inline-end: 0; width: 413px; height: 720px; background: var(--indigo); }
  .s10 .kp { position: absolute; inset-inline-start: 905px; width: 340px; }
  .s10 .kp b { display: block; font-size: 44px; line-height: 1; font-weight: 300; color: var(--light); margin-bottom: 9px; letter-spacing: 0.5px; }
  .s10 .kp strong { display: block; font-size: 17.5px; line-height: 23px; font-weight: 700; color: #fff; }
  .s10 .kp span { display: block; font-size: 17.5px; line-height: 23px; color: #D9E0F5; }
  .s10 .kp1 { top: 84px; } .s10 .kp2 { top: 239px; } .s10 .kp3 { top: 394px; } .s10 .kp4 { top: 550px; }
  .s10 .gpill { top: 262px; inset-inline-start: 108px; width: 685px; height: 47px; }
  .s10 .gb { position: absolute; inset-inline-start: 208px; width: 484px; height: 67px; border-radius: 12px; color: #fff; flex-direction: column; }
  .s10 .gb b { font-size: 19.5px; line-height: 23px; } .s10 .gb span { font-size: 17.5px; line-height: 23px; }
  .s10 .gb1 { top: 326px; background: var(--light); } .s10 .gb2 { top: 438px; background: var(--indigo); } .s10 .gb3 { top: 551px; background: var(--green); }

  /* 11 روافع خلق القيمة */
  .s11 .bandc { position: absolute; top: 382px; inset-inline-start: 0; width: 560px; height: 106px; background: linear-gradient(var(--to-end), #C9ECF2 0%, #E6F6F8 50%, rgb(255 255 255 / 0) 100%); }
  .s11 .stmt-bar { top: 393px; } .s11 .stmt { top: 400px; }
  .s11 .lt { position: absolute; width: 168px; margin: 0; font-size: 17.5px; line-height: 23px; }
  .s11 .l1, .s11 .l3 { inset-inline-start: 316px; } .s11 .l2, .s11 .l4 { inset-inline-start: 1067px; }
  .s11 .l1, .s11 .l2 { top: 267px; } .s11 .l3, .s11 .l4 { top: 537px; }
  .s11 .ht { position: absolute; width: 150px; height: 60px; margin: 0; font-size: 17.5px; line-height: 23px; font-weight: 700; }
  .s11 .m1, .s11 .m3 { inset-inline-start: 522px; } .s11 .m2, .s11 .m4 { inset-inline-start: 869px; }
  .s11 .m1, .s11 .m2 { top: 268px; } .s11 .m3, .s11 .m4 { top: 544px; }
  .s11 .sat { filter: drop-shadow(-5px 7px 5px rgb(0 0 0 / .22)); }

  /* 12 المخاطر وسبل الحد منها */
  .s12 .lgd { position: absolute; top: 141px; inset-inline-end: 56px; display: flex; gap: 38px; align-items: center; }
  html[lang="ar"] .s12 .lgd { gap: 28px; }
  .s12 .lgd span { display: flex; align-items: center; gap: 11px; font-size: 17.5px; white-space: nowrap; }
  .s12 .lgd i { width: 22px; height: 22px; border-radius: 3px; display: block; }
  .s12 .gpill { top: 202px; inset-inline-start: 132px; width: 1094px; height: 58px; }
  .s12 .vb { position: absolute; top: 288px; inset-inline-start: 55px; width: 60px; height: 290px; border: 3px solid transparent; border-inline-end-width: 0; border-start-start-radius: 16px; border-end-start-radius: 16px; background: linear-gradient(#fff, #fff) padding-box, linear-gradient(to bottom, var(--indigo), var(--light)) border-box; }
  .s12 .vt { position: absolute; top: 418px; inset-inline-start: -58px; width: 290px; height: 30px; transform: rotate(-90deg); font-size: 17.5px; font-weight: 700; white-space: nowrap; }
  .s12 .rl { position: absolute; inset-inline-start: 132px; width: 138px; height: 83px; border: 1px solid #D5D5D5; border-radius: 12px; font-size: 17.5px; font-weight: 700; }
  .s12 .y1 { top: 288px; } .s12 .y2 { top: 391px; } .s12 .y3 { top: 494px; }
  .s12 .rc { position: absolute; width: 297px; height: 83px; border-radius: 12px; flex-direction: column; font-size: 17.5px; line-height: 23px; color: var(--ink); }
  .s12 .rc b { font-weight: 700; }
  .s12 .xa { inset-inline-start: 279px; } .s12 .xb { inset-inline-start: 604px; } .s12 .xc { inset-inline-start: 928px; }
  .s12 .lo { background: #C0E496; } .s12 .md { background: #CACDF0; } .s12 .hi { background: #FED966; } .s12 .cr { background: #FF4D4D; color: #fff; } .s12 .cx { background: #CC3232; color: #fff; }
  .s12 .cl { position: absolute; top: 593px; width: 297px; height: 43px; border: 1px solid #D5D5D5; border-radius: 10px; font-size: 17.5px; font-weight: 700; }

  /* 13 الانتقال وخارطة الطريق */
  .s13 .cd { position: absolute; top: 212px; width: 286px; height: 310px; border: 3px solid var(--line); border-radius: 28px; }
  .s13 .cd.fade { -webkit-mask-image: linear-gradient(to bottom, #000 60%, rgb(0 0 0 / .25) 100%); mask-image: linear-gradient(to bottom, #000 60%, rgb(0 0 0 / .25) 100%); }
  .s13 .hp { position: absolute; top: 228px; width: 258px; height: 49px; border-radius: 12px; color: #fff; font-size: 19.5px; font-weight: 700; }
  .s13 .tp { position: absolute; top: 297px; width: 258px; margin: 0; text-align: center; font-size: 17.5px; line-height: 23px; padding: 0 8px; }
  .s13 .ob { position: absolute; width: 259px; height: 50px; border-radius: 12px; border: 1px solid; background: #fff; font-size: 17.5px; }
  .s13 .o1 { top: 381px; } .s13 .o2 { top: 448px; }
  .s13 .z1 { inset-inline-start: 36px; } .s13 .z2 { inset-inline-start: 344px; } .s13 .z3 { inset-inline-start: 652px; } .s13 .z4 { inset-inline-start: 960px; }
  .s13 .w1 { inset-inline-start: 49px; } .s13 .w2 { inset-inline-start: 357px; } .s13 .w3 { inset-inline-start: 665px; } .s13 .w4 { inset-inline-start: 973px; }
  .s13 .hp.w1 { background: var(--light); } .s13 .hp.w2 { background: var(--indigo); } .s13 .hp.w3 { background: var(--green); } .s13 .hp.w4 { background: var(--steel); }
  .s13 .ob.w1 { border-color: #6FAAD8; } .s13 .ob.w2 { border-color: #3B3F99; } .s13 .ob.w3 { border-color: #5E9C86; } .s13 .ob.w4 { border-color: #5E86A6; }
  .s13 .ft { position: absolute; top: 569px; inset-inline-start: 36px; width: 1210px; margin: 0; text-align: center; font-size: 17.5px; line-height: 23px; font-weight: 700; }

  /* 14 الخطوات التالية */
  .s14 .nn { position: absolute; top: 255px; width: 75px; height: 44px; margin: 0; text-align: center; font-size: 33px; line-height: 44px; font-weight: 300; color: var(--light); }
  .s14 .nh { position: absolute; top: 459px; width: 205px; margin: 0; font-size: 22.5px; line-height: 29px; font-weight: 700; }
  .s14 .np { position: absolute; top: 533px; width: 190px; margin: 0; font-size: 19.5px; line-height: 25.5px; }
  .s14 .u1 { inset-inline-start: 126px; } .s14 .u2 { inset-inline-start: 428px; } .s14 .u3 { inset-inline-start: 723px; } .s14 .u4 { inset-inline-start: 1018px; }
  .s14 .v1 { inset-inline-start: 60px; } .s14 .v2 { inset-inline-start: 366px; } .s14 .v3 { inset-inline-start: 660px; } .s14 .v4 { inset-inline-start: 955px; }

  /* 15 الختام */
  .s15 .deco15 { top: 0; inset-inline-end: 0; width: 551px; height: 408px; transform: rotate(180deg); }
  html[dir="rtl"] .s15 .deco15 { transform: scaleY(-1); }
  .s15 .ty { position: absolute; top: 112px; inset-inline-start: 34px; margin: 0; font-size: 56px; line-height: 1.15; font-weight: 700; color: var(--light); white-space: nowrap; }
  .s15 .cw { position: absolute; top: 180px; inset-inline-start: 34px; margin: 0; font-size: 54px; line-height: 1.15; font-weight: 400; color: var(--indigo); white-space: nowrap; }
  html[lang="ar"] .s15 .ty { font-weight: 800; }
  .s15 .tp { position: absolute; top: 275px; inset-inline-start: 43px; width: 535px; margin: 0; font-size: 22px; line-height: 29px; }
  .s15 .bnd { position: absolute; top: 360px; inset-inline-start: 0; width: 1280px; height: 197px; background: linear-gradient(var(--to-end), #3340AE 0%, #4F7CCB 50%, #69AEE9 100%); }
  .s15 .cn { position: absolute; top: 391px; width: 300px; color: #fff; }
  .s15 .cn .icon { width: 40px; height: 40px; margin-bottom: 26px; }
  .s15 .cn h3 { margin: 0; font-size: 19.5px; line-height: 24px; font-weight: 700; }
  .s15 .cn p { margin: 0; font-size: 17.5px; line-height: 23px; color: #EAF1FB; }
  .s15 .f1 { inset-inline-start: 43px; } .s15 .f2 { inset-inline-start: 351px; } .s15 .f3 { inset-inline-start: 679px; } .s15 .f4 { inset-inline-start: 984px; }
  .s15 .strip { position: absolute; top: 558px; inset-inline-start: 0; width: 780px; height: 110px; background: linear-gradient(var(--to-end), #BAD9F5 0%, #C9E1F7 38%, rgb(255 255 255 / 0) 100%); }
  .s15 .logo { top: 575px; inset-inline-start: 44px; }
  .s15 .lgt { top: 589px; inset-inline-start: 135px; color: #222; }"""

# ---------- الأيقونات ----------
I = {
    'up': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 21V3M5.5 9.5 12 3l6.5 6.5"/></svg>',
    'bul': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="12" cy="12" r="10"/><path d="M7 12h9M12.5 8l4 4-4 4"/></svg>',
    'growth': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><circle cx="5" cy="5" r="1.7"/><circle cx="9.5" cy="5" r="1.7"/><circle cx="14" cy="5" r="1.7"/><path d="M3.3 7.6h3.4l.4 4.6H5.9V15H4.1v-2.8H2.9zM7.8 7.6h3.4l.4 4.6h-1.2V14H8.6v-1.8H7.4zM12.3 7.6h3.4l.3 3.2-1.4 1.4h-.2v1.3h-1.8v-1.3h-.7z"/><path d="M2 21.5 9 15l3.4 3 6.4-6.6" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/><path d="M16.2 9.6H22v5.8z"/></svg>',
    'brief': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M9 4h6a1.5 1.5 0 0 1 1.5 1.5V7H21a1 1 0 0 1 1 1v4.2H2V8a1 1 0 0 1 1-1h4.5V5.5A1.5 1.5 0 0 1 9 4zm.2 1.7V7h5.6V5.7z"/><path d="M2 13.6h8.6v1.6h2.8v-1.6H22V20a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1z"/></svg>',
    'inbox': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><circle cx="16.5" cy="6" r="4.6"/><path d="m14.3 6 1.6 1.6 2.9-3" fill="none" stroke="#5589BB" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/><path d="M3.5 11h4.1l1.3 3h6.2l1.3-3h4.1l1.5 3.8V21a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1v-6.2zm-.1 4.2V20h17.2v-4.8h-3.9l-1.3 3H8.6l-1.3-3z"/></svg>',
    'hands': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M1 8.5 4.3 6l4.3 1.4L11 6.2l2.4.5-3.6 3.3a1.3 1.3 0 0 0 1.7 1.9l2.8-2.3 5.1 4.6.6-.4 1.3-1.2L23 8.5 19.7 6l-3.4 1.1-3.7-1.4-2 .1L7.5 7z"/><path d="m3 12 1.2 1.3 3.8 3.9a1.1 1.1 0 0 0 1.6-1.5l1.3 1.3a1.1 1.1 0 0 0 1.6-1.6l.7.7a1.1 1.1 0 0 0 1.6-1.5l.4.4a1.1 1.1 0 0 0 1.6-1.6l-3.3-3.1-1.9 1.6a2.6 2.6 0 0 1-3.6-3.7l.3-.3-1.9.7L5 8z"/></svg>',
    'mail': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linejoin="round" aria-hidden="true"><path d="M2.5 9.5 12 2.5l9.5 7V21h-19z"/><path d="M5.5 11.5V5.5h13v6"/><path d="M8 8h8M8 10.5h8M8 13h5"/><path d="m2.5 9.5 9.5 7 9.5-7M2.5 21l7.3-6.2M21.5 21l-7.3-6.2"/></svg>',
    'phone': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M6.6 3.5 8.9 3l1.9 4.4-1.6 1.6a12 12 0 0 0 5.8 5.8l1.6-1.6 4.4 1.9-.5 2.3a2.3 2.3 0 0 1-2.4 1.8A15.6 15.6 0 0 1 4.8 5.9a2.3 2.3 0 0 1 1.8-2.4z"/><path d="M14 2.6a7.6 7.6 0 0 1 7.4 7.4M14 5.4a4.8 4.8 0 0 1 4.6 4.6M14 8.2a2 2 0 0 1 1.8 1.8" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/></svg>',
    'city': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M1.5 21V11h5.5v10zM7.8 21V3.5h6.6V21zM15.2 21V8h7.3v13z" opacity=".98"/><g fill="#5A86CF"><rect x="2.8" y="12.5" width="1.2" height="1.6"/><rect x="4.6" y="12.5" width="1.2" height="1.6"/><rect x="2.8" y="15.5" width="1.2" height="1.6"/><rect x="4.6" y="15.5" width="1.2" height="1.6"/><rect x="9.3" y="5.5" width="1.3" height="1.7"/><rect x="11.6" y="5.5" width="1.3" height="1.7"/><rect x="9.3" y="9" width="1.3" height="1.7"/><rect x="11.6" y="9" width="1.3" height="1.7"/><rect x="9.3" y="12.5" width="1.3" height="1.7"/><rect x="11.6" y="12.5" width="1.3" height="1.7"/><rect x="10.3" y="17.5" width="1.6" height="3.5"/><rect x="16.7" y="10" width="1.3" height="1.7"/><rect x="19.6" y="10" width="1.3" height="1.7"/><rect x="16.7" y="13.5" width="1.3" height="1.7"/><rect x="19.6" y="13.5" width="1.3" height="1.7"/><rect x="16.7" y="17" width="1.3" height="1.7"/><rect x="19.6" y="17" width="1.3" height="1.7"/></g></svg>',
    'globe': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" aria-hidden="true"><circle cx="12" cy="12" r="9.5"/><ellipse cx="12" cy="12" rx="4.2" ry="9.5"/><path d="M2.5 12h19M4 7h16M4 17h16"/></svg>',
}
LOGO = ('<svg class="logo" viewBox="0 0 58 64" aria-hidden="true"><path fill="currentColor" fill-rule="evenodd" d="M10 3 49 9.5Q56 10.7 56 18L55 47Q54.8 54 48 55.5L10 62Q2 63 2 55L2 11Q2 2 10 3ZM16 14Q12 13.6 12 18L12 47Q12 51.4 16 50.8L43 46.5Q46 46 46 42.5L46.8 21Q47 17.6 43.6 17.2Z"/></svg>')

# ---------- الزخرفة المائلة (شكل واحد بمقياس 540×400 يُعاد استخدامه بتحويلات) ----------
GLOBAL_DEFS = ('<defs>'
    '<linearGradient id="dA" x1="0" y1="13" x2="0" y2="330" gradientUnits="userSpaceOnUse"><stop offset="0" stop-color="#A6CFF4"/><stop offset=".55" stop-color="#D9EAFA"/><stop offset="1" stop-color="#FFFFFF" stop-opacity="0"/></linearGradient>'
    '<linearGradient id="dB" x1="262" y1="285" x2="200" y2="420" gradientUnits="userSpaceOnUse"><stop offset="0" stop-color="#A9D0F4"/><stop offset=".55" stop-color="#D5E8F9"/><stop offset="1" stop-color="#FFFFFF" stop-opacity="0"/></linearGradient>'
    '<linearGradient id="dC" x1="135" y1="341" x2="105" y2="410" gradientUnits="userSpaceOnUse"><stop offset="0" stop-color="#A9D0F4"/><stop offset="1" stop-color="#FFFFFF" stop-opacity="0"/></linearGradient>'
    '<linearGradient id="tlg" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#C6E0F8"/><stop offset="1" stop-color="#FFFFFF" stop-opacity="0"/></linearGradient>'
    '<linearGradient id="tabg" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#A9CFF3"/><stop offset=".55" stop-color="#E4F0FB"/><stop offset="1" stop-color="#FFFFFF" stop-opacity="0"/></linearGradient>'
    '<linearGradient id="nsg" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#AED3F3"/><stop offset=".5" stop-color="#E0EEFB"/><stop offset="1" stop-color="#FFFFFF" stop-opacity="0"/></linearGradient>'
    '<linearGradient id="chg" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#FFFFFF"/><stop offset="1" stop-color="#D3D3D3"/></linearGradient>'
    '<linearGradient id="ringg" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#6AB0EB"/><stop offset="1" stop-color="#3442AE"/></linearGradient>'
    '<marker id="ag" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="7" markerHeight="7" orient="auto"><path d="M1,1 L9,5 L1,9" fill="none" stroke="#BDBDBD" stroke-width="1.4"/></marker>'
    '<marker id="ai" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="7" markerHeight="7" orient="auto"><path d="M1,1 L9,5 L1,9" fill="none" stroke="#3B45A6" stroke-width="1.4"/></marker>'
    '<marker id="al" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="7" markerHeight="7" orient="auto"><path d="M1,1 L9,5 L1,9" fill="none" stroke="#69AEE9" stroke-width="1.4"/></marker>'
    '<marker id="ad" viewBox="0 0 10 10" refX="8" refY="5" markerWidth="5" markerHeight="5" orient="auto"><path d="M0,0 L10,5 L0,10 z" fill="#D0D0D0"/></marker>'
    '<marker id="ab" viewBox="0 0 10 10" refX="8" refY="5" markerWidth="5" markerHeight="5" orient="auto"><path d="M0,0 L10,5 L0,10 z" fill="#69AEE9"/></marker>'
    '</defs>')


def deco(cls):
    return ('<svg class="deco mir %s" viewBox="0 0 540 400" preserveAspectRatio="none" aria-hidden="true">'
            '<path fill="url(#dA)" d="M0,13 L228,113 C258,127 272,150 272,182 L272,400 L0,400 Z"/>'
            '<path fill="url(#dB)" d="M0,170 L530,400 L0,400 Z"/>'
            '<path fill="url(#dC)" d="M0,283 L270,400 L0,400 Z"/></svg>' % cls)


DECO_S = deco('deco-s')
HEAD = '<p class="biz">§BIZ§</p>\n    '


def ttl(n):
    return '<h1 class="ttl">§T%d§</h1>' % n


def items():  # العنصران المرقّمان في الشرائح 3 و5
    return ''.join('<div class="ni ni%d"><p class="n">%d</p><h3>§HH§</h3><p>§IDT2§</p></div>' % (k, k) for k in (1, 2))


def arrow_circle(cx, cy, r=9, sw=1.4, color='#69AEE9'):  # دائرة بسهم نحو جهة النهاية (تُعكس مع mir)
    a = r * 0.5
    return ('<circle cx="%s" cy="%s" r="%s" fill="#fff" stroke="%s" stroke-width="%s"/>'
            '<path d="M%s,%s H%s M%s,%s L%s,%s L%s,%s" fill="none" stroke="%s" stroke-width="%s" stroke-linecap="round" stroke-linejoin="round"/>'
            % (cx, cy, r, color, sw, cx - a, cy, cx + a, cx + a * 0.1, cy - a * 0.9, cx + a, cy, cx + a * 0.1, cy + a * 0.9, color, sw))


# ---------- الشرائح ----------
S1 = ('<svg class="tl mir" viewBox="0 0 260 125" aria-hidden="true"><polygon points="0,0 255,0 0,120" fill="url(#tlg)"/></svg>\n    '
      + deco('deco1') + '\n    ' + LOGO + '<p class="lgt">§LOGO_T§</p>\n    '
      '<h1 class="abs c1">§C1§</h1><p class="abs c2">§C2§</p><p class="abs c3">§C3§</p>\n    '
      '<p class="abs lb k1">§PB§</p><p class="abs vl k1">§PBV§</p><p class="abs lb k2">§DT§</p><p class="abs vl k2">§DTV§</p>')


def agenda_row(i):
    band = '<div class="band"></div>' if i % 2 == 1 else ''
    return ('<div class="ag ag%d">%s<svg class="tab mir" viewBox="0 0 82 118" aria-hidden="true"><path d="M82,0 L16,30 Q0,37 0,56 L0,118 L82,118 Z" fill="url(#tabg)"/></svg>'
            '<p class="num">0%d</p><p class="tx">§SEC§</p></div>' % (i, band, i))


S2 = (deco('deco2') + '\n    ' + HEAD + ttl(2) + '\n    <p class="intro">§INTRO§</p>\n    '
      + '\n    '.join(agenda_row(i) for i in range(1, 6)))

S3 = (DECO_S + '\n    ' + HEAD + ttl(3) + '\n    <p class="intro">§INTRO§</p>\n    ' + items() + '\n    '
      + ''.join('<div class="card fade x%d"></div>' % k for k in (1, 2, 3)) + '\n    '
      + ''.join('<div class="sq ctr x%d">§V3_%d§</div><div class="up icon x%d">%s</div><h3 class="ch3 x%d">§HH§</h3><p class="cp x%d">§IDT_S§</p>\n    '
                % (k, k, k, I['up'], k, k) for k in (1, 2, 3)))

S4 = (DECO_S + '\n    ' + HEAD + ttl(4) + '\n    '
      + ''.join('<div class="bx b%d"><div class="pn">%s</div></div>' % (k, I[ic]) for k, ic in ((1, 'growth'), (2, 'brief'), (3, 'inbox'), (4, 'hands')))
      + '\n    ' + ''.join('<div class="bt t%d"><h3>§HH§</h3><p>§IDT2§</p></div>' % k for k in (1, 2, 3, 4))
      + '\n    <div class="mask"></div><div class="ring"></div><div class="core ctr"><b>§V4§</b><span>§IDT§</span></div>')


def cell(k):
    li = '<li>%s§IDT§</li>' % I['bul']
    return '<div class="ct c%d"><h3>§HH§</h3><p>§IDT2§</p><hr><ul>%s%s</ul></div>' % (k, li, li)


S5 = (DECO_S + '\n    ' + HEAD + ttl(5) + '\n    <p class="intro">§INTRO§</p>\n    ' + items() + '\n    '
      + ''.join('<div class="cell g%d"></div>' % k for k in (1, 2, 3, 4)) + '\n    ' + '\n    '.join(cell(k) for k in (1, 2, 3, 4)))

S6 = (DECO_S + '\n    ' + HEAD + ttl(6) + '\n    '
      '<svg class="full mir" viewBox="0 0 1280 720" aria-hidden="true"><g fill="none" stroke="#C8C8C8" stroke-width="1.2">'
      '<polyline points="124,333 124,462 229,462" marker-end="url(#ag)"/><line x1="321" y1="333" x2="321" y2="371" marker-end="url(#ag)"/>'
      '<polyline points="527,333 527,462 413,462" marker-end="url(#ag)"/><line x1="321" y1="550" x2="321" y2="587" marker-end="url(#ag)"/></g>'
      '<g fill="none" stroke="#3B45A6" stroke-width="1.2"><polyline points="730,333 730,462 835,462" marker-end="url(#ai)"/>'
      '<line x1="927" y1="333" x2="927" y2="371" marker-end="url(#ai)"/><polyline points="1133,333 1133,462 1019,462" marker-end="url(#ai)"/>'
      '<line x1="927" y1="550" x2="927" y2="587" marker-end="url(#ai)"/></g></svg>\n    '
      '<div class="hd hd1 ctr">§CUR§</div><div class="hd hd2 ctr">§FUT§</div>\n    '
      + ''.join('<div class="sb cur ctr p%d">§IDT§</div>' % k for k in (1, 2, 3))
      + ''.join('<div class="sb fut ctr p%d">§IDT§</div>' % k for k in (4, 5, 6))
      + '\n    <div class="cc cc1 ctr">§IDT§</div><div class="cc cc2 ctr">§IDT§</div>'
      '<div class="bb bb1 ctr">§IDT§</div><div class="bb bb2 ctr">§IDT§</div>')

S7_SVG = ('<svg class="full mir" viewBox="0 0 1280 720" aria-hidden="true"><g stroke="#C9DDEF" stroke-width="1.2">'
          + ''.join('<line x1="%d" y1="376" x2="%d" y2="399"/><line x1="%d" y1="467" x2="%d" y2="489"/>' % (x, x, x, x) for x in (125, 337, 549))
          + '</g><g stroke="#69AEE9" stroke-width="1.2"><line x1="781" y1="376" x2="781" y2="399"/><line x1="781" y1="467" x2="781" y2="489"/></g>'
          + ''.join(arrow_circle(x, 387) for x in (231, 443, 655))
          + '<g fill="none" stroke="#69AEE9" stroke-width="1.2"><polyline points="872,421 910,421 910,232 923,232" marker-end="url(#al)"/>'
          '<polyline points="1093,635 1093,657 781,657 781,560" marker-end="url(#al)"/></g></svg>')

S7 = (DECO_S + '\n    ' + HEAD + ttl(7) + '\n    <p class="sub" style="width:900px">§SUB§</p>\n    '
      + ''.join('<div class="col fade k%d"></div>' % k for k in (1, 2, 3, 4)) + '<div class="pnl"></div>\n    ' + S7_SVG + '\n    '
      + ''.join('<p class="ch k%d">§HH§</p>' % k for k in (1, 2, 3, 4)) + '\n    '
      + ''.join('<div class="bx ctr k%d r%d">§IDT§</div>' % (k, r) for k in (1, 2, 3, 4) for r in (1, 2, 3)) + '\n    '
      + '<p class="ph ph1">§HH§</p><p class="ph ph2">§HH§</p>'
      + ''.join('<div class="qx ctr q%d %s">§IDT§</div>' % (r, c) for r in (1, 2, 3) for c in ('qa', 'qb')))

S8_SVG = ('<svg class="full mir" viewBox="0 0 1280 720" aria-hidden="true"><g fill="none" stroke="#D3D3D3" stroke-width="2.5">'
          '<polyline points="676,365 634,365 634,160 672,160" marker-end="url(#ad)"/><polyline points="676,591 634,591 634,387 672,387" marker-end="url(#ad)"/>'
          '<polyline points="1164,160 1207,160 1207,365 1168,365" marker-end="url(#ad)"/><polyline points="1164,387 1207,387 1207,591 1168,591" marker-end="url(#ad)"/></g>'
          + ''.join('<circle cx="920" cy="%d" r="28" fill="#fff"/><circle cx="920" cy="%d" r="16.5" fill="none" stroke="%s" stroke-width="2"/>'
                    '<path d="M920,%d V%d M914,%d L920,%d L926,%d" fill="none" stroke="%s" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>'
                    % (cy, cy, c, cy - 8, cy + 8, cy + 2, cy + 8, cy + 2, c) for cy, c in ((262, '#69AEE9'), (466, '#3340AE')))
          + '</svg>')

S8 = (DECO_S + '\n    ' + HEAD + ttl(8) + '\n    <p class="sub">§SUB§</p>\n    '
      '<div class="stmt-bar"></div><p class="stmt">§STMT§</p>\n    '
      + ''.join('<p class="pc" style="top:%dpx">§V8_%d§</p><p class="pt" style="top:%dpx">§IDT2§</p>' % (t - 9, k, t - 11) for k, t in ((1, 405), (2, 498), (3, 590)))
      + '<div class="dv" style="top:461px"></div><div class="dv" style="top:558px"></div>\n    '
      + ''.join('<div class="pn pn%d"></div>' % k for k in (1, 2, 3)) + S8_SVG
      + ''.join('<p class="ph" style="top:%dpx">§HH§</p>' % t for t in (95, 312, 511)) + '\n    '
      + ''.join('<div class="wb ctr %s" style="top:%dpx">§IDT§</div>' % (c, t) for t in (153, 360, 566) for c in ('wa', 'wb2', 'wc')))


def chev(x1, x2, y1, y2, d, notch, fill):
    ym = (y1 + y2) / 2
    pts = '%s,%s %s,%s %s,%s %s,%s %s,%s' % (x1, y1, x2 - d, y1, x2, ym, x2 - d, y2, x1, y2)
    if notch:
        pts += ' %s,%s' % (x1 + d, ym)
    return '<polygon points="%s" fill="%s"/>' % (pts, fill)


S9_SVG = ('<svg class="full mir" viewBox="0 0 1280 720" aria-hidden="true">'
          + chev(35, 432, 245, 320, 22, False, '#69AEE9') + chev(434, 831, 245, 320, 22, True, '#3340AE') + chev(833, 1230, 245, 320, 22, True, '#5EAB8B')
          + ''.join(chev(a, b, 342, 387, 13, True, 'url(#chg)') for a, b in ((40, 291), (293, 524), (526, 757), (760, 990), (992, 1222)))
          + '</svg>')

S9 = (DECO_S + '\n    ' + HEAD + ttl(9) + '\n    <div class="gpill ctr">§IDTN§</div>\n    ' + S9_SVG + '\n    '
      + ''.join('<p class="chh ctr h%d">§HH§</p>' % k for k in (1, 2, 3))
      + ''.join('<p class="gc ctr gc%d">§ITH§</p>' % k for k in (1, 2, 3, 4, 5)) + '\n    '
      + ''.join('<div class="cd fade d%d"></div>' % k for k in (1, 2, 3, 4))
      + ''.join('<div class="ct e%d"><h3>§HTP§</h3><p>§IDT_S§</p></div>' % k for k in (1, 2, 3, 4)))

S10_SVG = ('<svg class="full mir" viewBox="0 0 1280 720" aria-hidden="true"><g fill="none" stroke="#C8C8C8" stroke-width="1.3">'
           '<polyline points="110,285 85,285 85,359 205,359" marker-end="url(#ag)"/><polyline points="691,358 745,358 745,471 695,471" marker-end="url(#ag)"/>'
           '<polyline points="211,471 83,471 83,584 205,584" marker-end="url(#ag)"/><line x1="450" y1="393" x2="450" y2="435" marker-end="url(#ag)"/>'
           '<line x1="450" y1="505" x2="450" y2="548" marker-end="url(#ag)"/></g>'
           '<g fill="#C8C8C8" stroke="#fff" stroke-width="1.5"><circle cx="110" cy="285" r="4.5"/><circle cx="691" cy="358" r="4.5"/><circle cx="211" cy="471" r="4.5"/></g></svg>')

S10 = ('<div class="side"></div>\n    ' + DECO_S + '\n    ' + HEAD + ttl(10) + '\n    <p class="sub">§SUB§</p>\n    '
       + ''.join('<div class="kp kp%d"><b>§V10_%d§</b><strong>§IDT§</strong><span>§ST§</span></div>' % (k, k) for k in (1, 2, 3, 4)) + '\n    '
       '<div class="gpill ctr">§IDTN§</div>\n    '
       + ''.join('<div class="gb ctr gb%d"><b>§HH§</b><span>§IDP§</span></div>' % k for k in (1, 2, 3)) + '\n    ' + S10_SVG)


def petal():
    return 'M690,322 Q712,366 748,372 L704,428 Q672,394 620,390 Z'


S11_SVG = ('<svg class="full mir" viewBox="0 0 1280 720" aria-hidden="true">'
           '<circle cx="768" cy="441" r="201" fill="none" stroke="#D6D6D6" stroke-width="3"/>'
           '<path d="%s" fill="#69AEE9"/>' % petal()
           + '<path d="%s" fill="#3340AE" transform="translate(1540 0) scale(-1 1)"/>' % petal()
           + '<path d="%s" fill="#548CBB" transform="translate(0 882) scale(1 -1)"/>' % petal()
           + '<path d="%s" fill="#5EAB8B" transform="translate(1540 882) scale(-1 -1)"/>' % petal()
           + '<g class="sat" fill="#fff" stroke-width="3"><circle cx="597" cy="298" r="92" stroke="#69AEE9"/><circle cx="943" cy="298" r="92" stroke="#3340AE"/>'
           '<circle cx="597" cy="584" r="92" stroke="#548CBB"/><circle cx="943" cy="584" r="92" stroke="#5EAB8B"/></g>'
           '<circle cx="770" cy="441" r="64" fill="none" stroke="url(#ringg)" stroke-width="24"/></svg>')

S11 = (DECO_S + '\n    ' + HEAD + ttl(11) + '\n    <div class="bandc"></div><div class="stmt-bar"></div><p class="stmt">§STMT§</p>\n    '
       + S11_SVG + '\n    '
       + ''.join('<p class="lt l%d">§IDT_S§</p>' % k for k in (1, 2, 3, 4))
       + ''.join('<p class="ht ctr m%d">§HTP§</p>' % k for k in (1, 2, 3, 4)))

CELLS = [('lo', 'md', 'hi'), ('md', 'hi', 'cr'), ('hi', 'cr', 'cx')]
S12 = (DECO_S + '\n    ' + HEAD + ttl(12) + '\n    '
       '<div class="lgd"><span><i class="lo"></i>§LG1§</span><span><i class="md"></i>§LG2§</span><span><i class="hi"></i>§LG3§</span><span><i class="cx"></i>§LG4§</span></div>\n    '
       '<div class="gpill ctr">§IDTN§</div>\n    <div class="vb"></div><p class="vt ctr">§VT§</p>\n    '
       + ''.join('<div class="rl ctr y%d">§LV%d§</div>' % (r, r) for r in (1, 2, 3)) + '\n    '
       + ''.join('<div class="rc ctr %s %s y%d"><b>§IDC§</b><span>§ST§</span></div>' % (c, x, r + 1)
                 for r in range(3) for c, x in zip(CELLS[r], ('xa', 'xb', 'xc'))) + '\n    '
       + ''.join('<div class="cl ctr %s">§LV%d§</div>' % (x, k) for k, x in ((1, 'xa'), (2, 'xb'), (3, 'xc'))))

S13_SVG = ('<svg class="full mir" viewBox="0 0 1280 720" aria-hidden="true">'
           + ''.join(arrow_circle(x, 354, 16, 2.5) for x in (335, 643, 951))
           + '<line x1="36" y1="557" x2="1240" y2="557" stroke="#69AEE9" stroke-width="2.5" marker-end="url(#ab)"/></svg>')

S13 = (DECO_S + '\n    ' + HEAD + ttl(13) + '\n    '
       + ''.join('<div class="cd fade z%d"></div>' % k for k in (1, 2, 3, 4)) + '\n    ' + S13_SVG + '\n    '
       + ''.join('<div class="hp ctr w%d">§HH§</div><p class="tp w%d">§IDT2§</p><div class="ob ctr o1 w%d">§ITH§</div><div class="ob ctr o2 w%d">§ITH§</div>\n    '
                 % (k, k, k, k) for k in (1, 2, 3, 4))
       + '<p class="ft">§IDT§</p>')


def ns_shape(x0):
    return ('<rect x="%d" y="218" width="75" height="150" fill="url(#tabg)"/>' % (x0 + 91)
            + '<path d="M%d,285 L%d,390 C%d,404 %d,425 %d,462 L%d,672 L%d,672 Z" fill="url(#nsg)"/>' % (x0, x0 + 223, x0 + 255, x0 + 273, x0 + 273, x0 + 273, x0)
            + '<polygon points="%d,285 %d,218 %d,324" fill="#69AEE9"/>' % (x0, x0 + 91, x0 + 91)
            + '<line x1="%d" y1="285" x2="%d" y2="390" stroke="#fff" stroke-width="2.5"/>' % (x0, x0 + 223))


S14 = (HEAD + ttl(14) + '\n    <svg class="full mir" viewBox="0 0 1280 720" aria-hidden="true">'
       + ''.join(ns_shape(x) for x in (35, 337, 632, 927)) + '</svg>\n    '
       + ''.join('<p class="nn u%d">0%d</p><h3 class="nh v%d">§HTP§</h3><p class="np v%d">§IDT_S§</p>\n    ' % (k, k, k, k) for k in (1, 2, 3, 4)))

S15 = ('<div class="bnd"></div>' + deco('deco15') + '\n    ' + HEAD + '<h1 class="ty">§TY§</h1><p class="cw">§CW§</p>\n    <p class="tp">§SUB§</p>\n    '
       + ''.join('<div class="cn f%d"><div class="icon">%s</div><h3>§CN%d§</h3><p>§IDT§</p></div>' % (k, I[ic], k)
                 for k, ic in ((1, 'mail'), (2, 'phone'), (3, 'city'), (4, 'globe'))) + '\n    '
       '<div class="strip"></div>' + LOGO + '<p class="lgt">§LOGO_T§</p>')

SLIDES = ['\n    ' + s + '\n  ' for s in (S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, S14, S15)]

STR = {
    'ar': {
        'BIZ': 'اسم مؤسستك هنا', 'LOGO_T': 'شعار شركتك هنا',
        'C1': 'الإسناد الخارجي', 'C2': 'لعمليات الأعمال', 'C3': 'قالب عرض تقديمي',
        'PB': 'تقديم', 'PBV': 'يمكنك إضافة اسمك هنا.', 'DT': 'التاريخ', 'DTV': 'أدخل التاريخ هنا.',
        'T2': '<b>جدول</b> الأعمال', 'T3': 'ما هو <b>الإسناد الخارجي لعمليات الأعمال؟</b>', 'T4': 'لماذا تلجأ المؤسسات إلى <b>الإسناد الخارجي</b>',
        'T5': 'العمليات <b>ضمن النطاق</b>', 'T6': '<b>الوضع الحالي</b> مقابل <b>الوضع المستقبلي</b>', 'T7': '<b>تدفق العمليات</b> من البداية إلى النهاية',
        'T8': 'نموذج تشغيل <b>الإسناد الخارجي</b>', 'T9': '<b>نموذج تقديم الخدمات</b>', 'T10': '<b>الحوكمة ومؤشرات الأداء</b>',
        'T11': '<b>روافع خلق القيمة</b>', 'T12': '<b>المخاطر وسبل الحد منها</b>', 'T13': '<b>الانتقال وخارطة الطريق</b> في أربع خطوات',
        'T14': '<b>الخطوات التالية</b>',
        'INTRO': 'أدخل النص المطلوب هنا. هذا نص توضيحي. عبارة موجزة عن موضوعك. هذا نص توضيحي.',
        'SUB': 'أدخل النص المطلوب هنا. هذا نص توضيحي. عبارة موجزة عن موضوعك.',
        'SEC': 'عنوان قسم نائب', 'HH': 'عنوان هنا', 'HTP': 'عنوان رئيسي نائب',
        'IDT': 'أدخل النص المطلوب هنا.', 'IDT2': 'أدخل النص المطلوب هنا. هذا نص توضيحي.', 'IDT_S': 'أدخل النص المطلوب هنا. هذا نص توضيحي.',
        'IDTN': 'أدخل النص المطلوب هنا', 'ITH': 'أدخل النص هنا', 'ST': 'هذا نص توضيحي.', 'IDC': 'أدخل النص المطلوب هنا:',
        'IDP': '(أدخل النص المطلوب هنا)', 'STMT': 'أدخل العبارة الرئيسية هنا. هذا نص توضيحي.',
        'V3_1': '75%', 'V3_2': '$4.6B', 'V3_3': '125M', 'V4': '90%',
        'V8_1': '40%', 'V8_2': '25%', 'V8_3': '35%',
        'V10_1': '&gt;98%', 'V10_2': '4.5/5', 'V10_3': '&lt;$2.50', 'V10_4': '&gt;99.5%',
        'CUR': 'الوضع الحالي', 'FUT': 'الوضع المستقبلي',
        'LG1': 'مخاطر منخفضة', 'LG2': 'مخاطر متوسطة', 'LG3': 'مخاطر مرتفعة', 'LG4': 'مخاطر حرجة',
        'VT': 'أدخل نصك هنا', 'LV1': 'منخفض', 'LV2': 'متوسط', 'LV3': 'مرتفع',
        'TY': 'شكرًا لكم', 'CW': 'تواصل معنا',
        'CN1': 'البريد الإلكتروني', 'CN2': 'رقم التواصل', 'CN3': 'موقع المكتب', 'CN4': 'الموقع الإلكتروني للشركة',
    },
    'en': {
        'BIZ': 'YOUR BUSINESS NAME HERE', 'LOGO_T': 'YOUR COMPANY LOGO HERE',
        'C1': 'BUSINESS PROCESS', 'C2': 'OUTSOURCING', 'C3': 'PRESENTATION TEMPLATE',
        'PB': 'PRESENTED BY', 'PBV': 'You can add your name here.', 'DT': 'DATE', 'DTV': 'Enter date here.',
        'T2': '<b>AGENDA</b> SLIDE', 'T3': 'WHAT IS <b>BUSINESS PROCESS OUTSOURCING?</b>', 'T4': 'WHY ORGANIZATIONS <b>OUTSOURCE</b>',
        'T5': 'PROCESSES <b>IN SCOPE</b>', 'T6': '<b>CURRENT STATE</b> VS. <b>FUTURE STATE</b>', 'T7': '<b>END-TO-END PROCESS FLOW</b> SLIDE',
        'T8': '<b>BPO</b> OPERATING MODEL', 'T9': '<b>SERVICE DELIVERY MODEL</b> SLIDE', 'T10': '<b>GOVERNANCE AND KPIs</b>',
        'T11': '<b>VALUE CREATION LEVERS</b> SLIDE', 'T12': '<b>RISK &amp; MITIGATION</b>', 'T13': '<b>4-STEP TRANSITION &amp; ROADMAP</b> SLIDE',
        'T14': '<b>NEXT STEPS</b> SLIDE',
        'INTRO': 'Insert your desired text here. This is a sample text. A brief statement about your topic. This is a sample text.',
        'SUB': 'Insert your desired text here. This is a sample text. A brief statement about your topic.',
        'SEC': 'Section Title Placeholder', 'HH': 'Heading Here', 'HTP': 'Heading Title Placeholder',
        'IDT': 'Insert your desired text here.', 'IDT2': 'Insert your desired text here. This is a sample text.',
        'IDT_S': 'Insert your desired text here. This is a sample text.',
        'IDTN': 'Insert your desired text here', 'ITH': 'Insert text here', 'ST': 'This is a sample text.', 'IDC': 'Insert your desired text here:',
        'IDP': '(Insert desired text here)', 'STMT': 'Insert your heading statement here. This is a sample text.',
        'V3_1': '75%', 'V3_2': '$4.6B', 'V3_3': '125M', 'V4': '90%',
        'V8_1': '40%', 'V8_2': '25%', 'V8_3': '35%',
        'V10_1': '&gt;98%', 'V10_2': '4.5/5', 'V10_3': '&lt;$2.50', 'V10_4': '&gt;99.5%',
        'CUR': 'CURRENT STATE', 'FUT': 'FUTURE STATE',
        'LG1': 'Low Risk', 'LG2': 'Medium Risk', 'LG3': 'High Risk', 'LG4': 'Critical Risk',
        'VT': 'Insert your text here', 'LV1': 'LOW', 'LV2': 'MEDIUM', 'LV3': 'HIGH',
        'TY': 'THANK YOU', 'CW': 'Connect With Us',
        'CN1': 'Email Address', 'CN2': 'Contact Number', 'CN3': 'Office Location', 'CN4': 'Company Website',
    },
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'ops', 'icon': '🤝',
    'name': 'الإسناد الخارجي لعمليات الأعمال', 'nameEn': 'Business Process Outsourcing',
    'description': 'خمس عشرة شريحة للإسناد الخارجي: الدوافع، النطاق، نموذج التشغيل، الحوكمة، المخاطر، وخارطة الانتقال',
    'descriptionEn': '15 BPO slides: drivers, scope, operating model, governance and KPIs, risk matrix and transition roadmap',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'جدول الأعمال', 'titleEn': 'Agenda'},
        {'title': 'ما هو الإسناد الخارجي (أرقام رئيسية)', 'titleEn': 'What Is BPO (Key Figures)', 'needsData': True},
        {'title': 'لماذا تلجأ المؤسسات إلى الإسناد الخارجي', 'titleEn': 'Why Organizations Outsource', 'needsData': True},
        {'title': 'العمليات ضمن النطاق', 'titleEn': 'Processes in Scope'},
        {'title': 'الوضع الحالي مقابل الوضع المستقبلي', 'titleEn': 'Current State vs. Future State'},
        {'title': 'تدفق العمليات من البداية إلى النهاية', 'titleEn': 'End-to-End Process Flow'},
        {'title': 'نموذج تشغيل الإسناد الخارجي', 'titleEn': 'BPO Operating Model', 'needsData': True},
        {'title': 'نموذج تقديم الخدمات', 'titleEn': 'Service Delivery Model'},
        {'title': 'الحوكمة ومؤشرات الأداء', 'titleEn': 'Governance and KPIs', 'needsData': True},
        {'title': 'روافع خلق القيمة', 'titleEn': 'Value Creation Levers'},
        {'title': 'المخاطر وسبل الحد منها', 'titleEn': 'Risk & Mitigation'},
        {'title': 'الانتقال وخارطة الطريق في أربع خطوات', 'titleEn': '4-Step Transition & Roadmap'},
        {'title': 'الخطوات التالية', 'titleEn': 'Next Steps'},
        {'title': 'شكرًا وتواصل معنا', 'titleEn': 'Thank You & Contact'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والزخرفة المائلة، وغيّر النصوص والأرقام فقط (الأرقام الرئيسية في الشريحة 3، والنسبة المركزية في 4، ونسب نموذج التشغيل في 8، ومؤشرات الحوكمة في 10)؛ ألوان مصفوفة المخاطر ثابتة بدلالتها مع وسيلة الإيضاح.',
}
