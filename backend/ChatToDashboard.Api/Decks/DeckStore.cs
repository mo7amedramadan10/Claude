using System.Text.Json;
using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Decks;

/// <summary>عروض «منشئ العروض» لكل مشروع: جدول <c>dbo.Decks</c> بالمواصفة كاملة (JSON).</summary>
public class DeckStore
{
    private readonly DataStore _db;
    private readonly SemaphoreSlim _schemaGate = new(1, 1);
    private volatile bool _schemaReady;
    public DeckStore(DataStore db) => _db = db;
    private string Table => "[dbo].[Decks]";
    private const string Columns = "Id, ProjectId, OrganizationId, CreatedByUserId, Title, TemplateKey, PresentationId, SlideCount, DataFingerprint, DataRefreshedAt, AutoRefresh, CreatedAt, UpdatedAt";

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
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            IF OBJECT_ID('dbo.Decks') IS NULL
            CREATE TABLE {Table} (
              [Id] NVARCHAR(32) PRIMARY KEY, [ProjectId] NVARCHAR(64), [OrganizationId] NVARCHAR(64), [CreatedByUserId] NVARCHAR(200) NOT NULL,
              [Title] NVARCHAR(300) NOT NULL, [SpecJson] NVARCHAR(MAX) NOT NULL, [TemplateKey] NVARCHAR(100), [PresentationId] NVARCHAR(32),
              [SlideCount] INT NOT NULL DEFAULT 0, [CreatedAt] DATETIME2 NOT NULL, [UpdatedAt] DATETIME2 NOT NULL);
            IF COL_LENGTH('dbo.Decks', 'DataFingerprint') IS NULL ALTER TABLE {Table} ADD [DataFingerprint] NVARCHAR(100) NULL;
            IF COL_LENGTH('dbo.Decks', 'DataRefreshedAt') IS NULL ALTER TABLE {Table} ADD [DataRefreshedAt] DATETIME2 NULL;
            IF COL_LENGTH('dbo.Decks', 'AutoRefresh') IS NULL ALTER TABLE {Table} ADD [AutoRefresh] BIT NOT NULL DEFAULT 0;
            """;
        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task<DeckRecord> CreateAsync(string? projectId, string? organizationId, string userId, DeckSpec spec, string? templateKey, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        var now = DateTime.UtcNow;
        var r = new DeckRecord
        {
            Id = Guid.NewGuid().ToString("N")[..16], ProjectId = projectId, OrganizationId = organizationId, CreatedByUserId = userId,
            Title = string.IsNullOrWhiteSpace(spec.Title) ? "عرض بلا عنوان" : spec.Title, SpecJson = JsonSerializer.Serialize(spec, DeckSpec.Json),
            TemplateKey = templateKey, SlideCount = spec.Slides.Count, CreatedAt = now, UpdatedAt = now,
        };
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"INSERT INTO {Table} ({Columns}, SpecJson) VALUES (@Id, @ProjectId, @OrganizationId, @CreatedByUserId, @Title, @TemplateKey, @PresentationId, @SlideCount, @DataFingerprint, @DataRefreshedAt, @AutoRefresh, @CreatedAt, @UpdatedAt, @SpecJson)", r);
        return r;
    }

    public async Task<DeckRecord?> GetAsync(string id, string? projectId, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<DeckRecord>($"SELECT {Columns}, SpecJson FROM {Table} WHERE Id = @id AND (ProjectId = @projectId OR (@projectId IS NULL AND ProjectId IS NULL))", new { id, projectId });
    }

    public async Task<IReadOnlyList<DeckRecord>> ListAsync(string? projectId, CancellationToken ct = default)
    {
        await EnsureSchemaOnceAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return (await connection.QueryAsync<DeckRecord>($"SELECT {Columns} FROM {Table} WHERE ProjectId = @projectId OR (@projectId IS NULL AND ProjectId IS NULL) ORDER BY UpdatedAt DESC", new { projectId })).ToList();
    }

    public async Task UpdateSpecAsync(string id, DeckSpec spec, CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {Table} SET SpecJson = @json, Title = @title, SlideCount = @count, UpdatedAt = @now WHERE Id = @id",
            new { id, json = JsonSerializer.Serialize(spec, DeckSpec.Json), title = string.IsNullOrWhiteSpace(spec.Title) ? "عرض بلا عنوان" : spec.Title, count = spec.Slides.Count, now = DateTime.UtcNow });
    }

    public async Task SetDataStateAsync(string id, string? fingerprint, DateTime refreshedAt, CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {Table} SET DataFingerprint = @fp, DataRefreshedAt = @at WHERE Id = @id", new { id, fp = fingerprint, at = refreshedAt });
    }

    public async Task SetAutoRefreshAsync(string id, bool on, CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {Table} SET AutoRefresh = @on WHERE Id = @id", new { id, on });
    }

    public async Task SetPresentationAsync(string id, string presentationId, CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {Table} SET PresentationId = @presentationId WHERE Id = @id", new { id, presentationId });
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"DELETE FROM {Table} WHERE Id = @id", new { id });
    }
}
