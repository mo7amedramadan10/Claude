using System.Globalization;
using System.Text.Json;

namespace ChatToDashboard.Api.Interactive;

/// <summary>Shared row/SQL helpers for the §4.22 providers (saas-metrics onward) — the six
/// §4.16-4.21 providers each kept their own private copies of these same five methods before
/// this file existed; this is the same logic, pulled out once rather than re-duplicated
/// eleven more times. Every new provider in this batch uses these instead of its own copies.</summary>
internal static class InteractiveRowHelpers
{
    /// <summary>Aliases each matched real column back to its logical key (SELECT real_col AS
    /// "logicalKey", ...) so the row dictionaries QueryAsync parses are keyed by the logical
    /// names a provider's own field readers expect — without this, the query still runs and
    /// returns the right row count, but every field reads back null because the JSON keys are
    /// the real column names, not the logical ones.</summary>
    public static string BuildSelect(MatchedInteractiveTable table, params string[] logicalCols)
    {
        var pairs = logicalCols
            .Select(key => (Key: key, Real: table.Column(key)))
            .Where(p => p.Real is not null)
            .ToList();
        return pairs.Count == 0
            ? $"SELECT * FROM {table.RealTable}"
            : $"SELECT {string.Join(", ", pairs.Select(p => $"{p.Real} AS \"{p.Key}\""))} FROM {table.RealTable}";
    }

    public static string? Str(Dictionary<string, JsonElement> row, string col) =>
        row.TryGetValue(col, out var v) && v.ValueKind is JsonValueKind.String ? v.GetString()
        : row.TryGetValue(col, out v) && v.ValueKind is JsonValueKind.Number ? v.ToString()
        : null;

    public static double Num(Dictionary<string, JsonElement> row, string col)
    {
        if (!row.TryGetValue(col, out var v)) return 0;
        return v.ValueKind switch
        {
            JsonValueKind.Number => v.GetDouble(),
            JsonValueKind.String when double.TryParse(v.GetString(), out var d) => d,
            _ => 0,
        };
    }

    public static bool Bool(Dictionary<string, JsonElement> row, string col)
    {
        if (!row.TryGetValue(col, out var v)) return false;
        return v.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => v.GetDouble() != 0,
            JsonValueKind.String => v.GetString() is { } s && (s.Equals("true", StringComparison.OrdinalIgnoreCase) || s == "1"),
            _ => false,
        };
    }

    public static DateTime? ParseDate(Dictionary<string, JsonElement> row, string col)
    {
        var s = Str(row, col);
        if (string.IsNullOrWhiteSpace(s)) return null;
        return DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ? dt : null;
    }

    public static string? DateStr(Dictionary<string, JsonElement> row, string col)
    {
        var dt = ParseDate(row, col);
        return dt?.ToString("yyyy-MM-dd") ?? Str(row, col);
    }
}
