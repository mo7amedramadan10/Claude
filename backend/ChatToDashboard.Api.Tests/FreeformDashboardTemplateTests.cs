using System.Text.Json;
using ChatToDashboard.Api.Freeform;
using ChatToDashboard.Api.Templates;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>نماذج كتالوج اللوحات داخل «تصميم حر» (نوع لوحة): التحويل الحتمي لكل نموذج، الرسم التخطيطي، والمفاتيح.</summary>
public class FreeformDashboardTemplateTests
{
    private const string Scratch = "/tmp/claude-0/-home-user-Claude/da72bb5f-6107-5fbc-b2dd-f8387c2e28b5/scratchpad/ffdash";

    [Fact]
    public void Convert_EveryBuiltinDashboard_ProducesValidTemplate()
    {
        var all = BuiltinTemplates.Dashboards.Select(b => FreeformDashboardTemplates.Convert(b.Id, b.Category, b.Name, b.Description, b.Widgets, b.RenderKind)).ToList();
        Assert.True(all.Count >= 240, $"عدد النماذج {all.Count}");
        Assert.Equal(all.Count, all.Select(t => t.Key).Distinct().Count());
        foreach (var t in all)
        {
            Assert.StartsWith("d:", t.Key); Assert.Equal("dashboard", t.Kind); Assert.True(t.Dashboard);
            Assert.NotEmpty(t.Sections); Assert.True(t.Sections.Count <= 40);
            Assert.All(t.Sections, s => { Assert.True(s.NeedsData); Assert.NotNull(s.Type); Assert.False(string.IsNullOrWhiteSpace(s.Title)); });
            Assert.Contains("query_data", t.Brief); Assert.StartsWith("h/", t.Sketch);
        }
        var interactive = all.Where(t => t.Key.Contains("network") || t.Sections.Any(s => s.Type == "interactive")).ToList();
        Assert.NotEmpty(interactive);
        try { Directory.CreateDirectory(Scratch); File.WriteAllText(Path.Combine(Scratch, "templates.json"), JsonSerializer.Serialize(all)); } catch { }
    }

    [Fact]
    public void Convert_MultiPage_PrefixesPageAndMentionsTabs()
    {
        var widgets = new[]
        {
            new BuiltinDashboardWidgetSpec("kpi", "الإيراد", Page: "نظرة عامة"), new BuiltinDashboardWidgetSpec("kpi", "الصفقات", Page: "نظرة عامة"),
            new BuiltinDashboardWidgetSpec("line", "الاتجاه الشهري", Page: "نظرة عامة"), new BuiltinDashboardWidgetSpec("donut", "التوزيع", Page: "التفاصيل"),
            new BuiltinDashboardWidgetSpec("table", "أكبر العملاء", Page: "التفاصيل"), new BuiltinDashboardWidgetSpec("table", "أكبر العملاء", Page: "التفاصيل"),
        };
        var t = FreeformDashboardTemplates.Convert("pl-x", "sales", "لوحة س", "وصف.", widgets, null);
        Assert.Equal(5, t.Sections.Count);
        Assert.Equal("نظرة عامة — الإيراد", t.Sections[0].Title); Assert.Equal("kpi", t.Sections[0].Type);
        Assert.Equal("table", t.Sections[4].Type);
        Assert.Contains("تبويبًا", t.Brief); Assert.Contains("نظرة عامة، التفاصيل", t.Brief);
        Assert.Equal("h/k2/l,d/t", t.Sketch);
        Assert.Equal("d:pl-x", t.Key); Assert.Equal("💼", t.Icon);
    }

    [Fact]
    public void Convert_Interactive_WithoutWidgets()
    {
        var t = FreeformDashboardTemplates.Convert("net-1", "network", "شبكة الموردين", "خريطة تفاعلية.", Array.Empty<BuiltinDashboardWidgetSpec>(), "network");
        Assert.Single(t.Sections); Assert.Equal("interactive", t.Sections[0].Type);
        Assert.Contains("لوحة تفاعلية", t.Brief); Assert.Equal("h/g/c,t", t.Sketch); Assert.Equal("🕸️", t.Icon);
    }

    [Theory]
    [InlineData("kpi", "kpi")] [InlineData("cbars", "bar")] [InlineData("hbars", "bar")] [InlineData("heat", "heat")] [InlineData("funnel", "funnel")] [InlineData("pie", "donut")] [InlineData("", "bar")]
    public void SectionType_MapsCatalogTypes(string input, string expected) => Assert.Equal(expected, FreeformDashboardTemplates.SectionType(input));

    [Fact]
    public void IsKey_OnlyPrefixed() { Assert.True(FreeformDashboardTemplates.IsKey("d:pl-x")); Assert.False(FreeformDashboardTemplates.IsKey("u:1")); Assert.False(FreeformDashboardTemplates.IsKey(null)); }
}
