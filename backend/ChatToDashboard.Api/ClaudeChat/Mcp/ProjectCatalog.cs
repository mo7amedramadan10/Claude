using System.Data;
using System.Data.Common;
using System.Text.Json;
using System.Text.RegularExpressions;
using ChatToDashboard.Api.Data;
using ChatToDashboard.Api.Sources;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace ChatToDashboard.Api.ClaudeChat.Mcp;

/// <summary>جدول متاح لمستخدم محادثة Claude: ملف من مشروعه (<c>proj_&lt;id&gt;.X</c>) أو نظام خارجي مفعَّل (<c>dbo.sys_X</c>).</summary>
public sealed record McpTable(string Name, string Schema, string Bare, string Kind, string? Source, int? Rows);

/// <summary>
/// كتالوج مشروع المستخدم لأدوات MCP — منطق مستقل: يقرأ <c>dbo.repo_Files</c> مباشرة (جداول المشروع) و
/// <c>SourceOptions.ActiveSystems</c> (جداول الأنظمة)، ويصف الأعمدة من <c>INFORMATION_SCHEMA</c> مع معنى كل عمود
/// من <c>dbo.ColumnSemantics</c>، وينفّذ استعلامات القراءة بمهلة وسقف صفوف.
/// </summary>
public class ProjectCatalog
{
    private readonly DataStore _db;
    private readonly SourceOptions _sources;
    private readonly IConfiguration _configuration;

    public ProjectCatalog(DataStore db, IOptions<SourceOptions> sources, IConfiguration configuration)
    {
        _db = db;
        _sources = sources.Value;
        _configuration = configuration;
    }

    public int MaxRows => Math.Clamp(_configuration.GetValue("ClaudeCli:MaxRows", 500), 1, 5000);
    public int QueryTimeoutSeconds => Math.Clamp(_configuration.GetValue("ClaudeCli:QueryTimeoutSeconds", 30), 1, 300);
    public int MaxQueriesPerMessage => Math.Clamp(_configuration.GetValue("ClaudeCli:MaxQueriesPerMessage", 15), 1, 200);

    /// <summary>جداول مشروع المستخدم الحالي فقط + جداول الأنظمة المفعَّلة الموجودة فعلًا في القاعدة.</summary>
    public async Task<IReadOnlyList<McpTable>> ListAsync(string? projectId, CancellationToken ct)
    {
        var result = new List<McpTable>();
        await using var connection = await _db.OpenConnectionAsync(ct);
        if (!string.IsNullOrWhiteSpace(projectId))
        {
            var rows = await connection.QueryAsync<(string TableName, string? DisplayName, string? OriginalFileName, int? RowCount)>(
                "SELECT TableName, DisplayName, OriginalFileName, [RowCount] FROM [dbo].[repo_Files] WHERE ProjectId = @projectId AND TableName IS NOT NULL AND TableName <> '' ORDER BY COALESCE(LastUpdatedAt, UploadedAt) DESC",
                new { projectId });
            foreach (var r in rows)
            {
                var (schema, bare) = Split(r.TableName);
                if (!schema.Equals(_db.ProjectSchema(projectId), StringComparison.OrdinalIgnoreCase)) continue; // دفاع: لا جدول خارج مخطط المشروع
                result.Add(new McpTable($"{schema}.{bare}", schema, bare, "file", r.DisplayName ?? r.OriginalFileName, r.RowCount));
            }
        }
        var existing = (await connection.QueryAsync<string>(
            "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME LIKE 'sys[_]%'")).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var system in _sources.ActiveSystems)
        {
            var bare = "sys_" + TabularFileParser.SanitizeTableName(system.Id);
            if (!existing.Contains(bare)) continue;
            result.Add(new McpTable($"dbo.{bare}", "dbo", bare, "system", system.Name, null));
        }
        return result;
    }

    public static (string Schema, string Bare) Split(string tableName)
    {
        var parts = tableName.Split('.', 2);
        return parts.Length == 2 ? (Unbracket(parts[0]), Unbracket(parts[1])) : ("dbo", Unbracket(parts[0]));
    }

    private static string Unbracket(string s) => s.StartsWith('[') && s.EndsWith(']') ? s[1..^1] : s;

    /// <summary>يطابق اسمًا كتبه الموديل (بمخطط أو بدونه، بأقواس أو بدونها) على جدول من القائمة المسموحة.</summary>
    public static McpTable? Match(IReadOnlyList<McpTable> allowed, string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var (schema, bare) = Split(name.Trim());
        var exact = allowed.FirstOrDefault(t => t.Schema.Equals(schema, StringComparison.OrdinalIgnoreCase) && t.Bare.Equals(bare, StringComparison.OrdinalIgnoreCase));
        if (exact is not null) return exact;
        if (!name.Contains('.'))
        {
            var byBare = allowed.Where(t => t.Bare.Equals(bare, StringComparison.OrdinalIgnoreCase)).ToList();
            if (byBare.Count == 1) return byBare[0];
        }
        return null;
    }

    public sealed record ColumnInfo(string Name, string Type, bool Nullable, string? Concept, string? ConceptLabel, string? Note);

    /// <summary>الأعمدة وأنواعها، ومعنى كل عمود (طبقة المعنى) إن وُجد، وعينة صفوف.</summary>
    public async Task<(IReadOnlyList<ColumnInfo> Columns, IReadOnlyList<string> SampleColumns, IReadOnlyList<object?[]> Sample)> DescribeAsync(
        McpTable table, string? projectId, int sampleRows, CancellationToken ct)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        var cols = (await connection.QueryAsync<(string ColumnName, string DataType, int? MaxLen, string IsNullable)>(
            "SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @bare ORDER BY ORDINAL_POSITION",
            new { schema = table.Schema, bare = table.Bare })).ToList();
        var semantics = new Dictionary<string, (string? Concept, string? Note)>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(projectId))
        {
            var rows = await connection.QueryAsync<(string ColumnName, string? Concept, string? Note)>(
                "SELECT ColumnName, Concept, Note FROM [dbo].[ColumnSemantics] WHERE ProjectId = @projectId AND (TableName = @full OR TableName = @bare)",
                new { projectId, full = table.Name, bare = table.Bare });
            foreach (var r in rows) semantics[r.ColumnName] = (r.Concept, r.Note);
        }
        var columns = cols.Select(c =>
        {
            semantics.TryGetValue(c.ColumnName, out var s);
            var concept = string.IsNullOrWhiteSpace(s.Concept) ? null : s.Concept;
            var label = concept is null ? null : Semantics.ConceptCatalog.Find(concept)?.Label;
            var type = c.MaxLen is > 0 and < 4000 ? $"{c.DataType}({c.MaxLen})" : c.DataType;
            return new ColumnInfo(c.ColumnName, type, c.IsNullable == "YES", concept, label, s.Note);
        }).ToList();

        var (sampleCols, sample, _) = await ExecuteAsync(connection, $"SELECT TOP ({sampleRows}) * FROM {_db.QualifiedTable(table.Schema, table.Bare)}", sampleRows, ct);
        return (columns, sampleCols, sample);
    }

    /// <summary>ينفّذ استعلامًا مدقَّقًا بمهلة ويقرأ حتى السقف + 1 لتحديد الاقتطاع.</summary>
    public async Task<(IReadOnlyList<string> Columns, IReadOnlyList<object?[]> Rows, bool Truncated)> RunAsync(string sql, CancellationToken ct)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await ExecuteAsync(connection, sql, MaxRows, ct);
    }

    private async Task<(IReadOnlyList<string>, IReadOnlyList<object?[]>, bool)> ExecuteAsync(DbConnection connection, string sql, int maxRows, CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandTimeout = QueryTimeoutSeconds;
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);
        var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
        var rows = new List<object?[]>();
        var truncated = false;
        while (await reader.ReadAsync(ct))
        {
            if (rows.Count >= maxRows) { truncated = true; break; }
            var row = new object?[reader.FieldCount];
            for (var i = 0; i < reader.FieldCount; i++) row[i] = Normalize(reader.IsDBNull(i) ? null : reader.GetValue(i));
            rows.Add(row);
        }
        return (columns, rows, truncated);
    }

    private static object? Normalize(object? v) => v switch
    {
        null => null,
        DateTime d => d.ToString("yyyy-MM-dd HH:mm:ss"),
        DateTimeOffset d => d.ToString("yyyy-MM-dd HH:mm:ss zzz"),
        DateOnly d => d.ToString("yyyy-MM-dd"),
        TimeSpan t => t.ToString(),
        byte[] => "<binary>",
        Guid g => g.ToString(),
        _ => v,
    };
}
