using ChatToDashboard.Api.Entities;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>أنواع الكيانات: تطبيع الأسماء، الجنس، والنوع الفعلي للكيان.</summary>
public class EntityTypeTests
{
    [Fact]
    public void Validate_StripsArticle_DerivesPluralAndGender()
    {
        var t = new EntityType();
        Assert.Null(EntityTypeStore.Validate(new EntityTypeRequest { Singular = " الإدارة " }, t, creating: true));
        Assert.Equal("إدارة", t.Singular);
        Assert.Equal("إدارة", t.Plural); // بلا جمع ← المفرد (تعدّله الواجهة)
        Assert.True(t.Feminine);
        Assert.Equal("layers", t.Icon);

        var q = new EntityType();
        Assert.Null(EntityTypeStore.Validate(new EntityTypeRequest { Singular = "قسم", Plural = "الأقسام", Icon = "briefcase" }, q, creating: true));
        Assert.Equal(("قسم", "أقسام", false, "briefcase"), (q.Singular, q.Plural, q.Feminine, q.Icon));

        Assert.NotNull(EntityTypeStore.Validate(new EntityTypeRequest { Singular = "  " }, new EntityType(), creating: true));
        Assert.NotNull(EntityTypeStore.Validate(new EntityTypeRequest { Singular = new string('س', 41) }, new EntityType(), creating: true));
        var bad = new EntityType();
        EntityTypeStore.Validate(new EntityTypeRequest { Singular = "فريق", Icon = "<script>" }, bad, creating: true);
        Assert.Equal("layers", bad.Icon);
    }

    [Fact]
    public void Validate_Update_KeepsUnsentFields_AndExplicitGenderWins()
    {
        var t = new EntityType { Singular = "قسم", Plural = "أقسام", Icon = "users", Feminine = false };
        Assert.Null(EntityTypeStore.Validate(new EntityTypeRequest { Plural = "الأقسام الإدارية" }, t, creating: false));
        Assert.Equal(("قسم", "أقسام الإدارية", "users", false), (t.Singular, t.Plural, t.Icon, t.Feminine));
        Assert.Null(EntityTypeStore.Validate(new EntityTypeRequest { Feminine = true }, t, creating: false));
        Assert.True(t.Feminine);
    }

    [Fact]
    public void Resolve_ExplicitTypeOrDefault()
    {
        var types = new List<EntityType>
        {
            new() { Id = "a", Singular = "مشروع", IsDefault = true },
            new() { Id = "b", Singular = "قسم" },
        };
        Assert.Equal("b", EntityTypeStore.Resolve(types, "b")!.Id);
        Assert.Equal("a", EntityTypeStore.Resolve(types, null)!.Id);
        Assert.Equal("a", EntityTypeStore.Resolve(types, "other-org-type")!.Id);
    }

    [Fact]
    public void Validate_EnglishNames_OptionalWithDerivedPlural()
    {
        var t = new EntityType();
        Assert.Null(EntityTypeStore.Validate(new EntityTypeRequest { Singular = "قسم", SingularEn = " Department " }, t, creating: true));
        Assert.Equal(("Department", "Departments"), (t.SingularEn, t.PluralEn));

        var u = new EntityType();
        Assert.Null(EntityTypeStore.Validate(new EntityTypeRequest { Singular = "فرع", SingularEn = "Branch", PluralEn = "Branches" }, u, creating: true));
        Assert.Equal(("Branch", "Branches"), (u.SingularEn, u.PluralEn));

        var none = new EntityType();
        Assert.Null(EntityTypeStore.Validate(new EntityTypeRequest { Singular = "وحدة" }, none, creating: true));
        Assert.Null(none.SingularEn);
        Assert.Null(none.PluralEn);

        // تعديل جزئي: إرسال العربي وحده لا يمس الإنجليزي، وإفراغ المفرد الإنجليزي يمسح الاثنين
        Assert.Null(EntityTypeStore.Validate(new EntityTypeRequest { Plural = "أقسام" }, t, creating: false));
        Assert.Equal("Department", t.SingularEn);
        Assert.Null(EntityTypeStore.Validate(new EntityTypeRequest { SingularEn = "" }, t, creating: false));
        Assert.Null(t.SingularEn);
        Assert.Null(t.PluralEn);
        Assert.NotNull(EntityTypeStore.Validate(new EntityTypeRequest { SingularEn = new string('x', 41) }, new EntityType { Singular = "قسم" }, creating: false));
    }
}
