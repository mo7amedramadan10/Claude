const el = id => document.getElementById(id);
// Mirrors the <html lang> the head script already set (before this script even loads) —
// read once here rather than re-touched, since switching it is a page reload (see
// lang-toggle's click handler), never an in-place state change.
// ---------- English translation layer ----------
// A dictionary-driven, whole-DOM post-processing pass rather than hundreds of individual
// call-site rewrites — the app is ~6000 lines of Arabic-native JS/HTML with no single
// render entry point all of it flows through. Every render function keeps emitting Arabic
// exactly as it always has; this only swaps what's already in the DOM, text node by text
// node and a few key attributes, whenever the page is in English mode. Matched by
// substring within each text node (not requiring an exact whole-node match), so labels
// built with interpolation ("٣ عناصر", "تصدير PDF") still get their static Arabic parts
// translated even though the whole string isn't itself a dictionary key. I18N_ENTRIES is
// sorted longest-first so a more specific phrase is tried before any shorter phrase it
// happens to contain (e.g. "مصدر البيانات" before a bare "البيانات" would ever match).
const I18N_EN = {
  // ---- brand / login / shared banner ----
  'المحادثة إلى لوحة معلومات': 'Chat to Dashboard',
  'اسم المستخدم': 'Username', 'كلمة المرور': 'Password', 'تسجيل الدخول': 'Sign In',
  '📤 عرض مشاركة —': '📤 Shared view —', 'افتح التطبيق الكامل ←': 'Open the full app ←',
  // ---- top nav ----
  'المحادثة': 'Chat', 'مستودع الملفات': 'File Repository', 'السجل': 'History',
  'اللوحات النشطة': 'Active Dashboards', 'روابط المشاركة': 'Share Links',
  'المستخدمون': 'Users', 'الإعدادات': 'Settings',
  'ما أُرسل للموديل وكم استهلك': 'What was sent to the model and how much it used',
  'الاستهلاك ↗': 'Usage ↗',
  'كل المصادر مفعّلة': 'All sources enabled',
  'الأنظمة — تحديد الكل': 'Systems — select all',
  'مستودع الملفات — تحديد الكل': 'File repository — select all',
  'تصغير لوحة المحادثة': 'Shrink chat panel', 'تكبير لوحة المحادثة': 'Grow chat panel',
  'إخفاء لوحة المحادثة': 'Hide chat panel',
  'تشغيل/إيقاف القراءة الصوتية التلقائية لردود المساعد': "Toggle automatic voice read-aloud for the assistant's replies",
  '🔇 قراءة تلقائية': '🔇 Auto read', 'دليل استخدام النظام': 'System usage guide',
  'تبديل المظهر': 'Toggle appearance', 'خروج': 'Log out',
  // ---- chat panel ----
  'بناء اللوحة': 'Build Dashboard', 'الاستفسارات': 'Inquiries',
  '📁 لا يوجد ملفات في المستودع': '📁 No files in the repository', 'إدارة الملفات ←': 'Manage files ←',
  'الصورة المرفقة': 'Attached image', 'إزالة الصورة': 'Remove image',
  'إرفاق صورة داشبورد لإعادة بنائه ببياناتك': 'Attach a dashboard screenshot to rebuild it with your data',
  'يفرّغ اللوحة المعروضة الآن فورًا؛ سؤالك الجاي هيبني لوحة جديدة تمامًا من الصفر':
    'Instantly clears the dashboard on screen — your next question builds a brand-new one from scratch',
  '🆕 ابدأ لوحة جديدة': '🆕 Start new dashboard',
  'اسأل عن بياناتك، أو أرفق صورة داشبورد لإعادة بنائه…': 'Ask about your data, or attach a dashboard screenshot to rebuild it…',
  'اسأل بصوتك': 'Ask by voice', 'إرسال': 'Send',
  // ---- dashboard toolbar ----
  'عرض': 'View', 'تحرير': 'Edit',
  'يعيد تنفيذ استعلام كل عنصر من البيانات الحية، مع الحفاظ على أي فلتر مطبّق حاليًا':
    "Re-runs every widget's query against live data, keeping any filter currently applied",
  '🔄 تحديث': '🔄 Refresh', 'تراجع': 'Undo', 'إعادة': 'Redo',
  'حوّل هذه اللوحة إلى لوحة نشطة — تصبح قابلة للمشاركة وللمنح صلاحيات تعديل/عرض لأشخاص آخرين':
    'Turn this dashboard into an Active one — shareable, with edit/view access grantable to others',
  '✅ تفعيل اللوحة': '✅ Activate dashboard', '🔗 مشاركة': '🔗 Share',
  '🖨️ تصدير PDF': '🖨️ Export PDF', '📊 تصدير PowerPoint': '📊 Export PowerPoint',
  // ---- repository ----
  'رفع ملفات جديدة': 'Upload new files', 'الأكثر استخدامًا': 'Most used',
  'الأحدث تحديثًا': 'Recently updated', 'الاسم': 'Name',
  'الملف': 'File', 'آخر تحديث': 'Last updated', 'الاستخدام': 'Usage', 'الصلاحيات': 'Permissions',
  'اختر ملفًا واحدًا أو أكثر (Excel أو CSV أو PDF)، اكتب بيانات كل ملف، ثم اضغط\n      "حفظ في المستودع" لكل ملف على حدة لإضافته للمستودع.':
    'Choose one or more files (Excel, CSV, or PDF), fill in each file\'s details, then click\n      "Save to repository" for each one to add it.',
  'اختر ملفات': 'Choose files', 'إغلاق': 'Close', 'تفاصيل الملف': 'File Details',
  'اسم العرض': 'Display name', 'الوصف': 'Description', 'التصنيف': 'Category',
  '🗑️ حذف الملف': '🗑️ Delete file', 'إلغاء': 'Cancel', 'حفظ': 'Save',
  'صلاحيات الملف': 'File Permissions',
  'هذا الملف مقيّد تلقائيًا لمنشئه ولا يمكن إزالة هذا القيد — يمكنك فقط\n      منح صلاحية استخدامه لمستخدمين آخرين بالإضافة إليه.':
    "This file is automatically restricted to its creator, and that can't be removed — you can only\n      additionally grant other users access to it.",
  'المالك (منشئ الملف):': 'Owner (file creator):',
  'مستخدمون لديهم صلاحية إضافية': 'Users with additional access',
  'إضافة مستخدم': 'Add user', 'ابحث باسم المستخدم أو الاسم…': 'Search by username or name…',
  'الملفات المرتبطة': 'Related Files',
  'اربط هذا الملف بملف آخر يشترك معه في عمود، ليظهر الربط كإشارة عند تحليل البيانات.':
    'Link this file to another that shares a column with it, so the link shows as a hint during data analysis.',
  'إضافة ربط': 'Add link', 'اسم العمود المشترك': 'Shared column name', 'ربط': 'Link',
  // ---- history / active / share links ----
  'مسح الكل': 'Clear all', 'اللوحة': 'Dashboard', 'آخر حفظ': 'Last saved', 'العناصر': 'Widgets',
  'الدور': 'Role', 'ⓘ ما معنى المالك والصلاحيات؟': 'ⓘ What do owner and permissions mean?',
  'الأدوار على اللوحة النشطة': 'Roles on an Active Dashboard',
  'تُسنَد لكل لوحة نشطة صلاحياتٌ لشخص واحد أو أكثر، وفق واحدٍ من ثلاثة أدوار، يحدّد كلٌّ منها من يملك حق التعديل عليها ومن يقتصر دوره على الاطّلاع فحسب.':
    'Every Active dashboard grants access to one or more people under one of three roles, each determining who can edit it and who can only view it.',
  'مالك': 'Owner',
  'هو الشخص الذي أنشأ اللوحة. تُستخرج بيانات اللوحة دائمًا وفق صلاحية وصول <b>المالك</b> إلى الأنظمة والمصادر المعتمَدة عليها؛ فإذا فقد المالك صلاحيته على أحد مصادر البيانات، تتعطّل اللوحة تلقائيًا لجميع من يطّلعون عليها، دون استثناء.':
    "The person who created the dashboard. Its data is always pulled under the <b>Owner's</b> own access to the systems/sources it depends on — if the Owner loses access to one of those sources, the dashboard breaks automatically for everyone viewing it, with no exception.",
  'ينفرد المالك بالصلاحيات التالية:': 'Only the Owner has the following abilities:',
  'حذف اللوحة نهائيًا.': 'Permanently delete the dashboard.',
  'إضافة المحرِّرين والمشاهدين أو إزالتهم، وتحديد دور كل منهم.': "Add or remove Editors and Viewers, and set each one's role.",
  'نقل الملكية إلى شخص آخر، بشرط أن يمتلك هذا الشخص صلاحية وصول سارية على البيانات ذاتها.':
    'Transfer ownership to someone else, provided they already have valid access to the same data.',
  'تغيير مصادر البيانات المعتمَدة في اللوحة — إضافة مصدر جديد إليها أو استبدال أحد مصادرها الحالية.':
    "Change the dashboard's data sources — add a new one or replace an existing one.",
  'وتشمل صلاحياته، بطبيعة الحال، جميع صلاحيات المحرِّر الموضّحة أدناه.': "Naturally, this also includes every Editor ability described below.",
  'محرر': 'Editor', 'صلاحية التعديل': 'Edit access',
  'يمكنه فتح اللوحة وتعديل محتواها؛ كإضافة عناصر أو حذفها، أو تغيير نوع الرسم البياني، أو استخدام عوامل التصفية، ضمن نطاق مصادر البيانات المعتمَدة أصلًا في اللوحة، وتُحفَظ أي تعديلات تلقائيًا. لا يملك المحرِّر صلاحية حذف اللوحة، ولا إدارة من له حق الوصول إليها، ولا نقل ملكيتها.':
    'Can open the dashboard and edit its content — adding or removing widgets, changing a chart type, or using filters — within the data sources already set on the dashboard; edits save automatically. An Editor cannot delete the dashboard, manage who has access to it, or transfer its ownership.',
  'مشاهد': 'Viewer', 'صلاحية العرض': 'View access',
  'يقتصر دوره على فتح اللوحة والاطّلاع على بياناتها ورسومها البيانية، دون أي إمكانية للتعديل — لكنه يظل يرى بيانات حيّة لا مجمَّدة: يمكنه الضغط على زر «🔄 تحديث» في أي وقت لإعادة تنفيذ استعلامات اللوحة وجلب أحدث البيانات المتاحة، تمامًا كما يفعل المالك أو المحرِّر.':
    'Can only open the dashboard and view its data and charts, with no ability to edit — but the data they see is still live, not frozen: they can click "🔄 Refresh" at any time to re-run the dashboard\'s queries and pull the latest available data, exactly like the Owner or an Editor can.',
  'ما الفرق بين المشاهد على اللوحة النشطة ورابط المشاركة؟': "What's the difference between a Viewer on an Active Dashboard and a share link?",
  'المشاهد على لوحة نشطة يرى نفس اللوحة الحيّة التي يراها المالك والمحرِّرون، ويمكنه الضغط على «🔄 تحديث» في أي وقت للحصول على أحدث البيانات المتوفرة فعليًا.':
    'A Viewer on an Active dashboard sees the exact same live dashboard the Owner and Editors do, and can click "🔄 Refresh" at any time to get the latest data actually available.',
  'أما <b>رابط المشاركة</b> فهو لقطة ثابتة (Snapshot) من اللوحة في لحظة إنشاء الرابط فقط — لا يحتوي على زر تحديث، ولا يعكس أي تغيير يطرأ على البيانات لاحقًا، حتى لو تغيّرت الأرقام الفعلية في الأنظمة المصدر بعد ذلك. فرابط المشاركة يناسب مشاركة نتيجة لحظة زمنية محددة (كتقرير شهري ثابت)، بينما صلاحية المشاهد على اللوحة النشطة تناسب من يحتاج متابعة البيانات باستمرار وهي محدَّثة.':
    'A <b>share link</b>, on the other hand, is a frozen snapshot of the dashboard at the moment the link was created — it has no refresh button and never reflects any later change to the data, even if the real numbers in the source systems change afterward. A share link suits sharing a fixed point-in-time result (like a static monthly report), while Viewer access on an Active dashboard suits someone who needs to keep following the data as it updates.',
  'من يملك صلاحية تغيير مصدر البيانات؟': "Who can change the dashboard's data source?",
  'المالك وحده يملك هذه الصلاحية؛ أما المحرِّر فيمكنه التعديل على العرض والتحليل ضمن المصادر القائمة فقط، دون أن يتمكّن من إقحام مصدر بيانات جديد على اللوحة.':
    'Only the Owner has this ability; an Editor can only adjust the view/analysis within the existing sources, without being able to introduce a new data source into the dashboard.',
  'السبب في ذلك مرتبط مباشرةً بالمبدأ الذي تقوم عليه اللوحة النشطة: بما أن تنفيذ الاستعلام يتم دائمًا وفق صلاحية المالك، فإن أي إضافة لمصدر بيانات جديد تستلزم بالضرورة أن يمتلك المالك نفسه صلاحية وصول سارية عليه. لو سُمح للمحرِّر بإضافة مصدر لا يملك المالك صلاحية عليه، لتعطّلت اللوحة فورًا لجميع المستخدمين — وهو ما يتعارض مع الغرض من التعديل أصلًا. لذلك، فإن أي تغييرٍ يمسّ نطاق مصادر البيانات يبقى قرارًا يخصّ المالك حصرًا، تمامًا كصلاحيات إدارة الوصول ونقل الملكية.':
    "This ties directly to the principle an Active dashboard runs on: since queries always execute under the Owner's own access, adding any new data source necessarily requires the Owner to already have valid access to it. If an Editor could add a source the Owner has no access to, the dashboard would break instantly for every user — defeating the whole point of the edit. So any change touching the dashboard's data-source scope stays the Owner's decision alone, just like managing access and transferring ownership.",
  'الرابط': 'Link', 'الإنشاء / الانتهاء': 'Created / Expires', 'المشاهدات': 'Views', 'الحالة': 'Status',
  '+ مستخدم جديد': '+ New user', 'طريقة الدخول': 'Sign-in method',
  // ---- settings ----
  'إعداد على مستوى النظام بالكامل': 'A System-Wide Setting',
  'الموديل المختار هنا هو نفسه اللي بيجاوب على أسئلة كل المستخدمين في النظام — مش إعداد شخصي لكل مستخدم على حدة، وأي تغيير يسري فورًا على أول سؤال جديد بعد الحفظ.':
    "The model chosen here is the same one that answers every user's questions across the system — not a per-user personal setting — and any change takes effect immediately on the next new question after saving.",
  'الموديل المستخدم حاليًا': 'Currently Active Model', 'جارٍ التحميل…': 'Loading…',
  'تغيير الموديل': 'Change Model', 'الموديل الداخلي (Ollama)': 'Internal Model (Ollama)',
  'موديل OpenAI': 'OpenAI Model', '✓ تم الحفظ': '✓ Saved', 'حفظ التغييرات': 'Save Changes',
  'نموذج قراءة المستندات (PDF)': 'Document Reader Model (PDF)',
  'يُستخدم وقت رفع أي ملف PDF جديد ليقرأ صفحاته كصور — بما فيها\n        صفحة ممسوحة ضوئيًا أو جدول/رسم بياني مضمّن — بدل الاكتفاء باستخراج النص الرقمي فقط.\n        إعداد مستقل تمامًا عن "الموديل المستخدم حاليًا" أعلاه: ممكن تختار موديل مختلف لكل غرض.\n        لو فشلت القراءة بالذكاء الاصطناعي لأي سبب، يرجع النظام تلقائيًا للاستخراج النصي العادي\n        بدل ما يفشل الرفع بالكامل.':
    'Used when a new PDF is uploaded, to read its pages as images — including\n        a scanned page or an embedded table/chart — instead of only extracting the digital text layer.\n        A setting fully independent from "Currently Active Model" above: you can choose a different model for each purpose.\n        If AI reading fails for any reason, the system automatically falls back to plain text extraction\n        instead of failing the whole upload.',
  'نموذج تحليل الصور': 'Image Analysis Model',
  'يُستخدم لما ترفق صورة داشبورد أو مخطط لإعادة بنائه ببياناتك —\n        إعداد مستقل تمامًا عن "الموديل المستخدم حاليًا" أعلاه: ممكن تختار موديل مختلف لتحليل\n        الصور. لو تركته «نفس الموديل الحالي»، ستُرسل الصورة لنفس الموديل المختار فوق كالمعتاد؛\n        بعض الموديلات الداخلية لا تدعم تحليل الصور فترجع خطأً واضحاً بدل تجاهل الصورة بصمت.':
    'Used when you attach a dashboard screenshot or mockup to rebuild it with your data —\n        a setting fully independent from "Currently Active Model" above: you can choose a different model for image\n        analysis. Leave it on "Same as current model" and the image is sent to the same model chosen above as usual;\n        some internal models do not support image analysis and will return a clear error instead of silently ignoring the image.',
  'نفس الموديل الحالي': 'Same as current model',
  '⚠️ بعض الموديلات الداخلية لا تدعم تحليل الصور': '⚠️ Some internal models do not support image analysis',
  'معلومات عن الموديلات المتاحة': 'About the Available Models',
  'مناسب للتحليل الدقيق والأسئلة المركّبة اللي تحتاج فهم وربط بين أكثر من مصدر بيانات في نفس الوقت.':
    'Well-suited to precise analysis and complex questions that need understanding and connecting more than one data source at once.',
  'بديل سريع وقوي، مع إمكانية اختيار موديل محدد من قائمة الموديلات المتاحة في الإعدادات.':
    'A fast, capable alternative, with the option to pick a specific model from the list available in settings.',
  'يعمل على خوادم المؤسسة نفسها دون إرسال أي بيانات لجهة خارجية — الخيار الأنسب للبيانات الحساسة، مع إمكانية اختيار حجم الموديل.':
    "Runs on the organization's own servers without sending any data to a third party — the best choice for sensitive data, with a choice of model size.",
  // ---- share / roles / rename / wizard / user modals ----
  'مشاركة اللوحة': 'Share Dashboard',
  'أي شخص معه الرابط يقدر يفتح نسخة ثابتة (لحظة إنشاء الرابط) من اللوحة — بدون تسجيل دخول. النسخة لا تتحدث لاحقًا.':
    'Anyone with the link can open a fixed snapshot (as of when the link was created) of the dashboard — no sign-in needed. The snapshot never updates afterward.',
  'انتهاء الرابط': 'Link Expiry', 'بعد 24 ساعة': 'After 24 hours', 'بعد 7 أيام': 'After 7 days',
  'بعد 30 يومًا': 'After 30 days', 'بعد 60 يومًا': 'After 60 days', 'بعد 90 يومًا': 'After 90 days',
  'تاريخ محدد…': 'Specific date…', '🔗 إنشاء الرابط': '🔗 Create Link', 'نسخ': 'Copy',
  'لإدارة روابط المشاركة السابقة (الإلغاء، متابعة عدد المشاهدات) —': 'To manage previous share links (revoke, track view counts) —',
  'روابط المشاركة ←': 'Share Links ←', 'إدارة صلاحيات اللوحة': 'Manage Dashboard Permissions',
  'المالك:': 'Owner:', 'نقل الملكية': 'Transfer Ownership',
  'المالك الجديد لازم يكون بالفعل عنده صلاحية وصول للبيانات اللي تعتمد عليها هذه اللوحة.':
    'The new Owner must already have access to the data this dashboard depends on.',
  'محررون ومشاهدون': 'Editors and Viewers', 'تعديل الاسم والوصف': 'Edit Name and Description',
  'إضافة عنصر': 'Add Widget', '→ رجوع': '→ Back', 'التالي': 'Next',
  'مستخدم جديد': 'New User', 'الاسم المعروض': 'Display Name',
  'محلي (اسم مستخدم وكلمة مرور)': 'Local (username & password)',
  'مستخدم عادي': 'Regular User', 'مسؤول': 'Admin', 'الحساب نشط': 'Account active',
  'الوصول لكل الأنظمة': 'Access to all systems',
  'صلاحيات الملفات بتتحدد من مستودع الملفات نفسه (زر 🔒 على كل ملف)، مش من هنا.':
    'File permissions are set from the file repository itself (the 🔒 button on each file), not from here.',

  // ---- dynamic: shared small words / connectors ----
  'الكل': 'All', 'أخرى': 'Other', 'الإجمالي': 'Total', 'عام': 'General',
  '٪': '%', '، ': ', ', ' من ': ' of ',

  // ---- dynamic: theme / lang toggle titles set from JS ----
  'التبديل للوضع الداكن': 'Switch to dark mode', 'التبديل للوضع الفاتح': 'Switch to light mode',

  // ---- dynamic: widget source popover ----
  'المؤشر': 'Metric', 'مجمّع حسب': 'Grouped by', 'الفترة': 'Period',
  'مصدر البيانات': 'Data Source', 'طريقة الحساب': 'Calculation Method',
  'غير متأثر بالفلتر': 'Not affected by filters',
  'هذا العنصر مش مبني على استعلام قابل لإعادة التنفيذ، فمش بيتفلتر تلقائيًا.':
    "This widget isn't built on a re-runnable query, so it isn't filtered automatically.",

  // ---- dynamic: forecast ----
  '✕ إخفاء التوقع': '✕ Hide Forecast', '🔮 توقّع الأشهر الجاية': '🔮 Forecast Upcoming Months',
  'جارٍ التوقع…': 'Forecasting…', 'تعذّر التوقع': "Couldn't generate the forecast",
  'تعذّر حساب التوقع: ': "Couldn't calculate the forecast: ",
  'الحد الأعلى للتوقع': 'Forecast Upper Bound', 'الحد الأدنى للتوقع': 'Forecast Lower Bound',
  'متوقّع': 'Forecast', 'انحدار خطي مع تعديل موسمي': 'Linear regression with seasonal adjustment',
  'انحدار خطي': 'Linear regression', '🔮 توقّع إحصائي (': '🔮 Statistical Forecast (',
  'نقطة إضافية مجمّعة ضمن الرسم': 'additional point(s) grouped into the chart',

  // ---- dynamic: widget menu (⋮) ----
  'خيارات العنصر': 'Widget Options', '🔀 تغيير نوع الرسم': '🔀 Change Chart Type',
  '↔️ تغيير الحجم': '↔️ Change Size', '📅 تغيير الفترة': '📅 Change Period',
  '📋 نسخ': '📋 Duplicate', '🗑️ حذف': '🗑️ Delete', 'حذف هذا العنصر؟': 'Delete this widget?',
  'صغير': 'Small', 'عريض (عمودين)': 'Wide (2 columns)', 'واسع (3 أعمدة)': 'Extra Wide (3 columns)',
  'عرض كامل': 'Full Width', 'الحجم': 'Size', 'نوع الرسم': 'Chart Type',
  '📊 أعمدة': '📊 Bar', '📈 خط': '📈 Line', '🥧 دائري': '🥧 Pie', '📋 جدول': '📋 Table',
  '🔢 مؤشر': '🔢 KPI', '📶 جدول تقدم': '📶 Progress Table', '📉 مصفوفة اتجاهات': '📉 Trend Matrix',
  '🚦 شريط حالة': '🚦 Status Bar', '🎯 مؤشر دائري': '🎯 Radial Gauge', '🎚️ مؤشر شريطي': '🎚️ Linear Gauge',
  'جارٍ الحفظ…': 'Saving…', 'تعذّر الحفظ: ': "Couldn't save: ", 'تعذّر الحفظ': "Couldn't save",
  'فشل الحفظ': 'Save failed', 'لوحة معلومات': 'Dashboard',

  // ---- dynamic: add-widget wizard ----
  'مقارنة': 'Comparison', 'اتجاه عبر الزمن': 'Trend Over Time', 'مؤشر رئيسي': 'Key Metric',
  'توزيع': 'Distribution', 'جدول': 'Table',
  'هذا الشهر': 'This Month', 'الشهر الماضي': 'Last Month', 'آخر 3 أشهر': 'Last 3 Months',
  'آخر 6 أشهر': 'Last 6 Months', 'هذا العام': 'This Year', 'كل البيانات': 'All Data', 'مخصص': 'Custom',
  'ماذا تريد أن تضيف؟': 'What would you like to add?', 'جارٍ التحميل…': 'Loading…',
  'لا تتوفر بيانات كافية لإنشاء هذا العنصر.<br>': "There isn't enough data to create this widget.<br>",
  'فعّل مصدرًا من قائمة «المصادر» أعلى الصفحة أو ارفع ملفًا في مستودع الملفات.':
    'Enable a source from the "Sources" menu above, or upload a file to the file repository.',
  'من أي مصدر بيانات؟': 'Which data source?', 'توزيع حسب ماذا؟': 'Distribution by what?',
  'مقارنة ماذا؟': 'Compare what?', 'ماذا تريد قياسه؟': 'What would you like to measure?',
  'الفترة؟': 'Which period?', 'حدد الفترة': 'Select the Period',
  'من': 'From', 'إلى': 'To',
  'التجميع؟': 'Group by?', 'يوميًا': 'Daily', 'أسبوعيًا': 'Weekly', 'شهريًا': 'Monthly',
  'اختر الأعمدة': 'Select Columns', 'تحديث الفترة': 'Update Period', 'إضافة العنصر': 'Add Widget',
  'جارٍ تجهيز المعاينة…': 'Preparing preview…', '💾 تحديث': '💾 Update', '➕ إضافة': '➕ Add',
  'تعذّر التنفيذ': "Couldn't run the query", 'تعذّر تنفيذ الاستعلام.': "Couldn't run the query.",
  'لا يوجد بيانات': 'No data available',

  // ---- dynamic: comparison / progress-table / trend-matrix / status-bar widgets ----
  'التقدّم': 'Progress', 'الاتجاه': 'Trend', 'الفترة الأولى': 'First Period',
  'الفترة الثانية': 'Second Period', 'البند': 'Item', 'القيمة': 'Value', 'التفاصيل': 'Details',
  'العدد': 'Count',

  // ---- dynamic: export ----
  'جارٍ التصدير…': 'Exporting…', 'فشل التصدير': 'Export failed',
  'تعذّر تصدير العرض التقديمي: ': "Couldn't export the presentation: ",

  // ---- dynamic: dashboard toolbar / filters / sources summary ----
  'جارٍ التحديث…': 'Refreshing…',
  'هذه اللوحة معطّلة حاليًا.': 'This dashboard is currently disabled.',
  '🗄️ مصادر البيانات:': '🗄️ Data Sources:',
  '✕ مسح كل الفلاتر': '✕ Clear All Filters', 'اضغط لإزالة هذا الفلتر': 'Click to remove this filter',
  'نشطة': 'Active', '⚙️ إدارة الصلاحيات': '⚙️ Manage Permissions',

  // ---- dynamic: chat panel ----
  'قراءة بصوت عالٍ': 'Read aloud',
  'اسأل سؤالاً عن بياناتك وسيبني لك المساعد لوحة معلومات.':
    'Ask a question about your data and the assistant will build you a dashboard.',
  'جرّب: «ما إجمالي الإيرادات حسب المنطقة؟»': 'Try: "What is total revenue by region?"',
  'اسأل سؤالاً عن بياناتك': 'Ask a question about your data',
  'مثال: «قارن الإيرادات بين المناطق» أو «أعطني تقريراً شاملاً»':
    'Example: "Compare revenue by region" or "Give me a comprehensive report"',
  '🔊 قراءة تلقائية': '🔊 Auto Read',
  'محتاج إذن استخدام الميكروفون من المتصفح.': 'Microphone permission is needed from the browser.',
  'مفيش ميكروفون متاح.': 'No microphone available.',
  'تعذّر التعرف على الصوت: ': "Couldn't recognize speech: ",
  'خليه شهري': 'Make it monthly', 'اعرض آخر 3 شهور بس': 'Show only the last 3 months',
  'غيّر الرسم لخط بياني': 'Change the chart to a line chart',
  'أضف رسم توزيع (Pie)': 'Add a distribution chart (Pie)', 'أضف جدول تفصيلي': 'Add a detailed table',
  'اعمل تقرير شامل بدل كده': 'Make a comprehensive report instead',
  'تعذّر تنفيذ الطلب': "Couldn't process the request",
  'تعذّر الوصول إلى الخادم: ': "Couldn't reach the server: ",
  'اسأل استفسارًا سريعًا عن بياناتك — إجابة نصية مباشرة، من غير بناء لوحة…':
    'Ask a quick question about your data — a direct text answer, without building a dashboard…',
  'ⓘ المصدر': 'ⓘ Source', '➕ أضف إلى لوحة المتابعة': '➕ Add to Follow-up Dashboard',
  'اسأل استفسارًا وسيجاوبك المساعد بإجابة نصية مباشرة، من غير بناء لوحة.':
    'Ask a question and the assistant will reply with a direct text answer, without building a dashboard.',
  'جرّب: «كام إجمالي الإيرادات الشهر ده؟»': 'Try: "What is total revenue this month?"',
  'تعذّر إضافة الإجابة إلى اللوحة': "Couldn't add the answer to the dashboard",

  // ---- dynamic: sources dropdown / repository ----
  'غير مربوط بعد': 'Not connected yet', '⟳ جلب': '⟳ Fetch',
  'لا يوجد ملفات بعد — ارفع ملفات أولاً': 'No files yet — upload files first',
  'تعذّر الجلب: ': "Couldn't fetch: ", 'لم يتم الجلب بعد — اضغط «جلب»': 'Not fetched yet — click "Fetch"',
  ' سجل · آخر جلب ': ' records · last fetched ', '⟳ جارٍ الجلب…': '⟳ Fetching…',
  'جارٍ الجلب من النظام…': 'Fetching from the system…', 'فشل الجلب': 'Fetch failed',
  'لا يوجد مصادر': 'No sources', 'لا يوجد مصدر مفعّل': 'No source enabled',
  'مصادر مفعّلة': 'sources enabled', 'صفحة': 'page(s)', 'صف': 'row(s)', 'عمود': 'column(s)',
  'اليوم': 'Today', 'أمس': 'Yesterday', 'ملفك': 'Your file', 'لديك صلاحية': 'You have access',
  '🔒 مقيّد': '🔒 Restricted', 'صلاحيات الملف': 'File Permissions',
  'تحديث البيانات': 'Update Data', 'التفاصيل': 'Details', 'الملفات المرتبطة': 'Related Files',
  'تنزيل الملف': 'Download File',
  'لا يوجد ملفات في هذا التصنيف': 'No files in this category',
  'لا يوجد ملفات ': 'No files ', 'في هذا التصنيف': 'in this category', 'في المستودع بعد': 'in the repository yet',
  '. ارفع ملفاً من الأعلى.': '. Upload a file from above.',
  'في المستودع بعد. ارفع ملفاً من الأعلى.': 'in the repository yet. Upload a file from above.',
  'اسم العرض (إجباري)': 'Display name (required)', 'وصف مختصر': 'Short description',
  'التصنيف (مثلاً: المالية)': 'Category (e.g. Finance)', 'حفظ في المستودع': 'Save to Repository',
  'اسم العرض إجباري.': 'Display name is required.', 'جارٍ الحفظ: ': 'Saving: ',
  'جارٍ رفع ': 'Uploading ', ' ملف وتحليل محتواه…': ' file(s) and analyzing content…',
  'جارٍ قراءة "': 'Reading "', '" بالموديل الداخلي — صفحة ': '" with the internal model — page ',
  ' من ': ' of ', 'تعذّر الرفع': 'Upload failed', 'تعذّر قراءة الملف.': "Couldn't read the file.",
  'فشل التحديث': 'Update failed', 'تعذّر تحديث بيانات الملف: ': "Couldn't update the file's data: ",
  'حذف "': 'Delete "', '" من المستودع؟ لا يمكن التراجع عن هذا.': '" from the repository? This cannot be undone.',
  'ليس لديك صلاحية حذف هذا الملف — الحذف مقصور على من رفعه أو على المسؤول.':
    "You don't have permission to delete this file — only the uploader or an admin can delete it.",
  'تعذّر حذف الملف': "Couldn't delete the file", 'تعذّر حفظ التفاصيل.': "Couldn't save the details.",
  'لا نتائج.': 'No results.', 'إزالة': 'Remove',
  'لا يوجد مستخدمون إضافيون بعد.': 'No additional users yet.',
  'تعذّر حفظ الصلاحيات.': "Couldn't save the permissions.",
  'تعذّر العثور على هذا الملف — يبدو أنه حُذف أو تحدّث. أعد فتح صلاحياته من قائمة الملفات.':
    "Couldn't find this file — it may have been deleted or updated. Reopen its permissions from the file list.",
  'تعذّر تحميل صلاحيات الملف الحالية.': "Couldn't load the file's current permissions.",
  'لا يوجد ملفات أخرى': 'No other files',
  'العمود المشترك: ': 'Shared column: ', 'إزالة الربط': 'Remove Link',
  'لا يوجد ملفات مرتبطة بعد.': 'No related files yet.',
  'اختر ملفًا واكتب اسم العمود المشترك.': 'Choose a file and enter the shared column name.',
  'تعذّر إضافة الربط.': "Couldn't add the link.",

  // ---- dynamic: history / active dashboards / share links ----
  ' لوحة محفوظة': ' saved dashboard(s)', 'لا يوجد لوحات محفوظة بعد': 'No saved dashboards yet',
  'مسودة': 'Draft', '⚠️ معطّلة': '⚠️ Disabled', 'المالك: ': 'Owner: ',
  'تفعيل': 'Activate', 'عنصر': 'widget(s)', 'فتح': 'Open',
  'لا يوجد لوحات محفوظة بعد — أي سؤال تسأله يُحفظ هنا تلقائياً.': 'No saved dashboards yet — every question you ask is saved here automatically.',
  ' لوحة نشطة': ' active dashboard(s)', 'لا يوجد لوحات نشطة بعد': 'No active dashboards yet',
  '⚠ معطّلة': '⚠ Disabled', 'تعديل الاسم والوصف': 'Edit Name and Description',
  'إدارة الصلاحيات': 'Manage Permissions',
  'لا يوجد لوحات نشطة بعد — فعّل لوحة من تبويب "السجل" لتظهر هنا.':
    'No active dashboards yet — activate a dashboard from the "History" tab for it to appear here.',
  'تعذّر تفعيل اللوحة.': "Couldn't activate the dashboard.", 'تعذّر الوصول إلى الخادم.': "Couldn't reach the server.",
  'حذف هذه اللوحة من السجل؟': 'Delete this dashboard from history?',
  'تعذّر تحميل الصلاحيات.': "Couldn't load the permissions.",
  'لا يوجد محررون أو مشاهدون بعد.': 'No editors or viewers yet.',
  'تعذّر إضافة الدور.': "Couldn't add the role.",
  'نقل ملكية اللوحة إلى ': 'Transfer dashboard ownership to ', '؟': '?',
  'تعذّر نقل الملكية.': "Couldn't transfer ownership.",
  'الاسم مطلوب.': 'Name is required.', 'مسح كل السجل؟ لا يمكن التراجع عن هذا.': 'Clear all history? This cannot be undone.',
  'إظهار لوحة المحادثة': 'Show Chat Panel',
  'الملف يجب أن يكون صورة.': 'The file must be an image.',
  'حجم الصورة كبير جداً (الحد الأقصى 15MB).': 'The image is too large (15MB maximum).',
  'تعذّر قراءة الصورة.': "Couldn't read the image.",
  'يجب تفعيل اللوحة أولاً قبل مشاركتها.': 'The dashboard must be activated before sharing it.',
  'فعّل اللوحة أولاً قبل مشاركتها — زر «✅ تفعيل اللوحة» بجانبه.':
    'Activate the dashboard first before sharing it — see the "✅ Activate Dashboard" button next to it.',
  'جارٍ الإنشاء…': 'Creating…', 'فشل إنشاء الرابط': 'Failed to create the link',
  'تعذّر إنشاء رابط المشاركة: ': "Couldn't create the share link: ", 'تم النسخ ✓': 'Copied ✓',
  'أُلغي': 'Revoked', 'منتهي': 'Expired', 'نشط': 'Live',
  'جارٍ التحميل…': 'Loading…',
  ' رابط مشاركة': ' share link(s)', 'لا يوجد روابط مشاركة بعد': 'No share links yet',
  'لا يوجد روابط مشاركة بعد — أنشئ رابطًا من زر "🔗 مشاركة" على أي لوحة.':
    'No share links yet — create one from the "🔗 Share" button on any dashboard.',
  'بدون انتهاء': 'Never expires', 'إلغاء الرابط': 'Revoke Link', 'نسخ الرابط': 'Copy Link',
  'ينتهي: ': 'Expires: ', 'مشاهدة': 'view(s)', 'حذف رابط المشاركة هذا؟': 'Delete this share link?',
  'تعذّر تحميل الروابط.': "Couldn't load the links.",
  'رابط غير نشط': 'Inactive Link', 'رابط غير نشط — لوحة مشاركة': 'Inactive Link — Shared Dashboard',
  'هذا الرابط لم يعد نشطًا.': 'This link is no longer active.',
  'تواصل مع ': 'Contact ', ' للحصول على رابط جديد.': ' to get a new link.', 'صاحب الرابط': 'the link owner',
  'الرابط غير موجود أو تم حذفه.': "The link doesn't exist or was deleted.",
  'تعذّر التحميل': "Couldn't load", ' — لوحة مشاركة': ' — Shared Dashboard',

  // ---- dynamic: settings ----
  'مسؤول': 'Admin', 'مستخدم': 'User', 'الموديل الداخلي (Ollama)': 'Internal Model (Ollama)',
  'تعذّر تحميل إعدادات الموديل.': "Couldn't load the model settings.",
  'معطّل (استخراج نص عادي فقط)': 'Disabled (plain text extraction only)',
  '⚠️ قد لا يدعم الصور — لو فشل بيرجع تلقائيًا لقراءة PdfPig العادية':
    '⚠️ May not support images — falls back automatically to regular PdfPig reading on failure',
  'مُعد': 'Configured', 'غير مُعد': 'Not Configured', 'فشل الحفظ': 'Save failed',
  'تعذّر تحميل قائمة الموديلات.': "Couldn't load the model list.",
  'مُعد على الخادم': 'Configured on the server', 'غير مُعد على الخادم': 'Not configured on the server',
  'تعذّر تسجيل الدخول.': "Couldn't sign in.",

  // ---- dynamic: onboarding tour ----
  '👋 مرحبًا بك': '👋 Welcome',
  'جولة موجزة تتناول أهم صفحات النظام. يمكنك تخطّيها الآن، وإعادة تشغيلها في أي وقت لاحقًا من الزر ❓ أعلى الصفحة.':
    'A short tour of the system\'s main pages. You can skip it now and replay it any time later from the ❓ button at the top of the page.',
  '💬 المحادثة': '💬 Chat',
  'الصفحة الرئيسية: اكتب سؤالك بلغة عربية طبيعية عن بياناتك، ويقوم النظام ببناء لوحة معلومات تفاعلية فورًا.':
    'The main page: type your question about your data in natural language, and the system builds an interactive dashboard instantly.',
  '🧭 بناء اللوحة / الاستفسارات': '🧭 Build Dashboard / Inquiries',
  '«بناء اللوحة» يحوّل سؤالك إلى عناصر رسومية وجداول. أما «الاستفسارات» فيقدّم إجابة نصية سريعة دون بناء لوحة — مناسب للأسئلة المباشرة.':
    '"Build Dashboard" turns your question into charts and tables. "Inquiries" gives a quick text answer without building a dashboard — suited to direct questions.',
  '📁 مصادر البيانات': '📁 Data Sources',
  'من هنا يمكن التحكم في الأنظمة أو الملفات المفعّلة وقت السؤال، واقتصار البحث على مصدر معيّن عند الحاجة.':
    'From here you can control which systems or files are enabled when asking a question, and restrict the search to a specific source when needed.',
  '📎 مستودع الملفات': '📎 File Repository',
  'من هذه الصفحة تُرفع ملفات Excel أو CSV أو PDF، لتُستخدم كمصدر بيانات يمكن الاستفسار عنه أسوة بأي نظام متصل.':
    'From this page you upload Excel, CSV, or PDF files, to be used as a data source you can query just like any connected system.',
  '🕘 السجل': '🕘 History',
  'يضم جميع اللوحات التي سبق إنشاؤها، ويتيح العودة إلى أي منها لمتابعة العمل عليها أو تعديلها.':
    'Holds every dashboard you have created, letting you return to any of them to continue working on it or edit it.',
  '✅ اللوحات النشطة': '✅ Active Dashboards',
  'لوحات مفعّلة يمكن مشاركتها مع فريق العمل بأدوار مختلفة (مالك/محرر/مشاهد)، وتظل بياناتها تتحدّث تلقائيًا من المصدر الحي.':
    'Activated dashboards that can be shared with your team under different roles (Owner/Editor/Viewer), and whose data keeps updating automatically from the live source.',
  '🔗 روابط المشاركة': '🔗 Share Links',
  'روابط ثابتة لمشاركة نسخة لحظية من لوحة مع أي شخص، دون الحاجة إلى تسجيل الدخول.':
    'Fixed links for sharing a snapshot of a dashboard with anyone, without needing to sign in.',
  '👥 المستخدمون': '👥 Users',
  'إدارة حسابات المستخدمين وأدوارهم والأنظمة المسموح لهم بالوصول إليها.':
    'Manage user accounts, their roles, and the systems they are allowed to access.',
  '⚙️ الإعدادات': '⚙️ Settings',
  'التحكم في نموذج الذكاء الاصطناعي المستخدم لبناء اللوحات، وفي نموذج قراءة ملفات PDF بشكل مستقل عنه.':
    'Control the AI model used to build dashboards, and the PDF-reading model independently of it.',
  '📊 الاستهلاك': '📊 Usage',
  'سجل تفصيلي لكل سؤال أُرسل إلى النموذج: عدد التوكنز المستخدمة، التكلفة التقديرية، والمستخدم الذي طرح السؤال.':
    'A detailed log of every question sent to the model: tokens used, estimated cost, and the user who asked it.',
  '❓ هل تحتاج إلى الدليل مرة أخرى؟': '❓ Need the guide again?',
  'اضغط هذا الزر في أي وقت لإعادة عرض هذا الدليل من البداية.':
    'Click this button any time to replay this guide from the start.',
  'السابق': 'Previous', 'إنهاء': 'Finish', 'تخطّي الدليل': 'Skip Guide',

  // ---- dynamic: users screen ----
  ' مستخدم': ' user(s)', 'محلي': 'Local', 'نظام': 'system(s)', 'كل الأنظمة': 'All Systems',
  'الأنظمة': 'Systems', 'حذف': 'Delete',
  'نشط': 'Active', 'معطّل': 'Disabled', 'تعديل': 'Edit',
  'لا يوجد عناصر بعد': 'No items yet',
  'مستخدم جديد': 'New User', '(اتركها فاضية لعدم التغيير)': "(leave blank to keep it unchanged)",
  'فشل الحفظ': 'Save failed',
};
const I18N_ENTRIES = Object.entries(I18N_EN).sort((a, b) => b[0].length - a[0].length);

const ARABIC_CHARS_RE = /[؀-ۿ]/;

function translateText(text) {
  let result = text;
  for (const [ar, en] of I18N_ENTRIES) {
    if (result.indexOf(ar) !== -1) result = result.split(ar).join(en);
  }
  // A short dictionary entry (e.g. a single word) can substring-match inside a longer
  // Arabic sentence that isn't itself in the dictionary yet, leaving a garbled mix of
  // English and Arabic. Only accept the translation once no Arabic script remains —
  // otherwise keep the original text untranslated rather than show broken output.
  if (ARABIC_CHARS_RE.test(result)) return text;
  return result;
}

const I18N_ATTRS = ['placeholder', 'title', 'aria-label'];

function translateNode(node) {
  if (!node) return;
  if (node.nodeType === Node.TEXT_NODE) {
    const t = translateText(node.textContent);
    if (t !== node.textContent) node.textContent = t;
    return;
  }
  if (node.nodeType !== Node.ELEMENT_NODE) return;
  if (node.tagName === 'SCRIPT' || node.tagName === 'STYLE') return;
  I18N_ATTRS.forEach(attr => {
    const v = node.getAttribute && node.getAttribute(attr);
    if (v) {
      const t = translateText(v);
      if (t !== v) node.setAttribute(attr, t);
    }
  });
  node.childNodes.forEach(translateNode);
}

// English mode: translate whatever the server already sent in the initial HTML first (nav
// labels, static modal markup — everything before any JS has rendered a single thing), then
// keep translating everything the app renders afterward. Every screen (chat, dashboard,
// repository, wizard, popovers…) builds fresh Arabic DOM straight from JS on its own
// schedule, so a live observer catches all of it without needing a hook added to each one
// individually.
if (document.documentElement.getAttribute('lang') === 'en') {
  document.addEventListener('DOMContentLoaded', () => translateNode(document.body));
  new MutationObserver(mutations => {
    mutations.forEach(m => {
      if (m.type === 'characterData') { translateNode(m.target); return; }
      if (m.type === 'attributes') {
        const v = m.target.getAttribute && m.target.getAttribute(m.attributeName);
        if (v) {
          const t = translateText(v);
          if (t !== v) m.target.setAttribute(m.attributeName, t);
        }
        return;
      }
      m.addedNodes.forEach(translateNode);
    });
  }).observe(document.documentElement, {
    childList: true, subtree: true, characterData: true,
    attributes: true, attributeFilter: I18N_ATTRS,
  });
  // alert()/confirm() are native browser dialogs, not DOM nodes — the observer above can
  // never reach their text, so their message is translated here instead.
  const nativeAlert = window.alert.bind(window);
  window.alert = (msg) => nativeAlert(translateText(String(msg)));
  const nativeConfirm = window.confirm.bind(window);
  window.confirm = (msg) => nativeConfirm(translateText(String(msg)));
}

const state = {
  lang: document.documentElement.getAttribute('lang') === 'en' ? 'en' : 'ar',
  messages: [], dashboard: null, loading: false,
  // "الاستفسارات" sub-tab: fully isolated from the dashboard-building chat above — its own
  // message log, never touches state.dashboard/currentHistoryId unless the user explicitly
  // clicks "➕ أضف إلى لوحة المتابعة" on one answer (see convertInquiryToDashboard).
  chatMode: 'dashboard',
  // الاستفسارات: state.inquiryTurns is the currently-open saved conversation's own turns
  // ({role, text, blocks, createdAt} — see Models/ConversationModels.cs), never a bare
  // {role,text} pair the way state.messages is — inquiryConversationId is null until the
  // first ask() in a brand-new conversation gets back its generated id.
  inquiryTurns: [], inquiryConversationId: null,
  inquiryConversations: [], inquiryConversationsLoaded: false,
  systems: [], sourceFiles: [], tableLabels: {}, // available, from /api/sources
  onSystems: new Set(), onFiles: new Set(), // enabled
  files: [], pending: [], filter: 'الكل',
  history: [], historyLoaded: false,
  attachedImage: null, attachedImageName: '',
  currentUser: null, users: [],
  editMode: false, editHistory: { past: [], future: [] },
  currentHistoryId: null, wizard: null,
  activeFilters: {}, // filterId -> selected values (or [from,to] for range types)
  filterRefreshWarning: null, // set by applyFilters() when a widget fetch still fails after its retry
  filtersLoading: false, // true while applyFilters() has a request in flight — see updateFiltersLoadingUI()
  // Ad-hoc filter definitions created by clicking a chart's data point (see
  // toggleCrossFilter) for a table+field the dashboard has no declared DashboardFilter
  // for — same shape as one, just never sent to/persisted from the backend's own filters
  // list. Keyed by the same id used in activeFilters, so getActiveFilters()/applyFilters()
  // handle both kinds identically without knowing which is which.
  crossFilterDefs: {},
  // Set only while the open dashboard is an Active one (see openHistoryEntry): gates
  // edit/wizard controls for a Viewer and drives the disabled-state banner.
  dashboardIsActive: false, dashboardRole: null, dashboardDisabled: false, dashboardDisabledReason: '',
};

// Every widget needs a stable client-side id (delete/duplicate/reorder/undo all key off
// it) and a layout.size (small/wide/full) driving its grid span — neither exists in the
// model's JSON contract, so they're assigned here the moment a dashboard enters state,
// never sent back to /api/chat.
function ensureWidgetMeta(dashboard) {
  (dashboard?.widgets || []).forEach(w => {
    if (!w.id) w.id = 'w' + Math.random().toString(36).slice(2, 10);
    if (!w.layout) w.layout = { size: w.type === 'kpi' ? 'small' : (w.type === 'table' ? 'full' : 'wide') };
  });
  return dashboard;
}

// Every API call rides on the auth cookie automatically (same-origin fetch). If the
// session ever stops being valid mid-use — expired, or an admin deactivated the account —
// any 401 sends the whole page back to the login screen rather than failing silently in
// place, without needing every individual fetch() call site to check for it.
const nativeFetch = window.fetch.bind(window);
window.fetch = async (...args) => {
  const res = await nativeFetch(...args);
  const url = String(args[0]);
  if (res.status === 401 && !url.startsWith('/api/auth/') && !url.startsWith('/api/share/')) location.reload();
  return res;
};

// ---------- design tokens ----------
// The single source every chart pulls its colors/fonts from — a JS mirror of the CSS
// custom properties in :root above. No chart component sets a color or font ad hoc;
// they all go through THEME so every chart, on every dashboard, looks like one system.
// Light and dark — same chart palette in both (a category means the same color either
// way), only the grid/tick/accent colors change, mirroring the CSS :root tokens above.
const THEMES = {
  dark: {
    palette: ['#3cb4e5', '#4d4184', '#00bbb4', '#007cbb', '#9dd3c9'],
    accent: '#3cb4e5', danger: '#EF6461', amber: '#F2B84B',
    gridColor: '#333436', tickColor: '#9A9CA0', ink: '#EDEFF1', surface: '#1A1B1D',
    fontFamily: 'Tanseek Modern Pro Arabic',
  },
  light: {
    palette: ['#3cb4e5', '#4d4184', '#00bbb4', '#007cbb', '#9dd3c9'],
    accent: '#007cbb', danger: '#D3402F', amber: '#B8730A',
    gridColor: '#E3E4E6', tickColor: '#6B6D70', ink: '#434345', surface: '#FFFFFF',
    fontFamily: 'Tanseek Modern Pro Arabic',
  },
};
// Reassigned by applyTheme() below — every chart component reads THEME.x at render
// time, so switching this and rebuilding the dashboard is enough to re-theme charts
// too (an already-rendered chart's SVG colors are static once drawn; CSS alone can't
// recolor them — ApexCharts has no global defaults object, so THEME is read fresh by
// chartConfig() on every rebuild instead).
let THEME = THEMES[document.documentElement.getAttribute('data-theme') === 'light' ? 'light' : 'dark'];

function applyTheme(name) {
  name = name === 'light' ? 'light' : 'dark';
  document.documentElement.setAttribute('data-theme', name);
  THEME = THEMES[name];
  try { localStorage.setItem('chatToDashboardTheme', name); } catch {}
  const btn = el('theme-toggle');
  btn.textContent = name === 'light' ? '🌙' : '☀️';
  btn.title = name === 'light' ? 'التبديل للوضع الداكن' : 'التبديل للوضع الفاتح';
  // Existing charts were painted with the old theme's colors baked into their SVG —
  // rebuild the dashboard so anything already on screen reflects the new theme too.
  if (state.dashboard) renderDashboard();
}

// The model only ever controls widget content (type/title/data) — never layout or
// style. Anything outside this whitelist renders as a table instead of risking a
// broken grid or a crash on an unexpected type.
const WIDGET_TYPES = ['kpi', 'bar', 'line', 'pie', 'table', 'progress-table', 'trend-matrix', 'status-bar', 'radial-gauge', 'linear-gauge'];
// Defense in depth: regardless of what the system prompt asked the model to do, a
// chart never renders more than this many points — the overflow is folded into one
// summed "+N more" bucket instead of an unreadably dense chart.
const MAX_CHART_POINTS = 15;

// ---------- helpers ----------
const esc = s => String(s ?? '').replace(/[&<>"']/g, c => ({ '&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;' }[c]));

function fmt(v) {
  if (typeof v !== 'number' || !isFinite(v)) return String(v ?? '—');
  const a = Math.abs(v);
  if (a >= 1e9) return (v / 1e9).toFixed(1) + 'B';
  if (a >= 1e6) return (v / 1e6).toFixed(1) + 'M';
  if (a >= 1e4) return (v / 1e3).toFixed(1) + 'K';
  return v.toLocaleString(undefined, { maximumFractionDigits: 2 });
}
const num = v => typeof v === 'number' ? v : (parseFloat(v) || 0);

// Counts a KPI up from 0 to its real value over ~700ms instead of just printing it — only
// called for a freshly (re)generated dashboard's reveal (see revealNow), never on a filter
// or edit re-render, where the number must just be correct immediately with no animation.
function animateKpiValue(target, value) {
  const start = performance.now();
  const duration = 700;
  function step(now) {
    const t = Math.min(1, (now - start) / duration);
    const eased = 1 - Math.pow(1 - t, 3);
    target.textContent = fmt(value * eased);
    if (t < 1) requestAnimationFrame(step);
  }
  requestAnimationFrame(step);
}

// A donut's idle center caption names what the number actually is — "مشروع" for a count of
// projects, "خطر" for a count of risks — instead of the generic "الإجمالي", which said
// nothing about the number itself. There's no structured "what this counts" field in the
// widget schema (chat-authored widgets carry only a title + raw SQL), so this is pulled
// from the widget's own title: strip a leading aggregate word ("عدد المخاطر" -> "المخاطر")
// and a trailing "by ..." clause ("توزيع المشاريع حسب الحالة" -> "المشاريع"), then singularize
// the remaining noun via a small curated dictionary of common business-Arabic plurals —
// genuine Arabic morphology (broken plurals especially) has no reliable general rule, so
// unlisted nouns are shown as extracted rather than risk a wrong guess.
const PLURAL_TO_SINGULAR = {
  'مشاريع': 'مشروع', 'مخاطر': 'خطر', 'عقود': 'عقد', 'طلبات': 'طلب', 'طلبيات': 'طلبية',
  'فواتير': 'فاتورة', 'عملاء': 'عميل', 'زبائن': 'زبون', 'موردين': 'مورد', 'موردون': 'مورد',
  'موظفين': 'موظف', 'موظفون': 'موظف', 'مستخدمين': 'مستخدم', 'مستخدمون': 'مستخدم',
  'منتجات': 'منتج', 'أصناف': 'صنف', 'حسابات': 'حساب', 'تقارير': 'تقرير', 'ملفات': 'ملف',
  'مستندات': 'مستند', 'وثائق': 'وثيقة', 'شركات': 'شركة', 'فروع': 'فرع', 'وحدات': 'وحدة',
  'أقسام': 'قسم', 'حالات': 'حالة', 'مهام': 'مهمة', 'إيرادات': 'إيراد', 'مصروفات': 'مصروف',
  'نفقات': 'نفقة', 'أصول': 'أصل', 'شكاوى': 'شكوى', 'حوادث': 'حادث', 'مخالفات': 'مخالفة',
  'تراخيص': 'ترخيص', 'عقارات': 'عقار', 'فرص': 'فرصة', 'صفقات': 'صفقة', 'اجتماعات': 'اجتماع',
  'تذاكر': 'تذكرة', 'شحنات': 'شحنة', 'قضايا': 'قضية', 'مطالبات': 'مطالبة', 'مخازن': 'مخزن',
  'عمليات': 'عملية', 'معاملات': 'معاملة', 'حملات': 'حملة', 'مبادرات': 'مبادرة', 'مهارات': 'مهارة',
  'دورات': 'دورة', 'إجازات': 'إجازة', 'مبيعات': 'مبيعات',
};
function deriveUnitLabel(title) {
  if (!title) return 'الإجمالي';
  let t = title
    .replace(/^(توزيع|نسبة|عدد|إجمالي|مجموع|تحليل|ملخص|أعداد|نسب)\s+/u, '')
    .replace(/\s+(موزعة\s+)?(حسب|بحسب|لكل|بين|عبر|على\s+حسب).*$/u, '')
    .trim();
  const bare = t.replace(/^ال/u, '');
  return PLURAL_TO_SINGULAR[bare] || PLURAL_TO_SINGULAR[t] || bare || 'الإجمالي';
}

function keysFor(w) {
  const rows = Array.isArray(w.data) ? w.data : [];
  const first = rows[0] || {};
  const ks = Object.keys(first);
  const xKey = w.xKey || ks.find(k => typeof first[k] !== 'number') || ks[0];
  const yKey = w.yKey || ks.find(k => k !== xKey && typeof first[k] === 'number') || ks[1];
  return { rows, xKey, yKey };
}

function capRows(rows, xKey, yKey) {
  if (rows.length <= MAX_CHART_POINTS) return { rows, truncated: 0 };
  const kept = rows.slice(0, MAX_CHART_POINTS - 1);
  const rest = rows.slice(MAX_CHART_POINTS - 1);
  const bucket = {};
  bucket[xKey] = `+${rest.length} أخرى`;
  bucket[yKey] = rest.reduce((s, r) => s + num(r[yKey]), 0);
  return { rows: [...kept, bucket], truncated: rest.length };
}

/**
 * Widget sources are authored as two sentences: "<where it came from>. <how it was calculated>."
 * Split on the first ". " so each half gets its own labelled block in the ⓘ popover.
 */
function sourcePopover(w) {
  const source = w.source || '';
  const at = source.indexOf('. ');
  const from = at > -1 ? source.slice(0, at + 1) : source;
  const method = at > -1 ? source.slice(at + 2) : '';
  // Widgets built through the "+ إضافة عنصر" wizard carry their real structured query
  // (metric/aggregation/dimension/period) — shown verbatim here, never authored by the
  // model. A chat-generated widget's own `query` (see AnalyticsTools.cs's "حقل query"
  // section) is only ever {table, sql} — set purely so dashboard filters/cross-filtering
  // can re-run it, never carrying metric/dimension — so gate on q.metric specifically
  // (not merely q being present) or this block renders with a "المؤشر" label and nothing
  // under it for every chat-built widget.
  const q = w.query;
  const queryBlock = q?.metric ? `
    <div class="src-block"><span class="src-lbl">المؤشر</span>${esc(q.metric || '')}${q.aggregation ? ` (${esc(q.aggregation.toUpperCase())})` : ''}</div>
    ${q.dimension ? `<div class="src-block"><span class="src-lbl">مجمّع حسب</span>${esc(q.dimension)}</div>` : ''}
    ${q.dateColumn && q.timeRange && q.timeRange !== 'all' ? `<div class="src-block"><span class="src-lbl">الفترة</span>${esc(timeRangeLabel(q.timeRange))}</div>` : ''}
  ` : '';
  return `<div class="src-pop">
    <div class="src-block"><span class="src-lbl">مصدر البيانات</span>${esc(from)}</div>
    ${method ? `<div class="src-block"><span class="src-lbl">طريقة الحساب</span>${esc(method)}</div>` : ''}
    ${queryBlock}
  </div>`;
}
function timeRangeLabel(v) { return (TIME_RANGE_OPTIONS.find(([k]) => k === v) || [v, v])[1]; }

function closePopovers() {
  document.querySelectorAll('.src-pop.open').forEach(p => p.classList.remove('open'));
  document.querySelectorAll('.src-btn.active').forEach(b => b.classList.remove('active'));
  document.querySelectorAll('.inquiry-src-btn.active').forEach(b => b.classList.remove('active'));
}
document.addEventListener('click', closePopovers);

// ---------- widget components ----------
// Every widget renders through exactly one of these five, chosen by validated `type`
// alone — never by its data volume or title length. Chart components share one
// mountChart() so the ApexCharts options object (palette, grid/tick color, font) is
// built in exactly one place.
let chartInstances = [];
// ApexCharts measures its container's real width/height at construction time, so a
// chart created before its widget card is attached to the document renders at 0x0.
// buildWidget() runs for every widget *before* the grid holding them is appended to
// #dash, so mountChart() only queues {card, holder, options} here; renderDashboard()
// instantiates them right after the grid is actually in the DOM.
let pendingCharts = [];

// ---------- dashboard reveal animation ----------
// Set right before a genuinely new/regenerated dashboard is rendered for the first time
// (see ask() in the chat section) — never touched by a filter re-run, an edit, undo/redo,
// or reopening a saved dashboard from السجل, so those all keep rendering instantly as
// before. renderDashboard() consumes it into revealNow for that one render only; KpiCard
// reads revealNow (still module-scope, set synchronously before widgets are built) to
// decide whether to count up instead of just printing the number.
let revealDashboardOnNextRender = false;
let revealNow = false;
function prefersReducedMotion() {
  try { return window.matchMedia('(prefers-reduced-motion: reduce)').matches; } catch { return false; }
}

function widgetShell(w, extraClass) {
  const card = document.createElement('div');
  card.className = 'widget' + (extraClass ? ' ' + extraClass : '');
  card.dataset.size = (w.layout && w.layout.size) || (extraClass === 'kpi' ? 'small' : 'wide');
  if (w.id) card.dataset.widgetId = w.id;

  const head = document.createElement('div');
  head.className = 'widget-head';
  const title = document.createElement('h3');
  title.textContent = w.title || '';
  head.appendChild(title);

  if (w.source) {
    const srcBtn = document.createElement('button');
    srcBtn.type = 'button'; srcBtn.className = 'src-btn'; srcBtn.textContent = 'ⓘ';
    srcBtn.setAttribute('aria-label', 'مصدر البيانات');
    head.appendChild(srcBtn);

    const wrap = document.createElement('div');
    wrap.innerHTML = sourcePopover(w);
    card.appendChild(wrap.firstElementChild);

    srcBtn.addEventListener('click', e => {
      e.stopPropagation();
      const pop = card.querySelector('.src-pop');
      const open = pop.classList.contains('open');
      closePopovers();
      if (!open) { pop.classList.add('open'); srcBtn.classList.add('active'); }
    });
  }
  // Forecasting is opt-in per widget, never automatic — only offered on a bar/line widget
  // with enough historical points for a regression to mean anything.
  if ((w.type === 'bar' || w.type === 'line') && Array.isArray(w.data) && w.data.length >= 3) {
    head.appendChild(buildForecastButton(w));
  }
  card.appendChild(head);

  if (getActiveFilterTables().size && (!w.query || w.query.integrationId || !getActiveFilterTables().has(w.query.table))) {
    const badge = document.createElement('span');
    badge.className = 'widget-unaffected';
    badge.textContent = 'غير متأثر بالفلتر';
    badge.title = 'هذا العنصر مش مبني على استعلام قابل لإعادة التنفيذ، فمش بيتفلتر تلقائيًا.';
    card.appendChild(badge);
  }

  if (state.editMode && w.id) {
    const { btn: menuBtn, menu } = widgetMenu(w);
    head.appendChild(menuBtn);
    card.appendChild(menu);
    enableWidgetDragDrop(card, w);
  }
  return card;
}

// ---------- forecasting ----------
// A real statistical forecast (POST /api/widgets/forecast — ordinary least-squares linear
// regression server-side, see ForecastService.cs), never an LLM guess: this button works on
// whatever data the chart already has client-side, so it's identical for a wizard-built widget
// and a chat-authored one. Strictly opt-in per widget — nothing here ever fires on its own.
function buildForecastButton(w) {
  const btn = document.createElement('button');
  btn.type = 'button';
  btn.className = 'forecast-btn';
  const setLabel = () => { btn.textContent = w.forecast ? '✕ إخفاء التوقع' : '🔮 توقّع الأشهر الجاية'; };
  setLabel();
  btn.addEventListener('click', async e => {
    e.stopPropagation();
    if (w.forecast) {
      delete w.forecast;
      renderDashboard(); scheduleAutosave();
      return;
    }
    const { rows, xKey, yKey } = keysFor(w);
    const values = rows.map(r => num(r[yKey]));
    btn.disabled = true; const original = btn.textContent; btn.textContent = 'جارٍ التوقع…';
    try {
      const res = await fetch('/api/widgets/forecast', {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ labels: rows.map(r => String(r[xKey])), values, periods: 3 }),
      });
      if (!res.ok) {
        const err = await res.json().catch(() => ({}));
        throw new Error(err.error || `تعذّر التوقع (${res.status})`);
      }
      w.forecast = await res.json();
      renderDashboard(); scheduleAutosave();
    } catch (err) {
      alert('تعذّر حساب التوقع: ' + err.message);
      btn.disabled = false; btn.textContent = original;
    }
  });
  return btn;
}

// ---------- widget editor: menu, drag/drop, undo/redo ----------
function closeAllWidgetMenus() {
  document.querySelectorAll('.widget-menu.open').forEach(m => m.classList.remove('open'));
}
document.addEventListener('click', closeAllWidgetMenus);

// Anchors a widget-menu to its ⋮ button so it always stays fully on screen, recomputed every
// time the menu opens or its content is swapped (default/size/type — each a different
// height). CSS alone (a fixed top:44px below the button) put the type submenu's 10 buttons
// wherever the widget happened to sit on the page, with no way to bring the clipped tail
// back into view once a short-viewport user hit it — this instead flips the menu to open
// upward when there isn't room below, and clamps both axes to the viewport.
//
// Positions stay in `menu.offsetParent` (the widget card)'s own coordinate space, not the
// viewport's — position:fixed would seem simpler, but .widget carries an entrance animation
// (`animation:rise`, see its keyframes) whose `transform` makes it the CSS containing block
// for any position:fixed descendant even after the animation ends (a fixed-position element
// then positions relative to that transformed ancestor, not the real viewport, silently
// breaking any viewport-relative math). Computing the desired viewport position and then
// converting it into an offset from the card's own rect sidesteps that entirely.
function positionWidgetMenu(btn, menu) {
  const margin = 8;
  const parent = menu.offsetParent || btn.closest('.widget') || document.body;
  const parentRect = parent.getBoundingClientRect();
  const btnRect = btn.getBoundingClientRect();
  const menuRect = menu.getBoundingClientRect();
  let top = btnRect.bottom + 4;
  if (top + menuRect.height > window.innerHeight - margin) {
    top = Math.max(margin, btnRect.top - menuRect.height - 4);
  }
  // Neither the "below" nor the "flipped up" placement can help once the anchor button
  // itself is barely on screen (near the very top or bottom edge) — this final clamp keeps
  // the menu fully visible regardless, even if that means it no longer hugs the button.
  top = Math.min(Math.max(top, margin), window.innerHeight - menuRect.height - margin);
  const isRtl = document.documentElement.getAttribute('dir') !== 'ltr';
  let left = isRtl ? btnRect.right - menuRect.width : btnRect.left;
  left = Math.min(Math.max(left, margin), window.innerWidth - menuRect.width - margin);
  menu.style.position = 'absolute';
  menu.style.insetInlineEnd = 'auto';
  menu.style.top = (top - parentRect.top) + 'px';
  menu.style.left = (left - parentRect.left) + 'px';
}

function widgetMenu(w) {
  const btn = document.createElement('button');
  btn.type = 'button'; btn.className = 'widget-menu-btn'; btn.textContent = '⋮';
  btn.title = 'خيارات العنصر';
  const menu = document.createElement('div');
  menu.className = 'widget-menu';
  renderWidgetMenuDefault(menu, w);

  btn.addEventListener('click', e => {
    e.stopPropagation();
    const open = menu.classList.contains('open');
    closeAllWidgetMenus();
    if (!open) {
      renderWidgetMenuDefault(menu, w);
      menu.classList.add('open');
      positionWidgetMenu(btn, menu);
    }
  });
  menu.__anchorBtn = btn;
  menu.addEventListener('click', e => e.stopPropagation());
  return { btn, menu };
}

function renderWidgetMenuDefault(menu, w) {
  const canEditData = !!w.query; // only wizard-created widgets carry a re-runnable query
  menu.classList.remove('type-grid'); // only the 10-option "نوع الرسم" list uses the grid layout
  menu.innerHTML = `
    <button data-act="type">🔀 تغيير نوع الرسم</button>
    <button data-act="size">↔️ تغيير الحجم</button>
    ${canEditData ? '<button data-act="period">📅 تغيير الفترة</button>' : ''}
    <button data-act="duplicate">📋 نسخ</button>
    <button data-act="delete" class="danger">🗑️ حذف</button>
  `;
  menu.querySelector('[data-act="type"]').onclick = () => renderWidgetMenuType(menu, w);
  menu.querySelector('[data-act="size"]').onclick = () => renderWidgetMenuSize(menu, w);
  menu.querySelector('[data-act="duplicate"]').onclick = () => { duplicateWidget(w.id); closeAllWidgetMenus(); };
  menu.querySelector('[data-act="delete"]').onclick = () => {
    if (confirm('حذف هذا العنصر؟')) { deleteWidget(w.id); closeAllWidgetMenus(); }
  };
  const periodBtn = menu.querySelector('[data-act="period"]');
  if (periodBtn) periodBtn.onclick = () => { closeAllWidgetMenus(); openPeriodEditor(w); };
}

function renderWidgetMenuSize(menu, w) {
  const current = (w.layout && w.layout.size) || (w.type === 'kpi' ? 'small' : 'wide');
  const opts = [['small', 'صغير'], ['wide', 'عريض (عمودين)'], ['large', 'واسع (3 أعمدة)'], ['full', 'عرض كامل']];
  menu.classList.remove('type-grid');
  menu.innerHTML = `<div class="menu-label">الحجم</div>` +
    opts.map(([v, l]) => `<button data-size="${v}"${v === current ? ' class="current"' : ''}>${l}</button>`).join('');
  menu.querySelectorAll('[data-size]').forEach(b =>
    b.onclick = () => { setWidgetSize(w.id, b.dataset.size); closeAllWidgetMenus(); });
  if (menu.__anchorBtn) positionWidgetMenu(menu.__anchorBtn, menu);
}

function renderWidgetMenuType(menu, w) {
  // bar/line/pie/table/kpi all read the same generic {rows, xKey, yKey}-style data (see
  // keysFor()), so switching among them just reinterprets the same rows. The three newer
  // types below have their own bespoke data shape (progress-table's target/current pairs,
  // trend-matrix's per-period series, status-bar's total+breakdown) that a widget built as
  // one of the five above won't carry — switching into one of these from an incompatible
  // widget shows that type's own empty state (like an unrelated data mismatch already
  // would) rather than breaking, but it won't reshape the data to fit automatically.
  const types = [
    ['bar', '📊 أعمدة'], ['line', '📈 خط'], ['pie', '🥧 دائري'], ['table', '📋 جدول'], ['kpi', '🔢 مؤشر'],
    ['progress-table', '📶 جدول تقدم'], ['trend-matrix', '📉 مصفوفة اتجاهات'], ['status-bar', '🚦 شريط حالة'],
    ['radial-gauge', '🎯 مؤشر دائري'], ['linear-gauge', '🎚️ مؤشر شريطي'],
  ];
  menu.classList.add('type-grid');
  menu.innerHTML = `<div class="menu-label">نوع الرسم</div>` +
    types.map(([v, l]) => `<button data-type="${v}"${v === w.type ? ' class="current"' : ''}>${l}</button>`).join('');
  menu.querySelectorAll('[data-type]').forEach(b =>
    b.onclick = () => { setWidgetType(w.id, b.dataset.type); closeAllWidgetMenus(); });
  if (menu.__anchorBtn) positionWidgetMenu(menu.__anchorBtn, menu);
}

function enableWidgetDragDrop(card, w) {
  card.classList.add('editable');
  card.draggable = true;
  card.addEventListener('dragstart', e => {
    e.dataTransfer.effectAllowed = 'move';
    e.dataTransfer.setData('text/plain', w.id);
    card.classList.add('dragging');
  });
  card.addEventListener('dragend', () => card.classList.remove('dragging'));
  card.addEventListener('dragover', e => { e.preventDefault(); card.classList.add('drag-over'); });
  card.addEventListener('dragleave', () => card.classList.remove('drag-over'));
  card.addEventListener('drop', e => {
    e.preventDefault();
    card.classList.remove('drag-over');
    const fromId = e.dataTransfer.getData('text/plain');
    if (fromId && fromId !== w.id) reorderWidget(fromId, w.id);
  });
}

function findWidget(id) { return (state.dashboard?.widgets || []).find(w => w.id === id); }

function snapshotDashboard() {
  return JSON.parse(JSON.stringify({ summary: state.dashboard.summary, widgets: state.dashboard.widgets }));
}
function updateUndoRedoButtons() {
  el('btn-undo').disabled = !state.editHistory.past.length;
  el('btn-redo').disabled = !state.editHistory.future.length;
}
function pushUndo() {
  if (!state.dashboard) return;
  state.editHistory.past.push(snapshotDashboard());
  if (state.editHistory.past.length > 50) state.editHistory.past.shift();
  state.editHistory.future = [];
}
function applySnapshot(snap) {
  state.dashboard.summary = snap.summary;
  state.dashboard.widgets = snap.widgets;
  renderDashboard();
  scheduleAutosave();
}
function undo() {
  if (!state.editHistory.past.length) return;
  state.editHistory.future.push(snapshotDashboard());
  applySnapshot(state.editHistory.past.pop());
  updateUndoRedoButtons();
}
function redo() {
  if (!state.editHistory.future.length) return;
  state.editHistory.past.push(snapshotDashboard());
  applySnapshot(state.editHistory.future.pop());
  updateUndoRedoButtons();
}
el('btn-undo').addEventListener('click', undo);
el('btn-redo').addEventListener('click', redo);

function deleteWidget(id) {
  pushUndo();
  state.dashboard.widgets = state.dashboard.widgets.filter(w => w.id !== id);
  renderDashboard(); scheduleAutosave(); updateUndoRedoButtons();
}
function duplicateWidget(id) {
  const idx = state.dashboard.widgets.findIndex(w => w.id === id);
  if (idx === -1) return;
  pushUndo();
  const copy = JSON.parse(JSON.stringify(state.dashboard.widgets[idx]));
  copy.id = 'w' + Math.random().toString(36).slice(2, 10);
  state.dashboard.widgets.splice(idx + 1, 0, copy);
  renderDashboard(); scheduleAutosave(); updateUndoRedoButtons();
}
function setWidgetSize(id, size) {
  const w = findWidget(id);
  if (!w) return;
  pushUndo();
  w.layout = { ...(w.layout || {}), size };
  renderDashboard(); scheduleAutosave(); updateUndoRedoButtons();
}
function setWidgetType(id, type) {
  const w = findWidget(id);
  if (!w || w.type === type) return;
  pushUndo();
  w.type = type;
  if (w.query) w.query = { ...w.query, chartType: type };
  renderDashboard(); scheduleAutosave(); updateUndoRedoButtons();
}
function reorderWidget(fromId, toId) {
  const widgets = state.dashboard.widgets;
  const fromIdx = widgets.findIndex(w => w.id === fromId);
  const toIdx = widgets.findIndex(w => w.id === toId);
  if (fromIdx === -1 || toIdx === -1 || fromIdx === toIdx) return;
  pushUndo();
  const [moved] = widgets.splice(fromIdx, 1);
  widgets.splice(toIdx, 0, moved);
  renderDashboard(); scheduleAutosave(); updateUndoRedoButtons();
}

el('edit-toggle').addEventListener('click', e => {
  const btn = e.target.closest('.edit-toggle-btn');
  if (!btn) return;
  state.editMode = btn.dataset.mode === 'edit';
  el('edit-toggle').querySelectorAll('.edit-toggle-btn').forEach(b =>
    b.classList.toggle('active', b.dataset.mode === (state.editMode ? 'edit' : 'view')));
  renderDashboard();
});


// ---------- autosave: updates the same history row instead of creating a new one per edit ----------
let autosaveTimer = null;
function scheduleAutosave() {
  // A Viewer never edits — their applyFilters()-driven live refresh (see openHistoryEntry)
  // must not try to persist anything back, or it'd just fail against Update()'s role check.
  if (!state.currentUser || !state.dashboard?.widgets?.length || state.dashboardRole === 'viewer') return;
  el('autosave-status').textContent = 'جارٍ الحفظ…';
  clearTimeout(autosaveTimer);
  autosaveTimer = setTimeout(doAutosave, 900);
}
async function doAutosave() {
  const d = state.dashboard;
  if (!d) return;
  try {
    if (state.currentHistoryId) {
      const res = await fetch('/api/history/' + state.currentHistoryId, {
        method: 'PUT', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          summary: d.summary, widgets: d.widgets,
          filters: d.filters || [], activeFilters: state.activeFilters || {},
        }),
      });
      if (res.ok) { el('autosave-status').textContent = '✓ تم الحفظ'; return; }
      // A rejected edit (e.g. an Editor's widget introducing a data source the dashboard
      // didn't already use — see HistoryController.Update) is a real validation failure,
      // not a transient error: surface the server's own reason instead of a generic
      // "تعذّر الحفظ" every subsequent keystroke would otherwise repeat silently.
      if (res.status === 400) {
        const payload = await res.json().catch(() => null);
        el('autosave-status').textContent = 'تعذّر الحفظ';
        if (payload?.error) alert(payload.error);
        return;
      }
      if (res.status !== 404) throw new Error();
      state.currentHistoryId = null; // entry was deleted elsewhere — fall through and recreate
      state.dashboardIsActive = false; state.dashboardRole = null;
      state.dashboardDisabled = false; state.dashboardDisabledReason = '';
    }
    const lastQuestion = [...state.messages].reverse().find(m => m.role === 'user')?.text || 'لوحة معلومات';
    const res = await fetch('/api/history', {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        question: lastQuestion, summary: d.summary, queryDescription: d.summary, widgets: d.widgets,
        filters: d.filters || [], activeFilters: state.activeFilters || {},
      }),
    });
    if (!res.ok) throw new Error();
    const saved = await res.json();
    state.currentHistoryId = saved.id;
    state.historyLoaded = false;
    el('autosave-status').textContent = '✓ تم الحفظ';
  } catch {
    el('autosave-status').textContent = 'تعذّر الحفظ';
  }
}

// ---------- add-widget wizard ----------
// A business-friendly, step-by-step alternative to writing a prompt: the user never sees
// a chart-type name or a column-type distinction — just "what to compare" and "what to
// measure" — and every step's options come from the real, permission-filtered schema
// (see /api/widgets/fields), never a hardcoded list. The final step runs the structured
// query through /api/widgets/query — the same deterministic, GPT-free path used for
// "change the period" on an existing widget (openPeriodEditor below).
const WIZARD_KINDS = [
  { kind: 'comparison', icon: '📊', label: 'مقارنة', chartType: 'bar' },
  { kind: 'trend', icon: '📈', label: 'اتجاه عبر الزمن', chartType: 'line' },
  { kind: 'kpi', icon: '🔢', label: 'مؤشر رئيسي', chartType: 'kpi' },
  { kind: 'distribution', icon: '🥧', label: 'توزيع', chartType: 'pie' },
  { kind: 'table', icon: '📋', label: 'جدول', chartType: 'table' },
];
const TIME_RANGE_OPTIONS = [
  ['this_month', 'هذا الشهر'], ['last_month', 'الشهر الماضي'], ['last_3_months', 'آخر 3 أشهر'],
  ['last_6_months', 'آخر 6 أشهر'], ['this_year', 'هذا العام'], ['all', 'كل البيانات'], ['custom', 'مخصص'],
];

function openWizard() {
  state.wizard = { mode: 'add', step: 'kind', stack: [], kind: null, table: null,
    dimension: null, metric: null, dateColumn: null, timeRange: 'all', granularity: 'month',
    columns: [], columnsTouched: false, sort: 'desc', fields: null, gen: 0 };
  el('wizard-next').classList.add('hidden');
  el('wizard-modal').classList.remove('hidden');
  renderWizardStep();
}

// Reopens the wizard scoped to just the period of an already-placed, wizard-created widget
// — the "تغيير الفترة" item in its ⋮ menu. Only offered when the widget carries a `query`
// (see renderWidgetMenuDefault): chat-generated widgets have no structured query to re-run.
function openPeriodEditor(w) {
  state.wizard = { mode: 'edit-period', targetId: w.id, step: 'period', stack: [],
    timeRange: w.query.timeRange || 'all', baseQuery: w.query, gen: 0 };
  el('wizard-next').classList.add('hidden');
  el('wizard-modal').classList.remove('hidden');
  renderWizardStep();
}

function closeWizard() { el('wizard-modal').classList.add('hidden'); state.wizard = null; }
el('wizard-close').addEventListener('click', closeWizard);
el('wizard-modal').addEventListener('click', e => { if (e.target.id === 'wizard-modal') closeWizard(); });

function wizardBack() {
  const w = state.wizard;
  if (!w.stack.length) { closeWizard(); return; }
  w.step = w.stack.pop();
  renderWizardStep();
}
el('wizard-back').addEventListener('click', wizardBack);

// User-driven transition: remembers where we came from so wizardBack() can return to it.
function advance(next) {
  const w = state.wizard;
  w.stack.push(w.step);
  w.step = next;
  renderWizardStep();
}
// Programmatic transition (e.g. an async fetch's result) — not a click, so nothing to undo back to.
function replaceStep(next) { state.wizard.step = next; renderWizardStep(); }

async function loadWizardFields() {
  const w = state.wizard;
  try {
    const res = await fetch('/api/widgets/fields', {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ sources: { systems: [...state.onSystems], files: [...state.onFiles] } }),
    });
    const data = await res.json();
    let tables = data.tables || [];
    tables = w.kind === 'table' ? tables.filter(t => t.allColumns.length) : tables.filter(t => t.metrics.length);
    if (w.kind === 'comparison' || w.kind === 'distribution') tables = tables.filter(t => t.dimensions.length);
    if (w.kind === 'trend') tables = tables.filter(t => t.dateColumns.length > 0);
    w.fields = tables;
    if (!tables.length) { replaceStep('no-data'); return; }
    if (tables.length === 1) { w.table = tables[0]; replaceStep(afterTableStep()); return; }
    replaceStep('table');
  } catch {
    replaceStep('no-data');
  }
}

function afterTableStep() {
  const w = state.wizard;
  if (w.kind === 'comparison' || w.kind === 'distribution') return 'dimension';
  if (w.kind === 'table') return 'columns';
  return 'metric'; // trend | kpi
}

function afterMetricStep() {
  const w = state.wizard;
  w.dateColumn = w.table.dateColumns.length ? w.table.dateColumns[0] : null;
  return (w.kind === 'trend' || w.dateColumn) ? 'period' : 'preview';
}

function buildWizardQuery(w) {
  if (w.mode === 'edit-period')
    return { ...w.baseQuery, timeRange: w.timeRange, customFrom: w.customFrom || null, customTo: w.customTo || null };

  const kindMeta = WIZARD_KINDS.find(k => k.kind === w.kind);
  return {
    table: w.table.table,
    metric: w.metric || (w.table.metrics[0] || ''),
    aggregation: 'sum',
    dimension: (w.kind === 'comparison' || w.kind === 'distribution') ? w.dimension : null,
    dateColumn: w.dateColumn || null,
    timeRange: w.dateColumn ? (w.timeRange || 'all') : 'all',
    customFrom: w.customFrom || null,
    customTo: w.customTo || null,
    timeGranularity: w.kind === 'trend' ? (w.granularity || 'month') : null,
    chartType: kindMeta.chartType,
    columns: w.kind === 'table' ? w.columns : null,
    topN: null,
    sort: 'desc',
  };
}

function confirmWizardResult() {
  const w = state.wizard;
  if (!w.result) return;
  pushUndo();
  if (w.mode === 'edit-period') {
    const target = findWidget(w.targetId);
    if (target) {
      target.type = w.result.type; target.data = w.result.data;
      target.xKey = w.result.xKey; target.yKey = w.result.yKey;
      target.source = w.result.source; target.query = w.result.query;
    }
  } else {
    const widget = {
      id: 'w' + Math.random().toString(36).slice(2, 10),
      type: w.result.type, title: w.result.title, data: w.result.data,
      xKey: w.result.xKey, yKey: w.result.yKey, source: w.result.source, query: w.result.query,
      layout: { size: w.result.type === 'kpi' ? 'small' : (w.result.type === 'table' ? 'full' : 'wide') },
    };
    if (!state.dashboard) state.dashboard = { summary: 'لوحة معلومات', widgets: [] };
    state.dashboard.widgets.push(widget);
  }
  closeWizard();
  renderDashboard();
  scheduleAutosave();
  updateUndoRedoButtons();
}

async function renderWizardStep() {
  const w = state.wizard;
  if (!w) return;
  const body = el('wizard-body');
  el('wizard-back').classList.toggle('hidden', !w.stack.length);
  el('wizard-next').classList.add('hidden');

  if (w.step === 'kind') {
    el('wizard-title').textContent = 'ماذا تريد أن تضيف؟';
    body.innerHTML = `<div class="wizard-grid">${WIZARD_KINDS.map(k =>
      `<button class="wizard-option" data-kind="${k.kind}"><span class="wizard-icon">${k.icon}</span><span>${esc(k.label)}</span></button>`).join('')}</div>`;
    body.querySelectorAll('[data-kind]').forEach(b => b.onclick = () => {
      w.kind = b.dataset.kind;
      advance('loading');
      loadWizardFields();
    });
    return;
  }

  if (w.step === 'loading') {
    el('wizard-title').textContent = 'إضافة عنصر';
    body.innerHTML = `<div class="wizard-loading">جارٍ التحميل…</div>`;
    return;
  }

  if (w.step === 'no-data') {
    el('wizard-title').textContent = 'إضافة عنصر';
    body.innerHTML = `<div class="wizard-empty">لا تتوفر بيانات كافية لإنشاء هذا العنصر.<br>
      فعّل مصدرًا من قائمة «المصادر» أعلى الصفحة أو ارفع ملفًا في مستودع الملفات.</div>`;
    return;
  }

  if (w.step === 'table') {
    el('wizard-title').textContent = 'من أي مصدر بيانات؟';
    body.innerHTML = `<div class="wizard-list">${w.fields.map(f =>
      `<button class="wizard-option wide" data-table="${esc(f.table)}">${esc(f.system || f.file || f.table)}</button>`).join('')}</div>`;
    body.querySelectorAll('[data-table]').forEach(b => b.onclick = () => {
      w.table = w.fields.find(f => f.table === b.dataset.table);
      advance(afterTableStep());
    });
    return;
  }

  if (w.step === 'dimension') {
    el('wizard-title').textContent = w.kind === 'distribution' ? 'توزيع حسب ماذا؟' : 'مقارنة ماذا؟';
    body.innerHTML = `<div class="wizard-list">${w.table.dimensions.map(d =>
      `<button class="wizard-option wide" data-dim="${esc(d)}">${esc(d)}</button>`).join('')}</div>`;
    body.querySelectorAll('[data-dim]').forEach(b => b.onclick = () => { w.dimension = b.dataset.dim; advance('metric'); });
    return;
  }

  if (w.step === 'metric') {
    el('wizard-title').textContent = 'ماذا تريد قياسه؟';
    body.innerHTML = `<div class="wizard-list">${w.table.metrics.map(m =>
      `<button class="wizard-option wide" data-metric="${esc(m)}">${esc(m)}</button>`).join('')}</div>`;
    body.querySelectorAll('[data-metric]').forEach(b => b.onclick = () => { w.metric = b.dataset.metric; advance(afterMetricStep()); });
    return;
  }

  if (w.step === 'period') {
    el('wizard-title').textContent = 'الفترة؟';
    body.innerHTML = `<div class="wizard-list">${TIME_RANGE_OPTIONS.map(([v, l]) =>
      `<button class="wizard-option wide${v === w.timeRange ? ' current' : ''}" data-range="${v}">${l}</button>`).join('')}</div>`;
    body.querySelectorAll('[data-range]').forEach(b => b.onclick = () => {
      w.timeRange = b.dataset.range;
      if (w.timeRange === 'custom') { advance('custom-range'); return; }
      advance(w.mode === 'edit-period' ? 'preview' : (w.kind === 'trend' ? 'granularity' : 'preview'));
    });
    return;
  }

  if (w.step === 'custom-range') {
    el('wizard-title').textContent = 'حدد الفترة';
    body.innerHTML = `
      <label class="wizard-field">من<input type="date" id="wiz-from" value="${esc(w.customFrom || '')}"></label>
      <label class="wizard-field">إلى<input type="date" id="wiz-to" value="${esc(w.customTo || '')}"></label>`;
    el('wizard-next').classList.remove('hidden');
    el('wizard-next').disabled = false;
    el('wizard-next').textContent = 'التالي';
    el('wizard-next').onclick = () => {
      const from = el('wiz-from').value, to = el('wiz-to').value;
      if (!from || !to) return;
      w.customFrom = from; w.customTo = to;
      advance(w.mode === 'edit-period' ? 'preview' : (w.kind === 'trend' ? 'granularity' : 'preview'));
    };
    return;
  }

  if (w.step === 'granularity') {
    el('wizard-title').textContent = 'التجميع؟';
    const opts = [['day', 'يوميًا'], ['week', 'أسبوعيًا'], ['month', 'شهريًا']];
    body.innerHTML = `<div class="wizard-list">${opts.map(([v, l]) =>
      `<button class="wizard-option wide${v === (w.granularity || 'month') ? ' current' : ''}" data-gran="${v}">${l}</button>`).join('')}</div>`;
    body.querySelectorAll('[data-gran]').forEach(b => b.onclick = () => { w.granularity = b.dataset.gran; advance('preview'); });
    return;
  }

  if (w.step === 'columns') {
    el('wizard-title').textContent = 'اختر الأعمدة';
    body.innerHTML = `<div class="wizard-checklist">${w.table.allColumns.map((c, i) =>
      `<label><input type="checkbox" value="${esc(c)}" ${w.columnsTouched ? (w.columns.includes(c) ? 'checked' : '') : (i < 8 ? 'checked' : '')}> ${esc(c)}</label>`).join('')}</div>`;
    el('wizard-next').classList.remove('hidden');
    el('wizard-next').disabled = false;
    el('wizard-next').textContent = 'التالي';
    el('wizard-next').onclick = () => {
      w.columns = [...body.querySelectorAll('input:checked')].map(i => i.value);
      w.columnsTouched = true;
      if (!w.columns.length) return;
      advance('preview');
    };
    return;
  }

  if (w.step === 'preview') {
    const isEdit = w.mode === 'edit-period';
    el('wizard-title').textContent = isEdit ? 'تحديث الفترة' : 'إضافة العنصر';
    body.innerHTML = `<div class="wizard-loading">جارٍ تجهيز المعاينة…</div>`;
    el('wizard-next').classList.remove('hidden');
    el('wizard-next').disabled = true;
    el('wizard-next').textContent = isEdit ? '💾 تحديث' : '➕ إضافة';

    const myGen = ++w.gen;
    try {
      const query = buildWizardQuery(w);
      const res = await fetch('/api/widgets/query', {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ sources: { systems: [...state.onSystems], files: [...state.onFiles] }, query }),
      });
      const payload = await res.json().catch(() => null);
      if (state.wizard !== w || w.gen !== myGen) return; // navigated away while loading
      if (!res.ok || !payload) throw new Error(payload?.error || `تعذّر التنفيذ (${res.status})`);
      w.result = payload;
      body.innerHTML = `<div class="wizard-preview-card">
        <div class="wizard-preview-title">${esc(payload.title)}</div>
        <div class="wizard-preview-meta">${esc(payload.source)}</div>
      </div>`;
      el('wizard-next').disabled = false;
      el('wizard-next').onclick = confirmWizardResult;
    } catch (err) {
      if (state.wizard !== w || w.gen !== myGen) return;
      body.innerHTML = `<div class="wizard-empty">${esc(err.message || 'تعذّر تنفيذ الاستعلام.')}</div>`;
      el('wizard-next').disabled = true;
    }
    return;
  }
}


function emptyBody(card) {
  const p = document.createElement('p');
  p.style.color = 'var(--muted)';
  p.textContent = 'لا يوجد بيانات';
  card.appendChild(p);
  return card;
}

function KpiCard(w) {
  const card = widgetShell(w, 'kpi');
  const e = Array.isArray(w.data) ? w.data[0] : null;
  const v = e?.value ?? Object.values(e || {}).find(x => typeof x === 'number');
  const body = document.createElement('div');
  body.className = 'kpi-body';
  const val = document.createElement('div');
  val.className = 'value';
  if (revealNow && typeof v === 'number' && isFinite(v)) animateKpiValue(val, v);
  else val.textContent = fmt(v);
  body.appendChild(val);
  if (e?.label) {
    const label = document.createElement('div');
    label.className = 'kpi-label';
    label.textContent = e.label;
    body.appendChild(label);
  }
  card.appendChild(body);
  return card;
}

function TableCard(w) {
  const card = widgetShell(w, 'table');
  const rows = Array.isArray(w.data) ? w.data : [];
  if (!rows.length) return emptyBody(card);
  const cols = Object.keys(rows[0]);
  const wrap = document.createElement('div');
  wrap.className = 'tbl-wrap';
  wrap.innerHTML = `<table><thead><tr>${cols.map(c => `<th>${esc(c)}</th>`).join('')}</tr></thead>
    <tbody>${rows.map(r => `<tr>${cols.map(c => {
      const v = r[c], n = typeof v === 'number';
      return `<td class="${n ? 'num' : ''}">${esc(n ? fmt(v) : (v ?? '—'))}</td>`;
    }).join('')}</tr>`).join('')}</tbody></table>`;
  card.appendChild(wrap);
  return card;
}

// Draws the "lollipop" bar decoration: a full-height, pill-shaped, muted background track
// behind each bar (wider than the bar itself — ApexCharts' own backgroundBarColors only
// draws a track the SAME width as the column, so this is plain SVG instead) plus a colored
// circular value badge at the top of the actual filled bar. Purely decorative — the bar's
// real geometry/data/hit area are untouched.
function applyLollipopStyle(svg, values, colors) {
  if (!svg) return;
  svg.querySelectorAll('.bar-lollipop-track,.bar-lollipop-badge').forEach(el => el.remove());
  const svgNS = 'http://www.w3.org/2000/svg';
  const grid = svg.querySelector('.apexcharts-grid');
  if (!grid) return;
  const gridTop = grid.getBBox().y;
  const badgeR = 14;
  svg.querySelectorAll('.apexcharts-bar-area').forEach((bar, i) => {
    const b = bar.getBBox();
    if (b.width <= 0) return;
    const cx = b.x + b.width / 2;
    const baseline = b.y + b.height; // the shared zero-line, same for every bar
    const trackW = b.width * 2.1;
    const trackH = Math.max(baseline - gridTop, 1);
    const track = document.createElementNS(svgNS, 'rect');
    track.setAttribute('class', 'bar-lollipop-track');
    track.setAttribute('x', (cx - trackW / 2).toFixed(1));
    track.setAttribute('y', gridTop.toFixed(1));
    track.setAttribute('width', trackW.toFixed(1));
    track.setAttribute('height', trackH.toFixed(1));
    track.setAttribute('rx', (trackW / 2).toFixed(1));
    track.setAttribute('fill', THEME.gridColor);
    bar.insertAdjacentElement('beforebegin', track);

    const color = colors[i % colors.length];
    const badge = document.createElementNS(svgNS, 'g');
    badge.setAttribute('class', 'bar-lollipop-badge');
    const circle = document.createElementNS(svgNS, 'circle');
    circle.setAttribute('cx', cx.toFixed(1)); circle.setAttribute('cy', b.y.toFixed(1)); circle.setAttribute('r', badgeR);
    circle.setAttribute('fill', color);
    const text = document.createElementNS(svgNS, 'text');
    text.setAttribute('x', cx.toFixed(1)); text.setAttribute('y', (b.y + 1).toFixed(1));
    text.setAttribute('text-anchor', 'middle'); text.setAttribute('dominant-baseline', 'central');
    text.setAttribute('fill', '#fff');
    text.setAttribute('font-size', '10.5'); text.setAttribute('font-weight', '700');
    text.setAttribute('font-family', THEME.fontFamily);
    text.textContent = fmt(values[i]);
    badge.appendChild(circle); badge.appendChild(text);
    bar.insertAdjacentElement('afterend', badge);
  });
}

// One shared config builder — the only place a chart's colors/fonts are set — passed
// to ApexCharts verbatim so its own defaults never leak through. Donut and xy (bar/line)
// options are built as two separate object literals rather than one with ternaries that
// null out xaxis/yaxis/grid for a donut — ApexCharts' internal option merge chokes on an
// explicit `undefined` value for those keys (throws inside its own constructor), so a
// donut's config must omit them entirely instead.
function chartConfig(type, labels, values, title, centerLabel) {
  const isLine = type === 'line';
  const isDonut = type === 'pie'; // rendered as a donut, same choice the old Chart.js config made
  // A plain stroked line, no fill under the curve — matching the visual-system guide's basic
  // line-chart example (ring markers + a value label at every point instead). The two-series
  // "mountain" look from that same guide page is the areaFill extension on the *multi*-series
  // line (buildMultiSeriesChart), not this single-series path.
  const apexType = isDonut ? 'donut' : type;
  const dark = document.documentElement.getAttribute('data-theme') !== 'light';
  const base = {
    chart: {
      type: apexType, height: '100%', width: '100%',
      fontFamily: THEME.fontFamily, foreColor: THEME.tickColor,
      background: 'transparent', toolbar: { show: false },
      // A brief, gently-staggered entrance instead of an instant snap-in. Capped low (and
      // never re-triggered by data updates — widgets are destroyed and rebuilt wholesale on
      // every change, never live-updated) so a dashboard with several widgets still feels
      // responsive rather than sluggish.
      animations: {
        enabled: true, easing: 'easeout', speed: 450,
        animateGradually: { enabled: true, delay: 60 },
        dynamicAnimation: { enabled: false },
      },
      // A soft shadow under the marks themselves — enabledOnSeries scopes it to the bars/
      // line/donut slices so it doesn't also fall on the gridlines or axes.
      dropShadow: { enabled: true, enabledOnSeries: [0], top: 3, left: 0, blur: 4, opacity: dark ? .35 : .14 },
    },
    dataLabels: { enabled: false },
    // Raw values can be long floats (e.g. an AVG() result) — fmt() (already used for KPI
    // values) rounds/abbreviates them ("42.0K") so axis ticks and tooltips stay short instead
    // of overflowing the card with fifteen decimal places.
    tooltip: { theme: dark ? 'dark' : 'light', y: { formatter: fmt } },
    colors: isLine ? [THEME.accent] : THEME.palette,
    // A gentle tonal gradient — lighter toward one end of the same hue, not a second color —
    // for a sense of depth without turning the categorical palette into a magnitude ramp.
    // Only ever visible on bar (a plain "line" chart type never paints a fill regardless of
    // this config, single-series or not).
    fill: {
      type: 'gradient',
      gradient: { shade: 'light', type: 'vertical', shadeIntensity: .35, opacityFrom: 1, opacityTo: .85 },
    },
  };
  if (isDonut) {
    return {
      ...base,
      series: values,
      labels,
      // A side legend with each entry's own share prefixed on it ("51% Lorem ipsum") instead
      // of a bare category name — the guide's donut-with-legend variant. `position: 'right'`
      // matches the reference literally rather than mirroring for RTL (ApexCharts' own
      // legend layout isn't RTL-aware, so 'left' would just move it, not truly mirror it).
      legend: {
        show: true, position: 'right', horizontalAlign: 'center',
        labels: { colors: THEME.tickColor }, fontFamily: THEME.fontFamily,
        formatter: (seriesName, opts) => {
          const val = opts.w.globals.series[opts.seriesIndex];
          const total = opts.w.globals.seriesTotals.reduce((a, b) => a + b, 0) || 1;
          return `${Math.round((val / total) * 100)}% ${seriesName}`;
        },
      },
      // Each slice's own share, printed right on the ring — the guide's other donut variant
      // (51%/45%/4% labelled in place rather than only in a legend). `val` here is already
      // the slice's percentage for a pie/donut series (ApexCharts' own convention), not the
      // raw value, so no separate percentage math is needed the way the legend above does.
      dataLabels: {
        enabled: true,
        formatter: val => Math.round(val) + '%',
        style: { fontSize: '11px', fontWeight: 700, fontFamily: THEME.fontFamily, colors: ['#fff'] },
        dropShadow: { enabled: true, top: 1, left: 1, blur: 1.5, opacity: .45 },
      },
      // The center label: a big bold number with a small muted caption under it — the
      // grand total by default (showAlways), captioned with deriveUnitLabel(title) (what the
      // number counts, e.g. "مشروع"/"خطر" — not the generic "الإجمالي"); a slice's own value
      // + category name while hovering it. Colored and formatted to match a KPI card's own
      // number exactly (THEME.accent, fmt()'s K/M-abbreviated form) rather than a distinct
      // full-precision/neutral-color treatment, so the same figure reads consistently
      // wherever it appears on a dashboard. The caption keeps the muted tickColor used for
      // everything else secondary in the UI. Font sizes and vertical offsets (ApexCharts
      // draws the caption *above* the number by default — flipped here to match the
      // requested look) are set per-widget in the pendingCharts render loop below,
      // proportional to the donut's actual rendered size, not a fixed px.
      plotOptions: {
        pie: {
          donut: {
            size: '72%',
            labels: {
              show: true,
              name: {
                show: true, fontFamily: THEME.fontFamily, fontWeight: 400,
                color: THEME.tickColor,
              },
              value: {
                show: true, fontFamily: THEME.fontFamily, fontWeight: 700,
                color: THEME.accent,
                formatter: v => fmt(v),
              },
              total: {
                show: true, showAlways: true, label: centerLabel || deriveUnitLabel(title),
                fontFamily: THEME.fontFamily, fontWeight: 400,
                color: THEME.tickColor,
                formatter: w => fmt(w.globals.seriesTotals.reduce((a, b) => a + b, 0)),
              },
            },
          },
        },
      },
    };
  }
  return {
    ...base,
    series: [{ name: title, data: values }],
    xaxis: {
      categories: labels,
      labels: { style: { colors: THEME.tickColor, fontFamily: THEME.fontFamily, fontSize: '11px' } },
      axisBorder: { color: THEME.gridColor }, axisTicks: { color: THEME.gridColor },
    },
    yaxis: { labels: { style: { colors: THEME.tickColor }, formatter: fmt } },
    // Extra headroom above the plot for bar charts only — otherwise the lollipop value
    // badge on the tallest bar (drawn above the bar's own top edge, see applyLollipopStyle)
    // gets clipped by the SVG's own viewBox with nothing reserved for it.
    // Extra headroom for line too — the per-point value labels (dataLabels below) sit right
    // above their marker, same clipping risk the bar badge has.
    grid: { borderColor: THEME.gridColor, strokeDashArray: 0, padding: isLine || apexType === 'bar' ? { top: 24 } : {} },
    stroke: isLine ? { curve: 'smooth', width: 3 } : { width: 0 },
    // A plain single-series line's marker at each point cycles through the brand palette —
    // "hollow ring" style (fillColor is the card's own surface color, so it reads as an
    // outlined circle rather than a solid dot) plus a value label above every point, matching
    // the visual-system guide's basic line-chart example. The connecting stroke itself stays
    // one consistent color (colors: [THEME.accent] above) rather than also shifting per
    // segment — cycling the *line's* own color would clash with how color is used to mean
    // "this series" everywhere else in the app (multi-series/bar/pie all key legend meaning
    // off of color), where here it would just be decorative and could read as unintentional.
    markers: isLine ? {
      size: 6, strokeWidth: 2, hover: { sizeOffset: 2 },
      discrete: values.map((_, i) => ({
        seriesIndex: 0, dataPointIndex: i,
        fillColor: THEME.surface, strokeColor: THEME.palette[i % THEME.palette.length], size: 6,
      })),
    } : {},
    dataLabels: isLine ? {
      enabled: true, formatter: fmt, offsetY: -14,
      style: { fontSize: '11px', fontWeight: 700, fontFamily: THEME.fontFamily, colors: [THEME.ink] },
      background: { enabled: false },
    } : { enabled: false },
    // "Lollipop" bars — narrow pill-shaped columns, one brand color per category
    // (distributed:true) instead of one color for the whole series — matching the visual-
    // system guide's basic bar-chart example. The wider rounded background "track" behind
    // each bar and the circular value badge at its top are NOT an ApexCharts option (its own
    // backgroundBarColors draws a track the SAME width as the column, not a visibly wider
    // one) — both are drawn as plain SVG after render instead, see applyLollipopStyle below;
    // plotOptions.bar.distributed here is exactly what that render step keys off of to know
    // this is the single-series chart it should decorate, vs. a grouped/stacked multi-series
    // bar built by buildMultiSeriesChart, which never sets it.
    plotOptions: apexType === 'bar'
      ? { bar: { borderRadius: 9, borderRadiusApplication: 'around', columnWidth: '30%', distributed: true } }
      : {},
    legend: { show: false },
  };
}

// ---------- click-a-chart-to-filter (Power BI-style cross-filtering) ----------
// Clicking a bar/line point or a pie slice filters the WHOLE dashboard by that category,
// through the exact same machinery a declared dashboard filter already uses
// (state.activeFilters/applyFilters) — see getActiveFilters(). An ad-hoc filter definition
// is created on the fly (state.crossFilterDefs) when no declared DashboardFilter already
// covers that table+field, so the filter bar shows a small chip for it either way.
//
// Only ever wired up when a real table+column can be resolved for the widget — a wizard
// widget's structured query.dimension, or a chat-authored one's xKey (set for bar/line,
// but NOT for pie: the system prompt has the model shape pie data as fixed {label,value}
// keys, discarding the real column name, so a chat-authored pie widget simply isn't
// clickable — same "not filterable" outcome buildWidget() already shows via
// "غير متأثر بالفلتر" for a widget with no query lineage at all).
function resolveCrossFilterField(w) {
  const table = w.query?.table;
  const field = w.query?.dimension || w.xKey;
  return (table && field) ? { table, field } : null;
}

function crossFilterId(table, field) { return 'xfilter:' + table + '::' + field; }

// The "+N أخرى" overflow bucket (see capRows/capMultiSeriesRows) isn't a real value to
// filter by — it's several rows folded together, so clicking it is a no-op.
function isOverflowBucketLabel(label) { return typeof label === 'string' && /^\+\d+ .+أخرى$/.test(label); }

function toggleCrossFilter(table, field, value, label) {
  if (state.shareId || isOverflowBucketLabel(value)) return; // a frozen share has nothing to re-run
  const id = crossFilterId(table, field);
  const current = state.activeFilters[id] || [];
  pushUndo();
  if (current.length === 1 && current[0] === value) {
    // Clicking the same point again clears it — the same toggle Power BI's own
    // cross-filter selection uses instead of a separate "clear" click.
    delete state.activeFilters[id];
    delete state.crossFilterDefs[id];
  } else {
    state.crossFilterDefs[id] = { id, label, field, table, type: 'single_select', options: [] };
    state.activeFilters[id] = [String(value)];
  }
  applyFilters();
}

// Wires a chart's own ApexCharts "dataPointSelection" event (fired on a bar/point/slice
// click) into toggleCrossFilter, mapping the clicked index back to that category's real
// value via the exact `labels` array the chart itself was built from. Mutates `options`
// in place — call before `new ApexCharts(holder, options)`.
function attachCrossFilterClick(options, holder, w, labels) {
  if (state.shareId) return;
  const target = resolveCrossFilterField(w);
  if (!target || !Array.isArray(labels) || !labels.length) return;
  holder.classList.add('chart-clickable');
  options.chart.events = {
    ...(options.chart.events || {}),
    dataPointSelection: (event, chartContext, config) => {
      const idx = config?.dataPointIndex;
      if (idx == null || idx < 0 || idx >= labels.length) return;
      // The chip names the *field* being filtered (e.g. "Region: North"), not the widget's
      // own title (typically the metric, e.g. "Revenue") — "Revenue: North" would misread
      // as filtering by a revenue value rather than by the category actually clicked.
      toggleCrossFilter(target.table, target.field, labels[idx], target.field);
    },
  };
}

function mountChart(card, type, labels, values, title, truncated, centerLabel, w) {
  const holder = document.createElement('div');
  holder.className = 'chart-holder';
  card.appendChild(holder);
  if (truncated > 0) {
    const note = document.createElement('div');
    note.className = 'chart-note';
    note.textContent = `+${truncated} نقطة إضافية مجمّعة ضمن الرسم`;
    card.appendChild(note);
  }
  pendingCharts.push({ card, holder, options: chartConfig(type, labels, values, title, centerLabel), labels, widget: w });
}

// Builds a mixed-series ApexCharts config for a bar/line widget extended with a forecast.
// The real historical marks render exactly as chartConfig() would; the forecast is ALWAYS a
// dashed line in a distinct color (never the historical series' own color, never solid) with
// a shaded confidence-interval band behind it and a "متوقّع" legend entry — the mandatory
// visual separation the forecasting feature requires, regardless of whether the base widget
// is a bar or a line chart.
function forecastChartConfig(type, labels, values, forecast, title) {
  const isLine = type === 'line';
  const dark = document.documentElement.getAttribute('data-theme') !== 'light';
  const histLen = values.length;
  const fLen = forecast.values.length;
  const categories = labels.concat(forecast.labels);

  // Padded with nulls over the forecast span so it doesn't extend past the real data.
  const historicalData = values.concat(new Array(fLen).fill(null));
  // Each of forecast/upper/lower starts at the last historical point (so its dashed line
  // visually continues from the real data rather than floating disconnected), null before that.
  const bridge = new Array(Math.max(0, histLen - 1)).fill(null).concat([values[histLen - 1]]);
  const forecastData = bridge.concat(forecast.values);
  const upperData = bridge.concat(forecast.upper);
  const lowerData = bridge.concat(forecast.lower);

  // The y-axis is scaled to the *real* points (historical + forecast), not the confidence
  // bounds — a short/noisy series can produce a bound so wide (correctly — that's the point of
  // flagging low confidence) that letting it drive the axis squashes the actual trend into an
  // unreadable sliver. The bound lines simply run off the top/bottom of the chart when they
  // exceed this range, which itself reads as "very uncertain" rather than hiding that fact.
  const realPoints = values.concat(forecast.values);
  const dataMin = Math.min(...realPoints), dataMax = Math.max(...realPoints);
  const padding = Math.max((dataMax - dataMin) * 0.3, Math.abs(dataMax) * 0.1, 1);

  // A shaded band would be the more standard way to show this, but ApexCharts' rangeArea
  // series type (verified against this exact vendored version) simply doesn't render when
  // mixed into a combo chart with a bar/line — it silently produces an empty series, tested
  // both against a bar-based and a line-based combo. Two dashed bound lines, fainter/thinner
  // than the forecast line itself, convey the same thing (a range, not a single confident
  // number, visibly wider with sparse history) without relying on a combination this chart
  // library doesn't actually support yet.
  const forecastColor = THEME.palette[1]; // amber — deliberately not the historical series' color
  return {
    __isForecast: true, // opts this chart out of the 3D bar-extrusion pass (see renderDashboard)
    chart: {
      type: isLine ? 'line' : 'bar', height: '100%', width: '100%',
      fontFamily: THEME.fontFamily, foreColor: THEME.tickColor, background: 'transparent',
      toolbar: { show: false },
      animations: { enabled: true, easing: 'easeout', speed: 450, animateGradually: { enabled: true, delay: 60 } },
    },
    series: [
      { name: title, type: isLine ? 'line' : 'bar', data: historicalData },
      { name: 'الحد الأعلى للتوقع', type: 'line', data: upperData },
      { name: 'الحد الأدنى للتوقع', type: 'line', data: lowerData },
      { name: 'متوقّع', type: 'line', data: forecastData },
    ],
    colors: [THEME.palette[0], forecastColor, forecastColor, forecastColor],
    fill: { opacity: [1, 1, 1, 1] },
    stroke: { curve: 'smooth', width: [isLine ? 2 : 0, 1.5, 1.5, 3], dashArray: [0, 3, 3, 6] },
    markers: { size: 0 },
    plotOptions: isLine ? {} : { bar: { borderRadius: 8, columnWidth: '55%' } },
    dataLabels: { enabled: false },
    xaxis: {
      categories,
      labels: { style: { colors: THEME.tickColor, fontFamily: THEME.fontFamily, fontSize: '11px' } },
      axisBorder: { color: THEME.gridColor }, axisTicks: { color: THEME.gridColor },
    },
    yaxis: { min: dataMin - padding, max: dataMax + padding, labels: { style: { colors: THEME.tickColor }, formatter: fmt } },
    grid: { borderColor: THEME.gridColor, strokeDashArray: 0 },
    legend: { show: true, labels: { colors: THEME.tickColor }, fontFamily: THEME.fontFamily },
    tooltip: { theme: dark ? 'dark' : 'light', y: { formatter: fmt } },
  };
}

function mountForecastChart(card, type, labels, values, forecast, title, truncated) {
  const holder = document.createElement('div');
  holder.className = 'chart-holder chart-holder-forecast';
  card.appendChild(holder);
  if (truncated > 0) {
    const note = document.createElement('div');
    note.className = 'chart-note';
    note.textContent = `+${truncated} نقطة إضافية مجمّعة ضمن الرسم`;
    card.appendChild(note);
  }
  const methodLabel = forecast.method === 'linear_regression_seasonal' ? 'انحدار خطي مع تعديل موسمي' : 'انحدار خطي';
  const noteEl = document.createElement('div');
  noteEl.className = 'forecast-note';
  noteEl.textContent = `🔮 توقّع إحصائي (${methodLabel}${typeof forecast.r2 === 'number' ? `، R²=${forecast.r2}` : ''})` +
    (forecast.note ? ` — ${forecast.note}` : '');
  card.appendChild(noteEl);
  pendingCharts.push({ card, holder, options: forecastChartConfig(type, labels, values, forecast, title) });
}

function buildXyChart(w, type) {
  const card = widgetShell(w, 'chart');
  // Extension (multi-series bar/line): w.series is [{key, label}] naming which data-row
  // fields to plot as separate series sharing one xKey category axis — a different shape
  // from the single-series xKey/yKey path below, so it's handled by its own function
  // rather than threading extra branches through the existing one.
  if (Array.isArray(w.series) && w.series.length) return buildMultiSeriesChart(card, w, type);
  const { rows: allRows, xKey, yKey } = keysFor(w);
  if (!allRows.length) return emptyBody(card);
  const { rows, truncated } = capRows(allRows, xKey, yKey);
  const labels = rows.map(r => r[xKey]);
  const values = rows.map(r => num(r[yKey]));
  if (w.forecast) mountForecastChart(card, type, labels, values, w.forecast, w.title, truncated);
  else mountChart(card, type, labels, values, w.title, truncated, undefined, w);
  return card;
}
const BarChartCard = w => buildXyChart(w, 'bar');
const LineChartCard = w => buildXyChart(w, 'line');

// Caps a multi-series row set the same way capRows() does for a single series — folding
// the overflow into one "+N أخرى" bucket, just summed across every series field at once.
function capMultiSeriesRows(rows, xKey, seriesKeys) {
  if (rows.length <= MAX_CHART_POINTS) return { rows, truncated: 0 };
  const kept = rows.slice(0, MAX_CHART_POINTS - 1);
  const rest = rows.slice(MAX_CHART_POINTS - 1);
  const bucket = { [xKey]: `+${rest.length} أخرى` };
  seriesKeys.forEach(k => { bucket[k] = rest.reduce((s, r) => s + num(r[k]), 0); });
  return { rows: [...kept, bucket], truncated: rest.length };
}

// Multi-series bar (grouped or stacked, per w.stacked) / line — a shared axis with one
// series per w.series entry. Colors always come from THEME.palette by series index, never
// a model-supplied value, for the same reason every other chart in the app does this: one
// consistent palette across the whole dashboard, no risk of a clashing/invented color.
// Data labels are always on (unlike the single-series path) since these charts get
// exported to PDF/PPTX, where hover-only tooltips don't exist.
function buildMultiSeriesChart(card, w, type) {
  const rows = Array.isArray(w.data) ? w.data : [];
  if (!rows.length) return emptyBody(card);
  const xKey = w.xKey || Object.keys(rows[0]).find(k => typeof rows[0][k] !== 'number') || Object.keys(rows[0])[0];
  const seriesKeys = w.series.map(s => s.key);
  const { rows: capped, truncated } = capMultiSeriesRows(rows, xKey, seriesKeys);
  const categories = capped.map(r => r[xKey]);
  const dark = document.documentElement.getAttribute('data-theme') !== 'light';
  const isLine = type === 'line';
  const stacked = !isLine && !!w.stacked;
  // areaFill (see AnalyticsTools.cs) — overlapping translucent gradient fills under each
  // series ("mountain" style) instead of plain crossing lines. line-only; bar ignores it.
  const areaFill = isLine && !!w.areaFill;
  const apexSeries = w.series.map(s => ({ name: s.label || s.key, data: capped.map(r => num(r[s.key])) }));

  const holder = document.createElement('div');
  holder.className = 'chart-holder';
  card.appendChild(holder);
  if (truncated > 0) {
    const note = document.createElement('div');
    note.className = 'chart-note';
    note.textContent = `+${truncated} نقطة إضافية مجمّعة ضمن الرسم`;
    card.appendChild(note);
  }

  const options = {
    chart: {
      type: areaFill ? 'area' : (isLine ? 'line' : 'bar'), height: '100%', width: '100%', stacked,
      fontFamily: THEME.fontFamily, foreColor: THEME.tickColor, background: 'transparent',
      toolbar: { show: false },
      animations: { enabled: true, easing: 'easeout', speed: 450, animateGradually: { enabled: true, delay: 60 } },
    },
    series: apexSeries,
    colors: THEME.palette.slice(0, apexSeries.length),
    // areaFill's overlapping mountains get cluttered fast with a value label on every
    // point of every series — same reason the single-series area chart never has them.
    dataLabels: areaFill ? { enabled: false } : {
      enabled: true, formatter: fmt,
      style: { fontSize: '10px', fontFamily: THEME.fontFamily, colors: [THEME.ink] },
      background: { enabled: false },
      offsetY: isLine ? -8 : 0,
    },
    stroke: isLine ? { curve: 'smooth', width: 2 } : { width: 0 },
    markers: areaFill ? { size: 0 } : (isLine ? { size: 4 } : {}),
    plotOptions: isLine ? {} : { bar: { borderRadius: 4, columnWidth: stacked ? '55%' : '70%' } },
    // Same gradient recipe as the single-series area chart in chartConfig() — a soft fade
    // toward transparent rather than a flat translucent fill, so overlapping series still
    // read as layered "mountains" instead of one another's fills flattening into mud.
    ...(areaFill ? { fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: .5, opacityTo: .05 } } } : {}),
    xaxis: {
      categories,
      labels: { style: { colors: THEME.tickColor, fontFamily: THEME.fontFamily, fontSize: '11px' } },
      axisBorder: { color: THEME.gridColor }, axisTicks: { color: THEME.gridColor },
    },
    yaxis: { labels: { style: { colors: THEME.tickColor }, formatter: fmt } },
    grid: { borderColor: THEME.gridColor, strokeDashArray: 0 },
    legend: { show: true, position: 'bottom', labels: { colors: THEME.tickColor }, fontFamily: THEME.fontFamily },
    tooltip: { theme: dark ? 'dark' : 'light', y: { formatter: fmt } },
  };
  pendingCharts.push({ card, holder, options, labels: categories, widget: w });
  return card;
}

function PieChartCard(w) {
  const card = widgetShell(w, 'chart');
  const allRows = (Array.isArray(w.data) ? w.data : []).map(r => ({
    label: r.label ?? Object.values(r).find(v => typeof v === 'string') ?? '—',
    value: num(r.value ?? Object.values(r).find(v => typeof v === 'number')),
  })).filter(r => r.value > 0);
  if (!allRows.length) return emptyBody(card);
  const { rows, truncated } = capRows(allRows, 'label', 'value');
  mountChart(card, 'pie', rows.map(r => r.label), rows.map(r => r.value), w.title, truncated, w.centerLabel, w);
  return card;
}

// A segment's `color` is one of a small fixed vocabulary — never an arbitrary hex the model
// invents — mapped to the exact same tokens every other severity/status indicator in the
// app already uses. "muted" (the default for an unrecognized/absent value) renders as a
// translucent neutral fill, the rest as solid theme colors.
function toneColor(tone) {
  if (tone === 'accent') return THEME.accent;
  if (tone === 'danger') return THEME.danger;
  if (tone === 'amber') return THEME.amber;
  return 'color-mix(in srgb, ' + THEME.tickColor + ' 35%, transparent)';
}

// ---------- reshaping generic data for the 5 bespoke widget types ----------
// progress-table/trend-matrix/status-bar/radial-gauge/linear-gauge each have their own data
// shape a widget built as bar/line/pie/table/kpi won't carry — switching the type of such a
// widget via "🔀 تغيير نوع الرسم" used to just show that type's own empty state (reported
// live as effectively a dead end: the card goes blank, and since w.data itself was never
// touched by setWidgetType, switching back "should" have worked too — but a user hitting a
// blank card with no visible explanation reasonably reads that as broken, not as "pick a
// different type"). Each Card below now tries its own native shape first and, if w.data
// doesn't look like it, derives a genuinely meaningful equivalent from the SAME generic
// {rows, xKey, yKey} every bar/line/pie/table/kpi widget already carries (see keysFor) —
// never mutating w.data itself, so switching to any other type afterward — including back
// to the original — always still works from the same untouched source.
function reshapeGenericForBespokeType(type, w) {
  const { rows, xKey, yKey } = keysFor(w);
  if (!rows.length) return null;
  const values = rows.map(r => num(r[yKey]));
  const total = values.reduce((s, v) => s + v, 0);
  switch (type) {
    case 'status-bar':
      // Every row becomes one status badge, "ontrack" by default — a generic {label,value}
      // row carries no severity of its own to map to severe/warning/ontrack.
      return {
        label: w.title || '',
        total,
        statuses: rows.map((r, i) => ({ label: String(r[xKey] ?? ''), count: values[i], severity: 'ontrack' })),
      };
    case 'radial-gauge': {
      // The single largest row as the ring's value, against the sum of every row as its max
      // — "this category's share of the total", e.g. the biggest region's % of all revenue.
      let maxIdx = 0;
      values.forEach((v, i) => { if (v > values[maxIdx]) maxIdx = i; });
      return { value: values[maxIdx] || 0, max: total || values[maxIdx] || 1, label: String(rows[maxIdx][xKey] ?? '') };
    }
    case 'linear-gauge': {
      // Every row as its own bar, all sharing the largest row's value as their common max —
      // directly analogous to the bar chart this data would otherwise be, just as gauges.
      const max = Math.max(...values, 1);
      return rows.map((r, i) => ({ label: String(r[xKey] ?? ''), value: values[i], max, tone: 'accent' }));
    }
    case 'progress-table':
      // Every row as its own single-segment progress bar, scaled against the largest row's
      // value — there's no second number in generic {label,value} data to pair as a real
      // target, so this is "how this row compares to the top one", not a spent/target ratio.
      return rows.map((r, i) => ({
        label: String(r[xKey] ?? ''), total: Math.max(...values, 1),
        segments: [{ label: String(r[xKey] ?? ''), value: values[i], color: 'accent' }],
      }));
    case 'trend-matrix':
      // The whole widget becomes one metric row, with every generic row as one of its
      // periods — turns exactly the same data a line/bar chart would plot into a single-row
      // trend view (sparkline + delta) instead.
      return [{ metric: w.title || yKey || '', periods: rows.map((r, i) => ({ label: String(r[xKey] ?? ''), value: values[i] })) }];
    default:
      return null;
  }
}

// ---------- new widget type: progress-table ----------
// Each row's bar is scaled to that row's own `total` (its segments' values need not sum to
// it — e.g. "spent" vs. a target — so the bar can visibly under- or over-fill).
function ProgressTableCard(w) {
  const card = widgetShell(w, 'progress-table');
  let rows = Array.isArray(w.data) ? w.data : [];
  if (!rows.some(r => r && (Array.isArray(r.segments) || typeof r.total === 'number')))
    rows = reshapeGenericForBespokeType('progress-table', w) || rows;
  if (!rows.length) return emptyBody(card);
  const wrap = document.createElement('div');
  wrap.className = 'tbl-wrap';
  wrap.innerHTML = `<table><thead><tr><th>${esc(w.labelHeader || '')}</th><th>${esc(w.progressHeader || 'التقدّم')}</th></tr></thead>
    <tbody>${rows.map(r => {
      const total = num(r.total);
      const segs = Array.isArray(r.segments) ? r.segments : [];
      const segsHtml = segs.map(s => {
        const value = num(s.value);
        const pct = total > 0 ? Math.max(0, Math.min(100, (value / total) * 100)) : 0;
        const tone = s.color || 'muted';
        return `<div class="prog-seg${tone === 'muted' ? ' tone-muted' : ''}" style="width:${pct}%;background:${toneColor(tone)}"
          title="${esc(s.label || '')}: ${esc(fmt(value))}">${pct >= 8 ? esc(fmt(value)) : ''}</div>`;
      }).join('');
      return `<tr><td>${esc(r.label ?? '—')}</td><td class="prog-cell">
          <div class="prog-track">${segsHtml}</div>
          <div class="prog-total">${esc(fmt(total))}</div>
        </td></tr>`;
    }).join('')}</tbody></table>`;
  card.appendChild(wrap);
  return card;
}

// ---------- new widget type: trend-matrix ----------
// Trend direction/delta/sparkline are all computed here from the row's own period values —
// never model-specified — comparing strictly the last two points, per the design spec.
function TrendMatrixCard(w) {
  const card = widgetShell(w, 'trend-matrix');
  let rows = Array.isArray(w.data) ? w.data : [];
  if (!rows.some(r => r && Array.isArray(r.periods)))
    rows = reshapeGenericForBespokeType('trend-matrix', w) || rows;
  if (!rows.length) return emptyBody(card);
  const periodLabels = (rows[0].periods || []).map(p => p.label);
  const wrap = document.createElement('div');
  wrap.className = 'tbl-wrap';
  wrap.innerHTML = `<table><thead><tr>
      <th>${esc(w.metricHeader || 'المؤشر')}</th>
      ${periodLabels.map(l => `<th>${esc(l)}</th>`).join('')}
      <th>${esc(w.trendHeader || 'الاتجاه')}</th>
    </tr></thead>
    <tbody>${rows.map(r => {
      const periods = Array.isArray(r.periods) ? r.periods : [];
      const values = periods.map(p => num(p.value));
      const cells = periods.map(p => `<td class="num">${esc(fmt(num(p.value)))}</td>`).join('');
      const last = values[values.length - 1], prev = values[values.length - 2];
      const dir = (last === undefined || prev === undefined) ? 'flat' : last > prev ? 'up' : last < prev ? 'down' : 'flat';
      const delta = (last !== undefined && prev !== undefined) ? last - prev : null;
      const deltaText = delta === null ? '—' : (delta >= 0 ? '+' : '') + fmt(delta);
      const min = Math.min(...values, 0), max = Math.max(...values, 1);
      const spark = values.map(v => {
        const h = max > min ? Math.round(4 + ((v - min) / (max - min)) * 12) : 8;
        return `<span class="spark-bar" style="height:${h}px"></span>`;
      }).join('');
      return `<tr><td>${esc(r.metric ?? '—')}</td>${cells}
          <td><div class="trend-cell">
            <span class="trend-dot ${dir}"></span><span class="trend-delta">${deltaText}</span>
            <span class="sparkline">${spark}</span>
          </div></td></tr>`;
    }).join('')}</tbody></table>`;
  card.appendChild(wrap);
  return card;
}

// ---------- new widget type: status-bar ----------
// One total+label+breakdown per widget instance (per the schema) — the model sends several
// of these as separate widgets if it wants more than one breakdown on a dashboard.
function StatusBarCard(w) {
  const card = widgetShell(w, 'status-bar');
  let row = Array.isArray(w.data) ? w.data[0] : w.data;
  if (!row || !(Array.isArray(row.statuses) || typeof row.total === 'number'))
    row = reshapeGenericForBespokeType('status-bar', w) || row;
  if (!row) return emptyBody(card);
  const statuses = Array.isArray(row.statuses) ? row.statuses : [];
  const wrap = document.createElement('div');
  wrap.className = 'status-bar-row';
  const sevClass = s => (s === 'severe' || s === 'warning' || s === 'ontrack') ? s : 'ontrack';
  const sevIcon = { severe: '🔴', warning: '🟡', ontrack: '🟢' };
  wrap.innerHTML = `
    <div class="status-bar-num">${esc(fmt(num(row.total)))}</div>
    <div>
      ${row.label ? `<div class="status-bar-label">${esc(row.label)}</div>` : ''}
      <div class="status-badges">
        ${statuses.map(s => `<span class="status-badge ${sevClass(s.severity)}">
            ${sevIcon[sevClass(s.severity)]} ${esc(s.label)}: ${esc(fmt(num(s.count)))}</span>`).join('')}
      </div>
    </div>`;
  card.appendChild(wrap);
  return card;
}

// ---------- new widget type: linear-gauge ----------
// One continuous track + a round handle marker per row — deliberately NOT progress-table's
// several discrete colored segments in one track: this is a single value/max measure per
// row (e.g. each team's own % of its own target), not a composition breakdown.
function LinearGaugeCard(w) {
  const card = widgetShell(w, 'linear-gauge');
  let rows = Array.isArray(w.data) ? w.data : [];
  if (!rows.some(r => r && typeof r.value === 'number'))
    rows = reshapeGenericForBespokeType('linear-gauge', w) || rows;
  if (!rows.length) return emptyBody(card);
  const wrap = document.createElement('div');
  wrap.className = 'tbl-wrap';
  wrap.innerHTML = `<table><tbody>${rows.map(r => {
    const value = num(r.value);
    const max = r.max != null ? num(r.max) : 100;
    const pct = max > 0 ? Math.max(0, Math.min(100, (value / max) * 100)) : 0;
    const color = toneColor(r.tone || 'accent');
    const valueLabel = (r.max == null) ? `${fmt(value)}٪` : `${fmt(value)} / ${fmt(max)}`;
    return `<tr><td>${esc(r.label ?? '—')}</td><td class="lg-cell">
        <div class="lg-track"><div class="lg-fill" style="width:${pct}%;background:${color}">
          <span class="lg-handle" style="border-color:${color}"></span>
        </div></div>
        <div class="lg-value">${esc(valueLabel)}</div>
      </td></tr>`;
  }).join('')}</tbody></table>`;
  card.appendChild(wrap);
  return card;
}

// ---------- new widget type: radial-gauge ----------
// A single value/max ring — data is one object (like status-bar), never an array. The
// percentage, the arc, and the center number are always computed/drawn here — the model
// only ever supplies the raw value/max/label, same "never model-supplied" rule as
// trend-matrix's trend arrow.
function radialGaugeConfig(value, max, tone) {
  const pct = max > 0 ? Math.max(0, Math.min(100, (value / max) * 100)) : 0;
  return {
    chart: {
      type: 'radialBar', height: '100%', width: '100%', fontFamily: THEME.fontFamily,
      background: 'transparent',
      animations: { enabled: true, easing: 'easeout', speed: 450 },
    },
    series: [pct],
    colors: [toneColor(tone || 'accent')],
    plotOptions: {
      radialBar: {
        hollow: { size: '62%' },
        track: { background: THEME.gridColor, strokeWidth: '100%' },
        dataLabels: {
          name: { show: false },
          value: {
            show: true, fontSize: '28px', fontWeight: 700, fontFamily: THEME.fontFamily,
            color: THEME.ink, offsetY: 10, formatter: () => Math.round(pct) + '٪',
          },
        },
      },
    },
    stroke: { lineCap: 'round' },
  };
}
function RadialGaugeCard(w) {
  const card = widgetShell(w, 'radial-gauge');
  let row = Array.isArray(w.data) ? w.data[0] : w.data;
  if (!row || row.value == null) row = reshapeGenericForBespokeType('radial-gauge', w) || row;
  if (!row || row.value == null) return emptyBody(card);
  const value = num(row.value);
  const max = row.max != null ? num(row.max) : 100;
  const holder = document.createElement('div');
  holder.className = 'chart-holder';
  card.appendChild(holder);
  if (row.label) {
    const caption = document.createElement('div');
    caption.className = 'rg-caption';
    caption.textContent = row.label;
    card.appendChild(caption);
  }
  // Never cross-filterable (see attachCrossFilterClick's own guard on `widget` being
  // present in pendingCharts) — a single aggregate measure has no per-category dimension
  // to filter the rest of the dashboard by, the same reason a forecast chart isn't either.
  pendingCharts.push({ card, holder, options: radialGaugeConfig(value, max, row.tone) });
  return card;
}

const WIDGET_COMPONENTS = {
  kpi: KpiCard, bar: BarChartCard, line: LineChartCard, pie: PieChartCard, table: TableCard,
  'progress-table': ProgressTableCard, 'trend-matrix': TrendMatrixCard, 'status-bar': StatusBarCard,
  'radial-gauge': RadialGaugeCard, 'linear-gauge': LinearGaugeCard,
};

// ---------- comparison layout (not a 6th type — a display option any type can carry) ----------
// { comparison: { left: {...partial widget}, right: {...partial widget} } } on an otherwise
// normal widget — reuses whichever component `type` already is for both halves (each a
// fully real, independent widget with its own title/source/etc.), just laid out side by
// side with a connecting arrow instead of stacked in the grid like every other widget.
function ComparisonCard(raw) {
  const type = (raw.type || '').toLowerCase();
  if (!WIDGET_TYPES.includes(type)) {
    console.warn('Unknown comparison type — rendering its left side as a table instead:', raw);
    return TableCard({ ...raw.comparison.left, type: 'table' });
  }
  const wrap = document.createElement('div');
  wrap.className = 'widget comparison-wrap';
  wrap.dataset.size = (raw.layout && raw.layout.size) || 'full';
  if (raw.id) wrap.dataset.widgetId = raw.id;

  // Same drag/resize/delete affordances every other top-level widget gets in edit mode —
  // the comparison as a whole is one entry in state.dashboard.widgets, so the existing
  // generic helpers (widgetMenu, enableWidgetDragDrop, delete/duplicate/setWidgetSize) all
  // work on it unchanged; it just never had a head to host the menu button before.
  if (raw.title || (state.editMode && raw.id)) {
    const head = document.createElement('div');
    head.className = 'widget-head';
    const h = document.createElement('h3');
    h.className = 'comparison-title';
    h.textContent = raw.title || '';
    head.appendChild(h);
    wrap.appendChild(head);
    if (state.editMode && raw.id) {
      const { btn: menuBtn, menu } = widgetMenu(raw);
      head.appendChild(menuBtn);
      wrap.appendChild(menu);
    }
  }
  const row = document.createElement('div');
  row.className = 'compare-row';
  // Each side is built through the exact same buildWidget() -> WIDGET_COMPONENTS path as
  // any other widget, just with its own id/layout stripped — it's a nested, non-editable
  // mini-widget, not an independent grid item the drag/resize/delete menu should touch.
  const buildSide = def => buildWidget({
    ...def, type, id: undefined, layout: { size: 'wide' },
    // A pie's own hollow center is where its side's caption reads best (matching the
    // reference mockup) — defaults to this side's title, e.g. "الربع الأول", unless the
    // side already specifies its own centerLabel.
    ...(type === 'pie' && !def.centerLabel ? { centerLabel: def.title } : {}),
  });
  row.appendChild(buildSide(raw.comparison.left || {}));
  const arrow = document.createElement('div');
  arrow.className = 'compare-arrow';
  arrow.textContent = '⟷';
  row.appendChild(arrow);
  row.appendChild(buildSide(raw.comparison.right || {}));
  wrap.appendChild(row);
  if (state.editMode && raw.id) enableWidgetDragDrop(wrap, raw);
  return wrap;
}

function buildWidget(raw) {
  if (raw.comparison && typeof raw.comparison === 'object') return ComparisonCard(raw);
  const type = (raw.type || '').toLowerCase();
  if (!WIDGET_TYPES.includes(type)) {
    console.warn('Unknown widget type — rendering as a table instead:', raw.type, raw);
    return TableCard({ ...raw, type: 'table' });
  }
  return WIDGET_COMPONENTS[type](raw);
}

// ---------- dashboard rendering ----------
function renderDashboard() {
  const d = state.dashboard;
  // Never editable in the public share view; a Viewer on an Active dashboard can look but
  // not refine it either — only the Owner/an Editor may (their own permission is what's
  // actually checked when they do, same as a fresh question — see DashboardAccessService).
  const editable = !!state.currentUser && state.dashboardRole !== 'viewer';
  el('dash-toolbar').classList.toggle('hidden', !d || !(d.widgets || []).length);
  el('edit-toggle').classList.toggle('hidden', !editable);
  el('autosave-status').classList.toggle('hidden', !editable);
  el('btn-undo').classList.toggle('hidden', !editable);
  el('btn-redo').classList.toggle('hidden', !editable);
  // A share is a frozen snapshot (Part 3) — there is nothing for "🔄 تحديث" to re-fetch.
  el('btn-refresh').classList.toggle('hidden', !!state.shareId);
  // A Draft has no Owner and can't be shared (see btn-share below) — offer to activate it
  // right here instead of sending the user to "السجل" to do it. Once Active, this hides and
  // مشاركة unlocks.
  const canActivate = editable && !state.shareId && !!state.currentHistoryId && !state.dashboardIsActive;
  el('btn-activate').classList.toggle('hidden', !canActivate);
  // Sharing publishes a frozen snapshot independent of the Active-dashboard Owner/Editor/
  // Viewer model (Part 3) — but a Draft has no Owner and no one else can even see it, so a
  // share link made from one would be an orphaned, unmanageable copy of someone's private
  // work-in-progress. Require activation first (also enforced server-side, see
  // ShareController.Create).
  el('btn-share').disabled = !state.dashboardIsActive;
  el('btn-share').title = state.dashboardIsActive ? '' : 'فعّل اللوحة أولاً قبل مشاركتها — زر «✅ تفعيل اللوحة» بجانبه.';
  // النشر إلى تكامل خارجي يحتاج لوحة مفعّلة (Active) بنفس شرط المشاركة أعلاه — لا معنى لنشر
  // Draft أو نسخة مشاركة مجمّدة. مخفي تمامًا (مش معطّل فقط) لغير المسؤولين — انظر renderIntegrationsAvailability.
  el('btn-publish').classList.toggle('hidden', !state.currentUser || state.currentUser.role !== 'Admin');
  el('btn-publish').disabled = !state.dashboardIsActive;
  el('btn-publish').title = state.dashboardIsActive ? 'نشر هذه اللوحة إلى نظام عميل خارجي' : 'فعّل اللوحة أولاً قبل نشرها.';
  if (!editable) state.editMode = false;
  updateUndoRedoButtons();

  chartInstances.forEach(c => c.destroy());
  chartInstances = [];
  pendingCharts = [];

  if (!d) {
    el.dash.innerHTML = state.loading
      ? `<div class="grid skeleton">${'<div class="widget"><div class="bar"></div></div>'.repeat(4)}</div>`
      : `<div class="empty"><div>
           <div class="big">اسأل سؤالاً عن بياناتك</div>
           <div>مثال: «قارن الإيرادات بين المناطق» أو «أعطني تقريراً شاملاً»</div>
         </div></div>`;
    return;
  }

  // A dashboard opened from السجل/اللوحات النشطة carries its own name (the title assigned
  // when it was saved or last renamed) — show that at the top instead of the summary text,
  // matching what "اللوحات النشطة" itself lists it as. A brand-new, not-yet-saved chat
  // dashboard has no name yet, so it keeps showing its summary as before.
  el.dash.innerHTML = `<div id="summary">${esc(d.name || d.summary)}</div>` + dashboardTopBarHtml(d.widgets);
  // A share (Part 3) is a frozen snapshot — its filter bar is informational only (which
  // selection was active when it was published), never interactive.
  if (state.shareId) {
    if (d.filters?.length) el.dash.appendChild(buildFilterBarReadOnly(d.filters));
  } else if (d.filters?.length || Object.keys(state.crossFilterDefs || {}).length) {
    el.dash.appendChild(buildFilterBar(d.filters || []));
  }

  // One-shot: true only for the render right after ask() lands a freshly (re)generated
  // dashboard, never for a filter/edit/undo re-render of the same widgets — see
  // revealDashboardOnNextRender's own comment above.
  revealNow = revealDashboardOnNextRender && !prefersReducedMotion();
  revealDashboardOnNextRender = false;

  const widgets = d.widgets || [];
  if (widgets.length) {
    const grid = document.createElement('div');
    grid.className = 'grid' + (state.editMode ? ' edit-mode' : '');
    widgets.forEach((w, i) => {
      const card = buildWidget(w);
      if (revealNow) {
        card.classList.add('widget-enter');
        // Capped so a dashboard with many widgets doesn't leave the last ones waiting
        // seconds to appear — beyond ~8 cards they all start together instead.
        card.style.animationDelay = `${Math.min(i, 8) * 55}ms`;
      }
      grid.appendChild(card);
    });
    el.dash.appendChild(grid);
    // Scoped strictly to the loop above — cleared immediately after so a widget built
    // outside a render pass (e.g. a comparison's own buildWidget() call is fine, it runs
    // inside this same loop; anything built later never should be) never counts up.
    revealNow = false;

    // Only now is every widget's chart-holder actually laid out with real dimensions —
    // instantiate the charts queued by mountChart() and keep each render's promise so
    // the PPTX export can await it before capturing the SVG (see widgetToPptxInput).
    pendingCharts.forEach(({ card, holder, options, labels: pointLabels, widget }) => {
      // chart.height:'100%' (set in chartConfig()/forecastChartConfig()) doesn't reliably
      // resolve for a mixed-series chart (a forecast's bar/line + rangeArea combo) — it falls
      // back near ApexCharts' own ~350px default regardless of the container, overflowing
      // .chart-holder and colliding with whatever sits below it. Passing the container's own
      // real measured height works for every chart shape, so it replaces '100%' universally.
      options.chart.height = holder.clientHeight;
      // The donut's center number + caption sizes, proportional to its actual rendered size
      // (not a fixed px) — right at holder.clientHeight/Width's real, laid-out values, the
      // same reason chart.height is set here rather than in chartConfig(). Ring thickness
      // (plotOptions.pie.donut.size) is untouched; only the center text scales.
      //
      // ApexCharts styles these two lines from two *different* config blocks even though
      // both render inside the same "total" state: the big number's font comes from
      // `labels.value` (yes, even for the idle grand-total figure, not just a hovered
      // slice's own value), while `labels.total`'s own fontSize/color style only the
      // caption underneath it (deriveUnitLabel(title)) — confirmed by inspecting the
      // actual rendered <text> elements, since it isn't obvious from the option names alone.
      if (options.chart.type === 'donut') {
        const box = Math.min(holder.clientWidth, holder.clientHeight);
        const numSize = Math.round(Math.max(18, Math.min(38, box * 0.16)));
        const capSize = Math.round(Math.max(10, numSize * 0.4));
        const labels = options.plotOptions.pie.donut.labels;
        labels.value.fontSize = numSize + 'px';
        // offsetY nudges each line away from ApexCharts' own default (already vertically
        // stacked, overlapping) position — *not* a shared center both are measured from, so
        // this can't be derived from font-size alone; multipliers below were tuned against
        // the real rendered <text> boxes (measured via getBoundingClientRect in a live
        // browser) until the number and caption cleared each other with a small gap, rather
        // than guessed from the font sizes' ratio.
        labels.value.offsetY = -Math.round(capSize * 1.15) - 3;
        labels.total.fontSize = capSize + 'px';
        labels.total.offsetY = Math.round(numSize * 0.55) + 3;
        labels.name.fontSize = capSize + 'px';
        labels.name.offsetY = Math.round(numSize * 0.55) + 3;
      }
      // Lollipop decoration only for the single-series bar chart built by chartConfig() (see
      // its plotOptions.bar.distributed:true — buildMultiSeriesChart never sets that) —
      // drawn once the entrance animation actually finishes; measuring bar geometry any
      // earlier (mid-animation) would decorate a not-yet-full-height bar.
      const isLollipopBar = options.chart.type === 'bar' && !options.__isForecast
        && !!options.plotOptions?.bar?.distributed;
      if (isLollipopBar) {
        const colors = options.colors;
        const values = options.series[0].data;
        // Stashed on the card (alongside __apexChart/__apexHolder below) so PPTX export can
        // re-decorate the captured SVG defensively without needing to reverse-engineer
        // ApexCharts' own internal color/series state — see widgetToPptxInput.
        card.__lollipopValues = values;
        card.__lollipopColors = colors;
        options.chart = {
          ...options.chart,
          events: { animationEnd: () => applyLollipopStyle(holder.querySelector('svg'), values, colors) },
        };
      }
      // Forecast charts (options.__isForecast) never reach here with a `widget` — their
      // categories mix real history with future, not-yet-real forecast periods, so
      // clicking one wouldn't correspond to an actual row to filter by.
      if (widget) attachCrossFilterClick(options, holder, widget, pointLabels);
      const chart = new ApexCharts(holder, options);
      card.__apexChart = chart;
      card.__apexHolder = holder;
      card.__apexRendered = chart.render();
      chartInstances.push(chart);
      if (isLollipopBar) {
        // Safety net: on the specific chart a cross-filter click just came from, ApexCharts'
        // own post-click bookkeeping can race with this destroy-and-recreate cycle and
        // "animationEnd" above simply never fires — leaving that one widget's bars
        // undecorated. Re-check shortly after the entrance animation should be done and
        // draw then if the event-driven path above didn't already (the class-presence check
        // keeps this a no-op in the normal case).
        const colors = options.colors;
        const values = options.series[0].data;
        const animMs = (options.chart.animations?.speed || 450) + (options.chart.animations?.animateGradually?.delay || 0);
        setTimeout(() => {
          const svg = holder.querySelector('svg');
          if (svg && !svg.querySelector('.bar-lollipop-badge')) applyLollipopStyle(svg, values, colors);
        }, animMs + 200);
      }
    });
    pendingCharts = [];
  }
  if (editable && state.editMode) el.dash.appendChild(buildAddWidgetButton());
}

// ---------- dashboard filters ----------
// Filters come from the model (see the "الفلاتر" section of the system prompt) as
// {id,label,field,table,type,options}. Applying one deterministically re-runs
// /api/widgets/query for every widget whose structured `query.table` matches the
// filter's table (wizard-created widgets) — never the LLM. A chat-authored widget
// carries no structured query, so a dashboard filter cannot reach it; rather than
// silently ignore it, buildWidget() marks it "غير متأثر بالفلتر" (see widgetShell).
function getActiveFilters() {
  // Declared filters (from the model/wizard) plus any ad-hoc ones created by clicking a
  // chart's data point (see toggleCrossFilter) — same {id,label,field,table,...} shape,
  // so applyFilters()'s per-table grouping below handles both without distinguishing them.
  const filters = [...(state.dashboard?.filters || []), ...Object.values(state.crossFilterDefs || {})];
  return filters
    .map(f => ({ def: f, values: state.activeFilters[f.id] || [] }))
    .filter(x => x.values.some(v => v !== '' && v != null));
}
function getActiveFilterTables() {
  return new Set(getActiveFilters().map(x => x.def.table).filter(Boolean));
}

function buildFilterBar(filters) {
  const bar = document.createElement('div');
  bar.id = 'filter-bar';
  filters.forEach(f => bar.appendChild(buildFilterControl(f)));
  // Ad-hoc cross-filters only ever show up here once they're actually active — there's no
  // "options" list to browse ahead of time the way a declared filter's dropdown has, so an
  // idle one has nothing worth rendering; clicking a different data point replaces it.
  Object.values(state.crossFilterDefs || {})
    .filter(f => (state.activeFilters[f.id] || []).length)
    .forEach(f => bar.appendChild(buildCrossFilterChip(f)));
  if (getActiveFilters().length) {
    const clearBtn = document.createElement('button');
    clearBtn.type = 'button'; clearBtn.className = 'filter-clear-btn'; clearBtn.textContent = '✕ مسح كل الفلاتر';
    clearBtn.addEventListener('click', clearAllFilters);
    bar.appendChild(clearBtn);
  }
  // Set by applyFilters() when one or more widgets couldn't be re-fetched even after its
  // own retry (e.g. a transient DB lock) — previously this failed completely silently,
  // leaving a widget stuck on stale/empty data with nothing on screen explaining why.
  if (state.filterRefreshWarning) {
    const warn = document.createElement('span');
    warn.className = 'filter-refresh-warning';
    warn.textContent = state.filterRefreshWarning;
    const retryBtn = document.createElement('button');
    retryBtn.type = 'button'; retryBtn.className = 'filter-refresh-retry'; retryBtn.textContent = '🔄 إعادة المحاولة';
    retryBtn.addEventListener('click', () => { state.filterRefreshWarning = null; applyFilters(); });
    warn.appendChild(retryBtn);
    bar.appendChild(warn);
  }
  return bar;
}

// A chart-click cross-filter's own chip — unlike buildSelectFilterControl there's no
// dropdown (only ever one known value: whatever was clicked), just a pill naming the
// field/value that clears itself on click, same interaction as Power BI's cross-filter
// icon on a selected visual.
function buildCrossFilterChip(f) {
  const values = state.activeFilters[f.id] || [];
  const btn = document.createElement('button');
  btn.type = 'button'; btn.className = 'xfilter-chip';
  btn.title = 'اضغط لإزالة هذا الفلتر';
  btn.innerHTML = `${esc(f.label)}: ${esc(values.filter(Boolean).join('، '))} <span class="xfilter-x">✕</span>`;
  btn.addEventListener('click', () => {
    pushUndo();
    delete state.activeFilters[f.id];
    delete state.crossFilterDefs[f.id];
    applyFilters();
  });
  return btn;
}

// A share's filter bar (Part 3) shows which selection was active when the snapshot was
// published — plain badges, no click handlers; nothing behind them can be re-run.
function buildFilterBarReadOnly(filters) {
  const bar = document.createElement('div');
  bar.id = 'filter-bar';
  filters.forEach(f => {
    const values = state.activeFilters[f.id];
    const text = Array.isArray(values) && values.length
      ? `${f.label}: ${values.filter(Boolean).join('، ') || 'الكل'}`
      : `${f.label}: الكل`;
    const badge = document.createElement('span');
    badge.className = 'filter-badge-static';
    badge.textContent = text;
    bar.appendChild(badge);
  });
  return bar;
}

function buildFilterControl(f) {
  return (f.type === 'date_range' || f.type === 'numeric_range') ? buildRangeFilterControl(f) : buildSelectFilterControl(f);
}

function buildSelectFilterControl(f) {
  const wrap = document.createElement('div');
  wrap.className = 'filter-ctl';
  const selected = state.activeFilters[f.id] || [];
  const isMulti = f.type === 'multi_select';

  const btn = document.createElement('button');
  btn.type = 'button'; btn.className = 'filter-btn' + (selected.length ? ' active' : '');
  btn.textContent = `${f.label}${selected.length ? ` (${selected.length})` : ''} ▾`;

  const panel = document.createElement('div');
  panel.className = 'filter-panel';
  const allChecked = !selected.length ? 'checked' : '';
  const optionsHtml = (f.options || []).map(o => `
    <label><input type="${isMulti ? 'checkbox' : 'radio'}" name="filter-${esc(f.id)}" value="${esc(o.value)}"
      ${selected.includes(o.value) ? 'checked' : ''}> ${esc(o.label)}</label>`).join('');
  panel.innerHTML = isMulti ? optionsHtml
    : `<label><input type="radio" name="filter-${esc(f.id)}" value="" ${allChecked}> الكل</label>${optionsHtml}`;

  btn.addEventListener('click', e => {
    e.stopPropagation();
    const open = panel.classList.contains('open');
    closeAllFilterPanels();
    if (!open) panel.classList.add('open');
  });
  panel.addEventListener('click', e => e.stopPropagation());
  panel.addEventListener('change', () => {
    const checked = [...panel.querySelectorAll('input:checked')].map(i => i.value).filter(v => v !== '');
    setFilterValue(f.id, checked);
  });

  wrap.appendChild(btn);
  wrap.appendChild(panel);
  return wrap;
}

function buildRangeFilterControl(f) {
  const wrap = document.createElement('div');
  wrap.className = 'filter-range';
  const [from = '', to = ''] = state.activeFilters[f.id] || [];
  const inputType = f.type === 'date_range' ? 'date' : 'number';
  wrap.innerHTML = `<span>${esc(f.label)}</span>
    <input type="${inputType}" data-role="from" value="${esc(from)}">
    <span>—</span>
    <input type="${inputType}" data-role="to" value="${esc(to)}">`;
  const apply = () => {
    const fromVal = wrap.querySelector('[data-role="from"]').value;
    const toVal = wrap.querySelector('[data-role="to"]').value;
    setFilterValue(f.id, (fromVal || toVal) ? [fromVal, toVal] : []);
  };
  wrap.querySelectorAll('input').forEach(inp => inp.addEventListener('change', apply));
  return wrap;
}

function closeAllFilterPanels() {
  document.querySelectorAll('.filter-panel.open').forEach(p => p.classList.remove('open'));
}
document.addEventListener('click', closeAllFilterPanels);

function setFilterValue(filterId, values) {
  pushUndo();
  state.activeFilters[filterId] = values;
  applyFilters();
}

function clearAllFilters() {
  pushUndo();
  state.activeFilters = {}; state.crossFilterDefs = {};
  applyFilters();
}

// Equality/IN filters only (date_range/numeric_range aren't supported by the deterministic
// backend yet — see WidgetQueryService — so they narrow the visible selection but don't
// re-query; this is a known v1 limitation, not a silent bug).
//
// Two calls can overlap: nothing here awaits or cancels a previous still-in-flight call
// before starting a new one, and every UI entry point (a filter dropdown, "✕ مسح كل
// الفلاتر", a cross-filter chart click, "🔄 تحديث") just calls applyFilters() directly. A
// user picking one filter value then quickly clearing it — or clicking two chart points in
// a row — starts a second call while the first's per-widget fetches are still in flight,
// and network timing gives no guarantee the *older* call's responses land first. Without a
// guard, an older call's slower response can land after the newer one and overwrite its
// correct (or correctly-cleared) w.data with stale filtered/empty data — a real, deterministic
// bug distinct from any transient fetch failure, and one that would survive a DB-side fix
// since nothing there was actually broken. applyFiltersSeq/mySeq below makes only the most
// recently *started* call allowed to write w.data or trigger the final render; a superseded
// call's in-flight responses are simply dropped once they arrive.
// A declared filter's `table` and a widget's own `query.table` are both free-form strings —
// a chat-authored dashboard's filter tool call and its widgets' own query tool calls are
// separate model outputs, so nothing guarantees they spell the same table identically. The
// backend's own ResolveTableAsync already matches table names case-insensitively (SQL
// Server's schema-qualified "staging.Sales" is a live example of a name the model can easily
// write inconsistently — with or without the schema prefix, in any case), but activeByTable's
// lookup below is a plain JS object key: an exact-string requirement that a filter and its
// widget disagree on gives a filter that silently matches nothing — no error, the request
// still succeeds, it's just sent with an empty filter list — which reads as "the filter does
// nothing at all". Comparing on this normalized form instead closes that gap.
function normalizeTableKey(t) {
  return String(t || '').trim().toLowerCase()
    .replace(/["[\]]/g, '')     // SQL Server bracket / quoted identifiers
    .replace(/^dbo\./, '')      // SQL Server's default schema is effectively invisible
    .replace(/^staging[._]/, ''); // this app's own schema prefix — see DataStore.DisplayTable
}

let applyFiltersSeq = 0;
async function applyFilters() {
  // A share is a frozen snapshot (Part 3) — nothing in it is ever re-run. Its filter
  // controls are read-only (buildFilterBarReadOnly) and it has no refresh button, so this
  // shouldn't normally be reachable in share view at all; the guard is defense in depth.
  if (state.shareId) return;
  const mySeq = ++applyFiltersSeq;
  state.filtersLoading = true;
  updateFiltersLoadingUI();
  const activeByTable = {};
  getActiveFilters().forEach(({ def, values }) => {
    if (!def.table || def.type === 'date_range' || def.type === 'numeric_range') return;
    (activeByTable[normalizeTableKey(def.table)] ||= []).push({ field: def.field, values });
  });

  if (state.currentUser) el('autosave-status')?.classList.remove('hidden');
  const status = el('autosave-status');
  if (status) status.textContent = 'جارٍ التحديث…';
  // Re-checked fresh on every call (a since-fixed dashboard should stop showing the
  // banner) — only a 409 from the Active-dashboard refresh branch below sets it again.
  if (state.dashboardIsActive) { state.dashboardDisabled = false; state.dashboardDisabledReason = ''; }

  const sources = { systems: [...state.onSystems], files: [...state.onFiles] };
  const widgets = state.dashboard?.widgets || [];
  // Each attempt below can transiently fail (e.g. the SQLite file taking a concurrent write
  // from another request while this filter's burst of per-widget reads lands) without being
  // this widget's fault — a single retry after a short pause clears the vast majority of
  // those instead of leaving the widget stuck on stale/empty data with no way to recover
  // short of a full page reload. `failed` tracks whatever doesn't clear even after the
  // retry, so the user gets an honest status instead of the previous silent no-op.
  let failed = 0;
  await Promise.all(widgets.map(async (w, index) => {
    if (!w.query || !w.query.table || w.query.integrationId) return;
    const filters = activeByTable[normalizeTableKey(w.query.table)] || [];
    for (let attempt = 0; attempt < 2; attempt++) {
      if (attempt > 0) await new Promise(r => setTimeout(r, 400));
      try {
        // An Active dashboard (Part 2) always re-executes live, under the Owner's own
        // permission, on every open/filter/refresh — same "index-only, never client-supplied
        // sql/table" endpoint shape as the share-scoped one above, just Owner- instead of
        // sharer-scoped and auth-required instead of anonymous.
        if (state.dashboardIsActive && state.currentHistoryId) {
          const res = await fetch(`/api/history/${encodeURIComponent(state.currentHistoryId)}/widgets/${index}/refresh`, {
            method: 'POST', headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ filters }),
          });
          // A newer applyFilters() call has started since this fetch went out — its own
          // responses (for whatever filter selection is now actually active) are what should
          // win, so this now-stale one is dropped rather than overwriting w.data out of order.
          if (mySeq !== applyFiltersSeq) return;
          if (res.status === 409) {
            const payload = await res.json().catch(() => null);
            if (mySeq !== applyFiltersSeq) return;
            state.dashboardDisabled = true;
            state.dashboardDisabledReason = payload?.reason || 'هذه اللوحة معطّلة حاليًا.';
            break;
          }
          if (!res.ok) { if (attempt === 1) failed++; continue; }
          const payload = await res.json();
          if (mySeq !== applyFiltersSeq) return;
          w.data = payload.data;
          if ('xKey' in payload) w.xKey = payload.xKey;
          if ('yKey' in payload) w.yKey = payload.yKey;
          if ('source' in payload) w.source = payload.source;
          break;
        }
        // Two shapes share the same "query" field depending on how the widget was built:
        // the wizard's fully-structured query (metric/aggregation/dimension/...), rebuilt
        // from scratch server-side — vs. a chat-authored widget's own stored SQL (just
        // {table, sql}), re-run with the filter spliced into that exact query instead.
        if (w.query.sql) {
          const res = await fetch('/api/widgets/sql-filter', {
            method: 'POST', headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ sources, table: w.query.table, sql: w.query.sql, filters }),
          });
          if (mySeq !== applyFiltersSeq) return;
          if (!res.ok) { if (attempt === 1) failed++; continue; }
          const payload = await res.json();
          if (mySeq !== applyFiltersSeq) return;
          w.data = payload.data;
          break;
        }
        const res = await fetch('/api/widgets/query', {
          method: 'POST', headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ sources, query: { ...w.query, filters } }),
        });
        if (mySeq !== applyFiltersSeq) return;
        if (!res.ok) { if (attempt === 1) failed++; continue; }
        const payload = await res.json();
        if (mySeq !== applyFiltersSeq) return;
        w.data = payload.data; w.xKey = payload.xKey; w.yKey = payload.yKey; w.source = payload.source;
        w.query = payload.query;
        break;
      } catch {
        // leave the widget's previous data in place unless every attempt fails
        if (attempt === 1) failed++;
      }
    }
  }));

  // A newer call already started (and owns rendering its own, more current result, including
  // clearing the loading state once *it* finishes) — this one's "failed" count and render
  // would only relitigate an outdated filter selection.
  if (mySeq !== applyFiltersSeq) return;

  state.filterRefreshWarning = failed > 0
    ? (failed === 1 ? '⚠ تعذّر تحديث رسم واحد.' : `⚠ تعذّر تحديث ${failed} رسومات.`)
    : null;
  state.filtersLoading = false;
  updateFiltersLoadingUI();
  renderDashboard();
  scheduleAutosave();
  updateUndoRedoButtons();
}

// Toggles the dashboard's loading overlay/dim without touching #dash's own contents — a
// full renderDashboard() mid-flight would tear down and rebuild every widget (and its
// ApexCharts instance) before the new data even arrives, which is wasted work and a visible
// flicker; this only needs to add/remove a class and flip one element's visibility.
function updateFiltersLoadingUI() {
  el('dash-pane')?.classList.toggle('filters-loading', !!state.filtersLoading);
  el('dash-loading')?.classList.toggle('open', !!state.filtersLoading);
}

// "🔄 تحديث": re-fetches every widget that carries query lineage straight from the live
// database — applyFilters() already does exactly this (it re-runs each such widget's query
// with whichever filter is currently active, even an empty one), so refreshing is just
// calling it again on demand instead of only after a filter change. A widget with no query
// lineage at all (e.g. combined from several tool calls, or a forecast) can't be refreshed
// this way and simply keeps its existing data, same as it already does for filtering.
el('btn-refresh').addEventListener('click', async () => {
  const btn = el('btn-refresh');
  btn.disabled = true;
  try { await applyFilters(); }
  finally { btn.disabled = false; }
});

function buildAddWidgetButton() {
  const btn = document.createElement('button');
  btn.type = 'button'; btn.className = 'add-widget-btn'; btn.textContent = '+ إضافة عنصر';
  btn.addEventListener('click', openWizard);
  return btn;
}

// ---------- data-source badges (shown at the top of any dashboard, Draft or Active) ----------
// Every widget built from a single query_data call carries query.table (see AnalyticsTools'
// system prompt) — the same structured lineage the dashboard-filter/refresh features already
// rely on. A widget with none (built from several aggregated calls, list_files/
// search_documents alone, or a forecast) simply isn't counted, same limitation those
// features already have and surface as "غير متأثر بالفلتر".
function computeDashboardSources(widgets) {
  const tables = new Set();
  const collect = w => {
    if (w.comparison) { collect(w.comparison.left || {}); collect(w.comparison.right || {}); return; }
    if (w.query?.table) tables.add(w.query.table);
  };
  (widgets || []).forEach(collect);
  return [...tables]
    .map(t => state.tableLabels[t] || t)
    .sort((a, b) => a.localeCompare(b, 'ar'));
}
function dashboardSourcesHtml(widgets) {
  const labels = computeDashboardSources(widgets);
  if (!labels.length) return '';
  return `<span class="ds-label">🗄️ مصادر البيانات:</span>
    ${labels.map(l => `<span class="ds-chip">${esc(l)}</span>`).join('')}`;
}

// ---------- Part 2: Active-dashboard status bar (state badge, role badge, manage-roles) ----------
const ROLE_LABELS = { owner: 'مالك', editor: 'محرر', viewer: 'مشاهد' };
function dashboardStatusBadgesHtml() {
  const roleBadge = state.dashboardRole
    ? `<span class="role-badge${state.dashboardRole === 'viewer' ? ' role-viewer' : ''}">${ROLE_LABELS[state.dashboardRole] || state.dashboardRole}</span>`
    : '';
  const manageBtn = state.dashboardRole === 'owner'
    ? `<button type="button" class="manage-roles-btn" data-id="${esc(state.currentHistoryId || '')}">⚙️ إدارة الصلاحيات</button>` : '';
  return `<span class="state-badge active">نشطة</span>${roleBadge}${manageBtn}`;
}
function dashboardDisabledBannerHtml() {
  return state.dashboardDisabled
    ? `<div class="dashboard-disabled-banner">⚠️ ${esc(state.dashboardDisabledReason || 'هذه اللوحة معطّلة حاليًا.')}</div>`
    : '';
}

// Data sources on one side, the Active-dashboard state/role/manage-roles badges on the
// other — one row, opposite ends — rather than two separate stacked rows.
function dashboardTopBarHtml(widgets) {
  const sources = dashboardSourcesHtml(widgets);
  const status = state.dashboardIsActive ? dashboardStatusBadgesHtml() : '';
  if (!sources && !status) return '';
  return `<div id="dashboard-top-bar">
      <div id="dashboard-sources">${sources}</div>
      <div id="dashboard-status-bar">${status}</div>
    </div>` + dashboardDisabledBannerHtml();
}
// Delegated (not re-bound every render, since #dashboard-status-bar's own HTML is replaced
// wholesale by renderDashboard() each time) — #dash is the stable ancestor. (el.dash isn't
// cached yet at this point in the script — see the "boot" section near the bottom — so this
// resolves the element directly instead.)
el('dash').addEventListener('click', e => {
  const btn = e.target.closest('.manage-roles-btn');
  if (btn?.dataset.id) openRolesModal(btn.dataset.id);
});

// ---------- export (PDF / PowerPoint) ----------
// PDF: the browser's own print-to-PDF, steered by the @media print rules above —
// no server round trip, and it prints exactly what's on screen.
el('btn-export-pdf').addEventListener('click', () => window.print());

// PowerPoint: KPI values and tables travel as plain data so they land as native,
// still-editable PowerPoint shapes; bar/line/pie charts are rendered as inline SVG by
// ApexCharts. Its own dataURI() export API sounded like the obvious way to turn that into
// a PNG, but it silently ignores the 3D view's extruded-bar polygons (mountChart() adds
// those as extra <polygon> elements next to the real bar path) — dataURI() apparently
// rebuilds the image from ApexCharts' own internal series model rather than serializing the
// live SVG DOM. Serializing that SVG element ourselves (clone -> XML string -> Image ->
// canvas) is a few more lines but captures the chart exactly as it looks on screen,
// extrusion included. The result is transparent wherever the chart is, so it's composited
// onto white first — the PPTX slide is white either way.
function svgElementToPngDataUrl(svgEl, scale = 2) {
  return new Promise((resolve, reject) => {
    const rect = svgEl.getBoundingClientRect();
    const clone = svgEl.cloneNode(true);
    clone.setAttribute('width', rect.width);
    clone.setAttribute('height', rect.height);
    clone.setAttribute('xmlns', 'http://www.w3.org/2000/svg');
    // Hover-only elements ApexCharts normally keeps invisible via its own stylesheet
    // (opacity: 0 until you hover) — that stylesheet isn't available to a standalone image
    // decode, so left in, the crosshair rect renders as a solid tinted band.
    clone.querySelectorAll('.apexcharts-xcrosshairs, .apexcharts-ycrosshairs, .apexcharts-tooltip')
      .forEach(el => el.remove());
    const xml = new XMLSerializer().serializeToString(clone);
    const img = new Image();
    img.onload = () => {
      const canvas = document.createElement('canvas');
      canvas.width = Math.max(1, Math.round(rect.width * scale));
      canvas.height = Math.max(1, Math.round(rect.height * scale));
      const ctx = canvas.getContext('2d');
      ctx.fillStyle = '#ffffff';
      ctx.fillRect(0, 0, canvas.width, canvas.height);
      ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
      resolve(canvas.toDataURL('image/png'));
    };
    img.onerror = () => reject(new Error('chart SVG image decode failed'));
    img.src = 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(xml);
  });
}

// progress-table / trend-matrix / status-bar / linear-gauge have no native PPTX shape of
// their own — the backend builder only knows kpi / table / image. Rather than teaching it
// four more bespoke layouts (or a native side-by-side layout for comparison), every one of
// them is flattened into the same {columns, rows} table shape it already renders as an
// editable PowerPoint table; a comparison widget is flattened into a two-column-by-side
// table regardless of the type being compared. Best-effort but faithful to the real
// numbers — never blank. radial-gauge needs none of this: it's ApexCharts-based (a
// radialBar chart, see RadialGaugeCard), so it already falls through to the generic
// chart-capture path below like bar/line/pie do.
function comparisonToPptxInput(base, w) {
  const left = w.comparison.left || {};
  const right = w.comparison.right || {};
  const leftRows = Array.isArray(left.data) ? left.data : [];
  const rightRows = Array.isArray(right.data) ? right.data : [];
  const labels = [];
  leftRows.forEach(r => { if (r?.label != null && !labels.includes(r.label)) labels.push(r.label); });
  rightRows.forEach(r => { if (r?.label != null && !labels.includes(r.label)) labels.push(r.label); });
  if (labels.length) {
    const findVal = (rows, label) => rows.find(r => r.label === label)?.value;
    return {
      ...base, type: 'table',
      columns: ['البند', left.title || 'الفترة الأولى', right.title || 'الفترة الثانية'],
      rows: labels.map(l => [String(l), fmt(findVal(leftRows, l)), fmt(findVal(rightRows, l))]),
    };
  }
  const valOf = rows => { const e = rows[0]; return e?.value ?? Object.values(e || {}).find(x => typeof x === 'number'); };
  return {
    ...base, type: 'table',
    columns: ['الفترة', 'القيمة'],
    rows: [
      [left.title || 'الفترة الأولى', fmt(valOf(leftRows))],
      [right.title || 'الفترة الثانية', fmt(valOf(rightRows))],
    ],
  };
}

async function widgetToPptxInput(w, card) {
  const base = { type: (w.type || '').toLowerCase(), title: w.title || '', source: w.source || '' };
  if (w.comparison && typeof w.comparison === 'object') return comparisonToPptxInput(base, w);
  if (base.type === 'kpi') {
    const e = Array.isArray(w.data) ? w.data[0] : null;
    const v = e?.value ?? Object.values(e || {}).find(x => typeof x === 'number');
    return { ...base, value: fmt(v), label: e?.label ?? '' };
  }
  if (base.type === 'table') {
    const rows = Array.isArray(w.data) ? w.data : [];
    const cols = rows.length ? Object.keys(rows[0]) : [];
    return { ...base, columns: cols, rows: rows.map(r => cols.map(c => String(r[c] ?? ''))) };
  }
  if (base.type === 'progress-table') {
    const rows = Array.isArray(w.data) ? w.data : [];
    return {
      ...base, type: 'table',
      columns: ['البند', 'التفاصيل', 'الإجمالي'],
      rows: rows.map(r => [
        String(r.label ?? ''),
        Array.isArray(r.segments) ? r.segments.map(s => `${s.label ?? ''}: ${fmt(s.value)}`).join(' · ') : '',
        fmt(r.total),
      ]),
    };
  }
  if (base.type === 'trend-matrix') {
    const rows = Array.isArray(w.data) ? w.data : [];
    const periodLabels = (Array.isArray(rows[0]?.periods) ? rows[0].periods : []).map(p => p.label ?? '');
    return {
      ...base, type: 'table',
      columns: ['المؤشر', ...periodLabels, 'الاتجاه'],
      rows: rows.map(r => {
        const periods = Array.isArray(r.periods) ? r.periods : [];
        const last = periods[periods.length - 1]?.value;
        const prev = periods[periods.length - 2]?.value;
        const arrow = (typeof last === 'number' && typeof prev === 'number')
          ? (last > prev ? '↑' : last < prev ? '↓' : '→') : '→';
        return [String(r.metric ?? ''), ...periods.map(p => fmt(p.value)), arrow];
      }),
    };
  }
  if (base.type === 'status-bar') {
    const data = w.data || {};
    const statuses = Array.isArray(data.statuses) ? data.statuses : [];
    return {
      ...base, type: 'table',
      columns: ['البند', 'العدد'],
      rows: [
        [data.label || 'الإجمالي', fmt(data.total)],
        ...statuses.map(s => [String(s.label ?? ''), fmt(s.count)]),
      ],
    };
  }
  if (base.type === 'linear-gauge') {
    const rows = Array.isArray(w.data) ? w.data : [];
    return {
      ...base, type: 'table',
      columns: ['البند', 'القيمة'],
      rows: rows.map(r => [String(r.label ?? ''), r.max != null ? `${fmt(r.value)} / ${fmt(r.max)}` : `${fmt(r.value)}٪`]),
    };
  }
  // radial-gauge has no branch here — it's ApexCharts-based (radialBar, see
  // RadialGaugeCard/radialGaugeConfig) and falls through to the generic chart-capture path
  // below exactly like bar/line/pie do, no bespoke flattening needed.
  const chart = card?.__apexChart;
  const holder = card?.__apexHolder;
  if (!chart || !holder) return base;
  try {
    await card.__apexRendered; // wait for the initial paint before capturing its SVG
    // The entrance animation (chartConfig()'s chart.animations) may still be mid-flight the
    // instant this runs — snap it to its finished state first, or the export can capture a
    // bar not yet at full height or a line not yet fully drawn. `animate: false` on this
    // particular update redraws immediately rather than transitioning.
    await chart.updateOptions({ chart: { animations: { enabled: false } } }, false, false, false);
    const svg = holder.querySelector('svg');
    if (!svg) return base;
    // Re-applied defensively (it's idempotent) rather than relying solely on the
    // animationEnd hook in renderDashboard() — exporting fast enough to beat that event
    // would otherwise capture a lollipop bar with no track/badge yet.
    if (card.__lollipopColors) applyLollipopStyle(svg, card.__lollipopValues, card.__lollipopColors);
    return { ...base, image: await svgElementToPngDataUrl(svg) };
  }
  catch { return base; }
}

async function exportPptx() {
  const d = state.dashboard;
  if (!d?.widgets?.length) return;
  const btn = el('btn-export-pptx');
  btn.disabled = true; const original = btn.textContent; btn.textContent = 'جارٍ التصدير…';
  try {
    const cards = [...document.querySelectorAll('#dash .grid > .widget')];
    const widgets = await Promise.all(d.widgets.map((w, i) => widgetToPptxInput(w, cards[i])));
    const lastQuestion = [...state.messages].reverse().find(m => m.role === 'user')?.text;

    const res = await fetch('/api/export/pptx', {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ title: lastQuestion || 'لوحة معلومات', summary: d.summary, widgets }),
    });
    if (!res.ok) throw new Error(`فشل التصدير (${res.status})`);
    const blob = await res.blob();
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = 'dashboard.pptx';
    document.body.appendChild(a); a.click(); a.remove();
    URL.revokeObjectURL(url);
  } catch (err) {
    alert('تعذّر تصدير العرض التقديمي: ' + err.message);
  } finally {
    btn.disabled = false; btn.textContent = original;
  }
}
el('btn-export-pptx').addEventListener('click', exportPptx);

// ---------- chat ----------
function renderMessages() {
  const items = state.messages.map((m, i) => {
    const cls = m.role === 'user' ? 'user' : (m.error ? 'err' : 'bot');
    // Only a real bot answer gets a speaker button — never an error message or the user's
    // own turn, and never at all in a browser with no speechSynthesis to act on it.
    const speak = (cls === 'bot' && ttsSupported())
      ? `<button type="button" class="msg-speak-btn" data-idx="${i}" title="قراءة بصوت عالٍ">🔊</button>` : '';
    return `<div class="msg ${cls}">${esc(m.text)}${speak}</div>`;
  }).join('');
  const typing = state.loading
    ? `<div class="msg bot dots"><span></span><span></span><span></span></div>` : '';
  const hint = state.messages.length === 0 && !state.loading
    ? `<div class="hint">اسأل سؤالاً عن بياناتك وسيبني لك المساعد لوحة معلومات.
         <span class="ex">جرّب: «ما إجمالي الإيرادات حسب المنطقة؟»</span></div>` : '';
  el.messages.innerHTML = hint + items + typing;
  el.messages.scrollTop = el.messages.scrollHeight;
  renderSuggestions();
}
el('messages').addEventListener('click', e => {
  const btn = e.target.closest('.msg-speak-btn');
  if (!btn) return;
  const msg = state.messages[Number(btn.dataset.idx)];
  speakText(msg?.text || '', msg?.speechWidgets);
});

// ---------- voice: text-to-speech (the model talks back) ----------
// Reads only the short plain-language summary a response already carries — never the charts
// or tables themselves, which have no meaningful spoken form. Two independent triggers: the
// 🔊 button on any individual bot message (always available), and an opt-in "قراءة تلقائية"
// toggle that speaks every *new* answer the moment it arrives — off by default, since an
// unannounced voice reading a business number aloud is the wrong call in a shared office.
const ttsSupported = () => 'speechSynthesis' in window;

// Splits the summary into rough sentences (Arabic/Latin stops or newlines), keeping each
// piece's start offset in the original string so a `boundary` event's charIndex can be
// mapped back to "which sentence is being read right now".
function splitSentences(text) {
  const sentences = [];
  const re = /[^.!؟\n]+[.!؟\n]*/g;
  let m;
  while ((m = re.exec(text))) sentences.push({ start: m.index, text: m[0] });
  return sentences;
}

function clearTtsHighlight() {
  document.querySelectorAll('.widget.tts-highlight').forEach(c => c.classList.remove('tts-highlight'));
}

// Best-effort only: a sentence "names" a widget when the widget's own title appears in it
// verbatim, which holds whenever the model's summary talks about a widget by name (the
// common case) and simply highlights nothing when it doesn't — never a hard requirement.
function widgetForSentence(sentence, widgets) {
  if (!widgets?.length) return null;
  const candidates = widgets.filter(w => w.title && w.title.trim().length >= 3 && sentence.includes(w.title));
  if (!candidates.length) return null;
  // Prefer the most specific (longest) title match when more than one widget's name appears.
  return candidates.reduce((a, b) => (b.title.length > a.title.length ? b : a));
}

// `widgets` is optional: [{id, title}, ...] for the dashboard this text describes, used only
// to drive the read-along highlight below — speech itself works identically without it.
function speakText(text, widgets) {
  if (!ttsSupported() || !text) return;
  window.speechSynthesis.cancel(); // never overlap two utterances
  clearTtsHighlight();
  const utter = new SpeechSynthesisUtterance(text);
  utter.lang = 'ar-SA';
  const arabicVoice = window.speechSynthesis.getVoices().find(v => (v.lang || '').toLowerCase().startsWith('ar'));
  if (arabicVoice) utter.voice = arabicVoice;

  if (widgets?.length) {
    const sentences = splitSentences(text);
    let lastId = null;
    // Not every engine fires word-level boundary events (support is inconsistent across
    // browsers); when it doesn't, the highlight simply never appears — speech is unaffected.
    utter.onboundary = e => {
      if (e.name && e.name !== 'word' && e.name !== 'sentence') return;
      const sentence = sentences.findLast ? sentences.findLast(s => s.start <= e.charIndex) : [...sentences].reverse().find(s => s.start <= e.charIndex);
      const match = sentence ? widgetForSentence(sentence.text, widgets) : null;
      if (match?.id === lastId) return;
      clearTtsHighlight();
      if (match) {
        const card = document.querySelector(`.widget[data-widget-id="${match.id}"]`);
        card?.classList.add('tts-highlight');
        lastId = match.id;
      } else {
        lastId = null;
      }
    };
    utter.onend = clearTtsHighlight;
    utter.onerror = clearTtsHighlight;
  }

  window.speechSynthesis.speak(utter);
}

try { state.autoRead = localStorage.getItem('chatToDashboardAutoRead') === '1'; } catch { state.autoRead = false; }

function syncAutoReadButton() {
  const btn = el('auto-read-toggle');
  btn.classList.toggle('active', state.autoRead);
  btn.textContent = state.autoRead ? '🔊 قراءة تلقائية' : '🔇 قراءة تلقائية';
}
if (ttsSupported()) {
  el('auto-read-toggle').classList.remove('hidden');
  syncAutoReadButton();
  el('auto-read-toggle').addEventListener('click', () => {
    state.autoRead = !state.autoRead;
    try { localStorage.setItem('chatToDashboardAutoRead', state.autoRead ? '1' : '0'); } catch {}
    syncAutoReadButton();
    if (!state.autoRead) window.speechSynthesis.cancel();
  });
}

// ---------- continuation: "🆕 ابدأ لوحة جديدة" ----------
// Empties the dashboard AND the chat transcript immediately — a real "start over", not just
// a flag for the next question. Since ask() only sends the current dashboard as continuation
// context when one actually exists (state.dashboard?.widgets?.length), an empty dashboard
// already means the next question is answered from scratch — no separate "armed" state
// needed. Deliberately a button, not wording detection — see AnalyticsTools.ComposeUserMessage.
el('new-dashboard-btn').addEventListener('click', () => {
  state.dashboard = null;
  state.editHistory = { past: [], future: [] };
  state.currentHistoryId = null;
  state.activeFilters = {}; state.crossFilterDefs = {}; state.filterRefreshWarning = null;
  state.messages = [];
  state.dashboardIsActive = false; state.dashboardRole = null;
  state.dashboardDisabled = false; state.dashboardDisabledReason = '';
  if (ttsSupported()) window.speechSynthesis.cancel(); // don't keep reading a wiped-out message
  renderDashboard();
  renderMessages();
});

// ---------- voice: speech-to-text (the user talks to the model) ----------
// A mic button that transcribes into the same #q box any typed question already goes
// through — voice is just an alternate way to fill that box, never a separate path, so every
// existing rule (sources, filters, the tool loop) applies unchanged. Deliberately never
// auto-sends: Arabic recognition can mishear a dialect word, a technical term, or an exact
// entity name, so the transcript stays editable until the user presses "إرسال" themselves.
//
// continuous:true, not false: with continuous:false the browser itself decides when a
// pause in speech means "done talking" and stops listening on its own — reported live as
// the mic cutting off mid-sentence before the user finished. continuous:true keeps the
// session open across pauses; only the user's own second click on مايك (stopListening,
// below) — or leaving the page/an actual error — ends it, matching "keep listening until I
// press it again."
const SpeechRecognitionCtor = window.SpeechRecognition || window.webkitSpeechRecognition;
if (SpeechRecognitionCtor) {
  el('mic-btn').classList.remove('hidden');
  let recognition = null;
  let listening = false;

  function stopListening() {
    listening = false;
    el('mic-btn').classList.remove('listening');
    try { recognition?.stop(); } catch {}
  }

  el('mic-btn').addEventListener('click', () => {
    if (listening) { stopListening(); return; }
    recognition = new SpeechRecognitionCtor();
    recognition.lang = 'ar-SA';
    recognition.interimResults = true;
    recognition.continuous = true;
    listening = true;
    el('mic-btn').classList.add('listening');

    // Text already confirmed by an earlier session, if the engine ends the session on its
    // own — some browsers still do this after a long-enough silence even with
    // continuous:true — and the 'end' handler below transparently restarts it. Each restart
    // is technically a fresh session whose own ev.results starts over from empty, so without
    // this, restarting would wipe out everything already said instead of just continuing it.
    let baseTranscript = '';

    recognition.addEventListener('result', ev => {
      let transcript = '';
      for (let i = 0; i < ev.results.length; i++) transcript += ev.results[i][0].transcript;
      el('q').value = baseTranscript + transcript;
    });
    recognition.addEventListener('error', ev => {
      // no-speech fires routinely in continuous mode during an ordinary pause between
      // sentences — it is not the user stopping, so (unlike before) don't tear the session
      // down for it; let it keep listening exactly as continuous:true promises.
      if (ev.error === 'no-speech') return;
      stopListening();
      if (ev.error === 'aborted') return;
      const messages = {
        'not-allowed': 'محتاج إذن استخدام الميكروفون من المتصفح.',
        'audio-capture': 'مفيش ميكروفون متاح.',
      };
      alert(messages[ev.error] || 'تعذّر التعرف على الصوت: ' + ev.error);
    });
    recognition.addEventListener('end', () => {
      if (!listening) return; // the user's own click (or a real error) already stopped it
      baseTranscript = el('q').value;
      try { recognition.start(); } catch { stopListening(); }
    });
    try { recognition.start(); } catch { stopListening(); }
  });
}

// ---------- follow-up suggestions ----------
// A handful of ready-to-click follow-up prompts ("خليه شهري"، "أضف جدول تفصيلي"...) —
// computed purely from the dashboard that's actually on screen right now (chart types,
// x-axis fields, widget count), never from a fixed list. Clicking one just calls ask()
// with that text, so it goes through the exact same follow-up handling (and the true-
// current-state context fix above) as anything the user types by hand.
function looksTimeBased(w) {
  const key = (w.xKey || '').toLowerCase();
  if (/date|month|week|year|شهر|تاريخ|سنة|أسبوع|period/i.test(key)) return true;
  const rows = Array.isArray(w.data) ? w.data : [];
  const v = w.xKey ? rows[0]?.[w.xKey] : undefined;
  return typeof v === 'string' && /^\d{4}-\d{2}(-\d{2})?$/.test(v);
}

function computeSuggestions(d) {
  if (!d?.widgets?.length) return [];
  const widgets = d.widgets;
  const types = new Set(widgets.map(w => (w.type || '').toLowerCase()));
  const hasTimeChart = widgets.some(w => ['bar', 'line'].includes((w.type || '').toLowerCase()) && looksTimeBased(w));

  const chips = [];
  if (hasTimeChart) { chips.push('خليه شهري'); chips.push('اعرض آخر 3 شهور بس'); }
  if (types.has('bar') && !types.has('line')) chips.push('غيّر الرسم لخط بياني');
  if (!types.has('pie') && widgets.length <= 6) chips.push('أضف رسم توزيع (Pie)');
  if (!types.has('table')) chips.push('أضف جدول تفصيلي');
  if (widgets.length <= 3) chips.push('اعمل تقرير شامل بدل كده');
  return chips.slice(0, 4);
}

function renderSuggestions() {
  const existing = document.getElementById('suggest-row');
  if (existing) existing.remove();
  if (state.loading || !state.dashboard?.widgets?.length) return;

  const chips = computeSuggestions(state.dashboard);
  if (!chips.length) return;

  const row = document.createElement('div');
  row.id = 'suggest-row';
  row.className = 'suggest-row';
  row.innerHTML = chips.map(c => `<button type="button" class="suggest-chip">${esc(c)}</button>`).join('');
  row.querySelectorAll('.suggest-chip').forEach(btn =>
    btn.addEventListener('click', () => { if (!state.loading) ask(btn.textContent); }));
  el.messages.after(row);
}

// Whether/how the new question continues the dashboard on screen is decided here, explicitly
// and structurally — never guessed from the question's wording (see AnalyticsTools.
// ComposeUserMessage on the backend, which frames this as the model's only continuation
// signal). Default: continue, sending the full current summary+widgets (source fields and
// all) so a follow-up like "اعرضلي بس القطاع الرقمي" can refine what's already there. Once "🆕
// ابدأ لوحة جديدة" empties state.dashboard, there is nothing to send and the next question
// starts from scratch automatically.
async function ask(question) {
  const currentDashboard = state.dashboard?.widgets?.length
    ? { summary: state.dashboard.summary, widgets: state.dashboard.widgets }
    : null;
  const isContinuation = !!currentDashboard;
  const image = state.attachedImage;

  state.messages.push({ role: 'user', text: image ? `📎 ${state.attachedImageName}\n${question}` : question });
  state.attachedImage = null; state.attachedImageName = ''; renderAttachedImage();
  state.loading = true;
  renderMessages(); renderDashboard(); el.send.disabled = true;

  try {
    const res = await fetch('/api/chat', {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        message: question, currentDashboard, image,
        // Only when actually continuing an Active dashboard (never a Draft) — lets the
        // server re-apply the same "Editors can't introduce a new data source" rule the
        // autosave/wizard path already enforces, since a chat continuation would otherwise
        // silently fork into a disconnected Draft that never goes through that check at all.
        historyId: isContinuation && state.dashboardIsActive ? state.currentHistoryId : null,
        sources: { systems: [...state.onSystems], files: [...state.onFiles] },
        lang: state.lang,
      }),
    });
    const payload = await res.json().catch(() => null);
    if (!res.ok || !payload?.dashboard) {
      state.messages.push({ role: 'bot', text: payload?.error || `تعذّر تنفيذ الطلب (${res.status})`, error: true });
    } else {
      if (isContinuation) {
        // The system prompt tells the model to always copy forward every existing widget
        // it isn't explicitly editing/deleting (see AnalyticsTools' "متابعة" instructions),
        // but that's still just an instruction — a smaller/local model (seen with Ollama)
        // can silently drop widgets from its JSON output while its own summary claims it
        // kept everything. Widgets have no stable id the model echoes back (unlike filters
        // below), so match by title instead: any existing widget whose title doesn't
        // survive into the new response is presumed dropped by mistake and restored,
        // unless the question itself looks like an explicit deletion request.
        const oldWidgets = state.dashboard?.widgets || [];
        const newWidgets = payload.dashboard.widgets || [];
        const deletionRequested = /احذف|امسح|شيل|إزال|ألغِ|ألغي|تخلص/.test(question);
        if (!deletionRequested && oldWidgets.length && newWidgets.length < oldWidgets.length) {
          const newTitles = new Set(newWidgets.map(w => w.title));
          const missing = oldWidgets.filter(w => !newTitles.has(w.title));
          payload.dashboard.widgets = [...missing, ...newWidgets];
        }

        // The model isn't told what filters already exist (only summary+widgets travel as
        // continuation context — see ComposeUserMessage), so it has no way to deliberately
        // keep them; left alone, every continuation turn would silently replace the whole
        // filters array with whatever it freshly decided this time, dropping the ones the
        // user already had selected. Keep every existing filter (by id) as-is, and only
        // append a genuinely new one the model proposed for this turn (e.g. for a widget
        // it just added) — never let a repeated id overwrite the one already on screen.
        const oldFilters = state.dashboard?.filters || [];
        const oldIds = new Set(oldFilters.map(f => f.id).filter(Boolean));
        const newFilters = (payload.dashboard.filters || []).filter(f => !f.id || !oldIds.has(f.id));
        payload.dashboard.filters = [...oldFilters, ...newFilters];
      } else {
        state.activeFilters = {}; state.crossFilterDefs = {}; state.filterRefreshWarning = null;
      }
      state.dashboard = ensureWidgetMeta(payload.dashboard);
      state.editHistory = { past: [], future: [] };
      state.currentHistoryId = null;
      // A brand-new answer, never a filter/edit re-render — the next renderDashboard()
      // call plays the entrance animation (see revealDashboardOnNextRender's own comment).
      revealDashboardOnNextRender = true;
      // A chat answer always lands as a fresh Draft (per Part 2: "any question's result
      // goes to History as a frozen snapshot") — even when it continues an Active
      // dashboard, so the Active one's own state no longer describes what's on screen now.
      state.dashboardIsActive = false; state.dashboardRole = null;
      state.dashboardDisabled = false; state.dashboardDisabledReason = '';
      // narration is the rich, read-aloud-friendly walkthrough (see AnalyticsTools'
      // "الشرح السردي" system-prompt section) — distinct from the short summary (used
      // elsewhere: continuation context, history list) and from each widget's technical
      // source. It's what the chat bubble shows and what 🔊/auto-read speaks.
      const speechWidgets = payload.dashboard.widgets.map(w => ({ id: w.id, title: w.title }));
      const narration = payload.dashboard.narration || payload.dashboard.summary;
      state.messages.push({ role: 'bot', text: narration, speechWidgets });
      saveToHistory(question, payload.dashboard);
      if (state.autoRead) speakText(narration, speechWidgets);
      // The user's already-applied filter selections carried over too (above) — re-apply
      // them now so a widget the continuation just added or changed reflects them
      // immediately, instead of showing unfiltered data until the next filter click.
      if (isContinuation && getActiveFilters().length) await applyFilters();
    }
  } catch (err) {
    state.messages.push({ role: 'bot', text: 'تعذّر الوصول إلى الخادم: ' + err.message, error: true });
  } finally {
    state.loading = false; el.send.disabled = false;
    renderMessages(); renderDashboard();
  }
}

// ---------- الاستفسارات (Inquiries): a conversational, saved-history sibling of ask() above ----------
// Two small sub-tabs inside the same side chat panel — never a top-level screen, and the
// dashboard area (#dash-pane) is never hidden or cleared by switching between them. Isolated
// from state.dashboard/state.messages entirely: an Inquiries question can never affect the
// dashboard currently on screen — only "🔄 حوّله لداشبورد" below ever bridges the two, and only
// on explicit click. Every conversation is saved server-side as it goes (see
// Inquiry/ConversationStore.cs) — state.inquiryTurns mirrors whichever one is currently open.
function setChatMode(mode) {
  state.chatMode = mode;
  document.querySelectorAll('.chat-mode-tab').forEach(t => t.classList.toggle('active', t.dataset.mode === mode));
  el('messages').classList.toggle('hidden', mode !== 'dashboard');
  el('inquiry-messages').classList.toggle('hidden', mode !== 'inquiry');
  el('inquiry-toolbar').classList.toggle('hidden', mode !== 'inquiry');
  // Attaching a dashboard screenshot and "ابدأ لوحة جديدة" only make sense for بناء اللوحة.
  el('dash-image-attach-label').classList.toggle('hidden', mode !== 'dashboard');
  el('new-dashboard-btn').classList.toggle('hidden', mode !== 'dashboard');
  el('q').placeholder = mode === 'inquiry'
    ? 'اسأل استفسارًا عن بياناتك، أو تابع نقاشًا سابقًا — إجابة نصية مباشرة، من غير بناء لوحة…'
    : 'اسأل عن بياناتك، أو أرفق صورة داشبورد لإعادة بنائه…';
  // Composer is a single rounded pill in both modes now, with a circular icon-only send button.
  el('send').innerHTML = '<span class="send-icon" aria-hidden="true">➤</span>';
  el('send').setAttribute('aria-label', 'إرسال');
  if (mode === 'inquiry') {
    renderInquiryMessages();
    if (!state.inquiryConversationsLoaded) loadInquiryConversations();
  } else renderMessages();
}
document.querySelectorAll('.chat-mode-tab').forEach(tab =>
  tab.addEventListener('click', () => { if (!state.loading) setChatMode(tab.dataset.mode); }));

// Turns a block's plain-text answer into real <p>/<ol>/<ul> markup instead of one run-in
// paragraph — a "1) ... 2) ..." or "- ..." line sequence in the model's text becomes an actual
// list, everything else stays a paragraph. Always escapes first; never trusts model text as HTML.
function renderInquiryBlockText(text) {
  const lines = String(text || '').split('\n');
  const numbered = /^\s*\d+[.)]\s+(.*)$/;
  const bulleted = /^\s*[-•]\s+(.*)$/;
  const out = [];
  let i = 0;
  while (i < lines.length) {
    if (numbered.test(lines[i]) || bulleted.test(lines[i])) {
      const isNumbered = numbered.test(lines[i]);
      const re = isNumbered ? numbered : bulleted;
      const items = [];
      while (i < lines.length && re.test(lines[i])) {
        items.push(`<li>${esc(lines[i].match(re)[1].trim())}</li>`);
        i++;
      }
      out.push(`<${isNumbered ? 'ol' : 'ul'}>${items.join('')}</${isNumbered ? 'ol' : 'ul'}>`);
    } else {
      const para = [];
      while (i < lines.length && !numbered.test(lines[i]) && !bulleted.test(lines[i])) {
        para.push(lines[i]); i++;
      }
      const joined = para.join('\n').trim();
      if (joined) out.push(`<p>${esc(joined)}</p>`);
    }
  }
  return out.join('');
}

// Tracks which block (if any) currently shows the inline "add to / replace the displayed
// dashboard?" card — see convertInquiryToDashboard below. null when no card is showing.
let pendingInquiryConvert = null;

function inquiryBlockHtml(block, turnIndex, blockIndex) {
  if (block.masked) {
    return `<div class="inquiry-block masked">
        <div class="inquiry-block-head"><span class="inquiry-block-label">🔒 من بيانات الجهة</span></div>
        <p class="inquiry-block-text">${esc(block.text)}</p>
        <p class="inquiry-block-subtext">لا يمكن عرضه أو تحويله إلى لوحة.</p>
      </div>`;
  }
  const isData = block.kind === 'data';
  if (!isData) {
    return `<div class="inquiry-block knowledge">
        <div class="inquiry-block-head">
          <span class="inquiry-block-label">💡 من خارج البيانات</span>
          <span class="inquiry-block-hint">معرفة عامة، لا تعتمد على بيانات الجهة</span>
        </div>
        <div class="inquiry-block-text">${renderInquiryBlockText(block.text)}</div>
      </div>`;
  }
  const dateHtml = block.extractedAt
    ? `<span class="inquiry-block-date">بيانات بتاريخ ${new Date(block.extractedAt).toLocaleDateString('ar-EG', { day: 'numeric', month: 'long', year: 'numeric' })}</span>`
    : '';
  const examinedHtml = block.examined && block.examined.length
    ? `<div class="inquiry-examined"><span class="inquiry-examined-label">تم فحص:</span>
        ${block.examined.map(s => `<span class="inquiry-chip">${esc(s)}</span>`).join('')}
      </div>` : '';
  const pending = pendingInquiryConvert;
  const isPending = pending && pending.turnIndex === turnIndex && pending.blockIndex === blockIndex;
  const promptHtml = isPending ? `
      <div class="inquiry-convert-prompt">
        <p>توجد لوحة معروضة الآن. كيف تريد إضافة هذه النتيجة؟</p>
        <div class="inquiry-convert-prompt-actions">
          <button type="button" class="icp-add" data-turn="${turnIndex}" data-block="${blockIndex}">إضافة إلى اللوحة</button>
          <button type="button" class="icp-replace" data-turn="${turnIndex}" data-block="${blockIndex}">استبدال اللوحة</button>
          <button type="button" class="icp-cancel">إلغاء</button>
        </div>
      </div>` : '';
  return `<div class="inquiry-block data">
      <div class="inquiry-block-head">
        <span class="inquiry-block-label">🗄️ من بيانات الجهة</span>
        ${dateHtml}
      </div>
      <div class="inquiry-block-text">${renderInquiryBlockText(block.text)}</div>
      ${examinedHtml}
      <div class="inquiry-block-footer">
        <button type="button" class="inquiry-src-btn"><span aria-hidden="true">ⓘ</span> المصدر</button>
        <button type="button" class="inquiry-convert-btn" data-turn="${turnIndex}" data-block="${blockIndex}">حوّله لداشبورد</button>
        ${sourcePopover({ source: block.source })}
      </div>
      ${promptHtml}
    </div>`;
}

function renderInquiryMessages() {
  const lastBotIndex = (() => {
    for (let i = state.inquiryTurns.length - 1; i >= 0; i--) {
      if (state.inquiryTurns[i].role !== 'user') return i;
    }
    return -1;
  })();
  const items = state.inquiryTurns.map((t, ti) => {
    if (t.role === 'user') return `<div class="msg user">${esc(t.text)}</div>`;
    if (t.error) return `<div class="msg err">${esc(t.text)}</div>`;
    // Follow-up chips only ever sit under the LATEST reply — asking one re-sends it as the
    // next question (see askInquiry below); the array is only ever populated server-side
    // when Inquiry:SuggestFollowUps is on (see AnalyticsTools.BuildInquirySystemPrompt).
    const followUps = ti === lastBotIndex && t.followUps && t.followUps.length
      ? `<div class="inquiry-followups">
          ${t.followUps.map(f => `<button type="button" class="inquiry-followup-chip">${esc(f)}</button>`).join('')}
        </div>` : '';
    return `<div class="msg bot inquiry-answer">
        ${(t.blocks || []).map((b, bi) => inquiryBlockHtml(b, ti, bi)).join('')}
        ${followUps}
      </div>`;
  }).join('');
  const typing = state.loading && state.chatMode === 'inquiry'
    ? `<div class="msg bot dots"><span></span><span></span><span></span></div>` : '';
  const hint = state.inquiryTurns.length === 0 && !(state.loading && state.chatMode === 'inquiry')
    ? `<div class="hint">اسأل استفسارًا وسيجاوبك المساعد بإجابة نصية مباشرة، من غير بناء لوحة — يقدر يتذكر
         الأسئلة اللي قبله في نفس المحادثة، وتقدر ترجع لها لاحقًا من «📂 المحادثات».
         <span class="ex">جرّب: «كام إجمالي الإيرادات الشهر ده؟»</span></div>` : '';
  const box = el('inquiry-messages');
  box.innerHTML = hint + items + typing;
  box.scrollTop = box.scrollHeight;
}
el('inquiry-messages').addEventListener('click', e => {
  const srcBtn = e.target.closest('.inquiry-src-btn');
  if (srcBtn) {
    e.stopPropagation();
    const pop = srcBtn.closest('.inquiry-block').querySelector('.src-pop');
    const open = pop.classList.contains('open');
    closePopovers();
    if (!open) { pop.classList.add('open'); srcBtn.classList.add('active'); }
    return;
  }
  const convertBtn = e.target.closest('.inquiry-convert-btn');
  if (convertBtn) { convertInquiryToDashboard(Number(convertBtn.dataset.turn), Number(convertBtn.dataset.block)); return; }
  const addBtn = e.target.closest('.icp-add');
  if (addBtn) { pendingInquiryConvert = null; runInquiryConvert(Number(addBtn.dataset.turn), Number(addBtn.dataset.block), true); return; }
  const replaceBtn = e.target.closest('.icp-replace');
  if (replaceBtn) { pendingInquiryConvert = null; runInquiryConvert(Number(replaceBtn.dataset.turn), Number(replaceBtn.dataset.block), false); return; }
  const cancelBtn = e.target.closest('.icp-cancel');
  if (cancelBtn) { pendingInquiryConvert = null; renderInquiryMessages(); return; }
  const chip = e.target.closest('.inquiry-followup-chip');
  if (chip) { if (!state.loading) askInquiry(chip.textContent); }
});

async function askInquiry(question) {
  const userTurn = { role: 'user', text: question, createdAt: new Date().toISOString() };
  state.inquiryTurns.push(userTurn);
  state.loading = true;
  renderInquiryMessages(); el.send.disabled = true;

  try {
    const res = await fetch('/api/inquiry', {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        conversationId: state.inquiryConversationId,
        message: question,
        // Same on/off source toggle the dashboard-building chat uses — the exact same
        // tool-use flow and gating, just a different final response shape.
        sources: { systems: [...state.onSystems], files: [...state.onFiles] },
        lang: state.lang,
      }),
    });
    const payload = await res.json().catch(() => null);
    if (!res.ok || !payload?.turn) {
      state.inquiryTurns.push({ role: 'bot', text: payload?.error || `تعذّر تنفيذ الطلب (${res.status})`, error: true });
    } else {
      state.inquiryTurns.push(payload.turn);
      const isNew = !state.inquiryConversationId;
      state.inquiryConversationId = payload.conversationId;
      // Patch the sidebar list in place instead of a round trip: a new conversation is
      // inserted at the top, an existing one is bumped there with a fresh timestamp — same
      // "most recently touched first" order the server's own ORDER BY UpdatedAt gives.
      if (isNew && payload.title) {
        state.inquiryConversations.unshift({ id: payload.conversationId, title: payload.title, updatedAt: new Date().toISOString() });
      } else {
        const entry = state.inquiryConversations.find(c => c.id === payload.conversationId);
        if (entry) {
          entry.updatedAt = new Date().toISOString();
          state.inquiryConversations = [entry, ...state.inquiryConversations.filter(c => c !== entry)];
        }
      }
    }
  } catch (err) {
    state.inquiryTurns.push({ role: 'bot', text: 'تعذّر الوصول إلى الخادم: ' + err.message, error: true });
  } finally {
    state.loading = false; el.send.disabled = false;
    renderInquiryMessages();
  }
}

// ---------- saved conversations (📂 المحادثات) ----------
async function loadInquiryConversations() {
  try {
    const res = await fetch('/api/inquiry/conversations');
    state.inquiryConversations = res.ok ? await res.json() : [];
  } catch { state.inquiryConversations = []; }
  state.inquiryConversationsLoaded = true;
  renderInquiryConvPanel();
}

function renderInquiryConvPanel() {
  const panel = el('inquiry-conv-panel');
  if (!state.inquiryConversations.length) {
    panel.innerHTML = `<div class="inquiry-conv-empty">لا توجد محادثات محفوظة بعد.</div>`;
    return;
  }
  panel.innerHTML = state.inquiryConversations.map(c => `
    <div class="inquiry-conv-item ${c.id === state.inquiryConversationId ? 'active' : ''}" data-id="${esc(c.id)}">
      <span class="inquiry-conv-title">${esc(c.title)}</span>
      <span class="inquiry-conv-date">${relTime(c.updatedAt)}</span>
      <button type="button" class="inquiry-conv-del" data-id="${esc(c.id)}" title="حذف" aria-label="حذف المحادثة">✕</button>
    </div>`).join('');
}
el('inquiry-conv-btn').addEventListener('click', e => {
  e.stopPropagation();
  const panel = el('inquiry-conv-panel');
  const open = !panel.classList.contains('hidden');
  if (open) { panel.classList.add('hidden'); return; }
  renderInquiryConvPanel();
  panel.classList.remove('hidden');
});
el('inquiry-conv-panel').addEventListener('click', e => e.stopPropagation());
document.addEventListener('click', () => el('inquiry-conv-panel')?.classList.add('hidden'));

el('inquiry-conv-panel').addEventListener('click', async e => {
  const delBtn = e.target.closest('.inquiry-conv-del');
  if (delBtn) {
    const id = delBtn.dataset.id;
    if (!confirm('حذف هذه المحادثة؟')) return;
    await fetch('/api/inquiry/conversations/' + id, { method: 'DELETE' });
    state.inquiryConversations = state.inquiryConversations.filter(c => c.id !== id);
    if (state.inquiryConversationId === id) startNewInquiryConversation();
    renderInquiryConvPanel();
    return;
  }
  const item = e.target.closest('.inquiry-conv-item');
  if (item) openInquiryConversation(item.dataset.id);
});

el('inquiry-new-btn').addEventListener('click', startNewInquiryConversation);
function startNewInquiryConversation() {
  state.inquiryConversationId = null;
  state.inquiryTurns = [];
  renderInquiryMessages();
  el('inquiry-conv-panel').classList.add('hidden');
}

async function openInquiryConversation(id) {
  if (state.loading) return;
  state.loading = true; renderInquiryMessages();
  try {
    const res = await fetch('/api/inquiry/conversations/' + id);
    const payload = await res.json().catch(() => null);
    if (!res.ok || !payload) { alert('تعذّر فتح هذه المحادثة.'); return; }
    state.inquiryConversationId = payload.id;
    state.inquiryTurns = payload.turns || [];
  } catch (err) {
    alert('تعذّر الوصول إلى الخادم: ' + err.message);
  } finally {
    state.loading = false;
    renderInquiryMessages();
    el('inquiry-conv-panel').classList.add('hidden');
  }
}

// "حوّله لداشبورد": never re-runs the question through the tool-calling loop — the data
// this one block already produced is handed to a single, tool-free call that only reshapes
// it into widgets (see GenerateDashboardFromInquiryAsync). If a dashboard is already on
// screen, shows an inline "add or replace?" card right under the block (pendingInquiryConvert,
// rendered by inquiryBlockHtml — its icp-add/icp-replace/icp-cancel clicks are handled in the
// inquiry-messages delegated listener above); if nothing is on screen yet, builds a fresh one
// directly — same as choosing "استبدال" would.
async function convertInquiryToDashboard(turnIndex, blockIndex) {
  if (state.loading) return;
  const block = state.inquiryTurns[turnIndex]?.blocks?.[blockIndex];
  if (!block) return;
  if (state.dashboard?.widgets?.length) {
    pendingInquiryConvert = { turnIndex, blockIndex };
    renderInquiryMessages();
    return;
  }
  await runInquiryConvert(turnIndex, blockIndex, false);
}

async function runInquiryConvert(turnIndex, blockIndex, addToExisting) {
  const block = state.inquiryTurns[turnIndex]?.blocks?.[blockIndex];
  if (!block) return;
  // Same "continue the dashboard on screen" contract as ask()'s own continuation — only sent
  // when the user explicitly chose "إضافة"; "استبدال" (or nothing on screen yet) sends none,
  // which the backend treats identically to a brand-new dashboard (see ConvertInquiryBlockRequest).
  const currentDashboard = addToExisting && state.dashboard?.widgets?.length
    ? { summary: state.dashboard.summary, widgets: state.dashboard.widgets }
    : null;
  const isContinuation = !!currentDashboard;
  state.loading = true;
  renderInquiryMessages();

  try {
    const res = await fetch('/api/inquiry/convert', {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        conversationId: state.inquiryConversationId, turnIndex, blockIndex, currentDashboard,
        sources: { systems: [...state.onSystems], files: [...state.onFiles] },
        lang: state.lang,
      }),
    });
    const payload = await res.json().catch(() => null);
    if (!res.ok || !payload?.dashboard) {
      alert(payload?.error || `تعذّر تحويل هذا الجزء إلى لوحة (${res.status})`);
      return;
    }

    if (isContinuation) {
      // Same safety net ask() applies on its own continuation path: a smaller/local model
      // can silently drop an existing widget despite the system prompt's copy-forward
      // instruction. Match by title (widgets have no stable id the model echoes back) and
      // restore anything missing, then carry forward existing filters the same way.
      const oldWidgets = state.dashboard?.widgets || [];
      const newWidgets = payload.dashboard.widgets || [];
      if (oldWidgets.length && newWidgets.length < oldWidgets.length) {
        const newTitles = new Set(newWidgets.map(w => w.title));
        const missing = oldWidgets.filter(w => !newTitles.has(w.title));
        payload.dashboard.widgets = [...missing, ...newWidgets];
      }

      const oldFilters = state.dashboard?.filters || [];
      const oldIds = new Set(oldFilters.map(f => f.id).filter(Boolean));
      const newFilters = (payload.dashboard.filters || []).filter(f => !f.id || !oldIds.has(f.id));
      payload.dashboard.filters = [...oldFilters, ...newFilters];
    } else {
      state.activeFilters = {}; state.crossFilterDefs = {}; state.filterRefreshWarning = null;
    }
    state.dashboard = ensureWidgetMeta(payload.dashboard);
    state.editHistory = { past: [], future: [] };
    state.currentHistoryId = null;
    state.dashboardIsActive = false; state.dashboardRole = null;
    state.dashboardDisabled = false; state.dashboardDisabledReason = '';

    const speechWidgets = payload.dashboard.widgets.map(w => ({ id: w.id, title: w.title }));
    const narration = payload.dashboard.narration || payload.dashboard.summary;
    state.messages.push({ role: 'bot', text: narration, speechWidgets });
    saveToHistory(block.text, payload.dashboard);
    if (state.autoRead) speakText(narration, speechWidgets);
    if (isContinuation && getActiveFilters().length) await applyFilters();
    setChatMode('dashboard');
  } catch (err) {
    alert('تعذّر الوصول إلى الخادم: ' + err.message);
  } finally {
    state.loading = false;
    renderInquiryMessages(); renderMessages(); renderDashboard();
  }
}

// ---------- sources ----------
async function loadSources() {
  const res = await fetch('/api/sources');
  const data = await res.json();
  const knownSystems = new Set(state.systems.map(s => s.id));
  const knownFiles = new Set(state.sourceFiles.map(f => f.id));

  // An external integration is offered as just another toggleable "system" — same shape
  // ({id, name, connected}), same onSystems set, same "systems" list sent back with every
  // chat request (see SourceSelection.AllowsSystem, reused verbatim for integration ids).
  state.systems = [...(data.systems || []), ...(data.integrations || [])];
  state.sourceFiles = data.files || [];
  state.tableLabels = data.tableLabels || {};

  // Newly appearing sources start enabled; the user's explicit choices are preserved.
  state.systems.forEach(s => { if (!knownSystems.has(s.id)) state.onSystems.add(s.id); });
  state.sourceFiles.forEach(f => { if (!knownFiles.has(f.id)) state.onFiles.add(f.id); });
  const currentFileIds = new Set(state.sourceFiles.map(f => f.id));
  [...state.onFiles].forEach(id => { if (!currentFileIds.has(id)) state.onFiles.delete(id); });

  renderSources();
}

function renderSources() {
  el('system-toggles').innerHTML = state.systems.map(s => `
    <label class="source-item">
      <input type="checkbox" class="sys-cb" data-id="${esc(s.id)}" ${state.onSystems.has(s.id) ? 'checked' : ''}>
      <span class="grow">${esc(s.name)}</span>
      ${s.kind === 'integration' ? '<span class="note">🔗 تكامل خارجي</span>' : (s.connected ? '' : '<span class="note">غير مربوط بعد</span>')}
      ${s.refreshable ? `<button type="button" class="refresh-btn" data-refresh="${esc(s.id)}">⟳ جلب</button>` : ''}
    </label>
    ${s.refreshable ? `<div class="sys-status${s.error ? ' bad' : ''}" data-status="${esc(s.id)}"${s.error ? ` title="${esc(s.error)}"` : ''}>${systemStatusText(s)}</div>` : ''}`).join('');

  // Each repository file is its own independent source, at the same level as a system —
  // not grouped by category (a small category hint still shown alongside the name, purely
  // informational, since it's a real descriptive attribute of the file).
  el('file-toggles').innerHTML = state.sourceFiles.length
    ? state.sourceFiles.map(f => `
        <label class="source-item">
          <input type="checkbox" class="file-cb" data-id="${esc(f.id)}" ${state.onFiles.has(f.id) ? 'checked' : ''}>
          <span class="grow">${esc(f.name)}</span>
          <span class="note">${esc(f.category)}</span>
        </label>`).join('')
    : '<div class="nested-empty">لا يوجد ملفات بعد — ارفع ملفات أولاً</div>';

  el('system-toggles').querySelectorAll('[data-refresh]').forEach(btn =>
    btn.addEventListener('click', e => { e.preventDefault(); e.stopPropagation(); refreshSystem(btn); }));

  el('system-toggles').querySelectorAll('.sys-cb').forEach(cb => cb.addEventListener('change', () => {
    cb.checked ? state.onSystems.add(cb.dataset.id) : state.onSystems.delete(cb.dataset.id);
    syncSourceHeaders();
  }));
  el('file-toggles').querySelectorAll('.file-cb').forEach(cb => cb.addEventListener('change', () => {
    cb.checked ? state.onFiles.add(cb.dataset.id) : state.onFiles.delete(cb.dataset.id);
    syncSourceHeaders();
  }));
  syncSourceHeaders();
}

function systemStatusText(s) {
  // The raw fetch error (often a .NET exception message, in English) goes in the row's
  // title tooltip instead of inline — dumping it into the list directly mixed English
  // technical text into the Arabic RTL flow and made the row balloon to several lines.
  if (s.error) return 'تعذّر الجلب من النظام — مرّر المؤشر لعرض السبب';
  if (!s.lastRefreshed) return 'لم يتم الجلب بعد — اضغط «جلب»';
  const when = new Date(s.lastRefreshed + (s.lastRefreshed.endsWith('Z') ? '' : 'Z'));
  return `${s.records.toLocaleString('en-US')} سجل · آخر جلب ${when.toLocaleTimeString('ar-EG')}`;
}

async function refreshSystem(btn) {
  const id = btn.dataset.refresh;
  const status = el('system-toggles').querySelector(`[data-status="${CSS.escape(id)}"]`);
  btn.disabled = true; btn.textContent = '⟳ جارٍ الجلب…';
  if (status) { status.className = 'sys-status'; status.textContent = 'جارٍ الجلب من النظام…'; }
  try {
    const res = await fetch(`/api/sources/${encodeURIComponent(id)}/refresh`, { method: 'POST' });
    const payload = await res.json().catch(() => ({}));
    if (!res.ok) throw new Error(payload.error || `فشل الجلب (${res.status})`);
    await loadSources();
  } catch (err) {
    if (status) {
      status.className = 'sys-status bad';
      status.textContent = 'تعذّر الجلب من النظام — مرّر المؤشر لعرض السبب';
      status.title = err.message;
    }
    btn.disabled = false; btn.textContent = '⟳ جلب';
  }
}

function syncSourceHeaders() {
  el('sys-all').checked = state.systems.length > 0 && state.onSystems.size === state.systems.length;
  el('file-all').checked = state.sourceFiles.length > 0 && state.onFiles.size === state.sourceFiles.length;
  const total = state.systems.length + state.sourceFiles.length;
  const on = state.onSystems.size + state.onFiles.size;
  el('sources-label').textContent =
    total === 0 ? 'لا يوجد مصادر'
    : on === 0 ? 'لا يوجد مصدر مفعّل'
    : on === total ? `كل المصادر مفعّلة (${total})`
    : `${on} من ${total} مصادر مفعّلة`;
}

el('sources-btn').addEventListener('click', e => {
  e.stopPropagation();
  el('sources-panel').classList.toggle('hidden');
});
el('sources-panel').addEventListener('click', e => e.stopPropagation());
document.addEventListener('click', () => el('sources-panel').classList.add('hidden'));

el('sys-all').addEventListener('change', e => {
  state.onSystems = e.target.checked ? new Set(state.systems.map(s => s.id)) : new Set();
  renderSources();
});
el('file-all').addEventListener('change', e => {
  state.onFiles = e.target.checked ? new Set(state.sourceFiles.map(f => f.id)) : new Set();
  renderSources();
});

// ---------- file repository ----------
async function loadFiles() {
  const res = await fetch('/api/repository/files');
  state.files = await res.json();
  const knownSourceFileIds = new Set(state.sourceFiles.map(f => f.id));
  if (!state.sourceFiles.length || state.files.some(f => !knownSourceFileIds.has(f.id)))
    await loadSources();
  // Only an Admin can manage per-file permissions (same authority level as managing user
  // accounts) and only an Admin's browser is even allowed to call GET /api/users — load it
  // here (not eagerly for everyone) so the row's access column can show real initials for
  // whoever configured this file's permission list.
  if (state.currentUser?.role === 'Admin' && !state.users.length) await loadUsers();
  renderRepo();
}

function repoCategories() {
  return [...new Set(state.files.map(f => f.category || 'عام'))];
}

// "قبل 3 أيام" / "اليوم" / "منذ 5 أشهر" — the same relative-time granularity the design
// mockup uses for "آخر تحديث" and the "لم يُستخدم منذ..." staleness hint.
function relTime(dateStr) {
  const diffDays = Math.floor((Date.now() - new Date(dateStr).getTime()) / 86400000);
  if (state.lang === 'en') {
    if (diffDays <= 0) return 'Today';
    if (diffDays === 1) return 'Yesterday';
    if (diffDays < 30) return `${diffDays} day${diffDays === 1 ? '' : 's'} ago`;
    const months = Math.floor(diffDays / 30);
    return months <= 1 ? 'A month ago' : `${months} months ago`;
  }
  if (diffDays <= 0) return 'اليوم';
  if (diffDays === 1) return 'أمس';
  if (diffDays < 30) return `قبل ${diffDays} يوم`;
  const months = Math.floor(diffDays / 30);
  return months <= 1 ? 'قبل شهر' : `منذ ${months} أشهر`;
}

function userInitials(userId) {
  const u = state.users.find(u => u.id === userId);
  return u ? (u.displayName || u.username).trim().slice(0, 2) : '؟';
}

function renderRepo() {
  el('repo-badge').textContent = state.files.length;
  el('repo-link-text').textContent = state.files.length
    ? `📁 ${state.files.length} ملف في المستودع`
    : '📁 لا يوجد ملفات في المستودع';

  const cats = ['الكل', ...repoCategories()];
  if (!cats.includes(state.filter)) state.filter = 'الكل';
  el('filters').innerHTML = cats.map(c =>
    `<button class="chip-old${c === state.filter ? ' active' : ''}" data-cat="${esc(c)}">${esc(c)}</button>`).join('');
  el('filters').querySelectorAll('.chip-old').forEach(chip => chip.addEventListener('click', () => {
    state.filter = chip.dataset.cat; renderRepo();
  }));

  el('cat-suggestions').innerHTML = repoCategories().map(c => `<option value="${esc(c)}">`).join('');

  let visible = state.filter === 'الكل'
    ? [...state.files] : state.files.filter(f => (f.category || 'عام') === state.filter);

  const sort = el('repo-sort').value = state.repoSort || 'usage';
  visible.sort(sort === 'name' ? (a, b) => a.displayName.localeCompare(b.displayName, 'ar')
    : sort === 'updated' ? (a, b) => new Date(b.lastUpdatedAt) - new Date(a.lastUpdatedAt)
    : (a, b) => b.usageCount - a.usageCount);

  const STALE_DAYS = 90;
  el('repo-list').innerHTML = visible.length ? visible.map(f => {
    const icon = f.kind === 'pdf' ? '📄' : '📊';
    const staleDays = f.lastUsedAt ? Math.floor((Date.now() - new Date(f.lastUsedAt).getTime()) / 86400000) : null;
    const isStale = staleDays !== null && staleDays > STALE_DAYS;
    const staleBadge = isStale ? `<span class="badge-stale">⚠ ${relTime(f.lastUsedAt)}</span>` : '';
    const meta = f.kind === 'pdf' ? `${f.pageCount} صفحة` : `${f.rowCount} صف · ${f.columnCount} عمود`;
    const relLinks = f.relationships.length
      ? `<div class="fr-links">${f.relationships.map(r => `<span class="link-chip">🔗 ${esc(r.relatedFileName)}</span>`).join('')}</div>`
      : '';
    // Every file is automatically restricted to its creator now (see
    // AnalyticsTools.DescribeSourcesAsync) — never "public" — so this column always shows
    // something. An Admin sees the real avatar stack (creator + granted); anyone else just
    // sees whether *they themselves* currently have access, never other people's identities.
    const isAdmin = state.currentUser?.role === 'Admin';
    const isCreator = !!state.currentUser && f.createdByUserId === state.currentUser.id;
    const iHaveAccess = isAdmin || isCreator || (state.currentUser && f.permittedUserIds.includes(state.currentUser.id));
    const access = isAdmin
      ? `<div class="avatar-stack">
          <div class="avatar owner" title="${esc(f.createdByUserId ? userInitials(f.createdByUserId) : '')}">${esc(userInitials(f.createdByUserId))}</div>
          ${f.permittedUserIds.slice(0, 2).map(id => `<div class="avatar" title="${esc(userInitials(id))}">${esc(userInitials(id))}</div>`).join('')}
          ${f.permittedUserIds.length > 2 ? `<div class="avatar">+${f.permittedUserIds.length - 2}</div>` : ''}
        </div>`
      : (iHaveAccess ? `<span class="access-locked ok">✅ ${isCreator ? 'ملفك' : 'لديك صلاحية'}</span>` : `<span class="access-locked">🔒 مقيّد</span>`);
    const canManage = isAdmin || isCreator;
    const permBtn = canManage
      ? `<button class="icon-btn perm-btn" data-id="${esc(f.id)}" title="صلاحيات الملف">🔒</button>` : '';
    const delBtn = canManage
      ? `<button class="icon-btn del-btn" data-id="${esc(f.id)}" data-name="${esc(f.displayName)}" title="حذف الملف">🗑️</button>` : '';

    return `<div class="file-row${isStale ? ' stale' : ''}" data-id="${esc(f.id)}">
        <div class="fr-main">
          <div class="fr-icon">${icon}</div>
          <div class="fr-text">
            <div class="fr-line1">
              <span class="fr-name">${esc(f.displayName)}</span>
              <span class="cat-badge">${esc(f.category || 'عام')}</span>
              ${staleBadge}
            </div>
            <div class="fr-line2">${esc(f.description || meta)}${f.description ? ` <span class="sep">·</span> ${meta} <span class="sep">·</span> <span class="orig-name">${esc(f.originalFileName)}</span>` : ` <span class="sep">·</span> <span class="orig-name">${esc(f.originalFileName)}</span>`}</div>
            ${relLinks}
          </div>
        </div>
        <div class="col-updated"><b>${relTime(f.lastUpdatedAt)}</b>${new Date(f.lastUpdatedAt).toLocaleDateString('ar-EG')}</div>
        <div class="col-usage"><div class="num">${f.usageCount}</div><div class="lbl">لوحة</div></div>
        <div class="col-access">${access}</div>
        <div class="col-actions">
          <a class="icon-btn dl" href="/api/repository/files/${esc(f.id)}/download" title="تنزيل الملف">⬇️</a>
          <button class="icon-btn upd" data-id="${esc(f.id)}" title="تحديث البيانات">🔄</button>
          <button class="icon-btn cfg" data-id="${esc(f.id)}" title="التفاصيل">✏️</button>
          ${permBtn}
          <button class="icon-btn rel-btn" data-id="${esc(f.id)}" title="الملفات المرتبطة">🔗</button>
          ${delBtn}
        </div>
      </div>`;
  }).join('')
    : `<div class="repo-empty">لا يوجد ملفات ${state.filter !== 'الكل' ? 'في هذا التصنيف' : 'في المستودع بعد'}. ارفع ملفاً من الأعلى.</div>`;

  el('repo-list').querySelectorAll('.upd').forEach(btn => btn.addEventListener('click', () => openUpdateFilePicker(btn.dataset.id)));
  el('repo-list').querySelectorAll('.cfg').forEach(btn => btn.addEventListener('click', () => openFileModal(btn.dataset.id)));
  el('repo-list').querySelectorAll('.perm-btn').forEach(btn => btn.addEventListener('click', () => openFilePermModal(btn.dataset.id)));
  el('repo-list').querySelectorAll('.rel-btn').forEach(btn => btn.addEventListener('click', () => openFileRelModal(btn.dataset.id)));
  el('repo-list').querySelectorAll('.del-btn').forEach(btn => btn.addEventListener('click', () => deleteFile(btn.dataset.id, btn.dataset.name)));

  renderPending();
}

el('repo-sort').addEventListener('change', () => { state.repoSort = el('repo-sort').value; renderRepo(); });

function renderPending() {
  // p.loading is the transient placeholder pushed the moment files are picked, before the
  // server has actually finished uploading+parsing them (see the file-input change handler
  // below) — no inputs, no save button at all during this state, so there's nothing to click
  // before the real PendingUpload (with a real token) replaces it.
  el('pending').innerHTML = state.pending.map((p, i) => p.loading
    ? `<div class="pending-card loading">
         <span class="spinner"></span><span class="name">${esc(p.name)}</span>
       </div>`
    : p.error
    ? `<div class="pending-card bad">
         <span>⚠️</span><span class="name">${esc(p.name)} — ${esc(p.error)}</span>
         <button class="cancel" data-idx="${i}">×</button>
       </div>`
    : `<div class="pending-card">
         <span>${p.kind === 'pdf' ? '📄' : '📊'}</span>
         <span class="name">${esc(p.name)} — ${p.kind === 'pdf' ? `${p.pageCount} صفحة` : `${p.rowCount} صف`}</span>
         <input placeholder="اسم العرض (إجباري)" value="${esc(p.name.replace(/\.[^.]+$/, ''))}" data-idx="${i}" data-field="displayName">
         <input placeholder="وصف مختصر" data-idx="${i}" data-field="description">
         <input list="cat-suggestions" placeholder="التصنيف (مثلاً: المالية)" value="عام" data-idx="${i}" data-field="category">
         <button class="save" data-idx="${i}">حفظ في المستودع</button>
         <button class="cancel" data-idx="${i}">×</button>
       </div>`).join('');

  el('pending').querySelectorAll('.save').forEach(btn => btn.addEventListener('click', async () => {
    const i = +btn.dataset.idx;
    const field = f => el('pending').querySelector(`input[data-idx="${i}"][data-field="${f}"]`)?.value || '';
    const displayName = field('displayName').trim();
    if (!displayName) { alert('اسم العرض إجباري.'); return; }
    const category = (field('category') || 'عام').trim() || 'عام';
    const description = field('description').trim();
    btn.disabled = true; btn.textContent = 'جارٍ الحفظ…';
    const res = await fetch('/api/repository/files', {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ token: state.pending[i].token, displayName, description, category }),
    });
    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      alert('تعذّر الحفظ: ' + (err.error || res.status));
      btn.disabled = false; btn.textContent = 'حفظ في المستودع';
      return;
    }
    state.pending.splice(i, 1);
    await loadFiles(); await loadSources();
  }));

  el('pending').querySelectorAll('.cancel').forEach(btn => btn.addEventListener('click', async () => {
    const i = +btn.dataset.idx;
    const token = state.pending[i].token;
    if (token) await fetch('/api/repository/pending/' + token, { method: 'DELETE' });
    state.pending.splice(i, 1);
    renderPending();
  }));
}

// ---------- upload modal: the popup the repository page's upload button opens — file
// picking, per-file metadata, and the save-to-repository step all happen inside it, instead
// of inline on the page. Unsaved pending cards (state.pending) are left as-is on close, so
// reopening the modal shows them again ready to finish, exactly like before this was a modal. ----------
function openUploadModal() { el('upload-modal').classList.remove('hidden'); }
function closeUploadModal() { el('upload-modal').classList.add('hidden'); }
el('btn-open-upload-modal').addEventListener('click', openUploadModal);
el('upload-modal-close').addEventListener('click', closeUploadModal);
el('upload-modal').addEventListener('click', e => { if (e.target.id === 'upload-modal') closeUploadModal(); });

// A fresh, unguessable id per upload attempt, purely to correlate this browser tab's polling
// with UploadProgressTracker's server-side entry (see RepositoryController.UploadProgress) —
// not an auth token, never sent anywhere but this one upload.
function makeProgressToken() {
  return 'up_' + Date.now().toString(36) + '_' + Math.random().toString(36).slice(2);
}

// Polls the upload-progress endpoint every 1.2s while `body` (the upload POST) is still in
// flight, calling onUpdate(progress) whenever the server has something new to report — a
// second connection reading state the first is updating, not background processing: the
// polling stops the moment the POST settles, same as the request it's watching.
function pollUploadProgress(token, onUpdate) {
  return setInterval(async () => {
    try {
      const res = await fetch('/api/repository/upload-progress/' + token);
      if (res.status === 200) onUpdate(await res.json());
    } catch { /* a missed poll just means the next one tries again in 1.2s */ }
  }, 1200);
}

el('file-input').addEventListener('change', async e => {
  const files = [...e.target.files];
  if (!files.length) return;
  const form = new FormData();
  files.forEach(f => form.append('files', f));
  const progressToken = makeProgressToken();
  form.append('progressToken', progressToken);
  e.target.value = '';

  const pendingCard = { name: `جارٍ رفع ${files.length} ملف وتحليل محتواه…`, loading: true };
  state.pending.push(pendingCard);
  renderPending();

  const poll = pollUploadProgress(progressToken, p => {
    pendingCard.name = `جارٍ قراءة "${p.fileName}" بالموديل الداخلي — صفحة ${p.pagesRead} من ${p.totalPages}…`;
    renderPending();
  });
  try {
    const res = await fetch('/api/repository/upload', { method: 'POST', body: form });
    const parsed = await res.json();
    state.pending.pop();
    state.pending.push(...parsed);
  } catch (err) {
    state.pending.pop();
    state.pending.push({ name: 'تعذّر الرفع', error: err.message });
  } finally {
    clearInterval(poll);
  }
  renderPending();
});

// ---------- "Update this file": re-upload data into an existing file's identity ----------
// Display name/category/relationships/permissions are preserved server-side (see
// RepositoryStore.UpdateDataAsync) — this flow only ever needs the new file itself.
let updateTargetFileId = null;
function openUpdateFilePicker(fileId) {
  updateTargetFileId = fileId;
  el('file-update-input').click();
}
el('file-update-input').addEventListener('change', async e => {
  const file = e.target.files[0];
  e.target.value = '';
  if (!file || !updateTargetFileId) return;
  const targetId = updateTargetFileId;
  const row = el('repo-list').querySelector(`.file-row[data-id="${targetId}"] .upd`);
  if (row) { row.disabled = true; row.textContent = '⏳'; }
  const progressToken = makeProgressToken();
  const poll = pollUploadProgress(progressToken, p => {
    if (row) row.textContent = `⏳ ${p.pagesRead}/${p.totalPages}`;
  });
  try {
    const form = new FormData();
    form.append('files', file);
    form.append('progressToken', progressToken);
    const uploadRes = await fetch('/api/repository/upload', { method: 'POST', body: form });
    const [parsed] = await uploadRes.json();
    if (!parsed || parsed.error) throw new Error(parsed?.error || 'تعذّر قراءة الملف.');

    const updateRes = await fetch(`/api/repository/files/${targetId}/update`, {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ token: parsed.token }),
    });
    if (!updateRes.ok) {
      const err = await updateRes.json().catch(() => ({}));
      throw new Error(err.error || `فشل التحديث (${updateRes.status})`);
    }
    await loadFiles(); await loadSources();
  } catch (err) {
    alert('تعذّر تحديث بيانات الملف: ' + err.message);
    if (row) { row.disabled = false; row.textContent = '🔄'; }
  } finally {
    clearInterval(poll);
    updateTargetFileId = null;
  }
});

// ---------- file detail modal: meta only (permissions/relationships have their own
// dedicated modals — see openFilePermModal/openFileRelModal below) ----------
function openFileModal(fileId) {
  const f = state.files.find(f => f.id === fileId);
  if (!f) return;
  el('file-modal-id').value = f.id;
  el('file-modal-title').textContent = f.displayName;
  el('file-display-name').value = f.displayName;
  el('file-description').value = f.description;
  el('file-category').value = f.category;
  el('file-form-error').classList.add('hidden');
  el('file-modal').classList.remove('hidden');
}

// Shared by the row's own 🗑️ button and the "✏️ التفاصيل" modal's delete button below —
// same confirm + DELETE + refresh, so there's one place this logic lives. The backend only
// allows the file's creator or an Admin to actually delete it (same gate as its permissions);
// a 403 here means someone merely granted access to the file tried anyway.
async function deleteFile(fileId, displayName) {
  if (!confirm(`حذف "${displayName}" من المستودع؟ لا يمكن التراجع عن هذا.`)) return;
  const res = await fetch('/api/repository/files/' + fileId, { method: 'DELETE' });
  if (!res.ok) {
    alert(res.status === 403
      ? 'ليس لديك صلاحية حذف هذا الملف — الحذف مقصور على من رفعه أو على المسؤول.'
      : `تعذّر حذف الملف (${res.status}).`);
    return;
  }
  el('file-modal').classList.add('hidden');
  await loadFiles(); await loadSources();
}

el('file-modal-cancel').addEventListener('click', () => el('file-modal').classList.add('hidden'));
el('file-modal-delete').addEventListener('click', () => {
  const fileId = el('file-modal-id').value;
  const f = state.files.find(x => x.id === fileId);
  if (f) deleteFile(f.id, f.displayName);
});
el('file-modal').addEventListener('click', e => { if (e.target.id === 'file-modal') el('file-modal').classList.add('hidden'); });

el('file-form').addEventListener('submit', async e => {
  e.preventDefault();
  const fileId = el('file-modal-id').value;
  const displayName = el('file-display-name').value.trim();
  if (!displayName) return;
  const description = el('file-description').value.trim();
  const category = el('file-category').value.trim() || 'عام';
  const errBox = el('file-form-error'); errBox.classList.add('hidden');
  const saveBtn = el('file-modal-save'); saveBtn.disabled = true;

  try {
    const metaRes = await fetch(`/api/repository/files/${fileId}/meta`, {
      method: 'PUT', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ displayName, description, category }),
    });
    if (!metaRes.ok) throw new Error((await metaRes.json().catch(() => ({}))).error || 'تعذّر حفظ التفاصيل.');
    el('file-modal').classList.add('hidden');
    await loadFiles(); await loadSources();
  } catch (err) {
    errBox.textContent = err.message; errBox.classList.remove('hidden');
  } finally {
    saveBtn.disabled = false;
  }
});

// ---------- searchable user picker ----------
// Shared by the file-permissions modal and the dashboard-roles modal's "add role" and
// "transfer owner" pickers: GET /api/users (a full listing) is Admin-only, so a non-Admin
// caller has no way to enumerate everyone — only search a few matches at a time via the
// narrow /api/users/search endpoint (UserLookupController).
//
// Selection fires on mousedown, not click, with preventDefault() on it — this keeps the
// text input focused (a mousedown on another element would otherwise blur it first) and
// guarantees the pick registers before this same click can bubble to any outside-click
// handler that hides the suggestion box, rather than racing it.
function initUserSearchPicker(input, box, onPick) {
  let timer = null;
  input.addEventListener('input', () => {
    clearTimeout(timer);
    const q = input.value.trim();
    if (q.length < 2) { box.classList.add('hidden'); return; }
    timer = setTimeout(async () => {
      const res = await fetch(`/api/users/search?q=${encodeURIComponent(q)}`);
      const results = res.ok ? await res.json() : [];
      box.innerHTML = results.length
        ? results.map(u => `<div class="user-search-item" data-pick="${esc(u.id)}">
            <span>${esc(u.displayName)}</span><span class="uname">@${esc(u.username)}</span>
          </div>`).join('')
        : '<div class="user-search-empty">لا نتائج.</div>';
      box.classList.remove('hidden');
      box.querySelectorAll('[data-pick]').forEach(item => {
        item.addEventListener('mousedown', e => {
          e.preventDefault();
          const picked = results.find(u => u.id === item.dataset.pick);
          box.classList.add('hidden');
          if (picked) onPick(picked);
        });
      });
    }, 250);
  });
}
document.addEventListener('click', e => {
  document.querySelectorAll('.user-search-box').forEach(wrap => {
    if (!wrap.contains(e.target)) wrap.querySelector('.user-search-suggestions')?.classList.add('hidden');
  });
});

// ---------- file permissions modal ----------
// Every file is automatically restricted to its creator (see AnalyticsTools.
// DescribeSourcesAsync) — this can't be turned off, only added to.
function openFilePermModal(fileId) {
  el('file-perm-modal-id').value = fileId;
  el('file-perm-search').value = '';
  el('file-perm-suggestions').classList.add('hidden');
  el('file-perm-error').classList.add('hidden');
  el('file-perm-modal').classList.remove('hidden');
  reloadFilePermModal();
}

async function reloadFilePermModal() {
  const fileId = el('file-perm-modal-id').value;
  const res = await fetch(`/api/repository/files/${fileId}/permissions`);
  if (!res.ok) { el('file-perm-creator-name').textContent = '—'; return; }
  const data = await res.json();
  el('file-perm-creator-name').textContent = data.creatorName || data.creatorId || '—';
  el('file-perm-list').innerHTML = data.granted.length
    ? data.granted.map(g => `<div class="role-row">
        <span class="name">${esc(g.displayName)}</span>
        <button type="button" class="remove-role-btn" data-remove="${esc(g.userId)}">إزالة</button>
      </div>`).join('')
    : '<div class="owner-line">لا يوجد مستخدمون إضافيون بعد.</div>';
  el('file-perm-list').querySelectorAll('[data-remove]').forEach(btn =>
    btn.addEventListener('click', () => setFilePermGranted(data.granted.filter(g => g.userId !== btn.dataset.remove).map(g => g.userId))));
}

async function setFilePermGranted(userIds) {
  const fileId = el('file-perm-modal-id').value;
  const errBox = el('file-perm-error'); errBox.classList.add('hidden');
  const res = await fetch(`/api/repository/files/${fileId}/permissions`, {
    method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ userIds }),
  });
  if (!res.ok) { errBox.textContent = 'تعذّر حفظ الصلاحيات.'; errBox.classList.remove('hidden'); return; }
  await reloadFilePermModal();
  await loadFiles();
}

initUserSearchPicker(el('file-perm-search'), el('file-perm-suggestions'), async picked => {
  el('file-perm-error').classList.add('hidden');
  const fileId = el('file-perm-modal-id').value;
  const res = await fetch(`/api/repository/files/${fileId}/permissions`);
  if (!res.ok) {
    // A 404 here means the file itself is gone (deleted, or replaced by a data update that
    // gave it a new id) while this modal was still open with the old id — silently crashing
    // on it (the previous behavior: the suggestion box just closes with no explanation) left
    // no way to tell. Refresh the list so the stale row disappears too.
    el('file-perm-error').textContent = res.status === 404
      ? 'تعذّر العثور على هذا الملف — يبدو أنه حُذف أو تحدّث. أعد فتح صلاحياته من قائمة الملفات.'
      : 'تعذّر تحميل صلاحيات الملف الحالية.';
    el('file-perm-error').classList.remove('hidden');
    if (res.status === 404) await loadFiles();
    return;
  }
  const current = await res.json();
  const ids = current.granted.map(g => g.userId);
  if (!ids.includes(picked.id)) ids.push(picked.id);
  el('file-perm-search').value = '';
  await setFilePermGranted(ids);
});
el('file-perm-modal-close').addEventListener('click', () => el('file-perm-modal').classList.add('hidden'));
el('file-perm-modal').addEventListener('click', e => { if (e.target.id === 'file-perm-modal') el('file-perm-modal').classList.add('hidden'); });

// ---------- file relationships modal ----------
function openFileRelModal(fileId) {
  const f = state.files.find(x => x.id === fileId);
  if (!f) return;
  el('file-rel-modal-id').value = fileId;
  renderFileRelationships(f);
  const targets = state.files.filter(other => other.id !== f.id);
  el('file-rel-target').innerHTML = targets.length
    ? targets.map(t => `<option value="${esc(t.id)}">${esc(t.displayName)}</option>`).join('')
    : '<option value="">لا يوجد ملفات أخرى</option>';
  el('file-rel-column').value = '';
  el('file-rel-modal').classList.remove('hidden');
}

function renderFileRelationships(f) {
  el('file-rel-list').innerHTML = f.relationships.length
    ? f.relationships.map(r => `<div class="role-row">
        <span class="name">🔗 ${esc(r.relatedFileName)} — العمود المشترك: <b>${esc(r.sharedColumn)}</b></span>
        <button type="button" class="remove-role-btn rel-del" data-rel="${esc(r.id)}" title="إزالة الربط">إزالة</button>
      </div>`).join('')
    : '<div class="owner-line">لا يوجد ملفات مرتبطة بعد.</div>';

  el('file-rel-list').querySelectorAll('.rel-del').forEach(btn => btn.addEventListener('click', async () => {
    const fileId = el('file-rel-modal-id').value;
    await fetch(`/api/repository/files/${fileId}/relationships/${btn.dataset.rel}`, { method: 'DELETE' });
    await loadFiles();
    const refreshed = state.files.find(x => x.id === fileId);
    if (refreshed) renderFileRelationships(refreshed);
  }));
}

el('file-rel-add-btn').addEventListener('click', async () => {
  const fileId = el('file-rel-modal-id').value;
  const relatedFileId = el('file-rel-target').value;
  const sharedColumn = el('file-rel-column').value.trim();
  if (!relatedFileId || !sharedColumn) { alert('اختر ملفًا واكتب اسم العمود المشترك.'); return; }
  const res = await fetch(`/api/repository/files/${fileId}/relationships`, {
    method: 'POST', headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ relatedFileId, sharedColumn }),
  });
  if (!res.ok) { alert('تعذّر إضافة الربط.'); return; }
  el('file-rel-column').value = '';
  await loadFiles();
  const refreshed = state.files.find(x => x.id === fileId);
  if (refreshed) renderFileRelationships(refreshed);
});
el('file-rel-modal-close').addEventListener('click', () => el('file-rel-modal').classList.add('hidden'));
el('file-rel-modal').addEventListener('click', e => { if (e.target.id === 'file-rel-modal') el('file-rel-modal').classList.add('hidden'); });

// ---------- history ----------
// Fire-and-forget: saving history must never block or break the dashboard that
// already rendered. Only questions that produced at least one widget are saved.
function saveToHistory(question, dashboard) {
  if (!dashboard?.widgets?.length) { state.currentHistoryId = null; return; }
  fetch('/api/history', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      question, summary: dashboard.summary,
      queryDescription: dashboard.summary, widgets: dashboard.widgets,
      filters: dashboard.filters || [], activeFilters: state.activeFilters || {},
    }),
  }).then(res => res.ok ? res.json() : null).then(saved => {
    // Captured so a later editor edit updates this same row instead of creating a new one.
    state.currentHistoryId = saved?.id || null;
    state.historyLoaded = false;
  }).catch(() => {});
}

async function loadHistory() {
  const res = await fetch('/api/history');
  state.history = res.ok ? await res.json() : [];
  state.historyLoaded = true;
  renderHistory();
}

function renderHistory() {
  el('history-badge').textContent = state.history.length;
  el('history-count').textContent = state.history.length
    ? `${state.history.length} لوحة محفوظة`
    : 'لا يوجد لوحات محفوظة بعد';

  el('history-grid').innerHTML = state.history.length ? state.history.map(h => {
    const widgets = parseWidgets(h.widgetsJson);
    const when = new Date(h.createdAt + (h.createdAt.endsWith('Z') ? '' : 'Z'));
    const stateBadge = h.isActive
      ? `<span class="state-badge active">نشطة</span>`
      : `<span class="state-badge draft">مسودة</span>`;
    const roleBadge = h.isActive && h.myRole
      ? `<span class="role-badge${h.myRole === 'viewer' ? ' role-viewer' : ''}">${ROLE_LABELS[h.myRole] || h.myRole}</span>` : '';
    const disabledBadge = h.disabled ? `<span class="state-badge disabled-badge" title="${esc(h.disabledReason || '')}">⚠️ معطّلة</span>` : '';
    const ownerLine = h.isActive && h.ownerName ? `<div class="owner-line">المالك: ${esc(h.ownerName)}</div>` : '';
    // Only a Draft's own creator ever sees it (server already filters the list), so
    // "تفعيل" is always safe to show for one; deletion on an Active dashboard is
    // Owner/Admin-only server-side — hide it for an Editor/Viewer rather than let them
    // hit a 404.
    const activateBtn = !h.isActive ? `<button class="activate-btn" data-activate="${esc(h.id)}">تفعيل</button>` : '';
    const canDelete = !h.isActive || h.myRole === 'owner';
    const delBtn = canDelete ? `<button class="del-btn" data-del="${esc(h.id)}">حذف</button>` : '';
    // Roles/ownership management for an Active dashboard now lives on its own dedicated
    // page ("اللوحات النشطة") rather than duplicated here — see renderActiveDashboardsList().
    return `<div class="hist-card">
        <div class="badges">${stateBadge}${roleBadge}${disabledBadge}</div>
        <div class="q">${esc(h.question)}</div>
        <div class="summary">${esc(h.summary)}</div>
        ${ownerLine}
        <div class="meta">${when.toLocaleString('ar-EG')} · ${widgets.length} عنصر</div>
        <div class="actions">
          <button class="open-btn" data-open="${esc(h.id)}">فتح</button>
          ${activateBtn}${delBtn}
        </div>
      </div>`;
  }).join('') : `<div class="hist-empty">لا يوجد لوحات محفوظة بعد — أي سؤال تسأله يُحفظ هنا تلقائياً.</div>`;

  el('history-grid').querySelectorAll('[data-open]').forEach(btn =>
    btn.addEventListener('click', () => openHistoryEntry(btn.dataset.open)));
  el('history-grid').querySelectorAll('[data-del]').forEach(btn =>
    btn.addEventListener('click', () => deleteHistoryEntry(btn.dataset.del)));
  el('history-grid').querySelectorAll('[data-activate]').forEach(btn =>
    btn.addEventListener('click', () => activateHistoryEntry(btn.dataset.activate)));

  renderActiveDashboardsList();
}

// ---------- Active dashboards: mini SVG preview thumbnails ----------
// A fast, purely data-driven "poster" of a saved dashboard's widgets — not a literal
// screenshot, since that would mean spinning up a real ApexCharts instance for every
// widget of every active dashboard just to rasterize them, which doesn't scale to a list.
// Colors are the same small semantic vocabulary used everywhere else in this app (CSS
// custom properties, referenced live via style="fill:var(--x)"), so unlike the real
// ApexCharts-based charts (which bake static SVG colors and need a rebuild) this
// thumbnail re-themes itself automatically on the light/dark toggle — no extra code needed.
const THUMB_W = 240, THUMB_H = 128, THUMB_COLS = 4, THUMB_PAD = 6, THUMB_GAP = 5, THUMB_MAX_ROWS = 3;
const THUMB_TONES = ['var(--accent)', 'var(--amber)', 'var(--danger)'];

function thumbSpan(w) {
  if (w.comparison) return THUMB_COLS;
  const size = w.layout && w.layout.size;
  if (size === 'small') return 1;
  if (size === 'large') return 3;
  if (size === 'full') return THUMB_COLS;
  if (size === 'wide') return 2;
  return (w.type || '').toLowerCase() === 'kpi' ? 1 : 2;
}

function packThumbnailRows(widgets) {
  const rows = [];
  let current = [], currentSpan = 0;
  for (const w of widgets) {
    if (rows.length >= THUMB_MAX_ROWS) break;
    const span = thumbSpan(w);
    if (currentSpan && currentSpan + span > THUMB_COLS) {
      rows.push(current);
      if (rows.length >= THUMB_MAX_ROWS) { current = null; break; }
      current = []; currentSpan = 0;
    }
    current.push(w); currentSpan += span;
  }
  if (current && current.length) rows.push(current);
  return rows;
}

function dashboardThumbnailSvg(widgets) {
  if (!Array.isArray(widgets) || !widgets.length) {
    return `<svg viewBox="0 0 ${THUMB_W} ${THUMB_H}" aria-hidden="true"><rect width="100%" height="100%" style="fill:var(--plane)"/></svg>`;
  }
  const rows = packThumbnailRows(widgets);
  const shown = rows.reduce((n, r) => n + r.length, 0);
  const overflow = widgets.length - shown;
  const rowH = (THUMB_H - THUMB_PAD * 2 - THUMB_GAP * (rows.length - 1)) / rows.length;
  const colW = (THUMB_W - THUMB_PAD * 2 - THUMB_GAP * (THUMB_COLS - 1)) / THUMB_COLS;
  let y = THUMB_PAD, cells = '';
  rows.forEach(row => {
    let x = THUMB_PAD;
    const rowSpan = row.reduce((s, w) => s + thumbSpan(w), 0);
    const scale = THUMB_COLS / Math.max(rowSpan, 1);
    row.forEach(w => {
      const span = thumbSpan(w) * scale;
      const cw = span * colW + (span - 1) * THUMB_GAP;
      cells += thumbCell(w, x, y, cw, rowH);
      x += cw + THUMB_GAP;
    });
    y += rowH + THUMB_GAP;
  });
  const overflowChip = overflow > 0 ? `
    <rect x="${THUMB_W - 28}" y="4" width="24" height="13" rx="6.5" style="fill:var(--surface);stroke:var(--grid)"/>
    <text x="${THUMB_W - 16}" y="13.5" text-anchor="middle" style="fill:var(--muted);font-size:8px">+${overflow}</text>` : '';
  return `<svg viewBox="0 0 ${THUMB_W} ${THUMB_H}" aria-hidden="true">
    <rect width="100%" height="100%" style="fill:var(--plane)"/>${cells}${overflowChip}
  </svg>`;
}

function thumbCell(w, x, y, cw, ch) {
  const type = w.comparison ? 'comparison' : (w.type || '').toLowerCase();
  const bg = `<rect x="${x.toFixed(1)}" y="${y.toFixed(1)}" width="${cw.toFixed(1)}" height="${ch.toFixed(1)}" rx="4" style="fill:var(--surface);stroke:var(--grid)"/>`;
  const pad = 4;
  const ix = x + pad, iy = y + pad, iw = Math.max(cw - pad * 2, 1), ih = Math.max(ch - pad * 2, 1);
  let inner;
  switch (type) {
    case 'kpi': inner = thumbKpi(w, x, y, cw, ch); break;
    case 'pie': inner = thumbDonut(w, x + cw / 2, y + ch / 2, Math.min(iw, ih) / 2); break;
    case 'bar': inner = thumbBars(w, ix, iy, iw, ih); break;
    case 'line': inner = thumbLine(w, ix, iy, iw, ih); break;
    case 'progress-table': inner = thumbProgressRows(ix, iy, iw, ih); break;
    case 'trend-matrix': inner = thumbTrendRows(ix, iy, iw, ih); break;
    case 'status-bar': inner = thumbChips(ix, iy, iw, ih); break;
    case 'radial-gauge': inner = thumbRadialGauge(w, x + cw / 2, y + ch / 2, Math.min(iw, ih) / 2); break;
    case 'linear-gauge': inner = thumbLinearGaugeRows(ix, iy, iw, ih); break;
    case 'comparison': inner = thumbComparisonBox(ix, iy, iw, ih); break;
    default: inner = thumbTableRows(ix, iy, iw, ih); break; // table and anything unrecognized
  }
  return bg + inner;
}

function thumbKpi(w, x, y, cw, ch) {
  const e = Array.isArray(w.data) ? w.data[0] : null;
  const v = e?.value ?? Object.values(e || {}).find(n => typeof n === 'number');
  const size = Math.min(ch * 0.32, cw * 0.22, 15);
  return `<text x="${(x + cw / 2).toFixed(1)}" y="${(y + ch / 2 + size * 0.35).toFixed(1)}" text-anchor="middle"
    style="fill:var(--accent);font-size:${size.toFixed(1)}px;font-weight:700">${esc(fmt(v))}</text>`;
}

function thumbSeriesValues(w) {
  if (Array.isArray(w.series) && w.series.length && Array.isArray(w.data)) {
    const keys = w.series.map(s => s.key);
    return w.data.map(r => keys.map(k => num(r[k])));
  }
  if (Array.isArray(w.data)) {
    return w.data.map(r => [num(r.value ?? Object.values(r).find(v => typeof v === 'number'))]);
  }
  return [];
}

function thumbBars(w, x, y, iw, ih) {
  const rows = thumbSeriesValues(w).slice(0, 5);
  if (!rows.length) return '';
  const seriesCount = rows[0].length;
  const groupW = iw / rows.length;
  const barW = Math.max((groupW - 3) / seriesCount - 1, 1.5);
  const max = Math.max(1, ...rows.flat());
  let out = '';
  rows.forEach((vals, i) => {
    vals.forEach((v, s) => {
      const h = Math.max((v / max) * ih, 1.5);
      const bx = x + i * groupW + s * (barW + 1);
      out += `<rect x="${bx.toFixed(1)}" y="${(y + ih - h).toFixed(1)}" width="${barW.toFixed(1)}" height="${h.toFixed(1)}" rx="1" style="fill:${THUMB_TONES[s % THUMB_TONES.length]}"/>`;
    });
  });
  return out;
}

function thumbLine(w, x, y, iw, ih) {
  const rows = thumbSeriesValues(w);
  if (!rows.length) return '';
  const seriesCount = rows[0].length;
  let out = '';
  for (let s = 0; s < seriesCount; s++) {
    const vals = rows.map(r => r[s] ?? 0);
    const max = Math.max(1, ...vals);
    const pts = vals.map((v, i) => {
      const px = x + (vals.length > 1 ? (i / (vals.length - 1)) * iw : iw / 2);
      const py = y + ih - (v / max) * ih;
      return `${px.toFixed(1)},${py.toFixed(1)}`;
    }).join(' ');
    out += `<polyline points="${pts}" fill="none" style="stroke:${THUMB_TONES[s % THUMB_TONES.length]}" stroke-width="1.6" stroke-linejoin="round" stroke-linecap="round"/>`;
  }
  return out;
}

function thumbDonut(w, cx, cy, r) {
  const rows = Array.isArray(w.data) ? w.data.slice(0, 3) : [];
  const total = rows.reduce((s, row) => s + num(row.value), 0) || 1;
  const rIn = r * 0.55;
  let angle = -90, out = '';
  rows.forEach((row, i) => {
    const sweep = (num(row.value) / total) * 360;
    out += thumbArcPath(cx, cy, rIn, r, angle, angle + sweep, THUMB_TONES[i % THUMB_TONES.length]);
    angle += sweep;
  });
  return out || `<circle cx="${cx}" cy="${cy}" r="${r}" style="fill:none;stroke:var(--grid)"/>`;
}

function thumbArcPath(cx, cy, rIn, rOut, a0, a1, color) {
  const large = (a1 - a0) % 360 > 180 ? 1 : 0;
  const p = (r, a) => [cx + r * Math.cos(a * Math.PI / 180), cy + r * Math.sin(a * Math.PI / 180)];
  const [x0, y0] = p(rOut, a0), [x1, y1] = p(rOut, a1), [x2, y2] = p(rIn, a1), [x3, y3] = p(rIn, a0);
  return `<path d="M${x0.toFixed(1)},${y0.toFixed(1)} A${rOut.toFixed(1)},${rOut.toFixed(1)} 0 ${large} 1 ${x1.toFixed(1)},${y1.toFixed(1)}
    L${x2.toFixed(1)},${y2.toFixed(1)} A${rIn.toFixed(1)},${rIn.toFixed(1)} 0 ${large} 0 ${x3.toFixed(1)},${y3.toFixed(1)} Z" style="fill:${color}"/>`;
}

function thumbTableRows(x, y, iw, ih) {
  const rows = 3, rh = ih / rows;
  let out = '';
  for (let i = 0; i < rows; i++) {
    const yy = y + i * rh + rh / 2;
    out += `<line x1="${x.toFixed(1)}" y1="${yy.toFixed(1)}" x2="${(x + iw * (i === 0 ? 1 : 0.7)).toFixed(1)}" y2="${yy.toFixed(1)}" style="stroke:var(--grid)" stroke-width="3"/>`;
  }
  return out;
}

function thumbProgressRows(x, y, iw, ih) {
  const rows = 2, rh = ih / rows;
  let out = '';
  for (let i = 0; i < rows; i++) {
    const yy = y + i * rh + rh / 2 - 1.5;
    const split = 0.4 + i * 0.25;
    out += `<rect x="${x.toFixed(1)}" y="${yy.toFixed(1)}" width="${(iw * split).toFixed(1)}" height="3" rx="1.5" style="fill:var(--accent)"/>`;
    out += `<rect x="${(x + iw * split).toFixed(1)}" y="${yy.toFixed(1)}" width="${(iw * (1 - split)).toFixed(1)}" height="3" rx="1.5" style="fill:var(--grid)"/>`;
  }
  return out;
}

function thumbTrendRows(x, y, iw, ih) {
  const rows = 3, rh = ih / rows;
  const tones = ['var(--accent)', 'var(--danger)', 'var(--muted)'];
  let out = '';
  for (let i = 0; i < rows; i++) {
    const yy = y + i * rh + rh / 2;
    out += `<circle cx="${(x + 3).toFixed(1)}" cy="${yy.toFixed(1)}" r="2" style="fill:${tones[i % tones.length]}"/>`;
    out += `<line x1="${(x + 9).toFixed(1)}" y1="${yy.toFixed(1)}" x2="${(x + iw * 0.7).toFixed(1)}" y2="${yy.toFixed(1)}" style="stroke:var(--grid)" stroke-width="2.5"/>`;
  }
  return out;
}

function thumbRadialGauge(w, cx, cy, r) {
  const row = Array.isArray(w.data) ? w.data[0] : w.data;
  const value = num(row?.value), max = row?.max != null ? num(row.max) : 100;
  // Capped just under a full turn — thumbArcPath's large-arc-flag math degenerates at
  // exactly 360° (start and end point coincide, drawing nothing); a value at or over max
  // still reads as "nearly full" rather than vanishing.
  const pct = max > 0 ? Math.max(0, Math.min(0.999, value / max)) : 0;
  const rIn = r * 0.6;
  const track = `<circle cx="${cx.toFixed(1)}" cy="${cy.toFixed(1)}" r="${((r + rIn) / 2).toFixed(1)}" style="fill:none;stroke:var(--grid)" stroke-width="${(r - rIn).toFixed(1)}"/>`;
  const arc = pct > 0 ? thumbArcPath(cx, cy, rIn, r, -90, -90 + pct * 360, 'var(--accent)') : '';
  return track + arc;
}

function thumbLinearGaugeRows(x, y, iw, ih) {
  const rows = 3, rh = ih / rows;
  let out = '';
  for (let i = 0; i < rows; i++) {
    const yy = y + i * rh + rh / 2;
    const pct = 0.35 + i * 0.22;
    out += `<line x1="${x.toFixed(1)}" y1="${yy.toFixed(1)}" x2="${(x + iw).toFixed(1)}" y2="${yy.toFixed(1)}" style="stroke:var(--grid)" stroke-width="3" stroke-linecap="round"/>`;
    out += `<line x1="${x.toFixed(1)}" y1="${yy.toFixed(1)}" x2="${(x + iw * pct).toFixed(1)}" y2="${yy.toFixed(1)}" style="stroke:var(--accent)" stroke-width="3" stroke-linecap="round"/>`;
    out += `<circle cx="${(x + iw * pct).toFixed(1)}" cy="${yy.toFixed(1)}" r="2.5" style="fill:var(--surface);stroke:var(--accent)" stroke-width="1.5"/>`;
  }
  return out;
}

function thumbChips(x, y, iw, ih) {
  const tones = ['var(--danger)', 'var(--amber)', 'var(--accent)'];
  const chipW = (iw - 8) / 3;
  let out = `<text x="${x.toFixed(1)}" y="${(y + ih * 0.45).toFixed(1)}" style="fill:var(--ink);font-size:${Math.min(ih * 0.4, 14).toFixed(1)}px;font-weight:700">••</text>`;
  tones.forEach((c, i) => {
    const cx = x + i * (chipW + 4);
    out += `<rect x="${cx.toFixed(1)}" y="${(y + ih - 8).toFixed(1)}" width="${chipW.toFixed(1)}" height="6" rx="3" style="fill:${c}"/>`;
  });
  return out;
}

function thumbComparisonBox(x, y, iw, ih) {
  const halfW = (iw - 10) / 2;
  return `
    <rect x="${x.toFixed(1)}" y="${y.toFixed(1)}" width="${halfW.toFixed(1)}" height="${ih.toFixed(1)}" rx="3" style="fill:var(--plane);stroke:var(--grid)"/>
    <rect x="${(x + halfW + 10).toFixed(1)}" y="${y.toFixed(1)}" width="${halfW.toFixed(1)}" height="${ih.toFixed(1)}" rx="3" style="fill:var(--plane);stroke:var(--grid)"/>
    <text x="${(x + iw / 2).toFixed(1)}" y="${(y + ih / 2 + 3).toFixed(1)}" text-anchor="middle" style="fill:var(--muted);font-size:10px">⟷</text>`;
}

// ---------- Active dashboards: own dedicated page (list row with a thumbnail per row) ----------
// Sourced from the same state.history the "السجل" tab already loaded (the List endpoint
// already returns Active dashboards the caller owns or holds a role on, alongside their
// own Drafts) — just filtered down to isActive here, no separate fetch.
function renderActiveDashboardsList() {
  const actives = state.history.filter(h => h.isActive);
  el('active-badge').textContent = actives.length;
  el('active-count').textContent = actives.length
    ? `${actives.length} لوحة نشطة` : 'لا يوجد لوحات نشطة بعد';

  el('active-list').innerHTML = actives.length ? actives.map(h => {
    const widgets = parseWidgets(h.widgetsJson);
    const when = new Date(h.createdAt + (h.createdAt.endsWith('Z') ? '' : 'Z'));
    const roleBadge = h.myRole ? `<span class="role-badge${h.myRole === 'viewer' ? ' role-viewer' : ''}">${ROLE_LABELS[h.myRole] || h.myRole}</span>` : '';
    const disabledBadge = h.disabled ? `<span class="badge-stale" title="${esc(h.disabledReason || '')}">⚠ معطّلة</span>` : '';
    const ownerLine = h.ownerName ? `المالك: ${esc(h.ownerName)}` : '';
    const isAdmin = state.currentUser?.role === 'Admin';
    const canRename = h.myRole === 'owner' || h.myRole === 'editor' || isAdmin;
    const renameBtn = canRename
      ? `<button class="icon-btn rename-active-btn" data-rename="${esc(h.id)}" title="تعديل الاسم والوصف">✏️</button>` : '';
    const rolesBtn = h.myRole === 'owner'
      ? `<button class="icon-btn roles-btn" data-roles="${esc(h.id)}" title="إدارة الصلاحيات">⚙️</button>` : '';
    const delBtn = h.myRole === 'owner'
      ? `<button class="icon-btn del-active-btn" data-del="${esc(h.id)}" title="حذف">🗑️</button>` : '';
    return `<div class="file-row${h.disabled ? ' stale' : ''}" data-id="${esc(h.id)}">
        <div class="fr-main">
          <div class="fr-thumb">${dashboardThumbnailSvg(widgets)}</div>
          <div class="fr-text">
            <div class="fr-line1">
              <span class="fr-name">${esc(h.question)}</span>
              ${disabledBadge}
            </div>
            <div class="fr-line2">${esc(h.summary)}${ownerLine ? ` <span class="sep">·</span> ${ownerLine}` : ''}</div>
          </div>
        </div>
        <div class="col-updated"><b>${relTime(h.createdAt)}</b>${when.toLocaleDateString('ar-EG')}</div>
        <div class="col-usage"><div class="num">${widgets.length}</div><div class="lbl">عنصر</div></div>
        <div class="col-access">${roleBadge || '—'}</div>
        <div class="col-actions">
          <button class="icon-btn open-active-btn" data-open="${esc(h.id)}" title="فتح">↗️</button>
          ${renameBtn}${rolesBtn}${delBtn}
        </div>
      </div>`;
  }).join('') : `<div class="repo-empty">لا يوجد لوحات نشطة بعد — فعّل لوحة من تبويب "السجل" لتظهر هنا.</div>`;

  el('active-list').querySelectorAll('[data-open]').forEach(btn =>
    btn.addEventListener('click', () => openHistoryEntry(btn.dataset.open)));
  el('active-list').querySelectorAll('[data-rename]').forEach(btn =>
    btn.addEventListener('click', () => openRenameModal(btn.dataset.rename)));
  el('active-list').querySelectorAll('[data-roles]').forEach(btn =>
    btn.addEventListener('click', () => openRolesModal(btn.dataset.roles)));
  el('active-list').querySelectorAll('.del-active-btn').forEach(btn =>
    btn.addEventListener('click', () => deleteHistoryEntry(btn.dataset.del)));
}

// Promotes a Draft to Active — the activating user becomes its Owner, which the backend
// rejects if they don't already hold valid permission on every source the dashboard's
// widgets depend on (DashboardAccessService — the exact same gate a fresh question goes
// through).
async function activateHistoryEntry(id) {
  try {
    const res = await fetch(`/api/history/${encodeURIComponent(id)}/activate`, { method: 'POST' });
    const payload = await res.json().catch(() => null);
    if (!res.ok) { alert(payload?.error || 'تعذّر تفعيل اللوحة.'); return; }
    const idx = state.history.findIndex(h => h.id === id);
    if (idx !== -1) state.history[idx] = payload; else state.history.unshift(payload);
    renderHistory();
    // If this is the dashboard currently on screen (e.g. activated via the toolbar's
    // ✅ تفعيل اللوحة button, not just from السجل), reflect its new Active state there too
    // — unlocks مشاركة and hides the activate button — rather than requiring a reopen.
    if (state.currentHistoryId === id) {
      state.dashboardIsActive = true;
      state.dashboardRole = payload.myRole || 'owner';
      state.dashboardDisabled = !!payload.disabled;
      state.dashboardDisabledReason = payload.disabledReason || '';
      renderDashboard();
    }
  } catch {
    alert('تعذّر الوصول إلى الخادم.');
  }
}
el('btn-activate').addEventListener('click', () => {
  if (state.currentHistoryId) activateHistoryEntry(state.currentHistoryId);
});

function parseWidgets(json) {
  try { const w = JSON.parse(json); return Array.isArray(w) ? w : []; } catch { return []; }
}

function parseFilters(json) {
  try { const f = JSON.parse(json); return Array.isArray(f) ? f : []; } catch { return []; }
}
function parseActiveFilters(json) {
  try { const f = JSON.parse(json); return f && typeof f === 'object' && !Array.isArray(f) ? f : {}; } catch { return {}; }
}

// Reopens a saved dashboard entirely client-side — no call to the chat/AI endpoint. Filters
// and the selection that was active when it was saved come back too (see saveToHistory/
// doAutosave), so the dashboard looks exactly as it did rather than losing its filter UI.
function openHistoryEntry(id) {
  const entry = state.history.find(h => h.id === id);
  if (!entry) return;
  state.dashboard = ensureWidgetMeta({
    name: entry.question, summary: entry.summary, widgets: parseWidgets(entry.widgetsJson),
    filters: parseFilters(entry.filtersJson),
  });
  state.editHistory = { past: [], future: [] };
  state.currentHistoryId = entry.id; // further edits autosave onto this same saved entry
  state.activeFilters = parseActiveFilters(entry.activeFiltersJson);
  // Part 2: an Active dashboard is never frozen — see the state.dashboardIsActive branch
  // in applyFilters(), triggered right below, which re-executes every widget live under
  // the Owner's own permission (re-validated fresh, not the List call's snapshot).
  state.dashboardIsActive = !!entry.isActive;
  state.dashboardRole = entry.isActive ? (entry.myRole || null) : null;
  state.dashboardDisabled = !!entry.disabled;
  state.dashboardDisabledReason = entry.disabledReason || '';
  state.messages.push({ role: 'user', text: entry.queryDescription || entry.question });
  state.messages.push({
    role: 'bot', text: entry.summary,
    speechWidgets: state.dashboard.widgets.map(w => ({ id: w.id, title: w.title })),
  });
  showScreen('chat');
  renderMessages(); renderDashboard();
  if (state.dashboardIsActive && !state.dashboardDisabled) applyFilters();
}

async function deleteHistoryEntry(id) {
  if (!confirm('حذف هذه اللوحة من السجل؟')) return;
  await fetch('/api/history/' + id, { method: 'DELETE' });
  state.history = state.history.filter(h => h.id !== id);
  renderHistory();
}

// ---------- Part 2: roles modal (Owner transfer + Editor/Viewer management) ----------
// Both pickers below use the same searchable user picker as the file-permissions modal
// (initUserSearchPicker) — type a few letters of a name or username, click a result to act
// immediately, rather than typing an exact username into a plain text field.
function rolesModalError(message) {
  const box = el('roles-error');
  box.textContent = message || '';
  box.classList.toggle('hidden', !message);
}

async function openRolesModal(id) {
  rolesModalError('');
  el('roles-modal-id').value = id;
  el('roles-owner-name').textContent = '…';
  el('roles-list').innerHTML = '';
  el('transfer-owner-search').value = '';
  el('role-add-search').value = '';
  el('transfer-owner-suggestions').classList.add('hidden');
  el('role-add-suggestions').classList.add('hidden');
  el('roles-modal').classList.remove('hidden');
  await reloadRolesModal();
}

async function reloadRolesModal() {
  const id = el('roles-modal-id').value;
  const res = await fetch(`/api/history/${encodeURIComponent(id)}/roles`);
  if (!res.ok) { rolesModalError('تعذّر تحميل الصلاحيات.'); return; }
  const data = await res.json();
  el('roles-owner-name').textContent = data.ownerName || data.ownerId || '—';
  el('roles-list').innerHTML = data.roles.length ? data.roles.map(r => `
      <div class="role-row">
        <span class="name">${esc(r.displayName)} — ${ROLE_LABELS[r.role] || r.role}</span>
        <button type="button" class="remove-role-btn" data-remove="${esc(r.userId)}">إزالة</button>
      </div>`).join('') : `<div class="owner-line">لا يوجد محررون أو مشاهدون بعد.</div>`;
  el('roles-list').querySelectorAll('[data-remove]').forEach(btn =>
    btn.addEventListener('click', async () => {
      await fetch(`/api/history/${encodeURIComponent(id)}/roles/${encodeURIComponent(btn.dataset.remove)}`, { method: 'DELETE' });
      await reloadRolesModal();
    }));
}

initUserSearchPicker(el('role-add-search'), el('role-add-suggestions'), async picked => {
  rolesModalError('');
  const id = el('roles-modal-id').value;
  const role = el('role-add-select').value;
  const res = await fetch(`/api/history/${encodeURIComponent(id)}/roles/${encodeURIComponent(picked.id)}`, {
    method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ role }),
  });
  if (!res.ok) { const p = await res.json().catch(() => null); rolesModalError(p?.error || 'تعذّر إضافة الدور.'); return; }
  el('role-add-search').value = '';
  await reloadRolesModal();
});

initUserSearchPicker(el('transfer-owner-search'), el('transfer-owner-suggestions'), async picked => {
  rolesModalError('');
  const id = el('roles-modal-id').value;
  if (!confirm(`نقل ملكية اللوحة إلى ${picked.displayName || picked.username}؟`)) return;
  const res = await fetch(`/api/history/${encodeURIComponent(id)}/transfer-owner`, {
    method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ newOwnerId: picked.id }),
  });
  const payload = await res.json().catch(() => null);
  if (!res.ok) { rolesModalError(payload?.error || 'تعذّر نقل الملكية.'); return; }
  // The caller may no longer be the Owner after a successful transfer — reflect that in
  // the open dashboard (if this is the one on screen) and the History list right away.
  const idx = state.history.findIndex(h => h.id === id);
  if (idx !== -1) state.history[idx] = payload;
  if (state.currentHistoryId === id) {
    state.dashboardRole = payload.myRole || null;
    state.dashboardDisabled = !!payload.disabled;
    state.dashboardDisabledReason = payload.disabledReason || '';
    renderDashboard();
  }
  el('transfer-owner-search').value = '';
  await reloadRolesModal();
  renderHistory();
});

el('roles-modal-close').addEventListener('click', () => el('roles-modal').classList.add('hidden'));

// ---------- rename (active dashboard title/description) ----------
function renameModalError(message) {
  const box = el('rename-error');
  box.textContent = message || '';
  box.classList.toggle('hidden', !message);
}

function openRenameModal(id) {
  const entry = state.history.find(h => h.id === id);
  if (!entry) return;
  renameModalError('');
  el('rename-modal-id').value = id;
  el('rename-name').value = entry.question || '';
  el('rename-desc').value = entry.summary || '';
  el('rename-modal').classList.remove('hidden');
  el('rename-name').focus();
}

el('rename-cancel').addEventListener('click', () => el('rename-modal').classList.add('hidden'));

el('rename-form').addEventListener('submit', async e => {
  e.preventDefault();
  renameModalError('');
  const id = el('rename-modal-id').value;
  const question = el('rename-name').value.trim();
  const summary = el('rename-desc').value.trim();
  if (!question) { renameModalError('الاسم مطلوب.'); return; }
  const res = await fetch(`/api/history/${encodeURIComponent(id)}/rename`, {
    method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ question, summary }),
  });
  const payload = await res.json().catch(() => null);
  if (!res.ok) { renameModalError(payload?.error || 'تعذّر الحفظ.'); return; }
  const idx = state.history.findIndex(h => h.id === id);
  if (idx !== -1) state.history[idx] = payload;
  if (state.currentHistoryId === id) {
    state.dashboard.name = payload.question;
    state.dashboard.summary = payload.summary;
    renderDashboard();
  }
  el('rename-modal').classList.add('hidden');
  renderHistory();
});

el('active-roles-info-btn').addEventListener('click', () => el('role-info-modal').classList.remove('hidden'));
el('role-info-close').addEventListener('click', () => el('role-info-modal').classList.add('hidden'));

el('history-clear').addEventListener('click', async () => {
  if (!state.history.length || !confirm('مسح كل السجل؟ لا يمكن التراجع عن هذا.')) return;
  await fetch('/api/history', { method: 'DELETE' });
  state.history = [];
  renderHistory();
});

// ---------- screens ----------
function showScreen(name) {
  document.querySelectorAll('.screen').forEach(s => s.classList.toggle('active', s.id === 'screen-' + name));
  document.querySelectorAll('.tab').forEach(t => t.classList.toggle('active', t.dataset.screen === name));
  el('rail-size-toggle').classList.toggle('hidden', name !== 'chat');
  if (name === 'repo') loadFiles();
  if (name === 'history' && !state.historyLoaded) loadHistory();
  if (name === 'active') { if (state.historyLoaded) renderActiveDashboardsList(); else loadHistory(); }
  if (name === 'sharelinks') loadShareList();
  if (name === 'users') loadUsers();
  if (name === 'settings') loadSettingsPage();
  if (name === 'integrations') loadIntegrations();
  // Coming back to المحادثة re-fetches the sources list — state.systems is otherwise only
  // ever loaded once at startApp(), so an integration newly set up (or newly schema-
  // discovered) in التكاملات الخارجية never appeared in the sources popover until a full
  // page reload, even though its own detail screen already showed it ready.
  if (name === 'chat') loadSources();
}
document.querySelectorAll('[data-screen]').forEach(b =>
  b.addEventListener('click', () => showScreen(b.dataset.screen)));

// ---------- طي القائمة الجانبية (يُحفظ لكل مستخدم في المتصفح) ----------
const appShell = el('app-shell');
if (localStorage.getItem('jeem.sidebar') === 'collapsed') appShell.classList.add('is-collapsed');
const sbCollapseBtn = el('sb-collapse-btn');
sbCollapseBtn.addEventListener('click', () => {
  appShell.classList.toggle('is-collapsed');
  const collapsed = appShell.classList.contains('is-collapsed');
  sbCollapseBtn.setAttribute('aria-expanded', String(!collapsed));
  try { localStorage.setItem('jeem.sidebar', collapsed ? 'collapsed' : 'open'); } catch (e) { /* تجاهل */ }
  window.dispatchEvent(new Event('resize')); // لإعادة رسم الرسوم البيانية المفتوحة
});

// ---------- القائمة على الجوال (sidebar drawer + scrim) ----------
const scrimEl = document.querySelector('.scrim');
const closeMobileNav = () => { appShell.classList.remove('nav-open'); scrimEl?.classList.remove('is-on'); };
el('topbar-menu-btn').addEventListener('click', () => { appShell.classList.add('nav-open'); scrimEl?.classList.add('is-on'); });
scrimEl?.addEventListener('click', closeMobileNav);
document.addEventListener('keydown', e => { if (e.key === 'Escape') closeMobileNav(); });
document.querySelectorAll('.sidebar [data-screen]').forEach(b => b.addEventListener('click', closeMobileNav));

// ---------- chat rail size (small / normal / large / hidden) ----------
// Three controls, as requested: shrink, grow, hide-entirely. Independent of theme,
// persisted the same way (localStorage) so the choice survives a reload.
const RAIL_SIZES = ['small', 'normal', 'large'];
function loadRailPrefs() {
  let size = 'normal', hidden = false;
  try {
    const storedSize = localStorage.getItem('chatToDashboardRailSize');
    if (RAIL_SIZES.includes(storedSize)) size = storedSize;
    hidden = localStorage.getItem('chatToDashboardRailHidden') === '1';
  } catch {}
  return { size, hidden };
}
const railState = loadRailPrefs();

function applyRailState() {
  const rail = el('chat-rail');
  rail.classList.remove('rail-small', 'rail-large', 'rail-hidden');
  if (railState.hidden) rail.classList.add('rail-hidden');
  else if (railState.size === 'small') rail.classList.add('rail-small');
  else if (railState.size === 'large') rail.classList.add('rail-large');

  el('rail-hide').textContent = railState.hidden ? '💬' : '🗕';
  el('rail-hide').title = railState.hidden ? 'إظهار لوحة المحادثة' : 'إخفاء لوحة المحادثة';
  el('rail-shrink').disabled = !railState.hidden && railState.size === 'small';
  el('rail-grow').disabled = !railState.hidden && railState.size === 'large';

  try {
    localStorage.setItem('chatToDashboardRailSize', railState.size);
    localStorage.setItem('chatToDashboardRailHidden', railState.hidden ? '1' : '0');
  } catch {}
}
applyRailState();

el('rail-shrink').addEventListener('click', () => {
  railState.hidden = false;
  const idx = RAIL_SIZES.indexOf(railState.size);
  if (idx > 0) railState.size = RAIL_SIZES[idx - 1];
  applyRailState();
});
el('rail-grow').addEventListener('click', () => {
  railState.hidden = false;
  const idx = RAIL_SIZES.indexOf(railState.size);
  if (idx < RAIL_SIZES.length - 1) railState.size = RAIL_SIZES[idx + 1];
  applyRailState();
});
el('rail-hide').addEventListener('click', () => {
  railState.hidden = !railState.hidden;
  applyRailState();
});

// ---------- boot ----------
el.dash = el('dash'); el.messages = el('messages'); el.send = el('send');

el('form').addEventListener('submit', e => {
  e.preventDefault();
  const q = el('q').value.trim();
  if (state.chatMode === 'inquiry') {
    if (state.loading || !q) return;
    el('q').value = '';
    askInquiry(q);
    return;
  }
  if (state.loading || (!q && !state.attachedImage)) return;
  el('q').value = '';
  ask(q || 'أعد إنشاء الداشبورد كما في الصورة المرفقة، بنفس أنواع الرسومات والعناصر، مستخدمًا بياناتي الفعلية.');
});

// ---------- image-to-dashboard attachment ----------
function renderAttachedImage() {
  const has = !!state.attachedImage;
  el('attached-image').classList.toggle('hidden', !has);
  if (has) {
    el('attached-image-preview').src = state.attachedImage;
    el('attached-image-name').textContent = state.attachedImageName;
  }
}

// Downscales to a sensible max dimension before sending — keeps the request small and
// fast regardless of how big the original screenshot/photo is.
async function fileToResizedDataUrl(file, maxDim = 1600, quality = 0.85) {
  const objectUrl = URL.createObjectURL(file);
  try {
    const img = await new Promise((resolve, reject) => {
      const image = new Image();
      image.onload = () => resolve(image);
      image.onerror = () => reject(new Error('image decode failed'));
      image.src = objectUrl;
    });
    const scale = Math.min(1, maxDim / Math.max(img.width, img.height));
    const canvas = document.createElement('canvas');
    canvas.width = Math.max(1, Math.round(img.width * scale));
    canvas.height = Math.max(1, Math.round(img.height * scale));
    const ctx = canvas.getContext('2d');
    ctx.fillStyle = '#ffffff';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
    return canvas.toDataURL('image/jpeg', quality);
  } finally {
    URL.revokeObjectURL(objectUrl);
  }
}

el('dash-image-input').addEventListener('change', async e => {
  const file = e.target.files[0];
  e.target.value = '';
  if (!file) return;
  if (!file.type.startsWith('image/')) { alert('الملف يجب أن يكون صورة.'); return; }
  if (file.size > 15 * 1024 * 1024) { alert('حجم الصورة كبير جداً (الحد الأقصى 15MB).'); return; }
  try {
    state.attachedImage = await fileToResizedDataUrl(file);
    state.attachedImageName = file.name;
    renderAttachedImage();
  } catch {
    alert('تعذّر قراءة الصورة.');
  }
});

el('attached-image-remove').addEventListener('click', () => {
  state.attachedImage = null; state.attachedImageName = '';
  renderAttachedImage();
});

// ---------- share (Part 3: creator-set expiry, manual revocation, view count) ----------
// Opens the modal in "compose" mode (pick an expiry, then confirm) rather than creating
// immediately — the expiry has to be chosen before the link exists. The modal also always
// shows the creator's past links (loadShareList) so revoke/view-count management lives in
// the same place as creating a new one.
function openShareModal() {
  const d = state.dashboard;
  if (!d?.widgets?.length) return;
  // The button is disabled for a Draft (see renderDashboard), but guard here too in case
  // this is ever reached another way — a Draft has no Owner, so a link made from it would
  // be an unmanageable orphan (also enforced server-side, see ShareController.Create).
  if (!state.dashboardIsActive) { alert('يجب تفعيل اللوحة أولاً قبل مشاركتها.'); return; }
  el('share-compose').classList.remove('hidden');
  el('share-link-box').classList.add('hidden');
  el('share-expiry').value = '24';
  el('share-expiry-date').classList.add('hidden');
  el('share-modal').classList.remove('hidden');
}
el('btn-share').addEventListener('click', openShareModal);

// ---------- نشر إلى تكامل خارجي (External Integrations Part A) ----------
// A separate, explicit action from saving/editing — see PublishService's own remarks. Only
// ever available for an Active dashboard (same gating as المشاركة above).
async function openPublishModal() {
  if (!state.dashboardIsActive || !state.currentHistoryId) { alert('يجب تفعيل اللوحة أولاً قبل نشرها.'); return; }
  el('publish-error').classList.add('hidden');
  el('publish-success').classList.add('hidden');
  el('publish-slot-choice').classList.add('hidden');
  resetPublishColumnConfirm();
  const select = el('publish-integration-select');
  select.innerHTML = '<option>جارٍ التحميل…</option>';
  el('publish-modal').classList.remove('hidden');
  try {
    const res = await fetch('/api/integrations');
    const integrations = res.ok ? await res.json() : [];
    state.publishIntegrations = integrations;
    if (!integrations.length) {
      select.innerHTML = '<option value="">لا توجد تكاملات — أنشئ واحدًا من شاشة «التكاملات الخارجية»</option>';
      return;
    }
    select.innerHTML = integrations.map(i => `<option value="${esc(i.id)}">${esc(i.name)}</option>`).join('');
    await Promise.all([checkExistingSlot(), loadPublishDataFilters()]);
  } catch {
    select.innerHTML = '<option value="">تعذّر تحميل قائمة التكاملات</option>';
  }
}
el('btn-publish').addEventListener('click', openPublishModal);

async function checkExistingSlot() {
  const integrationId = el('publish-integration-select').value;
  if (!integrationId) { el('publish-slot-choice').classList.add('hidden'); return; }
  try {
    const res = await fetch(`/api/integrations/${integrationId}/existing-slot?localHistoryId=${encodeURIComponent(state.currentHistoryId)}`);
    const data = await res.json();
    if (data.exists) {
      el('publish-existing-title').textContent = data.title;
      el('publish-slot-choice').dataset.slotId = data.slotId;
      el('publish-slot-choice').classList.remove('hidden');
      el('publish-modal').querySelector('input[name="publish-mode"][value="update"]').checked = true;
    } else {
      el('publish-slot-choice').classList.add('hidden');
    }
  } catch { el('publish-slot-choice').classList.add('hidden'); }
}

// ---------- فلترة بيانات حسب المستخدم (Part E, data-level extension) ----------
// Only ever shown when the selected integration's connector has auto-detected the client's own
// fixed-shape permissions table — see ClientSchemaDiscoveryService. Marks are saved immediately
// on every change (PUT, full-replace) rather than only at publish time, so they survive a
// cancelled modal and are already there the next time this dashboard is published.
async function loadPublishDataFilters() {
  resetPublishColumnConfirm();
  const box = el('publish-data-filters');
  const integrationId = el('publish-integration-select').value;
  const integ = (state.publishIntegrations || []).find(i => i.id === integrationId);
  const widgets = state.dashboard?.widgets || [];
  if (!integ?.dataPermissionsAvailable || !widgets.length) { box.classList.add('hidden'); return; }

  try {
    const [keysRes, marksRes] = await Promise.all([
      fetch(`/api/integrations/${integrationId}/data-permission-keys`),
      fetch(`/api/integrations/${integrationId}/dashboards/${encodeURIComponent(state.currentHistoryId)}/widget-filters`),
    ]);
    const keys = keysRes.ok ? await keysRes.json() : [];
    const marks = marksRes.ok ? await marksRes.json() : [];
    if (!keys.length) { box.classList.add('hidden'); return; }

    box.classList.remove('hidden');
    const list = el('publish-data-filters-list');
    list.innerHTML = widgets.map((w, i) => {
      const mark = marks.find(m => m.widgetIndex === i);
      return `
        <div style="display:flex;align-items:center;gap:10px;justify-content:space-between;padding:6px 0;border-bottom:1px solid var(--grid)">
          <label class="chk-row" style="flex:1"><input type="checkbox" class="pdf-toggle" data-index="${i}" ${mark ? 'checked' : ''}>
            ${esc(w.title || ('عنصر ' + (i + 1)))}</label>
          <select class="pdf-key" data-index="${i}" ${mark ? '' : 'disabled'} style="max-width:160px">
            ${keys.map(k => `<option value="${esc(k)}" ${mark && mark.filterKey === k ? 'selected' : ''}>${esc(k)}</option>`).join('')}
          </select>
        </div>`;
    }).join('');

    list.querySelectorAll('.pdf-toggle').forEach(cb => cb.addEventListener('change', () => {
      list.querySelector(`.pdf-key[data-index="${cb.dataset.index}"]`).disabled = !cb.checked;
      savePublishDataFilterMarks(integrationId);
    }));
    list.querySelectorAll('.pdf-key').forEach(sel => sel.addEventListener('change', () => savePublishDataFilterMarks(integrationId)));
  } catch { box.classList.add('hidden'); }
}

async function savePublishDataFilterMarks(integrationId) {
  const list = el('publish-data-filters-list');
  const filters = [...list.querySelectorAll('.pdf-toggle')].filter(cb => cb.checked).map(cb => ({
    widgetIndex: Number(cb.dataset.index),
    filterKey: list.querySelector(`.pdf-key[data-index="${cb.dataset.index}"]`).value,
  }));
  try {
    await fetch(`/api/integrations/${integrationId}/dashboards/${encodeURIComponent(state.currentHistoryId)}/widget-filters`, {
      method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ filters }),
    });
  } catch { /* best-effort — a save that fails here just means the mark isn't remembered next time */ }
}

el('publish-integration-select').addEventListener('change', () => {
  checkExistingSlot();
  loadPublishDataFilters();
});

function resetPublishColumnConfirm() {
  state.publishPending = null;
  el('publish-column-confirm').classList.add('hidden');
  el('publish-column-confirm-list').innerHTML = '';
  el('publish-submit').classList.remove('hidden');
  el('publish-confirm-columns').classList.add('hidden');
}

el('publish-cancel').addEventListener('click', () => el('publish-modal').classList.add('hidden'));
el('publish-modal').addEventListener('click', e => { if (e.target.id === 'publish-modal') el('publish-modal').classList.add('hidden'); });

async function runPublishConfirm(integrationId, previewId, slotId, columnChoices) {
  const res = await fetch(`/api/integrations/${integrationId}/publish/confirm`, {
    method: 'POST', headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ previewId, slotId, columnChoices }),
  });
  const data = await res.json().catch(() => ({}));
  if (!res.ok) { el('publish-error').textContent = data.error || `فشل النشر (${res.status})`; el('publish-error').classList.remove('hidden'); return; }
  el('publish-success').classList.remove('hidden');
  setTimeout(() => el('publish-modal').classList.add('hidden'), 1200);
}

el('publish-submit').addEventListener('click', async () => {
  const integrationId = el('publish-integration-select').value;
  if (!integrationId) return;
  const useExisting = !el('publish-slot-choice').classList.contains('hidden') &&
    el('publish-modal').querySelector('input[name="publish-mode"]:checked')?.value === 'update';
  const slotId = useExisting ? el('publish-slot-choice').dataset.slotId : undefined;

  const btn = el('publish-submit');
  // /publish/prepare does a live round-trip to the client's own connector (schema
  // re-discovery, possibly an LLM retargeting call per widget) — easily a few seconds, and
  // with nothing but a quietly-disabled button to show for it this read as "nothing
  // happens" when clicked. The disabled state alone isn't enough; say so explicitly.
  const originalLabel = btn.textContent;
  btn.disabled = true;
  btn.textContent = 'جارٍ التجهيز…';
  el('publish-error').classList.add('hidden');
  try {
    const prepRes = await fetch(`/api/integrations/${integrationId}/publish/prepare`, {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ dashboardId: state.currentHistoryId }),
    });
    const prep = await prepRes.json().catch(() => ({}));
    if (!prepRes.ok) {
      el('publish-error').textContent = prep.error || `تعذّر تجهيز النشر (${prepRes.status})`;
      el('publish-error').classList.remove('hidden');
      return;
    }

    // Only widgets whose retargeted query actually has real output columns need a confirmed
    // choice — one whose translation produced nothing (see PrepareAsync) is simply skipped, it
    // ships no data regardless of any filtering intent.
    const needsColumn = (prep.widgetsNeedingColumn || []).filter(w => w.columns && w.columns.length);
    if (needsColumn.length) {
      el('publish-column-confirm-list').innerHTML = needsColumn.map(w => `
        <div style="padding:6px 0;border-bottom:1px solid var(--grid)">
          <div class="model-switch-note" style="font-weight:700;color:var(--ink);margin-bottom:4px">${esc(w.title)} — فلترة حسب "${esc(w.filterKey)}"</div>
          <select class="pcc-column" data-index="${w.index}">
            ${w.columns.map(c => `<option value="${esc(c)}">${esc(c)}</option>`).join('')}
          </select>
        </div>`).join('');
      state.publishPending = { integrationId, previewId: prep.previewId, slotId };
      el('publish-column-confirm').classList.remove('hidden');
      btn.classList.add('hidden');
      el('publish-confirm-columns').classList.remove('hidden');
      return;
    }

    await runPublishConfirm(integrationId, prep.previewId, slotId, []);
  } catch (err) {
    el('publish-error').textContent = 'تعذّر الوصول إلى الخادم: ' + err.message;
    el('publish-error').classList.remove('hidden');
  } finally {
    btn.disabled = false;
    btn.textContent = originalLabel;
  }
});

el('publish-confirm-columns').addEventListener('click', async () => {
  const pending = state.publishPending;
  if (!pending) return;
  const btn = el('publish-confirm-columns');
  const originalLabel = btn.textContent;
  btn.disabled = true;
  btn.textContent = 'جارٍ النشر…';
  el('publish-error').classList.add('hidden');
  const columnChoices = [...document.querySelectorAll('.pcc-column')].map(sel => ({
    widgetIndex: Number(sel.dataset.index), column: sel.value,
  }));
  try {
    await runPublishConfirm(pending.integrationId, pending.previewId, pending.slotId, columnChoices);
  } catch (err) {
    el('publish-error').textContent = 'تعذّر الوصول إلى الخادم: ' + err.message;
    el('publish-error').classList.remove('hidden');
  } finally {
    btn.disabled = false;
    btn.textContent = originalLabel;
  }
});

el('share-goto-list').addEventListener('click', () => {
  el('share-modal').classList.add('hidden');
  showScreen('sharelinks');
});

el('share-expiry').addEventListener('change', () => {
  el('share-expiry-date').classList.toggle('hidden', el('share-expiry').value !== 'custom');
});

// Resolves whatever the creator picked (a duration preset or a specific date) to one
// absolute ISO instant — the server only ever sees "expiresAt" (see ShareModels.cs).
function resolveShareExpiresAt() {
  const choice = el('share-expiry').value;
  if (!choice) return null;
  if (choice === 'custom') {
    const dateVal = el('share-expiry-date').value;
    return dateVal ? new Date(dateVal + 'T23:59:59').toISOString() : null;
  }
  return new Date(Date.now() + Number(choice) * 3600 * 1000).toISOString();
}

el('share-create-confirm').addEventListener('click', async () => {
  const d = state.dashboard;
  if (!d?.widgets?.length) return;
  const btn = el('share-create-confirm');
  btn.disabled = true; const original = btn.textContent; btn.textContent = 'جارٍ الإنشاء…';
  try {
    const lastQuestion = [...state.messages].reverse().find(m => m.role === 'user')?.text || 'لوحة معلومات';
    const res = await fetch('/api/share', {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        question: lastQuestion, summary: d.summary, widgets: d.widgets,
        filters: d.filters || [], activeFilters: state.activeFilters || {},
        expiresAt: resolveShareExpiresAt(), historyId: state.currentHistoryId,
      }),
    });
    if (!res.ok) { const p = await res.json().catch(() => null); throw new Error(p?.error || `فشل إنشاء الرابط (${res.status})`); }
    const saved = await res.json();
    el('share-link').value = `${location.origin}/?share=${saved.id}`;
    el('share-compose').classList.add('hidden');
    el('share-link-box').classList.remove('hidden');
    el('share-link').select();
    await loadShareList();
  } catch (err) {
    alert('تعذّر إنشاء رابط المشاركة: ' + err.message);
  } finally {
    btn.disabled = false; btn.textContent = original;
  }
});

el('share-close').addEventListener('click', () => el('share-modal').classList.add('hidden'));
el('share-modal').addEventListener('click', e => { if (e.target.id === 'share-modal') el('share-modal').classList.add('hidden'); });
el('share-copy').addEventListener('click', async () => {
  const btn = el('share-copy'); const original = btn.textContent;
  try {
    await navigator.clipboard.writeText(el('share-link').value);
  } catch {
    el('share-link').select();
    document.execCommand('copy');
  }
  btn.textContent = 'تم النسخ ✓';
  setTimeout(() => { btn.textContent = original; }, 1500);
});

function shareStatus(s) {
  if (s.revokedAt) return { label: 'أُلغي', cls: 'inactive' };
  if (s.expiresAt && new Date(s.expiresAt + (s.expiresAt.endsWith('Z') ? '' : 'Z')) < new Date()) return { label: 'منتهي', cls: 'inactive' };
  return { label: 'نشط', cls: 'live' };
}

// ---------- share links: own dedicated page (list view, same shape as the file repository's
// and the Active-dashboards page) ----------
async function loadShareList() {
  const list = el('sharelinks-list');
  list.innerHTML = `<div class="repo-empty">جارٍ التحميل…</div>`;
  try {
    const res = await fetch('/api/share');
    const shares = res.ok ? await res.json() : [];
    el('sharelinks-badge').textContent = shares.length;
    el('sharelinks-count').textContent = shares.length ? `${shares.length} رابط مشاركة` : 'لا يوجد روابط مشاركة بعد';
    if (!shares.length) {
      list.innerHTML = `<div class="repo-empty">لا يوجد روابط مشاركة بعد — أنشئ رابطًا من زر "🔗 مشاركة" على أي لوحة.</div>`;
      return;
    }
    list.innerHTML = shares.map(s => {
      const status = shareStatus(s);
      const when = new Date(s.createdAt + (s.createdAt.endsWith('Z') ? '' : 'Z'));
      const expiryText = s.expiresAt
        ? new Date(s.expiresAt + (s.expiresAt.endsWith('Z') ? '' : 'Z')).toLocaleDateString('ar-EG')
        : 'بدون انتهاء';
      const revokeBtn = status.cls === 'live'
        ? `<button class="icon-btn revoke-share-btn" data-revoke="${esc(s.id)}" title="إلغاء الرابط">🚫</button>` : '';
      const copyBtn = status.cls === 'live'
        ? `<button class="icon-btn copy-share-btn" data-copy="${esc(s.id)}" title="نسخ الرابط">🔗</button>` : '';
      return `<div class="file-row${status.cls === 'inactive' ? ' stale' : ''}" data-id="${esc(s.id)}">
          <div class="fr-main">
            <div class="fr-icon">🔗</div>
            <div class="fr-text">
              <div class="fr-line1">
                <span class="fr-name">${esc(s.question)}</span>
              </div>
              <div class="fr-line2">${esc(s.summary || '')}</div>
            </div>
          </div>
          <div class="col-updated"><b>${relTime(s.createdAt)}</b>ينتهي: ${expiryText}</div>
          <div class="col-usage"><div class="num">${s.viewCount || 0}</div><div class="lbl">👁 مشاهدة</div></div>
          <div class="col-access"><span class="share-status-tag ${status.cls}">${status.label}</span></div>
          <div class="col-actions">
            ${copyBtn}${revokeBtn}
            <button class="icon-btn del-share-btn" data-del="${esc(s.id)}" title="حذف">🗑️</button>
          </div>
        </div>`;
    }).join('');
    list.querySelectorAll('[data-revoke]').forEach(btn => btn.addEventListener('click', async () => {
      await fetch(`/api/share/${encodeURIComponent(btn.dataset.revoke)}/revoke`, { method: 'POST' });
      await loadShareList();
    }));
    list.querySelectorAll('[data-copy]').forEach(btn => btn.addEventListener('click', async () => {
      const link = `${location.origin}/?share=${btn.dataset.copy}`;
      try { await navigator.clipboard.writeText(link); } catch {}
      const original = btn.textContent; btn.textContent = '✓'; setTimeout(() => { btn.textContent = original; }, 1200);
    }));
    list.querySelectorAll('[data-del]').forEach(btn => btn.addEventListener('click', async () => {
      if (!confirm('حذف رابط المشاركة هذا؟')) return;
      await fetch(`/api/share/${encodeURIComponent(btn.dataset.del)}`, { method: 'DELETE' });
      await loadShareList();
    }));
  } catch {
    list.innerHTML = `<div class="repo-empty">تعذّر تحميل الروابط.</div>`;
  }
}

// A URL like /?share=<id> opens a read-only public view: no login, no chat, no sources —
// just the published dashboard, fetched straight from the share endpoint and rendered
// through the same renderDashboard() as everything else.
async function bootSharedView(shareId) {
  // A share is a frozen snapshot (Part 3) — state.shareId only marks "we're in the public
  // share view" (banner, read-only filter display); it no longer routes any live query.
  state.shareId = shareId;
  el('shared-banner').classList.remove('hidden');
  document.querySelector('header').style.display = 'none';
  el('chat-rail').style.display = 'none';
  try {
    const res = await fetch(`/api/share/${encodeURIComponent(shareId)}`);
    if (res.status === 410) {
      const payload = await res.json().catch(() => null);
      el('shared-banner-q').textContent = 'رابط غير نشط';
      document.title = 'رابط غير نشط — لوحة مشاركة';
      // A clear, dedicated message — not the normal empty/loading dashboard placeholder —
      // per the design doc: shows a clear message, not a generic error page.
      el.dash.innerHTML = `<div class="empty"><div>
          <div class="big">⚠️ ${esc(payload?.reason || 'هذا الرابط لم يعد نشطًا.')}</div>
          <div>تواصل مع ${esc(payload?.creatorName || 'صاحب الرابط')} للحصول على رابط جديد.</div>
        </div></div>`;
      return;
    }
    if (!res.ok) throw new Error(res.status === 404 ? 'الرابط غير موجود أو تم حذفه.' : `تعذّر التحميل (${res.status})`);
    const shared = await res.json();
    el('shared-banner-q').textContent = shared.question;
    document.title = `${shared.question} — لوحة مشاركة`;
    state.dashboard = {
      summary: shared.summary,
      widgets: JSON.parse(shared.widgetsJson || '[]'),
      filters: JSON.parse(shared.filtersJson || '[]'),
    };
    state.activeFilters = JSON.parse(shared.activeFiltersJson || '{}');
  } catch (err) {
    el('shared-banner-q').textContent = err.message;
  }
  renderDashboard();
}

// ---------- auth ----------
function showLoginScreen(message) {
  el('login-screen').classList.remove('hidden');
  if (message) {
    el('login-error').textContent = message;
    el('login-error').classList.remove('hidden');
  }
}
function hideLoginScreen() { el('login-screen').classList.add('hidden'); }

function renderCurrentUser() {
  const u = state.currentUser;
  if (!u) return;
  el('current-user-name').textContent = `${u.displayName} (${u.role === 'Admin' ? 'مسؤول' : 'مستخدم'})`;
  // The four admin-only screens all share this one condition — toggled on their shared
  // wrapper (see #admin-menu) rather than each item individually now that they live
  // behind one dropdown instead of four flat tabs.
  el('admin-menu').classList.toggle('hidden', u.role !== 'Admin');
}

// ---------- settings page (admin only) — the system-wide AI model/provider, via
// GET/PUT /api/llm-settings; the switch takes effect on LlmRouter's very next call (see
// backend). This is the only place in the app that can change it — there is no per-user
// choice and no shortcut elsewhere, so every user's questions run on whatever is set here.
const PROVIDER_LABELS = { Anthropic: 'Claude (Anthropic)', OpenAI: 'GPT (OpenAI)', Ollama: 'الموديل الداخلي (Ollama)' };
state.llmSettings = null;
const SETTINGS_MODEL_PICKERS = {
  Ollama: { endpoint: 'ollama-models', rowId: 'settings-ollama-row', selectId: 'settings-ollama-select', noteId: 'settings-ollama-note', activeKey: 'activeOllamaModel', bodyKey: 'ollamaModel' },
  OpenAI: { endpoint: 'openai-models', rowId: 'settings-openai-row', selectId: 'settings-openai-select', noteId: 'settings-openai-note', activeKey: 'activeOpenAiModel', bodyKey: 'openAiModel' },
};
// Same two providers, same live model-list endpoints — but the document reader has its OWN
// sub-model choice (activeDocumentReader{Ollama,OpenAi}Model), independent of the dashboard
// picker above, so a different rowId/selectId/activeKey per provider, read/written through
// PUT /api/llm-settings/document-reader rather than the main settings-save handler below.
const DOC_READER_MODEL_PICKERS = {
  Ollama: { endpoint: 'ollama-models', rowId: 'settings-doc-reader-ollama-row', selectId: 'settings-doc-reader-ollama-select', noteId: 'settings-doc-reader-ollama-note', activeKey: 'activeDocumentReaderOllamaModel', bodyKey: 'ollamaModel' },
  OpenAI: { endpoint: 'openai-models', rowId: 'settings-doc-reader-openai-row', selectId: 'settings-doc-reader-openai-select', noteId: 'settings-doc-reader-openai-note', activeKey: 'activeDocumentReaderOpenAiModel', bodyKey: 'openAiModel' },
};
// Same shape again for the image-analysis picker (which model handles a request that
// attaches a reference image) — its own independent choice, LlmSettingsStore.
// ImageReaderProvider, read/written through PUT /api/llm-settings/image-reader.
const IMAGE_READER_MODEL_PICKERS = {
  Ollama: { endpoint: 'ollama-models', rowId: 'settings-image-reader-ollama-row', selectId: 'settings-image-reader-ollama-select', noteId: 'settings-image-reader-ollama-note', activeKey: 'activeImageReaderOllamaModel', bodyKey: 'ollamaModel' },
  OpenAI: { endpoint: 'openai-models', rowId: 'settings-image-reader-openai-row', selectId: 'settings-image-reader-openai-select', noteId: 'settings-image-reader-openai-note', activeKey: 'activeImageReaderOpenAiModel', bodyKey: 'openAiModel' },
};
// Same shape again for External Integrations' visual-identity extraction picker — its own
// independent choice, LlmSettingsStore.VisualIdentityReaderProvider, read/written through
// PUT /api/llm-settings/visual-identity-reader. Unlike the two pickers above, this one also
// carries an explicit, never-inferred "supports image input" checkbox (activeVisualIdentitySupportsImage).
const VIS_ID_MODEL_PICKERS = {
  Ollama: { endpoint: 'ollama-models', rowId: 'settings-vis-id-ollama-row', selectId: 'settings-vis-id-ollama-select', noteId: 'settings-vis-id-ollama-note', activeKey: 'activeVisualIdentityOllamaModel', bodyKey: 'ollamaModel' },
  OpenAI: { endpoint: 'openai-models', rowId: 'settings-vis-id-openai-row', selectId: 'settings-vis-id-openai-select', noteId: 'settings-vis-id-openai-note', activeKey: 'activeVisualIdentityOpenAiModel', bodyKey: 'openAiModel' },
};
// Keyed by provider id, shared between both picker maps above — it's the same live list of
// models either way (what exists), just used to pre-select a different activeKey per picker.
state.settingsModelChoices = {};

async function loadSettingsPage() {
  el('settings-current').textContent = 'جارٍ التحميل…';
  try {
    const res = await fetch('/api/llm-settings');
    if (!res.ok) throw new Error();
    state.llmSettings = await res.json();
    renderSettingsCurrent();
    renderSettingsProviders();
    renderSettingsInfoBadges();
    renderSettingsDocReaderProviders();
    renderSettingsImageReaderProviders();
    renderSettingsVisIdProviders();
  } catch {
    el('settings-current').textContent = 'تعذّر تحميل إعدادات الموديل.';
    el('settings-providers').innerHTML = '';
  }
}

// "معطّل" (id: '') plus the same three providers the dashboard-building model uses — a
// completely independent choice (LlmSettingsStore.DocumentReaderProvider), read/written
// through its own PUT /api/llm-settings/document-reader rather than the main settings-save
// handler below.
function renderSettingsDocReaderProviders() {
  const s = state.llmSettings;
  const current = s.activeDocumentReaderProvider || '';
  const options = [
    { id: '', label: 'معطّل (استخراج نص عادي فقط)', configured: true },
    ...s.providers,
  ];
  el('settings-doc-reader-providers').innerHTML = options.map(p => `
    <label class="model-switch-option${p.configured ? '' : ' disabled'}">
      <input type="radio" name="settings-doc-reader-provider" value="${esc(p.id)}"
        ${p.id === current ? 'checked' : ''} ${p.configured ? '' : 'disabled'}>
      ${esc(p.label)}
      ${p.id === 'Ollama'
        ? '<span class="badge">⚠️ قد لا يدعم الصور — لو فشل بيرجع تلقائيًا لقراءة PdfPig العادية</span>'
        : (p.id ? `<span class="badge${p.configured ? ' ok' : ''}">${p.configured ? 'مُعد' : 'غير مُعد'}</span>` : '')}
    </label>`).join('');
  el('settings-doc-reader-providers').querySelectorAll('input').forEach(inp =>
    inp.addEventListener('change', () => toggleDocReaderModelRow(inp.value)));
  toggleDocReaderModelRow(current);
}

el('settings-doc-reader-save').addEventListener('click', async () => {
  const provider = el('settings-doc-reader-providers').querySelector('input:checked')?.value ?? '';
  const picker = DOC_READER_MODEL_PICKERS[provider];
  const chosenModel = picker ? el(picker.selectId).value : undefined;
  const body = { provider: provider || null };
  if (picker && chosenModel) body[picker.bodyKey] = chosenModel;

  const btn = el('settings-doc-reader-save');
  btn.disabled = true;
  el('settings-doc-reader-error').classList.add('hidden');
  el('settings-doc-reader-save-note').classList.add('hidden');
  try {
    const res = await fetch('/api/llm-settings/document-reader', {
      method: 'PUT', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    });
    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      throw new Error(err.error || `فشل الحفظ (${res.status})`);
    }
    state.llmSettings.activeDocumentReaderProvider = provider || null;
    if (picker && chosenModel) state.llmSettings[picker.activeKey] = chosenModel;
    renderSettingsDocReaderProviders();
    el('settings-doc-reader-save-note').classList.remove('hidden');
  } catch (err) {
    el('settings-doc-reader-error').textContent = err.message;
    el('settings-doc-reader-error').classList.remove('hidden');
  } finally {
    btn.disabled = false;
  }
});

// "نفس الموديل الحالي" (id: '') plus the same three providers — unlike the document reader's
// own "معطّل" option, an empty choice here doesn't disable anything: a request that attaches
// an image still has to go through some model, so empty just means "use the dashboard-
// building provider above" (see LlmRouter.GenerateDashboardAsync). Independent choice,
// LlmSettingsStore.ImageReaderProvider, via its own PUT /api/llm-settings/image-reader.
function renderSettingsImageReaderProviders() {
  const s = state.llmSettings;
  const current = s.activeImageReaderProvider || '';
  const options = [
    { id: '', label: 'نفس الموديل الحالي', configured: true },
    ...s.providers,
  ];
  el('settings-image-reader-providers').innerHTML = options.map(p => `
    <label class="model-switch-option${p.configured ? '' : ' disabled'}">
      <input type="radio" name="settings-image-reader-provider" value="${esc(p.id)}"
        ${p.id === current ? 'checked' : ''} ${p.configured ? '' : 'disabled'}>
      ${esc(p.label)}
      ${p.id === 'Ollama'
        ? '<span class="badge">⚠️ بعض الموديلات الداخلية لا تدعم تحليل الصور</span>'
        : (p.id ? `<span class="badge${p.configured ? ' ok' : ''}">${p.configured ? 'مُعد' : 'غير مُعد'}</span>` : '')}
    </label>`).join('');
  el('settings-image-reader-providers').querySelectorAll('input').forEach(inp =>
    inp.addEventListener('change', () => toggleImageReaderModelRow(inp.value)));
  toggleImageReaderModelRow(current);
}

el('settings-image-reader-save').addEventListener('click', async () => {
  const provider = el('settings-image-reader-providers').querySelector('input:checked')?.value ?? '';
  const picker = IMAGE_READER_MODEL_PICKERS[provider];
  const chosenModel = picker ? el(picker.selectId).value : undefined;
  const body = { provider: provider || null };
  if (picker && chosenModel) body[picker.bodyKey] = chosenModel;

  const btn = el('settings-image-reader-save');
  btn.disabled = true;
  el('settings-image-reader-error').classList.add('hidden');
  el('settings-image-reader-save-note').classList.add('hidden');
  try {
    const res = await fetch('/api/llm-settings/image-reader', {
      method: 'PUT', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    });
    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      throw new Error(err.error || `فشل الحفظ (${res.status})`);
    }
    state.llmSettings.activeImageReaderProvider = provider || null;
    if (picker && chosenModel) state.llmSettings[picker.activeKey] = chosenModel;
    renderSettingsImageReaderProviders();
    el('settings-image-reader-save-note').classList.remove('hidden');
  } catch (err) {
    el('settings-image-reader-error').textContent = err.message;
    el('settings-image-reader-error').classList.remove('hidden');
  } finally {
    btn.disabled = false;
  }
});

// "معطّل" (id: '') plus the same three providers — same "genuinely disabled, no fallback"
// contract as the document reader (never "use the dashboard-building provider"): sending a
// logo image to a provider nobody explicitly vetted for image input would defeat the whole
// point of the supports-image checkbox below.
function renderSettingsVisIdProviders() {
  const s = state.llmSettings;
  const current = s.activeVisualIdentityProvider || '';
  const options = [
    { id: '', label: 'معطّل (استخراج الشعار بالصور غير متاح)', configured: true },
    ...s.providers,
  ];
  el('settings-vis-id-providers').innerHTML = options.map(p => `
    <label class="model-switch-option${p.configured ? '' : ' disabled'}">
      <input type="radio" name="settings-vis-id-provider" value="${esc(p.id)}"
        ${p.id === current ? 'checked' : ''} ${p.configured ? '' : 'disabled'}>
      ${esc(p.label)}
      ${p.id ? `<span class="badge${p.configured ? ' ok' : ''}">${p.configured ? 'مُعد' : 'غير مُعد'}</span>` : ''}
    </label>`).join('');
  el('settings-vis-id-providers').querySelectorAll('input').forEach(inp =>
    inp.addEventListener('change', () => toggleVisIdModelRow(inp.value)));
  toggleVisIdModelRow(current);
  el('settings-vis-id-supports-image').checked = !!s.activeVisualIdentitySupportsImage;
}

el('settings-vis-id-save').addEventListener('click', async () => {
  const provider = el('settings-vis-id-providers').querySelector('input:checked')?.value ?? '';
  const picker = VIS_ID_MODEL_PICKERS[provider];
  const chosenModel = picker ? el(picker.selectId).value : undefined;
  const body = { provider: provider || null, supportsImage: el('settings-vis-id-supports-image').checked };
  if (picker && chosenModel) body[picker.bodyKey] = chosenModel;

  const btn = el('settings-vis-id-save');
  btn.disabled = true;
  el('settings-vis-id-error').classList.add('hidden');
  el('settings-vis-id-save-note').classList.add('hidden');
  try {
    const res = await fetch('/api/llm-settings/visual-identity-reader', {
      method: 'PUT', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    });
    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      throw new Error(err.error || `فشل الحفظ (${res.status})`);
    }
    state.llmSettings.activeVisualIdentityProvider = provider || null;
    state.llmSettings.activeVisualIdentitySupportsImage = body.supportsImage;
    if (picker && chosenModel) state.llmSettings[picker.activeKey] = chosenModel;
    renderSettingsVisIdProviders();
    el('settings-vis-id-save-note').classList.remove('hidden');
  } catch (err) {
    el('settings-vis-id-error').textContent = err.message;
    el('settings-vis-id-error').classList.remove('hidden');
  } finally {
    btn.disabled = false;
  }
});

function renderSettingsCurrent() {
  const s = state.llmSettings;
  const modelName = s.activeProvider === 'Ollama' ? s.activeOllamaModel : s.activeProvider === 'OpenAI' ? s.activeOpenAiModel : '';
  el('settings-current').innerHTML = `<span class="settings-current-badge">${esc(PROVIDER_LABELS[s.activeProvider] || s.activeProvider)}</span>` +
    (modelName ? `<span class="settings-current-model">${esc(modelName)}</span>` : '');
}

function renderSettingsProviders() {
  const s = state.llmSettings;
  el('settings-providers').innerHTML = s.providers.map(p => `
    <label class="model-switch-option${p.configured ? '' : ' disabled'}">
      <input type="radio" name="settings-provider" value="${esc(p.id)}"
        ${p.id === s.activeProvider ? 'checked' : ''} ${p.configured ? '' : 'disabled'}>
      ${esc(p.label)}
      <span class="badge${p.configured ? ' ok' : ''}">${p.configured ? 'مُعد' : 'غير مُعد'}</span>
    </label>`).join('');
  el('settings-providers').querySelectorAll('input').forEach(inp =>
    inp.addEventListener('change', () => toggleSettingsModelRow(inp.value)));
  toggleSettingsModelRow(s.activeProvider);
}

function toggleSettingsModelRow(provider) { toggleModelRow(SETTINGS_MODEL_PICKERS, provider); }
function toggleDocReaderModelRow(provider) { toggleModelRow(DOC_READER_MODEL_PICKERS, provider); }
function toggleImageReaderModelRow(provider) { toggleModelRow(IMAGE_READER_MODEL_PICKERS, provider); }
function toggleVisIdModelRow(provider) { toggleModelRow(VIS_ID_MODEL_PICKERS, provider); }

function toggleModelRow(pickers, provider) {
  for (const [id, picker] of Object.entries(pickers)) {
    const isActive = provider === id;
    el(picker.rowId).classList.toggle('hidden', !isActive);
    if (isActive) loadSettingsModelChoices(picker, id);
  }
}

// The live model list (state.settingsModelChoices[providerId]) is fetched once and shared
// between the dashboard picker and the document-reader picker — it's the same "what models
// exist" either way; only which one is pre-selected (picker.activeKey) differs.
async function loadSettingsModelChoices(picker, providerId) {
  const select = el(picker.selectId);
  const cached = state.settingsModelChoices[providerId];
  if (cached) { renderModelSelect(picker, cached.models, cached.note); return; }
  select.innerHTML = '<option>جارٍ التحميل…</option>';
  try {
    const res = await fetch(`/api/llm-settings/${picker.endpoint}`);
    const data = await res.json();
    state.settingsModelChoices[providerId] = { models: data.models || [], note: data.note || '' };
    renderModelSelect(picker, data.models || [], data.note || '');
  } catch {
    select.innerHTML = '';
    el(picker.noteId).textContent = 'تعذّر تحميل قائمة الموديلات.';
  }
}

function renderModelSelect(picker, models, note) {
  const current = state.llmSettings?.[picker.activeKey];
  el(picker.selectId).innerHTML = models.map(m =>
    `<option value="${esc(m)}"${m === current ? ' selected' : ''}>${esc(m)}</option>`).join('');
  el(picker.noteId).textContent = note || '';
}

function renderSettingsInfoBadges() {
  const s = state.llmSettings;
  document.querySelectorAll('.settings-info-card').forEach(card => {
    const p = s.providers.find(x => x.id === card.dataset.provider);
    const badge = card.querySelector('.settings-info-badge');
    if (!p || !badge) return;
    badge.textContent = p.configured ? 'مُعد على الخادم' : 'غير مُعد على الخادم';
    badge.classList.toggle('ok', p.configured);
  });
}

el('settings-save').addEventListener('click', async () => {
  const provider = el('settings-providers').querySelector('input:checked')?.value;
  if (!provider) return;
  const picker = SETTINGS_MODEL_PICKERS[provider];
  const chosenModel = picker ? el(picker.selectId).value : undefined;
  const body = { provider };
  if (picker && chosenModel) body[picker.bodyKey] = chosenModel;

  const btn = el('settings-save');
  btn.disabled = true;
  el('settings-error').classList.add('hidden');
  el('settings-save-note').classList.add('hidden');
  try {
    const res = await fetch('/api/llm-settings', {
      method: 'PUT', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    });
    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      throw new Error(err.error || `فشل الحفظ (${res.status})`);
    }
    state.llmSettings.activeProvider = provider;
    if (picker && chosenModel) state.llmSettings[picker.activeKey] = chosenModel;
    renderSettingsCurrent();
    el('settings-save-note').classList.remove('hidden');
  } catch (err) {
    el('settings-error').textContent = err.message;
    el('settings-error').classList.remove('hidden');
  } finally {
    btn.disabled = false;
  }
});

el('login-form').addEventListener('submit', async e => {
  e.preventDefault();
  const username = el('login-username').value.trim();
  const password = el('login-password').value;
  if (!username || !password) return;
  el('login-submit').disabled = true;
  el('login-error').classList.add('hidden');
  try {
    const res = await fetch('/api/auth/login', {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password }),
    });
    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      el('login-error').textContent = err.error || 'تعذّر تسجيل الدخول.';
      el('login-error').classList.remove('hidden');
      return;
    }
    state.currentUser = await res.json();
    el('login-password').value = '';
    hideLoginScreen();
    await startApp();
  } finally {
    el('login-submit').disabled = false;
  }
});

el('logout-btn').addEventListener('click', async () => {
  await fetch('/api/auth/logout', { method: 'POST' });
  location.href = '/';
});

el('theme-toggle').addEventListener('click', () => {
  applyTheme(document.documentElement.getAttribute('data-theme') === 'light' ? 'dark' : 'light');
});
// Sync the button's icon/title with whatever the head script already set on <html>,
// without triggering a dashboard rebuild (nothing is rendered yet at boot).
(() => {
  const current = document.documentElement.getAttribute('data-theme') === 'light' ? 'light' : 'dark';
  el('theme-toggle').textContent = current === 'light' ? '🌙' : '☀️';
  el('theme-toggle').title = current === 'light' ? 'التبديل للوضع الداكن' : 'التبديل للوضع الفاتح';
})();

// Language switch reloads the page rather than re-rendering in place — every screen (chat,
// dashboard, repository, history, settings…) builds its own Arabic strings straight from
// JS, with no single re-render entry point that touches all of them at once the way
// renderDashboard() does for the theme. A reload re-runs the whole boot sequence with
// translateNode()'s MutationObserver (see I18N_EN below) already attached from the very
// first paint, so everything comes up translated consistently instead of needing dozens of
// call sites each manually re-invoked.
el('lang-toggle').addEventListener('click', () => {
  const next = document.documentElement.getAttribute('lang') === 'en' ? 'ar' : 'en';
  try { localStorage.setItem('chatToDashboardLang', next); } catch {}
  location.reload();
});
(() => {
  const current = document.documentElement.getAttribute('lang') === 'en' ? 'en' : 'ar';
  el('lang-toggle').textContent = current === 'en' ? 'AR' : 'EN';
  el('lang-toggle').title = current === 'en' ? 'التبديل للعربية' : 'Switch to English';
})();

// ---------- التكاملات الخارجية (External Integrations) ----------
// Admin-only screen over /api/integrations — settings CRUD, the three visual-identity paths
// (manual/AI-suggest/extraction), the identity-transport NL-match+confirm flow, the publish
// log, and the two deliverable downloads. Publishing itself lives on the dashboard toolbar's
// نشر button (see btn-publish below) — never here, since it acts on an Active dashboard, not
// on integration settings.
state.integrations = [];
state.currentIntegrationId = null;
state.integrationDetail = null;
state.viProposal = null; // last suggest/extract result, pending the analyst's review+apply
state.identityMatch = null;

async function loadIntegrations() {
  el('integrations-list').innerHTML = '<div class="model-switch-note">جارٍ التحميل…</div>';
  try {
    const res = await fetch('/api/integrations');
    if (!res.ok) throw new Error();
    state.integrations = await res.json();
  } catch { state.integrations = []; }
  renderIntegrationsList();
}

function renderIntegrationsList() {
  const box = el('integrations-list');
  if (!state.integrations.length) { box.innerHTML = '<div class="model-switch-note">لا توجد تكاملات بعد.</div>'; return; }
  box.innerHTML = state.integrations.map(i => `
    <div class="integration-row" data-id="${esc(i.id)}">
      <span class="integration-row-name">${esc(i.name)}</span>
      <div class="integration-row-badges">
        <span class="integration-badge ${i.connectorConfigured ? 'ok' : 'warn'}">${i.connectorConfigured ? '✓ خدمة الاتصال مضبوطة' : '⚠️ خدمة الاتصال غير مضبوطة'}</span>
        <span class="integration-badge ${i.identityConfirmed ? 'ok' : 'warn'}">${i.identityConfirmed ? '✓ هوية المستخدم مؤكدة' : '⚠️ هوية المستخدم غير مؤكدة'}</span>
      </div>
    </div>`).join('');
  box.querySelectorAll('.integration-row').forEach(row =>
    row.addEventListener('click', () => openIntegration(row.dataset.id)));
}

el('btn-new-integration').addEventListener('click', async () => {
  const nameInput = el('new-integration-name');
  const name = nameInput.value.trim();
  if (!name) return;
  const res = await fetch('/api/integrations', {
    method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ name }),
  });
  if (!res.ok) { alert('تعذّر إنشاء التكامل.'); return; }
  const created = await res.json();
  nameInput.value = '';
  await loadIntegrations();
  openIntegration(created.id);
});

async function openIntegration(id) {
  state.currentIntegrationId = id;
  state.viProposal = null;
  state.identityMatch = null;
  const res = await fetch('/api/integrations/' + id);
  if (!res.ok) { alert('تعذّر تحميل التكامل.'); return; }
  state.integrationDetail = await res.json();
  const [dashboards, log] = await Promise.all([
    fetch('/api/integrations/' + id + '/dashboards').then(r => r.ok ? r.json() : []),
    fetch('/api/integrations/' + id + '/publish-log').then(r => r.ok ? r.json() : []),
  ]);
  state.integrationDashboards = dashboards;
  state.integrationPublishLog = log;
  renderIntegrationDetail();
  el('integration-detail').scrollIntoView({ behavior: 'smooth', block: 'nearest' });
}

function renderIntegrationDetail() {
  const d = state.integrationDetail;
  const box = el('integration-detail');
  if (!d) { box.innerHTML = ''; return; }

  const identityStatusHtml = d.identityConfirmed
    ? `<span class="integration-badge ok">✓ مؤكدة — ${identityMechanismLabel(d.identityMechanism)} باسم "${esc(d.identityParameterName)}"</span>`
    : (d.identityMechanism
        ? `<span class="integration-badge warn">⚠️ محدّدة لكن غير مؤكدة — ${identityMechanismLabel(d.identityMechanism)} باسم "${esc(d.identityParameterName)}"</span>`
        : `<span class="integration-badge warn">⚠️ لم تُحدَّد بعد</span>`);

  // The one thing that actually gates whether this integration shows up as a buildable data
  // source (see SourcesController.Get) — surfaced here so "why isn't it in the sources list?"
  // has a visible, self-diagnosable answer instead of a silent discovery failure.
  const tableCount = (d.clientSchemaDescription || '').split('\n').filter(Boolean).length;
  const errorNoteHtml = d.lastSchemaDiscoveryError
    ? `<div class="model-switch-note" style="color:var(--danger);margin-top:6px">السبب: ${esc(d.lastSchemaDiscoveryError)}</div>`
    : `<p class="model-switch-note" style="margin-top:6px">تأكد إن خدمة الاتصال شغّالة ومتاحة على الرابط ده وإن قيمة/اسم المصادقة متطابقين، وبعدين احفظ نقاط الاتصال تاني لإعادة المحاولة.</p>`;
  const schemaStatusHtml = d.clientSchemaDescription
    ? `<span class="integration-badge ok">✓ بنية مكتشفة — ${tableCount} جدول (${esc(d.clientDbProvider || '')})${d.dataPermissionsAvailable ? ' · جدول صلاحيات بيانات متاح' : ''}</span>` +
      // The schema itself is never wiped by a later failed refresh (see UpdateClientSchemaAsync/
      // SetSchemaDiscoveryErrorAsync) — so a green badge alone could hide that the MOST RECENT
      // save attempt actually failed and the schema on file may now be stale.
      (d.lastSchemaDiscoveryError
        ? `<div class="model-switch-note" style="color:var(--danger);margin-top:6px">⚠️ آخر محاولة تحديث فشلت — البنية المعروضة فوق من آخر مرة نجحت، ممكن تكون قديمة. السبب: ${esc(d.lastSchemaDiscoveryError)}</div>`
        : '')
    : `<span class="integration-badge warn">⚠️ لسه مفيش بنية مكتشفة — التكامل مش هيظهر في قائمة المصادر لحد ما يتكشف.</span>` + errorNoteHtml;

  box.innerHTML = `
    <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:14px">
      <h3 style="margin:0">⚙️ ${esc(d.name)}</h3>
      <button type="button" id="btn-delete-integration"
        style="border:1px solid var(--danger);color:var(--danger);background:transparent;padding:5px 12px;border-radius:8px;font-size:12px;cursor:pointer">
        🗑️ حذف التكامل</button>
    </div>

    <div class="integ-tabs">
      <button type="button" class="integ-tab active" data-pane="integ-connection">الاتصال</button>
      <button type="button" class="integ-tab" data-pane="integ-visual">الهوية البصرية</button>
      <button type="button" class="integ-tab" data-pane="integ-identity">هوية المستخدم</button>
      <button type="button" class="integ-tab" data-pane="integ-files">الملفات والنشر</button>
    </div>

    <div id="integ-connection" class="integ-pane active">
    <div class="settings-section">
      <form id="integ-apis-form" class="integrations-form" style="display:flex;flex-direction:column;gap:14px">
        <div>
          <div class="model-switch-note" style="margin:0 0 8px;font-weight:700;color:var(--ink)">خدمة الاتصال (connector)</div>
          <div class="integrations-form-grid">
            <label>رابط الخدمة الأساسي (Connector Base URL)
              <input type="text" id="ia-connector-url" value="${escAttr(d.connectorBaseUrl)}" placeholder="مثال: http://connector.client-network:5000">
            </label>
            <label>اسم رأس المصادقة (Header)
              <input type="text" id="ia-connector-auth-header" value="${escAttr(d.connectorAuthHeader)}" placeholder="X-Api-Key">
            </label>
            <label>قيمة المصادقة ${d.connectorAuthConfigured ? '<span class="hint">(محفوظة — اكتب لاستبدالها)</span>' : ''}
              <input type="text" id="ia-connector-auth-value" placeholder="${d.connectorAuthConfigured ? '••••••••' : ''}">
            </label>
          </div>
          <p class="model-switch-note">رابط واحد بس — نقاط الاتصال كلها (الكتابة، القراءة، الصلاحيات،
            الدليل، اكتشاف البنية) عايشة على نفس الخدمة الجاهزة (connector.zip تحت) بشكل ثابت، فمحتاجينش
            نضبطهم كل واحد لوحده. مفتاح المصادقة ده بيحمي بس الاستدعاءات الجاية من عندنا (النشر واكتشاف
            البنية) — صفحتا العرض والإدارة بتكلّموا الخدمة مباشرة من غير مفتاح، لأنهم ملفات ثابتة موزّعة
            ومفيش سر ممكن نحطه فيهم بأمان؛ الحماية هناك بحدود شبكة العميل الداخلية. بمجرد ما الخدمة
            تشتغل عند العميل بقاعدة بياناته الحقيقية وتحفظ الرابط هنا، بنكتشف بنية قاعدة بياناته
            تلقائيًا (وبنعيد الاكتشاف تاني قبل كل عملية نشر) — من غير أي زرار أو خطوة إضافية.</p>
          <div style="margin:8px 0">${schemaStatusHtml}</div>
        </div>
        <div class="model-switch-actions">
          <span id="ia-apis-note" class="settings-save-note hidden">✓ تم الحفظ</span>
          <button type="submit">حفظ نقاط الاتصال</button>
        </div>
      </form>
    </div>
    </div>

    <div id="integ-visual" class="integ-pane">
    <div class="settings-section">
      <h3>الهوية البصرية</h3>
      <div class="vi-swatches">
        <div class="vi-swatch"><span class="vi-swatch-dot" style="background:${escAttr(d.accentColor) || '#2AB37F'}"></span>أساسي: ${esc(d.accentColor || '(افتراضي)')}</div>
        <div class="vi-swatch"><span class="vi-swatch-dot" style="background:${escAttr(d.secondaryColor) || '#0B2A22'}"></span>ثانوي: ${esc(d.secondaryColor || '(افتراضي)')}</div>
        <div class="vi-swatch">الخط: ${esc(d.fontFamily || '(افتراضي)')}</div>
      </div>
      <div class="vi-tabs">
        <button type="button" class="vi-tab active" data-pane="vi-manual">يدوي</button>
        <button type="button" class="vi-tab" data-pane="vi-suggest">اقتراح بالذكاء الاصطناعي</button>
        <button type="button" class="vi-tab" data-pane="vi-extract">استخراج من ملف</button>
      </div>

      <div id="vi-manual" class="vi-pane active">
        <form id="integ-vi-manual-form" class="integrations-form">
          <div class="integrations-form-grid">
            <label>اللون الأساسي (hex)
              <input type="text" id="vi-accent" value="${escAttr(d.accentColor) || '#2AB37F'}" placeholder="#2AB37F">
            </label>
            <label>اللون الثانوي (hex)
              <input type="text" id="vi-secondary" value="${escAttr(d.secondaryColor) || '#0B2A22'}" placeholder="#0B2A22">
            </label>
            <label>اسم الخط
              <input type="text" id="vi-font" value="${escAttr(d.fontFamily) || 'Inter'}" placeholder="Inter">
            </label>
          </div>
          <div class="model-switch-actions">
            <span id="ia-vi-note" class="settings-save-note hidden">✓ تم الحفظ</span>
            <button type="submit">حفظ الهوية البصرية</button>
          </div>
        </form>
      </div>

      <div id="vi-suggest" class="vi-pane">
        <div class="integrations-form">
          <label>وصف مختصر (اختياري لو عندك لون)
            <textarea id="vi-desc" placeholder="مثال: أزرق داكن وذهبي، خط رسمي"></textarea>
          </label>
          <label>لون معروف بالفعل (اختياري لو عندك وصف)
            <input type="text" id="vi-seed-color" placeholder="#123456">
          </label>
          <div class="model-switch-actions">
            <button type="button" id="btn-vi-suggest">اقترح هوية بصرية</button>
          </div>
        </div>
        <div id="vi-suggest-result"></div>
      </div>

      <div id="vi-extract" class="vi-pane">
        <div class="integrations-form" style="gap:10px">
          <div class="deliverable-row">
            <button type="button" id="btn-vi-extract-palette">🎨 استخراج ألوان من صورة (بدون AI)</button>
            <input type="file" id="vi-file-palette" accept="image/*" class="hidden">
          </div>
          <div class="deliverable-row">
            <button type="button" id="btn-vi-extract-font">🔤 استخراج اسم خط من ملف PDF</button>
            <input type="file" id="vi-file-font" accept="application/pdf" class="hidden">
          </div>
          <div class="deliverable-row">
            <button type="button" id="btn-vi-extract-image">🖼️ استخراج هوية من صورة شعار (AI بصري)</button>
            <input type="file" id="vi-file-image" accept="image/*" class="hidden">
          </div>
          <p class="model-switch-note">الاستخراج بالصورة (AI بصري) يحتاج موديل مُعد صراحةً كداعم للصور من
            <a href="#" id="link-to-vi-model-settings">إعدادات نموذج استخراج الهوية البصرية</a> — غير كده هيترفض بوضوح.</p>
        </div>
        <div id="vi-extract-result"></div>
      </div>
    </div>
    </div>

    <div id="integ-identity" class="integ-pane">
    <div class="settings-section">
      <h3>آلية تعريف المستخدم الحالي</h3>
      <div class="identity-status">${identityStatusHtml}</div>
      <div class="integrations-form">
        <label>اوصف إزاي نظام العميل بيعرّف هوية المستخدم الحالي (بالعربي، بلغتك)
          <textarea id="id-transport-desc" placeholder="مثال: بيبعتوا معرف المستخدم في رأس HTTP اسمه X-User-Id"></textarea>
        </label>
        <div class="model-switch-actions">
          <button type="button" id="btn-id-match">طابق الوصف مع آلية</button>
        </div>
      </div>
      <div id="id-match-result"></div>

      <div class="integrations-form" style="margin-top:14px">
        <div class="integrations-form-grid">
          <label>الآلية
            <select id="id-mechanism">
              <option value="query" ${d.identityMechanism === 'query' ? 'selected' : ''}>معامل ضمن الرابط (Query Parameter)</option>
              <option value="header" ${d.identityMechanism === 'header' ? 'selected' : ''}>رأس HTTP مخصّص (Header)</option>
              <option value="cookie" ${d.identityMechanism === 'cookie' ? 'selected' : ''}>كوكي (Cookie)</option>
            </select>
          </label>
          <label>الاسم الدقيق للمعامل/الرأس/الكوكي
            <input type="text" id="id-param-name" value="${escAttr(d.identityParameterName)}" placeholder="X-User-Id">
          </label>
        </div>
        <div class="model-switch-actions">
          <span id="ia-id-pending-note" class="settings-save-note hidden">✓ تم الحفظ كمعلّق</span>
          <button type="button" id="btn-id-save-pending">حفظ كمعلّق</button>
          <button type="button" id="btn-id-confirm" style="background:var(--accent);color:#fff;border:0">✅ تأكيد وتفعيل</button>
        </div>
      </div>
    </div>
    </div>

    <div id="integ-files" class="integ-pane">
    <div class="settings-section">
      <h3>الملفات الجاهزة للعميل</h3>
      <p class="model-switch-note">نزّل الملفات الثلاثة وسلّمهم لفريق العميل التقني: صفحتا العرض
        وإدارة الصلاحيات يُستضافوا على نطاقهم، وخدمة الاتصال (connector) تُشغَّل جوه شبكتهم بعد
        ما يحطّوا فيها رقم الاتصال بقاعدة بياناتهم الحقيقية محليًا (شرح كامل داخل الملف نفسه —
        README.md). أعد التنزيل بعد أي تعديل في الإعدادات أعلاه — الملفات لا تتحدّث تلقائيًا.</p>
      <div class="deliverable-row">
        <a href="/api/integrations/${esc(d.id)}/deliverables/viewer"><button type="button">⬇️ صفحة العرض (viewer.html)</button></a>
        <a href="/api/integrations/${esc(d.id)}/deliverables/admin"><button type="button">⬇️ صفحة إدارة الصلاحيات (admin.html)</button></a>
        <a href="/api/integrations/${esc(d.id)}/deliverables/connector"><button type="button">⬇️ خدمة الاتصال بقاعدة البيانات (connector.zip)</button></a>
      </div>
    </div>

    <div class="settings-section">
      <h3>اللوحات المنشورة وسجل النشر</h3>
      ${integrationDashboardsTableHtml(state.integrationDashboards)}
      <div class="model-switch-note" style="margin-top:14px;font-weight:700;color:var(--ink)">آخر عمليات النشر</div>
      ${integrationPublishLogTableHtml(state.integrationPublishLog)}
    </div>
    </div>
  `;
  wireIntegrationDetailHandlers();
}

function identityMechanismLabel(m) {
  return m === 'query' ? 'معامل رابط' : m === 'header' ? 'رأس HTTP' : m === 'cookie' ? 'كوكي' : m || '';
}

function integrationDashboardsTableHtml(rows) {
  if (!rows || !rows.length) return '<p class="model-switch-note">لا توجد لوحات منشورة بعد — استخدم زر "نشر" من شريط أدوات أي لوحة مفعّلة (Active).</p>';
  return `<table class="integ-table"><thead><tr><th>العنوان</th><th>معرّف اللوحة عند العميل</th><th>أول نشر</th><th>آخر نشر</th><th>بواسطة</th></tr></thead>
    <tbody>${rows.map(r => `<tr><td>${esc(r.title)}</td><td style="font-family:var(--mono);font-size:11px">${esc(r.externalDashboardId)}</td>
      <td>${esc(relTime(r.firstPublishedAt))}</td><td>${esc(relTime(r.lastPublishedAt))}</td><td>${esc(r.lastPublishedBy)}</td></tr>`).join('')}</tbody></table>`;
}

function integrationPublishLogTableHtml(rows) {
  if (!rows || !rows.length) return '<p class="model-switch-note">لا يوجد سجل نشر بعد.</p>';
  return `<table class="integ-table"><thead><tr><th>الحالة</th><th>اللوحة</th><th>الوقت</th><th>تفاصيل</th></tr></thead>
    <tbody>${rows.map(r => `<tr><td>${r.success ? '✅' : '❌'}</td><td>${esc(r.dashboardTitle)}</td>
      <td>${esc(relTime(r.createdAt))}</td><td style="color:var(--danger);font-size:11px">${esc(r.error || '')}</td></tr>`).join('')}</tbody></table>`;
}

function wireIntegrationDetailHandlers() {
  const id = state.currentIntegrationId;

  document.querySelectorAll('.integ-tab').forEach(tab => tab.addEventListener('click', () => {
    document.querySelectorAll('.integ-tab').forEach(t => t.classList.toggle('active', t === tab));
    document.querySelectorAll('.integ-pane').forEach(p => p.classList.toggle('active', p.id === tab.dataset.pane));
  }));

  el('btn-delete-integration').addEventListener('click', async () => {
    if (!confirm('حذف هذا التكامل نهائيًا؟ هيتشال معاه سجل النشر واللوحات المرتبطة به.')) return;
    await fetch('/api/integrations/' + id, { method: 'DELETE' });
    state.currentIntegrationId = null;
    state.integrationDetail = null;
    el('integration-detail').innerHTML = '';
    await loadIntegrations();
  });

  el('integ-apis-form').addEventListener('submit', async e => {
    e.preventDefault();
    const body = {
      connectorBaseUrl: el('ia-connector-url').value.trim() || null,
      connectorAuthHeader: el('ia-connector-auth-header').value.trim() || null,
      connectorAuthValue: el('ia-connector-auth-value').value.trim() || null,
    };
    const res = await fetch('/api/integrations/' + id + '/apis', {
      method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body),
    });
    if (!res.ok) { alert('تعذّر الحفظ.'); return; }
    await loadIntegrations();
    // Keep the credential-value fields blank (already-saved secrets are represented by the
    // "configured" placeholder text, never echoed back) — a full re-render after refetching
    // the detail keeps everything else in sync.
    const refreshed = await fetch('/api/integrations/' + id).then(r => r.json());
    state.integrationDetail = refreshed;
    renderIntegrationDetail();
    el('ia-apis-note').classList.remove('hidden');
  });


  document.querySelectorAll('.vi-tab').forEach(tab => tab.addEventListener('click', () => {
    document.querySelectorAll('.vi-tab').forEach(t => t.classList.toggle('active', t === tab));
    document.querySelectorAll('.vi-pane').forEach(p => p.classList.toggle('active', p.id === tab.dataset.pane));
  }));

  el('integ-vi-manual-form').addEventListener('submit', async e => {
    e.preventDefault();
    const body = { accentColor: el('vi-accent').value.trim(), secondaryColor: el('vi-secondary').value.trim(), fontFamily: el('vi-font').value.trim() };
    const res = await fetch('/api/integrations/' + id + '/visual-identity', {
      method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body),
    });
    if (!res.ok) { const err = await res.json().catch(() => ({})); alert(err.error || 'تعذّر الحفظ.'); return; }
    el('ia-vi-note').classList.remove('hidden');
    await openIntegration(id);
  });

  el('btn-vi-suggest').addEventListener('click', async () => {
    const description = el('vi-desc').value.trim();
    const seedColor = el('vi-seed-color').value.trim();
    const btn = el('btn-vi-suggest');
    btn.disabled = true;
    el('vi-suggest-result').innerHTML = '<p class="model-switch-note">جارٍ التوليد…</p>';
    try {
      const res = await fetch('/api/integrations/' + id + '/visual-identity/suggest', {
        method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ description, seedColor }),
      });
      const data = await res.json();
      if (!res.ok) { el('vi-suggest-result').innerHTML = `<p class="model-switch-note bad">${esc(data.error || 'تعذّر التوليد.')}</p>`; return; }
      renderViProposal('vi-suggest-result', data);
    } finally { btn.disabled = false; }
  });

  wireViFileButton('btn-vi-extract-palette', 'vi-file-palette', '/visual-identity/extract-palette', 'vi-extract-result');
  wireViFileButton('btn-vi-extract-font', 'vi-file-font', '/visual-identity/extract-pdf-font', 'vi-extract-result', true);
  wireViFileButton('btn-vi-extract-image', 'vi-file-image', '/visual-identity/extract-image', 'vi-extract-result');

  el('link-to-vi-model-settings').addEventListener('click', e => { e.preventDefault(); showScreen('settings'); });

  el('btn-id-match').addEventListener('click', async () => {
    const description = el('id-transport-desc').value.trim();
    if (!description) return;
    const btn = el('btn-id-match');
    btn.disabled = true;
    el('id-match-result').innerHTML = '<p class="model-switch-note">جارٍ التحليل…</p>';
    try {
      const res = await fetch('/api/integrations/' + id + '/identity-transport/match', {
        method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ description }),
      });
      const match = await res.json();
      el('id-match-result').innerHTML = `<div class="identity-match-result">${esc(match.summary)}</div>`;
      if (match.matched) {
        el('id-mechanism').value = match.mechanism;
        el('id-param-name').value = match.parameterName;
      }
    } finally { btn.disabled = false; }
  });

  el('btn-id-save-pending').addEventListener('click', async () => {
    const mechanism = el('id-mechanism').value;
    const parameterName = el('id-param-name').value.trim();
    if (!parameterName) { alert('اكتب اسم المعامل/الرأس/الكوكي أولاً.'); return; }
    const res = await fetch('/api/integrations/' + id + '/identity-transport/pending', {
      method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ mechanism, parameterName }),
    });
    if (!res.ok) { const err = await res.json().catch(() => ({})); alert(err.error || 'تعذّر الحفظ.'); return; }
    el('ia-id-pending-note').classList.remove('hidden');
    const refreshed = await fetch('/api/integrations/' + id).then(r => r.json());
    state.integrationDetail = refreshed;
  });

  el('btn-id-confirm').addEventListener('click', async () => {
    const mechanism = el('id-mechanism').value;
    const parameterName = el('id-param-name').value.trim();
    if (!parameterName) { alert('اكتب اسم المعامل/الرأس/الكوكي أولاً.'); return; }
    const summary = `ستقرأ صفحة العرض هوية المستخدم من ${identityMechanismLabel(mechanism)} اسمه "${parameterName}"، وترسلها بنفس الاسم إلى نقطة القراءة عندكم في كل استدعاء. متأكد إنك عايز تفعّل ده؟`;
    if (!confirm(summary)) return;
    await fetch('/api/integrations/' + id + '/identity-transport/pending', {
      method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ mechanism, parameterName }),
    });
    const res = await fetch('/api/integrations/' + id + '/identity-transport/confirm', { method: 'POST' });
    if (!res.ok) { const err = await res.json().catch(() => ({})); alert(err.error || 'تعذّر التأكيد.'); return; }
    await openIntegration(id);
  });
}

function renderViProposal(targetId, proposal) {
  const note = proposal.adjusted ? `<div class="vi-proposal-note">⚠️ ${esc(proposal.adjustmentNote)}</div>` : '';
  el(targetId).innerHTML = `
    <div class="vi-proposal">
      <div class="vi-swatches">
        <div class="vi-swatch"><span class="vi-swatch-dot" style="background:${escAttr(proposal.accentColor)}"></span>${esc(proposal.accentColor)}</div>
        <div class="vi-swatch"><span class="vi-swatch-dot" style="background:${escAttr(proposal.secondaryColor)}"></span>${esc(proposal.secondaryColor)}</div>
        <div class="vi-swatch">${esc(proposal.fontFamily)}</div>
      </div>
      ${note}
      <div class="model-switch-actions"><button type="button" id="btn-vi-apply-proposal">تطبيق هذا الاقتراح</button></div>
    </div>`;
  el('btn-vi-apply-proposal').addEventListener('click', () => {
    el('vi-accent').value = proposal.accentColor;
    el('vi-secondary').value = proposal.secondaryColor;
    el('vi-font').value = proposal.fontFamily;
    document.querySelector('.vi-tab[data-pane="vi-manual"]').click();
  });
}

/// Font extraction only returns { fontFamily } (a PDF's own text carries no colors) — its
/// proposal box merges that with the CURRENT accent/secondary rather than the full three-token
/// UI the other two paths show, since there is nothing else to apply.
function renderFontProposal(targetId, fontFamily) {
  el(targetId).innerHTML = `
    <div class="vi-proposal">
      <div class="vi-swatch">الخط المستخرج: <strong>${esc(fontFamily)}</strong></div>
      <div class="model-switch-actions"><button type="button" id="btn-vi-apply-font">تطبيق اسم الخط هذا</button></div>
    </div>`;
  el('btn-vi-apply-font').addEventListener('click', () => {
    el('vi-font').value = fontFamily;
    document.querySelector('.vi-tab[data-pane="vi-manual"]').click();
  });
}

function wireViFileButton(btnId, inputId, endpointSuffix, resultId, isFont) {
  const btn = el(btnId);
  const input = el(inputId);
  btn.addEventListener('click', () => input.click());
  input.addEventListener('change', async () => {
    const file = input.files[0];
    input.value = '';
    if (!file) return;
    const id = state.currentIntegrationId;
    const form = new FormData();
    form.append('file', file);
    el(resultId).innerHTML = '<p class="model-switch-note">جارٍ المعالجة…</p>';
    try {
      const res = await fetch('/api/integrations/' + id + endpointSuffix, { method: 'POST', body: form });
      const data = await res.json();
      if (!res.ok) { el(resultId).innerHTML = `<p class="model-switch-note bad">${esc(data.error || 'تعذّرت المعالجة.')}</p>`; return; }
      if (isFont) renderFontProposal(resultId, data.fontFamily);
      else renderViProposal(resultId, data);
    } catch (err) {
      el(resultId).innerHTML = `<p class="model-switch-note bad">${esc(err.message)}</p>`;
    }
  });
}

function escAttr(s) { return esc(s ?? '').replace(/"/g, '&quot;'); }

// ---------- guided tour ----------
// A short, dismissible walkthrough of the main screens — starts automatically once per user
// per browser (see maybeAutoStartTour, called from startApp), and can be replayed any time
// via the ❓ button. Each step either highlights a real header element (the nav tabs
// themselves, always present regardless of which screen is active — no screen-switching
// needed to keep them visible) or, for chat-only controls, relies on startTour forcing the
// chat screen active first.
function buildTourSteps() {
  const isAdmin = state.currentUser?.role === 'Admin';
  const steps = [
    {
      title: '👋 مرحبًا بك',
      body: 'جولة موجزة تتناول أهم صفحات النظام. يمكنك تخطّيها الآن، وإعادة تشغيلها في أي وقت لاحقًا من الزر ❓ أعلى الصفحة.',
    },
    {
      target: '[data-screen="chat"]',
      title: '💬 المحادثة',
      body: 'الصفحة الرئيسية: اكتب سؤالك بلغة عربية طبيعية عن بياناتك، ويقوم النظام ببناء لوحة معلومات تفاعلية فورًا.',
    },
    {
      target: '#chat-mode-tabs',
      title: '🧭 بناء اللوحة / الاستفسارات',
      body: '«بناء اللوحة» يحوّل سؤالك إلى عناصر رسومية وجداول. أما «الاستفسارات» فيقدّم إجابة نصية سريعة دون بناء لوحة — مناسب للأسئلة المباشرة.',
    },
    {
      target: '#sources-btn',
      title: '📁 مصادر البيانات',
      body: 'من هنا يمكن التحكم في الأنظمة أو الملفات المفعّلة وقت السؤال، واقتصار البحث على مصدر معيّن عند الحاجة.',
    },
    {
      target: '[data-screen="repo"]',
      title: '📎 مستودع الملفات',
      body: 'من هذه الصفحة تُرفع ملفات Excel أو CSV أو PDF، لتُستخدم كمصدر بيانات يمكن الاستفسار عنه أسوة بأي نظام متصل.',
    },
    {
      target: '[data-screen="history"]',
      title: '🕘 السجل',
      body: 'يضم جميع اللوحات التي سبق إنشاؤها، ويتيح العودة إلى أي منها لمتابعة العمل عليها أو تعديلها.',
    },
    {
      target: '[data-screen="active"]',
      title: '✅ اللوحات النشطة',
      body: 'لوحات مفعّلة يمكن مشاركتها مع فريق العمل بأدوار مختلفة (مالك/محرر/مشاهد)، وتظل بياناتها تتحدّث تلقائيًا من المصدر الحي.',
    },
    {
      target: '[data-screen="sharelinks"]',
      title: '🔗 روابط المشاركة',
      body: 'روابط ثابتة لمشاركة نسخة لحظية من لوحة مع أي شخص، دون الحاجة إلى تسجيل الدخول.',
    },
  ];
  if (isAdmin) {
    steps.push(
      {
        target: '#tab-users',
        title: '👥 المستخدمون',
        body: 'إدارة حسابات المستخدمين وأدوارهم والأنظمة المسموح لهم بالوصول إليها.',
      },
      {
        target: '#tab-settings',
        title: '⚙️ الإعدادات',
        body: 'التحكم في نموذج الذكاء الاصطناعي المستخدم لبناء اللوحات، وفي نموذج قراءة ملفات PDF بشكل مستقل عنه.',
      },
      {
        target: '#usage-link',
        title: '📊 الاستهلاك',
        body: 'سجل تفصيلي لكل سؤال أُرسل إلى النموذج: عدد التوكنز المستخدمة، التكلفة التقديرية، والمستخدم الذي طرح السؤال.',
      },
    );
  }
  steps.push({
    target: '#tour-btn',
    title: '❓ هل تحتاج إلى الدليل مرة أخرى؟',
    body: 'اضغط هذا الزر في أي وقت لإعادة عرض هذا الدليل من البداية.',
  });
  return steps;
}

let tourSteps = [];
let tourIndex = 0;

function startTour() {
  tourSteps = buildTourSteps();
  tourIndex = 0;
  showScreen('chat'); // guarantees the chat-only steps' targets are actually visible
  el('tour-dim').classList.add('open');
  renderTourStep();
}

function endTour() {
  el('tour-dim').classList.remove('open');
  el('tour-pop').classList.remove('open');
  document.querySelectorAll('.tour-target').forEach(e => e.classList.remove('tour-target'));
}

function renderTourStep() {
  document.querySelectorAll('.tour-target').forEach(e => e.classList.remove('tour-target'));
  const step = tourSteps[tourIndex];
  if (!step) { endTour(); return; }

  const pop = el('tour-pop');
  const isFirst = tourIndex === 0;
  const isLast = tourIndex === tourSteps.length - 1;
  pop.innerHTML = `
    <h4>${esc(step.title)}</h4>
    <p>${esc(step.body)}</p>
    <div class="tour-foot">
      <span class="tour-step-count">${tourIndex + 1} من ${tourSteps.length}</span>
      <div class="tour-actions">
        ${!isFirst ? '<button type="button" id="tour-prev">السابق</button>' : ''}
        <button type="button" id="tour-next" class="primary">${isLast ? 'إنهاء' : 'التالي'}</button>
      </div>
    </div>
    ${!isLast ? '<button type="button" class="tour-skip" id="tour-skip">تخطّي الدليل</button>' : ''}`;
  pop.classList.add('open');

  const targetEl = step.target ? document.querySelector(step.target) : null;
  if (targetEl) {
    targetEl.classList.add('tour-target');
    positionTourPopover(targetEl, pop);
  } else {
    pop.style.top = '50%'; pop.style.insetInlineStart = '50%'; pop.style.transform = 'translate(-50%,-50%)';
  }

  el('tour-next').addEventListener('click', () => { tourIndex++; renderTourStep(); });
  el('tour-prev')?.addEventListener('click', () => { tourIndex--; renderTourStep(); });
  el('tour-skip')?.addEventListener('click', endTour);
}

/// Plain top/left (not RTL logical properties) since getBoundingClientRect() is always
/// viewport/physical-coordinate, regardless of document direction.
function positionTourPopover(targetEl, pop) {
  pop.style.transform = 'none';
  pop.style.insetInlineStart = '';
  const rect = targetEl.getBoundingClientRect();
  const popRect = pop.getBoundingClientRect();
  const margin = 14;
  let top = rect.bottom + margin;
  if (top + popRect.height > window.innerHeight - 10) top = Math.max(10, rect.top - popRect.height - margin);
  let left = Math.min(Math.max(10, rect.left), window.innerWidth - popRect.width - 10);
  pop.style.top = `${top}px`;
  pop.style.left = `${left}px`;
}

el('tour-btn').addEventListener('click', startTour);
el('tour-dim').addEventListener('click', endTour);

/// Once per user per browser, a little while after their first successful login or
/// session-restore — long enough for the chat screen to have settled. Marks itself seen the
/// moment it decides to auto-start (not when the user finishes), so it never re-nags even if
/// they close the tab mid-tour; they can always replay it manually via #tour-btn regardless.
function maybeAutoStartTour() {
  const key = `tourSeen:${state.currentUser?.username || 'anon'}`;
  try {
    if (localStorage.getItem(key)) return;
    localStorage.setItem(key, '1');
  } catch { return; }
  setTimeout(startTour, 700);
}

async function startApp() {
  renderCurrentUser();
  renderMessages();
  renderDashboard();
  await loadSources();
  await loadFiles();
  maybeAutoStartTour();
}

// ---------- users admin ----------
async function loadUsers() {
  const res = await fetch('/api/users');
  state.users = res.ok ? await res.json() : [];
  renderUsers();
}

function renderUsers() {
  el('users-count').textContent = `${state.users.length} مستخدم`;
  el('users-tbody').innerHTML = state.users.map(u => `
    <tr>
      <td>${esc(u.username)}</td>
      <td>${esc(u.displayName)}</td>
      <td>${u.authMethod === 'ActiveDirectory' ? 'Active Directory' : 'محلي'}</td>
      <td><span class="role-badge${u.role === 'Admin' ? ' admin' : ''}">${u.role === 'Admin' ? 'مسؤول' : 'مستخدم'}</span></td>
      <td>${u.allowAllSystems ? 'كل الأنظمة' : `${u.allowedSystems.length} نظام`}</td>
      <td><span class="status-dot${u.isActive ? '' : ' off'}"></span>${u.isActive ? 'نشط' : 'معطّل'}</td>
      <td>
        <button class="edit-btn" data-edit="${esc(u.id)}">تعديل</button>
        <button class="del-btn" data-del="${esc(u.id)}">حذف</button>
      </td>
    </tr>`).join('');

  el('users-tbody').querySelectorAll('[data-edit]').forEach(btn =>
    btn.addEventListener('click', () => openUserModal(state.users.find(u => u.id === btn.dataset.edit))));
  el('users-tbody').querySelectorAll('[data-del]').forEach(btn =>
    btn.addEventListener('click', () => deleteUser(btn.dataset.del)));
}

function renderPermChecklist(containerId, items, checkedList, keyFn, labelFn) {
  const container = el(containerId);
  if (!items.length) { container.innerHTML = '<span class="empty">لا يوجد عناصر بعد</span>'; return; }
  container.innerHTML = items.map(item => {
    const key = keyFn(item);
    return `<label><input type="checkbox" value="${esc(key)}" ${checkedList.includes(key) ? 'checked' : ''}> ${esc(labelFn(item))}</label>`;
  }).join('');
}

function togglePermList(listId, allCheckboxId) {
  el(listId).classList.toggle('hidden', el(allCheckboxId).checked);
}

el('user-allow-all-systems').addEventListener('change', () => togglePermList('user-systems-list', 'user-allow-all-systems'));
el('user-authmethod').addEventListener('change', () => {
  el('user-password-row').style.display = el('user-authmethod').value === 'Local' ? '' : 'none';
});

function openUserModal(user) {
  el('user-form-error').classList.add('hidden');
  el('user-id').value = user ? user.id : '';
  el('user-modal-title').textContent = user ? `تعديل ${user.username}` : 'مستخدم جديد';
  el('user-username').value = user ? user.username : '';
  el('user-username').disabled = !!user;
  el('user-displayname').value = user ? user.displayName : '';
  el('user-authmethod').value = user ? user.authMethod : 'Local';
  el('user-role').value = user ? user.role : 'User';
  el('user-active').checked = user ? user.isActive : true;
  el('user-password').value = '';
  el('user-password-hint').textContent = user ? '(اتركها فاضية لعدم التغيير)' : '';
  el('user-password-row').style.display = (user ? user.authMethod : 'Local') === 'Local' ? '' : 'none';

  el('user-allow-all-systems').checked = user ? user.allowAllSystems : true;
  renderPermChecklist('user-systems-list', state.systems, user ? user.allowedSystems : [], s => s.id, s => s.name);
  togglePermList('user-systems-list', 'user-allow-all-systems');

  el('user-modal').classList.remove('hidden');
}

el('btn-new-user').addEventListener('click', () => openUserModal(null));
el('user-cancel').addEventListener('click', () => el('user-modal').classList.add('hidden'));
el('user-modal').addEventListener('click', e => { if (e.target.id === 'user-modal') el('user-modal').classList.add('hidden'); });

el('user-form').addEventListener('submit', async e => {
  e.preventDefault();
  const id = el('user-id').value;
  const body = {
    username: el('user-username').value.trim(),
    displayName: el('user-displayname').value.trim(),
    password: el('user-password').value || null,
    authMethod: el('user-authmethod').value,
    role: el('user-role').value,
    isActive: el('user-active').checked,
    allowAllSystems: el('user-allow-all-systems').checked,
    allowedSystems: [...el('user-systems-list').querySelectorAll('input:checked')].map(i => i.value),
    // File permissions are managed only from "مستودع الملفات" now — the server ignores
    // these two regardless (see UsersController), but omitting them keeps the request
    // honest about what this form actually controls.
  };
  const saveBtn = el('user-save');
  saveBtn.disabled = true;
  try {
    const res = await fetch(id ? `/api/users/${id}` : '/api/users', {
      method: id ? 'PUT' : 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    });
    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      el('user-form-error').textContent = err.error || `فشل الحفظ (${res.status})`;
      el('user-form-error').classList.remove('hidden');
      return;
    }
    el('user-modal').classList.add('hidden');
    await loadUsers();
  } finally {
    saveBtn.disabled = false;
  }
});

async function deleteUser(id) {
  const user = state.users.find(u => u.id === id);
  if (!user || !confirm(`حذف المستخدم "${user.username}"؟`)) return;
  const res = await fetch('/api/users/' + id, { method: 'DELETE' });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    alert(err.error || 'تعذّر الحذف.');
    return;
  }
  await loadUsers();
}

(async function boot() {
  const shareId = new URLSearchParams(location.search).get('share');
  if (shareId) { await bootSharedView(shareId); return; }

  const res = await fetch('/api/auth/me');
  if (!res.ok) { showLoginScreen(); return; }
  state.currentUser = await res.json();
  await startApp();
})();
