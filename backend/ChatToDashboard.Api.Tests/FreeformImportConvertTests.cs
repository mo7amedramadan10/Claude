using ChatToDashboard.Api.Decks;
using ChatToDashboard.Api.Freeform;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>تحويل ملف مقروء إلى نموذج تصميم حر مخصص، وإدراج محتواه في موجز الموديل.</summary>
public class FreeformImportConvertTests
{
    [Fact]
    public void FromDocument_BuildsSectionsFromPages_AndKeepsContent()
    {
        var p1 = new ImportedPage { Title = "نتائج الربع" }; p1.Paragraphs.Add("ارتفع الإيراد 12٪"); p1.Tables.Add(new DeckTable { Columns = new() { "البند", "القيمة" }, Rows = new() { new() { "إيراد", "1,200" } } });
        var p2 = new ImportedPage(); p2.Paragraphs.Add("التوصيات للربع القادم"); p2.Paragraphs.Add("نص طويل جدًا " + new string('x', 100));
        var p3 = new ImportedPage(); // فارغة تُهمل
        var doc = new ImportedDocument("pptx", "عرض النتائج", new() { p1, p2, p3 }, new());
        var t = FreeformTemplateStore.FromDocument(doc, "slides", null, "results.pptx");
        Assert.Equal("slides", t.Kind); Assert.Equal("عرض النتائج", t.Name); Assert.True(t.Custom); Assert.Equal("results.pptx", t.SourceFile);
        Assert.Equal(2, t.Sections.Count); Assert.Equal("نتائج الربع", t.Sections[0].Title); Assert.True(t.Sections[0].NeedsData);
        Assert.Equal("التوصيات للربع القادم", t.Sections[1].Title); Assert.False(t.Sections[1].NeedsData);
        Assert.Contains("جدول: البند | القيمة", t.Content); Assert.Contains("- ارتفع الإيراد 12٪", t.Content);
        var brief = FreeformTemplates.BriefFor(t);
        Assert.Contains("محتوى الملف المرفوع", brief); Assert.Contains("1. نتائج الربع", brief);
        Assert.Contains("نتائج الربع", FreeformBuilder.BuildSystemPrompt(null, false, "", "slides", t));
    }

    [Fact]
    public void FromDocument_UsesGivenName_AndTruncatesContent()
    {
        var pages = Enumerable.Range(0, 300).Select(i => { var p = new ImportedPage { Title = $"قسم {i}" }; for (var k = 0; k < 40; k++) p.Paragraphs.Add(new string('ن', 60)); return p; }).ToList();
        var t = FreeformTemplateStore.FromDocument(new ImportedDocument("pdf", "x", pages, new()), "doc", "اسمي", "a.pdf");
        Assert.Equal("اسمي", t.Name); Assert.True(t.Content!.Length <= FreeformTemplateStore.MaxContentChars + 20); Assert.EndsWith("…(اقتُطع)", t.Content);
        Assert.True(t.Sections.Count < 300);
    }
}
