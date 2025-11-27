using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs.Proveedores;
using Server.Models;

namespace Server.Services.Proveedores;

/// <summary>
/// Implementación del servicio de gestión de proveedores
/// Maneja toda la lógica de negocio para CRUD, búsqueda y estadísticas
/// </summary>
public class ProveedoresService : IProveedoresService
{
    private readonly AppDbContext _context;

    public ProveedoresService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<ProveedorDto> Proveedores, int TotalCount)> ObtenerProveedoresAsync(
        string? busqueda = null,
        bool? activo = null,
        int pagina = 1,
        int registrosPorPagina = 20)
    {
        var query = _context.Proveedores.AsNoTracking().AsQueryable();

        // Filtro de búsqueda
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            busqueda = busqueda.ToLower();
            query = query.Where(p =>
                p.Nombre.ToLower().Contains(busqueda) ||
                (p.Nit != null && p.Nit.ToLower().Contains(busqueda)) ||
                (p.ContactoEmail != null && p.ContactoEmail.ToLower().Contains(busqueda)) ||
                (p.ContactoNombre != null && p.ContactoNombre.ToLower().Contains(busqueda)));
        }

        // Filtro de estado
        if (activo.HasValue)
        {
            query = query.Where(p => p.Activo == activo.Value);
        }

        var totalCount = await query.CountAsync();

        // Obtener compras por proveedor
        var proveedores = await query
            .OrderBy(p => p.Nombre)
            .Skip((pagina - 1) * registrosPorPagina)
            .Take(registrosPorPagina)
            .Select(p => new ProveedorDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Nit = p.Nit ?? string.Empty,
                Telefono = p.ContactoTelefono,
                Email = p.ContactoEmail,
                Direccion = p.Direccion,
                Contacto = p.ContactoNombre,
                DiasCredito = p.DiasCredito,
                Calificacion = p.Calificacion,
                Activo = p.Activo,
                TotalCompras = p.Compras.Count,
                UltimaCompra = p.Compras
                    .OrderByDescending(c => c.FechaCompra)
                    .Select(c => c.FechaCompra)
                    .FirstOrDefault(),
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return (proveedores, totalCount);
    }

    public async Task<ProveedorDto?> ObtenerProveedorPorIdAsync(Guid id)
    {
        return await _context.Proveedores
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProveedorDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Nit = p.Nit ?? string.Empty,
                Telefono = p.ContactoTelefono,
                Email = p.ContactoEmail,
                Direccion = p.Direccion,
                Contacto = p.ContactoNombre,
                DiasCredito = p.DiasCredito,
                Calificacion = p.Calificacion,
                Activo = p.Activo,
                TotalCompras = p.Compras.Count,
                UltimaCompra = p.Compras
                    .OrderByDescending(c => c.FechaCompra)
                    .Select(c => c.FechaCompra)
                    .FirstOrDefault(),
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProveedorDetalleDto?> ObtenerProveedorDetalleAsync(Guid id)
    {
        var proveedor = await _context.Proveedores
            .Include(p => p.Compras)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (proveedor == null)
            return null;

        // Calcular estadísticas
        var totalCompradoCOP = proveedor.Compras.Sum(c => c.TotalCOP);
        var totalCompradoUSD = proveedor.Compras.Sum(c => c.TotalUSD ?? 0);
        var ultimaCompra = proveedor.Compras
            .OrderByDescending(c => c.FechaCompra)
            .FirstOrDefault();

        // Calcular próximo pago (última compra + días crédito)
        DateTime? proximoPago = null;
        if (ultimaCompra != null && proveedor.DiasCredito > 0)
        {
            proximoPago = ultimaCompra.FechaCompra.AddDays(proveedor.DiasCredito);
        }

        return new ProveedorDetalleDto
        {
            Id = proveedor.Id,
            Nombre = proveedor.Nombre,
            Nit = proveedor.Nit ?? string.Empty,
            Telefono = proveedor.ContactoTelefono,
            Email = proveedor.ContactoEmail,
            Direccion = proveedor.Direccion,
            Contacto = proveedor.ContactoNombre,
            TerminosPago = proveedor.TerminosPago,
            DiasCredito = proveedor.DiasCredito,
            Calificacion = proveedor.Calificacion,
            Activo = proveedor.Activo,
            TotalCompras = proveedor.Compras.Count,
            TotalCompradoCOP = totalCompradoCOP,
            TotalCompradoUSD = totalCompradoUSD,
            UltimaCompra = ultimaCompra?.FechaCompra,
            ProximoPago = proximoPago,
            CreatedAt = proveedor.CreatedAt,
            CreatedBy = proveedor.CreatedBy,
            UpdatedAt = proveedor.UpdatedAt,
            UpdatedBy = proveedor.UpdatedBy
        };
    }

    public async Task<(bool Success, string Message, Guid? ProveedorId)> CrearProveedorAsync(
        ProveedorFormDto dto,
        string usuarioId)
    {
        // Validar NIT único
            if (!string.IsNullOrWhiteSpace(dto.Nit) && await ExisteNitAsync(dto.Nit))
        {
            return (false, $"Ya existe un proveedor con el NIT {dto.Nit}", null);
        }

        // Validaciones de negocio
        if (string.IsNullOrWhiteSpace(dto.Nombre))
        {
            return (false, "El nombre del proveedor es requerido", null);
        }

        if (string.IsNullOrWhiteSpace(dto.Nit))
        {
            return (false, "El NIT del proveedor es requerido", null);
        }

        if (dto.DiasCredito < 0)
        {
            return (false, "Los días de crédito no pueden ser negativos", null);
        }

        if (dto.Calificacion.HasValue && (dto.Calificacion < 0 || dto.Calificacion > 5))
        {
            return (false, "La calificación debe estar entre 0 y 5", null);
        }

        // Crear proveedor
        var proveedor = new Proveedor
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            Nit = dto.Nit.Trim(),
            ContactoTelefono = dto.Telefono?.Trim(),
            ContactoEmail = dto.Email?.Trim(),
            Direccion = dto.Direccion?.Trim(),
            ContactoNombre = dto.Contacto?.Trim(),
            TerminosPago = dto.TerminosPago?.Trim(),
            DiasCredito = dto.DiasCredito,
            Calificacion = dto.Calificacion.HasValue ? (int?)dto.Calificacion.Value : null,
            Activo = dto.Activo,
            CreatedAt = DateTime.Now,
            CreatedBy = usuarioId
        };

        _context.Proveedores.Add(proveedor);
        await _context.SaveChangesAsync();

        return (true, "Proveedor creado exitosamente", proveedor.Id);
    }

    public async Task<(bool Success, string Message)> ActualizarProveedorAsync(
        Guid id,
        ProveedorFormDto dto,
        string usuarioId)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor == null)
        {
            return (false, "Proveedor no encontrado");
        }

        // Validar NIT único (excluyendo el proveedor actual)
        if (!string.IsNullOrWhiteSpace(dto.Nit) && await ExisteNitAsync(dto.Nit, id))
        {
            return (false, $"Ya existe otro proveedor con el NIT {dto.Nit}");
        }

        // Validaciones de negocio
        if (string.IsNullOrWhiteSpace(dto.Nombre))
        {
            return (false, "El nombre del proveedor es requerido");
        }

        if (string.IsNullOrWhiteSpace(dto.Nit))
        {
            return (false, "El NIT del proveedor es requerido");
        }

        if (dto.DiasCredito < 0)
        {
            return (false, "Los días de crédito no pueden ser negativos");
        }

        if (dto.Calificacion.HasValue && (dto.Calificacion < 0 || dto.Calificacion > 5))
        {
            return (false, "La calificación debe estar entre 0 y 5");
        }

        // Actualizar proveedor
        proveedor.Nombre = dto.Nombre.Trim();
        proveedor.Nit = dto.Nit.Trim();
        proveedor.ContactoTelefono = dto.Telefono?.Trim();
        proveedor.ContactoEmail = dto.Email?.Trim();
        proveedor.Direccion = dto.Direccion?.Trim();
        proveedor.ContactoNombre = dto.Contacto?.Trim();
        proveedor.TerminosPago = dto.TerminosPago?.Trim();
        proveedor.DiasCredito = dto.DiasCredito;
        proveedor.Calificacion = dto.Calificacion.HasValue ? (int?)dto.Calificacion.Value : null;
        proveedor.Activo = dto.Activo;
        proveedor.UpdatedAt = DateTime.Now;
        proveedor.UpdatedBy = usuarioId;

        await _context.SaveChangesAsync();

        return (true, "Proveedor actualizado exitosamente");
    }

    public async Task<(bool Success, string Message)> EliminarProveedorAsync(Guid id, string usuarioId)
    {
        var proveedor = await _context.Proveedores
            .Include(p => p.Compras)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (proveedor == null)
        {
            return (false, "Proveedor no encontrado");
        }

        // Validar si tiene compras asociadas
        if (proveedor.Compras.Any())
        {
            // No eliminar físicamente, solo desactivar
            proveedor.Activo = false;
            proveedor.UpdatedAt = DateTime.Now;
            proveedor.UpdatedBy = usuarioId;
            await _context.SaveChangesAsync();

            return (true, $"Proveedor desactivado exitosamente. Tiene {proveedor.Compras.Count} compra(s) asociada(s).");
        }

        // Si no tiene compras, eliminar físicamente
        _context.Proveedores.Remove(proveedor);
        await _context.SaveChangesAsync();

        return (true, "Proveedor eliminado exitosamente");
    }

    public async Task<List<CompraProveedorDto>> ObtenerHistorialComprasAsync(
        Guid proveedorId,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null)
    {
        var query = _context.ComprasProductos
            .Where(c => c.ProveedorId == proveedorId);

        if (fechaDesde.HasValue)
        {
            query = query.Where(c => c.FechaCompra >= fechaDesde.Value);
        }

        if (fechaHasta.HasValue)
        {
            query = query.Where(c => c.FechaCompra <= fechaHasta.Value);
        }

        return await query
            .OrderByDescending(c => c.FechaCompra)
            .Select(c => new CompraProveedorDto
            {
                CompraId = c.Id,
                NumeroOrden = c.NumeroCompra,
                FechaCompra = c.FechaCompra,
                FechaRecepcion = c.Estado == EstadoCompra.Recibida ? (DateTime?)c.UpdatedAt : null,
                Estado = c.Estado.ToString(),
                TotalCOP = c.TotalCOP,
                TotalUSD = c.TotalUSD,
                TotalProductos = c.Detalles.Count,
                Observaciones = c.Observaciones
            })
            .ToListAsync();
    }

    public async Task<bool> ExisteNitAsync(string nit, Guid? proveedorIdExcluir = null)
    {
        var query = _context.Proveedores.Where(p => p.Nit != null && p.Nit == nit);

        if (proveedorIdExcluir.HasValue)
        {
            query = query.Where(p => p.Id != proveedorIdExcluir.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<List<ProveedorSimpleDto>> ObtenerProveedoresActivosAsync()
    {
        return await _context.Proveedores
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .Select(p => new ProveedorSimpleDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Nit = p.Nit ?? string.Empty,
                DiasCredito = p.DiasCredito
            })
            .ToListAsync();
    }
}
