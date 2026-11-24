using System.Text.Json;
using System.Text.Json.Nodes;
using ChatToDashboard.Api.Build;
using ChatToDashboard.Api.Freeform;
using ChatToDashboard.Api.History;
using ChatToDashboard.Api.Models;
using ChatToDashboard.Api.Templates;
using Microsoft.Extensions.Caching.Memory;
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
        string sql; string? x = null, y = null; string? widgetType = null, title = null; JsonNode? widgetMeta = null;
        if (!string.IsNullOrWhiteSpace(request.BuildId))
        {
            // عنصر بُني للتو من «عناصر جاهزة/مكتبة المؤشرات» (data/build): SQL من ذاكرة الخادم لا من المتصفح
            if (!BuiltWidgets.TryGetValue(BuildKey(user.Id, id, request.BuildId), out JsonNode? bw) || bw is null)
                return BadRequest(new { error = "انتهت صلاحية العنصر المبني — ابنِه من جديد." });
            var bsql = bw["query"]?["sql"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(bsql)) return BadRequest(new { error = "هذا العنصر بلا استعلام." });
            sql = bsql; widgetType = bw["type"]?.GetValue<string>(); title = bw["title"]?.GetValue<string>();
            widgetMeta = new JsonObject { ["xKey"] = bw["xKey"]?.DeepClone(), ["yKey"] = bw["yKey"]?.DeepClone(), ["series"] = bw["series"]?.DeepClone(),
                ["stacked"] = bw["stacked"]?.DeepClone(), ["areaFill"] = bw["areaFill"]?.DeepClone(), ["centerLabel"] = bw["centerLabel"]?.DeepClone() };
        }
        else if (!string.IsNullOrWhiteSpace(request.HistoryId))
        {
            // عنصر من لوحة محفوظة يحق للمستخدم رؤيتها (القائمة نفسها مفلترة بالصلاحيات)
            var h = (await history.ListAsync(user.Id, selection.ProjectId, 200, ct)).FirstOrDefault(e => e.Id == request.HistoryId);
            if (h is null) return NotFound(new { error = "اللوحة غير موجودة أو لا تملك الوصول إليها." });
            JsonNode? w = null; try { w = (JsonNode.Parse(h.WidgetsJson) as JsonArray)?[request.WidgetIndex ?? -1]; } catch (Exception) { }
            var wsql = w?["query"]?["sql"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(wsql)) return BadRequest(new { error = "هذا العنصر بلا استعلام محفوظ." });
            sql = wsql; widgetType = w!["type"]?.GetValue<string>(); title = w["title"]?.GetValue<string>();
            // شكل العنصر كما في اللوحة («كما في اللوحة» في الواجهة): المفاتيح والسلاسل والتكديس ونص المركز
            widgetMeta = new JsonObject { ["xKey"] = w["xKey"]?.DeepClone(), ["yKey"] = w["yKey"]?.DeepClone(), ["series"] = w["series"]?.DeepClone(),
                ["stacked"] = w["stacked"]?.DeepClone(), ["areaFill"] = w["areaFill"]?.DeepClone(), ["centerLabel"] = w["centerLabel"]?.DeepClone() };
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
        return Ok(new { key, sql = sql.Trim(), result, rows, rowCount = rows.Count, columns, x, y, widgetType, title, widget = widgetMeta });
    }

    // العناصر المبنية بالموديل تُحفظ في الذاكرة 30 دقيقة بمفتاح المستخدم والتصميم: الإدراج يشير إليها بمعرّف فلا يُقبل SQL من المتصفح
    private static readonly MemoryCache BuiltWidgets = new(new MemoryCacheOptions { SizeLimit = 2000 });
    private static string BuildKey(string userId, string designId, string buildId) => $"{userId}|{designId}|{buildId}";

    /// <summary>«من المكتبة»: يبني عنصرًا واحدًا من «عناصر جاهزة» أو «مكتبة المؤشرات» على بيانات التصميم بالمسار المرحلي لعنصر واحد
    /// (نداء موديل واحد يكتب الاستعلام على أعمدة المستخدم ثم ينفّذه)، ويعيد معرّفًا يُدرج به عبر data/query بلا نداء ثانٍ.</summary>
    [HttpPost("{id}/data/build")]
    public async Task<IActionResult> DataBuild(string id, [FromBody] FreeformDataBuildRequest request, [FromServices] StagedTemplateBuilder staged,
        [FromServices] TemplatePromptService prompts, CancellationToken ct)
    {
        var cur = await CurrentAsync(ct); if (cur is null) return Unauthorized();
        var (user, selection) = cur.Value;
        var (d, _, err) = await LoadAsync(id, user, selection, FreeformAccess.Edit, ct); if (err is not null) return err;
        if (StoredSources(d) is { } storedSel) selection = (await CurrentAsync(ct, storedSel))!.Value.Selection;
        selection = InDesignProject(selection, d!);
        var title = (request.Title ?? "").Trim(); var type = string.IsNullOrWhiteSpace(request.Type) ? "bar" : request.Type!.Trim();
        string? prompt = request.Prompt, table = null, sql = null;
        if (request.Ref is { Kind: TemplateKinds.Widget or TemplateKinds.Kpi } r)
        {
            var (p, perr, required) = await prompts.ResolveAsync(r, user, selection, ct);
            if (perr is not null) return BadRequest(new { error = perr });
            prompt = p;
            if (r.Kind == TemplateKinds.Widget && BuiltinTemplates.FindWidget(r.Key) is { } tw && string.IsNullOrWhiteSpace(title)) title = tw.Title;
            if (r.Kind == TemplateKinds.Kpi) { type = "kpi"; if (string.IsNullOrWhiteSpace(title)) title = r.KpiName ?? ""; if (required is { Count: > 0 }) { table = required[0].Table; sql = request.Trend ? null : required[0].Sql; } }
        }
        if (string.IsNullOrWhiteSpace(title)) return BadRequest(new { error = "عنوان العنصر مطلوب." });
        // ربط رسمة قالب بمؤشر: الرقم الواحد لا يحل محل رسمة، فيُطلب المؤشر نفسه سلسلةً زمنية (مسودة SQL المكتبة للرقم الواحد لا تُمرَّر)
        if (request.Trend)
        {
            type = "line";
            prompt = (prompt ?? "").TrimEnd() + "\n\nاعرض هذا المؤشر اتجاهًا عبر الزمن لا رقمًا واحدًا: قيمته لكل شهر (آخر 12 شهرًا متاحة) من عمود التاريخ الأنسب، بعمود للفترة وعمود للقيمة؛ لأنه سيحل محل رسمة في الصفحة.";
        }
        if (!DashboardSpec.AllowedWidgetTypes.Contains(type)) type = "bar";
        var context = await _tools.DescribeSourcesAsync(selection, ct);
        var spec = new BuiltinDashboardWidgetSpec(type, title, prompt, Table: table, Sql: sql);
        var req = new StagedBuildRequest("عنصر لتصميم حر", $"عنصر واحد بعنوان «{title}» يُدرج في صفحة تصميم حر من بيانات المشروع.", new[] { spec }, null,
            new Dictionary<string, SchemaLibrary.SchemaMatchingService.MatchedWidget>(), context, request.Lang, user);
        DashboardWidget? widget;
        try { widget = await staged.BuildOneAsync(req, title, ct); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { return StatusCode(502, new { error = "تعذّر بناء العنصر: " + ex.Message }); }
        if (widget is null) return BadRequest(new { error = "المزوّد الحالي لا يدعم بناء عنصر واحد." });
        if (widget.NoData || string.IsNullOrWhiteSpace(widget.Query?.Sql))
            return Ok(new { buildId = (string?)null, title = widget.Title, type = widget.Type, reason = widget.MissingReason ?? "تعذّر بناء هذا العنصر من بياناتك." });
        var node = JsonSerializer.SerializeToNode(widget)!;
        var buildId = System.Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(12)).ToLowerInvariant();
        BuiltWidgets.Set(BuildKey(user.Id, id, buildId), node, new MemoryCacheEntryOptions { Size = 1, AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) });
        return Ok(new { buildId, title = widget.Title, type = widget.Type });
    }
}

public class FreeformDataBuildRequest
{
    [System.Text.Json.Serialization.JsonPropertyName("ref")] public TemplateRef? Ref { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("title")] public string? Title { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("type")] public string? Type { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("prompt")] public string? Prompt { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("lang")] public string? Lang { get; set; }
    /// <summary>ابنِ المؤشر سلسلة زمنية (شهرية) بدل رقم واحد — لربطه مكان رسمة.</summary>
    [System.Text.Json.Serialization.JsonPropertyName("trend")] public bool Trend { get; set; }
}
