using System.Security.Cryptography;
using ChatToDashboard.Api.Auth;
using ChatToDashboard.Api.Claude;
using ChatToDashboard.Api.Data;
using ChatToDashboard.Api.History;
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
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
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
builder.Services.AddSingleton<ChatToDashboard.Api.Projects.ProjectStore>();
builder.Services.AddSingleton<PermissionsService>();
builder.Services.Configure<LdapOptions>(builder.Configuration.GetSection(LdapOptions.SectionName));
builder.Services.AddSingleton<LdapAuthenticator>();

builder.Services.AddSingleton<DataStore>();
builder.Services.AddSingleton<DataFolderLoader>();
builder.Services.AddSingleton<DocumentSearchService>();
builder.Services.AddSingleton<RepositoryStore>();
builder.Services.AddSingleton<UploadProgressTracker>();
builder.Services.AddSingleton<UploadParser>();
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
builder.Services.AddSingleton<ChatToDashboard.Api.Widgets.WidgetQueryService>();
builder.Services.AddSingleton<ChatToDashboard.Api.Inquiry.ConversationStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.Inquiry.InquiryAccessService>();

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
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// The observability page lives on its own link, unrelated to the dashboard — admin only.
app.MapGet("/usage", (IWebHostEnvironment env) =>
    Results.File(Path.Combine(env.WebRootPath, "usage.html"), "text/html"))
    .RequireAuthorization(policy => policy.RequireRole(UserRoles.Admin));

// /o/{slug} (and anything else with no matching static file or API route) still serves the
// same single-page app — app.js reads the slug from location.pathname itself (see
// resolveOrgScopedLoginAsync) to show the right organization's login screen. Only ever
// reached for a GET that matched nothing above, so it never shadows a real API route.
// AllowAnonymous is required here — every endpoint requires a signed-in user by default
// (FallbackPolicy above), which would otherwise 401 this before the signed-out visitor it's
// actually for ever sees the login screen.
app.MapFallbackToFile("index.html").AllowAnonymous();

// Initial load: scan the data folder and (re)create the staging tables so the
// shared SQL Server copy reflects the current files. Failures are logged but do
// not prevent startup — POST /api/data/refresh can retry once SQL is reachable.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    // Logged so a stale or unsaved appsettings.json is obvious at a glance. The effective
    // provider can differ from this default if an admin overrode it from the dashboard's
    // model selector (LlmSettingsStore) — that only takes effect per-question, not here.
    var dataStore = scope.ServiceProvider.GetRequiredService<DataStore>();
    logger.LogInformation("Configuration in use — LLM provider (default): {Llm}, database: {Db}",
        llmProvider, dataStore.Provider);

    // Must run before anything below touches the database: on a fresh SQL Server the
    // target database doesn't exist yet, so without this the very first operation (seeding
    // the admin account, just below) fails with "Cannot open database" and no account ever
    // gets created — a silent lockout, since there's no self-signup to fall back on.
    await dataStore.EnsureDatabaseExistsAsync(logger);

    // SaaS hierarchy (Platform → Organization → Project — see Organizations/Projects): every
    // install needs at least a default organization and project for its existing/first users
    // and data to belong to. Created once, here, before the admin-seeding step below so a
    // brand-new install's seed admin can be attached to it directly.
    ChatToDashboard.Api.Organizations.Organization? defaultOrg = null;
    try
    {
        var orgStore = scope.ServiceProvider.GetRequiredService<ChatToDashboard.Api.Organizations.OrganizationStore>();
        var projectStore = scope.ServiceProvider.GetRequiredService<ChatToDashboard.Api.Projects.ProjectStore>();
        defaultOrg = await orgStore.FirstAsync();
        if (defaultOrg is null)
        {
            defaultOrg = await orgStore.CreateAsync("المنظمة الافتراضية");
            await projectStore.CreateAsync(defaultOrg.Id, "المشروع الرئيسي");
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
        // would see every pre-existing file/integration too (ProjectId NULL never matches a
        // real project id) — the leak this whole ProjectId column exists to close.
        var defaultProject = await projectStore.FirstForOrganizationAsync(defaultOrg.Id);
        if (defaultProject is not null)
        {
            var repositoryStoreForBackfill = scope.ServiceProvider.GetRequiredService<ChatToDashboard.Api.Repository.RepositoryStore>();
            await repositoryStoreForBackfill.BackfillMissingProjectAsync(defaultProject.Id);
            var integrationStoreForBackfill = scope.ServiceProvider.GetRequiredService<ChatToDashboard.Api.Integrations.IntegrationStore>();
            await integrationStoreForBackfill.BackfillMissingProjectAsync(defaultProject.Id);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to ensure the default organization/project exist.");
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
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to ensure an initial admin account exists.");
    }

    // Demo accounts showing the two non-platform-owner permission shapes this app supports:
    // an org Admin (Role=Admin — sees/manages everything in their own organization, same as
    // any org admin) and a plain "User" account scoped to exactly one project via
    // ProjectRoles (see ProjectsController.List / HistoryController.CurrentProjectIdAsync —
    // a project, unlike a dashboard, has no single natural owner, so this is a named role
    // row rather than an OwnerId column). Looked up by name/username first so a restart
    // never recreates or duplicates them; the role assignment is re-applied every time
    // regardless, so it self-heals if ever removed by hand.
    try
    {
        var orgStore = scope.ServiceProvider.GetRequiredService<ChatToDashboard.Api.Organizations.OrganizationStore>();
        var projectStore = scope.ServiceProvider.GetRequiredService<ChatToDashboard.Api.Projects.ProjectStore>();
        var userStore = scope.ServiceProvider.GetRequiredService<UserStore>();

        var demoOrg = (await orgStore.ListAsync()).FirstOrDefault(o => o.Name == "شركة النموذج التجريبي")
            ?? await orgStore.CreateAsync("شركة النموذج التجريبي");
        var demoProject = (await projectStore.ListByOrganizationAsync(demoOrg.Id)).FirstOrDefault(p => p.Name == "المشروع الأول")
            ?? await projectStore.CreateAsync(demoOrg.Id, "المشروع الأول");

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

        var projectUser = await userStore.FindByUsernameAsync("project.owner");
        if (projectUser is null)
        {
            var pw = RandomNumberGenerator.GetHexString(12);
            projectUser = await userStore.CreateAsync(new AppUser
            {
                Username = "project.owner",
                DisplayName = $"مسؤول {demoProject.Name}",
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
                "Seeded a project-owner demo account for {Project}. Username: {Username} | Password: {Password}",
                demoProject.Name, "project.owner", pw);
        }
        await projectStore.SetRoleAsync(demoProject.Id, projectUser.Id, ChatToDashboard.Api.Projects.ProjectRoles.Owner);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to seed the demo organization/project/users.");
    }

    try
    {
        var loader = scope.ServiceProvider.GetRequiredService<DataFolderLoader>();
        var loaded = await loader.LoadAllAsync();
        logger.LogInformation("Startup data load complete: {Count} table(s) loaded from {Folder}",
            loaded.Count, loader.DataFolderPath);

        var documents = scope.ServiceProvider.GetRequiredService<DocumentSearchService>();
        documents.Reindex(loader.DataFolderPath);

        var systems = await scope.ServiceProvider.GetRequiredService<SystemApiLoader>().LoadAllAsync();
        foreach (var system in systems.Where(s => s.Error is not null))
            logger.LogWarning("System {System} could not be loaded: {Error}", system.System, system.Error);
    }
    catch (Exception ex)
    {
        logger.LogError(ex,
            "Startup data load failed. Check ConnectionStrings:DataDb (user-secrets) and DataFolderPath, " +
            "then call POST /api/data/refresh to retry without restarting.");
    }
}

app.Run();
