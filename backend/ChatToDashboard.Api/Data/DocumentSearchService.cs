using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

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

    /// <summary>
    /// Best-effort, zero-dependency PDF text extraction: inflates FlateDecode content
    /// streams and pulls the string operands out of text-showing operators. Good enough
    /// for simple text PDFs; swap in a real PDF library (e.g. PdfPig) or a proper
    /// embeddings pipeline for production RAG.
    /// </summary>
    private static string ExtractPdfText(string file)
    {
        var bytes = File.ReadAllBytes(file);
        var raw = Encoding.Latin1.GetString(bytes);
        var sb = new StringBuilder();

        foreach (Match m in Regex.Matches(raw, @"stream\r?\n", RegexOptions.None))
        {
            var start = m.Index + m.Length;
            var end = raw.IndexOf("endstream", start, StringComparison.Ordinal);
            if (end < 0) continue;

            var streamBytes = bytes.AsSpan(start, end - start).ToArray();
            string content;
            try
            {
                using var input = new MemoryStream(streamBytes);
                using var zlib = new ZLibStream(input, CompressionMode.Decompress);
                using var reader = new StreamReader(zlib, Encoding.Latin1);
                content = reader.ReadToEnd();
            }
            catch (InvalidDataException)
            {
                content = Encoding.Latin1.GetString(streamBytes);
            }

            // Text-showing operators: (string) Tj, (string) ', and [(a) -120 (b)] TJ arrays.
            foreach (Match text in Regex.Matches(content, @"\(((?:[^()\\]|\\.)*)\)\s*(?:Tj|')|\[((?:[^\[\]\\]|\\.)*)\]\s*TJ"))
            {
                var value = text.Groups[1].Success
                    ? text.Groups[1].Value
                    : string.Concat(Regex.Matches(text.Groups[2].Value, @"\(((?:[^()\\]|\\.)*)\)")
                        .Select(p => p.Groups[1].Value));
                sb.Append(UnescapePdfString(value)).Append(' ');
            }
        }
        return sb.ToString();
    }

    private static string UnescapePdfString(string value) =>
        Regex.Replace(value, @"\\([nrtbf()\\]|\d{1,3})", m =>
        {
            var escape = m.Groups[1].Value;
            return escape switch
            {
                "n" => "\n",
                "r" => "\r",
                "t" => "\t",
                "b" => "\b",
                "f" => "\f",
                "(" => "(",
                ")" => ")",
                "\\" => "\\",
                _ => ((char)Convert.ToInt32(escape, 8)).ToString(),
            };
        });

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
