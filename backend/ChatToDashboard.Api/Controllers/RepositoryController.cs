using ChatToDashboard.Api.Repository;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

[ApiController]
[Route("api/repository")]
public class RepositoryController : ControllerBase
{
    private const long MaxUploadBytes = 50 * 1024 * 1024;

    private readonly RepositoryStore _store;
    private readonly UploadParser _parser;
    private readonly PermissionsService _permissions;
    private readonly UserStore _users;
    private readonly ILogger<RepositoryController> _logger;

    public RepositoryController(
        RepositoryStore store, UploadParser parser, PermissionsService permissions, UserStore users, ILogger<RepositoryController> logger)
    {
        _store = store;
        _parser = parser;
        _permissions = permissions;
        _users = users;
        _logger = logger;
    }

    /// <summary>
    /// Every file an Admin has, but for anyone else only the ones they can actually reach:
    /// the same two-layer rule DescribeSourcesAsync already applies before a query runs
    /// (the user's own file/category permission, AND — independently — a file with a
    /// recorded creator being auto-restricted to that creator/an Admin/whoever's explicitly
    /// granted). Actually *using* a file was always gated this way (download, query_data);
    /// this makes the file even being listed follow the same rule, so a user with no access
    /// to it never sees it exists at all, rather than a "🔒 مقيّد" entry they can't open.
    /// </summary>
    [HttpGet("files")]
    public async Task<IActionResult> Files(CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();

        var files = await _store.ListAsync(ct);
        if (user.Role == UserRoles.Admin) return Ok(files);

        var selection = PermissionsService.GetEffectiveSelection(user, null);
        var visible = files.Where(f =>
            selection.AllowsFile(f.Id) &&
            (string.IsNullOrWhiteSpace(f.CreatedByUserId)
                || string.Equals(f.CreatedByUserId, user.Id, StringComparison.OrdinalIgnoreCase)
                || f.PermittedUserIds.Contains(user.Id, StringComparer.OrdinalIgnoreCase)))
            .ToList();
        return Ok(visible);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Categories(CancellationToken ct) => Ok(await _store.ListCategoriesAsync(ct));

    /// <summary>
    /// Parses uploaded files on the server and returns them as "pending" — they are only
    /// stored once the user assigns each one a category (and, for a new file, a display
    /// name/description) via POST files, or points them at an existing file's identity via
    /// POST files/{id}/update.
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<IActionResult> Upload([FromForm] IFormFileCollection files, CancellationToken ct)
    {
        if (files is null || files.Count == 0)
            return BadRequest(new { error = "No files were uploaded." });

        var results = new List<PendingUpload>();
        foreach (var file in files)
        {
            if (!UploadParser.IsSupported(file.FileName))
            {
                results.Add(new PendingUpload
                {
                    Name = file.FileName,
                    Kind = "unknown",
                    Error = "نوع الملف غير مدعوم. المدعوم: xlsx و xls و csv و pdf.",
                });
                continue;
            }

            using var stream = file.OpenReadStream();
            results.Add(await _parser.ParseAsync(file.FileName, stream, ct));
        }
        return Ok(results);
    }

    /// <summary>"Upload new file" — a completely new file identity, unrelated to any existing one.</summary>
    [HttpPost("files")]
    public async Task<IActionResult> Save([FromBody] SaveUploadRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return BadRequest(new { error = "token is required." });
        if (string.IsNullOrWhiteSpace(request.DisplayName))
            return BadRequest(new { error = "displayName is required." });
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        if (!_parser.TryTake(request.Token, out var parsed))
            return NotFound(new { error = "انتهت صلاحية الملف المرفوع. ارفعه من جديد." });

        try
        {
            var saved = await _store.SaveAsync(parsed, request.DisplayName, request.Description, request.Category, user.Id, ct);
            return Ok(saved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save {File} to the repository", parsed.FileName);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// "Update this file" — re-uploads data into an existing file's identity. Display name,
    /// category, relationships and permissions are preserved; only the data and "last
    /// updated" change, and the queryable table identity never changes.
    /// </summary>
    [HttpPost("files/{id}/update")]
    public async Task<IActionResult> UpdateData(string id, [FromBody] UpdateFileDataRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return BadRequest(new { error = "token is required." });
        if (!_parser.TryTake(request.Token, out var parsed))
            return NotFound(new { error = "انتهت صلاحية الملف المرفوع. ارفعه من جديد." });

        try
        {
            var updated = await _store.UpdateDataAsync(id, parsed, ct);
            return updated is null ? NotFound(new { error = "الملف غير موجود." }) : Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update data for repository file {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// The original file exactly as uploaded (or as of the last "Update this file"), for the
    /// user to download. Gated the same way actually *using* this file as a data source
    /// already is — creator, an Admin, or someone explicitly granted (see
    /// AnalyticsTools.DescribeSourcesAsync's RestrictedFileTables) — since a raw export is at
    /// least as sensitive as a query result. Merely seeing the file listed in "مستودع
    /// الملفات" does not itself grant this, same as it doesn't grant querying it.
    /// </summary>
    [HttpGet("files/{id}/download")]
    public async Task<IActionResult> Download(string id, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();

        var createdByUserId = await _store.GetCreatedByUserIdAsync(id, ct);
        if (createdByUserId is null) return NotFound(new { error = "الملف غير موجود." });

        if (user.Role != UserRoles.Admin && !string.Equals(createdByUserId, user.Id, StringComparison.OrdinalIgnoreCase))
        {
            var granted = await _store.GetPermittedUserIdsAsync(id, ct);
            if (!granted.Contains(user.Id, StringComparer.OrdinalIgnoreCase))
                return Forbid();
        }

        var file = await _store.GetFileContentAsync(id, ct);
        if (file is null)
            return NotFound(new { error = "الملف الأصلي غير محفوظ (رُفع قبل إتاحة التنزيل) — أعد رفعه عبر «تحديث البيانات»." });

        var (content, originalFileName) = file.Value;
        var downloadName = string.IsNullOrWhiteSpace(originalFileName) ? $"{id}.bin" : originalFileName;
        return File(content, ContentTypeFor(downloadName), downloadName);
    }

    private static string ContentTypeFor(string fileName) => Path.GetExtension(fileName).ToLowerInvariant() switch
    {
        ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        ".xls" => "application/vnd.ms-excel",
        ".csv" => "text/csv",
        ".pdf" => "application/pdf",
        _ => "application/octet-stream",
    };

    /// <summary>Edits display name/description/category without touching the data.</summary>
    [HttpPut("files/{id}/meta")]
    public async Task<IActionResult> UpdateMeta(string id, [FromBody] UpdateFileMetaRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName))
            return BadRequest(new { error = "displayName is required." });
        var ok = await _store.UpdateMetaAsync(id, request.DisplayName, request.Description, request.Category, ct);
        return ok ? NoContent() : NotFound(new { error = "الملف غير موجود." });
    }

    /// <summary>The file's creator (its automatic, un-removable access) plus every named
    /// user explicitly granted on top of that — names resolved here (not left to the
    /// frontend) so a non-Admin creator, who can't call the Admin-only GET /api/users to
    /// build their own id->name map, still sees real names for the people already granted.</summary>
    [HttpGet("files/{id}/permissions")]
    public async Task<IActionResult> GetPermissions(string id, CancellationToken ct)
    {
        var createdByUserId = await _store.GetCreatedByUserIdAsync(id, ct);
        if (createdByUserId is null) return NotFound(new { error = "الملف غير موجود." });

        var creator = string.IsNullOrWhiteSpace(createdByUserId) ? null : await _users.FindByIdAsync(createdByUserId, ct);
        var grantedIds = await _store.GetPermittedUserIdsAsync(id, ct);
        var granted = new List<object>();
        foreach (var userId in grantedIds)
        {
            var u = await _users.FindByIdAsync(userId, ct);
            granted.Add(new { userId, displayName = u?.DisplayName ?? u?.Username ?? userId, username = u?.Username });
        }
        return Ok(new
        {
            creatorId = createdByUserId,
            creatorName = creator?.DisplayName ?? creator?.Username,
            granted,
        });
    }

    /// <summary>
    /// Grants named users access to this file on top of its automatic, un-removable
    /// creator-only restriction (see RepositoryModels.cs) — the file's own creator, or an
    /// Admin standing in for them. request.UserIds is the *additional* grant list; the
    /// creator's own access never needs to be (and can't be) listed here to keep it.
    /// </summary>
    [HttpPut("files/{id}/permissions")]
    public async Task<IActionResult> SetPermissions(string id, [FromBody] UpdateFilePermissionsRequest request, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var createdByUserId = await _store.GetCreatedByUserIdAsync(id, ct);
        if (createdByUserId is null) return NotFound(new { error = "الملف غير موجود." });
        if (user.Role != UserRoles.Admin && !string.Equals(createdByUserId, user.Id, StringComparison.OrdinalIgnoreCase))
            return Forbid();

        await _store.SetPermittedUserIdsAsync(id, request.UserIds, ct);
        return NoContent();
    }

    [HttpPost("files/{id}/relationships")]
    public async Task<IActionResult> AddRelationship(string id, [FromBody] AddFileRelationshipRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RelatedFileId) || string.IsNullOrWhiteSpace(request.SharedColumn))
            return BadRequest(new { error = "relatedFileId and sharedColumn are required." });
        var relationship = await _store.AddRelationshipAsync(id, request.RelatedFileId, request.SharedColumn, ct);
        return Ok(relationship);
    }

    [HttpDelete("files/{id}/relationships/{relationshipId}")]
    public async Task<IActionResult> DeleteRelationship(string id, string relationshipId, CancellationToken ct)
    {
        await _store.DeleteRelationshipAsync(id, relationshipId, ct);
        return NoContent();
    }

    /// <summary>Drops a pending upload that the user cancelled.</summary>
    [HttpDelete("pending/{token}")]
    public IActionResult DiscardPending(string token)
    {
        _parser.Discard(token);
        return NoContent();
    }

    [HttpDelete("files/{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        await _store.DeleteAsync(id, ct);
        return NoContent();
    }
}
