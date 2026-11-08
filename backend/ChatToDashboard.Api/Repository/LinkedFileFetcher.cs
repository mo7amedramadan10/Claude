using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using ChatToDashboard.Api.Data;

namespace ChatToDashboard.Api.Repository;

/// <summary>ملف منزَّل من رابط: بايتاته واسم ملف بامتداد يفهمه محلّل الرفع (<c>.xlsx</c>/<c>.csv</c>).</summary>
public sealed record FetchedFile(byte[] Bytes, string FileName, string ContentType);

/// <summary>
/// ربط ملف برابط عام (الخيار الأول من مزامنة الجداول السحابية): يحوّل رابط المشاركة إلى رابط تنزيل مباشر، ينزّل الملف،
/// ويحدد صيغته ليدخل مسار الرفع العادي نفسه (<see cref="UploadParser"/>). المدعوم:
/// Google Sheets (رابط <c>/spreadsheets/d/&lt;id&gt;</c> → تصدير xlsx للملف كاملًا، أو csv لورقة <c>gid</c> بعينها)،
/// OneDrive وSharePoint (روابط <c>1drv.ms</c>/<c>onedrive.live.com</c>/<c>*.sharepoint.com</c> → واجهة المشاركات أو <c>download=1</c>)،
/// Dropbox (<c>dl=1</c>)، وأي رابط مباشر لملف csv/xlsx. الشرط: الملف مشارك «لكل من معه الرابط» — لا تسجيل دخول.
/// </summary>
public sealed class LinkedFileFetcher
{
    public const int MaxBytes = 60 * 1024 * 1024;
    private static readonly Regex GoogleSheetRx = new(@"^https?://docs\.google\.com/spreadsheets/d/(?<id>[A-Za-z0-9_-]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex GidRx = new(@"[#?&]gid=(?<gid>\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private readonly IHttpClientFactory _http;

    public LinkedFileFetcher(IHttpClientFactory http) { _http = http; }

    /// <summary>نوع المصدر للعرض: <c>google</c> / <c>onedrive</c> / <c>sharepoint</c> / <c>dropbox</c> / <c>url</c>.</summary>
    public static string ProviderOf(string url)
    {
        if (GoogleSheetRx.IsMatch(url)) return "google";
        var host = SafeHost(url);
        if (host.EndsWith("sharepoint.com", StringComparison.OrdinalIgnoreCase)) return "sharepoint";
        if (host is "1drv.ms" || host.EndsWith("onedrive.live.com", StringComparison.OrdinalIgnoreCase) || host.EndsWith("onedrive.com", StringComparison.OrdinalIgnoreCase)) return "onedrive";
        if (host.EndsWith("dropbox.com", StringComparison.OrdinalIgnoreCase)) return "dropbox";
        return "url";
    }

    /// <summary>يحوّل رابط المشاركة إلى رابط تنزيل مباشر. يعيد الرابط نفسه إن كان مباشرًا أصلًا.</summary>
    public static string ToDownloadUrl(string url)
    {
        url = url.Trim();
        var g = GoogleSheetRx.Match(url);
        if (g.Success)
        {
            var gid = GidRx.Match(url);
            // ورقة بعينها (gid) → csv لتلك الورقة؛ وإلا الملف كاملًا xlsx (الورقة الأولى تُقرأ كما في الرفع العادي).
            return gid.Success
                ? $"https://docs.google.com/spreadsheets/d/{g.Groups["id"].Value}/export?format=csv&gid={gid.Groups["gid"].Value}"
                : $"https://docs.google.com/spreadsheets/d/{g.Groups["id"].Value}/export?format=xlsx";
        }
        var host = SafeHost(url);
        if (host is "1drv.ms" || host.EndsWith("onedrive.live.com", StringComparison.OrdinalIgnoreCase))
        {
            // واجهة المشاركات العامة في OneDrive: u! + base64url(الرابط) بلا حشو — تعمل لروابط «أي شخص معه الرابط» بلا مصادقة.
            var enc = Convert.ToBase64String(Encoding.UTF8.GetBytes(url)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            return $"https://api.onedrive.com/v1.0/shares/u!{enc}/root/content";
        }
        if (host.EndsWith("sharepoint.com", StringComparison.OrdinalIgnoreCase))
            return url.Contains("download=1", StringComparison.OrdinalIgnoreCase) ? url : url + (url.Contains('?') ? "&" : "?") + "download=1";
        if (host.EndsWith("dropbox.com", StringComparison.OrdinalIgnoreCase))
            return Regex.IsMatch(url, @"[?&]dl=") ? Regex.Replace(url, @"([?&])dl=\d", "$1dl=1") : url + (url.Contains('?') ? "&" : "?") + "dl=1";
        return url;
    }

    public static bool IsValidUrl(string? url) => !string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url.Trim(), UriKind.Absolute, out var u) && (u.Scheme == "https" || u.Scheme == "http");

    /// <summary>ينزّل الملف ويحدد صيغته: من الامتداد في الرابط أو اسم الملف في الترويسة أو نوع المحتوى أو توقيع البايتات (xlsx = ملف zip).</summary>
    public async Task<FetchedFile> FetchAsync(string url, string displayName, CancellationToken ct)
    {
        var download = ToDownloadUrl(url);
        var client = _http.CreateClient("linked-files");
        using var req = new HttpRequestMessage(HttpMethod.Get, download);
        req.Headers.TryAddWithoutValidation("User-Agent", "Jeem/1.0 (+linked-file-sync)");
        req.Headers.TryAddWithoutValidation("Accept", "text/csv,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/octet-stream,*/*");
        using var res = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
        if (res.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            throw new InvalidOperationException("الرابط يتطلب تسجيل دخول. شارك الملف بخيار «أي شخص معه الرابط» (عرض) ثم أعد المحاولة.");
        if (res.StatusCode == HttpStatusCode.NotFound) throw new InvalidOperationException("الملف غير موجود على هذا الرابط (404). تأكد من الرابط ومن أنه ما زال مشاركًا.");
        if (!res.IsSuccessStatusCode) throw new InvalidOperationException($"تعذّر التنزيل: {(int)res.StatusCode} {res.ReasonPhrase}.");
        if (res.Content.Headers.ContentLength is > MaxBytes) throw new InvalidOperationException($"حجم الملف يتجاوز {MaxBytes / 1024 / 1024} ميغابايت.");
        byte[] bytes;
        await using (var s = await res.Content.ReadAsStreamAsync(ct))
        using (var ms = new MemoryStream())
        {
            var buf = new byte[81920]; int n; long total = 0;
            while ((n = await s.ReadAsync(buf, ct)) > 0) { total += n; if (total > MaxBytes) throw new InvalidOperationException($"حجم الملف يتجاوز {MaxBytes / 1024 / 1024} ميغابايت."); ms.Write(buf, 0, n); }
            bytes = ms.ToArray();
        }
        if (bytes.Length == 0) throw new InvalidOperationException("الملف فارغ.");
        var contentType = res.Content.Headers.ContentType?.MediaType ?? "";
        var headerName = res.Content.Headers.ContentDisposition?.FileNameStar ?? res.Content.Headers.ContentDisposition?.FileName;
        var ext = DetectExtension(bytes, contentType, headerName?.Trim('"'), download);
        if (ext is null)
        {
            if (contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase) || LooksLikeHtml(bytes))
                throw new InvalidOperationException("الرابط يعيد صفحة ويب لا ملف بيانات — غالبًا الملف غير مشارك للعامة أو الرابط ليس رابط الملف نفسه. شارك الملف بخيار «أي شخص معه الرابط» وانسخ رابطه.");
            throw new InvalidOperationException("صيغة الملف غير مدعومة. المدعوم: Google Sheets، أو ملف Excel (.xlsx)، أو CSV.");
        }
        var safe = TabularFileParser.SanitizeTableName(string.IsNullOrWhiteSpace(displayName) ? "linked" : displayName);
        return new FetchedFile(bytes, $"{(safe.Length == 0 ? "linked" : safe)}{ext}", contentType);
    }

    internal static string? DetectExtension(byte[] bytes, string contentType, string? headerName, string url)
    {
        // xlsx = حاوية zip تبدأ بـ PK
        if (bytes.Length > 4 && bytes[0] == 0x50 && bytes[1] == 0x4B) return ".xlsx";
        var name = headerName ?? "";
        if (name.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)) return ".xlsx";
        if (name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)) return ".csv";
        if (contentType.Contains("spreadsheetml", StringComparison.OrdinalIgnoreCase)) return ".xlsx";
        if (contentType.Contains("text/csv", StringComparison.OrdinalIgnoreCase)) return ".csv";
        if (Regex.IsMatch(url, @"\.csv(\?|$)", RegexOptions.IgnoreCase) || url.Contains("format=csv", StringComparison.OrdinalIgnoreCase)) return LooksLikeHtml(bytes) ? null : ".csv";
        if (Regex.IsMatch(url, @"\.xls[xm]?(\?|$)", RegexOptions.IgnoreCase)) return null; // ليس zip فليس xlsx صالحًا
        // نص مفصول بفواصل بلا إشارات HTML
        if (!LooksLikeHtml(bytes) && (contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase) || contentType is "" or "application/octet-stream") && LooksLikeCsv(bytes)) return ".csv";
        return null;
    }

    private static bool LooksLikeHtml(byte[] bytes)
    {
        var head = Encoding.UTF8.GetString(bytes, 0, Math.Min(bytes.Length, 512)).TrimStart('﻿', ' ', '\r', '\n', '\t');
        return head.StartsWith("<!doctype", StringComparison.OrdinalIgnoreCase) || head.StartsWith("<html", StringComparison.OrdinalIgnoreCase) || head.Contains("<head", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeCsv(byte[] bytes)
    {
        var head = Encoding.UTF8.GetString(bytes, 0, Math.Min(bytes.Length, 4096));
        var first = head.Split('\n').FirstOrDefault() ?? "";
        return first.Contains(',') || first.Contains(';') || first.Contains('\t');
    }

    private static string SafeHost(string url) => Uri.TryCreate(url.Trim(), UriKind.Absolute, out var u) ? u.Host : "";
}
