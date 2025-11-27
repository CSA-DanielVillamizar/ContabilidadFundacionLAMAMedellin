using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Server.DTOs.Productos;
using Server.Services.Productos;

namespace Server.Controllers;

/// <summary>
/// API para gestión de productos
/// NOTA TEMPORAL: Sin [Authorize] porque Blazor Server HttpClient no envía cookies
/// </summary>
// [Authorize(Roles = "Admin,Tesorero,Junta,Gerente,gerentenegocios")] // Comentado temporalmente
[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IProductosService _productosService;

    public ProductosController(IProductosService productosService)
    {
        _productosService = productosService;
    }

    /// <summary>
    /// Obtiene todos los productos
    /// </summary>
    [HttpGet]
    [OutputCache(Duration = 60)] // Cache 1 minuto - datos semi-estables
    public async Task<ActionResult<List<ProductoDto>>> GetAll()
    {
        var productos = await _productosService.GetAllAsync();
        return Ok(productos);
    }

    /// <summary>
    /// Obtiene solo productos activos
    /// </summary>
    [HttpGet("activos")]
    [OutputCache(Duration = 60)] // Cache 1 minuto - datos semi-estables
    public async Task<ActionResult<List<ProductoDto>>> GetActivos()
    {
        var productos = await _productosService.GetActivosAsync();
        return Ok(productos);
    }

    /// <summary>
    /// Obtiene productos con stock bajo
    /// </summary>
    [HttpGet("bajo-stock")]
    [OutputCache(Duration = 30)] // Cache 30 segundos - datos más volátiles
    public async Task<ActionResult<List<ProductoDto>>> GetBajoStock()
    {
        var productos = await _productosService.GetBajoStockAsync();
        return Ok(productos);
    }

    /// <summary>
    /// Obtiene un producto por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductoDto>> GetById(Guid id)
    {
        var producto = await _productosService.GetByIdAsync(id);
        if (producto == null)
            return NotFound($"Producto {id} no encontrado");

        return Ok(producto);
    }

    /// <summary>
    /// Obtiene un producto por código
    /// </summary>
    [HttpGet("codigo/{codigo}")]
    public async Task<ActionResult<ProductoDto>> GetByCodigo(string codigo)
    {
        var producto = await _productosService.GetByCodigoAsync(codigo);
        if (producto == null)
            return NotFound($"Producto con código {codigo} no encontrado");

        return Ok(producto);
    }

    /// <summary>
    /// Crea un nuevo producto
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProductoDto>> Create([FromBody] ProductoCreateUpdateDto dto)
    {
        try
        {
            var producto = await _productosService.CreateAsync(dto, User.Identity?.Name);
            return CreatedAtAction(nameof(GetById), new { id = producto.Id }, producto);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Actualiza un producto existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ProductoDto>> Update(Guid id, [FromBody] ProductoCreateUpdateDto dto)
    {
        try
        {
            var producto = await _productosService.UpdateAsync(id, dto, User.Identity?.Name);
            return Ok(producto);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Elimina un producto
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var resultado = await _productosService.DeleteAsync(id);
        if (!resultado)
            return NotFound($"Producto {id} no encontrado");

        return NoContent();
    }

    /// <summary>
    /// Activa o desactiva un producto
    /// </summary>
    [HttpPatch("{id}/activar")]
    public async Task<ActionResult> ActivarDesactivar(Guid id, [FromBody] bool activo)
    {
        var resultado = await _productosService.ActivarDesactivarAsync(id, activo, User.Identity?.Name);
        if (!resultado)
            return NotFound($"Producto {id} no encontrado");

        return NoContent();
    }

    /// <summary>
    /// Ajusta manualmente el stock de un producto
    /// </summary>
    [HttpPost("{id}/ajustar-stock")]
    public async Task<ActionResult<int>> AjustarStock(Guid id, [FromBody] AjustarStockRequest request)
    {
        try
        {
            var nuevoStock = await _productosService.AjustarStockAsync(
                id, 
                request.NuevaCantidad, 
                request.Motivo, 
                User.Identity?.Name
            );
            return Ok(new { nuevoStock });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

public record AjustarStockRequest(int NuevaCantidad, string Motivo);
