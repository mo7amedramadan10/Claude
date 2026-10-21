using System.Security.Claims;
using ChatToDashboard.Api.History;
using ChatToDashboard.Api.Projects;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>
/// شاشة «اللوحات المنشورة» داخل المشروع (§4.23). كل الطلبات هنا على المشروع الحالي للمستخدم
/// (بنفس تعريف <see cref="ProjectStore.ResolveCurrentProjectIdAsync"/> الذي تستخدمه بقية
/// الشاشات). يرى كل عضو فقط ما يسمح به جمهور النشر ودوره في المشروع، والنشر وإعادة الترتيب
/// وإلغاء النشر للمالك والمحرر فقط.
/// </summary>
[ApiController]
[Route("api/published-dashboards")]
public class PublishedDashboardsController : ControllerBase
{
    private readonly PublishedDashboardStore _published;
    private readonly ProjectStore _projects;
    private readonly HistoryStore _history;
    private readonly UserStore _users;
    private readonly DashboardAccessService _access;

    public PublishedDashboardsController(
        PublishedDashboardStore published, ProjectStore projects, HistoryStore history, UserStore users,
        DashboardAccessService access)
    {
        _published = published;
        _projects = projects;
        _history = history;
        _users = users;
        _access = access;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private bool IsAdmin => User.IsInRole(UserRoles.Admin);

    private Task<string?> CurrentProjectIdAsync(CancellationToken ct) =>
        _projects.ResolveCurrentProjectIdAsync(User.FindFirstValue("OrganizationId"), UserId, IsAdmin, ct);

    /// <summary>دور المستخدم الفعلي في المشروع: مسؤول المنظمة يُعامل كمالك، والحساب بلا دور
    /// صريح يُعامل كمشاهد (أقل صلاحية).</summary>
    private async Task<string> MyProjectRoleAsync(string projectId, CancellationToken ct)
    {
        if (IsAdmin) return ProjectRoles.Owner;
        var roles = await _projects.ListRolesAsync(projectId, ct);
        return roles.FirstOrDefault(r => r.UserId == UserId)?.Role ?? ProjectRoles.Viewer;
    }

    /// <summary>اللوحات المنشورة على المشروع الحالي التي يحق للمستخدم رؤيتها، بترتيب التبويبات.</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var projectId = await CurrentProjectIdAsync(ct);
        if (projectId is null) return NotFound(new { error = "الحساب غير مرتبط بأي مشروع." });
        var project = await _projects.FindByIdAsync(projectId, ct);
        var myRole = await MyProjectRoleAsync(projectId, ct);

        var items = new List<object>();
        foreach (var row in await _published.ListAsync(projectId, ct))
        {
            if (!PublishedDashboardRules.CanView(row.Audience, myRole, IsAdmin)) continue;
            // لوحة حُذفت من السجل بعد نشرها: لا تُعرض (الصف يبقى حتى يُلغى نشره صراحةً).
            var entry = await _history.GetByIdAsync(row.DashboardId, ct);
            if (entry is null) continue;
            items.Add(await DescribeAsync(row, entry, ct));
        }

        return Ok(new
        {
            projectId,
            projectName = project?.Name,
            myRole,
            canManage = PublishedDashboardRules.CanManage(myRole, IsAdmin),
            items,
        });
    }

    /// <summary>
    /// ينشر لوحة نشطة (Active) على المشروع الحالي أو يعيد نشرها (يرفع رقم الإصدار). يشترط:
    /// أن يكون المستخدم مالكًا أو محررًا في المشروع، ومالكًا أو محررًا للوحة نفسها، وأن تكون
    /// اللوحة مفعّلة وتابعة لهذا المشروع.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Publish([FromBody] PublishDashboardRequest request, CancellationToken ct)
    {
        var projectId = await CurrentProjectIdAsync(ct);
        if (projectId is null) return NotFound(new { error = "الحساب غير مرتبط بأي مشروع." });
        var myRole = await MyProjectRoleAsync(projectId, ct);
        if (!PublishedDashboardRules.CanManage(myRole, IsAdmin))
            return StatusCode(403, new { error = "النشر على المشروع متاح للمالك والمحرر فقط." });

        var entry = string.IsNullOrWhiteSpace(request.DashboardId) ? null : await _history.GetByIdAsync(request.DashboardId, ct);
        if (entry is null) return NotFound(new { error = "اللوحة غير موجودة." });
        // صف قديم بلا مشروع يُعامل كتابع للمشروع الحالي — نفس قاعدة HistoryStore.ListAsync.
        if (entry.ProjectId is not null && entry.ProjectId != projectId)
            return BadRequest(new { error = "هذه اللوحة تابعة لمشروع آخر." });
        if (!entry.IsActive) return BadRequest(new { error = "فعّل اللوحة أولًا قبل نشرها على المشروع." });

        var dashRole = await _history.ResolveRoleAsync(UserId, entry, ct);
        if (!IsAdmin && dashRole is not ("owner" or DashboardRoles.Editor))
            return StatusCode(403, new { error = "نشر اللوحة متاح لمالكها أو محرريها فقط." });

        var audience = string.IsNullOrWhiteSpace(request.Audience) ? PublishedAudiences.All : request.Audience;
        if (!PublishedAudiences.Values.Contains(audience)) return BadRequest(new { error = "الجمهور غير صحيح." });
        var refreshMode = string.IsNullOrWhiteSpace(request.RefreshMode) ? PublishedRefreshModes.Live : request.RefreshMode;
        if (refreshMode is not (PublishedRefreshModes.Live or PublishedRefreshModes.Scheduled))
            return BadRequest(new { error = "نوع التحديث غير صحيح." });
        var refreshEvery = string.IsNullOrWhiteSpace(request.RefreshEvery) ? null : request.RefreshEvery.Trim();
        if (refreshEvery is { Length: > 100 }) return BadRequest(new { error = "وصف تكرار التحديث طويل جدًا." });

        var kind = string.IsNullOrWhiteSpace(entry.RenderKind) ? PublishedKinds.Jr : PublishedKinds.Interactive;
        var row = await _published.PublishAsync(projectId, entry.Id, kind, UserId, audience, refreshMode, refreshEvery, ct);
        return Ok(await DescribeAsync(row, entry, ct));
    }

    /// <summary>يحفظ ترتيب التبويبات على مستوى المشروع (حقل <c>SortOrder</c>).</summary>
    [HttpPut("order")]
    public async Task<IActionResult> Reorder([FromBody] ReorderPublishedRequest request, CancellationToken ct)
    {
        var projectId = await CurrentProjectIdAsync(ct);
        if (projectId is null) return NotFound(new { error = "الحساب غير مرتبط بأي مشروع." });
        if (!PublishedDashboardRules.CanManage(await MyProjectRoleAsync(projectId, ct), IsAdmin))
            return StatusCode(403, new { error = "إعادة الترتيب متاحة للمالك والمحرر فقط." });

        var current = (await _published.ListAsync(projectId, ct)).Select(r => r.Id).ToList();
        await _published.SaveOrderAsync(projectId, PublishedDashboardRules.ApplyOrder(current, request.Ids ?? new()), ct);
        return NoContent();
    }

    /// <summary>يلغي نشر لوحة من المشروع (اللوحة نفسها تبقى في السجل كما هي).</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Unpublish(string id, CancellationToken ct)
    {
        var projectId = await CurrentProjectIdAsync(ct);
        var row = await _published.GetAsync(id, ct);
        if (projectId is null || row is null || row.ProjectId != projectId) return NotFound(new { error = "غير موجود" });
        if (!PublishedDashboardRules.CanManage(await MyProjectRoleAsync(projectId, ct), IsAdmin))
            return StatusCode(403, new { error = "إلغاء النشر متاح للمالك والمحرر فقط." });

        await _published.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// اللوحة المنشورة كاملة (عناصرها وفلاترها) لوضع التضمين <c>?embed=1&amp;pub=</c> — بنفس
    /// شكل عناصر <c>GET /api/history</c> حتى يفتحها app.js بمسار الفتح العادي، لكن بدور
    /// «مشاهد» دائمًا: العرض المنشور للقراءة فقط، والتعديل من «المحادثة واللوحات».
    /// </summary>
    [HttpGet("{id}/entry")]
    public async Task<IActionResult> Entry(string id, CancellationToken ct)
    {
        var projectId = await CurrentProjectIdAsync(ct);
        var row = await _published.GetAsync(id, ct);
        if (projectId is null || row is null || row.ProjectId != projectId) return NotFound(new { error = "غير موجود" });
        if (!PublishedDashboardRules.CanView(row.Audience, await MyProjectRoleAsync(projectId, ct), IsAdmin))
            return NotFound(new { error = "غير موجود" });
        var entry = await _history.GetByIdAsync(row.DashboardId, ct);
        if (entry is null) return NotFound(new { error = "اللوحة لم تعد موجودة." });

        // نفس فحص الصلاحية الحي الذي يجريه HistoryController.EnrichAsync على مالك اللوحة.
        bool disabled = false;
        string? disabledReason = null;
        if (entry.IsActive)
        {
            var owner = string.IsNullOrWhiteSpace(entry.OwnerId) ? null : await _users.FindByIdAsync(entry.OwnerId, ct);
            var reason = owner is null
                ? "هذه اللوحة ليس لها مالك — لا يمكن تحديثها."
                : await _access.CheckEligibilityAsync(owner, entry.WidgetsJson, ct);
            if (reason is not null)
            {
                disabled = true;
                disabledReason = "هذه اللوحة لم يعد لمالكها صلاحية وصول صالحة للبيانات ولا يمكن تحديثها.";
            }
        }

        return Ok(new
        {
            id = entry.Id,
            question = entry.Question,
            queryDescription = entry.QueryDescription,
            summary = entry.Summary,
            widgetsJson = entry.WidgetsJson,
            filtersJson = entry.FiltersJson,
            activeFiltersJson = entry.ActiveFiltersJson,
            createdAt = entry.CreatedAt,
            isActive = entry.IsActive,
            projectId = entry.ProjectId,
            renderKind = entry.RenderKind,
            templateId = entry.TemplateId,
            myRole = "viewer",
            disabled,
            disabledReason,
        });
    }

    private async Task<object> DescribeAsync(PublishedDashboard row, DashboardHistoryEntry entry, CancellationToken ct)
    {
        var publisher = string.IsNullOrWhiteSpace(row.PublishedBy) ? null : await _users.FindByIdAsync(row.PublishedBy, ct);
        // اللوحة التفاعلية صفحة واحدة دائمًا من منظور هذه الشاشة (محركها يدير عرضه الداخلي).
        var pages = row.Kind == PublishedKinds.Interactive
            ? Array.Empty<string>()
            : PublishedDashboardRules.PagesFromWidgets(entry.WidgetsJson);
        return new
        {
            id = row.Id,
            dashboardId = row.DashboardId,
            name = string.IsNullOrWhiteSpace(entry.Question) ? entry.Summary : entry.Question,
            desc = entry.Summary,
            kind = row.Kind,
            version = row.Version,
            publishedBy = row.PublishedBy,
            publishedByName = publisher?.DisplayName ?? publisher?.Username ?? "—",
            publishedAt = row.PublishedAt,
            audience = row.Audience,
            audienceLabel = PublishedAudiences.Label(row.Audience),
            refreshMode = row.RefreshMode,
            refreshEvery = row.RefreshEvery,
            sortOrder = row.SortOrder,
            pages,
            embedUrl = $"/?embed=1&pub={Uri.EscapeDataString(row.Id)}",
        };
    }
}
