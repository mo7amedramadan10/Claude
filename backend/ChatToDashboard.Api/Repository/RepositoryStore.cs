using System.Data;
using System.Data.Common;
using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Repository;

/// <summary>
/// Persists uploaded files in the database: metadata (name, category, counts) in a
/// catalogue table, tabular rows in their own queryable table so query_data can reach
/// them, and PDF text alongside the metadata so search_documents can.
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
        ? "\"repo_Files\""
        : "[staging].[repo_Files]";

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);

        var text = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {CatalogueTable} (
                 "Id" TEXT PRIMARY KEY, "Name" TEXT, "Category" TEXT, "Kind" TEXT,
                 "RowCount" INTEGER, "ColumnCount" INTEGER, "PageCount" INTEGER,
                 "UploadedAt" TEXT, "TableName" TEXT, "TextContent" TEXT)
               """
            : $"""
               IF OBJECT_ID('staging.repo_Files') IS NULL
               CREATE TABLE {CatalogueTable} (
                 [Id] NVARCHAR(64) PRIMARY KEY, [Name] NVARCHAR(400), [Category] NVARCHAR(200), [Kind] NVARCHAR(20),
                 [RowCount] INT, [ColumnCount] INT, [PageCount] INT,
                 [UploadedAt] DATETIME2, [TableName] NVARCHAR(200), [TextContent] NVARCHAR(MAX))
               """;

        await using var command = connection.CreateCommand();
        command.CommandText = text;
        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task<IReadOnlyList<RepositoryFile>> ListAsync(CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<RepositoryFile>(
            $"SELECT Id, Name, Category, Kind, RowCount, ColumnCount, PageCount, UploadedAt, TableName " +
            $"FROM {CatalogueTable}");
        return rows.OrderByDescending(r => r.UploadedAt).ToList();
    }

    public async Task<IReadOnlyList<string>> ListCategoriesAsync(CancellationToken ct = default)
    {
        var files = await ListAsync(ct);
        return files.Select(f => f.Category).Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(c => c).ToList();
    }

    /// <summary>Saves a parsed upload under the given category.</summary>
    public async Task<RepositoryFile> SaveAsync(
        ParsedUpload parsed, string category, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        var id = Guid.NewGuid().ToString("N");
        var record = new RepositoryFile
        {
            Id = id,
            Name = parsed.FileName,
            Category = string.IsNullOrWhiteSpace(category) ? "عام" : category.Trim(),
            Kind = parsed.Kind,
            RowCount = parsed.Table?.Rows.Count ?? 0,
            ColumnCount = parsed.Table?.Columns.Count ?? 0,
            PageCount = parsed.PageCount,
            UploadedAt = DateTime.UtcNow,
        };

        await using var connection = await _db.OpenConnectionAsync(ct);

        if (parsed.Table is { } table && table.Columns.Count > 0)
        {
            // Rows go into their own table so the SQL tool can aggregate over them.
            var bareName = $"repo_{DataFolderLoader.SanitizeTableName(
                Path.GetFileNameWithoutExtension(parsed.FileName))}_{id[..6]}";
            record.TableName = _db.DisplayTable(bareName);
            await _db.RecreateAndLoadAsync(connection, bareName, table, ct);
        }

        await connection.ExecuteAsync(
            $"INSERT INTO {CatalogueTable} " +
            "(Id, Name, Category, Kind, RowCount, ColumnCount, PageCount, UploadedAt, TableName, TextContent) " +
            "VALUES (@Id, @Name, @Category, @Kind, @RowCount, @ColumnCount, @PageCount, @UploadedAt, @TableName, @TextContent)",
            new
            {
                record.Id, record.Name, record.Category, record.Kind, record.RowCount,
                record.ColumnCount, record.PageCount, record.UploadedAt, record.TableName,
                TextContent = parsed.Text,
            });

        _logger.LogInformation("Saved {File} to repository under category {Category}", record.Name, record.Category);
        return record;
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        var tableName = await connection.ExecuteScalarAsync<string?>(
            $"SELECT TableName FROM {CatalogueTable} WHERE Id = @id", new { id });

        if (!string.IsNullOrWhiteSpace(tableName))
            await _db.DropTableAsync(connection, _db.BareTableName(tableName), ct);

        await connection.ExecuteAsync($"DELETE FROM {CatalogueTable} WHERE Id = @id", new { id });
    }

    /// <summary>Text of PDF files in the enabled categories, for document search.</summary>
    public async Task<IReadOnlyList<(string Name, string Category, string Text)>> GetTextDocumentsAsync(
        CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<(string Name, string Category, string Text)>(
            $"SELECT Name, Category, TextContent FROM {CatalogueTable} " +
            "WHERE TextContent IS NOT NULL AND TextContent <> ''");
        return rows.ToList();
    }
}
