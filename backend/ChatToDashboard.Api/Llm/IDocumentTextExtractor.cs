using ChatToDashboard.Api.Users;

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
    /// <param name="onPageRead">Called after each page's own transcription completes — only
    /// ever a progress signal (how many of the total pages are done so far), never anything the
    /// extraction's correctness depends on. Claude/OpenAI process the whole document in one
    /// call, so they never invoke this; only OllamaClient's real per-page loop does.</param>
    Task<DocumentExtractionResult> ExtractDocumentTextAsync(
        string fileName, IReadOnlyList<string> pageImageDataUrls, AppUser? requestingUser = null,
        Action<int, int>? onPageRead = null, CancellationToken ct = default);
}

/// <summary>
/// What one extraction attempt produced. PagesRead can be less than pageImageDataUrls.Count —
/// a partial result, some pages read before a failure or cancellation cut the rest short —
/// which the caller (UploadParser) fills in from PdfPig's own per-page text rather than ever
/// silently dropping the pages this didn't reach. Claude/OpenAI only ever return PagesRead
/// equal to the full page count (or throw) — there's no partial concept for a single bundled
/// call the way there is for OllamaClient's real per-page requests.
/// </summary>
public record DocumentExtractionResult(string Text, int PagesRead);
