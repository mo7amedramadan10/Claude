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

    /// <summary>Quoted table reference for generated DDL/DML.</summary>
    private string QualifiedTable(string tableName) => Provider == DbProvider.Sqlite
        ? Quote(SqlitePrefix + tableName)
        : $"[{SqlServerSchema}].[{tableName.Replace("]", "]]")}]";

    /// <summary>The name Claude sees and writes in its queries, for a freshly loaded file.</summary>
    public string DisplayTable(string tableName) => Provider == DbProvider.Sqlite
        ? SqlitePrefix + tableName
        : $"{SqlServerSchema}.{tableName}";

    /// <summary>The name Claude sees, for a table name as returned by the catalog query.</summary>
    private string CatalogDisplayTable(string rawName) => Provider == DbProvider.Sqlite
        ? rawName // SQLite table names already carry the staging_ prefix.
        : $"{SqlServerSchema}.{rawName}";

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

    public async Task<IReadOnlyList<TableSchema>> GetSchemaAsync(CancellationToken ct = default)
    {
        var sql = Provider == DbProvider.Sqlite
            ? """
              SELECT m.name AS TableName, p.name AS ColumnName, p.type AS DataType
              FROM sqlite_master m
              JOIN pragma_table_info(m.name) p
              WHERE m.type = 'table' AND m.name LIKE 'staging\_%' ESCAPE '\'
              ORDER BY m.name, p.cid
              """
            : """
              SELECT c.TABLE_NAME AS TableName, c.COLUMN_NAME AS ColumnName, c.DATA_TYPE AS DataType
              FROM INFORMATION_SCHEMA.COLUMNS c
              WHERE c.TABLE_SCHEMA = 'staging'
              ORDER BY c.TABLE_NAME, c.ORDINAL_POSITION
              """;

        await using var connection = await OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        var tables = new Dictionary<string, List<TableColumn>>();
        var order = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var table = reader.GetString(0);
            if (!tables.TryGetValue(table, out var columns))
            {
                columns = new List<TableColumn>();
                tables[table] = columns;
                order.Add(table);
            }
            columns.Add(new TableColumn(reader.GetString(1), reader.GetString(2)));
        }

        return order.Select(t => new TableSchema(CatalogDisplayTable(t), tables[t])).ToList();
    }

    public async Task CreateContainerIfMissingAsync(DbConnection connection, CancellationToken ct = default)
    {
        if (Provider == DbProvider.Sqlite) return; // No schemas in SQLite; the prefix stands in.

        await using var command = connection.CreateCommand();
        command.CommandText = $"IF SCHEMA_ID('{SqlServerSchema}') IS NULL EXEC('CREATE SCHEMA [{SqlServerSchema}]')";
        await command.ExecuteNonQueryAsync(ct);
    }

    // ---- Loading ----

    /// <summary>Drops and recreates the staging table, then loads every row of <paramref name="table"/>.</summary>
    public async Task RecreateAndLoadAsync(
        DbConnection connection, string tableName, DataTable table, CancellationToken ct = default)
    {
        var target = QualifiedTable(tableName);
        var columnDefs = table.Columns.Cast<DataColumn>()
            .Select(c => $"{Quote(c.ColumnName)} {SqlTypeFor(c.DataType)} NULL");

        await using (var drop = connection.CreateCommand())
        {
            drop.CommandText = $"DROP TABLE IF EXISTS {target}";
            await drop.ExecuteNonQueryAsync(ct);
        }
        await using (var create = connection.CreateCommand())
        {
            create.CommandText = $"CREATE TABLE {target} ({string.Join(", ", columnDefs)})";
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

    public string TableNamingHint => Provider == DbProvider.Sqlite
        ? "staging_<TableName> (SQLite has no schemas, so the prefix is part of the name)"
        : "staging.<TableName>";

    public string DialectPrompt => Provider == DbProvider.Sqlite
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
        : """
          SQL DIALECT — this is Microsoft SQL Server (T-SQL), NOT PostgreSQL or MySQL:
          - Use TOP N, never LIMIT. Always cap results: SELECT TOP 500 ...
          - Use GETDATE() instead of NOW(); DATEADD/DATEDIFF for date math; FORMAT() or CONVERT() for formatting.
          - Quote identifiers with [square brackets], not double quotes.
          - Tables live in the staging schema; always reference them as staging.[TableName].
          - Only SELECT statements are allowed. Any INSERT/UPDATE/DELETE/DDL is rejected.
          - If a query fails, read the error message and correct your SQL.
          """;
}
