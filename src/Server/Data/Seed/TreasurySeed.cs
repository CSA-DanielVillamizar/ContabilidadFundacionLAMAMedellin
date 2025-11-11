using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data.Seed;

public static class TreasurySeed
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.Conceptos.AnyAsync())
        {
            var list = new List<Concepto>
            {
                new() { Codigo = "MENSUALIDAD", Nombre = "Mensualidad L.A.M.A. Medellín", Moneda = Moneda.COP, PrecioBase = 20000m, EsRecurrente = true, Periodicidad = Periodicidad.Mensual },
                new() { Codigo = "RENOV_ANUAL", Nombre = "Renovación Membresía", Moneda = Moneda.USD, PrecioBase = 20m, EsRecurrente = true, Periodicidad = Periodicidad.Anual },
                new() { Codigo = "INT_SOLICITUD", Nombre = "Membresía Internacional (Solicitud)", Moneda = Moneda.USD, PrecioBase = 48m, EsRecurrente = false, Periodicidad = Periodicidad.Unico },
                new() { Codigo = "INT_RENOVACION", Nombre = "Membresía Internacional (Renovación)", Moneda = Moneda.USD, PrecioBase = 20m, EsRecurrente = true, Periodicidad = Periodicidad.Anual },
                new() { Codigo = "INT_PARCHE", Nombre = "Membresía Internacional Parche", Moneda = Moneda.USD, PrecioBase = 20m },
                new() { Codigo = "P_PARCHE", Nombre = "\"P\" Parche", Moneda = Moneda.USD, PrecioBase = 20m },
                new() { Codigo = "ARCOS", Nombre = "Arcos", Moneda = Moneda.USD, PrecioBase = 25m },
                new() { Codigo = "ALAS", Nombre = "Alas", Moneda = Moneda.USD, PrecioBase = 25m },
                new() { Codigo = "PARCHE_PAIS", Nombre = "Parche País", Moneda = Moneda.USD, PrecioBase = 5m },
                new() { Codigo = "PARCHE_CAPITULO", Nombre = "Parche del Capítulo", Moneda = Moneda.USD, PrecioBase = 3m },
                new() { Codigo = "PARCHE_ESTADO", Nombre = "Parche Estado/Provincia", Moneda = Moneda.USD, PrecioBase = 3m },
                new() { Codigo = "LAMA_BANDERA", Nombre = "L.A.M.A. Bandera", Moneda = Moneda.USD, PrecioBase = 5m },
                new() { Codigo = "DAMA_LAMA", Nombre = "DAMA de L.A.M.A. Parche", Moneda = Moneda.USD, PrecioBase = 10m },
                new() { Codigo = "PARCHE_JUVENTUD", Nombre = "Parche de Juventud", Moneda = Moneda.USD, PrecioBase = 15m },
                new() { Codigo = "LAMA_PARCHE", Nombre = "L.A.M.A. Parche", Moneda = Moneda.USD, PrecioBase = 5m },
                new() { Codigo = "ALAS_PEQ", Nombre = "Alas Pequeñas", Moneda = Moneda.USD, PrecioBase = 5m },
                new() { Codigo = "MILLAS_PARCHE", Nombre = "Millas Parche", Moneda = Moneda.USD, PrecioBase = 5m },
                new() { Codigo = "RALLY_RECIENTE", Nombre = "Parche Rally Reciente", Moneda = Moneda.USD, PrecioBase = 6m },
                new() { Codigo = "RALLY_PASADO", Nombre = "Parche Rally Pasado", Moneda = Moneda.USD, PrecioBase = 3m },
                new() { Codigo = "PINS", Nombre = "Pins", Moneda = Moneda.USD, PrecioBase = 5m },
                new() { Codigo = "HEBILLAS", Nombre = "Hebillas", Moneda = Moneda.USD, PrecioBase = 25m },
                new() { Codigo = "CALCOMANIA", Nombre = "Calcomanía", Moneda = Moneda.USD, PrecioBase = 5m },
            };
            db.Conceptos.AddRange(list);
        }

        if (!await db.TasasCambio.AnyAsync())
        {
            db.TasasCambio.Add(new TasaCambio
            {
                Fecha = DateOnly.FromDateTime(DateTime.UtcNow),
                UsdCop = 4000m,
                Fuente = "Seed",
                ObtenidaAutomaticamente = false
            });
        }

        await db.SaveChangesAsync();
    }
}
