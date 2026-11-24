using ChatToDashboard.Api.Freeform;
using Xunit;

namespace ChatToDashboard.Api.Tests;

public class FreeformScopeTests
{
    private const string Page = """
        <!doctype html><html><head><style>.slide{width:1280px}.card{padding:8px}.big{font-size:40px}</style></head><body>
        <section class="slide on"><h1 class="big">الأولى</h1><section class="inner">داخلية</section></section>
        <section class="slide"><div class="card">ب</div><script>var s='<section class="slide">';</script></section>
        <div class="slide-nav">تنقل</div>
        <section class="slide"><p>ج</p></section>
        </body></html>
        """;

    [Fact]
    public void FindSlides_TopLevelOnly_IgnoresScriptsAndLookalikeClasses()
    {
        var slides = FreeformScope.FindSlides(Page);
        Assert.Equal(3, slides.Count);
        Assert.Contains("الأولى", Page[slides[0].Start..slides[0].End]);
        Assert.EndsWith("</section>", Page[slides[0].Start..slides[0].End]);
        Assert.Contains("داخلية", Page[slides[0].Start..slides[0].End]); // القسم الداخلي جزء من الأولى لا شريحة مستقلة
        Assert.Contains("<script>", Page[slides[1].Start..slides[1].End]);
        Assert.Contains("<p>ج</p>", Page[slides[2].Start..slides[2].End]);
        Assert.Empty(FreeformScope.FindSlides("<div class='slide'>واحدة</div>"));
    }

    [Fact]
    public void Splice_ChangesOnlyTheTargetSlide()
    {
        var slides = FreeformScope.FindSlides(Page); var orig = Page[slides[1].Start..slides[1].End];
        var (frag, err) = FreeformScope.CheckFragment(orig, "<section class=\"slide\"><div class=\"card\">جديد</div></section>", FreeformScope.KnownClasses(Page));
        Assert.Null(err);
        var html = FreeformScope.Splice(Page, slides[1], frag!);
        Assert.Contains("جديد", html);
        Assert.Equal(Page[..slides[1].Start], html[..slides[1].Start]);
        Assert.EndsWith(Page[slides[1].End..], html);
    }

    [Fact]
    public void CheckFragment_EnforcesTemplate()
    {
        var known = FreeformScope.KnownClasses(Page);
        const string orig = "<section class=\"slide\" style=\"--x:1\"><p>ب</p></section>";
        // الوسم الجذر الأصلي يُعاد كما هو حتى لو غيّره الموديل، وتُزال الأنماط والسكربتات
        var (f, e) = FreeformScope.CheckFragment(orig, "```html\n<section class=\"slide big\" style=\"background:red\"><style>p{color:red}</style><p>ب2</p><script>x()</script></section>\n```", known);
        Assert.Null(e);
        Assert.StartsWith("<section class=\"slide\" style=\"--x:1\">", f);
        Assert.DoesNotContain("<style", f); Assert.DoesNotContain("<script", f); Assert.Contains("ب2", f);
        // صنف جديد غير موجود في القالب → رفض
        Assert.NotNull(FreeformScope.CheckFragment(orig, "<section class=\"slide\"><div class=\"fancy-new\">x</div></section>", known).Error);
        // وسم جذر مختلف أو أكثر من عنصر → رفض
        Assert.NotNull(FreeformScope.CheckFragment(orig, "<div class=\"slide\">x</div>", known).Error);
        Assert.NotNull(FreeformScope.CheckFragment(orig, "<section class=\"slide\">x</section><section class=\"slide\">y</section>", known).Error);
        // رد فارغ = سؤال بلا تعديل
        Assert.Equal((null, null), FreeformScope.CheckFragment(orig, "  ", known));
    }

    [Fact]
    public void FindMarked_AndLogoStripping()
    {
        const string html = "<div><ul class=\"list\" data-jeem-target=\"1\"><li>أ</li><li><ul><li>ب</li></ul></li></ul><p>بعد</p><img data-jeem-logo=\"\" src=\"data:image/png;base64,AAA\" alt=\"x\"></div>";
        var r = FreeformScope.FindMarked(html);
        Assert.NotNull(r);
        Assert.EndsWith("</ul></li></ul>", html[r!.Value.Start..r.Value.End]);
        Assert.DoesNotContain("src=", FreeformScope.StripLogoSrc(html).Split("<img")[1]);
        Assert.Contains("أعده هو وحده", FreeformBuilder.FragmentRules("الشريحة 2 من 5"));
    }

    [Fact]
    public void FindSlides_MatchesSectionCount_OnAllRealSlideTemplates()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "ChatToDashboard.Api", "wwwroot"))) dir = dir.Parent;
        var root = Path.Combine(dir!.FullName, "ChatToDashboard.Api", "wwwroot", "templates", "freeform");
        var checkedCount = 0;
        foreach (var folder in Directory.GetDirectories(root))
        {
            var meta = Path.Combine(folder, "template.json"); var ar = Path.Combine(folder, "ar.html");
            if (!File.Exists(meta) || !File.Exists(ar)) continue;
            using var doc = System.Text.Json.JsonDocument.Parse(File.ReadAllText(meta));
            if (doc.RootElement.GetProperty("kind").GetString() != "slides") continue;
            var expected = doc.RootElement.GetProperty("sections").GetArrayLength(); if (expected < 2) continue; // شريحة واحدة تُعامل صفحةً عادية (لا نطاق شريحة)
            var html = File.ReadAllText(ar);
            var slides = FreeformScope.FindSlides(html);
            Assert.True(expected == slides.Count, $"{Path.GetFileName(folder)}: {slides.Count} بدل {expected}");
            // تركيب الشريحة نفسها مكانها يعيد الصفحة حرفيًا
            var mid = slides[slides.Count / 2];
            Assert.Equal(html, FreeformScope.Splice(html, mid, html[mid.Start..mid.End]));
            checkedCount++;
        }
        Assert.True(checkedCount >= 10);
    }
}
