# جيم (Chat to Dashboard) — دليل العمل على §4.16-4.24

هذا الملف يُقرأ تلقائيًا في أول كل جلسة Claude Code على هذا الـ repo. الهدف
منه إنك متبدأش من الصفر: السبيك والمرجع البصري والقرارات المعمارية كلها
موثّقة هنا أو مُشار لمكانها.

## السياق العام

- بنية التطبيق الأساسية (ASP.NET Core + SQL Server/SQLite، كيف البيانات
  بتتحمّل، إزاي المحادثة بتولّد لوحات) موثّقة في [README.md](README.md) —
  اقرأه لو محتاج خلفية عن الـ `staging` schema أو مسار `/api/chat`.
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
  أدوات `publish_presentation(title, html, expiresInDays?)` → multipart إلى `/api/presentations` ويعيد `/p/{id}`،
  `replace_presentation`، `list_presentations`، `revoke_presentation`، `whoami`. البيئة `JEEM_URL` + `JEEM_TOKEN`.
  القيد: المنصة داخلية فلا Custom Connector من claude.ai؛ الجسر يعمل من **Claude Desktop فقط** على جهاز داخل الشبكة
  (التركيب في `tools/jeem-mcp/README.md`). لو أُتيح نطاق عام لاحقًا تتحول الأدوات نفسها إلى خادم MCP بعيد.
- تحقق حي: إنشاء مفتاح → whoami بالـBearer 200 بلا كوكي، إنشاء مفتاح بمفتاح 403، مفتاح مُلغى/خاطئ 401؛ عميل MCP
  stdio حقيقي ضد الجسر: نشر → رابط عام 200، استبدال يغيّر المحتوى على نفس الرابط، قائمة، إلغاء → 410، HTML أقصر
  من الحد يُرفض من المخطط؛ النافذة بلا أخطاء JS وبلا تمرير أفقي على 390px.

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
- تحقّق حيّ عبر Playwright: شغّل السيرفر محليًا بـ
  `ASPNETCORE_URLS=http://127.0.0.1:5050 DatabaseProvider=Sqlite dotnet bin/Debug/net8.0/ChatToDashboard.Api.dll`،
  سجّل دخول بمنظمة "شركة النموذج التجريبي" ومستخدم `project.owner`
  (كلمة المرور تُطبع في الـ log عند أول تشغيل — دوّر على
  "Seeded a project-owner demo account" في الإخراج، أو جرّب كلمة مرور
  معروفة لو قاعدة البيانات محليًا فعلًا اتولدت قبل كده)، افتح معرض
  النماذج → التصنيف المناسب → زر المعاينة التجريبية، وتأكد من الشكل
  وعدم وجود أخطاء جافاسكربت.
