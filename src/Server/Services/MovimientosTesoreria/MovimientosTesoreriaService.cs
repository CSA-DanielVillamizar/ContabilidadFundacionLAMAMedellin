using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using Server.Services.CierreContable;
using Server.Services.Audit;

namespace Server.Services.MovimientosTesoreria;

/// <summary>
/// Servicio centralizado para gestión de MovimientosTesoreria con validación de cierre contable.
/// REGLA CRÍTICA: Ninguna operación de creación, actualización, anulación o eliminación
/// puede ejecutarse sobre movimientos cuya fecha pertenezca a un mes cerrado.
/// </summary>
public class MovimientosTesoreriaService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly CierreContableService _cierreService;
    private readonly IAuditService _auditService;

    public MovimientosTesoreriaService(
        IDbContextFactory<AppDbContext> contextFactory,
        CierreContableService cierreService,
        IAuditService auditService)
    {
        _contextFactory = contextFactory;
        _cierreService = cierreService;
        _auditService = auditService;
    }

    /// <summary>
    /// Verifica que la fecha NO esté en un mes cerrado. Lanza excepción si está cerrado.
    /// </summary>
    private async Task EnsureMesAbiertoAsync(DateTime fecha)
    {
        var esCerrado = await _cierreService.EsFechaCerradaAsync(fecha);
        if (esCerrado)
        {
            throw new InvalidOperationException(
                $"❌ Mes cerrado: no se permiten cambios en movimientos de tesorería para {fecha:MM/yyyy}. Contacte Admin.");
        }
    }

    /// <summary>
    /// Crea un nuevo movimiento de tesorería.
    /// Valida que el mes NO esté cerrado.
    /// </summary>
    public async Task<MovimientoTesoreria> CreateAsync(MovimientoTesoreria movimiento, string usuario)
    {
        // Validación crítica: mes abierto
        await EnsureMesAbiertoAsync(movimiento.Fecha);

        // Validaciones de negocio
        if (string.IsNullOrWhiteSpace(movimiento.NumeroMovimiento))
            throw new ArgumentException("El número de movimiento es obligatorio.");

        if (movimiento.CuentaFinancieraId == Guid.Empty)
            throw new ArgumentException("La cuenta financiera es obligatoria.");

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Evitar duplicado de número
        var existe = await context.MovimientosTesoreria
            .AnyAsync(m => m.NumeroMovimiento == movimiento.NumeroMovimiento);
        if (existe)
            throw new InvalidOperationException($"Ya existe un movimiento con número {movimiento.NumeroMovimiento}.");

        // Auditoría
        movimiento.CreatedAt = DateTime.UtcNow;
        movimiento.CreatedBy = usuario;
        movimiento.Id = Guid.NewGuid();

        context.MovimientosTesoreria.Add(movimiento);
        await context.SaveChangesAsync();

        // Log de auditoría
        await _auditService.LogAsync(
            "MovimientoTesoreria",
            movimiento.Id.ToString(),
            "CREATE",
            usuario,
            null,
            new
            {
                movimiento.NumeroMovimiento,
                movimiento.Fecha,
                movimiento.Tipo,
                movimiento.Valor,
                movimiento.CuentaFinancieraId,
                movimiento.Estado
            },
            $"Creado movimiento {movimiento.NumeroMovimiento} por ${movimiento.Valor:N0}");

        return movimiento;
    }

    /// <summary>
    /// Actualiza un movimiento de tesorería existente.
    /// Valida que AMBAS fechas (original y nueva) NO estén en meses cerrados.
    /// </summary>
    public async Task<MovimientoTesoreria> UpdateAsync(Guid id, MovimientoTesoreria movimientoActualizado, string usuario)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var dbMovimiento = await context.MovimientosTesoreria
            .FirstOrDefaultAsync(m => m.Id == id);

        if (dbMovimiento == null)
            throw new InvalidOperationException($"Movimiento {id} no encontrado.");

        // ✅ VALIDACIÓN CRÍTICA 1: La fecha original NO debe estar cerrada (no se puede modificar un movimiento de mes cerrado)
        await EnsureMesAbiertoAsync(dbMovimiento.Fecha);

        // ✅ VALIDACIÓN CRÍTICA 2: Si la fecha cambió, la nueva fecha tampoco debe estar cerrada
        if (movimientoActualizado.Fecha != dbMovimiento.Fecha)
        {
            await EnsureMesAbiertoAsync(movimientoActualizado.Fecha);
        }

        // Capturar valores antiguos para auditoría
        var oldValues = new
        {
            dbMovimiento.NumeroMovimiento,
            dbMovimiento.Fecha,
            dbMovimiento.Tipo,
            dbMovimiento.Valor,
            dbMovimiento.CuentaFinancieraId,
            dbMovimiento.Estado,
            dbMovimiento.Descripcion,
            dbMovimiento.Medio
        };

        // Actualizar campos
        dbMovimiento.NumeroMovimiento = movimientoActualizado.NumeroMovimiento;
        dbMovimiento.Fecha = movimientoActualizado.Fecha;
        dbMovimiento.Tipo = movimientoActualizado.Tipo;
        dbMovimiento.CuentaFinancieraId = movimientoActualizado.CuentaFinancieraId;
        dbMovimiento.FuenteIngresoId = movimientoActualizado.FuenteIngresoId;
        dbMovimiento.CategoriaEgresoId = movimientoActualizado.CategoriaEgresoId;
        dbMovimiento.Valor = movimientoActualizado.Valor;
        dbMovimiento.Descripcion = movimientoActualizado.Descripcion;
        dbMovimiento.Medio = movimientoActualizado.Medio;
        dbMovimiento.ReferenciaTransaccion = movimientoActualizado.ReferenciaTransaccion;
        dbMovimiento.TerceroId = movimientoActualizado.TerceroId;
        dbMovimiento.TerceroNombre = movimientoActualizado.TerceroNombre;
        dbMovimiento.SoporteUrl = movimientoActualizado.SoporteUrl;
        dbMovimiento.Estado = movimientoActualizado.Estado;

        // Auditoría
        dbMovimiento.UpdatedAt = DateTime.UtcNow;
        dbMovimiento.UpdatedBy = usuario;

        await context.SaveChangesAsync();

        // Log de auditoría
        await _auditService.LogAsync(
            "MovimientoTesoreria",
            dbMovimiento.Id.ToString(),
            "UPDATE",
            usuario,
            oldValues,
            new
            {
                dbMovimiento.NumeroMovimiento,
                dbMovimiento.Fecha,
                dbMovimiento.Tipo,
                dbMovimiento.Valor,
                dbMovimiento.CuentaFinancieraId,
                dbMovimiento.Estado,
                dbMovimiento.Descripcion,
                dbMovimiento.Medio
            },
            $"Actualizado movimiento {dbMovimiento.NumeroMovimiento}");

        return dbMovimiento;
    }

    /// <summary>
    /// Anula un movimiento de tesorería (cambia estado a Anulado).
    /// Valida que la fecha NO esté en un mes cerrado.
    /// </summary>
    public async Task<MovimientoTesoreria> AnularAsync(Guid id, string motivo, string usuario)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("El motivo de anulación es obligatorio.");

        await using var context = await _contextFactory.CreateDbContextAsync();

        var dbMovimiento = await context.MovimientosTesoreria
            .FirstOrDefaultAsync(m => m.Id == id);

        if (dbMovimiento == null)
            throw new InvalidOperationException($"Movimiento {id} no encontrado.");

        // ✅ VALIDACIÓN CRÍTICA: No se puede anular movimientos de meses cerrados
        await EnsureMesAbiertoAsync(dbMovimiento.Fecha);

        if (dbMovimiento.Estado == EstadoMovimientoTesoreria.Anulado)
            throw new InvalidOperationException("El movimiento ya está anulado.");

        var estadoAnterior = dbMovimiento.Estado;

        // Cambiar estado
        dbMovimiento.Estado = EstadoMovimientoTesoreria.Anulado;
        dbMovimiento.MotivoAnulacion = motivo;
        dbMovimiento.UpdatedAt = DateTime.UtcNow;
        dbMovimiento.UpdatedBy = usuario;

        await context.SaveChangesAsync();

        // Log de auditoría
        await _auditService.LogAsync(
            "MovimientoTesoreria",
            dbMovimiento.Id.ToString(),
            "ANULAR",
            usuario,
            new { Estado = estadoAnterior },
            new { Estado = EstadoMovimientoTesoreria.Anulado, Motivo = motivo },
            $"Anulado movimiento {dbMovimiento.NumeroMovimiento}. Motivo: {motivo}");

        return dbMovimiento;
    }

    /// <summary>
    /// Elimina físicamente un movimiento de tesorería.
    /// Valida que la fecha NO esté en un mes cerrado.
    /// ADVERTENCIA: Solo usar para correcciones críticas. Preferir AnularAsync.
    /// </summary>
    public async Task DeleteAsync(Guid id, string usuario)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var dbMovimiento = await context.MovimientosTesoreria
            .FirstOrDefaultAsync(m => m.Id == id);

        if (dbMovimiento == null)
            throw new InvalidOperationException($"Movimiento {id} no encontrado.");

        // ✅ VALIDACIÓN CRÍTICA: No se puede eliminar movimientos de meses cerrados
        await EnsureMesAbiertoAsync(dbMovimiento.Fecha);

        var movimientoInfo = new
        {
            dbMovimiento.NumeroMovimiento,
            dbMovimiento.Fecha,
            dbMovimiento.Tipo,
            dbMovimiento.Valor,
            dbMovimiento.Estado
        };

        context.MovimientosTesoreria.Remove(dbMovimiento);
        await context.SaveChangesAsync();

        // Log de auditoría
        await _auditService.LogAsync(
            "MovimientoTesoreria",
            dbMovimiento.Id.ToString(),
            "DELETE",
            usuario,
            movimientoInfo,
            null,
            $"⚠️ ELIMINACIÓN FÍSICA: Movimiento {dbMovimiento.NumeroMovimiento} eliminado por {usuario}");
    }

    /// <summary>
    /// Obtiene un movimiento por ID
    /// </summary>
    public async Task<MovimientoTesoreria?> GetByIdAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MovimientosTesoreria
            .Include(m => m.CuentaFinanciera)
            .Include(m => m.FuenteIngreso)
            .Include(m => m.CategoriaEgreso)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    /// <summary>
    /// Lista movimientos con filtros opcionales
    /// </summary>
    public async Task<List<MovimientoTesoreria>> ListAsync(
        DateTime? inicio = null,
        DateTime? fin = null,
        Guid? cuentaId = null,
        TipoMovimientoTesoreria? tipo = null,
        EstadoMovimientoTesoreria? estado = null,
        int maxResults = 200)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.MovimientosTesoreria.AsQueryable();

        if (inicio.HasValue) query = query.Where(m => m.Fecha >= inicio.Value);
        if (fin.HasValue) query = query.Where(m => m.Fecha <= fin.Value);
        if (cuentaId.HasValue) query = query.Where(m => m.CuentaFinancieraId == cuentaId.Value);
        if (tipo.HasValue) query = query.Where(m => m.Tipo == tipo.Value);
        if (estado.HasValue) query = query.Where(m => m.Estado == estado.Value);

        return await query
            .OrderByDescending(m => m.Fecha)
            .Take(maxResults)
            .ToListAsync();
    }
}
