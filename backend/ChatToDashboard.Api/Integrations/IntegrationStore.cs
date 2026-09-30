using ChatToDashboard.Api.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;

namespace ChatToDashboard.Api.Integrations;

/// <summary>
/// Persists External Integrations, their published-dashboard slots, and the publish audit log —
/// same SQLite/SqlServer dual-schema pattern as ConversationStore/HistoryStore. Admin-only
/// (enforced by IntegrationsController, not here — this store trusts its caller, same as every
/// other *Store in this app).
/// </summary>
public class IntegrationStore
{
    private readonly DataStore _db;

    public IntegrationStore(DataStore db) => _db = db;

    private string IntegrationsTable => _db.Provider == DbProvider.Sqlite
        ? "\"ExternalIntegrations\"" : "[staging].[ExternalIntegrations]";
    private string SlotsTable => _db.Provider == DbProvider.Sqlite
        ? "\"PublishedDashboardSlots\"" : "[staging].[PublishedDashboardSlots]";
    private string LogTable => _db.Provider == DbProvider.Sqlite
        ? "\"IntegrationPublishLog\"" : "[staging].[IntegrationPublishLog]";
    private string WidgetFiltersTable => _db.Provider == DbProvider.Sqlite
        ? "\"WidgetDataFilters\"" : "[staging].[WidgetDataFilters]";

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);

        var integrationsSql = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {IntegrationsTable} (
                 "Id" TEXT PRIMARY KEY, "Name" TEXT,
                 "ConnectorBaseUrl" TEXT, "ConnectorAuthHeader" TEXT, "ConnectorAuthValue" TEXT,
                 "IdentityMechanism" TEXT, "IdentityParameterName" TEXT,
                 "IdentityConfirmed" INTEGER, "IdentityConfirmedAt" TEXT, "IdentityConfirmedBy" TEXT,
                 "AccentColor" TEXT, "SecondaryColor" TEXT, "FontFamily" TEXT,
                 "CreatedBy" TEXT, "CreatedAt" TEXT, "UpdatedAt" TEXT)
               """
            : $"""
               IF OBJECT_ID('staging.ExternalIntegrations') IS NULL
               CREATE TABLE {IntegrationsTable} (
                 [Id] NVARCHAR(64) PRIMARY KEY, [Name] NVARCHAR(200),
                 [ConnectorBaseUrl] NVARCHAR(1000), [ConnectorAuthHeader] NVARCHAR(200), [ConnectorAuthValue] NVARCHAR(1000),
                 [IdentityMechanism] NVARCHAR(20), [IdentityParameterName] NVARCHAR(200),
                 [IdentityConfirmed] BIT, [IdentityConfirmedAt] DATETIME2, [IdentityConfirmedBy] NVARCHAR(200),
                 [AccentColor] NVARCHAR(20), [SecondaryColor] NVARCHAR(20), [FontFamily] NVARCHAR(200),
                 [CreatedBy] NVARCHAR(200), [CreatedAt] DATETIME2, [UpdatedAt] DATETIME2)
               """;
        await using (var cmd = connection.CreateCommand()) { cmd.CommandText = integrationsSql; await cmd.ExecuteNonQueryAsync(ct); }

        // Additive migrations for tables created before a given column existed — old
        // WriteApiUrl/ReadApiBaseUrl/DirectoryApiUrl/PermissionsApiUrl* columns (superseded by
        // the single ConnectorBaseUrl above, since the connector's route shape is fixed and
        // known) are simply left behind, unused, on any such table rather than dropped.
        foreach (var (column, sqliteType, sqlServerType) in new[]
                 {
                     ("ConnectorBaseUrl", "TEXT", "NVARCHAR(1000)"),
                     ("ConnectorAuthHeader", "TEXT", "NVARCHAR(200)"),
                     ("ConnectorAuthValue", "TEXT", "NVARCHAR(1000)"),
                     ("ClientDbProvider", "TEXT", "NVARCHAR(20)"),
                     ("ClientSchemaDescription", "TEXT", "NVARCHAR(MAX)"),
                     ("DataPermissionsAvailable", "INTEGER", "BIT"),
                 })
        {
            try
            {
                await using var alter = connection.CreateCommand();
                alter.CommandText = _db.Provider == DbProvider.Sqlite
                    ? $"ALTER TABLE {IntegrationsTable} ADD COLUMN \"{column}\" {sqliteType}"
                    : $"ALTER TABLE {IntegrationsTable} ADD [{column}] {sqlServerType}";
                await alter.ExecuteNonQueryAsync(ct);
            }
            catch (SqliteException ex) when (ex.Message.Contains("duplicate column", StringComparison.OrdinalIgnoreCase))
            {
            }
            catch (SqlException ex) when (ex.Number == 2705)
            {
            }
        }

        var slotsSql = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {SlotsTable} (
                 "Id" TEXT PRIMARY KEY, "IntegrationId" TEXT, "ExternalDashboardId" TEXT,
                 "LocalHistoryId" TEXT, "Title" TEXT,
                 "FirstPublishedAt" TEXT, "LastPublishedAt" TEXT, "LastPublishedBy" TEXT)
               """
            : $"""
               IF OBJECT_ID('staging.PublishedDashboardSlots') IS NULL
               CREATE TABLE {SlotsTable} (
                 [Id] NVARCHAR(64) PRIMARY KEY, [IntegrationId] NVARCHAR(64), [ExternalDashboardId] NVARCHAR(64),
                 [LocalHistoryId] NVARCHAR(64), [Title] NVARCHAR(500),
                 [FirstPublishedAt] DATETIME2, [LastPublishedAt] DATETIME2, [LastPublishedBy] NVARCHAR(200))
               """;
        await using (var cmd = connection.CreateCommand()) { cmd.CommandText = slotsSql; await cmd.ExecuteNonQueryAsync(ct); }

        var logSql = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {LogTable} (
                 "Id" TEXT PRIMARY KEY, "IntegrationId" TEXT, "ExternalDashboardId" TEXT,
                 "LocalHistoryId" TEXT, "DashboardTitle" TEXT, "UserId" TEXT,
                 "Success" INTEGER, "Error" TEXT, "CreatedAt" TEXT)
               """
            : $"""
               IF OBJECT_ID('staging.IntegrationPublishLog') IS NULL
               CREATE TABLE {LogTable} (
                 [Id] NVARCHAR(64) PRIMARY KEY, [IntegrationId] NVARCHAR(64), [ExternalDashboardId] NVARCHAR(64),
                 [LocalHistoryId] NVARCHAR(64), [DashboardTitle] NVARCHAR(500), [UserId] NVARCHAR(200),
                 [Success] BIT, [Error] NVARCHAR(MAX), [CreatedAt] DATETIME2)
               """;
        await using (var cmd = connection.CreateCommand()) { cmd.CommandText = logSql; await cmd.ExecuteNonQueryAsync(ct); }

        var widgetFiltersSql = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {WidgetFiltersTable} (
                 "IntegrationId" TEXT, "LocalHistoryId" TEXT, "WidgetIndex" INTEGER, "FilterKey" TEXT,
                 PRIMARY KEY ("IntegrationId", "LocalHistoryId", "WidgetIndex"))
               """
            : $"""
               IF OBJECT_ID('staging.WidgetDataFilters') IS NULL
               CREATE TABLE {WidgetFiltersTable} (
                 [IntegrationId] NVARCHAR(64), [LocalHistoryId] NVARCHAR(64), [WidgetIndex] INT, [FilterKey] NVARCHAR(100),
                 PRIMARY KEY ([IntegrationId], [LocalHistoryId], [WidgetIndex]))
               """;
        await using (var cmd = connection.CreateCommand()) { cmd.CommandText = widgetFiltersSql; await cmd.ExecuteNonQueryAsync(ct); }
    }

    // ---------- integrations ----------

    public async Task<ExternalIntegration> CreateAsync(string name, string createdBy, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        var now = DateTime.UtcNow;
        var integration = new ExternalIntegration
        {
            Id = Guid.NewGuid().ToString("N"), Name = name, CreatedBy = createdBy, CreatedAt = now, UpdatedAt = now,
        };
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"""
            INSERT INTO {IntegrationsTable}
              (Id, Name, IdentityConfirmed, CreatedBy, CreatedAt, UpdatedAt)
            VALUES (@Id, @Name, 0, @CreatedBy, @CreatedAt, @UpdatedAt)
            """, integration);
        return integration;
    }

    public async Task<IReadOnlyList<ExternalIntegration>> ListAsync(CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<ExternalIntegration>(
            $"SELECT * FROM {IntegrationsTable} ORDER BY CreatedAt DESC");
        return rows.ToList();
    }

    public async Task<ExternalIntegration?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<ExternalIntegration>(
            $"SELECT * FROM {IntegrationsTable} WHERE Id = @id", new { id });
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"DELETE FROM {IntegrationsTable} WHERE Id = @id", new { id });
        await connection.ExecuteAsync($"DELETE FROM {SlotsTable} WHERE IntegrationId = @id", new { id });
        await connection.ExecuteAsync($"DELETE FROM {LogTable} WHERE IntegrationId = @id", new { id });
    }

    public async Task UpdateApisAsync(string id, UpdateIntegrationApisRequest request, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        // ConnectorAuthValue is the one field here whose real stored value is NEVER echoed back
        // to the analyst (the form only ever shows a "••••••••" placeholder — see
        // renderIntegrationDetail) — unlike ConnectorBaseUrl/ConnectorAuthHeader, which the form
        // always shows in full, so a blank submission there is an explicit, visible choice to
        // clear it. A blank ConnectorAuthValue is never that: the analyst has no way to see it's
        // blank before submitting, so it only ever means "didn't mean to touch this" (e.g.
        // re-saving the base URL alone to retry schema discovery) — COALESCE keeps whatever
        // secret was already stored unless a real replacement value is actually sent.
        await connection.ExecuteAsync(
            $"""
            UPDATE {IntegrationsTable} SET
              ConnectorBaseUrl = @ConnectorBaseUrl, ConnectorAuthHeader = @ConnectorAuthHeader,
              ConnectorAuthValue = COALESCE(@ConnectorAuthValue, ConnectorAuthValue), UpdatedAt = @UpdatedAt
            WHERE Id = @Id
            """,
            new
            {
                Id = id, request.ConnectorBaseUrl, request.ConnectorAuthHeader, request.ConnectorAuthValue,
                UpdatedAt = DateTime.UtcNow,
            });
    }

    public async Task UpdateVisualIdentityAsync(
        string id, string accentColor, string secondaryColor, string fontFamily, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"UPDATE {IntegrationsTable} SET AccentColor = @accentColor, SecondaryColor = @secondaryColor, " +
            "FontFamily = @fontFamily, UpdatedAt = @updatedAt WHERE Id = @id",
            new { id, accentColor, secondaryColor, fontFamily, updatedAt = DateTime.UtcNow });
    }

    /// <summary>The client's own database shape — captured once so Publish can retarget each
    /// widget's query to run directly against it (see PublishService). Never touches the
    /// connection string itself; that stays local to the generated connector service, never
    /// given to us — see IntegrationDeliverables' connector project.
    /// <paramref name="dataPermissionsAvailable"/> is whether the discovered schema contains a
    /// table named DataPermissions.TableName — never set by hand, always derived from what was
    /// actually discovered (see ClientSchemaDiscoveryService).</summary>
    public async Task UpdateClientSchemaAsync(
        string id, string clientDbProvider, string clientSchemaDescription, bool dataPermissionsAvailable, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"UPDATE {IntegrationsTable} SET ClientDbProvider = @clientDbProvider, " +
            "ClientSchemaDescription = @clientSchemaDescription, DataPermissionsAvailable = @dataPermissionsAvailable, " +
            "UpdatedAt = @updatedAt WHERE Id = @id",
            new { id, clientDbProvider, clientSchemaDescription, dataPermissionsAvailable, updatedAt = DateTime.UtcNow });
    }

    /// <summary>Stores a matched-but-not-yet-confirmed mechanism/parameter pair. Never sets
    /// IdentityConfirmed — see ConfirmIdentityTransportAsync, a distinct, explicit step (Part C:
    /// "require explicit confirmation" before activation).</summary>
    public async Task SetIdentityTransportPendingAsync(
        string id, string mechanism, string parameterName, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"UPDATE {IntegrationsTable} SET IdentityMechanism = @mechanism, IdentityParameterName = @parameterName, " +
            "IdentityConfirmed = 0, IdentityConfirmedAt = NULL, IdentityConfirmedBy = NULL, UpdatedAt = @updatedAt WHERE Id = @id",
            new { id, mechanism, parameterName, updatedAt = DateTime.UtcNow });
    }

    public async Task ConfirmIdentityTransportAsync(string id, string confirmedBy, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var now = DateTime.UtcNow;
        await connection.ExecuteAsync(
            $"UPDATE {IntegrationsTable} SET IdentityConfirmed = 1, IdentityConfirmedAt = @now, " +
            "IdentityConfirmedBy = @confirmedBy, UpdatedAt = @now WHERE Id = @id",
            new { id, confirmedBy, now });
    }

    // ---------- published-dashboard slots ----------

    public async Task<IReadOnlyList<PublishedDashboardSlot>> ListSlotsAsync(string integrationId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<PublishedDashboardSlot>(
            $"SELECT * FROM {SlotsTable} WHERE IntegrationId = @integrationId ORDER BY LastPublishedAt DESC",
            new { integrationId });
        return rows.ToList();
    }

    public async Task<PublishedDashboardSlot?> GetSlotAsync(string integrationId, string slotId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<PublishedDashboardSlot>(
            $"SELECT * FROM {SlotsTable} WHERE Id = @slotId AND IntegrationId = @integrationId",
            new { integrationId, slotId });
    }

    /// <summary>The slot (if any) an Active dashboard already occupies under this integration —
    /// used by PublishService to offer "update the existing slot" as a default before falling
    /// back to "create a new one".</summary>
    public async Task<PublishedDashboardSlot?> FindSlotByLocalHistoryIdAsync(
        string integrationId, string localHistoryId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<PublishedDashboardSlot>(
            $"SELECT * FROM {SlotsTable} WHERE IntegrationId = @integrationId AND LocalHistoryId = @localHistoryId",
            new { integrationId, localHistoryId });
    }

    public async Task<PublishedDashboardSlot> CreateSlotAsync(
        string integrationId, string localHistoryId, string title, string publishedBy, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        var now = DateTime.UtcNow;
        var slot = new PublishedDashboardSlot
        {
            Id = Guid.NewGuid().ToString("N"), IntegrationId = integrationId,
            ExternalDashboardId = Guid.NewGuid().ToString("N"), LocalHistoryId = localHistoryId, Title = title,
            FirstPublishedAt = now, LastPublishedAt = now, LastPublishedBy = publishedBy,
        };
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"""
            INSERT INTO {SlotsTable}
              (Id, IntegrationId, ExternalDashboardId, LocalHistoryId, Title, FirstPublishedAt, LastPublishedAt, LastPublishedBy)
            VALUES (@Id, @IntegrationId, @ExternalDashboardId, @LocalHistoryId, @Title, @FirstPublishedAt, @LastPublishedAt, @LastPublishedBy)
            """, slot);
        return slot;
    }

    public async Task TouchSlotAsync(string slotId, string title, string publishedBy, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"UPDATE {SlotsTable} SET Title = @title, LastPublishedAt = @now, LastPublishedBy = @publishedBy WHERE Id = @slotId",
            new { slotId, title, publishedBy, now = DateTime.UtcNow });
    }

    // ---------- publish audit log ----------

    public async Task LogPublishAsync(IntegrationPublishLogEntry entry, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        entry.Id = string.IsNullOrWhiteSpace(entry.Id) ? Guid.NewGuid().ToString("N") : entry.Id;
        entry.CreatedAt = entry.CreatedAt == default ? DateTime.UtcNow : entry.CreatedAt;
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"""
            INSERT INTO {LogTable}
              (Id, IntegrationId, ExternalDashboardId, LocalHistoryId, DashboardTitle, UserId, Success, Error, CreatedAt)
            VALUES (@Id, @IntegrationId, @ExternalDashboardId, @LocalHistoryId, @DashboardTitle, @UserId, @Success, @Error, @CreatedAt)
            """, entry);
    }

    public async Task<IReadOnlyList<IntegrationPublishLogEntry>> ListPublishLogAsync(
        string integrationId, int limit = 100, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var top = _db.Provider == DbProvider.Sqlite ? "" : $"TOP {limit} ";
        var tail = _db.Provider == DbProvider.Sqlite ? $" LIMIT {limit}" : "";
        var rows = await connection.QueryAsync<IntegrationPublishLogEntry>(
            $"SELECT {top}* FROM {LogTable} WHERE IntegrationId = @integrationId ORDER BY CreatedAt DESC{tail}",
            new { integrationId });
        return rows.ToList();
    }

    // ---------- widget-level data-permission marks (Part E, data-level extension) ----------

    public async Task<IReadOnlyList<WidgetDataFilter>> GetWidgetDataFiltersAsync(
        string integrationId, string localHistoryId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<WidgetDataFilter>(
            $"SELECT * FROM {WidgetFiltersTable} WHERE IntegrationId = @integrationId AND LocalHistoryId = @localHistoryId",
            new { integrationId, localHistoryId });
        return rows.ToList();
    }

    /// <summary>Replaces the complete set of marks for this (integration, dashboard) pair —
    /// <paramref name="filters"/> is the full desired set, not a delta; an empty list clears
    /// everything.</summary>
    public async Task SetWidgetDataFiltersAsync(
        string integrationId, string localHistoryId, IReadOnlyList<WidgetDataFilterEntry> filters, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"DELETE FROM {WidgetFiltersTable} WHERE IntegrationId = @integrationId AND LocalHistoryId = @localHistoryId",
            new { integrationId, localHistoryId });
        if (filters.Count == 0) return;
        await connection.ExecuteAsync(
            $"INSERT INTO {WidgetFiltersTable} (IntegrationId, LocalHistoryId, WidgetIndex, FilterKey) " +
            "VALUES (@IntegrationId, @LocalHistoryId, @WidgetIndex, @FilterKey)",
            filters.Select(f => new { IntegrationId = integrationId, LocalHistoryId = localHistoryId, f.WidgetIndex, f.FilterKey }));
    }
}
