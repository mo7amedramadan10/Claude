using System.Text.Json.Serialization;

namespace ChatToDashboard.Api.Repository;

/// <summary>A file saved in the repository, as returned to the UI.</summary>
public class RepositoryFile
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("category")] public string Category { get; set; } = string.Empty;

    /// <summary>"excel", "csv" or "pdf" — drives the icon and the meta line in the UI.</summary>
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("rowCount")] public int RowCount { get; set; }
    [JsonPropertyName("columnCount")] public int ColumnCount { get; set; }
    [JsonPropertyName("pageCount")] public int PageCount { get; set; }
    [JsonPropertyName("uploadedAt")] public DateTime UploadedAt { get; set; }

    /// <summary>Queryable table holding the rows, for tabular files; null for PDFs.</summary>
    [JsonPropertyName("tableName")] public string? TableName { get; set; }
}

/// <summary>A parsed upload waiting for the user to assign it a category.</summary>
public class PendingUpload
{
    [JsonPropertyName("token")] public string Token { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;
    [JsonPropertyName("rowCount")] public int RowCount { get; set; }
    [JsonPropertyName("columnCount")] public int ColumnCount { get; set; }
    [JsonPropertyName("pageCount")] public int PageCount { get; set; }
    [JsonPropertyName("error")] public string? Error { get; set; }
}

public class SaveUploadRequest
{
    [JsonPropertyName("token")] public string Token { get; set; } = string.Empty;
    [JsonPropertyName("category")] public string Category { get; set; } = string.Empty;
}
