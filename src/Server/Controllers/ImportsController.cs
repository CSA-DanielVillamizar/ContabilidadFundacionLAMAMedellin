using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers;

[ApiController]
[Route("api/imports")]
public class ImportsController : ControllerBase
{
    [HttpGet("last-log")]
    [Authorize(Policy = "TesoreroJunta")]
    public IActionResult GetLastLog()
    {
        var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var logsDir = Path.Combine(webRoot, "data", "import_logs");
        if (!Directory.Exists(logsDir)) return NotFound();
        var file = Directory.GetFiles(logsDir).OrderByDescending(f => f).FirstOrDefault();
        if (file is null) return NotFound();
        var bytes = System.IO.File.ReadAllBytes(file);
        var name = Path.GetFileName(file);
        return File(bytes, "text/csv", name);
    }
}
