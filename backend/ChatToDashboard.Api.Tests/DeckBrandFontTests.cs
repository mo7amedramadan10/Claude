using ChatToDashboard.Api.Decks;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>قراءة اسم العائلة والوزن من ملفات الخطوط بلا مكتبة، والـ @font-face الناتج.</summary>
public class DeckBrandFontTests
{
    [Fact]
    public void ReadSfntNames_RealTtf_IfPresent()
    {
        var path = "/usr/share/fonts/truetype/dejavu/DejaVuSerif-Bold.ttf";
        if (!File.Exists(path)) return;
        var names = DeckBrandFontStore.ReadSfntNames(File.ReadAllBytes(path));
        Assert.NotNull(names);
        Assert.Equal("DejaVu Serif", names!.Value.Family);
        Assert.Equal((700, "normal"), DeckBrandFontStore.WeightStyleOf(names.Value.Subfamily));
    }

    [Fact]
    public void ReadSfntNames_RejectsGarbage()
    {
        Assert.Null(DeckBrandFontStore.ReadSfntNames(new byte[] { 1, 2, 3 }));
        Assert.Null(DeckBrandFontStore.ReadSfntNames(System.Text.Encoding.ASCII.GetBytes("<html>not a font</html>")));
    }

    [Theory]
    [InlineData("Regular", 400, "normal")]
    [InlineData("Bold Italic", 700, "italic")]
    [InlineData("SemiBold", 600, "normal")]
    [InlineData("Light", 300, "normal")]
    [InlineData("ExtraLight", 200, "normal")]
    [InlineData("Black", 900, "normal")]
    public void WeightStyleOf_Subfamily(string sub, int w, string st) => Assert.Equal((w, st), DeckBrandFontStore.WeightStyleOf(sub));

    [Fact]
    public void FontFaceCss_And_BrandAcceptsUploadedFamily()
    {
        var f = new DeckBrandFont { Id = "abc", Family = "Tanseek Modern Pro Arabic", Weight = 500, Style = "normal", Format = "truetype" };
        Assert.Contains("font-family:'Tanseek Modern Pro Arabic'", f.FontFaceCss());
        Assert.Contains("/api/decks/brand/fonts/abc/file", f.FontFaceCss());
        var b = new DeckBrand { Font = "tanseek modern pro arabic" };
        Assert.Null(b.Validate(new[] { "Tanseek Modern Pro Arabic" }));
        Assert.Equal("Tanseek Modern Pro Arabic", b.Font);
        var b2 = new DeckBrand { Font = "Tanseek Modern Pro Arabic" };
        b2.Validate(); Assert.Null(b2.Font);
    }
}
