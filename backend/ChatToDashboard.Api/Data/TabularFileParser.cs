using System.Data;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;

namespace ChatToDashboard.Api.Data;

public record TableColumn(string Name, string SqlType);

public record TableSchema(string Table, IReadOnlyList<TableColumn> Columns);

/// <summary>
/// قراءة ملفات CSV/XLSX/JSON إلى DataTable مع استنتاج أنواع الأعمدة وتنقية الأسماء — دوال ساكنة مشتركة
/// بين رفع الملفات (UploadParser) والأنظمة الخارجية (SystemApiLoader). آلية «مجلد البيانات» التي كانت
/// تحمّل ملفات مجلد data في المخطط المشترك عند كل إقلاع أُزيلت بقرار المستخدم (2026-11-02).
/// </summary>
public static class TabularFileParser
{
    public static string SanitizeTableName(string name)
    {
        var sanitized = Regex.Replace(name, @"[^\w]", "_");
        if (sanitized.Length == 0 || char.IsDigit(sanitized[0]))
            sanitized = "t_" + sanitized;
        return sanitized.Length > 100 ? sanitized[..100] : sanitized;
    }

    private static string SanitizeColumnName(string name, HashSet<string> used)
    {
        var sanitized = Regex.Replace(name.Trim(), @"[^\w ]", "_").Trim();
        if (sanitized.Length == 0) sanitized = "Column";
        var candidate = sanitized;
        var i = 2;
        while (!used.Add(candidate)) candidate = $"{sanitized}_{i++}";
        return candidate;
    }

    // ---- File readers: everything is read as strings first, then column types are inferred. ----

    public static DataTable LoadFileIntoDataTable(string file) =>
        Path.GetExtension(file).ToLowerInvariant() switch
        {
            ".csv" => InferTypes(ReadCsv(file)),
            ".xlsx" => InferTypes(ReadXlsx(file)),
            ".json" => InferTypes(ReadJson(file)),
            _ => throw new NotSupportedException($"Unsupported file type: {file}"),
        };

    public static (List<string> Headers, List<string?[]> Rows) ReadCsv(string file)
    {
        using var reader = new StreamReader(file);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            BadDataFound = null,
            MissingFieldFound = null,
        });

        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        csv.Read();
        csv.ReadHeader();
        var headers = (csv.HeaderRecord ?? Array.Empty<string>())
            .Select(h => SanitizeColumnName(h, used))
            .ToList();

        var rows = new List<string?[]>();
        while (csv.Read())
        {
            var row = new string?[headers.Count];
            for (var i = 0; i < headers.Count; i++)
                row[i] = csv.TryGetField<string>(i, out var value) ? value : null;
            rows.Add(row);
        }
        return (headers, rows);
    }

    public static (List<string> Headers, List<string?[]> Rows) ReadXlsx(string file)
    {
        using var workbook = new XLWorkbook(file);
        var sheet = workbook.Worksheets.First();
        var range = sheet.RangeUsed();
        if (range is null) return (new List<string>(), new List<string?[]>());

        var headerRow = FindHeaderRow(range);
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var headers = headerRow.Cells(1, range.ColumnCount())
            .Select(c => SanitizeColumnName(c.GetString(), used))
            .ToList();

        var rows = new List<string?[]>();
        foreach (var xlRow in range.RowsUsed().SkipWhile(r => r.RowNumber() <= headerRow.RowNumber()))
        {
            var row = new string?[headers.Count];
            for (var i = 0; i < headers.Count; i++)
            {
                var cell = xlRow.Cell(i + 1);
                row[i] = cell.IsEmpty() ? null
                    : cell.DataType == XLDataType.DateTime
                        ? cell.GetDateTime().ToString("o", CultureInfo.InvariantCulture)
                        : cell.GetString();
            }
            rows.Add(row);
        }
        return (headers, rows);
    }

    /// <summary>Real-world exports routinely have one or more title/banner rows above the
    /// actual column headers — a merged report title, a company name, a generated-on
    /// timestamp — each spanning only a cell or two of an otherwise-blank row. Unconditionally
    /// treating the file's first used row as the header row turns that banner into useless
    /// generic column names ("Column", "Column_2", ...) and pushes the real headers down to
    /// become the first "data" row instead (seen live: a real header row like "Internal ID,
    /// Etiad ID, Opp. Name, ..." ended up rendered as ordinary row data under fallback
    /// "Column_N" headers). Scans a handful of leading used rows and picks the first one that
    /// looks like an actual header: most of its cells within the sheet's used column span are
    /// filled in. Falls back to the very first used row — today's unconditional behavior — for
    /// any file where nothing further down looks better, rather than guessing indefinitely.</summary>
    private static IXLRangeRow FindHeaderRow(IXLRange range)
    {
        const int MaxRowsToScan = 5;
        const double MinFilledFraction = 0.5;

        var candidates = range.RowsUsed().Take(MaxRowsToScan).ToList();
        if (candidates.Count == 0) return range.FirstRowUsed();

        var columnCount = range.ColumnCount();
        foreach (var row in candidates)
        {
            var filled = row.Cells(1, columnCount).Count(c => !c.IsEmpty());
            if (filled >= columnCount * MinFilledFraction) return row;
        }
        return candidates[0];
    }

    private static (List<string> Headers, List<string?[]> Rows) ReadJson(string file)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(file));
        if (doc.RootElement.ValueKind != JsonValueKind.Array)
            throw new InvalidDataException($"{Path.GetFileName(file)}: expected a top-level JSON array of objects.");

        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var headers = new List<string>();
        var headerIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var rawRows = new List<Dictionary<string, string?>>();

        foreach (var element in doc.RootElement.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.Object) continue;
            var row = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in element.EnumerateObject())
            {
                if (!headerIndex.ContainsKey(prop.Name))
                {
                    var sanitized = SanitizeColumnName(prop.Name, used);
                    headerIndex[prop.Name] = headers.Count;
                    headers.Add(sanitized);
                }
                row[prop.Name] = prop.Value.ValueKind switch
                {
                    JsonValueKind.Null or JsonValueKind.Undefined => null,
                    JsonValueKind.String => prop.Value.GetString(),
                    _ => prop.Value.GetRawText(),
                };
            }
            rawRows.Add(row);
        }

        var rows = rawRows.Select(raw =>
        {
            var row = new string?[headers.Count];
            foreach (var (key, value) in raw)
                if (headerIndex.TryGetValue(key, out var i)) row[i] = value;
            return row;
        }).ToList();

        return (headers, rows);
    }

    // ---- Type inference: BIGINT / DECIMAL / DATETIME2 / BIT when every non-empty value parses, else NVARCHAR. ----

    public static DataTable InferTypes((List<string> Headers, List<string?[]> Rows) data)
    {
        var (headers, rows) = data;
        var table = new DataTable();

        var columnTypes = new Type[headers.Count];
        for (var i = 0; i < headers.Count; i++)
        {
            var values = rows.Select(r => i < r.Length ? r[i] : null)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToList();
            columnTypes[i] = InferColumnType(values!);
            table.Columns.Add(headers[i], columnTypes[i]);
        }

        foreach (var row in rows)
        {
            var items = new object?[headers.Count];
            for (var i = 0; i < headers.Count; i++)
            {
                var raw = i < row.Length ? row[i] : null;
                items[i] = ConvertValue(raw, columnTypes[i]);
            }
            table.Rows.Add(items.Select(v => v ?? DBNull.Value).ToArray());
        }
        return table;
    }

    private static Type InferColumnType(List<string> nonEmptyValues)
    {
        if (nonEmptyValues.Count == 0) return typeof(string);
        if (nonEmptyValues.All(v => long.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out _)))
            return typeof(long);
        if (nonEmptyValues.All(v => decimal.TryParse(v, NumberStyles.Number, CultureInfo.InvariantCulture, out _)))
            return typeof(decimal);
        if (nonEmptyValues.All(v => DateTime.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _)))
            return typeof(DateTime);
        if (nonEmptyValues.All(v => bool.TryParse(v, out _)))
            return typeof(bool);
        return typeof(string);
    }

    private static object? ConvertValue(string? raw, Type type)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        try
        {
            if (type == typeof(long)) return long.Parse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (type == typeof(decimal)) return decimal.Parse(raw, NumberStyles.Number, CultureInfo.InvariantCulture);
            if (type == typeof(DateTime)) return DateTime.Parse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            if (type == typeof(bool)) return bool.Parse(raw);
        }
        catch (FormatException)
        {
            return null;
        }
        return raw;
    }

}
