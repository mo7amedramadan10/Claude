using ChatToDashboard.Api.Interactive;

namespace ChatToDashboard.Api.Tests;

/// <summary>Exercises TreasuryFundingDataProvider.BuildCashPosition — the month-over-month
/// opening/current/net-movement derivation from a single raw balance ledger, which is easy to
/// get subtly wrong (comparing "opening" against the wrong month, "current" against an
/// unrelated figure).</summary>
public class TreasuryFundingDataProviderTests
{
    [Fact]
    public void BuildCashPosition_ComputesOpeningCurrentAndNetMovementAgainstTheRightPriorFigures()
    {
        // Previous month (May): opens at 1,000,000, closes at 1,200,000 (net +200,000).
        // Current month (June): opens at 1,200,000, now (latest record) at 1,500,000 (net +300,000).
        var balances = new List<(DateTime At, double Balance)>
        {
            (new DateTime(2025, 5, 1), 1_000_000),
            (new DateTime(2025, 5, 31), 1_200_000),
            (new DateTime(2025, 6, 1), 1_200_000),
            (new DateTime(2025, 6, 15), 1_500_000),
        };

        var rows = TreasuryFundingDataProvider.BuildCashPosition(balances);
        var byLabel = rows.ToDictionary(r => (string)r.GetType().GetProperty("label")!.GetValue(r)!, r => r);
        double Value(string label, string prop) => (double)byLabel[label].GetType().GetProperty(prop)!.GetValue(byLabel[label])!;

        Assert.Equal(1_200_000, Value("الرصيد الافتتاحي", "value"));
        Assert.Equal(1_500_000, Value("الرصيد الحالي", "value"));
        Assert.Equal(300_000, Value("حركة جديدة", "value"));

        // Opening: 1,200,000 vs last month's opening 1,000,000 -> +20%.
        Assert.Equal(20, Value("الرصيد الافتتاحي", "deltaPct"), 3);
        // Current: 1,500,000 vs last month's closing 1,200,000 -> +25%.
        Assert.Equal(25, Value("الرصيد الحالي", "deltaPct"), 3);
        // Net movement: 300,000 vs last month's net movement 200,000 -> +50%.
        Assert.Equal(50, Value("حركة جديدة", "deltaPct"), 3);
    }

    [Fact]
    public void BuildCashPosition_HandlesNoHistoryBeforeThisMonthWithoutThrowing()
    {
        var balances = new List<(DateTime At, double Balance)> { (new DateTime(2025, 6, 10), 500_000) };
        var rows = TreasuryFundingDataProvider.BuildCashPosition(balances);
        Assert.Equal(3, rows.Count);
    }
}
