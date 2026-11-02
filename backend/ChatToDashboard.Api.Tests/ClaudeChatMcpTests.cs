using ChatToDashboard.Api.ClaudeChat.Mcp;
using Microsoft.Extensions.Time.Testing;

namespace ChatToDashboard.Api.Tests;

/// <summary>مفاتيح جلسات MCP المؤقتة (الخطوة 2 من خطة أدوات محادثة Claude).</summary>
public class McpSessionTokensTests
{
    [Fact]
    public void Issue_ثم_Resolve_يعيد_نفس_المستخدم_والمشروع()
    {
        var tokens = new McpSessionTokens();
        var s = tokens.Issue("u1", "p1");
        Assert.StartsWith("mcp_", s.Token);
        Assert.Equal(52, s.Token.Length);
        var resolved = tokens.Resolve(s.Token);
        Assert.NotNull(resolved);
        Assert.Equal("u1", resolved!.UserId);
        Assert.Equal("p1", resolved.ProjectId);
    }

    [Fact]
    public void مفتاح_خاطئ_أو_فارغ_أو_ببادئة_أخرى_يُرفض()
    {
        var tokens = new McpSessionTokens();
        tokens.Issue("u1", "p1");
        Assert.Null(tokens.Resolve(null));
        Assert.Null(tokens.Resolve(""));
        Assert.Null(tokens.Resolve("mcp_" + new string('0', 48)));
        Assert.Null(tokens.Resolve("jeem_abc"));
    }

    [Fact]
    public void المفتاح_ينتهي_بعد_عمره_ويُحذف()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 11, 2, 10, 0, 0, TimeSpan.Zero));
        var tokens = new McpSessionTokens(clock);
        var s = tokens.Issue("u1", "p1", TimeSpan.FromMinutes(10));
        clock.Advance(TimeSpan.FromMinutes(9));
        Assert.NotNull(tokens.Resolve(s.Token));
        clock.Advance(TimeSpan.FromMinutes(2));
        Assert.Null(tokens.Resolve(s.Token));
        Assert.Equal(0, tokens.ActiveCount);
    }

    [Fact]
    public void الإلغاء_يوقف_المفتاح_فورًا_وعدّاد_الاستعلامات_يزداد()
    {
        var tokens = new McpSessionTokens();
        var s = tokens.Issue("u1", null);
        Assert.Equal(1, s.CountQuery());
        Assert.Equal(2, s.CountQuery());
        tokens.Revoke(s.Token);
        Assert.Null(tokens.Resolve(s.Token));
    }

    [Fact]
    public void مفتاحان_مختلفان_لرسالتين_ولا_يتداخلان()
    {
        var tokens = new McpSessionTokens();
        var a = tokens.Issue("u1", "p1"); var b = tokens.Issue("u2", "p2");
        Assert.NotEqual(a.Token, b.Token);
        Assert.Equal("p2", tokens.Resolve(b.Token)!.ProjectId);
        tokens.Revoke(a.Token);
        Assert.NotNull(tokens.Resolve(b.Token));
    }
}

/// <summary>مدقّق SELECT لأداة run_sql (الخطوة 3): رفض الكتابة والجمل المتعددة والجداول خارج المشروع، وقبول WITH، وفرض TOP.</summary>
public class ReadOnlySqlValidatorTests
{
    private static readonly string[] Allowed = { "proj_ab12.Sales", "proj_ab12.Opportunities", "dbo.sys_procurement" };

    [Theory]
    [InlineData("UPDATE proj_ab12.Sales SET Amount = 0")]
    [InlineData("DELETE FROM proj_ab12.Sales")]
    [InlineData("INSERT INTO proj_ab12.Sales VALUES (1)")]
    [InlineData("DROP TABLE proj_ab12.Sales")]
    [InlineData("SELECT * INTO x FROM proj_ab12.Sales")]
    [InlineData("EXEC xp_cmdshell 'dir'")]
    [InlineData("SELECT * FROM OPENROWSET('SQLNCLI', 'x', 'y')")]
    [InlineData("WAITFOR DELAY '00:00:05'")]
    public void جمل_الكتابة_والأوامر_الخطرة_تُرفض(string sql)
    {
        var r = ReadOnlySqlValidator.Validate(sql, Allowed);
        Assert.False(r.Ok);
        Assert.NotNull(r.Error);
    }

    [Fact]
    public void أكثر_من_جملة_يُرفض_والفاصلة_المنقوطة_الختامية_تُقبل()
    {
        Assert.False(ReadOnlySqlValidator.Validate("SELECT 1 FROM proj_ab12.Sales; DELETE FROM proj_ab12.Sales", Allowed).Ok);
        Assert.False(ReadOnlySqlValidator.Validate("SELECT 1 FROM proj_ab12.Sales; SELECT 2 FROM proj_ab12.Sales", Allowed).Ok);
        Assert.True(ReadOnlySqlValidator.Validate("SELECT Amount FROM proj_ab12.Sales;", Allowed).Ok);
    }

    [Fact]
    public void جدول_خارج_المشروع_يُرفض_برسالة_واضحة()
    {
        var r = ReadOnlySqlValidator.Validate("SELECT * FROM proj_zz99.Sales", Allowed);
        Assert.False(r.Ok);
        Assert.Contains("ليس من جداول مشروعك", r.Error);
        Assert.False(ReadOnlySqlValidator.Validate("SELECT * FROM dbo.repo_Files", Allowed).Ok);
        Assert.False(ReadOnlySqlValidator.Validate("SELECT * FROM INFORMATION_SCHEMA.COLUMNS", Allowed).Ok);
        Assert.False(ReadOnlySqlValidator.Validate("SELECT * FROM sys.tables", Allowed).Ok);
        Assert.False(ReadOnlySqlValidator.Validate("SELECT s.* FROM proj_ab12.Sales s JOIN proj_zz99.Other o ON o.Id = s.Id", Allowed).Ok);
        Assert.False(ReadOnlySqlValidator.Validate("SELECT * FROM proj_ab12.Sales, dbo.ColumnSemantics", Allowed).Ok);
    }

    [Fact]
    public void الاسم_بلا_مخطط_يُحل_إن_كان_فريدًا_وبالأقواس_يُقبل()
    {
        var r = ReadOnlySqlValidator.Validate("SELECT TOP 5 * FROM Sales", Allowed);
        Assert.True(r.Ok, r.Error);
        Assert.Equal(new[] { "proj_ab12.Sales" }, r.Tables);
        var b = ReadOnlySqlValidator.Validate("SELECT TOP 5 * FROM [proj_ab12].[Opportunities] AS o", Allowed);
        Assert.True(b.Ok, b.Error);
        Assert.Equal(new[] { "proj_ab12.Opportunities" }, b.Tables);
    }

    [Fact]
    public void WITH_مقبول_واسم_الـCTE_لا_يُعامل_كجدول()
    {
        var r = ReadOnlySqlValidator.Validate(
            "WITH s AS (SELECT Stage, COUNT(*) AS n FROM proj_ab12.Opportunities GROUP BY Stage) SELECT Stage, n FROM s ORDER BY n DESC", Allowed);
        Assert.True(r.Ok, r.Error);
        Assert.Equal(new[] { "proj_ab12.Opportunities" }, r.Tables);
        Assert.Contains("SELECT TOP (500) Stage, n FROM s", r.Sql);
        Assert.StartsWith("WITH s AS (SELECT Stage", r.Sql);
    }

    [Fact]
    public void TOP_يُفرض_عند_غيابه_ولا_يُكرَّر_ولا_يُضاف_مع_OFFSET()
    {
        Assert.StartsWith("SELECT TOP (500) Amount", ReadOnlySqlValidator.Validate("SELECT Amount FROM proj_ab12.Sales", Allowed).Sql);
        Assert.StartsWith("SELECT DISTINCT TOP (500) Region", ReadOnlySqlValidator.Validate("SELECT DISTINCT Region FROM proj_ab12.Sales", Allowed).Sql);
        var kept = ReadOnlySqlValidator.Validate("SELECT TOP 10 Amount FROM proj_ab12.Sales", Allowed).Sql;
        Assert.DoesNotContain("TOP (500)", kept);
        var offset = ReadOnlySqlValidator.Validate("SELECT Amount FROM proj_ab12.Sales ORDER BY Amount OFFSET 10 ROWS FETCH NEXT 5 ROWS ONLY", Allowed).Sql;
        Assert.DoesNotContain("TOP", offset);
        var custom = ReadOnlySqlValidator.Validate("SELECT Amount FROM proj_ab12.Sales", Allowed, maxRows: 50).Sql;
        Assert.StartsWith("SELECT TOP (50) Amount", custom);
    }

    [Fact]
    public void الكلمات_الممنوعة_داخل_النصوص_والتعليقات_لا_تُرفض()
    {
        var r = ReadOnlySqlValidator.Validate("SELECT * FROM proj_ab12.Sales -- DELETE later\nWHERE Status = 'DROP' AND Note <> 'x;y'", Allowed);
        Assert.True(r.Ok, r.Error);
    }

    [Fact]
    public void الفراغ_وغير_SELECT_يُرفضان()
    {
        Assert.False(ReadOnlySqlValidator.Validate("", Allowed).Ok);
        Assert.False(ReadOnlySqlValidator.Validate("SHOW TABLES", Allowed).Ok);
        Assert.False(ReadOnlySqlValidator.Validate("sp_help 'x'", Allowed).Ok);
    }

    [Fact]
    public void مطابقة_اسم_الجدول_في_الكتالوج()
    {
        var allowed = new List<McpTable>
        {
            new("proj_ab12.Sales", "proj_ab12", "Sales", "file", "sales.csv", 240),
            new("dbo.sys_procurement", "dbo", "sys_procurement", "system", "نظام المشتريات", null),
        };
        Assert.Equal("proj_ab12.Sales", ProjectCatalog.Match(allowed, "proj_ab12.Sales")!.Name);
        Assert.Equal("proj_ab12.Sales", ProjectCatalog.Match(allowed, "[proj_ab12].[Sales]")!.Name);
        Assert.Equal("proj_ab12.Sales", ProjectCatalog.Match(allowed, "sales")!.Name);
        Assert.Null(ProjectCatalog.Match(allowed, "proj_zz99.Sales"));
        Assert.Null(ProjectCatalog.Match(allowed, "repo_Files"));
    }
}
