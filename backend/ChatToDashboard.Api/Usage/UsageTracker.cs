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
    private readonly CostCalculator _cost;
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private readonly UsageRecord _record;

    internal UsageTrace(UsageStore store, CostCalculator cost, string provider, string model,
        string question, string enabledSources)
    {
        _store = store;
        _cost = cost;
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
        // Anthropic/OpenAI nest counts under "usage"; Ollama's native API has no "usage"
        // object at all and puts prompt_eval_count/eval_count on the response's top level —
        // fall back to the response object itself so that case is read too.
        var usage = response["usage"]?.AsObject() ?? response;
        var turn = new UsageTurn
        {
            Turn = _record.Turns.Count + 1,
            // Each provider names these differently; read whichever is present.
            InputTokens = Read(usage, "input_tokens", "prompt_tokens", "prompt_eval_count"),
            OutputTokens = Read(usage, "output_tokens", "completion_tokens", "eval_count"),
            CacheReadTokens = Read(usage, "cache_read_input_tokens")
                + ReadNested(usage, "prompt_tokens_details", "cached_tokens"),
            CacheWriteTokens = Read(usage, "cache_creation_input_tokens"),
            StopReason = response["stop_reason"]?.GetValue<string>()
                ?? response["choices"]?[0]?["finish_reason"]?.GetValue<string>()
                ?? response["done_reason"]?.GetValue<string>(),
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
        _record.EstimatedCost = _cost.Estimate(_record);
        await _store.SaveAsync(_record, ct);
    }

    private static int Read(JsonObject? usage, params string[] names)
    {
        if (usage is null) return 0;
        foreach (var name in names)
            if (usage[name] is { } v && v.GetValueKind() == JsonValueKind.Number) return v.GetValue<int>();
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
    private readonly CostCalculator _cost;

    public UsageTracker(UsageStore store, CostCalculator cost)
    {
        _store = store;
        _cost = cost;
    }

    public UsageTrace Begin(string provider, string model, string question, string enabledSources) =>
        new(_store, _cost, provider, model, question, enabledSources);
}
