using System.Text.Json.Nodes;
using ChatToDashboard.Api.Decks;
using ChatToDashboard.Api.Freeform;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>عقد البيانات الحيّة: كتلة jeem-data (حقن/قراءة/استبدال/مفاتيح) وإعادة صياغة شرائح العروض حتميًا.</summary>
public class FreeformDataRefreshTests
{
    private static JsonNode Res(string json) => JsonNode.Parse(json)!;

    [Fact]
    public void Inject_ThenParse_Roundtrips_AndReplacesOnReinject()
    {
        var html = "<!doctype html><html dir=\"rtl\"><head><meta charset=\"utf-8\"><script>var x=1</script></head><body><h1>t</h1></body></html>";
        var q1 = new FreeformQuery("q1", "SELECT 1 AS n", Res("{\"rowCount\":1,\"rows\":[{\"n\":1}]}"));
        var injected = FreeformDataBlock.Inject(html, new[] { q1 }, new DateTime(2026, 11, 5, 0, 0, 0, DateTimeKind.Utc));
        Assert.True(FreeformDataBlock.HasBlock(injected));
        Assert.True(injected.IndexOf("id=\"jeem-data\"") < injected.IndexOf("var x=1"), "الكتلة تسبق سكربتات الصفحة");
        Assert.Contains("window.JEEM_DATA=d", injected);
        var parsed = FreeformDataBlock.Parse(injected);
        Assert.Single(parsed); Assert.Equal("q1", parsed[0].Key); Assert.Equal("SELECT 1 AS n", parsed[0].Sql); Assert.Equal(1, parsed[0].Result!["rows"]![0]!["n"]!.GetValue<int>());
        var q1b = q1 with { Result = Res("{\"rowCount\":1,\"rows\":[{\"n\":2}]}") };
        var re = FreeformDataBlock.Inject(injected, new[] { q1b, new FreeformQuery("q2", "SELECT 2", null) }, DateTime.UtcNow);
        Assert.Equal(1, System.Text.RegularExpressions.Regex.Matches(re, "id=\"jeem-data\"").Count);
        Assert.Equal(2, FreeformDataBlock.Parse(re).Count); Assert.Equal(2, FreeformDataBlock.Parse(re)[0].Result!["rows"]![0]!["n"]!.GetValue<int>());
        Assert.Equal("q3", FreeformDataBlock.NextKey(FreeformDataBlock.Parse(re)));
        Assert.Equal("q1", FreeformDataBlock.NextKey(Array.Empty<FreeformQuery>()));
    }

    [Fact]
    public void Inject_WithoutHead_AndEscapesClosingTag()
    {
        var q = new FreeformQuery("q1", "SELECT '</script>' AS s", Res("{\"rows\":[{\"s\":\"</script>\"}]}"));
        var out1 = FreeformDataBlock.Inject("<html><body>x</body></html>", new[] { q }, DateTime.UtcNow);
        Assert.Contains("<head>", out1); Assert.DoesNotContain("</script>' AS s", out1); Assert.Contains("<\\/script>", out1);
        Assert.Single(FreeformDataBlock.Parse(out1));
        Assert.Equal("<html>", FreeformDataBlock.Inject("<html>", Array.Empty<FreeformQuery>(), DateTime.UtcNow));
    }

    [Fact]
    public void DeckRefresher_ShapesRowsByLayout()
    {
        var rows = (JsonArray)JsonNode.Parse("[{\"المنطقة\":\"الرياض\",\"الإيراد\":1200.5,\"n\":3},{\"المنطقة\":\"جدة\",\"الإيراد\":800,\"n\":2}]")!;
        var chart = new DeckSlide { Layout = "chart", Chart = new DeckChart { Type = "line" } };
        Assert.True(DeckDataRefresher.Apply(chart, rows)); Assert.Equal(new[] { "الرياض", "جدة" }, chart.Chart!.Labels); Assert.Equal(new[] { 1200.5, 800 }, chart.Chart.Values); Assert.Equal("line", chart.Chart.Type);
        var table = new DeckSlide { Layout = "table" }; Assert.True(DeckDataRefresher.Apply(table, rows)); Assert.Equal(3, table.Table!.Columns.Count); Assert.Equal("1,200.50", table.Table.Rows[0][1]);
        var kpis = new DeckSlide { Layout = "kpis" }; Assert.True(DeckDataRefresher.Apply(kpis, rows)); Assert.Equal(2, kpis.Kpis!.Count); Assert.Equal("الرياض", kpis.Kpis[0].Label);
        var one = (JsonArray)JsonNode.Parse("[{\"الإيراد\":5000,\"الصفقات\":12}]")!; var k1 = new DeckSlide { Layout = "kpis" };
        Assert.True(DeckDataRefresher.Apply(k1, one)); Assert.Equal(2, k1.Kpis!.Count); Assert.Equal("5,000", k1.Kpis[0].Value);
        var bullets = new DeckSlide { Layout = "bullets" }; Assert.True(DeckDataRefresher.Apply(bullets, rows)); Assert.Equal("الرياض: 1,200.50", bullets.Bullets![0]);
        Assert.False(DeckDataRefresher.Apply(new DeckSlide { Layout = "chart" }, new JsonArray()));
    }
}
