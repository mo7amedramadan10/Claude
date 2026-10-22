/* جيم — سلوك الواجهة المشترك (Vanilla JS، بدون أي مكتبات) */
(function () {
  const $ = (s, r = document) => r.querySelector(s);
  const $$ = (s, r = document) => [...r.querySelectorAll(s)];
  const app = $('.app');

  /* ---- وضع التضمين (?embed=1): اللوحة داخل شاشة «اللوحات المنشورة» بدون القائمة والشريط العلوي ---- */
  if (new URLSearchParams(location.search).has('embed')) {
    document.documentElement.classList.add('is-embed');
    if (window.parent !== window) {
      const post = () => window.parent.postMessage({ jeemEmbedH: Math.ceil(document.documentElement.scrollHeight) }, '*');
      new ResizeObserver(post).observe(document.body); addEventListener('load', post);
    }
  }
  const store = {
    get(k) { try { return localStorage.getItem(k); } catch (e) { return null; } },
    set(k, v) { try { localStorage.setItem(k, v); } catch (e) { /* تجاهل */ } }
  };

  /* ---- طي القائمة الجانبية (يُحفظ لكل مستخدم في المتصفح) ---- */
  if (store.get('jeem.sidebar') === 'collapsed') app.classList.add('is-collapsed');
  $$('[data-action="collapse"]').forEach(b => b.addEventListener('click', () => {
    app.classList.toggle('is-collapsed');
    const c = app.classList.contains('is-collapsed');
    b.setAttribute('aria-expanded', String(!c));
    store.set('jeem.sidebar', c ? 'collapsed' : 'open');
    window.dispatchEvent(new Event('resize')); // لإعادة رسم الرسوم البيانية
  }));

  /* ---- القائمة على الجوال ---- */
  const scrim = $('.scrim');
  const closeOverlays = () => { app.classList.remove('nav-open'); scrim?.classList.remove('is-on'); };
  $$('[data-action="nav"]').forEach(b => b.addEventListener('click', () => { app.classList.add('nav-open'); scrim?.classList.add('is-on'); }));
  scrim?.addEventListener('click', () => { closeOverlays(); if (dock && window.innerWidth < 1080) { dock.dataset.state = 'hidden'; document.documentElement.dataset.chatState = 'hidden'; } });
  document.addEventListener('keydown', e => { if (e.key === 'Escape') { closeOverlays(); $$('.w-src.is-open').forEach(p => p.classList.remove('is-open')); } });

  /* ---- المحادثة: عمود جانبي على الشمال (open / hidden) — يُحفظ لكل مستخدم ---- */
  const dock = $('.dock');
  if (dock) {
    const setChat = st => {
      dock.dataset.state = st;
      document.documentElement.dataset.chatState = st;
      store.set('jeem.chat', st);
      if (st !== 'hidden') { const th = $('.thread', dock); if (th) th.scrollTop = th.scrollHeight; }
    };
    const saved = store.get('jeem.chat');
    setChat(['open', 'hidden'].includes(saved) ? saved : (window.innerWidth < 1080 ? 'hidden' : 'open'));
    $$('button[data-chat]').forEach(b => b.addEventListener('click', () => {
      const st = b.dataset.chat;
      setChat(st);
      window.dispatchEvent(new Event('resize')); // إعادة رسم الرسوم بعد تغيّر العرض
      if (st === 'open') $('#q')?.focus();
    }));
    /* «/» يفتح المحادثة ويركّز على الكتابة */
    document.addEventListener('keydown', e => {
      if (e.key === '/' && !/INPUT|TEXTAREA|SELECT/.test(document.activeElement.tagName)) {
        e.preventDefault(); if (dock.dataset.state === 'hidden') setChat('open'); $('#q')?.focus();
      }
    });
  }

  /* ---- شجرة المشاريع ---- */
  $$('.proj-item > .nav-link').forEach(link => link.addEventListener('click', e => {
    if (app.classList.contains('is-collapsed')) return;
    const item = link.parentElement;
    if (item.querySelector('.proj-sub')) {
      e.preventDefault();
      item.classList.toggle('is-open');
      link.setAttribute('aria-expanded', String(item.classList.contains('is-open')));
    }
  }));

  /* ---- أزرار التبديل (المقاطع، مصادر المحادثة، الفلاتر) ---- */
  $$('.segmented').forEach(g => g.addEventListener('click', e => {
    const b = e.target.closest('button'); if (!b) return;
    $$('button', g).forEach(x => x.setAttribute('aria-pressed', String(x === b)));
  }));
  $$('.src-toggle, .chip[aria-pressed]').forEach(b => b.addEventListener('click', () => {
    b.setAttribute('aria-pressed', String(b.getAttribute('aria-pressed') !== 'true'));
    b.classList.toggle('is-on', b.getAttribute('aria-pressed') === 'true');
  }));

  /* ---- نافذة «مصدر البيانات» لكل عنصر في اللوحة ---- */
  $$('[data-action="source"]').forEach(b => b.addEventListener('click', e => {
    e.stopPropagation();
    const pop = b.closest('.widget').querySelector('.w-src');
    const open = !pop.classList.contains('is-open');
    $$('.w-src.is-open').forEach(p => p.classList.remove('is-open'));
    pop.classList.toggle('is-open', open);
    b.setAttribute('aria-expanded', String(open));
  }));
  document.addEventListener('click', () => $$('.w-src.is-open').forEach(p => p.classList.remove('is-open')));

  /* ---- السرد الصوتي (تجريبي: Web Speech API) ---- */
  const nar = $('.narrate');
  if (nar) nar.addEventListener('click', () => {
    const playing = nar.classList.toggle('is-playing');
    nar.setAttribute('aria-pressed', String(playing));
    $('use', nar.querySelector('.play')).setAttribute('href', playing ? '#i-pause' : '#i-play');
    nar.querySelector('.lbl').textContent = playing ? 'جارٍ السرد…' : 'استمع للملخص';
    try {
      if (playing && 'speechSynthesis' in window) {
        const u = new SpeechSynthesisUtterance(nar.dataset.text || '');
        u.lang = 'ar-SA'; u.onend = () => nar.click();
        speechSynthesis.speak(u);
      } else if ('speechSynthesis' in window) { speechSynthesis.cancel(); }
    } catch (e) { /* المتصفح لا يدعم */ }
  });

  /* ---- المحادثة (تجريبي: يحاكي استجابة الخادم) ---- */
  const form = $('#ask');
  if (form) {
    const chat = $('.chat'), ta = $('textarea', form);
    const thread = () => $('.thread:not([hidden])');
    const grow = () => { ta.style.height = 'auto'; ta.style.height = Math.min(ta.scrollHeight, 160) + 'px'; };

    /* نوع المحادثة: build = بناء/تعديل اللوحة، ask = استفسارات (لا تغيّر اللوحة). كل نوع له محادثته الخاصة. */
    const tabs = $$('.chat-modes [role="tab"]');
    const setMode = mode => {
      const tab = tabs.find(t => t.dataset.mode === mode);
      if (!tab || tab.disabled) mode = 'ask';
      chat.dataset.mode = mode;
      tabs.forEach(t => {
        const on = t.dataset.mode === mode;
        t.setAttribute('aria-selected', String(on)); t.tabIndex = on ? 0 : -1;
        const panel = document.getElementById(t.getAttribute('aria-controls')); if (panel) panel.hidden = !on;
      });
      if (mode !== 'templates') ta.placeholder = ta.dataset['ph' + (mode === 'build' ? 'Build' : 'Ask')] || ta.placeholder;
      const hint = $('.mode-hint'); if (hint && tab) hint.textContent = (tabs.find(t => t.dataset.mode === mode) || tab).dataset.hint || '';
      store.set('jeem.chatMode', mode);
      const th = thread(); if (th) th.scrollTop = mode === 'templates' ? 0 : th.scrollHeight;
    };
    tabs.forEach(t => t.addEventListener('click', () => { if (!t.disabled) { setMode(t.dataset.mode); if (t.dataset.mode !== 'templates') ta.focus(); } }));
    $('.chat-modes')?.addEventListener('keydown', e => {
      if (!['ArrowLeft', 'ArrowRight'].includes(e.key)) return;
      const usable = tabs.filter(t => !t.disabled), i = usable.findIndex(t => t.getAttribute('aria-selected') === 'true');
      const next = usable[(i + (e.key === 'ArrowLeft' ? 1 : usable.length - 1)) % usable.length];
      if (next) { setMode(next.dataset.mode); next.focus(); }
    });
    setMode(store.get('jeem.chatMode') || 'build');

    ta.addEventListener('input', grow);
    ta.addEventListener('keydown', e => { if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); form.requestSubmit(); } });
    $$('.followups .chip').forEach(c => c.addEventListener('click', () => { ta.value = c.textContent.trim(); grow(); ta.focus(); }));
    form.addEventListener('submit', e => {
      e.preventDefault();
      const q = ta.value.trim(); if (!q) return;
      if (dock && dock.dataset.state !== 'open') { dock.dataset.state = 'open'; document.documentElement.dataset.chatState = 'open'; store.set('jeem.chat', 'open'); }
      const mode = chat.dataset.mode, th = thread();
      const esc = s => s.replace(/[&<>"]/g, ch => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[ch]));
      const wait = mode === 'build' ? 'جيم يحلّل طلبك ويحدّث اللوحة…' : 'جيم يبحث في البيانات…';
      th.insertAdjacentHTML('beforeend', `<div class="msg msg-user"><div class="bubble">${esc(q)}</div><span class="msg-time">الآن</span></div>`);
      th.insertAdjacentHTML('beforeend', `<div class="msg msg-ai" id="pending"><div class="ai-row"><span class="mini"><img src="assets/img/logo-mark.svg" alt=""></span><div class="ai-body"><div class="thinking"><span class="jbars is-live"><i></i><i></i><i></i></span>${wait}</div></div></div></div>`);
      ta.value = ''; grow(); th.scrollTop = th.scrollHeight;
      setTimeout(() => {
        const p = $('#pending'); if (!p) return;
        p.removeAttribute('id');
        $('.ai-body', p).innerHTML = mode === 'build'
          ? '<p>تم تحديث اللوحة الحالية بناءً على طلبك. <b>(استجابة تجريبية — تُستبدل باستدعاء الـ API)</b></p>'
          : '<p>هنا تظهر الإجابة المباشرة (رقم، قائمة، أو جملة) بدون أي تغيير على اللوحة. <b>(استجابة تجريبية)</b></p>';
        th.scrollTop = th.scrollHeight;
      }, 1800);
    });
  }

  /* ---- نوافذ عامة: <button data-open="id"> يفتح <dialog id="id"> ---- */
  $$('[data-open]').forEach(b => b.addEventListener('click', () => { const d = document.getElementById(b.dataset.open); if (d) d.showModal(); }));
  $$('dialog.modal').forEach(d => {
    $$('[data-action="close-modal"]', d).forEach(b => b.addEventListener('click', () => d.close()));
    d.addEventListener('click', e => { if (e.target === d) d.close(); });
  });

  /* ---- التبويبات ---- */
  $$('[role="tablist"]').forEach(list => list.addEventListener('click', e => {
    const t = e.target.closest('[role="tab"]'); if (!t) return;
    $$('[role="tab"]', list).forEach(x => { const on = x === t; x.setAttribute('aria-selected', String(on)); const p = document.getElementById(x.getAttribute('aria-controls')); if (p) p.hidden = !on; });
  }));

  /* ---- منطقة رفع الملفات ---- */
  const dz = $('.dropzone');
  if (dz) {
    ['dragenter', 'dragover'].forEach(ev => dz.addEventListener(ev, e => { e.preventDefault(); dz.classList.add('is-over'); }));
    ['dragleave', 'drop'].forEach(ev => dz.addEventListener(ev, e => { e.preventDefault(); dz.classList.remove('is-over'); }));
  }

  /* ---- تحديد الكل في الجداول ---- */
  $$('[data-check-all]').forEach(c => c.addEventListener('change', () => $$('tbody .chk', c.closest('table')).forEach(x => x.checked = c.checked)));

  /* ---- نافذة إنشاء مشروع ---- */
  const dlg = $('#new-project');
  if (dlg) {
    $$('[data-action="new-project"]').forEach(b => b.addEventListener('click', () => { dlg.showModal(); $('input', dlg).focus(); }));
    if (location.hash === '#new') { dlg.showModal(); }
    $$('[data-action="close-modal"]', dlg).forEach(b => b.addEventListener('click', () => dlg.close()));
    dlg.addEventListener('click', e => { if (e.target === dlg) dlg.close(); });
    $('form', dlg).addEventListener('submit', e => {
      e.preventDefault();
      const name = $('#p-name', dlg).value.trim() || 'مشروع جديد';
      const color = ($('input[name="color"]:checked', dlg) || {}).value || 'var(--brand-blue)';
      const card = document.createElement('a');
      card.href = 'project-workspace.html'; card.className = 'proj-card';
      card.innerHTML = `<div class="top"><span class="proj-ic" style="background:${color}"><svg class="icon"><use href="#i-layers"/></svg></span><div><h3></h3><p class="desc">مشروع جديد — جاهز لربط المصادر.</p></div><span class="badge badge-blue role">مالك</span></div><div class="proj-metrics"><div><b>0</b><span>لوحات</span></div><div><b>0</b><span>مصادر</span></div><div><b>1</b><span>أعضاء</span></div></div><div class="proj-foot"><span class="last"><svg class="icon icon-sm"><use href="#i-clock"/></svg>أُنشئ الآن</span></div>`;
      $('h3', card).textContent = name;
      $('.proj-grid').insertBefore(card, $('.proj-grid .new-card'));
      dlg.close(); e.target.reset();
    });
  }
})();
