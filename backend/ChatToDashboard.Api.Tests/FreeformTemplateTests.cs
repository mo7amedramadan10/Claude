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
