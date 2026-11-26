using System.Text.Json.Serialization;
using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Freeform;

/// <summary>تصميم حر: صفحة HTML كاملة يكتبها الموديل (كـ artifacts) لكل مشروع، بإصداراتها ومحادثتها.</summary>
public class FreeformDesign
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("entityId")] public string? EntityId { get; set; }
    [JsonPropertyName("organizationId")] public string? OrganizationId { get; set; }
    [JsonPropertyName("createdByUserId")] public string CreatedByUserId { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    /// <summary>نوع المخرج كما في Claude: doc | slides | design | dashboard | motion.</summary>
    [JsonPropertyName("kind")] public string Kind { get; set; } = "design";
    /// <summary>مفتاح النموذج الذي بُني عليه (من <see cref="FreeformTemplates"/>)؛ null = تصميم حر بلا نموذج.</summary>
    [JsonPropertyName("templateKey")] public string? TemplateKey { get; set; }
    /// <summary>عدد الاستعلامات المسجَّلة التي تغذّي كتلة بيانات الصفحة (0 = لا بيانات حيّة).</summary>
    [JsonPropertyName("queryCount")] public int QueryCount { get; set; }
    /// <summary>آخر تحديث لبيانات الصفحة (بناء أو تحديث بلا موديل).</summary>
    [JsonPropertyName("dataRefreshedAt")] public DateTime? DataRefreshedAt { get; set; }
    /// <summary>بصمة جداول المصدر عند آخر بناء/تحديث (DataFingerprintService)؛ تُقارن عند الفتح.</summary>
    [JsonIgnore] public string? DataFingerprint { get; set; }
    /// <summary>تحديث البيانات تلقائيًا عند فتح التصميم إن تغيّرت بصمة المصدر.</summary>
    [JsonPropertyName("autoRefresh")] public bool AutoRefresh { get; set; }
    /// <summary>المصادر التي اختارها المستخدم لبناء هذا التصميم (JSON بشكل SourceSelection: systems/files)؛ null = كل مصادر المشروع المتاحة له.
    /// تُستخدم في كل بناء/تعديل/تحديث لاحق حتى لا يستعلم الموديل من مصدر لم يُختر.</summary>
    [JsonIgnore] public string? SourcesJson { get; set; }
    [JsonPropertyName("sources")] public Sources.SourceSelection? SourcesSelection
    {
        get { if (string.IsNullOrWhiteSpace(SourcesJson)) return null; try { return System.Text.Json.JsonSerializer.Deserialize<Sources.SourceSelection>(SourcesJson); } catch (System.Text.Json.JsonException) { return null; } }
    }
    [JsonIgnore] public string Html { get; set; } = "";
    /// <summary>هوية المكتبة المختارة لهذا التصميم (<see cref="Decks.BrandIdentityStore"/>)؛ null = الهوية الافتراضية للمشروع. تحدد شعار `data-jeem-logo` وهوية البناء.</summary>
    [JsonPropertyName("identityId")] public string? IdentityId { get; set; }
    /// <summary>الوصول العام للتصميم: private (صاحبه ومن شاركه معهم بالاسم) | project (كل من مشروعه الحالي مشروع التصميم) | org (كل المنظمة).
    /// التصاميم القائمة قبل المشاركة تأخذ project بالتعديل (سلوكها السابق)، والجديدة private.</summary>
    [JsonPropertyName("shareScope")] public string ShareScope { get; set; } = FreeformAccess.Private;
    /// <summary>دور الوصول العام: view | edit.</summary>
    [JsonPropertyName("shareScopeRole")] public string ShareScopeRole { get; set; } = FreeformAccess.View;
    /// <summary>من يُدخل سجلات (<c>jeem.records</c>) في الرابط المنشور: members (افتراضيًا) أو public (<see cref="FreeformRecordPolicy"/>).</summary>
    [JsonPropertyName("recordsPolicy")] public string RecordsPolicy { get; set; } = FreeformRecordPolicy.Members;
    /// <summary>هل تُبنى مجموعات السجلات جداولَ مصدر في «المصادر» (<see cref="FreeformRecordTableService"/>)؟ افتراضيًا نعم.</summary>
    [JsonPropertyName("recordsAsSource")] public bool RecordsAsSource { get; set; } = true;
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

/// <summary>مستخدم شورك معه تصميم بالاسم ودوره.</summary>
public class FreeformShare
{
    [JsonPropertyName("userId")] public string UserId { get; set; } = "";
    [JsonPropertyName("role")] public string Role { get; set; } = FreeformAccess.View;
}

/// <summary>جداول <c>dbo.FreeformDesigns</c> (آخر HTML) و<c>dbo.FreeformVersions</c> (كل إصدار بطلبه) و<c>dbo.FreeformMessages</c>.</summary>
public class FreeformStore
{
    /// <summary>حد صفحة التصميم (رُفع من 4 إلى 16 ميغابايت ليتسع لعرض PowerPoint مرسومًا بصوره المضمّنة).</summary>
    public const int MaxHtmlBytes = 16 * 1024 * 1024;
    public const int MaxVersions = 40;
    private readonly DataStore _db;
    private readonly SemaphoreSlim _schemaGate = new(1, 1);
    private volatile bool _schemaReady;
    public FreeformStore(DataStore db) => _db = db;
    private const string T = "[dbo].[FreeformDesigns]", V = "[dbo].[FreeformVersions]", M = "[dbo].[FreeformMessages]";
    private const string Cols = "Id, EntityId, OrganizationId, CreatedByUserId, Title, Kind, TemplateKey, QueryCount, DataRefreshedAt, DataFingerprint, AutoRefresh, SourcesJson, IdentityId, ShareScope, ShareScopeRole, RecordsPolicy, RecordsAsSource, Version, PresentationId, SizeBytes, CreatedAt, UpdatedAt";
    private const string Q = "[dbo].[FreeformQueries]";
    private const string S = "[dbo].[FreeformShares]";
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
                  [Id] NVARCHAR(32) PRIMARY KEY, [EntityId] NVARCHAR(64), [OrganizationId] NVARCHAR(64), [CreatedByUserId] NVARCHAR(200) NOT NULL,
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
                IF COL_LENGTH('dbo.FreeformDesigns', 'QueryCount') IS NULL ALTER TABLE {T} ADD [QueryCount] INT NOT NULL DEFAULT 0;
                IF COL_LENGTH('dbo.FreeformDesigns', 'DataRefreshedAt') IS NULL ALTER TABLE {T} ADD [DataRefreshedAt] DATETIME2 NULL;
                IF COL_LENGTH('dbo.FreeformDesigns', 'DataFingerprint') IS NULL ALTER TABLE {T} ADD [DataFingerprint] NVARCHAR(100) NULL;
                IF COL_LENGTH('dbo.FreeformDesigns', 'AutoRefresh') IS NULL ALTER TABLE {T} ADD [AutoRefresh] BIT NOT NULL DEFAULT 0;
                IF COL_LENGTH('dbo.FreeformDesigns', 'SourcesJson') IS NULL ALTER TABLE {T} ADD [SourcesJson] NVARCHAR(MAX) NULL;
                IF COL_LENGTH('dbo.FreeformDesigns', 'IdentityId') IS NULL ALTER TABLE {T} ADD [IdentityId] NVARCHAR(32) NULL;
                IF COL_LENGTH('dbo.FreeformDesigns', 'ShareScope') IS NULL ALTER TABLE {T} ADD [ShareScope] NVARCHAR(16) NOT NULL CONSTRAINT DF_FreeformDesigns_ShareScope DEFAULT 'project';
                IF COL_LENGTH('dbo.FreeformDesigns', 'ShareScopeRole') IS NULL ALTER TABLE {T} ADD [ShareScopeRole] NVARCHAR(8) NOT NULL CONSTRAINT DF_FreeformDesigns_ShareScopeRole DEFAULT 'edit';
                IF COL_LENGTH('dbo.FreeformDesigns', 'RecordsPolicy') IS NULL ALTER TABLE {T} ADD [RecordsPolicy] NVARCHAR(16) NOT NULL CONSTRAINT DF_FreeformDesigns_RecordsPolicy DEFAULT 'members';
                IF COL_LENGTH('dbo.FreeformDesigns', 'RecordsAsSource') IS NULL ALTER TABLE {T} ADD [RecordsAsSource] BIT NOT NULL CONSTRAINT DF_FreeformDesigns_RecordsAsSource DEFAULT 1;
                IF OBJECT_ID('dbo.FreeformShares') IS NULL
                CREATE TABLE {S} ([DesignId] NVARCHAR(32) NOT NULL, [UserId] NVARCHAR(200) NOT NULL, [Role] NVARCHAR(8) NOT NULL, [CreatedAt] DATETIME2 NOT NULL, CONSTRAINT PK_FreeformShares PRIMARY KEY ([DesignId], [UserId]));
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FreeformShares_User') CREATE INDEX IX_FreeformShares_User ON {S} ([UserId]);
                IF OBJECT_ID('dbo.FreeformQueries') IS NULL
                CREATE TABLE {Q} ([DesignId] NVARCHAR(32) NOT NULL, [Key] NVARCHAR(16) NOT NULL, [Sql] NVARCHAR(MAX) NOT NULL, [Seq] INT NOT NULL, CONSTRAINT PK_FreeformQueries PRIMARY KEY ([DesignId], [Key]));
                """;
            await command.ExecuteNonQueryAsync(ct);
            _schemaReady = true;
        }
        finally { _schemaGate.Release(); }
    }

    public async Task<FreeformDesign> CreateAsync(string? entityId, string? organizationId, string userId, string title, string html, string? prompt, string? kind = null, string? templateKey = null, CancellationToken ct = default, string? sourcesJson = null)
    {
        await EnsureSchemaAsync(ct);
        var now = DateTime.UtcNow;
        var d = new FreeformDesign { Id = Guid.NewGuid().ToString("N")[..16], EntityId = entityId, OrganizationId = organizationId, CreatedByUserId = userId, Title = title, Kind = NormalizeKind(kind), TemplateKey = string.IsNullOrWhiteSpace(templateKey) ? null : templateKey.Trim(), SourcesJson = sourcesJson, Html = html, Version = html.Length > 0 ? 1 : 0, SizeBytes = System.Text.Encoding.UTF8.GetByteCount(html), CreatedAt = now, UpdatedAt = now };
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"INSERT INTO {T} ({Cols}, Html) VALUES (@Id, @EntityId, @OrganizationId, @CreatedByUserId, @Title, @Kind, @TemplateKey, @QueryCount, @DataRefreshedAt, @DataFingerprint, @AutoRefresh, @SourcesJson, @IdentityId, @ShareScope, @ShareScopeRole, @RecordsPolicy, @RecordsAsSource, @Version, @PresentationId, @SizeBytes, @CreatedAt, @UpdatedAt, @Html)", d);
        if (d.Version == 1) await connection.ExecuteAsync($"INSERT INTO {V} (DesignId, Version, Html, Prompt, SizeBytes, CreatedAt) VALUES (@id, 1, @html, @prompt, @size, @now)", new { id = d.Id, html, prompt = Trim(prompt), size = d.SizeBytes, now });
        return d;
    }

    public async Task<FreeformDesign?> GetAsync(string id, string? entityId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<FreeformDesign>($"SELECT {Cols}, Html FROM {T} WHERE Id = @id AND (EntityId = @entityId OR (@entityId IS NULL AND EntityId IS NULL))", new { id, entityId });
    }

    /// <summary>بالمعرّف وحده بلا تقييد بالمشروع — للبيانات الحيّة للعروض المنشورة (الربط محفوظ على العرض نفسه).</summary>
    public async Task<FreeformDesign?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<FreeformDesign>($"SELECT {Cols}, Html FROM {T} WHERE Id = @id", new { id });
    }

    public async Task<IReadOnlyList<FreeformDesign>> ListAsync(string? entityId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return (await connection.QueryAsync<FreeformDesign>($"SELECT {Cols} FROM {T} WHERE (EntityId = @entityId OR (@entityId IS NULL AND EntityId IS NULL)) ORDER BY UpdatedAt DESC", new { entityId })).ToList();
    }

    /// <summary>التصاميم التي يحق للمستخدم رؤيتها: في كيانه الحالي — كلها للمسؤول، وإلا تصاميمه وما شورك مع الكيان؛ ومن أي كيان في
    /// منظمته — ما شورك مع المنظمة كلها أو معه بالاسم. (قبل تبديل الكيانات كانت كل تصاميم المنظمة تظهر للمسؤول وتصاميم المستخدم في كل كيان.)</summary>
    public async Task<IReadOnlyList<FreeformDesign>> ListVisibleAsync(string? organizationId, string? entityId, string userId, bool isAdmin, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return (await connection.QueryAsync<FreeformDesign>($"""
            SELECT {Cols} FROM {T}
            WHERE ((EntityId = @entityId OR (@entityId IS NULL AND EntityId IS NULL))
                   AND (OrganizationId = @organizationId OR OrganizationId IS NULL)
                   AND (@isAdmin = 1 OR CreatedByUserId = @userId OR ShareScope IN ('project', 'org')
                        OR Id IN (SELECT DesignId FROM {S} WHERE UserId = @userId)))
               OR (OrganizationId = @organizationId
                   AND (ShareScope = 'org' OR Id IN (SELECT DesignId FROM {S} WHERE UserId = @userId)))
            ORDER BY UpdatedAt DESC
            """, new { organizationId, entityId, userId, isAdmin })).ToList();
    }

    /// <summary>أدوار المستخدم في التصاميم المشاركة معه بالاسم (معرّف التصميم ← الدور).</summary>
    public async Task<Dictionary<string, string>> ShareRolesForUserAsync(string userId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<(string DesignId, string Role)>($"SELECT DesignId, Role FROM {S} WHERE UserId = @userId", new { userId });
        return rows.ToDictionary(r => r.DesignId, r => r.Role, StringComparer.Ordinal);
    }

    public async Task<string?> ShareRoleAsync(string designId, string userId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<string?>($"SELECT Role FROM {S} WHERE DesignId = @designId AND UserId = @userId", new { designId, userId });
    }

    public async Task<IReadOnlyList<FreeformShare>> SharesAsync(string designId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return (await connection.QueryAsync<FreeformShare>($"SELECT UserId, Role FROM {S} WHERE DesignId = @designId ORDER BY CreatedAt", new { designId })).ToList();
    }

    /// <summary>عدد من شورك معهم كل تصميم بالاسم (لشارة المشاركة في المعرض).</summary>
    public async Task<Dictionary<string, int>> ShareCountsAsync(IEnumerable<string> designIds, CancellationToken ct = default)
    {
        var ids = designIds.Distinct().ToList(); if (ids.Count == 0) return new();
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<(string DesignId, int N)>($"SELECT DesignId, COUNT(*) AS N FROM {S} WHERE DesignId IN @ids GROUP BY DesignId", new { ids });
        return rows.ToDictionary(r => r.DesignId, r => r.N, StringComparer.Ordinal);
    }

    /// <summary>يستبدل إعدادات المشاركة كلها (الوصول العام + قائمة المستخدمين) في معاملة واحدة.</summary>
    public async Task SetSharingAsync(string designId, string scope, string scopeRole, IReadOnlyList<FreeformShare> shares, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {T} SET ShareScope = @scope, ShareScopeRole = @scopeRole WHERE Id = @designId", new { designId, scope, scopeRole }, tx);
        await connection.ExecuteAsync($"DELETE FROM {S} WHERE DesignId = @designId", new { designId }, tx);
        var now = DateTime.UtcNow;
        foreach (var sh in shares)
            await connection.ExecuteAsync($"INSERT INTO {S} (DesignId, UserId, Role, CreatedAt) VALUES (@designId, @UserId, @Role, @now)", new { designId, sh.UserId, sh.Role, now }, tx);
        await tx.CommitAsync(ct);
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

    /// <summary>يستبدل الاستعلامات المسجَّلة للتصميم (مفتاح + SQL) ويحدّث العدّاد ووقت آخر تحديث للبيانات.</summary>
    public async Task SetQueriesAsync(string id, IReadOnlyList<FreeformQuery> queries, DateTime refreshedAt, string? fingerprint = null, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"DELETE FROM {Q} WHERE DesignId = @id", new { id });
        var seq = 0;
        foreach (var q in queries) await connection.ExecuteAsync($"INSERT INTO {Q} (DesignId, [Key], [Sql], Seq) VALUES (@id, @key, @sql, @seq)", new { id, key = q.Key, sql = q.Sql, seq = seq++ });
        await connection.ExecuteAsync($"UPDATE {T} SET QueryCount = @n, DataRefreshedAt = @at, DataFingerprint = @fp WHERE Id = @id", new { id, n = queries.Count, at = refreshedAt, fp = fingerprint });
    }

    /// <summary>يحفظ المصادر المختارة للتصميم (null = الكل).</summary>
    public async Task SetSourcesAsync(string id, string? sourcesJson, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {T} SET SourcesJson = @sourcesJson WHERE Id = @id", new { id, sourcesJson });
    }

    public async Task SetRecordsPolicyAsync(string id, string policy, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {T} SET RecordsPolicy = @policy WHERE Id = @id", new { id, policy = FreeformRecordPolicy.Normalize(policy) });
    }

    public async Task SetRecordsAsSourceAsync(string id, bool on, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {T} SET RecordsAsSource = @on WHERE Id = @id", new { id, on });
    }

    public async Task SetIdentityAsync(string id, string? identityId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {T} SET IdentityId = @identityId WHERE Id = @id", new { id, identityId });
    }

    public async Task SetAutoRefreshAsync(string id, bool on, CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {T} SET AutoRefresh = @on WHERE Id = @id", new { id, on });
    }

    public async Task<IReadOnlyList<FreeformQuery>> QueriesAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return (await connection.QueryAsync<(string Key, string Sql)>($"SELECT [Key], [Sql] FROM {Q} WHERE DesignId = @id ORDER BY Seq", new { id })).Select(r => new FreeformQuery(r.Key, r.Sql, null)).ToList();
    }

    public async Task TouchDataAsync(string id, DateTime refreshedAt, CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {T} SET DataRefreshedAt = @at WHERE Id = @id", new { id, at = refreshedAt });
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

    /// <summary>يغيّر نوع التصميم (مثلًا عند استيراد ملف PowerPoint إلى تصميم قائم فيصير صفحة شرائح).</summary>
    public async Task SetKindAsync(string id, string kind, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"UPDATE {T} SET Kind = @kind, UpdatedAt = @now WHERE Id = @id", new { id, kind = NormalizeKind(kind), now = DateTime.UtcNow });
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
        await connection.ExecuteAsync($"DELETE FROM {M} WHERE DesignId = @id; DELETE FROM {V} WHERE DesignId = @id; DELETE FROM {S} WHERE DesignId = @id; DELETE FROM {T} WHERE Id = @id", new { id });
    }

    private static string? Trim(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Length > 2000 ? s[..2000] : s;
}
