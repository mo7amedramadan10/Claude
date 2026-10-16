using ChatToDashboard.Api.Build;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Repository;
using ChatToDashboard.Api.Semantics;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>
/// صفحة الإدارة المجمّعة لطبقة المعنى (تبويب «معاني الأعمدة» في مكتبة النماذج): كل جداول المشروع
/// الحالي بأعمدتها ومعانيها في شاشة واحدة، مع تصحيح أي عمود وإعادة تحليل أي جدول. نفس المخازن
/// التي تستخدمها نافذة الملف في «المصادر» (RepositoryController) — هذه واجهة أوسع لا طبقة جديدة.
/// </summary>
[ApiController]
[Route("api/semantics")]
public class SemanticsController : ControllerBase
{
    private readonly PermissionsService _permissions;
    private readonly AnalyticsTools _tools;
    private readonly DataProfileService _profiles;
    private readonly ColumnSemanticsStore _store;
    private readonly ColumnSemanticsService _semantics;
    private readonly RepositoryStore _files;

    public SemanticsController(
        PermissionsService permissions, AnalyticsTools tools, DataProfileService profiles,
        ColumnSemanticsStore store, ColumnSemanticsService semantics, RepositoryStore files)
    {
        _permissions = permissions; _tools = tools; _profiles = profiles; _store = store; _semantics = semantics; _files = files;
    }

    /// <summary>كل جداول المشروع الحالي: الأعمدة بنوعها ومعناها ومصدره وثقته، وحالة التحليل لكل جدول.</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var selection = await _permissions.GetEffectiveSelectionAsync(user, null, ct);
        var context = await _tools.DescribeSourcesAsync(selection, ct);
        var profile = await _profiles.BuildAsync(context, ct);
        var projectId = context.ProjectId ?? "";
        var rows = string.IsNullOrWhiteSpace(projectId) ? Array.Empty<ColumnSemantic>() : await _store.ListForProjectAsync(projectId, ct);
        var byKey = rows.ToDictionary(r => (r.TableName.ToLowerInvariant(), r.ColumnName.ToLowerInvariant()), r => r);
        var files = (await _files.ListAsync(selection.ProjectId, ct)).Where(f => !string.IsNullOrWhiteSpace(f.TableName))
            .ToDictionary(f => f.TableName!, f => f, StringComparer.OrdinalIgnoreCase);
        // جدول بلا تحليل من الموديل (ملف قديم، جدول نظام خارجي، أو نداء سابق فشل) يبدأ تحليله الآن في
        // الخلفية ويظهر «قيد التحليل» — فلا يحتاج المستخدم إلى زر لكل جدول.
        var pending = await _semantics.UnanalyzedTablesAsync(projectId, profile.Tables.Select(t => t.Table), ct);
        if (pending.Count > 0)
            _ = _semantics.EnsureAnalyzedAsync(projectId, pending, TimeSpan.Zero, CancellationToken.None);
        var pendingSet = pending.Where(t => _semantics.GetFailure(projectId, t) is null).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var tables = profile.Tables.Select(t =>
        {
            var failure = _semantics.GetFailure(projectId, t.Table);
            var columns = t.Columns.Select(c =>
            {
                byKey.TryGetValue((t.Table.ToLowerInvariant(), c.Name.ToLowerInvariant()), out var s);
                var concept = ConceptCatalog.Find(s?.Concept);
                return new
                {
                    name = c.Name, kind = c.Kind,
                    concept = concept?.Id, conceptLabel = concept?.Label, role = s?.Role, unit = s?.Unit,
                    confidence = s?.Confidence ?? 0, source = s?.Source, note = s?.Note, valueMapJson = s?.ValueMapJson,
                    sample = c.TopValues?.Take(3).Select(v => v.Value).ToList(),
                };
            }).ToList();
            var tableRows = rows.Where(r => r.TableName.Equals(t.Table, StringComparison.OrdinalIgnoreCase)).ToList();
            files.TryGetValue(t.Table, out var file);
            return new
            {
                table = t.Table,
                fileId = file?.Id, fileName = file?.DisplayName ?? context.TableFiles.GetValueOrDefault(t.Table) ?? t.SourceName,
                rowCount = t.RowCount,
                // «محلَّل» = فيه صف من الموديل أو المستخدم؛ «تخمين فقط» = أسماء فقط؛ «لم يُحلَّل» = لا صفوف.
                analysis = tableRows.Any(r => r.Source is SemanticSources.Ai or SemanticSources.User) ? "analyzed"
                    : failure is not null ? "failed" : pendingSet.Contains(t.Table) ? "pending" : tableRows.Count == 0 ? "none" : "rule",
                // سبب آخر فشل لنداء الموديل (إن وُجد) ليُعرض للمستخدم مع إرشاد لإعادة المحاولة.
                analysisError = failure?.Message, analysisFailedAt = failure?.At,
                columns,
            };
        }).ToList();

        return Ok(new { projectId, tables, concepts = ConceptCatalog.Concepts });
    }

    /// <summary>تأكيد/تصحيح معنى عمود من الصفحة المجمّعة (يفوز على أي تحليل آلي لاحق).</summary>
    [HttpPut("{table}/{column}")]
    public async Task<IActionResult> Set(string table, string column, [FromBody] SetColumnSemanticRequest request, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var (context, profiled) = await ResolveTableAsync(user, table, ct);
        if (profiled is null) return NotFound(new { error = "الجدول غير موجود في مصادر المشروع." });
        if (!profiled.Columns.Any(c => c.Name.Equals(column, StringComparison.OrdinalIgnoreCase)))
            return NotFound(new { error = "العمود غير موجود في هذا الجدول." });
        if (request.Concept is not null && !ConceptCatalog.IsKnown(request.Concept))
            return BadRequest(new { error = "المفهوم غير معروف في القاموس." });
        var role = request.Role ?? ConceptCatalog.Find(request.Concept)?.Role ?? ConceptRoles.Text;
        var valueMap = request.ValueMap is { Count: > 0 } ? System.Text.Json.JsonSerializer.Serialize(request.ValueMap) : null;
        await _semantics.SetByUserAsync(context.ProjectId!, profiled.Table, column, request.Concept, role, request.Unit, valueMap, ct);
        return NoContent();
    }

    /// <summary>إعادة تحليل جدول واحد الآن (متزامن) — يبقي تأكيدات المستخدم.</summary>
    [HttpPost("{table}/analyze")]
    public async Task<IActionResult> Analyze(string table, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var (context, profiled) = await ResolveTableAsync(user, table, ct);
        if (profiled is null) return NotFound(new { error = "الجدول غير موجود في مصادر المشروع." });
        var rows = await _semantics.AnalyzeTableAsync(context.ProjectId!, profiled.Table, ct);
        // نجاح الطلب لا يعني نجاح الموديل: عند فشله تُحفظ تخمينات الأسماء فقط ويُعاد السبب للعرض.
        var failure = _semantics.GetFailure(context.ProjectId, profiled.Table);
        return Ok(new { table = profiled.Table, analyzed = rows.Count, analysisError = failure?.Message });
    }

    private async Task<(AnalyticsTools.SourceContext Context, ProfiledTable? Table)> ResolveTableAsync(AppUser user, string table, CancellationToken ct)
    {
        var selection = await _permissions.GetEffectiveSelectionAsync(user, null, ct);
        var context = await _tools.DescribeSourcesAsync(selection, ct);
        if (string.IsNullOrWhiteSpace(context.ProjectId)) return (context, null);
        var profile = await _profiles.BuildAsync(context, ct);
        return (context, profile.Find(table));
    }
}
