using System.Text.Json.Nodes;
using ChatToDashboard.Api.Freeform;
using Xunit;

namespace ChatToDashboard.Api.Tests;

public class FreeformBlockValidatorTests
{
    private static readonly List<FreeformQuery> Qs = new()
    {
        new("q1", "SELECT Region, Revenue FROM t", JsonNode.Parse("""{"rowCount":2,"rows":[{"Region":"الرياض","Revenue":5},{"Region":"جدة","Revenue":3}]}""")),
        new("q2", "SELECT 1", JsonNode.Parse("""{"rowCount":0,"rows":[]}""")),
    };

    [Fact]
    public void ValidBlocks_NoProblems_CaseInsensitiveAndEncoded()
    {
        const string html = """<div data-jeem-chart='{"q":"q1","type":"bar","x":"region","y":["Revenue"],"title":"الإيراد"}'></div>"""
            + """<div data-jeem-chart="{&quot;q&quot;:&quot;q1&quot;,&quot;type&quot;:&quot;donut&quot;}"></div><b data-jeem-value="q1.Revenue">—</b>"""
            + """<b data-jeem-value="q2.anything">—</b><script>var s = '<div data-jeem-chart=\'{"q":"q9"}\'>';</script>""";
        Assert.Empty(FreeformBlockValidator.Validate(html, Qs));
        Assert.Equal(4, FreeformBlockValidator.CountBlocks(html));
    }

    [Fact]
    public void InvalidBlocks_ReportedInArabic()
    {
        const string html = """<div data-jeem-chart='{"q":"q9","type":"bar"}'></div><div data-jeem-chart='{"q":"q1","type":"radar","x":"City","y":"Revenue","title":"ت"}'></div>"""
            + """<div data-jeem-chart='{bad'></div><span data-jeem-value="q1">—</span><span data-jeem-value="q1.Profit">—</span>""";
        var p = FreeformBlockValidator.Validate(html, Qs);
        Assert.Contains(p, x => x.Contains("q9") && x.Contains("query_data"));
        Assert.Contains(p, x => x.Contains("radar"));
        Assert.Contains(p, x => x.Contains("«City»"));
        Assert.Contains(p, x => x.Contains("JSON"));
        Assert.Contains(p, x => x.Contains("qN.<العمود>"));
        Assert.Contains(p, x => x.Contains("«Profit»"));
    }

    [Fact]
    public void Describe_ListsKeysAndColumns()
    {
        var d = FreeformBlockValidator.Describe(Qs);
        Assert.Contains("q1: 2 صف", d); Assert.Contains("Region", d); Assert.Contains("(بلا صفوف)", d);
        Assert.Equal("", FreeformBlockValidator.Describe(new List<FreeformQuery>()));
        Assert.Contains("data-jeem-chart", FreeformBuilder.FragmentRules("الشريحة 1 من 3"));
    }
}

public class FreeformSourceReferenceTests
{
    [Fact]
    public void Prompt_ExplainsSourcesAndChartReplacement()
    {
        var withData = FreeformBuilder.BuildSystemPrompt(null, true, "## الجدول proj_1.opp (المصدر: Opportunities)", "slides", null, "ar");
        Assert.Contains("ملف المصدر", withData); Assert.Contains("لا تطلب منه إرسال الملف", withData); Assert.Contains("data-jeem-replaced", withData);
        var noData = FreeformBuilder.BuildSystemPrompt(null, false, "", "slides", null, "ar");
        Assert.Contains("لا توجد مصادر بيانات متاحة", noData); Assert.DoesNotContain("data-jeem-replaced", noData);
    }

    [Fact]
    public void ChartElements_ListsTemplateChartsOnly()
    {
        const string html = """<section class="slide"><svg class="deco" viewBox="0 0 9 9"></svg><svg class="chart" id="c12" viewBox="0 0 640 310"></svg><canvas id="cv"></canvas><svg class="chart" id="old" data-jeem-replaced></svg><div class="chart" data-jeem-chart='{"q":"q1"}'></div><svg viewBox="0 0 24 24"></svg><script>var x='<svg class="chart" id="no">';</script></section>""";
        Assert.Equal(new[] { "svg#c12.chart", "canvas#cv" }, FreeformScope.ChartElements(html));
    }
}
