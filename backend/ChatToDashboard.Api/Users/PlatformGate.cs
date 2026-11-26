namespace ChatToDashboard.Api.Users;

/// <summary>
/// بوابة «مالك المنصّة بلا منظمة»: حساب مالك المنصّة المستقل (<see cref="AppUser.IsPlatformOwner"/> بلا
/// <see cref="AppUser.OrganizationId"/>) يدير المنصّة فقط — المنظمات ومسؤوليها، وكتالوج النماذج ومكتبة الأسماء،
/// وإعدادات الموديلات ومفاتيحها — ولا يصل إلى بيانات أي منظمة. كل واجهة /api غير عامة خارج هذه القائمة تُرفض له
/// بـ 403 (<see cref="Code"/>)، لأن كثيرًا من المتحكمات تعامل المنظمة الفارغة بلا تصفية.
/// </summary>
public static class PlatformGate
{
    private static readonly string[] Allowed =
    {
        "/api/auth", "/api/organizations", "/api/templates", "/api/schema-aliases", "/api/llm-settings",
    };

    public const string Code = "platform_only";
    public const string Message = "حساب مالك المنصّة يدير المنصّة فقط ولا يصل إلى بيانات المنظمات — ادخل بحساب من المنظمة.";

    /// <summary>هل الحساب مالك منصّة مستقل بلا منظمة؟ (من مطالبات الجلسة)</summary>
    public static bool IsStandaloneOwner(bool isPlatformOwner, string? organizationId)
        => isPlatformOwner && string.IsNullOrEmpty(organizationId);

    /// <summary>هل يُرفض هذا المسار لمالك المنصّة المستقل؟ (واجهة /api غير عامة وليست من المسموحة)</summary>
    public static bool Blocks(string path, bool isPublicEndpoint)
    {
        if (isPublicEndpoint || !path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase)) return false;
        foreach (var p in Allowed)
            if (path.Equals(p, StringComparison.OrdinalIgnoreCase) || path.StartsWith(p + "/", StringComparison.OrdinalIgnoreCase)) return false;
        return true;
    }
}
