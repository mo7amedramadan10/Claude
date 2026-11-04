using System.Buffers.Binary;
using System.Text;
using System.Text.Json.Serialization;
using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Decks;

/// <summary>ملف خط مرفوع ضمن هوية المشروع (ttf/otf/woff/woff2). <see cref="Url"/> مسار التنزيل العام للمتصفح والصفحة المنشورة.</summary>
public class DeckBrandFont
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("family")] public string Family { get; set; } = "";
    [JsonPropertyName("weight")] public int Weight { get; set; } = 400;
    [JsonPropertyName("style")] public string Style { get; set; } = "normal";
    [JsonPropertyName("fileName")] public string FileName { get; set; } = "";
    [JsonPropertyName("format")] public string Format { get; set; } = "truetype";
    [JsonPropertyName("sizeBytes")] public long SizeBytes { get; set; }
    [JsonPropertyName("uploadedAt")] public DateTime UploadedAt { get; set; }
    [JsonPropertyName("url")] public string Url => $"/api/decks/brand/fonts/{Id}/file";

    public static readonly Dictionary<string, (string Format, string ContentType)> Formats = new(StringComparer.OrdinalIgnoreCase)
    {
        [".ttf"] = ("truetype", "font/ttf"), [".otf"] = ("opentype", "font/otf"), [".woff"] = ("woff", "font/woff"), [".woff2"] = ("woff2", "font/woff2"),
    };

    /// <summary>قاعدة @font-face لهذا الملف (تُحقن في المحرر والصفحة المنشورة).</summary>
    public string FontFaceCss() =>
        $"@font-face{{font-family:'{Family.Replace("'", "")}';src:url('{Url}') format('{Format}');font-weight:{Weight};font-style:{Style};font-display:swap}}";
}

/// <summary>جدول <c>dbo.DeckBrandFonts</c>: ملفات الخطوط لكل مشروع (بياناتها في القاعدة كبقية مرفقات المنصة).</summary>
public class DeckBrandFontStore
{
    public const int MaxBytes = 6 * 1024 * 1024;
    public const int MaxPerProject = 12;
    private readonly DataStore _db;
    private readonly SemaphoreSlim _schemaGate = new(1, 1);
    private volatile bool _schemaReady;
    public DeckBrandFontStore(DataStore db) => _db = db;
    private const string Table = "[dbo].[DeckBrandFonts]";
    private static string Key(string? projectId) => string.IsNullOrEmpty(projectId) ? "-" : projectId;

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
                IF OBJECT_ID('dbo.DeckBrandFonts') IS NULL
                CREATE TABLE {Table} (
                  [Id] NVARCHAR(32) PRIMARY KEY, [ProjectKey] NVARCHAR(64) NOT NULL, [Family] NVARCHAR(120) NOT NULL, [Weight] INT NOT NULL,
                  [Style] NVARCHAR(10) NOT NULL, [FileName] NVARCHAR(260) NOT NULL, [Format] NVARCHAR(12) NOT NULL, [ContentType] NVARCHAR(40) NOT NULL,
                  [Bytes] VARBINARY(MAX) NOT NULL, [SizeBytes] BIGINT NOT NULL, [UploadedBy] NVARCHAR(200), [UploadedAt] DATETIME2 NOT NULL)
                """;
            await command.ExecuteNonQueryAsync(ct);
            _schemaReady = true;
        }
        finally { _schemaGate.Release(); }
    }

    public async Task<List<DeckBrandFont>> ListAsync(string? projectId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<DeckBrandFont>($"SELECT Id, Family, Weight, Style, FileName, Format, SizeBytes, UploadedAt FROM {Table} WHERE ProjectKey = @k ORDER BY Family, Weight, Style", new { k = Key(projectId) });
        return rows.ToList();
    }

    public async Task<DeckBrandFont> AddAsync(string? projectId, string? userId, string fileName, byte[] bytes, string family, int weight, string style, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var (format, contentType) = DeckBrandFont.Formats[ext];
        var f = new DeckBrandFont { Id = Guid.NewGuid().ToString("N")[..16], Family = family, Weight = weight, Style = style, FileName = Path.GetFileName(fileName), Format = format, SizeBytes = bytes.Length, UploadedAt = DateTime.UtcNow };
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"INSERT INTO {Table} (Id, ProjectKey, Family, Weight, Style, FileName, Format, ContentType, Bytes, SizeBytes, UploadedBy, UploadedAt) VALUES (@Id, @k, @Family, @Weight, @Style, @FileName, @Format, @contentType, @bytes, @SizeBytes, @userId, @UploadedAt)",
            new { f.Id, k = Key(projectId), f.Family, f.Weight, f.Style, f.FileName, f.Format, contentType, bytes, f.SizeBytes, userId, f.UploadedAt });
        return f;
    }

    public async Task<(byte[] Bytes, string ContentType, string FileName)?> GetFileAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var row = await connection.QuerySingleOrDefaultAsync($"SELECT Bytes, ContentType, FileName FROM {Table} WHERE Id = @id", new { id });
        return row is null ? null : ((byte[])row.Bytes, (string)row.ContentType, (string)row.FileName);
    }

    public async Task<bool> DeleteAsync(string? projectId, string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.ExecuteAsync($"DELETE FROM {Table} WHERE Id = @id AND ProjectKey = @k", new { id, k = Key(projectId) }) > 0;
    }

    // ---- قراءة اسم العائلة والوزن من جدول name في ملفات sfnt (ttf/otf) — بلا مكتبة ----
    /// <summary>يعيد (العائلة، العائلة الفرعية مثل Bold Italic) من ملف TTF/OTF، أو null إن لم يُقرأ (woff/woff2 أو ملف تالف).</summary>
    public static (string Family, string Subfamily)? ReadSfntNames(byte[] b)
    {
        try
        {
            if (b.Length < 12) return null;
            var tag = BinaryPrimitives.ReadUInt32BigEndian(b);
            if (tag is not (0x00010000 or 0x4F54544F or 0x74727565)) return null; // 1.0 / OTTO / true
            int numTables = BinaryPrimitives.ReadUInt16BigEndian(b.AsSpan(4));
            for (var i = 0; i < numTables; i++)
            {
                var rec = 12 + i * 16; if (rec + 16 > b.Length) return null;
                if (Encoding.ASCII.GetString(b, rec, 4) != "name") continue;
                var off = (int)BinaryPrimitives.ReadUInt32BigEndian(b.AsSpan(rec + 8)); var len = (int)BinaryPrimitives.ReadUInt32BigEndian(b.AsSpan(rec + 12));
                if (off + len > b.Length || len < 6) return null;
                int count = BinaryPrimitives.ReadUInt16BigEndian(b.AsSpan(off + 2)), strOff = BinaryPrimitives.ReadUInt16BigEndian(b.AsSpan(off + 4));
                string? fam = null, sub = null, famFallback = null, subFallback = null;
                for (var n = 0; n < count; n++)
                {
                    var r = off + 6 + n * 12; if (r + 12 > b.Length) break;
                    int platform = BinaryPrimitives.ReadUInt16BigEndian(b.AsSpan(r)), langId = BinaryPrimitives.ReadUInt16BigEndian(b.AsSpan(r + 4)), nameId = BinaryPrimitives.ReadUInt16BigEndian(b.AsSpan(r + 6));
                    int sl = BinaryPrimitives.ReadUInt16BigEndian(b.AsSpan(r + 8)), so = BinaryPrimitives.ReadUInt16BigEndian(b.AsSpan(r + 10));
                    var start = off + strOff + so; if (start + sl > b.Length) continue;
                    if (nameId is not (1 or 2 or 16 or 17)) continue;
                    var text = platform is 0 or 3 ? Encoding.BigEndianUnicode.GetString(b, start, sl) : Encoding.Latin1.GetString(b, start, sl);
                    text = text.Trim(); if (text.Length == 0) continue;
                    var english = platform == 3 && (langId == 0x0409 || langId == 0) || platform == 0;
                    // 16/17 (typographic) تغلب 1/2 لأن العائلة الكاملة فيها بلا تقسيم Light/Medium
                    if (nameId == 16) fam = text; else if (nameId == 17) sub = text;
                    else if (nameId == 1 && (english || famFallback is null)) famFallback = text;
                    else if (nameId == 2 && (english || subFallback is null)) subFallback = text;
                }
                var family = fam ?? famFallback; if (family is null) return null;
                return (family, sub ?? subFallback ?? "Regular");
            }
            return null;
        }
        catch { return null; }
    }

    /// <summary>الوزن ونمط الميل من اسم العائلة الفرعية («Bold Italic», «Medium», «Light»…).</summary>
    public static (int Weight, string Style) WeightStyleOf(string subfamily)
    {
        var s = subfamily.ToLowerInvariant();
        var style = s.Contains("italic") || s.Contains("oblique") ? "italic" : "normal";
        var weight = s.Contains("thin") || s.Contains("hairline") ? 100 : s.Contains("extralight") || s.Contains("ultralight") ? 200 : s.Contains("light") ? 300
            : s.Contains("medium") ? 500 : s.Contains("semibold") || s.Contains("demibold") ? 600 : s.Contains("extrabold") || s.Contains("ultrabold") ? 800
            : s.Contains("black") || s.Contains("heavy") ? 900 : s.Contains("bold") ? 700 : 400;
        return (weight, style);
    }
}
