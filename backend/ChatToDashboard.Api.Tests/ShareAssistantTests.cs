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
