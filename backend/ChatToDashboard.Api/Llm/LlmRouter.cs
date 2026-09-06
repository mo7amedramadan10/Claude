using ChatToDashboard.Api.Claude;
using ChatToDashboard.Api.Models;
using ChatToDashboard.Api.Ollama;
using ChatToDashboard.Api.OpenAi;
using ChatToDashboard.Api.Sources;
using Microsoft.Extensions.DependencyInjection;

namespace ChatToDashboard.Api.Llm;

/// <summary>
/// The single <see cref="IDashboardGenerator"/> registered with DI — every question goes
/// through this. It doesn't call any provider itself; it resolves whichever concrete client
/// (Claude, GPT, or the internal Ollama gateway) is currently active and delegates to it.
///
/// The active provider is read fresh on every call from <see cref="LlmSettingsStore"/> (an
/// admin-settable override) or, if none has ever been saved, from the Llm:Provider config
/// value — so switching from the dashboard's model selector takes effect immediately, no
/// restart needed. Resolving the concrete client lazily (rather than holding all three built
/// up front) means a provider whose API key isn't configured only fails if it's actually
/// selected — the other two keep working regardless.
/// </summary>
public class LlmRouter : IDashboardGenerator
{
    public const string Anthropic = "Anthropic";
    public const string OpenAI = "OpenAI";
    public const string Ollama = "Ollama";
    public static readonly IReadOnlyList<string> KnownProviders = new[] { Anthropic, OpenAI, Ollama };

    private readonly IServiceProvider _services;
    private readonly LlmSettingsStore _settings;
    private readonly string _defaultProvider;

    public LlmRouter(IServiceProvider services, LlmSettingsStore settings, IConfiguration configuration)
    {
        _services = services;
        _settings = settings;
        _defaultProvider = configuration["Llm:Provider"] ?? Anthropic;
    }

    public async Task<DashboardSpec> GenerateDashboardAsync(
        string question,
        DashboardStateInput? currentDashboard = null,
        SourceSelection? sources = null,
        string? imageDataUrl = null,
        CancellationToken ct = default)
    {
        var (savedProvider, _, _) = await _settings.GetAsync(ct);
        var provider = savedProvider is { Length: > 0 } ? savedProvider : _defaultProvider;

        IDashboardGenerator generator = provider switch
        {
            OpenAI => _services.GetRequiredService<OpenAiClient>(),
            Ollama => _services.GetRequiredService<OllamaClient>(),
            _ => _services.GetRequiredService<ClaudeClient>(),
        };
        return await generator.GenerateDashboardAsync(question, currentDashboard, sources, imageDataUrl, ct);
    }
}
