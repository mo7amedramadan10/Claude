using System.Linq;

namespace ChatToDashboard.Api.Templates;

public record BuiltinWidgetTemplate(string Id, string Category, string Icon, string Title, string Description, string Prompt);

/// <summary>One of a dashboard template's sub-widgets. <paramref name="Prompt"/> is optional
/// extra instruction text for this one widget specifically — appended to its title when the
/// {{widgets}} variable is expanded (see TemplatePromptService.ResolveDashboardAsync).
/// <paramref name="Page"/> is the reference design's own tab/page name for a multi-page
/// template (null for a single-page one) — ChatController.Post reconciles the model's response
/// against this list by literal title match and stamps each resulting DashboardWidget.Page from
/// here, so the real tab bar never depends on the model echoing a page field itself.</summary>
public record BuiltinDashboardWidgetSpec(string Type, string Title, string? Prompt = null, string? Page = null);

public record BuiltinDashboardTemplate(
    string Id, string Layout, string Category, string Name, bool Popular, string Description,
    string[] Sources, BuiltinDashboardWidgetSpec[] Widgets, string Prompt);

/// <summary>The 8 chat-widget templates, 8 dashboard templates and 6 KPI measure-type prompts
/// that shipped baked into wwwroot/js/app.js before the "مكتبة النماذج" admin screen — ported
/// here verbatim so they remain each item's built-in default (what a platform-owner's "رجوع
/// للنص الأصلي" reverts to, and what a never-touched item resolves to) once the frontend stopped
/// carrying the prompt text itself (see TemplatePromptService, ChatController.Post).</summary>
public static class BuiltinTemplates
{
    public static readonly IReadOnlyList<BuiltinWidgetTemplate> Widgets = new[]
    {
        new BuiltinWidgetTemplate("kpi-target-region", "kpi", "spark", "تحقيق المستهدف بالمناطق",
            "نسبة الوفاء بالمستهدف لكل منطقة، مع أعلى وأقل منطقة أداءً.",
            "أضف مؤشرًا يوضح نسبة تحقيق المستهدف موزّعة حسب المناطق، مع إبراز أعلى وأقل منطقة أداءً."),
        new BuiltinWidgetTemplate("kpi-trend", "kpi", "up", "مؤشر رقمي مع اتجاه",
            "أهم رقم عندك، مع نسبة تغيّره عن الفترة السابقة.",
            "أضف مؤشرًا رقميًا لأهم قيمة في بياناتي، مع نسبة التغيّر مقارنة بالفترة السابقة."),
        new BuiltinWidgetTemplate("chart-growth", "chart", "chart", "نمو العملاء الجدد",
            "رسم خطي لعدد العملاء الجدد شهريًا خلال آخر سنة.",
            "أضف رسمًا بيانيًا خطيًا يوضح نمو عدد العملاء الجدد شهريًا خلال آخر 12 شهرًا."),
        new BuiltinWidgetTemplate("chart-yoy", "chart", "calendar", "مقارنة سنوية",
            "أهم مؤشر عندك مقارنًا شهريًا بنفس الفترة من السنة السابقة.",
            "أضف رسمًا بيانيًا يقارن أهم مؤشر في بياناتي بين هذه السنة والسنة السابقة شهريًا."),
        new BuiltinWidgetTemplate("chart-funnel", "chart", "filter", "مراحل الصفقات",
            "توزيع الصفقات أو الطلبات على مراحلها كقمع.",
            "أضف رسم قمع (funnel) يوضح توزيع الصفقات أو الطلبات على مراحلها المختلفة."),
        new BuiltinWidgetTemplate("chart-channels", "chart", "share", "توزيع قنوات البيع",
            "رسم دائري لحصة كل قناة أو مصدر من إجمالي المبيعات.",
            "أضف رسمًا دائريًا (donut) يوضح توزيع المبيعات أو الطلبات حسب القناة أو المصدر."),
        new BuiltinWidgetTemplate("table-top", "table", "list", "أعلى العناصر أداءً",
            "جدول لأعلى 10 عناصر (منتجات/عملاء/فروع) مع قيمها ونموها.",
            "أضف جدولًا يعرض أعلى 10 عناصر (منتجات أو عملاء أو فروع حسب بياناتي) أداءً، مع قيمها ونسبة نموها."),
        new BuiltinWidgetTemplate("table-status", "table", "grid", "ملخص الحالة",
            "جدول يلخّص توزيع السجلات حسب حالتها، بالعدد والنسبة.",
            "أضف جدولًا يلخّص توزيع السجلات حسب حالتها (مثل: مكتمل، قيد التنفيذ، ملغى)، مع العدد والنسبة لكل حالة."),
    };

    public static readonly IReadOnlyList<BuiltinDashboardTemplate> Dashboards = new[]
    {
        new BuiltinDashboardTemplate("sales-exec", "a", "sales", "أداء المبيعات التنفيذي", true,
            "نظرة شاملة للإدارة العليا على المبيعات والمستهدفات والمناطق.",
            new[] { "ERP المبيعات", "المستهدفات" },
            new[] {
                new BuiltinDashboardWidgetSpec("kpi", "إجمالي المبيعات"), new BuiltinDashboardWidgetSpec("kpi", "عدد الطلبات"),
                new BuiltinDashboardWidgetSpec("kpi", "متوسط قيمة الطلب"), new BuiltinDashboardWidgetSpec("kpi", "نسبة تحقيق المستهدف"),
                new BuiltinDashboardWidgetSpec("line", "المبيعات الشهرية"), new BuiltinDashboardWidgetSpec("donut", "التوزيع حسب القطاع"),
                new BuiltinDashboardWidgetSpec("hbars", "المبيعات حسب المنطقة"), new BuiltinDashboardWidgetSpec("cbars", "مقارنة ربع سنوية"),
                new BuiltinDashboardWidgetSpec("table", "أعلى المنتجات"), new BuiltinDashboardWidgetSpec("funnel", "مراحل الصفقات"),
            },
            "ابنِ لي لوحة أداء مبيعات تنفيذية كاملة تتضمن: 1) إجمالي المبيعات كمؤشر رقمي مع نسبة النمو عن الفترة السابقة، 2) عدد الطلبات كمؤشر رقمي، 3) متوسط قيمة الطلب كمؤشر رقمي، 4) نسبة تحقيق المستهدف كمؤشر رقمي، 5) رسم خطي للمبيعات الشهرية خلال آخر 12 شهرًا، 6) رسم دائري لتوزيع المبيعات حسب القطاع، 7) رسم أعمدة أفقي للمبيعات حسب المنطقة، 8) رسم أعمدة يقارن المبيعات ربعيًا بين هذا العام والعام السابق، 9) جدول لأعلى 10 منتجات مبيعًا، 10) رسم قمع لمراحل الصفقات من عرض السعر حتى الإغلاق."),
        new BuiltinDashboardTemplate("pmo", "b", "projects", "متابعة المشاريع والمبادرات", true,
            "حالة المشاريع ونسب الإنجاز والميزانيات والمخاطر لمكتب إدارة المشاريع.",
            new[] { "نظام المشاريع", "الميزانية" },
            new[] {
                new BuiltinDashboardWidgetSpec("kpi", "المشاريع النشطة"), new BuiltinDashboardWidgetSpec("kpi", "متوسط الإنجاز"),
                new BuiltinDashboardWidgetSpec("kpi", "مشاريع متأخرة"), new BuiltinDashboardWidgetSpec("kpi", "الصرف من الميزانية"),
                new BuiltinDashboardWidgetSpec("line", "نسبة الإنجاز التراكمية"), new BuiltinDashboardWidgetSpec("donut", "المشاريع حسب الحالة"),
                new BuiltinDashboardWidgetSpec("hbars", "الإنجاز حسب القطاع"), new BuiltinDashboardWidgetSpec("cbars", "الميزانية: المخطط مقابل الفعلي"),
                new BuiltinDashboardWidgetSpec("table", "المشاريع الأعلى مخاطرة"), new BuiltinDashboardWidgetSpec("funnel", "مراحل المبادرات"),
            },
            "ابنِ لي لوحة متابعة مشاريع ومبادرات كاملة تتضمن: 1) عدد المشاريع النشطة كمؤشر رقمي، 2) متوسط نسبة الإنجاز كمؤشر رقمي، 3) عدد المشاريع المتأخرة كمؤشر رقمي، 4) نسبة الصرف من الميزانية كمؤشر رقمي، 5) رسم خطي لنسبة الإنجاز التراكمية شهريًا، 6) رسم دائري لتوزيع المشاريع حسب حالتها (في المسار/تحت المراقبة/متأخر)، 7) رسم أعمدة أفقي لمتوسط الإنجاز حسب القطاع أو الإدارة، 8) رسم أعمدة يقارن الميزانية المخططة بالفعلية لكل ربع، 9) جدول لأعلى المشاريع مخاطرة من حيث التأخير، 10) رسم قمع لمراحل المبادرات من الفكرة حتى الإغلاق."),
        new BuiltinDashboardTemplate("hr", "c", "hr", "الموارد البشرية والقوى العاملة", false,
            "التوظيف والدوران الوظيفي والحضور والتوطين في لوحة واحدة.",
            new[] { "نظام الموارد البشرية", "الحضور" },
            new[] {
                new BuiltinDashboardWidgetSpec("kpi", "إجمالي الموظفين"), new BuiltinDashboardWidgetSpec("kpi", "نسبة التوطين"),
                new BuiltinDashboardWidgetSpec("kpi", "معدل الدوران"), new BuiltinDashboardWidgetSpec("kpi", "نسبة الحضور"),
                new BuiltinDashboardWidgetSpec("line", "عدد الموظفين شهريًا"), new BuiltinDashboardWidgetSpec("donut", "التوزيع حسب الفئة"),
                new BuiltinDashboardWidgetSpec("hbars", "الموظفون حسب الإدارة"), new BuiltinDashboardWidgetSpec("cbars", "التعيينات مقابل الاستقالات"),
                new BuiltinDashboardWidgetSpec("table", "وظائف شاغرة طويلة"), new BuiltinDashboardWidgetSpec("heat", "الغياب حسب اليوم"),
            },
            "ابنِ لي لوحة موارد بشرية وقوى عاملة كاملة تتضمن: 1) إجمالي عدد الموظفين كمؤشر رقمي، 2) نسبة التوطين كمؤشر رقمي، 3) معدل الدوران الوظيفي كمؤشر رقمي، 4) نسبة الحضور كمؤشر رقمي، 5) رسم خطي لعدد الموظفين شهريًا (صافي بعد التعيين والاستقالات)، 6) رسم دائري لتوزيع الموظفين حسب الفئة الوظيفية، 7) رسم أعمدة أفقي لعدد الموظفين حسب الإدارة، 8) رسم أعمدة يقارن عدد التعيينات بالاستقالات لكل ربع، 9) جدول بالوظائف الشاغرة لأكثر من 60 يومًا، 10) خريطة حرارية للغياب حسب أيام الأسبوع."),
        new BuiltinDashboardTemplate("finance", "d", "finance", "المالية والميزانية", false,
            "الإيرادات والمصروفات والتدفق النقدي والانحراف عن الميزانية.",
            new[] { "النظام المالي", "الميزانية" },
            new[] {
                new BuiltinDashboardWidgetSpec("kpi", "الإيرادات"), new BuiltinDashboardWidgetSpec("kpi", "المصروفات"),
                new BuiltinDashboardWidgetSpec("kpi", "صافي الربح"), new BuiltinDashboardWidgetSpec("kpi", "الانحراف عن الميزانية"),
                new BuiltinDashboardWidgetSpec("line", "التدفق النقدي الشهري"), new BuiltinDashboardWidgetSpec("donut", "المصروفات حسب البند"),
                new BuiltinDashboardWidgetSpec("hbars", "الإيرادات حسب النشاط"), new BuiltinDashboardWidgetSpec("cbars", "الميزانية مقابل الفعلي"),
                new BuiltinDashboardWidgetSpec("table", "أكبر المستحقات"), new BuiltinDashboardWidgetSpec("funnel", "دورة التحصيل"),
            },
            "ابنِ لي لوحة مالية وميزانية كاملة تتضمن: 1) إجمالي الإيرادات كمؤشر رقمي مع نسبة النمو، 2) إجمالي المصروفات كمؤشر رقمي، 3) صافي الربح كمؤشر رقمي، 4) نسبة الانحراف عن الميزانية كمؤشر رقمي، 5) رسم خطي للتدفق النقدي الشهري، 6) رسم دائري لتوزيع المصروفات حسب البند، 7) رسم أعمدة أفقي للإيرادات حسب النشاط أو الخط التجاري، 8) رسم أعمدة يقارن الميزانية المخططة بالمصروفات الفعلية ربعيًا، 9) جدول لأكبر المستحقات المتأخرة السداد، 10) رسم قمع لدورة التحصيل من الفاتورة حتى التحصيل."),
        new BuiltinDashboardTemplate("procurement", "c", "procurement", "المشتريات والموردين", false,
            "أوامر الشراء وأداء الموردين ودورة الاعتماد والتوفير المحقق.",
            new[] { "نظام المشتريات" },
            new[] {
                new BuiltinDashboardWidgetSpec("kpi", "أوامر الشراء"), new BuiltinDashboardWidgetSpec("kpi", "قيمة المشتريات"),
                new BuiltinDashboardWidgetSpec("kpi", "متوسط مدة الاعتماد"), new BuiltinDashboardWidgetSpec("kpi", "التوفير المحقق"),
                new BuiltinDashboardWidgetSpec("line", "قيمة المشتريات الشهرية"), new BuiltinDashboardWidgetSpec("donut", "المشتريات حسب الفئة"),
                new BuiltinDashboardWidgetSpec("hbars", "أعلى الموردين"), new BuiltinDashboardWidgetSpec("cbars", "التسليم في الموعد"),
                new BuiltinDashboardWidgetSpec("table", "تقييم الموردين"), new BuiltinDashboardWidgetSpec("funnel", "دورة الشراء"),
            },
            "ابنِ لي لوحة مشتريات وموردين كاملة تتضمن: 1) عدد أوامر الشراء كمؤشر رقمي، 2) إجمالي قيمة المشتريات كمؤشر رقمي، 3) متوسط مدة دورة الاعتماد كمؤشر رقمي، 4) قيمة التوفير المحقق كمؤشر رقمي، 5) رسم خطي لقيمة المشتريات الشهرية، 6) رسم دائري لتوزيع المشتريات حسب الفئة، 7) رسم أعمدة أفقي لأعلى الموردين من حيث قيمة التعاملات، 8) رسم أعمدة يقارن عدد الأوامر المسلّمة في الموعد بالمتأخرة لكل ربع، 9) جدول لتقييم الموردين من حيث الجودة والالتزام، 10) رسم قمع لدورة الشراء من طلب الشراء حتى الاستلام."),
        new BuiltinDashboardTemplate("cx", "b", "cx", "تجربة العملاء والشكاوى", false,
            "رضا العملاء وزمن الاستجابة والشكاوى حسب القناة والسبب.",
            new[] { "نظام التذاكر", "الاستبيانات" },
            new[] {
                new BuiltinDashboardWidgetSpec("kpi", "رضا العملاء"), new BuiltinDashboardWidgetSpec("kpi", "التذاكر المفتوحة"),
                new BuiltinDashboardWidgetSpec("kpi", "زمن أول رد"), new BuiltinDashboardWidgetSpec("kpi", "مؤشر التوصية NPS"),
                new BuiltinDashboardWidgetSpec("line", "التذاكر الواردة"), new BuiltinDashboardWidgetSpec("donut", "التذاكر حسب القناة"),
                new BuiltinDashboardWidgetSpec("hbars", "أسباب الشكاوى"), new BuiltinDashboardWidgetSpec("cbars", "الالتزام بمستوى الخدمة"),
                new BuiltinDashboardWidgetSpec("table", "أداء الفرق"), new BuiltinDashboardWidgetSpec("heat", "أوقات الذروة"),
            },
            "ابنِ لي لوحة تجربة عملاء وشكاوى كاملة تتضمن: 1) نسبة رضا العملاء كمؤشر رقمي، 2) عدد التذاكر المفتوحة كمؤشر رقمي، 3) متوسط زمن أول رد كمؤشر رقمي، 4) مؤشر التوصية NPS كمؤشر رقمي، 5) رسم خطي لعدد التذاكر الواردة شهريًا، 6) رسم دائري لتوزيع التذاكر حسب القناة، 7) رسم أعمدة أفقي لأسباب الشكاوى الأكثر تكرارًا، 8) رسم أعمدة يقارن الالتزام المستهدف بمستوى الخدمة بالفعلي لكل ربع، 9) جدول بأداء فرق الدعم (تذاكر محلولة ونسبة الرضا)، 10) خريطة حرارية لأوقات ذروة التذاكر حسب اليوم والساعة."),
        new BuiltinDashboardTemplate("ops", "d", "ops", "العمليات والتشغيل", false,
            "الإنتاجية وجاهزية الأصول وأوامر العمل والصيانة.",
            new[] { "نظام التشغيل", "الصيانة" },
            new[] {
                new BuiltinDashboardWidgetSpec("kpi", "أوامر العمل المنجزة"), new BuiltinDashboardWidgetSpec("kpi", "جاهزية الأصول"),
                new BuiltinDashboardWidgetSpec("kpi", "أعطال حرجة"), new BuiltinDashboardWidgetSpec("kpi", "تكلفة الصيانة"),
                new BuiltinDashboardWidgetSpec("line", "الإنتاجية اليومية"), new BuiltinDashboardWidgetSpec("donut", "أوامر العمل حسب النوع"),
                new BuiltinDashboardWidgetSpec("hbars", "الأعطال حسب الموقع"), new BuiltinDashboardWidgetSpec("cbars", "المخطط مقابل المنفذ"),
                new BuiltinDashboardWidgetSpec("table", "الأصول الأكثر توقفًا"), new BuiltinDashboardWidgetSpec("heat", "الأعطال حسب الوردية"),
            },
            "ابنِ لي لوحة عمليات وتشغيل كاملة تتضمن: 1) عدد أوامر العمل المنجزة كمؤشر رقمي، 2) نسبة جاهزية الأصول كمؤشر رقمي، 3) عدد الأعطال الحرجة كمؤشر رقمي، 4) تكلفة الصيانة كمؤشر رقمي، 5) رسم خطي للإنتاجية اليومية، 6) رسم دائري لتوزيع أوامر العمل حسب النوع (وقائية/تصحيحية/طارئة)، 7) رسم أعمدة أفقي للأعطال حسب الموقع، 8) رسم أعمدة يقارن أوامر الصيانة الوقائية المخططة بالمنفذة لكل ربع، 9) جدول بالأصول الأكثر توقفًا من حيث ساعات التوقف، 10) خريطة حرارية للأعطال حسب الوردية."),
        new BuiltinDashboardTemplate("marketing", "b", "sales", "التسويق والحملات", false,
            "أداء الحملات والعملاء المحتملين وتكلفة الاستحواذ والعائد.",
            new[] { "CRM", "منصات الإعلان" },
            new[] {
                new BuiltinDashboardWidgetSpec("kpi", "العملاء المحتملون"), new BuiltinDashboardWidgetSpec("kpi", "معدل التحويل"),
                new BuiltinDashboardWidgetSpec("kpi", "تكلفة الاستحواذ"), new BuiltinDashboardWidgetSpec("kpi", "العائد على الإنفاق"),
                new BuiltinDashboardWidgetSpec("line", "العملاء المحتملون شهريًا"), new BuiltinDashboardWidgetSpec("donut", "المصادر"),
                new BuiltinDashboardWidgetSpec("hbars", "أفضل الحملات"), new BuiltinDashboardWidgetSpec("cbars", "الإنفاق مقابل الإيراد"),
                new BuiltinDashboardWidgetSpec("table", "أداء القنوات"), new BuiltinDashboardWidgetSpec("funnel", "رحلة العميل"),
            },
            "ابنِ لي لوحة تسويق وحملات كاملة تتضمن: 1) عدد العملاء المحتملين كمؤشر رقمي، 2) معدل التحويل كمؤشر رقمي، 3) تكلفة اكتساب العميل كمؤشر رقمي، 4) العائد على الإنفاق التسويقي كمؤشر رقمي، 5) رسم خطي لعدد العملاء المحتملين شهريًا، 6) رسم دائري لتوزيع العملاء المحتملين حسب المصدر، 7) رسم أعمدة أفقي لأفضل الحملات من حيث العائد، 8) رسم أعمدة يقارن الإنفاق التسويقي بالإيراد الناتج لكل ربع، 9) جدول بأداء القنوات التسويقية (عدد العملاء ونسبة التحويل)، 10) رسم قمع لرحلة العميل من الزيارة حتى الشراء."),
    }.Concat(BuiltinDashboardsGenerated.Items).ToList();

    /// <summary>Index = measure-type (0 pct, 1 time, 2 money, 3 score, 4 count, 5 other), matching
    /// kpi-library.js's own KPI_MT_KEY/mtypes ordering. {{kpi_name}} is the only placeholder —
    /// substituted server-side from the (non-sensitive, display-only) name the client names the
    /// row by, never trusted as instruction text itself.</summary>
    public static readonly IReadOnlyList<string> KpiMeasureTypePrompts = new[]
    {
        "أضف مؤشرًا بعنوان \"{{kpi_name}}\" كنسبة مئوية من بياناتي، مع شريط تقدّم يوضح مدى تحقيق المستهدف إن وُجد مستهدف في المصدر.",
        "أضف مؤشرًا بعنوان \"{{kpi_name}}\" بوحدة زمنية (أيام أو ساعات حسب بياناتي)، مع اتجاهه مقارنة بالفترة السابقة — علمًا أن القيمة الأقل تُعتبر أفضل هنا.",
        "أضف مؤشرًا ماليًا بعنوان \"{{kpi_name}}\"، مع نسبة التغيّر مقارنة بالفترة السابقة.",
        "أضف مؤشرًا بعنوان \"{{kpi_name}}\" كدرجة تقييم من 5، بناءً على بياناتي.",
        "أضف مؤشرًا بعنوان \"{{kpi_name}}\" يوضح العدد موزّعًا على آخر 6 أشهر كرسم أعمدة.",
        "أضف مؤشرًا يوضح \"{{kpi_name}}\" من بياناتي.",
    };

    public static BuiltinWidgetTemplate? FindWidget(string id) => Widgets.FirstOrDefault(w => w.Id == id);
    public static BuiltinDashboardTemplate? FindDashboard(string id) => Dashboards.FirstOrDefault(d => d.Id == id);
}
