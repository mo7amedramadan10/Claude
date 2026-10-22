namespace ChatToDashboard.Api.Analyst;

/// <summary>SSE stage ids from spec section 4 — always emitted in this order for every
/// question, even when a given stage's real work is near-instant (e.g. "query" for a general-
/// knowledge answer, "verify" for a non-org_data one).</summary>
public static class AnalystStages
{
    public const string Understand = "understand";
    public const string Sources = "sources";
    public const string Query = "query";
    public const string Verify = "verify";
    public const string Compose = "compose";
}

public static class AnalystStageStates
{
    public const string Running = "running";
    public const string Done = "done";
    public const string Failed = "failed";
}

/// <summary>Pushes one real SSE "stage" event — see AnalystController.Ask, the only
/// implementation (writes a "stage" SSE frame and flushes). Never a timer: every call site is a
/// genuine state transition in the pipeline (spec section 4's explicit requirement).</summary>
public delegate Task AnalystStageCallback(string stage, string state);

/// <summary>One query_data/query_client_data call the model made during the tool loop, captured
/// so the final answer_template's result_query_id can point back at it — see spec section 5
/// step 5: the model never restates rows, it only names which of its own tool calls produced
/// the result table.</summary>
public record AnalystCapturedQuery(string Sql, IReadOnlyList<string> Columns, IReadOnlyList<Dictionary<string, object?>> Rows);

public enum AnalystLoopOutcome { OrgData, General, OutOfScope, Failed }

/// <summary>What OllamaClient.GenerateAnalystAsync returns — see its own remarks for the full
/// pipeline this is one part of.</summary>
public record AnalystLoopResult(
    AnalystLoopOutcome Outcome,
    AnalystModelResult? Model,
    IReadOnlyDictionary<string, AnalystCapturedQuery> CapturedQueries,
    string? Error);
