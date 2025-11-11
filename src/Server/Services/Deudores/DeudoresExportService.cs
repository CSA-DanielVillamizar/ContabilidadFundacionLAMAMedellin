using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Server.Data;
using Server.Services.Exchange;
using Microsoft.AspNetCore.Hosting;

namespace Server.Services.Deudores;

/// <summary>
/// Implementación del servicio de exportaciones (Excel, PDF) para deudores.
/// Aplica Clean Architecture separando lógica de negocio de presentación.
/// </summary>
public class DeudoresExportService : IDeudoresExportService
{
    private readonly IDeudoresService _deudores;
    private readonly AppDbContext _db;
    private readonly IExchangeRateService _trm;
    private readonly IWebHostEnvironment _env;

    public DeudoresExportService(IDeudoresService deudores, AppDbContext db, IExchangeRateService trm, IWebHostEnvironment env)
    {
        _deudores = deudores;
        _db = db;
        _trm = trm;
        _env = env;
    }

    public async Task<byte[]> GenerarExcelAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        var rows = await _deudores.CalcularAsync(desde, hasta, ct);
        var mensualidad = await _db.Conceptos.FirstOrDefaultAsync(c => c.Codigo == "MENSUALIDAD", ct);
        decimal precioMensualDisplayCop = 0;
        bool esUsd = false;
        if (mensualidad is not null)
        {
            esUsd = mensualidad.Moneda == Server.Models.Moneda.USD;
            if (esUsd)
            {
                var refDate = hasta ?? DateOnly.FromDateTime(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1));
                var trm = await _trm.GetUsdCopAsync(refDate, ct);
                precioMensualDisplayCop = decimal.Round(mensualidad.PrecioBase * trm, 2);
            }
            else precioMensualDisplayCop = mensualidad.PrecioBase;
        }

        var trmPorMes = new Dictionary<DateOnly, decimal>();
        if (esUsd && mensualidad is not null)
        {
            var mesesUnicos = rows.SelectMany(r => r.MesesPendientes).Distinct();
            foreach (var m in mesesUnicos)
            {
                var trm = await _trm.GetUsdCopAsync(m, ct);
                trmPorMes[m] = trm;
            }
        }

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Deudores");
        ws.Cell(1, 1).Value = "Miembro";
        ws.Cell(1, 2).Value = "Ingreso";
        ws.Cell(1, 3).Value = "Meses";
        ws.Cell(1, 4).Value = "Detalle";
        ws.Cell(1, 5).Value = "Total Estimado (COP)";

        int rIdx = 2;
        foreach (var r in rows)
        {
            decimal total = 0;
            if (mensualidad is not null)
            {
                if (esUsd)
                {
                    foreach (var m in r.MesesPendientes)
                    {
                        var trm = trmPorMes.TryGetValue(m, out var trmVal) ? trmVal : 0;
                        total += decimal.Round(mensualidad.PrecioBase * trm, 2);
                    }
                }
                else
                {
                    total = r.MesesPendientes.Count * mensualidad.PrecioBase;
                }
            }
            ws.Cell(rIdx, 1).Value = r.Nombre;
            ws.Cell(rIdx, 2).Value = r.Ingreso?.ToString();
            ws.Cell(rIdx, 3).Value = r.MesesPendientes.Count;
            ws.Cell(rIdx, 4).Value = string.Join(", ", r.MesesPendientes.Select(m => m.ToString("yyyy-MM")));
            ws.Cell(rIdx, 5).Value = total;
            rIdx++;
        }
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> GenerarPdfAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        var rows = await _deudores.CalcularAsync(desde, hasta, ct);
        var mensualidad = await _db.Conceptos.FirstOrDefaultAsync(c => c.Codigo == "MENSUALIDAD", ct);
        decimal precioMensualDisplayCop = 0;
        bool esUsd = false;
        if (mensualidad is not null)
        {
            esUsd = mensualidad.Moneda == Server.Models.Moneda.USD;
            if (esUsd)
            {
                var refDate = hasta ?? DateOnly.FromDateTime(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1));
                var trm = await _trm.GetUsdCopAsync(refDate, ct);
                precioMensualDisplayCop = decimal.Round(mensualidad.PrecioBase * trm, 2);
            }
            else precioMensualDisplayCop = mensualidad.PrecioBase;
        }

        var trmPorMes = new Dictionary<DateOnly, decimal>();
        if (esUsd && mensualidad is not null)
        {
            var mesesUnicos = rows.SelectMany(r => r.MesesPendientes).Distinct();
            foreach (var m in mesesUnicos)
            {
                var trm = await _trm.GetUsdCopAsync(m, ct);
                trmPorMes[m] = trm;
            }
        }

        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var logoPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "images", "LogoLAMAMedellin.png");
        var pdf = Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                    page.Margin(40);
                
                    // Encabezado mejorado con diseño profesional
                    page.Header().Column(headerCol =>
                {
                        headerCol.Item().Row(row =>
                    {
                            // Logo a la izquierda (si existe)
                            if (File.Exists(logoPath))
                            {
                                row.ConstantItem(64).Height(64).AlignLeft().AlignMiddle().Image(logoPath).FitArea();
                            }

                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("FUNDACIÓN L.A.M.A.").Bold().FontSize(18).FontColor("#1e3a8a");
                                col.Item().Text("MEDELLÍN").Bold().FontSize(18).FontColor("#1e3a8a");
                                col.Item().PaddingTop(5).Text("REPORTE DE DEUDORES").FontSize(14).FontColor("#64748b");
                            });
                    });
                    
                        // Badge con información del período
                        headerCol.Item().PaddingTop(15).Background("#dbeafe").Border(1).BorderColor("#3b82f6").Padding(12).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text(text =>
                                {
                                    text.Span("Período: ").FontSize(11).FontColor("#64748b");
                                    text.Span($"{(desde?.ToString("MMM yyyy") ?? "Inicio")} - {(hasta?.ToString("MMM yyyy") ?? "Fin")}").Bold().FontSize(12).FontColor("#1e3a8a");
                                });
                                col.Item().Text(text =>
                                {
                                    text.Span("Generado: ").FontSize(10).FontColor("#64748b");
                                    text.Span($"{DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10).FontColor("#334155");
                                });
                            });
                            row.RelativeItem().AlignRight().Background("#ef4444").Padding(8).AlignCenter().Text("⚠ DEUDAS PENDIENTES").Bold().FontSize(11).FontColor("#ffffff");
                        });
                    
                        headerCol.Item().PaddingTop(10).LineHorizontal(2).LineColor("#1e3a8a");
                });
                page.Content().Column(col =>
                {
                        // Información de referencia con diseño mejorado
                        col.Item().PaddingVertical(15).Background("#fffbeb").Border(1).BorderColor("#fde047").Padding(12).Row(row =>
                        {
                            row.ConstantItem(20).Text("💰").FontSize(14);
                            row.RelativeItem().PaddingLeft(8).Column(infoCol =>
                            {
                                infoCol.Item().Text("INFORMACIÓN DE REFERENCIA").Bold().FontSize(11).FontColor("#92400e");
                                infoCol.Item().PaddingTop(5).Text($"Precio mensual: {precioMensualDisplayCop:N0} COP").FontSize(10).FontColor("#78350f");
                            });
                        });
                    
                        // Título de la tabla
                        col.Item().PaddingTop(20).Row(row =>
                        {
                            row.RelativeItem().Text("LISTADO DE MIEMBROS DEUDORES").Bold().FontSize(13).FontColor("#1e3a8a");
                            row.ConstantItem(80).Background("#ef4444").Padding(5).AlignCenter().Text("⚠️").FontSize(12);
                        });
                        col.Item().PaddingTop(5).LineHorizontal(2).LineColor("#1e3a8a");
                    
                        // Tabla con diseño profesional
                        col.Item().PaddingTop(10).Border(1).BorderColor("#cbd5e1").Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(5);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(7);
                            c.RelativeColumn(3);
                        });
                        
                            // Encabezado con fondo rojo (deudores)
                        t.Header(h =>
                        {
                                h.Cell().Background("#ef4444").Padding(10).Text("MIEMBRO").Bold().FontSize(10).FontColor("#ffffff");
                                h.Cell().Background("#ef4444").Padding(10).AlignCenter().Text("INGRESO").Bold().FontSize(10).FontColor("#ffffff");
                                h.Cell().Background("#ef4444").Padding(10).AlignCenter().Text("MESES").Bold().FontSize(10).FontColor("#ffffff");
                                h.Cell().Background("#ef4444").Padding(10).Text("DETALLE").Bold().FontSize(10).FontColor("#ffffff");
                                h.Cell().Background("#ef4444").Padding(10).AlignRight().Text("TOTAL (COP)").Bold().FontSize(10).FontColor("#ffffff");
                        });

                        decimal totalGlobal = 0;
                            bool alternar = false;
                        foreach (var r in rows)
                        {
                            decimal total = 0;
                            if (mensualidad is not null)
                            {
                                if (esUsd)
                                {
                                    foreach (var m in r.MesesPendientes)
                                    {
                                        var trm = trmPorMes.TryGetValue(m, out var trmVal) ? trmVal : 0;
                                        total += decimal.Round(mensualidad.PrecioBase * trm, 2);
                                    }
                                }
                                else total = r.MesesPendientes.Count * mensualidad.PrecioBase;
                            }
                            totalGlobal += total;

                                // Filas con alternancia de colores y separadores
                                t.Cell().Background(alternar ? "#fef2f2" : "#ffffff").BorderBottom(1).BorderColor("#fecaca").Padding(10).Text(r.Nombre).FontSize(9).FontColor("#334155");
                                t.Cell().Background(alternar ? "#fef2f2" : "#ffffff").BorderBottom(1).BorderColor("#fecaca").Padding(10).AlignCenter().Text(r.Ingreso?.ToString("MMM yyyy") ?? "-").FontSize(9).FontColor("#64748b");
                                t.Cell().Background(alternar ? "#fef2f2" : "#ffffff").BorderBottom(1).BorderColor("#fecaca").Padding(10).AlignCenter().Text(r.MesesPendientes.Count.ToString()).FontSize(9).FontColor("#334155").Bold();
                                t.Cell().Background(alternar ? "#fef2f2" : "#ffffff").BorderBottom(1).BorderColor("#fecaca").Padding(10).Text(string.Join(", ", r.MesesPendientes.Select(m => m.ToString("MMM yyyy")))).FontSize(8).FontColor("#64748b");
                                t.Cell().Background(alternar ? "#fef2f2" : "#ffffff").BorderBottom(1).BorderColor("#fecaca").Padding(10).AlignRight().Text(total.ToString("N0")).FontSize(9).FontColor("#dc2626").Bold();
                                alternar = !alternar;
                        }

                            // Fila de totales con diseño destacado
                            t.Cell().ColumnSpan(4).Background("#ef4444").Padding(12).Text("TOTAL GENERAL").Bold().FontSize(11).FontColor("#ffffff");
                            t.Cell().Background("#ef4444").Padding(12).AlignRight().Text(rows.Sum(r =>
                        {
                            decimal s = 0;
                            if (mensualidad is null) return 0m;
                            if (esUsd)
                                foreach (var m in r.MesesPendientes) s += decimal.Round(mensualidad.PrecioBase * trmPorMes[m], 2);
                            else s = r.MesesPendientes.Count * mensualidad.PrecioBase;
                            return s;
                            }).ToString("N0")).Bold().FontSize(11).FontColor("#ffffff");
                    });
                    
                        // Resumen estadístico
                        col.Item().PaddingTop(20).Background("#dcfce7").Border(1).BorderColor("#86efac").Padding(15).Column(statsCol =>
                        {
                            statsCol.Item().Row(row =>
                            {
                                row.ConstantItem(20).Text("📊").FontSize(14);
                                row.RelativeItem().PaddingLeft(8).Text("RESUMEN ESTADÍSTICO").Bold().FontSize(12).FontColor("#166534");
                            });
                            statsCol.Item().PaddingTop(8).LineHorizontal(1).LineColor("#86efac");
                            statsCol.Item().PaddingTop(8).Row(row =>
                            {
                                row.RelativeItem().Text(text =>
                                {
                                    text.Span("Total de deudores: ").FontSize(10).FontColor("#166534");
                                    text.Span($"{rows.Count}").Bold().FontSize(11).FontColor("#15803d");
                                });
                                row.RelativeItem().Text(text =>
                                {
                                    text.Span("Total meses pendientes: ").FontSize(10).FontColor("#166534");
                                    text.Span($"{rows.Sum(r => r.MesesPendientes.Count)}").Bold().FontSize(11).FontColor("#15803d");
                                });
                            });
                        });
                });

                    // Pie de página profesional mejorado
                    page.Footer().Column(footerCol =>
                    {
                        footerCol.Item().LineHorizontal(2).LineColor("#1e3a8a");
                        footerCol.Item().PaddingTop(8).Background("#f8fafc").Padding(8).Row(row =>
                        {
                            row.RelativeItem().AlignLeft().Text(text =>
                            {
                                text.Span("Fundación L.A.M.A. Medellín").Bold().FontSize(9).FontColor("#1e3a8a");
                                text.Span(" | ").FontSize(8).FontColor("#cbd5e1");
                                text.Span("Medellín, Colombia").FontSize(8).FontColor("#64748b");
                            });
                            row.RelativeItem().AlignCenter().Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(7).FontColor("#94a3b8");
                            row.RelativeItem().AlignRight().Background("#dbeafe").Padding(5).Text(text =>
                            {
                                text.Span("Pág. ").FontSize(8).FontColor("#1e40af");
                                text.CurrentPageNumber().Bold().FontSize(8).FontColor("#1e3a8a");
                                text.Span("/").FontSize(8).FontColor("#1e40af");
                                text.TotalPages().Bold().FontSize(8).FontColor("#1e3a8a");
                            });
                        });
                    });
            });
        }).GeneratePdf();

        return pdf;
    }
}
