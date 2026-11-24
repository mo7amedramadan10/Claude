# -*- coding: utf-8 -*-
# نموذج «التدريب المؤسسي بالذكاء الاصطناعي»: عشر شرائح بهوية كحلية/بنفسجية/زمردية —
# غلاف بصورة خطوط عصبية زرقاء ومدرِّبة، أجندة مرقّمة، إحصاءات في بطاقات، أساسيات بدوائر متداخلة،
# أقسام الشركة، تعاون الإنسان والآلة، مهارات المستقبل، الحوكمة، خارطة طريق زمنية، وخلاصات.
KEY = 'sl-ai-corporate-training'
TITLE = {'ar': 'التدريب المؤسسي بالذكاء الاصطناعي', 'en': 'AI Corporate Training'}
COMMENT = 'نموذج «التدريب المؤسسي بالذكاء الاصطناعي»: عشر شرائح بخلفيات كحلية وبيضاء متناوبة، ألواح بزوايا مدوّرة من جهة واحدة، أرقام كبيرة فوق دوائر داكنة، بطاقات إحصاءات، دوائر متداخلة SVG بالتدرّج، وخط زمني بسهم. الصور الفوتوغرافية (خلفية الغلاف وصورة المدرِّبة) في assets.'
VARS = '--ink: #080F39; --accent: #7B3FE4; --bg: #FFFFFF; --navy: #080F39; --purple: #7B3FE4; --teal: #12BBB2; --cyan: #0BB4D3; --lav: #D2B8F7; --lav-2: #D9DAEA; --gray: #5A5E6B; --circle: #252652; --circle-2: #1F2550; --tint: #F2EAFF;'

STYLE = """  /* أنماط مشتركة */
  .frame { font-family: var(--font); }
  .ttl { position: absolute; margin: 0; font-size: 56px; font-weight: 700; line-height: 1.12; color: var(--navy); letter-spacing: -0.3px; }
  .ttl.light { color: #fff; }
  .ttl.ctr { inset-inline: 0; text-align: center; }
  html[lang="ar"] .ttl { font-weight: 800; line-height: 1.25; }
  .sub { position: absolute; margin: 0; font-size: 22px; font-weight: 400; color: var(--purple); line-height: 1.3; }
  .sub.lav { color: var(--lav); }
  .sub.ctr { inset-inline: 0; text-align: center; }
  .h2 { margin: 0; font-size: 22px; font-weight: 700; line-height: 1.25; color: var(--navy); }
  .p { margin: 0; font-size: 18px; font-weight: 400; line-height: 1.4; color: var(--gray); }
  .light .h2, .dark .h2 { color: #fff; }
  .light .p, .dark .p { color: var(--lav-2); }
  .ic svg, .pic svg { width: 100%; height: 100%; }
  /* قائمة مرقّمة بأرقام كبيرة فوق دوائر داكنة (الشريحتان 2 و10) */
  .nl { position: absolute; margin: 0; padding: 0; list-style: none; width: 590px; }
  .nl li { position: relative; height: 124px; padding-inline-start: 100px; padding-top: 4px; }
  .nl .num { position: absolute; top: 0; inset-inline-start: 8px; font-size: 46px; font-weight: 700; line-height: 1; color: var(--lav); z-index: 1; }
  .nl .num::before { content: ''; position: absolute; z-index: -1; width: 50px; height: 50px; border-radius: 50%; background: var(--circle); top: -8px; inset-inline-start: -8px; }
  .nl.teal .num { color: #25C5BB; }
  .nl .h2 { color: #fff; margin-bottom: 8px; }
  .nl .p { color: var(--lav-2); font-size: 19px; }
  /* رأس كحلي بزاوية سفلية مدوّرة (الشريحتان 7 و9) */
  .hdr { position: absolute; top: 0; inset-inline-start: 0; height: 218px; background: var(--navy); border-end-end-radius: 52px; }
  /* الشريط الزمردي السفلي (الشريحتان 3 و5) */
  .bar { position: absolute; margin: 0; background: var(--teal); border-radius: 12px; color: var(--navy); font-size: 19px; line-height: 1.35; display: flex; align-items: center; justify-content: center; text-align: center; padding: 10px 70px; }
  /* دائرة أيقونة بلون فاتح مزاحة خلف الأيقونة (الشريحتان 4 و7) */
  .ic { position: absolute; width: 66px; height: 64px; }
  .ic::before { content: ''; position: absolute; top: 0; inset-inline-start: 0; width: 54px; height: 54px; border-radius: 50%; background: var(--tint); }
  .ic svg { position: absolute; inset-inline-start: 14px; top: 8px; width: 50px; height: 50px; }
  .ic.pu { color: var(--purple); } .ic.bl { color: #2F5BC6; } .ic.bl::before { background: #E8EEFF; } .ic.tl { color: #16B5A8; } .ic.tl::before { background: #E3F7F5; }
  /* الشعار (الشريحتان 1 و10) */
  .logo { position: absolute; width: 52px; height: 52px; }
  .logo-t { position: absolute; margin: 0; color: #fff; font-size: 17px; font-weight: 400; letter-spacing: 2.5px; text-transform: uppercase; line-height: 1.35; }
  html[lang="ar"] .logo-t { letter-spacing: 0; font-size: 18px; }

  /* 1) الغلاف */
  .s1 { background: #070B12; }
  .s1 .bgp { position: absolute; top: 0; inset-inline-end: 0; width: 720px; height: 720px; object-fit: cover; -webkit-mask-image: linear-gradient(to right, transparent, #000 28%); mask-image: linear-gradient(to right, transparent, #000 28%); }
  html[dir="rtl"] .s1 .bgp { transform: scaleX(-1); }
  .s1 .logo { top: 90px; inset-inline-start: 97px; }
  .s1 .logo-t { top: 96px; inset-inline-start: 175px; }
  .s1 .c1 { position: absolute; top: 246px; inset-inline-start: 98px; margin: 0; font-size: 54px; font-weight: 300; letter-spacing: 1px; line-height: 1.1; background: linear-gradient(90deg, #D9EDFF 0%, #B8C9FF 55%, #8E74F2 100%); -webkit-background-clip: text; background-clip: text; color: transparent; }
  .s1 .c2 { position: absolute; top: 312px; inset-inline-start: 94px; margin: 0; font-size: 86px; font-weight: 800; letter-spacing: 1px; line-height: 1.05; background: linear-gradient(90deg, #D9EDFF 0%, #A9B8FF 50%, #7A5CEE 100%); -webkit-background-clip: text; background-clip: text; color: transparent; }
  html[dir="rtl"] .s1 .c1, html[dir="rtl"] .s1 .c2 { background-image: linear-gradient(270deg, #D9EDFF 0%, #B0C2FF 55%, #8366F0 100%); }
  html[lang="ar"] .s1 .c1 { font-weight: 400; font-size: 50px; top: 240px; letter-spacing: 0; }
  html[lang="ar"] .s1 .c2 { font-size: 76px; top: 302px; letter-spacing: 0; line-height: 1.2; }
  .s1 .c3 { position: absolute; top: 420px; inset-inline-start: 98px; margin: 0; font-size: 32px; font-weight: 300; letter-spacing: 5px; color: #fff; line-height: 1.2; }
  html[lang="ar"] .s1 .c3 { letter-spacing: 0; font-size: 30px; font-weight: 400; }
  .s1 .c4 { position: absolute; top: 488px; inset-inline-start: 98px; margin: 0; font-size: 19px; color: #BFC2CC; }
  .s1 .avatar { position: absolute; top: 585px; inset-inline-start: 92px; width: 72px; height: 72px; border-radius: 50%; object-fit: cover; }
  .s1 .name { position: absolute; top: 590px; inset-inline-start: 197px; margin: 0; font-size: 22px; font-weight: 700; color: #fff; }
  .s1 .role { position: absolute; top: 626px; inset-inline-start: 197px; margin: 0; font-size: 19px; color: #F0F0F5; }

  /* 2) الأجندة */
  .s2 .panel { position: absolute; top: 0; inset-inline-end: 0; width: 708px; height: 720px; background: var(--navy); border-start-start-radius: 70px; }
  .s2 .ttl { top: 64px; inset-inline-start: 98px; }
  .s2 .sub { top: 268px; inset-inline-start: 98px; }
  .s2 .lead { position: absolute; top: 414px; inset-inline-start: 98px; margin: 0; font-size: 22px; font-weight: 700; color: var(--navy); line-height: 1.3; }
  .s2 .bul { position: absolute; top: 492px; inset-inline-start: 98px; width: 400px; margin: 0; padding: 0; list-style: none; }
  .s2 .bul li { position: relative; padding-inline-start: 32px; font-size: 18px; line-height: 1.4; color: var(--gray); margin-bottom: 12px; }
  .s2 .bul li::before { content: '✦'; position: absolute; inset-inline-start: 0; top: 0; color: var(--purple); font-size: 18px; }
  .s2 .nl { top: 78px; inset-inline-start: 625px; }

  /* 3) لماذا يهم الذكاء الاصطناعي */
  .s3 { background: var(--navy); }
  .s3 .ttl { top: 62px; }
  .s3 .sub { top: 138px; }
  .s3 .card { position: absolute; top: 227px; width: 280px; height: 327px; background: #fff; border-radius: 16px; text-align: center; padding: 42px 30px 0; }
  .s3 .card .big { margin: 0 0 30px; font-size: 62px; font-weight: 300; line-height: 1.1; color: var(--purple); }
  .s3 .card .h2 { margin-bottom: 14px; }
  .s3 .k1 { inset-inline-start: 51px; } .s3 .k2 { inset-inline-start: 350px; } .s3 .k3 { inset-inline-start: 650px; } .s3 .k4 { inset-inline-start: 949px; }
  .s3 .bar { top: 580px; inset-inline-start: 51px; width: 1178px; height: 88px; }

  /* 4) أساسيات الذكاء الاصطناعي */
  .s4 { background: var(--navy); }
  .s4 .panel { position: absolute; top: 255px; inset-inline-start: 0; width: 573px; height: 465px; background: #fff; border-start-end-radius: 46px; }
  .s4 .ttl { top: 64px; inset-inline-start: 98px; }
  .s4 .it { position: absolute; inset-inline-start: 88px; width: 420px; }
  .s4 .it .ic { top: -4px; inset-inline-start: 0; }
  .s4 .it .tx { margin-inline-start: 97px; }
  .s4 .it .h2 { margin-bottom: 10px; }
  .s4 .it .p { width: 310px; }
  .s4 .i1 { top: 310px; } .s4 .i2 { top: 437px; } .s4 .i3 { top: 564px; }
  .s4 .cir { position: absolute; border-radius: 50%; }
  .s4 .c-big { top: 80px; inset-inline-start: 653px; width: 576px; height: 576px; background: #412594; border: 2px solid #7B3FE4; }
  .s4 .c-mid { top: 230px; inset-inline-start: 729px; width: 424px; height: 424px; background: #3444BE; border: 2px solid #6A7BF5; }
  .s4 .c-sm { top: 390px; inset-inline-start: 808px; width: 266px; height: 266px; background: #247EBA; border: 2px solid #19C2B8; }
  .s4 .lbl { position: absolute; inset-inline-start: 653px; width: 576px; text-align: center; margin: 0; }
  .s4 .lbl .h2 { color: #fff; margin-bottom: 8px; }
  .s4 .lbl .p { color: #fff; }
  .s4 .l1 { top: 142px; } .s4 .l2 { top: 302px; } .s4 .l3 { top: 490px; }

  /* 5) الأقسام */
  .s5 .ttl { top: 62px; }
  .s5 .sub { top: 141px; }
  .s5 .panel { position: absolute; top: 203px; inset-inline-start: 28px; width: 1224px; height: 540px; background: var(--navy); border-start-start-radius: 34px; border-start-end-radius: 34px; }
  .s5 .it { position: absolute; width: 330px; }
  .s5 .it .pic { position: absolute; top: 0; inset-inline-start: 0; width: 84px; height: 84px; border-radius: 50%; background: #3B3C6C; color: #E6DDFF; display: grid; place-items: center; }
  .s5 .it .pic svg { width: 46px; height: 46px; }
  .s5 .it .tx { margin-inline-start: 118px; padding-top: 10px; }
  .s5 .it .h2 { color: #fff; margin-bottom: 8px; }
  .s5 .it .p { color: var(--lav-2); width: 180px; }
  .s5 .a1 { top: 262px; inset-inline-start: 112px; } .s5 .a2 { top: 262px; inset-inline-start: 487px; } .s5 .a3 { top: 262px; inset-inline-start: 861px; }
  .s5 .a4 { top: 430px; inset-inline-start: 112px; } .s5 .a5 { top: 430px; inset-inline-start: 487px; } .s5 .a6 { top: 430px; inset-inline-start: 861px; }
  .s5 .vl { position: absolute; top: 283px; width: 1px; height: 242px; background: #2A3060; }
  .s5 .v1 { inset-inline-start: 437px; } .s5 .v2 { inset-inline-start: 820px; }
  .s5 .hl { position: absolute; top: 397px; inset-inline-start: 115px; width: 1070px; height: 1px; background: #2A3060; }
  .s5 .src { position: absolute; top: 575px; inset-inline-start: 112px; margin: 0; background: #fff; color: var(--navy); font-size: 13px; font-weight: 700; letter-spacing: .5px; padding: 8px 18px; border-radius: 8px; z-index: 1; }
  .s5 .bar { top: 595px; inset-inline-start: 88px; width: 1104px; height: 82px; padding: 10px 30px; font-size: 18px; }

  /* 6) التعاون */
  .s6 .ttl { top: 62px; }
  .s6 .sub { top: 141px; }
  .s6 .card { position: absolute; top: 227px; width: 290px; height: 414px; background: var(--navy); border-radius: 16px; text-align: center; padding: 44px 28px 0; }
  .s6 .card .pic { width: 124px; height: 124px; border-radius: 50%; background: var(--circle-2); color: var(--teal); margin: 0 auto 50px; display: grid; place-items: center; }
  .s6 .card .pic svg { width: 56px; height: 56px; }
  .s6 .card .h2 { color: #fff; text-transform: uppercase; margin-bottom: 18px; }
  .s6 .card .p { color: var(--lav-2); }
  .s6 .k1 { inset-inline-start: 46px; } .s6 .k2 { inset-inline-start: 346px; } .s6 .k3 { inset-inline-start: 645px; } .s6 .k4 { inset-inline-start: 944px; }

  /* 7) مهارات المستقبل */
  .s7 .hdr { width: 1260px; }
  .s7 .ttl, .s9 .ttl { top: 64px; inset-inline-start: 98px; }
  .s7 .sub, .s9 .sub { top: 141px; inset-inline-start: 98px; }
  .s7 .it { position: absolute; width: 530px; }
  .s7 .it .ic { top: -8px; inset-inline-start: 0; }
  .s7 .it .tx { margin-inline-start: 94px; }
  .s7 .it .h2 { margin-bottom: 10px; }
  .s7 .it .p { width: 425px; }
  .s7 .r1 { top: 270px; } .s7 .r2 { top: 423px; } .s7 .r3 { top: 578px; }
  .s7 .cA { inset-inline-start: 86px; } .s7 .cB { inset-inline-start: 640px; }

  /* 8) الحوكمة */
  .s8 .ttl { top: 64px; inset-inline-start: 98px; }
  .s8 .sub { top: 204px; inset-inline-start: 98px; }
  .s8 .panel { position: absolute; top: 270px; inset-inline-start: 0; width: 1220px; height: 450px; background: var(--navy); border-start-end-radius: 60px; }
  .s8 .col { position: absolute; top: 340px; width: 230px; }
  .s8 .col .num { position: relative; z-index: 0; display: inline-block; margin: 0 0 54px 8px; font-size: 60px; font-weight: 700; line-height: 1; color: #25C5BB; }
  .s8 .col .num::before { content: ''; position: absolute; z-index: -1; width: 56px; height: 56px; border-radius: 50%; background: #1C2250; top: -12px; inset-inline-start: -8px; }
  .s8 .col .h2 { color: #fff; margin-bottom: 14px; }
  .s8 .col .p { color: var(--lav-2); width: 210px; }
  .s8 .g1 { inset-inline-start: 93px; } .s8 .g2 { inset-inline-start: 384px; } .s8 .g3 { inset-inline-start: 676px; } .s8 .g4 { inset-inline-start: 967px; }
  .s8 .vl { position: absolute; top: 305px; width: 1px; height: 365px; background: #1A2150; }
  .s8 .v1 { inset-inline-start: 345px; } .s8 .v2 { inset-inline-start: 637px; } .s8 .v3 { inset-inline-start: 929px; }

  /* 9) خارطة الطريق */
  .s9 .hdr { width: 1280px; }
  .s9 .line { position: absolute; top: 396px; inset-inline-start: 0; width: 1205px; height: 5px; background: var(--cyan); }
  .s9 .arrow { position: absolute; top: 389px; inset-inline-start: 1198px; width: 16px; height: 19px; background: var(--cyan); clip-path: polygon(0 0, 100% 50%, 0 100%); }
  html[dir="rtl"] .s9 .arrow { transform: scaleX(-1); }
  .s9 .dot { position: absolute; top: 385px; width: 26px; height: 26px; border-radius: 50%; background: var(--cyan); }
  .s9 .d1 { inset-inline-start: 98px; } .s9 .d2 { inset-inline-start: 389px; } .s9 .d3 { inset-inline-start: 681px; } .s9 .d4 { inset-inline-start: 973px; }
  .s9 .col { position: absolute; top: 284px; width: 230px; }
  .s9 .col .yr { position: relative; z-index: 0; display: inline-block; margin: 0 0 78px 6px; font-size: 56px; font-weight: 700; line-height: 1; color: var(--navy); }
  .s9 .col .yr::before { content: ''; position: absolute; z-index: -1; width: 60px; height: 60px; border-radius: 50%; background: #EDEDED; top: -8px; inset-inline-start: -10px; }
  .s9 .col .h2 { text-transform: uppercase; margin-bottom: 20px; }
  .s9 .col .p { width: 215px; }
  .s9 .g1 { inset-inline-start: 98px; } .s9 .g2 { inset-inline-start: 390px; } .s9 .g3 { inset-inline-start: 682px; } .s9 .g4 { inset-inline-start: 974px; }

  /* 10) الخلاصات */
  .s10 { background: var(--navy); }
  .s10 .ttl { top: 64px; inset-inline-start: 98px; }
  .s10 .sub { top: 212px; inset-inline-start: 98px; }
  .s10 .para { position: absolute; top: 293px; inset-inline-start: 98px; width: 400px; margin: 0; font-size: 18px; line-height: 1.42; color: var(--lav-2); }
  .s10 .logo { top: 582px; inset-inline-start: 110px; }
  .s10 .logo-t { top: 588px; inset-inline-start: 190px; }
  .s10 .nl { top: 86px; inset-inline-start: 625px; }
  .s10 .nl li { height: 122px; }"""

# ----- الأيقونات (SVG مضمّن بلون currentColor) -----
_S = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">%s</svg>'
_F = '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">%s</svg>'


def _snow():  # ندفة ثلج: ثلاثة محاور بفروع صغيرة
    axis = '<path d="M12 2v20M12 2l-2.3 2.3M12 2l2.3 2.3M12 22l-2.3-2.3M12 22l2.3-2.3M12 7.5l-2 2M12 7.5l2 2M12 16.5l-2-2M12 16.5l2-2"/>'
    return _S % (axis + '<g transform="rotate(60 12 12)">' + axis + '</g><g transform="rotate(120 12 12)">' + axis + '</g>')


ICONS = {
    'atom': _S % '<ellipse cx="12" cy="12" rx="10" ry="4"/><ellipse cx="12" cy="12" rx="10" ry="4" transform="rotate(60 12 12)"/><ellipse cx="12" cy="12" rx="10" ry="4" transform="rotate(120 12 12)"/><circle cx="12" cy="12" r="1.8" fill="currentColor"/>',
    'finger': _S % '<path d="M5.5 7.5A7.8 7.8 0 0 1 19.5 11v2"/><path d="M4.3 11a7.8 7.8 0 0 1 .5-2.5"/><path d="M7.2 20.5A10 10 0 0 1 4.3 13.5"/><path d="M12 6.6a4.6 4.6 0 0 0-4.6 4.6c0 3.2.9 6 2.6 8.4"/><path d="M16.6 11.2a4.6 4.6 0 0 0-1.4-3.3"/><path d="M12 11.2c0 3.4.9 6.4 2.6 9"/><path d="M16.6 14.4c0 1.9.3 3.7 1 5.3"/><path d="M9.6 11.2a2.4 2.4 0 0 1 4.8 0c0 2.6.6 5 1.6 7.2"/>',
    'spark': _S % '<path d="M12 2.5v4M12 17.5v4M2.5 12h4M17.5 12h4M5.3 5.3l2.8 2.8M15.9 15.9l2.8 2.8M18.7 5.3l-2.8 2.8M8.1 15.9l-2.8 2.8"/><path d="M12 8.5l1.2 2.3 2.3 1.2-2.3 1.2L12 15.5l-1.2-2.3-2.3-1.2 2.3-1.2z" fill="currentColor" stroke="none"/><circle cx="4" cy="19" r=".9" fill="currentColor" stroke="none"/><circle cx="20" cy="4.5" r=".9" fill="currentColor" stroke="none"/>',
    'person': _F % '<circle cx="12" cy="7.5" r="4.2"/><path d="M3.5 20.5a8.5 8.5 0 0 1 17 0V21h-17z"/>',
    'cycle': _S % '<path d="M4.5 12A7.5 7.5 0 0 1 17.2 6.6"/><path d="M17.5 3v3.8h-3.8"/><path d="M19.5 12A7.5 7.5 0 0 1 6.8 17.4"/><path d="M6.5 21v-3.8h3.8"/>',
    'snow': _snow(),
    'cloud': _F % '<path d="M7 19a4.5 4.5 0 0 1-.6-9 5.8 5.8 0 0 1 11.2-1.3A3.8 3.8 0 0 1 17.5 19H7z"/>',
    'flame': _F % '<path d="M12.5 2c.6 3.4 5 5.2 5 10.2a5.5 5.5 0 0 1-11 0c0-2 .9-3.6 2.1-4.7 0 1.6.6 2.7 1.6 3.2-.3-3.4.6-6 2.3-8.7z"/>',
    'crown': _F % '<path d="M3 7.5l4.6 4L12 5l4.4 6.5L21 7.5l-2 11.5H5L3 7.5z"/><rect x="5" y="20" width="14" height="1.6" rx=".8"/>',
    'hands': _F % '<path d="M2 8h4.2l2.4 2.4 2.3-2.3a2.2 2.2 0 0 1 3.1 0l3.3 3.3 4.7-4.7V16l-3.2 1.2-2.4 2.4a2.2 2.2 0 0 1-3.1 0l-1.2-1.2-1.3 1.3a2.2 2.2 0 0 1-3.1 0L5 17 2 16V8zm8 6.2l3.7 3.7"/>',
    'db': _S % '<ellipse cx="12" cy="5.5" rx="7.5" ry="3"/><path d="M4.5 5.5v6c0 1.7 3.4 3 7.5 3s7.5-1.3 7.5-3v-6"/><path d="M4.5 11.5v6c0 1.7 3.4 3 7.5 3s7.5-1.3 7.5-3v-6"/>',
    'head': _S % '<path d="M9.5 21.5v-3.2H7.6a1.6 1.6 0 0 1-1.6-1.6v-2.8L3.5 12.5l2.5-1.3V9.5a7 7 0 0 1 14 0c0 2.2-.9 4-2.3 5.3v6.7"/><circle cx="12.5" cy="9.5" r="2"/><circle cx="16" cy="12" r="1.3"/><path d="M12.5 6.3v1.2M12.5 11.5v1.2M9.3 9.5h1.2M14.5 9.5h1.2"/>',
}

LOGO = '<svg viewBox="0 0 48 48" aria-hidden="true"><rect x="5" y="5" width="38" height="38" rx="13" fill="none" stroke="url(#lg)" stroke-width="7" transform="rotate(-8 24 24)"/></svg>'

GLOBAL_DEFS = ('<defs><linearGradient id="lg" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#D9F3FF"/><stop offset=".55" stop-color="#C2CCFF"/><stop offset="1" stop-color="#8C6FF0"/></linearGradient></defs>')

A = '/templates/freeform/' + KEY + '/assets/'


def nlist(items, teal=False):  # قائمة مرقّمة: items = قائمة رموز العناوين (نص كل عنصر = §P_SAMPLE§)
    return ('<ol class="nl%s">' % (' teal' if teal else '')
            + ''.join('<li><span class="num">%02d</span><h2 class="h2">§%s§</h2><p class="p">§P_SAMPLE§</p></li>' % (i + 1, k) for i, k in enumerate(items))
            + '</ol>')


def icon_item(cls, icon, color, h, p):
    return '<div class="it %s"><span class="ic %s">%s</span><div class="tx"><h2 class="h2">§%s§</h2><p class="p">§%s§</p></div></div>' % (cls, color, ICONS[icon], h, p)


SLIDES = [
    # 1 الغلاف
    ('\n    <img class="bgp" alt="§ALT_BG§" src="%scover-bg.jpg">' % A
     + '\n    <div class="logo">' + LOGO + '</div><p class="logo-t">§LOGO1§<br>§LOGO2§</p>'
     + '\n    <h1 class="c1">§C1§</h1><h1 class="c2">§C2§</h1><p class="c3">§C3§</p><p class="c4">§P_SAMPLE§</p>'
     + '\n    <img class="avatar" alt="§ALT_TRAINER§" src="%strainer.jpg"><p class="name">§NAME§</p><p class="role">§ROLE§</p>\n  ' % A),
    # 2 الأجندة
    ('\n    <div class="panel"></div>'
     + '\n    <h1 class="ttl">§T2§</h1><p class="sub">§SUB§</p>'
     + '\n    <p class="lead">§LEAD2A§<br>§LEAD2B§</p>'
     + '\n    <ul class="bul"><li>§B1§</li><li>§B2§</li><li>§B3§</li></ul>'
     + '\n    ' + nlist(['A1', 'A2', 'A3', 'A4', 'A5']) + '\n  '),
    # 3 الإحصاءات
    ('\n    <h1 class="ttl light ctr">§T3§</h1><p class="sub lav ctr">§SUB§</p>'
     + ''.join('\n    <div class="card k%d"><p class="big">%s</p><h2 class="h2">§H_ADD§</h2><p class="p">§P_SAMPLE§</p></div>' % (i + 1, v) for i, v in enumerate(['79%', '$15.7T', '78%', '2.5X']))
     + '\n    <p class="bar">§BAR§</p>\n  '),
    # 4 الأساسيات
    ('\n    <div class="panel"></div>'
     + '\n    <h1 class="ttl light">§T4§</h1>'
     + '\n    ' + icon_item('i1', 'atom', 'pu', 'F1', 'P_MED')
     + '\n    ' + icon_item('i2', 'finger', 'bl', 'F2', 'P_MED')
     + '\n    ' + icon_item('i3', 'spark', 'tl', 'F3', 'P_MED')
     + '\n    <div class="cir c-big"></div><div class="cir c-mid"></div><div class="cir c-sm"></div>'
     + '\n    <div class="lbl l1"><h2 class="h2">§F1§</h2><p class="p">§H_ADD§</p></div>'
     + '\n    <div class="lbl l2"><h2 class="h2">§F2§</h2><p class="p">§H_ADD§</p></div>'
     + '\n    <div class="lbl l3"><h2 class="h2">§F3§</h2><p class="p">§H_ADD§</p></div>\n  '),
    # 5 الأقسام
    ('\n    <h1 class="ttl ctr">§T5§</h1><p class="sub ctr">§SUB§</p>'
     + '\n    <div class="panel"></div>'
     + ''.join('\n    <div class="it a%d"><span class="pic">%s</span><div class="tx"><h2 class="h2">§D%d§</h2><p class="p">§P_SHORT§</p></div></div>' % (i + 1, ICONS['person'], i + 1) for i in range(6))
     + '\n    <div class="vl v1"></div><div class="vl v2"></div><div class="hl"></div>'
     + '\n    <p class="src">§SRC§</p><p class="bar">§BAR§</p>\n  '),
    # 6 التعاون
    ('\n    <h1 class="ttl ctr">§T6§</h1><p class="sub ctr">§SUB§</p>'
     + ''.join('\n    <div class="card k%d"><div class="pic">%s</div><h2 class="h2">§C6_%d§</h2><p class="p">§P_SAMPLE§</p></div>' % (i + 1, ICONS[ic], i + 1) for i, ic in enumerate(['atom', 'cycle', 'snow', 'finger']))
     + '\n  '),
    # 7 مهارات المستقبل
    ('\n    <div class="hdr"></div>'
     + '\n    <h1 class="ttl light">§T7§</h1><p class="sub lav">§SUB§</p>'
     + '\n    ' + icon_item('r1 cA', 'cloud', 'pu', 'S1', 'P_LONG')
     + '\n    ' + icon_item('r2 cA', 'crown', 'pu', 'S2', 'P_LONG')
     + '\n    ' + icon_item('r3 cA', 'db', 'pu', 'S3', 'P_LONG')
     + '\n    ' + icon_item('r1 cB', 'flame', 'pu', 'S4', 'P_LONG2')
     + '\n    ' + icon_item('r2 cB', 'hands', 'pu', 'S5', 'P_LONG2')
     + '\n    ' + icon_item('r3 cB', 'head', 'pu', 'S6', 'P_LONG2') + '\n  '),
    # 8 الحوكمة
    ('\n    <h1 class="ttl">§T8§</h1><p class="sub">§SUB§</p>'
     + '\n    <div class="panel"></div>'
     + ''.join('\n    <div class="col g%d"><span class="num">%02d</span><h2 class="h2">§G%d§</h2><p class="p">§P_LONG§</p></div>' % (i + 1, i + 1, i + 1) for i in range(4))
     + '\n    <div class="vl v1"></div><div class="vl v2"></div><div class="vl v3"></div>\n  '),
    # 9 خارطة الطريق
    ('\n    <div class="hdr"></div>'
     + '\n    <h1 class="ttl light">§T9§</h1><p class="sub lav">§SUB§</p>'
     + '\n    <div class="line"></div><div class="arrow"></div><div class="dot d1"></div><div class="dot d2"></div><div class="dot d3"></div><div class="dot d4"></div>'
     + ''.join('\n    <div class="col g%d"><span class="yr">%s</span><h2 class="h2">§R%d§</h2><p class="p">§P_XL§</p></div>' % (i + 1, y, i + 1) for i, y in enumerate(['2021', '2025', '2030', '2035']))
     + '\n  '),
    # 10 الخلاصات
    ('\n    <h1 class="ttl light">§T10§</h1><p class="sub lav">§SUB§</p><p class="para">§P10§</p>'
     + '\n    <div class="logo">' + LOGO + '</div><p class="logo-t">§LOGO1§<br>§LOGO2§</p>'
     + '\n    ' + nlist(['K1', 'K2', 'K3', 'K4', 'K5'], teal=True) + '\n  '),
]

STR = {
    'ar': {
        'ALT_BG': 'خطوط ضوئية زرقاء تشبه شبكة عصبية', 'ALT_TRAINER': 'صورة المدرِّبة',
        'LOGO1': 'ضع شعار', 'LOGO2': 'شركتك هنا',
        'C1': 'الذكاء الاصطناعي في', 'C2': 'التدريب المؤسسي', 'C3': 'قالب عرض تقديمي',
        'NAME': 'إيميلي رودريغيز', 'ROLE': 'مدرِّبة',
        'SUB': 'يمكنك إضافة عنوان فرعي هنا',
        'P_SAMPLE': 'هذا نص توضيحي. أضف النص المطلوب هنا.',
        'P_SHORT': 'هذا نص توضيحي هنا.',
        'P_MED': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصك.',
        'P_LONG': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصك. هذا نص.',
        'P_LONG2': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصك. هذا نص توضيحي.',
        'P_XL': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصك. هذا نص توضيحي. أضف النص المطلوب هنا.',
        'P10': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصك. هذا نص. هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصك. هذا نص توضيحي.',
        'BAR': 'هذا نص توضيحي. أضف النص المطلوب هنا. أضف نصك. هذا نص توضيحي. أدخل نصك هنا.',
        'H_ADD': 'أضف نصًا هنا', 'SRC': 'المصدر:',
        'T2': 'الأجندة<br>وأهداف<br>التعلّم', 'LEAD2A': 'بنهاية هذا التدريب،', 'LEAD2B': 'ستكون قادرًا على:',
        'B1': 'هذا نص توضيحي. أضف النص المطلوب هنا.', 'B2': 'أضف نصك. أضف النص المطلوب هنا. أضف نصك. هذا نص.', 'B3': 'أضف النص المطلوب هنا.',
        'A1': 'أساسيات الذكاء الاصطناعي', 'A2': 'الذكاء الاصطناعي في بيئة العمل', 'A3': 'التعاون بين الإنسان والذكاء الاصطناعي', 'A4': 'مهارات المستقبل وعقليته', 'A5': 'الذكاء الاصطناعي المسؤول وتبنّيه',
        'T3': 'لماذا يهمّ الذكاء الاصطناعي الآن',
        'T4': 'فهم أساسيات<br>الذكاء الاصطناعي', 'F1': 'الذكاء الاصطناعي', 'F2': 'تعلّم الآلة', 'F3': 'الذكاء الاصطناعي التوليدي',
        'T5': 'فهم أساسيات الذكاء الاصطناعي', 'D1': 'التسويق', 'D2': 'الموارد البشرية', 'D3': 'المالية', 'D4': 'العمليات', 'D5': 'المبيعات', 'D6': 'خدمة العملاء',
        'T6': 'التعاون بين الإنسان والذكاء الاصطناعي', 'C6_1': 'الذكاء الاصطناعي يتلقّى', 'C6_2': 'أنت تقيّم', 'C6_3': 'أنت تقرّر', 'C6_4': 'الذكاء الاصطناعي يسرّع',
        'T7': 'مهارات المستقبل لعصر الذكاء الاصطناعي', 'S1': 'الإلمام بالذكاء الاصطناعي', 'S2': 'التفكير النقدي', 'S3': 'الطلاقة في البيانات', 'S4': 'الإبداع', 'S5': 'القدرة على التكيّف', 'S6': 'سرعة التعلّم',
        'T8': 'الذكاء الاصطناعي المسؤول<br>والحوكمة المؤسسية', 'G1': 'الاستخدام الأخلاقي', 'G2': 'الخصوصية والأمن', 'G3': 'المساءلة', 'G4': 'الامتثال',
        'T9': 'خارطة طريق تبنّي الذكاء الاصطناعي', 'R1': 'الجاهزية', 'R2': 'التجربة', 'R3': 'التبنّي', 'R4': 'التحسين',
        'T10': 'أبرز الخلاصات<br>والخطوات التالية', 'K1': 'الذكاء الاصطناعي هنا', 'K2': 'الذكاء الاصطناعي يعزّز', 'K3': 'المهارات المناسبة', 'K4': 'هذه تكفي', 'K5': 'المسؤولية',
    },
    'en': {
        'ALT_BG': 'Blue glowing lines resembling a neural network', 'ALT_TRAINER': 'Trainer portrait',
        'LOGO1': 'Your company', 'LOGO2': 'logo here',
        'C1': 'AI CORPORATE', 'C2': 'TRAINING', 'C3': 'PRESENTATION TEMPLATE',
        'NAME': 'Emily Rodriguez', 'ROLE': 'Trainer',
        'SUB': 'You can add a subheading here',
        'P_SAMPLE': 'This is a sample text. Insert your desired text here.',
        'P_SHORT': 'This is a sample text here.',
        'P_MED': 'This is a sample text. Insert your desired text here. Add your text.',
        'P_LONG': 'This is a sample text. Insert your desired text here. Add your text. This is a text.',
        'P_LONG2': 'This is a sample text. Insert your desired text here. Add your text. This is a sample text.',
        'P_XL': 'This is a sample text. Insert your desired text here. Add your text. This is a sample text. Insert your desired text here.',
        'P10': 'This is a sample text. Insert your desired text here. Add your text. This is a text. This is a sample text. Insert your desired text here. Add your text. This is a sample text.',
        'BAR': 'This is a sample text. Insert your desired text here. Add your text. This is a sample text. Insert your text here.',
        'H_ADD': 'Add text here', 'SRC': 'SOURCE:',
        'T2': 'Agenda<br>&amp; Learning<br>Objectives', 'LEAD2A': 'By the end of this training,', 'LEAD2B': 'you will be able to:',
        'B1': 'This is a sample text. Insert your desired text here.', 'B2': 'Add your text. Insert your desired text here. Add your text. This is a text.', 'B3': 'Insert your desired text here.',
        'A1': 'AI Fundamentals', 'A2': 'AI in the workplace', 'A3': 'Human + AI Collaboration', 'A4': 'Future skills &amp; mindset', 'A5': 'Responsible AI &amp; Adoption',
        'T3': 'Why AI Matters Now',
        'T4': 'Understanding<br>AI Fundamentals', 'F1': 'Artificial Intelligence', 'F2': 'Machine Learning', 'F3': 'Generative AI',
        'T5': 'Understanding AI Fundamentals', 'D1': 'Marketing', 'D2': 'HR', 'D3': 'Finance', 'D4': 'Operations', 'D5': 'Sales', 'D6': 'Customer Service',
        'T6': 'Human + AI Collaboration', 'C6_1': 'AI Accepts', 'C6_2': 'You Evaluate', 'C6_3': 'You Decide', 'C6_4': 'AI Accelerates',
        'T7': 'Future Skills for the AI Era', 'S1': 'AI Literacy', 'S2': 'Critical Thinking', 'S3': 'Data Fluency', 'S4': 'Creativity', 'S5': 'Adaptability', 'S6': 'Fast Learning',
        'T8': 'Responsible AI &amp;<br>Corporate Governance', 'G1': 'Ethical Use', 'G2': 'Privacy &amp; Security', 'G3': 'Accountability', 'G4': 'Compliance',
        'T9': 'AI Adoption Roadmap', 'R1': 'Readiness', 'R2': 'Experiment', 'R3': 'Adopt', 'R4': 'Optimize',
        'T10': 'Key Takeaways<br>&amp; Next Steps', 'K1': 'AI Is Here', 'K2': 'AI Augments', 'K3': 'The Right Skills', 'K4': 'These Will Do', 'K5': 'Responsible',
    },
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'hr', 'icon': '🤖',
    'name': 'التدريب المؤسسي بالذكاء الاصطناعي', 'nameEn': 'AI Corporate Training',
    'description': 'عشر شرائح: غلاف، أجندة، إحصاءات، أساسيات الذكاء الاصطناعي، أقسام الشركة، التعاون، المهارات، الحوكمة، خارطة طريق، خلاصات',
    'descriptionEn': 'Ten slides: cover, agenda, stats, AI fundamentals, departments, collaboration, skills, governance, roadmap, takeaways',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'الأجندة وأهداف التعلّم', 'titleEn': 'Agenda & Learning Objectives'},
        {'title': 'لماذا يهمّ الذكاء الاصطناعي الآن (إحصاءات)', 'titleEn': 'Why AI Matters Now (stats)', 'needsData': True},
        {'title': 'فهم أساسيات الذكاء الاصطناعي', 'titleEn': 'Understanding AI Fundamentals'},
        {'title': 'الذكاء الاصطناعي في أقسام الشركة', 'titleEn': 'AI Across Departments'},
        {'title': 'التعاون بين الإنسان والذكاء الاصطناعي', 'titleEn': 'Human + AI Collaboration'},
        {'title': 'مهارات المستقبل لعصر الذكاء الاصطناعي', 'titleEn': 'Future Skills for the AI Era'},
        {'title': 'الذكاء الاصطناعي المسؤول والحوكمة', 'titleEn': 'Responsible AI & Governance'},
        {'title': 'خارطة طريق التبنّي (خط زمني)', 'titleEn': 'AI Adoption Roadmap (timeline)', 'needsData': True},
        {'title': 'أبرز الخلاصات والخطوات التالية', 'titleEn': 'Key Takeaways & Next Steps'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والألوان وغيّر النصوص والأرقام فقط (الإحصاءات في الشريحة 3 والسنوات في الشريحة 9)؛ الصور في assets تبقى كما هي، والأيقونات SVG مضمّنة لا تُغيَّر.',
}
