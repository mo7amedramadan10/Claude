using System.Text.Json;
using System.Text.RegularExpressions;

namespace ChatToDashboard.Api.SchemaLibrary;

/// <summary>
/// Builds the one-time startup seed for <see cref="SchemaAliasStore"/> by mining kpi-library.js's
/// own draft "expected SELECT" — every row's 6th element, [table, sql] — for the (table,
/// value-column, group-column) concepts it already implies (see SchemaConceptExtractor). No new
/// data-collection effort: this is the exact same 3,839-row library item-c already generated and
/// shipped, just read from a new angle. For each distinct concept found, seeds a handful of
/// mechanical name variants (the generated name itself, a couple of casing conventions, and —
/// for tables — the name with this app's internal "staging_" prefix stripped) rather than a
/// hand-curated synonym dictionary, which isn't feasible at this scale; real breadth comes from
/// SchemaAliasStore.RecordLearnedAsync as builds against real client schemas happen over time.
/// </summary>
public static class SchemaAliasSeedGenerator
{
    private const string Prefix = "window.JEEM_KPI_LIBRARY=";

    public static IReadOnlyList<SchemaAliasEntry> BuildFromKpiLibraryJs(string fileContent)
    {
        var idx = fileContent.IndexOf(Prefix, StringComparison.Ordinal);
        if (idx < 0) return Array.Empty<SchemaAliasEntry>();
        var json = fileContent[(idx + Prefix.Length)..].Trim();
        if (json.EndsWith(';')) json = json[..^1];

        using var doc = JsonDocument.Parse(json);
        var rows = doc.RootElement.GetProperty("rows");

        // ConceptId -> (Kind, ParentTable, Label, OriginalName) — collected once per distinct
        // concept, in first-seen order, before any alias rows are built, so a concept seen under
        // many KPI rows still only gets one set of seed aliases. OriginalName keeps the name
        // exactly as generated (e.g. PascalCase "RateSatisfactionCustomers") — ConceptId itself
        // is lowercased for stable matching (see SchemaConceptExtractor), so variant generation
        // below must read case/word-boundary information from OriginalName, never back out of
        // the already-lowercased ConceptId (which has nothing left to split on).
        var concepts = new Dictionary<string, (string Kind, string? ParentTable, string Label, string OriginalName)>();

        foreach (var row in rows.EnumerateArray())
        {
            // row = [catIdx, subIdx, nameAr, nameEn, mtypeIdx, [table, sql]]
            if (row.GetArrayLength() < 6) continue;
            var queryEl = row[5];
            if (queryEl.ValueKind != JsonValueKind.Array || queryEl.GetArrayLength() < 2) continue;
            var table = queryEl[0].GetString();
            var sql = queryEl[1].GetString();
            if (string.IsNullOrWhiteSpace(table)) continue;

            var tableConceptId = SchemaConceptExtractor.TableConceptId(table);
            concepts.TryAdd(tableConceptId, (SchemaConceptKinds.Table, null, Humanize(table), table));

            var parsed = SchemaConceptExtractor.Parse(sql);
            // Trust the row's own literal table (authoritative) over whatever FROM parsed back
            // out of its own sql — they always agree for library-generated rows, but the literal
            // field is the one actually meant to be authoritative.
            if (!string.IsNullOrWhiteSpace(parsed.ValueColumn))
            {
                var id = SchemaConceptExtractor.ColumnConceptId(table, parsed.ValueColumn);
                concepts.TryAdd(id, (SchemaConceptKinds.Column, tableConceptId, Humanize(parsed.ValueColumn), parsed.ValueColumn));
            }
            if (!string.IsNullOrWhiteSpace(parsed.GroupColumn))
            {
                var id = SchemaConceptExtractor.ColumnConceptId(table, parsed.GroupColumn);
                concepts.TryAdd(id, (SchemaConceptKinds.Column, tableConceptId, Humanize(parsed.GroupColumn), parsed.GroupColumn));
            }
        }

        var result = new List<SchemaAliasEntry>();
        foreach (var (conceptId, (kind, parentTable, label, originalName)) in concepts)
        {
            foreach (var (variant, score) in kind == SchemaConceptKinds.Table ? TableVariants(originalName) : ColumnVariants(originalName))
            {
                result.Add(new SchemaAliasEntry
                {
                    ConceptId = conceptId,
                    Kind = kind,
                    ParentTable = parentTable,
                    Label = label,
                    Name = variant,
                    Score = score,
                    Source = SchemaAliasSources.Seed,
                });
            }
        }
        return result;
    }

    /// <summary>(name, score) candidates for a table concept — the stored id already has this
    /// app's own "staging_" prefix (see BuiltinDashboardsGenerated's SUBJECT_TABLE/slug scheme),
    /// which a real client schema is unlikely to share, so the unprefixed form is favored.</summary>
    private static IEnumerable<(string Name, double Score)> TableVariants(string tableName)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var stripped = tableName.StartsWith("staging_", StringComparison.OrdinalIgnoreCase)
            ? tableName["staging_".Length..] : tableName;

        foreach (var (name, score) in new[]
                 {
                     (stripped, 0.55),
                     (ToPascalCase(stripped), 0.5),
                     (ToCamelCase(stripped), 0.45),
                     (tableName, 0.4), // the literal (prefixed) generated name, as a last resort
                 })
        {
            if (seen.Add(name)) yield return (name, score);
        }
    }

    private static IEnumerable<(string Name, double Score)> ColumnVariants(string columnName)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, score) in new[]
                 {
                     (columnName, 0.55),         // the literal generated name (already PascalCase-ish)
                     (ToSnakeCase(columnName), 0.5),
                     (ToCamelCase(columnName), 0.45),
                 })
        {
            if (seen.Add(name)) yield return (name, score);
        }
    }

    private static readonly Regex WordBoundary = new(@"(?<=[a-z0-9])(?=[A-Z])|[_\s]+", RegexOptions.Compiled);

    private static string[] SplitWords(string s) =>
        WordBoundary.Split(s).Where(w => w.Length > 0).ToArray();

    private static string ToSnakeCase(string s) => string.Join("_", SplitWords(s).Select(w => w.ToLowerInvariant()));

    private static string ToPascalCase(string s) =>
        string.Concat(SplitWords(s).Select(w => char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant()));

    private static string ToCamelCase(string s)
    {
        var pascal = ToPascalCase(s);
        return pascal.Length == 0 ? pascal : char.ToLowerInvariant(pascal[0]) + pascal[1..];
    }

    private static string Humanize(string s) =>
        string.Join(" ", SplitWords(s).Select(w => char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant()));
}
