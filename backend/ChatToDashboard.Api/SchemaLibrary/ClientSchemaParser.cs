namespace ChatToDashboard.Api.SchemaLibrary;

/// <summary>
/// Parses the one flat text format every client schema description is written in — see
/// ClientSchemaDiscoveryService: <c>table1(col1 type1, col2 type2)\ntable2(...)</c>, one line per
/// table. The same shape AnalyticsTools.EnabledIntegration.SchemaDescription and
/// ExternalIntegration.ClientSchemaDescription both already carry; this just reads it back into
/// a structured per-table column list for SchemaMatchingService to compare candidate names
/// against. Deliberately NOT a single regex with a trailing <c>\)$</c> anchor — a real SQL
/// Server column type routinely nests its own parens (nvarchar(50), decimal(18,2)), which broke
/// an earlier regex-only version by making it reject the whole line (so every table with such a
/// column silently vanished from matching). Finding the table name's own boundary at the FIRST
/// '(' and the column list's end at the LAST ')' on the line sidesteps that; the column list
/// itself is still comma-split naively (a decimal(18,2) column's "2)" fragment becomes a
/// harmless, never-matching extra token — acceptable since this only ever narrows candidates
/// down to names that provably exist, never silently invents one).
/// </summary>
public static class ClientSchemaParser
{
    public readonly record struct ParsedTable(string Name, IReadOnlyList<string> Columns);

    public static IReadOnlyList<ParsedTable> Parse(string? description)
    {
        if (string.IsNullOrWhiteSpace(description)) return Array.Empty<ParsedTable>();

        var result = new List<ParsedTable>();
        foreach (var rawLine in description.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var line = rawLine.Trim();
            var openIdx = line.IndexOf('(');
            if (openIdx <= 0 || !line.EndsWith(')')) continue;

            var name = line[..openIdx].Trim();
            if (name.Length == 0) continue;

            var columns = line[(openIdx + 1)..^1]
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "")
                .Where(c => c.Length > 0)
                .ToList();
            result.Add(new ParsedTable(name, columns));
        }
        return result;
    }

    public static ParsedTable? FindTable(IReadOnlyList<ParsedTable> tables, string name) =>
        tables.Where(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase))
            .Select(t => (ParsedTable?)t)
            .FirstOrDefault();

    public static string? FindColumn(ParsedTable table, string name) =>
        table.Columns.FirstOrDefault(c => string.Equals(c, name, StringComparison.OrdinalIgnoreCase));
}
