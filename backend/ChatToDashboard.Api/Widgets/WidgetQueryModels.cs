using System.Text.Json.Serialization;
using ChatToDashboard.Api.Sources;

namespace ChatToDashboard.Api.Widgets;

/// <summary>Metrics/dimensions/date columns available for one table, for the "Add Widget" wizard.</summary>
public class TableFields
{
    [JsonPropertyName("table")] public string Table { get; set; } = "";
    [JsonPropertyName("category")] public string? Category { get; set; }
    [JsonPropertyName("system")] public string? System { get; set; }
    [JsonPropertyName("metrics")] public List<string> Metrics { get; set; } = new();
    [JsonPropertyName("dimensions")] public List<string> Dimensions { get; set; } = new();
    [JsonPropertyName("dateColumns")] public List<string> DateColumns { get; set; } = new();
    [JsonPropertyName("allColumns")] public List<string> AllColumns { get; set; } = new();
}

public class FieldsResponse
{
    [JsonPropertyName("tables")] public List<TableFields> Tables { get; set; } = new();
}

/// <summary>Body of POST /api/widgets/fields.</summary>
public class WidgetFieldsRequest
{
    [JsonPropertyName("sources")] public SourceSelection? Sources { get; set; }
}

/// <summary>
/// A structured widget query, chosen entirely through the "Add Widget" wizard UI (no SQL,
/// no chart-type jargon required of the user) — this is what lets the app answer "change
/// the period" or "change the chart type" deterministically, without calling the LLM.
/// </summary>
public class WidgetQueryRequest
{
    [JsonPropertyName("table")] public string Table { get; set; } = "";
    [JsonPropertyName("metric")] public string Metric { get; set; } = "";
    [JsonPropertyName("aggregation")] public string Aggregation { get; set; } = "sum"; // sum|avg|count|min|max
    [JsonPropertyName("dimension")] public string? Dimension { get; set; }
    [JsonPropertyName("dateColumn")] public string? DateColumn { get; set; }
    [JsonPropertyName("timeRange")] public string? TimeRange { get; set; } // all|this_month|last_month|last_3_months|last_6_months|this_year|custom
    [JsonPropertyName("customFrom")] public string? CustomFrom { get; set; }
    [JsonPropertyName("customTo")] public string? CustomTo { get; set; }
    [JsonPropertyName("timeGranularity")] public string? TimeGranularity { get; set; } // day|week|month
    [JsonPropertyName("topN")] public int? TopN { get; set; }
    [JsonPropertyName("sort")] public string? Sort { get; set; } // asc|desc
    [JsonPropertyName("chartType")] public string? ChartType { get; set; } // kpi|bar|line|pie|table hint
    [JsonPropertyName("title")] public string? Title { get; set; }
    [JsonPropertyName("columns")] public List<string>? Columns { get; set; } // table widgets: explicit column pick

    /// <summary>
    /// Dashboard-level filters currently applied to this widget's table (equality/IN on a
    /// real, schema-verified column — see <see cref="Widgets.WidgetQueryService"/>).
    /// </summary>
    [JsonPropertyName("filters")] public List<FilterCondition>? Filters { get; set; }
}

/// <summary>One active dashboard filter's effect on a query: field IN (values...).</summary>
public class FilterCondition
{
    [JsonPropertyName("field")] public string Field { get; set; } = "";
    [JsonPropertyName("values")] public List<string> Values { get; set; } = new();
}

/// <summary>Body of POST /api/widgets/filter-values.</summary>
public class FilterValuesRequest
{
    [JsonPropertyName("sources")] public SourceSelection? Sources { get; set; }
    [JsonPropertyName("table")] public string Table { get; set; } = "";
    [JsonPropertyName("field")] public string Field { get; set; } = "";
}

public class FilterValuesResponse
{
    [JsonPropertyName("values")] public List<string> Values { get; set; } = new();
}

/// <summary>Body of POST /api/widgets/query.</summary>
public class WidgetQueryEnvelope
{
    [JsonPropertyName("sources")] public SourceSelection? Sources { get; set; }
    [JsonPropertyName("query")] public WidgetQueryRequest Query { get; set; } = new();
}

/// <summary>
/// A ready-to-render widget (same shape as the chat-generated ones) plus the structured
/// query that produced it, so the editor can re-run it later with different parameters.
/// </summary>
public class WidgetQueryResult
{
    [JsonPropertyName("type")] public string Type { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("data")] public object Data { get; set; } = new List<object>();
    [JsonPropertyName("xKey")] public string? XKey { get; set; }
    [JsonPropertyName("yKey")] public string? YKey { get; set; }
    [JsonPropertyName("source")] public string Source { get; set; } = "";
    [JsonPropertyName("query")] public WidgetQueryRequest Query { get; set; } = new();
}

/// <summary>A validation failure the user caused (bad table/column/range) — reported as 400, not 500.</summary>
public class WidgetQueryValidationException : Exception
{
    public WidgetQueryValidationException(string message) : base(message) { }
}
