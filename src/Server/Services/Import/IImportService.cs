using Microsoft.AspNetCore.Components.Forms;
using Server.DTOs.Import;

namespace Server.Services.Import;

public interface IImportService
{
    Task<(List<ImportRowDto> rows, List<string> mensajes)> PreviewAsync(IBrowserFile file, CancellationToken ct = default);
    Task<ImportResult> ImportAsync(IEnumerable<ImportRowDto> rows, string currentUser, CancellationToken ct = default);
}
