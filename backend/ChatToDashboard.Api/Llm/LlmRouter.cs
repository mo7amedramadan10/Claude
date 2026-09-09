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
        var generator = await ResolveAsync(ct);
        return await generator.GenerateDashboardAsync(question, currentDashboard, sources, imageDataUrl, ct);
    }

    public async Task<InquiryResponse> GenerateInquiryAsync(
        string question, SourceSelection? sources = null, CancellationToken ct = default)
    {
        var generator = await ResolveAsync(ct);
        return await generator.GenerateInquiryAsync(question, sources, ct);
    }

    public async Task<DashboardSpec> GenerateDashboardFromInquiryAsync(
        InquiryResponse inquiry, DashboardStateInput? currentDashboard = null, SourceSelection? sources = null, CancellationToken ct = default)
    {
        var generator = await ResolveAsync(ct);
        return await generator.GenerateDashboardFromInquiryAsync(inquiry, currentDashboard, sources, ct);
    }

    private async Task<IDashboardGenerator> ResolveAsync(CancellationToken ct)
    {
        var (savedProvider, _, _, _) = await _settings.GetAsync(ct);
        var provider = savedProvider is { Length: > 0 } ? savedProvider : _defaultProvider;

        return provider switch
        {
            OpenAI => _services.GetRequiredService<OpenAiClient>(),
            Ollama => _services.GetRequiredService<OllamaClient>(),
            _ => _services.GetRequiredService<ClaudeClient>(),
        };
    }
}

/// <summary>The IDashboardGenerator equivalent of this router, for the document-reading side —
/// its own interface so the repository upload flow (UploadParser) can depend on this instead
/// of the concrete DocumentReaderRouter, the same indirection IDashboardGenerator/LlmRouter
/// already use elsewhere.</summary>
public interface IDocumentReaderRouter
{
    Task<bool> IsEnabledAsync(CancellationToken ct = default);

    Task<string?> ExtractTextAsync(
        string fileName, IReadOnlyList<string> pageImageDataUrls, CancellationToken ct = default);
}

/// <summary>
/// The IDocumentTextExtractor equivalent of LlmRouter above: resolves whichever concrete
/// client is currently set as LlmSettingsStore.DocumentReaderProvider — a completely
/// independent choice from LlmRouter's own provider, so an admin can read documents with one
/// model and build dashboards with another. Null/unset means "disabled": ExtractTextAsync
/// returns null rather than falling back to some default provider, since silently spending
/// on a provider nobody explicitly picked for this purpose would be the wrong failure mode —
/// the repository upload flow falls back to PdfPig's plain-text extraction in that case.
/// </summary>
public class DocumentReaderRouter : IDocumentReaderRouter
{
    private readonly IServiceProvider _services;
    private readonly LlmSettingsStore _settings;

    public DocumentReaderRouter(IServiceProvider services, LlmSettingsStore settings)
    {
        _services = services;
        _settings = settings;
    }

    /// <summary>Cheap check the repository upload flow makes before doing any of the
    /// (comparatively expensive) PDF page rasterization — no point rendering pages just to
    /// find out afterward that nothing is configured to read them.</summary>
    public async Task<bool> IsEnabledAsync(CancellationToken ct = default)
    {
        var (_, _, _, documentReaderProvider) = await _settings.GetAsync(ct);
        return !string.IsNullOrWhiteSpace(documentReaderProvider);
    }

    public async Task<string?> ExtractTextAsync(
        string fileName, IReadOnlyList<string> pageImageDataUrls, CancellationToken ct = default)
    {
        var (_, _, _, documentReaderProvider) = await _settings.GetAsync(ct);
        if (string.IsNullOrWhiteSpace(documentReaderProvider)) return null;

        IDocumentTextExtractor extractor = documentReaderProvider switch
        {
            LlmRouter.OpenAI => _services.GetRequiredService<OpenAiClient>(),
            LlmRouter.Ollama => _services.GetRequiredService<OllamaClient>(),
            LlmRouter.Anthropic => _services.GetRequiredService<ClaudeClient>(),
            _ => throw new InvalidOperationException($"Unknown DocumentReaderProvider '{documentReaderProvider}'."),
        };
        return await extractor.ExtractDocumentTextAsync(fileName, pageImageDataUrls, ct);
    }
}
