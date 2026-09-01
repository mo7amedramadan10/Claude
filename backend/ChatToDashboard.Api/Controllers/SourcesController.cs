using ChatToDashboard.Api.Repository;
using ChatToDashboard.Api.Sources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ChatToDashboard.Api.Controllers;

[ApiController]
[Route("api/sources")]
public class SourcesController : ControllerBase
{
    private readonly SourceOptions _options;
    private readonly RepositoryStore _store;

    public SourcesController(IOptions<SourceOptions> options, RepositoryStore store)
    {
        _options = options.Value;
        _store = store;
    }

    /// <summary>
    /// The source list the header dropdown is built from: the configured systems, plus the
    /// categories that actually exist in the file repository right now.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var categories = await _store.ListCategoriesAsync(ct);
        return Ok(new
        {
            systems = _options.Systems.Select(s => new { id = s.Id, name = s.Name, connected = s.IsConnected }),
            categories,
        });
    }
}
