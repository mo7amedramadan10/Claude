using System.Text;
using ChatToDashboard.Api.Organizations;
using ChatToDashboard.Api.Presentations;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>
/// العروض التقديمية (صفحات HTML مرفوعة) على مستوى المنظمة — انظر <see cref="Presentation"/>.
/// المسار العام <c>/p/{id}</c> يعرض صفحة مشاهدة (wwwroot/presentation.html) تضع المحتوى داخل iframe
/// بسمة <c>sandbox</c> بلا <c>allow-same-origin</c>: فالصفحة المرفوعة تعمل بأصل معتم لا يقرأ كوكي
/// النظام ولا ينادي واجهاته بصلاحيات المشاهد. مسار المحتوى نفسه يرسل ترويسة CSP sandbox احتياطًا
/// لمن يفتحه مباشرة.
/// </summary>
[ApiController]
[Route("api/presentations")]
public class PresentationsController : ControllerBase
{
    /// <summary>حد حجم الصفحة: 15 ميغابايت تكفي لعرض فيه صور مضمّنة (data URI).</summary>
    public const long MaxBytes = 15L * 1024 * 1024;
    private const string SandboxCsp = "sandbox allow-scripts allow-popups allow-forms allow-modals allow-downloads";

    private readonly PresentationStore _store;
    private readonly PresentationDataStore _data;
    private readonly PermissionsService _permissions;
    private readonly OrganizationStore _organizations;
    private readonly PresentationLiveDataService _live;
    private readonly PresentationAssistantService _assistant;

    public PresentationsController(PresentationStore store, PresentationDataStore data, PermissionsService permissions, OrganizationStore organizations, PresentationLiveDataService live, PresentationAssistantService assistant)
    {
        _store = store;
        _data = data;
        _permissions = permissions;
        _organizations = organizations;
        _live = live;
        _assistant = assistant;
    }

    private async Task<string?> OrgSlugAsync(string? organizationId, CancellationToken ct) =>
        organizationId is null ? null : (await _organizations.FindByIdAsync(organizationId, ct))?.Slug;

    private object Shape(Presentation p, AppUser user, string? orgSlug) => new
    {
        p.Id, p.Title, p.Slug, p.FileName, p.SizeBytes, p.CreatedByUserId, p.CreatedByName, p.CreatedAt, p.UpdatedAt, p.ExpiresAt, p.RevokedAt,
        p.ViewCount, p.IsActive, p.EditPolicy, p.SourceKind, p.LiveData, p.Assistant, p.AssistantPolicy, canManage = CanManage(user, p), url = p.PublicPath(orgSlug),
    };

    /// <summary>اسم الرابط النهائي: المُدخل أو المشتق من العنوان، مع 409 إن كان مستخدمًا داخل المنظمة (ومقترح بديل).</summary>
    private async Task<(string? Slug, IActionResult? Fail)> ResolveSlugAsync(string? organizationId, string? requested, string title, string? exceptId, CancellationToken ct)
    {
        var slug = PresentationSlug.Normalize(string.IsNullOrWhiteSpace(requested) ? title : requested);
        if (slug.Length < 2) slug = "presentation";
        if (!PresentationSlug.IsValid(slug))
            return (null, BadRequest(new { error = "اسم الرابط يجب أن يكون حروفًا أو أرقامًا أو شرطات فقط (2–80 حرفًا)." }));
        if (await _store.SlugTakenAsync(organizationId, slug, exceptId, ct))
        {
            var suggestion = slug; var n = 1;
            while (await _store.SlugTakenAsync(organizationId, suggestion, exceptId, ct)) suggestion = $"{slug}-{++n}";
            return (null, Conflict(new { error = $"اسم الرابط «{slug}» مستخدم في منظمتك — جرّب «{suggestion}».", suggestion }));
        }
        return (slug, null);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var list = await _store.ListAsync(user.OrganizationId, ct);
        var orgSlug = await OrgSlugAsync(user.OrganizationId, ct);
        return Ok(new { orgSlug, items = list.Select(p => Shape(p, user, orgSlug)) });
    }

    /// <summary>هل اسم الرابط متاح داخل منظمة المستخدم؟ (للتحقق الحي أثناء الكتابة) — يعيد الاسم بعد التطبيع.</summary>
    [HttpGet("slug-available")]
    public async Task<IActionResult> SlugAvailable([FromQuery] string? slug, [FromQuery] string? exceptId, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var normalized = PresentationSlug.Normalize(slug);
        if (!PresentationSlug.IsValid(normalized)) return Ok(new { slug = normalized, available = false, reason = "حروف وأرقام وشرطات فقط، من حرفين إلى 80." });
        var taken = await _store.SlugTakenAsync(user.OrganizationId, normalized, exceptId, ct);
        return Ok(new { slug = normalized, available = !taken, reason = taken ? "مستخدم في منظمتك." : null, orgSlug = await OrgSlugAsync(user.OrganizationId, ct) });
    }

    /// <summary>رفع عرض جديد: ملف .html/.htm واحد + عنوان + تاريخ انتهاء اختياري.</summary>
    [HttpPost]
    [RequestSizeLimit(MaxBytes + 1024 * 1024)]
    public async Task<IActionResult> Create([FromForm] IFormFile? file, [FromForm] string? title, [FromForm] string? slug, [FromForm] DateTime? expiresAt, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var (html, error) = await ReadHtmlAsync(file, ct);
        if (error is not null) return BadRequest(new { error });
        var finalTitle = string.IsNullOrWhiteSpace(title) ? Path.GetFileNameWithoutExtension(file!.FileName) : title.Trim();
        var (finalSlug, fail) = await ResolveSlugAsync(user.OrganizationId, slug, finalTitle, null, ct);
        if (fail is not null) return fail;
        var presentation = new Presentation
        {
            OrganizationId = user.OrganizationId, CreatedByUserId = user.Id, CreatedByName = user.DisplayName ?? user.Username,
            Title = finalTitle, Slug = finalSlug,
            FileName = Path.GetFileName(file!.FileName), Html = html!, SizeBytes = Encoding.UTF8.GetByteCount(html!),
            ExpiresAt = expiresAt?.ToUniversalTime(),
        };
        await _store.SaveAsync(presentation, ct);
        var orgSlug = await OrgSlugAsync(user.OrganizationId, ct);
        return Ok(new { presentation.Id, presentation.Title, presentation.Slug, orgSlug, url = presentation.PublicPath(orgSlug) });
    }

    public class SlugRequest { public string? Slug { get; set; } }

    /// <summary>إعادة تسمية اسم الرابط (الرابط القديم بالاسم يتوقف؛ الرابط بالمعرّف يبقى).</summary>
    [HttpPut("{id}/slug")]
    public async Task<IActionResult> SetSlug(string id, [FromBody] SlugRequest request, CancellationToken ct)
    {
        var (user, p, fail) = await ManagedAsync(id, ct);
        if (fail is not null) return fail;
        var (slug, slugFail) = await ResolveSlugAsync(p!.OrganizationId, request.Slug, p.Title, id, ct);
        if (slugFail is not null) return slugFail;
        await _store.SetSlugAsync(id, slug!, ct);
        var orgSlug = await OrgSlugAsync(user!.OrganizationId, ct);
        return Ok(new { id, slug, url = $"/p/{orgSlug}/{slug}" });
    }

    /// <summary>العرض بالاسم داخل المنظمة (`/p/{org}/{slug}`) — بلا تسجيل دخول.</summary>
    [HttpGet("by/{org}/{slug}/meta")]
    [AllowAnonymous]
    public async Task<IActionResult> MetaBySlug(string org, string slug, CancellationToken ct)
    {
        var p = await FindBySlugAsync(org, slug, ct);
        return p is null ? NotFound(new { error = "الرابط غير موجود أو تم حذفه." }) : await Meta(p.Id, ct);
    }

    [HttpGet("by/{org}/{slug}/content")]
    [AllowAnonymous]
    public async Task<IActionResult> ContentBySlug(string org, string slug, CancellationToken ct)
    {
        var p = await FindBySlugAsync(org, slug, ct);
        return p is null ? NotFound() : await Content(p.Id, ct);
    }

    [HttpGet("by/{org}/{slug}/live-data")]
    [AllowAnonymous]
    public async Task<IActionResult> LiveDataBySlug(string org, string slug, CancellationToken ct)
    {
        var p = await FindBySlugAsync(org, slug, ct);
        return p is null ? NotFound() : await LiveData(p.Id, ct);
    }

    private async Task<Presentation?> FindBySlugAsync(string org, string slug, CancellationToken ct)
    {
        var organization = await _organizations.FindBySlugAsync(org, ct);
        return organization is null ? null : await _store.GetBySlugAsync(organization.Id, slug, ct);
    }

    /// <summary>استبدال المحتوى بنسخة أحدث على نفس الرابط (صاحبه أو المسؤول).</summary>
    [HttpPut("{id}/file")]
    [RequestSizeLimit(MaxBytes + 1024 * 1024)]
    public async Task<IActionResult> Replace(string id, [FromForm] IFormFile? file, [FromForm] string? title, CancellationToken ct)
    {
        var (user, p, fail) = await ManagedAsync(id, ct);
        if (fail is not null) return fail;
        var (html, error) = await ReadHtmlAsync(file, ct);
        if (error is not null) return BadRequest(new { error });
        await _store.ReplaceAsync(id, html!, Path.GetFileName(file!.FileName), string.IsNullOrWhiteSpace(title) ? null : title.Trim(), ct);
        _assistant.Invalidate(id);
        return NoContent();
    }

    public class ExpiryRequest { public DateTime? ExpiresAt { get; set; } }

    /// <summary>تمديد/تغيير الصلاحية أو إعادة تفعيل رابط مُلغى.</summary>
    [HttpPut("{id}/expiry")]
    public async Task<IActionResult> SetExpiry(string id, [FromBody] ExpiryRequest request, CancellationToken ct)
    {
        var (_, _, fail) = await ManagedAsync(id, ct);
        if (fail is not null) return fail;
        await _store.SetExpiryAsync(id, request.ExpiresAt?.ToUniversalTime(), ct);
        return NoContent();
    }

    [HttpPost("{id}/revoke")]
    public async Task<IActionResult> Revoke(string id, CancellationToken ct)
    {
        var (_, _, fail) = await ManagedAsync(id, ct);
        if (fail is not null) return fail;
        await _store.RevokeAsync(id, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var (_, _, fail) = await ManagedAsync(id, ct);
        if (fail is not null) return fail;
        await _store.DeleteAsync(id, ct);
        await _data.DeleteAllAsync(id, ct);
        return NoContent();
    }

    /// <summary>بيانات العرض لصفحة المشاهدة العامة (العنوان وصاحبه وحالته) — بلا تسجيل دخول.</summary>
    [HttpGet("{id}/meta")]
    [AllowAnonymous]
    public async Task<IActionResult> Meta(string id, CancellationToken ct)
    {
        var p = await _store.GetAsync(id, ct);
        if (p is null) return NotFound(new { error = "الرابط غير موجود أو تم حذفه." });
        if (!p.IsActive)
            return StatusCode(StatusCodes.Status410Gone, new
            {
                inactive = true,
                reason = p.RevokedAt is not null ? $"تم إلغاء هذا الرابط من قِبل {p.CreatedByName ?? "صاحب الرابط"}." : "انتهت صلاحية هذا الرابط.",
                creatorName = p.CreatedByName,
            });
        return Ok(new { p.Id, p.Title, p.CreatedByName, p.UpdatedAt });
    }

    /// <summary>محتوى الصفحة كما رُفع — يُحمَّل داخل iframe معزول في صفحة المشاهدة. يزيد عدّاد المشاهدات.</summary>
    [HttpGet("{id}/content")]
    [AllowAnonymous]
    public async Task<IActionResult> Content(string id, CancellationToken ct)
    {
        var p = await _store.GetAsync(id, ct);
        if (p is null || !p.IsActive) return NotFound();
        var html = await _store.GetHtmlAsync(id, ct) ?? "";
        // البيانات الحيّة: صفحة منشورة من داخل المنصة تُقدَّم بآخر نتائج استعلامات مصدرها لا بلقطة النشر.
        if (PresentationLiveDataService.IsLive(p) && await _live.GetAsync(p, force: false, ct) is { } live)
            html = PresentationLiveDataService.ApplyToHtml(html, p, live);
        await _store.IncrementViewCountAsync(id, ct);
        Response.Headers["Content-Security-Policy"] = SandboxCsp;
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers["Referrer-Policy"] = "no-referrer";
        Response.Headers["Cache-Control"] = "no-store";
        return Content(html, "text/html; charset=utf-8");
    }

    /// <summary>البيانات الحيّة للصفحة المفتوحة (تطلبها الصفحة دوريًا عبر الجسر <c>jeem.data</c>) — بلا تسجيل دخول كالمحتوى نفسه.
    /// <c>{live:false}</c> لعرض بلا مصدر حيّ؛ وإلا <c>{refreshedAt, queries, spec?, failed}</c> من الذاكرة المؤقتة (60 ثانية).</summary>
    [HttpGet("{id}/live-data")]
    [AllowAnonymous]
    public async Task<IActionResult> LiveData(string id, CancellationToken ct)
    {
        var p = await _store.GetAsync(id, ct);
        if (p is null || !p.IsActive) return NotFound();
        Response.Headers["Cache-Control"] = "no-store";
        if (!PresentationLiveDataService.IsLive(p)) return Ok(new { live = false });
        var live = await _live.GetAsync(p, force: false, ct);
        if (live is null) return Ok(new { live = false, error = "مصدر العرض لم يعد موجودًا." });
        var o = live.ToJson(); o["live"] = true;
        return Content(o.ToJsonString(), "application/json; charset=utf-8");
    }

    public class LiveDataRequest { public bool On { get; set; } }

    /// <summary>تشغيل/إيقاف البيانات الحيّة — صاحب العرض أو المسؤول. لا يعمل لعرض بلا مصدر داخل المنصة.</summary>
    [HttpPut("{id}/live-data")]
    public async Task<IActionResult> SetLiveData(string id, [FromBody] LiveDataRequest request, CancellationToken ct)
    {
        var (_, p, fail) = await ManagedAsync(id, ct);
        if (fail is not null) return fail;
        if (request.On && (p!.SourceKind is null || p.SourceId is null)) return BadRequest(new { error = "هذا العرض ملف مرفوع يدويًا بلا مصدر بيانات داخل المنصة، فلا بيانات حيّة له." });
        await _store.SetLiveDataAsync(id, request.On, ct);
        _live.Invalidate(id);
        _assistant.Invalidate(id);
        return Ok(new { liveData = request.On });
    }

    // ---------- مساعد «جيم» في صفحة العرض (انظر PresentationAssistantService) ----------

    public class AssistantSettingsRequest { public bool Enabled { get; set; } = true; public string? Policy { get; set; } }
    public class AssistantAskRequest { public string? Question { get; set; } public List<AssistantTurnRequest>? History { get; set; } }
    public class AssistantTurnRequest { public string? Role { get; set; } public string? Text { get; set; } }

    /// <summary>تشغيل/إيقاف المساعد وسياسة من يسأله (anyone/members) — صاحب العرض أو المسؤول.</summary>
    [HttpPut("{id}/assistant")]
    public async Task<IActionResult> SetAssistant(string id, [FromBody] AssistantSettingsRequest request, CancellationToken ct)
    {
        var (_, _, fail) = await ManagedAsync(id, ct);
        if (fail is not null) return fail;
        var policy = string.IsNullOrWhiteSpace(request.Policy) ? PresentationEditPolicy.Anyone : request.Policy.Trim();
        if (policy is not (PresentationEditPolicy.Anyone or PresentationEditPolicy.Members)) return BadRequest(new { error = "سياسة غير معروفة: anyone أو members." });
        await _store.SetAssistantAsync(id, request.Enabled, policy, ct);
        return Ok(new { assistant = request.Enabled, assistantPolicy = policy });
    }

    /// <summary>حالة المساعد لمن يفتح الرابط: مفعَّل؟ مسموح له؟ — بلا تسجيل دخول.</summary>
    [HttpGet("{id}/assistant")]
    [AllowAnonymous]
    public Task<IActionResult> AssistantInfo(string id, CancellationToken ct) => AssistantInfoCoreAsync(() => _store.GetAsync(id, ct), ct);

    [HttpGet("by/{org}/{slug}/assistant")]
    [AllowAnonymous]
    public Task<IActionResult> AssistantInfoBySlug(string org, string slug, CancellationToken ct) => AssistantInfoCoreAsync(() => FindBySlugAsync(org, slug, ct), ct);

    private async Task<IActionResult> AssistantInfoCoreAsync(Func<Task<Presentation?>> find, CancellationToken ct)
    {
        var p = await find();
        if (p is null || !p.IsActive) return NotFound(new { error = "الرابط غير موجود أو غير نشط." });
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        var allowed = PresentationAssistantService.CanAsk(user, p);
        Response.Headers["Cache-Control"] = "no-store";
        return Ok(new
        {
            name = PresentationAssistantService.Name, enabled = p.Assistant, allowed, signedIn = user is not null, policy = p.AssistantPolicy,
            reason = !p.Assistant ? "المساعد موقوف لهذا العرض." : allowed ? null : user is null ? "سجّل الدخول بحساب المنظمة لسؤال جيم عن هذا العرض." : "المساعد متاح لأعضاء منظمة صاحب العرض فقط.",
        });
    }

    /// <summary>سؤال المساعد عن محتوى العرض كله؛ المحادثة السابقة تُرسل من المتصفح ولا تُخزَّن. 429 عند تجاوز السقف.</summary>
    [HttpPost("{id}/assistant/ask")]
    [AllowAnonymous]
    public Task<IActionResult> AssistantAsk(string id, [FromBody] AssistantAskRequest request, CancellationToken ct) => AssistantAskCoreAsync(() => _store.GetAsync(id, ct), request, ct);

    [HttpPost("by/{org}/{slug}/assistant/ask")]
    [AllowAnonymous]
    public Task<IActionResult> AssistantAskBySlug(string org, string slug, [FromBody] AssistantAskRequest request, CancellationToken ct) => AssistantAskCoreAsync(() => FindBySlugAsync(org, slug, ct), request, ct);

    private async Task<IActionResult> AssistantAskCoreAsync(Func<Task<Presentation?>> find, AssistantAskRequest request, CancellationToken ct)
    {
        var p = await find();
        if (p is null || !p.IsActive) return NotFound(new { error = "الرابط غير موجود أو غير نشط." });
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (!p.Assistant) return StatusCode(403, new { error = "المساعد موقوف لهذا العرض." });
        if (!PresentationAssistantService.CanAsk(user, p))
            return StatusCode(user is null ? 401 : 403, new { error = user is null ? "سجّل الدخول بحساب المنظمة لسؤال جيم عن هذا العرض." : "المساعد متاح لأعضاء منظمة صاحب العرض فقط." });
        var question = (request.Question ?? "").Trim();
        if (question.Length == 0) return BadRequest(new { error = "اكتب سؤالًا." });
        if (question.Length > PresentationAssistantService.MaxQuestionChars) return BadRequest(new { error = $"السؤال أطول من الحد ({PresentationAssistantService.MaxQuestionChars} حرفًا)." });
        var viewerKey = user?.Id ?? HttpContext.Connection.RemoteIpAddress?.ToString() ?? "anon";
        if (!_assistant.TryConsume(p.Id, viewerKey)) return StatusCode(429, new { error = "أسئلة كثيرة في وقت قصير — انتظر قليلًا ثم أعد المحاولة." });
        var history = (request.History ?? new()).Where(h => !string.IsNullOrWhiteSpace(h.Text)).TakeLast(PresentationAssistantService.MaxHistoryTurns)
            .Select(h => new AssistantTurn(h.Role == "user" ? "user" : "assistant", h.Text!.Trim())).ToList();
        try
        {
            var reply = await _assistant.AskAsync(p, question, history, ct);
            return Ok(new { reply });
        }
        catch (InvalidOperationException ex) { return StatusCode(503, new { error = ex.Message }); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return StatusCode(502, new { error = "تعذّر الحصول على رد من المساعد الآن — حاول مرة أخرى.", detail = ex.Message });
        }
    }

    // ---------- بيانات العرض (مخزن مفتاح ← JSON تقرأه/تكتبه الصفحة عبر jeem-page.js) ----------

    public class EditPolicyRequest { public string? Policy { get; set; } }
    public class DataWriteRequest { public System.Text.Json.JsonElement Value { get; set; } public long? Version { get; set; } }

    /// <summary>سياسة الكتابة في بيانات العرض (anyone/members/owner) — صاحب العرض أو المسؤول.</summary>
    [HttpPut("{id}/edit-policy")]
    public async Task<IActionResult> SetEditPolicy(string id, [FromBody] EditPolicyRequest request, CancellationToken ct)
    {
        var (_, _, fail) = await ManagedAsync(id, ct);
        if (fail is not null) return fail;
        if (!PresentationEditPolicy.IsValid(request.Policy)) return BadRequest(new { error = "سياسة غير معروفة: anyone أو members أو owner." });
        await _store.SetEditPolicyAsync(id, request.Policy!, ct);
        return NoContent();
    }

    /// <summary>ما يستطيعه من يفتح الرابط الآن: هل هو مسجَّل؟ هل يستطيع الكتابة بحسب سياسة العرض؟ — بلا تسجيل دخول.</summary>
    [HttpGet("{id}/access")]
    [AllowAnonymous]
    public Task<IActionResult> Access(string id, CancellationToken ct) => AccessCoreAsync(() => _store.GetAsync(id, ct), ct);

    [HttpGet("by/{org}/{slug}/access")]
    [AllowAnonymous]
    public Task<IActionResult> AccessBySlug(string org, string slug, CancellationToken ct) => AccessCoreAsync(() => FindBySlugAsync(org, slug, ct), ct);

    private async Task<IActionResult> AccessCoreAsync(Func<Task<Presentation?>> find, CancellationToken ct)
    {
        var p = await find();
        if (p is null || !p.IsActive) return NotFound(new { error = "الرابط غير موجود أو غير نشط." });
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        var canEdit = CanWriteData(user, p);
        return Ok(new { presentationId = p.Id, signedIn = user is not null, name = user?.DisplayName, canEdit, p.EditPolicy });
    }

    [HttpGet("{id}/data")]
    [AllowAnonymous]
    public Task<IActionResult> DataList(string id, CancellationToken ct) => DataListCoreAsync(() => _store.GetAsync(id, ct), ct);

    [HttpGet("by/{org}/{slug}/data")]
    [AllowAnonymous]
    public Task<IActionResult> DataListBySlug(string org, string slug, CancellationToken ct) => DataListCoreAsync(() => FindBySlugAsync(org, slug, ct), ct);

    private async Task<IActionResult> DataListCoreAsync(Func<Task<Presentation?>> find, CancellationToken ct)
    {
        var p = await find();
        if (p is null || !p.IsActive) return NotFound(new { error = "الرابط غير موجود أو غير نشط." });
        var keys = await _data.ListAsync(p.Id, ct);
        return Ok(new { keys = keys.Select(k => new { k.Key, k.Version, k.UpdatedAt, k.UpdatedBy, k.Bytes }) });
    }

    /// <summary>قراءة قيمة: عامة لأي من يفتح الرابط (كما الصفحة نفسها). مفتاح غير موجود → قيمة null وإصدار 0.</summary>
    [HttpGet("{id}/data/{key}")]
    [AllowAnonymous]
    public Task<IActionResult> DataGet(string id, string key, CancellationToken ct) => DataGetCoreAsync(() => _store.GetAsync(id, ct), key, ct);

    [HttpGet("by/{org}/{slug}/data/{key}")]
    [AllowAnonymous]
    public Task<IActionResult> DataGetBySlug(string org, string slug, string key, CancellationToken ct) => DataGetCoreAsync(() => FindBySlugAsync(org, slug, ct), key, ct);

    private async Task<IActionResult> DataGetCoreAsync(Func<Task<Presentation?>> find, string key, CancellationToken ct)
    {
        var p = await find();
        if (p is null || !p.IsActive) return NotFound(new { error = "الرابط غير موجود أو غير نشط." });
        if (!PresentationDataStore.IsValidKey(key)) return BadRequest(new { error = "مفتاح غير صالح." });
        var entry = await _data.GetAsync(p.Id, key, ct);
        Response.Headers["Cache-Control"] = "no-store";
        if (entry is null) return Ok(new { key, value = (object?)null, version = 0L, updatedAt = (DateTime?)null, updatedBy = (string?)null });
        return Ok(new { key, value = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(entry.Json), entry.Version, entry.UpdatedAt, entry.UpdatedBy });
    }

    /// <summary>كتابة قيمة بحسب سياسة العرض (anyone افتراضيًا)، مع إصدار متوقع اختياري (409 عند التعارض).</summary>
    [HttpPut("{id}/data/{key}")]
    [AllowAnonymous]
    public Task<IActionResult> DataSet(string id, string key, [FromBody] DataWriteRequest request, CancellationToken ct) =>
        DataSetCoreAsync(() => _store.GetAsync(id, ct), key, request, ct);

    [HttpPut("by/{org}/{slug}/data/{key}")]
    [AllowAnonymous]
    public Task<IActionResult> DataSetBySlug(string org, string slug, string key, [FromBody] DataWriteRequest request, CancellationToken ct) =>
        DataSetCoreAsync(() => FindBySlugAsync(org, slug, ct), key, request, ct);

    private async Task<IActionResult> DataSetCoreAsync(Func<Task<Presentation?>> find, string key, DataWriteRequest request, CancellationToken ct)
    {
        var p = await find();
        if (p is null || !p.IsActive) return NotFound(new { error = "الرابط غير موجود أو غير نشط." });
        if (!PresentationDataStore.IsValidKey(key)) return BadRequest(new { error = "مفتاح غير صالح." });
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (!CanWriteData(user, p))
            return StatusCode(user is null ? 401 : 403, new { error = user is null ? "سجّل الدخول لتعديل بيانات هذا العرض." : "لا تملك صلاحية تعديل بيانات هذا العرض." });
        var json = request.Value.ValueKind == System.Text.Json.JsonValueKind.Undefined ? "null" : request.Value.GetRawText();
        if (Encoding.UTF8.GetByteCount(json) > PresentationDataStore.MaxJsonBytes)
            return BadRequest(new { error = $"حجم البيانات يتجاوز الحد ({PresentationDataStore.MaxJsonBytes / 1024} كيلوبايت)." });
        var (ok, version, error) = await _data.SetAsync(p.Id, key, json, request.Version, user?.DisplayName ?? user?.Username ?? "زائر", ct);
        if (!ok) return StatusCode(error!.StartsWith("تعارض") ? 409 : 400, new { error, version });
        return Ok(new { key, version, updatedAt = DateTime.UtcNow });
    }

    [HttpDelete("{id}/data/{key}")]
    public async Task<IActionResult> DataDelete(string id, string key, CancellationToken ct)
    {
        var (_, _, fail) = await ManagedAsync(id, ct);
        if (fail is not null) return fail;
        await _data.DeleteAsync(id, key, ct);
        return NoContent();
    }

    /// <summary>سياسة الكتابة: anyone = أي من يفتح الرابط (حتى بلا دخول)؛ members = مسجَّل من نفس المنظمة؛ owner = صاحبه/المسؤول.</summary>
    internal static bool CanWriteData(AppUser? user, Presentation p) => p.EditPolicy switch
    {
        PresentationEditPolicy.Members => user is not null && string.Equals(user.OrganizationId, p.OrganizationId, StringComparison.Ordinal),
        PresentationEditPolicy.Owner => user is not null && string.Equals(user.OrganizationId, p.OrganizationId, StringComparison.Ordinal) && CanManage(user, p),
        _ => true,
    };

    private static bool CanManage(AppUser user, Presentation p) =>
        user.Role == UserRoles.Admin || string.Equals(p.CreatedByUserId, user.Id, StringComparison.Ordinal);

    private async Task<(AppUser? User, Presentation? Presentation, IActionResult? Fail)> ManagedAsync(string id, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return (null, null, Unauthorized());
        var p = await _store.GetAsync(id, ct);
        if (p is null || !string.Equals(p.OrganizationId, user.OrganizationId, StringComparison.Ordinal))
            return (user, null, NotFound(new { error = "العرض غير موجود." }));
        if (!CanManage(user, p)) return (user, p, StatusCode(403, new { error = "يمكن لصاحب العرض أو المسؤول فقط تعديله." }));
        return (user, p, null);
    }

    private static async Task<(string? Html, string? Error)> ReadHtmlAsync(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0) return (null, "اختر ملف HTML أولًا.");
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".html" or ".htm")) return (null, "الملف يجب أن يكون صفحة HTML واحدة (.html).");
        if (file.Length > MaxBytes) return (null, $"حجم الملف يتجاوز الحد ({MaxBytes / 1024 / 1024} ميغابايت). ضمّن الصور بحجم أصغر أو اربطها من رابط خارجي.");
        using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var html = await reader.ReadToEndAsync(ct);
        if (string.IsNullOrWhiteSpace(html)) return (null, "الملف فارغ.");
        return (html, null);
    }
}
