using System.Data;
using System.Data.Common;
using ChatToDashboard.Api.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;

namespace ChatToDashboard.Api.Repository;

/// <summary>
/// Persists uploaded files in the database: metadata (display name, original filename,
/// description, category, counts, schema-change tracking) in a catalogue table, tabular
/// rows in their own queryable table so query_data can reach them, and PDF text alongside
/// the metadata so search_documents can. Per-file named-user permissions and declared
/// file-to-file relationships live in two small side tables, both keyed by file id.
/// </summary>
public class RepositoryStore
{
    private readonly DataStore _db;
    private readonly ILogger<RepositoryStore> _logger;

    public RepositoryStore(DataStore db, ILogger<RepositoryStore> logger)
    {
        _db = db;
        _logger = logger;
    }

    private string CatalogueTable => _db.Provider == DbProvider.Sqlite
        ? "\"repo_Files\"" : "[staging].[repo_Files]";
    private string PermissionsTable => _db.Provider == DbProvider.Sqlite
        ? "\"repo_FilePermissions\"" : "[staging].[repo_FilePermissions]";
    private string RelationshipsTable => _db.Provider == DbProvider.Sqlite
        ? "\"repo_FileRelationships\"" : "[staging].[repo_FileRelationships]";
    private string HistoryTable => _db.Provider == DbProvider.Sqlite
        ? "\"DashboardHistory\"" : "[staging].[DashboardHistory]";

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);

        var text = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {CatalogueTable} (
                 "Id" TEXT PRIMARY KEY, "DisplayName" TEXT, "OriginalFileName" TEXT, "Description" TEXT,
                 "Category" TEXT, "Kind" TEXT, "RowCount" INTEGER, "ColumnCount" INTEGER, "PageCount" INTEGER,
                 "UploadedAt" TEXT, "LastUpdatedAt" TEXT, "TableName" TEXT, "TextContent" TEXT,
                 "ColumnsJson" TEXT, "SchemaChangedAt" TEXT, "CreatedByUserId" TEXT)
               """
            : $"""
               IF OBJECT_ID('staging.repo_Files') IS NULL
               CREATE TABLE {CatalogueTable} (
                 [Id] NVARCHAR(64) PRIMARY KEY, [DisplayName] NVARCHAR(400), [OriginalFileName] NVARCHAR(400),
                 [Description] NVARCHAR(1000), [Category] NVARCHAR(200), [Kind] NVARCHAR(20),
                 [RowCount] INT, [ColumnCount] INT, [PageCount] INT,
                 [UploadedAt] DATETIME2, [LastUpdatedAt] DATETIME2, [TableName] NVARCHAR(200),
                 [TextContent] NVARCHAR(MAX), [ColumnsJson] NVARCHAR(MAX), [SchemaChangedAt] DATETIME2,
                 [CreatedByUserId] NVARCHAR(200))
               """;
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = text;
            await command.ExecuteNonQueryAsync(ct);
        }

        // Migration for a catalogue created before the fields above existed. SQLite has no
        // "ADD COLUMN IF NOT EXISTS", so the duplicate-column failure is just swallowed (same
        // pattern as HistoryStore/ShareStore). Pre-existing rows' "Name" column (the old,
        // single "uploaded filename" field) becomes the DisplayName fallback below, since
        // that's the only name they ever had.
        foreach (var (column, sqliteType, sqlServerType) in new[]
                 {
                     ("DisplayName", "TEXT", "NVARCHAR(400)"),
                     ("OriginalFileName", "TEXT", "NVARCHAR(400)"),
                     ("Description", "TEXT", "NVARCHAR(1000)"),
                     ("LastUpdatedAt", "TEXT", "DATETIME2"),
                     ("ColumnsJson", "TEXT", "NVARCHAR(MAX)"),
                     ("SchemaChangedAt", "TEXT", "DATETIME2"),
                     ("CreatedByUserId", "TEXT", "NVARCHAR(200)"),
                 })
        {
            try
            {
                await using var alter = connection.CreateCommand();
                alter.CommandText = _db.Provider == DbProvider.Sqlite
                    ? $"ALTER TABLE {CatalogueTable} ADD COLUMN \"{column}\" {sqliteType}"
                    : $"ALTER TABLE {CatalogueTable} ADD [{column}] {sqlServerType}";
                await alter.ExecuteNonQueryAsync(ct);
            }
            catch (SqliteException ex) when (ex.Message.Contains("duplicate column", StringComparison.OrdinalIgnoreCase))
            {
            }
            catch (SqlException ex) when (ex.Message.Contains("already", StringComparison.OrdinalIgnoreCase))
            {
            }
        }
        // A pre-migration row has DisplayName/OriginalFileName NULL but its old "Name" value
        // is gone (the column was never renamed, just superseded) — nothing to backfill from
        // here since ListAsync's COALESCE already falls back sensibly on read.

        var permText = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {PermissionsTable} (
                 "FileId" TEXT, "UserId" TEXT, PRIMARY KEY ("FileId", "UserId"))
               """
            : $"""
               IF OBJECT_ID('staging.repo_FilePermissions') IS NULL
               CREATE TABLE {PermissionsTable} (
                 [FileId] NVARCHAR(64), [UserId] NVARCHAR(200), PRIMARY KEY ([FileId], [UserId]))
               """;
        await using (var command = connection.CreateCommand()) { command.CommandText = permText; await command.ExecuteNonQueryAsync(ct); }

        var relText = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {RelationshipsTable} (
                 "Id" TEXT PRIMARY KEY, "FileId" TEXT, "RelatedFileId" TEXT, "SharedColumn" TEXT)
               """
            : $"""
               IF OBJECT_ID('staging.repo_FileRelationships') IS NULL
               CREATE TABLE {RelationshipsTable} (
                 [Id] NVARCHAR(64) PRIMARY KEY, [FileId] NVARCHAR(64), [RelatedFileId] NVARCHAR(64),
                 [SharedColumn] NVARCHAR(200))
               """;
        await using (var command = connection.CreateCommand()) { command.CommandText = relText; await command.ExecuteNonQueryAsync(ct); }
    }

    private const string SelectColumns =
        "Id, COALESCE(DisplayName, OriginalFileName, '') AS DisplayName, " +
        "COALESCE(OriginalFileName, '') AS OriginalFileName, COALESCE(Description, '') AS Description, " +
        "Category, Kind, RowCount, ColumnCount, PageCount, UploadedAt, " +
        "COALESCE(LastUpdatedAt, UploadedAt) AS LastUpdatedAt, TableName, SchemaChangedAt, " +
        "COALESCE(CreatedByUserId, '') AS CreatedByUserId";

    public async Task<IReadOnlyList<RepositoryFile>> ListAsync(CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var files = (await connection.QueryAsync<RepositoryFile>($"SELECT {SelectColumns} FROM {CatalogueTable}"))
            .OrderByDescending(r => r.UploadedAt).ToList();
        if (files.Count == 0) return files;

        var permRows = await connection.QueryAsync<(string FileId, string UserId)>($"SELECT FileId, UserId FROM {PermissionsTable}");
        var permsByFile = permRows.GroupBy(p => p.FileId).ToDictionary(g => g.Key, g => g.Select(p => p.UserId).ToList());

        var relRows = (await connection.QueryAsync(
            $"SELECT r.Id, r.FileId, r.RelatedFileId, r.SharedColumn, f.DisplayName AS RelatedFileName " +
            $"FROM {RelationshipsTable} r LEFT JOIN {CatalogueTable} f ON f.Id = r.RelatedFileId")).ToList();
        var relsByFile = relRows.GroupBy(r => (string)r.FileId).ToDictionary(
            g => g.Key,
            g => g.Select(r => new FileRelationship
            {
                Id = r.Id, RelatedFileId = r.RelatedFileId,
                RelatedFileName = r.RelatedFileName ?? "", SharedColumn = r.SharedColumn ?? "",
            }).ToList());

        // Usage: how many Draft (History) dashboards currently reference this file's table —
        // a simple, deterministic proxy (no separate tracking table to keep in sync) since
        // every widget that queries a table already names it literally in its stored SQL/
        // structured query, both serialized into WidgetsJson.
        var usageCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var lastUsedAt = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var f in files.Where(f => !string.IsNullOrWhiteSpace(f.TableName)))
            {
                var bare = _db.BareTableName(f.TableName!);
                var count = await connection.ExecuteScalarAsync<int>(
                    $"SELECT COUNT(*) FROM {HistoryTable} WHERE WidgetsJson LIKE @pattern",
                    new { pattern = $"%{bare}%" });
                usageCounts[f.Id] = count;
                if (count > 0)
                {
                    var last = await connection.ExecuteScalarAsync<DateTime?>(
                        $"SELECT MAX(CreatedAt) FROM {HistoryTable} WHERE WidgetsJson LIKE @pattern",
                        new { pattern = $"%{bare}%" });
                    if (last is not null) lastUsedAt[f.Id] = last.Value;
                }
            }
        }
        catch (DbException)
        {
            // The History table may not exist yet on a brand-new install — usage just
            // reads as 0 rather than failing the whole file list.
        }

        foreach (var f in files)
        {
            f.PermittedUserIds = permsByFile.TryGetValue(f.Id, out var ids) ? ids : new List<string>();
            f.Relationships = relsByFile.TryGetValue(f.Id, out var rels) ? rels : new List<FileRelationship>();
            f.UsageCount = usageCounts.TryGetValue(f.Id, out var c) ? c : 0;
            f.LastUsedAt = lastUsedAt.TryGetValue(f.Id, out var lu) ? lu : null;
        }
        return files;
    }

    public async Task<IReadOnlyList<string>> ListCategoriesAsync(CancellationToken ct = default)
    {
        var files = await ListAsync(ct);
        return files.Select(f => f.Category).Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(c => c).ToList();
    }

    /// <summary>"Upload new file": creates a brand new file identity.</summary>
    public async Task<RepositoryFile> SaveAsync(
        ParsedUpload parsed, string displayName, string description, string category,
        string createdByUserId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        var id = Guid.NewGuid().ToString("N");
        var now = DateTime.UtcNow;
        var record = new RepositoryFile
        {
            Id = id,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? parsed.FileName : displayName.Trim(),
            OriginalFileName = parsed.FileName,
            Description = description?.Trim() ?? "",
            Category = string.IsNullOrWhiteSpace(category) ? "عام" : category.Trim(),
            Kind = parsed.Kind,
            RowCount = parsed.Table?.Rows.Count ?? 0,
            ColumnCount = parsed.Table?.Columns.Count ?? 0,
            PageCount = parsed.PageCount,
            UploadedAt = now,
            LastUpdatedAt = now,
            CreatedByUserId = createdByUserId,
        };

        await using var connection = await _db.OpenConnectionAsync(ct);

        var columnsJson = "[]";
        if (parsed.Table is { } table && table.Columns.Count > 0)
        {
            var bareName = $"repo_{DataFolderLoader.SanitizeTableName(
                Path.GetFileNameWithoutExtension(parsed.FileName))}_{id[..6]}";
            record.TableName = _db.DisplayTable(bareName);
            await _db.RecreateAndLoadAsync(connection, bareName, table, ct);
            columnsJson = System.Text.Json.JsonSerializer.Serialize(
                table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
        }

        await connection.ExecuteAsync(
            $"INSERT INTO {CatalogueTable} (Id, DisplayName, OriginalFileName, Description, Category, Kind, " +
            "RowCount, ColumnCount, PageCount, UploadedAt, LastUpdatedAt, TableName, TextContent, ColumnsJson, CreatedByUserId) " +
            "VALUES (@Id, @DisplayName, @OriginalFileName, @Description, @Category, @Kind, " +
            "@RowCount, @ColumnCount, @PageCount, @UploadedAt, @LastUpdatedAt, @TableName, @TextContent, @ColumnsJson, @CreatedByUserId)",
            new
            {
                record.Id, record.DisplayName, record.OriginalFileName, record.Description, record.Category,
                record.Kind, record.RowCount, record.ColumnCount, record.PageCount, record.UploadedAt,
                record.LastUpdatedAt, record.TableName, TextContent = parsed.Text, ColumnsJson = columnsJson,
                record.CreatedByUserId,
            });

        _logger.LogInformation("Saved {File} to repository under category {Category}", record.DisplayName, record.Category);
        return record;
    }

    /// <summary>
    /// "Update this file": replaces the data behind an existing identity. Display name,
    /// category, relationships and permissions are untouched — only the data,
    /// OriginalFileName and LastUpdatedAt change, and TableName never changes, which is
    /// what keeps every dashboard already built against this file working. If the new
    /// column set differs from the old one, stamps SchemaChangedAt so a dashboard saved
    /// before this moment can be flagged as possibly depending on a column that's gone.
    /// </summary>
    public async Task<RepositoryFile?> UpdateDataAsync(string id, ParsedUpload parsed, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);

        var row = await connection.QuerySingleOrDefaultAsync(
            $"SELECT TableName, ColumnsJson FROM {CatalogueTable} WHERE Id = @id", new { id });
        if (row is null) return null; // no file with this id at all
        string existingTableName = (string?)row.TableName ?? "";
        string existingColumnsJson = (string?)row.ColumnsJson ?? "[]";

        var now = DateTime.UtcNow;
        string? newColumnsJson = null;
        DateTime? schemaChangedAt = null;

        if (parsed.Table is { } table && table.Columns.Count > 0 && !string.IsNullOrWhiteSpace(existingTableName))
        {
            var bareName = _db.BareTableName(existingTableName);
            await _db.RecreateAndLoadAsync(connection, bareName, table, ct);

            var newColumns = table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
            newColumnsJson = System.Text.Json.JsonSerializer.Serialize(newColumns);
            var oldColumns = System.Text.Json.JsonSerializer.Deserialize<List<string>>(existingColumnsJson) ?? new();
            if (!newColumns.OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .SequenceEqual(oldColumns.OrderBy(x => x, StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase))
                schemaChangedAt = now;
        }

        var updated = await connection.ExecuteAsync(
            $"UPDATE {CatalogueTable} SET OriginalFileName = @OriginalFileName, RowCount = @RowCount, " +
            "ColumnCount = @ColumnCount, PageCount = @PageCount, LastUpdatedAt = @LastUpdatedAt, " +
            "TextContent = @TextContent" +
            (newColumnsJson is not null ? ", ColumnsJson = @ColumnsJson" : "") +
            (schemaChangedAt is not null ? ", SchemaChangedAt = @SchemaChangedAt" : "") +
            " WHERE Id = @Id",
            new
            {
                Id = id, OriginalFileName = parsed.FileName, RowCount = parsed.Table?.Rows.Count ?? 0,
                ColumnCount = parsed.Table?.Columns.Count ?? 0, PageCount = parsed.PageCount,
                LastUpdatedAt = now, TextContent = parsed.Text, ColumnsJson = newColumnsJson, SchemaChangedAt = schemaChangedAt,
            });
        if (updated == 0) return null;

        _logger.LogInformation("Updated data for repository file {Id}{Schema}", id,
            schemaChangedAt is not null ? " (schema changed)" : "");
        return (await ListAsync(ct)).FirstOrDefault(f => f.Id == id);
    }

    /// <summary>Edits display name/description/category without touching the data.</summary>
    public async Task<bool> UpdateMetaAsync(string id, string displayName, string description, string category, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var affected = await connection.ExecuteAsync(
            $"UPDATE {CatalogueTable} SET DisplayName = @displayName, Description = @description, Category = @category WHERE Id = @id",
            new { id, displayName = displayName.Trim(), description = description?.Trim() ?? "", category = string.IsNullOrWhiteSpace(category) ? "عام" : category.Trim() });
        return affected > 0;
    }

    /// <summary>Just the creator id — for an authorization check (Owner-or-Admin) that
    /// doesn't need the full joined ListAsync row. COALESCEd to "" (matching SelectColumns
    /// above) rather than a raw SELECT: a file uploaded before creator-tracking existed has
    /// CreatedByUserId = NULL in the database, and ExecuteScalarAsync returns C# null both
    /// for "row has a NULL value" and "no row matched at all" — without the COALESCE those
    /// two cases were indistinguishable, so callers (GetPermissions/SetPermissions) treated
    /// a perfectly real, listed file as 404 "not found" the moment anyone tried to manage its
    /// permissions, with no such file actually missing.</summary>
    public async Task<string?> GetCreatedByUserIdAsync(string fileId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.ExecuteScalarAsync<string?>(
            $"SELECT COALESCE(CreatedByUserId, '') FROM {CatalogueTable} WHERE Id = @fileId", new { fileId });
    }

    public async Task<List<string>> GetPermittedUserIdsAsync(string fileId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<string>($"SELECT UserId FROM {PermissionsTable} WHERE FileId = @fileId", new { fileId });
        return rows.ToList();
    }

    public async Task SetPermittedUserIdsAsync(string fileId, IReadOnlyList<string> userIds, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"DELETE FROM {PermissionsTable} WHERE FileId = @fileId", new { fileId });
        foreach (var userId in userIds.Distinct(StringComparer.OrdinalIgnoreCase))
            await connection.ExecuteAsync($"INSERT INTO {PermissionsTable} (FileId, UserId) VALUES (@fileId, @userId)", new { fileId, userId });
    }

    public async Task<FileRelationship> AddRelationshipAsync(string fileId, string relatedFileId, string sharedColumn, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var relId = Guid.NewGuid().ToString("N");
        await connection.ExecuteAsync(
            $"INSERT INTO {RelationshipsTable} (Id, FileId, RelatedFileId, SharedColumn) VALUES (@relId, @fileId, @relatedFileId, @sharedColumn)",
            new { relId, fileId, relatedFileId, sharedColumn = sharedColumn.Trim() });
        var relatedName = await connection.ExecuteScalarAsync<string?>(
            $"SELECT COALESCE(DisplayName, OriginalFileName, '') FROM {CatalogueTable} WHERE Id = @relatedFileId", new { relatedFileId });
        return new FileRelationship { Id = relId, RelatedFileId = relatedFileId, RelatedFileName = relatedName ?? "", SharedColumn = sharedColumn.Trim() };
    }

    public async Task DeleteRelationshipAsync(string fileId, string relationshipId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"DELETE FROM {RelationshipsTable} WHERE Id = @relationshipId AND FileId = @fileId", new { relationshipId, fileId });
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        var tableName = await connection.ExecuteScalarAsync<string?>(
            $"SELECT TableName FROM {CatalogueTable} WHERE Id = @id", new { id });

        if (!string.IsNullOrWhiteSpace(tableName))
            await _db.DropTableAsync(connection, _db.BareTableName(tableName), ct);

        await connection.ExecuteAsync($"DELETE FROM {CatalogueTable} WHERE Id = @id", new { id });
        await connection.ExecuteAsync($"DELETE FROM {PermissionsTable} WHERE FileId = @id", new { id });
        await connection.ExecuteAsync($"DELETE FROM {RelationshipsTable} WHERE FileId = @id OR RelatedFileId = @id", new { id });
    }

    /// <summary>Text of PDF files in the enabled categories, for document search.</summary>
    public async Task<IReadOnlyList<(string Id, string Name, string Category, string Text)>> GetTextDocumentsAsync(
        CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<(string Id, string Name, string Category, string Text)>(
            $"SELECT Id, COALESCE(DisplayName, OriginalFileName, '') AS Name, Category, TextContent FROM {CatalogueTable} " +
            "WHERE TextContent IS NOT NULL AND TextContent <> ''");
        return rows.ToList();
    }
}
