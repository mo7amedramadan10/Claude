namespace ChatToDashboard.Api.Templates;

/// <summary>
/// §4.24 — نموذج «متتبع الفرص والعروض» (<c>pl-opportunities-tracker</c>، فئة المبيعات، 3
/// صفحات). مكتوب يدويًا هنا بدل إضافته إلى <see cref="BuiltinDashboardsGenerated"/> لأن ذاك
/// ملف مولَّد آليًا من dash-library.js. العناوين والصفحات مطابقة حرفيًا لمدخل المكتبة نفسه
/// (حتى تتطابق معاينة المعرض مع اللوحة المبنية)، والأنواع المقترحة من أنواع عناصر التطبيق
/// الحقيقية. لا جداول أو استعلامات مقترحة: لا يوجد جدول فرص في البيانات المرحلية، والنموذج
/// يطابق جداول المشروع الفعلية بنفسه — ولا يُلفَّق المستهدف ولا سقف الاتفاقيات من بيانات أخرى.
/// </summary>
public static class BuiltinDashboardsOpportunities
{
    private const string Summary = "الملخص";
    private const string Stages = "مراحل الفرص";
    private const string Details = "التفاصيل";

    public static readonly IReadOnlyList<BuiltinDashboardTemplate> Items = new[]
    {
        new BuiltinDashboardTemplate("pl-opportunities-tracker", "a", "sales", "متتبع الفرص والعروض", false,
            "دورة الفرصة من التأهيل حتى التعاقد: العروض المقدّمة والمكسوبة مقابل المستهدف، توزيعها على الجهات المالكة والقطاعات ومديري الحسابات وخطوط الأعمال والعملاء، مع سجل تفصيلي لكل فرصة.",
            Array.Empty<string>(),
            new[]
            {
                new BuiltinDashboardWidgetSpec("kpi", "العروض المقدّمة", Page: Summary),
                new BuiltinDashboardWidgetSpec("kpi", "الفرص المكسوبة", Page: Summary),
                new BuiltinDashboardWidgetSpec("kpi", "المستهدف السنوي", Page: Summary),
                new BuiltinDashboardWidgetSpec("kpi", "استغلال الاتفاقيات الإطارية", Page: Summary),
                new BuiltinDashboardWidgetSpec("pie", "قيمة العروض المقدّمة حسب الجهة المالكة", Page: Summary),
                new BuiltinDashboardWidgetSpec("bar", "قيمة الفرص حسب القطاع", Page: Summary),
                new BuiltinDashboardWidgetSpec("pie", "عام أم خاص", Page: Summary),
                new BuiltinDashboardWidgetSpec("bar", "عدد الفرص حسب ربع التقديم", Page: Summary),
                new BuiltinDashboardWidgetSpec("bar", "عدد الفرص حسب مدير الحساب", Page: Summary),
                new BuiltinDashboardWidgetSpec("stages", "مسار الفرص من التأهيل حتى التعاقد", Page: Stages),
                new BuiltinDashboardWidgetSpec("pie", "الفرص حسب خط الأعمال", Page: Stages),
                new BuiltinDashboardWidgetSpec("pie", "الفرص حسب النوع", Page: Stages),
                new BuiltinDashboardWidgetSpec("bar", "الفرص حسب ربع التقديم", Page: Stages),
                new BuiltinDashboardWidgetSpec("bar", "الفرص حسب العميل", Page: Stages),
                new BuiltinDashboardWidgetSpec("kpi", "إجمالي الفرص المكسوبة", Page: Details),
                new BuiltinDashboardWidgetSpec("kpi", "قيمة الفرص المكسوبة", Page: Details),
                new BuiltinDashboardWidgetSpec("kpi", "متوسط قيمة الفرصة", Page: Details),
                new BuiltinDashboardWidgetSpec("kpi", "نسبة الفوز", Page: Details),
                new BuiltinDashboardWidgetSpec("bar", "قيمة الفرص المكسوبة حسب مدير الحساب", Page: Details),
                new BuiltinDashboardWidgetSpec("bar", "عدد الفرص المكسوبة حسب مدير الحساب", Page: Details),
                new BuiltinDashboardWidgetSpec("pie", "الفرص المكسوبة حسب خط الأعمال", Page: Details),
                new BuiltinDashboardWidgetSpec("bar", "الفرص المكسوبة حسب العميل", Page: Details),
                new BuiltinDashboardWidgetSpec("table", "سجل الفرص", Page: Details),
            },
            "ابنِ لوحة «متتبع الفرص والعروض» من {{sources}}، وتتكون من 3 صفحات بالعناصر التالية:\n{{widgets}}\n" +
            "- الهدف: دورة الفرصة من التأهيل حتى التعاقد — العروض المقدّمة والمكسوبة مقابل المستهدف، وتوزيعها على الجهات المالكة والقطاعات ومديري الحسابات وخطوط الأعمال والعملاء، مع سجل تفصيلي لكل فرصة.\n" +
            "- المصدر الأساسي سجل فرص على مستوى الصف (فرصة لكل صف): الرقم المرجعي، الاسم، العميل، مدير الحساب، الجهة المالكة، خط الأعمال، النوع (RFP / اتفاقية إطارية / RFQ)، القطاع، عام أم خاص، قناة التقديم، تاريخ التقديم، القيمة بدون الضريبة، المرحلة، والحالة الفرعية. اشتق كل العناصر من هذا السجل نفسه بالتجميع (SUM/COUNT/GROUP BY) حتى تبقى الأرقام متسقة بين الصفحات.\n" +
            "- «المستهدف السنوي» ونسبة تحقيقه تحتاج جدول مستهدفات صريح (السنة، القيمة)، و«استغلال الاتفاقيات الإطارية» يحتاج سقف الاتفاقيات والمستغل منها — إن لم يوجد أيٌّ منهما فاعرض العنصر noData بسببه، ولا تحسبه من بيانات أخرى.\n" +
            "- «مسار الفرص من التأهيل حتى التعاقد» عنصر stages بالمراحل الأربع بالترتيب: التأهيل ← إعداد الاستجابة ← التقديم ← التعاقد، مع الحالات الفرعية (ملغاة، غير مؤهلة، معلّقة، خاسرة، متأخرة) كصفوف row=\"sub\"، والقيمة بدون الضريبة في value حيث تتوفر.\n" +
            "- «سجل الفرص» جدول بالأعمدة: مدير الحساب، الرقم المرجعي، الاسم، الجهة المالكة، القيمة بدون الضريبة، قناة التقديم، التاريخ، القطاع، المرحلة.\n" +
            "- اعرض القيم المالية بنفس عملة مصادر البيانات، وبدون ضريبة القيمة المضافة.\n" +
            "- أضف فلاتر أعلى اللوحة: السنة، مدير الحساب، الجهة المالكة، نوع الفرصة.\n" +
            "- إذا لم يتوفر عمود مطلوب في المصادر، اسأل المستخدم بدل الافتراض."),
    };
}
