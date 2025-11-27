using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs.Productos;
using Server.Models;

namespace Server.Services.Productos;

/// <summary>
/// Servicio para gestión de productos
/// </summary>
public class ProductosService : IProductosService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public ProductosService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<ProductoDto>> GetAllAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        return await context.Productos
            .AsNoTracking()
            .OrderBy(p => p.Nombre)
            .Select(p => new ProductoDto
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Tipo = p.Tipo.ToString(),
                PrecioVentaCOP = p.PrecioVentaCOP,
                PrecioVentaUSD = p.PrecioVentaUSD,
                StockActual = p.StockActual,
                StockMinimo = p.StockMinimo,
                Talla = p.Talla,
                Descripcion = p.Descripcion,
                EsParcheOficial = p.EsParcheOficial,
                ImagenUrl = p.ImagenUrl,
                Activo = p.Activo,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<ProductoDto>> GetActivosAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        return await context.Productos
            .AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .Select(p => new ProductoDto
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Tipo = p.Tipo.ToString(),
                PrecioVentaCOP = p.PrecioVentaCOP,
                PrecioVentaUSD = p.PrecioVentaUSD,
                StockActual = p.StockActual,
                StockMinimo = p.StockMinimo,
                Talla = p.Talla,
                Descripcion = p.Descripcion,
                EsParcheOficial = p.EsParcheOficial,
                ImagenUrl = p.ImagenUrl,
                Activo = p.Activo,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<ProductoDto>> GetBajoStockAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        return await context.Productos
            .AsNoTracking()
            .Where(p => p.Activo && p.StockActual <= p.StockMinimo)
            .OrderBy(p => p.StockActual)
            .Select(p => new ProductoDto
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Tipo = p.Tipo.ToString(),
                PrecioVentaCOP = p.PrecioVentaCOP,
                PrecioVentaUSD = p.PrecioVentaUSD,
                StockActual = p.StockActual,
                StockMinimo = p.StockMinimo,
                Talla = p.Talla,
                Descripcion = p.Descripcion,
                EsParcheOficial = p.EsParcheOficial,
                ImagenUrl = p.ImagenUrl,
                Activo = p.Activo,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ProductoDto?> GetByIdAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var producto = await context.Productos.FindAsync(id);
        if (producto == null) return null;

        return new ProductoDto
        {
            Id = producto.Id,
            Codigo = producto.Codigo,
            Nombre = producto.Nombre,
            Tipo = producto.Tipo.ToString(),
            PrecioVentaCOP = producto.PrecioVentaCOP,
            PrecioVentaUSD = producto.PrecioVentaUSD,
            StockActual = producto.StockActual,
            StockMinimo = producto.StockMinimo,
            Talla = producto.Talla,
            Descripcion = producto.Descripcion,
            EsParcheOficial = producto.EsParcheOficial,
            ImagenUrl = producto.ImagenUrl,
            Activo = producto.Activo,
            CreatedAt = producto.CreatedAt
        };
    }

    public async Task<ProductoDto?> GetByCodigoAsync(string codigo)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var producto = await context.Productos
            .FirstOrDefaultAsync(p => p.Codigo == codigo);
        
        if (producto == null) return null;

        return new ProductoDto
        {
            Id = producto.Id,
            Codigo = producto.Codigo,
            Nombre = producto.Nombre,
            Tipo = producto.Tipo.ToString(),
            PrecioVentaCOP = producto.PrecioVentaCOP,
            PrecioVentaUSD = producto.PrecioVentaUSD,
            StockActual = producto.StockActual,
            StockMinimo = producto.StockMinimo,
            Talla = producto.Talla,
            Descripcion = producto.Descripcion,
            EsParcheOficial = producto.EsParcheOficial,
            ImagenUrl = producto.ImagenUrl,
            Activo = producto.Activo,
            CreatedAt = producto.CreatedAt
        };
    }

    public async Task<ProductoDto> CreateAsync(ProductoCreateUpdateDto dto, string? createdBy = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var producto = new Producto
        {
            Codigo = dto.Codigo,
            Nombre = dto.Nombre,
            Tipo = (TipoProducto)dto.Tipo,
            PrecioVentaCOP = dto.PrecioVentaCOP,
            PrecioVentaUSD = dto.PrecioVentaUSD,
            StockActual = dto.StockActual,
            StockMinimo = dto.StockMinimo,
            Talla = dto.Talla,
            Descripcion = dto.Descripcion,
            EsParcheOficial = dto.EsParcheOficial,
            ImagenUrl = dto.ImagenUrl,
            Activo = dto.Activo,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        context.Productos.Add(producto);
        await context.SaveChangesAsync();

        return new ProductoDto
        {
            Id = producto.Id,
            Codigo = producto.Codigo,
            Nombre = producto.Nombre,
            Tipo = producto.Tipo.ToString(),
            PrecioVentaCOP = producto.PrecioVentaCOP,
            PrecioVentaUSD = producto.PrecioVentaUSD,
            StockActual = producto.StockActual,
            StockMinimo = producto.StockMinimo,
            Talla = producto.Talla,
            Descripcion = producto.Descripcion,
            EsParcheOficial = producto.EsParcheOficial,
            ImagenUrl = producto.ImagenUrl,
            Activo = producto.Activo,
            CreatedAt = producto.CreatedAt
        };
    }

    public async Task<ProductoDto> UpdateAsync(Guid id, ProductoCreateUpdateDto dto, string? updatedBy = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var producto = await context.Productos.FindAsync(id);
        if (producto == null)
            throw new InvalidOperationException($"Producto con ID {id} no encontrado");

        producto.Codigo = dto.Codigo;
        producto.Nombre = dto.Nombre;
        producto.Tipo = (TipoProducto)dto.Tipo;
        producto.PrecioVentaCOP = dto.PrecioVentaCOP;
        producto.PrecioVentaUSD = dto.PrecioVentaUSD;
        producto.StockActual = dto.StockActual;
        producto.StockMinimo = dto.StockMinimo;
        producto.Talla = dto.Talla;
        producto.Descripcion = dto.Descripcion;
        producto.EsParcheOficial = dto.EsParcheOficial;
        producto.ImagenUrl = dto.ImagenUrl;
        producto.Activo = dto.Activo;
        producto.UpdatedAt = DateTime.UtcNow;
        producto.UpdatedBy = updatedBy;

        await context.SaveChangesAsync();

        return new ProductoDto
        {
            Id = producto.Id,
            Codigo = producto.Codigo,
            Nombre = producto.Nombre,
            Tipo = producto.Tipo.ToString(),
            PrecioVentaCOP = producto.PrecioVentaCOP,
            PrecioVentaUSD = producto.PrecioVentaUSD,
            StockActual = producto.StockActual,
            StockMinimo = producto.StockMinimo,
            Talla = producto.Talla,
            Descripcion = producto.Descripcion,
            EsParcheOficial = producto.EsParcheOficial,
            ImagenUrl = producto.ImagenUrl,
            Activo = producto.Activo,
            CreatedAt = producto.CreatedAt
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var producto = await context.Productos.FindAsync(id);
        if (producto == null) return false;

        context.Productos.Remove(producto);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActivarDesactivarAsync(Guid id, bool activo, string? updatedBy = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var producto = await context.Productos.FindAsync(id);
        if (producto == null) return false;

        producto.Activo = activo;
        producto.UpdatedAt = DateTime.UtcNow;
        producto.UpdatedBy = updatedBy;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<int> AjustarStockAsync(Guid productoId, int nuevaCantidad, string motivo, string? createdBy = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var producto = await context.Productos.FindAsync(productoId);
        if (producto == null)
            throw new InvalidOperationException($"Producto con ID {productoId} no encontrado");

        var stockAnterior = producto.StockActual;
        producto.StockActual = nuevaCantidad;
        producto.UpdatedAt = DateTime.UtcNow;
        producto.UpdatedBy = createdBy;

        // Registrar movimiento de inventario
        var tipoMovimiento = nuevaCantidad > stockAnterior
            ? TipoMovimiento.AjustePositivo
            : TipoMovimiento.AjusteNegativo;

        var movimiento = new MovimientoInventario
        {
            NumeroMovimiento = $"ADJ-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            FechaMovimiento = DateTime.UtcNow,
            Tipo = tipoMovimiento,
            ProductoId = productoId,
            Cantidad = Math.Abs(nuevaCantidad - stockAnterior),
            StockAnterior = stockAnterior,
            StockNuevo = nuevaCantidad,
            Motivo = motivo,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy ?? "Sistema"
        };

        context.MovimientosInventario.Add(movimiento);
        await context.SaveChangesAsync();

        return producto.StockActual;
    }
}
