using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Services.Reportes;

namespace Server.Controllers;

[ApiController]
[Route("api/reportes")]
public class ReportsController : ControllerBase
{
    private readonly IReportesService _reportes;

    public ReportsController(IReportesService reportes) => _reportes = reportes;

    [HttpGet("tesoreria")]
    [Authorize(Policy = "TesoreroJuntaConsulta")]
    public async Task<IActionResult> Tesoreria([FromQuery] int anio, [FromQuery] int mes)
    {
        if (anio <= 0 || mes <= 0) return BadRequest("anio y mes requeridos");
        var res = await _reportes.GenerarReporteMensualAsync(anio, mes);
        return Ok(res);
    }

    [HttpGet("tesoreria/pdf")]
    [Authorize(Policy = "TesoreroJuntaConsulta")]
    public async Task<IActionResult> TesoreriaPdf([FromQuery] int anio, [FromQuery] int mes)
    {
        if (anio <= 0 || mes <= 0) return BadRequest("anio y mes requeridos");
        var pdf = await _reportes.GenerarReporteMensualPdfAsync(anio, mes);
        return File(pdf, "application/pdf", $"reporte-tesoreria-{anio}-{mes}.pdf");
    }

    [HttpGet("tesoreria/excel")]
    [Authorize(Policy = "TesoreroJuntaConsulta")]
    public async Task<IActionResult> TesoreriaExcel([FromQuery] int anio, [FromQuery] int mes)
    {
        if (anio <= 0 || mes <= 0) return BadRequest("anio y mes requeridos");
        var xls = await _reportes.GenerarReporteMensualExcelAsync(anio, mes);
        return File(xls, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"reporte-tesoreria-{anio}-{mes}.xlsx");
    }
}
