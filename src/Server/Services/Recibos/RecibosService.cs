using Microsoft.EntityFrameworkCore;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Server.Data;
using Server.DTOs;
using Server.DTOs.Recibos;
using Server.DTOs.Recibos;
using Server.Models;
using Server.Services.Exchange;
using Server.Services.CierreContable;
using Server.Services.Audit;

namespace Server.Services.Recibos;

/// <summary>
/// Servicio que crea borradores de recibos, emite consecutivos por año/serie y genera PDF con QR.
/// Usa QuestPDF y QRCoder.
/// </summary>
public class RecibosService : IRecibosService
{
    private readonly AppDbContext _db;
    private readonly IExchangeRateService _trm;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _cfg;
    private readonly CierreContableService _cierreService;
    private readonly IAuditService _audit;

    public RecibosService(
        AppDbContext db, 
        IExchangeRateService trm, 
        IWebHostEnvironment env, 
        IConfiguration cfg, 
        CierreContableService cierreService,
        IAuditService audit)
    {
        _db = db;
        _trm = trm;
        _env = env;
        _cfg = cfg;
        _cierreService = cierreService;
        _audit = audit;
    }

    #region CRUD Operations
        /// <summary>
        /// Obtiene todos los conceptos disponibles para recibos.
        /// </summary>
        public async Task<List<ConceptoListItem>> GetConceptosAsync()
        {
            // Solo conceptos de ingreso para usarse en recibos (evita egresos/otros)
            return await _db.Conceptos
                .Where(c => c.EsIngreso)
                .Select(c => new ConceptoListItem
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Codigo = c.Codigo
                })
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene la TRM USD/COP para una fecha dada.
        /// </summary>
        public async Task<decimal?> GetTrmAsync(DateTime fecha)
        {
            var dateOnly = DateOnly.FromDateTime(fecha);
            return await _trm.GetUsdCopAsync(dateOnly);
        }

    public async Task<PagedResult<ReciboListItem>> GetPagedAsync(string? query, EstadoRecibo? estado, int page, int pageSize)
    {
        var q = _db.Recibos
            .Include(r => r.Miembro)
            .AsNoTracking()
            .AsQueryable();

        // Filtro por texto (número completo, nombre miembro o tercero libre)
        if (!string.IsNullOrWhiteSpace(query))
        {
            var txt = query.Trim().ToLower();
            q = q.Where(r =>
                (r.Serie + "-" + r.Ano + "-" + r.Consecutivo.ToString()).ToLower().Contains(txt) ||
                (r.Miembro != null && r.Miembro.NombreCompleto.ToLower().Contains(txt)) ||
                (r.TerceroLibre != null && r.TerceroLibre.ToLower().Contains(txt)));
        }

        // Filtro por estado
        if (estado.HasValue)
        {
            q = q.Where(r => r.Estado == estado.Value);
        }

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(r => r.FechaEmision)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReciboListItem
            {
                Id = r.Id,
                NumeroCompleto = $"{r.Serie}-{r.Ano}-{r.Consecutivo:D5}",
                FechaEmision = r.FechaEmision,
                Estado = r.Estado,
                NombrePagador = r.Miembro != null ? r.Miembro.NombreCompleto : (r.TerceroLibre ?? "Sin especificar"),
                TotalCop = r.TotalCop
            })
            .ToListAsync();

        return new PagedResult<ReciboListItem>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ReciboDetailDto?> GetByIdAsync(Guid id)
    {
        return await _db.Recibos
            .Include(r => r.Miembro)
            .Include(r => r.Items)
                .ThenInclude(i => i.Concepto)
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new ReciboDetailDto
            {
                Id = r.Id,
                Serie = r.Serie,
                Ano = r.Ano,
                Consecutivo = r.Consecutivo,
                FechaEmision = r.FechaEmision,
                Estado = r.Estado,
                MiembroId = r.MiembroId,
                MiembroNombre = r.Miembro != null ? r.Miembro.NombreCompleto : null,
                TerceroLibre = r.TerceroLibre,
                TotalCop = r.TotalCop,
                Observaciones = r.Observaciones,
                Items = r.Items.Select(i => new ReciboItemDetailDto
                {
                    Id = i.Id,
                    ConceptoId = i.ConceptoId,
                    ConceptoNombre = i.Concepto.Nombre,
                    Cantidad = i.Cantidad,
                    PrecioUnitarioMonedaOrigen = i.PrecioUnitarioMonedaOrigen,
                    MonedaOrigen = i.MonedaOrigen,
                    TrmAplicada = i.TrmAplicada,
                    SubtotalCop = i.SubtotalCop,
                    Notas = i.Notas
                }).ToList(),
                CreatedAt = r.CreatedAt,
                CreatedBy = r.CreatedBy
            })
            .FirstOrDefaultAsync();
    }

    public async Task<Guid> CreateAsync(CreateReciboDto dto, string currentUser)
    {
        var recibo = new Recibo
        {
            Serie = dto.Serie,
            Ano = dto.Ano,
            FechaEmision = dto.FechaEmision,
            MiembroId = dto.MiembroId,
            TerceroLibre = dto.TerceroLibre,
            Observaciones = dto.Observaciones,
            Estado = EstadoRecibo.Borrador,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser
        };

        // Procesar items
        var hoy = DateOnly.FromDateTime(dto.FechaEmision);
        foreach (var itemDto in dto.Items)
        {
            var concepto = await _db.Conceptos.FindAsync(itemDto.ConceptoId);
            if (concepto == null)
                throw new InvalidOperationException($"Concepto {itemDto.ConceptoId} no encontrado");

            decimal? trmAplicada = null;
            decimal subtotalCop;

            if (itemDto.MonedaOrigen == Moneda.USD)
            {
                // Si se proporcionó TRM manual, usarla; de lo contrario, buscar en la tabla
                trmAplicada = itemDto.TrmAplicada ?? await _trm.GetUsdCopAsync(hoy);
                subtotalCop = itemDto.Cantidad * itemDto.PrecioUnitarioMonedaOrigen * trmAplicada.Value;
            }
            else
            {
                subtotalCop = itemDto.Cantidad * itemDto.PrecioUnitarioMonedaOrigen;
            }

            recibo.Items.Add(new ReciboItem
            {
                ConceptoId = itemDto.ConceptoId,
                Cantidad = itemDto.Cantidad,
                PrecioUnitarioMonedaOrigen = itemDto.PrecioUnitarioMonedaOrigen,
                MonedaOrigen = itemDto.MonedaOrigen,
                TrmAplicada = trmAplicada,
                SubtotalCop = decimal.Round(subtotalCop, 2),
                Notas = itemDto.Notas
            });
        }

        recibo.TotalCop = recibo.Items.Sum(i => i.SubtotalCop);

        _db.Recibos.Add(recibo);
        await _db.SaveChangesAsync();

        return recibo.Id;
    }

    public async Task<bool> UpdateAsync(UpdateReciboDto dto, string currentUser)
    {
        var recibo = await _db.Recibos
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == dto.Id);

        if (recibo == null)
            return false;

        // Solo se pueden editar borradores
        if (recibo.Estado != EstadoRecibo.Borrador)
            throw new InvalidOperationException("Solo se pueden editar recibos en estado Borrador");

        // Actualizar propiedades
        recibo.Serie = dto.Serie;
        recibo.Ano = dto.Ano;
        recibo.FechaEmision = dto.FechaEmision;
        recibo.MiembroId = dto.MiembroId;
        recibo.TerceroLibre = dto.TerceroLibre;
        recibo.Observaciones = dto.Observaciones;

        // Eliminar items existentes
        _db.ReciboItems.RemoveRange(recibo.Items);

        // Agregar nuevos items
        recibo.Items.Clear();
        var hoy = DateOnly.FromDateTime(dto.FechaEmision);
        foreach (var itemDto in dto.Items)
        {
            var concepto = await _db.Conceptos.FindAsync(itemDto.ConceptoId);
            if (concepto == null)
                throw new InvalidOperationException($"Concepto {itemDto.ConceptoId} no encontrado");

            decimal? trmAplicada = null;
            decimal subtotalCop;

            if (itemDto.MonedaOrigen == Moneda.USD)
            {
                trmAplicada = itemDto.TrmAplicada ?? await _trm.GetUsdCopAsync(hoy);
                subtotalCop = itemDto.Cantidad * itemDto.PrecioUnitarioMonedaOrigen * trmAplicada.Value;
            }
            else
            {
                subtotalCop = itemDto.Cantidad * itemDto.PrecioUnitarioMonedaOrigen;
            }

            recibo.Items.Add(new ReciboItem
            {
                ConceptoId = itemDto.ConceptoId,
                Cantidad = itemDto.Cantidad,
                PrecioUnitarioMonedaOrigen = itemDto.PrecioUnitarioMonedaOrigen,
                MonedaOrigen = itemDto.MonedaOrigen,
                TrmAplicada = trmAplicada,
                SubtotalCop = decimal.Round(subtotalCop, 2),
                Notas = itemDto.Notas
            });
        }

        recibo.TotalCop = recibo.Items.Sum(i => i.SubtotalCop);

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var recibo = await _db.Recibos.FindAsync(id);
        if (recibo == null)
            return false;

        // Solo se pueden eliminar borradores
        if (recibo.Estado != EstadoRecibo.Borrador)
            throw new InvalidOperationException("Solo se pueden eliminar recibos en estado Borrador");

        _db.Recibos.Remove(recibo);
        await _db.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Workflow de Estados

    public async Task<bool> CambiarEstadoAsync(CambiarEstadoReciboDto dto, string currentUser)
    {
        var recibo = await _db.Recibos.FindAsync(dto.ReciboId);
        if (recibo == null)
            return false;

        // Validar transiciones permitidas
        if (recibo.Estado == EstadoRecibo.Borrador && dto.NuevoEstado == EstadoRecibo.Emitido)
        {
            return await EmitirAsync(dto.ReciboId, currentUser);
        }
        else if (recibo.Estado == EstadoRecibo.Emitido && dto.NuevoEstado == EstadoRecibo.Anulado)
        {
            return await AnularAsync(dto.ReciboId, dto.Razon ?? "", currentUser);
        }

        throw new InvalidOperationException($"Transición no permitida: {recibo.Estado} → {dto.NuevoEstado}");
    }

    public async Task<bool> EmitirAsync(Guid reciboId, string currentUser)
    {
        using var tx = await _db.Database.BeginTransactionAsync();
        
        var recibo = await _db.Recibos
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == reciboId);

        if (recibo == null)
            return false;

        if (recibo.Estado != EstadoRecibo.Borrador)
            throw new InvalidOperationException("Solo se pueden emitir recibos en estado Borrador");

        // Validar que tenga items
        if (!recibo.Items.Any())
            throw new InvalidOperationException("El recibo debe tener al menos un item");

        // Asignar consecutivo
        recibo.Ano = DateTime.UtcNow.Year;
        recibo.Consecutivo = await GetNextConsecutivoAsync(recibo.Serie, recibo.Ano);
        recibo.Estado = EstadoRecibo.Emitido;
        recibo.FechaEmision = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        // Registrar auditoría
        await _audit.LogAsync(
            entityType: "Recibo",
            entityId: recibo.Id.ToString(),
            action: "Emitted",
            userName: currentUser,
            newValues: new 
            { 
                Consecutivo = recibo.Consecutivo,
                Serie = recibo.Serie,
                Ano = recibo.Ano,
                Estado = recibo.Estado,
                FechaEmision = recibo.FechaEmision,
                TotalCop = recibo.TotalCop
            },
            additionalInfo: $"Recibo {recibo.Serie}-{recibo.Ano}-{recibo.Consecutivo:D5} emitido. Total: ${recibo.TotalCop:N0}"
        );

        return true;
    }

    public async Task<bool> AnularAsync(Guid reciboId, string razon, string currentUser)
    {
        var recibo = await _db.Recibos.FindAsync(reciboId);
        if (recibo == null)
            return false;

        if (recibo.Estado != EstadoRecibo.Emitido)
            throw new InvalidOperationException("Solo se pueden anular recibos en estado Emitido");

        // Validar que el mes del recibo no esté cerrado
        var esCerrado = await _cierreService.EsFechaCerradaAsync(recibo.FechaEmision);
        if (esCerrado)
            throw new InvalidOperationException($"No se puede anular el recibo porque el mes {recibo.FechaEmision:MM/yyyy} está cerrado");

        recibo.Estado = EstadoRecibo.Anulado;
        if (!string.IsNullOrWhiteSpace(razon))
        {
            recibo.Observaciones = (recibo.Observaciones ?? "") + $"\n[ANULADO: {razon}]";
        }

        await _db.SaveChangesAsync();

        // Registrar auditoría
        await _audit.LogAsync(
            entityType: "Recibo",
            entityId: recibo.Id.ToString(),
            action: "Annulled",
            userName: currentUser,
            oldValues: new { Estado = EstadoRecibo.Emitido },
            newValues: new 
            { 
                Estado = EstadoRecibo.Anulado,
                Observaciones = recibo.Observaciones
            },
            additionalInfo: $"Recibo {recibo.Serie}-{recibo.Ano}-{recibo.Consecutivo:D5} anulado. Razón: {razon}"
        );

        return true;
    }

    #endregion

    #region Numeración

    public async Task<int> GetNextConsecutivoAsync(string serie, int ano)
    {
        var max = await _db.Recibos
            .Where(r => r.Ano == ano && r.Serie == serie)
            .MaxAsync(r => (int?)r.Consecutivo) ?? 0;

        return max + 1;
    }

    #endregion

    #region PDF y QR

    public async Task<byte[]> GenerarPdfAsync(Guid reciboId)
    {
        var r = await _db.Recibos
            .Include(x => x.Miembro)
            .Include(x => x.Items).ThenInclude(i => i.Concepto)
            .FirstAsync(x => x.Id == reciboId);

        var baseUrl = _cfg["Receipt:VerifyBaseUrl"] ?? "https://lama-medellin.org";
        var verifyUrl = $"{baseUrl}/recibo/{r.Id}/verificacion";

        using var qrGen = new QRCodeGenerator();
        using var qrData = qrGen.CreateQrCode(verifyUrl, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrData);
        var qrBytes = qrCode.GetGraphic(10);

        var logoPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "images", "LogoLAMAMedellin.png");
        byte[]? logo = File.Exists(logoPath) ? await File.ReadAllBytesAsync(logoPath) : null;

        QuestPDF.Settings.License = LicenseType.Community;
        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                
                // Encabezado mejorado con diseño profesional
                page.Header().Column(headerCol =>
                {
                    headerCol.Item().Row(row =>
                    {
                        // Logo
                        if (logo != null)
                        {
                            row.ConstantItem(80).Image(logo, ImageScaling.FitArea);
                        }
                        
                        // Información central
                        row.RelativeItem().PaddingHorizontal(15).Column(col =>
                        {
                            col.Item().Text("FUNDACIÓN L.A.M.A.").Bold().FontSize(18).FontColor("#1e3a8a");
                            col.Item().Text("MEDELLÍN").Bold().FontSize(18).FontColor("#1e3a8a");
                            col.Item().PaddingTop(5).Text("RECIBO DE CAJA").FontSize(14).FontColor("#64748b");
                        });
                        
                        // QR Code
                        row.ConstantItem(100).Image(qrBytes, ImageScaling.FitArea);
                    });
                    
                    // Información del recibo en badge
                    headerCol.Item().PaddingTop(15).Background("#dbeafe").Border(1).BorderColor("#3b82f6").Padding(12).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(text =>
                            {
                                text.Span("No. Recibo: ").FontSize(11).FontColor("#64748b");
                                text.Span($"{r.Serie}-{r.Ano}-{r.Consecutivo:D6}").Bold().FontSize(12).FontColor("#1e3a8a");
                            });
                            col.Item().Text(text =>
                            {
                                text.Span("Fecha: ").FontSize(10).FontColor("#64748b");
                                text.Span($"{r.FechaEmision:dd/MM/yyyy HH:mm}").FontSize(10).FontColor("#334155");
                            });
                        });
                        row.RelativeItem().AlignRight().Background(
                            r.Estado == EstadoRecibo.Emitido ? "#10b981" : 
                            r.Estado == EstadoRecibo.Anulado ? "#ef4444" : "#fbbf24"
                        ).Padding(8).AlignCenter().Text(r.Estado.ToString().ToUpper()).Bold().FontSize(11).FontColor("#ffffff");
                    });
                    
                    headerCol.Item().PaddingTop(10).LineHorizontal(2).LineColor("#1e3a8a");
                });

                page.Content().PaddingVertical(15).Column(col =>
                {
                    // Datos del pagador con diseño mejorado
                    col.Item().Background("#f8fafc").Border(1).BorderColor("#cbd5e1").Padding(15).Column(infoCol =>
                    {
                        infoCol.Item().Row(row =>
                        {
                            row.ConstantItem(20).Text("👤").FontSize(14);
                            row.RelativeItem().PaddingLeft(8).Text("PAGADOR").Bold().FontSize(12).FontColor("#1e3a8a");
                        });
                        infoCol.Item().PaddingTop(8).LineHorizontal(1).LineColor("#cbd5e1");
                        infoCol.Item().PaddingTop(8).Text(r.Miembro is not null
                            ? $"{r.Miembro.Nombres} {r.Miembro.Apellidos}"
                            : r.TerceroLibre ?? "Sin especificar").FontSize(11).FontColor("#334155").Bold();
                        
                        if (r.Miembro is not null)
                        {
                            infoCol.Item().PaddingTop(3).Text(text =>
                            {
                                text.Span("Documento: ").FontSize(9).FontColor("#64748b");
                                text.Span(r.Miembro.Documento ?? "N/A").FontSize(9).FontColor("#334155");
                                text.Span("  |  Teléfono: ").FontSize(9).FontColor("#64748b");
                                text.Span(r.Miembro.Telefono ?? "N/A").FontSize(9).FontColor("#334155");
                            });
                        }
                    });

                    // Título de detalle
                    col.Item().PaddingTop(20).Row(row =>
                    {
                        row.RelativeItem().Text("DETALLE DE CONCEPTOS").Bold().FontSize(13).FontColor("#1e3a8a");
                        row.ConstantItem(80).Background("#3b82f6").Padding(5).AlignCenter().Text("📋").FontSize(12);
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
                            c.RelativeColumn(3);
                            c.RelativeColumn(3);
                            c.RelativeColumn(3);
                        });
                        
                        // Encabezado con fondo azul
                        t.Header(h =>
                        {
                            h.Cell().Background("#1e3a8a").Padding(10).Text("CONCEPTO").Bold().FontSize(10).FontColor("#ffffff");
                            h.Cell().Background("#1e3a8a").Padding(10).AlignCenter().Text("MONEDA").Bold().FontSize(10).FontColor("#ffffff");
                            h.Cell().Background("#1e3a8a").Padding(10).AlignCenter().Text("CANT.").Bold().FontSize(10).FontColor("#ffffff");
                            h.Cell().Background("#1e3a8a").Padding(10).AlignRight().Text("PRECIO").Bold().FontSize(10).FontColor("#ffffff");
                            h.Cell().Background("#1e3a8a").Padding(10).AlignRight().Text("TRM").Bold().FontSize(10).FontColor("#ffffff");
                            h.Cell().Background("#1e3a8a").Padding(10).AlignRight().Text("SUBTOTAL COP").Bold().FontSize(10).FontColor("#ffffff");
                        });
                        
                        // Filas con separadores y alternancia de colores
                        bool alternar = false;
                        foreach (var it in r.Items)
                        {
                            t.Cell().Background(alternar ? "#f8fafc" : "#ffffff").BorderBottom(1).BorderColor("#e2e8f0").Padding(10).Text($"{it.Concepto.Nombre}").FontSize(9).FontColor("#334155");
                            t.Cell().Background(alternar ? "#f8fafc" : "#ffffff").BorderBottom(1).BorderColor("#e2e8f0").Padding(10).AlignCenter().Text(it.MonedaOrigen.ToString()).FontSize(9).FontColor("#64748b").Bold();
                            t.Cell().Background(alternar ? "#f8fafc" : "#ffffff").BorderBottom(1).BorderColor("#e2e8f0").Padding(10).AlignCenter().Text(it.Cantidad.ToString()).FontSize(9).FontColor("#334155");
                            t.Cell().Background(alternar ? "#f8fafc" : "#ffffff").BorderBottom(1).BorderColor("#e2e8f0").Padding(10).AlignRight().Text(it.PrecioUnitarioMonedaOrigen.ToString("N2")).FontSize(9).FontColor("#334155");
                            t.Cell().Background(alternar ? "#f8fafc" : "#ffffff").BorderBottom(1).BorderColor("#e2e8f0").Padding(10).AlignRight().Text(it.TrmAplicada?.ToString("N2") ?? "-").FontSize(9).FontColor("#64748b");
                            t.Cell().Background(alternar ? "#f8fafc" : "#ffffff").BorderBottom(1).BorderColor("#e2e8f0").Padding(10).AlignRight().Text(it.SubtotalCop.ToString("N0")).FontSize(9).FontColor("#10b981").Bold();
                            alternar = !alternar;
                        }
                    });

                    // Total destacado con badge
                    col.Item().PaddingTop(15).AlignRight().Background("#10b981").Border(2).BorderColor("#059669").Padding(15).Row(row =>
                    {
                        row.RelativeItem().Text("TOTAL A PAGAR:").Bold().FontSize(13).FontColor("#ffffff");
                        row.ConstantItem(150).AlignRight().Text($"{r.TotalCop:N0} COP").Bold().FontSize(16).FontColor("#ffffff");
                    });
                    
                    // Observaciones si existen
                    if (!string.IsNullOrWhiteSpace(r.Observaciones))
                    {
                        col.Item().PaddingTop(15).Background("#fffbeb").Border(1).BorderColor("#fde047").Padding(12).Column(obsCol =>
                        {
                            obsCol.Item().Row(row =>
                            {
                                row.ConstantItem(20).Text("📝").FontSize(12);
                                row.RelativeItem().PaddingLeft(8).Text("OBSERVACIONES").Bold().FontSize(10).FontColor("#92400e");
                            });
                            obsCol.Item().PaddingTop(5).Text(r.Observaciones).FontSize(9).FontColor("#78350f");
                        });
                    }
                    
                    // Nota de validación
                    col.Item().PaddingTop(15).Background("#dcfce7").Border(1).BorderColor("#86efac").Padding(10).Row(row =>
                    {
                        row.ConstantItem(20).Text("✓").Bold().FontSize(12).FontColor("#10b981");
                        row.RelativeItem().PaddingLeft(8).Text("Este recibo no constituye factura. Validar autenticidad escaneando el código QR.").FontSize(9).FontColor("#166534");
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
                        row.RelativeItem().AlignCenter().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(7).FontColor("#94a3b8");
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

        return bytes;
    }

    public async Task<string> GenerarQRCodeAsync(Guid reciboId)
    {
        var baseUrl = _cfg["Receipt:VerifyBaseUrl"] ?? "https://lama-medellin.org";
        var verifyUrl = $"{baseUrl}/recibo/{reciboId}/verificacion";

        using var qrGen = new QRCodeGenerator();
        using var qrData = qrGen.CreateQrCode(verifyUrl, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrData);
        var qrBytes = qrCode.GetGraphic(10);

        return Convert.ToBase64String(qrBytes);
    }

    #endregion

    #region Métodos Legacy (compatibilidad)

    public async Task<Recibo> CrearBorradorAsync(Guid? miembroId, string? terceroLibre, IEnumerable<(string codigoConcepto, int cantidad)> items, string user, CancellationToken ct = default)
    {
        var conceptos = await _db.Conceptos.ToListAsync(ct);
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var r = new Recibo { MiembroId = miembroId, TerceroLibre = terceroLibre, CreatedBy = user };

        foreach (var it in items)
        {
            var c = conceptos.First(x => x.Codigo == it.codigoConcepto);
            decimal? trmUsd = null;
            decimal subtotalCop;
            if (c.Moneda == Moneda.USD)
            {
                trmUsd = await _trm.GetUsdCopAsync(hoy, ct);
                subtotalCop = it.cantidad * c.PrecioBase * trmUsd.Value;
            }
            else
            {
                subtotalCop = it.cantidad * c.PrecioBase;
            }
            r.Items.Add(new ReciboItem
            {
                ConceptoId = c.Id,
                Cantidad = it.cantidad,
                PrecioUnitarioMonedaOrigen = c.PrecioBase,
                MonedaOrigen = c.Moneda,
                TrmAplicada = trmUsd,
                SubtotalCop = decimal.Round(subtotalCop, 2)
            });
        }

        r.TotalCop = r.Items.Sum(x => x.SubtotalCop);
        _db.Recibos.Add(r);
        await _db.SaveChangesAsync(ct);
        return r;
    }

    public async Task<Recibo> EmitirAsync(Guid reciboId, string user, CancellationToken ct = default)
    {
        using var tx = await _db.Database.BeginTransactionAsync(ct);
        var r = await _db.Recibos.FirstAsync(x => x.Id == reciboId, ct);
        if (r.Estado != EstadoRecibo.Borrador) return r;
        r.Ano = DateTime.UtcNow.Year;
        var max = await _db.Recibos.Where(x => x.Ano == r.Ano && x.Serie == r.Serie).MaxAsync(x => (int?)x.Consecutivo, ct) ?? 0;
        r.Consecutivo = max + 1;
        r.Estado = EstadoRecibo.Emitido;
        r.FechaEmision = DateTime.UtcNow;
        r.CreatedBy = user;
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return r;
    }

    #endregion
}
