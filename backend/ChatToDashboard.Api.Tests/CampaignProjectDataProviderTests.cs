using ChatToDashboard.Api.Interactive;

namespace ChatToDashboard.Api.Tests;

/// <summary>Exercises CampaignProjectDataProvider.BuildPerformanceSeries — the null-padding/
/// forecast-connection-point arithmetic the engine's own `cut()` logic expects (see
/// campaign-project.js): actual values only for the real days, forecast values only from the
/// last real day onward (so the dashed line visually starts where the solid one ends).</summary>
public class CampaignProjectDataProviderTests
{
    [Fact]
    public void BuildPerformanceSeries_PadsActualAndForecastToConnectAtTheBoundary()
    {
        var today = new DateTime(2025, 6, 10);
        var actual = Enumerable.Range(0, 5)
            .Select(i => (Date: today.AddDays(-4 + i), Spend: 100.0 + i * 10, Conversions: 2.0 + i))
            .ToList();
        // Spends: 100,110,120,130,140 -> last-5-day average = 120; Conversions: 2,3,4,5,6 -> avg = 4.

        var series = CampaignProjectDataProvider.BuildPerformanceSeries(actual);

        Assert.Equal(12, series.Labels.Count); // 5 actual days + 7 forecast days
        Assert.Equal(12, series.SpendActual.Count);

        // Actual series: real values for the 5 real days, null afterwards.
        Assert.Equal(new double?[] { 100, 110, 120, 130, 140, null, null, null, null, null, null, null }, series.SpendActual);
        // Forecast series: null until the last actual day (index 4), which repeats the actual
        // value there to connect the dashed line, then a flat trailing-average projection.
        Assert.Null(series.SpendForecast[0]);
        Assert.Null(series.SpendForecast[3]);
        Assert.Equal(140, series.SpendForecast[4]);
        for (var i = 5; i < 12; i++) Assert.Equal(120, series.SpendForecast[i]);

        Assert.Equal(6, series.ConvForecast[4]);
        for (var i = 5; i < 12; i++) Assert.Equal(4, series.ConvForecast[i]);
    }

    [Fact]
    public void BuildPerformanceSeries_UsesOnlyTrailingSevenDaysWhenMoreHistoryExists()
    {
        var today = new DateTime(2025, 6, 10);
        // 10 days of history, spend rising sharply only in the last 7 — the forecast average
        // should reflect just that trailing window, not the earlier flat days.
        var actual = Enumerable.Range(0, 10)
            .Select(i => (Date: today.AddDays(-9 + i), Spend: i < 3 ? 10.0 : 100.0, Conversions: 1.0))
            .ToList();

        var series = CampaignProjectDataProvider.BuildPerformanceSeries(actual);
        // Trailing 7 days (indices 3..9) are all 100 -> average exactly 100, unaffected by the
        // three early 10-value days.
        Assert.Equal(100, series.SpendForecast[^1]);
    }
}
