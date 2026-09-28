using System.Text.Json;
using System.Text.Json.Serialization;
using ChatToDashboard.Api.Sources;

namespace ChatToDashboard.Api.Models;

public static class InquiryBlockKinds
{
    public const string Data = "data";
    public const string Knowledge = "knowledge";
}

/// <summary>
/// One block of an Inquiries-mode answer — every part of an answer belongs to exactly one of
/// two kinds (see AnalyticsTools.BuildInquirySystemPrompt's "من بيانات الجهة"/"من خارج
/// البيانات" rule). "data" is grounded in an actual tool result and carries the same
/// provenance a dashboard widget's own ⓘ popover shows (<see cref="Source"/>) plus the real
/// table it came from and the values themselves, so it can later be converted to a widget.
/// "knowledge" is general reasoning/explanation/brainstorming and must never introduce a new
/// figure about the organization's own data.
/// </summary>
public class InquiryBlock
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = InquiryBlockKinds.Data;

    [JsonPropertyName("text")]
    public string Text { get; set; } = "";

    /// <summary>Two sentences — where the data came from, then how it was calculated. Same
    /// convention as <see cref="DashboardWidget.Source"/>. Only meaningful for a "data" block.</summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /// <summary>The real table this block's figure came from — the same identifier a wizard
    /// widget's own query.table carries. Used both to build the findings ledger and, when a
    /// saved conversation is reopened, to re-check the reopening user's permission on it (see
    /// AnalyticsTools.CheckSourcePermission and InquiryAccessService).</summary>
    [JsonPropertyName("table")]
    public string? Table { get; set; }

    /// <summary>The real value(s) behind this block — same shape a tool result already has (a
    /// single number, an object, or an array of rows). Only meaningful for a "data" block; used
    /// verbatim by "➕ حوّله لداشبورد", never re-derived.</summary>
    [JsonPropertyName("data")]
    public JsonElement Data { get; set; }

    /// <summary>Set server-side only (see InquiryAccessService) when a saved conversation is
    /// reopened and this block's own <see cref="Table"/> is no longer accessible to the
    /// reopening user — at that point <see cref="Text"/>/<see cref="Source"/>/<see
    /// cref="Data"/>/<see cref="Table"/> have already been stripped down to defaults before
    /// this ever reaches the client, so the masked content never crosses the wire.</summary>
    [JsonPropertyName("masked")]
    public bool Masked { get; set; }

    [JsonPropertyName("maskedReason")]
    public string? MaskedReason { get; set; }

    /// <summary>When this block's figure was actually queried — shown next to it so a later
    /// re-read of a saved conversation reads as "as of that date", not live. Only meaningful
    /// for a "data" block.</summary>
    [JsonPropertyName("extractedAt")]
    public DateTime? ExtractedAt { get; set; }

    /// <summary>Real, human-readable source names actually touched by the tool calls that
    /// produced this turn's answer (see AnalyticsTools.ExtractExaminedSources) — set purely
    /// server-side from the recorded tool-call log, never from text the model wrote, so this
    /// "تم فحص" list can't be fabricated. Null/empty means omit the row entirely rather than
    /// show it empty.</summary>
    [JsonPropertyName("examined")]
    public List<string>? Examined { get; set; }
}

/// <summary>
/// The structured JSON one Inquiries-mode model turn must return (see
/// AnalyticsTools.BuildInquirySystemPrompt) — an ordered list of labeled blocks, plus the
/// model's own refreshed rolling summary of the conversation so far (everything before the
/// verbatim recent turns it was shown this call — see
/// AnalyticsTools.ComposeInquiryUserMessage) for the store to persist and hand back next turn.
/// </summary>
public class InquiryResponse
{
    [JsonPropertyName("blocks")]
    public List<InquiryBlock> Blocks { get; set; } = new();

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = "";

    /// <summary>2-3 suggested follow-up questions, shown as chips under the latest reply only
    /// — only ever populated when InquirySuggestFollowUps is enabled (see
    /// AnalyticsTools.BuildInquirySystemPrompt) and only ever questions, never a statement
    /// asserting something about the data (enforced by prompt + Validate() below).</summary>
    [JsonPropertyName("followUps")]
    public List<string>? FollowUps { get; set; }

    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (Blocks.Count == 0)
            errors.Add("\"blocks\" must be a non-empty array — every answer is at least one labeled block.");
        for (var i = 0; i < Blocks.Count; i++)
        {
            var b = Blocks[i];
            if (string.IsNullOrWhiteSpace(b.Text))
                errors.Add($"blocks[{i}].text is required.");
            if (b.Kind != InquiryBlockKinds.Data && b.Kind != InquiryBlockKinds.Knowledge)
                errors.Add($"blocks[{i}].kind must be \"{InquiryBlockKinds.Data}\" or \"{InquiryBlockKinds.Knowledge}\".");
            if (b.Kind == InquiryBlockKinds.Data && string.IsNullOrWhiteSpace(b.Source))
                errors.Add($"blocks[{i}] is a \"{InquiryBlockKinds.Data}\" block and requires \"source\" " +
                           "(two sentences: where the data came from, then how it was calculated).");
        }
        return errors;
    }
}

public class InquiryApiResponse
{
    [JsonPropertyName("answer")]
    public InquiryResponse? Answer { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

/// <summary>One turn of a saved Inquiries conversation — either the user's own question
/// (<see cref="Text"/> set, <see cref="Blocks"/> null) or the model's structured, block-labeled
/// answer (<see cref="Blocks"/> set, <see cref="Text"/> null).</summary>
public class ConversationTurn
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = ""; // "user" | "bot"

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("blocks")]
    public List<InquiryBlock>? Blocks { get; set; }

    /// <summary>Carried over from the InquiryResponse that produced this turn, so a reopened
    /// conversation still shows the chips under its latest reply, not just a freshly-asked one.</summary>
    [JsonPropertyName("followUps")]
    public List<string>? FollowUps { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

public static class ConversationRoles
{
    public const string User = "user";
    public const string Bot = "bot";
}

/// <summary>Body of POST /api/inquiry — asks a question inside a saved conversation, creating
/// one implicitly on its first message (no separate empty-shell "create" step exists).</summary>
public class InquiryAskRequest
{
    /// <summary>Absent/empty starts a brand-new saved conversation; the server returns its id.</summary>
    [JsonPropertyName("conversationId")]
    public string? ConversationId { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = "";

    [JsonPropertyName("sources")]
    public SourceSelection? Sources { get; set; }

    /// <summary>See <see cref="ChatRequest.Lang"/>.</summary>
    [JsonPropertyName("lang")]
    public string? Lang { get; set; }
}

public class InquiryAskApiResponse
{
    [JsonPropertyName("conversationId")]
    public string? ConversationId { get; set; }

    /// <summary>Only set on the turn that actually created the conversation — the frontend
    /// uses it to label a freshly-started conversation without a second round trip.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("turn")]
    public ConversationTurn? Turn { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

/// <summary>One row of the saved-conversations list inside الاستفسارات.</summary>
public class ConversationSummaryDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}

/// <summary>A reopened conversation's full display transcript — every "data" block whose
/// source the reopening user can no longer access has already been masked server-side (see
/// InquiryAccessService) before this is serialized, so masked content never reaches the
/// client.</summary>
public class ConversationDetailDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("turns")]
    public List<ConversationTurn> Turns { get; set; } = new();
}

/// <summary>Body of POST /api/inquiry/convert — "🔄 حوّله لداشبورد" on one specific "data" block
/// of one already-saved conversation turn. Re-reads the block from storage (never trusts the
/// client's own copy of it) so a masked block can never be converted regardless of what the
/// client sends.</summary>
public class ConvertInquiryBlockRequest
{
    [JsonPropertyName("conversationId")]
    public string ConversationId { get; set; } = "";

    [JsonPropertyName("turnIndex")]
    public int TurnIndex { get; set; }

    [JsonPropertyName("blockIndex")]
    public int BlockIndex { get; set; }

    /// <summary>The dashboard currently on screen, present only when the user chose "إضافة"
    /// on the replace-or-add prompt — same "add to it, don't replace" contract as
    /// <see cref="ChatRequest.CurrentDashboard"/>. Absent (whether because nothing is on
    /// screen yet, or because the user chose "استبدال") builds a fresh dashboard containing
    /// just this block's data — the frontend is what decides which of those two absent-vs-
    /// present cases applies; this endpoint itself only ever knows "add" vs "fresh".</summary>
    [JsonPropertyName("currentDashboard")]
    public DashboardStateInput? CurrentDashboard { get; set; }

    [JsonPropertyName("sources")]
    public SourceSelection? Sources { get; set; }

    /// <summary>See <see cref="ChatRequest.Lang"/>.</summary>
    [JsonPropertyName("lang")]
    public string? Lang { get; set; }
}
