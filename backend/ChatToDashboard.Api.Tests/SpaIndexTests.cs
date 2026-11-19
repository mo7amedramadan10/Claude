using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>index.html يُخدم تحت مسارات عميقة (/d/{org}/{slug}) بمراجع أصول مطلقة، مع بقاء مراجع أيقونات SVG الداخلية كما هي.</summary>
public class SpaIndexTests
{
    [Fact]
    public void Rewrite_MakesAssetPathsAbsolute_AndKeepsFragments()
    {
        const string html = "<link rel=\"stylesheet\" href=\"css/app.css\"><script src=\"js/app.js\"></script><img src=\"img/logo-mark.svg\"><use href=\"#i-send\"/><a href=\"/api/x\">x</a>";
        var outHtml = SpaIndex.Rewrite(html);
        Assert.Contains("href=\"/css/app.css\"", outHtml);
        Assert.Contains("src=\"/js/app.js\"", outHtml);
        Assert.Contains("src=\"/img/logo-mark.svg\"", outHtml);
        Assert.Contains("<use href=\"#i-send\"/>", outHtml);
        Assert.Contains("href=\"/api/x\"", outHtml);
    }

    [Fact]
    public void Rewrite_RealIndexHtml_HasNoRelativeAssetLeft()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "ChatToDashboard.Api", "wwwroot", "index.html");
        if (!File.Exists(path)) return;
        var outHtml = SpaIndex.Rewrite(File.ReadAllText(path));
        Assert.DoesNotMatch("\\s(href|src)=\"(css|js|img)/", outHtml);
        Assert.Contains("<use href=\"#i-", outHtml);
    }
}
