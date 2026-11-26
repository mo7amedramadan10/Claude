namespace ChatToDashboard.Api.Entities;

/// <summary>
/// بوابة «بلا كيان»: المستخدم العادي الذي ليس له دور في أي كيان لا يعمل في أي كيان (لا يُسند إلى أول كيان في المنظمة
/// كما كان). فكل واجهة مقيَّدة بالكيان تُرفض له (403 بالرمز <c>no_entity</c>)، ولا يبقى له إلا ما لا يتعلق بكيان:
/// الدخول، وقائمة الكيانات (فارغة له)، وأنواعها، وكتالوج النماذج، والعروض التقديمية على مستوى المنظمة.
/// الواجهات العامة (<c>[AllowAnonymous]</c>: الروابط المنشورة) لا تخضع لها، والمسؤول لا يخضع لها.
/// السبب: <see cref="EntityStore.ResolveCurrentEntityIdAsync"/> تعيد null لهذا المستخدم، وكثير من المخازن تعامل null
/// بلا تصفية — فالبوابة هي التي تمنع وصوله إلى بيانات كل الكيانات.
/// </summary>
public static class EntityGate
{
    private static readonly string[] Exempt =
    {
        "/api/auth", "/api/entities", "/api/projects", "/api/entity-types", "/api/templates", "/api/presentations",
        "/api/organizations",
    };

    public const string Code = "no_entity";
    public const string Message = "لم تُضف إلى أي كيان بعد — اطلب من مسؤول المنظمة إضافتك إلى كيان.";

    /// <summary>هل يخضع هذا المسار للبوابة؟ (واجهة /api غير عامة وليست من المستثناة)</summary>
    public static bool AppliesTo(string path, bool isPublicEndpoint)
    {
        if (isPublicEndpoint || !path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase)) return false;
        foreach (var p in Exempt)
            if (path.Equals(p, StringComparison.OrdinalIgnoreCase) || path.StartsWith(p + "/", StringComparison.OrdinalIgnoreCase)) return false;
        return true;
    }
}
