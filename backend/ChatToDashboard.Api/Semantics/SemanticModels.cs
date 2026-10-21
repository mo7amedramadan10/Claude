using System.Text.Json.Serialization;

namespace ChatToDashboard.Api.Semantics;

/// <summary>جسم <c>PUT /api/repository/files/{id}/semantics/{column}</c>.</summary>
public class SetColumnSemanticRequest
{
    [JsonPropertyName("concept")] public string? Concept { get; set; }
    [JsonPropertyName("role")] public string? Role { get; set; }
    [JsonPropertyName("unit")] public string? Unit { get; set; }
    [JsonPropertyName("valueMap")] public Dictionary<string, string>? ValueMap { get; set; }
}
