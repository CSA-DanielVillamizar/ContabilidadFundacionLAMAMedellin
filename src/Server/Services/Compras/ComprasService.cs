using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs.Compras;
using Server.Models;

namespace Server.Services.Compras;

/// <summary>
/// Servicio para gestión de compras de productos
/// </summary>
public class ComprasService : IComprasService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public ComprasService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<CompraProductoDto>> GetAllAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.ComprasProductos
            .AsNoTracking()
            .Include(c => c.Detalles)
            .ThenInclude(d => d.Producto)
            .OrderByDescending(c => c.FechaCompra)
            .Select(c => MapToDto(c))
            .ToListAsync();
    }

    public async Task<List<CompraProductoDto>> GetByEstadoAsync(int estado)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.ComprasProductos
            .AsNoTracking()
            .Include(c => c.Detalles)
            .ThenInclude(d => d.Producto)
            .Where(c => c.Estado == (EstadoCompra)estado)
            .OrderByDescending(c => c.FechaCompra)
            .Select(c => MapToDto(c))
            .ToListAsync();
    }

    public async Task<CompraProductoDto?> GetByIdAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var compra = await context.ComprasProductos
            .Include(c => c.Detalles)
            .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(c => c.Id == id);

        return compra == null ? null : MapToDto(compra);
    }

    public async Task<CompraProductoDto> CreateAsync(CompraProductoCreateDto dto, string? createdBy = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // Calcular total
        decimal totalCOP = dto.Detalles.Sum(d => d.Cantidad * d.PrecioUnitarioCOP);

        // Generar número de compra
        var ultimaCompra = await context.ComprasProductos
            .OrderByDescending(c => c.FechaCompra)
            .FirstOrDefaultAsync();
        
        var numeroCompra = GenerarNumeroCompra(dto.FechaCompra);

        var compra = new CompraProducto
        {
            NumeroCompra = numeroCompra,
            FechaCompra = dto.FechaCompra,
            Proveedor = dto.Proveedor,
            TotalCOP = totalCOP,
            TotalUSD = dto.TotalUSD,
            TrmAplicada = dto.TrmAplicada,
            Estado = EstadoCompra.Pendiente,
            NumeroFacturaProveedor = dto.NumeroFacturaProveedor,
            Observaciones = dto.Observaciones,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        // Si viene ProveedorId, vincular y asegurar nombre
        if (dto.ProveedorId.HasValue)
        {
            var prov = await context.Proveedores.FirstOrDefaultAsync(p => p.Id == dto.ProveedorId.Value);
            if (prov != null)
            {
                compra.ProveedorId = prov.Id;
                compra.Proveedor = prov.Nombre; // mantener nombre para reportes rápidos
            }
        }

        // Agregar detalles
        foreach (var detalleDto in dto.Detalles)
        {
            var detalle = new DetalleCompraProducto
            {
                ProductoId = detalleDto.ProductoId,
                Cantidad = detalleDto.Cantidad,
                PrecioUnitarioCOP = detalleDto.PrecioUnitarioCOP,
                SubtotalCOP = detalleDto.Cantidad * detalleDto.PrecioUnitarioCOP,
                Notas = detalleDto.Notas
            };
            compra.Detalles.Add(detalle);
        }

        context.ComprasProductos.Add(compra);
        await context.SaveChangesAsync();

        // Recargar con navegaciones para devolver DTO completo
        var compraCreada = await context.ComprasProductos
            .Include(c => c.Detalles)
            .ThenInclude(d => d.Producto)
            .FirstAsync(c => c.Id == compra.Id);

        return MapToDto(compraCreada);
    }

    public async Task<bool> CambiarEstadoAsync(Guid id, int nuevoEstado, string? updatedBy = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var compra = await context.ComprasProductos.FindAsync(id);
        if (compra == null) return false;

        compra.Estado = (EstadoCompra)nuevoEstado;
        compra.UpdatedAt = DateTime.UtcNow;
        compra.UpdatedBy = updatedBy;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RegistrarPagoAsync(Guid compraId, Guid egresoId, string? updatedBy = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var compra = await context.ComprasProductos.FindAsync(compraId);
        if (compra == null) return false;

        compra.EgresoId = egresoId;
        compra.Estado = EstadoCompra.Pagada;
        compra.UpdatedAt = DateTime.UtcNow;
        compra.UpdatedBy = updatedBy;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RegistrarRecepcionAsync(Guid compraId, string? updatedBy = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var compra = await context.ComprasProductos
            .Include(c => c.Detalles)
            .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(c => c.Id == compraId);

        if (compra == null) return false;

        // Actualizar stock de productos
        foreach (var detalle in compra.Detalles)
        {
            var producto = detalle.Producto;
            var stockAnterior = producto.StockActual;
            producto.StockActual += detalle.Cantidad;
            producto.UpdatedAt = DateTime.UtcNow;
            producto.UpdatedBy = updatedBy;

            // Registrar movimiento de inventario
            var movimiento = new MovimientoInventario
            {
                NumeroMovimiento = $"COMP-{compra.NumeroCompra}-{detalle.Producto.Codigo}",
                FechaMovimiento = DateTime.UtcNow,
                Tipo = TipoMovimiento.EntradaCompra,
                ProductoId = detalle.ProductoId,
                Cantidad = detalle.Cantidad,
                StockAnterior = stockAnterior,
                StockNuevo = producto.StockActual,
                CompraId = compraId,
                Motivo = $"Recepción de compra {compra.NumeroCompra}",
                Observaciones = $"Proveedor: {compra.Proveedor}",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = updatedBy ?? "Sistema"
            };

            context.MovimientosInventario.Add(movimiento);
        }

        // Actualizar estado de compra
        compra.Estado = EstadoCompra.Recibida;
        compra.UpdatedAt = DateTime.UtcNow;
        compra.UpdatedBy = updatedBy;

        await context.SaveChangesAsync();
        return true;
    }

    private static string GenerarNumeroCompra(DateTime fecha)
    {
        return $"COMP-{fecha:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    private static CompraProductoDto MapToDto(CompraProducto compra)
    {
        return new CompraProductoDto
        {
            Id = compra.Id,
            NumeroCompra = compra.NumeroCompra,
            FechaCompra = compra.FechaCompra,
            Proveedor = compra.Proveedor,
            TotalCOP = compra.TotalCOP,
            TotalUSD = compra.TotalUSD,
            TrmAplicada = compra.TrmAplicada,
            Estado = compra.Estado.ToString(),
                NumeroFacturaProveedor = compra.NumeroFacturaProveedor ?? string.Empty,
                Observaciones = compra.Observaciones ?? string.Empty,
            EgresoId = compra.EgresoId,
            CreatedAt = compra.CreatedAt,
            Detalles = compra.Detalles.Select(d => new DetalleCompraProductoDto
            {
                Id = d.Id,
                ProductoId = d.ProductoId,
                CodigoProducto = d.Producto.Codigo,
                NombreProducto = d.Producto.Nombre,
                Cantidad = d.Cantidad,
                PrecioUnitarioCOP = d.PrecioUnitarioCOP,
                SubtotalCOP = d.SubtotalCOP,
                Notas = d.Notas
            }).ToList()
        };
    }
}
