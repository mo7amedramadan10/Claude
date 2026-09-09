namespace ChatToDashboard.Api.Llm;

/// <summary>
/// Reads a PDF's actual content with a vision-capable model — rendered page images in, plain
/// transcribed text out — instead of (or on top of) PdfPig's plain-text-layer extraction, so a
/// scanned page, an embedded chart, or a complex table gets read the way a person would read
/// it rather than coming back empty or mangled. Deliberately its own interface, not folded
/// into IDashboardGenerator: which model reads documents is a completely separate setting from
/// which model builds dashboards (see LlmSettingsStore.DocumentReaderProvider) — an admin can
/// point this at the cheaper/internal model while dashboard-building stays on the external one.
/// </summary>
public interface IDocumentTextExtractor
{
    /// <param name="fileName">Shown to the model only for context in its instructions — never
    /// affects the extracted content itself.</param>
    /// <param name="pageImageDataUrls">One "data:image/png;base64,..." URL per page, in
    /// document order — the same data-URL shape ClaudeClient/OpenAiClient/OllamaClient already
    /// accept for a reference-image attachment.</param>
    Task<string> ExtractDocumentTextAsync(
        string fileName, IReadOnlyList<string> pageImageDataUrls, CancellationToken ct = default);
}
