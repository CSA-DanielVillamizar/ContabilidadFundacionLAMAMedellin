using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs.Clientes;
using Server.Models;

namespace Server.Services.Clientes;

/// <summary>
/// Implementación del servicio de gestión de clientes
/// Maneja toda la lógica de negocio para CRUD, búsqueda y estadísticas
/// </summary>
public class ClientesService : IClientesService
{
    private readonly AppDbContext _context;

    public ClientesService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<ClienteDto> Clientes, int TotalCount)> ObtenerClientesAsync(
        string? busqueda = null,
        bool? activo = null,
        string? tipo = null,
        int pagina = 1,
        int registrosPorPagina = 20)
    {
        var query = _context.Clientes.AsNoTracking().AsQueryable();

        // Filtro de búsqueda
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
              busqueda = busqueda?.ToLower() ?? string.Empty;
            query = query.Where(c =>
                (c.Nombre ?? string.Empty).ToLower().Contains(busqueda) ||
                (c.Identificacion ?? string.Empty).ToLower().Contains(busqueda) ||
                (c.Email != null && c.Email.ToLower().Contains(busqueda)) ||
                (c.Telefono != null && c.Telefono.ToLower().Contains(busqueda)));
        }

        // Filtro de estado
        if (activo.HasValue)
        {
            query = query.Where(c => c.Activo == activo.Value);
        }

        // Filtro de tipo
        if (!string.IsNullOrWhiteSpace(tipo))
        {
            query = query.Where(c => c.Tipo == tipo);
        }

        var totalCount = await query.CountAsync();

        var clientes = await query
            .OrderBy(c => c.Nombre)
            .Skip((pagina - 1) * registrosPorPagina)
            .Take(registrosPorPagina)
            .Select(c => new ClienteDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Identificacion = c.Identificacion ?? string.Empty,
                Tipo = c.Tipo,
                Telefono = c.Telefono,
                Email = c.Email,
                LimiteCredito = c.LimiteCredito,
                PuntosFidelizacion = c.PuntosFidelizacion,
                Activo = c.Activo,
                TotalVentas = c.Ventas.Count,
                UltimaVenta = c.Ventas
                    .OrderByDescending(v => v.FechaVenta)
                    .Select(v => v.FechaVenta)
                    .FirstOrDefault(),
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        return (clientes, totalCount);
    }

    public async Task<ClienteDto?> ObtenerClientePorIdAsync(Guid id)
    {
        return await _context.Clientes
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new ClienteDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Identificacion = c.Identificacion ?? string.Empty,
                Tipo = c.Tipo,
                Telefono = c.Telefono,
                Email = c.Email,
                LimiteCredito = c.LimiteCredito,
                PuntosFidelizacion = c.PuntosFidelizacion,
                Activo = c.Activo,
                TotalVentas = c.Ventas.Count,
                UltimaVenta = c.Ventas
                    .OrderByDescending(v => v.FechaVenta)
                    .Select(v => v.FechaVenta)
                    .FirstOrDefault(),
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ClienteDetalleDto?> ObtenerClienteDetalleAsync(Guid id)
    {
        var cliente = await _context.Clientes
            .Include(c => c.Ventas)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente == null)
            return null;

        // Calcular estadísticas
        var totalCompradoCOP = cliente.Ventas.Sum(v => v.TotalCOP);
        var totalCompradoUSD = cliente.Ventas.Sum(v => v.TotalUSD ?? 0);
        var ultimaVenta = cliente.Ventas
            .OrderByDescending(v => v.FechaVenta)
            .FirstOrDefault();
        var promedioCompra = cliente.Ventas.Any() ? totalCompradoCOP / cliente.Ventas.Count : 0;

        return new ClienteDetalleDto
        {
            Id = cliente.Id,
            Nombre = cliente.Nombre,
            Identificacion = cliente.Identificacion ?? string.Empty,
            Tipo = cliente.Tipo,
            Telefono = cliente.Telefono,
            Email = cliente.Email,
            Direccion = cliente.Direccion,
            LimiteCredito = cliente.LimiteCredito,
            PuntosFidelizacion = cliente.PuntosFidelizacion,
            Activo = cliente.Activo,
            TotalVentas = cliente.Ventas.Count,
            TotalCompradoCOP = totalCompradoCOP,
            TotalCompradoUSD = totalCompradoUSD,
            UltimaVenta = ultimaVenta?.FechaVenta,
            PromedioCompra = promedioCompra,
            CreatedAt = cliente.CreatedAt,
            CreatedBy = cliente.CreatedBy,
            UpdatedAt = cliente.UpdatedAt,
            UpdatedBy = cliente.UpdatedBy
        };
    }

    public async Task<(bool Success, string Message, Guid? ClienteId)> CrearClienteAsync(
        ClienteFormDto dto,
        string usuarioId)
    {
        // Validar identificación única
        if (await ExisteIdentificacionAsync(dto.Identificacion))
        {
            return (false, $"Ya existe un cliente con la identificación {dto.Identificacion}", null);
        }

        // Validaciones de negocio
        if (string.IsNullOrWhiteSpace(dto.Nombre))
        {
            return (false, "El nombre del cliente es requerido", null);
        }

        if (string.IsNullOrWhiteSpace(dto.Identificacion))
        {
            return (false, "La identificación del cliente es requerida", null);
        }

        // Crear cliente
        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            Identificacion = dto.Identificacion.Trim(),
            Tipo = dto.Tipo,
            Telefono = dto.Telefono?.Trim(),
            Email = dto.Email?.Trim(),
            Direccion = dto.Direccion?.Trim(),
            LimiteCredito = dto.LimiteCredito,
            PuntosFidelizacion = dto.PuntosFidelizacion,
            Activo = dto.Activo,
            CreatedAt = DateTime.Now,
            CreatedBy = usuarioId
        };

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return (true, "Cliente creado exitosamente", cliente.Id);
    }

    public async Task<(bool Success, string Message)> ActualizarClienteAsync(
        Guid id,
        ClienteFormDto dto,
        string usuarioId)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
        {
            return (false, "Cliente no encontrado");
        }

        // Validar identificación única (excluyendo el cliente actual)
        if (await ExisteIdentificacionAsync(dto.Identificacion, id))
        {
            return (false, $"Ya existe otro cliente con la identificación {dto.Identificacion}");
        }

        // Validaciones de negocio
        if (string.IsNullOrWhiteSpace(dto.Nombre))
        {
            return (false, "El nombre del cliente es requerido");
        }

        if (string.IsNullOrWhiteSpace(dto.Identificacion))
        {
            return (false, "La identificación del cliente es requerida");
        }

        // Actualizar cliente
        cliente.Nombre = dto.Nombre.Trim();
        cliente.Identificacion = dto.Identificacion.Trim();
        cliente.Tipo = dto.Tipo;
        cliente.Telefono = dto.Telefono?.Trim();
        cliente.Email = dto.Email?.Trim();
        cliente.Direccion = dto.Direccion?.Trim();
        cliente.LimiteCredito = dto.LimiteCredito;
        cliente.PuntosFidelizacion = dto.PuntosFidelizacion;
        cliente.Activo = dto.Activo;
        cliente.UpdatedAt = DateTime.Now;
        cliente.UpdatedBy = usuarioId;

        await _context.SaveChangesAsync();

        return (true, "Cliente actualizado exitosamente");
    }

    public async Task<(bool Success, string Message)> EliminarClienteAsync(Guid id, string usuarioId)
    {
        var cliente = await _context.Clientes
            .Include(c => c.Ventas)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente == null)
        {
            return (false, "Cliente no encontrado");
        }

        // Validar si tiene ventas asociadas
        if (cliente.Ventas.Any())
        {
            // No eliminar físicamente, solo desactivar
            cliente.Activo = false;
            cliente.UpdatedAt = DateTime.Now;
            cliente.UpdatedBy = usuarioId;
            await _context.SaveChangesAsync();

            return (true, $"Cliente desactivado exitosamente. Tiene {cliente.Ventas.Count} venta(s) asociada(s).");
        }

        // Si no tiene ventas ni cotizaciones, eliminar físicamente
        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();

        return (true, "Cliente eliminado exitosamente");
    }

    public async Task<List<VentaClienteDto>> ObtenerHistorialVentasAsync(
        Guid clienteId,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null)
    {
        var query = _context.VentasProductos
            .Where(v => v.ClienteId == clienteId);

        if (fechaDesde.HasValue)
        {
            query = query.Where(v => v.FechaVenta >= fechaDesde.Value);
        }

        if (fechaHasta.HasValue)
        {
            query = query.Where(v => v.FechaVenta <= fechaHasta.Value);
        }

        return await query
            .OrderByDescending(v => v.FechaVenta)
            .Select(v => new VentaClienteDto
            {
                VentaId = v.Id,
                NumeroVenta = v.NumeroVenta,
                FechaVenta = v.FechaVenta,
                Estado = v.Estado.ToString(),
                TotalCOP = v.TotalCOP,
                TotalUSD = v.TotalUSD,
                TotalProductos = v.Detalles.Count,
                Observaciones = v.Observaciones
            })
            .ToListAsync();
    }

    public async Task<bool> ExisteIdentificacionAsync(string identificacion, Guid? clienteIdExcluir = null)
    {
        var query = _context.Clientes.Where(c => c.Identificacion == identificacion);

        if (clienteIdExcluir.HasValue)
        {
            query = query.Where(c => c.Id != clienteIdExcluir.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<List<ClienteSimpleDto>> ObtenerClientesActivosAsync()
    {
        return await _context.Clientes
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .Select(c => new ClienteSimpleDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Identificacion = c.Identificacion ?? string.Empty,
                Tipo = c.Tipo,
                LimiteCredito = c.LimiteCredito
            })
            .ToListAsync();
    }
}
