using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs.Import;
using Server.Models;

namespace Server.Services.Import;

/// <summary>
/// Servicio para previsualizar e importar Miembros desde CSV/Excel.
/// Normaliza documento y campos, marca DatosIncompletos cuando falte documento o email.
/// </summary>
public class ImportService : IImportService
{
    private readonly AppDbContext _db;

    public ImportService(AppDbContext db) => _db = db;

    private static readonly string[] CsvExt = new[] { ".csv" };
    private static readonly string[] XlsxExt = new[] { ".xlsx", ".xls" };

    public async Task<(List<ImportRowDto> rows, List<string> mensajes)> PreviewAsync(IBrowserFile file, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(file.Name).ToLowerInvariant();
        if (CsvExt.Contains(ext))
        {
            var rows = await ReadCsvAsync(file, ct);
            return (rows, new List<string> { $"Archivo CSV detectado: {file.Name}" });
        }
        if (XlsxExt.Contains(ext))
        {
            var rows = ReadXlsx(file);
            return (rows, new List<string> { $"Archivo Excel detectado: {file.Name}" });
        }
        return (new List<ImportRowDto>(), new List<string> { "Formato no soportado. Use .csv o .xlsx" });
    }

    public async Task<ImportResult> ImportAsync(IEnumerable<ImportRowDto> rows, string currentUser, CancellationToken ct = default)
    {
        var result = new ImportResult { Total = rows.Count() };

        foreach (var r in rows)
        {
            try
            {
                var documento = NormalizeDoc(r.Cedula);
                var email = (r.Email ?? string.Empty).Trim();
                var telefono = (r.Celular ?? string.Empty).Trim();
                var direccion = (r.Direccion ?? string.Empty).Trim();
                int? memberNumber = ParseNullableInt(r.MemberNumber);
                var estado = MapEstado(r.Estatus);
                var fechaIngreso = ParseDateOnly(r.FechaIngresoISO);

                Miembro? entity = null;
                if (!string.IsNullOrWhiteSpace(documento))
                    entity = await _db.Miembros.FirstOrDefaultAsync(x => x.Documento == documento, ct);
                if (entity is null && !string.IsNullOrWhiteSpace(email))
                    entity = await _db.Miembros.FirstOrDefaultAsync(x => x.Email == email, ct);

                if (entity is null)
                {
                    entity = new Miembro
                    {
                        Nombres = (r.Nombres ?? string.Empty).Trim(),
                        Apellidos = (r.Apellidos ?? string.Empty).Trim(),
                        Documento = documento,
                        Email = email,
                        Telefono = telefono,
                        Direccion = direccion,
                        MemberNumber = memberNumber,
                        Cargo = (r.Cargo ?? string.Empty).Trim(),
                        Rango = (r.Rango ?? string.Empty).Trim(),
                        Estado = estado,
                        FechaIngreso = fechaIngreso,
                        DatosIncompletos = string.IsNullOrWhiteSpace(documento) || string.IsNullOrWhiteSpace(email),
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = currentUser
                    };
                    _db.Miembros.Add(entity);
                    result.Creados++;
                }
                else
                {
                    entity.Nombres = (r.Nombres ?? entity.Nombres).Trim();
                    entity.Apellidos = (r.Apellidos ?? entity.Apellidos).Trim();
                    entity.Documento = string.IsNullOrWhiteSpace(documento) ? entity.Documento : documento;
                    entity.Email = string.IsNullOrWhiteSpace(email) ? entity.Email : email;
                    entity.Telefono = string.IsNullOrWhiteSpace(telefono) ? entity.Telefono : telefono;
                    entity.Direccion = string.IsNullOrWhiteSpace(direccion) ? entity.Direccion : direccion;
                    entity.MemberNumber = memberNumber ?? entity.MemberNumber;
                    entity.Cargo = string.IsNullOrWhiteSpace(r.Cargo) ? entity.Cargo : r.Cargo.Trim();
                    entity.Rango = string.IsNullOrWhiteSpace(r.Rango) ? entity.Rango : r.Rango.Trim();
                    entity.Estado = estado;
                    entity.FechaIngreso = fechaIngreso ?? entity.FechaIngreso;
                    entity.DatosIncompletos = string.IsNullOrWhiteSpace(entity.Documento) || string.IsNullOrWhiteSpace(entity.Email);
                    entity.UpdatedAt = DateTime.UtcNow;
                    entity.UpdatedBy = currentUser;
                    result.Actualizados++;
                }
            }
            catch (Exception ex)
            {
                result.Errores++;
                result.Mensajes.Add($"Error en fila {r.FullName} - {ex.Message}");
            }
        }

        await _db.SaveChangesAsync(ct);

        // Generar log CSV en wwwroot/data/import_logs
        try
        {
            var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var logsDir = Path.Combine(webRoot, "data", "import_logs");
            Directory.CreateDirectory(logsDir);
            var filename = $"import_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            var path = Path.Combine(logsDir, filename);
            using var sw = new StreamWriter(path);
            sw.WriteLine("Total,Creados,Actualizados,Errores");
            sw.WriteLine($"{result.Total},{result.Creados},{result.Actualizados},{result.Errores}");
            if (result.Mensajes.Any())
            {
                sw.WriteLine();
                sw.WriteLine("Mensajes:");
                foreach (var m in result.Mensajes) sw.WriteLine(m);
            }
            result.LogFilePath = $"/data/import_logs/{filename}";
        }
        catch
        {
            // no bloquear la importación por errores al escribir el log
        }

        return result;
    }

    private static string NormalizeDoc(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var s = new string(raw.Where(char.IsDigit).ToArray());
        return s.Trim();
    }

    private static int? ParseNullableInt(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        if (int.TryParse(new string(raw.Where(char.IsDigit).ToArray()), out var v)) return v;
        return null;
    }

    private static EstadoMiembro MapEstado(string? estatus)
    {
        var e = (estatus ?? string.Empty).Trim().ToLowerInvariant();
        return e == "inactivo" ? EstadoMiembro.Inactivo : EstadoMiembro.Activo;
    }

    private static DateOnly? ParseDateOnly(string? iso)
    {
        if (string.IsNullOrWhiteSpace(iso)) return null;
        if (DateOnly.TryParseExact(iso, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            return d;
        if (DateTime.TryParse(iso, out var dt))
            return DateOnly.FromDateTime(dt);
        return null;
    }

    private async Task<List<ImportRowDto>> ReadCsvAsync(IBrowserFile file, CancellationToken ct)
    {
        using var stream = file.OpenReadStream(long.MaxValue, ct);
        using var reader = new StreamReader(stream);
        var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            BadDataFound = null,
            Delimiter = ","
        };
    using var csv = new CsvReader(reader, cfg);
    var records = csv.GetRecords<ImportRowDto>().ToList();
    return await Task.FromResult(records);
    }

    private List<ImportRowDto> ReadXlsx(IBrowserFile file)
    {
        using var ms = new MemoryStream();
        file.OpenReadStream(long.MaxValue).CopyTo(ms);
        ms.Position = 0;
        using var wb = new XLWorkbook(ms);
    var ws = wb.Worksheets.First();
        var rows = new List<ImportRowDto>();
        var header = ws.Row(1);
        var headers = header.Cells().Select(c => c.GetString()).ToList();
    var lastRowUsed = ws.LastRowUsed();
    if (lastRowUsed is null) return rows;
    int lastRow = lastRowUsed.RowNumber();
        for (int r = 2; r <= lastRow; r++)
        {
            var obj = new ImportRowDto
            {
                FullName = ws.Cell(r, headers.IndexOf("FullName") + 1).GetString(),
                Nombres = ws.Cell(r, headers.IndexOf("Nombres") + 1).GetString(),
                Apellidos = ws.Cell(r, headers.IndexOf("Apellidos") + 1).GetString(),
                Cedula = ws.Cell(r, headers.IndexOf("Cedula") + 1).GetString(),
                Email = ws.Cell(r, headers.IndexOf("Email") + 1).GetString(),
                Celular = ws.Cell(r, headers.IndexOf("Celular") + 1).GetString(),
                Direccion = ws.Cell(r, headers.IndexOf("Direccion") + 1).GetString(),
                MemberNumber = ws.Cell(r, headers.IndexOf("MemberNumber") + 1).GetString(),
                Cargo = ws.Cell(r, headers.IndexOf("Cargo") + 1).GetString(),
                Rango = ws.Cell(r, headers.IndexOf("Rango") + 1).GetString(),
                Estatus = ws.Cell(r, headers.IndexOf("Estatus") + 1).GetString(),
                FechaIngresoISO = ws.Cell(r, headers.IndexOf("FechaIngresoISO") + 1).GetString(),
            };
            rows.Add(obj);
        }
        return rows;
    }
}
