using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OutputCaching;
using Server.Data;
using Server.Models;

namespace Server.Controllers;

/// <summary>
/// API para gestión de conceptos de ingresos y egresos
/// </summary>
[Authorize(Policy = "AdminOrTesoreroWith2FA")]
[ApiController]
[Route("api/[controller]")]
public class ConceptosController : ControllerBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public ConceptosController(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    /// <summary>
    /// Obtiene todos los conceptos
    /// </summary>
    [HttpGet]
    [OutputCache(PolicyName = "Conceptos")]
    public async Task<ActionResult<List<ConceptoDto>>> GetAll(
        [FromQuery] bool? esIngreso = null,
        [FromQuery] bool? esRecurrente = null,
        [FromQuery] int? moneda = null)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        
        var query = db.Conceptos.AsNoTracking().AsQueryable();

        // Filtros
        if (esIngreso.HasValue)
            query = query.Where(c => c.EsIngreso == esIngreso.Value);

        if (esRecurrente.HasValue)
            query = query.Where(c => c.EsRecurrente == esRecurrente.Value);

        if (moneda.HasValue)
            query = query.Where(c => c.Moneda == (Moneda)moneda.Value);

        var conceptos = await query
            .OrderBy(c => c.EsIngreso ? 0 : 1) // Ingresos primero
            .ThenBy(c => c.Nombre)
            .Select(c => new ConceptoDto
            {
                Id = c.Id,
                Codigo = c.Codigo,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                EsIngreso = c.EsIngreso,
                EsRecurrente = c.EsRecurrente,
                Moneda = c.Moneda.ToString(),
                PrecioBase = c.PrecioBase,
                Periodicidad = c.Periodicidad.ToString()
            })
            .ToListAsync();

        return Ok(conceptos);
    }

    /// <summary>
    /// Obtiene listado simple de conceptos para dropdowns
    /// </summary>
    [HttpGet("simples")]
    [OutputCache(PolicyName = "Conceptos")]
    public async Task<ActionResult<List<ConceptoSimpleDto>>> GetSimples()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        
        var conceptos = await db.Conceptos
            .OrderBy(c => c.Nombre)
            .Select(c => new ConceptoSimpleDto
            {
                Id = c.Id,
                Codigo = c.Codigo,
                Nombre = c.Nombre,
                EsIngreso = c.EsIngreso
            })
            .ToListAsync();

        return Ok(conceptos);
    }

    /// <summary>
    /// Obtiene un concepto por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ConceptoDto>> GetById(int id)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        
        var concepto = await db.Conceptos
            .Where(c => c.Id == id)
            .Select(c => new ConceptoDto
            {
                Id = c.Id,
                Codigo = c.Codigo,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                EsIngreso = c.EsIngreso,
                EsRecurrente = c.EsRecurrente,
                Moneda = c.Moneda.ToString(),
                PrecioBase = c.PrecioBase,
                Periodicidad = c.Periodicidad.ToString()
            })
            .FirstOrDefaultAsync();

        if (concepto == null)
            return NotFound(new { Message = "Concepto no encontrado" });

        return Ok(concepto);
    }
}

/// <summary>
/// DTO para transferencia de conceptos completos
/// </summary>
public class ConceptoDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool EsIngreso { get; set; }
    public bool EsRecurrente { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public decimal PrecioBase { get; set; }
    public string? Periodicidad { get; set; }
}

/// <summary>
/// DTO simplificado para dropdowns y listas
/// </summary>
public class ConceptoSimpleDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool EsIngreso { get; set; }
}
