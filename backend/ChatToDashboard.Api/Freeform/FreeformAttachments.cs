using System.Text;
using System.Text.RegularExpressions;
using ChatToDashboard.Api.Data;
using ChatToDashboard.Api.Decks;
using Dapper;
using DocumentFormat.OpenXml.Packaging;

namespace ChatToDashboard.Api.Freeform;

/// <summary>
/// مرفق رسالة في محادثة تصميم حر: <c>doc</c> (نص مستخرج حتميًا من ملف مرجعي)، <c>image</c> (صورة مخزّنة برابط عام
/// <c>/api/freeform/assets/{id}</c> يضعها الموديل في الصفحة)، أو <c>data</c> (ملف بيانات حُفظ في «الملفات» وصار مصدرًا يستعلم عنه).
/// </summary>
public class FreeformAttachment
{
    public string Kind { get; set; } = "doc";
    public string Name { get; set; } = "";
    public string? Text { get; set; }
    public string? Url { get; set; }
    public string? Table { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
}

public static class FreeformAttachmentText
{
    public const int MaxDocChars = 30_000, MaxTotalChars = 60_000, MaxAttachments = 10;
    public static readonly Regex AssetUrl = new(@"^/api/freeform/assets/[0-9a-f]{24}$", RegexOptions.Compiled);
    public static readonly string[] DocExtensions = { ".pdf", ".pptx", ".html", ".htm", ".docx", ".txt", ".md" };

    /// <summary>كتلة «مرفقات المستخدم» في رسالة الموديل؛ تتجاهل الفارغ وغير الصالح (رابط صورة غير مسار أصولنا، نص زائد يُقتطع).</summary>
    public static string Build(IReadOnlyList<FreeformAttachment>? atts)
    {
        if (atts is null || atts.Count == 0) return "";
        var sb = new StringBuilder(); var total = 0; var any = false;
        sb.AppendLine("# مرفقات المستخدم مع هذا الطلب");
        foreach (var a in atts.Take(MaxAttachments))
        {
            var name = Clean(a.Name, 160);
            switch (a.Kind)
            {
                case "image" when a.Url is not null && AssetUrl.IsMatch(a.Url):
                    sb.AppendLine($"- صورة «{name}»{(a.Width > 0 && a.Height > 0 ? $" ({a.Width}×{a.Height})" : "")}: ضعها في الصفحة حيث يناسب الطلب بالرابط نفسه حرفيًا `<img src=\"{a.Url}\" alt=\"…\">` (لا تحوّلها ولا تغيّر الرابط).");
                    any = true; break;
                case "data" when !string.IsNullOrWhiteSpace(a.Table):
                    sb.AppendLine($"- ملف بيانات «{name}» حُفظ في مصادر المشروع كجدول `{Clean(a.Table, 200)}` — استعلم عنه بـ query_data واستخدم أرقامه الحقيقية.");
                    any = true; break;
                case "doc" when !string.IsNullOrWhiteSpace(a.Text):
                    var text = a.Text!.Length > MaxDocChars ? a.Text[..MaxDocChars] + "\n…(اقتُطع)" : a.Text;
                    if (total + text.Length > MaxTotalChars) { sb.AppendLine($"- ملف «{name}»: (أُهمل لتجاوز حد طول المرفقات)"); continue; }
                    total += text.Length;
                    sb.AppendLine($"## محتوى الملف «{name}» — مرجع: استخدم نصوصه وجداوله وأرقامه كمحتوى فعلي ولا تختلق ما ليس فيه");
                    sb.AppendLine(text.Trim());
                    any = true; break;
            }
        }
        return any ? sb.AppendLine().ToString() : "";
    }

    private static string Clean(string? s, int max) { s = Regex.Replace(s ?? "", @"[\r\n`]", " ").Trim(); return s.Length > max ? s[..max] : s; }

    /// <summary>نص مرجعي من ملف مرفوع، حتميًا بلا موديل: PowerPoint/PDF/HTML عبر <see cref="DeckImporter"/>، Word عبر OpenXML، ونص عادي كما هو.</summary>
    public static string ExtractText(string fileName, byte[] bytes)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        string text;
        if (ext is ".txt" or ".md") text = Encoding.UTF8.GetString(bytes);
        else if (ext == ".docx") text = DocxText(bytes);
        else text = FreeformTemplateStore.FromDocument(DeckImporter.Extract(fileName, bytes), "design", null, fileName).Content ?? "";
        text = text.Replace("\0", "").Trim();
        return text.Length > MaxDocChars ? text[..MaxDocChars] + "\n…(اقتُطع)" : text;
    }

    private static string DocxText(byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        using var doc = WordprocessingDocument.Open(ms, false);
        var body = doc.MainDocumentPart?.Document?.Body; if (body is null) return "";
        var sb = new StringBuilder();
        foreach (var el in body.Elements())
        {
            if (el is DocumentFormat.OpenXml.Wordprocessing.Table t)
            {
                foreach (var row in t.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>())
                    sb.AppendLine(string.Join(" | ", row.Elements<DocumentFormat.OpenXml.Wordprocessing.TableCell>().Select(c => c.InnerText.Trim())));
            }
            else { var s = el.InnerText.Trim(); if (s.Length > 0) sb.AppendLine(s); }
            if (sb.Length > MaxDocChars) break;
        }
        return sb.ToString();
    }

    /// <summary>نوع الصورة من توقيعها لا من امتدادها (PNG/JPEG/GIF/WebP فقط؛ SVG مرفوض لأنه قد يحمل سكربت).</summary>
    public static string? ImageType(byte[] b)
    {
        if (b.Length > 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47) return "image/png";
        if (b.Length > 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF) return "image/jpeg";
        if (b.Length > 6 && b[0] == 'G' && b[1] == 'I' && b[2] == 'F') return "image/gif";
        if (b.Length > 12 && b[0] == 'R' && b[1] == 'I' && b[2] == 'F' && b[3] == 'F' && b[8] == 'W' && b[9] == 'E' && b[10] == 'B' && b[11] == 'P') return "image/webp";
        return null;
    }
}

/// <summary>صور مرفقة في محادثة تصميم حر — جدول <c>dbo.FreeformAssets</c>. المعرّف عشوائي 24 hex ويُقدَّم بلا دخول (الصفحة داخل إطار معزول
/// بأصل معتم وفي الصفحة المنشورة)، كخطوط الشركة.</summary>
public class FreeformAssetStore
{
    public const int MaxBytes = 5 * 1024 * 1024;
    private readonly DataStore _db;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private volatile bool _ready;
    private const string T = "[dbo].[FreeformAssets]";
    public FreeformAssetStore(DataStore db) => _db = db;
    private sealed class AssetRow { public byte[] Bytes { get; set; } = Array.Empty<byte>(); public string ContentType { get; set; } = ""; }

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
                IF OBJECT_ID('dbo.FreeformAssets') IS NULL
                CREATE TABLE {T} ([Id] NVARCHAR(32) PRIMARY KEY, [ProjectId] NVARCHAR(64), [CreatedByUserId] NVARCHAR(200) NOT NULL,
                  [Name] NVARCHAR(300) NOT NULL, [ContentType] NVARCHAR(64) NOT NULL, [Bytes] VARBINARY(MAX) NOT NULL, [CreatedAt] DATETIME2 NOT NULL);
                """);
            _ready = true;
        }
        finally { _gate.Release(); }
    }

    public async Task<string> AddAsync(string? projectId, string userId, string name, string contentType, byte[] bytes, CancellationToken ct)
    {
        await EnsureAsync(ct);
        var id = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(12)).ToLowerInvariant();
        await using var c = await _db.OpenConnectionAsync(ct);
        await c.ExecuteAsync($"INSERT INTO {T} (Id, ProjectId, CreatedByUserId, Name, ContentType, Bytes, CreatedAt) VALUES (@id, @projectId, @userId, @name, @contentType, @bytes, SYSUTCDATETIME())",
            new { id, projectId, userId, name = name.Length > 300 ? name[..300] : name, contentType, bytes });
        return id;
    }

    public async Task<(byte[] Bytes, string ContentType)?> GetAsync(string id, CancellationToken ct)
    {
        await EnsureAsync(ct);
        await using var c = await _db.OpenConnectionAsync(ct);
        var r = await c.QuerySingleOrDefaultAsync<AssetRow>($"SELECT Bytes, ContentType FROM {T} WHERE Id = @id", new { id });
        return r is null ? null : (r.Bytes, r.ContentType);
    }
}
