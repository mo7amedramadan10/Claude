using System.Text.Json;
using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Users;

/// <summary>Accounts and their per-source permissions.</summary>
public class UserStore
{
    private readonly DataStore _db;
    private readonly ILogger<UserStore> _logger;

    public UserStore(DataStore db, ILogger<UserStore> logger)
    {
        _db = db;
        _logger = logger;
    }

    private string Table => _db.Provider == DbProvider.Sqlite ? "\"AppUsers\"" : "[staging].[AppUsers]";

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);

        var text = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {Table} (
                 "Id" TEXT PRIMARY KEY, "Username" TEXT UNIQUE, "DisplayName" TEXT,
                 "PasswordHash" TEXT, "AuthMethod" TEXT, "Role" TEXT, "IsActive" INTEGER,
                 "AllowAllSystems" INTEGER, "AllowedSystemsJson" TEXT,
                 "AllowAllCategories" INTEGER, "AllowedCategoriesJson" TEXT, "CreatedAt" TEXT)
               """
            : $"""
               IF OBJECT_ID('staging.AppUsers') IS NULL
               CREATE TABLE {Table} (
                 [Id] NVARCHAR(64) PRIMARY KEY, [Username] NVARCHAR(200) UNIQUE, [DisplayName] NVARCHAR(200),
                 [PasswordHash] NVARCHAR(400), [AuthMethod] NVARCHAR(40), [Role] NVARCHAR(40), [IsActive] BIT,
                 [AllowAllSystems] BIT, [AllowedSystemsJson] NVARCHAR(MAX),
                 [AllowAllCategories] BIT, [AllowedCategoriesJson] NVARCHAR(MAX), [CreatedAt] DATETIME2)
               """;

        await using var command = connection.CreateCommand();
        command.CommandText = text;
        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {Table}");
    }

    public async Task<IReadOnlyList<AppUser>> ListAsync(CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<AppUser>($"SELECT * FROM {Table} ORDER BY Username");
        return rows.ToList();
    }

    public async Task<AppUser?> FindByIdAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<AppUser>($"SELECT * FROM {Table} WHERE Id = @id", new { id });
    }

    public async Task<AppUser?> FindByUsernameAsync(string username, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<AppUser>(
            $"SELECT * FROM {Table} WHERE LOWER(Username) = LOWER(@username)", new { username });
    }

    public async Task<AppUser> CreateAsync(AppUser user, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        user.Id = Guid.NewGuid().ToString("N");
        user.CreatedAt = DateTime.UtcNow;

        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"INSERT INTO {Table} (Id, Username, DisplayName, PasswordHash, AuthMethod, Role, IsActive, " +
            "AllowAllSystems, AllowedSystemsJson, AllowAllCategories, AllowedCategoriesJson, CreatedAt) " +
            "VALUES (@Id, @Username, @DisplayName, @PasswordHash, @AuthMethod, @Role, @IsActive, " +
            "@AllowAllSystems, @AllowedSystemsJson, @AllowAllCategories, @AllowedCategoriesJson, @CreatedAt)",
            user);
        _logger.LogInformation("User {Username} created ({AuthMethod}, role {Role})", user.Username, user.AuthMethod, user.Role);
        return user;
    }

    public async Task UpdateAsync(AppUser user, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"UPDATE {Table} SET DisplayName = @DisplayName, PasswordHash = @PasswordHash, " +
            "AuthMethod = @AuthMethod, Role = @Role, IsActive = @IsActive, AllowAllSystems = @AllowAllSystems, " +
            "AllowedSystemsJson = @AllowedSystemsJson, AllowAllCategories = @AllowAllCategories, " +
            "AllowedCategoriesJson = @AllowedCategoriesJson WHERE Id = @Id",
            user);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"DELETE FROM {Table} WHERE Id = @id", new { id });
    }

    public static UserInfo ToInfo(AppUser user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        DisplayName = user.DisplayName,
        AuthMethod = user.AuthMethod,
        Role = user.Role,
        IsActive = user.IsActive,
        AllowAllSystems = user.AllowAllSystems,
        AllowedSystems = Deserialize(user.AllowedSystemsJson),
        AllowAllCategories = user.AllowAllCategories,
        AllowedCategories = Deserialize(user.AllowedCategoriesJson),
        CreatedAt = user.CreatedAt,
    };

    private static List<string> Deserialize(string? json) =>
        string.IsNullOrWhiteSpace(json) ? new() : JsonSerializer.Deserialize<List<string>>(json) ?? new();
}
