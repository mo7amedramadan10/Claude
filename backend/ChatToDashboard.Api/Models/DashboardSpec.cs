using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatToDashboard.Api.Models;

/// <summary>
/// The structured dashboard JSON Claude must return. Deserialized and validated
/// before anything reaches the frontend.
/// </summary>
public class DashboardSpec
{
    private static readonly HashSet<string> AllowedWidgetTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "kpi", "bar", "line", "pie", "table",
            // See BuildSystemPrompt's progress-table/trend-matrix/status-bar/radial-gauge/
            // linear-gauge sections — each has its own "data" shape, handled specially below
            // in Validate().
            "progress-table", "trend-matrix", "status-bar", "radial-gauge", "linear-gauge",
            // INTERACTIVE-HANDOFF.md §4.14/§5 additions. "goal" is object-shaped like
            // status-bar/radial-gauge (see ObjectShapedTypes below); the rest are row-array
            // shaped like every other chart type, one array entry per axis/segment/person/
            // event/stage — see each type's XML doc below for its exact row shape.
            "radar", "semi", "dco", "iconrank", "ribbon", "people", "feed", "goal",
            "stages", "bullet",
        };

    /// <summary>Types whose <see cref="DashboardWidget.Data"/> is a single JSON object
    /// (one computed total/summary), not a row array — mirrors the existing status-bar/
    /// radial-gauge special case in <see cref="Validate"/>.</summary>
    private static readonly HashSet<string> ObjectShapedTypes =
        new(StringComparer.OrdinalIgnoreCase) { "status-bar", "radial-gauge", "goal" };
    private static readonly HashSet<string> AllowedFilterTypes =
        new(StringComparer.OrdinalIgnoreCase) { "single_select", "multi_select", "date_range", "numeric_range" };

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// A rich, engaging narrative walkthrough of the dashboard — distinct in purpose from
    /// <see cref="Summary"/> (short, compact) and from each widget's <see
    /// cref="DashboardWidget.Source"/> (technical provenance, never mentioned here). Written in
    /// Modern Standard Arabic, meant to be read or listened to (see the 🔊 button, which reads
    /// this — never Summary or Source). See BuildSystemPrompt's "narration" section for the
    /// full content rules.
    /// </summary>
    [JsonPropertyName("narration")]
    public string Narration { get; set; } = string.Empty;

    [JsonPropertyName("widgets")]
    public List<DashboardWidget> Widgets { get; set; } = new();

    /// <summary>
    /// Optional dashboard-level filters the agent identified as useful, each backed by
    /// values it actually queried (never invented). Absent/empty is valid — not every
    /// dashboard needs one, and older saved dashboards have no such field at all.
    /// </summary>
    [JsonPropertyName("filters")]
    public List<DashboardFilter> Filters { get; set; } = new();

    /// <summary>مفتاح القالب الذي بُنيت منه اللوحة (عند البناء من قالب فقط) — يتيح إعادة بناء
    /// عنصر واحد لاحقًا من «تصحيح الأعمدة» (WidgetColumnsController.Rebuild).</summary>
    [JsonPropertyName("templateKey")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TemplateKey { get; set; }

    /// <summary>
    /// Drops any filter entry that fails its own structural requirements (missing field/
    /// table, an unrecognized type, or a select type with no real queried options) instead
    /// of letting it fail the whole response. Filters are explicitly optional/best-effort —
    /// the comment on <see cref="Filters"/> already says an absent/empty array is valid — so
    /// one malformed entry (observed live: a smaller model repeatedly re-sending a
    /// multi_select filter with an empty "options" after an empty-results query, instead of
    /// either re-querying for the real distinct values or dropping the filter per its own
    /// instructions) shouldn't burn every JSON-repair retry and fail an otherwise perfectly
    /// valid, real-data dashboard. Call once right after deserializing, before <see
    /// cref="Validate"/>, so nothing downstream ever sees a broken filter.
    /// </summary>
    public void SanitizeFilters()
    {
        if (Filters is null) { Filters = new(); return; }
        Filters = Filters.Where(f =>
            f is not null
            && !string.IsNullOrWhiteSpace(f.Field)
            && !string.IsNullOrWhiteSpace(f.Table)
            && !string.IsNullOrWhiteSpace(f.Type) && AllowedFilterTypes.Contains(f.Type)
            && (!NeedsOptions(f.Type) || f.Options is { Count: > 0 })
        ).ToList();
    }

    private static bool NeedsOptions(string type) =>
        string.Equals(type, "single_select", StringComparison.OrdinalIgnoreCase)
        || string.Equals(type, "multi_select", StringComparison.OrdinalIgnoreCase);

    /// <summary>Returns a list of validation problems; empty means the spec is valid. Filters
    /// are intentionally not checked here — see <see cref="SanitizeFilters"/>, which should
    /// run first and leaves only already-valid ones behind.</summary>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(Summary))
            errors.Add("\"summary\" is required and must be a non-empty string.");
        if (string.IsNullOrWhiteSpace(Narration))
            errors.Add("\"narration\" is required and must be a non-empty string.");
        // An empty array is valid: it is how the agent answers when the question needs a
        // source the user has switched off, and the summary explains what to enable.
        if (Widgets is null)
            errors.Add("\"widgets\" must be an array.");
        else
        {
            for (var i = 0; i < Widgets.Count; i++)
            {
                var w = Widgets[i];
                if (w is null) { errors.Add($"widgets[{i}] is null."); continue; }
                if (string.IsNullOrWhiteSpace(w.Type) || !AllowedWidgetTypes.Contains(w.Type))
                    errors.Add($"widgets[{i}].type must be one of: kpi, bar, line, pie, table, progress-table, " +
                               "trend-matrix, status-bar, radial-gauge, linear-gauge, radar, semi, dco, " +
                               $"iconrank, ribbon, people, feed, goal, stages, bullet (got \"{w.Type}\").");
                if (string.IsNullOrWhiteSpace(w.Title))
                    errors.Add($"widgets[{i}].title is required.");
                // A noData placeholder (see DashboardWidget.NoData) is a declared template
                // widget the model couldn't build from any available source — it carries no
                // real data or provenance, so neither is required; only type/title (checked
                // above) matter, so the frontend can still render it as the right kind of
                // empty-state card and the user can later fill it in.
                if (!w.NoData)
                {
                    // status-bar and radial-gauge's data are each a single object, not an array
                    // (see BuildSystemPrompt) — one total+breakdown, one single gauge value.
                    // A widget carrying "comparison" instead puts its real data inside left/right
                    // and is told to leave the top-level "data" empty or absent entirely.
                    var hasComparison = w.Comparison is { ValueKind: JsonValueKind.Object };
                    if (!hasComparison)
                    {
                        var expectsObject = ObjectShapedTypes.Contains(w.Type);
                        var expectedKind = expectsObject ? JsonValueKind.Object : JsonValueKind.Array;
                        if (w.Data.ValueKind != expectedKind)
                            errors.Add(expectedKind == JsonValueKind.Object
                                ? $"widgets[{i}].data must be a single JSON object for type \"{w.Type}\"."
                                : $"widgets[{i}].data must be a JSON array.");
                    }
                    if (string.IsNullOrWhiteSpace(w.Source))
                        errors.Add($"widgets[{i}].source is required: two sentences — where the data came " +
                                   "from, then how it was calculated.");
                }
                if (w.Forecast is not null)
                {
                    var f = w.Forecast;
                    var n = f.Labels?.Count ?? 0;
                    if (n == 0 || f.Values?.Count != n || f.Lower?.Count != n || f.Upper?.Count != n)
                        errors.Add($"widgets[{i}].forecast: labels/values/lower/upper must be non-empty " +
                                   "arrays of the same length.");
                }
            }
        }

        return errors;
    }
}

public class DashboardFilter
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    /// <summary>The real column this filter narrows — validated against the schema before use.</summary>
    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;

    /// <summary>The real table <see cref="Field"/> belongs to — which widgets it can affect.</summary>
    [JsonPropertyName("table")]
    public string? Table { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = "single_select"; // single_select | multi_select | date_range | numeric_range

    /// <summary>Selectable values — must come from an actual DISTINCT query, never invented.</summary>
    [JsonPropertyName("options")]
    public List<FilterOption> Options { get; set; } = new();

    [JsonPropertyName("appliesTo")]
    public string AppliesTo { get; set; } = "dashboard";
}

public class FilterOption
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

public class DashboardWidget
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The reference design's own tab/page name for a multi-page template dashboard (e.g.
    /// "المحفظة"/"الميزانية") — absent for a single-page one. Never set from the model's own
    /// response; ChatController.Post stamps it by matching this widget's Title against the
    /// template's declared BuiltinDashboardWidgetSpec list, so the frontend's tab bar
    /// (renderDashboard's getDashboardPages/buildDashTabs) can rely on it even though nothing
    /// requires the model to understand or echo the concept.
    /// </summary>
    [JsonPropertyName("page")]
    public string? Page { get; set; }

    [JsonPropertyName("data")]
    public JsonElement Data { get; set; }

    [JsonPropertyName("xKey")]
    public string? XKey { get; set; }

    [JsonPropertyName("yKey")]
    public string? YKey { get; set; }

    /// <summary>
    /// Provenance shown behind the widget's ⓘ button: exactly two sentences — where the
    /// data came from, then how it was calculated.
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /// <summary>
    /// Data-quality problems in the user's own source file that the model worked around to
    /// still build this widget — e.g. a row whose value in some column couldn't convert to a
    /// number, so it was excluded from an aggregate via TRY_CAST/TRY_CONVERT (see
    /// DataStore.DialectPrompt's SQL Server rule) instead of failing the whole query. Each
    /// entry is one short, specific Arabic sentence naming the real row (by whatever natural
    /// identifier column exists — never a raw internal row number) and the actual invalid
    /// value, plus what was done about it, so the user can go fix that row in their own file.
    /// Always from a real verification query the model actually ran (e.g. a DISTINCT/WHERE
    /// check for which rows a TRY_CAST nulled out) — never guessed. Null/empty is the normal
    /// case (nothing needed working around); the frontend shows a distinct ⚠ marker only when
    /// this is non-empty, separate from the ⓘ source popover.
    /// </summary>
    [JsonPropertyName("dataIssues")]
    public List<string>? DataIssues { get; set; }

    /// <summary>
    /// A statistically-computed forecast extending this widget's real historical <see
    /// cref="Data"/> — set by the model only via the forecast_data tool result (never invented),
    /// or attached client-side by the "🔮 توقّع الأشهر الجاية" button. Absent for an ordinary
    /// widget. The frontend renders this as a visually distinct dashed/shaded continuation, never
    /// blended into <see cref="Data"/> as if it were an observed value.
    /// </summary>
    [JsonPropertyName("forecast")]
    public WidgetForecast? Forecast { get; set; }

    /// <summary>
    /// The exact table and read-only SELECT that produced <see cref="Data"/>, so a dashboard
    /// filter can re-run this widget's own query with an extra condition spliced in — see
    /// WidgetQueryService.ExecuteSqlFilterAsync — without a fresh model call. Absent when the
    /// widget has no single query to point to (e.g. built from list_files metadata, or a
    /// forecast overlay); such a widget is simply not filterable, and the frontend marks it
    /// "غير متأثر بالفلتر" exactly as it already does for one with no query lineage at all.
    /// </summary>
    [JsonPropertyName("query")]
    public WidgetSqlQuery? Query { get; set; }

    /// <summary>
    /// "تخطيط المقارنة" (see BuildSystemPrompt) — a display option any widget type can carry
    /// instead of a direct <see cref="Data"/> array: { "left": {...}, "right": {...} }, each
    /// side shaped like whatever <see cref="Type"/> normally needs. Kept as a raw JsonElement
    /// (like <see cref="Data"/> itself) rather than a typed model, since its shape varies with
    /// Type; the frontend (buildWidget/ComparisonCard) is what actually interprets it.
    /// </summary>
    [JsonPropertyName("comparison")]
    public JsonElement? Comparison { get; set; }

    /// <summary>
    /// True for a declared template widget (see BuiltinDashboardTemplate/
    /// BuiltinDashboardWidgetSpec) the model couldn't build from any source currently
    /// enabled — no real <see cref="Data"/>/<see cref="Source"/>, just the original intended
    /// Type/Title so the dashboard still shows the template's full, familiar shape instead of
    /// silently dropping items. The frontend renders this as an explicit empty-state card
    /// (never faked data) with an action to pick a source for it — either the structured
    /// "Add Widget" wizard, or a follow-up chat question naming an alternate source — either
    /// of which replaces this placeholder with a normal, real widget in place. Set either by
    /// the model itself (instructed in TemplatePromptService's required-widgets block) or by
    /// ChatController's own reconciliation pass when the model dropped a required widget
    /// outright instead of marking it — see ChatController.Post.
    /// </summary>
    [JsonPropertyName("noData")]
    public bool NoData { get; set; }

    /// <summary>Short, honest Arabic reason shown on a <see cref="NoData"/> card — e.g. which
    /// source would be needed. Never a technical error message; same tone as a normal
    /// widget's refusal-to-fabricate explanation elsewhere in the system prompt.</summary>
    [JsonPropertyName("missingReason")]
    public string? MissingReason { get; set; }

    /// <summary>
    /// Per-type rendering tweaks from INTERACTIVE-HANDOFF.md §5 — a small, type-specific JSON
    /// object the frontend reads selectively (unknown keys are ignored), kept as a raw
    /// passthrough bag like <see cref="Comparison"/> rather than one typed field per option
    /// since the set varies by <see cref="Type"/>:
    ///   kpi:      { "tg": number (target %), "tgv": number (target value), "risk": number }
    ///   bar:      { "spot": "max" | "last" | number } — highlight one bar, rest gray
    ///   line:     { "dash": [seriesIndex,...] } — render those series dashed (reference/target)
    ///   bullet:   { "hi": bool (higher is better), "risk": number, "inv": bool }
    ///   semi/dco: { "tl": string (total's own label) }
    ///   radar:    { "max": number }
    ///   iconrank: { "av": [string,...] (short symbol per row), "nopct": bool, "u": string }
    ///   ribbon/people/feed: { "u": string (unit), "lab": {status: label} (feed only) }
    /// Every field above is optional; absent means the frontend's own default.
    /// </summary>
    [JsonPropertyName("options")]
    public JsonElement? Options { get; set; }

    /// <summary>توسعات bar/line/pie التي يطلبها BuildSystemPrompt من الموديل وتقرؤها الواجهة
    /// (buildMultiSeriesChart وتسمية مركز الدائرة): عدة سلاسل على نفس الرسم، وتكديسها،
    /// وتعبئة المساحة تحت الخطوط، ونص مركز الدائرة. كانت تضيع عند إعادة التسلسل لأنها لم تكن
    /// خصائص هنا — الآن تُحفظ كما هي (null = غائبة، لا تُكتب في JSON).</summary>
    [JsonPropertyName("series")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<WidgetSeries>? Series { get; set; }

    [JsonPropertyName("stacked")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Stacked { get; set; }

    [JsonPropertyName("areaFill")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? AreaFill { get; set; }

    [JsonPropertyName("centerLabel")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CenterLabel { get; set; }
}

/// <summary>سلسلة واحدة في رسم متعدد السلاسل: مفتاح الحقل الرقمي في كل صف من data، وتسميته.</summary>
public class WidgetSeries
{
    [JsonPropertyName("key")] public string Key { get; set; } = string.Empty;
    [JsonPropertyName("label")] public string? Label { get; set; }
}

/// <summary>
/// Row shapes expected in <see cref="DashboardWidget.Data"/> for each §4.14/§5 addition
/// (array-shaped unless noted). Documented here rather than enforced strictly in <see
/// cref="DashboardSpec.Validate"/>, matching how existing chart types (bar/line/pie) are
/// only checked for "is this an array of objects" — the exact column names are the model's
/// own responsibility, driven by TemplatePromptService's per-type prompt text.
///   radar:    [{ "axis": string, "&lt;seriesName&gt;": number, ... }] — one row per axis,
///             one numeric column per series (≤3 series).
///   semi/dco: [{ "label": string, "value": number }] — 2-3 rows (semi) or 3-5 (dco).
///   iconrank: [{ "label": string, "value": number }] — ranked descending by the frontend.
///   ribbon:   [{ "period": string, "category": string, "value": number }] — long format;
///             the frontend pivots period×category itself.
///   people:   [{ "name": string, "value": number, "goalPct": number?, "role": string? }]
///   feed:     [{ "title": string, "desc": string?, "value": string?, "status":
///             "ok"|"pend"|"bad"|"info", "time": string?, "owner": string? }]
///   goal:     single object (see DashboardSpec.ObjectShapedTypes): { "percent": number,
///             "remaining": number?, "who": string?, "days": number?, "headline": string? }
///   stages:   [{ "stage": string, "label": string, "count": number, "tone":
///             "info"|"ok"|"warn"|"bad"|"dark"|"mute", "value": number?, "row":
///             "main"|"sub"|"total"? }] — §4.24 long format, one row per tile, grouped by
///             stage in first-seen order and read right-to-left on screen. The older shape
///             (one row per stage: { "stage", "status": "ok"|"warn"|"bad"|"pending",
///             "subStatus"?, "tone"? }, no "count") still renders as before.
///   bullet:   [{ "label": string, "actual": number, "target": number }]
/// </summary>
internal static class InteractiveWidgetDataShapes { }

/// <summary>See <see cref="DashboardWidget.Query"/>.</summary>
public class WidgetSqlQuery
{
    [JsonPropertyName("table")]
    public string Table { get; set; } = string.Empty;

    [JsonPropertyName("sql")]
    public string Sql { get; set; } = string.Empty;

    /// <summary>Set only when this query was built via query_client_data (self-reported by the
    /// model, same as Table/Sql above) — the id of the ExternalIntegration whose real database
    /// schema this SQL is already written against. Absent for a widget built on this app's own
    /// internal sources. See WidgetQueryService (filter re-runs) and PublishService (publish
    /// skips retargeting for a widget published to this same integration).</summary>
    [JsonPropertyName("integrationId")]
    public string? IntegrationId { get; set; }
}

public class WidgetForecast
{
    [JsonPropertyName("labels")] public List<string> Labels { get; set; } = new();
    [JsonPropertyName("values")] public List<double> Values { get; set; } = new();
    [JsonPropertyName("lower")] public List<double> Lower { get; set; } = new();
    [JsonPropertyName("upper")] public List<double> Upper { get; set; } = new();
    [JsonPropertyName("method")] public string? Method { get; set; }
    [JsonPropertyName("note")] public string? Note { get; set; }
    [JsonPropertyName("r2")] public double? R2 { get; set; }
}
