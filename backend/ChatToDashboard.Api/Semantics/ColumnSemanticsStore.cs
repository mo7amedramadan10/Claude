using System.Text.Json.Serialization;
using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Semantics;

/// <summary>مصادر قيمة المعنى: قاعدة اسم حتمية، أو الموديل، أو تأكيد/تصحيح المستخدم (يفوز دائمًا).</summary>
public static class SemanticSources
{
    public const string Rule = "rule";
    public const string Ai = "ai";
    public const string User = "user";
}

/// <summary>صف واحد من <c>ColumnSemantics</c>: معنى عمود واحد في جدول واحد لمشروع واحد.</summary>
public class ColumnSemantic
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("projectId")] public string ProjectId { get; set; } = "";
    [JsonPropertyName("tableName")] public string TableName { get; set; } = "";
    [JsonPropertyName("columnName")] public string ColumnName { get; set; } = "";

    /// <summary>معرّف من <see cref="ConceptCatalog"/>، أو null عندما لا يطابق العمود أي مفهوم (عمود مساعد).</summary>
    [JsonPropertyName("concept")] public string? Concept { get; set; }

    [JsonPropertyName("role")] public string Role { get; set; } = ConceptRoles.Text;
    [JsonPropertyName("unit")] public string? Unit { get; set; }

    /// <summary>JSON لخريطة قيم العميل → القيمة القياسية للمفهوم (مثل {"مفتوحة":"open","مكسوبة":"won"})، أو null.</summary>
    [JsonPropertyName("valueMapJson")] public string? ValueMapJson { get; set; }

    [JsonPropertyName("confidence")] public double Confidence { get; set; }
    [JsonPropertyName("source")] public string Source { get; set; } = SemanticSources.Rule;

    /// <summary>شرح قصير بالعربية لسبب الاختيار (يُعرض للمستخدم عند المراجعة).</summary>
    [JsonPropertyName("note")] public string? Note { get; set; }
    [JsonPropertyName("updatedAt")] public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// تخزين طبقة المعنى (المرحلة أ من طبقة المعنى في CLAUDE.md): صف لكل عمود في كل جدول مرفوع،
/// فريد على (المشروع، الجدول، العمود). صف مصدره المستخدم لا يُستبدل بتحليل آلي لاحق.
/// </summary>
public class ColumnSemanticsStore
{
    private readonly DataStore _db;

    public ColumnSemanticsStore(DataStore db) => _db = db;

    private string Table => _db.Provider == DbProvider.Sqlite ? "\"ColumnSemantics\"" : "[staging].[ColumnSemantics]";

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);
        var text = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {Table} (
                 "Id" TEXT PRIMARY KEY, "ProjectId" TEXT NOT NULL, "TableName" TEXT NOT NULL, "ColumnName" TEXT NOT NULL,
                 "Concept" TEXT, "Role" TEXT NOT NULL, "Unit" TEXT, "ValueMapJson" TEXT, "Confidence" REAL NOT NULL,
                 "Source" TEXT NOT NULL, "Note" TEXT, "UpdatedAt" TEXT NOT NULL,
                 UNIQUE ("ProjectId", "TableName", "ColumnName"))
               """
            : $"""
               IF OBJECT_ID('staging.ColumnSemantics') IS NULL
               CREATE TABLE {Table} (
                 [Id] NVARCHAR(64) PRIMARY KEY, [ProjectId] NVARCHAR(64) NOT NULL, [TableName] NVARCHAR(300) NOT NULL,
                 [ColumnName] NVARCHAR(300) NOT NULL, [Concept] NVARCHAR(64), [Role] NVARCHAR(20) NOT NULL,
                 [Unit] NVARCHAR(40), [ValueMapJson] NVARCHAR(MAX), [Confidence] FLOAT NOT NULL,
                 [Source] NVARCHAR(10) NOT NULL, [Note] NVARCHAR(500), [UpdatedAt] DATETIME2 NOT NULL,
                 CONSTRAINT UQ_ColumnSemantics UNIQUE ([ProjectId], [TableName], [ColumnName]))
               """;
        await using var command = connection.CreateCommand();
        command.CommandText = text;
        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task<IReadOnlyList<ColumnSemantic>> ListForTableAsync(string projectId, string tableName, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<ColumnSemantic>(
            $"SELECT * FROM {Table} WHERE ProjectId = @projectId AND TableName = @tableName ORDER BY ColumnName",
            new { projectId, tableName });
        return rows.ToList();
    }

    public async Task<IReadOnlyList<ColumnSemantic>> ListForProjectAsync(string projectId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<ColumnSemantic>(
            $"SELECT * FROM {Table} WHERE ProjectId = @projectId ORDER BY TableName, ColumnName", new { projectId });
        return rows.ToList();
    }

    /// <summary>يحفظ نتيجة تحليل آلي: يستبدل صفوف rule/ai، ويترك صفوف user كما هي.</summary>
    public async Task UpsertAnalyzedAsync(IReadOnlyList<ColumnSemantic> rows, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        foreach (var r in rows)
        {
            var existing = await connection.QuerySingleOrDefaultAsync<ColumnSemantic>(
                $"SELECT * FROM {Table} WHERE ProjectId = @ProjectId AND TableName = @TableName AND ColumnName = @ColumnName", r);
            if (existing is { Source: SemanticSources.User }) continue;
            r.Id = existing?.Id ?? Guid.NewGuid().ToString("N");
            r.UpdatedAt = DateTime.UtcNow;
            if (existing is null)
                await connection.ExecuteAsync(
                    $"INSERT INTO {Table} (Id, ProjectId, TableName, ColumnName, Concept, Role, Unit, ValueMapJson, Confidence, Source, Note, UpdatedAt) " +
                    "VALUES (@Id, @ProjectId, @TableName, @ColumnName, @Concept, @Role, @Unit, @ValueMapJson, @Confidence, @Source, @Note, @UpdatedAt)", r);
            else
                await connection.ExecuteAsync(
                    $"UPDATE {Table} SET Concept = @Concept, Role = @Role, Unit = @Unit, ValueMapJson = @ValueMapJson, Confidence = @Confidence, " +
                    "Source = @Source, Note = @Note, UpdatedAt = @UpdatedAt WHERE Id = @Id", r);
        }
    }

    /// <summary>تأكيد أو تصحيح من المستخدم — يصير المصدر user بثقة كاملة ولا يُستبدل آليًا بعدها.</summary>
    public async Task SetByUserAsync(string projectId, string tableName, string columnName, string? concept, string role,
        string? unit, string? valueMapJson, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var now = DateTime.UtcNow;
        var updated = await connection.ExecuteAsync(
            $"UPDATE {Table} SET Concept = @concept, Role = @role, Unit = @unit, ValueMapJson = @valueMapJson, Confidence = 1, Source = 'user', " +
            "Note = NULL, UpdatedAt = @now WHERE ProjectId = @projectId AND TableName = @tableName AND ColumnName = @columnName",
            new { concept, role, unit, valueMapJson, now, projectId, tableName, columnName });
        if (updated == 0)
            await connection.ExecuteAsync(
                $"INSERT INTO {Table} (Id, ProjectId, TableName, ColumnName, Concept, Role, Unit, ValueMapJson, Confidence, Source, Note, UpdatedAt) " +
                "VALUES (@id, @projectId, @tableName, @columnName, @concept, @role, @unit, @valueMapJson, 1, 'user', NULL, @now)",
                new { id = Guid.NewGuid().ToString("N"), projectId, tableName, columnName, concept, role, unit, valueMapJson, now });
    }

    public async Task DeleteForTableAsync(string projectId, string tableName, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"DELETE FROM {Table} WHERE ProjectId = @projectId AND TableName = @tableName", new { projectId, tableName });
    }
}
