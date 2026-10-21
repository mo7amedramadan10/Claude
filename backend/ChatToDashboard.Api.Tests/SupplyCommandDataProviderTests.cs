using ChatToDashboard.Api.Interactive;

namespace ChatToDashboard.Api.Tests;

/// <summary>Exercises SupplyCommandDataProvider's 24-month bucketing (MonthlySum/
/// MonthlyOtifPct) — the part the engine's year→quarter→month drill depends on summing/
/// averaging correctly from a single flat months array (see supply-command.js's own `rollup`),
/// so an off-by-one here would silently misplace a whole month's data into the wrong year or
/// quarter in the UI.</summary>
public class SupplyCommandDataProviderTests
{
    private static SupplyCommandDataProvider.Order Order(DateTime date, double revenue, bool onTime) =>
        new(date, "reg", "الويب", "المستودع أ", "عام", "عادي", null, revenue, revenue * .3, revenue * .7, onTime, false, false, 0, 0, 2);

    [Fact]
    public void MonthlySum_Places24MonthsInOrderStartingFromMonthStart()
    {
        var start = new DateTime(2024, 1, 1);
        var orders = new List<SupplyCommandDataProvider.Order>
        {
            Order(new DateTime(2024, 1, 15), 100, true),   // month index 0
            Order(new DateTime(2024, 1, 20), 50, true),    // month index 0 too -> sums with the above
            Order(new DateTime(2024, 6, 1), 200, true),    // month index 5
            Order(new DateTime(2025, 12, 31), 400, true),  // month index 23 (last bucket)
            Order(new DateTime(2026, 1, 1), 999, true),    // outside the 24-month window -> must not leak in anywhere
        };

        var months = SupplyCommandDataProvider.MonthlySum(orders, start, o => o.Revenue);

        Assert.Equal(24, months.Count);
        Assert.Equal(150, months[0]);   // Jan 2024: 100 + 50
        Assert.Equal(200, months[5]);   // Jun 2024
        Assert.Equal(400, months[23]);  // Dec 2025, the last of the 24 months
        Assert.Equal(0, months[1]);     // Feb 2024: untouched
        Assert.DoesNotContain(999, months); // Jan 2026 falls outside the window entirely
    }

    [Fact]
    public void MonthlyOtifPct_ComputesPerMonthRateNotACumulativeCount()
    {
        var start = new DateTime(2024, 1, 1);
        var orders = new List<SupplyCommandDataProvider.Order>
        {
            Order(new DateTime(2024, 3, 1), 10, true),
            Order(new DateTime(2024, 3, 2), 10, true),
            Order(new DateTime(2024, 3, 3), 10, false),
            Order(new DateTime(2024, 3, 4), 10, false),
        };

        var months = SupplyCommandDataProvider.MonthlyOtifPct(orders, start);

        // March 2024 is month index 2: 2 of 4 orders on time -> 50%, not a raw count.
        Assert.Equal(50, months[2]);
        Assert.Equal(0, months[0]); // a month with no orders is 0%, not null/NaN
    }
}
