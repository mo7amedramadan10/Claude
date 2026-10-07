using System.Text.RegularExpressions;

namespace ChatToDashboard.Api.SchemaLibrary;

/// <summary>
/// Parses the fixed, deterministic shape every draft "expected SELECT" in kpi-library.js was
/// generated in (see the scratchpad's gen_sql_hints*.py — not part of this repo, but what wrote
/// every row's 6th element): <c>SELECT [group, ]AGG(value) AS value FROM table [WHERE ...]
/// [GROUP BY group]</c>. Used twice, by two different callers, against two different kinds of
/// SQL text: <see cref="SchemaAliasStore"/>'s startup seeding parses the library's own draft SQL
/// to discover which (table, value-column, group-column) concepts exist at all; ChatController's
/// post-build reconciliation parses a widget's own <c>Query.Sql</c> — the model's real,
/// already-retargeted query against a client's actual schema — with this exact same parser to
/// learn which real name the model resolved each concept to. Never used to validate or execute
/// anything; a query this can't parse (a shape the model wrote freely, with joins/CTEs/multiple
/// aggregates) just means nothing is extracted from it, not an error.
/// </summary>
public static class SchemaConceptExtractor
{
    private static readonly Regex FromTable = new(
        @"\bFROM\s+([A-Za-z_][A-Za-z0-9_]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex AggValue = new(
        @"\b(?:AVG|SUM|COUNT|MIN|MAX)\(\s*([A-Za-z_][A-Za-z0-9_]*)\s*\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex GroupByColumn = new(
        @"\bGROUP\s+BY\s+([A-Za-z_][A-Za-z0-9_]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>One query's extracted shape — any of the three can be null when that particular
    /// piece isn't present/recognizable (e.g. no GROUP BY, or an aggregate over "*").</summary>
    public readonly record struct ParsedConcepts(string? Table, string? ValueColumn, string? GroupColumn);

    public static ParsedConcepts Parse(string? sql)
    {
        if (string.IsNullOrWhiteSpace(sql)) return new ParsedConcepts(null, null, null);

        var table = FromTable.Match(sql) is { Success: true } tm ? tm.Groups[1].Value : null;
        var value = AggValue.Match(sql) is { Success: true } vm ? vm.Groups[1].Value : null;
        var group = GroupByColumn.Match(sql) is { Success: true } gm ? gm.Groups[1].Value : null;
        return new ParsedConcepts(table, value, group);
    }

    /// <summary>Stable id for a table concept — the table name itself, lowercased so a seed row
    /// and a later learned alias for the same table never silently diverge over casing.</summary>
    public static string TableConceptId(string table) => table.Trim().ToLowerInvariant();

    /// <summary>Stable id for a column concept — scoped to its table, same reasoning as
    /// <see cref="TableConceptId"/>.</summary>
    public static string ColumnConceptId(string table, string column) =>
        $"{table.Trim().ToLowerInvariant()}.{column.Trim().ToLowerInvariant()}";
}
