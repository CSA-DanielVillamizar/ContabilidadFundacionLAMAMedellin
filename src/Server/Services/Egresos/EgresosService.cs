using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using Server.Services.Audit;
using Server.Services.CierreContable;

namespace Server.Services.Egresos;

/// <summary>
/// Implementación de IEgresosService. Maneja persistencia en DB y archivos de soporte en wwwroot/data/egresos.
/// </summary>
public class EgresosService : IEgresosService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly string _root;
    private readonly CierreContableService _cierreService;
    private readonly IAuditService _auditService;

    public EgresosService(IDbContextFactory<AppDbContext> dbFactory, IWebHostEnvironment env, CierreContableService cierreService, IAuditService auditService)
    {
        _dbFactory = dbFactory;
        _root = Path.Combine(env.WebRootPath ?? "wwwroot", "data", "egresos");
        Directory.CreateDirectory(_root);
        _cierreService = cierreService;
        _auditService = auditService;
    }

    public async Task<Egreso> CrearAsync(Egreso egreso, IFormFile? soporte, string usuarioActual, CancellationToken ct = default)
    {
        if (egreso.ValorCop <= 0) throw new ArgumentException("El valor del egreso debe ser mayor a 0", nameof(egreso.ValorCop));
        
        // Validar que el mes del egreso no esté cerrado
        var esCerrado = await _cierreService.EsFechaCerradaAsync(egreso.Fecha);
        if (esCerrado)
            throw new InvalidOperationException($"No se puede registrar el egreso porque el mes {egreso.Fecha:MM/yyyy} está cerrado");

        egreso.Id = Guid.NewGuid();
        egreso.UsuarioRegistro = usuarioActual;
        egreso.CreatedAt = DateTime.UtcNow;
        egreso.CreatedBy = usuarioActual;

        if (soporte is not null && soporte.Length > 0)
        {
            var fileName = $"{egreso.Id}{Path.GetExtension(soporte.FileName)}";
            var dest = Path.Combine(_root, fileName);
            using var fs = File.Create(dest);
            await soporte.CopyToAsync(fs, ct);
            egreso.SoporteUrl = $"/data/egresos/{fileName}";
        }

        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        db.Egresos.Add(egreso);
        await db.SaveChangesAsync(ct);

        // ✅ AUDITORÍA: Registrar creación de egreso
        await _auditService.LogAsync(
            entityType: "Egreso",
            entityId: egreso.Id.ToString(),
            action: "EGRESO_CREADO",
            userName: usuarioActual,
            newValues: new
            {
                Fecha = egreso.Fecha.ToString("yyyy-MM-dd"),
                Categoria = egreso.Categoria,
                Proveedor = egreso.Proveedor,
                Descripcion = egreso.Descripcion,
                ValorCop = egreso.ValorCop,
                TieneSoporte = !string.IsNullOrEmpty(egreso.SoporteUrl)
            },
            additionalInfo: $"Egreso creado: {egreso.Descripcion} - ${egreso.ValorCop:N0} COP (Categoría: {egreso.Categoria}, Proveedor: {egreso.Proveedor})"
        );

        return egreso;
    }

    public async Task<Egreso> CrearAsync(Egreso egreso, Stream? soporteStream, string? soporteFileName, string usuarioActual, CancellationToken ct = default)
    {
        if (egreso.ValorCop <= 0) throw new ArgumentException("El valor del egreso debe ser mayor a 0", nameof(egreso.ValorCop));
        
        // Validar que el mes del egreso no esté cerrado
        var esCerrado = await _cierreService.EsFechaCerradaAsync(egreso.Fecha);
        if (esCerrado)
            throw new InvalidOperationException($"No se puede registrar el egreso porque el mes {egreso.Fecha:MM/yyyy} está cerrado");

        egreso.Id = Guid.NewGuid();
        egreso.UsuarioRegistro = usuarioActual;
        egreso.CreatedAt = DateTime.UtcNow;
        egreso.CreatedBy = usuarioActual;

        if (soporteStream is not null)
        {
            var ext = string.IsNullOrWhiteSpace(soporteFileName) ? ".bin" : Path.GetExtension(soporteFileName);
            var fileName = $"{egreso.Id}{ext}";
            var dest = Path.Combine(_root, fileName);
            using var fs = File.Create(dest);
            await soporteStream.CopyToAsync(fs, ct);
            egreso.SoporteUrl = $"/data/egresos/{fileName}";
        }

        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        db.Egresos.Add(egreso);
        await db.SaveChangesAsync(ct);

        // ✅ AUDITORÍA: Registrar creación de egreso (sobrecarga Stream)
        await _auditService.LogAsync(
            entityType: "Egreso",
            entityId: egreso.Id.ToString(),
            action: "EGRESO_CREADO",
            userName: usuarioActual,
            newValues: new
            {
                Fecha = egreso.Fecha.ToString("yyyy-MM-dd"),
                Categoria = egreso.Categoria,
                Proveedor = egreso.Proveedor,
                Descripcion = egreso.Descripcion,
                ValorCop = egreso.ValorCop,
                TieneSoporte = !string.IsNullOrEmpty(egreso.SoporteUrl)
            },
            additionalInfo: $"Egreso creado: {egreso.Descripcion} - ${egreso.ValorCop:N0} COP (Categoría: {egreso.Categoria}, Proveedor: {egreso.Proveedor})"
        );

        return egreso;
    }

    public async Task<Egreso?> ObtenerAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Egresos.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<List<Egreso>> ListarAsync(DateTime? desde, DateTime? hasta, string? categoria, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var q = db.Egresos.AsNoTracking().AsQueryable();
        if (desde.HasValue) q = q.Where(e => e.Fecha >= desde.Value);
        if (hasta.HasValue) q = q.Where(e => e.Fecha <= hasta.Value);
        if (!string.IsNullOrWhiteSpace(categoria)) q = q.Where(e => e.Categoria == categoria);
        return await q.OrderByDescending(e => e.Fecha).ThenByDescending(e => e.CreatedAt).ToListAsync(ct);
    }

    public async Task<Egreso?> ActualizarAsync(Guid id, Egreso egreso, IFormFile? soporte, string usuarioActual, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var dbEgreso = await db.Egresos.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (dbEgreso is null) return null;

        // ✅ VALIDACIÓN CRÍTICA: Verificar que el mes NO está cerrado (ni el antiguo ni el nuevo)
        var esCerradoAnterior = await _cierreService.EsFechaCerradaAsync(dbEgreso.Fecha);
        if (esCerradoAnterior)
            throw new InvalidOperationException($"No se puede actualizar el egreso porque el mes original {dbEgreso.Fecha:MM/yyyy} está cerrado");

        // Si la fecha cambió, validar que el nuevo mes tampoco esté cerrado
        if (egreso.Fecha != dbEgreso.Fecha)
        {
            var esCerradoNuevo = await _cierreService.EsFechaCerradaAsync(egreso.Fecha);
            if (esCerradoNuevo)
                throw new InvalidOperationException($"No se puede mover el egreso al mes {egreso.Fecha:MM/yyyy} porque ese mes está cerrado");
        }

        // Capturar valores anteriores para auditoría
        var valoresAnteriores = $"Fecha: {dbEgreso.Fecha:yyyy-MM-dd}, Categoría: {dbEgreso.Categoria}, Proveedor: {dbEgreso.Proveedor}, Valor: {dbEgreso.ValorCop:N0}";

        dbEgreso.Fecha = egreso.Fecha;
        dbEgreso.Categoria = egreso.Categoria;
        dbEgreso.Proveedor = egreso.Proveedor;
        dbEgreso.Descripcion = egreso.Descripcion;
        dbEgreso.ValorCop = egreso.ValorCop;
        dbEgreso.UsuarioRegistro = usuarioActual;

        if (soporte is not null && soporte.Length > 0)
        {
            // Borrar anterior si existe
            if (!string.IsNullOrEmpty(dbEgreso.SoporteUrl))
            {
                var prev = Path.Combine(_root, Path.GetFileName(dbEgreso.SoporteUrl));
                if (File.Exists(prev)) File.Delete(prev);
            }
            var fileName = $"{dbEgreso.Id}{Path.GetExtension(soporte.FileName)}";
            var dest = Path.Combine(_root, fileName);
            using var fs = File.Create(dest);
            await soporte.CopyToAsync(fs, ct);
            dbEgreso.SoporteUrl = $"/data/egresos/{fileName}";
        }

        await db.SaveChangesAsync(ct);

        // ✅ AUDITORÍA: Registrar actualización de egreso
        await _auditService.LogAsync(
            entityType: "Egreso",
            entityId: dbEgreso.Id.ToString(),
            action: "EGRESO_ACTUALIZADO",
            userName: usuarioActual,
            oldValues: new { Valores = valoresAnteriores },
            newValues: new
            {
                Fecha = dbEgreso.Fecha.ToString("yyyy-MM-dd"),
                Categoria = dbEgreso.Categoria,
                Proveedor = dbEgreso.Proveedor,
                ValorCop = dbEgreso.ValorCop
            },
            additionalInfo: $"Egreso actualizado: {dbEgreso.Descripcion} - ${dbEgreso.ValorCop:N0} COP"
        );

        return dbEgreso;
    }

    public async Task<Egreso?> ActualizarAsync(Guid id, Egreso egreso, Stream? soporteStream, string? soporteFileName, string usuarioActual, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var dbEgreso = await db.Egresos.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (dbEgreso is null) return null;

        // ✅ VALIDACIÓN CRÍTICA: Verificar que el mes NO está cerrado (ni el antiguo ni el nuevo)
        var esCerradoAnterior = await _cierreService.EsFechaCerradaAsync(dbEgreso.Fecha);
        if (esCerradoAnterior)
            throw new InvalidOperationException($"No se puede actualizar el egreso porque el mes original {dbEgreso.Fecha:MM/yyyy} está cerrado");

        // Si la fecha cambió, validar que el nuevo mes tampoco esté cerrado
        if (egreso.Fecha != dbEgreso.Fecha)
        {
            var esCerradoNuevo = await _cierreService.EsFechaCerradaAsync(egreso.Fecha);
            if (esCerradoNuevo)
                throw new InvalidOperationException($"No se puede mover el egreso al mes {egreso.Fecha:MM/yyyy} porque ese mes está cerrado");
        }

        // Capturar valores anteriores para auditoría
        var valoresAnteriores = $"Fecha: {dbEgreso.Fecha:yyyy-MM-dd}, Categoría: {dbEgreso.Categoria}, Proveedor: {dbEgreso.Proveedor}, Valor: {dbEgreso.ValorCop:N0}";

        dbEgreso.Fecha = egreso.Fecha;
        dbEgreso.Categoria = egreso.Categoria;
        dbEgreso.Proveedor = egreso.Proveedor;
        dbEgreso.Descripcion = egreso.Descripcion;
        dbEgreso.ValorCop = egreso.ValorCop;
        dbEgreso.UsuarioRegistro = usuarioActual;

        if (soporteStream is not null)
        {
            if (!string.IsNullOrEmpty(dbEgreso.SoporteUrl))
            {
                var prev = Path.Combine(_root, Path.GetFileName(dbEgreso.SoporteUrl));
                if (File.Exists(prev)) File.Delete(prev);
            }
            var ext = string.IsNullOrWhiteSpace(soporteFileName) ? ".bin" : Path.GetExtension(soporteFileName);
            var fileName = $"{dbEgreso.Id}{ext}";
            var dest = Path.Combine(_root, fileName);
            using var fs = File.Create(dest);
            await soporteStream.CopyToAsync(fs, ct);
            dbEgreso.SoporteUrl = $"/data/egresos/{fileName}";
        }

        await db.SaveChangesAsync(ct);

        // ✅ AUDITORÍA: Registrar actualización de egreso (sobrecarga Stream)
        await _auditService.LogAsync(
            entityType: "Egreso",
            entityId: dbEgreso.Id.ToString(),
            action: "EGRESO_ACTUALIZADO",
            userName: usuarioActual,
            oldValues: new { Valores = valoresAnteriores },
            newValues: new
            {
                Fecha = dbEgreso.Fecha.ToString("yyyy-MM-dd"),
                Categoria = dbEgreso.Categoria,
                Proveedor = dbEgreso.Proveedor,
                ValorCop = dbEgreso.ValorCop
            },
            additionalInfo: $"Egreso actualizado: {dbEgreso.Descripcion} - ${dbEgreso.ValorCop:N0} COP"
        );

        return dbEgreso;
    }

    public async Task<bool> EliminarAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var dbEgreso = await db.Egresos.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (dbEgreso is null) return false;

        // ✅ VALIDACIÓN CRÍTICA: Verificar que el mes NO está cerrado
        var esCerrado = await _cierreService.EsFechaCerradaAsync(dbEgreso.Fecha);
        if (esCerrado)
            throw new InvalidOperationException($"No se puede eliminar el egreso porque el mes {dbEgreso.Fecha:MM/yyyy} está cerrado");

        // Capturar información antes de eliminar
        var descripcion = dbEgreso.Descripcion;
        var categoria = dbEgreso.Categoria;
        var valor = dbEgreso.ValorCop;
        var fecha = dbEgreso.Fecha;

        if (!string.IsNullOrEmpty(dbEgreso.SoporteUrl))
        {
            var prev = Path.Combine(_root, Path.GetFileName(dbEgreso.SoporteUrl));
            if (File.Exists(prev)) File.Delete(prev);
        }

        db.Egresos.Remove(dbEgreso);
        await db.SaveChangesAsync(ct);

        // ✅ AUDITORÍA: Registrar eliminación de egreso
        await _auditService.LogAsync(
            entityType: "Egreso",
            entityId: id.ToString(),
            action: "EGRESO_ELIMINADO",
            userName: "system", // No tenemos usuarioActual en este método
            oldValues: new
            {
                Descripcion = descripcion,
                Categoria = categoria,
                ValorCop = valor,
                Fecha = fecha.ToString("yyyy-MM-dd")
            },
            additionalInfo: $"Egreso eliminado: {descripcion} - ${valor:N0} COP (Categoría: {categoria}, Fecha: {fecha:yyyy-MM-dd})"
        );

        return true;
    }
}
