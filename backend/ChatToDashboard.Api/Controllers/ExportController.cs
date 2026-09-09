using ChatToDashboard.Api.Export;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>
/// Turns the dashboard currently on screen into a downloadable file. PDF export is
/// handled entirely client-side (the browser's own print-to-PDF, see index.html) — this
/// controller only builds the .pptx, since a real PowerPoint file needs server-side
/// OOXML assembly.
/// </summary>
[ApiController]
[Route("api/export")]
public class ExportController : ControllerBase
{
    [HttpPost("pptx")]
    public IActionResult Pptx([FromBody] PptxExportRequest request)
    {
        if (request.Widgets is null || request.Widgets.Count == 0)
            return BadRequest(new { error = "لا يوجد عناصر لتصديرها." });

        var bytes = PptxBuilder.Build(request);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            "dashboard.pptx");
    }
}
