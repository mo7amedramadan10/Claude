using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Decks;

/// <summary>هوية المشروع البصرية المعمَّمة على كل نماذج العروض: الشعار (data URL)، الألوان، الخط، والتذييل.
/// تُرفق بالمواصفة عند الرسم (<c>spec.brand</c>) والنشر والتصدير، ولا تُحفظ داخل مواصفة كل عرض حتى يسري أي تغيير على الجميع.</summary>
public class DeckBrand
{
    public const int MaxLogoBytes = 400 * 1024;
    private static readonly Regex Hex = new("^#[0-9a-fA-F]{6}$", RegexOptions.Compiled);
    public static readonly string[] Fonts = { "IBM Plex Sans Arabic", "Cairo", "Tajawal", "Almarai", "Noto Kufi Arabic", "Noto Naskh Arabic" };

    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("tagline")] public string? Tagline { get; set; }
    /// <summary>الشعار كـ <c>data:image/…;base64,…</c> (PNG/JPEG/SVG/WebP) أو null.</summary>
    [JsonPropertyName("logo")] public string? Logo { get; set; }
    [JsonPropertyName("primary")] public string? Primary { get; set; }
    [JsonPropertyName("secondary")] public string? Secondary { get; set; }
    [JsonPropertyName("accent")] public string? Accent { get; set; }
    [JsonPropertyName("font")] public string? Font { get; set; }
    [JsonPropertyName("footer")] public string? Footer { get; set; }
    [JsonPropertyName("updatedAt")] public DateTime? UpdatedAt { get; set; }

    public bool IsEmpty => string.IsNullOrWhiteSpace(Name) && string.IsNullOrWhiteSpace(Logo) && Primary is null && Secondary is null && Accent is null && Font is null && string.IsNullOrWhiteSpace(Footer) && string.IsNullOrWhiteSpace(Tagline);

    /// <summary>تطبيع وتحقق: ألوان hex من 6 خانات، خط من القائمة، شعار صورة بحجم محدود. يعيد رسالة خطأ أو null.</summary>
    public string? Validate()
    {
        Name = Trim(Name, 120); Tagline = Trim(Tagline, 200); Footer = Trim(Footer, 200);
        Primary = Color(Primary); Secondary = Color(Secondary); Accent = Color(Accent);
        Font = string.IsNullOrWhiteSpace(Font) ? null : Fonts.FirstOrDefault(f => string.Equals(f, Font.Trim(), StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(Logo)) Logo = null;
        else
        {
            var m = Regex.Match(Logo, "^data:image/(png|jpeg|jpg|svg\\+xml|webp);base64,([A-Za-z0-9+/=]+)$");
            if (!m.Success) return "الشعار يجب أن يكون صورة PNG أو JPEG أو SVG أو WebP.";
            if (m.Groups[2].Value.Length * 3 / 4 > MaxLogoBytes) return $"حجم الشعار يتجاوز {MaxLogoBytes / 1024} كيلوبايت — صغّر الصورة.";
        }
        return null;
        static string? Trim(string? s, int max) { s = s?.Trim(); if (string.IsNullOrEmpty(s)) return null; return s.Length > max ? s[..max] : s; }
        static string? Color(string? c) => c is not null && Hex.IsMatch(c.Trim()) ? c.Trim().ToUpperInvariant() : null;
    }
}

/// <summary>جدول <c>dbo.DeckBrands</c>: صف واحد لكل مشروع (أو للمستوى بلا مشروع بمفتاح فارغ).</summary>
public class DeckBrandStore
{
    private readonly DataStore _db;
    private readonly SemaphoreSlim _schemaGate = new(1, 1);
    private volatile bool _schemaReady;
    public DeckBrandStore(DataStore db) => _db = db;
    private const string Table = "[dbo].[DeckBrands]";

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
                IF OBJECT_ID('dbo.DeckBrands') IS NULL
                CREATE TABLE {Table} (
                  [ProjectKey] NVARCHAR(64) PRIMARY KEY, [OrganizationId] NVARCHAR(64), [Name] NVARCHAR(120), [Tagline] NVARCHAR(200),
                  [Logo] NVARCHAR(MAX), [Primary] NVARCHAR(7), [Secondary] NVARCHAR(7), [Accent] NVARCHAR(7), [Font] NVARCHAR(60),
                  [Footer] NVARCHAR(200), [UpdatedBy] NVARCHAR(200), [UpdatedAt] DATETIME2 NOT NULL)
                """;
            await command.ExecuteNonQueryAsync(ct);
            _schemaReady = true;
        }
        finally { _schemaGate.Release(); }
    }

    private static string Key(string? projectId) => string.IsNullOrEmpty(projectId) ? "-" : projectId;

    public async Task<DeckBrand?> GetAsync(string? projectId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var row = await connection.QuerySingleOrDefaultAsync($"SELECT Name, Tagline, Logo, [Primary], Secondary, Accent, Font, Footer, UpdatedAt FROM {Table} WHERE ProjectKey = @k", new { k = Key(projectId) });
        if (row is null) return null;
        return new DeckBrand
        {
            Name = row.Name, Tagline = row.Tagline, Logo = row.Logo, Primary = row.Primary, Secondary = row.Secondary, Accent = row.Accent,
            Font = row.Font, Footer = row.Footer, UpdatedAt = row.UpdatedAt is DateTime d ? DateTime.SpecifyKind(d, DateTimeKind.Utc) : null,
        };
    }

    public async Task SetAsync(string? projectId, string? organizationId, string userId, DeckBrand b, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var p = new { k = Key(projectId), organizationId, b.Name, b.Tagline, b.Logo, b.Primary, b.Secondary, b.Accent, b.Font, b.Footer, userId, now = DateTime.UtcNow };
        var n = await connection.ExecuteAsync($"UPDATE {Table} SET OrganizationId=@organizationId, Name=@Name, Tagline=@Tagline, Logo=@Logo, [Primary]=@Primary, Secondary=@Secondary, Accent=@Accent, Font=@Font, Footer=@Footer, UpdatedBy=@userId, UpdatedAt=@now WHERE ProjectKey=@k", p);
        if (n == 0) await connection.ExecuteAsync($"INSERT INTO {Table} (ProjectKey, OrganizationId, Name, Tagline, Logo, [Primary], Secondary, Accent, Font, Footer, UpdatedBy, UpdatedAt) VALUES (@k, @organizationId, @Name, @Tagline, @Logo, @Primary, @Secondary, @Accent, @Font, @Footer, @userId, @now)", p);
    }

    public async Task DeleteAsync(string? projectId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"DELETE FROM {Table} WHERE ProjectKey = @k", new { k = Key(projectId) });
    }
}
