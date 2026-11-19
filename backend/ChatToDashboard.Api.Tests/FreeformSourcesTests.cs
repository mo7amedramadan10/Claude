using ChatToDashboard.Api.Controllers;
using ChatToDashboard.Api.Freeform;
using ChatToDashboard.Api.Sources;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>اختيار المصادر لتصميم حر: ما يُحفظ مع التصميم وما يُعاد للواجهة.</summary>
public class FreeformSourcesTests
{
    [Fact]
    public void SourcesJsonOf_NullOrAll_IsNull()
    {
        Assert.Null(FreeformController.SourcesJsonOf(null));
        Assert.Null(FreeformController.SourcesJsonOf(SourceSelection.AllEnabled()));
    }

    [Fact]
    public void SourcesJsonOf_ExplicitLists_RoundTripThroughDesign()
    {
        var json = FreeformController.SourcesJsonOf(new SourceSelection { Systems = new() { "crm" }, Files = new() { "f1", "f2" } });
        Assert.NotNull(json);
        var d = new FreeformDesign { SourcesJson = json };
        var sel = d.SourcesSelection!;
        Assert.Equal(new[] { "crm" }, sel.Systems);
        Assert.Equal(new[] { "f1", "f2" }, sel.Files);
        Assert.False(sel.SystemsUnset); Assert.False(sel.FilesUnset); // قوائم صريحة تقيّد الاستعلام فعلًا
        Assert.Null(new FreeformDesign { SourcesJson = "{bad" }.SourcesSelection);
        Assert.Null(new FreeformDesign().SourcesSelection);
    }

    [Fact]
    public void SourcesJsonOf_EmptyLists_MeansNoSources()
    {
        var json = FreeformController.SourcesJsonOf(new SourceSelection());
        var sel = new FreeformDesign { SourcesJson = json }.SourcesSelection!;
        Assert.Empty(sel.Systems); Assert.Empty(sel.Files); Assert.False(sel.AllowsFile("f1"));
    }
}
