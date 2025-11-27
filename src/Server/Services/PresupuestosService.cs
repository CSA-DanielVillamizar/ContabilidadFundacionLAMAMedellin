using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs;
using Server.Models;

namespace Server.Services;

/// <summary>
/// Implementación del servicio de presupuestos con cálculo de ejecución desde movimientos financieros.
/// Sigue Clean Architecture: lógica de negocio separada, validaciones, cálculos automáticos.
/// </summary>
public class PresupuestosService : IPresupuestosService
{
    private readonly AppDbContext _context;

    public PresupuestosService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lista presupuestos con filtros y cálculo de ejecución en tiempo real
    /// </summary>
    public async Task<(List<PresupuestoDto> presupuestos, int totalCount)> ListarAsync(
        int? ano = null,
        int? mes = null,
        int? conceptoId = null,
        int pagina = 1,
        int porPagina = 50)
    {
        var query = _context.Presupuestos
                .AsNoTracking()
            .Include(p => p.Concepto)
            .AsQueryable();

        // Filtros
        if (ano.HasValue)
            query = query.Where(p => p.Ano == ano.Value);
        
        if (mes.HasValue)
            query = query.Where(p => p.Mes == mes.Value);
        
        if (conceptoId.HasValue)
            query = query.Where(p => p.ConceptoId == conceptoId.Value);

        var totalCount = await query.CountAsync();

        var presupuestos = await query
            .OrderByDescending(p => p.Ano)
            .ThenByDescending(p => p.Mes)
            .ThenBy(p => p.Concepto.Nombre)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .Select(p => new PresupuestoDto
            {
                Id = p.Id,
                Ano = p.Ano,
                Mes = p.Mes,
                ConceptoId = p.ConceptoId,
                ConceptoNombre = p.Concepto.Nombre,
                MontoPresupuestado = p.MontoPresupuestado,
                MontoEjecutado = 0, // Se calcula después
                PorcentajeEjecucion = 0,
                Diferencia = 0,
                Notas = p.Notas
            })
            .ToListAsync();

        // Calcular ejecución para cada presupuesto
        foreach (var presupuesto in presupuestos)
        {
            presupuesto.MontoEjecutado = await CalcularEjecucionPorConceptoYPeriodoAsync(
                presupuesto.ConceptoId, 
                presupuesto.Ano, 
                presupuesto.Mes);
            
            presupuesto.Diferencia = presupuesto.MontoPresupuestado - presupuesto.MontoEjecutado;
            
            presupuesto.PorcentajeEjecucion = presupuesto.MontoPresupuestado > 0
                ? Math.Round((presupuesto.MontoEjecutado / presupuesto.MontoPresupuestado) * 100, 2)
                : 0;
        }

        return (presupuestos, totalCount);
    }

    /// <summary>
    /// Obtiene el detalle completo de un presupuesto con auditoría y ejecución
    /// </summary>
    public async Task<PresupuestoDetalleDto?> ObtenerDetalleAsync(Guid id)
    {
        var presupuesto = await _context.Presupuestos
            .Include(p => p.Concepto)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (presupuesto == null)
            return null;

        var montoEjecutado = await CalcularEjecucionPorConceptoYPeriodoAsync(
            presupuesto.ConceptoId, 
            presupuesto.Ano, 
            presupuesto.Mes);

        var diferencia = presupuesto.MontoPresupuestado - montoEjecutado;
        var porcentaje = presupuesto.MontoPresupuestado > 0
            ? Math.Round((montoEjecutado / presupuesto.MontoPresupuestado) * 100, 2)
            : 0;

        return new PresupuestoDetalleDto
        {
            Id = presupuesto.Id,
            Ano = presupuesto.Ano,
            Mes = presupuesto.Mes,
            ConceptoId = presupuesto.ConceptoId,
            ConceptoNombre = presupuesto.Concepto.Nombre,
            MontoPresupuestado = presupuesto.MontoPresupuestado,
            MontoEjecutado = montoEjecutado,
            Diferencia = diferencia,
            PorcentajeEjecucion = porcentaje,
            Notas = presupuesto.Notas,
            CreatedAt = presupuesto.CreatedAt,
            CreatedBy = presupuesto.CreatedBy,
            UpdatedAt = presupuesto.UpdatedAt,
            UpdatedBy = presupuesto.UpdatedBy
        };
    }

    /// <summary>
    /// Obtiene el consolidado anual: suma todos los presupuestos por concepto
    /// </summary>
    public async Task<List<PresupuestoConsolidadoDto>> ObtenerConsolidadoAnualAsync(int ano)
    {
        var presupuestos = await _context.Presupuestos
            .Include(p => p.Concepto)
            .Where(p => p.Ano == ano)
            .ToListAsync();

        var consolidado = presupuestos
            .GroupBy(p => new { p.ConceptoId, p.Concepto.Nombre })
            .Select(g => new PresupuestoConsolidadoDto
            {
                ConceptoId = g.Key.ConceptoId,
                ConceptoNombre = g.Key.Nombre,
                Ano = ano,
                MontoPresupuestadoTotal = g.Sum(p => p.MontoPresupuestado),
                MontoEjecutadoTotal = 0, // Se calcula después
                DiferenciaTotal = 0,
                PorcentajeEjecucionPromedio = 0,
                CantidadMeses = g.Count()
            })
            .ToList();

        // Calcular ejecución total por concepto en el año
        foreach (var item in consolidado)
        {
            item.MontoEjecutadoTotal = await CalcularEjecucionAnualPorConceptoAsync(item.ConceptoId, ano);
            item.DiferenciaTotal = item.MontoPresupuestadoTotal - item.MontoEjecutadoTotal;
            item.PorcentajeEjecucionPromedio = item.MontoPresupuestadoTotal > 0
                ? Math.Round((item.MontoEjecutadoTotal / item.MontoPresupuestadoTotal) * 100, 2)
                : 0;
        }

        return consolidado.OrderBy(c => c.ConceptoNombre).ToList();
    }

    /// <summary>
    /// Crea un nuevo presupuesto con validaciones de duplicidad
    /// </summary>
    public async Task<PresupuestoDetalleDto> CrearAsync(CrearPresupuestoDto dto, string usuario)
    {
        // Validaciones
        if (dto.MontoPresupuestado <= 0)
            throw new InvalidOperationException("El monto presupuestado debe ser mayor a cero");

        if (dto.Mes < 1 || dto.Mes > 12)
            throw new InvalidOperationException("El mes debe estar entre 1 y 12");

        if (dto.Ano < 2000 || dto.Ano > 2100)
            throw new InvalidOperationException("El año no es válido");

        var conceptoExiste = await _context.Conceptos.AnyAsync(c => c.Id == dto.ConceptoId);
        if (!conceptoExiste)
            throw new InvalidOperationException("El concepto especificado no existe");

        // Verificar duplicidad
        var existe = await ExistePresupuestoAsync(dto.Ano, dto.Mes, dto.ConceptoId);
        if (existe)
            throw new InvalidOperationException($"Ya existe un presupuesto para el concepto en {dto.Ano}-{dto.Mes:00}");

        var presupuesto = new Presupuesto
        {
            Ano = dto.Ano,
            Mes = dto.Mes,
            ConceptoId = dto.ConceptoId,
            MontoPresupuestado = dto.MontoPresupuestado,
            Notas = dto.Notas,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = usuario
        };

        _context.Presupuestos.Add(presupuesto);
        await _context.SaveChangesAsync();

        return (await ObtenerDetalleAsync(presupuesto.Id))!;
    }

    /// <summary>
    /// Actualiza un presupuesto existente (solo monto y notas, no período ni concepto)
    /// </summary>
    public async Task<PresupuestoDetalleDto> ActualizarAsync(Guid id, ActualizarPresupuestoDto dto, string usuario)
    {
        var presupuesto = await _context.Presupuestos.FindAsync(id);
        if (presupuesto == null)
            throw new InvalidOperationException("Presupuesto no encontrado");

        if (dto.MontoPresupuestado <= 0)
            throw new InvalidOperationException("El monto presupuestado debe ser mayor a cero");

        presupuesto.MontoPresupuestado = dto.MontoPresupuestado;
        presupuesto.Notas = dto.Notas;
        presupuesto.UpdatedAt = DateTime.UtcNow;
        presupuesto.UpdatedBy = usuario;

        await _context.SaveChangesAsync();

        return (await ObtenerDetalleAsync(id))!;
    }

    /// <summary>
    /// Elimina un presupuesto (eliminación física)
    /// </summary>
    public async Task<bool> EliminarAsync(Guid id)
    {
        var presupuesto = await _context.Presupuestos.FindAsync(id);
        if (presupuesto == null)
            return false;

        _context.Presupuestos.Remove(presupuesto);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Calcula la ejecución de un presupuesto específico
    /// </summary>
    public async Task<decimal> CalcularEjecucionAsync(Guid presupuestoId)
    {
        var presupuesto = await _context.Presupuestos.FindAsync(presupuestoId);
        if (presupuesto == null)
            return 0;

        return await CalcularEjecucionPorConceptoYPeriodoAsync(
            presupuesto.ConceptoId, 
            presupuesto.Ano, 
            presupuesto.Mes);
    }

    /// <summary>
    /// Copia presupuestos de un período a otro
    /// Útil para presupuestar el próximo mes o año basándose en el anterior
    /// </summary>
    public async Task<int> CopiarPresupuestosAsync(
        int anoOrigen, 
        int mesOrigen, 
        int anoDestino, 
        int mesDestino, 
        string usuario)
    {
        // Validaciones
        if (mesOrigen < 1 || mesOrigen > 12 || mesDestino < 1 || mesDestino > 12)
            throw new InvalidOperationException("Los meses deben estar entre 1 y 12");

        if (anoOrigen == anoDestino && mesOrigen == mesDestino)
            throw new InvalidOperationException("El período origen y destino no pueden ser iguales");

        // Obtener presupuestos origen
        var presupuestosOrigen = await _context.Presupuestos
            .Where(p => p.Ano == anoOrigen && p.Mes == mesOrigen)
            .ToListAsync();

        if (!presupuestosOrigen.Any())
            throw new InvalidOperationException($"No existen presupuestos en el período origen {anoOrigen}-{mesOrigen:00}");

        int copiados = 0;

        foreach (var origen in presupuestosOrigen)
        {
            // Verificar si ya existe en destino
            var existe = await ExistePresupuestoAsync(anoDestino, mesDestino, origen.ConceptoId);
            if (existe)
                continue; // Saltar si ya existe

            var nuevo = new Presupuesto
            {
                Ano = anoDestino,
                Mes = mesDestino,
                ConceptoId = origen.ConceptoId,
                MontoPresupuestado = origen.MontoPresupuestado,
                Notas = $"Copiado desde {anoOrigen}-{mesOrigen:00}. {origen.Notas}".Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = usuario
            };

            _context.Presupuestos.Add(nuevo);
            copiados++;
        }

        if (copiados > 0)
            await _context.SaveChangesAsync();

        return copiados;
    }

    /// <summary>
    /// Verifica si existe un presupuesto para un concepto y período
    /// </summary>
    public async Task<bool> ExistePresupuestoAsync(int ano, int mes, int conceptoId, Guid? excluyendoId = null)
    {
        var query = _context.Presupuestos
            .Where(p => p.Ano == ano && p.Mes == mes && p.ConceptoId == conceptoId);

        if (excluyendoId.HasValue)
            query = query.Where(p => p.Id != excluyendoId.Value);

        return await query.AnyAsync();
    }

    #region Métodos Privados de Cálculo

    /// <summary>
    /// Calcula la ejecución real consultando Recibos y Egresos por concepto y período
    /// </summary>
    private async Task<decimal> CalcularEjecucionPorConceptoYPeriodoAsync(int conceptoId, int ano, int mes)
    {
        // Calcular rango de fechas del período
        var fechaInicio = new DateTime(ano, mes, 1);
        var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

        // Sumar ingresos desde ReciboItems (solo recibos emitidos, no anulados)
        var totalIngresos = await _context.ReciboItems
            .Where(ri => ri.ConceptoId == conceptoId 
                && ri.Recibo.FechaEmision >= fechaInicio 
                && ri.Recibo.FechaEmision <= fechaFin
                && ri.Recibo.Estado == EstadoRecibo.Emitido)
            .SumAsync(ri => ri.SubtotalCop);

        // Sumar egresos desde Egresos (filtrado por Categoria que corresponde al concepto)
        // Nota: Si Egreso.Categoria es string, necesitamos mapear por nombre del concepto
        var concepto = await _context.Conceptos.FindAsync(conceptoId);
        if (concepto == null)
            return totalIngresos;

        var totalEgresos = await _context.Egresos
            .Where(e => e.Categoria == concepto.Nombre 
                && e.Fecha >= fechaInicio 
                && e.Fecha <= fechaFin)
            .SumAsync(e => e.ValorCop);

        // Ejecución = Ingresos + Egresos (ambos impactan el presupuesto)
        return totalIngresos + totalEgresos;
    }

    /// <summary>
    /// Calcula la ejecución anual total por concepto
    /// </summary>
    private async Task<decimal> CalcularEjecucionAnualPorConceptoAsync(int conceptoId, int ano)
    {
        var fechaInicio = new DateTime(ano, 1, 1);
        var fechaFin = new DateTime(ano, 12, 31, 23, 59, 59);

        var totalIngresos = await _context.ReciboItems
            .Where(ri => ri.ConceptoId == conceptoId 
                && ri.Recibo.FechaEmision >= fechaInicio 
                && ri.Recibo.FechaEmision <= fechaFin
                && ri.Recibo.Estado == EstadoRecibo.Emitido)
            .SumAsync(ri => ri.SubtotalCop);

        var concepto = await _context.Conceptos.FindAsync(conceptoId);
        if (concepto == null)
            return totalIngresos;

        var totalEgresos = await _context.Egresos
            .Where(e => e.Categoria == concepto.Nombre 
                && e.Fecha >= fechaInicio 
                && e.Fecha <= fechaFin)
            .SumAsync(e => e.ValorCop);

        return totalIngresos + totalEgresos;
    }

    #endregion
}
