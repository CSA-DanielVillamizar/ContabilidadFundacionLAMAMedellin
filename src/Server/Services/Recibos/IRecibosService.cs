using Server.DTOs;
using Server.DTOs.Recibos;
using Server.Models;

namespace Server.Services.Recibos;

public interface IRecibosService
{
    // CRUD básico
    Task<PagedResult<ReciboListItem>> GetPagedAsync(string? query, EstadoRecibo? estado, int page, int pageSize);
    Task<ReciboDetailDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(CreateReciboDto dto, string currentUser);
    Task<bool> UpdateAsync(UpdateReciboDto dto, string currentUser);
    Task<bool> DeleteAsync(Guid id);

    // Workflow de estados
    Task<bool> CambiarEstadoAsync(CambiarEstadoReciboDto dto, string currentUser);
    Task<bool> EmitirAsync(Guid reciboId, string currentUser);
    Task<bool> AnularAsync(Guid reciboId, string razon, string currentUser);

    // Numeración
    Task<int> GetNextConsecutivoAsync(string serie, int ano);

    // PDF y QR
    Task<byte[]> GenerarPdfAsync(Guid reciboId);
    Task<string> GenerarQRCodeAsync(Guid reciboId);

    /// <summary>
    /// Obtiene todos los conceptos disponibles para recibos.
    /// </summary>
    /// <returns>Lista de conceptos.</returns>
    Task<List<ConceptoListItem>> GetConceptosAsync();

    /// <summary>
    /// Obtiene la TRM USD/COP para una fecha dada.
    /// </summary>
    /// <param name="fecha">Fecha de consulta.</param>
    /// <returns>TRM o null si no existe.</returns>
    Task<decimal?> GetTrmAsync(DateTime fecha);

    // Métodos legacy (mantener compatibilidad)
    Task<Recibo> CrearBorradorAsync(Guid? miembroId, string? terceroLibre, IEnumerable<(string codigoConcepto, int cantidad)> items, string user, CancellationToken ct = default);
    Task<Recibo> EmitirAsync(Guid reciboId, string user, CancellationToken ct);
}
