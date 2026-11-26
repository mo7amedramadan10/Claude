using ChatToDashboard.Api.Models;
using ChatToDashboard.Api.Templates;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>نموذج «لوحة أداء المتجر» في لوحات المتابعة: في الكتالوج بصفحاته الثلاث وأنواع مسموحة، ومدخل المعاينة بالعناوين نفسها.</summary>
public class StorePerformanceTemplateTests
{
    private static string WebRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "ChatToDashboard.Api", "wwwroot"))) dir = dir.Parent;
        return Path.Combine(dir!.FullName, "ChatToDashboard.Api", "wwwroot");
    }

    [Fact]
    public void InCatalog_WithThreePages_AllowedTypes_AndMatchingGalleryEntry()
    {
        var t = BuiltinTemplates.Dashboards.Single(d => d.Id == "pl-store-performance");
        Assert.Equal("sales", t.Category);
        Assert.Equal(3, t.Widgets.Select(w => w.Page).Distinct().Count());
        Assert.All(t.Widgets, w => Assert.Contains(w.Type, DashboardSpec.AllowedWidgetTypes));
        Assert.Equal(t.Widgets.Length, t.Widgets.Select(w => w.Title).Distinct().Count());

        var lib = File.ReadAllText(Path.Combine(WebRoot(), "js", "dash-library.js"));
        Assert.Contains("\"id\":\"pl-store-performance\"", lib);
        Assert.All(t.Widgets, w => Assert.Contains($"\"ti\":\"{w.Title}\"", lib));
        Assert.All(t.Widgets.Select(w => w.Page!).Distinct(), p => Assert.Contains($"\"n\":\"{p}\"", lib));
    }
}
