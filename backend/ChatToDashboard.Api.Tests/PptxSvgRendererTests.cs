using ChatToDashboard.Api.Decks;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>محرّك PowerPoint → SVG: على ملف PPTX مولَّد ببنّائنا دائمًا، وعلى قالب المستخدم إن وُجد في scratchpad (يكتب SVG لكل صفحة للفحص البصري).</summary>
public class PptxSvgRendererTests
{
    [Fact]
    public void Renders_GeneratedDeck_WithTextAndTable()
    {
        var pptx = DeckPptxBuilder.Build(new DeckPptxRequest
        {
            Title = "اختبار",
            Slides = { new DeckPptxSlideInput { Title = "عنوان", Subtitle = "فرعي", IsTitle = true }, new DeckPptxSlideInput { Title = "جدول", Columns = new() { "أ", "ب" }, Rows = new() { new() { "1", "2" } } } },
        });
        using var r = new PptxSvgRenderer(pptx);
        Assert.Equal(2, r.Slides.Count);
        var s0 = r.RenderSlide(0); var s1 = r.RenderSlide(1);
        Assert.StartsWith("<svg", s0); Assert.Contains("عنوان", s0); Assert.Contains("foreignObject", s0);
        Assert.Contains("<rect", s1); Assert.Contains("أ", s1);
        Assert.Contains("viewBox=\"0 0 1280 720\"", s0);
    }

    [Fact]
    public void UserTemplate_IfPresent_RendersAllPages()
    {
        var path = "/tmp/claude-0/-home-user-Claude/da72bb5f-6107-5fbc-b2dd-f8387c2e28b5/scratchpad/tpl/t.pptx";
        if (!File.Exists(path)) return;
        var outDir = Path.Combine(Path.GetDirectoryName(path)!, "svg"); Directory.CreateDirectory(outDir);
        using var r = new PptxSvgRenderer(File.ReadAllBytes(path));
        Assert.Equal(69, r.Layouts.Count); Assert.Equal(73, r.Slides.Count);
        var log = new List<string>();
        for (var i = 0; i < r.Layouts.Count; i++) { var svg = r.RenderLayout(i); File.WriteAllText(Path.Combine(outDir, $"layout{i + 1}.svg"), svg); log.Add($"layout{i + 1}\t{PptxSvgRenderer.LayoutName(r.Layouts[i])}\t{svg.Length}\t{string.Join(",", r.Unsupported)}"); }
        for (var i = 0; i < r.Slides.Count; i++) { var svg = r.RenderSlide(i); File.WriteAllText(Path.Combine(outDir, $"slide{i + 1}.svg"), svg); log.Add($"slide{i + 1}\tlayout{r.LayoutIndexOfSlide(r.Slides[i]) + 1}\t{svg.Length}\t{string.Join(",", r.Unsupported)}"); }
        File.WriteAllLines(Path.Combine(outDir, "log.tsv"), log);
    }
}

public class DeckBrandTemplateAnalyzeTests
{
    [Fact]
    public void Analyze_GeneratedDeck_HasLayoutsAndSlides()
    {
        var pptx = DeckPptxBuilder.Build(new DeckPptxRequest { Title = "x", Slides = { new DeckPptxSlideInput { Title = "عنوان", IsTitle = true } } });
        var (info, pages) = DeckBrandTemplateStore.Analyze(pptx, "x.pptx", "0123456789abcdef");
        Assert.Single(info.Slides); Assert.NotEmpty(info.Layouts);
        Assert.All(pages, p => Assert.StartsWith("<svg", p.Svg));
        Assert.Equal("/api/decks/brand/template/0123456789abcdef/pages/slide/0.svg", info.Slides[0].SvgUrl);
    }

    [Fact]
    public void Analyze_UserTemplate_IfPresent()
    {
        var path = "/tmp/claude-0/-home-user-Claude/da72bb5f-6107-5fbc-b2dd-f8387c2e28b5/scratchpad/tpl/t.pptx";
        if (!File.Exists(path)) return;
        var bytes = File.ReadAllBytes(path);
        var (info, pages) = DeckBrandTemplateStore.Analyze(bytes, "Arabic_PPT.pptx", "0123456789abcdef");
        Assert.Equal(69, info.Layouts.Count); Assert.Equal(73, info.Slides.Count);
        Assert.Equal(33, info.Layouts.Count(l => l.Generic));
        Assert.Equal("Cover Slide", info.Layouts[0].Name);
        Assert.Contains(info.Layouts[0].Placeholders, p => p.Type == "body" && p.W > 0.3);
        Assert.Contains(info.Fonts, f => f.StartsWith("Tanseek"));
        Assert.Contains(info.ThemeColors, kv => kv.Key == "accent2" && kv.Value == "#33D4AD");
        Assert.Contains(pages, p => p.Svg.Contains("/media/image1.png"));
        var m = DeckBrandTemplateStore.Media(bytes, "image1.png");
        Assert.NotNull(m); Assert.Equal("image/png", m!.Value.ContentType); Assert.True(m.Value.Bytes.Length > 100000);
        Assert.Null(DeckBrandTemplateStore.Media(bytes, "../x"));
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(path)!, "info.json"), System.Text.Json.JsonSerializer.Serialize(info));
        var dir = Path.Combine(Path.GetDirectoryName(path)!, "pages"); Directory.CreateDirectory(dir);
        foreach (var pg in pages) File.WriteAllText(Path.Combine(dir, $"{pg.Kind}-{pg.Index}.svg"), pg.Svg);
    }
}
