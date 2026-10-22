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

        // Every early return below goes through this, so the analyst's own screen shows the
        // SPECIFIC reason discovery isn't working (a 401 vs. an unreachable service vs. a
        // malformed response are three completely different fixes) instead of one generic
        // warning — see ExternalIntegration.LastSchemaDiscoveryError.
        async Task<bool> Fail(string error)
        {
            await _integrations.SetSchemaDiscoveryErrorAsync(integration.Id, error, ct);
            return false;
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, integration.SchemaUrl);
            if (!string.IsNullOrWhiteSpace(integration.ConnectorAuthHeader) && !string.IsNullOrWhiteSpace(integration.ConnectorAuthValue))
                request.Headers.TryAddWithoutValidation(integration.ConnectorAuthHeader, integration.ConnectorAuthValue);

            using var response = await client.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
                return await Fail(response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                    ? "HTTP 401 — خدمة الاتصال رفضت الطلب: اسم/قيمة المصادقة هنا مش مطابقين لـ WriteApiAuthHeader/WriteApiKey في appsettings.json عند العميل."
                    : $"HTTP {(int)response.StatusCode} من خدمة الاتصال عند طلب /schema.");

            ConnectorSchemaResponse? schema;
            try
            {
                schema = await response.Content.ReadFromJsonAsync<ConnectorSchemaResponse>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);
            }
            catch (JsonException)
            {
                return await Fail("رد /schema مش JSON صالح — تأكد إن رابط الخدمة الأساسي صحيح ومش بيوصل لحاجة تانية (صفحة تسجيل دخول مثلًا).");
            }
            if (schema is null || schema.Tables.Count == 0)
                return await Fail("رد /schema رجع من غير أي جداول — تأكد إن TargetConnectionString مضبوط عند العميل ويشاور على قاعدة بيانات فيها جداول.");
            if (!ClientDbProviders.All.Contains(schema.Provider, StringComparer.OrdinalIgnoreCase))
                return await Fail($"نوع قاعدة البيانات \"{schema.Provider}\" غير مدعوم (المدعوم: SqlServer أو Sqlite).");

            var dataPermissionsAvailable = schema.Tables.Any(
                t => string.Equals(t.Name, DataPermissions.TableName, StringComparison.OrdinalIgnoreCase));

            // Never hand the data-permissions table itself to the retargeting prompt — it's
            // plumbing, not a business table, and the model has no reason to ever query it for
            // an ordinary widget (see PublishService for the deliberate, code-driven way it's
            // actually used).
            var description = string.Join("\n", schema.Tables
                .Where(t => !string.Equals(t.Name, DataPermissions.TableName, StringComparison.OrdinalIgnoreCase))
                .Select(t => $"{t.Name}({string.Join(", ", t.Columns.Select(c => $"{c.Name} {c.Type}"))})"));

            await _integrations.UpdateClientSchemaAsync(integration.Id, schema.Provider, description, dataPermissionsAvailable, ct);
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogInformation(ex, "Client schema discovery skipped for integration {IntegrationId}", integration.Id);
            return await Fail($"تعذّر الوصول لخدمة الاتصال على الرابط ده: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogInformation(ex, "Client schema discovery timed out for integration {IntegrationId}", integration.Id);
            return await Fail("انتهت مهلة الاتصال بخدمة الاتصال (لا يوجد رد) — تأكد إنها شغّالة ومتاحة من هنا.");
        }
        catch (Exception) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Client schema discovery skipped for integration {IntegrationId}", integration.Id);
            return await Fail($"خطأ غير متوقع أثناء اكتشاف البنية: {ex.Message}");
        }
    }

    /// <summary>The FilterKey values already sitting in the client's own data-permissions table
    /// (see IntegrationConnector's GET /permission-filter-keys) — so an analyst picks one from
    /// what the client has actually populated instead of typing/guessing a name. Empty (never an
    /// error) if the connector is unreachable or the table doesn't exist.</summary>
    public async Task<IReadOnlyList<string>> GetPermissionFilterKeysAsync(ExternalIntegration integration, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(integration.ConnectorBaseUrl)) return Array.Empty<string>();
        try
        {
            var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, integration.PermissionFilterKeysUrl);
            if (!string.IsNullOrWhiteSpace(integration.ConnectorAuthHeader) && !string.IsNullOrWhiteSpace(integration.ConnectorAuthValue))
                request.Headers.TryAddWithoutValidation(integration.ConnectorAuthHeader, integration.ConnectorAuthValue);

            using var response = await client.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return Array.Empty<string>();

            return await response.Content.ReadFromJsonAsync<List<string>>(cancellationToken: ct) ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Fetching permission filter keys skipped for integration {IntegrationId}", integration.Id);
            return Array.Empty<string>();
        }
    }
}
