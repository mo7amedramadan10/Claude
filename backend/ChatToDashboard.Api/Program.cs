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
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

builder.Services.AddSingleton<UserStore>();
builder.Services.AddSingleton<PermissionsService>();
builder.Services.Configure<LdapOptions>(builder.Configuration.GetSection(LdapOptions.SectionName));
builder.Services.AddSingleton<LdapAuthenticator>();

builder.Services.AddSingleton<DataStore>();
builder.Services.AddSingleton<DataFolderLoader>();
builder.Services.AddSingleton<DocumentSearchService>();
builder.Services.AddSingleton<RepositoryStore>();
builder.Services.AddSingleton<UploadParser>();
builder.Services.Configure<SourceOptions>(builder.Configuration.GetSection(SourceOptions.SectionName));
builder.Services.Configure<PricingOptions>(builder.Configuration.GetSection(PricingOptions.SectionName));
builder.Services.AddSingleton<CostCalculator>();
builder.Services.AddSingleton<UsageStore>();
builder.Services.AddSingleton<UsageTracker>();
builder.Services.AddSingleton<SystemApiLoader>();
builder.Services.AddSingleton<AnalyticsTools>();
builder.Services.AddSingleton<HistoryStore>();
builder.Services.AddSingleton<ShareStore>();
builder.Services.AddSingleton<ChatToDashboard.Api.Widgets.WidgetQueryService>();

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
    client.Timeout = TimeSpan.FromMinutes(5);
});
builder.Services.AddSingleton<IDashboardGenerator, ChatToDashboard.Api.Llm.LlmRouter>();

var app = builder.Build();

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

// Initial load: scan the data folder and (re)create the staging tables so the
// shared SQL Server copy reflects the current files. Failures are logged but do
// not prevent startup — POST /api/data/refresh can retry once SQL is reachable.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    // Logged so a stale or unsaved appsettings.json is obvious at a glance. The effective
    // provider can differ from this default if an admin overrode it from the dashboard's
    // model selector (LlmSettingsStore) — that only takes effect per-question, not here.
    logger.LogInformation("Configuration in use — LLM provider (default): {Llm}, database: {Db}",
        llmProvider, scope.ServiceProvider.GetRequiredService<DataStore>().Provider);

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
                AllowAllCategories = true,
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
