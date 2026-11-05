using ChatToDashboard.Api.Freeform;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>حقن سكربتات المنصة في صفحة مرفوعة: قبل آخر <c>&lt;/body&gt;</c> لا أوّله — صفحة تحمل «&lt;/body&gt;» داخل نص JavaScript
/// مقتبس (دالة تعيد بناء الملف) كانت تنكسر كليًا لأن السكربت المحقون وقع داخل سكربتها.</summary>
public class FreeformInjectionTests
{
    private const string PageWithBodyInsideScript =
        "<!doctype html><html><body><p>x</p><script id=\"app\">const t='<!doctype html><body>'+b+'</body></html>';</script></body></html>";

    [Fact]
    public void InsertBeforeBodyEnd_SkipsBodyTagInsideScriptString()
    {
        var outp = FreeformDataBlock.InsertBeforeBodyEnd(PageWithBodyInsideScript, "<i id=\"tag\"></i>");
        Assert.Equal("<!doctype html><html><body><p>x</p><script id=\"app\">const t='<!doctype html><body>'+b+'</body></html>';</script><i id=\"tag\"></i></body></html>", outp);
    }

    [Fact]
    public void InsertBeforeBodyEnd_AppendsWhenNoBody()
    {
        Assert.Equal("<p>x</p><i></i>", FreeformDataBlock.InsertBeforeBodyEnd("<p>x</p>", "<i></i>"));
        Assert.Equal("<p>x</p><i></i></BODY>", FreeformDataBlock.InsertBeforeBodyEnd("<p>x</p></BODY>", "<i></i>"));
    }

    [Fact]
    public void EnsurePageBridge_AndSlidesKit_InjectOnceBeforeLastBody()
    {
        var bridge = FreeformDataBlock.EnsurePageBridge(PageWithBodyInsideScript);
        Assert.Equal(1, bridge.Split("jeem-page.js").Length - 1);
        Assert.EndsWith("</script><script src=\"/js/jeem-page.js\"></script></body></html>", bridge);
        Assert.Contains("'+b+'</body></html>';</script>", bridge);

        var slides = FreeformDataBlock.EnsureSlidesKit(PageWithBodyInsideScript);
        Assert.Equal(1, slides.Split("jeem-slides.js").Length - 1);
        Assert.EndsWith("</script><script src=\"/js/jeem-slides.js\"></script></body></html>", slides);
    }
}
