using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ChatToDashboard.Api.Usage;

/// <summary>Price per million tokens for one model.</summary>
public class ModelPrice
{
    public decimal Input { get; set; }
    public decimal Output { get; set; }

    /// <summary>Cached-read price; defaults to 10% of input (Anthropic's cache-read rate).</summary>
    public decimal? CacheRead { get; set; }

    /// <summary>Cache-write price; defaults to 125% of input (Anthropic's cache-write rate).</summary>
    public decimal? CacheWrite { get; set; }
}

public class PricingOptions
{
    public const string SectionName = "Pricing";

    /// <summary>Model id -> price per million tokens. Unknown models cost 0 (shown as "—").</summary>
    public Dictionary<string, ModelPrice> Models { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Accumulates everything sent to and returned by the model for a single question,
/// then writes one row to the usage log. Created per request.
/// </summary>
public class UsageTrace
{
    private readonly UsageStore _store;
    private readonly PricingOptions _pricing;
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private readonly UsageRecord _record;

    internal UsageTrace(UsageStore store, PricingOptions pricing, string provider, string model,
        string question, string enabledSources)
    {
        _store = store;
        _pricing = pricing;
        _record = new UsageRecord
        {
            Id = Guid.NewGuid().ToString("N"),
            Timestamp = DateTime.UtcNow,
            Provider = provider,
            Model = model,
            Question = question,
            EnabledSources = enabledSources,
        };
    }

    public void SetSystemPrompt(string prompt) => _record.SystemPrompt ??= prompt;

    /// <summary>Records one model round-trip, with the exact bodies exchanged.</summary>
    public void RecordTurn(string requestBody, string responseBody, JsonObject response, long durationMs)
    {
        var usage = response["usage"]?.AsObject();
        var turn = new UsageTurn
        {
            Turn = _record.Turns.Count + 1,
            // Anthropic and OpenAI name these differently; read whichever is present.
            InputTokens = Read(usage, "input_tokens", "prompt_tokens"),
            OutputTokens = Read(usage, "output_tokens", "completion_tokens"),
            CacheReadTokens = Read(usage, "cache_read_input_tokens", null)
                + ReadNested(usage, "prompt_tokens_details", "cached_tokens"),
            CacheWriteTokens = Read(usage, "cache_creation_input_tokens", null),
            StopReason = response["stop_reason"]?.GetValue<string>()
                ?? response["choices"]?[0]?["finish_reason"]?.GetValue<string>(),
            DurationMs = durationMs,
            RequestBody = requestBody,
            ResponseBody = responseBody,
        };
        _record.Turns.Add(turn);

        _record.InputTokens += turn.InputTokens;
        _record.OutputTokens += turn.OutputTokens;
        _record.CacheReadTokens += turn.CacheReadTokens;
        _record.CacheWriteTokens += turn.CacheWriteTokens;
    }

    public void RecordToolCall(string tool, string input, string result, bool isError, long durationMs) =>
        _record.ToolCalls.Add(new UsageToolCall
        {
            Turn = _record.Turns.Count,
            Tool = tool,
            Input = input,
            Result = result,
            IsError = isError,
            DurationMs = durationMs,
        });

    public async Task CompleteAsync(bool success, string? finalResponse, string? error, CancellationToken ct = default)
    {
        _record.Success = success;
        _record.Error = error;
        _record.FinalResponse = finalResponse;
        _record.TurnCount = _record.Turns.Count;
        _record.ToolCallCount = _record.ToolCalls.Count;
        _record.TotalTokens = _record.InputTokens + _record.OutputTokens + _record.CacheReadTokens;
        _record.DurationMs = _stopwatch.ElapsedMilliseconds;
        _record.EstimatedCost = EstimateCost();
        await _store.SaveAsync(_record, ct);
    }

    private decimal EstimateCost()
    {
        if (!_pricing.Models.TryGetValue(_record.Model, out var price)) return 0m;
        const decimal perMillion = 1_000_000m;
        var cacheRead = price.CacheRead ?? price.Input * 0.10m;
        var cacheWrite = price.CacheWrite ?? price.Input * 1.25m;
        return (_record.InputTokens * price.Input
              + _record.OutputTokens * price.Output
              + _record.CacheReadTokens * cacheRead
              + _record.CacheWriteTokens * cacheWrite) / perMillion;
    }

    private static int Read(JsonObject? usage, string first, string? second)
    {
        if (usage is null) return 0;
        if (usage[first] is { } a && a.GetValueKind() == JsonValueKind.Number) return a.GetValue<int>();
        if (second is not null && usage[second] is { } b && b.GetValueKind() == JsonValueKind.Number)
            return b.GetValue<int>();
        return 0;
    }

    private static int ReadNested(JsonObject? usage, string parent, string child)
    {
        var node = usage?[parent]?[child];
        return node is not null && node.GetValueKind() == JsonValueKind.Number ? node.GetValue<int>() : 0;
    }
}

/// <summary>Starts a trace for each request.</summary>
public class UsageTracker
{
    private readonly UsageStore _store;
    private readonly PricingOptions _pricing;

    public UsageTracker(UsageStore store, Microsoft.Extensions.Options.IOptions<PricingOptions> pricing)
    {
        _store = store;
        _pricing = pricing.Value;
    }

    public UsageTrace Begin(string provider, string model, string question, string enabledSources) =>
        new(_store, _pricing, provider, model, question, enabledSources);
}
