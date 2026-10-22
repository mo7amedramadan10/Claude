using ChatToDashboard.Api.Interactive;

namespace ChatToDashboard.Api.Tests;

/// <summary>Exercises SaasMetricsDataProvider.BuildCohorts directly with synthetic account-week
/// data — the part of the provider the live LLM-matching pipeline can't be driven through in
/// this environment (no OpenAI/Anthropic key configured for the target app itself), and the
/// part most likely to hide an off-by-one bug (age-in-months arithmetic, "not reached yet"
/// nulling). Two cohorts, three calendar months (Jan/Feb/Mar 2025), one account that churns.</summary>
public class SaasMetricsDataProviderTests
{
    private static readonly DateTime Jan = new(2025, 1, 6);
    private static readonly DateTime Feb = new(2025, 2, 3);
    private static readonly DateTime Mar = new(2025, 3, 3);

    private static List<SaasMetricsDataProvider.Snapshot> BuildSnapshots() => new()
    {
        new("A1", Jan, 100, "Pro"),
        new("A1", Feb, 100, "Pro"),
        // A1 has no March row: churned between Feb and Mar.
        new("A2", Jan, 50, "Pro"),
        new("A2", Feb, 60, "Pro"),
        new("A2", Mar, 60, "Pro"),
        new("B1", Mar, 200, "Pro"),
    };

    [Fact]
    public void BuildCohorts_ComputesTwoCohortsWithCorrectLogoAndRevenueRetention()
    {
        var weeks = new List<DateTime> { Jan, Feb, Mar };
        var cohorts = SaasMetricsDataProvider.BuildCohorts(BuildSnapshots(), weeks);

        Assert.Equal(new[] { "يناير 25", "مارس 25" }, cohorts.Months);
        Assert.Equal(13, cohorts.Ages.Count);
        Assert.Equal("ش0", cohorts.Ages[0]);
        Assert.Equal("ش12", cohorts.Ages[12]);

        // Cohort "يناير 25" = {A1, A2}, age0 revenue = 100 + 50 = 150.
        var janLogo = cohorts.Logo[0];
        var janRev = cohorts.Rev[0];
        Assert.Equal(100, janLogo[0]); // both active in January itself
        Assert.Equal(100, janRev[0]);
        Assert.Equal(100, janLogo[1]); // both still active in February (age 1)
        Assert.Equal(107, janRev[1]);  // (100+60)/150*100 = 106.67 -> rounds to 107
        Assert.Equal(50, janLogo[2]);  // only A2 left in March (age 2) -> 1 of 2 accounts
        Assert.Equal(40, janRev[2]);   // 60/150*100 = 40
        Assert.Null(janLogo[3]);       // age 3 = April, past the latest month in the data -> not reached yet
        Assert.Null(janRev[3]);

        // Cohort "مارس 25" = {B1} only, age0 revenue = 200.
        var marLogo = cohorts.Logo[1];
        var marRev = cohorts.Rev[1];
        Assert.Equal(100, marLogo[0]);
        Assert.Equal(100, marRev[0]);
        Assert.Null(marLogo[1]); // age 1 = April, not reached yet
        Assert.Null(marRev[1]);
    }

    [Fact]
    public void BuildCohorts_ReturnsEmptyShapeWhenNoAccountEverHadPositiveMrr()
    {
        var snapshots = new List<SaasMetricsDataProvider.Snapshot> { new("A1", Jan, 0, null) };
        var cohorts = SaasMetricsDataProvider.BuildCohorts(snapshots, new List<DateTime> { Jan });

        Assert.Empty(cohorts.Months);
        Assert.Empty(cohorts.Ages);
        Assert.Empty(cohorts.Logo);
        Assert.Empty(cohorts.Rev);
    }
}
