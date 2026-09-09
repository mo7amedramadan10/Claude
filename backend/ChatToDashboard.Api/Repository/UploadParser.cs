using System.Collections.Concurrent;
using System.Data;
using System.Text;
using ChatToDashboard.Api.Data;
using ChatToDashboard.Api.Llm;
using SkiaSharp;
using UglyToad.PdfPig;

namespace ChatToDashboard.Api.Repository;

/// <summary>A file parsed on the server, held until the user assigns it a category.
/// <paramref name="Content"/> is the raw upload exactly as received — kept alongside the
/// parsed result purely so RepositoryStore can persist it for GET .../download, and never
/// otherwise touched here.</summary>
public record ParsedUpload(string FileName, string Kind, DataTable? Table, string? Text, int PageCount, byte[] Content);

/// <summary>
/// Parses uploads server-side (never in the browser): spreadsheets with ClosedXML/CsvHelper
/// and PDFs with PdfPig, then holds the result in memory until it is saved with a category.
/// </summary>
public class UploadParser
{
    private static readonly TimeSpan PendingLifetime = TimeSpan.FromHours(2);

    private readonly ConcurrentDictionary<string, (ParsedUpload Upload, DateTime At)> _pending = new();
    private readonly IDocumentReaderRouter _documentReader;
    private readonly ILogger<UploadParser> _logger;

    public UploadParser(IDocumentReaderRouter documentReader, ILogger<UploadParser> logger)
    {
        _documentReader = documentReader;
        _logger = logger;
    }

    public static bool IsSupported(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() is ".xlsx" or ".xls" or ".csv" or ".pdf";

    public async Task<PendingUpload> ParseAsync(string fileName, Stream content, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var token = Guid.NewGuid().ToString("N");

        // ClosedXML and PdfPig both want a seekable file; buffer the upload to a temp file.
        // The same bytes are kept in memory too (ParsedUpload.Content) so a later download
        // request can hand back the exact original file, not a reconstruction from parsed data.
        var temp = Path.Combine(Path.GetTempPath(), token + extension);
        try
        {
            byte[] bytes;
            using (var buffer = new MemoryStream()) { content.CopyTo(buffer); bytes = buffer.ToArray(); }
            File.WriteAllBytes(temp, bytes);

            ParsedUpload parsed = extension switch
            {
                ".csv" => new ParsedUpload(fileName, "csv",
                    DataFolderLoader.InferTypes(DataFolderLoader.ReadCsv(temp)), null, 0, bytes),
                ".xlsx" or ".xls" => new ParsedUpload(fileName, "excel",
                    DataFolderLoader.InferTypes(DataFolderLoader.ReadXlsx(temp)), null, 0, bytes),
                ".pdf" => await ParsePdfAsync(fileName, temp, bytes, ct),
                _ => throw new NotSupportedException($"Unsupported file type: {extension}"),
            };

            _pending[token] = (parsed, DateTime.UtcNow);
            Prune();

            return new PendingUpload
            {
                Token = token,
                Name = fileName,
                Kind = parsed.Kind,
                RowCount = parsed.Table?.Rows.Count ?? 0,
                ColumnCount = parsed.Table?.Columns.Count ?? 0,
                PageCount = parsed.PageCount,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse upload {File}", fileName);
            return new PendingUpload { Token = token, Name = fileName, Kind = "unknown", Error = ex.Message };
        }
        finally
        {
            try { File.Delete(temp); } catch (IOException) { /* temp file cleanup is best effort */ }
        }
    }

    private async Task<ParsedUpload> ParsePdfAsync(string fileName, string path, byte[] bytes, CancellationToken ct)
    {
        using var pdf = PdfDocument.Open(path);
        var text = new StringBuilder();
        var pages = 0;
        foreach (var page in pdf.GetPages())
        {
            text.AppendLine(page.Text);
            pages++;
        }
        var pdfPigText = text.ToString();

        // Per LlmSettingsStore.DocumentReaderProvider: when a document-reading model is
        // configured, its transcription of the rendered pages REPLACES PdfPig's text-layer
        // extraction outright (not appended alongside it) — it's meant to be strictly better,
        // reading a scanned page or an embedded table/chart PdfPig can't. Any failure along
        // the way (rasterization, the model call itself, an internal model with no vision
        // support) falls back to the PdfPig text already in hand rather than failing the
        // whole upload — a document search that's merely as good as before beats one that's
        // suddenly empty because of an unrelated setting.
        var aiText = await TryExtractWithAiAsync(fileName, bytes, pages, ct);
        return new ParsedUpload(fileName, "pdf", null, aiText ?? pdfPigText, pages, bytes);
    }

    private async Task<string?> TryExtractWithAiAsync(string fileName, byte[] pdfBytes, int pageCount, CancellationToken ct)
    {
        if (pageCount == 0 || !await _documentReader.IsEnabledAsync(ct)) return null;
        try
        {
            var pageImages = new List<string>();
            // CA1416 (platform-support analyzer): PDFtoImage lists Windows/Linux/macOS among
            // its supported platforms — every OS this app actually deploys to — the warning
            // just doesn't disappear without this project itself declaring a supported-OS
            // list, which it doesn't do anywhere else either.
#pragma warning disable CA1416
            await foreach (var bitmap in PDFtoImage.Conversion.ToImagesAsync(pdfBytes, password: null, cancellationToken: ct))
#pragma warning restore CA1416
            {
                using (bitmap)
                {
                    using var encoded = bitmap.Encode(SKEncodedImageFormat.Png, 90);
                    pageImages.Add("data:image/png;base64," + Convert.ToBase64String(encoded.ToArray()));
                }
            }
            if (pageImages.Count == 0) return null;

            var extracted = await _documentReader.ExtractTextAsync(fileName, pageImages, ct);
            return string.IsNullOrWhiteSpace(extracted) ? null : extracted;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "AI document extraction failed for {File} — keeping PdfPig's plain-text extraction instead.", fileName);
            return null;
        }
    }

    public bool TryTake(string token, out ParsedUpload upload)
    {
        if (_pending.TryRemove(token, out var entry))
        {
            upload = entry.Upload;
            return true;
        }
        upload = default!;
        return false;
    }

    public void Discard(string token) => _pending.TryRemove(token, out _);

    private void Prune()
    {
        var cutoff = DateTime.UtcNow - PendingLifetime;
        foreach (var (token, entry) in _pending)
            if (entry.At < cutoff) _pending.TryRemove(token, out _);
    }
}
