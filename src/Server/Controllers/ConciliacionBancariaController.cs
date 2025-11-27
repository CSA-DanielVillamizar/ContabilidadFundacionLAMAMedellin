using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.DTOs.ConciliacionBancaria;
using Server.Services.ConciliacionBancaria;
using System.Security.Claims;

namespace Server.Controllers;

/// <summary>
/// Controlador REST para conciliación bancaria
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminGerenteTesorero")]
public class ConciliacionBancariaController : ControllerBase
{
    private readonly IConciliacionBancariaService _service;

    public ConciliacionBancariaController(IConciliacionBancariaService service)
    {
        _service = service;
    }

    /// <summary>
    /// GET: api/conciliacionbancaria
    /// Obtiene listado de conciliaciones con filtros
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] int? ano,
        [FromQuery] int? mes,
        [FromQuery] string? estado,
        [FromQuery] int pagina = 1,
        [FromQuery] int porPagina = 20)
    {
        try
        {
            var (conciliaciones, totalCount) = await _service.ListarAsync(ano, mes, estado, pagina, porPagina);

            return Ok(new
            {
                Conciliaciones = conciliaciones,
                TotalCount = totalCount,
                Pagina = pagina,
                PorPagina = porPagina
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error al obtener conciliaciones", Error = ex.Message });
        }
    }

    /// <summary>
    /// GET: api/conciliacionbancaria/{id}
    /// Obtiene detalle completo de una conciliación
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerDetalle(Guid id)
    {
        try
        {
            var conciliacion = await _service.ObtenerDetalleAsync(id);

            if (conciliacion == null)
                return NotFound(new { Message = "Conciliación no encontrada" });

            return Ok(conciliacion);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error al obtener conciliación", Error = ex.Message });
        }
    }

    /// <summary>
    /// GET: api/conciliacionbancaria/resumen/{ano}
    /// Obtiene resumen anual de conciliaciones
    /// </summary>
    [HttpGet("resumen/{ano}")]
    public async Task<IActionResult> ObtenerResumenAnual(int ano)
    {
        try
        {
            var resumen = await _service.ObtenerResumenAnualAsync(ano);
            return Ok(resumen);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error al obtener resumen", Error = ex.Message });
        }
    }

    /// <summary>
    /// GET: api/conciliacionbancaria/calcular-saldos
    /// Calcula saldos automáticamente para un período
    /// </summary>
    [HttpGet("calcular-saldos")]
    public async Task<IActionResult> CalcularSaldos([FromQuery] int ano, [FromQuery] int mes)
    {
        try
        {
            var (saldoLibros, saldoBanco, diferencia) = await _service.CalcularSaldosAsync(ano, mes);

            return Ok(new
            {
                SaldoLibros = saldoLibros,
                SaldoBanco = saldoBanco,
                Diferencia = diferencia
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error al calcular saldos", Error = ex.Message });
        }
    }

    /// <summary>
    /// POST: api/conciliacionbancaria
    /// Crea una nueva conciliación
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminGerenteTesorero")]
    public async Task<IActionResult> Crear([FromBody] ConciliacionBancariaFormDto dto)
    {
        try
        {
            var usuario = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";
            var id = await _service.CrearAsync(dto, usuario);

            return CreatedAtAction(nameof(ObtenerDetalle), new { id }, new { Id = id, Message = "Conciliación creada exitosamente" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error al crear conciliación", Error = ex.Message });
        }
    }

    /// <summary>
    /// PUT: api/conciliacionbancaria/{id}
    /// Actualiza una conciliación existente
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminGerenteTesorero")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] ConciliacionBancariaFormDto dto)
    {
        try
        {
            var usuario = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";
            await _service.ActualizarAsync(id, dto, usuario);

            return Ok(new { Message = "Conciliación actualizada exitosamente" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error al actualizar conciliación", Error = ex.Message });
        }
    }

    /// <summary>
    /// DELETE: api/conciliacionbancaria/{id}
    /// Elimina una conciliación (solo si está en Pendiente)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminGerente")]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        try
        {
            await _service.EliminarAsync(id);
            return Ok(new { Message = "Conciliación eliminada exitosamente" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error al eliminar conciliación", Error = ex.Message });
        }
    }

    /// <summary>
    /// POST: api/conciliacionbancaria/{id}/items
    /// Agrega un item a la conciliación
    /// </summary>
    [HttpPost("{id}/items")]
    public async Task<IActionResult> AgregarItem(Guid id, [FromBody] ItemConciliacionFormDto dto)
    {
        try
        {
            var itemId = await _service.AgregarItemAsync(id, dto);
            return Ok(new { ItemId = itemId, Message = "Item agregado exitosamente" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error al agregar item", Error = ex.Message });
        }
    }

    /// <summary>
    /// PUT: api/conciliacionbancaria/items/{itemId}
    /// Actualiza un item de conciliación
    /// </summary>
    [HttpPut("items/{itemId}")]
    public async Task<IActionResult> ActualizarItem(Guid itemId, [FromBody] ItemConciliacionFormDto dto)
    {
        try
        {
            await _service.ActualizarItemAsync(itemId, dto);
            return Ok(new { Message = "Item actualizado exitosamente" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error al actualizar item", Error = ex.Message });
        }
    }

    /// <summary>
    /// DELETE: api/conciliacionbancaria/items/{itemId}
    /// Elimina un item de conciliación
    /// </summary>
    [HttpDelete("items/{itemId}")]
    public async Task<IActionResult> EliminarItem(Guid itemId)
    {
        try
        {
            await _service.EliminarItemAsync(itemId);
            return Ok(new { Message = "Item eliminado exitosamente" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error al eliminar item", Error = ex.Message });
        }
    }

    /// <summary>
    /// PATCH: api/conciliacionbancaria/items/{itemId}/conciliado
    /// Marca/desmarca un item como conciliado
    /// </summary>
    [HttpPatch("items/{itemId}/conciliado")]
    public async Task<IActionResult> MarcarConciliado(Guid itemId, [FromBody] bool conciliado)
    {
        try
        {
            await _service.MarcarItemConciliadoAsync(itemId, conciliado);
            return Ok(new { Message = $"Item marcado como {(conciliado ? "conciliado" : "pendiente")}" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error al marcar item", Error = ex.Message });
        }
    }

    /// <summary>
    /// POST: api/conciliacionbancaria/{id}/cambiar-estado
    /// Cambia el estado de una conciliación
    /// </summary>
    [HttpPost("{id}/cambiar-estado")]
    [Authorize(Policy = "AdminGerenteTesorero")]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] string nuevoEstado)
    {
        try
        {
            var usuario = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";
            await _service.CambiarEstadoAsync(id, nuevoEstado, usuario);

            return Ok(new { Message = $"Estado cambiado a {nuevoEstado}" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error al cambiar estado", Error = ex.Message });
        }
    }

    /// <summary>
    /// POST: api/conciliacionbancaria/{id}/matching-automatico
    /// Realiza matching automático de items
    /// </summary>
    [HttpPost("{id}/matching-automatico")]
    public async Task<IActionResult> MatchingAutomatico(Guid id)
    {
        try
        {
            var itemsMatcheados = await _service.RealizarMatchingAutomaticoAsync(id);
            return Ok(new { ItemsMatcheados = itemsMatcheados, Message = $"Se matchearon {itemsMatcheados} items automáticamente" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error al realizar matching", Error = ex.Message });
        }
    }

    /// <summary>
    /// POST: api/conciliacionbancaria/importar-extracto
    /// Importa extracto bancario
    /// </summary>
    [HttpPost("importar-extracto")]
    public async Task<IActionResult> ImportarExtracto([FromBody] ImportarExtractoDto dto)
    {
        try
        {
            var itemsImportados = await _service.ImportarExtractoAsync(dto);
            return Ok(new { ItemsImportados = itemsImportados, Message = $"Se importaron {itemsImportados} items" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error al importar extracto", Error = ex.Message });
        }
    }

    /// <summary>
    /// GET: api/conciliacionbancaria/existe
    /// Verifica si existe conciliación para un período
    /// </summary>
    [HttpGet("existe")]
    public async Task<IActionResult> Existe([FromQuery] int ano, [FromQuery] int mes, [FromQuery] Guid? excluyendoId = null)
    {
        try
        {
            var existe = await _service.ExisteConciliacionAsync(ano, mes, excluyendoId);
            return Ok(new { Existe = existe });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error al verificar existencia", Error = ex.Message });
        }
    }
}
