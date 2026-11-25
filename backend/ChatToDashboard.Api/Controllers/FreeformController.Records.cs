using System.Text.Json;
using ChatToDashboard.Api.Freeform;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>إدخال سجل أو تعديله: الحقول كائن JSON؛ replace=true يستبدل السجل كله بدل دمج الحقول.</summary>
public class FreeformRecordWriteRequest { public JsonElement Data { get; set; } public bool Replace { get; set; } }
public class FreeformRecordsPolicyRequest { public string? Policy { get; set; } }

/// <summary>سجلات الإدخال (<c>jeem.records</c>) داخل المنصة: المحرر ومن يرى التصميم. نفس البيانات التي يكتبها الرابط المنشور.</summary>
public partial class FreeformController
{
    private FreeformRecordsService Records => HttpContext.RequestServices.GetRequiredService<FreeformRecordsService>();

    private async Task<(FreeformDesign? D, FreeformRecordAccess.Viewer? V, string? Name, IActionResult? Error)> RecordsContextAsync(string id, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return (null, null, null, Unauthorized());
        var (d, access, err) = await LoadAsync(id, cur.Value.User, cur.Value.Selection, FreeformAccess.View, ct);
        if (err is not null) return (null, null, null, err);
        var u = cur.Value.User;
        return (d, new FreeformRecordAccess.Viewer(u.Id, u.OrganizationId, null, FreeformAccess.CanEdit(access!), InPlatform: true), string.IsNullOrWhiteSpace(u.DisplayName) ? u.Username : u.DisplayName, null);
    }

    /// <summary>مجموعات السجلات في التصميم بأعدادها، وسياسة الإدخال في الرابط المنشور.</summary>
    [HttpGet("{id}/records")]
    public async Task<IActionResult> RecordCollections(string id, CancellationToken ct)
    {
        var (d, v, _, err) = await RecordsContextAsync(id, ct); if (err is not null) return err;
        return await Records.CollectionsAsync(d!, v!, ct);
    }

    /// <summary>من يُدخل البيانات في الرابط المنشور: members (أعضاء المنظمة) أو public (أي من يفتح الرابط) — لصاحب التصميم.</summary>
    [HttpPut("{id}/records-policy")]
    public async Task<IActionResult> SetRecordsPolicy(string id, [FromBody] FreeformRecordsPolicyRequest request, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var (_, _, err) = await LoadAsync(id, cur.Value.User, cur.Value.Selection, FreeformAccess.Owner, ct); if (err is not null) return err;
        if (!FreeformRecordPolicy.IsValid(request.Policy)) return BadRequest(new { error = "سياسة غير معروفة: members أو public." });
        await _store.SetRecordsPolicyAsync(id, request.Policy!, ct);
        return Ok(new { policy = request.Policy });
    }

    [HttpGet("{id}/records/{collection}")]
    public async Task<IActionResult> RecordList(string id, string collection, [FromQuery] int? limit, [FromQuery] string? order, [FromQuery] long? after, [FromQuery] bool mine, CancellationToken ct)
    {
        var (d, v, _, err) = await RecordsContextAsync(id, ct); if (err is not null) return err;
        return await Records.ListAsync(d!, v!, collection, limit, order, after, mine, ct);
    }

    [HttpPost("{id}/records/{collection}")]
    public async Task<IActionResult> RecordAdd(string id, string collection, [FromBody] FreeformRecordWriteRequest request, CancellationToken ct)
    {
        var (d, v, name, err) = await RecordsContextAsync(id, ct); if (err is not null) return err;
        return await Records.AddAsync(d!, v!, collection, request.Data, name, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
    }

    [HttpPut("{id}/records/{collection}/{rowId:long}")]
    public async Task<IActionResult> RecordUpdate(string id, string collection, long rowId, [FromBody] FreeformRecordWriteRequest request, CancellationToken ct)
    {
        var (d, v, name, err) = await RecordsContextAsync(id, ct); if (err is not null) return err;
        return await Records.UpdateAsync(d!, v!, collection, rowId, request.Data, request.Replace, name, ct);
    }

    [HttpDelete("{id}/records/{collection}/{rowId:long}")]
    public async Task<IActionResult> RecordDelete(string id, string collection, long rowId, CancellationToken ct)
    {
        var (d, v, _, err) = await RecordsContextAsync(id, ct); if (err is not null) return err;
        return await Records.DeleteAsync(d!, v!, collection, rowId, ct);
    }

    /// <summary>تنزيل مجموعة CSV — لمن يملك تعديل التصميم.</summary>
    [HttpGet("{id}/records/{collection}/export.csv")]
    public async Task<IActionResult> RecordExport(string id, string collection, CancellationToken ct)
    {
        var (d, v, _, err) = await RecordsContextAsync(id, ct); if (err is not null) return err;
        if (!v!.Manager) return StatusCode(403, new { error = "التصدير لمن يملك تعديل التصميم." });
        return await Records.ExportCsvAsync(d!, collection, ct);
    }
}
