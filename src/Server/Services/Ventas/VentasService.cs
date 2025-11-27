using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs.Ventas;
using Server.Models;

namespace Server.Services.Ventas;

/// <summary>
/// Servicio para gestión de ventas de productos
/// </summary>
public class VentasService : IVentasService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public VentasService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<VentaProductoDto>> GetAllAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.VentasProductos
            .AsNoTracking()
            .Include(v => v.Detalles)
            .ThenInclude(d => d.Producto)
            .Include(v => v.Miembro)
            .OrderByDescending(v => v.FechaVenta)
            .Select(v => MapToDto(v))
            .ToListAsync();
    }

    public async Task<List<VentaProductoDto>> GetByEstadoAsync(int estado)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.VentasProductos
            .AsNoTracking()
            .Include(v => v.Detalles)
            .ThenInclude(d => d.Producto)
            .Include(v => v.Miembro)
            .Where(v => v.Estado == (EstadoVenta)estado)
            .OrderByDescending(v => v.FechaVenta)
            .Select(v => MapToDto(v))
            .ToListAsync();
    }

    public async Task<List<VentaProductoDto>> GetByMiembroAsync(Guid miembroId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.VentasProductos
            .AsNoTracking()
            .Include(v => v.Detalles)
            .ThenInclude(d => d.Producto)
            .Include(v => v.Miembro)
            .Where(v => v.MiembroId == miembroId)
            .OrderByDescending(v => v.FechaVenta)
            .Select(v => MapToDto(v))
            .ToListAsync();
    }

    public async Task<VentaProductoDto?> GetByIdAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var venta = await context.VentasProductos
            .Include(v => v.Detalles)
            .ThenInclude(d => d.Producto)
            .Include(v => v.Miembro)
            .FirstOrDefaultAsync(v => v.Id == id);

        return venta == null ? null : MapToDto(venta);
    }

    public async Task<VentaProductoDto> CreateAsync(VentaProductoCreateDto dto, string? createdBy = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // Validar disponibilidad de stock
        foreach (var detalleDto in dto.Detalles)
        {
            var producto = await context.Productos.FindAsync(detalleDto.ProductoId);
            if (producto == null)
                throw new InvalidOperationException($"Producto {detalleDto.ProductoId} no encontrado");

            if (producto.StockActual < detalleDto.Cantidad)
                throw new InvalidOperationException($"Stock insuficiente para {producto.Nombre}. Disponible: {producto.StockActual}, Requerido: {detalleDto.Cantidad}");
        }

        // Calcular total
        decimal totalCOP = dto.Detalles.Sum(d => (d.Cantidad * d.PrecioUnitarioCOP) - (d.DescuentoCOP ?? 0));

        // Generar número de venta
        var numeroVenta = GenerarNumeroVenta(dto.FechaVenta);

        var venta = new VentaProducto
        {
            NumeroVenta = numeroVenta,
            FechaVenta = dto.FechaVenta,
            TipoCliente = (TipoCliente)dto.TipoCliente,
            MiembroId = dto.MiembroId,
            NombreCliente = dto.NombreCliente,
            IdentificacionCliente = dto.IdentificacionCliente,
            EmailCliente = dto.EmailCliente,
            TotalCOP = totalCOP,
            TotalUSD = dto.TotalUSD,
            MetodoPago = (MetodoPagoVenta)dto.MetodoPago,
            Estado = EstadoVenta.Pendiente,
            Observaciones = dto.Observaciones,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        // Agregar detalles
        foreach (var detalleDto in dto.Detalles)
        {
            var detalle = new DetalleVentaProducto
            {
                ProductoId = detalleDto.ProductoId,
                Cantidad = detalleDto.Cantidad,
                PrecioUnitarioCOP = detalleDto.PrecioUnitarioCOP,
                DescuentoCOP = detalleDto.DescuentoCOP,
                SubtotalCOP = (detalleDto.Cantidad * detalleDto.PrecioUnitarioCOP) - (detalleDto.DescuentoCOP ?? 0),
                Notas = detalleDto.Notas
            };
            venta.Detalles.Add(detalle);
        }

        context.VentasProductos.Add(venta);

        // Actualizar stock inmediatamente
        foreach (var detalle in venta.Detalles)
        {
            var producto = await context.Productos.FindAsync(detalle.ProductoId);
            if (producto != null)
            {
                var stockAnterior = producto.StockActual;
                producto.StockActual -= detalle.Cantidad;
                producto.UpdatedAt = DateTime.UtcNow;
                producto.UpdatedBy = createdBy;

                // Registrar movimiento de inventario
                var movimiento = new MovimientoInventario
                {
                    NumeroMovimiento = $"VENT-{numeroVenta}-{producto.Codigo}",
                    FechaMovimiento = DateTime.UtcNow,
                    Tipo = TipoMovimiento.SalidaVenta,
                    ProductoId = detalle.ProductoId,
                    Cantidad = detalle.Cantidad,
                    StockAnterior = stockAnterior,
                    StockNuevo = producto.StockActual,
                    VentaId = venta.Id,
                    Motivo = $"Venta {numeroVenta}",
                    Observaciones = venta.TipoCliente == TipoCliente.MiembroLocal 
                        ? $"Venta a miembro" 
                        : $"Venta a {venta.NombreCliente}",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdBy ?? "Sistema"
                };

                context.MovimientosInventario.Add(movimiento);
            }
        }

        await context.SaveChangesAsync();

        // Recargar con navegaciones
        var ventaCreada = await context.VentasProductos
            .Include(v => v.Detalles)
            .ThenInclude(d => d.Producto)
            .Include(v => v.Miembro)
            .FirstAsync(v => v.Id == venta.Id);

        return MapToDto(ventaCreada);
    }

    public async Task<bool> CambiarEstadoAsync(Guid id, int nuevoEstado, string? updatedBy = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var venta = await context.VentasProductos.FindAsync(id);
        if (venta == null) return false;

        venta.Estado = (EstadoVenta)nuevoEstado;
        venta.UpdatedAt = DateTime.UtcNow;
        venta.UpdatedBy = updatedBy;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RegistrarPagoAsync(Guid ventaId, Guid ingresoId, string? updatedBy = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var venta = await context.VentasProductos.FindAsync(ventaId);
        if (venta == null) return false;

        venta.IngresoId = ingresoId;
        venta.Estado = EstadoVenta.Pagada;
        venta.UpdatedAt = DateTime.UtcNow;
        venta.UpdatedBy = updatedBy;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RegistrarEntregaAsync(Guid ventaId, string? updatedBy = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var venta = await context.VentasProductos.FindAsync(ventaId);
        if (venta == null) return false;

        venta.Estado = EstadoVenta.Entregada;
        venta.UpdatedAt = DateTime.UtcNow;
        venta.UpdatedBy = updatedBy;

        await context.SaveChangesAsync();
        return true;
    }

    private static string GenerarNumeroVenta(DateTime fecha)
    {
        return $"VENT-{fecha:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    private static VentaProductoDto MapToDto(VentaProducto venta)
    {
        return new VentaProductoDto
        {
            Id = venta.Id,
            NumeroVenta = venta.NumeroVenta,
            FechaVenta = venta.FechaVenta,
            TipoCliente = venta.TipoCliente.ToString(),
            MiembroId = venta.MiembroId,
            NombreMiembro = venta.Miembro != null ? venta.Miembro.NombreCompleto : null,
            NombreCliente = venta.NombreCliente,
            IdentificacionCliente = venta.IdentificacionCliente,
            EmailCliente = venta.EmailCliente,
            TotalCOP = venta.TotalCOP,
            TotalUSD = venta.TotalUSD,
            MetodoPago = venta.MetodoPago.ToString(),
            Estado = venta.Estado.ToString(),
            ReciboId = venta.ReciboId,
            IngresoId = venta.IngresoId,
            Observaciones = venta.Observaciones,
            CreatedAt = venta.CreatedAt,
            Detalles = venta.Detalles.Select(d => new DetalleVentaProductoDto
            {
                Id = d.Id,
                ProductoId = d.ProductoId,
                CodigoProducto = d.Producto.Codigo,
                NombreProducto = d.Producto.Nombre,
                Cantidad = d.Cantidad,
                PrecioUnitarioCOP = d.PrecioUnitarioCOP,
                DescuentoCOP = d.DescuentoCOP,
                SubtotalCOP = d.SubtotalCOP,
                Notas = d.Notas
            }).ToList()
        };
    }
}
