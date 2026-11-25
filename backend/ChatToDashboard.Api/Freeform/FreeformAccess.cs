namespace ChatToDashboard.Api.Freeform;

/// <summary>
/// قواعد الوصول إلى تصميم حر (دالة نقية مختبَرة): صاحبه والمسؤول في منظمته «مالك» (تعديل ومشاركة وحذف ونشر)؛ ثم أعلى دور من
/// (المشاركة بالاسم، الوصول العام على مستوى المنظمة، الوصول العام على مستوى المشروع حين يكون مشروع المستخدم الحالي هو مشروع التصميم)؛
/// وإلا لا وصول. التعديل يشمل البناء بالموديل والتحديث والاستيراد والنشر؛ المشاهدة للقراءة فقط.
/// </summary>
public static class FreeformAccess
{
    public const string Private = "private", Entity = "project", Org = "org";
    public const string View = "view", Edit = "edit", Owner = "owner";
    public static readonly string[] Scopes = { Private, Entity, Org };
    public static readonly string[] Roles = { View, Edit };

    public static string NormalizeScope(string? s) => Scopes.Contains(s ?? "") ? s! : Private;
    public static string NormalizeRole(string? r) => r == Edit ? Edit : View;

    /// <summary>دور المستخدم في التصميم: owner | edit | view | null (لا وصول).</summary>
    /// <param name="sharedRole">دوره إن شورك معه التصميم بالاسم، وإلا null.</param>
    public static string? Resolve(FreeformDesign d, string userId, string? organizationId, string? currentEntityId, bool isAdmin, string? sharedRole)
    {
        // تصميم من منظمة أخرى لا يُرى أبدًا؛ تصميم قديم بلا منظمة يُعامل بمشروعه
        var sameOrg = d.OrganizationId is not null ? string.Equals(d.OrganizationId, organizationId, StringComparison.Ordinal) : SameEntity(d, currentEntityId);
        if (!sameOrg) return null;
        if (isAdmin || string.Equals(d.CreatedByUserId, userId, StringComparison.OrdinalIgnoreCase)) return Owner;
        string? best = sharedRole is null ? null : NormalizeRole(sharedRole);
        var scope = NormalizeScope(d.ShareScope);
        if (scope == Org || (scope == Entity && SameEntity(d, currentEntityId))) best = Max(best, NormalizeRole(d.ShareScopeRole));
        return best;
    }

    public static bool CanEdit(string? access) => access is Owner or Edit;

    private static bool SameEntity(FreeformDesign d, string? currentEntityId) => string.Equals(d.EntityId ?? "", currentEntityId ?? "", StringComparison.Ordinal);
    private static string? Max(string? a, string b) => a == Edit || b == Edit ? Edit : View;
}
