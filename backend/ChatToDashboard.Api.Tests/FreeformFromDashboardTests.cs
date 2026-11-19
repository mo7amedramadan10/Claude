using System.Text.Json;
using ChatToDashboard.Api.Controllers;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>نص مواصفة اللوحة المرسل للموديل عند تحويل لوحة متابعة إلى تصميم حر.</summary>
public class FreeformFromDashboardTests
{
    [Fact]
    public void SpecText_ListsWidgetsWithTypeSqlAndData()
    {
        var json = JsonDocument.Parse("""
            {"name":"لوحة الإيرادات","summary":"ملخص","widgets":[
              {"title":"إجمالي الفرص","type":"kpi","layout":{"size":"small"},"query":{"sql":"SELECT COUNT(*) FROM t"},"data":{"value":1069}},
              {"title":"حسب الحالة","type":"bar","page":"المبيعات","data":[{"label":"a","value":1}]},
              {"title":"فارغ","type":"table","noData":true}
            ]}
            """).RootElement;
        var text = FreeformController.DashboardSpecText(json, out var sqls);
        Assert.Contains("العنوان: لوحة الإيرادات", text);
        Assert.Contains("العنصر 1: إجمالي الفرص (النوع: kpi، الحجم: small)", text);
        Assert.Contains("SQL: SELECT COUNT(*) FROM t", text);
        Assert.Contains("الصفحة/التبويب: المبيعات", text);
        Assert.Contains("بلا بيانات", text);
        Assert.Single(sqls);
    }

    [Fact]
    public void SpecText_EmptyWithoutWidgets()
    {
        var json = JsonDocument.Parse("""{"summary":"x","widgets":[]}""").RootElement;
        Assert.Equal("", FreeformController.DashboardSpecText(json, out var sqls));
        Assert.Empty(sqls);
    }
}
