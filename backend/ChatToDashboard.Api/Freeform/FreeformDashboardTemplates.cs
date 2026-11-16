using System.Text;
using System.Text.Json;
using ChatToDashboard.Api.Templates;

namespace ChatToDashboard.Api.Freeform;

/// <summary>
/// يعرض كتالوج نماذج اللوحات (الـ8 الأصلية + 212 المولَّدة + متتبع الفرص + اللوحات التفاعلية، وما يضيفه/يعدّله/يخفيه
/// مالك المنصّة من «مكتبة النماذج») داخل «تصميم حر» تحت نوع «لوحة» — بطلب المستخدم: اختيار «لوحة» يتيح أي نموذج من
/// نماذج المحادثة واللوحات. التحويل حتمي: كل عنصر في النموذج يصير قسمًا يحتاج بيانات (باسم صفحته إن تعدّدت الصفحات)،
/// والرسم التخطيطي من أنواع العناصر، والموجز يلزم الموديل بصفحة لوحة واحدة برسوم SVG بالعناوين نفسها وأرقام من
/// <c>query_data</c> (أسماء الجداول في النموذج افتراضية لا تُستخدم). المفتاح <c>d:&lt;id&gt;</c> ولا يُعاد تسميته
/// (يُحفظ في <c>FreeformDesigns.TemplateKey</c>). كتالوج المحادثة نفسه لا يُمس.
/// </summary>
public sealed class FreeformDashboardTemplates
{
    public const string KeyPrefix = "d:";
    private readonly TemplateStore _store;
    private IReadOnlyList<FreeformTemplate>? _cache; private DateTime _cachedAt;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public FreeformDashboardTemplates(TemplateStore store) => _store = store;

    public static bool IsKey(string? key) => key is not null && key.StartsWith(KeyPrefix, StringComparison.Ordinal);

    /// <summary>كل نماذج اللوحات المنشورة كنماذج تصميم حر (مخزَّنة 5 دقائق؛ تعديلات الإدارة تظهر بعدها أو بـ<see cref="Invalidate"/>).</summary>
    public async Task<IReadOnlyList<FreeformTemplate>> ListAsync(CancellationToken ct = default)
    {
        if (_cache is not null && DateTime.UtcNow - _cachedAt < TimeSpan.FromMinutes(5)) return _cache;
        await _gate.WaitAsync(ct);
        try
        {
            if (_cache is not null && DateTime.UtcNow - _cachedAt < TimeSpan.FromMinutes(5)) return _cache;
            var overrides = (await _store.ListByKindAsync(TemplateKinds.Dashboard, ct)).ToDictionary(o => o.Key);
            var list = new List<FreeformTemplate>();
            foreach (var b in BuiltinTemplates.Dashboards)
            {
                var o = overrides.GetValueOrDefault(b.Id);
                if ((o?.Status ?? TemplateStatuses.Published) != TemplateStatuses.Published) continue;
                var widgets = Widgets(o?.WidgetsJson) ?? b.Widgets;
                list.Add(Convert(b.Id, o?.Category ?? b.Category, o?.Title ?? b.Name, o?.Description ?? b.Description, widgets, b.RenderKind));
            }
            foreach (var o in overrides.Values.Where(o => o.IsCustom && o.Status == TemplateStatuses.Published))
                list.Add(Convert(o.Key, o.Category ?? "sales", o.Title ?? o.Key, o.Description ?? "", Widgets(o.WidgetsJson) ?? Array.Empty<BuiltinDashboardWidgetSpec>(), null));
            _cache = list; _cachedAt = DateTime.UtcNow;
            return list;
        }
        finally { _gate.Release(); }
    }

    public async Task<FreeformTemplate?> FindAsync(string? key, CancellationToken ct = default)
        => IsKey(key) ? (await ListAsync(ct)).FirstOrDefault(t => t.Key == key) : null;

    public void Invalidate() => _cache = null;

    private static BuiltinDashboardWidgetSpec[]? Widgets(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<List<BuiltinDashboardWidgetSpec>>(json, TemplatePromptService.JsonOptions)?.ToArray(); }
        catch (JsonException) { return null; }
    }

    /// <summary>تحويل نموذج لوحة واحد (حتمي، مختبَر).</summary>
    public static FreeformTemplate Convert(string id, string category, string name, string description, IReadOnlyList<BuiltinDashboardWidgetSpec> widgets, string? renderKind)
    {
        var interactive = renderKind is not null;
        var pages = widgets.Select(w => w.Page).Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList();
        var multiPage = pages.Count > 1;
        var sections = new List<FreeformTemplateSection>(); var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var w in widgets)
        {
            var title = multiPage && !string.IsNullOrWhiteSpace(w.Page) ? $"{w.Page} — {w.Title}" : w.Title;
            if (string.IsNullOrWhiteSpace(title) || !seen.Add(title)) continue;
            sections.Add(new FreeformTemplateSection(title, true, SectionType(w.Type)));
            if (sections.Count >= 40) break;
        }
        if (sections.Count == 0)
            sections.Add(new FreeformTemplateSection(interactive ? $"{name} (لوحة تفاعلية)" : name, true, interactive ? "interactive" : "kpi"));
        var brief = new StringBuilder();
        brief.Append("نموذج من كتالوج لوحات المتابعة. ابنِ صفحة لوحة واحدة: رأس بعنوان اللوحة وفلاتر (الفترة، وبُعد أو اثنان من بياناتك)، ثم صف بطاقات المؤشرات، ثم الرسوم والجداول بالعناوين نفسها وبنوع كل عنصر كما هو مذكور (");
        brief.Append(string.Join("، ", sections.Where(s => s.Type is not null).GroupBy(s => s.Type!).Select(g => $"{TypeLabel(g.Key)}: {g.Count()}")));
        brief.Append("). كل الأرقام من query_data على جداول المشروع الفعلية — أسماء الجداول والأعمدة في النموذج افتراضية فلا تستخدمها حرفيًا؛ وما لا تجد له بيانات اعرضه توضيحيًا بشارة واضحة أو احذفه. ");
        if (multiPage) brief.Append("اللوحة متعددة الصفحات: اجعل كل صفحة تبويبًا (").Append(string.Join("، ", pages)).Append(") بتنقّل بلا إعادة تحميل. ");
        if (interactive) brief.Append("هذا النموذج لوحة تفاعلية في الأصل (محرك ").Append(renderKind).Append("): اجعل الصفحة تفاعلية (نقر على العناصر يصفّي الباقي، تفاصيل عند التحويم، تحديث فوري) بما يناسب الوصف. ");
        brief.Append("رسوم SVG خفيفة بلا مكتبات خارجية، ألوان حالة ثابتة، بلا رسوم بمحورين، وتستجيب لعرض 390px.");
        return new FreeformTemplate
        {
            Key = KeyPrefix + id, Kind = "dashboard", Category = category, Icon = Icon(category, interactive), Name = name,
            Description = description, Sketch = Sketch(widgets, interactive), Brief = brief.ToString(), Sections = sections, Dashboard = true,
        };
    }

    /// <summary>نوع القسم من نوع العنصر في الكتالوج (الأنواع التوضيحية الـ28 في المرجع مُختزَلة إلى أنواع اللوحة الحقيقية).</summary>
    internal static string SectionType(string? type) => (type ?? "").ToLowerInvariant() switch
    {
        "kpi" or "number" or "stat" => "kpi",
        "line" or "area" or "spark" or "trend" => "line",
        "donut" or "pie" or "ring" => "donut",
        "table" or "progress-table" or "list" => "table",
        "funnel" or "stages" => "funnel",
        "heat" or "heatmap" or "map" => "heat",
        _ => "bar",
    };

    private static string TypeLabel(string t) => t switch
    {
        "kpi" => "مؤشرات", "line" => "خط زمني", "donut" => "دائري", "table" => "جداول", "funnel" => "قمع/مراحل", "heat" => "خريطة حرارية", "interactive" => "تفاعلي", _ => "أعمدة",
    };

    /// <summary>رسم تخطيطي للمصغّرة: صف مؤشرات، ثم صفوف رسوم (اثنان في الصف)، ثم جدول — حتى 4 صفوف.</summary>
    internal static string Sketch(IReadOnlyList<BuiltinDashboardWidgetSpec> widgets, bool interactive)
    {
        if (interactive && widgets.Count == 0) return "h/g/c,t";
        var types = widgets.Select(w => SectionType(w.Type)).ToList();
        var rows = new List<string> { "h" };
        var kpis = types.Count(t => t == "kpi");
        if (kpis > 0) rows.Add("k" + Math.Clamp(kpis, 2, 4));
        var charts = types.Where(t => t is "line" or "donut" or "bar" or "funnel" or "heat").Select(t => t switch { "line" => "l", "donut" => "d", _ => "c" }).ToList();
        for (var i = 0; i < charts.Count && rows.Count < 4; i += 2) rows.Add(charts.Count > i + 1 ? $"{charts[i]},{charts[i + 1]}" : charts[i]);
        if (types.Contains("table") && rows.Count < 5) rows.Add("t");
        if (rows.Count == 1) rows.Add("c,t");
        return string.Join("/", rows);
    }

    private static string Icon(string category, bool interactive) => interactive ? "🕸️" : category switch
    {
        "sales" => "💼", "projects" => "🗂️", "finance" => "💰", "hr" => "👥", "procurement" => "🛒", "cx" => "🎧", "ops" => "⚙️",
        "retail" => "🏬", "banking" => "🏦", "supply" => "🚚", "hse" => "🦺", "saas" => "☁️", "health" => "🏥", "hospitality" => "🏨",
        "gov" => "🏛️", "marketing" => "📣", "ads" => "📢", "social" => "💬", "dev" => "🧑‍💻", _ => "📊",
    };
}
