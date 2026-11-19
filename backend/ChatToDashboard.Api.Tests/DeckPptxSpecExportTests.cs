using ChatToDashboard.Api.Decks;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>تصدير PowerPoint من مواصفة عرض بلا متصفح (صفحة العرض المنشور): الرسم جدولًا بقيمه، وباقي التخطيطات كما في الواجهة.</summary>
public class DeckPptxSpecExportTests
{
    [Fact]
    public void SlidesFromSpec_MapsLayouts_AndChartBecomesTable()
    {
        var spec = new DeckSpec
        {
            Title = "عرض", Slides = new()
            {
                new DeckSlide { Layout = "title", Title = "الغلاف", Subtitle = "فرعي" },
                new DeckSlide { Layout = "chart", Title = "المبيعات", Chart = new DeckChart { Type = "bar", Labels = new() { "أ", "ب" }, Values = new() { 1000, 2.5 }, Unit = "ريال" } },
                new DeckSlide { Layout = "kpis", Title = "مؤشرات", Kpis = new() { new DeckKpi { Label = "x", Value = "1" } } },
                new DeckSlide { Layout = "tasks", Title = "مهام", Tasks = new() { new DeckTask { T = "أ", Done = true }, new DeckTask { T = "ب" } } },
                new DeckSlide { Layout = "two-column", Title = "عمودان", Columns = new() { new DeckColumn { Title = "ق1", Bullets = new() { "n1" } } } },
            }
        };
        var slides = DeckPptxBuilder.SlidesFromSpec(spec);
        Assert.Equal(5, slides.Count);
        Assert.True(slides[0].IsTitle);
        Assert.Equal(new[] { "", "ريال" }, slides[1].Columns);
        Assert.Equal(new[] { "أ", "1,000" }, slides[1].Rows![0]);
        Assert.Equal(new[] { "ب", "2.50" }, slides[1].Rows![1]);
        Assert.Single(slides[2].Kpis!);
        Assert.Equal(new[] { "☑ أ", "☐ ب" }, slides[3].Bullets);
        Assert.Equal(new[] { "ق1:", "   n1" }, slides[4].Bullets);
        var bytes = DeckPptxBuilder.Build(new DeckPptxRequest { Title = "عرض", Slides = slides });
        Assert.True(bytes.Length > 2000);
    }
}
