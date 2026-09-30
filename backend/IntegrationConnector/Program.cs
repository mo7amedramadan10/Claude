// غرفة القيادة — External Integration Connector
//
// A small, self-contained service YOU deploy inside your own network. It has two jobs:
//   1. Receive each dashboard's design (widgets + a live query per widget, already written
//      against YOUR OWN database schema — see the query descriptions you gave us) and store it
//      locally in a small SQLite file next to this service.
//   2. On every request from the "viewer.html" page we gave you, re-run each widget's stored
//      query against YOUR live database (configured below) and return the current values.
//
// Your database's connection string NEVER leaves this file/environment — it is never sent to
// غرفة القيادة, and this service never calls out to us either. Every query this service ever
// runs was already validated as read-only (SELECT/WITH only) before we published it, and this
// service re-checks that itself before running anything, as a second, independent safeguard.
//
// Run it with: dotnet run
// Or build a self-contained binary: dotnet publish -c Release
// Configure it via appsettings.json (see the template in this folder) or environment
// variables — e.g. Connector__TargetConnectionString=... Connector__TargetDbProvider=SqlServer

using System.Data.Common;
using System.Text.Json;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration.GetSection("Connector").Get<ConnectorOptions>() ?? new ConnectorOptions();
builder.Services.AddSingleton(config);
builder.Services.AddSingleton<LocalStore>();
builder.Services.AddSingleton<TargetDatabase>();

// viewer.html/admin.html are static files hosted on YOUR OWN domain — almost always a
// different origin than this service (a different host and/or port), so every fetch() they
// make here is cross-origin. Without CORS headers the browser blocks it outright, no matter
// how the network itself is configured. Reflecting the caller's own Origin (rather than "*")
// is what lets this also work for the cookie identity mechanism, which needs
// Access-Control-Allow-Credentials — the two are mutually exclusive with a wildcard origin.
// This widens no real attack surface: /publish and /schema stay behind WriteApiKey regardless
// of what CORS allows, and /dashboards, /permissions, /directory were already designed to be
// called from any browser with no shared secret — see their own remarks below.
const string CorsPolicy = "AllowAnyOriginWithCredentials";
builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p => p
    .SetIsOriginAllowed(_ => true)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

var app = builder.Build();
app.UseCors(CorsPolicy);

// Runs once at startup — creates the local storage file (dashboards + permissions) if it
// doesn't exist yet. Entirely separate from your own live database.
await app.Services.GetRequiredService<LocalStore>().EnsureSchemaAsync();

// ---------- write endpoint: غرفة القيادة publishes a dashboard's design here ----------
app.MapPost("/publish", async (HttpRequest request, LocalStore store, ConnectorOptions options) =>
{
    if (!IsAuthorized(request, options))
        return Results.Unauthorized();

    PublishedDashboard? payload;
    try
    {
        payload = await JsonSerializer.DeserializeAsync<PublishedDashboard>(request.Body, Json.Options);
    }
    catch (JsonException)
    {
        return Results.BadRequest(new { error = "invalid JSON body" });
    }
    if (payload is null || string.IsNullOrWhiteSpace(payload.DashboardId))
        return Results.BadRequest(new { error = "dashboardId is required" });

    // Defense in depth: every query was already validated as read-only before غرفة القيادة
    // ever sent it, but this service re-checks independently rather than trusting that.
    foreach (var widget in payload.Widgets)
    {
        if (widget.Sql is { Length: > 0 } sql && ReadOnlySqlValidator.Validate(sql) is { } reason)
            return Results.BadRequest(new { error = $"widget '{widget.Title}': rejected query — {reason}" });
    }

    await store.SaveDashboardAsync(payload);
    return Results.Ok(new { ok = true });
});

// ---------- read endpoint (list): viewer.html calls this first ----------
app.MapGet("/dashboards", async (HttpRequest request, LocalStore store, ConnectorOptions options) =>
{
    var identity = ExtractIdentity(request, options.IdentityParameterName);
    var dashboards = await store.ListDashboardsAsync();
    var visible = dashboards.Where(d => IsVisibleTo(d, identity));
    return Results.Ok(visible.Select(d => new { dashboardId = d.DashboardId, title = d.Title }));
});

// ---------- read endpoint (one dashboard): live query execution happens here ----------
app.MapGet("/dashboards/{id}", async (string id, HttpRequest request, LocalStore store, TargetDatabase target, ConnectorOptions options) =>
{
    var identity = ExtractIdentity(request, options.IdentityParameterName);
    var dashboard = await store.GetDashboardAsync(id);
    if (dashboard is null || !IsVisibleTo(dashboard, identity))
        return Results.NotFound();

    var widgetsWithData = new List<object>();
    foreach (var widget in dashboard.Widgets)
    {
        object? data = null;
        if (widget.Sql is { Length: > 0 } sql)
        {
            try
            {
                data = await target.RunReadOnlyQueryAsync(sql, identity);
            }
            catch (Exception ex)
            {
                // One widget's query failing (e.g. a table renamed on your side since this was
                // published) must never take down the whole dashboard — it just shows no data
                // for that one widget, with the reason in this service's own console log.
                Console.Error.WriteLine($"[connector] widget '{widget.Title}' query failed: {ex.Message}");
            }
        }
        widgetsWithData.Add(new { widget.Type, widget.Title, widget.XKey, widget.YKey, widget.Source, data });
    }

    return Results.Ok(new { dashboardId = dashboard.DashboardId, title = dashboard.Title, summary = dashboard.Summary, widgets = widgetsWithData });
});

// ---------- permissions: admin.html writes each dashboard's visibility decision here ----------
// Not behind the backend API key — admin.html is a static file opened in whoever's browser has
// it, same as /dashboards and /directory below, so there's no secret it could safely carry.
// Access control for this endpoint is whatever network/hosting boundary you put around
// admin.html itself (e.g. only reachable on your internal admin network).
app.MapPost("/permissions", async (HttpRequest request, LocalStore store) =>
{
    PermissionUpdate? update;
    try
    {
        update = await JsonSerializer.DeserializeAsync<PermissionUpdate>(request.Body, Json.Options);
    }
    catch (JsonException)
    {
        return Results.BadRequest(new { error = "invalid JSON body" });
    }
    if (update is null || string.IsNullOrWhiteSpace(update.DashboardId))
        return Results.BadRequest(new { error = "dashboardId is required" });

    await store.SavePermissionAsync(update);
    return Results.Ok(new { ok = true });
});

// ---------- directory (optional): returns nothing by default ----------
// admin.html falls back to manual entry when this returns an empty list — see its own remarks.
// Wire this up to your own user/role directory if you want live search suggestions instead.
app.MapGet("/directory", (string? q) => Results.Ok(Array.Empty<object>()));

// ---------- schema discovery: غرفة القيادة reads your DB's real shape here, once, at setup ----------
// Lets an analyst there build dashboards directly against your real tables/columns without
// anyone typing your schema out by hand — this endpoint answers with structure only (table and
// column names/types), never a row of your actual data.
app.MapGet("/schema", async (HttpRequest request, TargetDatabase target, ConnectorOptions options) =>
{
    if (!IsAuthorized(request, options))
        return Results.Unauthorized();

    try
    {
        var schema = await target.GetSchemaAsync();
        return Results.Ok(schema);
    }
    catch (Exception ex)
    {
        return Results.Problem($"could not read schema: {ex.Message}", statusCode: 502);
    }
});

// ---------- ad-hoc live query: an analyst building a dashboard on غرفة القيادة picked your ----------
// integration as a data source — lets them see real rows from your database WHILE BUILDING,
// not only once a dashboard is published and someone opens viewer.html. Same double safeguard
// as every other query this service ever runs: read-only validated here independently (never
// trusting that غرفة القيادة already checked it), and capped to a fixed row count since this
// SQL is written live in a chat session, never reviewed ahead of time the way a published
// widget's fixed query is.
const int AdHocQueryMaxRows = 500;
app.MapPost("/query", async (HttpRequest request, TargetDatabase target, ConnectorOptions options) =>
{
    if (!IsAuthorized(request, options))
        return Results.Unauthorized();

    AdHocQueryRequest? body;
    try
    {
        body = await JsonSerializer.DeserializeAsync<AdHocQueryRequest>(request.Body, Json.Options);
    }
    catch (JsonException)
    {
        return Results.BadRequest(new { error = "invalid JSON body" });
    }
    if (string.IsNullOrWhiteSpace(body?.Sql))
        return Results.BadRequest(new { error = "sql is required" });

    try
    {
        var rows = await target.RunReadOnlyQueryAsync(body.Sql, identity: null, maxRows: AdHocQueryMaxRows);
        return Results.Ok(new { rowCount = rows.Count, rows });
    }
    catch (InvalidOperationException ex)
    {
        // Covers both ReadOnlySqlValidator's rejection and a missing TargetConnectionString —
        // either way, the caller gets a clear reason back rather than a bare 500.
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem($"query failed: {ex.Message}", statusCode: 502);
    }
});

// ---------- data-level permission keys: what FilterKey values you've actually populated ----------
// Optional — only meaningful once you've created a table named DataPermissions.TableName (see
// its own remarks) and started filling it in. Never an error if you haven't; just an empty list.
app.MapGet("/permission-filter-keys", async (HttpRequest request, TargetDatabase target, ConnectorOptions options) =>
{
    if (!IsAuthorized(request, options))
        return Results.Unauthorized();
    return Results.Ok(await target.GetDistinctPermissionFilterKeysAsync());
});

app.Run();

// ---------- helpers ----------

static bool IsAuthorized(HttpRequest request, ConnectorOptions options)
{
    if (string.IsNullOrWhiteSpace(options.WriteApiKey)) return true; // no key configured — open (only do this on a trusted internal network)
    var header = options.WriteApiAuthHeader is { Length: > 0 } h ? h : "X-Api-Key";
    return request.Headers.TryGetValue(header, out var value) && value == options.WriteApiKey;
}

// Checks the query string, then headers, then cookies for options.IdentityParameterName — this
// service doesn't need to know which of the three mechanisms غرفة القيادة's viewer.html was
// configured to use; it just looks everywhere that name could plausibly appear.
static string? ExtractIdentity(HttpRequest request, string? parameterName)
{
    if (string.IsNullOrWhiteSpace(parameterName)) return null;
    if (request.Query.TryGetValue(parameterName, out var q) && q.Count > 0) return q[0];
    if (request.Headers.TryGetValue(parameterName, out var h) && h.Count > 0) return h[0];
    if (request.Cookies.TryGetValue(parameterName, out var c)) return c;
    return null;
}

static bool IsVisibleTo(StoredDashboard dashboard, string? identity)
{
    if (!string.Equals(dashboard.PermissionMode, "restricted", StringComparison.OrdinalIgnoreCase)) return true;
    if (dashboard.AllowedIdentifiers.Count == 0) return false; // Part E's explicit empty-state rule: restricted + empty = visible to no one
    return identity is not null && dashboard.AllowedIdentifiers.Any(a => string.Equals(a.Id, identity, StringComparison.OrdinalIgnoreCase));
}

internal static class Json
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}

/// <summary>Data-level (row) permissions — a second, optional layer beneath the whole-dashboard
/// everyone/restricted check above. If you create a table with this exact name and these three
/// columns (UserId, FilterKey, FilterValue — all text), a widget غرفة القيادة marks as needing
/// per-viewer filtering gets wrapped, at publish time, as a subquery that only returns rows whose
/// value in a chosen column appears in your own table for the current viewer's identity and a
/// chosen FilterKey. You populate this table entirely yourself, with whatever authorization facts
/// your business actually has (e.g. "ahmed can see Finance and IT department rows") — this
/// service only ever reads it as a value-answers-value lookup, never writes to it, and never
/// invents what belongs in it.</summary>
internal static class DataPermissions
{
    public const string TableName = "ChatToDashboard_DataPermissions";
}

// ---------- config ----------

public class ConnectorOptions
{
    /// <summary>"SqlServer" or "Sqlite" — which driver to use for TargetConnectionString.</summary>
    public string TargetDbProvider { get; set; } = "Sqlite";

    /// <summary>Your own live database's connection string. Never sent to غرفة القيادة —
    /// set this here (or via the Connector__TargetConnectionString environment variable) only.</summary>
    public string TargetConnectionString { get; set; } = "";

    /// <summary>The shared secret غرفة القيادة sends on /publish and admin.html sends on
    /// /permissions — must match what you entered as "قيمة المصادقة" for this integration's
    /// write API. Leave empty to accept requests with no check (fine only on a fully trusted
    /// internal network — not recommended if this service is reachable from outside it).</summary>
    public string? WriteApiKey { get; set; }

    /// <summary>The header name the key above arrives on — must match "اسم رأس المصادقة".</summary>
    public string WriteApiAuthHeader { get; set; } = "X-Api-Key";

    /// <summary>Must match the exact parameter/header/cookie name confirmed for this
    /// integration's identity-transport mechanism (see غرفة القيادة's "آلية تعريف المستخدم
    /// الحالي"). Leave empty to treat every dashboard as visible to everyone.</summary>
    public string? IdentityParameterName { get; set; }

    /// <summary>Where the local SQLite storage file (dashboards + permissions — NOT your own
    /// business data) lives. A relative value is resolved against this service's own binary
    /// folder (see LocalStore) — never the process's current directory, which under IIS is
    /// typically something like System32\inetsrv rather than the site folder, and whose account
    /// (the App Pool identity) has no reason to be able to write there.</summary>
    public string LocalStoragePath { get; set; } = "connector-storage.db";
}

// ---------- wire shapes (published payload / stored shape) ----------

public class PublishedWidgetDto
{
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public string? XKey { get; set; }
    public string? YKey { get; set; }
    public string? Source { get; set; }
    public string? Sql { get; set; }
}

public class PublishedDashboard
{
    public string DashboardId { get; set; } = "";
    public string Title { get; set; } = "";
    public string PublishedAt { get; set; } = "";
    public string Summary { get; set; } = "";
    public List<PublishedWidgetDto> Widgets { get; set; } = new();
}

public class AllowedIdentifier
{
    public string Id { get; set; } = "";
    public string Label { get; set; } = "";
    public string? Kind { get; set; }
}

public class PermissionUpdate
{
    public string DashboardId { get; set; } = "";
    public string Mode { get; set; } = "everyone"; // "everyone" | "restricted"
    public List<AllowedIdentifier> AllowedIdentifiers { get; set; } = new();
}

/// <summary>Body of POST /query — see its own remarks above.</summary>
public class AdHocQueryRequest
{
    public string Sql { get; set; } = "";
}

public class StoredDashboard
{
    public string DashboardId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public List<PublishedWidgetDto> Widgets { get; set; } = new();
    public string PermissionMode { get; set; } = "everyone";
    public List<AllowedIdentifier> AllowedIdentifiers { get; set; } = new();
}

// ---------- local storage (SQLite, bundled — separate from your own live database) ----------

public class LocalStore
{
    private readonly string _connectionString;

    // Path.Combine ignores its first argument when the second is already rooted, so an
    // operator-supplied absolute path passes through unchanged — only the relative default
    // actually gets anchored to the binary folder here.
    public LocalStore(ConnectorOptions options) =>
        _connectionString = $"Data Source={Path.Combine(AppContext.BaseDirectory, options.LocalStoragePath)}";

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    public Task EnsureSchemaAsync()
    {
        using var connection = Open();
        connection.Execute(
            """
            CREATE TABLE IF NOT EXISTS Dashboards (
              DashboardId TEXT PRIMARY KEY, Title TEXT, Summary TEXT, WidgetsJson TEXT, UpdatedAt TEXT);
            CREATE TABLE IF NOT EXISTS Permissions (
              DashboardId TEXT PRIMARY KEY, Mode TEXT, AllowedIdentifiersJson TEXT);
            """);
        return Task.CompletedTask;
    }

    public Task SaveDashboardAsync(PublishedDashboard dashboard)
    {
        using var connection = Open();
        connection.Execute(
            """
            INSERT INTO Dashboards (DashboardId, Title, Summary, WidgetsJson, UpdatedAt)
            VALUES (@DashboardId, @Title, @Summary, @WidgetsJson, @UpdatedAt)
            ON CONFLICT(DashboardId) DO UPDATE SET
              Title = @Title, Summary = @Summary, WidgetsJson = @WidgetsJson, UpdatedAt = @UpdatedAt
            """,
            new
            {
                dashboard.DashboardId, dashboard.Title, dashboard.Summary,
                WidgetsJson = JsonSerializer.Serialize(dashboard.Widgets, Json.Options),
                UpdatedAt = DateTime.UtcNow.ToString("o"),
            });
        return Task.CompletedTask;
    }

    public Task<List<StoredDashboard>> ListDashboardsAsync()
    {
        using var connection = Open();
        var rows = connection.Query<DashboardRow>("SELECT DashboardId, Title, Summary, WidgetsJson FROM Dashboards").ToList();
        var permissions = connection.Query<PermissionRow>("SELECT DashboardId, Mode, AllowedIdentifiersJson FROM Permissions")
            .ToDictionary(p => p.DashboardId, p => p);

        var result = rows.Select(r =>
        {
            var d = new StoredDashboard
            {
                DashboardId = r.DashboardId, Title = r.Title, Summary = r.Summary,
                Widgets = JsonSerializer.Deserialize<List<PublishedWidgetDto>>(r.WidgetsJson, Json.Options) ?? new(),
            };
            if (permissions.TryGetValue(r.DashboardId, out var p))
            {
                d.PermissionMode = p.Mode;
                d.AllowedIdentifiers = JsonSerializer.Deserialize<List<AllowedIdentifier>>(p.AllowedIdentifiersJson, Json.Options) ?? new();
            }
            return d;
        }).ToList();
        return Task.FromResult(result);
    }

    public async Task<StoredDashboard?> GetDashboardAsync(string dashboardId) =>
        (await ListDashboardsAsync()).FirstOrDefault(d => d.DashboardId == dashboardId);

    public Task SavePermissionAsync(PermissionUpdate update)
    {
        using var connection = Open();
        connection.Execute(
            """
            INSERT INTO Permissions (DashboardId, Mode, AllowedIdentifiersJson) VALUES (@DashboardId, @Mode, @AllowedIdentifiersJson)
            ON CONFLICT(DashboardId) DO UPDATE SET Mode = @Mode, AllowedIdentifiersJson = @AllowedIdentifiersJson
            """,
            new
            {
                update.DashboardId, update.Mode,
                AllowedIdentifiersJson = JsonSerializer.Serialize(update.AllowedIdentifiers, Json.Options),
            });
        return Task.CompletedTask;
    }
}

// Dapper row shapes for LocalStore's raw SQL — kept strongly typed (rather than Dapper's bare
// `dynamic` results) so downstream code never touches `dynamic`.
internal class DashboardRow
{
    public string DashboardId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string WidgetsJson { get; set; } = "[]";
}

internal class PermissionRow
{
    public string DashboardId { get; set; } = "";
    public string Mode { get; set; } = "";
    public string AllowedIdentifiersJson { get; set; } = "[]";
}

// ---------- your own live database (the one this connector was built to query) ----------

public class TargetDatabase
{
    private readonly ConnectorOptions _options;

    public TargetDatabase(ConnectorOptions options) => _options = options;

    /// <param name="identity">The current viewer's identity (see ExtractIdentity), bound as a
    /// real query parameter — never string-interpolated — when the widget's SQL references
    /// @__viewer_identity__ (see PublishedWidget's remarks on data-level permissions). Absent for
    /// plain widgets with no such reference; this method never adds the parameter unless the SQL
    /// text actually names it, so an ordinary widget's query is completely unaffected.</param>
    /// <param name="maxRows">Stops reading after this many rows. Null (the default, used for a
    /// published widget's own fixed, already-reviewed query) means no cap — kept exactly as it
    /// was before this parameter existed. The ad-hoc /query endpoint passes a real cap, since
    /// that SQL is written live during a chat session, never reviewed ahead of time.</param>
    public async Task<List<Dictionary<string, object?>>> RunReadOnlyQueryAsync(string sql, string? identity = null, int? maxRows = null)
    {
        var reason = ReadOnlySqlValidator.Validate(sql);
        if (reason is not null) throw new InvalidOperationException($"query rejected: {reason}");
        if (string.IsNullOrWhiteSpace(_options.TargetConnectionString))
            throw new InvalidOperationException("TargetConnectionString is not configured.");

        await using DbConnection connection = _options.TargetDbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase)
            ? new SqlConnection(_options.TargetConnectionString)
            : new SqliteConnection(_options.TargetConnectionString);
        await connection.OpenAsync();

        var rows = new List<Dictionary<string, object?>>();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandTimeout = 30;
        if (sql.Contains("@__viewer_identity__", StringComparison.Ordinal))
        {
            var identityParam = command.CreateParameter();
            identityParam.ParameterName = "__viewer_identity__";
            identityParam.Value = (object?)identity ?? DBNull.Value;
            command.Parameters.Add(identityParam);
        }
        await using var reader = await command.ExecuteReaderAsync();
        while ((maxRows is null || rows.Count < maxRows) && await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (var i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            rows.Add(row);
        }
        return rows;
    }

    /// <summary>Distinct FilterKey values already sitting in your own data-permissions table
    /// (see DataPermissions.TableName) — lets غرفة القيادة offer them as a picklist instead of
    /// anyone typing a FilterKey name that might not match what you've actually populated. An
    /// empty list (never an error) if you haven't created that table — data-level permissions
    /// are entirely optional, and most integrations will never have one.</summary>
    public async Task<List<string>> GetDistinctPermissionFilterKeysAsync()
    {
        if (string.IsNullOrWhiteSpace(_options.TargetConnectionString)) return new();
        try
        {
            await using DbConnection connection = _options.TargetDbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase)
                ? new SqlConnection(_options.TargetConnectionString)
                : new SqliteConnection(_options.TargetConnectionString);
            await connection.OpenAsync();
            var keys = await connection.QueryAsync<string>(
                $"SELECT DISTINCT FilterKey FROM {DataPermissions.TableName} ORDER BY FilterKey");
            return keys.ToList();
        }
        catch
        {
            // Table doesn't exist, or any other read failure — same as "not configured": data-
            // level permissions just aren't available for this integration, never a hard error.
            return new();
        }
    }

    /// <summary>Structure only — table and column names/types, never a data row. SQL Server via
    /// INFORMATION_SCHEMA.COLUMNS; SQLite via sqlite_master + pragma_table_info (no
    /// INFORMATION_SCHEMA there). Ordered by table then column position, so the result reads the
    /// same way a hand-written description would.</summary>
    public async Task<SchemaInfo> GetSchemaAsync()
    {
        if (string.IsNullOrWhiteSpace(_options.TargetConnectionString))
            throw new InvalidOperationException("TargetConnectionString is not configured.");

        var isSqlServer = _options.TargetDbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase);
        await using DbConnection connection = isSqlServer
            ? new SqlConnection(_options.TargetConnectionString)
            : new SqliteConnection(_options.TargetConnectionString);
        await connection.OpenAsync();

        var tables = new List<SchemaTable>();
        if (isSqlServer)
        {
            var rows = await connection.QueryAsync(
                """
                SELECT TABLE_NAME AS TableName, COLUMN_NAME AS ColumnName, DATA_TYPE AS DataType
                FROM INFORMATION_SCHEMA.COLUMNS
                ORDER BY TABLE_NAME, ORDINAL_POSITION
                """);
            foreach (var group in rows.GroupBy(r => (string)r.TableName))
                tables.Add(new SchemaTable
                {
                    Name = group.Key,
                    Columns = group.Select(r => new SchemaColumn { Name = r.ColumnName, Type = r.DataType }).ToList(),
                });
        }
        else
        {
            var tableNames = await connection.QueryAsync<string>(
                "SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' ORDER BY name");
            foreach (var tableName in tableNames)
            {
                var columns = await connection.QueryAsync(
                    $"SELECT name AS ColumnName, type AS DataType FROM pragma_table_info('{tableName.Replace("'", "''")}')");
                tables.Add(new SchemaTable
                {
                    Name = tableName,
                    Columns = columns.Select(r => new SchemaColumn { Name = r.ColumnName, Type = (string)(r.DataType ?? "") }).ToList(),
                });
            }
        }

        return new SchemaInfo { Provider = _options.TargetDbProvider, Tables = tables };
    }
}

public class SchemaInfo
{
    public string Provider { get; set; } = "";
    public List<SchemaTable> Tables { get; set; } = new();
}

public class SchemaTable
{
    public string Name { get; set; } = "";
    public List<SchemaColumn> Columns { get; set; } = new();
}

public class SchemaColumn
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
}

/// <summary>Same read-only check غرفة القيادة itself runs before ever publishing a query to
/// you — duplicated here (this service has no reference back to that codebase) as an
/// independent second check, never trusting the publisher alone.</summary>
public static class ReadOnlySqlValidator
{
    private static readonly Regex ForbiddenKeywords = new(
        @"\b(INSERT|UPDATE|DELETE|DROP|ALTER|CREATE|TRUNCATE|MERGE|EXEC|EXECUTE|GRANT|REVOKE|" +
        @"ATTACH|DETACH|PRAGMA|REPLACE|VACUUM)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string? Validate(string sql)
    {
        var stripped = Regex.Replace(sql, @"--[^\n]*|/\*.*?\*/", " ", RegexOptions.Singleline).Trim();
        if (stripped.Length == 0) return "empty statement.";
        if (!Regex.IsMatch(stripped, @"^(SELECT|WITH)\b", RegexOptions.IgnoreCase))
            return "only SELECT statements (optionally starting with a WITH clause) are allowed.";
        var withoutTrailingSemicolons = stripped.TrimEnd(';', ' ', '\t', '\r', '\n');
        if (withoutTrailingSemicolons.Contains(';')) return "multiple statements are not allowed.";
        var forbidden = ForbiddenKeywords.Match(stripped);
        return forbidden.Success ? $"forbidden keyword '{forbidden.Value.ToUpperInvariant()}'." : null;
    }
}
