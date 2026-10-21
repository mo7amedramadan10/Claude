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

    [JsonPropertyName("isActive")] public bool IsActive => RevokedAt is null && (ExpiresAt is null || ExpiresAt > DateTime.UtcNow);
}
