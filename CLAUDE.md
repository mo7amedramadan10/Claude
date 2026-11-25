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
- **«اختر مصدرًا يدويًا» يبدأ من اختيار المصدر لا النوع** (علّة أبلغ عنها المستخدم بصورة): كان `openFillPlaceholderWizard` يفتح المعالج
  على «ماذا تريد أن تضيف؟» رغم أن نوع العنصر معروف من القالب. الآن `wizardKindOfWidgetType` يحوّل نوع العنصر إلى شكل المعالج
  (kpi→مؤشر، bar/funnel/treemap…→مقارنة، line/area/forecast→اتجاه، pie/donut/radial→توزيع، table/progress-table→جدول) ويبدأ من
  `loading`→اختيار الجدول مباشرة؛ نوع بلا شكل مقابل (مراحل، مصفوفة اتجاه…) يبقى على خطوة الاختيار. تحقق Playwright: عنصر bar يفتح
  «من أي مصدر بيانات؟» بخيارين و`kind=comparison` بلا خطوة نوع، وعنصر stages يفتح خطوة النوع.

## نافذة «إضافة عناصر» باختيار متعدد (2026-11-19) ✅

- **الطلب**: زر «+ إضافة عنصر» في شريط التحرير يفتح معرضًا بأسلوب معرض النماذج فيه العناصر الجاهزة ومكتبة المؤشرات والعناصر
  الخام، باختيار أكثر من عنصر وإضافتها معًا.
- **الخادم**: `POST /api/widgets/columns/batch {sources, lang, items[{ref?: TemplateRef, title, type, prompt}]}` (≤12) في
  `WidgetColumnsController`: لكل عنصر مواصفة `BuiltinDashboardWidgetSpec` — نموذج عنصر (`ref.kind=widget`) أو مؤشر من المكتبة
  (`ref.kind=kpi` بحقوله المعروضة) عبر `TemplatePromptService.ResolveAsync` نفسه الذي تستخدمه المحادثة (النص المحلول + مسودة الجدول/SQL
  للمؤشر)، أو حر بعنوانه ونوعه؛ العناوين تُجعل فريدة داخل الدفعة (البنّاء يفهرس بالعنوان)، ثم `StagedTemplateBuilder.BuildOneAsync` لكل
  عنصر بالتوازي بحد 3 (`SemaphoreSlim`). يعيد `items[{index, title, widget|null, error}]`. `DashboardSpec.AllowedWidgetTypes` صار internal.
- **الواجهة** (app.js، قسم «نافذة إضافة عناصر»): `#add-widget-modal` (index.html، يعيد استخدام `.dash-gallery-box` وقواعدها) بثلاثة
  أقسام في `#aw-cats`: عناصر خام (`WIZARD_KINDS`)، عناصر جاهزة (`TEMPLATES` من الكتالوج)، مكتبة المؤشرات (`KPI_LIB` بتصفية التصنيف
  ونوع القياس والبحث وعرض 60 فأكثر، مع احترام إيقاف المسؤول `KPI_ROW_OVERRIDES/KPI_CAT_OFF`). البطاقات `.aw-card` بتبديل الاختيار
  (`AW.sel`) وعلامة ✓، وشريط سفلي `#aw-picked` بشرائح المختار (✕ للإزالة) وزر «إضافة (n)». البحث مشترك بين الأقسام. عند الإضافة
  (`awAddSelected`): الجاهزة والمؤشرات تُضاف فورًا كحوامل `noData` «جارٍ البناء من بياناتك…» (تراجع واحد) ثم `batch` يستبدلها بالنتيجة أو
  يترك الحامل بالسبب الصادق؛ والخام تُفتح لها نوافذ المعالج بالتتابع (`AW.queue` + `awNextRaw` من `closeWizard`) على خطوة اختيار
  المصدر مباشرة بنوعها. زر «+ إضافة عنصر» القديم (`btn-add-widget`) يفتح النافذة الجديدة، والمعالج القديم ما زال يُستخدم للخام.
- تحقق Playwright (1600 و390): النافذة بأقسامها الثلاثة (5 / 2 / 3,839)، اختيار خام + جاهز + مؤشر من البحث «رضا» → 3 شرائح و«إضافة (3)»
  وعدّاد لكل قسم، الإزالة من الشريط، الإضافة تغلق النافذة وتظهر حاملان «جارٍ البناء» ثم `batch` بعنصرين (kpi+widget) يستبدل الأول
  بعنصر مبني ويترك الثاني بسبب الخطأ، ثم معالج المؤشر الخام يفتح على «من أي مصدر بيانات؟»؛ بلا أخطاء وبلا تمرير أفقي. 237/237.
  **البناء الحي بموديل على دفعة لم يُجرَّب هنا.**
- **تصنيفات المكتبة في الشريط الجانبي** (طلب المستخدم: كمعرض اللوحات): عند اختيار «مكتبة المؤشرات» تظهر تحتها قائمة فرعية `.aw-subs`
  («كل التصنيفات» + كل تصنيف بعدد مؤشراته الفعّالة من `awKpiCatCounts`، بأزرار `.aw-sub[data-kpi-cat]`) فيُختار التصنيف ثم المؤشر؛ القائمة
  المنسدلة القديمة في شريط الفلاتر أُزيلت وبقيت رقائق «نوع القياس». عدّاد بند المكتبة صار عدد المؤشرات الفعّالة لا كل الصفوف. وعلّة أُصلحت:
  شريط الفلاتر وزر «عرض المزيد» (بعدّاد سالب) كانا يظهران في تبويبي الخام والجاهزة لأن قواعد display أقوى من `.hidden`/`[hidden]`.
- **بطاقة «قيد البناء» بشعار جيم** (طلب المستخدم بصورة: الحامل القديم «جارٍ البناء…» بزر «اختر مصدرًا يدويًا» لم يوضح أن الموديل
  يعمل): `NoDataPlaceholderCard` لعنصر `building` يرسم `.widget.widget-building` (إطار متقطع أزرق) بـ`JEEM_LOADER` («جيم يبني هذا العنصر
  الآن…» + تلميح «يقرأ بياناتك ويكتب الاستعلام بالموديل ثم ينفّذه») بلا أزرار؛ وبعد رد `batch` أي حامل بلا نتيجة يخرج من حالة البناء
  بسبب «لم يُعد الخادم نتيجة» بدل البقاء معلّقًا. تحقق Playwright بتأخير 900ms على `batch`: البطاقة بالشعار الثلاثي الأعمدة وبلا زر يدوي
  أثناء الانتظار، ثم تتحول إلى `noData` بالسبب.
- **معاينة شكل العنصر عند التحويم** (طلب المستخدم: «أعرف شكل المؤشر وأنا أختاره بلا زر عرض»): نافذة منبثقة `#aw-peek` (تُنشأ مرة داخل
  `.dash-gallery-box`) تظهر بعد 220ms من التحويم أو التركيز بلوحة المفاتيح على أي بطاقة (`awBindPeek`)، وتختفي بالمغادرة/التمرير/الإغلاق.
  المحتوى `awPeekHtml`: مؤشر (من المكتبة أو الخام أو الجاهز) = بطاقة مؤشر بعنوانه ورقم توضيحي بحسب نوع القياس (`awSampleValue`: نسبة
  72.4٪، زمن 4.2 يوم، مالية 3.7M ر.س، درجة 8.6/10، عدد 1,284) وتغيّر وخط مصغّر؛ وغيره رسم SVG صغير بأرقام ثابتة (`awPeekSvg`: bar/line/
  pie/funnel/table) بألوان الهوية. شكل العنصر الجاهز يُستنتج من أيقونة الكتالوج وكلمات العنوان (`awReadyShape`: filter→قمع، share→دائري،
  chart/calendar→خط، table→جدول، kpi→مؤشر) لأن الكتالوج لا يحمل نوع الرسم. سطر «أرقام توضيحية — يُبنى من بياناتك بعد الإضافة» أسفلها.
  الموضع: جهة البداية من البطاقة إن اتسع، وإلا الجهة الأخرى، وإلا تحتها، داخل حدود النافذة. على شاشات اللمس (`(hover: none)`) زر 👁
  على البطاقة يفتح/يغلق المعاينة ولا يختار. تحقق Playwright (1400 + جوال 390 بلمس): المعاينة تظهر بالتحويم والتركيز وتختفي بالمغادرة،
  القمع 4 أشرطة والخط مساران والدائري 4 شرائح والجدول، كلها داخل النافذة، النقر يختار، وزر العين على اللمس يبدّل بلا اختيار؛ بلا تمرير أفقي.

## استيراد ملف إلى عرض موجود في منشئ العروض (2026-11-22) ✅

- **الطلب**: «أعدّل عرضًا موجودًا برفع عرض آخر». الاستيراد القديم (`POST /api/decks/import`) ينشئ عرضًا جديدًا دائمًا (رابط نشر جديد).
- **الخادم**: `POST /api/decks/{id}/import` (multipart `file` + `mode` + `apply = replace|append`) في `DecksController`: الاستخراج المشترك
  `ImportSpecAsync` (نفس `DeckImporter.Extract/ToSpec` وأوضاع `understand/refine/none`) ثم تُستبدل شرائح العرض أو تُلحق بعد الحالية مع
  الاحتفاظ بالعنوان/العنوان الفرعي/اللون/القالب/وضع القالب والمعرّف ورابط النشر؛ حد `DeckSpec.MaxSlides` (30) وإلا 400؛ `UpdateSpecAsync`
  و`SetDataStateAsync(null)` (الشرائح المستوردة بلا استعلامات). يعيد `{deck, spec, kind, pagesRead, apply, slidesBefore, slidesAdded, warnings,
  published}`. الرابط المنشور لا يتغير إلا بـ«تحديث النشر».
- **الواجهة**: زر «📥 استيراد إلى هذا العرض» في شريط أدوات العرض المفتوح (معطَّل في حالة البداية كباقي الأزرار) يفتح نافذة الاستيراد نفسها
  (`openDeckImportModal(into)`، `dkiInto`) بعنوان «استيراد إلى هذا العرض» وبلا حقل العنوان ومع قائمة `#dki-apply` (استبدال/إلحاق) وتنبيه بأن
  الرابط يحتاج «تحديث النشر». بعد الاستيراد تبقى الرسائل ورابط النشر، والمؤشر ينتقل لأول شريحة ملحقة، ورسالة في المحادثة بالعدد قبل/بعد
  وتذكير بتحديث النشر إن كان منشورًا. تحقق Playwright (1600 و390): الزر معطَّل قبل فتح عرض، النافذة بالوضع الصحيح، إلحاق ملف pptx →
  `apply=append` و5 شرائح والمؤشر على 3 والرسالة، استبدال بملف pdf → شريحتان، ومعرض «عرض جديد» يبقي الاستيراد القديم بحقل العنوان؛
  بلا تمرير أفقي. 237/237. **القراءة الحية لملف حقيقي على عرض موجود لم تُجرَّب هنا.**

## استيراد ملف إلى تصميم موجود في تصميم حر (2026-11-22) ✅

- **الطلب** («نفذ الميزة دي في تصميم حر أيضًا»): بطاقة «رفع ملف كما هو» في المعرض تنشئ تصميمًا جديدًا دائمًا؛ الآن يمكن استبدال صفحة
  تصميم مفتوح بملف مرفوع مع بقاء معرّفه ورابطه.
- **الخادم**: قراءة الملف صارت مشتركة في `FreeformController.ReadPageFileAsync` (HTML حرفيًا بترميزه، أو .pptx عبر `PptxPageBuilder.Build`
  إلى صفحة شرائح بلا موديل؛ حدود الحجم والصيغة ورسائل الخطأ نفسها) يستخدمها `POST import-page` (تصميم جديد) و**`POST /api/freeform/{id}/import-page`**
  الجديد: `AddVersionAsync` بملاحظة «استيراد الملف «X» كما هو» (العنوان والرسائل والمصادر و`PresentationId` كما هي؛ الإصدارات السابقة تُستعاد
  من القائمة)، وملف PowerPoint يحوّل النوع إلى `slides` (`FreeformStore.SetKindAsync` الجديدة)؛ رسالتان في المحادثة، والرد يذكّر بأن الرابط
  المنشور يعرض النسخة السابقة حتى «تحديث النشر». يعيد `{design, html, version, versions, messages, slides, notes, kindChanged, published}`.
- **الواجهة**: زر «📥 استيراد إلى هذا التصميم» في شريط أدوات التصميم (معطَّل في حالة البداية كباقي الأزرار) يفتح نافذة `ff-page-modal` نفسها
  في وضع «into» (`openFfPageModal(into)`، `ffpInto`): عنوان «استيراد ملف إلى هذا التصميم»، ملاحظة `#ffp-into-note` بدل `#ffp-new-note`، حقل
  العنوان `#ffp-title-wrap` مخفي، زر «استيراد كما هو». بعد الاستيراد تُحدَّث `FF.design/FF.html/FF.versions/FF.messages` من الرد ويُعاد الرسم
  (الزر يبقى «تحديث النشر» وزر الرابط ظاهرًا). تحقق Playwright (1600 و390): الزر معطَّل قبل الفتح، النافذة بالوضع الصحيح بلا حقل عنوان، خطأ
  بلا ملف، HTML → الإصدار 3 والمعاينة بالمحتوى الجديد والعنوان والرابط باقيان والرسالة تذكّر بالنشر، PowerPoint → النوع شرائح وزر ملء الشاشة
  والإصدار 4، والمعرض يبقي الرفع كتصميم جديد بحقل العنوان؛ بلا أخطاء وبلا تمرير أفقي. 237/237. **القراءة الحية لملف حقيقي لم تُجرَّب هنا.**

## نماذج صفحات جاهزة في تصميم حر: «الأزرق المؤسسي» (2026-11-22) ✅

- **الطلب** (المستخدم بخمس صور لقالب PowerPoint: أبرز الخلاصات، خارطة الطريق الاستراتيجية، نموذج التشغيل، محرّكات نمو الأعمال، نظرة على
  الأداء): إضافته نموذجًا داخل نوع «شرائح» في تصميم حر ليُستخدم ويُعدَّل. لا ملف pptx متاح، فأُعيد بناء التصميم **صفحة HTML كاملة** بالعربية
  وRTL (مرآة التخطيط الأصلي: النص في جهة البداية والصور في جهة النهاية).
- **نوع جديد من النماذج — نموذج صفحة جاهزة**: `FreeformTemplate.Page` (مسار ملف تحت wwwroot؛ `page` في JSON) بخلاف النماذج البنيوية.
  `wwwroot/templates/freeform/corporate-blue.html`: خمس `section.slide` بإطار ثابت 1280×720 يُصغَّر ليلائم الحاوية (`flex:none` ضروري وإلا
  يتقلص الإطار في المعاينة الضيقة ويُقص)، خط Tajawal من Google Fonts، ألوان `--navy/--blue`، كتل زرقاء مائلة بـ`clip-path`، أماكن صور
  `.photo-ph` بنمط هندسي ونص «صورة — استبدلها بصورتك»، رسم أعمدة مجمّعة SVG في الشريحة الخامسة من كتلة `<script type="application/json"
  id="chart-data">` (الفئة الأولى في اليمين)، تنقّل أسهم/مسافة/لمس/أزرار وشريط تقدّم، `window.goToSlide`، حدث `jeem:slide`، تضمين
  `/js/jeem-slides.js` وحجز `--jeem-strip-h`، وطباعة شريحة لكل صفحة. البند في الكتالوج `sl-corporate-blue` (تصنيف mgmt) بـ`Brief` يلزم الموديل
  عند التعديل بالحفاظ على البنية والأنماط وتغيير النصوص/الأرقام فقط، وبتعديل أرقام الرسم في `chart-data` (من `query_data` عند طلب بيانات).
- **الخادم** (`FreeformController` + `IWebHostEnvironment`): `GET /api/freeform/templates/{key}/page` يعيد ملف الصفحة (لمعاينة المعرض؛
  يرفض `..`)، و`POST /api/freeform/templates/{key}/use {title?, sources?}` ينشئ تصميمًا **فورًا بلا موديل** من الملف بنوع النموذج ومفتاحه
  (`templateKey`) ورسالتين إرشاديتين ويعيده بشكل `import-page`؛ ثم التعديل نصيًا من «✏️ تعديل النص» أو بالمحادثة (الموديل يتلقى HTML
  الصفحة كاملًا مع الموجز).
- **الواجهة**: بطاقة النموذج في المعرض تعرض **معاينة حيّة** للشريحة الأولى (`.ff-live-card` بإطار صامت) بدل الرسم التخطيطي وشارة «جاهز»؛
  المعاينة تعرض الصفحة نفسها وزر «معاينة الشرائح كاملة» يفتحها بالتنقّل وشريط المصغّرات؛ كل أزرار «استخدم هذا النموذج» تمر من
  `ffUseTemplate(t)`: نموذج صفحة → `use` ثم `ffOpen` (شريحة المرفق تُستعاد من `templateKey`)، وغيره → الإرفاق في المحادثة كما كان.
  الصفحة تُجلب مرة وتُخزَّن (`ffPageCache`).
- اختبار `PageTemplates_PointToExistingFiles_WithSlidesAndNavigation` (الملف موجود، عدد الشرائح = الأقسام، التنقّل، الشريط، الموجز) 238/238.
  تحقق Playwright بالواجهة الحقيقية (1400 و390): البطاقة بالمعاينة الحيّة والشارة، المعاينة بالصفحة، المعاينة الكاملة بخمس شرائح وشريط
  المصغّرات، «استخدم» يستدعي `use` ويفتح التصميم بنوع شرائح وشريحة النموذج وزر ملء الشاشة والرسالة والشرائح الخمس في المعاينة؛ ولقطات
  الشرائح الخمس على 1280×760 مطابقة للتصميم المطلوب. **التعديل الحي بالموديل على الصفحة لم يُجرَّب هنا.** لإضافة نموذج جاهز آخر: ملف تحت
  `wwwroot/templates/freeform/` + بند بـ`Page` في `FreeformTemplates.All`.

## لغة النموذج في معرض تصميم حر: العربية/English لكل الأنواع (2026-11-22) ✅

- **الطلب** (بصورة): أزرار «العربية | English» فوق «استخدم هذا النموذج» في معاينة أي نموذج، تبدأ بلغة المنصة، فيستعرض المستخدم النموذج
  باللغة الأخرى ويستخدمه بها — في كل أنواع تصميم حر.
- **الترجمة** `Freeform/FreeformTemplateI18n.cs`: جدول ثابت (المفتاح → الاسم والوصف وعناوين الأقسام بالترتيب بالإنجليزية) للنماذج الثابتة
  الـ29 + «الأزرق المؤسسي». `FreeformTemplate` يعرض `nameEn/descriptionEn` (محسوبة من الجدول)، و`sections[].titleEn` (تُحقن في `All` عبر
  `WithEnglish`)، و`hasEn` (ترجمة كاملة) و`pageEn`. نماذج كتالوج اللوحات والمرفوعة بلا ترجمة (`hasEn=false`): المعاينة تبقى عربية داخل
  صفحة إنجليزية مع ملاحظة «المعاينة بالعربية، وتُبنى الصفحة بالإنجليزية»، لكن اللغة تصل للبناء.
- **الخادم**: `GET templates/{key}/page?lang=en` يعيد `PageEn` إن وُجد (`corporate-blue.en.html`: مرآة LTR للصفحة العربية بنصوص إنجليزية
  مولَّدة بسكربت `make_en_page.py` في scratchpad: تبديل right/left والاختصارات الرباعية ومرآة `clip-path` وزوايا التدرّج واتجاه الرسم ولوحة
  المفاتيح/اللمس)؛ `POST templates/{key}/use {lang}` ينشئ التصميم من ملف اللغة؛ `POST generate {lang}` للتصميم الجديد →
  `FreeformBuilder.GenerateAsync(..., lang)` → `LanguageOf(requested, currentHtml)` (الصريحة، وإلا من وسم `<html lang="en"/dir="ltr">`
  للتصميم القائم، وإلا العربية) → كتلة «# لغة الصفحة: الإنجليزية» في النظام (كل النصوص و`<html lang="en" dir="ltr">`، و`reply` يبقى
  بالعربية) و`BriefFor(t, lang)` يضيف الأسماء الإنجليزية للأقسام.
- **الواجهة** (app.js): `ffGalLang` (null = لغة المنصة `state.lang`) + `ffGalLangNow/ffTplName/ffTplDesc/ffTplSection`؛ `ffLangToggleHtml`
  (رقاقتان `.ff-lang [data-lang]`) في `pv-actions` يعيد رسم المعاينة بالاسم والأقسام والصفحة الحيّة باللغة المختارة؛ `ffTemplatePageHtml(t, lang)`
  بذاكرة لكل لغة؛ `JEEM_FF_DEMO.build(t, lang)` (الصفحة التوضيحية صارت ثنائية اللغة: جدول `I18N` للنصوص والأسماء والأشهر والأشخاص، والكشف عن
  نوع المحتوى يبقى بالعنوان العربي بينما يُعرض `titleEn`)؛ «استخدم» يرسل `lang` لنموذج الصفحة ويحفظ `FF.lang` للبنيوي فتظهر «· English»
  في شريحة المرفق وأول رسالة ويصل `lang` مع `generate`؛ فتح تصميم محفوظ يستنتج `FF.lang` من وسم صفحته. «جديد» يعيد الضبط.
- **علّة أُصلحت في `jeem-slides.js`**: المصغّرات كانت فارغة في صفحة LTR لأن المرساة `inset-inline-start` داخل شريط `direction:rtl` مع أصل
  تحويل `top left` تدفع المحتوى خارج الإطار؛ الآن مرساة فيزيائية `left:0` + `transform-origin:top left` دائمًا، ونص زر الإخفاء إنجليزي في
  صفحات LTR.
- اختبارات: `StaticTemplates_AllHaveEnglishNamesAndSections` (ترجمة كاملة لكل نموذج ثابت، الموجز، `LanguageOf`، كتلة اللغة في النظام) +
  فحص ملف `PageEn` (lang/dir وعدد الشرائح) 239/239. Playwright (منصة عربية 1400، منصة إنجليزية 1400، جوال 390): الزر يبدأ بلغة المنصة،
  English يعرض الاسم والأقسام والصفحة التوضيحية بالإنجليزية (`lang="en" dir="ltr"`، «Key Results»، «Next →») والمعاينة الكاملة كذلك،
  الاستخدام يُظهر «· English» في الشريحة و`generate` يحمل `lang:'en'`؛ نموذج الصفحة يجلب `?lang=en` ويرسل `lang` مع `use` ويفتح الصفحة
  الإنجليزية؛ نموذج بلا ترجمة يعرض الملاحظة؛ بلا أخطاء وبلا تمرير أفقي. **البناء الحي بالإنجليزية بموديل لم يُجرَّب هنا.**

## نماذج الشرائح الجاهزة كمجلدات + المحمِّل التلقائي (2026-11-23) ✅

- **التسليم** (العقد في `docs/freeform-template-spec.md`): ثريد مستقلة تحوّل صور شرائح PowerPoint إلى نماذج وتدفعها على فرع
  `templates/batch-1` في `wwwroot/templates/freeform/<key>/` (`template.json` + `ar.html` + `en.html` + `assets/` بالصور الفوتوغرافية
  الأصلية، بمسار مطلق `/templates/freeform/<key>/assets/…`). الدفعة الأولى 56 نموذجًا (مع «الأزرق المؤسسي» = 57) دُمجت fast-forward.
- **المحمِّل** `Freeform/FreeformFolderTemplates.cs`: `Load(webRoot)` يقرأ كل مجلد فيه `template.json` (المجلد بلا ملف يُتجاهل)، يتحقق
  (المفتاح = اسم المجلد وبصيغة `[a-z0-9-]`، النوع والتصنيف من المفردات الثابتة، `name`، أقسام بعناوين، `ar.html` موجود، وللشرائح عدد
  `<section class="slide">` = الأقسام) ويبني `FreeformTemplate` بـ`Page`/`PageEn` و`NameEn`/`DescriptionEn`/`TitleEn` من الملف و`Brief`
  الافتراضي `DefaultBrief` إن غاب؛ المخالف في `Rejected` بسببه. Program.cs بعد `builder.Build()` يستدعيه ويسجّل
  `FreeformTemplates.RegisterFolderTemplates` ويكتب عدد المقبول والمرفوض وسبب كل مرفوض في السجل.
- **الكتالوج**: `FreeformTemplates.Builtin` (المكتوبة في الكود) و`All` صارت خاصية = الثابتة ثم المجلدات (المفتاح المكرر مع ثابت يُتجاهل).
  `NameEn`/`DescriptionEn` صارتا حقلين يُملآن في `WithEnglish` من `FreeformTemplateI18n` للثابتة ومن `template.json` للمجلدات. لا تغيير في
  الـ API: `templates`/`templates/{key}/page?lang=`/`templates/{key}/use` تعمل على نماذج المجلدات كما على «الأزرق المؤسسي».
- **الواجهة**: معاينات بطاقات النماذج الجاهزة في المعرض صارت كسولة (`IntersectionObserver` بهامش 300px في `renderFfGalGrid`: عند الفتح
  6 إطارات على 1400px و1 على 390px بدل 57 دفعة واحدة بمئات الصور)، والمصغّرات الصامتة تخفي شريط `#jeem-strip` (`FF_THUMB_QUIET`).
- **علّتان في `jeem-slides.js` أُصلحتا** (ظهرتا مع الدفعة وكانتا في «الأزرق المؤسسي» أيضًا): (1) الشريط كان `direction:rtl` دائمًا فيبدأ
  من اليمين في الصفحات الإنجليزية — صار يتبع `dir` الصفحة؛ (2) نسخة المصغّرة كانت تُفرض `display:block` فيضيع توسيط الإطار الثابت
  (flex) ويظهر المحتوى منزاحًا مصغّرًا في الركن — صار `slideDisplay()` يأخذ عرض الشريحة الظاهرة.
- **تحقق**: فحص ثابت (scratchpad `check_batch.py`) و224 تحميل صفحة في Playwright (56 × عربي/إنجليزي × 1280/390): صفر أخطاء JS،
  صفر صور مكسورة، التنقّل والشريط، بلا تمرير أفقي. الواجهة الحقيقية بخادم وهمي: 57 بطاقة و9 تصنيفات بأعدادها، البحث، المعاينة
  بالعربية ثم English بالصفحة الإنجليزية، المعاينة الكاملة 14 شريحة و14 صورة بلا كسر، «استخدم» يرسل `lang:'en'` ويفتح المحرر بالصور.
  اختبارات `FreeformFolderTemplateTests` (المجلد الحقيقي كله مقبول وثنائي اللغة، التسجيل والأولوية، ورفض 5 حالات مخالفة) 241/241.
- **ملاحظة مفتوحة للثريد الأخرى**: `sl-business-case-study/en.html` فيه 30 وصف صورة (`alt`) بالعربية.
- **لم يُجرَّب هنا**: الإقلاع الحقيقي على SQL Server (يلزم ظهور «Freeform folder templates: 56 loaded, 0 rejected» في السجل)، والتعديل
  بالموديل على هذه الصفحات (حجمها حتى 95KB، داخل حد 180K حرف للتعديل).

## إخفاء شريط المحادثة في تصميم حر (2026-11-23) ✅

- **الطلب**: خيار لإخفاء المحادثة لمساحة عمل أوسع، ثم بصورة: «حط إخفاء المحادثة بدل زر التصاميم» في رأس المحادثة.
- **الزران**: «إخفاء المحادثة» (`#ff-chat-hide`، أيقونة `sidebar` + نص) في رأس `#ff-rail` بجوار «جديد» **مكان قائمة «التصاميم»**
  (`#ff-list-dropdown`/`#ff-list-btn`/`#ff-list-panel` ومعالجاها وقواعد CSS الخاصة بها **أُزيلت بطلب المستخدم**؛ التصاميم السابقة متاحة من
  «تصاميمك السابقة» في معرض البداية عبر «جديد»)؛ و«إظهار المحادثة» (`#ff-chat-toggle`، `aria-label`) آخر عنصر في رأس `#ff-pane` خارج
  مجموعتَي الأزرار، **يظهر فقط والمحادثة مخفية** (`.hidden`)، وأيقونة فقط على ≤520px.
- `ffSetChatHidden(hidden, persist)` يضع صنف `ff-chat-hidden` على `#screen-freeform` (`#ff-rail { display:none }`)، يبدّل زر الإظهار، ينقل
  التركيز إليه عند الإخفاء وإلى صندوق الكتابة عند الإظهار، ويحفظ الاختيار في localStorage `ffChatHidden` فيُستعاد عند الإقلاع. `ffAsk`
  يُظهر المحادثة تلقائيًا (بلا حفظ) حتى يُرى الرد لأي طلب للموديل من شريط الأدوات.
- تحقق Playwright (1400 و390): لا زر «التصاميم»، الإخفاء من رأس المحادثة يوسّع مساحة العمل 720→1120px (والجوال 410→683px ارتفاعًا) ويُظهر
  زر الإظهار ويركّزه، الاختيار يصمد بعد إعادة التحميل وفتح تصميم، طلب بالموديل يُظهر المحادثة برسالته، الإظهار يعيد الزر إلى الرأس ويركّز
  صندوق الكتابة، بلا أخطاء JS وبلا تمرير أفقي.

## مشاركة تصميم حر داخل المنصة (2026-11-23) ✅ — المرحلة 1 من «الصلاحيات»

- **الطلب**: مشاركة التصميم مع مستخدمين محددين أو المشروع أو المنظمة، و«مشترك معي» تعرض فقط ما يحق للمستخدم رؤيته. (المرحلة 2
  «صلاحيات الشاشات» أُنجزت لاحقًا — القسم التالي.)
- **النموذج**: عمودان على `FreeformDesigns`: `ShareScope` (`private|project|org`) و`ShareScopeRole` (`view|edit`)، وجدول
  `dbo.FreeformShares(DesignId, UserId, Role, CreatedAt)`. **التصاميم القائمة** تأخذ بقيم الأعمدة الافتراضية `project`/`edit` (سلوكها السابق:
  كل المشروع يراها ويعدّلها)، **والجديدة** `private`/`view` (`CreateAsync`). الحذف يحذف المشاركات.
- **القاعدة** `Freeform/FreeformAccess.Resolve` (نقية، مختبَرة): منظمة أخرى ← لا وصول أبدًا (تصميم قديم بلا منظمة يُعامل بمشروعه)؛ صاحبه أو
  المسؤول ← `owner`؛ وإلا الأعلى بين دور المشاركة بالاسم، ودور الوصول العام إن كان `org` أو `project` ومشروع المستخدم الحالي هو مشروع التصميم؛
  وإلا null. «المشروع» = المشروع الحالي للمستخدم (`ResolveCurrentProjectIdAsync`) كباقي المنصة.
- **الخادم** (`FreeformController`): `LoadAsync(id, user, selection, need)` بدل `GetAsync(id, projectId)` في كل نقطة: غير المرئي 404 (لا يُكشف
  وجوده)، والمرئي بدور أقل 403 برسالة. **مشاهدة**: الفتح والإصدارات، و«تحويل إلى» (ينشئ نسخة ملك المستخدم في مشروعه). **تعديل**: generate على
  تصميم، refresh، PUT، restore، import-page into، publish. **مالك**: الحذف والمشاركة. عمليات البيانات تجري في مشروع التصميم
  (`InDesignProject`) بصلاحيات المستخدم نفسه على الملفات (قيد صاحب الملف كما هو). الهوية من مشروع التصميم. البصمة لا تُحسب للمشاهد.
  `GET /api/freeform` صار `ListVisibleAsync` (كل ما يحق له في المنظمة؛ للمسؤول كل تصاميم المنظمة) ويعيد لكل تصميم `ownerName`، `access`،
  `shareScope`، `shareScopeRole`، `shareCount`. `GET {id}` يعيد `access`/`ownerName`/`shareCount`. جديد: `GET/PUT {id}/sharing` (الإعدادات؛
  التغيير للمالك فقط، مستخدمون نشطون من المنظمة نفسها ≤100، صاحبه يُتجاهل) و`GET share-candidates?q=` (مستخدمو المنظمة النشطون عدا نفسه، ≤50).
- **الواجهة** (app.js، قسم «مشاركة تصميم حر داخل المنصة»): زر «👥 مشاركة» (`#ff-share`، للمالك فقط) يفتح `ff-share-modal`: بحث عن أشخاص من المنظمة
  (النتائج تُفتح بالكتابة أو النقر لا بالتركيز البرمجي، وتُغلق بالنقر خارجها)، قائمتهم بدور عرض/تعديل وإزالة، والوصول العام (خاص/المشروع/المنظمة)
  بدوره (معطَّل مع «خاص»)؛ Escape/الخلفية تغلق. `FF.access` من الخادم؛ `ffApplyAccess` للمشاهد: تعطيل تعديل النص/الكود/الاستيراد/التحديث/النشر/
  الاستعادة والعنوان وصندوق الكتابة بنص يوضّح السبب (صنف `ff-readonly`)، و`ffAsk` يرفض برسالة. سطر الوصف يذكر «مشترك معك من X (عرض فقط)» أو
  «مشترك مع المشروع ومع شخص واحد». بطاقات المعرض: «👥 من سارة · عرض فقط» لتصاميم الآخرين و«👥 مشترك مع …» لتصاميمي. التبويبات كما هي (تصاميمي =
  أنشأتها، مشترك معي = غيرها مما يحق لي).
- **ملاحظة**: «مشاركة» داخل المنصة تختلف عن «نشر» (رابط عام لأي شخص) — مذكور في النافذة.
- تحقق: `FreeformAccessTests` 245/245؛ Playwright (1400 و390) بخادم وهمي: التبويبات بالشارات، المالك يضيف شخصًا بالبحث بدور تعديل ويضبط المشروع/تعديل
  فيُرسل PUT الصحيح ويتحدّث الوصف والرسالة، إعادة الفتح تعرض الشخص، Escape يغلق، المشاهد معطَّل بلا زر مشاركة ومحاولة الطلب ترفض، المحرّر بالمنظمة
  يعدّل بلا زر مشاركة، النافذة داخل الشاشة، بلا أخطاء وبلا تمرير أفقي. **لم يُجرَّب هنا على SQL Server** (ترحيل الأعمدة والاستعلام بـ`IN @ids`).

## صلاحيات الشاشات لكل مستخدم (2026-11-23) ✅ — المرحلة 2 من «الصلاحيات»

- **الطلب**: إعطاء شخص صلاحية يدخل بها فيجد «تصميم حر» وبياناته فقط؛ وبقرار المستخدم (الاختيار الأول) تبقى معه شاشة «الملفات» ليرفع بياناته،
  وهي تعرض أصلًا ملفاته فقط (`RepositoryController.Files`: صاحبه أو ممنوح بالاسم، وملف قديم بلا صاحب مسجَّل يبقى مفتوحًا).
- **النموذج**: عمود `AppUsers.AllowedScreensJson` (null = كل الشاشات؛ يُضاف بحلقة الترحيل نفسها) و`UserInfo/UserRequest.allowedScreens`.
  المسؤول يُحفظ له null دائمًا ويُعاد null. قائمة فارغة لمستخدم عادي تُرفض 400 («اختر شاشة واحدة على الأقل»).
- **`Users/ScreenAccess.cs`** (ساكن، مختبَر): `Screens` (13 شاشة قابلة للتقييد: chat, published, analyst, decks, freeform, sources, repo, users,
  history, active, sharelinks, presentations, projects)، `FreeformOnly = [freeform, repo]`، و**خريطة بادئة ← الشاشات التي تحتاجها**: البادئة
  تُرفض فقط إن لم تحتجها أي شاشة مسموحة، لأن تصميم حر يستخدم واجهات اللوحات (`/api/chat|history|share|widgets|interactive|inquiry|export`
  لـ«لوحات المتابعة»)، و`/api/templates`، و`/api/sources|repository|semantics`، و`/api/presentations` (جسر البيانات والنشر)، و`/api/decks/brand`.
  فعليًا «تصميم حر فقط» يُرفض له: `/api/analyst`، `/api/decks` (عدا brand)، `/api/published-dashboards`، `/api/tokens`. البادئة غير المذكورة
  مسموحة (الدخول، المشاريع، …)، وواجهات المسؤول تحميها صلاحية الدور أصلًا. **أي واجهة برمجية جديدة لشاشة تُضاف إلى الخريطة.**
- **الفرض في الخادم**: وسيط في Program.cs بعد `UseAuthentication`: لمسار مقيَّد فقط (`IsGated`) يُقرأ المستخدم ويُعاد 403 برسالة إن لم يُسمح.
- **الواجهة**: `screenAllowed`/`firstAllowedScreen`/`applyScreenAccess` (قسم «صلاحيات الشاشات» قبل `showScreen`): بنود `[data-screen]` غير المسموحة
  تُخفى (`.screen-denied`، في القائمة الجانبية وأيقونات الشريط العلوي)، ومجموعة القائمة التي فرغت تُخفى برأسها («المنظمة»)؛ `showScreen` يحوّل
  أي شاشة غير مسموحة (ومنها `data-goto-screen`) إلى أول مسموحة؛ `startApp` يبدأ المستخدم المقيَّد من أول شاشة مسموحة بلا الجولة التعريفية.
  نموذج المستخدم (الإدارة ← المستخدمون): كتلة «الوصول لكل الشاشات» + قائمة الشاشات + زر «✎ تصميم حر فقط (مع الملفات)» الذي يضبط الشاشتين
  **ويوقف «الوصول لكل الأنظمة»** (فلا يرى إلا ملفاته وما شورك معه؛ يمكن إعادة نظام يدويًا)؛ الكتلة مخفية لدور المسؤول؛ جدول المستخدمين يعرض
  ملخص الشاشات تحت الأنظمة. المعرض صار يقبل ردًا غير مصفوفة من `/api/freeform` بلا كسر.
- تحقق: `ScreenAccessTests` 247/247؛ Playwright (1400 و390) بخادم وهمي: المستخدم المقيَّد يبدأ في تصميم حر ويرى «تصميم حر» و«الملفات» فقط بلا
  رأس «المنظمة» ولا أيقونات الشريط العلوي، `showScreen('chat')` يبقيه في تصميم حر و«الملفات» تعمل؛ مستخدم بلا تقييد والمسؤول يريان كل شيء؛
  النموذج: الزر الجاهز يحدد الشاشتين ويوقف الأنظمة ويُرسل `allowedScreens:[freeform,repo], allowAllSystems:false`، «كل الشاشات» يُرسل null،
  دور المسؤول يخفي الكتلة؛ بلا أخطاء وبلا تمرير أفقي. اختبارا المشاركة وإخفاء المحادثة بلا تراجع. **الوسيط على خادم حقيقي بـSQL Server لم يُجرَّب
  هنا** (الخريطة مختبَرة بالوحدة).

## ضبط تصميم معرض تصميم حر وشاشة الملفات (2026-11-23) ✅

- **عرض كقائمة كان يُخفي التصاميم**: `.ff-cards.list` كانت `overflow:hidden`، والعنصر ذو overflow غير المرئي يفقد حده الأدنى في شبكة
  `#ff-start` فتنضغط الصفوف إلى 25px (1px على الجوال). صارت `overflow: clip` (القص نفسه للزوايا بلا هذا الأثر).
- **رقائق أنواع التصميم بدل قائمة التصفية** (طلب المستخدم: «تظهر زي الكل وتصاميمي والمنشورة، جذابة وسهلة»): أُزيل `#ff-filter-dropdown`
  ومعالجاه، وصار `#ff-kind-chips` صفًا كاملًا في رأس «تصاميمك السابقة»: «كل الأنواع» ثم كل نوع له تصاميم في التبويب الحالي (والمختار يبقى ولو
  صار صفرًا) بأيقونة النوع بلونه وعدد تصاميمه، `aria-pressed`، والمختار بلون النوع الداكن؛ يُمرَّر أفقيًا داخله على الجوال.
  `ffGalBase()` (التبويب + البحث) أساس العدّادات، و`ffGalFiltered()` = الأساس + النوع، و`ffRenderKindChips()` تُستدعى من `ffRenderGallery`.
- **بطاقة «مالك المنصّة» أُزيلت** من القائمة الجانبية بطلب المستخدم (`#platform-badge` وسطر إظهارها وقواعدها)؛ قائمة مالك المنصّة
  (`#platform-menu`) باقية كما هي.
- **زر «🔗 ربط ملف برابط»** في «الملفات» كان بلا حشوة ومسطّرًا لأن قاعدة `.link-btn` العامة في app.css (روابط نصية) تأتي بعده؛
  `.upload-btn.link-btn` صارت تحدد الحشوة وتلغي الخط مع حالة تحويم.
- تحقق Playwright (1400 و390): ارتفاع بطاقات القائمة 56px بعد أن كان 25/1، البطاقة غير موجودة، الرقائق بأعدادها (7/2/2/1/1/1)، «شرائح» تعرض 2
  و«تصاميمي» تعيد الحساب (4) مع بقاء النوع، بلا أخطاء JS وبلا تمرير أفقي.


## قائمة «إعداداتي» بجوار اسم المستخدم (2026-11-23) ✅

- بطلب المستخدم: «قراءة تلقائية» و«دليل الاستخدام» و«الوضع الليلي» و«اللغة» خرجت من ذيل القائمة الجانبية إلى قائمة `#user-settings-panel`
  تُفتح من زر الترس `#user-settings-btn` في بطاقة المستخدم (بجوار الخروج). المعرّفات كما هي (`auto-read-toggle`/`tour-btn`/`theme-toggle`/
  `lang-toggle`) فمعالجاتها لم تتغير؛ صفّا المظهر واللغة قابلان للنقر كاملين (`data-us-for`). `usSetOpen(open)`: تُفتح فوق البطاقة بعرض الذيل،
  وفي القائمة المطوية تُثبَّت `fixed` بجوار الشريط الجانبي لأنه يقص ما يخرج عنه؛ Escape والنقر خارجها يغلقانها، ودليل الاستخدام يغلقها قبل البدء.
  القواعد في ذيل jeem.css بمحدِّد `#user-settings-panel` لتغلب قواعد المعرّفات القديمة في app.css.
- تحقق Playwright (1400، مطوية، 390 بالدرج): العناصر مخفية قبل الفتح، الأربعة تظهر، نقر صف المظهر يبدّله والقائمة باقية، Escape والنقر خارجها
  يغلقان، المطوية بجوار الشريط داخل الشاشة، بلا أخطاء وبلا تمرير أفقي.

## الوضع الليلي الحقيقي (2026-11-24) ✅

- كان `data-theme="dark"` هو الافتراضي لكنه لا يغيّر شيئًا في الهوية الجديدة (رموز `.jeem-scope` فاتحة دائمًا). الآن:
  الافتراضي **فاتح** (سكربت الرأس في index.html: `dark` فقط إن حُفظ صراحة في `chatToDashboardTheme`)، و`:root[data-theme="dark"]
  .jeem-scope` في ذيل tokens.css يعيد تعريف كل رموز الهوية (الأسطح والنصوص والحدود، ألوان الهوية والحالات مفتّحة للتباين، ألوان الرسوم،
  الظلال) + `color-scheme: dark`. الرسوم (`jeem-viz.js` → `tokenTheme`) تتبع الرموز تلقائيًا، و`applyTheme` يعيد رسم اللوحة.
- `background: #fff` في jeem.css/app.css (42 موضعًا) صار `var(--surface)` (مطابق في الفاتح). والألوان الفاتحة المكتوبة مباشرة لها كتلة تصحيح
  في ذيل jeem.css مقيّدة بـ`:root[data-theme="dark"]`: الشريط العلوي، رؤوس المحادثة ومساحة العمل (`#F3F5F7`)، زر «ربط ملف برابط»، ألوان أنواع
  تصميم حر (`--kh` لكل نوع مخلوطًا بالسطح بـ`color-mix` بدل الباستيل)، ورقاقة «كل الأنواع». الشعار: `img/logo-lockup-dark.svg` (كلمة «جيم»
  فاتحة) عبر `content: url()`. **أي لون فاتح مكتوب مباشرة يُضاف تصحيحه في الكتلة نفسها.**
- زر «الوضع الليلي» في «إعداداتي» صار مفتاحًا بنص «مفعّل/معطّل» و`aria-pressed` (`syncThemeToggle`).
- خارج النطاق عمدًا: المحركات التفاعلية (`dash-kit` وأخواتها، لها مفتاح فاتح/داكن خاص بها)، معاينات المعرض والعروض التوضيحية، ومحتوى
  صفحات تصميم حر/العروض داخل الإطارات، وصفحة العرض المنشور `presentation.html` — تبقى بمظهرها.
- تحقق Playwright (1400 داكن وفاتح، 390 داكن): المحادثة، تصميم حر بمعرضه ونافذة نماذجه، لوحة متابعة، الملفات ونافذة الربط، المصادر، منشئ العروض،
  المستخدمون، الإعدادات، العروض، السجل، نافذة إضافة عناصر؛ الفاتح مطابق لما قبل؛ التبديل من القائمة يغيّر الألوان فورًا ويحفظ `dark`؛ بلا أخطاء JS.

## مرفقات وإملاء صوتي في محادثة تصميم حر (2026-11-24) ✅

- **الواجهة** (app.js، قسم «مرفقات محادثة تصميم حر والإملاء الصوتي»): زر 📎 (`#ff-attach-btn` + `#ff-file-input` متعدد) والسحب والإفلات على
  صندوق الكتابة واللصق، حتى 10 مرفقات في `FF.atts` تظهر شرائح `#ff-files` (أيقونة/مصغّرة، الاسم، الحالة: «جارٍ الرفع…» أو الملاحظة أو ⚠️ السبب،
  وزر ✕). الإرسال ينتظر اكتمال الرفع، ثم `ffTakeAttachments()` يرسل الجاهز في `generate.attachments` ويضيف سطر «📎 …» لرسالة المستخدم.
  زر 🎤 (`#ff-mic`، يظهر حيث يدعم المتصفح `SpeechRecognition`) يكتب في `#ff-q` بلغة الواجهة ويستمر عبر التوقفات حتى يُضغط ثانية
  (`aria-pressed`)، والنص قابل للتعديل قبل الإرسال. المشاهد (عرض فقط) يُعطَّل له الزران.
- **ثلاثة أنواع** (`ffAttKind`): **صورة** (PNG/JPG/GIF/WebP) → `POST /api/freeform/attachments` يخزّنها في `dbo.FreeformAssets`
  (`Freeform/FreeformAttachments.cs`، `FreeformAssetStore`، ≤5MB، النوع من **التوقيع** لا الامتداد؛ SVG مرفوض) ويعيد `/api/freeform/assets/{24 hex}`
  الذي يُقدَّم **بلا دخول** مع تخزين مؤقت (الصفحة داخل إطار معزول وفي الرابط المنشور)؛ **مستند** (PDF/PowerPoint/HTML عبر `DeckImporter` +
  `FromDocument`، Word عبر OpenXML، txt/md) → النص المستخرج حتميًا (≤30K حرف) يعود للمتصفح ويُرسل مع الطلب؛ **بيانات** (CSV/Excel) → مسار
  «الملفات» المعتاد (`/api/repository/upload` ثم `files` بتصنيف «مرفقات تصميم حر») فيصير جدولًا في المشروع، ويُضاف للمصادر المقيَّدة إن وُجدت.
- **الموديل**: `FreeformBuilder.GenerateAsync(..., attachments)` → `FreeformAttachmentText.Build` كتلة «# مرفقات المستخدم» قبل الطلب: الصورة برابطها
  حرفيًا (رابط غير أصولنا يُتجاهل)، البيانات باسم الجدول ليستعلم عنه، والمستند نصًا مرجعيًا (≤60K حرف للمرفقات معًا).
- **لوحات المتابعة**: ملفات البيانات فقط (تصير مصادر اللوحة)؛ الصور والمستندات تُرفض برسالة على الشريحة.
- **قيد**: الصورة في صفحة «تنزيل HTML» تبقى رابطًا للمنصة (لا تُضمَّن داخل الملف).
- اختبارات `FreeformAttachmentTests` (الكتلة، الاقتطاع، استخراج txt/html/docx، نوع الصورة بالتوقيع) 251/251. تحقق Playwright بخادم وهمي (1400 و390):
  الزران ظاهران، الإملاء يكتب في الصندوق ويتوقف بالضغط ثانية، رفع CSV+نص+صورة يمر بـ«جارٍ الرفع» ثم ثلاث شرائح جاهزة، الطلب يحمل
  `data:proj_1.sales` و`doc:…` و`image:/api/freeform/assets/…` ورسالة المستخدم تعرض المرفقات، الشرائح تُمسح بعد الإرسال، ملف مرفوض يظهر بالسبب ويُزال
  بـ✕، بلا تمرير أفقي. **على SQL Server والموديل الحقيقي لم يُجرَّب** (جدول `FreeformAssets` يُنشأ عند أول رفع).

## تغيير الصور داخل التصميم من وضع التعديل (2026-11-24) ✅

- **الطلب**: تغيير الصور داخل الشرائح في تصميم حر. وُسِّع وضع «✏️ تعديل النص» (الزر صار «✏️ تعديل» والشريط «اضغط أي نص لتعديله أو أي صورة
  لتغييرها») بلا موديل.
- **داخل الإطار** (`ffImgKit` في app.js، يُحقن كـ`FF_IMG_KIT` مع `FF_EDIT_KIT` في المعاينة فقط؛ مكتوب دالة عادية تُسلسل بـ`toString()`): في وضع التعديل
  يُعلَّم المرشّح عند التحويم بإطار وشارة «🖼️ اضغط لتغيير الصورة» (`#ff-img-badge`). المرشّحات: `.photo-ph`/`[data-jeem-photo]` بلا صورة (مكان فارغ،
  أولوية)، `<img>`، `<image>` داخل SVG (صفحات PowerPoint المرفوعة)، وعنصر بخلفية `url()` حتى 4 مستويات أعلى — **إلا إن كان المنقور نصًا مباشرًا**
  فيبقى لتعديل النص. معالج النقر على `window` بطور الالتقاط فيسبق معالج النصوص على `document` ويوقفه. يرسل `{type:'img-pick', id, kind, src, fit, pos,
  alt, w, h}` (`data-ff-img-id`)، ويطبّق `{type:'img-set'}`: `<img>` → `src` (يزيل `srcset`/`<source>`) + `object-fit/position` + `alt`؛ خلفية →
  `background-image/size/position`؛ مكان فارغ → يُستبدل محتواه بـ`<img>` يملؤه؛ SVG → `href` + `preserveAspectRatio`. ويعدّ الصور المعدَّلة (`img-dirty`).
- **في المنصة**: `ff-img-modal` (`ffImgOpen`): معاينة الصورة الحالية ومقاس إطارها، رفع/سحب صورة → `POST /api/freeform/attachments` (أصول المرفقات نفسها،
  PNG/JPG/GIF/WebP ≤5MB)، «صور مستخدمة في هذا التصميم» (`ffImgDesignImages`: أصول `/api/freeform/assets/…` وصور `/templates/freeform/…` في الصفحة + ما رُفع
  في الجلسة)، طريقة الملء (ملء/احتواء)، مكان التركيز (وسط/أعلى/أسفل/يمين/يسار)، والوصف البديل. مكان فارغ بلا صورة مختارة → رسالة. Escape/الخلفية تغلق.
  شريط التعديل يعدّ «معدَّل: نص واحد و3 صور» (`ffEditCountText`)، والحفظ هو نفسه (إصدار جديد، ملاحظة «تعديل نصوص وصور مباشر…»). التنظيف عند
  الحفظ يزيل `#ff-img-kit/#ff-img-style/#ff-img-badge` وسمات `data-ff-img-*`.
- **قيد**: صورة تُرسم في `<canvas>` أو تُضبط بسكربت الصفحة عند التشغيل لا تُغيَّر بهذه الطريقة؛ والمحفوظ DOM الصفحة لحظة الحفظ (قيد التعديل النصي نفسه).
- تحقق Playwright بخادم وهمي (1400 و390): الزر «✏️ تعديل»، الشارة عند التحويم، النقر على `<img>` يفتح النافذة بالصورة والوصف والمقاس وصورتي التصميم،
  الرفع ثم «احتواء/أعلى» يطبَّق داخل الإطار، النقر على نص فوق الخلفية يبقى تعديل نص، والنقر على فراغ الخلفية يغيّرها من صورة مختارة، المكان الفارغ يرفض
  بلا صورة ثم يتحول إلى `<img>`، العدّاد «نص واحد و3 صور»، Escape يغلق، والحفظ يرسل HTML بالروابط الجديدة والنص بلا أي أثر للأدوات؛ بلا أخطاء JS
  وبلا تمرير أفقي.

- **إصلاحان بعد أول تجربة للمستخدم** (2026-11-24، بصورتين): (1) **أزرار التنقّل بين الشرائح كانت تُعامل كنص** فتصير قابلة للتحرير ولا تنقل؛ الآن
  العناصر الحيّة `button,[role=button],input,select,textarea,summary,#jeem-strip` مستثناة في عدّتي النصوص (`LIVE` في `textual`) والصور (`find`)، فتعمل
  كما هي في وضع التعديل (ثمن واعٍ: نص الأزرار لا يُعدَّل مباشرة). (2) **مكان الشعار «شعار شركتك هنا»** لم يكن صورة بل أيقونة SVG ونص: `logoSlot` يتعرّف
  عليه (صنف `logo`/`lg`، أو `.logo-t` بجوار `.logo`، أو حاوية بأيقونة SVG ونص «شعار/logo» ≤40 حرفًا) بنوع `logo` (`data-ff-img-kind`) وشارة «🖼️ اضغط
  لإضافة شعارك» ونافذة «إضافة الشعار»؛ التطبيق يستبدل الأيقونة بـ`<img>` بارتفاعها **قبل تصغير الشريحة** (`offsetHeight`، والمقاس في النافذة كذلك) مع
  الحفاظ على نسبة الشعار، ويزيل نص «شعار شركتك هنا» من الحاوية أو شقيقه `.logo-t`. تحقق Playwright على نموذجين حقيقيين (`sl-ai-business-plan` بـ`.lg`،
  و`sl-ai-corporate-training` بـ`.logo`+`.logo-t`) على 1400 و390: التالي/السابق ينقلان في وضع التعديل بلا تحرير، الشعار يُضاف مكان الأيقونة بارتفاع 56/52px
  ويختفي النص، والحفظ بلا آثار؛ واختبار الصور السابق بلا تراجع.
- **التنقّل بين الشرائح خرج من الصفحة إلى المنصة** (2026-11-24، المستخدم رفض أن يكون ثمن الإصلاح السابق عدم تعديل نص الأزرار): استثناء
  `button/[role=button]/summary` من التعديل **أُلغي** (المستثنى الآن `input,select,textarea,#jeem-strip` فقط)، فنص أي زر يُعدَّل بالنقر. بدلًا منه
  `ffNavKit` (يُحقن كـ`FF_NAV_KIT` في معاينة المحرر فقط) **يخفي أزرار تنقّل الصفحة نفسها** (أزرار/روابط خارج الشرائح بمعرّف/صنف/وصف/نص يطابق
  prev/next/السابق/التالي أو سهمًا وحده، وحاويتها المشتركة مع العدّاد إن وُجدت) بسمة `data-ff-navhide` ونمط `#ff-nav-style`، ويعلن
  `{type:'nav-state', i, n}` (الشرائح بلا نسخ شريط المصغّرات ولا المتداخلة) وينفّذ `{type:'nav', dir|index}` عبر `goToSlide` أو صنف التفعيل.
  في المنصة شريط `#ff-slide-nav` تحت المعاينة («→ السابقة · 1 / 16 · التالية ←» بأسهم لا تنعكس في RTL، يُقلب في LTR) يظهر لصفحة بشريحتين فأكثر،
  ويعمل في العرض وفي وضع التعديل. الأسهم ومفاتيح التنقّل أثناء الكتابة لا تقلب الشريحة (`stopPropagation` في عدّة النصوص)، والمسافة/Enter
  داخل زر تُدرج نصًا بدل الضغط عليه (`insertText`). **الصفحة المحفوظة والمنشورة لا تتغيّر**: أزرار تنقّلها باقية فيها، والتنظيف عند الحفظ يزيل
  `#ff-nav-kit/#ff-nav-style` و`data-ff-navhide`. تحقق Playwright على نموذجين حقيقيين (1400 و390): أزرار الصفحة مخفية في المحرر، شريط المنصة
  «1 / 16» والسابق معطَّل، التالي/السابق ينقلان في العرض وفي التعديل، زر داخل الشريحة يُعدَّل («اطلب عرضًا الآن» بمسافة) والأسهم لا تقلب الشريحة،
  والحفظ يحمل النص الجديد وأزرار الصفحة الأصلية بلا آثار؛ واختبارا الصور والشعار بلا تراجع.

## هوية العرض في تصميم حر: مكتبة هويات + إعادة تلوين بلا موديل (2026-11-24) ✅ — المرحلة 1

- **الطلب**: «المستخدم يختار identity مختلفة للعرض فيتغيّر العرض بناءً عليها».
- **مكتبة الهويات** (`Decks/BrandIdentityStore.cs`، جدول `dbo.BrandIdentities(Id, ProjectKey, OrganizationId, Name, BrandJson, CreatedBy,
  CreatedAt, UpdatedAt)`، ≤30 لكل مشروع): هويات مسمّاة بشكل `DeckBrand` (بلا الخطوط المرفوعة التي تبقى للمشروع). الهوية الافتراضية تبقى في
  `DeckBrands` كما هي. API في `DecksController`: `GET /api/decks/brand/identities` (`items` أولها الافتراضية بمعرّف `""` + `fonts`/`uploadedFonts`/
  `canEdit`)، `POST`، `PUT {id}`، `DELETE {id}` بصلاحية `CanEditBrandAsync` وتحقق `DeckBrand.Validate` (ولون أو خط أو شعار واحد على الأقل).
- **التصميم يتذكّر هويته**: عمود `FreeformDesigns.IdentityId` (null = الافتراضية)، `PUT /api/freeform/{id} {identityId}` ("" = الافتراضية؛ يرفض هوية
  ليست في مشروع التصميم) ويعيد `html` بالشعار الجديد. `BrandAsync(projectId, ct, identityId)` يحلّ هوية التصميم لكل نقاط العرض والبناء والتحديث
  والاستعادة والنشر (`ApplyBrandAssets` يملأ `<img data-jeem-logo>` بشعارها).
- **الواجهة** (app.js، قسم «هوية العرض في تصميم حر»): زر «🎨 الهوية» (`#ff-identity`، ضمن `FF_RO_IDS`) يفتح شريط `#ff-id-bar` فوق المعاينة: رقائق
  الهويات بألوانها وشعارها و«الحالية»، صف «ألوان العرض ←» لكل عائلة لون مكتشفة قائمة أساسي/ثانوي/إبراز/بلا تغيير، «✎ تعديل الهوية»، «+ هوية
  جديدة» (`ff-idm-modal`: اسم، 3 ألوان، خط من خطوط الشركة أو Google، شعار ≤400KB، معاينة؛ تعديل الافتراضية يحفظ في `PUT /api/decks/brand`)، وإلغاء/
  «تطبيق كإصدار جديد» (PUT بالـ HTML بعد إزالة `src` من `data-jeem-logo` + `identityId` + ملاحظة «تطبيق الهوية «X»»). المعاينة فورية في الإطار بلا حفظ.
- **تعديل هوية موجودة من رقاقتها** (سؤال المستخدم «إزاي أعدّل الهوية الموجودة؟»: زر «✎ تعديل الهوية» كان لا يظهر إلا بعد اختيار هوية وفي آخر
  الشريط): زر ✎ دائري (`.ffid-pen[data-ffid-edit]`) بجوار كل رقاقة لمن يملك التعديل يفتح `ff-idm-modal` مباشرة (الافتراضية تحفظ في `PUT /api/decks/brand`).
  بعد الحفظ تُختار الهوية وتُعاين ويقول السطر إن العرض لا يتغيّر إلا بـ«تطبيق كإصدار جديد» (التطبيق السابق مخبوز في الصفحة). تحقق Playwright (1400 و390).
- **`ffRebrand(html, brand, map)`** (DOMParser ثم تسلسل): الألوان من `<style>` (داخل `{…}` فقط حتى لا تُلتقط محدِّدات مثل `#fade`)، سمات `style`،
  `fill/stroke/stop-color/color/flood-color`، والنصوص المقتبسة في السكربتات (عدا `jeem-data`)؛ لا `url(#…)`. الملوّن = تشبّع ≥0.18 وفاتحية بين 0.08
  و0.95؛ العائلات بتقارب الدرجة (<25°)، ممثلها الأكثر تكرارًا؛ الربط الافتراضي أكبر ثلاث → أساسي/ثانوي/إبراز والباقي لأقربها. التحويل: الدرجة من
  الهدف + فرق اللون عن ممثله، التشبّع بالنسبة، والفاتحية مقطعيًا (فوق الممثل تُمدّ نحو الأبيض، تحته نحو الأسود) فتبقى الدرجات الفاتحة فاتحة؛ يحفظ
  صيغة اللون والشفافية. الثانوي الناقص = الأساسي أدكن، والإبراز الناقص = الثانوي. **`<meta name="jeem-identity" data-roles>`** يسجّل دور كل لون ناتج،
  فالتطبيق التالي يبني العائلات بالدور أولًا (بدونه اندمج الأخضر الأساسي والداكن الثانوي في عائلة واحدة وضاع لون). الخط: يُضاف أول كل
  `font-family` ومتغيرات `--*font*` (و`data-font` يحذف خط الهوية السابقة)، و`<style id="jeem-identity-font">` بـ@font-face لخطوط الشركة أو `@import`
  من Google. الشعار: أماكن الشعار (صنف logo/lg بأيقونة SVG، أو أيقونة + «شعار…» ≤40 حرفًا) تصير `<img data-jeem-logo>` بارتفاع الأيقونة ويُحذف نصها.
- تحقق Playwright على `sl-ai-business-plan` (1400 و390): 106 موضع لون أُعيد تلوينه (الكحلي→أخضر، الوردي→برتقالي)، الخط Cairo، الشعار في 4 مواضع،
  تغيير الربط يعيد المعاينة، التطبيق يرسل `identityId` والصفحة بلا بيانات الشعار، هوية جديدة من المحرّر تُضاف وتُختار، Escape يغلق، بلا أخطاء وبلا
  تمرير أفقي. **جدول الهويات وعمود `IdentityId` على SQL Server لم يُجرَّبا هنا.** المرحلة 2 المقترحة: اختيار الهوية عند إنشاء التصميم وفي منشئ العروض.

## أدوات التصميم ولوحة الخصائص في تصميم حر (2026-11-24) ✅ — المرحلة 1

- **الطلب** (بصور من محرر Claude): شريط أدوات عائم (نص، صورة، فيديو، جدول، أشكال، تعليق، Ask Claude) ولوحة خصائص (Table/Layout/Appearance).
  نُفّذ ضمن وضع «✏️ تعديل» القائم؛ **الفيديو والتعليقات مؤجَّلان للمرحلة 2**.
- **داخل الإطار** `ffDesignKit` (app.js، يُحقن `FF_DESIGN_KIT` **قبل** `FF_IMG_KIT` حتى يسبق معالجه على window معالج الصور فيلغي نقرة ما بعد
  السحب): التحديد على `pointerdown` (طور الالتقاط، لا يمنع تعديل النص ولا الصور)، صفة `data-ff-sel` بإطار بنفسجي، وإرسال `{type:'d-sel'}` بالخصائص
  المحسوبة (تُقرأ بعد إزالة إبراز التحرير المؤقت `data-ff-editing` وإلا قُرئت خلفيته الزرقاء) ومعلومات الجدول الأقرب. أوامر `{type:'d-op'}`: `style`
  (خصائص CSS مضمّنة)، `insert` (text/image/table/shape: rect/circle/line/triangle/arrow)، `table` (rows/cols/width/header)، `parent`، `duplicate`، `delete`،
  `undo`، `deselect`، `mark`. الإدراج: في الشريحة الظاهرة (أو شريحة المحدد) مطلقًا في منتصفها بـ`data-jeem-free` (يُسحب؛ معامل التصغير من
  `getBoundingClientRect/offsetWidth`)، وإلا بعد العنصر المحدد في التدفق. الصورة المدرجة مكان `.photo-ph[data-jeem-photo]` تلتقطه طبقة الصور.
  عرض عمود الجدول يُطبَّع فيبقى المجموع 100٪ (colgroup + `table-layout:fixed`). التراجع مكدّس دوال (نمط العنصر السابق، إزالة المُدرج، إعادة
  المحذوف، لقطة `outerHTML` للجدول) — لا لقطة للصفحة كلها لأنها تفقد مستمعي سكربتات الصفحة. `d-dirty` يعدّ التغييرات.
- **الحفظ**: `serialize(keep)` في `FF_EDIT_KIT` يحذف `#ff-design-kit/#ff-design-style` و`data-ff-sel` و`data-jeem-target` (إلا مع `keepTarget`)، و`get`
  يعيد `purpose`. عدّاد الشريط «n تغييرات تصميم»، والملاحظة «تعديل تصميم مباشر من مساحة العمل (أدوات وخصائص)».
- **الأم** (قسم «أدوات التصميم ولوحة الخصائص»): `#ff-dtools` (شريط عائم، يرتفع فوق `#ff-slide-nav` بـ`:has`) و`#ff-insp` (`ffdRenderInsp`). على ≤640px
  اللوحة ورقة سفلية ثابتة **مغلقة افتراضيًا** وتُفتح من زر الخصائص، و✕ يغلقها حتى تُفتح ثانية (وإلا غطّت المعاينة الصغيرة كلها)؛ على سطح المكتب ✕ يلغي التحديد.
- **«اطلب من جيم»**: `mark` ثم `get {keepTarget, purpose:'ask'}` → `ffAsk(msg, {html, display})` (معامل جديد): الطلب + «طبّق على العنصر المعلَّم بالسمة
  data-jeem-target فقط»، والصفحة الحالية بتعديلاتها غير المحفوظة تُرسل `html`. الخادم (`generate`) يحذف `data-jeem-target` من ناتج الموديل.
- تحقق Playwright على `sl-ai-business-plan` (1400 و390): الشريط يظهر في التعديل فقط، تحديد العنوان يعرض التخطيط/النص/المظهر ويبقى قابلًا للكتابة، الحشوة
  والتعبئة والحد المتقطع تُطبَّق، جدول مُدرج → 5 أعمدة و6 صفوف وعرض 40٪ وإلغاء صف العناوين، دائرة تُسحب 60×30، الصورة تفتح نافذة الصور والتراجع يزيلها،
  الحفظ بلا آثار الأدوات، «اطلب من جيم» يرسل عنصرًا معلَّمًا واحدًا، بلا أخطاء JS وبلا تمرير أفقي. **التعديل الحي بالموديل على عنصر معلَّم لم يُجرَّب هنا.**

## إصلاحات بعد تجربة المستخدم: حذف الصورة، انتظار تعديل لوحة المتابعة، والتمرير الذي يرجع لأسفل (2026-11-24)

- **حذف الصورة** في `ff-img-modal`: زر «🗑 حذف الصورة» (`#ffim-remove`، مخفي لمكان فارغ أو مكان شعار) يرسل `img-set {remove:true}`: `<img>`/`<image>`
  يُزال (مع `<picture>`)، والخلفية المصوّرة `background-image:none`. يُعدّ تعديل صورة ويُحفظ بالمسار نفسه.
- **انتظار تعديل لوحة المتابعة**: المتابعة بالمحادثة على لوحة مفتوحة (`ffTrackAsk` غير الأول) لم تكن تضع طبقة الانتظار، فكانت اللوحة قابلة للتعديل
  أثناء عمل الموديل. الآن `FF.trackBusy` + `ccSetLoading('ff-pane', 'جارٍ تعديل اللوحة', …)` حتى ينتهي الطلب، و`dashLoadingDone` لا يزيلها ما دام `FF.trackBusy`.
- **الصفحة «ترجع لتحت» فلا يصل المستخدم إلى «جديد»**: سببان معًا — (1) شريطا التعديل/الهوية فوق المعاينة كانا يزيدان ارتفاع `#ff-preview-wrap` (صف flex ملتف
  والإطار `height:100%`) فتصير صفحة المنصة كلها قابلة للتمرير؛ (2) `jeem-slides.js` كان ينادي `scrollIntoView` على المصغّرة النشطة عند كل تغيّر، وهي
  داخل إطار فتمرّر كل الصفحات الأم. الإصلاح: `#ff-preview-wrap` عمود flex (`justify-content:flex-start`، الأشرطة `flex:none`، الإطار `flex:1 1 0`،
  تمرير داخلي فقط؛ على ≥641px فقط) فلا يتجاوز الشاشة، وعلى ≤640px الشاشة بارتفاعها الطبيعي والصفحة تتمرّر (المعاينة `min-height:78vh` ثم المحادثة — كان رأس
  مساحة العمل يأخذ معظم 60٪ فتصير المعاينة ~80px ويفيض شريط الشرائح فوق المحادثة)؛ و`jeem-slides.js` بلا `scrollIntoView`: الشريط يُمرَّر أفقيًا بـ`scrollBy` على حاويته، والانتقال لشريحة بـ`reveal`
  (أقرب حاوية قابلة للتمرير داخل الصفحة أو `window.scrollTo`)، وكذلك عدّة التنقّل وإدراج عنصر في المستند. **قاعدة: لا `scrollIntoView` في أي كود يعمل
  داخل إطار معاينة.** وفي وضع التعديل يُحجز أسفل المعاينة مكان الشريط العائم (`padding-bottom`) فلا يغطي أسفل الصفحة.
- تحقق Playwright (1400×700 و390): الحذف يزيل `<img>` ويجعل الخلفية بلا صورة والزر مخفي لمكان فارغ، الطبقة ظاهرة أثناء تعديل لوحة المتابعة وتزول بعده، وكل
  ارتفاعات أسلاف الإطار = ارتفاع الشاشة (لا تمرير للصفحة) مع الهوية مفتوحة.
- **ملء الشاشة في وضع التعديل** (طلب المستخدم): زر «⛶ ملء الشاشة» (`#ff-edit-full`) في شريط التعديل → `ffEditFull(on)`: صنف `ff-full` على `#ff-preview-wrap`
  (`position:fixed; inset:0; z-index:70` تحت النوافذ 80) فيبقى شريط التعديل والأدوات العائمة ولوحة الخصائص وتنقّل الشرائح، ويُطلب ملء الشاشة **للصفحة كلها**
  (`documentElement.requestFullscreen`) لا للمعاينة وحدها حتى تظهر النوافذ (تغيير الصورة، الهوية). الخروج بالزر أو Esc المتصفح (`fullscreenchange`) أو
  إنهاء التعديل. **نافذة تغيير الصورة كانت تطول ~660px** لأن `<img>` المعاينة بلا مصدر (مكان فارغ) يتجاهل `aspect-ratio`: الآن ارتفاع ثابت 165px
  (150 على الجوال)، واختيار صورة من «صور التصميم» يزيل `empty` فتظهر المعاينة. تحقق Playwright (1384 و390): التغطية الكاملة والأدوات والنافذة فوقها (469px).

## نطاق تعديل الموديل في تصميم حر: الشريحة الحالية / العنصر المحدد / العرض كله (2026-11-24) ✅

- **الطلب**: الموديل كان يغيّر كل الشرائح ويعيد تصميم القالب حين يُطلب منه تعديل شريحة. السبب بنيوي: كان يتلقى الصفحة كلها ويعيدها كلها،
  فأي قيد في التعليمات «رجاء» لا ضمان. **القيد صار في الكود**: الموديل لا يرى إلا الجزء المطلوب، وجوابه يُتحقَّق منه ويُركَّب مكانه.
- **`Freeform/FreeformScope.cs`** (ساكن، مختبَر): `FindSlides` (عناصر بصنف `slide` كلمةً كاملة أو `data-slide`، غير متداخلة — القاعدة نفسها في
  `ffNavKit` فيتطابق الترتيب مع شريط التنقّل؛ <2 = لا شرائح)، `FindMarked` (العنصر بـ`data-jeem-target`)، `ElementEnd` بعدّ الوسوم المتداخلة من الاسم
  نفسه على نسخة `Mask` تُفرَّغ فيها script/style/التعليقات، `PageCss`/`KnownClasses` (أصناف الوسوم ومحدِّدات CSS)، `CheckFragment` (يزيل
  script/style/link وأسوار ```، يشترط عنصرًا جذرًا واحدًا بالوسم نفسه **ويعيد وسم البداية الأصلي حرفيًا** فلا يتغيّر إطار الشريحة ولا أصنافه، ويرفض
  أي صنف غير موجود في الصفحة؛ الرد الفارغ = سؤال بلا تعديل)، `Splice`، `StripLogoSrc`.
- **`FreeformBuilder.GenerateFragmentAsync`**: النظام المعتاد + `FragmentRules(scopeLabel)` (قيود: أعد الجزء وحده في `fragment`، لا أنماط/سكربتات/
  أصناف جديدة/سمات style تغيّر التصميم، المسموح نصوص وأرقام وصور وبنود من النوع نفسه، اللغة والاتجاه، لا صور/شعار بلا طلب، لا أرقام مخترعة،
  الغموض = سؤال بلا تعديل، `reply` يذكر ما تغيّر تحديدًا). المدخل: أنماط الصفحة للقراءة فقط + الجزء + آخر 6 رسائل؛ `query_data` متاح وتُسجَّل
  استعلاماته في كتلة الصفحة كلها. **محاولة ثانية واحدة** بسبب الرفض، ثم «لم أغيّر شيئًا» بالسبب (بلا إصدار).
- **الخادم** (`generate`): `FreeformGenerateRequest.Scope {kind: all|slide|element, index}` لتصميم قائم: slide ← `FindSlides[index]` (400 برسالة لصفحة
  ليست شرائح أو فهرس خارج المدى)، element ← `FindMarked` في `request.Html`؛ الجزء العائد يُركَّب في الصفحة (بقية الصفحة حرفيًا) ثم إصدار جديد. الرد
  يعيد `scope` و`changed`.
- **الواجهة**: `#ff-scope` فوق صندوق الكتابة لتصميم قائم (لا للمشاهد ولا لوحات المتابعة): «🎯 نطاق التعديل: الشريحة الحالية (3 من 16) | العرض كله»
  (الافتراضي الشريحة الحالية لصفحة بشريحتين فأكثر؛ «العرض كله» بإطار أصفر وتلميح «قد يُعدَّل أي جزء»). `FF.nav` من `nav-state`، و`ffCurrentScope()`
  يُرسل مع `generate`. «اطلب من جيم» في شريط الأدوات صار نطاق `element` (بلا تعليمات نصية). بعد إعادة الرسم تعود المعاينة إلى الشريحة نفسها
  (`FF.navRestore` → `{type:'nav', index}`) بدل القفز للأولى. رد كل تعديل فيه زر «↶ تراجع عن هذا التعديل» (`.ff-undo-msg` → `POST restore/{السابق}`،
  يُزال بعد الاستخدام)، ورد بلا تغيير يذكر «لم يتغيّر شيء في الصفحة».
- اختبارات `FreeformScopeTests` (الشرائح العليا فقط وتجاهل السكربتات والأصناف الشبيهة، التركيب يغيّر الشريحة وحدها، فرض القالب والرفض، العنصر المعلَّم
  والشعار، وتطابق عدد الشرائح مع `template.json` في كل نماذج الشرائح الحقيقية) 256/256. Playwright (1400 و390): الشريحة الحالية 1 ثم 3 من 16 بعد التنقّل،
  الطلب يحمل `{slide, 2}` والمعاينة تبقى على 3/16، زر التراجع يستدعي `restore/1` ويختفي، «العرض كله» يرسل `all`، «اطلب من جيم» يرسل `element` بعنصر
  معلَّم واحد، بلا أخطاء وبلا تمرير أفقي. **التزام الموديل الحقيقي بالجزء (ونسبة الرفض والمحاولة الثانية) لم يُجرَّب هنا.**
- **فحص فيض النص بعد كل تعديل ✅**: `ffNavKit` يجيب `{type:'check-overflow'}` بـ`{type:'overflow', i, items}` للشريحة الظاهرة: كل عقدة نص يخرج مستطيلها عن
  الشريحة أو عن أقرب حاوية بـ`overflow` غير مرئي (بتسامح 3px × معامل التصغير)، مع تجاهل النص القصير/الأرقام الزخرفية (≤3 أحرف أو أرقام فقط — «01» مقصوصة
  عمدًا في بعض القوالب). الأم تطلب الفحص بعد آخر `nav-state` لأي رد غيّر الصفحة (`FF.overflowCheck`)، وتعرض «⚠️ النص في الشريحة n أطول من مساحتها…» بزر
  «✂️ اختصره» يرسل طلب اختصار بنطاق تلك الشريحة. فحص الإنذار الكاذب على **كل نماذج الشرائح الجاهزة (56 نموذجًا، 485 شريحة): صفر**.
- **تنبيه تغيّر عدد الشرائح ✅**: في نطاق «العرض كله» يقارن `generate` `FindSlides` قبل وبعد ويعيد `slideCount {before, after}` عند الاختلاف، والرد يعرض
  «⚠️ عدد الشرائح تغيّر من 16 إلى 15 — إن لم تطلب ذلك اضغط «تراجع»» بجوار زر التراجع.

## بلوكات البيانات في تصميم حر: رسوم وأرقام من بياناتك الفعلية (2026-11-24) — المرحلة 1 ✅ (المحرك)

- **الفكرة** (طلب المستخدم: رسوم في العرض من بياناته الفعلية وإضافة رسمة جديدة منها): بدل أن يرسم الموديل SVG بنفسه في كل مرة (شكل متفاوت،
  وتحديث يعتمد على التزامه بقراءة `JEEM_DATA`)، الرسمة في الصفحة **وصف فقط** يرسمه محرّك المنصة من نتائج الاستعلامات المسجَّلة:
  `<div data-jeem-chart='{"q":"q3","type":"bar|line|pie|donut|funnel|treemap|scatter","x":"<عمود>","y":"<عمود>|[أعمدة]","title":"…","fmt":"compact|currency|percent|number","unit":"…","stacked":true,"limit":30}'>`
  و`<span data-jeem-value="q2.<عمود>" data-fmt="…" data-agg="sum|avg|min|max|count" data-row="0">`. **الخطة**: (1) المحرك ✅، (2) «📊 رسمة من بياناتي» في
  شريط الأدوات (اختيار جدول/مقياس/تقسيم/نوع أو عنصر من لوحة محفوظة) + قسم «البيانات» في لوحة الخصائص، (3) المحادثة وربط رسوم/أرقام القوالب الموجودة.
- **`wwwroot/js/jeem-blocks.js`** (`window.JEEM_BLOCKS.render/renderChart/renderValue/spec/format`): يقرأ `window.JEEM_DATA[q].rows`، يحوّل المواصفة إلى مواصفة
  `jeem-viz.js` (`specOf`: bar/line، multi لأكثر من y، pie مع تجميع ما بعد 7 في «أخرى»، funnel، treemap، scatter) ويرسم داخل `.jb-plot` بعنوان `.jb-title`؛
  العمود بالاسم بلا حساسية للحالة وإلا أول نصي للتسميات وأول رقمي للقيم؛ الأرقام العربية والفواصل تُفهم. **الألوان من القالب**: `colors`/`data-colors` وإلا متغيّرات
  الصفحة الشائعة (`--chart-1..4`، `--primary`، `--brand`، `--accent`، `--blue`، `--navy`…) تُضبط على الحاوية كرموز المحرّك، ودرجات أفتح منها حتى أربعة ألوان
  (المحرّك يعود لألوانه الافتراضية بأقل من ثلاثة). بلا نتيجة → `.jb-empty` («لا توجد نتيجة للاستعلام q9 — حدّث البيانات»/«غير مربوط»). يعيد الرسم عند
  التحميل وتحميل الخطوط وحدث `jeem:data` (الصفحة المنشورة الحيّة)، والشريحة المخفية تُرسم عند ظهورها (ResizeObserver في المحرّك). CSS مستقل يُحقن
  (`#jeem-blocks-css`) لأن الصفحة لا ترى أنماط المنصة؛ حركة الدخول من `opacity:0` فقط (`to` كانت تلغي شفافية مساحة الخط).
- **الحقن**: المعاينة (`ffSandboxHtml`: `#ff-viz-kit` + `#ff-blocks-kit` متى وُجد `data-jeem-chart|value`) والنشر (`FreeformDataBlock.EnsureBlocksKit` قبل آخر
  `</body>`). **الحفظ** من وضع التعديل يفرّغ محتوى كل `[data-jeem-chart]` (الرسم يُعاد دائمًا) ويحذف السكربتين و`#jeem-blocks-css`. التحديث بلا موديل
  و`live-data` لا يحتاجان شيئًا جديدًا: يستبدلان `JEEM_DATA` فيُعاد الرسم.
- **الموديل**: نظام البناء (عند وجود بيانات) صار يطلب البلوكات للرسوم والأرقام من الاستعلامات بدل رسم SVG/Canvas يدويًا، بارتفاع مناسب للبلوك.
- اختبارات `FreeformBlocksTests` (الحقن مرة واحدة وقبل آخر `</body>`، والنظام يذكر البلوكات مع البيانات فقط) 258/258. Playwright (1400 و390): 4 رسوم SVG (أشرطة،
  خط زمني، حلقة بالإجمالي، سلسلتان بشرح) بلون القالب `#1B7F4B`، قيم «9.8M ر.س» و«37.4٪» و«4»، بلوك بلا استعلام يعرض السبب، تحديث `JEEM_DATA` + `jeem:data`
  يغيّر القيمة إلى «15.5M ر.س»، الحفظ من التعديل ببلوك فارغ وبلا سكربتات المنصة، بلا أخطاء وبلا تمرير أفقي. **التزام الموديل الحقيقي باستخدام البلوكات لم يُجرَّب هنا.**

### المرحلة 2 ✅ — «📊 رسمة من بياناتي» وقسم «البيانات» في لوحة الخصائص (2026-11-24)

- **الخادم** (`Controllers/FreeformController.Data.cs`، `FreeformController` صار `partial`؛ صلاحية **تعديل** التصميم، في مشروعه وبمصادره المختارة):
  - `GET /api/freeform/{id}/data/catalog` → `{tables:[{table, name, rows, columns:[{name, kind: number|date|text}]}], dashboards:[{historyId, title, widgets:[{index, title, type}]}]}`
    من `DataProfileService` (الجداول المرئية للمستخدم فقط) ومن لوحاته المحفوظة (`HistoryStore.ListAsync` المفلترة بالصلاحيات) ذات العناصر بـ`query.sql`.
  - `POST …/data/query {table, agg, measure, dimension, grain, limit | historyId, widgetIndex, preview}` → SQL حتمي من `Freeform/FreeformDataQuery.Build`
    (الجدول والأعمدة من ملف التعريف فقط، معرّفات مقتبسة دائمًا `[…]` مع تهريب `]`، التجميع sum|avg|min|max|count|count_distinct على `TRY_CONVERT(float,…)`،
    التقسيم: عمود فئة `ORDER BY 2 DESC` أو تاريخ بحبيبة day|month|quarter|year على `TRY_CONVERT(date,…)` بآخر N فترات ثم تصاعديًا، بلا تقسيم = قيمة واحدة،
    الحد 1..60 وافتراضيًا 12/24؛ الأسماء المستعارة «إجمالي X»/«عدد السجلات»…) أو SQL عنصر اللوحة المحفوظة، ثم `RunReadOnlyQueryAsync` (بوابة الصلاحيات
    والقراءة فقط نفسها). بلا `preview` يُسجَّل الاستعلام في `FreeformQueries` (موثوق من الخادم، فيُحدَّث مع «تحديث البيانات» والنشر الحيّ؛ لتصميم قديم بلا
    تسجيل تُضم استعلامات كتلة الصفحة أولًا) بمفتاح جديد لا يتعارض مع المسجَّل والكتلة و`keys` المرسلة. يعيد `{key, sql, result, rows, columns, x, y, widgetType, title}`.
- **الواجهة**: زر 📊 (`#ffd-chart`) في الشريط العائم → `ff-data-modal` (`ffdmOpen`): تبويبا «من جدول» (الجدول بعدد صفوفه، المقياس + العمود (الرقمية فقط إلا
  للعدّ)، «التقسيم حسب» مع «بلا تقسيم — رقم واحد»، والفترة للتاريخ) و«من لوحة محفوظة» (عناصر اللوحات، والنوع يُقترح من نوع العنصر)، رقائق «طريقة العرض»
  (أعمدة/خط/حلقة/قمع/شجرية/رقم واحد؛ الرقم وحده بلا تقسيم)، العنوان (تلقائي من الاختيار)، و**معاينة بنتيجة حقيقية** (`preview:true` بتأخير 350ms، بمحرّك
  `JEEM_VIZ` و`JEEM_BLOCKS.spec` المحمَّل في المنصة بلا رسم تلقائي — حارس `#app-shell` في jeem-blocks.js). «إدراج في الشريحة» يسجّل الاستعلام ويرسل للإطار
  `insert {kind: chart|value, cfg|ref, entry}`.
- **داخل الإطار** (`ffDesignKit`): `addData(entry)` يضيف النتيجة إلى كتلة `#jeem-data` (أو ينشئها مع `#jeem-data-rt` أول `<head>`) وإلى `window.JEEM_DATA` —
  فتُحفظ مع الصفحة؛ `ensureBlocks` يحمّل `jeem-viz.js`/`jeem-blocks.js` عند أول بلوك (`#ff-viz-kit`/`#ff-blocks-kit`، تُحذف عند الحفظ)؛ البلوك المُدرج حر
  (560×320 في الشريحة) أو في التدفق، والرقم بطاقة رقم كبير + عنوان. **البيانات تُضاف قبل التحديد** (وإلا قرأت لوحة الخصائص أعمدة فارغة). النقر داخل الرسمة
  يحدد البلوك نفسه، والبلوكات مستثناة من تحرير النص (`LIVE` في عدّة النص) لأنها تُرسم من المواصفة.
- **لوحة الخصائص**: قسم «البيانات» للرسمة (النوع، عمود التسميات، عمود القيم من أعمدة النتيجة، العنوان، التنسيق) و«القيمة من البيانات» للرقم (العمود، التجميع،
  التنسيق) → `chart-cfg`/`value-cfg` مع تراجع، وزر «📊 تغيير مصدر البيانات…» يفتح النافذة بوضع الاستبدال.
- اختبارات `FreeformDataQueryTests` (مجمّع/زمني/قيمة واحدة/تهريب، ورفض الجدول والأعمدة والتجميع غير المعروفة) 260/260. Playwright (1400 و390) بخادم وهمي: المعاينة
  3 أشرطة و«3 صف من بياناتك الفعلية» والعنوان التلقائي، التاريخ يُظهر الفترة والخط يُعاين، الإدراج يسجّل ويضع البلوك برسمه و`#jeem-data` فيه q1، لوحة الخصائص
  بأعمدة النتيجة، التحويل إلى حلقة ثم التراجع، رقم واحد «9.9M»، عنصر لوحة محفوظة بنوع قمع، الحفظ: بلوكات فارغة وكتلة بثلاثة مفاتيح وبلا سكربتات المنصة، وإعادة
  الرسم بعد الحفظ ترسمها؛ بلا أخطاء وبلا تمرير أفقي. **على SQL Server حقيقي لم يُجرَّب** (صياغة التاريخ والتجميع على أعمدة نصية).
### المرحلة 3 ✅ — ربط رسوم وأرقام القالب الموجودة + تحقق بلوكات الموديل (2026-11-24)

- **الربط من وضع التعديل** (`ffDesignKit`): `info()` يضيف `bind` لعنصر محدد ليس بلوكًا: `numHost` (عنصر بلا أبناء نصه رقم ≤24 حرفًا بعد حذف
  العملات/الوحدات/الرموز) ← `{kind:'value', text, fmt}`، وإلا `chartHost` (حتى 5 مستويات لأعلى حتى الشريحة: `canvas`، أو صنف chart/graph/plot/donut/pie/gauge/bars،
  أو `svg` فيه ≥3 أشكال ونص؛ يُستبعد `.deco` وما ≥80٪ من الشريحة وما أصغر من 90×60) ← `{kind:'chart', w, h}`. لوحة الخصائص: قسم «ربط ببياناتك» بزر
  «🔗 اربط…» → `ffdmOpen('bind')` (`FFDM.bind`): للرقم «التقسيم» معطَّل و«رقم واحد» فقط، وللرسمة يبدأ بأول تقسيم و«رقم واحد» ممنوع (`ffdmBindPreset`).
  «ربط» يسجّل الاستعلام كالمعتاد ثم `op:'bind'`: **الرقم** يأخذ `data-jeem-value` + `data-fmt` (نسبة/عملة ر.س/مختصر من نصه) ورمز العملة الأصلي
  `data-prefix` («$3.5B») أو `data-unit` («3.5B$») — `jeem-blocks.js` صار يقرأ `data-prefix`، ولوحة الخصائص فيها «قبل الرقم/بعد الرقم»؛ **الرسمة**
  تُستبدل ببلوك حر داخل حاوية الأصل نفسها بإحداثياته ومقاسه بوحدات CSS الأصلية (`scaleOf`: من أقرب عنصر HTML له `offsetWidth`، لأن القالب إطار ثابت
  مصغَّر بـ transform) وبألوان أشكاله (`colorsOf` ← `cfg.colors`)، **والأصل يُخفى لا يُحذف** (`data-jeem-replaced` + `display:none !important`) لأن سكربت
  القالب يرسم فيه عند التحميل. التراجع يعيد كل شيء. `ffRebrand` يعيد تلوين ألوان `data-jeem-chart`.
- **تحقق بلوكات الموديل** (`Freeform/FreeformBlockValidator.cs`): `Validate(html, queries)` (خارج السكربتات، مواصفة مرمّزة HTML مقبولة): JSON صالح، نوع من
  `ChartTypes`، `q` نتيجة مسجَّلة، x/y وأعمدة `qN.col` موجودة في صفّها الأول بلا حساسية لحالة الأحرف (نتيجة بلا صفوف لا تُفحص أعمدتها). في
  `GenerateFragmentAsync` المشاكل تُعاد للموديل في المحاولة الثانية ثم يُقبل الجزء مع «⚠️ تنبيه» في الرد؛ في الصفحة كاملة تُلحق بالرد. نداء الجزء
  يتلقى `Describe(...)` (المفاتيح المسجَّلة وأعمدتها وعدد صفوفها) و`FragmentRules` تسمح ببلوكات البيانات مع style للمقاس والموضع فقط. ذيل نتيجة
  `query_data` يذكر المفتاح للبلوك. اختبارات `FreeformBlockValidatorTests` 263/263.
- تحقق Playwright على `sl-ai-business-plan` الشريحة 12 (1400 و390): «$3.5B» → `$9.9M` ثم «بعد الرقم: ر.س» → «9.9M ر.س»، `svg#c12` → بلوك في المكان والمقاس
  نفسيهما (374×181 و180×87 على الشاشة) بألوان القالب الثلاثة، التراجع يعيد الأصل، والحفظ يحمل `data-jeem-value` والبلوك والأصل المخفي ومفاتيح البيانات؛
  بلا أخطاء JS وبلا تمرير أفقي. **التزام الموديل الحقيقي بالبلوكات ونسبة التصحيح لم يُجرَّبا هنا.** قيد واعٍ: نتيجة استعلام ربطٍ تُرُوجع عنه تبقى في كتلة
  البيانات (تُحدَّث بلا حاجة) — لا تُحذف آليًا لأن سكربت الصفحة قد يقرؤها.
- **علّة أبلغ عنها المستخدم** («استبدل الرسومات الموجودة بالرسومات من ملف المصدر المرفق» بنطاق الشريحة والمصدر Opportunities مختار ← الموديل ردّ
  «ملف المصدر غير متاح، أرسله»): النظام لم يربط كلمة «المصدر» بالجداول المختارة. الآن `FreeformBuilder.SourcesNote` (مع البيانات: «المصدر/ملف
  المصدر/الملف المرفق/البيانات = هذه الجداول، استعلم منها ولا تطلب الملف») و`NoDataNote` (بلا بيانات: لا اختلاق، وأرشده لـ«🗄️ المصادر» أو «الملفات»)،
  و`ReplaceChartRule`: رسمة القالب (svg/canvas يرسمها سكربت الصفحة بالمعرّف) **لا تُحذف**؛ تأخذ `data-jeem-replaced` (و`jeem-blocks.js` صار يخفيه
  بـ`[data-jeem-replaced]{display:none!important}`) ويُوضع بعدها بلوك بالأصناف نفسها فيأخذ موضعها من أنماط القالب. نداء الجزء يتلقى
  `FreeformScope.ChartElements(fragment)` («svg#c12.chart»، بلا الزخارف والمستبدَلة والبلوكات). تحقق Playwright على الشريحة 12: الأصل مخفي، البلوك
  بثلاثة أشرطة في مكانه، التنقّل يعمل، صفر أخطاء. اختبارات `FreeformSourceReferenceTests` 265/265. **رد الموديل الحقيقي على الطلب نفسه لم يُجرَّب هنا.**
- **«من لوحة محفوظة» بقائمة اللوحات و«كما في اللوحة»** (طلب المستخدم بصورة: القائمة الطويلة لكل اللوحات صعبة، والرسمة يجب أن تظهر كما في اللوحة):
  `#ffdm-dash-pick` قائمة منسدلة باللوحات (العنوان + عدد العناصر) ثم عناصر اللوحة المختارة بطاقات `.ffdm-wi` (العنوان + نوعه بالعربية `FFDM_WTYPE_AR`).
  نوع العرض الجديد `asis` «كما في اللوحة» (في تبويب اللوحة فقط، ويُختار تلقائيًا عند اختيار عنصر؛ لا يظهر لربط رقم): الخادم صار يعيد مع نتيجة عنصر
  اللوحة `widget {xKey, yKey, series, stacked, areaFill, centerLabel}`، و`ffdmAsIs(d)` يحوّله إلى مواصفة بلوك تُرسم بمحرّك اللوحات نفسه (`jeem-viz`):
  السلاسل المتعددة بأسمائها (`cfg.labels`) والتكديس، kpi → رقم، table/progress-table → **بلوك جدول جديد** (`type:"table"` في `jeem-blocks.js`
  `tableOf`: الأعمدة من `cols` أو كلها، حتى `limit` صفًا، أرقام منسّقة؛ مضاف لـ`ChartTypes` في المدقّق ولوثائق الموديل ولقائمة النوع في لوحة الخصائص)،
  والأنواع بلا مقابل (مراحل/مقياس…) بأقرب شكل مع تنبيه في سطر الحالة. **علّة أُصلحت**: تبديل التبويب كان يُبقي معاينة التبويب الآخر وعدد صفوفه
  («4 صف» مع «اختر مصدر البيانات») — `ffdmSetMode` والطلب بلا مصدر يفرّغان المعاينة والحالة ويلغيان الطلب الجاري (`++FFDM.seq`). تحقق Playwright
  (1400 و390): لوحتان في القائمة، عنصر مكدّس بسلسلتين وشرح بأسمائهما، دائري، مؤشر «1.2M»، جدول بثلاثة صفوف، مراحل بتنبيه، التبديل لأعمدة والعودة،
  الإدراج يضع بلوك جدول مرسومًا، وتبويب الجدول بلا «كما في اللوحة»؛ بلا أخطاء JS. 265/265.
- **الألوان: «ألوان العرض» أو «ألوان اللوحة»** (طلب المستخدم): صف «الألوان» في نافذة البيانات (`#ffdm-colors`، `FFDM.pal`؛ مخفي للرقم الواحد).
  «ألوان العرض» (الافتراضي) = لا ألوان في المواصفة فتتبع الصفحة والهوية — والمعاينة تعرضها فعلًا: النافذة تطلب من الإطار `op:'palette'`
  (`d-palette`: ألوان رسمة القالب عند الربط `colorsOf`، وإلا `JEEM_BLOCKS.palette` للشريحة) وتضعها على `--chart-1..4` للمعاينة (`ffdmPreviewPal`).
  «ألوان اللوحة» = `FFDM_DASH_PAL` (رموز `--chart-1..4` الفاتحة في tokens.css) تُثبَّت في المواصفة `colors` + `"palette":"dashboard"`، و`ffRebrand`
  يتخطّاها فلا تغيّرها الهوية. لوحة الخصائص فيها قائمة «الألوان» للتبديل بعد الإدراج (تحذف `colors/palette` للعودة لألوان العرض). تحقق Playwright
  (1400 و390): المعاينة بألوان القالب (#7532BF) ثم بألوان اللوحة (#127DBD/#00BAB3)، الإدراج يحفظ `palette:dashboard`، التبديل من الخصائص يعيد ألوان
  القالب، والرقم الواحد بلا صف الألوان؛ بلا أخطاء.
- **علّة أبلغ عنها المستخدم: الرسمة المضافة تظهر في المحرر ولا تظهر في «العرض»**: الصفحة المحفوظة لا تحمل `jeem-viz.js`/`jeem-blocks.js`
  (الحفظ من وضع التعديل يحذف سكربتات المنصة، والمعاينة تحقنها في `ffSandboxHtml` والنشر في الخادم بـ`EnsureBlocksKit`)، لكن «فتح في نافذة»
  والتنزيل و«PDF» كانت تستخدم الصفحة المحفوظة كما هي فتظهر البلوكات فارغة. الآن `ffWithRuntimeKits(html, kind)` في app.js تضيف محرّك الرسوم
  وراسم البلوكات (وشريط المصغّرات للشرائح) لهذه النسخ الثلاث، والتنزيل صار بروابط مطلقة إلى المنصة كالنافذة. الرابط المنشور يحتاج «تحديث
  النشر» ليعرض الإصدار الجديد. تحقق Playwright (1400 و390): رسمة على الشريحة 7 ← حفظ ← المعاينة بعد الحفظ والنافذة المستقلة ترسمان SVG.
- **قائمة «🔄 تحديث البيانات» متداخلة** (علّة أبلغ عنها المستخدم بصورة): بنودها بعنوان وشرح، والارتفاع الثابت 34px لـ`.dropdown-item` كان يقصّها فيطفو
  الشرح فوق البند التالي ويخرج آخرها من القائمة. الآن `#ff-refresh-panel .dropdown-item` بارتفاع تلقائي والعنوان والشرح سطران (بلا «—»)، ومربع «تحديث
  تلقائي» بجوار نصّيه؛ وعلى ≤640px القائمة `fixed` بعرض الشاشة تحت الزر (كانت تخرج 63px من حافة الشاشة).
- **تبويب ثالث «من المكتبة» في نافذة البيانات** (طلب المستخدم: كما في «إضافة عناصر» باللوحة): رقائق «عناصر جاهزة | مكتبة المؤشرات | عناصر خام» + بحث
  + تصنيف المؤشرات + قائمة اختيار (60 ثم «عرض المزيد»)؛ القوائم من `awItems(o)` نفسها (صارت تقبل حالة غير `AW`) وجسم الطلب من `awItemBody`
  (مُستخرَج من `awAddSelected`). **العنصر الجاهز والمؤشر وصفٌ بلا أعمدة فيحتاج الموديل**: بعد الاختيار تعرض المعاينة شكله بأرقام توضيحية
  (`awPeekHtml`) وزر «🪄 ابنِه من بياناتي» (لا بناء تلقائي) → `POST /api/freeform/{id}/data/build` (`FreeformDataBuildRequest {ref,title,type,
  prompt,lang}`، صلاحية تعديل، مصادر التصميم): `TemplatePromptService.ResolveAsync` ثم `StagedTemplateBuilder.BuildOneAsync` (نداء موديل واحد)؛
  العنصر المبني يُحفظ **في ذاكرة الخادم** 30 دقيقة (`BuiltWidgets` بمفتاح المستخدم|التصميم|المعرّف) ويُعاد `buildId` فقط، أو `reason` صادق إن تعذّر
  (`noData`). ثم `data/query {buildId}` يعامله كعنصر لوحة محفوظة (SQL من الذاكرة لا من المتصفح، «كما في اللوحة» افتراضيًا، التسجيل في
  `FreeformQueries` عند الإدراج) — فالموديل لا يُستدعى عند المعاينة ولا الإدراج ولا «تحديث البيانات». **العنصر الخام بلا موديل**: هو «من جدول»
  نفسه، فبطاقته تنقل إليه بطريقة العرض المقابلة وأول عمود مناسب (تاريخ للاتجاه). `ffdmW()` = وضع اللوحة أو المكتبة. تحقق Playwright بخادم وهمي
  (1400 و390): البطاقتان، الشكل قبل البناء والزر، انتظار بشعار جيم، المعاينة 4 أشرطة «كما في اللوحة»، الإدراج بـ`buildId` وبلوك مرسوم، سبب
  الرفض للمؤشر غير الممكن، المكتبة 60 من 3,839 و38 تصنيفًا والبحث، جسم `kpi`، والخام ينقل إلى «من جدول» بخط على عمود التاريخ؛ بلا أخطاء. 265/265.
  **البناء الحي بالموديل لم يُجرَّب هنا.**
- **زر «ربط» معطّل بلا سبب واضح** (سؤال المستخدم بصورة): ربط رسمة قالب بمؤشر من المكتبة يعيد رقمًا واحدًا، والرقم لا يحل محل رسمة فيُعطَّل الزر عمدًا،
  لكن السبب كان سطرًا رماديًا صغيرًا. الآن: (1) عند ربط رسمة، المؤشر يُبنى **اتجاهًا شهريًا تلقائيًا** (`trend:true` في `data/build` → النوع line، تعليمات
  «قيمته لكل شهر آخر 12 شهرًا» تُلحق بالموجز، ومسودة SQL المكتبة للرقم الواحد لا تُمرَّر)، وشاشة ما قبل البناء تذكر ذلك؛ (2) إن عاد رقم واحد رغم ذلك، السبب
  في صندوق تنبيه `#ffdm-status.warn` («⚠️ «ربط» معطّل: …») وزر «📈 ابنِه اتجاهًا عبر الزمن» داخل المعاينة. تحقق Playwright (1400 و390): البناء الآلي
  بـ`trend` يفعّل «ربط» برسمة خط، الرقم الواحد يعطّل بالسبب والزر، الزر يعيد البناء ويفعّل الربط، والربط يضع بلوك خط ويخفي `svg#c12`. 265/265.

## الصفحة الرئيسية للمنصة `wwwroot/home.html` (2026-11-25) ✅

- **القرار** (طلب المستخدم): صفحة رئيسية بأسلوب العرض الاستراتيجي للإدارة العليا، بالعربية الفصحى والإنجليزية، تظهر أول ما يُفتح النظام وفيها
  زر «تسجيل الدخول». المنتجات المعروضة هي أنواع «تصميم حر» الستة فقط، كل نوع منتج مستقل بقوالبه.
- **التوجيه** (Program.cs): أُزيل `UseDefaultFiles`؛ `MapGet("/")`: سلسلة استعلام (روابط `?share=`/`?embed=1&pub=` القديمة) أو مستخدم مسجَّل →
  التطبيق (`SpaIndex.Result`)، وإلا `home.html` (`HomePage`، `no-cache`). `/home` الصفحة دائمًا، و`/app` التطبيق دائمًا (شاشة الدخول/اختيار
  المنظمة لغير المسجَّل) — أزرار الدخول تشير إليه. `/o/{slug}` و`/d/…` و`/p/…` كما هي. «مش منظمتك؟» في شاشة الدخول صار يوجّه إلى `/app`،
  والخروج إلى `/` (الصفحة الرئيسية). **قرار افتراضي (لم يُحسم صراحة)**: المسجَّل يذهب إلى التطبيق مباشرة.
- **الصفحة**: مستقلة تمامًا (لا app.js ولا jeem.css)، خط Tanseek وألوان الهوية، أقسام: الافتتاحية، التحدي، الحل بثلاث خطوات، المنتجات (تبويبات
  `role=tablist` بلوحة المفاتيح ولقطة وقيمة ومثال وعدد القوالب ومزايا)، قالب لكل شيء، ما يميّزنا، الحوكمة (قسم داكن)، الأثر، الختام. النصوص في
  كائن `T.ar/T.en` وعناصر `data-i`/`data-i-html`/`data-i-aria`/`data-i-alt`؛ التبديل يغيّر `lang/dir` والعنوان واللقطات ويُحفظ في localStorage
  `jeemHomeLang`. الأعداد من الكتالوج الفعلي: مستندات 6، شرائح 63 (6 + «الأزرق المؤسسي» + 56 مجلدًا)، تصاميم 6، لوحات 6 + كتالوج 240، حركة 5، لوحات
  المتابعة كتالوج 240 منها 22 تفاعلية؛ «+320 قالبًا» و«+3,800 مؤشر»؛ زمن «25 ثانية» من القياس الحقيقي الموثّق أعلاه مع ملاحظته. **عند تغيّر
  الكتالوج تُحدَّث الأعداد في `home.html`.** ظهور تدريجي للأقسام فقط حين يعمل `IntersectionObserver` (صنف `js-reveal`)، ويُلغى مع
  `prefers-reduced-motion` وفي الطباعة.
- **اللقطات** `wwwroot/img/home/<kind>-<ar|en>.jpg` (لوحات المتابعة عربية فقط): مولَّدة بسكربت Playwright في scratchpad (`home/shots.js`) من
  صفحات المعرض التوضيحية (`JEEM_FF_DEMO`)، ونموذج شرائح جاهز (`sl-ai-business-plan`)، ومحرّك معرض اللوحات (`JR.mount` على `pl-sales-pipeline`) —
  أرقام توضيحية فقط، بلا بيانات عملاء.
- تحقق Playwright (1440 و390 وتقليل الحركة): الاتجاه والعنوان، 6 تبويبات و3 أزرار دخول إلى `/app`، التبديل للإنجليزية بلا نص عربي متبقٍّ وبلقطات
  إنجليزية، حفظ اللغة بعد إعادة التحميل، لوحة المفاتيح على التبويبات، صفر صور مكسورة وصفر أخطاء JS وبلا تمرير أفقي، وصفر حركات مع تقليل الحركة.
  **التوجيه على الخادم الحقيقي لم يُجرَّب هنا** (يحتاج SQL Server) — `dotnet build` ناجح.


## تصاميم حر كمصدر بيانات: سجلات الإدخال `jeem.records` (2026-11-25) — المرحلة 1 ✅

- **الطلب**: صفحات تصميم حر التي فيها إدخال (نماذج، طلبات، استبيانات) تصير مصدر بيانات تُبنى عليه لوحات المتابعة. **قرار المستخدم**:
  الإدخال في الرابط المنشور لأعضاء المنظمة افتراضيًا، ويُفتح للعامة لكل تصميم وحده. الخطة: (1) مخزن سجلات وواجهة الصفحة ✅،
  (2) جدول SQL في مخطط المشروع يظهر في «المصادر» وطبقة المعنى ولوحة عليه، (3) تعليم الموديل + قوالب إدخال + «اعمل لوحة من بيانات هذا التصميم».
- **لماذا لا `jeem.store`**: قيمة واحدة لكل مفتاح (≤256KB) فيتعارض المُدخلون المتزامنون، وقبل النشر في localStorage المتصفح فقط.
- **المخزن** `Freeform/FreeformRecordStore.cs`: جدول `dbo.FreeformRecords(Id IDENTITY, DesignId, Collection, Json, CreatedByUserId, CreatedByName,
  AnonId, CreatedAt, UpdatedAt, UpdatedByName)` بفهرس (DesignId, Collection, Id). **بمعرّف التصميم لا العرض** فالمحرر والنافذة والرابط المنشور
  يتشاركونها منذ البداية. الحدود: مجموعة `[حروف أي لغة/أرقام/_.-]{1,60}`، ≤20 مجموعة و≤50,000 سجل لكل تصميم، السجل كائن JSON ≤32KB و≤100 حقل.
  `Merge` للتعديل الجزئي (null يحذف الحقل). تُحذف مع التصميم.
- **السياسة**: عمود `FreeformDesigns.RecordsPolicy` (`members` افتراضيًا للقائم والجديد، أو `public`)، `PUT /api/freeform/{id}/records-policy` لصاحبه.
  **القواعد** `FreeformRecordAccess` (نقية، مختبَرة): داخل المنصة يُدخل كل من يرى التصميم؛ في الرابط: members = مسجَّل من منظمة التصميم (وإلا 401/403)،
  public = أي أحد. القراءة: العضو والمدير يريان الكل، والزائر سجلاته فقط (بمعرّفه المجهول). التعديل/الحذف: صاحب السجل أو المدير (من يملك تعديل
  التصميم: صاحبه، المسؤول، مشاركة بالتعديل). **الزائر المجهول**: `presentation.html` يولّد `jeem.anon` (32 hex في localStorage) ويرسله بترويسة
  `X-Jeem-Anon` (`NormalizeAnon` يقبل 16–64 hex/شرطة). حد الإدخال (`FreeformRecordsService`، `RateLimiter` المساعد): 60/10 دقائق للزائر، 300 للمسجَّل،
  3000/ساعة للتصميم → 429.
- **API**: داخل المنصة `GET /api/freeform/{id}/records` (المجموعات بأعدادها + السياسة + `manager`)، `GET|POST …/records/{collection}`
  (`?limit≤1000&order=desc&after=&mine=true` → `{items, total, hasMore, next, onlyOwn}`)، `PUT|DELETE …/records/{collection}/{rowId}` (`{data, replace}`)،
  `GET …/records/{collection}/export.csv` (للمدير، UTF-8 بـBOM، أعمدة = اتحاد الحقول + أدخله/التاريخ). والرابط المنشور بالمسارات نفسها تحت
  `/api/presentations/{id}` و`by/{org}/{slug}` (`[AllowAnonymous]`، يحلّ التصميم من `SourceKind=freeform`/`SourceId`، وإلا 400 `no_records`)؛
  `/access` صار يعيد `records {policy, canAdd, seesAll}`. الحقل في كل سجل: `{id, data, createdAt, createdBy, updatedAt, mine, canChange}`.
- **الجسر** (`jeem-page.js`): `jeem.records(name)` → `add(fields)`, `list({limit, order, after, mine})`, `all()` (يجمع الصفحات), `update(id, fields, {replace})`,
  `remove(id)`, `watch(fn, ms)` (استطلاع ≥10 ث). رسائل `rec-list/rec-add/rec-update/rec-remove` تجيبها: `presentation.html` (بالترويسة المجهولة)،
  ومعاينة المحرر `ffRecordsCall` في app.js (واجهة التصميم دائمًا، منشورًا أو لا؛ المصغّرات قراءة فقط)، والنافذة المستقلة (`meta jeem-design` → واجهة
  التصميم مباشرة)، وخارج المنصة مصفوفة في localStorage بالعقد نفسه.
- **الواجهة**: زر «📝 السجلات» في شريط أدوات التصميم بعدّاد (`#ff-records-count`) يفتح `ff-rec-modal`: سياسة الإدخال (لصاحبه، مع تأكيد قبل الفتح للعامة
  ورسالة في المحادثة)، رقائق المجموعات بأعدادها، جدول أحدث 500 سجل بالحقول والمُدخِل والتاريخ وحذف لمن يحق له، تحديث، وتنزيل CSV. حالة فارغة
  تشرح `jeem.records` وما يُطلب من جيم. Escape على المستند (بعد `confirm` يعود التركيز إلى body).
- تحقق: `FreeformRecordsTests` (السياسة الافتراضية والعامة، الرؤية، الملكية بالمستخدم والمعرّف المجهول، التحقق، الدمج، CSV) 271/271. Playwright (1400 و390)
  بخادم وهمي بالقواعد نفسها: صفحة نموذج تضيف سجلين من المعاينة وتعرضهما، العدّاد ٢، النافذة بالمجموعة والجدول وCSV، الحذف، تغيير السياسة يرسل PUT،
  Escape يغلق؛ الرابط المنشور لزائر: members → «sign_in_required» داخل الصفحة، public → الإضافة بترويسة `X-Jeem-Anon` و«طلباتك فقط»؛ بلا أخطاء JS وبلا
  تمرير أفقي. **على SQL Server والرابط الحقيقي لم يُجرَّب هنا.**

### المرحلة 2 ✅ — السجلات جداول في «المصادر» (2026-11-25)

- **`Freeform/FreeformRecordTableService.cs`** (singleton): كل مجموعة سجلات تُبنى جدولًا حقيقيًا في مخطط المشروع عبر مسار «الملفات» نفسه — ملف
  في `repo_Files` مرتبط بمصدر داخلي `SourceUrl = jeem-design://<designId>/<collection مرمّزة>` (`SourceUrlOf`/`Parse`)، فيظهر في «المصادر» وتحلّله
  طبقة المعنى وتعمل عليه المحادثة ولوحات المتابعة و«📊 رسمة من بياناتي» والتحديث الحيّ والبصمة **بلا أي تغيير فيها**. `BuildUpload`: `Flatten`
  (في `FreeformRecordsService`؛ «رقم السجل» + اتحاد الحقول بترتيب أول ظهور + «أدخله» و«تاريخ الإدخال»؛ المركّب JSON نصًا) ← `TabularFileParser.InferTypes`
  (أنواع رقم/تاريخ/نص كأي CSV) و`Content` = CSV بـBOM للتنزيل. أول مرة `RepositoryStore.SaveAsync` (الاسم «سجلات: <المجموعة> — <عنوان التصميم>»،
  التصنيف «بيانات من التصاميم»، **الصاحب = صاحب التصميم**، مشروع التصميم) + تحليل المعاني؛ بعدها `UpdateDataAsync` (هوية الجدول ثابتة) والمعاني تُعاد
  فقط إن تغيّرت الأعمدة (لا نداء موديل مع كل إدخال). `Schedule` بعد كل إضافة/تعديل/حذف بتأخير 4 ثوانٍ يدمج المتتالي في بناء واحد، وقفل لكل
  (تصميم، مجموعة). مجموعة بلا سجلات لا تُنشئ جدولًا.
- **التحكم**: عمود `FreeformDesigns.RecordsAsSource` (BIT افتراضيًا 1)، `PUT /api/freeform/{id}/records-source {on}` (لمن يملك التعديل؛ التشغيل يبني كل
  المجموعات فورًا، والإيقاف يُبقي الجداول لقطة أخيرة)، `POST …/records/{collection}/sync` (بناء الآن). `GET …/records` يعيد `asSource` و`tables` (لكل
  مجموعة: الملف والجدول والصفوف والأعمدة وآخر تحديث). حذف التصميم يفكّ ربط جداوله (`UnlinkDesignAsync`) فتبقى لقطة. `RepositoryStore.FindBySourcePrefixAsync`.
- **«الملفات»**: `LinkedFileFetcher.ProviderOf` يعيد `design` للمخطط الداخلي؛ شارة `.link-badge.design` «📝 من تصميم حر» بلون بنفسجي، وزر 🔄 «إعادة البناء من
  سجلات التصميم» يمر من `POST /api/repository/files/{id}/sync` الذي يتعرّف على المخطط ويعيد البناء من السجلات بالرد نفسه (أعمدة مضافة/محذوفة).
- **نافذة «📝 السجلات»**: قسم `#ffr-source`: مربع «استخدم السجلات مصدر بيانات» وسطر الجدول للمجموعة المختارة («🗄️ سجلات: … · n صف · n عمود · حُدِّث …»
  أو «⏳ لم يُبنَ بعد») وزر «🔄 مزامنة الآن»؛ التغيير يعيد تحميل المصادر ويبطل جدوى المعرض.
- **قيود واعية**: الجدول ملك صاحب التصميم فيراه هو ومن يمنحه الملف بالاسم (قيد الملفات المعتاد)، لا كل من يرى التصميم. حذف الملف من «الملفات» والسجلات
  ما زالت مصدرًا يعيد إنشاءه مع الإدخال التالي — لإيقافه أطفئ «استخدم السجلات مصدر بيانات». إعادة تسمية حقل في الصفحة = عمود جديد (يُعالج بـ«تصحيح الأعمدة»).
- تحقق: `FreeformRecordTableTests` (المخطط الداخلي ذهابًا وإيابًا بالعربية، المزوّد، الاسم، أنواع الأعمدة والتسطيح والـBOM) 273/273. Playwright (1400 و390) بخادم
  وهمي: القسم «لم يُبنَ بعد» ثم «مزامنة الآن» يعرض الجدول بصفّيه، الإيقاف يرسل `on:false` ويخفي السطر والتشغيل يعيده، «الملفات» تعرض الشارة البنفسجية وزر
  إعادة البناء، بلا أخطاء JS وبلا تمرير أفقي. **إنشاء الجدول الفعلي على SQL Server (SaveAsync/UpdateDataAsync من سجلات) لم يُجرَّب هنا.**

### المرحلة 3 ✅ — الموديل وقوالب الإدخال و«اعمل لوحة متابعة منه» (2026-11-25)

- **نظام الموديل** (`FreeformBuilder.BuildSystemPrompt`): قاعدة جديدة بعد `jeem.store`: الصفحات التي تجمع إدخالات من أشخاص كثيرين (طلب، استبيان، حضور،
  سجل متابعة، شكوى، حجز) تحفظ كل إرسال بـ`jeem.records('<مجموعة>').add({…})` لا بـ`jeem.store`، باسم مجموعة عربي قصير وحقول **ثابتة واضحة**
  وقيم رقمية أرقامًا واختيارات من قائمة ثابتة (لأن كل حقل يصير عمودًا في جدول المصدر)، والعرض بـ`list/watch/update/remove`، والتعامل مع
  `sign_in_required`/`rate_limited`/`forbidden`، وعدم عرض بيانات الآخرين حين `onlyOwn`.
- **أربعة قوالب إدخال** من نوع «تصميم» في `FreeformTemplates.Builtin` (مع ترجمتها في `FreeformTemplateI18n`؛ **المفاتيح لا يُعاد تسميتها**):
  `ds-entry-request` «نموذج طلبات» (ops، مجموعة «الطلبات»)، `ds-entry-survey` «استبيان رأي» (cx، «الردود»)، `ds-entry-log` «سجل متابعة»
  (projects، «التحديثات»)، `ds-entry-attendance` «تسجيل حضور» (hr، «الحضور») — كل موجز يحدد المجموعة والحقول بأسمائها وأنواعها. أعداد قوالب
  التصاميم في `home.html` صارت 10. المعاينة التوضيحية (`jeem-ff-demo.js`): `rating()` لأقسام «تقييم/استبيان» (أسئلة 1–5 بأزرار `aria-pressed`)،
  و«الاختيار والتعليق/التسجيل» ← نموذج.
- **«📊 اعمل لوحة متابعة منه»** في قسم المصدر بنافذة «📝 السجلات» (يظهر حين بُني جدول المجموعة): `ffRecordsMakeDashboard` يغلق النافذة، يحمّل المصادر،
  ويبدأ لوحة جديدة في «لوحات المتابعة» داخل تصميم حر (`ff-new-btn` → `ffSetKind('track')` → `ffSetSources({files:[الجدول]})` → `ffSetTemplate(null)`)
  ثم يرسل طلبًا جاهزًا (مؤشرات العدد والقيم الرقمية، اتجاه بحسب «تاريخ الإدخال»، توزيعات حقول الخيارات، أحدث السجلات، فلاتر) عبر `ffAsk` → `/api/chat`
  مقصورًا على جدول السجلات وحده. جدول لم يظهر في المصادر بعد → رسالة «اضغط مزامنة الآن».
- تحقق: `FreeformRecordEntryTemplateTests` (القوالب الأربعة بمجموعاتها والجسر والترجمة، والنظام يعلّم `jeem.records`) 278/278. Playwright (1400 و390): الزر ظاهر،
  النقر يغلق النافذة ويفتح لوحات المتابعة بشريحتي «بلا نموذج» و«المصادر: سجلات: الطلبات…» و`/api/chat` يحمل `files:[f9], systems:[]` وطلب «ابنِ لوحة
  متابعة لبيانات…» واللوحة تُرسم (عنصران)؛ المعاينات التوضيحية للقوالب الأربعة بنماذجها (الاستبيان بأسئلة تقييم) بلا أخطاء وبلا تمرير أفقي.
  **التزام الموديل الحقيقي بـ`jeem.records` في الصفحات المولَّدة لم يُجرَّب هنا.**

## إضافة شريحة من شريط أدوات التصميم (2026-11-25) ✅

- **الطلب** (بصورة لشريط الأدوات العائم): زر «إضافة شريحة» يفتح نافذة للاختيار بين شريحة من أي نموذج موجود أو شريحة فارغة بتصميم العرض الحالي.
- **الزر** `#ffd-slide` أول الشريط العائم (`#ff-dtools`) في وضع «✏️ تعديل»، يظهر فقط لتصميم شرائح (`kind=slides` أو `FF.nav.n ≥ 2`). يفتح `ff-slide-modal`
  بثلاثة تبويبات: **شريحة فارغة** (بطاقة واحدة)، **من هذا العرض** (مصغّرة لكل شريحة → نسخة منها)، **من نماذج أخرى** (نماذج الشرائح الجاهزة `kind=slides`
  ذات `page` من `/api/freeform/templates` ببحث ورقائق تصنيف `.ffsl-cats` (`DK_CATS`: «كل النماذج» + التصنيفات التي فيها نماذج شرائح بعدد ما يطابق البحث، `FFSL.cat` يبقى عند الرجوع من شرائح نموذج ويُعاد عند فتح النافذة)، ثم مصغّرات شرائح النموذج المختار بلغة العرض الحالي). الإضافة بعد الشريحة الظاهرة وتُحفظ **فورًا إصدارًا جديدًا**
  (ملاحظة الإصدار «أُضيفت…») لأن سكربتات القوالب تحفظ قائمة شرائحها عند التحميل فلا تتنقّل إلى شريحة أُدرجت بعده؛ بعد الحفظ تُعرض الشريحة الجديدة
  (`FF.navTarget` → `ffRender` يقدّمه على الاستعادة المعتادة) ويعود وضع التعديل (`FF.reEdit` عند أول `nav-state`)، ورسالة بزر «↶ تراجع عن إضافة الشريحة».
- **داخل الإطار** (`ffDesignKit`، أمر `slide-add`): `blank` = نسخة من الشريحة الظاهرة يُحذف من إطارها (`.frame` أو القسم) كل ابن ليس زخرفة (`DECOR`: deco/bg/lg/logo/
  brand/co/mir/foot/pn/num/shape…)، ويُضاف عنوان وفقرة بخط ولون أول عنوان في الشريحة الأصلية، مطلقين قابلين للسحب (`data-jeem-free`) حين الإطار ثابت
  (العنوان على 240px والفقرة 330px لتفادي الشعار)؛ `dup` = نسخة من الشريحة بالفهرس؛ `import` = HTML + CSS + خطوط مستوردة (`<style id="jeem-slide-css-…">` في
  الرأس، و`<script id="jeem-slide-fit">` يصغّر الإطار 1280×720 المستورد إن لم يكن للعرض إطار ثابت، بسمة `data-jeem-fit`). `cleanSlide` يزيل التفعيل وسمات المحرر
  والسكربتات وتحويل `fit()` من الإطار. يرد `{type:'slide-added', index}` ثم تطلب الأم `get` وتحفظ.
- **الاستيراد من نموذج آخر** (`ffslExtract` في الأم): صفحة النموذج تُحمَّل في إطار مخفي `sandbox="allow-scripts allow-same-origin"` (ملفات نماذجنا الثابتة) فتعمل
  سكربتاتها وتُرسم رسومها، ثم تُنسخ الشريحة المطلوبة. **العزل**: كل الأصناف (عدا `slide/active/frame`) والمعرّفات في الشريحة تُسبق ببادئة فريدة `x<tag>-` مع
  مراجعها (`url(#…)`، `href="#…"`، `for/aria-*`) — `ffslRename`؛ وكل قواعد صفحاتها من CSSOM (`ffslScopeRules`: style/media/supports، وfont-face/keyframes كما هي،
  و`@import` خطوطًا) تُعاد كتابة محدّداتها بالبادئة نفسها وتُقيَّد بصنف `jx<tag>` على الشريحة (`ffslScopeSel`: `:root` ← الصنف، `html[dir] …` ← `html[dir] .jx …`،
  قواعد `body` وحدها تُحذف). فلا تتصادم أنماط النموذجين في أي اتجاه. نموذج العرض نفسه (`templateKey` متطابق) يُدرج بلا عزل.
- **حذف الشريحة** (طلب المستخدم: «زي إضافة شريحة»): زر `#ffd-slide-del` (أيقونة حمراء) بجوار «إضافة شريحة» في الشريط العائم، يظهر بالشرط
  نفسه. `ffslRemove` يطلب تأكيدًا («حذف الشريحة 3 من 16؟») ثم أمر `slide-remove` في `ffDesignKit` يحذف الشريحة الظاهرة (يرفض حذف الشريحة
  الوحيدة) ويرد `slide-removed {index, n}`؛ `ffslRemoved` يحفظ فورًا إصدارًا جديدًا بملاحظة «حُذفت الشريحة n» (للسبب نفسه في الإضافة: سكربت
  القالب يحتفظ بقائمة شرائحه) وتُعرض الشريحة التي حلّت مكانها ويعود وضع التعديل، ورسالة «🗑 …» بزر «↶ تراجع عن حذف الشريحة». رفض التأكيد لا يغيّر
  شيئًا. تحقق Playwright (1400 و390): 16 → 15، العرض على 3/15، التراجع متاح، الإلغاء بلا حفظ.
- **تعريفات SVG المشتركة** (علّة أبلغ عنها المستخدم: «قمع مقلوب بثلاث مراحل» ظهر بلا تدرّجاته): بعض النماذج تضع `<defs>` (تدرّجات/أنماط/قصّ/رموز)
  في SVG خارج الشرائح وتشير إليها الشرائح وأنماط الصفحة بـ`url(#…)`/`href="#…"`. `ffslCarryDefs(doc, sec)` قبل إعادة التسمية يجمع هذه المراجع
  من الشريحة ومن CSS الصفحة (وما تشير إليه التعريفات نفسها)، وينسخ كل عنصر SVG غير موجود داخل الشريحة إلى `<svg data-jeem-defs>` مخفي أول الشريحة،
  فيأخذ البادئة نفسها مع مراجعه. تحقق Playwright (1400 و390): التدرّج `x…-fb` يُحَل والقمع مرسوم كما في النموذج.
- **اهتزاز الشرائح في وضع التعديل** (علّة أبلغ عنها المستخدم): حلقة تذبذب في `jeem-slides.js` — ارتفاع المصغّرة = نسبة الشريحة الظاهرة، والشريحة
  تُحجَّم بـ`100vh - --jeem-strip-h` في النماذج ذات الإطار الثابت، فيتأرجح ارتفاع الشريط بين قيمتين بفارق بكسل (58↔59) ويتغيّر مقياس الإطار باستمرار.
  الآن ارتفاع مصغّرة قائمة لا يتغيّر إلا بفارق >3px، و`announceHeight` يتجاهل فرق ≤1px. تحقق Playwright على 40 مقاسًا (محادثة ظاهرة/مخفية): صفر تذبذب.
- **المصغّرات** (`ffslThumbDoc`): الصفحة نفسها بشريحة واحدة — الشريحة المطلوبة تُنقل أولًا (سكربت القالب يفعّل الأولى عند التحميل) والبقية أغلفة فارغة فيها
  عناصرها ذات المعرّف و`.frame` فقط بلا صور (حتى لا تفشل سكربتات الرسم و`fit()`)، صامتة (`FF_THUMB_QUIET`)، تُحمَّل كسولًا ومتتابعة (3 معًا) ومقياسها من عرض
  البطاقة (`--ffsl-k`). تحميل 31 صفحة كاملة دفعة واحدة كان يُسقط المتصفح.
- تحقق Playwright (1400 و390) على `sl-ai-business-plan` بخادم وهمي: الزر في وضع التعديل، فارغة → 17 شريحة وعنوان «عنوان الشريحة» والعرض على 2/17 ووضع التعديل
  عاد، نسخة من الشريحة 3 → 18 والعرض على 3، من «دراسة حالة أعمال» (31 مصغّرة بعد البحث) الشريحة 2 → 19 بـ`jeem-slide-css-` وصنف `jx…` والشريحة مرسومة
  نشطة بصورتها بلا كسر وإطار مُصغَّر؛ صفر أخطاء JS وبلا تمرير أفقي. **قيود**: نموذج يرسم بسكربت عند التحميل يُنقل بلقطته المرسومة (لا يُعاد رسمه بعد الحفظ —
  أرقامه ثابتة حتى تُربط ببياناتك)؛ صفحة مولَّدة بلا إطار ثابت قد تحتاج ضبطًا يدويًا للشريحة المستوردة.

## تحويل لوحة متابعة إلى شرائح/تصميم برسوم حيّة (2026-11-25) ✅

- **الطلب**: الشرائح المبنية من لوحة متابعة («⇄ تحويل إلى» في «لوحات المتابعة») تعرض رسومها بالبيانات الحقيقية وتتحدّث، كأنها أُضيفت من
  «📊 رسمة من بياناتي ← من لوحة محفوظة ← كما في اللوحة». قبلها كان الموديل يُطلب منه تنفيذ SQL كل عنصر ورسمه بنفسه فتخرج رسومًا ثابتة.
- **الخادم** (`FreeformController.FromDashboard` + `DashboardBlocksAsync`): استعلام كل عنصر (`query.sql`، حتى 40 عنصرًا) يُنفَّذ **على الخادم بلا موديل**
  بـ`RunReadOnlyQueryAsync` (صلاحيات المستخدم ومصادره)، ويُسجَّل `q1, q2…` بنتيجته، ويُبنى له **وسم بلوك جاهز** بـ`Freeform/FreeformDashboardBlocks.Markup`
  (نقل خادمي لـ`ffdmAsIs`: kpi → `<span data-jeem-value="qN.<عمود>" data-fmt="number">` مع التسمية، table/progress-table → بلوك جدول، bar/line/area/pie/donut/
  funnel/treemap/scatter كما هي، radial/gauge → donut، stages → funnel، والباقي bar؛ المفاتيح `xKey/yKey/series` بأسمائها وتسمياتها والتكديس و`centerLabel`؛
  **بلا عنوان داخل البلوك** لأن الموديل يكتبه عنوانًا للبطاقة/الشريحة؛ JSON السمة يُهرَّب `&`/`'`). الرسالة للموديل تسرد لكل عنصر وسمه وتُلزمه بوضعه
  **حرفيًا** بلا SVG ولا أرقام ولا `query_data` لها. `FreeformBuilder.GenerateAsync(…, seedQueries)` (معامل جديد) يبدأ بالنتائج المسجَّلة فتدخل كتلة
  `JEEM_DATA` و`FreeformQueries` ويقبلها المدقّق. بعد التوليد `FreeformDashboardBlocks.EnsureBlocks` يضيف ما لم يضعه الموديل (`Present` يفحص `"q":"qN"`
  مرمّزًا أو لا و`data-jeem-value="qN.`): لصفحة شرائح شريحة `section.slide[data-jeem-added=chart]` لكل بلوك بعد آخر شريحة (`FreeformScope.FindSlides`)، ولغيرها
  قسم قبل آخر `</body>`، ويُذكر العدد في الرد. العنصر بلا استعلام أو الفاشل أو بلا صفوف يبقى للموديل كما كان.
- **النتيجة**: الرسوم تُرسم بمحرّك المنصة (`jeem-blocks.js` على `jeem-viz.js`) من صفوف حقيقية، وتتحدّث بـ«🔄 تحديث البيانات» بلا موديل (الاستعلامات مسجَّلة)،
  وفي الرابط المنشور الحيّ، وتظهر في لوحة الخصائص كأي بلوك (تغيير النوع/الأعمدة/الألوان، «تغيير مصدر البيانات»).
- تحقق: `FreeformDashboardBlocksTests` (الوسم لكل نوع، السلاسل المكدّسة بالتسميات، المؤشر، الجدول، الفاصلة العليا في العنوان، اجتياز `FreeformBlockValidator`،
  الإضافة بعد آخر شريحة أو قبل `</body>` وعدم التكرار) 281/281. Playwright بالواجهة الحقيقية على صفحة شرائح مولَّدة من `Markup` + `Inject` (الموديل يضع 3
  بلوكات وينسى الرابع): 4 شرائح، المؤشر «1,069» من البيانات، الأشرطة 4 والخط والحلقة مرسومة بلا `jb-empty`، الشريحة المضافة آليًا مرسومة، وتغيير
  `JEEM_DATA` + `jeem:data` يحدّث الرقم فورًا (1400 و390، بلا أخطاء وبلا تمرير أفقي). **التزام الموديل الحقيقي بوضع الوسوم حرفيًا لم يُجرَّب هنا** — الإضافة
  الآلية تضمن ظهور الرسوم وإن لم يلتزم.

## «⇄ تحويل إلى» يفتح معرض النوع المختار لاختيار نموذج (2026-11-25) ✅

- **الطلب** (بصورة لقائمة «تحويل إلى»): اختيار النوع يفتح صفحة الإضافة (المعرض) الخاصة به ليختار منها نموذجًا.
- **الواجهة** (app.js): بنود قائمتَي التحويل (`#ff-convert-panel` لتصميم، و`#ff-tk-convert-panel` للوحة متابعة) تنادي `ffStartConvert(from, kind)`: يفتح
  `openFfGallery(kind)` ثم يضع `FF.convert = {from, kind, title, sources}` ويغيّر العنوان («تحويل «X» إلى شرائح») والوصف وبطاقة «بلا نموذج» («تحويل بلا
  نموذج»)، ويخفي بطاقتي الرفع. كل نقاط الاختيار تمر من `ffConvertPick(t)` قبل سلوكها المعتاد: `ffUseTemplate` (النماذج البنيوية والجاهزة ومن العرض
  التوضيحي)، بطاقة «بلا نموذج»، و`galUseTemplate` في مضيف تصميم حر (نماذج كتالوج اللوحات `d:` ولوحات المتابعة `chat:`) — فيُغلق المعرض ويُنفَّذ
  `ffDoConvert(conv, t)`: تصميم → `ffConvert(kind, t)`، لوحة → `ffTrackConvert(kind, t)`، و**تصميم → لوحات المتابعة** = لوحة جديدة في «لوحات المتابعة» بمصادر
  التصميم والنموذج المختار وطلب جاهز. `closeFfGallery` يلغي `FF.convert` (الإغلاق بلا اختيار لا يحوّل). **علّة أُصلحت بالمناسبة**: بطاقتا الرفع لم تكونا
  تُخفيان (قاعدة display الخاصة بهما تغلب `.hidden`) فكانتا تظهران في معرض «لوحات المتابعة» أيضًا — صارتا تُخفيان بـ`style.display`.
- **الخادم**: `FreeformConvertRequest.TemplateKey` و`FreeformFromDashboardRequest.TemplateKey` → `ConvertTemplateAsync` (يحلّ المفتاح بـ`ResolveTemplateAsync` ويرفض
  نموذجًا من نوع آخر). **نموذج بنيوي** (بلا `Page`): يُمرَّر `template` إلى `GenerateAsync` + «ابنِ النتيجة على نموذج X». **نموذج صفحة جاهزة**: صفحة النموذج (بلغة
  المصدر) تصير `currentHtml` والمصدر مرجعًا في الرسالة (`TemplatePageConvertMessage`: انقل المحتوى وبلوكات `data-jeem-*` كما هي محافظًا على تصميم النموذج، أضف/احذف
  شرائح من النوع نفسه، ≤90K حرف من المصدر)، وكتلة `JEEM_DATA` للمصدر تُمرَّر `seedQueries` فتبقى بلوكاته مربوطة؛ من لوحة متابعة: الرسالة نفسها مع الرسوم الحيّة
  الجاهزة. التصميم الجديد يحفظ `TemplateKey` (شريحة النموذج تظهر عند فتحه)، و`convert` صار يسجّل استعلامات الناتج في `FreeformQueries`.
- تحقق: `FreeformConvertTemplateTests` 282/282. Playwright (1400 و390) بخادم وهمي: قائمة التحويل من مستند، اختيار «شرائح» يفتح المعرض بعنوان التحويل ونماذج الشرائح فقط
  و«تحويل بلا نموذج» وبلا بطاقتي الرفع، نموذج جاهز → `convert {kind:slides, templateKey:sl-ai-business-plan}` ويفتح الناتج، «بلا نموذج» → `templateKey:null`، الإغلاق
  يلغي الوضع، ومن لوحة متابعة إلى مستند بنموذج بنيوي → `from-dashboard {kind:doc, templateKey:doc-report}`؛ بلا أخطاء وبلا تمرير أفقي. **التحويل الحي بالموديل على
  نموذج لم يُجرَّب هنا.**

## أيقونات الشريط العلوي لـ«المحادثة واللوحات» فقط (2026-11-25)

- بطلب المستخدم: مجموعة `.topbar-nav-group` (السجل، اللوحات النشطة، روابط المشاركة) تظهر فقط في شاشة `chat` وشاشاتها الثلاث نفسها
  (`history`/`active`/`sharelinks`)، وتُخفى في غيرها من `showScreen` (`style.display` لأن قواعد الشريط تغلب `.hidden`). الشاشات نفسها ما زالت
  متاحة بروابطها. تحقق Playwright (1400 و390): ظاهرة في المحادثة/السجل/الروابط، مخفية في تصميم حر/منشئ العروض/الملفات، بلا أخطاء.

## قالب ثقة المؤسسي «نماذج ثقة» من PowerPoint (2026-11-25) ✅

- **الطلب**: إضافة ملف PowerPoint ثقة (العربي ثم الإنجليزي) قالبًا تحت تصنيف «ثقة». المجلد `wwwroot/templates/freeform/sl-thiqah-corporate/`
  (**المفتاح لا يُعاد تسميته**): `ar.html` (73 شريحة، 750KB) و`en.html` (88 شريحة — الملف الإنجليزي أطول، كلٌّ كما سلّمه المستخدم؛ المحمِّل يطابق
  الأقسام مع العربية فقط) و`assets/` (4,341 PNG مشتركة باسم بصمة المحتوى، ~12.6MB بعد ضغط لوحة 256 لونًا بلا تنعيم؛ كان 34MB) و`template.json`
  (`category: "thiqah"`، 73 قسمًا «الشريحة n»، موجز يلزم الموديل بتغيير النصوص والأرقام فقط).
- **التوليد** (مرة واحدة، أداة خارج المستودع في scratchpad `thq/gen`): `PptxPageBuilder.Build(pptx, title, mediaUrl, unsupportedUrl, lang)` — الصور ملفات في
  `assets/` بمسار مطلق لا data URL، و**EMF** (16 صورة) حُوّلت PNG بـLibreOffice→PDF→`pdftoppm` مع قصّ إطار EMF من رأسه وجعل الأبيض شفافًا، وتُمرَّر
  بـ`PptxSvgRenderer.UnsupportedUrl` (خاصية جديدة اختيارية: بديل للصورة غير المدعومة بدل الإطار الأحمر). LibreOffice للتأليف فقط لا في المنصة.
- **`PptxPageBuilder`**: معامل `lang` (en = `lang="en" dir="ltr"` وأزرار وتنقّل لوحة مفاتيح/لمس معكوسة)، وتحميل خط البديل **IBM Plex Sans Arabic** من
  Google Fonts (المحرّك يسمّيه بديلًا لكل نص؛ بدونه سقط إلى Arial فقُصّت النصوص) — يفيد «رفع ملف كما هو» أيضًا.
- **رسم الرسوم البيانية في `PptxSvgRenderer`** (كانت مكانًا محجوزًا «رسم بياني»): `ChartSvg` من القيم المخزّنة في `ChartPart` بألوان السلاسل
  ونقاطها (`dPt`) والثيم: حلقة/دائري (`holeSize`)، أعمدة وأشرطة (`barDir`، clustered/stacked/percentStacked)، خط، مساحة (مكدّسة أو لا)، خطوط شبكة وتسميات الفئات.
  أنواع أخرى تبقى مكانًا محجوزًا. القيد: لا تسميات بيانات ولا محاور قيم، وفئات التاريخ تظهر أرقامًا تسلسلية.
- **الواجهة**: `FF_EXTRA_CATS = [['thiqah','نماذج ثقة','briefcase']]` أول التصنيفات في معرض تصميم حر وفي «إضافة شريحة» (لا يمس `DK_CATS` لمنشئ العروض)؛
  `FreeformFolderTemplates.Categories` تقبل `thiqah`. أعداد الشرائح في `home.html` صارت 64.
- **أماكن الكتابة**: الأماكن المحجوزة الفارغة في شرائح PowerPoint لا تُرسم افتراضيًا (كما يعرضها PowerPoint)، فضاع العنوان والفرعي
  من القالب. `PptxSvgRenderer.SlidePromptLang` (اختياري، `PptxPageBuilder.Build(..., slidePrompts: true)` من أداة التوليد فقط) يرسمها
  بنص مطالبة التخطيط («عنوان العرض، تنسيق 72pt» / «Headline 72pt»، وإلا نص افتراضي بحسب النوع واللغة) نصًّا كاملًا لا باهتًا فيُعدَّل من
  «✏️ تعديل»؛ التاريخ/التذييل/رقم الشريحة/الصور/الرسوم مستثناة. «رفع ملف كما هو» بلا تغيير. الصفحتان أُعيد توليدهما (335 نصًّا بدل 261).
- **الرسوم البيانية قابلة للربط ببيانات المستخدم** (طلب المستخدم: شرائح 7 و12–14 و16–19 فيها 25 رسمًا أصليًا، و20 أرقام كبيرة):
  `RenderFrame` يعلّم كل رسم رسمه `ChartSvg` بـ`<g class="pptx-chart" data-jeem-chartable="bar|line|pie|donut" data-w data-h>` (النوع من نوع رسم
  PowerPoint) ومستطيل نقر شفاف `rect.pptx-chart-hit` (وإلا وقع النقر في الفراغ بين الأعمدة على صورة الخلفية). في `ffDesignKit`: `chartHost` يقبل
  أقرب `[data-jeem-chartable]` أولًا، و`info().bind.type` يقترح طريقة العرض («حلقة» لـpie/donut)، و`bind` يضع البلوك في `<foreignObject
  data-jeem-chart-fo>` شقيق للرسم بتحويله ومقاسه (فيتدرّج مع الشريحة كالأصل) ويخفي الأصل بـ`data-jeem-replaced`؛ التراجع يزيله، ولوحة الخصائص
  بلا قسم «التخطيط» له (`inSvg`). الأرقام داخل فقرة بـspan واحد (شريحة 20) صارت تُربط بـ`numTarget` (ينزل إلى الابن الوحيد). يسري أيضًا على
  «رفع ملف كما هو» لأي PowerPoint. تحقق Playwright على القالب الحقيقي (1400 و390): حلقة الشريحة 7 وخط الشريحة 13 → بلوك مرسوم في مكانه بألوان
  القالب، «70.000» في الشريحة 20 → «9.9M»، التراجع، الحفظ وإعادة الرسم بعده.
- **القيود**: خط ثقة Tanseek غير متاح فتُقص كلمات قليلة في صناديق ضيقة (مثل «Tittle» وهي خطأ إملائي في الملف الأصلي)؛ يُحل برفع ملفات الخط.
- تحقق: `ThiqahTemplateTests` (التصنيف، 73/88 شريحة، الاتجاه، لا data URL ولا «غير مدعومة» ولا «رسم بياني»، كل صورة موجودة، رسم الحلقات والأعمدة) 284/284؛
  Playwright: الصفحتان بلا أخطاء JS ولا صور مفقودة ولقطات مطابقة بصريًا للأصل (أغلفة، حلقات 50/70/30/90، أعمدة/أشرطة/خطوط/مساحات، مخططات)؛ المعرض على 1400 و390:
  «نماذج ثقة» أول تصنيف، البطاقة والمعاينة بالغلاف، بلا تمرير أفقي. **«استخدم» على خادم حقيقي لم يُجرَّب هنا.**
