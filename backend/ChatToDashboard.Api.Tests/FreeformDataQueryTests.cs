using ChatToDashboard.Api.Build;
using ChatToDashboard.Api.Freeform;
using Xunit;

namespace ChatToDashboard.Api.Tests;

public class FreeformDataQueryTests
{
    private static readonly DataProfile Profile = new(new[]
    {
        new ProfiledTable("proj_1.sales", "المبيعات.xlsx", 240, new[]
        {
            new ProfiledColumn("Region", "nvarchar", "text", null, false, null, null, 0),
            new ProfiledColumn("Revenue", "float", "number", null, false, "1", "9", 0),
            new ProfiledColumn("Order Date", "datetime2", "date", null, false, null, null, 0),
            new ProfiledColumn("x]y", "nvarchar", "text", null, false, null, null, 0),
        }, Array.Empty<IReadOnlyDictionary<string, string>>()),
    });

    [Fact]
    public void Builds_GroupedAndTemporalAndSingleValue()
    {
        var (q, e) = FreeformDataQuery.Build(Profile, new() { Table = "proj_1.sales", Agg = "sum", Measure = "revenue", Dimension = "Region" });
        Assert.Null(e);
        Assert.Equal("SELECT TOP (12) [Region] AS [Region], SUM(TRY_CONVERT(float, [Revenue])) AS [إجمالي Revenue] FROM [proj_1].[sales] WHERE [Region] IS NOT NULL GROUP BY [Region] ORDER BY 2 DESC", q!.Sql);
        Assert.Equal("Region", q.DimensionAlias); Assert.Equal("إجمالي Revenue", q.MeasureAlias);

        var (t, _) = FreeformDataQuery.Build(Profile, new() { Table = "proj_1.sales", Agg = "count", Dimension = "Order Date", Grain = "quarter", Limit = 8 });
        Assert.True(t!.Temporal);
        Assert.Contains("CONCAT(YEAR(TRY_CONVERT(date, [Order Date])), '-Q'", t.Sql);
        Assert.Contains("TOP (8)", t.Sql); Assert.EndsWith("ORDER BY 1 ASC", t.Sql); Assert.Contains("COUNT(*) AS [عدد السجلات]", t.Sql);

        var (v, _) = FreeformDataQuery.Build(Profile, new() { Table = "proj_1.sales", Agg = "avg", Measure = "Revenue" });
        Assert.Equal("SELECT AVG(TRY_CONVERT(float, [Revenue])) AS [متوسط Revenue] FROM [proj_1].[sales]", v!.Sql);

        // المعرّف بقوس مغلق يُهرَّب
        var (z, _) = FreeformDataQuery.Build(Profile, new() { Table = "proj_1.sales", Agg = "count_distinct", Measure = "x]y" });
        Assert.Contains("COUNT(DISTINCT [x]]y])", z!.Sql);
    }

    [Fact]
    public void Rejects_UnknownTableColumnsAndAggs()
    {
        Assert.NotNull(FreeformDataQuery.Build(Profile, new() { Table = "dbo.AppUsers", Agg = "count" }).Error);
        Assert.NotNull(FreeformDataQuery.Build(Profile, new() { Table = "proj_1.sales", Agg = "sum", Measure = "Revenue; DROP TABLE x" }).Error);
        Assert.NotNull(FreeformDataQuery.Build(Profile, new() { Table = "proj_1.sales", Agg = "sum", Measure = "Revenue", Dimension = "Nope" }).Error);
        Assert.NotNull(FreeformDataQuery.Build(Profile, new() { Table = "proj_1.sales", Agg = "median", Measure = "Revenue" }).Error);
        Assert.NotNull(FreeformDataQuery.Build(Profile, new() { Table = "proj_1.sales", Agg = "sum" }).Error);
        Assert.Contains("TOP (60)", FreeformDataQuery.Build(Profile, new() { Table = "proj_1.sales", Agg = "count", Dimension = "Region", Limit = 5000 }).Query!.Sql);
    }
}
