using ChatToDashboard.Api.Data;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>استخراج أسماء الجداول من SQL لبصمة البيانات.</summary>
public class DataFingerprintTests
{
    [Fact]
    public void ExtractTables_FindsFromAndJoin_WithBracketsAndSchemas()
    {
        var t = DataFingerprintService.ExtractTables(new[] { "SELECT a FROM [proj_x].[sales] s JOIN dbo.regions r ON 1=1 WHERE x IN (SELECT y FROM orders)", "select * from  [proj_x].[sales]" });
        Assert.Equal(new[] { "dbo.orders", "dbo.regions", "proj_x.sales" }, t);
        Assert.Empty(DataFingerprintService.ExtractTables(new[] { "SELECT 1", "" }));
    }
}
