using System.Text;
using System.Text.Json;
using ChatToDashboard.Api.Data;
using ChatToDashboard.Api.Decks;
using Dapper;

namespace ChatToDashboard.Api.Freeform;

/// <summary>
/// نماذج تصميم حر مرفوعة من ملفات موجودة (PowerPoint/PDF/HTML) لكل مشروع — جدول <c>dbo.FreeformTemplates</c>.
/// الملف يُقرأ حتميًا بـ <see cref="DeckImporter.Extract"/> ويُحوَّل إلى نموذج بأقسام من عناوين صفحاته ومحتوى نصي مرجعي
/// يُرفق بنظام الموديل عند البناء. مفتاح النموذج المخصص <c>u:&lt;id&gt;</c> ليتميّز عن الكتالوج الثابت.
/// </summary>
public class FreeformTemplateStore
{
    public const string KeyPrefix = "u:";
    public const int MaxContentChars = 30_000;
    private readonly DataStore _db;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private volatile bool _ready;
    private const string T = "[dbo].[FreeformTemplates]";
    public FreeformTemplateStore(DataStore db) => _db = db;

    private sealed class Row
    {
        public string Id { get; set; } = ""; public string? ProjectId { get; set; } public string CreatedByUserId { get; set; } = "";
        public string Kind { get; set; } = "design"; public string Name { get; set; } = ""; public string Description { get; set; } = "";
        public string SectionsJson { get; set; } = "[]"; public string Brief { get; set; } = ""; public string Content { get; set; } = "";
        public string SourceFile { get; set; } = ""; public DateTime CreatedAt { get; set; }
    }

    private async Task EnsureAsync(CancellationToken ct)
    {
        if (_ready) return;
        await _gate.WaitAsync(ct);
        try
        {
            if (_ready) return;
            await using var c = await _db.OpenConnectionAsync(ct);
            await _db.CreateContainerIfMissingAsync(c, ct);
            await c.ExecuteAsync($"""
                IF OBJECT_ID('dbo.FreeformTemplates') IS NULL
                CREATE TABLE {T} (
                  [Id] NVARCHAR(32) PRIMARY KEY, [ProjectId] NVARCHAR(64), [CreatedByUserId] NVARCHAR(200) NOT NULL, [Kind] NVARCHAR(16) NOT NULL,
                  [Name] NVARCHAR(200) NOT NULL, [Description] NVARCHAR(500) NOT NULL DEFAULT '', [SectionsJson] NVARCHAR(MAX) NOT NULL, [Brief] NVARCHAR(MAX) NOT NULL,
                  [Content] NVARCHAR(MAX) NOT NULL, [SourceFile] NVARCHAR(300) NOT NULL DEFAULT '', [CreatedAt] DATETIME2 NOT NULL);
                """);
            _ready = true;
        }
        finally { _gate.Release(); }
    }

    private static FreeformTemplate Map(Row r) => new()
    {
        Key = KeyPrefix + r.Id, Kind = r.Kind, Category = "uploaded", Icon = "🗂️", Name = r.Name, Description = r.Description, Sketch = "h/p/b/t",
        Sections = JsonSerializer.Deserialize<List<FreeformTemplateSection>>(r.SectionsJson) ?? new(), Brief = r.Brief, Content = r.Content,
        Custom = true, SourceFile = r.SourceFile, CreatedByUserId = r.CreatedByUserId, CreatedAt = r.CreatedAt,
    };

    public async Task<IReadOnlyList<FreeformTemplate>> ListAsync(string? projectId, CancellationToken ct = default)
    {
        await EnsureAsync(ct);
        await using var c = await _db.OpenConnectionAsync(ct);
        return (await c.QueryAsync<Row>($"SELECT * FROM {T} WHERE (ProjectId = @projectId OR (@projectId IS NULL AND ProjectId IS NULL)) ORDER BY CreatedAt DESC", new { projectId })).Select(Map).ToList();
    }

    public async Task<FreeformTemplate?> GetAsync(string key, string? projectId, CancellationToken ct = default)
    {
        if (key is null || !key.StartsWith(KeyPrefix, StringComparison.Ordinal)) return null;
        await EnsureAsync(ct);
        await using var c = await _db.OpenConnectionAsync(ct);
        var r = await c.QuerySingleOrDefaultAsync<Row>($"SELECT * FROM {T} WHERE Id = @id AND (ProjectId = @projectId OR (@projectId IS NULL AND ProjectId IS NULL))", new { id = key[KeyPrefix.Length..], projectId });
        return r is null ? null : Map(r);
    }

    public async Task<FreeformTemplate> CreateAsync(string? projectId, string userId, FreeformTemplate t, CancellationToken ct = default)
    {
        await EnsureAsync(ct);
        var r = new Row { Id = Guid.NewGuid().ToString("N")[..16], ProjectId = projectId, CreatedByUserId = userId, Kind = FreeformStore.NormalizeKind(t.Kind), Name = t.Name, Description = t.Description,
            SectionsJson = JsonSerializer.Serialize(t.Sections), Brief = t.Brief, Content = t.Content ?? "", SourceFile = t.SourceFile ?? "", CreatedAt = DateTime.UtcNow };
        await using var c = await _db.OpenConnectionAsync(ct);
        await c.ExecuteAsync($"INSERT INTO {T} (Id, ProjectId, CreatedByUserId, Kind, Name, Description, SectionsJson, Brief, Content, SourceFile, CreatedAt) VALUES (@Id, @ProjectId, @CreatedByUserId, @Kind, @Name, @Description, @SectionsJson, @Brief, @Content, @SourceFile, @CreatedAt)", r);
        return Map(r);
    }

    public async Task<bool> DeleteAsync(string key, string? projectId, CancellationToken ct = default)
    {
        if (!key.StartsWith(KeyPrefix, StringComparison.Ordinal)) return false;
        await EnsureAsync(ct);
        await using var c = await _db.OpenConnectionAsync(ct);
        return await c.ExecuteAsync($"DELETE FROM {T} WHERE Id = @id AND (ProjectId = @projectId OR (@projectId IS NULL AND ProjectId IS NULL))", new { id = key[KeyPrefix.Length..], projectId }) > 0;
    }

    /// <summary>تحويل ملف مقروء إلى نموذج: قسم لكل صفحة/شريحة بعنوانها (أو «القسم n»)، ومحتوى نصي مرجعي محدود الطول.</summary>
    public static FreeformTemplate FromDocument(ImportedDocument doc, string kind, string? name, string fileName)
    {
        var sections = new List<FreeformTemplateSection>();
        var content = new StringBuilder();
        var n = 0;
        foreach (var p in doc.Pages)
        {
            if (p.IsEmpty) continue;
            n++;
            var title = string.IsNullOrWhiteSpace(p.Title) ? (p.Paragraphs.FirstOrDefault(x => x.Length <= 80) ?? $"القسم {n}") : p.Title.Trim();
            if (title.Length > 80) title = title[..80] + "…";
            sections.Add(new FreeformTemplateSection(title, p.Tables.Count > 0 || p.Charts.Count > 0));
            content.AppendLine($"## {title}");
            foreach (var para in p.Paragraphs.Take(40)) content.AppendLine("- " + para.Trim());
            foreach (var t in p.Tables.Take(3))
            {
                content.AppendLine("جدول: " + string.Join(" | ", t.Columns));
                foreach (var row in t.Rows.Take(8)) content.AppendLine("  " + string.Join(" | ", row));
            }
            foreach (var ch in p.Charts.Take(2)) content.AppendLine($"رسم {ch.Type}: {string.Join("، ", ch.Labels.Zip(ch.Values ?? new List<double>(), (l, v) => $"{l}={v}"))}");
            if (p.Links.Count > 0) content.AppendLine("روابط: " + string.Join("، ", p.Links.Take(10).Select(l => l.Text)));
            if (content.Length > MaxContentChars) break;
        }
        foreach (var b in doc.DataBlocks.Take(5)) { content.AppendLine("بيانات: " + b); if (content.Length > MaxContentChars) break; }
        if (sections.Count == 0) sections.Add(new FreeformTemplateSection("المحتوى"));
        var text = content.ToString(); if (text.Length > MaxContentChars) text = text[..MaxContentChars] + "\n…(اقتُطع)";
        var nm = string.IsNullOrWhiteSpace(name) ? (string.IsNullOrWhiteSpace(doc.Title) ? Path.GetFileNameWithoutExtension(fileName) : doc.Title.Trim()) : name.Trim();
        if (nm.Length > 120) nm = nm[..120];
        return new FreeformTemplate
        {
            Kind = FreeformStore.NormalizeKind(kind), Name = nm, Description = $"نموذج مُحوَّل من ملف {doc.Kind.ToUpperInvariant()} «{Path.GetFileName(fileName)}» — {sections.Count} أقسام",
            Sections = sections, Content = text, SourceFile = Path.GetFileName(fileName), Custom = true,
            Brief = "هذا النموذج مُحوَّل من ملف رفعه المستخدم: التزم بترتيب أقسامه وعناوينه وأسلوبه قدر الإمكان، واستخدم نصوصه وجداوله وأرقامه الواردة في «محتوى الملف» كمحتوى فعلي للصفحة (لا تختلق ما ليس فيه)، وأعد صياغتها بما يطلبه المستخدم.",
        };
    }
}
