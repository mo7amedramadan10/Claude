using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using ChatToDashboard.Api.History;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Models;
using ChatToDashboard.Api.Users;

namespace ChatToDashboard.Api.Integrations;

/// <summary>
/// A widget sent to a client's write API — "build the dashboard directly on the client's own
/// tables". A Publish is a design change (which widgets exist, their type/title/layout, and now
/// the live query each one runs), never a data delivery: it deliberately carries no computed
/// values — no <see cref="DashboardWidget.Data"/>, no <see cref="DashboardWidget.Forecast"/>,
/// no <see cref="DashboardWidget.Comparison"/> — those are always computed live, on every
/// viewer.html open, by the generated connector service running the query below against the
/// client's own live database. <see cref="Sql"/> is never this app's own internal query — it is
/// a fresh translation of it against the client's real schema (see
/// IIntegrationSetupAssistant.RetargetSqlAsync and PublishService.RetargetWidgetAsync), so it
/// can run verbatim there. Null when the widget had no internal query to translate, or no
/// honest equivalent exists against the client's given schema — the connector then serves that
/// widget with no data rather than guessing.
/// </summary>
public class PublishedWidget
{
    [JsonPropertyName("type")] public string Type { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("xKey")] public string? XKey { get; set; }
    [JsonPropertyName("yKey")] public string? YKey { get; set; }
    [JsonPropertyName("source")] public string? Source { get; set; }
    [JsonPropertyName("sql")] public string? Sql { get; set; }
}

/// <summary>
/// What a Publish actually sends: the dashboard's shape plus, per widget, the live query that
/// computes its current value on the client's own database — never a value itself. The
/// generated connector service (Deliverable 3) stores this on its write endpoint, and on every
/// read request re-runs each widget's Sql against the target database it's configured with,
/// attaching a fresh "data" field per widget before answering viewer.html.
/// </summary>
public class PublishedDashboardPayload
{
    [JsonPropertyName("dashboardId")] public string DashboardId { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("publishedAt")] public string PublishedAt { get; set; } = "";
    [JsonPropertyName("widgets")] public List<PublishedWidget> Widgets { get; set; } = new();

    // A short, static description of the dashboard's purpose — not a data value, so it stays
    // here despite the "structure only" rule above. Narration (the richer, spoken-aloud
    // walkthrough) is generated at chat time only and never persisted once a dashboard is
    // saved (see DashboardHistoryEntry — no such column), so it has no equivalent here.
    [JsonPropertyName("summary")] public string Summary { get; set; } = "";
}

public record PublishResult(bool Success, string? Error, PublishedDashboardSlot? Slot);

/// <summary>
/// Part A's "Publish action" — a deliberately separate, explicit action from saving/editing a
/// dashboard (never auto-triggered — see IntegrationsController, the only caller), and a
/// deliberately infrequent one: it ships a design change (a widget added/removed/restyled, or a
/// whole new dashboard), not a data refresh — see PublishedDashboardPayload's remarks. Builds
/// that structure-only JSON payload, POSTs it to the integration's own write API with its
/// stored credential, and logs the attempt either way (IntegrationStore.LogPublishAsync) — same
/// transparency principle as UsageTracker.
/// </summary>
public class PublishService
{
    private readonly HistoryStore _history;
    private readonly IntegrationStore _integrations;
    private readonly IIntegrationSetupAssistant _assistant;
    private readonly ClientSchemaDiscoveryService _schemaDiscovery;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PublishService> _logger;

    public PublishService(
        HistoryStore history, IntegrationStore integrations, IIntegrationSetupAssistant assistant,
        ClientSchemaDiscoveryService schemaDiscovery, IHttpClientFactory httpClientFactory, ILogger<PublishService> logger)
    {
        _history = history;
        _integrations = integrations;
        _assistant = assistant;
        _schemaDiscovery = schemaDiscovery;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>Whether <paramref name="localHistoryId"/> has already been published under
    /// <paramref name="integrationId"/> — the controller uses this to decide whether to ask
    /// "update the existing slot or create a new one?" before calling PrepareAsync (see Part A:
    /// "If it was published before: ask..."). Null means never published there.</summary>
    public Task<PublishedDashboardSlot?> FindExistingSlotAsync(string integrationId, string localHistoryId, CancellationToken ct = default) =>
        _integrations.FindSlotByLocalHistoryIdAsync(integrationId, localHistoryId, ct);

    /// <summary>Server-held state between Prepare and Confirm (Part E, data-level extension) —
    /// short-lived, in-memory, never persisted: losing it (a restart, or just letting it expire)
    /// costs the analyst nothing worse than clicking "نشر" again to re-prepare. Keyed by a random
    /// token neither guessable nor tied to anything else, so Confirm can never be pointed at
    /// widgets/columns the analyst never actually saw in the preview.</summary>
    private sealed class PreparedPublish
    {
        public required string IntegrationId;
        public required string LocalHistoryId;
        public required string Question;
        public required string Summary;
        public required List<PublishedWidget> Widgets;
        public required Dictionary<int, (string FilterKey, List<string> Columns)> Flagged;
        public DateTime CreatedAt = DateTime.UtcNow;
    }

    private readonly ConcurrentDictionary<string, PreparedPublish> _pending = new();
    private static readonly TimeSpan PendingTtl = TimeSpan.FromMinutes(30);

    private void PrunePending()
    {
        var cutoff = DateTime.UtcNow - PendingTtl;
        foreach (var (key, value) in _pending)
            if (value.CreatedAt < cutoff) _pending.TryRemove(key, out _);
    }

    /// <summary>Refreshes the client's schema, retargets every widget's SQL (unchanged from
    /// before), and — new — for any widget the analyst has marked with a data-level filter (see
    /// IntegrationStore.GetWidgetDataFiltersAsync), parses its retargeted query's own real output
    /// columns so the analyst picks a filter column from what the query actually returns rather
    /// than typing/guessing one. Nothing is sent to the connector yet, and nothing is logged —
    /// see ConfirmAsync, the only step that actually publishes.</summary>
    public async Task<(PublishPrepareResult? Result, string? Error)> PrepareAsync(
        string integrationId, string localHistoryId, AppUser requestingUser, CancellationToken ct = default)
    {
        PrunePending();

        var integration = await _integrations.GetByIdAsync(integrationId, ct);
        if (integration is null) return (null, "لا يوجد تكامل بهذا المعرف.");
        if (string.IsNullOrWhiteSpace(integration.ConnectorBaseUrl))
            return (null, "رابط خدمة الاتصال (Connector Base URL) غير مضبوط لهذا التكامل بعد.");

        var entry = await _history.GetByIdAsync(localHistoryId, ct);
        if (entry is null || !entry.IsActive)
            return (null, "اللوحة غير موجودة أو ليست لوحة مفعّلة (Active) — النشر متاح للوحات المفعّلة فقط.");

        var role = await _history.ResolveRoleAsync(requestingUser.Id, entry, ct);
        if (role is null) return (null, "ليس لديك صلاحية وصول لهذه اللوحة.");

        // Refresh the client's DB schema from their connector right before retargeting against
        // it (see ClientSchemaDiscoveryService) — catches a connector that's only just now been
        // pointed at a real database, or a schema that changed since it was last discovered,
        // without anyone having to remember to refresh it themselves. Best-effort: a stale or
        // still-missing schema just means BuildPublishedWidgetsAsync leaves more widgets' Sql
        // unset below, never a failed prepare.
        if (await _schemaDiscovery.TryDiscoverAsync(integration, ct))
            integration = await _integrations.GetByIdAsync(integrationId, ct) ?? integration;

        var rawWidgets = ParseWidgets(entry.WidgetsJson);
        var widgets = await BuildPublishedWidgetsAsync(rawWidgets, integration, requestingUser, ct);

        var marks = integration.DataPermissionsAvailable
            ? await _integrations.GetWidgetDataFiltersAsync(integrationId, localHistoryId, ct)
            : Array.Empty<WidgetDataFilter>();

        var flagged = new Dictionary<int, (string, List<string>)>();
        var widgetsNeedingColumn = new List<PublishPreviewWidget>();
        foreach (var mark in marks)
        {
            if (mark.WidgetIndex < 0 || mark.WidgetIndex >= widgets.Count) continue;
            var widget = widgets[mark.WidgetIndex];
            var columns = widget.Sql is { Length: > 0 } sql ? ExtractSelectColumns(sql) : new List<string>();
            flagged[mark.WidgetIndex] = (mark.FilterKey, columns);
            widgetsNeedingColumn.Add(new PublishPreviewWidget
            {
                Index = mark.WidgetIndex, Title = widget.Title, Sql = widget.Sql, Columns = columns, FilterKey = mark.FilterKey,
            });
        }

        var previewId = Guid.NewGuid().ToString("N");
        _pending[previewId] = new PreparedPublish
        {
            IntegrationId = integrationId, LocalHistoryId = localHistoryId,
            Question = entry.Question, Summary = entry.Summary, Widgets = widgets, Flagged = flagged,
        };

        return (new PublishPrepareResult { PreviewId = previewId, WidgetsNeedingColumn = widgetsNeedingColumn }, null);
    }

    /// <param name="slotId">Null/absent creates a brand-new dashboardId slot; set updates that
    /// existing slot in place — the caller (IntegrationsController) is what turns the analyst's
    /// explicit "استبدال المنشور الحالي / انشر كمنفصل" choice into this parameter.</param>
    public async Task<PublishResult> ConfirmAsync(
        string integrationId, string previewId, string? slotId, List<ConfirmColumnChoice> columnChoices,
        AppUser requestingUser, CancellationToken ct = default)
    {
        PrunePending();

        if (!_pending.TryRemove(previewId, out var prepared) || prepared.IntegrationId != integrationId)
            return new PublishResult(false, "انتهت صلاحية معاينة النشر أو أُرسلت بالفعل — دوس «نشر» تاني.", null);

        var integration = await _integrations.GetByIdAsync(integrationId, ct);
        if (integration is null) return new PublishResult(false, "لا يوجد تكامل بهذا المعرف.", null);

        // Every widget the analyst flagged for data-level filtering MUST get a valid column
        // choice — one of the exact columns Prepare actually offered, never an arbitrary string
        // (defense in depth against a client calling this endpoint directly, bypassing the UI).
        // A flagged widget with no valid choice fails the whole publish rather than shipping
        // unfiltered: silently downgrading a widget someone deliberately marked as sensitive is
        // exactly the kind of guess this design never makes.
        var choiceByIndex = columnChoices.ToDictionary(c => c.WidgetIndex, c => c.Column);
        foreach (var (index, (filterKey, columns)) in prepared.Flagged)
        {
            if (prepared.Widgets[index].Sql is not { Length: > 0 } sql) continue; // nothing to wrap — no retargeted query at all
            if (!choiceByIndex.TryGetValue(index, out var column) || !columns.Contains(column, StringComparer.Ordinal))
                return new PublishResult(false, $"العنصر «{prepared.Widgets[index].Title}» معلّم بفلترة البيانات لكن مفيش عمود مؤكّد له.", null);

            var wrapped = WrapWithDataPermissionFilter(sql, column, filterKey);
            if (AnalyticsTools.ValidateReadOnlySql(wrapped) is not null)
                return new PublishResult(false, $"تعذّر بناء استعلام آمن للعنصر «{prepared.Widgets[index].Title}».", null);
            prepared.Widgets[index].Sql = wrapped;
        }

        PublishedDashboardSlot slot;
        if (!string.IsNullOrWhiteSpace(slotId))
        {
            var existing = await _integrations.GetSlotAsync(integrationId, slotId, ct);
            if (existing is null) return new PublishResult(false, "مكان النشر المطلوب تحديثه غير موجود.", null);
            slot = existing;
        }
        else
        {
            slot = await _integrations.CreateSlotAsync(integrationId, prepared.LocalHistoryId, prepared.Question, DisplayName(requestingUser), ct);
        }

        var payload = new PublishedDashboardPayload
        {
            DashboardId = slot.ExternalDashboardId,
            Title = prepared.Question,
            PublishedAt = DateTime.UtcNow.ToString("o"),
            Widgets = prepared.Widgets,
            Summary = prepared.Summary,
        };

        bool success;
        string? error = null;
        try
        {
            var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, integration.PublishUrl)
            {
                Content = JsonContent.Create(payload),
            };
            if (!string.IsNullOrWhiteSpace(integration.ConnectorAuthHeader) && !string.IsNullOrWhiteSpace(integration.ConnectorAuthValue))
                request.Headers.TryAddWithoutValidation(integration.ConnectorAuthHeader, integration.ConnectorAuthValue);

            using var response = await client.SendAsync(request, ct);
            success = response.IsSuccessStatusCode;
            if (!success)
                error = $"استجابة غير ناجحة من نقطة الكتابة عند العميل: HTTP {(int)response.StatusCode}.";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Publish to integration {IntegrationId} failed", integrationId);
            success = false;
            error = $"تعذّر الوصول لنقطة الكتابة عند العميل: {ex.Message}";
        }

        if (success && !string.IsNullOrWhiteSpace(slotId))
            await _integrations.TouchSlotAsync(slot.Id, prepared.Question, DisplayName(requestingUser), ct);

        await _integrations.LogPublishAsync(new IntegrationPublishLogEntry
        {
            IntegrationId = integrationId,
            ExternalDashboardId = slot.ExternalDashboardId,
            LocalHistoryId = prepared.LocalHistoryId,
            DashboardTitle = prepared.Question,
            UserId = requestingUser.Id,
            Success = success,
            Error = error,
        }, ct);

        return new PublishResult(success, error, success ? slot : null);
    }

    private static string DisplayName(AppUser user) => user.DisplayName is { Length: > 0 } d ? d : user.Username;

    /// <summary>Wraps an already-retargeted widget query in a deterministic, code-built subquery
    /// — never LLM-written — that only lets through rows whose value in <paramref name="column"/>
    /// appears in the client's own data-permissions table for the current viewer's identity and
    /// the analyst-chosen <paramref name="filterKey"/> (see DataPermissions and the connector's
    /// own remarks on @__viewer_identity__). <paramref name="column"/> was already checked
    /// against Prepare's own parsed column list by the caller; <paramref name="filterKey"/> is
    /// escaped here as a literal since it never varies per request (chosen once, at publish
    /// time) — only the viewer's identity is a real bound parameter, filled in by the connector
    /// per request.</summary>
    private static string WrapWithDataPermissionFilter(string sql, string column, string filterKey) =>
        $"SELECT * FROM ({sql}) AS _base WHERE _base.{column} IN " +
        $"(SELECT FilterValue FROM {DataPermissions.TableName} WHERE UserId = @__viewer_identity__ AND FilterKey = '{filterKey.Replace("'", "''")}')";

    /// <summary>Parses the top-level SELECT list of a widget's retargeted query to name its own
    /// output columns — deterministic, our own code, never the model's self-report — so the
    /// column picker in the publish modal only ever offers what the query actually returns.
    /// Handles the shapes RetargetSqlAsync actually produces (a plain SELECT, no leading WITH);
    /// an expression with neither a bare name nor an explicit "AS alias" is simply left out
    /// rather than guessed at — the analyst then just has fewer choices, never a wrong one.</summary>
    private static List<string> ExtractSelectColumns(string sql)
    {
        var match = Regex.Match(sql, @"^\s*SELECT\s+(.*?)\s+FROM\s", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (!match.Success) return new List<string>();

        var selectList = match.Groups[1].Value;
        var parts = new List<string>();
        var depth = 0;
        var start = 0;
        for (var i = 0; i < selectList.Length; i++)
        {
            if (selectList[i] == '(') depth++;
            else if (selectList[i] == ')') depth--;
            else if (selectList[i] == ',' && depth == 0)
            {
                parts.Add(selectList[start..i]);
                start = i + 1;
            }
        }
        parts.Add(selectList[start..]);

        var columns = new List<string>();
        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            var asMatch = Regex.Match(trimmed, @"\bAS\s+([A-Za-z_][A-Za-z0-9_]*)\s*$", RegexOptions.IgnoreCase);
            if (asMatch.Success) { columns.Add(asMatch.Groups[1].Value); continue; }

            var bareMatch = Regex.Match(trimmed, @"^([A-Za-z_][A-Za-z0-9_]*\.)?([A-Za-z_][A-Za-z0-9_]*)$");
            if (bareMatch.Success) { columns.Add(bareMatch.Groups[2].Value); continue; }
            // An unaliased expression (e.g. a bare COUNT(*)) — can't safely name it, so it's
            // simply not offered as a filter-column choice.
        }
        return columns;
    }

    /// <summary>Deserializes the Active dashboard's stored widgets as-is — including each
    /// one's own internal Query, which BuildPublishedWidgetsAsync below uses purely as a
    /// translation reference (never published verbatim; see PublishedWidget's remarks).</summary>
    private static List<DashboardWidget> ParseWidgets(string widgetsJson)
    {
        try
        {
            return JsonSerializer.Deserialize<List<DashboardWidget>>(widgetsJson) ?? new List<DashboardWidget>();
        }
        catch (JsonException)
        {
            return new List<DashboardWidget>();
        }
    }

    /// <summary>Strips each widget to PublishedWidget's shape and, when the integration has a
    /// client schema on file, retargets its internal query to run against that schema (see
    /// IIntegrationSetupAssistant.RetargetSqlAsync) — "build the dashboard directly on the
    /// client's own tables". No client schema configured, no internal query to translate from,
    /// a translation the model couldn't produce, or one that fails the same read-only check
    /// every internally-executed query passes: any of these just leaves that widget's Sql null
    /// rather than failing the whole publish — a dashboard can ship with some widgets live and
    /// others not yet wired up.</summary>
    private async Task<List<PublishedWidget>> BuildPublishedWidgetsAsync(
        List<DashboardWidget> widgets, ExternalIntegration integration, AppUser requestingUser, CancellationToken ct)
    {
        var hasClientSchema = !string.IsNullOrWhiteSpace(integration.ClientDbProvider)
            && !string.IsNullOrWhiteSpace(integration.ClientSchemaDescription);

        var result = new List<PublishedWidget>(widgets.Count);
        foreach (var w in widgets)
        {
            string? sql = null;
            if (hasClientSchema)
            {
                var candidate = await _assistant.RetargetSqlAsync(
                    w.Title, w.Query?.Sql, integration.ClientDbProvider!, integration.ClientSchemaDescription!, requestingUser, ct);
                if (candidate is not null && AnalyticsTools.ValidateReadOnlySql(candidate) is null)
                    sql = candidate;
            }

            result.Add(new PublishedWidget
            {
                Type = w.Type,
                Title = w.Title,
                XKey = w.XKey,
                YKey = w.YKey,
                Source = w.Source,
                Sql = sql,
            });
        }
        return result;
    }
}
