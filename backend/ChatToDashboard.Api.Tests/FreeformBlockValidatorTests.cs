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
