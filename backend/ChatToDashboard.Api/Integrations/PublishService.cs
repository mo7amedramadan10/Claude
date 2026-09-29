using System.Text.Json;
using System.Text.Json.Serialization;
using ChatToDashboard.Api.History;
using ChatToDashboard.Api.Models;
using ChatToDashboard.Api.Users;

namespace ChatToDashboard.Api.Integrations;

/// <summary>Lean, rendering-only shape of a widget sent to a client's write API — deliberately
/// missing <see cref="DashboardWidget.Query"/> (the internal table name and read-only SELECT
/// that produced it): an external system has no use for that internal SQL/schema detail, and
/// not sending it is one less thing that could leak this app's internal table naming to a third
/// party. Everything a client needs to actually render and explain the widget (type, data,
/// axis keys, the ⓘ source text, an attached forecast/comparison) is still here.</summary>
public class PublishedWidget
{
    [JsonPropertyName("type")] public string Type { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("data")] public JsonElement Data { get; set; }
    [JsonPropertyName("xKey")] public string? XKey { get; set; }
    [JsonPropertyName("yKey")] public string? YKey { get; set; }
    [JsonPropertyName("source")] public string? Source { get; set; }
    [JsonPropertyName("forecast")] public WidgetForecast? Forecast { get; set; }
    [JsonPropertyName("comparison")] public JsonElement? Comparison { get; set; }
}

public class PublishedDashboardPayload
{
    [JsonPropertyName("dashboardId")] public string DashboardId { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("publishedAt")] public string PublishedAt { get; set; } = "";
    [JsonPropertyName("widgets")] public List<PublishedWidget> Widgets { get; set; } = new();

    // Narration is generated at chat time only and never persisted once a dashboard is saved
    // (see DashboardHistoryEntry — no such column), so it cannot be included here; Summary is
    // the closest persisted equivalent and is what the viewer page shows for the dashboard's
    // own short description.
    [JsonPropertyName("summary")] public string Summary { get; set; } = "";
}

public record PublishResult(bool Success, string? Error, PublishedDashboardSlot? Slot);

/// <summary>
/// Part A's "Publish action" — a deliberately separate, explicit action from saving/editing a
/// dashboard (never auto-triggered — see IntegrationsController, the only caller). Builds the
/// lean JSON payload, POSTs it to the integration's own write API with its stored credential,
/// and logs the attempt either way (IntegrationStore.LogPublishAsync) — same transparency
/// principle as UsageTracker.
/// </summary>
public class PublishService
{
    // A comparison-carrying widget legitimately omits "data" (see DashboardSpec.Validate),
    // which deserializes to a JsonElement with ValueKind Undefined — serializing that back out
    // throws (same hazard AnalyticsTools.TryParseDashboard already guards against), so it is
    // normalized to an empty array here too.
    private static readonly JsonElement EmptyArrayElement = JsonDocument.Parse("[]").RootElement;

    private readonly HistoryStore _history;
    private readonly IntegrationStore _integrations;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PublishService> _logger;

    public PublishService(
        HistoryStore history, IntegrationStore integrations, IHttpClientFactory httpClientFactory, ILogger<PublishService> logger)
    {
        _history = history;
        _integrations = integrations;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>Whether <paramref name="localHistoryId"/> has already been published under
    /// <paramref name="integrationId"/> — the controller uses this to decide whether to ask
    /// "update the existing slot or create a new one?" before calling PublishAsync (see Part A:
    /// "If it was published before: ask..."). Null means never published there.</summary>
    public Task<PublishedDashboardSlot?> FindExistingSlotAsync(string integrationId, string localHistoryId, CancellationToken ct = default) =>
        _integrations.FindSlotByLocalHistoryIdAsync(integrationId, localHistoryId, ct);

    /// <param name="slotId">Null/absent creates a brand-new dashboardId slot; set updates that
    /// existing slot in place — the caller (IntegrationsController) is what turns the analyst's
    /// explicit "استبدال المنشور الحالي / انشر كمنفصل" choice into this parameter.</param>
    public async Task<PublishResult> PublishAsync(
        string integrationId, string localHistoryId, string? slotId, AppUser requestingUser, CancellationToken ct = default)
    {
        var integration = await _integrations.GetByIdAsync(integrationId, ct);
        if (integration is null) return new PublishResult(false, "لا يوجد تكامل بهذا المعرف.", null);
        if (string.IsNullOrWhiteSpace(integration.WriteApiUrl))
            return new PublishResult(false, "نقطة الكتابة (Write API) غير مضبوطة لهذا التكامل بعد.", null);

        var entry = await _history.GetByIdAsync(localHistoryId, ct);
        if (entry is null || !entry.IsActive)
            return new PublishResult(false, "اللوحة غير موجودة أو ليست لوحة مفعّلة (Active) — النشر متاح للوحات المفعّلة فقط.", null);

        var role = await _history.ResolveRoleAsync(requestingUser.Id, entry, ct);
        if (role is null)
            return new PublishResult(false, "ليس لديك صلاحية وصول لهذه اللوحة.", null);

        PublishedDashboardSlot slot;
        if (!string.IsNullOrWhiteSpace(slotId))
        {
            var existing = await _integrations.GetSlotAsync(integrationId, slotId, ct);
            if (existing is null) return new PublishResult(false, "مكان النشر المطلوب تحديثه غير موجود.", null);
            slot = existing;
        }
        else
        {
            slot = await _integrations.CreateSlotAsync(integrationId, localHistoryId, entry.Question, DisplayName(requestingUser), ct);
        }

        var widgets = ParseWidgets(entry.WidgetsJson);
        var payload = new PublishedDashboardPayload
        {
            DashboardId = slot.ExternalDashboardId,
            Title = entry.Question,
            PublishedAt = DateTime.UtcNow.ToString("o"),
            Widgets = widgets,
            Summary = entry.Summary,
        };

        bool success;
        string? error = null;
        try
        {
            var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, integration.WriteApiUrl)
            {
                Content = JsonContent.Create(payload),
            };
            if (!string.IsNullOrWhiteSpace(integration.WriteApiAuthHeader) && !string.IsNullOrWhiteSpace(integration.WriteApiAuthValue))
                request.Headers.TryAddWithoutValidation(integration.WriteApiAuthHeader, integration.WriteApiAuthValue);

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
            await _integrations.TouchSlotAsync(slot.Id, entry.Question, DisplayName(requestingUser), ct);

        await _integrations.LogPublishAsync(new IntegrationPublishLogEntry
        {
            IntegrationId = integrationId,
            ExternalDashboardId = slot.ExternalDashboardId,
            LocalHistoryId = localHistoryId,
            DashboardTitle = entry.Question,
            UserId = requestingUser.Id,
            Success = success,
            Error = error,
        }, ct);

        return new PublishResult(success, error, success ? slot : null);
    }

    private static string DisplayName(AppUser user) => user.DisplayName is { Length: > 0 } d ? d : user.Username;

    /// <summary>Deserializes the Active dashboard's stored widgets and strips them down to
    /// PublishedWidget's rendering-only shape (see its own remarks on why Query is dropped).</summary>
    private static List<PublishedWidget> ParseWidgets(string widgetsJson)
    {
        List<DashboardWidget>? widgets;
        try
        {
            widgets = JsonSerializer.Deserialize<List<DashboardWidget>>(widgetsJson);
        }
        catch (JsonException)
        {
            return new List<PublishedWidget>();
        }
        if (widgets is null) return new List<PublishedWidget>();

        return widgets.Select(w => new PublishedWidget
        {
            Type = w.Type,
            Title = w.Title,
            Data = w.Data.ValueKind == JsonValueKind.Undefined ? EmptyArrayElement : w.Data,
            XKey = w.XKey,
            YKey = w.YKey,
            Source = w.Source,
            Forecast = w.Forecast,
            Comparison = w.Comparison,
        }).ToList();
    }
}
