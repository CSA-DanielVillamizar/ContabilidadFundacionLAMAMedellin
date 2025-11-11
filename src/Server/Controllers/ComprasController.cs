using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.DTOs.Compras;
using Server.Services.Compras;

namespace Server.Controllers;

/// <summary>
/// API para gestión de compras de productos
/// </summary>
[Authorize(Policy = "TesoreroJunta")]
[ApiController]
[Route("api/[controller]")]
public class ComprasController : ControllerBase
{
    private readonly IComprasService _comprasService;

    public ComprasController(IComprasService comprasService)
    {
        _comprasService = comprasService;
    }

    /// <summary>
    /// Obtiene todas las compras
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<CompraProductoDto>>> GetAll()
    {
        var compras = await _comprasService.GetAllAsync();
        return Ok(compras);
    }

    /// <summary>
    /// Obtiene compras por estado
    /// </summary>
    [HttpGet("estado/{estado}")]
    public async Task<ActionResult<List<CompraProductoDto>>> GetByEstado(int estado)
    {
        var compras = await _comprasService.GetByEstadoAsync(estado);
        return Ok(compras);
    }

    /// <summary>
    /// Obtiene una compra por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CompraProductoDto>> GetById(Guid id)
    {
        var compra = await _comprasService.GetByIdAsync(id);
        if (compra == null)
            return NotFound($"Compra {id} no encontrada");

        return Ok(compra);
    }

    /// <summary>
    /// Crea una nueva compra
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CompraProductoDto>> Create([FromBody] CompraProductoCreateDto dto)
    {
        try
        {
            var compra = await _comprasService.CreateAsync(dto, User.Identity?.Name);
            return CreatedAtAction(nameof(GetById), new { id = compra.Id }, compra);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Cambia el estado de una compra
    /// </summary>
    [HttpPatch("{id}/estado")]
    public async Task<ActionResult> CambiarEstado(Guid id, [FromBody] CambiarEstadoRequest request)
    {
        var resultado = await _comprasService.CambiarEstadoAsync(id, request.NuevoEstado, User.Identity?.Name);
        if (!resultado)
            return NotFound($"Compra {id} no encontrada");

        return NoContent();
    }

    /// <summary>
    /// Registra el pago de una compra (vincula con egreso)
    /// </summary>
    [HttpPost("{id}/registrar-pago")]
    public async Task<ActionResult> RegistrarPago(Guid id, [FromBody] RegistrarPagoCompraRequest request)
    {
        var resultado = await _comprasService.RegistrarPagoAsync(id, request.EgresoId, User.Identity?.Name);
        if (!resultado)
            return NotFound($"Compra {id} no encontrada");

        return NoContent();
    }

    /// <summary>
    /// Registra la recepción de una compra (actualiza stock)
    /// </summary>
    [HttpPost("{id}/registrar-recepcion")]
    public async Task<ActionResult> RegistrarRecepcion(Guid id)
    {
        var resultado = await _comprasService.RegistrarRecepcionAsync(id, User.Identity?.Name);
        if (!resultado)
            return NotFound($"Compra {id} no encontrada");

        return Ok(new { mensaje = "Stock actualizado correctamente" });
    }
}

public record CambiarEstadoRequest(int NuevoEstado);
public record RegistrarPagoCompraRequest(Guid EgresoId);
