using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using Server.Models;

namespace Server.Data.Seed;

/// <summary>
/// Seed para registrar recibos de caja del año 2025 según información contable inicial.
/// Aplicación empezó a regir en enero 2025.
/// </summary>
public static class Recibos2025Seed
{
    private static string Norm(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        var norm = s.Normalize(NormalizationForm.FormD);
        var chars = norm.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
        return new string(chars).ToUpperInvariant();
    }
    public static async Task SeedAsync(AppDbContext db)
    {
        Console.WriteLine("📋 Iniciando seed de recibos 2025 (idempotente)...");

        // Obtener conceptos
        var conceptoMensualidad = await db.Conceptos.FirstOrDefaultAsync(c => c.Codigo == "MENSUALIDAD");
        var conceptoRenovMem = await db.Conceptos.FirstOrDefaultAsync(c => c.Codigo == "RENOV_ANUAL");
        var conceptoParche = await db.Conceptos.FirstOrDefaultAsync(c => c.Codigo == "LAMA_PARCHE");
        var conceptoAuxilio = await db.Conceptos.FirstOrDefaultAsync(c => c.Codigo == "AUXILIO");
        
        if (conceptoMensualidad == null)
        {
            Console.WriteLine("❌ No se encontró concepto MENSUALIDAD");
            return;
        }

        // Crear concepto de auxilio si no existe
        if (conceptoAuxilio == null)
        {
            conceptoAuxilio = new Concepto
            {
                Codigo = "AUXILIO",
                Nombre = "Auxilio por Asistencia a Evento",
                Moneda = Moneda.COP,
                PrecioBase = 0m,
                EsRecurrente = false,
                Periodicidad = Periodicidad.Unico
            };
            db.Conceptos.Add(conceptoAuxilio);
            await db.SaveChangesAsync();
        }

        // Obtener TRM promedio para conversiones
        var trmActual = await db.TasasCambio.OrderByDescending(t => t.Fecha).FirstOrDefaultAsync();
        decimal trm = trmActual?.UsdCop ?? 4000m;

        var miembros = await db.Miembros.ToListAsync();

        // === Ajustar fechas de ingreso según la realidad reportada ===
        // Junio 2025
        var ingresoJunioNombres = new[]
        {
            "LAURA VIVIANA SALAZAR MORENO",
            "JOSE JULIAN VILLAMIZAR ARAQUE"
        };
        foreach (var nombre in ingresoJunioNombres)
        {
            var target = Norm(nombre);
            var m = miembros.FirstOrDefault(x => Norm(x.NombreCompleto).Contains(target));
            if (m != null)
            {
                if (m.FechaIngreso == null || m.FechaIngreso < new DateOnly(2025, 6, 1))
                {
                    m.FechaIngreso = new DateOnly(2025, 6, 1);
                    Console.WriteLine($"🗓️ Actualizada FechaIngreso a 2025-06-01 para {m.NombreCompleto}");
                }
            }
            else
            {
                Console.WriteLine($"⚠️ No se encontró miembro para actualizar ingreso (junio): {nombre}");
            }
        }

        // Octubre 2025
        var ingresoOctubreNombres = new[]
        {
            "GUSTAVO ADOLFO GÓMEZ ZULUAGA",
            "NELSON AUGUSTO MONTOYA MATAUTE"
        };
        foreach (var nombre in ingresoOctubreNombres)
        {
            var target = Norm(nombre);
            var m = miembros.FirstOrDefault(x => Norm(x.NombreCompleto).Contains(target));
            if (m != null)
            {
                if (m.FechaIngreso == null || m.FechaIngreso < new DateOnly(2025, 10, 1))
                {
                    m.FechaIngreso = new DateOnly(2025, 10, 1);
                    Console.WriteLine($"🗓️ Actualizada FechaIngreso a 2025-10-01 para {m.NombreCompleto}");
                }
            }
            else
            {
                Console.WriteLine($"⚠️ No se encontró miembro para actualizar ingreso (octubre): {nombre}");
            }
        }

        // Guardar cambios de fechas de ingreso, incluso si no hay recibos por crear
        await db.SaveChangesAsync();
        var recibos = new List<Recibo>();

        // === MIEMBROS QUE DEBEN DESDE ENERO HASTA OCTUBRE (10 meses) ===
        var deudoresEneroOctubre = new[]
        {
            "HECTOR MARIO GONZALEZ HENAO",
            "CESAR LEONEL RODRIGUEZ GALAN",
            "JHON JARVEY GÓMEZ PATIÑO",
            "JUAN ESTEBAN SUAREZ CORREA",
            "JOSÉ EDINSON OSPINA CRUZ",
            "JEFFERSON MONTOYA MUÑOZ",
            "ROBINSON ALEHANDRO GALVIS PARRA",
            "JHON ENMANUEL ARZUZA PÁEZ",
            "JUAN ESTEBAN OSORIO",
            "YEFERSON BAIRÓN USUGA AGUDELO",
            "JHON DAVID SANCHEZ",
            "CARLOS JULIO RENDÓN DÍAZ",
            "JENNIFER ANDREA CARDONA BENITEZ",
            "WILLIAM HUMBERTO JIMENEZ PEREZ",
            "CARLOS MARIO DIAZ DIAZ"
        };

        // === MIEMBROS QUE PAGARON ENERO A JUNIO (6 meses) ===
        var pagadoresEneroJunio = new[]
        {
            "CARLOS ANDRES PEREZ AREIZA",
            "DANIEL ANDREY VILLAMIZAR ARAQUE",
            "MILTON DARIO GOMEZ RIVERA"
        };

    // === MIEMBRO QUE PAGÓ SOLO ENERO (1 mes) ===
    var pagadorSoloEnero = "GIRLESA MARÍA BUITRAGO";

    // (se eliminó caso de 2 meses; se actualiza a 9 meses más abajo)

        // === MIEMBRO AL DÍA ENERO A OCTUBRE (10 meses) ===
        var alDia = "RAMÓN ANTONIO  GONZÁLEZ  CASTAÑO";

        // === MIEMBRO AL DÍA ENERO A DICIEMBRE (12 meses - año completo) ===
        var alDiaEnerodiciembre = "CARLOS ALBERTO ARAQUE BETANCUR";

        // === MIEMBROS QUE INGRESARON EN JUNIO (deben junio a octubre = 5 meses) ===
        var ingresadosJunio = new[]
        {
            "LAURA VIVIAN ASALAZAR MORENO",
            "JOSE JULIAN VILLAMIZAR ARAQUE"
        };

        // === MIEMBRO QUE INICIÓ EN OCTUBRE (debe 1 mes) ===
        // "GUSTAVO ADOLFO GÓMEZ ZULUAGA" - pendiente de implementar

        int consecutivo = 1;
        
        // Helper local para asegurar meses pagados de mensualidad (idempotente, suma por miembro)
        async Task AsegurarMensualidadAsync(AppDbContext db, Concepto conceptoMensualidad, Miembro miembro, int mesesDeseados,
            DateTime fechaEmision, string observaciones)
        {
            // Meses ya registrados para 2025
            var mesesActuales = await db.ReciboItems
                .Include(ri => ri.Recibo)
                .Where(ri => ri.ConceptoId == conceptoMensualidad.Id && ri.Recibo.MiembroId == miembro.Id && ri.Recibo.Ano == 2025)
                .SumAsync(ri => (int?)ri.Cantidad) ?? 0;

            var faltantes = mesesDeseados - mesesActuales;
            if (faltantes <= 0) return; // nada por hacer

            var recibo = new Recibo
            {
                Serie = "RC",
                Ano = 2025,
                Consecutivo = consecutivo++,
                FechaEmision = fechaEmision,
                MiembroId = miembro.Id,
                Estado = EstadoRecibo.Emitido,
                Observaciones = observaciones,
                Items = new List<ReciboItem>
                {
                    new ReciboItem
                    {
                        ConceptoId = conceptoMensualidad.Id,
                        Cantidad = faltantes,
                        MonedaOrigen = conceptoMensualidad.Moneda,
                        PrecioUnitarioMonedaOrigen = conceptoMensualidad.PrecioBase,
                        TrmAplicada = null
                    }
                }
            };
            recibo.TotalCop = faltantes * conceptoMensualidad.PrecioBase;
            db.Recibos.Add(recibo);
            await db.SaveChangesAsync();
            Console.WriteLine($"✅ {miembro.NombreCompleto}: mensualidad aumentada a {mesesDeseados} meses (se agregaron {faltantes}).");
        }
        
        // Registrar pagos de miembros que pagaron enero a junio
        foreach (var nombreCompleto in pagadoresEneroJunio)
        {
            var target = Norm(nombreCompleto);
            var miembro = miembros.FirstOrDefault(m => Norm(m.NombreCompleto).Contains(target) || target.Contains(Norm(m.NombreCompleto)));
            
            if (miembro != null)
            {
                // Evitar duplicar si ya tiene mensualidad 2025
                var yaTiene = await db.ReciboItems
                    .Include(ri => ri.Recibo)
                    .AnyAsync(ri => ri.ConceptoId == conceptoMensualidad.Id && ri.Recibo.MiembroId == miembro.Id && ri.Recibo.Ano == 2025);
                if (!yaTiene)
                    recibos.Add(CrearRecibo(ref consecutivo, miembro.Id, conceptoMensualidad, 6, 
                        new DateTime(2025, 6, 30), $"Pago mensualidad enero-junio 2025"));
                else
                    Console.WriteLine($"⏭️ {miembro.NombreCompleto} ya tiene mensualidad 2025, no se duplica.");
            }
        }

        // Registrar pago de miembro que pagó solo enero
        var miembroEnero = miembros.FirstOrDefault(m => Norm(m.NombreCompleto).Contains(Norm(pagadorSoloEnero)));
        if (miembroEnero != null)
        {
            var yaTiene = await db.ReciboItems.Include(ri => ri.Recibo)
                .AnyAsync(ri => ri.ConceptoId == conceptoMensualidad.Id && ri.Recibo.MiembroId == miembroEnero.Id && ri.Recibo.Ano == 2025);
            if (!yaTiene)
                recibos.Add(CrearRecibo(ref consecutivo, miembroEnero.Id, conceptoMensualidad, 1,
                    new DateTime(2025, 1, 31), "Pago mensualidad enero 2025"));
            else
                Console.WriteLine($"⏭️ {miembroEnero.NombreCompleto} ya tiene mensualidad 2025, no se duplica.");
            Console.WriteLine($"✅ Recibo creado para {miembroEnero.NombreCompleto} (solo enero)");
        }
        else
        {
            Console.WriteLine($"⚠️ No se encontró miembro: {pagadorSoloEnero}");
        }

        // Registrar pago de miembro al día (enero a octubre)
        var miembroAlDia = miembros.FirstOrDefault(m => Norm(m.NombreCompleto).Contains(Norm(alDia)));
        if (miembroAlDia != null)
        {
            var yaTiene = await db.ReciboItems.Include(ri => ri.Recibo)
                .AnyAsync(ri => ri.ConceptoId == conceptoMensualidad.Id && ri.Recibo.MiembroId == miembroAlDia.Id && ri.Recibo.Ano == 2025);
            if (!yaTiene)
                recibos.Add(CrearRecibo(ref consecutivo, miembroAlDia.Id, conceptoMensualidad, 10,
                    new DateTime(2025, 10, 15), "Pago mensualidad enero-octubre 2025"));
            else
                Console.WriteLine($"⏭️ {miembroAlDia.NombreCompleto} ya tiene mensualidad 2025, no se duplica.");
            Console.WriteLine($"✅ Recibo creado para {miembroAlDia.NombreCompleto} (enero-octubre)");
        }
        else
        {
            Console.WriteLine($"⚠️ No se encontró miembro: {alDia}");
        }

        // Registrar pago de miembro al día (enero a diciembre - año completo)
        var miembroAlDiaAnual = miembros.FirstOrDefault(m => Norm(m.NombreCompleto).Contains(Norm(alDiaEnerodiciembre)));
        if (miembroAlDiaAnual != null)
        {
            var yaTiene = await db.ReciboItems.Include(ri => ri.Recibo)
                .AnyAsync(ri => ri.ConceptoId == conceptoMensualidad.Id && ri.Recibo.MiembroId == miembroAlDiaAnual.Id && ri.Recibo.Ano == 2025);
            if (!yaTiene)
                recibos.Add(CrearRecibo(ref consecutivo, miembroAlDiaAnual.Id, conceptoMensualidad, 12,
                    new DateTime(2025, 12, 15), "Pago mensualidad enero-diciembre 2025"));
            else
                Console.WriteLine($"⏭️ {miembroAlDiaAnual.NombreCompleto} ya tiene mensualidad 2025, no se duplica.");
            Console.WriteLine($"✅ Recibo creado para {miembroAlDiaAnual.NombreCompleto} (enero-diciembre)");
        }
        else
        {
            Console.WriteLine($"⚠️ No se encontró miembro: {alDiaEnerodiciembre}");
        }

        // Asegurar pagos correctos para casos reportados recientemente
        // 1) ANGELA MARIA RODRIGUEZ: pago hasta septiembre 2025 (9 meses)
        var angelaMariaMens = miembros.FirstOrDefault(m => Norm(m.NombreCompleto).Contains(Norm("ANGELA MARIA RODRIGUEZ")))
                               ?? miembros.FirstOrDefault(m => m.Documento == "43703788");
        if (angelaMariaMens != null)
        {
            await AsegurarMensualidadAsync(db, conceptoMensualidad, angelaMariaMens, 9, new DateTime(2025, 9, 30),
                "Pago mensualidad enero-septiembre 2025");
        }
        else
        {
            Console.WriteLine("⚠️ No se encontró miembro Angela Maria Rodriguez para mensualidad");
        }

        // 2) CESAR LEONEL RODRIGUEZ GALAN: pago hasta septiembre 2025 (9 meses)
        var cesarLeonel = miembros.FirstOrDefault(m => Norm(m.NombreCompleto).Contains(Norm("CESAR LEONEL RODRIGUEZ GALAN")));
        if (cesarLeonel != null)
        {
            await AsegurarMensualidadAsync(db, conceptoMensualidad, cesarLeonel, 9, new DateTime(2025, 9, 30),
                "Pago mensualidad enero-septiembre 2025");
        }
        else
        {
            Console.WriteLine("⚠️ No se encontró miembro Cesar Leonel Rodriguez Galan para mensualidad");
        }

        // === CONCEPTOS ADICIONALES ===
        
        // ANGELA MARIA RODRIGUEZ OCHOA - Parches 337.158 COP
    // Buscar por documento para evitar problemas de tildes/acentos
    var angelaMaria = miembros.FirstOrDefault(m => m.Documento == "43703788");
        
        if (angelaMaria != null && conceptoParche != null)
        {
            // Crear recibo por parches (deuda)
            var reciboParches = new Recibo
            {
                Serie = "RC",
                Ano = 2025,
                Consecutivo = consecutivo++,
                FechaEmision = new DateTime(2025, 1, 15),
                MiembroId = angelaMaria.Id,
                Estado = EstadoRecibo.Emitido,
                Observaciones = "Compra de parches - DEUDA PENDIENTE",
                Items = new List<ReciboItem>
                {
                    new ReciboItem
                    {
                        ConceptoId = conceptoParche.Id,
                        Cantidad = 1,
                        MonedaOrigen = Moneda.COP,
                        PrecioUnitarioMonedaOrigen = 337158m,
                        TrmAplicada = null
                    }
                }
            };
            reciboParches.TotalCop = 337158m;
            recibos.Add(reciboParches);
            Console.WriteLine($"✅ Recibo de parches creado para {angelaMaria.NombreCompleto}");
            
            // Angela Maria también recibe auxilio de 80.000 COP por asistencia Damas LAMA
            if (conceptoAuxilio != null)
            {
                var reciboAuxilio = new Recibo
                {
                    Serie = "RC",
                    Ano = 2025,
                    Consecutivo = consecutivo++,
                    FechaEmision = new DateTime(2025, 1, 15),
                    MiembroId = angelaMaria.Id,
                    Estado = EstadoRecibo.Emitido,
                    Observaciones = "Auxilio por asistencia Damas LAMA",
                    Items = new List<ReciboItem>
                    {
                        new ReciboItem
                        {
                            ConceptoId = conceptoAuxilio.Id,
                            Cantidad = 1,
                            MonedaOrigen = Moneda.COP,
                            PrecioUnitarioMonedaOrigen = 80000m,
                            TrmAplicada = null
                        }
                    }
                };
                reciboAuxilio.TotalCop = 80000m;
                recibos.Add(reciboAuxilio);
                Console.WriteLine($"✅ Recibo de auxilio creado para {angelaMaria.NombreCompleto}");
            }
        }
        else
        {
            Console.WriteLine("⚠️ No se encontró miembro Angela Maria Rodriguez");
        }

        // JHON DAVID SANCHEZ - Parches 153.579 COP
        var jhonDavid = miembros.FirstOrDefault(m => 
            m.NombreCompleto.Contains("Jhon David") && 
            m.NombreCompleto.Contains("S"));
        
        if (jhonDavid != null && conceptoParche != null)
        {
            var reciboParches = new Recibo
            {
                Serie = "RC",
                Ano = 2025,
                Consecutivo = consecutivo++,
                FechaEmision = new DateTime(2025, 2, 10),
                MiembroId = jhonDavid.Id,
                Estado = EstadoRecibo.Emitido,
                Observaciones = "Compra de parches - DEUDA PENDIENTE",
                Items = new List<ReciboItem>
                {
                    new ReciboItem
                    {
                        ConceptoId = conceptoParche.Id,
                        Cantidad = 1,
                        MonedaOrigen = Moneda.COP,
                        PrecioUnitarioMonedaOrigen = 153579m,
                        TrmAplicada = null
                    }
                }
            };
            reciboParches.TotalCop = 153579m;
            recibos.Add(reciboParches);
            Console.WriteLine($"✅ Recibo de parches creado para {jhonDavid.NombreCompleto}");
        }
        else
        {
            Console.WriteLine("⚠️ No se encontró miembro Jhon David Sánchez");
        }

        // CARLOS MARIO DIAZ DIAZ - Renovación membresía (él y esposa) 92.000 COP
        var carlosMario = miembros.FirstOrDefault(m => 
            m.NombreCompleto.Contains("Carlos Mario") && 
            m.NombreCompleto.Contains("D") && 
            m.NombreCompleto.Contains("az"));
        
        if (carlosMario != null && conceptoRenovMem != null)
        {
            var reciboRenov = new Recibo
            {
                Serie = "RC",
                Ano = 2025,
                Consecutivo = consecutivo++,
                FechaEmision = new DateTime(2025, 1, 20),
                MiembroId = carlosMario.Id,
                Estado = EstadoRecibo.Emitido,
                Observaciones = "Renovación membresía (él y esposa) - DEUDA PENDIENTE",
                Items = new List<ReciboItem>
                {
                    new ReciboItem
                    {
                        ConceptoId = conceptoRenovMem.Id,
                        Cantidad = 2,
                        MonedaOrigen = Moneda.COP,
                        PrecioUnitarioMonedaOrigen = 46000m,
                        TrmAplicada = null
                    }
                }
            };
            reciboRenov.TotalCop = 92000m;
            recibos.Add(reciboRenov);
            Console.WriteLine($"✅ Recibo de renovación creado para {carlosMario.NombreCompleto}");
        }
        else
        {
            Console.WriteLine("⚠️ No se encontró miembro Carlos Mario Díaz");
        }

        // Miembro con novia Natalia Gomez - Renovación 46.000 COP
        // (necesitaría identificar el nombre del miembro - falta en la info)

        // WILLIAM JIMENEZ - Renovación membresía 46.000 COP
        var williamJimenez = miembros.FirstOrDefault(m => 
            m.NombreCompleto.Contains("William") && 
            m.NombreCompleto.Contains("Jim"));
        
        if (williamJimenez != null && conceptoRenovMem != null)
        {
            var reciboRenov = new Recibo
            {
                Serie = "RC",
                Ano = 2025,
                Consecutivo = consecutivo++,
                FechaEmision = new DateTime(2025, 1, 25),
                MiembroId = williamJimenez.Id,
                Estado = EstadoRecibo.Emitido,
                Observaciones = "Renovación membresía - DEUDA PENDIENTE",
                Items = new List<ReciboItem>
                {
                    new ReciboItem
                    {
                        ConceptoId = conceptoRenovMem.Id,
                        Cantidad = 1,
                        MonedaOrigen = Moneda.COP,
                        PrecioUnitarioMonedaOrigen = 46000m,
                        TrmAplicada = null
                    }
                }
            };
            reciboRenov.TotalCop = 46000m;
            recibos.Add(reciboRenov);
            Console.WriteLine($"✅ Recibo de renovación creado para {williamJimenez.NombreCompleto}");
        }
        else
        {
            Console.WriteLine("⚠️ No se encontró miembro William Jiménez");
        }

        // Anderson Arlex Betancur Rua - Membresía asociado 40 USD
        var andersonArlex = miembros.FirstOrDefault(m => m.NombreCompleto.ToUpper().Contains("ANDERSON ARLEX BETANCUR"));
        if (andersonArlex != null && conceptoRenovMem != null)
        {
            var reciboMembresia = new Recibo
            {
                Serie = "RC",
                Ano = 2025,
                Consecutivo = consecutivo++,
                FechaEmision = new DateTime(2025, 1, 30),
                MiembroId = andersonArlex.Id,
                Estado = EstadoRecibo.Emitido,
                Observaciones = "Membresía asociado - DEUDA PENDIENTE 40 USD",
                Items = new List<ReciboItem>
                {
                    new ReciboItem
                    {
                        ConceptoId = conceptoRenovMem.Id,
                        Cantidad = 1,
                        MonedaOrigen = Moneda.USD,
                        PrecioUnitarioMonedaOrigen = 40m,
                        TrmAplicada = trm
                    }
                }
            };
            reciboMembresia.TotalCop = 40m * trm;
            recibos.Add(reciboMembresia);
            Console.WriteLine($"✅ Recibo de membresía asociado creado para {andersonArlex.NombreCompleto}");
        }
        else
        {
            Console.WriteLine("⚠️ No se encontró miembro Anderson Arlex Betancur Rua");
        }

        if (recibos.Any())
        {
            db.Recibos.AddRange(recibos);
            await db.SaveChangesAsync();
            Console.WriteLine($"✅ Se crearon {recibos.Count} recibos iniciales para 2025");
        }
        else
        {
            Console.WriteLine("⚠️ No se crearon recibos (verificar nombres de miembros)");
        }
    }

    private static Recibo CrearRecibo(ref int consecutivo, Guid miembroId, Concepto concepto, 
        int meses, DateTime fecha, string observaciones)
    {
        var recibo = new Recibo
        {
            Serie = "RC",
            Ano = 2025,
            Consecutivo = consecutivo++,
            FechaEmision = fecha,
            MiembroId = miembroId,
            Estado = EstadoRecibo.Emitido,
            Observaciones = observaciones,
            Items = new List<ReciboItem>
            {
                new ReciboItem
                {
                    ConceptoId = concepto.Id,
                    Cantidad = meses,
                    MonedaOrigen = concepto.Moneda,
                    PrecioUnitarioMonedaOrigen = concepto.PrecioBase,
                    TrmAplicada = null
                }
            }
        };
        
        recibo.TotalCop = meses * concepto.PrecioBase;
        return recibo;
    }
}
