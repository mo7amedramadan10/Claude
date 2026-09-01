using Microsoft.Extensions.Options;

namespace ChatToDashboard.Api.Usage;

/// <summary>
/// Turns token counts into an estimated cost using the configured price table.
/// Applied when the log is *read*, so correcting a price in appsettings.json is
/// reflected across the whole history rather than only on new requests.
/// </summary>
public class CostCalculator
{
    private const decimal PerMillion = 1_000_000m;

    private readonly PricingOptions _pricing;

    public CostCalculator(IOptions<PricingOptions> pricing) => _pricing = pricing.Value;

    public decimal Estimate(string model, int inputTokens, int outputTokens, int cacheReadTokens, int cacheWriteTokens)
    {
        if (string.IsNullOrWhiteSpace(model) || !_pricing.Models.TryGetValue(model, out var price)) return 0m;

        var cacheRead = price.CacheRead ?? price.Input * 0.10m;
        var cacheWrite = price.CacheWrite ?? price.Input * 1.25m;
        return (inputTokens * price.Input
              + outputTokens * price.Output
              + cacheReadTokens * cacheRead
              + cacheWriteTokens * cacheWrite) / PerMillion;
    }

    public decimal Estimate(UsageRecord record) =>
        Estimate(record.Model, record.InputTokens, record.OutputTokens,
            record.CacheReadTokens, record.CacheWriteTokens);
}
