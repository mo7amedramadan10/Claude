using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Models;
using ChatToDashboard.Api.Templates;

namespace ChatToDashboard.Api.SchemaLibrary;

/// <summary>
/// The deterministic half of the field/table matching design (see the conversation that settled
/// this): given a template widget's own draft Table/Sql (its "concept"), tries every known
/// candidate name from <see cref="SchemaAliasStore"/> — highest score first — against each
/// currently-enabled integration's real, discovered schema, entirely in code, no LLM call. A
/// resolved widget becomes a strong hint appended to the build prompt (see
/// TemplatePromptService) so the model uses the real name directly instead of guessing; an
/// unresolved one is simply left for the model to find the normal way (reading the full schema
/// itself via query_client_data), same as before this feature existed. Either way, once a widget
/// actually gets built against a real integration, <see cref="RecordLearningAsync"/> feeds
/// whatever real name the model ended up using back into the library — see ChatController's
/// post-build reconciliation, the only caller.
/// </summary>
public class SchemaMatchingService
{
    private readonly SchemaAliasStore _aliases;

    public SchemaMatchingService(SchemaAliasStore aliases) => _aliases = aliases;

    public record MatchedWidget(
        string Title, string IntegrationId, string IntegrationName,
        string RealTable, string? RealValueColumn, string? RealGroupColumn);

    /// <summary>Tries to resolve every widget that carries a draft Table/Sql against whichever of
    /// <paramref name="integrations"/> has a matching table — case-insensitive, first candidate
    /// (by score) that exists in that integration's real schema wins. Widgets with no draft
    /// concept, or that match no known candidate in any enabled integration, simply don't appear
    /// in the result — never an error, since the whole point is "help when we can, stay out of
    /// the way otherwise."</summary>
    public async Task<IReadOnlyDictionary<string, MatchedWidget>> MatchAsync(
        IReadOnlyList<BuiltinDashboardWidgetSpec> widgets,
        IReadOnlyList<AnalyticsTools.EnabledIntegration> integrations,
        CancellationToken ct = default)
    {
        var result = new Dictionary<string, MatchedWidget>(StringComparer.OrdinalIgnoreCase);
        if (integrations.Count == 0) return result;

        var parsedSchemas = integrations
            .Select(i => (Integration: i, Tables: ClientSchemaParser.Parse(i.SchemaDescription)))
            .ToList();

        foreach (var w in widgets)
        {
            if (string.IsNullOrWhiteSpace(w.Table) || result.ContainsKey(w.Title)) continue;

            var tableConceptId = SchemaConceptExtractor.TableConceptId(w.Table);
            var tableCandidates = await _aliases.GetCandidatesAsync(tableConceptId, ct);
            if (tableCandidates.Count == 0) continue;

            foreach (var (integration, tables) in parsedSchemas)
            {
                var realTable = tableCandidates
                    .Select(c => ClientSchemaParser.FindTable(tables, c.Name))
                    .FirstOrDefault(t => t is not null);
                if (realTable is null) continue;

                var parsed = SchemaConceptExtractor.Parse(w.Sql);
                var realValue = await ResolveColumnAsync(w.Table, parsed.ValueColumn, realTable.Value, ct);
                var realGroup = await ResolveColumnAsync(w.Table, parsed.GroupColumn, realTable.Value, ct);

                result[w.Title] = new MatchedWidget(
                    w.Title, integration.Id, integration.Name, realTable.Value.Name, realValue, realGroup);
                break; // first integration with a table match wins — a widget belongs to one source.
            }
        }
        return result;
    }

    private async Task<string?> ResolveColumnAsync(
        string draftTable, string? draftColumn, ClientSchemaParser.ParsedTable realTable, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(draftColumn)) return null;
        var conceptId = SchemaConceptExtractor.ColumnConceptId(draftTable, draftColumn);
        var candidates = await _aliases.GetCandidatesAsync(conceptId, ct);
        foreach (var c in candidates)
        {
            var found = ClientSchemaParser.FindColumn(realTable, c.Name);
            if (found is not null) return found;
        }
        return null;
    }

    /// <summary>Renders resolved matches as a prompt block telling the model exactly which real
    /// table/columns to use for which required widget — appended after
    /// TemplatePromptService.BuildRequiredWidgetsBlock. Null when nothing resolved, so an
    /// unmatched build's prompt is byte-for-byte what it was before this feature.</summary>
    public static string? BuildPromptHintBlock(IReadOnlyDictionary<string, MatchedWidget> matches)
    {
        if (matches.Count == 0) return null;

        var lines = matches.Values.Select(m =>
        {
            var cols = new List<string>();
            if (m.RealValueColumn is not null) cols.Add($"عمود القيمة: {m.RealValueColumn}");
            if (m.RealGroupColumn is not null) cols.Add($"عمود التجميع: {m.RealGroupColumn}");
            var colsText = cols.Count > 0 ? $"، {string.Join("، ", cols)}" : "";
            return $"- «{m.Title}»: الجدول {m.RealTable}{colsText} (integrationId: \"{m.IntegrationId}\" — {m.IntegrationName}).";
        });

        return "تطابق محتمل من مكتبة الحقول والجداول — استخدمه مباشرة عبر query_client_data بدل " +
               "ما تدوّر من الصفر، بعد ما تتأكد إنه فعلاً موجود في الجدول المذكور:\n" +
               string.Join("\n", lines) +
               "\nلو الجدول/العمود المذكور مش موجود فعليًا أو مش مناسب، تجاهل الاقتراح ودوّر بنفسك " +
               "في البنية الكاملة زي العادة.";
    }

    /// <summary>Feeds a successfully-built widget's real, model-chosen SQL back into the library
    /// — see SchemaAliasStore.RecordLearnedAsync for the scoring rule. Parses
    /// <paramref name="builtSql"/> with the exact same shape-based parser used to seed the
    /// library in the first place (SchemaConceptExtractor), so a learned alias and a seed alias
    /// are always directly comparable. Silently does nothing for a query that shape doesn't fit
    /// (a join, a different aggregate shape, multiple aggregates) — never required for a widget
    /// to "count" as built; this is pure opportunistic learning, not validation.</summary>
    public async Task RecordLearningAsync(
        BuiltinDashboardWidgetSpec requiredSpec, string builtSql, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(requiredSpec.Table)) return;
        var parsed = SchemaConceptExtractor.Parse(builtSql);
        if (string.IsNullOrWhiteSpace(parsed.Table)) return;

        var tableConceptId = SchemaConceptExtractor.TableConceptId(requiredSpec.Table);
        await _aliases.RecordLearnedAsync(tableConceptId, SchemaConceptKinds.Table, null, requiredSpec.Table, parsed.Table, ct);

        var draft = SchemaConceptExtractor.Parse(requiredSpec.Sql);
        if (!string.IsNullOrWhiteSpace(draft.ValueColumn) && !string.IsNullOrWhiteSpace(parsed.ValueColumn))
        {
            var id = SchemaConceptExtractor.ColumnConceptId(requiredSpec.Table, draft.ValueColumn);
            await _aliases.RecordLearnedAsync(id, SchemaConceptKinds.Column, tableConceptId, draft.ValueColumn, parsed.ValueColumn, ct);
        }
        if (!string.IsNullOrWhiteSpace(draft.GroupColumn) && !string.IsNullOrWhiteSpace(parsed.GroupColumn))
        {
            var id = SchemaConceptExtractor.ColumnConceptId(requiredSpec.Table, draft.GroupColumn);
            await _aliases.RecordLearnedAsync(id, SchemaConceptKinds.Column, tableConceptId, draft.GroupColumn, parsed.GroupColumn, ct);
        }
    }
}
