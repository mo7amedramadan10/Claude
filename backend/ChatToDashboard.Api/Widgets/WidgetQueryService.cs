using System.Globalization;
using System.Text.RegularExpressions;
using ChatToDashboard.Api.Data;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Sources;
using Dapper;

namespace ChatToDashboard.Api.Widgets;

/// <summary>
/// Executes a structured widget query (metric/aggregation/dimension/time range — no SQL,
/// chosen entirely through the "Add Widget" wizard) directly against the data tables,
/// without going through the LLM. This is what lets simple, deterministic dashboard edits
/// (add a KPI, change the period, change the chart type) skip the model entirely: natural
/// language still goes through GPT, but "click a button" never does.
///
/// Every table/column name that ends up in generated SQL is first matched against the real
/// schema (<see cref="DataFolderLoader.GetSchemaAsync"/>) and the caller's own source
/// permissions (<see cref="AnalyticsTools.DescribeSourcesAsync"/>) — the same gate
/// query_data enforces for the LLM path — so nothing the client sends is interpolated into
/// SQL verbatim; only names the schema itself reports back are used.
/// </summary>
public class WidgetQueryService
{
    private static readonly HashSet<string> NumericSqlTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "INTEGER", "REAL", "BIGINT", "INT", "SMALLINT", "TINYINT", "DECIMAL", "NUMERIC",
        "FLOAT", "MONEY", "SMALLMONEY", "DOUBLE",
    };
    private static readonly HashSet<string> DateSqlTypes = new(StringComparer.OrdinalIgnoreCase)
        { "DATETIME", "DATETIME2", "DATE", "SMALLDATETIME", "DATETIMEOFFSET" };
    private static readonly HashSet<string> Aggregations = new(StringComparer.OrdinalIgnoreCase)
        { "sum", "avg", "count", "min", "max" };
    private static readonly HashSet<string> TimeRanges = new(StringComparer.OrdinalIgnoreCase)
        { "all", "this_month", "last_month", "last_3_months", "last_6_months", "this_year", "custom" };
    // SQLite stores dates as ISO-8601 text, so the SQL type alone can't tell a date column
    // from any other text column there — this name heuristic fills that gap.
    private static readonly Regex DateNameHint = new(@"date|_at$|^at$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly DataStore _db;
    private readonly DataFolderLoader _loader;
    private readonly AnalyticsTools _tools;

    public WidgetQueryService(DataStore db, DataFolderLoader loader, AnalyticsTools tools)
    {
        _db = db;
        _loader = loader;
        _tools = tools;
    }

    public async Task<FieldsResponse> GetAvailableFieldsAsync(SourceSelection selection, CancellationToken ct)
    {
        var context = await _tools.DescribeSourcesAsync(selection, ct);
        var schema = await _loader.GetSchemaAsync(ct);

        var tables = schema
            .Where(t => !context.TableCategories.TryGetValue(t.Table, out var category)
                        || context.EnabledCategories.Contains(category, StringComparer.OrdinalIgnoreCase))
            .Where(t => !context.DisabledSystemTables.ContainsKey(t.Table))
            .Select(t => new TableFields
            {
                Table = t.Table,
                Category = context.TableCategories.TryGetValue(t.Table, out var c) ? c : null,
                System = context.TableSystems.TryGetValue(t.Table, out var s) ? s : null,
                Metrics = t.Columns.Where(c => IsNumeric(c.SqlType)).Select(c => c.Name).ToList(),
                Dimensions = t.Columns.Where(c => !IsNumeric(c.SqlType) && !IsDate(c)).Select(c => c.Name).ToList(),
                DateColumns = t.Columns.Where(IsDate).Select(c => c.Name).ToList(),
                AllColumns = t.Columns.Select(c => c.Name).ToList(),
            })
            .Where(t => t.Metrics.Count > 0 || t.AllColumns.Count > 0)
            .ToList();

        return new FieldsResponse { Tables = tables };
    }

    public async Task<WidgetQueryResult> ExecuteAsync(WidgetQueryRequest request, SourceSelection selection, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Table))
            throw new WidgetQueryValidationException("لم يتم اختيار مصدر بيانات.");

        var context = await _tools.DescribeSourcesAsync(selection, ct);
        var schema = await _loader.GetSchemaAsync(ct);

        var table = schema.FirstOrDefault(t => string.Equals(t.Table, request.Table, StringComparison.OrdinalIgnoreCase))
            ?? throw new WidgetQueryValidationException("الجدول المطلوب غير موجود أو غير متاح.");

        // Same permission gate query_data enforces for the LLM path.
        if (context.TableCategories.TryGetValue(table.Table, out var category)
            && !context.EnabledCategories.Contains(category, StringComparer.OrdinalIgnoreCase))
            throw new WidgetQueryValidationException($"لا يوجد صلاحية للوصول لتصنيف \"{category}\".");
        if (context.DisabledSystemTables.TryGetValue(table.Table, out var systemName))
            throw new WidgetQueryValidationException($"النظام \"{systemName}\" غير مفعّل حاليًا.");

        var aggregation = (request.Aggregation ?? "sum").ToLowerInvariant();
        if (!Aggregations.Contains(aggregation))
            throw new WidgetQueryValidationException("طريقة التجميع غير مدعومة.");

        var isTableWidget = string.Equals(request.ChartType, "table", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(request.Dimension) && string.IsNullOrWhiteSpace(request.TimeGranularity);

        TableColumn? metricCol = null;
        if (!isTableWidget)
        {
            metricCol = FindColumn(table, request.Metric)
                ?? throw new WidgetQueryValidationException("المؤشر المطلوب غير موجود في هذا الجدول.");
            if (!IsNumeric(metricCol.SqlType) && !string.Equals(aggregation, "count", StringComparison.OrdinalIgnoreCase))
                throw new WidgetQueryValidationException("المؤشر المختار ليس رقميًا — اختر مؤشرًا رقميًا أو التجميع \"عدد\".");
        }

        TableColumn? dimensionCol = null;
        if (!string.IsNullOrWhiteSpace(request.Dimension))
            dimensionCol = FindColumn(table, request.Dimension)
                ?? throw new WidgetQueryValidationException("حقل التصنيف المطلوب غير موجود في هذا الجدول.");

        TableColumn? dateCol = null;
        if (!string.IsNullOrWhiteSpace(request.DateColumn))
            dateCol = FindColumn(table, request.DateColumn)
                ?? throw new WidgetQueryValidationException("حقل التاريخ المطلوب غير موجود في هذا الجدول.");

        var tableRef = QualifiedTable(table.Table);

        await using var connection = await _db.OpenConnectionAsync(ct);

        if (isTableWidget)
            return await BuildTableWidgetAsync(connection, table, tableRef, request, dateCol, ct);

        var aggExpr = string.Equals(aggregation, "count", StringComparison.OrdinalIgnoreCase)
            ? "COUNT(*)"
            : $"{aggregation.ToUpperInvariant()}({Quote(metricCol!.Name)})";

        var where = "";
        if (dateCol is not null && !string.IsNullOrWhiteSpace(request.TimeRange) && request.TimeRange != "all")
        {
            if (!TimeRanges.Contains(request.TimeRange))
                throw new WidgetQueryValidationException("الفترة الزمنية غير مدعومة.");
            where = " WHERE " + BuildTimeRangeFilter(tableRef, Quote(dateCol.Name), request.TimeRange!, request.CustomFrom, request.CustomTo);
        }

        if (dateCol is not null && !string.IsNullOrWhiteSpace(request.TimeGranularity))
            return await BuildTrendWidgetAsync(connection, table, tableRef, request, dateCol, aggExpr, aggregation, metricCol!, where, ct);

        if (dimensionCol is not null)
            return await BuildComparisonWidgetAsync(connection, table, request, dimensionCol, aggExpr, aggregation, metricCol!, where, ct);

        return await BuildKpiWidgetAsync(connection, table, tableRef, request, aggExpr, aggregation, metricCol!, where, ct);
    }

    private async Task<WidgetQueryResult> BuildTableWidgetAsync(
        System.Data.Common.DbConnection connection, TableSchema table, string tableRef,
        WidgetQueryRequest request, TableColumn? dateCol, CancellationToken ct)
    {
        var cols = (request.Columns is { Count: > 0 }
                ? request.Columns.Select(c => FindColumn(table, c)?.Name
                    ?? throw new WidgetQueryValidationException($"العمود \"{c}\" غير موجود."))
                : table.Columns.Select(c => c.Name).Take(8))
            .ToList();
        if (cols.Count == 0)
            throw new WidgetQueryValidationException("لم يتم اختيار أي أعمدة لهذا الجدول.");

        var selectList = string.Join(", ", cols.Select(Quote));
        var order = dateCol is not null ? $" ORDER BY {Quote(dateCol.Name)} DESC" : "";
        var limit = Cap(request.TopN, 50);
        var sql = _db.Provider == DbProvider.Sqlite
            ? $"SELECT {selectList} FROM {tableRef}{order} LIMIT {limit}"
            : $"SELECT TOP {limit} {selectList} FROM {tableRef}{order}";

        var rows = (await connection.QueryAsync(new CommandDefinition(sql, cancellationToken: ct)))
            .Select(r => (IDictionary<string, object?>)r).ToList();

        return new WidgetQueryResult
        {
            Type = "table",
            Title = request.Title ?? table.Table,
            Data = rows,
            Source = $"من جدول {table.Table}. عرض الأعمدة المختارة كما هي بدون تجميع.",
            Query = request,
        };
    }

    private async Task<WidgetQueryResult> BuildTrendWidgetAsync(
        System.Data.Common.DbConnection connection, TableSchema table, string tableRef, WidgetQueryRequest request,
        TableColumn dateCol, string aggExpr, string aggregation, TableColumn metricCol, string where, CancellationToken ct)
    {
        var granularity = request.TimeGranularity!.ToLowerInvariant();
        var periodExpr = PeriodExpr(dateCol.Name, granularity);
        var sql = $"SELECT {periodExpr} AS period, {aggExpr} AS value FROM {tableRef}{where} " +
                  $"GROUP BY {periodExpr} ORDER BY period ASC";

        var rows = (await connection.QueryAsync(new CommandDefinition(sql, cancellationToken: ct)))
            .Select(r => (IDictionary<string, object?>)r)
            .Select(r => new Dictionary<string, object?> { ["period"] = r["period"], ["value"] = r["value"] })
            .ToList<object>();

        return new WidgetQueryResult
        {
            Type = request.ChartType is "bar" or "kpi" or "pie" or "table" ? request.ChartType : "line",
            Title = request.Title ?? metricCol.Name,
            Data = rows, XKey = "period", YKey = "value",
            Source = $"من جدول {table.Table}. تم تجميع {aggregation.ToUpperInvariant()}({metricCol.Name}) حسب {dateCol.Name} ({GranularityArabic(granularity)}).",
            Query = request,
        };
    }

    private async Task<WidgetQueryResult> BuildComparisonWidgetAsync(
        System.Data.Common.DbConnection connection, TableSchema table, WidgetQueryRequest request,
        TableColumn dimensionCol, string aggExpr, string aggregation, TableColumn metricCol, string where, CancellationToken ct)
    {
        var topN = Cap(request.TopN, 10);
        var sortDir = string.Equals(request.Sort, "asc", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";
        var tableRef = QualifiedTable(table.Table);
        var dimQuoted = Quote(dimensionCol.Name);
        var sql = _db.Provider == DbProvider.Sqlite
            ? $"SELECT {dimQuoted} AS label, {aggExpr} AS value FROM {tableRef}{where} " +
              $"GROUP BY {dimQuoted} ORDER BY value {sortDir} LIMIT {topN}"
            : $"SELECT TOP {topN} {dimQuoted} AS label, {aggExpr} AS value FROM {tableRef}{where} " +
              $"GROUP BY {dimQuoted} ORDER BY value {sortDir}";

        var rows = (await connection.QueryAsync(new CommandDefinition(sql, cancellationToken: ct)))
            .Select(r => (IDictionary<string, object?>)r)
            .Select(r => new Dictionary<string, object?> { ["label"] = r["label"], ["value"] = r["value"] })
            .ToList<object>();

        return new WidgetQueryResult
        {
            Type = request.ChartType is "line" or "kpi" or "table" ? request.ChartType : (request.ChartType ?? "bar"),
            Title = request.Title ?? metricCol.Name,
            Data = rows, XKey = "label", YKey = "value",
            Source = $"من جدول {table.Table}. تم تجميع {aggregation.ToUpperInvariant()}({metricCol.Name}) حسب {dimensionCol.Name}.",
            Query = request,
        };
    }

    private async Task<WidgetQueryResult> BuildKpiWidgetAsync(
        System.Data.Common.DbConnection connection, TableSchema table, string tableRef, WidgetQueryRequest request,
        string aggExpr, string aggregation, TableColumn metricCol, string where, CancellationToken ct)
    {
        var sql = $"SELECT {aggExpr} AS value FROM {tableRef}{where}";
        var value = await connection.ExecuteScalarAsync<double?>(new CommandDefinition(sql, cancellationToken: ct)) ?? 0;

        var data = new List<object> { new Dictionary<string, object?> { ["label"] = request.Title ?? metricCol.Name, ["value"] = value } };
        var periodNote = where.Length > 0 ? " للفترة المحددة" : "";

        return new WidgetQueryResult
        {
            Type = "kpi",
            Title = request.Title ?? metricCol.Name,
            Data = data,
            Source = $"من جدول {table.Table}. تم حساب {aggregation.ToUpperInvariant()}({metricCol.Name}){periodNote}.",
            Query = request,
        };
    }

    // ---- helpers ----

    private static bool IsNumeric(string sqlType) => NumericSqlTypes.Contains(BaseType(sqlType));
    private static bool IsDate(TableColumn c) => DateSqlTypes.Contains(BaseType(c.SqlType)) || DateNameHint.IsMatch(c.Name);
    private static string BaseType(string sqlType) => sqlType.Split('(')[0].Trim();

    private static TableColumn? FindColumn(TableSchema table, string name) =>
        table.Columns.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

    private static int Cap(int? requested, int fallback) => requested is > 0 and <= 100 ? requested.Value : fallback;

    private string Quote(string identifier) => _db.Provider == DbProvider.Sqlite
        ? $"\"{identifier.Replace("\"", "\"\"")}\""
        : $"[{identifier.Replace("]", "]]")}]";

    /// <summary>
    /// The display table name from the schema (e.g. "staging_sales" for SQLite, or the
    /// dotted "staging.Sales" for SQL Server) turned into a properly bracket/quote-qualified
    /// SQL reference — never built from client input, only from a name the schema itself returned.
    /// </summary>
    private string QualifiedTable(string displayName)
    {
        if (_db.Provider == DbProvider.Sqlite) return Quote(displayName);
        var parts = displayName.Split('.');
        return string.Join(".", parts.Select(p => $"[{p.Replace("]", "]]")}]"));
    }

    private string PeriodExpr(string dateColName, string granularity)
    {
        var col = Quote(dateColName);
        if (_db.Provider == DbProvider.Sqlite)
            return granularity switch
            {
                "day" => $"date({col})",
                "week" => $"strftime('%Y-W%W', {col})",
                _ => $"strftime('%Y-%m', {col})",
            };
        return granularity switch
        {
            "day" => $"FORMAT({col}, 'yyyy-MM-dd')",
            "week" => $"FORMAT({col}, 'yyyy') + '-W' + RIGHT('0' + CAST(DATEPART(ISO_WEEK, {col}) AS VARCHAR(2)), 2)",
            _ => $"FORMAT({col}, 'yyyy-MM')",
        };
    }

    private static string GranularityArabic(string granularity) => granularity switch
    {
        "day" => "يوميًا", "week" => "أسبوعيًا", _ => "شهريًا",
    };

    /// <summary>
    /// Anchors every relative range ("this month", "last 3 months"...) to the latest date
    /// actually present in the column — not real-world "today" — matching the same time
    /// semantics the chat agent's system prompt uses, so the two paths never disagree about
    /// what "this month" means for data that stopped updating a while ago.
    /// </summary>
    private string BuildTimeRangeFilter(string tableRef, string col, string range, string? customFrom, string? customTo)
    {
        var sqlite = _db.Provider == DbProvider.Sqlite;
        return range switch
        {
            "this_month" => sqlite
                ? $"strftime('%Y-%m', {col}) = (SELECT strftime('%Y-%m', MAX({col})) FROM {tableRef})"
                : $"FORMAT({col}, 'yyyy-MM') = (SELECT FORMAT(MAX({col}), 'yyyy-MM') FROM {tableRef})",
            "last_month" => sqlite
                ? $"strftime('%Y-%m', {col}) = (SELECT strftime('%Y-%m', date(MAX({col}), '-1 month')) FROM {tableRef})"
                : $"FORMAT({col}, 'yyyy-MM') = (SELECT FORMAT(DATEADD(month, -1, MAX({col})), 'yyyy-MM') FROM {tableRef})",
            "last_3_months" => sqlite
                ? $"{col} >= (SELECT date(MAX({col}), '-3 months') FROM {tableRef})"
                : $"{col} >= (SELECT DATEADD(month, -3, MAX({col})) FROM {tableRef})",
            "last_6_months" => sqlite
                ? $"{col} >= (SELECT date(MAX({col}), '-6 months') FROM {tableRef})"
                : $"{col} >= (SELECT DATEADD(month, -6, MAX({col})) FROM {tableRef})",
            "this_year" => sqlite
                ? $"strftime('%Y', {col}) = (SELECT strftime('%Y', MAX({col})) FROM {tableRef})"
                : $"YEAR({col}) = (SELECT YEAR(MAX({col})) FROM {tableRef})",
            "custom" => BuildCustomRangeFilter(col, customFrom, customTo),
            _ => throw new WidgetQueryValidationException("الفترة الزمنية غير مدعومة."),
        };
    }

    private static string BuildCustomRangeFilter(string col, string? customFrom, string? customTo)
    {
        var from = ParseDateLiteral(customFrom) ?? throw new WidgetQueryValidationException("تاريخ البداية غير صحيح.");
        var to = ParseDateLiteral(customTo) ?? throw new WidgetQueryValidationException("تاريخ النهاية غير صحيح.");
        return $"{col} BETWEEN '{from}' AND '{to} 23:59:59'";
    }

    // Re-parsed and re-formatted from a validated DateTime — never the raw client string —
    // before it is embedded as a SQL literal.
    private static string? ParseDateLiteral(string? raw) =>
        !string.IsNullOrWhiteSpace(raw) &&
        DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
            ? d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : null;
}
