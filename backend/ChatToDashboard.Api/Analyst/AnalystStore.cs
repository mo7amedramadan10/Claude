using System.Text.Json;
using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Analyst;

/// <summary>
/// Persists everything "المحلل الذكي" needs — same shared-DB/Dapper/[staging] pattern as every
/// other store in this app (see TemplateStore, Inquiry.ConversationStore), not a separate
/// database per organization (the reference design's own sidebar copy about an "isolated
/// database" is aspirational mockup text, not this app's real multi-tenancy — see
/// Organizations.OrganizationStore's own remarks, which this phase's own instructions ("use the
/// current context mechanism") point back to). Every table carries ProjectId (and reaches
/// OrganizationId transitively through Project), same scoping every other project-level store
/// already uses.
///
/// Four tables so far (AnalystConversations/Messages/Results/ResultSources) — Phase 1's own
/// scope. AnalystAnalyses (Phase 5) and AnalystReports (Phase 6) are deliberately NOT created
/// here yet, to avoid carrying schema for stores/logic that don't exist until their own phase.
/// </summary>
public class AnalystStore
{
    private readonly DataStore _db;

    public AnalystStore(DataStore db) => _db = db;

    private string ConversationsTable => "[staging].[AnalystConversations]";
    private string MessagesTable => "[staging].[AnalystMessages]";
    private string ResultsTable => "[staging].[AnalystResults]";
    private string ResultSourcesTable => "[staging].[AnalystResultSources]";

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);

        var statements = new[]
            {
                $"""
                 IF OBJECT_ID('staging.AnalystConversations') IS NULL
                 CREATE TABLE {ConversationsTable} (
                   [Id] NVARCHAR(64) PRIMARY KEY, [ProjectId] NVARCHAR(64), [OwnerUserId] NVARCHAR(64), [Title] NVARCHAR(400),
                   [IsSaved] BIT, [RollingSummary] NVARCHAR(MAX), [FindingsLedgerJson] NVARCHAR(MAX),
                   [CreatedAt] DATETIME2, [UpdatedAt] DATETIME2)
                 """,
                $"""
                 IF OBJECT_ID('staging.AnalystMessages') IS NULL
                 CREATE TABLE {MessagesTable} (
                   [Id] NVARCHAR(64) PRIMARY KEY, [ConversationId] NVARCHAR(64), [Role] NVARCHAR(16), [Text] NVARCHAR(MAX),
                   [TemplateText] NVARCHAR(MAX), [ResultId] NVARCHAR(64), [KnowledgeScope] NVARCHAR(16), [CreatedAt] DATETIME2)
                 """,
                $"""
                 IF OBJECT_ID('staging.AnalystResults') IS NULL
                 CREATE TABLE {ResultsTable} (
                   [Id] NVARCHAR(64) PRIMARY KEY, [ConversationId] NVARCHAR(64), [ProjectId] NVARCHAR(64), [Title] NVARCHAR(400),
                   [ResultType] NVARCHAR(16), [ColumnsJson] NVARCHAR(MAX), [RowsJson] NVARCHAR(MAX), [KeyColumn] NVARCHAR(200),
                   [PrimaryMeasure] NVARCHAR(200), [ExecutedSql] NVARCHAR(MAX), [AuditSql] NVARCHAR(MAX), [VerificationJson] NVARCHAR(MAX),
                   [DataVersion] INT, [IsTruncated] BIT, [CreatedAt] DATETIME2)
                 """,
                $"""
                 IF OBJECT_ID('staging.AnalystResultSources') IS NULL
                 CREATE TABLE {ResultSourcesTable} (
                   [Id] NVARCHAR(64) PRIMARY KEY, [ResultId] NVARCHAR(64), [SourceId] NVARCHAR(200), [SourceDisplayName] NVARCHAR(400),
                   [SourceKind] NVARCHAR(16), [TablesJson] NVARCHAR(MAX), [SourceLastUpdatedAt] DATETIME2, [IsStale] BIT)
                 """,
            };

        foreach (var text in statements)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = text;
            await command.ExecuteNonQueryAsync(ct);
        }

        // ترحيل: أي عمود من الأربعة جداول أعلاه مفقود من جدول أُنشئ قبل وجوده (قاعدة بيانات
        // مشتركة استُخدمت أثناء التطوير التكراري على هذا الملف) — بلا تأثير إن كان الجدول
        // جديدًا أصلًا (ADD COLUMN على عمود موجود يُلتقط ويُتجاهل، نفس نمط ProjectStore).
        await AddColumnIfMissingAsync(connection, ConversationsTable, "Id", "NVARCHAR(64)", ct);
        await AddColumnIfMissingAsync(connection, ConversationsTable, "ProjectId", "NVARCHAR(64)", ct);
        await AddColumnIfMissingAsync(connection, ConversationsTable, "OwnerUserId", "NVARCHAR(64)", ct);
        await AddColumnIfMissingAsync(connection, ConversationsTable, "Title", "NVARCHAR(400)", ct);
        await AddColumnIfMissingAsync(connection, ConversationsTable, "IsSaved", "BIT", ct);
        await AddColumnIfMissingAsync(connection, ConversationsTable, "RollingSummary", "NVARCHAR(MAX)", ct);
        await AddColumnIfMissingAsync(connection, ConversationsTable, "FindingsLedgerJson", "NVARCHAR(MAX)", ct);
        await AddColumnIfMissingAsync(connection, ConversationsTable, "CreatedAt", "DATETIME2", ct);
        await AddColumnIfMissingAsync(connection, ConversationsTable, "UpdatedAt", "DATETIME2", ct);

        await AddColumnIfMissingAsync(connection, MessagesTable, "Id", "NVARCHAR(64)", ct);
        await AddColumnIfMissingAsync(connection, MessagesTable, "ConversationId", "NVARCHAR(64)", ct);
        await AddColumnIfMissingAsync(connection, MessagesTable, "Role", "NVARCHAR(16)", ct);
        await AddColumnIfMissingAsync(connection, MessagesTable, "Text", "NVARCHAR(MAX)", ct);
        await AddColumnIfMissingAsync(connection, MessagesTable, "TemplateText", "NVARCHAR(MAX)", ct);
        await AddColumnIfMissingAsync(connection, MessagesTable, "ResultId", "NVARCHAR(64)", ct);
        await AddColumnIfMissingAsync(connection, MessagesTable, "KnowledgeScope", "NVARCHAR(16)", ct);
        await AddColumnIfMissingAsync(connection, MessagesTable, "CreatedAt", "DATETIME2", ct);

        await AddColumnIfMissingAsync(connection, ResultsTable, "Id", "NVARCHAR(64)", ct);
        await AddColumnIfMissingAsync(connection, ResultsTable, "ConversationId", "NVARCHAR(64)", ct);
        await AddColumnIfMissingAsync(connection, ResultsTable, "ProjectId", "NVARCHAR(64)", ct);
        await AddColumnIfMissingAsync(connection, ResultsTable, "Title", "NVARCHAR(400)", ct);
        await AddColumnIfMissingAsync(connection, ResultsTable, "ResultType", "NVARCHAR(16)", ct);
        await AddColumnIfMissingAsync(connection, ResultsTable, "ColumnsJson", "NVARCHAR(MAX)", ct);
        await AddColumnIfMissingAsync(connection, ResultsTable, "RowsJson", "NVARCHAR(MAX)", ct);
        await AddColumnIfMissingAsync(connection, ResultsTable, "KeyColumn", "NVARCHAR(200)", ct);
        await AddColumnIfMissingAsync(connection, ResultsTable, "PrimaryMeasure", "NVARCHAR(200)", ct);
        await AddColumnIfMissingAsync(connection, ResultsTable, "ExecutedSql", "NVARCHAR(MAX)", ct);
        await AddColumnIfMissingAsync(connection, ResultsTable, "AuditSql", "NVARCHAR(MAX)", ct);
        await AddColumnIfMissingAsync(connection, ResultsTable, "VerificationJson", "NVARCHAR(MAX)", ct);
        await AddColumnIfMissingAsync(connection, ResultsTable, "DataVersion", "INT", ct);
        await AddColumnIfMissingAsync(connection, ResultsTable, "IsTruncated", "BIT", ct);
        await AddColumnIfMissingAsync(connection, ResultsTable, "CreatedAt", "DATETIME2", ct);

        await AddColumnIfMissingAsync(connection, ResultSourcesTable, "Id", "NVARCHAR(64)", ct);
        await AddColumnIfMissingAsync(connection, ResultSourcesTable, "ResultId", "NVARCHAR(64)", ct);
        await AddColumnIfMissingAsync(connection, ResultSourcesTable, "SourceId", "NVARCHAR(200)", ct);
        await AddColumnIfMissingAsync(connection, ResultSourcesTable, "SourceDisplayName", "NVARCHAR(400)", ct);
        await AddColumnIfMissingAsync(connection, ResultSourcesTable, "SourceKind", "NVARCHAR(16)", ct);
        await AddColumnIfMissingAsync(connection, ResultSourcesTable, "TablesJson", "NVARCHAR(MAX)", ct);
        await AddColumnIfMissingAsync(connection, ResultSourcesTable, "SourceLastUpdatedAt", "DATETIME2", ct);
        await AddColumnIfMissingAsync(connection, ResultSourcesTable, "IsStale", "BIT", ct);
    }

    private async Task AddColumnIfMissingAsync(System.Data.Common.DbConnection connection, string table, string column, string sqlServer, CancellationToken ct)
    {
        try
        {
            await using var alter = connection.CreateCommand();
            alter.CommandText = $"ALTER TABLE {table} ADD [{column}] {sqlServer}";
            await alter.ExecuteNonQueryAsync(ct);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2705) { }
    }

    // ---- Conversations ----

    public async Task<AnalystConversation> CreateConversationAsync(
        string projectId, string ownerUserId, string title, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        var now = DateTime.UtcNow;
        var row = new AnalystConversation
        {
            Id = Guid.NewGuid().ToString("N"), ProjectId = projectId, OwnerUserId = ownerUserId,
            Title = title, IsSaved = false, CreatedAt = now, UpdatedAt = now,
        };
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"""
            INSERT INTO {ConversationsTable}
              (Id, ProjectId, OwnerUserId, Title, IsSaved, RollingSummary, FindingsLedgerJson, CreatedAt, UpdatedAt)
            VALUES (@Id, @ProjectId, @OwnerUserId, @Title, @IsSaved, @RollingSummary, @FindingsLedgerJson, @CreatedAt, @UpdatedAt)
            """, row);
        return row;
    }

    public async Task<AnalystConversation?> GetConversationAsync(string ownerUserId, string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<AnalystConversation>(
            $"SELECT * FROM {ConversationsTable} WHERE Id = @id AND OwnerUserId = @ownerUserId", new { id, ownerUserId });
    }

    public async Task TouchConversationAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"UPDATE {ConversationsTable} SET UpdatedAt = @now WHERE Id = @id", new { now = DateTime.UtcNow, id });
    }

    // ---- Messages ----

    public async Task<AnalystMessage> AddMessageAsync(AnalystMessage message, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        message.Id = string.IsNullOrEmpty(message.Id) ? Guid.NewGuid().ToString("N") : message.Id;
        if (message.CreatedAt == default) message.CreatedAt = DateTime.UtcNow;
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"""
            INSERT INTO {MessagesTable} (Id, ConversationId, Role, Text, TemplateText, ResultId, KnowledgeScope, CreatedAt)
            VALUES (@Id, @ConversationId, @Role, @Text, @TemplateText, @ResultId, @KnowledgeScope, @CreatedAt)
            """, message);
        return message;
    }

    public async Task<IReadOnlyList<AnalystMessage>> ListMessagesAsync(string conversationId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<AnalystMessage>(
            $"SELECT * FROM {MessagesTable} WHERE ConversationId = @conversationId ORDER BY CreatedAt", new { conversationId });
        return rows.ToList();
    }

    /// <summary>The one message carrying this ResultId — set once, in AddMessageAsync, for the
    /// final assistant message a result was created for, so no ORDER BY/LIMIT tiebreak is
    /// needed. Used by AnalystController.Reverify to recover the original answer_template
    /// (TemplateText) for re-running C4 — AnalystResult itself never stores it.</summary>
    public async Task<AnalystMessage?> GetMessageByResultIdAsync(string resultId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<AnalystMessage>(
            $"SELECT * FROM {MessagesTable} WHERE ResultId = @resultId", new { resultId });
    }

    // ---- Results ----

    public async Task<AnalystResult> AddResultAsync(AnalystResult result, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        result.Id = string.IsNullOrEmpty(result.Id) ? Guid.NewGuid().ToString("N") : result.Id;
        if (result.CreatedAt == default) result.CreatedAt = DateTime.UtcNow;
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"""
            INSERT INTO {ResultsTable}
              (Id, ConversationId, ProjectId, Title, ResultType, ColumnsJson, RowsJson, KeyColumn, PrimaryMeasure,
               ExecutedSql, AuditSql, VerificationJson, DataVersion, IsTruncated, CreatedAt)
            VALUES
              (@Id, @ConversationId, @ProjectId, @Title, @ResultType, @ColumnsJson, @RowsJson, @KeyColumn, @PrimaryMeasure,
               @ExecutedSql, @AuditSql, @VerificationJson, @DataVersion, @IsTruncated, @CreatedAt)
            """, result);
        return result;
    }

    /// <summary>Scoped to the requesting project so a result id from another project can never
    /// be read cross-tenant even if guessed/leaked — same defense-in-depth every other
    /// project-scoped GET-by-id in this app already applies.</summary>
    public async Task<AnalystResult?> GetResultAsync(string projectId, string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<AnalystResult>(
            $"SELECT * FROM {ResultsTable} WHERE Id = @id AND ProjectId = @projectId", new { id, projectId });
    }

    public async Task AddResultSourcesAsync(IReadOnlyList<AnalystResultSource> sources, CancellationToken ct = default)
    {
        if (sources.Count == 0) return;
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        foreach (var s in sources)
        {
            s.Id = string.IsNullOrEmpty(s.Id) ? Guid.NewGuid().ToString("N") : s.Id;
            await connection.ExecuteAsync($"""
                INSERT INTO {ResultSourcesTable}
                  (Id, ResultId, SourceId, SourceDisplayName, SourceKind, TablesJson, SourceLastUpdatedAt, IsStale)
                VALUES (@Id, @ResultId, @SourceId, @SourceDisplayName, @SourceKind, @TablesJson, @SourceLastUpdatedAt, @IsStale)
                """, s, transaction);
        }
        await transaction.CommitAsync(ct);
    }

    /// <summary>"تحقق الآن" (spec section 7) — refreshes the stored verification, and the rows/
    /// columns/DataVersion if a re-run of the original ExecutedSql came back different, on an
    /// existing result. Everything else (Title, ResultType, ExecutedSql, AuditSql, ...) is
    /// immutable after creation, so only these columns are ever touched here.</summary>
    public async Task UpdateResultVerificationAsync(
        string id, string columnsJson, string rowsJson, int dataVersion, bool isTruncated, string verificationJson,
        CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"""
            UPDATE {ResultsTable}
            SET ColumnsJson = @columnsJson, RowsJson = @rowsJson, DataVersion = @dataVersion,
                IsTruncated = @isTruncated, VerificationJson = @verificationJson
            WHERE Id = @id
            """, new { id, columnsJson, rowsJson, dataVersion, isTruncated, verificationJson });
    }

    /// <summary>Replaces (delete-then-insert, not append) a result's recorded sources — a
    /// reverify re-derives the same source list from the same touched tables every time, so
    /// repeated calls must not pile up duplicate rows the way calling AddResultSourcesAsync
    /// again would.</summary>
    public async Task ReplaceResultSourcesAsync(string resultId, IReadOnlyList<AnalystResultSource> sources, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        await connection.ExecuteAsync($"DELETE FROM {ResultSourcesTable} WHERE ResultId = @resultId", new { resultId }, transaction);
        foreach (var s in sources)
        {
            s.Id = string.IsNullOrEmpty(s.Id) ? Guid.NewGuid().ToString("N") : s.Id;
            s.ResultId = resultId;
            await connection.ExecuteAsync($"""
                INSERT INTO {ResultSourcesTable}
                  (Id, ResultId, SourceId, SourceDisplayName, SourceKind, TablesJson, SourceLastUpdatedAt, IsStale)
                VALUES (@Id, @ResultId, @SourceId, @SourceDisplayName, @SourceKind, @TablesJson, @SourceLastUpdatedAt, @IsStale)
                """, s, transaction);
        }
        await transaction.CommitAsync(ct);
    }

    public async Task<IReadOnlyList<AnalystResultSource>> ListResultSourcesAsync(string resultId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<AnalystResultSource>(
            $"SELECT * FROM {ResultSourcesTable} WHERE ResultId = @resultId", new { resultId });
        return rows.ToList();
    }

    public static List<Dictionary<string, object?>> DeserializeRows(string rowsJson) =>
        JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(rowsJson) ?? new();

    public static List<string> DeserializeColumns(string columnsJson) =>
        JsonSerializer.Deserialize<List<string>>(columnsJson) ?? new();
}
