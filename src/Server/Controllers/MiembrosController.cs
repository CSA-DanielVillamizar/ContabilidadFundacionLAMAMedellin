using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Services.Miembros;

namespace Server.Controllers;

/// <summary>
/// Controlador API para operaciones de Miembros (exportación, etc.).
/// NOTA TEMPORAL: Sin [Authorize] porque Blazor Server HttpClient no envía cookies
/// </summary>
[ApiController]
[Route("api/miembros")]
// [Authorize(Roles = "Admin,Tesorero,Junta,Consulta,Gerente,gerentenegocios")] // Comentado temporalmente
public class MiembrosController : ControllerBase
{
    private readonly IMiembrosExportService _exportService;

    public MiembrosController(IMiembrosExportService exportService)
    {
        _exportService = exportService;
    }

    /// <summary>
    /// Exporta la lista de miembros a un archivo Excel.
    /// </summary>
    /// <param name="query">Texto de búsqueda opcional (nombre, documento).</param>
    /// <param name="estado">Filtro por estado opcional.</param>
    /// <returns>Archivo Excel con los miembros filtrados.</returns>
    [HttpGet("export-excel")]
    public async Task<IActionResult> ExportExcel([FromQuery] string? query, [FromQuery] EstadoMiembro? estado)
    {
        var bytes = await _exportService.ExportToExcelAsync(query, estado);
        var fileName = $"Miembros-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
