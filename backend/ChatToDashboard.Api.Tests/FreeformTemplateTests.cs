using System.Text.Json;
using ChatToDashboard.Api.Freeform;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>نماذج تصميم حر: الكتالوج (مفاتيح فريدة، أنواع صحيحة، أقسام)، الموجز للموديل، ودخوله في نظام الموديل.</summary>
public class FreeformTemplateTests
{
    [Fact]
    public void Catalog_IsConsistent()
    {
        var all = FreeformTemplates.All;
        Assert.True(all.Count >= 25);
        Assert.Equal(all.Count, all.Select(t => t.Key).Distinct().Count());
        Assert.All(all, t => { Assert.Contains(t.Kind, FreeformStore.Kinds); Assert.NotEmpty(t.Sections); Assert.False(string.IsNullOrWhiteSpace(t.Brief)); Assert.False(string.IsNullOrWhiteSpace(t.Sketch)); });
        foreach (var k in FreeformStore.Kinds) Assert.Contains(all, t => t.Kind == k);
        Assert.Equal(4, FreeformTemplates.Find("db-sales")!.DataSections);
        Assert.Null(FreeformTemplates.Find("nope")); Assert.Null(FreeformTemplates.Find(null));
    }

    [Fact]
    public void StaticTemplates_AllHaveEnglishNamesAndSections()
    {
        // كل نموذج ثابت له ترجمة كاملة (اسم ووصف وعناوين أقسامه بالترتيب) فتُعرض معاينته بالإنجليزية
        foreach (var t in FreeformTemplates.All)
        {
            Assert.True(t.HasEn, $"نموذج بلا ترجمة إنجليزية كاملة: {t.Key}");
            Assert.False(string.IsNullOrWhiteSpace(t.DescriptionEn), t.Key);
        }
        var en = FreeformTemplates.Find("sl-qbr")!;
        Assert.Equal("Quarterly Business Review", en.NameEn);
        Assert.Equal("Key Results", en.Sections[1].TitleEn);
        Assert.Contains("(Key Results)", FreeformTemplates.BriefFor(en, "en"));
        Assert.DoesNotContain("(Key Results)", FreeformTemplates.BriefFor(en));
        Assert.Equal("en", FreeformBuilder.LanguageOf(null, "<!doctype html><html lang=\"en\" dir=\"ltr\"><head>"));
        Assert.Equal("ar", FreeformBuilder.LanguageOf(null, "<!doctype html><html lang=\"ar\" dir=\"rtl\">"));
        Assert.Equal("en", FreeformBuilder.LanguageOf("en", null));
        Assert.Contains("لغة الصفحة: الإنجليزية", FreeformBuilder.BuildSystemPrompt(null, false, "", "doc", null, "en"));
    }

    [Fact]
    public void PageTemplates_PointToExistingFiles_WithSlidesAndNavigation()
    {
        // نموذج الصفحة الجاهزة: الملف موجود تحت wwwroot، فيه شرائح بعدد الأقسام ودالة التنقّل وشريط المصغّرات
        var pages = FreeformTemplates.All.Where(t => t.Page is not null).ToList();
        Assert.Contains(pages, t => t.Key == "sl-corporate-blue");
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "ChatToDashboard.Api", "wwwroot"))) dir = dir.Parent;
        Assert.NotNull(dir);
        foreach (var t in pages)
        {
            Assert.DoesNotContain("..", t.Page!);
            var file = Path.Combine(dir!.FullName, "ChatToDashboard.Api", "wwwroot", t.Page!.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(file), $"ملف النموذج مفقود: {t.Page}");
            if (t.PageEn is not null)
            {
                var fileEn = Path.Combine(dir!.FullName, "ChatToDashboard.Api", "wwwroot", t.PageEn.Replace('/', Path.DirectorySeparatorChar));
                Assert.True(File.Exists(fileEn), $"ملف النموذج الإنجليزي مفقود: {t.PageEn}");
                var htmlEn = File.ReadAllText(fileEn);
                Assert.Contains("<html lang=\"en\" dir=\"ltr\">", htmlEn);
                Assert.Equal(t.Sections.Count, System.Text.RegularExpressions.Regex.Matches(htmlEn, "<section class=\"slide").Count);
            }
            var html = File.ReadAllText(file);
            Assert.Equal(t.Sections.Count, System.Text.RegularExpressions.Regex.Matches(html, "<section class=\"slide").Count);
            Assert.Contains("window.goToSlide", html);
            Assert.Contains("/js/jeem-slides.js", html);
            Assert.Contains("--jeem-strip-h", html);
            Assert.Contains("النموذج المختار", FreeformTemplates.BriefFor(t));
        }
    }

    [Fact]
    public void Brief_ListsSectionsInOrder_AndEntersSystemPrompt()
    {
        var t = FreeformTemplates.Find("doc-report")!;
        var brief = FreeformTemplates.BriefFor(t);
        Assert.Contains("1. الملخص التنفيذي", brief); Assert.Contains("2. المؤشرات الرئيسية — من بيانات المشروع", brief);
        var sys = FreeformBuilder.BuildSystemPrompt(null, false, "", "doc", t);
        Assert.Contains("النموذج المختار: تقرير أداء دوري", sys);
        Assert.DoesNotContain("النموذج المختار", FreeformBuilder.BuildSystemPrompt(null, false, "", "doc"));
    }

    /// <summary>يكتب الكتالوج JSON للخادم الوهمي في scratchpad الجلسة (إن وُجد المجلد) ليختبر المعرض في المتصفح بنفس البيانات.</summary>
    [Fact]
    public void Catalog_SerializesWithCamelCase_AndDumpsForMock()
    {
        var json = JsonSerializer.Serialize(FreeformTemplates.All);
        Assert.Contains("\"dataSections\":", json); Assert.Contains("\"sketch\":", json);
        var dir = "/tmp/claude-0/-home-user-Claude/da72bb5f-6107-5fbc-b2dd-f8387c2e28b5/scratchpad/decks";
        if (Directory.Exists(dir)) File.WriteAllText(Path.Combine(dir, "ff-templates.json"), json);
    }
}
