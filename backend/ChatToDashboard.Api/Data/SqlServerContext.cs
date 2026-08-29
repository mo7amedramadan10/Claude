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

    public async Task<SqlConnection> OpenConnectionAsync(CancellationToken ct = default)
    {
        var connection = CreateConnection();
        await connection.OpenAsync(ct);
        return connection;
    }
}
