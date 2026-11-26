namespace ChatToDashboard.Api.Templates;

/// <summary>
/// نموذج «لوحة أداء المتجر» (<c>pl-store-performance</c>، فئة المبيعات، 3 صفحات) في «لوحات المتابعة»، مماثل لنموذج
/// الشرائح الجاهز «لوحة أداء المتجر الزرقاء» (<c>sl-blue-store-performance-dashboard</c> في تصميم حر): الصفحات الثلاث
/// نفسها بعناوين بطاقاتها ورسومها، لكن كل عنصر يُبنى من بيانات المشروع الحقيقية بدل الأرقام التوضيحية في الشرائح.
/// مكتوب يدويًا هنا (كمتتبع الفرص) لا في <see cref="BuiltinDashboardsGenerated"/> المولَّد آليًا، ومدخله المطابق
/// للمعاينة في آخر wwwroot/js/dash-library.js بالعناوين والصفحات نفسها حرفيًا.
/// لا يُلفَّق رقم خارجي بطبيعته: المساحة (قدم مربع)، الزوار، المبيعات المتوقعة، والخصم/التكلفة تحتاج أعمدتها الصريحة.
/// </summary>
public static class BuiltinDashboardsStorePerformance
{
    private const string Kpis = "مؤشرات المبيعات";
    private const string Geo = "المناطق والمتاجر";
    private const string Strategy = "الاستراتيجية والمخزون";

    public static readonly IReadOnlyList<BuiltinDashboardTemplate> Items = new[]
    {
        new BuiltinDashboardTemplate("pl-store-performance", "a", "sales", "لوحة أداء المتجر", false,
            "أداء البيع بالتجزئة في ثلاث صفحات: مؤشرات المبيعات والربحية، المبيعات حسب المنطقة والمتاجر الرئيسية ومعدل التحويل، " +
            "والاستراتيجية بأكثر المنتجات مبيعًا والمبيعات الفعلية مقابل المتوقعة.",
            Array.Empty<string>(),
            new[]
            {
                new BuiltinDashboardWidgetSpec("kpi", "الإيرادات", Page: Kpis),
                new BuiltinDashboardWidgetSpec("kpi", "صافي الربح", Page: Kpis),
                new BuiltinDashboardWidgetSpec("kpi", "صافي الدخل لكل قدم مربع", Page: Kpis),
                new BuiltinDashboardWidgetSpec("kpi", "معدل الاحتفاظ بالعملاء", Page: Kpis),
                new BuiltinDashboardWidgetSpec("bar", "المبيعات لكل قدم مربع حسب الموقع", Page: Kpis),
                new BuiltinDashboardWidgetSpec("bar", "الوحدات لكل معاملة حسب المنتج", Page: Kpis),
                new BuiltinDashboardWidgetSpec("bar", "المبيعات حسب المنتج والسنة", Page: Kpis),
                new BuiltinDashboardWidgetSpec("kpi", "المبيعات الإجمالية", Page: Geo),
                new BuiltinDashboardWidgetSpec("kpi", "متوسط المبيعات اليومية", Page: Geo),
                new BuiltinDashboardWidgetSpec("kpi", "قيمة معاملة العميل (المتوسط)", Page: Geo),
                new BuiltinDashboardWidgetSpec("kpi", "متوسط الوحدات لكل عميل", Page: Geo),
                new BuiltinDashboardWidgetSpec("table", "إيرادات المتاجر الرئيسية", Page: Geo),
                new BuiltinDashboardWidgetSpec("bar", "المبيعات حسب الفئة", Page: Geo),
                new BuiltinDashboardWidgetSpec("pie", "المبيعات حسب المنطقة الجغرافية", Page: Geo),
                new BuiltinDashboardWidgetSpec("radial-gauge", "معدل التحويل", Page: Geo),
                new BuiltinDashboardWidgetSpec("line", "قيمة السلة (المتوسط)", Page: Geo),
                new BuiltinDashboardWidgetSpec("kpi", "إجمالي المبيعات", Page: Strategy),
                new BuiltinDashboardWidgetSpec("kpi", "إجمالي عدد العملاء", Page: Strategy),
                new BuiltinDashboardWidgetSpec("kpi", "متوسط قيمة المعاملة", Page: Strategy),
                new BuiltinDashboardWidgetSpec("kpi", "متوسط الوحدات في المعاملة", Page: Strategy),
                new BuiltinDashboardWidgetSpec("bar", "أكثر المنتجات مبيعًا هذا العام", Page: Strategy),
                new BuiltinDashboardWidgetSpec("bar", "أكثر المنتجات مبيعًا العام السابق", Page: Strategy),
                new BuiltinDashboardWidgetSpec("line", "الزوار مقابل إجمالي العملاء", Page: Strategy),
                new BuiltinDashboardWidgetSpec("line", "سعر الوحدة في كل معاملة", Page: Strategy),
                new BuiltinDashboardWidgetSpec("bar", "المبيعات الفعلية مقابل المتوقعة حسب الفئة", Page: Strategy),
                new BuiltinDashboardWidgetSpec("bar", "المبيعات الفعلية مقابل المتوقعة حسب المنطقة", Page: Strategy),
            },
            "ابنِ «لوحة أداء المتجر» من {{sources}}، وتتكون من 3 صفحات بالعناصر التالية:\n{{widgets}}\n" +
            "- الهدف: أداء البيع بالتجزئة — الإيرادات والربحية لكل موقع ومتر/قدم مربع، المبيعات حسب المنطقة والمتاجر والفئات ومعدل التحويل وقيمة السلة، وأكثر المنتجات مبيعًا والمبيعات الفعلية مقابل المتوقعة.\n" +
            "- المصدر الأساسي سجل مبيعات على مستوى المعاملة أو البند (معاملة/بند لكل صف): التاريخ، المتجر أو الموقع، المنطقة، الفئة، المنتج، الكمية، السعر أو المبلغ، والعميل إن وجد. اشتق كل العناصر من هذا السجل نفسه بالتجميع (SUM/COUNT/COUNT DISTINCT/GROUP BY) حتى تبقى الأرقام متسقة بين الصفحات.\n" +
            "- «المبيعات حسب المنتج والسنة» و«المبيعات لكل قدم مربع حسب الموقع» أعمدة متعددة السلاسل (سلسلة لكل سنة)؛ و«المبيعات حسب الفئة» أعمدة صافي المبيعات مع سلسلة إجمالي الربح إن توفّرت التكلفة.\n" +
            "- «إيرادات المتاجر الرئيسية» جدول بالأعمدة: المتجر، إجمالي المبيعات، الخصم، صافي المبيعات، تكلفة المبيعات، إجمالي الربح، هامش الربح — أكبر المتاجر بالمبيعات.\n" +
            "- ما يحتاج بيانات خارجية بطبيعته لا يُحسب من غيره: المساحة بالقدم المربع (المبيعات وصافي الدخل لكل قدم مربع)، الزوار (معدل التحويل والزوار مقابل العملاء)، المبيعات المتوقعة، الخصم وتكلفة المبيعات، وصافي الربح — إن غاب عموده فاعرض العنصر noData بسببه.\n" +
            "- «معدل الاحتفاظ بالعملاء» = نسبة عملاء الفترة السابقة الذين اشتروا في الحالية، ويحتاج معرّف عميل؛ وإلا noData.\n" +
            "- اعرض القيم المالية بعملة مصادر البيانات.\n" +
            "- أضف فلاتر أعلى اللوحة: السنة، المنطقة، المتجر، الفئة.\n" +
            "- إذا لم يتوفر عمود مطلوب في المصادر، اسأل المستخدم بدل الافتراض."),
    };
}
