using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using Server.Data;
using Server.Models;
using Server.Services.Exchange;

namespace Server.Services.Deudores;

/// <summary>
/// Implementación del servicio de cálculo de deudores de mensualidad.
/// </summary>
public class DeudoresService : IDeudoresService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IExchangeRateService _trm;
    
    public DeudoresService(IDbContextFactory<AppDbContext> dbFactory, IExchangeRateService trm)
    {
        _dbFactory = dbFactory;
        _trm = trm;
    }

    /// <inheritdoc />
    /// <summary>
    /// Calcula los deudores para un rango de meses [desde, hasta] dentro del AÑO del <paramref name="hasta"/>.
    /// Reglas clave:
    /// - Si <paramref name="desde"/> es null, se toma el inicio del año del reporte.
    /// - Si <paramref name="hasta"/> es null, se asume el corte en octubre del año actual (calibrado 2025-10).
    /// - Para cada miembro, el primer mes exigible es el máximo entre: mes de ingreso en el año del reporte y el mes <paramref name="desde"/>.
    /// - Los meses cubiertos por pagos se asignan secuencialmente desde el primer mes exigible.
    /// - Se excluyen los miembros con Rango = "Asociado".
    /// </summary>
    public async Task<List<DeudorRow>> CalcularAsync(DateOnly? desde = null, DateOnly? hasta = null, CancellationToken ct = default)
    {
        await using var _db = await _dbFactory.CreateDbContextAsync(ct);
        
        // Determinar año y rango del reporte (soporta cualquier mes del año y de aquí en adelante)
        var fechaBase = hasta ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var anioReporte = fechaBase.Year;
        var limiteHasta = hasta ?? new DateOnly(anioReporte, 10, 1); // por defecto calibrado a octubre
        int mesFinal = limiteHasta.Month; // Mes final del reporte
        // Mes inicial del reporte (dentro del mismo año del corte)
        int mesInicial = (desde.HasValue && desde.Value.Year == anioReporte) ? desde.Value.Month : 1;
        var mensualidad = await _db.Conceptos.FirstAsync(x => x.Codigo == "MENSUALIDAD", ct);

        // Tabla de pagos especiales (NombreCanon, UltimoMesPagado, MesPrimerPagoRequerido)
        var pagosEspeciales = new List<(string NombreCanon, int? UltimoMesPagado, int? MesPrimerPagoRequerido)>
        {
            ("RAMON ANTONIO  GONZALEZ  CASTAÑO", 10, 1),
            ("CESAR LEONEL RODRIGUEZ GALAN", 9, 1),
            ("ANGELA MARIA RODRIGUEZ OCHOA", 9, 1),
            ("CARLOS ANDRES PEREZ AREIZA", 6, 1),
            ("DANIEL ANDREY VILLAMIZAR ARAQUE", 6, 1),
            ("MILTON DARIO GOMEZ RIVERA", 6, 1),
            ("GIRLESA MARIA BUITRAGO", 1, 1),
            ("CARLOS ALBERTO ARAQUE BETANCUR", 12, 1),
            ("LAURA VIVIANA SALAZAR MORENO", null, 6),
            ("JOSE JULIAN VILLAMIZAR ARAQUE", null, 6),
            ("GUSTAVO ADOLFO GOMEZ ZULUAGA", null, 10),
            ("NELSON AUGUSTO MONTOYA MATAUTE", null, 10),
            ("HECTOR MARIO GONZALEZ HENAO", null, 1),
            ("JHON HARVEY GOMEZ PATIÑO", null, 1),
            ("CARLOS MARIO  CEBALLOS", null, 1),
            ("JUAN ESTEBAN  SUAREZ CORREA", null, 1),
            ("JOSE EDINSON  OSPINA CRUZ", null, 1),
            ("JEFFERSON  MONTOYA MUÑOZ", null, 1),
            ("ROBINSON ALEJANDRO GALVIS PARRA", null, 1),
            ("JHON EMMANUEL ARZUZA PAEZ", null, 1),
            ("JUAN ESTEBAN  OSORIO", null, 1),
            ("YEFERSON BAIRON  USUGA AGUDELO", null, 1),
            ("JHON DAVID SANCHEZ", null, 1),
            ("CARLOS JULIO  RENDON DIAZ", null, 1),
            ("JENNIFER ANDREA CARDONA BENITEZ", null, 1),
            ("WILLIAM HUMBERTO JIMENEZ PEREZ", null, 1),
            ("CARLOS MARIO DIAZ DIAZ", null, 1)
        };

        // Cargar miembros activos y excluir "Asociados"
        var miembros = await _db.Miembros
            .AsNoTracking()
            .Where(m => m.Estado == EstadoMiembro.Activo)
            .ToListAsync(ct);
        miembros = miembros
            .Where(m => !string.Equals((m.Rango ?? string.Empty).Trim(), "Asociado", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Cargar todos los recibos emitidos con items de mensualidad
        // Cargar todos los recibos emitidos con items de mensualidad, agrupados por miembro en memoria
        var pagos = await _db.Recibos
            .Include(r => r.Items)
            .Where(r => r.Estado == EstadoRecibo.Emitido && r.Items.Any(i => i.ConceptoId == mensualidad.Id))
            .ToListAsync(ct);
        var pagosPorMiembro = pagos
            .Where(r => r.MiembroId != null)
            .GroupBy(r => r.MiembroId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(r => r.FechaEmision).ToList());

        var rows = new List<DeudorRow>();
        foreach (var m in miembros)
        {
            // Normalizar nombre canónico para comparación con tabla de especiales
            var nombreCanon = NormalizacionNombres.NormalizeNombre(m.NombreCompleto ?? ($"{m.Nombres} {m.Apellidos}"));

            // Buscar si tiene regla especial
            var pagoEspecial = pagosEspeciales.FirstOrDefault(pe => nombreCanon == NormalizacionNombres.NormalizeNombre(pe.NombreCanon));
            int? ultimoMesPagado = pagoEspecial != default ? pagoEspecial.UltimoMesPagado : null;
            int? mesPrimerPagoRequerido = pagoEspecial != default ? pagoEspecial.MesPrimerPagoRequerido : null;

            // Si no tiene regla especial, aplicar lógica por defecto (como en SQL)
            if (mesPrimerPagoRequerido == null)
            {
                if (m.FechaIngreso == null)
                {
                    mesPrimerPagoRequerido = mesInicial;
                }
                else if (m.FechaIngreso.Value.Year < anioReporte)
                {
                    // Ingresó antes del año del reporte: empieza a exigir desde el mesInicial
                    mesPrimerPagoRequerido = mesInicial;
                }
                else if (m.FechaIngreso.Value.Year == anioReporte)
                {
                    // Ingresó en el año del reporte: exigir desde su mes de ingreso o desde el filtro 'desde' (lo mayor)
                    mesPrimerPagoRequerido = Math.Max(m.FechaIngreso.Value.Month, mesInicial);
                }
                else
                {
                    // Ingresará después del año del reporte: no debe en este año
                    mesPrimerPagoRequerido = 13; // fuera del rango (enero..diciembre)
                }
            }
            if (ultimoMesPagado == null)
            {
                // Buscar pagos reales en memoria
                pagosPorMiembro.TryGetValue(m.Id, out var recibosDelMiembro);
                int maxMesPagado = 0;
                int mesAsignable = mesPrimerPagoRequerido.Value;
                if (recibosDelMiembro != null)
                {
                    foreach (var recibo in recibosDelMiembro)
                    {
                        var itemMensualidad = recibo.Items.FirstOrDefault(i => i.ConceptoId == mensualidad.Id);
                        if (itemMensualidad == null) continue;
                        int cantidadMeses = Math.Max(0, itemMensualidad.Cantidad);
                        for (int i = 0; i < cantidadMeses; i++)
                        {
                            if (mesAsignable > mesFinal) break;
                            maxMesPagado = mesAsignable;
                            mesAsignable++;
                        }
                    }
                }
                ultimoMesPagado = maxMesPagado > 0 ? maxMesPagado : null;
            }

            // Correcciones manuales específicas (evitar falsos negativos por codificación)
            // Se confían a la comparación normalizada anterior. Mantener aquí por compatibilidad si se requiere añadir algún caso puntual.

            // Calcular meses adeudados
            int mesesAdeudados = 0;
            int? primerMesAdeudado = null;
            
            // Verificar si el primer mes requerido está fuera del rango
            if ((mesPrimerPagoRequerido ?? 1) > mesFinal)
            {
                // No debe nada porque aún no empieza su obligación
                mesesAdeudados = 0;
            }
            // Verificar si ya pagó hasta el mes final o más allá
            else if ((ultimoMesPagado ?? 0) >= mesFinal)
            {
                // Ya está al día
                mesesAdeudados = 0;
            }
            else
            {
                // Calcular desde qué mes debe
                int mesInicioDeuda = Math.Max((ultimoMesPagado ?? 0) + 1, mesPrimerPagoRequerido ?? 1);
                if (mesInicioDeuda <= mesFinal)
                {
                    mesesAdeudados = mesFinal - mesInicioDeuda + 1;
                    primerMesAdeudado = mesInicioDeuda;
                }
            }

            // Generar lista de meses pendientes
            var mesesPend = new List<DateOnly>();
            if (mesesAdeudados > 0 && primerMesAdeudado != null)
            {
                // Respetar el filtro 'desde' dentro del mismo año del reporte
                int inicioListado = Math.Max(primerMesAdeudado.Value, mesInicial);
                for (int mes = inicioListado; mes <= mesFinal; mes++)
                {
                    mesesPend.Add(new DateOnly(anioReporte, mes, 1));
                }
            }

            if (mesesPend.Count > 0)
            {
                rows.Add(new DeudorRow
                {
                    MiembroId = m.Id,
                    Nombre = m.Nombres + " " + m.Apellidos,
                    Ingreso = m.FechaIngreso,
                    MesesPendientes = mesesPend
                });
            }
        }

        return rows.OrderByDescending(r => r.MesesPendientes.Count).ToList();
    }

    /// <inheritdoc />
    public async Task<DeudorDetalle?> ObtenerDetalleAsync(Guid miembroId, DateOnly? desde = null, DateOnly? hasta = null, CancellationToken ct = default)
    {
        await using var _db = await _dbFactory.CreateDbContextAsync(ct);
        
        var miembro = await _db.Miembros.FirstOrDefaultAsync(m => m.Id == miembroId, ct);
        if (miembro is null) return null;

        var mensualidad = await _db.Conceptos.FirstAsync(x => x.Codigo == "MENSUALIDAD", ct);

        // Determinar rango de cálculo
        var ingreso = miembro.FechaIngreso ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var inicioMiembro = new DateOnly(ingreso.Year, ingreso.Month, 1);
        var inicioAnoActual = new DateOnly(DateTime.UtcNow.Year, 1, 1);
        var inicio = desde ?? inicioAnoActual;
        if (inicio < inicioMiembro) inicio = inicioMiembro;
        var limiteHasta = hasta ?? DateOnly.FromDateTime(DateTime.UtcNow);

        // Recibos del miembro con mensualidad
        var recibosDelMiembro = await _db.Recibos
            .Include(r => r.Items)
            .Where(r => r.Estado == EstadoRecibo.Emitido && r.MiembroId == miembroId && r.Items.Any(i => i.ConceptoId == mensualidad.Id))
            .OrderBy(r => r.FechaEmision)
            .ToListAsync(ct);

        // Asignación secuencial de meses cubiertos
        var mesesCubiertos = new HashSet<DateOnly>();
        var proximoMesAsignable = new DateOnly(inicio.Year, inicio.Month, 1);
        foreach (var recibo in recibosDelMiembro)
        {
            var itemMensualidad = recibo.Items.First(i => i.ConceptoId == mensualidad.Id);
            var cantidadMeses = Math.Max(0, itemMensualidad.Cantidad);
            for (int i = 0; i < cantidadMeses; i++)
            {
                if (proximoMesAsignable > limiteHasta) break;
                mesesCubiertos.Add(proximoMesAsignable);
                proximoMesAsignable = proximoMesAsignable.AddMonths(1);
            }
        }

        // Construir listas de pagados y pendientes dentro del rango
        var mesesPagados = new List<DateOnly>();
        var mesesPendientes = new List<DateOnly>();
        var cursor = inicio;
        while (cursor <= limiteHasta)
        {
            var primerDiaMes = new DateOnly(cursor.Year, cursor.Month, 1);
            if (mesesCubiertos.Contains(primerDiaMes)) mesesPagados.Add(primerDiaMes);
            else mesesPendientes.Add(primerDiaMes);
            cursor = cursor.AddMonths(1);
        }

        return new DeudorDetalle
        {
            MiembroId = miembro.Id,
            Nombre = miembro.Nombres + " " + miembro.Apellidos,
            Ingreso = miembro.FechaIngreso,
            MesesPagados = mesesPagados,
            MesesPendientes = mesesPendientes
        };
    }

    /// <inheritdoc />
    public async Task<decimal> ObtenerPrecioMensualidadCopAsync(DateOnly? fechaReferencia = null, CancellationToken ct = default)
    {
        await using var _db = await _dbFactory.CreateDbContextAsync(ct);
        
        var mensualidad = await _db.Conceptos.FirstOrDefaultAsync(c => c.Codigo == "MENSUALIDAD", ct);
        if (mensualidad == null) return 0;

        if (mensualidad.Moneda == Server.Models.Moneda.USD)
        {
            var refDate = fechaReferencia ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var trm = await _trm.GetUsdCopAsync(refDate, ct);
            return decimal.Round(mensualidad.PrecioBase * trm, 2);
        }
        return mensualidad.PrecioBase;
    }
}

/// <summary>
/// Utilidades para normalización de nombres con eliminación de diacríticos y espacios redundantes.
/// </summary>
internal static class NormalizacionNombres
{
    /// <summary>
    /// Normaliza un nombre: recorta, colapsa espacios, convierte a mayúsculas y elimina diacríticos.
    /// </summary>
    public static string NormalizeNombre(string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return string.Empty;
        var trimmed = string.Join(' ', nombre.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        var upper = trimmed.ToUpperInvariant();
        var formD = upper.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (var ch in formD)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}

