using System.Text;
using System.Text.Json;
using ChatToDashboard.Api.Presentations;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Freeform;

/// <summary>
/// منطق السجلات المشترك بين مسارَي الوصول: داخل المنصة (<c>/api/freeform/{id}/records…</c> بصلاحية التصميم) والرابط المنشور
/// (<c>/api/presentations/{id}/records…</c> بسياسة التصميم، حتى بلا دخول إن كانت عامة). يعيد نتائج HTTP جاهزة.
/// </summary>
public class FreeformRecordsService
{
    private readonly FreeformRecordStore _store;
    // حد الإدخالات: الزائر المجهول أضيق من المسجَّل، وسقف لكل تصميم يحمي من الإغراق.
    private readonly PresentationAssistantService.RateLimiter _anonLimit = new(60, TimeSpan.FromMinutes(10));
    private readonly PresentationAssistantService.RateLimiter _userLimit = new(300, TimeSpan.FromMinutes(10));
    private readonly PresentationAssistantService.RateLimiter _designLimit = new(3000, TimeSpan.FromHours(1));

    public FreeformRecordsService(FreeformRecordStore store) => _store = store;

    private static ObjectResult Fail(int status, string error) => new(new { error }) { StatusCode = status };

    private static object Dto(FreeformRecord r, FreeformRecordAccess.Viewer v)
    {
        JsonElement data;
        try { data = JsonSerializer.Deserialize<JsonElement>(r.Json); } catch (JsonException) { data = JsonSerializer.Deserialize<JsonElement>("{}"); }
        return new { id = r.Id, data, createdAt = r.CreatedAt, createdBy = r.CreatedByName, updatedAt = r.UpdatedAt, updatedBy = r.UpdatedByName,
            mine = FreeformRecordAccess.Owns(v, r), canChange = FreeformRecordAccess.CanChange(v, r) };
    }

    public async Task<IActionResult> CollectionsAsync(FreeformDesign d, FreeformRecordAccess.Viewer v, CancellationToken ct)
    {
        var cols = FreeformRecordAccess.SeesAll(v, d) ? await _store.CollectionsAsync(d.Id, ct) : Array.Empty<FreeformRecordCollection>();
        return new OkObjectResult(new { policy = FreeformRecordPolicy.Normalize(d.RecordsPolicy), canAdd = FreeformRecordAccess.CanAdd(v, d) is null,
            seesAll = FreeformRecordAccess.SeesAll(v, d), manager = v.Manager, collections = cols });
    }

    public async Task<IActionResult> ListAsync(FreeformDesign d, FreeformRecordAccess.Viewer v, string collection, int? limit, string? order, long? cursor, bool mine, CancellationToken ct)
    {
        if (!FreeformRecordStore.IsValidCollection(collection)) return Fail(400, "اسم مجموعة غير صالح (حروف وأرقام وشرطات، حتى 60 حرفًا).");
        var take = Math.Clamp(limit ?? 200, 1, 1000);
        var onlyOwn = mine || !FreeformRecordAccess.SeesAll(v, d);
        if (onlyOwn && v.UserId is null && v.AnonId is null) return new OkObjectResult(new { items = Array.Empty<object>(), total = 0, hasMore = false, onlyOwn });
        var desc = string.Equals(order, "desc", StringComparison.OrdinalIgnoreCase);
        var (items, total) = await _store.ListAsync(d.Id, collection, take, desc, cursor, onlyOwn, v.UserId, v.AnonId, ct);
        var hasMore = items.Count > take;
        var page = items.Take(take).ToList();
        return new OkObjectResult(new { items = page.Select(r => Dto(r, v)), total, hasMore, next = hasMore ? page[^1].Id : (long?)null, onlyOwn });
    }

    public async Task<IActionResult> AddAsync(FreeformDesign d, FreeformRecordAccess.Viewer v, string collection, JsonElement data, string? userName, string? ip, CancellationToken ct)
    {
        if (!FreeformRecordStore.IsValidCollection(collection)) return Fail(400, "اسم مجموعة غير صالح (حروف وأرقام وشرطات، حتى 60 حرفًا).");
        if (FreeformRecordAccess.CanAdd(v, d) is { } deny) return Fail(deny.Status, deny.Error);
        if (FreeformRecordStore.Validate(data) is { } bad) return Fail(400, bad);
        var who = v.UserId ?? v.AnonId ?? ip ?? "?";
        var limiter = v.UserId is null ? _anonLimit : _userLimit;
        if (!limiter.TryConsume(d.Id + "|" + who) || !_designLimit.TryConsume(d.Id))
            return Fail(429, "أُدخلت سجلات كثيرة في وقت قصير — انتظر قليلًا ثم أعد المحاولة.");
        var (r, error) = await _store.AddAsync(d.Id, collection, data.GetRawText(), v.UserId, userName ?? (v.UserId is null ? "زائر" : null), v.UserId is null ? v.AnonId : null, ct);
        if (r is null) return Fail(400, error!);
        return new OkObjectResult(Dto(r, v));
    }

    public async Task<IActionResult> UpdateAsync(FreeformDesign d, FreeformRecordAccess.Viewer v, string collection, long id, JsonElement data, bool replace, string? userName, CancellationToken ct)
    {
        if (!FreeformRecordStore.IsValidCollection(collection)) return Fail(400, "اسم مجموعة غير صالح.");
        var r = await _store.GetAsync(d.Id, collection, id, ct);
        if (r is null || (!FreeformRecordAccess.SeesAll(v, d) && !FreeformRecordAccess.Owns(v, r))) return Fail(404, "السجل غير موجود.");
        if (!FreeformRecordAccess.CanChange(v, r)) return Fail(403, "يعدّل السجلَّ صاحبُه أو من يملك تعديل التصميم.");
        if (data.ValueKind != JsonValueKind.Object) return Fail(400, "التعديل يجب أن يكون كائنًا بحقول.");
        var json = replace ? data.GetRawText() : FreeformRecordStore.Merge(r.Json, data);
        if (FreeformRecordStore.Validate(JsonSerializer.Deserialize<JsonElement>(json)) is { } bad) return Fail(400, bad);
        await _store.UpdateAsync(r, json, userName ?? "زائر", ct);
        return new OkObjectResult(Dto(r, v));
    }

    public async Task<IActionResult> DeleteAsync(FreeformDesign d, FreeformRecordAccess.Viewer v, string collection, long id, CancellationToken ct)
    {
        if (!FreeformRecordStore.IsValidCollection(collection)) return Fail(400, "اسم مجموعة غير صالح.");
        var r = await _store.GetAsync(d.Id, collection, id, ct);
        if (r is null || (!FreeformRecordAccess.SeesAll(v, d) && !FreeformRecordAccess.Owns(v, r))) return Fail(404, "السجل غير موجود.");
        if (!FreeformRecordAccess.CanChange(v, r)) return Fail(403, "يحذف السجلَّ صاحبُه أو من يملك تعديل التصميم.");
        await _store.DeleteAsync(r.Id, ct);
        return new NoContentResult();
    }

    /// <summary>تصدير مجموعة CSV (UTF-8 بـ BOM ليفتحها Excel صحيحًا): الأعمدة = اتحاد الحقول بترتيب أول ظهور + أعمدة الإدخال.</summary>
    public async Task<IActionResult> ExportCsvAsync(FreeformDesign d, string collection, CancellationToken ct)
    {
        if (!FreeformRecordStore.IsValidCollection(collection)) return Fail(400, "اسم مجموعة غير صالح.");
        var rows = await _store.AllAsync(d.Id, collection, ct);
        return new FileContentResult(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(ToCsv(rows))).ToArray(), "text/csv; charset=utf-8")
            { FileDownloadName = collection + ".csv" };
    }

    internal static string ToCsv(IReadOnlyList<FreeformRecord> rows)
    {
        var parsed = rows.Select(r => { try { return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(r.Json) ?? new(); } catch (JsonException) { return new Dictionary<string, JsonElement>(); } }).ToList();
        var fields = new List<string>();
        foreach (var p in parsed) foreach (var k in p.Keys) if (!fields.Contains(k)) fields.Add(k);
        static string Q(string? s) { s ??= ""; return s.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0 ? "\"" + s.Replace("\"", "\"\"") + "\"" : s; }
        static string V(JsonElement e) => e.ValueKind switch { JsonValueKind.String => e.GetString() ?? "", JsonValueKind.Null or JsonValueKind.Undefined => "", _ => e.GetRawText() };
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", new[] { "id" }.Concat(fields).Concat(new[] { "أدخله", "تاريخ الإدخال" }).Select(Q)));
        for (var i = 0; i < rows.Count; i++)
            sb.AppendLine(string.Join(",", new[] { rows[i].Id.ToString() }.Concat(fields.Select(f => parsed[i].TryGetValue(f, out var e) ? V(e) : ""))
                .Concat(new[] { rows[i].CreatedByName ?? "", rows[i].CreatedAt.ToString("yyyy-MM-dd HH:mm:ss") }).Select(Q)));
        return sb.ToString();
    }
}
