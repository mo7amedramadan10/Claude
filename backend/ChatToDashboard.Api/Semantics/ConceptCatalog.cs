namespace ChatToDashboard.Api.Semantics;

/// <summary>أدوار الأعمدة في التحليل: مقياس يُجمَّع، أو بُعد يُقسَّم عليه، أو زمن، أو معرّف، أو نص حر، أو علم منطقي.</summary>
public static class ConceptRoles
{
    public const string Measure = "measure";
    public const string Dimension = "dimension";
    public const string Time = "time";
    public const string Identifier = "identifier";
    public const string Text = "text";
    public const string Flag = "flag";
}

/// <summary>مفهوم واحد في القاموس الثابت.</summary>
/// <param name="Id">معرّف ثابت بالإنجليزية (مفتاح التخزين والقوالب).</param>
/// <param name="Label">التسمية العربية الظاهرة للمستخدم.</param>
/// <param name="Role">أحد <see cref="ConceptRoles"/>.</param>
/// <param name="Synonyms">أسماء أعمدة شائعة (بالعربية والإنجليزية، بحروف صغيرة) تدل عليه — للتخمين الحتمي الأولي.</param>
/// <param name="CanonicalValues">للأبعاد الحالية (مثل حالة الصفقة): القيم القياسية التي تُربط بها قيم العميل.</param>
public sealed record Concept(string Id, string Label, string Role, IReadOnlyList<string> Synonyms, IReadOnlyList<string>? CanonicalValues = null);

/// <summary>
/// القاموس الثابت لطبقة المعنى: مفاهيم تحليلية عامة (لا أسماء أعمدة قوالب) تُربط بها أعمدة أي ملف
/// يرفعه أي عميل. القوالب تُكتب بهذه المفاهيم، وجدوى كل عنصر تُحسم حتميًا بوجود مفهومه في
/// المشروع. الإضافة هنا آمنة؛ إعادة تسمية معرّف تكسر ما خُزّن في <c>ColumnSemantics</c>.
/// </summary>
public static class ConceptCatalog
{
    private static readonly IReadOnlyList<Concept> All = new[]
    {
        // --- مقاييس مالية ---
        new Concept("revenue", "الإيراد", ConceptRoles.Measure, new[] { "revenue", "sales", "amount", "total", "net_sales", "income", "الإيراد", "الايراد", "المبيعات", "قيمة المبيعات", "الإيرادات", "المبلغ", "الإجمالي" }),
        new Concept("cost", "التكلفة", ConceptRoles.Measure, new[] { "cost", "cogs", "expense", "expenses", "spend", "التكلفة", "المصروف", "المصروفات", "الإنفاق" }),
        new Concept("profit", "الربح", ConceptRoles.Measure, new[] { "profit", "margin", "net_profit", "gross_profit", "الربح", "الهامش", "صافي الربح" }),
        new Concept("budget", "الميزانية", ConceptRoles.Measure, new[] { "budget", "planned", "الميزانية", "المخطط", "المعتمد" }),
        new Concept("target", "المستهدف", ConceptRoles.Measure, new[] { "target", "goal", "quota", "plan", "المستهدف", "الهدف", "الحصة" }),
        new Concept("unit_price", "سعر الوحدة", ConceptRoles.Measure, new[] { "unit_price", "price", "unitprice", "rate", "السعر", "سعر الوحدة" }),
        new Concept("discount", "الخصم", ConceptRoles.Measure, new[] { "discount", "الخصم" }),
        new Concept("tax", "الضريبة", ConceptRoles.Measure, new[] { "tax", "vat", "الضريبة", "ضريبة القيمة المضافة" }),
        new Concept("balance", "الرصيد", ConceptRoles.Measure, new[] { "balance", "outstanding", "due", "الرصيد", "المتبقي", "المستحق" }),
        new Concept("payment_amount", "المدفوع", ConceptRoles.Measure, new[] { "paid", "payment", "collected", "المدفوع", "المحصل", "التحصيل" }),
        new Concept("salary", "الراتب", ConceptRoles.Measure, new[] { "salary", "wage", "pay", "الراتب", "الأجر" }),
        // --- مقاييس كمية ---
        new Concept("quantity", "الكمية", ConceptRoles.Measure, new[] { "quantity", "qty", "units", "count", "الكمية", "العدد", "الوحدات" }),
        new Concept("probability", "احتمالية الفوز", ConceptRoles.Measure, new[] { "probability", "win_probability", "likelihood", "الاحتمالية", "احتمال الفوز" }),
        new Concept("score", "الدرجة", ConceptRoles.Measure, new[] { "score", "rating", "grade", "الدرجة", "التقييم" }),
        new Concept("percentage", "نسبة", ConceptRoles.Measure, new[] { "percent", "percentage", "pct", "ratio", "النسبة", "نسبة" }),
        new Concept("duration_days", "المدة بالأيام", ConceptRoles.Measure, new[] { "duration", "days", "age", "cycle", "lead_time", "المدة", "الأيام", "العمر" }),
        new Concept("hours", "الساعات", ConceptRoles.Measure, new[] { "hours", "hrs", "الساعات" }),
        new Concept("headcount", "عدد الموظفين", ConceptRoles.Measure, new[] { "headcount", "employees", "staff_count", "عدد الموظفين" }),
        new Concept("stock_level", "مستوى المخزون", ConceptRoles.Measure, new[] { "stock", "inventory", "on_hand", "المخزون", "الرصيد المخزني" }),
        new Concept("visits", "الزيارات", ConceptRoles.Measure, new[] { "visits", "sessions", "pageviews", "views", "الزيارات", "المشاهدات" }),
        new Concept("clicks", "النقرات", ConceptRoles.Measure, new[] { "clicks", "النقرات" }),
        new Concept("impressions", "الظهور", ConceptRoles.Measure, new[] { "impressions", "الظهور" }),
        new Concept("conversions", "التحويلات", ConceptRoles.Measure, new[] { "conversions", "leads", "signups", "التحويلات" }),
        // --- الزمن ---
        new Concept("order_date", "تاريخ الطلب", ConceptRoles.Time, new[] { "order_date", "orderdate", "date", "transaction_date", "invoice_date", "التاريخ", "تاريخ الطلب", "تاريخ الفاتورة", "تاريخ العملية" }),
        new Concept("created_date", "تاريخ الإنشاء", ConceptRoles.Time, new[] { "created", "created_at", "createdat", "open_date", "start_date", "تاريخ الإنشاء", "تاريخ البداية", "تاريخ الفتح" }),
        new Concept("close_date", "تاريخ الإغلاق", ConceptRoles.Time, new[] { "close_date", "closed_at", "end_date", "completed_at", "تاريخ الإغلاق", "تاريخ النهاية", "تاريخ الإنجاز" }),
        new Concept("due_date", "تاريخ الاستحقاق", ConceptRoles.Time, new[] { "due_date", "deadline", "تاريخ الاستحقاق", "الموعد النهائي" }),
        new Concept("period", "الفترة", ConceptRoles.Time, new[] { "period", "month", "year", "quarter", "week", "الفترة", "الشهر", "السنة", "الربع", "الأسبوع" }),
        new Concept("hire_date", "تاريخ التعيين", ConceptRoles.Time, new[] { "hire_date", "join_date", "تاريخ التعيين", "تاريخ الالتحاق" }),
        // --- أبعاد ---
        new Concept("region", "المنطقة", ConceptRoles.Dimension, new[] { "region", "city", "area", "country", "branch", "location", "المنطقة", "المدينة", "الفرع", "الدولة", "الموقع" }),
        new Concept("product", "المنتج", ConceptRoles.Dimension, new[] { "product", "item", "sku", "service", "المنتج", "الصنف", "الخدمة" }),
        new Concept("category", "الفئة", ConceptRoles.Dimension, new[] { "category", "type", "class", "segment", "line", "الفئة", "النوع", "التصنيف", "الشريحة", "خط الأعمال" }),
        new Concept("customer", "العميل", ConceptRoles.Dimension, new[] { "customer", "client", "account", "company", "العميل", "الجهة", "الشركة", "الحساب" }),
        new Concept("supplier", "المورّد", ConceptRoles.Dimension, new[] { "supplier", "vendor", "المورد", "المورّد" }),
        new Concept("owner", "المسؤول", ConceptRoles.Dimension, new[] { "owner", "rep", "sales_rep", "salesrep", "agent", "assignee", "manager", "employee", "المسؤول", "المندوب", "مندوب المبيعات", "مدير الحساب", "الموظف" }),
        new Concept("department", "الإدارة", ConceptRoles.Dimension, new[] { "department", "dept", "unit", "team", "الإدارة", "القسم", "الوحدة", "الفريق" }),
        new Concept("channel", "القناة", ConceptRoles.Dimension, new[] { "channel", "source", "medium", "campaign", "القناة", "المصدر", "الحملة" }),
        new Concept("project", "المشروع", ConceptRoles.Dimension, new[] { "project", "initiative", "المشروع", "المبادرة" }),
        new Concept("priority", "الأولوية", ConceptRoles.Dimension, new[] { "priority", "severity", "الأولوية", "الخطورة" }, new[] { "high", "medium", "low" }),
        new Concept("sector", "القطاع", ConceptRoles.Dimension, new[] { "sector", "industry", "القطاع", "الصناعة" }),
        new Concept("nationality", "الجنسية", ConceptRoles.Dimension, new[] { "nationality", "الجنسية" }),
        new Concept("gender", "الجنس", ConceptRoles.Dimension, new[] { "gender", "sex", "الجنس", "النوع الاجتماعي" }),
        new Concept("job_title", "المسمى الوظيفي", ConceptRoles.Dimension, new[] { "title", "job_title", "position", "role", "المسمى", "المسمى الوظيفي", "الوظيفة" }),
        // --- حالات ومراحل (أبعاد بقيم قياسية) ---
        new Concept("deal_status", "حالة الصفقة", ConceptRoles.Dimension, new[] { "status", "deal_status", "result", "outcome", "الحالة", "النتيجة" }, new[] { "open", "won", "lost", "cancelled" }),
        new Concept("deal_stage", "مرحلة الصفقة", ConceptRoles.Dimension, new[] { "stage", "pipeline_stage", "phase", "المرحلة", "مرحلة" }),
        new Concept("task_status", "حالة المهمة", ConceptRoles.Dimension, new[] { "task_status", "progress", "حالة المهمة", "التقدم" }, new[] { "todo", "in_progress", "done", "blocked" }),
        new Concept("payment_status", "حالة الدفع", ConceptRoles.Dimension, new[] { "payment_status", "paid_status", "حالة الدفع", "حالة السداد" }, new[] { "paid", "unpaid", "partial", "overdue" }),
        new Concept("opportunity_type", "نوع الفرصة", ConceptRoles.Dimension, new[] { "opportunity_type", "deal_type", "tender_type", "نوع الفرصة", "نوع المنافسة" }, new[] { "rfp", "rfq", "framework", "direct" }),
        new Concept("loss_reason", "سبب الخسارة", ConceptRoles.Dimension, new[] { "loss_reason", "lost_reason", "reason", "سبب الخسارة", "السبب" }),
        // --- معرّفات ونصوص ---
        new Concept("record_id", "معرّف السجل", ConceptRoles.Identifier, new[] { "id", "ref", "reference", "code", "number", "no", "المعرف", "الرقم", "الرقم المرجعي", "الكود" }),
        new Concept("name", "الاسم", ConceptRoles.Text, new[] { "name", "subject", "description", "notes", "الاسم", "الوصف", "الموضوع", "ملاحظات" }),
        new Concept("email", "البريد", ConceptRoles.Text, new[] { "email", "mail", "البريد" }),
        new Concept("phone", "الهاتف", ConceptRoles.Text, new[] { "phone", "mobile", "tel", "الهاتف", "الجوال" }),
        new Concept("is_active", "نشط؟", ConceptRoles.Flag, new[] { "active", "is_active", "enabled", "نشط", "فعال" }),
        new Concept("is_public", "عام/خاص", ConceptRoles.Flag, new[] { "public", "is_public", "sector_type", "عام", "خاص", "عام أم خاص" }),
    };

    private static readonly IReadOnlyDictionary<string, Concept> ById = All.ToDictionary(c => c.Id, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<Concept> Concepts => All;

    public static Concept? Find(string? id) => id is not null && ById.TryGetValue(id, out var c) ? c : null;

    public static bool IsKnown(string? id) => Find(id) is not null;

    /// <summary>
    /// تخمين حتمي أولي من اسم العمود وحده: مطابقة كاملة لمرادف تفوز، ثم مطابقة «الاسم يحتوي
    /// المرادف» للمرادفات الأطول من 3 أحرف. يرجع null عند عدم وجود مرشح — الموديل يحسم بعدها.
    /// </summary>
    public static (Concept Concept, double Confidence)? GuessFromName(string columnName)
    {
        var norm = Normalize(columnName);
        if (norm.Length == 0) return null;
        foreach (var c in All)
            if (c.Synonyms.Any(s => Normalize(s) == norm)) return (c, 0.8);
        Concept? best = null; var bestLen = 0;
        foreach (var c in All)
            foreach (var s in c.Synonyms)
            {
                var ns = Normalize(s);
                if (ns.Length > 3 && norm.Contains(ns) && ns.Length > bestLen) { best = c; bestLen = ns.Length; }
            }
        return best is null ? null : (best, 0.5);
    }

    /// <summary>توحيد اسم للمقارنة: حروف صغيرة، بلا فراغات أو شرطات أو «ال» التعريف أو همزات مختلفة.</summary>
    internal static string Normalize(string s)
    {
        var t = s.Trim().ToLowerInvariant().Replace("أ", "ا").Replace("إ", "ا").Replace("آ", "ا").Replace("ة", "ه").Replace("ى", "ي");
        t = new string(t.Where(ch => !char.IsWhiteSpace(ch) && ch != '_' && ch != '-' && ch != '.').ToArray());
        return t.StartsWith("ال") && t.Length > 4 ? t[2..] : t;
    }

    /// <summary>نص القاموس المُرسَل للموديل عند تحليل ملف: معرّف — تسمية (دور) [قيم قياسية].</summary>
    public static string ToPromptText() => string.Join("\n", All.Select(c =>
        $"- {c.Id} — {c.Label} ({c.Role}){(c.CanonicalValues is { Count: > 0 } v ? " [القيم القياسية: " + string.Join("/", v) + "]" : "")}"));
}
