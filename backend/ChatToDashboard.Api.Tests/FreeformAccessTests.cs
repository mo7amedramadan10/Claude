using ChatToDashboard.Api.Freeform;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>قواعد الوصول إلى تصميم حر: الصاحب والمسؤول، المشاركة بالاسم، المشروع، المنظمة، وعزل المنظمات.</summary>
public class FreeformAccessTests
{
    private static FreeformDesign D(string scope = "private", string role = "view", string project = "p1", string? org = "o1", string owner = "u-owner")
        => new() { Id = "d1", ProjectId = project, OrganizationId = org, CreatedByUserId = owner, ShareScope = scope, ShareScopeRole = role };

    [Fact]
    public void OwnerAndAdmin_AreOwners_OtherOrgNeverSees()
    {
        Assert.Equal("owner", FreeformAccess.Resolve(D(), "U-OWNER", "o1", "p9", false, null)); // صاحبه حتى من مشروع آخر، بلا حساسية لحالة الأحرف
        Assert.Equal("owner", FreeformAccess.Resolve(D(), "admin", "o1", "p9", true, null));
        Assert.Null(FreeformAccess.Resolve(D(scope: "org", role: "edit"), "x", "o2", "p1", true, "edit")); // منظمة أخرى: لا شيء حتى للمسؤول
    }

    [Fact]
    public void Private_OnlyNamedUsers()
    {
        Assert.Null(FreeformAccess.Resolve(D(), "x", "o1", "p1", false, null)); // عضو المشروع نفسه لا يرى الخاص
        Assert.Equal("view", FreeformAccess.Resolve(D(), "x", "o1", "p1", false, "view"));
        Assert.Equal("edit", FreeformAccess.Resolve(D(), "x", "o1", "p2", false, "edit"));
        Assert.Equal("view", FreeformAccess.Resolve(D(), "x", "o1", "p1", false, "bogus")); // دور غير معروف = مشاهدة
    }

    [Fact]
    public void ProjectScope_OnlyMembersOfThatProject()
    {
        Assert.Equal("edit", FreeformAccess.Resolve(D("project", "edit"), "x", "o1", "p1", false, null));
        Assert.Null(FreeformAccess.Resolve(D("project", "edit"), "x", "o1", "p2", false, null));
        // الأعلى بين المشاركة بالاسم والوصول العام
        Assert.Equal("edit", FreeformAccess.Resolve(D("project", "view"), "x", "o1", "p1", false, "edit"));
        Assert.Equal("edit", FreeformAccess.Resolve(D("project", "edit"), "x", "o1", "p1", false, "view"));
    }

    [Fact]
    public void OrgScope_EveryoneInOrg_AndLegacyWithoutOrgUsesProject()
    {
        Assert.Equal("view", FreeformAccess.Resolve(D("org"), "x", "o1", "p7", false, null));
        Assert.Null(FreeformAccess.Resolve(D("org"), "x", "o2", "p7", false, null));
        Assert.Equal("edit", FreeformAccess.Resolve(D("project", "edit", org: null), "x", "o1", "p1", false, null));
        Assert.Null(FreeformAccess.Resolve(D("org", org: null), "x", "o1", "p2", false, null));
        Assert.True(FreeformAccess.CanEdit("owner")); Assert.True(FreeformAccess.CanEdit("edit")); Assert.False(FreeformAccess.CanEdit("view")); Assert.False(FreeformAccess.CanEdit(null));
        Assert.Equal("private", FreeformAccess.NormalizeScope("everyone"));
    }
}
