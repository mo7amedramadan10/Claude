namespace ChatToDashboard.Api.Widgets;

/// <summary>One future period's point forecast plus its ~95% prediction interval.</summary>
public class ForecastPoint
{
    public int Index { get; set; }
    public double Value { get; set; }
    public double Lower { get; set; }
    public double Upper { get; set; }
}

public class ForecastOutcome
{
    public string Method { get; set; } = "linear_regression";
    public double Slope { get; set; }
    public double Intercept { get; set; }
    public double RSquared { get; set; }
    public List<ForecastPoint> Points { get; set; } = new();

    /// <summary>Set when the historical series is short enough that the interval should be
    /// read with real caution — surfaced to the user rather than left implicit.</summary>
    public string? Note { get; set; }
}

/// <summary>
/// A real statistical forecast — ordinary least-squares linear regression, with an optional
/// additive seasonal adjustment when the series spans at least two full cycles — used by both
/// the "🔮 توقّع الأشهر الجاية" button (POST /api/widgets/forecast, on whatever data a chart
/// already has) and the forecast_data tool the LLM calls for a chat-requested forecast (see
/// AnalyticsTools). Neither path lets the model or the UI invent a predicted number: every
/// value here is computed from the actual historical series, never guessed.
/// </summary>
public static class ForecastService
{
    // Two-sided 95% t critical values for df 1..30 (Student's t) — a fixed z=1.96 approximation
    // is fine for large samples but overstates confidence badly for the handful of points most
    // business time series actually have, which is exactly when the interval matters most.
    private static readonly double[] TTable95 =
    {
        12.706, 4.303, 3.182, 2.776, 2.571, 2.447, 2.365, 2.306, 2.262, 2.228,
        2.201, 2.179, 2.160, 2.145, 2.131, 2.120, 2.110, 2.101, 2.093, 2.086,
        2.080, 2.074, 2.069, 2.064, 2.060, 2.056, 2.052, 2.048, 2.045, 2.042,
    };

    private static double TCritical(int df) => df is >= 1 and <= 30 ? TTable95[df - 1] : 1.96;

    public static ForecastOutcome Forecast(IReadOnlyList<double> values, int periodsAhead, int? seasonLength = null)
    {
        if (values.Count < 2)
            throw new InvalidOperationException("محتاج نقطتين بيانات تاريخية على الأقل عشان نقدر نتوقع.");
        if (periodsAhead < 1)
            throw new InvalidOperationException("عدد الفترات المطلوب توقعها لازم يكون واحد على الأقل.");

        var n = values.Count;
        var xMean = (n - 1) / 2.0; // x runs 0..n-1, so its mean has a closed form
        var yMean = values.Average();

        double sxx = 0, sxy = 0;
        for (var i = 0; i < n; i++)
        {
            var dx = i - xMean;
            sxx += dx * dx;
            sxy += dx * (values[i] - yMean);
        }
        var slope = sxx == 0 ? 0 : sxy / sxx;
        var intercept = yMean - slope * xMean;

        var residuals = new double[n];
        for (var i = 0; i < n; i++) residuals[i] = values[i] - (intercept + slope * i);

        // Additive seasonal index (average residual-from-trend per position in the cycle), only
        // attempted when there's enough history to estimate it from at least two full cycles —
        // otherwise a "seasonal" adjustment from one partial cycle would just be overfitting noise.
        double[]? seasonalIndex = null;
        if (seasonLength is >= 2 && n >= seasonLength.Value * 2)
        {
            var sl = seasonLength.Value;
            seasonalIndex = new double[sl];
            var counts = new int[sl];
            for (var i = 0; i < n; i++) { seasonalIndex[i % sl] += residuals[i]; counts[i % sl]++; }
            for (var s = 0; s < sl; s++) seasonalIndex[s] = counts[s] > 0 ? seasonalIndex[s] / counts[s] : 0;
            var meanSeason = seasonalIndex.Average();
            for (var s = 0; s < sl; s++) seasonalIndex[s] -= meanSeason; // de-meaned: doesn't shift the trend
        }

        var sse = residuals.Sum(r => r * r);
        var df = Math.Max(1, n - 2);
        var residualStdError = Math.Sqrt(sse / df);
        var sst = values.Sum(v => (v - yMean) * (v - yMean));
        var r2 = sst == 0 ? 1 : 1 - sse / sst;
        var tCrit = TCritical(df);

        var points = new List<ForecastPoint>();
        for (var h = 1; h <= periodsAhead; h++)
        {
            var x0 = n - 1 + h;
            var trend = intercept + slope * x0;
            var seasonal = seasonalIndex is not null ? seasonalIndex[x0 % seasonalIndex.Length] : 0;
            var point = trend + seasonal;
            // Standard OLS prediction-interval formula: widens both for a smaller sample (1/n
            // term) and the further x0 sits from the historical center (the (x0-xMean)² term) —
            // exactly the "less data or further out => less certain" behavior asked for.
            var se = residualStdError * Math.Sqrt(1 + 1.0 / n + (x0 - xMean) * (x0 - xMean) / (sxx == 0 ? 1 : sxx));
            var margin = tCrit * se;
            points.Add(new ForecastPoint { Index = x0, Value = point, Lower = point - margin, Upper = point + margin });
        }

        return new ForecastOutcome
        {
            Method = seasonalIndex is not null ? "linear_regression_seasonal" : "linear_regression",
            Slope = slope,
            Intercept = intercept,
            RSquared = Math.Round(r2, 3),
            Points = points,
            Note = n < 6
                ? $"عدد نقاط البيانات التاريخية قليل ({n}) — نطاق الثقة واسع لأن التوقع أقل دقة."
                : null,
        };
    }
}
