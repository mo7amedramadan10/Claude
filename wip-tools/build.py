# -*- coding: utf-8 -*-
"""مولّد صفحتي ar.html وen.html وملف template.json لنموذج شرائح واحد.
الاستخدام: python3 -I build.py <key>
يقرأ tpl/<key>.py الذي يعرّف: KEY, TITLE{ar,en}, STYLE, SLIDES (قائمة HTML بشرائح تحوي رموز §اسم§)،
STR{'ar':{...},'en':{...}}، TEMPLATE (قاموس template.json)، واختياريًا EXTRA_SCRIPT وJSON_BLOCKS{'id': {...}}.
الصفحة تستخدم خصائص CSS منطقية (inset-inline-start…) فتنعكس تلقائيًا بين RTL وLTR."""
import importlib.util, json, os, re, sys

OUT_ROOT = '/home/user/claude/backend/ChatToDashboard.Api/wwwroot/templates/freeform'
FONTS = {
    'ar': "<link href=\"https://fonts.googleapis.com/css2?family=Tajawal:wght@300;400;500;700;800&display=swap\" rel=\"stylesheet\">",
    'en': "<link href=\"https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&display=swap\" rel=\"stylesheet\">",
}
FONT_STACK = {'ar': "'Tajawal', 'Segoe UI', Tahoma, sans-serif", 'en': "'Inter', 'Segoe UI', Tahoma, Arial, sans-serif"}
NAV = {'ar': ('السابق', 'التالي'), 'en': ('Previous', 'Next')}

BASE = """<!doctype html>
<html lang="§LANG§" dir="§DIR§">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>§PAGE_TITLE§</title>
§FONT_LINK§
<style>
  /* §COMMENT§ */
  :root { --font: §FONT_STACK§; §VARS§ }
  * { box-sizing: border-box; }
  html, body { margin: 0; height: 100%; background: #1F2430; font-family: var(--font); color: var(--ink); }
  body { padding-bottom: var(--jeem-strip-h, 0px); overflow: hidden; }
  .deck { position: relative; height: calc(100vh - var(--jeem-strip-h, 0px)); }
  .slide { position: absolute; inset: 0; display: none; align-items: center; justify-content: center; }
  .slide.active { display: flex; }
  /* الإطار بحجم ثابت 1280×720 يُصغَّر ليلائم الشاشة؛ flex:none ضروري وإلا يُقص في المعاينة الضيقة */
  .frame { position: relative; flex: none; width: 1280px; height: 720px; background: var(--bg); overflow: hidden; transform-origin: center center; box-shadow: 0 20px 60px rgb(0 0 0 / .35); }
  .frame svg { display: block; }
§STYLE§
  /* التنقّل */
  .nav { position: fixed; bottom: calc(14px + var(--jeem-strip-h, 0px)); inset-inline-start: 50%; transform: translateX(§NAV_TX§); display: flex; gap: 8px; align-items: center; z-index: 5; background: rgb(0 0 0 / .45); color: #fff; padding: 6px 10px; border-radius: 999px; font-size: 14px; }
  .nav button { background: #fff; color: var(--ink); border: 0; width: 32px; height: 32px; border-radius: 50%; cursor: pointer; font-size: 16px; }
  .nav button:focus-visible { outline: 3px solid #4FB3FF; outline-offset: 2px; }
  .progress { position: fixed; top: 0; inset-inline-start: 0; height: 4px; background: var(--accent); width: 0; z-index: 6; transition: width .25s; }
  @media (prefers-reduced-motion: reduce) { .progress { transition: none; } *, *::before, *::after { animation: none !important; transition: none !important; } }
  @media print {
    html, body { background: #fff; height: auto; overflow: visible; }
    .deck { height: auto; } .slide { position: static; display: flex !important; page-break-after: always; height: 100vh; }
    .frame { transform: none !important; box-shadow: none; } .nav, .progress { display: none; }
    @page { size: 1280px 720px; margin: 0; }
  }
</style>
</head>
<body>
<div class="progress" id="progress"></div>
§GLOBAL_DEFS§<div class="deck" id="deck">
§SLIDES§
</div>
<div class="nav"><button type="button" id="prev" aria-label="§PREV§">§PREV_GLYPH§</button><span id="counter">1 / §N§</span><button type="button" id="next" aria-label="§NEXT§">§NEXT_GLYPH§</button></div>
§JSON_BLOCKS§<script>
(function () {
  var RTL = document.documentElement.dir === 'rtl';
§EXTRA_SCRIPT§
  // ----- التنقّل -----
  var slides = Array.prototype.slice.call(document.querySelectorAll('.slide')), cur = 0;
  function fit() { var deck = document.getElementById('deck'), s = Math.min(deck.clientWidth / 1280, deck.clientHeight / 720); slides.forEach(function (sl) { sl.querySelector('.frame').style.transform = 'scale(' + s + ')'; }); }
  function go(i) {
    i = Math.max(0, Math.min(slides.length - 1, i)); cur = i;
    slides.forEach(function (s, j) { s.classList.toggle('active', j === i); });
    document.getElementById('counter').textContent = (i + 1) + ' / ' + slides.length;
    document.getElementById('progress').style.width = ((i + 1) / slides.length * 100) + '%';
    document.dispatchEvent(new CustomEvent('jeem:slide', { detail: { index: i } }));
  }
  window.goToSlide = go;
  document.getElementById('next').addEventListener('click', function () { go(cur + 1); });
  document.getElementById('prev').addEventListener('click', function () { go(cur - 1); });
  document.addEventListener('keydown', function (e) {
    if (e.target && (e.target.isContentEditable || /INPUT|TEXTAREA/.test(e.target.tagName))) return;
    var fwd = RTL ? 'ArrowLeft' : 'ArrowRight', back = RTL ? 'ArrowRight' : 'ArrowLeft';
    if (e.key === fwd || e.key === ' ' || e.key === 'PageDown') { e.preventDefault(); go(cur + 1); }
    if (e.key === back || e.key === 'PageUp') { e.preventDefault(); go(cur - 1); }
    if (e.key === 'Home') go(0); if (e.key === 'End') go(slides.length - 1);
  });
  var tx = null;
  document.addEventListener('touchstart', function (e) { tx = e.touches[0].clientX; }, { passive: true });
  document.addEventListener('touchend', function (e) { if (tx == null) return; var dx = e.changedTouches[0].clientX - tx; tx = null; if (Math.abs(dx) > 50) go((RTL ? dx > 0 : dx < 0) ? cur + 1 : cur - 1); });
  window.addEventListener('resize', fit); fit(); go(0);
  var obs = new MutationObserver(fit); obs.observe(document.documentElement, { attributes: true, attributeFilter: ['style'] });
})();
</script>
<script src="/js/jeem-slides.js"></script>
</body>
</html>
"""


def render(mod, lang):
    s = dict(mod.STR[lang])
    s['LANG'] = lang; s['DIR'] = 'rtl' if lang == 'ar' else 'ltr'
    s['PAGE_TITLE'] = mod.TITLE[lang]
    s['FONT_LINK'] = getattr(mod, 'FONTS', {}).get(lang, FONTS[lang]); s['FONT_STACK'] = getattr(mod, 'FONT_STACK', {}).get(lang, FONT_STACK[lang])
    s['VARS'] = mod.VARS
    s['COMMENT'] = mod.COMMENT
    s['STYLE'] = mod.STYLE
    slides = mod.SLIDES
    s['SLIDES'] = '\n'.join('  <!-- %d -->\n  <section class="slide%s"><div class="frame s%d">%s</div></section>' % (i + 1, ' active' if i == 0 else '', i + 1, h) for i, h in enumerate(slides))
    s['N'] = str(len(slides))
    s['PREV'], s['NEXT'] = NAV[lang]
    s['PREV_GLYPH'], s['NEXT_GLYPH'] = ('›', '‹') if lang == 'ar' else ('‹', '›')
    s['NAV_TX'] = '50%' if lang == 'ar' else '-50%'
    s['EXTRA_SCRIPT'] = getattr(mod, 'EXTRA_SCRIPT', '')
    gd = getattr(mod, 'GLOBAL_DEFS', '')
    s['GLOBAL_DEFS'] = ('<!-- تعريفات SVG مشتركة (تدرّجات) خارج الشرائح حتى تعمل في كل شريحة -->\n<svg width="0" height="0" style="position:absolute" aria-hidden="true">' + gd + '</svg>\n') if gd else ''
    blocks = getattr(mod, 'JSON_BLOCKS', {})
    s['JSON_BLOCKS'] = ''.join('<script type="application/json" id="%s">%s</script>\n' % (k, json.dumps(v[lang] if isinstance(v, dict) and lang in v and 'ar' in v else v, ensure_ascii=False)) for k, v in blocks.items())
    html = BASE
    # استبدال متكرر حتى تُحل الرموز داخل الشرائح أيضًا
    for _ in range(4):
        html = re.sub(r'§([A-Za-z0-9_]+)§', lambda m: s.get(m.group(1), m.group(0)), html)
    left = re.findall(r'§[A-Za-z0-9_]+§', html)
    if left:
        raise SystemExit('رموز غير محلولة في %s: %s' % (lang, sorted(set(left))))
    return html


def main():
    key = sys.argv[1]
    here = os.path.dirname(os.path.abspath(__file__))
    spec = importlib.util.spec_from_file_location('tpl_' + key.replace('-', '_'), os.path.join(here, 'tpl', key + '.py'))
    mod = importlib.util.module_from_spec(spec); spec.loader.exec_module(mod)
    assert mod.KEY == key
    out = os.path.join(OUT_ROOT, key); os.makedirs(os.path.join(out, 'assets'), exist_ok=True)
    for lang in ('ar', 'en'):
        with open(os.path.join(out, lang + '.html'), 'w', encoding='utf-8') as f:
            f.write(render(mod, lang))
    tj = dict(mod.TEMPLATE)
    tj.setdefault('key', key); tj.setdefault('kind', 'slides')
    assert len(tj['sections']) == len(mod.SLIDES), 'عدد الأقسام لا يساوي عدد الشرائح'
    with open(os.path.join(out, 'template.json'), 'w', encoding='utf-8') as f:
        json.dump(tj, f, ensure_ascii=False, indent=2); f.write('\n')
    print('OK', out, len(mod.SLIDES), 'slides')


if __name__ == '__main__':
    main()
