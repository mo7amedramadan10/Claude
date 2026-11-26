using ChatToDashboard.Api.Entities;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>بوابة «بلا كيان»: أي واجهة كيان تُرفض لمستخدم بلا دور، والمستثناة والعامة تمر.</summary>
public class EntityGateTests
{
    [Theory]
    [InlineData("/api/repository/files", true)]
    [InlineData("/api/freeform", true)]
    [InlineData("/api/history", true)]
    [InlineData("/api/sources", true)]
    [InlineData("/api/viewing", true)]
    [InlineData("/api/chat", true)]
    [InlineData("/api/auth/me", false)]
    [InlineData("/api/entities", false)]
    [InlineData("/api/entities/current", false)]
    [InlineData("/api/entity-types", false)]
    [InlineData("/api/templates/catalog", false)]
    [InlineData("/api/presentations", false)]
    [InlineData("/api/entitiesx", true)]
    [InlineData("/index.html", false)]
    public void AppliesTo_EntityScopedApisOnly(string path, bool expected) =>
        Assert.Equal(expected, EntityGate.AppliesTo(path, isPublicEndpoint: false));

    [Fact]
    public void PublicEndpoints_AreNeverGated() =>
        Assert.False(EntityGate.AppliesTo("/api/freeform/assets/abc", isPublicEndpoint: true));
}
