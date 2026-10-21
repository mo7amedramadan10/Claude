using System.Collections.Concurrent;
using ChatToDashboard.Api.Data;
using Dapper;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

namespace ChatToDashboard.Api.Llm;

/// <summary>
/// مفاتيح وعناوين مزوّدي الذكاء الاصطناعي (Anthropic / OpenAI / Ollama) مخزَّنة في قاعدة البيانات
/// ليديرها المسؤول من صفحة «مفاتيح الموديلات» بلا user-secrets ولا إعادة تشغيل. المفتاح يُحفظ مشفّرًا
/// بـ Data Protection (لا يُرجَع أبدًا للواجهة إلا آخر 4 أحرف). القيمة في قاعدة البيانات تغلب
/// الإعدادات (appsettings / user-secrets / متغيرات البيئة) التي تبقى بديلًا لكل حقل لم يُحفظ هنا.
/// </summary>
public class LlmCredentialStore
{
    public const string ProtectorPurpose = "ChatToDashboard.LlmCredentials.v1";

    public sealed record Credential(string? ApiKey, string? BaseUrl, string KeySource, string BaseUrlSource, DateTime? UpdatedAt);

    private readonly DataStore _db;
    private readonly IConfiguration _configuration;
    private readonly IDataProtector _protector;
    private readonly ILogger<LlmCredentialStore> _logger;
    private readonly SemaphoreSlim _schemaGate = new(1, 1);
    private volatile bool _schemaReady;
    private readonly ConcurrentDictionary<string, Credential> _cache = new(StringComparer.OrdinalIgnoreCase);

    public LlmCredentialStore(DataStore db, IConfiguration configuration, IDataProtectionProvider protection, ILogger<LlmCredentialStore> logger)
    {
        _db = db;
        _configuration = configuration;
        _protector = protection.CreateProtector(ProtectorPurpose);
        _logger = logger;
    }

    /// <summary>العنوان الافتراضي لكل مزوّد عندما لا يُحفظ عنوان في القاعدة ولا في الإعدادات.</summary>
    public static string DefaultBaseUrl(string provider) => provider switch
    {
        LlmRouter.OpenAI => "https://api.openai.com/",
        LlmRouter.Ollama => "http://172.17.242.1:8081/api/v1/",
        _ => "https://api.anthropic.com/",
    };

    private static (string Key, string? BaseUrl) ConfigKeys(string provider) => provider switch
    {
        LlmRouter.OpenAI => ("OpenAI:ApiKey", "OpenAI:BaseUrl"),
        LlmRouter.Ollama => ("Ollama:ApiKey", "Ollama:BaseUrl"),
        _ => ("Anthropic:ApiKey", "Anthropic:BaseUrl"),
    };

    private string Table => _db.Provider == DbProvider.Sqlite ? "\"LlmCredentials\"" : "[staging].[LlmCredentials]";

    private async Task EnsureSchemaOnceAsync(CancellationToken ct)
    {
        if (_schemaReady) return;
        await _schemaGate.WaitAsync(ct);
        try { if (!_schemaReady) { await EnsureSchemaAsync(ct); _schemaReady = true; } }
        finally { _schemaGate.Release(); }
    }

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);
        var text = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {Table} (
                 "Provider" TEXT NOT NULL PRIMARY KEY, "ApiKey" TEXT, "BaseUrl" TEXT, "UpdatedAt" TEXT NOT NULL)
               """
            : $"""
               IF OBJECT_ID('staging.LlmCredentials') IS NULL
               CREATE TABLE {Table} (
                 [Provider] NVARCHAR(50) NOT NULL PRIMARY KEY, [ApiKey] NVARCHAR(MAX), [BaseUrl] NVARCHAR(500), [UpdatedAt] DATETIME2 NOT NULL)
               """;
        await using var command = connection.CreateCommand();
        command.CommandText = text;
        await command.ExecuteNonQueryAsync(ct);
    }

    private static string Canonical(string provider) =>
        LlmRouter.KnownProviders.FirstOrDefault(p => string.Equals(p, provider, StringComparison.OrdinalIgnoreCase))
        ?? throw new ArgumentException($"مزوّد غير معروف: {provider}");

    /// <summary>المفتاح والعنوان الفعليان للمزوّد (القاعدة أولًا ثم الإعدادات)، مع مصدر كل حقل.</summary>
    public async Task<Credential> GetAsync(string provider, CancellationToken ct = default)
    {
        provider = Canonical(provider);
        if (_cache.TryGetValue(provider, out var cached)) return cached;
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var row = await connection.QuerySingleOrDefaultAsync<Row>($"SELECT Provider, ApiKey, BaseUrl, UpdatedAt FROM {Table} WHERE Provider = @provider", new { provider });

        string? dbKey = null;
        if (row?.ApiKey is { Length: > 0 } protectedKey)
        {
            try { dbKey = _protector.Unprotect(protectedKey); }
            catch (Exception ex)
            {
                // مفاتيح Data Protection تغيّرت (نقل التطبيق لخادم آخر بلا مجلد المفاتيح): المفتاح المحفوظ لا يُقرأ
                // فيُعامل كغير موجود ويظهر للمسؤول «أعد إدخاله».
                _logger.LogWarning(ex, "تعذّر فك تشفير مفتاح {Provider} المحفوظ — أعد إدخاله من صفحة مفاتيح الموديلات", provider);
            }
        }
        var (keyPath, basePath) = ConfigKeys(provider);
        var configKey = _configuration[keyPath]?.Trim();
        var configBase = basePath is null ? null : _configuration[basePath]?.Trim();
        var key = dbKey is { Length: > 0 } ? dbKey : configKey is { Length: > 0 } ? configKey : null;
        var keySource = dbKey is { Length: > 0 } ? "db" : configKey is { Length: > 0 } ? "config" : row?.ApiKey is { Length: > 0 } ? "unreadable" : "none";
        var baseUrl = row?.BaseUrl is { Length: > 0 } b ? b : configBase is { Length: > 0 } ? configBase : null;
        var baseSource = row?.BaseUrl is { Length: > 0 } ? "db" : configBase is { Length: > 0 } ? "config" : "default";
        var credential = new Credential(key, baseUrl, keySource, baseSource, row?.UpdatedAt);
        _cache[provider] = credential;
        return credential;
    }

    /// <summary>المفتاح أو استثناء برسالة توجّه إلى صفحة «مفاتيح الموديلات» (كانت الرسالة القديمة توجّه إلى user-secrets).</summary>
    public async Task<string> RequireApiKeyAsync(string provider, CancellationToken ct = default)
    {
        var c = await GetAsync(provider, ct);
        if (c.ApiKey is { Length: > 0 }) return c.ApiKey;
        var label = provider switch { LlmRouter.OpenAI => "OpenAI", LlmRouter.Ollama => "الموديل الداخلي (Ollama)", _ => "Claude (Anthropic)" };
        throw new InvalidOperationException(
            c.KeySource == "unreadable"
                ? $"مفتاح {label} المحفوظ لا يمكن قراءته بعد تغيّر مفاتيح التشفير — أعد إدخاله من «مفاتيح الموديلات»."
                : $"مفتاح {label} غير مضبوط — أدخله من صفحة «مفاتيح الموديلات» في الإعدادات.");
    }

    /// <summary>العنوان الأساسي الفعلي (القاعدة ← الإعدادات ← الافتراضي) منتهيًا بشرطة مائلة.</summary>
    public async Task<Uri> BaseUriAsync(string provider, CancellationToken ct = default)
    {
        var c = await GetAsync(provider, ct);
        var url = c.BaseUrl is { Length: > 0 } ? c.BaseUrl : DefaultBaseUrl(provider);
        if (!url.EndsWith('/')) url += "/";
        return new Uri(url);
    }

    /// <summary>يحفظ المفتاح (null = يبقى كما هو، "" = يُمسح) والعنوان (null/"" = يُمسح فيعود للإعدادات أو الافتراضي).</summary>
    public async Task SetAsync(string provider, string? apiKey, string? baseUrl, CancellationToken ct = default)
    {
        provider = Canonical(provider);
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var existing = await connection.QuerySingleOrDefaultAsync<Row>($"SELECT Provider, ApiKey, BaseUrl, UpdatedAt FROM {Table} WHERE Provider = @provider", new { provider });
        var trimmedKey = apiKey?.Trim();
        var storedKey = trimmedKey is null ? existing?.ApiKey : trimmedKey.Length == 0 ? null : _protector.Protect(trimmedKey);
        var storedBase = string.IsNullOrWhiteSpace(baseUrl) ? null : baseUrl.Trim();
        if (storedBase is not null && (!Uri.TryCreate(storedBase, UriKind.Absolute, out var parsed) || parsed.Scheme is not ("http" or "https")))
            throw new ArgumentException("العنوان يجب أن يكون رابطًا كاملًا يبدأ بـ http:// أو https://");
        var args = new { provider, apiKey = storedKey, baseUrl = storedBase, now = DateTime.UtcNow };
        var sql = _db.Provider == DbProvider.Sqlite
            ? $"""
               INSERT INTO {Table} (Provider, ApiKey, BaseUrl, UpdatedAt) VALUES (@provider, @apiKey, @baseUrl, @now)
               ON CONFLICT(Provider) DO UPDATE SET ApiKey = @apiKey, BaseUrl = @baseUrl, UpdatedAt = @now
               """
            : $"""
               MERGE {Table} AS t USING (SELECT @provider AS Provider) AS s ON t.Provider = s.Provider
               WHEN MATCHED THEN UPDATE SET ApiKey = @apiKey, BaseUrl = @baseUrl, UpdatedAt = @now
               WHEN NOT MATCHED THEN INSERT (Provider, ApiKey, BaseUrl, UpdatedAt) VALUES (@provider, @apiKey, @baseUrl, @now);
               """;
        await connection.ExecuteAsync(sql, args);
        _cache.TryRemove(provider, out _);
    }

    /// <summary>يحذف ما حُفظ في القاعدة للمزوّد فيعود إلى الإعدادات (إن وُجدت).</summary>
    public async Task ClearAsync(string provider, CancellationToken ct = default)
    {
        provider = Canonical(provider);
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"DELETE FROM {Table} WHERE Provider = @provider", new { provider });
        _cache.TryRemove(provider, out _);
    }

    /// <summary>آخر 4 أحرف من المفتاح للعرض فقط (أو null).</summary>
    public static string? Hint(string? apiKey) =>
        apiKey is { Length: > 4 } ? "…" + apiKey[^4..] : apiKey is { Length: > 0 } ? "…" : null;

    private sealed class Row
    {
        public string Provider { get; set; } = "";
        public string? ApiKey { get; set; }
        public string? BaseUrl { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
