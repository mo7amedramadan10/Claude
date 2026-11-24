using ChatToDashboard.Api.Freeform;
using Xunit;

namespace ChatToDashboard.Api.Tests;

public class FreeformBlocksTests
{
    [Fact]
    public void EnsureBlocksKit_OnlyWhenBlocksPresent_AndOnce()
    {
        const string plain = "<html><body><p>بلا بيانات</p></body></html>";
        Assert.Equal(plain, FreeformDataBlock.EnsureBlocksKit(plain));
        const string withChart = "<html><body><div data-jeem-chart='{\"q\":\"q1\"}'></div><script>var s='</body>';</script></body></html>";
        var once = FreeformDataBlock.EnsureBlocksKit(withChart);
        Assert.Contains("/js/jeem-viz.js", once); Assert.Contains("/js/jeem-blocks.js", once);
        Assert.True(once.IndexOf("jeem-blocks.js") > once.IndexOf("var s='</body>'"), "الحقن قبل آخر </body> لا داخل السكربت");
        Assert.Equal(once, FreeformDataBlock.EnsureBlocksKit(once));
        Assert.Contains("jeem-blocks.js", FreeformDataBlock.EnsureBlocksKit("<body><span data-jeem-value=\"q2.x\">—</span></body>"));
    }

    [Fact]
    public void SystemPrompt_TeachesBlocksOnlyWithData()
    {
        var withData = FreeformBuilder.BuildSystemPrompt(null, true, "جدول sales", "dashboard", null, "ar");
        Assert.Contains("data-jeem-chart", withData); Assert.Contains("data-jeem-value", withData);
        Assert.DoesNotContain("data-jeem-chart", FreeformBuilder.BuildSystemPrompt(null, false, "", "dashboard", null, "ar"));
    }
}
