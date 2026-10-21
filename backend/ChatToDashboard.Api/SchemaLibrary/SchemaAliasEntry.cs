namespace ChatToDashboard.Api.SchemaLibrary;

public static class SchemaConceptKinds
{
    public const string Table = "table";
    public const string Column = "column";
}

public static class SchemaAliasSources
{
    /// <summary>Mechanically generated at startup from kpi-library.js's own draft SQL — the
    /// generated name itself plus a few deterministic casing/prefix variants (see
    /// SchemaAliasStore.SeedIfEmptyAsync). Never hand-curated.</summary>
    public const string Seed = "seed";

    /// <summary>Written at runtime, the moment a real build against a real client's schema
    /// resolved a concept to a name not already on file — see
    /// SchemaMatchingService.RecordLearnedMappingAsync.</summary>
    public const string Learned = "learned";
}

/// <summary>
/// One candidate real-world name for one concept (a table, or a column scoped to a table) —
/// see the explainer thread that designed this: a concept (e.g. "تاريخ الطلب" under the
/// "sales" table concept) can be expressed in a real client's schema under several different
/// physical names (order_date, OrderDate, order_dt, created_at...), each a separate row here
/// with its own Score. SchemaMatchingService reads these, ordered by Score, to try against a
/// client's actual discovered schema before ever asking the model to guess.
/// </summary>
public class SchemaAliasEntry
{
    public string Id { get; set; } = "";

    /// <summary>Stable id for the concept this row is a candidate name for — a lowercased table
    /// name (table concepts) or "table.column" (column concepts); see
    /// SchemaConceptExtractor.TableConceptId/ColumnConceptId.</summary>
    public string ConceptId { get; set; } = "";

    /// <summary>"table" or "column" — see SchemaConceptKinds.</summary>
    public string Kind { get; set; } = SchemaConceptKinds.Table;

    /// <summary>The owning table's concept id — null for a table concept itself, set for every
    /// column concept (a column's identity is only ever meaningful scoped to its table).</summary>
    public string? ParentTable { get; set; }

    /// <summary>Admin-facing display label for the concept (not the candidate name itself) —
    /// editable from the admin screen; every row sharing a ConceptId should carry the same
    /// label, but it's stored per-row rather than normalized out since this table has no
    /// separate "concepts" table of its own (kept deliberately simple — see the design thread).</summary>
    public string Label { get; set; } = "";

    /// <summary>One candidate physical name — what might actually appear in a real client's
    /// database for this concept.</summary>
    public string Name { get; set; } = "";

    public double Score { get; set; }

    /// <summary>"seed" or "learned" — see SchemaAliasSources.</summary>
    public string Source { get; set; } = SchemaAliasSources.Seed;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
