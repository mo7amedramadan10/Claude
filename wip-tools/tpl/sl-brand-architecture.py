# -*- coding: utf-8 -*-
# نموذج «هيكلية العلامة التجارية»: مخطط شجري بشريط علوي داكن وأربعة أنواع (البيت الموحّد، العلامة المدعومة، بيت العلامات،
# الهيكلية الهجينة)، لكل نوع دائرة أيقونة وبطاقة بعنوان ملوّن وبطاقتان فرعيتان مرقّمتان. الشريحة الأولى فاتحة والثانية داكنة.
KEY = 'sl-brand-architecture'
TITLE = {'ar': 'قالب هيكلية العلامة التجارية', 'en': 'Brand Architecture PowerPoint Template'}
COMMENT = 'نموذج «هيكلية العلامة التجارية»: شريط علوي كحلي بأيقونة، وخطوط وصل رمادية إلى أربع مجموعات ملوّنة (أزرق، أخضر مزرق، برتقالي، وردي)؛ كل مجموعة دائرة أيقونة وبطاقة رئيسية وبطاقتان فرعيتان مرقّمتان. الصنف dark على الإطار للنسخة الداكنة.'
VARS = '--ink: #1E1E1E; --ink-2: #333333; --accent: #1A77F0; --navy: #153254; --line: #C4C4C4; --bg: #FFFFFF;'

STYLE = """  .s2 { --bg: #10263E; --line: #B1C2D2; }
  .ttl { position: absolute; top: 42px; inset-inline-start: 64px; margin: 0; font-size: 41.5px; font-weight: 400; line-height: 1.2; color: var(--ink); white-space: nowrap; }
  html[lang="ar"] .ttl { font-weight: 500; font-size: 44px; top: 38px; }
  .s2 .ttl { color: #FFFFFF; }
  /* الشريط العلوي */
  .pill { position: absolute; top: 128px; inset-inline-start: 417px; width: 445px; height: 61px; border-radius: 31px; background: var(--navy); color: #fff; display: flex; align-items: center; }
  .s2 .pill { background: #173458; }
  .pill .pi { width: 42px; height: 42px; margin-inline-start: 28px; flex: none; }
  .pill .sep { width: 1.5px; height: 38px; background: rgb(255 255 255 / .85); margin-inline: 13px 14px; flex: none; }
  .pill h2 { margin: 0; font-size: 22px; font-weight: 700; line-height: 1; }
  /* خطوط الوصل */
  .ln { position: absolute; border: 0 solid var(--line); }
  .ln.l1 { top: 161px; inset-inline-start: 196px; width: 221px; height: 52px; border-top-width: 1.5px; border-inline-start-width: 1.5px; border-start-start-radius: 10px; }
  .ln.l4 { top: 161px; inset-inline-start: 862px; width: 223px; height: 52px; border-top-width: 1.5px; border-inline-end-width: 1.5px; border-start-end-radius: 10px; }
  .ln.l2 { top: 195px; inset-inline-start: 493px; height: 18px; border-inline-start-width: 1.5px; }
  .ln.l3 { top: 195px; inset-inline-start: 788px; height: 18px; border-inline-start-width: 1.5px; }
  /* المجموعة: ألوانها في متغيّرات */
  .g { position: absolute; top: 224px; width: 265px; height: 434px; }
  .g1 { inset-inline-start: 64px; --c1: #2079F7; --c2: #0A56C2; --h1: #1D7AF7; --h2: #1A70EC; --soft: #D3E4FD; --bd: #A9C7F0; --ic: #0E5ED0; }
  .g2 { inset-inline-start: 360px; --c1: #27A993; --c2: #1A7D70; --h1: #23A794; --h2: #1A7B6D; --soft: #CCF5ED; --bd: #96E1D1; --ic: #1E7F70; }
  .g3 { inset-inline-start: 656px; --c1: #FB8B3B; --c2: #DC5D0A; --h1: #F98A38; --h2: #E05F10; --soft: #FEE7D7; --bd: #F6CCAD; --ic: #F07A2B; }
  .g4 { inset-inline-start: 952px; --c1: #F7426D; --c2: #E0103F; --h1: #F4406B; --h2: #E10F40; --soft: #FDDAE1; --bd: #F4B6C6; --ic: #EA3B67; }
  .g .cir { position: absolute; top: 0; inset-inline-start: 100px; width: 64px; height: 64px; border-radius: 50%; background: linear-gradient(180deg, var(--c1), var(--c2)); color: #fff; display: grid; place-items: center; }
  .g .cir svg { width: 34px; height: 34px; }
  .g .card { position: absolute; top: 78px; inset-inline-start: 26px; width: 212px; height: 153px; border-radius: 14px; background: var(--soft); border: 1.5px solid var(--bd); overflow: hidden; }
  .g .card h3 { margin: 0; height: 60px; display: grid; place-items: center; background: linear-gradient(180deg, var(--h1), var(--h2)); color: #fff; font-size: 18px; font-weight: 700; line-height: 1.2; text-align: center; padding-inline: 8px; }
  .g .card p { margin: 0; padding: 10px 18px 0; font-size: 18.5px; line-height: 1.24; color: var(--ink-2); text-align: center; }
  .g .stem { position: absolute; top: 239px; inset-inline-start: 132px; height: 19px; border-inline-start: 1.5px solid var(--line); }
  .g .fork { position: absolute; top: 257px; inset-inline-start: 63px; width: 139px; height: 26px; border: 1.5px solid var(--line); border-bottom: 0; border-start-start-radius: 8px; border-start-end-radius: 8px; }
  .g .sub { position: absolute; top: 291px; width: 127px; height: 142px; border-radius: 10px; background: var(--soft); border: 1.5px solid var(--bd); text-align: center; }
  .g .sub.a { inset-inline-start: 0; } .g .sub.b { inset-inline-start: 138px; }
  .g .num { position: absolute; top: -17px; inset-inline-start: 46px; width: 34px; height: 34px; border-radius: 50%; background: linear-gradient(180deg, var(--c1), var(--c2)); color: #fff; font-size: 15px; font-weight: 700; display: grid; place-items: center; line-height: 1; }
  .g .sub .si { position: absolute; top: 27px; inset-inline-start: 41px; width: 44px; height: 44px; color: var(--ic); }
  .g .sub p { position: absolute; top: 73px; inset-inline: 12px; margin: 0; font-size: 15px; line-height: 1.17; color: var(--ink-2); }
  html[lang="ar"] .g .sub p { top: 76px; line-height: 1.35; }
  html[lang="ar"] .g .card p { line-height: 1.4; padding-top: 12px; font-size: 19px; }"""

# أيقونات مبسّطة (24×24)
ICONS = {
    'building': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.1" aria-hidden="true"><rect x="2.5" y="7" width="6" height="14.5"/><rect x="8.5" y="3" width="7" height="18.5"/><rect x="15.5" y="9" width="6" height="12.5"/><path d="M4.3 9.5h1.2M6.4 9.5h.8M4.3 12h1.2M6.4 12h.8M4.3 14.5h1.2M6.4 14.5h.8M4.3 17h1.2M6.4 17h.8M10.4 5.5h1.2M12.6 5.5h1.2M10.4 8h1.2M12.6 8h1.2M10.4 10.5h1.2M12.6 10.5h1.2M10.4 13h1.2M12.6 13h1.2M10.4 15.5h1.2M12.6 15.5h1.2M17.3 11.5h1.2M19.4 11.5h.8M17.3 14h1.2M19.4 14h.8M17.3 16.5h1.2M19.4 16.5h.8M11 21.5v-3h2v3"/></svg>',
    'crown': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><circle cx="12" cy="4.2" r="1.1"/><circle cx="3.4" cy="8" r="1"/><circle cx="20.6" cy="8" r="1"/><path d="M4 9.5l4.2 3.6L12 6.2l3.8 6.9L20 9.5l-1.6 7.3H5.6z"/><rect x="5.6" y="17.8" width="12.8" height="2" rx=".4"/></svg>',
    'layers': '<svg viewBox="1.5 1.5 21 21" fill="currentColor" aria-hidden="true"><path d="M12 4.5l9 4.7-9 4.7-9-4.7z"/><path d="M5.2 12.4L3 13.6l9 4.8 9-4.8-2.2-1.2-6.8 3.6z"/></svg>',
    'shield': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linejoin="round" aria-hidden="true"><path d="M12 2.8l7.5 2.8v5.6c0 4.8-3.2 8.6-7.5 10-4.3-1.4-7.5-5.2-7.5-10V5.6z"/><path d="M12 5.6l5 1.9v3.8c0 3.3-2.1 5.9-5 6.9-2.9-1-5-3.6-5-6.9V7.5z" stroke-width="1.2"/><path d="M9.3 11.6l1.9 1.9 3.6-3.7"/></svg>',
    'link': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.3" stroke-linecap="round" aria-hidden="true"><rect x="2.6" y="5.2" width="11" height="6.2" rx="3.1" transform="rotate(45 8.1 8.3)"/><rect x="10.4" y="12.6" width="11" height="6.2" rx="3.1" transform="rotate(45 15.9 15.7)"/></svg>',
    'people': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><circle cx="12" cy="7.6" r="2.7"/><circle cx="5.2" cy="8.8" r="2.2"/><circle cx="18.8" cy="8.8" r="2.2"/><path d="M6.6 17.2c0-3 2.4-5.2 5.4-5.2s5.4 2.2 5.4 5.2z"/><path d="M5.2 11.7c-2.4 0-4.2 1.6-4.2 3.8v.5h4.4c.2-1.6 1-3 2.2-4a4.8 4.8 0 0 0-2.4-.3z"/><path d="M18.8 11.7c-.8 0-1.7.1-2.4.3 1.2 1 2 2.4 2.2 4H23v-.5c0-2.2-1.8-3.8-4.2-3.8z"/></svg>',
    'tag': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M4.5 3.2l7.2.6 9 9a1.6 1.6 0 0 1 0 2.3l-5.6 5.6a1.6 1.6 0 0 1-2.3 0l-9-9-.6-7.2a1.2 1.2 0 0 1 1.3-1.3zm2.6 2.4a1.5 1.5 0 1 0 0 3 1.5 1.5 0 0 0 0-3z"/><path d="M11.6 11.2l4.6 4.6M13.2 9.6l4.6 4.6M10 12.8l4.6 4.6" stroke="#fff" stroke-width="1" opacity=".9"/></svg>',
    'target': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true"><circle cx="11" cy="13" r="9"/><circle cx="11" cy="13" r="5.6"/><circle cx="11" cy="13" r="2.2" fill="currentColor"/><path d="M11 13l8.5-8.5" stroke-linecap="round"/><path d="M17.6 2.6l.4 3.4 3.4.4-2.2 2.2-3.2-.4-.4-3.2z" fill="currentColor" stroke="none"/></svg>',
    'medal': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M6.5 1.8h3.8l2.4 6.4-1.9 1.7zM17.5 1.8h-3.8l-1.3 3.4 1.6 4.3z"/><circle cx="12" cy="15.6" r="6.3"/><circle cx="12" cy="15.6" r="3.6" fill="none" stroke="#FEE7D7" stroke-width="1.6"/></svg>',
    'star': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12 1.8l3 7 7.6.6-5.8 5 1.8 7.4L12 17.8l-6.6 4 1.8-7.4-5.8-5 7.6-.6z"/></svg>',
    'globe': '<svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="10" fill="none" stroke="currentColor" stroke-width="1.8"/><path fill="currentColor" d="M5 6.2c1.6-1.6 3.6-2.5 5.8-2.6l.6 1.6-1.8 1.4.4 1.8-2.2.8-.2 2 1.6 1.6-.4 2.4 1.2 2.6-1.8.4-2.2-3-.8-3.6zM13.6 4c2.6.4 4.8 2 6 4.2l-1.6.4-1.6-1.4-1.8.6.4 2 2.4 1.2-.2 2.6-1.8 2.6-.6 2.6-2 .6.4-3-1.6-2 .4-2.4 1.8-.8-.8-2-1.6-1z"/></svg>',
    'lock': '<svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M7 10V7.4a5 5 0 0 1 10 0V10h-2V7.4a3 3 0 0 0-6 0V10z"/><rect x="4" y="10" width="16" height="12" rx="1.6"/><circle cx="12" cy="15.2" r="1.7" fill="#FDDAE1"/><rect x="11.3" y="15.5" width="1.4" height="3.3" rx=".5" fill="#FDDAE1"/></svg>',
}

GROUPS = [('crown', 'people', 'tag'), ('layers', 'target', 'link'), ('shield', 'medal', 'star'), ('link', 'globe', 'lock')]


def group(n, icons):
    big, a, b = icons
    return ('<div class="g g%d"><div class="cir">%s</div>' % (n, ICONS[big])
            + '<div class="card"><h3>§H%d§</h3><p>§P§</p></div><div class="stem"></div><div class="fork"></div>' % n
            + '<div class="sub a"><span class="num">01</span><span class="si">%s</span><p>§Q§</p></div>' % ICONS[a]
            + '<div class="sub b"><span class="num">02</span><span class="si">%s</span><p>§Q§</p></div></div>' % ICONS[b])


def slide():
    return ('\n    <h1 class="ttl">§TTL§</h1>\n    <div class="ln l1"></div><div class="ln l2"></div><div class="ln l3"></div><div class="ln l4"></div>'
            + '\n    <div class="pill"><span class="pi">%s</span><span class="sep"></span><h2>§PILL§</h2></div>\n    ' % ICONS['building']
            + '\n    '.join(group(i + 1, g) for i, g in enumerate(GROUPS)) + '\n  ')


SLIDES = [slide(), slide()]

STR = {
    'ar': {'TTL': 'قالب هيكلية العلامة التجارية', 'PILL': 'هيكلية العلامة التجارية',
           'H1': 'العلامة الأم الموحّدة', 'H2': 'العلامة المدعومة', 'H3': 'بيت العلامات', 'H4': 'الهيكلية الهجينة',
           'P': 'أدخل النص المطلوب هنا.', 'Q': 'أدخل النص المطلوب هنا.'},
    'en': {'TTL': 'Brand Architecture PowerPoint Template', 'PILL': 'Brand Architecture',
           'H1': 'Branded House', 'H2': 'Endorsed Brand', 'H3': 'House of Brands', 'H4': 'Hybrid Architecture',
           'P': 'Insert your desired text here.', 'Q': 'Insert your desired text here.'},
}

FONTS = {'ar': '<link href="https://fonts.googleapis.com/css2?family=Tajawal:wght@400;500;700;800&display=swap" rel="stylesheet">',
         'en': '<link href="https://fonts.googleapis.com/css2?family=Open+Sans:wght@400;600;700&display=swap" rel="stylesheet">'}
FONT_STACK = {'ar': "'Tajawal', 'Segoe UI', Tahoma, sans-serif", 'en': "'Open Sans', 'Segoe UI', Arial, sans-serif"}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'marketing', 'icon': '🏷️',
    'name': 'هيكلية العلامة التجارية', 'nameEn': 'Brand Architecture',
    'description': 'شريحتان (فاتحة وداكنة) لمخطط شجري بأربعة أنماط لهيكلية العلامة، لكل نمط بطاقة وبندان فرعيان',
    'descriptionEn': 'Two slides (light and dark) of a tree chart with four brand architecture types, each with two sub-items',
    'sections': [
        {'title': 'هيكلية العلامة (فاتحة)', 'titleEn': 'Brand Architecture (Light)'},
        {'title': 'هيكلية العلامة (داكنة)', 'titleEn': 'Brand Architecture (Dark)'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والألوان وغيّر النصوص فقط؛ الشريحة الثانية نسخة داكنة من الأولى بالمحتوى نفسه.',
}
