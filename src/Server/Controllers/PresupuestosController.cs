using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.DTOs;
using Server.Services;
using Server.Services.Auth;

namespace Server.Controllers;

/// <summary>
/// Controlador para gestión de presupuestos y cálculo de ejecución presupuestal.
/// Permite crear, consultar, actualizar presupuestos y calcular % de ejecución desde movimientos reales.
/// </summary>
[Authorize(Policy = "AdminGerenteTesorero")]
[ApiController]
[Route("api/[controller]")]
public class PresupuestosController : ControllerBase
{
    private readonly IPresupuestosService _service;
    private readonly ICurrentUserService _currentUser;

    public PresupuestosController(IPresupuestosService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Obtiene listado de presupuestos con filtros y cálculo de ejecución
    /// GET /api/presupuestos?ano=2024&mes=11&conceptoId=5&pagina=1&porPagina=50
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerPresupuestos(
        [FromQuery] int? ano = null,
        [FromQuery] int? mes = null,
        [FromQuery] int? conceptoId = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int porPagina = 50)
    {
        try
        {
            var (presupuestos, totalCount) = await _service.ListarAsync(ano, mes, conceptoId, pagina, porPagina);
            return Ok(new 
            { 
                Presupuestos = presupuestos, 
                TotalCount = totalCount,
                Pagina = pagina,
                PorPagina = porPagina
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene detalle de un presupuesto con ejecución calculada
    /// GET /api/presupuestos/{id}
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerDetalle(Guid id)
    {
        try
        {
            var detalle = await _service.ObtenerDetalleAsync(id);
            if (detalle == null)
                return NotFound(new { Error = "Presupuesto no encontrado" });

            return Ok(detalle);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene consolidado anual: suma presupuestos por concepto
    /// GET /api/presupuestos/consolidado/{ano}
    /// </summary>
    [HttpGet("consolidado/{ano:int}")]
    public async Task<IActionResult> ObtenerConsolidadoAnual(int ano)
    {
        try
        {
            var consolidado = await _service.ObtenerConsolidadoAnualAsync(ano);
            return Ok(consolidado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Crea un nuevo presupuesto
    /// POST /api/presupuestos
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearPresupuestoDto dto)
    {
        try
        {
            var usuario = _currentUser.GetUserName() ?? "system";
            var detalle = await _service.CrearAsync(dto, usuario);
            return CreatedAtAction(nameof(ObtenerDetalle), new { id = detalle.Id }, detalle);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Error interno al crear presupuesto", Detalle = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza un presupuesto existente (solo monto y notas)
    /// PUT /api/presupuestos/{id}
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] ActualizarPresupuestoDto dto)
    {
        try
        {
            var usuario = _currentUser.GetUserName() ?? "system";
            var detalle = await _service.ActualizarAsync(id, dto, usuario);
            return Ok(detalle);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Error interno al actualizar presupuesto", Detalle = ex.Message });
        }
    }

    /// <summary>
    /// Elimina un presupuesto
    /// DELETE /api/presupuestos/{id}
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminGerente")]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        try
        {
            var eliminado = await _service.EliminarAsync(id);
            if (!eliminado)
                return NotFound(new { Error = "Presupuesto no encontrado" });

            return Ok(new { Message = "Presupuesto eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Calcula la ejecución de un presupuesto específico
    /// GET /api/presupuestos/{id}/ejecucion
    /// </summary>
    [HttpGet("{id:guid}/ejecucion")]
    public async Task<IActionResult> CalcularEjecucion(Guid id)
    {
        try
        {
            var montoEjecutado = await _service.CalcularEjecucionAsync(id);
            return Ok(new { PresupuestoId = id, MontoEjecutado = montoEjecutado });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Copia presupuestos de un período a otro
    /// POST /api/presupuestos/copiar
    /// </summary>
    [HttpPost("copiar")]
    public async Task<IActionResult> CopiarPresupuestos([FromBody] CopiarPresupuestosDto dto)
    {
        try
        {
            var usuario = _currentUser.GetUserName() ?? "system";
            var copiados = await _service.CopiarPresupuestosAsync(
                dto.AnoOrigen, 
                dto.MesOrigen, 
                dto.AnoDestino, 
                dto.MesDestino, 
                usuario);

            return Ok(new 
            { 
                Message = $"Se copiaron {copiados} presupuestos exitosamente",
                CantidadCopiada = copiados
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Error interno al copiar presupuestos", Detalle = ex.Message });
        }
    }

    /// <summary>
    /// Verifica si existe un presupuesto para un concepto y período
    /// GET /api/presupuestos/existe?ano=2024&mes=11&conceptoId=5
    /// </summary>
    [HttpGet("existe")]
    public async Task<IActionResult> VerificarExistencia(
        [FromQuery] int ano,
        [FromQuery] int mes,
        [FromQuery] int conceptoId)
    {
        try
        {
            var existe = await _service.ExistePresupuestoAsync(ano, mes, conceptoId);
            return Ok(new { Existe = existe });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}
