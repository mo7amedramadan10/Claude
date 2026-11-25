using ChatToDashboard.Api.Data;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>ترحيل «المشروع» إلى «الكيان»: جداول التطبيق وحدها، مشروط وقابل لإعادة التشغيل.</summary>
public class EntityRenameMigrationTests
{
    [Fact]
    public void RenameSql_IsConditional_AndLimitedToAppTables()
    {
        var sql = DataStore.EntityRenameSql();
        Assert.Contains("IF OBJECT_ID('dbo.Projects') IS NOT NULL AND OBJECT_ID('dbo.Entities') IS NULL EXEC sp_rename 'dbo.Projects', 'Entities';", sql);
        Assert.Contains("IF OBJECT_ID('dbo.ProjectRoles') IS NOT NULL AND OBJECT_ID('dbo.EntityRoles') IS NULL EXEC sp_rename 'dbo.ProjectRoles', 'EntityRoles';", sql);
        Assert.Contains("EXEC sp_rename 'dbo.repo_Files.ProjectId', 'EntityId', 'COLUMN'", sql);
        Assert.Contains("EXEC sp_rename 'dbo.DeckBrands.ProjectKey', 'EntityKey', 'COLUMN'", sql);
        Assert.Contains("SET [EntityId] = [ProjectId] WHERE [EntityId] IS NULL", sql);
        // جداول ليست من التطبيق في dbo لا تُمس
        Assert.DoesNotContain("dbo.KPI", sql);
        Assert.DoesNotContain("dbo.OKR", sql);
        // الجدولان يُعاد تسميتهما قبل أعمدتهما (EntityRoles في قائمة الأعمدة باسمه الجديد)
        Assert.True(sql.IndexOf("'dbo.ProjectRoles', 'EntityRoles'") < sql.IndexOf("dbo.EntityRoles.ProjectId"));
        Assert.Equal(DataStore.EntityColumnTables.Length, DataStore.EntityColumnTables.Distinct().Count());
    }
}
