using System.Text.Json;
using ChatToDashboard.Api.Data;
using ChatToDashboard.Api.Models;
using Dapper;

namespace ChatToDashboard.Api.Inquiry;

/// <summary>
/// Persists saved الاستفسارات conversations — private to their owner, no sharing, no roles
/// (unlike DashboardHistory's Active-dashboard model). Capped at <see cref="MaxPerUser"/> per
/// user, oldest evicted first, same inline-on-insert pattern as
/// <see cref="History.HistoryStore.SaveAsync"/>.
/// </summary>
public class ConversationStore
{
    private const int MaxPerUser = 60;

    private readonly DataStore _db;

    public ConversationStore(DataStore db)
    {
        _db = db;
    }

    private string Table => _db.Provider == DbProvider.Sqlite
        ? "\"InquiryConversations\""
        : "[staging].[InquiryConversations]";

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);

        var text = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {Table} (
                 "Id" TEXT PRIMARY KEY, "UserId" TEXT, "Title" TEXT, "TranscriptJson" TEXT,
                 "Summary" TEXT, "CreatedAt" TEXT, "UpdatedAt" TEXT)
               """
            : $"""
               IF OBJECT_ID('staging.InquiryConversations') IS NULL
               CREATE TABLE {Table} (
                 [Id] NVARCHAR(64) PRIMARY KEY, [UserId] NVARCHAR(200), [Title] NVARCHAR(400),
                 [TranscriptJson] NVARCHAR(MAX), [Summary] NVARCHAR(MAX), [CreatedAt] DATETIME2,
                 [UpdatedAt] DATETIME2)
               """;

        await using var command = connection.CreateCommand();
        command.CommandText = text;
        await command.ExecuteNonQueryAsync(ct);
    }

    /// <summary>Creates a new conversation from its first turn pair (the user's question and
    /// the model's first answer), returning the saved entry with its generated Id.</summary>
    public async Task<InquiryConversationEntry> CreateAsync(
        string userId, string title, IReadOnlyList<ConversationTurn> turns, string summary, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        var entry = new InquiryConversationEntry
        {
            Id = Guid.NewGuid().ToString("N"),
            UserId = userId,
            Title = title,
            TranscriptJson = JsonSerializer.Serialize(turns),
            Summary = summary,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"INSERT INTO {Table} (Id, UserId, Title, TranscriptJson, Summary, CreatedAt, UpdatedAt) " +
            "VALUES (@Id, @UserId, @Title, @TranscriptJson, @Summary, @CreatedAt, @UpdatedAt)",
            entry);

        // Retention: keep only the latest MaxPerUser conversations for this user — same
        // inline eviction HistoryStore.SaveAsync already uses for dashboards.
        var staleIds = (await connection.QueryAsync<string>(
                $"SELECT Id FROM {Table} WHERE UserId = @UserId ORDER BY UpdatedAt DESC",
                new { entry.UserId }))
            .Skip(MaxPerUser)
            .ToList();
        if (staleIds.Count > 0)
            await connection.ExecuteAsync($"DELETE FROM {Table} WHERE Id IN @Ids", new { Ids = staleIds });

        return entry;
    }

    /// <summary>Appends one more turn pair to an existing conversation and refreshes its
    /// rolling summary + UpdatedAt (which also keeps it from being evicted first). Returns
    /// false if the conversation doesn't exist or isn't owned by <paramref name="userId"/>.</summary>
    public async Task<bool> AppendTurnsAsync(
        string userId, string id, IReadOnlyList<ConversationTurn> newTurns, string summary, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var existing = await connection.QuerySingleOrDefaultAsync<InquiryConversationEntry>(
            $"SELECT Id, UserId, Title, TranscriptJson, Summary, CreatedAt, UpdatedAt FROM {Table} WHERE Id = @id AND UserId = @userId",
            new { id, userId });
        if (existing is null) return false;

        var turns = DeserializeTurns(existing.TranscriptJson);
        turns.AddRange(newTurns);

        await connection.ExecuteAsync(
            $"UPDATE {Table} SET TranscriptJson = @transcriptJson, Summary = @summary, UpdatedAt = @updatedAt WHERE Id = @id",
            new { id, transcriptJson = JsonSerializer.Serialize(turns), summary, updatedAt = DateTime.UtcNow });
        return true;
    }

    /// <summary>Newest first, for the saved-conversations list.</summary>
    public async Task<IReadOnlyList<InquiryConversationEntry>> ListAsync(
        string userId, int limit = MaxPerUser, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var top = _db.Provider == DbProvider.Sqlite ? "" : $"TOP {limit} ";
        var tail = _db.Provider == DbProvider.Sqlite ? $" LIMIT {limit}" : "";
        var rows = await connection.QueryAsync<InquiryConversationEntry>(
            $"SELECT {top}Id, UserId, Title, TranscriptJson, Summary, CreatedAt, UpdatedAt FROM {Table} " +
            $"WHERE UserId = @userId ORDER BY UpdatedAt DESC{tail}",
            new { userId });
        return rows.ToList();
    }

    /// <summary>Fetches one conversation — only if it belongs to <paramref name="userId"/>;
    /// private, no sharing (unlike an Active dashboard).</summary>
    public async Task<InquiryConversationEntry?> GetByIdAsync(string userId, string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<InquiryConversationEntry>(
            $"SELECT Id, UserId, Title, TranscriptJson, Summary, CreatedAt, UpdatedAt FROM {Table} WHERE Id = @id AND UserId = @userId",
            new { id, userId });
    }

    public async Task<bool> DeleteAsync(string userId, string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var affected = await connection.ExecuteAsync(
            $"DELETE FROM {Table} WHERE Id = @id AND UserId = @userId", new { id, userId });
        return affected > 0;
    }

    public static List<ConversationTurn> DeserializeTurns(string transcriptJson) =>
        JsonSerializer.Deserialize<List<ConversationTurn>>(transcriptJson) ?? new List<ConversationTurn>();
}

/// <summary>One saved الاستفسارات conversation row.</summary>
public class InquiryConversationEntry
{
    public string Id { get; set; } = "";
    public string UserId { get; set; } = "";
    public string Title { get; set; } = "";

    /// <summary>JSON-serialized List&lt;ConversationTurn&gt; — the full display transcript,
    /// with every block's label/source/table/data intact (masking is applied on read, never
    /// stored — see InquiryAccessService).</summary>
    public string TranscriptJson { get; set; } = "[]";

    /// <summary>The model's own rolling summary of everything before the most recent verbatim
    /// turns (see AnalyticsTools.ComposeInquiryUserMessage) — refreshed every turn.</summary>
    public string Summary { get; set; } = "";

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
