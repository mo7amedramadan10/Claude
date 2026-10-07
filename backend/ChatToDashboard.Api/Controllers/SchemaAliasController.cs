using System.Text.Json.Serialization;
using ChatToDashboard.Api.SchemaLibrary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>Admin screen for the field/table alias library ("مكتبة الحقول والجداول" under
/// "مكتبة النماذج") — platform-owner only, same as every other template-library admin surface
/// (see TemplatesController). Plain CRUD over SchemaAliasStore; no built-in/override split like
/// TemplateStore's other kinds, since every row here — seed or learned — is itself a real row,
/// not a default something can "revert to" (see the design conversation on why this table
/// doesn't mirror kpi-library.js's static-file + override shape).</summary>
[ApiController]
[Route("api/schema-aliases")]
[Authorize(Policy = "PlatformOwner")]
public class SchemaAliasController : ControllerBase
{
    private readonly SchemaAliasStore _store;

    public SchemaAliasController(SchemaAliasStore store) => _store = store;

    public class SchemaAliasRow
    {
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        [JsonPropertyName("conceptId")] public string ConceptId { get; set; } = "";
        [JsonPropertyName("kind")] public string Kind { get; set; } = "";
        [JsonPropertyName("parentTable")] public string? ParentTable { get; set; }
        [JsonPropertyName("label")] public string Label { get; set; } = "";
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        [JsonPropertyName("score")] public double Score { get; set; }
        [JsonPropertyName("source")] public string Source { get; set; } = "";
        [JsonPropertyName("updatedAt")] public DateTime UpdatedAt { get; set; }
    }

    private static SchemaAliasRow ToRow(SchemaAliasEntry e) => new()
    {
        Id = e.Id, ConceptId = e.ConceptId, Kind = e.Kind, ParentTable = e.ParentTable,
        Label = e.Label, Name = e.Name, Score = e.Score, Source = e.Source, UpdatedAt = e.UpdatedAt,
    };

    /// <summary>Every row, flat — the admin screen groups them by ConceptId client-side (same
    /// shape as the KPI-library admin pane already does with kpi-library.js's own rows).</summary>
    [HttpGet]
    public async Task<ActionResult<List<SchemaAliasRow>>> List(CancellationToken ct)
    {
        var rows = await _store.ListAllAsync(ct);
        return rows.Select(ToRow).ToList();
    }

    public class SaveAliasRequest
    {
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        [JsonPropertyName("score")] public double Score { get; set; }
        [JsonPropertyName("label")] public string Label { get; set; } = "";
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] SaveAliasRequest body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.Name)) return BadRequest(new { error = "الاسم مطلوب." });
        if (body.Score is < 0 or > 1) return BadRequest(new { error = "الدرجة (score) لازم تكون بين 0 و1." });
        var existing = await _store.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        await _store.UpdateAsync(id, body.Name.Trim(), body.Score, body.Label.Trim(), ct);
        return Ok();
    }

    public class AddAliasRequest
    {
        [JsonPropertyName("conceptId")] public string ConceptId { get; set; } = "";
        [JsonPropertyName("kind")] public string Kind { get; set; } = "";
        [JsonPropertyName("parentTable")] public string? ParentTable { get; set; }
        [JsonPropertyName("label")] public string Label { get; set; } = "";
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        [JsonPropertyName("score")] public double Score { get; set; } = 0.5;
    }

    /// <summary>Admin-authored candidate name — the manual counterpart to a model-learned one,
    /// for when an admin already knows a real client's column name and wants it on file ahead of
    /// the next build rather than waiting for the model to discover it live.</summary>
    [HttpPost]
    public async Task<ActionResult<SchemaAliasRow>> Add([FromBody] AddAliasRequest body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.ConceptId) || string.IsNullOrWhiteSpace(body.Name))
            return BadRequest(new { error = "conceptId والاسم مطلوبين." });
        if (body.Kind != SchemaConceptKinds.Table && body.Kind != SchemaConceptKinds.Column)
            return BadRequest(new { error = "kind لازم يكون table أو column." });
        if (body.Kind == SchemaConceptKinds.Column && string.IsNullOrWhiteSpace(body.ParentTable))
            return BadRequest(new { error = "مفهوم العمود لازم يكون له parentTable." });
        if (body.Score is < 0 or > 1) return BadRequest(new { error = "الدرجة (score) لازم تكون بين 0 و1." });

        var row = await _store.AddAsync(new SchemaAliasEntry
        {
            ConceptId = body.ConceptId.Trim(),
            Kind = body.Kind,
            ParentTable = body.ParentTable?.Trim(),
            Label = body.Label.Trim(),
            Name = body.Name.Trim(),
            Score = body.Score,
            Source = SchemaAliasSources.Seed, // admin-authored counts as curated, same tier as the seed
        }, ct);
        return ToRow(row);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        await _store.DeleteAsync(id, ct);
        return Ok();
    }
}
