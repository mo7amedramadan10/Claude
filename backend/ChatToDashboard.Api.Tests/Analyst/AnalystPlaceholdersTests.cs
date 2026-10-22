using ChatToDashboard.Api.Analyst;

namespace ChatToDashboard.Api.Tests.Analyst;

public class AnalystPlaceholdersTests
{
    private static readonly string[] Columns = { "rep_name", "region", "actual" };

    private static List<Dictionary<string, object?>> Rows() => new()
    {
        new() { ["rep_name"] = "أحمد علي", ["region"] = "الوسطى", ["actual"] = 920000d },
        new() { ["rep_name"] = "سارة محمد", ["region"] = "الغربية", ["actual"] = 740000d },
        new() { ["rep_name"] = "محمد خالد", ["region"] = "الشرقية", ["actual"] = 410000d },
    };

    [Fact]
    public void Render_Rows_ReturnsRowCount()
    {
        var result = AnalystPlaceholders.Render("عدد الصفوف: {{rows}}", Columns, Rows(), "actual");
        Assert.Equal("عدد الصفوف: 3", result.Text);
        Assert.Empty(result.MissingKeys);
    }

    [Fact]
    public void Render_SumAndAvg_ComputeFromRealRows()
    {
        var result = AnalystPlaceholders.Render(
            "الإجمالي {{sum.actual}} والمتوسط {{avg.actual}}", Columns, Rows(), "actual");
        Assert.Equal("الإجمالي 2070000 والمتوسط 690000", result.Text);
    }

    [Fact]
    public void Render_TopAndBottom_SortByPrimaryMeasure()
    {
        var result = AnalystPlaceholders.Render(
            "الأعلى {{top1.rep_name}} والأدنى {{bottom1.rep_name}}", Columns, Rows(), "actual");
        Assert.Equal("الأعلى أحمد علي والأدنى محمد خالد", result.Text);
    }

    [Fact]
    public void Render_ShareTop3_ComputesFractionOfTotal()
    {
        // Only 3 rows total, so share_top3 of all of them is exactly 1 (100%).
        var result = AnalystPlaceholders.Render("{{share_top3.actual|p}}", Columns, Rows(), "actual");
        Assert.Equal("100%", result.Text);
    }

    [Theory]
    [InlineData("{{sum.actual|n}}", "2,070,000")]
    [InlineData("{{share_top3.actual|p}}", "100%")]
    [InlineData("{{sum.actual|c}}", "2.1مليون")]
    public void Render_FormatSuffixes_ApplyExpectedFormat(string template, string expected)
    {
        var result = AnalystPlaceholders.Render(template, Columns, Rows(), "actual");
        Assert.Equal(expected, result.Text);
    }

    [Fact]
    public void Render_UnknownColumn_ReportsMissingKeyAndKeepsLiteralToken()
    {
        var result = AnalystPlaceholders.Render("{{sum.not_a_real_column}}", Columns, Rows(), "actual");
        Assert.Equal("{{sum.not_a_real_column}}", result.Text);
        Assert.Contains("sum.not_a_real_column", result.MissingKeys);
    }

    [Fact]
    public void Render_TopBeyondRowCount_ReportsMissingKey()
    {
        // Only 3 rows exist; top5 has no 5th row.
        var result = AnalystPlaceholders.Render("{{top5.rep_name}}", Columns, Rows(), "actual");
        Assert.Contains("top5.rep_name", result.MissingKeys);
    }

    [Theory]
    [InlineData("حقق أحمد أعلى إيراد بقيمة {{top1.actual|n}} ريال")]
    [InlineData("الأعلى {{top3.actual|n}} والأدنى {{bottom2.region}}")] // digits inside placeholder keys only
    public void HasStrayDigits_TemplateWithOnlyPlaceholderDigits_ReturnsFalse(string template)
    {
        Assert.False(AnalystPlaceholders.HasStrayDigits(template));
    }

    [Theory]
    [InlineData("حقق أحمد إيراد 920000 ريال")] // literal Western digits outside any token
    [InlineData("حقق أحمد إيراد ٩٢٠٠٠٠ ريال")] // literal Arabic-Indic digits
    [InlineData("الإجمالي {{sum.actual}} وده أعلى من 50% من المستهدف")] // a real placeholder plus a stray literal number
    public void HasStrayDigits_TemplateWithLiteralNumber_ReturnsTrue(string template)
    {
        Assert.True(AnalystPlaceholders.HasStrayDigits(template));
    }

    [Fact]
    public void HasStrayDigits_NoDigitsAnywhere_ReturnsFalse()
    {
        Assert.False(AnalystPlaceholders.HasStrayDigits("مفيش أي أرقام هنا خالص"));
    }
}
