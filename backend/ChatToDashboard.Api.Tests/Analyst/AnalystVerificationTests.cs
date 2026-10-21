using ChatToDashboard.Api.Analyst;

namespace ChatToDashboard.Api.Tests.Analyst;

public class AnalystVerificationTests
{
    [Theory]
    [InlineData(2070000, 2070000, 0.0001, true)] // exact match
    [InlineData(2070000, 2070000.1, 0.0001, true)] // within relative tolerance
    [InlineData(0, 0, 0.0001, true)] // both zero — no divide-by-zero
    [InlineData(1000, 2000, 0.0001, false)] // classic duplicate-row-from-a-JOIN case: double
    [InlineData(2070000, 2070300, 0.0001, false)] // just outside tolerance (~0.0145% relative diff)
    public void ReconciliationPasses_ComparesWithinRelativeTolerance(double actual, double audit, double tolerance, bool expected)
    {
        Assert.Equal(expected, AnalystVerification.ReconciliationPasses(actual, audit, tolerance));
    }

    [Fact]
    public void IsFresh_WithinMaxAge_ReturnsTrue()
    {
        var now = new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc);
        var lastUpdated = now.AddHours(-5);
        Assert.True(AnalystVerification.IsFresh(lastUpdated, maxAgeHours: 24, now));
    }

    [Fact]
    public void IsFresh_OlderThanMaxAge_ReturnsFalse()
    {
        var now = new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc);
        var lastUpdated = now.AddHours(-48);
        Assert.False(AnalystVerification.IsFresh(lastUpdated, maxAgeHours: 24, now));
    }

    [Fact]
    public void IsFresh_ExactlyAtMaxAge_ReturnsTrue()
    {
        var now = new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc);
        var lastUpdated = now.AddHours(-24);
        Assert.True(AnalystVerification.IsFresh(lastUpdated, maxAgeHours: 24, now));
    }
}
