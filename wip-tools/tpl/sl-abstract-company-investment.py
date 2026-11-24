# -*- coding: utf-8 -*-
# نموذج «استثمار الشركة (تجريدي)»: اثنتا عشرة شريحة بنفسجية بيضاء لعرض استثماري — غلاف بصورة مبنى، ملخص تنفيذي، مشكلة، حل،
# تحليل سوق برسم أعمدة، نموذج عمل، متطلبات تمويل بجدول، فريق بصور، استراتيجية تخارج، مخاطر بجدول، أسئلة وأجوبة، وخاتمة.
KEY = 'sl-abstract-company-investment'
TITLE = {'ar': 'استثمار الشركة — عرض تقديمي', 'en': 'Abstract Company Investment'}
COMMENT = 'نموذج «استثمار الشركة (تجريدي)»: 12 شريحة بلون بنفسجي (#5036F9) على خلفيات فاتحة مع كتل زرقاء بنفسجية مدوّرة، صور فوتوغرافية مقصوصة من الأصل، رسم أعمدة SVG من chart-data، وجدولان. الإحداثيات مطلقة داخل إطار 1280×720 وتنعكس بالخصائص المنطقية.'
VARS = '--ink: #2D3538; --ink-2: #555B60; --accent: #5036F9; --lav: #BAAFFD; --bg: #F4F4F4; --white: #FFFFFF; --line: #E3E3E3; --row: #E5E5E5;'

A = '/templates/freeform/sl-abstract-company-investment/assets/'

STYLE = """  /* ---- أنماط مشتركة ---- */
  .ttl { position: absolute; top: 52px; inset-inline-start: 87px; margin: 0; font-size: 56px; font-weight: 700; line-height: 1.02; color: var(--ink); letter-spacing: -0.5px; }
  html[lang="ar"] .ttl { font-weight: 800; letter-spacing: 0; line-height: 1.15; top: 44px; }
  .sub { position: absolute; top: 124px; inset-inline-start: 87px; margin: 0; font-size: 24px; font-weight: 400; color: var(--accent); line-height: 1.2; }
  .sub.two { top: 186px; }
  .p { position: absolute; margin: 0; font-size: 17px; line-height: 1.5; color: var(--ink-2); }
  .h3 { position: absolute; margin: 0; font-size: 24px; font-weight: 700; line-height: 1.35; color: var(--ink); }
  .dots { position: absolute; color: var(--accent); }
  .dots svg { width: 100%; height: 100%; display: block; }
  .big { font-weight: 700; color: var(--accent); line-height: 1; margin: 0; position: absolute; }
  .grad-num { font-weight: 800; line-height: 1; background: linear-gradient(90deg, #C9C0FD 0%, #8F7DFB 45%, #5036F9 100%); -webkit-background-clip: text; background-clip: text; color: transparent; }
  html[dir="rtl"] .grad-num { background: linear-gradient(270deg, #C9C0FD 0%, #8F7DFB 45%, #5036F9 100%); -webkit-background-clip: text; background-clip: text; }
  .blob { position: absolute; border-radius: 50%; pointer-events: none; }
  .frame { background: var(--white); }

  /* ---- 1 الغلاف ---- */
  .s1 { background: var(--bg); }
  .s1 .hero { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 445px; direction: ltr; }
  html[dir="rtl"] .s1 .hero { transform: scaleX(-1); }   /* الصورة والشعار يُعكسان لتبقى الصورة متصلة مع اللوحة في الجهة الأخرى */
  .s1 .hero img { position: absolute; display: block; object-fit: cover; }
  .s1 .logo { position: absolute; top: 55px; inset-inline-start: 85px; width: 80px; height: 80px; color: #fff; }
  .s1 .panel { position: absolute; top: 292px; inset-inline-start: 0; width: 640px; height: 180px; background: var(--bg); border-start-end-radius: 48px; }
  .s1 .kick { position: absolute; top: 362px; inset-inline-start: 87px; margin: 0; font-size: 28px; font-weight: 400; letter-spacing: 5px; color: var(--ink); line-height: 1.2; }
  html[lang="ar"] .s1 .kick { letter-spacing: 0; font-size: 30px; }
  .s1 .ct { position: absolute; top: 415px; inset-inline-start: 84px; margin: 0; font-size: 78px; font-weight: 800; line-height: 1.0; color: var(--ink); letter-spacing: -1px; width: 620px; }
  html[lang="ar"] .s1 .ct { font-size: 72px; letter-spacing: 0; line-height: 1.1; top: 408px; }
  .s1 .bar { position: absolute; top: 614px; inset-inline-start: 0; width: 654px; height: 54px; background: var(--accent); border-start-end-radius: 10px; border-end-end-radius: 10px; color: #fff; font-size: 27px; letter-spacing: 3px; line-height: 54px; padding-inline-start: 87px; font-weight: 400; white-space: nowrap; }
  html[lang="ar"] .s1 .bar { letter-spacing: 0; font-size: 28px; }
  .s1 .s1p { top: 490px; inset-inline-start: 780px; width: 440px; }
  .s1 .pres { position: absolute; top: 620px; inset-inline-start: 780px; display: flex; align-items: center; gap: 18px; font-size: 20px; font-weight: 700; color: var(--ink); }
  .s1 .pres .dots { position: static; width: 44px; height: 44px; }

  /* ---- 2 الملخص التنفيذي ---- */
  .s2 .g2 { position: absolute; top: 0; inset-inline-start: 0; width: 640px; height: 420px; background: var(--bg); }
  .s2 .w2 { position: absolute; top: 320px; inset-inline-start: 0; width: 1280px; height: 400px; background: var(--white); border-start-start-radius: 64px; }
  .s2 .mint { top: -240px; inset-inline-end: -160px; width: 460px; height: 400px; background: radial-gradient(ellipse closest-side, #CDEADB 0%, #D8EEE2 60%, #E6F3EB 90%, rgba(255,255,255,0) 100%); }
  .s2 .ttl { top: 52px; }
  .s2 .dots { top: 372px; inset-inline-start: 84px; width: 62px; height: 62px; }
  .s2 .h3 { top: 470px; inset-inline-start: 87px; }
  .s2 .p { top: 522px; inset-inline-start: 87px; width: 430px; }
  .s2 .card { position: absolute; width: 316px; height: 288px; background: var(--accent); border-radius: 28px; color: #fff; }
  .s2 .c1 { top: 58px; inset-inline-start: 585px; } .s2 .c2 { top: 373px; inset-inline-start: 931px; }
  .s2 .card .num { position: absolute; top: 44px; inset-inline-start: 40px; margin: 0; font-size: 58px; font-weight: 800; line-height: 1; letter-spacing: -1px; }
  .s2 .card h2 { position: absolute; top: 158px; inset-inline-start: 40px; margin: 0; font-size: 24px; font-weight: 700; line-height: 1.2; }
  .s2 .card p { position: absolute; top: 196px; inset-inline-start: 40px; width: 240px; margin: 0; font-size: 17px; line-height: 1.5; }
  .s2 .ph { position: absolute; border-radius: 28px; object-fit: cover; display: block; }
  .s2 .ph1 { top: 40px; inset-inline-start: 931px; width: 316px; height: 306px; }
  .s2 .ph2 { top: 373px; inset-inline-start: 588px; width: 317px; height: 289px; }

  /* ---- 3 بيان المشكلة ---- */
  .s3 { background: var(--bg); }
  .pink { top: -250px; inset-inline-end: -200px; width: 660px; height: 660px; background: radial-gradient(circle closest-side, #F1E6E5 0%, #F2EAE9 60%, #F4F0F0 90%, rgba(244,244,244,0) 100%); }
  .s3 .pc { position: absolute; top: 219px; width: 352px; height: 300px; background: var(--white); border-start-start-radius: 24px; border-start-end-radius: 24px; }
  .s3 .pc1 { inset-inline-start: 87px; } .s3 .pc2 { inset-inline-start: 464px; } .s3 .pc3 { inset-inline-start: 842px; }
  .s3 .tab { position: absolute; top: 0; inset-inline-start: 38px; height: 51px; padding: 0 16px; min-width: 160px; background: var(--accent); color: #fff; font-size: 22px; font-weight: 700; line-height: 51px; border-end-start-radius: 8px; border-end-end-radius: 8px; white-space: nowrap; }
  .s3 .pc h2 { position: absolute; top: 96px; inset-inline-start: 38px; width: 280px; margin: 0; font-size: 24px; font-weight: 700; line-height: 1.55; color: var(--ink); }
  .s3 .pc .ln { position: absolute; top: 193px; inset-inline-start: 24px; width: 304px; height: 1px; background: var(--line); }
  .s3 .pc p { position: absolute; top: 212px; inset-inline-start: 38px; width: 290px; margin: 0; font-size: 17px; line-height: 1.5; color: var(--ink-2); }
  .s3 .band { position: absolute; top: 511px; inset-inline-start: 48px; width: 1184px; height: 184px; background: var(--accent); border-radius: 24px; overflow: hidden; color: #fff; }
  .s3 .band .circ { position: absolute; top: 29px; inset-inline-start: -28px; width: 340px; height: 340px; border-radius: 50%; background: radial-gradient(circle at 30% 30%, rgba(255,255,255,.34), rgba(255,255,255,.18) 70%); }
  .s3 .band p { position: absolute; top: 58px; inset-inline-start: 390px; width: 600px; margin: 0; font-size: 32px; line-height: 1.3; font-weight: 400; }
  html[lang="ar"] .s3 .band p { font-size: 30px; }

  /* ---- 4 الحل ---- */
  .s4 .blob4 { top: 355px; inset-inline-start: -225px; width: 1050px; height: 1050px; background: radial-gradient(circle closest-side at 45% 55%, #A98FCF 0%, #BFAEDB 50%, #DCD2EA 80%, #EFEAF6 97%, rgba(255,255,255,0) 100%); }
  .s4 .p0 { top: 207px; inset-inline-start: 87px; width: 400px; }
  .s4 .sc { position: absolute; inset-inline-start: 568px; width: 664px; height: 150px; background: var(--white); border-radius: 16px; box-shadow: 0 2px 14px rgba(40,30,90,.07); overflow: hidden; }
  .s4 .sc1 { top: 43px; } .s4 .sc2 { top: 205px; } .s4 .sc3 { top: 366px; } .s4 .sc4 { top: 527px; }
  .s4 .sc .n { position: absolute; top: 26px; inset-inline-start: -46px; margin: 0; font-size: 106px; color: transparent; }
  html[dir="rtl"] .s4 .sc .n { inset-inline-start: 14px; }   /* بالعربية يُعرض الرقم كاملًا لأن قصّ «0» من جهة النص يقصّ «1» */
  .s4 .sc h2 { position: absolute; top: 30px; inset-inline-start: 147px; margin: 0; font-size: 22px; font-weight: 700; color: var(--ink); line-height: 1.2; }
  .s4 .sc p { position: absolute; top: 68px; inset-inline-start: 147px; width: 470px; margin: 0; font-size: 17px; line-height: 1.5; color: var(--ink-2); }

  /* ---- 5 تحليل السوق ---- */
  .s5 .blob5 { top: 420px; inset-inline-start: 520px; width: 960px; height: 960px; background: radial-gradient(circle closest-side, #A58BCC 0%, #AF96D7 60%, #CFC2E5 85%, #EBE5F3 100%); }
  .s5 .p0 { top: 78px; inset-inline-start: 763px; width: 430px; }
  .s5 .kc { position: absolute; top: 230px; inset-inline-start: 52px; width: 510px; height: 446px; background: var(--white); border: 2px solid var(--accent); border-radius: 20px; }
  .s5 .kc .ic { position: absolute; top: 52px; width: 50px; height: 50px; color: var(--ink); }
  .s5 .kc .ic svg { width: 100%; height: 100%; display: block; }
  .s5 .kc .ic1 { inset-inline-start: 50px; } .s5 .kc .ic2 { inset-inline-start: 298px; }
  .s5 .kc .vl { position: absolute; top: 45px; inset-inline-start: 254px; width: 2px; height: 205px; background: var(--accent); }
  .s5 .kc p.big { top: 148px; font-size: 48px; color: var(--accent); width: auto; }
  .s5 .kc .b1 { inset-inline-start: 50px; } .s5 .kc .b2 { inset-inline-start: 298px; }
  .s5 .kc .lb { position: absolute; top: 212px; margin: 0; font-size: 24px; font-weight: 700; color: var(--ink); line-height: 1.2; }
  .s5 .kc .l1 { inset-inline-start: 50px; } .s5 .kc .l2 { inset-inline-start: 298px; }
  .s5 .kc .hl { position: absolute; top: 292px; inset-inline-start: 45px; width: 423px; height: 1px; background: var(--line); }
  .s5 .kc p { position: absolute; top: 322px; inset-inline-start: 50px; width: 410px; margin: 0; font-size: 17px; line-height: 1.5; color: var(--ink-2); }
  .s5 .cc { position: absolute; top: 230px; inset-inline-start: 588px; width: 638px; height: 446px; background: var(--accent); border-radius: 20px; color: #fff; }
  .s5 .cc h2 { position: absolute; top: 30px; inset-inline-start: 44px; margin: 0; font-size: 20px; font-weight: 700; }
  .s5 .cc svg { position: absolute; top: 70px; inset-inline-start: 12px; width: 620px; height: 350px; }
  .s5 .cc svg text { font-family: var(--font); }

  /* ---- 6 نموذج العمل ---- */
  .s6 { background: var(--bg); }
  .s6 .blob6 { top: -560px; inset-inline-start: 590px; width: 880px; height: 880px; background: radial-gradient(circle closest-side, #BCA7DC 0%, #CDBFE3 50%, #DDD3EB 78%, #EBE7F2 92%, rgba(244,244,244,0) 100%); }
  .s6 .lp { position: absolute; top: -2px; inset-inline-start: -2px; width: 720px; height: 724px; background: var(--white); border: 1px solid var(--accent); border-start-end-radius: 140px; }
  .s6 .bi { position: absolute; inset-inline-start: 92px; width: 480px; height: 130px; }
  .s6 .bi1 { top: 214px; } .s6 .bi2 { top: 384px; } .s6 .bi3 { top: 555px; }
  .s6 .bi .nb { position: absolute; top: 8px; inset-inline-start: 0; width: 66px; height: 70px; overflow: hidden; }
  .s6 .bi .nb span { display: block; margin-inline-start: -24px; font-size: 62px; }
  html[dir="rtl"] .s6 .bi .nb { width: 84px; } html[dir="rtl"] .s6 .bi .nb span { margin-inline-start: 0; }
  .s6 .bi h2 { position: absolute; top: 12px; inset-inline-start: 93px; margin: 0; font-size: 22px; font-weight: 700; color: var(--ink); line-height: 1.2; }
  .s6 .bi p { position: absolute; top: 50px; inset-inline-start: 93px; width: 340px; margin: 0; font-size: 17px; line-height: 1.5; color: var(--ink-2); }
  .s6 .dv { position: absolute; inset-inline-start: 48px; width: 524px; height: 2px; background: var(--accent); }
  .s6 .dv1 { top: 347px; } .s6 .dv2 { top: 517px; }
  .s6 .p0 { top: 366px; inset-inline-start: 773px; width: 430px; }
  .s6 .big { top: 492px; font-size: 48px; }
  .s6 .b1 { inset-inline-start: 773px; } .s6 .b2 { inset-inline-start: 1028px; }
  .s6 .lb { position: absolute; top: 565px; margin: 0; font-size: 22px; font-weight: 700; color: var(--ink); line-height: 1.35; width: 230px; }
  .s6 .l1 { inset-inline-start: 773px; } .s6 .l2 { inset-inline-start: 1028px; }

  /* ---- 7 متطلبات التمويل ---- */
  .s7 { background: var(--bg); }
  .s7 .blob7 { top: 95px; inset-inline-start: 525px; width: 970px; height: 970px; background: radial-gradient(circle closest-side, #E0D7ED 0%, #E6DFF0 60%, #EDE8F3 85%, rgba(244,244,244,0) 100%); }
  .s7 .pr { position: absolute; top: 315px; inset-inline-start: 0; width: 1280px; height: 405px; background: var(--accent); border-start-start-radius: 50px; border-start-end-radius: 50px; }
  .s7 .wc { position: absolute; top: 230px; inset-inline-start: 52px; width: 1176px; height: 378px; background: var(--white); border: 2px solid var(--accent); border-radius: 24px; }
  .s7 table { position: absolute; top: 35px; inset-inline-start: 43px; width: 1058px; border-collapse: collapse; font-size: 18px; color: var(--ink-2); }
  .s7 th { background: var(--accent); color: #fff; font-size: 22px; font-weight: 700; text-align: start; height: 59px; padding: 0 20px; }
  .s7 td { height: 50px; padding: 0 20px; text-align: start; }
  .s7 tr:nth-child(even) td { background: var(--row); }
  .s7 td.am { font-weight: 700; color: var(--ink); }
  .s7 .c1 { width: 264px; } .s7 .c2 { width: 219px; }
  .s7 .mo { position: absolute; top: 638px; inset-inline-start: 102px; margin: 0; font-size: 38px; font-weight: 700; color: #fff; line-height: 1.2; letter-spacing: -0.5px; }
  html[lang="ar"] .s7 .mo { letter-spacing: 0; font-size: 34px; }
  .s7 .p0 { top: 640px; inset-inline-start: 410px; width: 740px; color: #fff; }

  /* ---- 8 الفريق ---- */
  .s8 { background: var(--bg); }
  .s8 .blob8 { top: 440px; inset-inline-start: 340px; width: 1120px; height: 1120px; background: radial-gradient(circle closest-side, #9480BC 0%, #9D88C1 64%, #C0B2D8 78%, #E6DFF0 95%, rgba(244,244,244,0) 100%); }
  .s8 .dots { top: 345px; inset-inline-start: 84px; width: 62px; height: 62px; }
  .s8 .h3 { top: 438px; inset-inline-start: 87px; width: 300px; }
  .s8 .p0 { top: 524px; inset-inline-start: 87px; width: 290px; font-size: 16px; }
  .s8 .tc { position: absolute; top: 45px; width: 247px; height: 605px; background: var(--white); border-radius: 24px; overflow: hidden; }
  .s8 .tc1 { inset-inline-start: 482px; } .s8 .tc2 { inset-inline-start: 743px; } .s8 .tc3 { inset-inline-start: 1004px; }
  .s8 .tc img { position: absolute; top: 0; inset-inline-start: 0; width: 247px; height: 390px; object-fit: cover; display: block; }
  .s8 .tc .tag { position: absolute; top: 337px; inset-inline-start: 40px; width: 159px; height: 53px; background: var(--accent); color: #fff; font-size: 22px; font-weight: 700; line-height: 53px; text-align: center; border-start-start-radius: 8px; border-start-end-radius: 8px; }
  html[lang="ar"] .s8 .tc .tag { font-size: 18px; }
  .s8 .tc h2 { position: absolute; top: 422px; inset-inline-start: 27px; width: 200px; margin: 0; font-size: 24px; font-weight: 700; line-height: 1.35; color: var(--ink); }
  .s8 .tc p { position: absolute; top: 504px; inset-inline-start: 27px; width: 200px; margin: 0; font-size: 16px; line-height: 1.5; color: var(--ink-2); }

  /* ---- 9 استراتيجية التخارج ---- */
  .s9 { background: var(--bg); }
  .s9 .ec { position: absolute; top: 219px; width: 353px; height: 246px; background: var(--white); border: 2px solid var(--accent); border-radius: 24px; }
  .s9 .ec1 { inset-inline-start: 62px; } .s9 .ec2 { inset-inline-start: 463px; } .s9 .ec3 { inset-inline-start: 865px; }
  .s9 .ec h2 { position: absolute; top: 48px; inset-inline-start: 38px; margin: 0; font-size: 26px; font-weight: 700; color: var(--ink); line-height: 1.2; }
  .s9 .ec .ln { position: absolute; top: 116px; inset-inline-start: 24px; width: 304px; height: 1px; background: var(--line); }
  .s9 .ec p { position: absolute; top: 136px; inset-inline-start: 38px; width: 290px; margin: 0; font-size: 17px; line-height: 1.5; color: var(--ink-2); }
  .s9 .con { position: absolute; top: 452px; inset-inline-start: 0; width: 1280px; height: 90px; }
  .s9 .vb { position: absolute; top: 537px; inset-inline-start: 62px; width: 1157px; height: 158px; background: var(--accent); border-radius: 20px; color: #fff; }
  .s9 .vb h2 { position: absolute; top: 32px; inset-inline-start: 54px; margin: 0; font-size: 24px; font-weight: 700; }
  .s9 .vb p { position: absolute; top: 78px; inset-inline-start: 54px; width: 1040px; margin: 0; font-size: 17px; line-height: 1.5; }

  /* ---- 10 المخاطر والتخفيف ---- */
  .s10 { background: var(--bg); }
  .s10 .blob10 { top: -430px; inset-inline-start: -160px; width: 660px; height: 660px; background: radial-gradient(circle closest-side, #DDD3EC 0%, #E4DCEF 60%, #EEEAF4 92%, rgba(244,244,244,0) 100%); }
  .s10 .ttl { inset-inline-start: 0; width: 1280px; text-align: center; }
  .s10 .sub { inset-inline-start: 0; width: 1280px; text-align: center; }
  .s10 .wp { position: absolute; top: 205px; inset-inline-start: 0; width: 1280px; height: 515px; background: var(--white); border-start-start-radius: 60px; border-start-end-radius: 60px; }
  .s10 table { position: absolute; top: 237px; inset-inline-start: 52px; width: 1176px; border-collapse: collapse; font-size: 18px; color: var(--ink-2); }
  .s10 th { color: var(--accent); font-size: 22px; font-weight: 700; text-align: start; height: 73px; padding: 0 20px; border-bottom: 1px solid #D5D5D5; border-inline-start: 1px solid var(--line); }
  .s10 td { height: 71px; padding: 0 20px; text-align: start; border-bottom: 1px solid #ECECEC; border-inline-start: 1px solid var(--line); }
  .s10 th:first-child, .s10 td:first-child { border-inline-start: 0; }
  .s10 tr:last-child td { border-bottom: 0; }
  .s10 .k1 { width: 278px; } .s10 .k2 { width: 249px; } .s10 .k3 { width: 213px; } .s10 .k4 { width: 222px; }

  /* ---- 11 أسئلة وأجوبة ---- */
  .s11 .rp { position: absolute; top: 0; inset-inline-start: 563px; width: 717px; height: 720px; background: var(--accent); border-start-start-radius: 150px; color: #fff; }
  .s11 .qa { position: absolute; inset-inline-start: 30px; width: 657px; }
  .s11 .qa .dots { position: absolute; top: 2px; inset-inline-start: 60px; width: 40px; height: 40px; color: #fff; }
  .s11 .qa h2 { position: absolute; top: 4px; inset-inline-start: 125px; width: 520px; margin: 0; font-size: 22px; font-weight: 700; line-height: 1.4; }
  .s11 .qa p { position: absolute; top: 46px; inset-inline-start: 125px; width: 510px; margin: 0; font-size: 17px; line-height: 1.5; opacity: .9; }
  .s11 .q1 { top: 88px; } .s11 .q2 { top: 235px; } .s11 .q3 { top: 322px; } .s11 .q4 { top: 438px; } .s11 .q5 { top: 525px; } .s11 .q6 { top: 612px; }
  .s11 .ql { position: absolute; inset-inline-start: 30px; width: 657px; height: 1px; background: rgba(255,255,255,.28); }
  .s11 .ql1 { top: 212px; } .s11 .ql2 { top: 299px; } .s11 .ql3 { top: 414px; } .s11 .ql4 { top: 502px; } .s11 .ql5 { top: 588px; }
  .s11 .h3 { top: 432px; inset-inline-start: 87px; width: 320px; }
  .s11 .disc { position: absolute; top: 520px; inset-inline-start: 87px; margin: 0; font-size: 20px; font-weight: 700; color: var(--accent); }
  .s11 .p0 { top: 557px; inset-inline-start: 87px; width: 430px; }

  /* ---- 12 الخطوة التالية ---- */
  .s12 .blob12 { top: 455px; inset-inline-start: 669px; width: 782px; height: 782px; background: radial-gradient(circle farthest-side at 65% 70%, #A48AC9 0%, #C4B4DE 45%, #DBD0E9 80%, #EAE4F2 100%); }
  .s12 .tag { position: absolute; top: 115px; inset-inline-start: 0; width: 345px; height: 85px; background: var(--accent); border-start-end-radius: 24px; border-end-end-radius: 24px; color: #fff; font-size: 28px; font-weight: 700; letter-spacing: 5px; line-height: 85px; padding-inline-start: 87px; white-space: nowrap; }
  html[lang="ar"] .s12 .tag { letter-spacing: 0; }
  .s12 .ct { position: absolute; top: 238px; inset-inline-start: 87px; width: 820px; margin: 0; font-size: 64px; font-weight: 700; line-height: 1.07; color: var(--ink); letter-spacing: -0.8px; }
  html[lang="ar"] .s12 .ct { font-weight: 800; letter-spacing: 0; line-height: 1.2; font-size: 60px; }
  .s12 .logo { position: absolute; top: 60px; inset-inline-end: 70px; width: 76px; height: 76px; color: var(--accent); }
  .s12 .cn { position: absolute; top: 586px; display: flex; align-items: center; gap: 28px; font-size: 20px; color: var(--accent); }
  .s12 .cn .dots { position: static; width: 40px; height: 40px; }
  .s12 .cn1 { inset-inline-start: 85px; } .s12 .cn2 { inset-inline-start: 460px; }"""


def _dots():
    # أيقونة «نقاط» دائرية: شبكة نقاط يصغر قطرها كلما بعدت عن المركز
    out = []
    for gy in range(-3, 4):
        for gx in range(-3, 4):
            d = (gx * gx + gy * gy) ** 0.5
            r = 2.6 - 0.55 * d
            if r < 0.55: continue
            out.append('<circle cx="%.1f" cy="%.1f" r="%.2f"/>' % (20 + gx * 5.4, 20 + gy * 5.4, r))
    return '<svg viewBox="0 0 40 40" fill="currentColor" aria-hidden="true">' + ''.join(out) + '</svg>'


DOTS = _dots()
LOGO = '<svg viewBox="0 0 100 100" aria-hidden="true"><path d="M30 16 C45 13 62 12 76 16 C86 19 88 28 86 42 C84 58 86 70 80 80 C72 90 52 89 34 86 C20 84 14 76 14 60 C14 44 16 32 20 22 C22 18 26 17 30 16 Z" fill="none" stroke="currentColor" stroke-width="9" stroke-linejoin="round"/></svg>'
ICON_REFRESH = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M4.5 12a7.5 7.5 0 0 1 12.8-5.3L19 8.4"/><path d="M19 3.8v4.6h-4.6"/><path d="M19.5 12a7.5 7.5 0 0 1-12.8 5.3L5 15.6"/><path d="M5 20.2v-4.6h4.6"/></svg>'
ICON_ATOM = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.4" aria-hidden="true"><ellipse cx="12" cy="12" rx="10" ry="4"/><ellipse cx="12" cy="12" rx="10" ry="4" transform="rotate(60 12 12)"/><ellipse cx="12" cy="12" rx="10" ry="4" transform="rotate(-60 12 12)"/><circle cx="12" cy="12" r="1.6" fill="currentColor"/></svg>'


def img(name, cls, alt, extra=''):
    return '<img class="%s" alt="%s" src="%s%s.jpg"%s>' % (cls, alt, A, name, extra)


S1 = ('\n    <div class="hero">'
      + img('hero-top', 'ht', '§ALT_BUILDING§', ' style="top:0;inset-inline-start:0;width:1280px;height:292px"')
      + img('hero-bottom', 'hb', '§ALT_BUILDING§', ' style="top:292px;inset-inline-start:560px;width:720px;height:153px"')
      + img('sky-patch', 'sp', '', ' style="top:44px;inset-inline-start:78px;width:100px;height:100px"')
      + '<div class="logo">' + LOGO + '</div></div>'
      + '\n    <div class="panel"></div>'
      + '\n    <p class="kick">§S1_KICK§</p>\n    <h1 class="ct">§S1_TTL§</h1>\n    <div class="bar">§S1_BAR§</div>'
      + '\n    <p class="p s1p">§TXT_LONG§</p>'
      + '\n    <div class="pres"><span class="dots">' + DOTS + '</span><span>§S1_PRES§</span></div>\n  ')

S2 = ('\n    <div class="g2"></div><div class="w2"></div><div class="blob mint"></div>'
      + '\n    <h1 class="ttl">§S2_TTL§</h1><p class="sub two">§SUBHEAD§</p>'
      + '\n    <div class="dots">' + DOTS + '</div><h2 class="h3">§S2_FOCUS§</h2><p class="p">§TXT_LONG5§</p>'
      + '\n    <div class="card c1"><p class="num">$18.4M</p><h2>§S2_FOCUS§</h2><p>§TXT_SHORT§</p></div>'
      + img('exec-1', 'ph ph1', '§ALT_EXEC1§')
      + img('exec-2', 'ph ph2', '§ALT_EXEC2§')
      + '\n    <div class="card c2"><p class="num">31%</p><h2>§S2_GM§</h2><p>§TXT_SHORT§</p></div>\n  ')


def pcard(n):
    return ('\n    <div class="pc pc%d"><div class="tab">§S3_TAB%d§</div><h2>§S3_H%d§</h2><div class="ln"></div><p>§TXT_SHORT2§</p></div>' % (n, n, n))


S3 = ('\n    <div class="blob pink"></div><h1 class="ttl">§S3_TTL§</h1><p class="sub">§SUBHEAD§</p>'
      + pcard(1) + pcard(2) + pcard(3)
      + '\n    <div class="band"><div class="circ"></div><p>§S3_QUOTE§</p></div>\n  ')


def scard(n):
    return '\n    <div class="sc sc%d"><p class="n grad-num">0%d</p><h2>§S4_H%d§</h2><p>§TXT_MED§</p></div>' % (n, n, n)


S4 = ('\n    <div class="blob blob4"></div><h1 class="ttl">§S4_TTL§</h1><p class="sub">§SUBHEAD§</p><p class="p p0">§TXT_SHORT3§</p>'
      + scard(1) + scard(2) + scard(3) + scard(4) + '\n  ')

S5 = ('\n    <div class="blob blob5"></div><h1 class="ttl">§S5_TTL§</h1><p class="sub">§SUBHEAD§</p><p class="p p0">§TXT_LONG4§</p>'
      + '\n    <div class="kc"><div class="ic ic1">' + ICON_REFRESH + '</div><div class="ic ic2">' + ICON_ATOM + '</div><div class="vl"></div>'
      + '<p class="big b1">$6.2B</p><p class="lb l1">§S5_SIZE§</p><p class="big b2">62.5%</p><p class="lb l2">§S5_GROWTH§</p><div class="hl"></div><p>§TXT_LONG4§</p></div>'
      + '\n    <div class="cc"><h2>§S5_CHART§</h2><svg id="chart" viewBox="0 0 620 350" aria-hidden="true"></svg></div>\n  ')


def bitem(n):
    return '\n    <div class="bi bi%d"><div class="nb"><span class="grad-num">0%d</span></div><h2>§S6_H§</h2><p>§TXT_SHORT4§</p></div>' % (n, n)


S6 = ('\n    <div class="blob blob6"></div><div class="lp"></div><h1 class="ttl">§S6_TTL§</h1><p class="sub">§SUBHEAD§</p>'
      + bitem(1) + '<div class="dv dv1"></div>' + bitem(2) + '<div class="dv dv2"></div>' + bitem(3)
      + '\n    <p class="p p0">§TXT_LONG4§</p><p class="big b1">$84K</p><p class="lb l1">§S6_ACV§</p><p class="big b2">38%</p><p class="lb l2">§S6_GMT§</p>\n  ')

S7_ROWS = [('§S7_R1§', '$4.2M', '§TXT_SHORT§'), ('§S7_R2§', '$3.6M', '§TXT_ADD2§'), ('§S7_R3§', '$2.4M', '§TXT_ADD3§'), ('§S7_R4§', '$1.2M', '§TXT_ADD4§'), ('§S7_R5§', '$0.6M', '§TXT_ADD5§')]
S7 = ('\n    <div class="blob blob7"></div><div class="pr"></div><h1 class="ttl">§S7_TTL§</h1><p class="sub">§SUBHEAD§</p>'
      + '\n    <div class="wc"><table><thead><tr><th class="c1">§S7_C1§</th><th class="c2">§S7_C2§</th><th>§S7_C3§</th></tr></thead><tbody>'
      + ''.join('<tr><td>%s</td><td class="am">%s</td><td>%s</td></tr>' % r for r in S7_ROWS)
      + '</tbody></table></div>'
      + '\n    <p class="mo">§S7_MONTHS§</p><p class="p p0">§TXT_LONG6§</p>\n  ')


def tcard(n):
    return ('\n    <div class="tc tc%d">' % n + img('team-%d' % n, 'tp', '§ALT_T%d§' % n)
            + '<div class="tag">§S8_ROLE%d§</div><h2>§S8_NAME%d§</h2><p>§TXT_SHORT2§</p></div>' % (n, n))


S8 = ('\n    <div class="blob blob8"></div><h1 class="ttl">§S8_TTL§</h1><p class="sub">§SUBHEAD§</p>'
      + '\n    <div class="dots">' + DOTS + '</div><h2 class="h3">§S8_H§</h2><p class="p p0">§TXT_LONG5B§</p>'
      + tcard(1) + tcard(2) + tcard(3) + '\n  ')


def ecard(n):
    return '\n    <div class="ec ec%d"><h2>§S9_H%d§</h2><div class="ln"></div><p>§TXT_SHORT2§</p></div>' % (n, n)


CONNECT = ('<svg class="con" viewBox="0 0 1280 90" aria-hidden="true">'
           '<path d="M211 25 V30 Q211 48 229 48 H1049 Q1067 48 1067 30 V25 M639 25 V90" fill="none" stroke="#5036F9" stroke-width="2"/>'
           + ''.join('<circle cx="%d" cy="13" r="12" fill="#AFA3FF"/><circle cx="%d" cy="13" r="6" fill="#170692"/>' % (x, x) for x in (211, 639, 1067))
           + '</svg>')
S9 = ('\n    <div class="blob pink"></div><h1 class="ttl">§S9_TTL§</h1><p class="sub">§SUBHEAD§</p>'
      + ecard(1) + ecard(2) + ecard(3) + '\n    ' + CONNECT
      + '\n    <div class="vb"><h2>§S9_VC§</h2><p>§TXT_LONG7§</p></div>\n  ')

S10_ROWS = [('§S10_R1§', '§S10_MED§', '§S10_T1§'), ('§S10_R2§', '§S10_HIGH§', '§S10_T2§'), ('§S10_R3§', '§S10_MED§', '§S10_T3§'), ('§S10_R4§', '§S10_MED§', '§S10_T4§'), ('§S10_R5§', '§S10_MED§', '§S10_T5§')]
S10 = ('\n    <div class="blob blob10"></div><div class="wp"></div><h1 class="ttl">§S10_TTL§</h1><p class="sub">§SUBHEAD§</p>'
       + '\n    <table><thead><tr><th class="k1">§S10_C1§</th><th class="k2">§S10_C2§</th><th class="k3">§S10_C3§</th><th class="k4">§S10_C4§</th><th>§S10_C5§</th></tr></thead><tbody>'
       + ''.join('<tr><td>%s</td><td>%s</td><td>§TXT_ADD§</td><td>%s</td><td>§TXT_ADD§</td></tr>' % r for r in S10_ROWS)
       + '</tbody></table>\n  ')


def qa(n, desc):
    return ('\n      <div class="qa q%d"><span class="dots">' % n + DOTS + '</span><h2>§S11_Q%d§</h2>' % n
            + ('<p>§TXT_MED§</p>' if desc else '') + '</div>')


S11 = ('\n    <div class="rp">' + ''.join(qa(n, n == 1) for n in range(1, 7))
       + ''.join('<div class="ql ql%d"></div>' % n for n in range(1, 6)) + '\n    </div>'
       + '\n    <h1 class="ttl">§S11_TTL§</h1><p class="sub two">§SUBHEAD§</p>'
       + '\n    <h2 class="h3">§S11_H§</h2><p class="disc">§S11_DISC§</p><p class="p p0">§TXT_LONG5B§</p>\n  ')

S12 = ('\n    <div class="blob blob12"></div><div class="logo">' + LOGO + '</div>'
       + '\n    <div class="tag">§S12_TAG§</div><h1 class="ct">§S12_TTL§</h1>'
       + '\n    <div class="cn cn1"><span class="dots">' + DOTS + '</span><span>§S12_EMAIL§</span></div>'
       + '\n    <div class="cn cn2"><span class="dots">' + DOTS + '</span><span>+1 (234) 567 8901</span></div>\n  ')

SLIDES = [S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12]

JSON_BLOCKS = {'chart-data': {
    'ar': {'categories': ['الشركات الكبرى', 'السوق المتوسطة', 'الشركات النامية'], 'max': 5, 'step': 0.5,
           'series': [{'name': 'السلسلة 1', 'color': '#FFFFFF', 'values': [4.2, 3.5, 2.7]}, {'name': 'السلسلة 2', 'color': '#BAAFFD', 'values': [2.4, 4.4, 1.7]}]},
    'en': {'categories': ['Core Enterprise', 'Mid-Market', 'Growth Businesses'], 'max': 5, 'step': 0.5,
           'series': [{'name': 'Series 1', 'color': '#FFFFFF', 'values': [4.2, 3.5, 2.7]}, {'name': 'Series 2', 'color': '#BAAFFD', 'values': [2.4, 4.4, 1.7]}]},
}}

EXTRA_SCRIPT = """  // رسم أعمدة مجمّعة من كتلة chart-data — الفئة الأولى في جهة البداية (اليمين للعربية)
  function drawChart() {
    var el = document.getElementById('chart'); if (!el) return;
    var d = JSON.parse(document.getElementById('chart-data').textContent);
    var W = 620, padS = 59, padE = 24, top = 13, base = 317;
    var n = d.categories.length, inner = W - padS - padE, gw = inner / n, bw = gw * 0.38;
    var y = function (v) { return base - (v / d.max) * (base - top); };
    var X = function (x) { return RTL ? W - x : x; };
    var s = '';
    for (var v = 0; v <= d.max + 1e-9; v += d.step) {
      s += '<text x="' + X(padS - 14) + '" y="' + (y(v) + 4.5) + '" text-anchor="' + (RTL ? 'start' : 'end') + '" font-size="13" fill="#fff">' + (Math.round(v * 10) / 10) + '</text>';
    }
    s += '<line x1="' + X(padS) + '" x2="' + X(W - padE) + '" y1="' + base + '" y2="' + base + '" stroke="#fff" stroke-width="1"/>';
    d.categories.forEach(function (c, i) {
      var cx = padS + gw * i + gw / 2;
      d.series.forEach(function (se, j) {
        var x = cx - bw + j * bw, h = base - y(se.values[i]);
        s += '<rect x="' + (RTL ? X(x + bw) : x) + '" y="' + y(se.values[i]) + '" width="' + bw + '" height="' + h + '" fill="' + se.color + '"/>';
      });
      s += '<text x="' + X(cx) + '" y="' + (base + 24) + '" text-anchor="middle" font-size="13" fill="#fff">' + c + '</text>';
    });
    el.innerHTML = s;
  }
  drawChart();"""

AR_SHORT = 'هذا نص توضيحي. أضف النص المطلوب هنا.'
STR = {
    'ar': {
        'SUBHEAD': 'أضف عنوانًا فرعيًا هنا',
        'TXT_SHORT': AR_SHORT,
        'TXT_SHORT2': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف النص هنا.',
        'TXT_SHORT3': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص هنا.',
        'TXT_SHORT4': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصك هنا.',
        'TXT_MED': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف النص هنا. هذا نص توضيحي.',
        'TXT_LONG': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف النص هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي.',
        'TXT_LONG4': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف النص هنا. هذا نص توضيحي. أضف النص المطلوب هنا. أضف النص هنا.',
        'TXT_LONG5': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف النص هنا. هذا نص توضيحي. أضف النص المطلوب هنا. أضف النص هنا. أضف النص هنا. هذا نص توضيحي.',
        'TXT_LONG5B': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف النص هنا. أضف النص المطلوب هنا. أضف النص هنا. هذا نص توضيحي',
        'TXT_LONG6': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص هنا. أضف النص المطلوب هنا. أضف النص هنا. هذا نص توضيحي. أضف النص هنا.',
        'TXT_LONG7': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف النص هنا. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. أضف النص هنا. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا.',
        'TXT_ADD': 'أضف النص هنا',
        'TXT_ADD2': 'أضف النص المطلوب هنا. أضف النص هنا.',
        'TXT_ADD3': 'هذا نص توضيحي. أضف النص هنا. أضف النص المطلوب هنا.',
        'TXT_ADD4': 'أضف النص هنا. هذا نص توضيحي.',
        'TXT_ADD5': 'أضف نصك هنا. أضف النص المطلوب هنا.',
        'ALT_BUILDING': 'مبنى حديث بواجهة بيضاء', 'ALT_EXEC1': 'سيدتان تراجعان مستندًا أمام حاسوب محمول', 'ALT_EXEC2': 'رجلان يناقشان مخططات على طاولة',
        'ALT_T1': 'صورة دانيال بروكسمان', 'ALT_T2': 'صورة مايا تشينوود', 'ALT_T3': 'صورة صوفيا مارتينيز',
        'S1_KICK': 'تجريدي', 'S1_TTL': 'استثمار<br>الشركة', 'S1_BAR': 'قالب عرض تقديمي', 'S1_PRES': 'أضف اسم المقدّم أو التاريخ هنا',
        'S2_TTL': 'الملخص<br>التنفيذي', 'S2_FOCUS': 'محور الاستثمار', 'S2_GM': 'هامش الربح الإجمالي',
        'S3_TTL': 'بيان المشكلة', 'S3_TAB1': 'المشكلة 1', 'S3_TAB2': 'المشكلة 2', 'S3_TAB3': 'المشكلة 3',
        'S3_H1': 'عمليات<br>مجزّأة', 'S3_H2': 'رؤية<br>محدودة', 'S3_H3': 'احتكاك مع<br>العملاء',
        'S3_QUOTE': 'الفجوة ليست في الطلب، بل في مدى<br>كفاءة الشركات في تلبيته.',
        'S4_TTL': 'الحل', 'S4_H1': 'ربط مسارات العمل الأساسية', 'S4_H2': 'تبسيط العمليات اليومية', 'S4_H3': 'توسيع الطاقة التشغيلية', 'S4_H4': 'تحسين تجربة العملاء',
        'S5_TTL': 'تحليل السوق', 'S5_SIZE': 'حجم 3032', 'S5_GROWTH': 'النمو', 'S5_CHART': 'أضف عنوان الرسم',
        'S6_TTL': 'نموذج العمل', 'S6_H': 'خدمات متكررة', 'S6_ACV': 'متوسط قيمة العميل', 'S6_GMT': 'مستهدف هامش الربح الإجمالي',
        'S7_TTL': 'متطلبات التمويل', 'S7_C1': 'التخصيص', 'S7_C2': 'المبلغ', 'S7_C3': 'الغرض',
        'S7_R1': 'التوسع في السوق', 'S7_R2': 'التقنية', 'S7_R3': 'العمليات', 'S7_R4': 'الكفاءات', 'S7_R5': 'الاحتياطي', 'S7_MONTHS': '18–24 شهرًا',
        'S8_TTL': 'الفريق', 'S8_H': 'خبرة مبنية<br>للمرحلة القادمة', 'S8_ROLE1': 'الرئيس التنفيذي', 'S8_ROLE2': 'رئيس العمليات', 'S8_ROLE3': 'الرئيس المالي',
        'S8_NAME1': 'دانيال<br>بروكسمان', 'S8_NAME2': 'مايا<br>تشينوود', 'S8_NAME3': 'صوفيا<br>مارتينيز',
        'S9_TTL': 'استراتيجية التخارج', 'S9_H1': 'استحواذ استراتيجي', 'S9_H2': 'الملكية الخاصة', 'S9_H3': 'السوق العامة', 'S9_VC': 'أولويات خلق القيمة',
        'S10_TTL': 'المخاطر والتخفيف', 'S10_C1': 'الخطر الرئيسي', 'S10_C2': 'الأثر المحتمل', 'S10_C3': 'التخفيف', 'S10_C4': 'المسؤول', 'S10_C5': 'الحالة',
        'S10_R1': 'التوسع في السوق', 'S10_R2': 'تركّز العملاء', 'S10_R3': 'توسيع التقنية', 'S10_R4': 'طاقة الكفاءات', 'S10_R5': 'ضغط التكاليف',
        'S10_MED': 'متوسط', 'S10_HIGH': 'مرتفع', 'S10_T1': 'فريق النمو', 'S10_T2': 'فريق المبيعات', 'S10_T3': 'فريق التقنية', 'S10_T4': 'فريق الكفاءات', 'S10_T5': 'الفريق المالي',
        'S11_TTL': 'أسئلة<br>وأجوبة', 'S11_H': 'الأسئلة ترسم ملامح<br>الحوار القادم', 'S11_DISC': 'نقاش الاستثمار',
        'S11_Q1': 'ما فرصة الاستثمار الحالية؟', 'S11_Q2': 'كيف تحقق الشركة إيراداتها؟', 'S11_Q3': 'ما الذي يميّز الشركة<br>عن المنافسين؟',
        'S11_Q4': 'كيف سيُوزَّع التمويل؟', 'S11_Q5': 'ما افتراضات النمو الرئيسية؟', 'S11_Q6': 'ما المخاطر الرئيسية على النمو؟',
        'S12_TAG': 'الخطوة التالية', 'S12_TTL': 'نبني ما هو قادم<br>بهدف واضح<br>وثقة راسخة', 'S12_EMAIL': 'أدخل بريدك الإلكتروني هنا',
    },
    'en': {
        'SUBHEAD': 'Add subheading here',
        'TXT_SHORT': 'This is a sample text. add your desired text here.',
        'TXT_SHORT2': 'This is a sample text. Add your desired text here. Add text here.',
        'TXT_SHORT3': 'This is a sample text. Add your desired text here. This is a sample text. Add text here.',
        'TXT_SHORT4': 'This is a sample text. Add your desired text here. Add your text here.',
        'TXT_MED': 'This is a sample text. Add your desired text here. Add text here. This is a sample text.',
        'TXT_LONG': 'This is a sample text. Add your desired text here. Add text here. This is a sample text. Add your desired text here. This is a sample text.',
        'TXT_LONG4': 'This is a sample text. Add your desired text here. Add text here. This is a sample text. Add your desired text here. Add text here.',
        'TXT_LONG5': 'This is a sample text. Add your desired text here. Add text here. This is a sample text. Add your desired text here. Add text here. Add text here. This is a sample text.',
        'TXT_LONG5B': 'This is a sample text. Add your desired text here. Add text here. Add your desired text here. Add text here. This is a sample text',
        'TXT_LONG6': 'This is a sample text. Add your desired text here. This is a sample text. Add text here. Add your desired text here. Add text here. This is a sample text. Add text here.',
        'TXT_LONG7': 'This is a sample text. add your desired text here. Add text here. Add your desired text here. This is a sample text. add your desired text here. Add text here. Add your desired text here. This is a sample text. add your desired text here.',
        'TXT_ADD': 'Add text here',
        'TXT_ADD2': 'Add your desired text here. Add text here.',
        'TXT_ADD3': 'This is a sample text. Add text here. Add your text here.',
        'TXT_ADD4': 'Add text here. This is a sample text.',
        'TXT_ADD5': 'Add your text here. Add your desired text here.',
        'ALT_BUILDING': 'Modern building with a white facade', 'ALT_EXEC1': 'Two women reviewing a document at a laptop', 'ALT_EXEC2': 'Two men discussing plans at a table',
        'ALT_T1': 'Portrait of Daniel Brooksman', 'ALT_T2': 'Portrait of Maya Chenwood', 'ALT_T3': 'Portrait of Sofia Martinez',
        'S1_KICK': 'ABSTRACT', 'S1_TTL': 'COMPANY<br>INVESTMENT', 'S1_BAR': 'PRESENTATION TEMPLATE', 'S1_PRES': 'Add presenter or date here',
        'S2_TTL': 'Executive<br>Summary', 'S2_FOCUS': 'Investment Focus', 'S2_GM': 'Gross Margin',
        'S3_TTL': 'Problem Statement', 'S3_TAB1': 'Problem 1', 'S3_TAB2': 'Problem 2', 'S3_TAB3': 'Problem 3',
        'S3_H1': 'Fragmented<br>Operations', 'S3_H2': 'Limited<br>Visibility', 'S3_H3': 'Customer<br>Friction',
        'S3_QUOTE': 'The gap is not demand. It is how<br>efficiently businesses can serve it.',
        'S4_TTL': 'Solution', 'S4_H1': 'Connect Core Workflows', 'S4_H2': 'Simplify Daily Operations', 'S4_H3': 'Scale Business Capacity', 'S4_H4': 'Improve Customer Experience',
        'S5_TTL': 'Market Analysis', 'S5_SIZE': '3032 Size', 'S5_GROWTH': 'Growth', 'S5_CHART': 'Add chart title',
        'S6_TTL': 'Business Model', 'S6_H': 'Recurring Services', 'S6_ACV': 'Average<br>Customer Value', 'S6_GMT': 'Gross<br>Margin Target',
        'S7_TTL': 'Funding Requirements', 'S7_C1': 'Allocation', 'S7_C2': 'Amount', 'S7_C3': 'Purpose',
        'S7_R1': 'Market Expansion', 'S7_R2': 'Technology', 'S7_R3': 'Operations', 'S7_R4': 'Talent', 'S7_R5': 'Reserve', 'S7_MONTHS': '18–24 months',
        'S8_TTL': 'Team', 'S8_H': 'Experience Built<br>for the Next Chapter', 'S8_ROLE1': 'CEO', 'S8_ROLE2': 'COO', 'S8_ROLE3': 'CFO',
        'S8_NAME1': 'Daniel<br>Brooksman', 'S8_NAME2': 'Maya<br>Chenwood', 'S8_NAME3': 'Sofia<br>Martinez',
        'S9_TTL': 'Exit Strategy', 'S9_H1': 'Strategic Acquisition', 'S9_H2': 'Private Equity', 'S9_H3': 'Public Market', 'S9_VC': 'Value Creation Priorities',
        'S10_TTL': 'Risks &amp; Mitigation', 'S10_C1': 'Key Risk', 'S10_C2': 'Potential Impact', 'S10_C3': 'Mitigation', 'S10_C4': 'Owner', 'S10_C5': 'Status',
        'S10_R1': 'Market Expansion', 'S10_R2': 'Customer Concentration', 'S10_R3': 'Technology Scaling', 'S10_R4': 'Talent Capacity', 'S10_R5': 'Cost Pressure',
        'S10_MED': 'Medium', 'S10_HIGH': 'High', 'S10_T1': 'Growth Team', 'S10_T2': 'Sales Team', 'S10_T3': 'Tech Team', 'S10_T4': 'People Team', 'S10_T5': 'Finance Team',
        'S11_TTL': 'Question<br>And Answer', 'S11_H': 'Questions Shape<br>the Next Conversation', 'S11_DISC': 'Investment Discussion',
        'S11_Q1': 'What is the current investment opportunity?', 'S11_Q2': 'How does the company generate revenue?', 'S11_Q3': 'What differentiates the business<br>from competitors?',
        'S11_Q4': 'How will the funding be allocated?', 'S11_Q5': 'What are the key growth assumptions?', 'S11_Q6': 'What are the main risks to growth?',
        'S12_TAG': 'NEXT STEP', 'S12_TTL': 'Building What Comes<br>Next With Purpose<br>and Confidence', 'S12_EMAIL': 'Insert your email here',
    },
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'finance', 'icon': '💼',
    'name': 'استثمار الشركة (تجريدي)', 'nameEn': 'Abstract Company Investment',
    'description': '12 شريحة لعرض استثماري: غلاف، ملخص، مشكلة، حل، سوق، نموذج عمل، تمويل، فريق، تخارج، مخاطر، أسئلة، خاتمة',
    'descriptionEn': '12 investor slides: cover, summary, problem, solution, market, model, funding, team, exit, risks, Q&A, close',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'الملخص التنفيذي', 'titleEn': 'Executive Summary', 'needsData': True},
        {'title': 'بيان المشكلة', 'titleEn': 'Problem Statement'},
        {'title': 'الحل', 'titleEn': 'Solution'},
        {'title': 'تحليل السوق (رسم)', 'titleEn': 'Market Analysis (chart)', 'needsData': True},
        {'title': 'نموذج العمل', 'titleEn': 'Business Model', 'needsData': True},
        {'title': 'متطلبات التمويل (جدول)', 'titleEn': 'Funding Requirements (table)', 'needsData': True},
        {'title': 'الفريق', 'titleEn': 'Team'},
        {'title': 'استراتيجية التخارج', 'titleEn': 'Exit Strategy'},
        {'title': 'المخاطر والتخفيف (جدول)', 'titleEn': 'Risks & Mitigation (table)', 'needsData': True},
        {'title': 'أسئلة وأجوبة', 'titleEn': 'Question and Answer'},
        {'title': 'الخطوة التالية', 'titleEn': 'Next Step'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص والأرقام فقط؛ أرقام رسم الأعمدة في كتلة JSON بالمعرّف chart-data، والصور في assets تبقى بمساراتها.',
}
