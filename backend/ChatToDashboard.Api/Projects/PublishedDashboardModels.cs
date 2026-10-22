using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatToDashboard.Api.Projects;

/// <summary>
/// صف واحد من جدول <c>PublishedDashboards</c> (§4.23): لوحة متابعة منشورة على شاشة
/// «اللوحات المنشورة» داخل مشروع. جدول مستقل تمامًا عن <c>PublishedDashboardSlots</c>
/// الخاص بالنشر إلى تكامل خارجي — ذاك ينسخ اللوحة إلى نظام عميل، وهذا يعرضها لأعضاء
/// المشروع نفسه حسب جمهور النشر ودور كل عضو.
/// </summary>
public class PublishedDashboard
{
    public string Id { get; set; } = "";
    public string ProjectId { get; set; } = "";

    /// <summary>معرّف اللوحة في سجل اللوحات (<c>DashboardHistory.Id</c>).</summary>
    public string DashboardId { get; set; } = "";

    /// <summary>يبدأ من 1 ويزيد مع كل إعادة نشر لنفس اللوحة على نفس المشروع.</summary>
    public int Version { get; set; }

    /// <summary>معرّف المستخدم الذي نشر آخر إصدار.</summary>
    public string PublishedBy { get; set; } = "";
    public DateTime PublishedAt { get; set; }

    /// <summary>أحد قيم <see cref="PublishedAudiences"/>.</summary>
    public string Audience { get; set; } = PublishedAudiences.All;

    /// <summary>"live" أو "scheduled" — انظر <see cref="PublishedRefreshModes"/>.</summary>
    public string RefreshMode { get; set; } = PublishedRefreshModes.Live;

    /// <summary>وصف تكرار التحديث كما يُعرض (مثل «كل ساعة» أو «يوميًا 6:00 ص»). اختياري.</summary>
    public string? RefreshEvery { get; set; }

    /// <summary>ترتيب التبويب على مستوى المشروع — يحدده المالك أو المحرر.</summary>
    public int SortOrder { get; set; }

    /// <summary>"jr" للوحة مبنية من المحادثة، "interactive" للوحة تفاعلية — يُشتق عند
    /// النشر من <c>DashboardHistory.RenderKind</c>.</summary>
    public string Kind { get; set; } = PublishedKinds.Jr;
}

/// <summary>
/// جمهور النشر — مفردات ثابتة مربوطة بأدوار المشروع (مالك/محرر/مشاهد) لأن الجمهور هو ما
/// يحدد فعلًا من يرى اللوحة، فلا يصح أن يكون نصًا حرًا لا يمكن التحقق منه.
/// </summary>
public static class PublishedAudiences
{
    /// <summary>كل أعضاء المشروع، بمن فيهم المشاهدون.</summary>
    public const string All = "all";

    /// <summary>المالكون والمحررون فقط.</summary>
    public const string Editors = "editors";

    /// <summary>مالكو المشروع فقط.</summary>
    public const string Owners = "owners";

    public static readonly IReadOnlyList<string> Values = new[] { All, Editors, Owners };

    public static string Label(string audience) => audience switch
    {
        Editors => "المالكون والمحررون",
        Owners => "مالكو المشروع",
        _ => "كل أعضاء المشروع",
    };
}

public static class PublishedRefreshModes
{
    public const string Live = "live";
    public const string Scheduled = "scheduled";
}

public static class PublishedKinds
{
    public const string Jr = "jr";
    public const string Interactive = "interactive";
}

/// <summary>
/// القواعد الصافية (بلا قاعدة بيانات) لشاشة اللوحات المنشورة — مفصولة هنا لتُختبر وحدها.
/// </summary>
public static class PublishedDashboardRules
{
    /// <summary>
    /// هل يرى صاحب هذا الدور في المشروع لوحة منشورة لهذا الجمهور؟ مسؤول المنظمة يرى كل
    /// شيء دائمًا. الدور الفارغ (حساب بلا دور صريح في المشروع) يُعامل كمشاهد — أقل صلاحية.
    /// </summary>
    public static bool CanView(string audience, string? projectRole, bool isAdmin)
    {
        if (isAdmin) return true;
        var role = string.IsNullOrEmpty(projectRole) ? ProjectRoles.Viewer : projectRole;
        return audience switch
        {
            PublishedAudiences.Owners => role == ProjectRoles.Owner,
            PublishedAudiences.Editors => role is ProjectRoles.Owner or ProjectRoles.Editor,
            _ => true,
        };
    }

    /// <summary>النشر وإعادة الترتيب وإلغاء النشر للمالك والمحرر (ومسؤول المنظمة) فقط.</summary>
    public static bool CanManage(string? projectRole, bool isAdmin) =>
        isAdmin || projectRole is ProjectRoles.Owner or ProjectRoles.Editor;

    /// <summary>
    /// أسماء صفحات اللوحة من عناصرها، بنفس منطق <c>getDashboardPages</c> في app.js: قيم
    /// <c>page</c> المميزة بترتيب أول ظهور، وتُضاف «عناصر أخرى» إن وُجدت عناصر بلا صفحة
    /// بجانب عناصر لها صفحات. لوحة بصفحة واحدة (أو بلا صفحات) تُعيد قائمة فارغة.
    /// </summary>
    public static IReadOnlyList<string> PagesFromWidgets(string? widgetsJson)
    {
        var pages = new List<string>();
        var hasOther = false;
        try
        {
            var root = JsonDocument.Parse(string.IsNullOrWhiteSpace(widgetsJson) ? "[]" : widgetsJson).RootElement;
            if (root.ValueKind != JsonValueKind.Array) return pages;
            foreach (var w in root.EnumerateArray())
            {
                if (w.ValueKind == JsonValueKind.Object && w.TryGetProperty("page", out var p)
                    && p.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(p.GetString()))
                {
                    var name = p.GetString()!;
                    if (!pages.Contains(name)) pages.Add(name);
                }
                else hasOther = true;
            }
        }
        catch (JsonException)
        {
            return new List<string>();
        }
        if (pages.Count <= 1) return new List<string>();
        if (hasOther) pages.Add("عناصر أخرى");
        return pages;
    }

    /// <summary>
    /// يطبّق ترتيبًا جديدًا مرسلًا من الواجهة: المعرّفات المرسلة أولًا بترتيبها، ثم أي لوحة
    /// لم تُذكر (مثلًا لوحة لا يراها المرسِل بسبب جمهورها) بترتيبها السابق — فلا يضيع موضع
    /// لوحة لم يكن المرسِل يراها أصلًا. معرّفات مجهولة أو مكررة تُتجاهل.
    /// </summary>
    public static IReadOnlyList<string> ApplyOrder(IReadOnlyList<string> currentOrder, IEnumerable<string> requested)
    {
        var known = new HashSet<string>(currentOrder);
        var result = new List<string>();
        foreach (var id in requested)
            if (known.Contains(id) && !result.Contains(id)) result.Add(id);
        foreach (var id in currentOrder)
            if (!result.Contains(id)) result.Add(id);
        return result;
    }
}

/// <summary>جسم <c>POST /api/published-dashboards</c> — نشر لوحة أو إعادة نشرها.</summary>
public class PublishDashboardRequest
{
    [JsonPropertyName("dashboardId")] public string DashboardId { get; set; } = "";
    [JsonPropertyName("audience")] public string? Audience { get; set; }
    [JsonPropertyName("refreshMode")] public string? RefreshMode { get; set; }
    [JsonPropertyName("refreshEvery")] public string? RefreshEvery { get; set; }
}

/// <summary>جسم <c>PUT /api/published-dashboards/order</c>.</summary>
public class ReorderPublishedRequest
{
    [JsonPropertyName("ids")] public List<string> Ids { get; set; } = new();
}
