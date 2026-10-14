using System.Text.Json.Serialization;
using ChatToDashboard.Api.Build;
using ChatToDashboard.Api.Data;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Semantics;
using ChatToDashboard.Api.Sources;
using ChatToDashboard.Api.Templates;
using ChatToDashboard.Api.Users;
using ChatToDashboard.Api.Widgets;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

public class WidgetColumnsInspectRequest
{
    [JsonPropertyName("sources")] public SourceSelection? Sources { get; set; }
    [JsonPropertyName("table")] public string? Table { get; set; }
    [JsonPropertyName("sql")] public string? Sql { get; set; }
    [JsonPropertyName("title")] public string? Title { get; set; }
}

public class WidgetColumnsRemapRequest
{
    [JsonPropertyName("sources")] public SourceSelection? Sources { get; set; }
    [JsonPropertyName("table")] public string Table { get; set; } = "";
    [JsonPropertyName("sql")] public string Sql { get; set; } = "";
    /// <summary>العمود الحالي في الاستعلام ← العمود البديل من الجدول نفسه.</summary>
    [JsonPropertyName("mapping")] public Dictionary<string, string> Mapping { get; set; } = new();
    /// <summary>true: يُنقل معنى العمود القديم إلى الجديد كتأكيد مستخدم (تعلّم طبقة المعنى).</summary>
    [JsonPropertyName("learn")] public bool Learn { get; set; } = true;
}

public class WidgetRebuildRequest
{
    [JsonPropertyName("sources")] public SourceSelection? Sources { get; set; }
    /// <summary>اختياري: بدونه يُبنى العنصر من عنوانه ونوعه فقط.</summary>
    [JsonPropertyName("templateKey")] public string? TemplateKey { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("prompt")] public string? Prompt { get; set; }
    [JsonPropertyName("lang")] public string? Lang { get; set; }
}

/// <summary>
/// «تصحيح الأعمدة» من قائمة العنصر (⋮): يكشف الأعمدة الحقيقية التي يعتمد عليها الاستعلام المحفوظ
/// والمفاهيم التي يحتاجها العنصر، ويعيد تنفيذ الاستعلام بعد استبدال أعمدة بأخرى بلا موديل، أو
/// يعيد بناء عنصر قالب واحد بالمسار المرحلي بعد تصحيح المعاني.
/// </summary>
[ApiController]
[Route("api/widgets/columns")]
public class WidgetColumnsController : ControllerBase
{
    private readonly PermissionsService _permissions;
    private readonly AnalyticsTools _tools;
    private readonly DataProfileService _profiles;
    private readonly ColumnSemanticsStore _semanticsStore;
    private readonly ColumnSemanticsService _semantics;
    private readonly WidgetQueryService _queries;
    private readonly StagedTemplateBuilder _staged;
    private readonly TemplatePromptService _templatePrompts;
    private readonly DataStore _db;
    private readonly IDashboardGenerator _generator;
    private readonly ILogger<WidgetColumnsController> _logger;

    public WidgetColumnsController(
        PermissionsService permissions, AnalyticsTools tools, DataProfileService profiles,
        ColumnSemanticsStore semanticsStore, ColumnSemanticsService semantics, WidgetQueryService queries,
        StagedTemplateBuilder staged, TemplatePromptService templatePrompts, DataStore db,
        IDashboardGenerator generator, ILogger<WidgetColumnsController> logger)
    {
        _generator = generator;
        _permissions = permissions; _tools = tools; _profiles = profiles; _semanticsStore = semanticsStore;
        _semantics = semantics; _queries = queries; _staged = staged; _templatePrompts = templatePrompts;
        _db = db; _logger = logger;
    }

    /// <summary>أعمدة الجدول (بمعانيها) مع أيّها يستخدمه الاستعلام، ومتطلبات العنصر من المفاهيم
    /// وحالة كل منها في بيانات المشروع كلها (لا هذا الجدول فقط).</summary>
    [HttpPost("inspect")]
    public async Task<IActionResult> Inspect([FromBody] WidgetColumnsInspectRequest request, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var selection = await _permissions.GetEffectiveSelectionAsync(user, request.Sources, ct);
        var context = await _tools.DescribeSourcesAsync(selection, ct);
        var profile = await _profiles.BuildAsync(context, ct);

        // كل جداول المشروع المرئية بأعمدتها ومعانيها — لاختيار عمود لمفهوم ناقص من أي جدول.
        var semantics = string.IsNullOrWhiteSpace(context.ProjectId)
            ? Array.Empty<ColumnSemantic>()
            : await _semanticsStore.ListForProjectAsync(context.ProjectId, ct);
        var byKey = semantics.ToDictionary(s => (s.TableName.ToLowerInvariant(), s.ColumnName.ToLowerInvariant()), s => s);
        object DescribeColumn(string table, ProfiledColumn c)
        {
            byKey.TryGetValue((table.ToLowerInvariant(), c.Name.ToLowerInvariant()), out var s);
            var concept = ConceptCatalog.Find(s?.Concept);
            return new { name = c.Name, kind = c.Kind, concept = concept?.Id, conceptLabel = concept?.Label, source = s?.Source, confidence = s?.Confidence };
        }
        var tables = profile.Tables.Select(t => new
        {
            table = t.Table,
            fileName = context.TableFiles.TryGetValue(t.Table, out var f) ? f : t.SourceName,
            columns = t.Columns.Select(c => DescribeColumn(t.Table, c)).ToList(),
        }).ToList();

        // الأعمدة التي يستخدمها الاستعلام المحفوظ (إن وُجد) من جدوله.
        IReadOnlyList<string> used = Array.Empty<string>();
        var queryTable = request.Table is { Length: > 0 } ? profile.Find(request.Table) : null;
        if (queryTable is not null && !string.IsNullOrWhiteSpace(request.Sql))
            used = SqlColumnRewriter.UsedColumns(request.Sql, queryTable.Columns.Select(c => c.Name));

        // متطلبات العنصر من عنوانه، وحالة كل مفهوم في المشروع.
        var available = semantics.Where(s => s.Concept is not null && s.Confidence >= 0.4)
            .GroupBy(s => s.Concept!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Select(s => new { table = s.TableName, column = s.ColumnName }).ToList(), StringComparer.OrdinalIgnoreCase);
        var requirements = string.IsNullOrWhiteSpace(request.Title)
            ? new List<object>()
            : WidgetRequirementResolver.Resolve(request.Title).Select(r =>
            {
                var hit = r.AnyOf.FirstOrDefault(available.ContainsKey);
                return (object)new
                {
                    label = r.Label, hard = r.Hard, concepts = r.AnyOf,
                    conceptLabels = r.AnyOf.Select(c => ConceptCatalog.Find(c)?.Label ?? c).ToList(),
                    satisfied = hit is not null,
                    matches = hit is null ? new() : available[hit],
                };
            }).ToList();

        return Ok(new { table = queryTable?.Table, used, tables, requirements, concepts = ConceptCatalog.Concepts });
    }

    /// <summary>يعيد تنفيذ استعلام العنصر بعد استبدال أعمدة بأخرى من الجدول نفسه — بلا موديل.</summary>
    [HttpPost("remap")]
    public async Task<IActionResult> Remap([FromBody] WidgetColumnsRemapRequest request, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var selection = await _permissions.GetEffectiveSelectionAsync(user, request.Sources, ct);
        var context = await _tools.DescribeSourcesAsync(selection, ct);
        var table = (await _profiles.BuildAsync(context, ct)).Find(request.Table);
        if (table is null) return BadRequest(new { error = "جدول هذا العنصر غير متاح في مصادر المشروع." });

        var names = table.Columns.ToDictionary(c => c.Name, c => c.Name, StringComparer.OrdinalIgnoreCase);
        var mapping = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (from, to) in request.Mapping)
        {
            if (!names.TryGetValue(from, out var realFrom) || !names.TryGetValue(to, out var realTo))
                return BadRequest(new { error = $"العمود «{from}» أو «{to}» غير موجود في الجدول {table.Table}." });
            if (realFrom != realTo) mapping[realFrom] = realTo;
        }
        if (mapping.Count == 0) return BadRequest(new { error = "لم يتغيّر أي عمود." });

        var quote = _db.Provider == DbProvider.Sqlite
            ? (Func<string, string>)(n => $"\"{n.Replace("\"", "\"\"")}\"")
            : n => $"[{n.Replace("]", "]]")}]";
        var sql = SqlColumnRewriter.Rewrite(request.Sql, mapping, quote);
        try
        {
            var result = await _queries.ExecuteSqlFilterAsync(table.Table, sql, null, selection, ct);
            // مفاتيح الصفوف التي تغيّرت فعلًا (عمود بلا اسم مستعار) ليحدّث العميل xKey/yKey.
            var first = (result.Data as IEnumerable<object>)?.FirstOrDefault() as IDictionary<string, object?>;
            var renamed = mapping.Where(kv => first is not null && first.ContainsKey(kv.Value) && !first.ContainsKey(kv.Key))
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            if (request.Learn && !string.IsNullOrWhiteSpace(context.ProjectId))
            {
                // تعلّم طبقة المعنى: العمود الذي اختاره المستخدم يحمل معنى العمود الذي استُبدل.
                var rows = await _semanticsStore.ListForTableAsync(context.ProjectId, table.Table, ct);
                foreach (var (from, to) in mapping)
                {
                    var old = rows.FirstOrDefault(r => r.ColumnName.Equals(from, StringComparison.OrdinalIgnoreCase));
                    var target = rows.FirstOrDefault(r => r.ColumnName.Equals(to, StringComparison.OrdinalIgnoreCase));
                    if (old?.Concept is null || target?.Source == SemanticSources.User) continue;
                    // للمشروع فقط (لا سابقة عابرة للعملاء): استبدال عمود في رسم واحد أضعف من تأكيد صريح في «المصادر».
                    try { await _semanticsStore.SetByUserAsync(context.ProjectId, table.Table, to, old.Concept, old.Role, old.Unit, old.ValueMapJson, ct); }
                    catch (Exception ex) { _logger.LogInformation(ex, "تعذّر نقل معنى {From} إلى {To}", from, to); }
                }
            }
            var source = $"من الجدول {table.Table} بعد تصحيح الأعمدة ({string.Join("، ", mapping.Select(kv => $"{kv.Key} ← {kv.Value}"))}). حُسب بالاستعلام: {sql}.";
            return Ok(new { sql, table = table.Table, data = result.Data, renamedKeys = renamed, source });
        }
        catch (WidgetQueryValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "فشل تنفيذ الاستعلام بعد تصحيح الأعمدة");
            return BadRequest(new { error = "تعذّر تنفيذ الاستعلام بعد الاستبدال — جرّب عمودًا آخر من النوع نفسه." });
        }
    }

    private async Task<Models.DashboardWidget> RebuildLegacyAsync(WidgetRebuildRequest request, SourceSelection selection, AppUser user, CancellationToken ct)
    {
        var question = $"ابنِ عنصرًا واحدًا فقط بعنوان «{request.Title}» من نوع {request.Type ?? "kpi"}" +
            (string.IsNullOrWhiteSpace(request.Prompt) ? "" : $" ({request.Prompt})") +
            " من بيانات المشروع المتاحة. لا تضف أي عنصر آخر، واحتفظ بالعنوان حرفيًا. إن لم تكفِ البيانات فأعد العنصر بـ noData وسبب واضح.";
        var spec = await _generator.GenerateDashboardAsync(question, null, selection, null, user, request.Lang, ct: ct);
        var widget = spec.Widgets.FirstOrDefault(w => string.Equals(w.Title, request.Title, StringComparison.OrdinalIgnoreCase))
            ?? spec.Widgets.FirstOrDefault(w => !w.NoData) ?? spec.Widgets.FirstOrDefault()
            ?? new Models.DashboardWidget { Type = request.Type ?? "kpi", Title = request.Title, NoData = true,
                MissingReason = "لم يُرجع الموديل عنصرًا.", Data = System.Text.Json.JsonDocument.Parse("[]").RootElement };
        widget.Title = request.Title;
        if (widget.Data.ValueKind == System.Text.Json.JsonValueKind.Undefined) widget.Data = System.Text.Json.JsonDocument.Parse("[]").RootElement;
        return widget;
    }

    /// <summary>يعيد بناء عنصر واحد (من قالبه إن عُرف، وإلا من عنوانه) بعد تصحيح المعاني.</summary>
    [HttpPost("rebuild")]
    public async Task<IActionResult> Rebuild([FromBody] WidgetRebuildRequest request, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest(new { error = "عنوان العنصر مطلوب." });
        var selection = await _permissions.GetEffectiveSelectionAsync(user, request.Sources, ct);
        try
        {
            // ١) من القالب إن عُرف مفتاحه والعنوان فيه (وصف العنصر الأصلي وقواعد المنصة).
            StagedBuildRequest? staged = null;
            if (!string.IsNullOrWhiteSpace(request.TemplateKey))
            {
                var templateRef = new TemplateRef { Kind = TemplateKinds.Dashboard, Key = request.TemplateKey };
                var (inputs, _) = await _templatePrompts.ResolveForStagedAsync(templateRef, user, selection, ct);
                if (inputs is not null && inputs.Widgets.Any(w => string.Equals(w.Title, request.Title, StringComparison.OrdinalIgnoreCase)))
                    staged = new StagedBuildRequest(inputs.Name, inputs.Body, inputs.Widgets, inputs.GeneralRules, inputs.Hints, inputs.Context, request.Lang, user);
            }
            // ٢) وإلا عنصر مفرد من عنوانه ونوعه (لوحة أُعيد فتحها بلا مفتاح قالب، أو عنصر من المحادثة).
            if (staged is null)
            {
                var context = await _tools.DescribeSourcesAsync(selection, ct);
                var spec = new BuiltinDashboardWidgetSpec(request.Type ?? "kpi", request.Title, request.Prompt);
                staged = new StagedBuildRequest("عنصر مفرد", $"عنصر واحد بعنوان «{request.Title}» من بيانات المشروع.", new[] { spec }, null,
                    new Dictionary<string, SchemaLibrary.SchemaMatchingService.MatchedWidget>(), context, request.Lang, user);
            }
            var widget = await _staged.BuildOneAsync(staged, request.Title, ct);
            // ٣) مزوّد بلا مسار مرحلي (Ollama): المسار القديم بسؤال يطلب هذا العنصر وحده.
            widget ??= await RebuildLegacyAsync(request, selection, user, ct);
            return Ok(new { widget });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "فشل إعادة بناء العنصر «{Title}»", request.Title);
            return StatusCode(500, new { error = "تعذّر إعادة بناء العنصر: " + ex.Message });
        }
    }
}
