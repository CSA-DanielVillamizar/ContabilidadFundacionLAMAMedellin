using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs.Inventario;
using Server.Models;

namespace Server.Services.Inventario;

/// <summary>
/// Servicio para gestión de inventario y movimientos
/// </summary>
public class InventarioService : IInventarioService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public InventarioService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<MovimientoInventarioDto>> GetAllMovimientosAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.MovimientosInventario
            .Include(m => m.Producto)
            .OrderByDescending(m => m.FechaMovimiento)
            .Select(m => MapToDto(m))
            .ToListAsync();
    }

    public async Task<List<MovimientoInventarioDto>> GetMovimientosByProductoAsync(Guid productoId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.MovimientosInventario
            .Include(m => m.Producto)
            .Where(m => m.ProductoId == productoId)
            .OrderByDescending(m => m.FechaMovimiento)
            .Select(m => MapToDto(m))
            .ToListAsync();
    }

    public async Task<List<MovimientoInventarioDto>> GetMovimientosByTipoAsync(int tipo)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.MovimientosInventario
            .Include(m => m.Producto)
            .Where(m => m.Tipo == (TipoMovimiento)tipo)
            .OrderByDescending(m => m.FechaMovimiento)
            .Select(m => MapToDto(m))
            .ToListAsync();
    }

    public async Task<List<MovimientoInventarioDto>> GetMovimientosByFechaAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.MovimientosInventario
            .Include(m => m.Producto)
            .Where(m => m.FechaMovimiento >= fechaInicio && m.FechaMovimiento <= fechaFin)
            .OrderByDescending(m => m.FechaMovimiento)
            .Select(m => MapToDto(m))
            .ToListAsync();
    }

    public async Task<MovimientoInventarioDto?> GetMovimientoByIdAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var movimiento = await context.MovimientosInventario
            .Include(m => m.Producto)
            .FirstOrDefaultAsync(m => m.Id == id);

        return movimiento == null ? null : MapToDto(movimiento);
    }

    public async Task<MovimientoInventarioDto> CreateMovimientoManualAsync(MovimientoInventarioCreateDto dto, string? createdBy = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var producto = await context.Productos.FindAsync(dto.ProductoId);
        if (producto == null)
            throw new InvalidOperationException($"Producto {dto.ProductoId} no encontrado");

        var tipoMovimiento = (TipoMovimiento)dto.Tipo;

        // Validar tipo de movimiento manual permitido
        var tiposPermitidos = new[]
        {
            TipoMovimiento.AjustePositivo,
            TipoMovimiento.AjusteNegativo,
            TipoMovimiento.DevolucionCliente,
            TipoMovimiento.DevolucionProveedor,
            TipoMovimiento.Merma,
            TipoMovimiento.Donacion
        };

        if (!tiposPermitidos.Contains(tipoMovimiento))
            throw new InvalidOperationException($"Tipo de movimiento {tipoMovimiento} no permitido para creación manual");

        var stockAnterior = producto.StockActual;
        int stockNuevo;

        // Calcular nuevo stock según tipo de movimiento
        switch (tipoMovimiento)
        {
            case TipoMovimiento.AjustePositivo:
            case TipoMovimiento.DevolucionCliente:
                stockNuevo = stockAnterior + dto.Cantidad;
                break;

            case TipoMovimiento.AjusteNegativo:
            case TipoMovimiento.DevolucionProveedor:
            case TipoMovimiento.Merma:
            case TipoMovimiento.Donacion:
                if (stockAnterior < dto.Cantidad)
                    throw new InvalidOperationException($"Stock insuficiente. Disponible: {stockAnterior}, Requerido: {dto.Cantidad}");
                stockNuevo = stockAnterior - dto.Cantidad;
                break;

            default:
                throw new InvalidOperationException($"Tipo de movimiento {tipoMovimiento} no soportado");
        }

        // Actualizar stock del producto
        producto.StockActual = stockNuevo;
        producto.UpdatedAt = DateTime.UtcNow;
        producto.UpdatedBy = createdBy;

        // Crear movimiento
        var movimiento = new MovimientoInventario
        {
            NumeroMovimiento = GenerarNumeroMovimiento(tipoMovimiento),
            FechaMovimiento = DateTime.UtcNow,
            Tipo = tipoMovimiento,
            ProductoId = dto.ProductoId,
            Cantidad = dto.Cantidad,
            StockAnterior = stockAnterior,
            StockNuevo = stockNuevo,
            Motivo = dto.Motivo,
            Observaciones = dto.Observaciones,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy ?? "Sistema"
        };

        context.MovimientosInventario.Add(movimiento);
        await context.SaveChangesAsync();

        // Recargar con navegaciones
        var movimientoCreado = await context.MovimientosInventario
            .Include(m => m.Producto)
            .FirstAsync(m => m.Id == movimiento.Id);

        return MapToDto(movimientoCreado);
    }

    private static string GenerarNumeroMovimiento(TipoMovimiento tipo)
    {
        var prefijo = tipo switch
        {
            TipoMovimiento.AjustePositivo => "ADJ+",
            TipoMovimiento.AjusteNegativo => "ADJ-",
            TipoMovimiento.DevolucionCliente => "DEVC",
            TipoMovimiento.DevolucionProveedor => "DEVP",
            TipoMovimiento.Merma => "MERM",
            TipoMovimiento.Donacion => "DONA",
            _ => "MOV"
        };

        return $"{prefijo}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    private static MovimientoInventarioDto MapToDto(MovimientoInventario movimiento)
    {
        return new MovimientoInventarioDto
        {
            Id = movimiento.Id,
            NumeroMovimiento = movimiento.NumeroMovimiento,
            FechaMovimiento = movimiento.FechaMovimiento,
            Tipo = movimiento.Tipo.ToString(),
            ProductoId = movimiento.ProductoId,
            CodigoProducto = movimiento.Producto.Codigo,
            NombreProducto = movimiento.Producto.Nombre,
            Cantidad = movimiento.Cantidad,
            StockAnterior = movimiento.StockAnterior,
            StockNuevo = movimiento.StockNuevo,
            Motivo = movimiento.Motivo,
            Observaciones = movimiento.Observaciones,
            CompraId = movimiento.CompraId,
            VentaId = movimiento.VentaId,
            CreatedAt = movimiento.CreatedAt,
            CreatedBy = movimiento.CreatedBy
        };
    }
}
