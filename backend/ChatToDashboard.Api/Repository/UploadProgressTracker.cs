using System.Collections.Concurrent;

namespace ChatToDashboard.Api.Repository;

/// <summary>One PDF's read-so-far state, as last reported by <see cref="UploadParser"/>.</summary>
public record UploadProgress(string FileName, int PagesRead, int TotalPages);

/// <summary>
/// In-memory progress counters for an in-flight upload, keyed by a token the browser generates
/// before it starts the upload POST and polls (GET /api/repository/upload-progress/{token})
/// while that same POST is still in flight — a second connection reading state the first one is
/// updating, nothing more. Deliberately not background/async processing: the token's lifetime is
/// tied to that one synchronous request; if it's aborted, UploadParser stops updating this and
/// the entry simply goes stale until pruned — nothing here keeps extraction running once the
/// request that started it is gone.
/// </summary>
public class UploadProgressTracker
{
    private static readonly TimeSpan EntryLifetime = TimeSpan.FromMinutes(30);

    private readonly ConcurrentDictionary<string, (UploadProgress Progress, DateTime At)> _progress = new();

    public void Report(string token, string fileName, int pagesRead, int totalPages)
    {
        if (string.IsNullOrWhiteSpace(token)) return;
        _progress[token] = (new UploadProgress(fileName, pagesRead, totalPages), DateTime.UtcNow);
        Prune();
    }

    public UploadProgress? Get(string token) =>
        _progress.TryGetValue(token, out var entry) ? entry.Progress : null;

    public void Clear(string token) => _progress.TryRemove(token, out _);

    private void Prune()
    {
        var cutoff = DateTime.UtcNow - EntryLifetime;
        foreach (var (token, entry) in _progress)
            if (entry.At < cutoff) _progress.TryRemove(token, out _);
    }
}
