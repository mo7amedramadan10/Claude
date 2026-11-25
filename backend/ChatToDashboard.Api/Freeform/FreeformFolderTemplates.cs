using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ChatToDashboard.Api.Freeform;

/// <summary>
/// نماذج تصميم حر الجاهزة المسلَّمة مجلداتٍ (العقد في <c>docs/freeform-template-spec.md</c>):
/// <c>wwwroot/templates/freeform/&lt;key&gt;/</c> فيه <c>template.json</c> و<c>ar.html</c> و<c>en.html</c> و<c>assets/</c>.
/// تُقرأ مرة عند الإقلاع وتُسجَّل في <see cref="FreeformTemplates.All"/> بلا أي تعديل في الكود؛ والصور تخدمها
/// الملفات الثابتة على <c>/templates/freeform/&lt;key&gt;/assets/…</c>. المجلد المخالف يُتخطّى بسبب مسجَّل ولا يوقف الباقي.
/// </summary>
public static class FreeformFolderTemplates
{
    public const string RelativeRoot = "templates/freeform";
    private static readonly HashSet<string> Kinds = new(StringComparer.Ordinal) { "slides", "doc", "design", "dashboard", "motion" };
    private static readonly HashSet<string> Categories = new(StringComparer.Ordinal) { "mgmt", "sales", "projects", "finance", "hr", "marketing", "ops", "cx", "general", "thiqah" };
    private static readonly Regex KeyRx = new("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);
    private static readonly Regex SlideRx = new("<section[^>]*class=\"[^\"]*\\bslide\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>الموجز الافتراضي حين يغيب <c>brief</c> من الملف.</summary>
    public const string DefaultBrief = "صفحة جاهزة بتصميم ثابت: عند أي تعديل أعد الصفحة كاملة محافظًا على البنية والأنماط والصور وغيّر النصوص والأرقام فقط؛ أرقام الرسوم في كتل JSON بالمعرّفات chart-data-*.";

    private sealed record Manifest(
        [property: JsonPropertyName("key")] string? Key,
        [property: JsonPropertyName("kind")] string? Kind,
        [property: JsonPropertyName("category")] string? Category,
        [property: JsonPropertyName("icon")] string? Icon,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("nameEn")] string? NameEn,
        [property: JsonPropertyName("description")] string? Description,
        [property: JsonPropertyName("descriptionEn")] string? DescriptionEn,
        [property: JsonPropertyName("sections")] List<ManifestSection>? Sections,
        [property: JsonPropertyName("brief")] string? Brief);

    private sealed record ManifestSection(
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("titleEn")] string? TitleEn,
        [property: JsonPropertyName("needsData")] bool NeedsData);

    /// <summary>نتيجة القراءة: النماذج المقبولة، وسبب رفض كل مجلد مخالف.</summary>
    public sealed record LoadResult(IReadOnlyList<FreeformTemplate> Templates, IReadOnlyList<(string Folder, string Reason)> Rejected);

    /// <summary>يقرأ كل المجلدات تحت <c>&lt;webRoot&gt;/templates/freeform</c> مرتبة بالاسم.</summary>
    public static LoadResult Load(string? webRoot)
    {
        var ok = new List<FreeformTemplate>(); var bad = new List<(string, string)>();
        if (string.IsNullOrWhiteSpace(webRoot)) return new(ok, bad);
        var root = Path.Combine(webRoot, RelativeRoot);
        if (!Directory.Exists(root)) return new(ok, bad);
        foreach (var dir in Directory.GetDirectories(root).OrderBy(d => d, StringComparer.Ordinal))
        {
            var folder = Path.GetFileName(dir);
            if (!File.Exists(Path.Combine(dir, "template.json"))) continue; // مجلد مساعد لا نموذج
            try
            {
                var (t, reason) = Read(dir, folder);
                if (t is null) bad.Add((folder, reason!)); else ok.Add(t);
            }
            catch (Exception ex) { bad.Add((folder, ex.Message)); }
        }
        return new(ok, bad);
    }

    private static (FreeformTemplate?, string?) Read(string dir, string folder)
    {
        Manifest? m;
        try { m = JsonSerializer.Deserialize<Manifest>(File.ReadAllText(Path.Combine(dir, "template.json"))); }
        catch (JsonException ex) { return (null, "template.json غير صالح: " + ex.Message); }
        if (m is null) return (null, "template.json فارغ");
        if (m.Key != folder) return (null, $"المفتاح «{m.Key}» لا يطابق اسم المجلد");
        if (!KeyRx.IsMatch(folder)) return (null, "المفتاح يجب أن يكون أحرفًا لاتينية صغيرة وأرقامًا وشرطات");
        if (m.Kind is null || !Kinds.Contains(m.Kind)) return (null, $"نوع غير معروف «{m.Kind}»");
        if (m.Category is null || !Categories.Contains(m.Category)) return (null, $"تصنيف غير معروف «{m.Category}»");
        if (string.IsNullOrWhiteSpace(m.Name)) return (null, "بلا name");
        if (m.Sections is not { Count: > 0 } || m.Sections.Any(s => string.IsNullOrWhiteSpace(s.Title))) return (null, "sections فارغة أو قسم بلا عنوان");
        var arPath = Path.Combine(dir, "ar.html");
        if (!File.Exists(arPath)) return (null, "ar.html غير موجود");
        if (m.Kind == "slides")
        {
            var n = SlideRx.Matches(File.ReadAllText(arPath)).Count;
            if (n != m.Sections.Count) return (null, $"ar.html فيه {n} شرائح والأقسام {m.Sections.Count}");
        }
        var hasEn = File.Exists(Path.Combine(dir, "en.html"));
        var rel = $"{RelativeRoot}/{folder}";
        return (new FreeformTemplate
        {
            Key = folder, Kind = m.Kind, Category = m.Category,
            Icon = string.IsNullOrWhiteSpace(m.Icon) ? "📄" : m.Icon.Trim(),
            Name = m.Name.Trim(), Description = (m.Description ?? "").Trim(),
            NameEn = Blank(m.NameEn), DescriptionEn = Blank(m.DescriptionEn),
            Sketch = string.Join('/', Enumerable.Repeat(m.Kind == "slides" ? "s" : "h", Math.Min(m.Sections.Count, 4))),
            Brief = string.IsNullOrWhiteSpace(m.Brief) ? DefaultBrief : m.Brief.Trim(),
            Page = rel + "/ar.html", PageEn = hasEn ? rel + "/en.html" : null,
            Sections = m.Sections.Select(s => new FreeformTemplateSection(s.Title!.Trim(), s.NeedsData) { TitleEn = Blank(s.TitleEn) }).ToList(),
        }, null);
    }

    private static string? Blank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
