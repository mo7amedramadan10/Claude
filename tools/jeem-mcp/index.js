#!/usr/bin/env node
// جسر MCP محلي لمنصة جيم: يعمل على جهاز المستخدم داخل شبكة الشركة (Claude Desktop يشغّله بـ stdio)،
// ويترجم أدوات النشر إلى نداءات HTTP على واجهة المنصة الداخلية بمفتاح الوصول الشخصي.
// الإعداد عبر متغيرات البيئة: JEEM_URL (عنوان المنصة) وJEEM_TOKEN (مفتاح من «ربط Claude» في المنصة).
import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js';
import { StdioServerTransport } from '@modelcontextprotocol/sdk/server/stdio.js';
import { z } from 'zod';

const BASE = (process.env.JEEM_URL || '').replace(/\/+$/, '');
const TOKEN = process.env.JEEM_TOKEN || '';
if (!BASE || !TOKEN) {
  console.error('JEEM_URL وJEEM_TOKEN مطلوبان في بيئة التشغيل (انظر README).');
  process.exit(1);
}

async function api(path, { method = 'GET', body, form } = {}) {
  const headers = { Authorization: `Bearer ${TOKEN}` };
  let payload;
  if (form) payload = form;
  else if (body !== undefined) { headers['Content-Type'] = 'application/json'; payload = JSON.stringify(body); }
  const res = await fetch(BASE + path, { method, headers, body: payload });
  const text = await res.text();
  let data = null; try { data = text ? JSON.parse(text) : null; } catch { data = text; }
  if (!res.ok) {
    const msg = ((data && data.error) || (typeof data === 'string' ? data.slice(0, 300) : '') || res.statusText) + (data && data.suggestion ? ` (اقتراح: ${data.suggestion})` : '');
    throw new Error(res.status === 401 ? 'مفتاح الوصول غير صالح أو مُلغى — أنشئ مفتاحًا جديدًا من «ربط Claude» في المنصة.' : `${res.status}: ${msg}`);
  }
  return data;
}

const publicUrl = p => `${BASE}${p.url || `/p/${p.id}`}`;
const text = value => ({ content: [{ type: 'text', text: typeof value === 'string' ? value : JSON.stringify(value, null, 2) }] });
const fail = err => ({ isError: true, content: [{ type: 'text', text: `تعذّر تنفيذ الطلب: ${err.message}` }] });

const server = new McpServer({ name: 'jeem', version: '1.0.0' });

server.registerTool('publish_presentation', {
  title: 'نشر عرض تقديمي على جيم',
  description: 'يرفع صفحة HTML كاملة (عرض تقديمي، تقرير، لوحة ثابتة) إلى منصة جيم ويعيد رابطًا عامًا يفتحه أي شخص بلا تسجيل دخول. مرّر مستند HTML كاملًا (doctype + head + body) بكل أنماطه وسكربتاته مضمّنة.',
  inputSchema: {
    title: z.string().min(1).max(300).describe('عنوان العرض كما يظهر في القائمة وشريط صفحة العرض'),
    html: z.string().min(20).describe('مستند HTML كامل ومكتفٍ بذاته'),
    slug: z.string().min(2).max(80).optional().describe('اسم الرابط (حروف/أرقام/شرطات) فريد داخل المنظمة، مثل q3-results؛ يُشتق من العنوان إن تُرك'),
    expiresInDays: z.number().int().positive().max(3650).optional().describe('انتهاء الرابط بعد كذا يومًا (اتركه فارغًا لرابط دائم)'),
  },
}, async ({ title, html, slug, expiresInDays }) => {
  try {
    const form = new FormData();
    form.append('file', new Blob([html], { type: 'text/html' }), `${title.replace(/[\\/:*?"<>|]+/g, '-')}.html`);
    form.append('title', title);
    if (slug) form.append('slug', slug);
    if (expiresInDays) form.append('expiresAt', new Date(Date.now() + expiresInDays * 86400000).toISOString());
    const r = await api('/api/presentations', { method: 'POST', form });
    return text({ id: r.id, title: r.title, slug: r.slug, url: publicUrl(r), note: 'شارك الرابط مع أي شخص؛ يفتح بلا تسجيل دخول. لاستبدال المحتوى لاحقًا على نفس الرابط استخدم replace_presentation بهذا المعرّف.' });
  } catch (e) { return fail(e); }
});

server.registerTool('replace_presentation', {
  title: 'استبدال محتوى عرض منشور',
  description: 'يستبدل HTML عرض منشور سابقًا بنسخة أحدث مع الإبقاء على نفس الرابط العام (المعرّف من publish_presentation أو list_presentations).',
  inputSchema: {
    id: z.string().min(1).describe('معرّف العرض'),
    html: z.string().min(20).describe('مستند HTML الجديد كاملًا'),
    title: z.string().max(300).optional().describe('عنوان جديد (اختياري)'),
  },
}, async ({ id, html, title }) => {
  try {
    const form = new FormData();
    form.append('file', new Blob([html], { type: 'text/html' }), 'presentation.html');
    if (title) form.append('title', title);
    await api(`/api/presentations/${encodeURIComponent(id)}/file`, { method: 'PUT', form });
    const items = await api('/api/presentations'); const p = (items.items || []).find(x => x.id === id);
    return text({ id, url: p ? publicUrl(p) : publicUrl({ id }), updated: true });
  } catch (e) { return fail(e); }
});

server.registerTool('list_presentations', {
  title: 'قائمة العروض المنشورة',
  description: 'يعيد عروض المنظمة المنشورة على جيم: المعرّف والعنوان والرابط والحالة وعدد المشاهدات وصاحب العرض.',
  inputSchema: {},
}, async () => {
  try {
    const items = (await api('/api/presentations')).items || [];
    return text(items.map(p => ({
      id: p.id, title: p.title, slug: p.slug, url: publicUrl(p), active: p.isActive, views: p.viewCount,
      createdBy: p.createdByName, createdAt: p.createdAt, expiresAt: p.expiresAt, canManage: p.canManage,
    })));
  } catch (e) { return fail(e); }
});

server.registerTool('revoke_presentation', {
  title: 'إلغاء رابط عرض',
  description: 'يوقف الرابط العام لعرض منشور فورًا (يظهر للزائر أن الرابط أُلغي). يمكن إعادة تفعيله من المنصة.',
  inputSchema: { id: z.string().min(1).describe('معرّف العرض') },
}, async ({ id }) => {
  try { await api(`/api/presentations/${encodeURIComponent(id)}/revoke`, { method: 'POST' }); return text({ id, revoked: true }); }
  catch (e) { return fail(e); }
});

server.registerTool('whoami', {
  title: 'التحقق من الاتصال بجيم',
  description: 'يتحقق من أن عنوان المنصة ومفتاح الوصول يعملان ويعيد اسم الحساب المرتبط.',
  inputSchema: {},
}, async () => {
  try { const me = await api('/api/tokens/whoami'); return text({ platform: BASE, user: me.displayName || me.username, role: me.role }); }
  catch (e) { return fail(e); }
});

await server.connect(new StdioServerTransport());
