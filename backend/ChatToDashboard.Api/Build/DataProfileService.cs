using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ChatToDashboard.Api.Data;
using ChatToDashboard.Api.Llm;
using Dapper;

namespace ChatToDashboard.Api.Build;

/// <summary>عمود واحد في ملف تعريف البيانات: نوعه الدلالي، ومجاله، وأكثر قيمه شيوعًا إن كان فئويًا.</summary>
public sealed record ProfiledColumn(
    string Name,
    string SqlType,
    /// <summary>"number" أو "date" أو "text".</summary>
    string Kind,
    /// <summary>القيم المميزة مع تكرارها عندما يكون العمود فئويًا (≤ 20 قيمة)؛ وإلا null.</summary>
    IReadOnlyList<(string Value, long Count)>? TopValues,
    /// <summary>عدد القيم المميزة التقريبي عندما يتجاوز حد التعداد (للأعمدة عالية التنوع).</summary>
    bool HighCardinality,
    string? Min,
    string? Max,
    long NullCount);

public sealed record ProfiledTable(
    string Table,
    string? SourceName,
    long RowCount,
    IReadOnlyList<ProfiledColumn> Columns,
    /// <summary>حتى ٣ صفوف عينة بقيم مقصوصة، لفهم المعنى الفعلي لكل عمود.</summary>
    IReadOnlyList<IReadOnlyDictionary<string, string>> SampleRows);

public sealed record DataProfile(IReadOnlyList<ProfiledTable> Tables)
{
    public ProfiledTable? Find(string table) =>
        Tables.FirstOrDefault(t => string.Equals(t.Table, table, StringComparison.OrdinalIgnoreCase));

    /// <summary>النص الذي يُرسل للموديل: مكثّف ومرتّب، لا JSON، بحد أقصى للطول حتى لا يبتلع السياق.</summary>
    public string ToPromptText(int maxChars = 14000, IReadOnlyDictionary<(string Table, string Column), string>? semantics = null)
    {
        var sb = new StringBuilder();
        foreach (var t in Tables)
        {
            sb.Append("## الجدول ").Append(t.Table);
            if (!string.IsNullOrWhiteSpace(t.SourceName)) sb.Append(" (المصدر: ").Append(t.SourceName).Append(')');
            sb.Append(" — ").Append(t.RowCount.ToString(CultureInfo.InvariantCulture)).AppendLine(" صف");
            foreach (var c in t.Columns)
            {
                sb.Append("- ").Append(c.Name).Append(" [").Append(c.Kind).Append(']');
                // طبقة المعنى (Semantics/): المفهوم المؤكَّد أو المرجّح لهذا العمود وخريطة قيمه.
                if (semantics is not null && semantics.TryGetValue((t.Table, c.Name), out var meaning))
                    sb.Append(" المعنى: ").Append(meaning).Append(" —");
                if (c.Kind == "text")
                {
                    if (c.TopValues is { Count: > 0 })
                    {
                        sb.Append(c.HighCardinality ? " أمثلة: " : " القيم: ");
                        sb.Append(string.Join("، ", c.TopValues.Select(v =>
                            c.HighCardinality ? Trim(v.Value, 30) : $"{Trim(v.Value, 30)} ({v.Count})")));
                    }
                }
                else if (c.Min is not null || c.Max is not null)
                {
                    sb.Append(" المدى: ").Append(c.Min ?? "؟").Append(" → ").Append(c.Max ?? "؟");
                }
                if (c.NullCount > 0) sb.Append(" (فارغ في ").Append(c.NullCount).Append(" صف)");
                sb.AppendLine();
            }
            if (t.SampleRows.Count > 0)
            {
                sb.AppendLine("عينة:");
                foreach (var row in t.SampleRows)
                    sb.Append("  {").Append(string.Join(", ", row.Select(kv => $"{kv.Key}: {Trim(kv.Value, 40)}"))).AppendLine("}");
            }
            sb.AppendLine();
            if (sb.Length > maxChars)
            {
                sb.AppendLine("… (اختُصر ملف التعريف لطوله؛ استخدم query_data للاستكشاف عند الحاجة)");
                break;
            }
        }
        return sb.ToString().TrimEnd();
    }

    private static string Trim(string s, int max) => s.Length <= max ? s : s[..max] + "…";
}

/// <summary>
/// يبني «ملف تعريف البيانات» لكل الجداول المرئية للطلب (نفس قائمة list_files) بلا موديل: عدد
/// الصفوف، والأعمدة بنوعها الدلالي ومجالها وقيمها الشائعة، وعينة صفوف. يُرسل مرة واحدة في
/// بداية كل نداء مرحلي فيغني الموديل عن الاستعلامات الاستكشافية (ما الأعمدة؟ ما قيم الحالة؟)
/// ويمنع تخمين قيم شروط WHERE. يُخزَّن مؤقتًا لدقائق لكل مشروع؛ مفتاح التخزين يتضمن بصمة
/// الجداول والأعمدة فيبطل تلقائيًا عند رفع ملف جديد أو تغيّر بنيته.
/// </summary>
public class DataProfileService
{
    private const int MaxTables = 40;
    private const int MaxColumnsPerTable = 40;
    private const int MaxDistinctValues = 20;
    private const int SampleRowCount = 3;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);
    private static readonly Regex IsoDate = new(@"^\d{4}-\d{2}-\d{2}", RegexOptions.Compiled);

    private readonly DataStore _db;
    private readonly AnalyticsTools _tools;
    private readonly ILogger<DataProfileService> _logger;
    private readonly ConcurrentDictionary<string, (DateTime At, DataProfile Profile)> _cache = new();

    public DataProfileService(DataStore db, AnalyticsTools tools, ILogger<DataProfileService> logger)
    {
        _db = db;
        _tools = tools;
        _logger = logger;
    }

    public async Task<DataProfile> BuildAsync(AnalyticsTools.SourceContext context, CancellationToken ct = default)
    {
        var tables = (await _tools.GetVisibleTablesAsync(context, ct)).Take(MaxTables).ToList();
        var key = (context.ProjectId ?? "") + "|" + string.Join("|", tables.Select(t => t.Table + ":" + t.Columns.Count));
        if (_cache.TryGetValue(key, out var hit) && DateTime.UtcNow - hit.At < CacheTtl) return hit.Profile;

        var profiled = new List<ProfiledTable>();
        foreach (var t in tables)
        {
            try { profiled.Add(await ProfileTableAsync(t, context, ct)); }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // جدول تعذّر فحصه (قفل، أو اسم غريب) لا يُسقط الملف كله — يُدرج ببنيته فقط.
                _logger.LogWarning(ex, "تعذّر بناء ملف تعريف للجدول {Table}", t.Table);
                profiled.Add(new ProfiledTable(t.Table, SourceOf(t.Table, context), -1,
                    t.Columns.Select(c => new ProfiledColumn(c.Name, c.SqlType, KindFromSqlType(c.SqlType), null, false, null, null, 0)).ToList(),
                    Array.Empty<IReadOnlyDictionary<string, string>>()));
            }
        }
        var profile = new DataProfile(profiled);
        _cache[key] = (DateTime.UtcNow, profile);
        return profile;
    }

    private static string? SourceOf(string table, AnalyticsTools.SourceContext? context) =>
        context is null ? null
        : context.TableFiles.TryGetValue(table, out var f) ? f
        : context.TableSystems.TryGetValue(table, out var s) ? s : null;

    /// <summary>ملف تعريف جدول واحد بالاسم (لتحليل المعاني بعد الرفع) — null إن لم يوجد. بلا تخزين مؤقت.</summary>
    public async Task<ProfiledTable?> ProfileOneAsync(string tableName, string? projectId, CancellationToken ct = default)
    {
        var schema = (await _tools.GetSchemaAsync(projectId, ct))
            .FirstOrDefault(t => string.Equals(t.Table, tableName, StringComparison.OrdinalIgnoreCase));
        return schema is null ? null : await ProfileTableAsync(schema, null, ct);
    }

    private async Task<ProfiledTable> ProfileTableAsync(TableSchema schema, AnalyticsTools.SourceContext? context, CancellationToken ct)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        var table = _db.QualifiedTable(schema.Table);
        var rowCount = await connection.ExecuteScalarAsync<long>(new CommandDefinition($"SELECT COUNT(*) FROM {table}", commandTimeout: 20, cancellationToken: ct));

        var columns = new List<ProfiledColumn>();
        foreach (var c in schema.Columns.Take(MaxColumnsPerTable))
        {
            var col = Quote(c.Name);
            var kind = KindFromSqlType(c.SqlType);
            long nulls = 0;
            string? min = null, max = null;
            List<(string, long)>? top = null;
            var high = false;
            try
            {
                nulls = await connection.ExecuteScalarAsync<long>(new CommandDefinition(
                    $"SELECT COUNT(*) FROM {table} WHERE {col} IS NULL OR {col} = ''", commandTimeout: 20, cancellationToken: ct));
                var rows = (await connection.QueryAsync(new CommandDefinition(
                    TopDistinctSql(table, col), commandTimeout: 20, cancellationToken: ct))).ToList();
                var values = rows.Select(r => ((object?)r.v, (long)Convert.ToInt64((object)r.n))).Where(x => x.Item1 is not null)
                    .Select(x => (Value: Convert.ToString(x.Item1, CultureInfo.InvariantCulture) ?? "", x.Item2)).ToList();
                // الأعمدة النصية في SQLite قد تحمل تواريخ أو أرقامًا: نستنتج النوع من القيم نفسها.
                if (kind == "text" && values.Count > 0)
                {
                    if (values.All(v => IsoDate.IsMatch(v.Value))) kind = "date";
                    else if (values.All(v => double.TryParse(v.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out _))) kind = "number";
                }
                if (kind == "text")
                {
                    high = values.Count > MaxDistinctValues;
                    top = (high ? values.Take(3) : values).ToList();
                }
                else
                {
                    var mm = await connection.QuerySingleAsync(new CommandDefinition(
                        $"SELECT MIN({col}) AS mn, MAX({col}) AS mx FROM {table} WHERE {col} IS NOT NULL AND {col} <> ''",
                        commandTimeout: 20, cancellationToken: ct));
                    min = Convert.ToString((object?)mm.mn, CultureInfo.InvariantCulture);
                    max = Convert.ToString((object?)mm.mx, CultureInfo.InvariantCulture);
                    if (kind == "number")
                    {
                        if (double.TryParse(min, NumberStyles.Any, CultureInfo.InvariantCulture, out var dmin)) min = dmin.ToString("0.##", CultureInfo.InvariantCulture);
                        if (double.TryParse(max, NumberStyles.Any, CultureInfo.InvariantCulture, out var dmax)) max = dmax.ToString("0.##", CultureInfo.InvariantCulture);
                    }
                    else { min = min?[..Math.Min(10, min.Length)]; max = max?[..Math.Min(10, max.Length)]; }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogDebug(ex, "تعذّر فحص العمود {Column} في {Table}", c.Name, schema.Table);
            }
            columns.Add(new ProfiledColumn(c.Name, c.SqlType, kind, top, high, min, max, nulls));
        }

        var samples = new List<IReadOnlyDictionary<string, string>>();
        try
        {
            var sampleRows = await connection.QueryAsync(new CommandDefinition(SampleSql(table), commandTimeout: 20, cancellationToken: ct));
            foreach (IDictionary<string, object?> row in sampleRows)
                samples.Add(row.Take(MaxColumnsPerTable).ToDictionary(
                    kv => kv.Key, kv => Convert.ToString(kv.Value, CultureInfo.InvariantCulture) ?? ""));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogDebug(ex, "تعذّر جلب عينة من {Table}", schema.Table);
        }

        return new ProfiledTable(schema.Table, SourceOf(schema.Table, context), rowCount, columns, samples);
    }

    private string TopDistinctSql(string table, string col) => _db.Provider == DbProvider.Sqlite
        ? $"SELECT {col} AS v, COUNT(*) AS n FROM {table} WHERE {col} IS NOT NULL AND {col} <> '' GROUP BY {col} ORDER BY n DESC LIMIT {MaxDistinctValues + 1}"
        : $"SELECT TOP {MaxDistinctValues + 1} {col} AS v, COUNT(*) AS n FROM {table} WHERE {col} IS NOT NULL AND CAST({col} AS NVARCHAR(MAX)) <> '' GROUP BY {col} ORDER BY n DESC";

    private string SampleSql(string table) => _db.Provider == DbProvider.Sqlite
        ? $"SELECT * FROM {table} LIMIT {SampleRowCount}"
        : $"SELECT TOP {SampleRowCount} * FROM {table}";

    private string Quote(string identifier) => _db.Provider == DbProvider.Sqlite
        ? $"\"{identifier.Replace("\"", "\"\"")}\""
        : $"[{identifier.Replace("]", "]]")}]";

    internal static string KindFromSqlType(string sqlType)
    {
        var t = sqlType.ToUpperInvariant();
        if (t.Contains("INT") || t.Contains("DECIMAL") || t.Contains("NUMERIC") || t.Contains("FLOAT") || t.Contains("REAL")
            || t.Contains("MONEY") || t.Contains("DOUBLE") || t.Contains("BIT")) return "number";
        if (t.Contains("DATE") || t.Contains("TIME")) return "date";
        return "text";
    }
}
