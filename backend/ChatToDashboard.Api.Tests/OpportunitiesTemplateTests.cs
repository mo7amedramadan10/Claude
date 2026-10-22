using ChatToDashboard.Api.Templates;

namespace ChatToDashboard.Api.Tests;

/// <summary>يتحقق من نموذج «متتبع الفرص والعروض» (§4.24) في كتالوج النماذج: معرّف فريد،
/// ثلاث صفحات بترتيبها، وعنصر stages في صفحة المراحل، وعناوين غير مكررة (المطابقة بين رد
/// النموذج والصفحات تتم بالعنوان الحرفي).</summary>
public class OpportunitiesTemplateTests
{
    private static BuiltinDashboardTemplate Template =>
        BuiltinTemplates.Dashboards.Single(d => d.Id == "pl-opportunities-tracker");

    [Fact]
    public void CatalogIdsStayUnique() =>
        Assert.Equal(BuiltinTemplates.Dashboards.Count, BuiltinTemplates.Dashboards.Select(d => d.Id).Distinct().Count());

    [Fact]
    public void HasThreePagesInOrderWithAStagesWidget()
    {
        var pages = Template.Widgets.Select(w => w.Page).Distinct().ToList();
        Assert.Equal(new[] { "الملخص", "مراحل الفرص", "التفاصيل" }, pages);
        var stages = Assert.Single(Template.Widgets, w => w.Type == "stages");
        Assert.Equal("مراحل الفرص", stages.Page);
        Assert.Equal("sales", Template.Category);
    }

    [Fact]
    public void WidgetTitlesAreUnique() =>
        Assert.Equal(Template.Widgets.Length, Template.Widgets.Select(w => w.Title).Distinct().Count());
}
