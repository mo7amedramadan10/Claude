namespace ChatToDashboard.Api.Build;

/// <summary>
/// شكل حقل <c>data</c> لكل نوع عنصر، مكثّفًا، ليُرسَل لنداء الرسمة الواحدة **للنوع المطلوب فقط**
/// بدل توثيق الأنواع العشرين كاملًا في كل نداء (انظر BuildSystemPrompt في AnalyticsTools لمرجع
/// الأشكال الأصلي؛ هذه نسخة مختصرة منه بنفس العقود حرفيًا — أي تغيير في شكل نوع هناك يجب أن
/// ينعكس هنا).
/// </summary>
public static class WidgetShapeCatalog
{
    private static readonly IReadOnlyDictionary<string, string> Shapes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["kpi"] = """
            data: [{"label": "وصف قصير", "value": <رقم>}] — عنصر واحد فقط. القيمة رقم JSON لا نص.
            options اختياري: {"tg": <نسبة المستهدف ٪>, "tgv": <قيمة المستهدف>} عندما يوجد مستهدف حقيقي في البيانات.
            """,
        ["bar"] = """
            data: مصفوفة كائنات، مع xKey لحقل التصنيف وyKey للحقل الرقمي.
            مثال: data [{"month": "2024-01", "revenue": 1234.5}] مع "xKey": "month" و"yKey": "revenue".
            لعدة سلاسل: بدل yKey ضع "series": [{"key": "...", "label": "..."}] وكل مفتاح موجود في كل صف؛ "stacked": true للتكديس.
            ١٥ صفًا كحد أقصى — جمّع أو رتّب (Top N) قبل ذلك. options اختياري: {"spot": "max" | "last"} لإبراز عمود واحد.
            """,
        ["line"] = """
            data: مصفوفة كائنات مرتبة زمنيًا، مع xKey لحقل الزمن وyKey للحقل الرقمي.
            مثال: data [{"month": "2024-01", "revenue": 1234.5}] مع "xKey": "month" و"yKey": "revenue".
            لعدة سلاسل: "series": [{"key": "...", "label": "..."}]؛ "areaFill": true لتعبئة المساحة. ١٥ نقطة كحد أقصى.
            """,
        ["pie"] = """
            data: [{"label": "...", "value": <رقم>}, ...] — ٦ شرائح كحد أقصى والباقي في "أخرى".
            "centerLabel" اختياري: كلمة تصف ما يمثّله الرقم في المنتصف.
            """,
        ["table"] = """
            data: مصفوفة صفوف؛ مفاتيح كل صف هي عناوين الأعمدة (بالعربية). ١٥ صفًا كحد أقصى.
            """,
        ["progress-table"] = """
            data: [{"label": "اسم الصف", "total": <الإجمالي أو المستهدف>, "segments": [{"value": <رقم>, "label": "...", "color": "accent" | "danger" | "amber" | "muted"}]}]
            """,
        ["trend-matrix"] = """
            data: [{"metric": "اسم المؤشر", "periods": [{"label": "Q1 2025", "value": <رقم>}, ...]}] — كل الصفوف بنفس الفترات وبنفس ترتيبها.
            لا تحسب الاتجاه بنفسك؛ الواجهة تحسبه.
            """,
        ["status-bar"] = """
            data: كائن واحد (لا مصفوفة): {"total": <رقم>, "label": "وصف الإجمالي", "statuses": [{"label": "...", "count": <رقم>, "severity": "severe" | "warning" | "ontrack"}]}
            """,
        ["radial-gauge"] = """
            data: كائن واحد (لا مصفوفة): {"value": <الفعلي>, "max": <السقف، افتراضه ١٠٠>, "label": "وصف قصير", "tone": "accent" | "danger" | "amber"}
            لا تحسب النسبة بنفسك؛ مرّر الأرقام الخام.
            """,
        ["linear-gauge"] = """
            data: [{"label": "اسم الصف", "value": <رقم>, "max": <رقم اختياري>, "tone": "accent" | "danger" | "amber"}]
            """,
        ["radar"] = """
            data: [{"axis": "اسم المحور", "<اسم السلسلة>": <رقم>, ...}] — ٥ إلى ٨ محاور بنفس المقياس، ٣ سلاسل كحد أقصى. options اختياري: {"max": <رقم>}.
            """,
        ["semi"] = """
            data: [{"label": "...", "value": <رقم>}] — صفّان أو ثلاثة بالضبط. options اختياري: {"tl": "وصف الإجمالي"}.
            """,
        ["dco"] = """
            data: [{"label": "...", "value": <رقم>}] — ٣ إلى ٥ صفوف. options اختياري: {"tl": "وصف الإجمالي"}.
            """,
        ["iconrank"] = """
            data: [{"label": "...", "value": <رقم>}] — تُرتَّب تنازليًا في الواجهة. options اختياري: {"av": ["رمز قصير لكل صف"], "u": "وحدة"}.
            """,
        ["ribbon"] = """
            data: [{"period": "اسم الفترة", "category": "اسم الفئة", "value": <رقم>}] — صف لكل (فترة، فئة)، ٣ إلى ٦ فترات، ٦ فئات كحد أقصى، ونفس الفئات في كل فترة.
            """,
        ["people"] = """
            data: [{"name": "...", "value": <رقم>, "goalPct": <٠-١٠٠ اختياري>, "role": "اختياري"}]. options اختياري: {"u": "وحدة"}.
            """,
        ["feed"] = """
            data: [{"title": "...", "desc": "اختياري", "value": "اختياري", "status": "ok" | "pend" | "bad" | "info", "time": "اختياري", "owner": "اختياري"}]
            """,
        ["goal"] = """
            data: كائن واحد (لا مصفوفة): {"percent": <نسبة الإنجاز>, "remaining": <اختياري>, "who": "اختياري", "days": <اختياري>, "headline": "اختياري"}
            """,
        ["stages"] = """
            data: مصفوفة طويلة الشكل، صف لكل خانة كما يخرج من GROUP BY على المرحلة والحالة:
            [{"stage": "اسم المرحلة", "label": "اسم الخانة", "count": <عدد>, "tone": "info" | "ok" | "warn" | "bad" | "dark" | "mute", "value": <قيمة مالية اختياري>, "row": "main" | "sub" | "total"}]
            رتّب المراحل بترتيبها الفعلي؛ row="sub" للحالات الفرعية (ملغاة، معلّقة، متأخرة...)، row="total" صف اختياري بإجمالي المرحلة. options اختياري: {"u": "وحدة value"}.
            """,
        ["bullet"] = """
            data: [{"label": "...", "actual": <رقم>, "target": <رقم>}]. options اختياري: {"hi": true} عندما يكون الأعلى أفضل.
            """,
    };

    /// <summary>الأنواع التي يكون <c>data</c> فيها كائنًا واحدًا لا مصفوفة (نفس قائمة DashboardSpec).</summary>
    public static readonly IReadOnlySet<string> ObjectShaped =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "status-bar", "radial-gauge", "goal" };

    public static bool IsKnown(string? type) => type is not null && Shapes.ContainsKey(type);

    /// <summary>وصف الشكل للنوع، أو شكل <c>table</c> لنوع غير معروف (الواجهة ترسم المجهول كجدول).</summary>
    public static string Describe(string? type) =>
        type is not null && Shapes.TryGetValue(type, out var text) ? text.Trim() : Shapes["table"].Trim();

    /// <summary>النوع الفعلي الذي يُبنى به عنصر قالب: الأنواع المعروفة كما هي، وأنواع مكتبة
    /// المعاينة (JR) تُترجم لأقرب نوع حقيقي، والمجهول جدول.</summary>
    public static string NormalizeType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type)) return "table";
        if (Shapes.ContainsKey(type)) return type.ToLowerInvariant();
        return type.ToLowerInvariant() switch
        {
            "cbars" or "grouped" or "stacked" or "waterfall" or "hbars" or "rank" or "compare" or "s100" or "div" => "bar",
            "area" or "cum" or "combo" => "line",
            "donut" or "ring" or "gauges" => "pie",
            "funnel" or "heat" or "cohort" or "risk" or "scatter" or "bubble" or "insights" or "note" => "table",
            "hero" or "kpiyoy" => "kpi",
            _ => "table",
        };
    }
}
