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
  (نفس حلقة الأدوات القديمة بسقف وإجبار قابلين للضبط؛ OpenAI بوضع `json_object`).
  Ollama لا يدعمها → `LlmRouter.ResolveStagedAsync` يرجع null → المسار القديم.
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
