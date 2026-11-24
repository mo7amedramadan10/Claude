# -*- coding: utf-8 -*-
# نموذج «دراسة حالة أعمال»: 31 شريحة بأسلوب عرض ماجستير إدارة الأعمال (غلاف، أجندة، المشكلة والحل، ملخص تنفيذي،
# فواصل أقسام بأشرطة ملونة فوق صور، القوى الخمس، الأداء المالي، عوامل النجاح، تحليل SWOT، الخيارات، مصفوفة الحلول،
# التوصية، المخاطر، الجدول الزمني، خطة التواصل، الخلاصة، الشكر).
# الخلفيات الفوتوغرافية مقصوصة من الأصل بعد محو النصوص، والأشرطة شبه الشفافة أُزيلت من الصورة رياضيًا
# فيُعاد رسمها بـ CSS بالشفافية نفسها (0.8)؛ لذلك تُعكس صور الخلفية في الصفحة العربية حتى تطابق مواضع الأشرطة المعكوسة.
KEY = 'sl-business-case-study'
TITLE = {'ar': 'دراسة حالة أعمال', 'en': 'Business Case Study'}
COMMENT = 'نموذج «دراسة حالة أعمال»: 31 شريحة بخلفية رمادية فاتحة (#F2F2F2) وعناوين بتدرّج بنفسجي، ولوحة ألوان كحلي/بنفسجي/أرجواني/أحمر/برتقالي؛ صور الخلفية تُعكس في العربية لتطابق الأشرطة.'
VARS = ('--ink: #2B3542; --ink-2: #3A3F47; --navy: #160B47; --purple: #5F0479; --magenta: #9C0F5E; --red: #C91B25; '
        '--orange: #F26B21; --slate: #34495E; --slate-2: #263445; --accent: #5F0479; --bg: #F2F2F2;')

FONTS = {'ar': '<link href="https://fonts.googleapis.com/css2?family=Tajawal:wght@400;500;700;800&display=swap" rel="stylesheet">',
         'en': '<link href="https://fonts.googleapis.com/css2?family=Arimo:ital,wght@0,400;0,700;1,400;1,700&display=swap" rel="stylesheet">'}
FONT_STACK = {'ar': "'Tajawal', 'Segoe UI', Tahoma, sans-serif", 'en': "'Arimo', 'Liberation Sans', Arial, sans-serif"}

A = '/templates/freeform/' + KEY + '/assets/'


def top(base, fs, lh=None):
    """أعلى صندوق السطر لخط Arial/Arimo من خط الأساس المقروء في الصورة."""
    L = lh if lh else 1.15 * fs
    return round(base - 0.905 * fs - (L - 1.149 * fs) / 2)


def img(name, cls, alt):
    return '<img class="%s" src="%s%s.jpg" alt="%s">' % (cls, A, name, alt)


# ===== الأيقونات (SVG مضمّن بلون النص) =====
def svg(body, vb='0 0 24 24', extra=''):
    return '<svg viewBox="%s" fill="currentColor" aria-hidden="true"%s>%s</svg>' % (vb, extra, body)


ST = ' fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"'
IC = {
    'handshake': svg('<path d="M1 9.6 5 6.2l3.4 1.1L11 6.2l2.1.6-3.5 3.3a1.3 1.3 0 0 0 1.8 1.9l2.3-1.8 6.4 4.7-1.2 1.4-.9-.6.6 1-1.2 1.2-1-.7.5 1-1.3 1-1-.7.3 1c-.6.5-1.5.5-2.1 0L6 13.5l-2.5 1.6L1 12.6z"/><path d="m14.4 6.1 3.6-.6 5 3.6v3.5l-2.6 1.7-6.2-4.6-2 1.5a.6.6 0 0 1-.8-.9z"/>'),
    'bulb': svg('<path d="M12 6.2a5.2 5.2 0 0 0-3.1 9.4V18h6.2v-2.4A5.2 5.2 0 0 0 12 6.2zm-.8 2h1.6v4.6h-1.6zm0 5.6h1.6v1.6h-1.6zM9.4 18.8h5.2v1.3H9.4zm1.1 1.9h3v1.1h-3zM11.3 1.6h1.4v2.6h-1.4zM4.3 4.6l1-1 1.8 1.8-1 1zM17 5.4l1.8-1.8 1 1L18 6.4zM2 10.6h2.6V12H2zm17.4 0H22V12h-2.6z"/>'),
    'coins': svg('<ellipse cx="8" cy="5" rx="5" ry="2"/><path d="M3 6.4c1 1 2.9 1.6 5 1.6s4-.6 5-1.6v1.9c0 1.1-2.2 2-5 2s-5-.9-5-2zm0 3.6c1 1 2.9 1.6 5 1.6s4-.6 5-1.6v1.9c0 1.1-2.2 2-5 2s-5-.9-5-2zm0 3.6c1 1 2.9 1.6 5 1.6s4-.6 5-1.6v1.9c0 1.1-2.2 2-5 2s-5-.9-5-2zm0 3.6c1 1 2.9 1.6 5 1.6s4-.6 5-1.6v1.9c0 1.1-2.2 2-5 2s-5-.9-5-2z"/><ellipse cx="16.5" cy="9" rx="4.5" ry="1.8"/><path d="M12 10.3c.9.9 2.6 1.4 4.5 1.4s3.6-.5 4.5-1.4V12c0 1-2 1.8-4.5 1.8S12 13 12 12zm0 3.4c.9.9 2.6 1.4 4.5 1.4s3.6-.5 4.5-1.4v1.7c0 1-2 1.8-4.5 1.8S12 16.4 12 15.4zm0 3.4c.9.9 2.6 1.4 4.5 1.4s3.6-.5 4.5-1.4v1.7c0 1-2 1.8-4.5 1.8S12 19.8 12 18.8z"/>'),
    'bag': svg('<path d="M8.6 2.5h6.8l-1.7 3.3h-3.4zM10 6.6h4c3.3 2.1 6.4 6 6.4 9.4a5.3 5.3 0 0 1-5.3 5.4H8.9A5.3 5.3 0 0 1 3.6 16c0-3.4 3.1-7.3 6.4-9.4z"/><path d="M14.3 12.4c-.5-.9-1.4-1.3-2.2-1.3-1.3 0-2.1.7-2.1 1.6 0 2 4.4 1.2 4.4 3.3 0 .9-.9 1.6-2.2 1.6-.9 0-1.8-.4-2.3-1.3M12.1 9.8v9" fill="none" stroke="var(--ib, #000)" stroke-width="1.4" stroke-linecap="round"/>'),
    'list': svg('<circle cx="4.5" cy="5.5" r="2.1"/><circle cx="4.5" cy="12" r="2.1"/><circle cx="4.5" cy="18.5" r="2.1"/><rect x="8.5" y="3.8" width="13" height="3.4"/><rect x="8.5" y="10.3" width="13" height="3.4"/><rect x="8.5" y="16.8" width="13" height="3.4"/>'),
    'info': svg('<circle cx="14" cy="4.6" r="2.4"/><path d="M9.6 9.6h5.6l-2.5 8.6c-.2.7.6 1 1.2.5l1.5-1.1-.5 1.8c-1.7 1.6-6.2 2.3-5.5-.9l2-6.6H9.4z"/>'),
    'tapcash': svg('<rect x="2.5" y="2.5" width="12" height="12" rx="2" fill="none" stroke="currentColor" stroke-width="1.6"/><path d="M10.6 6.3c-.4-.7-1-.9-1.6-.9-.9 0-1.5.5-1.5 1.1 0 1.4 3.2.9 3.2 2.4 0 .7-.7 1.2-1.6 1.2-.7 0-1.3-.3-1.7-.9M9 4.4v7" fill="none" stroke="currentColor" stroke-width="1.2" stroke-linecap="round"/><path d="M13.2 10.2c0-.8 1.6-.8 1.6 0v4.4l.4-1.4c.3-.8 1.6-.5 1.5.3l-.1 1 .5-.9c.4-.7 1.6-.3 1.4.5l-.1.8.5-.6c.5-.6 1.5-.1 1.3.7l-.8 4.3c-.4 2-1.9 3.2-3.9 3.2-1.9 0-3.1-.9-4-2.4l-2-3.4c-.4-.7.5-1.4 1.1-.8l1.6 1.6z" fill="none" stroke="currentColor" stroke-width="1.3" stroke-linejoin="round"/>'),
    'chartup': svg('<rect x="3" y="15" width="2.6" height="6"/><rect x="7" y="12.5" width="2.6" height="8.5"/><rect x="11" y="13.5" width="2.6" height="7.5"/><rect x="15" y="10" width="2.6" height="11"/><path d="m2.5 12 5-5 4 3 6.5-6.2" fill="none" stroke="currentColor" stroke-width="1.6"/><path d="m15.8 3 4.4-.6-.6 4.4z"/><rect x="2" y="21" width="17" height="1"/>'),
    'network': svg('<circle cx="12" cy="3.2" r="1.9"/><path d="M10 5.6h4l.7 6.2H13v6h-2v-6H9.3z"/><path d="M12 18 5 15m7 3 7-3m-7 3-5 3.5M12 18l5 3.5M12 18v3.8" fill="none" stroke="currentColor" stroke-width="1"/><circle cx="4.6" cy="14.8" r="1.4"/><circle cx="19.4" cy="14.8" r="1.4"/><circle cx="6.6" cy="21.6" r="1.4"/><circle cx="17.4" cy="21.6" r="1.4"/><circle cx="12" cy="22" r="1.3"/>'),
    'meeting': svg('<rect x="7" y="2" width="10" height="7" rx=".5" fill="none" stroke="currentColor" stroke-width="1.2"/><circle cx="12" cy="4.6" r="1.1"/><path d="M10.3 8c0-1.2.8-1.8 1.7-1.8s1.7.6 1.7 1.8z"/><circle cx="4" cy="8.6" r="1.9"/><circle cx="20" cy="8.6" r="1.9"/><path d="M2.2 11.4h3.6l1 4.8H9v6H7.6v-4.6H3.2zm19.6 0h-3.6l-1 4.8H15v6h1.4v-4.6h4.4z"/><rect x="7.6" y="13.6" width="8.8" height="2"/><rect x="10.8" y="15.6" width="2.4" height="5.4"/><path d="M1 13h1v9H1zm21 0h1v9h-1z"/>'),
    'shield': svg('<path d="M14.5 2.3 21 5v5.2c0 5-2.8 8.6-6.5 10.6C10.8 18.8 8 15.2 8 10.2V5z" fill="none" stroke="currentColor" stroke-width="1.6"/><path d="M14.5 4.6 19 6.5v3.9c0 3.7-1.9 6.4-4.5 8-2.6-1.6-4.5-4.3-4.5-8V6.5z"/><path d="M16.3 9.2c-.4-.8-1.1-1.1-1.8-1.1-1 0-1.7.6-1.7 1.3 0 1.6 3.6 1 3.6 2.7 0 .8-.8 1.3-1.8 1.3-.8 0-1.5-.3-1.9-1.1m1.8-4.3v7" fill="none" stroke="var(--ib, #000)" stroke-width="1.2" stroke-linecap="round"/><path d="M1.5 11.5h5v1.6h-5zm0 2.4h5.7v1.6H1.5zm0 2.4H8v1.6H1.5zm0 2.4H9v1.6H1.5zm2-9.6h3v1.6h-3z"/>'),
    'presenter': svg('<circle cx="12" cy="2.6" r="1.9"/><path d="M10 5h4l.6 2H9.4z"/><rect x="5" y="7.2" width="14" height="10.5" rx=".6" fill="none" stroke="currentColor" stroke-width="1.4"/><rect x="7.6" y="13" width="1.8" height="3"/><rect x="10.6" y="11.5" width="1.8" height="4.5"/><rect x="13.6" y="12.5" width="1.8" height="3.5"/><path d="m7.5 11 3-2.4 2.5 1.6 3.5-2.7" fill="none" stroke="currentColor" stroke-width="1"/><path d="M10.3 17.7h1.4v5h-1.4zm2 0h1.4v5h-1.4z"/>'),
    'mail': svg('<path d="M2 9.5 12 2l10 7.5V22H2z" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linejoin="round"/><path d="m2 9.8 10 7 10-7M2 21.6l7.6-6.4m12.4 6.4-7.6-6.4" fill="none" stroke="currentColor" stroke-width="1.5"/><rect x="6" y="4.5" width="12" height="10" fill="currentColor"/><circle cx="12" cy="9.2" r="2.8" fill="none" stroke="var(--ib, #000)" stroke-width="1.3"/><path d="M14.8 8.8v1.6c0 1 1.4 1 1.4-.3a4.3 4.3 0 1 0-1.7 3.4" fill="none" stroke="var(--ib, #000)" stroke-width="1.1"/>'),
    'www': svg('<circle cx="12" cy="12" r="9.2" fill="none" stroke="currentColor" stroke-width="2.2"/><path d="M8.5 4.5c1.6 1 2 2.7 1 4h-4c.4-1.8 1.6-3.2 3-4zm8.5 2c1 1 1.8 2.2 2 3.4h-3.6c.2-1.4.8-2.6 1.6-3.4zM6.3 15.2h3.8c.6 1.4.2 3-1.2 3.9a8 8 0 0 1-2.6-3.9zm8 0h3.4c-.6 1.6-1.8 3-3.3 3.7-.6-1.1-.6-2.6-.1-3.7z"/><rect x="1" y="9" width="22" height="6" rx=".6"/><path d="m4 10.5.8 3 .8-3 .8 3 .8-3m2 0 .8 3 .8-3 .8 3 .8-3m2 0 .8 3 .8-3 .8 3 .8-3" fill="none" stroke="var(--ib, #000)" stroke-width=".8"/>'),
    'check': svg('<path d="M20.2 8.3A9 9 0 1 1 16.5 4" fill="none" stroke="currentColor" stroke-width="2.3" stroke-linecap="round"/><path d="m7.6 11.4 3.6 3.6 9.6-9.6" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"/>'),
    'dumbbell': svg('<rect x=".5" y="9" width="3.4" height="6" rx="1"/><rect x="4.2" y="6.6" width="4.4" height="10.8" rx="1.2"/><rect x="8.4" y="10.6" width="7.2" height="2.8"/><rect x="15.4" y="6.6" width="4.4" height="10.8" rx="1.2"/><rect x="20.1" y="9" width="3.4" height="6" rx="1"/>'),
    'chain': svg('<path d="M9.8 16.6 7.5 18.9a3.6 3.6 0 0 1-5.1-5.1l3.4-3.4a3.6 3.6 0 0 1 4.9-.2m3.5-2.8 2.3-2.3a3.6 3.6 0 0 1 5.1 5.1l-3.4 3.4a3.6 3.6 0 0 1-4.9.2" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round"/><path d="M11.5 3.5l.6 3M8 5.6l2 2.2m10.5 6.5-3-.4" fill="none" stroke="currentColor" stroke-width="1.3" stroke-linecap="round"/>'),
    'key': svg('<path d="M15.3 1.8a6.6 6.6 0 0 0-6.2 8.9L1.5 18.3v3.2h3.8v-2.3h2.3v-2.3h2.3l2.3-2.3a6.6 6.6 0 1 0 3.1-12.8zm1.4 3.4a2.3 2.3 0 1 1 0 4.6 2.3 2.3 0 0 1 0-4.6z"/>'),
    'bomb': svg('<circle cx="10" cy="14" r="8"/><path d="M14.2 6.5 16 4.7l2.8 2.8-1.8 1.8z"/><path d="M18.5 4.5c.4-1.4 1.2-2 2.3-2M20 7.5l2.6-.8M17.4 1.5l.4-1.2M21.5 4l1.4-.8" fill="none" stroke="currentColor" stroke-width="1.1" stroke-linecap="round"/><path d="M5.8 12.5a4.5 4.5 0 0 1 3-3.6" fill="none" stroke="#fff" stroke-opacity=".55" stroke-width="1.3" stroke-linecap="round"/>'),
    'team': svg('<circle cx="12" cy="3.6" r="2.3"/><circle cx="6.2" cy="6.6" r="2"/><circle cx="17.8" cy="6.6" r="2"/><path d="M8.6 9.6c.8-1.6 2-2.4 3.4-2.4s2.6.8 3.4 2.4"/><circle cx="9" cy="11.4" r="2.2"/><circle cx="15" cy="11.4" r="2.2"/><path d="M2.4 13.6c0-2 1.7-3.6 3.8-3.6.9 0 1.6.3 2.2.7a3.4 3.4 0 0 0-.6 3.1H2.4zm19.2 0c0-2-1.7-3.6-3.8-3.6-.9 0-1.6.3-2.2.7.6 1 .7 2 .6 3.1h5.4z"/><circle cx="12" cy="15.6" r="2.5"/><path d="M5 19.3c0-2.3 1.8-4 4-4 .5 0 .9.1 1.3.2a3.6 3.6 0 0 0-.3 3.8H5zm14 0c0-2.3-1.8-4-4-4-.5 0-.9.1-1.3.2.6 1.2.6 2.5.3 3.8H19zM7.7 22.5c0-2.6 1.9-4.4 4.3-4.4s4.3 1.8 4.3 4.4z"/>'),
    'pin': svg('<path d="M12 1.6a5 5 0 0 0-5 5c0 3.8 5 9.3 5 9.3s5-5.5 5-9.3a5 5 0 0 0-5-5zm0 3a2 2 0 1 1 0 4 2 2 0 0 1 0-4z"/><path d="M8 13.4H5.4L1.6 21.4h20.8l-3.8-8H16" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linejoin="round"/>'),
    'trio': svg('<circle cx="12" cy="3" r="2.2"/><circle cx="5.5" cy="5" r="1.9"/><circle cx="18.5" cy="5" r="1.9"/><path d="M9 6h6l.8 8.5H14l-.4 8h-3.2l-.4-8H8.2zM11.4 6h1.2l.4 4-1 1.2-1-1.2z"/><path d="M3 7.6h5l-.2 6.4H6.6l-.3 8.3H4.7l-.3-8.3H3.1zm13 0h5l-.1 6.4h-1.2l-.3 8.3h-1.6l-.3-8.3h-1.2z"/>'),
    'monitor': svg('<circle cx="15.5" cy="5.2" r="3.6"/><path d="M9.5 21.5v-4.6c0-3 2.6-5.6 6-5.6s6 2.6 6 5.6v4.6z"/><rect x="1.5" y="11" width="11" height="8.3" fill="currentColor"/><rect x="2.4" y="11.9" width="9.2" height="6.5" fill="var(--ib, #000)"/><rect x="3.6" y="15" width="1" height="2.4"/><rect x="5.3" y="13.5" width="1" height="3.9"/><rect x="7" y="14.4" width="1" height="3"/><rect x="8.7" y="13" width="1" height="4.4"/><rect x="5.5" y="19.3" width="3" height="1.4"/><rect x="3.8" y="20.7" width="6.4" height="1"/>'),
    'search': svg('<circle cx="4.4" cy="3.2" r="1.7"/><circle cx="19.6" cy="3.2" r="1.7"/><path d="M1.8 8.2c0-1.8 1.2-2.9 2.6-2.9s2.6 1.1 2.6 2.9zm15.2 0c0-1.8 1.2-2.9 2.6-2.9s2.6 1.1 2.6 2.9z"/><circle cx="11" cy="13" r="6.2" fill="none" stroke="currentColor" stroke-width="2.4"/><path d="m15.4 17.6 5.4 5.4" stroke="currentColor" stroke-width="3" stroke-linecap="round"/><circle cx="11" cy="10.9" r="1.7"/><path d="M8 16.2v-1.4c0-1.1.9-2 2-2h2c1.1 0 2 .9 2 2v1.4z"/>'),
    'person': svg('<circle cx="12" cy="6" r="4.6"/><path d="M3.5 22.5v-3.8c0-4 3.8-6.6 8.5-6.6s8.5 2.6 8.5 6.6v3.8z"/><path d="m11.2 12 .8.8.8-.8.6 1.2-.6 6.6-.8 1.2-.8-1.2-.6-6.6z" fill="var(--ib, #000)"/>'),
    'flow': svg('<rect x="15.2" y="2" width="5.4" height="5.4" fill="none" stroke="currentColor" stroke-width="2"/><circle cx="4.2" cy="18" r="2.6" fill="none" stroke="currentColor" stroke-width="2"/><circle cx="18" cy="18" r="2.6" fill="none" stroke="currentColor" stroke-width="2"/><path d="M4.2 15.4c0-5.5 3.7-8.8 9-9.3M6.8 18h6.4m4.8-2.6V9.4" fill="none" stroke="currentColor" stroke-width="1.8"/><path d="m12.4 3.6 3 2.4-3.2 2.2zm2.6 13.4-2.8-1.8v3.6zM16.2 11l1.8-2.8 1.8 2.8z"/>'),
    'clickcash': svg('<rect x="1.5" y="2" width="11.5" height="11.5" rx="2"/><path d="M9.6 5.6c-.4-.7-1-.9-1.6-.9-.9 0-1.5.5-1.5 1.1 0 1.4 3.2.9 3.2 2.4 0 .7-.7 1.2-1.6 1.2-.7 0-1.3-.3-1.7-.9M8 3.7v8" fill="none" stroke="var(--ib, #000)" stroke-width="1.3" stroke-linecap="round"/><path d="M11.6 9.6c0-.9 1.7-.9 1.7 0v5l.5-1.5c.3-.8 1.7-.5 1.6.3l-.1 1.1.6-1c.4-.7 1.7-.3 1.5.5l-.1.9.6-.7c.5-.6 1.6-.1 1.4.7l-.9 4.6c-.4 2.1-2 3.4-4.1 3.4-2 0-3.3-.9-4.2-2.5l-2.1-3.6c-.4-.7.6-1.5 1.2-.9l1.8 1.7z" fill="currentColor" stroke="var(--ib, #000)" stroke-width="1.1" stroke-linejoin="round"/>'),
    'currency': svg('<circle cx="7.2" cy="7" r="5.2"/><circle cx="16.8" cy="8" r="5.2"/><circle cx="12" cy="16" r="6.6" stroke="var(--ib, #000)" stroke-width="1"/><path d="m5 4 2.2 3.2L9.4 4M7.2 7.2V10.6M5.2 7.8h4M5.2 9.2h4" fill="none" stroke="var(--ib, #000)" stroke-width="1.1"/><path d="M18.6 6.2c-.3-.6-.9-.9-1.5-.9-.9 0-1.6.7-1.6 1.7v4h3M14.6 8.6h3" fill="none" stroke="var(--ib, #000)" stroke-width="1.1"/><path d="M14.6 13.6c-.5-.9-1.4-1.3-2.4-1.3-1.4 0-2.3.8-2.3 1.7 0 2.2 4.9 1.3 4.9 3.6 0 1-1 1.8-2.4 1.8-1 0-2-.5-2.5-1.4M12.2 10.8v9.8" fill="none" stroke="var(--ib, #000)" stroke-width="1.5" stroke-linecap="round"/>'),
    'logo': ('<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M5 9.5V7.2C5 5 6.4 4 8.6 3.8l8.6-.6C19.6 3 21 4.4 21 6.8v6.6" fill="none" stroke="#E2262F" stroke-width="3.4" stroke-linecap="round"/>'
             '<path d="M21 13.4v2.4c0 2.2-1.4 3.4-3.6 3.6l-8.6.8C6.4 20.4 5 19 5 16.6V9.5" fill="none" stroke="#F7C51E" stroke-width="3.4" stroke-linecap="round"/></svg>'),
}


def brand(n, dark=False):
    """شعار عام (بدل شعار الناشر) + رقم الشريحة في مربع كحلي."""
    return ('<div class="brand%s"><span class="mk">%s</span><span class="bt">§LOGO§</span></div><span class="pg">%d</span>'
            % (' dk' if dark else '', IC['logo'], n))


def ttl(text_key, base, cls=''):
    return '<h1 class="ttl %s" style="top:%dpx"><span class="gt">§%s§</span></h1>' % (cls, top(base, 64), text_key)


STYLE = []
SLIDES = []
STR = {'ar': {'LOGO': 'الشعار'}, 'en': {'LOGO': 'Logo'}}


def S(n, en, ar):
    """إضافة نصوص بمفاتيح مسبوقة برقم الشريحة."""
    for k in en:
        STR['en']['S%d%s' % (n, k)] = en[k]
        STR['ar']['S%d%s' % (n, k)] = ar[k]


# ===== الأنماط المشتركة =====
STYLE.append("""  .frame p, .frame h1, .frame h2, .frame h3, .frame ul { margin: 0; }
  .frame > * { position: absolute; }
  .bg { inset: 0; width: 1280px; height: 720px; object-fit: cover; }
  /* صور الخلفية (وما فيه أشرطة مُزالة) تُعكس في العربية لتطابق مواضع الأشرطة المعكوسة */
  html[dir="rtl"] .bg, html[dir="rtl"] .mir { transform: scaleX(-1); }
  .brand { top: 21px; inset-inline-end: 82px; height: 30px; display: flex; align-items: center; gap: 3px; color: #111; }
  .brand.dk { color: #fff; }
  .brand .mk svg { width: 30px; height: 30px; display: block; }
  .brand .bt { font-size: 21px; font-weight: 700; letter-spacing: -.6px; }
  html[lang="ar"] .brand .bt { letter-spacing: 0; font-size: 20px; }
  .pg { top: 10px; inset-inline-end: 22px; width: 48px; height: 48px; border-radius: 7px; background: var(--navy); color: #fff; font-size: 18px; display: grid; place-items: center; box-shadow: 0 0 0 1px rgb(255 255 255 / .08); }
  .ttl { inset-inline-start: 0; width: 1280px; text-align: center; font-size: 64px; font-weight: 400; line-height: 1.15; white-space: nowrap; }
  html[lang="ar"] .ttl, html[lang="ar"] .gtt { font-weight: 500; }
  .gt { display: inline-block; background: linear-gradient(90deg, #440A58 0%, #5A0B74 22%, #8A1FB0 42%, #B24ED8 55%, #8A1BAE 70%, #5C0B76 86%, #43095A 100%); -webkit-background-clip: text; background-clip: text; color: transparent; }
  .t20 { font-size: 21.3px; line-height: 25.5px; color: var(--ink); }
  .ctr { text-align: center; }
  .ic svg { display: block; width: 100%; height: 100%; }
  /* فاصل قسم: صورة + شريط شبه شفاف + كتلة معتمة */
  .sec .band { top: 431px; inset-inline-start: 0; width: 1012px; height: 176px; background: var(--c); opacity: .8; }
  .sec .blk { top: 431px; inset-inline-start: 1033px; width: 247px; height: 176px; background: var(--c); }
  .sec h1 { top: {SECT}px; inset-inline-start: 64px; font-size: 95.4px; font-weight: 400; line-height: 1.15; color: #fff; white-space: nowrap; }
  html[lang="ar"] .sec h1 { font-weight: 500; font-size: 92px; }""".replace('{SECT}', str(top(553, 95.4))))


# ===== 1: الغلاف =====
STYLE.append("""  .s1 .lp { inset-inline-start: 0; top: 0; width: 496px; height: 720px; background: #273746; }
  .s1 .ph { top: 0; inset-inline-start: 496px; width: 784px; height: 720px; object-fit: cover; }
  .s1 .fold { top: 558px; inset-inline-start: 445px; width: 52px; height: 28px; background: #8E0F1A; clip-path: polygon(0 0, 100%% 0, 100%% 100%%); }
  html[dir="rtl"] .s1 .fold { transform: scaleX(-1); }
  .s1 .rb { top: 390px; inset-inline-start: 445px; width: 835px; height: 168px; background: #C91B25; }
  .s1 h1 { top: %dpx; inset-inline-start: 483px; font-size: 79.3px; font-weight: 400; line-height: 1.15; color: #fff; white-space: nowrap; }
  html[lang="ar"] .s1 h1 { font-weight: 700; font-size: 60px; margin-top: 8px; }
  .s1 .cl { top: 392px; inset-inline-start: 40px; display: flex; align-items: center; gap: 6px; color: #fff; }
  .s1 .cl svg { width: 60px; height: 60px; display: block; }
  .s1 .cl span { font-size: 44px; font-weight: 700; letter-spacing: -1.5px; }
  html[lang="ar"] .s1 .cl span { letter-spacing: 0; font-size: 40px; }
  .s1 .sub { top: 478px; inset-inline-start: 50px; width: 360px; font-size: 23.9px; line-height: 28.5px; font-style: italic; color: #fff; }
  html[lang="ar"] .s1 .sub { font-style: normal; font-size: 22px; line-height: 32px; }""" % top(502, 79.3))
SLIDES.append('<div class="lp"></div>' + img('cover', 'ph', 'فريق يعمل على دراسة حالة') + '<div class="fold"></div><div class="rb"></div>'
              '<h1>§S1T§</h1><div class="cl">%s<span>§LOGO§</span></div><p class="sub">§S1P§</p>' % IC['logo'])
S(1, {'T': 'MBA Presentation', 'P': 'This is a generic business case study presentation template for business students'},
  {'T': 'عرض ماجستير إدارة الأعمال', 'P': 'هذا قالب عام لعرض دراسة حالة أعمال مخصّص لطلاب إدارة الأعمال'})

# ===== 2: الأجندة =====
STYLE.append("""  .s2 h1 { top: %dpx; inset-inline-start: 0; width: 1280px; text-align: center; font-size: 64px; font-weight: 400; line-height: 1.15; color: #fff; }
  html[lang="ar"] .s2 h1 { font-weight: 500; }
  .s2 ul { top: 180px; inset-inline-start: 354px; width: 594px; list-style: none; padding: 0; }
  .s2 li { height: 67.2px; display: flex; align-items: flex-end; justify-content: center; padding-bottom: 4px; border-bottom: 2px solid rgb(255 255 255 / .38); font-size: 23.9px; line-height: 28px; color: #fff; }""" % top(160, 64))
SLIDES.append(img('bg-agenda', 'bg', 'خلفية مكتب داكنة') + brand(2, True) + '<h1>§S2T§</h1><ul>' + ''.join('<li>§S2I%d§</li>' % i for i in range(1, 8)) + '</ul>')
S(2, {'T': 'Agenda', 'I1': 'Problem &amp; solution', 'I2': 'Executive summary', 'I3': 'Brief history', 'I4': 'Industry analysis',
      'I5': 'Company analysis', 'I6': 'Alternatives', 'I7': 'Implementation'},
  {'T': 'جدول الأعمال', 'I1': 'المشكلة والحل', 'I2': 'الملخص التنفيذي', 'I3': 'نبذة تاريخية', 'I4': 'تحليل القطاع',
   'I5': 'تحليل الشركة', 'I6': 'البدائل', 'I7': 'التنفيذ'})

# ===== 3: المشكلة والحل =====
TRI = ['#2A0F62', '#6E0A93', '#A8136A', '#D4232B', '#F07A2A']
STYLE.append("""  .s3 .ph { top: 0; inset-inline-start: 0; width: 530px; height: 720px; object-fit: cover; }
  .s3 .wb { top: 434px; inset-inline-start: 0; width: 530px; height: 214px; background: #F2F2F2; opacity: .8; }
  .s3 h1 { top: %dpx; inset-inline-start: 38px; font-size: 71px; line-height: 78px; font-weight: 400; white-space: nowrap; }
  html[lang="ar"] .s3 h1 { font-size: 66px; }
  .s3 .bl { inset-inline-start: 603px; width: 560px; display: flex; align-items: center; gap: 30px; }
  .s3 .bl i { flex: none; width: 24px; height: 26px; clip-path: polygon(0 0, 100%% 50%%, 0 100%%); }
  html[dir="rtl"] .s3 .bl i { clip-path: polygon(100%% 0, 0 50%%, 100%% 100%%); }
  .s3 .bl p { font-size: 20px; line-height: 25px; color: var(--ink); }""" % top(527, 71, 78))
s3 = img('hands', 'ph', 'يدان متشابكتان') + '<div class="wb"></div>' + brand(3) + '<h1 class="gtt"><span class="gt">§S3T1§<br>§S3T2§</span></h1>'
for i, cy in enumerate([136, 248, 364, 478, 594]):
    s3 += '<div class="bl" style="top:%dpx;height:60px"><i style="background:linear-gradient(160deg,%s,%s)"></i><p>§S3B%d§</p></div>' % (cy - 30, TRI[i], TRI[i] + 'CC', i + 1)
SLIDES.append(s3)
S(3, {'T1': 'Problem &amp;', 'T2': 'Solution', 'B1': 'Frame the “problem statement” as a question.', 'B2': 'Put your recommended solution on this slide',
      'B3': 'You’ll sometimes see the solution presented at the end of a presentation,', 'B4': 'Putting the solution right after the problem makes you confirm that your solution',
      'B5': 'No surprises. If the audience knows your solution off the top, the rest of the deck should walk'},
  {'T1': 'المشكلة', 'T2': 'والحل', 'B1': 'صُغ «بيان المشكلة» في صورة سؤال.', 'B2': 'ضع الحل الذي توصي به في هذه الشريحة',
   'B3': 'قد ترى أحيانًا الحل معروضًا في نهاية العرض،', 'B4': 'وضع الحل مباشرة بعد المشكلة يجعلك تتأكد من أن حلك',
   'B5': 'لا مفاجآت. إذا عرف الجمهور حلك من البداية، فينبغي أن تمضي بقية الشرائح'})

# ===== 4: الملخص التنفيذي =====
STYLE.append("""  .s4 .hex { top: 172px; inset-inline-start: 65px; width: 378px; height: 418px; filter: drop-shadow(0 10px 18px rgb(0 0 0 / .35)); }
  .s4 .hp { top: 217px; inset-inline-start: 108px; width: 295px; height: 329px; object-fit: cover; clip-path: polygon(50%% 0, 100%% 22.3%%, 100%% 77.7%%, 50%% 100%%, 0 77.7%%, 0 22.3%%); }
  .s4 .ttl2 { top: %dpx; inset-inline-start: 491px; font-size: 64px; line-height: 1.15; font-weight: 400; white-space: nowrap; }
  .s4 .lead { top: 201px; inset-inline-start: 491px; width: 735px; font-size: 18.6px; line-height: 33.7px; color: var(--ink); }
  html[lang="ar"] .s4 .lead { font-size: 19px; }
  .s4 .it { width: 400px; display: flex; align-items: center; gap: 22px; }
  .s4 .it .ic { flex: none; width: 77px; height: 77px; border-radius: 50%%; color: #fff; padding: 17px; }
  .s4 .it p { font-size: 21.3px; line-height: 25px; color: var(--ink); }""" % top(175, 64))
s4 = ('<svg class="hex mir" viewBox="65 172 378 418" aria-hidden="true"><defs><linearGradient id="hx4" x1="0" y1="0" x2="1" y2="0.4"><stop offset="0" stop-color="#5B42B0"/><stop offset=".45" stop-color="#2A1872"/><stop offset="1" stop-color="#150A45"/></linearGradient></defs>'
      '<polygon points="254,190 423,288 423,472 254,570 85,472 85,288" fill="url(#hx4)"/></svg>'
      + img('meeting', 'hp', 'غرفة اجتماعات') + brand(4) + '<h1 class="ttl2 gtt"><span class="gt">§S4T§</span></h1><p class="lead">§S4L§</p>')
for i, (x, y, ic, col) in enumerate([(483, 391, 'handshake', 'var(--navy)'), (904, 391, 'bulb', '#A51260'), (483, 513, 'coins', 'var(--purple)'), (904, 513, 'bag', 'var(--red)')]):
    s4 += '<div class="it" style="top:%dpx;inset-inline-start:%dpx;width:%dpx"><span class="ic" style="background:%s;--ib:%s">%s</span><p>§S4I%d§</p></div>' % (y, x, 400 if x < 900 else 300, col, col, IC[ic], i + 1)
s4 = s4.replace('class="it" style="top:391px', 'class="it" style="height:77px;top:391px').replace('class="it" style="top:513px', 'class="it" style="height:77px;top:513px')
SLIDES.append(s4)
S(4, {'T': 'Executive Summary', 'L': 'The executive summary is a single slide that contains the key points from the rest of the presentation. The idea is an executive who doesn’t have time to sit through your whole presentation can look at this, understand the problem, what’s driving the problem, your solution and why it will work. Do this slide last, always.',
      'I1': 'Problem facing your company, our proposed solution', 'I2': 'Competitors are all doing X', 'I3': 'Market data backs up our solution (cite briefly)', 'I4': 'This is because the industry is doing Y'},
  {'T': 'الملخص التنفيذي', 'L': 'الملخص التنفيذي شريحة واحدة تضم النقاط الرئيسة من بقية العرض. والفكرة أن المسؤول التنفيذي الذي لا يملك وقتًا لمتابعة عرضك كاملًا يستطيع النظر إليها فيفهم المشكلة، وما يدفع إليها، وحلك، ولماذا سينجح. اكتب هذه الشريحة في النهاية دائمًا.',
   'I1': 'المشكلة التي تواجه شركتك، والحل الذي نقترحه', 'I2': 'المنافسون جميعًا يفعلون «س»', 'I3': 'بيانات السوق تدعم حلنا (اذكر المصدر باختصار)', 'I4': 'وذلك لأن القطاع يفعل «ص»'})


def section(n, bgname, color, alt, dark=True):
    return ('<div class="sec" style="--c:%s;position:absolute;inset:0">' % color + img(bgname, 'bg', alt)
            + '<div class="band"></div><div class="blk"></div><h1>§S%dT§</h1></div>' % n + brand(n, dark))


# ===== 5: نبذة تاريخية =====
STYLE.append("""  .sec > * { position: absolute; }""")
SLIDES.append(section(5, 'bg-history', '#34495E', 'شخص يكتب في دفتر'))
S(5, {'T': 'brief history'}, {'T': 'نبذة تاريخية'})

# ===== 6: نقاط العمل الرئيسة =====
STYLE.append("""  .s6 .hx { top: 222px; width: 268px; height: 288px; filter: drop-shadow(0 6px 16px rgb(0 0 0 / .45)); }
  .s6 .hi { top: 308px; width: 116px; height: 116px; }
  .s6 .cap { top: %dpx; width: 340px; font-size: 21.3px; line-height: 25.4px; color: #3A3F47; text-align: center; }""" % top(550, 21.3, 25.4))
HEX6 = [('#3A2A9A', '#120A40', '#130A42', '#A99CF2', 'handshake'), ('#A21CC0', '#4A0560', '#5E0478', '#D865F2', 'list'), ('#D2237F', '#6F0C42', '#9C0F5E', '#F07FC0', 'info')]
s6 = brand(6) + ttl('S6T', 147)
for i, (cx, (c1, c2, ci, icol, ic)) in enumerate(zip([272, 640, 1007], HEX6)):
    s6 += ('<svg class="hx mir" style="inset-inline-start:%dpx" viewBox="0 0 268 288" aria-hidden="true"><defs><linearGradient id="h6%d" x1="0" y1=".2" x2="1" y2=".8"><stop offset="0" stop-color="%s"/><stop offset=".35" stop-color="%s"/><stop offset="1" stop-color="%s"/></linearGradient></defs>'
           '<polygon points="134,11 253,80 253,208 134,277 15,208 15,80" fill="url(#h6%d)"/><polygon points="134,46 228,100 228,188 134,242 40,188 40,100" fill="%s"/></svg>'
           % (cx - 134, i, c1, ci, c2, i, ci))
    s6 += '<span class="hi ic" style="inset-inline-start:%dpx;color:%s">%s</span>' % (cx - 58, icol, IC[ic])
    w6 = [340, 340, 310][i]
    s6 += '<p class="cap" style="inset-inline-start:%dpx;width:%dpx">§S6C%d§</p>' % (cx - w6 // 2, w6, i + 1)
SLIDES.append(s6)
S(6, {'T': 'Business Key Points', 'C1': 'This is a “state of current affairs” slide. It sets up how the company has been doing.',
      'C2': 'Bullet points on this slide highlight key points about the company that are relevant', 'C3': 'Don’t fill with irrelevant information. Remember in most MBA case scenarios,'},
  {'T': 'نقاط العمل الرئيسة', 'C1': 'هذه شريحة «الوضع الراهن»، وتعرض كيف كان أداء الشركة.',
   'C2': 'تُبرز النقاط في هذه الشريحة الجوانب المهمة ذات الصلة بالشركة', 'C3': 'لا تملأها بمعلومات غير ذات صلة. وتذكّر أنه في معظم حالات دراسة الأعمال،'})

# ===== 7: التحديات الرئيسة =====
STYLE.append("""  .s7 .p1 { top: %dpx; inset-inline-start: 0; width: 1350px; }
  .s7 .p2 { top: %dpx; inset-inline-start: 0; width: 1280px; }
  .s7 .sq { top: 346px; width: 77px; height: 77px; padding: 18px; color: #fff; }
  .s7 .cp { top: %dpx; width: 180px; font-size: 21.3px; line-height: 25px; text-align: center; }
  .s7 .p3 { top: %dpx; inset-inline-start: 0; width: 1280px; }""" % (top(256, 21.3, 25.5), top(304, 21.3, 25.5), top(468, 21.3, 25), top(562, 21.3, 25.5)))
s7 = img('bg-challenges', 'bg', 'خلفية باهتة لذراعين تتصارعان') + brand(7) + ttl('S7T', 190) + '<p class="p1 t20 ctr">§S7A§</p><p class="p2 t20 ctr">§S7B§</p>'
for i, (x, col, ic) in enumerate(zip([279, 494, 709, 924], ['var(--navy)', 'var(--purple)', 'var(--magenta)', 'var(--red)'], ['tapcash', 'coins', 'chartup', 'network'])):
    s7 += '<span class="sq ic" style="inset-inline-start:%dpx;background:%s">%s</span><p class="cp" style="inset-inline-start:%dpx;color:%s">§S7C%d§</p>' % (x, col, IC[ic], x - 51, col, i + 1)
s7 += '<p class="p3 t20 ctr">§S7D§</p>'
SLIDES.append(s7)
S(7, {'T': 'Key Challenges', 'A': 'This slide should show a major challenge the company is facing', 'B': 'Use a chart to show the key challenge, which could be many things. Examples:',
      'C1': 'That they’re underperforming', 'C2': 'They’re losing market share', 'C3': 'Industry is consolidating', 'C4': 'The market has changed because',
      'D': 'There are many things which could be a key challenge. Pick the main one,<br>then communicate the challenge succinctly.'},
  {'T': 'التحديات الرئيسة', 'A': 'ينبغي أن تعرض هذه الشريحة تحديًا كبيرًا تواجهه الشركة', 'B': 'استخدم رسمًا بيانيًا لعرض التحدي الرئيس، وقد يكون أمورًا كثيرة. أمثلة:',
   'C1': 'أن أداءها دون المستوى', 'C2': 'أنها تفقد حصتها السوقية', 'C3': 'أن القطاع يتجه إلى الاندماج', 'C4': 'أن السوق تغيّر بسبب',
   'D': 'ثمة أمور كثيرة قد تكون تحديًا رئيسًا. اختر أهمها،<br>ثم عبّر عن التحدي بإيجاز.'})

# ===== 8: تحليل القطاع =====
SLIDES.append(section(8, 'bg-industry', '#160B47', 'ناطحات سحاب'))
S(8, {'T': 'industry analysis'}, {'T': 'تحليل القطاع'})

# ===== 9: هذا القسم (تحليل القطاع) =====
STYLE.append("""  .s9 .sub { top: %dpx; inset-inline-start: 0; width: 1280px; }
  .s9 .pn { top: 234px; height: 196px; display: flex; align-items: center; color: #fff; }
  .s9 .pn .ic { flex: none; width: 100px; height: 100px; margin-inline-start: 35px; }
  .s9 .pn p { margin-inline-start: 28px; font-size: 21.3px; line-height: 25px; width: 250px; }
  .s9 .col { top: 483px; height: 163px; width: 350px; display: flex; align-items: center; }
  .s9 .vl { top: 483px; width: 1px; height: 163px; background: #BDBDBD; }""" % top(198, 21.3, 25.5))
s9 = brand(9) + ttl('S9T', 150) + '<p class="sub t20 ctr">§S9S§</p>'
for i, (x, w, col, ic) in enumerate([(0, 427, 'var(--navy)', 'meeting'), (427, 426, 'var(--purple)', 'shield'), (853, 427, 'var(--magenta)', 'presenter')]):
    s9 += '<div class="pn" style="inset-inline-start:%dpx;width:%dpx;background:%s;--ib:%s"><span class="ic">%s</span><p>§S9P%d§</p></div>' % (x, w, col, col, IC[ic], i + 1)
s9 += ('<span class="vl" style="inset-inline-start:427px"></span><span class="vl" style="inset-inline-start:853px"></span>'
       '<div class="col" style="inset-inline-start:49px;width:338px"><p class="t20">§S9C1§</p></div>'
       '<div class="col" style="inset-inline-start:465px;justify-content:center"><p class="t20 ctr">§S9C2§</p></div>'
       '<div class="col" style="inset-inline-start:921px;width:312px;justify-content:flex-end"><p class="t20" style="text-align:end">§S9C3§</p></div>')
SLIDES.append(s9)
S(9, {'T': 'This Section', 'S': 'The industry analysis section should highlight trends in the industry that:',
      'P1': 'Put pressure on the company in question', 'P2': 'Have created the challenge you’re solving', 'P3': 'Are disrupting or sustaining the industry',
      'C1': 'Use standard industry analysis frameworks (PEST, strategy canvas, Porter’s five forces and generic strategies, etc.) whichever are relevant to your case',
      'C2': 'Examples on the next two slides can be used, or other frameworks, as needed for your specific case', 'C3': 'Use also charts, graphs and quotes. Delete this slide for your actual work'},
  {'T': 'هذا القسم', 'S': 'ينبغي أن يُبرز قسم تحليل القطاع الاتجاهات التي:',
   'P1': 'تضغط على الشركة موضوع الدراسة', 'P2': 'أوجدت التحدي الذي تعالجه', 'P3': 'تُحدث تحولًا في القطاع أو تحافظ على استقراره',
   'C1': 'استخدم أطر تحليل القطاع المعتادة (التحليل السياسي والاقتصادي والاجتماعي والتقني، ولوحة الاستراتيجية، وقوى بورتر الخمس والاستراتيجيات العامة…) بحسب ما يناسب حالتك',
   'C2': 'يمكن استخدام الأمثلة في الشريحتين التاليتين، أو أطر أخرى بحسب حاجة حالتك', 'C3': 'استخدم أيضًا الرسوم البيانية والاقتباسات. احذف هذه الشريحة من عملك الفعلي'})

# ===== 10: القوى الخمس =====
STYLE.append("""  .s10 .sub { top: %dpx; inset-inline-start: 0; width: 1280px; font-size: 23.9px; line-height: 28px; color: #3A3F47; text-align: center; }
  .s10 .bx { width: 181px; height: 152px; color: #fff; display: flex; flex-direction: column; align-items: center; justify-content: center; text-align: center; font-size: 18.7px; line-height: 23px; padding-top: 0; }
  .s10 .bx small { font-size: 15.6px; line-height: 20px; }
  .s10 .cn { top: 300px; inset-inline-start: 450px; width: 380px; height: 300px; }
  .s10 .cir { top: 324px; inset-inline-start: 507px; width: 266px; height: 266px; border-radius: 50%%; background: #344A62; color: #fff; display: flex; flex-direction: column; justify-content: center; text-align: center; font-size: 23.9px; line-height: 29.5px; }
  .s10 .cir b { font-weight: 700; }
  .s10 .nt { font-size: 18.7px; line-height: 23.4px; color: #262626; }
  .s10 .nt.e { text-align: end; }""" % top(192, 23.9, 28))
s10 = brand(10) + ttl('S10T', 150) + '<p class="sub">§S10S§</p>'
s10 += ('<svg class="cn" viewBox="450 300 380 300" aria-hidden="true" fill="none" stroke="#A6A6A6" stroke-width="1.4">'
        '<path d="M454 305.5H546V356M826 305.5H736V356M454 594.5H546V561M826 594.5H736V561"/>'
        '<g fill="#A6A6A6" stroke="none"><path d="M541 354h10l-5 9zM731 354h10l-5 9zM541 563h10l-5-9zM731 563h10l-5-9z"/></g></svg>')
for i, (x, y, col, k) in enumerate([(273, 229, 'var(--navy)', 'B1'), (822, 229, 'var(--magenta)', 'B2'), (273, 518, 'var(--purple)', 'B3'), (822, 518, 'var(--red)', 'B4')]):
    s10 += '<div class="bx" style="top:%dpx;inset-inline-start:%dpx;background:%s"><span>§S10%s§</span><small>§S10%sL§</small></div>' % (y, x, col, k, k)
s10 += '<div class="cir"><b>§S10C1§</b><b>§S10C2§</b><span>§S10C3§</span></div>'
s10 += ('<p class="nt e" style="top:%dpx;inset-inline-start:77px;width:170px">§S10N1§</p>' % top(299, 18.7, 23.4)
        + '<p class="nt e" style="top:%dpx;inset-inline-start:45px;width:202px">§S10N3§</p>' % top(577, 18.7, 23.4)
        + '<p class="nt" style="top:%dpx;inset-inline-start:1025px;width:230px">§S10N2§</p>' % top(299, 18.7, 23.4)
        + '<p class="nt" style="top:%dpx;inset-inline-start:1025px;width:215px;font-size:17.5px;line-height:22px">§S10N4§</p>' % top(575, 17.5, 22))
SLIDES.append(s10)
S(10, {'T': 'Macro Environmental Analysis', 'S': 'Porter’s five forces', 'B1': 'SUBSTITUTION', 'B1L': '(medium)', 'B2': 'BUYER POWER', 'B2L': '(medium)',
       'B3': 'SUPPLIER<br>POWER', 'B3L': '(high)', 'B4': 'NEW ENTRANTS', 'B4L': '(medium)', 'C1': 'COMPETITION', 'C2': '(high overlap)', 'C3': 'Briefly list<br>competition',
       'N1': 'Short list of substitute products', 'N2': 'Relevant info such as switching costs', 'N3': 'Brief explanation of why you’ve ranked this at low, medium or high',
       'N4': 'Brief explanation of why you’ve ranked this at low, medium or high'},
   {'T': 'تحليل البيئة الكلية', 'S': 'قوى بورتر الخمس', 'B1': 'البدائل', 'B1L': '(متوسطة)', 'B2': 'قوة المشترين', 'B2L': '(متوسطة)',
    'B3': 'قوة<br>الموردين', 'B3L': '(مرتفعة)', 'B4': 'الداخلون الجدد', 'B4L': '(متوسطة)', 'C1': 'المنافسة', 'C2': '(تداخل مرتفع)', 'C3': 'اذكر المنافسين<br>باختصار',
    'N1': 'قائمة مختصرة بالمنتجات البديلة', 'N2': 'معلومات ذات صلة مثل تكاليف التحوّل', 'N3': 'شرح موجز لسبب تصنيفك لها منخفضة أو متوسطة أو مرتفعة',
    'N4': 'شرح موجز لسبب تصنيفك لها منخفضة أو متوسطة أو مرتفعة'})

# ===== 11: الأداء المالي =====
STYLE.append("""  .s11 .grid { top: 218px; inset-inline-start: 0; width: 1280px; height: 410px; box-shadow: 0 12px 10px -8px rgb(0 0 0 / .45); }
  .s11 .tl { position: absolute; width: 256px; height: 205px; display: flex; align-items: center; justify-content: center; text-align: center; color: #fff; padding: 0 12px; font-size: 21.3px; line-height: 29px; }
  .s11 img.tl { object-fit: cover; padding: 0; }""")
TILES = [(0, 0, 'fin-1', None), (256, 0, None, 'radial-gradient(circle at 50% 55%, #6C0E92, #3E0557 80%)'), (512, 0, 'fin-2', None),
         (768, 0, None, 'radial-gradient(circle at 50% 55%, #C71F26, #8B1019 85%)'), (1024, 0, 'fin-3', None),
         (0, 205, None, 'radial-gradient(circle at 50% 50%, #221668, #0D0833 85%)'), (256, 205, 'fin-4', None),
         (512, 205, None, 'radial-gradient(circle at 50% 60%, #941067, #5C0A40 85%)'), (768, 205, 'fin-5', None),
         (1024, 205, None, 'radial-gradient(circle at 50% 55%, #F2691F, #B8460A 90%)')]
ALT11 = {'fin-1': 'رجل يحمل مظلة', 'fin-2': 'وجه رجل', 'fin-3': 'ساعة يد', 'fin-4': 'فريق حول طاولة', 'fin-5': 'برج إيفل'}
s11 = brand(11) + ttl('S11T', 150) + '<div class="grid">'
k = 0
for x, y, im, bgc in TILES:
    pos = 'top:%dpx;inset-inline-start:%dpx' % (y, x)
    if im:
        s11 += '<img class="tl" style="%s" src="%s%s.jpg" alt="%s">' % (pos, A, im, ALT11[im])
    else:
        k += 1
        extra = ';font-size:18.6px;line-height:23px' if k == 3 else ''
        s11 += '<p class="tl" style="%s;background:%s%s">§S11P%d§</p>' % (pos, bgc, extra, k)
SLIDES.append(s11 + '</div>')
S(11, {'T': 'Financial Performance', 'P1': 'Use industry benchmarks', 'P2': 'Don’t overwhelm them with numbers– only put numbers in that you’ll speak about and that matter to the challenge',
       'P3': 'A simple-to-read slide (or two) with how the company is performing against its competitors (remember this is the industry section)',
       'P4': 'Can get information from company reports or Reuters, Google Finance, etc', 'P5': 'Use graphs and charts to bring order and increase readability'},
   {'T': 'الأداء المالي', 'P1': 'استخدم المؤشرات المرجعية للقطاع', 'P2': 'لا تُثقل عليهم بالأرقام، وضع فقط الأرقام التي ستتحدث عنها والمهمة للتحدي',
    'P3': 'شريحة (أو اثنتان) سهلة القراءة تبيّن أداء الشركة مقارنة بمنافسيها (تذكّر أن هذا قسم القطاع)',
    'P4': 'يمكن الحصول على المعلومات من تقارير الشركات أو من رويترز وغوغل للمال وغيرهما', 'P5': 'استخدم الرسوم البيانية لتنظيم المعلومات وتسهيل قراءتها'})

# ===== 12: تحليل الشركة =====
SLIDES.append(section(12, 'bg-company', '#9C0F5E', 'يدان على لوحة مفاتيح'))
S(12, {'T': 'company analysis'}, {'T': 'تحليل الشركة'})

# ===== 13: هذا القسم (تحليل الشركة) =====
STYLE.append("""  .s13 .ph { top: 0; inset-inline-start: 0; width: 1280px; height: 458px; object-fit: cover; }
  .s13 h1 { top: %dpx; inset-inline-start: 45px; font-size: 64px; font-weight: 400; line-height: 1.15; color: #fff; white-space: nowrap; }
  html[lang="ar"] .s13 h1 { font-weight: 500; }
  .s13 .sb { top: %dpx; inset-inline-start: 45px; width: 1200px; font-size: 21.3px; line-height: 25.5px; color: #fff; white-space: nowrap; }
  .s13 .pn { top: 458px; height: 262px; }
  .s13 .nt { top: 527px; width: 28px; height: 43px; clip-path: polygon(0 0, 100%% 50%%, 0 100%%); }
  html[dir="rtl"] .s13 .nt { transform: scaleX(-1); }
  .s13 .ic { top: 490px; width: 90px; height: 90px; color: #fff; }
  .s13 .tx { top: %dpx; width: 400px; color: #fff; font-size: 21.3px; line-height: 25px; text-align: center; }""" % (top(390, 64), top(427, 21.3, 25.5), top(623, 21.3, 25)))
s13 = img('hat', 'ph mir', 'رجل يحمل قبعة وعصا') + brand(13, True) + '<h1>§S13T§</h1><p class="sb">§S13S§</p>'
for i, (x, w, col, ic) in enumerate([(0, 427, 'var(--navy)', 'mail'), (427, 426, 'var(--purple)', 'www'), (853, 427, 'var(--magenta)', 'check')]):
    s13 += '<div class="pn" style="inset-inline-start:%dpx;width:%dpx;background:%s"></div>' % (x, w, col)
s13 += '<span class="nt" style="inset-inline-start:427px;background:var(--navy)"></span><span class="nt" style="inset-inline-start:853px;background:var(--purple)"></span>'
for i, (cx, col, ic) in enumerate([(213, 'var(--navy)', 'mail'), (640, 'var(--purple)', 'www'), (1070, 'var(--magenta)', 'check')]):
    s13 += '<span class="ic" style="inset-inline-start:%dpx;--ib:%s">%s</span><p class="tx" style="inset-inline-start:%dpx">§S13P%d§</p>' % (cx - 45, col, IC[ic], cx - 200, i + 1)
SLIDES.append(s13)
S(13, {'T': 'This Section', 'S': 'The company analysis section should show where the company is setup to meet the challenge and where they’re not.',
       'P1': 'Use standard industry analysis frameworks that apply to your situation', 'P2': 'Use also charts, graphs<br>and quotes', 'P3': 'I’ve included some examples on the next few slides of frameworks you could'},
   {'T': 'هذا القسم', 'S': 'ينبغي أن يبيّن قسم تحليل الشركة المواضع التي تستعد فيها الشركة لمواجهة التحدي والمواضع التي لا تستعد فيها.',
    'P1': 'استخدم أطر تحليل القطاع المعتادة التي تناسب وضعك', 'P2': 'استخدم أيضًا الرسوم البيانية<br>والاقتباسات', 'P3': 'أدرجت في الشرائح القليلة التالية أمثلة لأطر يمكنك'})

# ===== 14: عامل النجاح الرئيس =====
STYLE.append("""  .s14 .kp { top: 206px; inset-inline-start: 52px; width: 463px; height: 240px; object-fit: cover; box-shadow: 0 0 0 5px #fff; }
  .s14 .nt { inset-inline-start: 59px; width: 442px; font-size: 18.6px; line-height: 22.4px; color: var(--ink); }
  .s14 table { top: 203px; inset-inline-start: 552px; width: 679px; border-collapse: collapse; table-layout: fixed; }
  .s14 th { height: 87px; padding: 4px 10px 0; vertical-align: top; text-align: start; color: #fff; font-size: 21.3px; line-height: 25.5px; font-weight: 700; }
  .s14 td { height: 125.7px; padding: 10px 10px 0; vertical-align: top; text-align: start; font-size: 21.3px; line-height: 25.5px; color: var(--ink); }
  .s14 tr.a td { background: #CCCBD0; } .s14 tr.b td { background: #E7E7E9; }""")
s14 = (img('bg-ksf', 'bg', 'خلفية باهتة لطفلين أمام حاسوب') + brand(14) + ttl('S14T', 150) + img('kids', 'kp', 'طفلان يحتفلان أمام حاسوب')
       + '<p class="nt" style="top:%dpx">§S14N1§</p><p class="nt" style="top:%dpx">§S14N2§</p>' % (top(489, 18.6, 22.4), top(600, 18.6, 22.4))
       + '<table><tr><th style="background:var(--navy)">§S14H1§</th><th style="background:var(--purple)">§S14H2§</th><th style="background:var(--magenta)">§S14H3§</th></tr>')
for r in range(1, 4):
    s14 += '<tr class="%s">' % ('b' if r == 2 else 'a') + ''.join('<td>§S14R%d%d§</td>' % (r, c) for c in range(1, 4)) + '</tr>'
SLIDES.append(s14 + '</table>')
S(14, {'T': 'Key Success Factor', 'N1': 'This is a simple framework from the book Contemporary Strategy Analysis by Robert M. Grant. You don’t need to use it, but it’s one way to show key success factors in the problem you’re solving.',
       'N2': 'The data in the cells are example only to illustrate how the framework can be used. Replace the information for your own case.',
       'H1': 'CUSTOMERS WANT', 'H2': 'COMPETITION PROVIDES', 'H3': 'KEY SUCCESS FACTORS',
       'R11': 'Feature', 'R12': 'How other competitors are fulfilling feature', 'R13': 'What needs to be place for that feature to happen',
       'R21': 'Smartphones at reasonable prices', 'R22': 'Partner with carriers to offset handset costs', 'R23': 'Carrier deals in multiple markets',
       'R31': 'Products that are easy to use', 'R32': 'Good Interface / touchscreen technology', 'R33': 'User testing, UX professionals, prototyping'},
   {'T': 'عامل النجاح الرئيس', 'N1': 'هذا إطار بسيط من كتاب «تحليل الاستراتيجية المعاصر» لروبرت م. غرانت. لست مضطرًا إلى استخدامه، لكنه إحدى طرق عرض عوامل النجاح الرئيسة في المشكلة التي تعالجها.',
    'N2': 'البيانات في الخلايا أمثلة فقط لتوضيح طريقة استخدام الإطار. استبدل بها معلومات حالتك.',
    'H1': 'ما يريده العملاء', 'H2': 'ما يقدّمه المنافسون', 'H3': 'عوامل النجاح الرئيسة',
    'R11': 'الميزة', 'R12': 'كيف يحقق المنافسون الآخرون هذه الميزة', 'R13': 'ما يلزم توفيره لتتحقق هذه الميزة',
    'R21': 'هواتف ذكية بأسعار معقولة', 'R22': 'الشراكة مع شركات الاتصالات لتعويض تكلفة الأجهزة', 'R23': 'اتفاقيات مع شركات الاتصالات في أسواق متعددة',
    'R31': 'منتجات سهلة الاستخدام', 'R32': 'واجهة جيدة / تقنية شاشات اللمس', 'R33': 'اختبار المستخدمين، وخبراء تجربة المستخدم، والنماذج الأولية'})

# ===== 15: تحليل نقاط القوة والضعف والفرص والتهديدات =====
STYLE.append("""  .s15 .vl { top: 100px; inset-inline-start: 639px; width: 1px; height: 520px; background: #9A9A9A; }
  .s15 .hl { top: 360px; inset-inline-start: 184px; width: 912px; height: 1px; background: #9A9A9A; }
  .s15 .dia { top: 271px; inset-inline-start: 551px; width: 178px; height: 178px; background: var(--navy); transform: rotate(45deg); box-shadow: 0 8px 22px rgb(0 0 0 / .45); }
  .s15 .sw { top: 335px; inset-inline-start: 540px; width: 200px; text-align: center; color: #fff; font-size: 32px; line-height: 50px; }
  .s15 .q { width: 330px; text-align: center; }
  .s15 .q .ic { display: block; margin: 0 auto; }
  .s15 .q h2 { font-size: 31.8px; line-height: 36px; font-weight: 700; margin-top: 0; }
  .s15 .q p { font-size: 21.2px; line-height: 25px; color: #3A4656; margin-top: 6px; }""")
s15 = img('bg-swot', 'bg', 'خلفية باهتة لنظارة') + brand(15) + '<span class="vl"></span><span class="hl"></span><span class="dia"></span><p class="sw">§S15SW§</p>'
for i, (cx, ty, col, ic, iw, ih, ityp) in enumerate([(352, 132, 'var(--purple)', 'dumbbell', 110, 110, 241), (940, 139, 'var(--magenta)', 'chain', 90, 90, 241),
                                                      (352, 409, 'var(--red)', 'key', 84, 84, 518), (940, 410, 'var(--orange)', 'bomb', 88, 88, 518)]):
    s15 += ('<span class="ic" style="position:absolute;top:%dpx;inset-inline-start:%dpx;width:%dpx;height:%dpx;color:%s">%s</span>' % (ty, cx - iw // 2, iw, ih, col, IC[ic])
            + '<div class="q" style="top:%dpx;inset-inline-start:%dpx"><h2 style="color:%s">§S15H%d§</h2><p>§S15P%d§</p></div>' % (ityp, cx - 165, col, i + 1, i + 1))
SLIDES.append(s15)
S(15, {'SW': 'SWOT', 'H1': 'STRENGTHS', 'P1': 'Internal company strength', 'H2': 'WEAKNESS', 'P2': 'An internal company weakness',
       'H3': 'OPPORTUNITIES', 'P3': 'An industry opportunity that the company can take advantage of', 'H4': 'THREATS', 'P4': 'An industry trend that the company can’t take advantage of'},
   {'SW': 'سوات', 'H1': 'نقاط القوة', 'P1': 'قوة داخلية في الشركة', 'H2': 'نقاط الضعف', 'P2': 'ضعف داخلي في الشركة',
    'H3': 'الفرص', 'P3': 'فرصة في القطاع يمكن للشركة الاستفادة منها', 'H4': 'التهديدات', 'P4': 'اتجاه في القطاع لا تستطيع الشركة الاستفادة منه'})

# ===== 16: البدائل =====
SLIDES.append(section(16, 'bg-alternative', '#C91B25', 'رجل يمسك هاتفًا', False))
S(16, {'T': 'alternative (options)'}, {'T': 'البدائل (الخيارات)'})

# ===== 17: هذا القسم (البدائل) =====
STYLE.append("""  .s17 .rw { inset-inline-start: 186px; width: 960px; height: 119px; background: #D9D9D9; box-shadow: 0 10px 8px -6px rgb(0 0 0 / .45); }
  .s17 .rw > * { position: absolute; }
  .s17 .sq { top: 0; inset-inline-start: 0; width: 154px; height: 119px; }
  .s17 .nt { top: 42px; inset-inline-start: 154px; width: 17px; height: 34px; clip-path: polygon(0 0, 100% 50%, 0 100%); }
  html[dir="rtl"] .s17 .nt { transform: scaleX(-1); }
  .s17 .ic { top: 24px; inset-inline-start: 39px; width: 76px; height: 72px; color: #fff; }
  .s17 .tx { top: 0; inset-inline-start: 195px; width: 740px; height: 119px; display: flex; align-items: center; font-size: 21.3px; line-height: 27px; color: var(--ink); }""")
s17 = img('bg-alts', 'bg', 'خلفية باهتة لرجل ينظر بتركيز') + brand(17) + ttl('S17T', 190)
for i, (y, col, ic) in enumerate([(236, 'var(--navy)', 'team'), (372, 'var(--purple)', 'pin'), (518, 'var(--magenta)', 'mail')]):
    s17 += ('<div class="rw" style="top:%dpx"><span class="sq" style="background:%s"></span><span class="nt" style="background:%s"></span>'
            '<span class="ic" style="--ib:%s">%s</span><p class="tx"%s>§S17P%d§</p></div>' % (y, col, col, col, IC[ic], ' style="width:650px"' if i == 2 else '', i + 1))
SLIDES.append(s17)
S(17, {'T': 'This Section', 'P1': 'The alternatives section should demonstrate you did your homework. You looked at three (always three!) possible solutions and you picked the best one.',
       'P2': 'Alternatives should follow the mutually exclusive and collectively exhaustive (MECE) principle.',
       'P3': 'Always list the solution that you came up with third, as it will then be top of mind when you present your implementation plan.'},
   {'T': 'هذا القسم', 'P1': 'ينبغي أن يُظهر قسم البدائل أنك أدّيت ما عليك؛ فقد درست ثلاثة حلول ممكنة (ثلاثة دائمًا!) واخترت أفضلها.',
    'P2': 'ينبغي أن تتبع البدائل مبدأ «التنافي والشمول»: لا تتداخل فيما بينها وتغطي الاحتمالات كلها.',
    'P3': 'اذكر الحل الذي توصلت إليه ثالثًا دائمًا، ليبقى حاضرًا في الأذهان عند عرض خطة التنفيذ.'})

# ===== 18: القضية الرئيسة / معايير القرار =====
STYLE.append("""  .s18 .ph { top: 20px; inset-inline-start: 100px; width: 490px; height: 700px; object-fit: cover; }
  .s18 h1 { top: %dpx; inset-inline-start: 630px; font-size: 64px; line-height: 68px; font-weight: 400; white-space: nowrap; }
  .s18 .pl { inset-inline-start: 621px; width: 574px; height: 80px; border-radius: 40px; background: var(--slate); display: flex; align-items: center; color: #fff; }
  .s18 .pl b { flex: none; width: 58px; height: 58px; border-radius: 50%%; margin-inline-start: 13px; display: grid; place-items: center; font-size: 21.3px; font-weight: 400; }
  .s18 .pl p { margin-inline-start: 16px; font-size: 21.3px; line-height: 25px; width: 450px; }""" % top(195, 64, 68))
s18 = img('camera', 'ph', 'فتاة تلتقط صورة بكاميرا') + brand(18) + '<h1 class="gtt"><span class="gt">§S18T1§<br>§S18T2§</span></h1>'
for i, (y, col, tc) in enumerate([(313, 'var(--red)', '#fff'), (424, 'var(--orange)', '#fff'), (536, '#F8E41E', '#222')]):
    s18 += '<div class="pl" style="top:%dpx"><b style="background:%s;color:%s">%d</b><p>§S18P%d§</p></div>' % (y, col, tc, i + 1, i + 1)
SLIDES.append(s18)
S(18, {'T1': 'Key Issue/', 'T2': 'Decision Criteria', 'P1': 'Recap the key challenge', 'P2': 'List additional key issues that fall<br>under this challenge', 'P3': 'The key issues become your decision criteria'},
   {'T1': 'القضية الرئيسة/', 'T2': 'معايير القرار', 'P1': 'لخّص التحدي الرئيس', 'P2': 'اذكر القضايا الرئيسة الأخرى التي تندرج تحت هذا التحدي', 'P3': 'تصبح القضايا الرئيسة معايير قرارك'})

# ===== 19: الخيار الأول =====
STYLE.append("""  .s19 .cd { top: 261px; width: 356px; height: 387px; border-radius: 18px; background: #263445; box-shadow: 0 6px 18px rgb(0 0 0 / .35); overflow: hidden; }
  .s19 .cd > * { position: absolute; }
  .s19 .lip { top: 0; inset-inline-start: 0; width: 356px; height: 164px; border-radius: 0 0 18px 18px; }
  .s19 .hd { top: 0; inset-inline-start: 0; width: 356px; height: 149px; border-radius: 0 0 18px 18px; color: #fff; font-weight: 400; font-size: 42.4px; line-height: 149px; text-align: center; }
  .s19 .tip { top: 163px; inset-inline-start: 160px; width: 36px; height: 28px; clip-path: polygon(0 0, 100%% 0, 50%% 100%%); }
  .s19 .bd { top: 191px; inset-inline-start: 0; width: 356px; height: 180px; display: flex; align-items: center; justify-content: center; text-align: center; color: #fff; font-size: 21.3px; line-height: 26px; padding: 0 18px; }
  .s19 .desc, .s20 .desc { top: %dpx; inset-inline-start: 0; width: 1280px; text-align: center; font-size: 23.9px; line-height: 28px; color: var(--ink); }""" % top(218, 23.9, 28))
s19 = img('bg-opt1', 'bg', 'خلفية ضبابية لمكتب') + brand(19) + ttl('S19T', 150) + '<p class="desc">§S19D§</p>'
for i, (x, c1, c2) in enumerate([(272, 'var(--purple)', '#48035C'), (652, 'var(--magenta)', '#760D48')]):
    s19 += ('<div class="cd" style="inset-inline-start:%dpx"><span class="lip" style="background:%s"></span><span class="tip" style="background:%s"></span>'
            '<h2 class="hd" style="background:%s">§S19H%d§</h2><p class="bd">§S19B%d§</p></div>' % (x, c2, c2, c1, i + 1, i + 1))
SLIDES.append(s19)
S(19, {'T': 'Option #1: Name of Option', 'D': 'A brief description of what this option is.', 'H1': 'Pros', 'H2': 'Cons',
       'B1': 'Key issue this<br>option solves', 'B2': 'What key issues it doesn’t solve<br><br>What gaps the company has in its ability to execute on this option'},
   {'T': 'الخيار رقم 1: اسم الخيار', 'D': 'وصف موجز لماهية هذا الخيار.', 'H1': 'المزايا', 'H2': 'العيوب',
    'B1': 'القضية الرئيسة التي<br>يحلّها هذا الخيار', 'B2': 'القضايا الرئيسة التي لا يحلّها<br><br>الفجوات في قدرة الشركة على تنفيذ هذا الخيار'})

# ===== 20: الخيار الثاني =====
STYLE.append("""  .s20 .cd { top: 267px; width: 486px; height: 328px; background: #D1DBE5; box-shadow: 0 14px 10px -8px rgb(0 0 0 / .5); }
  .s20 .cd > * { position: absolute; }
  .s20 .st { top: 0; inset-inline-start: 0; width: 119px; height: 328px; }
  .s20 .nt { top: 149px; inset-inline-start: 119px; width: 25px; height: 30px; clip-path: polygon(0 0, 100% 50%, 0 100%); }
  html[dir="rtl"] .s20 .nt { transform: scaleX(-1); }
  .s20 .vt { font-weight: 400; top: 0; inset-inline-start: 0; width: 119px; height: 328px; display: flex; align-items: center; justify-content: center; }
  .s20 .vt span { transform: rotate(-90deg); color: #fff; font-size: 40px; line-height: 1; white-space: nowrap; }
  .s20 .bd { top: 0; inset-inline-start: 186px; width: 262px; height: 328px; display: flex; flex-direction: column; justify-content: center; font-size: 23.9px; line-height: 29px; color: var(--ink); }""")
s20 = img('bg-opt2', 'bg', 'خلفية ضبابية لمكتب مضيء') + brand(20) + ttl('S20T', 150) + '<p class="desc">§S20D§</p>'
for i, (x, col) in enumerate([(144, 'var(--navy)'), (650, 'var(--magenta)')]):
    s20 += ('<div class="cd" style="inset-inline-start:%dpx"><span class="st" style="background:%s"></span><span class="nt" style="background:%s"></span>'
            '<h2 class="vt"><span>§S20H%d§</span></h2><p class="bd" style="inset-inline-start:%dpx">§S20B%d§</p></div>' % (x, col, col, i + 1, 186 if i == 0 else 176, i + 1))
SLIDES.append(s20)
S(20, {'T': 'Option #2: Name of Option', 'D': 'A brief description of what this option is.', 'H1': 'Pros', 'H2': 'Cons',
       'B1': 'Key issue this option solves<br><br>Another key issue it solves', 'B2': 'What key issues it doesn’t solve<br><br>What gaps the company has in its ability to execute on this option'},
   {'T': 'الخيار رقم 2: اسم الخيار', 'D': 'وصف موجز لماهية هذا الخيار.', 'H1': 'المزايا', 'H2': 'العيوب',
    'B1': 'القضية الرئيسة التي يحلّها هذا الخيار<br><br>قضية رئيسة أخرى يحلّها', 'B2': 'القضايا الرئيسة التي لا يحلّها<br><br>الفجوات في قدرة الشركة على تنفيذ هذا الخيار'})

# ===== 21: الخيار الثالث =====
STYLE.append("""  .s21 .desc { top: %dpx; inset-inline-start: 190px; width: 900px; text-align: center; font-size: 21.3px; line-height: 25.5px; color: var(--ink); }
  .s21 .cd { top: 299px; width: 340px; height: 337px; background: #fff; box-shadow: 0 12px 10px -8px rgb(0 0 0 / .55); }
  .s21 .cd > * { position: absolute; }
  .s21 .hd { top: 0; inset-inline-start: 0; width: 340px; height: 71px; color: #fff; font-size: 32px; line-height: 71px; text-align: center; font-weight: 400; box-shadow: 0 10px 8px -8px rgb(0 0 0 / .6); }
  .s21 .b1, .s21 .b2 { inset-inline-start: 30px; width: 280px; text-align: center; font-size: 21.3px; line-height: 25.5px; color: var(--ink); display: flex; align-items: center; justify-content: center; }
  .s21 .b1 { top: 95px; height: 90px; } .s21 .b2 { top: 205px; height: 115px; }
  .s21 .dv { top: 194px; inset-inline-start: 42px; width: 257px; height: 1px; background: #D0D0D0; }""" % top(218, 21.3, 25.5))
s21 = img('bg-opt3', 'bg', 'خلفية ضبابية لكوب وحاسوب') + brand(21) + ttl('S21T', 150) + '<p class="desc">§S21D§</p>'
for i, (x, col) in enumerate([(292, 'var(--navy)'), (650, 'var(--purple)')]):
    s21 += ('<div class="cd" style="inset-inline-start:%dpx"><h2 class="hd" style="background:%s">§S21H%d§</h2><p class="b1">§S21A%d§</p>'
            '<span class="dv"></span><p class="b2">§S21B%d§</p></div>' % (x, col, i + 1, i + 1, i + 1))
SLIDES.append(s21)
S(21, {'T': 'Option #3: Name of Option', 'D': 'A brief description of what this option is. This option will be the same as the solution you presented at the beginning of your presentation',
       'H1': 'Pros', 'H2': 'Cons', 'A1': 'Key issue this option solves', 'B1': 'As this is option #3, it should be your recommendation. It should also',
       'A2': 'What key issues it<br>doesn’t solve', 'B2': 'What gaps the company has in its ability to execute on this option'},
   {'T': 'الخيار رقم 3: اسم الخيار', 'D': 'وصف موجز لماهية هذا الخيار. وسيكون هذا الخيار هو الحل نفسه الذي عرضته في بداية عرضك',
    'H1': 'المزايا', 'H2': 'العيوب', 'A1': 'القضية الرئيسة التي يحلّها هذا الخيار', 'B1': 'بما أن هذا هو الخيار رقم 3، فينبغي أن يكون توصيتك. وينبغي أيضًا',
    'A2': 'القضايا الرئيسة التي<br>لا يحلّها', 'B2': 'الفجوات في قدرة الشركة على تنفيذ هذا الخيار'})

# ===== 22: القضايا الرئيسة والحلول المرتبطة بها =====
STYLE.append("""  .s22 .sub { top: %dpx; inset-inline-start: 0; width: 1280px; text-align: center; font-size: 21.3px; line-height: 25.5px; color: var(--ink); }
  .s22 table { top: 279px; inset-inline-start: 120px; width: 1009px; border-collapse: collapse; table-layout: fixed; }
  .s22 th { height: 88px; color: #fff; font-size: 18.7px; line-height: 23px; font-weight: 400; text-align: center; padding: 0 6px; }
  .s22 td { height: 88px; text-align: center; padding: 0; }
  .s22 td.lb { color: #fff; font-size: 21.3px; line-height: 25px; }
  .s22 .ck { display: inline-grid; place-items: center; width: 48px; height: 48px; border-radius: 50%%; vertical-align: middle; }
  .s22 .ck svg { width: 26px; height: 26px; }""" % top(206, 21.3, 25.5))
COLS22 = [('#160B47', '#6343E4', '#B0A1F2', '#7B62E8'), ('#5F0479', '#C820F9', '#DA6AFA', '#C93AF0'), ('#9C0F5E', '#ED45A5', '#F483C3', '#E44597'),
          ('#C91B25', '#EB6971', '#F29BA1', '#E2626A'), ('#F26B21', '#F7A478', '#F9C2A4', '#F2A27A')]
CHK = '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="m5 12.5 4.6 4.6L19 6.4" fill="none" stroke="%s" stroke-width="2.8" stroke-linecap="round" stroke-linejoin="round"/></svg>'
MARKS = [[1, 1, 0, 0, 0], [1, 1, 0, 0, 1], [2, 2, 2, 2, 2]]   # 0 فارغ، 1 دائرة بيضاء، 2 دائرة داكنة
s22 = brand(22) + ttl('S22T', 150) + '<p class="sub">§S22S§</p><table><tr><th style="background:#34495E;width:128px"></th>'
for j, (h, _, _, _) in enumerate(COLS22):
    s22 += '<th style="background:%s%s">§S22H%d§</th>' % (h, ';font-size:16px;line-height:19.5px' if j == 4 else '', j + 1)
s22 += '</tr>'
for r in range(3):
    s22 += '<tr><td class="lb" style="background:%s">§S22O%d§</td>' % (['#233446', '#7192B1', '#273746'][r], r + 1)
    for j, (_, c1, c2, ck) in enumerate(COLS22):
        cell = c2 if r == 1 else c1
        m = MARKS[r][j]
        inner = '' if m == 0 else '<span class="ck" style="background:%s">%s</span>' % ('#fff' if m == 1 else '#1F2435', CHK % (ck if m == 1 and r == 0 else (c2 if r == 1 else c1)))
        s22 += '<td style="background:%s">%s</td>' % (cell, inner)
    s22 += '</tr>'
SLIDES.append(s22 + '</table>')
S(22, {'T': 'Key Issues &amp; Related Solutions', 'S': 'A chart with checkmarks shows at-a-glace which option best solves the key issues / decision criteria.',
       'H1': 'Key issue<br>/decision criteria<br>#1', 'H2': 'Key issue<br>/decision criteria<br>#2', 'H3': 'Key issue<br>/decision criteria<br>#3', 'H4': 'Key issue<br>/decision criteria<br>#4',
       'H5': 'Timing in Market<br>Entry Key issue<br>/decision criteria &nbsp;#5', 'O1': 'Option #1:<br>title', 'O2': 'Option #2:<br>title', 'O3': 'Option #3:<br>title'},
   {'T': 'القضايا الرئيسة والحلول المرتبطة بها', 'S': 'يبيّن جدول بعلامات الاختيار بنظرة سريعة أيّ الخيارات يحلّ القضايا الرئيسة / معايير القرار على أفضل وجه.',
    'H1': 'القضية الرئيسة<br>/معيار القرار<br>رقم 1', 'H2': 'القضية الرئيسة<br>/معيار القرار<br>رقم 2', 'H3': 'القضية الرئيسة<br>/معيار القرار<br>رقم 3', 'H4': 'القضية الرئيسة<br>/معيار القرار<br>رقم 4',
    'H5': 'توقيت دخول السوق<br>القضية الرئيسة<br>/معيار القرار رقم 5', 'O1': 'الخيار رقم 1:<br>العنوان', 'O2': 'الخيار رقم 2:<br>العنوان', 'O3': 'الخيار رقم 3:<br>العنوان'})

# ===== 23: التوصية =====
STYLE.append("""  .s23 .ring { top: 92px; inset-inline-start: 626px; width: 568px; height: 568px; border-radius: 50%; background: rgb(20 20 26 / .55); }
  .s23 .cir { top: 144px; inset-inline-start: 678px; width: 464px; height: 464px; border-radius: 50%; background: linear-gradient(180deg, #1A0470, #2C0CAC); border: 3px solid #4B22EE; box-shadow: 0 0 22px 6px rgb(0 0 0 / .45); }
  .s23 h1 { top: 313px; inset-inline-start: 710px; width: 400px; text-align: center; font-size: 42.4px; line-height: 49px; font-weight: 400; color: #fff; }
  html[lang="ar"] .s23 h1 { font-weight: 700; }
  .s23 p { top: 386px; inset-inline-start: 735px; width: 350px; text-align: center; font-size: 23.9px; line-height: 29.5px; color: #fff; }""")
SLIDES.append(img('bg-reco', 'bg', 'يدان على لوحة مفاتيح') + brand(23, True) + '<span class="ring"></span><span class="cir"></span><h1>§S23T§</h1><p>§S23P§</p>')
S(23, {'T': 'Recommendation', 'P': 'Option #3, addressed most of the key issues and solves the problem of X'},
   {'T': 'التوصية', 'P': 'الخيار رقم 3، إذ عالج معظم القضايا الرئيسة ويحلّ مشكلة «س»'})

# ===== 24: التنفيذ =====
SLIDES.append(section(24, 'bg-implementation', '#5F0479', 'حاسوب محمول وكوب على مكتب'))
S(24, {'T': 'implementation'}, {'T': 'التنفيذ'})

# ===== 25: هذا القسم (التنفيذ) =====
STYLE.append("""  .s25 .lp { top: 133px; inset-inline-start: 0; width: 264px; height: 473px; background: var(--slate); color: #fff; text-align: center; }
  .s25 .lp h1 { position: absolute; top: %dpx; inset-inline-start: 0; width: 264px; font-size: 58px; line-height: 63px; font-weight: 400; color: #EF86CB; }
  html[lang="ar"] .s25 .lp h1 { font-weight: 700; font-size: 52px; }
  .s25 .lp p { position: absolute; top: %dpx; inset-inline-start: 34px; width: 196px; font-size: 21.3px; line-height: 25px; }
  .s25 .ph { top: 133px; inset-inline-start: 270px; width: 404px; height: 474px; object-fit: cover; }
  .s25 .rw { inset-inline-start: 680px; width: 600px; height: 119px; box-shadow: 0 8px 8px -6px rgb(0 0 0 / .45); }
  .s25 .rw > * { position: absolute; }
  .s25 .sq { top: 19px; inset-inline-start: 18px; width: 85px; height: 80px; padding: 13px 16px; color: #fff; }
  .s25 .tx { top: 0; inset-inline-start: 130px; width: 440px; height: 119px; display: flex; align-items: center; color: #fff; font-size: 23.9px; line-height: 29px; }""" % (top(318, 58, 63) - 133, top(432, 21.3, 25) - 133))
s25 = brand(25) + '<div class="lp"><h1>§S25T§</h1><p>§S25S§</p></div>' + img('cup', 'ph', 'فنجان قهوة على مكتب خشبي')
for i, (y, col, sq, ic) in enumerate([(133, 'var(--navy)', '#6343E4', 'bag'), (251, 'var(--purple)', '#C820F9', 'meeting'), (370, 'var(--magenta)', '#E747A7', 'trio'), (488, 'var(--red)', '#EB6971', 'monitor')]):
    s25 += '<div class="rw" style="top:%dpx;background:%s;z-index:%d"><span class="sq ic" style="background:%s;--ib:%s">%s</span><p class="tx">§S25P%d§</p></div>' % (y, col, 4 - i, sq, sq, IC[ic], i + 1)
SLIDES.append(s25)
S(25, {'T': 'This<br>Section', 'S': 'The implementation section should cover:', 'P1': 'Risks to your plan and also plans<br>to mitigate those risks', 'P2': 'High level timeline to implement<br>the plan',
       'P3': 'A communications priority plan. I use a matrix, but you don’t need to.', 'P4': 'Delete this slide in your actual<br>presentation'},
   {'T': 'هذا<br>القسم', 'S': 'ينبغي أن يغطي قسم التنفيذ:', 'P1': 'المخاطر التي تواجه خطتك وخطط الحد منها', 'P2': 'جدول زمني عام لتنفيذ الخطة',
    'P3': 'خطة أولويات التواصل. أستخدم مصفوفة، لكنك لست مضطرًا إلى ذلك.', 'P4': 'احذف هذه الشريحة من عرضك الفعلي'})

# ===== 26: الحد من المخاطر =====
STYLE.append("""  .s26 .lf { top: 290px; width: 146px; height: 148px; border-radius: 50% 50% 0 50%; color: #fff; padding: 30px; }
  html[dir="rtl"] .s26 .lf { border-radius: 50% 50% 50% 0; }
  .s26 .tx { top: 469px; width: 200px; text-align: end; font-size: 18.6px; line-height: 33.6px; color: #3A3F47; }""")
s26 = img('bg-risk', 'bg', 'خلفية باهتة لسلك شائك') + brand(26) + ttl('S26T', 213)
for i, (x, col, ic) in enumerate([(137, 'var(--navy)', 'search'), (354, 'var(--purple)', 'person'), (571, 'var(--magenta)', 'flow'), (788, 'var(--red)', 'clickcash'), (1006, 'var(--orange)', 'currency')]):
    w26 = [195, 165, 160, 160, 135][i]
    s26 += '<span class="lf ic" style="inset-inline-start:%dpx;background:%s;--ib:%s">%s</span><p class="tx" style="inset-inline-start:%dpx;width:%dpx">§S26P%d§</p>' % (x, col, col, IC[ic], x + 137 - w26, w26, i + 1)
SLIDES.append(s26)
S(26, {'T': 'Risk Mitigation', 'P1': 'List the challenges that would prevent the company from successfully executing your proposed solution', 'P2': 'List risk mitigation strategies for every challenge',
       'P3': 'Don’t skip obvious risks if you don’t have a way to mitigate the risk', 'P4': 'Think of a way you could mitigate every risk, even if that means the', 'P5': 'Don’t list superfluous risks– just ones that matter'},
   {'T': 'الحد من المخاطر', 'P1': 'اذكر التحديات التي قد تمنع الشركة من تنفيذ الحل المقترح بنجاح', 'P2': 'اذكر استراتيجيات الحد من المخاطر لكل تحدٍّ',
    'P3': 'لا تتجاهل المخاطر الواضحة إن لم تكن لديك وسيلة للحد منها', 'P4': 'فكّر في وسيلة للحد من كل خطر، حتى لو عنى ذلك', 'P5': 'لا تذكر مخاطر ثانوية، بل المهمة فقط'})

# ===== 27–28: الجدول الزمني للتنفيذ =====
STYLE.append("""  .tm > * { position: absolute; }
  .tm .ln { inset-inline-start: 640px; width: 1px; background: #D5D5D5; }
  .tm .dot { width: 58px; height: 58px; border-radius: 50%; inset-inline-start: 611px; }
  .tm .lbl { font-size: 42.2px; line-height: 50px; font-weight: 700; font-style: italic; white-space: nowrap; text-shadow: 1px 1px 1px rgb(0 0 0 / .15); }
  html[lang="ar"] .tm .lbl { font-style: normal; font-weight: 800; }
  .tm .lbl.e { inset-inline-start: 200px; width: 383px; text-align: end; }
  .tm .cd { width: 433px; height: 153px; }
  .tm .cd > * { position: absolute; }
  .tm .hd { top: 0; inset-inline-start: 0; width: 433px; height: 76px; border-radius: 12px 12px 0 0; color: #fff; font-size: 23.9px; line-height: 76px; font-weight: 700; padding-inline: 28px; white-space: nowrap; z-index: 1; box-shadow: 0 10px 8px -8px rgb(0 0 0 / .6); }
  .tm .hd.e { text-align: end; padding-inline-end: 37px; }
  .tm .bd { top: 76px; inset-inline-start: 0; width: 433px; height: 77px; border-radius: 0 0 12px 12px; background: #D9D9D9; box-shadow: 0 3px 6px rgb(0 0 0 / .35); font-size: 21.3px; line-height: 77px; color: #3A4656; padding-inline: 29px; white-space: nowrap; }
  .tm .ar { top: 26px; width: 15px; height: 24px; }
  .tm .ar.l { inset-inline-start: -14px; clip-path: polygon(100% 0, 0 50%, 100% 100%); }
  .tm .ar.r { inset-inline-start: 432px; clip-path: polygon(0 0, 100% 50%, 0 100%); }
  html[dir="rtl"] .tm .ar { transform: scaleX(-1); }""")


def tl_dot(y, c1, c2):
    return '<span class="dot" style="top:%dpx;background:radial-gradient(circle at 50%% 95%%, %s 0, %s 55%%);box-shadow:inset 0 -3px 6px rgb(255 255 255 / .2)"></span>' % (y, c2, c1)


def tl_card(x, y, col, side, hk, bk, bd_pad=None):
    hcls = 'hd e' if side == 'r' else 'hd'
    pad = ' style="padding-inline-start:%dpx"' % bd_pad if bd_pad else ''
    return ('<div class="cd" style="top:%dpx;inset-inline-start:%dpx"><span class="ar %s" style="background:%s"></span><h2 class="%s" style="background:%s">%s</h2><p class="bd"%s>%s</p></div>'
            % (y, x, side, col, hcls, col, hk, pad, bk))


s27 = ('<div class="tm" style="position:absolute;inset:0"><span class="ln" style="position:absolute;top:325px;height:395px"></span>' + brand(27) + ttl('S27T', 150)
       + tl_dot(269, '#160B47', '#4A3BA8') + '<p class="lbl e" style="position:absolute;top:%dpx;color:#1A1049">§S27L1§</p>' % top(315, 42.2, 50)
       + tl_card(712, 251, 'var(--navy)', 'l', '§S27H1§', '§S27B1§')
       + tl_dot(478, '#5F0479', '#B02CCB') + '<p class="lbl" style="position:absolute;top:%dpx;inset-inline-start:698px;color:#5F0479">§S27L2§</p>' % top(524, 42.2, 50)
       + tl_card(135, 478, 'var(--purple)', 'r', '§S27H2§', '§S27B2§', 69) + '</div>')
s27 = s27.replace('<div class="tm" style="position:absolute;inset:0">', '<div class="tm" style="position:absolute;inset:0">').replace('class="dot"', 'class="dot" data-p').replace(' data-p', '')
SLIDES.append(s27)
S(27, {'T': 'Implementation Timeline', 'L1': 'SHORT TERM', 'L2': 'MID TERM', 'H1': 'Phase I:&nbsp; Project to complete', 'B1': 'Requirements to complete project',
       'H2': 'Phase II: Project to complete', 'B2': 'Requirements to complete project'},
   {'T': 'الجدول الزمني للتنفيذ', 'L1': 'المدى القصير', 'L2': 'المدى المتوسط', 'H1': 'المرحلة الأولى: مشروع يُنجز', 'B1': 'متطلبات إنجاز المشروع',
    'H2': 'المرحلة الثانية: مشروع يُنجز', 'B2': 'متطلبات إنجاز المشروع'})

s28 = ('<div class="tm" style="position:absolute;inset:0"><span class="ln" style="position:absolute;top:0;height:480px"></span>' + brand(28)
       + tl_dot(214, '#9C0F5E', '#E0479F') + '<p class="lbl e" style="position:absolute;top:%dpx;inset-inline-start:185px;color:#9C0F5E">§S28L1§</p>' % top(263, 42.2, 50)
       + tl_card(698, 204, 'var(--magenta)', 'l', '§S28H1§', '§S28B1§')
       + tl_dot(478, '#C91B25', '#F0525A') + '<p class="lbl" style="position:absolute;top:%dpx;inset-inline-start:703px;color:#C91B25">§S28L2§</p>' % top(520, 42.2, 50)
       + tl_card(135, 478, 'var(--red)', 'r', '§S28H2§', '§S28B2§', 27) + '</div>')
SLIDES.append(s28)
S(28, {'L1': 'LONG TERM', 'L2': '2015-2020', 'H1': 'Phase III:&nbsp; Project to complete', 'B1': 'Requirements to complete project',
       'H2': 'Phase IV: Project to complete', 'B2': 'Strategic goals achieved, analyze, revisit'},
   {'L1': 'المدى الطويل', 'L2': '2015-2020', 'H1': 'المرحلة الثالثة: مشروع يُنجز', 'B1': 'متطلبات إنجاز المشروع',
    'H2': 'المرحلة الرابعة: مشروع يُنجز', 'B2': 'أهداف استراتيجية تحققت، حلّل، راجع'})

# ===== 29: خطة التواصل =====
STYLE.append("""  .s29 .bs { height: 52px; background: var(--c); clip-path: var(--cp); }
  html[dir="rtl"] .s29 .bs { transform: scaleX(-1); }
  .s29 .btx { height: 52px; color: #fff; display: flex; align-items: center; white-space: nowrap; }
  .s29 ul { list-style: none; padding: 0; font-size: 18.6px; line-height: 22.6px; color: var(--ink); }
  .s29 li { position: relative; padding-inline-start: 27px; }
  .s29 li::before { content: ""; position: absolute; inset-inline-start: 2px; top: 7px; width: 7px; height: 7px; border: 1.3px solid currentColor; border-radius: 50%; }
  .s29 .ax { font-size: 18.6px; line-height: 22px; color: #262626; }
  .s29 .sk { top: 192px; inset-inline-start: 497px; width: 308px; height: 435px; object-fit: cover; }""")
s29 = brand(29) + ttl('S29T', 140)
BANDS29 = [(250, 175, 350, 'var(--navy)', 'polygon(0 0, 100% 0, 100% 100%, 47px 100%)', 57, 'K1', 21.3),
           (411, 135, 385, 'var(--magenta)', 'polygon(0 0, 100% 0, 100% 100%, 48px 100%)', 58, 'K2', 21.3),
           (268, 645, 386, 'var(--purple)', 'polygon(0 0, 100% 0, calc(100% - 46px) 100%, 0 100%)', 48, 'K3', 18.6),
           (467, 700, 389, 'var(--red)', 'polygon(0 0, 100% 0, calc(100% - 46px) 100%, 0 100%)', 101, 'K4', 21.3)]
for y, x, w, col, cp, pad, k, fs in BANDS29:
    s29 += '<span class="bs" style="top:%dpx;inset-inline-start:%dpx;width:%dpx;--c:%s;--cp:%s"></span>' % (y, x, w, col, cp)
s29 += img('skater', 'sk mir', 'متزلجة على لوح تزلج')
for y, x, w, col, cp, pad, k, fs in BANDS29:
    s29 += '<p class="btx" style="top:%dpx;inset-inline-start:%dpx;padding-inline-start:%dpx;font-size:%spx">§S29%s§</p>' % (y, x, pad, fs, k)
for x, b, n, k in [(250, 340, 2, 'A'), (250, 496, 3, 'B'), (790, 364, 4, 'C'), (742, 566, 3, 'D')]:
    s29 += '<ul style="top:%dpx;inset-inline-start:%dpx;width:%dpx">' % (top(b, 18.6, 22.6), x, {'A': 200, 'B': 245, 'C': 260, 'D': 400}[k]) + ''.join('<li>§S29%s%d§</li>' % (k, i) for i in range(1, n + 1)) + '</ul>'
s29 += ('<svg style="top:425px;inset-inline-start:66px;width:12px;height:200px" viewBox="0 0 12 200" aria-hidden="true"><path d="M6 6v188" stroke="#BDBDBD" stroke-width="1.2"/><path d="M2 9 6 2l4 7zM2 191l4 7 4-7z" fill="#BDBDBD"/></svg>'
        '<svg class="mir" style="top:651px;inset-inline-start:114px;width:198px;height:12px" viewBox="0 0 198 12" aria-hidden="true"><path d="M6 6h186" stroke="#BDBDBD" stroke-width="1.2"/><path d="M9 2 2 6l7 4zM189 2l7 4-7 4z" fill="#BDBDBD"/></svg>'
        '<p class="ax" style="top:%dpx;inset-inline-start:52px">§S29H§</p><p class="ax" style="top:%dpx;inset-inline-start:55px">§S29L§</p><p class="ax" style="top:%dpx;inset-inline-start:332px">§S29H§</p>'
        % (top(398, 18.6, 22), top(665, 18.6, 22), top(665, 18.6, 22)))
SLIDES.append(s29)
S(29, {'T': 'Communication Plan', 'K1': 'Keep satisfied', 'K2': 'Monitor (minimal effort):', 'K3': 'Engage closely, influence actively:', 'K4': 'Keep informed',
       'A1': 'Partners and suppliers', 'A2': 'Government and regulators', 'B1': 'NGOs', 'B2': 'Professional associations', 'B3': 'Media',
       'C1': 'Customers', 'C2': 'Owners', 'C3': 'Employees', 'C4': 'Investors and creditors',
       'D1': 'Companies being analyzed', 'D2': 'Standards bodies (GRI, CDP, ISO 26000)', 'D3': 'Competitors', 'H': 'High', 'L': 'Low'},
   {'T': 'خطة التواصل', 'K1': 'حافظ على رضاهم', 'K2': 'راقب (بأقل جهد):', 'K3': 'أشرِك عن قرب وأثّر بفاعلية:', 'K4': 'أبقِهم على اطلاع',
    'A1': 'الشركاء والموردون', 'A2': 'الجهات الحكومية والتنظيمية', 'B1': 'المنظمات غير الحكومية', 'B2': 'الجمعيات المهنية', 'B3': 'وسائل الإعلام',
    'C1': 'العملاء', 'C2': 'الملاك', 'C3': 'الموظفون', 'C4': 'المستثمرون والدائنون',
    'D1': 'الشركات موضع التحليل', 'D2': 'هيئات المعايير (المبادرة العالمية لإعداد التقارير، مشروع الإفصاح الكربوني، آيزو 26000)', 'D3': 'المنافسون', 'H': 'مرتفع', 'L': 'منخفض'})

# ===== 30: الخلاصة =====
STYLE.append("""  .s30 .b1 { top: 422px; inset-inline-start: 0; width: 401px; height: 176px; background: var(--magenta); opacity: .8; }
  .s30 .b2 { top: 422px; inset-inline-start: 417px; width: 863px; height: 176px; background: var(--magenta); }
  .s30 h1 { top: %dpx; inset-inline-start: 0; width: 401px; text-align: center; font-size: 94.4px; line-height: 1.15; font-weight: 400; color: #fff; }
  html[lang="ar"] .s30 h1 { font-weight: 500; font-size: 88px; }
  .s30 p { top: %dpx; inset-inline-start: 466px; width: 720px; font-size: 23.9px; line-height: 28.8px; color: #fff; }""" % (top(545, 94.4), top(474, 23.9, 28.8)))
SLIDES.append(img('bg-recap', 'bg', 'حاسوب محمول ودفتر على مكتب') + brand(30, True) + '<span class="b1"></span><span class="b2"></span><h1>§S30T§</h1><p>§S30P§</p>')
S(30, {'T': 'recap', 'P': 'Use this slide to quickly recap your solution– don’t go over the problem again. Highlighting the positive outcomes is good as you just talked about risks, so reinforcing why your solution is the best one is a good tactic at this point.'},
   {'T': 'الخلاصة', 'P': 'استخدم هذه الشريحة لتلخيص حلك بسرعة، ولا تعد إلى المشكلة مرة أخرى. من الجيد إبراز النتائج الإيجابية بعد حديثك عن المخاطر، فتأكيد أن حلك هو الأفضل أسلوب ناجح في هذه المرحلة.'})

# ===== 31: الشكر =====
STYLE.append("""  .s31 .b1 { top: 264px; inset-inline-start: 0; width: 810px; height: 176px; background: var(--purple); opacity: .8; }
  .s31 .b2 { top: 264px; inset-inline-start: 823px; width: 457px; height: 176px; background: #48035C; }
  .s31 h1 { top: %dpx; inset-inline-start: 103px; font-size: 106px; line-height: 1.15; font-weight: 400; color: #fff; white-space: nowrap; }
  html[lang="ar"] .s31 h1 { font-weight: 700; font-size: 96px; }
  .s31 .cl { top: 318px; inset-inline-start: 903px; display: flex; align-items: center; gap: 4px; color: #fff; }
  .s31 .cl svg { width: 62px; height: 62px; display: block; }
  .s31 .cl span { font-size: 44px; font-weight: 700; letter-spacing: -1.5px; }
  html[lang="ar"] .s31 .cl span { letter-spacing: 0; font-size: 40px; }
  .s31 p { inset-inline-start: 103px; width: 990px; font-size: 16px; line-height: 19.5px; color: #fff; }""" % top(392, 106))
SLIDES.append(img('bg-thanks', 'bg', 'خلفية مكتب داكنة') + '<span class="b1"></span><span class="b2"></span><h1>§S31T§</h1><div class="cl">%s<span>§LOGO§</span></div>' % IC['logo']
              + '<p style="top:%dpx">§S31P1§</p><p style="top:%dpx">§S31P2§</p>' % (top(477, 16, 19.5), top(534, 16, 19.5)))
S(31, {'T': 'Thank You', 'P1': 'End your presentation with a thank you slide. Leave it up while you answer questions. You want this slide to be plain so the attention is on you for question period and not the presentation.',
       'P2': 'The frameworks in this template belong to their respective owners, none were created by this document owner. Frameworks are derived from various pieces of business literature.'},
   {'T': 'شكرًا لكم', 'P1': 'اختم عرضك بشريحة شكر، واتركها معروضة أثناء إجابتك عن الأسئلة. اجعل هذه الشريحة بسيطة حتى يتجه الانتباه إليك في فترة الأسئلة لا إلى العرض.',
    'P2': 'الأطر الواردة في هذا القالب ملك لأصحابها، ولم يُنشئ مالك هذا المستند أيًّا منها. وهي مستمدة من مصادر متنوعة في أدبيات الأعمال.'})

STYLE = '\n'.join(STYLE)

TEMPLATE = {
    'key': KEY, 'kind': 'slides', 'category': 'mgmt', 'icon': '💼',
    'name': 'دراسة حالة أعمال', 'nameEn': 'Business Case Study',
    'description': '31 شريحة لعرض دراسة حالة: المشكلة والحل، تحليل القطاع والشركة، البدائل، التوصية، المخاطر، خطة التنفيذ',
    'descriptionEn': '31 case-study slides: problem & solution, industry and company analysis, options, recommendation, risks, rollout plan',
    'sections': [
        {'title': 'الغلاف', 'titleEn': 'Cover'},
        {'title': 'جدول الأعمال', 'titleEn': 'Agenda'},
        {'title': 'المشكلة والحل', 'titleEn': 'Problem & Solution'},
        {'title': 'الملخص التنفيذي', 'titleEn': 'Executive Summary'},
        {'title': 'فاصل: نبذة تاريخية', 'titleEn': 'Divider: Brief History'},
        {'title': 'نقاط العمل الرئيسة', 'titleEn': 'Business Key Points'},
        {'title': 'التحديات الرئيسة', 'titleEn': 'Key Challenges'},
        {'title': 'فاصل: تحليل القطاع', 'titleEn': 'Divider: Industry Analysis'},
        {'title': 'محاور قسم تحليل القطاع', 'titleEn': 'Industry Analysis Section'},
        {'title': 'تحليل البيئة الكلية (قوى بورتر الخمس)', 'titleEn': 'Macro Environment (Five Forces)'},
        {'title': 'الأداء المالي', 'titleEn': 'Financial Performance', 'needsData': True},
        {'title': 'فاصل: تحليل الشركة', 'titleEn': 'Divider: Company Analysis'},
        {'title': 'محاور قسم تحليل الشركة', 'titleEn': 'Company Analysis Section'},
        {'title': 'عامل النجاح الرئيس (جدول)', 'titleEn': 'Key Success Factor (table)'},
        {'title': 'تحليل نقاط القوة والضعف والفرص والتهديدات', 'titleEn': 'SWOT Analysis'},
        {'title': 'فاصل: البدائل', 'titleEn': 'Divider: Alternatives'},
        {'title': 'محاور قسم البدائل', 'titleEn': 'Alternatives Section'},
        {'title': 'القضية الرئيسة ومعايير القرار', 'titleEn': 'Key Issue / Decision Criteria'},
        {'title': 'الخيار الأول: المزايا والعيوب', 'titleEn': 'Option 1: Pros & Cons'},
        {'title': 'الخيار الثاني: المزايا والعيوب', 'titleEn': 'Option 2: Pros & Cons'},
        {'title': 'الخيار الثالث: المزايا والعيوب', 'titleEn': 'Option 3: Pros & Cons'},
        {'title': 'مصفوفة القضايا والحلول', 'titleEn': 'Issues & Solutions Matrix'},
        {'title': 'التوصية', 'titleEn': 'Recommendation'},
        {'title': 'فاصل: التنفيذ', 'titleEn': 'Divider: Implementation'},
        {'title': 'محاور قسم التنفيذ', 'titleEn': 'Implementation Section'},
        {'title': 'الحد من المخاطر', 'titleEn': 'Risk Mitigation'},
        {'title': 'الجدول الزمني: المدى القصير والمتوسط', 'titleEn': 'Timeline: Short & Mid Term'},
        {'title': 'الجدول الزمني: المدى الطويل', 'titleEn': 'Timeline: Long Term'},
        {'title': 'خطة التواصل مع أصحاب المصلحة', 'titleEn': 'Stakeholder Communication Plan'},
        {'title': 'الخلاصة', 'titleEn': 'Recap'},
        {'title': 'الشكر', 'titleEn': 'Thank You'},
    ],
    'brief': 'صفحة جاهزة بتصميم ثابت من 31 شريحة: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والصور وغيّر النصوص فقط؛ فواصل الأقسام صورة + شريط شبه شفاف (.sec)، ومصفوفة الحلول جدول تحدد علاماته الخلايا ذات الدائرة.',
}
