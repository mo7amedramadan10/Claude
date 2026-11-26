using ChatToDashboard.Api.Users;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>بوابة مالك المنصّة المستقل: يدير المنصّة فقط ولا يصل إلى واجهات بيانات المنظمات.</summary>
public class PlatformGateTests
{
    [Theory]
    [InlineData(true, null, true)]
    [InlineData(true, "", true)]
    [InlineData(true, "org1", false)]   // مالك منصّة قديم داخل منظمة: ليس مستقلًا
    [InlineData(false, null, false)]
    public void IsStandaloneOwner(bool owner, string? org, bool expected)
        => Assert.Equal(expected, PlatformGate.IsStandaloneOwner(owner, org));

    [Theory]
    [InlineData("/api/auth/me")]
    [InlineData("/api/organizations")]
    [InlineData("/api/organizations/abc/admins")]
    [InlineData("/api/templates/catalog")]
    [InlineData("/api/schema-aliases")]
    [InlineData("/api/llm-settings/credentials")]
    public void PlatformPaths_AreAllowed(string path) => Assert.False(PlatformGate.Blocks(path, false));

    [Theory]
    [InlineData("/api/history")]
    [InlineData("/api/chat")]
    [InlineData("/api/freeform")]
    [InlineData("/api/repository/files")]
    [InlineData("/api/users")]
    [InlineData("/api/entities")]
    [InlineData("/api/semantics")]
    [InlineData("/api/organizationsX")]   // بادئة مشابهة لا تكفي
    public void OrganizationDataPaths_AreBlocked(string path) => Assert.True(PlatformGate.Blocks(path, false));

    [Fact]
    public void PublicEndpointsAndPages_AreNotBlocked()
    {
        Assert.False(PlatformGate.Blocks("/api/presentations/x/content", true));
        Assert.False(PlatformGate.Blocks("/platform", false));
        Assert.False(PlatformGate.Blocks("/js/app.js", false));
    }
}
