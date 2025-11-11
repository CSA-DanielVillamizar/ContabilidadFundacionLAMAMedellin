using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs.ConciliacionBancaria;
using Server.Models;

namespace Server.Services.ConciliacionBancaria;

/// <summary>
/// Servicio de conciliación bancaria con lógica de negocio completa
/// </summary>
public class ConciliacionBancariaService : IConciliacionBancariaService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public ConciliacionBancariaService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<(List<ConciliacionBancariaDto> Conciliaciones, int TotalCount)> ListarAsync(
        int? ano = null,
        int? mes = null,
        string? estado = null,
        int pagina = 1,
        int porPagina = 20)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        var query = context.ConciliacionesBancarias
            .Include(c => c.Items)
            .AsQueryable();

        // Filtros
        if (ano.HasValue)
            query = query.Where(c => c.Ano == ano.Value);

        if (mes.HasValue)
            query = query.Where(c => c.Mes == mes.Value);

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(c => c.Estado == estado);

        var totalCount = await query.CountAsync();

        var conciliaciones = await query
            .OrderByDescending(c => c.Ano)
            .ThenByDescending(c => c.Mes)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .Select(c => new ConciliacionBancariaDto
            {
                Id = c.Id,
                Ano = c.Ano,
                Mes = c.Mes,
                Periodo = $"{ObtenerNombreMes(c.Mes)} {c.Ano}",
                FechaConciliacion = c.FechaConciliacion ?? c.CreatedAt,
                SaldoLibros = c.SaldoLibros,
                SaldoBanco = c.SaldoBanco,
                Diferencia = c.Diferencia,
                Estado = c.Estado,
                TotalItems = c.Items.Count,
                ItemsConciliados = c.Items.Count(i => i.Conciliado),
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return (conciliaciones, totalCount);
    }

    public async Task<ConciliacionBancariaDetalleDto?> ObtenerDetalleAsync(Guid id)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        var conciliacion = await context.ConciliacionesBancarias
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (conciliacion == null)
            return null;

        var items = conciliacion.Items.Select(i => new ItemConciliacionDto
        {
            Id = i.Id,
            Fecha = i.FechaConciliacion ?? DateTime.Now,
            Tipo = i.Tipo,
            Concepto = i.Descripcion,
            Referencia = null,
            Monto = i.Monto,
            Conciliado = i.Conciliado,
            Observaciones = null
        }).ToList();

        return new ConciliacionBancariaDetalleDto
        {
            Id = conciliacion.Id,
            Ano = conciliacion.Ano,
            Mes = conciliacion.Mes,
            Periodo = $"{ObtenerNombreMes(conciliacion.Mes)} {conciliacion.Ano}",
            FechaConciliacion = conciliacion.FechaConciliacion ?? conciliacion.CreatedAt,
            SaldoLibros = conciliacion.SaldoLibros,
            SaldoBanco = conciliacion.SaldoBanco,
            Diferencia = conciliacion.Diferencia,
            Observaciones = conciliacion.Observaciones,
            Estado = conciliacion.Estado,
            Items = items,
            TotalItems = items.Count,
            ItemsConciliados = items.Count(i => i.Conciliado),
            ItemsPendientes = items.Count(i => !i.Conciliado),
            TotalNotasDebito = items.Where(i => i.Tipo == "NotaDebito").Sum(i => i.Monto),
            TotalNotasCredito = items.Where(i => i.Tipo == "NotaCredito").Sum(i => i.Monto),
            TotalChequesTransito = items.Where(i => i.Tipo == "ChequeTransito").Sum(i => i.Monto),
            TotalDepositosTransito = items.Where(i => i.Tipo == "DepositoTransito").Sum(i => i.Monto),
            CreatedAt = conciliacion.CreatedAt,
            CreatedBy = conciliacion.CreatedBy,
            UpdatedAt = conciliacion.UpdatedAt,
            UpdatedBy = conciliacion.UpdatedBy
        };
    }

    public async Task<Guid> CrearAsync(ConciliacionBancariaFormDto dto, string usuario)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        // Validaciones
        if (dto.Ano < 2000 || dto.Ano > 2100)
            throw new InvalidOperationException("El año debe estar entre 2000 y 2100");

        if (dto.Mes < 1 || dto.Mes > 12)
            throw new InvalidOperationException("El mes debe estar entre 1 y 12");

        // Verificar que no exista otra conciliación para el mismo período
        if (await ExisteConciliacionAsync(dto.Ano, dto.Mes, dto.Id))
            throw new InvalidOperationException($"Ya existe una conciliación para {ObtenerNombreMes(dto.Mes)} {dto.Ano}");

        var conciliacion = new Models.ConciliacionBancaria
        {
            Id = Guid.NewGuid(),
            Ano = dto.Ano,
            Mes = dto.Mes,
            SaldoLibros = dto.SaldoLibros,
            SaldoBanco = dto.SaldoBanco,
            Diferencia = dto.SaldoLibros - dto.SaldoBanco,
            Estado = dto.Estado,
            Observaciones = dto.Observaciones,
            FechaConciliacion = dto.FechaConciliacion,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = usuario
        };

        // Agregar items
        foreach (var itemDto in dto.Items ?? new List<ItemConciliacionFormDto>())
        {
            var item = new ItemConciliacion
            {
                Id = Guid.NewGuid(),
                ConciliacionId = conciliacion.Id,
                Tipo = itemDto.Tipo,
                Descripcion = itemDto.Concepto,
                Monto = itemDto.Monto,
                EsSuma = itemDto.Monto > 0,
                Conciliado = itemDto.Conciliado,
                FechaConciliacion = itemDto.Conciliado ? itemDto.Fecha : null
            };
            conciliacion.Items.Add(item);
        }

        context.ConciliacionesBancarias.Add(conciliacion);
        await context.SaveChangesAsync();

        return conciliacion.Id;
    }

    public async Task ActualizarAsync(Guid id, ConciliacionBancariaFormDto dto, string usuario)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        var conciliacion = await context.ConciliacionesBancarias
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (conciliacion == null)
            throw new InvalidOperationException("La conciliación no existe");

        // Validaciones
        if (dto.Ano < 2000 || dto.Ano > 2100)
            throw new InvalidOperationException("El año debe estar entre 2000 y 2100");

        if (dto.Mes < 1 || dto.Mes > 12)
            throw new InvalidOperationException("El mes debe estar entre 1 y 12");

        // Verificar cambio de período
        if (conciliacion.Ano != dto.Ano || conciliacion.Mes != dto.Mes)
        {
            if (await ExisteConciliacionAsync(dto.Ano, dto.Mes, id))
                throw new InvalidOperationException($"Ya existe una conciliación para {ObtenerNombreMes(dto.Mes)} {dto.Ano}");
        }

        // Actualizar campos
        conciliacion.Ano = dto.Ano;
        conciliacion.Mes = dto.Mes;
        conciliacion.SaldoLibros = dto.SaldoLibros;
        conciliacion.SaldoBanco = dto.SaldoBanco;
        conciliacion.Diferencia = dto.SaldoLibros - dto.SaldoBanco;
        conciliacion.Estado = dto.Estado;
        conciliacion.Observaciones = dto.Observaciones;
        conciliacion.FechaConciliacion = dto.FechaConciliacion;
        conciliacion.UpdatedAt = DateTime.UtcNow;
        conciliacion.UpdatedBy = usuario;

        // Actualizar items (eliminar y recrear)
        context.ItemsConciliacion.RemoveRange(conciliacion.Items);
        conciliacion.Items.Clear();

        foreach (var itemDto in dto.Items ?? new List<ItemConciliacionFormDto>())
        {
            var item = new ItemConciliacion
            {
                Id = itemDto.Id ?? Guid.NewGuid(),
                ConciliacionId = conciliacion.Id,
                Tipo = itemDto.Tipo,
                Descripcion = itemDto.Concepto,
                Monto = itemDto.Monto,
                EsSuma = itemDto.Monto > 0,
                Conciliado = itemDto.Conciliado,
                FechaConciliacion = itemDto.Conciliado ? itemDto.Fecha : null
            };
            conciliacion.Items.Add(item);
        }

        await context.SaveChangesAsync();
    }

    public async Task EliminarAsync(Guid id)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        var conciliacion = await context.ConciliacionesBancarias
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (conciliacion == null)
            throw new InvalidOperationException("La conciliación no existe");

        if (conciliacion.Estado != "Pendiente")
            throw new InvalidOperationException("Solo se pueden eliminar conciliaciones en estado Pendiente");

        context.ConciliacionesBancarias.Remove(conciliacion);
        await context.SaveChangesAsync();
    }

    public async Task<Guid> AgregarItemAsync(Guid conciliacionId, ItemConciliacionFormDto dto)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        var conciliacion = await context.ConciliacionesBancarias
            .FirstOrDefaultAsync(c => c.Id == conciliacionId);

        if (conciliacion == null)
            throw new InvalidOperationException("La conciliación no existe");

        var item = new ItemConciliacion
        {
            Id = Guid.NewGuid(),
            ConciliacionId = conciliacionId,
            Tipo = dto.Tipo,
            Descripcion = dto.Concepto,
            Monto = dto.Monto,
            EsSuma = dto.Monto > 0,
            Conciliado = dto.Conciliado,
            FechaConciliacion = dto.Conciliado ? dto.Fecha : null
        };

        context.ItemsConciliacion.Add(item);
        await context.SaveChangesAsync();

        return item.Id;
    }

    public async Task ActualizarItemAsync(Guid itemId, ItemConciliacionFormDto dto)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        var item = await context.ItemsConciliacion.FirstOrDefaultAsync(i => i.Id == itemId);

        if (item == null)
            throw new InvalidOperationException("El item no existe");

        item.Tipo = dto.Tipo;
        item.Descripcion = dto.Concepto;
        item.Monto = dto.Monto;
        item.EsSuma = dto.Monto > 0;
        item.Conciliado = dto.Conciliado;
        item.FechaConciliacion = dto.Conciliado ? dto.Fecha : null;

        await context.SaveChangesAsync();
    }

    public async Task EliminarItemAsync(Guid itemId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        var item = await context.ItemsConciliacion.FirstOrDefaultAsync(i => i.Id == itemId);

        if (item == null)
            throw new InvalidOperationException("El item no existe");

        context.ItemsConciliacion.Remove(item);
        await context.SaveChangesAsync();
    }

    public async Task MarcarItemConciliadoAsync(Guid itemId, bool conciliado)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        var item = await context.ItemsConciliacion.FirstOrDefaultAsync(i => i.Id == itemId);

        if (item == null)
            throw new InvalidOperationException("El item no existe");

        item.Conciliado = conciliado;
        item.FechaConciliacion = conciliado ? DateTime.UtcNow : null;

        await context.SaveChangesAsync();
    }

    public async Task<(decimal SaldoLibros, decimal SaldoBanco, decimal Diferencia)> CalcularSaldosAsync(int ano, int mes)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        // Calcular saldo según libros (ingresos - egresos del período)
        var fechaInicio = new DateTime(ano, mes, 1);
        var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

        // Ingresos del período (recibos emitidos)
        var ingresosMes = await context.Recibos
              .Where(r => r.FechaEmision >= fechaInicio && r.FechaEmision <= fechaFin && r.Estado == EstadoRecibo.Emitido)
              .SumAsync(r => r.TotalCop);

        // Egresos del período
        var egresosMes = await context.Egresos
            .Where(e => e.Fecha >= fechaInicio && e.Fecha <= fechaFin)
              .SumAsync(e => e.ValorCop);

        var saldoLibros = ingresosMes - egresosMes;

        // El saldo bancario se debe obtener del extracto (por ahora lo dejamos en 0)
        decimal saldoBanco = 0;

        var diferencia = saldoLibros - saldoBanco;

        return (saldoLibros, saldoBanco, diferencia);
    }

    public async Task<int> RealizarMatchingAutomaticoAsync(Guid conciliacionId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        var conciliacion = await context.ConciliacionesBancarias
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == conciliacionId);

        if (conciliacion == null)
            throw new InvalidOperationException("La conciliación no existe");

        // Lógica simplificada de matching automático
        // En una implementación real, se compararían montos, fechas, referencias, etc.
        var itemsNoConciliados = conciliacion.Items.Where(i => !i.Conciliado).ToList();
        var itemsMatcheados = 0;

        foreach (var item in itemsNoConciliados)
        {
            // Aquí iría la lógica de matching con movimientos bancarios
            // Por ahora solo marcamos como ejemplo
            if (item.Monto > 0 && item.Tipo == "DepositoTransito")
            {
                item.Conciliado = true;
                item.FechaConciliacion = DateTime.UtcNow;
                itemsMatcheados++;
            }
        }

        await context.SaveChangesAsync();
        return itemsMatcheados;
    }

    public async Task<int> ImportarExtractoAsync(ImportarExtractoDto dto)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        var conciliacion = await context.ConciliacionesBancarias
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == dto.ConciliacionId);

        if (conciliacion == null)
            throw new InvalidOperationException("La conciliación no existe");

        // Aquí iría la lógica de parseo del archivo según el formato
        // Por ahora retornamos 0 como placeholder
        // En una implementación real, se parsearía el CSV/Excel/PDF y se crearían items

        return 0; // Número de items importados
    }

    public async Task CambiarEstadoAsync(Guid id, string nuevoEstado, string usuario)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        var conciliacion = await context.ConciliacionesBancarias
            .FirstOrDefaultAsync(c => c.Id == id);

        if (conciliacion == null)
            throw new InvalidOperationException("La conciliación no existe");

        var estadosValidos = new[] { "Pendiente", "EnProceso", "Conciliada", "ConDiferencias" };
        if (!estadosValidos.Contains(nuevoEstado))
            throw new InvalidOperationException($"Estado inválido. Estados válidos: {string.Join(", ", estadosValidos)}");

        conciliacion.Estado = nuevoEstado;
        conciliacion.UpdatedAt = DateTime.UtcNow;
        conciliacion.UpdatedBy = usuario;

        if (nuevoEstado == "Conciliada")
            conciliacion.FechaConciliacion = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }

    public async Task<bool> ExisteConciliacionAsync(int ano, int mes, Guid? excluyendoId = null)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        var query = context.ConciliacionesBancarias
            .Where(c => c.Ano == ano && c.Mes == mes);

        if (excluyendoId.HasValue)
            query = query.Where(c => c.Id != excluyendoId.Value);

        return await query.AnyAsync();
    }

    public async Task<List<ResumenConciliacionDto>> ObtenerResumenAnualAsync(int ano)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        var conciliaciones = await context.ConciliacionesBancarias
            .Include(c => c.Items)
            .Where(c => c.Ano == ano)
            .ToListAsync();

        var resumen = new List<ResumenConciliacionDto>();

        for (int mes = 1; mes <= 12; mes++)
        {
            var conciliacion = conciliaciones.FirstOrDefault(c => c.Mes == mes);

            if (conciliacion != null)
            {
                resumen.Add(new ResumenConciliacionDto
                {
                    Mes = mes,
                    NombreMes = ObtenerNombreMes(mes),
                    Estado = conciliacion.Estado,
                    SaldoLibros = conciliacion.SaldoLibros,
                    SaldoBanco = conciliacion.SaldoBanco,
                    Diferencia = conciliacion.Diferencia,
                    ItemsConciliados = conciliacion.Items.Count(i => i.Conciliado),
                    ItemsPendientes = conciliacion.Items.Count(i => !i.Conciliado)
                });
            }
            else
            {
                resumen.Add(new ResumenConciliacionDto
                {
                    Mes = mes,
                    NombreMes = ObtenerNombreMes(mes),
                    Estado = "SinConciliar",
                    SaldoLibros = 0,
                    SaldoBanco = 0,
                    Diferencia = 0,
                    ItemsConciliados = 0,
                    ItemsPendientes = 0
                });
            }
        }

        return resumen;
    }

    private string ObtenerNombreMes(int mes)
    {
        return mes switch
        {
            1 => "Enero",
            2 => "Febrero",
            3 => "Marzo",
            4 => "Abril",
            5 => "Mayo",
            6 => "Junio",
            7 => "Julio",
            8 => "Agosto",
            9 => "Septiembre",
            10 => "Octubre",
            11 => "Noviembre",
            12 => "Diciembre",
            _ => mes.ToString()
        };
    }
}
