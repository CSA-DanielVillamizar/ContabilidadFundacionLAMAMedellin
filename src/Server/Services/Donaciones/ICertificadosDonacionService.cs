using Server.DTOs.Donaciones;
using Server.Models;

namespace Server.Services.Donaciones;

/// <summary>
/// Servicio para gestión de certificados de donación (RTE).
/// </summary>
public interface ICertificadosDonacionService
{
    // CRUD básico
    Task<PagedResult<CertificadoDonacionListItem>> GetPagedAsync(string? query, EstadoCertificado? estado, int page, int pageSize);
    Task<CertificadoDonacionDetailDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(CreateCertificadoDonacionDto dto, string currentUser);
    Task<bool> UpdateAsync(UpdateCertificadoDonacionDto dto, string currentUser);
    Task<bool> DeleteAsync(Guid id);

    // Workflow de estados
    Task<bool> EmitirAsync(EmitirCertificadoDto dto, string currentUser);
    Task<bool> AnularAsync(AnularCertificadoDto dto, string currentUser);

    // Numeración
    Task<int> GetNextConsecutivoAsync(int ano);

    // PDF
    Task<byte[]> GenerarPdfAsync(Guid certificadoId);
    
    // Email
    Task<bool> ReenviarEmailAsync(Guid certificadoId);
    
    // Buscar por recibo
    Task<List<CertificadoDonacionListItem>> GetByReciboIdAsync(Guid reciboId);
}
