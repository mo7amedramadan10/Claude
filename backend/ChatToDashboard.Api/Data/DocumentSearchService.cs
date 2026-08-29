using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace ChatToDashboard.Api.Data;

public record DocumentChunk(string SourceFile, int ChunkIndex, string Text);

public record DocumentHit(string SourceFile, int ChunkIndex, double Score, string Text);

/// <summary>
/// Lightweight search over unstructured files (.pdf, .docx) in the data folder.
/// Only active when EnableRag=true. This is a keyword-overlap scorer over text
/// chunks — a deliberately simple stand-in for an embeddings + vector store
/// pipeline, kept behind the same feature flag and tool surface so it can be
/// swapped out without touching the Claude tool-use loop.
/// </summary>
public class DocumentSearchService
{
    private const int ChunkSize = 1200;
    private const int ChunkOverlap = 200;

    private readonly IConfiguration _configuration;
    private readonly ILogger<DocumentSearchService> _logger;
    private readonly object _lock = new();
    private List<DocumentChunk> _chunks = new();

    public DocumentSearchService(IConfiguration configuration, ILogger<DocumentSearchService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public bool Enabled => _configuration.GetValue<bool>("EnableRag");

    public int IndexedChunkCount { get { lock (_lock) return _chunks.Count; } }

    public void Reindex(string dataFolderPath)
    {
        if (!Enabled) return;

        var chunks = new List<DocumentChunk>();
        foreach (var file in Directory.EnumerateFiles(dataFolderPath))
        {
            try
            {
                var text = Path.GetExtension(file).ToLowerInvariant() switch
                {
                    ".pdf" => ExtractPdfText(file),
                    ".docx" => ExtractDocxText(file),
                    _ => null,
                };
                if (string.IsNullOrWhiteSpace(text)) continue;

                var fileName = Path.GetFileName(file);
                var index = 0;
                for (var start = 0; start < text.Length; start += ChunkSize - ChunkOverlap)
                {
                    var length = Math.Min(ChunkSize, text.Length - start);
                    chunks.Add(new DocumentChunk(fileName, index++, text.Substring(start, length)));
                    if (start + length >= text.Length) break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to index {File}", file);
            }
        }

        lock (_lock) _chunks = chunks;
        _logger.LogInformation("Indexed {Count} document chunks for RAG search", chunks.Count);
    }

    public IReadOnlyList<DocumentHit> Search(string query, int topK = 5)
    {
        var queryTerms = Tokenize(query);
        if (queryTerms.Count == 0) return Array.Empty<DocumentHit>();

        List<DocumentChunk> chunks;
        lock (_lock) chunks = _chunks;

        return chunks
            .Select(chunk =>
            {
                var chunkTerms = Tokenize(chunk.Text);
                var overlap = queryTerms.Count(t => chunkTerms.Contains(t));
                var score = (double)overlap / queryTerms.Count;
                return new DocumentHit(chunk.SourceFile, chunk.ChunkIndex, score, chunk.Text);
            })
            .Where(h => h.Score > 0)
            .OrderByDescending(h => h.Score)
            .Take(topK)
            .ToList();
    }

    private static HashSet<string> Tokenize(string text) =>
        Regex.Matches(text.ToLowerInvariant(), @"[\p{L}\p{Nd}]{2,}")
            .Select(m => m.Value)
            .ToHashSet();

    private static string ExtractPdfText(string file)
    {
        using var pdf = PdfDocument.Open(file);
        var sb = new StringBuilder();
        foreach (var page in pdf.GetPages())
            sb.AppendLine(page.Text);
        return sb.ToString();
    }

    private static string ExtractDocxText(string file)
    {
        using var archive = ZipFile.OpenRead(file);
        var entry = archive.GetEntry("word/document.xml");
        if (entry is null) return string.Empty;

        using var reader = new StreamReader(entry.Open());
        var xml = reader.ReadToEnd();
        var withBreaks = Regex.Replace(xml, @"</w:p>", "\n");
        return System.Net.WebUtility.HtmlDecode(Regex.Replace(withBreaks, "<[^>]+>", string.Empty));
    }
}
