using System.Text.Json;
using System.Text.Json.Nodes;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

public class UpdateLlmSettingsRequest
{
    public string Provider { get; set; } = "";
    public string? OllamaModel { get; set; }
    public string? OpenAiModel { get; set; }
}

public class UpdateDocumentReaderRequest
{
    /// <summary>One of LlmRouter's known provider ids, or null/"" to disable.</summary>
    public string? Provider { get; set; }

    /// <summary>The document reader's OWN sub-model choice for that provider — independent of
    /// the dashboard-building OllamaModel/OpenAiModel in UpdateLlmSettingsRequest above. Only
    /// the field matching Provider is meaningful; the other is ignored.</summary>
    public string? OllamaModel { get; set; }
    public string? OpenAiModel { get; set; }
}

public class UpdateImageReaderRequest
{
    /// <summary>One of LlmRouter's known provider ids, or null/"" to fall back to the
    /// dashboard-building Provider for a request that carries a reference image.</summary>
    public string? Provider { get; set; }

    /// <summary>The image reader's OWN sub-model choice for that provider — independent of
    /// both UpdateLlmSettingsRequest's and UpdateDocumentReaderRequest's own model fields.
    /// Only the field matching Provider is meaningful; the other is ignored.</summary>
    public string? OllamaModel { get; set; }
    public string? OpenAiModel { get; set; }
}

public class UpdateVisualIdentityReaderRequest
{
    /// <summary>One of LlmRouter's known provider ids, or null/"" to disable — the External
    /// Integrations visual-identity image extraction (a logo/brand image with no accompanying
    /// text) refuses outright rather than falling back to any other provider.</summary>
    public string? Provider { get; set; }

    /// <summary>The visual-identity reader's OWN sub-model choice — independent of every other
    /// model field on this controller. Only the field matching Provider is meaningful.</summary>
    public string? OllamaModel { get; set; }
    public string? OpenAiModel { get; set; }

    /// <summary>Explicit, admin-set — never inferred from the provider/model name. Must be
    /// true for the image-extraction action to run at all (see VisualIdentityImageRouter).</summary>
    public bool SupportsImage { get; set; }
}

public class UpdateSemanticsAnalyzerRequest
{
    /// <summary>null/"" = نفس موديل اللوحات؛ "none" = معطَّل؛ وإلا أحد مزوّدي LlmRouter.</summary>
    public string? Provider { get; set; }
    public string? OllamaModel { get; set; }
    public string? OpenAiModel { get; set; }
}

public class UpdateCredentialRequest
{
    /// <summary>null = لا تغيير على المفتاح؛ "" = مسح المحفوظ (يعود للإعدادات إن وُجدت).</summary>
    public string? ApiKey { get; set; }
    /// <summary>null/"" = مسح العنوان المحفوظ (يعود للإعدادات ثم الافتراضي).</summary>
    public string? BaseUrl { get; set; }
}

/// <summary>
/// Which LLM answers questions, and which model for the providers that support picking one
/// (Ollama, OpenAI) — admin-only, changeable from the dashboard without a restart. See
/// LlmRouter for how a change here takes effect on the very next question.
/// </summary>
[ApiController]
[Route("api/llm-settings")]
[Authorize(Roles = UserRoles.Admin)]
public class LlmSettingsController : ControllerBase
{
    // The models the gateway's Connection Guide listed as installed — used only if the live
    // GET /models call to the gateway fails or is unreachable (e.g. no network path to it
    // from wherever this app happens to run). qwen3-vl:32b is vision-capable (unlike the
    // text-only qwen3/gemma4 models above it) — pick it for the Image Analysis Model setting
    // in الإعدادات, not just the main dashboard-building model, since it's the one model here
    // that can actually read an attached image.
    private static readonly string[] FallbackOllamaModels =
        { "qwen3:14b", "qwen3:32b", "qwen3:30b-a3b-instruct-2507-q4_K_M", "gemma4:31b", "qwen3-vl:32b" };

    private readonly LlmSettingsStore _settings;
    private readonly LlmCredentialStore _credentials;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public LlmSettingsController(LlmSettingsStore settings, LlmCredentialStore credentials, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _settings = settings;
        _credentials = credentials;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var saved = await _settings.GetAsync(ct);
        var activeProvider = saved.Provider is { Length: > 0 } ? saved.Provider : (_configuration["Llm:Provider"] ?? LlmRouter.Anthropic);
        var activeOllamaModel = saved.OllamaModel is { Length: > 0 } ? saved.OllamaModel : (_configuration["Ollama:Model"] ?? "qwen3:14b");
        var activeOpenAiModel = saved.OpenAiModel is { Length: > 0 } ? saved.OpenAiModel : (_configuration["OpenAI:Model"] ?? "gpt-4o");
        // Unlike activeProvider above, this has no config-file default — null/"" genuinely
        // means "disabled" (PDF uploads stay on PdfPig's plain-text extraction only), never
        // silently inherited from Llm:Provider.
        var activeDocumentReaderProvider = saved.DocumentReaderProvider is { Length: > 0 } ? saved.DocumentReaderProvider : null;
        // Each falls back to the same config default its dashboard-building counterpart
        // above uses — never to that counterpart's own saved choice, keeping this genuinely
        // a separate setting rather than one that silently mirrors the other.
        var activeDocumentReaderOllamaModel = saved.DocumentReaderOllamaModel is { Length: > 0 }
            ? saved.DocumentReaderOllamaModel : (_configuration["Ollama:Model"] ?? "qwen3:14b");
        var activeDocumentReaderOpenAiModel = saved.DocumentReaderOpenAiModel is { Length: > 0 }
            ? saved.DocumentReaderOpenAiModel : (_configuration["OpenAI:Model"] ?? "gpt-4o");
        // Unlike the document reader above, null/"" here means "same as activeProvider" — see
        // LlmSettingsStore.GetAsync's remarks — so this is the one field of the three that is
        // NOT independent of activeProvider; it mirrors it until an admin picks something else.
        var activeImageReaderProvider = saved.ImageReaderProvider is { Length: > 0 } ? saved.ImageReaderProvider : null;
        var activeImageReaderOllamaModel = saved.ImageReaderOllamaModel is { Length: > 0 }
            ? saved.ImageReaderOllamaModel : (_configuration["Ollama:Model"] ?? "qwen3:14b");
        var activeImageReaderOpenAiModel = saved.ImageReaderOpenAiModel is { Length: > 0 }
            ? saved.ImageReaderOpenAiModel : (_configuration["OpenAI:Model"] ?? "gpt-4o");

        // Same "disabled means genuinely disabled, no fallback" contract as the document
        // reader — see UpdateVisualIdentityReaderRequest's remarks.
        var visualIdentity = await _settings.GetVisualIdentityReaderAsync(ct);
        var activeVisualIdentityProvider = visualIdentity.Provider is { Length: > 0 } ? visualIdentity.Provider : null;
        var activeVisualIdentityOllamaModel = visualIdentity.OllamaModel is { Length: > 0 }
            ? visualIdentity.OllamaModel : (_configuration["Ollama:Model"] ?? "qwen3:14b");
        var activeVisualIdentityOpenAiModel = visualIdentity.OpenAiModel is { Length: > 0 }
            ? visualIdentity.OpenAiModel : (_configuration["OpenAI:Model"] ?? "gpt-4o");
        var activeVisualIdentitySupportsImage = visualIdentity.SupportsImage;

        // موديل تحليل معاني الأعمدة: null = نفس موديل اللوحات، "none" = معطَّل، وإلا مزوّد مستقل.
        var semantics = await _settings.GetSemanticsAsync(ct);
        var activeSemanticsProvider = semantics.Provider is { Length: > 0 } ? semantics.Provider : null;
        var activeSemanticsOllamaModel = semantics.OllamaModel is { Length: > 0 }
            ? semantics.OllamaModel : (_configuration["Ollama:Model"] ?? "qwen3:14b");
        var activeSemanticsOpenAiModel = semantics.OpenAiModel is { Length: > 0 }
            ? semantics.OpenAiModel : (_configuration["OpenAI:Model"] ?? "gpt-4o");

        return Ok(new
        {
            activeSemanticsProvider,
            activeSemanticsOllamaModel,
            activeSemanticsOpenAiModel,
            activeProvider,
            activeOllamaModel,
            activeOpenAiModel,
            activeDocumentReaderProvider,
            activeDocumentReaderOllamaModel,
            activeDocumentReaderOpenAiModel,
            activeImageReaderProvider,
            activeImageReaderOllamaModel,
            activeImageReaderOpenAiModel,
            activeVisualIdentityProvider,
            activeVisualIdentityOllamaModel,
            activeVisualIdentityOpenAiModel,
            activeVisualIdentitySupportsImage,
            providers = new[]
            {
                new { id = LlmRouter.Anthropic, label = "Claude (Anthropic)", configured = await IsConfiguredAsync(LlmRouter.Anthropic, ct) },
                new { id = LlmRouter.OpenAI, label = "GPT (OpenAI)", configured = await IsConfiguredAsync(LlmRouter.OpenAI, ct) },
                new { id = LlmRouter.Ollama, label = "الموديل الداخلي (Ollama)", configured = await IsConfiguredAsync(LlmRouter.Ollama, ct) },
            },
        });
    }

    /// <summary>
    /// Live model list from the gateway's own GET /models, per its Connection Guide
    /// ("read this list at runtime rather than hard-coding it"). Falls back to the
    /// known-at-build-time list if the gateway can't be reached right now.
    /// </summary>
    [HttpGet("ollama-models")]
    public async Task<IActionResult> OllamaModels(CancellationToken ct)
    {
        var credential = await _credentials.GetAsync(LlmRouter.Ollama, ct);
        var apiKey = credential.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
            return Ok(new { models = FallbackOllamaModels, live = false, note = "لم يُضبط مفتاح الموديل الداخلي بعد — أدخله من «مفاتيح الموديلات». القائمة أدناه غير محدّثة." });

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(8);
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(await _credentials.BaseUriAsync(LlmRouter.Ollama, ct), "models"));
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            using var response = await client.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) throw new HttpRequestException($"status {(int)response.StatusCode}");

            var body = await response.Content.ReadAsStringAsync(ct);
            var names = ExtractModelNames(body, "models", "data", "name", "model");
            if (names.Count == 0) throw new InvalidOperationException("unrecognized response shape");
            return Ok(new { models = names, live = true, note = (string?)null });
        }
        catch (Exception ex)
        {
            return Ok(new { models = FallbackOllamaModels, live = false, note = $"تعذّر الوصول للموديل الداخلي الآن ({ex.Message}) — القائمة أدناه من آخر مرة كانت متاحة." });
        }
    }

    /// <summary>
    /// A curated list from appsettings (OpenAI:AvailableModels) rather than OpenAI's raw
    /// account-wide GET /v1/models — that endpoint returns every model on the account
    /// (embeddings, audio, image, dozens of chat variants), which would make the switcher
    /// noisier, not more useful. This is where a custom/internal model name reached through
    /// this same OpenAI key/base URL (not a separate gateway) belongs — just add it here.
    /// </summary>
    [HttpGet("openai-models")]
    public IActionResult OpenAiModels()
    {
        var models = _configuration.GetSection("OpenAI:AvailableModels").Get<string[]>()
            is { Length: > 0 } configured ? configured : new[] { _configuration["OpenAI:Model"] ?? "gpt-4o" };
        return Ok(new { models, live = false, note = (string?)null });
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateLlmSettingsRequest request, CancellationToken ct)
    {
        if (!LlmRouter.KnownProviders.Contains(request.Provider, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { error = "مزوّد غير معروف." });

        await _settings.SetAsync(request.Provider, request.OllamaModel, request.OpenAiModel, ct);
        return NoContent();
    }

    /// <summary>Which model reads a PDF's page images at upload time — independent of Update
    /// above (the dashboard-building provider). null/"" disables it (PdfPig-only, the
    /// default) rather than falling back to any provider.</summary>
    [HttpPut("document-reader")]
    public async Task<IActionResult> UpdateDocumentReader([FromBody] UpdateDocumentReaderRequest request, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(request.Provider) &&
            !LlmRouter.KnownProviders.Contains(request.Provider, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { error = "مزوّد غير معروف." });

        await _settings.SetDocumentReaderAsync(request.Provider, request.OllamaModel, request.OpenAiModel, ct);
        return NoContent();
    }

    /// <summary>Which model handles a request that attaches a reference image (screenshot-to-
    /// dashboard) — independent of Update above. null/"" falls back to the dashboard-building
    /// provider (see LlmRouter.GenerateDashboardAsync), not to "disabled" — unlike the
    /// document reader, there is no PdfPig-style fallback for an image question.</summary>
    [HttpPut("image-reader")]
    public async Task<IActionResult> UpdateImageReader([FromBody] UpdateImageReaderRequest request, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(request.Provider) &&
            !LlmRouter.KnownProviders.Contains(request.Provider, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { error = "مزوّد غير معروف." });

        await _settings.SetImageReaderAsync(request.Provider, request.OllamaModel, request.OpenAiModel, ct);
        return NoContent();
    }

    /// <summary>Which model (if any) reads a logo/brand image with no accompanying text for
    /// External Integrations' visual-identity extraction — independent of every other setting
    /// on this controller. SupportsImage is stored exactly as sent, never inferred.</summary>
    [HttpPut("visual-identity-reader")]
    public async Task<IActionResult> UpdateVisualIdentityReader([FromBody] UpdateVisualIdentityReaderRequest request, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(request.Provider) &&
            !LlmRouter.KnownProviders.Contains(request.Provider, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { error = "مزوّد غير معروف." });

        await _settings.SetVisualIdentityReaderAsync(
            request.Provider, request.OllamaModel, request.OpenAiModel, request.SupportsImage, ct);
        return NoContent();
    }

    /// <summary>موديل تحليل معاني الأعمدة (طبقة المعنى) — مستقل عن كل ما سبق. null/"" = نفس موديل
    /// اللوحات، "none" = معطَّل (تبقى تخمينات الأسماء فقط).</summary>
    [HttpPut("semantics-analyzer")]
    public async Task<IActionResult> UpdateSemanticsAnalyzer([FromBody] UpdateSemanticsAnalyzerRequest request, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(request.Provider) &&
            !string.Equals(request.Provider, LlmSettingsStore.SemanticsDisabled, StringComparison.OrdinalIgnoreCase) &&
            !LlmRouter.KnownProviders.Contains(request.Provider, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { error = "مزوّد غير معروف." });

        await _settings.SetSemanticsAsync(request.Provider, request.OllamaModel, request.OpenAiModel, ct);
        return NoContent();
    }

    // ---------- مفاتيح الموديلات (صفحة مستقلة) ----------

    /// <summary>حالة مفتاح وعنوان كل مزوّد بلا كشف المفتاح (آخر 4 أحرف فقط) ومصدر كل حقل:
    /// db (من الصفحة) / config (appsettings أو user-secrets أو البيئة) / none / unreadable.</summary>
    [HttpGet("credentials")]
    public async Task<IActionResult> Credentials(CancellationToken ct)
    {
        var list = new List<object>();
        foreach (var provider in LlmRouter.KnownProviders)
        {
            var c = await _credentials.GetAsync(provider, ct);
            list.Add(new
            {
                provider,
                label = ProviderLabel(provider),
                hasKey = c.ApiKey is { Length: > 0 },
                keyHint = LlmCredentialStore.Hint(c.ApiKey),
                keySource = c.KeySource,
                baseUrl = c.BaseUrl,
                baseUrlSource = c.BaseUrlSource,
                defaultBaseUrl = LlmCredentialStore.DefaultBaseUrl(provider),
                updatedAt = c.UpdatedAt,
            });
        }
        return Ok(new { providers = list });
    }

    /// <summary>يحفظ مفتاح المزوّد و/أو عنوانه في قاعدة البيانات (مشفّرًا) ويسري فورًا على النداء التالي.</summary>
    [HttpPut("credentials/{provider}")]
    public async Task<IActionResult> SetCredential(string provider, [FromBody] UpdateCredentialRequest request, CancellationToken ct)
    {
        if (!LlmRouter.KnownProviders.Contains(provider, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { error = "مزوّد غير معروف." });
        try { await _credentials.SetAsync(provider, request.ApiKey, request.BaseUrl, ct); }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        return NoContent();
    }

    /// <summary>يحذف ما حُفظ من الصفحة فيعود المزوّد إلى الإعدادات (إن وُجدت) أو «غير مُعد».</summary>
    [HttpDelete("credentials/{provider}")]
    public async Task<IActionResult> ClearCredential(string provider, CancellationToken ct)
    {
        if (!LlmRouter.KnownProviders.Contains(provider, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { error = "مزوّد غير معروف." });
        await _credentials.ClearAsync(provider, ct);
        return NoContent();
    }

    /// <summary>اختبار اتصال خفيف بالمفتاح والعنوان الفعليين (قائمة الموديلات) — يعيد ok أو سبب الفشل بلا رمي.</summary>
    [HttpPost("credentials/{provider}/test")]
    public async Task<IActionResult> TestCredential(string provider, CancellationToken ct)
    {
        if (!LlmRouter.KnownProviders.Contains(provider, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { error = "مزوّد غير معروف." });
        provider = LlmRouter.KnownProviders.First(p => string.Equals(p, provider, StringComparison.OrdinalIgnoreCase));
        var c = await _credentials.GetAsync(provider, ct);
        if (c.ApiKey is not { Length: > 0 })
            return Ok(new { ok = false, message = "لا يوجد مفتاح مضبوط لهذا المزوّد." });
        var clock = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(12);
            var baseUri = await _credentials.BaseUriAsync(provider, ct);
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUri, provider == LlmRouter.Ollama ? "models" : "v1/models"));
            if (provider == LlmRouter.Anthropic)
            {
                request.Headers.Add("x-api-key", c.ApiKey);
                request.Headers.Add("anthropic-version", "2023-06-01");
            }
            else request.Headers.Add("Authorization", $"Bearer {c.ApiKey}");
            using var response = await client.SendAsync(request, ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
                return Ok(new { ok = false, message = $"الخادم ردّ بالحالة {(int)response.StatusCode}: {Trim(body)}", ms = clock.ElapsedMilliseconds });
            var names = ExtractModelNames(body, "models", "data", "name", "id");
            if (names.Count == 0) names = ExtractModelNames(body, "models", "data", "id", "model");
            return Ok(new { ok = true, message = names.Count > 0 ? $"متصل — {names.Count} موديل متاح" : "متصل", models = names.Take(30), ms = clock.ElapsedMilliseconds });
        }
        catch (Exception ex)
        {
            var inner = ex; while (inner.InnerException is not null) inner = inner.InnerException;
            return Ok(new { ok = false, message = Trim(inner.Message), ms = clock.ElapsedMilliseconds });
        }
    }

    private static string Trim(string text) => text.Length > 300 ? text[..300] + "…" : text;

    private static string ProviderLabel(string provider) => provider switch
    {
        LlmRouter.OpenAI => "GPT (OpenAI)",
        LlmRouter.Ollama => "الموديل الداخلي (Ollama)",
        _ => "Claude (Anthropic)",
    };

    private async Task<bool> IsConfiguredAsync(string provider, CancellationToken ct) => (await _credentials.GetAsync(provider, ct)).ApiKey is { Length: > 0 };

    /// <summary>
    /// Parsed leniently since these third-party response shapes weren't confirmed against a
    /// live call from here — tries a couple of reasonable array/name-field combinations and
    /// gives up (triggering the caller's fallback list) rather than guess wrong.
    /// </summary>
    private static List<string> ExtractModelNames(string json, string arrayKeyA, string arrayKeyB, string nameKeyA, string nameKeyB)
    {
        var names = new List<string>();
        try
        {
            var node = JsonNode.Parse(json);
            var array = node?[arrayKeyA]?.AsArray() ?? node?[arrayKeyB]?.AsArray() ?? node?.AsArray();
            if (array is null) return names;
            foreach (var item in array)
            {
                var name = item?[nameKeyA]?.GetValue<string>() ?? item?[nameKeyB]?.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(name)) names.Add(name);
            }
        }
        catch (JsonException) { /* fall through to empty -> caller falls back */ }
        return names;
    }
}
