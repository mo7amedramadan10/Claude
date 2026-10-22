using System.Text.Json.Serialization;

namespace ChatToDashboard.Api.Presentations;

/// <summary>
/// عرض تقديمي: صفحة HTML واحدة مرفوعة كما هي (مثل عرض أعدّه المستخدم بأداة خارجية) تُعرض على رابط
/// عام <c>/p/{id}</c> بلا تسجيل دخول داخل iframe معزول. على مستوى المنظمة (لا المشروع): كل مستخدمي
/// المنظمة يرونه في قائمة «روابط المشاركة»، ويلغيه أو يحذفه صاحبه أو المسؤول.
/// </summary>
public class Presentation
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("organizationId")] public string? OrganizationId { get; set; }
    [JsonPropertyName("createdByUserId")] public string CreatedByUserId { get; set; } = "";
    [JsonPropertyName("createdByName")] public string? CreatedByName { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    /// <summary>الاسم في الرابط (`/p/{org}/{slug}`) — فريد داخل المنظمة كاسم المستودع في GitHub؛ null للعروض
    /// القديمة التي تبقى على `/p/{id}`.</summary>
    [JsonPropertyName("slug")] public string? Slug { get; set; }
    [JsonPropertyName("fileName")] public string FileName { get; set; } = "";
    /// <summary>محتوى الصفحة كاملًا — لا يُرجَع في القوائم، فقط في مسار المحتوى العام.</summary>
    [JsonIgnore] public string Html { get; set; } = "";
    [JsonPropertyName("sizeBytes")] public long SizeBytes { get; set; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updatedAt")] public DateTime UpdatedAt { get; set; }
    /// <summary>null = بلا انتهاء.</summary>
    [JsonPropertyName("expiresAt")] public DateTime? ExpiresAt { get; set; }
    [JsonPropertyName("revokedAt")] public DateTime? RevokedAt { get; set; }
    [JsonPropertyName("viewCount")] public int ViewCount { get; set; }

    /// <summary>المسار العام النسبي: بالاسم إن وُجد وإلا بالمعرّف.</summary>
    public string PublicPath(string? orgSlug) =>
        // الاسمان حروف وأرقام وشرطات فقط، فيُتركان بلا ترميز ليبقى الرابط العربي مقروءًا عند نسخه.
        Slug is { Length: > 0 } && orgSlug is { Length: > 0 } ? $"/p/{orgSlug}/{Slug}" : $"/p/{Id}";

    [JsonPropertyName("isActive")] public bool IsActive => RevokedAt is null && (ExpiresAt is null || ExpiresAt > DateTime.UtcNow);
}

/// <summary>تطبيع اسم الرابط: حروف (بأي لغة) وأرقام وشرطات فقط، صغيرة، بلا شرطات متكررة، حتى 80 حرفًا.</summary>
public static class PresentationSlug
{
    public const int MaxLength = 80;

    public static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";
        var cleaned = new string(input.Trim().ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray());
        while (cleaned.Contains("--")) cleaned = cleaned.Replace("--", "-");
        cleaned = cleaned.Trim('-');
        return cleaned.Length > MaxLength ? cleaned[..MaxLength].Trim('-') : cleaned;
    }

    public static bool IsValid(string slug) => slug.Length is >= 2 and <= MaxLength && slug == Normalize(slug);
}
