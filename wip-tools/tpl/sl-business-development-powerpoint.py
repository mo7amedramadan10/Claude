# -*- coding: utf-8 -*-
# نموذج «تطوير الأعمال»: ثلاث عشرة شريحة بهوية كحلي وأزرق مخضر — غلاف، ملخص تنفيذي، نظرة عامة، فرص السوق،
# العملاء المستهدفون، القيمة المقدَّمة، استراتيجية النمو، مسار المبيعات، المشهد التنافسي، خارطة التنفيذ، مقاييس النجاح، التوصيات، الأيقونات.
KEY = 'sl-business-development-powerpoint'
TITLE = {'ar': 'تطوير الأعمال', 'en': 'Business Development'}
COMMENT = 'نموذج «تطوير الأعمال»: عنوان فرعي أزرق مخضر بخط متدرّج، عنوان كحلي غامق مع كلمة خفيفة، صور بقصّ مائل مدوّر بلون مخضر، بطاقات بيضاء أو كحلية بأيقونات دائرية، ورسوم (أنصاف دوائر السوق، مسار المبيعات، شبكة المربعات) من SVG وكتلة chart-data.'
VARS = '--ink: #001832; --navy: #001832; --accent: #31BFCD; --blue: #367BC6; --deep: #2D5FA8; --dteal: #289197; --text: #595959; --dark: #222222; --light: #F8F8F8; --bg: #FFFFFF;'

FONTS = {'ar': '<link href="https://fonts.googleapis.com/css2?family=Tajawal:wght@300;400;500;700;800&display=swap" rel="stylesheet">',
         'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@300;400;600;700;800&display=swap" rel="stylesheet">'}
FONT_STACK = {'ar': "'Tajawal', 'Segoe UI', Tahoma, sans-serif", 'en': "'Open Sans', 'Segoe UI', Arial, sans-serif"}

# مضلّعات قصّ الصور (بإحداثيات الصورة نفسها؛ تنعكس مع الصورة في العربية)
CLIP_S2 = 'polygon(149px 0px, 31.5px 85.3px, 26.7px 89.1px, 22.2px 93.3px, 18.1px 97.8px, 14.4px 102.7px, 11.1px 107.8px, 8.2px 113.3px, 5.8px 118.9px, 3.9px 124.7px, 2.4px 130.7px, 1.5px 136.7px, 1px 142.8px, 1.1px 149px, 1.6px 155.1px, 2.7px 161.1px, 4.3px 167.1px, 6.4px 172.8px, 8.9px 178.4px, 11.9px 183.8px, 15.3px 188.9px, 19.1px 193.7px, 23.3px 198.1px, 27.9px 202.2px, 32.8px 205.9px, 37.9px 209.2px, 43.4px 212.1px, 49px 214.4px, 762px 482px, 762px 0px)'
CLIP_S12 = 'polygon(119px 0px, 28px 82.7px, 23.6px 87.1px, 19.5px 91.8px, 15.7px 96.7px, 12.4px 101.9px, 9.5px 107.4px, 7px 113.1px, 4.9px 118.9px, 3.3px 124.9px, 2.1px 131px, 1.4px 137.1px, 1.1px 143.3px, 1.4px 149.5px, 2.1px 155.7px, 3.2px 161.8px, 4.8px 167.8px, 6.9px 173.6px, 9.4px 179.3px, 12.3px 184.8px, 15.6px 190px, 19.3px 195px, 23.4px 199.6px, 27.8px 204px, 32.6px 208px, 37.6px 211.6px, 42.9px 214.9px, 48.4px 217.7px, 612px 481px, 612px 0px)'
CLIP_S4 = 'polygon(26px 0px, 21.8px 4.5px, 18px 9.2px, 14.5px 14.2px, 11.4px 19.4px, 8.6px 24.9px, 6.3px 30.5px, 4.4px 36.3px, 2.9px 42.2px, 1.8px 48.2px, 1.2px 54.2px, 1px 60.3px, 1.2px 66.4px, 1.9px 72.4px, 3px 78.4px, 4.6px 84.3px, 6.5px 90.1px, 8.9px 95.7px, 11.7px 101.1px, 14.8px 106.3px, 18.3px 111.3px, 22.2px 116px, 532px 692px, 532px 0px)'
CLIP_S7 = 'polygon(0px 0px, 501.9px 544.5px, 505.9px 549.2px, 509.6px 554.2px, 513.1px 559.4px, 516.3px 564.7px, 519.2px 570.2px, 521.8px 575.8px, 524px 581.6px, 526px 587.5px, 527.7px 593.5px, 529px 599.6px, 530px 605.7px, 530.6px 611.9px, 531px 618.1px, 531px 624.3px, 530.6px 630.5px, 529.9px 636.7px, 528.9px 642.8px, 527.6px 648.9px, 525.9px 654.9px, 523.9px 660.8px, 521.6px 666.5px, 519px 672.2px, 516.1px 677.6px, 512.9px 683px, 509.4px 688.1px, 505.6px 693.1px, 0px 693px)'

STYLE = """  /* عناصر مشتركة */
  .frame.s2, .frame.s6, .frame.s9, .frame.s11 { background: var(--light); }
  .frame.s5, .frame.s10 { background: var(--navy); }
  .sub { position: absolute; top: 49px; inset-inline-start: 62px; margin: 0; font-size: 20px; line-height: 24px; font-weight: 700; color: var(--accent); letter-spacing: .5px; white-space: nowrap; }
  .subl { position: absolute; top: 84px; inset-inline-start: 59px; width: 316px; height: 2px; background: linear-gradient(90deg, var(--navy) 0%, var(--navy) 30%, var(--accent) 72%); }
  .s5 .subl, .s10 .subl { background: linear-gradient(90deg, rgb(49 191 205 / 0) 0%, rgb(49 191 205 / 0) 38%, var(--accent) 80%); }
  html[dir="rtl"] .subl, html[dir="rtl"] .flip { transform: scaleX(-1); }
  .ttl { position: absolute; top: 108px; inset-inline-start: 60px; margin: 0; font-size: 56px; line-height: 64px; font-weight: 700; color: var(--navy); letter-spacing: -.4px; white-space: nowrap; }
  .ttl .l { font-weight: 300; }
  .s5 .ttl, .s10 .ttl { color: #fff; }
  html[lang="ar"] .ttl { font-weight: 800; letter-spacing: 0; line-height: 66px; }
  html[lang="ar"] .ttl .l { font-weight: 300; }
  .p { position: absolute; margin: 0; font-size: 18px; line-height: 26px; color: var(--text); }
  html[lang="ar"] .p { font-size: 19px; line-height: 27px; }
  .dots { position: absolute; background: radial-gradient(circle, var(--accent) 0 2px, transparent 2.6px) 0 0 / 24.6px 24.6px; }
  .dots.w { background-image: radial-gradient(circle, #fff 0 2px, transparent 2.6px); }
  .ic { position: absolute; width: 64px; height: 64px; border-radius: 50%; display: grid; place-items: center; color: #fff; }
  .ic svg { width: 34px; height: 34px; }
  .ic.t { background: var(--accent); } .ic.g { background: linear-gradient(180deg, #38C4D3, #1F8E99); } .ic.dt { background: var(--dteal); }
  .ic.b { background: var(--blue); } .ic.d { background: var(--deep); }
  .ph { position: absolute; display: block; object-fit: cover; }
  .card { position: absolute; background: #fff; }
  .card h2 { position: absolute; margin: 0; font-size: 22px; line-height: 28px; font-weight: 700; color: var(--dark); white-space: nowrap; }
  .card .p { white-space: normal; }
  .n { unicode-bidi: isolate; direction: ltr; display: inline-block; }

  /* 1 — الغلاف */
  .s1 .ph { top: 0; inset-inline-start: 640px; width: 640px; height: 720px; }
  .s1 .ov { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; }
  .s1 .logo { position: absolute; top: 68px; inset-inline-start: 63px; width: 56px; height: 62px; color: var(--accent); }
  .s1 .lg { position: absolute; top: 79px; inset-inline-start: 139px; margin: 0; font-size: 16px; line-height: 23px; letter-spacing: 2.2px; color: #3A3F45; }
  html[lang="ar"] .s1 .lg { letter-spacing: 0; font-size: 18px; }
  .s1 .hero { position: absolute; top: 229px; inset-inline-start: 60px; margin: 0; font-size: 86px; line-height: 94px; font-weight: 700; color: var(--navy); letter-spacing: -.5px; white-space: nowrap; }
  .s1 .hero span { display: block; color: var(--accent); }
  html[lang="ar"] .s1 .hero { font-weight: 800; font-size: 92px; line-height: 96px; letter-spacing: 0; top: 222px; }
  .s1 .tp { position: absolute; top: 443px; inset-inline-start: 62px; margin: 0; font-size: 32px; line-height: 40px; font-weight: 400; color: var(--navy); white-space: nowrap; }
  .s1 .rule { position: absolute; top: 507px; inset-inline-start: 0; width: 745px; height: 3px; background: linear-gradient(90deg, var(--navy) 0%, var(--navy) 22%, var(--accent) 62%); }
  .s1 .p1 { top: 540px; inset-inline-start: 63px; width: 575px; }
  .s1 .meta { position: absolute; top: 624px; margin: 0; font-size: 20px; line-height: 24px; font-weight: 700; color: var(--accent); white-space: nowrap; }
  .s1 .m1 { inset-inline-start: 63px; } .s1 .m2 { inset-inline-start: 333px; }

  /* 2 — الملخص التنفيذي */
  .s2 .ph { top: 0; inset-inline-start: 518px; width: 762px; height: 482px; clip-path: @CLIP_S2@; }
  .s2 .card { inset-inline-start: 575px; width: 604px; border-radius: 10px; }
  .s2 .c1 { top: 98px; height: 139px; } .s2 .c2 { top: 262px; height: 138px; }
  .s2 .card .ic { top: 28px; inset-inline-start: 27px; }
  .s2 .card h2 { top: 20px; inset-inline-start: 108px; }
  .s2 .card .p { top: 61px; inset-inline-start: 108px; width: 425px; }
  .s2 .lp { top: 331px; inset-inline-start: 69px; width: 332px; }
  .s2 .dots { top: 270px; inset-inline-start: 428px; width: 123px; height: 148px; }
  .s2 .st { position: absolute; top: 477px; width: 285px; height: 168px; border-inline-start: 2px solid var(--accent); }
  .s2 .st1 { inset-inline-start: 68px; } .s2 .st2 { inset-inline-start: 367px; } .s2 .st3 { inset-inline-start: 667px; } .s2 .st4 { inset-inline-start: 965px; }
  .s2 .st svg { position: absolute; top: 3px; inset-inline-start: 32px; width: 36px; height: 36px; color: #111; }
  .s2 .st .v { position: absolute; top: 56px; inset-inline-start: 26px; margin: 0; font-size: 64px; line-height: 70px; font-weight: 700; color: var(--accent); white-space: nowrap; }
  .s2 .st .k { position: absolute; top: 137px; inset-inline-start: 26px; margin: 0; font-size: 20.5px; line-height: 26px; font-weight: 700; color: var(--dark); white-space: nowrap; }

  /* 3 — نظرة عامة على الشركة */
  .s3 .pan { position: absolute; top: 27px; inset-inline-start: 835px; width: 445px; height: 665px; background: var(--navy); border-start-start-radius: 24px; border-end-start-radius: 24px; }
  .s3 .ring { position: absolute; top: 101px; inset-inline-start: 706px; width: 519px; height: 519px; border-radius: 50%; background: linear-gradient(180deg, #36C3D2 0%, #2CA9B5 50%, #18737E 100%); }
  .s3 .ph { top: 125px; inset-inline-start: 730px; width: 470px; height: 470px; border-radius: 50%; }
  .s3 .card { width: 479px; height: 138px; border: 1.5px solid #74D0DA; border-radius: 10px; }
  .s3 .c1 { top: 207px; inset-inline-start: 60px; } .s3 .c2 { top: 360px; inset-inline-start: 321px; } .s3 .c3 { top: 514px; inset-inline-start: 582px; }
  .s3 .card .ic { top: 25px; inset-inline-start: 27px; }
  .s3 .card h2 { top: 17px; inset-inline-start: 107px; }
  .s3 .card .p { top: 58px; inset-inline-start: 107px; width: 345px; }
  .s3 .dots { top: 381px; inset-inline-start: 60px; width: 123px; height: 148px; }
  .s3 .bar { position: absolute; top: 596px; inset-inline-start: 68px; width: 2px; height: 52px; background: var(--accent); }
  .s3 .bp { top: 593px; inset-inline-start: 87px; width: 385px; }

  /* 4 — تحليل فرص السوق */
  .s4 .ph { top: 0; inset-inline-start: 748px; width: 532px; height: 720px; clip-path: @CLIP_S4@; }
  .s4 .box { position: absolute; top: 186px; inset-inline-start: 811px; width: 410px; height: 466px; background: var(--navy); border-radius: 14px; }
  .s4 .box h2 { position: absolute; top: 91px; inset-inline-start: 65px; margin: 0; font-size: 22px; line-height: 28px; font-weight: 700; color: #fff; }
  .s4 .box ul { position: absolute; top: 150px; inset-inline-start: 65px; width: 300px; margin: 0; padding: 0; list-style: none; }
  .s4 .box li { position: relative; margin: 0 0 16px; padding-inline-start: 37px; font-size: 20px; line-height: 29px; color: #D3DAE2; }
  .s4 .box li svg { position: absolute; top: 6px; inset-inline-start: 2px; width: 20px; height: 16px; color: var(--accent); }
  .s4 .dots { top: 237px; inset-inline-start: 712px; width: 123px; height: 148px; }
  .s4 .mk { position: absolute; top: 275px; inset-inline-start: 59px; width: 679px; height: 340px; }
  .s4 .lb { position: absolute; width: 230px; margin: 0; margin-inline-start: -115px; text-align: center; white-space: nowrap; }
  .s4 .lb.a { font-size: 18px; line-height: 24px; font-weight: 700; color: #fff; }
  .s4 .lb.v { font-size: 40px; line-height: 48px; font-weight: 700; color: #fff; }
  .s4 .lb.d { font-size: 18px; line-height: 26px; color: var(--text); white-space: normal; }
  .s4 .x1 { inset-inline-start: 195px; } .s4 .x2 { inset-inline-start: 432px; } .s4 .x3 { inset-inline-start: 635px; }
  .s4 .a { top: 521px; } .s4 .v { top: 553px; } .s4 .d { top: 624px; }

  /* 5 — العملاء المستهدفون */
  .s5 .phw { position: absolute; top: 97px; inset-inline-start: 60px; width: 1161px; height: 325px; overflow: hidden; border-start-end-radius: 22px; border-end-end-radius: 22px; }
  .s5 .ph { top: 0; inset-inline-start: 0; width: 100%; height: 100%; }
  .s5 .tb { position: absolute; top: 248px; inset-inline-start: 58px; width: 554px; height: 175px; background: var(--navy); border-start-end-radius: 22px; }
  .s5 .tb .ttl { top: 24px; inset-inline-start: 22px; }
  html[lang="ar"] .s5 .tb .ttl { font-size: 50px; line-height: 62px; top: 22px; }
  .s5 .dots { top: 25px; inset-inline-start: 1145px; width: 123px; height: 148px; }
  .s5 .card { top: 487px; width: 375px; height: 174px; border-radius: 14px; }
  .s5 .c1 { inset-inline-start: 60px; } .s5 .c2 { inset-inline-start: 453px; } .s5 .c3 { inset-inline-start: 846px; }
  .s5 .card .ic { top: -32px; inset-inline-start: 33px; }
  .s5 .card h2 { top: 53px; inset-inline-start: 35px; }
  .s5 .card .p { top: 95px; inset-inline-start: 34px; width: 310px; }

  /* 6 — القيمة المقدَّمة */
  .rt { top: 124px; inset-inline-end: 103px; width: 400px; text-align: end; }
  .rb { position: absolute; top: 127px; inset-inline-start: 1205px; width: 2px; height: 51px; background: var(--accent); }
  .s6 .card { top: 316px; width: 276px; height: 334px; border-radius: 20px; }
  .s6 .c1 { inset-inline-start: 59px; } .s6 .c2 { inset-inline-start: 350px; } .s6 .c3 { inset-inline-start: 640px; } .s6 .c4 { inset-inline-start: 930px; }
  .s6 .card .ic { top: -30px; inset-inline-start: 33px; }
  .s6 .card h2 { top: 98px; inset-inline-start: 30px; line-height: 29px; }
  .s6 .card .p { top: 173px; inset-inline-start: 30px; width: 228px; }
  .s6 .dots { top: 216px; inset-inline-start: 496px; width: 123px; height: 148px; }

  /* 7 — استراتيجية النمو */
  .s7 .ph { top: 27px; inset-inline-start: 0; width: 535px; height: 693px; clip-path: @CLIP_S7@; }
  .s7 .sub { inset-inline-start: 196px; } .s7 .subl { inset-inline-start: 193px; } .s7 .ttl { inset-inline-start: 194px; }
  .s7 .rt { width: 280px; }
  .s7 .card { width: 424px; height: 187px; background: var(--navy); border-radius: 14px; }
  .s7 .c1 { top: 255px; inset-inline-start: 303px; } .s7 .c2 { top: 255px; inset-inline-start: 771px; }
  .s7 .c3 { top: 486px; inset-inline-start: 307px; } .s7 .c4 { top: 486px; inset-inline-start: 775px; }
  .s7 .card .ic { top: -31px; inset-inline-start: 54px; }
  .s7 .card h2 { top: 56px; inset-inline-start: 53px; font-size: 24px; line-height: 30px; color: #fff; }
  .s7 .card .p { top: 97px; inset-inline-start: 53px; width: 300px; color: #CDD5DE; }

  /* 8 — مسار تطوير الأعمال */
  .s8 .rp { position: absolute; top: 0; inset-inline-start: 565px; width: 715px; height: 720px; background: var(--light); }
  .s8 .ic { top: 413px; inset-inline-start: 59px; }
  .s8 .tg { position: absolute; top: 509px; inset-inline-start: 61px; width: 432px; margin: 0; font-size: 32px; line-height: 40px; color: var(--accent); }
  html[lang="ar"] .s8 .tg { font-size: 33px; line-height: 44px; top: 502px; }
  .s8 .dots { top: 616px; inset-inline-start: 498px; width: 123px; height: 99px; }
  .s8 .pipe { position: absolute; top: 44px; inset-inline-start: 640px; width: 363px; height: 634px; }
  .s8 .nv { position: absolute; width: 140px; margin: 0; margin-inline-start: -70px; text-align: center; font-size: 40px; line-height: 48px; font-weight: 700; color: #fff; }
  .s8 .lt { position: absolute; inset-inline-start: 1032px; margin: 0; font-size: 24px; line-height: 30px; font-weight: 700; color: var(--dark); white-space: nowrap; }
  .s8 .ld { inset-inline-start: 1032px; white-space: nowrap; }

  /* 9 — المشهد التنافسي */
  .s9 .dots { top: 37px; inset-inline-start: 1054px; width: 123px; height: 148px; }
  .s9 .cd { position: absolute; top: 223px; inset-inline-start: 59px; width: 1170px; height: 423px; background: #fff; border-radius: 22px; }
  .s9 table { position: absolute; top: 257px; inset-inline-start: 89px; width: 1110px; border-collapse: collapse; table-layout: fixed; }
  .s9 th { height: 70px; padding: 0; padding-inline-start: 19px; background: var(--accent); color: #fff; font-size: 22px; font-weight: 700; text-align: start; }
  .s9 th + th { border-inline-start: 1.5px solid #fff; }
  .s9 td { height: 59px; padding: 0; padding-inline-start: 19px; font-size: 20px; color: #333; text-align: start; }
  .s9 td + td { border-inline-start: 1.5px solid #A8A8A8; }
  .s9 td:nth-child(2) { font-weight: 700; color: #555; } .s9 td:nth-child(n+3) { color: #666; }
  .s9 .bar { position: absolute; top: 664px; inset-inline-start: 81px; width: 2px; height: 34px; background: var(--accent); }
  .s9 .bp { top: 668px; inset-inline-start: 99px; width: 1000px; white-space: nowrap; }

  /* 10 — خارطة طريق التنفيذ */
  .s10 .hl { position: absolute; inset-inline-start: 59px; width: 1162px; height: 1px; background: rgb(255 255 255 / .22); }
  .s10 .dt { position: absolute; top: 228px; margin: 0; font-size: 20px; line-height: 26px; color: #C9D2DC; white-space: nowrap; }
  .s10 .bar { position: absolute; height: 66px; border-radius: 4px; display: flex; align-items: center; padding-inline-start: 26px; font-size: 22px; font-weight: 700; color: var(--dark); white-space: nowrap; }
  .s10 .bar.t { background: var(--accent); color: #1A3440; } .s10 .bar.w { background: #fff; }
  .s10 .bar .bd { position: absolute; top: 17px; inset-inline-end: 16px; height: 37px; padding: 0 12px; background: var(--navy); color: #E6ECF2; font-size: 18px; font-weight: 400; line-height: 37px; border-radius: 2px; }

  /* 11 — مقاييس النجاح */
  .s11 .wf { position: absolute; top: 226px; inset-inline-start: 59px; display: grid; grid-template-columns: repeat(6, 46px); column-gap: 6.2px; row-gap: 8.4px; }
  .s11 .wf i { display: block; width: 46px; height: 46px; }
  .s11 .mv { position: absolute; inset-inline-start: 405px; margin: 0; font-size: 40px; line-height: 48px; font-weight: 700; white-space: nowrap; }
  .s11 .mp { inset-inline-start: 405px; width: 295px; }
  .s11 .pn { position: absolute; top: 35px; inset-inline-start: 758px; width: 489px; height: 651px; background: #fff; border-radius: 30px; }
  .s11 .pt { position: absolute; top: 85px; inset-inline-start: 821px; margin: 0; font-size: 24px; line-height: 30px; font-weight: 700; color: var(--dark); white-space: nowrap; }
  .s11 .no { position: absolute; inset-inline-start: 820px; margin: 0; font-size: 30px; line-height: 36px; font-weight: 700; color: var(--navy); }
  .s11 .dl { position: absolute; inset-inline-start: 837px; width: 2px; background: repeating-linear-gradient(180deg, #5A5A5A 0 12px, transparent 12px 20px); }
  .s11 .np { inset-inline-start: 898px; width: 300px; }

  /* 12 — التوصيات */
  .s12 .ph { top: 0; inset-inline-start: 668px; width: 612px; height: 482px; clip-path: @CLIP_S12@; }
  .s12 .ip { top: 222px; inset-inline-start: 69px; width: 575px; }
  .s12 .vl { position: absolute; top: 324px; width: 2px; height: 310px; background: var(--accent); }
  .s12 .ci { position: absolute; top: 342px; width: 34px; height: 34px; color: var(--accent); }
  .s12 h2 { position: absolute; top: 423px; margin: 0; width: 260px; font-size: 22px; line-height: 29px; font-weight: 700; color: var(--dark); }
  .s12 .cp { top: 510px; width: 222px; }
  .s12 .k1 { inset-inline-start: 69px; } .s12 .k2 { inset-inline-start: 423px; } .s12 .k3 { inset-inline-start: 778px; }
  .s12 .dots { top: 545px; inset-inline-start: 1107px; width: 123px; height: 148px; }

  /* 13 — أيقونات الأعمال */
  .s13 .grid { position: absolute; top: 108px; inset-inline-start: 674px; display: grid; grid-template-columns: repeat(4, 46px); column-gap: 95px; row-gap: 43.6px; color: var(--navy); }
  .s13 .grid svg { width: 46px; height: 46px; }"""

for _k in ('S2', 'S4', 'S7', 'S12'):
    STYLE = STYLE.replace('@CLIP_%s@' % _k, globals()['CLIP_' + _k])

# أيقونات خطية بسيطة (24×24) تُرسم بلون النص
_I = {
    'case_clock': '<path d="M9 7V5.6A1.6 1.6 0 0 1 10.6 4h2.8A1.6 1.6 0 0 1 15 5.6V7"/><path d="M12 19.5H5a2 2 0 0 1-2-2V9a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2v2"/><path d="M3 12.5h8"/><circle cx="17" cy="17" r="4.2"/><path d="M17 15v2.2l1.4 1"/>',
    'box_up': '<rect x="3" y="3" width="18" height="18" rx="4"/><path d="M9 3v4.5h6V3"/><path d="M12 18v-5.5M9.8 14.6 12 12.4l2.2 2.2"/>',
    'runner': '<circle cx="13.5" cy="4.3" r="2"/><path d="M5 9.5l3.6-1.8 4.4.6 1.8 3.4 3.2 1.3"/><path d="M13 8.3l-2.6 5 4.3 2.7-1.2 4"/><path d="M10.4 13.3 7.2 15l-3-1.4"/><path d="M15.5 19.5 21 21.5"/>',
    'store': '<path d="M4 20.5V7.5L15 4v16.5"/><path d="M15 9.5h5v11"/><path d="M3 20.5h18"/><path d="M7.5 20.5v-6h4v6"/><path d="M7 9.5v1.5M11 8.3v1.5M17.5 12.5v5"/>',
    'users': '<circle cx="12" cy="8" r="3.2"/><path d="M6 19.5a6 6 0 0 1 12 0"/><circle cx="5" cy="9.6" r="2.1"/><path d="M1.8 17a3.6 3.6 0 0 1 4.2-3.2"/><circle cx="19" cy="9.6" r="2.1"/><path d="M22.2 17a3.6 3.6 0 0 0-4.2-3.2"/>',
    'chart': '<path d="M3.5 3v17.5H21"/><path d="M6.5 16.5l4-5.5 3.2 3.2 5.3-7.2"/><circle cx="19.4" cy="6.4" r="1.4"/><path d="M6.5 11h2M6.5 7h2M12 19v-2M16 19v-2"/>',
    'grin': '<circle cx="12" cy="12" r="9"/><path d="M7.8 13.2h8.4a4.2 4.2 0 0 1-8.4 0z"/><path d="M8.3 9.6q1.2-1.4 2.4 0M13.3 9.6q1.2-1.4 2.4 0"/>',
    'smile': '<circle cx="12" cy="12" r="9"/><path d="M7.8 13.4c1 1.8 2.5 2.7 4.2 2.7s3.2-.9 4.2-2.7"/><path d="M8.3 10q1.2-1.4 2.4 0M13.3 10q1.2-1.4 2.4 0"/>',
    'user_circle': '<circle cx="12" cy="12" r="9"/><circle cx="12" cy="10" r="3.2"/><path d="M6.3 18.6a6.3 6.3 0 0 1 11.4 0"/>',
    'house_pct': '<path d="M4 11l8-7 8 7v8.2A1.8 1.8 0 0 1 18.2 21H5.8A1.8 1.8 0 0 1 4 19.2z"/><path d="M14.5 12.5l-5 5.5"/><circle cx="9.6" cy="13" r=".8"/><circle cx="14.4" cy="17.5" r=".8"/>',
    'globe': '<circle cx="12" cy="12" r="9"/><path d="M7 4.8l1.4 3.7 3.2.6.8 3-2.2 2.3.6 3.6M15.5 3.6l-1.2 3 2.6 2.2 3.4-.5M15.8 20.5l.5-3.2 3.2-2"/>',
    'fence': '<path d="M3.5 20.5V7.5l2.5-3 2.5 3v13M8.5 20.5V7.5l3-3 3 3v13M14.5 20.5V7.5l3-3 3 3v13"/><path d="M2.5 20.5h19M3.5 12h17"/>',
    'question': '<circle cx="12" cy="12" r="9"/><path d="M9.6 9.6a2.5 2.5 0 1 1 3.6 2.2c-.8.4-1.2 1-1.2 1.8v.6"/><circle cx="12" cy="17.2" r=".6" fill="currentColor"/>',
    'sprout': '<path d="M12 20.5v-8"/><path d="M12 12.5c0-4.3 2.8-6.8 7.4-6.8 0 4.3-2.8 6.8-7.4 6.8z"/><path d="M12 14.5c0-3.4-2.2-5.4-5.8-5.4 0 3.4 2.2 5.4 5.8 5.4z"/><path d="M7 20.5h10"/>',
    'handshake': '<path d="M2 9h3.5l3-2h5l2.5 2"/><path d="M22 15h-3.5l-3.5 2.5h-4L8 15H2"/><path d="M10 9.5l2.5 2.3 3.5-2.3 2.5 1.5H22"/><path d="M8.5 15l-1.5-1.5"/>',
    'tree': '<path d="M12 2.5v19"/><path d="M12 7 9.5 4.5M12 7l2.5-2.5M12 11.5 8 7.5M12 11.5l4-4M12 16 6.5 10.5M12 16l5.5-5.5"/><path d="M7.5 6.5h-2M16.5 6.5h2M5 13H3.5M19 13h1.5"/>',
    'bulb': '<path d="M9.3 18h5.4M10.3 21h3.4"/><path d="M12 3a6 6 0 0 0-3.6 10.8c.7.5 1 1.2 1 2V16h5.2v-.2c0-.8.4-1.5 1-2A6 6 0 0 0 12 3z"/>',
    'pie': '<path d="M10.8 3.6a8.4 8.4 0 1 0 9.6 9.6h-9.6z"/><path d="M14 2.8a7.2 7.2 0 0 1 7.2 7.2H14z"/>',
    'play': '<path d="M3.5 3.5l2.2 2.2M5.7 3.5 3.5 5.7M9.5 2.8l2.2 2.2M11.7 2.8 9.5 5M14.5 8l2.2 2.2M16.7 8l-2.2 2.2"/><path d="M4 15.5c2.8-5.5 8.6-8 15.5-7.2"/><path d="M17.5 6l2.3 2.3-2.3 2.2"/><circle cx="5.5" cy="19.5" r="1.6"/><circle cx="11" cy="19.5" r="1.6"/><circle cx="16.5" cy="19.5" r="1.6"/>',
    'org': '<circle cx="12" cy="4.6" r="2.1"/><circle cx="4.8" cy="19.2" r="2.1"/><circle cx="19.2" cy="19.2" r="2.1"/><path d="M12 6.7V12M6 17.4 12 12l6 5.4M3.5 12h17"/>',
    'shirt': '<path d="M3.5 7.2h17v12.3a1.5 1.5 0 0 1-1.5 1.5H5a1.5 1.5 0 0 1-1.5-1.5z"/><path d="M8.5 7.2 10 3h4l1.5 4.2"/><path d="M10.8 7.2l-.9 4.3L12 14l2.1-2.5-.9-4.3"/><path d="M3.5 12h4M16.5 12h4"/>',
    'calendar': '<rect x="3" y="5" width="18" height="16" rx="3"/><path d="M8 3v4M16 3v4M3 10h18"/><circle cx="8" cy="14" r=".7" fill="currentColor"/><circle cx="12" cy="14" r=".7" fill="currentColor"/><circle cx="16" cy="14" r=".7" fill="currentColor"/><circle cx="8" cy="17.6" r=".7" fill="currentColor"/><circle cx="12" cy="17.6" r=".7" fill="currentColor"/><circle cx="16" cy="17.6" r=".7" fill="currentColor"/>',
    'bricks': '<rect x="3" y="5" width="18" height="14" rx="1"/><path d="M3 9.7h18M3 14.3h18M8 5v4.7M14 5v4.7M11 9.7v4.6M17 9.7v4.6M5.5 9.7v4.6M8 14.3V19M14 14.3V19"/>',
    'mounts': '<path d="M10.3 5.2a2 2 0 0 1 3.4 0l7.2 12.3A2 2 0 0 1 19.2 20.5H4.8a2 2 0 0 1-1.7-3z"/><path d="M6.5 20.5a3 3 0 0 1 5.5-1.7 3 3 0 0 1 5.5 1.7"/><path d="M9.4 14a2.6 2.6 0 0 1 5.2 0"/>',
    'circuit': '<circle cx="12" cy="12" r="9"/><path d="M12 21v-7.4M8.3 20.2v-4.6l-1.6-1.8M15.7 20.2v-4.6l1.6-1.8"/><circle cx="12" cy="11.2" r="1.7"/><circle cx="6.2" cy="12.4" r="1.4"/><circle cx="17.8" cy="12.4" r="1.4"/>',
    'bag': '<path d="M9 3.5h6l-1.6 3h-2.8z"/><path d="M10.2 6.5C6.4 8.6 4.5 12.4 5.3 16c.6 2.9 3 4.5 6.7 4.5s6.1-1.6 6.7-4.5c.8-3.6-1.1-7.4-4.9-9.5"/><path d="M13.6 11.6c-.4-.6-1-.9-1.7-.9-1 0-1.8.6-1.8 1.4 0 1.9 3.8 1 3.8 3 0 .8-.8 1.4-1.8 1.4-.8 0-1.4-.3-1.8-.9M12 9.6v1.1M12 16.5v1.1"/>',
    'badge': '<path d="M12 2.8l2.2 2.3 3.1-.6.8 3.1 2.9 1.4-1.2 2.9 1.2 2.9-2.9 1.4-.8 3.1-3.1-.6L12 21.2l-2.2-2.3-3.1.6-.8-3.1-2.9-1.4 1.2-2.9-1.2-2.9 2.9-1.4.8-3.1 3.1.6z"/><path d="M9 13.2l1.9 1.9 4.1-4.3"/>',
    'yinyang': '<circle cx="12" cy="12" r="9"/><path d="M12 3a4.5 4.5 0 0 1 0 9 4.5 4.5 0 0 0 0 9"/><circle cx="12" cy="7.5" r=".9" fill="currentColor"/><circle cx="12" cy="16.5" r=".9" fill="currentColor"/>',
    'presenter': '<rect x="3" y="4" width="18" height="12.5" rx="2"/><path d="M8 20.5h8M12 16.5v4"/><circle cx="12" cy="8.6" r="1.7"/><path d="M8.8 13.6a3.2 3.2 0 0 1 6.4 0"/>',
}


def ic(name, sw=1.7):
    return ('<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="%s" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">%s</svg>' % (sw, _I[name]))


ARROW = '<svg class="flip" viewBox="0 0 20 16" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M1 5.5h14M1 10.5h14M11.5 1.5 18 8l-6.5 6.5"/></svg>'

GLOBAL_DEFS = ('<defs>'
               '<linearGradient id="bd-gray" x1="0" y1="0" x2="0.55" y2="1"><stop offset="0" stop-color="#7C8695"/><stop offset=".55" stop-color="#C4C8D0"/><stop offset="1" stop-color="#FAFAFB"/></linearGradient>'
               '<linearGradient id="bd-teal" gradientUnits="userSpaceOnUse" x1="0" y1="210" x2="0" y2="720"><stop offset="0" stop-color="#36BFCD"/><stop offset=".55" stop-color="#21909A"/><stop offset="1" stop-color="#12636B"/></linearGradient>'
               '</defs>')


def head(n, sub='SUB', dk=False):
    return '<p class="sub">§%s§</p><div class="subl"></div><h1 class="ttl">§T%d§</h1>' % (sub, n)


def dots(cls=''):
    return '<div class="dots%s" aria-hidden="true"></div>' % cls


# 1 — الغلاف
S1 = ('<img class="ph flip" src="/templates/freeform/sl-business-development-powerpoint/assets/cover.jpg" alt="§ALT1§">'
      '<svg class="ov flip" viewBox="0 0 1280 720" aria-hidden="true">'
      '<polygon points="0,0 673,0 948,346 646,720 0,720" fill="#fff"/>'
      '<polygon points="671,-1 897,-1 1056,209 948,346" fill="url(#bd-gray)"/>'
      '<path fill="url(#bd-teal)" fill-rule="evenodd" d="M1055,210 L1458,720 L645,720 Z M700,721 L989,360 Q1058,268 1121,360 L1403,721 Z"/>'
      '</svg>'
      '<svg class="logo" viewBox="0 0 56 62" aria-hidden="true"><path d="M12 9 Q29 2 46 10 Q53 30 46 50 Q28 59 10 51 Q3 30 12 9Z" fill="none" stroke="currentColor" stroke-width="7.5" stroke-linejoin="round"/></svg>'
      '<p class="lg">§LOGO1§<br>§LOGO2§</p>'
      '<h1 class="hero">§H1A§<span>§H1B§</span></h1>'
      '<p class="tp">§H1C§</p><div class="rule flip"></div>'
      '<p class="p p1">§P1§</p>'
      '<p class="meta m1">§PRES§</p><p class="meta m2">§DATE§</p>')

# 2 — الملخص التنفيذي
def stat(n, icon):
    return ('<div class="st st%d">%s<p class="v"><span class="n">§SV%d§</span></p><p class="k">§SK%d§</p></div>' % (n, ic(icon, 1.6), n, n))

S2 = ('<img class="ph flip" src="/templates/freeform/sl-business-development-powerpoint/assets/meeting.jpg" alt="§ALT2§">'
      + head(2) + '<p class="p lp">§S2P§</p>' + dots()
      + '<div class="card c1"><div class="ic t">' + ic('bag') + '</div><h2>§S2H1§</h2><p class="p">§S2C1§</p></div>'
      + '<div class="card c2"><div class="ic b">' + ic('smile') + '</div><h2>§S2H2§</h2><p class="p">§S2C2§</p></div>'
      + stat(1, 'shirt') + stat(2, 'users') + stat(3, 'globe') + stat(4, 'badge'))

# 3 — نظرة عامة على الشركة
def c3(n, icon):
    return '<div class="card c%d"><div class="ic g">%s</div><h2>§S3H%d§</h2><p class="p">§S3C§</p></div>' % (n, ic(icon), n)

S3 = ('<div class="pan"></div><div class="ring"></div>'
      '<img class="ph flip" src="/templates/freeform/sl-business-development-powerpoint/assets/office.jpg" alt="§ALT3§">'
      + head(3) + dots() + c3(1, 'question') + c3(2, 'yinyang') + c3(3, 'pie')
      + '<div class="bar"></div><p class="p bp">§S3C§</p>')

# 4 — تحليل فرص السوق
S4 = ('<img class="ph flip" src="/templates/freeform/sl-business-development-powerpoint/assets/team.jpg" alt="§ALT4§">'
      + head(4) + dots()
      + '<svg class="mk flip" viewBox="59 275 679 340" aria-hidden="true">'
        '<path d="M59,614 A339,339 0 0 1 737,614 Z" fill="#31BFCD"/>'
        '<path d="M59,614 A237.5,237.5 0 0 1 534,614 Z" fill="#367BC6"/>'
        '<path d="M59,614 A137,137 0 0 1 333,614 Z" fill="#2D5FA8"/></svg>'
      + ''.join('<p class="lb a x%d">§MA%d§</p><p class="lb v x%d"><span class="n">§MV%d§</span></p><p class="lb d x%d">§MD%d§</p>' % (i, i, i, i, i, i) for i in (1, 2, 3))
      + '<div class="box"><h2>§GO§</h2><ul>' + ''.join('<li>%s§B%d§</li>' % (ARROW, i) for i in (1, 2, 3, 4)) + '</ul></div>')

# 5 — العملاء المستهدفون
def c5(n, icon, col):
    return '<div class="card c%d"><div class="ic %s">%s</div><h2>§S5H%d§</h2><p class="p">§S5C§</p></div>' % (n, col, ic(icon), n)

S5 = ('<p class="sub">§SUB§</p><div class="subl"></div>' + dots(' w')
      + '<div class="phw"><img class="ph flip" src="/templates/freeform/sl-business-development-powerpoint/assets/crowd.jpg" alt="§ALT5§"></div>'
      + '<div class="tb"><h1 class="ttl">§T5§</h1></div>'
      + c5(1, 'presenter', 't') + c5(2, 'store', 'b') + c5(3, 'sprout', 'd'))

# 6 — القيمة المقدَّمة
def c6(n, icon, col):
    return '<div class="card c%d"><div class="ic %s">%s</div><h2>§S6H%d§</h2><p class="p">§S6C§</p></div>' % (n, col, ic(icon), n)

S6 = (head(6) + '<p class="p rt">§S6R§</p><div class="rb"></div>'
      + c6(1, 'tree', 't') + c6(2, 'bulb', 'dt') + c6(3, 'sprout', 'b') + c6(4, 'chart', 'd') + dots())

# 7 — استراتيجية النمو
def c7(n, icon, col):
    return '<div class="card c%d"><div class="ic %s">%s</div><h2>§S7H%d§</h2><p class="p">§S7C§</p></div>' % (n, col, ic(icon), n)

S7 = ('<img class="ph flip" src="/templates/freeform/sl-business-development-powerpoint/assets/lounge.jpg" alt="§ALT7§">'
      + head(7) + '<p class="p rt">§S7R§</p><div class="rb"></div>'
      + c7(1, 'handshake', 't') + c7(2, 'mounts', 'dt') + c7(3, 'box_up', 'b') + c7(4, 'sprout', 'd'))

# 8 — مسار تطوير الأعمال: أنصاف قطوع ناقصة متداخلة تشترك في الحافة اليمنى
_PIPE = [(44, 363, '#176067'), (176, 287, '#25909A'), (296, 218, '#31BFCD'), (415, 150, '#367BC6'), (526, 87, '#2D5FA8')]
_NUM = [(928, 120), (934, 251), (942, 369), (950, 481), (961, 600)]
_LAB = [(87, 122), (222, 258), (337, 373), (457, 493), (583, 619)]
S8 = ('<div class="rp"></div>' + head(8)
      + '<div class="ic t">' + ic('case_clock') + '</div><p class="tg">§S8G§</p>' + dots()
      + '<svg class="pipe flip" viewBox="640 44 363 634" aria-hidden="true">'
      + ''.join('<path d="M1003,%d A%d,%g 0 0 0 1003,678 Z" fill="%s"/>' % (t, rx, (678 - t) / 2, c) for t, rx, c in _PIPE)
      + '</svg>'
      + ''.join('<p class="nv" style="top:%dpx; inset-inline-start:%dpx"><span class="n">§PV%d§</span></p>' % (y - 25, x, i + 1) for i, (x, y) in enumerate(_NUM))
      + ''.join('<p class="lt" style="top:%dpx">§PL%d§</p><p class="p ld" style="top:%dpx">§OBJ§</p>' % (a - 16, i + 1, b - 15) for i, (a, b) in enumerate(_LAB)))

# 9 — المشهد التنافسي
_ROWS = [('R1', 'EX', 'GD', 'HI'), ('R2', 'EX', 'GD', 'FA'), ('R3', 'EX', 'FA', 'MO'), ('R4', 'VH', 'HI', 'MO'), ('R5', 'ST', 'MO', 'GD')]
S9 = (head(9) + dots() + '<div class="cd"></div>'
      + '<table><colgroup><col style="width:310px"><col style="width:272px"><col style="width:266px"><col style="width:262px"></colgroup>'
      + '<thead><tr><th>§TH1§</th><th>§TH2§</th><th>§TH3§</th><th>§TH3§</th></tr></thead><tbody>'
      + ''.join('<tr>' + ''.join('<td>§%s§</td>' % c for c in r) + '</tr>' for r in _ROWS)
      + '</tbody></table><div class="bar"></div><p class="p bp">§S9P§</p>')

# 10 — خارطة طريق التنفيذ
_DX = [70, 187, 304, 421, 538, 656, 773, 890, 1007, 1125]
_BARS = [('RA', 't', 298, 59, 254, 'DA'), ('RB', 'w', 298, 327, 342, 'DB'), ('RC', 'w', 405, 477, 341, 'DC'), ('RD', 't', 405, 834, 341, 'DD'),
         ('RE', 't', 512, 408, 254, 'DE'), ('RF', 'w', 619, 545, 254, 'DF'), ('RG', 't', 619, 814, 409, 'DG')]
S10 = ('<p class="sub">§SUB§</p><div class="subl"></div><h1 class="ttl">§T10§</h1>'
       + ''.join('<div class="hl" style="top:%dpx"></div>' % y for y in (225, 385, 492, 599))
       + ''.join('<p class="dt" style="inset-inline-start:%dpx">§D%d§</p>' % (x, i + 1) for i, x in enumerate(_DX))
       + ''.join('<div class="bar %s" style="top:%dpx; inset-inline-start:%dpx; width:%dpx">§%s§<span class="bd">§%s§</span></div>' % (c, t, s, w, k, d) for k, c, t, s, w, d in _BARS))

# 11 — مقاييس النجاح: شبكة المربعات تُرسم من chart-data
_MV = [(254, 301, 'var(--accent)'), (408, 455, 'var(--blue)'), (562, 609, 'var(--deep)')]
S11 = (head(11) + '<div class="wf" id="waffle" aria-hidden="true"></div>'
       + ''.join('<p class="mv" style="top:%dpx; color:%s"><span class="n">§MM%d§</span></p><p class="p mp" style="top:%dpx">§S11P§</p>' % (a - 25, c, i + 1, b - 15) for i, (a, b, c) in enumerate(_MV))
       + '<div class="pn"></div><p class="pt">§PF§</p>'
       + ''.join('<p class="no" style="top:%dpx">0%d</p><p class="p np" style="top:%dpx">§S11N§</p>' % (n - 19, i + 1, t - 15) for i, (n, t) in enumerate([(172, 164), (368, 361), (564, 557)]))
       + '<div class="dl" style="top:208px; height:118px"></div><div class="dl" style="top:403px; height:125px"></div>')

# 12 — التوصيات
S12 = ('<img class="ph flip" src="/templates/freeform/sl-business-development-powerpoint/assets/workshop.jpg" alt="§ALT12§">'
       + '<p class="sub">§SUB§</p><div class="subl"></div><h1 class="ttl">§T12§</h1><p class="p ip">§S12P§</p>'
       + '<div class="vl" style="inset-inline-start:356px"></div><div class="vl" style="inset-inline-start:710px"></div>'
       + ''.join('<div class="ci k%d">%s</div><h2 class="k%d">§S12H%d§</h2><p class="p cp k%d">§S12C§</p>' % (i, ic(n, 1.6), i, i, i) for i, n in ((1, 'runner'), (2, 'circuit'), (3, 'play')))
       + dots())

# 13 — أيقونات الأعمال
_GRID = ['case_clock', 'box_up', 'runner', 'store', 'users', 'chart', 'grin', 'user_circle', 'house_pct', 'globe', 'fence', 'question',
         'sprout', 'handshake', 'tree', 'bulb', 'pie', 'play', 'org', 'shirt', 'calendar', 'bricks', 'mounts', 'circuit']
S13 = ('<p class="sub">§ASSETS§</p><div class="subl"></div><h1 class="ttl">§T13§</h1>'
       + '<div class="grid">' + ''.join(ic(n, 1.6) for n in _GRID) + '</div>')

SLIDES = [S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13]

JSON_BLOCKS = {'chart-data': {'cols': 6, 'groups': [{'cells': 15, 'color': '#31BFCD'}, {'cells': 18, 'color': '#367BC6'}, {'cells': 15, 'color': '#2D5FA8'}]}}

EXTRA_SCRIPT = """  // شبكة المربعات في شريحة مقاييس النجاح: كل مجموعة عدد خلايا بلونها، بالترتيب من جهة البداية
  (function () {
    var box = document.getElementById('waffle'); if (!box) return;
    var d = JSON.parse(document.getElementById('chart-data').textContent);
    box.style.gridTemplateColumns = 'repeat(' + (d.cols || 6) + ', 46px)';
    d.groups.forEach(function (g) { for (var k = 0; k < g.cells; k++) { var c = document.createElement('i'); c.style.background = g.color; box.appendChild(c); } });
  })();"""

_SAMPLE_EN = 'This is a sample text. Add your desired text here.'
_SAMPLE_AR = 'هذا نص توضيحي. أضف النص المطلوب هنا.'

STR = {
    'en': {
        'SUB': 'ADD SUBHEADING HERE', 'ASSETS': 'ASSETS',
        'ALT1': 'Team working in a modern open-plan office', 'ALT2': 'Colleagues meeting in an office', 'ALT3': 'Team working at computer desks',
        'ALT4': 'Business team reviewing documents', 'ALT5': 'Business people walking in a city plaza', 'ALT7': 'Two colleagues talking in an office lounge',
        'ALT12': 'Team discussing around a table',
        'LOGO1': 'YOUR COMPANY', 'LOGO2': 'LOGO HERE', 'H1A': 'BUSINESS', 'H1B': 'DEVELOPMENT', 'H1C': 'PRESENTATION TEMPLATE',
        'P1': 'This is a sample text. Add your desired text here. This is a sample text. This is a sample text. Add your desired text here.',
        'PRES': 'PRESENTER NAME', 'DATE': 'ADD DATE HERE',
        'T2': 'Executive<br>Summary <span class="l">Slide</span>',
        'S2P': 'This is a sample text. Add your desired text here. This is a sample text. Add your desired text here.',
        'S2H1': 'Increase recurring revenue', 'S2H2': 'Improve customer retention',
        'S2C1': 'This is a sample text. Add your desired text here. Add text here. This is a sample text.',
        'S2C2': 'This is a sample text. Add your desired text here. Add text here. This is a sample text.',
        'SV1': '15', 'SV2': '450+', 'SV3': '12', 'SV4': '94%',
        'SK1': 'YEARS EXPERIENCE', 'SK2': 'ENTERPRISE CLIENTS', 'SK3': 'GLOBAL OFFICES', 'SK4': 'CLIENT SATISFACTION',
        'T3': 'Company Overview <span class="l">Slide</span>',
        'S3H1': 'Who We Are', 'S3H2': 'What Makes Us Different', 'S3H3': 'Business Overview',
        'S3C': 'This is a sample text. Add your desired text here. This is a sample text.',
        'T4': 'Market Opportunity<br>Analysis <span class="l">Slide</span>',
        'MA1': 'SOM', 'MA2': 'SAM', 'MA3': 'TAM', 'MV1': '$16B', 'MV2': '$47B', 'MV3': '$124B',
        'MD1': 'Serviceable Obtainable<br>Market', 'MD2': 'Serviceable<br>Addressable Market', 'MD3': 'Total Addressable<br>Market',
        'GO': 'Growth Opportunities', 'B1': 'Add bullet points here.', 'B2': _SAMPLE_EN, 'B3': 'Add your text here. Add your desired text here.', 'B4': 'Add your desired text here.',
        'T5': 'Target Customers<br>&amp; Segments <span class="l">Slide</span>',
        'S5H1': 'Enterprise Organizations', 'S5H2': 'Mid-Market Companies', 'S5H3': 'High-Growth Industries',
        'S5C': 'This is a sample text. Add your desired text here. Add text here.',
        'T6': 'Value Proposition <span class="l">Slide</span>',
        'S6R': 'This is a sample text. Add your desired text here. This is a sample text. Add text here.',
        'S6H1': 'Strategic<br>Expertise', 'S6H2': 'Tailored<br>Solutions', 'S6H3': 'Scalable<br>Growth', 'S6H4': 'Measurable<br>Results',
        'S6C': 'This is a sample text. Add your desired text here. This is a sample text.',
        'T7': 'Growth Strategy <span class="l">Slide</span>', 'S7R': _SAMPLE_EN,
        'S7H1': 'Strategic Partnerships', 'S7H2': 'Channel Expansion', 'S7H3': 'Product Diversification', 'S7H4': 'Geographic Growth',
        'S7C': 'This is a sample text. Add your desired text here. Add text here.',
        'T8': 'Business<br>Development<br>Pipeline <span class="l">Slide</span>',
        'S8G': 'Converting opportunities into revenue across every sales stage.',
        'PV1': '420', 'PV2': '260', 'PV3': '145', 'PV4': '68', 'PV5': '31',
        'PL1': 'Closed Won', 'PL2': 'Negotiation', 'PL3': 'Proposal', 'PL4': 'Qualified Leads', 'PL5': 'Prospecting', 'OBJ': 'Add objective here.',
        'T9': 'Competitive Landscape <span class="l">Slide</span>',
        'TH1': 'Category', 'TH2': 'Our Business', 'TH3': 'Competitors',
        'R1': 'Industry Expertise', 'R2': 'Innovation', 'R3': 'Customer Support', 'R4': 'Scalability', 'R5': 'Partnership Network',
        'EX': 'Excellent', 'VH': 'Very High', 'ST': 'Strong', 'GD': 'Good', 'HI': 'High', 'FA': 'Fair', 'MO': 'Moderate',
        'S9P': 'This is a sample text. Add your desired text here. This is a sample text. Add your desired text here.',
        'T10': 'Implementation Roadmap <span class="l">Slide</span>',
        'D1': 'Feb 15', 'D2': 'Feb 20', 'D3': 'Feb 25', 'D4': 'Feb 30', 'D5': 'Mar 10', 'D6': 'Mar 20', 'D7': 'Mar 30', 'D8': 'Apr 15', 'D9': 'Apr 20', 'D10': 'Apr 30',
        'RA': 'Research', 'RB': 'Validation', 'RC': 'Strategy', 'RD': 'Pipeline Review', 'RE': 'Partner', 'RF': 'Sales', 'RG': 'Final Evaluation',
        'DA': '7 days', 'DB': '15 days', 'DC': '20 days', 'DD': '25 days', 'DE': '10 days', 'DF': '8 days', 'DG': '30 days',
        'T11': 'Success Metrics <span class="l">Slide</span>', 'MM1': '$16B', 'MM2': '$30B', 'MM3': '$25B',
        'S11P': 'This is a sample text. Add your desired text here. Add text here.', 'PF': 'Performance Focus',
        'S11N': 'This is a sample text. Add your desired text here. This is a sample text. Add your desired text here.',
        'T12': 'Recommendations',
        'S12P': 'This is a sample text. Add your desired text here. This is a sample text. Add your desired text here. This is a sample text.',
        'S12H1': 'Finalize Expansion<br>Roadmap', 'S12H2': 'Align Cross-<br>functional Teams', 'S12H3': 'Launch Strategic<br>Initiatives',
        'S12C': 'This is a sample text. Add your desired text here. Add text here. This is a sample text.',
        'T13': 'Business<br>Icons <span class="l">Slide</span>',
    },
    'ar': {
        'SUB': 'أضف عنوانًا فرعيًا هنا', 'ASSETS': 'الأصول',
        'ALT1': 'فريق يعمل في مكتب مفتوح حديث', 'ALT2': 'زملاء في اجتماع داخل مكتب', 'ALT3': 'فريق يعمل على مكاتب الحاسوب',
        'ALT4': 'فريق أعمال يراجع مستندات', 'ALT5': 'رجال أعمال يسيرون في ساحة بالمدينة', 'ALT7': 'زميلان يتحادثان في استراحة المكتب',
        'ALT12': 'فريق يتناقش حول طاولة',
        'LOGO1': 'ضع شعار', 'LOGO2': 'شركتك هنا', 'H1A': 'تطوير', 'H1B': 'الأعمال', 'H1C': 'قالب عرض تقديمي',
        'P1': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. هذا نص توضيحي. أضف النص المطلوب هنا.',
        'PRES': 'اسم المقدِّم', 'DATE': 'أضف التاريخ هنا',
        'T2': '<span class="l">شريحة</span> الملخص<br>التنفيذي',
        'S2P': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا.',
        'S2H1': 'زيادة الإيرادات المتكررة', 'S2H2': 'تحسين الاحتفاظ بالعملاء',
        'S2C1': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصًا هنا. هذا نص توضيحي.',
        'S2C2': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصًا هنا. هذا نص توضيحي.',
        'SV1': '15', 'SV2': '450+', 'SV3': '12', 'SV4': '94%',
        'SK1': 'عامًا من الخبرة', 'SK2': 'عميلًا من المؤسسات', 'SK3': 'مكتبًا حول العالم', 'SK4': 'رضا العملاء',
        'T3': '<span class="l">شريحة</span> نظرة عامة على الشركة',
        'S3H1': 'من نحن', 'S3H2': 'ما الذي يميّزنا', 'S3H3': 'نظرة عامة على الأعمال',
        'S3C': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي.',
        'T4': '<span class="l">شريحة</span> تحليل<br>فرص السوق',
        'MA1': 'الحصة', 'MA2': 'المخدوم', 'MA3': 'الإجمالي', 'MV1': '$16B', 'MV2': '$47B', 'MV3': '$124B',
        'MD1': 'السوق القابل<br>للاستحواذ', 'MD2': 'السوق القابل<br>للخدمة', 'MD3': 'إجمالي السوق<br>المستهدف',
        'GO': 'فرص النمو', 'B1': 'أضف النقاط هنا.', 'B2': _SAMPLE_AR, 'B3': 'أضف نصك هنا. أضف النص المطلوب هنا.', 'B4': 'أضف النص المطلوب هنا.',
        'T5': '<span class="l">شريحة</span> العملاء<br>المستهدفين وفئاتهم',
        'S5H1': 'المؤسسات الكبرى', 'S5H2': 'الشركات المتوسطة', 'S5H3': 'القطاعات سريعة النمو',
        'S5C': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصًا هنا.',
        'T6': '<span class="l">شريحة</span> القيمة المقدَّمة',
        'S6R': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف نصًا هنا.',
        'S6H1': 'خبرة<br>استراتيجية', 'S6H2': 'حلول<br>مصمَّمة حسب الحاجة', 'S6H3': 'نمو<br>قابل للتوسع', 'S6H4': 'نتائج<br>قابلة للقياس',
        'S6C': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي.',
        'T7': '<span class="l">شريحة</span> استراتيجية النمو', 'S7R': _SAMPLE_AR,
        'S7H1': 'الشراكات الاستراتيجية', 'S7H2': 'توسيع القنوات', 'S7H3': 'تنويع المنتجات', 'S7H4': 'التوسع الجغرافي',
        'S7C': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصًا هنا.',
        'T8': '<span class="l">شريحة</span> مسار<br>تطوير<br>الأعمال',
        'S8G': 'تحويل الفرص إلى إيرادات في كل مرحلة من مراحل البيع.',
        'PV1': '420', 'PV2': '260', 'PV3': '145', 'PV4': '68', 'PV5': '31',
        'PL1': 'صفقات مكتملة', 'PL2': 'التفاوض', 'PL3': 'العرض', 'PL4': 'عملاء مؤهَّلون', 'PL5': 'استكشاف العملاء', 'OBJ': 'أضف الهدف هنا.',
        'T9': '<span class="l">شريحة</span> المشهد التنافسي',
        'TH1': 'الفئة', 'TH2': 'أعمالنا', 'TH3': 'المنافسون',
        'R1': 'الخبرة في القطاع', 'R2': 'الابتكار', 'R3': 'دعم العملاء', 'R4': 'قابلية التوسع', 'R5': 'شبكة الشراكات',
        'EX': 'ممتاز', 'VH': 'مرتفع جدًا', 'ST': 'قوي', 'GD': 'جيد', 'HI': 'مرتفع', 'FA': 'مقبول', 'MO': 'متوسط',
        'S9P': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا.',
        'T10': '<span class="l">شريحة</span> خارطة طريق التنفيذ',
        'D1': '15 فبراير', 'D2': '20 فبراير', 'D3': '25 فبراير', 'D4': '30 فبراير', 'D5': '10 مارس', 'D6': '20 مارس', 'D7': '30 مارس', 'D8': '15 أبريل', 'D9': '20 أبريل', 'D10': '30 أبريل',
        'RA': 'البحث', 'RB': 'التحقق', 'RC': 'الاستراتيجية', 'RD': 'مراجعة المسار', 'RE': 'الشركاء', 'RF': 'المبيعات', 'RG': 'التقييم النهائي',
        'DA': '7 أيام', 'DB': '15 يومًا', 'DC': '20 يومًا', 'DD': '25 يومًا', 'DE': '10 أيام', 'DF': '8 أيام', 'DG': '30 يومًا',
        'T11': '<span class="l">شريحة</span> مقاييس النجاح', 'MM1': '$16B', 'MM2': '$30B', 'MM3': '$25B',
        'S11P': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصًا هنا.', 'PF': 'محور الأداء',
        'S11N': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا.',
        'T12': 'التوصيات',
        'S12P': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي.',
        'S12H1': 'إنجاز خارطة<br>طريق التوسع', 'S12H2': 'مواءمة الفرق<br>متعددة الوظائف', 'S12H3': 'إطلاق المبادرات<br>الاستراتيجية',
        'S12C': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصًا هنا. هذا نص توضيحي.',
        'T13': '<span class="l">شريحة</span> أيقونات<br>الأعمال',
    },
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'sales', 'icon': '💼',
    'name': 'تطوير الأعمال', 'nameEn': 'Business Development',
    'description': '13 شريحة لتطوير الأعمال: ملخص، فرص السوق، العملاء، القيمة، النمو، مسار المبيعات، المنافسة، الخارطة، المقاييس',
    'descriptionEn': '13 business development slides: summary, market, customers, value, growth, pipeline, competition, roadmap, metrics',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'الملخص التنفيذي', 'titleEn': 'Executive Summary', 'needsData': True},
        {'title': 'نظرة عامة على الشركة', 'titleEn': 'Company Overview'},
        {'title': 'تحليل فرص السوق', 'titleEn': 'Market Opportunity Analysis', 'needsData': True},
        {'title': 'العملاء المستهدفون وفئاتهم', 'titleEn': 'Target Customers & Segments'},
        {'title': 'القيمة المقدَّمة', 'titleEn': 'Value Proposition'},
        {'title': 'استراتيجية النمو', 'titleEn': 'Growth Strategy'},
        {'title': 'مسار تطوير الأعمال', 'titleEn': 'Business Development Pipeline', 'needsData': True},
        {'title': 'المشهد التنافسي', 'titleEn': 'Competitive Landscape'},
        {'title': 'خارطة طريق التنفيذ', 'titleEn': 'Implementation Roadmap', 'needsData': True},
        {'title': 'مقاييس النجاح', 'titleEn': 'Success Metrics', 'needsData': True},
        {'title': 'التوصيات', 'titleEn': 'Recommendations'},
        {'title': 'أيقونات الأعمال', 'titleEn': 'Business Icons'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والصور وغيّر النصوص والأرقام فقط؛ شبكة المربعات في شريحة مقاييس النجاح تُرسم من كتلة chart-data (عدد الخلايا لكل مجموعة).',
}
