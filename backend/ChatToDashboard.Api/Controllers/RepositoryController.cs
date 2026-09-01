using ChatToDashboard.Api.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

[ApiController]
[Route("api/repository")]
public class RepositoryController : ControllerBase
{
    private const long MaxUploadBytes = 50 * 1024 * 1024;

    private readonly RepositoryStore _store;
    private readonly UploadParser _parser;
    private readonly ILogger<RepositoryController> _logger;

    public RepositoryController(RepositoryStore store, UploadParser parser, ILogger<RepositoryController> logger)
    {
        _store = store;
        _parser = parser;
        _logger = logger;
    }

    /// <summary>All saved files, newest first.</summary>
    [HttpGet("files")]
    public async Task<IActionResult> Files(CancellationToken ct) => Ok(await _store.ListAsync(ct));

    [HttpGet("categories")]
    public async Task<IActionResult> Categories(CancellationToken ct) => Ok(await _store.ListCategoriesAsync(ct));

    /// <summary>
    /// Parses uploaded files on the server and returns them as "pending" — they are only
    /// stored once the user assigns each one a category via POST files.
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

    /// <summary>Saves a previously parsed upload into the repository under a category.</summary>
    [HttpPost("files")]
    public async Task<IActionResult> Save([FromBody] SaveUploadRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return BadRequest(new { error = "token is required." });
        if (!_parser.TryTake(request.Token, out var parsed))
            return NotFound(new { error = "انتهت صلاحية الملف المرفوع. ارفعه من جديد." });

        try
        {
            var saved = await _store.SaveAsync(parsed, request.Category, ct);
            return Ok(saved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save {File} to the repository", parsed.FileName);
            return StatusCode(500, new { error = ex.Message });
        }
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
