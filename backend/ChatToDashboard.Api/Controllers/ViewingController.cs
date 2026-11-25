using System.Text.Json;
using ChatToDashboard.Api.Freeform;
using ChatToDashboard.Api.History;
using ChatToDashboard.Api.Organizations;
using ChatToDashboard.Api.Presentations;
using ChatToDashboard.Api.Share;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>
/// صفحة «للاطلاع» داخل المشروع الحالي: كل ما له رابط منشور فعّال مما يراه المستخدم — تصاميم «تصميم حر» بأنواعها الخمسة (منشوراته هو،
/// أو مشاركة معه بالاسم أو مع المشروع أو المنظمة) ولوحات المتابعة المفعّلة التي يملكها أو له دور فيها ولها رابط /d/ حيّ. المقصورة على
/// مشروع المستخدم الحالي فقط (تصميم قديم بلا مشروع يُعامل كمشروع غير محدد). كل عنصر بطاقة واحدة مهما تعددت طرق وصوله، والفتح = الرابط
/// المنشور نفسه في تبويب جديد (لا عارض داخل المنصة). ما ليس منشورًا أو رابطه موقوف لا يظهر.
/// </summary>
[ApiController]
[Route("api/viewing")]
[Authorize]
public class ViewingController : ControllerBase
{
    private readonly PermissionsService _permissions;
    private readonly FreeformStore _designs;
    private readonly PresentationStore _presentations;
    private readonly OrganizationStore _organizations;
    private readonly HistoryStore _history;
    private readonly ShareStore _shares;
    private readonly UserStore _users;

    public ViewingController(PermissionsService permissions, FreeformStore designs, PresentationStore presentations, OrganizationStore organizations, HistoryStore history, ShareStore shares, UserStore users)
    { _permissions = permissions; _designs = designs; _presentations = presentations; _organizations = organizations; _history = history; _shares = shares; _users = users; }

    /// <summary>هامش التسامح بين آخر تعديل للتصميم وآخر نشر له (النشر نفسه يلمس تاريخ التصميم أحيانًا).</summary>
    private static readonly TimeSpan EditedSlack = TimeSpan.FromMinutes(2);

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var me = await _permissions.GetCurrentUserAsync(User, ct); if (me is null) return Unauthorized();
        var sel = await _permissions.GetEffectiveSelectionAsync(me, null, ct);
        var isAdmin = me.Role == UserRoles.Admin;
        var orgSlug = me.OrganizationId is null ? null : (await _organizations.FindByIdAsync(me.OrganizationId, ct))?.Slug;
        var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        async Task<string> NameOf(string? uid)
        {
            if (string.IsNullOrWhiteSpace(uid)) return "";
            if (names.TryGetValue(uid, out var n)) return n;
            var u = await _users.FindByIdAsync(uid, ct);
            return names[uid] = u is null ? "" : (string.IsNullOrWhiteSpace(u.DisplayName) ? u.Username : u.DisplayName);
        }
        var items = new List<object>();

        // تصاميم تصميم حر: المرئية للمستخدم (FreeformAccess) في مشروعه الحالي، ولها عرض منشور فعّال — صف واحد لكل تصميم
        var designs = (await _designs.ListVisibleAsync(me.OrganizationId, sel.EntityId, me.Id, isAdmin, ct))
            .Where(d => InEntity(d.EntityId, sel.EntityId));
        var pres = (await _presentations.ListAsync(me.OrganizationId, ct)).ToDictionary(p => p.Id, p => p);
        var myShares = await _designs.ShareRolesForUserAsync(me.Id, ct);
        foreach (var d in designs)
        {
            var mine = string.Equals(d.CreatedByUserId, me.Id, StringComparison.OrdinalIgnoreCase);
            if (d.PresentationId is null || !pres.TryGetValue(d.PresentationId, out var p) || !p.IsActive) continue;
            var byName = myShares.ContainsKey(d.Id);
            items.Add(new
            {
                key = "f:" + d.Id, source = "freeform", id = d.Id, presentationId = p.Id, kind = d.Kind, title = string.IsNullOrWhiteSpace(p.Title) ? d.Title : p.Title,
                url = p.PublicPath(orgSlug), ownerName = mine ? "" : await NameOf(d.CreatedByUserId), mine,
                sharedVia = mine ? "mine" : byName ? "user" : d.ShareScope, publishedAt = p.UpdatedAt, editedAfterPublish = d.UpdatedAt > p.UpdatedAt + EditedSlack,
                live = PresentationLiveDataService.IsLive(p),
            });
        }

        // لوحات المتابعة في المشروع الحالي: المفعّلة التي يملكها المستخدم أو له دور فيها، ولها رابط /d/ حيّ — بأحدث رابط لكل لوحة
        var dashboards = (await _history.ListAsync(me.Id, entityId: sel.EntityId, ct: ct))
            .Where(h => h.IsActive && InEntity(h.EntityId, sel.EntityId))
            .ToDictionary(h => h.Id, h => h);
        var links = await _shares.ListByHistoryIdsAsync(me.OrganizationId, dashboards.Keys.ToList(), ct);
        foreach (var g in links.Where(l => l.IsLive && l.HistoryId is not null).GroupBy(l => l.HistoryId!))
        {
            var link = g.OrderByDescending(l => l.UpdatedAt ?? l.CreatedAt).First();
            var h = dashboards[g.Key];
            var mine = string.Equals(h.OwnerId ?? h.UserId, me.Id, StringComparison.OrdinalIgnoreCase);
            items.Add(new
            {
                key = "d:" + h.Id, source = "dashboard", id = h.Id, kind = "track", title = FirstNonEmpty(link.Question, h.Question, "لوحة متابعة"),
                url = link.PublicPath(orgSlug), ownerName = mine ? "" : await NameOf(h.OwnerId ?? h.UserId), mine, sharedVia = mine ? "mine" : "user",
                publishedAt = link.UpdatedAt ?? link.CreatedAt, editedAfterPublish = false, live = true,
                renderKind = h.RenderKind, widgets = WidgetSketch(link.WidgetsJson),
            });
        }
        return Ok(new { items });
    }

    /// <summary>معاينة بطاقة تصميم: الصفحة المنشورة كما هي (بلا زيادة عدّاد المشاهدات وبلا بيانات حيّة) — لتصميم يراه المستخدم فقط.</summary>
    [HttpGet("thumb/{designId}")]
    public async Task<IActionResult> Thumb(string designId, CancellationToken ct)
    {
        var me = await _permissions.GetCurrentUserAsync(User, ct); if (me is null) return Unauthorized();
        var sel = await _permissions.GetEffectiveSelectionAsync(me, null, ct);
        var d = await _designs.GetByIdAsync(designId, ct);
        if (d?.PresentationId is null) return NotFound();
        var access = FreeformAccess.Resolve(d, me.Id, me.OrganizationId, sel.EntityId, me.Role == UserRoles.Admin, await _designs.ShareRoleAsync(d.Id, me.Id, ct));
        if (access is null) return NotFound();
        var p = await _presentations.GetAsync(d.PresentationId, ct);
        if (p is null || !p.IsActive) return NotFound();
        return Ok(new { html = await _presentations.GetHtmlAsync(p.Id, ct) ?? "", kind = d.Kind });
    }

    /// <summary>العنصر من المشروع الحالي؟ عنصر قديم بلا مشروع يُعرض في كل المشاريع (كما يعامله باقي التطبيق).</summary>
    internal static bool InEntity(string? itemEntity, string? currentEntity)
        => string.IsNullOrWhiteSpace(itemEntity) || string.Equals(itemEntity, currentEntity, StringComparison.OrdinalIgnoreCase);

    private static string FirstNonEmpty(params string?[] values) => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim() ?? "";

    /// <summary>شكل اللوحة للمصغّرة فقط (النوع والحجم والعنوان) — بلا بيانات.</summary>
    internal static List<object> WidgetSketch(string? widgetsJson)
    {
        var list = new List<object>();
        if (string.IsNullOrWhiteSpace(widgetsJson)) return list;
        try
        {
            using var doc = JsonDocument.Parse(widgetsJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return list;
            foreach (var w in doc.RootElement.EnumerateArray().Take(40))
            {
                if (w.ValueKind != JsonValueKind.Object) continue;
                string? S(string k) => w.TryGetProperty(k, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
                list.Add(new { type = S("type") ?? "bar", size = S("size"), title = S("title") ?? "" });
            }
        }
        catch (JsonException) { }
        return list;
    }
}
