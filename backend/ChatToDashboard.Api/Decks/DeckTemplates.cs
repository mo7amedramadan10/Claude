namespace ChatToDashboard.Api.Decks;

/// <summary>شريحة في نموذج عرض: تخطيط وعنوان ونية البيانات (ما يُستعلم عنه) إن كانت تحتاج بيانات.</summary>
public sealed record DeckTemplateSlide(string Layout, string Title, string? Intent = null, bool NeedsData = false, string? Hint = null);

/// <summary>نموذج عرض جاهز: هيكل شرائح يُملأ من بيانات المشروع بالموديل (الشرائح ذات NeedsData) أو يدويًا.</summary>
public sealed record DeckTemplate(string Key, string Name, string Category, string Description, string Icon, IReadOnlyList<DeckTemplateSlide> Slides)
{
    public int DataSlides => Slides.Count(s => s.NeedsData);
}

/// <summary>كتالوج نماذج العروض المدمجة (قابلة للتوسعة؛ لا تعتمد على أسماء أعمدة معروفة — المطابقة عند البناء).</summary>
public static class DeckTemplates
{
    /// <summary>مفاتيح التصنيف الثابتة (الاسم والأيقونة في الواجهة `DK_CATS`): mgmt, sales, projects, finance, hr, marketing, ops, cx, general.</summary>
    public static readonly IReadOnlyList<DeckTemplate> All = new[]
    {
        // ---- الإدارة ----
        new DeckTemplate("business-review", "مراجعة أعمال دورية", "mgmt", "ملخص تنفيذي بالمؤشرات الرئيسية والاتجاه والتوزيع وأهم العملاء والتوصيات.", "📈", new[]
        {
            new DeckTemplateSlide("title", "مراجعة الأعمال"),
            new DeckTemplateSlide("kpis", "المؤشرات الرئيسية", "أهم 4 مؤشرات: إجمالي الإيراد، عدد الصفقات/الطلبات، متوسط قيمة الصفقة، ومعدل النمو مقارنةً بالفترة السابقة", true),
            new DeckTemplateSlide("chart", "الاتجاه الشهري", "الإيراد أو المبيعات مجمّعة بالشهر عبر الفترة المتاحة", true, "line"),
            new DeckTemplateSlide("chart", "التوزيع حسب المنطقة أو الفئة", "الإيراد حسب المنطقة أو الفئة أو المنتج (أكثر 8)", true, "bar"),
            new DeckTemplateSlide("table", "أهم 10 عملاء", "أعلى 10 عملاء بالإيراد مع عدد الصفقات", true),
            new DeckTemplateSlide("bullets", "أبرز الملاحظات", "3–5 ملاحظات مستخلصة من الأرقام أعلاه", true),
            new DeckTemplateSlide("bullets", "التوصيات والخطوات التالية"),
        }),
        new DeckTemplate("quarterly-results", "نتائج الربع", "mgmt", "نتائج ربع سنوية: الإيراد مقابل المستهدف، النمو، التوزيع، وأهم الإنجازات والتحديات.", "🗓️", new[]
        {
            new DeckTemplateSlide("title", "نتائج الربع"),
            new DeckTemplateSlide("kpis", "الأرقام الرئيسية للربع", "إيراد الربع، المستهدف، نسبة التحقق، والنمو عن الربع السابق", true),
            new DeckTemplateSlide("chart", "الإيراد الربعي مقارنةً بالأرباع السابقة", "الإيراد مجمّعًا بالربع لآخر 6 أرباع", true, "bar"),
            new DeckTemplateSlide("chart", "التوزيع حسب خط الأعمال", "الإيراد حسب المنتج أو الفئة في الربع", true, "pie"),
            new DeckTemplateSlide("two-column", "الإنجازات والتحديات", null, false, "الإنجازات|التحديات"),
            new DeckTemplateSlide("bullets", "أولويات الربع القادم"),
        }),
        new DeckTemplate("board-update", "تحديث مجلس الإدارة", "mgmt", "تحديث موجز للمجلس: الوضع العام، المؤشرات، المخاطر، والقرارات المطلوبة.", "🏛️", new[]
        {
            new DeckTemplateSlide("title", "تحديث مجلس الإدارة"),
            new DeckTemplateSlide("bullets", "الملخص التنفيذي"),
            new DeckTemplateSlide("kpis", "مؤشرات الأداء", "أهم 4 مؤشرات أداء للفترة مع التغير عن الفترة السابقة", true),
            new DeckTemplateSlide("chart", "الاتجاه العام", "المقياس الرئيسي (الإيراد أو العمليات) مجمّعًا بالشهر", true, "line"),
            new DeckTemplateSlide("table", "سجل المخاطر الرئيسية"),
            new DeckTemplateSlide("bullets", "القرارات المطلوبة من المجلس"),
        }),
        new DeckTemplate("okr-review", "مراجعة الأهداف (OKR)", "mgmt", "الأهداف والنتائج الرئيسية بنسب الإنجاز والتعثّر.", "🧭", new[]
        {
            new DeckTemplateSlide("title", "مراجعة الأهداف"),
            new DeckTemplateSlide("kpis", "الإنجاز العام", "نسبة الإنجاز الإجمالية وعدد النتائج على المسار/المتعثرة", true),
            new DeckTemplateSlide("table", "النتائج الرئيسية", "كل نتيجة رئيسية مع المستهدف والفعلي ونسبة الإنجاز", true),
            new DeckTemplateSlide("chart", "الإنجاز حسب الهدف", "نسبة الإنجاز لكل هدف", true, "bar"),
            new DeckTemplateSlide("bullets", "النتائج المتعثرة وأسبابها", "النتائج الأقل إنجازًا", true),
            new DeckTemplateSlide("tasks", "إجراءات التصحيح", null, false, "قائمة مهام قابلة للتعديل"),
        }),
        new DeckTemplate("weekly-report", "التقرير الأسبوعي", "mgmt", "تقرير أسبوعي قصير: الأرقام، ما أُنجز، ما تعثّر، وخطة الأسبوع القادم.", "📅", new[]
        {
            new DeckTemplateSlide("title", "التقرير الأسبوعي"),
            new DeckTemplateSlide("kpis", "أرقام الأسبوع", "أهم 3–4 مقاييس لهذا الأسبوع مقارنةً بالأسبوع السابق", true),
            new DeckTemplateSlide("chart", "الأداء اليومي", "المقياس الرئيسي مجمّعًا باليوم لآخر 14 يومًا", true, "bar"),
            new DeckTemplateSlide("two-column", "ما أُنجز / ما تعثّر", null, false, "ما أُنجز|ما تعثّر"),
            new DeckTemplateSlide("tasks", "خطة الأسبوع القادم", null, false, "قائمة مهام قابلة للتعديل"),
        }),
        // ---- المبيعات والعملاء ----
        new DeckTemplate("sales-pipeline", "مسار المبيعات", "sales", "الفرص حسب المرحلة والقيمة المتوقعة وأداء مندوبي المبيعات.", "🎯", new[]
        {
            new DeckTemplateSlide("title", "مسار المبيعات"),
            new DeckTemplateSlide("kpis", "ملخص المسار", "عدد الفرص المفتوحة، القيمة الإجمالية، القيمة المرجّحة، ومعدل الفوز", true),
            new DeckTemplateSlide("chart", "الفرص حسب المرحلة", "عدد الفرص وقيمتها في كل مرحلة", true, "bar"),
            new DeckTemplateSlide("chart", "أداء المندوبين", "القيمة المغلقة حسب المسؤول (أكثر 8)", true, "bar"),
            new DeckTemplateSlide("table", "أكبر الفرص المفتوحة", "أعلى 10 فرص مفتوحة بالقيمة والمرحلة والمسؤول", true),
            new DeckTemplateSlide("bullets", "ما يحتاج قرارًا"),
        }),
        new DeckTemplate("sales-performance", "أداء المبيعات", "sales", "المبيعات حسب الشهر والمنتج والمنطقة وأفضل المندوبين.", "💼", new[]
        {
            new DeckTemplateSlide("title", "أداء المبيعات"),
            new DeckTemplateSlide("kpis", "ملخص الأداء", "إجمالي المبيعات، عدد الطلبات، متوسط قيمة الطلب، والنمو", true),
            new DeckTemplateSlide("chart", "المبيعات الشهرية", "المبيعات مجمّعة بالشهر", true, "line"),
            new DeckTemplateSlide("chart", "المبيعات حسب المنتج", "أعلى 8 منتجات بالمبيعات", true, "bar"),
            new DeckTemplateSlide("chart", "المبيعات حسب المنطقة", "المبيعات حسب المنطقة", true, "pie"),
            new DeckTemplateSlide("table", "أفضل المندوبين", "أعلى 10 مندوبين بالمبيعات وعدد الطلبات", true),
            new DeckTemplateSlide("bullets", "الخلاصة والتوصيات", "3–5 ملاحظات من الأرقام أعلاه", true),
        }),
        new DeckTemplate("customer-retention", "الاحتفاظ بالعملاء", "sales", "العملاء النشطون والمتسرّبون ومعدل الاحتفاظ وأهم الحسابات المعرّضة للخطر.", "🤝", new[]
        {
            new DeckTemplateSlide("title", "الاحتفاظ بالعملاء"),
            new DeckTemplateSlide("kpis", "مؤشرات الاحتفاظ", "عدد العملاء النشطين، العملاء الجدد، المتسرّبون، ومعدل الاحتفاظ", true),
            new DeckTemplateSlide("chart", "العملاء النشطون شهريًا", "عدد العملاء النشطين مجمّعًا بالشهر", true, "line"),
            new DeckTemplateSlide("chart", "التسرّب حسب الشريحة", "عدد العملاء المتسرّبين حسب الشريحة أو الفئة", true, "bar"),
            new DeckTemplateSlide("table", "حسابات معرّضة للخطر", "العملاء الذين انخفض نشاطهم أو إنفاقهم أكثر", true),
            new DeckTemplateSlide("tasks", "خطة الاحتفاظ", null, false, "قائمة مهام قابلة للتعديل"),
        }),
        // ---- المشاريع ----
        new DeckTemplate("project-status", "حالة مشروع", "projects", "موقف المشروع: الإنجاز، المهام حسب الحالة، المخاطر، والخطوات القادمة.", "🗂️", new[]
        {
            new DeckTemplateSlide("title", "حالة المشروع"),
            new DeckTemplateSlide("kpis", "نظرة سريعة", "عدد المهام الكلي، المنجز، المتأخر، ونسبة الإنجاز", true),
            new DeckTemplateSlide("chart", "المهام حسب الحالة", "توزيع المهام حسب الحالة", true, "pie"),
            new DeckTemplateSlide("table", "المهام المتأخرة", "المهام التي تجاوزت موعدها مع المسؤول والموعد", true),
            new DeckTemplateSlide("bullets", "المخاطر والعوائق"),
            new DeckTemplateSlide("tasks", "الخطوات القادمة", null, false, "قائمة مهام قابلة للتعديل من الجمهور بعد النشر"),
        }),
        new DeckTemplate("kickoff", "انطلاق مشروع", "projects", "الأهداف والنطاق والفريق والجدول الزمني — يدوي بالكامل بلا بيانات.", "🚀", new[]
        {
            new DeckTemplateSlide("title", "انطلاق المشروع"),
            new DeckTemplateSlide("bullets", "الأهداف"),
            new DeckTemplateSlide("two-column", "النطاق: داخل / خارج", null, false, "داخل النطاق|خارج النطاق"),
            new DeckTemplateSlide("bullets", "الفريق والأدوار"),
            new DeckTemplateSlide("bullets", "الجدول الزمني والمعالم"),
            new DeckTemplateSlide("bullets", "المخاطر المبكرة"),
            new DeckTemplateSlide("tasks", "المهام الأولى", null, false, "قائمة مهام قابلة للتعديل"),
        }),
        new DeckTemplate("portfolio-review", "مراجعة محفظة المشاريع", "projects", "كل المشاريع في نظرة واحدة: الحالة، الإنجاز، الميزانية المصروفة، والمتعثر منها.", "📚", new[]
        {
            new DeckTemplateSlide("title", "مراجعة محفظة المشاريع"),
            new DeckTemplateSlide("kpis", "المحفظة بالأرقام", "عدد المشاريع الجارية، المنجزة، المتأخرة، ونسبة الميزانية المصروفة", true),
            new DeckTemplateSlide("chart", "المشاريع حسب الحالة", "عدد المشاريع في كل حالة", true, "pie"),
            new DeckTemplateSlide("table", "جدول المشاريع", "كل مشروع مع المدير والحالة ونسبة الإنجاز والميزانية", true),
            new DeckTemplateSlide("bullets", "المشاريع المتعثرة وأسبابها", "المشاريع الأقل إنجازًا أو المتأخرة", true),
            new DeckTemplateSlide("bullets", "القرارات المطلوبة"),
        }),
        new DeckTemplate("project-closure", "إغلاق مشروع", "projects", "خلاصة مشروع منتهٍ: ما خُطّط وما تحقق، الدروس المستفادة، والتسليمات.", "🏁", new[]
        {
            new DeckTemplateSlide("title", "إغلاق المشروع"),
            new DeckTemplateSlide("kpis", "المخطَّط مقابل الفعلي", "المدة المخططة والفعلية، الميزانية والمصروف، وعدد التسليمات", true),
            new DeckTemplateSlide("bullets", "التسليمات المنجزة"),
            new DeckTemplateSlide("two-column", "ما نجح / ما يحتاج تحسينًا", null, false, "ما نجح|ما يحتاج تحسينًا"),
            new DeckTemplateSlide("bullets", "الدروس المستفادة"),
            new DeckTemplateSlide("quote", "كلمة شكر"),
        }),
        // ---- المالية ----
        new DeckTemplate("finance-monthly", "التقرير المالي الشهري", "finance", "الإيرادات والمصروفات والربح الشهري وتوزيع المصروفات.", "💰", new[]
        {
            new DeckTemplateSlide("title", "التقرير المالي الشهري"),
            new DeckTemplateSlide("kpis", "الملخص المالي", "إجمالي الإيرادات، إجمالي المصروفات، صافي الربح، وهامش الربح", true),
            new DeckTemplateSlide("chart", "الإيرادات والمصروفات شهريًا", "الإيرادات مقابل المصروفات مجمّعة بالشهر", true, "bar"),
            new DeckTemplateSlide("chart", "توزيع المصروفات", "المصروفات حسب الفئة أو مركز التكلفة", true, "pie"),
            new DeckTemplateSlide("table", "أكبر بنود المصروفات", "أعلى 10 بنود مصروفات بالقيمة والفئة", true),
            new DeckTemplateSlide("bullets", "ملاحظات وتوصيات", "3–5 ملاحظات من الأرقام أعلاه", true),
        }),
        new DeckTemplate("budget-vs-actual", "الميزانية مقابل الفعلي", "finance", "الانحراف بين الميزانية والمصروف الفعلي حسب الإدارة أو البند.", "⚖️", new[]
        {
            new DeckTemplateSlide("title", "الميزانية مقابل الفعلي"),
            new DeckTemplateSlide("kpis", "الالتزام بالميزانية", "إجمالي الميزانية، المصروف الفعلي، نسبة الاستهلاك، والانحراف", true),
            new DeckTemplateSlide("chart", "الميزانية والفعلي حسب الإدارة", "الميزانية مقابل الفعلي لكل إدارة أو مركز تكلفة", true, "bar"),
            new DeckTemplateSlide("table", "أكبر الانحرافات", "البنود الأعلى انحرافًا عن الميزانية بالقيمة والنسبة", true),
            new DeckTemplateSlide("bullets", "أسباب الانحراف", "تفسير أكبر الانحرافات", true),
            new DeckTemplateSlide("tasks", "إجراءات الضبط", null, false, "قائمة مهام قابلة للتعديل"),
        }),
        // ---- الموارد البشرية ----
        new DeckTemplate("hr-headcount", "تقرير القوى العاملة", "hr", "عدد الموظفين وتوزيعهم حسب الإدارة والدوران والتوظيف.", "👥", new[]
        {
            new DeckTemplateSlide("title", "تقرير القوى العاملة"),
            new DeckTemplateSlide("kpis", "القوى العاملة بالأرقام", "إجمالي الموظفين، التعيينات الجديدة، المغادرون، ومعدل الدوران", true),
            new DeckTemplateSlide("chart", "الموظفون حسب الإدارة", "عدد الموظفين في كل إدارة", true, "bar"),
            new DeckTemplateSlide("chart", "اتجاه عدد الموظفين", "عدد الموظفين مجمّعًا بالشهر", true, "line"),
            new DeckTemplateSlide("table", "الإدارات الأعلى دورانًا", "الإدارات مرتبة بمعدل الدوران مع عدد المغادرين", true),
            new DeckTemplateSlide("bullets", "ملاحظات وتوصيات"),
        }),
        new DeckTemplate("recruitment", "تقرير التوظيف", "hr", "الوظائف الشاغرة والمتقدمون ومراحل التوظيف ومدة الشغل.", "🧑‍💼", new[]
        {
            new DeckTemplateSlide("title", "تقرير التوظيف"),
            new DeckTemplateSlide("kpis", "ملخص التوظيف", "الوظائف الشاغرة، عدد المتقدمين، التعيينات، ومتوسط مدة الشغل بالأيام", true),
            new DeckTemplateSlide("chart", "المتقدمون حسب المرحلة", "عدد المتقدمين في كل مرحلة من مراحل التوظيف", true, "bar"),
            new DeckTemplateSlide("chart", "مصادر المتقدمين", "عدد المتقدمين حسب المصدر", true, "pie"),
            new DeckTemplateSlide("table", "الوظائف المفتوحة", "الوظائف الشاغرة مع الإدارة وعدد المتقدمين وأيام الفتح", true),
            new DeckTemplateSlide("tasks", "الخطوات التالية", null, false, "قائمة مهام قابلة للتعديل"),
        }),
        // ---- التسويق ----
        new DeckTemplate("marketing-campaign", "أداء الحملات التسويقية", "marketing", "الإنفاق والوصول والنقرات والتحويلات لكل حملة وقناة.", "📣", new[]
        {
            new DeckTemplateSlide("title", "أداء الحملات التسويقية"),
            new DeckTemplateSlide("kpis", "ملخص الأداء", "إجمالي الإنفاق، الانطباعات، النقرات، والتحويلات", true),
            new DeckTemplateSlide("chart", "التحويلات حسب القناة", "عدد التحويلات لكل قناة", true, "bar"),
            new DeckTemplateSlide("chart", "الإنفاق أسبوعيًا", "الإنفاق مجمّعًا بالأسبوع", true, "line"),
            new DeckTemplateSlide("table", "أفضل الحملات", "أعلى 10 حملات بالتحويلات مع الإنفاق وتكلفة التحويل", true),
            new DeckTemplateSlide("bullets", "ما نوقفه وما نوسّعه", "توصيات من الأرقام أعلاه", true),
        }),
        // ---- العمليات والإمداد ----
        new DeckTemplate("inventory-review", "مراجعة المخزون", "ops", "قيمة المخزون، الأصناف الراكدة، نواقص إعادة الطلب، وحركة المخزون.", "📦", new[]
        {
            new DeckTemplateSlide("title", "مراجعة المخزون"),
            new DeckTemplateSlide("kpis", "المخزون بالأرقام", "قيمة المخزون، عدد الأصناف، أصناف تحت حد إعادة الطلب، والأصناف الراكدة", true),
            new DeckTemplateSlide("chart", "قيمة المخزون حسب الفئة", "قيمة المخزون لكل فئة", true, "bar"),
            new DeckTemplateSlide("table", "أصناف تحتاج إعادة طلب", "الأصناف التي كميتها تحت حد إعادة الطلب مع الكمية والحد", true),
            new DeckTemplateSlide("table", "الأصناف الراكدة", "الأصناف بلا حركة لأطول فترة مع قيمتها", true),
            new DeckTemplateSlide("bullets", "التوصيات"),
        }),
        new DeckTemplate("supplier-review", "مراجعة الموردين", "ops", "الإنفاق حسب المورد، الالتزام بالتسليم، والجودة.", "🚚", new[]
        {
            new DeckTemplateSlide("title", "مراجعة الموردين"),
            new DeckTemplateSlide("kpis", "ملخص المشتريات", "إجمالي الإنفاق، عدد أوامر الشراء، عدد الموردين النشطين، ونسبة التسليم في الموعد", true),
            new DeckTemplateSlide("chart", "الإنفاق حسب المورد", "أعلى 8 موردين بالإنفاق", true, "bar"),
            new DeckTemplateSlide("chart", "التسليم في الموعد شهريًا", "نسبة أوامر الشراء المسلَّمة في موعدها مجمّعة بالشهر", true, "line"),
            new DeckTemplateSlide("table", "الموردون الأقل التزامًا", "الموردون مرتبين بنسبة التأخير مع عدد الأوامر", true),
            new DeckTemplateSlide("bullets", "القرارات التعاقدية"),
        }),
        // ---- خدمة العملاء ----
        new DeckTemplate("support-review", "مراجعة خدمة العملاء", "cx", "التذاكر والحلّ ووقت الاستجابة ورضا العملاء.", "🎧", new[]
        {
            new DeckTemplateSlide("title", "مراجعة خدمة العملاء"),
            new DeckTemplateSlide("kpis", "مؤشرات الخدمة", "عدد التذاكر، نسبة الحل من أول تواصل، متوسط وقت الاستجابة، ورضا العملاء", true),
            new DeckTemplateSlide("chart", "التذاكر أسبوعيًا", "عدد التذاكر الواردة مجمّعًا بالأسبوع", true, "line"),
            new DeckTemplateSlide("chart", "التذاكر حسب الفئة", "عدد التذاكر حسب فئة المشكلة", true, "pie"),
            new DeckTemplateSlide("table", "أكثر المشكلات تكرارًا", "أعلى 10 فئات مشكلات بعدد التذاكر ومتوسط وقت الحل", true),
            new DeckTemplateSlide("tasks", "خطة التحسين", null, false, "قائمة مهام قابلة للتعديل"),
        }),
        // ---- عام ----
        new DeckTemplate("blank", "ابدأ من فراغ", "general", "شريحة عنوان واحدة؛ أضف الشرائح يدويًا أو بالمحادثة.", "➕", new[] { new DeckTemplateSlide("title", "عرض جديد") }),
    };

    public static DeckTemplate? Find(string? key) => All.FirstOrDefault(t => string.Equals(t.Key, key, StringComparison.OrdinalIgnoreCase));

    /// <summary>مواصفة أولية من النموذج: الشرائح بعناوينها، وذات البيانات مُعلَّمة needsData لتعبئتها بالموديل.</summary>
    public static DeckSpec ToSpec(DeckTemplate t, string? title = null)
    {
        var spec = new DeckSpec { Title = string.IsNullOrWhiteSpace(title) ? t.Name : title!.Trim() };
        foreach (var s in t.Slides)
        {
            var slide = new DeckSlide { Id = DeckSpec.NewId(), Layout = s.Layout, Title = s.Layout == "title" ? spec.Title : s.Title, NeedsData = s.NeedsData ? true : null, Intent = s.Intent };
            if (s.Layout == "chart") slide.Chart = new DeckChart { Type = s.Hint ?? "bar" };
            if (s.Layout == "tasks") slide.Editable = new DeckEditable { Key = "tasks-" + slide.Id, Kind = "tasks" };
            if (s.Layout == "two-column") slide.Columns = (s.Hint ?? "العمود الأول|العمود الثاني").Split('|').Select(c => new DeckColumn { Title = c.Trim(), Bullets = new() { "…" } }).ToList();
            if (s.Layout == "bullets" && !s.NeedsData) slide.Bullets = new() { "…" };
            if (s.Layout == "quote") slide.Quote = "…";
            if (s.Layout == "table" && !s.NeedsData) slide.Table = new DeckTable { Columns = new() { "البند", "التفاصيل" }, Rows = new() { new() { "…", "…" } } };
            spec.Slides.Add(slide);
        }
        return spec.Normalize();
    }
}
