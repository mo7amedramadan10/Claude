# -*- coding: utf-8 -*-
# نموذج «إعادة تصميم علامة قهوة»: شريحة واحدة تعرض الهوية النهائية في ست صور (تغليف، حساب إنستغرام، الموقع، كوب، بطاقة عمل، منشور)
# بتسميات تحت صفوف الصور العليا وشريط بنّي داكن يحمل العنوان.
KEY = 'sl-coffee-rebranding'
TITLE = {'ar': 'إعادة تصميم علامة قهوة', 'en': 'Coffee Rebranding'}
COMMENT = 'نموذج «إعادة تصميم علامة قهوة»: ست صور فوتوغرافية في صفين (الصف العلوي بتسميات تحته)، ثم شريط بنّي داكن بعنوان أبيض بخط مذيّل. في العربية ينعكس ترتيب الصور مع تسمياتها.'
VARS = '--ink: #2E2A25; --accent: #3C1F11; --brown: #3C1F11; --sand: #DCCCBD; --sand-2: #D6C8BC; --rule: #B3A08F; --bg: #DCCCBD;'

FONTS = {
    'ar': '<link href="https://fonts.googleapis.com/css2?family=Amiri:wght@400;700&display=swap" rel="stylesheet">',
    'en': '<link href="https://fonts.googleapis.com/css2?family=Source+Serif+4:opsz,wght@8..60,400;8..60,600;8..60,700&display=swap" rel="stylesheet">',
}
FONT_STACK = {'ar': "'Amiri', 'Times New Roman', serif", 'en': "'Source Serif 4', Cambria, Georgia, serif"}

STYLE = """  /* الصور بإحداثيات الأصل؛ inset-inline-start يعكس ترتيبها في العربية */
  .s1 .ph { position: absolute; margin: 0; display: block; object-fit: cover; }
  .s1 .p1 { top: 0; inset-inline-start: 0; width: 427px; height: 340px; }
  .s1 .p2 { top: 0; inset-inline-start: 428px; width: 292px; height: 340px; }
  .s1 .p3 { top: 0; inset-inline-start: 720px; width: 560px; height: 340px; }
  .s1 .p4 { top: 365px; inset-inline-start: 0; width: 427px; height: 222px; }
  .s1 .p5 { top: 365px; inset-inline-start: 428px; width: 391px; height: 222px; }
  .s1 .p6 { top: 365px; inset-inline-start: 820px; width: 460px; height: 222px; }
  /* فاصل رأسي رفيع بين الصور كما في الأصل */
  .s1 .vr { position: absolute; width: 1px; background: rgb(60 40 25 / .25); }
  .s1 .vr1 { top: 0; height: 365px; inset-inline-start: 427px; }
  .s1 .vr2 { top: 365px; height: 222px; inset-inline-start: 427px; }
  .s1 .vr3 { top: 365px; height: 222px; inset-inline-start: 819px; }
  /* شريط التسميات أسفل الصف العلوي */
  .s1 .band { position: absolute; top: 340px; inset-inline-start: 0; width: 1280px; height: 25px; background: linear-gradient(to bottom, var(--sand), #DCCCBC 85%, var(--rule)); }
  .s1 .band-a { position: absolute; top: 340px; inset-inline-start: 0; width: 427px; height: 23px; background: var(--sand-2); }
  .s1 .cap { position: absolute; top: 340px; width: 260px; height: 23px; margin: 0; display: flex; align-items: center; justify-content: center; font-size: 14.5px; font-weight: 400; letter-spacing: 0.6px; color: var(--ink); text-transform: uppercase; white-space: nowrap; }
  html[lang="ar"] .s1 .cap { font-size: 16px; letter-spacing: 0; font-weight: 700; }
  .s1 .c1 { inset-inline-start: 82px; } .s1 .c2 { inset-inline-start: 458px; } .s1 .c3 { inset-inline-start: 856px; }
  /* الشريط البنّي والعنوان */
  .s1 .foot { position: absolute; top: 587px; inset-inline-start: 0; width: 1280px; height: 133px; background: var(--brown); }
  .s1 .ttl { position: absolute; top: 611px; inset-inline-start: 0; width: 1280px; margin: 0; text-align: center; font-size: 60px; font-weight: 700; line-height: 80px; color: #FFFFFF; letter-spacing: -0.3px; }
  html[lang="ar"] .s1 .ttl { font-size: 58px; line-height: 80px; top: 607px; letter-spacing: 0; }"""

SLIDES = ['''
    <img class="ph p1" src="/templates/freeform/sl-coffee-rebranding/assets/packaging.jpg" alt="§ALT1§">
    <img class="ph p2" src="/templates/freeform/sl-coffee-rebranding/assets/instagram.jpg" alt="§ALT2§">
    <img class="ph p3" src="/templates/freeform/sl-coffee-rebranding/assets/website.jpg" alt="§ALT3§">
    <div class="band" aria-hidden="true"></div><div class="band-a" aria-hidden="true"></div>
    <p class="cap c1">§CAP1§</p><p class="cap c2">§CAP2§</p><p class="cap c3">§CAP3§</p>
    <img class="ph p4" src="/templates/freeform/sl-coffee-rebranding/assets/mug.jpg" alt="§ALT4§">
    <img class="ph p5" src="/templates/freeform/sl-coffee-rebranding/assets/business-card.jpg" alt="§ALT5§">
    <img class="ph p6" src="/templates/freeform/sl-coffee-rebranding/assets/social-post.jpg" alt="§ALT6§">
    <span class="vr vr1" aria-hidden="true"></span><span class="vr vr2" aria-hidden="true"></span><span class="vr vr3" aria-hidden="true"></span>
    <div class="foot" aria-hidden="true"></div>
    <h1 class="ttl">§TTL§</h1>
  ''']

STR = {
    'ar': {'TTL': 'العرض النهائي للعلامة التجارية',
           'CAP1': 'التغليف', 'CAP2': 'حساب إنستغرام', 'CAP3': 'الصفحة الرئيسية للموقع',
           'ALT1': 'كيس قهوة بالهوية الجديدة', 'ALT2': 'هاتف يعرض حساب العلامة على إنستغرام', 'ALT3': 'حاسوب محمول يعرض الصفحة الرئيسية للموقع',
           'ALT4': 'كوب قهوة بشعار العلامة', 'ALT5': 'بطاقتا عمل بالهوية الجديدة', 'ALT6': 'منشور ترويجي للعلامة'},
    'en': {'TTL': 'Final Brand Showcase',
           'CAP1': 'PACKAGING', 'CAP2': 'INSTAGRAM PROFILE', 'CAP3': 'WEBSITE HOMEPAGE',
           'ALT1': 'Coffee bag with the new identity', 'ALT2': 'Phone showing the brand Instagram profile', 'ALT3': 'Laptop showing the website homepage',
           'ALT4': 'Coffee mug with the brand logo', 'ALT5': 'Business cards with the new identity', 'ALT6': 'Brand promotional post'},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'marketing', 'icon': '☕',
    'name': 'إعادة تصميم علامة قهوة', 'nameEn': 'Coffee Rebranding',
    'description': 'شريحة واحدة تعرض الهوية النهائية للعلامة في ست صور: التغليف والحساب والموقع والكوب وبطاقة العمل والمنشور',
    'descriptionEn': 'One slide showcasing the final brand in six photos: packaging, Instagram, website, mug, card and post',
    'sections': [
        {'title': 'العرض النهائي للعلامة التجارية', 'titleEn': 'Final Brand Showcase'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص فقط (العنوان والتسميات الثلاث)؛ الصور الست مقصوصة من الأصل ويمكن استبدال مساراتها بصور العلامة.',
}
