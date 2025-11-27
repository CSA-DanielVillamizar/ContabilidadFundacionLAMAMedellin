using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Server.Data;
using Server.DTOs.CuentasCobro;
using Server.Services.Deudores;
using Server.Models;
using System.Globalization;

namespace Server.Services.CuentasCobro;

/// <summary>
/// Implementación del servicio de cuentas de cobro.
/// </summary>
public class CuentasCobroService : ICuentasCobroService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IDeudoresService _deudoresService;
    private readonly string _wwwrootPath;

    public CuentasCobroService(
        IDbContextFactory<AppDbContext> dbFactory,
        IDeudoresService deudoresService,
        Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
    {
        _dbFactory = dbFactory;
        _deudoresService = deudoresService;
        _wwwrootPath = env.WebRootPath ?? "wwwroot";
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <inheritdoc />
    public async Task<CuentaCobroDto?> ObtenerDatosCuentaCobroAsync(Guid miembroId, DateOnly? desde = null, DateOnly? hasta = null, CancellationToken ct = default)
    {
        await using var _db = await _dbFactory.CreateDbContextAsync(ct);

        // Obtener datos del miembro
            var miembro = await _db.Miembros
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == miembroId, ct);
        if (miembro == null) return null;

        // Obtener detalle de deuda desde DeudoresService
        var detalle = await _deudoresService.ObtenerDetalleAsync(miembroId, desde, hasta, ct);
        if (detalle == null || detalle.MesesPendientes.Count == 0) return null;

        // Obtener precio de mensualidad vigente
        var precioMensualidad = await _deudoresService.ObtenerPrecioMensualidadCopAsync(hasta, ct);

        // Construir items de la cuenta de cobro
        var items = new List<ItemCuentaCobro>();
        var cultura = new CultureInfo("es-CO");

        foreach (var mes in detalle.MesesPendientes.OrderBy(m => m))
        {
            var nombreMes = mes.ToString("MMMM yyyy", cultura);
            nombreMes = char.ToUpper(nombreMes[0]) + nombreMes.Substring(1); // Capitalizar

            items.Add(new ItemCuentaCobro
            {
                Descripcion = $"Mensualidad {nombreMes}",
                Cantidad = 1,
                PrecioUnitarioCop = precioMensualidad,
                SubtotalCop = precioMensualidad,
                MesReferencia = mes
            });
        }

        var totalCop = items.Sum(i => i.SubtotalCop);

        return new CuentaCobroDto
        {
            MiembroId = miembro.Id,
            NombreCompleto = miembro.NombreCompleto ?? $"{miembro.Nombres} {miembro.Apellidos}",
            NumeroSocio = miembro.NumeroSocio ?? 0,
            FechaIngreso = miembro.FechaIngreso,
            Email = miembro.Email,
            Telefono = miembro.Celular ?? miembro.Telefono,
            Items = items,
            TotalCop = totalCop,
            FechaGeneracion = DateTime.UtcNow,
            Consecutivo = $"{DateTime.UtcNow.Year}-{miembroId.ToString().Substring(0, 8).ToUpper()}"
        };
    }

    /// <inheritdoc />
    public async Task<byte[]> GenerarCuentaCobroPdfAsync(Guid miembroId, DateOnly? desde = null, DateOnly? hasta = null, CancellationToken ct = default)
    {
        var datos = await ObtenerDatosCuentaCobroAsync(miembroId, desde, hasta, ct);
        if (datos == null)
            throw new InvalidOperationException("No se encontró información de deuda para el miembro especificado.");

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(container =>
                {
                    var logoPath = Path.Combine(_wwwrootPath, "images", "LogoLAMAMedellin.png");
                    
                    container.Column(column =>
                    {
                        // Logo (si existe)
                        if (File.Exists(logoPath))
                        {
                            column.Item().AlignCenter().Width(120).Image(logoPath).FitArea();
                            column.Item().PaddingVertical(5);
                        }

                        // Título
                        column.Item().AlignCenter().Text("CUENTA DE COBRO")
                            .Bold().FontSize(16).FontColor("#1e3a8a");
                        
                        column.Item().AlignCenter().Text($"N° {datos.Consecutivo}")
                            .FontSize(10).FontColor("#64748b");

                        column.Item().PaddingVertical(10).LineHorizontal(1).LineColor("#e2e8f0");
                    });
                });

                page.Content().Column(column =>
                {
                    column.Spacing(10);

                    // Información de LAMA Medellín
                    column.Item().Background("#f8fafc").Padding(12).Column(col =>
                    {
                        col.Item().Text(txt => txt.Span("Fundación L.A.M.A. Medellín").Bold().FontSize(12).FontColor("#1e40af"));
                        col.Item().Text(txt => txt.Span("Latin American Motorcycle Association - Capítulo Medellín").FontSize(9).FontColor("#64748b"));
                        col.Item().PaddingTop(5).Text(txt =>
                        {
                            txt.Span("Fecha de emisión: ").SemiBold();
                            txt.Span(datos.FechaGeneracion.ToString("dd/MM/yyyy"));
                        });
                    });

                    // Información del miembro deudor
                    column.Item().PaddingTop(10).Text(txt => txt.Span("DETALLES DEL MIEMBRO").Bold().FontSize(11).FontColor("#334155"));
                    column.Item().Background("#ffffff").Border(1).BorderColor("#e2e8f0").Padding(12).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("Nombre: ").SemiBold();
                                txt.Span(datos.NombreCompleto);
                            });
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("N° Socio: ").SemiBold();
                                txt.Span(datos.NumeroSocio.ToString());
                            });
                        });

                        if (datos.FechaIngreso.HasValue)
                        {
                            col.Item().PaddingTop(5).Text(txt =>
                            {
                                txt.Span("Fecha de ingreso: ").SemiBold();
                                txt.Span(datos.FechaIngreso.Value.ToString("dd/MM/yyyy"));
                            });
                        }

                        if (!string.IsNullOrWhiteSpace(datos.Email))
                        {
                            col.Item().PaddingTop(3).Text(txt =>
                            {
                                txt.Span("Email: ").SemiBold();
                                txt.Span(datos.Email);
                            });
                        }

                        if (!string.IsNullOrWhiteSpace(datos.Telefono))
                        {
                            col.Item().PaddingTop(3).Text(txt =>
                            {
                                txt.Span("Teléfono: ").SemiBold();
                                txt.Span(datos.Telefono);
                            });
                        }
                    });

                    // Tabla de conceptos adeudados
                    column.Item().PaddingTop(15).Text(txt => txt.Span("CONCEPTOS ADEUDADOS").Bold().FontSize(11).FontColor("#334155"));
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Descripción
                            columns.RelativeColumn(1); // Cantidad
                            columns.RelativeColumn(1.5f); // Precio Unit.
                            columns.RelativeColumn(1.5f); // Subtotal
                        });

                        // Encabezado
                        table.Header(header =>
                        {
                            header.Cell().Background("#1e3a8a").Padding(8).Text(txt => txt.Span("Descripción").Bold().FontColor("#ffffff").FontSize(9));
                            header.Cell().Background("#1e3a8a").Padding(8).AlignCenter().Text(txt => txt.Span("Cant.").Bold().FontColor("#ffffff").FontSize(9));
                            header.Cell().Background("#1e3a8a").Padding(8).AlignRight().Text(txt => txt.Span("Precio Unit.").Bold().FontColor("#ffffff").FontSize(9));
                            header.Cell().Background("#1e3a8a").Padding(8).AlignRight().Text(txt => txt.Span("Subtotal").Bold().FontColor("#ffffff").FontSize(9));
                        });

                        // Filas de items
                        foreach (var item in datos.Items)
                        {
                            table.Cell().Background("#f8fafc").BorderBottom(1).BorderColor("#e2e8f0").Padding(8)
                                .Text(txt => txt.Span(item.Descripcion).FontSize(9));
                            table.Cell().Background("#f8fafc").BorderBottom(1).BorderColor("#e2e8f0").Padding(8)
                                .AlignCenter().Text(txt => txt.Span(item.Cantidad.ToString()).FontSize(9));
                            table.Cell().Background("#f8fafc").BorderBottom(1).BorderColor("#e2e8f0").Padding(8)
                                .AlignRight().Text(txt => txt.Span(item.PrecioUnitarioCop.ToString("C0", new CultureInfo("es-CO"))).FontSize(9));
                            table.Cell().Background("#f8fafc").BorderBottom(1).BorderColor("#e2e8f0").Padding(8)
                                .AlignRight().Text(txt => txt.Span(item.SubtotalCop.ToString("C0", new CultureInfo("es-CO"))).FontSize(9));
                        }

                        // Fila de total
                        table.Cell().ColumnSpan(3).Background("#1e40af").Padding(10)
                            .AlignRight().Text(txt => txt.Span("TOTAL A PAGAR:").Bold().FontColor("#ffffff").FontSize(10));
                        table.Cell().Background("#1e40af").Padding(10)
                            .AlignRight().Text(txt => txt.Span(datos.TotalCop.ToString("C0", new CultureInfo("es-CO"))).Bold().FontColor("#ffffff").FontSize(11));
                    });

                    // Instrucciones de pago
                    column.Item().PaddingTop(20).Background("#fffbeb").Border(1).BorderColor("#fbbf24").Padding(12).Column(col =>
                    {
                        col.Item().Text(txt => txt.Span("INSTRUCCIONES DE PAGO").Bold().FontSize(10).FontColor("#92400e"));
                        col.Item().PaddingTop(5).Text(txt => txt.Span("Por favor realice el pago a través de los siguientes medios:").FontSize(9).FontColor("#78350f"));
                        col.Item().PaddingTop(8).Text(txt =>
                        {
                            txt.Span("• Transferencia bancaria: ").SemiBold().FontSize(9);
                            txt.Span("[Cuenta bancaria LAMA - Próximamente]").FontSize(9);
                        });
                        col.Item().PaddingTop(3).Text(txt =>
                        {
                            txt.Span("• PSE o pago en línea: ").SemiBold().FontSize(9);
                            txt.Span("[Portal de pagos - Próximamente]").FontSize(9);
                        });
                        col.Item().PaddingTop(3).Text(txt =>
                        {
                            txt.Span("• Efectivo: ").SemiBold().FontSize(9);
                            txt.Span("En tesorería de LAMA Medellín").FontSize(9);
                        });
                        col.Item().PaddingTop(8).Text(txt => txt.Span("Una vez realizado el pago, por favor enviar comprobante a tesorería +57 310 5127314.").Italic().FontSize(8).FontColor("#92400e"));
                    });
                });

                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("Documento generado automáticamente por el sistema de gestión LAMA Medellín • ").FontSize(7).FontColor("#94a3b8");
                    txt.Span(DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm")).FontSize(7).FontColor("#94a3b8");
                });
            });
        });

        return pdf.GeneratePdf();
    }

    /// <summary>
    /// Genera una cuenta de cobro en PDF a partir de una venta a MiembroLocal.
    /// </summary>
    public async Task<byte[]> GenerarCuentaCobroDesdeVentaPdfAsync(Guid ventaId, CancellationToken ct = default)
    {
        await using var _db = await _dbFactory.CreateDbContextAsync(ct);

        var venta = await _db.VentasProductos
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .Include(v => v.Miembro)
            .FirstOrDefaultAsync(v => v.Id == ventaId, ct);

        if (venta == null)
            throw new InvalidOperationException("No se encontró la venta especificada.");

        if (venta.TipoCliente != TipoCliente.MiembroLocal || !venta.MiembroId.HasValue)
            throw new InvalidOperationException("Solo se pueden generar cuentas de cobro desde ventas a Miembro Local.");

        if (venta.Detalles == null || venta.Detalles.Count == 0)
            throw new InvalidOperationException("La venta no tiene detalles.");

        var miembro = venta.Miembro ?? await _db.Miembros.FirstOrDefaultAsync(m => m.Id == venta.MiembroId.Value, ct);
        if (miembro == null)
            throw new InvalidOperationException("No se encontró el miembro asociado a la venta.");

        var items = venta.Detalles.Select(d => new ItemCuentaCobro
        {
            Descripcion = $"{d.Producto.Nombre}" + (string.IsNullOrWhiteSpace(d.Producto.Talla) ? string.Empty : $" - Talla {d.Producto.Talla}") + (string.IsNullOrWhiteSpace(d.Notas) ? string.Empty : $" ({d.Notas})"),
            Cantidad = d.Cantidad,
            PrecioUnitarioCop = d.PrecioUnitarioCOP,
            SubtotalCop = d.SubtotalCOP
        }).ToList();

        var datos = new CuentaCobroDto
        {
            MiembroId = miembro.Id,
            NombreCompleto = miembro.NombreCompleto ?? $"{miembro.Nombres} {miembro.Apellidos}",
            NumeroSocio = miembro.NumeroSocio ?? 0,
            FechaIngreso = miembro.FechaIngreso,
            Email = miembro.Email,
            Telefono = miembro.Celular ?? miembro.Telefono,
            Items = items,
            TotalCop = items.Sum(i => i.SubtotalCop),
            FechaGeneracion = DateTime.UtcNow,
            Consecutivo = venta.NumeroVenta
        };

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(container =>
                {
                    var logoPath = Path.Combine(_wwwrootPath, "images", "LogoLAMAMedellin.png");
                    
                    container.Column(column =>
                    {
                        if (File.Exists(logoPath))
                        {
                            column.Item().AlignCenter().Width(120).Image(logoPath).FitArea();
                            column.Item().PaddingVertical(5);
                        }

                        column.Item().AlignCenter().Text("CUENTA DE COBRO")
                            .Bold().FontSize(16).FontColor("#1e3a8a");
                        
                        column.Item().AlignCenter().Text($"N° {datos.Consecutivo}")
                            .FontSize(10).FontColor("#64748b");

                        column.Item().PaddingVertical(10).LineHorizontal(1).LineColor("#e2e8f0");
                    });
                });

                page.Content().Column(column =>
                {
                    column.Spacing(10);

                    // Información de LAMA Medellín
                    column.Item().Background("#f8fafc").Padding(12).Column(col =>
                    {
                        col.Item().Text(txt => txt.Span("Fundación L.A.M.A. Medellín").Bold().FontSize(12).FontColor("#1e40af"));
                        col.Item().Text(txt => txt.Span("Latin American Motorcycle Association - Capítulo Medellín").FontSize(9).FontColor("#64748b"));
                        col.Item().PaddingTop(5).Text(txt =>
                        {
                            txt.Span("Fecha de emisión: ").SemiBold();
                            txt.Span(datos.FechaGeneracion.ToString("dd/MM/yyyy"));
                        });
                    });

                    // Información del miembro
                    column.Item().PaddingTop(10).Text(txt => txt.Span("DETALLES DEL MIEMBRO").Bold().FontSize(11).FontColor("#334155"));
                    column.Item().Background("#ffffff").Border(1).BorderColor("#e2e8f0").Padding(12).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("Nombre: ").SemiBold();
                                txt.Span(datos.NombreCompleto);
                            });
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("N° Socio: ").SemiBold();
                                txt.Span(datos.NumeroSocio.ToString());
                            });
                        });

                        if (datos.FechaIngreso.HasValue)
                        {
                            col.Item().PaddingTop(5).Text(txt =>
                            {
                                txt.Span("Fecha de ingreso: ").SemiBold();
                                txt.Span(datos.FechaIngreso.Value.ToString("dd/MM/yyyy"));
                            });
                        }

                        if (!string.IsNullOrWhiteSpace(datos.Email))
                        {
                            col.Item().PaddingTop(3).Text(txt =>
                            {
                                txt.Span("Email: ").SemiBold();
                                txt.Span(datos.Email);
                            });
                        }

                        if (!string.IsNullOrWhiteSpace(datos.Telefono))
                        {
                            col.Item().PaddingTop(3).Text(txt =>
                            {
                                txt.Span("Teléfono: ").SemiBold();
                                txt.Span(datos.Telefono);
                            });
                        }
                    });

                    // Tabla de conceptos
                    column.Item().PaddingTop(15).Text(txt => txt.Span("CONCEPTOS").Bold().FontSize(11).FontColor("#334155"));
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background("#1e3a8a").Padding(8).Text(txt => txt.Span("Descripción").Bold().FontColor("#ffffff").FontSize(9));
                            header.Cell().Background("#1e3a8a").Padding(8).AlignCenter().Text(txt => txt.Span("Cant.").Bold().FontColor("#ffffff").FontSize(9));
                            header.Cell().Background("#1e3a8a").Padding(8).AlignRight().Text(txt => txt.Span("Precio Unit.").Bold().FontColor("#ffffff").FontSize(9));
                            header.Cell().Background("#1e3a8a").Padding(8).AlignRight().Text(txt => txt.Span("Subtotal").Bold().FontColor("#ffffff").FontSize(9));
                        });

                        foreach (var item in datos.Items)
                        {
                            table.Cell().Background("#f8fafc").BorderBottom(1).BorderColor("#e2e8f0").Padding(8)
                                .Text(txt => txt.Span(item.Descripcion).FontSize(9));
                            table.Cell().Background("#f8fafc").BorderBottom(1).BorderColor("#e2e8f0").Padding(8)
                                .AlignCenter().Text(txt => txt.Span(item.Cantidad.ToString()).FontSize(9));
                            table.Cell().Background("#f8fafc").BorderBottom(1).BorderColor("#e2e8f0").Padding(8)
                                .AlignRight().Text(txt => txt.Span(item.PrecioUnitarioCop.ToString("C0", new CultureInfo("es-CO"))).FontSize(9));
                            table.Cell().Background("#f8fafc").BorderBottom(1).BorderColor("#e2e8f0").Padding(8)
                                .AlignRight().Text(txt => txt.Span(item.SubtotalCop.ToString("C0", new CultureInfo("es-CO"))).FontSize(9));
                        }

                        table.Cell().ColumnSpan(3).Background("#1e40af").Padding(10)
                            .AlignRight().Text(txt => txt.Span("TOTAL A PAGAR:").Bold().FontColor("#ffffff").FontSize(10));
                        table.Cell().Background("#1e40af").Padding(10)
                            .AlignRight().Text(txt => txt.Span(datos.TotalCop.ToString("C0", new CultureInfo("es-CO"))).Bold().FontColor("#ffffff").FontSize(11));
                    });

                    // Instrucciones de pago
                    column.Item().PaddingTop(20).Background("#fffbeb").Border(1).BorderColor("#fbbf24").Padding(12).Column(col =>
                    {
                        col.Item().Text(txt => txt.Span("INSTRUCCIONES DE PAGO").Bold().FontSize(10).FontColor("#92400e"));
                        col.Item().PaddingTop(5).Text(txt => txt.Span("Por favor realice el pago a través de los siguientes medios:").FontSize(9).FontColor("#78350f"));
                        col.Item().PaddingTop(8).Text(txt =>
                        {
                            txt.Span("• Transferencia bancaria: ").SemiBold().FontSize(9);
                            txt.Span("[Cuenta bancaria LAMA - Próximamente]").FontSize(9);
                        });
                        col.Item().PaddingTop(3).Text(txt =>
                        {
                            txt.Span("• PSE o pago en línea: ").SemiBold().FontSize(9);
                            txt.Span("[Portal de pagos - Próximamente]").FontSize(9);
                        });
                        col.Item().PaddingTop(3).Text(txt =>
                        {
                            txt.Span("• Efectivo: ").SemiBold().FontSize(9);
                            txt.Span("En tesorería de LAMA Medellín").FontSize(9);
                        });
                        col.Item().PaddingTop(8).Text(txt => txt.Span("Una vez realizado el pago, por favor enviar comprobante a tesorería +57 310 5127314.").Italic().FontSize(8).FontColor("#92400e"));
                    });
                });

                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("Documento generado automáticamente por el sistema de gestión LAMA Medellín • ").FontSize(7).FontColor("#94a3b8");
                    txt.Span(DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm")).FontSize(7).FontColor("#94a3b8");
                });
            });
        });

        return pdf.GeneratePdf();
    }

    /// <summary>
    /// Genera una cuenta de cobro en PDF a partir de items personalizados.
    /// </summary>
    public async Task<byte[]> GenerarCuentaCobroDesdeCustomPdfAsync(CuentaCobroCustomRequestDto request, CancellationToken ct = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (request.Items == null || request.Items.Count == 0)
            throw new InvalidOperationException("Debe proporcionar al menos un ítem para la cuenta de cobro.");

        await using var _db = await _dbFactory.CreateDbContextAsync(ct);

        var miembro = await _db.Miembros.FirstOrDefaultAsync(m => m.Id == request.MiembroId, ct);
        if (miembro == null)
            throw new InvalidOperationException("No se encontró el miembro especificado.");

        var items = request.Items.Select(i => new ItemCuentaCobro
        {
            Descripcion = i.Descripcion,
            Cantidad = i.Cantidad,
            PrecioUnitarioCop = i.PrecioUnitarioCop,
            SubtotalCop = i.SubtotalCop ?? (i.PrecioUnitarioCop * i.Cantidad)
        }).ToList();

        var datos = new CuentaCobroDto
        {
            MiembroId = miembro.Id,
            NombreCompleto = miembro.NombreCompleto ?? $"{miembro.Nombres} {miembro.Apellidos}",
            NumeroSocio = miembro.NumeroSocio ?? 0,
            FechaIngreso = miembro.FechaIngreso,
            Email = miembro.Email,
            Telefono = miembro.Celular ?? miembro.Telefono,
            Items = items,
            TotalCop = items.Sum(i => i.SubtotalCop),
            FechaGeneracion = DateTime.UtcNow,
            Consecutivo = request.Consecutivo ?? $"CC-{DateTime.UtcNow:yyyy}-{miembro.Id.ToString().Substring(0, 8).ToUpper()}"
        };

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(container =>
                {
                    var logoPath = Path.Combine(_wwwrootPath, "images", "LogoLAMAMedellin.png");
                    
                    container.Column(column =>
                    {
                        if (File.Exists(logoPath))
                        {
                            column.Item().AlignCenter().Width(120).Image(logoPath).FitArea();
                            column.Item().PaddingVertical(5);
                        }

                        column.Item().AlignCenter().Text("CUENTA DE COBRO")
                            .Bold().FontSize(16).FontColor("#1e3a8a");
                        
                        column.Item().AlignCenter().Text($"N° {datos.Consecutivo}")
                            .FontSize(10).FontColor("#64748b");

                        column.Item().PaddingVertical(10).LineHorizontal(1).LineColor("#e2e8f0");
                    });
                });

                page.Content().Column(column =>
                {
                    column.Spacing(10);

                    // Información de LAMA Medellín
                    column.Item().Background("#f8fafc").Padding(12).Column(col =>
                    {
                        col.Item().Text(txt => txt.Span("Fundación L.A.M.A. Medellín").Bold().FontSize(12).FontColor("#1e40af"));
                        col.Item().Text(txt => txt.Span("Latin American Motorcycle Association - Capítulo Medellín").FontSize(9).FontColor("#64748b"));
                        col.Item().PaddingTop(5).Text(txt =>
                        {
                            txt.Span("Fecha de emisión: ").SemiBold();
                            txt.Span(datos.FechaGeneracion.ToString("dd/MM/yyyy"));
                        });
                    });

                    // Información del miembro
                    column.Item().PaddingTop(10).Text(txt => txt.Span("DETALLES DEL MIEMBRO").Bold().FontSize(11).FontColor("#334155"));
                    column.Item().Background("#ffffff").Border(1).BorderColor("#e2e8f0").Padding(12).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("Nombre: ").SemiBold();
                                txt.Span(datos.NombreCompleto);
                            });
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("N° Socio: ").SemiBold();
                                txt.Span(datos.NumeroSocio.ToString());
                            });
                        });

                        if (datos.FechaIngreso.HasValue)
                        {
                            col.Item().PaddingTop(5).Text(txt =>
                            {
                                txt.Span("Fecha de ingreso: ").SemiBold();
                                txt.Span(datos.FechaIngreso.Value.ToString("dd/MM/yyyy"));
                            });
                        }

                        if (!string.IsNullOrWhiteSpace(datos.Email))
                        {
                            col.Item().PaddingTop(3).Text(txt =>
                            {
                                txt.Span("Email: ").SemiBold();
                                txt.Span(datos.Email);
                            });
                        }

                        if (!string.IsNullOrWhiteSpace(datos.Telefono))
                        {
                            col.Item().PaddingTop(3).Text(txt =>
                            {
                                txt.Span("Teléfono: ").SemiBold();
                                txt.Span(datos.Telefono);
                            });
                        }
                    });

                    // Tabla de conceptos
                    column.Item().PaddingTop(15).Text(txt => txt.Span("CONCEPTOS").Bold().FontSize(11).FontColor("#334155"));
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background("#1e3a8a").Padding(8).Text(txt => txt.Span("Descripción").Bold().FontColor("#ffffff").FontSize(9));
                            header.Cell().Background("#1e3a8a").Padding(8).AlignCenter().Text(txt => txt.Span("Cant.").Bold().FontColor("#ffffff").FontSize(9));
                            header.Cell().Background("#1e3a8a").Padding(8).AlignRight().Text(txt => txt.Span("Precio Unit.").Bold().FontColor("#ffffff").FontSize(9));
                            header.Cell().Background("#1e3a8a").Padding(8).AlignRight().Text(txt => txt.Span("Subtotal").Bold().FontColor("#ffffff").FontSize(9));
                        });

                        foreach (var item in datos.Items)
                        {
                            table.Cell().Background("#f8fafc").BorderBottom(1).BorderColor("#e2e8f0").Padding(8)
                                .Text(txt => txt.Span(item.Descripcion).FontSize(9));
                            table.Cell().Background("#f8fafc").BorderBottom(1).BorderColor("#e2e8f0").Padding(8)
                                .AlignCenter().Text(txt => txt.Span(item.Cantidad.ToString()).FontSize(9));
                            table.Cell().Background("#f8fafc").BorderBottom(1).BorderColor("#e2e8f0").Padding(8)
                                .AlignRight().Text(txt => txt.Span(item.PrecioUnitarioCop.ToString("C0", new CultureInfo("es-CO"))).FontSize(9));
                            table.Cell().Background("#f8fafc").BorderBottom(1).BorderColor("#e2e8f0").Padding(8)
                                .AlignRight().Text(txt => txt.Span(item.SubtotalCop.ToString("C0", new CultureInfo("es-CO"))).FontSize(9));
                        }

                        table.Cell().ColumnSpan(3).Background("#1e40af").Padding(10)
                            .AlignRight().Text(txt => txt.Span("TOTAL A PAGAR:").Bold().FontColor("#ffffff").FontSize(10));
                        table.Cell().Background("#1e40af").Padding(10)
                            .AlignRight().Text(txt => txt.Span(datos.TotalCop.ToString("C0", new CultureInfo("es-CO"))).Bold().FontColor("#ffffff").FontSize(11));
                    });

                    if (!string.IsNullOrWhiteSpace(request.Observaciones))
                    {
                        column.Item().PaddingTop(10).Background("#f1f5f9").Border(1).BorderColor("#cbd5e1").Padding(10)
                            .Text(t => t.Span($"Observaciones: {request.Observaciones}").FontSize(9).FontColor("#334155"));
                    }

                    // Instrucciones de pago
                    column.Item().PaddingTop(20).Background("#fffbeb").Border(1).BorderColor("#fbbf24").Padding(12).Column(col =>
                    {
                        col.Item().Text(txt => txt.Span("INSTRUCCIONES DE PAGO").Bold().FontSize(10).FontColor("#92400e"));
                        col.Item().PaddingTop(5).Text(txt => txt.Span("Por favor realice el pago a través de los siguientes medios:").FontSize(9).FontColor("#78350f"));
                        col.Item().PaddingTop(8).Text(txt =>
                        {
                            txt.Span("• Transferencia bancaria: ").SemiBold().FontSize(9);
                            txt.Span("[Cuenta bancaria LAMA - Próximamente]").FontSize(9);
                        });
                        col.Item().PaddingTop(3).Text(txt =>
                        {
                            txt.Span("• PSE o pago en línea: ").SemiBold().FontSize(9);
                            txt.Span("[Portal de pagos - Próximamente]").FontSize(9);
                        });
                        col.Item().PaddingTop(3).Text(txt =>
                        {
                            txt.Span("• Efectivo: ").SemiBold().FontSize(9);
                            txt.Span("En tesorería de LAMA Medellín").FontSize(9);
                        });
                        col.Item().PaddingTop(8).Text(txt => txt.Span("Una vez realizado el pago, por favor enviar comprobante a tesorería +57 310 5127314.").Italic().FontSize(8).FontColor("#92400e"));
                    });
                });

                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("Documento generado automáticamente por el sistema de gestión LAMA Medellín • ").FontSize(7).FontColor("#94a3b8");
                    txt.Span(DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm")).FontSize(7).FontColor("#94a3b8");
                });
            });
        });

        return pdf.GeneratePdf();
    }
}
