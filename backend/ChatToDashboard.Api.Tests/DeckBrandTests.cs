using ChatToDashboard.Api.Decks;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>تحقق هوية المشروع: الألوان، الخط، الشعار، والتفريغ.</summary>
public class DeckBrandTests
{
    [Fact]
    public void Validate_NormalizesColorsAndFont()
    {
        var b = new DeckBrand { Name = "  الأفق  ", Primary = "#7a1f1f", Secondary = "red", Accent = " #C84A2A ", Font = "cairo", Footer = "" };
        Assert.Null(b.Validate());
        Assert.Equal("الأفق", b.Name);
        Assert.Equal("#7A1F1F", b.Primary);
        Assert.Null(b.Secondary);
        Assert.Equal("#C84A2A", b.Accent);
        Assert.Equal("Cairo", b.Font);
        Assert.Null(b.Footer);
        Assert.False(b.IsEmpty);
    }

    [Fact]
    public void Validate_RejectsNonImageLogo_AndOversize()
    {
        Assert.NotNull(new DeckBrand { Logo = "data:text/html;base64,PGh0bWw+" }.Validate());
        Assert.NotNull(new DeckBrand { Logo = "<svg/>" }.Validate());
        var big = "data:image/png;base64," + new string('A', (DeckBrand.MaxLogoBytes + 1024) * 4 / 3);
        Assert.NotNull(new DeckBrand { Logo = big }.Validate());
        Assert.Null(new DeckBrand { Logo = "data:image/png;base64,iVBORw0KGgo=" }.Validate());
    }

    [Fact]
    public void Validate_UnknownFont_DroppedAndEmptyDetected()
    {
        var b = new DeckBrand { Font = "Comic Sans", Primary = "#GGGGGG" };
        Assert.Null(b.Validate());
        Assert.Null(b.Font); Assert.Null(b.Primary);
        Assert.True(b.IsEmpty);
    }
}
