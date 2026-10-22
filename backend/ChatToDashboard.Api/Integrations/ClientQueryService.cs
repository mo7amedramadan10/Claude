using ChatToDashboard.Api.Llm;

namespace ChatToDashboard.Api.Integrations;

/// <summary>
/// Runs a live, read-only query against an external integration's real database WHILE AN
/// ANALYST IS BUILDING A DASHBOARD — via the connector's POST /query (see its own remarks) —
/// so a widget built on this source shows real rows immediately, not only once published and
/// opened at the client. Called from AnalyticsTools.ExecuteToolAsync (the query_client_data
/// tool), the exact same place query_data itself is executed, just routed to the connector
/// instead of this app's own DataStore.
/// </summary>
public class ClientQueryService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ClientQueryService> _logger;

    public ClientQueryService(IHttpClientFactory httpClientFactory, ILogger<ClientQueryService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <returns>The connector's own JSON response body verbatim — already shaped exactly like
    /// query_data's own tool result ({"rowCount":N,"rows":[...]}) — on success; an Arabic error
    /// message (never thrown) on any failure, so ExecuteToolAsync can feed it back to the model
    /// exactly like any other rejected tool call.</returns>
    public async Task<(string? ResultJson, string? Error)> RunQueryAsync(
        ExternalIntegration integration, string sql, CancellationToken ct = default)
    {
        // Independent read-only check on our own side, before this ever reaches the network —
        // the connector re-checks it again itself (ReadOnlySqlValidator), the same double-
        // safeguard every other query in this system already gets. Never trust one side alone.
        var validationError = AnalyticsTools.ValidateReadOnlySql(sql);
        if (validationError is not null) return (null, $"الاستعلام مرفوض: {validationError}");

        if (string.IsNullOrWhiteSpace(integration.ConnectorBaseUrl))
            return (null, "رابط خدمة الاتصال (Connector) غير مضبوط لهذا التكامل بعد.");

        try
        {
            var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, integration.QueryUrl)
            {
                Content = JsonContent.Create(new { sql }),
            };
            if (!string.IsNullOrWhiteSpace(integration.ConnectorAuthHeader) && !string.IsNullOrWhiteSpace(integration.ConnectorAuthValue))
                request.Headers.TryAddWithoutValidation(integration.ConnectorAuthHeader, integration.ConnectorAuthValue);

            using var response = await client.SendAsync(request, ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
                return (null, $"تعذّر تنفيذ الاستعلام عند العميل (HTTP {(int)response.StatusCode}): {body}");

            return (body, null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Live query to integration {IntegrationId} failed", integration.Id);
            return (null, $"تعذّر الوصول لخدمة الاتصال عند العميل: {ex.Message}");
        }
    }
}
