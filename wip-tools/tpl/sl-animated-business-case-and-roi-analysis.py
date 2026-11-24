# -*- coding: utf-8 -*-
# نموذج «دراسة الجدوى وتحليل العائد على الاستثمار»: 13 شريحة على خلفية كحلية بأزرق فاتح
# (غلاف، ملخص تنفيذي، تكلفة التقاعس، نطاق المشروع، ساحة القرار، تحوّل العمليات، تحقيق العائد،
# توزيع الاستثمار، إجمالي تكلفة الملكية، السيناريوهات، حساسية صافي القيمة، بوابات التمويل، الحوكمة).
KEY = 'sl-animated-business-case-and-roi-analysis'
TITLE = {'ar': 'دراسة الجدوى وتحليل العائد على الاستثمار', 'en': 'Business Case & ROI Analysis'}
COMMENT = 'نموذج «دراسة الجدوى وتحليل العائد على الاستثمار»: 13 شريحة بخلفية كحلية (#163B8C) وبطاقات زرقاء (#4E86FF) وبيضاء؛ الرسوم SVG تُرسم من كتل chart-data وchart-data-2، والباقي أشكال CSS/SVG بإحداثيات مطلقة داخل الإطار 1280×720.'
VARS = '--ink: #1F2328; --ink-2: #3A3F47; --muted: #6B7280; --accent: #4E86FF; --navy: #163B8C; --blue-2: #355FCF; --blue-3: #20377D; --sky: #A6CAFF; --sky-2: #9EC5FE; --bg: #163B8C;'

STYLE = """  .frame { color: #fff; }
  .frame > *, .s2 .prob > *, .s2 .kpi > *, .s2 .inv > *, .s2 .gauge > *, .s3 .q > *, .s3 .side > *, .s5 .panel > *, .s7 .panel > *, .s9 .row > *, .s10 .col > *, .s11 .bar > *, .s11 .g > *, .s12 .ph > *, .s13 .gc > * { position: absolute; }
  .ttl { top: 48px; inset-inline-start: 80px; margin: 0; font-size: 50px; font-weight: 700; line-height: 1.2; color: #fff; }
  .frame p, .frame h1, .frame h2, .frame h3 { margin: 0; }
  .wh { background: #fff; color: var(--ink); }
  .card { background: var(--accent); }
  /* الشريحة 1: الغلاف */
  .s1 .deco { inset: 0; clip-path: polygon(54% 0, 100% 0, 100% 82%); background: repeating-linear-gradient(45deg, rgb(0 0 0 / .12) 0 7px, rgb(0 0 0 / .04) 7px 60px, rgb(0 0 0 / 0) 60px 118px); }
  html[dir="rtl"] .s1 .deco { transform: scaleX(-1); }
  .s1 .logo { top: 112px; inset-inline-start: 120px; width: 56px; height: 58px; color: #A9C9FF; }
  .s1 .co { top: 120px; inset-inline-start: 202px; font-size: 18px; letter-spacing: 2.5px; line-height: 1.3; text-transform: uppercase; }
  html[lang="ar"] .s1 .co { letter-spacing: 0; font-size: 20px; }
  .s1 .t1 { top: 270px; inset-inline-start: 118px; font-size: 92px; font-weight: 800; line-height: 1; letter-spacing: -1px; text-transform: uppercase; white-space: nowrap; }
  html[lang="ar"] .s1 .t1 { font-size: 78px; letter-spacing: 0; }
  .s1 .t2 { top: 378px; inset-inline-start: 120px; font-size: 68px; font-weight: 300; line-height: 1; letter-spacing: 1px; text-transform: uppercase; white-space: nowrap; }
  html[lang="ar"] .s1 .t2 { font-size: 50px; font-weight: 400; }
  .s1 .p { top: 544px; inset-inline-start: 120px; width: 745px; font-size: 20px; line-height: 1.3; color: #D5DEF5; }
  /* الشريحة 2: الملخص التنفيذي */
  .s2 .prob { top: 155px; inset-inline-start: 65px; width: 757px; height: 175px; }
  .s2 .ico { width: 44px; height: 44px; background: #fff; color: var(--accent); display: grid; place-items: center; }
  .s2 .ico svg { width: 26px; height: 26px; }
  .s2 .prob .ico { top: 13px; inset-inline-start: 32px; }
  .s2 .prob h2 { top: 75px; inset-inline-start: 32px; font-size: 26px; font-weight: 400; letter-spacing: 4px; line-height: 1.25; width: 170px; text-transform: uppercase; }
  html[lang="ar"] .s2 .prob h2 { letter-spacing: 0; font-weight: 700; }
  .s2 .prob .big { top: 52px; inset-inline-start: 222px; font-size: 24px; line-height: 1.2; white-space: nowrap; }
  .s2 .prob .big b { font-weight: 700; }
  .s2 .prob .sm { top: 100px; inset-inline-start: 222px; font-size: 14px; color: #E6EEFF; white-space: nowrap; }
  .s2 .kpi { width: 367px; height: 160px; }
  .s2 .k1 { top: 345px; inset-inline-start: 65px; } .s2 .k2 { top: 345px; inset-inline-start: 453px; }
  .s2 .k3 { top: 520px; inset-inline-start: 65px; } .s2 .k4 { top: 520px; inset-inline-start: 453px; }
  .s2 .kpi .v { top: 30px; inset-inline-start: 34px; font-size: 46px; font-weight: 700; color: var(--accent); line-height: 1.1; }
  .s2 .kpi .l { top: 108px; inset-inline-start: 34px; font-size: 22px; color: var(--ink-2); text-transform: uppercase; white-space: nowrap; }
  html[lang="ar"] .s2 .kpi .l { font-size: 21px; }
  .s2 .inv { top: 155px; inset-inline-start: 835px; width: 380px; height: 525px; }
  .s2 .inv .ico { top: 20px; inset-inline-start: 27px; background: var(--accent); color: #fff; }
  .s2 .inv h2 { top: 80px; inset-inline-start: 27px; font-size: 26px; font-weight: 700; text-transform: uppercase; }
  .s2 .inv .p { top: 128px; inset-inline-start: 27px; width: 300px; font-size: 18px; line-height: 1.25; color: var(--muted); }
  .s2 .mini { top: 223px; width: 102px; height: 63px; background: var(--accent); color: #fff; text-align: center; padding-top: 9px; }
  .s2 .mini b { display: block; font-size: 22px; line-height: 1.1; }
  .s2 .mini span { display: block; font-size: 14px; color: #DCE7FF; }
  .s2 .m1 { inset-inline-start: 20px; } .s2 .m2 { inset-inline-start: 134px; } .s2 .m3 { inset-inline-start: 246px; }
  .s2 .gauge { top: 297px; inset-inline-start: 18px; width: 330px; height: 193px; background: var(--accent); color: #fff; }
  .s2 .gauge svg { position: absolute; inset: 0; width: 330px; height: 193px; }
  .s2 .gauge .go { top: 90px; inset-inline-start: 0; width: 100%; text-align: center; font-size: 46px; font-weight: 300; line-height: 1; }
  html[lang="ar"] .s2 .gauge .go { font-size: 38px; font-weight: 500; top: 94px; }
  .s2 .gauge .dc { top: 145px; inset-inline-start: 0; width: 100%; text-align: center; font-size: 15px; letter-spacing: 2px; color: #D5E3FF; text-transform: uppercase; }
  html[lang="ar"] .s2 .gauge .dc { letter-spacing: 0; }
  /* الشريحة 3: تكلفة التقاعس */
  .s3 .hd { top: 165px; width: 200px; font-size: 20px; font-weight: 700; line-height: 1.25; text-transform: uppercase; }
  .s3 .h1 { inset-inline-start: 80px; } .s3 .h2 { inset-inline-start: 300px; } .s3 .h3 { inset-inline-start: 500px; } .s3 .h4 { inset-inline-start: 702px; }
  .s3 .rule { top: 233px; inset-inline-start: 75px; width: 806px; height: 2px; background: #fff; }
  .s3 .ln { inset-inline-start: 75px; width: 837px; height: 1px; background: rgb(255 255 255 / .35); }
  .s3 .q { width: 201px; }
  .s3 .q1 { top: 265px; inset-inline-start: 75px; height: 88px; } .s3 .q2 { top: 333px; inset-inline-start: 277px; height: 95px; }
  .s3 .q3 { top: 416px; inset-inline-start: 478px; height: 111px; } .s3 .q4 { top: 527px; inset-inline-start: 680px; height: 121px; }
  .s3 .q .n { top: 12px; inset-inline-end: 16px; font-size: 18px; color: var(--ink-2); }
  .s3 .q .v { bottom: 14px; inset-inline-start: 24px; font-size: 30px; font-weight: 700; color: var(--accent); line-height: 1; }
  .s3 .plus { width: 34px; height: 34px; border-radius: 50%; background: var(--accent); color: #fff; font-size: 26px; line-height: 32px; text-align: center; font-weight: 300; }
  .s3 .p1 { top: 338px; inset-inline-start: 258px; } .s3 .p2 { top: 410px; inset-inline-start: 460px; } .s3 .p3 { top: 511px; inset-inline-start: 662px; }
  .s3 .side { top: 148px; inset-inline-start: 913px; width: 292px; height: 502px; }
  .s3 .side h2 { top: 112px; inset-inline-start: 32px; width: 240px; font-size: 24px; font-weight: 700; line-height: 1.3; text-transform: uppercase; }
  .s3 .side p { top: 188px; inset-inline-start: 32px; width: 225px; font-size: 17px; line-height: 1.35; color: #DCE7FF; }
  .s3 .side .big { top: 414px; inset-inline-start: 32px; font-size: 58px; font-weight: 700; line-height: 1; }
  /* الشريحة 4: نطاق المشروع */
  .s4 { background: var(--accent); }
  .s4 .lead { top: 70px; inset-inline-start: 843px; width: 370px; font-size: 20px; line-height: 1.25; }
  .s4 .lg { inset-inline-start: 80px; font-size: 20px; display: flex; align-items: center; gap: 10px; }
  .s4 .lg i { display: inline-block; width: 18px; height: 18px; border-radius: 50%; }
  .s4 .lg1 { top: 150px; } .s4 .lg1 i { background: var(--blue-2); }
  .s4 .lg2 { top: 183px; } .s4 .lg2 i { background: #fff; }
  .s4 .lg3 { top: 226px; } .s4 .lg3 i { border: 1.5px dashed #fff; }
  .s4 .orbit { inset: 0; width: 1280px; height: 720px; }
  .s4 .core { top: 292px; inset-inline-start: 525px; width: 230px; height: 230px; border-radius: 50%; background: var(--blue-2); display: grid; place-items: center; text-align: center; font-size: 28px; font-weight: 700; line-height: 1.3; text-transform: uppercase; }
  .s4 .dom { width: 180px; height: 180px; border-radius: 50%; border: 1.5px dashed #fff; background: var(--accent); display: grid; place-items: center; text-align: center; font-size: 20px; line-height: 1.25; padding: 0 20px; }
  .s4 .d1 { top: 362px; inset-inline-start: 110px; } .s4 .d2 { top: 228px; inset-inline-start: 973px; } .s4 .d3 { top: 458px; inset-inline-start: 976px; }
  /* الشريحة 5: ساحة القرار */
  .s5 { background: radial-gradient(ellipse 60% 65% at 50% 42%, #3A64D4 0%, #2C52BE 40%, #1B3F9A 80%, #163B8C 100%); }
  .s5 .grid { top: 100px; inset-inline-start: 100px; width: 1080px; height: 470px; background-image: linear-gradient(rgb(255 255 255 / .09) 1px, transparent 1px), linear-gradient(90deg, rgb(255 255 255 / .09) 1px, transparent 1px); background-size: 150px 70px; background-position: 75px 20px; }
  .s5 .ttl { top: 604px; font-size: 50px; }
  .s5 .bub { border-radius: 50%; background: var(--accent); display: grid; place-content: center; text-align: center; }
  .s5 .bub b { display: block; font-weight: 700; line-height: 1.2; }
  .s5 .bub span { display: block; color: #E6EEFF; line-height: 1.2; }
  .s5 .ba { top: 373px; inset-inline-start: 184px; width: 126px; height: 126px; } .s5 .ba b { font-size: 24px; } .s5 .ba span { font-size: 20px; }
  .s5 .bb { top: 305px; inset-inline-start: 376px; width: 104px; height: 104px; } .s5 .bb b { font-size: 20px; } .s5 .bb span { font-size: 16px; }
  .s5 .bc { top: 172px; inset-inline-start: 704px; width: 126px; height: 126px; opacity: .55; padding-top: 18px; } .s5 .bc b { font-size: 24px; } .s5 .bc span { font-size: 20px; }
  .s5 .bc svg { position: absolute; top: 12px; inset-inline-start: 52px; width: 22px; height: 22px; color: #fff; }
  .s5 .bd { top: 204px; inset-inline-start: 960px; width: 196px; height: 196px; } .s5 .bd b { font-size: 32px; } .s5 .bd span { font-size: 24px; }
  .s5 .panel { top: 437px; inset-inline-start: 720px; width: 457px; height: 145px; }
  .s5 .panel h2 { top: 78px; inset-inline-start: 25px; font-size: 22px; font-weight: 700; line-height: 1.2; color: var(--ink); }
  html[lang="ar"] .s5 .panel h2 { top: 56px; width: 120px; }
  .s5 .panel svg { position: absolute; top: 12px; inset-inline-start: 148px; width: 300px; height: 125px; }
  .s5 .panel .lab { font-size: 15px; fill: var(--ink-2); }
  .s5 .tot { top: 616px; inset-inline-end: 100px; font-size: 20px; font-weight: 700; text-transform: uppercase; white-space: nowrap; }
  /* الشريحة 6: تحوّل العمليات */
  .s6 .ttl { inset-inline-start: 0; width: 100%; text-align: center; top: 68px; font-size: 46px; }
  .s6 .beams { inset: 0; width: 1280px; height: 720px; }
  html[dir="rtl"] .s6 .beams { transform: scaleX(-1); }
  .s6 .c { top: 185px; width: 140px; height: 140px; border-radius: 50%; background: var(--accent); display: grid; place-items: center; font-size: 28px; font-weight: 700; }
  .s6 .c1 { inset-inline-start: 302px; } .s6 .c2 { inset-inline-start: 499px; } .s6 .c3 { inset-inline-start: 666px; } .s6 .c4 { inset-inline-start: 863px; }
  .s6 .big { width: 240px; text-align: center; font-size: 48px; font-weight: 700; line-height: 1; }
  .s6 .lab { width: 240px; text-align: center; font-size: 20px; color: #B9C9F2; }
  .s6 .v1 { top: 462px; inset-inline-start: 63px; } .s6 .v2 { top: 474px; inset-inline-start: 380px; } .s6 .v3 { top: 474px; inset-inline-start: 685px; } .s6 .v4 { top: 462px; inset-inline-start: 993px; }
  .s6 .l1 { top: 553px; inset-inline-start: 65px; } .s6 .l2 { top: 563px; inset-inline-start: 380px; } .s6 .l3 { top: 563px; inset-inline-start: 683px; } .s6 .l4 { top: 553px; inset-inline-start: 1000px; }
  .s6 .tf { top: 290px; inset-inline-start: 1018px; width: 220px; text-align: center; font-size: 20px; letter-spacing: 2px; text-transform: uppercase; transform: rotate(55deg); white-space: nowrap; }
  html[dir="rtl"] .s6 .tf { transform: rotate(-55deg); letter-spacing: 0; }
  /* الشريحة 7: تحقيق العائد */
  .s7 .ttl { top: 604px; font-size: 50px; }
  .s7 .li { inset-inline-start: 82px; width: 165px; font-size: 20px; line-height: 1.2; }
  .s7 .li1 { top: 157px; } .s7 .li2 { top: 275px; } .s7 .li3 { top: 388px; } .s7 .li4 { top: 505px; }
  .s7 .panel { top: 50px; inset-inline-start: 555px; width: 495px; height: 605px; background: linear-gradient(180deg, #C9D3E8 0%, #F6F7FB 60%, #E8ECF6 100%); color: #2B3A6B; }
  .s7 .panel .band { inset-inline-start: 0; width: 100%; height: 145px; background: var(--sky-2); }
  .s7 .panel .b1 { top: 0; } .s7 .panel .b2 { top: 495px; height: 110px; }
  .s7 .panel h2 { inset-inline-start: 32px; font-size: 20px; font-weight: 700; }
  .s7 .panel .sub { top: 58px; inset-inline-start: 32px; font-size: 19px; color: #4A5A8A; }
  .s7 .panel .amt { inset-inline-end: 38px; font-size: 35px; font-weight: 700; line-height: 1; }
  .s7 .panel .h1 { top: 32px; } .s7 .panel .a1 { top: 32px; } .s7 .panel .h2 { top: 546px; } .s7 .panel .a2 { top: 536px; }
  .s7 .links { inset: 0; width: 1280px; height: 720px; }
  html[dir="rtl"] .s7 .links { transform: scaleX(-1); }
  .s7 .bx { width: 182px; height: 56px; border-radius: 6px; background: var(--accent); color: #fff; font-size: 12.5px; line-height: 1.3; display: flex; align-items: center; text-align: start; padding: 0 20px; }
  .s7 .bx.r4 { width: 210px; }
  /* الشريحة 8: توزيع الاستثمار */
  .s8 .left { top: 0; inset-inline-start: 0; width: 676px; height: 720px; background: #1F3B8F; }
  .s8 .right { top: 0; inset-inline-start: 676px; width: 604px; height: 720px; background: linear-gradient(180deg, #DEE2ED 0%, #F4F5F9 60%, #ECEEF5 100%); color: #2B3A6B; }
  .s8 .mnt { top: 0; inset-inline-start: 0; width: 676px; height: 720px; }
  html[dir="rtl"] .s8 .mnt { transform: scaleX(-1); }
  .s8 .ttl { top: 60px; line-height: 1.25; }
  .s8 .card { top: 267px; inset-inline-start: 178px; width: 227px; height: 133px; border-radius: 10px; background: #fff; color: var(--ink-2); text-align: center; padding-top: 20px; }
  .s8 .card b { display: block; font-size: 36px; font-weight: 400; line-height: 1.1; }
  .s8 .card span { display: block; font-size: 16px; line-height: 1.3; color: var(--muted); padding: 8px 42px 0; }
  .s8 .qq { top: 643px; font-size: 38px; font-weight: 300; }
  .s8 .q1 { inset-inline-start: 62px; } .s8 .q2 { inset-inline-start: 408px; }
  .s8 .rh { top: 62px; inset-inline-end: 52px; font-size: 36px; font-weight: 300; color: #3A4A7A; }
  .s8 .rt { top: 114px; inset-inline-end: 55px; font-size: 18px; color: #3A4A7A; }
  .s8 .bub { border-radius: 50%; color: #fff; display: grid; place-content: center; text-align: center; }
  .s8 .bub b { display: block; font-weight: 300; line-height: 1.1; }
  .s8 .bub span { display: block; line-height: 1.3; padding-top: 4px; }
  .s8 .u1 { top: 142px; inset-inline-start: 718px; width: 216px; height: 216px; background: var(--accent); } .s8 .u1 b { font-size: 36px; } .s8 .u1 span { font-size: 15px; }
  .s8 .u6 { top: 432px; inset-inline-start: 685px; width: 276px; height: 276px; background: var(--accent); } .s8 .u6 b { font-size: 48px; } .s8 .u6 span { font-size: 18px; }
  .s8 .u7 { top: 410px; inset-inline-start: 963px; width: 310px; height: 310px; background: #325CD4; } .s8 .u7 b { font-size: 48px; } .s8 .u7 span { font-size: 18px; }
  .s8 .u3 { top: 218px; inset-inline-start: 1096px; width: 184px; height: 184px; background: var(--sky); color: #3A4A7A; } .s8 .u3 b { font-size: 32px; } .s8 .u3 span { font-size: 15px; }
  .s8 .u2 { top: 232px; inset-inline-start: 978px; width: 120px; height: 120px; background: var(--accent); } .s8 .u2 b { font-size: 24px; } .s8 .u2 span { font-size: 12px; }
  .s8 .u4 { top: 331px; inset-inline-start: 675px; width: 124px; height: 124px; background: var(--blue-2); } .s8 .u4 b { font-size: 24px; } .s8 .u4 span { font-size: 12px; }
  .s8 .u5 { top: 316px; inset-inline-start: 867px; width: 158px; height: 158px; background: #9DC5FF; color: #2B3A6B; } .s8 .u5 b { font-size: 32px; } .s8 .u5 span { font-size: 15px; }
  /* الشريحة 9: إجمالي تكلفة الملكية */
  .s9 .sea { top: 305px; inset-inline-start: 0; width: 1280px; height: 415px; background: var(--accent); }
  .s9 .cap { inset-inline-start: 93px; font-size: 24px; letter-spacing: 2px; text-transform: uppercase; }
  html[lang="ar"] .s9 .cap { letter-spacing: 0; }
  .s9 .sum { inset-inline-start: 250px; font-size: 40px; font-weight: 300; line-height: 1; }
  .s9 .c1 { top: 78px; } .s9 .s1 { top: 70px; } .s9 .c2 { top: 615px; } .s9 .s2 { top: 607px; }
  .s9 .row { inset-inline-start: 80px; width: 290px; height: 47px; border-radius: 3px; }
  .s9 .row p { top: 9px; inset-inline-start: 23px; width: 108px; font-size: 12px; line-height: 1.25; color: var(--ink); }
  .s9 .row span { top: 10px; inset-inline-start: 205px; font-size: 22px; color: var(--accent); }
  .s9 .berg { inset: 0; width: 1280px; height: 720px; }
  .s9 .pct { inset-inline-start: 583px; width: 115px; height: 62px; border-radius: 3px; display: grid; place-items: center; font-size: 34px; color: var(--accent); }
  .s9 .pc1 { top: 225px; } .s9 .pc2 { top: 323px; }
  .s9 .chart { top: 78px; inset-inline-start: 872px; width: 356px; height: 440px; }
  .s9 .tco { top: 108px; inset-inline-end: 68px; font-size: 24px; font-weight: 300; text-transform: uppercase; }
  .s9 .ttl { top: 590px; inset-inline-start: auto; inset-inline-end: 55px; font-size: 50px; }
  /* الشريحة 10: تخطيط السيناريوهات */
  .s10 .col { top: 148px; width: 211px; height: 502px; }
  .s10 .col h2 { top: 18px; inset-inline-start: 20px; width: 170px; font-size: 20px; font-weight: 700; line-height: 1.25; text-transform: uppercase; }
  .s10 .col i { top: 74px; inset-inline-start: 20px; width: 171px; height: 1px; background: #A9C9FF; }
  .s10 .col p { top: 88px; inset-inline-start: 20px; width: 158px; font-size: 17px; line-height: 1.35; color: #EAF1FF; }
  .s10 .half { inset-inline-start: 13px; width: 184px; height: 92px; border-radius: 184px 184px 0 0; background: #fff; display: grid; place-items: center; font-size: 32px; padding-top: 18px; }
  .s10 .half.dk { background: var(--blue-3); color: #fff; }
  .s10 .h1 { top: 207px; } .s10 .h2 { top: 302px; } .s10 .h3 { top: 399px; }
  .s10 .co1 { inset-inline-start: 78px; } .s10 .co2 { inset-inline-start: 309px; } .s10 .co3 { inset-inline-start: 540px; } .s10 .co4 { inset-inline-start: 771px; } .s10 .co5 { inset-inline-start: 1002px; }
  /* الشريحة 11: تحليل الحساسية */
  .s11 .bar { top: 148px; inset-inline-start: 77px; width: 988px; height: 54px; border-radius: 6px; }
  .s11 .bar span { top: 0; line-height: 54px; font-size: 20px; color: var(--ink-2); white-space: nowrap; }
  .s11 .bar .x1 { inset-inline-start: 28px; } .s11 .bar .x2 { inset-inline-start: 270px; } .s11 .bar .x3 { inset-inline-start: 530px; } .s11 .bar .x4 { inset-inline-start: 796px; }
  .s11 .axis { top: 428px; inset-inline-start: 270px; width: 935px; height: 5px; background: #fff; }
  .s11 .g { top: 240px; width: 196px; height: 382px; background: #fff; border-start-end-radius: 196px; border-end-end-radius: 196px; }
  .s11 .g1 { inset-inline-start: 77px; } .s11 .g2 { inset-inline-start: 342px; } .s11 .g3 { inset-inline-start: 606px; } .s11 .g4 { inset-inline-start: 871px; }
  .s11 .up { inset-inline-start: 0; bottom: 191px; background: var(--blue-2); border-start-end-radius: 999px; }
  .s11 .dn { inset-inline-start: 0; top: 191px; background: var(--accent); border-end-end-radius: 999px; }
  .s11 .g .v { inset-inline-start: 28px; font-size: 26px; color: #fff; line-height: 1; }
  .s11 .g .vu { top: 139px; } .s11 .g .vd { top: 211px; }
  .s11 .sw { inset-inline-start: 1085px; font-size: 48px; font-weight: 300; line-height: 1; }
  .s11 .swl { inset-inline-start: 1123px; font-size: 18px; letter-spacing: 1.5px; color: #DCE7FF; text-transform: uppercase; }
  html[lang="ar"] .s11 .swl { letter-spacing: 0; }
  .s11 .sw1 { top: 270px; } .s11 .sl1 { top: 328px; } .s11 .sl2 { top: 520px; } .s11 .sw2 { top: 560px; }
  /* الشريحة 12: خارطة طريق بوابات التمويل */
  .s12 .ph { color: #fff; }
  .s12 .ph small { top: 20px; inset-inline-start: 18px; font-size: 14px; color: #14235C; }
  .s12 .ph span { inset-inline-end: 30px; font-size: 36px; font-weight: 300; line-height: 1; }
  .s12 .p1 { top: 148px; inset-inline-start: 412px; width: 456px; height: 100px; background: var(--accent); } .s12 .p1 span { top: 32px; }
  .s12 .p2 { top: 250px; inset-inline-start: 437px; width: 406px; height: 99px; background: #4277E7; } .s12 .p2 span { top: 30px; }
  .s12 .p3 { top: 350px; inset-inline-start: 462px; width: 356px; height: 97px; background: #3D70DD; } .s12 .p3 span { top: 30px; }
  .s12 .p4 { top: 449px; inset-inline-start: 487px; width: 306px; height: 100px; background: #3869D2; } .s12 .p4 span { top: 32px; }
  .s12 .p5 { top: 551px; inset-inline-start: 511px; width: 258px; height: 100px; background: #3360C7; } .s12 .p5 span { top: 32px; }
  .s12 .ln { inset-inline-start: 78px; width: 1127px; height: 2px; background: #fff; }
  .s12 .gate { inset-inline-start: 78px; width: 79px; height: 32px; background: #CBD6E8; color: var(--ink-2); font-size: 14px; letter-spacing: 1px; text-align: center; line-height: 32px; text-transform: uppercase; white-space: nowrap; }
  html[lang="ar"] .s12 .gate { letter-spacing: 0; }
  .s12 .amt { inset-inline-start: 80px; font-size: 26px; font-weight: 700; line-height: 1; }
  .s12 .rh { inset-inline-end: 75px; font-size: 20px; font-weight: 700; text-transform: uppercase; text-align: end; }
  .s12 .rp { inset-inline-end: 75px; font-size: 17px; color: #D8E4FF; text-align: end; }
  /* الشريحة 13: الحوكمة */
  .s13 .vt { top: 332px; inset-inline-start: -144px; width: 520px; height: 60px; text-align: center; font-size: 64px; font-weight: 700; line-height: 60px; letter-spacing: 0; text-transform: uppercase; transform: rotate(-90deg); white-space: nowrap; }
  html[dir="rtl"] .s13 .vt { transform: rotate(90deg); letter-spacing: 0; }
  .s13 .hd { background: var(--accent); border-radius: 50% 50% 0 0 / 100% 100% 0 0; display: flex; align-items: center; justify-content: center; text-align: center; line-height: 1.25; text-transform: uppercase; }
  .s13 .hd1 { top: 61px; inset-inline-start: 269px; width: 326px; height: 92px; font-size: 18px; padding-top: 6px; }
  .s13 .hd2 { top: 154px; inset-inline-start: 252px; width: 360px; height: 124px; font-size: 23px; padding-top: 8px; }
  .s13 .hd3 { top: 278px; inset-inline-start: 227px; width: 410px; height: 169px; font-size: 26px; padding-top: 10px; }
  .s13 .hd4 { top: 448px; inset-inline-start: 227px; width: 412px; height: 210px; font-size: 31px; padding-top: 40px; }
  .s13 .pill { height: 33px; border-radius: 4px; display: flex; align-items: center; gap: 8px; padding: 0 14px; font-size: 14px; text-transform: uppercase; white-space: nowrap; }
  .s13 .pm { inset-inline-start: 742px; width: 113px; background: var(--blue-2); }
  .s13 .pm svg { width: 18px; height: 18px; flex: none; }
  .s13 .pf { inset-inline-start: 868px; width: 219px; background: var(--accent); gap: 14px; }
  .s13 .pf b { font-weight: 500; }
  .s13 .pf span { font-size: 13px; color: #DCE7FF; text-transform: none; }
  .s13 .gc { inset-inline-start: 741px; width: 456px; height: 95px; border-radius: 4px; }
  .s13 .gc .k { top: 16px; font-size: 16px; color: var(--muted); text-transform: uppercase; }
  .s13 .gc .k1 { inset-inline-start: 30px; } .s13 .gc .k2 { inset-inline-start: 224px; }
  .s13 .gc .nm { top: 40px; inset-inline-start: 30px; font-size: 18px; font-weight: 700; color: #3A4A6B; }
  .s13 .gc .dd { top: 38px; inset-inline-start: 222px; width: 215px; font-size: 15px; font-weight: 600; line-height: 1.3; color: var(--ink-2); }"""

ICON_PLANT = '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M11 21v-7.2C8.2 13.5 6 11.1 6 8.2V5.5c2.9 0 5.3 2.2 5.8 5.1V8.4C11.3 5.9 13 3.7 15.6 3H18v2.7c0 2.9-2.2 5.3-5 5.6V21h-2z"/><path d="M4 22h16v-1.5H4z"/></svg>'
ICON_GEAR = '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M10.6 2.5h2.8l.4 2.3a7 7 0 0 1 1.8 1l2.2-.8 1.4 2.4-1.8 1.5a7 7 0 0 1 0 2.2l1.8 1.5-1.4 2.4-2.2-.8a7 7 0 0 1-1.8 1l-.4 2.3h-2.8l-.4-2.3a7 7 0 0 1-1.8-1l-2.2.8-1.4-2.4 1.8-1.5a7 7 0 0 1 0-2.2L4.8 7.4l1.4-2.4 2.2.8a7 7 0 0 1 1.8-1zM12 8.5a3.5 3.5 0 1 0 0 7 3.5 3.5 0 0 0 0-7z"/></svg>'
ICON_EYE = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true"><path d="M2 12s3.5-6 10-6 10 6 10 6-3.5 6-10 6S2 12 2 12z"/><circle cx="12" cy="12" r="3"/></svg>'
ICON_LOGO = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linejoin="round" aria-hidden="true"><path d="M7 3.5c3-.7 7-.7 10 0 1.8.4 3 1.6 3.4 3.4.7 3 .7 7 0 10-.4 1.8-1.6 3-3.4 3.4-3 .7-7 .7-10 0-1.8-.4-3-1.6-3.4-3.4-.7-3-.7-7 0-10C4 5.1 5.2 3.9 7 3.5z"/></svg>'

GLOBAL_DEFS = ('<defs>'
               '<linearGradient id="beam" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#4E86FF" stop-opacity=".12"/><stop offset=".55" stop-color="#4E86FF" stop-opacity=".6"/><stop offset="1" stop-color="#5A8FFF" stop-opacity="1"/></linearGradient>'
               '<radialGradient id="glow" cx=".5" cy=".5" r=".5"><stop offset="0" stop-color="#93B6FF"/><stop offset=".7" stop-color="#7FA8FF"/><stop offset="1" stop-color="#4E86FF" stop-opacity="0"/></radialGradient>'
               '</defs>')


def ring(cx, cy, ro, ri, a0, a1):
    """مسار قطاع حلقي من الزاوية a0 إلى a1 (بالدرجات، 180 يسارًا و0 يمينًا فوق المركز)."""
    import math
    def pt(r, a):
        a = math.radians(a); return '%.1f,%.1f' % (cx + r * math.cos(a), cy - r * math.sin(a))
    return 'M%s A%d,%d 0 0 1 %s L%s A%d,%d 0 0 0 %s Z' % (pt(ro, a0), ro, ro, pt(ro, a1), pt(ri, a1), ri, ri, pt(ri, a0))


GAUGE = ('<svg viewBox="0 0 330 193" aria-hidden="true">'
         + '<path d="%s" fill="none" stroke="#fff" stroke-width="2"/>' % ring(165, 162, 138, 102, 180, 116)
         + '<path d="%s" fill="#A9C9FF"/>' % ring(165, 162, 138, 102, 116, 62)
         + '<path d="%s" fill="#163B8C"/>' % ring(165, 162, 138, 102, 62, 0)
         + '</svg>')

# الشريحة 1
S1 = ('\n    <div class="deco"></div>\n    <div class="logo">%s</div>\n    <p class="co">§S1_CO1§<br>§S1_CO2§</p>\n'
      '    <h1 class="t1">§S1_T1§</h1>\n    <h2 class="t2">§S1_T2§</h2>\n    <p class="p">§S1_P§</p>\n  ' % ICON_LOGO)

# الشريحة 2
S2 = ('\n    <h1 class="ttl">§S2_TTL§</h1>\n'
      '    <div class="prob card"><div class="ico">%s</div><h2>§S2_PROB§</h2><p class="big"><b>§S2_LOSS_V§</b> §S2_LOSS§</p><p class="sm">§S2_SAMPLE§</p></div>\n' % ICON_PLANT
      + ''.join('    <div class="kpi k%d wh"><b class="v">§S2_K%dV§</b><span class="l">§S2_K%dL§</span></div>\n' % (i, i, i) for i in range(1, 5))
      + '    <div class="inv wh"><div class="ico">%s</div><h2>§S2_INV§</h2><p class="p">§S2_INVP§</p>\n' % ICON_PLANT
      + ''.join('      <div class="mini m%d"><b>§S2_M%dV§</b><span>§S2_M%dL§</span></div>\n' % (i, i, i) for i in range(1, 4))
      + '      <div class="gauge">%s<p class="go">§S2_GO§</p><p class="dc">§S2_DC§</p></div>\n    </div>\n  ' % GAUGE)

# الشريحة 3
S3 = ('\n    <h1 class="ttl">§S3_TTL§</h1>\n'
      + ''.join('    <h2 class="hd h%d">§S3_H%d§</h2>\n' % (i, i) for i in range(1, 5))
      + '    <div class="rule"></div>\n'
      + ''.join('    <div class="ln" style="top:%dpx"></div>\n' % y for y in (270, 328, 372, 418, 466, 512, 558, 605, 650))
      + ''.join('    <div class="q q%d wh"><span class="n">§S3_Q%d§</span><b class="v">§S3_V%d§</b></div>\n' % (i, i, i) for i in range(1, 5))
      + ''.join('    <div class="plus p%d">+</div>\n' % i for i in range(1, 4))
      + '    <div class="side card"><h2>§S3_SIDE§</h2><p>§S3_SIDEP§</p><b class="big">§S3_BIG§</b></div>\n  ')

# الشريحة 4
S4 = ('\n    <h1 class="ttl">§S4_TTL§</h1>\n    <p class="lead">§S4_LEAD§</p>\n'
      '    <div class="lg lg1"><i></i><span>§S4_L1§</span></div>\n    <div class="lg lg2"><i></i><span>§S4_L2§</span></div>\n    <div class="lg lg3"><i></i><span>§S4_L3§</span></div>\n'
      '    <svg class="orbit" viewBox="0 0 1280 720" aria-hidden="true"><ellipse cx="640" cy="425" rx="440" ry="265" fill="none" stroke="#fff" stroke-width="1.2"/>'
      '<ellipse cx="638" cy="421" rx="290" ry="212" fill="url(#glow)"/></svg>\n'
      '    <div class="core"><span>§S4_CORE1§<br>§S4_CORE2§</span></div>\n'
      '    <div class="dom d1"><span>§S4_D1§</span></div>\n    <div class="dom d2"><span>§S4_D2§</span></div>\n    <div class="dom d3"><span>§S4_D3§</span></div>\n  ')

# الشريحة 5
S5 = ('\n    <div class="grid"></div>\n    <h1 class="ttl">§S5_TTL§</h1>\n'
      '    <div class="bub ba"><b>§S5_VA§</b><span>§S5_OA§</span></div>\n    <div class="bub bb"><b>§S5_VB§</b><span>§S5_OB§</span></div>\n'
      '    <div class="bub bc">%s<b>§S5_VC§</b><span>§S5_OC§</span></div>\n    <div class="bub bd"><b>§S5_VD§</b><span>§S5_OD§</span></div>\n' % ICON_GEAR
      + '    <div class="panel wh"><h2>§S5_WS§</h2><svg id="score-chart" viewBox="0 0 300 125" aria-hidden="true"></svg></div>\n'
      '    <p class="tot">§S5_TOT§</p>\n  ')

# الشريحة 6
BEAMS = ('<svg class="beams" viewBox="0 0 1280 720" aria-hidden="true">'
         '<polygon points="307,243 422,275 301,572 105,442" fill="url(#beam)"/>'
         '<polygon points="506,269 634,275 621,550 392,499" fill="url(#beam)"/>'
         '<polygon points="672,275 787,269 911,499 685,550" fill="url(#beam)"/>'
         '<polygon points="890,307 1005,237 1199,442 998,572" fill="url(#beam)"/></svg>')
S6 = ('\n    <h1 class="ttl">§S6_TTL§</h1>\n    ' + BEAMS + '\n'
      + ''.join('    <div class="c c%d">§S6_C%d§</div>\n' % (i, i) for i in range(1, 5))
      + ''.join('    <b class="big v%d">§S6_V%d§</b>\n    <span class="lab l%d">§S6_L%d§</span>\n' % (i, i, i, i) for i in range(1, 5))
      + '    <p class="tf">§S6_TF§</p>\n  ')

# الشريحة 7
ROWS7 = [(152, (472, 712, 953)), (267, (351, 592, 832, 1072)), (383, (472, 712, 953)), (499, (459, 700, 940))]


def links7():
    """منحنيات رفيعة تصل كل صندوق بالصناديق المجاورة في الصف التالي."""
    d = []
    for r in range(3):
        y1 = ROWS7[r][0] + 56; y2 = ROWS7[r + 1][0]
        for x in ROWS7[r][1]:
            cx1 = x + 91
            for x2 in ROWS7[r + 1][1]:
                cx2 = x2 + 91
                if abs(cx2 - cx1) <= 130:
                    d.append('M%d,%d C%d,%d %d,%d %d,%d' % (cx1, y1, cx1, (y1 + y2) // 2, cx2, (y1 + y2) // 2, cx2, y2))
    return '<svg class="links" viewBox="0 0 1280 720" aria-hidden="true"><path d="%s" fill="none" stroke="#7FA8FF" stroke-width="1.3"/></svg>' % ' '.join(d)


S7 = ('\n    <h1 class="ttl">§S7_TTL§</h1>\n'
      + ''.join('    <p class="li li%d">§S7_LI§</p>\n' % i for i in range(1, 5))
      + '    <div class="panel"><div class="band b1"></div><div class="band b2"></div><h2 class="h1">§S7_PRJ§</h2><p class="sub">§S7_PH§</p><b class="amt a1">§S7_A1§</b><h2 class="h2">§S7_NPV§</h2><b class="amt a2">§S7_A2§</b></div>\n'
      + '    ' + links7() + '\n'
      + ''.join('    <div class="bx%s" style="top:%dpx;inset-inline-start:%dpx">§S7_BX§</div>\n' % (' r4' if y == 499 else '', y, x) for y, xs in ROWS7 for x in xs) + '  ')

# الشريحة 8
S8 = ('\n    <div class="left"></div>\n    <div class="right"></div>\n'
      '    <svg class="mnt" viewBox="0 0 676 720" aria-hidden="true"><polygon points="0,268 95,237 390,520 575,570 676,612 676,720 0,720" fill="#4E86FF"/><path d="M291,420 L291,720" stroke="#3F73E6" stroke-width="2"/></svg>\n'
      '    <h1 class="ttl">§S8_T1§<br>§S8_T2§</h1>\n'
      '    <div class="card"><b>§S8_PCT§</b><span>§S8_SAMPLE§</span></div>\n'
      '    <span class="qq q1">§S8_Q1§</span>\n    <span class="qq q2">§S8_Q2§</span>\n'
      '    <h2 class="rh">§S8_RH§</h2>\n    <p class="rt">§S8_TOTAL§</p>\n'
      + ''.join('    <div class="bub u%d"><b>§S8_B%d§</b><span>§S8_ADD§</span></div>\n' % (i, i) for i in (1, 6, 7, 3, 2, 4, 5)) + '  ')

# الشريحة 9
BERG = ('<svg class="berg" viewBox="0 0 1280 720" aria-hidden="true">'
        '<polygon points="405,305 520,165 560,150 615,40 650,120 700,95 760,160 790,145 875,305" fill="#DDE4F5" fill-opacity=".38"/>'
        '<polygon points="560,150 615,40 650,120 640,305 520,305" fill="#fff" fill-opacity=".12"/>'
        '<polygon points="700,95 760,160 790,145 875,305 700,305" fill="#000" fill-opacity=".06"/>'
        '<polygon points="405,305 875,305 760,420 700,520 650,575 560,470 470,400" fill="#DDE4F5" fill-opacity=".22"/>'
        '<polygon points="520,305 640,305 650,575 560,470" fill="#fff" fill-opacity=".1"/>'
        '<polygon points="700,305 875,305 760,420 700,520" fill="#000" fill-opacity=".05"/></svg>')
S9 = ('\n    <div class="sea"></div>\n    <span class="cap c1">§S9_VIS§</span><span class="sum s1">§S9_VISV§</span>\n'
      + ''.join('    <div class="row wh" style="top:%dpx"><p>§S9_ROW§</p><span>§S9_ROWV§</span></div>\n' % y for y in (133, 187, 241, 320, 375, 430, 485, 540))
      + '    <span class="cap c2">§S9_HID§</span><span class="sum s2">§S9_HIDV§</span>\n    ' + BERG + '\n'
      '    <div class="pct pc1 wh">§S9_P1§</div>\n    <div class="pct pc2 wh">§S9_P2§</div>\n'
      '    <svg class="chart" id="tco-chart" viewBox="0 0 356 440" aria-hidden="true"></svg>\n'
      '    <span class="tco">§S9_TCO§</span>\n    <h1 class="ttl">§S9_TTL§</h1>\n  ')

# الشريحة 10
DARK10 = {1: 2, 2: 1, 3: 3, 4: 2, 5: 1}  # رقم نصف القرص الداكن (الذي يحمل القيمة) في كل عمود


def col10(i):
    halves = ''.join('<div class="half h%d%s">%s</div>' % (h, ' dk' if DARK10[i] == h else '', '§S10_V%d§' % i if DARK10[i] == h else '') for h in (1, 2, 3))
    return '    <div class="col co%d card"><h2>§S10_H%d§</h2><i></i><p>§S10_P§</p>%s</div>\n' % (i, i, halves)


S10 = '\n    <h1 class="ttl">§S10_TTL§</h1>\n' + ''.join(col10(i) for i in range(1, 6)) + '  '

# الشريحة 11
R11 = [(156, 154, 140), (142, 142, 124), (125, 125, 114), (120, 120, 100)]  # (عرض العلوي، ارتفاع العلوي، نصف قطر السفلي)
S11 = ('\n    <h1 class="ttl">§S11_TTL§</h1>\n    <div class="bar wh">' + ''.join('<span class="x%d">§S11_X%d§</span>' % (i, i) for i in range(1, 5)) + '</div>\n'
       '    <div class="axis"></div>\n'
       + ''.join('    <div class="g g%d"><div class="up" style="width:%dpx;height:%dpx"></div><div class="dn" style="width:%dpx;height:%dpx"></div><span class="v vu">§S11_U%d§</span><span class="v vd">§S11_D%d§</span></div>\n'
                 % (i + 1, w, h, r, r, i + 1, i + 1) for i, (w, h, r) in enumerate(R11))
       + '    <b class="sw sw1">§S11_SW1§</b>\n    <span class="swl sl1">§S11_SWL§</span>\n    <span class="swl sl2">§S11_SWL§</span>\n    <b class="sw sw2">§S11_SW2§</b>\n  ')

# الشريحة 12
S12 = ('\n    <h1 class="ttl">§S12_TTL§</h1>\n'
       + ''.join('    <div class="ph p%d"><small>§S12_M%d§</small><span>§S12_N%d§</span></div>\n' % (i, i, i) for i in range(1, 6))
       + ''.join('    <div class="ln" style="top:%dpx"></div>\n' % y for y in (247, 348, 448, 548))
       + ''.join('    <div class="gate" style="top:%dpx">§S12_G%d§</div>\n    <b class="amt" style="top:%dpx">§S12_A%d§</b>\n    <h2 class="rh" style="top:%dpx">§S12_RH§</h2>\n    <p class="rp" style="top:%dpx">§S12_RP§</p>\n'
                 % (y + 2, i + 1, y + 49, i + 1, y + 16, y + 43) for i, y in enumerate((247, 348, 448, 548))) + '  ')

# الشريحة 13
S13 = ('\n    <h1 class="vt">§S13_TTL§</h1>\n'
       + ''.join('    <div class="hd hd%d"><span>§S13_ADD1§<br>§S13_ADD2§</span></div>\n' % i for i in range(1, 5))
       + ''.join('    <div class="pill pm" style="top:%dpx">%s<span>§S13_MON§</span></div>\n    <div class="pill pf" style="top:%dpx"><b>§S13_FORUM§</b><span>§S13_SAMPLE§</span></div>\n'
                 '    <div class="gc wh" style="top:%dpx"><span class="k k1">§S13_OWNER§</span><b class="nm">§S13_O%d§</b><span class="k k2">§S13_DEC§</span><p class="dd">§S13_DD§</p></div>\n'
                 % (y, ICON_EYE, y, y + 42, i + 1) for i, y in enumerate((50, 207, 364, 521))) + '  ')

SLIDES = [S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13]

JSON_BLOCKS = {
    # الشريحة 5: الدرجة الموزونة لكل خيار (0–100) — الشريط الأطول للخيار ج مع أيقونة الترس
    'chart-data': {
        'ar': {'labels': ['الخيار أ', 'الخيار ب', 'الخيار ج', 'الخيار د'], 'values': [44, 66, 100, 87], 'marked': 2},
        'en': {'labels': ['Option A', 'Option B', 'Option C', 'Option D'], 'values': [44, 66, 100, 87], 'marked': 2},
    },
    # الشريحة 9: نسب التكلفة الظاهرة (فوق خط الماء) والمخفية (تحته) لكل بند
    'chart-data-2': {'visible': [56, 16, 0, 0, 0], 'hidden': [21, 16, 11, 8, 5]},
}

EXTRA_SCRIPT = """  // الشريحة 5: أشرطة الدرجة الموزونة (الفئة الأولى في الأعلى؛ الأشرطة تنطلق من جهة البداية)
  function drawScore() {
    var el = document.getElementById('score-chart'); if (!el) return;
    var d = JSON.parse(document.getElementById('chart-data').textContent), max = Math.max.apply(null, d.values), out = '';
    d.labels.forEach(function (lab, i) {
      var y = 15 + i * 30, w = Math.round(d.values[i] / max * 183), lx = RTL ? 300 - 0 : 67, bx = RTL ? 300 - 67 - w : 67;
      out += '<text class="lab" x="' + (RTL ? 236 : 62) + '" y="' + (y + 17) + '" text-anchor="' + (RTL ? 'start' : 'end') + '">' + lab + '</text>';
      out += '<rect x="' + bx + '" y="' + y + '" width="' + w + '" height="24" fill="#4E86FF"/>';
      if (i === d.marked) out += '<g transform="translate(' + (RTL ? bx + 6 : bx + w - 26) + ' ' + (y + 2) + ') scale(.85)" fill="#fff"><path d="M10.6 2.5h2.8l.4 2.3a7 7 0 0 1 1.8 1l2.2-.8 1.4 2.4-1.8 1.5a7 7 0 0 1 0 2.2l1.8 1.5-1.4 2.4-2.2-.8a7 7 0 0 1-1.8 1l-.4 2.3h-2.8l-.4-2.3a7 7 0 0 1-1.8-1l-2.2.8-1.4-2.4 1.8-1.5a7 7 0 0 1 0-2.2L4.8 7.4l1.4-2.4 2.2.8a7 7 0 0 1 1.8-1zM12 8.5a3.5 3.5 0 1 0 0 7 3.5 3.5 0 0 0 0-7z"/></g>';
    });
    el.innerHTML = out;
  }
  // الشريحة 9: أعمدة التكلفة الظاهرة فوق خط الماء والمخفية تحته (الأول في جهة البداية)
  function drawTco() {
    var el = document.getElementById('tco-chart'); if (!el) return;
    var d = JSON.parse(document.getElementById('chart-data-2').textContent), water = 227, k = 3.54, b0 = 30, out = '';
    for (var gx = 21; gx <= 356; gx += 45) out += '<line x1="' + gx + '" y1="0" x2="' + gx + '" y2="440" stroke="#fff" stroke-opacity=".18"/>';
    for (var gy = 0; gy <= 440; gy += 45) out += '<line x1="0" y1="' + gy + '" x2="356" y2="' + gy + '" stroke="#fff" stroke-opacity=".18"/>';
    d.visible.forEach(function (v, i) {
      var x = RTL ? 356 - 21 - 45 - i * 63.5 : 21 + i * 63.5, h = d.hidden[i];
      // الارتفاع = قاعدة ثابتة لتتسع التسمية + تناسب مع القيمة
      var hv = b0 + v * k, hh = b0 + h * k;
      if (v) { out += '<rect x="' + x + '" y="' + (water - hv) + '" width="45" height="' + hv + '" fill="#4E86FF"/>';
        out += '<text x="0" y="0" transform="translate(' + (x + 27) + ' ' + (water - hv / 2) + ') rotate(90)" text-anchor="middle" fill="#fff" font-size="15">' + v + '%</text>'; }
      out += '<rect x="' + x + '" y="' + water + '" width="45" height="' + hh + '" fill="#355FCF"/>';
      out += '<text x="0" y="0" transform="translate(' + (x + 27) + ' ' + (water + hh / 2) + ') rotate(90)" text-anchor="middle" fill="#fff" font-size="15">' + h + '%</text>';
    });
    el.innerHTML = out;
  }
  drawScore(); drawTco();"""

SAMPLE_AR = 'هذا نص توضيحي. أضف نصًا هنا.'
SAMPLE_EN = 'This is a sample text. Add text here.'

STR = {
    'ar': {
        'S1_CO1': 'اسم شركتك', 'S1_CO2': 'شعارك هنا', 'S1_T1': 'دراسة الجدوى', 'S1_T2': 'وتحليل العائد على الاستثمار',
        'S1_P': 'أضف نصًا هنا. أدخل النص المطلوب هنا. هذا نص بسيط. أضف نصًا هنا. أدخل النص المطلوب هنا. هذا نص بسيط. أضف نصًا هنا. أدخل النص المطلوب هنا. هذا نص بسيط. أدخل النص المطلوب هنا.',
        'S2_TTL': 'الملخص التنفيذي', 'S2_PROB': 'المشكلة', 'S2_LOSS_V': '$2.99M', 'S2_LOSS': 'خسائر في صيانة المعدات',
        'S2_SAMPLE': 'هذا نص توضيحي. أدخل النص المطلوب هنا. هذا نص توضيحي.',
        'S2_K1V': '$4.2M', 'S2_K1L': 'صافي القيمة الحالية', 'S2_K2V': '15.4%', 'S2_K2L': 'معدل العائد الداخلي',
        'S2_K3V': '3.2 سنة', 'S2_K3L': 'فترة الاسترداد المخصومة', 'S2_K4V': '138%', 'S2_K4L': 'العائد على الاستثمار لخمس سنوات',
        'S2_INV': 'حالة الاستثمار', 'S2_INVP': 'هذا نص توضيحي. أدخل النص المطلوب هنا. هذا نص توضيحي.',
        'S2_M1V': '$1.4M', 'S2_M1L': 'إجمالي الطلب', 'S2_M2V': '4', 'S2_M2L': 'مراحل', 'S2_M3V': '5 أشهر', 'S2_M3L': 'حتى القيمة',
        'S2_GO': 'انطلق', 'S2_DC': 'قرار الاعتماد',
        'S3_TTL': 'تكلفة التقاعس', 'S3_H1': 'إعادة العمل والشطب', 'S3_H2': 'غرامات اتفاقيات الخدمة والتعجيل', 'S3_H3': 'العمل الإضافي والتوظيف المؤقت', 'S3_H4': 'تكاليف مشكلات التسليم',
        'S3_Q1': 'الربع 1', 'S3_Q2': 'الربع 2', 'S3_Q3': 'الربع 3', 'S3_Q4': 'الربع 4', 'S3_V1': '-$400K', 'S3_V2': '-$405K', 'S3_V3': '-$500K', 'S3_V4': '-$550K',
        'S3_SIDE': 'تكلفة التقاعس في السنة الأولى', 'S3_SIDEP': '$X.XM على مدى خمس سنوات مع نمو الحجم وتراكم تكاليف الأخطاء.', 'S3_BIG': '$1.85M',
        'S4_TTL': 'نطاق المشروع', 'S4_LEAD': 'مبادرة أساسية تدور حولها مجالات القدرات، والاستثناءات محددة صراحة.',
        'S4_L1': 'الأساس', 'S4_L2': 'المجالات', 'S4_L3': 'مستبعد', 'S4_CORE1': 'المشروع', 'S4_CORE2': 'س',
        'S4_D1': 'استبدال نظام علاقات العملاء', 'S4_D2': 'أتمتة المستودعات', 'S4_D3': 'إعادة بناء نظام الموارد المالية',
        'S5_TTL': 'ساحة القرار', 'S5_VA': '$0.1M', 'S5_OA': 'الخيار أ', 'S5_VB': '$0.7M', 'S5_OB': 'الخيار ب', 'S5_VC': '$2.4M', 'S5_OC': 'الخيار ج', 'S5_VD': '$3.4M', 'S5_OD': 'الخيار د',
        'S5_WS': 'الدرجة الموزونة', 'S5_TOT': 'إجمالي التكلفة لخمس سنوات ⟵',
        'S6_TTL': 'تحوّل العمليات', 'S6_C1': '11', 'S6_C2': '55%', 'S6_C3': '8.2%', 'S6_C4': '6 أيام',
        'S6_V1': '1', 'S6_V2': '12%', 'S6_V3': '0.8%', 'S6_V4': '1.1 يوم', 'S6_L1': 'المؤشر 1', 'S6_L2': 'المؤشر 2', 'S6_L3': 'المؤشر 3', 'S6_L4': 'المؤشر المحسّن',
        'S6_TF': 'اليوم ⟵ المستقبل',
        'S7_TTL': 'تحقيق العائد', 'S7_LI': SAMPLE_AR, 'S7_PRJ': 'المشروع س', 'S7_PH': 'على مراحل خلال 24 شهرًا', 'S7_A1': '$2.4M', 'S7_NPV': 'صافي القيمة الحالية', 'S7_A2': '$3.8M', 'S7_BX': SAMPLE_AR,
        'S8_T1': 'توزيع', 'S8_T2': 'الاستثمار', 'S8_PCT': '60%', 'S8_SAMPLE': SAMPLE_AR, 'S8_Q1': 'الربع 1 – الربع 4', 'S8_Q2': 'الربع 5 – الربع 8',
        'S8_RH': 'التوزيع حسب الفئة', 'S8_TOTAL': 'الإجمالي: $26M', 'S8_ADD': 'أضف نصًا هنا.',
        'S8_B1': '3.68M', 'S8_B2': '1.1M', 'S8_B3': '2.93M', 'S8_B4': '1.1M', 'S8_B5': '2.1M', 'S8_B6': '6.43M', 'S8_B7': '8.61M',
        'S9_VIS': 'الظاهر', 'S9_VISV': '1.67M', 'S9_HID': 'المخفي', 'S9_HIDV': '1.29M', 'S9_ROW': SAMPLE_AR, 'S9_ROWV': '$123K',
        'S9_P1': '56%', 'S9_P2': '44%', 'S9_TCO': 'التكلفة لخمس سنوات', 'S9_TTL': 'إجمالي تكلفة الملكية',
        'S10_TTL': 'تخطيط السيناريوهات', 'S10_H1': 'معدل التبني', 'S10_H2': 'القيمة لكل طلب', 'S10_H3': 'تضخم التكاليف', 'S10_H4': 'وقت الإطلاق', 'S10_H5': 'معدل الخصم',
        'S10_P': SAMPLE_AR, 'S10_V1': '68%', 'S10_V2': '$14.2', 'S10_V3': '5.1%', 'S10_V4': '10 أشهر', 'S10_V5': '9%',
        'S11_TTL': 'تحليل حساسية صافي القيمة الحالية', 'S11_X1': 'معدل التبني', 'S11_X2': 'تحقق التسعير', 'S11_X3': 'انزلاق الجدول الزمني', 'S11_X4': 'تكلفة التنفيذ',
        'S11_U1': '11M', 'S11_D1': '9M', 'S11_U2': '8M', 'S11_D2': '7M', 'S11_U3': '7M', 'S11_D3': '4M', 'S11_U4': '1.1M', 'S11_D4': '1M',
        'S11_SW1': '-15%', 'S11_SWL': 'التأرجح', 'S11_SW2': '+15%',
        'S12_TTL': 'خارطة طريق بوابات التمويل', 'S12_M1': 'ش1–ش4', 'S12_N1': 'الاستكشاف', 'S12_M2': 'ش4–ش10', 'S12_N2': 'البناء', 'S12_M3': 'ش10–ش15', 'S12_N3': 'التجربة', 'S12_M4': 'ش15–ش20', 'S12_N4': 'التوسع', 'S12_M5': 'ش23', 'S12_N5': 'التشغيل',
        'S12_G1': 'البوابة 1', 'S12_G2': 'البوابة 2', 'S12_G3': 'البوابة 3', 'S12_G4': 'البوابة 4', 'S12_A1': '$0.3M', 'S12_A2': '$1.3M', 'S12_A3': '$0.6M', 'S12_A4': '$0.2M سنويًا',
        'S12_RH': 'أضف نصًا هنا', 'S12_RP': 'هذا نص توضيحي.',
        'S13_TTL': 'الحوكمة', 'S13_ADD1': 'أضف', 'S13_ADD2': 'نصًا هنا', 'S13_MON': 'شهريًا', 'S13_FORUM': 'المنتدى', 'S13_SAMPLE': 'هذا نص توضيحي.',
        'S13_OWNER': 'المالك', 'S13_DEC': 'يقرر', 'S13_O1': 'رئيس العمليات', 'S13_O2': 'المدير المالي', 'S13_O3': 'رئيس المنتجات', 'S13_O4': 'رئيس العمليات',
        'S13_DD': 'أضف نصًا هنا. هذا نص توضيحي. أضف نصًا هنا.',
    },
    'en': {
        'S1_CO1': 'Your Company', 'S1_CO2': 'Logo Here', 'S1_T1': 'Business Case', 'S1_T2': '&amp; ROI Analysis',
        'S1_P': 'Add text here. Insert your desired text here. This is a simple text. Add text here. Insert your desired text here. This is a simple text. Add text here. Insert your desired text here. This is a simple text. Insert your desired text here.',
        'S2_TTL': 'Executive Summary', 'S2_PROB': 'The Problem', 'S2_LOSS_V': '$2.99M', 'S2_LOSS': 'Loses on Equipment Meintenance',
        'S2_SAMPLE': 'This is a sample text. Insert your desired text here. This is a sample text.',
        'S2_K1V': '$4.2M', 'S2_K1L': 'Net Present Value', 'S2_K2V': '15.4%', 'S2_K2L': 'Internal Rate of Return',
        'S2_K3V': '3.2 yrs', 'S2_K3L': 'Discounted Payback', 'S2_K4V': '138%', 'S2_K4L': '5-Year ROI',
        'S2_INV': 'Investment Case', 'S2_INVP': 'This is a sample text. Insert your desired text here. This is a sample text.',
        'S2_M1V': '$1.4M', 'S2_M1L': 'Total Ask', 'S2_M2V': '4', 'S2_M2L': 'Stages', 'S2_M3V': '5 mo', 'S2_M3L': 'To Value',
        'S2_GO': 'GO', 'S2_DC': 'Decision Call',
        'S3_TTL': 'Cost of Inaction', 'S3_H1': 'Error Rework<br>&amp; Write-offs', 'S3_H2': 'SLA Penalties<br>&amp; Expediting', 'S3_H3': 'Overtime &amp;<br>Temp Staffing', 'S3_H4': 'Delivery<br>Issues Costs',
        'S3_Q1': 'Q1', 'S3_Q2': 'Q2', 'S3_Q3': 'Q3', 'S3_Q4': 'Q4', 'S3_V1': '-$400K', 'S3_V2': '-$405K', 'S3_V3': '-$500K', 'S3_V4': '-$550K',
        'S3_SIDE': 'Year-One Cost of Inaction', 'S3_SIDEP': '$X.XM over five years as volume grows and error costs compound.', 'S3_BIG': '$1.85M',
        'S4_TTL': 'Scope Universe', 'S4_LEAD': 'Core initiative with orbiting capability domains, exclusions are explicit.',
        'S4_L1': 'Core', 'S4_L2': 'Domains', 'S4_L3': 'Excluded', 'S4_CORE1': 'Project', 'S4_CORE2': 'XYZ',
        'S4_D1': 'CRM Replacement', 'S4_D2': 'Warehouse Automation', 'S4_D3': 'Finance ERP Re-platform',
        'S5_TTL': 'Decision Arena', 'S5_VA': '$0.1M', 'S5_OA': 'Option A', 'S5_VB': '$0.7M', 'S5_OB': 'Option B', 'S5_VC': '$2.4M', 'S5_OC': 'Option C', 'S5_VD': '$3.4M', 'S5_OD': 'Option D',
        'S5_WS': 'Weighted<br>Score', 'S5_TOT': 'Total 5-yr Cost ⟶',
        'S6_TTL': 'Process Transformation', 'S6_C1': '11', 'S6_C2': '55%', 'S6_C3': '8.2%', 'S6_C4': '6 d',
        'S6_V1': '1', 'S6_V2': '12%', 'S6_V3': '0.8%', 'S6_V4': '1.1 d', 'S6_L1': 'KPI 1', 'S6_L2': 'KPI 2', 'S6_L3': 'KPI 3', 'S6_L4': 'Improved KPI',
        'S6_TF': 'Today ⟶ Future',
        'S7_TTL': 'ROI Delivery', 'S7_LI': SAMPLE_EN, 'S7_PRJ': 'Project XYZ', 'S7_PH': 'Phased over 24 months', 'S7_A1': '$2.4M', 'S7_NPV': 'Net Present Value', 'S7_A2': '$3.8M', 'S7_BX': SAMPLE_EN,
        'S8_T1': 'Investment', 'S8_T2': 'Breakdown', 'S8_PCT': '60%', 'S8_SAMPLE': SAMPLE_EN, 'S8_Q1': 'Q1 – Q4', 'S8_Q2': 'Q5 – Q8',
        'S8_RH': 'Allocation by Category', 'S8_TOTAL': 'Total : $26M', 'S8_ADD': 'Add text here.',
        'S8_B1': '3.68M', 'S8_B2': '1.1M', 'S8_B3': '2.93M', 'S8_B4': '1.1M', 'S8_B5': '2.1M', 'S8_B6': '6.43M', 'S8_B7': '8.61M',
        'S9_VIS': 'Visible', 'S9_VISV': '1.67M', 'S9_HID': 'Hidden', 'S9_HIDV': '1.29M', 'S9_ROW': SAMPLE_EN, 'S9_ROWV': '$123K',
        'S9_P1': '56%', 'S9_P2': '44%', 'S9_TCO': '5-Year TCO', 'S9_TTL': 'Total Cost Of Ownership',
        'S10_TTL': 'Scenario Planning', 'S10_H1': 'Adoption<br>Rate', 'S10_H2': 'Value<br>per Order', 'S10_H3': 'Cost<br>Inflation', 'S10_H4': 'Time to<br>Go-Live', 'S10_H5': 'Discount<br>Rate',
        'S10_P': SAMPLE_EN, 'S10_V1': '68%', 'S10_V2': '$14.2', 'S10_V3': '5.1%', 'S10_V4': '10mo', 'S10_V5': '9%',
        'S11_TTL': 'NPV Sensitivity Analysis', 'S11_X1': 'Adoption Rate', 'S11_X2': 'Pricing Realization', 'S11_X3': 'Timeline Slip', 'S11_X4': 'Implement. Cost',
        'S11_U1': '11M', 'S11_D1': '9M', 'S11_U2': '8M', 'S11_D2': '7M', 'S11_U3': '7M', 'S11_D3': '4M', 'S11_U4': '1.1M', 'S11_D4': '1M',
        'S11_SW1': '-15%', 'S11_SWL': 'Swing', 'S11_SW2': '+15%',
        'S12_TTL': 'Funding Gate Roadmap', 'S12_M1': 'M1-M4', 'S12_N1': 'Discovery', 'S12_M2': 'M4-M10', 'S12_N2': 'Build', 'S12_M3': 'M10-M15', 'S12_N3': 'Pilot', 'S12_M4': 'M15-M20', 'S12_N4': 'Scale', 'S12_M5': 'M23', 'S12_N5': 'Operate',
        'S12_G1': 'Gate 1', 'S12_G2': 'Gate 2', 'S12_G3': 'Gate 3', 'S12_G4': 'Gate 4', 'S12_A1': '$ 0.3M', 'S12_A2': '$1.3M', 'S12_A3': '$ 0.6M', 'S12_A4': '$ 0.2M/yr',
        'S12_RH': 'Add Text Here', 'S12_RP': 'This is a sample text.',
        'S13_TTL': 'Governance', 'S13_ADD1': 'Add', 'S13_ADD2': 'Text Here', 'S13_MON': 'Monthly', 'S13_FORUM': 'Forum', 'S13_SAMPLE': 'This is a sample text.',
        'S13_OWNER': 'Owner', 'S13_DEC': 'Decides', 'S13_O1': 'COO', 'S13_O2': 'CFO', 'S13_O3': 'CPO', 'S13_O4': 'COO',
        'S13_DD': 'Add text here. This is a sample text. Add text here.',
    },
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'finance', 'icon': '💹',
    'name': 'دراسة الجدوى والعائد على الاستثمار', 'nameEn': 'Business Case & ROI Analysis',
    'description': 'ملخص تنفيذي، تكلفة التقاعس، النطاق، الخيارات، العائد، التوزيع، التكلفة، السيناريوهات، الحساسية، التمويل، الحوكمة',
    'descriptionEn': 'Summary, cost of inaction, scope, options, ROI, breakdown, TCO, scenarios, sensitivity, funding gates, governance',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'الملخص التنفيذي', 'titleEn': 'Executive Summary', 'needsData': True},
        {'title': 'تكلفة التقاعس', 'titleEn': 'Cost of Inaction', 'needsData': True},
        {'title': 'نطاق المشروع', 'titleEn': 'Scope Universe'},
        {'title': 'ساحة القرار', 'titleEn': 'Decision Arena', 'needsData': True},
        {'title': 'تحوّل العمليات', 'titleEn': 'Process Transformation', 'needsData': True},
        {'title': 'تحقيق العائد', 'titleEn': 'ROI Delivery', 'needsData': True},
        {'title': 'توزيع الاستثمار', 'titleEn': 'Investment Breakdown', 'needsData': True},
        {'title': 'إجمالي تكلفة الملكية', 'titleEn': 'Total Cost of Ownership', 'needsData': True},
        {'title': 'تخطيط السيناريوهات', 'titleEn': 'Scenario Planning', 'needsData': True},
        {'title': 'تحليل الحساسية', 'titleEn': 'NPV Sensitivity Analysis', 'needsData': True},
        {'title': 'خارطة طريق بوابات التمويل', 'titleEn': 'Funding Gate Roadmap', 'needsData': True},
        {'title': 'الحوكمة', 'titleEn': 'Governance'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص والأرقام فقط؛ أشرطة الدرجة الموزونة وأعمدة إجمالي تكلفة الملكية تُرسم من كتلتي chart-data وchart-data-2، والباقي أشكال CSS/SVG ثابتة.',
}
