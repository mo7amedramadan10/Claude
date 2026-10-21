using ChatToDashboard.Api.Interactive;

namespace ChatToDashboard.Api.Tests;

/// <summary>Exercises FinanceOpsDataProvider's bucket-boundary math (weekly/monthly windows)
/// and the real running-balance "opening" derivation — the parts most likely to silently put a
/// transaction in the wrong bucket or miscompute the opening balance the whole chart anchors on.</summary>
public class FinanceOpsDataProviderTests
{
    private static dynamic Get(object o, string prop) => o.GetType().GetProperty(prop)!.GetValue(o)!;

    [Fact]
    public void WeeklyBuckets_ProducesTwelveConsecutive7DayWindowsEndingToday()
    {
        var latest = new DateTime(2025, 6, 30); // a Monday
        var buckets = FinanceOpsDataProvider.WeeklyBuckets(latest, 12);

        Assert.Equal(12, buckets.Count);
        // Oldest-first ordering (matches the engine's own "oldest on the right" RTL convention).
        Assert.True(buckets[0].Start < buckets[^1].Start);
        // The last bucket must end exactly the day after `latest` (so `latest` itself falls inside it).
        Assert.Equal(latest.AddDays(1), buckets[^1].EndExclusive);
        // Buckets are contiguous: each one's end is the next one's start, no gap or overlap.
        for (var i = 1; i < buckets.Count; i++) Assert.Equal(buckets[i].Start, buckets[i - 1].EndExclusive);
    }

    [Fact]
    public void MonthlyBuckets_AlignsToCalendarMonthsEndingWithLatest()
    {
        var latest = new DateTime(2025, 6, 15);
        var buckets = FinanceOpsDataProvider.MonthlyBuckets(latest, 12);

        Assert.Equal(12, buckets.Count);
        Assert.Equal(new DateTime(2024, 7, 1), buckets[0].Start);   // 12 months back from June 2025
        Assert.Equal(new DateTime(2025, 6, 1), buckets[^1].Start);  // June 2025 itself, latest month
        Assert.Equal(new DateTime(2025, 7, 1), buckets[^1].EndExclusive);
    }

    [Fact]
    public void BuildCashWindow_OpeningIsTheRealRunningBalanceBeforeTheWindowStarts()
    {
        var cash = new List<(DateTime At, double In, double Out)>
        {
            (new DateTime(2025, 1, 1), 1000, 0),   // before the window -> part of "opening"
            (new DateTime(2025, 1, 2), 0, 200),    // before the window -> part of "opening"
            (new DateTime(2025, 2, 1), 500, 100),  // inside the window
        };
        var buckets = new List<FinanceOpsDataProvider.Bucket> { new(new DateTime(2025, 2, 1), new DateTime(2025, 2, 2)) };

        var result = FinanceOpsDataProvider.BuildCashWindow(cash, buckets, i => i.ToString());

        Assert.Equal(800, (double)Get(result, "opening")); // 1000 - 200, both transactions before Feb 1
        var points = (System.Collections.IList)Get(result, "points");
        Assert.Single(points);
        Assert.Equal(500, (double)Get(points[0]!, "in"));
        Assert.Equal(100, (double)Get(points[0]!, "out"));
    }
}
