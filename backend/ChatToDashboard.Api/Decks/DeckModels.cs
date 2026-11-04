using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatToDashboard.Api.Decks;

/// <summary>
/// مواصفة عرض تقديمي: الموديل يولّد هذا الـ JSON (لا HTML حرًّا)، ومحرك العرض `jeem-deck.js` يرسمه بهوية جيم في
/// المنصة وفي الصفحة المنشورة نفسها. التخطيطات ثابتة؛ البيانات (أرقام/رسوم/جداول) من استعلامات المشروع.
/// </summary>
public class DeckSpec
{
    public const int MaxSlides = 30;
    public static readonly string[] Layouts = { "title", "section", "bullets", "two-column", "kpis", "chart", "table", "image", "tasks", "quote" };

    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("subtitle")] public string? Subtitle { get; set; }
    [JsonPropertyName("accent")] public string? Accent { get; set; }
    /// <summary>هوية المشروع المرفقة عند الرسم/النشر/التصدير (من <see cref="DeckBrandStore"/>؛ لا تُحفظ داخل العرض).</summary>
    [JsonPropertyName("brand")] public DeckBrand? Brand { get; set; }
    [JsonPropertyName("slides")] public List<DeckSlide> Slides { get; set; } = new();

    /// <summary>تطبيع: معرّفات فريدة، تخطيط معروف (وإلا bullets)، حدود الحجم، وإزالة الحقول الفارغة.</summary>
    public DeckSpec Normalize()
    {
        Title = (Title ?? "").Trim(); if (Title.Length > 200) Title = Title[..200];
        Subtitle = string.IsNullOrWhiteSpace(Subtitle) ? null : Subtitle.Trim();
        Accent = Accent is { Length: 7 } a && a[0] == '#' && a[1..].All(Uri.IsHexDigit) ? a : null;
        Slides ??= new();
        if (Slides.Count > MaxSlides) Slides = Slides.Take(MaxSlides).ToList();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var s in Slides)
        {
            if (string.IsNullOrWhiteSpace(s.Id) || !seen.Add(s.Id)) { s.Id = NewId(); seen.Add(s.Id); }
            s.Layout = Layouts.Contains(s.Layout ?? "") ? s.Layout! : InferLayout(s);
            s.Title = (s.Title ?? "").Trim();
            s.Bullets = s.Bullets?.Where(b => !string.IsNullOrWhiteSpace(b)).Select(b => b.Trim()).Take(12).ToList();
            if (s.Bullets is { Count: 0 }) s.Bullets = null;
            s.Columns = s.Columns?.Take(3).ToList();
            s.Kpis = s.Kpis?.Take(6).ToList();
            if (s.Table is { } t) { t.Columns ??= new(); t.Rows = (t.Rows ?? new()).Take(40).ToList(); }
            if (s.Chart is { } c) { c.Type = c.Type is "bar" or "line" or "pie" or "column" ? c.Type : "bar"; c.Labels ??= new(); c.Values ??= new(); }
            s.Tasks = s.Tasks?.Where(t => !string.IsNullOrWhiteSpace(t.T)).Take(30).ToList(); if (s.Tasks is { Count: 0 }) s.Tasks = null;
            if (s.Editable is { } e) { e.Kind = e.Kind is "tasks" or "text" ? e.Kind : "text"; e.Key = string.IsNullOrWhiteSpace(e.Key) ? "slide-" + s.Id : e.Key; }
        }
        return this;
    }

    private static string InferLayout(DeckSlide s) =>
        s.Chart is not null ? "chart" : s.Table is not null ? "table" : s.Kpis is { Count: > 0 } ? "kpis"
        : s.Columns is { Count: > 0 } ? "two-column" : s.Image is not null ? "image" : s.Editable?.Kind == "tasks" ? "tasks" : "bullets";

    public static string NewId() => Guid.NewGuid().ToString("N")[..8];

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, PropertyNameCaseInsensitive = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
}

public class DeckSlide
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    /// <summary>title | section | bullets | two-column | kpis | chart | table | image | tasks | quote</summary>
    [JsonPropertyName("layout")] public string? Layout { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("subtitle")] public string? Subtitle { get; set; }
    [JsonPropertyName("bullets")] public List<string>? Bullets { get; set; }
    [JsonPropertyName("columns")] public List<DeckColumn>? Columns { get; set; }
    [JsonPropertyName("kpis")] public List<DeckKpi>? Kpis { get; set; }
    [JsonPropertyName("chart")] public DeckChart? Chart { get; set; }
    [JsonPropertyName("table")] public DeckTable? Table { get; set; }
    [JsonPropertyName("image")] public DeckImage? Image { get; set; }
    [JsonPropertyName("quote")] public string? Quote { get; set; }
    [JsonPropertyName("notes")] public string? Notes { get; set; }
    /// <summary>مصدر الأرقام (جدول/استعلام) كما في عناصر اللوحات — للشفافية.</summary>
    [JsonPropertyName("source")] public string? Source { get; set; }
    [JsonPropertyName("query")] public DeckQuery? Query { get; set; }
    /// <summary>كتلة قابلة للتعديل من الجمهور عبر «بيانات العرض» (jeem.store) بعد النشر.</summary>
    [JsonPropertyName("editable")] public DeckEditable? Editable { get; set; }
    /// <summary>المهام الأولية لشريحة tasks (يقرؤها المحرك `slide.tasks`؛ بعد النشر تُحفظ في بيانات العرض).</summary>
    [JsonPropertyName("tasks")] public List<DeckTask>? Tasks { get; set; }
    /// <summary>من مرحلة التخطيط: تحتاج بيانات من المشروع وما المطلوب تحديدًا (يُحذف بعد التعبئة).</summary>
    [JsonPropertyName("needsData")] public bool? NeedsData { get; set; }
    [JsonPropertyName("intent")] public string? Intent { get; set; }
    [JsonPropertyName("missingReason")] public string? MissingReason { get; set; }
}

public class DeckColumn { [JsonPropertyName("title")] public string? Title { get; set; } [JsonPropertyName("bullets")] public List<string> Bullets { get; set; } = new(); }
public class DeckKpi { [JsonPropertyName("label")] public string Label { get; set; } = ""; [JsonPropertyName("value")] public string Value { get; set; } = ""; [JsonPropertyName("delta")] public string? Delta { get; set; } [JsonPropertyName("tone")] public string? Tone { get; set; } }
public class DeckChart
{
    /// <summary>bar | column | line | pie</summary>
    [JsonPropertyName("type")] public string Type { get; set; } = "bar";
    [JsonPropertyName("labels")] public List<string> Labels { get; set; } = new();
    [JsonPropertyName("values")] public List<double> Values { get; set; } = new();
    [JsonPropertyName("series")] public List<DeckSeries>? Series { get; set; }
    [JsonPropertyName("unit")] public string? Unit { get; set; }
}
public class DeckSeries { [JsonPropertyName("name")] public string Name { get; set; } = ""; [JsonPropertyName("values")] public List<double> Values { get; set; } = new(); }
public class DeckTable { [JsonPropertyName("columns")] public List<string> Columns { get; set; } = new(); [JsonPropertyName("rows")] public List<List<string>> Rows { get; set; } = new(); }
public class DeckImage { [JsonPropertyName("src")] public string Src { get; set; } = ""; [JsonPropertyName("caption")] public string? Caption { get; set; } }
public class DeckQuery { [JsonPropertyName("table")] public string? Table { get; set; } [JsonPropertyName("sql")] public string? Sql { get; set; } }
public class DeckTask { [JsonPropertyName("t")] public string T { get; set; } = ""; [JsonPropertyName("done")] public bool Done { get; set; } }
public class DeckEditable { [JsonPropertyName("key")] public string Key { get; set; } = ""; [JsonPropertyName("kind")] public string Kind { get; set; } = "text"; }

/// <summary>سجل عرض محفوظ لمشروع (المواصفة كاملة في SpecJson).</summary>
public class DeckRecord
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("projectId")] public string? ProjectId { get; set; }
    [JsonPropertyName("organizationId")] public string? OrganizationId { get; set; }
    [JsonPropertyName("createdByUserId")] public string CreatedByUserId { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonIgnore] public string SpecJson { get; set; } = "{}";
    [JsonPropertyName("templateKey")] public string? TemplateKey { get; set; }
    /// <summary>معرّف العرض المنشور في «العروض التقديمية» (على مستوى المنظمة) إن نُشر.</summary>
    [JsonPropertyName("presentationId")] public string? PresentationId { get; set; }
    [JsonPropertyName("slideCount")] public int SlideCount { get; set; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updatedAt")] public DateTime UpdatedAt { get; set; }
}
