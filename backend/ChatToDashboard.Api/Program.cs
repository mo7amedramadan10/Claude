using ChatToDashboard.Api.Claude;
using ChatToDashboard.Api.Data;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.OpenAi;
using ChatToDashboard.Api.Repository;
using ChatToDashboard.Api.Sources;
using ChatToDashboard.Api.Usage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<DataStore>();
builder.Services.AddSingleton<DataFolderLoader>();
builder.Services.AddSingleton<DocumentSearchService>();
builder.Services.AddSingleton<RepositoryStore>();
builder.Services.AddSingleton<UploadParser>();
builder.Services.Configure<SourceOptions>(builder.Configuration.GetSection(SourceOptions.SectionName));
builder.Services.Configure<PricingOptions>(builder.Configuration.GetSection(PricingOptions.SectionName));
builder.Services.AddSingleton<UsageStore>();
builder.Services.AddSingleton<UsageTracker>();
builder.Services.AddSingleton<AnalyticsTools>();

// Which LLM answers the questions: "Anthropic" (default) or "OpenAI".
var llmProvider = builder.Configuration["Llm:Provider"] ?? "Anthropic";
if (string.Equals(llmProvider, "OpenAI", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddHttpClient<IDashboardGenerator, OpenAiClient>(client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com/");
        client.Timeout = TimeSpan.FromMinutes(5);
    });
}
else
{
    builder.Services.AddHttpClient<IDashboardGenerator, ClaudeClient>(client =>
    {
        client.BaseAddress = new Uri("https://api.anthropic.com/");
        client.Timeout = TimeSpan.FromMinutes(5);
    });
}

var app = builder.Build();

// The UI lives in wwwroot and is served from this same app — one project, one URL.
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// The observability page lives on its own link, unrelated to the dashboard.
app.MapGet("/usage", (IWebHostEnvironment env) =>
    Results.File(Path.Combine(env.WebRootPath, "usage.html"), "text/html"));

// Initial load: scan the data folder and (re)create the staging tables so the
// shared SQL Server copy reflects the current files. Failures are logged but do
// not prevent startup — POST /api/data/refresh can retry once SQL is reachable.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    // Logged so a stale or unsaved appsettings.json is obvious at a glance.
    logger.LogInformation("Configuration in use — LLM provider: {Llm}, database: {Db}",
        llmProvider, scope.ServiceProvider.GetRequiredService<DataStore>().Provider);
    try
    {
        var loader = scope.ServiceProvider.GetRequiredService<DataFolderLoader>();
        var loaded = await loader.LoadAllAsync();
        logger.LogInformation("Startup data load complete: {Count} table(s) loaded from {Folder}",
            loaded.Count, loader.DataFolderPath);

        var documents = scope.ServiceProvider.GetRequiredService<DocumentSearchService>();
        documents.Reindex(loader.DataFolderPath);
    }
    catch (Exception ex)
    {
        logger.LogError(ex,
            "Startup data load failed. Check ConnectionStrings:DataDb (user-secrets) and DataFolderPath, " +
            "then call POST /api/data/refresh to retry without restarting.");
    }
}

app.Run();
