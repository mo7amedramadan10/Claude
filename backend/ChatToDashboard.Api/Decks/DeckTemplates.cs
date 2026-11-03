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
    public static readonly IReadOnlyList<DeckTemplate> All = new[]
    {
        new DeckTemplate("business-review", "مراجعة أعمال دورية", "الإدارة", "ملخص تنفيذي بالمؤشرات الرئيسية والاتجاه والتوزيع وأهم العملاء والتوصيات.", "📈", new[]
        {
            new DeckTemplateSlide("title", "مراجعة الأعمال"),
            new DeckTemplateSlide("kpis", "المؤشرات الرئيسية", "أهم 4 مؤشرات: إجمالي الإيراد، عدد الصفقات/الطلبات، متوسط قيمة الصفقة، ومعدل النمو مقارنةً بالفترة السابقة", true),
            new DeckTemplateSlide("chart", "الاتجاه الشهري", "الإيراد أو المبيعات مجمّعة بالشهر عبر الفترة المتاحة", true, "line"),
            new DeckTemplateSlide("chart", "التوزيع حسب المنطقة أو الفئة", "الإيراد حسب المنطقة أو الفئة أو المنتج (أكثر 8)", true, "bar"),
            new DeckTemplateSlide("table", "أهم 10 عملاء", "أعلى 10 عملاء بالإيراد مع عدد الصفقات", true),
            new DeckTemplateSlide("bullets", "أبرز الملاحظات", "3–5 ملاحظات مستخلصة من الأرقام أعلاه", true),
            new DeckTemplateSlide("bullets", "التوصيات والخطوات التالية"),
        }),
        new DeckTemplate("project-status", "حالة مشروع", "المشاريع", "موقف المشروع: الإنجاز، المهام حسب الحالة، المخاطر، والخطوات القادمة.", "🗂️", new[]
        {
            new DeckTemplateSlide("title", "حالة المشروع"),
            new DeckTemplateSlide("kpis", "نظرة سريعة", "عدد المهام الكلي، المنجز، المتأخر، ونسبة الإنجاز", true),
            new DeckTemplateSlide("chart", "المهام حسب الحالة", "توزيع المهام حسب الحالة", true, "pie"),
            new DeckTemplateSlide("table", "المهام المتأخرة", "المهام التي تجاوزت موعدها مع المسؤول والموعد", true),
            new DeckTemplateSlide("bullets", "المخاطر والعوائق"),
            new DeckTemplateSlide("tasks", "الخطوات القادمة", null, false, "قائمة مهام قابلة للتعديل من الجمهور بعد النشر"),
        }),
        new DeckTemplate("sales-pipeline", "مسار المبيعات", "المبيعات", "الفرص حسب المرحلة والقيمة المتوقعة وأداء مندوبي المبيعات.", "🎯", new[]
        {
            new DeckTemplateSlide("title", "مسار المبيعات"),
            new DeckTemplateSlide("kpis", "ملخص المسار", "عدد الفرص المفتوحة، القيمة الإجمالية، القيمة المرجّحة، ومعدل الفوز", true),
            new DeckTemplateSlide("chart", "الفرص حسب المرحلة", "عدد الفرص وقيمتها في كل مرحلة", true, "bar"),
            new DeckTemplateSlide("chart", "أداء المندوبين", "القيمة المغلقة حسب المسؤول (أكثر 8)", true, "bar"),
            new DeckTemplateSlide("table", "أكبر الفرص المفتوحة", "أعلى 10 فرص مفتوحة بالقيمة والمرحلة والمسؤول", true),
            new DeckTemplateSlide("bullets", "ما يحتاج قرارًا"),
        }),
        new DeckTemplate("okr-review", "مراجعة الأهداف (OKR)", "الإدارة", "الأهداف والنتائج الرئيسية بنسب الإنجاز والتعثّر.", "🧭", new[]
        {
            new DeckTemplateSlide("title", "مراجعة الأهداف"),
            new DeckTemplateSlide("kpis", "الإنجاز العام", "نسبة الإنجاز الإجمالية وعدد النتائج على المسار/المتعثرة", true),
            new DeckTemplateSlide("table", "النتائج الرئيسية", "كل نتيجة رئيسية مع المستهدف والفعلي ونسبة الإنجاز", true),
            new DeckTemplateSlide("chart", "الإنجاز حسب الهدف", "نسبة الإنجاز لكل هدف", true, "bar"),
            new DeckTemplateSlide("bullets", "النتائج المتعثرة وأسبابها", "النتائج الأقل إنجازًا", true),
            new DeckTemplateSlide("tasks", "إجراءات التصحيح", null, false, "قائمة مهام قابلة للتعديل"),
        }),
        new DeckTemplate("kickoff", "انطلاق مشروع", "المشاريع", "الأهداف والنطاق والفريق والجدول الزمني — يدوي بالكامل بلا بيانات.", "🚀", new[]
        {
            new DeckTemplateSlide("title", "انطلاق المشروع"),
            new DeckTemplateSlide("bullets", "الأهداف"),
            new DeckTemplateSlide("two-column", "النطاق: داخل / خارج"),
            new DeckTemplateSlide("bullets", "الفريق والأدوار"),
            new DeckTemplateSlide("bullets", "الجدول الزمني والمعالم"),
            new DeckTemplateSlide("bullets", "المخاطر المبكرة"),
            new DeckTemplateSlide("tasks", "المهام الأولى", null, false, "قائمة مهام قابلة للتعديل"),
        }),
        new DeckTemplate("blank", "ابدأ من فراغ", "عام", "شريحة عنوان واحدة؛ أضف الشرائح يدويًا أو بالمحادثة.", "➕", new[] { new DeckTemplateSlide("title", "عرض جديد") }),
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
            if (s.Layout == "two-column") slide.Columns = new() { new DeckColumn { Title = "داخل النطاق", Bullets = new() { "…" } }, new DeckColumn { Title = "خارج النطاق", Bullets = new() { "…" } } };
            if (s.Layout == "bullets" && !s.NeedsData) slide.Bullets = new() { "…" };
            spec.Slides.Add(slide);
        }
        return spec.Normalize();
    }
}
