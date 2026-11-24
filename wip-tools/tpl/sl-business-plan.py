# -*- coding: utf-8 -*-
# نموذج «خطة العمل»: 42 شريحة (غلافان، فهرس، تواصل، فريق، تعريفات، ملخص، ثم شرائح داكنة على خلفية ناطحات سحاب
# للتنظيم والخدمات والسوق والاستراتيجية والمالية والمخاطر، وختام). الأنماط المشتركة تُكتب مرة واحدة.
KEY = 'sl-business-plan'
A = '/templates/freeform/sl-business-plan/assets/'
TITLE = {'ar': 'خطة العمل', 'en': 'Business Plan'}
COMMENT = 'نموذج «خطة العمل»: أزرق #1D80F7 وبرتقالي #F98226 ورمادي داكن؛ معظم الشرائح على خلفية ناطحات سحاب داكنة (bg-city.jpg) بعنوان في المنتصف، وتذييل بأيقونات التواصل ورقم الشريحة.'
VARS = '--ink: #FFFFFF; --accent: #1D80F7; --bg: #262626; --blue: #1D80F7; --blue-d: #0A5FC8; --navy: #123A6B; --orange: #F98226; --slate: #465866; --gray: #404040; --muted: #BFC3C8;'
FONTS = {'ar': '<link href="https://fonts.googleapis.com/css2?family=Tajawal:wght@300;400;500;700;800&display=swap" rel="stylesheet">',
         'en': '<link href="https://fonts.googleapis.com/css2?family=Arimo:ital,wght@0,400;0,700;1,400&family=Oswald:wght@700&display=swap" rel="stylesheet">'}
FONT_STACK = {'ar': "'Tajawal', 'Segoe UI', Tahoma, sans-serif", 'en': "'Arimo', Arial, Helvetica, sans-serif"}

# ---------------------------------------------------------------- الأنماط المشتركة
STYLE = """  :where(.frame p, .frame h1, .frame h2, .frame h3) { margin: 0; font-weight: 400; }
  .ab { position: absolute; }
  .bg { position: absolute; inset: 0; width: 100%; height: 100%; object-fit: cover; }
  .bluebg { background: linear-gradient(160deg, #1189F8 0%, #0A78E8 55%, #0064D0 100%); }
  .t { position: absolute; top: 38px; inset-inline-start: 0; width: 100%; text-align: center; font-size: 44px; font-weight: 400; line-height: 1.25; color: #fff; }
  html[lang="ar"] .t { font-weight: 500; }
  .sm { font-size: 13px; line-height: 1.25; color: #fff; }
  .flipx { }
  html[dir="rtl"] .flipx { transform: scaleX(-1); }
  /* التذييل: أيقونات التواصل ورقم الشريحة */
  .ft { position: absolute; top: 666px; inset-inline-start: 62px; display: flex; gap: 14px; align-items: center; color: rgb(255 255 255 / .38); }
  .ft svg { height: 22px; width: auto; }
  .ft .tw { width: 25px; } .ft .fb { width: 11px; } .ft .li { width: 23px; }
  .pg { position: absolute; top: 656px; inset-inline-end: 0; width: 40px; height: 38px; background: rgb(255 255 255 / .45); color: #fff; font-size: 14px; display: grid; place-items: center; }
  .light .ft { color: #B9B9B9; } .light .pg { background: #B5B5B5; }
  /* شعار الغلاف */
  .logo { position: absolute; top: 162px; inset-inline-start: 514px; width: 246px; height: 246px; }
  .cov1, .cov2 { position: absolute; inset-inline-start: 0; width: 100%; text-align: center; }
  .cov1 { top: 494px; font-size: 59px; font-weight: 700; color: #fff; line-height: 1.2; letter-spacing: .5px; }
  .cov2 { top: 568px; font-size: 41px; font-weight: 400; line-height: 1.2; }
  html[lang="ar"] .cov1 { font-weight: 800; }
  .s1 .cov2 { color: var(--blue); } .s2 .cov2 { color: #fff; }
  .s2 { background: #1F80F7; }
  .s2 .waves { position: absolute; inset: 0; width: 1280px; height: 720px; }
  .s2 .logo .ring { filter: drop-shadow(0 2px 3px rgb(0 0 0 / .25)); }
  /* 3: الفهرس */
  .toc { position: absolute; display: flex; align-items: center; gap: 0; }
  .toc b { width: 93px; flex: none; font-size: 58px; font-weight: 700; line-height: 1; color: #fff; text-shadow: 2px 3px 4px rgb(0 0 0 / .35); }
  .toc p { font-size: 20px; line-height: 23px; color: #fff; width: 300px; }
  /* 4: تواصل */
  .s4 .map { position: absolute; top: 0; inset-inline-start: 0; width: 900px; height: 720px; object-fit: cover; }
  .s4 .pb { position: absolute; top: 0; inset-inline-start: 900px; width: 380px; height: 358px; background: #1F80F7; }
  .s4 .pd { position: absolute; top: 358px; inset-inline-start: 900px; width: 380px; height: 362px; background: var(--gray); }
  .s4 .h { position: absolute; top: 42px; inset-inline-start: 947px; font-size: 49px; line-height: 1.2; color: #fff; }
  .s4 .h2 { position: absolute; top: 122px; inset-inline-start: 947px; font-size: 32px; line-height: 1.2; color: #fff; }
  .s4 .bd { position: absolute; top: 175px; inset-inline-start: 947px; font-size: 19px; line-height: 23px; color: #fff; width: 300px; }
  .s4 .so { position: absolute; inset-inline-start: 941px; display: flex; align-items: center; gap: 18px; color: #fff; font-size: 19px; line-height: 20px; }
  .s4 .so i { width: 40px; height: 40px; border-radius: 50%; background: #fff; color: var(--gray); display: grid; place-items: center; box-shadow: 1px 3px 4px rgb(0 0 0 / .45); }
  .s4 .so i svg { width: 24px; height: 24px; }
  /* 5: ضبط الوثيقة */
  .dc { position: absolute; inset-inline-start: 268px; width: 756px; text-align: center; font-size: 23.5px; line-height: 28px; color: #fff; text-transform: uppercase; text-shadow: 0 2px 6px rgb(0 0 0 / .35); }
  .dl { position: absolute; inset-inline-start: 270px; width: 751px; height: 1px; background: rgb(255 255 255 / .35); }
  .dl::before, .dl::after { content: ''; position: absolute; top: -3px; width: 7px; height: 7px; border-radius: 50%; background: rgb(255 255 255 / .35); }
  .dl::before { inset-inline-start: -3px; } .dl::after { inset-inline-end: -3px; }
  /* 6: الفريق */
  .s6 { background: #F8F8F8; }
  .s6 .top { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 312px; background: linear-gradient(180deg, #0F86F6, #0062CE); }
  .tm { position: absolute; top: 202px; width: 208px; }
  .tm img { width: 208px; height: 208px; border-radius: 50%; border: 6px solid #fff; box-shadow: 0 3px 6px rgb(0 0 0 / .35); object-fit: cover; background: #fff; }
  .tm h2 { margin-top: 26px; margin-inline-start: 5px; font-size: 27px; font-weight: 400; line-height: 1.2; color: #444; }
  .tm p { margin-top: 12px; margin-inline-start: 5px; font-size: 19px; line-height: 23px; color: #444; width: 200px; }
  /* 7: التعريفات */
  .s7 .ph { position: absolute; top: 0; inset-inline-start: 0; width: 664px; height: 720px; object-fit: cover; }
  .s7 .pn { position: absolute; top: 0; inset-inline-start: 664px; width: 616px; height: 720px; background: linear-gradient(160deg, #1189F8, #0064D0); }
  .s7 .h { position: absolute; top: 79px; inset-inline-start: 733px; font-size: 48px; line-height: 1.2; color: #fff; }
  .s7 .bd { position: absolute; top: 168px; inset-inline-start: 730px; width: 472px; font-size: 20.4px; line-height: 25.6px; color: #fff; }
  /* 8: الملخص التنفيذي */
  .s8 .ph { position: absolute; top: 0; inset-inline-start: 560px; width: 720px; height: 720px; object-fit: cover; }
  .s8 .pan { position: absolute; top: 0; inset-inline-start: 0; width: 900px; height: 720px; }
  .s8 .h { position: absolute; top: 40px; inset-inline-start: 65px; font-size: 46px; line-height: 1.25; color: #fff; }
  .s8 ul { position: absolute; top: 172px; inset-inline-start: 72px; width: 430px; margin: 0; padding: 0; padding-inline-start: 32px; list-style: none; }
  .s8 li { position: relative; font-size: 23px; line-height: 29px; color: #fff; margin-bottom: 26px; }
  .s8 li::before { content: '•'; position: absolute; inset-inline-start: -29px; font-size: 18px; }
  /* 9: عنوان قسم */
  .s9 .ph { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 448px; object-fit: cover; }
  .s9 .dk { position: absolute; top: 448px; inset-inline-start: 0; width: 1280px; height: 272px; background: #262626; }
  .s9 .h { position: absolute; top: 132px; inset-inline-start: 0; width: 100%; text-align: center; font-size: 100px; font-weight: 700; line-height: 90px; letter-spacing: -4px; color: #fff; text-shadow: 3px 5px 7px rgb(0 0 0 / .35); }
  html[lang="ar"] .s9 .h { letter-spacing: 0; font-weight: 800; font-size: 92px; line-height: 96px; top: 126px; }
  .s9 .bd { position: absolute; top: 520px; inset-inline-start: 240px; width: 800px; text-align: center; font-size: 23.5px; line-height: 29px; color: #DDD; }
  /* 10: معلومات أساسية */
  .s10 { background: #F8F8F8; }
  .s10 .ph { position: absolute; top: 0; inset-inline-start: 560px; width: 720px; height: 720px; object-fit: cover; }
  .s10 .pan { position: absolute; top: 0; inset-inline-start: 0; width: 900px; height: 720px; }
  .s10 .h { position: absolute; top: 40px; inset-inline-start: 64px; font-size: 46px; line-height: 1.25; color: #444; }
  .s10 .q { position: absolute; top: 190px; inset-inline-start: 62px; width: 118px; height: 96px; filter: drop-shadow(1px 3px 3px rgb(0 0 0 / .25)); }
  .s10 .bd { position: absolute; top: 289px; inset-inline-start: 193px; width: 416px; font-size: 23.5px; line-height: 29px; color: #555; font-style: italic; }
  html[lang="ar"] .s10 .bd { font-style: normal; }
  /* 11: الرؤية والرسالة والقيم */
  .vm { position: absolute; top: 0; width: 344px; height: 320px; box-shadow: 0 3px 8px rgb(0 0 0 / .35); }
  .vm svg { position: absolute; top: 66px; inset-inline-start: 97px; width: 150px; height: 120px; color: #fff; filter: drop-shadow(2px 4px 3px rgb(0 0 0 / .3)); }
  .vm h2 { position: absolute; top: 228px; inset-inline-start: 33px; font-size: 58px; line-height: 1.2; color: #fff; text-shadow: 2px 3px 4px rgb(0 0 0 / .3); }
  .vmt { position: absolute; top: 366px; width: 318px; font-size: 22.5px; line-height: 29px; color: #DDD; }
  /* 12: تاريخنا */
  .s12 .h1, .s12 .h2 { position: absolute; inset-inline-start: 98px; font-size: 93px; font-weight: 700; line-height: 1; letter-spacing: -2.5px; text-shadow: 3px 5px 6px rgb(0 0 0 / .5); }
  html[lang="ar"] .s12 .h1, html[lang="ar"] .s12 .h2 { letter-spacing: 0; font-weight: 800; }
  .s12 .h1 { top: 100px; color: #fff; } .s12 .h2 { top: 190px; color: var(--blue); }
  .s12 .bd { position: absolute; top: 320px; inset-inline-start: 100px; width: 420px; font-size: 25.5px; line-height: 32px; color: #fff; }
  .yr { position: absolute; inset-inline-start: 640px; width: 104px; height: 104px; border-radius: 50%; display: grid; place-items: center; font-size: 25px; color: #fff; box-shadow: 0 3px 6px rgb(0 0 0 / .35); }
  .yt { position: absolute; inset-inline-start: 781px; width: 260px; }
  .yt h2 { font-size: 25.5px; line-height: 30px; color: #fff; }
  .yt p { margin-top: 7px; font-size: 17.5px; line-height: 21px; color: #fff; }
  /* 13: الهيكل التنظيمي */
  .op { position: absolute; border-radius: 50%; border: 5px solid #4A5A6A; object-fit: cover; background: #fff; box-shadow: 0 2px 5px rgb(0 0 0 / .5); }
  .ol { position: absolute; background: #3B7DD8; }
  .ot { position: absolute; font-size: 16.5px; line-height: 19px; color: #fff; }
  .ot b { display: block; font-size: 20px; line-height: 26px; margin-bottom: 4px; font-weight: 700; }
  .otc { text-align: center; width: 180px; }
  /* 14: خدماتنا */
  .sv { position: absolute; top: 152px; width: 360px; box-shadow: 0 3px 8px rgb(0 0 0 / .5); }
  .sv h2 { height: 56px; background: var(--blue); color: #fff; font-size: 24px; line-height: 56px; text-align: center; }
  .sv img { display: block; width: 360px; height: 240px; object-fit: cover; }
  .svt { position: absolute; top: 494px; width: 345px; font-size: 20.5px; line-height: 25px; color: #fff; }
  /* 15: منتجاتنا */
  .s15 .bgt { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 488px; object-fit: cover; object-position: top; }
  .s15 .lo { position: absolute; top: 488px; inset-inline-start: 0; width: 1280px; height: 232px; background: #F8F8F8; }
  .pi { position: absolute; display: flex; align-items: center; gap: 30px; }
  .pi i { width: 72px; height: 72px; border-radius: 50%; border: 3px solid var(--blue); color: var(--blue); display: grid; place-items: center; box-shadow: 0 2px 5px rgb(0 0 0 / .6); flex: none; }
  .pi i svg { width: 38px; height: 38px; }
  .pi p { font-size: 20px; line-height: 23px; color: #fff; }
  .s15 .bd { position: absolute; top: 549px; inset-inline-start: 225px; width: 830px; text-align: center; font-size: 21px; line-height: 25.5px; color: #444; }
  /* 16: الكفاءات الأساسية */
  .s16 .hx { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; }
  .hl { position: absolute; width: 150px; text-align: center; font-size: 18px; line-height: 23px; color: #fff; }
  .hl svg { display: block; width: 36px; height: 36px; margin: 0 auto 4px; }
  .hl.g { color: #555; } .hl.c { font-size: 15.5px; line-height: 19px; text-transform: uppercase; }
  .cs { position: absolute; width: 300px; }
  .cs h2 { font-size: 23px; line-height: 28px; color: #fff; text-transform: uppercase; }
  .cs p { margin-top: 6px; font-size: 16.5px; line-height: 19px; color: #fff; }
  .cs.e { text-align: end; }
  /* 17: البيئة الخارجية */
  .s17 .h { position: absolute; top: 60px; inset-inline-start: 64px; font-size: 44px; line-height: 48px; color: #fff; }
  .s17 .bd { position: absolute; top: 191px; inset-inline-start: 64px; width: 388px; font-size: 18.3px; line-height: 22.5px; color: #fff; }
  .s17 .ring { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; }
  .ec { position: absolute; width: 132px; height: 132px; border-radius: 50%; display: grid; place-items: center; text-align: center; font-size: 18px; line-height: 23px; }
  .ec.g { color: #444; } .ec.b { color: #fff; }
  .s17 .ct { position: absolute; inset-inline-start: 702px; width: 260px; text-align: center; font-size: 20px; line-height: 25px; color: #fff; text-transform: uppercase; }
  .s17 .sep { position: absolute; top: 362px; inset-inline-start: 693px; width: 280px; height: 1px; background: rgb(255 255 255 / .45); }
  /* 18: الاقتصاد */
  .s18 .ov { position: absolute; top: 263px; inset-inline-start: 0; width: 1280px; height: 457px; background: rgb(22 56 110 / .62); }
  .s18 .kp { position: absolute; top: 140px; inset-inline-start: 138px; display: flex; align-items: center; gap: 10px; font-size: 58px; font-weight: 700; line-height: 1; color: #fff; }
  .s18 .kp svg { width: 30px; height: 52px; }
  .s18 .bd { position: absolute; top: 151px; inset-inline-start: 339px; width: 760px; font-size: 20px; line-height: 24px; color: #fff; }
  .chart { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; pointer-events: none; }
  .chart text { font-family: var(--font); fill: #fff; }
  .s18 .call { position: absolute; top: 248px; inset-inline-start: 277px; width: 232px; height: 169px; background: var(--blue); text-align: center; color: #fff; box-shadow: 0 3px 6px rgb(0 0 0 / .4); }
  .s18 .call::after { content: ''; position: absolute; bottom: -15px; inset-inline-start: 101px; border: 15px solid transparent; border-bottom: 0; border-top-color: var(--blue); }
  .s18 .call p:nth-child(1) { margin-top: 20px; font-size: 23px; line-height: 28px; text-transform: uppercase; }
  .s18 .call p:nth-child(2) { font-size: 56px; font-weight: 700; line-height: 60px; }
  .s18 .call p:nth-child(3) { font-size: 19px; line-height: 24px; }
  /* 19: تحليل السوق */
  .mc { position: absolute; border-radius: 50%; box-shadow: 0 -2px 8px rgb(0 0 0 / .3); }
  .ml { position: absolute; inset-inline-start: 200px; width: 210px; text-align: center; font-size: 18px; line-height: 23px; color: #444; }
  .mq { position: absolute; width: 300px; text-align: center; color: #fff; }
  .mq h2 { font-size: 19.5px; line-height: 24px; text-transform: uppercase; }
  .mq .v { display: flex; justify-content: center; align-items: center; gap: 8px; height: 70px; margin-top: 18px; font-size: 52px; font-weight: 700; color: var(--blue); }
  .mq .v svg { width: 26px; height: 44px; }
  .mq p { margin-top: 12px; font-size: 18.5px; line-height: 23px; }
  .s19 .vl { position: absolute; top: 152px; inset-inline-start: 880px; width: 1px; height: 480px; background: rgb(255 255 255 / .4); }
  .s19 .hl2 { position: absolute; top: 390px; inset-inline-start: 577px; width: 607px; height: 1px; background: rgb(255 255 255 / .4); }
  /* 20: تحليل المنافسين */
  .s20 .ln { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; }
  .pill { position: absolute; width: 259px; height: 69px; border-radius: 35px; display: grid; place-items: center; text-align: center; font-size: 16.5px; line-height: 19px; box-shadow: 0 4px 6px rgb(0 0 0 / .45); }
  .pill.g { background: #D9D9D9; color: #444; } .pill.b { background: var(--blue); color: #fff; }
  .s20 .cc { position: absolute; top: 285px; inset-inline-start: 526px; width: 224px; height: 224px; border-radius: 50%; background: var(--blue); display: grid; place-items: center; text-align: center; color: #fff; font-size: 32px; line-height: 38px; box-shadow: 0 4px 8px rgb(0 0 0 / .45); }
  /* 21: المزايا التنافسية */
  .s21 .hb { position: absolute; top: 135px; inset-inline-start: 469px; width: 502px; height: 40px; background: #D9D9D9; color: #111; font-size: 22px; line-height: 40px; text-align: center; text-transform: uppercase; box-shadow: 0 3px 5px rgb(0 0 0 / .5); }
  .s21 .vb { position: absolute; top: 234px; inset-inline-start: 308px; width: 39px; height: 421px; background: #D9D9D9; box-shadow: 0 3px 5px rgb(0 0 0 / .5); }
  .s21 .vb span { position: absolute; top: 50%; inset-inline-start: 50%; width: 400px; transform: translate(-50%, -50%) rotate(-90deg); text-align: center; color: #111; font-size: 22px; line-height: 39px; text-transform: uppercase; white-space: nowrap; }
  html[dir="rtl"] .s21 .vb span { transform: translate(50%, -50%) rotate(-90deg); }
  .s21 .cl { position: absolute; top: 198px; width: 246px; text-align: center; font-size: 17.5px; line-height: 22px; color: #fff; }
  .s21 .rl { position: absolute; inset-inline-start: 380px; width: 75px; text-align: end; font-size: 17.5px; line-height: 23px; color: #fff; }
  .cq { position: absolute; display: grid; place-items: center; text-align: center; font-size: 25px; line-height: 32px; box-shadow: 0 3px 6px rgb(0 0 0 / .5); }
  /* 22: الاستراتيجية والخطط */
  .s22 .st { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; }
  .s22 .ax { position: absolute; inset-inline-start: 540px; width: 200px; text-align: center; font-size: 22px; line-height: 26px; color: #fff; text-transform: uppercase; }
  .sb { position: absolute; inset-inline-start: 163px; height: 50px; background: rgb(28 62 112 / .62); display: flex; align-items: center; justify-content: space-between; padding-inline-start: 22px; padding-inline-end: 28px; font-size: 20px; color: #fff; }
  .sx { position: absolute; color: #fff; display: grid; place-items: center; }
  .sx svg { width: 42px; height: 42px; }
  .sd { position: absolute; font-size: 18px; line-height: 23px; color: #fff; width: 300px; }
  /* 23: الأهداف */
  .s23 .tg { position: absolute; top: 180px; inset-inline-start: 195px; width: 380px; height: 430px; }
  .on { position: absolute; inset-inline-start: 688px; width: 86px; height: 86px; border-radius: 50%; display: grid; place-items: center; font-size: 35px; font-weight: 700; color: #fff; box-shadow: 0 3px 6px rgb(0 0 0 / .45); }
  .ob { position: absolute; inset-inline-start: 809px; width: 300px; }
  .ob h2 { font-size: 23.5px; line-height: 28px; color: #fff; }
  .ob p { margin-top: 7px; font-size: 16.5px; line-height: 19px; color: #fff; }
  /* 24: استراتيجية الأعمال */
  .s24 .ar { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; }
  .s24 .bx { position: absolute; top: 144px; inset-inline-start: 527px; width: 632px; height: 64px; border: 1px solid #3A7BD5; display: grid; place-items: center; font-size: 23px; color: #fff; }
  .bc { position: absolute; top: 250px; width: 199px; height: 264px; padding: 18px 22px 0; text-align: center; color: #fff; }
  .bc h2 { font-size: 20.5px; line-height: 25px; }
  .bc p { margin-top: 20px; font-size: 18px; line-height: 22.5px; }
  .s24 .sp { position: absolute; top: 211px; inset-inline-start: 216px; width: 128px; height: 128px; border-radius: 50%; border: 1px solid #3A7BD5; display: grid; place-items: center; text-align: center; font-size: 20px; line-height: 24px; color: #fff; }
  .s24 .ci { position: absolute; border-radius: 50%; display: grid; place-items: center; text-align: center; font-size: 19px; line-height: 23px; color: #fff; }
  .s24 .it { position: absolute; top: 572px; inset-inline-start: 424px; font-size: 22px; color: #fff; transform: rotate(7deg); transform-origin: left center; white-space: nowrap; }
  html[dir="rtl"] .s24 .it { transform: rotate(-7deg); transform-origin: right center; }
  /* 25: خطة التشغيل */
  .s25 .cv { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; }
  .s25 .yr2 { position: absolute; font-size: 30px; line-height: 36px; color: var(--blue); }
  .s25 .oi { position: absolute; width: 330px; }
  .s25 .oi h2 { font-size: 20.5px; line-height: 26px; color: #fff; }
  .s25 .oi p { margin-top: 4px; font-size: 16.5px; line-height: 19px; color: #fff; }
  .s25 .oi.e { text-align: end; }
  .s25 .ic2 { position: absolute; width: 44px; height: 44px; display: grid; place-items: center; color: #fff; }
  .s25 .ic2 svg { width: 24px; height: 24px; }
  /* 26: التحليل المالي */
  .s26 .bd { position: absolute; top: 548px; inset-inline-start: 93px; width: 495px; font-size: 18.3px; line-height: 23px; color: #fff; }
  .ft2 { position: absolute; border-collapse: collapse; color: #fff; }
  .ft2 td { padding: 0 19px; }
  .ft2 td:last-child { text-align: end; }
  .s26 .ft2 { top: 130px; inset-inline-start: 712px; width: 488px; box-shadow: 0 4px 8px rgb(0 0 0 / .45); }
  .s26 .ft2 tr { height: 51.5px; }
  .s26 .ft2 td { font-size: 18px; background: rgb(18 48 98 / .55); border-bottom: 1px solid rgb(255 255 255 / .1); }
  .s26 .ft2 td:first-child { border-inline-end: 1px solid rgb(255 255 255 / .1); width: 245px; }
  .s26 .ft2 tr:first-child td { height: 48px; background: rgb(5 45 110 / .92); font-size: 20px; }
  .s26 .ft2 tr:last-child td { height: 51px; background: #0A5FC8; font-size: 21px; font-weight: 700; border: 0; }
  /* 27: تحليل التعادل */
  .s27 .be { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; }
  .s27 .bc2 { position: absolute; top: 171px; inset-inline-start: 456px; width: 134px; height: 134px; border-radius: 50%; background: var(--blue); display: grid; place-items: center; text-align: center; color: #fff; font-size: 20px; line-height: 25px; box-shadow: 0 3px 6px rgb(0 0 0 / .45); }
  .s27 .lb { position: absolute; font-size: 22px; line-height: 26px; color: #fff; }
  .s27 .cost { top: 409px; inset-inline-start: 72px; width: 64px; text-align: center; transform: rotate(-90deg); }
  .s27 .qty { top: 612px; inset-inline-start: 410px; width: 200px; text-align: center; }
  .bi { position: absolute; inset-inline-start: 950px; width: 300px; color: #fff; }
  .bi h2 { font-size: 20px; line-height: 26px; text-transform: uppercase; }
  .bi p { margin-top: 9px; font-size: 16.5px; line-height: 19px; }
  /* 28: توقعات المبيعات */
  .s28 .pn { position: absolute; top: 112px; inset-inline-start: 864px; width: 352px; height: 311px; background: rgb(22 58 115 / .55); }
  .s28 .dn { position: absolute; top: 169px; inset-inline-start: 940px; width: 200px; height: 200px; }
  .s28 .pc { position: absolute; top: 236px; inset-inline-start: 940px; width: 200px; text-align: center; color: #fff; font-size: 56px; font-weight: 700; line-height: 66px; }
  .s28 .pc small { font-size: 34px; }
  .s28 .h { position: absolute; top: 462px; inset-inline-start: 873px; font-size: 34px; line-height: 44px; color: #fff; }
  .s28 .bd { position: absolute; top: 519px; inset-inline-start: 873px; width: 327px; font-size: 18px; line-height: 22.5px; color: #fff; }
  /* 29: الإنفاق الرأسمالي */
  .pnl { position: absolute; background: rgb(30 62 112 / .5); }
  .yc { position: absolute; top: 128px; width: 197px; height: 252px; background: rgb(14 38 80 / .72); color: #fff; padding-inline-start: 19px; }
  .yc h2 { position: absolute; top: 20px; inset-inline-start: 0; width: 100%; text-align: center; font-size: 20px; line-height: 26px; }
  .yc .rg { position: absolute; top: 67px; inset-inline-start: 52px; width: 92px; height: 92px; border-radius: 50%; border: 3px solid #2F80F0; display: grid; place-items: center; font-size: 30px; box-shadow: 0 3px 6px rgb(0 0 0 / .6), inset 0 0 4px rgb(0 0 0 / .4); }
  .yc p { position: absolute; top: 177px; inset-inline-start: 19px; width: 166px; font-size: 17.2px; line-height: 19px; }
  /* 30: تكاليف التشغيل */
  .s30 .bxo { position: absolute; top: 130px; inset-inline-start: 707px; width: 518px; height: 334px; border: 1px solid #2F7BF0; }
  .s30 .pt { position: absolute; top: 150px; inset-inline-start: 707px; width: 518px; text-align: center; font-size: 36px; line-height: 44px; color: var(--blue); text-transform: uppercase; }
  .s30 .stk { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; }
  .s30 .pr { position: absolute; top: 354px; width: 62px; height: 62px; border-radius: 50%; border: 2.5px solid #2F7BF0; display: grid; place-items: center; color: #fff; font-size: 18px; }
  .s30 .h { position: absolute; top: 502px; inset-inline-start: 706px; font-size: 30px; font-weight: 700; line-height: 36px; color: #fff; }
  .s30 .bd { position: absolute; top: 555px; inset-inline-start: 706px; width: 445px; font-size: 18.5px; line-height: 23px; color: #fff; }
  .lg { position: absolute; display: flex; align-items: center; gap: 22px; font-size: 18.5px; color: #fff; }
  .lg i { width: 36px; height: 19px; box-shadow: 2px 3px 3px rgb(0 0 0 / .45); }
  /* 31–33: الجداول المالية */
  .tb { position: absolute; border-collapse: collapse; color: #fff; table-layout: fixed; box-shadow: 0 4px 8px rgb(0 0 0 / .4); }
  .tb td { padding: 0 19px; font-size: 18px; background: rgb(22 52 100 / .58); border-bottom: 1px solid rgb(255 255 255 / .1); border-inline-start: 1px solid rgb(255 255 255 / .1); white-space: nowrap; overflow: hidden; }
  .tb td:first-child { border-inline-start: 0; }
  .tb td + td { text-align: end; }
  .tb tr.tot td { background: #0A5FC8; font-size: 22px; border: 0; }
  .tb tr.hi td { background: #0A5FC8; border: 0; } .tb tr.hi2 td { background: #1D80F7; border: 0; }
  .tb tr.hd td { background: transparent; border: 0; padding: 0; }
  .tb tr.hd td.c { background: #7DB6F6; padding: 0 19px; font-size: 19px; }
  .tb tr.hd2 td { background: transparent; border: 0; padding: 0; }
  .tb tr.hd2 td.c { background: #1D80F7; padding: 0 19px; font-size: 19px; }
  .s31 .lh { position: absolute; top: 126px; font-size: 25px; line-height: 32px; color: #fff; }
  .s31 .cn { position: absolute; top: 128px; width: 40px; height: 40px; color: var(--orange); }
  .s31 .ot { position: absolute; top: 589px; inset-inline-start: 665px; width: 552px; display: flex; justify-content: space-between; color: var(--blue); font-size: 25px; line-height: 32px; text-transform: uppercase; }
  .s31 .ot b { font-weight: 700; }
  /* 34: نظرة على المخاطر */
  .s34 .bd { position: absolute; top: 157px; inset-inline-start: 240px; width: 800px; text-align: center; font-size: 23.5px; line-height: 29px; color: #fff; }
  .rb { position: absolute; width: 422px; height: 46px; background: var(--blue); color: #fff; font-size: 20px; line-height: 46px; text-align: center; box-shadow: 0 3px 5px rgb(0 0 0 / .45); }
  /* 35: تحليل المخاطر */
  .s35 .rg3 { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; }
  .rl3 { position: absolute; width: 140px; text-align: center; color: #fff; font-size: 18px; line-height: 22px; }
  .rl3 svg { display: block; width: 46px; height: 46px; margin: 0 auto 6px; }
  .s35 .ctr { position: absolute; top: 386px; inset-inline-start: 340px; width: 160px; text-align: center; color: #fff; font-size: 20px; line-height: 25px; }
  .s35 .ctr b { font-size: 19px; font-weight: 700; text-transform: uppercase; }
  .ri { position: absolute; inset-inline-start: 794px; width: 320px; }
  .ri h2 { font-size: 25px; line-height: 30px; font-weight: 700; }
  .ri p { margin-top: 3px; font-size: 18.3px; line-height: 23px; color: #fff; }
  /* 36: تحليل سوات */
  .sw { position: absolute; width: 411px; height: 231px; background: rgb(18 44 92 / .72); }
  .sw h2 { height: 40px; padding-inline-start: 19px; font-size: 18px; line-height: 40px; text-transform: uppercase; }
  .sw ol { margin: 25px 0 0; padding: 0; padding-inline-start: 64px; color: #fff; font-size: 16.5px; line-height: 28px; }
  .sw li { padding-inline-start: 18px; }
  .sl2 { position: absolute; font-size: 112px; font-weight: 700; line-height: 1; text-shadow: 3px 4px 5px rgb(0 0 0 / .55); }
  /* 37: العوامل المقيّدة */
  .s37 .bd { position: absolute; top: 277px; inset-inline-start: 165px; width: 415px; text-align: center; font-size: 21.5px; line-height: 25.5px; color: #fff; }
  .s37 .vl { position: absolute; top: 209px; inset-inline-start: 656px; width: 1px; height: 366px; background: #3A7BD5; }
  .s37 .nb { position: absolute; inset-inline-start: 752px; width: 48px; height: 48px; border-radius: 50%; background: var(--blue); display: grid; place-items: center; color: #fff; font-size: 20px; font-weight: 700; box-shadow: 0 3px 5px rgb(0 0 0 / .5); }
  .s37 .nt { position: absolute; inset-inline-start: 834px; width: 250px; font-size: 15.5px; line-height: 18px; color: #fff; }
  /* 38: عوامل النجاح الحاسمة */
  .s38 .hp { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; }
  .cf { position: absolute; width: 84px; height: 84px; display: grid; place-items: center; color: #fff; }
  .cf svg { width: 40px; height: 40px; }
  .s38 .mid { position: absolute; top: 352px; inset-inline-start: 604px; width: 72px; height: 76px; color: #fff; }
  .cfl { position: absolute; width: 300px; color: #fff; }
  .cfl h2 { font-size: 20.5px; line-height: 26px; font-weight: 700; }
  .cfl p { margin-top: 3px; font-size: 16.5px; line-height: 19px; }
  .cfl.e { text-align: end; }
  /* 39: السيناريوهات البديلة */
  .as { position: absolute; display: flex; gap: 37px; }
  .as i { flex: none; width: 104px; height: 104px; border-radius: 50%; background: var(--blue); color: #fff; display: grid; place-items: center; box-shadow: 0 4px 6px rgb(0 0 0 / .5); }
  .as i svg { width: 56px; height: 56px; }
  .as h2 { margin-top: 10px; font-size: 25px; line-height: 30px; color: #fff; }
  .as p { margin-top: 6px; font-size: 18px; line-height: 20px; color: #C8C8C8; }
  /* 40: المخاطر والحلول */
  .rh { position: absolute; top: 160px; width: 423px; height: 60px; background: var(--blue); display: flex; align-items: center; justify-content: space-between; padding-inline: 19px 18px; color: #fff; font-size: 30px; font-weight: 700; box-shadow: 0 4px 6px rgb(0 0 0 / .5); }
  .rh svg { width: 46px; height: 46px; }
  .s40 .vl { position: absolute; top: 160px; inset-inline-start: 640px; width: 1px; height: 440px; background: #3A7BD5; }
  .s40 .amp { position: absolute; top: 333px; inset-inline-start: 597px; width: 86px; height: 86px; border-radius: 50%; background: var(--blue); display: grid; place-items: center; color: #fff; font-size: 40px; font-weight: 700; box-shadow: 0 4px 6px rgb(0 0 0 / .5); }
  .ck { position: absolute; display: flex; align-items: center; gap: 13px; width: 400px; font-size: 20px; line-height: 23px; color: #fff; }
  .ck svg { flex: none; width: 24px; height: 24px; color: #2F80F0; }
  /* 41: الخلاصة */
  .s41 .bn { position: absolute; top: 0; inset-inline-start: 0; width: 1280px; height: 720px; }
  .s41 .bd { position: absolute; top: 285px; inset-inline-start: 340px; width: 600px; text-align: center; font-size: 21.8px; line-height: 29px; color: #fff; }
  /* 42: شكرًا */
  .s42 { background: #262626; }
  .s42 .ty { position: absolute; top: 132px; inset-inline-start: 0; width: 100%; text-align: center; font-family: 'Oswald', var(--font); font-weight: 700; font-size: 70px; line-height: 70px; color: #fff; letter-spacing: 1px; text-shadow: 2px 3px 4px rgb(0 0 0 / .5); }
  .s42 .ty b { display: block; font-size: 110px; line-height: 108px; color: var(--blue); }
  html[lang="ar"] .s42 .ty { font-family: var(--font); font-weight: 800; font-size: 72px; line-height: 92px; top: 124px; }
  html[lang="ar"] .s42 .ty b { font-weight: 800; font-size: 104px; line-height: 120px; }
  .s42 .bd { position: absolute; top: 562px; inset-inline-start: 0; width: 100%; text-align: center; font-size: 18.5px; line-height: 24px; color: #fff; }"""

# ---------------------------------------------------------------- الأيقونات
SOC = {
    'tw': '<svg class="tw" viewBox="0 0 24 20" fill="currentColor" aria-hidden="true"><path d="M24 2.4a9.8 9.8 0 0 1-2.8.8A4.9 4.9 0 0 0 23.3.5a9.9 9.9 0 0 1-3.1 1.2A4.9 4.9 0 0 0 11.8 6 14 14 0 0 1 1.7.9a4.9 4.9 0 0 0 1.5 6.6A4.9 4.9 0 0 1 1 6.9v.1a4.9 4.9 0 0 0 3.9 4.8 5 5 0 0 1-2.2.1 4.9 4.9 0 0 0 4.6 3.4A9.9 9.9 0 0 1 0 17.3 14 14 0 0 0 7.5 19.5c9.1 0 14-7.5 14-14v-.6A10 10 0 0 0 24 2.4z"/></svg>',
    'fb': '<svg class="fb" viewBox="0 0 10 20" fill="currentColor" aria-hidden="true"><path d="M6.5 20V11h3l.5-3.5H6.5V5.3c0-1 .3-1.7 1.8-1.7H10V.5A24 24 0 0 0 7.4.3C4.8.3 3 1.9 3 4.8v2.7H0V11h3v9z"/></svg>',
    'li': '<svg class="li" viewBox="0 0 22 20" fill="currentColor" aria-hidden="true"><rect x="0" y="6.5" width="4.4" height="13.5"/><circle cx="2.2" cy="2.4" r="2.4"/><path d="M7.5 6.5h4.2v1.9c.6-1.1 2-2.2 4.2-2.2 4.4 0 5.1 2.9 5.1 6.6V20h-4.4v-6.4c0-1.6 0-3.6-2.2-3.6s-2.5 1.7-2.5 3.5V20H7.5z"/></svg>',
    'gp': '<svg viewBox="0 0 26 20" fill="currentColor" aria-hidden="true"><path d="M8.2 8.6v3h4.4c-.3 1.5-1.6 3.2-4.4 3.2a4.8 4.8 0 0 1 0-9.6c1.5 0 2.5.6 3 1.1l2.1-2A7.7 7.7 0 0 0 8.2 2.2a7.8 7.8 0 1 0 0 15.6c4.5 0 7.5-3.2 7.5-7.6 0-.5 0-.9-.1-1.6zM26 8.6h-2.5V6.1h-2.2v2.5h-2.5v2.2h2.5v2.5h2.2v-2.5H26z"/></svg>',
}


def ft(n, light=False):
    """التذييل المشترك: أيقونات التواصل + رقم الشريحة."""
    return '<div class="ft">' + SOC['tw'] + SOC['fb'] + SOC['li'] + '</div><div class="pg">%d</div>' % n


def easel(mid_white):
    """شعار اللوحة والحامل داخل حلقة (إحداثيات الشريحة نفسها)."""
    col = 'var(--blue)' if not mid_white else '#fff'
    ring = 'var(--blue)' if not mid_white else '#fff'
    mid = '#fff'
    return ('<svg class="logo" viewBox="514 162 246 246" aria-hidden="true">'
            '<circle class="ring" cx="637" cy="285" r="121" fill="none" stroke="%s" stroke-width="2.6"/>' % ring
            + '<g fill="none" stroke="%s" stroke-linecap="round" stroke-linejoin="round">' % col
            + '<path d="M582 221 H692" stroke-width="8"/><path d="M588 225 V297 H687 V225" stroke-width="6"/>'
            + '<path d="M613 300 L605 343 M661 300 L669 343" stroke-width="8"/><path d="M581 314 L620 286" stroke-width="5"/>'
            + '<path d="M634 262 V284 M677 251 V284" stroke-width="7"/></g>'
            + '<path d="M655 241 V284" stroke="%s" stroke-width="7" stroke-linecap="round"/>' % mid
            + '</svg>')


WAVES = ('<svg class="waves flipx" viewBox="0 0 1280 720" aria-hidden="true">'
         '<defs><linearGradient id="wv1" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#70B6F3"/><stop offset=".5" stop-color="#56A9F6"/><stop offset="1" stop-color="#3EA4F6"/></linearGradient>'
         '<linearGradient id="wv2" x1="0" y1="0" x2=".5" y2="1"><stop offset="0" stop-color="#41C2FF"/><stop offset=".45" stop-color="#8EE9FF"/><stop offset="1" stop-color="#86D8FA"/></linearGradient></defs>'
         '<path d="M0 0 H1280 V138 C1160 100 1040 72 920 80 C660 104 320 190 0 312 Z" fill="url(#wv1)"/>'
         '<path d="M0 0 H440 C290 10 140 32 0 62 Z" fill="#1588FA"/>'
         '<path d="M0 62 C140 32 290 10 440 0 H690 C480 40 240 130 0 252 Z" fill="url(#wv2)"/>'
         '</svg>')

TOC_ROWS = [218, 307, 396, 486, 575]


def toc():
    h = ''
    for i, y in enumerate(TOC_ROWS):
        for col, x in ((0, 192), (1, 714)):
            n = i + 1 + col * 5
            h += '\n    <div class="toc" style="top:%dpx; inset-inline-start:%dpx"><b>%02d</b><p>§TOC§</p></div>' % (y - 29, x, n)
    return h


SL = []
# 1 الغلاف الداكن
SL.append('\n    <img class="bg" src="' + A + 'cover-bg.jpg" alt="§ALT_COVER§">\n    ' + easel(False)
          + '\n    <h1 class="cov1">§COV1§</h1>\n    <p class="cov2">§COV2§</p>\n  ')
# 2 الغلاف الأزرق
SL.append('\n    ' + WAVES + '\n    ' + easel(True) + '\n    <h1 class="cov1">§COV1§</h1>\n    <p class="cov2">§COV2§</p>\n  ')
# 3 الفهرس
SL.append('\n    <div class="bg bluebg"></div>\n    <h1 class="t">§TOC_T§</h1>' + toc() + '\n    ' + ft(3) + '\n  ')
# 4 تواصل معنا
SO = [('fb', 'FB', 434), ('li', 'LI', 503), ('tw', 'TW', 571), ('gp', 'GP', 638)]
SL.append('\n    <img class="map" src="' + A + 'map.jpg" alt="§ALT_MAP§">\n    <div class="pb"></div><div class="pd"></div>'
          '\n    <h1 class="h">§CU_T§</h1>\n    <h2 class="h2">§SAMPLE_T§</h2>'
          '\n    <p class="bd">§CU_B1§<br><br>§EMAIL§<br>§PHONE§ <span dir="ltr">(999) 999-9999</span></p>'
          + ''.join('\n    <div class="so" style="top:%dpx"><i>%s</i><p>§FOLLOW§<br>§SO_%s§</p></div>' % (y - 20, SOC[k], s) for k, s, y in SO)
          + '\n    ' + ft(4) + '\n  ')
# 5 ضبط الوثيقة
DC = [(208, 'DC1'), (274, 'DC2'), (340, 'DC1'), (406, 'DC1'), (472, 'DC3'), (538, 'DC3')]
SL.append('\n    <div class="bg bluebg"></div>\n    <h1 class="t">§DC_T§</h1>'
          + ''.join('\n    <p class="dc" style="top:%dpx">§%s§</p>' % (y - 14, k) for y, k in DC)
          + ''.join('\n    <div class="dl" style="top:%dpx"></div>' % y for y in (241, 308, 374, 440, 508))
          + '\n    ' + ft(5) + '\n  ')
# 6 فريقنا
SL.append('\n    <div class="top"></div>\n    <h1 class="t">§TEAM_T§</h1>'
          + ''.join('\n    <div class="tm" style="inset-inline-start:%dpx"><img src="%steam-%d.jpg" alt="§ALT_PERSON§"><h2>§SAMPLE_T§</h2><p>§TEAM_B§</p></div>' % (x, A, i + 1)
                    for i, x in enumerate((126, 399, 673, 946)))
          + '\n    <div class="light">' + ft(6) + '</div>\n  ')
# 7 التعريفات
SL.append('\n    <img class="ph" src="' + A + 'definitions.jpg" alt="§ALT_DEF§">\n    <div class="pn"></div>'
          '\n    <h1 class="h">§DEF_T§</h1>\n    <p class="bd">§LOREM§</p>\n    ' + ft(7) + '\n  ')
# 8 الملخص التنفيذي
SL.append('\n    <img class="ph flipx" src="' + A + 'exec-summary.jpg" alt="§ALT_EXEC§">'
          '\n    <svg class="pan flipx" viewBox="0 0 900 720" aria-hidden="true"><defs><linearGradient id="es" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#1088F8"/><stop offset="1" stop-color="#0567D6"/></linearGradient></defs>'
          '<polygon points="0,0 860,0 545,720 0,720" fill="url(#es)"/><polygon points="860,0 890,0 580,720 545,720" fill="#2A86F8"/></svg>'
          '\n    <h1 class="h">§ES_T§</h1>\n    <ul><li>§B1§</li><li>§B2§</li><li>§B1§</li><li>§B1§</li><li>§B3§</li><li>§B3§</li></ul>\n    ' + ft(8) + '\n  ')

SLIDES = SL

STR = {
    'ar': {
        'ALT_COVER': 'خلفية هندسية داكنة', 'ALT_MAP': 'خريطة الموقع', 'ALT_PERSON': 'صورة عضو الفريق', 'ALT_DEF': 'رجل أعمال يعدّل ربطة عنقه', 'ALT_EXEC': 'رجلا أعمال مبتسمان',
        'COV1': 'خطة العمل', 'COV2': 'قوالب عروض تقديمية',
        'TOC_T': 'جدول المحتويات', 'TOC': 'هذا نص توضيحي.<br>أضف النص المطلوب هنا.',
        'CU_T': 'تواصل معنا', 'SAMPLE_T': 'نص توضيحي', 'CU_B1': 'هذا نص توضيحي.<br>أضف النص المطلوب هنا.',
        'EMAIL': 'البريد الإلكتروني: بريدك@شركتك', 'PHONE': 'الهاتف:', 'FOLLOW': 'تابعنا على',
        'SO_FB': 'فيسبوك', 'SO_LI': 'لينكد إن', 'SO_TW': 'تويتر', 'SO_GP': 'جوجل بلس',
        'DC_T': 'ضبط الوثيقة', 'DC1': 'هذا نص توضيحي. أضف النص المطلوب هنا.', 'DC2': 'هذا نص توضيحي.', 'DC3': 'أضف النص المطلوب هنا.',
        'TEAM_T': 'فريقنا', 'TEAM_B': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
        'DEF_T': 'التعريفات',
        'LOREM': 'هذا نص توضيحي يمكن استبداله بنص آخر في المساحة نفسها، وقد وُضع هنا ليعطي تصورًا عن شكل الفقرة عند تعبئتها بالمحتوى الحقيقي. يُكتب في هذه المساحة تعريف المصطلحات الرئيسية الواردة في خطة العمل، مثل الإيرادات والتكاليف التشغيلية وهامش الربح ونقطة التعادل، حتى يفهمها القارئ بالمعنى نفسه في كل أقسام الوثيقة. ويمكن إضافة تعريفات أخرى بحسب طبيعة النشاط والسوق المستهدفة، مع الحرص على أن تكون واضحة ومختصرة ومتسقة مع المصطلحات المستخدمة في الشرائح التالية، وأن تُراجع دوريًا كلما تغيّرت الخطة.',
        'ES_T': 'الملخص التنفيذي', 'B1': 'هذا نص توضيحي. أضف النص المطلوب هنا.', 'B2': 'هذا نص توضيحي.', 'B3': 'أضف النص المطلوب هنا.',
        'ALT_CITY': 'ناطحات سحاب', 'ALT_ROOM': 'قاعة اجتماعات', 'ALT_CORP': 'رجل وامرأة يراجعان مستندًا', 'ALT_SVC1': 'فني يركّب مقبسًا', 'ALT_SVC2': 'رجل يكتب على أريكة', 'ALT_SVC3': 'يد تكتب على لوحة مفاتيح',
    },
    'en': {
        'ALT_COVER': 'Dark geometric background', 'ALT_MAP': 'Location map', 'ALT_PERSON': 'Team member photo', 'ALT_DEF': 'Businessman adjusting his tie', 'ALT_EXEC': 'Two smiling businessmen',
        'COV1': 'BUSINESS PLAN', 'COV2': 'POWERPOINT TEMPLATES',
        'TOC_T': 'Table of Contents', 'TOC': 'This is a sample text.<br>Insert your desired text here.',
        'CU_T': 'Contact Us', 'SAMPLE_T': 'Sample Text', 'CU_B1': 'This is a sample text.<br>Insert your desired text here.',
        'EMAIL': 'Email: you@company.com', 'PHONE': 'Phone:', 'FOLLOW': 'Follow us on',
        'SO_FB': 'Facebook', 'SO_LI': 'LinkedIn', 'SO_TW': 'Twitter', 'SO_GP': 'Google+',
        'DC_T': 'Document Control', 'DC1': 'This is a sample text. Insert your desired text here.', 'DC2': 'This is a sample text.', 'DC3': 'Insert your desired text here.',
        'TEAM_T': 'Our Team', 'TEAM_B': 'This is a sample text. Insert your desired text here.',
        'DEF_T': 'Definitions',
        'LOREM': 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec blandit dui ac est molestie, at volutpat est tempor. Maecenas tincidunt pharetra gravida. Praesent ac feugiat massa. Vivamus fringilla convallis ornare. Proin blandit, massa placerat tempor sagittis, risus magna convallis arcu, in rhoncus sapien leo in elit. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec at volutpat lorem, a varius nisi. Nulla malesuada, neque eu porttitor mattis, lectus eros condimentum lacus, ut maximus sem lectus ac ipsum. Suspendisse mi sem, maximus nec bibendum ut, sollicitudin sit amet mauris. Aliquam vel arcu sit amet risus egestas pretium eu eget urna. Sed interdum sem et lorem auctor, a auctor enim volutpat.',
        'ES_T': 'Executive Summary', 'B1': 'This is a sample text. Insert your desired text here.', 'B2': 'This is a sample text.', 'B3': 'Insert your desired text here.',
        'ALT_CITY': 'Skyscrapers', 'ALT_ROOM': 'Meeting room', 'ALT_CORP': 'Man and woman reviewing a document', 'ALT_SVC1': 'Technician fitting a socket', 'ALT_SVC2': 'Man writing on a sofa', 'ALT_SVC3': 'Hand typing on a keyboard',
    },
}

SECTIONS = [
    ('الغلاف الداكن', 'Dark Cover', False), ('الغلاف الأزرق', 'Blue Cover', False), ('جدول المحتويات', 'Table of Contents', False),
    ('تواصل معنا', 'Contact Us', False), ('ضبط الوثيقة', 'Document Control', False), ('فريقنا', 'Our Team', False),
    ('التعريفات', 'Definitions', False), ('الملخص التنفيذي', 'Executive Summary', False),
]

# ======================================================== الشرائح 9–14
CITY = '\n    <img class="bg" src="' + A + 'bg-city.jpg" alt="§ALT_CITY§">'
ICO = {
    'eye': '<svg viewBox="0 0 150 120" fill="currentColor" aria-hidden="true"><path fill-rule="evenodd" d="M2 56 Q75 -30 148 56 Q75 142 2 56 Z M75 22 a32 32 0 1 0 .1 0 Z"/><circle cx="75" cy="54" r="22" fill="none" stroke="currentColor" stroke-width="7"/><circle cx="83" cy="46" r="8"/></svg>',
    'rocket': '<svg viewBox="0 0 150 120" fill="currentColor" aria-hidden="true"><path d="M134 4 C105 4 80 20 64 46 L45 50 L30 66 L52 68 L70 86 L72 108 L88 93 L92 74 C118 58 134 33 134 4 Z M108 22 a9 9 0 1 1 -.1 0 Z" fill-rule="evenodd"/><path d="M48 76 C36 80 30 92 28 112 C46 110 58 104 62 92 Z" fill-opacity=".95"/></svg>',
    'coins': '<svg viewBox="0 0 150 120" fill="currentColor" aria-hidden="true"><rect x="38" y="2" width="76" height="16" rx="8"/><rect x="28" y="24" width="76" height="16" rx="8"/><rect x="28" y="46" width="60" height="16" rx="8"/><rect x="28" y="68" width="56" height="16" rx="8"/><rect x="28" y="90" width="60" height="16" rx="8"/><circle cx="110" cy="77" r="36" stroke="#465866" stroke-width="5"/><path d="M122 62 Q113 54 104 60 Q98 66 98 77 Q98 88 104 94 Q113 100 122 92 M92 72 H112 M92 82 H112" fill="none" stroke="#465866" stroke-width="7" stroke-linecap="round"/></svg>',
}
# 9 عنوان قسم
SL.append('\n    <img class="ph" src="' + A + 'title-room.jpg" alt="§ALT_ROOM§">\n    <div class="dk"></div>'
          '\n    <h1 class="h">§S9_T1§<br>§S9_T2§</h1>\n    <p class="bd">§S9_B§</p>\n    <div class="pg">9</div>\n  ')
# 10 معلومات أساسية عن الشركة
QUOTE = '<path d="M0 0 H48 V53 H38 Q29 53 29 62.5 Q29 72 38 72 H48 V92 H18 Q0 92 0 74 Z"/>'
SL.append('\n    <img class="ph flipx" src="' + A + 'corporate.jpg" alt="§ALT_CORP§">'
          '\n    <svg class="pan flipx" viewBox="0 0 900 720" aria-hidden="true"><polygon points="0,0 860,0 548,720 0,720" fill="#F8F8F8"/><polygon points="860,0 888,0 578,720 548,720" fill="#1D80F7"/></svg>'
          '\n    <h1 class="h">§S10_T§</h1>'
          '\n    <svg class="q flipx" viewBox="0 0 118 96" fill="#1D80F7" aria-hidden="true"><g>' + QUOTE + '</g><g transform="translate(66 0)">' + QUOTE + '</g></svg>'
          '\n    <p class="bd">§S10_B§</p>\n    <div class="light">' + ft(10) + '</div>\n  ')
# 11 الرؤية والرسالة والقيم
VM = [(94, '#1D80F7', 'eye', 'VIS'), (469, '#F98226', 'rocket', 'MIS'), (842, '#465866', 'coins', 'VAL')]
SL.append('\n    <img class="bg" src="' + A + 'bg-vision.jpg" alt="§ALT_CITY§">'
          + ''.join('\n    <div class="vm" style="inset-inline-start:%dpx; background:%s">%s<h2>§%s§</h2></div><p class="vmt" style="inset-inline-start:%dpx">§S11_B§</p>' % (x, c, ICO[i], k, x + 7) for x, c, i, k in VM)
          + '\n    ' + ft(11) + '\n  ')
# 12 تاريخنا
YR = [(148, '#0A5FC8', '#fff', '2019'), (289, '#1D80F7', '#fff', '2018'), (430, '#7DB6F6', '#fff', '2017'), (572, '#B7D7FB', '#3A4552', '2016')]
SL.append('\n    <img class="bg" src="' + A + 'history.jpg" alt="§ALT_ROOM§">'
          '\n    <h1 class="h1">§S12_T1§</h1>\n    <h1 class="h2">§S12_T2§</h1>\n    <p class="bd">§S12_B§</p>'
          + ''.join('\n    <div class="yr" style="top:%dpx; background:%s; color:%s">%s</div><div class="yt" style="top:%dpx"><h2>§SAMPLE_t§</h2><p>§S12_I§</p></div>' % (y - 52, bg, fg, yr, y - 39) for y, bg, fg, yr in YR)
          + '\n    ' + ft(12) + '\n  ')
# 13 الهيكل التنظيمي


def op(cx, cy, r, img, col):
    return '<img class="op" src="%s%s.jpg" alt="§ALT_PERSON§" style="top:%dpx; inset-inline-start:%dpx; width:%dpx; height:%dpx; border-color:%s">' % (A, img, cy - r, cx - r, 2 * r, 2 * r, col)


def ln(x, y, w, h):
    return '<div class="ol" style="top:%dpx; inset-inline-start:%dpx; width:%dpx; height:%dpx"></div>' % (y, x, w, h)


S13 = CITY + '\n    <h1 class="t">§S13_T§</h1>\n    '
S13 += ln(640, 283, 1, 15) + ln(430, 297, 422, 1) + ln(430, 297, 1, 22) + ln(851, 297, 1, 22)
S13 += ln(430, 423, 1, 14) + ln(274, 437, 246, 1) + ln(274, 437, 1, 20) + ln(519, 437, 1, 20)
S13 += ln(851, 423, 1, 14) + ln(765, 437, 242, 1) + ln(765, 437, 1, 20) + ln(1006, 437, 1, 20)
S13 += '\n    ' + op(640, 215, 70, 'team-4', '#F98226') + op(430, 371, 55, 'team-3', '#1D80F7') + op(851, 371, 55, 'org-m2', '#1D80F7')
S13 += '\n    ' + ''.join(op(x, 511, 54, im, '#4A5A6A') for x, im in ((273, 'team-1'), (518, 'team-2'), (762, 'team-3'), (1006, 'team-4')))
S13 += '\n    <p class="ot" style="top:174px; inset-inline-start:748px; width:240px"><b>§S13_H§</b>§S13_B2§</p>'
S13 += '\n    <p class="ot" style="top:343px; inset-inline-start:507px; width:180px">§S13_B3§</p><p class="ot" style="top:343px; inset-inline-start:932px; width:180px">§S13_B3§</p>'
S13 += ''.join('\n    <p class="ot otc" style="top:575px; inset-inline-start:%dpx">§S13_B3§</p>' % (x - 90) for x in (273, 518, 762, 1006))
SL.append(S13 + '\n    ' + ft(13) + '\n  ')
# 14 خدماتنا
SL.append(CITY + '\n    <h1 class="t">§S14_T§</h1>'
          + ''.join('\n    <div class="sv" style="inset-inline-start:%dpx"><h2>§SAMPLE_T§</h2><img src="%ssvc-%d.jpg" alt="§ALT_SVC%d§"></div>' % (x, A, i + 1, i + 1) for i, x in enumerate((64, 460, 856)))
          + '\n    <p class="svt" style="inset-inline-start:74px">§S14_B1§</p><p class="svt" style="inset-inline-start:467px">§S14_B2§</p><p class="svt" style="inset-inline-start:866px">§S14_B1§</p>'
          + '\n    ' + ft(14) + '\n  ')
SECTIONS += [('عنوان قسم', 'Section Title', False), ('معلومات أساسية عن الشركة', 'Basic Corporate Information', False),
             ('الرؤية والرسالة والقيم', 'Vision, Mission and Values', False), ('تاريخنا', 'Our History', False),
             ('الهيكل التنظيمي', 'Business Organization', False), ('خدماتنا', 'Our Services', False)]
STR['ar'].update({
    'S9_T1': 'أضف عنوانك', 'S9_T2': 'هنا', 'S9_B': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا.',
    'S10_T': 'معلومات أساسية عن الشركة', 'S10_B': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا.',
    'VIS': 'الرؤية', 'MIS': 'الرسالة', 'VAL': 'القيم', 'S11_B': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا.',
    'S12_T1': 'تاريخنا', 'S12_T2': 'ومسيرتنا', 'S12_B': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا.',
    'SAMPLE_t': 'نص توضيحي', 'S12_I': 'هذا نص توضيحي.<br>أضف النص المطلوب هنا.',
    'S13_T': 'الهيكل التنظيمي', 'S13_H': 'نص توضيحي.', 'S13_B2': 'هذا نص توضيحي.<br>أضف النص المطلوب هنا.', 'S13_B3': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
    'S14_T': 'خدماتنا', 'S14_B1': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا.', 'S14_B2': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي.',
})
STR['en'].update({
    'S9_T1': 'ADD YOUR', 'S9_T2': 'TITLE HERE', 'S9_B': 'This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here.',
    'S10_T': 'Basic Corporate Information', 'S10_B': 'This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here.',
    'VIS': 'Vision', 'MIS': 'Mission', 'VAL': 'Values', 'S11_B': 'This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here.',
    'S12_T1': 'Our', 'S12_T2': 'History', 'S12_B': 'This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here.',
    'SAMPLE_t': 'Sample text', 'S12_I': 'This is a sample text.<br>Insert your desired text here.',
    'S13_T': 'Business Organization', 'S13_H': 'Sample text.', 'S13_B2': 'This is a sample text.<br>Insert your desired text here.', 'S13_B3': 'This is a sample text. Insert your desired text here.',
    'S14_T': 'Our Services', 'S14_B1': 'This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here.', 'S14_B2': 'This is a sample text. Insert your desired text here. This is a sample text.',
})


# ======================================================== الشرائح 15–20
def svg(body, vb='0 0 24 24'):
    return '<svg viewBox="%s" fill="currentColor" aria-hidden="true">%s</svg>' % (vb, body)


ICO.update({
    'factory': svg('<path d="M2 21V9h3V3h3v6l5-3v3l5-3v3l4-2v14z" opacity=".001"/><path d="M3 21V10h3V4h2.5v8.5l4.5-2.6v2.6l4.5-2.6V21zM14 15h2v2h-2zm4 0h2v2h-2zM5 3h4v1H5z"/>'),
    'wheat': svg('<rect x="3" y="3" width="18" height="18" rx="2"/><path d="M12 18V8M12 11l-3-2M12 14l-3-2M12 11l3-2M12 14l3-2M12 8l-1.5-2.5M12 8l1.5-2.5M8 17l4 1 4-1" stroke="#262626" stroke-width="1.5" fill="none" stroke-linecap="round"/>'),
    'plant': svg('<path d="M12 4c2 1.5 2.5 4 1 6.5-1.6-1-2.2-3.3-1-6.5zM11.5 11C9 9 6 9.5 4.5 11c1.8 1.6 4.5 2 7 0zM12.5 11c2.5-2 5.5-1.5 7 0-1.8 1.6-4.5 2-7 0zM12 11v4"/><path d="M2 16h3l3 1h5a1.5 1.5 0 0 1 0 3H8v.5h6l6-3.5a1.4 1.4 0 0 1 1.8 2L15 22H5l-3-1.5z"/>'),
    'pour': svg('<path d="M4 3l9 2-1.5 5-8-2.5zM13 9h2v2h-2zM14 12.5l1.5 2.5a1.5 1.5 0 1 1-3 0z"/><path d="M3 15h4l3 1h4.5a1.4 1.4 0 0 1 0 2.8H10v.5h5l5-3a1.3 1.3 0 0 1 1.6 2L15.5 22H6l-3-1.5z"/>'),
    'barrel': svg('<path d="M5 2h14v2h-1v16h1v2H5v-2h1V4H5zM8 6v2h8V6zm0 10v2h8v-2z"/><path d="M12 9.5l2 3a2 2 0 1 1-4 0z" fill="#262626"/>'),
    'cosm': svg('<path d="M15 2h5v3h-5zM14.5 6h6v14a1.5 1.5 0 0 1-1.5 1.5h-3A1.5 1.5 0 0 1 14.5 20z"/><path d="M2.5 15.5L11 12l.8 2-8.5 3.5zM2 18h10v2.5A1.5 1.5 0 0 1 10.5 22h-7A1.5 1.5 0 0 1 2 20.5z"/>'),
    'chat': svg('<path d="M9 3a7 6 0 0 0-5.5 9.8L2.5 16l3.6-1.4A7 6 0 1 0 9 3z"/><path d="M16 9.5a6 5.2 0 0 1 4.9 8.3l.9 2.7-3.1-1.2A6 5.2 0 1 1 16 9.5z" stroke="#1D80F7" stroke-width="1.2"/>'),
    'team': svg('<circle cx="12" cy="9" r="2.6"/><path d="M7.5 18c0-3 2-4.6 4.5-4.6s4.5 1.6 4.5 4.6z"/><circle cx="4.5" cy="5" r="2"/><circle cx="19.5" cy="5" r="2"/><path d="M1.5 11c0-2.3 1.3-3.6 3-3.6s3 1.3 3 3.6zM16.5 11c0-2.3 1.3-3.6 3-3.6s3 1.3 3 3.6zM8 6h8v1.4H8z"/><rect x="6" y="18" width="12" height="2"/>'),
    'cloud': svg('<path d="M6.5 15a4 4 0 0 1-.3-8 5 5 0 0 1 9.4-1.4A4 4 0 1 1 17 15z"/><circle cx="15" cy="18" r="1.6"/><circle cx="18.5" cy="21" r="1.1"/>'),
    'bulb': svg('<path d="M12 6a5.5 5.5 0 0 0-3.2 10c.4.3.7.9.7 1.4V18h5v-.6c0-.5.3-1.1.7-1.4A5.5 5.5 0 0 0 12 6zM9.5 19h5v1.2h-5zM10.3 21h3.4v1.2h-3.4z"/><path d="M12 1v3M4 4l2 2M20 4l-2 2M1 11h3M20 11h3" stroke="currentColor" stroke-width="1.6" stroke-linecap="round"/>'),
    'star': svg('<path d="M12 2.5l2.9 6.2 6.6.8-4.9 4.6 1.3 6.6L12 17.4l-5.9 3.3 1.3-6.6-4.9-4.6 6.6-.8z" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linejoin="round"/>'),
    'heart': svg('<path d="M12 21C5 15.5 2 12.5 2 8.5A5 5 0 0 1 12 6a5 5 0 0 1 10 2.5c0 4-3 7-10 12.5z"/><path d="M3 12h5l1.5-3 2.5 6 2-4.5 1.2 1.5H21" fill="none" stroke="#D9D9D9" stroke-width="1.6" stroke-linejoin="round"/>'),
    'down': svg('<path d="M10 0h10v34h10L15 52 0 34h10z"/>', '0 0 30 52'),
    'up': svg('<path d="M10 52h10V18h10L15 0 0 18h10z"/>', '0 0 30 52'),
})
# 15 منتجاتنا
PI = [(203, 'factory', 'pour'), (299, 'wheat', 'barrel'), (395, 'plant', 'cosm')]
SL.append('\n    <img class="bgt" src="' + A + 'bg-city.jpg" alt="§ALT_CITY§">\n    <div class="lo"></div>\n    <h1 class="t">§S15_T§</h1>'
          + ''.join('\n    <div class="pi" style="top:%dpx; inset-inline-start:192px"><i>%s</i><p>§TOC§</p></div><div class="pi" style="top:%dpx; inset-inline-start:682px"><i>%s</i><p>§TOC§</p></div>' % (y - 36, ICO[a], y - 36, ICO[b]) for y, a, b in PI)
          + '\n    <p class="bd">§S15_B§</p>\n    <div class="light">' + ft(15) + '</div>\n  ')
# 16 الكفاءات الأساسية


def hexpts(cx, cy, r=78):
    h = r * 0.866
    return '%g,%g %g,%g %g,%g %g,%g %g,%g %g,%g' % (cx - r, cy, cx - r / 2, cy - h, cx + r / 2, cy - h, cx + r, cy, cx + r / 2, cy + h, cx - r / 2, cy + h)


HX = [(637, 247, '#1D80F7', 'chat', 'H_COM', ''), (510, 316, '#D9D9D9', 'team', 'H_TEAM', 'g'), (763, 316, '#D9D9D9', 'cloud', 'H_THK', 'g'),
      (510, 460, '#1D80F7', 'bulb', 'H_PS', ''), (763, 460, '#1D80F7', 'star', 'H_Q', ''), (637, 532, '#D9D9D9', 'heart', 'H_PSP', 'g')]
S16 = CITY + '\n    <h1 class="t">§S16_T§</h1>\n    <svg class="hx" viewBox="0 0 1280 720" aria-hidden="true"><g style="filter:drop-shadow(1px 4px 4px rgb(0 0 0 / .55))">'
S16 += ''.join('<polygon points="%s" fill="%s"/>' % (hexpts(x, y), c) for x, y, c, i, k, g in HX)
S16 += '<polygon points="%s" fill="#0B3F86"/></g></svg>' % hexpts(637, 390)
S16 += ''.join('\n    <div class="hl %s" style="top:%dpx; inset-inline-start:%dpx">%s<p>§%s§</p></div>' % (g, y - (40 if k in ('H_COM', 'H_PS', 'H_PSP') else 30), x - 75, ICO[i], k) for x, y, c, i, k, g in HX)
S16 += '\n    <div class="hl c" style="top:370px; inset-inline-start:562px">§H_CORE§</div>'
CS = [(178, 'COM'), (352, 'TEAM'), (526, 'PS')]
S16 += ''.join('\n    <div class="cs e" style="top:%dpx; inset-inline-start:68px"><h2>§C_%s§</h2><p>§TOC§</p></div>' % (y, k) for y, k in CS)
S16 += ''.join('\n    <div class="cs" style="top:%dpx; inset-inline-start:905px"><h2>§C_%s§</h2><p>§TOC§</p></div>' % (y, k) for y, k in ((178, 'THK'), (352, 'Q'), (526, 'PSP')))
SL.append(S16 + '\n    ' + ft(16) + '\n  ')
# 17 البيئة الخارجية
EC = [(830, 160, 'g', 'E_ECO'), (1005, 258, 'b', 'E_SOC'), (1013, 459, 'g', 'E_GLO'), (830, 563, 'b', 'E_TEC'), (660, 469, 'g', 'E_POL'), (652, 273, 'b', 'E_DEM')]
S17 = CITY + '\n    <h1 class="h">§S17_T§</h1>\n    <p class="bd">§S17_B§</p>'
S17 += '\n    <svg class="ring flipx" viewBox="0 0 1280 720" aria-hidden="true"><circle cx="830" cy="368" r="200" fill="none" stroke="rgb(255 255 255 / .28)" stroke-width="12"/>'
S17 += ''.join('<circle cx="%d" cy="%d" r="78" fill="%s"/>' % (x, y, 'rgb(255 255 255 / .22)' if g == 'g' else 'rgb(29 100 220 / .45)') for x, y, g, k in EC) + '</svg>'
S17 += ''.join('\n    <div class="ec %s" style="top:%dpx; inset-inline-start:%dpx; background:%s; box-shadow:0 2px 6px rgb(0 0 0 / .5)"><p>§%s§</p></div>' % (g, y - 66, x - 66, '#DCDCDC' if g == 'g' else '#1D80F7', k) for x, y, g, k in EC)
S17 += '\n    <p class="ct" style="top:285px">§E_IND§</p><div class="sep"></div><p class="ct" style="top:381px">§E_CMP§</p>'
SL.append(S17 + '\n    ' + ft(17) + '\n  ')
# 18 الاقتصاد
SL.append(CITY + '\n    <div class="ov"></div>\n    <h1 class="t">§S18_T§</h1>'
          '\n    <div class="kp">' + ICO['down'] + '<span>10%</span></div>\n    <p class="bd">§S18_B§</p>'
          '\n    <svg class="chart" id="ch18" viewBox="0 0 1280 720" aria-hidden="true"></svg>'
          '\n    <div class="call"><p>§S18_C1§</p><p>99</p><p>§SAMPLE_t§</p></div>\n    ' + ft(18) + '\n  ')
# 19 تحليل السوق
MC = [(305, 390, 212, '#1D80F7'), (305, 442, 160, '#A9CDFB'), (304, 494, 113, '#D6E8FD'), (305, 535, 67, '#FFFFFF')]
S19 = CITY + '\n    <h1 class="t">§S19_T§</h1>'
S19 += ''.join('\n    <div class="mc" style="top:%dpx; inset-inline-start:%dpx; width:%dpx; height:%dpx; background:%s"></div>' % (y - r, x - r, 2 * r, 2 * r, c) for x, y, r, c in MC)
S19 += '\n    <p class="ml" style="top:225px; color:#fff">§M_TAM§</p><p class="ml" style="top:316px">§M_SAM§</p><p class="ml" style="top:430px">§M_TGT§</p><p class="ml" style="top:523px">§M_SHR§</p>'
S19 += '\n    <div class="vl"></div><div class="hl2"></div>'
MQ = [(570, 169, 'Q_TAM', 'up'), (885, 169, 'Q_SAM', 'down'), (570, 423, 'Q_TGT', 'down'), (885, 423, 'Q_SHR', 'up')]
S19 += ''.join('\n    <div class="mq" style="top:%dpx; inset-inline-start:%dpx%s"><h2>§%s§</h2><div class="v">%s<span>10%%</span></div><p>§S13_B2§</p></div>' % (y, x, '' if k in ('Q_TAM', 'Q_SAM') else '; padding-top:0', k, ICO[ic]) for x, y, k, ic in MQ)
SL.append(S19 + '\n    ' + ft(19) + '\n  ')
# 20 تحليل المنافسين
PL = [(510, 143, 'g'), (228, 243, 'b'), (792, 243, 'b'), (228, 481, 'g'), (792, 481, 'g'), (510, 586, 'b')]
S20 = CITY + '\n    <h1 class="t">§S20_T§</h1>'
S20 += ('\n    <svg class="ln" viewBox="0 0 1280 720" aria-hidden="true"><g stroke="#9A9A9A" stroke-width="3">'
        '<line x1="638" y1="211" x2="638" y2="290"/><line x1="638" y1="505" x2="638" y2="586"/>'
        '<line x1="477" y1="303" x2="545" y2="340"/><line x1="800" y1="303" x2="732" y2="340"/>'
        '<line x1="477" y1="490" x2="545" y2="453"/><line x1="800" y1="490" x2="732" y2="453"/></g></svg>')
S20 += ''.join('\n    <div class="pill %s" style="top:%dpx; inset-inline-start:%dpx"><p>§S13_B2§</p></div>' % (g, y, x) for x, y, g in PL)
S20 += '\n    <div class="cc"><p>§S20_C§</p></div>'
SL.append(S20 + '\n    ' + ft(20) + '\n  ')
SECTIONS += [('منتجاتنا', 'Our Products', False), ('الكفاءات الأساسية', 'Core Competencies', False), ('البيئة الخارجية', 'External Environment', False),
             ('الاقتصاد (رسم)', 'The Economy (chart)', True), ('تحليل السوق', 'Market Analysis', True), ('تحليل المنافسين', 'Competitor Analysis', False)]
STR['ar'].update({
    'S15_T': 'منتجاتنا', 'S15_B': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا.',
    'S16_T': 'الكفاءات الأساسية', 'H_COM': 'التواصل', 'H_TEAM': 'العمل الجماعي', 'H_THK': 'التفكير', 'H_PS': 'حل<br>المشكلات', 'H_Q': 'الجودة', 'H_PSP': 'الروح<br>الشخصية', 'H_CORE': 'الكفاءات<br>الأساسية',
    'C_COM': 'التواصل', 'C_TEAM': 'العمل الجماعي', 'C_PS': 'حل المشكلات', 'C_THK': 'التفكير', 'C_Q': 'الجودة', 'C_PSP': 'الروح الشخصية',
    'S17_T': 'البيئة<br>الخارجية', 'S17_B': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا.',
    'E_ECO': 'اقتصادية', 'E_SOC': 'اجتماعية<br>وثقافية', 'E_GLO': 'عالمية', 'E_TEC': 'تقنية', 'E_POL': 'سياسية /<br>قانونية', 'E_DEM': 'سكانية',
    'E_IND': 'بيئة<br>الصناعة', 'E_CMP': 'البيئة<br>التنافسية',
    'S18_T': 'الاقتصاد', 'S18_B': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا.', 'S18_C1': 'نص توضيحي',
    'S19_T': 'تحليل السوق', 'M_TAM': 'إجمالي السوق<br>المتاح', 'M_SAM': 'السوق المتاح<br>القابل للخدمة', 'M_TGT': 'سوقك المستهدفة', 'M_SHR': 'حصتك<br>السوقية',
    'Q_TAM': 'إجمالي السوق<br>المتاح', 'Q_SAM': 'السوق المتاح<br>القابل للخدمة', 'Q_TGT': 'سوقك المستهدفة', 'Q_SHR': 'حصتك السوقية',
    'S20_T': 'تحليل المنافسين', 'S20_C': 'تحليل<br>المنافسين',
})
STR['en'].update({
    'S15_T': 'Our Products', 'S15_B': 'This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here.',
    'S16_T': 'Core Competencies', 'H_COM': 'Communi-<br>cations', 'H_TEAM': 'Teamwork', 'H_THK': 'Thinking', 'H_PS': 'Problem<br>Solving', 'H_Q': 'Quality', 'H_PSP': 'Personal<br>Spirit', 'H_CORE': 'Core<br>Competencies',
    'C_COM': 'Communications', 'C_TEAM': 'Teamwork', 'C_PS': 'Problem Solving', 'C_THK': 'Thinking', 'C_Q': 'Quality', 'C_PSP': 'Personal Spirit',
    'S17_T': 'External<br>Environment', 'S17_B': 'This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here.',
    'E_ECO': 'Economic', 'E_SOC': 'Sociocultural', 'E_GLO': 'Global', 'E_TEC': 'Technol-<br>ogical', 'E_POL': 'Political /<br>Legal', 'E_DEM': 'Demographic',
    'E_IND': 'Industry<br>Environment', 'E_CMP': 'Competitive<br>Environment',
    'S18_T': 'The Economy', 'S18_B': 'This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here.', 'S18_C1': 'Sample Text',
    'S19_T': 'Market Analysis', 'M_TAM': 'Total Available Market<br>( TAM )', 'M_SAM': 'Serviceable Available<br>Market ( SAM )', 'M_TGT': 'Your Target Market', 'M_SHR': 'Your Market<br>Share',
    'Q_TAM': 'Total Available Market<br>( TAM )', 'Q_SAM': 'Serviceable Available<br>Market ( SAM )', 'Q_TGT': 'Your Target Market', 'Q_SHR': 'Your Market Share',
    'S20_T': 'Competitor Analysis', 'S20_C': 'COMPETITOR<br>ANALYSIS',
})
# ======================================================== الشرائح 21–24
ICO.update({
    'target': svg('<circle cx="11" cy="13" r="9" fill="none" stroke="currentColor" stroke-width="2.4"/><circle cx="11" cy="13" r="4.6" fill="none" stroke="currentColor" stroke-width="2.4"/><circle cx="11" cy="13" r="1.6"/><path d="M11 13L20 4M17 2v4h4" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>'),
    'binoc': svg('<rect x="2" y="5" width="8" height="5" rx="1.2" transform="rotate(15 6 7.5)"/><rect x="11" y="12" width="9" height="6" rx="1.2"/><rect x="4" y="9.5" width="9" height="6" rx="1.2"/><rect x="13" y="7" width="8" height="5" rx="1.2" transform="rotate(15 17 9.5)"/>'),
    'shake': svg('<path d="M2 9l5-3 4 2 3-1 4 1 4 4-3 3-2-1-1.5 1.5-2-1-1.5 1.5-2-1L8 18l-3-3 1-2-2-1z"/><path d="M8 12l4 4M10 11l4 4M12 10l4 4" stroke="#1D80F7" stroke-width="1.2"/>'),
    'clip': svg('<rect x="4" y="3.5" width="14" height="18" rx="1.5" fill="none" stroke="currentColor" stroke-width="2"/><rect x="8" y="2" width="6" height="3.5" rx="1"/><path d="M7 9h2M11 9h4M7 13h2M11 13h4M7 17h2" stroke="currentColor" stroke-width="1.8"/><circle cx="18" cy="18" r="4.5"/><path d="M15.8 18l1.5 1.5 2.7-3" stroke="#1D80F7" stroke-width="1.5" fill="none"/>'),
    'trophy': svg('<path d="M6 2h12v6a6 6 0 0 1-12 0z"/><path d="M6 4H2.5v2.5A4 4 0 0 0 6.5 10.5M18 4h3.5v2.5a4 4 0 0 1-4 4" fill="none" stroke="currentColor" stroke-width="1.8"/><path d="M11 14h2v3h-2zM8 17h8v2.5H8zM7 20h10v2H7z"/>'),
})
# 21 المزايا التنافسية
CQ = [(467, 234, 246, 203, '#1D80F7', '#fff', 'Q_CL'), (726, 234, 245, 203, '#0A5FC8', '#fff', 'Q_DF'), (467, 453, 246, 202, '#A9CDFB', '#444', 'Q_CF'), (726, 453, 245, 202, '#D6E8FD', '#444', 'Q_DFF')]
S21 = CITY + '\n    <h1 class="t">§S21_T§</h1>\n    <div class="hb">§S21_H§</div>\n    <div class="vb"><span>§S21_V§</span></div>'
S21 += '\n    <p class="cl" style="inset-inline-start:467px">§S21_LC§</p><p class="cl" style="inset-inline-start:726px">§Q_DF§</p>'
S21 += '\n    <p class="rl" style="top:234px">§S21_BT§</p><p class="rl" style="top:605px">§S21_NT§</p>'
S21 += ''.join('\n    <div class="cq" style="top:%dpx; inset-inline-start:%dpx; width:%dpx; height:%dpx; background:%s; color:%s"><p>§%s§</p></div>' % (y, x, w, h, bg, fg, k) for x, y, w, h, bg, fg, k in CQ)
SL.append(S21 + '\n    ' + ft(21) + '\n  ')
# 22 الاستراتيجية والخطط
STEP = [(233, 584, 695, 'target', 'SP1', 550, 747), (310, 549, 731, 'binoc', 'SP2', 515, 780), (386, 507, 773, 'shake', 'SP3', 473, 818),
        (464, 457, 823, 'clip', 'SP4', 423, 861), (541, 408, 872, 'trophy', 'SP5', 374, 927)]
S22 = CITY + '\n    <h1 class="t">§S22_T§</h1>\n    <p class="ax" style="top:135px">§S22_S§</p><p class="ax" style="top:609px">§S22_TA§</p>'
S22 += '\n    <svg class="st" viewBox="0 0 1280 720" aria-hidden="true"><g fill="#0B3F86">'
for (y, x0, x1, ic, k, nx, tx), nxt in zip(STEP, STEP[1:] + [None]):
    if nxt:
        S22 += '<polygon points="%d,%d %d,%d %d,%d %d,%d"/>' % (x0, y + 22, x1, y + 22, nxt[2], nxt[0] - 25, nxt[1], nxt[0] - 25)
S22 += '</g><g fill="#1D80F7" style="filter:drop-shadow(0 2px 3px rgb(0 0 0 / .4))">'
S22 += ''.join('<rect x="%d" y="%d" width="%d" height="%d"/>' % (x0, y - 26, x1 - x0, 50) for y, x0, x1, ic, k, nx, tx in STEP)
S22 += '</g><g stroke="#1D80F7" stroke-width="1.5" fill="#1D80F7"><line x1="640" y1="186" x2="640" y2="585"/><polygon points="634,188 640,177 646,188"/><polygon points="634,584 640,595 646,584"/></g></svg>'
S22 += ''.join('\n    <div class="sb" style="top:%dpx; width:%dpx"><span>§%s§</span><span>%d</span></div>' % (y - 26, x0 - 163 - 4, k, n + 1) for n, (y, x0, x1, ic, k, nx, tx) in enumerate(STEP))
S22 += ''.join('\n    <div class="sx" style="top:%dpx; inset-inline-start:%dpx; width:%dpx; height:50px">%s</div>' % (y - 26, x0, x1 - x0, ICO[ic]) for y, x0, x1, ic, k, nx, tx in STEP)
S22 += ''.join('\n    <p class="sd" style="top:%dpx; inset-inline-start:%dpx">§S13_B2§</p>' % (y - 22, tx) for y, x0, x1, ic, k, nx, tx in STEP)
SL.append(S22 + '\n    ' + ft(22) + '\n  ')
# 23 الأهداف
S23 = CITY + '\n    <h1 class="t">§S23_T§</h1>'
S23 += ('\n    <svg class="tg flipx" viewBox="195 180 380 430" aria-hidden="true">'
        '<rect x="312" y="500" width="36" height="102" rx="18" fill="#C4C7CB" transform="rotate(5 330 550)"/><rect x="415" y="500" width="36" height="102" rx="18" fill="#C4C7CB" transform="rotate(-5 433 550)"/>'
        '<g style="filter:drop-shadow(0 3px 5px rgb(0 0 0 / .45))"><circle cx="382" cy="366" r="183" fill="#1D80F7"/></g><circle cx="382" cy="366" r="135" fill="#A9CDFB"/>'
        '<circle cx="382" cy="366" r="89" fill="#0A5FC8"/><circle cx="382" cy="366" r="45" fill="#A9CDFB"/></svg>')
OB = [(216, '#0A5FC8', '#fff'), (333, '#1D80F7', '#fff'), (451, '#78B4F5', '#fff'), (569, '#B5D6FA', '#3A4552')]
S23 += ''.join('\n    <div class="on" style="top:%dpx; background:%s; color:%s">%02d</div><div class="ob" style="top:%dpx"><h2>§S13_H§</h2><p>§S13_B2§</p></div>' % (y - 43, bg, fg, i + 1, y - 40) for i, (y, bg, fg) in enumerate(OB))
SL.append(S23 + '\n    ' + ft(23) + '\n  ')
# 24 استراتيجية الأعمال
LA = '#D6E8FD'
S24 = CITY + '\n    <h1 class="t">§S24_T§</h1>\n    <div class="bx">§S24_T§</div>'
S24 += ('\n    <svg class="ar flipx" viewBox="0 0 1280 720" aria-hidden="true">'
        '<g fill="%s">' % LA
        + ''.join('<polygon points="%d,218 %d,218 %d,231 %d,231 %d,242 %d,231 %d,231"/>' % (x - 6, x + 6, x + 6, x + 14, x, x - 14, x - 6) for x in (627, 836, 1053))
        + '<polygon points="399,262 428,262 428,250 455,273 428,297 428,285 399,285"/>'
        + '<polygon points="215,348 228,337 240,350 245,345 247,370 222,368 227,363"/><polygon points="272,346 286,346 286,364 296,364 279,383 262,364 272,364"/><polygon points="325,337 338,348 333,353 341,368 316,370 318,345 323,350" transform="translate(0 0)"/>'
        + '</g><path d="M1012 545 C860 598 650 600 467 585 L473 540 L375 552 L447 655 L455 618 C700 640 900 602 1012 545 Z" fill="none" stroke="#2F7BF0" stroke-width="1.5"/>'
        '</svg>')
S24 += '\n    <div class="bc" style="inset-inline-start:528px; background:rgb(13 52 112 / .85)"><h2>§S24_C1§</h2><p>§S24_B§</p></div>'
S24 += '\n    <div class="bc" style="inset-inline-start:744px; background:rgb(115 135 165 / .55)"><h2>§S24_C2§</h2><p>§S24_B§</p></div>'
S24 += '\n    <div class="bc" style="inset-inline-start:960px; background:rgb(16 62 135 / .85)"><h2>§S24_C3§</h2><p>§S24_B§</p></div>'
S24 += ('\n    <svg class="ar flipx" viewBox="0 0 1280 720" aria-hidden="true"><g fill="%s">' % LA
        + ''.join('<polygon points="%d,486 %d,476 %d,482 %d,482 %d,476 %d,486 %d,496 %d,490 %d,490 %d,496"/>' % (x, x + 14, x + 14, x + 50, x + 50, x + 64, x + 50, x + 50, x + 14, x + 14) for x in (705, 920))
        + '</g></svg>')
S24 += '\n    <div class="sp"><p>§S24_SP§</p></div>'
S24 += '\n    <div class="ci" style="top:382px; inset-inline-start:121px; width:120px; height:122px; background:#0E3F86"><p>§SAMPLE_tt§</p></div>'
S24 += '\n    <div class="ci" style="top:390px; inset-inline-start:226px; width:108px; height:107px; background:rgb(110 132 165 / .85)"><p>§SAMPLE_tt§</p></div>'
S24 += '\n    <div class="ci" style="top:380px; inset-inline-start:314px; width:128px; height:127px; background:rgb(22 76 150 / .92)"><p>§SAMPLE_tt§</p></div>'
S24 += '\n    <p class="it">§S24_IT§</p>'
SL.append(S24 + '\n    ' + ft(24) + '\n  ')
SECTIONS += [('المزايا التنافسية', 'Competitive Advantages', False), ('الاستراتيجية والخطط', 'Strategy and Plans', False),
             ('الأهداف', 'Objectives', False), ('استراتيجية الأعمال', 'Business Strategy', False)]
STR['ar'].update({
    'S21_T': 'المزايا التنافسية', 'S21_H': 'الميزة التنافسية', 'S21_V': 'نطاق المنافسة', 'S21_LC': 'تكلفة أقل', 'S21_BT': 'نطاق<br>واسع', 'S21_NT': 'نطاق<br>ضيق',
    'Q_CL': 'ريادة<br>التكلفة', 'Q_DF': 'التمايز', 'Q_CF': 'التركيز على التكلفة', 'Q_DFF': 'التركيز على<br>التمايز',
    'S22_T': 'الاستراتيجية والخطط', 'S22_S': 'استراتيجي', 'S22_TA': 'تكتيكي', 'SP1': 'أهداف العمل', 'SP2': 'مجالات التركيز', 'SP3': 'المبادرات الرئيسية', 'SP4': 'خطط المشاريع', 'SP5': 'التنفيذ',
    'S23_T': 'الأهداف',
    'S24_T': 'استراتيجية الأعمال', 'S24_C1': 'تطوير<br>نموذج عمل', 'S24_C2': 'تنفيذ<br>الاستراتيجية', 'S24_C3': 'المراجعة', 'S24_B': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
    'S24_SP': 'خطة<br>الاستراتيجية', 'SAMPLE_tt': 'نص<br>توضيحي', 'S24_IT': 'التكرار والتحسين',
})
STR['en'].update({
    'S21_T': 'Competitive Advantages', 'S21_H': 'Competitive Advantage', 'S21_V': 'Competitive Edge', 'S21_LC': 'Lower Cost', 'S21_BT': 'Broad<br>Target', 'S21_NT': 'Narrow<br>Target',
    'Q_CL': 'Cost<br>Leadership', 'Q_DF': 'Differentiation', 'Q_CF': 'Cost Focus', 'Q_DFF': 'Differentiation<br>Focus',
    'S22_T': 'Strategy and Plans', 'S22_S': 'Strategic', 'S22_TA': 'Tactical', 'SP1': 'Business Goals', 'SP2': 'Focus Areas', 'SP3': 'Major Initiatives', 'SP4': 'Project Plans', 'SP5': 'Execution',
    'S23_T': 'Objectives',
    'S24_T': 'Business Strategy', 'S24_C1': 'Develop a<br>Working Model', 'S24_C2': 'Execute The<br>Strategy', 'S24_C3': 'Review', 'S24_B': 'This is a sample text. Insert your desired text here.',
    'S24_SP': 'Strategy<br>Plan', 'SAMPLE_tt': 'Sample<br>text', 'S24_IT': 'Iterate and Refine',
})

# ======================================================== الشرائح 25–26
ICO.update({
    'bulb2': svg('<path d="M12 4a5 5 0 0 0-3 9c.4.3.6.8.6 1.3V15h4.8v-.7c0-.5.2-1 .6-1.3a5 5 0 0 0-3-9zM9.6 16h4.8v1.4H9.6zM10.4 18.4h3.2V20h-3.2z"/><path d="M12 0v2M4 4l1.4 1.4M20 4l-1.4 1.4" stroke="currentColor" stroke-width="1.5"/>'),
    'money': svg('<rect x="3" y="6" width="18" height="11" rx="1" transform="rotate(-30 12 11.5)"/><circle cx="12" cy="11.5" r="2.6" fill="#1D80F7"/>'),
    'link': svg('<path d="M10 14l4-4M8.5 11.5L6 14a3 3 0 0 0 4 4l2.5-2.5M15.5 12.5L18 10a3 3 0 0 0-4-4l-2.5 2.5" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round"/>'),
    'growth': svg('<path d="M3 21h3v-6H3zM8 21h3v-9H8zM13 21h3v-7h-3zM18 21h3V9h-3z"/><path d="M3 12l5-5 4 3 7-7M15 3h4v4" fill="none" stroke="currentColor" stroke-width="1.8"/>'),
    'gears2': svg('<path d="M9 3h2l.4 1.8 1.4.6 1.6-1 1.4 1.4-1 1.6.6 1.4L17 9v2l-1.8.4-.6 1.4 1 1.6-1.4 1.4-1.6-1-1.4.6L11 17H9l-.4-1.8-1.4-.6-1.6 1L4.2 14.2l1-1.6-.6-1.4L3 11V9l1.8-.4.6-1.4-1-1.6L5.8 4.2l1.6 1 1.4-.6zm1 4.5a2.5 2.5 0 1 0 0 5 2.5 2.5 0 0 0 0-5z"/><circle cx="18" cy="18" r="3.5"/><circle cx="18" cy="18" r="1.3" fill="#1D80F7"/>'),
    'search': svg('<circle cx="10" cy="10" r="6.5" fill="none" stroke="currentColor" stroke-width="2.6"/><path d="M15 15l6 6" stroke="currentColor" stroke-width="3.2" stroke-linecap="round"/>'),
})
# 25 خطة التشغيل
COIL = ('M632,142 L590,165 A43,43 0 0,0 590,251 L680,197 A43,43 0 0,1 680,283 L590,329 A43,43 0 0,0 590,415 '
        'L680,360 A43,43 0 0,1 680,446 L590,492 A43,43 0 0,0 590,578 L680,520 A43,43 0 0,1 680,606 L632,632')
DARK = 'M632,142 L590,165 A43,43 0 0,0 590,251 L622,232 M640,302 L590,329 A43,43 0 0,0 590,415 L622,396 M640,467 L590,492 A43,43 0 0,0 590,578 L622,559'
S25 = CITY + '\n    <h1 class="t">§S25_T§</h1>'
S25 += ('\n    <svg class="cv flipx" viewBox="0 0 1280 720" aria-hidden="true"><defs><marker id="ah" viewBox="0 0 8 8" refX="4" refY="4" markerWidth="7" markerHeight="7" orient="auto-start-reverse"><path d="M0 0 8 4 0 8z" fill="#2F7BF0"/></marker></defs>'
        '<g stroke="#2F7BF0" stroke-width="1.3" stroke-dasharray="4 3" marker-end="url(#ah)">'
        '<line x1="545" y1="208" x2="68" y2="208"/><line x1="545" y1="372" x2="243" y2="372"/><line x1="545" y1="535" x2="68" y2="535"/>'
        '<line x1="725" y1="238" x2="1213" y2="238"/><line x1="725" y1="400" x2="1041" y2="400"/><line x1="725" y1="561" x2="1213" y2="561"/></g>'
        '<path d="%s" fill="none" stroke="#7DB6F6" stroke-width="14" stroke-linejoin="round"/><path d="%s" fill="none" stroke="#1D80F7" stroke-width="14" stroke-linejoin="round"/>' % (COIL, DARK)
        + ''.join('<circle cx="%d" cy="%d" r="21" fill="#1D80F7" stroke="#0D2C5E" stroke-width="4"/>' % (x, y) for x, y in ((590, 208), (680, 240), (590, 372), (680, 403), (590, 535), (680, 563)))
        + '</svg>')
S25 += ''.join('\n    <div class="ic2" style="top:%dpx; inset-inline-start:%dpx">%s</div>' % (y - 22, x - 22, ICO[i]) for x, y, i in ((590, 208, 'bulb2'), (680, 240, 'money'), (590, 372, 'link'), (680, 403, 'growth'), (590, 535, 'gears2'), (680, 563, 'search')))
S25 += ''.join('\n    <p class="yr2" style="top:%dpx; inset-inline-start:%dpx">%s</p>' % (y - 18, x, t) for x, y, t in ((64, 168, '2017'), (242, 332, '2019'), (64, 497, '2021')))
S25 += ''.join('\n    <p class="yr2" style="top:%dpx; inset-inline-end:%dpx">%s</p>' % (y - 18, 1280 - x, t) for x, y, t in ((1217, 200, '2018'), (1043, 362, '2020'), (1217, 523, '2022')))
S25 += ''.join('\n    <div class="oi" style="top:%dpx; inset-inline-start:%dpx"><h2>§SAMPLE_t§</h2><p>§S13_B2§</p></div>' % (y - 13, x) for x, y in ((64, 238), (340, 398), (62, 564)))
S25 += ''.join('\n    <div class="oi e" style="top:%dpx; inset-inline-end:%dpx"><h2>§SAMPLE_t§</h2><p>§S13_B2§</p></div>' % (y - 13, 1280 - x) for x, y in ((1212, 269), (1042, 431), (1212, 591)))
SL.append(S25 + '\n    ' + ft(25) + '\n  ')
# 26 التحليل المالي
FROWS = ['99.00', '999.90', '9.00', '99.90', '999.90', '99.00', '99.00', '9.99']
S26 = CITY + '\n    <h1 class="t">§S26_T§</h1>\n    <svg class="chart" id="ch26" viewBox="0 0 1280 720" aria-hidden="true"></svg>\n    <p class="bd">§S26_B§</p>'
S26 += '\n    <table class="ft2"><tr><td>§S26_P§</td><td>§S26_R§</td></tr>' + ''.join('<tr><td>§SAMPLE_T§</td><td>%s</td></tr>' % v for v in FROWS) + '<tr><td>§TOTAL§</td><td>9999.90</td></tr></table>'
SL.append(S26 + '\n    ' + ft(26) + '\n  ')
JSON_BLOCKS_LATE = {'chart-data-2': {'categories': ['1', '2', '3', '4', '5', '6'], 'values': [4.3, 2.5, 1.5, 2, 3, 6], 'max': 7, 'step': 1}}
SECTIONS += [('خطة التشغيل', 'Operating Plan', False), ('التحليل المالي (رسم وجدول)', 'Financial Analysis (chart & table)', True)]
STR['ar'].update({'S25_T': 'خطة التشغيل', 'S26_T': 'التحليل المالي', 'S26_B': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي.',
                  'S26_P': 'الفترة', 'S26_R': 'الإيرادات بالدولار', 'TOTAL': 'الإجمالي'})
STR['en'].update({'S25_T': 'Operating Plan', 'S26_T': 'Financial Analysis', 'S26_B': 'This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text.',
                  'S26_P': 'Period', 'S26_R': 'Revenue, US$', 'TOTAL': 'Total'})

# ======================================================== الشرائح 27–30
S27 = CITY + '\n    <h1 class="t">§S27_T§</h1>'
S27 += ('\n    <svg class="be flipx" viewBox="0 0 1280 720" aria-hidden="true">'
        '<rect x="127" y="457" width="767" height="143" fill="rgb(28 84 170 / .55)"/>'
        '<g stroke="#C9CDD2" stroke-width="1.5" fill="#C9CDD2"><line x1="127" y1="600" x2="127" y2="180"/><polygon points="121,183 127,170 133,183"/><line x1="127" y1="600" x2="884" y2="600"/><polygon points="882,594 895,600 882,606"/>'
        '<line x1="522" y1="306" x2="522" y2="334"/><polygon points="516,332 522,344 528,332"/></g>'
        '<g stroke="#2F80F0" stroke-width="3"><line x1="127" y1="457" x2="895" y2="305"/><line x1="127" y1="600" x2="897" y2="172"/></g><circle cx="522" cy="378" r="9" fill="#2F80F0"/>'
        '<g fill="none" stroke="#C9CDD2" stroke-width="1" stroke-dasharray="3 2">'
        + ''.join('<path d="M905 %d q12 0 12 12 v%d q0 10 8 %d q-8 0 -8 10 v%d q0 12 -12 12"/>' % (a, (b - a) / 2 - 22, 0, (b - a) / 2 - 22) for a, b in ((172, 305), (305, 457), (457, 600)))
        + '</g></svg>')
S27 += '\n    <div class="bc2"><p>§S27_BE§</p></div>\n    <p class="lb cost">§S27_C§</p><p class="lb qty">§S27_Q§</p>'
S27 += ''.join('\n    <div class="bi" style="top:%dpx"><h2>§%s§</h2><p>§S13_B2§</p></div>' % (y, k) for y, k in ((201, 'S27_P'), (344, 'S27_V'), (492, 'S27_F')))
SL.append(S27 + '\n    ' + ft(27) + '\n  ')
# 28 توقعات المبيعات
S28 = CITY + '\n    <h1 class="t">§S28_T§</h1>\n    <svg class="chart" id="ch28" viewBox="0 0 1280 720" aria-hidden="true"></svg>'
S28 += ('\n    <div class="pn"></div>\n    <svg class="dn flipx" viewBox="0 0 200 200" aria-hidden="true"><circle cx="100" cy="100" r="90" fill="none" stroke="#7DB6F6" stroke-width="20"/>'
        '<circle cx="100" cy="100" r="90" fill="none" stroke="#0A5FC8" stroke-width="20" stroke-dasharray="396 566" transform="rotate(-90 100 100)"/></svg>'
        '\n    <p class="pc">99<small>%</small></p>\n    <h2 class="h">§SAMPLE_t§</h2>\n    <p class="bd">§S28_B§</p>')
SL.append(S28 + '\n    ' + ft(28) + '\n  ')
# 29 الإنفاق الرأسمالي
S29 = CITY + '\n    <h1 class="t">§S29_T§</h1>\n    <div class="pnl" style="top:128px; inset-inline-start:61px; width:739px; height:512px"></div>\n    <svg class="chart" id="ch29" viewBox="0 0 1280 720" aria-hidden="true"></svg>'
S29 += ''.join('\n    <div class="yc" style="top:%dpx; inset-inline-start:%dpx"><h2>%s</h2><div class="rg">99%%</div><p>§S29_B§</p></div>' % (y, x, yr) for x, y, yr in ((810, 128, '2017'), (1018, 128, '2018'), (810, 388, '2019'), (1018, 388, '2020')))
SL.append(S29 + '\n    ' + ft(29) + '\n  ')
# 30 تكاليف التشغيل
STK = [(827, ['#C9D9F0'] * 1 + ['#1D80F7'] * 6, '82%'), (920, ['#C9D9F0'] * 3 + ['#1E4A85'] * 4, '32%'), (1015, ['#C9D9F0'] * 6 + ['#F98226'], '14%'), (1108, ['#C9D9F0'] * 5 + ['#46566A'] * 2, '12%')]
S30 = CITY + '\n    <h1 class="t">§S30_T§</h1>\n    <div class="pnl" style="top:128px; inset-inline-start:61px; width:603px; height:512px"></div>\n    <svg class="chart" id="ch30" viewBox="0 0 1280 720" aria-hidden="true"></svg>'
S30 += '\n    <div class="bxo"></div><p class="pt">§S30_P§</p>\n    <svg class="stk flipx" viewBox="0 0 1280 720" aria-hidden="true">'
S30 += ''.join('<rect x="%d" y="%d" width="50" height="8" rx="3" fill="%s"/>' % (x - 25, 240 + 14 * j, c) for x, cols, pct in STK for j, c in enumerate(cols)) + '</svg>'
S30 += ''.join('\n    <div class="pr" style="inset-inline-start:%dpx">%s</div>' % (x - 31, pct) for x, cols, pct in STK)
S30 += '\n    <h2 class="h">§SAMPLE_t§</h2>\n    <p class="bd">§S30_B§</p>'
S30 += ''.join('\n    <div class="lg" style="top:%dpx; inset-inline-start:%dpx"><i style="background:%s"></i><span>§OPT%d§</span></div>' % (y, x, c, n) for x, y, c, n in ((143, 522, '#1D80F7', 1), (143, 569, '#D9D9D9', 2), (374, 522, '#F98226', 3), (374, 569, '#1E4A85', 4)))
SL.append(S30 + '\n    ' + ft(30) + '\n  ')
JSON_BLOCKS_LATE.update({
    'chart-data-3': {'categories': ['2017', '2018', '2019', '2020', '2021', '2022'], 'values': [0.5, 1, 2, 3, 4, 5], 'max': 6, 'step': 1,
                     'colors': ['#D6E8FD', '#A9CDFB', '#78B4F5', '#1D80F7', '#0A5FC8', '#0B3F86']},
    'chart-data-4': {'categories': ['2017', '2018', '2019', '2020'], 'series': [[5000, 4000, 4000, 3000], [8000, 6000, 7000, 9000], [3000, 7000, 7000, 5000]],
                     'colors': ['#2489FF', '#0F67D6', '#0A4A9A'], 'max': 10000, 'step': 1000},
    'chart-data-5': {'ar': {'labels': ['الخيار 1', 'الخيار 4', 'الخيار 3', 'الخيار 2'], 'values': [60, 22, 10, 8], 'colors': ['#1D80F7', '#1E4A85', '#F98226', '#4A5868']},
                     'en': {'labels': ['Option 1', 'Option 4', 'Option 3', 'Option 2'], 'values': [60, 22, 10, 8], 'colors': ['#1D80F7', '#1E4A85', '#F98226', '#4A5868']}},
})
SECTIONS += [('تحليل التعادل', 'Break-Even Analysis', False), ('توقعات المبيعات (رسم)', 'Sales Projections (chart)', True),
             ('الإنفاق الرأسمالي (رسم)', 'Capital Spending (chart)', True), ('تكاليف التشغيل (رسم)', 'Operating Costs (chart)', True)]
STR['ar'].update({
    'S27_T': 'تحليل التعادل', 'S27_BE': 'نقطة<br>التعادل', 'S27_C': 'التكلفة', 'S27_Q': 'الكمية', 'S27_P': 'الربح', 'S27_V': 'إجمالي التكاليف المتغيرة', 'S27_F': 'التكاليف الثابتة',
    'S28_T': 'توقعات المبيعات', 'S28_B': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا.',
    'S29_T': 'الإنفاق الرأسمالي', 'S29_B': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
    'S30_T': 'تكاليف التشغيل', 'S30_P': 'النسبة المئوية', 'S30_B': 'خلافًا للاعتقاد السائد، هذا النص ليس كلامًا عشوائيًا، بل له جذور في نص أدبي كلاسيكي يعود إلى عام 45 قبل الميلاد.',
    'OPT1': 'الخيار 1', 'OPT2': 'الخيار 2', 'OPT3': 'الخيار 3', 'OPT4': 'الخيار 4',
})
STR['en'].update({
    'S27_T': 'Break-Even Analysis', 'S27_BE': 'Break<br>Even', 'S27_C': 'Cost', 'S27_Q': 'Quantity', 'S27_P': 'Profit', 'S27_V': 'Total Variable Costs', 'S27_F': 'Fixed Costs',
    'S28_T': 'Sales Projections', 'S28_B': 'This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here.',
    'S29_T': 'Capital Spending', 'S29_B': 'This is a sample text. Insert your desired text here.',
    'S30_T': 'Operating Costs', 'S30_P': 'Percentage', 'S30_B': 'Contrary to popular belief, Lorem Ipsum is not simply random text. It has roots in a piece of classical Latin literature from 45 BC.',
    'OPT1': 'Option 1', 'OPT2': 'Option 2', 'OPT3': 'Option 3', 'OPT4': 'Option 4',
})

# ======================================================== الشرائح 31–36
def table(x, y, widths, rows, rh, cls_rows=None, extra=''):
    cls_rows = cls_rows or {}
    h = '<table class="tb" style="top:%dpx; inset-inline-start:%dpx; width:%dpx%s"><colgroup>%s</colgroup>' % (y, x, sum(widths), extra, ''.join('<col style="width:%dpx">' % w for w in widths))
    for i, r in enumerate(rows):
        c = cls_rows.get(i, '')
        hh = rh[i] if isinstance(rh, list) else rh
        h += '<tr class="%s" style="height:%spx">%s</tr>' % (c, hh, ''.join(cell if cell.startswith('<td') else '<td>%s</td>' % cell for cell in r))
    return h + '</table>'


ICO['dollar_up'] = svg('<circle cx="12" cy="12" r="12"/><path d="M12 5v14M15.5 8.2c-.8-1-2-1.5-3.5-1.5-2 0-3.3 1-3.3 2.5 0 3.5 7 2 7 5.6 0 1.6-1.5 2.7-3.6 2.7-1.6 0-3-.6-3.8-1.7" fill="none" stroke="#262626" stroke-width="2"/><path d="M17.5 21v-5.5M15 18l2.5-3 2.5 3" fill="none" stroke="#fff" stroke-width="1.6"/>')
ICO['dollar_dn'] = svg('<circle cx="12" cy="12" r="12"/><path d="M12 5v14M15.5 8.2c-.8-1-2-1.5-3.5-1.5-2 0-3.3 1-3.3 2.5 0 3.5 7 2 7 5.6 0 1.6-1.5 2.7-3.6 2.7-1.6 0-3-.6-3.8-1.7" fill="none" stroke="#262626" stroke-width="2"/><path d="M17.5 15v5.5M15 18l2.5 3 2.5-3" fill="none" stroke="#fff" stroke-width="1.6"/>')
INC = ['$99.99', '$999.99', '$9.99', '$99,999.99', '$99.99', '$99,999.99', '$9.99', '$99.99', '$99.99', '$99,999.99', '$9.99']
EXP = ['$99.99', '$999.99', '$9.99', '$99,999.99', '$99.99', '$99,999.99', '$9.99', '$99.99', '$99.99']
S31 = CITY + '\n    <h1 class="t">§S31_T§</h1>'
S31 += '\n    <h2 class="lh" style="inset-inline-start:63px">§S31_I§</h2><div class="cn" style="inset-inline-start:576px">' + ICO['dollar_up'] + '</div>'
S31 += '<h2 class="lh" style="inset-inline-start:665px">§S31_E§</h2><div class="cn" style="inset-inline-start:1176px">' + ICO['dollar_dn'] + '</div>'
S31 += '\n    ' + table(63, 176, [374, 178], [['§SAMPLE_T§', v] for v in INC] + [['', ''], ['§S31_TI§', '$99,999.99']], [33.7] * 12 + [41], {12: 'tot'})
S31 += '\n    ' + table(665, 176, [374, 178], [['§SAMPLE_T§', v] for v in EXP] + [['', ''], ['§S31_TE§', '$9,999.99']], [33.7] * 10 + [41], {10: 'tot'})
S31 += '\n    <p class="ot"><span>§S31_OT§</span><b>$9,999.99</b></p>'
SL.append(S31 + '\n    ' + ft(31) + '\n  ')
# 32 الميزانية العمومية
BS = [('CASH', '$99.99', '$9.99'), ('AR', '$999.99', '$99,999.99'), ('SUP', '$9.99', '$99.99'), ('PRE', '$99,999.99', '$9.99'), ('EQP', '$99.99', '$99,999.99'),
      ('AP', '$99,999.99', '$99,999.99'), ('OC', '$9.99', '$99.99'), ('OD', '$99.99', '$99,999.99'), ('SAL', '$99.99', '$9.99'), ('SE', '$99,999.99', '$9.99'), ('MSC', '$9.99', '$9.99')]
S32 = CITY + '\n    <h1 class="t">§S32_T§</h1>'
S32 += '\n    ' + table(63, 120, [673, 249, 232], [['', '<td class="c">§DEBIT§</td>', '<td class="c">§CREDIT§</td>']] + [['§BS_%s§' % k, a, b] for k, a, b in BS] + [['', '', ''], ['', '', ''], ['§S32_TA§', '$99,999.99', '$99,999.99']],
                          [37] + [33.7] * 13 + [41], {0: 'hd', 14: 'tot'}, '; box-shadow:none')
SL.append(S32 + '\n    ' + ft(32) + '\n  ')
# 33 التدفق النقدي
CF = [('§CF_NI§', '$99.99', '$9.99', ''), ('§SAMPLE_T§', '', '', ''), ('§SAMPLE_T§', '$9.99', '$99.99', '$9.99'), ('§SAMPLE_T§', '$99,999.99', '$9.99', '-'),
      ('§SAMPLE_T§', '$99.99', '$99,999.99', '$9.99'), ('§SAMPLE_T§', '-', '-', '-'), ('§SAMPLE_T§', '$9.99', '$99.99', '-'), ('§SAMPLE_T§', '$99.99', '$99,999.99', '$99.99'),
      ('§CF_NC§', '$99.99', '$9.99', ''), ('§SAMPLE_T§', '$99,999.99', '$9.99', '$9.99'), ('§SAMPLE_T§', '$9.99', '$9.99', '$99.99'), ('', '', '', ''), ('', '', '', ''), ('§CF_NC§', '$99.99', '$99.99', '$99.99')]
S33 = CITY + '\n    <h1 class="t">§S33_T§</h1>'
S33 += '\n    ' + table(63, 122, [472, 233, 232, 217], [['', '<td class="c">2017</td>', '<td class="c">2018</td>', '<td class="c">2019</td>']] + [list(r) for r in CF],
                          [37] + [33.7] * 14, {0: 'hd2', 1: 'hi', 9: 'hi2', 14: 'hi'}, '; box-shadow:none')
SL.append(S33 + '\n    ' + ft(33) + '\n  ')
# 34 نظرة على المخاطر
S34 = '\n    <img class="bg" src="' + A + 'risks-bg.jpg" alt="§ALT_DUNE§">\n    <h1 class="t">§S34_T§</h1>\n    <p class="bd">§S9_B§</p>'
S34 += ''.join('\n    <div class="rb" style="top:%dpx; inset-inline-start:%dpx">§SAMPLE_t§</div>' % (y, x) for y in (336, 400, 464, 528) for x in (200, 658))
SL.append(S34 + '\n    ' + ft(34) + '\n  ')
# 35 تحليل المخاطر
ICO['dome'] = svg('<path d="M3 15a9 9 0 0 1 18 0h-3.2a5.8 5.8 0 0 0-11.6 0z"/><circle cx="12" cy="15.5" r="4.2"/>')
ICO['easel'] = svg('<path d="M3 4h18M4.5 4v11h15V4M9 15l-2 6M15 15l2 6M8 13V10M11 13V8M14 13V9M17 13V7" fill="none" stroke="currentColor" stroke-width="1.6"/><path d="M2 19l7-6" stroke="currentColor" stroke-width="1.4"/>')
ICO['power'] = svg('<path d="M12 2v9M7 5.5a8 8 0 1 0 10 0" fill="none" stroke="currentColor" stroke-width="2.8" stroke-linecap="round"/>')


def ring(cx, cy, col, gap_at, gap):
    c = 2 * 3.14159 * 108
    return ('<circle cx="%d" cy="%d" r="108" fill="none" stroke="%s" stroke-width="43" stroke-dasharray="%.1f %.1f" transform="rotate(%.1f %d %d)" style="filter:drop-shadow(0 3px 5px rgb(0 0 0 / .5))"/>'
            % (cx, cy, col, c * (360 - gap) / 360, c * gap / 360, gap_at + gap / 2, cx, cy))


S35 = CITY + '\n    <h1 class="t">§S35_T§</h1>'
S35 += ('\n    <svg class="rg3 flipx" viewBox="0 0 1280 720" aria-hidden="true">' + ring(312, 474, '#465866', -45, 70) + ring(420, 285, '#1D80F7', 125, 40) + ring(527, 474, '#F98226', 202, 44) + '</svg>')
S35 += '\n    <div class="rl3" style="top:244px; inset-inline-start:345px; color:#fff"><span style="color:#1D80F7; display:block">' + ICO['dome'] + '</span>§RA_A§</div>'
S35 += '\n    <div class="rl3" style="top:434px; inset-inline-start:243px"><span style="color:#465866; display:block">' + ICO['easel'] + '</span>§RA_C§</div>'
S35 += '\n    <div class="rl3" style="top:434px; inset-inline-start:458px"><span style="color:#F98226; display:block">' + ICO['power'] + '</span>§RA_X§</div>'
S35 += '\n    <p class="ctr"><b>§RA_R§</b><br>§RA_M§</p>'
S35 += ''.join('\n    <div class="ri" style="top:%dpx"><h2 style="color:%s">§%s§</h2><p>§S13_B2§</p></div>' % (y, c, k) for y, c, k in ((223, '#1D80F7', 'RA_A'), (340, '#465866', 'RA_C'), (458, '#F98226', 'RA_X')))
SL.append(S35 + '\n    ' + ft(35) + '\n  ')
# 36 تحليل سوات
SW = [(223, 152, '#D9D9D9', '#444', 'SW_S'), (645, 152, '#F98226', '#fff', 'SW_W'), (223, 394, '#1D80F7', '#fff', 'SW_O'), (645, 394, '#465866', '#fff', 'SW_T')]
S36 = CITY + '\n    <h1 class="t">§S36_T§</h1>'
S36 += ''.join('\n    <div class="sw" style="top:%dpx; inset-inline-start:%dpx"><h2 style="background:%s; color:%s">§%s§</h2><ol><li>§SW_1§</li><li>§SW_2§</li><li>§SW_2§</li></ol></div>' % (y, x, bg, fg, k) for x, y, bg, fg, k in SW)
S36 += ('\n    <p class="sl2" style="top:146px; inset-inline-start:128px; color:#D9D9D9">§L_S§</p><p class="sl2" style="top:146px; inset-inline-start:1066px; color:#F98226">§L_W§</p>'
        '\n    <p class="sl2" style="top:530px; inset-inline-start:124px; color:#1D80F7">§L_O§</p><p class="sl2" style="top:530px; inset-inline-start:1085px; color:#465866">§L_T§</p>')
SL.append(S36 + '\n    ' + ft(36) + '\n  ')
SECTIONS += [('الأرباح والخسائر (جدول)', 'Profit and Loss (table)', True), ('الميزانية العمومية (جدول)', 'Balance Sheet (table)', True),
             ('التدفق النقدي (جدول)', 'Cash Flow (table)', True), ('نظرة على المخاطر', 'Risks Overview', False),
             ('تحليل المخاطر', 'Risk Analysis', False), ('تحليل سوات', 'SWOT Analysis', False)]
STR['ar'].update({
    'S31_T': 'الأرباح والخسائر', 'S31_I': 'الإيرادات', 'S31_E': 'المصروفات', 'S31_TI': 'إجمالي الإيرادات', 'S31_TE': 'إجمالي المصروفات', 'S31_OT': 'الإجمالي العام',
    'S32_T': 'الميزانية العمومية', 'DEBIT': 'مدين', 'CREDIT': 'دائن', 'S32_TA': 'إجمالي الأصول',
    'BS_CASH': 'النقد', 'BS_AR': 'الذمم المدينة', 'BS_SUP': 'المستلزمات', 'BS_PRE': 'الإيجار المدفوع مقدمًا', 'BS_EQP': 'المعدات', 'BS_AP': 'الذمم الدائنة',
    'BS_OC': 'رأس مال المالك', 'BS_OD': 'مسحوبات المالك', 'BS_SAL': 'المبيعات', 'BS_SE': 'مصروف الرواتب', 'BS_MSC': 'مصروفات متنوعة',
    'S33_T': 'التدفق النقدي', 'CF_NI': 'صافي الدخل', 'CF_NC': 'صافي التدفق النقدي من الأنشطة التشغيلية',
    'S34_T': 'نظرة على المخاطر', 'ALT_DUNE': 'سيارة دفع رباعي على كثيب رملي',
    'S35_T': 'تحليل المخاطر', 'RA_A': 'التحليل', 'RA_C': 'الرقابة', 'RA_X': 'الإجراء', 'RA_R': 'إدارة', 'RA_M': 'المخاطر',
    'S36_T': 'تحليل سوات', 'SW_S': 'نقاط القوة', 'SW_W': 'نقاط الضعف', 'SW_O': 'الفرص', 'SW_T': 'التهديدات', 'SW_1': 'هذا نص توضيحي.', 'SW_2': 'نص توضيحي',
    'L_S': 'ق', 'L_W': 'ض', 'L_O': 'ف', 'L_T': 'ت',
})
STR['en'].update({
    'S31_T': 'Profit and Loss', 'S31_I': 'Income', 'S31_E': 'Expenses', 'S31_TI': 'Total Income', 'S31_TE': 'Total Expenses', 'S31_OT': 'Overall Total',
    'S32_T': 'Balance Sheet', 'DEBIT': 'Debit', 'CREDIT': 'Credit', 'S32_TA': 'Total Assets',
    'BS_CASH': 'Cash', 'BS_AR': 'Accounts Receivable', 'BS_SUP': 'Supplies', 'BS_PRE': 'Prepaid Pent', 'BS_EQP': 'Equipment', 'BS_AP': 'Accounts Payable',
    'BS_OC': 'Owner, Capital', 'BS_OD': 'Owner  Drawing', 'BS_SAL': 'Sales', 'BS_SE': 'Salary Expense', 'BS_MSC': 'Msc, Expense',
    'S33_T': 'Cash Flow', 'CF_NI': 'Net Income', 'CF_NC': 'Net Cash Flow From Operating Activities',
    'S34_T': 'Risks Overview', 'ALT_DUNE': 'Off-road vehicle on a sand dune',
    'S35_T': 'Risk Analysis', 'RA_A': 'Analysis', 'RA_C': 'Control', 'RA_X': 'Action', 'RA_R': 'Risk', 'RA_M': 'Management',
    'S36_T': 'SWOT Analysis', 'SW_S': 'Strengths', 'SW_W': 'Weaknesses', 'SW_O': 'Opportunities', 'SW_T': 'Threats', 'SW_1': 'This is a sample text.', 'SW_2': 'Sample text',
    'L_S': 'S', 'L_W': 'W', 'L_O': 'O', 'L_T': 'T',
})

# ======================================================== الشرائح 37–42
ICO.update({
    'piggy': svg('<path d="M5 10a7 6 0 0 1 12-2l3-1-1 3.2A6 6 0 0 1 20 13l2 .5v3l-2.4.4A7 6 0 0 1 16 19.5V22h-3v-1.6a8 8 0 0 1-3 0V22H7v-2.6A6 6 0 0 1 4 14H2v-3h2.3z"/><circle cx="8" cy="6.5" r="4.2" fill="currentColor"/><path d="M8 4v5M9.5 5c-.4-.5-1-.7-1.6-.7-.9 0-1.5.5-1.5 1.1 0 1.6 3.2.9 3.2 2.5 0 .7-.7 1.2-1.6 1.2-.7 0-1.3-.3-1.7-.8" fill="none" stroke="#1D80F7" stroke-width="1.1"/>'),
    'timer': svg('<circle cx="12" cy="13" r="8.5" fill="none" stroke="currentColor" stroke-width="2.6"/><path d="M12 13l-4-4" stroke="currentColor" stroke-width="2.6" stroke-linecap="round"/><path d="M10 2h4v2.5h-4z"/>'),
    'podium': svg('<path d="M3 13h18v8H3z"/><path d="M6 15h12v2H6z" fill="#262626"/><path d="M12 1.5l2.6 4.5 5 .8-3.6 3.6.8 5-4.8-2.4-4.8 2.4.8-5L4.4 6.8l5-.8z"/><path d="M11 6.5h1.6v5" fill="none" stroke="#262626" stroke-width="1.4"/>', '0 0 24 22'),
    'dollar': svg('<circle cx="12" cy="12" r="10.5"/><path d="M12 5v14M15.5 8.2c-.8-1-2-1.5-3.5-1.5-2 0-3.3 1-3.3 2.5 0 3.5 7 2 7 5.6 0 1.6-1.5 2.7-3.6 2.7-1.6 0-3-.6-3.8-1.7" fill="none" stroke="#1D80F7" stroke-width="2.2"/>'),
    'laptop': svg('<path d="M4 5h16v10H4z" fill="none" stroke="currentColor" stroke-width="2.2"/><path d="M1 17h22l-1.5 2.5h-19z"/>'),
    'bars3': svg('<path d="M5 13h3.5v7H5zM10.25 9h3.5v11h-3.5zM15.5 4H19v16h-3.5z"/>'),
    'cup': svg('<path d="M6 3h12v5a6 6 0 0 1-12 0z"/><path d="M6 5H3v2a4 4 0 0 0 4 4M18 5h3v2a4 4 0 0 1-4 4" fill="none" stroke="currentColor" stroke-width="2"/><path d="M11 14h2v3h-2zM7.5 17h9v3h-9z"/>'),
    'skull': svg('<path d="M12 3a7 6.5 0 0 0-5 11v2.5h10V14a7 6.5 0 0 0-5-11z"/><circle cx="9.3" cy="10" r="1.8" fill="#1D80F7"/><circle cx="14.7" cy="10" r="1.8" fill="#1D80F7"/><path d="M11 13.5l1-1.8 1 1.8z" fill="#1D80F7"/><circle cx="3" cy="17" r="2"/><circle cx="21" cy="17" r="2"/><circle cx="3" cy="4" r="2"/><circle cx="21" cy="4" r="2"/><path d="M3 17l18 4M21 17L3 21M3 4l3 2M21 4l-3 2" stroke="currentColor" stroke-width="2"/><path d="M9 17.5h6v3H9z"/>'),
    'person': svg('<circle cx="9" cy="4" r="2.5"/><path d="M5.5 8h7v7h-1.5v8h-1.5v-6H8.5v6H7v-8H5.5z"/><circle cx="17" cy="5" r="2.6" fill="none" stroke="currentColor" stroke-width="1.6"/><circle cx="20.5" cy="2" r="1.6" fill="none" stroke="currentColor" stroke-width="1.2"/>'),
    'check': svg('<circle cx="12" cy="12" r="10" fill="none" stroke="currentColor" stroke-width="1.8"/><path d="M7.5 12.3l3 3 6-6.3" fill="none" stroke="currentColor" stroke-width="1.8"/>'),
})
# 37 العوامل المقيّدة (بلا أيقونات تواصل في الأصل)
S37 = CITY + '\n    <h1 class="t">§S37_T§</h1>\n    <p class="bd">§S37_B§<br><br>§S37_B§</p>\n    <div class="vl"></div>'
S37 += ''.join('\n    <div class="nb" style="top:%dpx">%s</div><p class="nt" style="top:%dpx">§S37_I§</p>' % (y - 24, n, y - 18) for y, n in ((285, '1'), (356, '2'), (430, '2'), (501, '4')))
SL.append(S37 + '\n    <div class="pg">37</div>\n  ')
# 38 عوامل النجاح الحاسمة
CF7 = [(640, 237, 'clip'), (767, 297, 'timer'), (798, 435, 'shake'), (712, 546, 'growth'), (569, 546, 'bulb'), (482, 432, 'trophy'), (513, 297, 'piggy')]
S38 = CITY + '\n    <h1 class="t">§S38_T§</h1>'
S38 += ('\n    <svg class="hp flipx" viewBox="0 0 1280 720" aria-hidden="true"><polygon points="%s" fill="rgb(29 128 247 / .08)" stroke="rgb(29 110 235 / .45)" stroke-width="86" stroke-linejoin="round"/>' % ' '.join('%d,%d' % (x, y) for x, y, i in CF7)
        + ''.join('<circle cx="%d" cy="%d" r="58" fill="rgb(20 110 240 / .35)"/><circle cx="%d" cy="%d" r="49" fill="#0A5FC8"/><circle cx="%d" cy="%d" r="42" fill="#1D80F7"/>' % (x, y, x, y, x, y) for x, y, i in CF7) + '</svg>')
S38 += ''.join('\n    <div class="cf" style="top:%dpx; inset-inline-start:%dpx">%s</div>' % (y - 42, x - 42, ICO[i]) for x, y, i in CF7)
S38 += '\n    <div class="mid">' + ICO['podium'] + '</div>'
S38 += ''.join('\n    <div class="cfl e" style="top:%dpx; inset-inline-start:80px"><h2>§S13_H0§</h2><p>§S13_B2§</p></div>' % (y - 13) for y in (218, 370, 521))
S38 += ''.join('\n    <div class="cfl" style="top:%dpx; inset-inline-start:900px"><h2>§S13_H0§</h2><p>§S13_B2§</p></div>' % (y - 13) for y in (152, 297, 442, 587))
SL.append(S38 + '\n    ' + ft(38) + '\n  ')
# 39 السيناريوهات البديلة
AS = [(148, 185, 'bulb'), (652, 185, 'dollar'), (148, 336, 'laptop'), (652, 336, 'target'), (148, 487, 'bars3'), (652, 487, 'cup')]
S39 = CITY + '\n    <h1 class="t">§S39_T§</h1>'
S39 += ''.join('\n    <div class="as" style="top:%dpx; inset-inline-start:%dpx"><i>%s</i><div><h2>§SAMPLE_t§</h2><p>§S13_B2§</p></div></div>' % (y, x, ICO[ic]) for x, y, ic in AS)
SL.append(S39 + '\n    ' + ft(39) + '\n  ')
# 40 المخاطر وحلولها
CK = [(274, 'B1'), (335, 'B2'), (397, 'B1'), (469, 'B1'), (531, 'B3'), (580, 'B3')]
S40 = CITY + '\n    <h1 class="t">§S40_T§</h1>\n    <div class="vl"></div>'
S40 += '\n    <div class="rh" style="inset-inline-start:112px"><span>§S40_R§</span>' + ICO['skull'] + '</div><div class="rh" style="inset-inline-start:745px"><span>§S40_S§</span>' + ICO['person'] + '</div>'
S40 += ''.join('\n    <div class="ck" style="top:%dpx; inset-inline-start:%dpx">%s<span>§%s§</span></div>' % (y - 23, x, ICO['check'], k) for y, k in CK for x in (118, 747))
S40 += '\n    <div class="amp">&amp;</div>'
SL.append(S40 + '\n    ' + ft(40) + '\n  ')
# 41 الخلاصة
S41 = CITY + '\n    <h1 class="t">§S41_T§</h1>'
S41 += ('\n    <svg class="bn flipx" viewBox="0 0 1280 720" aria-hidden="true"><polygon points="226,484 318,484 303,537" fill="#0A5FC8"/><polygon points="993,202 1047,183 1055,202" fill="#0A5FC8"/>'
        '<polygon points="287,202 1055,202 993,484 226,484" fill="#1D80F7" style="filter:drop-shadow(0 3px 5px rgb(0 0 0 / .4))"/></svg>')
S41 += '\n    <p class="bd">§S41_B§</p>'
SL.append(S41 + '\n    ' + ft(41) + '\n  ')
# 42 شكرًا
SL.append('\n    <h1 class="ty">§TY1§<b>§TY2§</b></h1>\n    <p class="bd">§B1§</p>\n    ' + ft(42) + '\n  ')
SECTIONS += [('العوامل المقيّدة', 'Limiting Factors', False), ('عوامل النجاح الحاسمة', 'Critical Success Factors', False),
             ('السيناريوهات البديلة', 'Alternative Scenarios', False), ('المخاطر المحددة وحلولها', 'Specific Risks and Their Solutions', False),
             ('الخلاصة', 'Conclusion', False), ('شكرًا لكم', 'Thank You', False)]
STR['ar'].update({
    'S37_T': 'العوامل المقيّدة', 'S37_B': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا.', 'S37_I': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
    'S38_T': 'عوامل النجاح الحاسمة', 'S13_H0': 'نص توضيحي',
    'S39_T': 'السيناريوهات البديلة',
    'S40_T': 'المخاطر المحددة وحلولها', 'S40_R': 'المخاطر', 'S40_S': 'الحلول',
    'S41_T': 'الخلاصة', 'S41_B': 'هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا. هذا نص توضيحي. أضف النص المطلوب هنا.',
    'TY1': 'شكرًا', 'TY2': 'لكم',
})
STR['en'].update({
    'S37_T': 'Limiting Factors', 'S37_B': 'This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here.', 'S37_I': 'This is a sample text. Insert your desired text here.',
    'S38_T': 'Critical Success Factors', 'S13_H0': 'Sample text',
    'S39_T': 'Alternative Scenarios',
    'S40_T': 'Specific Risks and Their Solutions', 'S40_R': 'Risks', 'S40_S': 'Solutions',
    'S41_T': 'Conclusion', 'S41_B': 'This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here. This is a sample text. Insert your desired text here.',
    'TY1': 'THANK', 'TY2': 'YOU',
})

JSON_BLOCKS = {}
EXTRA_SCRIPT = r"""  // ----- الرسوم البيانية: الإحداثيات بنظام الإطار 1280×720؛ في العربية تنعكس الفئات أفقيًا (الأولى في اليمين) -----
  var NS = 'http://www.w3.org/2000/svg';
  function el(t, a, p) { var e = document.createElementNS(NS, t); for (var k in a) e.setAttribute(k, a[k]); if (p) p.appendChild(e); return e; }
  function X(x) { return RTL ? 1280 - x : x; }
  function data(id) { var n = document.getElementById(id); return n ? JSON.parse(n.textContent) : null; }
  function fmt(v) { return String(v); }
"""


JSON_BLOCKS['chart-data'] = {'ar': {'categories': ['الفئة 1', 'الفئة 2', 'الفئة 3', 'الفئة 4', 'الفئة 5', 'الفئة 6'], 'values': [4.3, 2.5, 3.5, 4.5, 3, 2], 'max': 5, 'step': 0.5},
                             'en': {'categories': ['Category 1', 'Category 2', 'Category 3', 'Category 4', 'Category 5', 'Category 6'], 'values': [4.3, 2.5, 3.5, 4.5, 3, 2], 'max': 5, 'step': 0.5}}
EXTRA_SCRIPT += r"""
  // رسم أعمدة عام: o = {x0, x1, base, top, barW, labelX, catY, fill, grid, fs}
  function bars(svgId, d, o) {
    var svg = document.getElementById(svgId); if (!svg || !d) return;
    var n = d.categories.length, w = (o.x1 - o.x0) / n, k = (o.base - o.top) / d.max;
    for (var v = 0; v <= d.max + 1e-9; v += d.step) {
      var y = o.base - v * k;
      if (o.grid) el('line', { x1: X(o.x0), x2: X(o.x1), y1: y, y2: y, stroke: o.grid, 'stroke-width': 1 }, svg);
      var t = el('text', { x: X(o.labelX), y: y + 5, 'font-size': o.fs || 15, 'text-anchor': RTL ? 'start' : 'end' }, svg); t.textContent = fmt(Math.round(v * 100) / 100);
    }
    el('line', { x1: X(o.x0), x2: X(o.x1), y1: o.base, y2: o.base, stroke: o.axis || 'rgb(255 255 255 / .8)', 'stroke-width': 1 }, svg);
    d.values.forEach(function (v, i) {
      var cx = o.x0 + w * (i + .5), h = v * k;
      el('rect', { x: X(cx) - o.barW / 2, y: o.base - h, width: o.barW, height: h, fill: o.fill }, svg);
      var t = el('text', { x: X(cx), y: o.catY, 'font-size': o.cfs || 18, 'text-anchor': 'middle' }, svg); t.textContent = d.categories[i];
    });
  }
  bars('ch18', data('chart-data'), { x0: 128, x1: 1186, base: 614, top: 314, barW: 70, labelX: 112, catY: 644, fill: '#D6E7FF', fs: 16 });
"""

JSON_BLOCKS.update(JSON_BLOCKS_LATE)
EXTRA_SCRIPT += r"""
  // رسم خطي: o = {x0, dx, base, top, labelX, catY, color}
  function line(svgId, d, o) {
    var svg = document.getElementById(svgId); if (!svg || !d) return;
    var k = (o.base - o.top) / d.max, pts = [];
    for (var v = 0; v <= d.max + 1e-9; v += d.step) { var t = el('text', { x: X(o.labelX), y: o.base - v * k + 7, 'font-size': 19, 'text-anchor': 'middle' }, svg); t.textContent = fmt(v); }
    el('line', { x1: X(o.x0 - 40), x2: X(o.x0 + o.dx * (d.values.length - 1) + 40), y1: o.base, y2: o.base, stroke: 'rgb(255 255 255 / .85)', 'stroke-width': 1 }, svg);
    d.values.forEach(function (v, i) { var x = X(o.x0 + o.dx * i), y = o.base - v * k; pts.push(x + ',' + y);
      var t = el('text', { x: x, y: o.catY, 'font-size': 19, 'text-anchor': 'middle' }, svg); t.textContent = d.categories[i]; });
    el('polyline', { points: pts.join(' '), fill: 'none', stroke: o.color, 'stroke-width': 3, 'stroke-linejoin': 'round' }, svg);
    pts.forEach(function (p) { var a = p.split(','); el('circle', { cx: a[0], cy: a[1], r: 5, fill: o.color }, svg); });
  }
  line('ch26', data('chart-data-2'), { x0: 162, dx: 77.8, base: 476, top: 158, labelX: 98, catY: 508, color: '#2F7BF0' });
"""

EXTRA_SCRIPT += r"""
  // أعمدة ملوّنة لكل فئة (28)
  (function () {
    var d = data('chart-data-3'), svg = document.getElementById('ch28'); if (!d || !svg) return;
    var base = 601, k = (601 - 148) / d.max, x0 = 107, w = (847 - 107) / d.values.length;
    for (var v = 0; v <= d.max; v += d.step) { var t = el('text', { x: X(83), y: base - v * k + 7, 'font-size': 20, 'text-anchor': 'middle' }, svg); t.textContent = v; }
    el('line', { x1: X(107), x2: X(847), y1: base, y2: base, stroke: 'rgb(255 255 255 / .85)' }, svg);
    d.values.forEach(function (v, i) { var cx = x0 + w * (i + .5);
      el('rect', { x: X(cx) - 25, y: base - v * k, width: 50, height: v * k, fill: d.colors[i] }, svg);
      var t = el('text', { x: X(cx), y: 634, 'font-size': 20, 'text-anchor': 'middle' }, svg); t.textContent = d.categories[i]; });
  })();
  // أعمدة مجمّعة (29)
  (function () {
    var d = data('chart-data-4'), svg = document.getElementById('ch29'); if (!d || !svg) return;
    var base = 575, k = (575 - 178) / d.max, x0 = 155, w = (750 - 155) / d.categories.length, ns = d.series.length;
    for (var v = 0; v <= d.max; v += d.step) { var y = base - v * k;
      el('line', { x1: X(157), x2: X(750), y1: y, y2: y, stroke: 'rgb(255 255 255 / .28)' }, svg);
      var t = el('text', { x: X(141), y: y + 5, 'font-size': 14.5, 'text-anchor': RTL ? 'start' : 'end' }, svg); t.textContent = v; }
    d.categories.forEach(function (c, i) { var cx = x0 + w * (i + .5);
      d.series.forEach(function (s, j) { var bx = cx + (j - (ns - 1) / 2) * 41, h = s[i] * k;
        el('rect', { x: X(bx) - 16.5, y: base - h, width: 33, height: h, fill: d.colors[j] }, svg); });
      var t = el('text', { x: X(cx), y: 599, 'font-size': 15, 'text-anchor': 'middle' }, svg); t.textContent = c; });
  })();
  // فطيرة ثلاثية الأبعاد (30): طبقات داكنة متراصة ثم الوجه العلوي؛ الزاوية تبدأ من الأعلى باتجاه عقارب الساعة (عكسه في العربية)
  (function () {
    var d = data('chart-data-5'), svg = document.getElementById('ch30'); if (!d || !svg) return;
    var cx = 640 - 305, cy = 318, rx = 185, ry = 92, depth = 30, tot = d.values.reduce(function (a, b) { return a + b; }, 0);
    function shade(hex, f) { var n = parseInt(hex.slice(1), 16), r = (n >> 16) * f, g = (n >> 8 & 255) * f, b = (n & 255) * f; return 'rgb(' + [r, g, b].map(Math.round).join(',') + ')'; }
    var a = 0, sl = [];
    d.values.forEach(function (v, i) { var a1 = a + v / tot * Math.PI * 2, mid = (a + a1) / 2, ex = i === 0 ? 10 : 16;
      sl.push({ a0: a, a1: a1, dx: Math.sin(mid) * ex, dy: -Math.cos(mid) * ex * .5, c: d.colors[i] }); a = a1; });
    function path(s, off) {
      var ox = cx + s.dx, oy = cy + s.dy + off, n = 40, p = 'M' + X(ox) + ',' + oy;
      for (var k = 0; k <= n; k++) { var t = s.a0 + (s.a1 - s.a0) * k / n; p += ' L' + X(ox + rx * Math.sin(t)) + ',' + (oy - ry * Math.cos(t)); }
      return p + ' Z';
    }
    for (var off = depth; off > 0; off -= 2) sl.forEach(function (s) { el('path', { d: path(s, off), fill: shade(s.c, .62) }, svg); });
    sl.forEach(function (s) { el('path', { d: path(s, 0), fill: s.c }, svg); });
  })();
"""

# ##TAIL## (يُبقى في آخر الملف)
TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'mgmt', 'icon': '📘',
    'name': 'خطة العمل', 'nameEn': 'Business Plan',
    'description': '42 شريحة لخطة عمل كاملة: الشركة والفريق والسوق والمنافسين والاستراتيجية والمالية والمخاطر',
    'descriptionEn': '42 slides for a full business plan: company, team, market, competitors, strategy, finance and risks',
    'sections': [dict({'title': a, 'titleEn': b}, **({'needsData': True} if d else {})) for a, b, d in SECTIONS],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والصور وغيّر النصوص والأرقام فقط؛ أرقام الرسوم البيانية في كتل JSON بالمعرّفات chart-data-*.',
}
