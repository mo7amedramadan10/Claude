using System.Text.RegularExpressions;
using ChatToDashboard.Api.Decks;
using ChatToDashboard.Api.Freeform;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>قالب ثقة المؤسسي (من PowerPoint كما هو) ورسم الرسوم البيانية في محرّك PowerPoint → SVG.</summary>
public class ThiqahTemplateTests
{
    private static string WebRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "ChatToDashboard.Api", "wwwroot"))) dir = dir.Parent;
        return Path.Combine(dir!.FullName, "ChatToDashboard.Api", "wwwroot");
    }

    [Fact]
    public void ThiqahTemplate_Loads_InThiqahCategory_WithLocalAssetsOnly()
    {
        var root = WebRoot();
        var t = FreeformFolderTemplates.Load(root).Templates.Single(x => x.Key == "sl-thiqah-corporate");
        Assert.Equal("thiqah", t.Category);
        Assert.Equal(73, t.Sections.Count);
        foreach (var (page, n, dir) in new[] { (t.Page!, 73, "rtl"), (t.PageEn!, 88, "ltr") })
        {
            var html = File.ReadAllText(Path.Combine(root, page));
            Assert.Contains($"dir=\"{dir}\"", html);
            Assert.Equal(n, Regex.Matches(html, "<section class=\"slide").Count);
            Assert.DoesNotContain("data:image", html);
            Assert.DoesNotContain("غير مدعومة", html);           // صور EMF حُوّلت PNG
            Assert.DoesNotContain(">رسم بياني<", html);           // الرسوم البيانية مرسومة لا أماكن محجوزة
            Assert.Contains("window.goToSlide", html);
            Assert.Contains(dir == "rtl" ? "عنوان العرض" : "Headline", html);   // أماكن الكتابة الفارغة بنص مطالبة التخطيط
            foreach (Match m in Regex.Matches(html, "/templates/freeform/sl-thiqah-corporate/assets/([0-9a-f]+\\.png)"))
                Assert.True(File.Exists(Path.Combine(root, "templates", "freeform", "sl-thiqah-corporate", "assets", m.Groups[1].Value)), m.Value);
        }
    }

    [Fact]
    public void ChartsInUserDeck_RenderAsShapes_NotPlaceholders()
    {
        const string path = "/tmp/claude-0/-home-user-Claude/da72bb5f-6107-5fbc-b2dd-f8387c2e28b5/scratchpad/thq/ar.pptx";
        if (!File.Exists(path)) return;
        using var r = new PptxSvgRenderer(File.ReadAllBytes(path));
        var doughnuts = r.RenderSlide(6);      // الشريحة 7: أربع حلقات
        Assert.DoesNotContain("رسم بياني", doughnuts);
        Assert.True(Regex.Matches(doughnuts, "<path d=\"M[^\"]* A").Count >= 4);
        var bars = r.RenderSlide(13);          // الشريحة 14: أعمدة وأشرطة
        Assert.DoesNotContain("رسم بياني", bars);
        Assert.Contains("Category 1", bars);
    }

    [Fact]
    public void EmptySlidePlaceholders_RenderLayoutPrompt_OnlyWhenRequested()
    {
        const string path = "/tmp/claude-0/-home-user-Claude/da72bb5f-6107-5fbc-b2dd-f8387c2e28b5/scratchpad/thq/ar.pptx";
        if (!File.Exists(path)) return;
        var bytes = File.ReadAllBytes(path);
        using (var plain = new PptxSvgRenderer(bytes)) Assert.DoesNotContain("عنوان العرض", plain.RenderSlide(0));   // السلوك المعتاد: الفارغ لا يُرسم
        using var r = new PptxSvgRenderer(bytes) { SlidePromptLang = "ar" };
        var cover = r.RenderSlide(0);
        Assert.Contains("عنوان العرض", cover);
        Assert.DoesNotContain("opacity:.45", cover);   // نص حقيقي لا باهت
    }
}
