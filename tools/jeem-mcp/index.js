#!/usr/bin/env node
// جسر MCP محلي لمنصة جيم: يعمل على جهاز المستخدم داخل شبكة الشركة (Claude Desktop يشغّله بـ stdio)،
// ويترجم أدوات النشر إلى نداءات HTTP على واجهة المنصة الداخلية بمفتاح الوصول الشخصي.
// الإعداد عبر متغيرات البيئة: JEEM_URL (عنوان المنصة) وJEEM_TOKEN (مفتاح من «ربط Claude» في المنصة).
import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js';
import { StdioServerTransport } from '@modelcontextprotocol/sdk/server/stdio.js';
import { z } from 'zod';
import fs from 'node:fs';
import path from 'node:path';
import os from 'node:os';

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

// مصدر HTML للنشر/الاستبدال: إمّا نص مباشر، أو مسار ملف على جهاز المستخدم (الجسر يعمل محليًا فيقرأه هو)
// — فلا يحتاج الموديل إلى إعادة كتابة صفحة كبيرة حرفًا بحرف في كل تحديث. يعيد النص واسم الملف.
const MAX_BYTES = 15 * 1024 * 1024;
function resolveHtml({ html, filePath }, fallbackName) {
  if (filePath) {
    let p = filePath.trim();
    if (p.startsWith('~')) p = path.join(os.homedir(), p.slice(1));
    p = path.resolve(p);
    if (!fs.existsSync(p)) throw new Error(`الملف غير موجود: ${p}`);
    if (!/\.html?$/i.test(p)) throw new Error('الملف يجب أن يكون صفحة HTML واحدة (.html).');
    const size = fs.statSync(p).size;
    if (size > MAX_BYTES) throw new Error(`حجم الملف ${(size / 1024 / 1024).toFixed(1)} ميغابايت يتجاوز الحد (15).`);
    return { html: fs.readFileSync(p, 'utf8'), fileName: path.basename(p) };
  }
  if (!html || html.trim().length < 20) throw new Error('مرّر html كاملًا أو filePath لملف HTML موجود على هذا الجهاز.');
  return { html, fileName: fallbackName };
}
const text = value => ({ content: [{ type: 'text', text: typeof value === 'string' ? value : JSON.stringify(value, null, 2) }] });
const fail = err => ({ isError: true, content: [{ type: 'text', text: `تعذّر تنفيذ الطلب: ${err.message}` }] });

const server = new McpServer({ name: 'jeem', version: '1.0.0' });

server.registerTool('publish_presentation', {
  title: 'نشر عرض تقديمي على جيم',
  description: 'يرفع صفحة HTML كاملة (عرض تقديمي، تقرير، لوحة ثابتة) إلى منصة جيم ويعيد رابطًا عامًا يفتحه أي شخص بلا تسجيل دخول. مرّر إمّا filePath لملف HTML محفوظ على هذا الجهاز (الأفضل للصفحات الكبيرة أو المعدَّلة محليًا)، أو html نصًا كاملًا (doctype + head + body) بكل أنماطه وسكربتاته مضمّنة.',
  inputSchema: {
    title: z.string().min(1).max(300).describe('عنوان العرض كما يظهر في القائمة وشريط صفحة العرض'),
    filePath: z.string().min(1).optional().describe('مسار ملف .html على هذا الجهاز (مطلق أو يبدأ بـ ~) يقرؤه الجسر مباشرة — بديل عن html'),
    html: z.string().optional().describe('مستند HTML كامل ومكتفٍ بذاته — يُهمل إن أُعطي filePath'),
    slug: z.string().min(2).max(80).optional().describe('اسم الرابط (حروف/أرقام/شرطات) فريد داخل المنظمة، مثل q3-results؛ يُشتق من العنوان إن تُرك'),
    expiresInDays: z.number().int().positive().max(3650).optional().describe('انتهاء الرابط بعد كذا يومًا (اتركه فارغًا لرابط دائم)'),
  },
}, async ({ title, html, filePath, slug, expiresInDays }) => {
  try {
    const src = resolveHtml({ html, filePath }, `${title.replace(/[\\/:*?"<>|]+/g, '-')}.html`);
    const form = new FormData();
    form.append('file', new Blob([src.html], { type: 'text/html' }), src.fileName);
    form.append('title', title);
    if (slug) form.append('slug', slug);
    if (expiresInDays) form.append('expiresAt', new Date(Date.now() + expiresInDays * 86400000).toISOString());
    const r = await api('/api/presentations', { method: 'POST', form });
    return text({ id: r.id, title: r.title, slug: r.slug, url: publicUrl(r), note: 'شارك الرابط مع أي شخص؛ يفتح بلا تسجيل دخول. لاستبدال المحتوى لاحقًا على نفس الرابط استخدم replace_presentation بهذا المعرّف.' });
  } catch (e) { return fail(e); }
});

server.registerTool('replace_presentation', {
  title: 'استبدال محتوى عرض منشور',
  description: 'يستبدل HTML عرض منشور سابقًا بنسخة أحدث مع الإبقاء على نفس الرابط العام (المعرّف من publish_presentation أو list_presentations). مرّر filePath لملف محلي معدَّل (الأفضل) أو html نصًا كاملًا.',
  inputSchema: {
    id: z.string().min(1).describe('معرّف العرض'),
    filePath: z.string().min(1).optional().describe('مسار ملف .html على هذا الجهاز يقرؤه الجسر مباشرة — بديل عن html'),
    html: z.string().optional().describe('مستند HTML الجديد كاملًا — يُهمل إن أُعطي filePath'),
    title: z.string().max(300).optional().describe('عنوان جديد (اختياري)'),
  },
}, async ({ id, html, filePath, title }) => {
  try {
    const src = resolveHtml({ html, filePath }, 'presentation.html');
    const form = new FormData();
    form.append('file', new Blob([src.html], { type: 'text/html' }), src.fileName);
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
