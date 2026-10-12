using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Sources;

namespace ChatToDashboard.Api.Interactive;

/// <summary>One column a template's engine needs from one of its logical tables — see
/// <see cref="InteractiveTableContract"/>.</summary>
/// <param name="Key">The key the engine's provider code reads the matched real name back by
/// (see <see cref="MatchedInteractiveTable.Columns"/>) — not shown to the model as-is, just
/// used as a dictionary key on both sides of the match.</param>
/// <param name="Description">Plain Arabic description of what this column holds, shown to
/// the model so it can judge which real column (if any) corresponds to it.</param>
/// <param name="Required">False for an optional column whose absence shouldn't fail the
/// whole table match (e.g. a nice-to-have field the engine can render blank).</param>
public record InteractiveColumnContract(string Key, string Description, bool Required = true);

/// <summary>One logical table a §4.16+ interactive template's data contract needs — see each
/// provider's own Contract definition for the real per-template shapes (e.g. crm-pipeline's
/// customers/stages/new_customers_daily/pipeline_stats).</summary>
/// <param name="Key">The key the provider reads this table's match back by.</param>
/// <param name="Description">Plain Arabic description of the table's purpose/content.</param>
/// <param name="Columns">Every column the engine might read from this table.</param>
/// <param name="Required">False for a table whose absence still allows a degraded-but-linked
/// render (none of the six Phase-1 providers use this yet — every one of their tables is
/// required — but kept for future templates with genuinely optional data).</param>
public record InteractiveTableContract(
    string Key, string Description, IReadOnlyList<InteractiveColumnContract> Columns, bool Required = true);

/// <summary>A successfully matched table: the real table name exactly as it appears in the
/// project's own schema (safe to reference directly in SQL — see DataStore's own "name the
/// model sees" convention, which this reuses verbatim), and logical column key -> real column
/// name for every column that matched (an optional column that didn't match is simply absent
/// from this dictionary, not present with a null value).</summary>
public record MatchedInteractiveTable(string RealTable, IReadOnlyDictionary<string, string> Columns)
{
    public string? Column(string key) => Columns.GetValueOrDefault(key);
}

/// <summary>The result of successfully matching every required table (and whichever optional
/// ones also matched) of one template's contract against one project's real data — everything
/// a provider needs to build and run its own SQL.</summary>
public record MatchedInteractiveContract(
    IReadOnlyDictionary<string, MatchedInteractiveTable> Tables, AnalyticsTools.SourceContext Context);

/// <summary>
/// Shared real-data matching for every §4.16+ interactive template provider — one LLM call
/// per BuildAsync that maps a template's declared logical "tables" (see
/// <see cref="InteractiveTableContract"/>) onto the real tables a project has actually
/// uploaded/connected, then re-verifies every claimed match against the real catalog before
/// trusting it (the model only ever picks from real names it was given — see
/// InteractiveSchemaMatchSystemPrompt — but this still independently confirms rather than
/// assuming the model followed that rule). A provider that gets a null match back from
/// <see cref="MatchAsync"/> renders the standard "not linked yet" empty state, exactly like
/// the StubInteractiveDataProvider it replaces — never a guess, never fabricated data.
/// </summary>
public class InteractiveContractMatcher
{
    private readonly AnalyticsTools _tools;
    private readonly IIntegrationSetupAssistant _assistant;

    public InteractiveContractMatcher(AnalyticsTools tools, IIntegrationSetupAssistant assistant)
    {
        _tools = tools;
        _assistant = assistant;
    }

    public async Task<MatchedInteractiveContract?> MatchAsync(
        string projectId, IReadOnlyList<InteractiveTableContract> contract, CancellationToken ct)
    {
        var selection = SourceSelection.AllEnabled();
        selection.ProjectId = projectId;
        selection.IsAdmin = true; // a system-level build, not on behalf of one user's own file permissions.
        var context = await _tools.DescribeSourcesAsync(selection, ct);
        var realTables = await _tools.GetVisibleTablesAsync(context, ct);
        if (realTables.Count == 0) return null;

        var contractText = BuildContractText(contract);
        var catalogText = BuildCatalogText(realTables);
        var llmMatch = await _assistant.MatchSchemaContractAsync(contractText, catalogText, null, ct);
        if (!llmMatch.Matched || llmMatch.Tables is null) return null;

        var resolved = new Dictionary<string, MatchedInteractiveTable>();
        foreach (var table in contract)
        {
            if (!llmMatch.Tables.TryGetValue(table.Key, out var claimed) || claimed.RealTable is null)
            {
                if (table.Required) return null;
                continue;
            }

            // Re-verify: the claimed real table must actually be in this project's catalog —
            // never trust an LLM-returned name at face value, same defense ExecuteQueryAsync's
            // own permission gate already applies to model-written SQL elsewhere in this app.
            var realTable = realTables.FirstOrDefault(t => string.Equals(t.Table, claimed.RealTable, StringComparison.OrdinalIgnoreCase));
            if (realTable is null)
            {
                if (table.Required) return null;
                continue;
            }

            var columns = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var col in table.Columns)
            {
                var claimedCol = claimed.Columns.GetValueOrDefault(col.Key);
                var realCol = claimedCol is null ? null
                    : realTable.Columns.FirstOrDefault(c => string.Equals(c.Name, claimedCol, StringComparison.OrdinalIgnoreCase))?.Name;
                if (realCol is null)
                {
                    if (col.Required) { realTable = null; break; }
                    continue;
                }
                columns[col.Key] = realCol;
            }
            if (realTable is null) { if (table.Required) return null; continue; }

            resolved[table.Key] = new MatchedInteractiveTable(realTable.Table, columns);
        }

        // Every required table must have resolved — a required table skipped above (optional
        // branch never taken for it) would otherwise silently leave callers with a missing key.
        if (contract.Where(t => t.Required).Any(t => !resolved.ContainsKey(t.Key))) return null;

        return new MatchedInteractiveContract(resolved, context);
    }

    /// <summary>Runs one read-only SELECT through the exact same permission/validation gate
    /// every other query in this app goes through — see AnalyticsTools.RunReadOnlyQueryAsync.
    /// Returns the parsed rows (column name -> value), or an empty list if the query fails
    /// (a provider should treat that the same as "no rows", not surface a raw SQL error to the
    /// dashboard — the <paramref name="sql"/> here is built entirely from real, re-verified
    /// names, so a failure here means something changed server-side between matching and
    /// querying, not a bad query).</summary>
    public async Task<List<Dictionary<string, System.Text.Json.JsonElement>>> QueryAsync(
        string sql, AnalyticsTools.SourceContext context, CancellationToken ct)
    {
        var (result, isError) = await _tools.RunReadOnlyQueryAsync(sql, context, ct);
        if (isError) return new();
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(result);
            if (!doc.RootElement.TryGetProperty("rows", out var rowsEl) || rowsEl.ValueKind != System.Text.Json.JsonValueKind.Array)
                return new();
            var rows = new List<Dictionary<string, System.Text.Json.JsonElement>>();
            foreach (var rowEl in rowsEl.EnumerateArray())
            {
                var row = new Dictionary<string, System.Text.Json.JsonElement>(StringComparer.OrdinalIgnoreCase);
                foreach (var prop in rowEl.EnumerateObject()) row[prop.Name] = prop.Value;
                rows.Add(row);
            }
            return rows;
        }
        catch (System.Text.Json.JsonException)
        {
            return new();
        }
    }

    private static string BuildContractText(IReadOnlyList<InteractiveTableContract> contract)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var t in contract)
        {
            sb.AppendLine($"- جدول \"{t.Key}\": {t.Description}");
            foreach (var c in t.Columns)
                sb.AppendLine($"  - عمود \"{c.Key}\": {c.Description}{(c.Required ? "" : " (اختياري)")}");
        }
        return sb.ToString();
    }

    private static string BuildCatalogText(IReadOnlyList<Data.TableSchema> tables)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var t in tables)
            sb.AppendLine($"- {t.Table} ({string.Join("، ", t.Columns.Select(c => c.Name))})");
        return sb.ToString();
    }
}
