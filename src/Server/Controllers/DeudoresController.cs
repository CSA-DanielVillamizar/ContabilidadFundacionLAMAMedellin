using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Data;
using Server.Services.Deudores;
using Server.Services.Exchange;
using Microsoft.EntityFrameworkCore;
using Server.Services.Recibos;

namespace Server.Controllers;

[ApiController]
[Route("api/deudores")]
public class DeudoresController : ControllerBase
{
    private readonly IDeudoresService _deudores;
    private readonly IRecibosService _recibos;
    private readonly AppDbContext _db;
    private readonly IExchangeRateService _trm;
    private readonly IDeudoresExportService _export;

    public DeudoresController(
        IDeudoresService deudores, 
        IRecibosService recibos, 
        AppDbContext db, 
        IExchangeRateService trm,
        IDeudoresExportService export)
    {
        _deudores = deudores;
        _recibos = recibos;
        _db = db;
        _trm = trm;
        _export = export;
    }

    [HttpGet]
    [Authorize(Policy = "TesoreroJuntaConsulta")]
    public async Task<IActionResult> Get([FromQuery] string? desde = null, [FromQuery] string? hasta = null, CancellationToken ct = default)
    {
        DateOnly? d = null, h = null;
        if (!string.IsNullOrWhiteSpace(desde) && DateOnly.TryParse(desde + "-01", out var d1)) d = d1;
        if (!string.IsNullOrWhiteSpace(hasta) && DateOnly.TryParse(hasta + "-01", out var h1)) h = h1;
        var rows = await _deudores.CalcularAsync(d, h, ct);
        // Obtener precio vigente del concepto MENSUALIDAD
        var mensualidad = await _db.Conceptos.FirstOrDefaultAsync(c => c.Codigo == "MENSUALIDAD", ct);
        decimal precioMensualDisplayCop = 0;
        bool esUsd = false;
        if (mensualidad is not null)
        {
            esUsd = mensualidad.Moneda == Server.Models.Moneda.USD;
            if (esUsd)
            {
                var refDate = h ?? DateOnly.FromDateTime(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1));
                var trmUsd = await _trm.GetUsdCopAsync(refDate, ct);
                precioMensualDisplayCop = decimal.Round(mensualidad.PrecioBase * trmUsd, 2);
            }
            else
            {
                precioMensualDisplayCop = mensualidad.PrecioBase;
            }
        }

        // TRM por mes (solo si USD)
        var trmPorMes = new Dictionary<DateOnly, decimal>();
        if (esUsd)
        {
            var mesesUnicos = rows.SelectMany(r => r.MesesPendientes).Distinct();
            foreach (var m in mesesUnicos)
            {
                var trm = await _trm.GetUsdCopAsync(m, ct);
                trmPorMes[m] = trm;
            }
        }

        var payload = rows.Select(r =>
        {
            decimal total = 0;
            if (mensualidad is null)
            {
                total = 0;
            }
            else if (esUsd)
            {
                foreach (var m in r.MesesPendientes)
                {
                    var trm = trmPorMes.TryGetValue(m, out var trmVal) ? trmVal : 0;
                    total += decimal.Round(mensualidad.PrecioBase * trm, 2);
                }
            }
            else
            {
                total = r.MesesPendientes.Count * mensualidad.PrecioBase;
            }

            return new
            {
                r.MiembroId,
                r.Nombre,
                r.Ingreso,
                MesesPendientes = r.MesesPendientes.Select(m => m.ToString("yyyy-MM")).ToList(),
                PrecioMensualCop = precioMensualDisplayCop,
                TotalEstimadoCop = total
            };
        }).ToList();

        var totalGlobal = payload.Sum(x => x.TotalEstimadoCop);
        return Ok(payload);
    }

    [HttpGet("excel")]
    [Authorize(Policy = "TesoreroJuntaConsulta")]
    public async Task<IActionResult> GetExcel([FromQuery] string? desde = null, [FromQuery] string? hasta = null, CancellationToken ct = default)
    {
        DateOnly? d = null, h = null;
        if (!string.IsNullOrWhiteSpace(desde) && DateOnly.TryParse(desde + "-01", out var d1)) d = d1;
        if (!string.IsNullOrWhiteSpace(hasta) && DateOnly.TryParse(hasta + "-01", out var h1)) h = h1;
        var bytes = await _export.GenerarExcelAsync(d, h, ct);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "deudores.xlsx");
    }

    [HttpGet("pdf")]
    [Authorize(Policy = "TesoreroJuntaConsulta")]
    public async Task<IActionResult> GetPdf([FromQuery] string? desde = null, [FromQuery] string? hasta = null, CancellationToken ct = default)
    {
        DateOnly? d = null, h = null;
        if (!string.IsNullOrWhiteSpace(desde) && DateOnly.TryParse(desde + "-01", out var d1)) d = d1;
        if (!string.IsNullOrWhiteSpace(hasta) && DateOnly.TryParse(hasta + "-01", out var h1)) h = h1;
        var pdf = await _export.GenerarPdfAsync(d, h, ct);
        return File(pdf, "application/pdf", "deudores.pdf");
    }

    public record GenerarDto(Guid MiembroId, int CantidadMeses);

    [HttpPost("generar-recibo")]
    [Authorize(Policy = "TesoreroJunta")]
    public async Task<IActionResult> GenerarRecibo([FromBody] GenerarDto dto, CancellationToken ct)
    {
        if (dto.CantidadMeses <= 0) return BadRequest("Cantidad inválida");

        var items = new List<(string codigoConcepto, int cantidad)> { ("MENSUALIDAD", dto.CantidadMeses) };
        var creador = User?.Identity?.Name ?? "api";
        var rec = await _recibos.CrearBorradorAsync(dto.MiembroId, null, items, creador, ct);
        rec = await _recibos.EmitirAsync(rec.Id, creador, ct);
        return Ok(new { rec.Id, rec.Serie, rec.Ano, rec.Consecutivo });
    }
}
