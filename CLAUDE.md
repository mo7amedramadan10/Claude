# جيم (Chat to Dashboard) — دليل العمل على §4.16-4.24

هذا الملف يُقرأ تلقائيًا في أول كل جلسة Claude Code على هذا الـ repo. الهدف
منه إنك متبدأش من الصفر: السبيك والمرجع البصري والقرارات المعمارية كلها
موثّقة هنا أو مُشار لمكانها.

## السياق العام

- بنية التطبيق الأساسية (ASP.NET Core + SQL Server حصرًا، كيف البيانات
  بتتحمّل، إزاي المحادثة بتولّد لوحات) موثّقة في [README.md](README.md) —
  اقرأه لو محتاج خلفية عن مخططات القاعدة (`dbo` المشترك + `proj_<id>` لكل مشروع) أو مسار `/api/chat`.
- فرع العمل الحالي: **`claude/chat-to-dashboard-app-at0up6`** (مش `main`) —
  كل الشغل بتاع §4.16-4.24 عليه.

## السبيك الملزم (اقرأه قبل أي حاجة)

**[docs/reference/jeem-ui-prototype/INTERACTIVE-HANDOFF.md](docs/reference/jeem-ui-prototype/INTERACTIVE-HANDOFF.md)**
هو السبيك الملزم لمهمة §4.16-4.24 (اللوحات التفاعلية). هو اللي يحسم أي خلاف
مع أي مصدر تاني، بما فيه النموذج الأولي نفسه.

**[docs/reference/jeem-ui-prototype/IMPLEMENTATION.md](docs/reference/jeem-ui-prototype/IMPLEMENTATION.md)**
دليل التنفيذ الأوسع للمنتج كله (مش بس اللوحات التفاعلية) — خلفية مفيدة لو
السؤال طلع برّه نطاق §4.16-4.24.

**[docs/reference/jeem-ui-prototype/](docs/reference/jeem-ui-prototype/)**
النموذج الأولي الكامل (HTML/CSS/JS ثابت) — **مرجع بصري وسلوكي فقط، لا
يُنسخ أو يُشحن كما هو**. كل مولّد بيانات تجريبية فيه لازم يتستبدل ببيانات
حقيقية في كود الإنتاج (البيانات التجريبية تفضل بس لمعاينة معرض النماذج).

## القيود الثابتة (ما تتخالفش)

- ممنوع إضافة أي عنصر navigation اسمه "Interactive Dashboards" أو مشابه.
- كل مولّد بيانات تجريبية من النموذج الأولي يتستبدل بـ API حقيقي في كود
  الإنتاج — ملفات `-demo.js` تفضل للمعاينة في معرض النماذج فقط.
- قواعد RTL/تنسيق الأرقام، ألوان الحالة المحجوزة، أيقونة+نص (لا لون لوحده)،
  ممنوع رسوم بمحورين (dual-axis)، `prefers-reduced-motion` يلغي كل حركة،
  قواعد الوصولية (accessibility).
- ممنوع حذف أو إعادة تسمية أي شيء موجود من غير ما تسأل المستخدم أولًا.
- عند التعارض: السبيك يفوز على النموذج الأولي.
- التنفيذ مرحلة بمرحلة: ابنِ، اختبر، لخّص للمستخدم بعد كل مرحلة — ما
  تكملش عدة مراحل من غير توقف.
- كل نص وتعليقات الكود بالعربية الفصحى.

## حالة المشروع

- ✅ **Phase 1 (§4.16-4.21)**: 6 نماذج تفاعلية مستقلة — واجهة + موفّري
  بيانات حقيقية. كاملة.
- ✅ **Phase 2 (§4.22)**: `dash-kit.js` المشتركة + 11 لوحة (saas-metrics،
  project-studio، campaign-project، treasury-funding، construction-crm،
  task-board، crm-leads، supplier-risk، crm-analysis، supply-command،
  finance-ops) — كل واحدة فيها محرك JS + بيانات تجريبية للمعرض + موفّر
  بيانات حقيقي. كاملة.
- ✅ **Phase 3 (§4.23)**: شاشة "اللوحات المنشورة" (`screen-published`).
  جدول `PublishedDashboards` مستقل (`Projects/PublishedDashboardStore.cs`)
  عن `PublishedDashboardSlots` الخاص بالتكاملات، و`/api/published-dashboards`
  (`PublishedDashboardsController`). الجمهور مفردات ثابتة مربوطة بأدوار
  المشروع (`all`/`editors`/`owners`)، والحساب بلا دور صريح يُعامل كمشاهد.
  كل لوحة (محادثة أو تفاعلية) تُعرض في iframe بوضع `?embed=1&pub=<id>&p=<n>`
  (`bootEmbedView` في app.js) يتبادل postMessage مع `wwwroot/js/project-published.js`.
  النشر من زر 📌 في «اللوحات النشطة» أو زر «نشر لوحة» في الشاشة.
- ✅ **Phase 4 (§4.24)**: `stages` + `vals` في `bars` + أبعاد متتبع الفرص في
  `dash-render.js` (معاينة المعرض)، ومدخل `pl-opportunities-tracker` في
  `dash-library.js`. عنصر `stages` الحقيقي في app.js (`StagesCard`) يقبل الآن الشكل
  الطويل العددي `{stage,label,count,tone,value?,row?: main|sub|total}` مع بقاء
  الشكل الأقدم يعمل، ومُعرَّف للذكاء الاصطناعي في `AnalyticsTools.cs`. النموذج
  في الكتالوج مكتوب يدويًا في `Templates/BuiltinDashboardsOpportunities.cs`
  (لا في الملف المولَّد).
- ✅ **مراجعة قبول نهائية (§9)**: مرّت كل نماذج JR (213) ولوحات المعرض التفاعلية (22)
  بلا أخطاء، وبلا تمرير أفقي على 390px، ومع `prefers-reduced-motion` بلا حركة جارية.
  إخفاء أزرار التنفيذ عن المشاهد مركزي في app.js (`INTERACTIVE_EXEC_HIDE`/`_LOCK`/
  `_KEYMOVE` + `applyInteractiveViewerGuard`) — أي محرك جديد بأزرار تنفيذ يُضاف
  محدِّده لهذه القوائم.

## البناء المرحلي لقوالب اللوحات (`Build/`)

مسار بناء القالب الحقيقي («استخدم هذا النموذج») لم يعد حلقة أدوات واحدة بسقف 15
دورة — ده كان السبب الرئيسي في اللوحات الفاضية (قالب بـ28 عنصر محتاج 28+ استعلام
في 11 دورة، والباقي `noData`). الجديد في `Build/StagedTemplateBuilder.cs`:

1. **ملف تعريف البيانات** (`DataProfileService`، بلا موديل): لكل جدول مرئي عدد الصفوف،
   والأعمدة بنوعها الدلالي (number/date/text) ومداها وقيمها الشائعة (≤20)، وعينة
   3 صفوف. مخزّن مؤقتًا 10 دقائق ببصمة الجداول.
2. **نداء تخطيط واحد** بلا أدوات: لكل عنصر إما SQL أو «غير ممكن» بسبب، + سياق مشترك
   (تعريفات المقاييس والفترة) + فلاتر مقترحة.
3. **نداء لكل عنصر ممكن** بالتوازي (`Build:Parallelism`، افتراضي 4)، أداة
   `query_data` فقط، 5 دورات كحد أقصى، شكل `data` للنوع المطلوب فقط من
   `WidgetShapeCatalog`. العنوان ثابت من القالب (التسوية في `ChatController` تعتمد عليه).
4. **تجميع**: نداء صغير للملخص والسرد؛ والفلاتر تُتحقَّق قيمها من قاعدة البيانات
   مباشرة (`SELECT DISTINCT`) لا من الموديل.

- الطبقة الموحّدة للمزوّدين: `Llm/IStagedLlm.cs` تنفّذها `OpenAiClient` و`ClaudeClient`
  و`OllamaClient` (نفس حلقة الأدوات القديمة بسقف وإجبار قابلين للضبط؛ OpenAI بوضع `json_object`).
  **Ollama** (بوابة qwen3): لا `tool_choice` فيُحاكى إجبار الأداة في الدورة الأولى بتذكير «نادِ
  query_data أولًا» (`forceToolRounds`)، و`format: json` يُطلب فقط في دورة الرد النهائي (مع الأدوات
  يمنع بعض الموديلات من ندائها)، وكتلة `<think>…</think>` تُحذف قبل التفسير (`StripThinking`).
  مُختبَر على خادم وهمي بشكل Ollama (`mock-ollama.js` في scratchpad الجلسة: 14/28 عنصرًا
  باستعلامات + فلاتر محقَّقة)؛ **لم يُختبر على بوابة qwen3 حقيقية** (غير متاحة من بيئة الجلسة).
- التبديل: `Build:TemplateMode` = `staged` (افتراضي) أو `legacy`. الأسئلة الحرة ومتابعة
  لوحة معروضة تبقى على المسار القديم دائمًا.
- كل نداء مرحلي بيتسجّل في صفحة الاستهلاك بعنوان `[قالب] <الاسم> — <المرحلة/العنصر>`.
- **قياس حقيقي على gpt-4o** (ملف CSV مبيعات 240 صف، 9 أعمدة، نفس الجلسة):
  - «مسار المبيعات» (28 عنصر): القديم بنى **2** (kpi فقط، بلا فلاتر، بلا `query`)؛
    المرحلي بنى **12–16** بأنواع bar/line/kpi/table + 4 فلاتر بقيم حقيقية، في ~25 ثانية.
  - «متتبع الفرص» (23): القديم **1**؛ المرحلي **12–13** في ~16 ثانية.
  - أسباب `noData` في المرحلي صارت محددة وصادقة («لا يوجد عمود لسبب الخسارة»).
- **اختبار بلا مفتاح:** `OpenAI:BaseUrl` قابل للضبط؛ خادم وهمي يحاكي chat/completions
  (`mock-openai.js` في scratchpad الجلسة) يكفي لاختبار التنسيق من طرف لطرف.
- علّة قديمة أُصلحت بالمناسبة: عنصر `noData` الذي يضيفه `ChatController` عند التسوية كان بلا
  `Data` (JsonElement غير معرَّف) فيُسقط تسلسل الرد كله بـ 500 كلما أسقط الموديل عنصرًا
  مطلوبًا — أي أن المسار القديم كان يفشل بالكامل لا يرجع لوحة ناقصة فحسب.
- حقول `series`/`stacked`/`areaFill`/`centerLabel` أُضيفت إلى `DashboardWidget` (كانت تضيع
  عند إعادة التسلسل).

## طبقة المعنى (`Semantics/`) — المراحل (أ) ✅، (ج) ✅، (ب) ✅

المنتج SaaS: أي عميل يرفع أي ملف، فلا يمكن كتابة القوالب على أعمدة معروفة. الحل طبقة معنى
بين البيانات الخام والقوالب (بديل حقيقي لمكتبة الأسماء `SchemaLibrary/` التي تطابق
أسماءً بأسماء وتتعلّم فقط من اللوحات الناجحة):

- **`ConceptCatalog`**: قاموس ثابت (~55 مفهومًا: `revenue`، `deal_status`، `owner`…) بدور
  (measure/dimension/time/identifier/text/flag) ومرادفات عربية/إنجليزية وقيم قياسية للحالات.
  **الإضافة آمنة؛ إعادة تسمية معرّف تكسر المخزَّن.**
- **`ColumnSemanticsStore`**: جدول `ColumnSemantics(ProjectId, TableName, ColumnName, Concept,
  Role, Unit, ValueMapJson, Confidence, Source: rule|ai|user)`. صف `user` لا يُستبدل آليًا أبدًا.
- **`ColumnSemanticsService.AnalyzeTableAsync`**: ملف تعريف الجدول ← تخمين حتمي من الاسم ←
  نداء واحد للموديل بلا أدوات (يستدل من **القيم والعينة** لا الاسم وحده) يحدد المفهوم والثقة
  وخريطة القيم (`مفتوحة=open`). يعمل في الخلفية بعد الرفع والتحديث (`RepositoryController`)،
  ويُحذف مع الملف. على gpt-4.1: 9 أعمدة في ~6 ثوانٍ بثقة 1.0 وخريطة قيم صحيحة.
- API: `GET/POST(analyze) /api/repository/files/{id}/semantics`،
  `PUT …/semantics/{column}` لتأكيد/تصحيح المستخدم.
- الربط الحالي بالبناء: `GetPromptNotesAsync` تُلحق «المعنى: الإيراد (revenue)» بكل عمود في
  ملف تعريف البيانات لنداءات البناء المرحلي (ثقة ≥ 0.4).
- **(ج) ✅ الجدوى الحتمية** (`WidgetRequirements.cs`): `WidgetRequirementResolver.TitleRules`
  قواعد regex على عنوان العنصر → متطلبات مفاهيم. متطلب **صلب** (hard: مستهدف، ميزانية، احتمالية،
  مرحلة/حالة الصفقة، عميل، مورّد، قناة، تكلفة، زيارات، نقرات…) غيابه يحسم «غير ممكن» قبل أي
  نداء للموديل؛ متطلب **ليّن** (soft: منطقة، منتج، إيراد) تلميح فقط. **مجموعة معانٍ فارغة =
  مجهول** (لا رفض). `WidgetFeasibilityService.EvaluateAsync` تُستدعى في
  `StagedTemplateBuilder.BuildAsync` قبل المخطِّط: العناصر المرفوضة تخرج بـ `noData` وسبب
  «يحتاج «X» وهو غير موجود في بياناتك — أضف عمودًا له أو أكّد معنى عمود موجود من «المصادر»»،
  والمخطِّط يرى الممكن فقط. `GET /api/templates/feasibility` يعيد لكل قالب في الكتالوج
  `{templateId,total,buildable,topMissing}` (`buildable` null بلا ملفات). قياس حي (gpt-4.1،
  ملف مبيعات 9 أعمدة): pl-sales-pipeline 16 من 19 noData محسومة حتميًا بلا نداء، 17.7 ثانية.
- **(ج) ✅ التعلّم العابر للعملاء** (`ConceptPriorStore`): جدول `ConceptPriors(NormalizedName,
  Concept, Hits, UserHits)` — اسم العمود المُوحَّد فقط (`ConceptCatalog.Normalize`)، بلا أي
  بيانات عميل. يُسجَّل عند تأكيد المستخدم (`ColumnSemanticsService.SetByUserAsync`) وعند
  نتيجة موديل بثقة ≥ 0.9؛ يُستشار قبل التخمين من الاسم (تأكيد مستخدم واحد → 0.9، ثلاث نتائج
  موديل → 0.85).
- **الإدارة المجمّعة** (تبويب «معاني الأعمدة» في «مكتبة النماذج» بجوار «مكتبة الحقول والجداول»):
  `Controllers/SemanticsController.cs` — `GET /api/semantics` (كل جداول المشروع الحالي بأعمدتها:
  النوع، المعنى، المصدر، الثقة، الملاحظة، خريطة القيم، عينة قيم، وحالة التحليل لكل جدول
  `analyzed|rule|none`)، `PUT /api/semantics/{table}/{column}`، `POST /api/semantics/{table}/analyze`.
  في app.js: `TPLADM_KINDS.semantics` + `tpladmSemanticsPane()` (تحميل كسول عند فتح التبويب،
  بحث، فلتر «غير محلَّلة/فيها أعمدة بلا معنى»، تصحيح فوري من قائمة المفهوم، إعادة تحليل جدول،
  إبطال `dashGalFeasibility`). نطاقها المشروع الحالي للمستخدم لا كل المنظمات (بخلاف باقي التبويبات).
  تحقق حي على gpt-4.1: 9 أعمدة محلَّلة، تصحيح يُحفظ كـ user ويصمد بعد إعادة التحليل، بلا تمرير
  أفقي على 390px.
- **صفحة «مصادر المشروع»** (`renderSourcesPage`): كل بطاقة ملف فيها شارة حالة التحليل وعدد الأعمدة
  وزر «🏷️ معاني الأعمدة» يفتح جدول المعاني داخل البطاقة (نفس `semTableHtml`/`bindSemTable`/
  `semReanalyze` المشتركة مع تبويب الإدارة) مع التصحيح الفوري وإعادة التحليل — فمالك المنظمة أو
  المشروع يصحّح من مكانه بلا صفحة إدارة. البيانات من `/api/semantics` تُحمَّل مع الصفحة وتُربط
  بالملف عبر `fileId`؛ البطاقات المفتوحة تبقى مفتوحة عبر إعادة الرسم (`sourcesSemOpen`).
- **التحليل الآلي تلقائي لكل جدول** (`ColumnSemanticsService`): `RunAnalysisAsync` مشغّل خلفي واحد
  بمنع تكرار (`_inFlight` بمفتاح المشروع|الجدول) وحد توازٍ 3 (`_gate`) يستخدمه الرفع (`AnalyzeInBackground`)
  وتحديث نظام خارجي (`SourcesController.Refresh`) و`EnsureAnalyzedAsync(projectId, tables, budget)`:
  تحلّل كل جدول بلا صف من الموديل/المستخدم (ملف قديم، جدول نظام، أو نداء فشل) وتنتظر حتى المهلة
  (البناء المرحلي 45 ثانية، `BuildOneAsync` 30) ثم يستمر الباقي في الخلفية. `GET /api/semantics` يشغّلها
  بمهلة صفر ويعيد `analysis: "pending"` فتظهر «⏳ قيد التحليل الآلي» في «المصادر» وتبويب الإدارة، مع
  إعادة تحميل واحدة بعد 8 ثوانٍ (`semScheduleRefresh`). تحقق حي (gpt-4.1): الرفع → محلَّل في 4 ثوانٍ؛
  حذف الصفوف → القائمة تعيد pending ثم analyzed في 5 ثوانٍ؛ بناء قالب على جدول بلا معانٍ يُسبقه نداء
  `[معاني]` قبل التخطيط؛ صفحة المصادر تعرض «قيد التحليل» ثم «✓ محلَّل» بلا أخطاء JS.
- **فشل نداء الموديل مرئي لا صامت**: `AnalyzeTableAsync` تحفظ آخر سبب فشل لكل جدول في الذاكرة
  (`GetFailure`؛ مثل «Ollama API token is not configured» أو تعذّر الاتصال بالبوابة) — وبدونها كان الملف
  المرفوع يظهر «تخمين من الأسماء فقط» بلا تفسير. `GET /api/semantics` يعيد `analysis: "failed"` +
  `analysisError`، والواجهة تعرض «! فشل التحليل الآلي» وصندوق `.sem-error` بالسبب وإرشاد للإعدادات ثم
  «إعادة التحليل الآلي»؛ و`POST …/analyze` يعيد `analysisError` فتقول الحالة «تعذّر…» لا «اكتمل». التشغيل
  التلقائي لا يعيد المحاولة قبل 10 دقائق من آخر فشل (`RetryAfter`)؛ الزر اليدوي يعيدها فورًا.
  تحقق حي بمزوّد Ollama بلا مفتاح: الرفع → «فشل التحليل الآلي» بالسبب على البطاقة وفي حالة الزر، بلا
  أخطاء JS وبلا تمرير أفقي على 390px.
- **(ب) ✅ الواجهة** (app.js، قسم «طبقة المعنى (ب)»): زر 🏷️ «معاني الأعمدة» في صف كل ملف
  جدولي في «المصادر» يفتح `file-sem-modal` (index.html): صف لكل عمود باسمه ودوره، قائمة مفاهيم
  مجمّعة بالدور من `concepts` في رد `GET …/semantics`، وشارة المصدر/الثقة (تخمين من الاسم /
  تحليل آلي · 95٪ / ✅ مؤكّد منك)، والملاحظة وخريطة القيم. تغيير المفهوم يحفظ فورًا عبر
  `PUT …/semantics/{column}`؛ زر «إعادة التحليل الآلي» يستدعي `POST …/analyze`. في المعرض:
  `loadDashGalFeasibility()` تجلب `/api/templates/feasibility` مرة (تُبطَل بـ
  `dashGalFeasibility = null` عند تغيّر الملفات أو المعاني) وتعرض على كل بطاقة غير تفاعلية
  شارة «✓/◐/! 15 من 28 ببياناتك» (أيقونة+نص؛ ≥80٪ ok، ≥40٪ warn، وإلا danger)، وفي
  المعاينة أكثر المفاهيم نقصًا مع إرشاد إلى «المصادر». لا شارة قبل تحليل أي جدول
  (`buildable` null). تحقق حي بـ Playwright على gpt-4.1: بلا أخطاء، وتأكيد
  Category=sector رفع sales-exec من 8 إلى 9 من 10، وبلا تمرير أفقي على 390px.

- **تصحيح الأعمدة ورسالة الملخص** (بعد المراحل الثلاث):
  - بعد بناء قالب، `pushMissingConceptsMessage` في app.js تضيف رسالة واحدة في المحادثة بعدد
    العناصر المحسومة حتميًا («يحتاج «X»…») وأكثر المفاهيم نقصًا، برابط يفتح نافذة معاني الأعمدة
    للجدول الذي اعتمدت عليه اللوحة (`openSemModalForTable`). الرسائل تقبل `m.html` لمحتوى
    نبنيه نحن فقط (لا من الموديل).
  - بند «🧩 تصحيح الأعمدة» في قائمة العنصر ⋮ (لعنصر له `query.sql` أو `noData`) يفتح
    `widget-cols-modal`: (أ) الأعمدة التي يستخدمها الاستعلام من جدوله مع بديل لكل منها ←
    `POST /api/widgets/columns/remap` يعيد كتابة المعرّفات (`Widgets/SqlColumnRewriter.cs`: لا داخل
    النصوص المقتبسة ولا أسماء الدوال ولا الاسم المستعار بعد AS) وينفّذ عبر `ExecuteSqlFilterAsync`
    ويعيد `renamedKeys` لتحديث xKey/yKey؛ خيار «learn» ينقل معنى العمود القديم للجديد **للمشروع
    فقط** (بلا سابقة عابرة للعملاء). (ب) المفاهيم التي يحتاجها العنصر (`WidgetRequirementResolver`)
    وحالتها في كل جداول المشروع مع اختيار عمود لأي مفهوم ← `PUT …/semantics/{column}`.
    (ج) لعنصر `noData` في لوحة لها `templateKey` (حقل جديد في `DashboardSpec` يضبطه
    `ChatController` عند البناء من قالب): «إعادة بناء هذا العنصر الآن» ←
    `POST /api/widgets/columns/rebuild` → `StagedTemplateBuilder.BuildOneAsync` (جدوى ← تخطيط ←
    بناء لعنصر واحد). `inspect` في `Controllers/WidgetColumnsController.cs`.
  - المخطِّط يتلقّى الآن لكل عنصر «الأعمدة المؤكدة لمفاهيم هذا العنصر: القطاع ← Category»
    (`WithConceptHints`) فلا يرفض عنصرًا لأن اسم العمود لا يطابق المفهوم حرفيًا.
  - تحقق حي (gpt-4.1، Playwright): رسالة الملخص بـ13 عنصرًا، استبدال Revenue→Quantity أعاد تنفيذ
    الاستعلام بلا موديل، وتأكيد مفهوم ناقص ثم إعادة بناء العنصر في 5 ثوانٍ، بلا أخطاء JS.
  - **علّة أُصلحت**: `templateKey` كان يضيع عند إعادة فتح اللوحة من السجل فلا يظهر زر إعادة
    البناء (المستخدم يحدد العمود ولا يحدث شيء). الآن يُحفظ في `HistoryEntry.TemplateId` للوحات
    المحادثة (`SaveHistoryRequest.templateId`) ويُستعاد في `openHistoryEntry`. و`rebuild` يعمل
    بلا مفتاح قالب (عنصر مفرد من عنوانه ونوعه). وبعد تحديد عمود لآخر مفهوم ضروري ناقص يُعاد البناء
    تلقائيًا. تحقق حي: إعادة فتح من السجل تحتفظ بالمفتاح، وتحديد عمود لعنصر «دورة البيع»
    أعاد بناءه تلقائيًا في 4.5 ثانية.

## إعدادات الموديلات: موديل تحليل المعاني المستقل + صفحة «مفاتيح الموديلات»

- **نموذج تحليل معاني الأعمدة** (قسم في «الإعدادات»): `LlmSettingsStore.GetSemanticsAsync/SetSemanticsAsync`
  (أعمدة `SemanticsProvider/SemanticsOllamaModel/SemanticsOpenAiModel`)، `PUT /api/llm-settings/semantics-analyzer`.
  null = نفس موديل اللوحات، `"none"` = معطَّل (تخمين الأسماء فقط، ويظهر كسبب فشل على البطاقة)، وإلا مزوّد
  مستقل بموديله. `LlmRouter.ResolveSemanticsStagedAsync` يعيد `(IStagedLlm, model?)` و`StagedCall.ModelOverride`
  (حقل اختياري جديد) يمرّر الموديل للنداء الواحد في المزوّدين الثلاثة (Claude عبر `RunLoopAsync(model:)`).
- **صفحة «مفاتيح الموديلات»** (`screen-llm-keys`، في قائمة الإدارة بعد «الإعدادات»): `Llm/LlmCredentialStore.cs`
  جدول `LlmCredentials(Provider, ApiKey, BaseUrl, UpdatedAt)` — المفتاح مشفّر بـ Data Protection (مجلد
  `dp-keys/` بجوار التطبيق، في .gitignore؛ فقدانه يجعل المفاتيح المحفوظة «لا تُقرأ» ويُطلب إعادة إدخالها).
  القاعدة تغلب الإعدادات (`OpenAI:ApiKey`…) حقلًا بحقل، والافتراضي للعناوين في `DefaultBaseUrl`.
  **المزوّدون الثلاثة لم يعودوا يقرؤون المفتاح في المنشئ** بل عند كل نداء (`RequireApiKeyAsync` برسالة تشير
  إلى الصفحة، و`BaseUriAsync` يبني عنوانًا مطلقًا فيغلب `BaseAddress`). API:
  `GET /api/llm-settings/credentials` (المصدر db/config/none/unreadable + آخر 4 أحرف فقط)،
  `PUT/DELETE …/credentials/{provider}`، `POST …/credentials/{provider}/test` (قائمة الموديلات بالمفتاح
  الفعلي). `providers[].configured` وقائمة موديلات Ollama تعتمدان على المخزن لا على الإعدادات.
  **المفتاح لا يُرجَع للواجهة أبدًا ولا يُكتب في المستودع.** تحقق حي بلا أي مفتاح في الإعدادات: حفظ
  مفتاح خطأ → اختبار 401 واضح؛ المفتاح الصحيح → «متصل — 141 موديل»؛ عنوان غير صالح → 400 برسالة؛
  رفع ملف → نداء `[معاني]` على gpt-4o (موديل المعاني) واقتراح الاسم على gpt-4.1 (موديل اللوحات).

## العروض التقديمية (صفحات HTML مرفوعة برابط عام)

- `Presentations/` (`Presentation`، `PresentationStore` جدول `Presentations` بالمحتوى كاملًا في القاعدة) +
  `Controllers/PresentationsController.cs` (`/api/presentations`): رفع ملف `.html` واحد (≤15MB، multipart) بعنوان
  وانتهاء اختياري، قائمة **على مستوى المنظمة** (كل مستخدمي المنظمة يرونها؛ الإلغاء/الاستبدال/الحذف لصاحبه أو
  المسؤول — `canManage`)، `PUT {id}/file` استبدال بنسخة أحدث على نفس الرابط، `PUT {id}/expiry` (إعادة تفعيل)،
  `POST {id}/revoke`، `DELETE`. المسارات العامة بلا دخول: `GET {id}/meta` (410 مع السبب عند الإلغاء/الانتهاء)
  و`GET {id}/content` (يزيد المشاهدات).
- **رابط مقروء** `/p/{اسم-المنظمة}/{اسم-العرض}`: عمود `Slug` (فريد داخل المنظمة، يُفرض في الكود عبر `SlugTakenAsync`؛
  `PresentationSlug.Normalize`: حروف بأي لغة وأرقام وشرطات، صغيرة، 2–80 حرفًا) يُشتق من العنوان إن لم يُدخله المستخدم،
  والتعارض يعيد 409 مع `suggestion` (`q3-results-2`) تملؤه الواجهة تلقائيًا. `GET slug-available` للتحقق الحي أثناء
  الكتابة، `PUT {id}/slug` لإعادة التسمية (الرابط القديم بالاسم يتوقف، والرابط بالمعرّف يبقى). المسارات العامة بالاسم:
  `GET by/{org}/{slug}/meta|content` ومسار الصفحة `/p/{org}/{slug}` في Program.cs؛ `presentation.html` يميّز بين
  مقطع واحد (معرّف) ومقطعين. الروابط تُعاد بلا ترميز (`PublicPath`) ليبقى العربي مقروءًا عند النسخ؛ القائمة تعيد
  `{orgSlug, items[].url}`. أداة `publish_presentation` في الجسر تقبل `slug` اختياريًا.
- **العزل**: الرابط العام `/p/{id}` (في Program.cs) يخدم `wwwroot/presentation.html` — شريط عنوان + iframe
  بسمة `sandbox` **بلا `allow-same-origin`** (أصل معتم: لا كوكي ولا API بصلاحيات المشاهد)، ومسار المحتوى يرسل
  `Content-Security-Policy: sandbox …` + `nosniff` + `no-referrer` + `no-store` احتياطًا لمن يفتحه مباشرة.
  تحقق حي: صفحة مرفوعة تحاول `document.cookie` و`fetch('/api/…')` من داخل الإطار → `cookie-err | api-blocked`
  حتى مع مشاهد مسجَّل دخوله.
- الواجهة: شاشة مستقلة `screen-presentations` ببند «العروض التقديمية» في القائمة الجانبية تحت «المنظمة» (مع شارة
  العدد `presentations-badge`) — كانت في البداية قسمًا داخل «روابط المشاركة» التي لا تظهر إلا كأيقونة في الشريط
  العلوي فلم يجدها المستخدم. `loadPresentations`، نافذة `presentation-modal` للرفع والاستبدال، نسخ الرابط `/p/{id}`،
  فتح، إلغاء/إعادة تفعيل/حذف، وزر «🔑 ربط Claude». بلا تمرير أفقي على 390px للقائمة وللعارض.

## بيانات العرض: أجزاء قابلة للتعديل في صفحات HTML المرفوعة (المرحلة الأولى)

- **الهدف**: صفحة العرض تبقى ثابتة وبياناتها القابلة للتعديل (مهام، جداول…) في القاعدة بصلاحيات من المنصة —
  بدل إعادة نشر الصفحة كلها أو تنزيل نسخة (ما كانت تفعله صفحة المستخدم عبر واجهة Claude artifact).
- **المخزن** `Presentations/PresentationDataStore.cs`: جدول `dbo.PresentationData(PresentationId, Key, Json, Version,
  UpdatedAt, UpdatedBy)`؛ مفتاح `[\w.-]{1,100}`، قيمة ≤256KB، ≤50 مفتاحًا للعرض؛ `SetAsync` بإصدار متوقع (null = بلا
  تحقق) ويعيد تعارضًا عند عدم التطابق، والتحديث مشروط بـ`Version = current` ضد السباق. تُحذف مع العرض.
- **السياسة** `Presentation.EditPolicy` (عمود جديد، افتراضي `anyone` بقرار المستخدم): `anyone` = أي من يفتح الرابط حتى
  بلا دخول، `members` = مسجَّل من نفس المنظمة، `owner` = صاحبه/المسؤول. `CanWriteData` (internal، مختبَرة).
  قائمة العروض فيها قائمة منسدلة لتغييرها (`PUT {id}/edit-policy`).
- **API عامة** (`[AllowAnonymous]`، بالمعرّف أو `by/{org}/{slug}`): `GET …/access` (signedIn/canEdit/editPolicy)،
  `GET …/data` (المفاتيح)، `GET …/data/{key}` (مفقود → `value:null, version:0`)، `PUT …/data/{key}` `{value, version?}`
  (401 بلا دخول حين تلزم، 403، 409 تعارض مع `version` الحالي)، `DELETE …/data/{key}` لصاحبه.
- **الجسر**: الصفحة داخل iframe معزول (أصل معتم) فلا تنادي الـ API مباشرة. `wwwroot/js/jeem-page.js` (تضمّنه الصفحة
  بـ `<script src="/js/jeem-page.js">`) يرسل `{jeem:1,id,type,payload}` بـ postMessage إلى `presentation.html`
  (`installBridge`: يتحقق أن `event.source === frame.contentWindow`، ينفّذ على الـ API بكوكي المشاهد، ويرد بنفس
  المعرّف). الواجهة: `jeem.store.get/set/list/getWithMeta`، `jeem.viewer()`، `jeem.onChange(key, fn)` (استطلاع 20 ث).
  `set` يرسل آخر إصدار قرأته تلقائيًا (`{force:true}` يتجاوز)؛ الأخطاء بـ `code`: conflict/sign_in_required/forbidden/
  bad_key. خارج المنصة يعمل على localStorage بنفس العقد (للتأليف). الدليل للمؤلفين `docs/presentations-data.md`
  (يُخدم على `/docs/presentations-data.md`) ومقتطف في نافذة الرفع.
- **تحقق** (بلا SQL Server: خادم Node وهمي يحاكي الـ API + صفحة المشاهدة الحقيقية + الجسر الحقيقي في Playwright):
  get/set/الإصدار/409/list/bad_key، `fetch` مباشر من داخل الإطار محجوب، سياسة members بلا دخول → 401 والقراءة تعمل؛
  وصفحة المستخدم («دور الإدارة في مشروع الخوارزمي») بعد تكييف تبويب المهام: حفظ ← API v1 ← إعادة تحميل تحتفظ ←
  تعارض يعرض الرسالة. اختبارات وحدة للسياسات والمفاتيح (146/146). **المرحلتان 2 و3** (وضع التحديد + الأنماط الجاهزة،
  ثم التحويل بالـ AI) لم تبدآ.

## ربط Claude Desktop: مفاتيح الوصول الشخصية + جسر MCP (`tools/jeem-mcp/`)

- **مفاتيح الوصول** (`Users/ApiTokenStore.cs`، جدول `ApiTokens`): `jeem_<48 hex>`، يُخزَّن SHA-256 فقط + بادئة 12 حرفًا
  للعرض؛ يُعرض كاملًا مرة واحدة من `POST /api/tokens`. `GET /api/tokens` قائمة، `DELETE {id}` إلغاء،
  `GET /api/tokens/whoami` للتحقق (يعمل بالكوكي أو بالمفتاح). الإنشاء والإلغاء مرفوضان (403) لطلب مصادَق بمفتاح
  حتى لا يولّد مفتاح مسرَّب مفاتيح أخرى.
- **المصادقة** (`Users/ApiTokenAuthenticationHandler.cs`): المخطط الافتراضي صار سياسة `CookieOrApiToken` في Program.cs
  تحوّل الطلب إلى مخطط `ApiToken` عند ترويسة `Authorization: Bearer jeem_…` وإلا إلى الكوكي. المعالج يبني **نفس
  مطالبات** دخول الكوكي (+ `AuthVia=ApiToken`) فكل المتحكمات تعمل بلا تغيير وبصلاحيات صاحب المفتاح. الدخول/الخروج
  ما زالا يسمّيان مخطط الكوكي صراحة.
- **الواجهة**: زر «🔑 ربط Claude» في شاشة «العروض التقديمية» يفتح `api-token-modal`: إنشاء باسم، عرض المفتاح مرة واحدة مع
  مقطع `claude_desktop_config.json` جاهز (`apiTokenSnippet`: عنوان المنصة الحالي + المفتاح)، قائمة بالبادئات وآخر
  استخدام، إلغاء.
- **الجسر** `tools/jeem-mcp/index.js` (Node ≥20، `@modelcontextprotocol/sdk` عبر stdio؛ `node_modules/` في .gitignore):
  أدوات `publish_presentation(title, html|filePath, slug?, expiresInDays?)` → multipart إلى `/api/presentations` ويعيد الرابط المقروء،
  `replace_presentation`، `list_presentations`، `revoke_presentation`، `whoami`. البيئة `JEEM_URL` + `JEEM_TOKEN`.
  القيد: المنصة داخلية فلا Custom Connector من claude.ai؛ الجسر يعمل من **Claude Desktop فقط** على جهاز داخل الشبكة
  (التركيب في `tools/jeem-mcp/README.md`). لو أُتيح نطاق عام لاحقًا تتحول الأدوات نفسها إلى خادم MCP بعيد.
- **`filePath` بدل `html`** في النشر والاستبدال (`resolveHtml` في index.js): الجسر محلي فيقرأ ملف `.html` من جهاز
  المستخدم (مسار مطلق أو `~`، ≤15MB) ويرسله — أُضيف لأن Claude كان يضطر إلى إعادة كتابة صفحة 1.1MB حرفًا بحرف في كل
  تحديث. تحقق حي: نشر واستبدال ملف 1.15MB من المسار، ورسائل واضحة لملف مفقود أو بلا أي مصدر.
- تحقق حي: إنشاء مفتاح → whoami بالـBearer 200 بلا كوكي، إنشاء مفتاح بمفتاح 403، مفتاح مُلغى/خاطئ 401؛ عميل MCP
  stdio حقيقي ضد الجسر: نشر → رابط عام 200، استبدال يغيّر المحتوى على نفس الرابط، قائمة، إلغاء → 410، HTML أقصر
  من الحد يُرفض من المخطط؛ النافذة بلا أخطاء JS وبلا تمرير أفقي على 390px.

## محادثة Claude (Claude Code CLI على الخادم) — `ClaudeChat/`

- **الفكرة**: شاشة `screen-claude-chat` (بند «محادثة Claude» تحت «المنظمة»، يظهر فقط لمن تسمح له
  `ClaudeCli:AllowedRoles`، افتراضيًا Admin): شريط محادثة يسارًا بعرض الشات المعتاد + لوحة نتائج يمينًا تعرض
  الرد الكامل منسّقًا من Markdown (`ccMarkdown` في app.js: محوّل صغير يهرّب النص كله أولًا؛ عناوين/قوائم/كود مع زر
  نسخ/جداول/اقتباسات/روابط). الشريط يعرض رسائل المستخدم وردودًا مختصرة؛ الضغط على رد يعرضه في اللوحة.
- **الخادم** `ClaudeCliService`: يشغّل `claude -p --output-format stream-json --verbose --include-partial-messages
  --tools "" --permission-prompts none --no-chrome` (محادثة فقط، بلا أدوات ولا ملفات)، الرسالة عبر stdin، ومعرّف
  الجلسة نختاره (`--session-id`) ثم `--resume` للمتابعة فتبقى الذاكرة لدى الـ CLI. يزيل `CLAUDECODE` من البيئة.
  يبث `init/delta/done/error`؛ مهلة `ClaudeCli:TimeoutSeconds` (600) وقتل العملية عند إلغاء الطلب.
  `ClaudeChatStore`: جدولا `ClaudeChats`/`ClaudeChatMessages` (لكل مستخدم محادثاته فقط) مع التكلفة والمدة من حدث
  `result`. `ClaudeChatController` (`/api/claude-chat`): `status` (مفعّل؟ مسموح؟ الأمر متاح؟ بالإصدار)، `chats`،
  `chats/{id}/messages`، `PUT/DELETE chats/{id}`، و`POST chats/{id}/send` بـ Server-Sent Events (`id=new` ينشئ
  محادثة ويعيد `chat` أولًا). الإعدادات `ClaudeCli` في appsettings (Command/WorkingDirectory/Model/AllowedRoles).
  **الأمر `claude` يجب أن يكون مثبّتًا ومسجَّل دخوله على الخادم**، والتكلفة على حساب Claude الخاص بالخادم.
  **Windows**: `Process.Start` بلا shell لا يكمل الامتداد، فالاسم «claude» وحده يفشل (علّة حقيقية ظهرت للمستخدم).
  `ResolvedCommand` يبحث في PATH بكل PATHEXT ثم `%USERPROFILE%\.local\bin` (المثبّت الأصلي `claude.exe`) و`%APPDATA%\npm`
  (`claude.cmd`)، ورسالة التعذّر تذكر أين بحث وكيف يُضبط المسار الكامل في `ClaudeCli:Command`؛ `status` يعيد `command` المحلول.
- **أدوات القراءة عبر MCP (الخطوات 1–4 من `docs/plans/claude-chat-mcp-tools.md`)** — `ClaudeChat/Mcp/`، منطق مستقل
  بالكامل عن `AnalyticsTools`/`WidgetQueryService`/البنّاء (قيد الخطة):
  - خادم MCP بالحزمة الرسمية `ModelContextProtocol.AspNetCore` 2.3.0 على `/api/claude-chat/mcp` (Streamable HTTP،
    `Stateless = true`، `MapMcp(...).AllowAnonymous()`)، يحرسه `McpTokenMiddleware` (`UseMcpSessionGate` قبل
    `UseAuthentication`): `Authorization: Bearer mcp_…` لمفتاح صالح وإلا 401، والجلسة تُوضع في `HttpContext.Items`.
  - `McpSessionTokens` في الذاكرة: `Issue(userId, projectId, عمر)` → `mcp_<48 hex>`، `Resolve` يرفض المنتهي/المجهول،
    `Revoke` عند انتهاء الرسالة، وعدّاد استعلامات لكل جلسة (`McpSession.CountQuery`).
  - `JeemMcpTools` (`[McpServerToolType]`): `list_tables` (من `dbo.repo_Files` حيث `ProjectId` = مشروع المستخدم وضمن
    مخطط `proj_<id>` فقط + `dbo.sys_*` للأنظمة المفعَّلة الموجودة فعلًا)، `describe_table` (INFORMATION_SCHEMA +
    `dbo.ColumnSemantics` بالاسم الكامل أو المجرد + عينة 5 صفوف؛ يرفض ما ليس في القائمة)، `run_sql` (المدقّق ثم تنفيذ
    بمهلة `QueryTimeoutSeconds` وقراءة حتى `MaxRows`+1 لتحديد `truncated`). سقف `MaxQueriesPerMessage` يعيد
    «توقف وأجب بما لديك». الأخطاء نص يبدأ بـ«خطأ:» ليقرأه الموديل.
  - `ReadOnlySqlValidator` (ساكن، مختبَر): يزيل التعليقات ويفرّغ النصوص المقتبسة قبل الفحص؛ جملة واحدة تبدأ بـ
    SELECT/WITH؛ كلمات ممنوعة بحدود كلمات + بادئات `xp_/sp_/fn_`؛ كل اسم بعد FROM/JOIN/APPLY وفي قائمة FROM يجب أن
    يطابق `schema.name` مسموحًا (أو اسمًا مجردًا فريدًا؛ أسماء CTE مسموحة)؛ `EnsureTop` يُدرج `TOP (n)` بعد SELECT
    الرئيسي (بعد DISTINCT/ALL) ما لم يوجد TOP أو OFFSET. 
  - `ClaudeCliService.SendAsync(…, userId, projectId)`: مع `ClaudeCli:ToolsEnabled` (true) يصدر مفتاح جلسة ويكتب
    `mcp-config` مؤقتًا (`{"mcpServers":{"jeem":{"type":"http","url":…,"headers":{"Authorization":…}}}}`) ويشغّل الـ CLI
    بـ `--tools ""` (المدمجة معطَّلة دائمًا) + `--mcp-config <file> --strict-mcp-config --allowedTools mcp__jeem__*
    --append-system-prompt <ToolsSystemPrompt>`، ويحذف الملف ويلغي المفتاح في `finally`. عنوان الخادم من
    `ClaudeCli:McpBaseUrl` أو أول عنوان HTTP يستمع إليه الخادم (`IServerAddressesFeature`، 0.0.0.0→127.0.0.1).
    المتحكم يمرّر `GetEffectiveSelectionAsync(user).ProjectId`؛ `status` يعيد `toolsEnabled` و`mcpUrl`.
  - اختبارات `ClaudeChatMcpTests.cs` (21): المفاتيح (إصدار/رفض/انتهاء بـ FakeTimeProvider/إلغاء) والمدقّق
    (UPDATE/DELETE/INSERT/DROP/INTO/xp_/OPENROWSET/WAITFOR مرفوضة، جمل متعددة مرفوضة، جدول خارج المشروع و
    `dbo.repo_Files`/`INFORMATION_SCHEMA`/`sys.` مرفوضة، WITH مقبول، TOP يُفرض ولا يُكرَّر ولا مع OFFSET، الكلمات
    داخل النصوص/التعليقات لا تُرفض) ومطابقة الأسماء في الكتالوج، وملخص نداء الأداة. 137/137.
  - **الخطوة 5 (الواجهة)**: رسائل `assistant` في stream-json تحمل كتل `tool_use`؛ الخدمة تبث `StreamEvent("tool")`
    بـ `SummarizeToolUse` (الاسم بلا `mcp__jeem__` + أهم مدخل: SQL مختصر 160 حرفًا أو اسم الجدول)، والمتحكم يكتب حدث
    SSE `tool`، وapp.js (`ccShowToolActivity`) يعرض «🔎 يستعلم عن `SQL`» في الشريط أثناء العمل ثم يطويه بعد الرد في
    `<details>` «n عمليات على البيانات». الخطوة 6 (الإعدادات) أُنجزت ضمن الخطوة 4.
  - **ما تحقق هنا بلا SQL Server**: الـ CLI يقبل ملف `mcp-config` بالشكل المكتوب (`type: http` + `headers`) مع
    `--strict-mcp-config --allowedTools mcp__jeem__*`: حدث `init` يعرض `mcp_servers: [{name: jeem}]` (status failed
    لأن الخادم غير شغّال هنا). **التحقق الحي الكامل (الخطوة 8) لم يُجرَ** — على جهاز المستخدم.
- تحقق حي ضد CLI حقيقي (2.1.312): غير المسؤول لا يرى البند و`status.allowed=false`؛ المسؤول: بث يبدأ فورًا، رد
  بعنوان وقائمة وكود مُنسَّق في 3.6 ثانية مع المدة والتكلفة؛ متابعة بذاكرة («كم بندًا؟» → 3)؛ القائمة والفتح
  بعد إعادة التحميل؛ بلا أخطاء JS وبلا تمرير أفقي على 390px.

## رسوم لوحات المحادثة: DevExtreme بدل ApexCharts (`wwwroot/js/jeem-dx.js`)

- **النطاق**: لوحات المحادثة فقط (bar/line/pie/multi-series/forecast/radial-gauge). اللوحات
  التفاعلية (`dash-kit.js` + 17 محركًا) ومعاينة المعرض (`dash-render.js`) لا تستخدم المكتبة ولم تُمس.
  باقي الأنواع (kpi، table، progress-table، stages، bullet…) HTML/SVG مخصص كما كانت.
- **الحزمة**: `wwwroot/lib/devextreme/dx.viz.jeem.js` مبنية بـ esbuild من `devextreme@26.1.5`
  (مكوّنات viz فقط، ~920KB بدل dx.all.js 4.7MB) — وصفة البناء في `lib/devextreme/build/`
  والشرح في `lib/devextreme/README.md`. ملفات ApexCharts القديمة (`lib/apexcharts.min.js`،
  `vendor/apexcharts.min.js`، `js/jeem-charts.js`) ودالة `applyLollipopStyle` **حُذفت** بطلب المستخدم.
- **الترخيص**: لكل مطوّر. المفتاح من `DevExtreme:LicenseKey` (user-secrets / `DevExtreme__LicenseKey`)
  يحقنه `Controllers/UiConfigController.cs` عبر `GET /api/ui-config.js` → `window.JEEM_UI_CONFIG`
  قبل تحميل الحزمة؛ `jeem-dx.js` يستدعي `DevExpress.config({licenseKey})`. بلا مفتاح: نسخة
  تجريبية بشريط تنبيه أعلى الصفحة (`.dx-license`). **لا يُكتب المفتاح في المستودع أبدًا.**
- **العقد في app.js**: `chartConfig`/`forecastChartConfig`/`buildMultiSeriesChart`/`radialGaugeConfig`
  تعيد **مواصفة محايدة** `{kind: bar|line|pie|multi|forecast|radial, …, onPointClick?}` تُصطفّ في
  `pendingCharts`، وحلقة الرسم في `renderDashboard` تستدعي `JEEM_DX.mount(holder, spec, {theme: THEME, fmt})`
  التي تعيد مقبضًا `{instance, rendered, svg(), freeze(), destroy()}`. الأسماء `card.__apexChart/
  __apexHolder/__apexRendered` أُبقيت لأن تصدير PPTX يقرأها (`widgetToPptxInput` يستدعي `freeze()`
  ثم يلتقط SVG). `attachCrossFilterClick` يضبط `spec.onPointClick(idx)` للتصفية المتقاطعة.
- **القرارات البصرية**: أعمدة الفئات أفقية (`rotated`) مع قيمة عند طرف كل شريط، والسلاسل الزمنية
  (`looksTemporal`) رأسية؛ الخط spline بعلامات مفرغة؛ الدائري حلقة بإجمالي + تسمية في المنتصف
  (`centerTemplate`) وشرح جانبي بالنسب؛ المكدّس تسميات داخلية بيضاء؛ التنبؤ خط متقطع بحدّي ثقة منقّطين؛
  المقياس الدائري حلقة doughnut بشريحتين. `rtlEnabled` من `dir`، والحركة تُعطَّل مع
  `prefers-reduced-motion`. التخطيط الأكثر كثافة في ذيل `jeem.css` (ملخص أصغر، بطاقات مؤشرات 108px،
  فجوة 12px، ارتفاع الرسم 250px).
- تحقق حي (gpt-4.1 + Playwright): لوحة «أداء المبيعات» 3 رسوم SVG بلا أخطاء، التقاط PPTX يعمل،
  وكل الأنواع الستة تُرسم بلا أخطاء، وبلا تمرير أفقي على 390px.
- **الشبكة بحسب عرض منطقة اللوحة** (container query على `#dash-pane`، ذيل jeem.css): ≥1040px أربعة
  أعمدة (رسمان جنبًا إلى جنب، 4 مؤشرات في صف)، 640–1039px عمودان وكل رسم/جدول عمود واحد، <640px عمود
  واحد. المتصفحات بلا دعم container queries تبقى على `auto-fit` القديمة.
- **التغيّر عن الفترة السابقة في المؤشرات** (`WidgetQueryService.ComputeKpiDeltaAsync`،
  `POST /api/widgets/kpi-delta`): بلا موديل — يعيد تنفيذ استعلام المؤشر نفسه على آخر 30 يومًا (حتى
  أحدث تاريخ في الجدول) وعلى الـ30 التي قبلها، على عمود التاريخ الذي يستخدمه الاستعلام أو أول عمود
  تاريخ، مع الفلاتر النشطة. null بلا عمود تاريخ أو إن لم يرجع الاستعلام صفًا واحدًا. في app.js:
  `KpiCard` يضيف حامل `.kpi-delta` لمؤشر له `query.sql`، و`loadKpiDeltas()` في نهاية
  `renderDashboard` تملؤه (▲/▼/◆ + نسبة + «آخر 30 يومًا مقابل التي قبلها (A مقابل B)») بذاكرة
  `kpiDeltaCache` بمفتاح الجدول+الاستعلام+الفلاتر. تحقق حي: 4 مؤشرات في «أداء المبيعات» (▲25٪، ▲7.9٪،
  ◆0٪×2) ورسمان في صف واحد عند عرض لوحة 920px.

- **الدفعة الثانية (كل اللازم)**:
  - **أنواع جديدة**: `funnel` (قمع تحويل بنسب من الأولى)، `treemap` (حصص هرمية، `group` اختياري)،
    `scatter` (مقياسان لكل نقطة، `xKey`/`yKey` أسماء محورين). مُعرَّفة في `DashboardSpec.AllowedWidgetTypes`،
    `WidgetShapeCatalog`، وثائق الذكاء الاصطناعي في `AnalyticsTools.BuildSystemPrompt`، وفي app.js
    (`WIDGET_TYPES`، `FunnelCard/TreemapCard/ScatterCard`، قائمة «نوع الرسم»). الحزمة أُعيد بناؤها مع
    `tree_map` (~944KB). `NormalizeType`: `bubble→scatter`، و`funnel/scatter` كما هما.
  - **خط مصغّر في المؤشر**: `KpiDeltaResult.series` = ٨ نوافذ أسبوعية تنتهي بأحدث تاريخ؛ `renderKpiDelta`
    يرسمه بـ `dxSparkline` في `.kpi-spark` بلون اتجاه التغيّر (مخفي على ≤420px وفي الطباعة).
  - **الثيم من رموز التصميم**: `tokenTheme(fallback, holder)` في jeem-dx.js يقرأ `--chart-1..4`، `--line`،
    `--ink`، `--ink-3`، `--surface`، `--font` من **حاوية الرسم** (الرموز مقيّدة بـ `.jeem-scope` لا `:root`)
    فتتبع الرسوم الصفحة دائمًا. كائن `THEMES.dark` في app.js لا يقابله أي CSS (لا يوجد وضع داكن فعلي في
    الهوية الجديدة) — كان يقلب الرسوم داكنة على بطاقات فاتحة.
  - **الطباعة** (ذيل jeem.css، `@media print`): إخفاء `aside.sidebar`/`.topbar`/زر ⋮/الخط المصغّر، شبكة
    عمودين تغلب container query، والرسوم SVG تُطبع كما هي (مُتحقَّق بـ `page.pdf`). **شريط النسخة
    التجريبية `<dx-license>` لا يُخفى** (أنماط مضمّنة مفروضة) ويزول فقط بتركيب المفتاح.
  - غير منفَّذ عمدًا: معاينة المعرض تبقى SVG خفيفًا (`dash-render.js`) لأداء 200+ بطاقة؛ تحديث البيانات
    في مكانها بحركة (الرسوم تُبنى من جديد عند الفلترة كما كان).

## مراجعة التثبيت (بعد DevExtreme وطبقة المعنى)

مراجعة كود على النطاق `2bb0dff~1..HEAD` طلعت 16 ملاحظة؛ أُصلح 14 وقُبل 2 بوعي:
- أُصلح: `GET /api/ui-config.js` صار `[AllowAnonymous]` (كان يرجع 401 قبل الدخول فلا يُطبَّق الترخيص)؛
  اختيار المفهوم في «تصحيح الأعمدة» عند تعدد المفاهيم للمتطلب (`.wc-concept-pick`) بدل أولها؛
  `SqlColumnRewriter.Rewrite` مرور واحد بنمط مجمّع (التبادل والسلاسل لا تتداخل)؛ `ConceptPriorStore`
  مخطط مرة واحدة + إدراج آمن للتزامن + `LookupManyAsync` (كان N+1 مع DDL لكل عمود)؛ إبطال
  `kpiDeltaCache` عند تحديث الملفات؛ تضييق قواعد العنوان الإنجليزية بحدود كلمات وإزالة «قسم/عمر»
  المطلقة (تقسيم ≠ القسم)؛ حذف `RebuildLegacyAsync` غير القابل للوصول وتحديث التوثيق؛
  `WidgetFeasibilityService.Evaluate` الساكنة تعيد استخدام خريطة المفاهيم (قراءة واحدة لكل بناء)؛
  `WidgetQueryService.QuoteIdentifier` بدل نسخة اقتباس ثانية؛ `looksTemporal` بحدود كلمات
  (Marketing ≠ mar)؛ `templateKey` يُحتفظ به عبر أسئلة المتابعة؛ لون الخط المصغّر من ألوان الحالة
  المحجوزة (`--ok`/`--danger`) لا لوحة السلاسل؛ قواعد الطباعة في jeem.css فقط؛ تعليق index.html.
- مقبول بوعي: `ComputeKpiDeltaAsync` ينفّذ 11 استعلامًا لكل مؤشر (لا يمكن دمجها في GROUP BY على SQL
  عشوائي بأمان لمقاييس غير جمعية مثل AVG) — مخفَّف بذاكرة العميل؛ والبناء المرحلي مُفهرَس بعنوان
  العنصر (عنوان مكرر في قالب واحد يرث جدوى الأول) وهو قرار تصميم موثّق («العنوان ثابت»).
- اختبارات مضافة (`ReviewRegressionTests`): تبادل/سلسلة الأعمدة، حدود كلمات قواعد العنوان،
  `WithConceptHints` (صارت `internal`)، و`Evaluate` الساكنة. 114/114.
- §9 أُعيدت بعد تبديل المكتبة: 213 نموذج JR و22 تفاعلية على 1400px و390px مع تقليل الحركة — بلا
  أخطاء وبلا تمرير أفقي.

## القرارات المعمارية المتّبعة (§4.16+)

### محرك اللوحة التفاعلية (frontend)

كل لوحة من نوع `RenderKind` لها:
- `wwwroot/js/<name>.js` — `window.JEEM_<X>_MOUNT = function mount(root, data, opts = {})`
  بدل IIFE ذاتي التشغيل، يقبل `data` حقيقية أو من مكتبة العرض التجريبي.
- `wwwroot/js/<name>-demo.js` — `window.JEEM_<X>_DEMO` لمعاينة معرض النماذج
  فقط، بنفس أرقام/توزيعات النموذج الأولي لكن بشكل عقد البيانات الجديد.
- قسم CSS في `wwwroot/css/jeem.css` تحت تعليق `/* §4.22 — <name> (<key>) */`.
- في `wwwroot/js/app.js`: دالة `interactive<Name>SkeletonHtml(d)`، فرع
  `else if` في `mountInteractiveDashboard`، قيد في `DASH_GAL_RENDER_ICON`،
  فرع في `dashGalDemoAvailable`، فرع في موزّع `gal-demo-btn`، دالة
  `open<Name>GalDemo(d)`.
- قيد في `Templates/BuiltinTemplates.cs` بـ `RenderKind: "<key>"`.
- وسوم `<script>` في `wwwroot/index.html`.

**المبدأ:** مفردات تصنيف ثابتة (أعمدة كانبان، مراحل مسار مبيعات، فئات
KPI) تبقى في المحرك نفسه — مش بيانات مشروع. أي شيء تاني (أسماء، قيم،
أبعاد) يجي من `data` الممرَّرة.

### موفّر البيانات الحقيقية (backend)

كل لوحة لها `Interactive/<Name>DataProvider.cs` يطبّق `IInteractiveDataProvider`:
- `Contract`: قائمة `InteractiveTableContract`/`InteractiveColumnContract`
  تصف الجداول/الأعمدة المنطقية المطلوبة بالعربية — تُستخدم كمدخل لمطابقة
  بمساعدة الذكاء الاصطناعي عبر `InteractiveContractMatcher.MatchAsync`
  (تطابق جداول المشروع الحقيقية، وتُعيد التحقق منها مقابل الكتالوج الفعلي
  قبل الثقة بها — لا تثق أبدًا باسم رجعه النموذج فقط).
- يُسجَّل كـ singleton في `Program.cs` (قسم "§4.22 — 11-dashboard batch").
- يستخدم `Interactive/InteractiveRowHelpers.cs` المشتركة
  (`BuildSelect`/`Str`/`Num`/`Bool`/`ParseDate`/`DateStr`) بدل تكرارها.

**مبدأ التصميم المتكرر:** اشتق أكبر قدر من البيانات من أقل عدد من الجداول
الخام (سجلّ واحد على مستوى الصف غالبًا كافٍ) بدل طلب عدة جداول مُجمَّعة
مسبقًا ممكن تتعارض حسابيًا مع بعضها. مثال: `supply-command` كل اتجاهه
الهرمي (سنة←ربع←شهر) مُشتق من سلسلة شهرية واحدة؛ `saas-metrics` شلال
الدخل وتحليل الاحتفاظ كلاهما من سجلّ لقطات أسبوعية واحد.

**لا تلفيق أبدًا:** رقم خارجي بطبيعته (نسبة LTV:CAC، العائد على الإنفاق
الإعلاني، مستهدف SLA) يصير حقل مطلوب/اختياري صريح في العقد، مش معادلة
مُخترَعة لحسابه من بيانات تانية.

**تسلسل JSON:** استخدم أسماء خصائص صريحة بحروف صغيرة أول كل كلمة
(camelCase) في الـ anonymous objects دايمًا — لا تعتمد على افتراض وجود
naming policy تلقائية في السيريلايزر.

### التحقق

- `dotnet build` بعد كل موفّر/محرك.
- اختبارات وحدة مستهدفة في `backend/ChatToDashboard.Api.Tests/` للحسابات
  الأكثر عرضة للخطأ (تجميع تواريخ، رياضيات دفعات/احتفاظ، نوافذ توقع
  بالمتوسط المتحرك) — ضروري لأن **مسار المطابقة بالذكاء الاصطناعي الحي
  (رفع جداول حقيقية فعلًا واختباره) محتاج مفتاح OpenAI/Anthropic خاص
  بتطبيق جيم نفسه** (مش مفاتيح الجلسة) — تأكد منه في `appsettings.json`/
  user-secrets قبل ما تفترض إن التحقق الحي مش ممكن.
- تحقّق حيّ عبر Playwright: شغّل السيرفر محليًا **على SQL Server لا SQLite** (قرار المستخدم: لا
  تشغيل محلي على SQLite إطلاقًا؛ منذ 2026-11-02 أُزيل دعم SQLite من الكود كله: لا `DbProvider` ولا
  `DataStore.Provider`، وحزمة `Microsoft.Data.Sqlite` محذوفة من مشروع API، و`DatabaseProvider=Sqlite` يوقف الإقلاع
  برسالة. **`ClientDbProvider` في التكاملات شيء آخر**: نوع قاعدة بيانات العميل لمشروع `IntegrationConnector`
  المستقل الذي ما زال يدعم SqlServer/Sqlite عند العميل ولم يُمس) بـ
  `ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://127.0.0.1:5050 dotnet bin/Debug/net8.0/ChatToDashboard.Api.dll`.
  بيئة Development ضرورية لأن سلسلة الاتصال `ConnectionStrings:DataDb` (قاعدة `ChatToDashboard` على
  `OL-MHGOMAA\SQLEXPRESS`) ومفاتيح الموديلات في user-secrets للمشروع (`chat-to-dashboard-api`)، ولا تُقرأ في
  Production. تأكد في السجل من «database: SqlServer». سجّل دخول بمنظمة «المنظمة الافتراضية»
  (slug `المنظمة-الافتراضية`) وحساب `admin` (كلمة المرور في user-secrets تحت `Auth:SeedAdmin`)، افتح معرض
  النماذج → التصنيف المناسب → زر المعاينة التجريبية، وتأكد من الشكل وعدم وجود أخطاء جافاسكربت.
  القاعدة حقيقية فيها ملفات العميل التجريبية، فلا تحذف منها شيئًا أثناء الفحص.

## الأنظمة الخارجية في `Sources:Systems` — خاصية `Enabled`

- `SystemSource.Enabled` (افتراضيًا true). `false` = النظام مخفي تمامًا كأنه غير معرَّف: لا يُحمَّل عند الإقلاع ولا
  يُحدَّث (`refresh` يعيد 404) ولا يظهر في «المصادر» ولا يراه الموديل. كل الاستهلاك يمر من `SourceOptions.ActiveSystems`
  لا من `Systems` مباشرة — أي مستهلك جديد يستخدمها.
- «نظام المشتريات» (`procurement`، بوابة `platformtest.ahad.sa`) معطَّل حاليًا في appsettings.json بقرار المستخدم
  (2026-10-29) لأن البوابة غير متاحة؛ أعده إلى true عند الحاجة. التكامل المستقل «نظام المشتريات» في
  «التكاملات» (IntegrationConnector) شيء آخر ولم يُمس.

## حالة القاعدة الحقيقية (SQL Server) — إعادة ضبط 2026-10-29

- بطلب المستخدم حُذفت كل المنظمات وملفاتها (صفوف 26 جدول مستأجرين بـ TRUNCATE + إسقاط جداول الرفع ومخططات
  `proj_*`)، مع الاحتفاظ بمكتبة الأسماء وسوابق المفاهيم وإعدادات/مفاتيح الموديلات وتعديلات القوالب، وبكل جداول
  `dbo` (KPI/OKR/All…) لأنها ليست من التطبيق. نسخة احتياطية كاملة قبل الحذف:
  `C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\Backup\ChatToDashboard_before_reset_20261029.bak`.
- المنصة تبدأ الآن بـ«المنظمة الافتراضية» وحساب `admin` (مالك المنصّة، كلمة مروره في user-secrets تحت
  `Auth:SeedAdmin:Password`) وهو من ينشئ المنظمات من `POST /api/organizations` (شاشة «المنظمات»).
- **`Auth:SeedDemoOrganization`** (appsettings.json، افتراضيًا false): كان Program.cs يزرع «شركة النموذج التجريبي»
  بحسابي `org.admin`/`project.owner` في كل إقلاع ويعيد إنشاءها إن حُذفت. فعّله true في بيئة عرض فقط.
- درس تشغيلي: حذف `usage_Log` بـ DELETE فشل بـ«insufficient memory in the buffer pool» (184 صفًا بحجم 231MB نصوصًا)
  على جهاز بذاكرة شبه معدومة؛ `TRUNCATE` لكل جدول في دفعة مستقلة هو الطريق. لا مفاتيح أجنبية في القاعدة.

## مخطط القاعدة: `dbo` بدل `staging` (2026-11-02)

- بقرار المستخدم انتقلت كل جداول التطبيق (31 جدولًا) من مخطط `staging` إلى `dbo` بـ `ALTER SCHEMA dbo TRANSFER` مع
  حفظ البيانات، وحُذف مخطط `staging`. الكود: `DataStore.SqlServerSchema = "dbo"` وكل الأسماء الثابتة `[dbo].[X]`
  و`OBJECT_ID('dbo.X')`. مخططات المشاريع `proj_<id>` كما هي. نسخة احتياطية قبل النقل:
  `ChatToDashboard_before_dbo_move_20261102.bak` في مجلد Backup بتاع SQLEXPRESS.
- تبعة أمنية واعية: الموديل يرى كل أعمدة `dbo` (`GetSchemaAsync` بـ `@shared = dbo`)، فأي جدول غريب يوضع في `dbo`
  يصير مرئيًا للذكاء الاصطناعي. القاعدة الآن للتطبيق وحده.
- أسماء `staging_x` في قوالب `Templates/BuiltinDashboards*.cs` والاختبارات أسماء افتراضية (placeholders) من عهد
  SQLite لا أسماء جداول حقيقية، وتُطابَق على جداول العميل عند البناء؛ لم تُغيَّر.

## إزالة «مجلد البيانات» وملف العينة (2026-11-02)

- بقرار المستخدم حُذف `data/sample_sales.csv` وآلية `DataFolderLoader` (المسح والتحميل عند كل إقلاع + `DataFolderPath`
  في appsettings/Dockerfile/docker-compose/pipeline). ما بقي من الصنف صار `Data/TabularFileParser.cs`: دوال ساكنة
  فقط (`ReadCsv`/`ReadXlsx`/`InferTypes`/`SanitizeTableName`) يستخدمها رفع الملفات والأنظمة الخارجية.
- `AnalyticsTools` و`WidgetQueryService` و`DataController` تقرأ الكتالوج من `DataStore.GetSchemaAsync` مباشرة.
  `POST /api/data/refresh` صار يعيد تحميل الأنظمة الخارجية المفعَّلة فقط.
- `DocumentSearchService` (RAG، `EnableRag=false`) كان يفهرس مستندات المجلد نفسه؛ بلا مجلد لم يعد يُستدعى `Reindex`
  من أي مكان، فالصنف وأداة `search_documents` معطَّلان فعليًا — مرشّحان للحذف لاحقًا إن لم تُعد RAG على ملفات المستودع.
- لا بيانات مشتركة افتراضية الآن: كل جدول إما ملف مرفوع في `proj_<id>` أو نظام خارجي في `dbo`.

## جاهزية النشر على Azure بعد تغييرات 2026-11 (قرار المستخدم: كل شيء يعمل هناك عدا محادثة Claude)

- **ترحيل المخطط تلقائي**: `DataStore.MigrateLegacyStagingSchemaAsync` يعمل عند كل إقلاع قبل أي مخزن (Program.cs بعد
  `EnsureDatabaseExistsAsync`): ينقل كل جدول في `staging` لا نظير له في `dbo` بـ `ALTER SCHEMA TRANSFER` (البيانات
  والفهارس كما هي)، يعيد كتابة `repo_Files.TableName` ذات البادئة القديمة، ويحذف المخطط إن فرغ. جدول له نظير في `dbo`
  يُترك مع تحذير. مُتحقَّق منه محليًا بجدول تجريبي (صفّان انتقلا، المخطط حُذف، الدخول القديم يعمل). لا SQL يدوي على
  قاعدة الإنتاج مطلوب.
- **محادثة Claude على Azure**: لا Claude Code CLI في الحاوية بعد، فالـ pipeline يضبط `ClaudeCli__Enabled: "false"`
  (البند يختفي من القائمة، و`status.enabled=false`). خادم MCP الداخلي يُسجَّل لكنه بلا فائدة دون الـ CLI. تفعيلها لاحقًا =
  تثبيت الـ CLI وتسجيل دخوله داخل الصورة + إزالة هذا السطر.
- «نظام المشتريات» معطَّل في appsettings؛ لتفعيله على Azure دون تعديل الملف: متغير بيئة `Sources__Systems__0__Enabled=true`.
- **استعادة كلمة مرور مالك المنصّة على نشر قائم** (2026-11-03): الزرع في Program.cs لا يعمل إلا على قاعدة بلا حسابات،
  فأُضيف `Auth:SeedAdmin:ResetPassword` (افتراضيًا false): true عند الإقلاع يعيد كتابة كلمة مرور حساب مالك المنصّة المحلي
  الحامل لاسم `Auth:SeedAdmin:Username` من `Auth:SeedAdmin:Password` (ويفعّله)، أو **ينشئه** مالكَ منصّة في المنظمة
  الافتراضية إن لم يوجد (اسم جديد = حساب جديد)، ثم يسجّل تحذيرًا بإزالة العلم؛ بلا كلمة أو مع أكثر من حساب بالاسم لا يغيّر
  شيئًا. على Azure: `Auth__SeedAdmin__ResetPassword: "true"` **مفعَّل الآن في env الـ pipeline** (المستخدم بلا وصول للقاعدة
  هناك) مع `SEED_ADMIN_PASSWORD` التي يحلّها القالب `deploy-app.yml@platform` بـ `$(VAR)` من مجموعة المكتبة `ai-dashboard`
  أو متغيرات الـ pipeline؛ يُعاد إلى false بعد أول نشر ناجح. البديل الفوري بلا نشر: UPDATE على
  `dbo.AppUsers.PasswordHash` بتجزئة بصيغة `PasswordHasher` (`100000.<salt b64>.<hash b64>`، PBKDF2-HMACSHA256) —
  سكربت جاهز في `Claude outputs/azure-reset-admin-password.sql` (مجلد غير متتبَّع؛ لا يُرفع).
- لوحات محفوظة قديمة في `DashboardHistory` قد تحمل SQL بأسماء `staging.sys_*`؛ لا يُعاد كتابتها تلقائيًا (نادرة، والنظام
  الوحيد معطَّل).

## المستودعان: GitHub (`origin`) و Azure DevOps (`azure`) من مجلد واحد

- هذا المجلد وحده يكفي. `origin` = GitHub (فرع العمل `claude/chat-to-dashboard-app-at0up6`)، و`azure` = Azure DevOps
  (`THQ_MVPs/AI_Dashboard`). المجلد القديم `chat-to-dashboard-Azure/` لم يعد مطلوبًا للمزامنة.
- التاريخان رُبطا مرة واحدة بدمج `-s ours --allow-unrelated-histories azure/develop` (الشجرة بقيت شجرة GitHub)،
  فصار `develop` على Azure سلفًا لفرعنا وكل دفع بعده fast-forward بلا force. قبل الربط أُعيدت إصلاحات Azure الخاصة
  (ترحيل الأعمدة الناقصة backfill في 13 مخزنًا، نسخ `IntegrationConnector/` في Dockerfile، و`azure-pipelines.yml`) إلى الفرع.
- `remote.azure.push` مضبوط على `claude/chat-to-dashboard-app-at0up6:feature/newSaaS`، فالدورة:
  `git pull origin claude/chat-to-dashboard-app-at0up6` ثم `git push origin` ثم `git push azure`، وبعدها PR من
  `feature/newSaaS` إلى `develop` على Azure (الـ pipeline يعمل على `develop`).
- `frontend/node_modules` كان متتبَّعًا بالخطأ على Azure (5265 ملفًا، سبب بطء النسخ القديم) ويُحذف من هناك مع أول PR؛ وهو في
  `.gitignore` أصلًا. أي إصلاح خاص بالنشر يُكتب هنا ويُدفع للاثنين حتى لا يتباعد المستودعان ثانية.

## منشئ العروض (`Decks/`) — صفحة على مستوى المشروع تحت «المحلل الذكي»

- **القرار**: شاشة «العروض التقديمية» (مستوى المنظمة، رفع HTML جاهز) بقيت كما هي. الجديد شاشة `screen-decks`
  («منشئ العروض» في القائمة بعد «المحلل الذكي»، ضمن `PROJECT_SCOPED_SCREENS`): محادثة يسارًا ومعاينة/محرر يمينًا،
  يبني عرضًا من **مواصفة JSON منظّمة** (`DeckSpec`) لا HTML خام، فيُحرَّر يدويًا ويُصدَّر ويُنشر.
- **النموذج** `Decks/DeckModels.cs`: `DeckSpec{title,subtitle,theme,slides[]}` مع `Normalize()`؛ تخطيطات الشريحة
  `title|section|bullets|two-column|kpis|chart|table|image|tasks|quote`؛ `DeckChart{type: bar|line|pie, labels, values|series, unit}`،
  `DeckTable`، `DeckKpi`، و`DeckEditable{key,kind}` لكتلة المهام القابلة للتعديل عبر بيانات العرض. المخزن `DeckStore`
  (جدول `dbo.Decks` بالمشروع والمنشئ والمواصفة JSON و`PresentationId` إن نُشر).
- **القوالب** `DeckTemplates.cs`: 20 نموذجًا في 9 تصنيفات بمفاتيح ثابتة (`mgmt`, `sales`, `projects`, `finance`, `hr`,
  `marketing`, `ops`, `cx`, `general`؛ الأسماء والأيقونات في `DK_CATS` في app.js — لا تُعاد تسميتها) بعناوين وشرائح ونوايا
  بيانات، و`Hint` نوع الرسم أو عناوين العمودين (`أ|ب`). `GET /api/decks/templates` يعيد لكل قالب جدوى حتمية بنفس آلية
  اللوحات (`WidgetRequirementResolver.Resolve(title,intent)` + `WidgetFeasibilityService.Evaluate`) تظهر شارة «✓/◐/! n من m ببياناتك».
- **معرض «عرض جديد»** (`deck-gallery-modal` في index.html، يعيد استخدام أنماط `.dash-gallery-box`): بطلب المستخدم نفس تجربة
  معرض «لوحة جديدة» — زر «جديد» في الشريط أو بطاقة «عرض جديد» في حالة البداية يفتحانه؛ تصنيفات بعدّاد، بحث، بطاقتا
  «عرض فارغ»/«اطلب من جيم»، وبطاقة لكل نموذج مصغّرتها **شريحة حقيقية من المحرك** (أول شريحة مؤشرات/رسم/جدول)، ثم معاينة
  (قائمة الشرائح + الجدوى + «استخدم هذا النموذج») ثم «معاينة بأرقام توضيحية» تعرض العرض كاملًا بالتنقّل (`openDeckGalDemo`).
  البيانات التجريبية في `wwwroot/js/jeem-deck-demo.js` (`JEEM_DECK_DEMO.build(template)`: مصفوفة لكل مفتاح نموذج بترتيب
  شرائحه + احتياطي حسب التخطيط لأي نموذج جديد بلا بيانات) — **للمعرض فقط**، والبناء الحقيقي يبدأ فارغًا ويُملأ من
  بيانات المشروع. لوحة العرض بلا عرض مفتوح تعرض حالة بداية (`.dk-start`) + عروض المشروع المحفوظة فقط.
- **هوية المشروع** (`Decks/DeckBrandStore.cs`، جدول `dbo.DeckBrands` صف لكل مشروع): `DeckBrand{name,tagline,logo(data URL PNG ≤400KB),
  primary,secondary,accent,font(من قائمة `DeckBrand.Fonts`),footer}` مع `Validate()` (مختبَرة). `GET/PUT/DELETE /api/decks/brand`
  (التعديل للمسؤول أو مالك/محرّر المشروع عبر `ProjectStore.ListRolesAsync`؛ الباقي عرض فقط). **لا تُحفظ داخل مواصفة العرض**:
  الواجهة ترفقها `spec.brand` عند الرسم فقط (`dkSpecForSave` يحذفها قبل الحفظ/الإرسال للموديل)، والخادم يرفقها من المخزن عند
  النشر (`Publish`) وتصل مع طلب PPTX (`DeckPptxRequest.Brand`: شعار على كل شريحة + ألوان العناوين والمؤشرات + تذييل) — فأي
  تغيير يسري على كل النماذج والمعاينات والعروض الجديدة فورًا وعلى المنشورة عند إعادة نشرها. المحرك `jeem-deck.js`: متغيّرات
  `--jd-primary/--jd-secondary/--jd-accent/--jd-font` من `brandVars(spec)` (تدرّج شريحة العنوان، لون العناوين والمؤشرات، الخط)،
  `.jd-logo` (أعلى اليسار في شرائح المحتوى وفي منتصف شريحة العنوان)، والتذييل في سطر المصدر وفي `.jd-foot` لشريحة العنوان؛
  `thumbnail(slide, index, spec)` يأخذ المواصفة للألوان. الصفحة المنشورة تحمّل خط الهوية من Google Fonts (`DeckHtmlExporter`).
  الواجهة: زر «🎨 الهوية» في شريط أدوات منشئ العروض (`.dk-always` لا يُعطَّل في حالة البداية) يفتح `deck-brand-modal`: اسم،
  شعار (يُصغَّر في المتصفح إلى 512px ويُحوَّل PNG في `dkbLogoFromFile`)، 3 ألوان، خط، تذييل، ومعاينة حيّة بشريحتين،
  حفظ/إعادة الافتراضي. تحقق Playwright على الـ mock: المعاينة الحيّة، الحفظ يطبّقها على مصغّرات المعرض الـ20 والعرض التوضيحي
  والمحرر، الحفظ لا يرسل `brand`، طلب PPTX يحملها، إعادة الافتراضي تزيل الشعار، بلا تمرير أفقي على 390px.
- **خطوط الشركة** (`Decks/DeckBrandFontStore.cs`، جدول `dbo.DeckBrandFonts` بالبايتات في القاعدة): قسم «خطوط الشركة» في نافذة الهوية
  يرفع ttf/otf/woff/woff2 (≤6MB للملف، ≤12 للمشروع) عبر `POST /api/decks/brand/fonts` (multipart `files` متعددة + `family` اختياري):
  العائلة والوزن والميل تُقرأ من جدول `name` في ملفات sfnt بمحلّل صغير بلا مكتبة (`ReadSfntNames`: nameID 16/17 تغلب 1/2؛
  `WeightStyleOf` من العائلة الفرعية)، وملفات woff/woff2 تأخذ العائلة من اسم الملف. `DELETE brand/fonts/{id}` (ويعيد الهوية
  إلى الخط الافتراضي إن كانت تختار عائلة لم يبقَ لها ملف)، و`GET brand/fonts/{id}/file` **بلا دخول** مع تخزين مؤقت 30 يومًا
  (الصفحة المنشورة داخل iframe بأصل معتم بلا كوكي؛ المعرّف عشوائي 16 hex والخط ليس سرًّا). `DeckBrand.FontFaces` تُملأ من
  المخزن عند القراءة والنشر فقط (`BrandWithFontsAsync`) ولا تُخزَّن؛ `Validate(uploadedFamilies)` يقبل العائلات المرفوعة في
  `Font` إلى جانب القائمة الحرة. الحقن: الواجهة `dkInjectBrandFonts` (وسم `<style id="dk-brand-fonts-css">` بقواعد
  `@font-face` من `brand.fontFaces`/`uploadedFonts`) فتعمل في المحرر والمعرض والمعاينة الحيّة؛ الصفحة المنشورة عبر
  `DeckHtmlExporter` (`DeckBrandFont.FontFaceCss`)؛ وPPTX يسمّي العائلة في `a:latin`/`a:cs` لكل نص (`PptxBuilder.TextBox(typeface:)`)
  فيظهر على أي جهاز الخط مثبّت عليه (لا تضمين للخط داخل الملف). قائمة الخط في النافذة مجموعتان: «خطوط الشركة (مرفوعة)»
  و«خطوط حرة (Google Fonts)». اختبارات `DeckBrandFontTests` (TTF حقيقي إن وُجد، ملف تالف، الأوزان، @font-face، قبول العائلة
  المرفوعة). تحقق Playwright على الـ mock برفع DejaVuSerif عادي+عريض: الصفان، `document.fonts` 400/700 loaded، المعاينة والمعرض
  بالخط، الحفظ يحمل `fontFaces`، الحذف يعيد الافتراضي، بلا تمرير أفقي على 390px.
- **قالب الشركة PowerPoint — المرحلة 1 ✅** (طلب المستخدم: «نفس الخلفية ونفس الصفحات» في الويب لا الألوان فقط؛ رفض LibreOffice،
  والموديل لا يرسم): محرّك حتمي `Decks/PptxSvgRenderer.cs` يحوّل تخطيط/شريحة/أساس إلى SVG بالإحداثيات نفسها (EMU÷9525 = px):
  الخلفية (لون/تدرّج/صورة، بالوراثة صفحة←تخطيط←أساس)، أشكال الأساس والتخطيط غير المحجوزة ثم الصفحة، `prstGeom` (مستطيل، مدوّر،
  بيضاوي، خطوط/موصّلات، مثلث، معيّن، أسهم، شيفرون، سداسي، أسطوانة…؛ غير المعروف ← مستطيل) و`custGeom` (moveTo/lnTo/cubic/quad/arcTo/
  close بتحجيم مسار w/h)، تعبئة (solid/grad خطي/blip بقص clipPath/noFill أو من `p:style`)، حدود (عرض، لون، شرطات)، ألوان الثيم
  عبر `clrMap` مع `lumMod/lumOff/alpha/tint/shade/satMod` (HSL)، الصور مع `srcRect` (svg متداخل) و`alphaModFix`، المجموعات
  (`chOff/chExt` → تحويل محلي)، الجداول، الرسوم البيانية كمنطقة «رسم بياني»، والنص داخل `foreignObject` (XHTML) ليلتفّ ويعمل RTL
  تلقائيًا للعربي: وراثة الحجم/اللون/الخط/الغامق/التنقيط من lstStyle الشكل ← المحجوز في التخطيط ← المحجوز في الأساس ← `txStyles`؛
  `bodyPr` (هوامش، anchor، `normAutofit fontScale`)؛ المحجوز بلا نص على التخطيط يُعرض بنص المطالبة باهتًا، وعلى الشريحة لا يُعرض.
  يُتجاهل: الظلال/التأثيرات/SmartArt. **EMF/WMF** تُعلَّم بإطار أحمر «غير مدعومة» (`Unsupported`؛ التحويل على Windows مؤجَّل).
  الصور تُحال عبر `MediaUrl` (data URL في الاختبارات، `/api/decks/brand/template/{tid}/media/{name}` في المنصة).
  `Decks/DeckBrandTemplateStore.cs`: قالب واحد لكل مشروع (`dbo.DeckBrandTemplates` بالبايتات + `InfoJson`، و`dbo.DeckBrandTemplatePages`
  SVG لكل صفحة تُرسم مرة عند الرفع)؛ `Analyze` يعيد `TemplateInfo` (الحجم، ألوان الثيم، أكثر الخطوط، تخطيطات بأسمائها وأساسها
  وأماكنها المحجوزة كنسب `TemplatePlaceholder{type,idx,x,y,w,h,prompt}` وعدد الصفحات التي تستخدمها، وصفحات الأمثلة بعنوانها وعدد
  نصوصها ورسومها) ويعلّم `generic` التخطيطات الافتراضية (أسماء PowerPoint الإنجليزية القياسية غير المستخدمة) فتُخفى افتراضيًا.
  API: `GET/POST/DELETE /api/decks/brand/template` (≤64MB، .pptx/.potx، صلاحية الهوية نفسها؛ `?all=true` يُظهر الافتراضية)،
  `GET …/template/{tid}/pages/{layout|slide}/{i}.svg` و`…/media/{name}` **بلا دخول** مع تخزين مؤقت (ذاكرة بايتات 15 دقيقة).
  الواجهة: قسم «قالب الشركة (PowerPoint)» في نافذة الهوية: رفع/استبدال/حذف، ملخص وتحذيرات، تبويبا «التخطيطات/الصفحات الجاهزة»
  بمصغّرات **SVG مضمّنة** (تُجلب بثلاث قنوات متتابعة؛ داخل `<img>` لا تُحمَّل الصور الخارجية ولا يُرسم foreignObject)، وخيار إظهار
  الافتراضية. قياس على قالب ثقة (19.8MB، 4 أساسات، 69 تخطيطًا منها 33 افتراضية، 73 صفحة، 1,871 صورة): كل الصفحات تُرسم بلا أخطاء
  وبدقة عالية (الخلفيات والشعارات صور فتطابق، الأشكال والمجموعات والنصوص في مكانها)؛ النواقص: 12 EMF، وخط Tanseek غير متاح
  (يُحل برفع ملفاته من قسم الخطوط). اختبارات `PptxSvgRendererTests`/`DeckBrandTemplateAnalyzeTests` (ملف مولَّد دائمًا + قالب المستخدم
  إن وُجد في scratchpad بكتابة SVG لكل صفحة للفحص البصري بـ Playwright).
- **قالب الشركة — المرحلة 2 ✅ (الشرائح على صفحات القالب)**: التحليل يستنتج لكل تخطيط **أدوار** الأماكن المحجوزة
  (`TemplatePlaceholder.Role`: title = ctrTitle/title أو أعلى محجوز بمطالبة «عنوان»/قصير، subtitle، body = أكبر مساحة متبقية، pic) مع
  حجم/لون/خط/محاذاة النص من `lstStyle lvl1pPr defRPr` للتخطيط، و`TemplateDefaults{title,section,content,end}` بالاسم ثم البنية
  (`GuessDefaults`؛ على قالب ثقة: Cover Slide / Separator2 / 1_General Slide / End Cover) قابلة للتعديل من 4 قوائم في قسم القالب
  (`PUT /api/decks/brand/template/defaults`). لكل تخطيط نسخة SVG ثانية **بلا الأماكن المحجوزة** (`RenderLayoutBackground`، kind `bg`،
  `TemplatePage.BgUrl`) تُستخدم خلفيةً — النسخة بالمطالبات للمعرض فقط. المواصفة: `DeckSpec.Template` تُرفق عند الرسم/النشر فقط (الواجهة
  من `DK.template`، والخادم في `Publish` ما لم يكن `templateMode == "off"`)، `DeckSpec.TemplateMode` ("off" يعطّل القالب للعرض كله؛
  زر «🏢 القالب: مفعّل/معطّل» في شريط الأدوات)، و`DeckSlide.Page{kind: layout|slide|none, index}` لصفحة بعينها (قائمة «صفحة القالب»
  في مفتّش الشريحة: تلقائي / بلا قالب / التخطيطات / الصفحات الجاهزة). المحرك `jeem-deck.js`: `templatePage(spec, slide)` يحسم الصفحة
  (صريحة أو الافتراضية بحسب التخطيط)، و`renderOnTemplate` يرسم `.jd-slide.jd-tpl` بنسبة أبعاد القالب (`--jd-ar`): خلفية `.jd-tpl-bg`
  (SVG يُجلب مرة ويُخزَّن في `svgCache`) + صناديق `.jd-box` مطلقة بإحداثيات الأماكن المحجوزة (نسب مئوية) للعنوان والفرعي والمحتوى
  بحجم خط pt→cqw ولون المحجوز؛ بلا محجوز محتوى يُصنَّع صندوق تحت العنوان؛ **الصفحة الجاهزة** (kind slide) تُعرض كما هي مع العنوان فقط
  (رسوماتها تشغل المساحة). `bodyHtml(slide, L, ce, ed)` صار مشتركًا بين الشكلين. شعار الهوية والتذييل لا يُرسمان في وضع القالب (القالب
  يحملهما). تحقق Playwright على الـ mock بقالب ثقة: بلا قالب الشكل الافتراضي، بعد الرفع الغلاف بعنوان 72pt بلون 38B6E7 في مكانه
  وخلفية الدوائر والشعار والتذييل، شريحة المؤشرات والرسم على «1_General Slide» بلا نصوص مطالبة، التعديل المباشر يحدّث المواصفة،
  اختيار صفحة جاهزة/بلا قالب/تعطيل، الحفظ لا يرسل `template`/`templateMode` الافتراضي، مصغّرات المعرض الـ20 على القالب، بلا تمرير
  أفقي على 390px. نافذة الهوية صارت قابلة للتمرير (`max-height: calc(100vh - 32px)`). **المرحلة 3** (تصدير PowerPoint فوق القالب
  بالتخطيطات والشرائح الأصلية) لم تبدأ — PPTX الحالي ما زال بالأساس المولَّد.
- **استيراد ملف موجود** (`Decks/DeckImporter.cs`، `POST /api/decks/import` multipart `file` + `refine` + `title`، ≤25MB):
  بطاقة «استيراد ملف موجود» في معرض «عرض جديد» تفتح `deck-import-modal`. الاستخراج **حتمي بلا موديل**: PPTX بـ OpenXML (عنوان
  من placeholder العنوان، فقرات/أسطر النص كنقاط بلا رموز التنقيط، جداول `a:tbl`، صور مضمّنة ≤600KB حتى 20، وأول سلسلة من رسوم
  الأعمدة/الخط/الدائري عبر `ChartPart` → `DeckChart`، وملاحظات المتحدث)، PDF بـ PdfPig (كلمات → أسطر بالإحداثي → أول سطر عنوان،
  دمج الأسطر فقرات، النقاط بالرموز)، HTML بقارئ regex خفيف بلا مكتبة (إزالة script/style/svg، العناوين تبدأ شرائح، p/li/div نقاط،
  الجداول، صور data URL فقط والخارجية تُذكر في `warnings`). `ToSpec`: شريحة عنوان من الصفحة الأولى إن كانت عنوانًا فقط، نقاط
  بحد 8 لكل شريحة (+ «تابع»)، 2–6 نقاط بصيغة «تسمية: رقم» → شريحة kpis، جدول/رسم/صورة شريحة لكل منها، حد 30 شريحة.
  خيار «تحسين الترتيب بالذكاء الاصطناعي» = نداء واحد بلا أدوات (`RefineAsync`، الصور تُستبدل بمعرّفات قبل الإرسال وتُعاد بعده؛
  أي فشل يُبقي الحتمي ويضيف ملاحظة). العرض يُنشأ بـ `templateKey` null ويُفتح في المحرر بهوية المشروع. اختبارات
  `DeckImporterTests` (PPTX مولَّد ببنّائنا ذهابًا وإيابًا، HTML بعناوين/قوائم/جدول/صورة/script، PDF يدوي بسيط، امتداد مرفوض).
  تحقق Playwright على الـ mock: البطاقة، أخطاء بلا ملف/امتداد خاطئ، استيراد يفتح المحرر برسالة الملخص، بلا تمرير أفقي على 390px.
  **درس من صفحة المستخدم «دور الإدارة في مشروع الخوارزمي»** (تطبيق تفاعلي بناه Claude): محتواها الحقيقي في مصفوفات
  جافاسكربت (`R`/`P`/`L`) وكتلة `tk-data` JSON لا في الوسوم، فالاستخراج الحتمي أعاد هيكل الواجهة فقط (أزرار، تلميحات، «لديك
  تعديلات غير محفوظة»). الإصلاح: (1) قارئ HTML يتجاهل شجرة `nav/button/input/select/form` و`aria-hidden=true` و`role=tab|tablist|
  button` (مكدّس `skipTag/skipDepth`)، يجمع `<a href=http…>` في `ImportedPage.Links` → شريحة «روابط»، ويحذف رسائل الواجهة
  (`UiTextRx`)؛ **لا يتجاهل سمة `hidden`** لأن تبويبات الصفحة المخفية محتوى. (2) `CollectDataBlocks` يحفظ كتل
  `<script type="application/json">` كما هي والنصوص الحرفية من السكربتات المضمّنة التي تبدو كلامًا (عربي، أو لاتيني بحرف كبير
  أو ≥4 كلمات؛ يستبعد الكود والمحددات ورسائل الواجهة) في `ImportedDocument.DataBlocks` بلا تنفيذ أي كود. (3) وضع
  `mode=understand` (الافتراضي لـ .html في الواجهة؛ `none` للباقي): `UnderstandAsync` نداء واحد بلا أدوات يتلقى
  `BuildUnderstandInput` (مخطط الصفحات + الجداول + الروابط + الصور بمعرّفات `image:p-k` + كتل البيانات، ≤80K حرف) ويبني
  المواصفة كاملة بتخطيطات `two-column`/`table`/`tasks` (الحقل الجديد `DeckSlide.Tasks[{t,done}]`)، و`ApplyUnderstood` يعيد
  الصور ويضيف `editable` لشرائح المهام؛ الفشل يُبقي الحتمي مع ملاحظة. `mode=refine` هو التحسين القديم. الاختبارات
  `DeckImporterAppPageTests` (تجاهل الواجهة، الروابط، كتل البيانات بلا كود، تفسير رد الموديل) + اختبار على ملف المستخدم إن وُجد
  في scratchpad. **نداء الفهم بالموديل لم يُجرَّب حيًا** (لا مفتاح في بيئة الجلسة) — والمطابقة البصرية لصفحة تفاعلية ليست هدفًا:
  الصفحة نفسها تُنشر كما هي من «العروض التقديمية».
- **البناء بالذكاء الاصطناعي** `DeckBuilder.cs` (`POST /api/decks/generate {message, deckId?, templateKey?, spec?}`):
  نداء تخطيط واحد بلا أدوات يعيد `{reply, spec}` وكل شريحة بيانات فيها `needsData/intent/sql`، ثم نداءات ملء بالتوازي
  لكل شريحة (`FillSlideAsync`، أداة `query_data` فقط) على بيانات المشروع الحقيقية. تعديل عرض مفتوح يمرّر `spec` الحالية
  فيعيد الموديل نسخة معدَّلة (إضافة شريحة، تغيير عنوان…). يُسجَّل في الاستهلاك بعنوان `[عرض] …`.
- **المحرك** `wwwroot/js/jeem-deck.js` (`window.JEEM_DECK`): `renderSlide/thumbnail/present/chartSvg/CSS` — رسوم SVG
  خفيفة (أعمدة/خط/دائري) بوحدات `cqw` فتتدرج مع الحاوية، تحرير مباشر `contenteditable` بمسارات `data-f` إلى المواصفة،
  وكتلة المهام تُخزَّن عبر `opts.editableStore` (في الصفحة المنشورة = `jeem.store` من `jeem-page.js`، وإلا محليًا).
  `present()` عرض ملء الشاشة بلوحة مفاتيح وطباعة.
- **التصدير والنشر**: `DeckHtmlExporter.Build(spec, runtimeJs, includeBridge)` صفحة HTML واحدة تضمّن المحرك والجسر؛
  `POST /api/decks/{id}/publish` ينشئ عرضًا في «العروض التقديمية» (أو يستبدل ملفه على نفس الرابط إن نُشر قبلًا) ويعيد
  الرابط المقروء؛ `POST /api/decks/export/pptx` عبر `DeckPptxBuilder` (يعيد استخدام دوال `PptxBuilder` التي صارت
  `internal`؛ الرسوم تُرسل PNG من الواجهة بـ `dkSvgToPng`)؛ PDF من نافذة طباعة.
- **الواجهة** في app.js (قسم `DK`): معرض القوالب + العروض المحفوظة (`dkShowGallery`)، المحرر (`dkRender`: الشريحة +
  شريط مصغّرات بسحب لإعادة الترتيب + مفتّش `dkInspectorHtml` لبيانات الرسم/الجدول/المؤشرات/النقاط/الصورة/الملاحظات)،
  شريط أدوات (إضافة/حذف/نقل/تخطيط/حفظ/نشر/PPTX/طباعة)، والمحادثة `dkAsk`.
- **ما تحقق** (بلا SQL Server: خادم Node وهمي يحاكي `/api/decks/*` + Playwright): المعرض بـ20 نموذجًا و9 تصنيفات وبحث
  ومعاينة وعرض توضيحي بكل شرائح الـ20 بلا `jd-missing`، حالة البداية، توليد 7 شرائح
  من المحادثة، رسم KPI/خط/دائري/جدول، التحرير المباشر والمفتّش، إضافة/تخطيط/نقل/حذف، مهام تُحفظ، حفظ، تعديل بالمحادثة،
  نشر برابط، تنزيل PPTX، بلا تمرير أفقي على 390px. اختبارات الوحدة 146/146.
- **تحقق حي على جهاز المستخدم (2026-11-03، gpt-4.1، SQL Server، عبر API)**: قالب sales-pipeline على ملف الفرص (1069 صفًا)
  → 6 شرائح و4/4 شرائح بيانات مبنية (مؤشرات بقيم حقيقية، رسمان، جدول أكبر 10 فرص) في 29 ثانية؛ تعديل بالمحادثة
  («أضف شريحة دائرية…») على `deckId` → 7 شرائح، 1/1، 28 ثانية؛ النشر أنشأ عرضًا برابط مقروء، وإعادة النشر بعد التعديل
  أعادت `updated: true` على نفس الرابط والصفحة العامة تحمل المحرك والمواصفة الجديدة؛ تصدير PPTX بـ7 شرائح و3 صور
  صالح؛ صفر أخطاء في سجل الخادم. لم يُختبر هنا الرسم في متصفح حقيقي (Playwright غير متاح على الجهاز).

## تصميم حر (`Freeform/`) — صفحة تحت «منشئ العروض» بتجربة Claude artifacts

- **الفكرة** (طلب المستخدم: «زي اللي متنفذ في Claude»): شاشة `screen-freeform` («تصميم حر» في القائمة بعد «منشئ العروض»، ضمن
  `PROJECT_SCOPED_SCREENS`): محادثة يسارًا، والموديل يكتب **صفحة HTML كاملة ومستقلة** تُعرض يمينًا فورًا في `<iframe sandbox>`
  (بلا `allow-same-origin` فلا كوكي ولا API — مُتحقَّق: `document.cookie` محجوب داخل الإطار) عبر `srcdoc`. الفرق عن «منشئ العروض»: لا
  مواصفة ولا قالب شرائح؛ الموديل حرّ في التصميم (صفحة تفاعلية، لوحة، تقرير…) لكنه داخل المنصة: يرى ملف تعريف بيانات المشروع ويستعلم
  بـ `query_data` ليضمّن أرقامًا حقيقية، ويلتزم بهوية المشروع (ألوان/خط/قواعد @font-face لخطوط الشركة/التذييل)، ويضمّن
  `jeem-page.js` + `jeem.store` للأجزاء القابلة للتعديل، ويضع `<img data-jeem-logo>` تملؤه المنصة بالشعار عند العرض/النشر
  (`FreeformController.ApplyBrandAssets`؛ المخزَّن بلا بيانات الشعار).
- **الخادم**: `Freeform/FreeformStore.cs` — جداول `dbo.FreeformDesigns` (آخر HTML ≤4MB، `Version`، `PresentationId`)،
  `dbo.FreeformVersions` (كل إصدار بطلبه، يُحتفظ بآخر 40) و`dbo.FreeformMessages`. `Freeform/FreeformBuilder.cs`: `GenerateAsync` نداء
  مرحلي واحد (`StagedCall` بأداة `query_data` إن وُجدت بيانات، 6 دورات، بلا إجبار) بنظام يصف القواعد (صفحة واحدة مضمّنة، RTL، متجاوبة،
  لا مكتبات إلا Google Fonts، لا اختلاق، الأجزاء القابلة للتعديل عبر الجسر، التعديل يعيد الصفحة كاملة) ويعيد JSON
  `{reply, html, usedData}`؛ `ExtractHtml` يقبل حقل `html` أو أول `<html>…</html>` في النص ويغلّف المقتطف بوثيقة RTL.
  التعديل يمرّر HTML الحالي كاملًا (≤180K حرف) + آخر 8 رسائل. `Controllers/FreeformController.cs` (`/api/freeform`): قائمة، `GET {id}`
  (التصميم + HTML + الإصدارات + الرسائل + رابط النشر)، `GET {id}/versions/{v}`، `POST generate {message, designId?, html?}` (بلا
  designId ينشئ تصميمًا بعنوان من الطلب `TitleFrom`)، `PUT {id}` (HTML معدَّل يدويًا = إصدار جديد، و/أو عنوان)، `POST {id}/restore/{v}`،
  `POST {id}/publish` (إلى «العروض التقديمية» أو تحديث المنشور على نفس الرابط، بنفس منطق نشر العروض)، `DELETE` (صاحبه أو المسؤول).
- **الواجهة** (app.js قسم `FF`): حالة بداية ببطاقات تصاميم المشروع، شريط أدوات (عنوان قابل للتعديل، قائمة الإصدارات مع «استعادة» عند
  عرض إصدار قديم، «📱 جوال» معاينة 390px، «</> الكود» محرر نصي يحفظ إصدارًا جديدًا، فتح في نافذة، تنزيل HTML، نشر)، والمحادثة
  (`ffAsk`؛ عند عرض إصدار قديم يُرسَل HTML المعروض ليُعدَّل هو). تحقق Playwright على الـ mock: الترتيب في القائمة، التوليد والمعاينة داخل
  الإطار المعزول، التعديل بالمحادثة → إصدار 2، عرض إصدار قديم واستعادته، تعديل الكود وتطبيقه، معاينة الجوال 390px، حفظ العنوان،
  النشر برابط، التنزيل، رسالة خطأ 503 واضحة، إعادة فتح تصميم برسائله، بلا تمرير أفقي على 390px. اختبارات `FreeformTests` (استخراج HTML،
  العنوان، حقن الشعار، نظام الموديل) 176/176. **التوليد الحي بموديل حقيقي لم يُجرَّب** (لا مفتاح هنا) — أول تجربة على جهاز
  المستخدم ستحدد جودة الصفحات وحاجة الـ prompt للضبط؛ الصفحات الكبيرة (> ~40KB) قد تُقتطع بحد `Anthropic:MaxTokens` (16000).
- **أنواع المخرجات كما في Claude** (طلب المستخدم: «نفس اللي في Claude بالضبط وسيبك من اللي عندنا»): بطاقات «اصنع شيئًا جديدًا»
  في حالة البداية (`.ff-kinds`: مستند، شرائح، تصميم، لوحة، حركة — أيقونات SVG مضمّنة وألوان خلفية كبطاقات Claude، شارات «جديد/تجريبي»).
  لا بد من اختيار نوع قبل أول طلب (رسالة إرشاد وإلا). `FreeformDesign.Kind` (`doc|slides|design|dashboard|motion`، عمود `Kind` يُضاف
  بـ `COL_LENGTH` للجداول القائمة، `NormalizeKind` → design)، يُرسل مع `generate` ويثبت للتصميم. `FreeformBuilder.KindInstructions`
  تضيف لكل نوع بنيته: مستند = ورقة 820px بترويسة/محتويات/تذييل و`@page A4` للطباعة؛ شرائح = `<section class="slide">` 16:9 بتنقّل
  لوحة مفاتيح/لمس وشريط تقدّم وملء شاشة وطباعة شريحة لكل صفحة؛ لوحة = بطاقات مؤشرات + رسوم SVG من مصفوفة JS واحدة (بيانات حقيقية من
  `query_data` أو شارة «توضيحية») وفلاتر وألوان حالة ثابتة بلا محورين؛ حركة = مشهد SVG/CSS 10–30 ثانية بتحكم تشغيل/إيقاف/إعادة وتكرار
  ويحترم `prefers-reduced-motion`؛ تصميم = حرّ. الواجهة لكل نوع: placeholder مختلف، شريحة النوع في العنوان والقائمة، معاينة المستند
  على خلفية رمادية بعرض 980px، أزرار سياقية: «⛶ ملء الشاشة» (شرائح/حركة، `requestFullscreen` على الإطار)، «↻ إعادة» (حركة، إعادة
  تحميل srcdoc)، «PDF» (مستند/شرائح، نافذة جديدة تطبع عند التحميل). تحقق Playwright على الـ mock: البطاقات الخمس، إرشاد بلا نوع، اختيار
  مستند يغيّر الـ placeholder ويُرسل `kind=doc` ويُظهر PDF ويخفي ملء الشاشة، حركة تُظهر إعادة/ملء الشاشة، الشرائح، شارات الأنواع في
  القائمة، بلا تمرير أفقي على 390px. اختبارات `FreeformKindTests` 182/182. غير منفَّذ بعد (مقترح): تصدير Word (.docx) للمستند
  وWebM للحركة من المتصفح.
- **معرض البداية كمعرض Artifacts في Claude** (طلب المستخدم: «نسخة طبق الأصل»): `#ff-start` صار رأسًا بعنوان «تصميم حر» وتبويبات
  «الكل / تصاميمي / مشترك معي» (`FF.gal.tab`؛ تصاميمي = `createdByUserId === state.currentUser.id`، مشترك معي = الباقي في المشروع)،
  وأدوات يمينًا: بحث يتوسّع عند النقر (`.ff-search.open`)، تصفية بالنوع (قائمة `ff-filter-panel` — تحتاج `stopPropagation` لأن
  `closeAllDropdowns` مربوطة على نقر المستند)، وتبديل شبكة/قائمة (`FF.gal.list`، يُحفظ في localStorage `ffGalView`). ثم بطاقات
  «اصنع شيئًا جديدًا» الخمس (`.ff-kinds` بخمسة أعمدة ثابتة، تلتف تحت 760px)، ثم التصاميم السابقة كبطاقات `.ff-card.k-<kind>`
  (`ffRenderGallery`): منطقة فنية ملوّنة بلون النوع وفيها «ورقة» بيضاء بمعاينة حيّة مصغّرة — `iframe.ff-thumb` بعرض 1100px
  مُصغَّر بـ `--ff-s` (`ffScaleThumbs`) تُجلب صفحته كسولًا عند الظهور (`IntersectionObserver` → `ffLoadThumb` → `GET /api/freeform/{id}`
  بذاكرة `FF.gal.htmlCache` تُبطَل لتصميم مفتوح عند `ffRender`)؛ وتذييل بمربع أيقونة النوع (نسخة من أيقونة بطاقة النوع
  `ffKindIcon`) والعنوان و«فُتح قبل …» (يُسجَّل محليًا لكل مشاهد في `ffOpened:<id>` عند `ffOpen`، وإلا «عُدِّل قبل …»)
  بجمع عربي صحيح (`ffAgo`). **ضبط الطباعة** (طلب المستخدم: الفرق بين الأقسام والخطوط): رأس بعنوان 26px/700 وخط فاصل، رؤوس أقسام
  `.ff-sec-head` (18px/700 + وصف 13px) لـ«اصنع شيئًا جديدًا» و«تصاميمك السابقة» بفاصل علوي وشارة عدد `#ff-count`، بطاقات الأنواع باسم 15px/700
  ووصف 13px وبدون سطر التلميح (`#ff-make-hint` مخفي)، تذييل البطاقة 14px/700 بفاصل علوي، فجوات 28px بين الأقسام. **قرار المستخدم**:
  فصل الشريط/اللوحة/القائمتين يبقى بالشكل الأصلي (خطوط فاصلة رفيعة) — جُرِّبت حشوة وبطاقات ثم 5 بدائل (ظلال، تباين نغمة، لمسة هوية،
  مجارٍ، شريط داكن) ورُفضت كلها وأُزيلت من CSS؛ لا تعد إليها بلا طلب. درس CSS: `.ff-make`/`.ff-start` شبكات بعمود `minmax(0,1fr)` صراحة وإلا يحسب `auto-fill` داخل عمود
  `auto` عرضه بالمحتوى الأقصى فيتمدد 780px ويظهر تمرير أفقي. تحقق Playwright على الـ mock (مزروع بـ4 تصاميم لمستخدمَين): التبويبات
  والبحث والتصفية والقائمة، 5 معاينات محمّلة، «فُتح الآن» بعد الفتح، تصميم جديد يتصدّر، بلا أخطاء وبلا تمرير أفقي على 390px.
  **لمسة كلود المقبولة** (بعد الرجوع للشكل القديم، بطلب المستخدم): شريط رأس مساحة العمل (`.cc-pane-head.dk-toolbar`) ورأس المحادثة في
  شاشات منشئ العروض/تصميم حر/محادثة Claude (`.chat-head`) بخلفية رمادية خفيفة `#F3F5F7` وحشوة أكبر و`line-height` 1.5/1.6 فلا يلاصق
  النص الخطوط الفاصلة؛ صندوق الكتابة `.cc-composer .input-box` بزوايا 16px وظل ناعم وحلقة تركيز زرقاء؛ وقاعدة عامة `.jeem-scope .send`
  (زر مربّع 38px أزرق زاهٍ بأيقونة `#i-send` معكوسة للـ RTL وظل وتحويم) — كانت القاعدة القديمة مقيّدة بـ`#send.send` فبقي `#ff-send`/`#dk-send`
  بلا تنسيق وبلا أيقونة. لا يغيّر فصل اللوحات نفسه (الخطوط الرفيعة باقية).
  **التبويبات بجوار «تصاميمك السابقة»** (بطلب المستخدم): صف الكل/تصاميمي/مشترك معي + البحث/التصفية/القائمة انتقل من تحت عنوان
  الصفحة إلى رأس قسم «تصاميمك السابقة» (`.ff-recent-head .ff-tabs/.ff-tools`)؛ المعرّفات والسلوك كما هي. **علّة التمرير العام أُصلحت**:
  كل الشاشات ذات الارتفاع الثابت كانت `calc(100vh - 53px)` بينما الشريط العلوي 68px (`min-height`) فظهر شريط تمرير 15px للصفحة كلها؛
  الآن `calc(100vh - var(--topbar-h, 68px))` في jeem.css وapp.css، و`trackTopbarHeight` في app.js (ResizeObserver على `.topbar`) يضبط
  المتغيّر على `#app-shell` بالارتفاع الفعلي فيصحّ على الجوال أيضًا حين يلتف مسار التنقل. تحقق: لا تمرير للصفحة في المحادثة/تصميم حر/
  منشئ العروض على 950px ولا على 390px.
  **القائمة المطوية تُبقي صفحات المشروع** (علّة أبلغ عنها المستخدم: كانت تختفي كليًا): `.proj-sub` خرجت من قائمة الإخفاء في
  `.app.is-collapsed` وتُعرض أيقونات تحت نقطة المشروع بلا الخط العمودي، وكل `.nav-link` يأخذ `title` من نصه عند الإقلاع (app.js قبل
  `sbCollapseBtn`) فتُقرأ الأيقونات بالتلميح. **حالة الانتظار** (طلب المستخدم): `ccSetLoading(paneId, title, hint)` في app.js تضع طبقة
  `.cc-loading` (دوّار + عنوان + تلميح، `role=status`) فوق `#ff-pane`/`#dk-pane` أثناء `ffAsk`/`ffRefreshFast`/`ffConvert`/`dkAsk`/`dkRefreshData`
  وتزيلها في `finally`؛ وفي المحادثة فقاعة «يكتب…» بثلاث نقاط متحركة (`.cc-msg .typing::before`؛ نص الحالة في `FF.busyText`، ورسالة
  `thinking` في منشئ العروض صارت `isHtml` بـ`span.typing`). `prefers-reduced-motion` يوقف الدوّار والنقاط. تحقق Playwright بتأخير 1.5 ث على
  `generate`: 8 صفحات ظاهرة بأيقوناتها عند الطي والتنقل منها يعمل، الطبقة والفقاعة تظهران أثناء الانتظار وتزولان بعد الرد في الشاشتين،
  بلا أخطاء وبلا تمرير أفقي على 390px.
- **نماذج تصميم حر مرفقة في المحادثة ✅** (`Freeform/FreeformTemplates.cs`): كتالوج ثابت 29 نموذجًا في الأنواع الخمسة (مستند 6، شرائح 6،
  تصميم 6، لوحة 6، حركة 5) بتصنيفات `DK_CATS` نفسها؛ النموذج **موجز بنيوي لا HTML**: `{key, kind, category, icon, name, description,
  sketch, sections[{title, needsData}], brief}` (المفاتيح تُحفظ في `FreeformDesigns.TemplateKey` — الإضافة آمنة وإعادة التسمية تكسر المحفوظ).
  `BriefFor` يُلحق بنظام الموديل «النموذج المختار… الأقسام بترتيبها… تعليمات الشكل» (`BuildSystemPrompt(…, template)`). `GET /api/freeform/templates?kind=`،
  و`generate` يقبل `templateKey` في الطلب الأول ويُثبّته على التصميم فتحافظ التعديلات اللاحقة على البنية (نموذج من نوع آخر يُتجاهل).
  الواجهة: النقر على بطاقة نوع يفتح `ff-gallery-modal` (نفس `dash-gallery-box` ومحدّداته أُضيفت لـ`#ff-gallery-modal`): تصنيفات بعدّاد،
  بحث، بطاقة «تصميم حر بلا نموذج»، وبطاقة لكل نموذج مصغّرتها **رسم تخطيطي (wireframe)** من `sketch` (`ffSketchHtml`: صفوف `/` وكتل `,`؛
  `h p b k3 c l d t i f g n s a` بأنماط `.ff-sk .sk-*` بلون النوع)، ثم معاينة بالأقسام والموجز و«استخدم هذا النموذج». الاختيار في
  `FF.template` (undefined = لم يُختر بعد → رسالة إرشاد؛ null = حر؛ كائن = نموذج) ويظهر **شريحة مرفق** فوق صندوق الكتابة (`#ff-attach`،
  `ffRenderAttach`: «📎 نموذج: X · مستند» بزر ✕ يحوّل إلى حر، أو «✦ تصميم حر بلا نموذج»؛ النقر يعيد فتح المعرض)، وفي أول رسالة بالمحادثة
  (`.ff-msg-attach`) فيبقى الاختيار واضحًا. بعد البناء تثبت الشريحة بلا ✕، وإعادة فتح تصميم تستعيدها من `design.templateKey`.
  اختبارات `FreeformTemplateTests` (اتساق الكتالوج، الموجز في النظام، وإسقاط JSON للـ mock) 185/185. تحقق Playwright على الـ mock: المعرض
  بـ6 نماذج للمستند و5 تصنيفات و33 كتلة مصغّرة، بحث وتصنيف ومعاينة، الاستخدام يغلق ويُظهر الشريحة، الإرسال يحمل `templateKey` والرسالة
  تحمل المرفق، الحر يرسل null، ✕ يحوّل لحر، الشريحة تعود عند إعادة الفتح، بلا أخطاء وبلا تمرير أفقي على 390px. **التوليد الحي على
  نموذج لم يُجرَّب** (لا مفتاح هنا).
- **معاينة تفاعلية ببيانات تجريبية لكل نموذج** (`wwwroot/js/jeem-ff-demo.js`، `JEEM_FF_DEMO.build(template)` — **للمعرض فقط**):
  يبني صفحة HTML كاملة من أقسام النموذج بكلمات مفتاحية في عناوينها (مؤشرات ← بطاقات KPI، اتجاه ← خط SVG، توزيع ← أعمدة/دائري،
  جدول/تفاصيل ← جدول، مهام/خطوات ← قائمة تعليم تفاعلية، أسئلة ← details، نموذج/تواصل ← نموذج بإرسال، فريق/ميزات ← بطاقات،
  زمني ← محطات بنقر، وإلا فقرة ونقاط) بأرقام حتمية من بذرة المفتاح (`rng`) وشارة «بيانات توضيحية». لكل نوع هيكله: مستند = ورقة A4
  بترويسة وتذييل؛ شرائح = `<section class="slide">` بتنقّل أزرار/لوحة مفاتيح/لمس وشريط تقدّم؛ تصميم = شريط تنقّل + hero (للهبوط/
  التعريف) + كتالوج بتصفية حيّة (`ds-catalog`) وخطوات للنموذج (`ds-form`)؛ لوحة = رأس بفلاتر (تغيير الفترة يضاعف المؤشرات)؛ حركة =
  مشهد SVG بمسار يُرسم ومحطات تظهر تباعًا مع إيقاف/إعادة/تكرار و`prefers-reduced-motion`. في المعرض: معاينة النموذج تعرض الصفحة
  نفسها مصغّرة (`.ff-live` + `ffLiveInto`، iframe بلا تفاعل) وزر «معاينة تفاعلية ببيانات تجريبية» يفتح `ffgal-view-demo` بشريط
  تنبيه أصفر وإطار كامل `sandbox="allow-scripts allow-modals"` و«استخدم هذا النموذج». خلفية حالة البداية صارت بيضاء (`.ff-start`).
  تحقق Playwright: الـ29 صفحة تُبنى بلا أخطاء، فلتر الفترة يغيّر المؤشر، التنقّل بين الشرائح، تصفية الكتالوج 2 من 6، إيقاف الحركة،
  بلا تمرير أفقي على 390px.
- **نماذج كتالوج اللوحات داخل نوع «لوحة»** (طلب المستخدم: «انقل كل نماذج المحادثة واللوحات إلى تصميم حر تحت نوع لوحة»):
  `Freeform/FreeformDashboardTemplates.cs` (singleton) يحوّل **حتميًا** كل نموذج لوحة منشور في الكتالوج (`BuiltinTemplates.Dashboards`
  = 8 أصلية + 212 مولَّدة + متتبع الفرص + 22 تفاعلية، مع تعديلات/إخفاء/إضافات مالك المنصّة من `TemplateStore` بنفس منطق
  `TemplatesController.Catalog`) إلى `FreeformTemplate` من نوع `dashboard` بمفتاح `d:<id>` (**لا يُعاد تسميته**: يُحفظ في
  `FreeformDesigns.TemplateKey`): قسم لكل عنصر (`needsData` دائمًا، بادئة اسم الصفحة عند تعدد الصفحات، ≤40، بلا تكرار) مع حقل جديد
  `FreeformTemplateSection.Type` (kpi/bar/line/donut/table/funnel/heat/interactive — `SectionType` يختزل أنواع المرجع الـ28)،
  رسم تخطيطي من الأنواع (`Sketch`: h/k{n}/رسمان في الصف/t)، أيقونة بالتصنيف، و`Brief` يلزم الموديل بصفحة لوحة واحدة (رأس وفلاتر، صف
  مؤشرات، رسوم SVG بالعناوين والأنواع نفسها، تبويب لكل صفحة، تفاعل للوحات التفاعلية) وبأرقام من `query_data` على جداول المشروع
  **لا أسماء الجداول الافتراضية في النموذج**. مخزَّن 5 دقائق ويُبطَل عند حفظ/حذف الإدارة. `GET /api/freeform/templates` يلحقها
  بعد الثابتة لنوع `dashboard` (العلم `dashboard: true` وشريحة «من كتالوج اللوحات» في المعاينة)، و`ResolveTemplateAsync` يحلّ `d:`.
  الواجهة: تصنيفات كتالوج اللوحات (`DASH_GAL_CATS`) تُضاف لقائمة المعرض عند نوع لوحة؛ `jeem-ff-demo.js` يحترم `section.type` ويجمع
  أقسام kpi المتتالية في صف بطاقات واحد (`kpiOne`). **كتالوج «المحادثة واللوحات» نفسه لم يُمس** (نسخة لا نقل — الحذف يحتاج قرار
  المستخدم). اختبارات `FreeformDashboardTemplateTests` (تحويل الـ243 كلها، تعدد الصفحات، التفاعلية، الأنواع، المفاتيح) 225/225
  وتُسقط `templates.json` في scratchpad للفحص؛ Playwright بالواجهة الحقيقية: 243 بطاقة و22 تصنيفًا، البحث، المعاينة بـ16 قسمًا،
  المعاينة التفاعلية (7 مؤشرات، 8 رسوم، جدول)، الاستخدام يرفق `d:pl-workforce-insights`، بلا أخطاء وبلا تمرير أفقي على 390px.
  **البناء الحي بموديل على نموذج لوحة محوَّل لم يُجرَّب هنا**.
  **ثم بطلب المستخدم («نفس الشكل بالضبط دون أي تغيير… أعد هيكلتها فقط لتناسب اللوحة»)**: اختيار «لوحة» يفتح **معرض تصميم حر نفسه**
  (`ff-gallery-modal` ببطاقاته الثلاث والنماذج الستة القديمة)، لكن كل نموذج منقول من الكتالوج (`t.dashboard`، يُطابَق بـ`DASH_GALLERY`
  على `d:<id>`) يُعرض **ببطاقة معرض المحادثة حرفيًا** (`dashGalThumb` + «n عنصر» + المصادر + شارة الجدوى `dashGalFeasBadge`)، والنقر
  يفتح `openDashGalPreview` الأصلية (المصغّرة الكبيرة، وسوم المصادر، تلميح الجدوى، قائمة العناصر، «معاينة تفاعلية بأرقام توضيحية»
  بالمحركات الحقيقية: JR والـ21 محركًا). الآلية: **مضيف المعرض** `GAL_HOSTS {chat, ff}` + `galHost/galEl(part)/galSetHost(kind)`؛ كل
  إشارات `el('gal-view-demo'|'gal-view-preview'|'gal-view-list')` في دوال المعاينة والعروض التوضيحية (80+46+3) صارت `galEl(...)`
  فترسم في حاويات المضيف الحالي (`ffgal-view-preview`/`ffgal-view-demo` في تصميم حر) بلا تكرار؛ `galSetHost` يفرّغ حاويات المضيف
  الآخر لأن معرّفات الأزرار داخل المعاينة ثابتة (`gal-use-btn`…). كل أزرار «استخدم هذا النموذج» (المعاينة + 21 عرضًا) تمر من
  `galUseTemplate(d)`: في مضيف تصميم حر تغلق المعرض وترفق `d:<id>` من `FF.templates` (أو تبنيه من بيانات الكتالوج احتياطًا)، وإلا
  المسار القديم كما هو. `openFfGallery('dashboard')` يحمّل الكتالوج والجدوى إن لزم ثم `galSetHost('ff')`؛ `openDashGallery` يضبط
  `chat`. الرسم التخطيطي والمعاينة التجريبية من الأقسام يبقيان للنماذج الستة القديمة والمرفوعة فقط. تحقق Playwright بالواجهة
  الحقيقية (1280 و390): معرض تصميم حر يفتح لا معرض المحادثة، البطاقات الثلاث، بطاقة الكتالوج بالمصغّرة الشبكية و«4 عنصر» وشارة
  «◐ 3 من 4 ببياناتك»، بطاقة تفاعلية بأيقونة المحرك، المعاينة الأصلية (4 عناصر، مصادر، جدوى، زر العرض)، العرض التوضيحي JR
  داخل نافذة تصميم حر (10 عناصر + شريط الفلاتر)، «استخدم» يغلق ويرفق `d:pl-workforce-insights`، ثم معرض المحادثة يعمل في مضيفه
  وحاوية تصميم حر فارغة، بلا أخطاء وبلا تمرير أفقي. **علّة أبلغ عنها المستخدم بصورة**: العرض التوضيحي داخل تصميم حر ظهر بلا شبكة ولا
  شريط تبويبات/فلاتر لأن كل CSS العرض التوضيحي (390 قاعدة في jeem.css: `.grid`/`.span-N`/`.widget`/`.jr-*`) مقفول بالمعرّف
  `#gal-view-demo` عمدًا (حتى لا يكسر اللوحة الحقيقية). الحل: كل محدِّد صار `:is(#gal-view-demo, #ffgal-view-demo)` (التخصيص نفسه)؛
  أي قاعدة جديدة للعرض التوضيحي تُكتب بالصيغة نفسها. تحقق Playwright بلقطتين متطابقتين للعرض نفسه في المعرضين.
- **تبويب «المنشورة» والرابط الدائم** (طلب المستخدم): لكل تصميم عرض منشور واحد (`PresentationId`؛ إعادة النشر تحدّث الصفحة على
  الرابط نفسه). `GET /api/freeform` صار يعيد لكل تصميم `publishedUrl` (المسار المقروء) و`publishedActive` (الرابط ليس مُلغى/منتهيًا)
  بتحليل قائمة عروض المنظمة مرة واحدة. في معرض البداية: تبويب رابع «المنشورة» (`FF.gal.tab === 'published'`)، وشارة «🔗 منشور» (أو
  «منشور (الرابط موقوف)» بالأحمر) في تذييل البطاقة، وزر `.ff-card-link` ينسخ الرابط الكامل بلا فتح التصميم (`ffCopyLink`:
  «✓ نُسخ» 1.4 ثانية، وعند فشل الحافظة `prompt`). في شريط التصميم المفتوح: زر «🔗 الرابط» (`#ff-link`، يظهر فقط لتصميم منشور) ينسخ
  الرابط الدائم، وزر «نشر» يصير «تحديث النشر». تحقق Playwright بالواجهة الحقيقية و`page.route` بدل الخادم: التبويبات الأربعة،
  المنشورة تعرض 2 من 3، النسخ من البطاقة لا يفتح التصميم، الفتح يُظهر الزر بالرابط، بلا أخطاء وبلا تمرير أفقي على 390px.
- **علّة أُصلحت — روابط `#قسم` داخل المعاينة تفتح المنصة**: وثيقة `srcdoc` ترث عنوان الصفحة الأم، فالرابط `href="#s1"` يُحلّ إلى
  `/index.html#s1` وينتقل الإطار إلى المنصة نفسها (ظهرت للمستخدم في معاينة «صفحة تعريفية بمشروع»). الحل في العرض فقط:
  `ffSandboxHtml(html)` في app.js يحقن `<script id="ff-anchor-fix">` يعترض نقر أي `a[href^="#"]` ويمرّر إلى العنصر بـ
  `scrollIntoView` — يُطبَّق على معاينة التصميم الحقيقية (`ffRender`)، ومصغّرات البطاقات، ومعاينة النموذج الحيّة، والمعاينة
  التفاعلية. الصفحة المحفوظة/المنشورة/المنزَّلة لا تُمس (خارج srcdoc تعمل الروابط طبيعيًا). صفحات العرض التوضيحي صارت تستخدم
  `data-go` بتمرير JS أيضًا. تحقق Playwright: نقر رابط في العرض التوضيحي وفي صفحة حقيقية بروابط `#` يبقي الإطار على `about:srcdoc`
  ويمرّر إلى القسم.
- **رفع من ملف موجود → نموذج مخصص** (`Freeform/FreeformTemplateStore.cs`، جدول `dbo.FreeformTemplates` لكل مشروع): بطاقة «رفع من ملف
  موجود» في معرض النماذج تفتح `ff-import-modal` (ملف .pptx/.pdf/.html ≤25MB + اسم اختياري؛ النوع = نوع المعرض المفتوح) →
  `POST /api/freeform/templates/import` يقرأ الملف **حتميًا بلا موديل** بـ `DeckImporter.Extract` و`FromDocument` يحوّله إلى نموذج:
  قسم لكل صفحة غير فارغة بعنوانها (أو أول فقرة قصيرة/«القسم n»، `needsData` إن فيها جدول/رسم) + `Content` نصي مرجعي (فقرات، جداول،
  رسوم، روابط، كتل بيانات؛ ≤30K حرف) يُلحق بموجز الموديل («# محتوى الملف المرفوع») مع `Brief` يلزمه ببنية الملف ومحتواه بلا اختلاق.
  المفتاح `u:<id>`، `custom: true`، تصنيف `uploaded` («مرفوعة من ملف» أول التصنيفات)، أيقونة 🗂️؛ `GET templates` يدمج المخصص قبل
  الثابت، و`ResolveTemplateAsync` في `generate` يحلّ المفتاحين. بعد الرفع يُغلق المعرض ويُرفق النموذج في المحادثة مباشرة برسالة تلخيص.
  `DELETE templates/{key}` لصاحبه أو المسؤول (زر «🗑️ حذف النموذج» في المعاينة؛ التصاميم المبنية عليه لا تتأثر). `Content` لا يُرسل للواجهة.
- **رفع صفحة HTML كما هي** (طلب المستخدم: «تتحط زي ما هي بالظبط بدون تعديل»): بطاقة ثالثة «رفع صفحة HTML كما هي» في معرض النماذج
  (`#ffgal-start-page`، أيقونة `code` أُضيفت إلى icons.js) تفتح `ff-page-modal` (ملف .html/.htm ≤4MB + عنوان اختياري) →
  `POST /api/freeform/import-page` (multipart `file` + `kind` + `title`): يحفظ الملف **حرفيًا** كتصميم جديد (إصدار 1، بلا موديل ولا نموذج،
  `templateKey` null)، العنوان من `<title>` ثم `<h1>` (`TitleFromHtml`) وإلا اسم الملف، ورسالتان في المحادثة؛ ثم يُفتح في المحرر فيُعدَّل نصيًا
  أو بالمحادثة أو يُنشر. حقن الشعار (`ApplyBrandAssets`) لا يمس الصفحة إلا إن كان فيها `<img data-jeem-logo>`. تحقق Playwright على الـ mock:
  البطاقة، خطأ بلا ملف، رفع صفحة → `FF.html` مطابق للملف حرفيًا والعنوان من `<title>` والمعاينة تعرضه، بلا أخطاء وبلا تمرير أفقي على 390px.
  اختبار `FreeformImportPageTests` 198/198.
  **وPowerPoint كما هو** (طلب المستخدم التالي): البطاقة صارت «رفع ملف كما هو» (HTML أو .pptx، ≤25MB) والمسار نفسه `import-page`:
  ملف .pptx يُحوَّل بـ`Freeform/PptxPageBuilder.cs` **حتميًا بلا موديل** إلى صفحة شرائح واحدة مستقلة: كل شريحة `<section class="slide">`
  فيها SVG من `PptxSvgRenderer.RenderSlide` (الخلفيات والأشكال والصور والنصوص بمواضعها الأصلية) بالصور مضمّنة `DataUrl` (لا روابط
  خادم)، تنقّل أزرار/لوحة مفاتيح/لمس، شريط تقدّم، `window.goToSlide`، حدث `jeem:slide`، طباعة شريحة لكل صفحة، نسبة الأبعاد من الملف
  (`--ar`)؛ النوع يُفرض `slides` فيظهر شريط المصغّرات، وEMF/WMF تُذكر في `notes`. حد صفحة التصميم `FreeformStore.MaxHtmlBytes` رُفع من 4
  إلى 16MB ليتسع للصور. **شريط المصغّرات صار يعلن ارتفاعه** في `--jeem-strip-h` على `<html>` (ResizeObserver في jeem-slides.js، يتحدّث عند
  الإخفاء) لأنه ثابت أسفل الصفحة وكان يغطي أزرار التنقّل؛ صفحة PowerPoint تطرحه من ارتفاعها، ونظام الموديل لنوع شرائح يطلب حجز
  `padding-bottom:var(--jeem-strip-h,0px)`. اختبار `PptxPageBuilderTests` (ملف مولَّد + ملف المستخدم `imp/user.pptx` في scratchpad إن وُجد،
  بكتابة الصفحتين للفحص) 199/199. تحقق Playwright على الـ mock بملف المستخدم (15 شريحة): النوع شرائح، 15 SVG، الشريط يعلن 95px والأزرار فوقه
  لا تحته، التالي وgoToSlide يعملان، PDF مرفوض برسالة، بلا أخطاء وبلا تمرير أفقي على 390px.
- **تحويل تصميم إلى نوع آخر**: زر «⇄ تحويل إلى» في شريط أدوات التصميم (قائمة بالأنواع الأربعة الأخرى) → `POST /api/freeform/{id}/convert
  {kind, note?}`: نداء بناء واحد بالـ HTML الحالي كاملًا ورسالة «حوّل الصفحة من X إلى Y محتفظًا بالمحتوى والهوية» و`KindInstructions`
  النوع الجديد → **تصميم جديد** بعنوان «الأصل (شرائح)» ورسالتين، والأصل يبقى كما هو؛ الواجهة تفتحه (`ffConvert` → `ffOpen`) برسالة توضّح
  ذلك. اختبارات `FreeformImportConvertTests` (أقسام من الصفحات، إهمال الفارغة، الاسم المعطى، الاقتطاع، دخول المحتوى في النظام) 187/187.
  تحقق Playwright على الـ mock: البطاقة والتصنيف، خطأ بلا ملف، رفع HTML → شريحة النموذج ورسالة، الإرسال بمفتاح `u:1`، الحذف من المعاينة،
  تحويل مستند إلى شرائح يفتح نسخة جديدة بعنوانها وأزرار الشرائح والأصل باقٍ في المعرض. **القراءة الحية لملفات حقيقية والتحويل بموديل
  لم يُجرَّبا هنا**.
- **تعديل النص مباشرة من مساحة العمل**: زر «✏️ تعديل النص» في شريط أدوات التصميم يفعّل وضعًا على إطار المعاينة (شريط أصفر
  `#ff-edit-bar` بعدّاد العناصر المعدَّلة وزرّي «حفظ كإصدار جديد/إلغاء»). العدّة `FF_EDIT_KIT` (سكربت `#ff-edit-kit`) تُحقن مع صفحة
  التصميم المفتوحة فقط (`ffSandboxHtml(html, true)` في `ffRender`؛ المصغّرات والمعرض بلا عدّة) وتبقى خاملة حتى تتلقى
  `postMessage {jeemEdit:1,type:enable}` من الأم (الإطار معزول بلا `allow-same-origin` فالتواصل بـ postMessage فقط، ويتحقق كل طرف من
  `source`). النقر على نص يختار أقرب عنصر نصي (قائمة وسوم + أبناء نصية/تنسيقية فقط) ويجعله `contenteditable` بإطار أزرق؛ Escape ينهي
  العنصر؛ `input` يرسل `dirty` بعدد العناصر المعدَّلة. «حفظ» يطلب `get` فيعيد الإطار HTML الصفحة **بعد تنظيف كل أثر** (السكربتان
  المحقونان، نمط التحرير، سمات contenteditable/data-ff-*) مع DOCTYPE، فتُحفظ بـ `PUT /api/freeform/{id}` بملاحظة «تعديل نصي مباشر من
  مساحة العمل» كإصدار جديد ويُعاد الرسم؛ «إلغاء» يعيد رسم الأصل (بتأكيد إن وُجدت تعديلات). أي `ffRender` يخرج من الوضع. **قيد واعٍ**:
  يُحفظ DOM الصفحة كما يظهر لحظة الحفظ (بما غيّرته سكربتات الصفحة نفسها، كشريحة نشطة أو عناصر مخفية بفلتر)، لا المصدر الأصلي حرفيًا.
  تحقق Playwright: العدّة محقونة، التفعيل، النقر على العنوان يجعله قابلًا للتحرير، الكتابة تفعّل الحفظ بعدّاد، Escape، الحفظ يرسل
  HTML نظيفًا بالتعديل ويرفع الإصدار ويعرضه برسالة، والإلغاء يعيد الأصل.
- **عقد البيانات الحيّة وتحديث البيانات بلا موديل — المرحلة 1 ✅** (قرار المستخدم: الأرقام الفعلية تتغيّر فلا بد من تحديث بلا إعادة توليد):
  - **تسجيل استعلامات الموديل**: `AnalyticsTools.ToolObserver` (AsyncLocal) مراقب محيطي يراه `ExecuteToolAsync` في المزوّدين الثلاثة؛
    `FreeformBuilder.GenerateAsync` يضبطه حول `RunStagedAsync` فيسجّل كل `query_data` ناجح (SQL + النتيجة) بمفتاح `q1, q2…`
    (يُكمل مفاتيح الكتلة القائمة في الصفحة عند التعديل) ويُلحق بنص النتيجة «مفتاح هذه النتيجة في الصفحة: JEEM_DATA.qN».
  - **كتلة البيانات** (`Freeform/FreeformDataBlock.cs`): `<script type="application/json" id="jeem-data">{refreshedAt, queries:{qN:{sql,result}}}</script>`
    + `<script id="jeem-data-rt">` يعرّف `window.JEEM_DATA = {qN:{sql,rows,rowCount}}` و`JEEM_DATA_META`، تُحقن أول `<head>` لتسبق سكربتات
    الصفحة (`Inject` يستبدل القائمة؛ `Parse`؛ `NextKey`؛ `</` يُهرَّب). نظام الموديل عند وجود بيانات يُلزمه بقراءة الأرقام من
    `JEEM_DATA.qN.rows` في جافاسكربت الصفحة لا حرفيًا، وبعدم إدراج الكتلة بنفسه.
  - **التخزين**: جدول `dbo.FreeformQueries(DesignId, Key, Sql, Seq)` + عمودا `QueryCount`/`DataRefreshedAt` في `FreeformDesigns`
    (`SetQueriesAsync`/`QueriesAsync`)، تُحدَّث بعد كل توليد أنتج استعلامات. `FreeformResult.Queries`.
  - **`POST /api/freeform/{id}/refresh`** بلا موديل: يعيد تنفيذ الاستعلامات المسجَّلة (أو المقروءة من الكتلة لتصميم قديم) عبر
    `RunReadOnlyQueryAsync` (نفس بوابة الصلاحيات والقراءة فقط)، يستبدل الكتلة، يحفظ إصدارًا «تحديث البيانات (n من m)»؛ الاستعلام
    الفاشل يحتفظ بنتيجته القديمة ويُبلَّغ؛ 400 `noQueries` لتصميم بلا استعلامات، 502 إن فشلت كلها.
  - **الواجهة**: قائمة «🔄 تحديث البيانات» في شريط الأدوات ببندين: «تحديث البيانات» (بلا موديل إن وُجدت استعلامات مسجَّلة؛ وإلا — بطلب
    المستخدم أن يبقى متاحًا لكل الأنواع — يعيد البناء بالموديل هذه المرة فتُسجَّل استعلاماته وتصير التحديثات التالية فورية، بتلميح في البند
    ورسالة في المحادثة) و«إعادة البناء
    بالبيانات الحالية» (رسالة للموديل عبر `ffAsk` تطلب إعادة تنفيذ الاستعلامات بلا تغيير التصميم — للصفحات القديمة أو التي كتبت
    أرقامها حرفيًا). سطر الوصف يعرض «بيانات حيّة (n استعلام، آخر تحديث قبل …)».
  - **منشئ العروض**: `POST /api/decks/{id}/refresh` + زر «🔄 تحديث البيانات»: لكل شريحة بـ`query.sql` تُعاد الصفوف وتُصاغ **حتميًا**
    بـ`Decks/DeckDataRefresher.Apply` (أول عمود نصي تسميات، أول رقمي قيم؛ صف واحد = مؤشر لكل عمود؛ table كل الأعمدة؛ غيرها نقاط
    «تسمية: قيمة»؛ تنسيق N0/N2 إنجليزي) ثم `UpdateSpecAsync`؛ الواجهة تحفظ المسودّة أولًا إن كانت معدَّلة.
  - اختبارات `FreeformDataRefreshTests` (حقن/قراءة/استبدال/مفاتيح/تهريب، وصياغة الشرائح) 190/190. تحقق Playwright على الـ mock:
    صفحة بكتلة تعرض 5 → التحديث يرفع الإصدار ويعرض 99 ومعلومة «آخر تحديث الآن»؛ تصميم بلا استعلامات يعطّل البند السريع ويرسل
    إعادة البناء للموديل؛ شريحة مؤشرات تُحدَّث. **التزام الموديل الحقيقي بالعقد (قراءة JEEM_DATA لا الأرقام الحرفية) لم يُجرَّب هنا**
    وهو ما يحدد فائدة التحديث السريع؛ الاحتياط بالموديل يغطي غيره.
- **المرحلة 2 ✅ — شارة «تغيّرت البيانات» والتحديث التلقائي عند الفتح**: `Data/DataFingerprintService.cs` (singleton): `ExtractTables`
  يقرأ أسماء الجداول بعد FROM/JOIN (أقواس/مخططات/عربي) من SQL الاستعلامات، ويقبل منها ما في كتالوج المشروع فقط؛ لكل جدول
  `COUNT_BIG(*)` + `CHECKSUM_AGG(BINARY_CHECKSUM(*))` (يكتشف الإضافة والحذف والتعديل؛ عند نوع لا يقبله ← العدد وحده؛ جدول مفقود ←
  `missing`) بذاكرة 45 ثانية لكل جدول، والناتج تجزئة SHA-256 مقطوعة (24 حرفًا). تُحفظ `DataFingerprint` + `AutoRefresh` على
  `FreeformDesigns` (مع `SetQueriesAsync(..., fingerprint)` بعد كل بناء/تحديث) وعلى `Decks` (`SetDataStateAsync` بعد التوليد والتحديث).
  `GET /api/freeform/{id}` و`GET /api/decks/{id}` يعيدان `dataChanged` (null بلا بصمة؛ true إن اختلفت البصمة الحالية عن المحفوظة).
  `PUT /api/freeform/{id} {autoRefresh}` و`PUT /api/decks/{id}/auto-refresh {on}`. الواجهة: شارة صفراء «⚠️ تغيّرت البيانات — حدّث»
  (`#ff-stale`/`#dk-stale`) تظهر عند الفتح وتُطلق التحديث بلا موديل بالنقر وتختفي بعده؛ خيار «تحديث تلقائي عند الفتح» (مربع في
  قائمة 🔄 لتصميم حر، زر «⏱ تلقائي» في منشئ العروض) يجعل الفتح يحدّث فورًا إن تغيّرت البصمة برسالة في المحادثة. اختبار
  `DataFingerprintTests` (استخراج الجداول) 191/191. تحقق Playwright على الـ mock: الشارة تظهر لتصميم ببيانات وتختفي بعد التحديث
  ولا تظهر لتصميم بلا استعلامات، ضبط التلقائي يُحفظ، الفتح مع التلقائي يحدّث فورًا، والعرض: شارة + تلقائي + تحديث. **البصمة الحقيقية
  على SQL Server لم تُجرَّب هنا**؛ تكلفتها مسح كامل للجدول مرة كل 45 ثانية على الأكثر عند الفتح.
- **المرحلة 3 ✅ — الصفحات المنشورة حيّة** (`Presentations/PresentationLiveDataService.cs`): العرض المنشور يحمل مصدره داخل المنصة
  (`Presentation.SourceKind` = `freeform|deck` + `SourceId`، و`LiveData` تُفعَّل تلقائيًا عند النشر من مصدر له استعلامات؛ النشر المكرر
  يحترم إيقافًا سابقًا). `GET …/content` العام يحقن قبل التقديم آخر نتائج الاستعلامات المسجَّلة (تصميم حر: `FreeformDataBlock.Inject`؛
  عرض: `ReplaceDeckSpec` يستبدل `#deck-spec` مع الإبقاء على `brand/template/templateMode` المرفقة في الصفحة المنشورة) — الاستعلامات
  ثابتة من البناء وتُنفَّذ بـ`RunReadOnlyQueryAsync` بصلاحيات **صاحب التصميم** (`SourceSelection.AllEnabled` + `UserId`/`IsAdmin` من
  حسابه) لا بصلاحيات المشاهد المجهول، ومخزَّنة مؤقتًا لكل عرض `Presentations:LiveDataCacheSeconds` (60) ببوابة لكل عرض ضد التزاحم؛
  الفشل يعيد آخر نتيجة مخزَّنة. `GET …/live-data` (بلا دخول، بالمعرّف أو بالاسم) يعيد `{live, refreshedAt, queries:{qN:{rows,rowCount}},
  spec?, failed}` **بلا SQL**؛ `PUT {id}/live-data {on}` لصاحبه/المسؤول (400 لملف مرفوع بلا مصدر). الإبطال عند إعادة النشر والتحديث
  اليدوي. الجسر: `presentation.html` نوع `data` → `/live-data`؛ `jeem-page.js` يضيف `jeem.data.{get, load, watch, autoInterval, meta}`:
  `load` يحدّث `window.JEEM_DATA`/`JEEM_DATA_META` ويبث `document` حدث `jeem:data` عند تغيّر `refreshedAt`، والاستطلاع التلقائي كل 60
  ثانية داخل المنصة لصفحة فيها `JEEM_DATA` أو `#deck-spec` (`autoInterval = 0` يعطّله؛ يتوقف عند `live:false`). النشر من تصميم حر
  ببيانات يضمن `jeem-page.js` (`FreeformDataBlock.EnsurePageBridge`)، ونظام الموديل يطلب `render()` واحدة تُستدعى عند التحميل وفي
  مستمع `jeem:data`. الصفحة المنشورة للعرض (`DeckHtmlExporter`، الجسر مضمَّن متى وُجدت شرائح باستعلام) تعيد `JEEM_DECK.present` على
  الشريحة الحالية (`opts.startAt`) عند وصول `spec`؛ `present` صار يستبدل مستمع لوحة المفاتيح السابق بدل مضاعفته. الواجهة: زر
  «🟢 حيّة / ⚪ لقطة» في قائمة «العروض التقديمية» (لصاحبه؛ شارة فقط لغيره؛ لا يظهر لملف مرفوع)، ورسالة النشر تذكر أن الصفحة حيّة.
  توثيق المؤلفين في `docs/presentations-data.md`. اختبارات `PresentationLiveDataTests` (الحقن، استبدال المواصفة مع الهوية، كائن الجسر
  بلا SQL، الجسر مرة واحدة) 197/197، وتحقق Playwright بصفحة المشاهدة الحقيقية والجسر الحقيقي على الـ mock: المحتوى يُقدَّم بقيمة متغيّرة،
  الـ API محجوبة من داخل الإطار، `load` يحدّث القيمة ويبث الحدث، `watch` يعمل، العرض يعيد الرسم على الشريحة نفسها بقيم جديدة وسهم واحد
  ينقل شريحة واحدة، ملف مرفوع يعيد `live:false`، الزر يرسل PUT ويتبدّل، بلا تمرير أفقي على 390px. **قيد واعٍ**: كتلة `jeem-data`
  المحقونة في صفحة تصميم حر تحمل جمل SQL (قرار المرحلة 1) فهي مرئية لمن يفتح مصدر الصفحة المنشورة؛ مسار `live-data` لا يرسلها.
- **شريط مصغّرات الشرائح** (`wwwroot/js/jeem-slides.js`، لصفحات نوع «شرائح»): يكتشف `section.slide`/`.slide`/`[data-slide]` (≥2) ويضيف
  شريطًا ثابتًا أسفل الصفحة (`#jeem-strip`) بمصغّرات **حيّة** لكل شريحة (نسخة `cloneNode` بلا ids/سكربتات، مفروض عليها display/opacity
  وأبعاد الشريحة الظاهرة، مصغّرة بـ `transform: scale(128/العرض)`)، ورقم كل شريحة وعدّاد «4 / 8»، وزر «▾ إخفاء/▴ إظهار المصغّرات» يُحفظ في
  localStorage (داخل srcdoc المعزول يفشل بصمت). التنقّل بالنقر: `window.goToSlide(i)` إن عرّفتها الصفحة (نظام الموديل لنوع شرائح صار
  يطلبها ويطلب تضمين `<script src="/js/jeem-slides.js">`)، وإلا نقل صنف التفعيل المكتشف (on/active/current/…) إلى الشريحة المستهدفة
  وبث `jeem:slide`، وإلا `scrollIntoView`. المزامنة مع تنقّل الصفحة نفسها بـ MutationObserver على class + scroll. مخفي في الطباعة.
  الحقن: المعاينة (`ffSandboxHtml(html, withEditor, kind)` تضيف `<script id="ff-slides-kit">` إن غاب، وفي المعاينة التفاعلية للمعرض)،
  وعند النشر في الخادم (`FreeformDataBlock.EnsureSlidesKit` في `Publish` لنوع slides). عدّة التعديل المباشر تحذف `#ff-slides-kit`/
  `#jeem-strip`/`#jeem-strip-style` قبل الحفظ فلا يُحفظ الشريط في الصفحة. صفحات العرض التوضيحي للشرائح تعرّف `goToSlide`. تحقق
  Playwright: 8 مصغّرات بمحتوى لعرض توضيحي، النقر ينقل ويحدّث العدّاد، زر «التالي» في الصفحة يحدّث المصغّرة النشطة، الإخفاء يعمل،
  وصفحة شرائح بلا `goToSlide` تنتقل بنقل الصنف، والحفظ من وضع التعديل بلا أثر للشريط.

## مساعد «جيم» في صفحة العرض المنشور — المرحلة 1 ✅ (`Presentations/PresentationAssistantService.cs`)

- **الفكرة** (طلب المستخدم): زر عائم في صفحة المشاهدة العامة `presentation.html` (خارج الإطار المعزول، فالميكروفون والـ API
  بكوكي المشاهد يعملان) بشخصية جيم: **الشعار نفسه بمساراته الأصلية** مع عينين على الضلع العلوي وفم على الضلع السفلي السميك
  (`wwwroot/js/jeem-assistant.js`، SVG مضمّن `CHAR`) في أربع حالات بـ`data-state` على الجذر `.ja`: `idle` (رمش)، `listening`
  (عينان واسعتان + حلقة نبض)، `thinking` (الأعمدة تنبض)، `speaking` (عينان مبتسمتان + فم يفتح ويغلق + الأعمدة تتمايل).
  `prefers-reduced-motion` يلغي كل الحركة (مُتحقَّق: 0 حركات). المساعد **يعرف الملف كله لا الشريحة الظاهرة**.
- **السياق** حتمي من الصفحة المنشورة كما تُقدَّم (بعد حقن البيانات الحيّة) — `ExtractContext(html, title)`: `#deck-spec` → نص كل
  شريحة (عناوين، نقاط، أعمدة، مؤشرات، رسوم، جداول حتى 60 صفًا، مهام، ملاحظات المتحدث)؛ وإلا `section.slide` (≥2) → أسطر كل شريحة
  بما فيها نص SVG/foreignObject (PowerPoint المرفوع كما هو)؛ وإلا `DeckImporter.Extract` للصفحة (فقرات/جداول/روابط/كتل بيانات).
  ثم صفوف `JEEM_DATA` (`FreeformDataBlock.Parse`) **بلا جمل SQL** (الكتلة تُحذف قبل الاستخراج). سقف 60K حرف، مخزَّن 10 دقائق ما لم
  يتغيّر `UpdatedAt` (`Invalidate` عند الاستبدال وتبديل البيانات الحيّة).
- **النداء**: `AskAsync` نداء مرحلي واحد **بلا أدوات** (`StagedCall` بـ `SystemPrompt` ثابت يلزمه بالمحتوى فقط وبأسلوب يصلح للقراءة
  الصوتية ويعيد `{reply}`)، المحادثة السابقة (≤12 دورة) تُرسل من المتصفح ولا تُخزَّن في الخادم، ويُسجَّل في الاستهلاك باسم
  **صاحب العرض** بعنوان `[مساعد العرض] <العنوان> — <السؤال>`. **لا وصول لقاعدة البيانات في هذه المرحلة** (المرحلة 2 المقترحة:
  `query_data` بصلاحيات صاحب العرض).
- **التحكم**: `Presentation.Assistant` (افتراضيًا true) و`AssistantPolicy` (`anyone`/`members`) — أعمدة جديدة في `Presentations`،
  `PUT /api/presentations/{id}/assistant {enabled, policy}` لصاحبه/المسؤول؛ قائمة «العروض التقديمية» فيها قائمة «جيم: لأي شخص /
  لأعضاء المنظمة / موقوف» (`data-pres-assistant`). عام بلا دخول (بالمعرّف أو `by/{org}/{slug}`): `GET …/assistant`
  (`enabled/allowed/reason`) و`POST …/assistant/ask {question, history}` (401/403 بحسب السياسة، 429 عند السقف، 503 بلا مزوّد).
  السقف في الذاكرة (`RateLimiter` بنافذة منزلقة): `Presentations:AssistantPerViewerPer10Min` (20، بمفتاح المستخدم أو IP) و
  `Presentations:AssistantPerPresentationPerHour` (300).
- **الصوت**: Web Speech API (`SpeechRecognition` بلغة الصفحة `ar-SA`) للاستماع — الضغط على الميكروفون يشغّل **حلقة صوتية**: استماع →
  إرسال → قراءة الرد بـ`SpeechSynthesis` (صوت عربي إن وُجد، مقطّع جملًا لتفادي توقف Chrome بعد ~15 ثانية) → استماع مجددًا حتى يضغط
  المستخدم مرة أخرى أو يغلق اللوحة؛ زر 🔊 يوقف القراءة التلقائية للكتابة. بلا دعم: زر الميكروفون يختفي ويبقى الشات. يحتاج HTTPS.
  المحادثة تُحفظ في `sessionStorage` (`ja:<apiBase>`). **فقاعة تنبيه عند الفتح** (طلب المستخدم): بعد ~0.9 ثانية من تحميل الصفحة تظهر
  فوق الزر `#ja-hello` («👋 أهلًا! أنا جيم، أعرف محتوى هذا العرض كاملًا…») مع إيماءة للشخصية (`.ja-wave`)؛ النقر عليها يفتح اللوحة، ✕ يغلقها،
  وتختفي وحدها بعد 12 ثانية؛ مرة واحدة لكل جلسة متصفح (`ja:<apiBase>:hello`) ولا تظهر إن كانت هناك محادثة سابقة أو المساعد غير مسموح. الموديل غير معطَّل: الصفحة بلا سمة `enabled` تزيل الزر كليًا؛ `allowed=false`
  يعرض السبب ورابط الدخول ويعطّل الإدخال.
- اختبارات `PresentationAssistantTests` (مواصفة عرض، شرائح SVG، بيانات بلا SQL، اقتطاع، سياسة، سقف) 214/214. تحقق Playwright على خادم
  وهمي بصفحة المشاهدة الحقيقية: الزر والحالات الأربع، الترحيب والاقتراحات، سؤال من اقتراح ثم بالكتابة مع تاريخ المحادثة، تنسيق الرد
  (غامق/قائمة)، 429 برسالة، الاحتفاظ بالمحادثة بعد إعادة التحميل، Escape يغلق، `members` بلا دخول يعرض الإشعار ويعطّل الإدخال، موقوف
  يزيل الزر، بلا أخطاء JS وبلا تمرير أفقي على 390px. **الرد الحي بموديل حقيقي والصوت في متصفح حقيقي لم يُجرَّبا هنا**
  (لا مفتاح ولا ميكروفون في بيئة الجلسة) — أول تجربة على جهاز المستخدم.

## ربط ملف برابط عام ومزامنته (الخيار الأول من مزامنة الجداول السحابية) — المرحلة 1 ✅

- **القرار** (بعد عرض ثلاثة خيارات: رابط عام / OAuth لملفات الحساب الخاصة / مصدر خارجي مركزي): البدء بالرابط العام على مستوى
  الملف في «الملفات». الشرط: الملف مشارك «أي شخص معه الرابط» (عرض). الملفات الخاصة داخل الحساب تحتاج الخيار الثاني (لم يُنفَّذ).
- **الخادم** `Repository/LinkedFileFetcher.cs`: `ToDownloadUrl` يحوّل رابط المشاركة إلى تنزيل مباشر — Google Sheets (`/spreadsheets/d/<id>`
  → `export?format=xlsx` للملف كاملًا، أو `format=csv&gid=` لورقة بعينها إن حمل الرابط `gid`)، OneDrive (`1drv.ms`/`onedrive.live.com` →
  `api.onedrive.com/v1.0/shares/u!<base64url>/root/content`)، SharePoint (`download=1`)، Dropbox (`dl=1`)، وأي رابط مباشر. `FetchAsync`
  ينزّل (≤60MB، عميل `linked-files` بمهلة 60 ث) ويكشف الصيغة بـ`DetectExtension` (توقيع zip = xlsx، اسم الملف في الترويسة، نوع المحتوى،
  الرابط، أو شمّ CSV) ويرفض صفحة HTML برسالة «غالبًا الملف غير مشارك للعامة» و401/403 برسالة «يتطلب تسجيل دخول». ثم يمر الملف على
  `UploadParser.ParseAsync` نفسه (`FetchAndParseAsync` في `RepositoryController`) فيدخل مسار الرفع العادي بلا استثناء.
- **النموذج والمخزن**: أعمدة `SourceUrl/LastSyncedAt/SyncError` على `repo_Files` (`SourceProvider` محسوب: google/onedrive/sharepoint/
  dropbox/url)، `SetSourceAsync`، `GetColumnsAsync` (من `ColumnsJson`)، `FindAsync`.
- **API**: `POST /api/repository/link {url, displayName, category, description}` → ملف جديد في المشروع بالرابط؛
  `POST files/{id}/sync` → إعادة التنزيل و`UpdateDataAsync` (هوية الجدول ثابتة كـ«تحديث الملف») ويعيد `{rows, columns, addedColumns,
  removedColumns, schemaChanged}`؛ الفشل 502/500 مع `keptOld` ويُسجَّل `SyncError` على الملف والجدول القديم يبقى؛ `DELETE files/{id}/link`
  فكّ الربط (يبقى الملف لقطة). تحليل المعاني في الخلفية بعد الربط والمزامنة.
- **الواجهة**: زر «🔗 ربط ملف برابط» بجوار «رفع ملفات جديدة» يفتح `link-modal` (رابط، اسم، تصنيف، وصف)؛ صف الملف المرتبط يحمل شارة
  `.link-badge` «🔗 Google Sheets · آخر مزامنة» (حمراء ⚠️ مع سبب الفشل في التلميح) وزر 🔄 يصير «مزامنة من الرابط» (`syncLinkedFile`) بدل
  رفع نسخة؛ `linkToast` يعرض النتيجة ويفصّل الأعمدة المضافة/المختفية ويذكّر بـ«تصحيح الأعمدة» عند تغيّر البنية.
- **سلوك تغيّر الأعمدة** (سؤال المستخدم): الصفوف والقيم آمنة؛ عمود جديد آمن؛ إعادة تسمية/حذف عمود تكسر الاستعلامات المحفوظة عليه
  (لوحات/تصاميم/عروض) وتُعالَج بـ«تصحيح الأعمدة» أو إعادة البناء — نفس سلوك «تحديث الملف» اليدوي.
- اختبارات `LinkedFileFetcherTests` (تحويل الروابط، المزوّد، كشف الصيغة) و`LinkedFileFetcherHttpTests` (CSV من تصدير Google بعنوان
  التصدير الصحيح، صفحة HTML → رسالة المشاركة، 403 → رسالة الدخول) 206/206. تحقق Playwright على الـ mock: الزر، خطأ رابط خاص، الربط يرسل
  الحقول ويغلق النافذة ويضيف الصف بالشارة وزر المزامنة، المزامنة تعرض تقرير الأعمدة، الفشل يبقي الجدول ويعلّم الشارة، بلا أخطاء وبلا تمرير
  أفقي على 390px. **التنزيل الحي من Google/OneDrive لم يُجرَّب هنا** (لا ملف مشارك متاح من بيئة الجلسة) — أول تجربة على جهاز المستخدم.
  **المرحلة 2** (مزامنة مجدولة كل ساعة/يوم بخدمة خلفية) لم تبدأ.
- **تصحيح بعد أول تجربة للمستخدم** (الملف المنزَّل فيه تبويب واحد والعربي مشوّه): رابط Google بـ`gid` كان يصدّر CSV لورقة واحدة فيُحفظ
  «الأصل» بلا باقي التبويبات، وExcel على Windows يفتح CSV UTF-8 بلا BOM بترميز النظام. الآن: (1) Google يُصدَّر **xlsx كاملًا دائمًا**
  (`ToDownloadUrl`؛ `GoogleGid` تلميح فقط) فـ«تنزيل الملف» يعيد كل التبويبات بترميز صحيح؛ (2) اختيار الورقة **بالاسم**: `TabularFileParser.
  ReadXlsx(file, sheetName)` و`SheetNames(file)` و`UploadParser.ParseAsync(..., sheetName)`، حقل «الورقة» في نافذة الربط
  (`LinkFileRequest.Sheet`)، وأعمدة `SourceSheet`/`SourceSheetsJson` على الملف (`RepositoryFile.SourceSheets`)؛ `POST files/{id}/sync
  {sheet?}` يغيّر الورقة ويزامن، وصف الملف يعرض قائمة `.link-sheet-pick` بالأوراق حين تزيد عن واحدة (التغيير يزامن فورًا) أو شارة
  الورقة؛ ورقة غير موجودة أو فارغة → رسالة تذكر الأوراق المتاحة؛ (3) تنزيل أي CSV من `files/{id}/download` يُسبق بـBOM إن غاب.
  **قيد باقٍ**: الملف الواحد = جدول واحد (ورقة واحدة)؛ لقراءة عدة تبويبات اربط الملف أكثر من مرة بورقة مختلفة لكل ربط. اختبار
  `MultiSheetXlsxTests` 207/207، وPlaywright: قائمة الأوراق الثلاث تظهر وتغييرها يرسل `{sheet}` للمزامنة.

## حقن سكربتات المنصة في صفحة مرفوعة: قبل «آخر» `</body>` (2026-11-05)

- **علّة حقيقية**: صفحة المستخدم «دور الإدارة في مشروع الخوارزمي» المرفوعة كما هي في «تصميم حر» ظهرت ميتة بالكامل (لا تبويبات ولا
  قائمة مسؤوليات، والمهام على «جارٍ التحميل…») بخطأ `Unexpected identifier 'click'`. السبب أن `ffSandboxHtml` كانت تحقن سكربتَي
  إصلاح الروابط وتعديل النصوص قبل **أول** `</body>`، والصفحة تحمل `</body>` داخل نص JavaScript مقتبس (دالة تعيد بناء الملف عند الحفظ)،
  فوقع الحقن داخل سكربت الصفحة وكسره. الشيء نفسه كان في `FreeformDataBlock.EnsurePageBridge/EnsureSlidesKit` (وأسوأ: `Regex.Replace`
  لكل التكرارات) وزر الطباعة.
- **الإصلاح**: `ffInsertBeforeBodyEnd` في app.js و`FreeformDataBlock.InsertBeforeBodyEnd` في الخادم (آخر `</body>` بلا حساسية لحالة
  الأحرف، وبلا وسم يُلحق في النهاية) — أي حقن جديد في صفحة ليست من إنتاجنا يمر منهما. اختبارات `FreeformInjectionTests`.
- **جسر بيانات العرض داخل معاينة المحرر** (app.js، قبل مستمع `jeemEdit`): صفحة تضمّن `jeem-page.js` تنادي `jeem.store/viewer/data`
  بـ postMessage ولم يكن في المحرر من يجيبها (مهلة 15 ثانية ثم خطأ). الآن `ffBridgeHandle`: تصميم منشور → وكيل إلى
  `/api/presentations/{presentationId}/…` (نفس عقد `presentation.html`) فيتشارك المحرر والمشاهدون البيانات؛ غير منشور → مخزن مؤقت
  في localStorage المتصفح بمفتاح `jeem.ff.<designId>:<key>` بإصدار وتعارض 409؛ مصغّرات المعرض تُجاب للعرض فقط (`set` → forbidden).
- **تكييف تبويب المهام في صفحة المستخدم** (إصدار 2 من التصميم): بدل `claude.use("artifact")`/تنزيل الملف صار يضمّن `jeem-page.js`
  ويقرأ/يحفظ تحت المفتاح `tasks` (البذرة من كتلة `tk-data` حين لا قيمة محفوظة، تعارض يطلب إعادة الحفظ بعد المراجعة، `onChange` للتحديث
  من الآخرين، وخارج المنصة عرض فقط). تحقق حي بـ puppeteer-core على Chrome المثبّت (Playwright غير متاح): المسؤوليات والعلاقات والمهام
  تعمل في المعاينة، الحفظ يكتب الإصدار 1 ويظهر بعد إعادة الفتح، وصفر أخطاء من صفحة المستخدم (الأخطاء الباقية في الكونسول من تصاميم
  أخرى ولّدها الموديل).
- **«فتح في نافذة» كان يفقد الجسر**: الصفحة تُفتح من رابط `blob:` فلا تُحل الروابط الجذرية (`/js/jeem-page.js` لا يُحمَّل ولا حتى `fetch`
  نسبي) فتتحول الأجزاء القابلة للتعديل إلى عرض فقط — وهذا ما ظنّه المستخدم «العرض لا يسمح بالتعديل». `ffStandaloneHtml` في app.js
  يجعل الروابط الجذرية مطلقة إلى أصل المنصة ويضيف `<meta name="jeem-design" content="<designId>" data-presentation="<presentationId?>">`؛
  و`jeem-page.js` خارج الإطار: مع `data-presentation` ينادي واجهة بيانات العرض المنشور مباشرة (`viaApi` بأصل المنصة من `platformOrigin()`
  = مصدر السكربت نفسه، بكوكي المستخدم) فتتشارك النافذة ومساحة العمل والرابط العام البيانات؛ وبلا نشر يستخدم `jeem.ff.<designId>:<key>`
  في localStorage، وهو المفتاح نفسه الذي يستخدمه جسر المحرر. تحقق حي: النافذة تحفظ الإصدار 7 في بيانات العرض ومساحة العمل تراه.
  الصفحة المنشورة نفسها (`/p/…`) كانت تسمح بالإضافة والحذف والتعديل لأي زائر (سياسة `anyone`) — مُتحقَّق مسجَّلًا وبدون تسجيل.
- **قائمة «التصاميم»/«العروض» في رأس شريط المحادثة كانت مبتورة** (علّة أبلغ عنها المستخدم بصورة): الزر عند الحافة الداخلية للشريط
  (يسار النافذة في RTL) و`.dropdown-panel` العادية تمتد من الزر إلى الخارج فتُقطع عند حافة النافذة، وبنودها `nowrap`. الحل في ذيل
  jeem.css كقائمة محادثات الاستفسارات: `#ff-list-panel.dropdown-panel`/`#dk-list-panel.dropdown-panel` تمتد إلى الداخل
  (`inset-inline-end:0`) بعرض `min(340px, 100vw - 32px)` وتمرير رأسي، وبنود «التصاميم» تلتف (`white-space:normal`) والإصدار سطر مستقل.
  تحقق puppeteer على 1400px و390px: 16 و10 بنود كلها داخل النافذة بلا بتر.
- **نافذة «ربط ملف برابط» (الملفات)**: حقولها كانت بعرضها الافتراضي الضيق لأنها بلا غلاف `label` كنموذج «تفاصيل الملف». بطلب المستخدم
  أُضيفت في app.css قواعد `#link-form` (الحقول `width:100%` بمظهر حقول `#file-form`، تسمية فوق كل حقل، النافذة 520px). تحقق puppeteer:
  الحقول الأربعة بعرض النموذج كاملًا على 1200px و390px بلا تمرير أفقي.

## وثيقة المميزات (`docs/features/`)

- 11 فصلًا بالعربية تصف خصائص المنصة كمنتج (ما هي، المشكلة، كيف تعمل، ما يميزها، القيود) — لا دليل استخدام.
  الجمهور: الإدارة والعملاء والمبيعات والمنضمون الجدد. الفهرس في `docs/features/README.md`.
- **عند إضافة ميزة جديدة** تُحدَّث بطاقتها في الفصل المناسب هنا مع توثيقها التقني في هذا الملف؛ والملحق
  `11-limits.md` يذكر ما لم يُختبر حيًّا وما على الخارطة — يُحدَّث عند إنجاز أي بند منه.

## مظهر معاينة المعرض (JR) للوحة المحادثة الحقيقية (2026-11-18)

- **القرار** (بعد مقارنة المستخدم بين لوحة المحادثة ولوحة تصميم حر): لوحة المحادثة أصح بيانات لكن أقل جمالًا؛ بدل ابتكار مظهر
  جديد أُخذ مظهر معاينة المعرض (`dash-render.js`/قواعد `.jr-*` في jeem.css) الذي أعجب المستخدم وطُبِّق على اللوحة الحقيقية بالكود،
  مع بقاء رسوم DevExtreme للبيانات الحقيقية. المعاينة نفسها لم تُمس.
- **الرأس** (`renderDashboard` في app.js): شعار المشروع واسمه من `/api/decks/brand` (`state.dashBrand`: undefined = لم تُطلب،
  تُجلب مرة بـ `loadDashBrand` وتُعاد الرسم عند وصولها؛ تُبطَل عند حفظ الهوية من منشئ العروض)، سطر «لوحة متابعة · <الجهة>»،
  عنوان = اسم اللوحة المحفوظ أو أول جملة من الملخص (`firstSentence`)، الملخص كسطر فرعي `.dash-sub`، وشارة «بيانات حيّة» حين يحمل
  أي عنصر `query.sql` (لا في المشاركة). مصادر البيانات بلا بادئة المخطط `proj_<id>.`.
- **رأس البطاقة** (`widgetShell`): `.widget-head-t` يضم العنوان وتحته `.widget-src` (أول جملة من `source` بلا بادئة المخطط، سطران
  كحد أقصى) لغير المؤشرات؛ نافذة ⓘ تبقى للنص الكامل.
- **المؤشرات**: التسمية أعلى (عنوان البطاقة بخط صغير)، الرقم 28px تحتها بمحاذاة البداية، ثم التغيّر والخط المصغّر. المؤشرات
  المتتالية تُجمَّع في `.kpi-row` (شبكة فرعية تملأ العرض، حتى 4 في الصف، 2 على الجوال) حتى لا تتمدد البطاقة إلى ارتفاع رسم مجاور.
  السحب والإفلات يعمل بمعرّف العنصر فلا يتأثر بالتجميع.
- **الشبكة**: 12 عمودًا — رسم 6، جدول/`large`/`full`/مقارنة 12، `small` 3؛ container query على `#dash-pane`: 640–1039px كل شيء 12
  والمؤشرات نصف الصف، <640px كلها 12 والمؤشرات اثنان في الصف. **الفلاتر** بطاقة واحدة برقائق 26px، **التبويبات** أزرار داخل إطار.
- تحقق Playwright بالواجهة الحقيقية ولوحة الفرص الحقيقية (7 عناصر، 5 فلاتر) على 1900 و1280px مع هوية وهمية: الرأس والشعار
  والشارة والفلاتر والمؤشرات في صفها والرسوم SVG وسطر المصدر، بلا أخطاء JS وبلا تمرير أفقي. 225/225 اختبار.

## محرّك الرسوم الموحّد `wwwroot/js/jeem-viz.js` بدل DevExtreme (2026-11-18)

- **القرار** (المستخدم: «وحّد الرسوم على محركنا»): رسوم لوحات المحادثة صارت SVG خالصًا يكتبه كودنا بلغة معاينة المعرض
  (dash-render.js)، بلا مكتبة خارجية ولا ترخيص ولا شريط «For evaluation purposes only». **العقد نفسه** الذي كانت عليه
  `jeem-dx.js`: `JEEM_VIZ.mount(holder, spec, {theme, fmt})` يعيد `{instance, holder, rendered, svg(), freeze(), destroy()}`،
  والمواصفات من app.js كما هي (`bar|line|multi|forecast|pie|radial|funnel|treemap|scatter|sparkline` + `onPointClick`).
  `window.JEEM_DX` اسم بديل للمحرك نفسه، ومراجع app.js صارت `JEEM_VIZ`. تصدير PPTX يلتقط SVG المحرك كما كان (`freeze()`
  يزيل صنف الحركة).
- **اللغة البصرية**: أشرطة أفقية مدوّرة بقيمة عند الطرف (فئات ≤14 غير زمنية)، أعمدة رأسية للزمني (أول فترة في جهة البداية
  بحسب RTL)، خط ناعم (Catmull-Rom) بعلامات مفرغة ومساحة باهتة، حلقة بإجمالي في المنتصف وشرح جانبي بالنسب، تنبؤ بخط متقطع
  ونطاق ثقة مظلل، مقياس دائري، قمع، خريطة شجرية (squarify)، مبعثر، خط مصغّر بلون الحالة. الألوان من رموز الهوية
  (`--chart-1..4`، `--line`، `--ink-3`…) تُقرأ من الحاوية. تلميح `.viz-tip` عند التحويم/التركيز، شرح `.viz-legend` تحت الرسم،
  حركة دخول بـ CSS (`.viz-anim`) تُلغى مع `prefers-reduced-motion`، وإعادة رسم عند تغيّر عرض الحاوية (ResizeObserver).
  **الـ SVG بسمة `direction="ltr"`** حتى تكون `text-anchor` هندسية؛ الاتجاه المنطقي يُحسب من `isRtl()` في كل رسم.
- **الحذف بإذن المستخدم («احذف»)**: `wwwroot/lib/devextreme/` (الحزمة ووصفة البناء وREADME)، `js/jeem-dx.js`،
  `Controllers/UiConfigController.cs` (مسار `/api/ui-config.js`)، قواعد `.dx-license`، والاسم البديل `JEEM_DX`. إعداد
  `DevExtreme:LicenseKey` في user-secrets لم يعد يُقرأ. القسم القديم «DevExtreme بدل ApexCharts» أعلاه تاريخ فقط.
- تحقق Playwright بالواجهة الحقيقية: لوحة بكل الأنواع (مؤشران بخط مصغّر، أشرطة أفقية، أعمدة زمنية، خط، دائري، مكدّس، خطوط
  متعددة، مقياس، قمع، خريطة شجرية، مبعثر) + تنبؤ على الخط: 10 رسوم SVG، 5 شروح، تلميح عند التحويم، نقر يضيف تصفية متقاطعة،
  `widgetToPptxInput` يعيد صورة لكل رسم، صفر حركات مع تقليل الحركة، بلا أخطاء وبلا تمرير أفقي. 225/225 اختبار.

## «لوحات المتابعة» نوعًا سادسًا داخل تصميم حر (2026-11-18) — المرحلة (أ) ✅

- **القرار** (المستخدم: «انقل صفحة المحادثة واللوحات داخل صفحة التصميم الحر بنوع جديد اسمه لوحات المتابعة»، ثم «لما أضغط على
  لوحة متابعة تظهر لي تلقائيًا الـpopup الخاصة بإضافة لوحة متابعة وأختار منها النموذج فيتعمل له attachment زي بالضبط اللي بيحصل
  في تصميم حر»؛ البند في القائمة يبقى اختصارًا؛ الاستفسارات لا تنتقل): بطاقة `.ff-kind.k-track` (`data-kind="track"`،
  `FF_KINDS.track`) سادسة في «اصنع شيئًا جديدًا» تعمل **كباقي الأنواع تمامًا**:
  1. النقر يفتح معرض تصميم حر نفسه (`openFfGallery('track')`): `ffGalItems()` لهذا النوع = كتالوج لوحات المحادثة كاملًا
     (`DASH_GALLERY`: العادية والتفاعلية) عبر `ffTrackItems()`/`ffTrackTemplateOf(d)` (مفتاح `chat:<id>` + `dashId` + `renderKind`)،
     فتُعرض ببطاقة معرض المحادثة الحرفية (`catalogOf` يحلّ `dashId`) مع شارة الجدوى، والتصنيفات `DASH_GAL_CATS`، والمعاينة الأصلية
     `openDashGalPreview` بعروضها التوضيحية. بطاقة «بلا نموذج» تصير «لوحة متابعة بلا نموذج»، وبطاقتا الرفع تُخفيان (نتاجهما صفحات).
  2. «استخدم هذا النموذج» في مضيف تصميم حر بنوع track → `ffSetTemplate(ffTrackTemplateOf(d))`: شريحة «📎 نموذج: X · لوحات المتابعة»
     فوق صندوق الكتابة (أو «✦ لوحة متابعة بلا نموذج»)، وتظهر في أول رسالة بالمحادثة، وتثبت بلا ✕ بعد البناء (`locked` في
     `ffRenderAttach` يشمل لوحة متابعة مبنية). المفتاح `chat:` لا يُحفظ في أي مكان — اللوحة تُحفظ في السجل كالمعتاد.
  3. الإرسال (`ffAsk` → `ffTrackAsk`): أول رسالة `startBlankDashboard()` ثم `ffTrackMount()` ثم المسار الحقيقي نفسه — نموذج عادي:
     `ask(text, text, {kind:'dashboard', key})`؛ نموذج تفاعلي: `useInteractiveTemplate(d, true)` (معامل `inPlace` جديد يتخطّى
     `showScreen('chat')`، و`openHistoryEntry` لا يبدّل الشاشة حين `ffTrackActive()`)؛ بلا نموذج: `ask(text)`. ما بعدها متابعة
     `ask(text)` على اللوحة نفسها.
  4. **الاستضافة**: عقدة `#dash-pane` نفسها (بشريط أدوات اللوحة: عرض/تحرير، تفعيل، تحديث، تصدير، مشاركة، نشر، تراجع، الأدوار،
     إضافة عنصر) تُنقل إلى `#ff-body` (`ffTrackMount`، مرساة تعليق `ffDashAnchor`، صنف `in-ff` عليها و`ff-track` على `#ff-pane`
     يخفي أزرار تصميم حر ويقفل العنوان)، وتعود إلى `#screen-chat` عند الخروج (`ffTrackUnmount`: من `showScreen` لأي شاشة أخرى،
     و«جديد»، و`ffOpen`، وبطاقات الأنواع). الحالة واحدة (`state.dashboard`/`state.messages`) فبند «المحادثة واللوحات» في القائمة
     يعرض اللوحة نفسها مع تبويب الاستفسارات. الرسائل: `renderMessages` تستدعي `ffTrackRenderMessages()` فتُعرض `state.messages`
     في `#ff-messages` بشكل `.cc-msg` مع فقاعة «يبني اللوحة…» أثناء `state.loading`؛ رأس اللوحة (`ffTrackHead`) اسم اللوحة/أول جملة
     من الملخص + عدد العناصر. `loadFreeformScreen` يعيد التركيب إن كان `FF.track`.
  - مضيف المرحلة 1 القديم (نقل `#screen-chat` كاملًا، `#ff-dash-host`/`ffOpenTrack`/`ffCloseTrack`/`ff-track-mode`) **أُزيل**.
  - تحقق Playwright بالواجهة الحقيقية (1600 و390، كتالوج وهمي ببطاقتين): المعرض يفتح من البطاقة بعنوان «لوحات المتابعة جديد»
    و«لوحة متابعة بلا نموذج» وإخفاء الرفع، المعاينة ثم الاستخدام يرفق الشريحة بالمفتاح `chat:pl-sales-pipeline`، الإرسال يحمل
    `template:{kind:'dashboard',key}` إلى `/api/chat` ويعرض اللوحة (7 عناصر، 5 رسوم، شريط أدوات اللوحة) داخل لوحة تصميم حر
    بعرض 920px والرسائل في الشريط بالمرفق، المتابعة ترسل `currentDashboard`، الاختصار من القائمة يعيد `#dash-pane` إلى شاشة
    المحادثة باللوحة نفسها والاستفسارات ظاهرة، العودة لتصميم حر تعرض البداية، «بلا نموذج» يبني من الصفر، والنموذج التفاعلي
    يستدعي `from-template` ويفتح السجل داخل تصميم حر؛ بلا أخطاء وبلا تمرير أفقي. 225/225.
- **شريط الأدوات بمنطق تصميم حر ✅** (طلب المستخدم: لا تصدير/طباعة/مشاركة/نشر للتكاملات/تفعيل/أدوار هنا؛ «نشر» يعطي رابطًا؛
  و«تحويل إلى» للتحويل إلى HTML): في وضع track يُخفى `#dash-toolbar` ويظهر `#ff-track-actions` في رأس لوحة تصميم حر (`.ff-track-actions`
  بدل `.dk-actions` الأصلية) بأزرار تنادي منطق اللوحة الموجود: **✏️ تعديل** (`ffTrackSetEdit` = `state.editMode` نفسه؛ شريط أصفر
  `#ff-tk-edit-bar` في `#ff-body` بإضافة عنصر/تراجع/إعادة/إنهاء يعكس `btn-add-widget`/`undo`/`redo` وحالة الحفظ التلقائي عبر
  MutationObserver على `#autosave-status`)، **📱 جوال** (صنف `mobile` على `#dash-pane`: عرض 390px و`align-self:center` — في عمود flex
  تلغي `margin:auto` التمدد فلا بد من `width` صريح — وcontainer query تعطي تخطيط الجوال)، **🔄 تحديث البيانات** (قائمة: بلا موديل =
  `btn-refresh`؛ بالموديل = متابعة بالمحادثة)، **⇄ تحويل إلى** (الأنواع الأربعة)، **🔗 الرابط**، **نشر**.
  - **نشر بنقرة** (`ffTrackPublish`): يفعّل اللوحة إن لم تكن مفعّلة (`activateHistoryEntry`؛ رابط المشاركة يحتاج مالكًا) ثم
    `POST /api/share` بـ`expiresAt: null` (بلا انتهاء) و`historyId`؛ (لاحقًا: الرابط مقروء ويُحدَّث في مكانه — انظر بند «رابط مقروء» أدناه؛ لم يعد يُستخدم localStorage). اللوحة التفاعلية تُستثنى برسالة.
  - **تحويل إلى HTML** (`POST /api/freeform/from-dashboard {kind, title, dashboard:{name,summary,widgets}}` في `FreeformController`):
    `DashboardSpecText` يصوغ المواصفة نصًا (العنوان، الملخص، ولكل عنصر العنوان/النوع/الحجم/الصفحة/SQL/البيانات ≤2500 حرف، ≤60K) ويطلب
    من الموديل تنفيذ SQL كل عنصر بـ`query_data` (فتُسجَّل في `JEEM_DATA` ويعمل التحديث بلا موديل)، ثم `GenerateAsync` بالنوع المطلوب →
    تصميم جديد «<العنوان> (لوحة)» يُفتح في تصميم حر (`ffTrackConvert` → `ffOpen`)، واللوحة الأصلية تبقى في السجل. اختبار
    `FreeformFromDashboardTests`.
  - **تعديل المسميات مباشرة** (في الشاشتين، وضع التحرير فقط): `makeInlineEditable` تجعل عنوان كل عنصر (`widgetShell`/`NoDataPlaceholderCard`/
    المقارنة) وعنوان اللوحة `#dash-head-title` وملخصها `#dash-head-sub` (يُعرض فارغًا بعنصر نائب في التحرير) `contenteditable` بإطار متقطع
    (`[data-inline-edit]`)؛ Enter يحفظ وEscape يلغي والسحب يُعطَّل أثناء التركيز. `renameWidget` (تراجع + حفظ تلقائي؛ يحفظ `templateTitle`
    الأصلي أول مرة ويستخدمه `rebuild`)، `renameDashboard` (`d.name` + `PUT /api/history/{id}/rename`)، `setDashboardSummary` (تراجع + حفظ).
  - تحقق Playwright بالواجهة الحقيقية (1600 و390): الشريط بأزراره السبعة وشريط اللوحة مخفي، التعديل يُظهر الشريط الأصفر و9 عناوين قابلة
    للتحرير، إعادة تسمية عنصر تُحفظ (`PUT` بالعنوان الجديد) مع `templateTitle`، عنوان اللوحة → `rename` ويظهر في رأس تصميم حر، الملخص →
    حفظ، إنهاء التحرير يبقي العناوين، الجوال 388px، النشر → تفعيل ثم `share` بلا انتهاء ورابط في الرسالة وزر الرابط و«تحديث النشر» (الثاني
    يوقف الأول)، التحديث السريع، التحويل → `from-dashboard` بـ7 عناصر ويفتح التصميم الجديد والمعاينة و`#dash-pane` يعود لشاشة المحادثة،
    والتعديل المباشر يعمل في الشاشة الأصلية أيضًا؛ بلا أخطاء وبلا تمرير أفقي. 227/227. **التحويل الحي بموديل لم يُجرَّب هنا.**
- **رابط المشاركة بصفحة كصفحة العرض المنشور ✅** (طلب المستخدم بصورة: الرابط كان يعرض المنصة بقائمتها الجانبية وشريطها):
  `bootSharedView` يضيف `body.is-shared` فتُخفى القائمة الجانبية والشريط العلوي وشريط أدوات اللوحة (app.css)، و`#shared-banner`
  صار رأسًا بشكل `presentation.html` نفسه (شعار `logo-lockup`، العنوان، «من <المنشئ>» من `creatorName` الذي صار يعيده
  `GET /api/share/{id}`، و«لوحة مشارَكة عبر جيم»). **مساعد جيم** يُحمَّل عند الحاجة (`sharedMountAssistant` → `js/jeem-assistant.js`)
  على `apiBase=/api/share/{id}`: `GET …/assistant` (مفعّل ومسموح لأي من يفتح الرابط، اللقطة عامة أصلًا) و`POST …/assistant/ask`
  في `ShareController` بسقف `TryConsume("share:"+id)` وسياق حتمي `PresentationAssistantService.DashboardContext(question, summary,
  widgetsJson)` (الملخص وكل عنصر بعنوانه ونوعه وتبويبه وصفوف بياناته حتى 60، **بلا SQL**) عبر `AskWithContextAsync` (المشترك مع
  العروض؛ الاستهلاك باسم صاحب الرابط). اختبار `ShareAssistantTests`. تحقق Playwright (1600 و390): الرأس بالشعار والعنوان والمنشئ،
  لا قائمة ولا شريط ولا أدوات، 7 عناصر و5 رسوم بعرض الصفحة، زر جيم يظهر والسؤال يصل ويُعرض الرد؛ بلا أخطاء وبلا تمرير أفقي.
- **رابط المشاركة حيّ: الفلاتر والتحديث في الصفحة نفسها ✅** (طلب المستخدم): `GET /api/share/{id}` يعيد `live` = صاحب الرابط
  موجود وصلاحيته على البيانات صالحة (`DashboardAccessService.CheckEligibilityAsync`) وفي اللوحة عنصر له `query.table`. مع `live`
  تضبط الواجهة `state.shareLive`: شريط الفلاتر التفاعلي (`buildFilterBar`) بدل القراءة فقط، التصفية المتقاطعة بالنقر، زر «🔄 تحديث»
  في رأس الصفحة (`#shared-refresh`) مع «آخر تحديث …» (`formatUpdatedAt`)، و**تحديث تلقائي عند كل فتح** (`applyFilters` بعد الرسم).
  `applyFilters` في وضع المشاركة الحيّ ينادي `POST /api/share/{id}/widgets/{index}/refresh` (`[AllowAnonymous]`، الفهرس فقط لا جدول
  ولا SQL من العميل — الشكل نفسه لـ`HistoryController.RefreshWidget`) ويُنفَّذ **بصلاحية صاحب الرابط** (`GetEffectiveSelectionAsync(owner)`)
  لا المشاهد؛ 409 عند فقدان الصلاحية فتعود الصفحة لقطة مجمّدة بسبب ظاهر (`filterRefreshWarning` يُحتفظ به، الزر يختفي، الشريط للقراءة).
  الحفظ التلقائي معطَّل في وضع المشاركة (`scheduleAutosave`) حتى لا ينشئ مستخدم مسجَّل يفتح الرابط لوحة جديدة في سجله. الرابط بلا
  `live` (ملف/صاحب محذوف) يبقى كما كان: لقطة مجمّدة بفلاتر للقراءة. التغيّر عن الفترة السابقة (`loadKpiDeltas`) يبقى معطّلًا هنا
  (مساره يتطلب دخولًا). تحقق Playwright (1600 و390 + حالة مجمّدة): 6 نداءات عند الفتح والمؤشر يتغيّر، اختيار رقاقة فلتر يعيد
  التنفيذ بالفلتر، زر التحديث يعيد النداءات ويحدّث الوقت، 409 يحوّل الصفحة لقطة بسبب، والمجمّدة بلا زر وبلا فلاتر تفاعلية؛ بلا أخطاء
  وبلا تمرير أفقي. 229/229.
- **رابط مقروء للوحة المشارَكة `/d/{اسم-المنظمة}/{اسم-اللوحة}` ✅** (سؤال المستخدم: لماذا `?share=` بينما تصميم حر `/p/…`):
  أعمدة `Slug/OrganizationId/HistoryId` على `SharedDashboard` (ترحيل بالحلقة نفسها)، `SharedDashboard.PublicPath(orgSlug)` (بلا اسم →
  `/?share=<id>` القديم الذي ما زال يعمل) و`IsLive`. `POST /api/share`: الاسم من `request.Slug` أو العنوان بـ`PresentationSlug.Normalize`
  وفريد داخل المنظمة بلاحقة `-2`…؛ **وإن كان للوحة (`historyId`) رابط حيّ لصاحبه حُدّثت لقطته في مكانه** (`UpdateSnapshotAsync`) وأُعيد
  `{url, updated:true}` — فلا رابط جديد عند «تحديث النشر» ولا localStorage. `GET /api/share/by/{org}/{slug}` (عام) يحلّ الاسم ثم يعيد ما
  يعيده `GET {id}` (+`slug`/`url`)، `GET /api/share/links` (للمستخدم: `{id, historyId, url}` للروابط الحيّة) تغذّي `FF.trackLinks` (شارات
  «منشور» في المعرض وزر الرابط و«تحديث النشر»)، وقائمة `GET /api/share` تعيد `url` فتنسخه «روابط المشاركة». Program.cs: `/d/{org}/{slug}`
  يخدم `index.html` و`boot` يلتقط المسار ويمرّر `bySlugApi` إلى `bootSharedView`. `loadHistory` صار يتحقق أن الرد مصفوفة. اختبارات
  `SharePublicPathTests`. تحقق Playwright: النشر يعرض `/d/ثقة/لوحة-الإيرادات` والثاني «حُدّثت… على رابطها نفسه»، المعرض يعلّم «منشور» من
  الخادم، وفتح `/d/ثقة/x` يحلّ المعرّف ويعمل الرابط الحيّ كاملًا؛ بلا أخطاء. 231/231.
- **علّة حقيقية عند المستخدم: صفحة المشاركة ظهرت بقائمة جانبية وشريط قديم وزر عرض/تحرير** رغم أن index.html وapp.js الجديدين
  وصلا (زر التحديث ظاهر): المتصفح احتفظ بنسخة قديمة من `app.css` بلا قواعد `body.is-shared` (لا ترويسات تخزين على الملفات الثابتة).
  الإصلاح في Program.cs: `UseStaticFiles` بـ`Cache-Control: no-cache` (إعادة مصادقة بـETag عند كل تحميل) لكل الملفات الثابتة؛ وبالمناسبة
  `index.html` تحت مسار بمقطعين (`/d/{org}/{slug}` و`/o/{slug}` عبر الاحتياطي) كان يحلّ مراجعه النسبية `css/ js/ img/` تحت المسار
  نفسه فتفشل — `SpaIndex.Rewrite` يجعلها مطلقة (`/css/ …`) بلا مساس بمراجع `<use href="#i-…">`، ويُخدم من `/d/…` و`MapFallback`
  (مخزَّن بحسب وقت تعديل الملف). `sharedMountAssistant` يحمّل `/js/jeem-assistant.js` بمسار مطلق. اختبار `SpaIndexTests` (على ملف
  index.html الحقيقي). 233/233.
- **إزالة رأس اللوحة (2026-11-19، بقرار المستخدم)**: كتلة `.dash-head` (شعار الهوية، «لوحة متابعة · الجهة»، العنوان، الملخص، سطر
  المصادر/آخر تحديث/شارة «بيانات حيّة») لم تعد تُرسم في البناء ولا في المشاركة؛ `renderDashboard` يضع `state.dashMeta`
  (`sourceText/updatedText/metaHtml`) و`renderDashMetaLine()` يعرض المصادر وآخر تحديث في **شريط أدوات اللوحة** (`#dash-meta-line` في شاشة
  المحادثة)، وفي **رأس الصفحة المشارَكة** بجوار زر التحديث (`#shared-sources`)، وفي **رأس لوحة «لوحات المتابعة»** (`ff-meta`: العدد ·
  المصادر · آخر تحديث). تعديل اسم اللوحة في وضع track صار من `#ff-title` نفسه (blur → `renameDashboard`) بدل العنوان داخل اللوحة؛ الملخص
  لم يعد ظاهرًا (يبقى محفوظًا ويُحدَّث بالمحادثة). قواعد `.dash-head` في CSS باقية بلا استخدام. تحقق Playwright: لا `.dash-head`، المصادر في
  الشريط والمعرض والمشاركة، إعادة التسمية من `ff-title` تحفظ. 233/233.
- **علّة أبلغ عنها المستخدم: معرض تصميم حر «يرجع لأسفل وحده» عند التمرير لأعلى**: المصغّرات iframes بـ`sandbox="allow-scripts"`،
  وصفحة مولَّدة تنادي `focus()`/`scrollIntoView()`/`autofocus` (شرائح، لوحات بمؤقّتات) فيمرّر المتصفح الحاوية الأم `#ff-start`
  لإظهار العنصر المركَّز داخل الإطار ولو بلا تفاعل. الإصلاح: `ffSandboxHtml(html, withEditor, kind, quiet)` — مع `quiet` يُحقن
  `FF_THUMB_QUIET` أول `<head>` يعطّل `focus`/`scrollIntoView`/`scrollTo`/`scrollBy`/`window.focus` ويزيل `autofocus` داخل المصغّرة فقط
  (بطاقات المعرض `ffLoadThumb` ومعاينة النموذج الحيّة `ffLiveInto`)؛ المعاينة الحقيقية والتفاعلية بلا تغيير. تحقق Playwright بصفحة
  تنادي focus/scrollIntoView كل 300ms: بلا الإصلاح يثبت `scrollTop` عند 918، ومعه يبقى حيث يضعه المستخدم (0/400/0).
- **المرحلة (ب) ✅ — اللوحات المحفوظة في «تصاميمك السابقة»** (علّة أبلغ عنها المستخدم بصورة: لوحات المتابعة لا تظهر هناك):
  `ffShowStart` يحمّل السجل (`loadHistory` إن لم يكن محمَّلًا) ويدمج `ffTrackDesigns()` في `FF.list`: لكل لوحة محادثة محفوظة بطاقة
  `k-track` بمعرّف `h:<historyId>` وعنوانها سؤال اللوحة، ومصغّرتها **شبكة عناصرها** بـ`dashGalThumb` (أيقونة المحرك للتفاعلية) داخل
  `.ff-card-dash` بدل iframe، و«عُدِّل/فُتح قبل…»، وتبويبات الكل/تصاميمي/مشترك معي (`userId`) والتصفية بالنوع والبحث تعمل عليها؛ تبويب
  «المنشورة» وشارة «🔗 منشور» وزر نسخ الرابط من رابط المشاركة المسجَّل محليًا (`jeem.trackShare:<id>` → `/?share=<id>`). النقر →
  `ffOpenTrack(historyId)`: يرفق شريحة النموذج من `templateId` (أو «بلا نموذج»)، `startBlankDashboard` + `ffTrackMount` +
  `openHistoryEntry` (الحالة نفسها التي تعرضها شاشة المحادثة، مع رسالتي السجل في الشريط). تحقق Playwright (1600 و390): 3 بطاقات (مستند
  + لوحتان) بمصغّرة 7 عناصر وأيقونة المحرك، المنشورة تعرض اللوحة ذات الرابط، «مشترك معي» تعرض لوحة المستخدم الآخر، التصفية بالنوع،
  الفتح يعرض اللوحة داخل المضيف بعنوانها وشريحتها ورابطها و«تحديث النشر»، «جديد» يعيد المعرض، والتفاعلية تُفتح بلا تعديل؛ بلا أخطاء
  وبلا تمرير أفقي.

## تصدير PDF وPowerPoint من صفحات النشر لكل الأنواع (2026-11-19)

- **القرار** (طلب المستخدم): صفحتا النشر العامتان (العرض المنشور `/p/…` بأنواعه: تصميم حر، منشئ العروض، صفحة HTML/PowerPoint
  مرفوعة؛ واللوحة المشارَكة `/d/…`) تحملان في الرأس «تصدير: PDF · PowerPoint» بلا دخول.
- **PDF** = الصفحة نفسها عبر نافذة طباعة المتصفح (حفظ كـ PDF): في `presentation.html` رابط `…/content?print=1` يُفتح في تبويب جديد،
  و`PresentationsController.Content` مع `print=1` يحقن `<script id="jeem-print">` ينادي `window.print()` بعد التحميل (عبر
  `FreeformDataBlock.InsertBeforeBodyEnd`). في اللوحة المشارَكة الزر `#shared-exp-pdf` → `window.print()` (قواعد الطباعة تخفي
  `#shared-banner`).
- **PowerPoint**: `GET /api/presentations/{id}/export/pptx` و`GET by/{org}/{slug}/export/pptx` (`[AllowAnonymous]`، نفس شروط عرض
  المحتوى): لعرض من منشئ العروض (`SourceKind == deck`) المواصفة من `DeckStore` + هوية المشروع → `DeckPptxBuilder.SlidesFromSpec`
  (دالة جديدة: الرسم جدولًا بقيمه، المؤشرات، المهام ☑/☐، العمودان، الاقتباس، الصور data URL) ثم `DeckPptxBuilder.Build`؛ ولأي صفحة
  أخرى (تصميم حر، HTML مرفوع، PowerPoint مرفوع) الاستخراج الحتمي `DeckImporter.Extract` → `ToSpec` → الشرائح نفسها (بلا موديل). اسم الملف
  من العنوان (`SafeFileName`). في اللوحة المشارَكة الزر `#shared-exp-pptx` → `exportPptx(btn)` (صارت تقبل زرًا؛ التسمية = `.btn-label`
  أو نص الزر؛ العنوان من `d.name` أو عنوان الرأس) على `POST /api/export/pptx` الذي صار `[AllowAnonymous]` (المدخلات صور SVG وبيانات
  الواجهة فقط).
- اختبار `DeckPptxSpecExportTests` (التخطيطات والرسم جدولًا وبناء الملف) 234/234. تحقق Playwright (1400 و390): روابط التصدير في
  صفحة العرض بالمسارين الصحيحين وتنزيل من رابط PowerPoint، وفي اللوحة المشارَكة الزران في الرأس، PDF يستدعي الطباعة، PowerPoint يرسل
  7 عناصر بالعنوان ويعيد التسمية بعد التنزيل، بلا أخطاء وبلا تمرير أفقي. **جودة ملف PowerPoint لصفحة HTML حرّة تعتمد على الاستخراج
  الحتمي** (عناوين/فقرات/جداول) لا على الشكل البصري.
- **حالة الانتظار في «لوحات المتابعة» كتصميم حر** (علّة أبلغ عنها المستخدم بصورة: هيكل الأعمدة الداكن القديم): `dashLoadingHtml(title, hint)`
  في app.js بدل الهيكل الثابت في المواضع الثلاثة (`renderDashboard` بلا لوحة أثناء `state.loading`، `useInteractiveTemplate`،
  `mountInteractiveDashboard`): داخل وضع track تضع طبقة `ccSetLoading('ff-pane', …)` نفسها (دوّار + عنوان + تلميح) وتترك اللوحة فارغة،
  وفي شاشة المحادثة الهيكل كما كان؛ `dashLoadingDone()` تزيلها عند وصول لوحة عادية (`renderDashboard`) أو بعد جلب بيانات التفاعلية أو
  عند الخطأ. تحقق Playwright: الطبقة بعنوانها أثناء البناء ولا هيكل، تزول عند الرسم (7 عناصر)، وشاشة المحادثة تحتفظ بالهيكل.
- **المؤشرات أول اللوحة في وضع التحرير أيضًا** (علّة أبلغ عنها المستخدم بصورة: في التحرير نزلت البطاقات تحت الجدول بينما في العرض
  فوقه): كان `renderDashboard` يقدّم المؤشرات في العرض فقط ويبقي الترتيب الحقيقي في التحرير. الآن الترتيب نفسه في الوضعين، و`reorderWidget`
  يتجاهل إسقاط مؤشر على غير مؤشر (أو العكس) بلا تراجع لأنه بلا أثر ظاهر؛ السحب بين المؤشرات وبين باقي العناصر يعمل كما كان. تحقق
  Playwright بترتيب مخزَّن `kpi,kpi,table,kpi×3`: صف واحد بخمسة مؤشرات ثم الجدول في العرض والتحرير، سحب مؤشر على مؤشر يعيد الترتيب،
  وإسقاطه على الجدول لا يغيّر شيئًا.
- **مؤشّر تحميل بشعار جيم في كل حالات الانتظار** (طلب المستخدم: «لوجو جيم وأعمدته تتحرك فوق وتحت… في البناء أو العرض»):
  `wwwroot/js/jeem-loader.js` (`JEEM_LOADER.html({title, hint, size})`/`node`): الشعار بمساره الأصلي (المسار نفسه الذي يستخدمه المساعد)
  وأعمدته الثلاثة `rect.jl-bar` بحركة `jl-bounce` (scaleY من القاعدة بتأخير متدرج)، CSS تُحقن مرة (`#jeem-loader-css`)، وبلا حركة مع
  `prefers-reduced-motion`. يُستخدم في: طبقة `ccSetLoading` (تصميم حر، منشئ العروض، لوحات المتابعة) بدل `.cc-spin`؛ `dashLoadingHtml`
  في شاشة المحادثة (حاوية `.empty.dash-wait` بالشعار والعنوان والتلميح بدل هيكل الأعمدة الداكن — الهيكل احتياط فقط إن غاب السكربت)؛
  طبقة «جارٍ التحديث…» عند تطبيق الفلاتر (`#dash-loading .jl-slot` تُملأ عند الإقلاع)؛ الرابط العام للوحة (`bootSharedView` قبل الجلب)؛
  وصفحة العرض المنشور `presentation.html` (`#loading` حتى حدث `load` للإطار، ثم يُغطّي منطقة المحتوى تحت الشريط بـ`frame-pending`).
  تحقق Playwright (1600 و390): الشعار بثلاثة أعمدة وحركة `jl-bounce` في لوحات المتابعة والمحادثة والفلاتر والرابط العام وصفحة العرض،
  يزول عند وصول المحتوى، وبلا حركة مع تقليل الحركة؛ بلا أخطاء.
- **قائمة ⋮ لآخر بطاقة في الصف كانت تُبتر** (علّة أبلغ عنها المستخدم بصورة): `positionWidgetMenu` كانت تحصر القائمة داخل النافذة فقط،
  بينما اللوحة داخل `#dash-pane` بـ`overflow:auto` يقصّ ما يخرج عنه، فقائمة البطاقة عند الطرف الداخلي (أقصى اليسار في RTL) تمتد
  خارج الحاوية. الآن الحدود = أقرب حاوية قصّ (`#dash-pane`/`.modal-body`/`[data-menu-clip]`) ∩ النافذة، وعند الخروج من الطرف
  تنعكس القائمة لتمتد من الزر إلى الداخل. تحقق Playwright (1600 و390): قبل الإصلاح القائمة تبدأ عند 286 والحاوية عند 400؛ بعده
  تبدأ عند 438 داخل البطاقة.

## اختيار المصادر في تصميم حر (2026-11-19) ✅

- **الطلب**: «اختيار مصدر محدد لبناء اللوحة عليه» في صفحة تصميم حر. بنّاء تصميم حر كان يرى كل مصادر المشروع دائمًا
  (`CurrentAsync` بـ`GetEffectiveSelectionAsync(user, null)`).
- **الخادم**: عمود `SourcesJson` على `FreeformDesigns` (JSON بشكل `SourceSelection {systems, files}`؛ null = كل المصادر المتاحة للمستخدم)،
  يُعرض للواجهة باسم `sources` (`FreeformDesign.SourcesSelection`). الطلبات `generate`/`convert`/`from-dashboard` تقبل `sources`
  (قوائم صريحة؛ عدم الإرسال = لا تغيير)، و`PUT {id}` يقبل `sources` أو `clearSources:true`. الاختيار الفعلي = المرسل في الطلب وإلا
  المحفوظ مع التصميم وإلا الكل، **مقيَّدًا دائمًا بصلاحيات المستخدم** (`CurrentAsync(ct, requested)` → `GetEffectiveSelectionAsync`
  يقاطع مع المسموح). يُحفظ عند الإنشاء (`CreateAsync(..., sourcesJson)`) ويُحدَّث عند إرساله مع تعديل، و`refresh` بلا موديل يلتزم بالمحفوظ.
  `SourcesJsonOf` (internal) تعيد null للكل. اختبارات `FreeformSourcesTests` 237/237.
- **الواجهة** (app.js، قسم «مصادر البيانات لتصميم حر»): شريحة «🗄️ المصادر: X» بجوار شريحة النموذج في `#ff-attach` (`#ff-src-chip`؛ صنف
  `narrow` عند التقييد) تفتح لوحة `#ff-src-panel` فوق صندوق الكتابة: «كل المصادر» + الأنظمة + الملفات من `state.systems/sourceFiles`
  (تُحمَّل بـ`loadSources` عند الحاجة). `FF.sources` null = الكل؛ التغيير على تصميم مفتوح يُحفظ فورًا بـ`PUT` ويُعرض في سطر الوصف
  (`ff-meta`)؛ أول رسالة في المحادثة تحمل «🗄️ المصادر: …» مع مرفق النموذج؛ `generate` يرسل `sources` للتصميم الجديد، و`convert`
  و`from-dashboard` كذلك؛ إعادة فتح تصميم تستعيد الاختيار من `design.sources`. **في وضع لوحات المتابعة** الشريحة تتحكم في اختيار
  المحادثة نفسه (`state.onSystems/onFiles`، لأن اللوحة تُبنى عبر `/api/chat`) فتصل المصادر المختارة مع الطلب كما في شاشة المحادثة.
- تحقق Playwright (1600 و390): الشريحة «كل المصادر» ثم لوحة بـ3 مصادر، إلغاء اثنين → «المصادر: الفرص.csv» و`generate` يحمل
  `{systems:[], files:[f2]}` والرسالة والوصف يذكرانها، فتح تصميم محفوظ يستعيد الشريحة و«كل المصادر» يرسل `clearSources`، وفي لوحات
  المتابعة يصل `/api/chat` بـ`{systems:[crm], files:[f1]}`؛ بلا أخطاء وبلا تمرير أفقي. **البناء الحي بموديل على مصادر مقيَّدة لم يُجرَّب هنا.**
- **إعادة تسمية التبويبات (صفحات اللوحة) مباشرة** (طلب المستخدم بصورة): في وضع التحرير اسم **التبويب النشط** داخل `buildDashTabs` يصير
  `span` قابلًا للكتابة بـ`makeInlineEditable` (التبويبات الأخرى تبقى أزرار تبديل؛ النقر على الاسم القابل للتعديل لا يبدّل التبويب).
  الاسم ليس كيانًا مستقلًا بل حقل `page` في كل عنصر، فـ`renameDashboardPage(old, new)` يحدّثه في كل عناصر الصفحة مع التراجع والحفظ
  التلقائي وينقل `state.activeDashTab`. «عناصر أخرى» (التبويب الضمني للعناصر بلا صفحة) وصفحة المشاركة لا تُعدَّل. تحقق Playwright
  (1600 و390): بلا تحرير لا شيء قابل للكتابة، في التحرير النشط فقط، الكتابة + Enter تغيّر الاسم في التبويب وفي 4 عناصر مع تراجع،
  Escape يلغي، التبديل ينقل القابلية للتبويب الآخر، وإنهاء التحرير يزيلها.
