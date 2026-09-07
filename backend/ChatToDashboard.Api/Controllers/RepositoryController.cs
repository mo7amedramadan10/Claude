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
    private readonly ILogger<RepositoryController> _logger;

    public RepositoryController(
        RepositoryStore store, UploadParser parser, PermissionsService permissions, ILogger<RepositoryController> logger)
    {
        _store = store;
        _parser = parser;
        _permissions = permissions;
        _logger = logger;
    }

    /// <summary>All saved files, newest first.</summary>
    [HttpGet("files")]
    public async Task<IActionResult> Files(CancellationToken ct) => Ok(await _store.ListAsync(ct));

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
    public IActionResult Upload([FromForm] IFormFileCollection files)
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
            results.Add(_parser.Parse(file.FileName, stream));
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

    /// <summary>Edits display name/description/category without touching the data.</summary>
    [HttpPut("files/{id}/meta")]
    public async Task<IActionResult> UpdateMeta(string id, [FromBody] UpdateFileMetaRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName))
            return BadRequest(new { error = "displayName is required." });
        var ok = await _store.UpdateMetaAsync(id, request.DisplayName, request.Description, request.Category, ct);
        return ok ? NoContent() : NotFound(new { error = "الملف غير موجود." });
    }

    /// <summary>Named users allowed to use this file as a data source, on top of their
    /// existing category access. Only an Admin manages this — same authority level as
    /// managing user accounts and their category/system permissions.</summary>
    [HttpGet("files/{id}/permissions")]
    public async Task<IActionResult> GetPermissions(string id, CancellationToken ct) =>
        Ok(await _store.GetPermittedUserIdsAsync(id, ct));

    [HttpPut("files/{id}/permissions")]
    public async Task<IActionResult> SetPermissions(string id, [FromBody] UpdateFilePermissionsRequest request, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        if (user.Role != UserRoles.Admin) return Forbid();

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
