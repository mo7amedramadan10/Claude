using System.Globalization;
using System.Text.RegularExpressions;
using ChatToDashboard.Api.Data;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Sources;
using Dapper;

namespace ChatToDashboard.Api.Widgets;

/// <summary>
/// Executes a structured widget query (metric/aggregation/dimension/time range/filters —
/// no SQL, chosen entirely through the "Add Widget" wizard or a dashboard filter) directly
/// against the data tables, without going through the LLM. This is what lets simple,
/// deterministic dashboard edits (add a KPI, change the period, apply a filter, change the
/// chart type) skip the model entirely: natural language still goes through GPT, but
/// "click a button" never does.
///
/// Every table/column name that ends up in generated SQL is first matched against the real
/// schema (<see cref="DataFolderLoader.GetSchemaAsync"/>) and the caller's own source
/// permissions (<see cref="AnalyticsTools.DescribeSourcesAsync"/>) — the same gate
/// query_data enforces for the LLM path — so nothing the client sends is interpolated into
/// SQL verbatim; only names the schema itself reports back are used. Filter *values* (which
/// are much less constrained than identifiers) are always passed as real ADO.NET parameters,
/// never string-interpolated.
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

    /// <summary>
    /// Distinct, non-null values of one real column — the only legitimate source for a
    /// filter's option list (never invented; see the system prompt's "الفلاتر" section).
    /// Capped at 50 values: a column with more distinct values than that is not a usable
    /// filter dimension anyway.
    /// </summary>
    public async Task<FilterValuesResponse> GetFilterValuesAsync(
        string tableName, string field, SourceSelection selection, CancellationToken ct)
    {
        var table = await ResolveTableAsync(tableName, selection, ct);
        var col = FindColumn(table, field)
            ?? throw new WidgetQueryValidationException("العمود المطلوب غير موجود في هذا الجدول.");

        var tableRef = QualifiedTable(table.Table);
        var colQuoted = Quote(col.Name);
        var limit = 50;
        var sql = _db.Provider == DbProvider.Sqlite
            ? $"SELECT DISTINCT {colQuoted} AS v FROM {tableRef} WHERE {colQuoted} IS NOT NULL ORDER BY v LIMIT {limit}"
            : $"SELECT DISTINCT TOP {limit} {colQuoted} AS v FROM {tableRef} WHERE {colQuoted} IS NOT NULL ORDER BY v";

        await using var connection = await _db.OpenConnectionAsync(ct);
        var values = (await connection.QueryAsync<string>(new CommandDefinition(sql, cancellationToken: ct)))
            .Where(v => !string.IsNullOrEmpty(v))
            .ToList();

        return new FilterValuesResponse { Values = values };
    }

    public async Task<WidgetQueryResult> ExecuteAsync(WidgetQueryRequest request, SourceSelection selection, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Table))
            throw new WidgetQueryValidationException("لم يتم اختيار مصدر بيانات.");

        var table = await ResolveTableAsync(request.Table, selection, ct);

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

        // Every filter value becomes a real parameter — never string-interpolated — even
        // though the field name itself is schema-verified like any other identifier here.
        var parameters = new DynamicParameters();
        var clauses = new List<string>();
        var filterNote = "";
        if (dateCol is not null && !string.IsNullOrWhiteSpace(request.TimeRange) && request.TimeRange != "all")
        {
            if (!TimeRanges.Contains(request.TimeRange))
                throw new WidgetQueryValidationException("الفترة الزمنية غير مدعومة.");
            clauses.Add(BuildTimeRangeFilter(tableRef, Quote(dateCol.Name), request.TimeRange!, request.CustomFrom, request.CustomTo));
        }
        if (request.Filters is { Count: > 0 })
        {
            var (filterClauses, note) = BuildFilterClauses(table, request.Filters, parameters);
            clauses.AddRange(filterClauses);
            filterNote = note;
        }
        var where = clauses.Count > 0 ? " WHERE " + string.Join(" AND ", clauses) : "";

        await using var connection = await _db.OpenConnectionAsync(ct);

        if (isTableWidget)
            return await BuildTableWidgetAsync(connection, table, tableRef, request, dateCol, where, parameters, filterNote, ct);

        var aggExpr = string.Equals(aggregation, "count", StringComparison.OrdinalIgnoreCase)
            ? "COUNT(*)"
            : $"{aggregation.ToUpperInvariant()}({Quote(metricCol!.Name)})";

        if (dateCol is not null && !string.IsNullOrWhiteSpace(request.TimeGranularity))
            return await BuildTrendWidgetAsync(connection, table, tableRef, request, dateCol, aggExpr, aggregation, metricCol!, where, parameters, filterNote, ct);

        if (dimensionCol is not null)
            return await BuildComparisonWidgetAsync(connection, table, request, dimensionCol, aggExpr, aggregation, metricCol!, where, parameters, filterNote, ct);

        return await BuildKpiWidgetAsync(connection, table, tableRef, request, aggExpr, aggregation, metricCol!, where, parameters, filterNote, ct);
    }

    /// <summary>
    /// Re-runs a chat-authored widget's own stored SQL (see DashboardWidget.Query) with the
    /// active dashboard filter(s) added as an extra WHERE condition — the counterpart to
    /// <see cref="ExecuteAsync"/> for widgets the model built directly with SQL rather than
    /// through the wizard. Splicing text into an already-written SELECT is inherently more
    /// fragile than building one from scratch (it can't handle every shape a model might
    /// write — e.g. a filtered column that only exists inside a subquery's own scope — and a
    /// widget it can't handle safely just stays visibly "غير متأثر بالفلتر" rather than
    /// silently returning a wrong result), but it covers the common single-SELECT,
    /// no-nested-subquery shape the system prompt's own SQL rules already push the model
    /// toward, without a fresh model round-trip on every filter click.
    ///
    /// Security note: unlike every other method here, "sql" itself comes from the client —
    /// it can only live there, since the model returns it once and the frontend holds it from
    /// then on. Three checks keep this from being an open SQL-execution endpoint: (1)
    /// ValidateReadOnlySql rejects anything but a single read-only SELECT/WITH statement
    /// (checked both before and after splicing), (2) CheckSourcePermission — the exact scan
    /// query_data applies to every LLM-issued query — rejects any reference to a disabled
    /// system or category table, and (3) the filter condition itself goes through
    /// BuildFilterClauses, which only accepts a schema-verified column name and always
    /// parameterizes the values, never string-interpolates them.
    /// </summary>
    public async Task<SqlFilterResult> ExecuteSqlFilterAsync(
        string tableName, string sql, List<FilterCondition>? filters, SourceSelection selection, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new WidgetQueryValidationException("لم يتم تحديد الجدول.");
        if (string.IsNullOrWhiteSpace(sql) || AnalyticsTools.ValidateReadOnlySql(sql) is not null)
            throw new WidgetQueryValidationException("الاستعلام المخزّن لهذا العنصر غير صالح.");

        var table = await ResolveTableAsync(tableName, selection, ct);

        var context = await _tools.DescribeSourcesAsync(selection, ct);
        if (AnalyticsTools.CheckSourcePermission(sql, context) is not null)
            throw new WidgetQueryValidationException("لا يوجد صلاحية لتنفيذ هذا الاستعلام.");

        var parameters = new DynamicParameters();
        var (clauses, _) = BuildFilterClauses(table, filters ?? new List<FilterCondition>(), parameters);
        var filteredSql = clauses.Count > 0 ? SpliceWhereClause(sql, string.Join(" AND ", clauses)) : sql;

        if (AnalyticsTools.ValidateReadOnlySql(filteredSql) is not null)
            throw new WidgetQueryValidationException("تعذّر تطبيق الفلتر على استعلام هذا العنصر.");

        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = (await connection.QueryAsync(
                new CommandDefinition(filteredSql, parameters, commandTimeout: 30, cancellationToken: ct)))
            .Take(AnalyticsTools.MaxRowsReturned)
            .Select(r => (IDictionary<string, object?>)r)
            .ToList<object>();

        return new SqlFilterResult { Data = rows };
    }

    /// <summary>
    /// Inserts <paramref name="condition"/> into <paramref name="sql"/> as an additional WHERE
    /// condition, respecting the statement's real structure — a keyword inside parentheses or
    /// a string literal (a subquery, a CTE, a quoted value) is never mistaken for a clause
    /// boundary. Combines with an existing WHERE via AND; adds a new WHERE if none exists.
    /// </summary>
    private static string SpliceWhereClause(string sql, string condition)
    {
        var (whereIndex, nextClauseIndex) = FindTopLevelClauses(sql);
        var insertAt = nextClauseIndex ?? sql.TrimEnd(';', ' ', '\t', '\r', '\n').Length;
        var prefix = sql[..insertAt].TrimEnd();
        var suffix = sql[insertAt..];
        return whereIndex is not null
            ? $"{prefix} AND ({condition}) {suffix}"
            : $"{prefix} WHERE {condition} {suffix}";
    }

    private static readonly Regex TopLevelClauseKeyword =
        new(@"\b(WHERE|GROUP\s+BY|ORDER\s+BY|LIMIT)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// The position of the statement's own WHERE (if any) and of the first clause keyword
    /// after it — GROUP BY/ORDER BY/LIMIT, or the same WHERE position if none of those exist
    /// — skipping any match that falls inside parentheses or a string literal, so a
    /// subquery's or CTE's own clauses are never mistaken for the main statement's.
    /// </summary>
    private static (int? WhereIndex, int? NextClauseIndex) FindTopLevelClauses(string sql)
    {
        int? whereIndex = null, nextIndex = null;
        foreach (Match m in TopLevelClauseKeyword.Matches(sql))
        {
            if (ParenDepthAt(sql, m.Index) != 0) continue;
            var keyword = Regex.Replace(m.Value, @"\s+", " ").Trim().ToUpperInvariant();
            if (keyword == "WHERE")
            {
                whereIndex ??= m.Index;
                continue;
            }
            if (nextIndex is null) { nextIndex = m.Index; break; }
        }
        return (whereIndex, nextIndex);
    }

    /// <summary>Parenthesis depth just before <paramref name="index"/>, ignoring parens inside quoted strings.</summary>
    private static int ParenDepthAt(string sql, int index)
    {
        var depth = 0;
        var inSingleQuote = false;
        var inDoubleQuote = false;
        for (var i = 0; i < index; i++)
        {
            var c = sql[i];
            if (inSingleQuote) { if (c == '\'') inSingleQuote = false; continue; }
            if (inDoubleQuote) { if (c == '"') inDoubleQuote = false; continue; }
            switch (c)
            {
                case '\'': inSingleQuote = true; break;
                case '"': inDoubleQuote = true; break;
                case '(': depth++; break;
                case ')': depth--; break;
            }
        }
        return depth;
    }

    private async Task<TableSchema> ResolveTableAsync(string tableName, SourceSelection selection, CancellationToken ct)
    {
        var context = await _tools.DescribeSourcesAsync(selection, ct);
        var schema = await _loader.GetSchemaAsync(ct);

        var table = schema.FirstOrDefault(t => string.Equals(t.Table, tableName, StringComparison.OrdinalIgnoreCase))
            ?? throw new WidgetQueryValidationException("الجدول المطلوب غير موجود أو غير متاح.");

        // Same permission gate query_data enforces for the LLM path.
        if (context.TableCategories.TryGetValue(table.Table, out var category)
            && !context.EnabledCategories.Contains(category, StringComparer.OrdinalIgnoreCase))
            throw new WidgetQueryValidationException($"لا يوجد صلاحية للوصول لتصنيف \"{category}\".");
        if (context.DisabledSystemTables.TryGetValue(table.Table, out var systemName))
            throw new WidgetQueryValidationException($"النظام \"{systemName}\" غير مفعّل حاليًا.");

        return table;
    }

    /// <summary>
    /// Builds "<paramref name="table"/>.field IN (@p0, @p1...)" clauses for every filter
    /// whose field actually belongs to this widget's table; a filter for an unrelated table
    /// is silently skipped here rather than rejected — see the frontend's "غير متأثر
    /// بالفلتر" badge for widgets a dashboard filter cannot reach.
    /// </summary>
    private (List<string> Clauses, string Note) BuildFilterClauses(
        TableSchema table, List<FilterCondition> filters, DynamicParameters parameters)
    {
        var clauses = new List<string>();
        var notes = new List<string>();
        var p = 0;
        foreach (var f in filters)
        {
            if (string.IsNullOrWhiteSpace(f.Field) || f.Values is not { Count: > 0 }) continue;
            var col = FindColumn(table, f.Field);
            if (col is null) continue; // field belongs to a different table — not applicable here

            var names = f.Values.Select(_ => $"@wf{p++}").ToList();
            for (var i = 0; i < f.Values.Count; i++)
                parameters.Add(names[i], f.Values[i]);
            clauses.Add(f.Values.Count == 1
                ? $"{Quote(col.Name)} = {names[0]}"
                : $"{Quote(col.Name)} IN ({string.Join(", ", names)})");
            notes.Add($"{col.Name} = {string.Join("/", f.Values)}");
        }
        return (clauses, notes.Count > 0 ? " مع تطبيق فلتر: " + string.Join("، ", notes) : "");
    }

    private async Task<WidgetQueryResult> BuildTableWidgetAsync(
        System.Data.Common.DbConnection connection, TableSchema table, string tableRef,
        WidgetQueryRequest request, TableColumn? dateCol, string where, DynamicParameters parameters,
        string filterNote, CancellationToken ct)
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
            ? $"SELECT {selectList} FROM {tableRef}{where}{order} LIMIT {limit}"
            : $"SELECT TOP {limit} {selectList} FROM {tableRef}{where}{order}";

        var rows = (await connection.QueryAsync(new CommandDefinition(sql, parameters, cancellationToken: ct)))
            .Select(r => (IDictionary<string, object?>)r).ToList();

        return new WidgetQueryResult
        {
            Type = "table",
            Title = request.Title ?? table.Table,
            Data = rows,
            Source = $"من جدول {table.Table}. عرض الأعمدة المختارة كما هي بدون تجميع{filterNote}.",
            Query = request,
        };
    }

    private async Task<WidgetQueryResult> BuildTrendWidgetAsync(
        System.Data.Common.DbConnection connection, TableSchema table, string tableRef, WidgetQueryRequest request,
        TableColumn dateCol, string aggExpr, string aggregation, TableColumn metricCol, string where,
        DynamicParameters parameters, string filterNote, CancellationToken ct)
    {
        var granularity = request.TimeGranularity!.ToLowerInvariant();
        var periodExpr = PeriodExpr(dateCol.Name, granularity);
        var sql = $"SELECT {periodExpr} AS period, {aggExpr} AS value FROM {tableRef}{where} " +
                  $"GROUP BY {periodExpr} ORDER BY period ASC";

        var rows = (await connection.QueryAsync(new CommandDefinition(sql, parameters, cancellationToken: ct)))
            .Select(r => (IDictionary<string, object?>)r)
            .Select(r => new Dictionary<string, object?> { ["period"] = r["period"], ["value"] = r["value"] })
            .ToList<object>();

        return new WidgetQueryResult
        {
            Type = request.ChartType is "bar" or "kpi" or "pie" or "table" ? request.ChartType : "line",
            Title = request.Title ?? metricCol.Name,
            Data = rows, XKey = "period", YKey = "value",
            Source = $"من جدول {table.Table}. تم تجميع {aggregation.ToUpperInvariant()}({metricCol.Name}) حسب {dateCol.Name} ({GranularityArabic(granularity)}){filterNote}.",
            Query = request,
        };
    }

    private async Task<WidgetQueryResult> BuildComparisonWidgetAsync(
        System.Data.Common.DbConnection connection, TableSchema table, WidgetQueryRequest request,
        TableColumn dimensionCol, string aggExpr, string aggregation, TableColumn metricCol, string where,
        DynamicParameters parameters, string filterNote, CancellationToken ct)
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

        var rows = (await connection.QueryAsync(new CommandDefinition(sql, parameters, cancellationToken: ct)))
            .Select(r => (IDictionary<string, object?>)r)
            .Select(r => new Dictionary<string, object?> { ["label"] = r["label"], ["value"] = r["value"] })
            .ToList<object>();

        return new WidgetQueryResult
        {
            Type = request.ChartType is "line" or "kpi" or "table" ? request.ChartType : (request.ChartType ?? "bar"),
            Title = request.Title ?? metricCol.Name,
            Data = rows, XKey = "label", YKey = "value",
            Source = $"من جدول {table.Table}. تم تجميع {aggregation.ToUpperInvariant()}({metricCol.Name}) حسب {dimensionCol.Name}{filterNote}.",
            Query = request,
        };
    }

    private async Task<WidgetQueryResult> BuildKpiWidgetAsync(
        System.Data.Common.DbConnection connection, TableSchema table, string tableRef, WidgetQueryRequest request,
        string aggExpr, string aggregation, TableColumn metricCol, string where, DynamicParameters parameters,
        string filterNote, CancellationToken ct)
    {
        var sql = $"SELECT {aggExpr} AS value FROM {tableRef}{where}";
        var value = await connection.ExecuteScalarAsync<double?>(new CommandDefinition(sql, parameters, cancellationToken: ct)) ?? 0;

        var data = new List<object> { new Dictionary<string, object?> { ["label"] = request.Title ?? metricCol.Name, ["value"] = value } };
        var periodNote = where.Length > 0 && filterNote.Length == 0 ? " للفترة المحددة" : "";

        return new WidgetQueryResult
        {
            Type = "kpi",
            Title = request.Title ?? metricCol.Name,
            Data = data,
            Source = $"من جدول {table.Table}. تم حساب {aggregation.ToUpperInvariant()}({metricCol.Name}){periodNote}{filterNote}.",
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
