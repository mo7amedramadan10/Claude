/* مساعد «جيم» في صفحة العرض المنشور (presentation.html — خارج الإطار المعزول فيعمل الميكروفون والـ API بكوكي المشاهد).
   زر عائم بشخصية جيم (الشعار نفسه بوجه في أربع حالات: ساكن / يستمع / يفكّر / يتكلم) يفتح لوحة محادثة:
   صوتي في الأساس (Web Speech API للتعرّف + SpeechSynthesis للقراءة؛ يحتاجان HTTPS ومتصفحًا يدعمهما) ويمكن
   استخدامه كتابةً دائمًا. المحادثة تُحفظ في sessionStorage لهذه الصفحة فقط ولا تُخزَّن في الخادم.
   الاستخدام: JEEM_ASSISTANT.mount({ apiBase, title }). يحترم prefers-reduced-motion (بلا أي حركة). */
window.JEEM_ASSISTANT = (function () {
  'use strict';
  const NAME = 'جيم';
  const CSS = `
.ja{--ja-blue:#127DBD;--ja-sky:#28B6E6;--ja-ink:#1f2430;--ja-mute:#6b7280;--ja-line:#e3e6ea;position:fixed;bottom:18px;inset-inline-end:18px;z-index:2147483000;font-family:"Tanseek",system-ui,-apple-system,"Segoe UI",sans-serif;direction:rtl}
.ja *{box-sizing:border-box}
.ja-fab{position:relative;display:inline-flex;align-items:center;gap:8px;background:#fff;border:1.5px solid var(--ja-blue);border-radius:999px;padding:5px 14px 5px 7px;box-shadow:0 8px 22px rgba(20,40,60,.18);font:inherit;font-weight:700;color:var(--ja-blue);font-size:15px;cursor:pointer;transition:transform .15s,box-shadow .15s}
.ja-fab:hover{transform:translateY(-1px);box-shadow:0 10px 26px rgba(20,40,60,.22)}
.ja-fab:focus-visible,.ja button:focus-visible,.ja textarea:focus-visible{outline:2px solid var(--ja-sky);outline-offset:2px}
.ja-fab .ja-char{width:42px;height:42px}
.ja-ring{position:absolute;inset:-4px;border-radius:999px;border:2px solid rgba(40,182,230,0);pointer-events:none}
.ja[data-state=listening] .ja-ring{border-color:rgba(40,182,230,.7);animation:ja-pulse 1.6s ease-out infinite}
.ja-char{display:block;overflow:visible}
.ja-char .ja-eye-idle,.ja-char .ja-eye-wide,.ja-char .ja-eye-happy,.ja-char .ja-m-smile,.ja-char .ja-m-open,.ja-char .ja-m-o{display:none;transform-box:fill-box;transform-origin:center}
.ja[data-state=idle] .ja-eye-idle,.ja[data-state=idle] .ja-m-smile{display:block}
.ja[data-state=listening] .ja-eye-wide,.ja[data-state=listening] .ja-m-o{display:block}
.ja[data-state=thinking] .ja-eye-idle,.ja[data-state=thinking] .ja-m-o{display:block}
.ja[data-state=speaking] .ja-eye-happy,.ja[data-state=speaking] .ja-m-open{display:block}
.ja[data-state=idle] .ja-eye-idle{animation:ja-blink 4.6s ease-in-out infinite}
.ja[data-state=speaking] .ja-m-open{animation:ja-talk .32s ease-in-out infinite alternate}
.ja-char .ja-bar{transform-box:fill-box;transform-origin:bottom center}
.ja[data-state=speaking] .ja-bar{animation:ja-wave .7s ease-in-out infinite alternate}
.ja[data-state=speaking] .ja-bar.b2{animation-delay:.15s}.ja[data-state=speaking] .ja-bar.b3{animation-delay:.3s}
.ja[data-state=thinking] .ja-bar{animation:ja-think 1.2s ease-in-out infinite}
.ja[data-state=thinking] .ja-bar.b2{animation-delay:.2s}.ja[data-state=thinking] .ja-bar.b3{animation-delay:.4s}
@keyframes ja-pulse{0%{transform:scale(.96);opacity:1}100%{transform:scale(1.28);opacity:0}}
@keyframes ja-blink{0%,92%,100%{transform:scaleY(1)}95%{transform:scaleY(.08)}}
@keyframes ja-talk{from{transform:scaleY(.45)}to{transform:scaleY(1)}}
@keyframes ja-wave{from{transform:scaleY(.6)}to{transform:scaleY(1)}}
@keyframes ja-think{0%,100%{opacity:1}50%{opacity:.35}}
.ja-panel{position:absolute;bottom:62px;inset-inline-end:0;width:380px;max-width:calc(100vw - 36px);height:560px;max-height:calc(100vh - 100px);background:#fff;border:1px solid var(--ja-line);border-radius:18px;box-shadow:0 18px 50px rgba(20,40,60,.22);display:flex;flex-direction:column;overflow:hidden;color:var(--ja-ink)}
.ja-panel.hidden{display:none}
.ja-head{display:flex;align-items:center;gap:10px;padding:10px 12px;border-bottom:1px solid var(--ja-line);background:#F3F5F7}
.ja-head .ja-char{width:36px;height:36px;flex-shrink:0}
.ja-head .ja-who{flex:1;min-width:0;line-height:1.3}
.ja-head .ja-who b{display:block;font-size:15px;color:var(--ja-blue)}
.ja-head .ja-status{font-size:12px;color:var(--ja-mute);white-space:nowrap;overflow:hidden;text-overflow:ellipsis;display:block}
.ja-ib{background:transparent;border:1px solid transparent;border-radius:10px;width:34px;height:34px;font-size:16px;cursor:pointer;color:var(--ja-ink);display:inline-flex;align-items:center;justify-content:center;flex-shrink:0}
.ja-ib:hover{background:#e9edf1}
.ja-ib[aria-pressed=true]{border-color:var(--ja-sky);background:#e8f6fc}
.ja-msgs{flex:1;overflow:auto;padding:12px;display:flex;flex-direction:column;gap:10px;scroll-behavior:smooth}
.ja-msg{max-width:88%;padding:9px 12px;border-radius:14px;font-size:14.5px;line-height:1.7;white-space:pre-wrap;word-wrap:break-word}
.ja-msg.user{align-self:flex-start;background:var(--ja-blue);color:#fff;border-bottom-right-radius:4px}
.ja-msg.bot{align-self:flex-end;background:#F3F5F7;border-bottom-left-radius:4px}
.ja-msg.err{align-self:center;background:#fdecec;color:#9b1c1c;font-size:13px}
.ja-msg ul{margin:4px 0 0;padding-inline-start:18px}
.ja-msg.typing{color:var(--ja-mute)}
.ja-msg.typing::after{content:"…";display:inline-block;min-width:1.2em;animation:ja-dots 1.2s steps(4,end) infinite}
@keyframes ja-dots{0%{content:""}25%{content:"."}50%{content:".."}75%{content:"..."}}
.ja-chips{display:flex;flex-wrap:wrap;gap:6px;padding:0 12px 8px}
.ja-chip{background:#fff;border:1px solid var(--ja-line);border-radius:999px;padding:5px 11px;font:inherit;font-size:13px;color:var(--ja-blue);cursor:pointer}
.ja-chip:hover{border-color:var(--ja-blue);background:#f2f9fd}
.ja-form{display:flex;align-items:flex-end;gap:6px;padding:8px 10px;border-top:1px solid var(--ja-line);background:#fff}
.ja-form textarea{flex:1;resize:none;border:1px solid var(--ja-line);border-radius:14px;padding:9px 12px;font:inherit;font-size:14.5px;line-height:1.5;max-height:120px;min-height:40px;background:#fff;color:var(--ja-ink)}
.ja-form textarea:focus{border-color:var(--ja-sky)}
.ja-send{width:40px;height:40px;border-radius:12px;border:0;background:var(--ja-blue);color:#fff;font-size:18px;cursor:pointer;flex-shrink:0;display:inline-flex;align-items:center;justify-content:center}
.ja-send:disabled,.ja-mic:disabled{opacity:.5;cursor:not-allowed}
.ja-mic{width:40px;height:40px;border-radius:12px;border:1.5px solid var(--ja-blue);background:#fff;color:var(--ja-blue);font-size:18px;cursor:pointer;flex-shrink:0;display:inline-flex;align-items:center;justify-content:center;position:relative}
.ja-mic[aria-pressed=true]{background:var(--ja-blue);color:#fff}
.ja-mic.hidden{display:none}
.ja-foot{padding:4px 12px 8px;font-size:11.5px;color:var(--ja-mute);text-align:center}
.ja-notice{margin:12px;padding:12px;border-radius:12px;background:#fff8e6;border:1px solid #f1d58a;font-size:14px;line-height:1.7}
.ja-notice a{color:var(--ja-blue)}
@media (max-width:520px){.ja{bottom:12px;inset-inline-end:12px}.ja-panel{position:fixed;inset:auto 0 0 0;width:100%;max-width:100%;height:78vh;max-height:78vh;border-radius:18px 18px 0 0}.ja-fab .ja-fab-name{display:none}.ja-fab{padding:5px 7px}}
@media print{.ja{display:none!important}}
@media (prefers-reduced-motion:reduce){.ja *,.ja *::before,.ja *::after{animation:none!important;transition:none!important}}
`;
  // الشعار بمساراته الأصلية + وجه: العينان على الضلع العلوي والفم على الضلع السفلي السميك (انظر المرجع البصري في CLAUDE.md)
  const CHAR = `<svg class="ja-char" viewBox="56 -720 774 1102" aria-hidden="true" focusable="false">
<path class="ja-body" d="M283 342Q240 326 205.5 298.5Q171 271 146.5 235.5Q122 200 109.0 159.5Q96 119 96 76Q96 15 117.0 -35.0Q138 -85 177.0 -121.0Q216 -157 270.5 -177.0Q325 -197 393 -197H530Q556 -197 575.0 -213.0Q594 -229 594 -264V-418Q594 -449 575.0 -466.0Q556 -483 530 -483H236V-680H528Q586 -680 634.0 -660.0Q682 -640 716.5 -605.0Q751 -570 769.5 -522.0Q788 -474 788 -418V-260Q788 -203 769.5 -155.5Q751 -108 716.5 -73.5Q682 -39 632.5 -19.5Q583 0 522 0H379Q363 0 347.5 5.0Q332 10 320.0 20.0Q308 30 300.5 45.0Q293 60 293 80Q293 101 301.0 116.5Q309 132 322.0 142.0Q335 152 351.5 157.0Q368 162 385 162H403Z" fill="#127DBD"/>
<rect class="ja-bar b1" x="400" y="232" width="110" height="110" rx="16" fill="#127DBD"/>
<rect class="ja-bar b2" x="540" y="137" width="110" height="205" rx="16" fill="#127DBD"/>
<rect class="ja-bar b3" x="680" y="42" width="110" height="300" rx="16" fill="#28B6E6"/>
<g class="ja-eye-idle"><circle cx="330" cy="-582" r="46" fill="#fff"/><circle cx="450" cy="-582" r="46" fill="#fff"/><circle cx="342" cy="-574" r="22" fill="#40403D"/><circle cx="462" cy="-574" r="22" fill="#40403D"/><circle cx="350" cy="-584" r="7" fill="#fff"/><circle cx="470" cy="-584" r="7" fill="#fff"/></g>
<g class="ja-eye-wide"><circle cx="330" cy="-582" r="54" fill="#fff"/><circle cx="450" cy="-582" r="54" fill="#fff"/><circle cx="336" cy="-578" r="26" fill="#40403D"/><circle cx="456" cy="-578" r="26" fill="#40403D"/><circle cx="346" cy="-590" r="8" fill="#fff"/><circle cx="466" cy="-590" r="8" fill="#fff"/></g>
<g class="ja-eye-happy"><path d="M290 -570q40-44 80 0" stroke="#fff" stroke-width="22" fill="none" stroke-linecap="round"/><path d="M410 -570q40-44 80 0" stroke="#fff" stroke-width="22" fill="none" stroke-linecap="round"/></g>
<g class="ja-m-smile"><path d="M370 60q60 70 130 10" stroke="#9DD5CA" stroke-width="30" fill="none" stroke-linecap="round"/></g>
<g class="ja-m-open"><ellipse cx="440" cy="80" rx="56" ry="42" fill="#0E6399"/><ellipse cx="440" cy="98" rx="30" ry="15" fill="#9DD5CA"/></g>
<g class="ja-m-o"><circle cx="440" cy="80" r="34" fill="#0E6399"/></g>
</svg>`;

  const SR = window.SpeechRecognition || window.webkitSpeechRecognition || null;
  const TTS = ('speechSynthesis' in window && 'SpeechSynthesisUtterance' in window) ? window.speechSynthesis : null;
  const reduced = () => window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  const esc = s => String(s == null ? '' : s).replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
  const hasArabic = s => /[؀-ۿ]/.test(s || '');

  /* تنسيق خفيف لرد الموديل: غامق، وقوائم بشرطة، وأسطر. (النص يُهرَّب أولًا كله.) */
  function render(text) {
    const lines = String(text || '').split(/\r?\n/);
    let html = '', list = [];
    const flush = () => { if (list.length) { html += '<ul>' + list.map(l => '<li>' + l + '</li>').join('') + '</ul>'; list = []; } };
    for (const raw of lines) {
      const l = esc(raw).replace(/\*\*(.+?)\*\*/g, '<b>$1</b>');
      const m = /^\s*(?:[-•*]|\d+[.)])\s+(.*)$/.exec(l);
      if (m) list.push(m[1]); else { flush(); html += (html && !html.endsWith('</ul>') ? '\n' : '') + l.replace(/^#+\s*/, ''); }
    }
    flush();
    return html;
  }
  /* نص للقراءة الصوتية: بلا علامات تنسيق */
  function plain(text) { return String(text || '').replace(/\*\*|__|`|#+\s/g, '').replace(/^\s*(?:[-•*]|\d+[.)])\s+/gm, '').replace(/\s+/g, ' ').trim(); }
  function chunks(text) {
    const out = []; let buf = '';
    for (const part of plain(text).split(/(?<=[.!?؟。\n])\s+|(?<=،)\s+(?=.{60,})/)) {
      if ((buf + ' ' + part).length > 180 && buf) { out.push(buf.trim()); buf = part; } else buf += ' ' + part;
    }
    if (buf.trim()) out.push(buf.trim());
    return out;
  }

  function mount(opts) {
    const apiBase = opts.apiBase;
    const storeKey = 'ja:' + apiBase;
    const root = document.createElement('div');
    root.className = 'ja'; root.dataset.state = 'idle';
    const style = document.createElement('style'); style.id = 'ja-style'; style.textContent = CSS;
    document.head.appendChild(style);
    root.innerHTML = `
<button type="button" class="ja-fab" aria-expanded="false" aria-controls="ja-panel" title="اسأل ${NAME} عن هذا العرض"><span class="ja-ring"></span>${CHAR}<span class="ja-fab-name">${NAME}</span></button>
<section class="ja-panel hidden" id="ja-panel" role="dialog" aria-label="مساعد ${NAME}">
  <header class="ja-head">${CHAR}<div class="ja-who"><b>${NAME}</b><span class="ja-status" id="ja-status">مساعد العرض — اسألني عن أي شيء في الملف</span></div>
    <button type="button" class="ja-ib" id="ja-speaker" aria-pressed="true" title="قراءة الردود بصوت عالٍ (تشغيل/إيقاف)">🔊</button>
    <button type="button" class="ja-ib" id="ja-clear" title="محادثة جديدة">🗑️</button>
    <button type="button" class="ja-ib" id="ja-close" aria-label="إغلاق">✕</button></header>
  <div class="ja-msgs" id="ja-msgs" aria-live="polite"></div>
  <div class="ja-chips" id="ja-chips"></div>
  <form class="ja-form" id="ja-form">
    <button type="button" class="ja-mic${SR ? '' : ' hidden'}" id="ja-mic" aria-pressed="false" title="تحدّث إلى ${NAME} (الميكروفون)">🎙️</button>
    <textarea id="ja-input" rows="1" placeholder="اسأل عن أي شيء في هذا العرض…" aria-label="سؤالك"></textarea>
    <button type="submit" class="ja-send" id="ja-send" aria-label="إرسال">➤</button>
  </form>
  <div class="ja-foot">الإجابات من محتوى هذا العرض فقط وقد تحتمل الخطأ.</div>
</section>`;
    document.body.appendChild(root);
    const $ = id => root.querySelector('#' + id);
    const fab = root.querySelector('.ja-fab'), panel = $('ja-panel'), msgs = $('ja-msgs'), input = $('ja-input'), mic = $('ja-mic'), send = $('ja-send'), status = $('ja-status'), speaker = $('ja-speaker'), chips = $('ja-chips');

    const S = { state: 'idle', history: [], busy: false, voiceLoop: false, rec: null, speakOn: !!TTS, allowed: true, reason: null, info: null };
    try { const saved = JSON.parse(sessionStorage.getItem(storeKey) || 'null'); if (saved && Array.isArray(saved.history)) S.history = saved.history.slice(-30); if (saved && typeof saved.speakOn === 'boolean' && TTS) S.speakOn = saved.speakOn; } catch { }
    if (!TTS) { S.speakOn = false; speaker.disabled = true; speaker.title = 'القراءة الصوتية غير مدعومة في هذا المتصفح'; }
    speaker.setAttribute('aria-pressed', S.speakOn ? 'true' : 'false'); speaker.textContent = S.speakOn ? '🔊' : '🔇';
    const persist = () => { try { sessionStorage.setItem(storeKey, JSON.stringify({ history: S.history.slice(-30), speakOn: S.speakOn })); } catch { } };
    const setState = s => { S.state = s; root.dataset.state = s; mic.setAttribute('aria-pressed', s === 'listening' ? 'true' : 'false'); };
    const setStatus = t => { status.textContent = t; };

    function addMsg(role, text, extraCls) {
      const el = document.createElement('div');
      el.className = 'ja-msg ' + role + (extraCls ? ' ' + extraCls : '');
      if (role === 'bot') el.innerHTML = render(text); else el.textContent = text;
      msgs.appendChild(el); msgs.scrollTop = msgs.scrollHeight;
      return el;
    }
    function renderChips() {
      const list = S.history.length ? [] : ['لخّص لي هذا العرض', 'ما أهم الأرقام فيه؟', 'ما الخلاصة أو التوصيات؟'];
      chips.innerHTML = list.map(c => `<button type="button" class="ja-chip">${esc(c)}</button>`).join('');
      chips.querySelectorAll('.ja-chip').forEach(b => b.addEventListener('click', () => ask(b.textContent)));
    }
    function renderAll() {
      msgs.innerHTML = '';
      if (!S.allowed) { const n = document.createElement('div'); n.className = 'ja-notice'; n.innerHTML = esc(S.reason || 'المساعد غير متاح.') + (S.info && !S.info.signedIn ? ' <a href="/" target="_blank" rel="noopener">تسجيل الدخول ↗</a>' : ''); msgs.appendChild(n); input.disabled = true; send.disabled = true; mic.disabled = true; chips.innerHTML = ''; return; }
      if (!S.history.length) addMsg('bot', `أهلًا! أنا ${NAME}. أعرف محتوى «${opts.title || 'هذا العرض'}» كاملًا، فاسألني عن أي جزء فيه${SR ? ' — بالكتابة أو بالضغط على الميكروفون' : ''}.`);
      for (const h of S.history) addMsg(h.role === 'user' ? 'user' : 'bot', h.text);
      renderChips();
    }

    /* ---------- الصوت ---------- */
    let voices = [];
    const loadVoices = () => { if (TTS) voices = TTS.getVoices() || []; };
    if (TTS) { loadVoices(); TTS.onvoiceschanged = loadVoices; }
    function pickVoice(lang) {
      const pref = lang.slice(0, 2).toLowerCase();
      return voices.find(v => v.lang && v.lang.toLowerCase() === lang.toLowerCase()) || voices.find(v => v.lang && v.lang.toLowerCase().startsWith(pref)) || null;
    }
    function stopSpeaking() { if (TTS) { try { TTS.cancel(); } catch { } } if (S.state === 'speaking') setState('idle'); }
    function speak(text, done) {
      if (!TTS || !S.speakOn) { done && done(); return; }
      const parts = chunks(text); if (!parts.length) { done && done(); return; }
      const lang = hasArabic(text) ? 'ar-SA' : (navigator.language || 'en-US');
      const voice = pickVoice(lang);
      try { TTS.cancel(); } catch { }
      setState('speaking'); setStatus('يتكلم…');
      let i = 0;
      const next = () => {
        if (i >= parts.length || S.state !== 'speaking') { if (S.state === 'speaking') setState('idle'); setStatus('مساعد العرض — اسألني عن أي شيء في الملف'); done && done(); return; }
        const u = new SpeechSynthesisUtterance(parts[i++]);
        u.lang = lang; if (voice) u.voice = voice; u.rate = 1;
        u.onend = next; u.onerror = () => { setState('idle'); setStatus('تعذّرت القراءة الصوتية'); done && done(); };
        TTS.speak(u);
      };
      next();
    }
    function listen() {
      if (!SR || S.busy) return;
      stopSpeaking();
      const rec = new SR(); S.rec = rec;
      rec.lang = (document.documentElement.lang || 'ar').startsWith('ar') ? 'ar-SA' : (navigator.language || 'en-US');
      rec.interimResults = true; rec.continuous = false; rec.maxAlternatives = 1;
      let finalText = '';
      rec.onstart = () => { setState('listening'); setStatus('أستمع… تكلّم الآن'); };
      rec.onresult = ev => {
        let interim = '';
        for (let k = ev.resultIndex; k < ev.results.length; k++) { const r = ev.results[k]; if (r.isFinal) finalText += r[0].transcript; else interim += r[0].transcript; }
        input.value = (finalText || interim).trim();
      };
      rec.onerror = ev => {
        S.rec = null; setState('idle'); S.voiceLoop = false;
        setStatus(ev.error === 'not-allowed' || ev.error === 'service-not-allowed' ? 'الميكروفون غير مسموح — اكتب سؤالك' : ev.error === 'no-speech' ? 'لم أسمع شيئًا — جرّب مرة أخرى' : 'تعذّر التعرّف على الصوت — اكتب سؤالك');
      };
      rec.onend = () => {
        S.rec = null;
        if (S.state === 'listening') setState('idle');
        const q = (finalText || input.value).trim();
        if (q && S.state !== 'thinking') { input.value = ''; ask(q, true); } else if (!q) { S.voiceLoop = false; if (status.textContent.startsWith('أستمع')) setStatus('لم أسمع شيئًا — اضغط الميكروفون وتكلّم'); }
      };
      try { rec.start(); } catch { S.rec = null; setState('idle'); setStatus('تعذّر تشغيل الميكروفون'); }
    }
    function stopListening() { if (S.rec) { try { S.rec.abort(); } catch { } S.rec = null; } if (S.state === 'listening') setState('idle'); }

    /* ---------- السؤال ---------- */
    async function ask(question, fromVoice) {
      question = (question || '').trim();
      if (!question || S.busy || !S.allowed) return;
      stopSpeaking();
      S.busy = true; send.disabled = true; mic.disabled = true;
      addMsg('user', question); chips.innerHTML = '';
      const typing = addMsg('bot', `${NAME} يفكّر`, 'typing');
      setState('thinking'); setStatus('يقرأ الملف ويجهّز الرد…');
      try {
        const res = await fetch(`${apiBase}/assistant/ask`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ question, history: S.history.slice(-12) }) });
        const body = await res.json().catch(() => ({}));
        typing.remove();
        if (!res.ok) {
          addMsg('err', body.error || `تعذّر الحصول على رد (${res.status})`);
          setState('idle'); setStatus('مساعد العرض — اسألني عن أي شيء في الملف'); S.voiceLoop = false;
          return;
        }
        const reply = String(body.reply || '');
        S.history.push({ role: 'user', text: question }, { role: 'assistant', text: reply }); persist();
        addMsg('bot', reply);
        setState('idle'); setStatus('مساعد العرض — اسألني عن أي شيء في الملف');
        if (fromVoice || S.speakOn) speak(reply, () => { if (S.voiceLoop && fromVoice && panel.classList.contains('hidden') === false) listen(); });
      } catch (e) {
        typing.remove(); addMsg('err', 'تعذّر الاتصال بالخادم — تحقق من الشبكة ثم أعد المحاولة.'); setState('idle'); S.voiceLoop = false;
      } finally { S.busy = false; send.disabled = false; mic.disabled = false; }
    }

    /* ---------- الأحداث ---------- */
    function open() { panel.classList.remove('hidden'); fab.setAttribute('aria-expanded', 'true'); renderAll(); if (S.allowed) setTimeout(() => input.focus(), 50); }
    function close() { panel.classList.add('hidden'); fab.setAttribute('aria-expanded', 'false'); S.voiceLoop = false; stopListening(); stopSpeaking(); }
    fab.addEventListener('click', () => panel.classList.contains('hidden') ? open() : close());
    $('ja-close').addEventListener('click', close);
    $('ja-clear').addEventListener('click', () => { S.history = []; persist(); stopSpeaking(); renderAll(); });
    speaker.addEventListener('click', () => { S.speakOn = !S.speakOn; speaker.setAttribute('aria-pressed', S.speakOn ? 'true' : 'false'); speaker.textContent = S.speakOn ? '🔊' : '🔇'; if (!S.speakOn) stopSpeaking(); persist(); });
    mic.addEventListener('click', () => { if (S.state === 'listening') { S.voiceLoop = false; stopListening(); setStatus('مساعد العرض — اسألني عن أي شيء في الملف'); } else { S.voiceLoop = true; listen(); } });
    $('ja-form').addEventListener('submit', ev => { ev.preventDefault(); if (S.busy) return; const q = input.value; input.value = ''; input.style.height = 'auto'; S.voiceLoop = false; ask(q, false); });
    input.addEventListener('keydown', ev => { if (ev.key === 'Enter' && !ev.shiftKey) { ev.preventDefault(); $('ja-form').requestSubmit(); } });
    input.addEventListener('input', () => { input.style.height = 'auto'; input.style.height = Math.min(120, input.scrollHeight) + 'px'; });
    document.addEventListener('keydown', ev => { if (ev.key === 'Escape' && !panel.classList.contains('hidden')) close(); });
    window.addEventListener('pagehide', () => { stopSpeaking(); stopListening(); });

    /* حالة المساعد لهذا المشاهد: مفعَّل؟ مسموح؟ */
    fetch(`${apiBase}/assistant`, { cache: 'no-store' }).then(r => r.ok ? r.json() : null).then(info => {
      if (!info) return;
      S.info = info;
      if (!info.enabled) { root.remove(); style.remove(); return; }
      S.allowed = !!info.allowed; S.reason = info.reason || null;
      if (!panel.classList.contains('hidden')) renderAll();
    }).catch(() => { });

    return { ask, open, close, setState, get state() { return S.state; }, get history() { return S.history; }, voice: { supported: !!SR, tts: !!TTS } };
  }

  return { mount, render, plain, chunks };
})();
