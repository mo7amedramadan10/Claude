using System.Text.RegularExpressions;
using ChatToDashboard.Api.SchemaLibrary;
using ChatToDashboard.Api.Templates;

namespace ChatToDashboard.Api.Semantics;

/// <summary>متطلب مفهومي واحد لعنصر قالب: يكفي وجود **أي** مفهوم من <see cref="AnyOf"/> في بيانات المشروع.
/// الصلب (<see cref="Hard"/>) يحسم عدم الجدوى حتميًا عند غيابه؛ اللين مجرد تلميح للمخطِّط.</summary>
public sealed record ConceptRequirement(IReadOnlyList<string> AnyOf, bool Hard, string Label)
{
    public static ConceptRequirement Of(string label, bool hard, params string[] anyOf) => new(anyOf, hard, label);
}

/// <summary>نتيجة الجدوى الحتمية لعنصر واحد.</summary>
public sealed record WidgetFeasibility(bool Feasible, IReadOnlyList<string> Missing, IReadOnlyList<string> Satisfied)
{
    public static readonly WidgetFeasibility Unknown = new(true, Array.Empty<string>(), Array.Empty<string>());
}

/// <summary>
/// يستنتج متطلبات المفاهيم لعنصر قالب من **عنوانه** (الإشارة الأصدق — مكتوب للبشر) ومن مسودة
/// استعلامه (<c>Table</c>/<c>Sql</c>، إشارة لينة لأن أعمدتها مُولَّدة من العنوان آليًا). القاعدة:
/// الصلب هو ما لا يُشتق من ملف مبيعات عام (مستهدف، احتمالية، مرحلة، سبب خسارة…) فغيابه يعني
/// «غير ممكن» قبل أي نداء للموديل؛ واللين (إيراد، منطقة، منتج) تلميح فقط.
/// </summary>
public static class WidgetRequirementResolver
{
    private static readonly (Regex Pattern, ConceptRequirement Requirement)[] TitleRules =
    {
        (Rx(@"مستهدف|المستهدف|حصة|الحصة|مخطط مقابل|مقابل الخطة|الخطة|quota|target"), ConceptRequirement.Of("المستهدف", true, "target")),
        (Rx(@"ميزانية|الميزانية|budget"), ConceptRequirement.Of("الميزانية", true, "budget")),
        (Rx(@"مرجّح|مرجح|احتمال|احتمالية|weighted|probability"), ConceptRequirement.Of("احتمالية الفوز", true, "probability")),
        (Rx(@"سبب الخسارة|أسباب الخسارة|سبب خسارة|الخسارة حسب السبب|الخسائر حسب السبب|loss reason"), ConceptRequirement.Of("سبب الخسارة", true, "loss_reason")),
        (Rx(@"مرحلة|المراحل|مراحل|قمع|funnel|\bstages?\b"), ConceptRequirement.Of("مرحلة الصفقة أو حالتها", true, "deal_stage", "task_status", "deal_status")),
        (Rx(@"فوز|الفوز|خسارة|الخسارة|مكسوب|خاسر|أنبوب|الأنبوب|pipeline|win rate|\bwon\b|\blost\b"), ConceptRequirement.Of("حالة الصفقة", true, "deal_status")),
        (Rx(@"دورة البيع|أعمار|عمر الأنبوب|عمر الصفقة|عمر الفرصة|راكد|الراكد|ركود|aging|sales cycle|cycle time|\bcycle\b"), ConceptRequirement.Of("تاريخ الإغلاق أو المدة", true, "close_date", "duration_days")),
        (Rx(@"مندوب|المندوب|مدير الحساب|لكل مسؤول|حسب المسؤول|المسؤول|\breps?\b|\bowner\b"), ConceptRequirement.Of("المسؤول/المندوب", true, "owner")),
        (Rx(@"عميل|العملاء|عملاء|customer|client"), ConceptRequirement.Of("العميل", true, "customer")),
        (Rx(@"مورد|الموردين|موردين|supplier|vendor"), ConceptRequirement.Of("المورّد", true, "supplier")),
        (Rx(@"قناة|القنوات|قنوات|مصدر العميل|مصادر|حملة|الحملات|channel|campaign"), ConceptRequirement.Of("القناة/الحملة", true, "channel")),
        (Rx(@"قطاع|القطاع|شريحة|الشريحة|segment|sector|industry"), ConceptRequirement.Of("القطاع/الشريحة", true, "sector", "category")),
        (Rx(@"تكلفة|التكلفة|مصروف|المصروفات|إنفاق|الإنفاق|\bcosts?\b|expense|\bspend\b"), ConceptRequirement.Of("التكلفة", true, "cost")),
        (Rx(@"ربح|الربح|هامش|الهامش|profit|margin"), ConceptRequirement.Of("الربح أو التكلفة", true, "profit", "cost")),
        (Rx(@"راتب|رواتب|الرواتب|salary|payroll"), ConceptRequirement.Of("الراتب", true, "salary")),
        (Rx(@"إدارة|الإدارات|القسم|الأقسام|حسب القسم|لكل قسم|\bdepartments?\b"), ConceptRequirement.Of("الإدارة", true, "department")),
        (Rx(@"تحصيل|التحصيل|المدفوع|سداد|collection|\bpaid\b"), ConceptRequirement.Of("التحصيل/السداد", true, "payment_amount", "payment_status", "balance")),
        (Rx(@"مستحق|متأخر السداد|ذمم|الذمم|overdue|receivable"), ConceptRequirement.Of("الذمم/الاستحقاق", true, "balance", "due_date", "payment_status")),
        (Rx(@"مخزون|المخزون|\bstock\b|inventory"), ConceptRequirement.Of("المخزون", true, "stock_level")),
        (Rx(@"زيارات|الزيارات|جلسات|الجلسات|\bvisits?\b|\bsessions?\b"), ConceptRequirement.Of("الزيارات", true, "visits")),
        (Rx(@"نقرات|النقرات|\bclicks?\b"), ConceptRequirement.Of("النقرات", true, "clicks")),
        (Rx(@"ظهور|الظهور|impression"), ConceptRequirement.Of("الظهور", true, "impressions")),
        (Rx(@"تحويل|التحويلات|conversion"), ConceptRequirement.Of("التحويلات", true, "conversions")),
        (Rx(@"ساعات|الساعات|hours"), ConceptRequirement.Of("الساعات", true, "hours")),
        (Rx(@"أولوية|الأولوية|priority"), ConceptRequirement.Of("الأولوية", true, "priority")),
        (Rx(@"مهمة|مهام|المهام|\btasks?\b"), ConceptRequirement.Of("حالة المهمة", true, "task_status")),
        (Rx(@"موظف|الموظفين|موظفين|التعيين|headcount|employee|hiring"), ConceptRequirement.Of("الموظفون", true, "headcount", "owner", "job_title", "hire_date")),
        // زمنية: بدون أي عمود تاريخ أو فترة لا يُبنى اتجاه.
        (Rx(@"شهر|شهري|شهريًا|سنوي|سنويًا|ربع|أسبوع|اتجاه|يومي|يوميًا|عبر الزمن|زمني|العام السابق|السنة الماضية|monthly|trend|daily|weekly|yoy"),
            ConceptRequirement.Of("عمود زمني", true, "order_date", "created_date", "close_date", "period")),
        // لينة: الأبعاد والمقاييس العامة تلميح للمخطِّط لا شرط.
        (Rx(@"منطقة|مناطق|المناطق|فرع|فروع|الفروع|مدينة|دولة|region|branch|city|country"), ConceptRequirement.Of("المنطقة", false, "region")),
        (Rx(@"منتج|منتجات|المنتجات|صنف|أصناف|product|\bsku\b|\bitems?\b"), ConceptRequirement.Of("المنتج", false, "product")),
        (Rx(@"فئة|الفئة|فئات|نوع|النوع|تصنيف|category|\btype\b"), ConceptRequirement.Of("الفئة", false, "category", "sector", "product")),
        (Rx(@"إيراد|الإيراد|الإيرادات|مبيعات|المبيعات|قيمة|القيمة|revenue|sales|\bvalue\b"), ConceptRequirement.Of("الإيراد", false, "revenue")),
        (Rx(@"كمية|الكمية|quantity"), ConceptRequirement.Of("الكمية", false, "quantity")),
    };

    private static Regex Rx(string p) => new(p, RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static IReadOnlyList<ConceptRequirement> Resolve(BuiltinDashboardWidgetSpec spec) => Resolve(spec.Title, spec.Prompt, spec.Sql);

    public static IReadOnlyList<ConceptRequirement> Resolve(string title, string? prompt = null, string? draftSql = null)
    {
        var text = title + " " + (prompt ?? "");
        var result = new List<ConceptRequirement>();
        foreach (var (pattern, req) in TitleRules)
            if (pattern.IsMatch(text) && !result.Any(r => r.Label == req.Label)) result.Add(req);

        // عمود التجميع في المسودة إشارة لينة إضافية (الأسماء مولَّدة آليًا فلا تُجعل صلبة).
        var parsed = SchemaConceptExtractor.Parse(draftSql);
        if (parsed.GroupColumn is not null && ConceptCatalog.GuessFromName(parsed.GroupColumn) is { } g
            && !result.Any(r => r.AnyOf.Contains(g.Concept.Id)))
            result.Add(ConceptRequirement.Of(g.Concept.Label, false, g.Concept.Id));
        return result;
    }

    /// <summary>يحسم الجدوى مقابل مجموعة مفاهيم المشروع. مجموعة فارغة (لم يُحلَّل أي جدول بعد) تعني
    /// «غير معروف» فلا يُرفض شيء — القرار يُترك للموديل كما كان.</summary>
    public static WidgetFeasibility Evaluate(IReadOnlyList<ConceptRequirement> requirements, IReadOnlySet<string> available)
    {
        if (available.Count == 0) return WidgetFeasibility.Unknown;
        var missing = new List<string>();
        var satisfied = new List<string>();
        foreach (var r in requirements)
        {
            var hit = r.AnyOf.FirstOrDefault(available.Contains);
            if (hit is not null) satisfied.Add(hit);
            else if (r.Hard) missing.Add(r.Label);
        }
        return new WidgetFeasibility(missing.Count == 0, missing, satisfied);
    }
}

/// <summary>جدوى عناصر القوالب لمشروع معيّن، حتميًا وبلا موديل، من معاني أعمدته المخزّنة.</summary>
public class WidgetFeasibilityService
{
    private const double MinConfidence = 0.4;
    private readonly ColumnSemanticsStore _semantics;

    public WidgetFeasibilityService(ColumnSemanticsStore semantics) => _semantics = semantics;

    /// <summary>المفاهيم المتاحة في المشروع ← الأعمدة الحاملة لها («revenue» ← «staging_sales.Revenue»).</summary>
    public async Task<IReadOnlyDictionary<string, List<(string Table, string Column)>>> AvailableConceptsAsync(string? projectId, CancellationToken ct = default)
    {
        var map = new Dictionary<string, List<(string, string)>>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(projectId)) return map;
        foreach (var s in await _semantics.ListForProjectAsync(projectId, ct))
        {
            if (s.Concept is null || s.Confidence < MinConfidence || !ConceptCatalog.IsKnown(s.Concept)) continue;
            if (!map.TryGetValue(s.Concept, out var list)) map[s.Concept] = list = new();
            list.Add((s.TableName, s.ColumnName));
        }
        return map;
    }

    public async Task<IReadOnlyDictionary<string, WidgetFeasibility>> EvaluateAsync(
        string? projectId, IReadOnlyList<BuiltinDashboardWidgetSpec> widgets, CancellationToken ct = default)
        => Evaluate(await AvailableConceptsAsync(projectId, ct), widgets);

    /// <summary>نفس التقييم على خريطة مفاهيم سبق جلبها (البناء المرحلي يجلبها مرة واحدة للجدوى والتلميحات).</summary>
    public static IReadOnlyDictionary<string, WidgetFeasibility> Evaluate(
        IReadOnlyDictionary<string, List<(string Table, string Column)>> availableMap, IReadOnlyList<BuiltinDashboardWidgetSpec> widgets)
    {
        var available = availableMap.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var result = new Dictionary<string, WidgetFeasibility>(StringComparer.OrdinalIgnoreCase);
        foreach (var w in widgets)
            result.TryAdd(w.Title, WidgetRequirementResolver.Evaluate(WidgetRequirementResolver.Resolve(w), available));
        return result;
    }

    /// <summary>لشارة المعرض: لكل قالب عدد العناصر القابلة للبناء وأكثر المفاهيم نقصًا.</summary>
    public async Task<IReadOnlyList<TemplateFeasibility>> ForCatalogAsync(string? projectId, CancellationToken ct = default)
    {
        var available = (await AvailableConceptsAsync(projectId, ct)).Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var analyzed = available.Count > 0;
        var list = new List<TemplateFeasibility>();
        foreach (var t in BuiltinTemplates.Dashboards)
        {
            if (t.Widgets.Length == 0) continue; // لوحة تفاعلية: جدواها من عقد مزوّدها لا من هنا
            var missing = new Dictionary<string, int>();
            var buildable = 0;
            foreach (var w in t.Widgets)
            {
                var f = WidgetRequirementResolver.Evaluate(WidgetRequirementResolver.Resolve(w), available);
                if (f.Feasible) buildable++;
                foreach (var m in f.Missing) missing[m] = missing.GetValueOrDefault(m) + 1;
            }
            list.Add(new TemplateFeasibility(t.Id, t.Widgets.Length, analyzed ? buildable : (int?)null,
                missing.OrderByDescending(kv => kv.Value).Take(3).Select(kv => kv.Key).ToList()));
        }
        return list;
    }
}

/// <summary><see cref="Buildable"/> null عندما لم يُحلَّل أي جدول في المشروع بعد.</summary>
public sealed record TemplateFeasibility(string TemplateId, int Total, int? Buildable, IReadOnlyList<string> TopMissing);
