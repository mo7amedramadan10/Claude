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
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public LlmSettingsController(LlmSettingsStore settings, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _settings = settings;
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

        return Ok(new
        {
            activeProvider,
            activeOllamaModel,
            activeOpenAiModel,
            activeDocumentReaderProvider,
            activeDocumentReaderOllamaModel,
            activeDocumentReaderOpenAiModel,
            activeImageReaderProvider,
            activeImageReaderOllamaModel,
            activeImageReaderOpenAiModel,
            providers = new[]
            {
                new { id = LlmRouter.Anthropic, label = "Claude (Anthropic)", configured = IsConfigured("Anthropic:ApiKey") },
                new { id = LlmRouter.OpenAI, label = "GPT (OpenAI)", configured = IsConfigured("OpenAI:ApiKey") },
                new { id = LlmRouter.Ollama, label = "الموديل الداخلي (Ollama)", configured = IsConfigured("Ollama:ApiKey") },
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
        var apiKey = _configuration["Ollama:ApiKey"]?.Trim();
        var baseUrl = _configuration["Ollama:BaseUrl"] ?? "http://172.17.242.1:8081/api/v1/";
        if (string.IsNullOrWhiteSpace(apiKey))
            return Ok(new { models = FallbackOllamaModels, live = false, note = "لم يتم إعداد Ollama:ApiKey — القائمة أدناه غير محدّثة." });

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(8);
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(new Uri(baseUrl), "models"));
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

    private bool IsConfigured(string key) => _configuration[key]?.Trim() is { Length: > 0 };

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
