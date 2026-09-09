using System.Data;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;

namespace ChatToDashboard.Api.Data;

public record LoadedTable(string Table, string SourceFile, int Rows);

public record TableColumn(string Name, string SqlType);

public record TableSchema(string Table, IReadOnlyList<TableColumn> Columns);

/// <summary>
/// Scans the configured data folder and bulk-loads each .csv/.xlsx/.json file into
/// its own table in the [staging] schema of SQL Server. Tables are dropped and
/// recreated on every load so the app can be refreshed against updated files.
/// </summary>
public class DataFolderLoader
{
    private readonly DataStore _db;
    private readonly ILogger<DataFolderLoader> _logger;
    private readonly string _dataFolderPath;

    public DataFolderLoader(DataStore db, IConfiguration configuration, ILogger<DataFolderLoader> logger)
    {
        _db = db;
        _logger = logger;
        _dataFolderPath = configuration["DataFolderPath"]
            ?? throw new InvalidOperationException("DataFolderPath is not configured.");
    }

    public string DataFolderPath => Path.GetFullPath(_dataFolderPath);

    public async Task<IReadOnlyList<LoadedTable>> LoadAllAsync(CancellationToken ct = default)
    {
        var folder = DataFolderPath;
        if (!Directory.Exists(folder))
            throw new DirectoryNotFoundException($"Data folder not found: {folder}");

        var files = Directory.EnumerateFiles(folder)
            .Where(f => new[] { ".csv", ".xlsx", ".json" }.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .OrderBy(f => f)
            .ToList();

        await _db.EnsureDatabaseExistsAsync(_logger, ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);

        var results = new List<LoadedTable>();
        foreach (var file in files)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var table = LoadFileIntoDataTable(file);
                var tableName = SanitizeTableName(Path.GetFileNameWithoutExtension(file));
                await _db.RecreateAndLoadAsync(connection, tableName, table, ct);
                var displayName = _db.DisplayTable(tableName);
                results.Add(new LoadedTable(displayName, Path.GetFileName(file), table.Rows.Count));
                _logger.LogInformation("Loaded {File} into {Table} ({Rows} rows)",
                    Path.GetFileName(file), displayName, table.Rows.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load {File}", file);
            }
        }
        return results;
    }

    public Task<IReadOnlyList<TableSchema>> GetSchemaAsync(CancellationToken ct = default) =>
        _db.GetSchemaAsync(ct);

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

        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var headerRow = range.FirstRowUsed();
        var headers = headerRow.Cells(1, range.ColumnCount())
            .Select(c => SanitizeColumnName(c.GetString(), used))
            .ToList();

        var rows = new List<string?[]>();
        foreach (var xlRow in range.RowsUsed().Skip(1))
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
