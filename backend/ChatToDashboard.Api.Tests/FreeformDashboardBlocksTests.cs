using System.Text.Json;
using System.Text.Json.Nodes;
using ChatToDashboard.Api.Freeform;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>تحويل لوحة متابعة إلى تصميم حر برسوم حيّة: وسم البلوك الجاهز لكل نوع عنصر، اجتيازه للمدقّق، وإضافة ما ينساه الموديل.</summary>
public class FreeformDashboardBlocksTests
{
    static JsonElement W(string json) => JsonSerializer.Deserialize<JsonElement>(json);
    static JsonArray Rows(string json) => (JsonArray)JsonNode.Parse(json)!;
    static FreeformQuery Q(string key, string rows) => new(key, "SELECT 1", JsonNode.Parse($"{{\"rows\":{rows},\"rowCount\":1}}"));

    [Fact]
    public void Markup_BarWithKeys_StackedSeries_Kpi_Table()
    {
        var rows = Rows("""[{"المرحلة":"عرض","العدد":4,"القيمة":10},{"المرحلة":"تفاوض","العدد":2,"القيمة":7}]""");
        var bar = FreeformDashboardBlocks.Markup(W("""{"type":"bar","title":"الفرص حسب المرحلة","xKey":"المرحلة","yKey":"العدد"}"""), "q1", rows);
        Assert.Contains("data-jeem-chart='", bar);
        Assert.Contains("\"q\":\"q1\"", bar); Assert.Contains("\"type\":\"bar\"", bar); Assert.Contains("\"x\":\"المرحلة\"", bar); Assert.Contains("\"y\":\"العدد\"", bar);

        var multi = FreeformDashboardBlocks.Markup(W("""{"type":"bar","title":"x","stacked":true,"series":[{"key":"العدد","label":"عدد"},{"key":"القيمة","label":"قيمة"}]}"""), "q2", rows);
        Assert.Contains("\"y\":[\"العدد\",\"القيمة\"]", multi); Assert.Contains("\"stacked\":true", multi); Assert.Contains("\"labels\":{", multi);

        var kpi = FreeformDashboardBlocks.Markup(W("""{"type":"kpi","title":"إجمالي الفرص"}"""), "q3", Rows("""[{"إجمالي":17}]"""));
        Assert.Contains("data-jeem-value=\"q3.إجمالي\"", kpi); Assert.Contains("إجمالي الفرص", kpi);

        Assert.Contains("\"type\":\"table\"", FreeformDashboardBlocks.Markup(W("""{"type":"progress-table","title":"t"}"""), "q4", rows));
        Assert.Contains("\"type\":\"donut\"", FreeformDashboardBlocks.Markup(W("""{"type":"radial-gauge","title":"t"}"""), "q5", rows));
        Assert.Contains("\"type\":\"line\"", FreeformDashboardBlocks.Markup(W("""{"type":"area","title":"t"}"""), "q6", rows));
        // عنوان فيه فاصلة عليا لا يكسر السمة المقتبسة بعلامة مفردة
        Assert.DoesNotContain("'s", FreeformDashboardBlocks.Markup(W("""{"type":"bar","title":"Q3's pipeline"}"""), "q7", rows).Replace("data-jeem-chart='", "").Replace("' style", ""));
    }

    [Fact]
    public void Markup_PassesBlockValidator()
    {
        var rows = """[{"المرحلة":"عرض","العدد":4}]""";
        var html = "<html><body>" + FreeformDashboardBlocks.Markup(W("""{"type":"bar","title":"a","xKey":"المرحلة","yKey":"العدد"}"""), "q1", Rows(rows))
            + FreeformDashboardBlocks.Markup(W("""{"type":"kpi","title":"b"}"""), "q2", Rows("""[{"العدد":4}]""")) + "</body></html>";
        Assert.Empty(FreeformBlockValidator.Validate(html, new[] { Q("q1", rows), Q("q2", """[{"العدد":4}]""") }));
    }

    [Fact]
    public void EnsureBlocks_AddsMissingAsSlidesAfterLastSlide_OrSectionsBeforeBodyEnd()
    {
        var blocks = new List<FreeformDashboardBlocks.Block>
        {
            new("q1", "الأولى", "<div data-jeem-chart='{\"q\":\"q1\",\"type\":\"bar\"}'></div>"),
            new("q2", "الثانية", "<span data-jeem-value=\"q2.x\"></span>"),
        };
        var deck = "<html dir=\"rtl\"><body><div id=\"deck\"><section class=\"slide\">1 <div data-jeem-chart='{\"q\":\"q1\",\"type\":\"bar\"}'></div></section><section class=\"slide\">2</section></div><nav>x</nav></body></html>";
        var (h, n) = FreeformDashboardBlocks.EnsureBlocks(deck, blocks);
        Assert.Equal(1, n);
        Assert.Equal(3, FreeformScope.FindSlides(h).Count);
        Assert.True(h.IndexOf("الثانية") < h.IndexOf("</div><nav>"));
        var (h2, n2) = FreeformDashboardBlocks.EnsureBlocks("<html><body><p>x</p></body></html>", blocks);
        Assert.Equal(2, n2); Assert.EndsWith("</body></html>", h2); Assert.Contains("data-jeem-added=\"chart\"", h2);
        Assert.Equal(0, FreeformDashboardBlocks.EnsureBlocks(h2, blocks).Added);
    }
}
