using Microsoft.EntityFrameworkCore;
using Server.Models;
using Server.Data;

namespace Server.Data.Seed;

/// <summary>
/// Seed para cargar el histórico de tesorería 2025 desde los informes mensuales.
/// Registra los ingresos y egresos de cada mes con sus conceptos específicos.
/// </summary>
public static class HistoricoTesoreria2025Seed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Verificar si ya se cargó el histórico de OCTUBRE 2025 (producción)
        var existeHistorico = await context.Recibos
            .AnyAsync(r => r.Ano == 2025 && r.Serie == "SI" && r.Consecutivo == 10);
        
        if (existeHistorico)
        {
            Console.WriteLine("✓ Histórico de tesorería octubre 2025 ya cargado");
            return;
        }

        Console.WriteLine("📋 Cargando histórico de tesorería octubre 2025 (producción)...");

        // Asegurar que existen los conceptos necesarios
        await AsegurarConceptosAsync(context);

        // Obtener conceptos
        var conceptos = await context.Conceptos
            .ToDictionaryAsync(c => c.Codigo, c => c.Id);

        // Cargar SOLO saldo inicial de octubre (producción comienza aquí)
        await CargarSaldoInicialOctubreAsync(context, conceptos);

        // OCTUBRE 2025 - Movimientos reales de producción
        await CargarOctubre2025Async(context, conceptos);

        // NOVIEMBRE 2025 - Cargar saldo inicial (arrastre de octubre)
        await CargarSaldoInicialNoviembreAsync(context, conceptos);

        await context.SaveChangesAsync();
        Console.WriteLine("✓ Histórico de tesorería octubre 2025 cargado exitosamente");
        Console.WriteLine("  📊 Saldo inicial: $4,718,042");
        Console.WriteLine("  📈 Ingresos: $2,193,300");
        Console.WriteLine("  📉 Egresos: $1,772,396");
        Console.WriteLine("  💰 Saldo final: $5,138,946");
    }

    private static async Task AsegurarConceptosAsync(AppDbContext context)
    {
        var conceptosNecesarios = new[]
        {
            ("SALDO_INICIAL", "Saldo Efectivo Mes Anterior", Moneda.COP, 0m, true),
            ("INSCRIPCION", "Inscripción Rally/Evento", Moneda.COP, 0m, true),
            ("VENTA_PARCHES", "Venta de Parches", Moneda.COP, 0m, true),
            ("VENTA_CAMISETAS", "Venta de Camisetas", Moneda.COP, 60000m, true),
            ("VENTA_BUFF", "Venta de Buff", Moneda.COP, 20000m, true),
            ("VENTA_BALACLAVA", "Venta de Balaclava", Moneda.COP, 20000m, true),
            ("VENTA_JERSEY", "Venta de Jersey", Moneda.COP, 0m, true),
            ("COMPRA_PARCHES", "Compra de Parches", Moneda.COP, 0m, false),
            ("COMPRA_MEMBRESIAS", "Compra de Membresías", Moneda.USD, 0m, false),
            ("TRANSPORTE", "Gastos de Transporte", Moneda.COP, 0m, false),
            ("ALQUILER_SALON", "Alquiler de Salón", Moneda.COP, 0m, false),
            ("ALQUILER_FINCA", "Alquiler de Finca", Moneda.COP, 0m, false),
            ("RECONOCIMIENTOS", "Reconocimientos/Recordatorios", Moneda.COP, 0m, false),
            ("FIESTA", "Gastos de Fiesta", Moneda.COP, 0m, false),
            ("BANQUETE", "Pago Banquete", Moneda.COP, 0m, false),
            ("PUBLICIDAD", "Publicidad/Marketing", Moneda.COP, 0m, false),
            ("TORTA", "Compra de Torta", Moneda.COP, 90000m, false),
            ("DONACION", "Donación", Moneda.COP, 0m, true),
            ("APOYO_CUBA", "Apoyo Cuba", Moneda.COP, 0m, false),
            ("INTERES_AHORRO", "Intereses Cuenta Ahorros", Moneda.COP, 0m, true),
            ("REINTEGRO", "Reintegro de Pago", Moneda.COP, 0m, false),
            ("AJUSTE", "Ajuste de Tesorería", Moneda.COP, 0m, true)
        };

        foreach (var (codigo, nombre, moneda, precioBase, esIngreso) in conceptosNecesarios)
        {
            var existe = await context.Conceptos.AnyAsync(c => c.Codigo == codigo);
            if (!existe)
            {
                context.Conceptos.Add(new Concepto
                {
                    Codigo = codigo,
                    Nombre = nombre,
                    Moneda = moneda,
                    PrecioBase = precioBase,
                    EsIngreso = esIngreso,
                    EsRecurrente = false,
                    Periodicidad = Periodicidad.Unico
                });
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Carga únicamente el saldo inicial de octubre 2025 (inicio de producción).
    /// </summary>
    private static async Task CargarSaldoInicialOctubreAsync(AppDbContext context, Dictionary<string, int> conceptos)
    {
        var conceptoSaldoInicial = conceptos["SALDO_INICIAL"];
        var mes = 10;
        var ano = 2025;
        var monto = 4718042m;
        var observaciones = "Saldo inicial Octubre 2025 - Inicio de operación en producción (entrega oficial tesorero: incluye ajustes no registrados en septiembre)";

        var fecha = new DateOnly(ano, mes, 1); // 1 de octubre 2025
        var recibo = new Recibo
        {
            Id = Guid.NewGuid(),
            Serie = "SI", // Serie especial para Saldo Inicial
            Ano = ano,
            Consecutivo = mes, // Consecutivo 10 (octubre)
            FechaEmision = fecha.ToDateTime(TimeOnly.MinValue),
            Estado = EstadoRecibo.Emitido,
            TotalCop = monto,
            Observaciones = observaciones,
            TerceroLibre = "TESORERÍA",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "seed-produccion"
        };

        recibo.Items.Add(new ReciboItem
        {
            ConceptoId = conceptoSaldoInicial,
            Cantidad = 1,
            MonedaOrigen = Moneda.COP,
            PrecioUnitarioMonedaOrigen = monto,
            SubtotalCop = monto,
            Notas = observaciones
        });

        context.Recibos.Add(recibo);
        Console.WriteLine("  ✓ Saldo inicial octubre $4,718,042 registrado");
    }

    /// <summary>
    /// Carga el saldo inicial de noviembre 2025 (arrastre del saldo final de octubre).
    /// </summary>
    private static async Task CargarSaldoInicialNoviembreAsync(AppDbContext context, Dictionary<string, int> conceptos)
    {
        // Verificar si ya existe el saldo inicial de noviembre
        var existeSaldoNoviembre = await context.Recibos
            .AnyAsync(r => r.Ano == 2025 && r.Serie == "SI" && r.Consecutivo == 11);
        
        if (existeSaldoNoviembre)
        {
            Console.WriteLine("  ✓ Saldo inicial noviembre ya existe");
            return;
        }

        var conceptoSaldoInicial = conceptos["SALDO_INICIAL"];
        var mes = 11;
        var ano = 2025;
        var monto = 5138946m; // Saldo final de octubre: $4,718,042 + $2,193,300 - $1,772,396
        var observaciones = "Saldo inicial Noviembre 2025 - Arrastre automático del saldo final de Octubre 2025";

        var fecha = new DateOnly(ano, mes, 1); // 1 de noviembre 2025
        var recibo = new Recibo
        {
            Id = Guid.NewGuid(),
            Serie = "SI", // Serie especial para Saldo Inicial
            Ano = ano,
            Consecutivo = mes, // Consecutivo 11 (noviembre)
            FechaEmision = fecha.ToDateTime(TimeOnly.MinValue),
            Estado = EstadoRecibo.Emitido,
            TotalCop = monto,
            Observaciones = observaciones,
            TerceroLibre = "TESORERÍA",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "seed-produccion"
        };

        recibo.Items.Add(new ReciboItem
        {
            ConceptoId = conceptoSaldoInicial,
            Cantidad = 1,
            MonedaOrigen = Moneda.COP,
            PrecioUnitarioMonedaOrigen = monto,
            SubtotalCop = monto,
            Notas = observaciones
        });

        context.Recibos.Add(recibo);
        Console.WriteLine("  ✓ Saldo inicial noviembre $5,138,946 registrado (arrastre de octubre)");
    }

    #region Métodos de carga por mes

    private static async Task CargarEnero2025Async(AppDbContext context, Dictionary<string, int> conceptos)
    {
        var fecha = new DateOnly(2025, 1, 31);
        int consecutivo = 1000;

        // Ingresos - Total: $2,131,800
        var ingresos = new[]
        {
            ("MENSUALIDAD", 60000m, "Mario Jiménez"),
            ("VENTA_PARCHES", 253000m, "Movie Arzuza"),
            ("MENSUALIDAD", 40000m, "Angela"),
            ("MENSUALIDAD", 180000m, "Movie"),
            ("MENSUALIDAD", 106000m, "Héctor + Inscripción 2025"),
            ("VENTA_PARCHES", 265800m, "Angela"),
            ("MENSUALIDAD", 80000m, "Camilo Ortegón"),
            ("MENSUALIDAD", 60000m, "Carlos Andrés Peres"),
            ("MENSUALIDAD", 80000m, "Edinson Ospina"),
            ("MENSUALIDAD", 60000m, "Jhon Harvey"),
            ("MENSUALIDAD", 120000m, "Carlos Mario Ceballos"),
            ("MENSUALIDAD", 120000m, "César Rodríguez"),
            ("MENSUALIDAD", 120000m, "Robinson Galvis"),
            ("MENSUALIDAD", 80000m, "Juan Esteban Suárez"),
            ("MENSUALIDAD", 80000m, "Girlesa Buitrago + Inscripción"),
            ("MENSUALIDAD", 46000m, "Ramón González"),
            ("MENSUALIDAD", 66000m, "Daniel Enero 2025 + Membresía"),
            ("FIESTA", 315000m, "Recaudo cuota fiesta fin de año")
        };

        foreach (var (concepto, monto, desc) in ingresos)
        {
            await CrearReciboAsync(context, conceptos[concepto], fecha, consecutivo++, monto, $"Histórico Enero 2025 - {desc}");
        }

        // Egresos - Total: $3,340,000
        var egresos = new[]
        {
            ("TRANSPORTE", 380000m, "Sillas y mesas transporte para aniversario"),
            ("COMPRA_PARCHES", 1098000m, "Parches internacionales"),
            ("TRANSPORTE", 22000m, "Transportadora envío parches"),
            ("RECONOCIMIENTOS", 40000m, "5 recordatorios (3 Argentina, 1 Buga, 1 Pereira)"),
            ("FIESTA", 1750000m, "Fiesta fin de año"),
            ("ALQUILER_SALON", 50000m, "Ramón uso salón asamblea enero")
        };

        foreach (var (concepto, monto, desc) in egresos)
        {
            await CrearEgresoAsync(context, conceptos[concepto], fecha, consecutivo++, monto, $"Histórico Enero 2025 - {desc}");
        }

        Console.WriteLine("  ✓ Enero: 18 ingresos, 6 egresos");
    }

    private static async Task CargarFebrero2025Async(AppDbContext context, Dictionary<string, int> conceptos)
    {
        var fecha = new DateOnly(2025, 2, 28);
        int consecutivo = 2000;

        // Ingresos - Total: $1,595,800
        var ingresos = new[]
        {
            ("MENSUALIDAD", 46000m, "Membresía esposa Daniel Villamizar"),
            ("MENSUALIDAD", 220000m, "Daniel Feb-Dic"),
            ("MENSUALIDAD", 46000m, "Membresía Jhon Arzuza"),
            ("MENSUALIDAD", 46000m, "Membresía Carlos Julio Movie"),
            ("MENSUALIDAD", 66000m, "Membresía + Enero Angela María"),
            ("MENSUALIDAD", 92000m, "Membresía Mario Jiménez"),
            ("MENSUALIDAD", 46000m, "Membresía Jhon David Sánchez Tiin"),
            ("MENSUALIDAD", 92000m, "Membresía Jhon Jarvey"),
            ("MENSUALIDAD", 46000m, "Membresía Camilo Ortegón"),
            ("MENSUALIDAD", 46000m, "Membresía Yeferson Usuga"),
            ("MENSUALIDAD", 45000m, "Membresía Jeferson Montoya"),
            ("MENSUALIDAD", 20000m, "Enero Girlesa"),
            ("MENSUALIDAD", 286000m, "Membresía + Ene-Dic Carlos Araque"),
            ("MENSUALIDAD", 166000m, "Membresía + Ene-Jun Capa"),
            ("MENSUALIDAD", 92000m, "Membresía José Edinson Ospina"),
            ("MENSUALIDAD", 92000m, "Membresía Robinson Gálvez"),
            ("VENTA_PARCHES", 128800m, "Daniel Villamizar"),
            ("MENSUALIDAD", 20000m, "Febrero Angela")
        };

        foreach (var (concepto, monto, desc) in ingresos)
        {
            await CrearReciboAsync(context, conceptos[concepto], fecha, consecutivo++, monto, $"Histórico Febrero 2025 - {desc}");
        }

        // Egresos - Total: $2,768,122
        var egresos = new[]
        {
            ("RECONOCIMIENTOS", 50000m, "Muestra reconocimiento"),
            ("COMPRA_MEMBRESIAS", 2251622m, "Compra membresías"),
            ("TRANSPORTE", 16500m, "Transportadora membresías"),
            ("ALQUILER_FINCA", 200000m, "Alquiler finca fiesta fin de año"),
            ("RECONOCIMIENTOS", 250000m, "5 recordatorios")
        };

        foreach (var (concepto, monto, desc) in egresos)
        {
            await CrearEgresoAsync(context, conceptos[concepto], fecha, consecutivo++, monto, $"Histórico Febrero 2025 - {desc}");
        }

        Console.WriteLine("  ✓ Febrero: 18 ingresos, 5 egresos");
    }

    private static async Task CargarMarzo2025Async(AppDbContext context, Dictionary<string, int> conceptos)
    {
        // Marzo tiene los mismos movimientos que febrero
        var fecha = new DateOnly(2025, 3, 31);
        int consecutivo = 3000;

        // Ingresos - Total: $1,595,800 (mismos de febrero)
        var ingresos = new[]
        {
            ("MENSUALIDAD", 46000m, "Esposa Daniel Villamizar"),
            ("MENSUALIDAD", 220000m, "Daniel Villamizar FEB-DIC"),
            ("MENSUALIDAD", 46000m, "Jhon Arzuza"),
            ("MENSUALIDAD", 46000m, "Carlos Julio Movie"),
            ("MENSUALIDAD", 66000m, "Angela María + Enero"),
            ("MENSUALIDAD", 92000m, "Mario Jiménez"),
            ("MENSUALIDAD", 46000m, "Jhon David Sánchez Tiin"),
            ("MENSUALIDAD", 92000m, "Jhon Jarvey"),
            ("MENSUALIDAD", 46000m, "Camilo Ortegón"),
            ("MENSUALIDAD", 46000m, "Yeferson Usuga"),
            ("MENSUALIDAD", 45000m, "Jeferson Montoya"),
            ("MENSUALIDAD", 20000m, "Girlesa Enero"),
            ("MENSUALIDAD", 286000m, "Carlos Araque ENE-DIC"),
            ("MENSUALIDAD", 166000m, "Capa ENE-JUN"),
            ("MENSUALIDAD", 92000m, "José Edinson Ospina"),
            ("MENSUALIDAD", 92000m, "Robinson Galvez"),
            ("VENTA_PARCHES", 128800m, "Daniel Villamizar"),
            ("MENSUALIDAD", 20000m, "Angela Febrero")
        };

        foreach (var (concepto, monto, desc) in ingresos)
        {
            await CrearReciboAsync(context, conceptos[concepto], fecha, consecutivo++, monto, $"Histórico Marzo 2025 - {desc}");
        }

        // Egresos - Total: $2,768,122 (mismos de febrero)
        var egresos = new[]
        {
            ("RECONOCIMIENTOS", 50000m, "Compra muestra reconocimiento"),
            ("COMPRA_MEMBRESIAS", 2251622m, "Compra de membresías"),
            ("TRANSPORTE", 16500m, "Transportadora membresías"),
            ("ALQUILER_FINCA", 200000m, "Alquiler finca fiesta fin de año"),
            ("RECONOCIMIENTOS", 250000m, "5 recordatorios")
        };

        foreach (var (concepto, monto, desc) in egresos)
        {
            await CrearEgresoAsync(context, conceptos[concepto], fecha, consecutivo++, monto, $"Histórico Marzo 2025 - {desc}");
        }

        Console.WriteLine("  ✓ Marzo: 18 ingresos, 5 egresos");
    }

    private static async Task CargarAbril2025Async(AppDbContext context, Dictionary<string, int> conceptos)
    {
        var fecha = new DateOnly(2025, 4, 30);
        int consecutivo = 4000;

        // Ingresos - Total: $1,227,050
        var ingresos = new[]
        {
            ("MENSUALIDAD", 72000m, "Membresía Milton Gómez"),
            ("AJUSTE", 655050m, "Venta 165 dólares @ 3,970"),
            ("VENTA_CAMISETAS", 480000m, "8 camisetas"),
            ("VENTA_BUFF", 20000m, "1 buff")
        };

        foreach (var (concepto, monto, desc) in ingresos)
        {
            await CrearReciboAsync(context, conceptos[concepto], fecha, consecutivo++, monto, $"Histórico Abril 2025 - {desc}");
        }

        // Egresos - Total: $1,244,082
        var egresos = new[]
        {
            ("COMPRA_MEMBRESIAS", 438798m, "5 x $20 USD @ TRM 4.387,98"),
            ("COMPRA_PARCHES", 772284m, "176 USD parches @ TRM 4.387,98"),
            ("TRANSPORTE", 16500m, "Transportadora membresías"),
            ("TRANSPORTE", 16500m, "Transportadora Interrapidisimo")
        };

        foreach (var (concepto, monto, desc) in egresos)
        {
            await CrearEgresoAsync(context, conceptos[concepto], fecha, consecutivo++, monto, $"Histórico Abril 2025 - {desc}");
        }

        Console.WriteLine("  ✓ Abril: 4 ingresos, 4 egresos");
    }

    private static async Task CargarMayo2025Async(AppDbContext context, Dictionary<string, int> conceptos)
    {
        var fecha = new DateOnly(2025, 5, 31);
        int consecutivo = 5000;

        // Ingresos - Total: $60,000
        await CrearReciboAsync(context, conceptos["VENTA_CAMISETAS"], fecha, consecutivo++, 60000m, "Histórico Mayo 2025 - Camiseta Julián");

        // Egresos - Total: $106,500
        var egresos = new[]
        {
            ("TORTA", 90000m, "Torta cumpleaños trimestre"),
            ("TRANSPORTE", 16500m, "Transportadora Interrapidisimo 4 parches membresía")
        };

        foreach (var (concepto, monto, desc) in egresos)
        {
            await CrearEgresoAsync(context, conceptos[concepto], fecha, consecutivo++, monto, $"Histórico Mayo 2025 - {desc}");
        }

        Console.WriteLine("  ✓ Mayo: 1 ingreso, 2 egresos");
    }

    private static async Task CargarJulio2025Async(AppDbContext context, Dictionary<string, int> conceptos)
    {
        var fecha = new DateOnly(2025, 7, 31);
        int consecutivo = 7000;

        // Ingresos - Total: $3,500,000
        await CrearReciboAsync(context, conceptos["INSCRIPCION"], fecha, consecutivo++, 3500000m, "Histórico Julio 2025 - Inscripciones pagas julio");

        // Egresos - Total: $6,829,000
        var egresos = new[]
        {
            ("BANQUETE", 1000000m, "Abono banquete 16/07"),
            ("VENTA_JERSEY", 2672000m, "Abono Jersey's 31/07"),
            ("RECONOCIMIENTOS", 395000m, "Recordatorio parches 23/07"),
            ("VENTA_JERSEY", 2762000m, "Abono Jersey's 31/07")
        };

        foreach (var (concepto, monto, desc) in egresos)
        {
            await CrearEgresoAsync(context, conceptos[concepto], fecha, consecutivo++, monto, $"Histórico Julio 2025 - {desc}");
        }

        Console.WriteLine("  ✓ Julio: 1 ingreso, 4 egresos");
    }

    private static async Task CargarAgosto2025Async(AppDbContext context, Dictionary<string, int> conceptos)
    {
        var fecha = new DateOnly(2025, 8, 31);
        int consecutivo = 8000;

        // Ingresos - Total: $4,941,000
        var ingresos = new[]
        {
            ("INSCRIPCION", 3980000m, "Inscripciones, Jersey's, Donaciones - Agosto"),
            ("VENTA_JERSEY", 101000m, "Devolución Jersey's 01/08"),
            ("INSCRIPCION", 860000m, "Inscripciones efectivo 04/08")
        };

        foreach (var (concepto, monto, desc) in ingresos)
        {
            await CrearReciboAsync(context, conceptos[concepto], fecha, consecutivo++, monto, $"Histórico Agosto 2025 - {desc}");
        }

        // Egresos - Total: $4,189,800
        var egresos = new[]
        {
            ("BANQUETE", 1000000m, "Abono banquete #2 01/08"),
            ("RECONOCIMIENTOS", 362000m, "Reconocimientos visitantes 01/08"),
            ("REINTEGRO", 281500m, "Reintegro pagos efectivo 03/08"),
            ("BANQUETE", 1546300m, "Cancelación total banquete 04/08"),
            ("PUBLICIDAD", 1000000m, "Veracruz Estéreo 04/08")
        };

        foreach (var (concepto, monto, desc) in egresos)
        {
            await CrearEgresoAsync(context, conceptos[concepto], fecha, consecutivo++, monto, $"Histórico Agosto 2025 - {desc}");
        }

        Console.WriteLine("  ✓ Agosto: 3 ingresos, 5 egresos");
    }

    private static async Task CargarSeptiembre2025Async(AppDbContext context, Dictionary<string, int> conceptos)
    {
        var fecha = new DateOnly(2025, 9, 30);
        int consecutivo = 9000;

        // Ingresos - Total: $2,871,975
        var ingresos = new[]
        {
            ("INSCRIPCION", 1620000m, "Consignaciones inscripciones"),
            ("INTERES_AHORRO", 11975m, "Intereses corrientes cuenta ahorros"),
            ("MENSUALIDAD", 360000m, "Mensualidad + Inscripción César 10/09"),
            ("MENSUALIDAD", 140000m, "Mensualidad Angela María 11/09"),
            ("VENTA_PARCHES", 140000m, "Venta parches LAMA 11/09"),
            ("VENTA_JERSEY", 600000m, "Venta Jersey's LAMA 11/09")
        };

        foreach (var (concepto, monto, desc) in ingresos)
        {
            await CrearReciboAsync(context, conceptos[concepto], fecha, consecutivo++, monto, $"Histórico Septiembre 2025 - {desc}");
        }

        Console.WriteLine("  ✓ Septiembre: 6 ingresos, 0 egresos");
    }

    private static async Task CargarOctubre2025Async(AppDbContext context, Dictionary<string, int> conceptos)
    {
        var fechaOct = new DateOnly(2025, 10, 31);
        int consecutivo = 10000;

        // Ingresos - Total: $2,193,300
        var ingresos = new[]
        {
            ("VENTA_JERSEY", 1913300m, "30 jersey $50,000 aniversario Rally Sudamericano 01/10"),
            ("VENTA_CAMISETAS", 120000m, "2 camisetas LM 02/10"),
            ("VENTA_BALACLAVA", 40000m, "2 balaclavas 03/10"),
            ("VENTA_CAMISETAS", 120000m, "2 camisetas LM Robinson 05/10")
        };

        foreach (var (concepto, monto, desc) in ingresos)
        {
            await CrearReciboAsync(context, conceptos[concepto], fechaOct, consecutivo++, monto, $"Histórico Octubre 2025 - {desc}");
        }

        // Egresos - Total: $1,772,396
        var egresos = new[]
        {
            ("APOYO_CUBA", 600000m, "Parches apoyo Cuba 04/10"),
            ("FIESTA", 133200m, "Compra de 2 pollos Frisby"),
            ("FIESTA", 17290m, "Compra de gaseosas"),
            ("FIESTA", 211762m, "Sancocho donde Milton"),
            ("FIESTA", 53144m, "Revuelto"),
            ("FIESTA", 39800m, "Gaseosas y cervezas"),
            ("FIESTA", 63200m, "Gaseosas y cervezas"),
            ("RECONOCIMIENTOS", 190000m, "Arreglo floral madre de Maria Paez"),
            ("PUBLICIDAD", 464000m, "Pago inscripción Cámara de Comercio")
        };

        foreach (var (concepto, monto, desc) in egresos)
        {
            await CrearEgresoAsync(context, conceptos[concepto], fechaOct, consecutivo++, monto, $"Histórico Octubre 2025 - {desc}");
        }

        Console.WriteLine("  ✓ Octubre: 4 ingresos, 9 egresos");
    }

    #endregion

    #region Métodos auxiliares

    private static async Task CrearReciboAsync(AppDbContext context, int conceptoId, DateOnly fecha, int consecutivo, decimal monto, string observaciones)
    {
        var recibo = new Recibo
        {
            Id = Guid.NewGuid(),
            Serie = "HT",
            Ano = 2025,
            Consecutivo = consecutivo,
            FechaEmision = fecha.ToDateTime(TimeOnly.MinValue),
            Estado = EstadoRecibo.Emitido,
            TotalCop = monto,
            Observaciones = observaciones,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "seed-historico"
        };

        recibo.Items.Add(new ReciboItem
        {
            ConceptoId = conceptoId,
            Cantidad = 1,
            MonedaOrigen = Moneda.COP,
            PrecioUnitarioMonedaOrigen = monto,
            SubtotalCop = monto
        });

        context.Recibos.Add(recibo);
    }

    private static async Task CrearEgresoAsync(AppDbContext context, int conceptoId, DateOnly fecha, int consecutivo, decimal monto, string observaciones)
    {
        // Obtener el concepto para usar como categoría
        var concepto = await context.Conceptos.FindAsync(conceptoId);
        
        var egreso = new Egreso
        {
            Id = Guid.NewGuid(),
            Fecha = fecha.ToDateTime(TimeOnly.MinValue),
            Categoria = concepto?.Nombre ?? "Otros",
            Proveedor = "Histórico",
            Descripcion = observaciones,
            ValorCop = monto,
            UsuarioRegistro = "seed-historico",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "seed-historico"
        };

        context.Egresos.Add(egreso);
    }

    #endregion
}
