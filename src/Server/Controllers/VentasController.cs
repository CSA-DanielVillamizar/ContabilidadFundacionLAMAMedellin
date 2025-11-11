using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.DTOs.Ventas;
using Server.Services.Ventas;

namespace Server.Controllers;

/// <summary>
/// API para gestión de ventas de productos
/// </summary>
[Authorize(Policy = "TesoreroJunta")]
[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly IVentasService _ventasService;

    public VentasController(IVentasService ventasService)
    {
        _ventasService = ventasService;
    }

    /// <summary>
    /// Obtiene todas las ventas
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<VentaProductoDto>>> GetAll()
    {
        var ventas = await _ventasService.GetAllAsync();
        return Ok(ventas);
    }

    /// <summary>
    /// Obtiene ventas por estado
    /// </summary>
    [HttpGet("estado/{estado}")]
    public async Task<ActionResult<List<VentaProductoDto>>> GetByEstado(int estado)
    {
        var ventas = await _ventasService.GetByEstadoAsync(estado);
        return Ok(ventas);
    }

    /// <summary>
    /// Obtiene ventas de un miembro específico
    /// </summary>
    [HttpGet("miembro/{miembroId}")]
    public async Task<ActionResult<List<VentaProductoDto>>> GetByMiembro(Guid miembroId)
    {
        var ventas = await _ventasService.GetByMiembroAsync(miembroId);
        return Ok(ventas);
    }

    /// <summary>
    /// Obtiene una venta por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<VentaProductoDto>> GetById(Guid id)
    {
        var venta = await _ventasService.GetByIdAsync(id);
        if (venta == null)
            return NotFound($"Venta {id} no encontrada");

        return Ok(venta);
    }

    /// <summary>
    /// Crea una nueva venta (valida y actualiza stock automáticamente)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<VentaProductoDto>> Create([FromBody] VentaProductoCreateDto dto)
    {
        try
        {
            var venta = await _ventasService.CreateAsync(dto, User.Identity?.Name);
            return CreatedAtAction(nameof(GetById), new { id = venta.Id }, venta);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Cambia el estado de una venta
    /// </summary>
    [HttpPatch("{id}/estado")]
    public async Task<ActionResult> CambiarEstado(Guid id, [FromBody] CambiarEstadoVentaRequest request)
    {
        var resultado = await _ventasService.CambiarEstadoAsync(id, request.NuevoEstado, User.Identity?.Name);
        if (!resultado)
            return NotFound($"Venta {id} no encontrada");

        return NoContent();
    }

    /// <summary>
    /// Registra el pago de una venta (vincula con ingreso)
    /// </summary>
    [HttpPost("{id}/registrar-pago")]
    public async Task<ActionResult> RegistrarPago(Guid id, [FromBody] RegistrarPagoVentaRequest request)
    {
        var resultado = await _ventasService.RegistrarPagoAsync(id, request.IngresoId, User.Identity?.Name);
        if (!resultado)
            return NotFound($"Venta {id} no encontrada");

        return NoContent();
    }

    /// <summary>
    /// Registra la entrega de una venta
    /// </summary>
    [HttpPost("{id}/registrar-entrega")]
    public async Task<ActionResult> RegistrarEntrega(Guid id)
    {
        var resultado = await _ventasService.RegistrarEntregaAsync(id, User.Identity?.Name);
        if (!resultado)
            return NotFound($"Venta {id} no encontrada");

        return Ok(new { mensaje = "Entrega registrada correctamente" });
    }
}

public record CambiarEstadoVentaRequest(int NuevoEstado);
public record RegistrarPagoVentaRequest(Guid IngresoId);
