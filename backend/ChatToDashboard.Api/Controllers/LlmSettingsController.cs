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
    // The 4 models the gateway's Connection Guide listed as installed at the time this was
    // built — used only if the live GET /models call to the gateway fails or is unreachable
    // (e.g. no network path to it from wherever this app happens to run).
    private static readonly string[] FallbackOllamaModels =
        { "qwen3:14b", "qwen3:32b", "qwen3:30b-a3b-instruct-2507-q4_K_M", "gemma4:31b" };

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
        var (savedProvider, savedOllamaModel, savedOpenAiModel) = await _settings.GetAsync(ct);
        var activeProvider = savedProvider is { Length: > 0 } ? savedProvider : (_configuration["Llm:Provider"] ?? LlmRouter.Anthropic);
        var activeOllamaModel = savedOllamaModel is { Length: > 0 } ? savedOllamaModel : (_configuration["Ollama:Model"] ?? "qwen3:14b");
        var activeOpenAiModel = savedOpenAiModel is { Length: > 0 } ? savedOpenAiModel : (_configuration["OpenAI:Model"] ?? "gpt-4o");

        return Ok(new
        {
            activeProvider,
            activeOllamaModel,
            activeOpenAiModel,
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
