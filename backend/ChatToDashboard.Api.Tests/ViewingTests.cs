using ChatToDashboard.Api.Controllers;
using ChatToDashboard.Api.Users;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>صفحة «للاطلاع»: صلاحية الشاشة الجاهزة «اطلاع فقط» ومصغّرة اللوحة بلا بيانات.</summary>
public class ViewingTests
{
    [Fact]
    public void ViewOnlyPreset_AllowsOnlyViewingApi()
    {
        var allowed = ScreenAccess.Normalize(ScreenAccess.ViewOnly)!;
        Assert.Equal(new[] { "viewing" }, allowed);
        Assert.True(ScreenAccess.IsApiAllowed("/api/viewing", allowed));
        Assert.True(ScreenAccess.IsApiAllowed("/api/viewing/thumb/abc", allowed));
        Assert.False(ScreenAccess.IsApiAllowed("/api/chat", allowed));
        Assert.False(ScreenAccess.IsApiAllowed("/api/history", allowed));
        Assert.False(ScreenAccess.IsApiAllowed("/api/decks", allowed));
        Assert.False(ScreenAccess.IsApiAllowed("/api/viewing", new[] { "freeform", "repo" }));
        Assert.Contains("viewing", ScreenAccess.Screens);
    }

    [Fact]
    public void WidgetSketch_KeepsShapeOnly_NoData()
    {
        var sketch = ViewingController.WidgetSketch("""[{"type":"kpi","size":"small","title":"الإيراد","data":[{"v":5}],"query":{"sql":"SELECT 1"}},{"type":"bar","title":"حسب المنطقة"},"x"]""");
        Assert.Equal(2, sketch.Count);
        var json = System.Text.Json.JsonSerializer.Serialize(sketch);
        Assert.Contains("\"type\":\"kpi\"", json);
        Assert.DoesNotContain("SELECT", json);
        Assert.DoesNotContain("\"data\"", json);
        Assert.Empty(ViewingController.WidgetSketch("not json"));
        Assert.Empty(ViewingController.WidgetSketch(null));
    }

    [Fact]
    public void InProject_CurrentProjectOrLegacyWithoutProject()
    {
        Assert.True(ViewingController.InEntity("p1", "p1"));
        Assert.True(ViewingController.InEntity("P1", "p1"));
        Assert.True(ViewingController.InEntity(null, "p1"));
        Assert.False(ViewingController.InEntity("p2", "p1"));
        Assert.False(ViewingController.InEntity("p2", null));
    }
}
