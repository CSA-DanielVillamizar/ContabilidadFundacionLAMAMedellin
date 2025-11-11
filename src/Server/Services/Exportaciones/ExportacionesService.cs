using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Services.Exportaciones;

/// <summary>
/// Servicio para exportar datos a Excel
/// </summary>
public class ExportacionesService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public ExportacionesService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Exporta recibos a Excel con filtros de fecha
    /// </summary>
    public async Task<byte[]> ExportarRecibosAsync(DateTime? desde = null, DateTime? hasta = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Consulta base
        var query = context.Recibos
            .Include(r => r.Miembro)
            .Include(r => r.Items)
                .ThenInclude(i => i.Concepto)
            .AsQueryable();

        // Aplicar filtros
        if (desde.HasValue)
            query = query.Where(r => r.FechaEmision >= desde.Value);
        
        if (hasta.HasValue)
            query = query.Where(r => r.FechaEmision <= hasta.Value);

        var recibos = await query
            .OrderByDescending(r => r.FechaEmision)
            .ToListAsync();

        // Crear libro Excel
        using var workbook = new XLWorkbook();
        
        // Hoja 1: Resumen de Recibos
        var wsResumen = workbook.Worksheets.Add("Recibos");
        
        // Encabezados
        wsResumen.Cell(1, 1).Value = "Número";
        wsResumen.Cell(1, 2).Value = "Fecha Emisión";
        wsResumen.Cell(1, 3).Value = "Miembro";
        wsResumen.Cell(1, 4).Value = "Documento";
        wsResumen.Cell(1, 5).Value = "Estado";
        wsResumen.Cell(1, 6).Value = "Total COP";
        wsResumen.Cell(1, 7).Value = "Observaciones";

        // Formatear encabezados
        var headerRange = wsResumen.Range(1, 1, 1, 7);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Datos
        int row = 2;
        foreach (var recibo in recibos)
        {
            wsResumen.Cell(row, 1).Value = $"{recibo.Serie}-{recibo.Ano}-{recibo.Consecutivo}";
            wsResumen.Cell(row, 2).Value = recibo.FechaEmision;
            wsResumen.Cell(row, 3).Value = recibo.Miembro?.NombreCompleto ?? "";
            wsResumen.Cell(row, 4).Value = recibo.Miembro?.Documento ?? "";
            wsResumen.Cell(row, 5).Value = recibo.Estado.ToString();
            wsResumen.Cell(row, 6).Value = recibo.TotalCop;
            wsResumen.Cell(row, 7).Value = recibo.Observaciones ?? "";

            // Formato de fecha
            wsResumen.Cell(row, 2).Style.DateFormat.Format = "dd/MM/yyyy";
            
            // Formato de moneda
            wsResumen.Cell(row, 6).Style.NumberFormat.Format = "$#,##0";

            // Color según estado
            if (recibo.Estado == EstadoRecibo.Anulado)
            {
                wsResumen.Row(row).Style.Font.FontColor = XLColor.Red;
                wsResumen.Row(row).Style.Font.Strikethrough = true;
            }

            row++;
        }

        // Ajustar ancho de columnas
        wsResumen.Columns().AdjustToContents();

        // Hoja 2: Detalle de Items
        var wsDetalle = workbook.Worksheets.Add("Detalle Items");
        
        // Encabezados detalle
        wsDetalle.Cell(1, 1).Value = "Número Recibo";
        wsDetalle.Cell(1, 2).Value = "Fecha";
        wsDetalle.Cell(1, 3).Value = "Miembro";
        wsDetalle.Cell(1, 4).Value = "Concepto";
        wsDetalle.Cell(1, 5).Value = "Código";
        wsDetalle.Cell(1, 6).Value = "Cantidad";
        wsDetalle.Cell(1, 7).Value = "Precio Unitario";
        wsDetalle.Cell(1, 8).Value = "Moneda";
        wsDetalle.Cell(1, 9).Value = "TRM";
        wsDetalle.Cell(1, 10).Value = "Subtotal COP";

        // Formatear encabezados
        var headerRangeDetalle = wsDetalle.Range(1, 1, 1, 10);
        headerRangeDetalle.Style.Font.Bold = true;
        headerRangeDetalle.Style.Fill.BackgroundColor = XLColor.LightBlue;
        headerRangeDetalle.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Datos de items
        row = 2;
        foreach (var recibo in recibos)
        {
            foreach (var item in recibo.Items)
            {
                wsDetalle.Cell(row, 1).Value = $"{recibo.Serie}-{recibo.Ano}-{recibo.Consecutivo}";
                wsDetalle.Cell(row, 2).Value = recibo.FechaEmision;
                wsDetalle.Cell(row, 3).Value = recibo.Miembro?.NombreCompleto ?? "";
                wsDetalle.Cell(row, 4).Value = item.Concepto?.Nombre ?? "";
                wsDetalle.Cell(row, 5).Value = item.Concepto?.Codigo ?? "";
                wsDetalle.Cell(row, 6).Value = item.Cantidad;
                wsDetalle.Cell(row, 7).Value = item.PrecioUnitarioMonedaOrigen;
                wsDetalle.Cell(row, 8).Value = item.MonedaOrigen.ToString();
                wsDetalle.Cell(row, 9).Value = item.TrmAplicada ?? 1;
                wsDetalle.Cell(row, 10).Value = item.SubtotalCop;

                // Formatos
                wsDetalle.Cell(row, 2).Style.DateFormat.Format = "dd/MM/yyyy";
                wsDetalle.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
                wsDetalle.Cell(row, 9).Style.NumberFormat.Format = "#,##0.0000";
                wsDetalle.Cell(row, 10).Style.NumberFormat.Format = "$#,##0";

                if (recibo.Estado == EstadoRecibo.Anulado)
                {
                    wsDetalle.Row(row).Style.Font.FontColor = XLColor.Red;
                    wsDetalle.Row(row).Style.Font.Strikethrough = true;
                }

                row++;
            }
        }

        // Ajustar ancho de columnas
        wsDetalle.Columns().AdjustToContents();

        // Guardar en memoria
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Exporta egresos a Excel con filtros de fecha
    /// </summary>
    public async Task<byte[]> ExportarEgresosAsync(DateTime? desde = null, DateTime? hasta = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.Egresos.AsQueryable();

        if (desde.HasValue)
            query = query.Where(e => e.Fecha >= desde.Value);
        
        if (hasta.HasValue)
            query = query.Where(e => e.Fecha <= hasta.Value);

        var egresos = await query
            .OrderByDescending(e => e.Fecha)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Egresos");

        // Encabezados
        ws.Cell(1, 1).Value = "Fecha";
        ws.Cell(1, 2).Value = "Categoría";
        ws.Cell(1, 3).Value = "Proveedor";
        ws.Cell(1, 4).Value = "Descripción";
        ws.Cell(1, 5).Value = "Valor COP";
        ws.Cell(1, 6).Value = "Usuario";
        ws.Cell(1, 7).Value = "Soporte";

        var headerRange = ws.Range(1, 1, 1, 7);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightCoral;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        int row = 2;
        foreach (var egreso in egresos)
        {
            ws.Cell(row, 1).Value = egreso.Fecha;
            ws.Cell(row, 2).Value = egreso.Categoria ?? "";
            ws.Cell(row, 3).Value = egreso.Proveedor ?? "";
            ws.Cell(row, 4).Value = egreso.Descripcion ?? "";
            ws.Cell(row, 5).Value = egreso.ValorCop;
            ws.Cell(row, 6).Value = egreso.UsuarioRegistro ?? "";
            ws.Cell(row, 7).Value = egreso.SoporteUrl ?? "";

            ws.Cell(row, 1).Style.DateFormat.Format = "dd/MM/yyyy";
            ws.Cell(row, 5).Style.NumberFormat.Format = "$#,##0";

            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
