using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.DTOs.Inventario;
using Server.Services.Inventario;

namespace Server.Controllers;

/// <summary>
/// API para gestión de inventario y movimientos
/// </summary>
[Authorize(Policy = "TesoreroJunta")]
[ApiController]
[Route("api/[controller]")]
public class InventarioController : ControllerBase
{
    private readonly IInventarioService _inventarioService;

    public InventarioController(IInventarioService inventarioService)
    {
        _inventarioService = inventarioService;
    }

    /// <summary>
    /// Obtiene todos los movimientos de inventario
    /// </summary>
    [HttpGet("movimientos")]
    public async Task<ActionResult<List<MovimientoInventarioDto>>> GetAllMovimientos()
    {
        var movimientos = await _inventarioService.GetAllMovimientosAsync();
        return Ok(movimientos);
    }

    /// <summary>
    /// Obtiene movimientos de un producto específico
    /// </summary>
    [HttpGet("movimientos/producto/{productoId}")]
    public async Task<ActionResult<List<MovimientoInventarioDto>>> GetMovimientosByProducto(Guid productoId)
    {
        var movimientos = await _inventarioService.GetMovimientosByProductoAsync(productoId);
        return Ok(movimientos);
    }

    /// <summary>
    /// Obtiene movimientos por tipo
    /// </summary>
    [HttpGet("movimientos/tipo/{tipo}")]
    public async Task<ActionResult<List<MovimientoInventarioDto>>> GetMovimientosByTipo(int tipo)
    {
        var movimientos = await _inventarioService.GetMovimientosByTipoAsync(tipo);
        return Ok(movimientos);
    }

    /// <summary>
    /// Obtiene movimientos por rango de fechas
    /// </summary>
    [HttpGet("movimientos/fecha")]
    public async Task<ActionResult<List<MovimientoInventarioDto>>> GetMovimientosByFecha(
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin)
    {
        var movimientos = await _inventarioService.GetMovimientosByFechaAsync(fechaInicio, fechaFin);
        return Ok(movimientos);
    }

    /// <summary>
    /// Obtiene un movimiento por ID
    /// </summary>
    [HttpGet("movimientos/{id}")]
    public async Task<ActionResult<MovimientoInventarioDto>> GetMovimientoById(Guid id)
    {
        var movimiento = await _inventarioService.GetMovimientoByIdAsync(id);
        if (movimiento == null)
            return NotFound($"Movimiento {id} no encontrado");

        return Ok(movimiento);
    }

    /// <summary>
    /// Crea un movimiento manual de inventario (ajustes, devoluciones, mermas, donaciones)
    /// </summary>
    [HttpPost("movimientos/manual")]
    public async Task<ActionResult<MovimientoInventarioDto>> CreateMovimientoManual(
        [FromBody] MovimientoInventarioCreateDto dto)
    {
        try
        {
            var movimiento = await _inventarioService.CreateMovimientoManualAsync(dto, User.Identity?.Name);
            return CreatedAtAction(nameof(GetMovimientoById), new { id = movimiento.Id }, movimiento);
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
}
