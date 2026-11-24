// فحص قبول نموذج شرائح: node check.js <key> [srcFolder]
// يفتح ar.html وen.html من الخادم المحلي، يرصد أخطاء JS والموارد الخارجية، يلتقط كل شريحة 1280×720،
// يركّب (الأصل | الإنجليزي | العربي) لكل شريحة، ويفحص بنود القبول الستة و390px.
const { chromium } = require('playwright');
const fs = require('fs'), path = require('path'), { execFileSync } = require('child_process');
const key = process.argv[2];
const SRC_ROOT = '/home/user/slides-source';
const srcFolder = process.argv[3] || (fs.readFileSync('/tmp/claude-0/-home-user/7c327ac0-b846-52e1-a71d-20505f07e134/scratchpad/catalog.txt','utf8').split('\n').map(l => l.split('|')).find(r => r[0] === key) || [])[1];
const ROOT = '/home/user/claude/backend/ChatToDashboard.Api/wwwroot';
const DIR = path.join(ROOT, 'templates/freeform', key);
const SHOTS = path.join('/tmp/claude-0/-home-user/7c327ac0-b846-52e1-a71d-20505f07e134/scratchpad/shots', key);
const PORT = process.env.PORT || 8765;
fs.mkdirSync(SHOTS, { recursive: true });
const problems = [];
function bad(m) { problems.push(m); console.log('✗', m); }
function ok(m) { console.log('✓', m); }

(async () => {
  // 1) template.json
  const tj = JSON.parse(fs.readFileSync(path.join(DIR, 'template.json'), 'utf8'));
  for (const f of ['key','kind','category','icon','name','nameEn','description','descriptionEn','sections']) if (!(f in tj)) bad('template.json بلا حقل ' + f);
  if (tj.key !== key || !/^sl-[a-z0-9-]+$/.test(tj.key)) bad('المفتاح غير صالح: ' + tj.key);
  if (tj.kind !== 'slides') bad('kind ليس slides');
  if (!['mgmt','sales','projects','finance','hr','marketing','ops','cx','general'].includes(tj.category)) bad('تصنيف غير مسموح: ' + tj.category);
  if (tj.name.length > 40 || tj.nameEn.length > 40) bad('الاسم أطول من 40 حرفًا');
  if (tj.description.length > 120 || tj.descriptionEn.length > 120) bad('الوصف أطول من 120 حرفًا');
  tj.sections.forEach((s, i) => { if (!s.title || !s.titleEn) bad('قسم ' + (i+1) + ' بلا title/titleEn'); });
  // تفرّد المفتاح: الكتالوج الثابت + مجلدات النماذج الأخرى
  const cs = fs.readFileSync(path.join(ROOT, '../Freeform/FreeformTemplates.cs'), 'utf8');
  if (cs.includes('"' + key + '"')) bad('المفتاح مكرر في الكتالوج الثابت');
  ok('template.json: ' + tj.sections.length + ' أقسام، تصنيف ' + tj.category);

  const browser = await chromium.launch();
  const originals = srcFolder ? fs.readdirSync(path.join(SRC_ROOT, srcFolder)).filter(f => /\.jpe?g|\.png$/i.test(f)).sort() : [];
  if (originals.length && originals.length !== tj.sections.length) bad('عدد صور الأصل ' + originals.length + ' ≠ عدد الأقسام ' + tj.sections.length);
  const shotsByLang = {};
  for (const lang of ['ar', 'en']) {
    const html = fs.readFileSync(path.join(DIR, lang + '.html'), 'utf8');
    const nSec = (html.match(/<section class="slide/g) || []).length;
    if (nSec !== tj.sections.length) bad(lang + ': عدد الشرائح ' + nSec + ' ≠ الأقسام ' + tj.sections.length);
    if (!/window\.goToSlide\s*=/.test(html)) bad(lang + ': goToSlide غير معرّفة');
    if (!html.includes('<script src="/js/jeem-slides.js"></script>')) bad(lang + ': jeem-slides.js غير مضمّن');
    if (!html.includes('--jeem-strip-h')) bad(lang + ': --jeem-strip-h غير محجوز');
    if (/data:/i.test(html.replace(/<meta[^>]*>/g,''))) bad(lang + ': يحوي data URL');
    const expDir = lang === 'ar' ? 'rtl' : 'ltr';
    if (!new RegExp('<html lang="' + lang + '" dir="' + expDir + '">').test(html)) bad(lang + ': وسم html/dir غير صحيح');
    // كل img يشير إلى assets بالمسار المطلق وملف موجود
    for (const m of html.matchAll(/<img\b[^>]*>/g)) { const sm = m[0].match(/\ssrc="([^"]+)"/); if (!sm) { bad(lang + ": صورة بلا src"); continue; }
      const src = sm[1], pre = '/templates/freeform/' + key + '/assets/';
      if (!src.startsWith(pre)) bad(lang + ': مسار صورة غير مطلوب: ' + src);
      else if (!fs.existsSync(path.join(DIR, 'assets', src.slice(pre.length)))) bad(lang + ': صورة مفقودة ' + src);
      if (!/alt="/.test(m[0])) bad(lang + ': صورة بلا alt: ' + src);
    }
    // نص اللغة الأخرى
    const text = html.replace(/<script[\s\S]*?<\/script>/g, '').replace(/<style[\s\S]*?<\/style>/g, '').replace(/<[^>]+>/g, ' ').replace(/&[a-z]+;/g, ' ');
    const textNoAttr = text;
    if (lang === 'en' && /[؀-ۿ]/.test(textNoAttr)) bad('en: يحوي نصًا عربيًا: ' + textNoAttr.match(/[؀-ۿ][^<]{0,30}/)[0]);
    if (lang === 'ar') { const lat = textNoAttr.match(/\b[A-Za-z]{3,}\b/g); if (lat) bad('ar: يحوي كلمات لاتينية: ' + [...new Set(lat)].slice(0, 8).join(', ')); }
    // الموارد الخارجية + أخطاء JS
    const ctx = await browser.newContext({ viewport: { width: 1280, height: 720 }, deviceScaleFactor: 1 });
    const page = await ctx.newPage();
    const errors = [], ext = [];
    page.on('pageerror', e => errors.push(String(e)));
    page.on('console', m => { if (m.type() === 'error') errors.push(m.text()); });
    page.on('request', r => { const u = r.url(); if (!u.startsWith('http://127.0.0.1:') && !/^https:\/\/fonts\.(googleapis|gstatic)\.com\//.test(u)) ext.push(u); });
    page.on('response', r => { if (r.status() >= 400 && r.url().startsWith('http://127.0.0.1:')) errors.push('HTTP ' + r.status() + ' ' + r.url()); });
    await page.goto('http://127.0.0.1:' + PORT + '/templates/freeform/' + key + '/' + lang + '.html', { waitUntil: 'load' });
    await page.waitForTimeout(1200);
    const info = await page.evaluate(() => ({ go: typeof window.goToSlide, strip: !!document.getElementById('jeem-strip'), stripH: getComputedStyle(document.documentElement).getPropertyValue('--jeem-strip-h').trim(), n: document.querySelectorAll('#deck > section.slide').length, dir: document.documentElement.dir, fonts: document.fonts.status }));
    if (info.go !== 'function') bad(lang + ': goToSlide ليست دالة في وقت التشغيل');
    if (!info.strip && info.n >= 2) bad(lang + ': شريط المصغّرات لم يُبنَ');
    if (errors.length) bad(lang + ': أخطاء JS/HTTP: ' + errors.join(' | '));
    if (ext.length) bad(lang + ': موارد خارجية: ' + [...new Set(ext)].join(', '));
    ok(lang + ': ' + info.n + ' شرائح، dir=' + info.dir + '، الشريط ' + info.stripH + '، بلا أخطاء=' + !errors.length);
    // التنقّل بلوحة المفاتيح والأزرار
    await page.keyboard.press('End'); const last = await page.evaluate(() => document.getElementById('counter').textContent);
    if (last !== info.n + ' / ' + info.n) bad(lang + ': End لم ينتقل للأخيرة (' + last + ')');
    await page.keyboard.press('Home');
    await page.click('#next'); const second = await page.evaluate(() => document.getElementById('counter').textContent);
    if (info.n > 1 && second !== '2 / ' + info.n) bad(lang + ': زر التالي لم ينتقل (' + second + ')');
    // لقطات 1280×720 لكل شريحة (بإخفاء الشريط والتنقّل حتى يكون المقياس 1)
    await page.addStyleTag({ content: '#jeem-strip,.nav,.progress{display:none!important} html{--jeem-strip-h:0px!important} .frame{box-shadow:none!important}' });
    await page.evaluate(() => document.documentElement.style.setProperty('--jeem-strip-h', '0px'));
    await page.waitForTimeout(300);
    const shots = [];
    for (let i = 0; i < info.n; i++) {
      await page.evaluate(k => window.goToSlide(k), i);
      await page.waitForTimeout(150);
      const sc = await page.evaluate(() => { const f = document.querySelector('#deck .slide.active .frame'); const r = f.getBoundingClientRect(); return [r.width, r.height]; });
      if (Math.abs(sc[0] - 1280) > 1) bad(lang + ': مقياس الإطار ليس 1 (' + sc + ')');
      const fp = path.join(SHOTS, lang + '-' + String(i + 1).padStart(2, '0') + '.png');
      await page.locator('#deck .slide.active .frame').screenshot({ path: fp });
      shots.push(fp);
    }
    shotsByLang[lang] = shots;
    // 390px بلا تمرير أفقي
    await ctx.close();
    const mctx = await browser.newContext({ viewport: { width: 390, height: 844 }, deviceScaleFactor: 1, hasTouch: true, isMobile: true });
    const mp = await mctx.newPage(); const merr = [];
    mp.on('pageerror', e => merr.push(String(e)));
    await mp.goto('http://127.0.0.1:' + PORT + '/templates/freeform/' + key + '/' + lang + '.html', { waitUntil: 'load' });
    await mp.waitForTimeout(800);
    const sw = await mp.evaluate(() => [document.scrollingElement.scrollWidth, document.documentElement.clientWidth, document.body.scrollWidth]);
    if (sw[0] > 390 || sw[2] > 390) bad(lang + ': تمرير أفقي على 390px (' + sw + ')'); else ok(lang + ': بلا تمرير أفقي على 390px');
    if (merr.length) bad(lang + ': أخطاء JS على الجوال: ' + merr.join(' | '));
    await mp.screenshot({ path: path.join(SHOTS, lang + '-mobile.png') });
    await mctx.close();
  }
  await browser.close();
  // تركيب المقارنة: الأصل | الإنجليزي | العربي
  for (let i = 0; i < tj.sections.length; i++) {
    const orig = originals[i] ? path.join(SRC_ROOT, srcFolder, originals[i]) : null;
    const out = path.join(SHOTS, 'cmp-' + String(i + 1).padStart(2, '0') + '.png');
    const args = ['-background', '#1F2430', '-gravity', 'north', '-font', 'DejaVu-Sans', '-pointsize', '22', '-fill', '#fff'];
    const tiles = [[orig, 'ORIGINAL'], [shotsByLang.en[i], 'EN'], [shotsByLang.ar[i], 'AR']].filter(t => t[0]);
    const cmd = ['-background', '#1F2430'];
    for (const [f, label] of tiles) cmd.push('(', f, '-resize', '640x360', '-gravity', 'North', '-splice', '0x28', '-font', 'DejaVu-Sans', '-pointsize', '18', '-fill', '#fff', '-annotate', '+0+4', label, ')');
    cmd.push('+append', out);
    execFileSync('convert', cmd);
  }
  console.log(problems.length ? ('\n✗ ' + problems.length + ' مشكلة') : '\n✓ كل البنود مرّت');
  console.log('shots:', SHOTS);
  process.exit(problems.length ? 1 : 0);
})().catch(e => { console.error(e); process.exit(2); });
