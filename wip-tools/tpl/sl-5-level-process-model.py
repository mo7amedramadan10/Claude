# -*- coding: utf-8 -*-
# نموذج «نموذج عملية بخمسة مستويات»: شريحتان بالتخطيط نفسه (فاتحة وداكنة) — جدول بعمود تسميات وخمسة أعمدة
# برؤوس ملوّنة متدرّجة الارتفاع كالدرج ودائرة مرقّمة فوق كل رأس، وأربعة صفوف، وشريط أسهم (أدوار) أسفل كل عمود.
KEY = 'sl-5-level-process-model'
TITLE = {'ar': 'نموذج عملية بخمسة مستويات', 'en': '5-Level Process Model'}
COMMENT = 'نموذج «نموذج عملية بخمسة مستويات»: عنوان أعلى، عمود تسميات داكن وخمسة أعمدة برؤوس ملوّنة متصاعدة كالدرج مع دائرة مرقّمة، أربعة صفوف خلايا رمادية، وأسهم أدوار ملوّنة أسفل الأعمدة. الشريحة الثانية نسخة داكنة الخلفية.'
VARS = '--ink: #1B1B1B; --ink-2: #3A3A3A; --accent: #72AFDB; --c1: #72AFDB; --c2: #F24F52; --c3: #FA772F; --c4: #906DD5; --c5: #8053B2; --cell: #F2F2F2; --label: #595959; --dark: #262626; --bg: #FFFFFF;'

STYLE = """  .ttl { position: absolute; top: 44px; inset-inline-start: 65px; margin: 0; font-size: 40px; font-weight: 300; line-height: 1.2; color: var(--ink); letter-spacing: -0.3px; }
  html[lang="ar"] .ttl { font-weight: 400; letter-spacing: 0; }
  .frame.s2 { background: var(--dark); } .s2 .ttl { color: #fff; }
  /* الأعمدة الخمسة: لون وموضع */
  .k1 { --c: var(--c1); --x: 257px; } .ch.k1 { --c: #5DB6DE; } .k2 { --c: var(--c2); --x: 450px; } .k3 { --c: var(--c3); --x: 643px; } .k4 { --c: var(--c4); --x: 836px; } .k5 { --c: var(--c5); --x: 1029px; }
  /* رأس العمود المتدرّج */
  .hd { position: absolute; inset-inline-start: var(--x); width: 186px; top: var(--t); height: calc(307px - var(--t)); background: var(--c); }
  .k1.hd { --t: 239px; } .k2.hd { --t: 211px; } .k3.hd { --t: 184px; } .k4.hd { --t: 156px; } .k5.hd { --t: 129px; }
  .hd h2 { position: absolute; bottom: 13px; inset-inline: 0; margin: 0; text-align: center; color: #fff; font-size: 24px; font-weight: 700; line-height: 1.2; }
  .hd .num { position: absolute; top: -38px; inset-inline-start: 63px; width: 60px; height: 60px; border-radius: 50%; background: #fff; border: 3px solid var(--c); color: var(--c); font-size: 20px; font-weight: 700; display: grid; place-items: center; line-height: 1; }
  /* الصفوف */
  .r1 { --y: 313px; } .r2 { --y: 388px; } .r3 { --y: 463px; } .r4 { --y: 538px; }
  .lb { position: absolute; top: var(--y); inset-inline-start: 64px; width: 186px; height: 68px; margin: 0; background: var(--label); color: #fff; font-size: 18px; line-height: 1.2; padding-inline-start: 11px; display: flex; align-items: center; }
  .cell { position: absolute; top: var(--y); inset-inline-start: var(--x); width: 186px; height: 68px; margin: 0; background: var(--cell); color: var(--ink-2); font-size: 15px; line-height: 19px; padding: 0 15px; display: flex; align-items: center; }
  /* أسهم الأدوار: مستطيل برأس مدبّب في جهة النهاية؛ يُعكس للعربية */
  .ch { position: absolute; top: 613px; inset-inline-start: var(--x); width: 186px; height: 50px; margin: 0; background: var(--c); clip-path: polygon(0 0, calc(100% - 12px) 0, 100% 50%, calc(100% - 12px) 100%, 0 100%); color: #fff; font-size: 17px; font-weight: 700; line-height: 1.2; display: flex; align-items: center; justify-content: center; padding-inline-end: 12px; }
  html[dir="rtl"] .ch { clip-path: polygon(12px 0, 100% 0, 100% 100%, 12px 100%, 0 50%); }"""


def slide():
    h = '\n    <h1 class="ttl">§TTL§</h1>\n'
    for k in range(1, 6):
        h += '    <div class="hd k%d"><span class="num">0%d</span><h2>§HD§</h2></div>\n' % (k, k)
    for r in range(1, 5):
        h += '    <p class="lb r%d">§LB§</p>' % r
        h += ''.join('<p class="cell r%d k%d">§C§</p>' % (r, k) for k in range(1, 6)) + '\n'
    h += '    ' + ''.join('<p class="ch k%d">§RL§</p>' % k for k in range(1, 6)) + '\n  '
    return h


SLIDES = [slide(), slide()]

STR = {
    'ar': {'TTL': 'قالب جدول نموذج العملية المتنامية', 'HD': 'عدّل النص', 'LB': 'عدّل النص هنا',
           'C': 'أضف النص المطلوب هنا.', 'RL': 'عدّل الدور هنا'},
    'en': {'TTL': 'Growing Process Model Table PowerPoint Template', 'HD': 'EDIT TEXT', 'LB': 'Edit Text Here',
           'C': 'Insert your desired text here.', 'RL': 'Edit Role Here'},
}

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'ops', 'icon': '📶',
    'name': 'نموذج عملية بخمسة مستويات', 'nameEn': '5-Level Process Model',
    'description': 'شريحتان لجدول عملية متدرّج بخمسة مستويات وأدوار: نسخة فاتحة ونسخة داكنة',
    'descriptionEn': 'Two slides of a five-level stepped process table with roles: light and dark versions',
    'sections': [
        {'title': 'النسخة الفاتحة', 'titleEn': 'Light Version'},
        {'title': 'النسخة الداكنة', 'titleEn': 'Dark Version'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط وغيّر النصوص فقط؛ الأعمدة k1–k5 والصفوف r1–r4 أصناف مواضع، والشريحة الثانية نسخة داكنة من الأولى.',
}
