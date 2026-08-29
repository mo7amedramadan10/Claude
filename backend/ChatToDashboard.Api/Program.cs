using ChatToDashboard.Api.Claude;
using ChatToDashboard.Api.Data;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "frontend";

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddSingleton<DataStore>();
builder.Services.AddSingleton<DataFolderLoader>();
builder.Services.AddSingleton<DocumentSearchService>();
builder.Services.AddHttpClient<ClaudeClient>(client =>
{
    client.BaseAddress = new Uri("https://api.anthropic.com/");
    client.Timeout = TimeSpan.FromMinutes(5);
});

var app = builder.Build();

app.UseCors(FrontendCorsPolicy);

// In production the frontend build is copied into wwwroot (see Dockerfile) and served
// from the same origin; in dev, Vite serves it separately and these are no-ops.
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// Initial load: scan the data folder and (re)create the staging tables so the
// shared SQL Server copy reflects the current files. Failures are logged but do
// not prevent startup — POST /api/data/refresh can retry once SQL is reachable.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
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
