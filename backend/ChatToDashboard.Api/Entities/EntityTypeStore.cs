using System.Text.Json.Serialization;
using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Entities;

/// <summary>
/// نوع الكيان (مشروع، قسم، إدارة، فريق…): الكيان هو الوحدة التي تُنظَّم تحتها المحادثة واللوحات والتصاميم والملفات والصلاحيات
/// داخل المنظمة، واسمه يختلف من جهة لأخرى. لكل منظمة أنواعها، ولكل كيان نوع واحد (<c>Projects.TypeId</c>؛ null = النوع الافتراضي).
/// الاسم يُعرض للمستخدم بصيغتيه (المفرد والجمع) وبجنسه لتتفق الصفات معه («القسم الحالي» / «الإدارة الحالية»).
/// </summary>
public class EntityType
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("organizationId")] public string OrganizationId { get; set; } = "";
    [JsonPropertyName("singular")] public string Singular { get; set; } = "";
    [JsonPropertyName("plural")] public string Plural { get; set; } = "";
    /// <summary>الاسم في الواجهة الإنجليزية (Department / Departments)؛ null = «Entity/Entities».</summary>
    [JsonPropertyName("singularEn")] public string? SingularEn { get; set; }
    [JsonPropertyName("pluralEn")] public string? PluralEn { get; set; }
    /// <summary>مفتاح أيقونة من icons.js (layers، briefcase، users…).</summary>
    [JsonPropertyName("icon")] public string Icon { get; set; } = "layers";
    /// <summary>مؤنث؟ لاتفاق الصفات («الإدارة الحالية»).</summary>
    [JsonPropertyName("feminine")] public bool Feminine { get; set; }
    /// <summary>النوع الافتراضي للمنظمة: يأخذه كل كيان بلا نوع صريح، ولا يُحذف.</summary>
    [JsonPropertyName("isDefault")] public bool IsDefault { get; set; }
    [JsonPropertyName("sortOrder")] public int SortOrder { get; set; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }
}

public class EntityTypeRequest
{
    [JsonPropertyName("singular")] public string? Singular { get; set; }
    [JsonPropertyName("plural")] public string? Plural { get; set; }
    [JsonPropertyName("singularEn")] public string? SingularEn { get; set; }
    [JsonPropertyName("pluralEn")] public string? PluralEn { get; set; }
    [JsonPropertyName("icon")] public string? Icon { get; set; }
    [JsonPropertyName("feminine")] public bool? Feminine { get; set; }
    [JsonPropertyName("isDefault")] public bool? IsDefault { get; set; }
}

public class EntityTypeStore
{
    public const int MaxPerOrganization = 20;
    public const int MaxNameLength = 40;
    /// <summary>الأيقونات المسموحة (من icons.js) — لا يُخزَّن أي اسم آخر.</summary>
    public static readonly string[] Icons = { "layers", "briefcase", "users", "grid", "target", "server", "dashboard", "shield", "chart", "star", "pin", "calendar" };

    private readonly DataStore _db;
    public EntityTypeStore(DataStore db) => _db = db;

    private const string Table = "[dbo].[EntityTypes]";
    private bool _schemaReady;

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        if (_schemaReady) return;
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"""
            IF OBJECT_ID('dbo.EntityTypes') IS NULL
            CREATE TABLE {Table} (
              [Id] NVARCHAR(64) NOT NULL PRIMARY KEY, [OrganizationId] NVARCHAR(64) NOT NULL,
              [Singular] NVARCHAR(60) NOT NULL, [Plural] NVARCHAR(60) NOT NULL, [Icon] NVARCHAR(30) NOT NULL,
              [Feminine] BIT NOT NULL DEFAULT 0, [IsDefault] BIT NOT NULL DEFAULT 0, [SortOrder] INT NOT NULL DEFAULT 0,
              [CreatedAt] DATETIME2 NOT NULL, [SingularEn] NVARCHAR(60) NULL, [PluralEn] NVARCHAR(60) NULL)
            """);
        // الاسمان الإنجليزيان أُضيفا بعد إنشاء الجدول في بعض القواعد
        await connection.ExecuteAsync($"""
            IF COL_LENGTH('dbo.EntityTypes', 'SingularEn') IS NULL ALTER TABLE {Table} ADD [SingularEn] NVARCHAR(60) NULL;
            IF COL_LENGTH('dbo.EntityTypes', 'PluralEn') IS NULL ALTER TABLE {Table} ADD [PluralEn] NVARCHAR(60) NULL;
            """);
        _schemaReady = true;
    }

    /// <summary>أنواع المنظمة بترتيبها، والافتراضي أولًا. منظمة بلا أنواع يُنشأ لها «مشروع / المشاريع» افتراضيًا (سلوك المنصة قبل الأنواع).</summary>
    public async Task<IReadOnlyList<EntityType>> ListAsync(string organizationId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = (await connection.QueryAsync<EntityType>(
            $"SELECT * FROM {Table} WHERE OrganizationId = @organizationId ORDER BY IsDefault DESC, SortOrder, CreatedAt", new { organizationId })).ToList();
        if (rows.Count > 0) return rows;
        var def = new EntityType
        {
            Id = Guid.NewGuid().ToString("N"), OrganizationId = organizationId, Singular = "مشروع", Plural = "مشاريع", SingularEn = "Project", PluralEn = "Projects", Icon = "layers",
            Feminine = false, IsDefault = true, SortOrder = 0, CreatedAt = DateTime.UtcNow,
        };
        // إدراج آمن للتزامن: لا يُنشأ افتراضيان لو طُلبت القائمة مرتين في اللحظة نفسها
        await connection.ExecuteAsync($"""
            IF NOT EXISTS (SELECT 1 FROM {Table} WITH (UPDLOCK, HOLDLOCK) WHERE OrganizationId = @OrganizationId)
            INSERT INTO {Table} (Id, OrganizationId, Singular, Plural, SingularEn, PluralEn, Icon, Feminine, IsDefault, SortOrder, CreatedAt)
            VALUES (@Id, @OrganizationId, @Singular, @Plural, @SingularEn, @PluralEn, @Icon, @Feminine, @IsDefault, @SortOrder, @CreatedAt)
            """, def);
        return (await connection.QueryAsync<EntityType>(
            $"SELECT * FROM {Table} WHERE OrganizationId = @organizationId ORDER BY IsDefault DESC, SortOrder, CreatedAt", new { organizationId })).ToList();
    }

    public async Task<EntityType?> FindAsync(string organizationId, string id, CancellationToken ct = default)
        => (await ListAsync(organizationId, ct)).FirstOrDefault(t => t.Id == id);

    /// <summary>نوع الكيان الفعلي: نوعه الصريح إن كان من أنواع منظمته، وإلا الافتراضي.</summary>
    public static EntityType? Resolve(IReadOnlyList<EntityType> types, string? typeId)
        => types.FirstOrDefault(t => typeId is not null && t.Id == typeId) ?? types.FirstOrDefault(t => t.IsDefault) ?? types.FirstOrDefault();

    /// <summary>تحقق وتطبيع الطلب: الاسمان مطلوبان بطول معقول، والجمع يُشتق من المفرد إن غاب، والجنس من التاء المربوطة إن لم يُحدَّد.</summary>
    internal static string? Validate(EntityTypeRequest r, EntityType target, bool creating)
    {
        var singular = (r.Singular ?? (creating ? "" : target.Singular)).Trim();
        var plural = (r.Plural ?? (creating ? "" : target.Plural)).Trim();
        if (singular.Length == 0) return "اسم النوع (المفرد) مطلوب.";
        singular = StripArticle(singular);
        plural = StripArticle(plural.Length == 0 ? singular : plural);
        if (singular.Length > MaxNameLength || plural.Length > MaxNameLength) return $"الاسم أطول من {MaxNameLength} حرفًا.";
        target.Singular = singular;
        target.Plural = plural;
        // الإنجليزي اختياري: المرسل يُطبَّع (فارغ = بلا اسم)، والجمع يُشتق بـ s إن أُرسل المفرد وحده
        if (r.SingularEn is not null || r.PluralEn is not null || creating)
        {
            var sEn = (r.SingularEn ?? (creating ? null : target.SingularEn))?.Trim();
            var pEn = (r.PluralEn ?? (creating ? null : target.PluralEn))?.Trim();
            if (string.IsNullOrEmpty(sEn)) { sEn = null; pEn = string.IsNullOrEmpty(pEn) ? null : pEn; }
            else if (string.IsNullOrEmpty(pEn)) pEn = sEn + "s";
            if ((sEn?.Length ?? 0) > MaxNameLength || (pEn?.Length ?? 0) > MaxNameLength) return $"الاسم الإنجليزي أطول من {MaxNameLength} حرفًا.";
            target.SingularEn = sEn;
            target.PluralEn = sEn is null ? null : pEn;
        }
        if (r.Icon is not null) target.Icon = Icons.Contains(r.Icon) ? r.Icon : "layers";
        if (creating && string.IsNullOrEmpty(target.Icon)) target.Icon = "layers";
        target.Feminine = r.Feminine ?? (creating || r.Singular is not null ? LooksFeminine(singular) : target.Feminine);
        return null;
    }

    /// <summary>الاسم يُخزَّن نكرة («قسم» لا «القسم») — أداة التعريف تضيفها الواجهة بحسب الموضع.</summary>
    internal static string StripArticle(string s) => s.StartsWith("ال", StringComparison.Ordinal) && s.Length > 3 ? s[2..] : s;

    internal static bool LooksFeminine(string singular) => singular.EndsWith('ة');

    public async Task<EntityType> CreateAsync(string organizationId, EntityType t, CancellationToken ct = default)
    {
        var existing = await ListAsync(organizationId, ct);
        t.Id = Guid.NewGuid().ToString("N");
        t.OrganizationId = organizationId;
        t.SortOrder = existing.Count == 0 ? 0 : existing.Max(x => x.SortOrder) + 1;
        t.CreatedAt = DateTime.UtcNow;
        await using var connection = await _db.OpenConnectionAsync(ct);
        if (t.IsDefault) await connection.ExecuteAsync($"UPDATE {Table} SET IsDefault = 0 WHERE OrganizationId = @organizationId", new { organizationId });
        await connection.ExecuteAsync($"""
            INSERT INTO {Table} (Id, OrganizationId, Singular, Plural, SingularEn, PluralEn, Icon, Feminine, IsDefault, SortOrder, CreatedAt)
            VALUES (@Id, @OrganizationId, @Singular, @Plural, @SingularEn, @PluralEn, @Icon, @Feminine, @IsDefault, @SortOrder, @CreatedAt)
            """, t);
        return t;
    }

    public async Task UpdateAsync(EntityType t, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        if (t.IsDefault) await connection.ExecuteAsync($"UPDATE {Table} SET IsDefault = 0 WHERE OrganizationId = @OrganizationId AND Id <> @Id", t);
        await connection.ExecuteAsync(
            $"UPDATE {Table} SET Singular = @Singular, Plural = @Plural, SingularEn = @SingularEn, PluralEn = @PluralEn, Icon = @Icon, Feminine = @Feminine, IsDefault = @IsDefault WHERE Id = @Id AND OrganizationId = @OrganizationId", t);
    }

    public async Task DeleteAsync(string organizationId, string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"DELETE FROM {Table} WHERE Id = @id AND OrganizationId = @organizationId AND IsDefault = 0", new { id, organizationId });
    }
}
