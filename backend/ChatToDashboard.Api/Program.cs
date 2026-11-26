using ChatToDashboard.Api.ClaudeChat.Mcp;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;
using System.Security.Cryptography;
using ChatToDashboard.Api.Auth;
using ChatToDashboard.Api.Claude;
using ChatToDashboard.Api.Data;
using ChatToDashboard.Api.History;
using ChatToDashboard.Api.Live;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.OpenAi;
using ChatToDashboard.Api.Repository;
using ChatToDashboard.Api.Share;
using ChatToDashboard.Api.Sources;
using ChatToDashboard.Api.Usage;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Cookie auth: every endpoint requires a signed-in user by default (FallbackPolicy) —
// individual actions opt out with [AllowAnonymous] (login itself, and the public
// GET /api/share/{id} view link). API calls get a 401 instead of a login-page redirect,
// since this is a JSON API consumed by the SPA's own fetch calls, not a browser nav.
// مفاتيح الوصول الشخصية (Users/ApiTokenAuthenticationHandler): طلب يحمل «Bearer jeem_…» يُصادَق بالمفتاح
// بدل الكوكي — لربط Claude Desktop وأي أداة خارجية — عبر سياسة تختار المخطط بحسب الترويسة.
const string SmartScheme = "CookieOrApiToken";
builder.Services.AddSingleton<ApiTokenStore>();
builder.Services.AddAuthentication(SmartScheme)
    .AddPolicyScheme(SmartScheme, "Cookie or API token", options =>
    {
        options.ForwardDefault = CookieAuthenticationDefaults.AuthenticationScheme;
        options.ForwardDefaultSelector = context => ApiTokenAuthenticationHandler.HasToken(context.Request)
            ? ApiTokenAuthenticationHandler.Scheme
            : CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, ApiTokenAuthenticationHandler>(ApiTokenAuthenticationHandler.Scheme, null)
    .AddCookie(options =>
    {
        options.Cookie.Name = "ctd_auth";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = 401; return Task.CompletedTask; };
        options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = 403; return Task.CompletedTask; };
    });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    // Platform-level endpoints (Controllers/OrganizationsController) — every organization,
    // not just the caller's own. A claim rather than a Role value: see AppUser.IsPlatformOwner.
    options.AddPolicy("PlatformOwner", policy => policy.RequireClaim("IsPlatformOwner", "true"));
});

builder.Services.AddSingleton<UserStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.Organizations.OrganizationStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.Entities.EntityStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.Entities.EntityTypeStore>();
// §4.23 — اللوحات المنشورة داخل المشروع (جدول مستقل عن PublishedDashboardSlots الخاص بالتكاملات).
builder.Services.AddSingleton<ChatToDashboard.Api.Entities.PublishedDashboardStore>();
builder.Services.AddSingleton<PermissionsService>();
builder.Services.Configure<LdapOptions>(builder.Configuration.GetSection(LdapOptions.SectionName));
builder.Services.AddSingleton<LdapAuthenticator>();

builder.Services.AddSingleton<DataStore>();
builder.Services.AddSingleton<DocumentSearchService>();
builder.Services.AddSingleton<RepositoryStore>();
builder.Services.AddSingleton<UploadProgressTracker>();
builder.Services.AddSingleton<UploadParser>();
builder.Services.AddSingleton<ChatToDashboard.Api.Repository.LinkedFileFetcher>();
builder.Services.AddHttpClient("linked-files", c => { c.Timeout = TimeSpan.FromSeconds(60); c.MaxResponseContentBufferSize = ChatToDashboard.Api.Repository.LinkedFileFetcher.MaxBytes; });
builder.Services.Configure<SourceOptions>(builder.Configuration.GetSection(SourceOptions.SectionName));
builder.Services.Configure<PricingOptions>(builder.Configuration.GetSection(PricingOptions.SectionName));
builder.Services.AddSingleton<CostCalculator>();
builder.Services.AddSingleton<UsageStore>();
builder.Services.AddSingleton<UsageTracker>();
builder.Services.AddSingleton<SystemApiLoader>();
builder.Services.AddSingleton<AnalyticsTools>();
builder.Services.AddSingleton<HistoryStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.History.DashboardAccessService>();
builder.Services.AddSingleton<ShareStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.Presentations.PresentationStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.Presentations.PresentationLiveDataService>();
builder.Services.AddSingleton<ChatToDashboard.Api.Presentations.PresentationAssistantService>();
builder.Services.AddSingleton<ChatToDashboard.Api.Presentations.PresentationDataStore>();
// منشئ العروض (مستوى المشروع): مواصفة JSON + بناء بالموديل + نشر في العروض التقديمية — Decks/
builder.Services.AddSingleton<ChatToDashboard.Api.Decks.DeckStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.Decks.DeckBrandStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.Decks.BrandIdentityStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.Decks.DeckBrandFontStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.Decks.DeckBrandTemplateStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.Decks.DeckBuilder>();
builder.Services.AddSingleton<ChatToDashboard.Api.Decks.DeckImporter>();
builder.Services.AddSingleton<ChatToDashboard.Api.Freeform.FreeformStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.Freeform.FreeformRecordStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.Freeform.FreeformRecordsService>();
builder.Services.AddSingleton<ChatToDashboard.Api.Freeform.FreeformRecordTableService>();
builder.Services.AddSingleton<ChatToDashboard.Api.Freeform.FreeformBuilder>();
builder.Services.AddSingleton<ChatToDashboard.Api.Freeform.FreeformTemplateStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.Freeform.FreeformAssetStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.Freeform.FreeformDashboardTemplates>();
builder.Services.AddSingleton<ChatToDashboard.Api.Data.DataFingerprintService>();
// محادثة Claude عبر Claude Code CLI على الخادم (ClaudeChat/) — انظر ClaudeCliService.
builder.Services.AddSingleton<ChatToDashboard.Api.ClaudeChat.ClaudeCliService>();
builder.Services.AddSingleton<ChatToDashboard.Api.ClaudeChat.ClaudeChatStore>();
// خادم MCP داخلي لأدوات القراءة في محادثة Claude (ClaudeChat/Mcp): نقل Streamable HTTP بلا حالة على
// /api/claude-chat/mcp، يحرسه مفتاح جلسة مؤقت (McpTokenMiddleware) لا كوكي المستخدم.
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ChatToDashboard.Api.ClaudeChat.Mcp.McpSessionTokens>();
builder.Services.AddSingleton<ChatToDashboard.Api.ClaudeChat.Mcp.EntityCatalog>();
builder.Services.AddMcpServer(options =>
    {
        options.ServerInfo = new ModelContextProtocol.Protocol.Implementation { Name = "jeem", Version = "1.0.0" };
    })
    .WithHttpTransport(options => { options.Stateless = true; })
    .WithTools<ChatToDashboard.Api.ClaudeChat.Mcp.JeemMcpTools>();
builder.Services.AddSingleton<ChatToDashboard.Api.Widgets.WidgetQueryService>();
builder.Services.AddSingleton<ChatToDashboard.Api.Inquiry.ConversationStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.Inquiry.InquiryAccessService>();
builder.Services.AddSingleton<ChatToDashboard.Api.Templates.TemplateStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.SchemaLibrary.SchemaAliasStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.SchemaLibrary.SchemaMatchingService>();
builder.Services.AddSingleton<ChatToDashboard.Api.Templates.TemplatePromptService>();
// البناء المرحلي لقوالب اللوحات (ملف تعريف البيانات + تخطيط + نداء لكل عنصر + تجميع).
builder.Services.AddSingleton<ChatToDashboard.Api.Build.DataProfileService>();
builder.Services.AddSingleton(sp => (ChatToDashboard.Api.Llm.LlmRouter)sp.GetRequiredService<IDashboardGenerator>());
builder.Services.AddSingleton<ChatToDashboard.Api.Build.StagedTemplateBuilder>();
// طبقة المعنى: فهم أعمدة كل ملف مرفوع مرة واحدة وربطها بقاموس مفاهيم ثابت.
builder.Services.AddSingleton<ChatToDashboard.Api.Semantics.ColumnSemanticsStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.Semantics.ColumnSemanticsService>();
builder.Services.AddSingleton<ChatToDashboard.Api.Semantics.ConceptPriorStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.Semantics.WidgetFeasibilityService>();
builder.Services.AddSingleton<ChatToDashboard.Api.Analyst.AnalystStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.Graph.GraphStore>();
builder.Services.AddSingleton<LiveChannelHub>();

// Interactive (RenderKind) templates' real-data providers — see Interactive/
// InteractiveDataModels.cs. Stubs for the 5 pre-existing network/workflow/marketplace
// templates (their real per-project SQL mapping isn't part of this task's assigned scope,
// §4.16-§4.24 — see StubInteractiveDataProvider's own doc comment); §4.16+ templates get
// their own real IInteractiveDataProvider registered here as each is built.
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.IInteractiveDataProvider>(
    new ChatToDashboard.Api.Interactive.StubInteractiveDataProvider("policy-network",
        new[] { "اللوائح والأنظمة", "الجهات والمنصات", "الشكاوى" }));
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.IInteractiveDataProvider>(
    new ChatToDashboard.Api.Interactive.StubInteractiveDataProvider("procurement-network",
        new[] { "طلبات الشراء", "الموردون", "أوامر الشراء" }));
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.IInteractiveDataProvider>(
    new ChatToDashboard.Api.Interactive.StubInteractiveDataProvider("client-supplier-network",
        new[] { "العملاء", "الموردون", "المشاريع المشتركة" }));
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.IInteractiveDataProvider>(
    new ChatToDashboard.Api.Interactive.StubInteractiveDataProvider("procurement-workflow",
        new[] { "أحداث مراحل الطلبات", "مدة الخدمة لكل مرحلة" }));
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.IInteractiveDataProvider>(
    new ChatToDashboard.Api.Interactive.StubInteractiveDataProvider("procurement-marketplace",
        new[] { "طلبات المنصة", "العملاء", "الموردون" }));
// Real SQL matching (§4.16-4.21 Phase-1 templates) — see InteractiveContractMatcher.cs
// (LLM-assisted logical->real table/column matching, re-verified against the project's own
// catalog before trusting it). Converted one at a time, starting with the simplest data
// shapes; the remaining stubs above get the same treatment next.
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.InteractiveContractMatcher>();
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.IInteractiveDataProvider, ChatToDashboard.Api.Interactive.CrmPipelineDataProvider>();
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.IInteractiveDataProvider, ChatToDashboard.Api.Interactive.WarehouseTeamDataProvider>();
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.IInteractiveDataProvider, ChatToDashboard.Api.Interactive.TeamTasksDataProvider>();
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.IInteractiveDataProvider, ChatToDashboard.Api.Interactive.TimeTrackingDataProvider>();
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.IInteractiveDataProvider, ChatToDashboard.Api.Interactive.LiquidityPoolsDataProvider>();
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.IInteractiveDataProvider, ChatToDashboard.Api.Interactive.TokenTreasuryDataProvider>();
// §4.22 (11-dashboard batch, dash-kit engines) — same matcher, one provider per dashboard.
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.IInteractiveDataProvider, ChatToDashboard.Api.Interactive.SaasMetricsDataProvider>();
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.IInteractiveDataProvider, ChatToDashboard.Api.Interactive.ProjectStudioDataProvider>();
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.IInteractiveDataProvider, ChatToDashboard.Api.Interactive.CampaignProjectDataProvider>();
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.IInteractiveDataProvider, ChatToDashboard.Api.Interactive.TreasuryFundingDataProvider>();
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.IInteractiveDataProvider, ChatToDashboard.Api.Interactive.ConstructionCrmDataProvider>();
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.IInteractiveDataProvider, ChatToDashboard.Api.Interactive.TaskBoardDataProvider>();
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.IInteractiveDataProvider, ChatToDashboard.Api.Interactive.CrmLeadsDataProvider>();
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.IInteractiveDataProvider, ChatToDashboard.Api.Interactive.SupplierRiskDataProvider>();
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.IInteractiveDataProvider, ChatToDashboard.Api.Interactive.CrmAnalysisDataProvider>();
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.IInteractiveDataProvider, ChatToDashboard.Api.Interactive.SupplyCommandDataProvider>();
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.IInteractiveDataProvider, ChatToDashboard.Api.Interactive.FinanceOpsDataProvider>();
builder.Services.AddSingleton<ChatToDashboard.Api.Interactive.InteractiveDataRegistry>();

// Named clients for the back-office endpoints. The "insecure" one exists only for an
// internal server with a self-signed certificate, and is opt-in per system.
builder.Services.AddHttpClient(SystemApiClients.Default);
builder.Services.AddHttpClient(SystemApiClients.Insecure)
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
    });

// Which LLM answers the questions — "Anthropic" (default), "OpenAI", or "Ollama" (an
// internal deployment reached through a company API gateway). All three clients are always
// registered, each with its own typed HttpClient; only the one actually selected (via
// LlmSettingsStore, overridable from the dashboard at runtime, falling back to this config
// value) is ever resolved and called — see LlmRouter. That also means a provider whose API
// key isn't configured only breaks if it's the one currently selected, not at startup.
var llmProvider = builder.Configuration["Llm:Provider"] ?? "Anthropic";
builder.Services.AddSingleton<ChatToDashboard.Api.Llm.LlmSettingsStore>();
// مفاتيح المزوّدين من صفحة «مفاتيح الموديلات»: تُحفظ مشفّرة بـ Data Protection؛ مجلد المفاتيح بجوار
// التطبيق (لا في ملف المستخدم) حتى تبقى قابلة للقراءة بعد إعادة التشغيل أو التشغيل كخدمة.
builder.Services.AddDataProtection()
    .SetApplicationName("ChatToDashboard")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "dp-keys")));
builder.Services.AddSingleton<ChatToDashboard.Api.Llm.LlmCredentialStore>();
builder.Services.AddHttpClient<ClaudeClient>(client =>
{
    client.BaseAddress = new Uri("https://api.anthropic.com/");
    client.Timeout = TimeSpan.FromMinutes(5);
});
builder.Services.AddHttpClient<OpenAiClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com/");
    client.Timeout = TimeSpan.FromMinutes(5);
});
builder.Services.AddHttpClient<ChatToDashboard.Api.Ollama.OllamaClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Ollama:BaseUrl"] ?? "http://172.17.242.1:8081/api/v1/");
    // Local/self-hosted inference can be far slower than a cloud API — especially on
    // CPU-only hardware, a large model, or a long system prompt — so the 5-minute default
    // that's fine for Claude/OpenAI can genuinely be too tight here ("HttpClient.Timeout of
    // 300 seconds elapsing" mid-request). Configurable per deployment since hardware varies;
    // set Ollama:TimeoutSeconds higher still if a single reply routinely needs more than this.
    var timeoutSeconds = builder.Configuration.GetValue("Ollama:TimeoutSeconds", 600);
    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
});
builder.Services.AddSingleton<IDashboardGenerator, ChatToDashboard.Api.Llm.LlmRouter>();
// Same LlmRouter singleton, exposed under its second interface too (RepositoryStore's table-
// naming suggestion at upload time — see ITableNamingAssistant) — resolved via the
// IDashboardGenerator registration above rather than a second AddSingleton<LlmRouter> so both
// interfaces share the exact same instance instead of quietly constructing two.
builder.Services.AddSingleton<ChatToDashboard.Api.Llm.ITableNamingAssistant>(
    sp => (ChatToDashboard.Api.Llm.LlmRouter)sp.GetRequiredService<IDashboardGenerator>());
// Same LlmRouter singleton again, for External Integrations' visual-identity/identity-
// transport text suggestions (see IIntegrationSetupAssistant's remarks on why this shares
// LlmRouter's normal provider resolution instead of its own independent setting).
builder.Services.AddSingleton<ChatToDashboard.Api.Llm.IIntegrationSetupAssistant>(
    sp => (ChatToDashboard.Api.Llm.LlmRouter)sp.GetRequiredService<IDashboardGenerator>());
builder.Services.AddSingleton<ChatToDashboard.Api.Llm.IDocumentReaderRouter, ChatToDashboard.Api.Llm.DocumentReaderRouter>();
// Registered as its own concrete type (not just its interface) because VisualIdentityService
// also needs DescribeAsync — its own method, not part of IVisualIdentityImageExtractor — to
// tell "nothing configured" apart from "configured but not image-capable" for its refusal
// message; same one-singleton-two-registrations shape as LlmRouter/IDashboardGenerator above.
builder.Services.AddSingleton<ChatToDashboard.Api.Llm.VisualIdentityImageRouter>();
builder.Services.AddSingleton<ChatToDashboard.Api.Llm.IVisualIdentityImageExtractor>(
    sp => sp.GetRequiredService<ChatToDashboard.Api.Llm.VisualIdentityImageRouter>());
builder.Services.AddSingleton<ChatToDashboard.Api.Integrations.VisualIdentityService>();
builder.Services.AddSingleton<ChatToDashboard.Api.Integrations.IntegrationStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.Integrations.ClientSchemaDiscoveryService>();
builder.Services.AddSingleton<ChatToDashboard.Api.Integrations.ClientQueryService>();
builder.Services.AddSingleton<ChatToDashboard.Api.Integrations.PublishService>();
builder.Services.AddSingleton<ChatToDashboard.Api.Integrations.IntegrationDeliverables>();

var app = builder.Build();

// نماذج تصميم حر الجاهزة المسلَّمة مجلداتٍ تحت wwwroot/templates/freeform/<key>/ (template.json + ar.html + en.html + assets/):
// تُقرأ مرة وتُسجَّل في الكتالوج بلا تعديل كود؛ المجلد المخالف يُتخطّى بسببه.
{
    var folderTemplates = ChatToDashboard.Api.Freeform.FreeformFolderTemplates.Load(app.Environment.WebRootPath);
    ChatToDashboard.Api.Freeform.FreeformTemplates.RegisterFolderTemplates(folderTemplates.Templates);
    app.Logger.LogInformation("Freeform folder templates: {Count} loaded, {Rejected} rejected", folderTemplates.Templates.Count, folderTemplates.Rejected.Count);
    foreach (var (folder, reason) in folderTemplates.Rejected)
        app.Logger.LogWarning("Freeform folder template '{Folder}' skipped: {Reason}", folder, reason);
}

// This is a JSON API consumed by the SPA's own fetch calls (see the 401/403 auth events
// above) — every response from it needs to stay JSON, including an unhandled exception.
// Without this, an exception thrown outside a controller action's own try/catch (or from
// an action that simply doesn't have one — e.g. GET /api/repository/files) falls through to
// ASP.NET Core's default handling, which in Development serves an HTML/plain-text
// diagnostics page instead: the frontend's response.json() then throws "Unexpected token
// 'M', 'Microsoft....' is not valid JSON" instead of showing the actual error. Placed first
// so it wraps every middleware and endpoint below it.
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    context.RequestServices.GetRequiredService<ILogger<Program>>()
        .LogError(error, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(new { error = error?.Message ?? "حدث خطأ غير متوقع." });
}));

// The UI lives in wwwroot and is served from this same app — one project, one URL.
// Static files (including index.html, which renders its own login screen) are served
// before authentication runs, so the app shell always loads; every API call underneath
// it still requires a session via the FallbackPolicy above.
// لا UseDefaultFiles: «/» يُحسم في نقطة نهاية خاصة أدناه (الصفحة الرئيسية للزائر، والتطبيق للمسجَّل أو لرابط مشاركة).
// الملفات الثابتة تُعاد مصادقتها عند كل تحميل (ETag → 304 رخيصة) حتى لا يبقى المتصفح على نسخة قديمة من app.css/app.js
// بعد نشر جديد — ظهر ذلك كصفحة مشاركة بقائمة جانبية وشريط قديم لأن app.css المخزَّن لم يحمل قواعد is-shared.
app.UseStaticFiles(new StaticFileOptions { OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "no-cache" });
app.UseWebSockets();

app.UseRouting();
app.UseMcpSessionGate();
app.UseAuthentication();
// صلاحيات الشاشات (ScreenAccess): مستخدم عادي مقيَّد بشاشات لا يصل إلى واجهات لا تحتاجها أيٌّ من شاشاته — 403 برسالة.
// يُقرأ المستخدم من القاعدة فقط للمسارات المقيَّدة، فلا كلفة على غيرها.
app.Use(async (ctx, next) =>
{
    var path = ctx.Request.Path.Value ?? "";
    var isPublic = ctx.GetEndpoint()?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() is not null;
    if (ctx.User.Identity?.IsAuthenticated == true && ChatToDashboard.Api.Users.ScreenAccess.AppliesTo(path, isPublic)
        && ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value is { } uid)
    {
        var u = await ctx.RequestServices.GetRequiredService<ChatToDashboard.Api.Users.UserStore>().FindByIdAsync(uid, ctx.RequestAborted);
        if (u is not null && u.Role != ChatToDashboard.Api.Users.UserRoles.Admin
            && !ChatToDashboard.Api.Users.ScreenAccess.IsApiAllowed(path, ChatToDashboard.Api.Users.ScreenAccess.Parse(u.AllowedScreensJson)))
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            await ctx.Response.WriteAsJsonAsync(new { error = "ليست لديك صلاحية على هذه الشاشة — راجع مسؤول المنظمة." });
            return;
        }
    }
    await next();
});
app.UseAuthorization();

app.MapControllers();
// خادم MCP لمحادثة Claude — بلا سياسة الكوكي (الحارس أعلاه يتحقق من مفتاح الجلسة).
app.MapMcp(ChatToDashboard.Api.ClaudeChat.Mcp.McpTokenMiddleware.Path).AllowAnonymous();

// INTERACTIVE-HANDOFF.md §7's "hub /hubs/live" — see LiveChannelHub's own doc comment for
// why this is a plain WebSocket endpoint instead of a SignalR hub. Channel convention:
// "{entityId}:{key}" (the project id is always the first segment) — any authenticated
// project member (or platform Admin) may subscribe; nobody else's channel is reachable
// even by guessing a entityId, since the role check below runs before the socket upgrade
// is accepted. FallbackPolicy (RequireAuthenticatedUser, configured above) already covers
// "must be signed in" for this endpoint like every other one; the project-membership check
// here is the additional, endpoint-specific layer.
app.Map("/ws/live/{channel}", async (
    HttpContext ctx, string channel, LiveChannelHub hub,
    ChatToDashboard.Api.Entities.EntityStore projects) =>
{
    if (!ctx.WebSockets.IsWebSocketRequest) { ctx.Response.StatusCode = StatusCodes.Status400BadRequest; return; }
    var entityId = channel.Split(':', 2)[0];
    if (string.IsNullOrEmpty(entityId)) { ctx.Response.StatusCode = StatusCodes.Status400BadRequest; return; }
    var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId is null) { ctx.Response.StatusCode = StatusCodes.Status401Unauthorized; return; }
    if (!ctx.User.IsInRole(UserRoles.Admin))
    {
        var roles = await projects.ListRolesAsync(entityId, ctx.RequestAborted);
        if (!roles.Any(r => r.UserId == userId)) { ctx.Response.StatusCode = StatusCodes.Status403Forbidden; return; }
    }
    var socket = await ctx.WebSockets.AcceptWebSocketAsync();
    await hub.RunAsync(channel, socket, ctx.RequestAborted);
});

// The observability page lives on its own link, unrelated to the dashboard — admin only.
app.MapGet("/usage", (IWebHostEnvironment env) =>
    Results.File(Path.Combine(env.WebRootPath, "usage.html"), "text/html"))
    .RequireAuthorization(policy => policy.RequireRole(UserRoles.Admin));

// صفحة مشاهدة عرض تقديمي عام (انظر PresentationsController): بلا تسجيل دخول، وتضع المحتوى في iframe معزول.
app.MapGet("/p/{id}", (IWebHostEnvironment env) =>
    Results.File(Path.Combine(env.WebRootPath, "presentation.html"), "text/html"))
    .AllowAnonymous();
// دليل «بيانات العرض» لمؤلفي الصفحات (نص عادي من docs/ في المستودع، يُنسخ مع النشر إن وُجد).
app.MapGet("/docs/presentations-data.md", (IWebHostEnvironment env) =>
{
    var candidates = new[] { Path.Combine(env.ContentRootPath, "..", "..", "docs", "presentations-data.md"), Path.Combine(env.WebRootPath, "docs", "presentations-data.md") };
    var file = candidates.FirstOrDefault(File.Exists);
    return file is null ? Results.NotFound() : Results.File(Path.GetFullPath(file), "text/markdown; charset=utf-8");
}).AllowAnonymous();
// الرابط المقروء: /p/{اسم-المنظمة}/{اسم-العرض} (فريد داخل المنظمة) — نفس صفحة المشاهدة.
app.MapGet("/p/{org}/{slug}", (IWebHostEnvironment env) =>
    Results.File(Path.Combine(env.WebRootPath, "presentation.html"), "text/html"))
    .AllowAnonymous();
// الرابط المقروء للوحة متابعة مشارَكة: /d/{اسم-المنظمة}/{اسم-اللوحة} — التطبيق نفسه في وضع المشاركة (bootSharedView).
// الصفحة تحت مسار بمقطعين فتُحوَّل مراجع index.html النسبية (css/ js/ img/) إلى مطلقة وإلا حلّها المتصفح تحت /d/{org}/.
app.MapGet("/d/{org}/{slug}", (HttpContext ctx, IWebHostEnvironment env) => SpaIndex.Result(ctx, env))
    .AllowAnonymous();

// الصفحة الرئيسية (wwwroot/home.html): «/» للزائر غير المسجَّل؛ والمسجَّل دخوله يذهب إلى التطبيق مباشرة. أي «/» بسلسلة استعلام
// (روابط المشاركة القديمة ?share= و?embed=1&pub=…) يبقى للتطبيق كما كان. «/home» يعرض الصفحة الرئيسية دائمًا، و«/app» التطبيق دائمًا
// (بشاشة الدخول لمن لم يسجّل) — زر «تسجيل الدخول» في الصفحة الرئيسية يوجّه إليه.
app.MapGet("/", (HttpContext ctx, IWebHostEnvironment env) =>
    ctx.Request.QueryString.HasValue || ctx.User.Identity?.IsAuthenticated == true ? SpaIndex.Result(ctx, env) : HomePage(ctx, env))
    .AllowAnonymous();
app.MapGet("/home", (HttpContext ctx, IWebHostEnvironment env) => HomePage(ctx, env)).AllowAnonymous();
app.MapGet("/app", (HttpContext ctx, IWebHostEnvironment env) => SpaIndex.Result(ctx, env)).AllowAnonymous();
static IResult HomePage(HttpContext ctx, IWebHostEnvironment env)
{
    ctx.Response.Headers.CacheControl = "no-cache";
    return Results.File(Path.Combine(env.WebRootPath, "home.html"), "text/html; charset=utf-8");
}

// /o/{slug} (and anything else with no matching static file or API route) still serves the
// same single-page app — app.js reads the slug from location.pathname itself (see
// resolveOrgScopedLoginAsync) to show the right organization's login screen. Only ever
// reached for a GET that matched nothing above, so it never shadows a real API route.
// AllowAnonymous is required here — every endpoint requires a signed-in user by default
// (FallbackPolicy above), which would otherwise 401 this before the signed-out visitor it's
// actually for ever sees the login screen.
app.MapFallback((HttpContext ctx, IWebHostEnvironment env) => SpaIndex.Result(ctx, env)).AllowAnonymous();

// Initial load: fetch the enabled external systems into the shared schema. Failures are logged
// but do not prevent startup — POST /api/data/refresh can retry once SQL is reachable.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    // Logged so a stale or unsaved appsettings.json is obvious at a glance. The effective
    // provider can differ from this default if an admin overrode it from the dashboard's
    // model selector (LlmSettingsStore) — that only takes effect per-question, not here.
    var dataStore = scope.ServiceProvider.GetRequiredService<DataStore>();
    logger.LogInformation("Configuration in use — LLM provider (default): {Llm}, database: {Db}",
        llmProvider, "SqlServer");

    // Must run before anything below touches the database: on a fresh SQL Server the
    // target database doesn't exist yet, so without this the very first operation (seeding
    // the admin account, just below) fails with "Cannot open database" and no account ever
    // gets created — a silent lockout, since there's no self-signup to fall back on.
    await dataStore.EnsureDatabaseExistsAsync(logger);

    // قاعدة نُشرت قبل 2026-11-02 ما زالت جداولها تحت [staging]؛ تُنقل إلى [dbo] هنا قبل أن ينشئ أي مخزن جداوله
    // (انظر DataStore.MigrateLegacyStagingSchemaAsync). فشله يُسجَّل ولا يوقف الإقلاع.
    try { await dataStore.MigrateLegacyStagingSchemaAsync(logger); }
    catch (Exception ex) { logger.LogError(ex, "Legacy [staging] → [dbo] schema migration failed."); }
    // «المشروع» صار «الكيان» في القاعدة: الجداول Projects/ProjectRoles والأعمدة ProjectId/ProjectKey (DataStore.MigrateProjectToEntityAsync)
    // فشله يوقف الإقلاع عمدًا: لو أكمل التطبيق لأنشأت المخازن جداول/أعمدة جديدة فارغة (Entities/EntityId) بجوار القديمة فتبدو البيانات مفقودة.
    // الترحيل داخل معاملة فلا يترك القاعدة نصف مُرحَّلة؛ أصلح السبب في السجل ثم أعد التشغيل.
    try { await dataStore.MigrateProjectToEntityAsync(logger); }
    catch (Exception ex) { logger.LogCritical(ex, "Project→Entity rename migration failed — startup stopped to avoid creating empty Entity tables next to the old ones."); throw; }
    // مخططات بيانات الكيانات proj_<id> ← ent_<id> مع إعادة كتابة أسماء الجداول المحفوظة نصًّا (DataStore.MigrateEntitySchemasAsync).
    // فشله يوقف الإقلاع للسبب نفسه: بدونه يبحث التطبيق عن الملفات في ent_ فتبدو مفقودة وتُنشأ رفعات جديدة في مخطط آخر.
    try { await dataStore.MigrateEntitySchemasAsync(logger); }
    catch (Exception ex) { logger.LogCritical(ex, "Entity schema migration (proj_ → ent_) failed — startup stopped; nothing was changed (single transaction)."); throw; }

    // SaaS hierarchy (Platform → Organization → Entity — see Organizations/Projects): every
    // install needs at least a default organization and project for its existing/first users
    // and data to belong to. Created once, here, before the admin-seeding step below so a
    // brand-new install's seed admin can be attached to it directly.
    ChatToDashboard.Api.Organizations.Organization? defaultOrg = null;
    try
    {
        var orgStore = scope.ServiceProvider.GetRequiredService<ChatToDashboard.Api.Organizations.OrganizationStore>();
        var entityStore = scope.ServiceProvider.GetRequiredService<ChatToDashboard.Api.Entities.EntityStore>();
        defaultOrg = await orgStore.FirstAsync();
        if (defaultOrg is null)
        {
            defaultOrg = await orgStore.CreateAsync("المنظمة الافتراضية");
            await entityStore.CreateAsync(defaultOrg.Id, "المشروع الرئيسي");
            logger.LogInformation("Created the default organization + project.");
        }

        // Attach any account that predates this feature (including a fresh seed-admin
        // scenario can't hit this — CountAsync below is still 0 then) to the default org.
        var userStoreForBackfill = scope.ServiceProvider.GetRequiredService<UserStore>();
        await userStoreForBackfill.BackfillMissingOrganizationAsync(defaultOrg.Id);

        // Files and external integrations predate project-level isolation entirely — every
        // one of them, on any pre-existing install, implicitly belonged to "the" single
        // workspace that existed before organizations/projects did, which is exactly the
        // default organization's own default project. Without this, a brand-new organization
        // would see every pre-existing file/integration too (EntityId NULL never matches a
        // real project id) — the leak this whole EntityId column exists to close.
        var defaultEntity = await entityStore.FirstForOrganizationAsync(defaultOrg.Id);
        if (defaultEntity is not null)
        {
            var repositoryStoreForBackfill = scope.ServiceProvider.GetRequiredService<ChatToDashboard.Api.Repository.RepositoryStore>();
            await repositoryStoreForBackfill.BackfillMissingEntityAsync(defaultEntity.Id);
            var integrationStoreForBackfill = scope.ServiceProvider.GetRequiredService<ChatToDashboard.Api.Integrations.IntegrationStore>();
            await integrationStoreForBackfill.BackfillMissingEntityAsync(defaultEntity.Id);

            // Now that every repository file has a real EntityId (just backfilled above, or
            // already had one), move each one's uploaded-data table out of the shared SQL
            // Server schema into its own project's schema — see RepositoryStore.MigrateExisting-
            // TablesToEntitySchemasAsync's own doc comment. No-op on SQLite, and idempotent
            // (re-checked by OBJECT_ID, not a flag) so it's cheap to run on every startup.
            await repositoryStoreForBackfill.MigrateExistingTablesToEntitySchemasAsync();
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to ensure the default organization/project exist.");
    }

    // Field/table alias library (see ChatToDashboard.Api.SchemaLibrary) — seeded once, the
    // first time this table is ever empty, by mining kpi-library.js's own draft "expected
    // SELECT" for the (table, value-column, group-column) concepts it already implies. A no-op
    // on every later startup (SeedIfEmptyAsync checks first) — never re-runs against a library
    // an admin has already started curating, and never blocks startup if it fails.
    try
    {
        var aliasStore = scope.ServiceProvider.GetRequiredService<ChatToDashboard.Api.SchemaLibrary.SchemaAliasStore>();
        if (!await aliasStore.HasAnyAsync())
        {
            var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            var kpiLibraryPath = Path.Combine(env.WebRootPath, "js", "kpi-library.js");
            if (File.Exists(kpiLibraryPath))
            {
                var content = await File.ReadAllTextAsync(kpiLibraryPath);
                var seedRows = ChatToDashboard.Api.SchemaLibrary.SchemaAliasSeedGenerator.BuildFromKpiLibraryJs(content);
                await aliasStore.SeedIfEmptyAsync(seedRows);
                logger.LogInformation("Seeded the field/table alias library: {Count} candidate names.", seedRows.Count);
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to seed the field/table alias library.");
    }

    // Accounts are admin-provisioned only (no self-signup) — so the very first admin has
    // to come from somewhere. If no account exists yet at all, create one: from
    // Auth:SeedAdmin:Username/Password if set (user-secrets, same as every other
    // credential here), otherwise a random password logged once so the app is usable
    // out of the box.
    try
    {
        var userStore = scope.ServiceProvider.GetRequiredService<UserStore>();
        if (await userStore.CountAsync() == 0)
        {
            var seedUsername = builder.Configuration["Auth:SeedAdmin:Username"] ?? "admin";
            var seedPassword = builder.Configuration["Auth:SeedAdmin:Password"];
            var generated = string.IsNullOrWhiteSpace(seedPassword);
            if (generated) seedPassword = RandomNumberGenerator.GetHexString(12);

            await userStore.CreateAsync(new AppUser
            {
                Username = seedUsername,
                DisplayName = "مدير النظام",
                Role = UserRoles.Admin,
                AuthMethod = AuthMethods.Local,
                PasswordHash = PasswordHasher.Hash(seedPassword!),
                IsActive = true,
                AllowAllSystems = true,
                AllowAllFiles = true,
                // The very first account is, by definition, us — the platform owner — until
                // real customer organizations exist (see AppUser.IsPlatformOwner).
                OrganizationId = defaultOrg?.Id,
                IsPlatformOwner = true,
            });

            if (generated)
                logger.LogWarning(
                    "No accounts existed — created the initial admin account. " +
                    "Username: {Username} | Password: {Password} — sign in and create real accounts, " +
                    "or set Auth:SeedAdmin:Username/Password via user-secrets before first run to skip this.",
                    seedUsername, seedPassword);
            else
                logger.LogInformation(
                    "No accounts existed — created the initial admin account {Username} from Auth:SeedAdmin.",
                    seedUsername);
        }
        // إعادة ضبط كلمة مرور مالك المنصّة من الإعدادات عند الإقلاع (Auth:SeedAdmin:ResetPassword = true مع
        // Auth:SeedAdmin:Password): الطريق الوحيد لاستعادة الدخول على نشر قائم (Azure) حين تُنسى كلمة المرور،
        // لأن الزرع أعلاه لا يعمل إلا على قاعدة بلا حسابات. يُطبَّق على حساب مالك المنصّة الحامل لاسم الزرع
        // فقط (ويُنشأ إن لم يوجد)، ويُسجَّل تحذير بإزالة العلم بعدها حتى لا تُعاد الكتابة مع كل إقلاع.
        else if (builder.Configuration.GetValue<bool>("Auth:SeedAdmin:ResetPassword"))
        {
            var seedUsername = builder.Configuration["Auth:SeedAdmin:Username"] ?? "admin";
            var seedPassword = builder.Configuration["Auth:SeedAdmin:Password"];
            if (string.IsNullOrWhiteSpace(seedPassword))
            {
                logger.LogWarning(
                    "Auth:SeedAdmin:ResetPassword is set but Auth:SeedAdmin:Password is empty — nothing was reset.");
            }
            else
            {
                var owners = (await userStore.ListAsync())
                    .Where(u => u.IsPlatformOwner && u.AuthMethod == AuthMethods.Local &&
                                string.Equals(u.Username, seedUsername, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (owners.Count == 0)
                {
                    // لا حساب بهذا الاسم: يُنشأ حساب مالك منصّة جديد (كالزرع الأول) — هكذا يُستعاد الدخول باسم جديد
                    // حين يتعذّر الوصول إلى القاعدة لإصلاح الحساب القديم.
                    await userStore.CreateAsync(new AppUser
                    {
                        Username = seedUsername,
                        DisplayName = "مدير النظام",
                        Role = UserRoles.Admin,
                        AuthMethod = AuthMethods.Local,
                        PasswordHash = PasswordHasher.Hash(seedPassword),
                        IsActive = true,
                        AllowAllSystems = true,
                        AllowAllFiles = true,
                        OrganizationId = defaultOrg?.Id,
                        IsPlatformOwner = true,
                    });
                    logger.LogWarning(
                        "Auth:SeedAdmin:ResetPassword: no platform-owner account named {Username} existed — created one from configuration. " +
                        "Remove Auth:SeedAdmin:ResetPassword now.",
                        seedUsername);
                }
                else if (owners.Count > 1)
                {
                    logger.LogWarning(
                        "Auth:SeedAdmin:ResetPassword: found {Count} platform-owner accounts named {Username} — nothing was reset.",
                        owners.Count, seedUsername);
                }
                else
                {
                    var owner = owners[0];
                    owner.PasswordHash = PasswordHasher.Hash(seedPassword);
                    owner.IsActive = true;
                    await userStore.UpdateAsync(owner);
                    logger.LogWarning(
                        "Auth:SeedAdmin:ResetPassword: password of platform-owner account {Username} was reset from configuration. " +
                        "Remove Auth:SeedAdmin:ResetPassword now so it is not reapplied on every startup.",
                        seedUsername);
                }
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to ensure an initial admin account exists.");
    }

    // Demo accounts showing the two non-platform-owner permission shapes this app supports:
    // an org Admin (Role=Admin — sees/manages everything in their own organization, same as
    // any org admin) and a plain "User" account scoped to exactly one project via
    // EntityRoles (see EntitiesController.List / HistoryController.CurrentEntityIdAsync —
    // a project, unlike a dashboard, has no single natural owner, so this is a named role
    // row rather than an OwnerId column). Looked up by name/username first so a restart
    // never recreates or duplicates them; the role assignment is re-applied every time
    // regardless, so it self-heals if ever removed by hand.
    // معطَّل افتراضيًا (Auth:SeedDemoOrganization = false): المنصة الحقيقية تبدأ بمالك المنصّة فقط،
    // وهو من ينشئ المنظمات؛ فعّله true في بيئة عرض تجريبي فقط.
    if (builder.Configuration.GetValue<bool>("Auth:SeedDemoOrganization"))
    try
    {
        var orgStore = scope.ServiceProvider.GetRequiredService<ChatToDashboard.Api.Organizations.OrganizationStore>();
        var entityStore = scope.ServiceProvider.GetRequiredService<ChatToDashboard.Api.Entities.EntityStore>();
        var userStore = scope.ServiceProvider.GetRequiredService<UserStore>();

        var demoOrg = (await orgStore.ListAsync()).FirstOrDefault(o => o.Name == "شركة النموذج التجريبي")
            ?? await orgStore.CreateAsync("شركة النموذج التجريبي");
        var demoEntity = (await entityStore.ListByOrganizationAsync(demoOrg.Id)).FirstOrDefault(p => p.Name == "المشروع الأول")
            ?? await entityStore.CreateAsync(demoOrg.Id, "المشروع الأول");

        if (await userStore.FindByUsernameAsync("org.admin") is null)
        {
            var pw = RandomNumberGenerator.GetHexString(12);
            await userStore.CreateAsync(new AppUser
            {
                Username = "org.admin",
                DisplayName = $"مسؤول {demoOrg.Name}",
                Role = UserRoles.Admin,
                AuthMethod = AuthMethods.Local,
                PasswordHash = PasswordHasher.Hash(pw),
                IsActive = true,
                AllowAllSystems = true,
                AllowAllFiles = true,
                OrganizationId = demoOrg.Id,
                IsPlatformOwner = false,
            });
            logger.LogWarning(
                "Seeded an org-admin demo account for {Org}. Username: {Username} | Password: {Password}",
                demoOrg.Name, "org.admin", pw);
        }

        var entityOwnerUser = await userStore.FindByUsernameAsync("project.owner");
        if (entityOwnerUser is null)
        {
            var pw = RandomNumberGenerator.GetHexString(12);
            entityOwnerUser = await userStore.CreateAsync(new AppUser
            {
                Username = "project.owner",
                DisplayName = $"مسؤول {demoEntity.Name}",
                Role = UserRoles.User,
                AuthMethod = AuthMethods.Local,
                PasswordHash = PasswordHasher.Hash(pw),
                IsActive = true,
                AllowAllSystems = true,
                AllowAllFiles = true,
                OrganizationId = demoOrg.Id,
                IsPlatformOwner = false,
            });
            logger.LogWarning(
                "Seeded a project-owner demo account for {Entity}. Username: {Username} | Password: {Password}",
                demoEntity.Name, "project.owner", pw);
        }
        await entityStore.SetRoleAsync(demoEntity.Id, entityOwnerUser.Id, ChatToDashboard.Api.Entities.EntityRoles.Owner);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to seed the demo organization/project/users.");
    }

    try
    {
        var systems = await scope.ServiceProvider.GetRequiredService<SystemApiLoader>().LoadAllAsync();
        foreach (var system in systems.Where(s => s.Error is not null))
            logger.LogWarning("System {System} could not be loaded: {Error}", system.System, system.Error);
    }
    catch (Exception ex)
    {
        logger.LogError(ex,
            "Startup data load failed. Check ConnectionStrings:DataDb (user-secrets), " +
            "then call POST /api/data/refresh to retry without restarting.");
    }
}

app.Run();

/// <summary>index.html بمراجع أصول مطلقة (css/ js/ img/ → /css/ /js/ /img/) لخدمته تحت مسارات عميقة مثل /d/{org}/{slug} و/o/{slug}.</summary>
internal static class SpaIndex
{
    private static string? _cached; private static DateTime _stamp;
    public static string Html(IWebHostEnvironment env)
    {
        var path = Path.Combine(env.WebRootPath, "index.html");
        var stamp = File.GetLastWriteTimeUtc(path);
        if (_cached is null || stamp != _stamp) { _cached = Rewrite(File.ReadAllText(path)); _stamp = stamp; }
        return _cached;
    }
    /// <summary>
    /// الصفحة نفسها بترويسة «أعد التحقق دائمًا» كالملفات الثابتة؛ بدونها قد يعيد المتصفح نسخة قديمة من index.html
    /// تحت /o/{slug} مع app.js جديد، فتنقص عناصر يطلبها الكود ويتوقف (مثلًا معاينة تصميم حر تبقى بيضاء).
    /// </summary>
    public static IResult Result(HttpContext ctx, IWebHostEnvironment env)
    {
        ctx.Response.Headers.CacheControl = "no-cache";
        return Results.Content(Html(env), "text/html; charset=utf-8", System.Text.Encoding.UTF8);
    }
    public static string Rewrite(string html) =>
        System.Text.RegularExpressions.Regex.Replace(html, @"(?<=\s(?:href|src)=\x22)(css|js|img|lib|vendor)/", "/$1/");
}
