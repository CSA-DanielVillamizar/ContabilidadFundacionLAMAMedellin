using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Services.Miembros;

/// <summary>
/// Implementación del servicio de exportación de miembros usando ClosedXML.
/// </summary>
public class MiembrosExportService : IMiembrosExportService
{
    private readonly AppDbContext _db;

    public MiembrosExportService(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportToExcelAsync(string? query, EstadoMiembro? estado)
    {
        // Obtener datos filtrados
        var q = _db.Miembros.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var txt = query.Trim();
            q = q.Where(m => m.NombreCompleto.Contains(txt) || 
                           m.Nombres.Contains(txt) || 
                           m.Apellidos.Contains(txt) || 
                           m.Documento.Contains(txt) || 
                           m.Cedula.Contains(txt));
        }

        if (estado.HasValue)
        {
            q = q.Where(m => m.Estado == estado.Value);
        }

        var miembros = await q
            .OrderBy(m => m.NumeroSocio ?? 9999)
            .ThenBy(m => m.NombreCompleto)
            .ToListAsync();

        // Crear workbook de Excel
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Miembros");

        // Configurar encabezados
        worksheet.Cell(1, 1).Value = "Número Socio";
        worksheet.Cell(1, 2).Value = "Nombre Completo";
        worksheet.Cell(1, 3).Value = "Nombres";
        worksheet.Cell(1, 4).Value = "Apellidos";
        worksheet.Cell(1, 5).Value = "Cédula";
        worksheet.Cell(1, 6).Value = "Email";
        worksheet.Cell(1, 7).Value = "Celular";
        worksheet.Cell(1, 8).Value = "Dirección";
        worksheet.Cell(1, 9).Value = "Cargo";
        worksheet.Cell(1, 10).Value = "Rango";
        worksheet.Cell(1, 11).Value = "Estado";
        worksheet.Cell(1, 12).Value = "Fecha Ingreso";

        // Estilo de encabezados
        var headerRange = worksheet.Range(1, 1, 1, 12);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563eb");
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Llenar datos
        int row = 2;
        foreach (var miembro in miembros)
        {
            worksheet.Cell(row, 1).Value = miembro.NumeroSocio?.ToString() ?? "";
            worksheet.Cell(row, 2).Value = miembro.NombreCompleto;
            worksheet.Cell(row, 3).Value = miembro.Nombres;
            worksheet.Cell(row, 4).Value = miembro.Apellidos;
            worksheet.Cell(row, 5).Value = miembro.Cedula;
            worksheet.Cell(row, 6).Value = miembro.Email;
            worksheet.Cell(row, 7).Value = miembro.Celular;
            worksheet.Cell(row, 8).Value = miembro.Direccion;
            worksheet.Cell(row, 9).Value = miembro.Cargo;
            worksheet.Cell(row, 10).Value = miembro.Rango;
            worksheet.Cell(row, 11).Value = miembro.Estado.ToString();
            worksheet.Cell(row, 12).Value = miembro.FechaIngreso?.ToString("dd/MM/yyyy") ?? "";

            row++;
        }

        // Auto-ajustar columnas
        worksheet.Columns().AdjustToContents();

        // Aplicar bordes a toda la tabla
        var dataRange = worksheet.Range(1, 1, row - 1, 12);
        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        // Convertir a bytes
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
