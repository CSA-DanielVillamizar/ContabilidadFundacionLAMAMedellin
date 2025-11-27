using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Data;
using Server.Models;
using Server.Services.Recibos;

namespace Server.Controllers;

[ApiController]
[Route("api/recibos")]
public class RecibosController : ControllerBase
{
    private readonly IRecibosService _recibos;
    private readonly AppDbContext _db;

    public RecibosController(IRecibosService recibos, AppDbContext db)
    {
        _recibos = recibos;
        _db = db;
    }

    [HttpGet]
    [Authorize(Policy = "TesoreroJuntaConsulta")]
    public IActionResult List([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var skip = (page - 1) * pageSize;
        var q = _db.Recibos
            .OrderByDescending(r => r.FechaEmision)
            .Skip(skip)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                r.Serie,
                r.Ano,
                r.Consecutivo,
                r.FechaEmision,
                r.TotalCop,
                r.Estado,
                TieneCertificado = _db.CertificadosDonacion.Any(cd => cd.ReciboId == r.Id),
                CertificadoId = _db.CertificadosDonacion
                    .Where(cd => cd.ReciboId == r.Id)
                    .OrderByDescending(cd => cd.FechaEmision)
                    .Select(cd => (Guid?)cd.Id)
                    .FirstOrDefault(),
                EstadoCertificado = _db.CertificadosDonacion
                    .Where(cd => cd.ReciboId == r.Id)
                    .OrderByDescending(cd => cd.FechaEmision)
                    .Select(cd => (EstadoCertificado?)cd.Estado)
                    .FirstOrDefault()
            });
        return Ok(q.ToList());
    }

    public record CreateReciboDto(Guid? MiembroId, string? TerceroLibre, List<CreateItemDto> Items);
    public record CreateItemDto(string CodigoConcepto, int Cantidad);

    [HttpPost]
    [Authorize(Policy = "TesoreroJunta")]
    public async Task<IActionResult> Create([FromBody] CreateReciboDto dto)
    {
        if (dto is null || dto.Items is null || dto.Items.Count == 0) return BadRequest("Items requeridos");

        var items = dto.Items.Select(i => (i.CodigoConcepto, i.Cantidad));
        var r = await _recibos.CrearBorradorAsync(dto.MiembroId, dto.TerceroLibre, items, User?.Identity?.Name ?? "api", HttpContext.RequestAborted);
        r = await _recibos.EmitirAsync(r.Id, User?.Identity?.Name ?? "api", HttpContext.RequestAborted);
        return CreatedAtAction(nameof(Get), new { id = r.Id }, new { r.Id, r.Serie, r.Ano, r.Consecutivo });
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "TesoreroJuntaConsulta")]
    public IActionResult Get(Guid id)
    {
        var r = _db.Recibos.Where(x => x.Id == id).Select(x => new { x.Id, x.Serie, x.Ano, x.Consecutivo, x.FechaEmision, x.TotalCop, x.Estado }).FirstOrDefault();
        if (r is null) return NotFound();
        return Ok(r);
    }

    [HttpGet("{id:guid}/pdf")]
    [Authorize(Policy = "TesoreroJuntaConsulta")]
    public async Task<IActionResult> GetPdf(Guid id)
    {
        var pdf = await _recibos.GenerarPdfAsync(id);
        return File(pdf, "application/pdf", $"recibo-{id}.pdf");
    }

    [HttpPost("{id:guid}/emitir")]
    [Authorize(Policy = "TesoreroJunta")]
    public async Task<IActionResult> Emitir(Guid id)
    {
        try
        {
            Console.WriteLine($"🎯 Controller: Recibiendo solicitud para emitir recibo {id}");
            var recibo = await _recibos.EmitirAsync(id, User?.Identity?.Name ?? "api", HttpContext.RequestAborted);
            if (recibo == null)
            {
                Console.WriteLine($"❌ Controller: Recibo {id} no encontrado");
                return NotFound();
            }
            Console.WriteLine($"✅ Controller: Recibo {id} emitido. Estado: {recibo.Estado}");
            return Ok(new { recibo.Id, recibo.Estado });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Controller: Error emitiendo recibo {id}: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return BadRequest(ex.Message);
        }
    }

    public record AnularRequest(string? Razon);

    [HttpPost("{id:guid}/anular")]
    [Authorize(Policy = "TesoreroJunta")]
    public async Task<IActionResult> Anular(Guid id, [FromBody] AnularRequest req)
    {
        var ok = await _recibos.AnularAsync(id, req?.Razon ?? string.Empty, User?.Identity?.Name ?? "api");
        if (!ok) return NotFound();
        return NoContent();
    }
}
