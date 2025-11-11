using Microsoft.EntityFrameworkCore;
using Server.DTOs.Cotizaciones;
using Server.Models;
using Server.Data;

namespace Server.Services.Cotizaciones;

/// <summary>
/// Servicio para gestión de cotizaciones. Implementa reglas de negocio: generación de número, cálculo de totales, transiciones de estado.
/// </summary>
public class CotizacionesService : ICotizacionesService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public CotizacionesService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<(List<CotizacionDto> Cotizaciones, int TotalCount)> ObtenerCotizacionesAsync(string? busqueda = null, string? estado = null, Guid? clienteId = null, Guid? miembroId = null, DateTime? desde = null, DateTime? hasta = null, int pagina = 1, int registrosPorPagina = 20)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var query = db.Cotizaciones
            .AsNoTracking()
            .Include(c => c.Detalles)
            .Include(c => c.Cliente)
            .Include(c => c.Miembro)
            .AsQueryable();

        // Filtros
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var term = busqueda.Trim().ToLower();
            query = query.Where(c => c.NumeroCotizacion.ToLower().Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(estado))
        {
            query = query.Where(c => c.Estado == estado);
        }
        if (clienteId.HasValue)
        {
            query = query.Where(c => c.ClienteId == clienteId.Value);
        }
        if (miembroId.HasValue)
        {
            query = query.Where(c => c.MiembroId == miembroId.Value);
        }
        if (desde.HasValue)
        {
            query = query.Where(c => c.FechaCotizacion >= desde.Value.Date);
        }
        if (hasta.HasValue)
        {
            var fin = hasta.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(c => c.FechaCotizacion <= fin);
        }

        // Auto marcas de vencidas (sin UPDATE masivo, calculado en proyección)
        var total = await query.CountAsync();
        var skip = (pagina - 1) * registrosPorPagina;
        var cotizaciones = await query
            .OrderByDescending(c => c.FechaCotizacion)
            .Skip(skip)
            .Take(registrosPorPagina)
            .Select(c => new CotizacionDto
            {
                Id = c.Id,
                NumeroCotizacion = c.NumeroCotizacion,
                Estado = CalcularEstadoDerivado(c),
                ClienteId = c.ClienteId,
                ClienteNombre = c.Cliente != null ? c.Cliente.Nombre : null,
                MiembroId = c.MiembroId,
                MiembroNombre = c.Miembro != null ? (c.Miembro.Nombres + " " + c.Miembro.Apellidos) : null,
                TipoCliente = c.ClienteId.HasValue ? "Externo" : (c.MiembroId.HasValue ? "Miembro" : ""),
                FechaCotizacion = c.FechaCotizacion,
                FechaValidez = c.FechaVencimiento,
                SubtotalCOP = c.SubtotalCOP,
                DescuentoCOP = c.DescuentoCOP,
                TotalCOP = c.TotalCOP,
                TotalUSD = c.TotalUSD,
                TotalItems = c.Detalles.Count,
                Convertida = c.VentaId.HasValue,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return (cotizaciones, total);
    }

    public async Task<CotizacionDetalleDto?> ObtenerDetalleAsync(Guid id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var cot = await db.Cotizaciones
            .Include(c => c.Detalles).ThenInclude(d => d.Producto)
            .Include(c => c.Cliente)
            .Include(c => c.Miembro)
            .Include(c => c.Venta)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (cot == null) return null;

        var estadoDerivado = CalcularEstadoDerivado(cot);

        return new CotizacionDetalleDto
        {
            Id = cot.Id,
            NumeroCotizacion = cot.NumeroCotizacion,
            Estado = estadoDerivado,
            ClienteId = cot.ClienteId,
            ClienteNombre = cot.Cliente?.Nombre,
            ClienteIdentificacion = cot.Cliente?.Identificacion,
            ClienteEmail = cot.Cliente?.Email,
            MiembroId = cot.MiembroId,
            MiembroNombre = cot.Miembro != null ? (cot.Miembro.Nombres + " " + cot.Miembro.Apellidos) : null,
            MiembroEmail = cot.Miembro?.Email,
            TipoCliente = cot.ClienteId.HasValue ? "Externo" : (cot.MiembroId.HasValue ? "Miembro" : ""),
            FechaCotizacion = cot.FechaCotizacion,
            FechaValidez = cot.FechaVencimiento,
            SubtotalCOP = cot.SubtotalCOP,
            DescuentoCOP = cot.DescuentoCOP,
            TotalCOP = cot.TotalCOP,
            TotalUSD = cot.TotalUSD,
            Observaciones = cot.Observaciones,
            VentaId = cot.VentaId,
            NumeroVenta = cot.Venta?.NumeroVenta,
            FechaConversion = cot.Venta?.FechaVenta,
            CreatedAt = cot.CreatedAt,
            CreatedBy = cot.CreatedBy,
            UpdatedAt = cot.UpdatedAt,
            UpdatedBy = cot.UpdatedBy,
            Detalles = cot.Detalles.Select(d => new DetalleCotizacionDto
            {
                Id = d.Id,
                ProductoId = d.ProductoId,
                ProductoCodigo = d.Producto.Codigo,
                ProductoNombre = d.Producto.Nombre,
                Cantidad = d.Cantidad,
                PrecioUnitarioCOP = d.PrecioUnitarioCOP,
                DescuentoCOP = d.DescuentoCOP,
                SubtotalCOP = d.SubtotalCOP,
                Notas = d.Notas
            }).ToList()
        };
    }

    public async Task<(bool Success, string Message, Guid? CotizacionId)> CrearCotizacionAsync(CotizacionFormDto dto, string usuarioId)
    {
        // Validaciones básicas
        if (dto.Detalles == null || dto.Detalles.Count == 0)
            return (false, "Debe incluir al menos un detalle.", null);
        if (dto.Detalles.Any(d => d.Cantidad <= 0))
            return (false, "Cada detalle debe tener cantidad > 0.", null);
        if (dto.FechaValidez < DateTime.UtcNow.Date)
            return (false, "La fecha de validez no puede ser en el pasado.", null);

        await using var db = await _dbFactory.CreateDbContextAsync();

        var numero = await GenerarNumeroAsync(dto.FechaCotizacion);

        var cotizacion = new Cotizacion
        {
            Id = Guid.NewGuid(),
            NumeroCotizacion = numero,
            Estado = "Pendiente",
            ClienteId = dto.ClienteId,
            MiembroId = dto.MiembroId,
            FechaCotizacion = dto.FechaCotizacion,
            FechaVencimiento = dto.FechaValidez,
            Observaciones = dto.Observaciones,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = usuarioId
        };

        // Detalles y totales
        foreach (var det in dto.Detalles)
        {
            var subtotal = det.Cantidad * det.PrecioUnitarioCOP;
            cotizacion.Detalles.Add(new DetalleCotizacion
            {
                Id = Guid.NewGuid(),
                CotizacionId = cotizacion.Id,
                ProductoId = det.ProductoId,
                Cantidad = det.Cantidad,
                PrecioUnitarioCOP = det.PrecioUnitarioCOP,
                DescuentoCOP = det.DescuentoCOP,
                SubtotalCOP = subtotal,
                Notas = det.Notas
            });
        }

        RecalcularTotales(cotizacion);

        db.Cotizaciones.Add(cotizacion);
        await db.SaveChangesAsync();

        return (true, "Cotización creada correctamente.", cotizacion.Id);
    }

    public async Task<(bool Success, string Message)> ActualizarCotizacionAsync(Guid id, CotizacionFormDto dto, string usuarioId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var cot = await db.Cotizaciones.Include(c => c.Detalles).FirstOrDefaultAsync(c => c.Id == id);
        if (cot == null) return (false, "No encontrada.");
        if (cot.Estado is "Rechazada" or "Convertida") return (false, "No se puede editar en este estado.");

        if (dto.Detalles == null || dto.Detalles.Count == 0) return (false, "Debe incluir al menos un detalle.");
        if (dto.Detalles.Any(d => d.Cantidad <= 0)) return (false, "Cada detalle debe tener cantidad > 0.");
        if (dto.FechaValidez < DateTime.UtcNow.Date) return (false, "Fecha de validez inválida.");

        cot.ClienteId = dto.ClienteId;
        cot.MiembroId = dto.MiembroId;
        cot.FechaCotizacion = dto.FechaCotizacion;
        cot.FechaVencimiento = dto.FechaValidez;
        cot.Observaciones = dto.Observaciones;
        cot.UpdatedAt = DateTime.UtcNow;
        cot.UpdatedBy = usuarioId;

        // Reemplazar detalles
        db.DetallesCotizaciones.RemoveRange(cot.Detalles);
        cot.Detalles.Clear();
        foreach (var det in dto.Detalles)
        {
            var subtotal = det.Cantidad * det.PrecioUnitarioCOP;
            cot.Detalles.Add(new DetalleCotizacion
            {
                Id = Guid.NewGuid(),
                CotizacionId = cot.Id,
                ProductoId = det.ProductoId,
                Cantidad = det.Cantidad,
                PrecioUnitarioCOP = det.PrecioUnitarioCOP,
                DescuentoCOP = det.DescuentoCOP,
                SubtotalCOP = subtotal,
                Notas = det.Notas
            });
        }
        RecalcularTotales(cot);
        await db.SaveChangesAsync();
        return (true, "Cotización actualizada.");
    }

    public async Task<(bool Success, string Message)> CambiarEstadoAsync(Guid id, string nuevoEstado, string usuarioId)
    {
        var estadosValidos = new[] { "Pendiente", "Aprobada", "Rechazada", "Convertida", "Vencida" };
        if (!estadosValidos.Contains(nuevoEstado)) return (false, "Estado inválido.");

        await using var db = await _dbFactory.CreateDbContextAsync();
        var cot = await db.Cotizaciones.FirstOrDefaultAsync(c => c.Id == id);
        if (cot == null) return (false, "No encontrada.");

        var estadoActual = CalcularEstadoDerivado(cot);

        // Reglas de transición
        if (estadoActual == "Convertida") return (false, "Ya convertida, no modificable.");
        if (estadoActual == "Rechazada" && nuevoEstado != "Pendiente") return (false, "Rechazada solo puede volver a Pendiente.");
        if (estadoActual == "Vencida" && nuevoEstado != "Pendiente") return (false, "Vencida solo puede volver a Pendiente.");
        if (estadoActual == "Pendiente" && nuevoEstado == "Convertida") return (false, "Debe aprobar antes de convertir.");

        cot.Estado = nuevoEstado;
        cot.UpdatedAt = DateTime.UtcNow;
        cot.UpdatedBy = usuarioId;
        await db.SaveChangesAsync();
        return (true, "Estado actualizado.");
    }

    public async Task<(bool Success, string Message)> EliminarCotizacionAsync(Guid id, string usuarioId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var cot = await db.Cotizaciones.Include(c => c.Detalles).FirstOrDefaultAsync(c => c.Id == id);
        if (cot == null) return (false, "No encontrada.");
        var estadoActual = CalcularEstadoDerivado(cot);
        if (estadoActual is "Aprobada" or "Convertida") return (false, "No se puede eliminar por su estado.");

        db.DetallesCotizaciones.RemoveRange(cot.Detalles);
        db.Cotizaciones.Remove(cot);
        await db.SaveChangesAsync();
        return (true, "Cotización eliminada.");
    }

    public async Task<string> GenerarNumeroAsync(DateTime fechaCotizacion)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var prefijo = $"COT-{fechaCotizacion:yyyyMM}-";
        // Buscar últimos del mes
        var ultimo = await db.Cotizaciones
            .Where(c => c.NumeroCotizacion.StartsWith(prefijo))
            .OrderByDescending(c => c.NumeroCotizacion)
            .Select(c => c.NumeroCotizacion)
            .FirstOrDefaultAsync();
        int secuencia = 1;
        if (ultimo != null)
        {
            var parte = ultimo.Substring(prefijo.Length);
            if (int.TryParse(parte, out var n)) secuencia = n + 1;
        }
        return prefijo + secuencia.ToString("D5");
    }

    private static void RecalcularTotales(Cotizacion cotizacion)
    {
        decimal subtotal = cotizacion.Detalles.Sum(d => d.SubtotalCOP);
        decimal descuentoTotal = cotizacion.Detalles.Sum(d => d.DescuentoCOP);
        cotizacion.SubtotalCOP = subtotal;
        cotizacion.DescuentoCOP = descuentoTotal;
        cotizacion.TotalCOP = subtotal - descuentoTotal;
        // TODO: lógica para TotalUSD si se requiere tasa de cambio; por ahora conservar valor existente
    }

    private static string CalcularEstadoDerivado(Cotizacion c)
    {
        if (c.Estado is "Aprobada" or "Rechazada" or "Convertida") return c.Estado;
        if (c.FechaVencimiento.Date < DateTime.UtcNow.Date) return "Vencida";
        return c.Estado; // Pendiente
    }
}