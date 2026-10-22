using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Projects;

/// <summary>
/// تخزين جدول <c>PublishedDashboards</c> (§4.23) — بنفس نمط Dapper المشترك لبقية المخازن
/// هنا (SQLite أو SQL Server تحت مخطط <c>staging</c>). صف واحد لكل (مشروع، لوحة): إعادة
/// النشر تحدّث الصف نفسه وترفع رقم الإصدار بدل إضافة صف جديد.
/// </summary>
public class PublishedDashboardStore
{
    private readonly DataStore _db;

    public PublishedDashboardStore(DataStore db) => _db = db;

    private string Table => _db.Provider == DbProvider.Sqlite
        ? "\"PublishedDashboards\""
        : "[staging].[PublishedDashboards]";

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);

        var text = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {Table} (
                 "Id" TEXT PRIMARY KEY, "ProjectId" TEXT NOT NULL, "DashboardId" TEXT NOT NULL,
                 "Version" INTEGER NOT NULL, "PublishedBy" TEXT, "PublishedAt" TEXT,
                 "Audience" TEXT NOT NULL, "RefreshMode" TEXT NOT NULL, "RefreshEvery" TEXT,
                 "SortOrder" INTEGER NOT NULL, "Kind" TEXT NOT NULL,
                 UNIQUE ("ProjectId", "DashboardId"))
               """
            : $"""
               IF OBJECT_ID('staging.PublishedDashboards') IS NULL
               CREATE TABLE {Table} (
                 [Id] NVARCHAR(64) PRIMARY KEY, [ProjectId] NVARCHAR(64) NOT NULL, [DashboardId] NVARCHAR(64) NOT NULL,
                 [Version] INT NOT NULL, [PublishedBy] NVARCHAR(200), [PublishedAt] DATETIME2,
                 [Audience] NVARCHAR(20) NOT NULL, [RefreshMode] NVARCHAR(20) NOT NULL, [RefreshEvery] NVARCHAR(100),
                 [SortOrder] INT NOT NULL, [Kind] NVARCHAR(20) NOT NULL,
                 CONSTRAINT UQ_PublishedDashboards_Project_Dashboard UNIQUE ([ProjectId], [DashboardId]))
               """;

        await using var command = connection.CreateCommand();
        command.CommandText = text;
        await command.ExecuteNonQueryAsync(ct);
    }

    /// <summary>كل اللوحات المنشورة على المشروع بترتيب التبويبات (بلا تصفية جمهور — هذه
    /// مسؤولية المتحكم).</summary>
    public async Task<IReadOnlyList<PublishedDashboard>> ListAsync(string projectId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<PublishedDashboard>(
            $"SELECT * FROM {Table} WHERE ProjectId = @projectId ORDER BY SortOrder, PublishedAt",
            new { projectId });
        return rows.ToList();
    }

    public async Task<PublishedDashboard?> GetAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<PublishedDashboard>(
            $"SELECT * FROM {Table} WHERE Id = @id", new { id });
    }

    /// <summary>كل نشرات لوحة واحدة (على أي مشروع) — للتحقق من أحقية مشاهدة لوحة منشورة
    /// عند تحديث عناصرها.</summary>
    public async Task<IReadOnlyList<PublishedDashboard>> ListForDashboardAsync(string dashboardId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<PublishedDashboard>(
            $"SELECT * FROM {Table} WHERE DashboardId = @dashboardId", new { dashboardId });
        return rows.ToList();
    }

    /// <summary>
    /// ينشر اللوحة على المشروع: صف جديد بالإصدار 1 في آخر الترتيب، أو — إن كانت منشورة
    /// مسبقًا — يحدّث الجمهور ونوع التحديث والناشر والتاريخ ويرفع الإصدار، مع الإبقاء على
    /// موضعها في الترتيب.
    /// </summary>
    public async Task<PublishedDashboard> PublishAsync(
        string projectId, string dashboardId, string kind, string publishedBy,
        string audience, string refreshMode, string? refreshEvery, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var now = DateTime.UtcNow;

        var existing = await connection.QuerySingleOrDefaultAsync<PublishedDashboard>(
            $"SELECT * FROM {Table} WHERE ProjectId = @projectId AND DashboardId = @dashboardId",
            new { projectId, dashboardId });
        if (existing is not null)
        {
            existing.Version += 1;
            existing.PublishedBy = publishedBy;
            existing.PublishedAt = now;
            existing.Audience = audience;
            existing.RefreshMode = refreshMode;
            existing.RefreshEvery = refreshEvery;
            existing.Kind = kind;
            await connection.ExecuteAsync(
                $"UPDATE {Table} SET Version = @Version, PublishedBy = @PublishedBy, PublishedAt = @PublishedAt, " +
                "Audience = @Audience, RefreshMode = @RefreshMode, RefreshEvery = @RefreshEvery, Kind = @Kind " +
                "WHERE Id = @Id",
                existing);
            return existing;
        }

        var nextOrder = await connection.ExecuteScalarAsync<int?>(
            $"SELECT MAX(SortOrder) FROM {Table} WHERE ProjectId = @projectId", new { projectId }) ?? -1;
        var row = new PublishedDashboard
        {
            Id = Guid.NewGuid().ToString("N"),
            ProjectId = projectId,
            DashboardId = dashboardId,
            Version = 1,
            PublishedBy = publishedBy,
            PublishedAt = now,
            Audience = audience,
            RefreshMode = refreshMode,
            RefreshEvery = refreshEvery,
            SortOrder = nextOrder + 1,
            Kind = kind,
        };
        await connection.ExecuteAsync(
            $"INSERT INTO {Table} (Id, ProjectId, DashboardId, Version, PublishedBy, PublishedAt, Audience, " +
            "RefreshMode, RefreshEvery, SortOrder, Kind) VALUES (@Id, @ProjectId, @DashboardId, @Version, " +
            "@PublishedBy, @PublishedAt, @Audience, @RefreshMode, @RefreshEvery, @SortOrder, @Kind)",
            row);
        return row;
    }

    /// <summary>يحفظ ترتيبًا كاملًا للمشروع: الموضع في القائمة يصبح <c>SortOrder</c>.</summary>
    public async Task SaveOrderAsync(string projectId, IReadOnlyList<string> orderedIds, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        for (var i = 0; i < orderedIds.Count; i++)
            await connection.ExecuteAsync(
                $"UPDATE {Table} SET SortOrder = @order WHERE Id = @id AND ProjectId = @projectId",
                new { order = i, id = orderedIds[i], projectId });
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"DELETE FROM {Table} WHERE Id = @id", new { id });
    }
}
