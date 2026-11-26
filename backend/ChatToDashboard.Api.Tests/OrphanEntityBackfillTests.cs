using ChatToDashboard.Api.Data;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>إسناد المحتوى القديم بلا كيان إلى أول كيان في منظمته: مصدر المنظمة بحسب أعمدة الجدول.</summary>
public class OrphanEntityBackfillTests
{
    [Fact]
    public void UsesOrganizationColumnThenOwnerThenDefault()
    {
        var sql = DataStore.OrphanBackfillSql("FreeformDesigns", new[] { "Id", "EntityId", "OrganizationId", "CreatedByUserId" });
        Assert.Contains("[dbo].[FreeformDesigns] t", sql);
        var org = sql.IndexOf("NULLIF(t.[OrganizationId], '')", StringComparison.Ordinal);
        var owner = sql.IndexOf("u.[Id] = t.[CreatedByUserId]", StringComparison.Ordinal);
        var def = sql.IndexOf("@defaultOrg", StringComparison.Ordinal);
        Assert.True(org > 0 && owner > org && def > owner);
        Assert.Contains("t.[EntityId] IS NULL OR t.[EntityId] = ''", sql);
        Assert.Contains("ORDER BY e.[CreatedAt]", sql);
    }

    [Fact]
    public void WithoutOrganizationColumn_UsesOwnerColumnsInPriority()
    {
        var sql = DataStore.OrphanBackfillSql("DashboardHistory", new[] { "Id", "EntityId", "UserId", "OwnerId" });
        Assert.DoesNotContain("t.[OrganizationId]", sql);
        Assert.True(sql.IndexOf("t.[OwnerId]", StringComparison.Ordinal) < sql.IndexOf("t.[UserId]", StringComparison.Ordinal));
    }

    [Fact]
    public void WithNoOrganizationOrOwnerColumn_UsesDefaultWithoutSingleArgCoalesce()
    {
        var sql = DataStore.OrphanBackfillSql("PublishedDashboards", new[] { "Id", "EntityId", "HistoryId" });
        Assert.DoesNotContain("COALESCE", sql);
        Assert.Contains("e.[OrganizationId] = @defaultOrg", sql);
    }

    [Fact]
    public void CoversTheVisibleContentTables()
    {
        foreach (var t in new[] { "DashboardHistory", "FreeformDesigns", "Decks", "repo_Files", "AnalystConversations" })
            Assert.Contains(t, DataStore.OrphanEntityTables);
    }
}
