/* ==========================================================
   جيم — نظرة على فريق المستودع (لوحة تفاعلية، المحرك: warehouse)
   مُعاد بناؤه كدالة تركيب قابلة لإعادة الاستخدام (window.JEEM_WAREHOUSE_MOUNT) بدل IIFE
   ذاتي التشغيل — تقبل بيانات حقيقية (من GET /api/interactive/{templateId}/data) أو بيانات
   تجريبية ثابتة (معاينة المعرض، عبر warehouse-team-demo.js)، بدل التوليد العشوائي الداخلي.
   في وضع العرض التجريبي (opts.demo===true) فقط: محاكاة إكمال المهام العشوائية (tick) ومحاكاة
   أفعال إعادة الإسناد/التعديل محليًا — في الوضع الحقيقي تُطبَّق التعديلات على نفس العرض لمدة
   الجلسة (لا توجد طبقة حفظ دائم بعد)، بلا أي توليد عشوائي لبيانات غير حقيقية.
   ========================================================== */
(function () {
  function mount(root, data, opts = {}) {
    if (!root || !data) return;
    const demo = !!opts.demo;
    const $ = (s, el = root) => el.querySelector(s), $$ = (s, el = root) => [...el.querySelectorAll(s)];
    const ic = (n, c = 'icon icon-sm') => `<svg class="${c}" aria-hidden="true"><use href="#i-${n}"/></svg>`;
    const esc = s => String(s).replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
    const RM = matchMedia('(prefers-reduced-motion: reduce)').matches;
    const nf = v => Number(v).toLocaleString('en-US');
    const pad = n => String(n).padStart(2, '0');
    const count = (el, to, fmt = v => nf(Math.round(v)), dur = 900) => {
      if (!el) return; if (RM) { el.textContent = fmt(to); return; }
      const from = +el.dataset.v || 0, t0 = performance.now(); el.dataset.v = to;
      const st = t => { const p = Math.min(1, (t - t0) / dur), e = 1 - Math.pow(1 - p, 3); el.textContent = fmt(from + (to - from) * e); if (p < 1) requestAnimationFrame(st); };
      requestAnimationFrame(st);
    };
    const toast = msg => {
      let t = document.querySelector('.toast');
      if (!t) { t = document.createElement('div'); t.className = 'toast'; t.setAttribute('role', 'status'); document.body.append(t); }
      t.innerHTML = `${ic('check')}<span>${esc(msg)}</span>`;
      requestAnimationFrame(() => t.classList.add('is-on'));
      clearTimeout(t._h); t._h = setTimeout(() => t.classList.remove('is-on'), 2600);
    };
    const norm = s => String(s).replace(/[أإآ]/g, 'ا').replace(/ة/g, 'ه').replace(/ى/g, 'ي');

    /* ---------- البيانات (من data، لا توليد هنا) ---------- */
    let oid = 5840 + Math.floor(Math.random() * 100);
    const W = (data.staff || []).map(w => ({ ...w, log: w.log || [] }));
    const OFF = (data.off || []).map(w => ({ ...w }));
    const ALL = W.concat(OFF), byId = Object.fromEntries(ALL.map(w => [w.id, w]));
    // الأدوار والمناطق تُستخرج من البيانات نفسها (نص حقيقي، مش قائمة ثابتة) — يدعم أي
    // تسمية حقيقية للدور/المنطقة في مصدر المشروع، بدل افتراض أربعة أدوار ثابتة فقط.
    const ROLE_LABELS = [...new Set(ALL.map(w => w.role).filter(Boolean))];
    const S = { role: 'all', q: '', more: false, unread: false, live: true, sel: null };
    const FEED = (data.feed || []).map(f => ({ ...f, fresh: false }));
    const ini = n => String(n).split(' ').slice(0, 2).map(x => x.replace(/^ال/, '')[0] || '').join('‌');
    const hue = id => (hashId(id) * 53 + (String(id).startsWith('o') ? 20 : 190)) % 360;
    function hashId(id) { let h = 0; for (const c of String(id)) h = (h * 31 + c.charCodeAt(0)) % 1000; return h; }

    /* ---------- 1) المؤشرات ---------- */
    const HOURS = ['6ص', '8ص', '10ص', '12م', '2م', '4م', '6م'];
    function kpis() {
      const on = W.filter(w => w.on);
      const tasks = W.reduce((a, w) => a + (w.done || 0), 0);
      const eff = Math.round(on.reduce((a, w) => a + (w.eff || 0), 0) / (on.length || 1));
      const hourly = data.hourly || { staff: [], tasks: [], eff: [] };
      const K = [
        ['users', 'الموظفون في المناوبة', on.length, v => nf(Math.round(v)), hourly.staff.length ? hourly.staff : [on.length]],
        ['layers', 'مهام اليوم المنجزة', tasks, v => nf(Math.round(v)), hourly.tasks.length ? hourly.tasks : [tasks]],
        ['target', 'متوسط الكفاءة', eff, v => Math.round(v) + '%', hourly.eff.length ? hourly.eff : [eff]],
      ];
      if (!$('#whKpis').children.length) {
        $('#whKpis').innerHTML = K.map(([icn, n, , , bars], i) => {
          const mx = Math.max(...bars, 1), mn = Math.min(...bars) * .8;
          return `<div class="wh-kpi" style="--i:${i}"><span class="lb">${ic(icn)}${n}</span><div class="row"><b class="num" data-k="${i}">0</b><span class="mb" role="img" aria-label="${n} حسب الساعة">${bars.map((b, j) => `<i style="--h:${((b - mn) / (mx - mn || 1) * 80 + 20).toFixed(0)}%;--j:${j}" title="${HOURS[j] || ''}: ${b}"></i>`).join('')}</span></div></div>`;
        }).join('');
      }
      K.forEach(([, , v, f], i) => count($(`[data-k="${i}"]`), v, f));
    }

    /* ---------- 2) العدّ التنازلي لتغيير المناوبة ---------- */
    const STEP = 5 * 60 * 1000;
    let shiftAt = data.shift && data.shift.nextAt
      ? new Date(data.shift.nextAt).getTime()
      : Math.ceil((Date.now() + 14 * 60 * 1000) / STEP) * STEP;
    function clock() {
      const left = Math.max(0, Math.round((shiftAt - Date.now()) / 1000)), d = new Date(shiftAt);
      $('#whMin').textContent = Math.ceil(left / 60); $('#whCd').classList.toggle('soon', left <= 300);
      $('#whH').textContent = pad(Math.floor(left / 3600)); $('#whM').textContent = pad(Math.floor(left % 3600 / 60)); $('#whS').textContent = pad(left % 60);
      $('#whAt').textContent = `${pad(d.getHours())}:${pad(d.getMinutes())}`;
      $('#whCd').setAttribute('aria-label', `تغيير المناوبة بعد ${Math.ceil(left / 60)} دقيقة`);
      if (!left) {
        shiftAt = Date.now() + 8 * 3600 * 1000;
        if (demo) addFeed(null, 'بدأت مناوبة جديدة · تم تبديل الفرق', 'info');
      }
    }

    /* ---------- 3) بطاقات الفرق ---------- */
    const match = w => (S.role === 'all' || w.role === S.role) && (!S.q || norm((w.name || '') + (w.role || '') + (w.zone || '')).includes(norm(S.q)));
    function card(w, i) {
      const segs = Array.from({ length: w.tot || 0 }, (_, j) => `<i class="${j < w.done ? 'f' : ''}" style="--j:${j}"></i>`).join('');
      const effSt = w.eff >= 90 ? 'ok' : w.eff >= 85 ? 'mid' : 'low';
      return `<article class="wh-card${w.on ? '' : ' off'}${S.sel === w.id ? ' sel' : ''}" data-w="${esc(w.id)}" style="--i:${i};--hu:${hue(w.id)}">
        <header><span class="av"><b>${esc(ini(w.name))}</b><em>${esc(w.role || '')}</em></span><span class="nm"><b>${esc(w.name)}</b><small>${ic('pin')}${esc(w.zone || '')}</small></span>
          <span class="pill ${w.on ? 'on' : 'off'}">${ic(w.on ? 'check' : 'clock')}${w.on ? 'في المناوبة' : 'خارج المناوبة'}</span></header>
        ${w.on ? `<div class="prog"><span class="h"><span>تقدّم المهام</span><span class="eff ${effSt}">الكفاءة: <b class="num">${w.eff}%</b></span></span>
            <span class="segs" role="progressbar" aria-valuemin="0" aria-valuemax="${w.tot}" aria-valuenow="${w.done}" aria-label="تقدّم مهام ${esc(w.name)}">${segs}</span><small><span class="num">${w.done}/${w.tot}</span> مهام</small></div>
          <div class="cur"><span>المهمة الحالية:</span><b>${ic('layers')}${esc(w.task || '—')}</b><code class="num">${esc(w.ord || '')}</code></div>
          <footer><button type="button" class="lnk" data-det="${esc(w.id)}" aria-expanded="${S.sel === w.id}">${S.sel === w.id ? 'إخفاء التفاصيل' : 'عرض التفاصيل'}</button><span class="sp"></span>
            <button type="button" class="btn btn-sm" data-adj="${esc(w.id)}">${ic('edit')}تعديل</button><button type="button" class="btn btn-sm btn-primary" data-re="${esc(w.id)}">${ic('share')}إعادة إسناد</button></footer>
          ${S.sel === w.id ? `<ol class="det">${(w.log.length ? w.log : [['—', 'لا سجل تفصيلي متاح لهذه الفترة']]).slice(-4).map(([t, x]) => `<li><span class="num">${esc(t)}</span>${esc(x)}</li>`).join('')}</ol>` : ''}`
        : `<div class="sum"><span><small>آخر مناوبة</small><b><span class="num">${w.done || 0}</span> مهمة</b></span><span><small>الكفاءة</small><b class="num">${w.eff || 0}%</b></span><span><small>العودة</small><b>${esc(w.ret || '—')}</b></span></div>
          <footer><span class="sp"></span><button type="button" class="btn btn-sm" data-call="${esc(w.id)}">${ic('bell')}استدعاء</button></footer>`}
      </article>`;
    }
    function teams() {
      const on = W.filter(w => w.on && match(w)), off = OFF.concat(W.filter(w => !w.on)).filter(match);
      $('#whOnN').textContent = on.length; $('#whOffN').textContent = OFF.concat(W.filter(w => !w.on)).length;
      $('#whOn').innerHTML = on.slice(0, S.more ? 99 : 4).map(card).join('') || '<p class="wh-none">لا موظفين يطابقون التصفية.</p>';
      $('#whOff').innerHTML = off.slice(0, S.more ? 99 : 4).map(card).join('') || '<p class="wh-none">لا موظفين يطابقون التصفية.</p>';
      $$('[data-more]').forEach(b => { b.textContent = S.more ? 'عرض أقل' : 'عرض الكل'; b.hidden = !S.more && on.length <= 4 && off.length <= 4; });
    }

    /* ---------- 4) سجل النشاط ---------- */
    const now = () => { const d = new Date(); return `${pad(d.getHours())}:${pad(d.getMinutes())}`; };
    function addFeed(w, html, kind = 'ok', unread = true) {
      FEED.unshift({ w: w && w.id, html, kind, t: now(), unread, fresh: true });
      if (FEED.length > 40) FEED.pop();
      feed();
    }
    function feed() {
      const L = FEED.filter(f => !S.unread || f.unread);
      $('#whUnread').textContent = FEED.filter(f => f.unread).length;
      $('#whFeed').innerHTML = L.map(f => `<li class="k-${f.kind}${f.fresh && !RM ? ' fresh' : ''}${f.unread ? ' un' : ''}"${f.w ? ` data-go="${esc(f.w)}" tabindex="0" role="button"` : ''}><span class="dot" aria-hidden="true">${ic({ ok: 'check', move: 'share', start: 'play', info: 'info', warn: 'clock' }[f.kind] || 'info')}</span><p>${f.html}</p><time class="num">${esc(f.t)}</time></li>`).join('') || '<li class="none">لا أحداث غير مقروءة.</li>';
      FEED.forEach(f => f.fresh = false);
    }
    const who = w => `<b class="who" style="--hu:${hue(w.id)}">@${esc(w.name)}</b>`;
    const chip = s => `<code class="num">${esc(s)}</code>`;

    /* ---------- 5) محاكاة حية — فقط في معاينة المعرض التجريبية ---------- */
    function demoTick() {
      if (!S.live || document.hidden) return;
      const on = W.filter(w => w.on && w.done < w.tot); if (!on.length) return;
      const w = on[Math.floor(Math.random() * on.length)];
      if (Math.random() < .75) {
        w.done++; const o = w.ord; w.ord = '#ORD-' + (oid += 3); w.log.push([now(), `أكمل ${w.task} ${o}`]); w.eff = Math.min(99, w.eff + (Math.random() < .5 ? 1 : 0));
        addFeed(w, `${who(w)} أكمل ${w.task === 'نقل منصّة' ? 'نقل المنصّة' : 'الطلب'} ${chip(o)}`, 'ok');
        const el = $(`[data-w="${w.id}"]`);
        if (el) {
          const s = el.querySelectorAll('.segs i')[w.done - 1]; if (s) s.classList.add('f', 'pop');
          const p = el.querySelector('.prog small .num'); if (p) p.textContent = `${w.done}/${w.tot}`;
          const c = el.querySelector('.cur code'); if (c) { c.textContent = w.ord; c.classList.remove('flash'); void c.offsetWidth; c.classList.add('flash'); }
          const e = el.querySelector('.eff .num'); if (e) e.textContent = w.eff + '%';
        }
        kpis();
      } else {
        const dock = Math.random() < .5 ? 'الرصيف 1' : 'الرصيف 2';
        addFeed(w, `${who(w)} بدأ تفريغ ${chip(dock)}`, 'start');
      }
    }

    /* ---------- 6) نافذة إعادة الإسناد / التعديل ---------- */
    const dlg = $('#whDlg');
    const ZONES = [...new Set(ALL.map(w => w.zone).filter(Boolean))];
    function openDlg(kind, w) {
      const others = W.filter(x => x.on && x.id !== w.id).sort((a, b) => (a.tot - a.done) - (b.tot - b.done));
      dlg.innerHTML = `<div class="wh-dlg" role="dialog" aria-modal="true" aria-labelledby="whDlgT"><h3 id="whDlgT">${kind === 're' ? 'إعادة إسناد المهمة' : 'تعديل منطقة العمل'}</h3>
        <p>${kind === 're' ? `نقل ${esc(w.task || '')} ${chip(w.ord || '')} من ${who(w)} إلى:` : `نقل ${who(w)} إلى منطقة أخرى:`}</p>
        <div class="opts" role="radiogroup">${kind === 're'
          ? others.map((x, i) => `<label><input type="radio" name="whOpt" value="${esc(x.id)}" ${i ? '' : 'checked'}><span class="av sm" style="--hu:${hue(x.id)}">${esc(ini(x.name))}</span><span>${esc(x.name)}<small>${esc(x.role || '')} · متبقٍ <span class="num">${x.tot - x.done}</span> مهام${i ? '' : ' · الأقل حملًا'}</small></span></label>`).join('')
          : ZONES.map((z, i) => `<label><input type="radio" name="whOpt" value="${esc(z)}" ${z === w.zone ? 'checked' : ''}><span>${esc(z)}</span></label>`).join('')}</div>
        <div class="act"><button type="button" class="btn" data-dlg-x>إلغاء</button><button type="button" class="btn btn-primary" data-dlg-ok="${kind}|${esc(w.id)}">تأكيد</button></div></div>`;
      dlg.hidden = false; dlg.querySelector('input:checked')?.focus();
    }
    const closeDlg = () => { dlg.hidden = true; dlg.innerHTML = ''; };
    function confirmDlg(kind, id) {
      const w = byId[id], v = dlg.querySelector('input:checked')?.value; if (v == null) return closeDlg();
      if (kind === 're') {
        const to = byId[v];
        addFeed(to, `${who(to)} استلم ${chip(w.ord)} بإعادة إسناد من ${who(w)}`, 'move');
        to.tot = (to.tot || 0) + 1; w.tot = Math.max(w.done, (w.tot || 0) - 1);
        to.log.push([now(), `استلم ${w.ord}`]); w.ord = '#ORD-' + (oid += 3);
        toast(`أُعيد إسناد المهمة إلى ${to.name}`);
        opts.onAction?.('reassign', { from: w.id, to: to.id });
      } else {
        if (v !== w.zone) {
          addFeed(w, `${who(w)} نُقل من ${chip(w.zone)} إلى ${chip(v)}`, 'move'); w.zone = v;
          toast(`نُقل ${w.name} إلى ${v}`);
          opts.onAction?.('adjustZone', { staffId: w.id, zone: v });
        }
      }
      closeDlg(); teams();
    }

    /* ---------- الأحداث ---------- */
    root.addEventListener('click', e => {
      const t = e.target;
      const r = t.closest('[data-role]'); if (r) { S.role = r.dataset.role; $$('[data-role]').forEach(b => b.setAttribute('aria-pressed', String(b === r))); return teams(); }
      if (t.closest('[data-more]')) { S.more = !S.more; return teams(); }
      const d = t.closest('[data-det]'); if (d) { S.sel = S.sel === d.dataset.det ? null : d.dataset.det; return teams(); }
      const a = t.closest('[data-adj]'); if (a) return openDlg('adj', byId[a.dataset.adj]);
      const re = t.closest('[data-re]'); if (re) return openDlg('re', byId[re.dataset.re]);
      const c = t.closest('[data-call]'); if (c) {
        const w = byId[c.dataset.call]; addFeed(w, `أُرسل استدعاء إلى ${who(w)} للمناوبة القادمة`, 'warn');
        opts.onAction?.('recall', { staffId: w.id });
        return toast(`أُرسل الاستدعاء إلى ${w.name}`);
      }
      const ok = t.closest('[data-dlg-ok]'); if (ok) { const [k, id] = ok.dataset.dlgOk.split('|'); return confirmDlg(k, id); }
      if (t.closest('[data-dlg-x]') || t === dlg) return closeDlg();
      const g = t.closest('[data-go]'); if (g) return goTo(g.dataset.go);
      if (t.closest('[data-read]')) { FEED.forEach(f => f.unread = false); return feed(); }
      if (t.closest('[data-live]')) { S.live = !S.live; const b = $('[data-live]'); b.setAttribute('aria-pressed', String(S.live)); b.lastChild.textContent = S.live ? 'مباشر' : 'متوقف'; return; }
      if (t.closest('[data-new]')) return toast('هنا يُفتح نموذج إضافة مهمة: النوع، المنطقة، الطلب، والموظف.');
    });
    root.addEventListener('keydown', e => { const g = e.target.closest('[data-go]'); if (g && (e.key === 'Enter' || e.key === ' ')) { e.preventDefault(); goTo(g.dataset.go); } });
    const escHandler = e => { if (e.key === 'Escape' && !dlg.hidden) closeDlg(); };
    addEventListener('keydown', escHandler);
    $('#whUnreadOnly').addEventListener('change', e => { S.unread = e.target.checked; feed(); });
    $('#whQ').addEventListener('input', e => { S.q = e.target.value.trim(); teams(); });
    // أزرار تصفية الدور تُبنى من الأدوار الحقيقية الموجودة في البيانات، لا قائمة ثابتة.
    const rolesBar = $('#whRoles');
    if (rolesBar) {
      rolesBar.innerHTML = `<button type="button" data-role="all" aria-pressed="true">الكل</button>` +
        ROLE_LABELS.map(r => `<button type="button" data-role="${esc(r)}" aria-pressed="false">${esc(r)}</button>`).join('');
    }
    function goTo(id) {
      const f = FEED.find(x => x.w === id && x.unread); if (f) { f.unread = false; feed(); }
      const w = byId[id]; if (!w) return;
      if (S.role !== 'all' && w.role !== S.role) { S.role = 'all'; $$('[data-role]').forEach(b => b.setAttribute('aria-pressed', String(b.dataset.role === 'all'))); }
      S.q = ''; $('#whQ').value = ''; S.more = true; teams();
      const el = $(`[data-w="${id}"]`); if (!el) return;
      el.scrollIntoView({ block: 'center', behavior: RM ? 'auto' : 'smooth' });
      el.classList.remove('ping'); void el.offsetWidth; el.classList.add('ping');
    }

    /* ---------- البدء ---------- */
    kpis(); teams(); feed(); clock();
    const clockTimer = setInterval(clock, 1000);
    let tickTimer = null;
    if (demo) tickTimer = setInterval(demoTick, 3500);

    let liveSub = null;
    if (!demo && opts.channel && window.JeemLive) {
      liveSub = window.JeemLive.subscribe(opts.channel, msg => {
        if (msg && msg.kind === 'staff_event') addFeed(byId[msg.staffId], msg.html, msg.eventKind || 'info');
      });
    }

    return {
      destroy() {
        clearInterval(clockTimer); if (tickTimer) clearInterval(tickTimer);
        removeEventListener('keydown', escHandler);
        liveSub?.close();
      },
    };
  }

  window.JEEM_WAREHOUSE_MOUNT = mount;
})();
