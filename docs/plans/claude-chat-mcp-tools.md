# خطة: أدوات قراءة لمحادثة Claude عبر MCP (المرحلة الأولى)

الحالة: **منفَّذة بالكامل ومتحقَّق منها حيًا (2026-11-03)**: 1–7 من الجلسة السحابية، و8 على جهاز المستخدم: سؤال «كم فرصة في كل مرحلة؟» → `list_tables` ثم `describe_table` ثم `run_sql` ×2 وأرقام مطابقة للقاعدة (1069 صفًا، 609 بلا مرحلة) في 58 ثانية بتكلفة 0.89$؛ `run_sql` على `dbo.Organizations` رُفض من الخادم برسالة «ليس من جداول مشروعك»؛ MCP بلا مفتاح → 401 حتى مع كوكي الدخول؛ لا ملفات mcp-config متبقية؛ صفر أخطاء في السجل. الفرع: `claude/chat-to-dashboard-app-at0up6`.
الفرع: `claude/chat-to-dashboard-app-at0up6`. التحقق الحي يحتاج SQL Server والـ CLI مسجَّل دخوله (غير متاحين في السحابة).

## الهدف

شاشة «محادثة Claude» (`ClaudeChat/`) تشغّل Claude Code CLI على الخادم بـ `--tools ""` فهي محادثة صِرف. المطلوب أن يقدر
Claude يرى جداول **مشروع المستخدم الحالي فقط** ويستعلم عليها **قراءةً فقط**، بمنطق مستقل بالكامل.

## قيود ملزمة

- **لا تستخدم** `AnalyticsTools` ولا `WidgetQueryService` ولا البنّاء المرحلي ولا أي أداة موجودة؛ المنطق الجديد كله تحت
  `backend/ChatToDashboard.Api/ClaudeChat/Mcp/` (قرار المستخدم: الأدوات القديمة قد تتغير).
- المزوّد الوحيد SQL Server، المخطط المشترك `dbo`، وجداول كل مشروع في `proj_<ProjectId>` (انظر CLAUDE.md).
- كل نص وتعليقات الكود بالعربية الفصحى. لا حذف أو إعادة تسمية لشيء موجود دون سؤال المستخدم.
- التنفيذ مرحلة بمرحلة: نفّذ الخطوات 1–4 ثم توقف ولخّص قبل الواجهة (5) وما بعدها.

## الخطوات

1. **خادم MCP داخل المنصة** على `/api/claude-chat/mcp` باستخدام الحزمة الرسمية `ModelContextProtocol.AspNetCore`
   (النقل فقط: JSON-RPC + Streamable HTTP). منطق الأدوات في `ClaudeChat/Mcp/`.
2. **مفتاح جلسة مؤقت** `ClaudeMcpSessionTokens` (في الذاكرة، لا في القاعدة): مع كل رسالة يولَّد مفتاح عشوائي مربوط بـ
   (UserId, ProjectId) بعمر 10 دقائق، يُمرَّر للـ CLI في ترويسة `Authorization: Bearer`، ويُلغى عند انتهاء الرسالة.
   الخادم يرفض أي نداء بلا مفتاح صالح (401).
3. **ثلاث أدوات قراءة فقط** (أسماؤها كما هي):
   - `list_tables`: جداول المشروع من `dbo.repo_Files` حيث `ProjectId` = مشروع المستخدم (الاسم الظاهر `TableName`،
     اسم الملف، عدد الصفوف) + جداول الأنظمة المفعَّلة `dbo.sys_*` (من `SourceOptions.ActiveSystems`).
   - `describe_table(table)`: الأعمدة وأنواعها من `INFORMATION_SCHEMA.COLUMNS`، ومعنى كل عمود من `dbo.ColumnSemantics`
     (ProjectId, TableName, ColumnName → Concept) إن وُجد، وعينة 5 صفوف. يرفض أي جدول ليس في نتيجة `list_tables`.
   - `run_sql(sql)`: مدقّق SELECT جديد: جملة واحدة تبدأ بـ `SELECT` أو `WITH`؛ كلمات ممنوعة
     (INSERT/UPDATE/DELETE/MERGE/DROP/ALTER/CREATE/TRUNCATE/EXEC/EXECUTE/GRANT/xp_/sp_/OPENROWSET/OPENQUERY/BULK/
     INTO/WAITFOR/SHUTDOWN/RECONFIGURE)؛ كل جدول مذكور لازم يكون ضمن `list_tables` لهذا المستخدم وإلا رفض برسالة
     واضحة؛ فرض `TOP 500` إن غاب؛ مهلة 30 ثانية؛ النتيجة JSON مضغوط `{columns:[], rows:[[]], truncated}`.
4. **تشغيل الـ CLI** في `ClaudeCliService`: ملف `mcp-config` مؤقت لكل رسالة (عنوان الخادم + المفتاح)، وتشغيله بـ
   `--mcp-config <file> --strict-mcp-config --allowedTools "mcp__jeem__*"` بدل `--tools ""`، مع
   `--append-system-prompt` قصير بالعربية يعرّفه بالأدوات الثلاث ويطلب الاستكشاف قبل الإجابة. حذف الملف المؤقت بعد
   الرسالة. اسم الخادم في الإعداد `jeem`.
5. **الواجهة**: حدث SSE جديد `tool` (اسم الأداة وملخص المدخل) يظهر في شريط المحادثة كسطر «🔎 يستعلم عن…» أثناء العمل
   (stream-json يتضمن كتل `tool_use`).
6. **إعدادات** في `ClaudeCli`: `ToolsEnabled` (true)، `MaxQueriesPerMessage` (15)، `QueryTimeoutSeconds` (30)،
   `MaxRows` (500). تجاوز السقف يرجّع للموديل «توقف وأجب بما لديك».
7. **اختبارات وحدة** (`backend/ChatToDashboard.Api.Tests/`): المدقّق (رفض UPDATE/DELETE/جمل متعددة/جدول خارج
   المشروع، قبول WITH، فرض TOP)، وتوليد المفتاح وانتهاؤه ورفض المفتاح الخاطئ.
8. **تحقق حي** (على جهاز المستخدم، ليس في السحابة: يحتاج SQL Server Express والـ CLI مسجَّل دخوله): سؤال «كم فرصة في
   كل مرحلة؟» على ملف Opportunities → في السجل `list_tables` ثم `run_sql` ورقم صحيح؛ ثم استعلام على جدول مشروع
   آخر → مرفوض.

## ما ليس في هذه المرحلة

بناء لوحات من المحادثة (`build_dashboard`)، اختيار منظمة لمالك المنصّة، الكتابة في القاعدة.
