# Task: rebuild Databox dashboard templates as Jeem (جيم) dashboards

Jeem is an Arabic (RTL) BI SaaS. We are adding the public dashboard templates from https://databox.com/dashboard-examples
to our own template gallery, re-expressed in Jeem's JSON format, in Modern Standard Arabic, tool-agnostic.

## Steps
1. For every slug in your batch, call WebFetch on `https://databox.com/dashboard-examples/<slug>` with a prompt like:
   "List every metric/visualization shown on this dashboard template (metric name, visualization type e.g. number, line, bar, table, pie, funnel, and any breakdown dimension), the data sources/integrations, and the template's description/purpose. Be exhaustive."
   If the fetch fails or returns little, fall back to what the title implies and your knowledge of that tool's standard metrics (note it in your final summary).
   Fetch several pages in parallel (multiple WebFetch calls in one message). Do NOT use curl/python to fetch.
2. Translate each template into one Jeem dashboard object (schema below). Reflect the ORIGINAL metrics faithfully — same metrics, similar visualization type, similar count. Do not pad with unrelated metrics.
3. MERGE tool-variants inside your batch: templates that are the same dashboard over different tools (e.g. "E-commerce Financial (Shopify + Xero)" and "(WooCommerce + QuickBooks)") become ONE Jeem dashboard whose `ref` lists all of them and whose `sources` lists the tool alternatives. Only merge when the metric sets are essentially the same.
4. Write the JSON array to `out/<BATCH>.json` (path given in your prompt) and run `python3 validate.py out/<BATCH>.json`; fix until 0 errors.
5. Final reply (short): number of templates processed, number of dashboards produced, merges done, slugs whose page could not be fetched.

## Dashboard object
```json
{
  "id": "db-ga4-acquisition",            // db- + short kebab slug, unique, no vendor names needed
  "cat": "marketing",                    // marketing | ads | social | sales | retail | finance | saas | cx | dev | projects
  "name": "اكتساب زوار الموقع",           // Arabic, functional, NO vendor/tool names (platform names that ARE the subject like إنستغرام/يوتيوب/لينكدإن/تيك توك/إكس/فيسبوك/جوجل are OK)
  "en": "Website Acquisition Overview",  // English generic name, same rule (no HubSpot/Semrush/Shopify… names)
  "desc": "من أين يأتي الزوار وكيف يتحولون…", // Arabic, 1–2 sentences: what questions it answers
  "sources": ["Google Analytics 4"],      // the real tools/integrations (Latin names OK here), alternatives joined like "Shopify / WooCommerce"
  "ref": [{"title": "Website Acquisition Overview", "slug": "google-analytics-4-acquisition-overview-dashboard-template"}],
  "f": [["الفترة", ["آخر 7 أيام", "آخر 30 يومًا", "هذا الربع"]], ["الجهاز", ["جوال", "حاسب مكتبي"]]],   // 0–3 filters, 2–6 options each
  "p": [ { "n": "نظرة عامة", "w": [ ...widgets ] } ]   // 1–4 pages; use >1 page only when the original has many metrics (>14) or clear sections
}
```
Category guide: marketing = web analytics, SEO, email, marketing automation, content, leads; ads = paid advertising (any network);
social = organic social & video; sales = CRM/pipeline/sales activity/calls; retail = e-commerce stores; finance = accounting/P&L/cash/balance/payments;
saas = subscriptions/MRR/churn; cx = customer support/success/surveys; dev = software dev, app stores, product analytics; projects = project/time tracking.

## Widgets (exact keys; validator rejects unknown keys)
Layout: a page is a 12-column grid. Consecutive kpi/kpiyoy widgets form one KPI strip (max 8 per strip; 3–6 is ideal). Other widgets flow in rows with span `sp` (2,3,4,5,6,7,8,9,10,12); the last widget of a row stretches to fill. Typical: line/area sp 8 + donut/hbars sp 4; two sp 6; table sp 12.
Put KPIs first on each page, then charts. Values are DEMO numbers: realistic, Western digits, formatted strings for KPIs.

- `{"t":"kpi","ti":"الجلسات","v":"48,210","u":"","d":"+12.4%","g":true}` — u unit (ر.س for money, %, ث, دقيقة, يوم…), d delta string vs previous period, g = is this delta good (false for rising cost/bounce/churn). Optional: "spk":1 sparkline, "hl":1 highlight (one per page max), "n" comparison note.
- `{"t":"kpiyoy","ti":"…","ly":"1.2م","cy":"1.5م","d":"+25%","g":true}` year-over-year card.
- `{"t":"line","ti":"الجلسات يوميًا","x":"DAY7"|"M12"|"W13"|[labels],"b":1500,"k":"","s":["الجلسات","المستخدمون"],"ly":1,"sp":8}` — b = typical magnitude, k number format: "" plain, "sar" money, "pct" percent, "s" seconds/decimals, "r" ratio 2dp. ly:1 adds dashed previous-period line (then omit s). Max 4 series; all series share ONE axis — never put money and % on the same chart.
- `area`, `cum` (cumulative): same keys as line.
- `{"t":"bars","ti":"…","x":"M12","b":200,"k":""}` single series columns. `grouped`/`stacked` need "s" (2–4 series names) and x. `combo` = bars + small rate strip: {"t":"combo","x":"M12","s":["التحويلات","معدل التحويل %"],"b":300,"lb":3.2,"lk":"pct"}.
- `{"t":"hbars","ti":"…","x":"TRF"|[labels],"b":12000,"k":""}` horizontal ranked bars (≤12). `rank` same but numbered with share. `compare` = previous vs current per item.
- `{"t":"donut","ti":"…","x":"DEVC"|[≤5 labels],"tot":"48.2 ألف"}` share breakdown.
- `{"t":"s100","ti":"…","x":[rows],"s":["جديد","عائد"]}` 100% stacked rows.
- `{"t":"funnel","ti":"…","x":["الزيارات","العملاء المحتملون","الفرص","العملاء"],"b":25000}`
- `{"t":"ring","ti":"…","v":72,"lab":"من المستهدف","it":[["المستهدف","500"],["المحقق","360"]]}` progress 0–100.
- `{"t":"gauges","ti":"…","x":[≤6 labels],"tg":80}` percentage gauges vs target.
- `{"t":"heat","ti":"…","rows":"DAY7","cols":"HR8","b":100,"num":1}` matrix.
- `{"t":"waterfall","ti":"…","x":["الإيرادات","تكلفة المبيعات","المصروفات","الضرائب","صافي الربح"],"b":1000000,"k":"sar","sg":[-1,-1,-1]}` sg = sign of each middle step.
- `{"t":"table","ti":"…","rows":"PAGE"|[labels],"c":[["الصفحة","l"],["المشاهدات","n"],["متوسط المدة","s"],["التغيّر","dp"],["الاتجاه","sp"]],"n":8,"sp":12}` column kinds: l row label, n number, sar money, pct percent, d days, r ratio, s decimal, dp delta %, sp sparkline, bar inline bar %, st status tag (with "sts":[["نشطة","ok"],["متوقفة","bad"]]), dt date, id code (with "idp":"INV-"), p person name, "@DIM" value from a dimension (e.g. "@TRF").
- `{"t":"cohort","ti":"…","rows":"M12:6","cols":6}` retention cohort.
- `{"t":"insights","ti":"…","it":[["عنوان","نص قصير","قيمة اختيارية","ok|warn|bad"]]}`.
- Also allowed: `div` (diverging +/- bars, x,b,k), `scatter` (s,xl,yl), `hero` (big number: ti,v,u,it:[[label,value]]), `note` (tx:[strings]).
Every widget may have "su" (Arabic subtitle) and "sp".

Built-in dimensions for x/rows/cols (use "KEY" or "KEY:n" for first n):
M12 months, W13 weeks, Q4, Y5 years, DAY7 weekdays, DAY5, HR8 hours, TRF traffic channels (بحث عضوي، مباشر، تواصل اجتماعي، بحث مدفوع، إحالة، بريد إلكتروني، أخرى),
DEVC devices, SOC social networks, CNTRY countries, PAGE site pages, KW keywords, CMPN ad/email campaigns, LS lifecycle stages, DST deal stages, PLAN subscription plans,
AGEM age bands, GEN gender, PROD products, AGENT support agents, PERSON people (sales reps), CITY Saudi cities, REG regions, CH sales channels, SUP companies/customers, DEPT departments, PRJ projects, STAT statuses.
Use explicit Arabic label arrays when none fits (e.g. ad sets, ticket priorities, repos).

## Language rules
- Modern Standard Arabic. Common acronyms may follow in parentheses: "تكلفة النقرة (CPC)", "الإيراد الشهري المتكرر (MRR)", "معدل النقر (CTR)".
- Money uses "ر.س" (Saudi riyal) in u, k:"sar" in charts. Sample magnitudes appropriate for a mid-size Saudi company.
- Keep titles short (≤ 5 words where possible). No emojis. No vendor names in name/en/titles; platform names (جوجل، إنستغرام…) OK where the platform is the subject.
