using System.Globalization;
using System.Text.RegularExpressions;

namespace ChatToDashboard.Api.Analyst;

/// <summary>
/// Fills an answer_template's {{key}}/{{key|fmt}} tokens from the result's own rows — see spec
/// section 6. The model never sees row values, only this engine's key vocabulary (rows,
/// sum/avg/min/max/top1-5/bottom1-3/share_top3 per column), so every number in a composed
/// message traces back to a real cell, never the model's own arithmetic or invention.
///
/// Phase 1 scope note: this is the substitution engine only. The spec's own Phase 3
/// ("القالب والأرقام") additionally calls for a GUARD — scanning the model's answer_template for
/// any stray digit outside a recognized token and retrying/failing the composition if one slips
/// through — plus the regex needs to handle Arabic-Indic digits in the model's own output. That
/// guard is deliberately NOT implemented here yet (see the Phase 1 report); this engine renders
/// correctly today but a model that free-types a number next to a placeholder would currently go
/// unflagged. Built now anyway, not as a throwaway, specifically so Phase 3 only has to add the
/// guard on top of this rather than build substitution from scratch too.
/// </summary>
public static class AnalystPlaceholders
{
    private static readonly Regex Token = new(@"\{\{\s*([A-Za-z0-9_.]+)(?:\|([A-Za-z]))?\s*\}\}", RegexOptions.Compiled);
    private static readonly Regex TopPattern = new(@"^top([1-5])$", RegexOptions.Compiled);
    private static readonly Regex BottomPattern = new(@"^bottom([1-3])$", RegexOptions.Compiled);

    public record RenderResult(string Text, IReadOnlyList<string> MissingKeys);

    /// <summary>Renders every token it can; a key with no matching data (an unknown column, a
    /// topN beyond the row count) is left as the literal "missing" list — the caller (the
    /// compose step) decides what to do with that (Phase 1: log and keep the literal token in
    /// place rather than silently drop it, so a broken template is visibly broken, not silently
    /// wrong — Phase 3 is where this becomes a real retry-then-fail per spec section 6).</summary>
    public static RenderResult Render(
        string template, IReadOnlyList<string> columns, IReadOnlyList<Dictionary<string, object?>> rows, string? primaryMeasure)
    {
        var sortedDesc = SortByMeasure(rows, columns, primaryMeasure, descending: true);
        var sortedAsc = SortByMeasure(rows, columns, primaryMeasure, descending: false);
        var missing = new List<string>();

        var text = Token.Replace(template, m =>
        {
            var key = m.Groups[1].Value;
            var format = m.Groups[2].Success ? m.Groups[2].Value : null;
            var (found, value) = Resolve(key, columns, rows, sortedDesc, sortedAsc);
            if (!found)
            {
                missing.Add(key);
                return m.Value;
            }
            return FormatValue(value, format);
        });

        return new RenderResult(text, missing);
    }

    private static List<Dictionary<string, object?>> SortByMeasure(
        IReadOnlyList<Dictionary<string, object?>> rows, IReadOnlyList<string> columns, string? primaryMeasure, bool descending)
    {
        if (primaryMeasure is null || !columns.Contains(primaryMeasure)) return rows.ToList();
        var ordered = rows.OrderBy(r => ToDouble(r.GetValueOrDefault(primaryMeasure)));
        return (descending ? ordered.Reverse() : ordered).ToList();
    }

    private static (bool Found, object? Value) Resolve(
        string key, IReadOnlyList<string> columns, IReadOnlyList<Dictionary<string, object?>> rows,
        List<Dictionary<string, object?>> sortedDesc, List<Dictionary<string, object?>> sortedAsc)
    {
        if (key == "rows") return (true, rows.Count);

        var dot = key.IndexOf('.');
        if (dot <= 0 || dot == key.Length - 1) return (false, null);
        var agg = key[..dot];
        var col = key[(dot + 1)..];
        if (!columns.Contains(col)) return (false, null);

        switch (agg)
        {
            case "sum": return (true, rows.Sum(r => ToDouble(r.GetValueOrDefault(col))));
            case "avg": return (true, rows.Count > 0 ? rows.Average(r => ToDouble(r.GetValueOrDefault(col))) : 0d);
            case "min": return (true, rows.Count > 0 ? rows.Min(r => ToDouble(r.GetValueOrDefault(col))) : 0d);
            case "max": return (true, rows.Count > 0 ? rows.Max(r => ToDouble(r.GetValueOrDefault(col))) : 0d);
            case "share_top3":
            {
                var total = rows.Sum(r => ToDouble(r.GetValueOrDefault(col)));
                var top3 = sortedDesc.Take(3).Sum(r => ToDouble(r.GetValueOrDefault(col)));
                return (true, total != 0 ? top3 / total : 0d);
            }
        }

        var topMatch = TopPattern.Match(agg);
        if (topMatch.Success)
        {
            var index = int.Parse(topMatch.Groups[1].Value, CultureInfo.InvariantCulture) - 1;
            return index < sortedDesc.Count ? (true, sortedDesc[index].GetValueOrDefault(col)) : (false, null);
        }

        var bottomMatch = BottomPattern.Match(agg);
        if (bottomMatch.Success)
        {
            var index = int.Parse(bottomMatch.Groups[1].Value, CultureInfo.InvariantCulture) - 1;
            return index < sortedAsc.Count ? (true, sortedAsc[index].GetValueOrDefault(col)) : (false, null);
        }

        return (false, null);
    }

    private static double ToDouble(object? value) => value switch
    {
        null => 0d,
        double d => d,
        float f => f,
        decimal dec => (double)dec,
        int i => i,
        long l => l,
        string s when double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) => parsed,
        _ => 0d,
    };

    /// <summary>|n (thousands-separated), |p (percentage, value already 0-1), |c (compact K/M),
    /// |d (date), no format = the value's own natural string form.</summary>
    private static string FormatValue(object? value, string? format)
    {
        if (format is null) return value switch
        {
            null => "",
            double d => d.ToString("0.##", CultureInfo.InvariantCulture),
            DateTime dt => dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            _ => value.ToString() ?? "",
        };

        var num = value is DateTime dateValue ? (double?)null : TryToDouble(value);
        return format switch
        {
            "n" when num is { } n1 => n1.ToString("#,##0.##", CultureInfo.InvariantCulture),
            "p" when num is { } n2 => (n2 * 100).ToString("0.#", CultureInfo.InvariantCulture) + "%",
            "c" when num is { } n3 => CompactNumber(n3),
            "d" => value switch
            {
                DateTime dt => dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                string s when DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) =>
                    parsed.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                _ => value?.ToString() ?? "",
            },
            _ => value?.ToString() ?? "",
        };
    }

    private static double? TryToDouble(object? value) => value switch
    {
        null => null,
        double d => d,
        float f => f,
        decimal dec => (double)dec,
        int i => i,
        long l => l,
        string s when double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) => parsed,
        _ => null,
    };

    private static string CompactNumber(double n)
    {
        var abs = Math.Abs(n);
        if (abs >= 1_000_000_000) return (n / 1_000_000_000).ToString("0.#", CultureInfo.InvariantCulture) + "مليار";
        if (abs >= 1_000_000) return (n / 1_000_000).ToString("0.#", CultureInfo.InvariantCulture) + "مليون";
        if (abs >= 1_000) return (n / 1_000).ToString("0.#", CultureInfo.InvariantCulture) + "ألف";
        return n.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
