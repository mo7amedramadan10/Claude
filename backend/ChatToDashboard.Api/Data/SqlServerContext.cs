using Microsoft.Data.SqlClient;

namespace ChatToDashboard.Api.Data;

/// <summary>
/// Creates SqlConnection instances from the "DataDb" connection string
/// (set via dotnet user-secrets: ConnectionStrings:DataDb).
/// </summary>
public class SqlServerContext
{
    private readonly string _connectionString;

    public SqlServerContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DataDb");
        _connectionString = !string.IsNullOrWhiteSpace(connectionString)
            ? connectionString
            : throw new InvalidOperationException(
                "Connection string 'DataDb' is not configured. " +
                "Set it with: dotnet user-secrets set \"ConnectionStrings:DataDb\" \"<connection string>\"");
    }

    public SqlConnection CreateConnection() => new(_connectionString);

    /// <summary>
    /// Creates the target database if it doesn't exist yet (fresh SQL Server container,
    /// new local instance). Requires permission to connect to master; on servers where
    /// that's denied (e.g. Azure SQL) the failure is logged and the database is assumed
    /// to be pre-provisioned.
    /// </summary>
    public async Task EnsureDatabaseExistsAsync(ILogger logger, CancellationToken ct = default)
    {
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

    public async Task<SqlConnection> OpenConnectionAsync(CancellationToken ct = default)
    {
        var connection = CreateConnection();
        await connection.OpenAsync(ct);
        return connection;
    }
}
