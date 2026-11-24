# -*- coding: utf-8 -*-
# نموذج «الخطة الاستراتيجية للذكاء الاصطناعي»: 14 شريحة بهوية بنفسجية/أرجوانية/زرقاء —
# غلاف بموجات وجوهرة سداسية، ملخص تنفيذي ببطاقات متدرجة، الوضع الراهن، الرؤية والرسالة بصورة،
# ركائز، مصفوفة حالات الاستخدام، استراتيجية البيانات، التقنية، الهيكل التنظيمي، خارطة طريق زمنية،
# مصفوفة مخاطر، نتائج متوقعة برسم خطي، خطوات تالية بسداسيات، وشريحة شكر.
KEY = 'sl-ai-strategic-plan'
TITLE = {'ar': 'الخطة الاستراتيجية للذكاء الاصطناعي', 'en': 'AI Strategic Plan'}
COMMENT = 'نموذج «الخطة الاستراتيجية للذكاء الاصطناعي»: 14 شريحة بعنوان بنفسجي أعلى البداية، وأشكال سداسية مقوّسة (الشعار) بتدرّج أرجواني→أزرق تزيّن الزوايا، ولوحات جانبية متدرّجة، وبطاقات بيضاء مدوّرة، ورسم خطي SVG من كتلة chart-data في شريحة النتائج. الصورتان الفوتوغرافيتان في assets.'
VARS = '--ink: #1E1E1E; --accent: #6A1B7A; --bg: #FFFFFF; --purple: #6A1B7A; --magenta: #A51EBA; --blue: #1414DD; --navy: #0A0A80; --dark: #0B0B5E; --lav: #CFD4F7; --gray: #EFEFEF; --gray-2: #C9C9D6; --muted: #8A8A95; --green: #2EAD4B;'

A = '/templates/freeform/sl-ai-strategic-plan/assets/'

STYLE = """  /* ---- أنماط مشتركة ---- */
  .ab { position: absolute; margin: 0; }
  .ttl { position: absolute; top: 108px; inset-inline-start: 112px; margin: 0; font-size: 44px; font-weight: 700; line-height: 1.25; color: var(--purple); letter-spacing: -0.3px; white-space: nowrap; }
  .ttl.wr { white-space: normal; }
  html[lang="ar"] .ttl { font-weight: 800; letter-spacing: 0; }
  .p { font-size: 18px; line-height: 1.3; color: var(--ink); }
  .h { font-weight: 700; font-size: 20px; line-height: 1.25; }
  .c { text-align: center; }
  .wt { color: #fff; }
  .deco { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; pointer-events: none; }
  html[dir="rtl"] .deco, html[dir="rtl"] .flip { transform: scaleX(-1); }
  .gl { stroke-width: 16px; stroke-linejoin: round; }
  /* تدرّجات CSS المائلة تُعكس زاويتها في العربية */
  .g-mb { background: linear-gradient(135deg, var(--magenta), var(--blue)); }
  html[dir="rtl"] .g-mb { background: linear-gradient(225deg, var(--magenta), var(--blue)); }
  .g-mb2 { background: linear-gradient(225deg, var(--magenta), var(--blue)); }
  html[dir="rtl"] .g-mb2 { background: linear-gradient(135deg, var(--magenta), var(--blue)); }
  .g-mp { background: linear-gradient(135deg, var(--magenta), var(--purple)); }
  html[dir="rtl"] .g-mp { background: linear-gradient(225deg, var(--magenta), var(--purple)); }
  .g-mbh { background: linear-gradient(90deg, var(--magenta), var(--blue)); }
  html[dir="rtl"] .g-mbh { background: linear-gradient(270deg, var(--magenta), var(--blue)); }
  .g-bmh { background: linear-gradient(90deg, var(--blue), var(--magenta)); }
  html[dir="rtl"] .g-bmh { background: linear-gradient(270deg, var(--blue), var(--magenta)); }
  .g-gray { background: linear-gradient(90deg, #E6E6E6, #CFCFCF); }
  html[dir="rtl"] .g-gray { background: linear-gradient(270deg, #E6E6E6, #CFCFCF); }
  .stripes { background-image: repeating-linear-gradient(-35deg, rgb(255 255 255 / .05) 0 36px, transparent 36px 72px); }
  .ic svg { width: 100%; height: 100%; display: block; }
  /* إطار سداسي مقوّس بلون لافندر يحمل أيقونة (الشريحة 2) */
  .hf { position: absolute; width: 115px; height: 115px; }
  .hf svg.fr { position: absolute; top: 0; inset-inline-start: 0; width: 115px; height: 115px; }
  .hf .ic { position: absolute; top: 8px; inset-inline-start: 60px; width: 46px; height: 46px; color: var(--purple); }
  /* شعار الشركة */
  .logo { position: absolute; display: flex; align-items: center; gap: 20px; }
  .logo .mk { width: 60px; height: 60px; color: var(--purple); }
  .logo p { margin: 0; font-size: 14px; letter-spacing: 1.6px; text-transform: uppercase; line-height: 1.35; color: #222; }
  html[lang="ar"] .logo p { letter-spacing: 0; font-size: 15px; }

  /* ---- 1 الغلاف ---- */
  .s1 .t { top: 246px; inset-inline-start: 112px; width: 450px; font-size: 60px; font-weight: 700; line-height: 1.17; color: var(--purple); text-transform: uppercase; }
  html[lang="ar"] .s1 .t { width: 560px; font-size: 54px; font-weight: 800; line-height: 1.3; top: 236px; }
  .s1 .st { top: 418px; inset-inline-start: 112px; font-size: 30px; letter-spacing: 1px; color: var(--blue); font-weight: 400; }
  html[lang="ar"] .s1 .st { letter-spacing: 0; font-weight: 500; }
  .s1 .bt { top: 540px; inset-inline-start: 112px; width: 340px; font-size: 20px; line-height: 1.45; color: #fff; }
  .s1 .logo { top: 95px; inset-inline-start: 942px; }

  /* ---- 2 الملخص التنفيذي ---- */
  .s2 .panel { position: absolute; top: 0; inset-inline-start: 972px; width: 308px; height: 720px; background: linear-gradient(180deg, var(--magenta), var(--blue)); }
  .s2 .lead { top: 195px; inset-inline-start: 112px; width: 500px; font-size: 22px; line-height: 1.35; }
  .s2 .card { position: absolute; width: 275px; background: linear-gradient(180deg, #FFFFFF 0%, #ECECFD 35%, #C9C9F8 100%); }
  .s2 .card .hf { top: 38px; inset-inline-start: 32px; }
  .s2 .card h2 { position: absolute; top: 186px; inset-inline-start: 35px; margin: 0; font-size: 22px; font-weight: 700; color: var(--purple); }
  .s2 .card p { position: absolute; top: 224px; inset-inline-start: 35px; width: 220px; margin: 0; }
  .s2 .kv { position: absolute; inset-inline-start: 1012px; width: 230px; color: #fff; }
  .s2 .kv .n { margin: 0; font-size: 44px; font-weight: 700; line-height: 1; }
  .s2 .kv .ln { width: 66px; height: 3px; background: #fff; margin: 22px 0 18px; }
  .s2 .kv h3 { margin: 0 0 8px; font-size: 20px; font-weight: 700; }
  .s2 .kv p { margin: 0; width: 180px; color: #fff; }

  /* ---- 3 الوضع الراهن ---- */
  .s3 .cs { top: 288px; inset-inline-start: 113px; font-size: 26px; color: #fff; }
  .s3 .ct { top: 343px; inset-inline-start: 113px; width: 300px; color: #fff; font-size: 18px; }
  .s3 .big { position: absolute; top: 436px; margin: 0; font-size: 46px; font-weight: 700; color: #fff; line-height: 1; }
  .s3 .big sup { font-size: 24px; vertical-align: top; position: relative; top: 2px; }
  .s3 .ul { position: absolute; top: 504px; width: 70px; height: 3px; background: #fff; }
  .s3 .kh { position: absolute; top: 534px; margin: 0; color: #fff; }
  .s3 .kp { position: absolute; top: 570px; margin: 0; width: 190px; color: #fff; font-size: 17px; }
  .s3 .sap { top: 250px; inset-inline-start: 596px; width: 200px; font-size: 24px; line-height: 1.3; color: #111; }
  .s3 .st { position: absolute; width: 188px; height: 52px; border-radius: 6px; background: var(--c); color: #fff; font-weight: 700; font-size: 20px; display: grid; place-items: center; box-shadow: 0 16px 16px -10px rgb(0 0 0 / .3); }
  .s3 .st::after { content: ''; position: absolute; top: 100%; inset-inline-start: calc(50% - 14px); border: 14px solid transparent; border-top-color: var(--c); }
  .s3 .st1 { --c: var(--purple); top: 414px; inset-inline-start: 596px; }
  .s3 .st2 { --c: var(--magenta); top: 334px; inset-inline-start: 802px; }
  .s3 .st3 { --c: var(--navy); top: 252px; inset-inline-start: 1010px; }
  .s3 .sd { position: absolute; width: 200px; text-align: center; }
  .s3 .ac { position: absolute; width: 28px; height: 28px; border: 2px solid #BDBDBD; border-radius: 50%; color: #BDBDBD; }
  .s3 .ac svg { width: 100%; height: 100%; display: block; }

  /* ---- 4 الرؤية والرسالة ---- */
  .s4 .photo { position: absolute; top: 0; inset-inline-start: 800px; width: 480px; height: 490px; object-fit: cover; clip-path: polygon(15% 0, 100% 0, 100% 50%, 76% 50%, 76% 100%, 42% 100%, 0 19%, 0 11%); }
  html[dir="rtl"] .s4 .photo { clip-path: polygon(85% 0, 0 0, 0 50%, 24% 50%, 24% 100%, 58% 100%, 100% 19%, 100% 11%); }
  .s4 .vm { position: absolute; margin: 0; font-size: 28px; font-weight: 400; color: #C4BCDD; line-height: 1.2; }
  .s4 .vm b { font-weight: 700; }
  .s4 .vm.v { top: 320px; inset-inline-start: 90px; width: 268px; text-align: end; } .s4 .vm.v b { color: var(--purple); }
  .s4 .vm.m { top: 348px; inset-inline-start: 633px; width: 300px; } .s4 .vm.m b { color: var(--navy); }
  .s4 .vt { position: absolute; margin: 0; font-size: 19px; }
  .s4 .vt.v { top: 362px; inset-inline-start: 90px; width: 268px; text-align: end; }
  .s4 .vt.m { top: 393px; inset-inline-start: 633px; width: 300px; }
  .s4 .row { position: absolute; width: 365px; height: 60px; line-height: 60px; padding-inline-start: 30px; font-size: 19px; background: linear-gradient(90deg, #EBEBEB, #F6F6F6 55%, #FFFFFF); }
  html[dir="rtl"] .s4 .row { background: linear-gradient(270deg, #EBEBEB, #F6F6F6 55%, #FFFFFF); }

  /* ---- 5 الركائز ---- */
  .s5 .band { position: absolute; top: 465px; inset-inline-start: 0; width: 1280px; height: 255px; background: linear-gradient(20deg, var(--blue), var(--purple) 75%); }
  html[dir="rtl"] .s5 .band { background: linear-gradient(340deg, var(--blue), var(--purple) 75%); }
  .s5 .card { position: absolute; top: 255px; width: 244px; height: 360px; background: #fff; border-radius: 30px; box-shadow: 0 8px 24px rgb(40 20 90 / .14); }
  .s5 .card .cp { position: absolute; top: 132px; inset-inline-start: 24px; width: 196px; margin: 0; font-size: 20px; line-height: 1.3; }
  .s5 .card ul { position: absolute; top: 250px; inset-inline-start: 24px; margin: 0; padding-inline-start: 18px; font-size: 16px; line-height: 2.05; }
  .s5 .hp { position: absolute; top: 258px; width: 170px; margin: 0; text-align: center; color: var(--purple); font-size: 20px; font-weight: 700; line-height: 1.25; }

  /* ---- 6 حالات الاستخدام ---- */
  .s6 .panel { position: absolute; top: 185px; inset-inline-start: 750px; width: 530px; height: 535px; border-start-start-radius: 60px; }
  .s6 .q { position: absolute; top: 225px; height: 375px; width: 262px; background: linear-gradient(180deg, #FFFFFF, #E6E6FF 70%); }
  .s6 .bub { position: absolute; border-radius: 50%; color: #fff; font-weight: 700; font-size: 15px; line-height: 1.25; display: grid; place-items: center; text-align: center; padding: 10px; }
  .s6 .ax { position: absolute; margin: 0; font-size: 17px; font-weight: 700; color: #111; }
  .s6 .num { position: absolute; margin: 0; font-size: 40px; font-weight: 300; color: #9CB0F0; line-height: 1; }
  .s6 .ph { position: absolute; margin: 0; color: #fff; font-size: 22px; font-weight: 700; }
  .s6 .pp { position: absolute; margin: 0; color: #fff; width: 300px; }

  /* ---- 7 استراتيجية البيانات ---- */
  .s7 .callout { position: absolute; top: 112px; inset-inline-start: 718px; width: 450px; height: 116px; background: #EFEFEF; box-shadow: 0 10px 20px rgb(0 0 0 / .18); }
  .s7 .callout::before { content: ''; position: absolute; top: 0; bottom: 0; inset-inline-start: 0; width: 24px; background: var(--purple); }
  .s7 .callout p { position: absolute; top: 18px; inset-inline-start: 50px; width: 350px; margin: 0; font-size: 19px; line-height: 1.35; }
  .s7 .hp { position: absolute; inset-inline-start: 110px; margin: 0; font-size: 20px; font-weight: 700; color: #111; }
  .s7 .seg { position: absolute; top: 284px; height: 59px; width: 221px; display: grid; place-items: center; font-weight: 700; font-size: 17px; clip-path: polygon(0 0, calc(100% - 18px) 0, 100% 50%, calc(100% - 18px) 100%, 0 100%, 18px 50%); }
  html[dir="rtl"] .s7 .seg { clip-path: polygon(100% 0, 18px 0, 0 50%, 18px 100%, 100% 100%, calc(100% - 18px) 50%); }
  .s7 .seg.first { clip-path: polygon(0 0, calc(100% - 18px) 0, 100% 50%, calc(100% - 18px) 100%, 0 100%); }
  html[dir="rtl"] .s7 .seg.first { clip-path: polygon(100% 0, 18px 0, 0 50%, 18px 100%, 100% 100%); }
  .s7 .seg.last { width: 248px; border-start-end-radius: 30px; border-end-end-radius: 30px; clip-path: polygon(0 0, 100% 0, 100% 100%, 0 100%, 18px 50%); }
  html[dir="rtl"] .s7 .seg.last { clip-path: polygon(100% 0, 0 0, 0 100%, 100% 100%, calc(100% - 18px) 50%); }
  .s7 .bsh { position: absolute; top: 343px; inset-inline-start: 110px; width: 1060px; height: 42px; background: linear-gradient(180deg, rgb(0 0 0 / .16), transparent); clip-path: polygon(1% 0, 99% 0, 100% 100%, 0 100%); }
  .s7 .it { position: absolute; top: 452px; height: 60px; }
  .s7 .it .lb { position: absolute; top: 0; inset-inline-start: 30px; height: 60px; width: 195px; padding-inline-start: 40px; line-height: 60px; font-size: 20px; font-weight: 700; color: var(--blue); clip-path: polygon(0 0, calc(100% - 20px) 0, 100% 50%, calc(100% - 20px) 100%, 0 100%); }
  html[dir="rtl"] .s7 .it .lb { clip-path: polygon(100% 0, 20px 0, 0 50%, 20px 100%, 100% 100%); }
  .s7 .it .no { position: absolute; top: 0; inset-inline-start: 0; width: 60px; height: 60px; border-radius: 50%; background: var(--blue); color: #fff; font-size: 24px; font-weight: 700; display: grid; place-items: center; }
  .s7 .it.on .no { background: var(--magenta); } .s7 .it.on .lb { color: #fff; }
  .s7 .it4 .lb { width: 274px; }
  .s7 .id { position: absolute; top: 540px; width: 225px; margin: 0; text-align: center; }

  /* ---- 8 التقنية ---- */
  .s8 .gband { position: absolute; top: 405px; inset-inline-start: 0; width: 890px; height: 315px; background: linear-gradient(180deg, #E3E3E3, #FFFFFF); }
  .s8 .panel { position: absolute; top: 0; inset-inline-start: 890px; width: 390px; height: 720px; }
  .s8 .card { position: absolute; top: 185px; width: 214px; height: 335px; background: #fff; border-radius: 30px; box-shadow: 0 8px 24px rgb(40 20 90 / .14); overflow: hidden; }
  .s8 .card .hd { position: absolute; top: 0; inset-inline-start: 28px; width: 158px; height: 75px; border-radius: 22px 22px 0 0; color: #fff; font-weight: 700; font-size: 20px; display: grid; place-items: center; }
  .s8 .card .ft { position: absolute; top: 320px; inset-inline-start: 28px; width: 158px; height: 40px; border-radius: 22px 22px 0 0; }
  .s8 .card .r { position: absolute; inset-inline-start: 20px; width: 190px; margin: 0; display: flex; align-items: center; gap: 10px; font-size: 15px; color: #222; }
  .s8 .card .r .ic { width: 36px; height: 36px; color: #B3B3CD; flex: none; }
  .s8 .cd { position: absolute; top: 552px; width: 214px; margin: 0; text-align: center; }
  .s8 .pi { position: absolute; inset-inline-start: 945px; width: 250px; color: #fff; }
  .s8 .pi h3 { margin: 0 0 8px; font-size: 20px; font-weight: 700; }
  .s8 .pi p { margin: 0; width: 180px; color: #fff; }

  /* ---- 9 المواهب ---- */
  .s9 .node { position: absolute; width: 330px; height: 93px; border-radius: 47px; color: #fff; background: var(--blue); }
  .s9 .node.top { background: var(--magenta); }
  .s9 .node .av { position: absolute; top: 0; inset-inline-start: 0; width: 94px; height: 94px; border-radius: 50%; background: var(--navy); color: #fff; }
  .s9 .node .av svg { width: 100%; height: 100%; display: block; }
  .s9 .node h3 { position: absolute; top: 20px; inset-inline-start: 110px; margin: 0; font-size: 20px; font-weight: 700; }
  .s9 .node p { position: absolute; top: 52px; inset-inline-start: 110px; margin: 0; font-size: 18px; color: #fff; }
  .s9 .box { position: absolute; top: 405px; width: 322px; height: 236px; border: 3px dashed var(--gray-2); border-radius: 40px; }
  .s9 .box p { position: absolute; inset-inline-start: 8px; width: 298px; height: 65px; margin: 0; border-radius: 33px; background: var(--gray); line-height: 65px; text-align: center; font-size: 18px; }

  /* ---- 10 خارطة الطريق ---- */
  .s10 .band { position: absolute; top: 380px; inset-inline-start: 0; width: 1280px; height: 82px; }
  .s10 .yr { position: absolute; top: 404px; margin: 0; color: #fff; font-size: 26px; font-weight: 700; }
  .s10 .hh { position: absolute; top: 230px; margin: 0; color: var(--blue); font-size: 20px; font-weight: 700; }
  .s10 .ht { position: absolute; top: 263px; width: 150px; margin: 0; font-size: 17px; }
  .s10 .sep { position: absolute; top: 208px; width: 1px; height: 142px; background: #D9D9E2; }
  .s10 .ms { position: absolute; top: 560px; display: flex; align-items: center; gap: 14px; }
  .s10 .ms .star { width: 32px; height: 32px; border-radius: 50%; background: #FFC000; color: #fff; }
  .s10 .ms .star svg { width: 100%; height: 100%; display: block; }
  .s10 .ms p { margin: 0; font-size: 18px; font-weight: 700; color: var(--muted); letter-spacing: .5px; }
  .s10 .dl { position: absolute; top: 462px; width: 0; height: 95px; border-inline-start: 1px dashed #B9B9C4; }

  /* ---- 11 المخاطر ---- */
  .s11 .panel { position: absolute; top: 318px; inset-inline-start: 0; width: 398px; height: 402px; border-start-end-radius: 60px; }
  .s11 .lead { top: 248px; inset-inline-start: 112px; width: 300px; font-size: 19px; }
  .s11 .pi { position: absolute; inset-inline-start: 112px; width: 240px; color: #fff; }
  .s11 .pi h3 { margin: 0 0 8px; font-size: 20px; font-weight: 700; }
  .s11 .pi p { margin: 0; width: 180px; color: #fff; }
  .s11 .ch { position: absolute; top: 138px; width: 220px; height: 40px; border-radius: 10px 10px 0 0; color: #fff; font-weight: 700; font-size: 18px; display: grid; place-items: center; }
  .s11 .col { position: absolute; top: 178px; width: 220px; height: 422px; background: linear-gradient(180deg, #F0F0F0, #FFFFFF); }
  .s11 .rh { position: absolute; inset-inline-start: 453px; width: 70px; color: #fff; font-weight: 700; font-size: 18px; display: grid; place-items: center; }
  .s11 .cell { position: absolute; margin: 0; background: #F3F3F3; box-shadow: 0 6px 14px rgb(0 0 0 / .12); display: grid; place-items: center; font-size: 18px; text-align: center; }
  .s11 .lbl { position: absolute; margin: 0; font-size: 18px; font-weight: 700; color: #111; }
  .s11 .prob { top: 388px; inset-inline-start: 388px; width: 90px; text-align: center; transform: rotate(-90deg); }
  html[dir="rtl"] .s11 .prob { transform: rotate(90deg); }

  /* ---- 12 النتائج ---- */
  .s12 .corner { position: absolute; top: 455px; inset-inline-start: 0; width: 378px; height: 265px; }
  .s12 .kpi { position: absolute; width: 173px; height: 175px; border: 1px solid #D2D2D2; border-radius: 10px; background: #fff; }
  .s12 .kpi h3 { position: absolute; top: 22px; inset-inline-start: 14px; margin: 0; font-size: 18px; font-weight: 400; color: #222; }
  .s12 .kpi .v { position: absolute; top: 52px; inset-inline-start: 14px; margin: 0; font-size: 36px; font-weight: 700; line-height: 1.2; }
  .s12 .kpi .v sup { font-size: 18px; vertical-align: top; position: relative; top: 4px; }
  .s12 .kpi .bar { position: absolute; top: 112px; inset-inline-start: 14px; width: 80px; height: 6px; border-radius: 3px; background: linear-gradient(90deg, var(--k) 0 62%, #fff 62% 68%, var(--k) 68% 78%, #fff 78% 84%, var(--k) 84%); }
  html[dir="rtl"] .s12 .kpi .bar { background: linear-gradient(270deg, var(--k) 0 62%, #fff 62% 68%, var(--k) 68% 78%, #fff 78% 84%, var(--k) 84%); }
  .s12 .kpi .d { position: absolute; top: 132px; inset-inline-start: 14px; margin: 0; display: flex; align-items: center; gap: 8px; font-size: 16px; color: #9A9AA3; }
  .s12 .kpi .d span { width: 20px; height: 20px; border-radius: 50%; border: 1.5px solid var(--green); color: var(--green); display: grid; place-items: center; font-size: 12px; }
  .s12 .kpi.pu { --k: var(--purple); } .s12 .kpi.pu .v { color: var(--purple); }
  .s12 .kpi.bl { --k: var(--blue); } .s12 .kpi.bl .v { color: var(--blue); }
  .s12 .ctx { top: 580px; inset-inline-start: 120px; width: 250px; color: #fff; }
  .s12 .banner { position: absolute; top: 185px; inset-inline-start: 505px; width: 710px; height: 137px; border-radius: 24px; color: #fff; }
  .s12 .banner .ic { position: absolute; top: 28px; inset-inline-start: 36px; width: 70px; height: 76px; color: #fff; }
  .s12 .banner h3 { position: absolute; top: 38px; inset-inline-start: 143px; margin: 0; font-size: 22px; font-weight: 700; }
  .s12 .banner p { position: absolute; top: 74px; inset-inline-start: 143px; margin: 0; font-size: 19px; color: #fff; }
  .s12 .chart { position: absolute; top: 345px; inset-inline-start: 505px; width: 710px; height: 300px; }
  .s12 .chart svg { width: 100%; height: 100%; }
  .s12 .pill { position: absolute; top: 572px; inset-inline-start: 555px; height: 40px; display: flex; border-radius: 20px; overflow: hidden; background: #F3CBF3; }
  .s12 .pill b { background: var(--purple); color: #fff; font-size: 22px; padding: 0 16px; line-height: 40px; border-radius: 20px; }
  .s12 .pill span { font-size: 15px; line-height: 40px; padding: 0 44px 0 16px; color: var(--purple); }
  html[dir="rtl"] .s12 .pill span { padding: 0 16px 0 44px; }

  /* ---- 13 الخطوات التالية ---- */
  .s13 .lead { top: 195px; inset-inline-start: 112px; width: 500px; font-size: 22px; line-height: 1.35; }
  .s13 .hx { position: absolute; width: 260px; height: 230px; }
  .s13 .hx svg { width: 100%; height: 100%; }
  .s13 .hx h3 { position: absolute; top: 85px; inset-inline-start: 30px; width: 200px; margin: 0; text-align: center; color: var(--navy); font-size: 22px; font-weight: 700; line-height: 1.3; }
  .s13 .hd { position: absolute; width: 240px; margin: 0; text-align: center; font-size: 17px; }

  /* ---- 14 شكرًا ---- */
  .s14 .photo { position: absolute; top: 0; inset-inline-start: 680px; width: 600px; height: 380px; object-fit: cover; clip-path: polygon(4% 0, 53% 0, 75% 79%, 100% 79%, 100% 100%, 17.7% 100%, 0 29.7%); }
  html[dir="rtl"] .s14 .photo { clip-path: polygon(96% 0, 47% 0, 25% 79%, 0 79%, 0 100%, 82.3% 100%, 100% 29.7%); }
  .s14 .ph2 { position: absolute; top: 380px; inset-inline-start: 855px; width: 63px; height: 185px; object-fit: cover; }
  .s14 .ph3 { position: absolute; top: 380px; inset-inline-start: 1145px; width: 135px; height: 185px; object-fit: cover; }
  .s14 .t1 { top: 106px; inset-inline-start: 112px; font-size: 50px; font-weight: 700; color: var(--purple); line-height: 1.2; }
  html[lang="ar"] .s14 .t1 { font-weight: 800; }
  .s14 .t2 { top: 178px; inset-inline-start: 112px; font-size: 42px; font-weight: 400; color: var(--blue); line-height: 1.2; }
  .s14 .lead { top: 270px; inset-inline-start: 112px; width: 560px; font-size: 20px; line-height: 1.45; }
  .s14 .logo { top: 88px; inset-inline-start: 1010px; }
  .s14 .card { position: absolute; top: 380px; width: 228px; height: 245px; background: #fff; border-radius: 30px; box-shadow: 0 10px 30px rgb(40 20 90 / .18); }
  .s14 .card .hex { position: absolute; top: -20px; inset-inline-start: -32px; width: 86px; height: 84px; color: #fff; }
  .s14 .card .hex svg { width: 100%; height: 100%; display: block; }
  .s14 .card .hex .ic { position: absolute; top: 22px; inset-inline-start: 23px; width: 40px; height: 40px; }
  .s14 .card h3 { position: absolute; top: 88px; inset-inline-start: 15px; margin: 0; font-size: 20px; font-weight: 700; color: #111; }
  .s14 .card .ul { position: absolute; top: 126px; inset-inline-start: 15px; width: 38px; height: 3px; background: var(--purple); }
  .s14 .card p { position: absolute; top: 148px; inset-inline-start: 15px; margin: 0; }"""

# ---- أيقونات SVG بسيطة ----
ICONS = {
    'aperture': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20zm0 2.2 3.6 6.2H8.4L12 4.2zm-5.2 3 3.6 6.3-3.6 6.2A7.9 7.9 0 0 1 4.1 12c0-1.8.6-3.4 1.6-4.8h1.1zm10.4 0h1.1a7.8 7.8 0 0 1 1.6 4.8c0 2.9-1.5 5.4-3.8 6.9l-3.5-6.2 3.6-6.3-.9 1.4zm-1.3 8.6 2 3.4A7.9 7.9 0 0 1 12 19.9a8 8 0 0 1-5.9-2.7h9.8z"/></svg>',
    'molecule': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" aria-hidden="true"><polygon points="12,4 19,8 19,16 12,20 5,16 5,8"/><circle cx="12" cy="4" r="2" fill="currentColor"/><circle cx="19" cy="8" r="2" fill="currentColor"/><circle cx="19" cy="16" r="2" fill="currentColor"/><circle cx="12" cy="20" r="2" fill="currentColor"/><circle cx="5" cy="16" r="2" fill="currentColor"/><circle cx="5" cy="8" r="2" fill="currentColor"/><circle cx="12" cy="12" r="2.2" fill="currentColor"/></svg>',
    'list': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><rect x="3" y="4" width="3" height="3"/><rect x="8" y="4" width="10" height="3"/><rect x="3" y="10.5" width="3" height="3"/><rect x="8" y="10.5" width="13" height="3"/><rect x="3" y="17" width="3" height="3"/><rect x="8" y="17" width="8" height="3"/><circle cx="21" cy="5.5" r="1.3"/></svg>',
    'qr': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M3 3h7v7H3zm2 2v3h3V5zm9-2h7v7h-7zm2 2v3h3V5zM3 14h7v7H3zm2 2v3h3v-3zm9-2h3v3h-3zm4 0h3v3h-3zm-4 4h3v3h-3zm4 0h3v3h-3z"/></svg>',
    'layers': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12 3 2 8l10 5 10-5-10-5zm-7.5 8.6L2 13l10 5 10-5-2.5-1.4L12 15.4l-7.5-3.8zM4.5 15.6 2 17l10 5 10-5-2.5-1.4L12 19.4l-7.5-3.8z"/></svg>',
    'cloud': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" aria-hidden="true"><path d="M7 18h10a4 4 0 0 0 .5-8A5.5 5.5 0 0 0 6.8 11 3.5 3.5 0 0 0 7 18z"/><path d="M12 21v-6m-2.5 2.5L12 15l2.5 2.5"/></svg>',
    'cubes': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12 2 8 4v4l4 2 4-2V4l-4-2zM6 9l-4 2v4l4 2 4-2v-4L6 9zm12 0-4 2v4l4 2 4-2v-4l-4-2zm-6 7-4 2v3l4 2 4-2v-3l-4-2z"/></svg>',
    'person': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><circle cx="12" cy="6" r="3.5"/><path d="M5 20v-3.5a4 4 0 0 1 4-4h6a4 4 0 0 1 4 4V20H5z"/><rect x="8" y="14" width="8" height="5" rx="1" fill="#fff" opacity=".6"/></svg>',
    'monitor': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M2 4h14v11H2zm1.5 1.5v8h11v-8zM6 17h6v1.5H6zm12-11h4v14h-4zm1 1.5v2h2v-2z"/></svg>',
    'server': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M3 3h18v5H3zm2 1.5v2h4v-2zM3 9.5h18v5H3zm2 1.5v2h4v-2zM3 16h18v5H3zm2 1.5v2h4v-2z"/></svg>',
    'webcam': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12 2a7 7 0 1 0 0 14 7 7 0 0 0 0-14zm0 3a4 4 0 1 1 0 8 4 4 0 0 1 0-8zm0 2a2 2 0 1 0 0 4 2 2 0 0 0 0-4zM9 17h6l2 4H7z"/></svg>',
    'user': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><circle cx="12" cy="8.5" r="4"/><path d="M4 21c0-4 3.6-6.5 8-6.5s8 2.5 8 6.5z"/></svg>',
    'star': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="m12 4 2.4 5 5.6.7-4.1 3.8 1 5.5-4.9-2.7L7.1 19l1-5.5L4 9.7l5.6-.7z"/></svg>',
    'bars': '<svg viewBox="0 0 70 76" fill="currentColor" aria-hidden="true"><rect x="0" y="54" width="12" height="22"/><rect x="19" y="36" width="12" height="40"/><rect x="38" y="18" width="12" height="58"/><rect x="57" y="0" width="12" height="76"/></svg>',
    'arrow': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M8 16 16 8M10 8h6v6"/></svg>',
    'mail': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linejoin="round" aria-hidden="true"><path d="M3 9l9-6 9 6v11H3z"/><path d="M3 9l9 6 9-6M7 11h10M7 14h7"/></svg>',
    'phone': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M6.5 3h3l1.5 4-2 1.5a12 12 0 0 0 6.5 6.5L17 13l4 1.5v3a2 2 0 0 1-2 2A16 16 0 0 1 4.5 5a2 2 0 0 1 2-2z"/><path d="M14 4a6 6 0 0 1 6 6M14 7.5a2.5 2.5 0 0 1 2.5 2.5" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/></svg>',
    'globe': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" aria-hidden="true"><circle cx="12" cy="12" r="9"/><path d="M3 12h18M12 3c3 3 3 15 0 18M12 3c-3 3-3 15 0 18M5 7.5h14M5 16.5h14"/></svg>',
}

GLOBAL_DEFS = ('<defs>'
    '<linearGradient id="gmb" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#A51EBA"/><stop offset="1" stop-color="#1414DD"/></linearGradient>'
    '<linearGradient id="gbm" x1="0" y1="1" x2="1" y2="0"><stop offset="0" stop-color="#1414DD"/><stop offset="1" stop-color="#A51EBA"/></linearGradient>'
    '<linearGradient id="gbmv" x1="0" y1="0" x2="0.4" y2="1"><stop offset="0" stop-color="#1414DD"/><stop offset="1" stop-color="#A51EBA"/></linearGradient>'
    '<linearGradient id="gmbh" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#A51EBA"/><stop offset="1" stop-color="#1414DD"/></linearGradient>'
    '<linearGradient id="gmp" x1="0" y1="0" x2="0.6" y2="1"><stop offset="0" stop-color="#A51EBA"/><stop offset="1" stop-color="#6A1B7A"/></linearGradient>'
    '<linearGradient id="gbn" x1="0" y1="0" x2="0.6" y2="1"><stop offset="0" stop-color="#1414DD"/><stop offset="1" stop-color="#0A0A80"/></linearGradient>'
    '<linearGradient id="glav" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#D5DCF8"/><stop offset="1" stop-color="#C6D2F6"/></linearGradient>'
    '<linearGradient id="gwave" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#7A108E"/><stop offset="1" stop-color="#1C1CE0"/></linearGradient>'
    '<linearGradient id="gblob" x1="0" y1="0" x2="0.3" y2="1"><stop offset="0" stop-color="#7A0E82"/><stop offset="1" stop-color="#3E0A9E"/></linearGradient>'
    '<linearGradient id="gfr" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#E4E6FF"/><stop offset="1" stop-color="#C2C6F6"/></linearGradient>'
    '<linearGradient id="gell" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#FFFFFF"/><stop offset="1" stop-color="#BDBDD0"/></linearGradient>'
    '<radialGradient id="ggem" cx="0.42" cy="0.52" r="0.6"><stop offset="0" stop-color="#F8F0FF" stop-opacity=".95"/><stop offset=".4" stop-color="#D2B4EC" stop-opacity=".55"/><stop offset="1" stop-color="#C9A6E6" stop-opacity=".22"/></radialGradient>'
    '<radialGradient id="gglow" cx="0.5" cy="0.5" r="0.5"><stop offset="0" stop-color="#FFFFFF" stop-opacity=".95"/><stop offset=".25" stop-color="#FFFFFF" stop-opacity=".55"/><stop offset="1" stop-color="#FFFFFF" stop-opacity="0"/></radialGradient>'
    '<filter id="soft" x="-30%" y="-30%" width="160%" height="160%"><feDropShadow dx="0" dy="0" stdDeviation="9" flood-color="#B4B8F0" flood-opacity=".75"/></filter>'
    '<filter id="soft2" x="-30%" y="-30%" width="160%" height="160%"><feDropShadow dx="0" dy="0" stdDeviation="12" flood-color="#C4C8F2" flood-opacity=".5"/></filter>'
    '<filter id="hexsh" x="-30%" y="-30%" width="160%" height="160%"><feDropShadow dx="0" dy="6" stdDeviation="10" flood-color="#8A90D8" flood-opacity=".45"/></filter>'
    '</defs>')


def deco(inner):
    return '<svg class="deco" viewBox="0 0 1280 720" aria-hidden="true">' + inner + '</svg>'


def glyph(points, fill):  # شكل سداسي مقوّس بزوايا مدوّرة (خط عريض بنفس اللون)
    return '<polygon class="gl" points="%s" fill="%s" stroke="%s"/>' % (points, fill, fill)


def ring(outer, inner, fill, w=28):  # شكل الشعار: محيط خارجي وفتحة داخلية بزوايا مدوّرة
    return '<path fill-rule="evenodd" d="M%s Z M%s Z" fill="%s" stroke="%s" stroke-width="%d" stroke-linejoin="round"/>' % (outer.replace(' ', ' L '), inner.replace(' ', ' L '), fill, fill, w)


def hexpts(cx, cy, r):  # سداسي مدبّب الرأس
    import math
    return ' '.join('%.0f,%.0f' % (cx + r * math.cos(math.radians(a)), cy + r * math.sin(math.radians(a))) for a in (270, 330, 30, 90, 150, 210))


def hf(icon):  # إطار لافندر مقوّس مع أيقونة
    return ('<div class="hf"><svg class="fr" viewBox="0 0 115 115" aria-hidden="true"><polyline class="flip" points="12,104 12,50 38,16 70,16 102,52 84,104" fill="none" stroke="url(#gfr)" stroke-width="26" stroke-linejoin="round"/></svg>'
            '<span class="ic">%s</span></div>' % ICONS[icon])


def logo():
    return ('<div class="logo"><svg class="mk" viewBox="0 0 60 60" fill="none" stroke="currentColor" stroke-width="7" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">'
            '<path d="M48 14 Q30 6 14 14 Q6 30 14 46 Q30 54 46 46 Q52 38 50 30"/></svg><p>§LOGO1§<br>§LOGO2§</p></div>')


# ---- الشرائح ----
S1 = ('\n    ' + deco(
    '<path fill="url(#gblob)" d="M560,720 C640,640 740,560 830,420 C900,300 990,185 1090,180 C1170,178 1240,262 1280,312 L1280,720 Z"/>'
    '<path fill="#3A0A8A" opacity=".5" d="M560,720 C720,640 930,600 1120,650 C1190,668 1245,690 1280,705 L1280,720 Z"/>'
    '<path fill="url(#glav)" d="M0,190 C110,60 250,70 400,170 C540,265 620,345 700,465 C760,555 730,650 650,720 L0,720 Z"/>'
    '<path fill="url(#gwave)" d="M0,490 L320,490 C410,490 450,545 490,612 C515,652 528,692 525,720 L0,720 Z"/>'
    '<g filter="url(#soft2)"><polygon fill="#fff" points="' + hexpts(640, 147, 85) + '"/><polygon fill="#fff" points="' + hexpts(820, 152, 55) + '"/></g>'
    '<polygon fill="#fff" opacity=".45" points="' + hexpts(770, 400, 150) + '"/>'
    '<polygon fill="url(#ggem)" points="' + hexpts(860, 418, 170) + '"/>'
    '<circle cx="832" cy="412" r="120" fill="url(#gglow)"/>')
    + '\n    ' + logo()
    + '\n    <h1 class="ab t">§T1§</h1>\n    <p class="ab st">§SUB1§</p>\n    <p class="ab bt">§S2§<br>§TST§</p>\n  ')

S2 = ('\n    <div class="panel"></div>\n    <h1 class="ttl">§T2§</h1>\n    <p class="ab lead">§S3§</p>'
      + ''.join('\n    <div class="card" style="top:%dpx;inset-inline-start:%dpx;height:%dpx">%s<h2>§HH§</h2><p class="p">§S§</p></div>'
                % (t, l, 720 - t, hf(i)) for t, l, i in ((340, 80, 'aperture'), (275, 388, 'molecule'), (185, 696, 'list')))
      + '\n    <div class="kv" style="top:135px"><p class="n">40%</p><div class="ln"></div><h3>§HH§</h3><p class="p">§S2§</p></div>'
      + '\n    <div class="kv" style="top:392px"><p class="n">$5.3M</p><div class="ln"></div><h3>§HH§</h3><p class="p">§S2§</p></div>\n  ')

S3 = ('\n    ' + deco('<path fill="url(#gmbh)" stroke="url(#gmbh)" stroke-width="12" stroke-linejoin="round" d="M6,320 Q6,236 90,236 H300 Q378,238 418,302 L574,560 L478,714 H6 Z"/>'
                     + ring('1140,455 1300,455 1300,740 975,740', '1185,600 1300,600 1300,740 1130,740', '#6A1B7A'))
      + '\n    <h1 class="ttl">§T3§</h1>\n    <p class="ab h cs">§CS§</p>\n    <p class="ab p ct">§S2§<br>§TST§</p>'
      + '\n    <p class="big" style="inset-inline-start:110px">40<sup>%</sup></p><div class="ul" style="inset-inline-start:110px"></div><p class="kh h" style="inset-inline-start:110px">§HH§</p><p class="kp" style="inset-inline-start:110px">§S2§</p>'
      + '\n    <p class="big" style="inset-inline-start:347px">300<sup>+</sup></p><div class="ul" style="inset-inline-start:347px"></div><p class="kh h" style="inset-inline-start:347px">§HH§</p><p class="kp" style="inset-inline-start:347px">§S2§</p>'
      + '\n    <p class="ab h sap">§SAP§</p>'
      + '\n    <div class="st st1">§HH§</div><p class="sd p" style="top:512px;inset-inline-start:590px">§S§</p>'
      + '\n    <div class="st st2">§HH§</div><p class="sd p" style="top:420px;inset-inline-start:796px">§S§</p>'
      + '\n    <div class="st st3">§HH§</div><p class="sd p" style="top:338px;inset-inline-start:1004px">§S§</p>'
      + '\n    <div class="ac flip" style="top:368px;inset-inline-start:760px">' + ICONS['arrow'] + '</div><div class="ac flip" style="top:286px;inset-inline-start:970px">' + ICONS['arrow'] + '</div>\n  ')

S4 = ('\n    <img class="photo" alt="§ALT4§" src="' + A + 'vision.jpg">\n    '
      + deco('<polygon class="gl" points="1100,490 1280,490 1280,720 900,720" fill="url(#gbmv)" stroke="url(#gbmv)"/>'
             '<polyline points="248,300 318,218 365,218 462,318 410,415" fill="none" stroke="url(#gmp)" stroke-width="62" stroke-linejoin="round"/>'
             '<polyline points="745,335 672,248 615,248 528,335 575,445" fill="none" stroke="url(#gbn)" stroke-width="62" stroke-linejoin="round"/>')
      + '\n    <h1 class="ttl">§T4§</h1>'
      + '\n    <p class="vm v">§OUR§ <b>§VIS§</b></p><p class="vt v">§S2§</p>'
      + '\n    <p class="vm m">§OUR§ <b>§MIS§</b></p><p class="vt m">§S2§</p>'
      + ''.join('\n    <div class="row" style="top:%dpx;inset-inline-start:%dpx">§S2§</div>' % (t, l) for l in (103, 628) for t in (445, 512, 580)) + '\n  ')

S5 = ('\n    <div class="band stripes"></div>\n    '
      + deco(ring('1150,-20 1300,-20 1300,335 1135,335 985,235', '1180,145 1300,145 1300,325 1180,325 1135,235', '#6A1B7A')
             + '<polygon fill="#fff" opacity=".8" filter="url(#soft)" points="' + hexpts(1005, 236, 32) + '"/>'
             + ''.join('<g filter="url(#soft)"><polygon fill="#fff" opacity=".9" points="%s"/><polygon fill="#fff" opacity=".7" points="%s"/></g>' % (hexpts(x + 92, 282, 90), hexpts(x + 185, 262, 52)) for x in (105, 462, 817)))
      + '\n    <h1 class="ttl">§T5§</h1>'
      + ''.join('\n    <div class="card" style="inset-inline-start:%dpx"><p class="cp">§S§</p><ul><li>§ITH§</li><li>§TST§</li></ul></div><p class="hp" style="inset-inline-start:%dpx">§HP§</p>' % (x, x - 58) for x in (168, 525, 880)) + '\n  ')

S6 = ('\n    <div class="panel g-mp"></div>\n    <div class="q" style="inset-inline-start:153px"></div><div class="q" style="inset-inline-start:430px"></div>\n    '
      + deco('<path d="M143,604 V218 M135,230 L143,214 L151,230 M143,608 H704 M692,600 L708,608 L692,616" fill="none" stroke="#9A80C8" stroke-width="1.6"/>'
             + ring('-20,565 75,565 152,665 120,740 -20,740', '-20,640 48,640 88,692 76,740 -20,740', 'url(#gbmv)'))
      + '\n    <h1 class="ttl">§T6§</h1>'
      + ''.join('\n    <p class="bub" style="top:%dpx;inset-inline-start:%dpx;width:%dpx;height:%dpx;background:%s;color:%s">§IT§</p>' % (cy - r, cx - r, 2 * r, 2 * r, bg, fg)
                for cx, cy, r, bg, fg in ((255, 297, 87, 'var(--blue)', '#fff'), (430, 368, 75, 'var(--purple)', '#fff'), (577, 285, 53, '#CDEBFA', 'var(--navy)'), (309, 513, 62, 'var(--magenta)', '#fff'), (540, 497, 62, 'var(--navy)', '#fff')))
      + '\n    <p class="ax" style="top:405px;inset-inline-start:62px;width:140px;text-align:center;transform:rotate(-90deg)" data-rot>§BV§</p><p class="ax" style="top:624px;inset-inline-start:290px;width:260px;text-align:center">§FE§</p>'
      + '\n    <p class="ab p wt" style="top:236px;inset-inline-start:828px;width:380px;font-size:22px;line-height:1.35">§S2§<br>§TST§</p>'
      + '\n    <p class="num" style="top:345px;inset-inline-start:835px">1</p><p class="ph" style="top:352px;inset-inline-start:884px">§HH§</p><p class="pp p" style="top:390px;inset-inline-start:884px">§S2§<br>§TST§</p>'
      + '\n    <p class="num" style="top:505px;inset-inline-start:835px">2</p><p class="ph" style="top:512px;inset-inline-start:884px">§HH§</p><p class="pp p" style="top:550px;inset-inline-start:884px">§S2§<br>§TST§</p>\n  ')

S7 = ('\n    ' + deco(ring('-20,565 75,565 152,665 120,740 -20,740', '-20,640 48,640 88,692 76,740 -20,740', 'url(#gbmv)'))
      + '\n    <h1 class="ttl wr" style="width:460px">§T7§</h1>\n    <div class="callout"><p>§S3§</p></div>'
      + '\n    <p class="hp" style="top:245px">§HP§</p><div class="bsh"></div>'
      + '\n    <div class="seg first g-mp wt" style="inset-inline-start:110px">§IT§</div>'
      + ''.join('\n    <div class="seg g-gray" style="inset-inline-start:%dpx">§IT§</div>' % (110 + 203 * k) for k in (1, 2, 3))
      + '\n    <div class="seg last g-mbh wt" style="inset-inline-start:922px">§IT§</div>'
      + '\n    <p class="hp" style="top:400px">§HP§</p>'
      + ''.join('\n    <div class="it it%d%s" style="inset-inline-start:%dpx"><div class="lb %s">§HH§</div><div class="no">%d</div></div>' % (k + 1, ' on' if k == 2 else '', x, 'g-mbh' if k == 2 else 'g-gray', k + 1) for k, x in enumerate((110, 360, 610, 866)))
      + ''.join('\n    <p class="id p" style="inset-inline-start:%dpx">§S2§</p>' % x for x in (110, 360, 610))
      + '\n    <p class="id p" style="inset-inline-start:866px;width:300px">§S2§<br>§TST§</p>\n  ')


def s8card(x, hd, icons):
    rows = ''.join('<p class="r" style="top:%dpx"><span class="ic">%s</span>§IT§</p>' % (t, ICONS[i]) for t, i in zip((97, 167, 240), icons))
    return '\n    <div class="card" style="inset-inline-start:%dpx"><div class="hd %s">§HH§</div>%s<div class="ft %s"></div></div><p class="cd p" style="inset-inline-start:%dpx">§S2§</p>' % (x, hd, rows, hd, x)


S8 = ('\n    <div class="gband"></div><div class="panel g-mb stripes"></div>\n    '
      + deco(ring('-20,565 75,565 152,665 120,740 -20,740', '-20,640 48,640 88,692 76,740 -20,740', 'url(#gbmv)'))
      + '\n    <h1 class="ttl">§T8§</h1>'
      + s8card(110, 'g-mp', ('qr', 'aperture', 'layers')) + s8card(358, 'g-bmn', ('cloud', 'cubes', 'person')) + s8card(608, 'g-mb', ('monitor', 'server', 'webcam'))
      + ''.join('\n    <div class="pi" style="top:%dpx"><h3>§HH§</h3><p class="p">§S2§</p></div>' % t for t in (188, 340, 500)) + '\n  ')


def node(x, y, top=False):
    return '\n    <div class="node%s" style="top:%dpx;inset-inline-start:%dpx"><div class="av">%s</div><h3>§NAME§</h3><p>§ROLE§</p></div>' % (' top' if top else '', y, x, ICONS['user'])


S9 = ('\n    ' + deco('<path d="M658,210 V245 M278,245 H1047 M278,245 V260 M658,245 V260 M1047,245 V260 M278,353 V405 M658,353 V405 M1047,353 V405" fill="none" stroke="#C9C9D6" stroke-width="2"/>')
      + '\n    <h1 class="ttl wr" style="width:340px">§T9§</h1>' + node(493, 117, True) + node(110, 260) + node(493, 260) + node(882, 260)
      + ''.join('\n    <div class="box" style="inset-inline-start:%dpx"><p style="top:9px">§IDT§</p><p style="top:84px">§IDT§</p><p style="top:159px">§IDT§</p></div>' % x for x in (110, 493, 882)) + '\n  ')

S10 = ('\n    <div class="band g-bmh"></div>\n    '
       + deco(ring('-20,565 75,565 152,665 120,740 -20,740', '-20,640 48,640 88,692 76,740 -20,740', 'url(#gbmv)')
              + ''.join('<ellipse cx="%d" cy="428" rx="40" ry="76" fill="url(#gell)"/><ellipse cx="%d" cy="428" rx="45" ry="85" fill="none" stroke="#A51EBA" stroke-width="5" stroke-dasharray="150 32 180 32" stroke-dashoffset="20"/>' % (x, x) for x in (255, 467, 710, 943, 1178)))
       + '\n    <h1 class="ttl">§T10§</h1>'
       + ''.join('\n    <p class="yr" style="inset-inline-start:%dpx">§Y%d§</p>' % (x, i + 1) for i, x in enumerate((112, 323, 565, 800, 1035)))
       + ''.join('\n    <p class="hh" style="inset-inline-start:%dpx">§HH§</p><p class="ht p" style="inset-inline-start:%dpx">§S2§</p>' % (x, x) for x in (112, 323, 565, 800, 1035))
       + ''.join('\n    <div class="sep" style="inset-inline-start:%dpx"></div>' % x for x in (292, 505, 747, 980, 1213))
       + ''.join('\n    <div class="dl" style="inset-inline-start:%dpx"></div><div class="ms" style="inset-inline-start:%dpx"><span class="star">%s</span><p>§MS%d§</p></div>' % (x, x - 16, ICONS['star'], i + 1) for i, x in enumerate((338, 640, 1052))) + '\n  ')


def cell(x, y, w, h):
    return '\n    <p class="cell" style="top:%dpx;inset-inline-start:%dpx;width:%dpx;height:%dpx">§IT§</p>' % (y, x, w, h)


S11 = ('\n    <div class="panel g-mb2 stripes"></div>\n    '
       + deco('<path d="M522,100 H800 M925,100 H1203 M432,175 V628" fill="none" stroke="#6A1B7A" stroke-width="1.5"/><circle cx="522" cy="100" r="4" fill="#6A1B7A"/><circle cx="1203" cy="100" r="4" fill="#6A1B7A"/><circle cx="432" cy="175" r="4" fill="#1414DD"/><circle cx="432" cy="628" r="4" fill="#1414DD"/>')
       + '\n    <h1 class="ttl wr" style="width:300px">§T11§</h1>\n    <p class="ab p lead">§S2§<br>§TST§</p>'
       + ''.join('\n    <div class="pi" style="top:%dpx"><h3>§HH§</h3><p class="p">§S2§</p></div>' % t for t in (388, 543))
       + '\n    <p class="lbl" style="top:88px;inset-inline-start:800px;width:125px;text-align:center">§IMP§</p><p class="lbl prob">§PROB§</p>'
       + ''.join('\n    <div class="ch" style="inset-inline-start:%dpx;background:%s">§%s§</div><div class="col" style="inset-inline-start:%dpx"></div>' % (x, bg, k, x) for x, bg, k in ((523, 'var(--magenta)', 'LOW'), (753, 'var(--purple)', 'MEDIUM'), (983, '#4A0F60', 'HIGH')))
       + ''.join('\n    <div class="rh" style="top:%dpx;height:%dpx;background:%s">§%s§</div>' % (t, h, bg, k) for t, h, bg, k in ((175, 143, 'var(--blue)', 'LOW'), (328, 145, 'var(--navy)', 'MED'), (483, 145, 'var(--dark)', 'HIGH')))
       + cell(552, 190, 156, 110) + cell(783, 190, 162, 42) + cell(783, 257, 162, 42) + cell(1012, 190, 162, 42) + cell(1012, 257, 162, 42)
       + cell(783, 352, 162, 108) + cell(1012, 352, 162, 108)
       + cell(552, 503, 156, 110) + cell(783, 507, 162, 42) + cell(783, 573, 162, 42) + '\n  ')


def kpi(x, y, cls, val):
    return '\n    <div class="kpi %s" style="top:%dpx;inset-inline-start:%dpx"><h3>§HH§</h3><p class="v">%s</p><div class="bar"></div><p class="d"><span>↑</span>3.4%%</p></div>' % (cls, y, x, val)


S12 = ('\n    <div class="corner g-mb2 stripes"></div>\n    '
       + deco(ring('1165,-30 1300,-30 1300,155 1205,155 1125,45', '1225,-30 1300,-30 1300,80 1225,80', 'url(#gbmv)'))
       + '\n    <h1 class="ttl">§T12§</h1>'
       + kpi(112, 185, 'pu', '<sup>$</sup>43.5M') + kpi(310, 185, 'bl', '76<sup>%</sup>') + kpi(112, 378, 'pu', '300<sup>+</sup>') + kpi(310, 378, 'bl', '45<sup>+</sup>')
       + '\n    <p class="ab p ctx">§S2§<br>§TST§</p>'
       + '\n    <div class="banner g-mb"><span class="ic">' + ICONS['bars'] + '</span><h3>§HH§</h3><p>§S§</p></div>'
       + '\n    <div class="chart" id="roi-chart"></div>\n    <div class="pill"><b>4.5%</b><span>§IT§</span></div>\n  ')

S13 = ('\n    ' + deco(glyph('0,335 200,335 325,550 220,720 0,720', 'url(#gbmv)') + glyph('330,635 445,635 500,720 265,720', '#0A0A80')
                      + ring('1130,435 1300,435 1300,740 1040,740 1005,650', '1200,565 1300,565 1300,740 1200,740 1150,650', 'url(#gbmv)'))
       + '\n    <h1 class="ttl">§T13§</h1>\n    <p class="ab lead">§S3§</p>'
       + ''.join('\n    <div class="hx" style="top:%dpx;inset-inline-start:%dpx"><svg viewBox="0 0 260 230" aria-hidden="true"><polygon points="%s" fill="#fff" filter="url(#hexsh)"/></svg><h3>§HP§</h3></div><p class="hd p" style="top:%dpx;inset-inline-start:%dpx">§S§</p>'
                 % (cy - 115, cx - 130, hexpts(130, 115, 112), cy + 150, cx - 120) for cx, cy in ((465, 422), (762, 315), (1057, 200))) + '\n  ')


def ccard(x, icon, key):
    return ('\n    <div class="card" style="inset-inline-start:%dpx"><div class="hex"><svg viewBox="0 0 86 84" aria-hidden="true"><polygon points="43,2 80,22 80,62 43,82 6,62 6,22" fill="url(#gmb)"/></svg><span class="ic">%s</span></div>'
            '<h3>§%s§</h3><div class="ul"></div><p class="p">§IYT§</p></div>' % (x, ICONS[icon], key))


S14 = ('\n    <img class="photo" alt="§ALT14§" src="' + A + 'thanks.jpg"><img class="ph2" alt="" src="' + A + 'thanks-2.jpg"><img class="ph3" alt="" src="' + A + 'thanks-3.jpg">\n    '
       + deco(ring('-20,375 240,375 430,740 -20,740', '-20,540 185,540 250,655 230,740 -20,740', 'url(#gbmv)', 20))
       + '\n    ' + logo() + '\n    <h1 class="ab t1">§T14§</h1>\n    <p class="ab t2">§CWU§</p>\n    <p class="ab lead">§S3§</p>'
       + ccard(345, 'mail', 'EMAIL') + ccard(628, 'phone', 'CONTACT') + ccard(918, 'globe', 'WEB') + '\n  ')

SLIDES = [S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, S14]

# الشريحة 8: ترويسة البطاقة الثانية بتدرّج أزرق→كحلي
STYLE += """
  .g-bmn { background: linear-gradient(135deg, var(--blue), var(--navy)); }
  html[dir="rtl"] .g-bmn { background: linear-gradient(225deg, var(--blue), var(--navy)); }
  html[dir="rtl"] .s6 [data-rot] { transform: rotate(90deg); }"""

JSON_BLOCKS = {'chart-data': {
    'ar': {'max': 6, 'labels': ['السنة 1', 'السنة 2', 'السنة 3', 'السنة 4'], 'series': [{'color': '#6A1B7A', 'values': [4.3, 2.5, 3.5, 4.5]}, {'color': '#A51EBA', 'values': [2.4, 4.4, 1.8, 2.8]}, {'color': '#1414DD', 'values': [2, 2, 3, 5]}]},
    'en': {'max': 6, 'labels': ['YEAR 1', 'YEAR 2', 'YEAR 3', 'YEAR 4'], 'series': [{'color': '#6A1B7A', 'values': [4.3, 2.5, 3.5, 4.5]}, {'color': '#A51EBA', 'values': [2.4, 4.4, 1.8, 2.8]}, {'color': '#1414DD', 'values': [2, 2, 3, 5]}]},
}}

EXTRA_SCRIPT = """  // رسم خطي (الشريحة 12) من كتلة chart-data؛ الفئة الأولى في جهة البداية
  function drawChart() {
    var el = document.getElementById('roi-chart'); if (!el) return;
    var d = JSON.parse(document.getElementById('chart-data').textContent);
    var W = 710, H = 300, L = RTL ? 20 : 40, R = RTL ? 40 : 20, T = 14, B = 34, pw = W - L - R, ph = H - T - B, max = d.max || 6, n = d.labels.length;
    var s = '';
    for (var v = 0; v <= max; v++) {
      var y = T + ph - v / max * ph;
      s += '<line x1="' + L + '" x2="' + (W - R) + '" y1="' + y + '" y2="' + y + '" stroke="#DCDCE2"/>';
      s += '<text x="' + (RTL ? W - R + 12 : L - 12) + '" y="' + (y + 4) + '" text-anchor="' + (RTL ? 'start' : 'end') + '" font-size="12" fill="#555">' + v + '</text>';
    }
    var xs = [];
    for (var i = 0; i < n; i++) { var k = RTL ? n - 1 - i : i; xs.push(L + pw * (k + 0.5) / n); }
    d.labels.forEach(function (lb, i) { s += '<text x="' + xs[i] + '" y="' + (H - 8) + '" text-anchor="middle" font-size="12" fill="#555">' + lb + '</text>'; });
    d.series.forEach(function (sr) {
      var pts = sr.values.map(function (v, i) { return [xs[i], T + ph - v / max * ph]; });
      s += '<polyline points="' + pts.map(function (p) { return p[0] + ',' + p[1]; }).join(' ') + '" fill="none" stroke="' + sr.color + '" stroke-width="4" stroke-linejoin="round"/>';
      pts.forEach(function (p) { s += '<circle cx="' + p[0] + '" cy="' + p[1] + '" r="9" fill="' + sr.color + '"/>'; });
    });
    el.innerHTML = '<svg viewBox="0 0 ' + W + ' ' + H + '" aria-hidden="true">' + s + '</svg>';
  }
  drawChart();"""

STR = {
    'ar': {
        'T1': 'الخطة الاستراتيجية للذكاء الاصطناعي', 'SUB1': 'قالب عرض تقديمي', 'LOGO1': 'شعار شركتك', 'LOGO2': 'هنا',
        'T2': 'الملخص التنفيذي', 'T3': 'الوضع الراهن ودراسة الجدوى', 'T4': 'رؤية الذكاء الاصطناعي ورسالته', 'T5': 'ركائز استراتيجيتنا للذكاء الاصطناعي',
        'T6': 'أبرز حالات الاستخدام والمبادرات', 'T7': 'استراتيجية البيانات وحوكمتها', 'T8': 'التقنية والبنية التحتية', 'T9': 'المواهب والتنظيم',
        'T10': 'خارطة طريق التنفيذ والجدول الزمني', 'T11': 'المخاطر والتخفيف منها', 'T12': 'النتائج المتوقعة والعائد على الاستثمار', 'T13': 'الخطوات التالية',
        'T14': 'شكرًا لكم', 'CWU': 'تواصلوا معنا',
        'S': 'أضف النص المطلوب هنا. هذا نص توضيحي.', 'S2': 'أضف النص المطلوب هنا.', 'S3': 'أضف النص المطلوب هنا. هذا نص توضيحي. عبارة موجزة عن موضوعك.',
        'TST': 'هذا نص توضيحي.', 'HH': 'عنوان هنا', 'HP': 'عنوان نائب', 'IT': 'أضف النص هنا', 'ITH': 'أضف النص هنا.', 'IDT': 'أضف النص المطلوب هنا', 'IYT': 'أضف نصك هنا',
        'NAME': 'الاسم هنا', 'ROLE': 'وصف الدور', 'CS': 'الوضع الراهن', 'SAP': 'عملية مبسّطة ومؤتمتة', 'BV': 'القيمة التجارية', 'FE': 'الجدوى / الجهد',
        'OUR': '', 'VIS': 'رؤيتنا', 'MIS': 'رسالتنا', 'IMP': 'الأثر', 'PROB': 'الاحتمالية', 'LOW': 'منخفض', 'MED': 'متوسط', 'MEDIUM': 'متوسط', 'HIGH': 'مرتفع',
        'MS1': 'المَعلَم 1', 'MS2': 'المَعلَم 2', 'MS3': 'المَعلَم 3', 'Y1': '2031', 'Y2': '2032', 'Y3': '2033', 'Y4': '2034', 'Y5': '2035',
        'EMAIL': 'البريد الإلكتروني', 'CONTACT': 'التواصل', 'WEB': 'الموقع الإلكتروني',
        'ALT4': 'تجريد سائل بألوان بنفسجية وزرقاء', 'ALT14': 'خطوط ضوئية بنفسجية متموجة على خلفية داكنة',
    },
    'en': {
        'T1': 'AI Strategic Plan', 'SUB1': 'PRESENTATION TEMPLATE', 'LOGO1': 'Your Company', 'LOGO2': 'Logo Here',
        'T2': 'Executive Summary', 'T3': 'The Current State &amp; Business Case', 'T4': 'AI Vision &amp; Mission', 'T5': 'Foundation Pillars of our AI Strategy',
        'T6': 'Key AI Use Cases &amp; Initiatives', 'T7': 'Data Strategy &amp; Governance', 'T8': 'Technology &amp; Infrastructure', 'T9': 'Talent &amp; Organization',
        'T10': 'Implementation Roadmap &amp; Timeline', 'T11': 'Risks &amp; Mitigation', 'T12': 'Expected Outcome &amp; ROI', 'T13': 'Next Steps',
        'T14': 'Thank You', 'CWU': 'Connect With Us',
        'S': 'Insert your desired text here. This is a sample text.', 'S2': 'Insert your desired text here.', 'S3': 'Insert your desired text here. This is a sample text. A brief statement about your topic.',
        'TST': 'This is a sample text.', 'HH': 'Heading Here', 'HP': 'Heading Placeholder', 'IT': 'Insert text here', 'ITH': 'Insert text here.', 'IDT': 'Insert desired text here', 'IYT': 'Insert your text here',
        'NAME': 'Name Here', 'ROLE': 'Role Description', 'CS': 'The Current State', 'SAP': 'Simplified and Automated Process', 'BV': 'Business Value', 'FE': 'Feasibility / Effort',
        'OUR': 'OUR', 'VIS': 'VISION', 'MIS': 'MISSION', 'IMP': 'Impact', 'PROB': 'Probability', 'LOW': 'Low', 'MED': 'Med', 'MEDIUM': 'Medium', 'HIGH': 'High',
        'MS1': 'MILESTONE 1', 'MS2': 'MILESTONE 2', 'MS3': 'MILESTONE 3', 'Y1': '2031', 'Y2': '2032', 'Y3': '2033', 'Y4': '2034', 'Y5': '2035',
        'EMAIL': 'Email', 'CONTACT': 'Contact', 'WEB': 'Website',
        'ALT4': 'Fluid abstract in purple and blue tones', 'ALT14': 'Glowing purple wavy lines on a dark background',
    },
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'mgmt', 'icon': '🧠',
    'name': 'الخطة الاستراتيجية للذكاء الاصطناعي', 'nameEn': 'AI Strategic Plan',
    'description': '14 شريحة لخطة ذكاء اصطناعي: غلاف، ملخص، الوضع الراهن، الرؤية، الركائز، البيانات، التقنية، خارطة طريق، مخاطر، عائد',
    'descriptionEn': '14 slides for an AI plan: cover, summary, current state, vision, pillars, data, tech, roadmap, risks, ROI',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'الملخص التنفيذي', 'titleEn': 'Executive Summary', 'needsData': True},
        {'title': 'الوضع الراهن ودراسة الجدوى', 'titleEn': 'Current State & Business Case', 'needsData': True},
        {'title': 'الرؤية والرسالة', 'titleEn': 'AI Vision & Mission'},
        {'title': 'ركائز الاستراتيجية', 'titleEn': 'Foundation Pillars'},
        {'title': 'حالات الاستخدام والمبادرات', 'titleEn': 'Key AI Use Cases & Initiatives'},
        {'title': 'استراتيجية البيانات وحوكمتها', 'titleEn': 'Data Strategy & Governance'},
        {'title': 'التقنية والبنية التحتية', 'titleEn': 'Technology & Infrastructure'},
        {'title': 'المواهب والتنظيم', 'titleEn': 'Talent & Organization'},
        {'title': 'خارطة طريق التنفيذ', 'titleEn': 'Implementation Roadmap & Timeline', 'needsData': True},
        {'title': 'المخاطر والتخفيف منها', 'titleEn': 'Risks & Mitigation'},
        {'title': 'النتائج المتوقعة والعائد (رسم)', 'titleEn': 'Expected Outcome & ROI (chart)', 'needsData': True},
        {'title': 'الخطوات التالية', 'titleEn': 'Next Steps'},
        {'title': 'شكرًا وتواصلوا معنا', 'titleEn': 'Thank You'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص والأرقام فقط؛ أرقام الرسم الخطي في شريحة النتائج في كتلة JSON بالمعرّف chart-data (labels وseries).',
}
