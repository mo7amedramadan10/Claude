using ChatToDashboard.Api.Projects;

namespace ChatToDashboard.Api.Tests;

/// <summary>يختبر القواعد الصافية لشاشة «اللوحات المنشورة» (§4.23): تصفية الجمهور حسب دور
/// المشروع، واشتقاق صفحات اللوحة من عناصرها، ودمج ترتيب جزئي مع الترتيب القائم.</summary>
public class PublishedDashboardRulesTests
{
    [Theory]
    [InlineData("all", "viewer", true)]
    [InlineData("all", null, true)]
    [InlineData("editors", "viewer", false)]
    [InlineData("editors", null, false)]
    [InlineData("editors", "editor", true)]
    [InlineData("editors", "owner", true)]
    [InlineData("owners", "editor", false)]
    [InlineData("owners", "owner", true)]
    public void CanView_FiltersByAudienceAndProjectRole(string audience, string? role, bool expected) =>
        Assert.Equal(expected, PublishedDashboardRules.CanView(audience, role, isAdmin: false));

    [Fact]
    public void CanView_AdminSeesEveryAudience() =>
        Assert.True(PublishedDashboardRules.CanView(PublishedAudiences.Owners, null, isAdmin: true));

    [Theory]
    [InlineData("owner", true)]
    [InlineData("editor", true)]
    [InlineData("viewer", false)]
    [InlineData(null, false)]
    public void CanManage_OnlyOwnerAndEditor(string? role, bool expected) =>
        Assert.Equal(expected, PublishedDashboardRules.CanManage(role, isAdmin: false));

    [Fact]
    public void PagesFromWidgets_KeepsFirstSeenOrderAndAddsOtherBucket()
    {
        var json = """[{"page":"الملخص"},{"page":"التفاصيل"},{"title":"بلا صفحة"},{"page":"الملخص"}]""";
        Assert.Equal(new[] { "الملخص", "التفاصيل", "عناصر أخرى" }, PublishedDashboardRules.PagesFromWidgets(json));
    }

    [Theory]
    [InlineData("""[{"page":"وحيدة"},{"title":"x"}]""")]
    [InlineData("""[{"title":"x"}]""")]
    [InlineData("[]")]
    [InlineData("ليس json")]
    [InlineData(null)]
    public void PagesFromWidgets_SinglePageOrInvalidMeansNoTabs(string? json) =>
        Assert.Empty(PublishedDashboardRules.PagesFromWidgets(json));

    [Fact]
    public void ApplyOrder_PutsRequestedFirstAndKeepsUnmentionedInPlace()
    {
        var current = new[] { "a", "b", "c", "d" };
        // المرسِل لا يرى «b» (جمهور أضيق) — تبقى بعد المعرّفات المرسلة بترتيبها السابق.
        var result = PublishedDashboardRules.ApplyOrder(current, new[] { "d", "x", "a", "d", "c" });
        Assert.Equal(new[] { "d", "a", "c", "b" }, result);
    }
}
