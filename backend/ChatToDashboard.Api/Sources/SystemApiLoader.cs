using System.Data;
using System.Text;
using System.Text.Json;
using ChatToDashboard.Api.Data;
using Microsoft.Extensions.Options;

namespace ChatToDashboard.Api.Sources;

public record LoadedSystem(string System, string Table, int Records, string? Error);

/// <summary>
/// Pulls each configured system's records from its HTTP endpoint and loads them into a
/// staging table, so the SQL tool can aggregate over them exactly like a loaded file.
/// The data is cached in the database and refreshed on demand, not fetched per question.
/// </summary>
public class SystemApiLoader
{
    private static readonly string[] CandidatePaths = { "result.items", "result.data", "result", "items", "data" };

    private readonly SourceOptions _options;
    private readonly DataStore _db;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<SystemApiLoader> _logger;

    public SystemApiLoader(
        IOptions<SourceOptions> options,
        DataStore db,
        IHttpClientFactory httpFactory,
        ILogger<SystemApiLoader> logger)
    {
        _options = options.Value;
        _db = db;
        _httpFactory = httpFactory;
        _logger = logger;
    }

    /// <summary>The staging table a system's records land in.</summary>
    public string TableFor(SystemSource system) =>
        _db.DisplayTable("sys_" + DataFolderLoader.SanitizeTableName(system.Id));

    public async Task<IReadOnlyList<LoadedSystem>> LoadAllAsync(CancellationToken ct = default)
    {
        var systems = _options.Systems.Where(s => s.HasApi).ToList();
        if (systems.Count == 0) return Array.Empty<LoadedSystem>();

        var results = new List<LoadedSystem>();
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);

        foreach (var system in systems)
        {
            ct.ThrowIfCancellationRequested();
            var bareName = "sys_" + DataFolderLoader.SanitizeTableName(system.Id);
            try
            {
                var records = await FetchAsync(system, ct);
                var table = BuildTable(records, system.Api!.MaxRecords);
                if (table.Columns.Count == 0)
                {
                    results.Add(new LoadedSystem(system.Name, TableFor(system), 0,
                        "الاستجابة لا تحتوي على سجلات."));
                    continue;
                }

                await _db.RecreateAndLoadAsync(connection, bareName, table, ct);
                results.Add(new LoadedSystem(system.Name, TableFor(system), table.Rows.Count, null));
                _logger.LogInformation("Loaded {Count} record(s) from {System} into {Table}",
                    table.Rows.Count, system.Name, TableFor(system));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load system {System} from {Url}", system.Name, system.Api?.Url);
                results.Add(new LoadedSystem(system.Name, TableFor(system), 0, ex.Message));
            }
        }
        return results;
    }

    private async Task<List<JsonElement>> FetchAsync(SystemSource system, CancellationToken ct)
    {
        var api = system.Api!;
        var client = _httpFactory.CreateClient(api.AllowInvalidCertificate
            ? SystemApiClients.Insecure
            : SystemApiClients.Default);
        client.Timeout = TimeSpan.FromSeconds(api.TimeoutSeconds);

        using var request = new HttpRequestMessage(new HttpMethod(api.Method), api.Url);
        foreach (var (key, value) in api.Headers) request.Headers.TryAddWithoutValidation(key, value);
        if (!string.IsNullOrWhiteSpace(api.Body) && !HttpMethod.Get.Method.Equals(api.Method, StringComparison.OrdinalIgnoreCase))
            request.Content = new StringContent(api.Body, Encoding.UTF8, "application/json");

        using var response = await client.SendAsync(request, ct);
        var text = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"{(int)response.StatusCode} {response.ReasonPhrase}: {Truncate(text, 400)}");

        using var document = JsonDocument.Parse(text);
        var array = FindArray(document.RootElement, api.ResultPath);
        if (array is null)
            throw new InvalidDataException(
                "لم يتم العثور على مصفوفة سجلات في الاستجابة. حدّد ResultPath في الإعدادات " +
                $"(مثال \"result.items\"). بداية الاستجابة: {Truncate(text, 300)}");

        // Clone: the elements outlive the JsonDocument, which is disposed on return.
        return array.Value.EnumerateArray().Select(e => e.Clone()).ToList();
    }

    /// <summary>Finds the record array, either at an explicit path or at a conventional one.</summary>
    private static JsonElement? FindArray(JsonElement root, string? path)
    {
        if (!string.IsNullOrWhiteSpace(path))
            return Navigate(root, path) is { ValueKind: JsonValueKind.Array } explicitMatch ? explicitMatch : null;

        if (root.ValueKind == JsonValueKind.Array) return root;
        foreach (var candidate in CandidatePaths)
            if (Navigate(root, candidate) is { ValueKind: JsonValueKind.Array } found) return found;

        // Last resort: the first array-valued property anywhere one level down.
        if (root.ValueKind == JsonValueKind.Object)
            foreach (var property in root.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Array) return property.Value;
                if (property.Value.ValueKind == JsonValueKind.Object)
                    foreach (var nested in property.Value.EnumerateObject())
                        if (nested.Value.ValueKind == JsonValueKind.Array) return nested.Value;
            }
        return null;
    }

    private static JsonElement? Navigate(JsonElement element, string path)
    {
        var current = element;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (current.ValueKind != JsonValueKind.Object) return null;
            if (!current.TryGetProperty(segment, out var next)) return null;
            current = next;
        }
        return current;
    }

    /// <summary>
    /// Flattens the records into columns. Nested objects become dotted column names
    /// (customer.name); arrays are kept as their JSON text.
    /// </summary>
    private static DataTable BuildTable(List<JsonElement> records, int maxRecords)
    {
        var headers = new List<string>();
        var index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var rows = new List<Dictionary<string, string?>>();

        foreach (var record in records.Take(maxRecords))
        {
            if (record.ValueKind != JsonValueKind.Object) continue;
            var row = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            Flatten(record, prefix: null, row, headers, index);
            rows.Add(row);
        }

        var matrix = rows.Select(row =>
        {
            var values = new string?[headers.Count];
            foreach (var (key, value) in row)
                if (index.TryGetValue(key, out var i)) values[i] = value;
            return values;
        }).ToList();

        return DataFolderLoader.InferTypes((headers, matrix));
    }

    private static void Flatten(
        JsonElement element, string? prefix,
        Dictionary<string, string?> row, List<string> headers, Dictionary<string, int> index)
    {
        foreach (var property in element.EnumerateObject())
        {
            var name = prefix is null ? property.Name : $"{prefix}.{property.Name}";
            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                Flatten(property.Value, name, row, headers, index);
                continue;
            }

            if (!index.ContainsKey(name))
            {
                index[name] = headers.Count;
                headers.Add(name);
            }
            row[name] = property.Value.ValueKind switch
            {
                JsonValueKind.Null or JsonValueKind.Undefined => null,
                JsonValueKind.String => property.Value.GetString(),
                _ => property.Value.GetRawText(),
            };
        }
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max] + "…";
}

public static class SystemApiClients
{
    public const string Default = "system-api";

    /// <summary>For an internal endpoint whose TLS certificate does not validate.</summary>
    public const string Insecure = "system-api-insecure";
}
