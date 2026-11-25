using System.Collections.Concurrent;
using System.Text;
using ChatToDashboard.Api.Data;
using ChatToDashboard.Api.Repository;
using ChatToDashboard.Api.Semantics;

namespace ChatToDashboard.Api.Freeform;

/// <summary>
/// سجلات تصميم حر كمصدر بيانات (المرحلة 2): كل مجموعة سجلات تُبنى جدولًا حقيقيًا في مخطط المشروع (<c>proj_&lt;id&gt;</c>) عبر مسار
/// «الملفات» نفسه — ملف مرتبط بمصدر داخلي <c>jeem-design://&lt;designId&gt;/&lt;collection&gt;</c> — فيظهر في «المصادر» وتحلّله طبقة المعنى،
/// وتعمل عليه المحادثة ولوحات المتابعة و«📊 رسمة من بياناتي» والتحديث الحيّ وتنبيه «تغيّرت البيانات» بلا تغيير. يُعاد بناؤه بعد كل
/// إدخال/تعديل/حذف بتأخير قصير يجمع الإدخالات المتتالية في إعادة بناء واحدة. صاحب الملف = صاحب التصميم.
/// </summary>
public class FreeformRecordTableService
{
    public const string Scheme = "jeem-design://";
    public const string Category = "بيانات من التصاميم";
    private static readonly TimeSpan Debounce = TimeSpan.FromSeconds(4);

    private readonly FreeformRecordStore _records;
    private readonly FreeformStore _designs;
    private readonly RepositoryStore _repo;
    private readonly ColumnSemanticsService _semantics;
    private readonly ILogger<FreeformRecordTableService> _logger;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _pending = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public FreeformRecordTableService(FreeformRecordStore records, FreeformStore designs, RepositoryStore repo, ColumnSemanticsService semantics, ILogger<FreeformRecordTableService> logger)
    { _records = records; _designs = designs; _repo = repo; _semantics = semantics; _logger = logger; }

    public static string SourceUrlOf(string designId, string collection) => $"{Scheme}{designId}/{Uri.EscapeDataString(collection)}";

    /// <summary>يحلّ رابط المصدر الداخلي إلى (التصميم، المجموعة)؛ null لغيره.</summary>
    public static (string DesignId, string Collection)? Parse(string? url)
    {
        if (url is null || !url.StartsWith(Scheme, StringComparison.OrdinalIgnoreCase)) return null;
        var rest = url[Scheme.Length..]; var slash = rest.IndexOf('/');
        if (slash <= 0 || slash == rest.Length - 1) return null;
        return (rest[..slash], Uri.UnescapeDataString(rest[(slash + 1)..]));
    }

    /// <summary>اسم الملف في «الملفات»: «سجلات: &lt;المجموعة&gt; — &lt;عنوان التصميم&gt;».</summary>
    public static string DisplayNameOf(string designTitle, string collection)
    {
        var t = (designTitle ?? "").Trim(); if (t.Length > 80) t = t[..80] + "…";
        return $"سجلات: {collection} — {t}";
    }

    /// <summary>جدول من السجلات بأنواع مستنتجة (رقم/تاريخ/نص) كملف CSV مرفوع، والمحتوى الأصلي CSV للتنزيل.</summary>
    internal static ParsedUpload BuildUpload(IReadOnlyList<FreeformRecord> rows, string collection)
    {
        var flat = FreeformRecordsService.Flatten(rows);
        var csv = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(FreeformRecordsService.ToCsv(rows))).ToArray();
        return new ParsedUpload(collection + ".csv", "csv", TabularFileParser.InferTypes(flat), null, 0, csv);
    }

    /// <summary>يجدول إعادة بناء الجدول بعد تغيّر السجلات (يُدمج ما يصل خلال ثوانٍ في بناء واحد). لا ينتظر.</summary>
    public void Schedule(FreeformDesign d, string collection)
    {
        if (!d.RecordsAsSource) return;
        var key = d.Id + "|" + collection;
        var cts = new CancellationTokenSource();
        if (_pending.TryRemove(key, out var old)) { try { old.Cancel(); } catch (ObjectDisposedException) { } }
        _pending[key] = cts;
        _ = Task.Run(async () =>
        {
            try { await Task.Delay(Debounce, cts.Token); }
            catch (TaskCanceledException) { return; }
            _pending.TryRemove(new KeyValuePair<string, CancellationTokenSource>(key, cts));
            try { await SyncAsync(d.Id, collection, CancellationToken.None); }
            catch (Exception ex) { _logger.LogWarning(ex, "تعذّر بناء جدول سجلات التصميم {Design}/{Collection}", d.Id, collection); }
        });
    }

    /// <summary>يبني جدول المجموعة الآن (ينشئ الملف أول مرة أو يستبدل بياناته بهوية ثابتة). يعيد الملف، أو null بلا سجلات/تصميم.</summary>
    public async Task<RepositoryFile?> SyncAsync(string designId, string collection, CancellationToken ct)
    {
        var gate = _locks.GetOrAdd(designId + "|" + collection, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct);
        try
        {
            var d = await _designs.GetByIdAsync(designId, ct);
            if (d is null) return null;
            var rows = await _records.AllAsync(designId, collection, ct);
            var url = SourceUrlOf(designId, collection);
            var existing = (await _repo.FindBySourcePrefixAsync(url, ct)).FirstOrDefault(f => f.SourceUrl == url);
            if (rows.Count == 0) return existing;
            var parsed = BuildUpload(rows, collection);
            if (existing is null)
            {
                var saved = await _repo.SaveAsync(parsed, DisplayNameOf(d.Title, collection),
                    $"بيانات أُدخلت عبر صفحة التصميم «{d.Title}» (مجموعة «{collection}»)، وتتحدّث تلقائيًا بعد كل إدخال.", Category, d.CreatedByUserId, d.ProjectId, ct);
                await _repo.SetSourceAsync(saved.Id, url, DateTime.UtcNow, null, null, null, ct);
                _semantics.AnalyzeInBackground(saved.ProjectId, saved.TableName);
                _logger.LogInformation("أُنشئ جدول سجلات التصميم {Design}/{Collection}: {Table}", designId, collection, saved.TableName);
                return await _repo.FindAsync(saved.Id, ct) ?? saved;
            }
            var before = await _repo.GetColumnsAsync(existing.Id, ct);
            var updated = await _repo.UpdateDataAsync(existing.Id, parsed, ct);
            await _repo.SetSourceAsync(existing.Id, url, DateTime.UtcNow, null, null, null, ct);
            var after = await _repo.GetColumnsAsync(existing.Id, ct);
            // المعاني تُعاد فقط حين تتغيّر الأعمدة (لا نداء موديل مع كل إدخال).
            if (!before.OrderBy(x => x).SequenceEqual(after.OrderBy(x => x), StringComparer.OrdinalIgnoreCase))
                _semantics.AnalyzeInBackground(updated?.ProjectId, updated?.TableName);
            return await _repo.FindAsync(existing.Id, ct) ?? updated;
        }
        finally { gate.Release(); }
    }

    /// <summary>جداول المصدر المرتبطة بتصميم (لكل مجموعة) — لنافذة «📝 السجلات».</summary>
    public async Task<IReadOnlyList<RepositoryFile>> TablesOfAsync(string designId, CancellationToken ct) =>
        await _repo.FindBySourcePrefixAsync(Scheme + designId + "/", ct);

    /// <summary>عند حذف التصميم: الجداول تبقى لقطة أخيرة بلا ربط (لا تُحذف بيانات استُخدمت ربما في لوحات).</summary>
    public async Task UnlinkDesignAsync(string designId, CancellationToken ct)
    {
        foreach (var f in await TablesOfAsync(designId, ct))
            await _repo.SetSourceAsync(f.Id, null, f.LastSyncedAt, null, null, null, ct);
    }
}
