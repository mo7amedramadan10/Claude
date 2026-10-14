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

    [Fact]
    public void DashboardWidget_KeepsMultiSeriesExtensionsThroughRoundTrip()
    {
        const string json = """{"type":"bar","title":"مخطط مقابل فعلي","data":[{"q":"Q1","plan":1,"actual":2}],"xKey":"q","series":[{"key":"plan","label":"الخطة"},{"key":"actual","label":"الفعلي"}],"stacked":true,"areaFill":false,"centerLabel":"مشروع","source":"أ. ب."}""";
        var w = System.Text.Json.JsonSerializer.Deserialize<ChatToDashboard.Api.Models.DashboardWidget>(json)!;
        var back = System.Text.Json.JsonSerializer.Serialize(w);
        Assert.Equal(2, w.Series!.Count);
        // System.Text.Json يهرّب العربية افتراضيًا، فنتحقق بإعادة القراءة لا بمطابقة النص.
        var again = System.Text.Json.JsonSerializer.Deserialize<ChatToDashboard.Api.Models.DashboardWidget>(back)!;
        Assert.Equal("الخطة", again.Series![0].Label);
        Assert.Equal("plan", again.Series[0].Key);
        Assert.True(again.Stacked);
        Assert.False(again.AreaFill);
        Assert.Equal("مشروع", again.CenterLabel);
        var plain = System.Text.Json.JsonSerializer.Serialize(new ChatToDashboard.Api.Models.DashboardWidget { Type = "kpi", Title = "x", Data = System.Text.Json.JsonDocument.Parse("[]").RootElement });
        Assert.DoesNotContain("series", plain);
    }
}

/// <summary>التخمين الحتمي لطبقة المعنى من أسماء الأعمدة (Semantics/ConceptCatalog).</summary>
public class ConceptCatalogTests
{
    [Theory]
    [InlineData("Revenue", "revenue", 0.8)]
    [InlineData("الإيراد", "revenue", 0.8)]
    [InlineData("OrderDate", "order_date", 0.8)]
    [InlineData("order_date", "order_date", 0.8)]
    [InlineData("Status", "deal_status", 0.8)]
    [InlineData("SalesRep", "owner", 0.8)]
    [InlineData("Total_Revenue_SAR", "revenue", 0.5)]
    [InlineData("مدير الحساب", "owner", 0.8)]
    public void GuessFromName_MapsCommonNames(string column, string concept, double confidence)
    {
        var guess = ChatToDashboard.Api.Semantics.ConceptCatalog.GuessFromName(column);
        Assert.NotNull(guess);
        Assert.Equal(concept, guess!.Value.Concept.Id);
        Assert.Equal(confidence, guess.Value.Confidence);
    }

    [Theory]
    [InlineData("xyz123")]
    [InlineData("")]
    public void GuessFromName_ReturnsNullWhenNothingMatches(string column) =>
        Assert.Null(ChatToDashboard.Api.Semantics.ConceptCatalog.GuessFromName(column));

    [Fact]
    public void Catalog_IdsAreUniqueAndKnown()
    {
        var ids = ChatToDashboard.Api.Semantics.ConceptCatalog.Concepts.Select(c => c.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.True(ChatToDashboard.Api.Semantics.ConceptCatalog.IsKnown("DEAL_STATUS"));
        Assert.False(ChatToDashboard.Api.Semantics.ConceptCatalog.IsKnown("made_up"));
    }
}

/// <summary>الجدوى الحتمية لعناصر القوالب من عناوينها (Semantics/WidgetRequirements).</summary>
public class WidgetRequirementTests
{
    private static readonly IReadOnlySet<string> SalesFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "order_date", "region", "product", "category", "quantity", "unit_price", "revenue", "deal_status", "owner" };

    [Theory]
    [InlineData("قيمة الأنبوب", true)]
    [InlineData("نسبة الفوز", true)]
    [InlineData("الإيراد حسب المنطقة", true)]
    [InlineData("المبيعات شهريًا", true)]
    [InlineData("الأنبوب المرجّح", false)]
    [InlineData("تحقيق الحصة", false)]
    [InlineData("الخسارة حسب السبب", false)]
    [InlineData("الصفقات حسب المرحلة", true)] // حالة الصفقة تصلح بديلًا للمرحلة
    [InlineData("أعمار الأنبوب", false)]
    [InlineData("حسب مصدر العميل", false)]
    public void Evaluate_AgainstPlainSalesFile(string title, bool feasible)
    {
        var reqs = ChatToDashboard.Api.Semantics.WidgetRequirementResolver.Resolve(title);
        var f = ChatToDashboard.Api.Semantics.WidgetRequirementResolver.Evaluate(reqs, SalesFile);
        Assert.Equal(feasible, f.Feasible);
        if (!feasible) Assert.NotEmpty(f.Missing);
    }

    [Fact]
    public void Evaluate_UnknownWhenNothingAnalyzedYet()
    {
        var reqs = ChatToDashboard.Api.Semantics.WidgetRequirementResolver.Resolve("الأنبوب المرجّح");
        Assert.True(ChatToDashboard.Api.Semantics.WidgetRequirementResolver.Evaluate(reqs, new HashSet<string>()).Feasible);
    }

    [Fact]
    public void Resolve_SoftRequirementsNeverReject()
    {
        var reqs = ChatToDashboard.Api.Semantics.WidgetRequirementResolver.Resolve("الإيراد حسب المنتج");
        Assert.All(reqs, r => Assert.False(r.Hard));
        var f = ChatToDashboard.Api.Semantics.WidgetRequirementResolver.Evaluate(reqs, new HashSet<string> { "quantity" });
        Assert.True(f.Feasible);
    }

    [Fact]
    public void Resolve_UsesDraftGroupColumnAsSoftHint()
    {
        var reqs = ChatToDashboard.Api.Semantics.WidgetRequirementResolver.Resolve("حسب", null, "SELECT Region, SUM(Amount) AS value FROM staging_sales GROUP BY Region");
        Assert.Contains(reqs, r => r.AnyOf.Contains("region") && !r.Hard);
    }
}

public class SqlColumnRewriterTests
{
    private static string Q(string n) => $"\"{n}\"";

    [Fact]
    public void Rewrite_ReplacesBareAndQuotedIdentifiers_NotAliasesOrLiterals()
    {
        var sql = "SELECT Region, SUM(Revenue) AS Revenue FROM sales WHERE Status = 'Revenue' AND \"Revenue\" > 0 GROUP BY Region";
        var map = new Dictionary<string, string> { ["Revenue"] = "Sales", ["Region"] = "City Name" };
        var result = ChatToDashboard.Api.Widgets.SqlColumnRewriter.Rewrite(sql, map, Q);
        Assert.Equal("SELECT \"City Name\", SUM(Sales) AS Revenue FROM sales WHERE Status = 'Revenue' AND \"Sales\" > 0 GROUP BY \"City Name\"", result);
    }

    [Fact]
    public void Rewrite_DoesNotTouchFunctionNamesOrPartialWords()
    {
        var sql = "SELECT DATE(OrderDate) AS d, Revenue2, Revenue FROM t";
        var map = new Dictionary<string, string> { ["Date"] = "X", ["Revenue"] = "Y" };
        Assert.Equal("SELECT DATE(OrderDate) AS d, Revenue2, Y FROM t", ChatToDashboard.Api.Widgets.SqlColumnRewriter.Rewrite(sql, map, Q));
    }

    [Fact]
    public void UsedColumns_FindsOnlyReferencedColumns_IgnoringLiterals()
    {
        var sql = "SELECT Region, SUM(Revenue) AS v FROM sales WHERE Status = 'Product'";
        var used = ChatToDashboard.Api.Widgets.SqlColumnRewriter.UsedColumns(sql, new[] { "Region", "Revenue", "Product", "Status", "Quantity" });
        Assert.Equal(new[] { "Region", "Revenue", "Status" }, used);
    }
}
