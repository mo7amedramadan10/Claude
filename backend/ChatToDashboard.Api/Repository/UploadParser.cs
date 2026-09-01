using System.Collections.Concurrent;
using System.Data;
using System.Text;
using ChatToDashboard.Api.Data;
using UglyToad.PdfPig;

namespace ChatToDashboard.Api.Repository;

/// <summary>A file parsed on the server, held until the user assigns it a category.</summary>
public record ParsedUpload(string FileName, string Kind, DataTable? Table, string? Text, int PageCount);

/// <summary>
/// Parses uploads server-side (never in the browser): spreadsheets with ClosedXML/CsvHelper
/// and PDFs with PdfPig, then holds the result in memory until it is saved with a category.
/// </summary>
public class UploadParser
{
    private static readonly TimeSpan PendingLifetime = TimeSpan.FromHours(2);

    private readonly ConcurrentDictionary<string, (ParsedUpload Upload, DateTime At)> _pending = new();
    private readonly ILogger<UploadParser> _logger;

    public UploadParser(ILogger<UploadParser> logger) => _logger = logger;

    public static bool IsSupported(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() is ".xlsx" or ".xls" or ".csv" or ".pdf";

    public PendingUpload Parse(string fileName, Stream content)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var token = Guid.NewGuid().ToString("N");

        // ClosedXML and PdfPig both want a seekable file; buffer the upload to a temp file.
        var temp = Path.Combine(Path.GetTempPath(), token + extension);
        try
        {
            using (var file = File.Create(temp)) content.CopyTo(file);

            ParsedUpload parsed = extension switch
            {
                ".csv" => new ParsedUpload(fileName, "csv",
                    DataFolderLoader.InferTypes(DataFolderLoader.ReadCsv(temp)), null, 0),
                ".xlsx" or ".xls" => new ParsedUpload(fileName, "excel",
                    DataFolderLoader.InferTypes(DataFolderLoader.ReadXlsx(temp)), null, 0),
                ".pdf" => ParsePdf(fileName, temp),
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

    private static ParsedUpload ParsePdf(string fileName, string path)
    {
        using var pdf = PdfDocument.Open(path);
        var text = new StringBuilder();
        var pages = 0;
        foreach (var page in pdf.GetPages())
        {
            text.AppendLine(page.Text);
            pages++;
        }
        return new ParsedUpload(fileName, "pdf", null, text.ToString(), pages);
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
