using System.Text.Json;

namespace ChatToDashboard.Api.Integrations;

/// <summary>
/// Discovers the client's own database shape from their deployed connector's own GET /schema
/// (see IntegrationConnector's TargetDatabase.GetSchemaAsync) — never typed by hand. Runs
/// automatically rather than behind a manual button: IntegrationsController.UpdateApis calls it
/// right after a new connector base URL is saved, and PublishService calls it again right before
/// building each widget's retargeted SQL, so the schema Publish retargets against is always as
/// fresh as the connector can currently answer, without anyone having to remember to refresh it.
/// </summary>
public class ClientSchemaDiscoveryService
{
    private readonly IntegrationStore _integrations;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ClientSchemaDiscoveryService> _logger;

    public ClientSchemaDiscoveryService(
        IntegrationStore integrations, IHttpClientFactory httpClientFactory, ILogger<ClientSchemaDiscoveryService> logger)
    {
        _integrations = integrations;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <returns>true if the schema was (re)discovered and saved. False on any failure — a
    /// connector not deployed yet, unreachable, or answering with no tables — which is never
    /// fatal to the caller: PublishService just leaves affected widgets' Sql unset (see its own
    /// remarks) rather than guessing, and UpdateApis's save still succeeds either way.</returns>
    public async Task<bool> TryDiscoverAsync(ExternalIntegration integration, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(integration.ConnectorBaseUrl)) return false;

        try
        {
            var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, integration.SchemaUrl);
            if (!string.IsNullOrWhiteSpace(integration.ConnectorAuthHeader) && !string.IsNullOrWhiteSpace(integration.ConnectorAuthValue))
                request.Headers.TryAddWithoutValidation(integration.ConnectorAuthHeader, integration.ConnectorAuthValue);

            using var response = await client.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return false;

            var schema = await response.Content.ReadFromJsonAsync<ConnectorSchemaResponse>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);
            if (schema is null || schema.Tables.Count == 0) return false;
            if (!ClientDbProviders.All.Contains(schema.Provider, StringComparer.OrdinalIgnoreCase)) return false;

            var description = string.Join("\n", schema.Tables.Select(t =>
                $"{t.Name}({string.Join(", ", t.Columns.Select(c => $"{c.Name} {c.Type}"))})"));

            await _integrations.UpdateClientSchemaAsync(integration.Id, schema.Provider, description, ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Client schema discovery skipped for integration {IntegrationId}", integration.Id);
            return false;
        }
    }
}
