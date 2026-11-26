using System.Text.Json;

namespace ChatToDashboard.Api.Users;

/// <summary>
/// صلاحيات الشاشات لكل مستخدم عادي (المسؤول يرى كل شيء دائمًا). <c>AppUser.AllowedScreensJson</c>: null = كل الشاشات، وإلا قائمة
/// مفاتيح من <see cref="Screens"/> (نفس قيم <c>data-screen</c> في الواجهة).
/// الفرض في الخادم بخريطة «بادئة واجهة برمجية ← الشاشات التي تحتاجها»: البادئة تُرفض فقط إن لم تحتجها أي شاشة مسموحة، لأن شاشات
/// كثيرة تتشارك واجهات (تصميم حر يستخدم واجهات اللوحات في «لوحات المتابعة»، والمصادر، وجسر بيانات العروض). البادئة غير المذكورة مسموحة
/// (الدخول، المشاريع الحالية، الإعدادات العامة…)، وواجهات المسؤول تحميها صلاحية الدور أصلًا.
/// </summary>
public static class ScreenAccess
{
    /// <summary>الشاشات التي يمكن تقييد المستخدم العادي بها.</summary>
    public static readonly string[] Screens =
        { "chat", "published", "analyst", "decks", "freeform", "sources", "repo", "users", "history", "active", "sharelinks", "presentations", "projects", "viewing" };

    /// <summary>الاختيار الجاهز «تصميم حر فقط»: الشاشة نفسها و«الملفات» ليرفع بياناته (تعرض ملفاته هو فقط).</summary>
    public static readonly string[] FreeformOnly = { "freeform", "repo" };

    /// <summary>الاختيار الجاهز «اطلاع فقط» (للمديرين): صفحة «للاطلاع» وحدها — ما شورك معه وله رابط منشور يُفتح في تبويب جديد.</summary>
    public static readonly string[] ViewOnly = { "viewing" };

    private static readonly string[] Dashboards = { "chat", "history", "active", "sharelinks", "published", "freeform" };

    /// <summary>بادئة ← الشاشات التي تحتاجها (أي واحدة منها تكفي). الأطول يُطابَق أولًا.</summary>
    private static readonly (string Prefix, string[] Screens)[] Map = new (string, string[])[]
    {
        ("/api/analyst", new[] { "analyst" }),
        ("/api/viewing", new[] { "viewing" }),
        // تصميم حر: كل نداءاته من شاشته (ومنها «لوحات المتابعة» داخلها)؛ أصوله العامة assets/{id} موسومة [AllowAnonymous] فلا تخضع.
        ("/api/freeform", new[] { "freeform" }),
        ("/api/decks/brand", new[] { "decks", "freeform", "chat" }),
        ("/api/decks", new[] { "decks" }),
        ("/api/published-dashboards", new[] { "published", "active" }),
        ("/api/presentations", new[] { "presentations", "freeform", "decks" }),
        ("/api/tokens", new[] { "presentations" }),
        ("/api/chat", Dashboards),
        ("/api/history", Dashboards),
        ("/api/share", Dashboards),
        ("/api/widgets", Dashboards),
        ("/api/interactive", Dashboards),
        ("/api/inquiry", Dashboards),
        ("/api/export", Dashboards),
        ("/api/templates", new[] { "chat", "freeform", "decks" }),
        ("/api/repository", new[] { "repo", "sources", "freeform", "chat", "analyst", "decks" }),
        ("/api/semantics", new[] { "repo", "sources", "freeform", "chat", "decks" }),
        ("/api/sources", new[] { "sources", "chat", "freeform", "analyst", "decks", "repo" }),
        ("/api/users/lookup", new[] { "users", "repo" }),
    }.OrderByDescending(x => x.Item1.Length).ToArray();

    /// <summary>القائمة المحفوظة: null = كل الشاشات؛ تُصفّى إلى المفاتيح المعروفة بلا تكرار.</summary>
    public static List<string>? Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return Normalize(JsonSerializer.Deserialize<List<string>>(json)); } catch (JsonException) { return null; }
    }

    public static List<string>? Normalize(IEnumerable<string>? screens)
        => screens is null ? null : Screens.Where(s => screens.Contains(s, StringComparer.OrdinalIgnoreCase)).ToList();

    /// <summary>ما يُحفظ: null للكل (أو قائمة تشمل كل الشاشات)، وإلا JSON القائمة. قائمة فارغة تُحفظ فارغة (لا شاشات).</summary>
    public static string? Serialize(IEnumerable<string>? screens)
    {
        var list = Normalize(screens);
        if (list is null || list.Count == Screens.Length) return null;
        return JsonSerializer.Serialize(list);
    }

    /// <summary>هل للمسار بادئة مقيَّدة أصلًا (فلا يُقرأ المستخدم من القاعدة لغيرها)؟</summary>
    public static bool IsGated(string path) => Match(path) is not null;

    /// <summary>
    /// هل يخضع هذا الطلب لصلاحيات الشاشات؟ نقطة نهاية عامة ([AllowAnonymous]: الرابط المنشور /p/… ولوحة /d/… وأصولهما) لا تخضع،
    /// لأن الزائر المجهول يصلها أصلًا — وكان تقييدها يرفض (403) صاحب حساب «اطلاع فقط» وهو مسجَّل دخوله، فيرى «الرابط غير موجود».
    /// </summary>
    public static bool AppliesTo(string path, bool isPublicEndpoint) => !isPublicEndpoint && IsGated(path);

    /// <summary>هل يُسمح للمستخدم بهذا المسار؟ <paramref name="allowed"/> null = كل الشاشات.</summary>
    public static bool IsApiAllowed(string path, IReadOnlyCollection<string>? allowed)
    {
        if (allowed is null) return true;
        var screens = Match(path);
        return screens is null || screens.Any(s => allowed.Contains(s, StringComparer.OrdinalIgnoreCase));
    }

    private static string[]? Match(string path)
    {
        foreach (var (prefix, screens) in Map)
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && (path.Length == prefix.Length || path[prefix.Length] == '/')) return screens;
        return null;
    }
}
