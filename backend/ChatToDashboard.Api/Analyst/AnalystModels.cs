namespace ChatToDashboard.Api.Analyst;

/// <summary>See the Project Analyst spec, section 5 step 1 — a question is classified before
/// anything else runs, and the classification decides whether the tool loop (org_data), a
/// tool-free general-knowledge answer (general), or an outright refusal (out_of_scope) happens.</summary>
public static class AnalystKnowledgeScopes
{
    public const string OrgData = "org_data";
    public const string General = "general";
    public const string OutOfScope = "out_of_scope";
}

public static class AnalystRoles
{
    public const string User = "user";
    public const string Assistant = "assistant";
}

/// <summary>The shape of one result's data — a display hint for the frontend (Phase 2), not
/// something this phase validates beyond "is it one of these".</summary>
public static class AnalystResultTypes
{
    public const string Table = "table";
    public const string Ranking = "ranking";
    public const string Comparison = "comparison";
    public const string Summary = "summary";

    public static bool IsValid(string? value) =>
        value is Table or Ranking or Comparison or Summary;
}

public static class AnalystSourceKinds
{
    public const string System = "system";
    public const string File = "file";
}

/// <summary>See spec section 3. One saved/unsaved analyst session — "جلسة جديدة" vs. "جلسات
/// تحليل". RollingSummary/FindingsLedgerJson exist in the schema from Phase 1 on (so later
/// phases don't need a migration) but are only ever written starting Phase 6 (see spec section
/// 12) — left null until then.</summary>
public class AnalystConversation
{
    public string Id { get; set; } = "";
    public string ProjectId { get; set; } = "";
    public string OwnerUserId { get; set; } = "";
    public string Title { get; set; } = "";
    public bool IsSaved { get; set; }
    public string? RollingSummary { get; set; }
    public string? FindingsLedgerJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>One turn in a conversation. <see cref="Text"/> is already placeholder-filled (what
/// the user reads); <see cref="TemplateText"/> keeps the model's original answer_template with
/// its {{...}} tokens intact — not read anywhere yet in Phase 1, but stored from the start so
/// Phase 3/4 (re-rendering after a reverify bumps DataVersion) never needs a migration to add
/// it.</summary>
public class AnalystMessage
{
    public string Id { get; set; } = "";
    public string ConversationId { get; set; } = "";
    public string Role { get; set; } = "";
    public string Text { get; set; } = "";
    public string? TemplateText { get; set; }
    public string? ResultId { get; set; }
    public string? KnowledgeScope { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>One query result a question produced — see spec section 3/8. <see
/// cref="VerificationJson"/> in this phase is always the Phase-1 placeholder shape (see
/// AnalystVerification.NotYetChecked) — the real C1-C4 checks are Phase 4's job; storing the
/// column from the start avoids a later migration.</summary>
public class AnalystResult
{
    public string Id { get; set; } = "";
    public string ConversationId { get; set; } = "";
    public string ProjectId { get; set; } = "";
    public string Title { get; set; } = "";
    public string ResultType { get; set; } = AnalystResultTypes.Table;
    public string ColumnsJson { get; set; } = "[]";
    public string RowsJson { get; set; } = "[]";
    public string? KeyColumn { get; set; }
    public string? PrimaryMeasure { get; set; }
    public string? ExecutedSql { get; set; }
    public string? AuditSql { get; set; }
    public string VerificationJson { get; set; } = "{}";
    public int DataVersion { get; set; } = 1;
    public bool IsTruncated { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>One data source a result drew on — see spec section 3/8 ("مصادر البيانات" panel).</summary>
public class AnalystResultSource
{
    public string Id { get; set; } = "";
    public string ResultId { get; set; } = "";
    public string SourceId { get; set; } = "";
    public string SourceDisplayName { get; set; } = "";
    public string SourceKind { get; set; } = AnalystSourceKinds.System;
    public string TablesJson { get; set; } = "[]";
    public DateTime? SourceLastUpdatedAt { get; set; }
    public bool IsStale { get; set; }
}
