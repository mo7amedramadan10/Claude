using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

namespace ChatToDashboard.Api.Data;

public enum DbProvider
{
    /// <summary>Shared, centrally hosted database — the deployment default.</summary>
    SqlServer,

    /// <summary>Single local file, no database server to install — for trying the app out.</summary>
    Sqlite,
}

/// <summary>
/// Owns everything that differs between the supported databases: connections, how staging
/// tables are named and created, how their columns are listed, and which SQL dialect Claude
/// is told to write.
/// </summary>
public class DataStore
{
    private const string SqlServerSchema = "staging";
    private const string SqlitePrefix = "staging_";

    private readonly string _connectionString;

    public DbProvider Provider { get; }

    public DataStore(IConfiguration configuration)
    {
        Provider = string.Equals(configuration["DatabaseProvider"], "Sqlite", StringComparison.OrdinalIgnoreCase)
            ? DbProvider.Sqlite
            : DbProvider.SqlServer;

        var configured = configuration.GetConnectionString("DataDb");
        if (Provider == DbProvider.Sqlite)
        {
            _connectionString = string.IsNullOrWhiteSpace(configured)
                ? $"Data Source={Path.Combine(AppContext.BaseDirectory, "chat-to-dashboard.db")}"
                : configured;
        }
        else
        {
            _connectionString = !string.IsNullOrWhiteSpace(configured)
                ? configured
                : throw new InvalidOperationException(
                    "Connection string 'DataDb' is not configured. Set it with: " +
                    "dotnet user-secrets set \"ConnectionStrings:DataDb\" \"<connection string>\" — or set " +
                    "\"DatabaseProvider\": \"Sqlite\" in appsettings.json to run against a local file " +
                    "with no database server to install.");
        }
    }

    public DbConnection CreateConnection() => Provider == DbProvider.Sqlite
        ? new SqliteConnection(_connectionString)
        : new SqlConnection(_connectionString);

    public async Task<DbConnection> OpenConnectionAsync(CancellationToken ct = default)
    {
        var connection = CreateConnection();
        await connection.OpenAsync(ct);
        if (Provider == DbProvider.Sqlite)
        {
            // Dashboard filters fire one read per widget concurrently (Promise.all on the
            // frontend), and the same file also takes writes (autosave, uploads) from other
            // requests landing at the same time. SQLite's default rollback-journal mode
            // blocks a reader while a writer holds the commit lock, and with the default
            // zero busy_timeout that surfaces immediately as "database is locked" — silently
            // swallowed by the frontend's per-widget fetch, leaving that widget's data stuck
            // on whatever it last showed. WAL lets readers proceed without waiting on a
            // writer's commit in the first place; the busy_timeout is a second line of
            // defense for the cases WAL alone doesn't cover (e.g. a concurrent schema change).
            await using var pragma = connection.CreateCommand();
            pragma.CommandText = "PRAGMA journal_mode=WAL; PRAGMA busy_timeout=10000;";
            await pragma.ExecuteNonQueryAsync(ct);
        }
        return connection;
    }

    /// <summary>
    /// Creates the target database if it doesn't exist yet (fresh SQL Server container, new
    /// local instance). SQLite creates its file on first connect, so this is a no-op there.
    /// Requires permission to connect to master; where that's denied (e.g. Azure SQL) the
    /// failure is logged and the database is assumed to be pre-provisioned.
    /// </summary>
    public async Task EnsureDatabaseExistsAsync(ILogger logger, CancellationToken ct = default)
    {
        if (Provider == DbProvider.Sqlite) return;

        var builder = new SqlConnectionStringBuilder(_connectionString);
        var database = builder.InitialCatalog;
        if (string.IsNullOrWhiteSpace(database)) return;

        try
        {
            builder.InitialCatalog = "master";
            await using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText =
                $"IF DB_ID(N'{database.Replace("'", "''")}') IS NULL CREATE DATABASE [{database.Replace("]", "]]")}]";
            await command.ExecuteNonQueryAsync(ct);
        }
        catch (SqlException ex)
        {
            logger.LogWarning(
                "Could not verify/create database '{Database}' via master ({Message}); assuming it already exists.",
                database, ex.Message);
        }
    }

    // ---- Naming ----

    /// <summary>The schema a project's own uploaded-file tables live in — SQL Server only.
    /// SQLite has no real schema concept, so every project keeps sharing the single
    /// <c>staging_</c> prefix there (SQLite is the local/trial path, never a real multi-tenant
    /// deployment — see <see cref="DbProvider"/>'s own doc comments). Falls back to the shared
    /// "staging" schema when <paramref name="projectId"/> is null/blank or doesn't look like one
    /// of this app's own ids (defensive: an unexpected value never flows into a dynamically
    /// built schema name). System-sourced and data-folder tables (see SystemApiLoader,
    /// DataFolderLoader) have no project of their own and always stay in "staging" — only
    /// RepositoryStore's uploaded files get their own project schema.</summary>
    internal string ProjectSchema(string? projectId)
    {
        if (Provider == DbProvider.Sqlite) return SqlServerSchema;
        if (string.IsNullOrWhiteSpace(projectId) || !IsValidProjectId(projectId)) return SqlServerSchema;
        return "proj_" + projectId;
    }

    private static bool IsValidProjectId(string projectId) =>
        projectId.Length is > 0 and <= 64 && projectId.All(char.IsAsciiHexDigit);

    /// <summary>Quoted table reference for generated DDL/DML. <paramref name="schema"/> is
    /// ignored on SQLite (no real schema concept there — the prefix stands in).</summary>
    internal string QualifiedTable(string schema, string tableName) => Provider == DbProvider.Sqlite
        ? Quote(SqlitePrefix + tableName)
        : $"[{schema}].[{tableName.Replace("]", "]]")}]";

    /// <summary>Same as the two-argument overload, but for a display name already carrying its
    /// own schema/prefix (e.g. "proj_xxx.Sales" or "staging.Sales" on SQL Server, "staging_sales"
    /// on SQLite) — as returned by <see cref="GetSchemaAsync"/>, never built from client input.
    /// Generic over whichever schema the name actually carries (shared or any project's), so
    /// callers never need to know or re-derive which one a given table lives in.</summary>
    internal string QualifiedTable(string displayName)
    {
        if (Provider == DbProvider.Sqlite) return Quote(displayName);
        var parts = displayName.Split('.');
        return string.Join(".", parts.Select(p => $"[{p.Replace("]", "]]")}]"));
    }

    /// <summary>The name Claude sees and writes in its queries, for a freshly loaded file —
    /// under <paramref name="projectId"/>'s own schema on SQL Server, or the single shared
    /// prefix on SQLite.</summary>
    public string DisplayTable(string? projectId, string tableName) => Provider == DbProvider.Sqlite
        ? SqlitePrefix + tableName
        : $"{ProjectSchema(projectId)}.{tableName}";

    /// <summary>Strips the schema/prefix from a display name, giving the bare table name. Works
    /// the same regardless of which SQL Server schema (shared or per-project) the name carries —
    /// it only looks for the single '.' separator, not a specific schema name.</summary>
    public string BareTableName(string displayName) => Provider == DbProvider.Sqlite
        ? (displayName.StartsWith(SqlitePrefix, StringComparison.Ordinal)
            ? displayName[SqlitePrefix.Length..]
            : displayName)
        : (displayName.Contains('.') ? displayName.Split('.').Last() : displayName);

    public async Task DropTableAsync(DbConnection connection, string? projectId, string bareName, CancellationToken ct = default)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"DROP TABLE IF EXISTS {QualifiedTable(ProjectSchema(projectId), bareName)}";
        await command.ExecuteNonQueryAsync(ct);
    }

    /// <summary>The name Claude sees, for a table name as returned by the catalog query —
    /// <paramref name="schema"/> is whichever schema that particular row actually came from
    /// (shared or a specific project's), since <see cref="GetSchemaAsync"/> can return rows from
    /// both in one call.</summary>
    private string CatalogDisplayTable(string schema, string rawName) => Provider == DbProvider.Sqlite
        ? rawName // SQLite table names already carry the staging_ prefix.
        : $"{schema}.{rawName}";

    private string Quote(string identifier) => Provider == DbProvider.Sqlite
        ? $"\"{identifier.Replace("\"", "\"\"")}\""
        : $"[{identifier.Replace("]", "]]")}]";

    private string SqlTypeFor(Type type)
    {
        if (Provider == DbProvider.Sqlite)
            return type == typeof(long) || type == typeof(bool) ? "INTEGER"
                : type == typeof(decimal) ? "REAL"
                : "TEXT"; // DateTime is stored as ISO-8601 text.

        return type == typeof(long) ? "BIGINT"
            : type == typeof(decimal) ? "DECIMAL(18,4)"
            : type == typeof(DateTime) ? "DATETIME2"
            : type == typeof(bool) ? "BIT"
            : "NVARCHAR(MAX)";
    }

    // ---- Schema ----

    /// <summary>Every column of every table visible to <paramref name="projectId"/>: the shared
    /// schema (system sources, data-folder reference tables) on its own when
    /// <paramref name="projectId"/> is null, or unioned with that project's own uploaded-file
    /// schema when it isn't. On SQL Server this scopes the scan at the SQL level — a project
    /// never sees, and the scan never even reads, another project's tables — rather than relying
    /// solely on the application-level <c>IsTableKnownToProject</c> filter every caller still
    /// applies on top (kept as a second, now-redundant layer of defense). SQLite has no per-
    /// project schema (see <see cref="ProjectSchema"/>), so <paramref name="projectId"/> is
    /// ignored there and the single shared prefix is scanned exactly as before.</summary>
    public async Task<IReadOnlyList<TableSchema>> GetSchemaAsync(string? projectId = null, CancellationToken ct = default)
    {
        var projectSchema = ProjectSchema(projectId);
        var scopedToProject = Provider != DbProvider.Sqlite
            && !string.Equals(projectSchema, SqlServerSchema, StringComparison.Ordinal);

        var sql = Provider == DbProvider.Sqlite
            ? """
              SELECT 'staging' AS TableSchema, m.name AS TableName, p.name AS ColumnName, p.type AS DataType
              FROM sqlite_master m
              JOIN pragma_table_info(m.name) p
              WHERE m.type = 'table' AND m.name LIKE 'staging\_%' ESCAPE '\'
              ORDER BY m.name, p.cid
              """
            : scopedToProject
                ? """
                  SELECT c.TABLE_SCHEMA AS TableSchema, c.TABLE_NAME AS TableName, c.COLUMN_NAME AS ColumnName, c.DATA_TYPE AS DataType
                  FROM INFORMATION_SCHEMA.COLUMNS c
                  WHERE c.TABLE_SCHEMA IN (@shared, @project)
                  ORDER BY c.TABLE_SCHEMA, c.TABLE_NAME, c.ORDINAL_POSITION
                  """
                : """
                  SELECT c.TABLE_SCHEMA AS TableSchema, c.TABLE_NAME AS TableName, c.COLUMN_NAME AS ColumnName, c.DATA_TYPE AS DataType
                  FROM INFORMATION_SCHEMA.COLUMNS c
                  WHERE c.TABLE_SCHEMA = @shared
                  ORDER BY c.TABLE_NAME, c.ORDINAL_POSITION
                  """;

        await using var connection = await OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        // Same slow/shared-SQL-Server reasoning as LoadCommandTimeoutSeconds's own comment:
        // this runs ahead of every new upload's save (see RepositoryStore.ResolveBareTable-
        // NameAsync's collision check) and was seen live hitting SqlCommand's 30s default on
        // its own, as "Execution Timeout Expired" on save well before the actual table load
        // — scanning INFORMATION_SCHEMA.COLUMNS is cheap on a fast link but not free on a
        // loaded one, even now that it's scoped to (at most) two schemas instead of every table
        // on the server.
        command.CommandTimeout = LoadCommandTimeoutSeconds;
        if (Provider != DbProvider.Sqlite)
        {
            command.Parameters.Add(new SqlParameter("@shared", SqlServerSchema));
            if (scopedToProject) command.Parameters.Add(new SqlParameter("@project", projectSchema));
        }

        var tables = new Dictionary<string, (string Schema, string Table, List<TableColumn> Columns)>();
        var order = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var schema = reader.GetString(0);
            var table = reader.GetString(1);
            var key = schema + "." + table;
            if (!tables.TryGetValue(key, out var entry))
            {
                entry = (schema, table, new List<TableColumn>());
                tables[key] = entry;
                order.Add(key);
            }
            entry.Columns.Add(new TableColumn(reader.GetString(2), reader.GetString(3)));
        }

        return order.Select(k =>
        {
            var entry = tables[k];
            return new TableSchema(CatalogDisplayTable(entry.Schema, entry.Table), entry.Columns);
        }).ToList();
    }

    public async Task CreateContainerIfMissingAsync(DbConnection connection, CancellationToken ct = default)
    {
        if (Provider == DbProvider.Sqlite) return; // No schemas in SQLite; the prefix stands in.

        await using var command = connection.CreateCommand();
        command.CommandText = $"IF SCHEMA_ID('{SqlServerSchema}') IS NULL EXEC('CREATE SCHEMA [{SqlServerSchema}]')";
        await command.ExecuteNonQueryAsync(ct);
    }

    /// <summary>Same as <see cref="CreateContainerIfMissingAsync"/> but for one project's own
    /// schema (SQL Server only — a no-op on SQLite, and a no-op when <paramref name="projectId"/>
    /// resolves to the shared schema, which <see cref="CreateContainerIfMissingAsync"/> already
    /// covers). Called lazily, right before a project's first upload needs its schema to exist —
    /// the same lazy, idempotent, first-use pattern every schema in this app is already created
    /// by; no eager hook at project-creation time.</summary>
    /// <param name="projectName">When given, stamped onto the schema as its MS_Description
    /// extended property (what SSMS/Object Explorer shows as the schema's description/tooltip),
    /// purely so a human browsing the database can tell which project <c>proj_&lt;id&gt;</c>
    /// belongs to without a lookup. Never affects the schema's own name — every query, DDL
    /// statement and the LLM's own prompt keep referencing the short, stable <c>proj_&lt;id&gt;</c>
    /// form; embedding the (often Arabic, always mutable) display name into the identifier
    /// itself would make every generated query longer and would orphan the name the moment the
    /// project is renamed. Kept up to date on every call (not just the first) via
    /// sp_updateextendedproperty, so a later rename is reflected without a special migration
    /// path of its own.</param>
    public async Task CreateProjectSchemaIfMissingAsync(
        DbConnection connection, string? projectId, string? projectName = null, CancellationToken ct = default)
    {
        if (Provider == DbProvider.Sqlite) return;
        var schema = ProjectSchema(projectId);
        if (string.Equals(schema, SqlServerSchema, StringComparison.Ordinal)) return;

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = $"IF SCHEMA_ID('{schema}') IS NULL EXEC('CREATE SCHEMA [{schema}]')";
            await command.ExecuteNonQueryAsync(ct);
        }

        if (string.IsNullOrWhiteSpace(projectName)) return;

        await using var prop = connection.CreateCommand();
        prop.CommandText = """
            IF EXISTS (SELECT 1 FROM sys.extended_properties
                       WHERE class = 3 AND major_id = SCHEMA_ID(@schema) AND name = N'MS_Description')
                EXEC sp_updateextendedproperty @name = N'MS_Description', @value = @projectName,
                    @level0type = N'SCHEMA', @level0name = @schema;
            ELSE
                EXEC sp_addextendedproperty @name = N'MS_Description', @value = @projectName,
                    @level0type = N'SCHEMA', @level0name = @schema;
            """;
        prop.Parameters.Add(new SqlParameter("@schema", schema));
        prop.Parameters.Add(new SqlParameter("@projectName", projectName));
        await prop.ExecuteNonQueryAsync(ct);
    }

    // ---- Loading ----

    /// <summary>The default SqlCommand/SqlBulkCopy timeout (30s) is plenty on a fast local
    /// connection, but real deployments hit it over a slower or more loaded shared SQL Server
    /// link — seen live as "Execution Timeout Expired" while loading a ~1000-row upload. A
    /// larger ceiling here costs nothing on a fast connection and gives a slow one the room a
    /// bigger table load can genuinely need.</summary>
    private const int LoadCommandTimeoutSeconds = 120;

    /// <summary>Drops and recreates the table under <paramref name="projectId"/>'s own schema
    /// (or the shared schema when null — used by SystemApiLoader/DataFolderLoader, which have
    /// no project of their own), then loads every row of <paramref name="table"/>.</summary>
    public async Task RecreateAndLoadAsync(
        DbConnection connection, string? projectId, string tableName, DataTable table, CancellationToken ct = default)
    {
        var target = QualifiedTable(ProjectSchema(projectId), tableName);
        var columnDefs = table.Columns.Cast<DataColumn>()
            .Select(c => $"{Quote(c.ColumnName)} {SqlTypeFor(c.DataType)} NULL");

        await using (var drop = connection.CreateCommand())
        {
            drop.CommandText = $"DROP TABLE IF EXISTS {target}";
            drop.CommandTimeout = LoadCommandTimeoutSeconds;
            await drop.ExecuteNonQueryAsync(ct);
        }
        await using (var create = connection.CreateCommand())
        {
            create.CommandText = $"CREATE TABLE {target} ({string.Join(", ", columnDefs)})";
            create.CommandTimeout = LoadCommandTimeoutSeconds;
            await create.ExecuteNonQueryAsync(ct);
        }

        if (table.Rows.Count == 0) return;

        if (Provider == DbProvider.Sqlite)
            await InsertRowsAsync((SqliteConnection)connection, target, table, ct);
        else
            await BulkCopyAsync((SqlConnection)connection, target, table, ct);
    }

    private static async Task BulkCopyAsync(
        SqlConnection connection, string target, DataTable table, CancellationToken ct)
    {
        using var bulk = new SqlBulkCopy(connection)
        {
            DestinationTableName = target,
            BatchSize = 5000,
            BulkCopyTimeout = LoadCommandTimeoutSeconds,
        };
        foreach (DataColumn column in table.Columns)
            bulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        await bulk.WriteToServerAsync(table, ct);
    }

    private async Task InsertRowsAsync(
        SqliteConnection connection, string target, DataTable table, CancellationToken ct)
    {
        var columns = table.Columns.Cast<DataColumn>().ToList();
        var columnList = string.Join(", ", columns.Select(c => Quote(c.ColumnName)));
        var valueList = string.Join(", ", columns.Select((_, i) => $"$p{i}"));

        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(ct);
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"INSERT INTO {target} ({columnList}) VALUES ({valueList})";

        var parameters = columns
            .Select((_, i) => command.Parameters.Add(new SqliteParameter($"$p{i}", DBNull.Value)))
            .ToList();

        foreach (DataRow row in table.Rows)
        {
            for (var i = 0; i < columns.Count; i++)
            {
                var value = row[i];
                // Microsoft.Data.Sqlite stores decimal as TEXT, which breaks SUM/AVG — use REAL.
                parameters[i].Value = value is decimal d ? (double)d : value;
            }
            await command.ExecuteNonQueryAsync(ct);
        }

        await transaction.CommitAsync(ct);
    }

    // ---- Dialect (fed into Claude's system prompt and tool descriptions) ----

    public string DialectName => Provider == DbProvider.Sqlite ? "SQLite" : "Microsoft SQL Server (T-SQL)";

    /// <summary><paramref name="projectId"/> picks which schema the hint names — a project's
    /// own uploaded files live under its own schema on SQL Server now, not the shared one.</summary>
    public string TableNamingHint(string? projectId) => Provider == DbProvider.Sqlite
        ? "staging_<TableName> (SQLite has no schemas, so the prefix is part of the name)"
        : $"{ProjectSchema(projectId)}.<TableName> for your own uploaded files, or staging.<TableName> for shared/system tables";

    /// <summary>Same <paramref name="projectId"/> scoping as <see cref="TableNamingHint"/>.</summary>
    public string DialectPrompt(string? projectId) => Provider == DbProvider.Sqlite
        ? """
          SQL DIALECT — this is SQLite, NOT SQL Server or PostgreSQL:
          - Use LIMIT 500 to cap results; there is no TOP.
          - Dates are stored as ISO-8601 text. Group by month with strftime('%Y-%m', [col]);
            use date('now') for the current date.
          - Quote identifiers with "double quotes".
          - Tables have no schema; reference them as staging_<TableName> (the prefix is part of the name).
          - Only SELECT statements are allowed. Any INSERT/UPDATE/DELETE/DDL/PRAGMA is rejected.
          - If a query fails, read the error message and correct your SQL.
          """
        : $"""
          SQL DIALECT — this is Microsoft SQL Server (T-SQL), NOT PostgreSQL or MySQL:
          - Use TOP N, never LIMIT. Always cap results: SELECT TOP 500 ...
          - Use GETDATE() instead of NOW(); DATEADD/DATEDIFF for date math; FORMAT() or CONVERT() for formatting.
          - Quote identifiers with [square brackets], not double quotes.
          - Reference a table exactly as it was listed to you (either {ProjectSchema(projectId)}.[TableName]
            or staging.[TableName]) — never assume one schema for every table.
          - Only SELECT statements are allowed. Any INSERT/UPDATE/DELETE/DDL is rejected.
          - Every uploaded-file column is stored as nvarchar regardless of what it actually
            holds, even a column that is numeric in almost every row. A plain CAST/CONVERT to a
            numeric type fails the ENTIRE query the instant even one row holds a blank/non-
            numeric value — "Error converting data type nvarchar to numeric" — even though every
            other row would have cast fine. If you hit that exact error, retry immediately with
            TRY_CAST(... AS DECIMAL) / TRY_CONVERT(DECIMAL, ...) instead of CAST/CONVERT: it
            returns NULL for the one bad row instead of failing the whole statement, and
            SUM/AVG/COUNT already ignore NULLs on their own. Never give up on a widget (noData)
            over this specific error without trying TRY_CAST/TRY_CONVERT first.
          - If a query fails for any other reason, read the error message and correct your SQL.
          """;
}
