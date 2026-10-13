/* ==========================================================
   جيم — محاكاة الأدوار (وضع المعاينة فقط)
   في التطبيق الفعلي: الدور يأتي من الـ Claims في الجلسة، والسيرفر هو اللي
   بيمنع العرض/الإجراء. هنا بنحاكي نفس القواعد عشان تتنقل بين الأدوار وتشوف
   الفرق. احذف هذا الملف وزر «عرض كـ» عند التنفيذ.

   السمات المستخدمة في الصفحات:
   data-scope="platform|org|org-admin|project"   ← نطاق الصفحة (للحماية والتحويل)
   data-require="super|org-admin|project:viewer|project:editor|project:owner"  ← يختفي لو الدور أقل
   data-require-enable="..."                      ← يتعطّل بدل ما يختفي
   data-show-roles="viewer,editor"                ← يظهر فقط لأدوار مشروع معيّنة
   data-project="sales|hr|proc|cx"                ← عنصر مرتبط بمشروع (يختفي لو المستخدم مش عضو)
   ========================================================== */
(function () {
  const RANK = { viewer: 1, editor: 2, owner: 3 };
  const ROLE_AR = { owner: 'مالك', editor: 'محرر', viewer: 'مشاهد' };
  const ROLE_BADGE = { owner: 'badge-blue', editor: 'badge-teal', viewer: 'badge-purple' };

  const PERSONAS = {
    super:    { name: 'أحمد الزهراني',  letter: 'أ', color: 'var(--brand-gray)',   title: 'مسؤول المنصّة',  desc: 'شاشة المنظمات فقط — بدون أي اطلاع على بياناتها', level: 'super', projects: {} },
    orgadmin: { name: 'محمد رمضان',     letter: 'م', color: 'var(--brand-blue)',   title: 'مسؤول المنظمة',  desc: 'ينشئ المشاريع ويدير الحسابات ويربطها بالمشاريع',  level: 'org-admin', projects: { sales: 'owner', hr: 'owner', proc: 'owner', cx: 'owner' } },
    owner:    { name: 'سارة العتيبي',    letter: 'س', color: 'var(--brand-teal)',   title: 'مالك مشروع',     desc: 'مالك «المبيعات» ومحرر «الموارد البشرية»',          level: 'member', projects: { sales: 'owner', hr: 'editor' } },
    editor:   { name: 'نورة القحطاني',   letter: 'ن', color: 'var(--brand-purple)', title: 'محرر',           desc: 'محرر في «المبيعات» و«تجربة العملاء»',              level: 'member', projects: { sales: 'editor', cx: 'editor' } },
    viewer:   { name: 'خالد الدوسري',    letter: 'خ', color: '#0E8FB8',             title: 'مشاهد',          desc: 'مشاهد في «المبيعات» فقط',                          level: 'member', projects: { sales: 'viewer' } }
  };
  const CURRENT_PROJECT = 'sales'; // صفحات المشروع في النموذج كلها لمشروع «منصة المبيعات»

  const store = {
    get() { try { return localStorage.getItem('jeem.persona'); } catch (e) { return null; } },
    set(v) { try { localStorage.setItem('jeem.persona', v); } catch (e) { /* تجاهل */ } }
  };
  const fromUrl = new URLSearchParams(location.search).get('as');
  if (fromUrl && PERSONAS[fromUrl]) store.set(fromUrl);
  const key = (fromUrl && PERSONAS[fromUrl]) ? fromUrl : (PERSONAS[store.get()] ? store.get() : 'orgadmin');
  const P = PERSONAS[key];
  const projRole = P.projects[CURRENT_PROJECT] || null;

  /* ---- حماية الصفحات: تحويل لو الدور مالوش وصول ---- */
  const scope = document.body.dataset.scope;
  const home = P.level === 'super' ? 'platform-organizations.html' : 'org-projects.html';
  let denied = false;
  if (scope === 'platform' && P.level !== 'super') denied = true;
  if ((scope === 'org' || scope === 'project') && P.level === 'super') denied = true;
  if (scope === 'org-admin' && P.level !== 'org-admin') denied = true;
  if (scope === 'project' && !projRole) denied = true;
  if (denied) { location.replace(home); return; }

  /* ---- تطبيق القواعد على العناصر ---- */
  const allowed = req => {
    if (!req) return true;
    if (req === 'super') return P.level === 'super';
    if (req === 'org-admin') return P.level === 'org-admin';
    if (req.startsWith('project:')) return !!projRole && RANK[projRole] >= RANK[req.split(':')[1]];
    return true;
  };
  document.documentElement.dataset.persona = key;
  if (projRole) document.documentElement.dataset.projectRole = projRole;

  document.querySelectorAll('[data-require]').forEach(el => { if (!allowed(el.dataset.require)) el.hidden = true; });
  document.querySelectorAll('[data-require-enable]').forEach(el => {
    if (!allowed(el.dataset.requireEnable)) { el.disabled = true; el.setAttribute('aria-disabled', 'true'); el.title = 'يتطلب صلاحية أعلى'; }
  });
  document.querySelectorAll('[data-show-roles]').forEach(el => {
    el.hidden = !(projRole && el.dataset.showRoles.split(',').includes(projRole));
  });
  document.querySelectorAll('[data-project]').forEach(el => {
    const r = P.projects[el.dataset.project];
    if (!r) { el.hidden = true; return; }
    el.querySelectorAll('.js-role').forEach(b => { b.textContent = ROLE_AR[r]; b.className = 'badge role js-role ' + ROLE_BADGE[r]; });
  });
  /* عدّاد المشاريع في القائمة والصفحة */
  const visible = Object.keys(P.projects).length;
  document.querySelectorAll('.js-proj-count').forEach(el => { el.textContent = visible; });

  /* ---- بطاقة المستخدم ---- */
  document.querySelectorAll('.user-card').forEach(c => {
    const a = c.querySelector('.avatar'); if (a) { a.textContent = P.letter; a.style.background = P.color; }
    const s = c.querySelector('.meta strong'); if (s) s.textContent = P.name;
    const t = c.querySelector('.meta span');
    if (t) t.textContent = P.level === 'member' ? (projRole && scope === 'project' ? ROLE_AR[projRole] + ' في هذا المشروع' : 'عضو في المنظمة') : P.title;
  });
  document.querySelectorAll('.js-greet').forEach(el => { el.textContent = 'مساء الخير يا ' + P.name.split(' ')[0]; });
  document.querySelectorAll('.js-my-role').forEach(el => { el.textContent = projRole ? ROLE_AR[projRole] : ''; el.className = 'badge js-my-role ' + (ROLE_BADGE[projRole] || ''); });

  /* ---- زر «عرض كـ» (أداة معاينة) ---- */
  const css = `
  .persona-fab{position:fixed;bottom:20px;inset-inline-end:20px;z-index:60;font-family:var(--font)}
  .persona-fab.in-sidebar{position:relative;bottom:auto;inset-inline-end:auto}
  .persona-fab.in-sidebar>button{width:100%;height:44px;box-shadow:none;border-style:dashed;border-radius:12px;padding-inline:8px 12px}
  .persona-fab.in-sidebar>button .txt{flex:1;text-align:start}
  .persona-fab.in-sidebar .persona-menu{position:fixed;inset-inline-end:auto;inset-inline-start:12px;bottom:176px;z-index:70}
  .app.is-collapsed .persona-fab.in-sidebar>button{justify-content:center;padding:0;border-style:solid}
  .app.is-collapsed .persona-fab.in-sidebar>button .txt,.app.is-collapsed .persona-fab.in-sidebar>button .tag{display:none}
  .persona-fab>button{display:flex;align-items:center;gap:10px;height:48px;padding:0 8px 0 16px;padding-inline:8px 16px;border-radius:999px;border:1px solid var(--line);background:var(--surface);box-shadow:var(--shadow-3);font-weight:500;font-size:13px;color:var(--ink)}
  .persona-fab>button .avatar{width:32px;height:32px;font-size:13px}
  .persona-fab>button small{display:block;font-size:11px;color:var(--ink-3);font-weight:300;line-height:1.2}
  .persona-fab>button .tag{font-size:10px;font-weight:700;color:#fff;background:var(--brand-cyan);border-radius:6px;padding:1px 6px}
  .persona-menu{position:absolute;bottom:58px;inset-inline-end:0;width:330px;padding:8px;border-radius:16px;background:var(--surface);border:1px solid var(--line);box-shadow:var(--shadow-3);display:none;animation:rise .2s var(--ease)}
  .persona-fab.is-open .persona-menu{display:block}
  .persona-menu h4{font-size:12px;font-weight:500;color:var(--ink-3);padding:8px 10px 6px;margin:0}
  .persona-menu a{display:grid;grid-template-columns:36px 1fr auto;gap:10px;align-items:center;padding:10px;border-radius:12px;color:var(--ink)}
  .persona-menu a:hover{background:var(--bg)}
  .persona-menu a[aria-current="true"]{background:var(--blue-50)}
  .persona-menu a strong{display:block;font-weight:700;font-size:13px}
  .persona-menu a span{display:block;font-size:11px;color:var(--ink-3);line-height:1.5}
  .persona-menu .sep{height:1px;background:var(--line-2);margin:4px 6px}
  .persona-menu .lvl{font-size:10px;padding:2px 8px;border-radius:999px;background:var(--bg);color:var(--ink-2);white-space:nowrap}
  @media (max-width:860px){.persona-fab:not(.in-sidebar){bottom:12px;inset-inline-end:12px}.persona-fab:not(.in-sidebar)>button .txt{display:none}}`;
  document.head.insertAdjacentHTML('beforeend', `<style>${css}</style>`);
  const lvl = k => ({ super: 'المنصّة', orgadmin: 'المنظمة', owner: 'المشروع', editor: 'المشروع', viewer: 'المشروع' }[k]);
  const item = k => {
    const p = PERSONAS[k];
    return `<a href="#" data-persona="${k}" ${k === key ? 'aria-current="true"' : ''}><span class="avatar" style="background:${p.color};width:36px;height:36px">${p.letter}</span><div><strong>${p.title} · ${p.name}</strong><span>${p.desc}</span></div><span class="lvl">${lvl(k)}</span></a>`;
  };
  const fab = document.createElement('div');
  fab.className = 'persona-fab';
  fab.innerHTML = `<div class="persona-menu" role="menu" aria-label="عرض كـ"><h4>جرّب النظام بدور مختلف</h4>${item('super')}<div class="sep"></div>${item('orgadmin')}<div class="sep"></div>${item('owner')}${item('editor')}${item('viewer')}</div>
    <button type="button" aria-haspopup="true" aria-expanded="false"><span class="avatar" style="background:${P.color}">${P.letter}</span><span class="txt"><small>عرض كـ</small>${P.title}</span><span class="tag">معاينة</span></button>`;
  const foot = document.querySelector('.sidebar .sb-foot');
  if (foot) { fab.classList.add('in-sidebar'); foot.prepend(fab); } else { document.body.appendChild(fab); }
  const btn = fab.querySelector('button');
  btn.addEventListener('click', e => { e.stopPropagation(); const o = fab.classList.toggle('is-open'); btn.setAttribute('aria-expanded', String(o)); });
  document.addEventListener('click', () => fab.classList.remove('is-open'));
  document.addEventListener('keydown', e => { if (e.key === 'Escape') fab.classList.remove('is-open'); });
  fab.querySelectorAll('[data-persona]').forEach(a => a.addEventListener('click', e => {
    e.preventDefault();
    const k = a.dataset.persona; store.set(k);
    const target = PERSONAS[k].level === 'super'
      ? (scope === 'platform' ? location.pathname.split('/').pop() : 'platform-organizations.html')
      : (scope === 'platform' || scope === 'login' ? 'org-projects.html' : location.pathname.split('/').pop());
    location.href = target + '?as=' + k;
  }));
})();
