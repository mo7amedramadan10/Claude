using System.Text.Json.Serialization;

namespace ChatToDashboard.Api.Repository;

/// <summary>A file saved in the repository, as returned to the UI.</summary>
public class RepositoryFile
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;

    /// <summary>The uploader-chosen, human-readable name — what's shown everywhere in the
    /// UI and in chat/dashboard references. Required, distinct from the actual uploaded
    /// filename (see <see cref="OriginalFileName"/>).</summary>
    [JsonPropertyName("displayName")] public string DisplayName { get; set; } = string.Empty;

    /// <summary>The file's real uploaded filename — kept for reference only (small/secondary
    /// in the UI). Reflects the most recent upload if the file was later updated via
    /// "Update this file", so it always names the data actually behind the table today.</summary>
    [JsonPropertyName("originalFileName")] public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>Short free-text description of what's in the file and why it matters.</summary>
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;

    [JsonPropertyName("category")] public string Category { get; set; } = string.Empty;

    /// <summary>"excel", "csv" or "pdf" — drives the icon and the meta line in the UI.</summary>
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("rowCount")] public int RowCount { get; set; }
    [JsonPropertyName("columnCount")] public int ColumnCount { get; set; }
    [JsonPropertyName("pageCount")] public int PageCount { get; set; }

    /// <summary>When this file identity was first created — never changes after that.</summary>
    [JsonPropertyName("uploadedAt")] public DateTime UploadedAt { get; set; }

    /// <summary>When the underlying data was last replaced via "Update this file" — equals
    /// <see cref="UploadedAt"/> until the first update.</summary>
    [JsonPropertyName("lastUpdatedAt")] public DateTime LastUpdatedAt { get; set; }

    /// <summary>Queryable table holding the rows, for tabular files; null for PDFs. Stable
    /// across "Update this file" — this is what keeps saved dashboards working after a
    /// data refresh, since every widget's stored query still names the same table.</summary>
    [JsonPropertyName("tableName")] public string? TableName { get; set; }

    /// <summary>Set the moment an update changes the column set (added/removed/renamed) —
    /// null if the shape has never changed since upload. A dashboard saved before this
    /// timestamp may depend on a column that no longer exists.</summary>
    [JsonPropertyName("schemaChangedAt")] public DateTime? SchemaChangedAt { get; set; }

    /// <summary>The account that created this file identity (via "Upload new file").</summary>
    [JsonPropertyName("createdByUserId")] public string CreatedByUserId { get; set; } = string.Empty;

    /// <summary>Named users explicitly allowed to use this file as a data source, on top of
    /// their existing category access — empty means no per-file narrowing is configured yet
    /// (category-level gating alone still applies, same as before this feature existed).</summary>
    [JsonPropertyName("permittedUserIds")] public List<string> PermittedUserIds { get; set; } = new();

    /// <summary>Other files manually declared as related to this one.</summary>
    [JsonPropertyName("relationships")] public List<FileRelationship> Relationships { get; set; } = new();

    /// <summary>How many Draft (History) dashboards currently reference this file's table.</summary>
    [JsonPropertyName("usageCount")] public int UsageCount { get; set; }

    /// <summary>When the most recent Draft dashboard referencing this file was saved — null
    /// if it has never been used in one. Drives the "not used in N months" hint.</summary>
    [JsonPropertyName("lastUsedAt")] public DateTime? LastUsedAt { get; set; }
}

/// <summary>A manually declared link between two repository files sharing a column.</summary>
public class FileRelationship
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("relatedFileId")] public string RelatedFileId { get; set; } = string.Empty;
    [JsonPropertyName("relatedFileName")] public string RelatedFileName { get; set; } = string.Empty;
    [JsonPropertyName("sharedColumn")] public string SharedColumn { get; set; } = string.Empty;
}

/// <summary>A parsed upload waiting for the user to assign it a category (and, for a new
/// file, a display name + description).</summary>
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

/// <summary>Body of POST /api/repository/files — "Upload new file": creates a brand new
/// file identity, unrelated to any existing one.</summary>
public class SaveUploadRequest
{
    [JsonPropertyName("token")] public string Token { get; set; } = string.Empty;
    [JsonPropertyName("displayName")] public string DisplayName { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
    [JsonPropertyName("category")] public string Category { get; set; } = string.Empty;
}

/// <summary>Body of POST /api/repository/files/{id}/update — "Update this file": replaces
/// the data behind an existing identity. Display name, category, relationships and
/// permissions are untouched; only the data, OriginalFileName and LastUpdatedAt change.</summary>
public class UpdateFileDataRequest
{
    [JsonPropertyName("token")] public string Token { get; set; } = string.Empty;
}

/// <summary>Body of PUT /api/repository/files/{id}/meta — editing display name/description
/// without touching the underlying data.</summary>
public class UpdateFileMetaRequest
{
    [JsonPropertyName("displayName")] public string DisplayName { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
    [JsonPropertyName("category")] public string Category { get; set; } = string.Empty;
}

/// <summary>Body of PUT /api/repository/files/{id}/permissions.</summary>
public class UpdateFilePermissionsRequest
{
    [JsonPropertyName("userIds")] public List<string> UserIds { get; set; } = new();
}

/// <summary>Body of POST /api/repository/files/{id}/relationships.</summary>
public class AddFileRelationshipRequest
{
    [JsonPropertyName("relatedFileId")] public string RelatedFileId { get; set; } = string.Empty;
    [JsonPropertyName("sharedColumn")] public string SharedColumn { get; set; } = string.Empty;
}
