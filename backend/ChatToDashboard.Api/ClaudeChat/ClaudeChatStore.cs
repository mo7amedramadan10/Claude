using System.Text.Json.Serialization;
using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.ClaudeChat;

/// <summary>محادثة مع Claude Code CLI (انظر <see cref="ClaudeCliService"/>): المعرّف هو معرّف جلسة الـ CLI نفسه
/// (`--session-id` ثم `--resume`)، والرسائل تُحفظ هنا لعرض السجل في الواجهة بعد إعادة الفتح.</summary>
public class ClaudeChatSummary
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("userId")] public string UserId { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updatedAt")] public DateTime UpdatedAt { get; set; }
    [JsonPropertyName("messageCount")] public int MessageCount { get; set; }
}

public class ClaudeChatMessage
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("chatId")] public string ChatId { get; set; } = "";
    /// <summary>user أو assistant.</summary>
    [JsonPropertyName("role")] public string Role { get; set; } = "user";
    [JsonPropertyName("text")] public string Text { get; set; } = "";
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }
    /// <summary>تكلفة الدورة بالدولار كما يعيدها الـ CLI (للردود فقط).</summary>
    [JsonPropertyName("costUsd")] public double? CostUsd { get; set; }
    [JsonPropertyName("durationMs")] public long? DurationMs { get; set; }
}

public class ClaudeChatStore
{
    private readonly DataStore _db;
    private readonly SemaphoreSlim _schemaGate = new(1, 1);
    private volatile bool _schemaReady;

    public ClaudeChatStore(DataStore db) => _db = db;

    private string Chats => _db.Provider == DbProvider.Sqlite ? "\"ClaudeChats\"" : "[staging].[ClaudeChats]";
    private string Messages => _db.Provider == DbProvider.Sqlite ? "\"ClaudeChatMessages\"" : "[staging].[ClaudeChatMessages]";

    private async Task EnsureSchemaOnceAsync(CancellationToken ct)
    {
        if (_schemaReady) return;
        await _schemaGate.WaitAsync(ct);
        try { if (!_schemaReady) { await EnsureSchemaAsync(ct); _schemaReady = true; } }
        finally { _schemaGate.Release(); }
    }

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);
        var sqlite = _db.Provider == DbProvider.Sqlite;
        var statements = sqlite
            ? new[]
            {
                $"""CREATE TABLE IF NOT EXISTS {Chats} ("Id" TEXT PRIMARY KEY, "UserId" TEXT NOT NULL, "Title" TEXT NOT NULL, "CreatedAt" TEXT NOT NULL, "UpdatedAt" TEXT NOT NULL)""",
                $"""CREATE TABLE IF NOT EXISTS {Messages} ("Id" TEXT PRIMARY KEY, "ChatId" TEXT NOT NULL, "Role" TEXT NOT NULL, "Text" TEXT NOT NULL, "CreatedAt" TEXT NOT NULL, "CostUsd" REAL, "DurationMs" INTEGER)""",
            }
            : new[]
            {
                $"""IF OBJECT_ID('staging.ClaudeChats') IS NULL CREATE TABLE {Chats} ([Id] NVARCHAR(64) PRIMARY KEY, [UserId] NVARCHAR(200) NOT NULL, [Title] NVARCHAR(300) NOT NULL, [CreatedAt] DATETIME2 NOT NULL, [UpdatedAt] DATETIME2 NOT NULL)""",
                $"""IF OBJECT_ID('staging.ClaudeChatMessages') IS NULL CREATE TABLE {Messages} ([Id] NVARCHAR(32) PRIMARY KEY, [ChatId] NVARCHAR(64) NOT NULL, [Role] NVARCHAR(16) NOT NULL, [Text] NVARCHAR(MAX) NOT NULL, [CreatedAt] DATETIME2 NOT NULL, [CostUsd] FLOAT, [DurationMs] BIGINT)""",
            };
        foreach (var text in statements)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = text;
            await command.ExecuteNonQueryAsync(ct);
        }
    }

    public async Task<ClaudeChatSummary> CreateAsync(string id, string userId, string title, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        var chat = new ClaudeChatSummary { Id = id, UserId = userId, Title = title, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"INSERT INTO {Chats} (Id, UserId, Title, CreatedAt, UpdatedAt) VALUES (@Id, @UserId, @Title, @CreatedAt, @UpdatedAt)", chat);
        return chat;
    }

    public async Task<ClaudeChatSummary?> GetAsync(string id, string userId, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<ClaudeChatSummary>(
            $"SELECT c.Id, c.UserId, c.Title, c.CreatedAt, c.UpdatedAt, (SELECT COUNT(*) FROM {Messages} m WHERE m.ChatId = c.Id) AS MessageCount FROM {Chats} c WHERE c.Id = @id AND c.UserId = @userId",
            new { id, userId });
    }

    public async Task<IReadOnlyList<ClaudeChatSummary>> ListAsync(string userId, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<ClaudeChatSummary>(
            $"SELECT c.Id, c.UserId, c.Title, c.CreatedAt, c.UpdatedAt, (SELECT COUNT(*) FROM {Messages} m WHERE m.ChatId = c.Id) AS MessageCount FROM {Chats} c WHERE c.UserId = @userId ORDER BY c.UpdatedAt DESC",
            new { userId });
        return rows.ToList();
    }

    public async Task<IReadOnlyList<ClaudeChatMessage>> MessagesAsync(string chatId, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<ClaudeChatMessage>($"SELECT Id, ChatId, Role, Text, CreatedAt, CostUsd, DurationMs FROM {Messages} WHERE ChatId = @chatId ORDER BY CreatedAt", new { chatId });
        return rows.ToList();
    }

    public async Task<ClaudeChatMessage> AddMessageAsync(string chatId, string role, string text, double? costUsd = null, long? durationMs = null, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        var m = new ClaudeChatMessage { Id = Guid.NewGuid().ToString("N")[..16], ChatId = chatId, Role = role, Text = text, CreatedAt = DateTime.UtcNow, CostUsd = costUsd, DurationMs = durationMs };
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"INSERT INTO {Messages} (Id, ChatId, Role, Text, CreatedAt, CostUsd, DurationMs) VALUES (@Id, @ChatId, @Role, @Text, @CreatedAt, @CostUsd, @DurationMs)", m);
        await connection.ExecuteAsync($"UPDATE {Chats} SET UpdatedAt = @now WHERE Id = @chatId", new { chatId, now = m.CreatedAt });
        return m;
    }

    public async Task RenameAsync(string id, string title, CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {Chats} SET Title = @title WHERE Id = @id", new { id, title });
    }

    public async Task DeleteAsync(string id, string userId, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var n = await connection.ExecuteAsync($"DELETE FROM {Chats} WHERE Id = @id AND UserId = @userId", new { id, userId });
        if (n > 0) await connection.ExecuteAsync($"DELETE FROM {Messages} WHERE ChatId = @id", new { id });
    }
}
