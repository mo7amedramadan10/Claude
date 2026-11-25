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

    [Fact]
    public void SchemaMigration_RecognizesOnlyValidLegacySchemas_AndTextTablesAreAppOnly()
    {
        Assert.Equal("ent_", DataStore.EntitySchemaPrefix);
        Assert.Equal("0123abcdef0123abcdef0123abcdef01", DataStore.LegacySchemaEntityId("proj_0123abcdef0123abcdef0123abcdef01"));
        Assert.Null(DataStore.LegacySchemaEntityId("proj_"));
        Assert.Null(DataStore.LegacySchemaEntityId("proj_not-hex"));
        Assert.Null(DataStore.LegacySchemaEntityId("project_abc"));
        Assert.Null(DataStore.LegacySchemaEntityId("dbo"));
        Assert.Contains("DashboardHistory", DataStore.EntitySchemaTextTables);
        Assert.Contains("FreeformQueries", DataStore.EntitySchemaTextTables);
        Assert.Contains("repo_Files", DataStore.EntitySchemaTextTables);
        Assert.DoesNotContain("usage_Log", DataStore.EntitySchemaTextTables);
        Assert.DoesNotContain("AppUsers", DataStore.EntitySchemaTextTables);
        Assert.DoesNotContain("LlmCredentials", DataStore.EntitySchemaTextTables);
        Assert.NotEqual("dbo", DataStore.EntitySchemaBackup);
    }
}
