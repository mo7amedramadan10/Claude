using ChatToDashboard.Api.Data;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Models;
using ChatToDashboard.Api.Sources;
using ChatToDashboard.Api.Users;

namespace ChatToDashboard.Api.Inquiry;

/// <summary>
/// Masks a saved conversation's "data" blocks the reopening user can no longer access —
/// continuing (or just re-reading) a saved conversation always runs under the user's *current*
/// permissions, never whatever they had when it was first asked (see AnalyticsTools.
/// CheckSourcePermission, the exact same table/category gate query_data itself enforces).
/// Applied server-side so a masked block's real text/source/data/table never reaches the
/// client at all — mirrors DashboardAccessService's role in the Active-dashboard model, just
/// per-block instead of per-dashboard.
/// </summary>
public class InquiryAccessService
{
    private readonly AnalyticsTools _tools;
    private readonly PermissionsService _permissions;

    public InquiryAccessService(AnalyticsTools tools, PermissionsService permissions)
    {
        _tools = tools;
        _permissions = permissions;
    }

    /// <summary>Returns a new list of turns with every inaccessible "data" block replaced by a
    /// masked placeholder — the input list/blocks are never mutated in place, so a caller that
    /// also needs the real content (e.g. building the next model call's own context, which the
    /// spec explicitly says must exclude masked blocks) can still tell the two apart via
    /// <see cref="InquiryBlock.Masked"/> without a second pass over storage.</summary>
    public async Task<List<ConversationTurn>> MaskInaccessibleBlocksAsync(
        AppUser user, IReadOnlyList<ConversationTurn> turns, SourceSelection? sources, CancellationToken ct = default)
    {
        var selection = await _permissions.GetEffectiveSelectionAsync(user, sources ?? SourceSelection.AllEnabled(), ct);
        var context = await _tools.DescribeSourcesAsync(selection, ct);
        var schema = await _tools.GetSchemaAsync(ct);

        var result = new List<ConversationTurn>(turns.Count);
        foreach (var turn in turns)
        {
            if (turn.Blocks is null) { result.Add(turn); continue; }

            var maskedBlocks = turn.Blocks.Select(b => MaskIfNeeded(b, context, schema)).ToList();
            result.Add(new ConversationTurn
            {
                Role = turn.Role, Text = turn.Text, CreatedAt = turn.CreatedAt, Blocks = maskedBlocks,
                FollowUps = turn.FollowUps,
            });
        }
        return result;
    }

    /// <summary>Same check for exactly one block — used by the convert endpoint to refuse
    /// converting a block the requesting user can no longer reach, regardless of what the
    /// client itself believes about it.</summary>
    public async Task<string?> CheckBlockAccessAsync(
        AppUser user, InquiryBlock block, SourceSelection? sources, CancellationToken ct = default)
    {
        if (block.Kind != InquiryBlockKinds.Data || string.IsNullOrWhiteSpace(block.Table)) return null;
        var selection = await _permissions.GetEffectiveSelectionAsync(user, sources ?? SourceSelection.AllEnabled(), ct);
        var context = await _tools.DescribeSourcesAsync(selection, ct);
        return AnalyticsTools.CheckSourcePermission(block.Table, context, await _tools.GetSchemaAsync(ct));
    }

    private static InquiryBlock MaskIfNeeded(
        InquiryBlock block, AnalyticsTools.SourceContext context, IReadOnlyList<TableSchema> schema)
    {
        if (block.Kind != InquiryBlockKinds.Data || string.IsNullOrWhiteSpace(block.Table)) return block;
        var reason = AnalyticsTools.CheckSourcePermission(block.Table, context, schema);
        if (reason is null) return block;

        return new InquiryBlock
        {
            Kind = block.Kind,
            Text = "هذا الجزء مبني على مصدر لم يعد متاحًا لك.",
            Masked = true,
            MaskedReason = reason,
            // Source/Table/Data/ExtractedAt deliberately left at their defaults — the masked
            // reason above is the only thing this block still carries.
        };
    }
}
