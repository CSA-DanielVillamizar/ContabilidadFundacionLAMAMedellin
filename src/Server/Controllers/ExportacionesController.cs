using Microsoft.AspNetCore.Mvc;
using Server.Services.Exportaciones;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExportacionesController : ControllerBase
{
    private readonly ExportacionesService _exportService;

    public ExportacionesController(ExportacionesService exportService)
    {
        _exportService = exportService;
    }

    /// <summary>
    /// Exporta recibos a Excel
    /// </summary>
    [HttpGet("recibos")]
    public async Task<IActionResult> ExportarRecibos([FromQuery] DateTime? desde = null, [FromQuery] DateTime? hasta = null)
    {
        var bytes = await _exportService.ExportarRecibosAsync(desde, hasta);
        
        var fileName = $"Recibos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    /// <summary>
    /// Exporta egresos a Excel
    /// </summary>
    [HttpGet("egresos")]
    public async Task<IActionResult> ExportarEgresos([FromQuery] DateTime? desde = null, [FromQuery] DateTime? hasta = null)
    {
        var bytes = await _exportService.ExportarEgresosAsync(desde, hasta);
        
        var fileName = $"Egresos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
