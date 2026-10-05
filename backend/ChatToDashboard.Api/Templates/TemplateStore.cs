using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Templates;

/// <summary>
/// Admin edits to the AI-facing templates behind "النماذج" (chat-widget templates,
/// dashboard-gallery templates, the KPI library's 6 measure-type prompts and individual
/// KPI-row overrides) and the platform-wide general rules text — one shared table, since
/// every kind is the same shape: a sparse override/addition keyed by (Kind, Key), with a
/// built-in default (see BuiltinTemplates) a never-touched or reverted row simply falls back
/// to. "رجوع للنص الأصلي" on a built-in is therefore just deleting its override row; only
/// IsCustom rows (platform-owner-added, no built-in to fall back to) support real deletion.
/// </summary>
public class TemplateStore
{
    private readonly DataStore _db;

    public TemplateStore(DataStore db) => _db = db;

    private string Table => _db.Provider == DbProvider.Sqlite ? "\"PromptTemplateOverrides\"" : "[staging].[PromptTemplateOverrides]";

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);

        var text = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {Table} (
                 "Id" TEXT PRIMARY KEY, "Kind" TEXT, "Key" TEXT, "IsCustom" INTEGER, "Status" TEXT,
                 "Title" TEXT, "Description" TEXT, "Icon" TEXT, "Category" TEXT,
                 "SourcesJson" TEXT, "WidgetsJson" TEXT, "PromptText" TEXT, "Version" INTEGER,
                 "UpdatedByUserId" TEXT, "UpdatedByName" TEXT, "UpdatedAt" TEXT, "CreatedAt" TEXT)
               """
            : $"""
               IF OBJECT_ID('staging.PromptTemplateOverrides') IS NULL
               CREATE TABLE {Table} (
                 [Id] NVARCHAR(200) PRIMARY KEY, [Kind] NVARCHAR(32), [Key] NVARCHAR(100), [IsCustom] BIT, [Status] NVARCHAR(16),
                 [Title] NVARCHAR(400), [Description] NVARCHAR(1000), [Icon] NVARCHAR(64), [Category] NVARCHAR(64),
                 [SourcesJson] NVARCHAR(MAX), [WidgetsJson] NVARCHAR(MAX), [PromptText] NVARCHAR(MAX), [Version] INT,
                 [UpdatedByUserId] NVARCHAR(64), [UpdatedByName] NVARCHAR(200), [UpdatedAt] DATETIME2, [CreatedAt] DATETIME2)
               """;

        await using var command = connection.CreateCommand();
        command.CommandText = text;
        await command.ExecuteNonQueryAsync(ct);
    }

    private static string RowId(string kind, string key) => $"{kind}:{key}";

    public async Task<TemplateOverride?> GetAsync(string kind, string key, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<TemplateOverride>(
            $"SELECT * FROM {Table} WHERE Id = @id", new { id = RowId(kind, key) });
    }

    public async Task<IReadOnlyList<TemplateOverride>> ListByKindAsync(string kind, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<TemplateOverride>($"SELECT * FROM {Table} WHERE Kind = @kind", new { kind });
        return rows.ToList();
    }

    /// <summary>Every per-row KPI override's status, for the catalog endpoint to hide/mark
    /// stopped rows in the (otherwise static, client-side) kpi-library.js list — sparse, since
    /// an un-overridden row is implicitly "published".</summary>
    public async Task<IReadOnlyDictionary<int, string>> GetKpiRowStatusesAsync(CancellationToken ct = default)
    {
        var rows = await ListByKindAsync(TemplateKinds.Kpi, ct);
        var result = new Dictionary<int, string>();
        foreach (var r in rows)
            if (int.TryParse(r.Key, out var idx)) result[idx] = r.Status;
        return result;
    }

    /// <summary>Every kpi-library.js category index the platform-owner has disabled — a row's
    /// mere presence here means "off" (see TemplateKinds.KpiCategory); re-enabling a category
    /// is just deleting its row, same "revert" shape as every other kind.</summary>
    public async Task<IReadOnlySet<int>> GetDisabledKpiCategoriesAsync(CancellationToken ct = default)
    {
        var rows = await ListByKindAsync(TemplateKinds.KpiCategory, ct);
        var result = new HashSet<int>();
        foreach (var r in rows)
            if (int.TryParse(r.Key, out var idx)) result.Add(idx);
        return result;
    }

    /// <summary>Upserts an override row, bumping Version (1 on first save). <paramref
    /// name="row"/>'s Id/Version/CreatedAt/UpdatedAt are set/overwritten here.</summary>
    public async Task<TemplateOverride> UpsertAsync(TemplateOverride row, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        var existing = await GetAsync(row.Kind, row.Key, ct);
        row.Id = RowId(row.Kind, row.Key);
        row.Version = (existing?.Version ?? 0) + 1;
        row.CreatedAt = existing?.CreatedAt ?? DateTime.UtcNow;
        row.UpdatedAt = DateTime.UtcNow;

        await using var connection = await _db.OpenConnectionAsync(ct);
        if (existing is null)
        {
            await connection.ExecuteAsync($"""
                INSERT INTO {Table}
                  (Id, Kind, Key, IsCustom, Status, Title, Description, Icon, Category, SourcesJson, WidgetsJson,
                   PromptText, Version, UpdatedByUserId, UpdatedByName, UpdatedAt, CreatedAt)
                VALUES
                  (@Id, @Kind, @Key, @IsCustom, @Status, @Title, @Description, @Icon, @Category, @SourcesJson, @WidgetsJson,
                   @PromptText, @Version, @UpdatedByUserId, @UpdatedByName, @UpdatedAt, @CreatedAt)
                """, row);
        }
        else
        {
            await connection.ExecuteAsync($"""
                UPDATE {Table} SET
                  IsCustom = @IsCustom, Status = @Status, Title = @Title, Description = @Description, Icon = @Icon,
                  Category = @Category, SourcesJson = @SourcesJson, WidgetsJson = @WidgetsJson, PromptText = @PromptText,
                  Version = @Version, UpdatedByUserId = @UpdatedByUserId, UpdatedByName = @UpdatedByName, UpdatedAt = @UpdatedAt
                WHERE Id = @Id
                """, row);
        }
        return row;
    }

    public async Task DeleteAsync(string kind, string key, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"DELETE FROM {Table} WHERE Id = @id", new { id = RowId(kind, key) });
    }
}
