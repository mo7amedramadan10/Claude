using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.SchemaLibrary;

/// <summary>
/// The field/table alias library — one row per (concept, candidate real-world name). A single
/// table holds both the mechanically-generated seed (Source = "seed", written once at startup
/// if the table is empty — see SeedIfEmptyAsync) and whatever's been learned since from real
/// builds against real client schemas (Source = "learned", written by RecordLearnedAsync as
/// ordinary application traffic happens — no seed/override split, no second file: see the
/// design conversation that settled on this over a static generated JS asset, specifically
/// because — unlike kpi-library.js — this data has to be read by backend C# code on every
/// template build, not just rendered in an admin screen).
/// </summary>
public class SchemaAliasStore
{
    private readonly DataStore _db;

    public SchemaAliasStore(DataStore db) => _db = db;

    private string Table => _db.Provider == DbProvider.Sqlite ? "\"SchemaAliasEntries\"" : "[staging].[SchemaAliasEntries]";

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);

        var text = _db.Provider == DbProvider.Sqlite
            ? $"""
               CREATE TABLE IF NOT EXISTS {Table} (
                 "Id" TEXT PRIMARY KEY, "ConceptId" TEXT, "Kind" TEXT, "ParentTable" TEXT, "Label" TEXT,
                 "Name" TEXT, "Score" REAL, "Source" TEXT, "CreatedAt" TEXT, "UpdatedAt" TEXT)
               """
            : $"""
               IF OBJECT_ID('staging.SchemaAliasEntries') IS NULL
               CREATE TABLE {Table} (
                 [Id] NVARCHAR(200) PRIMARY KEY, [ConceptId] NVARCHAR(400), [Kind] NVARCHAR(16), [ParentTable] NVARCHAR(200),
                 [Label] NVARCHAR(400), [Name] NVARCHAR(200), [Score] FLOAT, [Source] NVARCHAR(16),
                 [CreatedAt] DATETIME2, [UpdatedAt] DATETIME2)
               """;

        await using var command = connection.CreateCommand();
        command.CommandText = text;
        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task<bool> HasAnyAsync(CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var count = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(1) FROM {Table}");
        return count > 0;
    }

    /// <summary>Called once at startup (see Program.cs) when the table is still empty — bulk
    /// inserts the mechanically-generated seed rows. A no-op on every later startup once the
    /// table has anything in it, seed or learned, so this never re-runs against a library an
    /// admin has already started curating/pruning.</summary>
    public async Task SeedIfEmptyAsync(IReadOnlyList<SchemaAliasEntry> rows, CancellationToken ct = default)
    {
        if (rows.Count == 0 || await HasAnyAsync(ct)) return;

        await using var connection = await _db.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        foreach (var row in rows)
        {
            row.Id = Guid.NewGuid().ToString("N");
            await connection.ExecuteAsync($"""
                INSERT INTO {Table} (Id, ConceptId, Kind, ParentTable, Label, Name, Score, Source, CreatedAt, UpdatedAt)
                VALUES (@Id, @ConceptId, @Kind, @ParentTable, @Label, @Name, @Score, @Source, @CreatedAt, @UpdatedAt)
                """, row, transaction);
        }
        await transaction.CommitAsync(ct);
    }

    /// <summary>Every candidate name on file for one concept, highest score first — what
    /// SchemaMatchingService tries, in order, against a client's real discovered schema.</summary>
    public async Task<IReadOnlyList<SchemaAliasEntry>> GetCandidatesAsync(string conceptId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<SchemaAliasEntry>(
            $"SELECT * FROM {Table} WHERE ConceptId = @conceptId ORDER BY Score DESC", new { conceptId });
        return rows.ToList();
    }

    /// <summary>Every column concept known under a given table concept — so once a widget's
    /// table concept is resolved, the matcher knows which column concepts to try next without
    /// the caller having to already know them.</summary>
    public async Task<IReadOnlyList<string>> GetColumnConceptIdsForTableAsync(string tableConceptId, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<string>(
            $"SELECT DISTINCT ConceptId FROM {Table} WHERE Kind = @kind AND ParentTable = @table",
            new { kind = SchemaConceptKinds.Column, table = tableConceptId });
        return rows.ToList();
    }

    /// <summary>Records a real-world name a build just resolved a concept to. An exact
    /// (ConceptId, Name) match already on file is reinforced (score nudged up, capped at 1) —
    /// real usage confirming an existing guess. A genuinely new name is inserted at a modest
    /// starting score (see StartingLearnedScore) — present for next time, but not yet trusted
    /// over an established alias. Creates the concept's Label from <paramref name="label"/> only
    /// when inserting; an existing concept keeps whatever label an admin may have already set.</summary>
    public async Task RecordLearnedAsync(
        string conceptId, string kind, string? parentTable, string label, string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);

        var existing = await connection.QuerySingleOrDefaultAsync<SchemaAliasEntry>(
            $"SELECT * FROM {Table} WHERE ConceptId = @conceptId AND Name = @name COLLATE NOCASE",
            new { conceptId, name });
        // SQL Server has no COLLATE NOCASE — fall back to an in-memory case-insensitive check
        // there (the table is small enough per concept that this never matters for performance).
        if (existing is null && _db.Provider != DbProvider.Sqlite)
        {
            var all = await connection.QueryAsync<SchemaAliasEntry>($"SELECT * FROM {Table} WHERE ConceptId = @conceptId", new { conceptId });
            existing = all.FirstOrDefault(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        if (existing is not null)
        {
            var newScore = Math.Min(1.0, existing.Score + LearnedReinforceDelta);
            await connection.ExecuteAsync(
                $"UPDATE {Table} SET Score = @score, UpdatedAt = @now WHERE Id = @id",
                new { score = newScore, now = DateTime.UtcNow, id = existing.Id });
            return;
        }

        var row = new SchemaAliasEntry
        {
            Id = Guid.NewGuid().ToString("N"),
            ConceptId = conceptId,
            Kind = kind,
            ParentTable = parentTable,
            Label = label,
            Name = name,
            Score = StartingLearnedScore,
            Source = SchemaAliasSources.Learned,
        };
        await connection.ExecuteAsync($"""
            INSERT INTO {Table} (Id, ConceptId, Kind, ParentTable, Label, Name, Score, Source, CreatedAt, UpdatedAt)
            VALUES (@Id, @ConceptId, @Kind, @ParentTable, @Label, @Name, @Score, @Source, @CreatedAt, @UpdatedAt)
            """, row);
    }

    public const double StartingLearnedScore = 0.3;
    private const double LearnedReinforceDelta = 0.1;

    // ---- Admin screen ----

    /// <summary>Every row, for the admin "مكتبة الحقول والجداول" screen — grouped by concept
    /// client-side (same shape as kpi-library.js's own admin pane), so this stays one simple
    /// SELECT * rather than a bespoke grouped query.</summary>
    public async Task<IReadOnlyList<SchemaAliasEntry>> ListAllAsync(CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<SchemaAliasEntry>($"SELECT * FROM {Table} ORDER BY ConceptId, Score DESC");
        return rows.ToList();
    }

    public async Task<SchemaAliasEntry?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<SchemaAliasEntry>($"SELECT * FROM {Table} WHERE Id = @id", new { id });
    }

    /// <summary>Admin edit — name/score/label, never ConceptId/Kind/ParentTable/Source (those
    /// define what the row IS; changing them is "delete and add a new one", not an edit).</summary>
    public async Task UpdateAsync(string id, string name, double score, string label, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync(
            $"UPDATE {Table} SET Name = @name, Score = @score, Label = @label, UpdatedAt = @now WHERE Id = @id",
            new { name, score, label, now = DateTime.UtcNow, id });
    }

    /// <summary>Adds one admin-authored candidate name directly (the manual counterpart to
    /// RecordLearnedAsync) — e.g. an admin who already knows a client's real column name wants
    /// it on file before the next build rather than waiting for the model to discover it.</summary>
    public async Task<SchemaAliasEntry> AddAsync(SchemaAliasEntry row, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        row.Id = Guid.NewGuid().ToString("N");
        row.CreatedAt = row.UpdatedAt = DateTime.UtcNow;
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"""
            INSERT INTO {Table} (Id, ConceptId, Kind, ParentTable, Label, Name, Score, Source, CreatedAt, UpdatedAt)
            VALUES (@Id, @ConceptId, @Kind, @ParentTable, @Label, @Name, @Score, @Source, @CreatedAt, @UpdatedAt)
            """, row);
        return row;
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await using var connection = await _db.OpenConnectionAsync(ct);
        await connection.ExecuteAsync($"DELETE FROM {Table} WHERE Id = @id", new { id });
    }
}
