using ChatToDashboard.Api.Build;

namespace ChatToDashboard.Api.Tests;

/// <summary>الأجزاء الحتمية من البناء المرحلي (§ البناء المرحلي في CLAUDE.md): كتالوج أشكال العناصر،
/// واستنتاج نوع العمود، ونص ملف تعريف البيانات الذي يُرسل للموديل.</summary>
public class StagedBuildTests
{
    [Theory]
    [InlineData("kpi", "kpi")]
    [InlineData("STAGES", "stages")]
    [InlineData("cbars", "bar")]
    [InlineData("hbars", "bar")]
    [InlineData("donut", "pie")]
    [InlineData("area", "line")]
    [InlineData("funnel", "table")]
    [InlineData("hero", "kpi")]
    [InlineData(null, "table")]
    [InlineData("غير معروف", "table")]
    public void NormalizeType_MapsPreviewTypesToRealOnes(string? input, string expected) =>
        Assert.Equal(expected, WidgetShapeCatalog.NormalizeType(input));

    [Fact]
    public void Describe_FallsBackToTableForUnknownType() =>
        Assert.Equal(WidgetShapeCatalog.Describe("table"), WidgetShapeCatalog.Describe("مجهول"));

    [Theory]
    [InlineData("INTEGER", "number")]
    [InlineData("DECIMAL(18,4)", "number")]
    [InlineData("BIT", "number")]
    [InlineData("DATETIME2", "date")]
    [InlineData("TEXT", "text")]
    [InlineData("NVARCHAR(MAX)", "text")]
    public void KindFromSqlType_ClassifiesColumns(string sqlType, string expected) =>
        Assert.Equal(expected, DataProfileService.KindFromSqlType(sqlType));

    [Fact]
    public void ToPromptText_RendersValuesRangesAndSamplesCompactly()
    {
        var profile = new DataProfile(new[]
        {
            new ProfiledTable("staging_sales", "ملف المبيعات", 40, new[]
            {
                new ProfiledColumn("Region", "TEXT", "text", new[] { ("الرياض", 20L), ("جدة", 20L) }, false, null, null, 0),
                new ProfiledColumn("Revenue", "REAL", "number", null, false, "10", "999.5", 2),
                new ProfiledColumn("OrderDate", "TEXT", "date", null, false, "2024-01-01", "2024-12-31", 0),
                new ProfiledColumn("Note", "TEXT", "text", new[] { ("أ", 1L) }, true, null, null, 0),
            }, new[] { (IReadOnlyDictionary<string, string>)new Dictionary<string, string> { ["Region"] = "الرياض", ["Revenue"] = "120" } }),
        });

        var text = profile.ToPromptText();

        Assert.Contains("## الجدول staging_sales (المصدر: ملف المبيعات) — 40 صف", text);
        Assert.Contains("- Region [text] القيم: الرياض (20)، جدة (20)", text);
        Assert.Contains("- Revenue [number] المدى: 10 → 999.5 (فارغ في 2 صف)", text);
        Assert.Contains("- Note [text] أمثلة: أ", text);
        Assert.Contains("{Region: الرياض, Revenue: 120}", text);
        Assert.NotNull(profile.Find("STAGING_SALES"));
        Assert.Null(profile.Find("other"));
    }

    [Fact]
    public void ToPromptText_TruncatesWhenOverBudget()
    {
        var tables = Enumerable.Range(0, 50).Select(i => new ProfiledTable($"t{i}", null, 1,
            Enumerable.Range(0, 30).Select(c => new ProfiledColumn($"c{c}", "TEXT", "text", null, false, null, null, 0)).ToList(),
            Array.Empty<IReadOnlyDictionary<string, string>>())).ToList();
        var text = new DataProfile(tables).ToPromptText(maxChars: 2000);
        Assert.Contains("اختُصر ملف التعريف", text);
        Assert.True(text.Length < 4000);
    }
}
