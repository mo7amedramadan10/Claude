using ChatToDashboard.Api.Presentations;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>سياق مساعد «جيم» على رابط لوحة متابعة مشارَكة: من لقطة اللوحة (الملخص والعناصر وبياناتها) بلا SQL.</summary>
public class ShareAssistantTests
{
    [Fact]
    public void DashboardContext_ListsWidgetsAndRows_WithoutSql()
    {
        var widgets = """
            [{"title":"إجمالي الفرص","type":"kpi","data":{"value":1069,"label":"عدد الفرص"},"query":{"sql":"SELECT COUNT(*) FROM t"}},
             {"title":"حسب الحالة","type":"bar","page":"المبيعات","data":[{"label":"Won","value":55},{"label":"Lost","value":20}]},
             {"title":"فارغ","type":"table","noData":true}]
            """;
        var text = PresentationAssistantService.DashboardContext("لوحة الفرص", "ملخص", widgets);
        Assert.Contains("العنوان: لوحة الفرص", text);
        Assert.Contains("الملخص: ملخص", text);
        Assert.Contains("العنصر 1: إجمالي الفرص (kpi)", text);
        Assert.Contains("value: 1069", text);
        Assert.Contains("التبويب: المبيعات", text);
        Assert.Contains("label: Won | value: 55", text);
        Assert.Contains("(بلا بيانات)", text);
        Assert.DoesNotContain("SELECT", text);
    }

    [Fact]
    public void DashboardContext_ToleratesBadJson()
    {
        var text = PresentationAssistantService.DashboardContext("x", null, "not json");
        Assert.Contains("العنوان: x", text);
    }
}

/// <summary>الرابط المقروء للوحة المشارَكة /d/{org}/{slug} والاحتياطي بالمعرّف.</summary>
public class SharePublicPathTests
{
    [Fact]
    public void PublicPath_UsesSlugWhenAvailable()
    {
        var e = new ChatToDashboard.Api.Share.SharedDashboard { Id = "abc", Slug = "لوحة-الفرص" };
        Assert.Equal("/d/ثقة/لوحة-الفرص", e.PublicPath("ثقة"));
        Assert.Equal("/?share=abc", e.PublicPath(null));
        Assert.Equal("/?share=abc", new ChatToDashboard.Api.Share.SharedDashboard { Id = "abc" }.PublicPath("ثقة"));
    }

    [Fact]
    public void IsLive_RespectsRevokeAndExpiry()
    {
        Assert.True(new ChatToDashboard.Api.Share.SharedDashboard().IsLive);
        Assert.False(new ChatToDashboard.Api.Share.SharedDashboard { RevokedAt = DateTime.UtcNow }.IsLive);
        Assert.False(new ChatToDashboard.Api.Share.SharedDashboard { ExpiresAt = DateTime.UtcNow.AddMinutes(-1) }.IsLive);
    }
}
