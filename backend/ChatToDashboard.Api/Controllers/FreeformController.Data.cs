using System.Text.Json;
using System.Text.Json.Nodes;
using ChatToDashboard.Api.Build;
using ChatToDashboard.Api.Freeform;
using ChatToDashboard.Api.History;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>«📊 رسمة من بياناتي» في وضع التعديل: كتالوج الجداول والأعمدة المرئية للمستخدم (ولوحاته المحفوظة)، وتنفيذ استعلام مبني حتميًا
/// أو منسوخ من عنصر لوحة محفوظة وتسجيله في التصميم، فيُدرج بلوك رسم/قيمة في الصفحة يقرأ نتيجته من JEEM_DATA ويتحدّث معها.</summary>
public partial class FreeformController
{
    [HttpGet("{id}/data/catalog")]
    public async Task<IActionResult> DataCatalog(string id, [FromServices] DataProfileService profiles, [FromServices] HistoryStore history, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var (user, selection) = cur.Value;
        var (d, _, err) = await LoadAsync(id, user, selection, FreeformAccess.Edit, ct); if (err is not null) return err;
        if (StoredSources(d) is { } storedSel) selection = (await CurrentAsync(ct, storedSel))!.Value.Selection;
        selection = InDesignProject(selection, d!);
        var profile = await profiles.BuildAsync(await _tools.DescribeSourcesAsync(selection, ct), ct);
        var tables = profile.Tables.Select(t => new { table = t.Table, name = t.SourceName ?? t.Table.Split('.').Last(), rows = t.RowCount, columns = t.Columns.Select(c => new { name = c.Name, kind = c.Kind }) });
        var dashboards = new List<object>();
        foreach (var h in await history.ListAsync(user.Id, selection.ProjectId, 40, ct))
        {
            var widgets = new List<object>();
            try
            {
                if (JsonNode.Parse(h.WidgetsJson) is JsonArray arr)
                    for (var i = 0; i < arr.Count; i++)
                        if (arr[i]?["query"]?["sql"]?.GetValue<string>() is { Length: > 0 })
                            widgets.Add(new { index = i, title = arr[i]!["title"]?.GetValue<string>() ?? $"عنصر {i + 1}", type = arr[i]!["type"]?.GetValue<string>() ?? "bar" });
            }
            catch (JsonException) { }
            if (widgets.Count > 0) dashboards.Add(new { historyId = h.Id, title = string.IsNullOrWhiteSpace(h.Question) ? "لوحة" : h.Question, widgets });
        }
        return Ok(new { tables, dashboards, aggs = FreeformDataQuery.Aggs, grains = FreeformDataQuery.Grains });
    }

    [HttpPost("{id}/data/query")]
    public async Task<IActionResult> DataQuery(string id, [FromBody] FreeformDataQueryRequest request, [FromServices] DataProfileService profiles, [FromServices] HistoryStore history, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var (user, selection) = cur.Value;
        var (d, _, err) = await LoadAsync(id, user, selection, FreeformAccess.Edit, ct); if (err is not null) return err;
        if (StoredSources(d) is { } storedSel) selection = (await CurrentAsync(ct, storedSel))!.Value.Selection;
        selection = InDesignProject(selection, d!);
        var context = await _tools.DescribeSourcesAsync(selection, ct);
        string sql; string? x = null, y = null; string? widgetType = null, title = null;
        if (!string.IsNullOrWhiteSpace(request.HistoryId))
        {
            // عنصر من لوحة محفوظة يحق للمستخدم رؤيتها (القائمة نفسها مفلترة بالصلاحيات)
            var h = (await history.ListAsync(user.Id, selection.ProjectId, 200, ct)).FirstOrDefault(e => e.Id == request.HistoryId);
            if (h is null) return NotFound(new { error = "اللوحة غير موجودة أو لا تملك الوصول إليها." });
            JsonNode? w = null; try { w = (JsonNode.Parse(h.WidgetsJson) as JsonArray)?[request.WidgetIndex ?? -1]; } catch (Exception) { }
            var wsql = w?["query"]?["sql"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(wsql)) return BadRequest(new { error = "هذا العنصر بلا استعلام محفوظ." });
            sql = wsql; widgetType = w!["type"]?.GetValue<string>(); title = w["title"]?.GetValue<string>();
        }
        else
        {
            var profile = await profiles.BuildAsync(context, ct);
            var (built, berr) = FreeformDataQuery.Build(profile, request);
            if (built is null) return BadRequest(new { error = berr });
            sql = built.Sql; x = built.DimensionAlias; y = built.MeasureAlias;
        }
        var (res, isError) = await _tools.RunReadOnlyQueryAsync(sql, context, ct);
        if (isError) return BadRequest(new { error = "تعذّر تنفيذ الاستعلام: " + res });
        JsonNode? result; try { result = JsonNode.Parse(res); } catch (JsonException) { return BadRequest(new { error = "نتيجة الاستعلام غير مقروءة." }); }
        var rows = result?["rows"] as JsonArray ?? new JsonArray();
        var columns = rows.FirstOrDefault() is JsonObject first ? first.Select(kv => kv.Key).ToList() : new List<string>();
        string? key = null;
        if (!request.Preview)
        {
            // تسجيل الاستعلام في التصميم (موثوق من الخادم) فيُحدَّث مع «تحديث البيانات» وفي الصفحة المنشورة الحيّة
            var stored = (await _store.QueriesAsync(id, ct)).ToList();
            if (stored.Count == 0) stored = FreeformDataBlock.Parse(d!.Html);
            var taken = stored.Select(q => q.Key).Concat(FreeformDataBlock.Parse(d!.Html).Select(q => q.Key)).Concat(request.Keys ?? new List<string>())
                .Select(k => new FreeformQuery(k, "", null)).ToList();
            key = FreeformDataBlock.NextKey(taken);
            stored.Add(new FreeformQuery(key, sql.Trim(), null));
            await _store.SetQueriesAsync(id, stored, d!.DataRefreshedAt ?? DateTime.UtcNow, await _fingerprints.ComputeAsync(stored.Select(q => q.Sql), selection.ProjectId, ct), ct);
            if (d.PresentationId is not null) _live.Invalidate(d.PresentationId);
        }
        return Ok(new { key, sql = sql.Trim(), result, rows, rowCount = rows.Count, columns, x, y, widgetType, title });
    }
}
