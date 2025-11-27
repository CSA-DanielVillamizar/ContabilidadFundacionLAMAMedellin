using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.DTOs.Donaciones;
using Server.Models;
using Server.Services.Donaciones;

namespace Server.Controllers;

/// <summary>
/// Controlador para gestión de certificados de donación (RTE).
/// </summary>
[ApiController]
[Route("api/certificados-donacion")]
[Authorize(Policy = "TesoreroJunta")]
public class CertificadosDonacionController : ControllerBase
{
    private readonly ICertificadosDonacionService _service;

    public CertificadosDonacionController(ICertificadosDonacionService service)
    {
        _service = service;
    }

    /// <summary>
    /// Obtiene lista paginada de certificados.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] string? query,
        [FromQuery] EstadoCertificado? estado,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _service.GetPagedAsync(query, estado, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un certificado por ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var certificado = await _service.GetByIdAsync(id);
        if (certificado == null) return NotFound();
        return Ok(certificado);
    }

    /// <summary>
    /// Crea un nuevo certificado (estado Borrador).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCertificadoDonacionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var currentUser = User?.Identity?.Name ?? "api";
        var id = await _service.CreateAsync(dto, currentUser);

        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>
    /// Actualiza un certificado existente (solo estado Borrador).
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCertificadoDonacionDto dto)
    {
        if (id != dto.Id) return BadRequest("El ID no coincide");
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var currentUser = User?.Identity?.Name ?? "api";
        
        try
        {
            var success = await _service.UpdateAsync(dto, currentUser);
            if (!success) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Elimina un certificado (solo estado Borrador).
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Emite un certificado (cambia de Borrador a Emitido y asigna consecutivo).
    /// </summary>
    [HttpPost("{id:guid}/emitir")]
    public async Task<IActionResult> Emitir(Guid id, [FromBody] EmitirCertificadoDto dto)
    {
        if (id != dto.Id) return BadRequest("El ID no coincide");
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var currentUser = User?.Identity?.Name ?? "api";
        
        try
        {
            var success = await _service.EmitirAsync(dto, currentUser);
            if (!success) return NotFound();
            return Ok(new { message = "Certificado emitido correctamente" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Anula un certificado emitido.
    /// </summary>
    [HttpPost("{id:guid}/anular")]
    public async Task<IActionResult> Anular(Guid id, [FromBody] AnularCertificadoDto dto)
    {
        if (id != dto.Id) return BadRequest("El ID no coincide");
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var currentUser = User?.Identity?.Name ?? "api";
        
        try
        {
            var success = await _service.AnularAsync(dto, currentUser);
            if (!success) return NotFound();
            return Ok(new { message = "Certificado anulado correctamente" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Descarga el PDF de un certificado.
    /// </summary>
    [HttpGet("{id:guid}/pdf")]
    [Authorize(Policy = "TesoreroJuntaConsulta")]
    public async Task<IActionResult> GetPdf(Guid id)
    {
        try
        {
            var pdf = await _service.GenerarPdfAsync(id);
            return File(pdf, "application/pdf", $"certificado-donacion-{id}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene certificados asociados a un recibo.
    /// </summary>
    [HttpGet("por-recibo/{reciboId:guid}")]
    public async Task<IActionResult> GetByReciboId(Guid reciboId)
    {
        var certificados = await _service.GetByReciboIdAsync(reciboId);
        return Ok(certificados);
    }

    /// <summary>
    /// Obtiene el siguiente consecutivo disponible para el año actual.
    /// </summary>
    [HttpGet("siguiente-consecutivo")]
    public async Task<IActionResult> GetSiguienteConsecutivo([FromQuery] int? ano)
    {
        int year = ano ?? DateTime.Now.Year;
        var consecutivo = await _service.GetNextConsecutivoAsync(year);
        return Ok(new { ano = year, consecutivo });
    }
}
