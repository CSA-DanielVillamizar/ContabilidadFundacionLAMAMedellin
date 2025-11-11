using System.Text;
using Server.Data;
using Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Server.Services.Export;

/// <summary>
/// Servicio para exportar datos a formato CSV.
/// </summary>
public interface ICsvExportService
{
    Task<byte[]> ExportarMiembrosAsync();
    Task<byte[]> ExportarDeudoresAsync();
    Task<byte[]> ExportarRecibosAsync(DateTime? desde = null, DateTime? hasta = null);
    Task<byte[]> ExportarEgresosAsync(DateTime? desde = null, DateTime? hasta = null);
    Task<byte[]> ExportarCertificadosAsync(int? ano = null);
    Task<byte[]> ExportarAuditoriaAsync(List<AuditLog> logs);
}

public class CsvExportService : ICsvExportService
{
    private readonly AppDbContext _db;

    public CsvExportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<byte[]> ExportarMiembrosAsync()
    {
        var miembros = await _db.Miembros
            .OrderBy(m => m.NombreCompleto)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("NumeroSocio,Documento,NombreCompleto,Email,Celular,Cargo,Estado,FechaIngreso");

        foreach (var m in miembros)
        {
            csv.AppendLine($"{m.NumeroSocio},{EscapeCsv(m.Documento)},{EscapeCsv(m.NombreCompleto)},{EscapeCsv(m.Email)},{EscapeCsv(m.Celular)},{EscapeCsv(m.Cargo)},{m.Estado},{m.FechaIngreso:yyyy-MM-dd}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportarDeudoresAsync()
    {
        var deudores = await _db.Miembros
            .Where(m => m.Estado == EstadoMiembro.Activo)
            .Select(m => new
            {
                m.NumeroSocio,
                m.NombreCompleto,
                m.Email,
                m.Celular,
                TotalRecibos = _db.Recibos.Count(r => r.MiembroId == m.Id && r.Estado == EstadoRecibo.Emitido)
            })
            .OrderBy(x => x.NombreCompleto)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("NumeroSocio,NombreCompleto,Email,Celular,RecibosEmitidos");

        foreach (var d in deudores)
        {
            csv.AppendLine($"{d.NumeroSocio},{EscapeCsv(d.NombreCompleto)},{EscapeCsv(d.Email)},{EscapeCsv(d.Celular)},{d.TotalRecibos}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportarRecibosAsync(DateTime? desde = null, DateTime? hasta = null)
    {
        var query = _db.Recibos.AsQueryable();

        if (desde.HasValue)
            query = query.Where(r => r.FechaEmision >= desde.Value);
        if (hasta.HasValue)
            query = query.Where(r => r.FechaEmision <= hasta.Value);

        var recibos = await query
            .Include(r => r.Miembro)
            .OrderByDescending(r => r.FechaEmision)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Serie,Ano,Consecutivo,FechaEmision,Miembro,TotalCOP,Estado");

        foreach (var r in recibos)
        {
            var miembro = r.Miembro?.NombreCompleto ?? r.TerceroLibre ?? "N/A";
            csv.AppendLine($"{r.Serie},{r.Ano},{r.Consecutivo},{r.FechaEmision:yyyy-MM-dd},{EscapeCsv(miembro)},{r.TotalCop},{r.Estado}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportarEgresosAsync(DateTime? desde = null, DateTime? hasta = null)
    {
        var query = _db.Egresos.AsQueryable();

        if (desde.HasValue)
            query = query.Where(e => e.Fecha >= desde.Value);
        if (hasta.HasValue)
            query = query.Where(e => e.Fecha <= hasta.Value);

        var egresos = await query
            .OrderByDescending(e => e.Fecha)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Fecha,Categoria,Proveedor,Descripcion,ValorCOP,UsuarioRegistro");

        foreach (var e in egresos)
        {
            csv.AppendLine($"{e.Fecha:yyyy-MM-dd},{EscapeCsv(e.Categoria)},{EscapeCsv(e.Proveedor)},{EscapeCsv(e.Descripcion)},{e.ValorCop},{EscapeCsv(e.UsuarioRegistro)}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportarCertificadosAsync(int? ano = null)
    {
        var query = _db.CertificadosDonacion.AsQueryable();

        if (ano.HasValue)
            query = query.Where(c => c.Ano == ano.Value);

        var certificados = await query
            .OrderByDescending(c => c.FechaEmision)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Ano,Consecutivo,FechaEmision,Donante,Identificacion,ValorCOP,Estado");

        foreach (var c in certificados)
        {
            csv.AppendLine($"{c.Ano},{c.Consecutivo},{c.FechaEmision:yyyy-MM-dd},{EscapeCsv(c.NombreDonante)},{EscapeCsv(c.IdentificacionDonante)},{c.ValorDonacionCOP},{c.Estado}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public Task<byte[]> ExportarAuditoriaAsync(List<AuditLog> logs)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Timestamp,EntityType,EntityId,Action,UserName,IpAddress,OldValues,NewValues,AdditionalInfo");

        foreach (var log in logs)
        {
            csv.AppendLine($"{log.Timestamp:yyyy-MM-dd HH:mm:ss},{EscapeCsv(log.EntityType)},{EscapeCsv(log.EntityId)},{EscapeCsv(log.Action)},{EscapeCsv(log.UserName)},{EscapeCsv(log.IpAddress)},{EscapeCsv(log.OldValues)},{EscapeCsv(log.NewValues)},{EscapeCsv(log.AdditionalInfo)}");
        }

        return Task.FromResult(Encoding.UTF8.GetBytes(csv.ToString()));
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
