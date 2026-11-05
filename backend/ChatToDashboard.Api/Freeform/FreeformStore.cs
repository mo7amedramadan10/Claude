using System.Text.Json.Serialization;
using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Freeform;

/// <summary>تصميم حر: صفحة HTML كاملة يكتبها الموديل (كـ artifacts) لكل مشروع، بإصداراتها ومحادثتها.</summary>
public class FreeformDesign
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("projectId")] public string? ProjectId { get; set; }
    [JsonPropertyName("organizationId")] public string? OrganizationId { get; set; }
    [JsonPropertyName("createdByUserId")] public string CreatedByUserId { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    /// <summary>نوع المخرج كما في Claude: doc | slides | design | dashboard | motion.</summary>
    [JsonPropertyName("kind")] public string Kind { get; set; } = "design";
    /// <summary>مفتاح النموذج الذي بُني عليه (من <see cref="FreeformTemplates"/>)؛ null = تصميم حر بلا نموذج.</summary>
    [JsonPropertyName("templateKey")] public string? TemplateKey { get; set; }
    [JsonIgnore] public string Html { get; set; } = "";
    [JsonPropertyName("version")] public int Version { get; set; }
    [JsonPropertyName("presentationId")] public string? PresentationId { get; set; }
    [JsonPropertyName("sizeBytes")] public int SizeBytes { get; set; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updatedAt")] public DateTime UpdatedAt { get; set; }
}

public class FreeformVersion
{
    [JsonPropertyName("version")] public int Version { get; set; }
    [JsonPropertyName("prompt")] public string? Prompt { get; set; }
    [JsonPropertyName("sizeBytes")] public int SizeBytes { get; set; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }
}

public class FreeformMessage
{
    [JsonPropertyName("role")] public string Role { get; set; } = "user";
    [JsonPropertyName("text")] public string Text { get; set; } = "";
    [JsonPropertyName("version")] public int? Version { get; set; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }
}

/// <summary>جداول <c>dbo.FreeformDesigns</c> (آخر HTML) و<c>dbo.FreeformVersions</c> (كل إصدار بطلبه) و<c>dbo.FreeformMessages</c>.</summary>
public class FreeformStore
{
    public const int MaxHtmlBytes = 4 * 1024 * 1024;
    public const int MaxVersions = 40;
    private readonly DataStore _db;
    private readonly SemaphoreSlim _schemaGate = new(1, 1);
    private volatile bool _schemaReady;
    public FreeformStore(DataStore db) => _db = db;
    private const string T = "[dbo].[FreeformDesigns]", V = "[dbo].[FreeformVersions]", M = "[dbo].[FreeformMessages]";
    private const string Cols = "Id, ProjectId, OrganizationId, CreatedByUserId, Title, Kind, TemplateKey, Version, PresentationId, SizeBytes, CreatedAt, UpdatedAt";
    public static readonly string[] Kinds = { "doc", "slides", "design", "dashboard", "motion" };
    public static string NormalizeKind(string? k) => Kinds.Contains(k ?? "") ? k! : "design";

    private async Task EnsureSchemaAsync(CancellationToken ct)
    {
        if (_schemaReady) return;
        await _schemaGate.WaitAsync(ct);
        try
        {
            if (_schemaReady) return;
            await using var connection = await _db.OpenConnectionAsync(ct);
            await _db.CreateContainerIfMissingAsync(connection, ct);
            await using var command = connection.CreateCommand();
            command.CommandText = $"""
                IF OBJECT_ID('dbo.FreeformDesigns') IS NULL
                CREATE TABLE {T} (
                  [Id] NVARCHAR(32) PRIMARY KEY, [ProjectId] NVARCHAR(64), [OrganizationId] NVARCHAR(64), [CreatedByUserId] NVARCHAR(200) NOT NULL,
                  [Title] NVARCHAR(300) NOT NULL, [Kind] NVARCHAR(16) NOT NULL DEFAULT 'design', [Html] NVARCHAR(MAX) NOT NULL, [Version] INT NOT NULL DEFAULT 0, [PresentationId] NVARCHAR(32),
                  [SizeBytes] INT NOT NULL DEFAULT 0, [CreatedAt] DATETIME2 NOT NULL, [UpdatedAt] DATETIME2 NOT NULL);
                IF OBJECT_ID('dbo.FreeformVersions') IS NULL
                CREATE TABLE {V} (
                  [DesignId] NVARCHAR(32) NOT NULL, [Version] INT NOT NULL, [Html] NVARCHAR(MAX) NOT NULL, [Prompt] NVARCHAR(2000), [SizeBytes] INT NOT NULL,
                  [CreatedAt] DATETIME2 NOT NULL, CONSTRAINT PK_FreeformVersions PRIMARY KEY ([DesignId], [Version]));
                IF OBJECT_ID('dbo.FreeformMessages') IS NULL
                CREATE TABLE {M} (
                  [Id] INT IDENTITY PRIMARY KEY, [DesignId] NVARCHAR(32) NOT NULL, [Role] NVARCHAR(12) NOT NULL, [Text] NVARCHAR(MAX) NOT NULL,
                  [Version] INT NULL, [CreatedAt] DATETIME2 NOT NULL);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FreeformMessages_Design') CREATE INDEX IX_FreeformMessages_Design ON {M} ([DesignId], [Id]);
                IF COL_LENGTH('dbo.FreeformDesigns', 'Kind') IS NULL ALTER TABLE {T} ADD [Kind] NVARCHAR(16) NOT NULL DEFAULT 'design';
                IF COL_LENGTH('dbo.FreeformDesigns', 'TemplateKey') IS NULL ALTER TABLE {T} ADD [TemplateKey] NVARCHAR(60) NULL;
                """;
            await command.ExecuteNonQueryAsync(ct);
            _schemaReady = true;
        }
        finally { _schemaGate.Release(); }
    }

    public async Task<FreeformDesign> CreateAsync(string? projectId, string? organizationId, string userId, string title, string html, string? prompt, string? kind = null, string? templateKey = null, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        var now = DateTime.UtcNow;
        var d = new FreeformDesign { Id = Guid.NewGuid().ToString("N")[..16], ProjectId = projectId, OrganizationId = organizationId, CreatedByUserId = userId, Title = title, Kind = NormalizeKind(kind), TemplateKey = string.IsNullOrWhiteSpace(templateKey) ? null : templateKey.Trim(), Html = html, Version = html.Length > 0 ? 1 : 0, SizeBytes = System.Text.Encoding.UTF8.GetByteCount(html), CreatedAt = now, UpdatedAt = now };
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"INSERT INTO {T} ({Cols}, Html) VALUES (@Id, @ProjectId, @OrganizationId, @CreatedByUserId, @Title, @Kind, @TemplateKey, @Version, @PresentationId, @SizeBytes, @CreatedAt, @UpdatedAt, @Html)", d);
        if (d.Version == 1) await connection.ExecuteAsync($"INSERT INTO {V} (DesignId, Version, Html, Prompt, SizeBytes, CreatedAt) VALUES (@id, 1, @html, @prompt, @size, @now)", new { id = d.Id, html, prompt = Trim(prompt), size = d.SizeBytes, now });
        return d;
    }

    public async Task<FreeformDesign?> GetAsync(string id, string? projectId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<FreeformDesign>($"SELECT {Cols}, Html FROM {T} WHERE Id = @id AND (ProjectId = @projectId OR (@projectId IS NULL AND ProjectId IS NULL))", new { id, projectId });
    }

    public async Task<IReadOnlyList<FreeformDesign>> ListAsync(string? projectId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return (await connection.QueryAsync<FreeformDesign>($"SELECT {Cols} FROM {T} WHERE (ProjectId = @projectId OR (@projectId IS NULL AND ProjectId IS NULL)) ORDER BY UpdatedAt DESC", new { projectId })).ToList();
    }

    /// <summary>يحفظ HTML جديدًا كإصدار تالٍ (ويحذف الأقدم فوق الحد) ويعيد رقم الإصدار.</summary>
    public async Task<int> AddVersionAsync(string id, string html, string? prompt, string? title, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var now = DateTime.UtcNow; var size = System.Text.Encoding.UTF8.GetByteCount(html);
        var v = await connection.ExecuteScalarAsync<int>($"UPDATE {T} SET Version = Version + 1, Html = @html, SizeBytes = @size, UpdatedAt = @now, Title = COALESCE(@title, Title) OUTPUT INSERTED.Version WHERE Id = @id", new { id, html, size, now, title = Trim(title) });
        await connection.ExecuteAsync($"INSERT INTO {V} (DesignId, Version, Html, Prompt, SizeBytes, CreatedAt) VALUES (@id, @v, @html, @prompt, @size, @now)", new { id, v, html, prompt = Trim(prompt), size, now });
        await connection.ExecuteAsync($"DELETE FROM {V} WHERE DesignId = @id AND Version <= @v - @max", new { id, v, max = MaxVersions });
        return v;
    }

    public async Task<IReadOnlyList<FreeformVersion>> VersionsAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return (await connection.QueryAsync<FreeformVersion>($"SELECT Version, Prompt, SizeBytes, CreatedAt FROM {V} WHERE DesignId = @id ORDER BY Version DESC", new { id })).ToList();
    }

    public async Task<string?> VersionHtmlAsync(string id, int version, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<string>($"SELECT Html FROM {V} WHERE DesignId = @id AND Version = @version", new { id, version });
    }

    public async Task SetTitleAsync(string id, string title, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {T} SET Title = @title, UpdatedAt = @now WHERE Id = @id", new { id, title, now = DateTime.UtcNow });
    }

    public async Task SetPresentationAsync(string id, string presentationId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {T} SET PresentationId = @presentationId WHERE Id = @id", new { id, presentationId });
    }

    public async Task AddMessageAsync(string id, string role, string text, int? version, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"INSERT INTO {M} (DesignId, Role, Text, Version, CreatedAt) VALUES (@id, @role, @text, @version, @now)", new { id, role, text = text.Length > 20000 ? text[..20000] : text, version, now = DateTime.UtcNow });
    }

    public async Task<IReadOnlyList<FreeformMessage>> MessagesAsync(string id, int take = 60, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<FreeformMessage>($"SELECT TOP (@take) Role, Text, Version, CreatedAt FROM {M} WHERE DesignId = @id ORDER BY Id DESC", new { id, take });
        return rows.Reverse().ToList();
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"DELETE FROM {M} WHERE DesignId = @id; DELETE FROM {V} WHERE DesignId = @id; DELETE FROM {T} WHERE Id = @id", new { id });
    }

    private static string? Trim(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Length > 2000 ? s[..2000] : s;
}
